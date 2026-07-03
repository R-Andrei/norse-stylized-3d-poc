# CS_RiverFoam Compute Refactor Baseline

Date: 2026-07-03

Scope: Item 3, Pass 1 of the monolith refactor checklist. This is a contract map only; no compute code has been moved yet.

## Source Asset

- Compute asset: `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`
- Current line count recorded in checklist: 3467 lines.
- Current include order:
  - Kernel pragmas first.
  - `#pragma target 5.0`.
  - `#include "../Shaders/Includes/RiverWaterCommon.hlsl"`.
  - Local structs, resources, uniforms, helpers, then kernels.

Keep this order stable in pass 2 until each include group has been moved and revalidated.

## Kernel Manifest

The C# runtime resolves these exact names with `ComputeShader.FindKernel` in `StylizedRiverFoamRuntime.Compute.cs`.

| Kernel | Current line | Threads | Dispatch shape |
| --- | ---: | --- | --- |
| `ClearRange` | 1304 | `8, 8, 1` | 2D range clear |
| `InjectFoam` | 1357 | `8, 8, 1` | 2D injection rect |
| `BuildGuidance` | 2771 | `8, 8, 1` | 2D guidance texture |
| `BuildCurrentShoreEdges` | 1473 | `64, 1, 1` | 1D chunk/row edge build |
| `ComposeTopology` | 2488 | `8, 8, 1` | 2D topology write |
| `CaptureGeneratedTopology` | 2457 | `8, 8, 1` | 2D topology capture |
| `BuildEvolvingMajorSupport` | 2100 | `8, 8, 1` | 2D evolving support |
| `ClearObstacleExclusion` | 1609 | `8, 8, 1` | 2D obstacle texture clear |
| `UpdateObstacleExclusion` | 1621 | `64, 1, 1` | 1D obstacle interval cells |
| `ResetTopologyMetrics` | 2676 | `64, 1, 1` | 1D metrics reset |
| `MeasureTopologyMetrics` | 2687 | `8, 8, 1` | 2D metrics accumulation |
| `ResetPopulation` | 2817 | `64, 1, 1` | 1D population reset |
| `MeasurePopulation` | 2834 | `8, 8, 1` | 2D population accumulation |
| `UpdateFracture` | 2923 | `8, 8, 1` | 2D fracture update |
| `ClearFractureRange` | 2905 | `8, 8, 1` | 2D fracture clear |
| `AdvectForward` | 3038 | `8, 8, 1` | 2D foam advection |
| `AdvectReverse` | 3083 | `8, 8, 1` | 2D reverse advection |
| `SimulateFoam` | 3128 | `8, 8, 1` | 2D main foam simulation |
| `ApplyBoundary` | 1322 | `8, 8, 1` | 2D boundary clamp |

Refactor rule: do not rename kernel functions, pragma names, thread group sizes, or dispatch dimensions during include-split passes.

## Struct Layouts

Local compute structs that must keep field order and scalar/vector packing:

- `FoamMetricRow`
  - `float4 widthsAndSpacing`
  - `float4 topologyData`
  - `float4 shoreData`
- `FoamObstacleSample`
  - `float4 intervals`
  - `float4 waterParameters`
- `FoamObstacleIntervalCell`
  - `float4 coordinateAndOffset`
- `FoamMajorEvolutionData`
  - `float4 centreAndPlacement`
  - `float4 candidateShape`
  - `float4 candidateExtents`
  - `float4 morph`
  - `float4 warp`
- `FoamHostedNegativeEvolutionData`
  - `float4 hostAndMask`
  - `float4 centreAndOffset`
  - `float4 morph`
- `FoamFreeWaterEvolutionData`
  - `float4 centreAndPlacement`
  - `float4 maskAndStrength`
  - `float4 morph`
- `FoamConnectorIdentityData`
  - `float4 pointRangeAndRadii`
- `FoamWeakSpanIdentityData`
  - `float4 connectorAndPath`
  - `float4 shape`
  - `uint4 noiseAndFlags`
- `FoamMotionSample`
  - Local helper struct used by motion sampling.

Refactor rule: struct declarations are good candidates for the first include, but the C# buffer upload structs must be checked before and after the move.

## Resource Declarations

Texture resources declared by the compute file:

- State/advection: `_FoamStateWrite`, `_FoamStateRead`, `_FoamAdvectionWrite`, `_FoamAdvectedRead`, `_FoamReverseRead`
- Boundary/guidance: `_FoamBoundary`, `_FoamGuidanceWrite`, `_FoamGuidanceRead`
- Obstacle exclusion: `_FoamObstacleExclusionWrite`, `_FoamObstacleExclusionRead`
- Topology: `_FoamTopologyGeneratedRead`, `_FoamTopologyTransitionFromRead`, `_FoamTopologyWrite`, `_FoamTopologySourcesWrite`, `_FoamTopologyTransitionCaptureWrite`, `_FoamCurrentShoreEdgesWrite`, `_FoamCurrentShoreEdgesRead`, `_FoamTopologyRead`, `_FoamTopologySourcesRead`
- Evolution masks/state: `_FoamEvolvingMajorRead`, `_FoamEvolvingMajorWrite`, `_FoamEvolvingHostedNegativeRead`, `_FoamEvolvingHostedNegativeWrite`, `_FoamEvolvingFreeWaterNegativeRead`, `_FoamEvolvingFreeWaterNegativeWrite`, `_FoamEvolvingConnectorRead`, `_FoamEvolvingConnectorWrite`, `_FoamEvolvingWeakSpanNegativeRead`, `_FoamEvolvingWeakSpanNegativeWrite`, `_FoamMajorMasks`, `_FoamHostedNegativeMasks`, `_FoamFreeWaterNegativeMasks`
- Disturbance inputs: `_FoamWakeField`, `_FoamRippleField`, `_FoamStaticWakeField`, `_FoamStaticPressureField`
- Fracture: `_FoamFractureWrite`, `_FoamFractureRead`

Buffer resources declared by the compute file:

- `_FoamMetricRows`
- `_FoamTopologyTransitionMetricRows`
- `_FoamObstacleSamples`
- `_FoamObstacleCells`
- `_FoamMajorEvolutionRecords`
- `_FoamHostedNegativeEvolutionRecords`
- `_FoamFreeWaterEvolutionRecords`
- `_FoamConnectorIdentityRecords`
- `_FoamConnectorPathPoints`
- `_FoamWeakSpanIdentityRecords`
- `_FoamPopulationMetrics`
- `_FoamTopologyMetrics`

Refactor rule: C# currently binds optional disturbance textures to neutral render textures when missing. Do not move or rename those declarations without keeping the all-kernel binding assumption intact.

## Uniform Groups

Uniforms are currently global declarations, not `cbuffer` grouped.

- Dimensions and ranges: `_FoamDimensions`, `_FoamGuidanceDimensions`, `_FoamTopologyDimensions`, `_FoamTopologyTransitionDimensions`, `_FoamMajorMaskDimensions`, `_FoamHostedNegativeMaskDimensions`, `_FoamFreeWaterMaskDimensions`, `_FoamWakeDimensions`, `_FoamRippleDimensions`, `_FoamStaticWakeDimensions`, `_FoamStaticPressureDimensions`, `_FoamFractureDimensions`, `_FoamRangeStart`, `_FoamRangeCount`, `_FoamFractureRangeStart`, `_FoamFractureRangeCount`
- Counts: `_FoamObstacleCellCount`, `_FoamMajorEvolutionCount`, `_FoamHostedNegativeEvolutionCount`, `_FoamFreeWaterEvolutionCount`, `_FoamConnectorIdentityCount`, `_FoamWeakSpanIdentityCount`, `_FoamResolutionPerChunk`, `_FoamChunkCount`
- Simulation state/material: `_FoamDeltaTime`, `_FoamGlobalStart`, `_FoamFieldLength`, `_FoamValidLength`, `_FoamSimulationLength`, `_FoamFlowSpeed`, `_FoamFlowDirection`, `_FoamEvolution`, `_FoamBreakup`, `_FoamSpread`, `_FoamCohesion`, `_FoamConnectivity`, `_FoamAmountDecay`, `_FoamFreshnessDecay`, `_FoamIntegrityDamage`, `_FoamShoreRetention`, `_FoamTime`, `_FoamSeed`, `_FoamTargetCoverage`, `_FoamSupplyRate`, `_FoamVisibleThreshold`, `_FoamGuidanceStrength`, `_FoamBoundaryAttraction`, `_FoamWakeReinforcement`, `_FoamImpactReinforcement`, `_FoamDisturbanceEnabled`
- Fracture and motion: `_FoamFractureDeltaTime`, `_FoamMotionFlowSpeed`, `_FoamMotionWaveHeight`, `_FoamMotionWaveLength`, `_FoamMotionWaveSteepness`, `_FoamMotionTurbulence`
- Shore/topology controls: `_FoamShoreMotion`, `_FoamShoreMotionWidth`, `_FoamShoreWaveHeightScale`, `_FoamShoreWaveLengthScale`, `_FoamShoreWaveReach`, `_FoamShoreWaveTransitionLength`, `_FoamShoreWaveSizeVariation`, `_FoamShoreWaveSideAsymmetry`, `_FoamShoreWaveProfileVariation`, `_FoamShoreBankCover`, `_FoamFreezeAmount`, `_FoamShoreCaptureCoreWidth`, `_FoamShoreCaptureFadeWidth`
- Evolution/transition toggles: `_FoamMajorEvolutionEnabled`, `_FoamHostedNegativeEvolutionEnabled`, `_FoamFreeWaterNegativeEvolutionEnabled`, `_FoamConnectorIdentityReconstructionEnabled`, `_FoamWeakSpanIdentityReconstructionEnabled`, `_FoamTopologyTransitionEnabled`, `_FoamTopologyTransitionBlend`, `_FoamTopologyTransitionSameMapping`, `_FoamTopologyTransitionGlobalStart`, `_FoamTopologyTransitionFieldLength`, `_FoamTopologyTransitionValidLength`
- Injection: `_FoamInjectionGlobalDistance`, `_FoamInjectionAcrossNormalized`, `_FoamInjectionRadius`, `_FoamInjectionAmount`, `_FoamInjectionFreshness`, `_FoamInjectionIntegrity`, `_FoamInjectionPhase`, `_FoamInjectionElongation`, `_FoamInjectionShapeSeed`, `_FoamInjectionShapeVariety`, `_FoamInjectionCompound`

## Helper Group Map

Use this order when creating includes. Later helpers depend on earlier helpers.

1. Coordinate/domain helpers: `FoamTexelCentreUV1D`, `FoamTexelCentreUV`, `FoamLocalDistanceAtTexel`, `FoamAcross01AtTexel`, `FoamUVToTexelCoordinate`, `FoamUVToContainingTexel`, `ResolveFoamCellBilinearCoordinates`, `IsFoamColumnInsideDomain`, `IsFoamUInsideDomain`, `IsFoamColumnInsideSimulation`, `ClampX`, `ClampY`.
2. Texture load helpers: `LoadState`, `LoadBoundaryClasses`, `LoadBoundary`, `LoadObstacleExclusionCell`.
3. Pressure/support and metric helpers: `ResolvePressureSupportEnvelope`, `FoamAcross01ToMetres`, `FoamMetresToAcross01`.
4. Bilinear sampling helpers: `SampleStateBilinear`, `LoadAdvected`, `LoadReverse`, `SampleAdvectedBilinear`, `SampleBoundaryClassesBilinear`, `SampleBoundaryBilinear`, `SampleGuidanceBilinear`, `SampleStateAtUV`, `LoadFracture`, `SampleFractureBilinear`, `ResolveExternalBilinearCoordinates`, `SampleWakeBilinear`, `SampleRippleBilinear`, `SampleStaticWakeBilinear`, `SampleStaticPressureBilinear`.
5. Noise/phase/shape helpers: `FoamHash11`, `FoamHash22`, `PhaseDistance`, `MixPhaseShortest`, `FoamEllipseMask`, `EvaluateCompoundInjectionShape`, `FoamValueNoise`, `FoamFbm`, `VoronoiEdgeDistance`.
6. Network and motion helpers: `EvaluateNetworkSample`, `EvaluateNetworkDistance`, `ResolveMotion`, `ResolveOriginalBounds`.
7. Topology/evolution helpers: `FoamCombinedAnchoredSupport`, `FoamCombinedNegativeInfluence`, `FoamComposeLegacyNetSupport`, obstacle interval helpers, mask samplers, major candidate positioning, smoothstep/geometry helpers, identity hash/noise helpers, connector/weak-span identity helpers, generated topology transition helpers.
8. Kernels remain in the main `.compute` file until pass 3 or pass 4 says otherwise.

## C# Binding Assumptions

Kernel names are exact string contracts in `StylizedRiverFoamRuntime.Compute.cs`. The runtime also uses exact compute resource names across the foam runtime partials. Key assumptions:

- `ResolveComputeKernels` must find all 19 kernels listed above.
- `ConfigureSharedComputeParameters` binds dimensions, ranges, material controls, disturbance textures, and motion-kernel resources.
- `BindMotionKernel` binds `_FoamMetricRows`, `_FoamBoundary`, `_FoamGuidanceRead`, `_FoamWakeField`, `_FoamRippleField`, `_FoamStaticWakeField`, and `_FoamStaticPressureField` for `AdvectForward`, `AdvectReverse`, `SimulateFoam`, and `UpdateFracture`.
- Topology paths bind generated topology, transition, metrics, shore edges, obstacle exclusion, mask arrays, and evolution records by exact `_Foam...` names.
- Injection binds `_FoamMetricRows`, `_FoamBoundary`, `_FoamStateWrite`, and all `_FoamInjection...` uniforms.
- Material property IDs such as `_FoamEnabled`, `_FoamPrevious`, `_FoamCurrent`, `_FoamTopology`, and `_FoamDebugView` are shader/material bindings, not compute declarations; keep them out of compute include contracts unless the shader include also uses them.

## Pass 2 Include Split Recommendation

Recommended first include files, in order:

1. `CS_RiverFoam.Structs.hlsl` for struct declarations.
2. `CS_RiverFoam.Resources.hlsl` for texture, buffer, and uniform declarations.
3. `CS_RiverFoam.Coordinates.hlsl` for coordinate/domain helpers.
4. `CS_RiverFoam.Sampling.hlsl` for load and bilinear sampling helpers.
5. `CS_RiverFoam.Noise.hlsl` for hash/noise/phase/shape helpers.

Keep `#pragma kernel` lines in `CS_RiverFoam.compute`, followed by `#pragma target 5.0`, then `RiverWaterCommon.hlsl`, then these new includes in dependency order.

## Validation Checklist For Pass 2

- Kernel pragma list unchanged.
- `FindKernel` names still match exactly.
- Struct field order unchanged.
- Resource/uniform names unchanged.
- Include order keeps `RiverWaterCommon.hlsl` before helpers that depend on it.
- `git diff --check` clean.
- Unity compute import/compile checked after moving includes.
