# Codebase Monolith Refactor Checklist

## Document Status

**Status:** Initial scan and ranked refactor-opportunity log.

**Created for:** A future behaviour-preserving refactor pass focused on splitting oversized source, shader, and compute modules into smaller maintainable chunks.

**Current scope:** Project-owned files under `Assets`. Package cache, generated Unity package code, and third-party code were not treated as refactor targets.

**Important constraint:** This is not a performance pass. Any possible performance benefit should be treated as incidental and should not drive the design. The primary goal is to reduce monolithic file size, review conflict, navigation cost, and change risk.

## Scan Notes

This scan used the current working tree on 2 July 2026. The repository already had unrelated modified and untracked files before this document was created. The line counts below are physical line counts from the current workspace, including blank lines.

The scan looked at:

- `*.cs`
- `*.compute`
- `*.hlsl`
- `*.shader`
- `*.cginc`

The scan considered:

- total file size;
- very long methods and shader functions;
- Unity serialization risk;
- public API surface;
- compute kernel lookup and shader property contracts;
- editor tooling dependencies;
- likely review and merge-conflict pressure;
- whether a split can be done mechanically before deeper extraction.

No implementation changes are proposed here as final. Each item below is a candidate checklist entry to research, approve, and refactor one at a time.

## Current Largest Project-Owned Files

| Rank | File | Lines | Notes |
|---:|---|---:|---|
| 1 | `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.cs` | 12903 | Largest runtime monolith; many internal phases and GPU bindings. |
| 2 | `Assets/Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.cs` | 7706 | Large runtime monolith; public/static diagnostics and generated-source handling. |
| 3 | `Assets/Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamPocketTopologyGenerator.cs` | 4282 | Static algorithm monolith. |
| 4 | `Assets/Game/Procedural/Rivers/StylizedRiver.cs` | 3945 | Main serialized river component with broad public property surface. |
| 5 | `Assets/Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamConnectorTopologyGenerator.cs` | 3519 | Static algorithm monolith. |
| 6 | `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute` | 3467 | Large compute asset with 19 kernels. |
| 7 | `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs` | 3396 | Large custom inspector with two very large draw methods. |
| 8 | `Assets/Game/Procedural/Masses/MassGenerator.cs` | 2241 | Non-river procedural generator monolith. |
| 9 | `Assets/Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyCacheCodec.cs` | 1907 | Cache serialization/deserialization monolith. |
| 10 | `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverDisturbance.compute` | 1836 | Compute asset with 11 kernels and large helper functions. |
| 11 | `Assets/Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamMajorTopologyGenerator.cs` | 1694 | Static algorithm monolith. |
| 12 | `Assets/Game/Procedural/Rivers/RiverDisturbanceFootprintResolver.cs` | 1579 | Disturbance footprint and pressure-support resolver. |
| 13 | `Assets/Game/Procedural/Rivers/StylizedRiverCorridorGeometry.cs` | 1269 | Medium-large geometry utility. |
| 14 | `Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader` | 1187 | Shader pass contains a large fragment routine. |
| 15 | `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs` | 1176 | Medium-large editor file. |
| 16 | `Assets/Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamMajorCandidateGenerator.cs` | 1111 | Static algorithm utility. |

## Cross-Cutting Refactor Rules

- Prefer mechanical splits before logic changes.
- Do not rename serialized fields during the first pass.
- Do not change MonoBehaviour or ScriptableObject type identity during the first pass.
- Do not change shader property names, compute kernel names, buffer layouts, or resource paths.
- Keep public APIs intact unless a later approved migration explicitly changes them.
- Keep `.meta` files stable when moving or creating Unity assets.
- Compile and manually validate after each small slice.
- Treat every large method extraction as higher risk than moving already-separable methods into partial files.
- Document every completed split in this file as the log of record.

## Recommended Pass Counts

These counts are intentionally conservative. A pass should be large enough to produce a useful structural improvement, but small enough to compile and validate before moving on.

| Item | Opportunity | Recommended Passes | Reason |
|---:|---|---:|---|
| 1 | `StylizedRiverFoamRuntime.cs` | 5 | Largest runtime file; needs separate mechanical split, compute/resource split, topology/evolution split, long-method reduction, and final cleanup. |
| 2 | `StylizedRiverDisturbanceRuntime.cs` | 4 | Runtime file with public diagnostics and generated-source paths; split contracts/resources first, then sources/dispatch, then long methods. |
| 3 | `CS_RiverFoam.compute` | 4 | Compute kernel manifest must stay stable; includes and long helper reductions should be staged carefully. |
| 4 | `StylizedRiverEditor.cs` | 3 | Editor-only work is lower runtime risk, but the two huge draw methods deserve a separate extraction pass. |
| 5 | `StylizedRiver.cs` | 4 | Serialized main component; split methods while keeping fields stable, then reduce validation/material binding. |
| 6 | Foam topology generator files | 4 | Multiple deterministic generators; split one topology family at a time and validate deterministic output. |
| 7 | `StylizedRiverFoamTopologyCacheCodec.cs` | 3 | Binary/cache compatibility risk; separate read/write/validate, then round-trip cleanup. |
| 8 | Disturbance compute and footprint resolver package | 4 | Mixed CPU/GPU contract; split compute includes and C# resolver separately. |
| 9 | River water shader pass and shared HLSL includes | 3 | Existing include structure helps; main risk is visual parity and include order. |
| 10 | `MassGenerator.cs` | 2 | Existing region markers make this a simpler mechanical split followed by optional cleanup. |

## Opportunity 1: `StylizedRiverFoamRuntime.cs`

**Priority:** Highest.

**Current size:** 12903 lines.

**Recommended pass count:** 5.

**Pass checklist:**

- [x] Pass 1 - Baseline and mechanical shell split: create partial files for constants, state records, lifecycle/orchestration, and public/runtime entry points without moving logic across responsibility boundaries yet.
- [x] Pass 2 - Resource and compute ownership split: move allocation/release, kernel lookup, shared bindings, dispatch helpers, and material binding into focused partial files.
- [x] Pass 3 - Topology and obstacle split: move topology build/replacement, cache validation, generated topology upload, and obstacle exclusion code into their own partials.
- [x] Pass 4 - Evolution and injection split: move manual injection/reservation code and Major/Hosted/FreeWater/Connector evolution groups into focused partials.
- [x] Pass 5 - Long-method reduction and cleanup: extract helpers from the largest methods, tighten file ownership, update this log, and validate the final structure.

**Observed shape:**

- Single hidden `MonoBehaviour` runtime owns a very broad Stage 6 Foam surface area.
- No `[SerializeField]` fields were detected in this file, which makes mechanical partial splitting less risky than splitting the main serialized river component.
- Large number of methods detected: 232 by the scan heuristic.
- Several very large methods:
  - `UpdateConnectorEvolutionDescriptors` - about 504 lines.
  - `AdvanceInitializationPhase` - about 366 lines.
  - `InitializeConnectorIdentityReconstruction` - about 326 lines.
  - `BuildEvolvingMajorField` - about 291 lines.
  - `LateUpdate` - about 277 lines.
  - `ConfigureSharedComputeParameters` - about 233 lines.
- Owns compute resource path, kernel resolution, resource creation/release, staged initialization, topology building, topology replacement, obstacle exclusion, injection, evolution, diagnostics, and material bindings.
- Referenced by:
  - `StylizedRiver`
  - `StylizedRiverEditor`
  - `StylizedRiverFoamBuildPreflight`
  - `StylizedRiverFoamDevelopmentCacheCoordinator`
  - related topology generator classes.

**Refactor goal:**

Split the code into multiple smaller partial files while preserving the exact component type and public API. Do not attempt a performance rewrite.

**Suggested first-pass split:**

- `StylizedRiverFoamRuntime.cs`
  - Keep the type declaration, Unity attributes, public surface, high-level lifecycle, and obvious orchestration entry points.
- `StylizedRiverFoamRuntime.Constants.cs`
  - Constants, shader/material property IDs, compute resource path, and internal enum declarations if they are not tightly coupled to a specific phase.
- `StylizedRiverFoamRuntime.State.cs`
  - Internal structs/classes used as runtime state records.
  - Keep CPU/GPU layout structs very close to their existing definitions and add comments warning not to alter layout casually.
- `StylizedRiverFoamRuntime.Initialization.cs`
  - Initialization phase machine and readiness transitions.
- `StylizedRiverFoamRuntime.Resources.cs`
  - Texture/buffer allocation, release, fallback textures, resource-completeness checks.
- `StylizedRiverFoamRuntime.Compute.cs`
  - Kernel lookup, shared parameter binding, dispatch helpers, field binding.
- `StylizedRiverFoamRuntime.Topology.cs`
  - Topology generation orchestration, active/requested topology signatures, generated topology upload.
- `StylizedRiverFoamRuntime.TopologyReplacement.cs`
  - Replacement build state, transition capture, activation, retirement of old topology resources.
- `StylizedRiverFoamRuntime.Obstacles.cs`
  - Obstacle exclusion cache and mask update logic.
- `StylizedRiverFoamRuntime.Injection.cs`
  - Manual/pending injections, reservations, activation ranges.
- `StylizedRiverFoamRuntime.Evolution.cs`
  - Shared evolution update entry points.
- `StylizedRiverFoamRuntime.EvolutionMajor.cs`
  - Major Support evolution.
- `StylizedRiverFoamRuntime.EvolutionHostedNegative.cs`
  - Hosted negative evolution.
- `StylizedRiverFoamRuntime.EvolutionFreeWater.cs`
  - Free-water negative evolution.
- `StylizedRiverFoamRuntime.EvolutionConnector.cs`
  - Connector and Weak Span identity/evolution logic.
- `StylizedRiverFoamRuntime.Diagnostics.cs`
  - Population metrics, topology metrics, profiler counters, debug/material reporting.

**Suggested second-pass extraction:**

Only after the partial split compiles and validates, inspect long methods for helper extraction. Start with helper extraction where inputs and outputs are obvious:

- Break `AdvanceInitializationPhase` into one private method per phase.
- Break `ConfigureSharedComputeParameters` into grouped binding helpers.
- Break `UpdateConnectorEvolutionDescriptors` into descriptor selection, slot update, relationship selection, and buffer upload helpers.
- Break `BuildEvolvingMajorField` into preparation, binding, dispatch, and metrics/update-result helpers.

**Do not do in the first pass:**

- Do not move this into separate components.
- Do not rename kernels or compute properties.
- Do not alter topology scheduling or staged initialization.
- Do not split the compute asset at the same time.
- Do not alter CPU/GPU struct layout.

**Validation gate:**

- Unity compile succeeds.
- Existing river scenes load.
- A river with Foam enabled initializes to ready.
- Foam material bindings still show current/previous/guidance/topology textures.
- Topology cache assignment, preflight, and development cache coordinator still find the runtime.
- All compute kernels still resolve by their existing names.
- Enable/disable and domain-change resource release paths still work.

**Checklist:**

- [x] Baseline behaviour captured.
- [x] Public surface recorded.
- [x] Partial-file split plan approved.
- [x] Mechanical partial split completed.
- [x] Compile validated.
- [x] Runtime Foam initialization validated.
- [x] Long-method extraction plan approved.
- [x] Long-method extraction completed.
- [x] Documentation updated.

## Opportunity 2: `StylizedRiverDisturbanceRuntime.cs`

**Priority:** Very high.

**Current size:** 7706 lines.

**Recommended pass count:** 4.

**Pass checklist:**

- [x] Pass 1 - Baseline and contract split: isolate public structs, diagnostics contracts, constants, property IDs, and internal state records while preserving serialized field names.
- [x] Pass 2 - Resource and compute split: move compute loading, kernel lookup, texture/buffer allocation, release, shared binding, and dispatch helpers into focused partial files.
- [x] Pass 3 - Source-path split: move generated-source, static-source, continuous-source, impact, pressure, wake, and boundary responsibilities into separate partials.
- [x] Pass 4 - Long-method reduction and cleanup: reduce the largest methods, preserve static diagnostics, update this log, and validate emitter/generated-source workflows.

**Observed shape:**

- Hidden `MonoBehaviour` runtime with public/static diagnostic entry points.
- Contains one serialized public struct area through `ImpactRippleEventSettings`, with 7 `[SerializeField]` fields detected.
- Method count detected: 142.
- Several very large methods:
  - `CreateLegacy` - about 547 lines.
  - `TryGetGeneratedSourcePressureProfileDebugData` - about 414 lines.
  - `ProcessGeneratedGeometrySource` - about 299 lines.
  - `DispatchStaticBakeCommon` - about 296 lines.
  - `RegisterStaticSource` - about 245 lines.
  - `BuildRippleMetricData` - about 225 lines.
  - `EnsureResources` - about 195 lines.
- Referenced by:
  - `StylizedRiver`
  - `StylizedRiverFoamRuntime`
  - `StylizedRiverDisturbanceEmitter`
  - `StylizedRiverEditor`
  - `GeneratedMassEditor`
  - generated geometry source diagnostics.

**Refactor goal:**

Split into partials by runtime responsibility while preserving the runtime component identity and static diagnostics.

**Suggested first-pass split:**

- `StylizedRiverDisturbanceRuntime.cs`
  - Keep Unity attributes, type declaration, public entry points, and lifecycle skeleton.
- `StylizedRiverDisturbanceRuntime.Contracts.cs`
  - Public structs, diagnostics DTOs, enums, constants, shader property IDs.
  - Keep `ImpactRippleEventSettings` serialization stable.
- `StylizedRiverDisturbanceRuntime.State.cs`
  - Internal source, reservation, metric, wake variation, and upload state structs.
- `StylizedRiverDisturbanceRuntime.Resources.cs`
  - Compute load, texture/buffer allocation, texture/buffer release, resource failure reporting.
- `StylizedRiverDisturbanceRuntime.GeneratedSources.cs`
  - Generated geometry registration, refresh, source processing, ownership conflict handling.
- `StylizedRiverDisturbanceRuntime.StaticSources.cs`
  - Static source registration/update paths.
- `StylizedRiverDisturbanceRuntime.StaticPressure.cs`
  - Pressure profiles, pressure bake profile generation, static pressure dispatch.
- `StylizedRiverDisturbanceRuntime.StaticWake.cs`
  - Static wake variation/profile generation and dispatch.
- `StylizedRiverDisturbanceRuntime.Impacts.cs`
  - Impact command emission, reservations, ripple injection.
- `StylizedRiverDisturbanceRuntime.ContinuousSources.cs`
  - Moving/continuous source update and wake injection.
- `StylizedRiverDisturbanceRuntime.Boundary.cs`
  - Ripple metric rows, ripple boundary baking, obstacle boundary interaction.
- `StylizedRiverDisturbanceRuntime.Compute.cs`
  - Kernel lookup, shared parameter binding, dispatch helpers.
- `StylizedRiverDisturbanceRuntime.Diagnostics.cs`
  - Public static diagnostics and editor-facing debug data.

**Suggested second-pass extraction:**

- Extract the legacy/default `ImpactRippleEventSettings` factory logic only after confirming all serialized defaults and editor paths.
- Split `ProcessGeneratedGeometrySource` into bounds filtering, footprint resolution, static pressure setup, static wake setup, and diagnostic registration.
- Split `DispatchStaticBakeCommon` into binding and dispatch helpers without changing dispatch order.
- Split `TryGetGeneratedSourcePressureProfileDebugData` into data lookup and formatting/copy helpers.

**Do not do in the first pass:**

- Do not change generated-source ownership semantics.
- Do not change static diagnostic dictionaries.
- Do not rename `ImpactRippleEventSettings` fields.
- Do not change compute kernel names or dispatch sequence.
- Do not alter emitter resolution or runtime lookup.

**Validation gate:**

- Unity compile succeeds.
- `StylizedRiverDisturbanceEmitter` still resolves and updates a runtime.
- Generated mass diagnostics still display.
- Static/generated sources still register and unregister.
- Impact ripple test controls still emit.
- Static pressure and wake textures still bind into the river material.
- Disturbance compute kernels still resolve by existing names.

**Checklist:**

- [ ] Baseline behaviour captured.
- [ ] Public/static diagnostic APIs recorded.
- [ ] Serialized struct fields recorded.
- [ ] Mechanical partial split completed.
- [ ] Compile validated.
- [ ] Emitter workflow validated.
- [ ] Generated-source workflow validated.
- [ ] Long-method extraction completed.
- [ ] Documentation updated.

## Opportunity 3: `CS_RiverFoam.compute`

**Priority:** Very high.

**Current size:** 3467 lines.

**Recommended pass count:** 4.

**Pass checklist:**

- [x] Pass 1 - Baseline and include-order map: record all kernels, resource declarations, struct layouts, and C# binding assumptions before moving code.
- [x] Pass 2 - Struct/resource/sampling include split: move low-level declarations and pure sampling helpers into includes while keeping the main compute file as the kernel manifest.
- [x] Pass 3 - Topology/evolution/simulation include split: move domain-specific helper groups into includes without changing kernel entry points.
- [x] Pass 4 - Large function reduction and cleanup: reduce the biggest helper/kernel bodies where safe, revalidate all kernel lookup, and update this log.

**Observed shape:**

- One compute asset with 19 kernels:
  - `ClearRange`
  - `InjectFoam`
  - `BuildGuidance`
  - `BuildCurrentShoreEdges`
  - `ComposeTopology`
  - `CaptureGeneratedTopology`
  - `BuildEvolvingMajorSupport`
  - `ClearObstacleExclusion`
  - `UpdateObstacleExclusion`
  - `ResetTopologyMetrics`
  - `MeasureTopologyMetrics`
  - `ResetPopulation`
  - `MeasurePopulation`
  - `UpdateFracture`
  - `ClearFractureRange`
  - `AdvectForward`
  - `AdvectReverse`
  - `SimulateFoam`
  - `ApplyBoundary`
- Large functions:
  - `SimulateFoam` - about 340 lines.
  - `BuildEvolvingMajorSupport` - about 210 lines.
  - `ComposeTopology` - about 188 lines.
  - `EvaluateNetworkDistance` - about 184 lines.
  - `ResolvePressureSupportEnvelope` - about 124 lines.
  - `UpdateFracture` - about 115 lines.
- C# runtime resolves kernels by exact string names in `StylizedRiverFoamRuntime`.

**Refactor goal:**

Split helper code into include files while keeping the main compute asset as the kernel manifest and preserving all kernel names, resource names, and buffer layouts.

**Suggested first-pass split:**

- `CS_RiverFoam.compute`
  - Keep `#pragma kernel` declarations.
  - Keep `#pragma target`.
  - Keep all kernel entry point functions.
  - Keep include order explicit.
- `Includes/RiverFoamComputeStructs.hlsl`
  - Foam metric rows, obstacle samples, evolution data structs, connector identity data.
- `Includes/RiverFoamComputeResources.hlsl`
  - Texture, buffer, and scalar parameter declarations.
  - This is riskier than helper extraction because declarations are used everywhere; do only if include order stays simple.
- `Includes/RiverFoamComputeSampling.hlsl`
  - Texel coordinate conversion, load/sample helpers, bilinear helpers.
- `Includes/RiverFoamComputeTopology.hlsl`
  - Topology sampling/composition helpers, generated topology transition helpers.
- `Includes/RiverFoamComputeEvolution.hlsl`
  - Major, hosted negative, free-water, connector, and weak-span helper functions.
- `Includes/RiverFoamComputeSimulation.hlsl`
  - Advection, fracture, injection, boundary application, simulation helpers.
- `Includes/RiverFoamComputeMetrics.hlsl`
  - Population and topology metrics helpers.

**Suggested second-pass extraction:**

- Reduce `SimulateFoam` by extracting named local helper functions that preserve order.
- Reduce `ComposeTopology` by separating source sampling, generated topology blending, live-source composition, and output packing.
- Reduce `BuildEvolvingMajorSupport` by separating record decoding, shape evaluation, and writeback.

**Do not do in the first pass:**

- Do not split this into multiple compute assets.
- Do not rename kernels.
- Do not rename textures, buffers, or scalar parameters.
- Do not alter struct field ordering.
- Do not change thread group size or dispatch dimensions.

**Validation gate:**

- Unity imports the compute asset with no errors.
- `StylizedRiverFoamRuntime` resolves all 19 kernels.
- Foam initialization and simulation still run.
- Debug views that depend on topology, guidance, boundary, population, and fracture still display.
- No C# binding changes required beyond possible include path changes.

**Checklist:**

- [ ] Include split map approved.
- [ ] Struct/resource declarations audited against C# layouts.
- [ ] First include extraction completed.
- [ ] Compute asset imports successfully.
- [ ] Kernel lookup validated.
- [ ] Runtime Foam workflow validated.
- [ ] Large kernel/helper functions reduced.
- [ ] Documentation updated.

## Opportunity 4: `StylizedRiverEditor.cs`

**Priority:** High.

**Current size:** 3396 lines.

**Recommended pass count:** 3.

**Status:** Deferred as of 2026-07-03. Re-evaluate this opportunity before starting any pass; the checklist below is retained as a planning snapshot, not an active implementation queue.

**Pass checklist:**

- [ ] Pass 1 - Mechanical editor partial split: split the custom inspector by visible Inspector section and shared editor helpers without changing serialized property names.
- [ ] Pass 2 - Large draw-method reduction: break `DrawFoam` and `DrawRuntimeDisturbances` into smaller section helpers.
- [ ] Pass 3 - Editor validation and cleanup: verify Inspector workflows, multi-object editing, Undo, cache creation, preview refresh, and update this log.

**Observed shape:**

- Custom editor class for `StylizedRiver`.
- Contains very large UI methods:
  - `DrawFoam` - about 1239 lines.
  - `DrawRuntimeDisturbances` - about 865 lines.
  - `DrawSurfaceMotion` - about 107 lines.
  - `DrawWaterBody` - about 99 lines.
  - `DrawRefraction` - about 98 lines.
- Uses `SerializedProperty`, `EditorGUILayout`, `EditorGUI`, `Undo`, asset creation, preview textures, and runtime lookup.
- Already references newer editor helpers such as cache/preflight classes.

**Refactor goal:**

Split the custom inspector into partial files by visible Inspector section and helper category. This should be mostly mechanical and should not change editor behaviour.

**Suggested first-pass split:**

- `StylizedRiverEditor.cs`
  - Keep type declaration, `OnInspectorGUI`, `OnDisable`, shared `Find`, target iteration, and menu item.
- `StylizedRiverEditor.Setup.cs`
  - Setup, domain, channel, natural variation, advanced shoreline.
- `StylizedRiverEditor.Motion.cs`
  - Surface mesh, surface motion, refraction sections.
- `StylizedRiverEditor.Disturbances.cs`
  - Runtime disturbances UI, debug controls, impact test controls, memory diagnostics.
- `StylizedRiverEditor.Foam.cs`
  - Foam UI, topology cache controls, foam test controls.
- `StylizedRiverEditor.Body.cs`
  - Water body, liquid/frozen controls, advanced body.
- `StylizedRiverEditor.Status.cs`
  - Deferred stage status, runtime status, diagnostics descriptions.
- `StylizedRiverEditor.Actions.cs`
  - Buttons, apply-to-targets helper, create menu, scene repaint.
- `StylizedRiverEditor.Preview.cs`
  - Preview textures and refresh helpers.

**Suggested second-pass extraction:**

- Break `DrawFoam` into discrete draw helpers for:
  - enable/cache controls;
  - topology generation controls;
  - topology diagnostics;
  - runtime status;
  - foam test emission;
  - debug view descriptions.
- Break `DrawRuntimeDisturbances` into:
  - enable/preset controls;
  - pressure controls;
  - wake controls;
  - impact controls;
  - runtime diagnostics.

**Do not do in the first pass:**

- Do not rename serialized property strings.
- Do not change Undo labels or multi-target application semantics.
- Do not move editor code into runtime folders.
- Do not remove legacy migration/help text without a separate decision.

**Validation gate:**

- Unity editor compile succeeds.
- Inspector opens for one and multiple selected rivers.
- Foldouts, buttons, cache controls, and test emit buttons still work.
- Undo still records preset applications.
- Asset creation paths still create the expected cache assets.
- No runtime assembly receives editor-only dependencies.

**Checklist:**

- [ ] Serialized property names captured.
- [ ] Partial editor split completed.
- [ ] Inspector opens.
- [ ] Multi-object editing sanity checked.
- [ ] Cache asset creation checked.
- [ ] Runtime debug sections checked.
- [ ] Long UI methods reduced.
- [ ] Documentation updated.

## Opportunity 5: `StylizedRiver.cs`

**Priority:** High, but riskier than editor/runtime partials because of serialization.

**Current size:** 3945 lines.

**Recommended pass count:** 4.

**Status:** Deferred as of 2026-07-03. Re-evaluate this opportunity before starting any pass; the checklist below is retained as a planning snapshot, not an active implementation queue.

**Pass checklist:**

- [ ] Pass 1 - Serialization-safe partial split: convert to partial and move methods by responsibility while leaving serialized field declarations and names stable.
- [ ] Pass 2 - Preset/domain/geometry split: separate preset application, domain queries, surface/corridor build, generated object cleanup, and runtime component creation.
- [ ] Pass 3 - Validation/material binding reduction: reduce `ValidateSettings` and `ApplyBodyProperties` into grouped helpers without changing shader property names.
- [ ] Pass 4 - Scene and Inspector validation cleanup: verify serialized values, editor property lookup, regeneration flows, material binding, and update this log.

**Observed shape:**

- Main `MonoBehaviour` river component.
- Large serialized configuration surface: 156 `[SerializeField]` occurrences detected.
- Large public property surface.
- Long methods:
  - `ValidateSettings` - about 309 lines.
  - `ApplyBodyProperties` - about 195 lines.
  - `ApplyDisturbancePreset` - about 129 lines.
- Owns or coordinates:
  - serialized authoring controls;
  - domain and geometry build;
  - surface/corridor output objects;
  - material property binding;
  - preset application;
  - runtime component creation;
  - live regeneration;
  - generated object cleanup.

**Refactor goal:**

Reduce the main component file without moving serialized state into new components or changing field names.

**Suggested first-pass split:**

- Convert `StylizedRiver` to partial.
- Keep serialized field declarations in one file during the first split.
- Keep Unity attributes and type declaration in the main file.
- Split methods into partial files:
  - `StylizedRiver.Presets.cs`
  - `StylizedRiver.Validation.cs`
  - `StylizedRiver.Domain.cs`
  - `StylizedRiver.GeometryBuild.cs`
  - `StylizedRiver.RuntimeComponents.cs`
  - `StylizedRiver.MaterialProperties.cs`
  - `StylizedRiver.GeneratedObjects.cs`
  - `StylizedRiver.PublicQueries.cs`
- Consider moving enums to `StylizedRiverEnums.cs` only if it does not harm discoverability.

**Suggested second-pass extraction:**

- Extract material property ID constants into a small internal contract class only if the shader contract is also documented.
- Break `ValidateSettings` into grouped validation helpers matching Inspector sections.
- Break `ApplyBodyProperties` into body, lighting, motion, refraction, disturbance, foam, and reflection binding helpers.

**Do not do in the first pass:**

- Do not rename or move serialized fields into a different component.
- Do not change property names used by `StylizedRiverEditor.Find`.
- Do not change generated child object names.
- Do not change runtime component creation logic.
- Do not change shader property names.

**Validation gate:**

- Existing scenes retain all river Inspector values.
- Inspector still finds every serialized property.
- Regenerate, rebuild surface, rebuild corridor, and clear generated still work.
- Material still receives body/motion/refraction/disturbance/foam/reflection properties.
- Runtime disturbance and Foam components are still created and cached.

**Checklist:**

- [ ] Serialized field list captured.
- [ ] Editor property-name dependencies captured.
- [ ] Partial split completed without field moves.
- [ ] Scene serialization checked.
- [ ] Regeneration workflow checked.
- [ ] Material binding checked.
- [ ] Long method extraction completed.
- [ ] Documentation updated.

## Opportunity 6: Foam Topology Generator Files

**Priority:** High.

**Recommended pass count:** 4.

**Status:** Deferred as of 2026-07-03. Re-evaluate this opportunity before starting any pass; the checklist below is retained as a planning snapshot, not an active implementation queue.

**Pass checklist:**

- [ ] Pass 1 - Determinism baseline and Major generator split: record expected output, then split Major topology generation into context, selection, placement, and rasterization helpers.
- [ ] Pass 2 - Connector generator split: split component discovery, pair selection, pathfinding, prepared catalogue, and rasterization helpers.
- [ ] Pass 3 - Pocket generator split: split hosted pockets/cavities, free-water events, weak spans, rasterization, and merge helpers.
- [ ] Pass 4 - Shared helper review and cleanup: only extract genuinely shared helpers after deterministic output is rechecked, then update this log.

**Current files:**

- `StylizedRiverFoamPocketTopologyGenerator.cs` - 4282 lines.
- `StylizedRiverFoamConnectorTopologyGenerator.cs` - 3519 lines.
- `StylizedRiverFoamMajorTopologyGenerator.cs` - 1694 lines.
- Related follow-up: `StylizedRiverFoamMajorCandidateGenerator.cs` - 1111 lines.

**Observed shape:**

- Static generator classes with public `Generate` entry points.
- No Unity serialization detected.
- Large algorithmic methods:
  - Pocket `Generate` - about 576 lines.
  - Pocket `CreateEmpty` - about 404 lines.
  - Pocket `MergeUnpreparedHostedPrefix` - about 367 lines.
  - Connector `Generate` - about 476 lines.
  - Connector `BuildCandidatePairs` - about 273 lines.
  - Connector `ResolvePreparedMajorPairKey` - about 214 lines.
  - Major `Generate` - about 499 lines.
  - Major `EvaluatePlacement` - about 304 lines.
- Called by `StylizedRiverFoamRuntime`.
- Connector and Pocket generators depend on shared field context from the Major generator.

**Refactor goal:**

Separate pure algorithm responsibilities and nested helper data while preserving each public `Generate` entry point.

**Suggested first-pass split:**

For each generator, use one of two safe patterns:

1. Convert the static class to partial and split helpers by algorithm phase.
2. Keep the public class as a thin entry point and move private helpers into internal helper classes only when the data boundary is obvious.

Suggested file groups:

- `StylizedRiverFoamMajorTopologyGenerator.cs`
  - Keep public `Generate`.
- `StylizedRiverFoamMajorTopologyGenerator.Context.cs`
  - Fluid context and candidate analysis.
- `StylizedRiverFoamMajorTopologyGenerator.Selection.cs`
  - Opportunity building, placement scoring, recycle anchor selection.
- `StylizedRiverFoamMajorTopologyGenerator.Rasterization.cs`
  - Mask/sample output and region construction.
- `StylizedRiverFoamConnectorTopologyGenerator.cs`
  - Keep public `Generate`.
- `StylizedRiverFoamConnectorTopologyGenerator.Components.cs`
  - Component discovery and endpoint construction.
- `StylizedRiverFoamConnectorTopologyGenerator.Pathfinding.cs`
  - Path search, path segment resolution, heap/union-find helpers.
- `StylizedRiverFoamConnectorTopologyGenerator.Rasterization.cs`
  - Connector rasterization and prepared catalogue path construction.
- `StylizedRiverFoamPocketTopologyGenerator.cs`
  - Keep public `Generate`.
- `StylizedRiverFoamPocketTopologyGenerator.Hosted.cs`
  - Hosted interior pocket and edge cavity logic.
- `StylizedRiverFoamPocketTopologyGenerator.FreeWater.cs`
  - Free-water opportunity and recycle-anchor logic.
- `StylizedRiverFoamPocketTopologyGenerator.WeakSpan.cs`
  - Connector weak span candidates and preparation.
- `StylizedRiverFoamPocketTopologyGenerator.Rasterization.cs`
  - Raster samples, region masks, merge helpers.
- `StylizedRiverFoamTopologyGenerator.Shared.cs`
  - Only for helpers proven to be shared across generators without introducing circular coupling.

**Suggested second-pass extraction:**

- Break each `Generate` method into a readable pipeline of private phase methods.
- Move nested heap/union-find/simple data helpers into small internal types if they are independent.
- Extract hash/mix helpers only if the deterministic seeding contract is documented and tested.

**Do not do in the first pass:**

- Do not change public `Generate` signatures.
- Do not change stable IDs, seed mixing, sorting order, or tie-breakers.
- Do not change prepared topology data shapes.
- Do not merge the three generators into one shared abstraction.
- Do not deduplicate similar-looking math until deterministic output has a baseline.

**Validation gate:**

- Existing topology cache builds still produce accepted output.
- Same input produces byte-identical or explicitly approved equivalent topology data.
- Runtime-generated topology still uploads to Foam runtime.
- Connector, Major, Pocket, and Weak Span debug views still display expected classes.

**Checklist:**

- [ ] Public generator API captured.
- [ ] Determinism baseline captured.
- [ ] Major generator split completed.
- [ ] Connector generator split completed.
- [ ] Pocket generator split completed.
- [ ] Candidate generator reviewed.
- [ ] Determinism rechecked.
- [ ] Documentation updated.

## Opportunity 7: `StylizedRiverFoamTopologyCacheCodec.cs`

**Priority:** Medium-high.

**Current size:** 1907 lines.

**Recommended pass count:** 3.

**Status:** Deferred as of 2026-07-03. Re-evaluate this opportunity before starting any pass; the checklist below is retained as a planning snapshot, not an active implementation queue.

**Pass checklist:**

- [ ] Pass 1 - Format baseline and mechanical split: document the current cache format, sample existing assets, and split package/contracts from codec entry points.
- [ ] Pass 2 - Read/write/validate split: separate deserialization, serialization, validation, and primitive helpers without changing byte layout.
- [ ] Pass 3 - Round-trip validation and cleanup: verify old cache reads, new cache writes, round-trip compatibility, and update this log.

**Observed shape:**

- Internal package type plus codec class in one file.
- Handles serialization and deserialization of multiple topology families.
- Long methods:
  - `ReadPocketTopology` - about 270 lines.
  - `TryDeserialize` - about 191 lines.
  - `WritePocketTopology` - about 154 lines.
  - `WriteConnectorPaths` - about 139 lines.
  - `ReadMajorTopology` - about 126 lines.
  - `ValidatePackage` - about 87 lines.
- Cache files and generated assets make this higher-risk than its line count alone suggests.

**Refactor goal:**

Split codec responsibilities into readable read/write/validate chunks without changing binary format or cache compatibility.

**Suggested first-pass split:**

- `StylizedRiverFoamTopologyCachePackage.cs`
  - Package DTO and version constants if applicable.
- `StylizedRiverFoamTopologyCacheCodec.cs`
  - Public/internal entry points and format-level orchestration.
- `StylizedRiverFoamTopologyCacheCodec.Read.cs`
  - `TryDeserialize`, package read, major/connector/pocket read helpers.
- `StylizedRiverFoamTopologyCacheCodec.Write.cs`
  - Serialize/write package, major/connector/pocket write helpers.
- `StylizedRiverFoamTopologyCacheCodec.Validate.cs`
  - Validation, compatibility checks, error reporting.
- `StylizedRiverFoamTopologyCacheCodec.Primitives.cs`
  - Low-level binary read/write helpers only if they are truly format-neutral.

**Suggested second-pass extraction:**

- Extract each topology family's read/write pair into a narrow helper once round-trip tests exist.
- Add an explicit format-contract note near version constants.

**Do not do in the first pass:**

- Do not change cache version.
- Do not change byte order or field ordering.
- Do not alter generated cache asset GUIDs.
- Do not silently migrate or discard old cache data.

**Validation gate:**

- Existing cache assets deserialize.
- New cache assets serialize and deserialize.
- Round-trip output is identical or explicitly approved.
- Runtime cache assignment still accepts valid cache assets and rejects invalid ones.

**Checklist:**

- [ ] Current format contract documented.
- [ ] Existing cache assets sampled.
- [ ] Mechanical split completed.
- [ ] Deserialize existing cache validated.
- [ ] Serialize new cache validated.
- [ ] Round-trip checked.
- [ ] Documentation updated.

## Opportunity 8: Disturbance Compute and Footprint Resolver Package

**Priority:** Medium-high.

**Recommended pass count:** 4.

**Status:** Deferred as of 2026-07-03. Re-evaluate this opportunity before starting any pass; the checklist below is retained as a planning snapshot, not an active implementation queue.

**Pass checklist:**

- [ ] Pass 1 - Contract baseline: record compute kernels, C# resolver public API, buffer/texture/property names, sample limits, and current debug workflows.
- [ ] Pass 2 - Disturbance compute include split: move structs, sampling, pressure, wake, ripple, and boundary helpers into includes while preserving kernel entry points.
- [ ] Pass 3 - Footprint resolver split: split bounds, exact footprint, pressure support, contour rows, pressure bake profile, and primitive helpers.
- [ ] Pass 4 - Long-method reduction and validation cleanup: reduce the largest CPU/GPU helpers, validate static source workflows and debug views, then update this log.

**Current files:**

- `CS_RiverDisturbance.compute` - 1836 lines.
- `RiverDisturbanceFootprintResolver.cs` - 1579 lines.
- `RiverWaterDisturbance.hlsl` - 481 lines.

**Observed shape:**

- `CS_RiverDisturbance.compute` has 11 kernels:
  - `ClearRange`
  - `BakeStaticPressure`
  - `FinalizeStaticPressure`
  - `BakeStaticWakeSource`
  - `BakeRippleBoundaryBase`
  - `BakeRippleBoundaryObstacle`
  - `ApplyRippleBoundary`
  - `InjectRipple`
  - `InjectWake`
  - `SimulateRipple`
  - `SimulateWake`
- Large compute functions:
  - `SampleStaticWakeVariationProfile` - about 229 lines.
  - `SimulateRipple` - about 195 lines.
  - `EvaluateHeightAwareStaticPressure` - about 143 lines.
  - `EvaluateRippleObstacleSignedDistance` - about 129 lines.
- `RiverDisturbanceFootprintResolver` has long C# methods:
  - `TryResolvePressureSupport` - about 264 lines.
  - `TryResolve` - about 199 lines.
  - `TryResolveContourRow` - about 191 lines.
  - `TryResolveBoundsOnly` - about 163 lines.
  - `TryBuildPressureBakeProfile` - about 156 lines.
- The runtime resolves disturbance compute kernels by exact string names.

**Refactor goal:**

Make disturbance CPU/GPU code easier to navigate without changing output, physics, kernel lookup, or pressure/wake contracts.

**Suggested first-pass split:**

For compute:

- Keep `CS_RiverDisturbance.compute` as kernel manifest and entry point owner.
- Add includes:
  - `RiverDisturbanceComputeStructs.hlsl`
  - `RiverDisturbanceComputeSampling.hlsl`
  - `RiverDisturbanceComputePressure.hlsl`
  - `RiverDisturbanceComputeWake.hlsl`
  - `RiverDisturbanceComputeRipple.hlsl`
  - `RiverDisturbanceComputeBoundary.hlsl`

For C#:

- Convert `RiverDisturbanceFootprintResolver` to partial or split into helper classes:
  - bounds-only resolution;
  - exact footprint resolution;
  - pressure support;
  - contour rows;
  - pressure bake profile;
  - geometric primitives.

For HLSL:

- Split `RiverWaterDisturbance.hlsl` only if helpers are clearly grouped:
  - disturbance UV/bank mask;
  - wake geometry;
  - static dynamics decode;
  - normal application.

**Suggested second-pass extraction:**

- Reduce `TryResolvePressureSupport` into sample selection, row evaluation, support accumulation, and result packing.
- Reduce `TryResolveContourRow` into intersection collection, sorting, interval construction, and row output.
- Reduce compute helper functions only after the include split imports cleanly.

**Do not do in the first pass:**

- Do not change pressure sample counts or contour limits.
- Do not change kernel names.
- Do not change C# to GPU data layout.
- Do not tune wake/ripple/pressure behaviour.

**Validation gate:**

- Disturbance compute asset imports.
- All 11 kernels resolve.
- Generated/static source footprints still register.
- Ripple boundary obstacle debug still looks correct.
- Static pressure and static wake outputs still bind to material.

**Checklist:**

- [ ] Kernel/resource contract captured.
- [ ] CPU footprint resolver public API captured.
- [ ] Compute include split completed.
- [ ] C# resolver split completed.
- [ ] Runtime static source workflow validated.
- [ ] Ripple/pressure/wake debug checked.
- [ ] Documentation updated.

## Opportunity 9: River Water Shader Pass and Shared HLSL Includes

**Priority:** Medium.

**Recommended pass count:** 3.

**Status:** Deferred as of 2026-07-03. Re-evaluate this opportunity before starting any pass; the checklist below is retained as a planning snapshot, not an active implementation queue.

**Pass checklist:**

- [ ] Pass 1 - Shader contract baseline: record pass state, property names, include order, debug values, and representative visual modes.
- [ ] Pass 2 - Include and helper split: move pure helpers from the pass/common/refraction includes into smaller include groups without changing material bindings.
- [ ] Pass 3 - Fragment reduction and visual cleanup: reduce the large `Frag` path into named helpers, revalidate visual parity, and update this log.

**Current files:**

- `SH_CleanStylizedRiver.shader` - 1187 lines.
- `RiverWaterCommon.hlsl` - 917 lines.
- `RiverWaterRefraction.hlsl` - 576 lines.
- Related includes: motion, body, lighting, foam, depth.

**Observed shape:**

- Main shader has a large `Frag` function - about 714 lines by scan heuristic.
- Shared HLSL includes contain substantial helper functions:
  - `RiverWaterResolveCurrentVisibleShoreHalfWidth` - about 126 lines.
  - `RiverWaterResolveSilhouettePreservation` - about 379 lines.
  - `RiverWaterDecodeStaticDynamics` - about 257 lines.
- Includes are already present, so this is not a first-principles split. The opportunity is to reduce remaining overlarge pass/helper functions and improve include ownership.

**Refactor goal:**

Keep shader behaviour identical while reducing the size of the main fragment path and grouping shared helpers by contract.

**Suggested first-pass split:**

- Keep `SH_CleanStylizedRiver.shader` as the pass owner.
- Move only pure helper blocks out of the shader pass if they are not already in includes.
- Split `RiverWaterCommon.hlsl` into smaller includes only after include order is mapped:
  - domain/shore metrics;
  - shore wave profile;
  - macro height and surface height;
  - hidden bank cover;
  - current visible shore width.
- Consider splitting `RiverWaterRefraction.hlsl` into:
  - screen-space validation;
  - static ice normal;
  - silhouette preservation.

**Suggested second-pass extraction:**

- Break `Frag` into named helper functions for:
  - input preparation;
  - body/depth colour;
  - surface motion;
  - refraction;
  - disturbance;
  - foam;
  - lighting/final composition;
  - debug output selection.
- Break `RiverWaterResolveSilhouettePreservation` into validation, edge protection, fallback, and final sample helpers.

**Do not do in the first pass:**

- Do not change shader property names.
- Do not change render queue, pass tags, blend state, or depth state.
- Do not change debug view enum values.
- Do not change include order without testing import.

**Validation gate:**

- Shader imports with no compile errors.
- Existing river material renders.
- Liquid/frozen body, motion, refraction, disturbance, foam, and debug views still display.
- No C# material property binding changes are required.

**Checklist:**

- [ ] Include order mapped.
- [ ] Shader property contract captured.
- [ ] Pure helper extraction completed.
- [ ] Shader imports cleanly.
- [ ] Main visual modes checked.
- [ ] Fragment helper extraction completed.
- [ ] Documentation updated.

## Opportunity 10: `MassGenerator.cs`

**Priority:** Medium.

**Current size:** 2241 lines.

**Recommended pass count:** 2.

**Status:** Deferred as of 2026-07-03. Re-evaluate this opportunity before starting any pass; the checklist below is retained as a planning snapshot, not an active implementation queue.

**Pass checklist:**

- [ ] Pass 1 - Mechanical region-to-partial split: convert the existing regions into partial files while preserving `Generate(MassRecipe)`.
- [ ] Pass 2 - Helper cleanup and validation: move independent helper data where obvious, verify generated mass rebuild/editor workflows, and update this log.

**Observed shape:**

- Static procedural generator outside the river runtime.
- Already has region markers:
  - Plane-cut mass.
  - Radial polished mass.
  - Shared transformation and mesh output.
  - Geodesic topology.
  - Helpers and settings.
- Public API appears narrow: `MassGenerator.Generate(MassRecipe recipe)`.
- Longest methods are smaller than the river runtime/editor hotspots, but the file is still a large standalone monolith.
- Referenced by `GeneratedMass`.

**Refactor goal:**

Convert the existing region structure into actual files with minimal logic changes.

**Suggested first-pass split:**

- Convert `MassGenerator` to partial.
- Keep public `Generate` in `MassGenerator.cs`.
- Split by existing regions:
  - `MassGenerator.PlaneCut.cs`
  - `MassGenerator.Radial.cs`
  - `MassGenerator.Transform.cs`
  - `MassGenerator.MeshOutput.cs`
  - `MassGenerator.Geodesic.cs`
  - `MassGenerator.Helpers.cs`
  - `MassGenerator.Settings.cs`

**Suggested second-pass extraction:**

- If helper data types are independent, move them into small internal structs/classes.
- Consider separating recipe sanitization from mesh construction if scan confirms that boundary.

**Do not do in the first pass:**

- Do not change `Generate(MassRecipe)` signature.
- Do not alter mesh topology output.
- Do not change vertex ordering, normals, UVs, or deterministic recipe behaviour.

**Validation gate:**

- Generated mass still rebuilds.
- Existing mass assets retain serialized recipe values.
- Generated mesh visual shape and collider interaction remain unchanged.
- Generated mass editor still works.

**Checklist:**

- [ ] Public API captured.
- [ ] Region-to-file split completed.
- [ ] GeneratedMass rebuild checked.
- [ ] GeneratedMassEditor checked.
- [ ] Optional helper extraction completed.
- [ ] Documentation updated.

## Watchlist: Not Top-Priority Yet

These files are large enough to keep visible, but should probably wait until the higher-priority items are under control or until a specific feature touches them:

- `StylizedRiverCorridorGeometry.cs` - 1269 lines.
- `GeneratedMassEditor.cs` - 1176 lines.
- `StylizedRiverFoamMajorCandidateGenerator.cs` - 1111 lines.
- `StylizedRiverGeometry.cs` - 957 lines.
- `GroundGenerator.cs` - 851 lines.
- `StylizedRiverFoamPocketTopology.cs` - 808 lines.
- `RiverObstacleExclusionResolver.cs` - 674 lines.

## Suggested Overall Order

1. Split `StylizedRiverEditor.cs`.
   - Lowest runtime risk and immediately improves reviewability.
2. Split `StylizedRiverFoamRuntime.cs` mechanically into partials.
   - Largest win, but keep it behaviour-preserving.
3. Split `StylizedRiverDisturbanceRuntime.cs` mechanically into partials.
   - Similar runtime risk profile, with public diagnostics to preserve.
4. Split `CS_RiverFoam.compute` into includes.
   - Do after the C# runtime split so compute binding ownership is easier to see.
5. Split `StylizedRiver.cs` carefully.
   - High value, but serialized-field risk means it should follow safer wins.
6. Split Foam topology generator files.
   - Good algorithm readability win; determinism baseline required.
7. Split cache codec.
   - Needs round-trip validation.
8. Split disturbance compute/footprint resolver.
   - Needs CPU/GPU contract validation.
9. Split shader pass/shared includes.
   - Needs visual validation.
10. Split `MassGenerator.cs`.
   - Good non-river cleanup once river pressure is reduced.

## Per-Item Completion Log

Use this section as work proceeds.

| Date | Item | Change | Validation | Notes |
|---|---|---|---|---|
| 2026-07-02 | Item 1, Pass 1 | Split `StylizedRiverFoamRuntime` into partial files for constants, state records, public surface, and lifecycle/public runtime commands. | Textual symbol-location and brace-balance checks passed; command-line compile could not run because no .NET SDK is installed. | Behaviour-preserving mechanical move only; Unity import/compile remains required. |
| 2026-07-02 | Item 1, Pass 2 | Split `StylizedRiverFoamRuntime` resource initialization/allocation/release, compute kernels/dispatch helpers, and material binding into focused partial files. | Textual symbol-location, method-equivalence, brace-balance, and `git diff --check` checks passed; command-line compile could not run because no .NET SDK is installed. | Behaviour-preserving mechanical move only; Unity import/compile remains required. |
| 2026-07-02 | Item 1, Pass 3 | Split `StylizedRiverFoamRuntime` topology cache/startup validation, topology replacement/transition, generated topology build/signature/upload, and obstacle/boundary exclusion code into focused partial files. | Textual symbol-location, method-equivalence, brace-balance, and `git diff --check` checks passed; command-line compile could not run because no .NET SDK is installed. | Behaviour-preserving mechanical move only; Unity import/compile remains required. |
| 2026-07-02 | Item 1, Pass 4 | Split `StylizedRiverFoamRuntime` manual injection/reservation, Major evolution, FreeWater evolution, HostedNegative evolution, Connector identity/evolution, and shared evolution helper code into focused partial files. | Textual symbol-location, method-equivalence, brace-balance, and `git diff --check` checks passed; command-line compile could not run because no .NET SDK is installed. | Behaviour-preserving mechanical move only; Unity import/compile remains required. |
| 2026-07-02 | Item 1, Pass 5 | Moved the remaining runtime shell methods and member storage into focused partial files, restored the topology-cache validation XML summary beside its method, and reduced `UpdateConnectorEvolutionDescriptors` into ordered phase helpers. | Whole-method equivalence passed for the final moved methods; connector phase extraction check passed; brace-balance and `git diff --check` checks passed; command-line compile could not run because no .NET SDK is installed. | Final cleanup pass for Item 1; Unity import/compile remains required. |
| 2026-07-02 | Item 2, Pass 1 | Converted `StylizedRiverDisturbanceRuntime` to partial and split public disturbance contracts, diagnostics/static public surface, constants/property IDs, member storage, and internal state records into focused partial files. | Range-equivalence, symbol-location, brace-balance, and `git diff --check` checks passed; command-line compile could not run because no .NET SDK is installed. | Behaviour-preserving mechanical move only; Unity import/compile remains required. |
| 2026-07-02 | Item 2, Pass 2 | Split `StylizedRiverDisturbanceRuntime` resource allocation/release, kernel setup, generic compute dispatch, domain dispatch wrappers, and renderer material binding into focused partial files. | Method-equivalence, symbol-location, brace-balance, and `git diff --check` checks passed; command-line compile could not run because no .NET SDK is installed. | Behaviour-preserving mechanical move only; Unity import/compile remains required. |
| 2026-07-02 | Item 2, Pass 3 | Split `StylizedRiverDisturbanceRuntime` generated-source syncing, continuous/static source APIs, impact/debug emission and reservations, static pressure profiles, static wake source/variation code, ripple boundary/metrics, and source-path coordinate/chunk helpers into focused partial files. | Method-equivalence, symbol-location, brace-balance, and `git diff --check` checks passed; command-line compile could not run because no .NET SDK is installed. | Behaviour-preserving mechanical move only; Unity import/compile remains required. |
| 2026-07-02 | Item 2, Pass 4 | Reduced the largest remaining `StylizedRiverDisturbanceRuntime` flows by extracting generated static-pressure resolution and static bake upload helpers while preserving generated-source diagnostics and emitter/static-source call order. | Focused call-site review, symbol-location, brace-balance, trailing-whitespace, and `git diff --check` checks passed; command-line compile could not run because no .NET SDK is installed. | Final cleanup pass for Item 2; Unity import/compile remains required. |
| 2026-07-03 | Item 3, Pass 1 | Recorded the `CS_RiverFoam.compute` baseline in `CS_RiverFoam_Compute_Refactor_Baseline.md`, including kernel manifest, current include order, struct layouts, resource/uniform declarations, helper dependency groups, and C# binding assumptions. | Kernel pragma list and C# `FindKernel` names were cross-checked; `_Foam...` compute/material string contracts were scanned; no compute code was moved in this pass. | Baseline-only pass for the upcoming include split; Unity compute import/compile remains required after future code movement. |
| 2026-07-03 | Item 3, Pass 2 | Split `CS_RiverFoam.compute` structs, resource/uniform declarations, coordinate/load helpers, bilinear sampling helpers, and hash/noise/injection-shape helpers into focused HLSL includes while leaving all kernel entry points in the main compute asset. | Kernel pragma/C# `FindKernel` contract check passed; moved struct/function equivalence and `_Foam...` symbol-set checks passed; `git diff --check` reported only existing line-ending warnings. | Behaviour-preserving include split only; Unity compute import/compile remains required. |
| 2026-07-03 | Item 3, Pass 3 | Split `CS_RiverFoam.compute` support envelope, network/guidance, motion, topology helper, evolution/identity, and topology-transition helpers into focused HLSL includes while keeping all kernel entry points in the main compute asset. | Kernel pragma/C# `FindKernel` contract check passed; moved helper definitions no longer remain in the main compute file; include line endings are consistently CRLF; `git diff --check` reported only existing line-ending warnings. | Behaviour-preserving include split only; Unity compute import/compile remains required. |
| 2026-07-03 | Item 3, Pass 4 | Reduced the largest remaining `CS_RiverFoam.compute` kernel by extracting simulation neighbourhood sampling, population source-need, and distributed supply helpers into `CS_RiverFoam.Simulation.hlsl`; normalized the main compute and include files to consistent CRLF endings. | Kernel pragma/C# `FindKernel` contract check passed; main compute still contains the 19 kernel entry functions; all `CS_RiverFoam` compute/include files have consistent line endings; `git diff --check` reported only the checklist line-ending warning. | Final cleanup pass for Item 3; Unity compute import/compile remains required. |
| 2026-07-03 | Items 4-10 deferred | Marked the remaining refactor opportunities as deferred after completing the first three priority items. | No code changes for the deferred items; their pass plans remain in the document as planning snapshots. | Re-scan and re-prioritize these opportunities before starting future refactor passes. |
