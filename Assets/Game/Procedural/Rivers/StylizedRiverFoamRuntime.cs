using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProgrammaticStylized3D.Geometry;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// Hidden Stage 6 runtime that owns the complete shared Foam network.
    /// Amount, Freshness, Integrity, and material phase are transported in one
    /// persistent state while a shared structural-resolution guidance field,
    /// GPU-only population controller, boundaries, Wake, and Impact activity organise
    /// that material into an evolving web-like tracer network.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(StylizedRiver))]
    [AddComponentMenu("")]
    public sealed class StylizedRiverFoamRuntime : MonoBehaviour
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
        private const float MaximumManualReservationSeconds = 90f;
        private const float DecayToFivePercent = 2.995732f;
        private const float TopologyMetricsUpdateRate = 2f;
        private const int ThreadGroupSize = 8;
        private const int TopologyMetricCount = 16;
        private const float TopologyMetricQuantisation = 1023f;
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
        private const float ProvisionalMaterialBreakup = 0.703747f;
        private const float ProvisionalMaterialSpread = 0.42858f;
        private const float ProvisionalMaterialCohesion = 0.056401f;
        private const float ProvisionalMaterialConnectivity = 0.228127f;
        private const float ProvisionalMaterialLifetime = 29.810668f;
        private const float ProvisionalMaterialFreshnessLifetime = 2.307567f;
        private const float ProvisionalMaterialIntegrityDamage = 0.703747f;
        private const float ProvisionalMaterialShoreRetention = 1.35f;
        private const float ProvisionalMaterialTargetCoverage = 0.203078f;
        private const float ProvisionalMaterialSupplyRate = 1.1152f;
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
            ClearFracture,
            BuildGuidance,
            RefreshInitialTopologySources,
            MeasureInitialPopulation,
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
            public RenderTexture Fracture;
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
        private static readonly ProfilerMarker InitClearFractureProfilerMarker =
            new ProfilerMarker("RiverFoam.Init.ClearFracture");
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
        private static readonly ProfilerMarker DiagnosticsMeasurePopulationProfilerMarker =
            new ProfilerMarker("RiverFoam.Diagnostics.MeasurePopulation");
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
        private static readonly int FoamGuidanceId =
            Shader.PropertyToID("_FoamGuidance");
        private static readonly int FoamTopologyId =
            Shader.PropertyToID("_FoamTopology");
        private static readonly int FoamTopologySourcesId =
            Shader.PropertyToID("_FoamTopologySources");
        private static readonly int FoamFractureId =
            Shader.PropertyToID("_FoamFracture");
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

        private readonly struct PendingInjection
        {
            public PendingInjection(
                float globalDistance,
                float acrossNormalized,
                float radius,
                float amount,
                float freshness,
                float integrity,
                float phase,
                float elongation,
                bool isManual,
                float shapeSeed = 0f,
                float shapeVariety = 0f,
                bool compoundShape = false)
            {
                GlobalDistance = globalDistance;
                AcrossNormalized = acrossNormalized;
                Radius = radius;
                Amount = amount;
                Freshness = freshness;
                Integrity = integrity;
                Phase = phase;
                Elongation = elongation;
                IsManual = isManual;
                ShapeSeed = shapeSeed;
                ShapeVariety = shapeVariety;
                CompoundShape = compoundShape;
            }

            public float GlobalDistance { get; }
            public float AcrossNormalized { get; }
            public float Radius { get; }
            public float Amount { get; }
            public float Freshness { get; }
            public float Integrity { get; }
            public float Phase { get; }
            public float Elongation { get; }
            public bool IsManual { get; }
            public float ShapeSeed { get; }
            public float ShapeVariety { get; }
            public bool CompoundShape { get; }
        }

        private sealed class FoamReservation
        {
            public float CentreGlobalDistance;
            public float AlongRadius;
            public float RemainingAmount;
            public float Elapsed;
            public float MaximumLifetime;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FoamMetricRow
        {
            public Vector4 WidthsAndSpacing;
            public Vector4 TopologyData;
            public Vector4 ShoreData;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FoamObstacleIntervalCellData
        {
            public Vector4 CoordinateAndOffset;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FoamMajorEvolutionData
        {
            public Vector4 CentreAndPlacement;
            public Vector4 CandidateShape;
            public Vector4 CandidateExtents;
            public Vector4 Morph;
            public Vector4 Warp;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FoamHostedNegativeEvolutionData
        {
            public Vector4 HostAndMask;
            public Vector4 CentreAndOffset;
            public Vector4 Morph;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FoamFreeWaterEvolutionData
        {
            public Vector4 CentreAndPlacement;
            public Vector4 MaskAndStrength;
            public Vector4 Morph;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FoamConnectorIdentityData
        {
            // x = first flattened path point, y = point count,
            // z = outer radius, w = core radius.
            public Vector4 PointRangeAndRadii;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FoamWeakSpanIdentityData
        {
            // x = Connector record index, y = normalized path distance,
            // z/w = gate-safe normalized interval.
            public Vector4 ConnectorAndPath;
            // x/y = physical along/across radii, z = pressure strength,
            // w = accepted identity orientation.
            public Vector4 Shape;
            // x = static irregular-boundary noise seed; remaining lanes reserved.
            public uint NoiseSeed;
            public uint Reserved0;
            public uint Reserved1;
            public uint Reserved2;
        }

        private enum ConnectorReleaseReason
        {
            None,
            Unavailable,
            Turnover,
            StretchBreak
        }

        private struct ConnectorRelationshipCandidate
        {
            public StylizedRiverFoamConnectorPath Path;
            public int StartHostSlotIndex;
            public int EndHostSlotIndex;
            public float BasePathLengthMetres;
            public float SelectionWeight;
        }

        private struct ConnectorEvolutionSlot
        {
            public uint StableId;
            public int OriginalCandidateIndex;
            public int AssignedCandidateIndex;
            public int ActiveCandidateIndex;
            public int LastReleasedCandidateIndex;
            public int ReleaseCooldownTicks;
            public int RelationshipRevision;
            public int PointOffset;
            public int PointCapacity;
            public int PointCount;
            public int ActiveStartAnchorIndex;
            public int ActiveEndAnchorIndex;
            public ConnectorReleaseReason PendingReleaseReason;
            public int TurnoverFallbackCandidateIndex;
            public float ReferenceLengthMetres;
            public int ReferenceCandidateIndex;
            public int ReferenceStartAnchorIndex;
            public int ReferenceEndAnchorIndex;
            public int ObservedStartRecycleCount;
            public int ObservedEndRecycleCount;
            public int StretchBlockedCandidateIndex;
            public int StretchBlockedStartRecycleCount;
            public int StretchBlockedEndRecycleCount;
            public bool IsActive;
            public bool HasRuntimeState;
        }

        private struct MajorEvolutionPose
        {
            public float LocalDistance;
            public float AcrossNormalized;
            public float OrientationRadians;
            public float MetresPerCandidateCell;
            public float ScaleAlong;
            public float ScaleAcross;
            public float Shear;
            public float WarpAlong;
            public float WarpAcross;
            public float WarpPhaseA;
            public float WarpPhaseB;
            public float SupportScale;
        }

        private struct MajorEvolutionSlot
        {
            public uint StableId;
            public int PreparedIndex;
            public float BaseMetresPerCandidateCell;
            public MajorEvolutionPose Current;
            public MajorEvolutionPose Start;
            public MajorEvolutionPose Target;
            public float DwellRemaining;
            public float LastDwellDuration;
            public float MoveElapsed;
            public float MoveDuration;
            public float OccurrenceElapsed;
            public float LifetimeUnitBudget;
            public float MaximumOccurrenceSeconds;
            public int HopIndex;
            public int RecycleCount;
            public int LastAnchorIndex;
            public bool IsMoving;
        }

        private struct HostedNegativeEvolutionPose
        {
            public Vector2 OffsetCells;
            public float RotationRadians;
            public float ScaleAlong;
            public float ScaleAcross;
            public float StrengthScale;
        }

        private struct HostedNegativeEvolutionSlot
        {
            public uint StableId;
            public int PreparedIndex;
            public int HostSlotIndex;
            public StylizedRiverFoamNegativeRegionClass RegionClass;
            public int CurrentVariantIndex;
            public int TargetVariantIndex;
            public HostedNegativeEvolutionPose Current;
            public HostedNegativeEvolutionPose Start;
            public HostedNegativeEvolutionPose Target;
        }

        private struct FreeWaterEvolutionPose
        {
            public float LocalDistance;
            public float AcrossNormalized;
            public float OrientationRadians;
            public float ScaleAlong;
            public float ScaleAcross;
            public float StrengthScale;
        }

        private struct FreeWaterEvolutionSlot
        {
            public uint StableId;
            public int PreparedIndex;
            public FreeWaterEvolutionPose Current;
            public FreeWaterEvolutionPose Start;
            public FreeWaterEvolutionPose Target;
            public float DwellRemaining;
            public float LastDwellDuration;
            public float MoveElapsed;
            public float MoveDuration;
            public float OccurrenceElapsed;
            public float LifetimeUnitBudget;
            public float MaximumOccurrenceSeconds;
            public int HopIndex;
            public int RecycleCount;
            public int LastAnchorIndex;
            public bool IsMoving;
        }

        private StylizedRiver river;
        private MeshRenderer surfaceRenderer;
        private MaterialPropertyBlock propertyBlock;
        private ComputeShader computeShader;
        private RenderTexture stateA;
        private RenderTexture stateB;
        private RenderTexture advectedState;
        private RenderTexture reverseState;
        private RenderTexture previousState;
        private RenderTexture currentState;
        private RenderTexture writeState;
        private RenderTexture guidanceTexture;
        private RenderTexture topologyTexture;
        private RenderTexture topologySourcesTexture;
        private RenderTexture topologyGeneratedTexture;
        private RenderTexture evolvingMajorTexture;
        private RenderTexture evolvingHostedNegativeTexture;
        private RenderTexture evolvingFreeWaterNegativeTexture;
        private RenderTexture evolvingConnectorTexture;
        private RenderTexture evolvingWeakSpanNegativeTexture;
        private RenderTexture currentShoreEdgesTexture;
        private RenderTexture obstacleExclusionTexture;
        private RenderTexture fractureA;
        private RenderTexture fractureB;
        private RenderTexture currentFracture;
        private RenderTexture writeFracture;
        private RenderTexture neutralDisturbanceTexture;
        private Texture2D boundaryTexture;
        private Texture2D obstacleExclusionReadbackTexture;
        private Texture2D obstacleExclusionUploadTexture;
        private Texture2D topologyGeneratedUploadTexture;
        private Texture2DArray majorMaskTextureArray;
        private Texture2DArray hostedNegativeMaskTextureArray;
        private Texture2DArray freeWaterNegativeMaskTextureArray;
        private ComputeBuffer metricBuffer;
        private ComputeBuffer obstacleExclusionCellBuffer;
        private ComputeBuffer obstacleExclusionSampleBuffer;
        private ComputeBuffer populationMetricsBuffer;
        private ComputeBuffer topologyMetricsBuffer;
        private ComputeBuffer majorEvolutionBuffer;
        private ComputeBuffer hostedNegativeEvolutionBuffer;
        private ComputeBuffer freeWaterEvolutionBuffer;
        private ComputeBuffer connectorIdentityBuffer;
        private ComputeBuffer connectorPathPointBuffer;
        private ComputeBuffer weakSpanIdentityBuffer;
        private StylizedRiverDisturbanceRuntime disturbanceRuntime;
        private FoamMetricRow[] metricRows = Array.Empty<FoamMetricRow>();
        private readonly uint[] latestTopologyMetrics =
            new uint[TopologyMetricCount];
        private bool topologyMetricsReadbackPending;
        private bool topologyMetricsAvailable;
        private int topologyMetricsGeneration;
        private StylizedRiverFoamMajorTopology majorTopology;
        private StylizedRiverFoamConnectorTopology connectorTopology;
        private StylizedRiverFoamPocketTopology pocketTopology;
        private int majorTopologyInputSignature = int.MinValue;
        private int connectorTopologyInputSignature = int.MinValue;
        private int pocketTopologyInputSignature = int.MinValue;
        private int majorEvolutionLifetimeInputSignature = int.MinValue;

        // Patch 4.9A keeps its proof telemetry entirely separate from the
        // active topology lifecycle. The explicit Inspector action serializes
        // immutable prepared data only and never replaces renderer bindings.
        private string topologyCacheRoundTripState = "Not Run";
        private string topologyCacheRoundTripSummary =
            "Run the explicit cache round-trip proof after initialization.";
        private int topologyCacheRoundTripPayloadBytes;
        private ulong topologyCacheRoundTripPayloadHash;
        private double topologyCacheRoundTripSerializationMilliseconds;
        private double topologyCacheRoundTripLoadMilliseconds;
        private double topologyCacheRoundTripVerificationMilliseconds;
        private int topologyCacheRoundTripRunCount;
        private int topologyCacheRoundTripPassCount;

        // Patch 4.9B keeps cache creation explicit. Patch 4.9C reuses the
        // stable-input diagnostics during staged startup, while renderer
        // ownership remains with the existing Foam runtime.
        private string topologyCacheBuildState = "Not Built";
        private string topologyCacheBuildSummary =
            "Build a cache explicitly after the prepared topology is Ready.";
        private int topologyCacheBuildCount;
        private int topologyCacheBuildSuccessCount;
        private int topologyCacheBuildPayloadBytes;
        private string topologyCacheBuildPayloadHash = "—";
        private double topologyCacheBuildMilliseconds;
        private string topologyCacheValidationState = "Not Evaluated";
        private string topologyCacheValidationSummary =
            "Assigned cache has not been checked against current stable inputs.";
        private int topologyCacheValidationCount;
        private int topologyCacheValidationHitCount;
        private string topologyCacheDomainFingerprint = "—";
        private string topologyCacheObstacleFingerprint = "—";
        private string topologyCacheGenerationFingerprint = "—";
        private string topologyCacheCombinedFingerprint = "—";
        private int topologyCacheObstacleSourceCount;

        // Patch 4.9C changes ordinary startup from generation-first to
        // cache-first. Patch 4.9C.1 keeps the strict production miss policy,
        // while Editor and Development builds automatically generate a missing
        // or stale topology and the Editor coordinator persists the completed,
        // round-trip-validated payload.
        private string topologyCacheStartupState = "Not Evaluated";
        private string topologyCacheStartupSummary =
            "Ordinary startup has not evaluated the assigned cache.";
        private int topologyCacheStartupAttemptCount;
        private int topologyCacheStartupHitCount;
        private int topologyCacheStartupMissCount;
        private int topologyCacheStartupPayloadBytes;
        private string topologyCacheStartupPayloadHash = "—";
        private double topologyCacheStartupLoadMilliseconds;
        private bool topologyCacheLoadedForActiveResources;
        private bool obstacleExclusionUsesCachedScalar;
        private bool activeTopologyObstacleStale;
        private bool explicitTopologyGenerationRequested;
        private bool explicitTopologyGenerationInProgress;
        private bool automaticTopologyGenerationInProgress;
        private double automaticDevelopmentRebuildNotBefore;
        private int automaticDevelopmentObservedSignature = int.MinValue;
        private AutomaticDevelopmentRebuildReason
            automaticDevelopmentRebuildReason;
        private bool pendingAutomaticDevelopmentRebuildAfterStartup;
        private bool pendingStartupCacheStaleObstacles;
        private bool pendingStartupCacheStaleSettings;
        private bool automaticTopologyCacheWritePending;
        private string automaticTopologyCachePersistenceState = "Idle";
        private string automaticTopologyCachePersistenceSummary =
            "No automatic development cache write is pending.";
        private int automaticTopologyCacheWriteCount;
        private int automaticTopologyCacheWriteSuccessCount;
        private string topologyCacheSessionPersistentInputKey = string.Empty;
        private bool topologyCacheSessionPersistentInputCaptured;
        private StylizedRiverFoamTopologyCachePackage
            pendingStartupCachePackage;

        // Patch 4.9D records decisive cold/warm startup evidence without
        // changing scheduling. Each AdvanceInitializationPhase call remains one
        // bounded staged step; the counters prove whether expensive preparation
        // executed and identify the slowest individual step.
        private bool topologyStartupValidationActive;
        private bool topologyStartupValidationComplete;
        private double topologyStartupValidationStartedAt;
        private double topologyStartupValidationTotalMilliseconds;
        private double topologyStartupValidationSlowestStepMilliseconds;
        private string topologyStartupValidationSlowestStep = "—";
        private int topologyStartupValidationStepCount;
        private int topologyStartupValidationCacheInstallCount;
        private int topologyStartupValidationObstacleBuildCount;
        private int topologyStartupValidationMajorBuildCount;
        private int topologyStartupValidationConnectorBuildCount;
        private int topologyStartupValidationPocketBuildCount;

        private readonly List<PendingInjection> pendingInjections = new();
        private readonly List<FoamReservation> reservations = new();
        private readonly List<MeshFilter> obstacleExclusionMeshFilters = new();
        private readonly List<MeshFilter> topologyCacheFingerprintMeshFilters =
            new();
        private readonly List<GeneratedGeometryStableFingerprint>
            topologyCachePreparedObstacleFingerprints = new();
        private readonly List<RiverObstacleExclusionCell>
            obstacleExclusionCells = new();
        private readonly List<RiverObstacleExclusionSample>
            obstacleExclusionSamples = new();
        private FoamObstacleIntervalCellData[] obstacleExclusionGpuCells =
            Array.Empty<FoamObstacleIntervalCellData>();
        private float[] obstacleExclusionScalar = Array.Empty<float>();
        private Color[] topologyGeneratedUploadPixels = Array.Empty<Color>();
        private Color[] majorMaskUploadPixels = Array.Empty<Color>();
        private Color[] hostedNegativeMaskUploadPixels = Array.Empty<Color>();
        private Color[] freeWaterNegativeMaskUploadPixels = Array.Empty<Color>();
        private MajorEvolutionSlot[] majorEvolutionSlots =
            Array.Empty<MajorEvolutionSlot>();
        private FoamMajorEvolutionData[] majorEvolutionGpuData =
            Array.Empty<FoamMajorEvolutionData>();
        private HostedNegativeEvolutionSlot[] hostedNegativeEvolutionSlots =
            Array.Empty<HostedNegativeEvolutionSlot>();
        private FoamHostedNegativeEvolutionData[]
            hostedNegativeEvolutionGpuData =
                Array.Empty<FoamHostedNegativeEvolutionData>();
        private FreeWaterEvolutionSlot[] freeWaterEvolutionSlots =
            Array.Empty<FreeWaterEvolutionSlot>();
        private FoamFreeWaterEvolutionData[] freeWaterEvolutionGpuData =
            Array.Empty<FoamFreeWaterEvolutionData>();
        private FoamConnectorIdentityData[] connectorIdentityGpuData =
            Array.Empty<FoamConnectorIdentityData>();
        private Vector4[] connectorPathPointGpuData = Array.Empty<Vector4>();
        private FoamWeakSpanIdentityData[] weakSpanIdentityGpuData =
            Array.Empty<FoamWeakSpanIdentityData>();
        private ConnectorEvolutionSlot[] connectorEvolutionSlots =
            Array.Empty<ConnectorEvolutionSlot>();
        private ConnectorRelationshipCandidate[] connectorRelationshipCandidates =
            Array.Empty<ConnectorRelationshipCandidate>();
        private bool[] connectorCandidateClaimed = Array.Empty<bool>();
        private bool[] connectorMajorPairClaimed = Array.Empty<bool>();
        private int[] connectorMajorDegree = Array.Empty<int>();
        private int[] connectorPreviousMajorDegree = Array.Empty<int>();
        private int connectorEvolutionActiveCount;
        private int connectorEvolutionTemporaryAbsenceCount;
        private int connectorEvolutionIdentityPathCount;
        private int connectorEvolutionRecycleVariantCount;
        private int connectorEvolutionOriginalRelationshipCount;
        private int connectorEvolutionReplacementRelationshipCount;
        private int connectorEvolutionRelationshipRebindCount;
        private int connectorEvolutionVariantSwitchCount;
        private int connectorEvolutionStretchBreakCount;
        private int connectorEvolutionRetainDecisionCount;
        private int connectorEvolutionTurnoverRequestCount;
        private int connectorEvolutionSuccessfulTurnoverCount;
        private int connectorEvolutionNoAlternativeFallbackCount;
        private int connectorEvolutionCrowdingBoostedTurnoverCount;
        private int connectorEvolutionMajorDegreeZeroCount;
        private int connectorEvolutionMajorDegreeOneCount;
        private int connectorEvolutionMajorDegreeTwoCount;
        private int connectorEvolutionMajorDegreeThreePlusCount;
        private int connectorEvolutionMaximumMajorDegree;
        private int connectorEvolutionAbsenceEventCount;
        private int connectorEvolutionReappearanceCount;
        private int weakSpanEvolutionActiveCount;
        private float majorEvolutionAccumulator;
        private float freeWaterEvolutionAccumulator;
        private bool majorEvolutionReady;
        private bool hostedNegativeEvolutionReady;
        private bool freeWaterEvolutionReady;
        private bool connectorIdentityReconstructionReady;
        private bool weakSpanIdentityReconstructionReady;
        private int majorEvolutionReconstructionTicks;
        private int hostedNegativeLocalChangeCount;
        private int freeWaterMoveCount;
        private int freeWaterRecycleCount;
        private int freeWaterUpstreamViolationCount;
        private float freeWaterObservedMinimumDwell = float.PositiveInfinity;
        private float freeWaterObservedMaximumDwell;
        private float freeWaterObservedMinimumMove = float.PositiveInfinity;
        private float freeWaterObservedMaximumMove;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private bool hostedNegativeInitialParityPending;
        private bool hostedNegativeInitialParityReadbackPending;
        private bool hostedNegativeInitialParityAvailable;
        private float hostedNegativeInitialParityMeanDifference;
        private float hostedNegativeInitialParityMaximumDifference;
        private int hostedNegativeInitialParityGeneration;
        private bool connectorIdentityParityPending;
        private bool connectorIdentityParityReadbackPending;
        private bool connectorIdentityParityAvailable;
        private float connectorIdentityParityMeanDifference;
        private float connectorIdentityParityMaximumDifference;
        private int connectorIdentityParityGeneration;
        private bool weakSpanIdentityParityPending;
        private bool weakSpanIdentityParityReadbackPending;
        private bool weakSpanIdentityParityAvailable;
        private float weakSpanIdentityParityMeanDifference;
        private float weakSpanIdentityParityMaximumDifference;
        private int weakSpanIdentityParityGeneration;
#endif
        private int majorEvolutionRecycleCount;
        private int majorEvolutionCrowdedRecycleFallbackCount;
        private int majorEvolutionUpstreamViolations;
        private float majorEvolutionObservedMinimumDwell = float.PositiveInfinity;
        private float majorEvolutionObservedMaximumDwell;
        private float majorEvolutionObservedMinimumMove = float.PositiveInfinity;
        private float majorEvolutionObservedMaximumMove;
        private double majorEvolutionLastCpuMilliseconds;
        private long majorEvolutionLastAllocatedBytes;

        private bool[] chunkActive = Array.Empty<bool>();
        private double[] chunkActiveUntil = Array.Empty<double>();
        private bool resourcesDirty = true;
        private bool boundaryDirty = true;
        private InitializationPhase initializationPhase =
            InitializationPhase.NotStarted;
        private RebuildPhase rebuildPhase = RebuildPhase.Idle;
        private bool pendingBoundaryRebuild;
        private bool pendingObstacleRebuild;
        private bool pendingTopologyRefresh;
        private bool pendingTopologyReplacementAfterMaintenance;
        private int pendingObstacleObservedVersion = int.MinValue;
        private int pendingObstacleStableFrameCount;
        private int initializationObstacleObservedVersion = int.MinValue;
        private int initializationObstacleStableFrameCount;
        private TopologyReplacementBuild topologyReplacementBuild;
        private TopologyReplacementPhase topologyReplacementPhase =
            TopologyReplacementPhase.Idle;
        private int topologyReplacementRequestSequence;
        private int topologyReplacementRequestCount;
        private int topologyReplacementCoalescedCount;
        private int topologyReplacementCancelledCount;
        private int topologyReplacementActivatedCount;
        private int topologyReplacementIdenticalPreparedCount;
        private string topologyReplacementLastReason = "None";
        private TopologyTransitionSnapshot topologyTransitionSnapshot;
        private RenderTexture retiredTopologyTransitionTexture;
        private ComputeBuffer retiredTopologyTransitionMetricBuffer;
        private int retiredTopologyTransitionReleaseFrame = -1;
        private RenderTexture retiredGeneratedTopologyTexture;
        private int retiredGeneratedTopologyReleaseFrame = -1;
        private float topologyTransitionElapsed;
        private int topologyTransitionStartedCount;
        private int topologyTransitionCompletedCount;
        private int topologyTransitionRemappedCount;
        private int topologyTransitionFlattenedCount;
        private bool supportWarningReported;
        private bool allocationWarningReported;
        private bool fullyFrozenLastUpdate;
        private int domainVersion = -1;
        private int fieldWidth;
        private int fieldHeight;
        private int chunkCount;
        private int resolutionPerChunk;
        private int guidanceWidth;
        private int guidanceHeight;
        private int fractureWidth;
        private int fractureHeight;
        private float fieldLength;
        private float validFieldLength;
        private float simulationFieldLength;
        private float allocatedGlobalStart;
        private float simulationAccumulator;
        private float guidanceAccumulator;
        private float topologyMetricsAccumulator;
        private float populationAccumulator;
        private float fractureAccumulator;
        private float simulationInterpolation = 1f;
        private float initializationMotionTime;
        private float lastRuntimeTime;
        private double idleSince;
        private int clearKernel = -1;
        private int injectKernel = -1;
        private int buildGuidanceKernel = -1;
        private int buildCurrentShoreEdgesKernel = -1;
        private int composeTopologyKernel = -1;
        private int captureGeneratedTopologyKernel = -1;
        private int buildEvolvingMajorSupportKernel = -1;
        private int clearObstacleExclusionKernel = -1;
        private int updateObstacleExclusionKernel = -1;
        private int resetTopologyMetricsKernel = -1;
        private int measureTopologyMetricsKernel = -1;
        private int resetPopulationKernel = -1;
        private int measurePopulationKernel = -1;
        private int updateFractureKernel = -1;
        private int clearFractureKernel = -1;
        private int advectForwardKernel = -1;
        private int advectReverseKernel = -1;
        private int simulateKernel = -1;
        private int applyBoundaryKernel = -1;
        private int obstacleGeometryVersion = -1;
        private StylizedRiverQuality allocatedQuality;

        private int lastUpdateDispatches;
        private int recentPeakDispatches;
        private long lastUpdateCellIterations;
        private long recentPeakCellIterations;
        private double recentPeakWindowEnd;
        private int injectedLastUpdate;
        private float lastInjectionBoundaryCoverage = -1f;
        private bool lastInjectionStateSynchronized;
        private int manualInjectionSequence;

        public int FieldWidth => currentState != null ? currentState.width : 0;
        public int FieldHeight => currentState != null ? currentState.height : 0;
        public int GuidanceWidth => guidanceTexture != null ? guidanceTexture.width : 0;
        public int GuidanceHeight => guidanceTexture != null ? guidanceTexture.height : 0;
        public int TopologyWidth => topologyTexture != null ? topologyTexture.width : 0;
        public int TopologyHeight => topologyTexture != null ? topologyTexture.height : 0;
        public bool MajorTopologyAvailable => majorTopology != null;
        public int MajorOpportunityCount => majorTopology != null
            ? majorTopology.AttemptedOpportunityCount
            : 0;
        public int MajorAcceptedRegionCount => majorTopology != null
            ? majorTopology.AcceptedRegionCount
            : 0;
        public int MajorRejectedRegionCount => majorTopology != null
            ? majorTopology.RejectedRegionCount
            : 0;
        public int MajorRegionMetadataCount => majorTopology != null
            ? majorTopology.Regions.Count
            : 0;
        public float MajorGeneratedCoverage => majorTopology != null
            ? majorTopology.Coverage
            : 0f;
        public double MajorGenerationMilliseconds => majorTopology != null
            ? majorTopology.GenerationMilliseconds
            : 0.0;
        public string MajorTopRejectionReasons => majorTopology != null
            ? majorTopology.GetTopRejectionSummary()
            : "None";
        public bool MajorEvolutionAvailable => majorEvolutionReady;
        public int MajorEvolutionSlotCount => majorEvolutionSlots.Length;
        public int MajorEvolutionMovingCount => CountMovingMajorSlots();
        public int MajorEvolutionDwellingCount =>
            Mathf.Max(0, majorEvolutionSlots.Length - CountMovingMajorSlots());
        public float MajorEvolutionMinimumDwell =>
            float.IsPositiveInfinity(majorEvolutionObservedMinimumDwell)
                ? 0f
                : majorEvolutionObservedMinimumDwell;
        public float MajorEvolutionMaximumDwell =>
            majorEvolutionObservedMaximumDwell;
        public float MajorEvolutionMinimumMove =>
            float.IsPositiveInfinity(majorEvolutionObservedMinimumMove)
                ? 0f
                : majorEvolutionObservedMinimumMove;
        public float MajorEvolutionMaximumMove =>
            majorEvolutionObservedMaximumMove;
        public int MajorEvolutionReconstructionTicks =>
            majorEvolutionReconstructionTicks;
        public int MajorEvolutionRecycleCount => majorEvolutionRecycleCount;
        public int MajorEvolutionCrowdedRecycleFallbackCount =>
            majorEvolutionCrowdedRecycleFallbackCount;
        public int MajorEvolutionUpstreamViolations =>
            majorEvolutionUpstreamViolations;
        public int MajorPreparedRecycleAnchorCount => majorTopology != null
            ? majorTopology.PreparedRecycleAnchorCount
            : 0;
        public int MajorRecycleFallbackCount => majorTopology != null
            ? majorTopology.RecycleFallbackCount
            : 0;
        public double MajorEvolutionLastCpuMilliseconds =>
            majorEvolutionLastCpuMilliseconds;
        public long MajorEvolutionLastAllocatedBytes =>
            majorEvolutionLastAllocatedBytes;
        public bool HostedNegativeEvolutionAvailable =>
            hostedNegativeEvolutionReady;
        public int HostedNegativeEvolutionSlotCount =>
            hostedNegativeEvolutionSlots.Length;
        public int HostedNegativeInteriorCount =>
            CountHostedNegativeSlots(
                StylizedRiverFoamNegativeRegionClass.InteriorPocket);
        public int HostedNegativeCavityCount =>
            CountHostedNegativeSlots(
                StylizedRiverFoamNegativeRegionClass.EdgeCavity);
        public int HostedNegativeLocalChangeCount =>
            hostedNegativeLocalChangeCount;
        public int HostedNegativeAcceptedCount => pocketTopology != null
            ? pocketTopology.AcceptedHostedRegionCount
            : 0;
        public int HostedNegativePreparedCount => pocketTopology != null
            ? pocketTopology.PreparedHostedRegionCount
            : 0;
        public int HostedNegativeFallbackCount => pocketTopology != null
            ? pocketTopology.HostedFallbackRegionCount
            : 0;
        public bool FreeWaterEvolutionAvailable => freeWaterEvolutionReady;
        public int FreeWaterEvolutionSlotCount =>
            freeWaterEvolutionSlots.Length;
        public int FreeWaterPreparedCount => pocketTopology != null
            ? pocketTopology.PreparedFreeWaterRegionCount
            : 0;
        public int FreeWaterPreparedRecycleAnchorCount =>
            pocketTopology != null
                ? pocketTopology.PreparedFreeWaterRecycleAnchorCount
                : 0;
        public int FreeWaterRecycleFallbackCount => pocketTopology != null
            ? pocketTopology.FreeWaterRecycleFallbackCount
            : 0;
        public int FreeWaterEvolutionMovingCount =>
            CountMovingFreeWaterSlots();
        public int FreeWaterEvolutionDwellingCount =>
            Mathf.Max(0, freeWaterEvolutionSlots.Length -
                CountMovingFreeWaterSlots());
        public float FreeWaterEvolutionMinimumDwell =>
            float.IsPositiveInfinity(freeWaterObservedMinimumDwell)
                ? 0f
                : freeWaterObservedMinimumDwell;
        public float FreeWaterEvolutionMaximumDwell =>
            freeWaterObservedMaximumDwell;
        public float FreeWaterEvolutionMinimumMove =>
            float.IsPositiveInfinity(freeWaterObservedMinimumMove)
                ? 0f
                : freeWaterObservedMinimumMove;
        public float FreeWaterEvolutionMaximumMove =>
            freeWaterObservedMaximumMove;
        public int FreeWaterMoveCount => freeWaterMoveCount;
        public int FreeWaterRecycleCount => freeWaterRecycleCount;
        public int FreeWaterUpstreamViolationCount =>
            freeWaterUpstreamViolationCount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public bool HostedNegativeInitialParityAvailable =>
            hostedNegativeInitialParityAvailable;
        public bool HostedNegativeInitialParityPending =>
            hostedNegativeInitialParityPending ||
            hostedNegativeInitialParityReadbackPending;
        public float HostedNegativeInitialParityMeanDifference =>
            hostedNegativeInitialParityMeanDifference;
        public float HostedNegativeInitialParityMaximumDifference =>
            hostedNegativeInitialParityMaximumDifference;
#endif
        public bool ConnectorIdentityReconstructionAvailable =>
            connectorIdentityReconstructionReady;
        public bool WeakSpanIdentityReconstructionAvailable =>
            weakSpanIdentityReconstructionReady;
        public bool ConnectorEvolutionAvailable =>
            majorEvolutionReady &&
            connectorIdentityReconstructionReady &&
            connectorEvolutionSlots.Length == connectorIdentityGpuData.Length &&
            connectorRelationshipCandidates.Length >=
                connectorEvolutionSlots.Length;
        public int ConnectorEvolutionActiveCount =>
            connectorEvolutionActiveCount;
        public int ConnectorEvolutionTemporaryAbsenceCount =>
            connectorEvolutionTemporaryAbsenceCount;
        public int ConnectorEvolutionIdentityPathCount =>
            connectorEvolutionIdentityPathCount;
        public int ConnectorEvolutionRecycleVariantCount =>
            connectorEvolutionRecycleVariantCount;
        public int ConnectorEvolutionOriginalRelationshipCount =>
            connectorEvolutionOriginalRelationshipCount;
        public int ConnectorEvolutionReplacementRelationshipCount =>
            connectorEvolutionReplacementRelationshipCount;
        public int ConnectorEvolutionRelationshipRebindCount =>
            connectorEvolutionRelationshipRebindCount;
        public int ConnectorEvolutionVariantSwitchCount =>
            connectorEvolutionVariantSwitchCount;
        public int ConnectorEvolutionStretchBreakCount =>
            connectorEvolutionStretchBreakCount;
        public int ConnectorEvolutionRetainDecisionCount =>
            connectorEvolutionRetainDecisionCount;
        public int ConnectorEvolutionTurnoverRequestCount =>
            connectorEvolutionTurnoverRequestCount;
        public int ConnectorEvolutionSuccessfulTurnoverCount =>
            connectorEvolutionSuccessfulTurnoverCount;
        public int ConnectorEvolutionNoAlternativeFallbackCount =>
            connectorEvolutionNoAlternativeFallbackCount;
        public int ConnectorEvolutionCrowdingBoostedTurnoverCount =>
            connectorEvolutionCrowdingBoostedTurnoverCount;
        public int ConnectorEvolutionMajorDegreeZeroCount =>
            connectorEvolutionMajorDegreeZeroCount;
        public int ConnectorEvolutionMajorDegreeOneCount =>
            connectorEvolutionMajorDegreeOneCount;
        public int ConnectorEvolutionMajorDegreeTwoCount =>
            connectorEvolutionMajorDegreeTwoCount;
        public int ConnectorEvolutionMajorDegreeThreePlusCount =>
            connectorEvolutionMajorDegreeThreePlusCount;
        public int ConnectorEvolutionMaximumMajorDegree =>
            connectorEvolutionMaximumMajorDegree;
        public int ConnectorEvolutionAbsenceEventCount =>
            connectorEvolutionAbsenceEventCount;
        public int ConnectorEvolutionReappearanceCount =>
            connectorEvolutionReappearanceCount;
        public int WeakSpanEvolutionActiveCount =>
            weakSpanEvolutionActiveCount;
        public int WeakSpanEvolutionTemporaryAbsenceCount => Mathf.Max(
            0,
            weakSpanIdentityGpuData.Length - weakSpanEvolutionActiveCount);
        public int ConnectorIdentityRecordCount => connectorIdentityGpuData.Length;
        public int ConnectorIdentityPathPointCount =>
            connectorPathPointGpuData.Length;
        public int WeakSpanIdentityRecordCount => weakSpanIdentityGpuData.Length;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public bool ConnectorIdentityParityAvailable =>
            connectorIdentityParityAvailable;
        public bool ConnectorIdentityParityPending =>
            connectorIdentityParityPending ||
            connectorIdentityParityReadbackPending;
        public float ConnectorIdentityParityMeanDifference =>
            connectorIdentityParityMeanDifference;
        public float ConnectorIdentityParityMaximumDifference =>
            connectorIdentityParityMaximumDifference;
        public bool WeakSpanIdentityParityAvailable =>
            weakSpanIdentityParityAvailable;
        public bool WeakSpanIdentityParityPending =>
            weakSpanIdentityParityPending ||
            weakSpanIdentityParityReadbackPending;
        public float WeakSpanIdentityParityMeanDifference =>
            weakSpanIdentityParityMeanDifference;
        public float WeakSpanIdentityParityMaximumDifference =>
            weakSpanIdentityParityMaximumDifference;
#endif
        public bool ConnectorTopologyAvailable => connectorTopology != null;
        public int ConnectorEligibleEndpointCount => connectorTopology != null
            ? connectorTopology.EligibleEndpointCount
            : 0;
        public int ConnectorPathAttemptCount => connectorTopology != null
            ? connectorTopology.PathAttemptCount
            : 0;
        public int ConnectorAcceptedCount => connectorTopology != null
            ? connectorTopology.AcceptedConnectorCount
            : 0;
        public int ConnectorPreparedCount => connectorTopology != null
            ? connectorTopology.PreparedConnectorCount
            : 0;
        public int ConnectorUnavailableCount => connectorTopology != null
            ? connectorTopology.UnavailableConnectorCount
            : 0;
        public int ConnectorPreparedEndpointCount => connectorTopology != null
            ? connectorTopology.PreparedEndpointCount
            : 0;
        public int ConnectorUnresolvedEndpointCount =>
            connectorTopology != null
                ? connectorTopology.UnresolvedEndpointCount
                : 0;
        public int ConnectorPreparedPathPointCount =>
            connectorTopology != null
                ? connectorTopology.PreparedPathPointCount
                : 0;
        public int ConnectorPreparedPathVariantCount =>
            connectorTopology != null
                ? connectorTopology.PreparedPathVariantCount
                : 0;
        public int ConnectorUnavailablePathVariantCount =>
            connectorTopology != null
                ? connectorTopology.UnavailablePathVariantCount
                : 0;
        public int ConnectorPreparedRelationshipCatalogueCount =>
            connectorTopology != null
                ? connectorTopology.PreparedRelationshipCatalogueCount
                : 0;
        public int ConnectorPreparedReplacementRelationshipCount =>
            connectorTopology != null
                ? connectorTopology.PreparedReplacementRelationshipCount
                : 0;
        public int ConnectorPreparedRelationshipCataloguePathPointCount =>
            connectorTopology != null
                ? connectorTopology.PreparedRelationshipCataloguePathPointCount
                : 0;
        public int ConnectorPreparedReplacementPathVariantCount =>
            connectorTopology != null
                ? connectorTopology.PreparedReplacementPathVariantCount
                : 0;
        public int ConnectorUnavailableReplacementPathVariantCount =>
            connectorTopology != null
                ? connectorTopology.UnavailableReplacementPathVariantCount
                : 0;
        public string ConnectorTopRejectionReason => connectorTopology != null
            ? connectorTopology.GetTopRejectionSummary()
            : "None";
        public double ConnectorGenerationMilliseconds =>
            connectorTopology != null
                ? connectorTopology.GenerationMilliseconds
                : 0.0;
        public bool PocketTopologyAvailable => pocketTopology != null;
        public int PocketEligibleHostCount => pocketTopology != null
            ? pocketTopology.EligibleHostCount
            : 0;
        public int PocketCandidateCentreCount => pocketTopology != null
            ? pocketTopology.CandidateCentreCount
            : 0;
        public int PocketAcceptedCount => pocketTopology != null
            ? pocketTopology.AcceptedPocketCount
            : 0;
        public int InteriorPocketEligibleHostCount => pocketTopology != null
            ? pocketTopology.InteriorEligibleHostCount
            : 0;
        public int InteriorPocketCandidateCount => pocketTopology != null
            ? pocketTopology.InteriorCandidateCount
            : 0;
        public int InteriorPocketAcceptedCount => pocketTopology != null
            ? pocketTopology.AcceptedInteriorPocketCount
            : 0;
        public int EdgeCavityEligibleHostCount => pocketTopology != null
            ? pocketTopology.CavityEligibleHostCount
            : 0;
        public int EdgeCavityCandidateCount => pocketTopology != null
            ? pocketTopology.CavityCandidateCount
            : 0;
        public int EdgeCavityAcceptedCount => pocketTopology != null
            ? pocketTopology.AcceptedEdgeCavityCount
            : 0;
        public int ConnectorWeakSpanEligibleConnectorCount =>
            pocketTopology != null
                ? pocketTopology.WeakSpanEligibleConnectorCount
                : 0;
        public int ConnectorWeakSpanCandidateCount => pocketTopology != null
            ? pocketTopology.WeakSpanCandidateCount
            : 0;
        public int ConnectorWeakSpanAcceptedCount => pocketTopology != null
            ? pocketTopology.AcceptedConnectorWeakSpanCount
            : 0;
        public int ConnectorWeakSpanPreparedCount => pocketTopology != null
            ? pocketTopology.PreparedWeakSpanRegionCount
            : 0;
        public int ConnectorWeakSpanUnavailableCount => pocketTopology != null
            ? pocketTopology.UnavailableWeakSpanRegionCount
            : 0;
        public int FreeWaterEventOpportunityCount => pocketTopology != null
            ? pocketTopology.FreeWaterOpportunityCount
            : 0;
        public int FreeWaterEventCandidateCount => pocketTopology != null
            ? pocketTopology.FreeWaterCandidateCount
            : 0;
        public int FreeWaterEventAcceptedCount => pocketTopology != null
            ? pocketTopology.AcceptedFreeWaterEventCount
            : 0;
        public string PocketTopRejectionReason => pocketTopology != null
            ? pocketTopology.GetTopRejectionSummary()
            : "None";
        public double PocketGenerationMilliseconds => pocketTopology != null
            ? pocketTopology.GenerationMilliseconds
            : 0.0;
        public bool TopologyReplacementInProgress =>
            topologyReplacementPhase != TopologyReplacementPhase.Idle;
        public bool TopologyReplacementReady =>
            topologyReplacementPhase ==
                TopologyReplacementPhase.ReadyToActivate;
        public string TopologyReplacementState =>
            topologyReplacementPhase.ToString();
        public string TopologyReplacementLastReason =>
            topologyReplacementLastReason;
        public int TopologyReplacementRequestCount =>
            topologyReplacementRequestCount;
        public int TopologyReplacementCoalescedCount =>
            topologyReplacementCoalescedCount;
        public int TopologyReplacementCancelledCount =>
            topologyReplacementCancelledCount;
        public int TopologyReplacementActivatedCount =>
            topologyReplacementActivatedCount;
        public int TopologyReplacementIdenticalPreparedCount =>
            topologyReplacementIdenticalPreparedCount;
        public bool TopologyTransitionActive =>
            topologyTransitionSnapshot != null;
        public string TopologyTransitionState =>
            topologyTransitionSnapshot == null
                ? "Idle"
                : topologyTransitionSnapshot.HoldsVisibleResources
                    ? "Holding Previous Mapping"
                    : "Crossfading";
        public float TopologyTransitionProgress =>
            topologyTransitionSnapshot == null ||
            topologyTransitionSnapshot.HoldsVisibleResources
                ? 0f
                : Mathf.Clamp01(
                    topologyTransitionElapsed /
                    TopologyReplacementTransitionSeconds);
        public float TopologyTransitionDuration =>
            TopologyReplacementTransitionSeconds;
        public int TopologyTransitionStartedCount =>
            topologyTransitionStartedCount;
        public int TopologyTransitionCompletedCount =>
            topologyTransitionCompletedCount;
        public int TopologyTransitionRemappedCount =>
            topologyTransitionRemappedCount;
        public int TopologyTransitionFlattenedCount =>
            topologyTransitionFlattenedCount;
        public string TopologyCacheRoundTripState =>
            topologyCacheRoundTripState;
        public string TopologyCacheRoundTripSummary =>
            topologyCacheRoundTripSummary;
        public int TopologyCacheRoundTripPayloadBytes =>
            topologyCacheRoundTripPayloadBytes;
        public string TopologyCacheRoundTripPayloadHash =>
            topologyCacheRoundTripPayloadHash > 0ul
                ? topologyCacheRoundTripPayloadHash.ToString("X16")
                : "—";
        public double TopologyCacheRoundTripSerializationMilliseconds =>
            topologyCacheRoundTripSerializationMilliseconds;
        public double TopologyCacheRoundTripLoadMilliseconds =>
            topologyCacheRoundTripLoadMilliseconds;
        public double TopologyCacheRoundTripVerificationMilliseconds =>
            topologyCacheRoundTripVerificationMilliseconds;
        public int TopologyCacheRoundTripRunCount =>
            topologyCacheRoundTripRunCount;
        public int TopologyCacheRoundTripPassCount =>
            topologyCacheRoundTripPassCount;
        public bool TopologyCacheRoundTripReady =>
            initializationPhase == InitializationPhase.Ready &&
            AreResourcesCompleteAndCurrent() &&
            majorTopology != null &&
            connectorTopology != null &&
            pocketTopology != null &&
            obstacleExclusionScalar.Length == fieldWidth * fieldHeight;
        public bool TopologyCacheBuildReady =>
            AreTopologyCacheInputsCurrent();
        public int TopologyCacheFormatVersion =>
            StylizedRiverFoamTopologyCacheCodec.FormatVersion;
        public int TopologyCacheGeneratorContractVersion =>
            StylizedRiverFoamTopologyCacheCodec.GeneratorContractVersion;
        public string TopologyCacheBuildState => topologyCacheBuildState;
        public string TopologyCacheBuildSummary => topologyCacheBuildSummary;
        public int TopologyCacheBuildCount => topologyCacheBuildCount;
        public int TopologyCacheBuildSuccessCount =>
            topologyCacheBuildSuccessCount;
        public int TopologyCacheBuildPayloadBytes =>
            topologyCacheBuildPayloadBytes;
        public string TopologyCacheBuildPayloadHash =>
            topologyCacheBuildPayloadHash;
        public double TopologyCacheBuildMilliseconds =>
            topologyCacheBuildMilliseconds;
        public string TopologyCacheValidationState =>
            topologyCacheValidationState;
        public string TopologyCacheValidationSummary =>
            topologyCacheValidationSummary;
        public int TopologyCacheValidationCount =>
            topologyCacheValidationCount;
        public int TopologyCacheValidationHitCount =>
            topologyCacheValidationHitCount;
        public string TopologyCacheDomainFingerprint =>
            topologyCacheDomainFingerprint;
        public string TopologyCacheObstacleFingerprint =>
            topologyCacheObstacleFingerprint;
        public string TopologyCacheGenerationFingerprint =>
            topologyCacheGenerationFingerprint;
        public string TopologyCacheCombinedFingerprint =>
            topologyCacheCombinedFingerprint;
        public int TopologyCacheObstacleSourceCount =>
            topologyCacheObstacleSourceCount;
        public bool TopologyCacheObstacleRegistryReady
        {
            get
            {
                disturbanceRuntime ??=
                    GetComponent<StylizedRiverDisturbanceRuntime>();
                return disturbanceRuntime == null ||
                    disturbanceRuntime.GeneratedObstacleRegistryReady;
            }
        }
        public int TopologyCacheObstacleRegistryProcessedCount
        {
            get
            {
                disturbanceRuntime ??=
                    GetComponent<StylizedRiverDisturbanceRuntime>();
                return disturbanceRuntime != null
                    ? disturbanceRuntime
                        .GeneratedObstacleRegistryProcessedCount
                    : 0;
            }
        }
        public int TopologyCacheObstacleRegistryTotalCount
        {
            get
            {
                disturbanceRuntime ??=
                    GetComponent<StylizedRiverDisturbanceRuntime>();
                return disturbanceRuntime != null
                    ? disturbanceRuntime.GeneratedObstacleRegistryTotalCount
                    : 0;
            }
        }
        public string TopologyCacheStartupState => topologyCacheStartupState;
        public string TopologyCacheStartupSummary =>
            topologyCacheStartupSummary;
        public int TopologyCacheStartupAttemptCount =>
            topologyCacheStartupAttemptCount;
        public int TopologyCacheStartupHitCount =>
            topologyCacheStartupHitCount;
        public int TopologyCacheStartupMissCount =>
            topologyCacheStartupMissCount;
        public int TopologyCacheStartupPayloadBytes =>
            topologyCacheStartupPayloadBytes;
        public string TopologyCacheStartupPayloadHash =>
            topologyCacheStartupPayloadHash;
        public double TopologyCacheStartupLoadMilliseconds =>
            topologyCacheStartupLoadMilliseconds;
        public bool TopologyCacheLoadedForActiveResources =>
            topologyCacheLoadedForActiveResources;
        public bool AutomaticDevelopmentCacheEnabled =>
            IsAutomaticDevelopmentCacheEnabled;
        public bool AutomaticTopologyCacheWritePending =>
            automaticTopologyCacheWritePending;
        public string AutomaticTopologyCachePersistenceState =>
            automaticTopologyCachePersistenceState;
        public string AutomaticTopologyCachePersistenceSummary =>
            automaticTopologyCachePersistenceSummary;
        public int AutomaticTopologyCacheWriteCount =>
            automaticTopologyCacheWriteCount;
        public int AutomaticTopologyCacheWriteSuccessCount =>
            automaticTopologyCacheWriteSuccessCount;
        public bool TopologyStartupValidationComplete =>
            topologyStartupValidationComplete;
        public double TopologyStartupTotalMilliseconds =>
            topologyStartupValidationTotalMilliseconds;
        public double TopologyStartupSlowestStepMilliseconds =>
            topologyStartupValidationSlowestStepMilliseconds;
        public string TopologyStartupSlowestStep =>
            topologyStartupValidationSlowestStep;
        public int TopologyStartupStepCount =>
            topologyStartupValidationStepCount;
        public int TopologyStartupCacheInstallCount =>
            topologyStartupValidationCacheInstallCount;
        public int TopologyStartupObstacleBuildCount =>
            topologyStartupValidationObstacleBuildCount;
        public int TopologyStartupMajorBuildCount =>
            topologyStartupValidationMajorBuildCount;
        public int TopologyStartupConnectorBuildCount =>
            topologyStartupValidationConnectorBuildCount;
        public int TopologyStartupPocketBuildCount =>
            topologyStartupValidationPocketBuildCount;
        public int TopologyStartupExpensivePreparationCount =>
            topologyStartupValidationObstacleBuildCount +
            topologyStartupValidationMajorBuildCount +
            topologyStartupValidationConnectorBuildCount +
            topologyStartupValidationPocketBuildCount;
        public bool ExplicitTopologyGenerationAvailable =>
            Application.isPlaying &&
            !DevelopmentTopologyGenerationInProgress &&
            (initializationPhase ==
                InitializationPhase.AwaitExplicitTopologyGeneration ||
             (initializationPhase == InitializationPhase.Ready &&
              (ResolveRequestedTopologySignature() !=
                   ResolveActiveTopologySignature() ||
               ResolveCurrentObstacleGeometryVersion() !=
                   obstacleGeometryVersion)));
        public int DynamicShoreRowCount => currentShoreEdgesTexture != null
            ? currentShoreEdgesTexture.width
            : 0;
        public bool TopologyMetricsAvailable => topologyMetricsAvailable;
        public float MajorSupportCoverage => TopologyCoverageRatio(1);
        public float ConnectorSupportCoverage => TopologyCoverageRatio(2);
        public float PocketPressureCoverage => TopologyCoverageRatio(3);
        public float FoamWithinPocketPressure => TopologyRegionRatio(4, 3);
        public float VisibleMaterialCoverage => TopologyCoverageRatio(5);
        public float MaterialDeficitInsideNetSupport =>
            topologyMetricsAvailable && latestTopologyMetrics[6] > 0u
                ? latestTopologyMetrics[7] /
                  (TopologyMetricQuantisation * latestTopologyMetrics[6])
                : 0f;
        public float FoamWithinShoreSupport => TopologyRegionRatio(9, 8);
        public float FoamWithinPressureLeeSupport => TopologyRegionRatio(13, 12);
        public float PerimeterRatio => TopologyRegionRatio(14, 5);
        public float LegacyNetSupportCoverage => TopologyCoverageRatio(6);
        public float OpenSpanCoverage => topologyMetricsAvailable
            ? Mathf.Clamp01(1f - LegacyNetSupportCoverage)
            : 0f;
        public float ConnectorMajorOverlap => TopologyRegionRatio(15, 2);
        public int FractureWidth => currentFracture != null ? currentFracture.width : 0;
        public int FractureHeight => currentFracture != null ? currentFracture.height : 0;
        public float GuidanceUpdateRate => ResolveGuidanceUpdateRate();
        public float PopulationUpdateRate => ResolvePopulationUpdateRate();
        public float FractureUpdateRate => ResolveFractureUpdateRate();
        public int ActiveChunkCount => CountActiveChunks();
        public int PendingInjectionCount => pendingInjections.Count;
        public int ActiveReservationCount => reservations.Count;
        public int InjectedLastUpdate => injectedLastUpdate;
        public float LastInjectionBoundaryCoverage => lastInjectionBoundaryCoverage;
        public bool LastInjectionStateSynchronized =>
            lastInjectionStateSynchronized;
        public int LastUpdateDispatches => lastUpdateDispatches;
        public int RecentPeakDispatches => recentPeakDispatches;
        public long LastUpdateCellIterations => lastUpdateCellIterations;
        public long RecentPeakCellIterations => recentPeakCellIterations;
        public float UpdateRate => ResolveUpdateRate();
        public bool InitializationComplete =>
            initializationPhase == InitializationPhase.Ready;
        public bool ResourcesAllocated =>
            InitializationComplete && currentState != null;
        public bool CorrectedAdvectionActive =>
            InitializationComplete &&
            currentState != null &&
            advectedState != null &&
            reverseState != null;
        public bool IsSleeping =>
            !TopologyReplacementInProgress &&
            !TopologyTransitionActive &&
            !IsAutomaticMaterialSupplyActive &&
            !IsTopologyDebugActive &&
            pendingInjections.Count == 0 &&
            reservations.Count == 0 &&
            CountActiveChunks() == 0;
        public long EstimatedMemoryBytes =>
            EstimateTextureBytes(stateA) +
            EstimateTextureBytes(stateB) +
            EstimateTextureBytes(advectedState) +
            EstimateTextureBytes(reverseState) +
            EstimateTextureBytes(guidanceTexture) +
            EstimateTextureBytes(topologyTexture) +
            EstimateTextureBytes(topologySourcesTexture) +
            EstimateTextureBytes(topologyGeneratedTexture) +
            EstimateTextureBytes(
                topologyReplacementBuild != null
                    ? topologyReplacementBuild.GeneratedTexture
                    : null) +
            EstimateTopologyTransitionBytes() +
            EstimateTextureBytes(retiredTopologyTransitionTexture) +
            EstimateTextureBytes(retiredGeneratedTopologyTexture) +
            EstimateTextureBytes(evolvingMajorTexture) +
            EstimateTextureBytes(evolvingHostedNegativeTexture) +
            EstimateTextureBytes(evolvingFreeWaterNegativeTexture) +
            EstimateTextureBytes(evolvingConnectorTexture) +
            EstimateTextureBytes(evolvingWeakSpanNegativeTexture) +
            EstimateTextureBytes(topologyGeneratedUploadTexture) +
            EstimateTextureBytes(majorMaskTextureArray) +
            EstimateTextureBytes(hostedNegativeMaskTextureArray) +
            EstimateTextureBytes(freeWaterNegativeMaskTextureArray) +
            EstimateTextureBytes(currentShoreEdgesTexture) +
            EstimateTextureBytes(obstacleExclusionTexture) +
            EstimateTextureBytes(fractureA) +
            EstimateTextureBytes(fractureB) +
            EstimateTextureBytes(neutralDisturbanceTexture) +
            EstimateTextureBytes(boundaryTexture) +
            EstimateTextureBytes(obstacleExclusionReadbackTexture) +
            EstimateTextureBytes(obstacleExclusionUploadTexture) +
            (obstacleExclusionCellBuffer != null
                ? (long)obstacleExclusionCellBuffer.count *
                    obstacleExclusionCellBuffer.stride
                : 0L) +
            (obstacleExclusionSampleBuffer != null
                ? (long)obstacleExclusionSampleBuffer.count *
                    obstacleExclusionSampleBuffer.stride
                : 0L) +
            (metricBuffer != null
                ? (long)metricBuffer.count * metricBuffer.stride
                : 0L) +
            (populationMetricsBuffer != null
                ? (long)populationMetricsBuffer.count * populationMetricsBuffer.stride
                : 0L) +
            (topologyMetricsBuffer != null
                ? (long)topologyMetricsBuffer.count * topologyMetricsBuffer.stride
                : 0L) +
            (majorEvolutionBuffer != null
                ? (long)majorEvolutionBuffer.count * majorEvolutionBuffer.stride
                : 0L) +
            (hostedNegativeEvolutionBuffer != null
                ? (long)hostedNegativeEvolutionBuffer.count *
                    hostedNegativeEvolutionBuffer.stride
                : 0L) +
            (freeWaterEvolutionBuffer != null
                ? (long)freeWaterEvolutionBuffer.count *
                    freeWaterEvolutionBuffer.stride
                : 0L) +
            (connectorIdentityBuffer != null
                ? (long)connectorIdentityBuffer.count *
                    connectorIdentityBuffer.stride
                : 0L) +
            (connectorPathPointBuffer != null
                ? (long)connectorPathPointBuffer.count *
                    connectorPathPointBuffer.stride
                : 0L) +
            (weakSpanIdentityBuffer != null
                ? (long)weakSpanIdentityBuffer.count *
                    weakSpanIdentityBuffer.stride
                : 0L);

        private static bool IsAutomaticDevelopmentCacheEnabled
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return true;
#else
                return false;
#endif
            }
        }

        private bool DevelopmentTopologyGenerationInProgress =>
            explicitTopologyGenerationInProgress ||
            automaticTopologyGenerationInProgress;

        private bool IsAutomaticMaterialSupplyActive =>
            river != null && river.FoamEnabled;

        private bool IsTopologyDebugActive
        {
            get
            {
                if (river == null)
                {
                    return false;
                }

                StylizedRiverFoamDebugView view = river.FoamDebugView;
                return view == StylizedRiverFoamDebugView.AnchoredSupport ||
                    view == StylizedRiverFoamDebugView.SupportAndNegativeInfluence ||
                    view == StylizedRiverFoamDebugView.SupportClasses ||
                    view == StylizedRiverFoamDebugView.NegativeInfluenceClasses;
            }
        }

        private bool IsSupported =>
            SystemInfo.supportsComputeShaders &&
            SystemInfo.supports2DArrayTextures &&
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf) &&
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf) &&
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RHalf) &&
            SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf);

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
        }

        private void OnDestroy()
        {
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

            bool automaticMaterialSupplyActive =
                IsAutomaticMaterialSupplyActive;
            // TODO: Audit topology/debug gating so Final Foam does not force
            // diagnostic-grade topology composition or metric refreshes.
            bool topologyDebugActive = IsTopologyDebugActive;
            bool materialWork =
                automaticMaterialSupplyActive ||
                pendingInjections.Count > 0 ||
                reservations.Count > 0 ||
                CountActiveChunks() > 0;
            bool hasWork = materialWork || topologyDebugActive;

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
                // Manual diagnostics remain immediately visible, but they now
                // enter the same autonomous guidance, population, boundary,
                // Wake, Impact, merging, and structural-failure solver on the
                // following simulation step.
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
                    bool materialStepActive =
                        automaticMaterialSupplyActive ||
                        reservations.Count > 0 ||
                        CountActiveChunks() > 0;

                    if (materialStepActive)
                    {
                        UpdateReservations(stepDuration, now);

                        if (automaticMaterialSupplyActive)
                        {
                            ActivateAllChunks(now + stepDuration * 3f);
                        }
                        else
                        {
                            UpdateActiveChunks(now);
                        }

                        ConfigureSharedComputeParameters(stepDuration);
                        populationAccumulator += stepDuration;
                        fractureAccumulator += stepDuration;
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
                        float populationInterval = 1f /
                            Mathf.Max(1f, ResolvePopulationUpdateRate());
                        if (populationAccumulator >= populationInterval)
                        {
                            MeasurePopulation();
                            populationAccumulator %= populationInterval;
                        }

                        float fractureInterval = 1f /
                            Mathf.Max(1f, ResolveFractureUpdateRate());
                        if (fractureAccumulator >= fractureInterval)
                        {
                            UpdateFractureField(fractureAccumulator);
                            fractureAccumulator %= fractureInterval;
                        }

                        SimulateActiveChunks(stepDuration);
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
            float resolvedFreshness = Mathf.Clamp01(freshness);
            float shapeSeed = river.VisualSeed + injectionIndex * 17.371f;
            float phase = Mathf.Repeat(
                river.VisualSeed * 0.000173f + injectionIndex * 0.6180339f,
                1f);
            float integrity = Mathf.Clamp01(
                Mathf.Lerp(0.78f, 1f, resolvedFreshness));

            pendingInjections.Add(
                new PendingInjection(
                    globalDistance,
                    Mathf.Clamp(acrossNormalized, -1f, 1f),
                    Mathf.Clamp(radius, 0.05f, 8f),
                    resolvedAmount,
                    resolvedFreshness,
                    integrity,
                    phase,
                    Mathf.Clamp(elongation, 0.25f, 8f),
                    true,
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
            ResetManualInjectionSequence();
            lastInjectionBoundaryCoverage = -1f;
            lastInjectionStateSynchronized = false;
            simulationAccumulator = 0f;
            guidanceAccumulator = 0f;
            topologyMetricsAccumulator = 0f;
            populationAccumulator = 0f;
            fractureAccumulator = 0f;
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

            DispatchClearFracture(fractureA, 0, fractureWidth);
            DispatchClearFracture(fractureB, 0, fractureWidth);

            Array.Clear(chunkActive, 0, chunkActive.Length);
            Array.Clear(chunkActiveUntil, 0, chunkActiveUntil.Length);
        }

        public void ResetRecentPeaks()
        {
            recentPeakDispatches = lastUpdateDispatches;
            recentPeakCellIterations = lastUpdateCellIterations;
            recentPeakWindowEnd = Time.realtimeSinceStartupAsDouble + 5.0;
        }

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

        private void BeginTopologyStartupValidation()
        {
            if (topologyStartupValidationActive)
            {
                return;
            }

            topologyStartupValidationActive = true;
            topologyStartupValidationComplete = false;
            topologyStartupValidationStartedAt =
                Time.realtimeSinceStartupAsDouble;
            topologyStartupValidationTotalMilliseconds = 0.0;
            topologyStartupValidationSlowestStepMilliseconds = 0.0;
            topologyStartupValidationSlowestStep = "—";
            topologyStartupValidationStepCount = 0;
            topologyStartupValidationCacheInstallCount = 0;
            topologyStartupValidationObstacleBuildCount = 0;
            topologyStartupValidationMajorBuildCount = 0;
            topologyStartupValidationConnectorBuildCount = 0;
            topologyStartupValidationPocketBuildCount = 0;
        }

        private void RecordTopologyStartupStep(
            InitializationPhase phase,
            double elapsedMilliseconds)
        {
            if (!topologyStartupValidationActive)
            {
                return;
            }

            topologyStartupValidationStepCount++;
            if (elapsedMilliseconds >
                topologyStartupValidationSlowestStepMilliseconds)
            {
                topologyStartupValidationSlowestStepMilliseconds =
                    elapsedMilliseconds;
                topologyStartupValidationSlowestStep = phase.ToString();
            }
        }

        private void CompleteTopologyStartupValidation()
        {
            if (!topologyStartupValidationActive)
            {
                return;
            }

            topologyStartupValidationTotalMilliseconds = Math.Max(
                0.0,
                (Time.realtimeSinceStartupAsDouble -
                 topologyStartupValidationStartedAt) * 1000.0);
            topologyStartupValidationActive = false;
            topologyStartupValidationComplete = true;
        }

        private bool HasQueuedRebuildWork =>
            rebuildPhase != RebuildPhase.Idle ||
            pendingBoundaryRebuild ||
            pendingObstacleRebuild ||
            pendingTopologyRefresh;

        private void RequestBoundaryRebuild()
        {
            pendingBoundaryRebuild = true;
            pendingTopologyReplacementAfterMaintenance =
                DevelopmentTopologyGenerationInProgress;
            pendingTopologyRefresh = true;
        }

        private void RequestObstacleRebuild(
            int currentObstacleVersion,
            bool prepareTopologyReplacement)
        {
            pendingObstacleRebuild = true;
            pendingTopologyReplacementAfterMaintenance |=
                prepareTopologyReplacement;
            pendingTopologyRefresh = true;

            if (pendingObstacleObservedVersion != currentObstacleVersion)
            {
                pendingObstacleObservedVersion = currentObstacleVersion;
                pendingObstacleStableFrameCount = 0;
            }
        }

        private void QueueObstacleRebuildIfNeeded()
        {
            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            int currentObstacleVersion = disturbanceRuntime != null
                ? disturbanceRuntime.ObstacleGeometryVersion
                : -1;

            if (currentObstacleVersion == obstacleGeometryVersion)
            {
                return;
            }

            if (DevelopmentTopologyGenerationInProgress)
            {
                RequestObstacleRebuild(
                    currentObstacleVersion,
                    true);
                return;
            }

            activeTopologyObstacleStale = true;
            if (IsAutomaticDevelopmentCacheEnabled)
            {
                automaticTopologyGenerationInProgress = true;
                automaticDevelopmentRebuildReason =
                    AutomaticDevelopmentRebuildReason.Obstacles;
                topologyCacheStartupState =
                    "Using Previous Cache — Rebuilding";
                topologyCacheStartupSummary =
                    "Static obstacle geometry changed. The previous generated " +
                    "topology remains visible while the exact live obstacle " +
                    "field and a complete replacement are prepared " +
                    "automatically.";
                RequestObstacleRebuild(
                    currentObstacleVersion,
                    true);
                return;
            }

            MarkActiveTopologyCacheStale(
                "Stale — Obstacles Changed",
                "Static obstacle geometry changed after activation. The " +
                "exact live obstacle field is refreshed, while the generated " +
                "topology is retained until explicit development regeneration " +
                "or a valid cache reload.");
            RequestObstacleRebuild(
                currentObstacleVersion,
                false);
        }

        private void QueueMajorTopologyRebuildIfNeeded()
        {
            if (river == null || !river.Domain.IsValid ||
                initializationPhase != InitializationPhase.Ready ||
                resourcesDirty || fieldWidth < 2 || fieldHeight < 2)
            {
                return;
            }

            int requestedSignature = ResolveRequestedTopologySignature();
            int activeSignature = ResolveActiveTopologySignature();
            if (requestedSignature == activeSignature)
            {
                automaticDevelopmentObservedSignature = requestedSignature;
                if (topologyReplacementBuild != null &&
                    !topologyReplacementBuild.IsIdenticalValidation &&
                    topologyReplacementBuild.TargetSignature !=
                        requestedSignature)
                {
                    CancelTopologyReplacementBuild(true);
                }

                // A development setting may be dragged away from the active
                // value and then restored before its replacement completes.
                // Once the superseded settings-only build and maintenance
                // work are both gone, release automatic-generation ownership
                // instead of leaving the runtime permanently marked busy.
                if (automaticTopologyGenerationInProgress &&
                    automaticDevelopmentRebuildReason ==
                        AutomaticDevelopmentRebuildReason.Settings &&
                    topologyReplacementBuild == null &&
                    !HasQueuedRebuildWork)
                {
                    automaticTopologyGenerationInProgress = false;
                    automaticDevelopmentRebuildReason =
                        AutomaticDevelopmentRebuildReason.None;
                    topologyCacheStartupState =
                        topologyCacheLoadedForActiveResources
                            ? "Loaded"
                            : "Development Topology Current";
                    topologyCacheStartupSummary =
                        "The temporary setting change returned to the active " +
                        "topology before a replacement completed. No cache " +
                        "write or additional generation is required.";
                }
                return;
            }

            if (IsAutomaticDevelopmentCacheEnabled &&
                !explicitTopologyGenerationInProgress)
            {
                double now = Time.realtimeSinceStartupAsDouble;
                if (automaticDevelopmentObservedSignature !=
                    requestedSignature)
                {
                    automaticDevelopmentObservedSignature =
                        requestedSignature;
                    automaticDevelopmentRebuildNotBefore = now +
                        AutomaticDevelopmentRebuildDebounceSeconds;
                    if (topologyReplacementBuild != null &&
                        !topologyReplacementBuild.IsIdenticalValidation &&
                        topologyReplacementBuild.TargetSignature !=
                            requestedSignature)
                    {
                        CancelTopologyReplacementBuild(true);
                    }
                }

                if (!activeTopologyObstacleStale)
                {
                    topologyCacheStartupState =
                        "Using Previous Cache — Rebuilding";
                    topologyCacheStartupSummary =
                        "Topology settings changed. The active topology remains " +
                        "visible while the replacement request waits for the " +
                        "development debounce and then regenerates automatically.";
                }

                if (now < automaticDevelopmentRebuildNotBefore ||
                    HasQueuedRebuildWork)
                {
                    return;
                }

                automaticTopologyGenerationInProgress = true;
                if (automaticDevelopmentRebuildReason !=
                    AutomaticDevelopmentRebuildReason.Obstacles)
                {
                    automaticDevelopmentRebuildReason =
                        AutomaticDevelopmentRebuildReason.Settings;
                }
            }
            else if (!DevelopmentTopologyGenerationInProgress)
            {
                if (!activeTopologyObstacleStale)
                {
                    MarkActiveTopologyCacheStale(
                        "Stale — Settings Changed",
                        "Topology-generation settings changed after activation. " +
                        "The active prepared topology is retained until explicit " +
                        "development regeneration or a valid cache reload.");
                }

                CancelTopologyReplacementBuild(true);
                return;
            }

            RequestTopologyReplacement(
                automaticDevelopmentRebuildReason ==
                    AutomaticDevelopmentRebuildReason.Obstacles
                        ? TopologyReplacementReason.Obstacle
                        : TopologyReplacementReason.Settings,
                false);
        }

        private void MarkActiveTopologyCacheStale(
            string state,
            string summary)
        {
            topologyCacheStartupState = state ?? "Stale";
            topologyCacheStartupSummary = summary ?? string.Empty;
        }

        public bool RequestIdenticalTopologyReplacementValidation()
        {
            if (!Application.isPlaying ||
                initializationPhase != InitializationPhase.Ready ||
                !AreResourcesCompleteAndCurrent())
            {
                return false;
            }

            RequestTopologyReplacement(
                TopologyReplacementReason.IdenticalValidation,
                true);
            return topologyReplacementBuild != null;
        }

        public bool RunTopologyCacheRoundTripValidation()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            topologyCacheRoundTripRunCount++;
            topologyCacheRoundTripState = "Running";
            topologyCacheRoundTripSummary =
                "Capturing the immutable prepared topology graph.";

            if (!Application.isPlaying ||
                !TopologyCacheRoundTripReady ||
                river == null ||
                !river.Domain.IsValid ||
                TopologyReplacementInProgress)
            {
                topologyCacheRoundTripState = "Unavailable";
                topologyCacheRoundTripSummary =
                    "The proof requires a fully initialized Play-mode topology " +
                    "with no replacement build in progress.";
                return false;
            }

            try
            {
                if (!TryCaptureTopologyCacheFingerprints(
                        out StylizedRiverFoamTopologyFingerprint
                            domainFingerprint,
                        out StylizedRiverFoamTopologyFingerprint
                            obstacleFingerprint,
                        out StylizedRiverFoamTopologyFingerprint
                            generationFingerprint,
                        out _,
                        out string fingerprintError))
                {
                    topologyCacheRoundTripState = "Failed";
                    topologyCacheRoundTripSummary = fingerprintError;
                    return false;
                }

                StylizedRiverFoamTopologyCachePackage package =
                    CreateTopologyCachePackage(
                        domainFingerprint,
                        obstacleFingerprint,
                        generationFingerprint);

                StylizedRiverFoamTopologyCacheRoundTripResult result =
                    StylizedRiverFoamTopologyCacheCodec.ValidateRoundTrip(
                        package);
                topologyCacheRoundTripPayloadBytes = result.PayloadByteCount;
                topologyCacheRoundTripPayloadHash = result.PayloadHash;
                topologyCacheRoundTripSerializationMilliseconds =
                    result.SerializationMilliseconds;
                topologyCacheRoundTripLoadMilliseconds =
                    result.LoadMilliseconds;
                topologyCacheRoundTripVerificationMilliseconds =
                    result.VerificationMilliseconds;
                topologyCacheRoundTripSummary = result.Summary;
                topologyCacheRoundTripState = result.Passed
                    ? "Passed"
                    : "Failed";
                if (result.Passed)
                {
                    topologyCacheRoundTripPassCount++;
                    Debug.Log(
                        $"[River Foam 4.9A] Cache round-trip passed for " +
                        $"'{river.name}': {result.PayloadByteCount:N0} bytes, " +
                        $"hash {result.PayloadHash:X16}.",
                        river);
                }
                else
                {
                    Debug.LogError(
                        $"[River Foam 4.9A] Cache round-trip failed for " +
                        $"'{river.name}': {result.Summary}",
                        river);
                }

                return result.Passed;
            }
            catch (Exception exception)
            {
                topologyCacheRoundTripState = "Failed";
                topologyCacheRoundTripSummary = exception.Message;
                Debug.LogException(exception, river);
                return false;
            }
#else
            return false;
#endif
        }

        public bool TryBuildTopologyCache(
            out StylizedRiverFoamTopologyCacheBuildArtifact artifact)
        {
            artifact = default;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            topologyCacheBuildCount++;
            topologyCacheBuildState = "Building";
            topologyCacheBuildSummary =
                "Capturing stable inputs and validating the complete payload.";

            if (!Application.isPlaying ||
                !TopologyCacheBuildReady ||
                river == null ||
                !river.Domain.IsValid ||
                TopologyReplacementInProgress)
            {
                topologyCacheBuildState = "Unavailable";
                topologyCacheBuildSummary =
                    "Cache building requires a fully initialized Play-mode " +
                    "topology whose maintenance and replacement work is complete.";
                return false;
            }

            System.Diagnostics.Stopwatch stopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            try
            {
                if (!TryCaptureTopologyCacheFingerprints(
                        out StylizedRiverFoamTopologyFingerprint
                            domainFingerprint,
                        out StylizedRiverFoamTopologyFingerprint
                            obstacleFingerprint,
                        out StylizedRiverFoamTopologyFingerprint
                            generationFingerprint,
                        out StylizedRiverFoamTopologyFingerprint
                            combinedFingerprint,
                        out string fingerprintError))
                {
                    stopwatch.Stop();
                    topologyCacheBuildState = "Failed";
                    topologyCacheBuildSummary = fingerprintError;
                    topologyCacheBuildMilliseconds =
                        stopwatch.Elapsed.TotalMilliseconds;
                    return false;
                }

                StylizedRiverFoamTopologyCachePackage package =
                    CreateTopologyCachePackage(
                        domainFingerprint,
                        obstacleFingerprint,
                        generationFingerprint);
                StylizedRiverFoamTopologyCacheRoundTripResult proof =
                    StylizedRiverFoamTopologyCacheCodec.ValidateRoundTrip(
                        package);
                if (!proof.Passed)
                {
                    stopwatch.Stop();
                    topologyCacheBuildState = "Failed";
                    topologyCacheBuildSummary =
                        $"The accepted 4.9A proof failed during cache build: " +
                        proof.Summary;
                    topologyCacheBuildMilliseconds =
                        stopwatch.Elapsed.TotalMilliseconds;
                    return false;
                }

                byte[] payload =
                    StylizedRiverFoamTopologyCacheCodec.Serialize(package);
                ulong payloadHash =
                    StylizedRiverFoamTopologyCacheCodec.ReadPayloadHash(
                        payload);
                if (payload.Length != proof.PayloadByteCount ||
                    payloadHash != proof.PayloadHash)
                {
                    stopwatch.Stop();
                    topologyCacheBuildState = "Failed";
                    topologyCacheBuildSummary =
                        "The final cache payload did not reproduce the " +
                        "validated round-trip size and checksum.";
                    topologyCacheBuildMilliseconds =
                        stopwatch.Elapsed.TotalMilliseconds;
                    return false;
                }

                stopwatch.Stop();
                topologyCacheBuildPayloadBytes = payload.Length;
                topologyCacheBuildPayloadHash = payloadHash.ToString("X16");
                topologyCacheBuildMilliseconds =
                    stopwatch.Elapsed.TotalMilliseconds;
                topologyCacheBuildSuccessCount++;
                topologyCacheBuildState = "Built";
                topologyCacheBuildSummary =
                    "A complete versioned payload was built from the active " +
                    "prepared topology. It has not been loaded or activated.";

                artifact = new StylizedRiverFoamTopologyCacheBuildArtifact(
                    payload,
                    StylizedRiverFoamTopologyCacheCodec.FormatVersion,
                    StylizedRiverFoamTopologyCacheCodec
                        .GeneratorContractVersion,
                    domainFingerprint.ToString(),
                    obstacleFingerprint.ToString(),
                    generationFingerprint.ToString(),
                    combinedFingerprint.ToString(),
                    payloadHash.ToString("X16"),
                    topologyCacheBuildMilliseconds,
                    river.name);

                Debug.Log(
                    $"[River Foam 4.9B] Built cache payload for " +
                    $"'{river.name}': {payload.Length:N0} bytes, hash " +
                    $"{payloadHash:X16}, inputs {combinedFingerprint}.",
                    river);
                return true;
            }
            catch (Exception exception)
            {
                stopwatch.Stop();
                topologyCacheBuildState = "Failed";
                topologyCacheBuildSummary = exception.Message;
                topologyCacheBuildMilliseconds =
                    stopwatch.Elapsed.TotalMilliseconds;
                Debug.LogException(exception, river);
                return false;
            }
#else
            topologyCacheBuildState = "Unavailable";
            topologyCacheBuildSummary =
                "Explicit cache building is available only in the Editor or " +
                "a Development Build.";
            return false;
#endif
        }

        /// <summary>
        /// Validates the assigned persistent topology cache against the exact
        /// current authored inputs without activating Foam or allocating its GPU
        /// simulation field. Patch 4.9D build preflight uses this strict path; it
        /// never falls back to mesh scanning, generation, asset creation, or
        /// cache mutation.
        /// </summary>
        public bool TryValidateAssignedTopologyCacheForRelease(
            out string state,
            out string summary,
            out int payloadBytes,
            out string payloadHash,
            out int obstacleSourceCount)
        {
            state = "Invalid";
            summary = string.Empty;
            payloadBytes = 0;
            payloadHash = "—";
            obstacleSourceCount = 0;

            river ??= GetComponent<StylizedRiver>();
            if (river == null)
            {
                summary = "The Foam runtime has no StylizedRiver owner.";
                return false;
            }

            if (!river.Domain.IsValid)
            {
                state = "Inputs Unavailable";
                summary =
                    "The authored river does not currently expose a valid resampled domain.";
                return false;
            }

            StylizedRiverFoamTopologyCacheAsset asset =
                river.FoamTopologyCacheAsset;
            if (asset == null)
            {
                state = "Unassigned";
                summary = "No persistent Foam topology cache is assigned.";
                return false;
            }

            if (!asset.HasSupportedStorageContract)
            {
                state = "Unsupported Storage";
                summary =
                    "The assigned asset uses an unsupported storage contract.";
                return false;
            }

            if (!asset.HasPayload)
            {
                state = "Empty";
                summary = "The assigned cache asset contains no payload bytes.";
                return false;
            }

            byte[] payload = asset.GetPayloadCopy();
            payloadBytes = payload.Length;
            if (!StylizedRiverFoamTopologyCacheCodec.TryDeserialize(
                    payload,
                    out StylizedRiverFoamTopologyCachePackage package,
                    out string loadError))
            {
                state = "Invalid Payload";
                summary = loadError;
                return false;
            }

            ulong resolvedPayloadHash =
                StylizedRiverFoamTopologyCacheCodec.ReadPayloadHash(payload);
            payloadHash = resolvedPayloadHash.ToString("X16");
            StylizedRiverFoamTopologyFingerprint storedCombined =
                StylizedRiverFoamTopologyFingerprints.ComputeCombined(
                    package.DomainFingerprint,
                    package.ObstacleFingerprint,
                    package.GenerationFingerprint);
            bool metadataMatches =
                asset.PayloadFormatVersion ==
                    StylizedRiverFoamTopologyCacheCodec.FormatVersion &&
                asset.GeneratorContractVersion ==
                    StylizedRiverFoamTopologyCacheCodec
                        .GeneratorContractVersion &&
                asset.PayloadHash == payloadHash &&
                asset.DomainFingerprint ==
                    package.DomainFingerprint.ToString() &&
                asset.ObstacleFingerprint ==
                    package.ObstacleFingerprint.ToString() &&
                asset.GenerationFingerprint ==
                    package.GenerationFingerprint.ToString() &&
                asset.CombinedFingerprint == storedCombined.ToString();
            if (!metadataMatches)
            {
                state = "Metadata Mismatch";
                summary =
                    "The cache asset metadata does not match its validated binary payload.";
                return false;
            }

            bool completeContract =
                package.FieldWidth >= 2 &&
                package.FieldHeight >= 2 &&
                package.FieldLength > 0f &&
                package.ValidFieldLength > 0f &&
                package.ObstacleExclusion != null &&
                package.ObstacleExclusion.Length ==
                    package.FieldWidth * package.FieldHeight &&
                package.MajorTopology != null &&
                package.ConnectorTopology != null &&
                package.PocketTopology != null;
            if (!completeContract)
            {
                state = "Incomplete Contract";
                summary =
                    "The payload does not contain one complete prepared topology graph and obstacle field.";
                return false;
            }

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            if (disturbanceRuntime != null &&
                !disturbanceRuntime
                    .PrepareGeneratedGeometrySourcesForCacheValidation(
                        out string preparationStatus))
            {
                state = "Obstacle Inputs Unavailable";
                summary = preparationStatus;
                return false;
            }

            topologyCachePreparedObstacleFingerprints.Clear();
            StylizedRiverFoamTopologyFingerprint currentObstacles = default;
            string obstacleStatus = string.Empty;
            bool obstacleInputsAvailable;
            if (disturbanceRuntime == null)
            {
                obstacleInputsAvailable = RiverObstacleExclusionResolver
                    .TryCombineStableSourceFingerprints(
                        topologyCachePreparedObstacleFingerprints,
                        out currentObstacles,
                        out obstacleSourceCount,
                        out obstacleStatus);
            }
            else
            {
                obstacleInputsAvailable = disturbanceRuntime
                    .TryCopyObstacleExclusionStableFingerprintsTo(
                        topologyCachePreparedObstacleFingerprints,
                        out obstacleSourceCount,
                        out obstacleStatus) &&
                    RiverObstacleExclusionResolver
                        .TryCombineStableSourceFingerprints(
                            topologyCachePreparedObstacleFingerprints,
                            out currentObstacles,
                            out obstacleSourceCount,
                            out obstacleStatus);
            }
            if (!obstacleInputsAvailable)
            {
                state = "Obstacle Inputs Unavailable";
                summary = obstacleStatus;
                return false;
            }

            StylizedRiverFoamTopologyFingerprint currentDomain =
                StylizedRiverFoamTopologyFingerprints.ComputeDomain(
                    river.Domain);
            StylizedRiverFoamTopologyFingerprint currentGeneration =
                StylizedRiverFoamTopologyFingerprints.ComputeGeneration(
                    river,
                    package.FieldWidth,
                    package.FieldHeight,
                    package.FieldLength,
                    package.ValidFieldLength);
            StylizedRiverFoamTopologyFingerprint currentCombined =
                StylizedRiverFoamTopologyFingerprints.ComputeCombined(
                    currentDomain,
                    currentObstacles,
                    currentGeneration);

            if (package.DomainFingerprint != currentDomain)
            {
                state = "Stale Domain";
                summary =
                    "The complete resampled river-domain content changed.";
                return false;
            }

            if (package.ObstacleFingerprint != currentObstacles)
            {
                state = "Stale Obstacles";
                summary =
                    "The exact transformed static obstacle-source geometry changed.";
                return false;
            }

            if (package.GenerationFingerprint != currentGeneration)
            {
                state = "Stale Settings";
                summary =
                    "Quality, field mapping, or a topology-generation setting changed.";
                return false;
            }

            if (asset.CombinedFingerprint != currentCombined.ToString())
            {
                state = "Combined Key Mismatch";
                summary =
                    "The assigned cache does not match the current combined stable input key.";
                return false;
            }

            state = "Valid";
            summary =
                "The complete payload, metadata, domain, exact obstacle sources, and generation inputs are release-ready.";
            return true;
        }

        public bool ValidateAssignedTopologyCache()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            topologyCacheValidationCount++;
            topologyCacheValidationState = "Evaluating";
            topologyCacheValidationSummary =
                "Comparing the assigned payload with current stable inputs.";

            if (!Application.isPlaying ||
                !TopologyCacheBuildReady ||
                river == null ||
                !river.Domain.IsValid ||
                TopologyReplacementInProgress)
            {
                SetTopologyCacheValidationMiss(
                    "Unavailable",
                    "Validation requires a fully initialized Play-mode " +
                    "topology whose maintenance and replacement work is complete.");
                return false;
            }

            StylizedRiverFoamTopologyCacheAsset asset =
                river.FoamTopologyCacheAsset;
            if (asset == null)
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Unassigned",
                    "No Foam topology cache asset is assigned to this river.");
                return false;
            }

            if (!asset.HasSupportedStorageContract)
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Storage Version",
                    "The assigned asset uses an unsupported storage contract.");
                return false;
            }

            if (!asset.HasPayload)
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Empty",
                    "The assigned cache asset contains no payload bytes.");
                return false;
            }

            if (!TryCaptureTopologyCacheFingerprints(
                    out StylizedRiverFoamTopologyFingerprint
                        currentDomain,
                    out StylizedRiverFoamTopologyFingerprint
                        currentObstacles,
                    out StylizedRiverFoamTopologyFingerprint
                        currentGeneration,
                    out StylizedRiverFoamTopologyFingerprint
                        currentCombined,
                    out string fingerprintError))
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Inputs Unavailable",
                    fingerprintError);
                return false;
            }

            byte[] payload = asset.GetPayloadCopy();
            if (!StylizedRiverFoamTopologyCacheCodec.TryDeserialize(
                    payload,
                    out StylizedRiverFoamTopologyCachePackage package,
                    out string loadError))
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Invalid Payload",
                    loadError);
                return false;
            }

            ulong payloadHash =
                StylizedRiverFoamTopologyCacheCodec.ReadPayloadHash(payload);
            StylizedRiverFoamTopologyFingerprint storedCombined =
                StylizedRiverFoamTopologyFingerprints.ComputeCombined(
                    package.DomainFingerprint,
                    package.ObstacleFingerprint,
                    package.GenerationFingerprint);
            bool metadataMatches =
                asset.PayloadFormatVersion ==
                    StylizedRiverFoamTopologyCacheCodec.FormatVersion &&
                asset.GeneratorContractVersion ==
                    StylizedRiverFoamTopologyCacheCodec
                        .GeneratorContractVersion &&
                asset.PayloadHash == payloadHash.ToString("X16") &&
                asset.DomainFingerprint ==
                    package.DomainFingerprint.ToString() &&
                asset.ObstacleFingerprint ==
                    package.ObstacleFingerprint.ToString() &&
                asset.GenerationFingerprint ==
                    package.GenerationFingerprint.ToString() &&
                asset.CombinedFingerprint == storedCombined.ToString();
            if (!metadataMatches)
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Metadata Mismatch",
                    "The asset metadata does not match its validated binary " +
                    "payload. Rebuild the cache asset explicitly.");
                return false;
            }

            if (package.DomainFingerprint != currentDomain)
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Stale Domain",
                    "The complete resampled river-domain content changed.");
                return false;
            }

            if (package.ObstacleFingerprint != currentObstacles)
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Stale Obstacles",
                    "The exact transformed static obstacle-source geometry " +
                    "changed.");
                return false;
            }

            if (package.GenerationFingerprint != currentGeneration)
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Stale Settings",
                    "Quality, field mapping, or a topology-generation setting " +
                    "changed.");
                return false;
            }

            if (asset.CombinedFingerprint != currentCombined.ToString())
            {
                SetTopologyCacheValidationMiss(
                    "Miss — Combined Key",
                    "The assigned cache does not match the current combined " +
                    "stable input key.");
                return false;
            }

            topologyCacheValidationHitCount++;
            topologyCacheValidationState = "Hit Candidate";
            topologyCacheValidationSummary =
                "The assigned payload is valid and all stable inputs match. " +
                "Patch 4.9C startup ownership is reported separately above.";
            Debug.Log(
                $"[River Foam 4.9B] Cache hit candidate for '{river.name}': " +
                $"{payload.Length:N0} bytes, inputs {currentCombined}.",
                river);
            return true;
#else
            SetTopologyCacheValidationMiss(
                "Unavailable",
                "Explicit cache validation is available only in the Editor " +
                "or a Development Build.");
            return false;
#endif
        }

        private bool AreTopologyCacheInputsCurrent()
        {
            if (!TopologyCacheRoundTripReady ||
                HasQueuedRebuildWork ||
                pendingTopologyReplacementAfterMaintenance ||
                TopologyReplacementInProgress ||
                river == null ||
                !river.Domain.IsValid)
            {
                return false;
            }

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            int currentObstacleGeometryVersion =
                disturbanceRuntime != null
                    ? disturbanceRuntime.ObstacleGeometryVersion
                    : -1;
            if (currentObstacleGeometryVersion != obstacleGeometryVersion)
            {
                return false;
            }

            return majorTopologyInputSignature ==
                    ResolveMajorTopologyInputSignature() &&
                connectorTopologyInputSignature ==
                    ResolveConnectorTopologyInputSignature() &&
                pocketTopologyInputSignature ==
                    ResolvePocketTopologyInputSignature();
        }

        private bool TryCaptureTopologyCacheFingerprints(
            out StylizedRiverFoamTopologyFingerprint domainFingerprint,
            out StylizedRiverFoamTopologyFingerprint obstacleFingerprint,
            out StylizedRiverFoamTopologyFingerprint generationFingerprint,
            out StylizedRiverFoamTopologyFingerprint combinedFingerprint,
            out string error,
            bool allowMeshFingerprintFallback = true)
        {
            domainFingerprint = default;
            obstacleFingerprint = default;
            generationFingerprint = default;
            combinedFingerprint = default;
            error = string.Empty;

            if (river == null || !river.Domain.IsValid)
            {
                error = "A valid resampled river domain is required.";
                return false;
            }

            domainFingerprint =
                StylizedRiverFoamTopologyFingerprints.ComputeDomain(
                    river.Domain);
            generationFingerprint =
                StylizedRiverFoamTopologyFingerprints.ComputeGeneration(
                    river,
                    fieldWidth,
                    fieldHeight,
                    fieldLength,
                    validFieldLength);

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            if (disturbanceRuntime != null &&
                !disturbanceRuntime.GeneratedObstacleRegistryReady)
            {
                error =
                    "The generated obstacle registry is still refreshing " +
                    $"({disturbanceRuntime.GeneratedObstacleRegistryProcessedCount:N0} / " +
                    $"{disturbanceRuntime.GeneratedObstacleRegistryTotalCount:N0} sources).";
                return false;
            }

            topologyCachePreparedObstacleFingerprints.Clear();
            int sourceCount;
            string obstacleStatus;
            bool preparedFingerprintsAvailable = disturbanceRuntime == null
                ? RiverObstacleExclusionResolver
                    .TryCombineStableSourceFingerprints(
                        topologyCachePreparedObstacleFingerprints,
                        out obstacleFingerprint,
                        out sourceCount,
                        out obstacleStatus)
                : disturbanceRuntime
                    .TryCopyObstacleExclusionStableFingerprintsTo(
                        topologyCachePreparedObstacleFingerprints,
                        out sourceCount,
                        out obstacleStatus) &&
                  RiverObstacleExclusionResolver
                    .TryCombineStableSourceFingerprints(
                        topologyCachePreparedObstacleFingerprints,
                        out obstacleFingerprint,
                        out sourceCount,
                        out obstacleStatus);

            if (!preparedFingerprintsAvailable)
            {
                if (!allowMeshFingerprintFallback)
                {
                    error = obstacleStatus;
                    return false;
                }

                topologyCacheFingerprintMeshFilters.Clear();
                disturbanceRuntime?.CopyObstacleExclusionMeshFiltersTo(
                    topologyCacheFingerprintMeshFilters);
                if (!RiverObstacleExclusionResolver
                    .TryComputeStableSourceFingerprint(
                        topologyCacheFingerprintMeshFilters,
                        out obstacleFingerprint,
                        out sourceCount,
                        out obstacleStatus))
                {
                    error = obstacleStatus;
                    return false;
                }
            }

            combinedFingerprint =
                StylizedRiverFoamTopologyFingerprints.ComputeCombined(
                    domainFingerprint,
                    obstacleFingerprint,
                    generationFingerprint);
            topologyCacheDomainFingerprint = domainFingerprint.ToString();
            topologyCacheObstacleFingerprint =
                obstacleFingerprint.ToString();
            topologyCacheGenerationFingerprint =
                generationFingerprint.ToString();
            topologyCacheCombinedFingerprint =
                combinedFingerprint.ToString();
            topologyCacheObstacleSourceCount = sourceCount;
            return true;
        }

        private void BeginAutomaticDevelopmentStartupGeneration()
        {
            automaticTopologyGenerationInProgress = true;
            automaticDevelopmentRebuildReason =
                AutomaticDevelopmentRebuildReason.StartupMiss;
            pendingAutomaticDevelopmentRebuildAfterStartup = false;
            pendingStartupCacheStaleObstacles = false;
            pendingStartupCacheStaleSettings = false;
            topologyCacheLoadedForActiveResources = false;
            topologyCacheStartupState = "Auto-Generating Cache Miss";
            topologyCacheStartupSummary =
                "No current persistent payload could be installed. This " +
                "Editor/Development session is running the accepted expensive " +
                "preparation path automatically. The Editor will validate and " +
                "save the completed result without additional user actions.";
        }

        private void BeginAutomaticDevelopmentRebuildAfterStartup()
        {
            if (!pendingAutomaticDevelopmentRebuildAfterStartup ||
                !IsAutomaticDevelopmentCacheEnabled ||
                initializationPhase != InitializationPhase.Ready)
            {
                return;
            }

            pendingAutomaticDevelopmentRebuildAfterStartup = false;
            automaticTopologyGenerationInProgress = true;
            automaticDevelopmentObservedSignature =
                ResolveRequestedTopologySignature();
            automaticDevelopmentRebuildNotBefore =
                Time.realtimeSinceStartupAsDouble;

            if (pendingStartupCacheStaleObstacles)
            {
                automaticDevelopmentRebuildReason =
                    AutomaticDevelopmentRebuildReason.Obstacles;
                obstacleExclusionUsesCachedScalar = false;
                RequestObstacleRebuild(
                    ResolveCurrentObstacleGeometryVersion(),
                    true);
            }
            else
            {
                automaticDevelopmentRebuildReason =
                    AutomaticDevelopmentRebuildReason.Settings;
                RequestTopologyReplacement(
                    TopologyReplacementReason.Settings,
                    false);
            }

            pendingStartupCacheStaleObstacles = false;
            pendingStartupCacheStaleSettings = false;
        }

        private void CompleteDevelopmentTopologyGeneration(
            string completedPath)
        {
            bool completedAutomatically =
                automaticTopologyGenerationInProgress;
            bool completedExplicitly =
                explicitTopologyGenerationInProgress;
            automaticTopologyGenerationInProgress = false;
            explicitTopologyGenerationInProgress = false;
            automaticDevelopmentRebuildReason =
                AutomaticDevelopmentRebuildReason.None;
            activeTopologyObstacleStale = false;
            if (completedAutomatically || completedExplicitly)
            {
                topologyCacheLoadedForActiveResources = false;
            }

            if (completedAutomatically)
            {
                QueueAutomaticTopologyCachePersistence(completedPath);
            }
            else if (completedExplicitly)
            {
                topologyCacheStartupState = "Generated Explicitly";
                topologyCacheStartupSummary =
                    "Development diagnostics explicitly ran the expensive " +
                    "preparation path. Automatic development caching was not " +
                    "required for this operation.";
            }
        }

        private void QueueAutomaticTopologyCachePersistence(
            string completedPath)
        {
            if (!TryCaptureTopologyCacheFingerprints(
                    out _,
                    out _,
                    out _,
                    out StylizedRiverFoamTopologyFingerprint currentCombined,
                    out string fingerprintError))
            {
                automaticTopologyCacheWritePending = false;
                automaticTopologyCachePersistenceState =
                    "Cache Write Failed";
                automaticTopologyCachePersistenceSummary =
                    "The generated topology is active, but its stable input " +
                    $"key could not be captured: {fingerprintError}";
                topologyCacheStartupState =
                    "Generated — Cache Write Failed";
                topologyCacheStartupSummary =
                    automaticTopologyCachePersistenceSummary;
                return;
            }

            string currentInputKey = currentCombined.ToString();
            if (!topologyCacheSessionPersistentInputCaptured ||
                topologyCacheSessionPersistentInputKey != currentInputKey)
            {
                automaticTopologyCacheWritePending = false;
                automaticTopologyCachePersistenceState =
                    "Session-Only Development Topology";
                automaticTopologyCachePersistenceSummary =
                    "The generated topology is active for this Play session, " +
                    "but its inputs differ from the persistent values captured " +
                    "at Play entry, so the authored cache was not overwritten.";
                topologyCacheStartupState =
                    "Session-Only Development Topology";
                topologyCacheStartupSummary =
                    automaticTopologyCachePersistenceSummary;
                return;
            }

#if UNITY_EDITOR
            automaticTopologyCacheWritePending = true;
            automaticTopologyCachePersistenceState =
                "Pending Automatic Save";
            automaticTopologyCachePersistenceSummary =
                $"The complete topology produced by {completedPath} is active. " +
                "The Editor cache coordinator will build, validate, and save " +
                "its persistent payload automatically.";
            topologyCacheStartupState = "Auto-Generated — Saving Cache";
            topologyCacheStartupSummary =
                automaticTopologyCachePersistenceSummary;
#else
            automaticTopologyCacheWritePending = false;
            automaticTopologyCachePersistenceState =
                "Session-Only Development Topology";
            automaticTopologyCachePersistenceSummary =
                "A Development Player generated the topology successfully. " +
                "Asset persistence is Editor-only, so this result remains " +
                "session-local.";
            topologyCacheStartupState =
                "Session-Only Development Topology";
            topologyCacheStartupSummary =
                automaticTopologyCachePersistenceSummary;
#endif
        }

        public bool TryBuildAutomaticTopologyCache(
            out StylizedRiverFoamTopologyCacheBuildArtifact artifact)
        {
            artifact = default;
#if UNITY_EDITOR
            if (!automaticTopologyCacheWritePending ||
                !TopologyCacheBuildReady)
            {
                return false;
            }

            automaticTopologyCacheWritePending = false;
            automaticTopologyCacheWriteCount++;
            if (!TryBuildTopologyCache(out artifact))
            {
                automaticTopologyCachePersistenceState =
                    "Cache Write Failed";
                automaticTopologyCachePersistenceSummary =
                    "The automatic payload build failed. The generated topology " +
                    "remains active and any previous cache asset remains available.";
                topologyCacheStartupState =
                    "Generated — Cache Write Failed";
                topologyCacheStartupSummary =
                    automaticTopologyCachePersistenceSummary;
                return false;
            }

            automaticTopologyCachePersistenceState =
                "Built — Awaiting Editor Save";
            automaticTopologyCachePersistenceSummary =
                "The payload passed the exact round-trip proof and is ready for " +
                "the Editor coordinator to store in the assigned asset.";
            return true;
#else
            return false;
#endif
        }

        public bool ValidateAutomaticTopologyCacheArtifact(
            StylizedRiverFoamTopologyCacheBuildArtifact artifact,
            out string error)
        {
            error = string.Empty;
#if UNITY_EDITOR
            topologyCacheValidationCount++;
            if (!Application.isPlaying || !TopologyCacheBuildReady ||
                river == null || !river.Domain.IsValid)
            {
                error =
                    "Automatic persistence requires a complete current topology.";
                SetTopologyCacheValidationMiss(
                    "Unavailable",
                    error);
                return false;
            }

            if (artifact.PayloadByteCount <= 0 ||
                artifact.FormatVersion !=
                    StylizedRiverFoamTopologyCacheCodec.FormatVersion ||
                artifact.GeneratorContractVersion !=
                    StylizedRiverFoamTopologyCacheCodec
                        .GeneratorContractVersion)
            {
                error =
                    "The automatic build artifact is empty or uses an unsupported contract.";
                SetTopologyCacheValidationMiss(
                    "Miss — Build Artifact",
                    error);
                return false;
            }

            byte[] payload = artifact.CopyPayload();
            if (!StylizedRiverFoamTopologyCacheCodec.TryDeserialize(
                    payload,
                    out StylizedRiverFoamTopologyCachePackage package,
                    out string loadError))
            {
                error = loadError;
                SetTopologyCacheValidationMiss(
                    "Miss — Invalid Build Artifact",
                    error);
                return false;
            }

            ulong payloadHash =
                StylizedRiverFoamTopologyCacheCodec.ReadPayloadHash(payload);
            StylizedRiverFoamTopologyFingerprint storedCombined =
                StylizedRiverFoamTopologyFingerprints.ComputeCombined(
                    package.DomainFingerprint,
                    package.ObstacleFingerprint,
                    package.GenerationFingerprint);
            bool artifactMetadataMatches =
                artifact.PayloadHash == payloadHash.ToString("X16") &&
                artifact.DomainFingerprint ==
                    package.DomainFingerprint.ToString() &&
                artifact.ObstacleFingerprint ==
                    package.ObstacleFingerprint.ToString() &&
                artifact.GenerationFingerprint ==
                    package.GenerationFingerprint.ToString() &&
                artifact.CombinedFingerprint == storedCombined.ToString();
            if (!artifactMetadataMatches)
            {
                error =
                    "The automatic build artifact metadata does not match its payload.";
                SetTopologyCacheValidationMiss(
                    "Miss — Build Metadata",
                    error);
                return false;
            }

            if (!TryCaptureTopologyCacheFingerprints(
                    out StylizedRiverFoamTopologyFingerprint currentDomain,
                    out StylizedRiverFoamTopologyFingerprint currentObstacles,
                    out StylizedRiverFoamTopologyFingerprint currentGeneration,
                    out StylizedRiverFoamTopologyFingerprint currentCombined,
                    out string fingerprintError))
            {
                error = fingerprintError;
                SetTopologyCacheValidationMiss(
                    "Miss — Inputs Unavailable",
                    error);
                return false;
            }

            bool current = package.FieldWidth == fieldWidth &&
                package.FieldHeight == fieldHeight &&
                package.DomainFingerprint == currentDomain &&
                package.ObstacleFingerprint == currentObstacles &&
                package.GenerationFingerprint == currentGeneration &&
                artifact.CombinedFingerprint == currentCombined.ToString();
            if (!current)
            {
                error =
                    "Stable inputs changed after the automatic payload was built; the previous persistent cache was retained.";
                SetTopologyCacheValidationMiss(
                    "Miss — Inputs Changed During Save",
                    error);
                return false;
            }

            topologyCacheValidationHitCount++;
            topologyCacheValidationState = "Hit Candidate";
            topologyCacheValidationSummary =
                "The automatic payload passed its binary contract, metadata, " +
                "field-dimension, and current stable-input checks before the " +
                "persistent asset was modified.";
            return true;
#else
            error =
                "Automatic persistent asset validation is Editor-only.";
            return false;
#endif
        }

        public void ReportAutomaticTopologyCachePersisted(
            int payloadBytes,
            string payloadHash,
            string assetPath)
        {
#if UNITY_EDITOR
            automaticTopologyCacheWriteSuccessCount++;
            automaticTopologyCachePersistenceState =
                "Automatically Generated and Cached";
            automaticTopologyCachePersistenceSummary =
                $"The complete payload was validated and saved to " +
                $"'{assetPath}'. Future matching Play entries load it directly.";
            topologyCacheStartupPayloadBytes = Mathf.Max(0, payloadBytes);
            topologyCacheStartupPayloadHash =
                string.IsNullOrEmpty(payloadHash) ? "—" : payloadHash;
            topologyCacheStartupState =
                "Automatically Generated and Cached";
            topologyCacheStartupSummary =
                automaticTopologyCachePersistenceSummary;
            Debug.Log(
                $"[River Foam 4.9C.1] Automatically cached topology for " +
                $"'{river.name}': {payloadBytes:N0} bytes, hash {payloadHash}.",
                river);
#endif
        }

        public void ReportAutomaticTopologyCachePersistenceFailure(
            string reason)
        {
#if UNITY_EDITOR
            automaticTopologyCacheWritePending = false;
            automaticTopologyCachePersistenceState = "Cache Write Failed";
            automaticTopologyCachePersistenceSummary =
                string.IsNullOrEmpty(reason)
                    ? "The automatic Editor cache write failed."
                    : reason;
            topologyCacheStartupState =
                "Generated — Cache Write Failed";
            topologyCacheStartupSummary =
                automaticTopologyCachePersistenceSummary;
            Debug.LogWarning(
                $"[River Foam 4.9C.1] Automatic cache persistence failed for " +
                $"'{river.name}': {automaticTopologyCachePersistenceSummary}",
                river);
#endif
        }

        public bool RequestExplicitTopologyGeneration()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!Application.isPlaying || DevelopmentTopologyGenerationInProgress)
            {
                return false;
            }

            if (initializationPhase ==
                InitializationPhase.AwaitExplicitTopologyGeneration)
            {
                explicitTopologyGenerationRequested = true;
                explicitTopologyGenerationInProgress = true;
                automaticTopologyGenerationInProgress = false;
                automaticTopologyCacheWritePending = false;
                topologyCacheStartupState = "Generating Explicitly";
                topologyCacheStartupSummary =
                    "Development tooling explicitly authorized the expensive " +
                    "obstacle and topology preparation path.";
                return true;
            }

            if (initializationPhase == InitializationPhase.Ready)
            {
                int obstacleVersion = ResolveCurrentObstacleGeometryVersion();
                bool obstacleTopologyStale =
                    obstacleVersion != obstacleGeometryVersion ||
                    activeTopologyObstacleStale;
                bool settingsTopologyStale =
                    ResolveRequestedTopologySignature() !=
                    ResolveActiveTopologySignature();
                if (!obstacleTopologyStale && !settingsTopologyStale)
                {
                    return false;
                }

                explicitTopologyGenerationInProgress = true;
                automaticTopologyGenerationInProgress = false;
                automaticTopologyCacheWritePending = false;
                topologyCacheLoadedForActiveResources = false;
                if (obstacleTopologyStale)
                {
                    obstacleExclusionUsesCachedScalar = false;
                    RequestObstacleRebuild(
                        obstacleVersion,
                        true);
                }
                else
                {
                    RequestTopologyReplacement(
                        TopologyReplacementReason.Settings,
                        false);
                }

                topologyCacheStartupState = "Generating Explicitly";
                topologyCacheStartupSummary =
                    "Development tooling explicitly authorized replacement " +
                    "generation for the stale active topology.";
                return true;
            }

            return false;
#else
            return false;
#endif
        }

        private int ResolveCurrentObstacleGeometryVersion()
        {
            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            return disturbanceRuntime != null
                ? disturbanceRuntime.ObstacleGeometryVersion
                : -1;
        }

        private TopologyCacheStartupResolution
            TryResolveAssignedTopologyCacheForStartup(
                out StylizedRiverFoamTopologyCachePackage package,
                out bool staleObstacles,
                out bool staleSettings)
        {
            package = null;
            staleObstacles = false;
            staleSettings = false;
            topologyCacheStartupAttemptCount++;
            topologyCacheStartupState = "Evaluating";
            topologyCacheStartupSummary =
                "Validating the assigned payload against prepared stable inputs.";
            topologyCacheStartupPayloadBytes = 0;
            topologyCacheStartupPayloadHash = "—";
            topologyCacheStartupLoadMilliseconds = 0.0;

            System.Diagnostics.Stopwatch stopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            if (!TryCaptureTopologyCacheFingerprints(
                    out StylizedRiverFoamTopologyFingerprint currentDomain,
                    out StylizedRiverFoamTopologyFingerprint currentObstacles,
                    out StylizedRiverFoamTopologyFingerprint currentGeneration,
                    out StylizedRiverFoamTopologyFingerprint currentCombined,
                    out string fingerprintError,
                    IsAutomaticDevelopmentCacheEnabled))
            {
                SetTopologyCacheStartupMiss(
                    "Miss — Inputs Unavailable",
                    fingerprintError);
                return TopologyCacheStartupResolution.Miss;
            }

            CaptureTopologyCacheSessionPersistentInputKey(currentCombined);

            StylizedRiverFoamTopologyCacheAsset asset =
                river != null ? river.FoamTopologyCacheAsset : null;
            if (asset == null)
            {
                SetTopologyCacheStartupMiss(
                    "Miss — Unassigned",
                    "No Foam topology cache asset is assigned.");
                return TopologyCacheStartupResolution.Miss;
            }

            if (!asset.HasSupportedStorageContract)
            {
                SetTopologyCacheStartupMiss(
                    "Miss — Storage Version",
                    "The assigned asset uses an unsupported storage contract.");
                return TopologyCacheStartupResolution.Miss;
            }

            if (!asset.HasPayload)
            {
                SetTopologyCacheStartupMiss(
                    "Miss — Empty",
                    "The assigned cache asset contains no payload bytes.");
                return TopologyCacheStartupResolution.Miss;
            }

            byte[] payload = asset.GetPayloadCopy();
            if (!StylizedRiverFoamTopologyCacheCodec.TryDeserialize(
                    payload,
                    out package,
                    out string loadError))
            {
                SetTopologyCacheStartupMiss(
                    "Miss — Invalid Payload",
                    loadError);
                package = null;
                return TopologyCacheStartupResolution.Miss;
            }

            ulong payloadHash =
                StylizedRiverFoamTopologyCacheCodec.ReadPayloadHash(payload);
            StylizedRiverFoamTopologyFingerprint storedCombined =
                StylizedRiverFoamTopologyFingerprints.ComputeCombined(
                    package.DomainFingerprint,
                    package.ObstacleFingerprint,
                    package.GenerationFingerprint);
            bool metadataMatches =
                asset.PayloadFormatVersion ==
                    StylizedRiverFoamTopologyCacheCodec.FormatVersion &&
                asset.GeneratorContractVersion ==
                    StylizedRiverFoamTopologyCacheCodec
                        .GeneratorContractVersion &&
                asset.PayloadHash == payloadHash.ToString("X16") &&
                asset.DomainFingerprint ==
                    package.DomainFingerprint.ToString() &&
                asset.ObstacleFingerprint ==
                    package.ObstacleFingerprint.ToString() &&
                asset.GenerationFingerprint ==
                    package.GenerationFingerprint.ToString() &&
                asset.CombinedFingerprint == storedCombined.ToString();
            if (!metadataMatches)
            {
                SetTopologyCacheStartupMiss(
                    "Miss — Metadata Mismatch",
                    "The asset metadata does not match its binary payload.");
                package = null;
                return TopologyCacheStartupResolution.Miss;
            }

            bool completeCurrentContract =
                package.FieldWidth == fieldWidth &&
                package.FieldHeight == fieldHeight &&
                package.ObstacleExclusion != null &&
                package.ObstacleExclusion.Length == fieldWidth * fieldHeight &&
                package.MajorTopology != null &&
                package.ConnectorTopology != null &&
                package.PocketTopology != null;
            if (!completeCurrentContract)
            {
                SetTopologyCacheStartupMiss(
                    "Miss — Incomplete Contract",
                    "The payload does not contain a complete topology graph " +
                    "for the current field dimensions.");
                package = null;
                return TopologyCacheStartupResolution.Miss;
            }

            bool domainMatches = package.DomainFingerprint == currentDomain;
            bool obstaclesMatch =
                package.ObstacleFingerprint == currentObstacles;
            bool generationMatches =
                package.GenerationFingerprint == currentGeneration;
            bool combinedMatches =
                asset.CombinedFingerprint == currentCombined.ToString();

            stopwatch.Stop();
            topologyCacheStartupPayloadBytes = payload.Length;
            topologyCacheStartupPayloadHash = payloadHash.ToString("X16");
            topologyCacheStartupLoadMilliseconds =
                stopwatch.Elapsed.TotalMilliseconds;

            if (domainMatches && obstaclesMatch && generationMatches &&
                combinedMatches)
            {
                topologyCacheStartupHitCount++;
                topologyCacheStartupState = "Hit — Ready to Install";
                topologyCacheStartupSummary =
                    "The complete payload and all stable inputs match. No " +
                    "obstacle mesh scan, GPU readback, or topology generator " +
                    "was used.";
                return TopologyCacheStartupResolution.ExactHit;
            }

            staleObstacles = !obstaclesMatch;
            staleSettings = !generationMatches || !combinedMatches;
            if (IsAutomaticDevelopmentCacheEnabled && domainMatches &&
                (staleObstacles || staleSettings))
            {
                topologyCacheStartupMissCount++;
                topologyCacheStartupState =
                    "Using Previous Cache — Rebuilding";
                topologyCacheStartupSummary =
                    "The assigned cache is structurally valid and uses the " +
                    "same river mapping and dimensions. Its previous generated " +
                    "topology will remain visible while current development " +
                    "inputs regenerate automatically.";
                return TopologyCacheStartupResolution.StaleCompatible;
            }

            if (!domainMatches)
            {
                SetTopologyCacheStartupMiss(
                    "Miss — Stale Domain",
                    "The complete resampled river-domain content changed.");
            }
            else if (staleObstacles)
            {
                SetTopologyCacheStartupMiss(
                    "Miss — Stale Obstacles",
                    "The exact transformed static obstacle geometry changed.");
            }
            else
            {
                SetTopologyCacheStartupMiss(
                    "Miss — Stale Settings",
                    "Quality, mapping, or topology-generation settings changed.");
            }

            package = null;
            return TopologyCacheStartupResolution.Miss;
        }

        private void CaptureTopologyCacheSessionPersistentInputKey(
            StylizedRiverFoamTopologyFingerprint combinedFingerprint)
        {
            if (topologyCacheSessionPersistentInputCaptured)
            {
                return;
            }

            topologyCacheSessionPersistentInputKey =
                combinedFingerprint.ToString();
            topologyCacheSessionPersistentInputCaptured = true;
        }

        private void SetTopologyCacheStartupMiss(
            string state,
            string summary)
        {
            topologyCacheStartupMissCount++;
            topologyCacheStartupState = state ?? "Miss";
            topologyCacheStartupSummary = summary ?? string.Empty;
            if (IsAutomaticDevelopmentCacheEnabled)
            {
                topologyCacheStartupSummary +=
                    " Development startup will generate the complete topology " +
                    "automatically; the Editor will persist it when the inputs " +
                    "still match the values present at Play entry.";
            }
            else
            {
                topologyCacheStartupSummary +=
                    " Production startup remains neutral and never runs the " +
                    "expensive generator path automatically.";
            }

            topologyCacheStartupPayloadBytes = 0;
            topologyCacheStartupPayloadHash = "—";
            topologyCacheStartupLoadMilliseconds = 0.0;
        }

        private bool InstallStartupTopologyCache(
            StylizedRiverFoamTopologyCachePackage package,
            bool staleObstacles,
            bool staleSettings)
        {
            System.Diagnostics.Stopwatch stopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            if (package == null || package.FieldWidth != fieldWidth ||
                package.FieldHeight != fieldHeight ||
                package.ObstacleExclusion == null ||
                package.ObstacleExclusion.Length != fieldWidth * fieldHeight ||
                package.MajorTopology == null ||
                package.ConnectorTopology == null ||
                package.PocketTopology == null)
            {
                SetTopologyCacheStartupMiss(
                    "Miss — Install Rejected",
                    "The resolved payload became incomplete before installation.");
                return false;
            }

            ReleaseObstacleExclusionBuffers();
            obstacleExclusionMeshFilters.Clear();
            topologyCacheFingerprintMeshFilters.Clear();
            topologyCachePreparedObstacleFingerprints.Clear();
            obstacleExclusionCells.Clear();
            obstacleExclusionSamples.Clear();
            obstacleExclusionGpuCells =
                Array.Empty<FoamObstacleIntervalCellData>();
            obstacleExclusionScalar = package.ObstacleExclusion;
            obstacleGeometryVersion = ResolveCurrentObstacleGeometryVersion();
            obstacleExclusionUsesCachedScalar = true;
            activeTopologyObstacleStale = staleObstacles;
            UploadCachedObstacleExclusionScalar();

            majorTopology = package.MajorTopology;
            connectorTopology = package.ConnectorTopology;
            pocketTopology = package.PocketTopology;
            if (staleObstacles || staleSettings)
            {
                ApplyCachedTopologyInputSignatures(
                    package,
                    staleObstacles);
            }
            else
            {
                majorTopologyInputSignature =
                    ResolveMajorTopologyInputSignature();
                connectorTopologyInputSignature =
                    ResolveConnectorTopologyInputSignature();
                pocketTopologyInputSignature =
                    ResolvePocketTopologyInputSignature();
            }

            InitializeMajorEvolution();
            InitializeConnectorIdentityReconstruction(false);
            UploadGeneratedTopology();
            BuildEvolvingMajorField();
            topologyCacheLoadedForActiveResources = true;
            stopwatch.Stop();
            topologyCacheStartupLoadMilliseconds +=
                stopwatch.Elapsed.TotalMilliseconds;

            pendingStartupCacheStaleObstacles = staleObstacles;
            pendingStartupCacheStaleSettings = staleSettings;
            pendingAutomaticDevelopmentRebuildAfterStartup =
                IsAutomaticDevelopmentCacheEnabled &&
                (staleObstacles || staleSettings);

            if (pendingAutomaticDevelopmentRebuildAfterStartup)
            {
                topologyCacheStartupState =
                    "Using Previous Cache — Rebuilding";
                topologyCacheStartupSummary = staleObstacles
                    ? "The previous generated topology was installed. The live " +
                      "obstacle field and a complete replacement will now be " +
                      "prepared automatically without a neutral frame."
                    : "The previous generated topology was installed and will " +
                      "remain visible while current settings prepare a complete " +
                      "replacement automatically.";
                Debug.Log(
                    $"[River Foam 4.9C.1] Retained stale cache for " +
                    $"'{river.name}' while automatic regeneration starts.",
                    river);
            }
            else
            {
                topologyCacheStartupState = "Loaded";
                topologyCacheStartupSummary =
                    "Persistent topology and the exact cached obstacle scalar " +
                    "were installed. Runtime shore, obstacle, pressure, lee, " +
                    "wake, and other live sources remain composed normally.";
                Debug.Log(
                    $"[River Foam 4.9C] Loaded cache for '{river.name}': " +
                    $"{topologyCacheStartupPayloadBytes:N0} bytes, hash " +
                    $"{topologyCacheStartupPayloadHash}.",
                    river);
            }

            return true;
        }

        private void ApplyCachedTopologyInputSignatures(
            StylizedRiverFoamTopologyCachePackage package,
            bool staleObstacles)
        {
            int signatureObstacleVersion = staleObstacles
                ? int.MinValue + 49
                : obstacleGeometryVersion;
            majorTopologyInputSignature =
                ResolveMajorTopologyInputSignature(
                    river.Domain,
                    signatureObstacleVersion,
                    package.Quality,
                    package.FieldWidth,
                    package.FieldHeight,
                    package.MajorSeed,
                    package.MajorAmount,
                    package.MajorSize,
                    package.MajorSizeVariation,
                    package.MajorRecycleTerritoryDeviationPercent,
                    package.ShoreMotion);
            connectorTopologyInputSignature =
                ResolveConnectorTopologyInputSignature(
                    majorTopologyInputSignature,
                    package.ConnectorAmount,
                    package.ConnectorDirectness,
                    package.ConnectorLengthPreference);
            pocketTopologyInputSignature =
                ResolvePocketTopologyInputSignature(
                    majorTopologyInputSignature,
                    connectorTopologyInputSignature,
                    package.InteriorPocketAmount,
                    package.EdgeCavityAmount,
                    package.ConnectorWeakSpanAmount,
                    package.FreeWaterEventAmount);
        }

        private void UploadCachedObstacleExclusionScalar()
        {
            if (obstacleExclusionTexture == null ||
                obstacleExclusionScalar.Length != fieldWidth * fieldHeight)
            {
                return;
            }

            if (obstacleExclusionUploadTexture == null ||
                obstacleExclusionUploadTexture.width != fieldWidth ||
                obstacleExclusionUploadTexture.height != fieldHeight)
            {
                if (obstacleExclusionUploadTexture != null)
                {
                    DestroyUnityObject(obstacleExclusionUploadTexture);
                }

                obstacleExclusionUploadTexture = new Texture2D(
                    fieldWidth,
                    fieldHeight,
                    TextureFormat.RFloat,
                    false,
                    true)
                {
                    name = "T_PS3D_RiverFoamObstacleExclusion_Upload",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.DontSave
                };
            }

            obstacleExclusionUploadTexture.SetPixelData(
                obstacleExclusionScalar,
                0);
            obstacleExclusionUploadTexture.Apply(false, false);
            Graphics.Blit(
                obstacleExclusionUploadTexture,
                obstacleExclusionTexture);
        }

        private StylizedRiverFoamTopologyCachePackage
            CreateTopologyCachePackage(
                StylizedRiverFoamTopologyFingerprint domainFingerprint,
                StylizedRiverFoamTopologyFingerprint obstacleFingerprint,
                StylizedRiverFoamTopologyFingerprint generationFingerprint)
        {
            RiverDomainSnapshot domain = river.Domain;
            return new StylizedRiverFoamTopologyCachePackage(
                fieldWidth,
                fieldHeight,
                fieldLength,
                validFieldLength,
                domain.GlobalDistanceMinimum,
                domain.GlobalDistanceMaximum,
                domain.LocalLength,
                domain.RequestedSampleSpacing,
                domain.ReverseFlow,
                domain.SampleCount,
                domainFingerprint,
                obstacleFingerprint,
                generationFingerprint,
                river.Quality,
                river.ShoreMotion,
                river.FoamMajorSupportSeed,
                river.FoamMajorSupportAmount,
                river.FoamMajorSupportSize,
                river.FoamMajorSupportSizeVariation,
                river.FoamMajorRecycleTerritoryDeviationPercent,
                river.FoamConnectorAmount,
                river.FoamConnectorDirectness,
                river.FoamConnectorLengthPreference,
                river.FoamInteriorPocketAmount,
                river.FoamEdgeCavityAmount,
                river.FoamConnectorWeakSpanAmount,
                river.FoamFreeWaterEventAmount,
                obstacleExclusionScalar,
                majorTopology,
                connectorTopology,
                pocketTopology);
        }

        private void SetTopologyCacheValidationMiss(
            string state,
            string summary)
        {
            topologyCacheValidationState = state ?? "Miss";
            topologyCacheValidationSummary = summary ?? string.Empty;
        }

        private void RequestTopologyReplacement(
            TopologyReplacementReason reason,
            bool identicalValidation)
        {
            if (river == null || !river.Domain.IsValid ||
                initializationPhase != InitializationPhase.Ready ||
                resourcesDirty || domainVersion != river.Domain.Version ||
                allocatedQuality != river.Quality ||
                fieldWidth < 2 || fieldHeight < 2)
            {
                return;
            }

            ResolveRequestedTopologySignatures(
                out int majorSignature,
                out int connectorSignature,
                out int pocketSignature,
                out int targetSignature);
            int activeSignature = ResolveActiveTopologySignature();
            if (!identicalValidation && targetSignature == activeSignature)
            {
                return;
            }

            if (topologyReplacementBuild != null)
            {
                bool sameRequest =
                    topologyReplacementBuild.TargetSignature ==
                        targetSignature &&
                    topologyReplacementBuild.IsIdenticalValidation ==
                        identicalValidation;
                if (sameRequest)
                {
                    return;
                }

                CancelTopologyReplacementBuild(true);
            }

            float[] obstacleSnapshot = obstacleExclusionScalar.Length ==
                fieldWidth * fieldHeight
                    ? (float[])obstacleExclusionScalar.Clone()
                    : new float[fieldWidth * fieldHeight];

            topologyReplacementBuild = new TopologyReplacementBuild
            {
                RequestId = ++topologyReplacementRequestSequence,
                TargetSignature = targetSignature,
                MajorInputSignature = majorSignature,
                ConnectorInputSignature = connectorSignature,
                PocketInputSignature = pocketSignature,
                Reason = reason,
                IsIdenticalValidation = identicalValidation,
                Domain = river.Domain,
                Quality = river.Quality,
                FieldWidth = fieldWidth,
                FieldHeight = fieldHeight,
                FieldLength = fieldLength,
                ValidFieldLength = validFieldLength,
                ShoreMotion = river.ShoreMotion,
                MajorAmount = river.FoamMajorSupportAmount,
                MajorSize = river.FoamMajorSupportSize,
                MajorSizeVariation = river.FoamMajorSupportSizeVariation,
                MajorRecycleTerritoryDeviationPercent =
                    river.FoamMajorRecycleTerritoryDeviationPercent,
                MajorSeed = river.FoamMajorSupportSeed,
                ConnectorAmount = river.FoamConnectorAmount,
                ConnectorDirectness = river.FoamConnectorDirectness,
                ConnectorLengthPreference =
                    river.FoamConnectorLengthPreference,
                InteriorPocketAmount = river.FoamInteriorPocketAmount,
                EdgeCavityAmount = river.FoamEdgeCavityAmount,
                ConnectorWeakSpanAmount =
                    river.FoamConnectorWeakSpanAmount,
                FreeWaterEventAmount = river.FoamFreeWaterEventAmount,
                ObstacleExclusion = obstacleSnapshot
            };
            topologyReplacementPhase =
                TopologyReplacementPhase.BuildMajorTopology;
            topologyReplacementRequestCount++;
            topologyReplacementLastReason =
                FormatTopologyReplacementReason(reason);
        }

        private bool AdvanceTopologyReplacementBuild()
        {
            TopologyReplacementBuild build = topologyReplacementBuild;
            if (build == null ||
                topologyReplacementPhase == TopologyReplacementPhase.Idle)
            {
                return false;
            }

            if (river == null || !river.Domain.IsValid || resourcesDirty ||
                domainVersion != river.Domain.Version ||
                allocatedQuality != river.Quality ||
                build.FieldWidth != fieldWidth ||
                build.FieldHeight != fieldHeight)
            {
                CancelTopologyReplacementBuild(false);
                return false;
            }

            int currentTargetSignature = ResolveRequestedTopologySignature();
            if (build.IsIdenticalValidation)
            {
                if (currentTargetSignature != ResolveActiveTopologySignature())
                {
                    CancelTopologyReplacementBuild(true);
                    RequestTopologyReplacement(
                        TopologyReplacementReason.Settings,
                        false);
                    return false;
                }
            }
            else if (currentTargetSignature != build.TargetSignature)
            {
                CancelTopologyReplacementBuild(true);
                RequestTopologyReplacement(
                    TopologyReplacementReason.Settings,
                    false);
                return false;
            }

            switch (topologyReplacementPhase)
            {
                case TopologyReplacementPhase.BuildMajorTopology:
                    using (ReplacementBuildMajorProfilerMarker.Auto())
                    {
                        build.MajorTopology =
                            StylizedRiverFoamMajorTopologyGenerator.Generate(
                                build.Domain,
                                build.FieldWidth,
                                build.FieldHeight,
                                build.FieldLength,
                                build.ValidFieldLength,
                                build.Quality,
                                build.ShoreMotion,
                                build.MajorAmount,
                                build.MajorSize,
                                build.MajorSizeVariation,
                                build.MajorRecycleTerritoryDeviationPercent,
                                build.MajorSeed,
                                build.ObstacleExclusion);
                    }
                    topologyReplacementPhase =
                        TopologyReplacementPhase.BuildConnectorTopology;
                    break;

                case TopologyReplacementPhase.BuildConnectorTopology:
                    using (ReplacementBuildConnectorProfilerMarker.Auto())
                    {
                        build.ConnectorTopology =
                            StylizedRiverFoamConnectorTopologyGenerator.Generate(
                                build.Domain,
                                build.FieldWidth,
                                build.FieldHeight,
                                build.FieldLength,
                                build.ValidFieldLength,
                                build.Quality,
                                build.ShoreMotion,
                                build.MajorSeed,
                                build.ConnectorAmount,
                                build.ConnectorDirectness,
                                build.ConnectorLengthPreference,
                                build.ObstacleExclusion,
                                build.MajorTopology);
                    }
                    topologyReplacementPhase =
                        TopologyReplacementPhase.BuildPocketTopology;
                    break;

                case TopologyReplacementPhase.BuildPocketTopology:
                    using (ReplacementBuildPocketProfilerMarker.Auto())
                    {
                        build.PocketTopology =
                            StylizedRiverFoamPocketTopologyGenerator.Generate(
                                build.Domain,
                                build.FieldWidth,
                                build.FieldHeight,
                                build.FieldLength,
                                build.ValidFieldLength,
                                build.Quality,
                                build.ShoreMotion,
                                build.MajorSeed,
                                build.InteriorPocketAmount,
                                build.EdgeCavityAmount,
                                build.ConnectorWeakSpanAmount,
                                build.FreeWaterEventAmount,
                                build.ObstacleExclusion,
                                build.MajorTopology,
                                build.ConnectorTopology);
                    }
                    topologyReplacementPhase =
                        TopologyReplacementPhase.PrepareGeneratedTexture;
                    break;

                case TopologyReplacementPhase.PrepareGeneratedTexture:
                    using (ReplacementPrepareTextureProfilerMarker.Auto())
                    {
                        PrepareTopologyReplacementGeneratedTexture(build);
                    }
                    topologyReplacementPhase =
                        TopologyReplacementPhase.ReadyToActivate;
                    break;

                case TopologyReplacementPhase.ReadyToActivate:
                    if (build.IsIdenticalValidation)
                    {
                        topologyReplacementIdenticalPreparedCount++;
                        topologyReplacementLastReason =
                            FormatTopologyReplacementReason(build.Reason);
                        ReleaseTopologyReplacementBuild(true);
                        return false;
                    }
                    using (ReplacementActivateProfilerMarker.Auto())
                    {
                        return ActivateTopologyReplacement(build);
                    }
            }

            return false;
        }

        private void PrepareTopologyReplacementGeneratedTexture(
            TopologyReplacementBuild build)
        {
            int cellCount = build.FieldWidth * build.FieldHeight;
            Color[] pixels = new Color[cellCount];
            build.MajorTopology?.FillUploadPixels(pixels);
            build.ConnectorTopology?.AddToUploadPixels(pixels, false);
            build.PocketTopology?.AddToUploadPixels(
                pixels,
                false,
                false,
                false);

            build.GeneratedTexture = CreateGuidanceTexture(
                $"PS3D_RiverFoam_TopologyGenerated_Replacement_{build.RequestId}");
            Texture2D upload = new Texture2D(
                build.FieldWidth,
                build.FieldHeight,
                TextureFormat.RGBAHalf,
                false,
                true)
            {
                name =
                    $"T_PS3D_RiverFoamTopologyReplacement_{build.RequestId}",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            upload.SetPixels(pixels);
            upload.Apply(false, false);
            Graphics.Blit(upload, build.GeneratedTexture);
            DestroyUnityObject(upload);
        }

        private bool ActivateTopologyReplacement(
            TopologyReplacementBuild build)
        {
            if (build == null || build.MajorTopology == null ||
                build.ConnectorTopology == null ||
                build.PocketTopology == null ||
                build.GeneratedTexture == null ||
                !build.GeneratedTexture.IsCreated())
            {
                CancelTopologyReplacementBuild(false);
                return false;
            }

            CaptureActiveGeneratedTopologyTransition(false);

            RenderTexture retiredGeneratedTexture = topologyGeneratedTexture;
            topologyGeneratedTexture = build.GeneratedTexture;
            build.GeneratedTexture = null;
            majorTopology = build.MajorTopology;
            connectorTopology = build.ConnectorTopology;
            pocketTopology = build.PocketTopology;
            majorTopologyInputSignature = build.MajorInputSignature;
            connectorTopologyInputSignature = build.ConnectorInputSignature;
            pocketTopologyInputSignature = build.PocketInputSignature;

            InitializeMajorEvolution();
            InitializeConnectorIdentityReconstruction(false);
            UploadGeneratedTopology();
            BuildEvolvingMajorField();
            RefreshDynamicTopologySources(true);

            RetireGeneratedTopologyTexture(retiredGeneratedTexture);
            topologyReplacementActivatedCount++;
            topologyReplacementLastReason =
                FormatTopologyReplacementReason(build.Reason);
            ReleaseTopologyReplacementBuild(false);
            activeTopologyObstacleStale = false;
            if (DevelopmentTopologyGenerationInProgress)
            {
                CompleteDevelopmentTopologyGeneration(
                    "an automatic in-session replacement");
            }
            return true;
        }

        private void CancelTopologyReplacementBuild(bool coalesced)
        {
            if (topologyReplacementBuild == null)
            {
                return;
            }

            topologyReplacementCancelledCount++;
            if (coalesced)
            {
                topologyReplacementCoalescedCount++;
            }
            ReleaseTopologyReplacementBuild(true);
        }

        private void ReleaseTopologyReplacementBuild(bool releaseTexture)
        {
            if (topologyReplacementBuild != null && releaseTexture &&
                topologyReplacementBuild.GeneratedTexture != null)
            {
                RenderTexture texture =
                    topologyReplacementBuild.GeneratedTexture;
                ReleaseTexture(ref texture);
                topologyReplacementBuild.GeneratedTexture = null;
            }

            topologyReplacementBuild = null;
            topologyReplacementPhase = TopologyReplacementPhase.Idle;
        }

        private static string FormatTopologyReplacementReason(
            TopologyReplacementReason reason)
        {
            return reason == TopologyReplacementReason.None
                ? "None"
                : reason.ToString().Replace(",", " +");
        }

        private bool HasTopologyTransitionVisibleHold =>
            topologyTransitionSnapshot != null &&
            topologyTransitionSnapshot.HoldsVisibleResources;

        private void PrepareDimensionChangingTopologyTransition()
        {
            if (HasTopologyTransitionVisibleHold ||
                !CanCaptureActiveGeneratedTopology())
            {
                return;
            }

            if (CaptureActiveGeneratedTopologyTransition(true))
            {
                topologyTransitionRemappedCount++;
            }
        }

        private bool CanCaptureActiveGeneratedTopology()
        {
            return computeShader != null &&
                captureGeneratedTopologyKernel >= 0 &&
                fieldWidth > 0 && fieldHeight > 0 &&
                guidanceWidth > 0 && guidanceHeight > 0 &&
                metricBuffer != null &&
                metricRows.Length == fieldWidth &&
                topologyGeneratedTexture != null &&
                topologyGeneratedTexture.IsCreated() &&
                evolvingMajorTexture != null &&
                evolvingHostedNegativeTexture != null &&
                evolvingFreeWaterNegativeTexture != null &&
                evolvingConnectorTexture != null &&
                evolvingWeakSpanNegativeTexture != null;
        }

        private bool CaptureActiveGeneratedTopologyTransition(
            bool holdVisibleResources)
        {
            if (!CanCaptureActiveGeneratedTopology())
            {
                return false;
            }

            using var profilerScope =
                TopologyTransitionCaptureProfilerMarker.Auto();
            RenderTexture capture = CreateTopologyTexture(
                guidanceWidth,
                guidanceHeight,
                $"PS3D_RiverFoam_TopologyTransition_{Time.frameCount}");
            ComputeBuffer capturedMetricBuffer = new ComputeBuffer(
                fieldWidth,
                Marshal.SizeOf<FoamMetricRow>(),
                ComputeBufferType.Structured);
            capturedMetricBuffer.SetData(metricRows);

            ConfigureTopologyParameters(0f);
            computeShader.SetFloat("_FoamGlobalStart", allocatedGlobalStart);
            BindGeneratedTopologyInputs(captureGeneratedTopologyKernel);
            ConfigureTopologyTransitionInputs(
                captureGeneratedTopologyKernel);
            computeShader.SetBuffer(
                captureGeneratedTopologyKernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetTexture(
                captureGeneratedTopologyKernel,
                "_FoamTopologyTransitionCaptureWrite",
                capture);
            Dispatch(
                captureGeneratedTopologyKernel,
                guidanceWidth,
                guidanceHeight);

            TopologyTransitionSnapshot previousSnapshot =
                topologyTransitionSnapshot;
            topologyTransitionSnapshot = new TopologyTransitionSnapshot
            {
                GeneratedTexture = capture,
                MetricBuffer = capturedMetricBuffer,
                Width = guidanceWidth,
                Height = guidanceHeight,
                DomainVersion = domainVersion,
                GlobalStart = allocatedGlobalStart,
                FieldLength = fieldLength,
                ValidFieldLength = validFieldLength
            };
            topologyTransitionElapsed = 0f;
            topologyTransitionStartedCount++;

            if (previousSnapshot != null)
            {
                topologyTransitionFlattenedCount++;
                RetireTopologyTransitionSnapshot(previousSnapshot);
            }

            if (holdVisibleResources)
            {
                DetachVisibleResourcesToTopologyTransition(
                    topologyTransitionSnapshot);
            }

            return true;
        }

        private void BindGeneratedTopologyInputs(int kernel)
        {
            computeShader.SetFloat(
                "_FoamMajorEvolutionEnabled",
                majorEvolutionReady ? 1f : 0f);
            computeShader.SetFloat(
                "_FoamHostedNegativeEvolutionEnabled",
                hostedNegativeEvolutionReady ? 1f : 0f);
            computeShader.SetFloat(
                "_FoamFreeWaterNegativeEvolutionEnabled",
                freeWaterEvolutionReady ? 1f : 0f);
            computeShader.SetFloat(
                "_FoamConnectorIdentityReconstructionEnabled",
                connectorIdentityReconstructionReady ? 1f : 0f);
            computeShader.SetFloat(
                "_FoamWeakSpanIdentityReconstructionEnabled",
                weakSpanIdentityReconstructionReady ? 1f : 0f);
            computeShader.SetTexture(
                kernel,
                "_FoamTopologyGeneratedRead",
                topologyGeneratedTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamEvolvingMajorRead",
                evolvingMajorTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamEvolvingHostedNegativeRead",
                evolvingHostedNegativeTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamEvolvingFreeWaterNegativeRead",
                evolvingFreeWaterNegativeTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamEvolvingConnectorRead",
                evolvingConnectorTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamEvolvingWeakSpanNegativeRead",
                evolvingWeakSpanNegativeTexture);
        }

        private void ConfigureTopologyTransitionInputs(int kernel)
        {
            TopologyTransitionSnapshot snapshot = topologyTransitionSnapshot;
            bool enabled = snapshot != null &&
                !snapshot.HoldsVisibleResources &&
                snapshot.GeneratedTexture != null &&
                snapshot.GeneratedTexture.IsCreated() &&
                snapshot.MetricBuffer != null;
            if (!enabled)
            {
                computeShader.SetFloat(
                    "_FoamTopologyTransitionEnabled",
                    0f);
                computeShader.SetFloat(
                    "_FoamTopologyTransitionBlend",
                    1f);
                computeShader.SetFloat(
                    "_FoamTopologyTransitionSameMapping",
                    1f);
                computeShader.SetInts(
                    "_FoamTopologyTransitionDimensions",
                    guidanceWidth,
                    guidanceHeight);
                computeShader.SetFloat(
                    "_FoamTopologyTransitionGlobalStart",
                    allocatedGlobalStart);
                computeShader.SetFloat(
                    "_FoamTopologyTransitionFieldLength",
                    fieldLength);
                computeShader.SetFloat(
                    "_FoamTopologyTransitionValidLength",
                    validFieldLength);
                computeShader.SetBuffer(
                    kernel,
                    "_FoamTopologyTransitionMetricRows",
                    metricBuffer);
                computeShader.SetTexture(
                    kernel,
                    "_FoamTopologyTransitionFromRead",
                    topologyGeneratedTexture);
                return;
            }

            bool sameMapping = snapshot.DomainVersion == domainVersion &&
                snapshot.Width == guidanceWidth &&
                snapshot.Height == guidanceHeight &&
                Mathf.Abs(snapshot.GlobalStart - allocatedGlobalStart) <
                    0.0001f &&
                Mathf.Abs(snapshot.FieldLength - fieldLength) < 0.0001f &&
                Mathf.Abs(snapshot.ValidFieldLength - validFieldLength) <
                    0.0001f;
            computeShader.SetFloat(
                "_FoamTopologyTransitionEnabled",
                1f);
            computeShader.SetFloat(
                "_FoamTopologyTransitionBlend",
                TopologyTransitionProgress);
            computeShader.SetFloat(
                "_FoamTopologyTransitionSameMapping",
                sameMapping ? 1f : 0f);
            computeShader.SetInts(
                "_FoamTopologyTransitionDimensions",
                snapshot.Width,
                snapshot.Height);
            computeShader.SetFloat(
                "_FoamTopologyTransitionGlobalStart",
                snapshot.GlobalStart);
            computeShader.SetFloat(
                "_FoamTopologyTransitionFieldLength",
                snapshot.FieldLength);
            computeShader.SetFloat(
                "_FoamTopologyTransitionValidLength",
                snapshot.ValidFieldLength);
            computeShader.SetBuffer(
                kernel,
                "_FoamTopologyTransitionMetricRows",
                snapshot.MetricBuffer);
            computeShader.SetTexture(
                kernel,
                "_FoamTopologyTransitionFromRead",
                snapshot.GeneratedTexture);
        }

        private bool AdvanceTopologyTransition(float deltaTime)
        {
            if (topologyTransitionSnapshot == null ||
                topologyTransitionSnapshot.HoldsVisibleResources ||
                initializationPhase != InitializationPhase.Ready)
            {
                return false;
            }

            float previousProgress = TopologyTransitionProgress;
            topologyTransitionElapsed = Mathf.Min(
                TopologyReplacementTransitionSeconds,
                topologyTransitionElapsed + Mathf.Max(0f, deltaTime));
            if (topologyTransitionElapsed >=
                TopologyReplacementTransitionSeconds - 0.0001f)
            {
                TopologyTransitionSnapshot completed =
                    topologyTransitionSnapshot;
                topologyTransitionSnapshot = null;
                topologyTransitionElapsed = 0f;
                topologyTransitionCompletedCount++;
                RetireTopologyTransitionSnapshot(completed);
                return true;
            }

            return TopologyTransitionProgress > previousProgress + 0.000001f;
        }

        private void DetachVisibleResourcesToTopologyTransition(
            TopologyTransitionSnapshot snapshot)
        {
            if (snapshot == null || currentState == null ||
                previousState == null || topologyTexture == null ||
                topologySourcesTexture == null)
            {
                return;
            }

            snapshot.PreviousState = previousState;
            snapshot.CurrentState = currentState;
            snapshot.Guidance = guidanceTexture;
            snapshot.Topology = topologyTexture;
            snapshot.TopologySources = topologySourcesTexture;
            snapshot.Fracture = currentFracture;
            snapshot.ObstacleExclusion = obstacleExclusionTexture;
            snapshot.Boundary = boundaryTexture;
            snapshot.Interpolation = simulationInterpolation;
            snapshot.HoldsVisibleResources = true;

            if (stateA == previousState || stateA == currentState)
            {
                stateA = null;
            }
            if (stateB == previousState || stateB == currentState)
            {
                stateB = null;
            }
            previousState = null;
            currentState = null;
            writeState = null;
            guidanceTexture = null;
            topologyTexture = null;
            topologySourcesTexture = null;
            if (fractureA == snapshot.Fracture)
            {
                fractureA = null;
            }
            if (fractureB == snapshot.Fracture)
            {
                fractureB = null;
            }
            currentFracture = null;
            obstacleExclusionTexture = null;
            boundaryTexture = null;
        }

        private void ReleaseTopologyTransitionVisibleHold()
        {
            TopologyTransitionSnapshot snapshot = topologyTransitionSnapshot;
            if (snapshot == null || !snapshot.HoldsVisibleResources)
            {
                return;
            }

            ReleaseHeldTopologyTransitionTextures(snapshot);
            snapshot.HoldsVisibleResources = false;
        }

        private void RetireTopologyTransitionSnapshot(
            TopologyTransitionSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            ReleaseHeldTopologyTransitionTextures(snapshot);
            ReleaseRetiredTopologyTransitionResourcesNow();
            retiredTopologyTransitionTexture = snapshot.GeneratedTexture;
            retiredTopologyTransitionMetricBuffer = snapshot.MetricBuffer;
            retiredTopologyTransitionReleaseFrame = Time.frameCount + 2;
            snapshot.GeneratedTexture = null;
            snapshot.MetricBuffer = null;
        }

        private void ReleaseHeldTopologyTransitionTextures(
            TopologyTransitionSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            RenderTexture previous = snapshot.PreviousState;
            RenderTexture current = snapshot.CurrentState;
            if (current == previous)
            {
                current = null;
            }
            ReleaseTexture(ref previous);
            ReleaseTexture(ref current);
            snapshot.PreviousState = null;
            snapshot.CurrentState = null;

            RenderTexture guidance = snapshot.Guidance;
            RenderTexture topology = snapshot.Topology;
            RenderTexture sources = snapshot.TopologySources;
            RenderTexture fracture = snapshot.Fracture;
            RenderTexture obstacle = snapshot.ObstacleExclusion;
            ReleaseTexture(ref guidance);
            ReleaseTexture(ref topology);
            ReleaseTexture(ref sources);
            ReleaseTexture(ref fracture);
            ReleaseTexture(ref obstacle);
            snapshot.Guidance = null;
            snapshot.Topology = null;
            snapshot.TopologySources = null;
            snapshot.Fracture = null;
            snapshot.ObstacleExclusion = null;

            if (snapshot.Boundary != null)
            {
                DestroyUnityObject(snapshot.Boundary);
                snapshot.Boundary = null;
            }
        }

        private void ReleaseRetiredTopologyTransitionResourcesIfReady()
        {
            if (retiredTopologyTransitionReleaseFrame < 0 ||
                Time.frameCount < retiredTopologyTransitionReleaseFrame)
            {
                return;
            }

            ReleaseRetiredTopologyTransitionResourcesNow();
        }

        private void ReleaseRetiredTopologyTransitionResourcesNow()
        {
            ReleaseTexture(ref retiredTopologyTransitionTexture);
            retiredTopologyTransitionMetricBuffer?.Release();
            retiredTopologyTransitionMetricBuffer = null;
            retiredTopologyTransitionReleaseFrame = -1;
        }


        private void RetireGeneratedTopologyTexture(RenderTexture texture)
        {
            if (texture == null)
            {
                return;
            }

            ReleaseRetiredGeneratedTopologyTextureNow();
            retiredGeneratedTopologyTexture = texture;
            retiredGeneratedTopologyReleaseFrame = Time.frameCount + 2;
        }

        private void ReleaseRetiredGeneratedTopologyTextureIfReady()
        {
            if (retiredGeneratedTopologyReleaseFrame < 0 ||
                Time.frameCount < retiredGeneratedTopologyReleaseFrame)
            {
                return;
            }

            ReleaseRetiredGeneratedTopologyTextureNow();
        }

        private void ReleaseRetiredGeneratedTopologyTextureNow()
        {
            ReleaseTexture(ref retiredGeneratedTopologyTexture);
            retiredGeneratedTopologyReleaseFrame = -1;
        }

        private void ReleaseTopologyTransition(bool releaseVisibleResources)
        {
            if (topologyTransitionSnapshot != null)
            {
                if (releaseVisibleResources)
                {
                    ReleaseHeldTopologyTransitionTextures(
                        topologyTransitionSnapshot);
                }
                RenderTexture generated =
                    topologyTransitionSnapshot.GeneratedTexture;
                ReleaseTexture(ref generated);
                topologyTransitionSnapshot.GeneratedTexture = null;
                topologyTransitionSnapshot.MetricBuffer?.Release();
                topologyTransitionSnapshot.MetricBuffer = null;
                topologyTransitionSnapshot = null;
            }

            topologyTransitionElapsed = 0f;
            ReleaseRetiredTopologyTransitionResourcesNow();
            ReleaseRetiredGeneratedTopologyTextureNow();
        }

        private long EstimateTopologyTransitionBytes()
        {
            TopologyTransitionSnapshot snapshot = topologyTransitionSnapshot;
            if (snapshot == null)
            {
                return 0L;
            }

            return EstimateTextureBytes(snapshot.GeneratedTexture) +
                EstimateTextureBytes(snapshot.PreviousState) +
                EstimateTextureBytes(snapshot.CurrentState) +
                EstimateTextureBytes(snapshot.Guidance) +
                EstimateTextureBytes(snapshot.Topology) +
                EstimateTextureBytes(snapshot.TopologySources) +
                EstimateTextureBytes(snapshot.Fracture) +
                EstimateTextureBytes(snapshot.ObstacleExclusion) +
                EstimateTextureBytes(snapshot.Boundary);
        }

        private bool AdvanceQueuedRebuild()
        {
            PromotePendingRebuildPhase();
            if (rebuildPhase == RebuildPhase.Idle)
            {
                return false;
            }

            switch (rebuildPhase)
            {
                case RebuildPhase.BuildBoundary:
                    using (RebuildBuildBoundaryProfilerMarker.Auto())
                    {
                        RebuildBoundaryTexture(false);
                    }

                    pendingBoundaryRebuild = false;
                    rebuildPhase = RebuildPhase.ApplyBoundary;
                    break;

                case RebuildPhase.ApplyBoundary:
                    using (RebuildApplyBoundaryProfilerMarker.Auto())
                    {
                        ApplyBoundaryToState(stateA);
                        ApplyBoundaryToState(stateB);
                    }

                    if (!pendingObstacleRebuild &&
                        pendingTopologyReplacementAfterMaintenance)
                    {
                        RequestTopologyReplacement(
                            TopologyReplacementReason.Boundary,
                            false);
                        pendingTopologyReplacementAfterMaintenance = false;
                    }

                    rebuildPhase = pendingObstacleRebuild
                        ? RebuildPhase.WaitForObstacleStability
                        : RebuildPhase.RefreshTopologySources;
                    break;

                case RebuildPhase.WaitForObstacleStability:
                    using (RebuildWaitObstacleProfilerMarker.Auto())
                    {
                        disturbanceRuntime ??=
                            GetComponent<StylizedRiverDisturbanceRuntime>();
                        if (disturbanceRuntime != null &&
                            !disturbanceRuntime.GeneratedObstacleRegistryReady)
                        {
                            pendingObstacleObservedVersion = int.MinValue;
                            pendingObstacleStableFrameCount = 0;
                            break;
                        }

                        int currentObstacleVersion = disturbanceRuntime != null
                            ? disturbanceRuntime.ObstacleGeometryVersion
                            : -1;

                        if (currentObstacleVersion !=
                            pendingObstacleObservedVersion)
                        {
                            pendingObstacleObservedVersion =
                                currentObstacleVersion;
                            pendingObstacleStableFrameCount = 0;
                        }
                        else
                        {
                            pendingObstacleStableFrameCount++;
                            if (pendingObstacleStableFrameCount >=
                                ObstacleRebuildStableFrameCount)
                            {
                                rebuildPhase =
                                    RebuildPhase.BuildObstacleExclusion;
                            }
                        }
                    }
                    break;

                case RebuildPhase.BuildObstacleExclusion:
                {
                    bool prepareTopologyReplacement =
                        pendingTopologyReplacementAfterMaintenance;
                    using (RebuildBuildObstacleProfilerMarker.Auto())
                    {
                        RebuildObstacleExclusionCache(
                            prepareTopologyReplacement);
                    }

                    pendingObstacleRebuild = false;
                    pendingObstacleObservedVersion = int.MinValue;
                    pendingObstacleStableFrameCount = 0;
                    if (prepareTopologyReplacement)
                    {
                        RequestTopologyReplacement(
                            TopologyReplacementReason.Obstacle,
                            false);
                        pendingTopologyReplacementAfterMaintenance = false;
                    }
                    rebuildPhase = RebuildPhase.RefreshTopologySources;
                    break;
                }

                case RebuildPhase.RefreshTopologySources:
                    using (RebuildRefreshSourcesProfilerMarker.Auto())
                    {
                        RefreshDynamicTopologySources(true);
                    }

                    pendingTopologyRefresh = false;
                    rebuildPhase = RebuildPhase.Idle;
                    break;
            }

            return true;
        }

        private void PromotePendingRebuildPhase()
        {
            if (pendingBoundaryRebuild &&
                (rebuildPhase == RebuildPhase.Idle ||
                 (int)rebuildPhase > (int)RebuildPhase.BuildBoundary))
            {
                rebuildPhase = RebuildPhase.BuildBoundary;
                return;
            }

            if (pendingObstacleRebuild &&
                (rebuildPhase == RebuildPhase.Idle ||
                 (int)rebuildPhase >
                    (int)RebuildPhase.BuildObstacleExclusion))
            {
                rebuildPhase = RebuildPhase.WaitForObstacleStability;
                return;
            }

            if (pendingTopologyRefresh &&
                rebuildPhase == RebuildPhase.Idle)
            {
                rebuildPhase = RebuildPhase.RefreshTopologySources;
            }
        }

        private bool AreResourcesCompleteAndCurrent()
        {
            return !resourcesDirty &&
                currentState != null &&
                advectedState != null &&
                reverseState != null &&
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
                currentFracture != null &&
                writeFracture != null &&
                neutralDisturbanceTexture != null &&
                neutralDisturbanceTexture.IsCreated() &&
                boundaryTexture != null &&
                metricBuffer != null &&
                populationMetricsBuffer != null &&
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
                        fractureA = CreateFractureTexture(
                            "PS3D_RiverFoam_FractureA");
                        fractureB = CreateFractureTexture(
                            "PS3D_RiverFoam_FractureB");
                        currentFracture = fractureA;
                        writeFracture = fractureB;
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
                        populationMetricsBuffer = new ComputeBuffer(
                            chunkCount * 8,
                            sizeof(uint),
                            ComputeBufferType.Raw);
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
                    }

                    initializationPhase = InitializationPhase.ClearFracture;
                    break;

                case InitializationPhase.ClearFracture:
                    using (InitClearFractureProfilerMarker.Auto())
                    {
                        DispatchClearFracture(fractureA, 0, fractureWidth);
                        DispatchClearFracture(fractureB, 0, fractureWidth);
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
                    initializationPhase =
                        InitializationPhase.MeasureInitialPopulation;
                    break;

                case InitializationPhase.MeasureInitialPopulation:
                    MeasurePopulation();
                    initializationPhase = InitializationPhase.Finalize;
                    break;

                case InitializationPhase.Finalize:
                    FinalizeInitialization();
                    break;
            }
        }

        private void ResolveComputeKernels()
        {
            clearKernel = computeShader.FindKernel("ClearRange");
            injectKernel = computeShader.FindKernel("InjectFoam");
            buildGuidanceKernel = computeShader.FindKernel("BuildGuidance");
            buildCurrentShoreEdgesKernel =
                computeShader.FindKernel("BuildCurrentShoreEdges");
            composeTopologyKernel =
                computeShader.FindKernel("ComposeTopology");
            captureGeneratedTopologyKernel =
                computeShader.FindKernel("CaptureGeneratedTopology");
            buildEvolvingMajorSupportKernel =
                computeShader.FindKernel("BuildEvolvingMajorSupport");
            clearObstacleExclusionKernel =
                computeShader.FindKernel("ClearObstacleExclusion");
            updateObstacleExclusionKernel =
                computeShader.FindKernel("UpdateObstacleExclusion");
            resetTopologyMetricsKernel =
                computeShader.FindKernel("ResetTopologyMetrics");
            measureTopologyMetricsKernel =
                computeShader.FindKernel("MeasureTopologyMetrics");
            resetPopulationKernel =
                computeShader.FindKernel("ResetPopulation");
            measurePopulationKernel =
                computeShader.FindKernel("MeasurePopulation");
            updateFractureKernel =
                computeShader.FindKernel("UpdateFracture");
            clearFractureKernel =
                computeShader.FindKernel("ClearFractureRange");
            advectForwardKernel =
                computeShader.FindKernel("AdvectForward");
            advectReverseKernel =
                computeShader.FindKernel("AdvectReverse");
            simulateKernel = computeShader.FindKernel("SimulateFoam");
            applyBoundaryKernel = computeShader.FindKernel("ApplyBoundary");
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
            fractureWidth = Mathf.Max(16, Mathf.CeilToInt(fieldWidth * 0.5f));
            fractureHeight = Mathf.Max(
                16,
                Mathf.CeilToInt(fieldHeight * 0.5f));

            if (maximumTextureSize < 24 ||
                fieldHeight > maximumTextureSize ||
                guidanceHeight > maximumTextureSize ||
                fractureWidth > maximumTextureSize ||
                fractureHeight > maximumTextureSize)
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
            populationAccumulator = 0f;
            fractureAccumulator = 0f;
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

        private RenderTexture CreateFractureTexture(string textureName)
        {
            RenderTexture texture = new RenderTexture(
                fractureWidth,
                fractureHeight,
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

        private void BuildMetricBuffer()
        {
            using var profilerScope = InitBuildMetricBufferProfilerMarker.Auto();
            metricRows = new FoamMetricRow[fieldWidth];
            float longitudinalSpacing =
                StylizedRiverFoamTopologyFieldSpace.TexelSpacing(
                    fieldLength,
                    fieldWidth);
            float curvatureSampleDistance = Mathf.Max(
                0.5f,
                longitudinalSpacing * 2f);
            float flowSign = river.Domain.ReverseFlow ? -1f : 1f;

            for (int x = 0; x < fieldWidth; x++)
            {
                float localDistance =
                    StylizedRiverFoamTopologyFieldSpace.LocalDistanceAtTexel(
                        x,
                        fieldWidth,
                        fieldLength);
                float clampedLocalDistance = Mathf.Min(
                    localDistance,
                    validFieldLength);
                float globalDistance =
                    river.Domain.GlobalDistanceMinimum + clampedLocalDistance;
                StylizedRiverSplineSample sample =
                    river.Domain.SampleAtGlobalDistance(globalDistance);
                float left = Mathf.Max(0.05f, sample.LeftSurfaceHalfWidth);
                float right = Mathf.Max(0.05f, sample.RightSurfaceHalfWidth);
                float minimumLateralSpacing =
                    StylizedRiverFoamTopologyFieldSpace.TexelSpacing(
                        left + right,
                        fieldHeight);

                float previousGlobal = Mathf.Max(
                    river.Domain.GlobalDistanceMinimum,
                    globalDistance - curvatureSampleDistance);
                float nextGlobal = Mathf.Min(
                    river.Domain.GlobalDistanceMaximum,
                    globalDistance + curvatureSampleDistance);
                StylizedRiverSplineSample previousSample =
                    river.Domain.SampleAtGlobalDistance(previousGlobal);
                StylizedRiverSplineSample nextSample =
                    river.Domain.SampleAtGlobalDistance(nextGlobal);
                Vector3 previousTangent =
                    previousSample.Tangent * flowSign;
                Vector3 nextTangent =
                    nextSample.Tangent * flowSign;
                // Topology lateral coordinates retain the domain's stored
                // Side basis under reverse flow, so curvature sign must be
                // expressed in that same basis rather than a flipped flow-side.
                Vector3 topologySide = sample.Side;
                float curvatureDistance = Mathf.Max(
                    0.01f,
                    nextGlobal - previousGlobal);
                float signedCurvature = Vector3.Dot(
                    (nextTangent - previousTangent) / curvatureDistance,
                    topologySide);
                float previousWidth =
                    previousSample.LeftSurfaceHalfWidth +
                    previousSample.RightSurfaceHalfWidth;
                float nextWidth =
                    nextSample.LeftSurfaceHalfWidth +
                    nextSample.RightSurfaceHalfWidth;
                float widthDerivative =
                    (nextWidth - previousWidth) / curvatureDistance;
                float asymmetry =
                    (right - left) / Mathf.Max(0.05f, left + right);

                metricRows[x] = new FoamMetricRow
                {
                    WidthsAndSpacing = new Vector4(
                        left,
                        right,
                        longitudinalSpacing,
                        minimumLateralSpacing),
                    TopologyData = new Vector4(
                        signedCurvature,
                        widthDerivative,
                        asymmetry,
                        localDistance <= validFieldLength + 0.0001f
                            ? 1f
                            : 0f),
                    ShoreData = new Vector4(
                        Mathf.Max(0.01f, sample.LeftHalfWidth),
                        Mathf.Max(0.01f, sample.RightHalfWidth),
                        sample.SurfaceHeight,
                        0f)
                };
            }

            metricBuffer?.Release();
            metricBuffer = new ComputeBuffer(
                fieldWidth,
                Marshal.SizeOf<FoamMetricRow>(),
                ComputeBufferType.Structured);
            metricBuffer.SetData(metricRows);
        }

        private void RebuildBoundaryTexture(bool applyToExistingState = true)
        {
            using var profilerScope = InitBuildBoundaryProfilerMarker.Auto();
            if (fieldWidth <= 0 || fieldHeight <= 0 || metricRows.Length != fieldWidth)
            {
                return;
            }

            Color[] pixels = new Color[fieldWidth * fieldHeight];
            float edgeCells = river.Quality switch
            {
                StylizedRiverQuality.Low => 1.5f,
                StylizedRiverQuality.Medium => 2.0f,
                StylizedRiverQuality.High => 2.5f,
                _ => 2.0f
            };

            for (int x = 0; x < fieldWidth; x++)
            {
                float localDistance =
                    StylizedRiverFoamTopologyFieldSpace.LocalDistanceAtTexel(
                        x,
                        fieldWidth,
                        fieldLength);
                if (localDistance > simulationFieldLength + 0.0001f)
                {
                    continue;
                }

                float globalDistance =
                    river.Domain.GlobalDistanceMinimum +
                    Mathf.Min(localDistance, validFieldLength);
                StylizedRiverSplineSample sample =
                    river.Domain.SampleAtGlobalDistance(globalDistance);
                float leftSurface = Mathf.Max(0.05f, sample.LeftSurfaceHalfWidth);
                float rightSurface = Mathf.Max(0.05f, sample.RightSurfaceHalfWidth);
                float leftVisible = Mathf.Max(0.01f, sample.LeftHalfWidth);
                float rightVisible = Mathf.Max(0.01f, sample.RightHalfWidth);
                float animatedEnvelope = Mathf.Lerp(
                    0.25f,
                    0.90f,
                    Mathf.Clamp01(river.ShoreMotion));
                float leftFoamReach = Mathf.Lerp(
                    leftVisible,
                    leftSurface,
                    animatedEnvelope);
                float rightFoamReach = Mathf.Lerp(
                    rightVisible,
                    rightSurface,
                    animatedEnvelope);
                float edgeWidth = Mathf.Max(
                    0.05f,
                    StylizedRiverFoamTopologyFieldSpace.TexelSpacing(
                        leftSurface + rightSurface,
                        fieldHeight) * edgeCells);

                for (int y = 0; y < fieldHeight; y++)
                {
                    float across01 =
                        StylizedRiverFoamTopologyFieldSpace.Across01AtTexel(
                            y,
                            fieldHeight);
                    float lateral =
                        StylizedRiverFoamTopologyFieldSpace.Across01ToMetres(
                        across01,
                        leftSurface,
                        rightSurface);
                    float foamReach = lateral < 0f
                        ? leftFoamReach
                        : rightFoamReach;
                    float distanceInsideReach = foamReach - Mathf.Abs(lateral);
                    float coverage = Mathf.Clamp01(
                        distanceInsideReach / edgeWidth);
                    float attraction = coverage *
                        (1f - Mathf.SmoothStep(
                            0f,
                            1f,
                            Mathf.InverseLerp(0.10f, 0.95f, coverage)));
                    // R/G remain the legacy material-simulation fluid and
                    // attraction contract. B/A are reserved zero: registered
                    // solids now use the full-resolution Obstacle Footprint
                    // mask. Canonical Shore Support comes from the
                    // instantaneous Stage 3 edge.
                    pixels[y * fieldWidth + x] = new Color(
                        coverage,
                        attraction,
                        0f,
                        0f);
                }
            }

            // Registered solid geometry is no longer rasterised into this
            // static boundary texture. Obstacle Footprint is reconstructed
            // from cached exact transformed-mesh solid intervals in a dedicated
            // point-sampled mask.

            if (boundaryTexture == null ||
                boundaryTexture.width != fieldWidth ||
                boundaryTexture.height != fieldHeight)
            {
                if (boundaryTexture != null)
                {
                    DestroyUnityObject(boundaryTexture);
                }

                boundaryTexture = new Texture2D(
                    fieldWidth,
                    fieldHeight,
                    TextureFormat.RGBAHalf,
                    false,
                    true)
                {
                    name = "T_PS3D_RiverFoamBoundary_Runtime",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.DontSave
                };
            }

            boundaryTexture.SetPixels(pixels);
            boundaryTexture.Apply(false, false);
            boundaryDirty = false;

            if (applyToExistingState)
            {
                ApplyBoundaryToState(stateA);
                ApplyBoundaryToState(stateB);
            }
        }

        private void RebuildObstacleExclusionCache(
            bool captureScalarForTopology = true)
        {
            using var profilerScope = InitBuildObstacleExclusionProfilerMarker.Auto();
            obstacleExclusionUsesCachedScalar = false;
            activeTopologyObstacleStale = !captureScalarForTopology;
            if (captureScalarForTopology)
            {
                topologyCacheLoadedForActiveResources = false;
            }

            ReleaseObstacleExclusionBuffers();
            obstacleExclusionMeshFilters.Clear();
            obstacleExclusionCells.Clear();
            obstacleExclusionSamples.Clear();

            if (captureScalarForTopology)
            {
                int cellCount = Mathf.Max(0, fieldWidth * fieldHeight);
                if (obstacleExclusionScalar.Length != cellCount)
                {
                    obstacleExclusionScalar = new float[cellCount];
                }
                else
                {
                    Array.Clear(
                        obstacleExclusionScalar,
                        0,
                        obstacleExclusionScalar.Length);
                }
            }

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            obstacleGeometryVersion = disturbanceRuntime != null
                ? disturbanceRuntime.ObstacleGeometryVersion
                : -1;

            if (disturbanceRuntime == null ||
                fieldWidth < 1 || fieldHeight < 1 ||
                fieldLength <= 0.0001f ||
                river == null || !river.Domain.IsValid)
            {
                ClearObstacleExclusionMask();
                return;
            }

            disturbanceRuntime.CopyObstacleExclusionMeshFiltersTo(
                obstacleExclusionMeshFilters);
            if (obstacleExclusionMeshFilters.Count == 0)
            {
                ClearObstacleExclusionMask();
                return;
            }

            // TODO(PROCEDURAL-CHUNK-BUILD): this exact triangle preparation is
            // the temporary development fallback. Move it into the future map
            // chunk generation/building/linking phase after procedural objects
            // have their final transforms, then load the cached compact cells
            // and intervals here instead of rescanning meshes during startup.
            for (int index = 0;
                 index < obstacleExclusionMeshFilters.Count;
                 index++)
            {
                RiverObstacleExclusionResolver.TryBake(
                    river,
                    obstacleExclusionMeshFilters[index],
                    fieldWidth,
                    fieldHeight,
                    fieldLength,
                    obstacleExclusionCells,
                    obstacleExclusionSamples,
                    out _);
            }

            if (obstacleExclusionCells.Count == 0 ||
                obstacleExclusionSamples.Count == 0)
            {
                ClearObstacleExclusionMask();
                return;
            }

            if (obstacleExclusionGpuCells.Length !=
                obstacleExclusionCells.Count)
            {
                obstacleExclusionGpuCells =
                    new FoamObstacleIntervalCellData[
                        obstacleExclusionCells.Count];
            }

            for (int index = 0;
                 index < obstacleExclusionCells.Count;
                 index++)
            {
                RiverObstacleExclusionCell cell =
                    obstacleExclusionCells[index];
                obstacleExclusionGpuCells[index] =
                    new FoamObstacleIntervalCellData
                    {
                        CoordinateAndOffset = new Vector4(
                            cell.Coordinate.x,
                            cell.Coordinate.y,
                            cell.IntervalOffset,
                            0f)
                    };
            }

            obstacleExclusionCellBuffer = new ComputeBuffer(
                obstacleExclusionGpuCells.Length,
                sizeof(float) * 4,
                ComputeBufferType.Structured);
            obstacleExclusionCellBuffer.SetData(obstacleExclusionGpuCells);

            obstacleExclusionSampleBuffer = new ComputeBuffer(
                obstacleExclusionSamples.Count,
                sizeof(float) * 8,
                ComputeBufferType.Structured);
            obstacleExclusionSampleBuffer.SetData(obstacleExclusionSamples);

            ConfigureTopologyParameters(0f);
            UpdateObstacleExclusionMask();
            if (captureScalarForTopology)
            {
                ReadBackObstacleExclusionScalar();
            }
        }

        private void ReadBackObstacleExclusionScalar()
        {
            if (obstacleExclusionTexture == null ||
                obstacleExclusionScalar.Length != fieldWidth * fieldHeight)
            {
                return;
            }

            if (SystemInfo.supportsAsyncGPUReadback)
            {
                AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(
                    obstacleExclusionTexture,
                    0,
                    TextureFormat.RFloat,
                    null);
                request.WaitForCompletion();
                if (!request.hasError)
                {
                    var data = request.GetData<float>();
                    int count = Mathf.Min(
                        data.Length,
                        obstacleExclusionScalar.Length);
                    for (int index = 0; index < count; index++)
                    {
                        obstacleExclusionScalar[index] =
                            Mathf.Clamp01(data[index]);
                    }

                    return;
                }
            }

            if (obstacleExclusionReadbackTexture == null ||
                obstacleExclusionReadbackTexture.width != fieldWidth ||
                obstacleExclusionReadbackTexture.height != fieldHeight)
            {
                if (obstacleExclusionReadbackTexture != null)
                {
                    DestroyUnityObject(obstacleExclusionReadbackTexture);
                }

                obstacleExclusionReadbackTexture = new Texture2D(
                    fieldWidth,
                    fieldHeight,
                    TextureFormat.RFloat,
                    false,
                    true)
                {
                    name = "T_PS3D_RiverFoamObstacleFootprint_Readback",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.DontSave
                };
            }

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = obstacleExclusionTexture;
            obstacleExclusionReadbackTexture.ReadPixels(
                new Rect(0f, 0f, fieldWidth, fieldHeight),
                0,
                0,
                false);
            obstacleExclusionReadbackTexture.Apply(false, false);
            RenderTexture.active = previous;

            var fallbackData =
                obstacleExclusionReadbackTexture.GetPixelData<float>(0);
            int fallbackCount = Mathf.Min(
                fallbackData.Length,
                obstacleExclusionScalar.Length);
            for (int index = 0; index < fallbackCount; index++)
            {
                obstacleExclusionScalar[index] =
                    Mathf.Clamp01(fallbackData[index]);
            }
        }

        private void ReleaseObstacleExclusionBuffers()
        {
            obstacleExclusionCellBuffer?.Release();
            obstacleExclusionCellBuffer = null;
            obstacleExclusionSampleBuffer?.Release();
            obstacleExclusionSampleBuffer = null;

            if (obstacleExclusionReadbackTexture != null)
            {
                DestroyUnityObject(obstacleExclusionReadbackTexture);
                obstacleExclusionReadbackTexture = null;
            }

            if (obstacleExclusionUploadTexture != null)
            {
                DestroyUnityObject(obstacleExclusionUploadTexture);
                obstacleExclusionUploadTexture = null;
            }
        }

        private void ClearObstacleExclusionMask()
        {
            if (computeShader == null ||
                obstacleExclusionTexture == null ||
                clearObstacleExclusionKernel < 0)
            {
                return;
            }

            computeShader.SetInts(
                "_FoamDimensions",
                fieldWidth,
                fieldHeight);
            computeShader.SetTexture(
                clearObstacleExclusionKernel,
                "_FoamObstacleExclusionWrite",
                obstacleExclusionTexture);
            Dispatch(
                clearObstacleExclusionKernel,
                fieldWidth,
                fieldHeight);
        }

        private void UpdateObstacleExclusionMask()
        {
            if (obstacleExclusionUsesCachedScalar)
            {
                return;
            }

            ClearObstacleExclusionMask();
            if (computeShader == null ||
                updateObstacleExclusionKernel < 0 ||
                obstacleExclusionTexture == null ||
                obstacleExclusionCellBuffer == null ||
                obstacleExclusionSampleBuffer == null ||
                obstacleExclusionCells.Count == 0)
            {
                return;
            }

            computeShader.SetInt(
                "_FoamObstacleCellCount",
                obstacleExclusionCells.Count);
            computeShader.SetBuffer(
                updateObstacleExclusionKernel,
                "_FoamObstacleCells",
                obstacleExclusionCellBuffer);
            computeShader.SetBuffer(
                updateObstacleExclusionKernel,
                "_FoamObstacleSamples",
                obstacleExclusionSampleBuffer);
            computeShader.SetTexture(
                updateObstacleExclusionKernel,
                "_FoamObstacleExclusionWrite",
                obstacleExclusionTexture);
            DispatchOneDimensional(
                updateObstacleExclusionKernel,
                obstacleExclusionCells.Count,
                64);
        }

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

            bool manualInjected = false;
            for (int index = 0; index < pendingInjections.Count; index++)
            {
                PendingInjection injection = pendingInjections[index];
                ActivateInjectionRange(injection, now);
                DispatchInjection(injection);
                reservations.Add(CreateReservation(injection));

                injectedLastUpdate++;
                manualInjected = true;
            }

            pendingInjections.Clear();
            return manualInjected;
        }

        private FoamReservation CreateReservation(PendingInjection injection)
        {
            return new FoamReservation
            {
                CentreGlobalDistance = injection.GlobalDistance,
                AlongRadius = injection.Radius * injection.Elongation,
                RemainingAmount = injection.Amount,
                Elapsed = 0f,
                MaximumLifetime = MaximumManualReservationSeconds
            };
        }

        private void UpdateReservations(float deltaTime, float now)
        {
            float liquid = river.LiquidFactor;
            float speed =
                river.FlowSpeedMetresPerSecond *
                ProvisionalMaterialFlowFollow * liquid;
            float amountDecay =
                DecayToFivePercent /
                Mathf.Max(0.05f, ProvisionalMaterialLifetime);
            float spread =
                ProvisionalMaterialSpread *
                ProvisionalMaterialEvolution;

            for (int index = reservations.Count - 1; index >= 0; index--)
            {
                FoamReservation reservation = reservations[index];
                reservation.Elapsed += deltaTime;
                reservation.CentreGlobalDistance += speed * deltaTime;
                reservation.AlongRadius +=
                    (0.15f + spread * 0.35f) * deltaTime;
                reservation.RemainingAmount *=
                    Mathf.Exp(-amountDecay * deltaTime);

                if (reservation.Elapsed >= reservation.MaximumLifetime ||
                    reservation.RemainingAmount < 0.015f)
                {
                    reservations.RemoveAt(index);
                    continue;
                }

                ActivateReservationRange(reservation, now);
            }
        }

        private void ActivateInjectionRange(PendingInjection injection, float now)
        {
            float padding = Mathf.Max(0.5f, injection.Radius * injection.Elongation);
            ActivateGlobalRange(
                injection.GlobalDistance - padding,
                injection.GlobalDistance + padding,
                now + Mathf.Max(
                    ProvisionalMaterialLifetime,
                    ProvisionalMaterialFreshnessLifetime));
        }

        private void ActivateReservationRange(FoamReservation reservation, float now)
        {
            float margin = Mathf.Max(
                0.5f,
                Mathf.Abs(
                    river.FlowSpeedMetresPerSecond *
                    ProvisionalMaterialFlowFollow) /
                Mathf.Max(1f, ResolveUpdateRate()) * 2f);
            ActivateGlobalRange(
                reservation.CentreGlobalDistance - reservation.AlongRadius - margin,
                reservation.CentreGlobalDistance + reservation.AlongRadius + margin,
                now + 1.5f / Mathf.Max(1f, ResolveUpdateRate()));
        }

        private void ActivateGlobalRange(
            float minimumGlobal,
            float maximumGlobal,
            double activeUntil)
        {
            int minimumChunk = GlobalDistanceToChunk(minimumGlobal);
            int maximumChunk = GlobalDistanceToChunk(maximumGlobal);

            for (int chunk = minimumChunk; chunk <= maximumChunk; chunk++)
            {
                if (!chunkActive[chunk])
                {
                    // Inactive chunks are already cleared by allocation or
                    // the existing sleep path; reactivation needs no extra
                    // clear dispatch.
                    chunkActive[chunk] = true;
                }

                chunkActiveUntil[chunk] = Math.Max(
                    chunkActiveUntil[chunk],
                    activeUntil);
            }
        }

        private void ActivateAllChunks(double activeUntil)
        {
            for (int chunk = 0; chunk < chunkCount; chunk++)
            {
                chunkActive[chunk] = true;
                chunkActiveUntil[chunk] = Math.Max(
                    chunkActiveUntil[chunk],
                    activeUntil);
            }
        }

        private void UpdateActiveChunks(float now)
        {
            for (int chunk = 0; chunk < chunkCount; chunk++)
            {
                if (!chunkActive[chunk] || now <= chunkActiveUntil[chunk])
                {
                    continue;
                }

                chunkActive[chunk] = false;
                chunkActiveUntil[chunk] = 0.0;
                ClearChunk(chunk);
            }
        }

        private void SimulateActiveChunks(float deltaTime)
        {
            if (CountActiveChunks() == 0)
            {
                return;
            }

            previousState = currentState;

            int chunk = 0;
            while (chunk < chunkCount)
            {
                while (chunk < chunkCount && !chunkActive[chunk])
                {
                    chunk++;
                }

                if (chunk >= chunkCount)
                {
                    break;
                }

                int startChunk = chunk;
                while (chunk < chunkCount && chunkActive[chunk])
                {
                    chunk++;
                }

                int endChunkExclusive = chunk;
                int startX = startChunk * resolutionPerChunk;
                int countX = Mathf.Min(
                    fieldWidth - startX,
                    (endChunkExclusive - startChunk) * resolutionPerChunk);
                DispatchSimulation(startX, countX);
            }

            (currentState, writeState) = (writeState, currentState);
        }

        private void DispatchInjection(PendingInjection injection)
        {
            int centreX = GlobalDistanceToX(injection.GlobalDistance);
            float alongRadius = injection.Radius * injection.Elongation *
                (injection.CompoundShape ? 1.25f : 1f);
            float dx = StylizedRiverFoamTopologyFieldSpace.TexelSpacing(
                fieldLength,
                fieldWidth);
            int radiusPixels = Mathf.CeilToInt(alongRadius / Mathf.Max(0.001f, dx)) + 2;
            int startX = Mathf.Clamp(centreX - radiusPixels, 0, fieldWidth - 1);
            int endX = Mathf.Clamp(centreX + radiusPixels, 0, fieldWidth - 1);
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
            computeShader.SetFloat("_FoamInjectionGlobalDistance", injection.GlobalDistance);
            computeShader.SetFloat("_FoamInjectionAcrossNormalized", injection.AcrossNormalized);
            computeShader.SetFloat("_FoamInjectionRadius", injection.Radius);
            computeShader.SetFloat("_FoamInjectionAmount", injection.Amount);
            computeShader.SetFloat("_FoamInjectionFreshness", injection.Freshness);
            computeShader.SetFloat("_FoamInjectionIntegrity", injection.Integrity);
            computeShader.SetFloat("_FoamInjectionPhase", injection.Phase);
            computeShader.SetFloat("_FoamInjectionElongation", injection.Elongation);
            computeShader.SetFloat("_FoamInjectionShapeSeed", injection.ShapeSeed);
            computeShader.SetFloat("_FoamInjectionShapeVariety", injection.ShapeVariety);
            computeShader.SetFloat(
                "_FoamInjectionCompound",
                injection.CompoundShape ? 1f : 0f);
            computeShader.SetBuffer(injectKernel, "_FoamMetricRows", metricBuffer);
            computeShader.SetTexture(injectKernel, "_FoamBoundary", boundaryTexture);

            DispatchInjectionToState(currentState, countX);

            if (injection.IsManual)
            {
                // Manual diagnostics exist in both temporal states so
                // interpolation cannot hide a fresh source behind an empty
                // previous field. They then evolve through the complete solver.
                if (stateA != null && stateA != currentState)
                {
                    DispatchInjectionToState(stateA, countX);
                }

                if (stateB != null && stateB != currentState && stateB != stateA)
                {
                    DispatchInjectionToState(stateB, countX);
                }

                lastInjectionStateSynchronized = stateA != null && stateB != null;
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

        private void BuildGuidanceField(float deltaTime)
        {
            using var profilerScope = InitBuildGuidanceProfilerMarker.Auto();
            if (computeShader == null || guidanceTexture == null ||
                buildGuidanceKernel < 0)
            {
                return;
            }

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
            computeShader.SetInts(
                "_FoamGuidanceDimensions",
                guidanceWidth,
                guidanceHeight);
            computeShader.SetInts(
                "_FoamFractureDimensions",
                fractureWidth,
                fractureHeight);
            computeShader.SetInt("_FoamChunkCount", chunkCount);
            computeShader.SetFloat(
                "_FoamGlobalStart",
                allocatedGlobalStart);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat("_FoamDeltaTime", deltaTime);
            computeShader.SetFloat(
                "_FoamTime",
                ResolveInitializationMotionTime());
            computeShader.SetFloat("_FoamSeed", river.VisualSeed);
            computeShader.SetFloat(
                "_FoamEvolution",
                ProvisionalMaterialEvolution);
            computeShader.SetBuffer(
                buildGuidanceKernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetTexture(
                buildGuidanceKernel,
                "_FoamGuidanceWrite",
                guidanceTexture);
            Dispatch(buildGuidanceKernel, guidanceWidth, guidanceHeight);
        }

        private void BuildMajorTopology()
        {
            using var profilerScope = TopologyBuildMajorProfilerMarker.Auto();
            if (river == null || !river.Domain.IsValid ||
                topologyGeneratedTexture == null ||
                fieldWidth < 2 || fieldHeight < 2 ||
                fieldLength <= 0.0001f || validFieldLength <= 0.0001f)
            {
                majorTopology = null;
                connectorTopology = null;
                pocketTopology = null;
                majorTopologyInputSignature = int.MinValue;
                connectorTopologyInputSignature = int.MinValue;
                pocketTopologyInputSignature = int.MinValue;
                ReleaseMajorEvolutionResources();
                ClearRenderTexture(topologyGeneratedTexture);
                ClearRenderTexture(evolvingMajorTexture);
                ClearRenderTexture(evolvingHostedNegativeTexture);
                ClearRenderTexture(evolvingFreeWaterNegativeTexture);
                ClearRenderTexture(evolvingConnectorTexture);
                ClearRenderTexture(evolvingWeakSpanNegativeTexture);
                return;
            }

            int cellCount = fieldWidth * fieldHeight;
            if (obstacleExclusionScalar.Length != cellCount)
            {
                obstacleExclusionScalar = new float[cellCount];
            }

            majorTopology =
                StylizedRiverFoamMajorTopologyGenerator.Generate(
                    river.Domain,
                    fieldWidth,
                    fieldHeight,
                    fieldLength,
                    validFieldLength,
                    river.Quality,
                    river.ShoreMotion,
                    river.FoamMajorSupportAmount,
                    river.FoamMajorSupportSize,
                    river.FoamMajorSupportSizeVariation,
                    river.FoamMajorRecycleTerritoryDeviationPercent,
                    river.FoamMajorSupportSeed,
                    obstacleExclusionScalar);
            connectorTopology = null;
            pocketTopology = null;
            connectorTopologyInputSignature = int.MinValue;
            pocketTopologyInputSignature = int.MinValue;
            InitializeMajorEvolution();
            UploadGeneratedTopology();
            majorTopologyInputSignature = ResolveMajorTopologyInputSignature();
        }

        private void BuildConnectorTopology()
        {
            using var profilerScope =
                TopologyBuildConnectorProfilerMarker.Auto();
            if (river == null || !river.Domain.IsValid ||
                topologyGeneratedTexture == null ||
                majorTopology == null ||
                fieldWidth < 2 || fieldHeight < 2 ||
                fieldLength <= 0.0001f || validFieldLength <= 0.0001f)
            {
                ReleaseConnectorIdentityReconstructionResources();
                connectorTopology = null;
                pocketTopology = null;
                connectorTopologyInputSignature = int.MinValue;
                pocketTopologyInputSignature = int.MinValue;
                InitializeHostedNegativeEvolution(false);
                InitializeFreeWaterEvolution(false);
                UploadGeneratedTopology();
                BuildEvolvingMajorField();
                return;
            }

            ReleaseConnectorIdentityReconstructionResources();
            connectorTopology =
                StylizedRiverFoamConnectorTopologyGenerator.Generate(
                    river.Domain,
                    fieldWidth,
                    fieldHeight,
                    fieldLength,
                    validFieldLength,
                    river.Quality,
                    river.ShoreMotion,
                    river.FoamMajorSupportSeed,
                    river.FoamConnectorAmount,
                    river.FoamConnectorDirectness,
                    river.FoamConnectorLengthPreference,
                    obstacleExclusionScalar,
                    majorTopology);
            connectorTopologyInputSignature =
                ResolveConnectorTopologyInputSignature();
            pocketTopology = null;
            pocketTopologyInputSignature = int.MinValue;
            InitializeHostedNegativeEvolution(false);
            InitializeFreeWaterEvolution(false);
            UploadGeneratedTopology();
            BuildEvolvingMajorField();
        }

        private void BuildPocketTopology()
        {
            using var profilerScope = TopologyBuildPocketProfilerMarker.Auto();
            if (
                topologyGeneratedTexture == null ||
                majorTopology == null ||
                connectorTopology == null ||
                river == null || !river.Domain.IsValid)
            {
                ReleaseConnectorIdentityReconstructionResources();
                pocketTopology = null;
                pocketTopologyInputSignature = int.MinValue;
                InitializeHostedNegativeEvolution(false);
                InitializeFreeWaterEvolution(false);
                UploadGeneratedTopology();
                BuildEvolvingMajorField();
                return;
            }

            pocketTopology =
                StylizedRiverFoamPocketTopologyGenerator.Generate(
                    river.Domain,
                    fieldWidth,
                    fieldHeight,
                    fieldLength,
                    validFieldLength,
                    river.Quality,
                    river.ShoreMotion,
                    river.FoamMajorSupportSeed,
                    river.FoamInteriorPocketAmount,
                    river.FoamEdgeCavityAmount,
                    river.FoamConnectorWeakSpanAmount,
                    river.FoamFreeWaterEventAmount,
                    obstacleExclusionScalar,
                    majorTopology,
                    connectorTopology);
            pocketTopologyInputSignature =
                ResolvePocketTopologyInputSignature();
            InitializeHostedNegativeEvolution(false);
            InitializeFreeWaterEvolution(false);
            InitializeConnectorIdentityReconstruction(false);
            UploadGeneratedTopology();
            BuildEvolvingMajorField();
        }

        private void UploadGeneratedTopology()
        {
            if (topologyGeneratedTexture == null ||
                fieldWidth < 1 || fieldHeight < 1)
            {
                return;
            }

            int cellCount = fieldWidth * fieldHeight;
            if (topologyGeneratedUploadPixels.Length != cellCount)
            {
                topologyGeneratedUploadPixels = new Color[cellCount];
            }
            else
            {
                Array.Clear(
                    topologyGeneratedUploadPixels,
                    0,
                    topologyGeneratedUploadPixels.Length);
            }

            majorTopology?.FillUploadPixels(topologyGeneratedUploadPixels);
            connectorTopology?.AddToUploadPixels(
                topologyGeneratedUploadPixels,
                connectorIdentityReconstructionReady);
            pocketTopology?.AddToUploadPixels(
                topologyGeneratedUploadPixels,
                hostedNegativeEvolutionReady,
                freeWaterEvolutionReady,
                weakSpanIdentityReconstructionReady);

            if (topologyGeneratedUploadTexture == null ||
                topologyGeneratedUploadTexture.width != fieldWidth ||
                topologyGeneratedUploadTexture.height != fieldHeight)
            {
                if (topologyGeneratedUploadTexture != null)
                {
                    DestroyUnityObject(topologyGeneratedUploadTexture);
                }

                topologyGeneratedUploadTexture = new Texture2D(
                    fieldWidth,
                    fieldHeight,
                    TextureFormat.RGBAHalf,
                    false,
                    true)
                {
                    name = "T_PS3D_RiverFoamGeneratedTopology_Upload",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.DontSave
                };
            }

            topologyGeneratedUploadTexture.SetPixels(
                topologyGeneratedUploadPixels);
            topologyGeneratedUploadTexture.Apply(false, false);
            Graphics.Blit(
                topologyGeneratedUploadTexture,
                topologyGeneratedTexture);
        }

        private void InitializeMajorEvolution()
        {
            ReleaseMajorEvolutionResources();
            if (majorTopology == null || river == null ||
                !river.Domain.IsValid || computeShader == null ||
                evolvingMajorTexture == null ||
                buildEvolvingMajorSupportKernel < 0)
            {
                ClearRenderTexture(evolvingMajorTexture);
                ClearRenderTexture(evolvingHostedNegativeTexture);
                ClearRenderTexture(evolvingFreeWaterNegativeTexture);
                ClearRenderTexture(evolvingConnectorTexture);
                ClearRenderTexture(evolvingWeakSpanNegativeTexture);
                return;
            }

            IReadOnlyList<StylizedRiverFoamMajorRegion> regions =
                majorTopology.Regions;
            IReadOnlyList<StylizedRiverFoamPreparedMajorRegion> prepared =
                majorTopology.PreparedRegions;
            int slotCount = Mathf.Min(regions.Count, prepared.Count);
            if (slotCount <= 0)
            {
                ClearRenderTexture(evolvingMajorTexture);
                ClearRenderTexture(evolvingHostedNegativeTexture);
                ClearRenderTexture(evolvingFreeWaterNegativeTexture);
                ClearRenderTexture(evolvingConnectorTexture);
                ClearRenderTexture(evolvingWeakSpanNegativeTexture);
                return;
            }

            int maskResolution = prepared[0].MaskResolution;
            for (int index = 1; index < slotCount; index++)
            {
                if (prepared[index].MaskResolution != maskResolution)
                {
                    ClearRenderTexture(evolvingMajorTexture);
                    ClearRenderTexture(evolvingHostedNegativeTexture);
                    ClearRenderTexture(evolvingFreeWaterNegativeTexture);
                    ClearRenderTexture(evolvingConnectorTexture);
                    ClearRenderTexture(evolvingWeakSpanNegativeTexture);
                    return;
                }
            }

            majorEvolutionSlots = new MajorEvolutionSlot[slotCount];
            majorEvolutionGpuData = new FoamMajorEvolutionData[slotCount];
            majorMaskUploadPixels = new Color[
                maskResolution * maskResolution];
            majorMaskTextureArray = new Texture2DArray(
                maskResolution,
                maskResolution,
                slotCount,
                TextureFormat.RGBAHalf,
                false,
                true)
            {
                name = "T_PS3D_RiverFoamMajorMasks",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            for (int index = 0; index < slotCount; index++)
            {
                StylizedRiverFoamPreparedMajorRegion preparedRegion =
                    prepared[index];
                float[] source = preparedRegion.LocalSupportData;
                for (int pixel = 0;
                     pixel < majorMaskUploadPixels.Length;
                     pixel++)
                {
                    float value = pixel < source.Length
                        ? Mathf.Clamp01(source[pixel])
                        : 0f;
                    majorMaskUploadPixels[pixel] = new Color(
                        value,
                        0f,
                        0f,
                        0f);
                }

                majorMaskTextureArray.SetPixels(
                    majorMaskUploadPixels,
                    index,
                    0);

                StylizedRiverFoamMajorRegion region = regions[index];
                MajorEvolutionPose initialPose = new MajorEvolutionPose
                {
                    LocalDistance = Mathf.Clamp(
                        region.CentreGlobalDistance -
                            river.Domain.GlobalDistanceMinimum,
                        0f,
                        validFieldLength),
                    AcrossNormalized = Mathf.Clamp(
                        region.CentreAcrossNormalized,
                        -0.82f,
                        0.82f),
                    OrientationRadians = region.OrientationRadians,
                    MetresPerCandidateCell =
                        region.MetresPerCandidateCell,
                    ScaleAlong = 1f,
                    ScaleAcross = 1f,
                    Shear = 0f,
                    WarpAlong = 0f,
                    WarpAcross = 0f,
                    WarpPhaseA = HashMajorEvolution(
                        region.EvolutionSeed,
                        1u) * Mathf.PI * 2f,
                    WarpPhaseB = HashMajorEvolution(
                        region.EvolutionSeed,
                        2u) * Mathf.PI * 2f,
                    SupportScale = 1f
                };

                MajorEvolutionSlot slot = new MajorEvolutionSlot
                {
                    StableId = region.StableId,
                    PreparedIndex = index,
                    BaseMetresPerCandidateCell =
                        region.MetresPerCandidateCell,
                    Current = initialPose,
                    Start = initialPose,
                    Target = initialPose,
                    MoveElapsed = 0f,
                    MoveDuration = 0f,
                    OccurrenceElapsed = 0f,
                    HopIndex = 0,
                    RecycleCount = 0,
                    LastAnchorIndex = -1,
                    IsMoving = false
                };
                ResolveMajorOccurrenceBudget(ref slot);
                ResolveMajorDwell(ref slot, 10u);
                if (IsInMajorEgress(initialPose.LocalDistance))
                {
                    slot.DwellRemaining = Mathf.Min(
                        slot.DwellRemaining,
                        Mathf.Lerp(
                            0.25f,
                            0.85f,
                            HashMajorEvolution(
                                slot.StableId,
                                11u)));
                }

                majorEvolutionSlots[index] = slot;
            }

            majorMaskTextureArray.Apply(false, true);
            majorEvolutionBuffer = new ComputeBuffer(
                slotCount,
                sizeof(float) * 20,
                ComputeBufferType.Structured);
            majorEvolutionReady = true;
            majorEvolutionAccumulator = 0f;
            majorEvolutionLifetimeInputSignature =
                ResolveMajorEvolutionLifetimeInputSignature();
            InitializeHostedNegativeEvolution(false);
            InitializeFreeWaterEvolution(false);
            BuildEvolvingMajorField();
        }

        private void InitializeHostedNegativeEvolution(
            bool rebuildField = true)
        {
            ReleaseHostedNegativeEvolutionResources();
            if (!majorEvolutionReady || majorMaskTextureArray == null ||
                evolvingHostedNegativeTexture == null)
            {
                ClearRenderTexture(evolvingHostedNegativeTexture);
                return;
            }

            int maskResolution = majorMaskTextureArray.width;
            IReadOnlyList<StylizedRiverFoamPreparedHostedNegativeRegion>
                prepared = pocketTopology != null
                    ? pocketTopology.PreparedHostedRegions
                    : Array.Empty<
                        StylizedRiverFoamPreparedHostedNegativeRegion>();
            int validCount = 0;
            for (int index = 0; index < prepared.Count; index++)
            {
                if (prepared[index].MaskResolution == maskResolution &&
                    prepared[index].HostPreparedIndex >= 0 &&
                    prepared[index].HostPreparedIndex <
                        majorEvolutionSlots.Length)
                {
                    validCount++;
                }
            }

            int sliceCount = Mathf.Max(1, validCount);
            hostedNegativeMaskUploadPixels = new Color[
                maskResolution * maskResolution];
            hostedNegativeMaskTextureArray = new Texture2DArray(
                maskResolution,
                maskResolution,
                sliceCount,
                TextureFormat.RGBAHalf,
                false,
                true)
            {
                name = "T_PS3D_RiverFoamHostedNegativeMasks",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            hostedNegativeEvolutionSlots =
                new HostedNegativeEvolutionSlot[validCount];
            hostedNegativeEvolutionGpuData =
                new FoamHostedNegativeEvolutionData[Mathf.Max(1, validCount)];
            int writeIndex = 0;
            for (int index = 0; index < prepared.Count; index++)
            {
                StylizedRiverFoamPreparedHostedNegativeRegion region =
                    prepared[index];
                if (region.MaskResolution != maskResolution ||
                    region.HostPreparedIndex < 0 ||
                    region.HostPreparedIndex >= majorEvolutionSlots.Length)
                {
                    continue;
                }

                float[] source = region.LocalPressureData;
                for (int pixel = 0;
                     pixel < hostedNegativeMaskUploadPixels.Length;
                     pixel++)
                {
                    float value = pixel < source.Length
                        ? Mathf.Clamp01(source[pixel])
                        : 0f;
                    hostedNegativeMaskUploadPixels[pixel] = new Color(
                        value,
                        0f,
                        0f,
                        0f);
                }

                hostedNegativeMaskTextureArray.SetPixels(
                    hostedNegativeMaskUploadPixels,
                    writeIndex,
                    0);
                HostedNegativeEvolutionPose initialPose =
                    CreateIdentityHostedNegativePose();
                hostedNegativeEvolutionSlots[writeIndex] =
                    new HostedNegativeEvolutionSlot
                    {
                        StableId = region.StableId,
                        PreparedIndex = index,
                        HostSlotIndex = region.HostPreparedIndex,
                        RegionClass = region.RegionClass,
                        CurrentVariantIndex = 0,
                        TargetVariantIndex = 0,
                        Current = initialPose,
                        Start = initialPose,
                        Target = initialPose
                    };
                writeIndex++;
            }

            if (validCount == 0)
            {
                hostedNegativeMaskTextureArray.SetPixels(
                    hostedNegativeMaskUploadPixels,
                    0,
                    0);
            }
            hostedNegativeMaskTextureArray.Apply(false, true);
            hostedNegativeEvolutionBuffer = new ComputeBuffer(
                Mathf.Max(1, validCount),
                sizeof(float) * 12,
                ComputeBufferType.Structured);
            if (validCount == 0)
            {
                hostedNegativeEvolutionBuffer.SetData(
                    hostedNegativeEvolutionGpuData);
            }
            hostedNegativeEvolutionReady = validCount > 0;
            hostedNegativeLocalChangeCount = 0;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            hostedNegativeInitialParityPending =
                hostedNegativeEvolutionReady &&
                IsTopologyDebugActive &&
                SystemInfo.supportsAsyncGPUReadback;
            hostedNegativeInitialParityReadbackPending = false;
            hostedNegativeInitialParityAvailable = false;
            hostedNegativeInitialParityMeanDifference = 0f;
            hostedNegativeInitialParityMaximumDifference = 0f;
#endif
            if (rebuildField)
            {
                BuildEvolvingMajorField();
            }
        }

        private void ReleaseHostedNegativeEvolutionResources()
        {
            hostedNegativeEvolutionBuffer?.Release();
            hostedNegativeEvolutionBuffer = null;
            if (hostedNegativeMaskTextureArray != null)
            {
                DestroyUnityObject(hostedNegativeMaskTextureArray);
                hostedNegativeMaskTextureArray = null;
            }

            hostedNegativeEvolutionSlots =
                Array.Empty<HostedNegativeEvolutionSlot>();
            hostedNegativeEvolutionGpuData =
                Array.Empty<FoamHostedNegativeEvolutionData>();
            hostedNegativeMaskUploadPixels = Array.Empty<Color>();
            hostedNegativeEvolutionReady = false;
            hostedNegativeLocalChangeCount = 0;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            hostedNegativeInitialParityGeneration++;
            hostedNegativeInitialParityPending = false;
            hostedNegativeInitialParityReadbackPending = false;
            hostedNegativeInitialParityAvailable = false;
            hostedNegativeInitialParityMeanDifference = 0f;
            hostedNegativeInitialParityMaximumDifference = 0f;
#endif
        }

        private void InitializeFreeWaterEvolution(bool rebuildField = true)
        {
            ReleaseFreeWaterEvolutionResources();
            if (!majorEvolutionReady || evolvingFreeWaterNegativeTexture == null)
            {
                ClearRenderTexture(evolvingFreeWaterNegativeTexture);
                ClearRenderTexture(evolvingConnectorTexture);
                ClearRenderTexture(evolvingWeakSpanNegativeTexture);
                return;
            }

            IReadOnlyList<StylizedRiverFoamPreparedFreeWaterRegion> prepared =
                pocketTopology != null
                    ? pocketTopology.PreparedFreeWaterRegions
                    : Array.Empty<StylizedRiverFoamPreparedFreeWaterRegion>();
            int acceptedCount = pocketTopology != null
                ? pocketTopology.AcceptedFreeWaterEventCount
                : 0;
            bool allAcceptedPrepared = acceptedCount > 0 &&
                prepared.Count == acceptedCount;
            int maskResolution = allAcceptedPrepared && prepared.Count > 0
                ? prepared[0].MaskResolution
                : 1;
            bool consistentMasks = allAcceptedPrepared;
            for (int index = 1; index < prepared.Count; index++)
            {
                if (prepared[index].MaskResolution != maskResolution)
                {
                    consistentMasks = false;
                    break;
                }
            }

            int validCount = consistentMasks ? prepared.Count : 0;
            int sliceCount = Mathf.Max(1, validCount);
            freeWaterNegativeMaskUploadPixels = new Color[
                maskResolution * maskResolution];
            freeWaterNegativeMaskTextureArray = new Texture2DArray(
                maskResolution,
                maskResolution,
                sliceCount,
                TextureFormat.RGBAHalf,
                false,
                true)
            {
                name = "T_PS3D_RiverFoamFreeWaterNegativeMasks",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            freeWaterEvolutionSlots =
                new FreeWaterEvolutionSlot[validCount];
            freeWaterEvolutionGpuData =
                new FoamFreeWaterEvolutionData[Mathf.Max(1, validCount)];
            for (int index = 0; index < validCount; index++)
            {
                StylizedRiverFoamPreparedFreeWaterRegion region =
                    prepared[index];
                float[] source = region.LocalPressureData;
                for (int pixel = 0;
                     pixel < freeWaterNegativeMaskUploadPixels.Length;
                     pixel++)
                {
                    float value = pixel < source.Length
                        ? Mathf.Clamp01(source[pixel])
                        : 0f;
                    freeWaterNegativeMaskUploadPixels[pixel] = new Color(
                        value,
                        0f,
                        0f,
                        0f);
                }

                freeWaterNegativeMaskTextureArray.SetPixels(
                    freeWaterNegativeMaskUploadPixels,
                    index,
                    0);
                FreeWaterEvolutionPose initialPose =
                    CreateInitialFreeWaterPose(region);
                FreeWaterEvolutionSlot slot =
                    new FreeWaterEvolutionSlot
                    {
                        StableId = region.StableId,
                        PreparedIndex = index,
                        Current = initialPose,
                        Start = initialPose,
                        Target = initialPose,
                        MoveElapsed = 0f,
                        MoveDuration = 0f,
                        OccurrenceElapsed = 0f,
                        HopIndex = 0,
                        RecycleCount = 0,
                        LastAnchorIndex = -1,
                        IsMoving = false
                    };
                ResolveFreeWaterOccurrenceBudget(ref slot);
                ResolveFreeWaterDwell(ref slot, 0u);
                if (IsInFreeWaterEgress(initialPose.LocalDistance))
                {
                    slot.DwellRemaining = Mathf.Min(
                        slot.DwellRemaining,
                        Mathf.Lerp(
                            0.5f,
                            1.5f,
                            HashMajorEvolution(
                                slot.StableId,
                                1u)));
                }

                freeWaterEvolutionSlots[index] = slot;
            }

            if (validCount == 0)
            {
                freeWaterNegativeMaskTextureArray.SetPixels(
                    freeWaterNegativeMaskUploadPixels,
                    0,
                    0);
            }
            freeWaterNegativeMaskTextureArray.Apply(false, true);
            freeWaterEvolutionBuffer = new ComputeBuffer(
                Mathf.Max(1, validCount),
                sizeof(float) * 12,
                ComputeBufferType.Structured);
            if (validCount == 0)
            {
                freeWaterEvolutionBuffer.SetData(freeWaterEvolutionGpuData);
            }
            freeWaterEvolutionReady = validCount > 0;
            freeWaterEvolutionAccumulator = 0f;
            freeWaterMoveCount = 0;
            freeWaterRecycleCount = 0;
            freeWaterUpstreamViolationCount = 0;
            freeWaterObservedMinimumDwell = float.PositiveInfinity;
            freeWaterObservedMaximumDwell = 0f;
            freeWaterObservedMinimumMove = float.PositiveInfinity;
            freeWaterObservedMaximumMove = 0f;
            for (int index = 0; index < freeWaterEvolutionSlots.Length; index++)
            {
                float dwell = freeWaterEvolutionSlots[index]
                    .LastDwellDuration;
                freeWaterObservedMinimumDwell = Mathf.Min(
                    freeWaterObservedMinimumDwell,
                    dwell);
                freeWaterObservedMaximumDwell = Mathf.Max(
                    freeWaterObservedMaximumDwell,
                    dwell);
            }
            if (rebuildField)
            {
                BuildEvolvingMajorField();
            }
        }

        private void ReleaseFreeWaterEvolutionResources()
        {
            freeWaterEvolutionBuffer?.Release();
            freeWaterEvolutionBuffer = null;
            if (freeWaterNegativeMaskTextureArray != null)
            {
                DestroyUnityObject(freeWaterNegativeMaskTextureArray);
                freeWaterNegativeMaskTextureArray = null;
            }

            freeWaterEvolutionSlots = Array.Empty<FreeWaterEvolutionSlot>();
            freeWaterEvolutionGpuData =
                Array.Empty<FoamFreeWaterEvolutionData>();
            freeWaterNegativeMaskUploadPixels = Array.Empty<Color>();
            freeWaterEvolutionReady = false;
            freeWaterEvolutionAccumulator = 0f;
            freeWaterMoveCount = 0;
            freeWaterRecycleCount = 0;
            freeWaterUpstreamViolationCount = 0;
            freeWaterObservedMinimumDwell = float.PositiveInfinity;
            freeWaterObservedMaximumDwell = 0f;
            freeWaterObservedMinimumMove = float.PositiveInfinity;
            freeWaterObservedMaximumMove = 0f;
        }

        private void InitializeConnectorIdentityReconstruction(
            bool rebuildField = true)
        {
            ReleaseConnectorIdentityReconstructionResources();
            if (!majorEvolutionReady ||
                connectorTopology == null || pocketTopology == null ||
                computeShader == null ||
                majorEvolutionBuffer == null ||
                majorMaskTextureArray == null ||
                hostedNegativeEvolutionBuffer == null ||
                hostedNegativeMaskTextureArray == null ||
                freeWaterEvolutionBuffer == null ||
                freeWaterNegativeMaskTextureArray == null ||
                topologyGeneratedTexture == null ||
                boundaryTexture == null ||
                obstacleExclusionTexture == null ||
                metricBuffer == null ||
                evolvingConnectorTexture == null ||
                evolvingWeakSpanNegativeTexture == null ||
                buildEvolvingMajorSupportKernel < 0)
            {
                ClearRenderTexture(evolvingConnectorTexture);
                ClearRenderTexture(evolvingWeakSpanNegativeTexture);
                return;
            }

            int acceptedConnectorCount =
                connectorTopology.AcceptedConnectorCount;
            IReadOnlyList<StylizedRiverFoamConnectorPath> acceptedPaths =
                connectorTopology.PreparedPaths;
            IReadOnlyList<StylizedRiverFoamConnectorPath> cataloguePaths =
                connectorTopology.PreparedRelationshipCataloguePaths;
            bool allConnectorsPrepared = acceptedConnectorCount > 0 &&
                connectorTopology.PreparedConnectorCount ==
                    acceptedConnectorCount &&
                acceptedPaths.Count == acceptedConnectorCount &&
                cataloguePaths.Count >= acceptedConnectorCount &&
                connectorTopology.PreparedRelationshipCatalogueCount ==
                    cataloguePaths.Count;

            List<FoamConnectorIdentityData> connectorRecords = new(
                acceptedConnectorCount);
            List<ConnectorEvolutionSlot> evolutionSlots = new(
                acceptedConnectorCount);
            List<ConnectorRelationshipCandidate> relationshipCandidates = new(
                cataloguePaths.Count);
            Dictionary<uint, int> candidateIndexByStableId = new(
                cataloguePaths.Count);
            Dictionary<uint, int> connectorIndexByStableId = new(
                acceptedConnectorCount);
            int maximumPathPointCount = 0;

            if (allConnectorsPrepared)
            {
                for (int pathIndex = 0;
                     pathIndex < cataloguePaths.Count;
                     pathIndex++)
                {
                    StylizedRiverFoamConnectorPath path =
                        cataloguePaths[pathIndex];
                    Vector2[] points = path?.PreparedMetricPointData;
                    float[] cumulative = path?.NormalizedCumulativeLengthData;
                    if (path == null || !path.PreparationAvailable ||
                        points == null || points.Length < 2 ||
                        points.Length >
                            StylizedRiverFoamConnectorTopologyGenerator
                                .MaximumPreparedPathPointCount ||
                        cumulative == null ||
                        cumulative.Length != points.Length ||
                        candidateIndexByStableId.ContainsKey(path.StableId) ||
                        !TryResolveMajorEvolutionSlot(
                            path.StartEndpointBinding,
                            out int startHostSlotIndex) ||
                        !TryResolveMajorEvolutionSlot(
                            path.EndEndpointBinding,
                            out int endHostSlotIndex) ||
                        startHostSlotIndex == endHostSlotIndex)
                    {
                        allConnectorsPrepared = false;
                        break;
                    }

                    candidateIndexByStableId.Add(
                        path.StableId,
                        relationshipCandidates.Count);
                    relationshipCandidates.Add(
                        new ConnectorRelationshipCandidate
                        {
                            Path = path,
                            StartHostSlotIndex = startHostSlotIndex,
                            EndHostSlotIndex = endHostSlotIndex,
                            BasePathLengthMetres =
                                MeasureConnectorPreparedPathLength(points),
                            SelectionWeight = 1f
                        });
                    maximumPathPointCount = Mathf.Max(
                        maximumPathPointCount,
                        points.Length);
                }
            }

            if (allConnectorsPrepared)
            {
                ResolveConnectorRelationshipSelectionWeights(
                    relationshipCandidates);
            }

            Vector4[] flattenedPoints = allConnectorsPrepared
                ? new Vector4[
                    acceptedConnectorCount * maximumPathPointCount]
                : Array.Empty<Vector4>();
            if (allConnectorsPrepared)
            {
                for (int pathIndex = 0;
                     pathIndex < acceptedPaths.Count;
                     pathIndex++)
                {
                    StylizedRiverFoamConnectorPath path =
                        acceptedPaths[pathIndex];
                    Vector2[] points = path?.PreparedMetricPointData;
                    float[] cumulative = path?.NormalizedCumulativeLengthData;
                    if (path == null ||
                        !candidateIndexByStableId.TryGetValue(
                            path.StableId,
                            out int candidateIndex) ||
                        connectorIndexByStableId.ContainsKey(path.StableId) ||
                        points == null || points.Length < 2 ||
                        cumulative == null || cumulative.Length != points.Length)
                    {
                        allConnectorsPrepared = false;
                        break;
                    }

                    int pointOffset = pathIndex * maximumPathPointCount;
                    for (int pointIndex = 0;
                         pointIndex < points.Length;
                         pointIndex++)
                    {
                        flattenedPoints[pointOffset + pointIndex] =
                            new Vector4(
                                points[pointIndex].x,
                                points[pointIndex].y,
                                Mathf.Clamp01(cumulative[pointIndex]),
                                0f);
                    }

                    float outerRadius = Mathf.Lerp(
                        0.17f,
                        0.27f,
                        HashConnectorIdentity(path.StableId, 31u));
                    float coreRadius = outerRadius * Mathf.Lerp(
                        0.20f,
                        0.36f,
                        HashConnectorIdentity(path.StableId, 32u));
                    connectorIndexByStableId.Add(
                        path.StableId,
                        connectorRecords.Count);
                    connectorRecords.Add(new FoamConnectorIdentityData
                    {
                        PointRangeAndRadii = new Vector4(
                            pointOffset,
                            points.Length,
                            outerRadius,
                            coreRadius)
                    });
                    evolutionSlots.Add(new ConnectorEvolutionSlot
                    {
                        StableId = path.StableId,
                        OriginalCandidateIndex = candidateIndex,
                        AssignedCandidateIndex = candidateIndex,
                        ActiveCandidateIndex = candidateIndex,
                        LastReleasedCandidateIndex = -1,
                        ReleaseCooldownTicks = 0,
                        RelationshipRevision = 0,
                        PointOffset = pointOffset,
                        PointCapacity = maximumPathPointCount,
                        PointCount = points.Length,
                        ActiveStartAnchorIndex = -1,
                        ActiveEndAnchorIndex = -1,
                        PendingReleaseReason = ConnectorReleaseReason.None,
                        TurnoverFallbackCandidateIndex = -1,
                        ReferenceLengthMetres = 0f,
                        ReferenceCandidateIndex = -1,
                        ReferenceStartAnchorIndex = -2,
                        ReferenceEndAnchorIndex = -2,
                        ObservedStartRecycleCount = -1,
                        ObservedEndRecycleCount = -1,
                        StretchBlockedCandidateIndex = -1,
                        StretchBlockedStartRecycleCount = -1,
                        StretchBlockedEndRecycleCount = -1,
                        IsActive = true,
                        HasRuntimeState = false
                    });
                }
            }

            if (!allConnectorsPrepared ||
                connectorRecords.Count != acceptedConnectorCount ||
                evolutionSlots.Count != acceptedConnectorCount ||
                relationshipCandidates.Count != cataloguePaths.Count)
            {
                connectorRecords.Clear();
                evolutionSlots.Clear();
                relationshipCandidates.Clear();
                connectorIndexByStableId.Clear();
                flattenedPoints = Array.Empty<Vector4>();
            }

            connectorIdentityGpuData = connectorRecords.ToArray();
            connectorPathPointGpuData = flattenedPoints;
            connectorEvolutionSlots = evolutionSlots.ToArray();
            connectorRelationshipCandidates = relationshipCandidates.ToArray();
            connectorCandidateClaimed = new bool[
                connectorRelationshipCandidates.Length];
            connectorMajorDegree = new int[majorEvolutionSlots.Length];
            connectorPreviousMajorDegree = new int[
                majorEvolutionSlots.Length];
            connectorMajorPairClaimed = new bool[
                majorEvolutionSlots.Length * majorEvolutionSlots.Length];
            connectorIdentityReconstructionReady =
                acceptedConnectorCount > 0 &&
                connectorIdentityGpuData.Length == acceptedConnectorCount &&
                connectorEvolutionSlots.Length == acceptedConnectorCount &&
                connectorRelationshipCandidates.Length >=
                    acceptedConnectorCount &&
                maximumPathPointCount >= 2 &&
                connectorPathPointGpuData.Length ==
                    acceptedConnectorCount * maximumPathPointCount;

            IReadOnlyList<StylizedRiverFoamPreparedWeakSpanRegion> weakSpans =
                pocketTopology.PreparedWeakSpanRegions;
            int acceptedWeakSpanCount =
                pocketTopology.AcceptedConnectorWeakSpanCount;
            bool allWeakSpansPrepared =
                connectorIdentityReconstructionReady &&
                acceptedWeakSpanCount > 0 &&
                pocketTopology.PreparedWeakSpanRegionCount ==
                    acceptedWeakSpanCount &&
                weakSpans.Count == acceptedWeakSpanCount;
            List<FoamWeakSpanIdentityData> weakSpanRecords = new(
                acceptedWeakSpanCount);
            if (allWeakSpansPrepared)
            {
                for (int weakSpanIndex = 0;
                     weakSpanIndex < weakSpans.Count;
                     weakSpanIndex++)
                {
                    StylizedRiverFoamPreparedWeakSpanRegion weakSpan =
                        weakSpans[weakSpanIndex];
                    if (weakSpan == null || !weakSpan.IsAvailable ||
                        !connectorIndexByStableId.TryGetValue(
                            weakSpan.ConnectorStableId,
                            out int connectorIndex))
                    {
                        allWeakSpansPrepared = false;
                        break;
                    }

                    weakSpanRecords.Add(new FoamWeakSpanIdentityData
                    {
                        ConnectorAndPath = new Vector4(
                            connectorIndex,
                            weakSpan.NormalizedPathDistance,
                            weakSpan.MinimumNormalizedPathDistance,
                            weakSpan.MaximumNormalizedPathDistance),
                        Shape = new Vector4(
                            weakSpan.AlongRadiusMetres,
                            weakSpan.AcrossRadiusMetres,
                            weakSpan.Strength,
                            weakSpan.AcceptedOrientationRadians),
                        NoiseSeed = weakSpan.StableId ^ 0x6C8E9CF5u,
                        Reserved0 = 0u,
                        Reserved1 = 0u,
                        Reserved2 = 0u
                    });
                }
            }

            if (!allWeakSpansPrepared ||
                weakSpanRecords.Count != acceptedWeakSpanCount)
            {
                weakSpanRecords.Clear();
            }

            weakSpanIdentityGpuData = weakSpanRecords.ToArray();
            weakSpanIdentityReconstructionReady =
                weakSpanIdentityGpuData.Length == acceptedWeakSpanCount &&
                acceptedWeakSpanCount > 0;

            UpdateConnectorEvolutionDescriptors(false);
            EnsureConnectorIdentityBuffers();
            if (connectorIdentityReconstructionReady)
            {
                connectorIdentityBuffer.SetData(connectorIdentityGpuData);
                connectorPathPointBuffer.SetData(connectorPathPointGpuData);
            }
            if (weakSpanIdentityReconstructionReady)
            {
                weakSpanIdentityBuffer.SetData(weakSpanIdentityGpuData);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            connectorIdentityParityPending =
                connectorIdentityReconstructionReady &&
                IsTopologyDebugActive &&
                SystemInfo.supportsAsyncGPUReadback;
            connectorIdentityParityReadbackPending = false;
            connectorIdentityParityAvailable = false;
            connectorIdentityParityMeanDifference = 0f;
            connectorIdentityParityMaximumDifference = 0f;
            weakSpanIdentityParityPending =
                weakSpanIdentityReconstructionReady &&
                IsTopologyDebugActive &&
                SystemInfo.supportsAsyncGPUReadback;
            weakSpanIdentityParityReadbackPending = false;
            weakSpanIdentityParityAvailable = false;
            weakSpanIdentityParityMeanDifference = 0f;
            weakSpanIdentityParityMaximumDifference = 0f;
#endif

            if (rebuildField)
            {
                BuildEvolvingMajorField();
            }
        }

        private bool TryResolveMajorEvolutionSlot(
            StylizedRiverFoamConnectorEndpointBinding binding,
            out int slotIndex)
        {
            slotIndex = -1;
            if (!binding.IsAvailable)
            {
                return false;
            }

            int preferredIndex = binding.MajorPreparedIndex;
            if (preferredIndex >= 0 &&
                preferredIndex < majorEvolutionSlots.Length)
            {
                MajorEvolutionSlot preferred =
                    majorEvolutionSlots[preferredIndex];
                if (preferred.PreparedIndex == binding.MajorPreparedIndex &&
                    preferred.StableId == binding.MajorStableId)
                {
                    slotIndex = preferredIndex;
                    return true;
                }
            }

            // Preparation-only fallback for defensive index drift. Runtime
            // evolution stores the resolved slot and performs no host search.
            for (int index = 0;
                 index < majorEvolutionSlots.Length;
                 index++)
            {
                MajorEvolutionSlot candidate = majorEvolutionSlots[index];
                if (candidate.PreparedIndex == binding.MajorPreparedIndex &&
                    candidate.StableId == binding.MajorStableId)
                {
                    slotIndex = index;
                    return true;
                }
            }

            return false;
        }

        private void UpdateConnectorEvolutionDescriptors(
            bool trackTransitions)
        {
            connectorEvolutionActiveCount = 0;
            connectorEvolutionTemporaryAbsenceCount = 0;
            connectorEvolutionIdentityPathCount = 0;
            connectorEvolutionRecycleVariantCount = 0;
            connectorEvolutionOriginalRelationshipCount = 0;
            connectorEvolutionReplacementRelationshipCount = 0;
            weakSpanEvolutionActiveCount = 0;

            if (!connectorIdentityReconstructionReady ||
                connectorEvolutionSlots.Length !=
                    connectorIdentityGpuData.Length ||
                connectorRelationshipCandidates.Length == 0 ||
                connectorCandidateClaimed.Length !=
                    connectorRelationshipCandidates.Length ||
                connectorMajorDegree.Length != majorEvolutionSlots.Length ||
                connectorPreviousMajorDegree.Length !=
                    majorEvolutionSlots.Length ||
                connectorMajorPairClaimed.Length !=
                    majorEvolutionSlots.Length * majorEvolutionSlots.Length ||
                majorTopology == null || river == null ||
                !river.Domain.IsValid)
            {
                return;
            }

            Array.Clear(
                connectorPreviousMajorDegree,
                0,
                connectorPreviousMajorDegree.Length);
            for (int connectorIndex = 0;
                 connectorIndex < connectorEvolutionSlots.Length;
                 connectorIndex++)
            {
                ConnectorEvolutionSlot previousSlot =
                    connectorEvolutionSlots[connectorIndex];
                int previousCandidateIndex =
                    previousSlot.AssignedCandidateIndex;
                if (!previousSlot.IsActive || previousCandidateIndex < 0 ||
                    previousCandidateIndex >=
                        connectorRelationshipCandidates.Length)
                {
                    continue;
                }

                ConnectorRelationshipCandidate previousCandidate =
                    connectorRelationshipCandidates[
                        previousCandidateIndex];
                int previousStartHost =
                    previousCandidate.StartHostSlotIndex;
                int previousEndHost = previousCandidate.EndHostSlotIndex;
                if (previousStartHost >= 0 &&
                    previousStartHost <
                        connectorPreviousMajorDegree.Length &&
                    previousEndHost >= 0 &&
                    previousEndHost <
                        connectorPreviousMajorDegree.Length &&
                    previousStartHost != previousEndHost)
                {
                    connectorPreviousMajorDegree[previousStartHost]++;
                    connectorPreviousMajorDegree[previousEndHost]++;
                }
            }

            Array.Clear(
                connectorCandidateClaimed,
                0,
                connectorCandidateClaimed.Length);
            Array.Clear(
                connectorMajorDegree,
                0,
                connectorMajorDegree.Length);
            Array.Clear(
                connectorMajorPairClaimed,
                0,
                connectorMajorPairClaimed.Length);

            for (int connectorIndex = 0;
                 connectorIndex < connectorEvolutionSlots.Length;
                 connectorIndex++)
            {
                ref ConnectorEvolutionSlot slot =
                    ref connectorEvolutionSlots[connectorIndex];
                slot.PendingReleaseReason = ConnectorReleaseReason.None;
                slot.TurnoverFallbackCandidateIndex = -1;
                RefreshConnectorStretchBlock(ref slot);
            }

            // Preserve valid current relationships first, except when a host
            // recycle makes the deterministic turnover decision request a new
            // pair or the live path exceeds its assignment-relative stretch
            // limit. Those releases are handled in a dedicated second pass so
            // retained relationships cannot be stolen by replacement slots.
            for (int connectorIndex = 0;
                 connectorIndex < connectorEvolutionSlots.Length;
                 connectorIndex++)
            {
                ref ConnectorEvolutionSlot slot =
                    ref connectorEvolutionSlots[connectorIndex];
                int candidateIndex = slot.AssignedCandidateIndex;
                if (candidateIndex < 0)
                {
                    continue;
                }

                bool stateAvailable = TryResolveConnectorCandidateState(
                    candidateIndex,
                    out int startAnchorIndex,
                    out int endAnchorIndex,
                    out Vector2[] basePoints,
                    out float[] baseCumulative,
                    out Vector2 startGate,
                    out Vector2 endGate);
                if (!stateAvailable)
                {
                    ReleaseConnectorSlotForRebind(
                        ref slot,
                        candidateIndex,
                        ConnectorReleaseReason.Unavailable,
                        false);
                    continue;
                }

                ConnectorRelationshipCandidate candidate =
                    connectorRelationshipCandidates[candidateIndex];
                bool referenceMatchesCurrentState =
                    slot.ReferenceLengthMetres > 0.0001f &&
                    slot.ReferenceCandidateIndex == candidateIndex &&
                    slot.ReferenceStartAnchorIndex == startAnchorIndex &&
                    slot.ReferenceEndAnchorIndex == endAnchorIndex;
                if (referenceMatchesCurrentState)
                {
                    bool measured = TryMeasureConnectorDeformedPath(
                        slot,
                        candidate,
                        basePoints,
                        baseCumulative,
                        startGate,
                        endGate,
                        out float currentLengthMetres);
                    float stretchLimit = slot.ReferenceLengthMetres *
                        river.FoamConnectorBreakStretchRatio;
                    if (!measured ||
                        currentLengthMetres > stretchLimit + 0.0001f)
                    {
                        if (measured)
                        {
                            RecordConnectorStretchBlock(
                                ref slot,
                                candidateIndex);
                            if (trackTransitions)
                            {
                                connectorEvolutionStretchBreakCount++;
                            }
                        }
                        ReleaseConnectorSlotForRebind(
                            ref slot,
                            candidateIndex,
                            measured
                                ? ConnectorReleaseReason.StretchBreak
                                : ConnectorReleaseReason.Unavailable,
                            false);
                        continue;
                    }
                }

                int startRecycleCount = majorEvolutionSlots[
                    candidate.StartHostSlotIndex].RecycleCount;
                int endRecycleCount = majorEvolutionSlots[
                    candidate.EndHostSlotIndex].RecycleCount;
                bool sameObservedRelationship = slot.HasRuntimeState &&
                    slot.ActiveCandidateIndex == candidateIndex &&
                    slot.ObservedStartRecycleCount >= 0 &&
                    slot.ObservedEndRecycleCount >= 0;
                bool hostRecycled = sameObservedRelationship &&
                    (slot.ObservedStartRecycleCount != startRecycleCount ||
                     slot.ObservedEndRecycleCount != endRecycleCount);
                if (hostRecycled)
                {
                    bool requestTurnover =
                        ShouldRequestConnectorRelationshipTurnover(
                            slot,
                            candidateIndex,
                            startRecycleCount,
                            endRecycleCount,
                            out bool crowdingBoostedTurnover);
                    if (requestTurnover)
                    {
                        if (trackTransitions)
                        {
                            connectorEvolutionTurnoverRequestCount++;
                            if (crowdingBoostedTurnover)
                            {
                                connectorEvolutionCrowdingBoostedTurnoverCount++;
                            }
                        }
                        ReleaseConnectorSlotForRebind(
                            ref slot,
                            candidateIndex,
                            ConnectorReleaseReason.Turnover,
                            true);
                        continue;
                    }

                    if (trackTransitions)
                    {
                        connectorEvolutionRetainDecisionCount++;
                    }
                }

                if (!TryClaimConnectorCandidate(candidateIndex))
                {
                    ReleaseConnectorSlotForRebind(
                        ref slot,
                        candidateIndex,
                        ConnectorReleaseReason.Unavailable,
                        false);
                }
            }

            // Turnover requests and stretch breaks explicitly exclude the old
            // Major pair. A requested turnover may retain its old relationship
            // only when no different prepared pair is currently available and
            // the old state remains valid. Stretch-broken relationships never
            // fall back to the path that just exceeded its limit.
            for (int connectorIndex = 0;
                 connectorIndex < connectorEvolutionSlots.Length;
                 connectorIndex++)
            {
                ref ConnectorEvolutionSlot slot =
                    ref connectorEvolutionSlots[connectorIndex];
                bool turnoverRequested = slot.PendingReleaseReason ==
                    ConnectorReleaseReason.Turnover;
                bool stretchBroken = slot.PendingReleaseReason ==
                    ConnectorReleaseReason.StretchBreak;
                if (!turnoverRequested && !stretchBroken)
                {
                    continue;
                }

                int releasedCandidateIndex =
                    slot.TurnoverFallbackCandidateIndex >= 0
                        ? slot.TurnoverFallbackCandidateIndex
                        : slot.LastReleasedCandidateIndex;
                int selectedCandidate = SelectConnectorReplacementCandidate(
                    slot,
                    releasedCandidateIndex,
                    true);
                if (selectedCandidate >= 0)
                {
                    slot.AssignedCandidateIndex = selectedCandidate;
                    slot.RelationshipRevision++;
                    continue;
                }

                if (turnoverRequested && releasedCandidateIndex >= 0 &&
                    TryResolveConnectorCandidateState(
                        releasedCandidateIndex,
                        out _,
                        out _,
                        out _,
                        out _,
                        out _,
                        out _) &&
                    TryClaimConnectorCandidate(
                        releasedCandidateIndex))
                {
                    slot.AssignedCandidateIndex = releasedCandidateIndex;
                }
            }

            // Slots released because their previous state became unavailable,
            // plus slots that were already absent, may claim any currently
            // valid prepared relationship. Directed turnover/stretch releases
            // have already had their one bounded selection attempt above.
            for (int connectorIndex = 0;
                 connectorIndex < connectorEvolutionSlots.Length;
                 connectorIndex++)
            {
                ref ConnectorEvolutionSlot slot =
                    ref connectorEvolutionSlots[connectorIndex];
                if (slot.AssignedCandidateIndex >= 0)
                {
                    continue;
                }
                if (slot.PendingReleaseReason ==
                        ConnectorReleaseReason.Turnover ||
                    slot.PendingReleaseReason ==
                        ConnectorReleaseReason.StretchBreak)
                {
                    continue;
                }

                int selectedCandidate =
                    SelectConnectorReplacementCandidate(
                        slot,
                        -1,
                        false);
                if (selectedCandidate >= 0)
                {
                    slot.AssignedCandidateIndex = selectedCandidate;
                    slot.RelationshipRevision++;
                }
            }

            for (int connectorIndex = 0;
                 connectorIndex < connectorEvolutionSlots.Length;
                 connectorIndex++)
            {
                ref ConnectorEvolutionSlot slot =
                    ref connectorEvolutionSlots[connectorIndex];
                if (slot.ReleaseCooldownTicks > 0)
                {
                    slot.ReleaseCooldownTicks--;
                }

                bool previousActive = slot.IsActive;
                int previousCandidateIndex = slot.ActiveCandidateIndex;
                int previousStartAnchorIndex =
                    slot.ActiveStartAnchorIndex;
                int previousEndAnchorIndex = slot.ActiveEndAnchorIndex;

                int assignedCandidateIndex =
                    slot.AssignedCandidateIndex;
                int startAnchorIndex = -1;
                int endAnchorIndex = -1;
                Vector2[] basePoints = null;
                float[] baseCumulative = null;
                Vector2 startGate = Vector2.zero;
                Vector2 endGate = Vector2.zero;
                float currentLengthMetres = 0f;
                bool active = assignedCandidateIndex >= 0 &&
                    TryResolveConnectorCandidateState(
                        assignedCandidateIndex,
                        out startAnchorIndex,
                        out endAnchorIndex,
                        out basePoints,
                        out baseCumulative,
                        out startGate,
                        out endGate) &&
                    basePoints.Length <= slot.PointCapacity &&
                    WriteConnectorDeformedPath(
                        slot,
                        connectorRelationshipCandidates[
                            assignedCandidateIndex],
                        basePoints,
                        baseCumulative,
                        startGate,
                        endGate,
                        out currentLengthMetres);

                if (!active)
                {
                    startAnchorIndex = -1;
                    endAnchorIndex = -1;
                    if (assignedCandidateIndex >= 0)
                    {
                        ReleaseConnectorSlotForRebind(
                            ref slot,
                            assignedCandidateIndex,
                            ConnectorReleaseReason.Unavailable,
                            false);
                    }
                }
                else
                {
                    slot.PointCount = basePoints.Length;
                    bool referenceStateChanged =
                        slot.ReferenceLengthMetres <= 0.0001f ||
                        slot.ReferenceCandidateIndex !=
                            assignedCandidateIndex ||
                        slot.ReferenceStartAnchorIndex !=
                            startAnchorIndex ||
                        slot.ReferenceEndAnchorIndex != endAnchorIndex;
                    if (referenceStateChanged)
                    {
                        slot.ReferenceLengthMetres = currentLengthMetres;
                        slot.ReferenceCandidateIndex =
                            assignedCandidateIndex;
                        slot.ReferenceStartAnchorIndex = startAnchorIndex;
                        slot.ReferenceEndAnchorIndex = endAnchorIndex;
                    }
                }

                if (trackTransitions && active &&
                    slot.PendingReleaseReason ==
                        ConnectorReleaseReason.Turnover)
                {
                    if (assignedCandidateIndex ==
                        slot.TurnoverFallbackCandidateIndex)
                    {
                        connectorEvolutionNoAlternativeFallbackCount++;
                    }
                    else
                    {
                        connectorEvolutionSuccessfulTurnoverCount++;
                    }
                }

                FoamConnectorIdentityData record =
                    connectorIdentityGpuData[connectorIndex];
                record.PointRangeAndRadii.y = active
                    ? slot.PointCount
                    : 0f;
                connectorIdentityGpuData[connectorIndex] = record;

                if (trackTransitions && slot.HasRuntimeState)
                {
                    bool relationshipChanged = active &&
                        previousCandidateIndex >= 0 &&
                        previousCandidateIndex != assignedCandidateIndex;
                    bool anchorCombinationChanged = active &&
                        previousCandidateIndex == assignedCandidateIndex &&
                        (previousStartAnchorIndex != startAnchorIndex ||
                         previousEndAnchorIndex != endAnchorIndex);
                    if (relationshipChanged)
                    {
                        connectorEvolutionRelationshipRebindCount++;
                    }
                    if (anchorCombinationChanged)
                    {
                        connectorEvolutionVariantSwitchCount++;
                    }
                    if (previousActive && !active)
                    {
                        connectorEvolutionAbsenceEventCount++;
                    }
                    else if (!previousActive && active)
                    {
                        connectorEvolutionReappearanceCount++;
                    }
                }

                if (active)
                {
                    slot.ActiveCandidateIndex = assignedCandidateIndex;
                    slot.ActiveStartAnchorIndex = startAnchorIndex;
                    slot.ActiveEndAnchorIndex = endAnchorIndex;
                    ConnectorRelationshipCandidate activeCandidate =
                        connectorRelationshipCandidates[
                            assignedCandidateIndex];
                    slot.ObservedStartRecycleCount = majorEvolutionSlots[
                        activeCandidate.StartHostSlotIndex].RecycleCount;
                    slot.ObservedEndRecycleCount = majorEvolutionSlots[
                        activeCandidate.EndHostSlotIndex].RecycleCount;
                }
                slot.IsActive = active;
                slot.HasRuntimeState = true;

                if (active)
                {
                    connectorEvolutionActiveCount++;
                    bool usesRecycleVariant =
                        startAnchorIndex >= 0 || endAnchorIndex >= 0;
                    if (usesRecycleVariant)
                    {
                        connectorEvolutionRecycleVariantCount++;
                    }
                    else
                    {
                        connectorEvolutionIdentityPathCount++;
                    }

                    if (assignedCandidateIndex ==
                        slot.OriginalCandidateIndex)
                    {
                        connectorEvolutionOriginalRelationshipCount++;
                    }
                    else
                    {
                        connectorEvolutionReplacementRelationshipCount++;
                    }
                }
                else
                {
                    connectorEvolutionTemporaryAbsenceCount++;
                }
            }

            UpdateConnectorDegreeTelemetry();

            if (!weakSpanIdentityReconstructionReady)
            {
                return;
            }

            for (int weakSpanIndex = 0;
                 weakSpanIndex < weakSpanIdentityGpuData.Length;
                 weakSpanIndex++)
            {
                int connectorIndex = Mathf.RoundToInt(
                    weakSpanIdentityGpuData[weakSpanIndex]
                        .ConnectorAndPath.x);
                if (connectorIndex >= 0 &&
                    connectorIndex < connectorEvolutionSlots.Length &&
                    connectorEvolutionSlots[connectorIndex].IsActive)
                {
                    weakSpanEvolutionActiveCount++;
                }
            }
        }

        private static float MeasureConnectorPreparedPathLength(
            IReadOnlyList<Vector2> points)
        {
            if (points == null || points.Count < 2)
            {
                return 0f;
            }

            float length = 0f;
            for (int pointIndex = 1;
                 pointIndex < points.Count;
                 pointIndex++)
            {
                length += Vector2.Distance(
                    points[pointIndex - 1],
                    points[pointIndex]);
            }
            return length;
        }

        private void ResolveConnectorRelationshipSelectionWeights(
            List<ConnectorRelationshipCandidate> candidates)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return;
            }

            float minimumLength = float.PositiveInfinity;
            float maximumLength = 0f;
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                float length = Mathf.Max(
                    0f,
                    candidates[candidateIndex].BasePathLengthMetres);
                minimumLength = Mathf.Min(minimumLength, length);
                maximumLength = Mathf.Max(maximumLength, length);
            }

            float centredLengthPreference = river != null
                ? (Mathf.Clamp01(river.FoamConnectorLengthPreference) -
                   0.5f) * 2f
                : 0f;
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                ConnectorRelationshipCandidate candidate =
                    candidates[candidateIndex];
                float normalizedLength = maximumLength >
                        minimumLength + 0.0001f
                    ? Mathf.InverseLerp(
                        minimumLength,
                        maximumLength,
                        candidate.BasePathLengthMetres)
                    : 0.5f;
                float lengthPreferenceWeight = Mathf.Exp(
                    centredLengthPreference *
                    (normalizedLength - 0.5f) *
                    ConnectorLengthPreferenceWeightStrength);
                float deterministicJitter = Mathf.Lerp(
                    ConnectorSelectionJitterMinimum,
                    ConnectorSelectionJitterMaximum,
                    HashConnectorIdentity(candidate.Path.StableId, 173u));
                candidate.SelectionWeight = Mathf.Max(
                    0.0001f,
                    lengthPreferenceWeight * deterministicJitter);
                candidates[candidateIndex] = candidate;
            }
        }

        private void UpdateConnectorDegreeTelemetry()
        {
            connectorEvolutionMajorDegreeZeroCount = 0;
            connectorEvolutionMajorDegreeOneCount = 0;
            connectorEvolutionMajorDegreeTwoCount = 0;
            connectorEvolutionMajorDegreeThreePlusCount = 0;
            connectorEvolutionMaximumMajorDegree = 0;

            for (int majorIndex = 0;
                 majorIndex < connectorMajorDegree.Length;
                 majorIndex++)
            {
                int degree = connectorMajorDegree[majorIndex];
                connectorEvolutionMaximumMajorDegree = Mathf.Max(
                    connectorEvolutionMaximumMajorDegree,
                    degree);
                if (degree <= 0)
                {
                    connectorEvolutionMajorDegreeZeroCount++;
                }
                else if (degree == 1)
                {
                    connectorEvolutionMajorDegreeOneCount++;
                }
                else if (degree == 2)
                {
                    connectorEvolutionMajorDegreeTwoCount++;
                }
                else
                {
                    connectorEvolutionMajorDegreeThreePlusCount++;
                }
            }
        }

        private void RefreshConnectorStretchBlock(
            ref ConnectorEvolutionSlot slot)
        {
            int candidateIndex = slot.StretchBlockedCandidateIndex;
            if (candidateIndex < 0 ||
                candidateIndex >= connectorRelationshipCandidates.Length)
            {
                ClearConnectorStretchBlock(ref slot);
                return;
            }

            ConnectorRelationshipCandidate candidate =
                connectorRelationshipCandidates[candidateIndex];
            if (candidate.StartHostSlotIndex < 0 ||
                candidate.StartHostSlotIndex >= majorEvolutionSlots.Length ||
                candidate.EndHostSlotIndex < 0 ||
                candidate.EndHostSlotIndex >= majorEvolutionSlots.Length ||
                majorEvolutionSlots[candidate.StartHostSlotIndex]
                    .RecycleCount !=
                    slot.StretchBlockedStartRecycleCount ||
                majorEvolutionSlots[candidate.EndHostSlotIndex]
                    .RecycleCount !=
                    slot.StretchBlockedEndRecycleCount)
            {
                ClearConnectorStretchBlock(ref slot);
            }
        }

        private void RecordConnectorStretchBlock(
            ref ConnectorEvolutionSlot slot,
            int candidateIndex)
        {
            if (candidateIndex < 0 ||
                candidateIndex >= connectorRelationshipCandidates.Length)
            {
                ClearConnectorStretchBlock(ref slot);
                return;
            }

            ConnectorRelationshipCandidate candidate =
                connectorRelationshipCandidates[candidateIndex];
            if (candidate.StartHostSlotIndex < 0 ||
                candidate.StartHostSlotIndex >= majorEvolutionSlots.Length ||
                candidate.EndHostSlotIndex < 0 ||
                candidate.EndHostSlotIndex >= majorEvolutionSlots.Length)
            {
                ClearConnectorStretchBlock(ref slot);
                return;
            }

            slot.StretchBlockedCandidateIndex = candidateIndex;
            slot.StretchBlockedStartRecycleCount = majorEvolutionSlots[
                candidate.StartHostSlotIndex].RecycleCount;
            slot.StretchBlockedEndRecycleCount = majorEvolutionSlots[
                candidate.EndHostSlotIndex].RecycleCount;
        }

        private static void ClearConnectorStretchBlock(
            ref ConnectorEvolutionSlot slot)
        {
            slot.StretchBlockedCandidateIndex = -1;
            slot.StretchBlockedStartRecycleCount = -1;
            slot.StretchBlockedEndRecycleCount = -1;
        }

        private static void ReleaseConnectorSlotForRebind(
            ref ConnectorEvolutionSlot slot,
            int candidateIndex,
            ConnectorReleaseReason releaseReason,
            bool allowTurnoverFallback)
        {
            if (candidateIndex >= 0)
            {
                slot.LastReleasedCandidateIndex = candidateIndex;
                slot.ReleaseCooldownTicks = 1;
            }
            slot.AssignedCandidateIndex = -1;
            slot.PendingReleaseReason = releaseReason;
            slot.TurnoverFallbackCandidateIndex =
                allowTurnoverFallback ? candidateIndex : -1;
            ClearConnectorReferenceLength(ref slot);
        }

        private static void ClearConnectorReferenceLength(
            ref ConnectorEvolutionSlot slot)
        {
            slot.ReferenceLengthMetres = 0f;
            slot.ReferenceCandidateIndex = -1;
            slot.ReferenceStartAnchorIndex = -2;
            slot.ReferenceEndAnchorIndex = -2;
        }

        private bool ShouldRequestConnectorRelationshipTurnover(
            ConnectorEvolutionSlot slot,
            int candidateIndex,
            int startRecycleCount,
            int endRecycleCount,
            out bool crowdingBoostedTurnover)
        {
            crowdingBoostedTurnover = false;
            if (candidateIndex < 0 ||
                candidateIndex >= connectorRelationshipCandidates.Length)
            {
                return false;
            }

            ConnectorRelationshipCandidate candidate =
                connectorRelationshipCandidates[candidateIndex];
            int startDegree = candidate.StartHostSlotIndex >= 0 &&
                candidate.StartHostSlotIndex <
                    connectorPreviousMajorDegree.Length
                    ? connectorPreviousMajorDegree[
                        candidate.StartHostSlotIndex]
                    : 0;
            int endDegree = candidate.EndHostSlotIndex >= 0 &&
                candidate.EndHostSlotIndex <
                    connectorPreviousMajorDegree.Length
                    ? connectorPreviousMajorDegree[
                        candidate.EndHostSlotIndex]
                    : 0;
            int excessDegree = Mathf.Max(0, startDegree - 1) +
                Mathf.Max(0, endDegree - 1);
            float turnoverProbability = Mathf.Min(
                ConnectorTurnoverMaximumProbability,
                ConnectorTurnoverBaseProbability +
                excessDegree * ConnectorTurnoverCrowdingIncrement);

            uint stream;
            unchecked
            {
                stream = 151u ^
                    ((uint)(candidateIndex + 1) * 0x9E3779B9u) ^
                    ((uint)(startRecycleCount + 1) * 0x85EBCA6Bu) ^
                    ((uint)(endRecycleCount + 1) * 0xC2B2AE35u) ^
                    ((uint)(slot.RelationshipRevision + 1) * 0x27D4EB2Du);
            }
            float sample = HashConnectorIdentity(slot.StableId, stream);
            crowdingBoostedTurnover =
                sample >= ConnectorTurnoverBaseProbability &&
                sample < turnoverProbability;
            return sample < turnoverProbability;
        }

        private int SelectConnectorReplacementCandidate(
            ConnectorEvolutionSlot slot,
            int excludedCandidateIndex,
            bool excludeSameMajorPair)
        {
            int candidateCount = connectorRelationshipCandidates.Length;
            if (candidateCount <= 0)
            {
                return -1;
            }

            double totalWeight = 0.0;
            for (int candidateIndex = 0;
                 candidateIndex < candidateCount;
                 candidateIndex++)
            {
                if (!TryResolveConnectorSelectionWeight(
                        slot,
                        candidateIndex,
                        excludedCandidateIndex,
                        excludeSameMajorPair,
                        out float candidateWeight))
                {
                    continue;
                }

                totalWeight += candidateWeight;
            }

            if (totalWeight <= 0.0)
            {
                return -1;
            }

            uint stream;
            unchecked
            {
                stream = 211u ^
                    ((uint)(slot.RelationshipRevision + 1) * 0x9E3779B9u) ^
                    ((uint)(excludedCandidateIndex + 2) * 0x85EBCA6Bu) ^
                    (excludeSameMajorPair ? 0xC2B2AE35u : 0u);
            }
            double threshold = HashConnectorIdentity(
                slot.StableId,
                stream) * totalWeight;
            double cumulativeWeight = 0.0;
            int fallbackIndex = -1;
            for (int candidateIndex = 0;
                 candidateIndex < candidateCount;
                 candidateIndex++)
            {
                if (!TryResolveConnectorSelectionWeight(
                        slot,
                        candidateIndex,
                        excludedCandidateIndex,
                        excludeSameMajorPair,
                        out float candidateWeight))
                {
                    continue;
                }

                fallbackIndex = candidateIndex;
                cumulativeWeight += candidateWeight;
                if (threshold <= cumulativeWeight)
                {
                    return TryClaimConnectorCandidate(candidateIndex)
                        ? candidateIndex
                        : -1;
                }
            }

            return fallbackIndex >= 0 &&
                TryClaimConnectorCandidate(fallbackIndex)
                    ? fallbackIndex
                    : -1;
        }

        private bool TryResolveConnectorSelectionWeight(
            ConnectorEvolutionSlot slot,
            int candidateIndex,
            int excludedCandidateIndex,
            bool excludeSameMajorPair,
            out float weight)
        {
            weight = 0f;
            if (candidateIndex < 0 ||
                candidateIndex >= connectorRelationshipCandidates.Length ||
                candidateIndex == excludedCandidateIndex ||
                (slot.StretchBlockedCandidateIndex >= 0 &&
                 IsSameConnectorMajorPair(
                     candidateIndex,
                     slot.StretchBlockedCandidateIndex)) ||
                (excludeSameMajorPair &&
                 IsSameConnectorMajorPair(
                     candidateIndex,
                     excludedCandidateIndex)) ||
                (slot.ReleaseCooldownTicks > 0 &&
                 candidateIndex == slot.LastReleasedCandidateIndex) ||
                !CanClaimConnectorCandidate(candidateIndex) ||
                !TryResolveConnectorCandidateState(
                    candidateIndex,
                    out _,
                    out _,
                    out _,
                    out _,
                    out _,
                    out _))
            {
                return false;
            }

            ConnectorRelationshipCandidate candidate =
                connectorRelationshipCandidates[candidateIndex];
            int startDegree = connectorMajorDegree[
                candidate.StartHostSlotIndex];
            int endDegree = connectorMajorDegree[
                candidate.EndHostSlotIndex];
            int combinedDegree = startDegree + endDegree;
            int maximumDegree = Mathf.Max(startDegree, endDegree);
            float loadWeight = Mathf.Pow(
                ConnectorLoadPenaltyBase,
                combinedDegree);
            float hubWeight = Mathf.Pow(
                ConnectorHubPenaltyBase,
                maximumDegree);
            weight = Mathf.Max(
                0.0000001f,
                candidate.SelectionWeight * loadWeight * hubWeight);
            return true;
        }

        private bool IsSameConnectorMajorPair(
            int firstCandidateIndex,
            int secondCandidateIndex)
        {
            if (firstCandidateIndex < 0 ||
                firstCandidateIndex >=
                    connectorRelationshipCandidates.Length ||
                secondCandidateIndex < 0 ||
                secondCandidateIndex >=
                    connectorRelationshipCandidates.Length)
            {
                return false;
            }

            ConnectorRelationshipCandidate first =
                connectorRelationshipCandidates[firstCandidateIndex];
            ConnectorRelationshipCandidate second =
                connectorRelationshipCandidates[secondCandidateIndex];
            int firstLow = Mathf.Min(
                first.StartHostSlotIndex,
                first.EndHostSlotIndex);
            int firstHigh = Mathf.Max(
                first.StartHostSlotIndex,
                first.EndHostSlotIndex);
            int secondLow = Mathf.Min(
                second.StartHostSlotIndex,
                second.EndHostSlotIndex);
            int secondHigh = Mathf.Max(
                second.StartHostSlotIndex,
                second.EndHostSlotIndex);
            return firstLow == secondLow && firstHigh == secondHigh;
        }

        private bool CanClaimConnectorCandidate(int candidateIndex)
        {
            if (candidateIndex < 0 ||
                candidateIndex >= connectorRelationshipCandidates.Length ||
                connectorCandidateClaimed[candidateIndex])
            {
                return false;
            }

            ConnectorRelationshipCandidate candidate =
                connectorRelationshipCandidates[candidateIndex];
            int startHost = candidate.StartHostSlotIndex;
            int endHost = candidate.EndHostSlotIndex;
            if (startHost < 0 || startHost >= connectorMajorDegree.Length ||
                endHost < 0 || endHost >= connectorMajorDegree.Length ||
                startHost == endHost)
            {
                return false;
            }

            int lowHost = Mathf.Min(startHost, endHost);
            int highHost = Mathf.Max(startHost, endHost);
            int pairIndex = lowHost * connectorMajorDegree.Length + highHost;
            return pairIndex >= 0 &&
                pairIndex < connectorMajorPairClaimed.Length &&
                !connectorMajorPairClaimed[pairIndex];
        }

        private bool TryClaimConnectorCandidate(int candidateIndex)
        {
            if (!CanClaimConnectorCandidate(candidateIndex))
            {
                return false;
            }

            ConnectorRelationshipCandidate candidate =
                connectorRelationshipCandidates[candidateIndex];
            int startHost = candidate.StartHostSlotIndex;
            int endHost = candidate.EndHostSlotIndex;
            int lowHost = Mathf.Min(startHost, endHost);
            int highHost = Mathf.Max(startHost, endHost);
            int pairIndex = lowHost * connectorMajorDegree.Length + highHost;

            connectorCandidateClaimed[candidateIndex] = true;
            connectorMajorPairClaimed[pairIndex] = true;
            connectorMajorDegree[startHost]++;
            connectorMajorDegree[endHost]++;
            return true;
        }

        private bool TryResolveConnectorCandidateState(
            int candidateIndex,
            out int startAnchorIndex,
            out int endAnchorIndex,
            out Vector2[] metricPoints,
            out float[] normalizedCumulativeLength,
            out Vector2 startGate,
            out Vector2 endGate)
        {
            startAnchorIndex = -1;
            endAnchorIndex = -1;
            metricPoints = null;
            normalizedCumulativeLength = null;
            startGate = Vector2.zero;
            endGate = Vector2.zero;
            if (candidateIndex < 0 ||
                candidateIndex >= connectorRelationshipCandidates.Length)
            {
                return false;
            }

            ConnectorRelationshipCandidate candidate =
                connectorRelationshipCandidates[candidateIndex];
            if (candidate.Path == null ||
                candidate.StartHostSlotIndex < 0 ||
                candidate.StartHostSlotIndex >= majorEvolutionSlots.Length ||
                candidate.EndHostSlotIndex < 0 ||
                candidate.EndHostSlotIndex >= majorEvolutionSlots.Length)
            {
                return false;
            }

            startAnchorIndex = majorEvolutionSlots[
                candidate.StartHostSlotIndex].LastAnchorIndex;
            endAnchorIndex = majorEvolutionSlots[
                candidate.EndHostSlotIndex].LastAnchorIndex;
            return TryResolveConnectorBasePath(
                    candidate.Path,
                    startAnchorIndex,
                    endAnchorIndex,
                    out metricPoints,
                    out normalizedCumulativeLength) &&
                TryResolveConnectorEndpointGate(
                    candidate.Path.StartEndpointBinding,
                    candidate.StartHostSlotIndex,
                    out startGate) &&
                TryResolveConnectorEndpointGate(
                    candidate.Path.EndEndpointBinding,
                    candidate.EndHostSlotIndex,
                    out endGate);
        }

        private static bool TryResolveConnectorBasePath(
            StylizedRiverFoamConnectorPath path,
            int startAnchorIndex,
            int endAnchorIndex,
            out Vector2[] metricPoints,
            out float[] normalizedCumulativeLength)
        {
            if (path == null)
            {
                metricPoints = null;
                normalizedCumulativeLength = null;
                return false;
            }

            return path.TryResolvePreparedPath(
                startAnchorIndex,
                endAnchorIndex,
                out metricPoints,
                out normalizedCumulativeLength);
        }

        private bool TryResolveConnectorEndpointGate(
            StylizedRiverFoamConnectorEndpointBinding binding,
            int hostSlotIndex,
            out Vector2 metricPosition)
        {
            metricPosition = binding.AcceptedMetricPosition;
            if (!binding.IsAvailable || hostSlotIndex < 0 ||
                hostSlotIndex >= majorEvolutionSlots.Length ||
                majorTopology == null || river == null ||
                !river.Domain.IsValid)
            {
                return false;
            }

            MajorEvolutionSlot host = majorEvolutionSlots[hostSlotIndex];
            if (host.StableId != binding.MajorStableId ||
                host.PreparedIndex != binding.MajorPreparedIndex ||
                host.PreparedIndex < 0 ||
                host.PreparedIndex >= majorTopology.PreparedRegions.Count)
            {
                return false;
            }

            StylizedRiverFoamPreparedMajorRegion prepared =
                majorTopology.PreparedRegions[host.PreparedIndex];
            MajorEvolutionPose pose = ResolveMajorPose(host);
            Vector2 sourceOffset = ResolveConnectorPreWarpLocalOffset(
                binding.MajorLocalOffsetCells,
                pose,
                prepared);

            float principalMinorMetres = sourceOffset.y *
                pose.MetresPerCandidateCell * pose.ScaleAcross;
            float principalMajorMetres =
                (sourceOffset.x + sourceOffset.y * pose.Shear) *
                pose.MetresPerCandidateCell * pose.ScaleAlong;
            float orientationCosine = Mathf.Cos(pose.OrientationRadians);
            float orientationSine = Mathf.Sin(pose.OrientationRadians);
            float deltaAlong =
                orientationCosine * principalMajorMetres -
                orientationSine * principalMinorMetres;
            float deltaAcross =
                orientationSine * principalMajorMetres +
                orientationCosine * principalMinorMetres;
            float localDistance = pose.LocalDistance + deltaAlong;
            if (localDistance < 0f || localDistance > validFieldLength)
            {
                return false;
            }

            StylizedRiverSplineSample sample =
                river.Domain.SampleAtOrientedDistance(Mathf.Clamp(
                    localDistance,
                    0f,
                    river.Domain.LocalLength));
            float centreAcrossMetres =
                StylizedRiverFoamTopologyFieldSpace.SignedNormalizedToMetres(
                    pose.AcrossNormalized,
                    Mathf.Max(0.05f, sample.LeftHalfWidth),
                    Mathf.Max(0.05f, sample.RightHalfWidth));
            metricPosition = new Vector2(
                localDistance,
                centreAcrossMetres + deltaAcross);
            return true;
        }

        private static Vector2 ResolveConnectorPreWarpLocalOffset(
            Vector2 desiredLocalOffset,
            MajorEvolutionPose pose,
            StylizedRiverFoamPreparedMajorRegion prepared)
        {
            Vector2 source = desiredLocalOffset;
            float majorExtent = Mathf.Max(
                0.5f,
                prepared.MajorHalfExtentCells);
            float minorExtent = Mathf.Max(
                0.5f,
                prepared.MinorHalfExtentCells);
            for (int iteration = 0;
                 iteration < ConnectorEndpointWarpSolveIterations;
                 iteration++)
            {
                float normalMajor = source.x / majorExtent;
                float normalMinor = source.y / minorExtent;
                float majorWarp =
                    Mathf.Sin(normalMinor * 3.35f + pose.WarpPhaseA) *
                        pose.WarpAlong * majorExtent +
                    Mathf.Sin(
                        (normalMajor + normalMinor) * 1.85f +
                        pose.WarpPhaseB) *
                        pose.WarpAlong * majorExtent * 0.42f;
                float minorWarp =
                    Mathf.Sin(normalMajor * 2.80f + pose.WarpPhaseB) *
                        pose.WarpAcross * minorExtent +
                    Mathf.Sin(
                        (normalMajor - normalMinor) * 2.10f +
                        pose.WarpPhaseA) *
                        pose.WarpAcross * minorExtent * 0.36f;
                source = desiredLocalOffset -
                    new Vector2(majorWarp, minorWarp);
            }

            return source;
        }

        private bool WriteConnectorDeformedPath(
            ConnectorEvolutionSlot slot,
            ConnectorRelationshipCandidate candidate,
            Vector2[] basePoints,
            float[] baseCumulative,
            Vector2 startGate,
            Vector2 endGate,
            out float pathLengthMetres)
        {
            return EvaluateConnectorDeformedPath(
                slot,
                candidate,
                basePoints,
                baseCumulative,
                startGate,
                endGate,
                true,
                out pathLengthMetres);
        }

        private bool TryMeasureConnectorDeformedPath(
            ConnectorEvolutionSlot slot,
            ConnectorRelationshipCandidate candidate,
            Vector2[] basePoints,
            float[] baseCumulative,
            Vector2 startGate,
            Vector2 endGate,
            out float pathLengthMetres)
        {
            return EvaluateConnectorDeformedPath(
                slot,
                candidate,
                basePoints,
                baseCumulative,
                startGate,
                endGate,
                false,
                out pathLengthMetres);
        }

        private bool EvaluateConnectorDeformedPath(
            ConnectorEvolutionSlot slot,
            ConnectorRelationshipCandidate candidate,
            Vector2[] basePoints,
            float[] baseCumulative,
            Vector2 startGate,
            Vector2 endGate,
            bool writePoints,
            out float pathLengthMetres)
        {
            pathLengthMetres = 0f;
            int pointCount = basePoints != null
                ? basePoints.Length
                : 0;
            if (basePoints == null || baseCumulative == null ||
                baseCumulative.Length != pointCount ||
                pointCount < 2 || pointCount > slot.PointCapacity ||
                slot.PointOffset < 0 ||
                slot.PointOffset + pointCount >
                    connectorPathPointGpuData.Length ||
                candidate.StartHostSlotIndex < 0 ||
                candidate.StartHostSlotIndex >= majorEvolutionSlots.Length ||
                candidate.EndHostSlotIndex < 0 ||
                candidate.EndHostSlotIndex >= majorEvolutionSlots.Length)
            {
                return false;
            }

            MajorEvolutionSlot startHost =
                majorEvolutionSlots[candidate.StartHostSlotIndex];
            MajorEvolutionSlot endHost =
                majorEvolutionSlots[candidate.EndHostSlotIndex];
            MajorEvolutionPose startPose = ResolveMajorPose(startHost);
            MajorEvolutionPose endPose = ResolveMajorPose(endHost);
            Vector2 startDelta = startGate - basePoints[0];
            Vector2 endDelta = endGate -
                basePoints[basePoints.Length - 1];
            float endpointMotion =
                (startDelta.magnitude + endDelta.magnitude) * 0.5f;
            float deformationActivity = Mathf.Clamp01(
                endpointMotion / 0.65f);
            if (startHost.IsMoving || endHost.IsMoving)
            {
                deformationActivity = Mathf.Max(
                    deformationActivity,
                    0.30f);
            }

            float amplitude = Mathf.Lerp(
                ConnectorMinimumInteriorDeformationMetres,
                ConnectorMaximumInteriorDeformationMetres,
                HashConnectorIdentity(slot.StableId, 41u)) *
                deformationActivity;
            float frequency = Mathf.Lerp(
                1.20f,
                2.25f,
                HashConnectorIdentity(slot.StableId, 42u));
            float phase =
                HashConnectorIdentity(slot.StableId, 43u) *
                    Mathf.PI * 2f +
                (startPose.LocalDistance + endPose.LocalDistance) * 0.31f +
                (startPose.AcrossNormalized -
                    endPose.AcrossNormalized) * 1.70f +
                (startHost.HopIndex + endHost.HopIndex) * 0.37f +
                (startHost.RecycleCount + endHost.RecycleCount) * 0.83f;

            float cumulativeLength = 0f;
            Vector2 previousPosition = startGate;
            for (int pointIndex = 0;
                 pointIndex < pointCount;
                 pointIndex++)
            {
                float pathFraction = Mathf.Clamp01(
                    baseCumulative[pointIndex]);
                Vector2 endpointWarp = Vector2.Lerp(
                    startDelta,
                    endDelta,
                    pathFraction);
                Vector2 position = basePoints[pointIndex] + endpointWarp;

                if (pointIndex > 0 &&
                    pointIndex < pointCount - 1 &&
                    amplitude > 0.0001f)
                {
                    int previousIndex = Mathf.Max(0, pointIndex - 1);
                    int nextIndex = Mathf.Min(
                        pointCount - 1,
                        pointIndex + 1);
                    Vector2 previousReference =
                        basePoints[previousIndex] + Vector2.Lerp(
                            startDelta,
                            endDelta,
                            Mathf.Clamp01(
                                baseCumulative[previousIndex]));
                    Vector2 nextReference =
                        basePoints[nextIndex] + Vector2.Lerp(
                            startDelta,
                            endDelta,
                            Mathf.Clamp01(baseCumulative[nextIndex]));
                    Vector2 tangent = nextReference - previousReference;
                    if (tangent.sqrMagnitude > 0.000001f)
                    {
                        tangent.Normalize();
                        Vector2 normal = new Vector2(
                            -tangent.y,
                            tangent.x);
                        float envelope = Mathf.Sin(
                            pathFraction * Mathf.PI);
                        envelope *= envelope;
                        float wave = Mathf.Sin(
                            pathFraction * frequency *
                                Mathf.PI * 2f + phase);
                        position += normal *
                            (amplitude * envelope * wave);
                    }
                }

                if (pointIndex == 0)
                {
                    position = startGate;
                }
                else if (pointIndex == pointCount - 1)
                {
                    position = endGate;
                }

                if (pointIndex > 0)
                {
                    cumulativeLength += Vector2.Distance(
                        previousPosition,
                        position);
                }
                if (writePoints)
                {
                    connectorPathPointGpuData[
                        slot.PointOffset + pointIndex] = new Vector4(
                            position.x,
                            position.y,
                            cumulativeLength,
                            0f);
                }
                previousPosition = position;
            }

            if (cumulativeLength <= 0.0001f)
            {
                return false;
            }

            pathLengthMetres = cumulativeLength;
            if (!writePoints)
            {
                return true;
            }

            float inverseLength = 1f / cumulativeLength;
            for (int pointIndex = 0;
                 pointIndex < pointCount;
                 pointIndex++)
            {
                int flattenedIndex = slot.PointOffset + pointIndex;
                Vector4 point = connectorPathPointGpuData[flattenedIndex];
                point.z = Mathf.Clamp01(point.z * inverseLength);
                connectorPathPointGpuData[flattenedIndex] = point;
            }

            return true;
        }

        private void EnsureConnectorIdentityBuffers()
        {
            if (connectorIdentityBuffer == null)
            {
                connectorIdentityBuffer = new ComputeBuffer(
                    Mathf.Max(1, connectorIdentityGpuData.Length),
                    sizeof(float) * 4,
                    ComputeBufferType.Structured);
                if (connectorIdentityGpuData.Length == 0)
                {
                    connectorIdentityBuffer.SetData(
                        new FoamConnectorIdentityData[1]);
                }
            }
            if (connectorPathPointBuffer == null)
            {
                connectorPathPointBuffer = new ComputeBuffer(
                    Mathf.Max(1, connectorPathPointGpuData.Length),
                    sizeof(float) * 4,
                    ComputeBufferType.Structured);
                if (connectorPathPointGpuData.Length == 0)
                {
                    connectorPathPointBuffer.SetData(new Vector4[1]);
                }
            }
            if (weakSpanIdentityBuffer == null)
            {
                weakSpanIdentityBuffer = new ComputeBuffer(
                    Mathf.Max(1, weakSpanIdentityGpuData.Length),
                    sizeof(float) * 12,
                    ComputeBufferType.Structured);
                if (weakSpanIdentityGpuData.Length == 0)
                {
                    weakSpanIdentityBuffer.SetData(
                        new FoamWeakSpanIdentityData[1]);
                }
            }
        }

        private void ReleaseConnectorIdentityReconstructionResources()
        {
            connectorIdentityBuffer?.Release();
            connectorIdentityBuffer = null;
            connectorPathPointBuffer?.Release();
            connectorPathPointBuffer = null;
            weakSpanIdentityBuffer?.Release();
            weakSpanIdentityBuffer = null;
            connectorIdentityGpuData =
                Array.Empty<FoamConnectorIdentityData>();
            connectorPathPointGpuData = Array.Empty<Vector4>();
            weakSpanIdentityGpuData =
                Array.Empty<FoamWeakSpanIdentityData>();
            connectorEvolutionSlots = Array.Empty<ConnectorEvolutionSlot>();
            connectorRelationshipCandidates =
                Array.Empty<ConnectorRelationshipCandidate>();
            connectorCandidateClaimed = Array.Empty<bool>();
            connectorMajorPairClaimed = Array.Empty<bool>();
            connectorMajorDegree = Array.Empty<int>();
            connectorPreviousMajorDegree = Array.Empty<int>();
            connectorEvolutionActiveCount = 0;
            connectorEvolutionTemporaryAbsenceCount = 0;
            connectorEvolutionIdentityPathCount = 0;
            connectorEvolutionRecycleVariantCount = 0;
            connectorEvolutionOriginalRelationshipCount = 0;
            connectorEvolutionReplacementRelationshipCount = 0;
            connectorEvolutionRelationshipRebindCount = 0;
            connectorEvolutionVariantSwitchCount = 0;
            connectorEvolutionStretchBreakCount = 0;
            connectorEvolutionRetainDecisionCount = 0;
            connectorEvolutionTurnoverRequestCount = 0;
            connectorEvolutionSuccessfulTurnoverCount = 0;
            connectorEvolutionNoAlternativeFallbackCount = 0;
            connectorEvolutionCrowdingBoostedTurnoverCount = 0;
            connectorEvolutionMajorDegreeZeroCount = 0;
            connectorEvolutionMajorDegreeOneCount = 0;
            connectorEvolutionMajorDegreeTwoCount = 0;
            connectorEvolutionMajorDegreeThreePlusCount = 0;
            connectorEvolutionMaximumMajorDegree = 0;
            connectorEvolutionAbsenceEventCount = 0;
            connectorEvolutionReappearanceCount = 0;
            weakSpanEvolutionActiveCount = 0;
            connectorIdentityReconstructionReady = false;
            weakSpanIdentityReconstructionReady = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            connectorIdentityParityGeneration++;
            connectorIdentityParityPending = false;
            connectorIdentityParityReadbackPending = false;
            connectorIdentityParityAvailable = false;
            connectorIdentityParityMeanDifference = 0f;
            connectorIdentityParityMaximumDifference = 0f;
            weakSpanIdentityParityGeneration++;
            weakSpanIdentityParityPending = false;
            weakSpanIdentityParityReadbackPending = false;
            weakSpanIdentityParityAvailable = false;
            weakSpanIdentityParityMeanDifference = 0f;
            weakSpanIdentityParityMaximumDifference = 0f;
#endif
        }

        private static HostedNegativeEvolutionPose
            CreateIdentityHostedNegativePose()
        {
            return new HostedNegativeEvolutionPose
            {
                OffsetCells = Vector2.zero,
                RotationRadians = 0f,
                ScaleAlong = 1f,
                ScaleAcross = 1f,
                StrengthScale = 1f
            };
        }

        private void ReleaseMajorEvolutionResources()
        {
            ReleaseHostedNegativeEvolutionResources();
            ReleaseFreeWaterEvolutionResources();
            ReleaseConnectorIdentityReconstructionResources();
            majorEvolutionBuffer?.Release();
            majorEvolutionBuffer = null;
            if (majorMaskTextureArray != null)
            {
                DestroyUnityObject(majorMaskTextureArray);
                majorMaskTextureArray = null;
            }

            majorEvolutionSlots = Array.Empty<MajorEvolutionSlot>();
            majorEvolutionGpuData = Array.Empty<FoamMajorEvolutionData>();
            majorMaskUploadPixels = Array.Empty<Color>();
            majorEvolutionAccumulator = 0f;
            majorEvolutionReady = false;
            majorEvolutionLifetimeInputSignature = int.MinValue;
            majorEvolutionReconstructionTicks = 0;
            majorEvolutionRecycleCount = 0;
            majorEvolutionCrowdedRecycleFallbackCount = 0;
            majorEvolutionUpstreamViolations = 0;
            majorEvolutionObservedMinimumDwell = float.PositiveInfinity;
            majorEvolutionObservedMaximumDwell = 0f;
            majorEvolutionObservedMinimumMove = float.PositiveInfinity;
            majorEvolutionObservedMaximumMove = 0f;
            majorEvolutionLastCpuMilliseconds = 0.0;
            majorEvolutionLastAllocatedBytes = 0L;
        }

        private bool AdvanceMajorEvolution(float deltaTime)
        {
            if (!majorEvolutionReady || deltaTime <= 0f ||
                majorEvolutionSlots.Length == 0)
            {
                return false;
            }

            using var profilerScope =
                MajorEvolutionAdvanceProfilerMarker.Auto();
            RefreshMajorEvolutionLifetimeBudgetsIfNeeded();
            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            double startTime = Time.realtimeSinceStartupAsDouble;
            bool immediateRebuild = false;
            bool anyMoving = false;

            for (int index = 0;
                 index < majorEvolutionSlots.Length;
                 index++)
            {
                ref MajorEvolutionSlot slot =
                    ref majorEvolutionSlots[index];
                slot.OccurrenceElapsed += deltaTime;

                if (slot.IsMoving)
                {
                    slot.MoveElapsed += deltaTime;
                    if (slot.MoveElapsed >= slot.MoveDuration)
                    {
                        if (slot.Target.LocalDistance + 0.0001f <
                            slot.Start.LocalDistance)
                        {
                            majorEvolutionUpstreamViolations++;
                        }

                        slot.Current = slot.Target;
                        slot.IsMoving = false;
                        slot.MoveElapsed = slot.MoveDuration;
                        slot.HopIndex++;
                        CompleteHostedNegativeMove(index);
                        immediateRebuild = true;

                        if (ShouldRecycleMajor(slot) ||
                            IsInMajorEgress(
                                slot.Current.LocalDistance))
                        {
                            RecycleMajor(index, ref slot);
                        }
                        else
                        {
                            ResolveMajorDwell(ref slot, 20u);
                        }
                    }
                    else
                    {
                        anyMoving = true;
                    }

                    continue;
                }

                // A dwelling occurrence may exhaust its combined lifetime
                // budget between hops. Recycle it immediately rather than
                // allowing the remaining dwell to create a persistent clump.
                if (ShouldRecycleMajor(slot))
                {
                    RecycleMajor(index, ref slot);
                    immediateRebuild = true;
                    continue;
                }

                slot.DwellRemaining -= deltaTime;
                if (slot.DwellRemaining > 0f)
                {
                    continue;
                }

                if (BeginMajorMove(index, ref slot))
                {
                    anyMoving = true;
                }
                else
                {
                    immediateRebuild = true;
                }
            }

            if (anyMoving)
            {
                majorEvolutionAccumulator += deltaTime;
            }
            else
            {
                majorEvolutionAccumulator = 0f;
            }

            float tickInterval = 1f / MajorEvolutionTickRate;
            bool scheduledRebuild = anyMoving &&
                majorEvolutionAccumulator >= tickInterval;
            if (scheduledRebuild)
            {
                majorEvolutionAccumulator %= tickInterval;
            }

            bool reconstructionRequired =
                immediateRebuild || scheduledRebuild;

            majorEvolutionLastCpuMilliseconds =
                (Time.realtimeSinceStartupAsDouble - startTime) * 1000.0;
            majorEvolutionLastAllocatedBytes = Math.Max(
                0L,
                GC.GetAllocatedBytesForCurrentThread() - allocatedBefore);
            return reconstructionRequired;
        }

        private bool BeginMajorMove(
            int slotIndex,
            ref MajorEvolutionSlot slot)
        {
            uint cycleSeed = ResolveMajorCycleSeed(slot, 30u);
            float flowScale = Mathf.Lerp(
                0.78f,
                1.32f,
                Mathf.Clamp01(
                    river.FlowSpeedMetresPerSecond / 4.5f));
            float downstreamStep = Mathf.Lerp(
                MajorMinimumHopMetres,
                MajorMaximumHopMetres,
                HashMajorEvolution(cycleSeed, 1u)) * flowScale;
            float targetDistance = slot.Current.LocalDistance +
                downstreamStep;
            if (targetDistance >= ResolveMajorEgressStart())
            {
                RecycleMajor(slotIndex, ref slot);
                return false;
            }

            float lateralMagnitude = Mathf.Lerp(
                MajorMinimumLateralHop,
                MajorMaximumLateralHop,
                HashMajorEvolution(cycleSeed, 2u));
            float lateralSign = HashMajorEvolution(cycleSeed, 3u) < 0.5f
                ? -1f
                : 1f;
            float targetAcross = Mathf.Clamp(
                slot.Current.AcrossNormalized +
                    lateralMagnitude * lateralSign,
                -0.78f,
                0.78f);
            if (Mathf.Abs(
                    targetAcross - slot.Current.AcrossNormalized) < 0.02f)
            {
                targetAcross = Mathf.Clamp(
                    slot.Current.AcrossNormalized -
                        lateralMagnitude * lateralSign,
                    -0.78f,
                    0.78f);
            }

            float scaleAlong = Mathf.Lerp(
                0.80f,
                1.24f,
                HashMajorEvolution(cycleSeed, 4u));
            float scaleAcross = Mathf.Lerp(
                0.78f,
                1.22f,
                HashMajorEvolution(cycleSeed, 5u));
            float metresScale = Mathf.Lerp(
                0.96f,
                1.04f,
                HashMajorEvolution(cycleSeed, 7u));
            float areaScale = Mathf.Max(
                0.20f,
                scaleAlong * scaleAcross * metresScale * metresScale);
            float supportScale = Mathf.Clamp(
                1f / areaScale,
                0.72f,
                1.28f);

            slot.Start = slot.Current;
            slot.Target = new MajorEvolutionPose
            {
                LocalDistance = targetDistance,
                AcrossNormalized = targetAcross,
                OrientationRadians = WrapMajorAngle(
                    slot.Current.OrientationRadians +
                    Mathf.Lerp(
                        -0.30f,
                        0.30f,
                        HashMajorEvolution(cycleSeed, 6u))),
                MetresPerCandidateCell =
                    slot.BaseMetresPerCandidateCell * metresScale,
                ScaleAlong = scaleAlong,
                ScaleAcross = scaleAcross,
                Shear = Mathf.Lerp(
                    -0.18f,
                    0.18f,
                    HashMajorEvolution(cycleSeed, 8u)),
                WarpAlong = Mathf.Lerp(
                    0.035f,
                    0.115f,
                    HashMajorEvolution(cycleSeed, 9u)),
                WarpAcross = Mathf.Lerp(
                    0.045f,
                    0.145f,
                    HashMajorEvolution(cycleSeed, 10u)),
                WarpPhaseA = HashMajorEvolution(
                    cycleSeed,
                    11u) * Mathf.PI * 2f,
                WarpPhaseB = HashMajorEvolution(
                    cycleSeed,
                    12u) * Mathf.PI * 2f,
                SupportScale = supportScale
            };

            float dwellProgress = Mathf.InverseLerp(
                MajorMinimumDwellSeconds,
                MajorMaximumDwellSeconds,
                slot.LastDwellDuration);
            slot.MoveDuration = Mathf.Clamp(
                Mathf.Lerp(
                    MajorMinimumMoveSeconds,
                    MajorMaximumMoveSeconds,
                    dwellProgress * 0.72f +
                    HashMajorEvolution(cycleSeed, 13u) * 0.28f),
                MajorMinimumMoveSeconds,
                MajorMaximumMoveSeconds);
            slot.MoveElapsed = 0f;
            slot.IsMoving = true;
            majorEvolutionObservedMinimumMove = Mathf.Min(
                majorEvolutionObservedMinimumMove,
                slot.MoveDuration);
            majorEvolutionObservedMaximumMove = Mathf.Max(
                majorEvolutionObservedMaximumMove,
                slot.MoveDuration);
            BeginHostedNegativeMove(slotIndex, cycleSeed);
            return true;
        }

        private bool AdvanceFreeWaterEvolution(float deltaTime)
        {
            if (!freeWaterEvolutionReady || deltaTime <= 0f ||
                freeWaterEvolutionSlots.Length == 0)
            {
                return false;
            }

            bool immediateRebuild = false;
            bool anyMoving = false;
            for (int index = 0;
                 index < freeWaterEvolutionSlots.Length;
                 index++)
            {
                ref FreeWaterEvolutionSlot slot =
                    ref freeWaterEvolutionSlots[index];
                slot.OccurrenceElapsed += deltaTime;

                if (slot.IsMoving)
                {
                    slot.MoveElapsed += deltaTime;
                    if (slot.MoveElapsed >= slot.MoveDuration)
                    {
                        if (slot.Target.LocalDistance + 0.0001f <
                            slot.Start.LocalDistance)
                        {
                            freeWaterUpstreamViolationCount++;
                        }

                        slot.Current = slot.Target;
                        slot.Start = slot.Target;
                        slot.IsMoving = false;
                        slot.MoveElapsed = slot.MoveDuration;
                        slot.HopIndex++;
                        freeWaterMoveCount++;
                        immediateRebuild = true;

                        if (ShouldRecycleFreeWater(slot) ||
                            IsInFreeWaterEgress(
                                slot.Current.LocalDistance))
                        {
                            RecycleFreeWater(ref slot);
                        }
                        else
                        {
                            ResolveFreeWaterDwell(ref slot, 20u);
                        }
                    }
                    else
                    {
                        anyMoving = true;
                    }

                    continue;
                }

                if (ShouldRecycleFreeWater(slot))
                {
                    RecycleFreeWater(ref slot);
                    immediateRebuild = true;
                    continue;
                }

                slot.DwellRemaining -= deltaTime;
                if (slot.DwellRemaining > 0f)
                {
                    continue;
                }

                if (BeginFreeWaterMove(ref slot))
                {
                    anyMoving = true;
                }
                else
                {
                    immediateRebuild = true;
                }
            }

            if (anyMoving)
            {
                freeWaterEvolutionAccumulator += deltaTime;
            }
            else
            {
                freeWaterEvolutionAccumulator = 0f;
            }

            float tickInterval = 1f / FreeWaterEvolutionTickRate;
            bool scheduledRebuild = anyMoving &&
                freeWaterEvolutionAccumulator >= tickInterval;
            if (scheduledRebuild)
            {
                freeWaterEvolutionAccumulator %= tickInterval;
            }

            return immediateRebuild || scheduledRebuild;
        }

        private bool BeginFreeWaterMove(ref FreeWaterEvolutionSlot slot)
        {
            uint cycleSeed = ResolveFreeWaterCycleSeed(slot, 30u);
            float flowScale = Mathf.Lerp(
                0.72f,
                1.22f,
                Mathf.Clamp01(
                    river.FlowSpeedMetresPerSecond / 4.5f));
            float downstreamStep = Mathf.Lerp(
                FreeWaterMinimumHopMetres,
                FreeWaterMaximumHopMetres,
                HashMajorEvolution(cycleSeed, 1u)) * flowScale;
            float targetDistance = slot.Current.LocalDistance +
                downstreamStep;
            if (targetDistance >= ResolveFreeWaterEgressStart())
            {
                RecycleFreeWater(ref slot);
                return false;
            }

            float lateralMagnitude = Mathf.Lerp(
                FreeWaterMinimumLateralHop,
                FreeWaterMaximumLateralHop,
                HashMajorEvolution(cycleSeed, 2u));
            float lateralSign = HashMajorEvolution(cycleSeed, 3u) < 0.5f
                ? -1f
                : 1f;
            float targetAcross = Mathf.Clamp(
                slot.Current.AcrossNormalized +
                    lateralMagnitude * lateralSign,
                -0.84f,
                0.84f);
            if (Mathf.Abs(
                    targetAcross - slot.Current.AcrossNormalized) < 0.025f)
            {
                targetAcross = Mathf.Clamp(
                    slot.Current.AcrossNormalized -
                        lateralMagnitude * lateralSign,
                    -0.84f,
                    0.84f);
            }

            float scaleAlong = Mathf.Lerp(
                0.82f,
                1.22f,
                HashMajorEvolution(cycleSeed, 4u));
            float scaleAcross = Mathf.Lerp(
                0.82f,
                1.22f,
                HashMajorEvolution(cycleSeed, 5u));
            float areaScale = Mathf.Max(
                0.25f,
                scaleAlong * scaleAcross);
            slot.Start = slot.Current;
            slot.Target = new FreeWaterEvolutionPose
            {
                LocalDistance = targetDistance,
                AcrossNormalized = targetAcross,
                OrientationRadians = WrapMajorAngle(
                    slot.Current.OrientationRadians +
                    Mathf.Lerp(
                        -0.22f,
                        0.22f,
                        HashMajorEvolution(cycleSeed, 6u))),
                ScaleAlong = scaleAlong,
                ScaleAcross = scaleAcross,
                StrengthScale = Mathf.Clamp(
                    1f / Mathf.Sqrt(areaScale),
                    0.88f,
                    1.12f)
            };

            float dwellProgress = Mathf.InverseLerp(
                FreeWaterMinimumDwellSeconds,
                FreeWaterMaximumDwellSeconds,
                slot.LastDwellDuration);
            slot.MoveDuration = Mathf.Clamp(
                Mathf.Lerp(
                    FreeWaterMinimumMoveSeconds,
                    FreeWaterMaximumMoveSeconds,
                    dwellProgress * 0.72f +
                    HashMajorEvolution(cycleSeed, 7u) * 0.28f),
                FreeWaterMinimumMoveSeconds,
                FreeWaterMaximumMoveSeconds);
            slot.MoveElapsed = 0f;
            slot.IsMoving = true;
            freeWaterObservedMinimumMove = Mathf.Min(
                freeWaterObservedMinimumMove,
                slot.MoveDuration);
            freeWaterObservedMaximumMove = Mathf.Max(
                freeWaterObservedMaximumMove,
                slot.MoveDuration);
            return true;
        }

        private FreeWaterEvolutionPose ResolveFreeWaterPose(
            FreeWaterEvolutionSlot slot)
        {
            if (!slot.IsMoving || slot.MoveDuration <= 0.0001f)
            {
                return slot.Current;
            }

            float t = Mathf.Clamp01(slot.MoveElapsed / slot.MoveDuration);
            t = t * t * (3f - 2f * t);
            return new FreeWaterEvolutionPose
            {
                LocalDistance = Mathf.Lerp(
                    slot.Start.LocalDistance,
                    slot.Target.LocalDistance,
                    t),
                AcrossNormalized = Mathf.Lerp(
                    slot.Start.AcrossNormalized,
                    slot.Target.AcrossNormalized,
                    t),
                OrientationRadians = LerpMajorAngle(
                    slot.Start.OrientationRadians,
                    slot.Target.OrientationRadians,
                    t),
                ScaleAlong = Mathf.Lerp(
                    slot.Start.ScaleAlong,
                    slot.Target.ScaleAlong,
                    t),
                ScaleAcross = Mathf.Lerp(
                    slot.Start.ScaleAcross,
                    slot.Target.ScaleAcross,
                    t),
                StrengthScale = Mathf.Lerp(
                    slot.Start.StrengthScale,
                    slot.Target.StrengthScale,
                    t)
            };
        }

        private static FreeWaterEvolutionPose CreateInitialFreeWaterPose(
            StylizedRiverFoamPreparedFreeWaterRegion prepared)
        {
            return new FreeWaterEvolutionPose
            {
                LocalDistance = prepared.CentreLocalDistance,
                AcrossNormalized = prepared.CentreAcrossNormalized,
                OrientationRadians = prepared.OrientationRadians,
                ScaleAlong = 1f,
                ScaleAcross = 1f,
                StrengthScale = 1f
            };
        }

        private void ResolveFreeWaterDwell(
            ref FreeWaterEvolutionSlot slot,
            uint stream)
        {
            uint seed = ResolveFreeWaterCycleSeed(slot, stream);
            float dwell = Mathf.Lerp(
                FreeWaterMinimumDwellSeconds,
                FreeWaterMaximumDwellSeconds,
                HashMajorEvolution(seed, 1u));
            slot.DwellRemaining = dwell;
            slot.LastDwellDuration = dwell;
            freeWaterObservedMinimumDwell = Mathf.Min(
                freeWaterObservedMinimumDwell,
                dwell);
            freeWaterObservedMaximumDwell = Mathf.Max(
                freeWaterObservedMaximumDwell,
                dwell);
        }

        private void ResolveFreeWaterOccurrenceBudget(
            ref FreeWaterEvolutionSlot slot)
        {
            uint seed = ResolveFreeWaterOccurrenceSeed(slot, 70u);
            slot.LifetimeUnitBudget = Mathf.Lerp(
                FreeWaterMinimumLifetimeUnits,
                FreeWaterMaximumLifetimeUnits,
                HashMajorEvolution(seed, 1u));
            slot.MaximumOccurrenceSeconds =
                slot.LifetimeUnitBudget *
                FreeWaterLifetimeSecondsPerUnit *
                FreeWaterLifetimeSafetyMultiplier;
        }

        private bool ShouldRecycleFreeWater(FreeWaterEvolutionSlot slot)
        {
            float usedUnits =
                FreeWaterLifetimeTimeWeight *
                    (slot.OccurrenceElapsed /
                        FreeWaterLifetimeSecondsPerUnit) +
                FreeWaterLifetimeHopWeight * slot.HopIndex;
            return usedUnits >= slot.LifetimeUnitBudget ||
                slot.OccurrenceElapsed >= slot.MaximumOccurrenceSeconds;
        }

        private float ResolveFreeWaterEgressStart()
        {
            return StylizedRiverFoamMajorTopology
                .ResolveEvolutionEgressStart(validFieldLength);
        }

        private bool IsInFreeWaterEgress(float localDistance)
        {
            return localDistance >= ResolveFreeWaterEgressStart();
        }

        private void RecycleFreeWater(ref FreeWaterEvolutionSlot slot)
        {
            IReadOnlyList<StylizedRiverFoamPreparedFreeWaterRegion> prepared =
                pocketTopology != null
                    ? pocketTopology.PreparedFreeWaterRegions
                    : Array.Empty<StylizedRiverFoamPreparedFreeWaterRegion>();
            if (slot.PreparedIndex < 0 ||
                slot.PreparedIndex >= prepared.Count)
            {
                return;
            }

            StylizedRiverFoamPreparedFreeWaterRegion preparedRegion =
                prepared[slot.PreparedIndex];
            IReadOnlyList<StylizedRiverFoamFreeWaterRecycleAnchor> anchors =
                preparedRegion.RecycleAnchors;
            if (anchors.Count == 0)
            {
                return;
            }

            slot.RecycleCount++;
            freeWaterRecycleCount++;
            uint recycleSeed = ResolveFreeWaterCycleSeed(slot, 50u);
            int anchorIndex = (int)(EvolutionMixBits(recycleSeed) %
                (uint)anchors.Count);
            if (anchors.Count > 1 && anchorIndex == slot.LastAnchorIndex)
            {
                anchorIndex = (anchorIndex + 1) % anchors.Count;
            }

            StylizedRiverFoamFreeWaterRecycleAnchor anchor =
                anchors[anchorIndex];
            slot.LastAnchorIndex = anchorIndex;
            float scaleAlong = Mathf.Lerp(
                0.86f,
                1.18f,
                HashMajorEvolution(recycleSeed, 1u));
            float scaleAcross = Mathf.Lerp(
                0.86f,
                1.18f,
                HashMajorEvolution(recycleSeed, 2u));
            float areaScale = Mathf.Max(
                0.25f,
                scaleAlong * scaleAcross);
            FreeWaterEvolutionPose recycledPose =
                new FreeWaterEvolutionPose
                {
                    LocalDistance = anchor.CentreLocalDistance,
                    AcrossNormalized = anchor.CentreAcrossNormalized,
                    OrientationRadians = WrapMajorAngle(
                        anchor.OrientationRadians +
                        Mathf.Lerp(
                            -0.12f,
                            0.12f,
                            HashMajorEvolution(recycleSeed, 3u))),
                    ScaleAlong = scaleAlong,
                    ScaleAcross = scaleAcross,
                    StrengthScale = Mathf.Clamp(
                        1f / Mathf.Sqrt(areaScale),
                        0.90f,
                        1.10f)
                };

            slot.Current = recycledPose;
            slot.Start = recycledPose;
            slot.Target = recycledPose;
            slot.MoveElapsed = 0f;
            slot.MoveDuration = 0f;
            slot.IsMoving = false;
            slot.OccurrenceElapsed = 0f;
            slot.HopIndex = 0;
            ResolveFreeWaterOccurrenceBudget(ref slot);
            ResolveFreeWaterDwell(ref slot, 60u);
        }

        private void RecycleMajor(
            int slotIndex,
            ref MajorEvolutionSlot slot)
        {
            IReadOnlyList<StylizedRiverFoamPreparedMajorRegion> prepared =
                majorTopology.PreparedRegions;
            if (slot.PreparedIndex < 0 ||
                slot.PreparedIndex >= prepared.Count)
            {
                return;
            }

            StylizedRiverFoamPreparedMajorRegion preparedRegion =
                prepared[slot.PreparedIndex];
            IReadOnlyList<StylizedRiverFoamMajorRecycleAnchor> anchors =
                preparedRegion.RecycleAnchors;
            if (anchors.Count == 0)
            {
                return;
            }

            slot.RecycleCount++;
            majorEvolutionRecycleCount++;
            uint recycleSeed = ResolveMajorCycleSeed(slot, 50u);
            int anchorIndex = ResolveMajorRecycleAnchorIndex(
                slot,
                preparedRegion,
                anchors,
                recycleSeed,
                out bool crowdedFallback);
            if (crowdedFallback)
            {
                majorEvolutionCrowdedRecycleFallbackCount++;
            }

            StylizedRiverFoamMajorRecycleAnchor anchor =
                anchors[anchorIndex];
            slot.LastAnchorIndex = anchorIndex;
            slot.BaseMetresPerCandidateCell =
                anchor.MetresPerCandidateCell;
            float scaleAlong = Mathf.Lerp(
                0.86f,
                1.18f,
                HashMajorEvolution(recycleSeed, 1u));
            float scaleAcross = Mathf.Lerp(
                0.84f,
                1.16f,
                HashMajorEvolution(recycleSeed, 2u));
            float areaScale = Mathf.Max(
                0.20f,
                scaleAlong * scaleAcross);
            MajorEvolutionPose recycledPose = new MajorEvolutionPose
            {
                LocalDistance = Mathf.Clamp(
                    anchor.CentreLocalDistance,
                    0f,
                    ResolveMajorEgressStart() - 0.01f),
                AcrossNormalized = anchor.CentreAcrossNormalized,
                OrientationRadians = WrapMajorAngle(
                    anchor.OrientationRadians +
                    Mathf.Lerp(
                        -0.18f,
                        0.18f,
                        HashMajorEvolution(recycleSeed, 3u))),
                MetresPerCandidateCell =
                    anchor.MetresPerCandidateCell,
                ScaleAlong = scaleAlong,
                ScaleAcross = scaleAcross,
                Shear = Mathf.Lerp(
                    -0.14f,
                    0.14f,
                    HashMajorEvolution(recycleSeed, 4u)),
                WarpAlong = Mathf.Lerp(
                    0.03f,
                    0.11f,
                    HashMajorEvolution(recycleSeed, 5u)),
                WarpAcross = Mathf.Lerp(
                    0.04f,
                    0.14f,
                    HashMajorEvolution(recycleSeed, 6u)),
                WarpPhaseA = HashMajorEvolution(
                    recycleSeed,
                    7u) * Mathf.PI * 2f,
                WarpPhaseB = HashMajorEvolution(
                    recycleSeed,
                    8u) * Mathf.PI * 2f,
                SupportScale = Mathf.Clamp(
                    1f / areaScale,
                    0.72f,
                    1.28f)
            };

            slot.Current = recycledPose;
            slot.Start = recycledPose;
            slot.Target = recycledPose;
            slot.MoveElapsed = 0f;
            slot.MoveDuration = 0f;
            slot.IsMoving = false;
            slot.OccurrenceElapsed = 0f;
            slot.HopIndex = 0;
            ResolveMajorOccurrenceBudget(ref slot);
            ResolveMajorDwell(ref slot, 60u);
            RecycleHostedNegativeSlots(slotIndex, recycleSeed);
        }

        private void BeginHostedNegativeMove(
            int hostSlotIndex,
            uint hostCycleSeed)
        {
            if (!hostedNegativeEvolutionReady)
            {
                return;
            }

            IReadOnlyList<StylizedRiverFoamPreparedHostedNegativeRegion>
                prepared = pocketTopology.PreparedHostedRegions;
            for (int index = 0;
                 index < hostedNegativeEvolutionSlots.Length;
                 index++)
            {
                ref HostedNegativeEvolutionSlot slot =
                    ref hostedNegativeEvolutionSlots[index];
                if (slot.HostSlotIndex != hostSlotIndex ||
                    slot.PreparedIndex < 0 ||
                    slot.PreparedIndex >= prepared.Count)
                {
                    continue;
                }

                uint seed = EvolutionMixBits(
                    slot.StableId ^ hostCycleSeed ^
                    ((uint)(majorEvolutionSlots[hostSlotIndex].HopIndex + 1) *
                        0x27D4EB2Fu));
                float changeProbability = slot.RegionClass ==
                    StylizedRiverFoamNegativeRegionClass.InteriorPocket
                    ? HostedInteriorChangeProbability
                    : HostedCavityChangeProbability;
                slot.Start = slot.Current;
                if (HashMajorEvolution(seed, 1u) > changeProbability)
                {
                    slot.Target = slot.Current;
                    slot.TargetVariantIndex = slot.CurrentVariantIndex;
                    continue;
                }

                slot.Target = ResolveHostedNegativeTarget(
                    prepared[slot.PreparedIndex],
                    seed,
                    slot.CurrentVariantIndex,
                    out int targetVariantIndex);
                slot.TargetVariantIndex = targetVariantIndex;
                if (targetVariantIndex != slot.CurrentVariantIndex)
                {
                    hostedNegativeLocalChangeCount++;
                }
            }
        }

        private void CompleteHostedNegativeMove(int hostSlotIndex)
        {
            for (int index = 0;
                 index < hostedNegativeEvolutionSlots.Length;
                 index++)
            {
                ref HostedNegativeEvolutionSlot slot =
                    ref hostedNegativeEvolutionSlots[index];
                if (slot.HostSlotIndex != hostSlotIndex)
                {
                    continue;
                }

                slot.Current = slot.Target;
                slot.Start = slot.Target;
                slot.CurrentVariantIndex = slot.TargetVariantIndex;
            }
        }

        private void RecycleHostedNegativeSlots(
            int hostSlotIndex,
            uint recycleSeed)
        {
            if (!hostedNegativeEvolutionReady)
            {
                return;
            }

            IReadOnlyList<StylizedRiverFoamPreparedHostedNegativeRegion>
                prepared = pocketTopology.PreparedHostedRegions;
            for (int index = 0;
                 index < hostedNegativeEvolutionSlots.Length;
                 index++)
            {
                ref HostedNegativeEvolutionSlot slot =
                    ref hostedNegativeEvolutionSlots[index];
                if (slot.HostSlotIndex != hostSlotIndex ||
                    slot.PreparedIndex < 0 ||
                    slot.PreparedIndex >= prepared.Count)
                {
                    continue;
                }

                uint seed = EvolutionMixBits(
                    slot.StableId ^ recycleSeed ^ 0xA24BAED5u);
                HostedNegativeEvolutionPose pose =
                    ResolveHostedNegativeTarget(
                        prepared[slot.PreparedIndex],
                        seed,
                        slot.CurrentVariantIndex,
                        out int targetVariantIndex);
                if (targetVariantIndex != slot.CurrentVariantIndex)
                {
                    hostedNegativeLocalChangeCount++;
                }
                slot.CurrentVariantIndex = targetVariantIndex;
                slot.TargetVariantIndex = targetVariantIndex;
                slot.Current = pose;
                slot.Start = pose;
                slot.Target = pose;
            }
        }

        private static HostedNegativeEvolutionPose
            ResolveHostedNegativeTarget(
                StylizedRiverFoamPreparedHostedNegativeRegion prepared,
                uint seed,
                int currentVariantIndex,
                out int targetVariantIndex)
        {
            IReadOnlyList<StylizedRiverFoamHostedNegativeVariant> variants =
                prepared.Variants;
            if (variants == null || variants.Count == 0)
            {
                targetVariantIndex = 0;
                return CreateIdentityHostedNegativePose();
            }

            int count = variants.Count;
            targetVariantIndex = Mathf.Clamp(currentVariantIndex, 0, count - 1);
            if (count > 1)
            {
                int offset = 1 + Mathf.FloorToInt(
                    HashMajorEvolution(seed, 2u) * (count - 1));
                targetVariantIndex = (targetVariantIndex + offset) % count;
            }

            StylizedRiverFoamHostedNegativeVariant variant =
                variants[targetVariantIndex];
            HostedNegativeEvolutionPose pose = new HostedNegativeEvolutionPose
            {
                OffsetCells = variant.OffsetCells,
                RotationRadians = variant.RotationRadians,
                ScaleAlong = variant.ScaleAlong,
                ScaleAcross = variant.ScaleAcross,
                StrengthScale = 1f
            };
            float areaScale = Mathf.Max(
                0.25f,
                pose.ScaleAlong * pose.ScaleAcross);
            pose.StrengthScale = Mathf.Clamp(
                1f / Mathf.Sqrt(areaScale),
                0.90f,
                1.10f);
            return pose;
        }

        private HostedNegativeEvolutionPose ResolveHostedNegativePose(
            HostedNegativeEvolutionSlot slot)
        {
            if (slot.HostSlotIndex < 0 ||
                slot.HostSlotIndex >= majorEvolutionSlots.Length)
            {
                return slot.Current;
            }

            MajorEvolutionSlot host =
                majorEvolutionSlots[slot.HostSlotIndex];
            if (!host.IsMoving || host.MoveDuration <= 0.0001f)
            {
                return slot.Current;
            }

            float t = Mathf.Clamp01(host.MoveElapsed / host.MoveDuration);
            t = t * t * (3f - 2f * t);
            return new HostedNegativeEvolutionPose
            {
                OffsetCells = Vector2.Lerp(
                    slot.Start.OffsetCells,
                    slot.Target.OffsetCells,
                    t),
                RotationRadians = Mathf.Lerp(
                    slot.Start.RotationRadians,
                    slot.Target.RotationRadians,
                    t),
                ScaleAlong = Mathf.Lerp(
                    slot.Start.ScaleAlong,
                    slot.Target.ScaleAlong,
                    t),
                ScaleAcross = Mathf.Lerp(
                    slot.Start.ScaleAcross,
                    slot.Target.ScaleAcross,
                    t),
                StrengthScale = Mathf.Lerp(
                    slot.Start.StrengthScale,
                    slot.Target.StrengthScale,
                    t)
            };
        }

        private int ResolveMajorRecycleAnchorIndex(
            MajorEvolutionSlot slot,
            StylizedRiverFoamPreparedMajorRegion preparedRegion,
            IReadOnlyList<StylizedRiverFoamMajorRecycleAnchor> anchors,
            uint recycleSeed,
            out bool crowdedFallback)
        {
            crowdedFallback = false;
            if (anchors.Count <= 1)
            {
                crowdedFallback = anchors.Count == 1 &&
                    ResolveMajorAnchorMinimumSpacing(
                        slot.PreparedIndex,
                        preparedRegion,
                        anchors[0]) < 0.72f;
                return 0;
            }

            int startIndex = (int)(EvolutionMixBits(recycleSeed) %
                (uint)anchors.Count);
            int bestIndex = -1;
            float bestMinimumSpacing = float.NegativeInfinity;
            for (int offset = 0; offset < anchors.Count; offset++)
            {
                int anchorIndex = (startIndex + offset) % anchors.Count;
                if (anchorIndex == slot.LastAnchorIndex)
                {
                    continue;
                }

                StylizedRiverFoamMajorRecycleAnchor anchor =
                    anchors[anchorIndex];
                float minimumSpacing = ResolveMajorAnchorMinimumSpacing(
                    slot.PreparedIndex,
                    preparedRegion,
                    anchor);
                if (minimumSpacing > bestMinimumSpacing + 0.0001f)
                {
                    bestMinimumSpacing = minimumSpacing;
                    bestIndex = anchorIndex;
                }
            }

            if (bestIndex < 0)
            {
                bestIndex = startIndex;
                bestMinimumSpacing = ResolveMajorAnchorMinimumSpacing(
                    slot.PreparedIndex,
                    preparedRegion,
                    anchors[bestIndex]);
            }

            crowdedFallback = bestMinimumSpacing < 0.72f;
            return bestIndex;
        }

        private float ResolveMajorAnchorMinimumSpacing(
            int slotPreparedIndex,
            StylizedRiverFoamPreparedMajorRegion preparedRegion,
            StylizedRiverFoamMajorRecycleAnchor anchor)
        {
            StylizedRiverSplineSample anchorSample =
                river.Domain.SampleAtOrientedDistance(
                    anchor.CentreLocalDistance);
            float anchorAcrossMetres =
                StylizedRiverFoamTopologyFieldSpace
                    .SignedNormalizedToMetres(
                        anchor.CentreAcrossNormalized,
                        Mathf.Max(
                            0.05f,
                            anchorSample.LeftSurfaceHalfWidth),
                        Mathf.Max(
                            0.05f,
                            anchorSample.RightSurfaceHalfWidth));
            float anchorRadius = Mathf.Max(
                preparedRegion.MajorHalfExtentCells,
                preparedRegion.MinorHalfExtentCells) *
                anchor.MetresPerCandidateCell;
            float minimumNormalisedDistance = 4f;

            IReadOnlyList<StylizedRiverFoamPreparedMajorRegion> prepared =
                majorTopology.PreparedRegions;
            for (int index = 0; index < majorEvolutionSlots.Length; index++)
            {
                MajorEvolutionSlot otherSlot = majorEvolutionSlots[index];
                if (otherSlot.PreparedIndex == slotPreparedIndex ||
                    otherSlot.PreparedIndex < 0 ||
                    otherSlot.PreparedIndex >= prepared.Count)
                {
                    continue;
                }

                MajorEvolutionPose otherPose = ResolveMajorPose(otherSlot);
                float otherAcrossMetres =
                    StylizedRiverFoamTopologyFieldSpace
                        .SignedNormalizedToMetres(
                            otherPose.AcrossNormalized,
                            Mathf.Max(
                                0.05f,
                                anchorSample.LeftSurfaceHalfWidth),
                            Mathf.Max(
                                0.05f,
                                anchorSample.RightSurfaceHalfWidth));
                StylizedRiverFoamPreparedMajorRegion otherPrepared =
                    prepared[otherSlot.PreparedIndex];
                float otherRadius = Mathf.Max(
                    otherPrepared.MajorHalfExtentCells,
                    otherPrepared.MinorHalfExtentCells) *
                    otherPose.MetresPerCandidateCell *
                    Mathf.Max(otherPose.ScaleAlong, otherPose.ScaleAcross);
                float alongDistance = Mathf.Abs(
                    anchor.CentreLocalDistance - otherPose.LocalDistance);
                float acrossDistance = Mathf.Abs(
                    anchorAcrossMetres - otherAcrossMetres);
                float normalisedDistance = Mathf.Sqrt(
                    alongDistance * alongDistance +
                    acrossDistance * acrossDistance) /
                    Mathf.Max(
                        0.25f,
                        (anchorRadius + otherRadius) * 0.82f);
                minimumNormalisedDistance = Mathf.Min(
                    minimumNormalisedDistance,
                    normalisedDistance);
            }

            return minimumNormalisedDistance;
        }

        private void ResolveMajorOccurrenceBudget(
            ref MajorEvolutionSlot slot)
        {
            uint cycleSeed = ResolveMajorOccurrenceSeed(slot, 70u);
            float baseUnits = river != null
                ? river.FoamMajorLifetimeUnits
                : 6f;
            float deviation = river != null
                ? river.FoamMajorLifetimeUnitDeviation
                : 2f;
            float signedVariation = Mathf.Lerp(
                -deviation,
                deviation,
                HashMajorEvolution(cycleSeed, 1u));
            slot.LifetimeUnitBudget = Mathf.Max(
                1f,
                baseUnits + signedVariation);
            slot.MaximumOccurrenceSeconds =
                slot.LifetimeUnitBudget *
                MajorLifetimeSecondsPerUnit *
                MajorLifetimeSafetyMultiplier;
        }

        private void RefreshMajorEvolutionLifetimeBudgetsIfNeeded()
        {
            int signature = ResolveMajorEvolutionLifetimeInputSignature();
            if (signature == majorEvolutionLifetimeInputSignature)
            {
                return;
            }

            for (int index = 0;
                 index < majorEvolutionSlots.Length;
                 index++)
            {
                ResolveMajorOccurrenceBudget(
                    ref majorEvolutionSlots[index]);
            }

            majorEvolutionLifetimeInputSignature = signature;
        }

        private int ResolveMajorEvolutionLifetimeInputSignature()
        {
            if (river == null)
            {
                return int.MinValue;
            }

            unchecked
            {
                int hash = 31;
                hash = hash * 37 + Mathf.RoundToInt(
                    river.FoamMajorLifetimeUnits * 1000f);
                hash = hash * 37 + Mathf.RoundToInt(
                    river.FoamMajorLifetimeUnitDeviation * 1000f);
                return hash;
            }
        }

        private void ResolveMajorDwell(
            ref MajorEvolutionSlot slot,
            uint stream)
        {
            uint cycleSeed = ResolveMajorCycleSeed(slot, stream);
            float dwell = Mathf.Lerp(
                MajorMinimumDwellSeconds,
                MajorMaximumDwellSeconds,
                HashMajorEvolution(cycleSeed, 1u));
            slot.DwellRemaining = dwell;
            slot.LastDwellDuration = dwell;
            majorEvolutionObservedMinimumDwell = Mathf.Min(
                majorEvolutionObservedMinimumDwell,
                dwell);
            majorEvolutionObservedMaximumDwell = Mathf.Max(
                majorEvolutionObservedMaximumDwell,
                dwell);
        }

        private bool ShouldRecycleMajor(MajorEvolutionSlot slot)
        {
            // A normal five-second dwell-plus-move cycle and one completed hop
            // consume approximately one unit. Slow occurrences are therefore
            // charged by time, active occurrences by hops, and neither factor
            // can independently permit an unusually persistent local clump.
            float usedUnits =
                MajorLifetimeTimeWeight *
                    (slot.OccurrenceElapsed /
                        MajorLifetimeSecondsPerUnit) +
                MajorLifetimeHopWeight * slot.HopIndex;
            return usedUnits >= slot.LifetimeUnitBudget ||
                slot.OccurrenceElapsed >=
                    slot.MaximumOccurrenceSeconds;
        }

        private float ResolveMajorEgressStart()
        {
            return StylizedRiverFoamMajorTopology
                .ResolveEvolutionEgressStart(validFieldLength);
        }

        private bool IsInMajorEgress(float localDistance)
        {
            return localDistance >= ResolveMajorEgressStart();
        }

        private bool BuildEvolvingMajorField()
        {
            if (!majorEvolutionReady || computeShader == null ||
                majorEvolutionBuffer == null ||
                majorMaskTextureArray == null ||
                hostedNegativeEvolutionBuffer == null ||
                hostedNegativeMaskTextureArray == null ||
                freeWaterEvolutionBuffer == null ||
                freeWaterNegativeMaskTextureArray == null ||
                evolvingMajorTexture == null ||
                evolvingHostedNegativeTexture == null ||
                evolvingFreeWaterNegativeTexture == null ||
                evolvingConnectorTexture == null ||
                evolvingWeakSpanNegativeTexture == null ||
                topologyGeneratedTexture == null ||
                boundaryTexture == null ||
                obstacleExclusionTexture == null ||
                metricBuffer == null ||
                buildEvolvingMajorSupportKernel < 0 ||
                river == null || !river.Domain.IsValid)
            {
                return false;
            }

            IReadOnlyList<StylizedRiverFoamPreparedMajorRegion> prepared =
                majorTopology.PreparedRegions;
            for (int index = 0;
                 index < majorEvolutionSlots.Length;
                 index++)
            {
                MajorEvolutionSlot slot = majorEvolutionSlots[index];
                MajorEvolutionPose pose = ResolveMajorPose(slot);
                StylizedRiverFoamPreparedMajorRegion preparedRegion =
                    prepared[slot.PreparedIndex];
                majorEvolutionGpuData[index] =
                    new FoamMajorEvolutionData
                    {
                        CentreAndPlacement = new Vector4(
                            pose.LocalDistance,
                            pose.AcrossNormalized,
                            pose.OrientationRadians,
                            pose.MetresPerCandidateCell),
                        CandidateShape = new Vector4(
                            preparedRegion.CentroidCells.x,
                            preparedRegion.CentroidCells.y,
                            preparedRegion.PrincipalAngleRadians,
                            index),
                        CandidateExtents = new Vector4(
                            preparedRegion.MajorHalfExtentCells,
                            preparedRegion.MinorHalfExtentCells,
                            preparedRegion.MaskResolution,
                            pose.SupportScale),
                        Morph = new Vector4(
                            pose.ScaleAlong,
                            pose.ScaleAcross,
                            pose.Shear,
                            0f),
                        Warp = new Vector4(
                            pose.WarpAlong,
                            pose.WarpAcross,
                            pose.WarpPhaseA,
                            pose.WarpPhaseB)
                    };
            }

            IReadOnlyList<StylizedRiverFoamPreparedHostedNegativeRegion>
                hostedPrepared = pocketTopology != null
                    ? pocketTopology.PreparedHostedRegions
                    : Array.Empty<
                        StylizedRiverFoamPreparedHostedNegativeRegion>();
            for (int index = 0;
                 index < hostedNegativeEvolutionSlots.Length;
                 index++)
            {
                HostedNegativeEvolutionSlot slot =
                    hostedNegativeEvolutionSlots[index];
                HostedNegativeEvolutionPose pose =
                    ResolveHostedNegativePose(slot);
                StylizedRiverFoamPreparedHostedNegativeRegion preparedRegion =
                    hostedPrepared[slot.PreparedIndex];
                hostedNegativeEvolutionGpuData[index] =
                    new FoamHostedNegativeEvolutionData
                    {
                        HostAndMask = new Vector4(
                            slot.HostSlotIndex,
                            index,
                            pose.StrengthScale,
                            slot.RegionClass ==
                                StylizedRiverFoamNegativeRegionClass
                                    .EdgeCavity
                                ? 1f
                                : 0f),
                        CentreAndOffset = new Vector4(
                            preparedRegion.CentreCandidateCells.x,
                            preparedRegion.CentreCandidateCells.y,
                            pose.OffsetCells.x,
                            pose.OffsetCells.y),
                        Morph = new Vector4(
                            pose.ScaleAlong,
                            pose.ScaleAcross,
                            pose.RotationRadians,
                            0f)
                    };
            }

            IReadOnlyList<StylizedRiverFoamPreparedFreeWaterRegion>
                freeWaterPrepared = pocketTopology != null
                    ? pocketTopology.PreparedFreeWaterRegions
                    : Array.Empty<StylizedRiverFoamPreparedFreeWaterRegion>();
            for (int index = 0;
                 index < freeWaterEvolutionSlots.Length;
                 index++)
            {
                FreeWaterEvolutionSlot slot = freeWaterEvolutionSlots[index];
                FreeWaterEvolutionPose pose = ResolveFreeWaterPose(slot);
                StylizedRiverFoamPreparedFreeWaterRegion preparedRegion =
                    freeWaterPrepared[slot.PreparedIndex];
                freeWaterEvolutionGpuData[index] =
                    new FoamFreeWaterEvolutionData
                    {
                        CentreAndPlacement = new Vector4(
                            pose.LocalDistance,
                            pose.AcrossNormalized,
                            pose.OrientationRadians,
                            preparedRegion.MetresPerCell),
                        MaskAndStrength = new Vector4(
                            preparedRegion.MaskResolution * 0.5f,
                            preparedRegion.MaskResolution * 0.5f,
                            index,
                            pose.StrengthScale),
                        Morph = new Vector4(
                            0f,
                            0f,
                            pose.ScaleAlong,
                            pose.ScaleAcross)
                    };
            }

            UpdateConnectorEvolutionDescriptors(true);
            EnsureConnectorIdentityBuffers();

            using (MajorEvolutionUploadProfilerMarker.Auto())
            {
                majorEvolutionBuffer.SetData(majorEvolutionGpuData);
                if (hostedNegativeEvolutionSlots.Length > 0)
                {
                    hostedNegativeEvolutionBuffer.SetData(
                        hostedNegativeEvolutionGpuData,
                        0,
                        0,
                        hostedNegativeEvolutionSlots.Length);
                }
                if (freeWaterEvolutionSlots.Length > 0)
                {
                    freeWaterEvolutionBuffer.SetData(
                        freeWaterEvolutionGpuData,
                        0,
                        0,
                        freeWaterEvolutionSlots.Length);
                }
                if (connectorIdentityReconstructionReady)
                {
                    connectorIdentityBuffer.SetData(
                        connectorIdentityGpuData);
                    connectorPathPointBuffer.SetData(
                        connectorPathPointGpuData);
                }
            }

            using (MajorEvolutionBuildProfilerMarker.Auto())
            {
                ConfigureTopologyParameters(0f);
                computeShader.SetInt(
                    "_FoamMajorEvolutionCount",
                    majorEvolutionSlots.Length);
                computeShader.SetInt(
                    "_FoamHostedNegativeEvolutionCount",
                    hostedNegativeEvolutionSlots.Length);
                computeShader.SetInt(
                    "_FoamFreeWaterEvolutionCount",
                    freeWaterEvolutionSlots.Length);
                computeShader.SetInt(
                    "_FoamConnectorIdentityCount",
                    connectorIdentityReconstructionReady
                        ? connectorIdentityGpuData.Length
                        : 0);
                computeShader.SetInt(
                    "_FoamWeakSpanIdentityCount",
                    weakSpanIdentityReconstructionReady
                        ? weakSpanIdentityGpuData.Length
                        : 0);
                computeShader.SetInts(
                    "_FoamMajorMaskDimensions",
                    majorMaskTextureArray.width,
                    majorMaskTextureArray.height);
                computeShader.SetInts(
                    "_FoamHostedNegativeMaskDimensions",
                    hostedNegativeMaskTextureArray.width,
                    hostedNegativeMaskTextureArray.height);
                computeShader.SetInts(
                    "_FoamFreeWaterMaskDimensions",
                    freeWaterNegativeMaskTextureArray.width,
                    freeWaterNegativeMaskTextureArray.height);
                computeShader.SetBuffer(
                    buildEvolvingMajorSupportKernel,
                    "_FoamMajorEvolutionRecords",
                    majorEvolutionBuffer);
                computeShader.SetBuffer(
                    buildEvolvingMajorSupportKernel,
                    "_FoamMetricRows",
                    metricBuffer);
                computeShader.SetBuffer(
                    buildEvolvingMajorSupportKernel,
                    "_FoamHostedNegativeEvolutionRecords",
                    hostedNegativeEvolutionBuffer);
                computeShader.SetBuffer(
                    buildEvolvingMajorSupportKernel,
                    "_FoamFreeWaterEvolutionRecords",
                    freeWaterEvolutionBuffer);
                computeShader.SetBuffer(
                    buildEvolvingMajorSupportKernel,
                    "_FoamConnectorIdentityRecords",
                    connectorIdentityBuffer);
                computeShader.SetBuffer(
                    buildEvolvingMajorSupportKernel,
                    "_FoamConnectorPathPoints",
                    connectorPathPointBuffer);
                computeShader.SetBuffer(
                    buildEvolvingMajorSupportKernel,
                    "_FoamWeakSpanIdentityRecords",
                    weakSpanIdentityBuffer);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamMajorMasks",
                    majorMaskTextureArray);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamHostedNegativeMasks",
                    hostedNegativeMaskTextureArray);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamFreeWaterNegativeMasks",
                    freeWaterNegativeMaskTextureArray);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamTopologyGeneratedRead",
                    topologyGeneratedTexture);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamBoundary",
                    boundaryTexture);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamObstacleExclusionRead",
                    obstacleExclusionTexture);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamEvolvingMajorWrite",
                    evolvingMajorTexture);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamEvolvingHostedNegativeWrite",
                    evolvingHostedNegativeTexture);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamEvolvingFreeWaterNegativeWrite",
                    evolvingFreeWaterNegativeTexture);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamEvolvingConnectorWrite",
                    evolvingConnectorTexture);
                computeShader.SetTexture(
                    buildEvolvingMajorSupportKernel,
                    "_FoamEvolvingWeakSpanNegativeWrite",
                    evolvingWeakSpanNegativeTexture);
                Dispatch(
                    buildEvolvingMajorSupportKernel,
                    guidanceWidth,
                    guidanceHeight);
            }

            majorEvolutionReconstructionTicks++;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            RequestHostedNegativeInitialParityIfNeeded();
            RequestConnectorIdentityParityIfNeeded();
            RequestWeakSpanIdentityParityIfNeeded();
#endif
            return true;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void RequestHostedNegativeInitialParityIfNeeded()
        {
            // This comparison is intentionally restricted to Editor and
            // development diagnostics. Normal runs do not read the evolving
            // field back or pay any parity-validation cost.
            if (!hostedNegativeInitialParityPending ||
                hostedNegativeInitialParityReadbackPending ||
                !IsTopologyDebugActive ||
                !SystemInfo.supportsAsyncGPUReadback ||
                evolvingHostedNegativeTexture == null ||
                pocketTopology == null)
            {
                return;
            }

            hostedNegativeInitialParityPending = false;
            hostedNegativeInitialParityReadbackPending = true;
            int generation = hostedNegativeInitialParityGeneration;
            StylizedRiverFoamPocketTopology requestedTopology = pocketTopology;
            AsyncGPUReadback.Request(
                evolvingHostedNegativeTexture,
                0,
                TextureFormat.RFloat,
                request =>
                {
                    if (this == null ||
                        generation != hostedNegativeInitialParityGeneration ||
                        requestedTopology != pocketTopology)
                    {
                        return;
                    }

                    hostedNegativeInitialParityReadbackPending = false;
                    if (request.hasError)
                    {
                        hostedNegativeInitialParityAvailable = false;
                        return;
                    }

                    var data = request.GetData<float>();
                    float[] expected = requestedTopology.HostedPressureData;
                    float[] fallback =
                        requestedTopology.HostedFallbackPressureData;
                    int count = Mathf.Min(
                        data.Length,
                        Mathf.Min(expected.Length, fallback.Length));
                    if (count <= 0)
                    {
                        hostedNegativeInitialParityAvailable = false;
                        return;
                    }

                    double totalDifference = 0.0;
                    int relevantCount = 0;
                    float maximumDifference = 0f;
                    for (int index = 0; index < count; index++)
                    {
                        float reconstructed = Mathf.Max(
                            Mathf.Clamp01(data[index]),
                            Mathf.Clamp01(fallback[index]));
                        float expectedValue = Mathf.Clamp01(expected[index]);
                        float difference = Mathf.Abs(
                            reconstructed - expectedValue);
                        if (Mathf.Max(reconstructed, expectedValue) > 0.01f)
                        {
                            totalDifference += difference;
                            relevantCount++;
                        }
                        maximumDifference = Mathf.Max(
                            maximumDifference,
                            difference);
                    }

                    hostedNegativeInitialParityMeanDifference =
                        relevantCount > 0
                            ? (float)(totalDifference / relevantCount)
                            : 0f;
                    hostedNegativeInitialParityMaximumDifference =
                        maximumDifference;
                    hostedNegativeInitialParityAvailable = true;
                });
        }

        private void RequestConnectorIdentityParityIfNeeded()
        {
            // Identity parity is development-only proof that the retained
            // Connector path records reconstruct the accepted static field.
            if (!connectorIdentityParityPending ||
                connectorIdentityParityReadbackPending ||
                !IsTopologyDebugActive ||
                !SystemInfo.supportsAsyncGPUReadback ||
                evolvingConnectorTexture == null ||
                connectorTopology == null)
            {
                return;
            }

            connectorIdentityParityPending = false;
            connectorIdentityParityReadbackPending = true;
            int generation = connectorIdentityParityGeneration;
            StylizedRiverFoamConnectorTopology requestedTopology =
                connectorTopology;
            AsyncGPUReadback.Request(
                evolvingConnectorTexture,
                0,
                TextureFormat.RFloat,
                request =>
                {
                    if (this == null ||
                        generation != connectorIdentityParityGeneration ||
                        requestedTopology != connectorTopology)
                    {
                        return;
                    }

                    connectorIdentityParityReadbackPending = false;
                    if (request.hasError)
                    {
                        connectorIdentityParityAvailable = false;
                        return;
                    }

                    var data = request.GetData<float>();
                    float[] expected = requestedTopology.SupportData;
                    int count = Mathf.Min(data.Length, expected.Length);
                    if (count <= 0)
                    {
                        connectorIdentityParityAvailable = false;
                        return;
                    }

                    double totalDifference = 0.0;
                    int relevantCount = 0;
                    float maximumDifference = 0f;
                    for (int index = 0; index < count; index++)
                    {
                        float reconstructed = Mathf.Clamp01(data[index]);
                        float expectedValue = Mathf.Clamp01(expected[index]);
                        float difference = Mathf.Abs(
                            reconstructed - expectedValue);
                        if (Mathf.Max(reconstructed, expectedValue) > 0.01f)
                        {
                            totalDifference += difference;
                            relevantCount++;
                        }
                        maximumDifference = Mathf.Max(
                            maximumDifference,
                            difference);
                    }

                    connectorIdentityParityMeanDifference =
                        relevantCount > 0
                            ? (float)(totalDifference / relevantCount)
                            : 0f;
                    connectorIdentityParityMaximumDifference =
                        maximumDifference;
                    connectorIdentityParityAvailable = true;
                });
        }

        private void RequestWeakSpanIdentityParityIfNeeded()
        {
            // Weak-Span identity parity remains debug-only. Normal gameplay
            // never reads the reconstructed pressure field back.
            if (!weakSpanIdentityParityPending ||
                weakSpanIdentityParityReadbackPending ||
                !IsTopologyDebugActive ||
                !SystemInfo.supportsAsyncGPUReadback ||
                evolvingWeakSpanNegativeTexture == null ||
                pocketTopology == null)
            {
                return;
            }

            weakSpanIdentityParityPending = false;
            weakSpanIdentityParityReadbackPending = true;
            int generation = weakSpanIdentityParityGeneration;
            StylizedRiverFoamPocketTopology requestedTopology = pocketTopology;
            AsyncGPUReadback.Request(
                evolvingWeakSpanNegativeTexture,
                0,
                TextureFormat.RFloat,
                request =>
                {
                    if (this == null ||
                        generation != weakSpanIdentityParityGeneration ||
                        requestedTopology != pocketTopology)
                    {
                        return;
                    }

                    weakSpanIdentityParityReadbackPending = false;
                    if (request.hasError)
                    {
                        weakSpanIdentityParityAvailable = false;
                        return;
                    }

                    var data = request.GetData<float>();
                    float[] expected =
                        requestedTopology.StaticIndependentPressureData;
                    int count = Mathf.Min(data.Length, expected.Length);
                    if (count <= 0)
                    {
                        weakSpanIdentityParityAvailable = false;
                        return;
                    }

                    double totalDifference = 0.0;
                    int relevantCount = 0;
                    float maximumDifference = 0f;
                    for (int index = 0; index < count; index++)
                    {
                        float reconstructed = Mathf.Clamp01(data[index]);
                        float expectedValue = Mathf.Clamp01(expected[index]);
                        float difference = Mathf.Abs(
                            reconstructed - expectedValue);
                        if (Mathf.Max(reconstructed, expectedValue) > 0.01f)
                        {
                            totalDifference += difference;
                            relevantCount++;
                        }
                        maximumDifference = Mathf.Max(
                            maximumDifference,
                            difference);
                    }

                    weakSpanIdentityParityMeanDifference =
                        relevantCount > 0
                            ? (float)(totalDifference / relevantCount)
                            : 0f;
                    weakSpanIdentityParityMaximumDifference =
                        maximumDifference;
                    weakSpanIdentityParityAvailable = true;
                });
        }
#endif

        private MajorEvolutionPose ResolveMajorPose(
            MajorEvolutionSlot slot)
        {
            if (!slot.IsMoving || slot.MoveDuration <= 0.0001f)
            {
                return slot.Current;
            }

            float t = Mathf.Clamp01(
                slot.MoveElapsed / slot.MoveDuration);
            t = t * t * (3f - 2f * t);
            return new MajorEvolutionPose
            {
                LocalDistance = Mathf.Lerp(
                    slot.Start.LocalDistance,
                    slot.Target.LocalDistance,
                    t),
                AcrossNormalized = Mathf.Lerp(
                    slot.Start.AcrossNormalized,
                    slot.Target.AcrossNormalized,
                    t),
                OrientationRadians = LerpMajorAngle(
                    slot.Start.OrientationRadians,
                    slot.Target.OrientationRadians,
                    t),
                MetresPerCandidateCell = Mathf.Lerp(
                    slot.Start.MetresPerCandidateCell,
                    slot.Target.MetresPerCandidateCell,
                    t),
                ScaleAlong = Mathf.Lerp(
                    slot.Start.ScaleAlong,
                    slot.Target.ScaleAlong,
                    t),
                ScaleAcross = Mathf.Lerp(
                    slot.Start.ScaleAcross,
                    slot.Target.ScaleAcross,
                    t),
                Shear = Mathf.Lerp(
                    slot.Start.Shear,
                    slot.Target.Shear,
                    t),
                WarpAlong = Mathf.Lerp(
                    slot.Start.WarpAlong,
                    slot.Target.WarpAlong,
                    t),
                WarpAcross = Mathf.Lerp(
                    slot.Start.WarpAcross,
                    slot.Target.WarpAcross,
                    t),
                WarpPhaseA = LerpFullAngle(
                    slot.Start.WarpPhaseA,
                    slot.Target.WarpPhaseA,
                    t),
                WarpPhaseB = LerpFullAngle(
                    slot.Start.WarpPhaseB,
                    slot.Target.WarpPhaseB,
                    t),
                SupportScale = Mathf.Lerp(
                    slot.Start.SupportScale,
                    slot.Target.SupportScale,
                    t)
            };
        }

        private int CountMovingMajorSlots()
        {
            int count = 0;
            for (int index = 0;
                 index < majorEvolutionSlots.Length;
                 index++)
            {
                if (majorEvolutionSlots[index].IsMoving)
                {
                    count++;
                }
            }

            return count;
        }

        private int CountMovingFreeWaterSlots()
        {
            int count = 0;
            for (int index = 0;
                 index < freeWaterEvolutionSlots.Length;
                 index++)
            {
                if (freeWaterEvolutionSlots[index].IsMoving)
                {
                    count++;
                }
            }

            return count;
        }

        private int CountHostedNegativeSlots(
            StylizedRiverFoamNegativeRegionClass regionClass)
        {
            int count = 0;
            for (int index = 0;
                 index < hostedNegativeEvolutionSlots.Length;
                 index++)
            {
                if (hostedNegativeEvolutionSlots[index].RegionClass ==
                    regionClass)
                {
                    count++;
                }
            }

            return count;
        }

        private uint ResolveMajorCycleSeed(
            MajorEvolutionSlot slot,
            uint stream)
        {
            return EvolutionMixBits(
                slot.StableId ^
                ((uint)(slot.RecycleCount + 1) * 0x9E3779B9u) ^
                ((uint)(slot.HopIndex + 1) * 0x85EBCA6Bu) ^
                EvolutionMixBits(stream + 0xC2B2AE35u));
        }

        private uint ResolveMajorOccurrenceSeed(
            MajorEvolutionSlot slot,
            uint stream)
        {
            // Occurrence-level values must remain stable throughout every hop.
            // Excluding HopIndex also means Inspector changes preserve the same
            // deterministic deviation selector for the current occurrence.
            return EvolutionMixBits(
                slot.StableId ^
                ((uint)(slot.RecycleCount + 1) * 0x9E3779B9u) ^
                EvolutionMixBits(stream + 0xC2B2AE35u));
        }

        private uint ResolveFreeWaterCycleSeed(
            FreeWaterEvolutionSlot slot,
            uint stream)
        {
            return EvolutionMixBits(
                slot.StableId ^
                ((uint)(slot.RecycleCount + 1) * 0x9E3779B9u) ^
                ((uint)(slot.HopIndex + 1) * 0x85EBCA6Bu) ^
                EvolutionMixBits(stream + 0x7F4A7C15u));
        }

        private uint ResolveFreeWaterOccurrenceSeed(
            FreeWaterEvolutionSlot slot,
            uint stream)
        {
            return EvolutionMixBits(
                slot.StableId ^
                ((uint)(slot.RecycleCount + 1) * 0x9E3779B9u) ^
                EvolutionMixBits(stream + 0x7F4A7C15u));
        }

        private static float HashMajorEvolution(uint seed, uint stream)
        {
            return (EvolutionMixBits(
                seed ^ EvolutionMixBits(stream + 0x27D4EB2Fu)) &
                0x00FFFFFFu) / 16777216f;
        }

        private static uint EvolutionMixBits(uint value)
        {
            value ^= value >> 16;
            value *= 0x7FEB352Du;
            value ^= value >> 15;
            value *= 0x846CA68Bu;
            value ^= value >> 16;
            return value;
        }

        private static float HashConnectorIdentity(
            uint stableId,
            uint stream)
        {
            return (EvolutionMixBits(
                stableId ^ EvolutionMixBits(stream + 0xC2B2AE35u)) &
                0x00FFFFFFu) / 16777216f;
        }

        private static float WrapMajorAngle(float angle)
        {
            while (angle > Mathf.PI * 0.5f)
            {
                angle -= Mathf.PI;
            }
            while (angle < -Mathf.PI * 0.5f)
            {
                angle += Mathf.PI;
            }
            return angle;
        }

        private static float LerpMajorAngle(float from, float to, float t)
        {
            float delta = WrapMajorAngle(to - from);
            return WrapMajorAngle(from + delta * t);
        }

        private static float LerpFullAngle(float from, float to, float t)
        {
            float delta = Mathf.Repeat(
                to - from + Mathf.PI,
                Mathf.PI * 2f) - Mathf.PI;
            return from + delta * t;
        }

        private int ResolveMajorTopologyInputSignature()
        {
            if (river == null || !river.Domain.IsValid ||
                fieldWidth < 2 || fieldHeight < 2)
            {
                return int.MinValue + 1;
            }

            return ResolveMajorTopologyInputSignature(
                river.Domain,
                obstacleGeometryVersion,
                river.Quality,
                fieldWidth,
                fieldHeight,
                river.FoamMajorSupportSeed,
                river.FoamMajorSupportAmount,
                river.FoamMajorSupportSize,
                river.FoamMajorSupportSizeVariation,
                river.FoamMajorRecycleTerritoryDeviationPercent,
                river.ShoreMotion);
        }

        private int ResolveConnectorTopologyInputSignature()
        {
            if (river == null || !river.Domain.IsValid ||
                fieldWidth < 2 || fieldHeight < 2 ||
                majorTopology == null)
            {
                return int.MinValue + 2;
            }

            return ResolveConnectorTopologyInputSignature(
                majorTopologyInputSignature,
                river.FoamConnectorAmount,
                river.FoamConnectorDirectness,
                river.FoamConnectorLengthPreference);
        }

        private int ResolvePocketTopologyInputSignature()
        {
            if (river == null || !river.Domain.IsValid ||
                fieldWidth < 2 || fieldHeight < 2 ||
                majorTopology == null || connectorTopology == null)
            {
                return int.MinValue + 3;
            }

            return ResolvePocketTopologyInputSignature(
                majorTopologyInputSignature,
                connectorTopologyInputSignature,
                river.FoamInteriorPocketAmount,
                river.FoamEdgeCavityAmount,
                river.FoamConnectorWeakSpanAmount,
                river.FoamFreeWaterEventAmount);
        }

        private void ResolveRequestedTopologySignatures(
            out int majorSignature,
            out int connectorSignature,
            out int pocketSignature,
            out int combinedSignature)
        {
            majorSignature = ResolveMajorTopologyInputSignature(
                river.Domain,
                obstacleGeometryVersion,
                river.Quality,
                fieldWidth,
                fieldHeight,
                river.FoamMajorSupportSeed,
                river.FoamMajorSupportAmount,
                river.FoamMajorSupportSize,
                river.FoamMajorSupportSizeVariation,
                river.FoamMajorRecycleTerritoryDeviationPercent,
                river.ShoreMotion);
            connectorSignature = ResolveConnectorTopologyInputSignature(
                majorSignature,
                river.FoamConnectorAmount,
                river.FoamConnectorDirectness,
                river.FoamConnectorLengthPreference);
            pocketSignature = ResolvePocketTopologyInputSignature(
                majorSignature,
                connectorSignature,
                river.FoamInteriorPocketAmount,
                river.FoamEdgeCavityAmount,
                river.FoamConnectorWeakSpanAmount,
                river.FoamFreeWaterEventAmount);
            combinedSignature = CombineTopologySignatures(
                majorSignature,
                connectorSignature,
                pocketSignature);
        }

        private int ResolveRequestedTopologySignature()
        {
            ResolveRequestedTopologySignatures(
                out _,
                out _,
                out _,
                out int combinedSignature);
            return combinedSignature;
        }

        private int ResolveActiveTopologySignature()
        {
            if (majorTopology == null || connectorTopology == null ||
                pocketTopology == null)
            {
                return int.MinValue + 4;
            }

            return CombineTopologySignatures(
                majorTopologyInputSignature,
                connectorTopologyInputSignature,
                pocketTopologyInputSignature);
        }

        private static int ResolveMajorTopologyInputSignature(
            RiverDomainSnapshot domain,
            int obstacleVersion,
            StylizedRiverQuality quality,
            int width,
            int height,
            int majorSeed,
            float majorAmount,
            float majorSize,
            float majorSizeVariation,
            float recycleTerritoryDeviationPercent,
            float shoreMotion)
        {
            if (domain == null || !domain.IsValid || width < 2 || height < 2)
            {
                return int.MinValue + 1;
            }

            unchecked
            {
                int hash = 17;
                hash = hash * 31 + domain.Version;
                hash = hash * 31 + obstacleVersion;
                hash = hash * 31 + (int)quality;
                hash = hash * 31 + width;
                hash = hash * 31 + height;
                hash = hash * 31 + majorSeed;
                hash = hash * 31 + Mathf.RoundToInt(majorAmount * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(majorSize * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(
                    majorSizeVariation * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(
                    recycleTerritoryDeviationPercent * 1000f);
                hash = hash * 31 + Mathf.RoundToInt(shoreMotion * 10000f);
                return hash;
            }
        }

        private static int ResolveConnectorTopologyInputSignature(
            int majorSignature,
            float connectorAmount,
            float connectorDirectness,
            float connectorLengthPreference)
        {
            unchecked
            {
                int hash = 23;
                hash = hash * 31 + majorSignature;
                hash = hash * 31 + Mathf.RoundToInt(
                    connectorAmount * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(
                    connectorDirectness * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(
                    connectorLengthPreference * 10000f);
                return hash;
            }
        }

        private static int ResolvePocketTopologyInputSignature(
            int majorSignature,
            int connectorSignature,
            float interiorPocketAmount,
            float edgeCavityAmount,
            float weakSpanAmount,
            float freeWaterAmount)
        {
            unchecked
            {
                int hash = 29;
                hash = hash * 31 + majorSignature;
                hash = hash * 31 + connectorSignature;
                hash = hash * 31 + Mathf.RoundToInt(
                    interiorPocketAmount * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(
                    edgeCavityAmount * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(
                    weakSpanAmount * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(
                    freeWaterAmount * 10000f);
                hash = hash * 31 + 4;
                return hash;
            }
        }

        private static int CombineTopologySignatures(
            int majorSignature,
            int connectorSignature,
            int pocketSignature)
        {
            unchecked
            {
                int hash = 43;
                hash = hash * 31 + majorSignature;
                hash = hash * 31 + connectorSignature;
                hash = hash * 31 + pocketSignature;
                return hash;
            }
        }

        private void ConfigureTopologyParameters(float deltaTime)
        {
            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetInts(
                "_FoamTopologyDimensions",
                guidanceWidth,
                guidanceHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamGlobalStart",
                allocatedGlobalStart);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat("_FoamDeltaTime", deltaTime);
            computeShader.SetFloat(
                "_FoamFlowSpeed",
                river.FlowSpeedMetresPerSecond * river.LiquidFactor);
            computeShader.SetFloat(
                "_FoamFlowDirection",
                river.FlowDirection);
            computeShader.SetFloat(
                "_FoamTime",
                ResolveInitializationMotionTime());
            computeShader.SetFloat("_FoamSeed", river.VisualSeed);
            computeShader.SetFloat(
                "_FoamMotionFlowSpeed",
                river.FlowSpeedMetresPerSecond);
            computeShader.SetFloat(
                "_FoamMotionWaveHeight",
                river.MotionWaveHeight);
            computeShader.SetFloat(
                "_FoamMotionWaveLength",
                river.MotionWaveLength);
            computeShader.SetFloat(
                "_FoamMotionWaveSteepness",
                river.MotionWaveSteepness);
            computeShader.SetFloat(
                "_FoamMotionTurbulence",
                river.MotionTurbulence);
            computeShader.SetFloat(
                "_FoamShoreMotion",
                river.ShoreMotion);
            computeShader.SetFloat(
                "_FoamShoreMotionWidth",
                river.ShoreMotionWidth);
            computeShader.SetFloat(
                "_FoamShoreWaveHeightScale",
                river.ShoreWaveHeightScale);
            computeShader.SetFloat(
                "_FoamShoreWaveLengthScale",
                river.ShoreWaveLengthScale);
            computeShader.SetFloat(
                "_FoamShoreWaveReach",
                river.ShoreWaveReach);
            computeShader.SetFloat(
                "_FoamShoreWaveTransitionLength",
                river.ShoreWaveTransitionLength);
            computeShader.SetFloat(
                "_FoamShoreWaveSizeVariation",
                river.ShoreWaveSizeVariation);
            computeShader.SetFloat(
                "_FoamShoreWaveSideAsymmetry",
                river.ShoreWaveSideAsymmetry);
            computeShader.SetFloat(
                "_FoamShoreWaveProfileVariation",
                river.ShoreWaveProfileVariation);
            computeShader.SetFloat(
                "_FoamShoreBankCover",
                river.ShorelineBankCover);
            computeShader.SetFloat(
                "_FoamFreezeAmount",
                river.FreezeAmount);
            computeShader.SetFloat(
                "_FoamShoreCaptureCoreWidth",
                ShoreSupportCoreWidthMetres);
            computeShader.SetFloat(
                "_FoamShoreCaptureFadeWidth",
                ShoreSupportFadeWidthMetres);
        }

        private void RefreshDynamicTopologySources(
            bool measureMetrics,
            bool refreshLiveInputs = true)
        {
            using var profilerScope = TopologyRefreshSourcesProfilerMarker.Auto();
            if (computeShader == null || topologyTexture == null ||
                topologySourcesTexture == null ||
                topologyGeneratedTexture == null ||
                evolvingMajorTexture == null ||
                evolvingHostedNegativeTexture == null ||
                evolvingFreeWaterNegativeTexture == null ||
                evolvingConnectorTexture == null ||
                evolvingWeakSpanNegativeTexture == null ||
                currentShoreEdgesTexture == null ||
                obstacleExclusionTexture == null ||
                boundaryTexture == null || metricBuffer == null ||
                buildCurrentShoreEdgesKernel < 0 ||
                composeTopologyKernel < 0)
            {
                return;
            }

            ConfigureTopologyParameters(0f);

            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            bool disturbanceAvailable =
                disturbanceRuntime != null &&
                disturbanceRuntime.IsAllocated;
            RenderTexture staticWakeSource = disturbanceAvailable
                ? disturbanceRuntime.StaticWakeSourceTexture
                : null;
            RenderTexture staticPressureSource = disturbanceAvailable
                ? disturbanceRuntime.StaticPressureTexture
                : null;
            bool staticWakeAvailable = IsCreatedTexture(staticWakeSource);
            bool staticPressureAvailable = IsCreatedTexture(staticPressureSource);
            Texture staticWakeTexture = staticWakeAvailable
                ? staticWakeSource
                : neutralDisturbanceTexture;
            Texture staticPressureTexture = staticPressureAvailable
                ? staticPressureSource
                : neutralDisturbanceTexture;
            Vector2Int staticWakeDimensions = staticWakeAvailable
                ? new Vector2Int(staticWakeSource.width, staticWakeSource.height)
                : Vector2Int.one;
            Vector2Int staticPressureDimensions = staticPressureAvailable
                ? new Vector2Int(
                    staticPressureSource.width,
                    staticPressureSource.height)
                : Vector2Int.one;

            computeShader.SetInts(
                "_FoamStaticWakeDimensions",
                staticWakeDimensions.x,
                staticWakeDimensions.y);
            computeShader.SetInts(
                "_FoamStaticPressureDimensions",
                staticPressureDimensions.x,
                staticPressureDimensions.y);
            computeShader.SetFloat(
                "_FoamDisturbanceEnabled",
                staticWakeAvailable || staticPressureAvailable ? 1f : 0f);

            if (refreshLiveInputs)
            {
                computeShader.SetBuffer(
                    buildCurrentShoreEdgesKernel,
                    "_FoamMetricRows",
                    metricBuffer);
                computeShader.SetTexture(
                    buildCurrentShoreEdgesKernel,
                    "_FoamCurrentShoreEdgesWrite",
                    currentShoreEdgesTexture);
                DispatchOneDimensional(
                    buildCurrentShoreEdgesKernel,
                    guidanceWidth,
                    64);

                UpdateObstacleExclusionMask();
            }

            using (TopologyComposeProfilerMarker.Auto())
            {
                computeShader.SetBuffer(
                    composeTopologyKernel,
                    "_FoamMetricRows",
                    metricBuffer);
                computeShader.SetTexture(
                    composeTopologyKernel,
                    "_FoamBoundary",
                    boundaryTexture);
                computeShader.SetTexture(
                    composeTopologyKernel,
                    "_FoamObstacleExclusionRead",
                    obstacleExclusionTexture);
                BindGeneratedTopologyInputs(composeTopologyKernel);
                ConfigureTopologyTransitionInputs(composeTopologyKernel);
                computeShader.SetTexture(
                    composeTopologyKernel,
                    "_FoamStaticWakeField",
                    staticWakeTexture);
                computeShader.SetTexture(
                    composeTopologyKernel,
                    "_FoamStaticPressureField",
                    staticPressureTexture);
                computeShader.SetTexture(
                    composeTopologyKernel,
                    "_FoamCurrentShoreEdgesRead",
                    currentShoreEdgesTexture);
                computeShader.SetTexture(
                    composeTopologyKernel,
                    "_FoamTopologyWrite",
                    topologyTexture);
                computeShader.SetTexture(
                    composeTopologyKernel,
                    "_FoamTopologySourcesWrite",
                    topologySourcesTexture);
                Dispatch(
                    composeTopologyKernel,
                    guidanceWidth,
                    guidanceHeight);
            }

            if (measureMetrics)
            {
                MeasureTopologyMetrics();
            }
        }

        private void MeasureTopologyMetrics()
        {
            using var profilerScope = DiagnosticsMeasureTopologyProfilerMarker.Auto();
            if (computeShader == null || currentState == null ||
                topologyTexture == null || topologySourcesTexture == null ||
                topologyMetricsBuffer == null ||
                resetTopologyMetricsKernel < 0 ||
                measureTopologyMetricsKernel < 0)
            {
                return;
            }

            computeShader.SetBuffer(
                resetTopologyMetricsKernel,
                "_FoamTopologyMetrics",
                topologyMetricsBuffer);
            DispatchOneDimensional(
                resetTopologyMetricsKernel,
                TopologyMetricCount,
                64);

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetInts(
                "_FoamTopologyDimensions",
                guidanceWidth,
                guidanceHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat(
                "_FoamVisibleThreshold",
                ProvisionalMaterialVisibleThreshold);
            computeShader.SetTexture(
                measureTopologyMetricsKernel,
                "_FoamTopologyRead",
                topologyTexture);
            computeShader.SetTexture(
                measureTopologyMetricsKernel,
                "_FoamTopologySourcesRead",
                topologySourcesTexture);
            computeShader.SetTexture(
                measureTopologyMetricsKernel,
                "_FoamStateRead",
                currentState);
            computeShader.SetTexture(
                measureTopologyMetricsKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetBuffer(
                measureTopologyMetricsKernel,
                "_FoamTopologyMetrics",
                topologyMetricsBuffer);
            Dispatch(
                measureTopologyMetricsKernel,
                guidanceWidth,
                guidanceHeight);
            RequestTopologyMetricsReadback();
        }

        private void RequestTopologyMetricsReadback()
        {
            if (!SystemInfo.supportsAsyncGPUReadback ||
                topologyMetricsReadbackPending ||
                topologyMetricsBuffer == null)
            {
                return;
            }

            topologyMetricsReadbackPending = true;
            int generation = topologyMetricsGeneration;
            ComputeBuffer requestedBuffer = topologyMetricsBuffer;
            AsyncGPUReadback.Request(
                requestedBuffer,
                request =>
                {
                    if (this == null || generation != topologyMetricsGeneration)
                    {
                        requestedBuffer?.Release();
                        return;
                    }

                    topologyMetricsReadbackPending = false;
                    if (request.hasError)
                    {
                        topologyMetricsAvailable = false;
                        return;
                    }

                    var data = request.GetData<uint>();
                    int count = Mathf.Min(
                        data.Length,
                        latestTopologyMetrics.Length);
                    for (int index = 0; index < count; index++)
                    {
                        latestTopologyMetrics[index] = data[index];
                    }

                    topologyMetricsAvailable = count == TopologyMetricCount;
                });
        }

        private float TopologyCoverageRatio(int numeratorIndex)
        {
            return TopologyRegionRatio(numeratorIndex, 0);
        }

        private float TopologyRegionRatio(
            int numeratorIndex,
            int denominatorIndex)
        {
            if (!topologyMetricsAvailable ||
                numeratorIndex < 0 || numeratorIndex >= TopologyMetricCount ||
                denominatorIndex < 0 || denominatorIndex >= TopologyMetricCount)
            {
                return 0f;
            }

            uint denominator = latestTopologyMetrics[denominatorIndex];
            return denominator > 0u
                ? latestTopologyMetrics[numeratorIndex] / (float)denominator
                : 0f;
        }

        private void MeasurePopulation()
        {
            using var profilerScope = DiagnosticsMeasurePopulationProfilerMarker.Auto();
            if (computeShader == null || currentState == null ||
                boundaryTexture == null || populationMetricsBuffer == null ||
                resetPopulationKernel < 0 || measurePopulationKernel < 0)
            {
                return;
            }

            computeShader.SetInt("_FoamChunkCount", chunkCount);
            computeShader.SetBuffer(
                resetPopulationKernel,
                "_FoamPopulationMetrics",
                populationMetricsBuffer);
            DispatchOneDimensional(
                resetPopulationKernel,
                chunkCount,
                64);

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
            computeShader.SetInt(
                "_FoamResolutionPerChunk",
                resolutionPerChunk);
            computeShader.SetFloat(
                "_FoamVisibleThreshold",
                ProvisionalMaterialVisibleThreshold);
            computeShader.SetTexture(
                measurePopulationKernel,
                "_FoamStateRead",
                currentState);
            computeShader.SetTexture(
                measurePopulationKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetInts(
                "_FoamGuidanceDimensions",
                guidanceWidth,
                guidanceHeight);
            computeShader.SetTexture(
                measurePopulationKernel,
                "_FoamGuidanceRead",
                guidanceTexture);
            computeShader.SetBuffer(
                measurePopulationKernel,
                "_FoamPopulationMetrics",
                populationMetricsBuffer);
            Dispatch(measurePopulationKernel, fieldWidth, fieldHeight);
        }

        private void UpdateFractureField(float deltaTime)
        {
            if (computeShader == null || currentState == null ||
                currentFracture == null || writeFracture == null ||
                updateFractureKernel < 0)
            {
                return;
            }

            computeShader.SetFloat(
                "_FoamFractureDeltaTime",
                Mathf.Max(0.0001f, deltaTime));
            computeShader.SetTexture(
                updateFractureKernel,
                "_FoamStateRead",
                currentState);
            computeShader.SetTexture(
                updateFractureKernel,
                "_FoamFractureRead",
                currentFracture);
            computeShader.SetTexture(
                updateFractureKernel,
                "_FoamFractureWrite",
                writeFracture);
            Dispatch(updateFractureKernel, fractureWidth, fractureHeight);
            (currentFracture, writeFracture) =
                (writeFracture, currentFracture);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamFractureRead",
                currentFracture);
        }

        private void ConfigureSharedComputeParameters(float deltaTime)
        {
            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
            computeShader.SetInts(
                "_FoamGuidanceDimensions",
                guidanceWidth,
                guidanceHeight);
            computeShader.SetInts(
                "_FoamFractureDimensions",
                fractureWidth,
                fractureHeight);
            computeShader.SetInt(
                "_FoamResolutionPerChunk",
                resolutionPerChunk);
            computeShader.SetInt("_FoamChunkCount", chunkCount);
            computeShader.SetFloat("_FoamDeltaTime", deltaTime);
            computeShader.SetFloat(
                "_FoamGlobalStart",
                river.Domain.GlobalDistanceMinimum);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat(
                "_FoamFlowSpeed",
                river.FlowSpeedMetresPerSecond *
                ProvisionalMaterialFlowFollow *
                river.LiquidFactor);
            // Pressure Support builds its fail-closed upstream support
            // envelope from the exact obstacle mask. Flow direction selects
            // which side of each solid cell is the object-facing front on both
            // normal-flow and reverse-flow rivers.
            computeShader.SetFloat(
                "_FoamFlowDirection",
                river.FlowDirection);
            computeShader.SetFloat(
                "_FoamEvolution",
                ProvisionalMaterialEvolution);
            computeShader.SetFloat("_FoamBreakup", ProvisionalMaterialBreakup);
            computeShader.SetFloat("_FoamSpread", ProvisionalMaterialSpread);
            computeShader.SetFloat("_FoamCohesion", ProvisionalMaterialCohesion);
            computeShader.SetFloat(
                "_FoamConnectivity",
                ProvisionalMaterialConnectivity);
            computeShader.SetFloat(
                "_FoamAmountDecay",
                DecayToFivePercent /
                Mathf.Max(0.05f, ProvisionalMaterialLifetime));
            computeShader.SetFloat(
                "_FoamFreshnessDecay",
                DecayToFivePercent /
                Mathf.Max(0.05f, ProvisionalMaterialFreshnessLifetime));
            computeShader.SetFloat(
                "_FoamIntegrityDamage",
                Mathf.Clamp01(ProvisionalMaterialIntegrityDamage));
            computeShader.SetFloat(
                "_FoamShoreRetention",
                ProvisionalMaterialShoreRetention);
            computeShader.SetFloat("_FoamTime", river.MotionTime);
            computeShader.SetFloat("_FoamSeed", river.VisualSeed);
            computeShader.SetFloat(
                "_FoamTargetCoverage",
                IsAutomaticMaterialSupplyActive
                    ? ProvisionalMaterialTargetCoverage
                    : 0f);
            computeShader.SetFloat(
                "_FoamSupplyRate",
                IsAutomaticMaterialSupplyActive
                    ? ProvisionalMaterialSupplyRate
                    : 0f);
            computeShader.SetFloat(
                "_FoamVisibleThreshold",
                ProvisionalMaterialVisibleThreshold);
            computeShader.SetFloat(
                "_FoamGuidanceStrength",
                ProvisionalMaterialGuidanceStrength);
            computeShader.SetFloat(
                "_FoamBoundaryAttraction",
                ProvisionalMaterialBoundaryAttraction);
            computeShader.SetFloat(
                "_FoamWakeReinforcement",
                ProvisionalMaterialWakeReinforcement);
            computeShader.SetFloat(
                "_FoamImpactReinforcement",
                ProvisionalMaterialImpactReinforcement);

            bool disturbanceAvailable =
                disturbanceRuntime != null &&
                disturbanceRuntime.IsAllocated;

            RenderTexture wakeSource = disturbanceAvailable
                ? disturbanceRuntime.CurrentWakeTexture
                : null;
            RenderTexture rippleSource = disturbanceAvailable
                ? disturbanceRuntime.CurrentRippleTexture
                : null;
            RenderTexture staticWakeSource = disturbanceAvailable
                ? disturbanceRuntime.StaticWakeSourceTexture
                : null;
            RenderTexture staticPressureSource = disturbanceAvailable
                ? disturbanceRuntime.StaticPressureTexture
                : null;

            bool wakeAvailable = IsCreatedTexture(wakeSource);
            bool rippleAvailable = IsCreatedTexture(rippleSource);
            bool staticWakeAvailable = IsCreatedTexture(staticWakeSource);
            bool staticPressureAvailable =
                IsCreatedTexture(staticPressureSource);

            // Every texture declared by a compute kernel must be bound for
            // every dispatch. Stage 5 allocates its optional fields
            // independently, so an allocated disturbance runtime does not
            // guarantee that each individual texture is already created.
            // Bind one explicit zero-valued RenderTexture for every missing
            // input rather than relying on a built-in Texture2D fallback.
            Texture wakeTexture = wakeAvailable
                ? wakeSource
                : neutralDisturbanceTexture;
            Texture rippleTexture = rippleAvailable
                ? rippleSource
                : neutralDisturbanceTexture;
            Texture staticWakeTexture = staticWakeAvailable
                ? staticWakeSource
                : neutralDisturbanceTexture;
            Texture staticPressureTexture = staticPressureAvailable
                ? staticPressureSource
                : neutralDisturbanceTexture;

            Vector2Int wakeDimensions = wakeAvailable
                ? new Vector2Int(wakeSource.width, wakeSource.height)
                : Vector2Int.one;
            Vector2Int rippleDimensions = rippleAvailable
                ? new Vector2Int(rippleSource.width, rippleSource.height)
                : Vector2Int.one;
            Vector2Int staticWakeDimensions = staticWakeAvailable
                ? new Vector2Int(staticWakeSource.width, staticWakeSource.height)
                : Vector2Int.one;
            Vector2Int staticPressureDimensions = staticPressureAvailable
                ? new Vector2Int(
                    staticPressureSource.width,
                    staticPressureSource.height)
                : Vector2Int.one;

            computeShader.SetFloat(
                "_FoamDisturbanceEnabled",
                disturbanceAvailable ? 1f : 0f);
            computeShader.SetInts(
                "_FoamWakeDimensions",
                wakeDimensions.x,
                wakeDimensions.y);
            computeShader.SetInts(
                "_FoamRippleDimensions",
                rippleDimensions.x,
                rippleDimensions.y);
            computeShader.SetInts(
                "_FoamStaticWakeDimensions",
                staticWakeDimensions.x,
                staticWakeDimensions.y);
            computeShader.SetInts(
                "_FoamStaticPressureDimensions",
                staticPressureDimensions.x,
                staticPressureDimensions.y);

            BindMotionKernel(
                advectForwardKernel,
                wakeTexture,
                rippleTexture,
                staticWakeTexture,
                staticPressureTexture);
            BindMotionKernel(
                advectReverseKernel,
                wakeTexture,
                rippleTexture,
                staticWakeTexture,
                staticPressureTexture);
            BindMotionKernel(
                simulateKernel,
                wakeTexture,
                rippleTexture,
                staticWakeTexture,
                staticPressureTexture);
            BindMotionKernel(
                updateFractureKernel,
                wakeTexture,
                rippleTexture,
                staticWakeTexture,
                staticPressureTexture);

            computeShader.SetTexture(
                advectForwardKernel,
                "_FoamStateRead",
                currentState);
            computeShader.SetTexture(
                advectForwardKernel,
                "_FoamAdvectionWrite",
                advectedState);

            computeShader.SetTexture(
                advectReverseKernel,
                "_FoamAdvectedRead",
                advectedState);
            computeShader.SetTexture(
                advectReverseKernel,
                "_FoamAdvectionWrite",
                reverseState);

            computeShader.SetBuffer(
                simulateKernel,
                "_FoamPopulationMetrics",
                populationMetricsBuffer);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamStateRead",
                currentState);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamAdvectedRead",
                advectedState);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamReverseRead",
                reverseState);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamFractureRead",
                currentFracture);
            computeShader.SetTexture(
                simulateKernel,
                "_FoamStateWrite",
                writeState);
        }

        private void BindMotionKernel(
            int kernel,
            Texture wakeTexture,
            Texture rippleTexture,
            Texture staticWakeTexture,
            Texture staticPressureTexture)
        {
            computeShader.SetBuffer(
                kernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetTexture(
                kernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamGuidanceRead",
                guidanceTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamWakeField",
                wakeTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamRippleField",
                rippleTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamStaticWakeField",
                staticWakeTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamStaticPressureField",
                staticPressureTexture);
        }

        private void DispatchSimulation(int startX, int countX)
        {
            computeShader.SetInt("_FoamRangeStart", startX);
            computeShader.SetInt("_FoamRangeCount", countX);

            // Corrected transport is intentionally a three-dispatch sequence:
            // forward advection, reverse error estimate, then bounded correction
            // plus population, topology, tearing, capture, and reinforcement.
            Dispatch(advectForwardKernel, countX, fieldHeight);
            Dispatch(advectReverseKernel, countX, fieldHeight);
            Dispatch(simulateKernel, countX, fieldHeight);
        }

        private void DispatchClearFracture(
            RenderTexture target,
            int startX,
            int countX)
        {
            if (computeShader == null || target == null ||
                clearFractureKernel < 0 || countX <= 0)
            {
                return;
            }

            computeShader.SetInts(
                "_FoamFractureDimensions",
                fractureWidth,
                fractureHeight);
            computeShader.SetInt("_FoamFractureRangeStart", startX);
            computeShader.SetInt("_FoamFractureRangeCount", countX);
            computeShader.SetTexture(
                clearFractureKernel,
                "_FoamFractureWrite",
                target);
            Dispatch(clearFractureKernel, countX, fractureHeight);
        }

        private void DispatchClear(RenderTexture target, int startX, int countX)
        {
            if (computeShader == null || target == null || clearKernel < 0 || countX <= 0)
            {
                return;
            }

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
            computeShader.SetInt("_FoamRangeStart", startX);
            computeShader.SetInt("_FoamRangeCount", countX);
            computeShader.SetTexture(clearKernel, "_FoamStateWrite", target);
            Dispatch(clearKernel, countX, fieldHeight);
        }


        private void ApplyBoundaryToState(RenderTexture target)
        {
            if (computeShader == null || target == null ||
                boundaryTexture == null || applyBoundaryKernel < 0 ||
                fieldWidth <= 0 || fieldHeight <= 0)
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
            computeShader.SetTexture(
                applyBoundaryKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                applyBoundaryKernel,
                "_FoamStateWrite",
                target);
            Dispatch(applyBoundaryKernel, fieldWidth, fieldHeight);
        }

        private void ClearChunk(int chunk)
        {
            if (stateA == null || stateB == null || chunk < 0 || chunk >= chunkCount)
            {
                return;
            }

            int startX = chunk * resolutionPerChunk;
            int countX = Mathf.Min(resolutionPerChunk, fieldWidth - startX);
            DispatchClear(stateA, startX, countX);
            DispatchClear(stateB, startX, countX);
            DispatchClear(advectedState, startX, countX);
            DispatchClear(reverseState, startX, countX);

            int fractureStart = Mathf.Clamp(
                Mathf.FloorToInt(startX / (float)Mathf.Max(1, fieldWidth) * fractureWidth),
                0,
                Mathf.Max(0, fractureWidth - 1));
            int fractureEnd = Mathf.Clamp(
                Mathf.CeilToInt((startX + countX) / (float)Mathf.Max(1, fieldWidth) * fractureWidth),
                fractureStart + 1,
                fractureWidth);
            int fractureCount = fractureEnd - fractureStart;
            DispatchClearFracture(fractureA, fractureStart, fractureCount);
            DispatchClearFracture(fractureB, fractureStart, fractureCount);
        }

        private void Dispatch(int kernel, int width, int height)
        {
            int groupsX = Mathf.CeilToInt(width / (float)ThreadGroupSize);
            int groupsY = Mathf.CeilToInt(height / (float)ThreadGroupSize);
            computeShader.Dispatch(kernel, groupsX, groupsY, 1);
            lastUpdateDispatches++;
            lastUpdateCellIterations += (long)width * height;
        }

        private void DispatchOneDimensional(
            int kernel,
            int count,
            int threadsPerGroup)
        {
            if (count <= 0)
            {
                return;
            }

            int groups = Mathf.CeilToInt(
                count / (float)Mathf.Max(1, threadsPerGroup));
            computeShader.Dispatch(kernel, groups, 1, 1);
            lastUpdateDispatches++;
            lastUpdateCellIterations += count;
        }

        private bool BindTopologyTransitionHold()
        {
            TopologyTransitionSnapshot snapshot = topologyTransitionSnapshot;
            if (snapshot == null || !snapshot.HoldsVisibleResources ||
                snapshot.PreviousState == null || snapshot.CurrentState == null ||
                snapshot.Topology == null || snapshot.TopologySources == null)
            {
                return false;
            }

            if (surfaceRenderer == null && river != null)
            {
                surfaceRenderer = river.SurfaceRenderer;
            }
            if (surfaceRenderer == null)
            {
                return false;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            surfaceRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(FoamEnabledId, 1f);
            propertyBlock.SetTexture(FoamPreviousId, snapshot.PreviousState);
            propertyBlock.SetTexture(FoamCurrentId, snapshot.CurrentState);
            propertyBlock.SetTexture(
                FoamGuidanceId,
                snapshot.Guidance != null
                    ? snapshot.Guidance
                    : Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamTopologyId, snapshot.Topology);
            propertyBlock.SetTexture(
                FoamTopologySourcesId,
                snapshot.TopologySources);
            propertyBlock.SetTexture(
                FoamFractureId,
                snapshot.Fracture != null
                    ? snapshot.Fracture
                    : Texture2D.blackTexture);
            propertyBlock.SetTexture(
                FoamBoundaryId,
                snapshot.Boundary != null
                    ? snapshot.Boundary
                    : Texture2D.blackTexture);
            propertyBlock.SetTexture(
                FoamObstacleExclusionId,
                snapshot.ObstacleExclusion != null
                    ? snapshot.ObstacleExclusion
                    : Texture2D.blackTexture);
            propertyBlock.SetFloat(
                FoamInterpolationId,
                snapshot.Interpolation);
            propertyBlock.SetFloat(FoamGlobalStartId, snapshot.GlobalStart);
            propertyBlock.SetFloat(
                FoamFieldLengthId,
                Mathf.Max(0.001f, snapshot.FieldLength));
            propertyBlock.SetColor(FoamColourId, river.FoamColour);
            propertyBlock.SetFloat(
                FoamStrengthId,
                ProvisionalMaterialStrength);
            propertyBlock.SetFloat(
                FoamCoverageId,
                ProvisionalMaterialCoverage);
            propertyBlock.SetFloat(
                FoamSharpnessId,
                ProvisionalMaterialSharpness);
            propertyBlock.SetFloat(
                FoamDetailScaleId,
                ProvisionalMaterialDetailScale);
            propertyBlock.SetFloat(
                FoamDetailStrengthId,
                ProvisionalMaterialDetailStrength);
            propertyBlock.SetFloat(
                FoamDebugViewId,
                (float)river.FoamDebugView);
            propertyBlock.SetFloat(FoamSeedId, river.VisualSeed);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
            return true;
        }

        private void BindField()
        {
            if (surfaceRenderer == null || currentState == null || previousState == null)
            {
                BindDisabled();
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            surfaceRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(FoamEnabledId, 1f);
            propertyBlock.SetTexture(FoamPreviousId, previousState);
            propertyBlock.SetTexture(FoamCurrentId, currentState);
            propertyBlock.SetTexture(FoamGuidanceId, guidanceTexture);
            propertyBlock.SetTexture(FoamTopologyId, topologyTexture);
            propertyBlock.SetTexture(
                FoamTopologySourcesId,
                topologySourcesTexture);
            propertyBlock.SetTexture(FoamFractureId, currentFracture);
            propertyBlock.SetTexture(FoamBoundaryId, boundaryTexture);
            propertyBlock.SetTexture(
                FoamObstacleExclusionId,
                obstacleExclusionTexture);
            propertyBlock.SetFloat(FoamInterpolationId, simulationInterpolation);
            propertyBlock.SetFloat(FoamGlobalStartId, allocatedGlobalStart);
            propertyBlock.SetFloat(FoamFieldLengthId, Mathf.Max(0.001f, fieldLength));
            propertyBlock.SetColor(FoamColourId, river.FoamColour);
            propertyBlock.SetFloat(
                FoamStrengthId,
                ProvisionalMaterialStrength);
            propertyBlock.SetFloat(
                FoamCoverageId,
                ProvisionalMaterialCoverage);
            propertyBlock.SetFloat(
                FoamSharpnessId,
                ProvisionalMaterialSharpness);
            propertyBlock.SetFloat(
                FoamDetailScaleId,
                ProvisionalMaterialDetailScale);
            propertyBlock.SetFloat(
                FoamDetailStrengthId,
                ProvisionalMaterialDetailStrength);
            propertyBlock.SetFloat(FoamDebugViewId, (float)river.FoamDebugView);
            propertyBlock.SetFloat(FoamSeedId, river.VisualSeed);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
        }

        private void BindDisabled()
        {
            if (surfaceRenderer == null && river != null)
            {
                surfaceRenderer = river.SurfaceRenderer;
            }

            if (surfaceRenderer == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            surfaceRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(FoamEnabledId, 0f);
            propertyBlock.SetTexture(FoamGuidanceId, Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamTopologyId, Texture2D.blackTexture);
            propertyBlock.SetTexture(
                FoamTopologySourcesId,
                Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamFractureId, Texture2D.blackTexture);
            propertyBlock.SetTexture(FoamBoundaryId, Texture2D.blackTexture);
            propertyBlock.SetTexture(
                FoamObstacleExclusionId,
                Texture2D.blackTexture);
            propertyBlock.SetFloat(FoamInterpolationId, 1f);
            propertyBlock.SetFloat(
                FoamDebugViewId,
                river != null && river.FoamEnabled
                    ? (float)river.FoamDebugView
                    : 0f);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
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
            ReleaseTexture(ref fractureA);
            ReleaseTexture(ref fractureB);
            ReleaseTexture(ref neutralDisturbanceTexture);
            currentFracture = null;
            writeFracture = null;
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
            populationMetricsBuffer?.Release();
            populationMetricsBuffer = null;
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
            resetPopulationKernel = -1;
            measurePopulationKernel = -1;
            updateFractureKernel = -1;
            clearFractureKernel = -1;
            advectForwardKernel = -1;
            advectReverseKernel = -1;
            simulateKernel = -1;
            applyBoundaryKernel = -1;
            fieldWidth = 0;
            fieldHeight = 0;
            guidanceWidth = 0;
            guidanceHeight = 0;
            fractureWidth = 0;
            fractureHeight = 0;
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
            populationAccumulator = 0f;
            fractureAccumulator = 0f;
        }

        private void HandleDomainChanged(RiverDomainSnapshot _)
        {
            resourcesDirty = true;
            boundaryDirty = true;
            if (initializationPhase == InitializationPhase.Failed)
            {
                initializationPhase = InitializationPhase.NotStarted;
            }
        }

        private void HandleGeneratedSourceChanged(IGeneratedGeometrySource _)
        {
            boundaryDirty = true;
        }

        private float ResolveGuidanceUpdateRate()
        {
            if (river == null)
            {
                return 6f;
            }

            return river.Quality switch
            {
                StylizedRiverQuality.Low => 4f,
                StylizedRiverQuality.Medium => 6f,
                StylizedRiverQuality.High => 8f,
                _ => 6f
            };
        }

        private float ResolvePopulationUpdateRate()
        {
            if (river == null)
            {
                return 6f;
            }

            return river.Quality switch
            {
                StylizedRiverQuality.Low => 4f,
                StylizedRiverQuality.Medium => 6f,
                StylizedRiverQuality.High => 8f,
                _ => 6f
            };
        }

        private float ResolveFractureUpdateRate()
        {
            if (river == null)
            {
                return 10f;
            }

            return river.Quality switch
            {
                StylizedRiverQuality.Low => 8f,
                StylizedRiverQuality.Medium => 10f,
                StylizedRiverQuality.High => 12f,
                _ => 10f
            };
        }

        private float ResolveUpdateRate()
        {
            if (river == null)
            {
                return 20f;
            }

            return river.Quality switch
            {
                StylizedRiverQuality.Low => 12f,
                StylizedRiverQuality.Medium => 20f,
                StylizedRiverQuality.High => 30f,
                _ => 20f
            };
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

        private int GlobalDistanceToChunk(float globalDistance)
        {
            float local = globalDistance - river.Domain.GlobalDistanceMinimum;
            return Mathf.Clamp(
                Mathf.FloorToInt(local / ChunkLengthMetres),
                0,
                Mathf.Max(0, chunkCount - 1));
        }

        private int CountActiveChunks()
        {
            int count = 0;
            for (int index = 0; index < chunkActive.Length; index++)
            {
                if (chunkActive[index])
                {
                    count++;
                }
            }

            return count;
        }

        private void ResetLastUpdateDiagnostics()
        {
            lastUpdateDispatches = 0;
            lastUpdateCellIterations = 0;
            injectedLastUpdate = 0;
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
