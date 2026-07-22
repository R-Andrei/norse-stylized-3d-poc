# River Foam Fixed-Metric Grid Upgrade Plan

## 0. Document control

| Field | Value |
|---|---|
| Proposed canonical repository path | `Assets/Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md` |
| Companion dependency register | `Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md` |
| Date | 2026-07-18 |
| Project | Norse Stylized 3D PoC |
| Engine | Unity 6000.5.0f1, URP |
| Work type | Canonical architecture and implementation plan |
| Architecture status | Fixed-metric, centreline-relative river-space lattice accepted as the direction |
| Implementation status | **`RG-METRIC-P2` through `RG-METRIC-P12d` are closed. The P12d Unity matrix passed all 12 cases and selected `0.15 m`. P12e is Unity-imported. Unity rejected P12f because it detected two contours and fragmented candidate removal. Unity partially accepted `RG-METRIC-P12g`: Current remains exact and the Presence-Amplitude single exterior eligibility contour is correct, but doubled-diameter production admission is rejected as over-broad. `RG-METRIC-P12h` is Unity-rejected because one projected reach still produced a second permission area beyond the displayed eligibility mask. `RG-METRIC-P12i` proved exact Candidate × Eligibility ownership but is Unity-rejected because eligibility remained stippled and fractional. `RG-METRIC-P12j` is Unity-rejected because its clean Presence/life silhouette does not include patterned erosion or structural Strand shaping and therefore does not coincide with rendered Foam. `RG-METRIC-P12k` is mechanically implemented: Presence-Amplitude eligibility and removal will use the exact no-Chip rendered mask `foam.mask × strandKeep`; Current remains unchanged.** |
| Code authorization | **The user explicitly authorized direct P12 fixed-metric activation and practical Play Mode testing, including temporary visible River breakage. Authorization covers source-default and Inspector selection, real allocation/invalidation ownership, active-selection diagnostics, and read-only candidate evidence. Scene/prefab/material/cache-asset raw edits and automatic cache writes remain outside scope.** |
| Persistent game-file changes made while producing this document | P12 adds two serialized River enum fields, production descriptor selection/invalidation ownership, active-selection diagnostics, and one Editor-only P12 report. P12a/P12b add committed-state presentation ownership, hybrid source-deposition history, lateral face/flux evidence, and report clipboard actions. P12c restores Object Arc/Semi-Arc phase `0/1/2` persistent emission and removes the D3D11 warning-producing helper. P12d adds the nonserialized complete runtime sweep. P12e adds two serialized A/B enums and uniform-selected render/transport branches. P12f is rejected. P12g reuses the existing Presence-footprint uniform for mode-specific Chip eligibility, candidate admission, and removal. P12h is rejected. P12i removes reach-derived admission entirely. P12j is rejected. P12k replaces its surrogate clean silhouette with exact pre-Chip rendered-mask ownership and retires the unused clean-silhouette plumbing; it adds no control, resource, kernel, dispatch, cache, scene, prefab, material, or amplitude compression. |
| Source snapshot used | User-supplied `Assets(72).zip` with accepted P9a, P10, and P10a overlays |
| Source limitations | No `.git` metadata, package manifest, Library, current `Editor.log`, or complete project root in the supplied snapshot |

### 0.1 Purpose of this document

This is the implementation-governing plan for migrating River Foam from its current mixed-coordinate structural grid to a fixed-metric, centreline-relative river-space lattice.

The current grid is approximately fixed in physical metres downstream, but its lateral rows are normalized across each local river cross-section. Consequently, physical lateral cell size changes with river width, equal Y indices at adjacent downstream rows can represent different lateral metre positions, and small automatic sources can inherit visibly anisotropic rectangular structural footprints.

This document converts the accepted architectural direction into a controlled implementation program. It defines:

- the target coordinate contract;
- the exact invariants that may not drift during implementation;
- the required design decisions and when they are made;
- the order in which contracts, resources, topology, sources, transport, rendering, diagnostics, caches, and tooling are migrated;
- the dependency and file boundaries;
- the required pre-change baseline evidence;
- the mechanical, numerical, visual, cache, compatibility, and performance validation gates;
- rollback checkpoints and stop conditions;
- the boundary between the contiguous one-strip implementation and the later scalable strip-pool implementation;
- the conditions under which the upgrade may be declared complete.

This document is deliberately more detailed than an ordinary implementation plan. The migration changes the meaning of a Foam field coordinate. That meaning is independently reconstructed by multiple CPU modules, GPU kernels, shader includes, topology generators, cache contracts, debug views, and editor tools. Partial migration is not safe.

### 0.2 Relationship to the dependency register

The companion dependency register is the exhaustive static inventory of systems that must be updated, reviewed, or tested. This upgrade plan is the execution order and acceptance contract for those dependencies.

The two documents have different authority:

- the dependency register answers **what can be affected**;
- this plan answers **what will be changed, in what order, under what invariants, with what evidence, and when work must stop**.

Neither document alone is sufficient for implementation.

### 0.3 Status vocabulary

| Status | Meaning |
|---|---|
| `NOT STARTED` | No implementation work has begun |
| `REVIEW REQUIRED` | Live repository evidence must be collected before editing |
| `DECISION REQUIRED` | A material architecture or compatibility choice must be resolved and recorded |
| `READY` | Prerequisites and scope are satisfied; implementation may begin after authorization |
| `IN PROGRESS` | Authorized work is underway |
| `MECHANICALLY VERIFIED` | Parser/compiler/static and deterministic checks passed, but Unity/runtime evidence may still be pending |
| `UNITY VALIDATED` | Required Unity import, runtime, visual, and profiler checks passed |
| `BLOCKED` | A named unresolved condition prevents continuation |
| `SUPERSEDED` | Replaced by a later recorded plan item |
| `COMPLETE` | Implementation and every required verification gate passed |

### 0.4 Current plan state

| Plan area | Status |
|---|---|
| Architecture direction | Accepted |
| Dependency inventory | Complete for the supplied snapshot; live-repository re-scan pending |
| Canonical upgrade plan | Installed at the proposed `Assets/Docs/` path by documentation patch 01 |
| Static source review | Complete for all 94 registered paths in the supplied snapshot |
| Live repository review | `BLOCKED`: supplied archive contains no `.git` metadata, branch, HEAD, upstream, status, diff, or history |
| Runtime baseline capture | P12d completed the 12-case runtime matrix; `0.15 m` is selected. P12e is imported and visually exercised; P12f is rejected; P12g eligibility is Unity-accepted but production admission is rejected; P12k exact pre-Chip rendered-mask evidence is pending. |
| Canonical queue reconciliation | Complete for documentation patch 01 |
| Runtime implementation | P2-P12d are closed. P12e A/B options are present. P12k exact pre-Chip rendered-mask correction is mechanically implemented. |
| Unity validation | P2-P12d complete. P12k requires warning-free import, unchanged Current-mode Chipping, and proof that Presence-Amplitude grey support, yellow eligibility, magenta removal, and Final use the same exact pre-Chip rendered geometry while Production Chip never removes outside that exact mask at `0.15 m`. |
| Strip-pool production architecture | Planned future phase; not implemented |

## RG-METRIC-P12d — One-button fixed-spacing and lateral-response sweep

### Objective

Replace the manual candidate workflow with one bounded Play Mode state machine that tests the real runtime at `0.25`, `0.20`, `0.15`, and `0.10 m`, each at lateral ratios `0`, authored, and `1`, then restores authored ownership and writes one combined disk-plus-clipboard report.

### Accepted implementation

- `StylizedRiverFoamRuntime.P12Sweep.cs` owns nonserialized effective-value overrides, the 12-case state machine, timeout/cancellation handling, deterministic reset, evidence capture, report construction, cache-mutation proof, and authored-runtime restoration.
- `Resources.cs` uses the effective requested spacing at the same production descriptor/allocation gates and permits transient topology generation only while the explicit sweep is active.
- `TopologyCache.cs` completes that transient generation without reading, writing, or replacing the assigned cache asset.
- `Compute.cs`, `Binding.cs`, `Lifecycle.cs`, and `PublicSurface.cs` use the effective lateral ratio so the zero/authored/max cases exercise the real transport and rendering contracts.
- Every case reinitializes the real runtime, clears material/source state, warms for two seconds, and captures at least five seconds and 30 frames. Initialization Motion Time is frozen to the suite-start value across all cases for comparable topology generation.
- The report records descriptor dimensions/signature, topology counts and startup cost, CFL/substeps/Jacobian/curvature, persistent memory, dispatch/cell/CPU work, lane face cancellation, material-weighted lateral speed/movement, zero-ratio isolation, assigned-cache immutability, and restoration.
- The Inspector exposes Run, Cancel, progress/status, and `Copy P12 Sweep Report to Clipboard`; the existing single-candidate P12, P7, and P9 reports remain unchanged and available.

### Non-goals

No serialized River change, cache write, scene/prefab/material edit, source/lifecycle/Film/Shape/render retuning, visual-winner selection, or overall Foam-amount tuning. P13 remains the owner of final amount/tier/cache freeze.

### Unity status

The complete 12-case matrix returned `Overall: PASS`, restored authored runtime ownership, and left the assigned cache unchanged. Visual review rejected `0.20 m`; `0.15 m` is selected.


## RG-METRIC-P12e — Presence-amplitude rendering and TVD transport A/B

### Objective

Test the two proven contributors to fat Layer C blobs independently while retaining the accepted result as an exact baseline: weak-Presence visual amplification and first-order donor-cell numerical diffusion.

### Accepted implementation

- `Foam > Runtime & Quality > Material Transport Scheme` exposes `Donor Cell (Current)` and `TVD Superbee`.
- `Foam > Layer E — Rendering > General Composition > Presence Footprint` exposes `Current` and `Presence-Amplitude`.
- Donor Cell returns the exact former face donor before any higher-order neighbour read.
- TVD Superbee reconstructs only interior-face packed states with a bounded componentwise limiter and the existing per-substep CFL contract. The same face velocity/area flux transports Presence, life moment, and pattern moment conservatively; closed faces and endpoint behavior remain unchanged.
- Presence-Amplitude is render-only: the resolved base footprint is capped by raw committed Presence before the existing patterned opaque-body evaluation. Current preserves the former renderer exactly.
- Both options are serialized, independently selectable, live-switchable, and included in P12 reports. They add no resource, kernel, dispatch, cache, topology, source, Film, Shape, or Debug View.

### Acceptance and limits

Mechanical validation must prove exact default compatibility, shader call/binding completeness, unchanged 23-kernel/resource contracts, bounded reconstruction, packed-state invariants, conservation under periodic model cases, and a non-amplifying Presence cap. Unity must still establish warning-free import, actual GPU behavior, visual value, and cost. TVD is not represented as a formal zero-support-growth guarantee.

### Status

Implementation and static/model/package validation are complete. Unity import succeeded and visual review retained Presence-Amplitude for further testing, but Chip edge eligibility did not follow much of its rendered perimeter.


## RG-METRIC-P12g — Mode-specific single-contour Chip admission

### Objective

Reject P12f's hardened-mask derivative and per-pixel candidate clipping. Preserve the accepted Current path exactly. Give Presence-Amplitude one monotonic exterior edge coordinate, admit complete connected edge candidates, and remove them coherently from the hardened pre-Chip mask. Do not alter Presence amplitude, transport, source/lifecycle behavior, candidate identity, or Current output.

### Approved implementation

- Keep the existing `_FoamPresenceFootprintMode` selection owner; add no control.
- Current remains `preChipSoftVisibility` with edge start `0.06`, existing derivative normalization, candidate × edge-band selection, and soft-mask reconstruction exactly.
- Presence-Amplitude uses `preChipSoftVisibility` with calibrated edge start `0.148228`, where the unchanged hardening function reaches the existing `preChipMask = 0.08` rendered-support boundary.
- `Chip Eligibility Composite` remains the narrow candidate-independent exterior band.
- Presence-Amplitude accumulates complete connected candidates admitted by Chip Edge Width plus each candidate's current bounded projected contour diameter; it does not multiply final candidate geometry by the narrow band.
- Presence-Amplitude carves the admitted selection directly from the hardened pre-Chip mask; Current keeps the accepted soft-mask reconstruction.
- Presence-Amplitude remains `baseMask = min(baseMask, presence)` exactly. No compression, new threshold control, diagnostic, resource, kernel, dispatch, or serialized state is allowed.

## RG-METRIC-P12h — Edge-attached Presence Chip bites

### Objective

Preserve P12g's accepted single exterior eligibility contour and direct hardened-mask carving. Reject only its over-broad production permission, which used Edge Width plus two projected candidate reaches and produced magenta interior removal far beyond the yellow eligibility band.

### Contract

- Current remains byte-identical: soft edge start `0.06`, candidate × edge-band selection, Interior Access, and soft-mask reconstruction.
- Presence-Amplitude eligibility remains byte-identical: monotonic `preChipSoftVisibility`, calibrated start `0.148228`, and the same candidate-independent yellow exterior band.
- Presence-Amplitude production evaluates the unchanged analytical candidate field inside `Chip Edge Width + one projected candidate reach`; this is broader than narrow-band clipping but deliberately narrower than P12g complete-candidate admission.
- Presence-Amplitude direct hardened-mask carving remains unchanged.
- Presence-Amplitude remains `baseMask = min(baseMask, presence)` exactly. No compression, control, candidate, transport, source, lifecycle, Film, Shape, resource, kernel, dispatch, or serialized-state change is allowed.

### Unity acceptance

Require warning-free import, unchanged Current behavior, the unchanged one exterior yellow contour, coherent magenta bites visibly attached to nearby eligible edges, and no broad detached interior Production Chip regions.

### Acceptance

Mechanical validation must prove Current and eligibility byte/model equivalence, one-reach permission as a strict subset of P12g, unchanged direct-carve boundedness, stable shader signatures/calls, and no protected-file/resource/property change. Unity must import without warnings, preserve the one yellow exterior contour, show coherent magenta bites attached to nearby eligible edges, remove broad detached interior Production Chip regions, and leave Current mode visually unchanged.

### Status

Implementation and final mechanical/package validation are complete. Primary audit gates pass 46/46 and the independent audit passes 18/18; 593 protected files remain byte-identical; eligibility and Chip application are byte-identical; the only executable HLSL change removes one extra projected reach; randomized one-reach, direct-carve, HLSL parse, and archive-byte gates pass. Unity import and visual validation remain pending.


## 1. Authority, precedence, and change control

### 1.1 Authority order

Implementation must apply authority in this order:

1. repository `AGENTS.md` and any more local `AGENTS.md` applying to the edited files;
2. explicit current user direction;
3. this upgrade plan after it has been placed in the live repository and reconciled with live evidence;
4. the fixed-metric dependency register;
5. `River_Foam_Active_Blockers_and_Next_Patches.md`;
6. `River_Foam_Stage6_Architecture.md`;
7. `River_Rendering_Roadmap.md`;
8. the current implementation and serialized configuration;
9. historical commits and superseded records.

If the live repository conflicts with a path, symbol, serialized value, or assumption recorded here, the live repository is authoritative evidence. The plan must then be amended before implementation continues.

### 1.2 Mandatory repository workflow

The repository instructions establish four non-bypassable gates:

1. complete a read-only review before editing;
2. create or update the persistent canonical plan before implementation;
3. implement strictly from recorded plan items and approved scope;
4. complete a post-implementation consistency and compliance audit.

This document satisfies the standalone planning request, but it does not substitute for the first live-workspace gate. Before code edits, the implementation thread must still inspect:

- current Git status and branch;
- pre-existing diffs;
- the exact current versions of every expected file;
- direct callers, consumers, producers, and shared contracts;
- current cache assets and editor tooling;
- relevant accepted and superseded commits;
- current Unity baseline behavior and diagnostics.

### 1.3 Material-deviation rule

Implementation must stop before continuing if any of the following occurs:

- a required file is outside the approved scope;
- a new resource, buffer, texture, kernel, component, folder, serialized field, dependency, scene edit, prefab edit, material edit, layer, or tag becomes necessary but is not recorded and approved;
- a source family cannot preserve accepted behavior under the selected unit policy;
- CPU and GPU mappings cannot be made identical without changing the descriptor contract;
- a cache change would require raw asset editing;
- the fixed-metric allocation exceeds texture, cache, memory, dispatch, or active-cell limits;
- a candidate causes an unapproved additional transport substep;
- curvature error exceeds the adopted policy;
- the current Arc/Semi-Arc accepted baseline cannot be reproduced before migration;
- unrelated Disturbance, corridor, water rendering, lighting, refraction, or geometry behavior changes;
- tests reveal that Stage 1 cannot meet its stated acceptance criteria without Stage 2 strip infrastructure.

When a stop condition occurs, evidence must be added to the canonical plan, the affected item marked `BLOCKED`, and the design/scope decision resolved before further edits.

### 1.4 No opportunistic cleanup

The upgrade does not authorize unrelated refactors, formatting, file moves, symbol renames, public API cleanup, inspector reorganization, scene migration, prefab migration, shader cleanup, cache format redesign beyond the required contract, or Disturbance architecture changes.

## 2. Executive decision

The River Foam structural field will migrate to a **fixed-metric lattice in centreline-relative river coordinates**.

The coordinate axes are:

- `s`: physical metres along the oriented river centreline/domain;
- `n`: signed physical metres laterally from the river centreline, with a stable left/right convention.

The migration will not use a world-aligned XZ simulation field.

The first implementation will be a contiguous field represented as **one strip using the final strip-compatible coordinate semantics**. It will not be a temporary normalized-row remap. All later strips belonging to one river or connected river network will share:

- the same metric lateral spacing;
- the same lateral lattice phase;
- the same signed lateral orientation;
- integer global-Y indices.

The contiguous one-strip implementation is the validation and short/uniform-river representation. It is not the general scalability solution for arbitrary length and width. Local-width strip allocation, cross-strip transport, pooling, renderer indirection, and active scheduling are later phases governed by this plan.

### 2.1 Why this decision is required

The existing Medium configuration uses approximately:

```text
longitudinal spacing = 32 m / 96 = 0.3333 m
lateral spacing on a 4.9 m river = 4.9 m / 96 = 0.05104 m
nominal aspect ratio = 0.3333 / 0.05104 ≈ 6.53 : 1
```

The physical minimum macro source footprint therefore differs radically by orientation. A small source can be narrow laterally while remaining at least one long downstream cell. At broader widths, the opposite problem appears: lateral cells become physically large because row count remains fixed.

The fixed metric lattice addresses both defects and also removes hidden cross-row lateral squeeze/stretch: identical Y indices at adjacent downstream columns will represent identical signed lateral metre coordinates rather than identical normalized cross-river fractions.

### 2.2 What this decision does not imply

It does not imply:

- that every quality tier immediately adopts `0.25/0.15/0.10 m`;
- that every source dimension remains numerically unchanged;
- that all cell-based morphology becomes metre-based;
- that Disturbance fields adopt Foam dimensions;
- that a single contiguous texture is sufficient for production-scale rivers;
- that inter-river Foam transfer is included in Stage 1;
- that broad highly curved rivers are correct without a curvature policy;
- that physical fidelity can remain constant over arbitrarily increasing water area at constant total cost.

## 3. Mission objective and success definition

### 3.1 Primary objective

Replace the current normalized lateral Foam grid with a fixed-metric, centreline-relative lattice while preserving or deliberately re-establishing all accepted River Foam behavior.

### 3.2 User-visible success criteria

The upgrade succeeds only if all of the following are true:

1. A short or small source is no longer forced into a visibly long downstream rectangular macro block at the accepted quality.
2. Comparable rivers using the same Foam quality have comparable physical structural cell dimensions regardless of river width or total length.
3. The current approximately 4.9-metre demo river gains materially better downstream source resolution without increasing baseline structural work beyond the approved threshold.
4. Arc and Semi-Arc remain accepted thin front-owned C/half-C shapes with no rear wrap, no upstream source, no detached unintended segments, correct Build/Hold/Release, and preserved source ownership.
5. Shore Ribbon, Inward Wash, Fleck, Lace, Cross-Lace, Torn Fragment, manual injection, and isolated probes retain intended physical dimensions and lifecycle behavior.
6. Persistent material transport is conservative within accepted tolerance, follows equal lateral metre positions, and remains stable under the selected cadence/substep policy.
7. Topology, obstacle exclusion, routing, pressure support, motion lane, disturbance sampling, visual occupancy, shape evaluation, and rendering remain aligned to the same physical points.
8. Existing normalized-grid cache artifacts are rejected deterministically; new caches are generated only through approved tooling.
9. Low, Medium, and High remain serialized-compatible shared quality values, while Foam receives an explicit Foam-specific metric mapping.
10. Disturbance allocation, corridor geometry tessellation, river geometry, lighting, refraction, water colour, and unrelated rendering remain unchanged unless a separately approved dependency requires a narrowly scoped interface update.
11. No scene, prefab, or material is raw-edited or incidentally reserialized.
12. Diagnostics report requested/resolved cell sizes, allocation waste, physical source areas, CFL components, memory, dispatches, cache state, and curvature headroom accurately.
13. The contiguous implementation is explicitly bounded by texture/cache/length/width constraints and does not silently violate fixed-metric scale.
14. The final documentation contains no active normalized-lateral instructions that contradict the implementation.

### 3.3 Engineering success criteria

The upgrade must also satisfy:

- one authoritative descriptor supplies all grid dimensions and conversions;
- CPU and GPU cell-centre calculations match within a recorded numerical tolerance;
- no module derives `dx`, `dy`, lateral origin, or global-Y indexing independently;
- no production renderer reconstructs Foam Y from local `surfaceHalfWidth`;
- all 22 compute kernels are classified and validated;
- all dependency-register files are dispositioned as modified, reviewed/tested, or future-only;
- all changed C# files pass an available real parser/compiler, required namespaces are present, malformed multiline strings are scanned, and project line endings are preserved;
- all changed HLSL/compute functions pass available parsing/code generation and Unity shader import remains authoritative;
- final diff matches approved scope and plan items exactly.

## 4. Scope

### 4.1 Stage 1 implementation scope

Stage 1 includes the complete one-strip coordinate-contract migration:

- metric grid descriptor and initialization signature;
- quality-to-requested-metric mapping;
- exact resolved spacing and dimensions;
- CPU field-space conversion;
- GPU coordinate conversion;
- resource allocation and lifecycle;
- metric rows and CPU/GPU ABI changes;
- topology generation, boundary masks, support, obstacle exclusion, routing, motion lane, and source dispatch;
- all automatic and manual birth paths;
- persistent transport, CFL, metrics, and topology replacement;
- half-resolution visual occupancy and shape evaluation;
- production renderer sampling and debug sampling;
- cache contracts, fingerprints, preparation, and build preflight;
- editor labels, diagnostics, and metrics;
- canonical documentation;
- complete mechanical and Unity validation.

### 4.2 Stage 2 production-scaling scope

Stage 2 is a separate implementation program after Stage 1 is frozen:

- fixed-length locally sized strips;
- shared global-Y lattice intervals;
- strip descriptors and cache payloads;
- pooled/bucketed resources or approved texture-array representation;
- cross-strip ghost borders and conservative flux;
- renderer strip lookup/indirection;
- active/offscreen scheduling;
- global memory/cell/dispatch budgets;
- connected-component endpoint compatibility where later required.

Stage 2 is not allowed to redefine the Stage 1 coordinate meaning.

### 4.3 Explicit non-goals for Stage 1

- No world-space sparse field.
- No quadtree or variable-resolution mesh.
- No arbitrary connected-river transfer.
- No cross-river global world lattice.
- No Disturbance resolution migration.
- No corridor geometry redesign.
- No automatic quality degradation hidden from diagnostics.
- No shader-only fake subcell transport.
- No new Foam material-state channel merely to hide structural cells.
- No replacement of current accepted lifecycle architecture.
- No broad River performance rewrite beyond measurements and changes required by the metric field.
- No scene, prefab, or material edit.
- No unrelated source-family redesign.

## 5. Baseline facts and evidence that implementation must re-verify live

The supplied snapshot supports these baseline statements. The live implementation thread must verify them again against the repository before editing.

| Fact | Snapshot evidence |
|---|---|
| Structural quality values are 64/96/128 | `StylizedRiverFoamRuntime.Constants.cs` |
| Longitudinal chunk length is 32 metres | `StylizedRiverFoamRuntime.Constants.cs` |
| Initialization uses chunk count × resolution for X and one resolution for Y | `ResolveInitializationDimensions()` in `StylizedRiverFoamRuntime.Resources.cs` |
| The demo river width is approximately 4.9 m and quality is Medium | `Game/Demo/Scenes/VisualFrameworkDemo.unity` |
| CPU topology Y is normalized across each row | `StylizedRiverFoamTopologyFieldSpace.cs` |
| Compute independently reconstructs normalized lateral metres | `CS_RiverFoam.Coordinates.hlsl` |
| Production render sampling divides lateral metres by local surface half-width | `RiverWaterFoam.hlsl` |
| Automatic source raster evaluates structural cell centres | `CS_RiverFoam.compute` |
| Persistent state remains cell-based | `CS_RiverFoam.Simulation.hlsl` and sampling includes |
| Cache maximum field dimension is 8192 | `StylizedRiverFoamTopologyCacheCodec.cs` |
| Shared `StylizedRiverQuality` is used outside Foam | `StylizedRiver.cs`, Disturbance and geometry implementations |
| Transport CFL uses downstream and lateral spacing | `ResolveTransportSubsteps()` in `StylizedRiverFoamRuntime.Lifecycle.cs` |
| Transport cell area currently uses rectangular `dx × dy` | `CS_RiverFoam.Simulation.hlsl` |
| Signed curvature is available in metric-row topology data | `StylizedRiverFoamRuntime.Topology.cs` and `CS_RiverFoam.Structs.hlsl` |
| Current active Foam plan contains stale queue language | `River_Foam_Active_Blockers_and_Next_Patches.md` |

### 5.1 Baseline evidence package required before code

The live implementation thread must capture and store a concise baseline package containing:

- branch, HEAD, upstream, status, and diff summary;
- all pre-existing user-owned modified paths;
- current `fieldWidth`, `fieldHeight`, valid length, padded length, quality, `dx`, minimum/maximum `dy`, CFL components, substeps, estimated memory, and dispatch rate;
- exact topology cache status and cache asset path;
- current automatic source screenshots for all eight source families;
- current Arc and Semi-Arc hidden-obstacle Automatic Birth Source views through Build, Hold, Release, and Rest;
- current final Foam rendering screenshots under the same camera and settings;
- current topology, obstacle, routing, motion, material-state, film, shape, and boundary debug views;
- current profiler capture or built-in performance accounting for the demo;
- current serialized authoring values relevant to source units;
- accepted current behavior notes and known existing defects.

The package must distinguish behavior to preserve from the block-footprint defect the migration is intended to change.

## 6. Terminology and coordinate contract

### 6.1 Coordinates

| Symbol | Meaning |
|---|---|
| `s` | physical metres along the oriented River Foam domain |
| `n` | signed physical lateral metres from the centreline |
| `x` | local structural column index |
| `localY` | local allocated structural row index |
| `globalY` | integer lateral lattice index shared by strips in one river/network |
| `dx` | resolved physical longitudinal cell spacing |
| `dy` | resolved physical lateral cell spacing |
| `sStart` | field/strip start distance in metres |
| `validLength` | actual Foam domain length containing valid river |
| `allocatedLength` | padded allocated length represented by the field/strip |
| `latticePhase` | centreline-relative offset defining where lateral cell centres lie |
| `globalYBase` | global-Y represented by local row zero |
| `rowCount` | allocated lateral rows |

### 6.2 Canonical descriptor

The implementation must introduce one authoritative conceptual descriptor. The exact C# type and GPU representation are determined during the live review, but the information contract is fixed:

```text
MappingContractVersion
Quality
RequestedDxMetres
RequestedDyMetres
ColumnsPer32MetreChunk
ResolvedDxMetres
ResolvedDyMetres
LateralLatticePhaseMetres
GlobalYBase
RowCount
FieldOrStripStartMetres
AllocatedLengthMetres
ValidLengthMetres
ColumnCount
StructuralCellCount
FilmWidth
FilmHeight
AllocationGuardRows
```

Optional or later-strip fields may include:

```text
StripIndex
PreviousStripIndex
NextStripIndex
GhostColumnCount
AllocationBucket
ConnectedNetworkId
```

The descriptor may be split into immutable configuration, runtime dimensions, and GPU uniforms for implementation efficiency. It must remain one semantic authority.

### 6.3 Longitudinal quantization

For a requested `dxTarget`:

```text
columnsPer32m = ceil(32 / dxTarget)
resolvedDx = 32 / columnsPer32m
chunkCount = ceil(validLength / 32)
columnCount = chunkCount * columnsPer32m
allocatedLength = chunkCount * 32
```

This keeps every 32-metre boundary exactly aligned to an integer column and prevents longitudinal drift between strips.

Implementation must not derive one spacing from `validLength / columnCount` and another from `allocatedLength / columnCount`.

### 6.4 Lateral lattice

The planned default is a centreline-anchored signed lattice. The descriptor must support a phase explicitly even if the initial phase is fixed.

Preferred centre-based form:

```text
globalY = globalYBase + localY
nCentre = latticePhase + globalY * dy
```

With `latticePhase = 0`, global row zero is centred on the river centreline. A cell footprint is:

```text
[nCentre - 0.5*dy, nCentre + 0.5*dy]
```

This avoids an arbitrary half-cell asymmetry about the centreline and gives strips an exact integer correspondence.

The live review must verify that centreline-centred cells do not conflict with any even-row assumption. If a different phase is required, that phase must be stable, serialized/fingerprinted where necessary, and shared by all connected strips.

### 6.5 Lateral allocation range

For the union of physical left/right water extents over a field or strip:

```text
nMin = minimum signed lateral water extent
nMax = maximum signed lateral water extent
```

Cells whose footprints intersect the water range are included. Conceptually:

```text
firstIntersectingY = ceil((nMin - 0.5*dy - latticePhase) / dy)
lastIntersectingY  = floor((nMax + 0.5*dy - latticePhase) / dy)
```

Additional guard rows may be allocated only for a documented stencil, source feather, boundary, transition, or strip-neighbour requirement. Guard rows are invalid water unless the local bank test marks them valid.

The plan does not authorize an arbitrary fixed padding percentage.

### 6.6 Valid water mask

Allocation and physical validity are separate:

- the rectangle allocates a range of `s/n` cells;
- each row’s actual left/right bank limits determine whether each cell intersects or lies within valid water;
- padded downstream cells beyond `validLength` are invalid;
- obstacle exclusion remains a separate occupancy mask;
- topology support remains separate from raw valid water.

All systems must distinguish:

```text
allocated cell
valid river-water cell
obstacle-excluded cell
topology-supported cell
active material cell
visible final-shape cell
```

### 6.7 CPU/GPU parity requirement

For every sampled structural cell, CPU and GPU must agree on:

- `s` centre;
- `n` centre;
- local/world position reconstructed through the domain;
- local/global Y index;
- valid/padded status;
- bank-relative normalized coordinate where still intentionally needed;
- source and obstacle culling ranges.

Parity must be tested at:

- first and last valid columns;
- padded columns;
- global Y zero;
- first/last allocated rows;
- both banks;
- asymmetric rows;
- width-transition rows;
- reversed flow.

## 7. Quality policy and candidate selection

### 7.1 Shared enum policy

`StylizedRiverQuality` remains a shared serialized enum. The upgrade must not reinterpret Disturbance or geometry quality constants.

Foam receives a Foam-specific mapping:

```text
StylizedRiverQuality -> requested Foam metric cell target
```

Existing serialized Low/Medium/High values continue to deserialize unchanged.

### 7.2 Candidate sweep

Before assigning final tier values, the implementation must support and test at least:

| Candidate | Purpose |
|---:|---|
| `0.25 m` | conservative metric baseline |
| `0.20 m` | intermediate tradeoff |
| `0.15 m` | target candidate for current visual defect |
| `0.10 m` | high-detail and performance stress case |

The descriptor may retain separate requested X and Y values for future policy, but initial candidate testing should use square requested cells unless evidence supports intentional anisotropy.

Because X is quantized to an integer divisor of 32 metres, the actual `resolvedDx` is reported and used everywhere. If square resolved cells are selected, `resolvedDy` should equal `resolvedDx`; otherwise the difference must be intentional and documented.

### 7.3 Selection criteria

A permanent tier mapping is selected only after recording for each candidate:

- allocated dimensions and valid-cell ratio;
- dispatch-rounded thread envelope;
- structural and film memory;
- CPU submission and GPU time;
- CFL components and substep count;
- topology generation/cache time;
- source minimum physical footprint;
- all source-family physical bounds;
- Arc/Semi-Arc connectivity and thickness;
- final-camera visual result;
- wide-river scaling;
- curvature headroom.

### 7.4 No silent fallback

If requested fixed metric dimensions exceed a limit, the runtime must not silently lower columns per chunk or enlarge `dy` while continuing to report the requested tier as satisfied.

Allowed outcomes are:

1. use strip representation if implemented;
2. explicitly resolve a lower Foam quality and report requested versus actual;
3. reject/disable Foam with an actionable diagnostic;
4. require an explicit user-authored override if such a policy is later approved.

## 8. Unit policy

### 8.1 General rule

Every affected value must be classified as one of:

- **physical metres/metres per second/seconds:** preserves physical behavior across grid resolution;
- **structural cells:** represents a discrete stencil, connectivity, neighbour count, or anti-aliasing support;
- **normalized proportion:** intentionally scales with host geometry, source progression, lifetime, or river fraction;
- **dimensionless strength/probability:** retains its semantic value but requires regression testing;
- **legacy/ambiguous:** cannot be reinterpreted until the producing and consuming code is reviewed.

### 8.2 Required migration behavior

- Existing values already named `Metres`, `MetresPerSecond`, or `Seconds` remain physically authoritative.
- Existing values named `Cells` are not automatically retained or multiplied. Each is classified according to whether it describes visible physical geometry or discrete raster support.
- Source thickness, offset, reach, trail, and feather that define visible geometry should be metre-based where practical.
- Minimum raster coverage, one-cell connectivity support, dispatch padding, and finite-difference stencil radius may remain cell-based.
- Normalized object-relative progression and weight values remain normalized.
- Serialized fields may not change meaning silently. If a field changes unit, migration/compatibility behavior and inspector wording must be explicit.

### 8.3 Arc/Semi-Arc profile decision

The current accepted profile uses local normal cell spacing. Under the current anisotropic grid, this produces orientation-dependent physical thickness. Under an approximately square metric grid, the same coefficients produce uniform but physically different thickness.

The implementation must separately determine:

- accepted physical strong-core thickness;
- accepted physical feather/outer support;
- minimum cell support required for 8-connectivity;
- whether coefficients remain cell-relative only as raster support;
- whether physical width authoring controls become authoritative.

This decision must be based on the frozen baseline and candidate screenshots, not inferred solely from current numerical coefficients.

### 8.4 Shore Ribbon decision

`FoamShoreRibbonThicknessCells` and `FoamShoreRibbonOffsetVariationCells` require explicit migration. A one-cell Ribbon is approximately five centimetres on the current demo but approximately fifteen centimetres on a `0.15 m` grid.

The plan preference is:

- visible Ribbon thickness and offset variation become metre-based or are resolved from an explicitly preserved physical baseline;
- cell-relative minimum support remains internal and non-author-facing;
- existing serialized values remain readable until a deliberate compatibility path is implemented.

## 9. Transport geometry and curvature policy

### 9.1 Straight/low-curvature transport

The metric grid corrects the current row-index mismatch. For straight or sufficiently low-curvature regions, structural cell area may be approximated by:

```text
A = dx * dy
```

Longitudinal and lateral velocities convert to cell flux using the resolved metric spacings.

### 9.2 Curvilinear metric

For centreline curvature `kappa` and signed lateral offset `n`, the offset-coordinate longitudinal scale includes:

```text
J = 1 - kappa * n
```

The current solver does not use this factor in cell area/face metrics. The migration must not claim unrestricted broad-river correctness without resolving this.

### 9.3 Required curvature diagnostic

For every allocated valid cell or conservative per-row bound, compute/report:

```text
maxAbsKappaN = max(abs(kappa * n))
minimumJacobian = min(1 - kappa*n)
maximumJacobian = max(1 - kappa*n)
```

The diagnostic must identify:

- the row/distance of maximum error;
- the lateral side;
- the implied local relative scale error;
- whether the coordinate mapping approaches folding (`J <= 0`).

### 9.4 Decision gate

Before final broad-width acceptance, select one policy:

**Policy A — bounded approximation**

- retain rectangular area/face metrics;
- define an accepted maximum `abs(kappa*n)` from the visual/numerical error budget;
- reject, warn, or require subdivision above that bound.

**Policy B — corrected curvilinear metrics**

- use Jacobian-adjusted cell areas and appropriate face lengths;
- update conservative flux and diagnostics;
- validate both sides of bends and near-limit geometry.

The current narrow-river prototype may proceed under a measured bounded approximation, but the 40-metre acceptance case may not pass merely because it allocates successfully.

### 9.5 Hard geometry stop

Any valid-water cell with non-positive or near-zero longitudinal Jacobian is a geometry/coordinate failure. The runtime must not simulate through a folded offset coordinate silently.

## 10. Contiguous one-strip representation

### 10.1 Purpose

The one-strip representation proves the final coordinate semantics with the smallest storage-system change. It supports current short/uniform-width rivers and provides a stable baseline for later strip pooling.

### 10.2 Required properties

- one descriptor represents the complete padded domain;
- columns are exact repetitions of the 32-metre quantized layout;
- lateral rows use one signed metric lattice interval;
- local bank masks invalidate unused rows per column;
- renderer, topology, obstacle, source, transport, and debug code use the same descriptor;
- cache fingerprints contain every coordinate-defining field;
- explicit diagnostics report unused rectangle percentage.

### 10.3 Known limitation

Cost remains approximately proportional to:

```text
padded total length * maximum allocated lateral extent
```

It does not scale with local active water area. A short broad region can force all narrow rows to retain the broad allocation.

### 10.4 Length limit

The current cache codec’s 8192 dimension limit creates a contiguous X limit. At roughly `0.15 m`, a 32-metre chunk uses about 214 columns, allowing 38 complete chunks or approximately 1,216 metres before exceeding 8192 columns.

The implementation must calculate and display limit headroom from actual resolved columns. It may not silently degrade spatial scale.

## 11. Future strip representation

### 11.1 Non-negotiable compatibility with Stage 1

Stage 1 is strip zero in semantic terms. Stage 2 may change resource ownership and scheduling, not coordinate meaning.

### 11.2 Strip descriptor

Each strip requires at least:

```text
StripIndex
SStart
ValidLength
AllocatedLength
ColumnCount
GlobalYBase
RowCount
ResolvedDx
ResolvedDy
LatticePhase
Previous/Next adjacency
Ghost ownership
Cache fingerprint
```

### 11.3 Shared lateral indices

Two adjacent strips may allocate different global-Y intervals, but equal global-Y values must represent equal signed lateral metres.

Example:

```text
Strip A: global Y [-20, +19]
Strip B: global Y [ -8, +11]
```

The overlap is exact and needs no lateral resampling.

### 11.4 Cross-strip transport

The strip plan must define:

- ghost columns or explicit boundary exchange;
- ownership of material at shared boundaries;
- downstream and lateral flux ordering;
- endpoint outflow versus inter-strip transfer;
- multi-substep exchange;
- topology replacement across boundaries;
- source events spanning strips;
- render sampling near strip boundaries;
- diagnostics for conservation across boundaries.

### 11.5 Resource representation decision

Evaluate, with measured evidence:

- individually pooled textures by dimension bucket;
- texture arrays with bucketed dimensions;
- atlas/indirection representation;
- other approved fixed-size pooled representation.

Do not select a representation before measuring renderer lookup complexity, dispatch count, memory waste, cache format, and Unity platform constraints.

### 11.6 Scheduling

Only Stage 2 may claim active/local-area scaling. It must define:

- visible, near, offscreen, sleeping, frozen, and unloaded states;
- update cadence by state;
- maximum active strips;
- maximum active structural cells;
- total River Foam memory cap contribution;
- deterministic wake-up and state preservation behavior.

## 12. Cache migration plan

### 12.1 Existing cache incompatibility

Normalized-lateral topology products cannot be reused under the metric mapping even when field dimensions happen to match. Old caches must miss because the coordinate contract changed.

### 12.2 Required fingerprint inputs

The generation fingerprint must include, directly or through a canonical descriptor hash:

- mapping contract version;
- quality and requested cell target;
- columns per 32 metres;
- resolved `dx` and `dy`;
- lattice phase;
- global-Y base and row count;
- field/strip start;
- allocated and valid lengths;
- column count;
- boundary/guard policy version;
- topology generator contract;
- domain, geometry, obstacle, and settings fingerprints already required.

### 12.3 Contract version policy

- generator and generation-fingerprint contracts must change;
- payload format changes only when serialized binary layout changes;
- asset storage contract changes only when the Unity asset-facing storage contract changes;
- stale/miss reasons must identify metric-contract incompatibility rather than a generic failure.

### 12.4 Tooling requirements

The existing editor preparation and build preflight must:

- detect old normalized-grid caches;
- show the requested/resolved descriptor;
- rebuild only through the approved explicit workflow;
- never rebuild or save expensive topology silently during Play;
- pass release preflight only with an exact compatible cache;
- avoid raw scene/prefab/material edits and avoid unnecessary reserialization.

### 12.5 Corruption and limits

Validate:

- maximum dimension;
- maximum cell count;
- integer overflow protections;
- payload length and checksum/validation behavior;
- descriptor mismatch;
- corrupt payload;
- partial/stale obstacle data;
- exact encode/decode round trip;
- deterministic repeated generation.

## 13. State lifecycle and topology replacement

### 13.1 Initialization

Initialization signatures must include all descriptor-defining values. A resource set may be reused only when its complete descriptor and required contracts match.

### 13.2 Reallocation triggers

At minimum, reallocation/rebuild may be triggered by:

- quality candidate/tier change;
- river length crossing a 32-metre allocation boundary;
- maximum lateral extent changing enough to alter global-Y interval;
- lattice phase or spacing change;
- domain direction/orientation change;
- cache contract change;
- future strip topology change.

### 13.3 State preservation choices

For each change class, the plan must specify:

- exact remap;
- transition blend;
- clear-and-reseed;
- reject during Play;
- editor-only rebuild.

No generic resampling may be assumed safe for Presence, life moment, and pattern moment.

### 13.4 Topology replacement mapping

Current and previous descriptors must both be available to transition code. Mapping must use physical `s/n`, not normalized field UV.

Required cases:

- width-only change with same `dx/dy`;
- row-interval expansion/contraction;
- valid-length extension/shortening;
- quality/spacing change;
- flow reversal;
- incompatible mapping-contract version.

### 13.5 Failure policy

If exact physical remap is unsupported or numerically unsafe, state must be cleared deliberately with a visible diagnostic rather than silently sampled through the old normalized mapping.

## 14. Rendering and half-resolution visual occupancy

### 14.1 Production renderer

The renderer must stop deriving Foam Y from:

```text
lateralMetres / surfaceHalfWidth
```

It must receive or derive the same descriptor mapping used by simulation.

Required render conversion:

```text
fieldX = (s - sStart) / allocatedLength
fieldY = (n - representedLateralMinimum) / representedLateralExtent
```

or an exactly equivalent global-Y-based mapping. The implementation must account for texel-centre conventions and avoid half-cell offsets.

### 14.2 Metric offsets

Visual warp, stretch, strand displacement, or other metre-based offsets converted to field UV must divide by the descriptor’s physical represented extent, not local surface width.

### 14.3 Half-resolution film

Film fields derived from structural dimensions must define exact mappings for odd dimensions:

- represented structural columns/rows per film texel;
- valid-water count or area at banks and padded edges;
- integrated source/support area;
- advection spacing;
- renderer sampling.

A film texel cannot be described unconditionally as four structural cells at odd edges.

### 14.4 Unrelated render invariants

The migration must not change:

- water colour;
- lighting;
- shadows;
- refraction;
- reflection state;
- wetness;
- riverbed response;
- corridor material role;
- non-Foam disturbance rendering.

## 15. Diagnostics and observability plan

The migration is not acceptable if developers cannot prove which coordinate contract is active.

### 15.1 Descriptor diagnostics

Expose in the existing compact River diagnostics:

- mapping contract version;
- requested `dx/dy`;
- resolved `dx/dy`;
- columns per 32 metres;
- field/strip count;
- field/strip dimensions;
- valid and allocated length;
- lattice phase;
- global-Y interval;
- represented lateral extent;
- allocated/valid/out-of-bank cells;
- rectangle waste percentage;
- film dimensions;
- cache compatibility state;
- contiguous dimension headroom.

### 15.2 Transport diagnostics

Expose:

- downstream CFL component;
- lateral CFL component;
- total CFL;
- required substeps;
- maximum/minimum `dy` only where relevant after migration;
- conservation errors for Presence, life moment, and pattern moment;
- endpoint outflow;
- maximum `abs(kappa*n)` and minimum Jacobian.

### 15.3 Source diagnostics

For each source family and event:

- physical bounding length/width;
- raster dispatch rectangle;
- affected structural cells;
- affected physical area;
- minimum/maximum Presence deposition;
- progression phase;
- source-family identity;
- clipping by bank/obstacle/padding.

Raw texel counts must not be presented as physical equivalence across resolutions without an area conversion.

### 15.4 Performance diagnostics

Expose or record:

- allocated texture and buffer bytes;
- CPU array/readback bytes;
- dispatches per commit;
- launched threads;
- logical valid cells processed;
- material commits/substeps per second;
- empty-field commits;
- visible/offscreen frames under current behavior;
- CPU command-submission time;
- GPU time from Unity profiling;
- topology/cache generation time.

### 15.5 Debug-view parity

Automatic Birth Sources production/debug kernels must be pixel-identical in coordinate and source evaluation logic except for debug output. Every field overlay must align with world geometry after the migration.

## 16. Performance and memory acceptance policy

### 16.1 Priority order

```text
active gameplay compute
> dirty/change-time runtime compute
> memory
>> storage
```

### 16.2 Narrow-demo expectation

For a nominal 32 m × 5 m strip at a 0.15 m target:

```text
columns = ceil(32/0.15) = 214
centreline-lattice rows intersecting [-2.5 m, +2.5 m] = 35
allocated texels = 214x35 = 7,490
8x8 launched envelope = 216x40 = 8,640
current Medium = 96x96 = 9,216
```

The 35-row result follows the accepted centreline-centred lattice: global Y zero is one cell centre, and both edge-intersecting cells are retained. This predicts approximately 18.7% fewer allocated texels and 6.25% fewer launched threads per 32-metre region, before accounting for valid-mask waste, resource mix, cache behavior, and GPU access patterns.

This is an analytical expectation, not a measured acceptance result.

### 16.3 CFL expectation for supplied demo settings

Using the serialized values identified in the supplied snapshot:

```text
base Foam speed = 0.7 * 0.45 = 0.315 m/s
maximum lateral speed = 0.315 * 0.777 = 0.244755 m/s
update interval = 1/12 s
```

Nominal current:

```text
dx = 0.3333, dy = 0.05104
CFLx ≈ 0.07875
CFLy ≈ 0.39960
CFLtotal ≈ 0.47835
```

Nominal 0.15 m metric:

```text
CFLx ≈ 0.17500
CFLy ≈ 0.13598
CFLtotal ≈ 0.31098
```

Both predict one substep under the current 0.90 target. Runtime values remain authoritative because actual minimum spacing, speed multipliers, liquid factor, and state can differ.

### 16.4 Wide-river scaling

A 32 m × 40 m region at 0.15 m requires approximately:

```text
214 x 267 = 57,138 allocated cells
216 x 272 = 58,752 launched threads
```

This is approximately 6.38 times the current 96 × 96 launched envelope. Maintaining fixed physical fidelity over more water area necessarily costs more.

### 16.5 Acceptance thresholds

Exact CPU/GPU/memory thresholds must be recorded from the live baseline before implementation. At minimum:

- current 4.9 m Medium candidate must not exceed the approved frame-time or memory regression;
- no unapproved additional substep;
- no hidden full-field per-frame rebuild;
- cache generation remains editor/build-time only under current policy;
- wide-river costs scale predictably with allocated area;
- Stage 1 diagnostics report rectangle waste honestly;
- Stage 2 is required before claiming active local-area scaling.

## 17. Implementation program and phase ledger

Every phase below is independently reviewable. No phase is complete merely because code was written. Each phase requires its listed evidence and rollback checkpoint.

| ID | Phase | Current status |
|---|---|---|
| `RG-METRIC-P0` | Live repository review and baseline freeze | `BLOCKED` — static snapshot review complete; live Git/Unity evidence unavailable |
| `RG-METRIC-P1` | Canonical plan reconciliation and scope lock | `COMPLETE` — documentation-only archive patch |
| `RG-METRIC-P2` | Descriptor, quality mapping, and CPU/GPU contract foundation | `ACCEPTED FOR CONTINUATION` — user confirmed Patch 02 |
| `RG-METRIC-P3` | One-strip allocation and canonical CPU field-space mapping | `ACCEPTED FOR CONTINUATION` — user confirmed Patch 03; runtime activation remains deferred |
| `RG-METRIC-P4` | Cache contracts, cache tooling, and initialization compatibility | `COMPLETE FOR STAGE 1` — legacy rejection, explicit rebuild, stored descriptor metadata, exhaustive proof, and read-only cache metadata Inspector are present |
| `RG-METRIC-P5` | Metric rows, topology, boundary, and obstacle exclusion | `COMPLETE` — P5.3 generator-4 report reproduced exactly after restart |
| `RG-METRIC-P5.1` | Determinism, obstacle provenance, and cache-diff diagnostics | `COMPLETE AS EVIDENCE PHASE` — report identified an `Input Fingerprint Gap`; retained as supplemental diagnostics |
| `RG-METRIC-P5.2` | Obstacle fingerprint repair and true legacy parity | `EVIDENCE COMPLETE` — identity repair, five-build determinism, and frozen legacy raster parity passed; reports exposed dynamic topology-phase dependence and a false CPU/GPU equality gate |
| `RG-METRIC-P5.3` | Deterministic topology evaluation phase and publication parity | `COMPLETE` — both comprehensive reports passed; current generator-4 payload exact after restart |
| `RG-METRIC-P6` | Obstacle routing, Motion Lane, and external-field integration | `COMPLETE` — corrected live-resource report passed every ledger gate |
| `RG-METRIC-P7` | Automatic/manual source migration and unit policy | `COMPLETE` — comprehensive report passed every ledger gate |
| `RG-METRIC-P8` | Persistent transport, CFL, curvature, and topology replacement | `COMPLETE — UNITY VALIDATED` |
| `RG-METRIC-P9` | Film occupancy, shape evaluation, and production rendering | `COMPLETE — UNITY VALIDATED`; P9a removed the three D3D11 warning forms and the rerun returned `Overall: PASS` |
| `RG-METRIC-P10` | Diagnostics, inspector semantics, and documentation | `UNITY-VALIDATED AND CLOSED` |
| `RG-METRIC-P11` | Mechanical verification and full consistency audit | `MECHANICALLY VERIFIED AND COMPLETE` |
| `RG-METRIC-P12` | Unity candidate sweep and visual/performance selection | `MECHANICALLY VERIFIED — UNITY EVIDENCE PENDING` |
| `RG-METRIC-P13` | Final tier tuning, cache freeze, and contiguous baseline closure | `NOT STARTED` |
| `RG-STRIP-P0` | Strip/pool architecture review and design | `FUTURE` |
| `RG-STRIP-P1` | Strip resource/cache/render implementation | `FUTURE` |
| `RG-STRIP-P2` | Cross-strip transport and scheduling validation | `FUTURE` |

### 17.1 `RG-METRIC-P0` — Live repository review and baseline freeze

**Objective:** Establish authoritative current evidence and an immutable comparison baseline before the first repository edit.

**Required work:**

1. Read all applicable agent instructions.
2. Record branch, HEAD, upstream, status, pre-existing diffs, line-ending state, and user-owned changes.
3. Read complete current versions of all files in the Stage 1 file register plus direct callers/consumers/producers.
4. Compare relevant source against `HEAD` and accepted/superseded Foam commits, especially current Arc/Semi-Arc geometry and cache performance gates.
5. Re-run reference searches for normalized lateral conversion, field dimensions, spacing derivation, quality mappings, cell-relative constants, field UV reconstruction, cache fingerprinting, and compute properties.
6. Capture the baseline evidence package defined in Section 5.1.
7. Create an immutable rollback copy/commit reference according to live repository workflow without disturbing user changes.

**No modification permitted:** code, shaders, serialized assets, generated caches, formatting, generators, or autofix tools.

**Completion evidence:** exact reviewed file list, commit/history findings, baseline screenshots/logs/metrics, pre-existing diff inventory, unresolved discrepancies.

**Stop conditions:** source snapshot assumptions differ materially; Arc/Semi-Arc baseline is not current; required files are missing; user-owned changes overlap planned files without a safe preservation strategy.

**Rollback checkpoint:** live pre-edit repository state and captured baseline.

#### `RG-METRIC-P0` execution record — supplied snapshot static review

The static portion of `RG-METRIC-P0` was executed against the exact user-supplied `Assets(71).zip` extraction. This does not satisfy the live Git or Unity baseline requirements and therefore does not release code work.

| Evidence item | Result |
|---|---|
| Supplied source | `Assets(71).zip`, extracted under the analysis workspace without substituting a Git clone |
| Applicable agent instructions found | One root `AGENTS.md`; no more-local `AGENTS.md` was present in the supplied snapshot |
| Registered Stage 1 dependency paths read | 94 of 94 |
| Missing registered paths | 0 |
| Aggregate reviewed source size | 3,804,092 bytes |
| Aggregate reviewed line count | 95,910 lines |
| Registered-file line endings | 62 LF files; 32 CRLF files; preserve each file's existing convention during later patches |
| Normalized-lateral reference scan | 22 registered files contain one or more normalized-lateral mapping indicators |
| Field-dimension reference scan | 32 registered files contain structural field-dimension indicators |
| Spacing-derivation reference scan | 20 registered files contain relevant spacing derivations or consumers |
| Cache-contract reference scan | 6 registered files contain cache contract/fingerprint/version indicators |
| Shared-quality reference scan | 18 registered files contain `StylizedRiverQuality` or Foam structural-quality indicators |
| Curvature reference scan | 12 registered files contain curvature or topology-curvature indicators |
| Git branch/HEAD/upstream/status/diff/history | Unavailable because the supplied snapshot contains no `.git` metadata |
| Immutable Git rollback reference | Unavailable for the same reason |
| Current runtime logs, profiler data, and cache status | Unavailable because no runnable project root, Library, or current `Editor.log` was supplied |
| Required source-family and debug-view screenshots | Unavailable in the supplied archive; descriptive evidence exists in the handoff, but no immutable image baseline can be produced from it |
| Source, shader, scene, prefab, material, cache, or generated-data edits | None |

**Static review conclusion:** the 94-path dependency register remains internally consistent with the supplied snapshot, and all registered paths exist. The scan reconfirms that the migration boundary extends beyond allocation into CPU field-space conversion, source dispatch, topology, obstacle systems, compute coordinate reconstruction, production rendering, cache contracts, diagnostics, quality policy, and integration tests.

**Blocking conclusion:** `RG-METRIC-P0` remains `BLOCKED` until the live workspace provides Git evidence and the required Unity/runtime baseline package. After this blocker was disclosed, the user explicitly directed continuation to the next patch. `RG-METRIC-P2` was therefore implemented only as a supplied-snapshot changed-files candidate. It is not a substitute for live-workspace reconciliation and cannot be described as Unity validated, merge-ready, or behaviorally accepted until the missing evidence is recorded.

### 17.2 `RG-METRIC-P1` — Canonical plan reconciliation and scope lock

**Objective:** Make the live canonical documents internally consistent and record the exact approved patch scope before code.

**Required work:**

- place this plan and the dependency register under approved `Assets/Docs/` paths;
- reconcile the stale active queue in `River_Foam_Active_Blockers_and_Next_Patches.md`;
- mark the current accepted Arc/Semi-Arc baseline and its Unity-validation state accurately;
- add the fixed-metric program identifier and status;
- record exact initial approved files, candidate values, non-goals, performance gates, and cache consequences;
- identify files that are review/test-only and explicitly prohibit incidental edits;
- record any live-repository deviations from this standalone plan.

**Completion evidence:** documentation-only diff, no implementation edits, every plan item status current.

**Stop conditions:** required canonical document is outside approved scope; active Arc patch state cannot be established; user has not authorized the implementation scope.

**Rollback checkpoint:** documentation commit/patch containing only plan reconciliation.

#### `RG-METRIC-P1` execution record — documentation patch 01

`RG-METRIC-P1` is complete for the supplied archive patch.

Exact documentation changes:

1. Added `Assets/Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md` and its Unity `.meta` file.
2. Added `Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md` and its Unity `.meta` file.
3. Reconciled `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md` so the active queue no longer incorrectly names `4.11C.5.18H.4`.
4. Recorded `4.11C.5.18H.6.2` as the preserved pre-migration Arc/Semi-Arc source baseline whose Unity acceptance evidence must be captured before coordinate migration.
5. Recorded the fixed-metric program as active only at the baseline/scope-lock gate. At that historical point runtime code remained unmodified pending the live portion of `RG-METRIC-P0`; P0 and all implementation phases through P9 are now closed.

Files classified as review/test-only in Appendix B remain explicitly outside incidental edit scope. No Stage 1 implementation file is authorized merely because it appears in the dependency register.

### 17.3 `RG-METRIC-P2` — Descriptor, quality mapping, and CPU/GPU contract foundation

**Objective:** Establish the single coordinate authority before changing behavior.

**Required work:**

- define the C# descriptor type or immutable set of structures;
- define initialization equality/hash/signature behavior;
- define requested versus resolved spacing;
- define 32-metre X quantization;
- define lateral phase, global-Y base, row count, represented extent, and guards;
- define shader property IDs/uniform layout or descriptor buffer;
- preserve the shared quality enum while adding Foam-specific candidate mapping;
- add CPU/GPU ABI assertions/tests;
- route public diagnostics through the descriptor without changing allocation yet where feasible.

**Invariants:** no duplicate spacing authority; no Disturbance/geometry quality changes; no scene serialization.

**Verification:** C# parser/compiler; struct stride/order; deterministic descriptor calculations; unit tests or editor assertions for length/width edge cases.

**Stop conditions:** descriptor cannot represent future strips; a new resource/kernel is required but unapproved; serialized compatibility requires an unplanned field migration.

**Rollback checkpoint:** descriptor foundation compiles but old grid remains behaviorally active.

#### `RG-METRIC-P2` execution record — descriptor foundation patch 02

Status: `CLOSED — UNITY-VALIDATED`. The text below records the historical Patch 02 delivery evidence.

The patch establishes the descriptor and ABI without enabling fixed-metric allocation:

1. Added immutable `StylizedRiverFoamGridDescriptor` and `StylizedRiverFoamGridGpuData` contracts.
2. Added explicit mapping kinds for the current `LegacyNormalizedAcross` field and future `FixedMetricLattice` field.
3. Centralized the 32-metre longitudinal chunk constant under the descriptor contract.
4. Added requested/resolved X/Y spacing, lattice phase, global-Y base, represented lateral extent, guard rows, dimensions, film dimensions, and deterministic initialization signature fields.
5. Added a deterministic fixed-metric candidate constructor with explicit dimension-limit failure instead of silent spatial degradation.
6. Added provisional Foam-only quality candidates (`0.25/0.15/0.10 m`) and retained `0.20 m` as the intermediate sweep value; these values do not alter current serialized quality behavior.
7. Represented every active legacy allocation through the descriptor while retaining the existing allocation and normalized lateral coordinate path unchanged.
8. Added five `float4` CPU/GPU descriptor lanes and a prepared C# binding method. The method is deliberately not invoked while no kernel consumes the descriptor: Unity `ComputeShader` parameter state is shared and fully unused uniforms may be stripped, so premature initialization-only binding would be unsafe and provides no validation value. No runtime parameter call, buffer, texture, kernel, dispatch, or shader-rendering consumer was added.
9. Added compact public read-only descriptor diagnostics without serialized fields.
10. Added Editor-only deterministic foundation assertions for GPU stride/offsets, centreline-lattice dimensions, global-Y zero, equality/signature, and CPU/GPU lane order.

Exact implementation files:

- `Game/Procedural/Rivers/StylizedRiverFoamGridDescriptor.cs` and `.meta`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Compute.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs`
- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Structs.hlsl`
- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl`
- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Coordinates.hlsl`

Intentional non-changes:

- active dimensions remain `chunkCount * 64/96/128` by `64/96/128`;
- lateral structural mapping remains normalized per row;
- no cache contract or fingerprint changes;
- no source, topology, obstacle, Motion Lane, transport, film, shape, or render sampling changes;
- no Disturbance or geometry quality changes;
- no scene, prefab, material, or generated-cache edit.

Mechanical evidence:

- all six changed C# implementation files parse without error under the Tree-sitter C# grammar;
- all newly introduced type references resolve within the same namespace and required `System`, `System.Diagnostics`, `System.Runtime.InteropServices`, and `UnityEngine` imports are present;
- all C# descriptor uniform names match the five HLSL declarations exactly;
- C# and HLSL lane order is `Contract`, `Spacing`, `Lateral`, `Longitudinal`, `Extent`;
- deterministic independent calculation confirms the 32 m × 5 m, 0.15 m centreline-lattice candidate resolves to 214 columns, global-Y base -17, 35 rows, and a 216 × 40 dispatch envelope;
- changed C# files contain no malformed multiline string literals and preserve LF line endings;
- changed HLSL files pass delimiter/include-order/resource-reference checks;
- no C# compiler, Unity shader importer, or Unity runtime was available at Patch 02 delivery. Subsequent Unity validation closed the phase.

Required Unity gate before `RG-METRIC-P3`:

1. Import with zero C# and compute-shader errors.
2. Confirm the active grid still reports `LegacyNormalizedAcross` and current field dimensions.
3. Confirm existing Automatic Birth Source and Final Foam views are visually unchanged.
4. Confirm no cache regeneration, serialized value reset, new dispatch, or resource allocation appears.

### 17.4 `RG-METRIC-P3` — One-strip allocation and canonical CPU field-space mapping

**Objective:** Make field dimensions and CPU coordinate conversions metric while preserving controlled initialization.

**Required work:**

- replace quality structural-resolution allocation with descriptor-driven columns/rows;
- remove/retire normalized-row field-space conversion as structural authority;
- implement cell-to-`s/n`, `s/n`-to-fractional-cell, nearest-cell, bounds, and global-Y conversions;
- build metric positions from fixed lateral centres;
- calculate valid/out-of-bank masks independently from allocation;
- update resource dimensions, film dimensions, CPU arrays, public dimensions, and reallocation signatures;
- define explicit texture/cache limit behavior;
- keep all downstream consumers temporarily blocked or migrated in lockstep so no mixed coordinate state reaches runtime.

**Verification:** exhaustive conversion round trips; asymmetric banks; padded endpoints; negative/positive Y; reversed flow; dimensions for 0.25/0.20/0.15/0.10 candidates.

**Stop conditions:** any consumer still expects normalized Y; field allocations exceed current limits without explicit failure; CPU conversion parity cannot be proven.

**Rollback checkpoint:** old-grid baseline remains preserved; metric allocation patch can be reverted independently before dependent GPU behavior is enabled.

#### `RG-METRIC-P3` execution record — one-strip/CPU mapping patch 03

Status: `CLOSED — UNITY-VALIDATED`. The prepared fixed-metric mapping remains intentionally inactive until P12.

The patch closes the safe, independently reversible part of P3:

1. The immutable descriptor is now the only source from which active runtime dimensions, film dimensions, structural dimensions, field lengths, chunk count, and columns-per-chunk are assigned.
2. Legacy allocation resolution remains available solely to preserve the confirmed runtime while dependent migration phases are incomplete; it returns a descriptor rather than mutating dimension fields directly.
3. Every initialization also resolves the exact fixed-metric one-strip candidate from the domain's maximum left/right surface reach and provisional quality cell target. There is no silent X/Y scale degradation when the candidate exceeds the hardware dimension limit.
4. Candidate state is retained as non-serialized diagnostics: availability/failure reason, dimensions, film dimensions, resolved spacing, global-Y interval, represented lateral interval, and structural-cell count.
5. The descriptor now defines exact cell-centre `s/n`, metric-to-fractional-cell, nearest-cell, containing-cell, allocated-boundary, valid-length, global-Y/local-Y, and lattice-boundary behavior.
6. The canonical CPU field-space helper now supports descriptor-based metric-position arrays, allocated-cell conversion, valid-water conversion, independent valid/out-of-bank masks, and scalar sampling through the descriptor.
7. Valid-water classification samples each fixed cell centre against the local left/right surface bank after allocation. Padded X and out-of-bank Y cells remain allocated but invalid.
8. `BuildMetricBuffer` now uses descriptor-owned X spacing and column centres. Its metric branch uses the uniform fixed `dy`; its active legacy branch preserves the former per-row lateral spacing.
9. Editor-only deterministic validation covers 0.25/0.20/0.15/0.10-metre candidates, asymmetric lateral ranges, negative/positive global Y, represented boundaries, centre round trips, nearest/containing cells, and dimension-limit rejection.
10. Fixed-metric runtime activation remains explicitly deferred. This is not a hidden feature flag and cannot be enabled through serialization. P4-P9 must migrate cache, topology/obstacle, external fields, sources, transport, film, compute, and rendering before activation.

Exact implementation files:

- `Game/Procedural/Rivers/StylizedRiverFoamGridDescriptor.cs`
- `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyFieldSpace.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Topology.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs`

Intentional non-changes:

- active mapping remains `LegacyNormalizedAcross`;
- current active field and film dimensions remain those selected by the legacy quality resolver;
- no descriptor uniform is bound;
- no HLSL, compute kernel, renderer, cache, source, obstacle, Motion Lane, topology generator, transport, film, or shape behavior changes;
- no new allocation, resource, dispatch, serialized field, scene, prefab, material, or generated asset;
- candidate resolution remains provisional and does not redefine serialized `StylizedRiverQuality` yet.

Mechanical evidence:

- all changed C# files parse without error under the available Tree-sitter C# parser;
- newly introduced `System`, `System.Collections.Generic`, Unity vector, and existing domain/sample references have matching imports/types in the supplied snapshot;
- independent numerical tests reproduce 32-metre columns of 128/160/214/320 for 0.25/0.20/0.15/0.10 metres and exact centre/boundary round trips;
- legacy active allocation remains descriptor-identical to Patch 02 for unchanged domain, quality, and hardware limit;
- fixed candidate construction is side-effect-free and does not allocate textures, arrays, buffers, or dispatches;
- line endings are preserved and no malformed multiline string literal was introduced;
- no C# compiler or Unity runtime was available at Patch 03 delivery. Subsequent Unity validation closed the phase.

Required Unity gate before `RG-METRIC-P4`:

1. Import with zero C# errors.
2. Confirm active mapping remains `LegacyNormalizedAcross` and active dimensions match Patch 02.
3. Confirm `FoamFixedMetricCandidateAvailable` is true for the demo and candidate dimensions are physically plausible for its actual domain length/width.
4. Confirm Automatic Birth Source and Final Foam views are unchanged.
5. Confirm no cache rebuild, new texture/buffer, dispatch-count change, serialization, or scene/prefab/material change.
6. Capture the candidate descriptor diagnostics for the P4 cache-contract baseline.

### 17.5 `RG-METRIC-P4` — Cache contracts, cache tooling, and initialization compatibility

**Objective:** Prevent old coordinate products from being accepted and preserve cache-only Play startup policy.

**Required work:**

- bump required generator/fingerprint contracts;
- include descriptor semantics in fingerprints;
- change payload format only if layout changes;
- update cache asset validation, runtime cache state, stale reasons, encode/decode, maximum-dimension handling, and diagnostics;
- update explicit cache preparation, development coordinator, and build preflight;
- prove old normalized caches fail for a specific contract reason;
- prove exact metric caches install without runtime generation.

**Verification:** encode/decode round trip, corruption tests, exact hit, each stale reason, release preflight pass/fail, no Play rebuild/save.

**Stop conditions:** cache tooling requires scene/prefab raw edits; cache artifacts cannot distinguish coordinate contracts; cache limit forces silent scale change.

**Rollback checkpoint:** cache contract patch with deterministic incompatibility; no production metric cache declared final yet.

#### `RG-METRIC-P4` execution record — cache-contract patch 04

**Implemented against the supplied snapshot:**

- binary payload format advanced from `2` to `3` because the payload layout now owns the complete immutable grid descriptor;
- topology generator contract advanced from `1` to `2`;
- generation and combined fingerprint contracts advanced from `1` to `2`;
- format `2` / generator `1` products are classified specifically as legacy normalized-lateral coordinate caches;
- payloads serialize descriptor contract, mapping, mapping contract, quality, requested/resolved spacing, chunk columns, lattice phase, global-Y base, row count, field/strip start, allocated/valid length, structural and film dimensions, guard rows, represented lateral extent, and initialization signature;
- deserialization reconstructs the descriptor only after validating enum values, exact mapping contract, positive/finite values, 32-metre chunk spacing, structural/film dimensions, global-Y range, lateral extent, and signature;
- generation fingerprints hash every descriptor field and signature, preventing coordinate products from sharing a stable key across mappings or dimensions;
- cache asset metadata records descriptor identity and must agree exactly with the validated payload;
- release validation, explicit validation, startup resolution, and installation reject unsupported, legacy, or descriptor-mismatched cache products before use;
- contiguous cache limits are checked directly against the descriptor (`8192` per axis and `16,777,216` cells) without invoking a lower physical resolution;
- explicit Editor preparation, development persistence, and release preflight expose the descriptor/mapping identity;
- active runtime mapping remains `LegacyNormalizedAcross`; no fixed-metric cache is declared a production result.

**Mechanical verification performed:**

- every changed C# file parsed with the available C# grammar;
- cache writer/reader descriptor field order and type widths were audited lane by lane;
- descriptor reconstruction was tested independently for legacy and fixed-metric examples, including each derived invariant and signature failure;
- current (`3/2`), legacy (`2/1`), and unsupported contract classification was tested;
- every descriptor field was confirmed to participate in the generation fingerprint;
- cache startup/validation/install call paths were audited for contract-first failure ordering;
- no Play Mode generation/save path, compute kernel, HLSL file, runtime allocation, source, transport, film, render, scene, prefab, material, or cache asset was modified.

**Observed P4 Unity evidence and deferred remainder:**

1. the real format-2/generator-1 asset produced `PreparationRequired / CoordinateContract` with one miss, zero installs, zero builds, zero replacements, and zero writes;
2. explicit Edit Mode preparation stored a format-3/generator-2 descriptor-owned payload of `1,954,946` bytes with hash `E9DB3347A43E97DD` and descriptor identity `descriptor-v1/mapping-0-v0/768212E451E606B9`;
3. the built-in exact encode/decode/corruption proof passed with the identical byte count and hash;
4. Unity Debug Inspector did not reveal hidden cache metadata, but the later `StylizedRiverFoamTopologyCacheAssetEditor` now provides the required read-only contract metadata and explicit payload-section analysis;
5. final exact post-rebuild Play startup, restart persistence, and release-preflight evidence were not supplied before the user authorized P5 continuation;
6. those remaining P4 checks are still required for final program closure but are not represented as completed.

**Known limitation:** Exact fixed-metric cache installation is structurally supported by the descriptor-aware package, but cannot be executed honestly while `FixedMetricLattice` remains inactive. Runtime proof is deferred to the coordinated activation phase; this patch proves deterministic compatibility and rejection mechanics without mixing coordinate systems.

### 17.6 `RG-METRIC-P5` — Metric rows, topology, boundary, and obstacle exclusion

**Objective:** Migrate generated physical support and occupancy products to fixed `s/n` cells.

**Required work:**

- populate metric rows with canonical spacing and row geometry;
- migrate major, connector, and pocket topology generators;
- classify every topology morphology radius/width/budget as metres, cells, or normalized;
- migrate boundary and shore-edge generation;
- migrate exact-mesh obstacle projection and compact intervals;
- preserve obstacle registry/version/fingerprint behavior;
- validate cache capture/readback parity.

**Verification:** straight/widening/narrowing/asymmetric/bent domains; no out-of-bank topology; obstacle silhouettes; small obstacles; bank-touching obstacles; deterministic generation; physical support dimensions.

**Stop conditions:** topology cannot preserve accepted physical shapes without a material tuning decision; obstacle occupancy differs between CPU and GPU; generated cache parity fails.

**Rollback checkpoint:** metric topology/cache baseline independent of source and final rendering tuning.

#### `RG-METRIC-P5` pre-implementation review and scope lock — patch 05

Historical scope-lock state: `MECHANICALLY VERIFIED`. P5 later closed through the Unity-validated P5.3 generator-4 result; the pre-edit scope record remains below.

**Observed P4 Unity evidence accepted for continuation:**

- Play startup classified the assigned old cache as `PreparationRequired / CoordinateContract`; the Layer A Inspector displayed `Miss — Legacy Coordinate Contract`.
- The startup summary recorded `attempt:1/hit:0/miss:1/install:0/build:0`, zero obstacle/Major/Connector/Pocket builds, zero replacement attempts, and zero writes.
- Explicit Edit Mode preparation stored a `1,954,946`-byte payload with hash `E9DB3347A43E97DD` and descriptor identity `descriptor-v1/mapping-0-v0/768212E451E606B9`.
- The exhaustive cache proof passed with the identical byte count and hash.
- The cache ScriptableObject fields remain intentionally hidden from default serialization UI; `StylizedRiverFoamTopologyCacheAssetEditor` now presents the required read-only metadata and explicit payload-section analysis.
- The negative legacy-cache Play test correctly allocated no completed Foam resources, so a selected Foam debug view had no source texture to display. The later rebuilt-current-cache and P5.3 reports supplied the exact-load proof and closed the phase.

**Read-only source review performed against the supplied Patch 04 snapshot:**

- canonical documents: this plan, `River_Foam_Fixed_Metric_Dependency_Register.md`, `River_Foam_Active_Blockers_and_Next_Patches.md`, and the Layer A/cache/coordinate sections of `River_Foam_Stage6_Architecture.md`;
- coordinate and descriptor contracts: `StylizedRiverFoamGridDescriptor.cs`, `FoamTopology/StylizedRiverFoamTopologyFieldSpace.cs`, `StylizedRiverFoamRuntime.State.cs`, `CS_RiverFoam.Structs.hlsl`, `CS_RiverFoam.Coordinates.hlsl`, and `CS_RiverFoam.Resources.hlsl`;
- metric-row/topology ownership: `StylizedRiverFoamRuntime.Topology.cs`, `StylizedRiverFoamRuntime.Compute.cs`, `StylizedRiverFoamRuntime.Resources.cs`, `StylizedRiverFoamRuntime.TopologyCache.cs`, `CS_RiverFoam.Topology.hlsl`, `CS_RiverFoam.TopologyTransition.hlsl`, and all P5-owned kernels in `CS_RiverFoam.compute`;
- CPU topology products: `StylizedRiverFoamMajorTopologyGenerator.cs`, `StylizedRiverFoamConnectorTopologyGenerator.cs`, `StylizedRiverFoamPocketTopologyGenerator.cs`, their topology/result contracts, candidate generator, and evolution consumers;
- boundary and exact obstacle ownership: `StylizedRiverFoamRuntime.Obstacles.cs`, `RiverObstacleExclusionResolver.cs`, obstacle registry/fingerprint call paths, `StylizedRiverFoamRuntime.Injection.cs` object-contact consumer, and cache capture/readback paths;
- integration/test-only inputs: `RiverDomainSnapshot.cs`, `StylizedRiverGeometry.cs`, `StylizedRiverCorridorGeometry.cs`, disturbance obstacle registry APIs, and the demo scene as read-only evidence.

The supplied archive has no `.git` metadata or runnable Unity project root. Live branch/HEAD/diff/history comparison remains unavailable and must not be represented as complete. Patch 05 is therefore prepared as a changed-files-only continuation against the user-confirmed Patch 04 snapshot.

**Patch 05 implementation decisions:**

1. The active runtime mapping remains `LegacyNormalizedAcross`. P5 prepares complete descriptor-aware topology/boundary/obstacle behavior but does not permit a mixed metric simulation before P6-P9.
2. Every CPU topology generator receives the immutable grid descriptor rather than independent width/height/field-length arguments. Legacy coordinate evaluation must remain numerically identical; fixed-metric evaluation uses descriptor cell centres and metric-to-cell conversion.
3. Major placement size, connector radii/path lengths, pocket/free-water radii, prepared-path sampling, and obstacle geometry remain physical metres. Existing normalized lateral placement controls remain normalized authoring coordinates and are converted to metres at the sampled river row.
4. The static valid-water boundary feather is classified as a physical shoreline transition. Legacy mode keeps the former quality-cell expression exactly. Fixed-metric mode uses the derived pre-migration narrow-river baseline of `0.10 m`, bounded by the existing `0.05 m` minimum, pending P12 visual selection. This avoids tripling the demo transition merely because fixed `dy` is larger than the old normalized-row spacing.
5. Major upstream-obstacle lateral reach is classified as physical. Legacy mode preserves `max(2, round(height * 0.08))`; fixed mode resolves the same nominal pre-migration reach (`0.40 m`) through `resolvedDy`.
6. Exact-mesh obstacle samples own complete physical cell rectangles. Legacy cells retain the former normalized-row rectangle. Fixed cells sample descriptor X boundaries and global-Y-centred lateral boundaries.
7. P5 compute kernels bind the descriptor at their immediate topology dispatch boundary. No descriptor is bound globally or once at initialization because `ComputeShader` parameter state is shared. P6-P9 must bind the descriptor for their own consuming dispatches.
8. Disturbance texture sampling remains normalized and is not corrected in P5; that is explicitly owned by P6. Source rasterization, transport, topology replacement, film, shape, and production rendering remain unchanged.
9. No cache contract bump is required if legacy generated bytes remain unchanged. The existing P4 descriptor fingerprint already distinguishes future fixed-metric products. Any discovered legacy-output difference is a stop condition requiring plan revision and a generator-contract decision.
10. The topology-morphology audit classifies resolution-dependent quantities as follows before final P5 code closure:
    - Major candidate-mask `*Cells` values remain candidate-local raster coordinates, not Foam field cells. They continue to acquire physical size only through `metresPerCandidateCell`.
    - Four/eight-neighbour graph traversal, connectedness, boundary detection, and one-cell conservative raster guards remain structural-cell operations. Their purpose is discrete topology/sampling support, not authored physical width.
    - Opportunity counts, host limits, variant attempts, endpoint sectors, prepared-path point capacities, recycle-anchor counts, and relationship capacities remain semantic bounded counts independent of field resolution.
    - Coverage and host-remainder fractions remain ratios. On a fixed lattice every valid cell owns the same `resolvedDx × resolvedDy` area, so those ratios are physical-area ratios without additional scaling.
    - Major minimum newly covered support and its ranking contribution become fixed-mode physical-area equivalents while the legacy path retains the exact former integer-cell calculation. The recorded reference area is `0.02 m²` per legacy-equivalent coverage unit and the former four-cell rejection therefore represents `0.08 m²`.
    - Connector minimum-component rejection becomes `0.10 m²` in fixed mode while the legacy path retains exactly five cells. Component-to-region identity fallback becomes a `1.50 m` metric search in fixed mode while the legacy path retains the former ten-cell ring search.
    - Free-water nearest-valid-cell fallback becomes a metric-radius-covered X/Y search in fixed mode using the existing `0.34 m` acceptance radius, while legacy mode retains the former `2 × 2` cell-radius scan.
    - All existing values already named in metres remain metres. Existing normalized lateral authoring coordinates remain normalized authoring data and are converted at the sampled row.

**Approved Patch 05 implementation files:**

- `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyFieldSpace.cs`
- `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamMajorTopologyGenerator.cs`
- `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamConnectorTopologyGenerator.cs`
- `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamPocketTopologyGenerator.cs`
- `Game/Procedural/Rivers/RiverObstacleExclusionResolver.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Topology.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Obstacles.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Compute.cs` only if immediate descriptor binding must be reused by P5 dispatch ownership
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs` only if the P5-owned object-contact dispatch consumes descriptor coordinates
- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Coordinates.hlsl`
- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Structs.hlsl` for contract comments only if required
- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`
- the three governing River Foam Markdown documents.

**Review/test-only files prohibited from incidental modification:**

- `RiverDomainSnapshot.cs`;
- `StylizedRiverGeometry.cs`;
- `StylizedRiverCorridorGeometry.cs`;
- all Disturbance implementation files;
- source-event preparation/raster code outside the object-contact field;
- transport, topology replacement, film, shape, production render, scene, prefab, material, and generated cache assets.

**Patch 05 acceptance evidence required before delivery:**

- changed C# files parse with the strongest available parser and all introduced references/imports are audited;
- C#/HLSL descriptor and cell-centre formulas are line-by-line consistent;
- legacy topology/boundary/obstacle calculations remain unchanged under `LegacyNormalizedAcross`;
- fixed-metric straight, widening, narrowing, asymmetric, bent, padded-end, reverse-flow, small-obstacle, and bank-touching mathematical cases pass deterministic assertions;
- exact obstacle candidate bounds and 3x3 sample positions stay inside their owning physical cell;
- all 22 compute kernels remain present and no texture/buffer/kernel/dispatch is added;
- cache payload layout and contracts remain unchanged;
- final diff contains only recorded files and no serialized asset.

#### `RG-METRIC-P5` implementation result — Patch 05

Status: `CLOSED — UNITY-VALIDATED THROUGH P5.3`.

**Implemented coordinate ownership:**

1. `StylizedRiverFoamTopologyFieldSpace` now owns descriptor-aware longitudinal column centres, metric lateral row centres, metre-to-nearest/containing/ceiling cell conversion, valid-water classification, fixed-lattice scalar sampling, and the fixed-metric shoreline-feather policy.
2. Legacy normalized-row calls route through the original formulas. Fixed-metric calls use descriptor `s/n` centres and global-Y lattice indices. Bulk metric-position generation samples the river domain once per X column in legacy mode rather than once per cell, preserving the former preparation-cost shape.
3. Major, Connector, and Pocket topology runtimes receive the immutable descriptor directly. Their public pre-descriptor overloads remain as compatibility wrappers for existing topology-replacement callers and construct an equivalent legacy descriptor.
4. Major candidate rasterization, recycle-anchor validation, Connector component identity/path validation, Pocket/free-water cell resolution, hosted/free-water mask resampling, and prepared recycle validation now use descriptor-aware metric positions and metre-to-cell lookup.
5. The completed topology-morphology audit keeps candidate-local raster coordinates, graph adjacency, conservative one-cell guards, bounded semantic counts, and dimensionless ratios in their original units. Fixed mode converts only quantities that represented physical acceptance/search despite being encoded as field-cell counts: Major minimum new coverage/ranking use `0.02 m²` equivalent units with a `0.08 m²` minimum; Connector minimum components use `0.10 m²`; Connector source-region identity fallback uses `1.50 m`; and Free Water nearest-valid fallback derives X/Y scan radii from its existing `0.34 m` acceptance radius. Every corresponding legacy branch retains the exact former cell-count/ring behavior.
6. The valid-water boundary and CPU topology fluid context share one shoreline feather resolver. Legacy mode reproduces the former quality-cell formula; fixed mode uses the recorded provisional `0.10 m` physical feather with the existing `0.05 m` floor.
7. Major upstream-obstacle context preserves the legacy `max(2, round(height × 0.08))` window and resolves a fixed-metric `0.40 m` reach through `resolvedDy` for the future lattice.
8. Exact obstacle preparation receives the descriptor, uses descriptor X bounds, derives fixed-lattice candidate Y intervals from global-Y indices, and places all nine conservative samples inside the owning physical cell rectangle. Samples outside the current physical banks are rejected.
9. Runtime metric rows use descriptor `resolvedDx` and, for the future fixed mapping, descriptor `resolvedDy`. The runtime boundary and exact-obstacle bake now consume the same descriptor-owned CPU mapping as generated topology.
10. `ConfigureTopologyParameters` binds the five-lane descriptor immediately before topology-owned dispatch sequences. The binding is not moved to initialization and is not treated as persistent global `ComputeShader` state.
11. Dedicated P5 HLSL helpers resolve descriptor-aware topology distance/domain membership and lateral metres. Existing source, transport, film, and other staged legacy helpers remain unchanged until their owning phases. `BuildCurrentShoreEdges`, `BuildEvolvingMajorSupport`, `CaptureGeneratedTopology`, `ComposeTopology`, and `MeasureTopologyMetrics` use the P5 descriptor-aware path.

**Exact changed implementation files:**

- `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyFieldSpace.cs`
- `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamMajorTopologyGenerator.cs`
- `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamConnectorTopologyGenerator.cs`
- `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamPocketTopologyGenerator.cs`
- `Game/Procedural/Rivers/RiverObstacleExclusionResolver.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Topology.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Obstacles.cs`
- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Coordinates.hlsl`
- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Structs.hlsl`
- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`
- this plan, the dependency register, and the active-blocker document.

**Intentionally unchanged:**

- active mapping and active `192 × 96` legacy allocation;
- cache payload format `3`, generator contract `2`, fingerprints, serialized descriptor layout, and generated cache assets;
- obstacle registry/version/fingerprint ownership;
- routing, Motion Lane, Disturbance-field sampling, automatic/manual source rasterization, persistent transport, topology replacement semantics, film, shape, production rendering, scenes, prefabs, materials, and serialized `StylizedRiver` fields;
- compute kernel count, texture/buffer inventory, and dispatch inventory.

**Mechanical evidence:**

- all seven changed C# files parsed with zero syntax-error or missing nodes using the available C# grammar parser;
- project-wide static class/method call analysis found zero unresolved changed-signature call sites across `3,259` discovered definitions and `18,737` calls;
- newly introduced descriptor references were scanned for scope and namespace ownership; no malformed multiline C# string literal was introduced;
- `100,000` randomized legacy formula cases produced zero mismatches for X centres, nearest/containing/ceiling X conversion, asymmetric lateral centres, shoreline feather, and legacy obstacle sample UVs;
- `10,279,238` fixed-lattice numerical/morphology assertions produced zero failures across `0.25/0.20/0.15/0.10 m` candidates, 5/10/20/40-metre widths, CPU/HLSL centre parity, physical obstacle-cell ownership, straight/widening/narrowing/asymmetric/bent-width profiles, area-equivalent Major/Connector thresholds, metric Connector identity search bounds, and Free Water nearest-search coverage;
- changed HLSL/compute files have balanced delimiters and preprocessor blocks, no duplicate function definitions, unchanged include ordering, and all `22` compute kernels remain present;
- no cache codec, cache asset, fingerprint, resource declaration, serialized asset, scene, prefab, or material file changed.

**Unity evidence and blocker:** Patch 05 compiled and executed, but legacy byte parity did not reproduce. The first explicit Patch 05 rebuild produced `1,954,946` bytes with hash `58C8036175508509`; a second explicit rebuild produced `1,954,518` bytes with hash `24BCF968B2B94F28`. Both builds reported the same complete combined input fingerprint `F182CD9FCC93A961B19B60CBD53C5639`, the same descriptor `descriptor-v1/mapping-0-v0/768212E451E606B9`, and five obstacle sources. The second payload was 428 bytes smaller despite the unchanged input key. The exhaustive proof passed for each individual immutable payload, which proves serialization round-trip integrity but does not prove deterministic regeneration. P6 is blocked until the exact changing section and source are identified and corrected or explicitly accepted.

### 17.7 `RG-METRIC-P5.1` — Determinism, obstacle provenance, and cache-diff diagnostics

**Objective:** Convert opaque whole-payload drift into exhaustive, reproducible evidence. The patch must identify whether drift originates in obstacle-source geometry/provenance, input capture, topology generation, collection ordering, prepared path/identity catalogues, scalar fields, or serialization. It must not alter topology generation, cache storage semantics, active mapping, source behavior, transport, rendering, or any serialized scene/prefab/material setting.

**Observed evidence requiring this phase:**

1. The previously accepted Patch 04 cache validated after Patch 05 as `Stale Obstacles`; the source count remained five.
2. The first Patch 05 rebuild produced `1,954,946` bytes / `58C8036175508509`.
3. The immediate second rebuild produced `1,954,518` bytes / `24BCF968B2B94F28`.
4. Both rebuilds reported combined input fingerprint `F182CD9FCC93A961B19B60CBD53C5639`, proving that the current aggregate fingerprint contract did not distinguish the generated payloads.
5. Both exhaustive round-trip proofs passed, proving each captured package is internally deterministic after capture while leaving cross-generation determinism unresolved.

**Approved implementation scope:**

Exact project file scope:

- modify `Assets/Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`;
- modify `Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md`;
- modify `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`;
- modify `Assets/Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyCacheAsset.cs`;
- modify `Assets/Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyCacheCodec.cs` only to permit the diagnostic partial;
- modify `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`;
- create `Assets/Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamCacheDiagnostics.cs`;
- create `Assets/Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.CacheDiagnostics.cs`;
- create `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.CacheDiagnostics.cs`;
- create `Assets/Game/Procedural/Rivers/Editor/StylizedRiverFoamTopologyCacheAssetEditor.cs`;
- create the four matching `.meta` files.

Explicit user-triggered non-asset outputs:

- `Library/RiverFoamDiagnostics/<river>_LatestCacheAudit.txt`;
- `Library/RiverFoamDiagnostics/<river>_ObstacleBaseline.bin`;
- `Library/RiverFoamDiagnostics/<river>_ObstacleBaseline.txt`;
- `Library/RiverFoamDiagnostics/<river>_LatestObstacleComparison.txt`.

- add a detailed source-provenance snapshot for every exact obstacle source;
- record stable source key, source/owner/MeshFilter entity IDs, registry enumeration order, unique-MeshFilter inclusion ownership, duplicate registrations, hierarchy path, owner/provider types, mesh identity, readable-state, vertex/index counts, exact local/world bounds, exact local-mesh fingerprint, exact transform fingerprint, provider-prepared world fingerprint, independently recomputed world fingerprint, provider/direct agreement, and build/captured-obstacle agreement;
- capture and compare obstacle source sets twice back-to-back and against an explicit baseline stored under `Library/RiverFoamDiagnostics`;
- add full-payload and payload-section snapshots with byte count, stable hash, exact first-difference offset, byte neighbourhoods, and topology counts/rejection summaries for grid/domain/settings, obstacle input key, obstacle scalar field, Major support/regions/prepared regions, Connector support/relationships/prepared paths/catalogue paths, and Pocket scalar fields/regions/prepared hosted/free-water/weak-span records;
- add one explicit full audit that performs two independent Edit Mode preparations without storing either result, verifies input fingerprints before comparing generated sections, compares the assigned asset when compatible, writes one latest report under `Library/RiverFoamDiagnostics`, and logs the complete report;
- add Inspector actions to run the full audit, capture an obstacle baseline, compare against the baseline, copy the latest report, log it again, and reveal the report file;
- add a read-only custom Inspector for `StylizedRiverFoamTopologyCacheAsset` exposing all cache-contract metadata plus section-digest information without exposing or editing the raw payload byte array;
- update the canonical plan, dependency register, and active queue;
- establish the permanent rule that any patch changing a deterministic generation/cache/coordinate contract must ship its owning diagnostics and parity test in the same patch rather than deferring validation observability.

**Diagnostic execution policy:**

- all expensive work is explicit Editor-button work only;
- no per-frame, startup, automatic import, `InitializeOnLoad`, scene-save, prefab-save, asset-mutation, or Play Mode generation path is added;
- diagnostic reports are written only below `Library/RiverFoamDiagnostics`;
- the full audit may release and recreate temporary Foam resources exactly as explicit cache preparation already does, but it must not call `StoreBuild`, `AssetDatabase.SaveAssets`, or mutate the assigned cache asset;
- source-baseline capture writes a diagnostic baseline file only and never changes generated geometry;
- report strings are retained in non-serialized runtime memory for Inspector copy/log actions.

**Required report verdicts:**

- obstacle source set stable/changed;
- provider fingerprint agrees/disagrees with direct exact-world recomputation per source;
- build A/B domain, obstacle, generation, and combined inputs equal/unequal;
- build A/B descriptor equal/unequal;
- build A/B payload equal/unequal;
- first changed payload section and first byte offset;
- all changed section hashes/counts;
- assigned asset equal to build A, build B, both, or neither;
- final classification: `Exact`, `Obstacle Input Drift`, `Input Fingerprint Gap`, `Topology Generation Drift`, `Serialization Drift`, or `Audit Inconclusive`.

**Verification:**

1. C# compilation and Editor import produce zero errors.
2. The cache asset Inspector visibly reports format `3`, generator `2`, descriptor contract `1`, mapping `Legacy Normalized Across`, mapping contract `0`, signature, fingerprints, payload size/hash, and build time.
3. Capturing and immediately comparing an obstacle baseline identifies every source and reports zero changes when no generated geometry changed.
4. The full audit produces two build records, complete source records, all section digests, first-difference evidence, a saved latest report, Console output, and copyable Inspector text.
5. The audit does not modify the assigned cache, scene, prefab, material, generated geometry, or serialized River settings.
6. At this historical P5.1 gate, P6 remained blocked until the report identified the changing section. P5.2/P5.3 later restored deterministic same-input generation and released P6.

**Stop conditions:** any diagnostic requires automatic runtime work; exact source provenance cannot be established; the audit mutates persistent project assets; section hashing cannot distinguish the observed 428-byte payload-size change; report evidence is truncated or unavailable to copy.

**Rollback checkpoint:** Patch 05 code and the currently assigned cache remain untouched; P5.1 consists only of explicit diagnostics/editor presentation and canonical documentation.

#### `RG-METRIC-P5.1` implementation record — diagnostic patch 05.1

Status: `CLOSED — UNITY-VALIDATED THROUGH P5.3`. The diagnostic evidence below identified the drift source and released P6.

Implemented behavior:

1. Added one explicit two-build determinism audit. It captures obstacle provenance before Build A, between Build A and Build B, and after Build B; performs two independent existing Edit Mode cache preparations; does not call `StoreBuild`, `AssetDatabase.SaveAssets`, or any persistent cache-write API; records complete build artifacts; compares raw payload bytes and twenty structured diagnostic sections; compares the assigned asset before/after; writes one exhaustive report; and logs the same report.
2. Added exact per-registration obstacle evidence: hierarchy/component-stable diagnostic key, session EntityIds retained as provenance, registry enumeration order, unique-MeshFilter inclusion ownership, provider/owner types, mesh identity, readability, exact vertex/index counts, exact local/world bounds including IEEE-754 float bits, local-mesh fingerprint, local-to-world transform fingerprint, provider-prepared exact-world fingerprint, independently recomputed exact-world fingerprint, and provider/direct agreement.
3. Separated production-relevant obstacle-input equality from provenance-only metadata. Session-local EntityIds, registry order, and status text remain visible but do not by themselves classify unchanged geometry as obstacle drift. Duplicate registrations remain visible, and first-seen unique-MeshFilter ownership remains a production-relevant comparison.
4. Added explicit obstacle baseline capture and comparison below `Library/RiverFoamDiagnostics`. Baseline capture writes both machine-readable and human-readable records, immediately reads the binary record back, and refuses success unless production-relevant source data survives the round trip exactly.
5. Added payload-section diagnostics for grid descriptor, domain, input fingerprints, generation settings, obstacle scalar field, Major support/regions/prepared/complete, Connector support/relationships/prepared/catalogue/complete, and Pocket scalar/regions/hosted/free-water/weak-span/complete data. Every section records a byte count, stable FNV-1a 64-bit digest, exact first-difference offset, byte neighbourhood, and semantic count/rejection summary.
6. Added audit classifications: `Exact`, `Obstacle Input Drift`, `Input Fingerprint Gap`, `Topology Generation Drift`, `Serialization Drift`, and `Audit Inconclusive`. The audit also verifies that each build's stored obstacle fingerprint equals the exact post-build source capture and that the assigned cache metadata and payload remain byte-identical throughout the audit.
7. Added River Inspector buttons that are now retained under `Actions → Foam Cache & Validation → Historical / Deep Diagnostics`: the P5.1 two-build audit, obstacle baseline capture/comparison, and the shared copy/log/reveal report controls. Expensive actions remain disabled in Play Mode.
8. Added a normal read-only custom Inspector for `StylizedRiverFoamTopologyCacheAsset`, correcting the P4 validation-UX omission. It exposes storage/payload/generator/grid contracts, mapping name/value, signature, source/build metadata, payload size/hash, and all stable input fingerprints. Payload section decoding occurs only when `Analyze Payload Sections` is pressed; raw payload bytes remain hidden and uneditable.
9. Added the permanent same-patch diagnostics rule to the canonical plan, dependency register, and active queue. Deterministic generator, coordinate, cache, source-contract, or state-migration patches must carry their own owning evidence surface and parity test rather than deferring observability.

Exact changed project files:

- `Assets/Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
- `Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
- `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
- `Assets/Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyCacheAsset.cs`
- `Assets/Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyCacheCodec.cs`
- `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`
- `Assets/Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamCacheDiagnostics.cs` and `.meta`
- `Assets/Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.CacheDiagnostics.cs` and `.meta`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.CacheDiagnostics.cs` and `.meta`
- `Assets/Game/Procedural/Rivers/Editor/StylizedRiverFoamTopologyCacheAssetEditor.cs` and `.meta`

Intentional non-changes:

- cache payload format remains `3`; generator contract remains `2`; no serialized cache field or payload layout changed;
- the codec's only production-file change is `static class` to `static partial class` so the Editor-only diagnostic partial can reuse the exact private serialization contract;
- active mapping, active field dimensions, topology algorithms, obstacle rasterization, routing, Motion Lane, sources, transport, film, shape, rendering, Disturbance allocation, compute/HLSL, resources, kernels, dispatches, and quality values remain unchanged;
- no scene, prefab, material, generated cache asset, `.asset`, or serialized `StylizedRiver` field is edited by the patch;
- no `InitializeOnLoad`, import callback, startup audit, automatic Play Mode generation, per-frame diagnostic, or hidden project-asset write was introduced.

Mechanical verification evidence:

- all seven changed/new C# source files parse with zero syntax-error or missing nodes under the available Tree-sitter C# grammar;
- both `UNITY_EDITOR`-enabled and disabled preprocessed forms parse with zero syntax errors; all preprocessor blocks balance;
- the existing cache asset's fifteen serialized fields are byte-for-byte declaration-identical to Patch 05;
- the existing codec diff is exactly one declaration change: `internal static class` to `internal static partial class`;
- twenty diagnostic sections are present with unique ordered names;
- the obstacle baseline writer and reader contain the same ordered 26-lane source contract, plus version, timestamp, aggregate fingerprint, status, and count framing;
- all introduced project-specific types have declarations in the supplied source and all new public/internal diagnostic methods have matching call sites;
- a namespace/import scan found and removed a potential `System.Object`/`UnityEngine.Object` ambiguity from the River Inspector file;
- all four new Unity GUIDs are unique across 315 supplied `.meta` files;
- exact diff scope is fourteen approved files; no HLSL, compute, scene, prefab, material, cache asset, or unrelated subsystem file differs;
- existing LF line endings are preserved and changed Markdown files decode as UTF-8 with balanced code fences;
- no C# compiler or Unity Editor was available at P5.1 delivery. The later P5.2/P5.3 Unity reports closed the diagnostic sequence.

Required Unity evidence before any corrective diagnosis or P6 work:

1. Import with zero C# errors and verify the cache asset's normal Inspector displays format `3`, generator `2`, mapping `0 — Legacy Normalized Across`, descriptor signature, payload identity, and fingerprints.
2. Capture an obstacle baseline and immediately compare it. The binary round-trip must pass; production-relevant obstacle input should be `EXACT`. Provenance-only differences may be reported separately and must be retained in the evidence.
3. Run the full audit once. It intentionally performs two cache preparations and may report `Failed`; a failed verdict is diagnostically valid when it identifies the first changed source/section/byte. The assigned cache mutation check must pass.
4. Copy or provide `Library/RiverFoamDiagnostics/River_Strip_LatestCacheAudit.txt` in full. Do not rebuild/store the assigned cache between applying P5.1 and running the audit unless a separate comparison is explicitly required.
5. Confirm version control contains no audit-induced scene, prefab, material, generated cache, or serialized River change.

### 17.7A `RG-METRIC-P5.2` — Obstacle fingerprint repair and true legacy parity

**Objective:** Repair the proven exact-geometry identity defect before accepting P5, make every unreliable cache contract explicitly obsolete, and prove in one exhaustive report that the descriptor-owned legacy obstacle path is bit-exact to the frozen pre-P5 path for the same current mesh inputs.

#### Accepted P5.1 evidence

The user supplied the complete baseline, comparison, and determinism reports. They establish the following facts:

1. all five registered obstacle sources remained exact across immediate baseline comparison and the complete audit transaction;
2. two current non-storing preparations were byte-for-byte exact at `1,954,518` bytes / `E1483A3A22B304FC`, so current same-session generation and serialization are deterministic;
3. the assigned cache differed first in the obstacle scalar field by one occupied texel (`587` versus `588`), while the substantive Major, Connector, and Pocket products were otherwise stable;
4. every `GeneratedMass` provider reported success with `00000000000000000000000000000000` while direct exact-world-triangle hashing produced a distinct nonzero fingerprint for each source;
5. the combined obstacle fingerprint therefore did not identify the source geometry and could remain unchanged while generated obstacle output differed;
6. the correct classification is `Input Fingerprint Gap`, not same-session topology randomness.

#### Root-cause contract

`GeneratedMass` owned four coupled transient cache lanes:

```text
valid flag
128-bit readonly fingerprint
Mesh reference
local-to-world matrix
```

The four fields were not explicitly excluded from Unity hot-reload serialization. The observed state—successful provider result, unchanged mesh/matrix ownership, and a default readonly struct value—is consistent with a restored validity lane whose fingerprint lane was restored as default. P5.2 treats this as a high-confidence root cause while also defending every downstream boundary so the same invalid state cannot be accepted even if another provider later reproduces it.

Required invariants:

- the all-zero 128-bit value is a reserved invalid sentinel;
- no exact-geometry utility or provider may report success with the sentinel;
- all four Generated Mass cache lanes are `[NonSerialized]`;
- a detected valid-plus-zero state forces recomputation;
- refreshed values are resolved into locals and published only after all lanes are complete;
- explicit Edit Mode cache validation/preparation independently recomputes direct world-triangle identity and requires provider/direct equality;
- normal Play Mode startup does not add triangle rescans and continues consuming prepared provider identities;
- the obstacle-set aggregate includes an explicit contract value `2` and rejects every zero source;
- cache generator contract advances from `2` to `3` while payload format remains `3`;
- format-3/generator-2 assets are classified specifically as `Legacy Obstacle Fingerprint Contract` and may not install, validate as current, or rebuild automatically in Play Mode.

#### True legacy obstacle parity

P5.2 adds an Editor-only partial containing a frozen copy of the pre-P5 normalized-lateral obstacle candidate/raster path. It executes only from the explicit comprehensive report action. For each unique source, the reference and P5 descriptor path receive the same:

- river/domain object;
- exact MeshFilter and transformed triangle data;
- `192 × 96` or current active legacy dimensions;
- allocated field length;
- sample offsets and solid-interval constants.

The audit compares:

- candidate availability and X bounds;
- exact projected lateral extrema bits;
- emitted cell count, order, coordinates, and interval offsets;
- every accepted sample's interval and water-parameter float bits;
- the complete CPU occupancy scalar;
- duplicate-cell counts;
- first mismatch, including source, cell, sample coordinate, reconstructed base point, and Up vector.

The mandatory result while the active mapping is `LegacyNormalizedAcross` is:

```text
Legacy obstacle parity: EXACT
cell mismatches = 0
accepted-sample mismatches = 0
CPU scalar mismatches = 0
```

The frozen path is diagnostic-only and is excluded from player compilation by `UNITY_EDITOR`. It must never become a production fallback.

#### One-report validation pipeline

The phase-specific P5.2 action described below has been superseded by the current endpoint regression at `Actions → Foam Cache & Validation → Run Fixed-Metric Consumer Regression (P9)`. Retained closed-phase tools are grouped under `Historical / Deep Diagnostics`.

One user action performed:

1. assigned-cache metadata and current/legacy classification;
2. exact provider/direct provenance for all sources;
3. five independent non-storing Edit Mode preparations;
4. obstacle stability checks after every preparation;
5. complete payload and twenty-section determinism comparisons;
6. CPU-emitted obstacle cells versus GPU readback scalar for each build;
7. frozen pre-P5 versus descriptor-owned legacy-raster parity for each source;
8. assigned-cache metadata and payload mutation proof;
9. one final pass/fail ledger with first-difference evidence.

Output:

```text
Library/RiverFoamDiagnostics/
  <river>_LatestP52ComprehensiveValidation.txt
```

The user normally supplies this single report. A second run of the same report is requested only after an Editor restart when the change specifically requires hot-reload/restart persistence evidence. Supplemental P5.1 actions remain available only when the comprehensive report identifies a narrower failure requiring isolation.

#### Exact implementation scope

Modified implementation files:

- `Game/Procedural/Core/GeneratedGeometryStableFingerprint.cs`;
- `Game/Procedural/Masses/GeneratedMass.cs`;
- `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.GeneratedSources.cs`;
- `Game/Procedural/Rivers/RiverObstacleExclusionResolver.cs`;
- `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.CacheDiagnostics.cs`;
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.CacheDiagnostics.cs`;
- `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyCacheAsset.cs`;
- `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyCacheCodec.cs`;
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs`;
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.TopologyCache.cs`;
- `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`.

Created diagnostic-only implementation:

- `Game/Procedural/Rivers/RiverObstacleExclusionResolver.LegacyParityDiagnostics.cs` and its `.meta`.

Modified canonical documents:

- this plan;
- the dependency register;
- the active blocker document.

#### Explicit non-changes

- active mapping remains `LegacyNormalizedAcross`;
- no fixed-metric allocation is activated;
- no source, routing, Motion Lane, transport, topology replacement, film, shape, or production-render behavior changes;
- no compute/HLSL file, kernel, texture, buffer, or dispatch changes;
- no scene, prefab, material, River serialized field, generated cache asset, layer, tag, component, or dependency changes;
- the expensive report is explicit Editor work only and creates output solely below `Library/RiverFoamDiagnostics`.

#### Mechanical acceptance

Before packaging, the implementation must prove:

- every changed C# file parses in raw, `UNITY_EDITOR`, and player-preprocessed forms;
- no local function/lambda captures a `ref`, `out`, or `in` parameter;
- every changed method call matches the changed signature;
- the fifteen serialized cache-asset fields are unchanged;
- payload format remains `3` and generator contract is exactly `3`;
- contract `(3,2)` is classified only as legacy obstacle fingerprint, `(2,1)` only as legacy coordinate, and `(3,3)` as current;
- all zero provider/utility/aggregate success paths are rejected;
- the frozen reference is structurally derived from the pre-P5 implementation and has no production caller;
- no diagnostic path stores the built artifact, saves assets, marks scenes/prefabs dirty, or runs automatically;
- all existing line endings and new unique metadata GUIDs are valid.

#### P5.2 implementation and mechanical-validation record

The supplied-snapshot implementation completed all recorded P5.2 work. The final post-implementation audit found and corrected one important diagnostic-lifetime hazard before delivery: `TryPrepareTopologyCacheInEditor` always releases `obstacleExclusionCells` and `obstacleExclusionScalar` in its `finally` block. Reading those fields after the preparation returned would therefore have produced an invalid empty CPU/GPU comparison. The final implementation requests a diagnostic capture before each build, snapshots CPU cells against the package's GPU-readback obstacle scalar inside `TryBuildTopologyCache` before resource release, and consumes only that immutable report after the preparation returns. The hook is wrapped in `UNITY_EDITOR`, inactive unless the comprehensive report explicitly requests it, and cannot affect ordinary builds or runtime startup.

Mechanical evidence:

- exact changed-file scope: sixteen files, including twelve C# files, three canonical Markdown documents, and one new `.meta`;
- all twelve C# files parse in raw, Editor-preprocessed, and player-preprocessed forms;
- no syntax-error or missing parser nodes;
- no nested function captures a `ref`, `out`, or `in` parameter;
- all changed API call arities match their definitions;
- all fifteen serialized fields on the cache asset are byte-for-byte declaration-identical to P5.1;
- cache payload/generator contract is exactly `3/3`;
- `(2,1)`, `(3,2)`, and `(3,3)` are classified as legacy coordinate, legacy obstacle fingerprint, and current respectively;
- every sentinel success boundary is rejected in utility, Generated Mass, provider collection, and aggregate construction;
- the frozen pre-P5 `TryBake`, candidate-range, and base-sample formulas are token-equivalent after diagnostic renaming/string formatting normalization;
- the exact-mesh intersection constants delegated by the frozen path are unchanged from pre-P5;
- the legacy parity method has exactly one caller, in the Editor-only disturbance diagnostic;
- the comprehensive report has one Inspector caller, five independent builds, eight final ledger rows, no asset-store/save/dirty call, and no automatic execution hook;
- no HLSL, compute shader, resource, scene, prefab, material, generated cache asset, or serialized River file changed;
- existing line-ending styles are preserved and the new Unity GUID is unique;
- all Markdown files are UTF-8 with balanced fences and record the one-report/two-report maximum validation policy.

A live Unity/C# compiler and Unity shader importer are unavailable in the supplied environment. The implementation is therefore mechanically verified, not Unity-compiled or runtime-validated.

#### Unity gate

Before rebuilding the assigned topology cache, run the comprehensive report once and supply the complete file. Required final ledger:

```text
Provider/direct fingerprint parity: PASS
Obstacle stability across five builds: PASS
Input fingerprint parity: PASS
Five-build payload/section determinism: PASS
CPU cells vs GPU obstacle scalar: PASS
Frozen pre-P5 vs descriptor legacy raster: PASS
Assigned cache stage/current parity: PASS
Assigned cache remained unchanged: PASS
Overall: PASS
```

Two-report closure sequence:

1. Run Report 1 before rebuilding. A format-3/generator-2 assigned cache is accepted only as the expected `PRE-REBUILD LEGACY OBSTACLE FINGERPRINT` stage; every other ledger line must pass.
2. Explicitly rebuild the assigned cache once. This is the only separate manual action and does not require pasted Console output.
3. Close and reopen Unity.
4. Run the same comprehensive report as Report 2. It must classify the assigned cache as current format `3` / generator `3`, compare its payload and all contract metadata exactly to Build 1, re-prove provider/direct identity after reload, and end with `Overall: PASS`.
5. Exact Play startup is not requested as a third P5.2 workflow. It becomes a mandatory row in the next owning runtime comprehensive report.

Any failed ledger line blocks P6 and must be diagnosed from the same report before requesting supplemental evidence. P5.2 requires at most the two comprehensive reports above.

#### Permanent validation-pipeline rule

Every future patch must ship the diagnostics needed to validate its own changed contract. The normal user workflow is one Inspector-triggered comprehensive report, or at most two reports when a reload/persistence boundary cannot be proven within one process. A long manual action sequence is permitted only when the required evidence cannot be represented in a complete report, and the plan must state why.

### 17.7B `RG-METRIC-P5.3` — Deterministic topology evaluation phase and publication parity

#### Objective

Close the final P5 blocker by removing live animation phase from the static cached obstacle product, correcting the CPU/GPU validation contract, and invalidating every cache generated under the hidden dynamic-phase contract.

#### Accepted Unity evidence

The two complete P5.2 reports supplied on 2026-07-17 established:

```text
provider/direct exact-world identity = PASS for all 5 sources
obstacle input stability across 5 builds = PASS
five-build payload/section determinism within each Editor session = PASS
frozen pre-P5 vs descriptor-owned legacy raster = PASS
assigned cache mutation proof = PASS
```

The first session produced five identical current builds with `587` occupied obstacle texels. After explicit rebuild and Editor restart, the second session produced five identical current builds with `590` occupied texels while descriptor, domain fingerprint, exact obstacle fingerprint, generation fingerprint, and combined fingerprint remained identical. The assigned generator-3 cache therefore differed from fresh Build 1 after restart despite identical recorded inputs.

Source review traced the omitted input to:

- `StylizedRiverFoamRuntime.Resources.cs::ResolveInitializationDimensions`, which captures `initializationMotionTime = river.MotionTime`;
- `StylizedRiverFoamRuntime.Topology.cs::ConfigureTopologyParameters`, which binds that value to `_FoamTime` during initialization;
- `CS_RiverFoam.Topology.hlsl::IsObstacleIntervalSampleInside`, which evaluates each exact obstacle interval against `RiverWaterEvaluateSurfaceHeight(..., _FoamTime, ...)`;
- `StylizedRiverFoamRuntime.TopologyCache.cs::CreateTopologyCachePackage`, which stores the resulting scalar but has no motion-time lane in its deterministic inputs.

The P5.2 report also proved its CPU/GPU equality rule was invalid: CPU data contains conservative candidate cells and nine exact solid-interval samples; the GPU accepts only the subset whose canonical water height lies inside every sample interval. `738` candidates versus `587/590` accepted cells is therefore not itself a publication mismatch. A valid publication audit must reject GPU-only cells, nonbinary output, malformed candidates, duplicate/out-of-range publication, or repeat instability while treating candidate-only cells as expected interval-test rejections.

#### Read-only review record completed before P5.3 edits

Reviewed complete current implementations and direct contracts:

- `Assets/AGENTS.md`;
- all three canonical River Foam plan/register/blocker documents;
- both supplied `LatestP52ComprehensiveValidation` reports;
- `StylizedRiverFoamRuntime.Resources.cs` allocation-phase time capture and resolver;
- `StylizedRiverFoamRuntime.Topology.cs` topology parameter binding and dynamic refresh ownership;
- `StylizedRiverFoamRuntime.Obstacles.cs` candidate upload, dispatch, and scalar readback;
- `StylizedRiverFoamRuntime.TopologyCache.cs` explicit preparation, package construction, capture timing, startup validation, and installation;
- `StylizedRiverFoamRuntime.CacheDiagnostics.cs` five-build orchestration, CPU/GPU report, assigned-cache staging, and final ledger;
- `StylizedRiverFoamTopologyCacheAsset.cs` stored-contract API;
- `StylizedRiverFoamTopologyCacheCodec.cs` format/generator classification and foundation tests;
- `StylizedRiverFoamRuntime.Constants.cs` startup-reason contract;
- `StylizedRiverEditor.Actions.cs` explicit Inspector action surface;
- `CS_RiverFoam.Resources.hlsl`, `CS_RiverFoam.Topology.hlsl`, and `CS_RiverFoam.compute` obstacle kernel/data contract;
- all direct `TryValidateStoredContract`, `TryValidateContractVersions`, `ConfigureTopologyParameters`, `RefreshDynamicTopologySources`, and `UpdateObstacleExclusionMask` call sites.

The supplied workspace has no `.git` directory, so branch/status/history comparison remains unavailable and is recorded as a validation limitation rather than inferred.

#### Approved file scope

**Canonical documents**

- `Assets/Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
- `Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
- `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`

**Cache/runtime/editor contracts**

- `Assets/Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyCacheAsset.cs`
- `Assets/Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyCacheCodec.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Topology.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.TopologyCache.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.CacheDiagnostics.cs`
- `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`

**Compute contract**

- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl`
- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Topology.hlsl`

No other path is approved. Any required expansion must be recorded here before editing.

#### Recorded scope expansion before edit

The activation implementation invalidates two current code comments that described legacy as the universally active runtime. `StylizedRiverFoamGridDescriptor.cs` and `StylizedRiverFoamRuntime.Obstacles.cs` are therefore added before modification for comment-only correction. Their executable bytes remain protected and will be compared separately in the post-change audit. **Status: approved by the existing P12 activation objective; implementation pending.**

### Invariants and non-goals

1. Active mapping remains `LegacyNormalizedAcross`; fixed-metric allocation remains deferred.
2. Live water rendering and current-shore animation continue using live/captured `_FoamTime` exactly as before.
3. Only static obstacle acceptance uses the new canonical topology evaluation time.
4. Canonical topology evaluation time is exactly `0f`; it is not seeded and is never derived from `river.MotionTime`.
5. Payload format remains `3`; generator contract advances `3 -> 4`.
6. Format-3/generator-3 assets are classified explicitly as `Legacy Dynamic Topology Phase` and are never installed or rewritten in Play Mode.
7. No kernel, dispatch count, texture, buffer, serialized field, scene, prefab, material, source, transport, film, shape, or production-render behavior changes.
8. Diagnostics remain explicit Editor actions with no automatic, import-time, domain-reload, `OnValidate`, or per-frame work.
9. Validation remains one comprehensive report before rebuild and the same report once after rebuild/restart.

#### File-by-file implementation sequence

| Step | File(s) | Change | Status |
|---|---|---|---|
| 1 | canonical plan | record evidence, scope, invariants, sequence, and gates before implementation | `COMPLETE` |
| 2 | `StylizedRiverFoamRuntime.Topology.cs`, `CS_RiverFoam.Resources.hlsl`, `CS_RiverFoam.Topology.hlsl` | add topology phase contract/time zero, bind separate compute scalar, and use it only in obstacle interval acceptance | `COMPLETE` |
| 3 | cache codec/asset/runtime constants/topology-cache callers | advance generator to 4 and classify generator 3 as legacy dynamic topology phase | `COMPLETE` |
| 4 | runtime diagnostics + Editor action | rename/extend P5.3 report, correct candidate/publication semantics, report evaluation phase for all five builds, and preserve two-stage assigned-cache proof | `COMPLETE` |
| 5 | dependency register + active blocker | replace stale P5.2 gate with P5.3 ownership and validation | `COMPLETE` |
| 6 | post-change audit | reread final files/callers, compare against P5.2 base, parse C#/HLSL, verify API arity/contracts/kernels/resources/line endings/package | `COMPLETE` |

#### Implementation and mechanical-validation record

P5.3 is mechanically complete in the supplied snapshot. The implementation adds one separate compute scalar, `_FoamTopologyEvaluationTime`, bound to canonical `0f` only for exact obstacle interval acceptance. Existing `_FoamTime` binding and current-shore/live-water consumers remain unchanged. Payload format remains `3`; generator contract is `4`; generator `3` is rejected explicitly as `Legacy Dynamic Topology Phase`. The P5.3 comprehensive report performs five non-storing builds, validates candidate/publication subset ownership, records phase contract/time per build, preserves frozen pre-P5 raster parity, and proves assigned-cache immutability.

Post-change audit result:

```text
approved changed files = 12 / 12
changed C# files parsed = 7 / 7 in raw, Editor, development, and release forms
C# parser errors or missing nodes = 0
CS1628 nested ref/out/in capture candidates = 0
changed API arity mismatches = 0
cache serialized fields changed = 0
compute kernels before/after = 22 / 22, same order
new kernels/dispatches/textures/buffers = 0
main compute shader changed = no
line-ending regressions = 0
scene/prefab/material/cache/meta changes = 0
static checks = 44 passed / 0 failed
```

A Unity/C# compiler and Unity shader importer are unavailable in the supplied environment. The candidate is therefore mechanically verified, not Unity-compiled or runtime-validated.

#### Acceptance criteria

The first report may accept an assigned format-3/generator-3 cache only as the expected pre-rebuild legacy stage. All five fresh builds must use:

```text
topology phase contract = 1
topology evaluation time = 0 [0x00000000]
live river motion time = ignored by obstacle cache generation
```

Required ledger:

```text
Provider/direct fingerprint parity: PASS
Obstacle stability across five builds: PASS
Input fingerprint parity: PASS
Five-build payload/section determinism: PASS
CPU candidate/GPU publication parity: PASS
Frozen pre-P5 vs descriptor legacy raster: PASS
Topology evaluation phase: PASS
Assigned cache stage/current parity: PASS
Assigned cache remained unchanged: PASS
Overall: PASS
```

After one explicit rebuild and Editor restart, the same report must classify the assigned asset as format 3 / generator 4 and prove payload plus metadata exact to Build 1. No third report or separate Play checklist is requested unless the comprehensive report cannot isolate a concrete failure.

#### Stop conditions

Stop before P6 if any of the following occurs:

- any fresh build resolves a nonzero or nonidentical topology evaluation time;
- any GPU-occupied cell is absent from the CPU candidate set;
- output contains nonbinary obstacle texels, duplicate/out-of-range candidates, or repeat instability;
- frozen pre-P5/descriptor legacy raster parity regresses;
- generator-3 cache is accepted as current or installed in Play Mode;
- assigned cache changes during a non-storing report;
- current-shore/live rendering time is accidentally frozen;
- implementation requires a new kernel, resource allocation, serialized field, scene, prefab, or material edit.

### 17.7.4 P6 pre-edit review, decisions, and approved implementation scope

**Status:** `COMPLETE — corrected P6b live-resource report passed`

**Prerequisite evidence:** Both user-supplied P5.3 reports passed. The pre-rebuild report classified format 3 / generator 3 as `Legacy Dynamic Topology Phase`; the post-rebuild report classified format 3 / generator 4 as current and reproduced payload `1,954,946` bytes / `0BA390B87B301420` exactly after Editor restart.

**Repository limitation:** the supplied workspace contains no `.git` directory. Branch, HEAD, status, history, and pre-existing live working-tree diffs cannot be inspected. The immutable comparison baseline for P6 is `/mnt/data/p6base`, copied byte-for-byte from the validated `/mnt/data/p53work` snapshot before the first P6 edit.

**Complete read-only review performed before implementation:**

- governing rules: `Assets/AGENTS.md`;
- canonical docs: this plan, `River_Foam_Fixed_Metric_Dependency_Register.md`, and `River_Foam_Active_Blockers_and_Next_Patches.md`;
- routing/Motion Lane owner and lifecycle caller: `StylizedRiverFoamRuntime.Obstacles.cs`, `StylizedRiverFoamRuntime.Lifecycle.cs`, `StylizedRiverFoamRuntime.Resources.cs`, `StylizedRiverFoamRuntime.Members.cs`, `StylizedRiverFoamRuntime.PublicSurface.cs`;
- compute binding and descriptor owner: `StylizedRiverFoamRuntime.Compute.cs`, `StylizedRiverFoamRuntime.Topology.cs`, `StylizedRiverFoamGridDescriptor.cs`, `CS_RiverFoam.Resources.hlsl`, `CS_RiverFoam.Coordinates.hlsl`;
- routing/Motion Lane consumers: `CS_RiverFoam.Motion.hlsl`, `CS_RiverFoam.Simulation.hlsl`, `RiverWaterFoamVelocity.hlsl`, `SH_CleanStylizedRiver.shader`, `StylizedRiverFoamRuntime.Binding.cs`;
- external-field producers/contracts: `StylizedRiverDisturbanceRuntime.Resources.cs`, `StylizedRiverDisturbanceRuntime.PublicSurface.cs`, `CS_RiverDisturbance.compute`;
- external-field consumers: `CS_RiverFoam.Sampling.hlsl`, `CS_RiverFoam.compute`, and `StylizedRiverFoamRuntime.Topology.cs`;
- validation UI and retained diagnostic infrastructure: `StylizedRiverEditor.Actions.cs`, `StylizedRiverFoamRuntime.CacheDiagnostics.cs`.

**Recorded findings:**

1. Legacy Motion Lane downstream wavelength is already approximately physical because normalized U is multiplied by `fieldWidth / fieldHeight`; with 32 m chunks and equal legacy structural resolution per chunk/across, the first octave is `32 / 8.5 = 3.7647 m`. Its lateral scale remains normalized to the river rectangle and therefore changes with width.
2. Legacy fixed-cell Y smoothing uses offsets `1` and `2` rows for two passes. Fixed-metric smoothing must convert explicit physical radii to rows; legacy code must remain byte-identical.
3. Motion Lane scrolling is authored in metres but stored as cells. Fixed mode can preserve physical scroll speed by dividing by descriptor `ResolvedDxMetres`; legacy retains the existing minimum metric-row spacing fallback.
4. Routing approach currently scales with total field width (`max(6, round(fieldWidth*0.055))`), so its physical reach grows with river length. Closure/contact/margins/dead-band are also cell-authored. Fixed mode requires metre-owned reaches while legacy remains exact.
5. Routing is already upstream-only (`releaseCells = 0`) and component-local; P6 must preserve that no-rear/no-wrap topology.
6. Disturbance fields intentionally keep independent Low/Medium/High dimensions and normalized cross-river mapping. The narrow integration required is Foam physical `(s,n)` to Disturbance normalized `(u,v)`; no Disturbance allocation or generator change is required.
7. Current external sampling passes Foam UV directly. That is correct only for legacy normalized-across Foam. Fixed mode must reconstruct local distance and lateral metres from the descriptor/metric row, then convert lateral metres back to the Disturbance field's local normalized cross-section.
8. Motion Lane and routing renderer diagnostics sample `foam.fieldUV`; P6 must bind the descriptor to the material and compute a dedicated motion-field UV. Normal production Foam/state sampling remains owned by P9 and is not changed.

**P6 unit decisions:**

- fixed Motion Lane downstream basis: 32 m repeat domain, preserving the accepted first-octave wavelength and all existing octave coefficients;
- fixed Motion Lane lateral reference span: 10 m, so wider rivers contain proportionally more independent lateral structure while a 10 m river reproduces the accepted normalized scale;
- fixed smoothing offsets: 0.20 m near and 0.40 m far, two passes with unchanged weights;
- fixed routing approach reach: 2.0 m; front-contact closure: 0.35 m; contact-strength reach: 0.50 m; minimum lateral margin: one resolved lateral cell, otherwise 22% of obstacle physical height; centre tie dead-band: max(0.10 m, 10% of obstacle physical height);
- fixed routing connectivity remains 8-neighbour cell connectivity because connectivity is topological, not a physical morphology radius;
- Disturbance U uses the Disturbance allocated 32 m-chunk field length; Disturbance V uses the local metric row's left/right surface widths and the physical Foam cell-centre lateral metre coordinate.

**Approved files:**

- modify the three canonical Markdown documents;
- modify `StylizedRiverFoamRuntime.Obstacles.cs`, `StylizedRiverFoamRuntime.Compute.cs`, `StylizedRiverFoamRuntime.Binding.cs`, `StylizedRiverFoamRuntime.PublicSurface.cs`, `StylizedRiverFoamRuntime.Constants.cs`, `StylizedRiverEditor.Actions.cs`;
- modify `CS_RiverFoam.Coordinates.hlsl`, `CS_RiverFoam.Sampling.hlsl`, and `SH_CleanStylizedRiver.shader`;
- create `StylizedRiverFoamRuntime.P6Diagnostics.cs` and its `.meta`.

**Explicit non-goals:** no active mapping switch; no source rasterization; no persistent transport/CFL changes; no topology replacement; no film/shape migration; no normal production Foam UV migration; no Disturbance allocation/generation edit; no scene, prefab, material, cache asset, serialized River field, resource, kernel, dispatch, texture, or buffer addition.

**Implementation sequence:**

1. Add exact legacy/fixed Motion Lane coordinate, smoothing, signature, and scroll branches.
2. Add exact legacy/fixed routing reach conversion and physical diagnostics while preserving one-sided component stamping.
3. Add descriptor-aware Foam-to-Disturbance UV mapping and route all four external samplers through it.
4. Bind descriptor uniforms to the river material and use a dedicated motion/routing debug UV without touching normal Foam sampling.
5. Add one user-triggered P6 comprehensive report covering legacy parity, fixed physical invariance, routing ownership, external same-point mapping, renderer/compute coordinate parity, and scope/runtime resource invariants.
6. Run parser/static/API/HLSL/scope/line-ending/package audits, update statuses, and package changed files only.

**Acceptance:** active legacy Motion Lane/routing arrays remain exact against frozen reference functions; fixed candidate wavelength/smoothing/scroll/reaches stay within half-cell tolerance across 5/10/20/40 m widths; external UV is dimension-independent for the same physical point; no GPU field consumer samples a fixed Foam cell by coincident external indices; one P6 report returns `Overall: PASS`.

### 17.7.5 P6 implementation and mechanical verification disposition

P6 is implemented within the exact recorded 13-file scope:

- legacy Motion Lane generation, smoothing, routing formulas, external UV, and renderer-debug UV remain on exact legacy branches;
- fixed Motion Lane uses physical `(s,n)` cell centres, a 32-m downstream basis, 10-m lateral reference span, descriptor `dx` scroll conversion, and 0.20/0.40-m smoothing offsets;
- fixed obstacle routing resolves 2.0-m approach, 0.35-m closure, 0.50-m contact, physical lateral margin/dead-band, and zero downstream release;
- Pressure, Static Wake, Wake, and Ripple samplers map a fixed Foam point to Disturbance UV through physical local distance and local asymmetric surface widths, without changing Disturbance allocation or dimensions;
- the material receives the existing five-lane descriptor ABI and only Motion Lane/routing/obstacle debug sampling gains fixed-metric UV; normal production Foam state UV remains owned by P9;
- one Editor-only `LatestP6ComprehensiveValidation` report owns active legacy readiness, fixed physical invariance, no-zero-speed evidence, upstream-only routing, unequal-dimension same-point mapping, source readiness, neutral fallback, and assigned-cache mutation proof.

Mechanical evidence: 13 changed files exactly; five changed/new C# files parsed in raw, Editor, development-player, and release-player forms with zero parser errors; no CS1628 candidates or malformed multiline strings; 22 compute kernels remain in the same order; project-wide RenderTexture/ComputeBuffer/dispatch/FindKernel counts remain `11/15/21/33`; serialized declarations are unchanged; 100,000 randomized legacy routing cases matched; 1,000 fixed-routing reach assertions and 44 fixed coordinate/unequal-dimension assertions passed; no scene, prefab, material, cache asset, source, transport, film, or shape implementation changed. Unity compilation and shader import passed after P6a. The first report exposed an invalid diagnostic lifecycle, corrected by P6b. The corrected report installed the assigned cache without build/write, reached live `Ready`, measured nonzero Motion Lane and obstacle routing with zero rear leakage, proved same-point external-field mapping and fallback creation, then proved cleanup, disabled bindings, and cache immutability. Its final ledger returned `Overall: PASS`; P6 is complete.

### 17.7.6 `RG-METRIC-P6b` — Live diagnostic preparation transaction

**Status:** `COMPLETE — corrected report returned Overall: PASS`

**Observed Unity evidence:** The user-triggered P6 report reached the normal cache build successfully (`1,954,946` bytes / `0BA390B87B301420`) but then reported `Resources ready: True` together with `Active descriptor: Unallocated`, no fixed candidate, empty Motion Lane/routing arrays, sentinel signatures `-2147483648`, and a missing neutral fallback. The synthetic fixed-routing cases and assigned-cache mutation proof passed. This combination proves the diagnostic read state after cleanup rather than while resources were live.

**Complete corrective read-only review:**

- `StylizedRiverFoamRuntime.P6Diagnostics.cs`: report ordering, cache snapshot, runtime-resource reads, final ledger, and signature checks;
- `StylizedRiverFoamRuntime.TopologyCache.cs`: `TryPrepareTopologyCacheInEditor()` preparation loop, serialization, and unconditional cleanup;
- `StylizedRiverFoamRuntime.Resources.cs`: `EnsureResources()`, initialization phases, current-cache installation, `AreResourcesCompleteAndCurrent()`, finalization, and `ReleaseResources()` reset contract;
- `StylizedRiverFoamRuntime.Obstacles.cs`: `EnsureMotionFieldsCurrent()` and Motion Lane/routing telemetry publication;
- `StylizedRiverFoamRuntime.Binding.cs`: `BindDisabled()` cleanup binding;
- `StylizedRiverDisturbanceRuntime.GeneratedSources.cs`: exact generated-obstacle registry preparation;
- `StylizedRiverEditor.Actions.cs`: the existing single-button caller;
- canonical P6 scope and one-report validation policy in this plan, the dependency register, and active blockers.

**Repository limitation:** `/mnt/data/p6work` has no `.git` directory. Branch, status, history, and HEAD comparison remain unavailable. `/mnt/data/p6base` remains the immutable pre-P6 baseline; the current P6 implementation plus P6a is the corrective comparison source.

**Proven defect:** `TryPrepareTopologyCacheInEditor()` calls `ReleaseResources()` and `BindDisabled()` in its unconditional `finally`. `RunP6ComprehensiveValidationReport()` calls that method and then reads `gridDescriptor`, `fixedMetricCandidateDescriptor`, textures, arrays, and telemetry. `ReleaseResources()` resets all of those values, including both signatures to `int.MinValue`. The report's additional `signature != 0` checks also use the wrong uninitialized sentinel.

**Approved files:**

1. `Assets/Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`;
2. `Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md`;
3. `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`;
4. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.P6Diagnostics.cs`.

**Invariants and non-goals:**

- no P6 production Motion Lane, routing, external-field, renderer, compute, or cache behavior changes;
- no topology payload build or serialization during the P6 report;
- no cache, scene, prefab, material, serialized River state, resource contract, kernel, dispatch, texture type, or buffer type change;
- the assigned generator-4 cache is read-only validation input;
- all live evidence is captured before cleanup;
- cleanup is mandatory in `finally`, disables bindings, and leaves the runtime in normal dirty/not-started state;
- the single-report user workflow is retained.

**Implementation sequence:**

1. Replace cache-building preparation with a P6-owned non-storing live-resource transaction: prepare generated obstacle sources, reset transient Foam resources, and step `EnsureResources()` to `Ready` by installing the assigned current cache.
2. Keep descriptors, textures, arrays, source readiness, and telemetry alive while every P6 ledger section executes.
3. Correct Motion Lane and routing readiness checks to reject `int.MinValue`, not zero.
4. Capture assigned-cache metadata/payload after evidence collection, then release resources and call `BindDisabled()` in guaranteed cleanup.
5. Add cleanup evidence proving resources/descriptors are released, initialization is reset, bindings are disabled, and the assigned cache remains byte-identical.
6. Run parser, preprocessor, API/member, control-flow ordering, prohibited-call, lifecycle-contract, scope, line-ending, and package extraction audits.

**Acceptance:** The report must not call `TryPrepareTopologyCacheInEditor()` or `TryBuildTopologyCache()`. It must reach `InitializationPhase.Ready`, prove `AreResourcesCompleteAndCurrent()`, inspect all P6 runtime state before cleanup, then prove cleanup and assigned-cache immutability afterward. The same existing Inspector button must produce one `LatestP6ComprehensiveValidation` file whose final ledger includes live preparation, all five P6 contracts, cleanup, cache immutability, and `Overall: PASS`.

**Implementation disposition:** The P6 report now owns a non-storing assigned-cache installation transaction. It prepares the exact generated-obstacle registry, sets sentinel monitors around cache serialization and explicit-preparation upload telemetry, advances the normal `EnsureResources()` state machine to `Ready`, and evaluates every P6 runtime ledger while descriptors, textures, cached obstacle scalar, Motion Lane, routing, and neutral fallback remain live. It no longer calls either cache-building API. Runtime routing evidence counts occupied cells from the cached obstacle scalar when CPU candidate cells are intentionally absent after cache installation. Both readiness checks now use `int.MinValue`, the actual release sentinel. Cleanup occurs only when the diagnostic owns the transaction; it runs in `finally`, releases resources, restores normal dirty/not-started state, disables renderer bindings, verifies black fallback bindings, and only then performs the assigned-cache byte proof.

**Post-change evidence:** exactly four approved files differ from the reconstructed P6+P6a state; the production P6 C#/HLSL/shader files are byte-identical; the complete P6 static suite remains `25/25`; the P6b lifecycle/semantic suite is `45/45`; raw, Editor-preprocessed, and player-preprocessed C# parse with zero errors; no CS1628 candidate, malformed string, invalid `.Payload` access, cache-build call, topology-build call, wrong `ByteArraysEqual` arity, serialized field, new allocation, kernel, dispatch, scene, prefab, material, or cache-asset edit exists. Unity then compiled/imported the patch, and the corrected report proved: live assigned-cache installation with `build:0` and `writes:0`; active legacy ownership; fixed-candidate readiness; nonzero Motion Lane; obstacle routing with `rearLeak=0`; unequal-dimension external-field same-point mapping; neutral fallback creation; cleanup and disabled bindings; assigned-cache immutability; `Overall: PASS`.

### 17.8 `RG-METRIC-P6` — Obstacle routing, Motion Lane, and external-field integration

**Objective:** Preserve physical flow behavior and correct sampling of fields with independent dimensions.

**Required work:**

- migrate routing approach, closure, margins, contact and support reaches according to unit policy;
- migrate Motion Lane noise aspect, wavelength, smoothing, scroll, rebuild signatures, readback, and renderer sampling;
- map each Foam cell centre to Disturbance UV by physical position rather than field-index coincidence;
- preserve Disturbance allocation and quality;
- verify obstacle-generated source readiness and neutral fallbacks.

**Verification:** no zero-speed regions; physical lane wavelength stable; scroll speed stable; obstacle C routing; no upstream/rear leakage; Pressure/Wake/Ripple same-point sampling with unequal field dimensions.

**Stop conditions:** Disturbance code would need an architectural migration rather than a narrow physical-point sampling interface; routing physical reach is undefined; Motion Lane accepted scale cannot be reproduced.

**Rollback checkpoint:** topology/obstacle metric baseline before automatic birth and transport changes.

### 17.9 `RG-METRIC-P7` — Automatic/manual source migration and unit policy

**Status:** `CLOSED — UNITY-VALIDATED`

**Objective:** Migrate every automatic and manual Layer C source path to an explicit physical-unit contract while preserving the active `LegacyNormalizedAcross` result on its exact compatibility branches. Fixed-metric allocation remains deferred; P7 prepares source geometry, dispatch bounds, manual commands, probes, and debug parity so later activation cannot inherit normalized-row anisotropy.

**Read-only evidence reviewed before implementation:**

- canonical architecture and queue: this plan, `River_Foam_Fixed_Metric_Dependency_Register.md`, and `River_Foam_Active_Blockers_and_Next_Patches.md`;
- repository rules: `Assets/AGENTS.md`;
- source parameter ownership and validation: `StylizedRiver.cs`, `StylizedRiverEditor.Foam.cs`, `StylizedRiverEditor.Actions.cs`;
- automatic source preparation and all eight event families: `StylizedRiverFoamRuntime.BirthEvents.cs`, `StylizedRiverFoamRuntime.State.cs`;
- manual ellipse/stroke/compound command path, automatic GPU upload, dispatch culling, isolated probe, production/debug raster kernels: `StylizedRiverFoamRuntime.Injection.cs`, `StylizedRiverFoamRuntime.Lifecycle.cs`, `StylizedRiverFoamRuntime.BirthDiagnostics.cs`, `CS_RiverFoam.compute`, `CS_RiverFoam.Resources.hlsl`, and `CS_RiverFoam.Coordinates.hlsl`;
- removed transfer path: `StylizedRiverFoamRuntime.BirthTransfer.cs` confirms no source-transfer texture remains;
- active descriptor/candidate mapping APIs: `StylizedRiverFoamGridDescriptor.cs` and `StylizedRiverFoamRuntime.RuntimeUpdates.cs`;
- validated live diagnostic lifecycle: `StylizedRiverFoamRuntime.P6Diagnostics.cs` and the Unity P6 report dated 2026-07-17;
- comparison state: `/mnt/data/p7base` is the immutable P6+P6a+P6b source baseline. The supplied snapshot has no `.git` directory, so Git status/history/SHA review is unavailable and must not be claimed.

**Interruption-resume audit (2026-07-17):**

- the interrupted workspace is recoverable and remains based on the validated P6+P6a+P6b source baseline; no restart from rollback is required;
- implementation was incomplete: `StylizedRiverFoamRuntime.P7Diagnostics.cs` and the two remaining canonical document updates were not created, and no P7 validation/package artifact existed;
- one accidental out-of-scope compute change was found: `EvaluateFoamShape` had been switched to source-range Y uniforms even though its production dispatch does not bind them. P7 must restore that kernel exactly and apply the Y-range migration only to `InjectFoam`;
- `CS_RiverFoam.Coordinates.hlsl` had its baseline CRLF line endings unintentionally converted to LF. P7 must restore CRLF while retaining only the intended helper addition;
- the normalized composition API cannot be implemented as a literal metric-command wrapper without changing accepted normalized drift/bend behavior along width-varying river samples. The implementation therefore uses one explicit compatibility mode and one explicit metric mode that share queue/raster infrastructure but preserve distinct unit semantics. This replaces the earlier wrapper wording without changing the objective or approved scope.
- post-resume range audit found the same distinction in fixed-metric manual raster culling: compatibility ellipse/compound centres and compatibility segment endpoints are converted through each candidate row’s local left/right widths by the GPU, while metric commands remain fixed in lateral metres. Fixed compatibility Y bounds must therefore union every candidate X row; one centre-row conversion can under-bound width-varying rivers. Validation must prove shared anchor placement and independent physical containment, not require compatibility and metric dispatch rectangles to be identical.
- final semantic audit found and removed one duplicate exact `ResolveCompatibilityManualLateralBounds` method left by the interruption; the validation pipeline now scans exact method signatures across the runtime partial class so this class of compile defect cannot pass packaging.
- final fixed-range audit found that Shore/Wash source bounds must sample each actual candidate row distance, including longitudinal feather rows outside the authored endpoint interval. Clamping those rows back to the source endpoints could under-bound a width-varying river. Production and validation now evaluate every dispatch column plus padded longitudinal endpoints at their true domain distance.
- the comprehensive report was strengthened before delivery to validate compatibility and metric compound commands separately, prove `ClearRange` retains independent full-Y ownership, compare every automatic-event GPU lane including build/hold/release progression, and resolve inspected source paths from `Application.dataPath` rather than Unity's working directory.

**Serialized/public source-unit classification:**

- metres: all fields whose names end in `Metres`, `MetresPerSecond`, or physical length/width/reach/offset contracts already documented as metres;
- seconds: duration, hold, release, rest, and formation timing fields;
- normalized/unitless: coverage, activity, weights, lifecycle fractions, Presence, Remaining Life, breakup strengths, seeds, progress, and compatibility position controls;
- compatibility cells: `foamShoreRibbonThicknessCells` and `foamShoreRibbonOffsetVariationCells` remain serialized unchanged. Legacy rasterization continues to interpret them in local cross-river cells. Fixed-metric source preparation resolves them once to source-local metres;
- compatibility normalized manual position: existing public/serialized normalized entry points remain valid compatibility commands. They share queue/raster infrastructure with new metric entry points, but preserve row-local normalized semantics; metric commands own fixed global-distance/lateral-metre placement and metre drift.

**Approved implementation scope:**

1. `Assets/Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`;
2. `Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md`;
3. `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`;
4. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.SourceUnits.cs` plus `.meta`;
5. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.P7Diagnostics.cs` plus `.meta`;
6. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.State.cs`;
7. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs`;
8. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs`;
9. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs`;
10. `Assets/Game/Procedural/Rivers/StylizedRiver.cs`;
11. `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`;
12. `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`;
13. `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl`;
14. `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Coordinates.hlsl`;
15. `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`.

**Invariants and non-goals:**

- active allocation remains `LegacyNormalizedAcross`; no serialized mapping switch or hidden activation path;
- no source family, weight, seed, lifecycle value, activity budget, event capacity, ownership rule, or accepted Arc/Semi-Arc path topology is retuned;
- existing serialized field names/types/defaults remain unchanged; no scene, prefab, material, or cache asset edit;
- legacy automatic source GPU values and raster formulas remain exact on the legacy branch, including Shore Ribbon cell-authored thickness/offset variation;
- fixed-metric branches use physical cell centres, physical local-normal spacing, and metric dispatch bounds;
- debug and production automatic-source kernels continue to call one shared evaluator;
- no transport, film, shape, topology, routing, Motion Lane, Disturbance allocation, or production Foam-render migration;
- no new per-frame readback or full-field rebuild; diagnostics are explicit Editor actions only;
- the P7 report reuses the Unity-validated non-storing live-resource transaction and must inspect live state before cleanup, prove cleanup afterward, and prove assigned-cache immutability.

**Implementation sequence:**

1. Add one centralized source-unit resolver for global-distance/lateral-metre conversion, legacy-exact/fixed-metric X/Y dispatch ranges, Shore Ribbon compatibility-cell resolution, and physical probe layout.
2. Extend transient CPU source commands/events with explicit metric-lateral values while retaining normalized compatibility fields and without changing serialized data or GPU buffer stride.
3. Route normalized and metric manual APIs through shared internal command infrastructure with an explicit compatibility/metric unit mode; fixed compatibility dispatch bounds must union row-local normalized-to-metre conversions across every candidate X column, while metric bounds remain fixed in metres. Add metric public APIs and update the existing River manual action to expose the resolved start-anchor values without changing normalized command semantics.
4. Migrate automatic-source CPU dispatch culling to descriptor-aware metric ranges and preserve the legacy formulas exactly.
5. Bind metric manual-source uniforms and migrate `InjectFoam`, segment injection, automatic source raster cell centres, and source-domain clipping to descriptor-aware coordinate helpers.
6. Resolve Shore Ribbon cell-authored thickness/variation deliberately: legacy uses local cells; fixed metric uses source-prepared metres. Preserve Arc/Semi-Arc one-cell normal shell and physical core/feather behavior.
7. Migrate isolated probe dimensions/gaps to a fixed physical contract only on the fixed branch; preserve the current legacy percentage/cell layout exactly.
8. Update Inspector text to state compatibility units and show resolved physical placement without changing serialized values.
9. Add one comprehensive P7 report covering parameter classification, all eight source families, manual ellipse/segment/compound commands, dispatch containment, source cell-centre mapping, Shore Ribbon policy, Arc/Semi-Arc normal spacing, probe layout, flow reversal, debug/production evaluator identity, lifecycle/event-cap invariants, cleanup, and cache immutability.
10. Run parser/preprocessor, symbol/member/arity, HLSL declaration/binding, CPU/HLSL formula, legacy-equivalence, fixed-metric invariant, resource/kernel/dispatch-count, serialized-field, scope, line-ending, GUID, and package extraction audits.

**Acceptance criteria:**

- Unity imports with zero C# and shader/compute errors;
- one P7 report reaches live `Ready` state without cache build/write and ends `Overall: PASS`;
- all eight automatic families report physical bounds, nonzero/minimum footprint, progression/continuity, forward/reversed-flow containment, and source-unit compliance;
- manual ellipse, segment/stroke, compound, clear-range ownership, and isolated probe metric contracts pass; normalized and metric commands must match at their authored anchors, while each mode’s independently correct width-varying physical bounds are validated rather than forced to share one rectangle;
- fixed candidate dispatch ranges contain every sampled physical footprint without full-field fallback; legacy ranges are bit/formula equivalent to the baseline;
- automatic production/debug raster paths are proven to share the same evaluator;
- assigned cache metadata/payload remain byte-identical and diagnostic cleanup/bindings pass.

**Post-change mechanical evidence:**

- exactly 17 approved paths differ from the immutable P6 baseline: 15 approved implementation/document paths plus the two new `.meta` files; no scene, prefab, material, cache asset, topology, routing, Motion Lane, transport, film, shape, or production-render file changed;
- nine changed/new C# files parse with zero syntax/missing-node errors in raw, Editor, development-player, and release-player preprocessing modes; no malformed multiline string, CS1628 capture candidate, duplicate exact method signature, invalid cache API use, or changed-call arity defect remains;
- the seven-`Vector4` automatic-source GPU stride and every serialized River/runtime field declaration remain unchanged; the active mapping remains `LegacyNormalizedAcross` and fixed allocation remains deferred;
- exact legacy-equivalence tests passed 800,000 automatic-source comparisons, 100,000 manual-source comparisons, and 100,000 isolated-probe comparisons; fixed-metric source/range/round-trip invariants passed 200,000 cases with zero failures;
- 165 primary static/semantic checks and 34 supplementary HLSL/compute checks passed. Only `EvaluateFoamAutomaticSourceRasterSample`, `FoamEvaluateShoreRibbonSource`, `InjectFoam`, and `ResolveFoamSegmentInjectionSample` differ inside the compute file; `EvaluateFoamShape` and `ClearRange` are byte-identical to P6;
- kernel order/count, resource declarations, RenderTexture/ComputeBuffer allocation counts, dispatch counts, GUID uniqueness, intended line endings, balanced Markdown fences, and changed-file scope passed;
- Unity compilation and compute import passed; the Inspector-triggered P7 comprehensive report returned `Overall: PASS`, closing P7.

**Stop conditions:**

- any existing serialized field would require a changed meaning rather than an explicit compatibility resolver;
- Arc/Semi-Arc accepted front-only path or thin source shell cannot be preserved;
- a source family needs a new resource, state texture, kernel, dispatch, or event-capacity change;
- the validator cannot prove it measured live source state before cleanup.

**Rollback checkpoint:** `/mnt/data/p7base` — validated P6 source-metric baseline before source migration.

### 17.10 `RG-METRIC-P8` — Persistent transport, CFL, curvature, and topology replacement

**Status:** `CLOSED — UNITY-VALIDATED`. P8a fixed the real lateral descriptor defect; P8b corrected the validator symbol; the final P8 report proved `1,491` overlap cells, `863` cleared cells, zero GPU remap mismatches, physical topology-transition mapping, cleanup, cache immutability, and `Overall: PASS`.

**Objective:** Make persistent material movement and resource replacement numerically correct under the prepared fixed-metric descriptor while preserving exact active legacy behavior.

**Reviewed implementation evidence:**

- `StylizedRiverFoamRuntime.Lifecycle.cs::ResolveTransportSubsteps` owns the material-tick CFL/substep gate.
- `StylizedRiverFoamRuntime.Compute.cs::DispatchSimulateRange` owns each conservative substep dispatch and packed-state swap.
- `CS_RiverFoam.Simulation.hlsl` already carries Presence, life moment, and pattern moment through a first-order finite-volume donor-cell solve and separately records endpoint outflow/clamp attribution.
- `StylizedRiverFoamRuntime.Topology.cs::BuildMetricBuffer` provides per-column signed centreline curvature and descriptor spacing.
- `StylizedRiverFoamRuntime.Resources.cs::FinalizeInitialization` releases dimension-change visible holds after the new resource set is complete.
- `StylizedRiverFoamRuntime.TopologyReplacement.cs` owns the previous generated topology, previous metric rows, and held material textures, but currently maps generated topology through normalized UV and does not remap persistent material.
- `CS_RiverFoam.TopologyTransition.hlsl` is the previous/current topology mapper.

**Approved files:** the 19 paths listed under the active P8 gate in `River_Foam_Active_Blockers_and_Next_Patches.md`. No other code, asset, or generated file may change.

**Required work:**

- preserve exact legacy transport formulas and simulation bounds;
- use descriptor spacing and the fixed curvilinear metric `J = 1 - kappa*n` for fixed cell area, lateral-face length, and downstream CFL;
- adopt bounded corrected curvature with `J >= 0.25`, while reporting raw and bounded values;
- expose separate downstream/lateral/total CFL evidence and preserve the 0.90 target / 64-substep stop policy;
- retain conservative packed transport and endpoint-only outflow under forward and reverse flow;
- store the complete previous descriptor alongside transition metric rows;
- map generated topology by physical `(global s,n)` for legacy/fixed previous descriptors;
- exact-copy persistent material only between compatible integer-aligned fixed lattices; clip to new valid fluid after copy;
- deliberately clear legacy, spacing-changing, phase-changing, non-integer, unsupported-contract, or curvature-incompatible replacements;
- add one dirty-time exact-remap kernel with no new persistent resource;
- add one comprehensive live P8 report and reuse the proven P6/P7 cleanup and cache-immutability transaction.

**Implemented result:**

- fixed-only curvilinear cell area and lateral-face geometry use bounded `J = max(0.25, 1 - kappa*n)`;
- CFL now reports downstream/lateral components and applies `dx*Jmin` only to the fixed downstream component while preserving the existing target and hard limit;
- previous transition snapshots retain the complete descriptor, GPU descriptor, metric rows, and material-authority state;
- exact fixed-lattice replacement is policy-gated by contracts, spacing, phase, integer longitudinal/global-Y alignment, and overlapping curvature; unsupported replacements clear explicitly;
- one dirty-time `RemapPersistentFoamState` kernel copies packed Presence/life moment/pattern moment by exact physical cell identity, clips to current valid fluid, and reuses existing resources;
- generated-topology transition now maps current physical `(global s,n)` into the previous legacy or fixed descriptor;
- the single P8 report executes a live synthetic GPU remap/readback in addition to CPU conservation, CFL, curvature, policy, source-contract, cleanup, and cache-immutability gates.

**Mechanical audit:** 44/44 primary checks passed; all nine changed C# files parsed in raw/Editor/development/release configurations; the project-owned method declaration/call audit found zero arity or duplicate-signature failures; kernel count changed 22→23 with no existing reorder; production persistent-resource token counts remained unchanged; 100,000 packed-flux and 100,000 CFL/Jacobian reference cases passed. The subsequent P8a/P8b Unity reports closed the real descriptor defect and validator-symbol defect, ending with `Overall: PASS`.

**First Unity report evidence and `RG-METRIC-P8a` correction:**

- GPU remap reported `overlap=1491`, `mismatches=1491`, first mismatch `(0,2)`.
- The C# descriptor lateral vector is `(phase, globalYBase, rowCount, guardRows)`; the fixed HLSL cell-centre resolver incorrectly used `.z` as base and `.w` as row count. With zero guard rows, every row collapsed to one invalid lateral coordinate and no previous cell could resolve.
- `ApplyGeneratedTopologyTransition` receives texel-centre UV from both production callers and resolves physical `(s,n)` through `FoamGridLocalDistanceAtUV` and `FoamLateralMetresAtUV`; the validator incorrectly required the `AtTexel` helper names.
- P8a corrects the HLSL lane decode, validates producer/consumer lane agreement, checks the real production UV call path, and retains the one-report validation workflow.
- Post-correction evidence: exactly two HLSL lane substitutions; legacy branch byte-identical; 7,887 CPU/HLSL lateral cell-centre comparisons passed; the original synthetic remap resolves 1,491 overlap and 863 exterior cells exactly; targeted audit 21/21; full P8 regression 44/44.

**Second Unity report evidence and `RG-METRIC-P8b` plan:**

- the real GPU remap passed: `overlap=1491`, `cleared=863`, `mismatches=0`, and `Synthetic GPU packed-state remap=True`;
- all CFL, conservation, curvature, replacement-policy, kernel/resource, live-state, cleanup, and cache-immutability gates passed;
- the sole failed boolean was `Current cell resolves one texel-centre physical (s,n) point=False`;
- repository evidence: `StylizedRiverFoamRuntime.P8Diagnostics.cs::ValidateP8TopologyTransitionMapping` extracts `"void CaptureGeneratedTopologyTransition("`, but `CS_RiverFoam.compute` declares `void CaptureGeneratedTopology(uint3 dispatchId : SV_DispatchThreadID)` and that kernel calls both `FoamTexelCentreUV` and `ApplyGeneratedTopologyTransition`;
- approved P8b scope: `River_Foam_Active_Blockers_and_Next_Patches.md`, `River_Foam_Fixed_Metric_Dependency_Register.md`, `River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`, and `StylizedRiverFoamRuntime.P8Diagnostics.cs` only;
- implementation sequence: correct the extracted symbol; report explicit body-found state for `ApplyGeneratedTopologyTransition`, `CaptureGeneratedTopology`, and `ComposeTopology`; make missing inspected symbols an explicit validator-contract failure rather than an indirect physical-mapping failure; run parser/preprocessor, symbol declaration/extraction, scope, line-ending, full P8 regression, and package byte audits;
- acceptance: the validator must locate both production caller bodies and prove each constructs texel-centre UV before invoking the shared physical transition evaluator; no production file may differ from P8a; Unity exit remains one rerun of the existing P8 comprehensive report.
- implemented result: extraction now targets `CaptureGeneratedTopology`; missing shared/caller bodies explicitly block `currentPhysicalPoint`; the report prints body-found state for `ApplyGeneratedTopologyTransition`, `CaptureGeneratedTopology`, and `ComposeTopology`.
- post-change evidence: exact four-file scope; all production files byte-identical to P8a; four C# parser configurations passed; exact extraction returned non-empty body lengths `1236/843/7704`; targeted P8b audit passed 25/25; full P8 regression passed 44/44. Preliminary four-file archive extraction matched byte-for-byte; the final archive is rebuilt from this final documented state before delivery.

**Verification:**

- exact active legacy branch comparison;
- no-birth/death Presence, life-moment, and pattern-moment conservation;
- endpoint outflow accounting and no lateral/bank/obstacle leakage;
- forward/reverse-flow outlet swap;
- candidate downstream/lateral/total CFL and multi-substep count;
- widening/narrowing and obstacle-diversion synthetic fields;
- 40 m diameter-width / 40 m bend-radius curvature stress (`max abs(kappa*n)=0.5`, raw minimum `J=0.5`);
- exact fixed-lattice expansion/contraction remap, integer offsets, clipping, and incompatible clear policy;
- generated-topology physical previous-descriptor mapping;
- live cleanup, disabled bindings, queue invariants, and assigned-cache byte immutability.

**Stop conditions:** conservation or moment residual exceeds tolerance; an extra runtime substep violates the existing 64-pass budget; raw required Jacobian is non-positive; required 40 m stress needs clamping; exact remap duplicates/teleports/loses overlapping material; unsupported mappings are silently sampled; active legacy output changes; or the validator cannot prove live measurement before cleanup.

**Non-goals:** fixed allocation activation; source/film/shape/render migration; recipe/birth/cadence changes; topology generation; routing/Motion Lane/Disturbance allocation; scene/prefab/material/cache/serialized River changes.

**Rollback checkpoint:** `/mnt/data/p8base` — validated P7 source-unit baseline.

### 17.11 `RG-METRIC-P9` — Film occupancy, shape evaluation, and production rendering

**Objective:** Complete visual-layer and production-sampling migration.

**Required work:**

- migrate full-to-half mappings and represented physical area;
- handle odd dimensions and partial bank/padded film texels;
- migrate visual occupancy advection/support/source;
- migrate shape evaluation cell-scale clamps and noise aspect;
- replace production Foam field Y mapping with descriptor mapping;
- migrate metric visual offsets to UV;
- validate motion/routing/topology/state/film/shape texture sampling parity;
- preserve unrelated water rendering.

**Verification:** half-cell offset tests; bank edges; padded endpoint; odd dimensions; static/moving camera; production and debug modes; no block artifacts introduced by film; unrelated shader comparison.

**Stop conditions:** shader must alter unrelated shared behavior; film integrated area diverges; simulation/debug/render positions do not match.

**Rollback checkpoint:** complete simulation baseline before final render migration.

### 17.12 `RG-METRIC-P10` — Diagnostics, inspector semantics, and documentation

**Status:** `MECHANICALLY VERIFIED — UNITY VALIDATION PENDING`.

**Objective:** expose the completed P2–P9 coordinate contract compactly, reduce Inspector diagnostic noise, and remove stale fixed-metric status/documentation semantics without changing simulation, rendering, allocation, caches, or serialized River data.

#### Authorization and exact file scope

The user authorized P10 after the P9a Unity rerun returned `Overall: PASS`. The approved persistent scope is:

Documentation:

- `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`;
- `Docs/River_Foam_Fixed_Metric_Dependency_Register.md`;
- `Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`;
- `Docs/River_Foam_Stage6_Architecture.md`;
- `Docs/River_Rendering_Roadmap.md`.

Editor/runtime diagnostics:

- `Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs`;
- `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`;
- `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Diagnostics.cs`;
- `Game/Procedural/Rivers/Editor/StylizedRiverFoamTopologyCacheAssetEditor.cs`;
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs`;
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.CacheDiagnostics.cs`.

No file is created, deleted, moved, renamed, generated, or serialized. Production compute/HLSL/render code, `StylizedRiverFoamRuntime.P6Diagnostics.cs` through `.P9Diagnostics.cs`, Debug View definitions, scenes, prefabs, materials, cache assets, and `.meta` files are outside scope.

#### Reviewed evidence

- Supplied source authority is `Assets(72).zip` with Patch 09a overlaid. The archive has no `.git` directory, so branch, `HEAD`, status, and commit-history comparison are unavailable. The untouched extracted source and the overlaid working copy are the comparison baseline.
- The P9a Unity report dated `2026-07-17T20:30:54Z` passed all twelve ledger gates, including actual GPU Film Source, visual-occupancy, and shape paths; cleanup; and assigned-cache immutability. It ended `Overall: PASS`.
- `StylizedRiverEditor.Actions.cs::DrawFoamLayerACacheActions` currently mixes normal cache lifecycle, the current P9 gate, four closed comprehensive validators, the P5.1 two-build audit, and obstacle-baseline tools in one always-expanded block. The visible heading and main button remain P9-specific after P9 closure.
- `StylizedRiverFoamRuntime.CacheDiagnostics.cs` stores every explicit Foam report in one common nonserialized state/report/path contract. P6–P9 finalizers already use the same Passed/Failed semantics and latest-report lifecycle; changing those validated report bodies is not justified.
- `StylizedRiverFoamRuntime.PublicSurface.cs` already exposes active and fixed-candidate descriptor dimensions, spacing, lateral extent, cell count, and activation-deferred state. The Inspector does not currently present that evidence.
- P8 already records separate downstream/lateral CFL, raw/bounded Jacobian, and maximum `|κn|` fields internally. The public diagnostic surface exposes only total CFL, so the Inspector cannot display the completed metric ownership compactly.
- `StylizedRiverEditor.Diagnostics.cs` already owns compact source-area, cache, memory, dispatch, cell-iteration, transport-accounting, and shape evidence. No new Debug View or GPU readback is required.
- `StylizedRiverFoamTopologyCacheAssetEditor.cs` already supplies the read-only cache metadata UI previously deferred from P4. The P4 statements claiming that metadata remains hidden are stale documentation, not a current code defect.
- `StylizedRiverFoamTopologyCacheAssetEditor.cs` and `StylizedRiverFoamRuntime.CacheDiagnostics.cs` still direct users to the old `Foam Layer A Cache Tools` path; the renamed action group therefore requires text-only updates in both direct callers.
- `StylizedRiverEditor.DebugViews.cs`, `.Authoring.cs`, `.Foam.cs`, `.UI.cs`, and `StylizedRiver.cs` were reviewed. Current fixed-metric work does not justify a new view, serialized label change, or production authoring change.
- `Docs/handoff.md` is a generic handoff-production policy, not the live River Foam handoff. Modifying it would not improve fixed-metric status accuracy and is therefore excluded despite the older P10 placeholder list.

#### Invariants and non-goals

1. Active mapping remains `LegacyNormalizedAcross`; fixed-metric allocation and candidate selection remain deferred to P12.
2. No runtime update, dispatch, readback, resource, cache, transport, source, topology, film, shape, or render behavior changes.
3. No serialized property, default, label-backed value, scene, prefab, material, or cache payload changes.
4. No new Debug View and no warning-specific validator.
5. Closed P6–P9 validators remain callable, but historical/deep actions become collapsed by default instead of occupying the normal cache workflow.
6. Existing report files, report names, pass/fail ledgers, and cache-mutation proofs remain unchanged.
7. Inspector evidence is read-only and must not request asynchronous readback or cause asset reserialization.

#### File-by-file implementation sequence

1. **Canonical plan first:** record this evidence, exact scope, invariants, sequence, risks, and verification before implementation edits.
2. **Runtime public diagnostics:** expose read-only downstream/lateral CFL and curvature/Jacobian evidence already computed by P8. Add no new state or computation.
3. **Runtime Diagnostics Inspector:** add compact active-descriptor, fixed-candidate, allocation-comparison, split-CFL, Jacobian/curvature, memory, and dispatch rows by consuming existing/public fields. Reuse current sections; add no view.
4. **Actions Inspector:** rename the cache foldout to represent cache plus validation; separate normal cache lifecycle from current fixed-metric verification; collapse closed P5.1/P5.3/P6/P7/P8 tools under one historical/deep foldout; correct generic diagnostic-state wording.
5. **Direct diagnostic guidance:** update the cache-asset Inspector and default runtime diagnostic summary to the renamed River Inspector path; preserve all payload-analysis, report-state, and metadata behavior.
6. **Documentation:** replace the active-blockers history dump with one compact current ledger and P11 handoff; mark P9/P9a complete; mark P10 implementation state accurately; correct stale P4 metadata-UI statements; add concise fixed-metric status notes to Stage 6 architecture and the rendering roadmap.
7. **Post-change audit:** compare every changed file against the untouched extracted baseline, reread all changed files and direct diagnostic consumers, verify no out-of-scope file differs, and record final evidence here.

#### Risks and mitigations

- **Inspector repaint or serialization regression:** use only existing nonserialized Editor foldout state and read-only public properties; do not call `serializedObject.ApplyModifiedProperties`, `SetDirty`, cache build, or readback from the new rows.
- **False allocation claim:** label candidate/active counts as an allocation comparison, not physical waste, because active legacy and candidate fixed grids represent different contracts.
- **Diagnostic loss:** retain all existing P5.1/P5.3/P6/P7/P8/P9 methods and buttons; only the presentation hierarchy changes.
- **P-number ambiguity:** keep P labels where they identify immutable report contracts, while headings explain current versus historical ownership.
- **Scope drift into production:** verify compute kernels, HLSL, render shader, serialized River fields, cache assets, and Debug View files remain byte-identical.

#### Acceptance criteria

- P9 and P9a are documented as closed from the passing Unity report.
- Normal Inspector flow shows active mapping, fixed-candidate readiness, descriptor dimensions/spacing/extent, allocation comparison, split CFL, Jacobian/curvature, cache state, memory, and dispatch evidence compactly.
- Historical/deep actions are collapsed by default and remain fully available when opened.
- No new Debug View, GPU readback, persistent field, serialized value, resource, kernel, or production behavior exists.
- At P10 delivery, canonical documents agreed that P10 was the current non-behavioral cleanup and P11 was next; P11 now records the completed audit and releases P12.
- All changed C# files pass an available real parser in Editor and player preprocessing forms; introduced references resolve; multiline strings, braces, duplicate signatures, and required namespaces pass.
- Final scope and archive extraction are exact; Unity Inspector/compile verification is explicitly pending.

#### Implementation and post-change audit

P10 is implemented in the exact eleven-file scope recorded above. The final source state provides read-only split-CFL and curvilinear-metric properties, compact active/candidate grid evidence, a cache-versus-validation action hierarchy, a collapsed historical/deep diagnostic group, and corrected user-facing action paths. No validator body, report ledger, compute/render path, Debug View, serialized field, resource, scene, prefab, material, cache asset, or `.meta` file changed.

Mechanical evidence:

```text
Primary P10 audit:                 110 passed / 0 failed
Independent final audit:             35 passed / 0 failed
Exact changed files:                11
Changed C# files:                    6
Changed C# parser configurations:   24
River C# files parsed:              89
River methods indexed:           1,644
Invocation expressions indexed: 13,704
Files added / removed:             0 / 0
```

The audit proved exact scope, LF/UTF-8 integrity, raw plus Editor/development/release parser coverage, no duplicate exact River method signatures, resolution of all new public diagnostic references, preservation and declaration of every P5.1/P5.3/P6/P7/P8/P9 action target, byte identity of P6-P9 report implementations and production compute/HLSL/render/Debug View files, absence of new serialization/readback/dispatch/resource behavior, and removal of stale operational action labels. Unity remains the authority for compilation, Inspector layout, no-dirty/no-reserialization behavior, and the unchanged P9 endpoint rerun.

#### Unity verification

1. Import with zero C# and shader/compute errors or warnings introduced by P10.
2. Open `Runtime Diagnostics → Foam` and verify the new descriptor/CFL/curvature/resource rows display without changing any serialized value.
3. Open `Actions → Foam Cache & Validation`; confirm historical/deep diagnostics are collapsed by default and every previous action remains available inside the foldout.
4. Inspect the River and assigned cache asset without saving; confirm neither becomes dirty or reserialized.
5. Run the unchanged P9 comprehensive report once and require `Overall: PASS`.

**Stop conditions:** any serialized-value change, unexpected asset dirtiness, missing historical action, new continuous readback, report regression, production diff, or contradiction between Inspector labels and runtime ownership.

**Rollback checkpoint:** the P9a-overlaid supplied source plus the passing final P9 report.

### 17.13 `RG-METRIC-P11` — Mechanical verification and full consistency audit

**Status:** `MECHANICALLY VERIFIED AND COMPLETE`.

**Objective:** prove that the complete P2-P10 implementation is internally consistent before fixed-metric activation and Unity candidate selection.

#### Reviewed evidence

The read-only review used the exact post-P10a source reconstructed from the user-supplied `Assets(72).zip` plus the accepted P9a, P10, and P10a archives. No Git metadata was present in the supplied snapshot, so comparison authority is the byte-exact supplied baseline and accepted patch archives.

The audit reread and indexed:

- all 89 C# files under `Game/Procedural/Rivers`;
- all 24 compute, HLSL, and River render-shader files under `Game/Rendering/Water/Resources/PS3DRiver`;
- the grid descriptor, allocation, binding, cache codec/fingerprint, topology, source, obstacle, transport, replacement, film, shape, production render, and Inspector endpoint owners;
- the five canonical fixed-metric documents;
- the final Unity 6000.5.0f1 P9 report dated `2026-07-17T22:11:11Z`, which ended `Overall: PASS` after actual GPU film-source, visual-occupancy, and shape execution;
- the supplied P10 Inspector screenshot, which shows the expected Edit Mode post-cleanup unallocated state and the consolidated Foam diagnostics surface.

#### Exact implementation scope

P11 changes documentation only:

1. `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`;
2. `Docs/River_Foam_Fixed_Metric_Dependency_Register.md`;
3. `Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`;
4. `Docs/River_Foam_Stage6_Architecture.md`;
5. `Docs/River_Rendering_Roadmap.md`.

No C#, compute, HLSL, render shader, Debug View, resource, kernel, serialized field, cache codec/payload, scene, prefab, material, asset, or `.meta` file is modified.

#### Invariants and non-goals

- active runtime mapping remains `LegacyNormalizedAcross`;
- fixed candidate activation remains P12;
- no source recipe, birth budget, update cadence, topology, transport, film, shape, or render formula changes;
- no diagnostic action, GPU readback, warning-specific validator, or Inspector control is added;
- pre-existing mixed line endings in two untouched Disturbance files are recorded but not normalized or included in scope;
- historical legacy formulas remain only where explicitly owned by compatibility wrappers, fallback branches, or closed diagnostics.

#### Audit result

```text
Primary P11 audit:                           24 passed / 0 failed
River C# files:                              89
C# parser configurations:                   356
C# syntax nodes inspected:            2,043,155
River methods indexed:                    1,641
C# 9 multiline interpolation defects:         0
Missing known namespace/imports:               0
Duplicate exact method signatures:             0
Compute/HLSL/render files:                     24
Local shader includes:                         26
Foam kernels:                             23 / 23
FindKernel order mismatches:                    0
CPU/GPU structured-buffer ABI contracts: 10 / 10
Literal Foam property contracts:          207 / 207
Stale production normalized-Y formulas:         0
Unexpected structural-spacing owners:           0
Scene/prefab/material/cache/meta changes:        0
```

The ABI audit verified the 80-byte five-`float4` grid descriptor and nine additional CPU/GPU structured-buffer contracts. Kernel pragmas, function bodies, thread-group declarations, and C# `FindKernel` order match exactly. Descriptor lanes are declared once and bound to both compute and production material paths. Cache format `3`, generator contract `4`, descriptor serialization, and descriptor fingerprint identity remain consistent.

The structural-Y search found one normalized formula only in the closed P6 diagnostic where it is the explicit expected legacy-renderer value. All nine direct spacing derivations are accounted for as legacy compatibility constructors/fallbacks, one non-grid opportunity-distribution spacing, closed diagnostics, or explicit fixed/legacy branches. No migrated production consumer retains independent normalized lateral reconstruction or unguarded duplicate structural spacing ownership.

Two Disturbance source files contain pre-existing mixed line endings. Their bytes are identical to the supplied baseline and they are outside fixed-metric and P11 change scope. P11 preserves them unchanged.

#### P10 Unity closure evidence

The post-P10a Unity run compiled sufficiently to render the consolidated Inspector and execute the unchanged P9 endpoint. The report proved:

```text
Fixed candidate: True; status=Ready; activation deferred
Actual GPU finite-volume advection: True
Live runtime state remained untouched: PASS
Diagnostic cleanup and disabled bindings: PASS
Assigned cache remained unchanged: PASS
Overall: PASS
```

P10 and P10a are therefore closed. P11 introduces no executable change and requires no additional Unity rerun.

**Completion condition:** met. There is no unresolved syntax, reference, ABI, stale-formula, scope, or canonical-document contradiction in the fixed-metric candidate.

**Rollback checkpoint:** the Unity-validated post-P10a implementation plus this documentation-only P11 closure record.

### 17.14 `RG-METRIC-P12` — Unity candidate sweep and visual/performance selection

**Objective:** Select final Foam metric quality mapping using actual runtime evidence.

**Required work:**

- import and compile in Unity 6000.5.0f1/D3D11;
- prepare exact caches through tooling;
- run candidate 0.25/0.20/0.15/0.10 m sweeps;
- capture six compact validation groups: coordinate/debug parity, source families, topology/obstacles/disturbance, transport/replacement, final render, performance/memory;
- test 5/10/20/40 m widths and straight/curved/asymmetric cases without raw scene edits, using approved test harness or temporary nonserialized/editor test methods;
- measure CPU, GPU, memory, dispatch, cache generation, CFL, and curvature;
- compare against frozen baseline.

**Completion condition:** one mapping per quality is selected or the plan records why tier values remain provisional.

**Stop conditions:** no candidate preserves accepted source appearance; performance exceeds approved threshold; broad-river curvature policy fails; cache/preflight fails.

**Rollback checkpoint:** mechanically verified candidates and Unity evidence, before final tuning commit.

### 17.15 `RG-METRIC-P13` — Final tier tuning, cache freeze, and contiguous baseline closure

**Objective:** Freeze the production-ready contiguous metric baseline.

**Required work:**

- record final Low/Medium/High requested/resolved policy;
- make only evidence-backed source/topology tuning;
- regenerate exact validation caches through tooling;
- repeat mechanical and Unity regression after final values;
- complete post-implementation compliance audit;
- mark all Stage 1 items complete or explicitly deferred;
- publish limitations: contiguous length, width/waste, curvature bounds, no active-area scaling;
- create immutable rollback baseline before Stage 2 experimentation.

**Completion condition:** all Stage 1 acceptance gates pass and documentation exactly matches implementation.

**Stop conditions:** any final tuning invalidates prior test evidence; unresolved dependency-register item; unverified Unity behavior.

## Implementation record — `RG-METRIC-P9 — Film occupancy, shape evaluation, and production rendering`

### Current objective

Complete the remaining inactive fixed-metric visual-layer and renderer-coordinate migration while preserving exact active `LegacyNormalizedAcross` behavior. P9 owns full-to-half film mapping, represented physical area, visual-occupancy transport geometry, shape/film alignment, production Foam field UV, visual metre-offset conversion, and production/debug same-point parity. Fixed-metric allocation remains deliberately deferred.

### Accepted prerequisite

The final P8 comprehensive report returned `Overall: PASS`. It proved live current-cache installation without build/write, active legacy ownership, fixed-candidate readiness, conservative packed transport, CFL/substep policy, curvilinear area/face metrics, exact GPU persistent-state remap with `1,491` overlap cells and zero mismatches, physical generated-topology transition mapping, cleanup, and assigned-cache immutability. P8 is closed.

### Reviewed evidence

- `StylizedRiverFoamGridDescriptor` defines film dimensions as `ceil(structural/2)` but the compute film path currently maps film texel centres with ordinary normalized UV. Odd structural edges therefore do not resolve the exact centre or represented area of their one-cell terminal groups.
- `BuildFoamFilmSource` samples only one normalized point per film texel. It does not area-average the one-to-four represented structural cells, so bank-edge, padded-endpoint, odd-width, and odd-height film texels cannot preserve integrated physical coverage.
- `AdvanceFoamVisualOccupancy` derives film spacing from `fieldLength/filmWidth` and local width/filmHeight. That is not the fixed lattice's exact two-cell grouping, does not represent partial odd-edge cells, and omits the P8 curvature-aware aggregate area/face contract.
- `EvaluateFoamShape` samples the film texture with structural field UV directly. Standard half-resolution UV is only centre-aligned for even dimensions; odd terminal film groups require explicit structural-to-film mapping.
- `ApplyBoundary` still uses the legacy simulation-column helper and its dispatch path does not bind the descriptor at the immediate consumer boundary.
- `RiverWaterFoam.hlsl::RiverWaterEvaluateFoam` reconstructs field Y as `lateralMetres/surfaceHalfWidth`, and converts visual metre offsets with local surface width. Both are incompatible with the fixed centreline lattice.
- `SH_CleanStylizedRiver.shader` independently reconstructs legacy Foam UV before evaluation and samples shape/film/debug textures with structural UV rather than descriptor-owned field/film coordinates.
- `RiverWaterFoamVelocity.hlsl`, unrelated water lighting/refraction/disturbance composition, serialized River fields, and existing resource ownership do not require implementation changes.
- The supplied workspace contains no `.git` metadata. `/mnt/data/p9base` is the immutable validated P8b source checkpoint and `/mnt/data/p9work` is the P9 implementation workspace.

### Approved P9 file scope

Documentation:

- `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
- `Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
- `Assets/Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`

Editor/runtime:

- `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Compute.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.P9Diagnostics.cs` plus `.meta`

Compute/rendering:

- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Coordinates.hlsl`
- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`
- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl`
- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader`

### P9 implementation contract

1. Active legacy film, shape, and production-render formulas remain on exact legacy branches.
2. A film texel represents structural ranges `[2i, min(2i+2, fullCount))`; its centre, represented count, physical extent, and area derive from those exact ranges. Odd terminal groups represent one structural row/column without half-cell drift.
3. Fixed film source is the physical-area-weighted average of the represented structural cells. Invalid bank/obstacle/padded cells contribute zero coverage but remain part of represented area, preserving integrated coverage.
4. Fixed visual-occupancy advection uses aggregate P8 cell area and face lengths for the exact represented structural ranges. Legacy advection remains formula-compatible.
5. Structural-to-film sampling uses one explicit piecewise mapping that respects partial terminal groups. Shape evaluation and renderer film/debug sampling share that mapping.
6. Fixed production field UV maps `(global s,n)` through the five-lane descriptor. Metre warp/stretch offsets convert through allocated X length and fixed lateral row span. Legacy `surfaceHalfWidth` behavior remains unchanged.
7. Fixed production sampling rejects padded longitudinal endpoints and coordinates outside the represented lateral interval rather than saturating onto edge material.
8. Normal production Foam state, shape, film, Motion Lane/routing debug, and film debug views resolve the same physical river point.
9. One Inspector-triggered P9 report validates live preparation, exact legacy ownership, odd/even film grouping, integrated area, bank/padded coverage, visual transport geometry, shape mapping, production/debug source identity, unrelated-shader invariants, cleanup, and cache immutability.

### Non-goals

No fixed-metric activation; no source tuning, topology generation, transport-state policy, disturbance allocation, quality-tier selection, birth budget, cadence, scene, prefab, material, cache asset, serialized River-field, lighting, refraction, reflection, wetness, riverbed, or non-Foam shader behavior change. P9 adds no persistent texture, buffer, or per-frame readback.

### File-by-file sequence

1. Record this plan and dependency disposition before implementation.
2. Add exact film-group/field-to-film coordinate helpers and fixed-only physical aggregation.
3. Migrate film source, support spacing, visual occupancy geometry, shape sampling, and boundary descriptor ownership.
4. Add descriptor-owned production field UV, valid-field clipping, metre-offset conversion, and renderer film/debug mapping.
5. Add one live non-storing P9 comprehensive report and Inspector action.
6. Run parser/preprocessor/API/signature/kernel/resource/legacy-equivalence/numeric/source-inspection/package audits and record results here.

### Acceptance and stop conditions

P9 may be delivered for Unity validation only if exact legacy reference comparisons pass; odd/even film groups cover every structural cell exactly once; represented area and area-weighted source integrate within tolerance; fixed film advection geometry matches aggregate structural geometry; production/debug mappings resolve identical physical points; unrelated shader regions are byte-identical; no persistent resource or serialized state is added; and the validator proves live-state-before-cleanup semantics. A mismatch in integrated area, a half-cell shift, padded-edge sampling, or an unrelated water-shader diff blocks delivery.

### Post-change consistency and compliance audit

- Exactly the 11 approved P9 paths differ from the immutable validated P8b baseline. No scene, prefab, material, cache asset, serialized River field, persistent texture, persistent buffer, kernel, or resource declaration changed.
- Active `LegacyNormalizedAcross` film, support, shape, boundary, field-UV, and metre-offset formulas remain on explicit legacy branches. Fixed-only branches own exact two-cell film groups, odd terminal groups, represented physical area, aggregate P8 area/face geometry, descriptor field coordinates, and valid-field clipping.
- The three changed/new C# files parse with zero syntax/missing-node errors in raw, Editor, development-player, and release-player preprocessing. An independent project scan parsed all 89 River C# files, found no duplicate exact signatures within a containing type, and found no introduced project-method arity mismatch. Unity compilation remains unavailable and is not claimed.
- All introduced HLSL functions have matching declaration/call arity; braces, parentheses, and preprocessors are balanced; the 23 existing compute kernels retain exact order; `CS_RiverFoam.Resources.hlsl`, shader property declarations, and all shader text outside the approved Foam block remain byte-identical.
- Numeric validation passed exact film grouping for every structural dimension from 1 through 2,048, 10,000 randomized represented-area cases, and 100,000 fixed renderer cell-centre mappings.
- The P9 report uses the validated live resource transaction and no cache-build path. It executes the actual `BuildFoamFilmSource`, `AdvanceFoamVisualOccupancy`, and `EvaluateFoamShape` kernels on temporary resources, checks exact source symbols using absolute project paths, measures before cleanup, and separately proves cleanup, disabled bindings, live-reference immutability, and assigned-cache byte immutability.
- Primary mechanical validation returned 92/92 passes. The independent final audit returned 68/68 passes. The final changed-files archive extracted with all 11 files byte-identical to this documented state.

### Implementation state

`COMPLETE — UNITY VALIDATED`. The initial P9 report passed every ledger gate. P9a removed the three D3D11 warning-prone visual-occupancy helper forms without changing the formulas, and the post-P9a rerun again passed actual GPU film source, visual occupancy, shape mapping, production/debug mapping, resource ownership, cleanup, live-state immutability, assigned-cache immutability, and `Overall: PASS`.

## 18. Source-family preservation cards

### 18.1 Shore Ribbon

**Preserve:** progressive shoreline-aligned ribbons, accepted length/lifecycle, bank attachment, no interior rectangular slab.

**Migrate:** structural Y placement, dispatch bounds, thickness/variation unit semantics, bank clipping, source-area diagnostics.

**Failure signatures:** width triples solely because `Cells` was retained; ribbon leaves bank; offset varies with river width; debug/production mismatch.

### 18.2 Inward Wash

**Preserve:** authored length, width, inward reach, offsets, initial Presence/life, breakup behavior, progressive formation.

**Migrate:** fixed-metric cell-centre sampling, source bounds, bank-origin mapping, clipping through width changes.

**Failure signatures:** reach changes with quality; wash crosses opposite bank; longitudinal block remains; flow reversal mirrors incorrectly.

### 18.3 Object Contact Arc

**Preserve:** exact mesh-fitted front path, both thin front halves, two downstream arms, no rear/upstream bridge, event ownership, Build/Hold/Release/Rest, no breakup, accepted signed offsets.

**Migrate:** local normal spacing, physical core/feather, metric bounds, global-Y culling, object contour sampling into metric cells.

**Failure signatures:** O/near-O wrap, rear-centre cell, widened arms, missing front, detached pieces, source rectangle, field contact texture becoming authority again.

### 18.4 Object Contact Semi-Arc

**Preserve:** exactly one physical-front half plus one arm, deterministic side, front present from first Build through final Release, arm releases before front, no opposite half.

**Migrate:** same coordinate/profile concerns as Arc.

**Failure signatures:** complete connector, second arm, rear wrap, front gap, side flip, arm-only state.

### 18.5 Object Contact Fleck

**Preserve:** accepted stochastic contact use, size/life/presence, object-contact-field authority where intended.

**Migrate:** contact field metric raster, event bounds, physical length/width/offset.

**Failure signatures:** density changes by width without policy; flecks appear behind excluded geometry; event cap saturation.

### 18.6 Free Water Lace Connector

**Preserve:** metre-authored length/width, curvature, formation, support relation, life/presence/breakup.

**Migrate:** placement, event culling, structural raster, topology/negative support sampling.

**Failure signatures:** anisotropic width, quality-dependent curvature, disconnected source, bank leakage.

### 18.7 Free Water Cross-Lace Connector

**Preserve:** cross-lace geometry and intended wider/local connectivity.

**Migrate:** same as Lace, including orientation and physical bounds.

**Failure signatures:** cross becomes stretched block; one branch clipped by normalized assumptions; object-proximity policy changes.

### 18.8 Free Water Torn Fragment

**Preserve:** metre-authored fragment bounds, lifecycle, breakup, formation, spatial distribution.

**Migrate:** fixed-metric raster and budget/density validation.

**Failure signatures:** fragments collapse to one oversized cell or become excessively numerous solely from additional cells.

### 18.9 Manual injection and probes

**Preserve:** exact physical ellipse/stroke dimensions, clear ranges, compound behavior, isolated lifetime test semantics.

**Migrate:** all Y coordinates and dispatch culling.

**Failure signatures:** centre/bank placement drift, clear leaves stale cells, probe selects different physical location after rebuild.

## 19. Numerical validation specification

### 19.1 Mapping round trip

For selected cells and metric points:

```text
cell -> metric -> fractional cell
metric -> nearest cell -> metric centre
```

Record maximum errors separately for `s` and `n`. Expected cell-centre round trip should be exact within floating-point tolerance; arbitrary metric point error must be bounded by half a cell plus numerical tolerance.

### 19.2 CPU/GPU parity

Upload or derive a deterministic test set and compare:

- metric centre;
- local/world position;
- normalized bank-relative values where retained;
- source-distance calculations;
- obstacle interval membership;
- render UV.

### 19.3 Conservation

With birth, death, support aging, negative aging, clipping, and endpoint outflow disabled or isolated:

```text
sum(Presence)
sum(Presence * Life)
sum(Presence * PatternMoment)
```

must remain within an adopted numerical tolerance after transport. With endpoint outflow enabled, loss must equal measured outflow within tolerance.

### 19.4 Physical area

For a rectangular approximation:

```text
cell area = dx * dy
source physical area ≈ affected fractional coverage * cell area
```

For a corrected curvature policy, use the adopted Jacobian-aware area.

### 19.5 CFL

Report:

```text
CFLx = maxDownstreamSpeed * dt / dx
CFLy = maxLateralSpeed * dt / dy
CFLtotal = CFLx + CFLy
substeps = ceil(CFLtotal / target), clamped by policy
```

Use actual runtime speed inputs and minimum relevant spacing.

### 19.6 Allocation waste

```text
allocatedCells = columnCount * rowCount
validWaterCells = count(valid bank and valid length)
waste = 1 - validWaterCells / allocatedCells
```

Report obstacle-excluded cells separately; they are physical water allocation, not rectangle waste.

### 19.7 Curvature error

```text
relative longitudinal scale difference = abs(kappa*n)
J = 1 - kappa*n
```

Record worst valid cell and compare to adopted policy.

## 20. Visual and integration scenario matrix

Every final candidate must be evaluated against all categories below. Existing scene assets are read-only validation inputs.

### 20.1 Width/length geometry

- approximately 5 m straight river;
- 10 m, 20 m, and 40 m controlled widths;
- width expanding gradually;
- width contracting gradually;
- abrupt but valid width change;
- asymmetric left/right widths;
- just under/exactly/just over 32 m length;
- multiple padded chunks;
- near contiguous cache dimension limit.

### 20.2 Curvature/orientation

- straight;
- gentle left/right bend;
- tight valid bend;
- broad river on bend;
- reversed flow;
- domain rebuild with same geometry;
- centreline orientation consistency.

### 20.3 Obstacles

- tiny obstacle smaller than one candidate cell;
- narrow long obstacle;
- broad short obstacle;
- rotated obstacle;
- sloped/exact-mesh silhouette;
- obstacle touching bank;
- two nearby obstacles;
- hidden renderer with active simulation;
- obstacle dirty refresh and cache change.

### 20.4 Source lifecycle

For every automatic family:

- first visible Build step;
- intermediate Build;
- Hold;
- early and late Release;
- Rest;
- seed variation;
- low/high authoring ranges;
- source clipping at bank, obstacle, padding, and field edge.

### 20.5 State and topology

- no topology;
- major only;
- connector only;
- pocket only;
- combined topology;
- shore support;
- pressure support;
- wake negative aging;
- topology replacement;
- cache exact/stale/missing/corrupt.

### 20.6 Rendering

- Automatic Birth Sources live view;
- topology views;
- obstacle exclusion/contact/routing;
- Motion Lane/canonical velocity;
- material state;
- film occupancy;
- final shape;
- final Foam render;
- moving and static cameras;
- current isometric gameplay distance.

## 21. Validation gate format

User-facing Unity validation after each deliverable must contain at most six numbered steps. Internally, evidence may be comprehensive, but the requested user actions must remain compact.

Recommended six grouped gates:

1. **Compile/cache:** zero errors; prepare exact cache; confirm descriptor/cache diagnostics.
2. **Coordinates/topology:** inspect grid, banks, topology, obstacles, routing, and Disturbance alignment.
3. **Sources:** inspect all source families, with Arc/Semi-Arc lifecycle emphasized.
4. **Transport/state:** run conservation, CFL, replacement, flow reversal, and endpoint checks.
5. **Final visual:** compare final Foam at fixed camera and settings against baseline.
6. **Performance:** capture CPU/GPU/memory/dispatch/cell/substep evidence for required width cases.

## 22. Rollback and preservation strategy

### 22.1 Baselines

Create immutable references for:

- pre-migration current accepted repository state;
- current accepted Arc/Semi-Arc source geometry;
- descriptor foundation with old behavior;
- metric one-strip topology baseline;
- metric source baseline;
- mechanically verified full candidate;
- Unity-validated final contiguous baseline.

### 22.2 Rollback granularity

Changes must be structured so these concerns can be reverted independently where practical:

- documentation/plan;
- descriptor foundation;
- allocation/mapping;
- cache contract;
- topology/obstacles;
- sources;
- transport;
- rendering;
- diagnostics/tuning.

### 22.3 User-owned changes

Never use destructive Git commands or broad checkout/restore. Preserve all pre-existing modifications. Do not overwrite dirty files without first recording and integrating their current content.

### 22.4 Generated assets

Generated cache files are never hand-edited. Regenerate only through approved editor actions. Scene and prefab changes required only to refresh caches or serialized code defaults are left to approved tooling/manual action and are not silently included in code patches.

## 23. Documentation migration

### 23.1 Documents to update

- `River_Foam_Active_Blockers_and_Next_Patches.md`
- `River_Foam_Stage6_Architecture.md`
- `River_Rendering_Roadmap.md`
- `handoff.md` where current continuation state is maintained
- this upgrade plan
- the companion dependency register when live review finds additional dependencies

### 23.2 Required documentation outcomes

- Layer A owns the metric descriptor and mappings.
- Layer F owns allocation, limits, strips, scheduling, and budgets.
- normalized lateral structural mapping is marked superseded.
- quality values are described as Foam-specific metric targets/resolved spacing.
- source parameters have explicit units.
- cache contract and preparation behavior are current.
- Stage 1 limitations and Stage 2 requirements are explicit.
- stale active-patch queue language is removed.
- accepted Arc/Semi-Arc behavior remains documented.
- performance claims distinguish measured, calculated, and future behavior.

### 23.3 No endless temporary logs

Validation evidence belongs in concise ledgers and referenced files. Do not append unbounded raw logs to canonical architecture documents.

## 24. Definition of Stage 1 complete

Stage 1 may be declared complete only when:

1. the live repository review and baseline package are recorded;
2. every modified file is within approved scope and traceable to a plan item;
3. every dependency-register item is dispositioned;
4. one descriptor controls all CPU/GPU/render/cache mappings;
5. no structural normalized-Y formula remains active;
6. all source families pass physical and lifecycle regression;
7. topology, obstacles, routing, motion, Disturbance sampling, transport, film, shape, and rendering align;
8. cache preparation and build preflight pass with exact metric caches and reject old caches;
9. conservation, CFL, curvature, memory, dispatch, and performance evidence pass adopted thresholds;
10. final Low/Medium/High policy is recorded;
11. no scene/prefab/material raw edit occurred;
12. C#, HLSL, Unity import, runtime, visual, profiler, and compliance audits pass;
13. canonical documentation matches final behavior;
14. the contiguous limitations are recorded honestly;
15. an immutable rollback baseline is preserved.

Writing code, compiling mechanically, or obtaining one acceptable screenshot is insufficient.

## 25. Definition of production-scalable complete

The broader fixed-metric program may be called production-scalable only after Stage 2 also passes:

- local-width strip allocation;
- shared global-Y intervals;
- cross-strip conservative transport;
- source/topology/obstacle events spanning strips;
- renderer lookup without seams;
- exact strip cache preparation;
- active/offscreen scheduling;
- explicit global cell/memory/dispatch budgets;
- many-river stress validation;
- connected-component orientation compatibility where transfer is enabled;
- no silent metric degradation;
- measured cost proportional to active represented water area within the adopted scheduling policy.

# Appendix A — Dependency-area traceability matrix

This matrix maps every top-level dependency-register area to implementation phases. `T` means mandatory regression testing even if no code changes.

| Dependency area | Primary phase(s) | Required outcome |
|---|---|---|

| Grid allocation and dimensions | `P2/P3/P12` | Descriptor-derived dimensions and candidate evidence |

| CPU field-space conversion | `P3/P11` | Metric round-trip and no normalized structural Y |

| Compute coordinate conversion | `P2/P5/P11` | GPU parity with CPU centres |

| Production renderer sampling | `P9/P12` | Same descriptor and no local-width Y remap |

| Topology generation | `P5/P12` | Physical topology preservation |

| Boundary generation | `P5/P9` | Metric shore/bank support |

| Obstacle exclusion | `P5` | Exact mesh aligned to metric cells |

| Obstacle routing | `P6` | Physical reaches and no wrap |

| Motion Lane | `P6/P9` | Physical wavelength/scroll and shared sampling |

| Automatic birth scheduling/budgets | `P7/P12` | Density and capacities across area |

| Automatic source geometry | `P7/P12` | Eight families preserved |

| Manual injection/probes | `P7` | Physical placement/range |

| Persistent transport | `P8` | Conservation and equal lateral metre neighbours |

| CFL/substeps | `P8/P12` | Correct components and accepted count |

| Curvilinear metrics | `P8/P12` | Bounded or corrected broad-bend policy |

| Topology replacement | `P8` | Physical descriptor remap or explicit clear |

| Half-resolution occupancy | `P9` | Odd-edge area/alignment |

| Shape/breakup/noise | `P9/P12` | Physical scale and no structural holes |

| Disturbance integration | `P6/P12 (T)` | Same physical-point sampling; no Disturbance migration |

| River domain/geometry | `P3/P5/P12 (T)` | Widths, curvature, direction, endpoints valid |

| Quality policy | `P2/P12/P13` | Foam-only metric mapping |

| Birth budgets/capacities | `P7/P12` | No area-driven starvation/saturation |

| Resource lifecycle | `P3/P8` | Deterministic rebuild/state policy |

| CPU/GPU layout | `P2/P11` | Stride/order parity |

| Cache/fingerprint | `P4` | Deterministic incompatibility and exact cache |

| Cache tooling/preflight | `P4/P12` | Explicit preparation and release gate |

| Diagnostics/metrics | `P10` | Physical and allocation evidence |

| Memory/performance | `P10/P12/P13` | Measured approved budgets |

| Debug views | `P9/P10/P12` | World-aligned parity |

| Inspector/serialization | `P7/P10` | Explicit units and persistence |

| Documentation | `P1/P10/P13` | No stale contract |

| Scene/assets | `P12 (T only)` | Read-only validation; tooling-only caches |

| Future strip allocation | `RG-STRIP phases` | Active/local-area scalability |

# Appendix B — Exact file review/change/test map

The paths below were present in the supplied snapshot. Final live scope must be revalidated. Classification from the dependency register is retained; phase assignment is the expected first owning phase, not permission to edit.

| Class | Path | Expected owning phase | Planned disposition |
|---:|---|---|---|

| U/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Binding.cs` | `P2-P12 review` | Expected update, plus full regression |

| R/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthDiagnostics.cs` | `P7` | Mandatory review; update only if final contract requires |

| U/D/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs` | `P7` | Expected update, plus full regression |

| R/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthTransfer.cs` | `P7` | Mandatory review; update only if final contract requires |

| U/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Compute.cs` | `P2-P12 review` | Expected update, plus full regression |

| U/D/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs` | `P2/P3` | Expected update, plus full regression |

| U/R/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Evolution.Connector.cs` | `P7` | Expected update, plus full regression |

| U/R/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Evolution.FreeWater.cs` | `P7` | Expected update, plus full regression |

| R/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Evolution.HostedNegative.Pose.cs` | `P7` | Mandatory review; update only if final contract requires |

| U/R/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Evolution.HostedNegative.cs` | `P7` | Expected update, plus full regression |

| U/R/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Evolution.Major.cs` | `P7` | Expected update, plus full regression |

| U/R/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Evolution.Shared.cs` | `P7` | Expected update, plus full regression |

| U/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs` | `P7` | Expected update, plus full regression |

| U/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs` | `P8` | Expected update, plus full regression |

| U/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs` | `P2/P3` | Expected update, plus full regression |

| U/D/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Obstacles.cs` | `P5/P6` | Expected update, plus full regression |

| U/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs` | `P2-P12 review` | Expected update, plus full regression |

| U/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs` | `P2/P3` | Expected update, plus full regression |

| U/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.RuntimeUpdates.cs` | `P8` | Expected update, plus full regression |

| U/D/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.State.cs` | `P2-P12 review` | Expected update, plus full regression |

| U/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Topology.cs` | `P5` | Expected update, plus full regression |

| U/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.TopologyCache.cs` | `P4` | Expected update, plus full regression |

| U/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.TopologyReplacement.cs` | `P8` | Expected update, plus full regression |

| R/T | `Game/Procedural/Rivers/StylizedRiverFoamRuntime.cs` | `P2-P12 review` | Mandatory review; update only if final contract requires |

| R/T | `Game/Procedural/Rivers/StylizedRiverFoamSimulation.cs` | `P2-P12 review` | Mandatory review; update only if final contract requires |

| R/T | `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamConnectorTopology.cs` | `P5` | Mandatory review; update only if final contract requires |

| U/D/T | `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamConnectorTopologyGenerator.cs` | `P5` | Expected update, plus full regression |

| R/T | `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamMajorCandidate.cs` | `P5` | Mandatory review; update only if final contract requires |

| R/D/T | `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamMajorCandidateGenerator.cs` | `P5` | Mandatory review; update only if final contract requires |

| R/T | `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamMajorTopology.cs` | `P5` | Mandatory review; update only if final contract requires |

| U/D/T | `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamMajorTopologyGenerator.cs` | `P5` | Expected update, plus full regression |

| R/T | `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamPocketTopology.cs` | `P5` | Mandatory review; update only if final contract requires |

| U/D/T | `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamPocketTopologyGenerator.cs` | `P5` | Expected update, plus full regression |

| U/R/T | `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyCacheAsset.cs` | `P4` | Expected update, plus full regression |

| U/D/T | `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyCacheCodec.cs` | `P4` | Expected update, plus full regression |

| U/T | `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyFieldSpace.cs` | `P2/P3` | Expected update, plus full regression |

| U/T | `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyFingerprints.cs` | `P4` | Expected update, plus full regression |

| U/T | `Game/Procedural/Rivers/RiverObstacleExclusionResolver.cs` | `P5/P6` | Expected update, plus full regression |

| U/T | `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Coordinates.hlsl` | `P5-P9 (kernel-specific)` | Expected update, plus full regression |

| U/R/T | `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Evolution.hlsl` | `P7` | Expected update, plus full regression |

| U/R/T | `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Motion.hlsl` | `P5-P9 (kernel-specific)` | Expected update, plus full regression |

| R/D/T | `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Noise.hlsl` | `P9` | Mandatory review; update only if final contract requires |

| U/T | `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl` | `P5-P9 (kernel-specific)` | Expected update, plus full regression |

| U/R/T | `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Sampling.hlsl` | `P5-P9 (kernel-specific)` | Expected update, plus full regression |

| U/D/T | `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Simulation.hlsl` | `P8` | Expected update, plus full regression |

| U/T | `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Structs.hlsl` | `P5-P9 (kernel-specific)` | Expected update, plus full regression |

| U/R/T | `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Support.hlsl` | `P5-P9 (kernel-specific)` | Expected update, plus full regression |

| U/R/T | `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Topology.hlsl` | `P5` | Expected update, plus full regression |

| U/T | `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.TopologyTransition.hlsl` | `P8` | Expected update, plus full regression |

| R/T | `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Transport.hlsl` | `P8` | Mandatory review; update only if final contract requires |

| U/D/T | `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute` | `P5-P9 (kernel-specific)` | Expected update, plus full regression |

| U/T | `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl` | `P9` | Expected update, plus full regression |

| R/T | `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoamVelocity.hlsl` | `P9` | Mandatory review; update only if final contract requires |

| U/R/T | `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader` | `P9` | Expected update, plus full regression |

| U/R/T | `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs` | `P7/P10` | Expected update, plus full regression |

| U/T | `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Authoring.cs` | `P7/P10` | Expected update, plus full regression |

| U/T | `Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs` | `P10` | Expected update, plus full regression |

| U/T | `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Diagnostics.cs` | `P10` | Expected update, plus full regression |

| T | `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Disturbances.cs` | `P7/P10` | Mandatory integration/validation; no automatic edit |

| U/D/T | `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs` | `P7/P10` | Expected update, plus full regression |

| R/T | `Game/Procedural/Rivers/Editor/StylizedRiverEditor.UI.cs` | `P7/P10` | Mandatory review; update only if final contract requires |

| R/T | `Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs` | `P7/P10` | Mandatory review; update only if final contract requires |

| U/R/T | `Game/Procedural/Rivers/Editor/StylizedRiverFoamBuildPreflight.cs` | `P4` | Expected update, plus full regression |

| U/R/T | `Game/Procedural/Rivers/Editor/StylizedRiverFoamDevelopmentCacheCoordinator.cs` | `P4` | Expected update, plus full regression |

| U/D/T | `Game/Procedural/Rivers/StylizedRiver.cs` | `P2/P7/P10` | Expected update, plus full regression |

| T/R | `Game/Procedural/Rivers/RiverDomainSnapshot.cs` | `P3/P5/P12 (test)` | Mandatory review; update only if final contract requires |

| T/R | `Game/Procedural/Rivers/StylizedRiverGeometry.cs` | `P3/P5/P12 (test)` | Mandatory review; update only if final contract requires |

| T | `Game/Procedural/Rivers/StylizedRiverCorridorGeometry.cs` | `P3/P5/P12 (test)` | Mandatory integration/validation; no automatic edit |

| T | `Game/Procedural/Rivers/StylizedRiverDomainDebug.cs` | `P3/P5/P12 (test)` | Mandatory integration/validation; no automatic edit |

| T/R | `Game/Procedural/Rivers/RiverDisturbanceFootprintResolver.cs` | `P6/P12 (test unless interface requires)` | Mandatory review; update only if final contract requires |

| T/R | `Game/Procedural/Rivers/StylizedRiverDisturbanceEmitter.cs` | `P6/P12 (test unless interface requires)` | Mandatory review; update only if final contract requires |

| T | `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.cs` | `P6/P12 (test unless interface requires)` | Mandatory integration/validation; no automatic edit |

| T | `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Binding.cs` | `P6/P12 (test unless interface requires)` | Mandatory integration/validation; no automatic edit |

| T | `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Compute.cs` | `P6/P12 (test unless interface requires)` | Mandatory integration/validation; no automatic edit |

| T | `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Constants.cs` | `P6/P12 (test unless interface requires)` | Mandatory integration/validation; no automatic edit |

| T | `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.ContinuousSources.cs` | `P6/P12 (test unless interface requires)` | Mandatory integration/validation; no automatic edit |

| T | `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Contracts.cs` | `P6/P12 (test unless interface requires)` | Mandatory integration/validation; no automatic edit |

| T | `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Diagnostics.cs` | `P6/P12 (test unless interface requires)` | Mandatory integration/validation; no automatic edit |

| T | `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Dispatch.cs` | `P6/P12 (test unless interface requires)` | Mandatory integration/validation; no automatic edit |

| T | `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.GeneratedSources.cs` | `P6/P12 (test unless interface requires)` | Mandatory integration/validation; no automatic edit |

| T | `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Impact.cs` | `P6/P12 (test unless interface requires)` | Mandatory integration/validation; no automatic edit |

| T | `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Members.cs` | `P6/P12 (test unless interface requires)` | Mandatory integration/validation; no automatic edit |

| T | `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.PublicSurface.cs` | `P6/P12 (test unless interface requires)` | Mandatory integration/validation; no automatic edit |

| T | `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Resources.cs` | `P6/P12 (test unless interface requires)` | Mandatory integration/validation; no automatic edit |

| T | `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Ripple.cs` | `P6/P12 (test unless interface requires)` | Mandatory integration/validation; no automatic edit |

| T | `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.SourcePathMath.cs` | `P6/P12 (test unless interface requires)` | Mandatory integration/validation; no automatic edit |

| T | `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.State.cs` | `P6/P12 (test unless interface requires)` | Mandatory integration/validation; no automatic edit |

| T | `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.StaticPressure.cs` | `P6/P12 (test unless interface requires)` | Mandatory integration/validation; no automatic edit |

| T | `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.StaticWake.cs` | `P6/P12 (test unless interface requires)` | Mandatory integration/validation; no automatic edit |

| T, no raw edit | `Game/Demo/Scenes/VisualFrameworkDemo.unity` | `P12 (validation only)` | Mandatory review; update only if final contract requires |

| U | `Docs/River_Foam_Active_Blockers_and_Next_Patches.md` | `P1/P10/P13` | Expected update, plus full regression |

| U | `Docs/River_Foam_Stage6_Architecture.md` | `P1/P10/P13` | Expected update, plus full regression |

| U | `Docs/River_Rendering_Roadmap.md` | `P1/P10/P13` | Expected update, plus full regression |

| R/U | `Docs/handoff.md` | `P1/P10/P13` | Expected update, plus full regression |

# Appendix C — Compute kernel plan

| Kernel | Owning phase | Required proof |
|---|---|---|

| `ClearRange` | `P7` | Metric range/addressing and exact clearing |

| `InjectFoam` | `P7` | Manual source physical placement/extent |

| `RasterizeFoamSourceEvent` | `P7` | Metric centres, source units, dispatch bounds |

| `RasterizeFoamSourceEventDebug` | `P7/P10` | Exact parity with production source raster |

| `WriteIsolatedLifeProbe` | `P7` | Physical probe placement |

| `ClearAutomaticBirthDebugAll` | `P10` | Complete descriptor dimensions |

| `BuildCurrentShoreEdges` | `P5` | Metric bank edge/support thickness |

| `ComposeTopology` | `P5` | Topology layers, valid bank/padding |

| `CaptureGeneratedTopology` | `P4/P5` | Cache/readback dimensions and parity |

| `BuildEvolvingMajorSupport` | `P5` | Metric extents/support widths |

| `ClearObstacleExclusion` | `P5` | Complete dimensions |

| `UpdateObstacleExclusion` | `P5` | Metric obstacle intervals |

| `BuildFoamObjectContactField` | `P5/P6/P7` | Metric neighbour/contact and pressure alignment |

| `ResetTopologyMetrics` | `P10` | Metric buffer reset |

| `MeasureTopologyMetrics` | `P10` | Physical area/perimeter semantics |

| `ResetTransportMetrics` | `P8/P10` | Transport metrics reset |

| `SimulateFoam` | `P8` | Conservation, CFL, area, curvature policy |

| `BuildFoamFilmSource` | `P9` | Full-to-half mapping and area |

| `BuildFoamFilmSupport` | `P9` | Film support alignment |

| `AdvanceFoamVisualOccupancy` | `P9` | Metric advection and represented area |

| `EvaluateFoamShape` | `P9` | Structural/film alignment and noise scale |

| `ApplyBoundary` | `P5/P9` | Valid-bank and padded clipping |

# Appendix D — Exhaustive Foam authoring/public-property unit review

This inventory is generated from public Foam properties in the supplied `StylizedRiver.cs`. It is a review checklist, not a claim that every property is serialized directly under the same name.

| Property | Current apparent unit/role | Planned treatment |
|---|---|---|

| `FoamNotifications` | review required | Trace exact unit and dependency before implementation; source line 275 in snapshot |

| `FoamEnabled` | enum/boolean policy | Preserve serialized semantics; source line 2353 in snapshot |

| `FoamStateHeld` | review required | Trace exact unit and dependency before implementation; source line 2354 in snapshot |

| `FoamTopologyCacheAsset` | review required | Trace exact unit and dependency before implementation; source line 2356 in snapshot |

| `FoamMajorSupportAmount` | review required | Trace exact unit and dependency before implementation; source line 2358 in snapshot |

| `FoamMajorSupportSize` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2360 in snapshot |

| `FoamMajorSupportSizeVariation` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2362 in snapshot |

| `FoamMajorRecycleTerritoryDeviationPercent` | review required | Trace exact unit and dependency before implementation; source line 2364 in snapshot |

| `FoamMajorLifetimeUnits` | review required | Trace exact unit and dependency before implementation; source line 2369 in snapshot |

| `FoamMajorLifetimeUnitDeviation` | review required | Trace exact unit and dependency before implementation; source line 2371 in snapshot |

| `FoamMajorSupportSeed` | review required | Trace exact unit and dependency before implementation; source line 2373 in snapshot |

| `FoamConnectorAmount` | review required | Trace exact unit and dependency before implementation; source line 2375 in snapshot |

| `FoamConnectorDirectness` | review required | Trace exact unit and dependency before implementation; source line 2377 in snapshot |

| `FoamConnectorLengthPreference` | review required | Trace exact unit and dependency before implementation; source line 2379 in snapshot |

| `FoamConnectorBreakStretchRatio` | review required | Trace exact unit and dependency before implementation; source line 2381 in snapshot |

| `FoamInteriorPocketAmount` | review required | Trace exact unit and dependency before implementation; source line 2383 in snapshot |

| `FoamEdgeCavityAmount` | review required | Trace exact unit and dependency before implementation; source line 2385 in snapshot |

| `FoamConnectorWeakSpanAmount` | review required | Trace exact unit and dependency before implementation; source line 2387 in snapshot |

| `FoamFreeWaterEventAmount` | event budget/count | Review area scaling and event-cap saturation; source line 2389 in snapshot |

| `FoamAutomaticBirthEnabled` | enum/boolean policy | Preserve serialized semantics; source line 2391 in snapshot |

| `FoamSourcePopulationPreset` | review required | Trace exact unit and dependency before implementation; source line 2392 in snapshot |

| `FoamAutomaticShoreBirthEnabled` | enum/boolean policy | Preserve serialized semantics; source line 2394 in snapshot |

| `FoamAutomaticShoreBirthActive` | review required | Trace exact unit and dependency before implementation; source line 2396 in snapshot |

| `FoamAutomaticObjectBirthEnabled` | enum/boolean policy | Preserve serialized semantics; source line 2400 in snapshot |

| `FoamAutomaticObjectBirthActive` | review required | Trace exact unit and dependency before implementation; source line 2402 in snapshot |

| `FoamAutomaticFreeWaterBirthEnabled` | enum/boolean policy | Preserve serialized semantics; source line 2406 in snapshot |

| `FoamAutomaticFreeWaterBirthActive` | review required | Trace exact unit and dependency before implementation; source line 2408 in snapshot |

| `FoamSourcePopulationPresetImplemented` | review required | Trace exact unit and dependency before implementation; source line 2412 in snapshot |

| `FoamShoreFoamCoverage` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2425 in snapshot |

| `FoamShoreFoamActivity` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2427 in snapshot |

| `FoamShoreFoamPatchSize` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2429 in snapshot |

| `FoamShoreFoamFormationSpeedMetresPerSecond` | physical rate | Preserve metres/second; verify grid conversion uses descriptor; source line 2431 in snapshot |

| `FoamShoreFoamPattern` | enum/boolean policy | Preserve serialized semantics; source line 2436 in snapshot |

| `FoamShoreFoamSize` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2438 in snapshot |

| `FoamShoreRibbonPatternWeight` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2440 in snapshot |

| `FoamInwardWashPatternWeight` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2442 in snapshot |

| `FoamShoreRibbonFormationSpeedMultiplier` | dimensionless multiplier | Preserve; verify base physical formation speed; source line 2444 in snapshot |

| `FoamShoreRibbonLengthMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2446 in snapshot |

| `FoamShoreRibbonLengthMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2450 in snapshot |

| `FoamShoreRibbonThicknessCells` | structural cells | Mandatory unit decision: physical geometry -> metres; raster support may remain cells; source line 2452 in snapshot |

| `FoamShoreRibbonOffsetMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2454 in snapshot |

| `FoamShoreRibbonOffsetVariationCells` | structural cells | Mandatory unit decision: physical geometry -> metres; raster support may remain cells; source line 2456 in snapshot |

| `FoamShoreRibbonInitialPresenceMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2458 in snapshot |

| `FoamShoreRibbonInitialPresenceMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2462 in snapshot |

| `FoamShoreRibbonInitialLifeMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2466 in snapshot |

| `FoamShoreRibbonInitialLifeMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2470 in snapshot |

| `FoamShoreRibbonBreakupStrengthMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2474 in snapshot |

| `FoamShoreRibbonBreakupStrengthMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2478 in snapshot |

| `FoamInwardWashFormationSpeedMultiplier` | dimensionless multiplier | Preserve; verify base physical formation speed; source line 2482 in snapshot |

| `FoamInwardWashLengthMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2484 in snapshot |

| `FoamInwardWashLengthMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2488 in snapshot |

| `FoamInwardWashWidthMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2490 in snapshot |

| `FoamInwardWashWidthMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2494 in snapshot |

| `FoamInwardWashReachMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2496 in snapshot |

| `FoamInwardWashReachMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2500 in snapshot |

| `FoamInwardWashOffsetMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2502 in snapshot |

| `FoamInwardWashOffsetMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2506 in snapshot |

| `FoamInwardWashInitialPresenceMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2508 in snapshot |

| `FoamInwardWashInitialPresenceMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2512 in snapshot |

| `FoamInwardWashInitialLifeMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2516 in snapshot |

| `FoamInwardWashInitialLifeMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2520 in snapshot |

| `FoamInwardWashBreakupStrengthMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2524 in snapshot |

| `FoamInwardWashBreakupStrengthMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2528 in snapshot |

| `FoamObjectFoamCoverage` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2533 in snapshot |

| `FoamObjectContactCycleCoverage` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2535 in snapshot |

| `FoamObjectFoamActivity` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2537 in snapshot |

| `FoamObjectFoamFormationSpeedMetresPerSecond` | physical rate | Preserve metres/second; verify grid conversion uses descriptor; source line 2539 in snapshot |

| `FoamObjectContactHoldDurationMinSeconds` | time seconds | Preserve time semantics; regression-test lifecycle/cadence; source line 2544 in snapshot |

| `FoamObjectContactHoldDurationMaxSeconds` | time seconds | Preserve time semantics; regression-test lifecycle/cadence; source line 2548 in snapshot |

| `FoamObjectContactReleaseDurationMinSeconds` | time seconds | Preserve time semantics; regression-test lifecycle/cadence; source line 2552 in snapshot |

| `FoamObjectContactReleaseDurationMaxSeconds` | time seconds | Preserve time semantics; regression-test lifecycle/cadence; source line 2556 in snapshot |

| `FoamObjectContactRestDurationMinSeconds` | time seconds | Preserve time semantics; regression-test lifecycle/cadence; source line 2560 in snapshot |

| `FoamObjectContactRestDurationMaxSeconds` | time seconds | Preserve time semantics; regression-test lifecycle/cadence; source line 2564 in snapshot |

| `FoamObjectFoamPattern` | enum/boolean policy | Preserve serialized semantics; source line 2568 in snapshot |

| `FoamObjectContactCyclesEnabled` | enum/boolean policy | Preserve serialized semantics; source line 2570 in snapshot |

| `FoamObjectContactArcPatternWeight` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2575 in snapshot |

| `FoamObjectContactFleckPatternWeight` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2577 in snapshot |

| `FoamObjectContactArcFormationSpeedMultiplier` | dimensionless multiplier | Preserve; verify base physical formation speed; source line 2579 in snapshot |

| `FoamObjectContactArcArmReachMin` | ambiguous/current source semantic | Trace producer/consumer; preserve serialization; classify metre/normalized/cell before edit; source line 2581 in snapshot |

| `FoamObjectContactArcArmReachMax` | ambiguous/current source semantic | Trace producer/consumer; preserve serialization; classify metre/normalized/cell before edit; source line 2585 in snapshot |

| `FoamObjectContactArcLengthMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2589 in snapshot |

| `FoamObjectContactArcLengthMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2593 in snapshot |

| `FoamObjectContactArcWakeArmLengthMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2595 in snapshot |

| `FoamObjectContactArcWakeArmLengthMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2597 in snapshot |

| `FoamObjectContactArcAlongFlowContactOffsetMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2599 in snapshot |

| `FoamObjectContactArcAcrossRiverContactOffsetMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2601 in snapshot |

| `FoamObjectContactArcWidthMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2603 in snapshot |

| `FoamObjectContactArcWidthMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2607 in snapshot |

| `FoamObjectContactArcOffsetMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2609 in snapshot |

| `FoamObjectContactArcOffsetMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2613 in snapshot |

| `FoamObjectContactArcInitialPresenceMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2615 in snapshot |

| `FoamObjectContactArcInitialPresenceMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2619 in snapshot |

| `FoamObjectContactArcInitialLifeMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2623 in snapshot |

| `FoamObjectContactArcInitialLifeMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2627 in snapshot |

| `FoamObjectContactArcBreakupStrengthMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2631 in snapshot |

| `FoamObjectContactArcBreakupStrengthMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2635 in snapshot |

| `FoamObjectContactSemiArcPatternWeight` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2639 in snapshot |

| `FoamObjectContactSemiArcFormationSpeedMultiplier` | dimensionless multiplier | Preserve; verify base physical formation speed; source line 2641 in snapshot |

| `FoamObjectContactSemiArcArmReachMin` | ambiguous/current source semantic | Trace producer/consumer; preserve serialization; classify metre/normalized/cell before edit; source line 2643 in snapshot |

| `FoamObjectContactSemiArcArmReachMax` | ambiguous/current source semantic | Trace producer/consumer; preserve serialization; classify metre/normalized/cell before edit; source line 2647 in snapshot |

| `FoamObjectContactSemiArcLengthMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2651 in snapshot |

| `FoamObjectContactSemiArcLengthMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2655 in snapshot |

| `FoamObjectContactSemiArcWakeArmLengthMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2657 in snapshot |

| `FoamObjectContactSemiArcWakeArmLengthMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2659 in snapshot |

| `FoamObjectContactSemiArcAlongFlowContactOffsetMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2661 in snapshot |

| `FoamObjectContactSemiArcAcrossRiverContactOffsetMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2663 in snapshot |

| `FoamObjectContactSemiArcWidthMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2665 in snapshot |

| `FoamObjectContactSemiArcWidthMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2669 in snapshot |

| `FoamObjectContactSemiArcOffsetMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2671 in snapshot |

| `FoamObjectContactSemiArcOffsetMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2675 in snapshot |

| `FoamObjectContactSemiArcInitialPresenceMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2677 in snapshot |

| `FoamObjectContactSemiArcInitialPresenceMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2681 in snapshot |

| `FoamObjectContactSemiArcInitialLifeMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2685 in snapshot |

| `FoamObjectContactSemiArcInitialLifeMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2689 in snapshot |

| `FoamObjectContactSemiArcBreakupStrengthMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2693 in snapshot |

| `FoamObjectContactSemiArcBreakupStrengthMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2697 in snapshot |

| `FoamObjectContactSemiArcLopsidednessMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2701 in snapshot |

| `FoamObjectContactSemiArcLopsidednessMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2705 in snapshot |

| `FoamObjectContactFleckFormationSpeedMultiplier` | dimensionless multiplier | Preserve; verify base physical formation speed; source line 2709 in snapshot |

| `FoamObjectContactFleckLengthMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2711 in snapshot |

| `FoamObjectContactFleckLengthMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2715 in snapshot |

| `FoamObjectContactFleckWidthMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2717 in snapshot |

| `FoamObjectContactFleckWidthMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2721 in snapshot |

| `FoamObjectContactFleckOffsetMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2723 in snapshot |

| `FoamObjectContactFleckOffsetMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2727 in snapshot |

| `FoamObjectContactFleckInitialPresenceMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2729 in snapshot |

| `FoamObjectContactFleckInitialPresenceMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2733 in snapshot |

| `FoamObjectContactFleckInitialLifeMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2737 in snapshot |

| `FoamObjectContactFleckInitialLifeMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2741 in snapshot |

| `FoamObjectContactFleckBreakupStrengthMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2745 in snapshot |

| `FoamObjectContactFleckBreakupStrengthMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2749 in snapshot |

| `FoamFreeWaterFoamCoverage` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2754 in snapshot |

| `FoamFreeWaterFoamActivity` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2756 in snapshot |

| `FoamFreeWaterFoamFormationSpeedMetresPerSecond` | physical rate | Preserve metres/second; verify grid conversion uses descriptor; source line 2758 in snapshot |

| `FoamFreeWaterFoamPattern` | enum/boolean policy | Preserve serialized semantics; source line 2763 in snapshot |

| `FoamFreeWaterLaceConnectorPatternWeight` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2765 in snapshot |

| `FoamFreeWaterCrossLaceConnectorPatternWeight` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2767 in snapshot |

| `FoamFreeWaterTornFragmentPatternWeight` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2769 in snapshot |

| `FoamFreeWaterLaceFormationSpeedMultiplier` | dimensionless multiplier | Preserve; verify base physical formation speed; source line 2771 in snapshot |

| `FoamFreeWaterLaceLengthMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2773 in snapshot |

| `FoamFreeWaterLaceLengthMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2777 in snapshot |

| `FoamFreeWaterLaceWidthMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2779 in snapshot |

| `FoamFreeWaterLaceWidthMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2783 in snapshot |

| `FoamFreeWaterLaceInitialPresenceMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2785 in snapshot |

| `FoamFreeWaterLaceInitialPresenceMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2789 in snapshot |

| `FoamFreeWaterLaceInitialLifeMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2793 in snapshot |

| `FoamFreeWaterLaceInitialLifeMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2797 in snapshot |

| `FoamFreeWaterLaceBreakupStrengthMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2801 in snapshot |

| `FoamFreeWaterLaceBreakupStrengthMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2805 in snapshot |

| `FoamFreeWaterLaceCurvatureMin` | dimensionless/shape curvature control | Preserve intended shape semantics; test against metric aspect; source line 2809 in snapshot |

| `FoamFreeWaterLaceCurvatureMax` | dimensionless/shape curvature control | Preserve intended shape semantics; test against metric aspect; source line 2813 in snapshot |

| `FoamFreeWaterCrossLaceFormationSpeedMultiplier` | dimensionless multiplier | Preserve; verify base physical formation speed; source line 2817 in snapshot |

| `FoamFreeWaterCrossLaceLengthMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2819 in snapshot |

| `FoamFreeWaterCrossLaceLengthMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2823 in snapshot |

| `FoamFreeWaterCrossLaceWidthMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2825 in snapshot |

| `FoamFreeWaterCrossLaceWidthMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2829 in snapshot |

| `FoamFreeWaterCrossLaceInitialPresenceMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2831 in snapshot |

| `FoamFreeWaterCrossLaceInitialPresenceMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2835 in snapshot |

| `FoamFreeWaterCrossLaceInitialLifeMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2839 in snapshot |

| `FoamFreeWaterCrossLaceInitialLifeMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2843 in snapshot |

| `FoamFreeWaterCrossLaceBreakupStrengthMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2847 in snapshot |

| `FoamFreeWaterCrossLaceBreakupStrengthMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2851 in snapshot |

| `FoamFreeWaterFragmentFormationSpeedMultiplier` | dimensionless multiplier | Preserve; verify base physical formation speed; source line 2855 in snapshot |

| `FoamFreeWaterFragmentLengthMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2857 in snapshot |

| `FoamFreeWaterFragmentLengthMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2861 in snapshot |

| `FoamFreeWaterFragmentWidthMinMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2863 in snapshot |

| `FoamFreeWaterFragmentWidthMaxMetres` | physical metres | Preserve physical value; migrate raster/mapping only; source line 2867 in snapshot |

| `FoamFreeWaterFragmentInitialPresenceMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2869 in snapshot |

| `FoamFreeWaterFragmentInitialPresenceMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2873 in snapshot |

| `FoamFreeWaterFragmentInitialLifeMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2877 in snapshot |

| `FoamFreeWaterFragmentInitialLifeMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2881 in snapshot |

| `FoamFreeWaterFragmentBreakupStrengthMin` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2885 in snapshot |

| `FoamFreeWaterFragmentBreakupStrengthMax` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2889 in snapshot |

| `FoamShoreFoamStrength` | review required | Trace exact unit and dependency before implementation; source line 2893 in snapshot |

| `FoamShoreFoamPersistence` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2895 in snapshot |

| `FoamNeutralLifetime` | review required | Trace exact unit and dependency before implementation; source line 2897 in snapshot |

| `FoamSupportedAgingRate` | review required | Trace exact unit and dependency before implementation; source line 2902 in snapshot |

| `FoamFullSupportedAgingAt` | review required | Trace exact unit and dependency before implementation; source line 2907 in snapshot |

| `FoamFinalVisibilityMode` | review required | Trace exact unit and dependency before implementation; source line 2912 in snapshot |

| `FoamNegativeAgingRate` | review required | Trace exact unit and dependency before implementation; source line 2917 in snapshot |

| `FoamDownstreamSpeedRatio` | review required | Trace exact unit and dependency before implementation; source line 2922 in snapshot |

| `FoamMaximumLateralSpeedRatio` | review required | Trace exact unit and dependency before implementation; source line 2927 in snapshot |

| `FoamLaneAdvectionRatio` | review required | Trace exact unit and dependency before implementation; source line 2932 in snapshot |

| `FoamLowLateralMotionCoverage` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2937 in snapshot |

| `FoamDirectionChangeFrequency` | review required | Trace exact unit and dependency before implementation; source line 2942 in snapshot |

| `FoamAcrossRiverCoherence` | review required | Trace exact unit and dependency before implementation; source line 2947 in snapshot |

| `FoamObstacleSlowdownStrength` | review required | Trace exact unit and dependency before implementation; source line 2952 in snapshot |

| `FoamObstacleMinimumDownstreamFactor` | review required | Trace exact unit and dependency before implementation; source line 2957 in snapshot |

| `FoamVisualOccupancyBuildTime` | review required | Trace exact unit and dependency before implementation; source line 2962 in snapshot |

| `FoamVisualOccupancyReleaseTime` | review required | Trace exact unit and dependency before implementation; source line 2967 in snapshot |

| `FoamColour` | review required | Trace exact unit and dependency before implementation; source line 2972 in snapshot |

| `FoamInteriorOpacityFloor` | review required | Trace exact unit and dependency before implementation; source line 2973 in snapshot |

| `FoamEdgeContrast` | review required | Trace exact unit and dependency before implementation; source line 2975 in snapshot |

| `FoamChipActivation` | review required | Trace exact unit and dependency before implementation; source line 2977 in snapshot |

| `FoamChipCandidateSpacing` | review required | Trace exact unit and dependency before implementation; source line 2979 in snapshot |

| `FoamChipSize` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 2984 in snapshot |

| `FoamChipIrregularity` | review required | Trace exact unit and dependency before implementation; source line 2986 in snapshot |

| `FoamChipStableScreenRadiusPixels` | review required | Trace exact unit and dependency before implementation; source line 2988 in snapshot |

| `FoamChipMaximumViewScale` | review required | Trace exact unit and dependency before implementation; source line 2993 in snapshot |

| `FoamChipEdgeWidthPixels` | review required | Trace exact unit and dependency before implementation; source line 2998 in snapshot |

| `FoamChipInteriorAccess` | review required | Trace exact unit and dependency before implementation; source line 3000 in snapshot |

| `FoamChipFieldSpeed` | review required | Trace exact unit and dependency before implementation; source line 3002 in snapshot |

| `FoamChipFormationTime` | review required | Trace exact unit and dependency before implementation; source line 3007 in snapshot |

| `FoamChipStableTime` | review required | Trace exact unit and dependency before implementation; source line 3012 in snapshot |

| `FoamChipDissolveTime` | review required | Trace exact unit and dependency before implementation; source line 3017 in snapshot |

| `FoamChipDormantTime` | review required | Trace exact unit and dependency before implementation; source line 3022 in snapshot |

| `FoamChipLateralMotionAmount` | review required | Trace exact unit and dependency before implementation; source line 3027 in snapshot |

| `FoamChipLateralMotionSpeed` | review required | Trace exact unit and dependency before implementation; source line 3032 in snapshot |

| `FoamChipRotationAmountDegrees` | review required | Trace exact unit and dependency before implementation; source line 3037 in snapshot |

| `FoamChipRotationSpeed` | review required | Trace exact unit and dependency before implementation; source line 3042 in snapshot |

| `FoamChipSizePulseAmount` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 3047 in snapshot |

| `FoamChipSizePulseSpeed` | dimensionless/lifecycle/composition | No coordinate-unit conversion; regression-test density, lifecycle, and appearance; source line 3052 in snapshot |

| `FoamChipShapeChangeAmount` | review required | Trace exact unit and dependency before implementation; source line 3057 in snapshot |

| `FoamChipShapeChangeSpeed` | review required | Trace exact unit and dependency before implementation; source line 3059 in snapshot |

| `FoamChipShapeTransitionTime` | review required | Trace exact unit and dependency before implementation; source line 3064 in snapshot |

| `FoamStrandStrength` | review required | Trace exact unit and dependency before implementation; source line 3069 in snapshot |

| `FoamStrandScale` | review required | Trace exact unit and dependency before implementation; source line 3071 in snapshot |

| `FoamStrandDensity` | review required | Trace exact unit and dependency before implementation; source line 3073 in snapshot |

| `FoamStrandReach` | review required | Trace exact unit and dependency before implementation; source line 3075 in snapshot |

| `FoamDebugView` | review required | Trace exact unit and dependency before implementation; source line 3077 in snapshot |

| `FoamSpawnDistanceNormalized` | review required | Trace exact unit and dependency before implementation; source line 3078 in snapshot |

| `FoamSpawnAcrossNormalized` | review required | Trace exact unit and dependency before implementation; source line 3080 in snapshot |

| `FoamSpawnScale` | review required | Trace exact unit and dependency before implementation; source line 3082 in snapshot |

| `FoamSpawnAmount` | review required | Trace exact unit and dependency before implementation; source line 3087 in snapshot |

| `FoamSpawnRemainingLife` | review required | Trace exact unit and dependency before implementation; source line 3088 in snapshot |

| `FoamSpawnRibbonDuration` | time seconds | Preserve time semantics; source line 3089 in snapshot |

| `FoamSpawnRibbonTravelDistance` | review required | Trace exact unit and dependency before implementation; source line 3094 in snapshot |

| `FoamSpawnRibbonAcrossDrift` | review required | Trace exact unit and dependency before implementation; source line 3099 in snapshot |

| `FoamSpawnRibbonPathWander` | review required | Trace exact unit and dependency before implementation; source line 3104 in snapshot |

# Appendix E — Decision ledger

| Decision | Planned resolution | Status before implementation |
|---|---|---|
| Coordinate orientation | Centreline-relative river `s/n` | Accepted |
| World-aligned XZ field | Rejected | Closed |
| Stage 1 representation | One strip using final lattice semantics | Accepted |
| Lateral strip alignment | Shared phase and global-Y indices within river/network | Accepted |
| Shared quality enum | Retain serialized enum; Foam-specific metric mapping | Accepted |
| Final quality target values | Select after 0.25/0.20/0.15/0.10 Unity sweep | Decision required after evidence |
| Square versus independent `dx/dy` | Descriptor supports both; initial candidates square unless evidence says otherwise | Planned |
| Lateral lattice phase | Prefer centreline-centred global Y zero; verify even-row assumptions | Review/decision gate P2 |
| Guard rows | Derive from documented stencil/support needs; no arbitrary padding | Decision gate P3/P5 |
| Source visible widths | Prefer metres; keep cells only for discrete raster support | Per-parameter decision P7 |
| Arc/Semi coefficients | Re-establish accepted physical profile, not accidental anisotropic coefficient result | Decision P7/P12 |
| Birth budget scaling | Measure density/cap saturation across width; adopt explicit policy | Decision P7/P12 |
| Curvature | Bounded approximation or corrected metric before broad-width claim | Decision P8/P12 |
| Cache payload bump | Only if binary layout changes | Decision P4 |
| Old cache compatibility | Deterministic rejection | Accepted |
| Oversize field behavior | No silent metric degradation | Accepted |
| Scene/prefab/material edits | Prohibited unless separately explicit | Accepted |
| Disturbance field resolution | Unchanged; physical-point integration testing | Accepted |
| Stage 1 active-area scaling claim | Prohibited | Accepted |
| Stage 2 storage representation | Select after measured design review | Future decision |

# Appendix F — Pre-edit checklist

- [ ] Read all live agent instructions.
- [ ] Confirm branch, HEAD, upstream, and status.
- [ ] Inventory every pre-existing dirty path.
- [ ] Verify this plan and dependency register match live paths/symbols.
- [ ] Read complete Stage 1 expected files and direct dependencies.
- [ ] Inspect relevant history and accepted/superseded Foam versions.
- [ ] Reconcile active queue and current Arc/Semi state.
- [ ] Capture source/debug/render/performance baseline.
- [ ] Record exact approved files and implementation authorization.
- [ ] Confirm no scene/prefab/material/cache raw edit is in scope.

# Appendix G — Final compliance checklist

- [ ] Every diff hunk maps to an active plan item.
- [ ] No unapproved path changed.
- [ ] No user-owned change lost or overwritten.
- [ ] One descriptor owns all coordinate values.
- [ ] No active normalized structural-Y formula remains.
- [ ] No duplicate `dx/dy` derivation remains.
- [ ] CPU/GPU/render/cache parity passed.
- [ ] All 22 kernels dispositioned and validated.
- [ ] All eight automatic source families passed.
- [ ] Manual injections/probes passed.
- [ ] Topology/boundary/obstacle/routing/Motion Lane passed.
- [ ] Disturbance same-point integration passed without unintended migration.
- [ ] Transport conservation/CFL/curvature/replacement passed.
- [ ] Film/shape/final rendering passed.
- [ ] Cache exact/stale/missing/corrupt/preflight passed.
- [ ] Inspector values persist and units are explicit.
- [ ] Memory/CPU/GPU/dispatch evidence passed.
- [ ] C# real parser/compiler and namespace scan passed.
- [ ] HLSL/compute parsing and Unity import passed.
- [ ] Scene/prefab/material policy respected.
- [ ] Canonical docs match implementation.
- [ ] Stage 1 limitations recorded.
- [ ] Immutable final rollback baseline preserved.

# Appendix H — Immediate next action after documentation patch 01

The next implementation step is to finish the blocked live portion of `RG-METRIC-P0`, not code. Use this plan and the dependency register against the actual Git workspace, record branch/HEAD/upstream/status/diffs/history and the immutable rollback reference, then capture the required Unity baseline package. Only after that evidence is recorded may `RG-METRIC-P2` be changed from `NOT STARTED` to `READY` and receive an exact approved implementation-file scope.


## Implementation record — `RG-METRIC-P12a — Committed-state temporal presentation and candidate evidence`

### Status

Implementation complete; Unity compilation, shader import, visual confirmation, and expanded P12 snapshot remain pending.

### Objective and observed evidence

The first Unity fixed candidate activated the full migrated production path and passed its machine-verifiable runtime snapshot. The supplied `Material Presence` video contains 172 frames over 6.239567 seconds and shows abrupt whole-cell edge changes. Source inspection identified the presentation discontinuity:

- `StylizedRiverFoamRuntime.Lifecycle.cs` forced `simulationInterpolation = 1f` after every update;
- `StylizedRiverFoamRuntime.Injection.cs::SimulateFullField` already assigns `previousState = currentState` before one complete material tick and publishes the newly committed state through the existing ping-pong swap;
- `RiverWaterFoam.hlsl::RiverWaterFoamSampleInterpolatedState` already performs packed previous/current interpolation for Final Foam;
- `SH_CleanStylizedRiver.shader::SampleCommittedFoamState` bypassed that helper and sampled `_FoamCurrent` directly for Layer C and Motion Field diagnostics.

The lateral-response observation is separate. `Maximum Lateral Speed Ratio` owns the canonical lateral speed ceiling. `Lane Advection Ratio` only scrolls the generated Motion Lane phase downstream and does not amplify signed lateral intent. P12a therefore records the generated lane distribution before deciding whether any generator or transport change is justified.

### Invariants and non-goals

- Preserve conservative Layer C transport, lifecycle, topology, source, and cache state exactly.
- Reuse the existing previous/current ARGBHalf textures; add no resource, kernel, dispatch, persistent allocation, serialized field, or cache contract.
- Interpolate packed Presence, life moment, and pattern moment at the unchanged field coordinate. Do not restore velocity reconstruction, point backtracing, obstacle prediction, or any cross-face presentation path.
- State Hold, initialization, topology-replacement hold, resource reset, and direct probe paths retain exact-current alpha `1` where already required.
- Do not change Motion Lane generation or transport. Added lane statistics are observational only.
- Do not change automatic/manual source budgets, lifecycle controls, shape, visibility, or overall Final Foam amount. Excessive coverage remains a P13 layer-by-layer tuning item.
- Add no Debug View.
- Retain report files and add an adjacent clipboard-copy action for every Foam report action.

### Approved file scope

Canonical documentation:

1. `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
3. `Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
4. `Docs/River_Foam_Stage6_Architecture.md`
5. `Docs/River_Rendering_Roadmap.md`

Editor and diagnostics:

6. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`
7. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs`
8. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Diagnostics.cs`
9. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.P12Diagnostics.cs`
10. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs`

Runtime presentation and evidence:

11. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs`
12. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs`
13. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Obstacles.cs`
14. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.RuntimeUpdates.cs`
15. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader`
16. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl`

No file is created, removed, renamed, generated, or serialized.

### File-by-file implementation sequence

1. Record P12a evidence, scope, invariants, acceptance, and deferred amount tuning in the five canonical documents.
2. Resolve `simulationInterpolation` from `simulationAccumulator / stepDuration` after fixed-step work and retain exact-current special paths.
3. Route Layer C and Motion Field debug sampling through the same existing packed-state interpolation helper as Final Foam; update active comments/descriptions only.
4. Accumulate generated Motion Lane range, mean absolute, RMS, and sign/near-neutral coverage without altering the texture output.
5. Record presentation-alpha min/max during the explicit P12 accounting window; expose both evidence groups in the Inspector and P12 report.
6. Make the P12 report reject hard-pinned interpolation by requiring a captured alpha range of at least `0.25`, and validate lane fractions sum to one.
7. Place clipboard-copy actions adjacent to P12, P9, P8, P7, P6, P5.3, P5.1, obstacle-baseline, and obstacle-comparison reports while retaining logging and disk reveal.
8. Complete the full diff, C# 9, shader/include, resource/kernel, source-budget, packed-moment, timing, and cross-subsystem consistency audits.

### Risks and controls

- **One-tick visual latency:** ordinary fixed-step interpolation intentionally presents from the previous committed state toward the current committed state. It is limited to one material tick and is the cost of removing hard cadence steps without prediction.
- **Packed moment validity:** convex interpolation must preserve `0 <= lifeMoment <= Presence` and `0 <= patternMoment <= Presence`; randomized validation is required.
- **False diagnostic pass:** the P12 report must fail if alpha remains pinned even when all values are finite.
- **Misdiagnosed lateral weakness:** lane statistics must remain evidence only; no steering retune is authorized without the Unity result.
- **Shared shader impact:** all shader source outside `SampleCommittedFoamState` must remain executable-equivalent, and the shared Foam include change must be comment-only.

### Acceptance and validation status

- [x] Exact 16-file scope and no serialized/resource/kernel changes.
- [x] All 90 River C# files pass delimiter/string and C# 9 multiline-interpolation scans.
- [x] All changed shader/include delimiters and local includes pass.
- [x] Compute shaders and render resource/kernel/property declarations remain unchanged.
- [x] 250,000 randomized packed-state blends preserve Presence/life/pattern moment bounds.
- [x] 15 cadence/frame-rate timing cases keep alpha in `[0,1]` and produce at least `0.80` captured range.
- [x] Motion Lane production output calculation is unchanged; statistics are appended after the exact half output assignment.
- [x] Every Foam report action has an adjacent clipboard-copy action and disk output remains available.
- [x] Overall Foam source/amount owners remain byte-identical and the issue is deferred to P13.
- [ ] Unity C# compilation and D3D11 shader import produce zero errors or warnings.
- [ ] Layer C Material Presence and Final Foam no longer hard-step at the material cadence.
- [ ] Expanded P12 snapshot passes interpolation range, lane evidence, runtime, and cache gates.
- [ ] P9 consumer regression remains `Overall: PASS` after visual acceptance.

## Implementation record — `RG-METRIC-P12 — Fixed-metric activation and candidate evidence`

### Current objective

Activate the already-migrated `FixedMetricLattice` path as the default P12 test configuration, retain a direct `LegacyNormalizedAcross` compatibility switch, expose the four approved candidate spacings (`0.25`, `0.20`, `0.15`, and `0.10 m`), and produce one compact live candidate report containing the runtime/cache/transport/work evidence needed for visual and performance selection. This patch intentionally allows a changed candidate to invalidate the current cache and interrupt Foam until the matching cache is rebuilt; P12 does not add a shadow runtime, test-only duplicate resources, automatic rollback, or source-recipe compensation.

### User authorization and testing policy

The user explicitly authorized direct Play Mode activation even when a candidate may visibly break the River. Practical failure discovery is preferred over additional safety scaffolding. Persistent scene/prefab/material/cache writes remain explicit: this patch changes source defaults and Inspector controls but does not raw-edit or serialize any scene, prefab, material, or cache asset.

### Reviewed evidence

- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs::ResolveInitializationDimensions` always resolves `TryResolveLegacyInitializationDescriptor`, then prepares but never selects `fixedMetricCandidateDescriptor`. This is the single active-allocation gate.
- `Game/Procedural/Rivers/StylizedRiverFoamGridDescriptor.cs::TryCreateFixedMetricOneStrip` already resolves the complete contiguous fixed descriptor, including exact 32 m chunk columns, global lateral rows, odd film dimensions, represented lateral extent, and initialization signature. No new coordinate contract is required.
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.TopologyCache.cs::CreateTopologyCachePackage` serializes the active `gridDescriptor`; `TryResolveAssignedTopologyCacheForStartup` rejects descriptor mismatches. Therefore switching mapping or spacing must deliberately require rebuilding the assigned cache through the existing explicit Edit Mode workflow.
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs::AreResourcesCompleteAndCurrent`, `InitializationInputsChanged`, and `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs::NotifyRiverChanged` currently track only domain and quality. New mapping/spacing authoring must join those invalidation contracts so live changes cannot retain resources built for a different descriptor.
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.P9Diagnostics.cs::RunP9ComprehensiveValidationReport` hard-codes active legacy ownership. P12 activation would make the current endpoint fail for the wrong reason unless the report verifies the authored active selection instead. Its actual fixed GPU film/shape/render tests remain valid and must remain unchanged.
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.RuntimeUpdates.cs` already owns explicit steady-state work accounting, including dispatches, cell iterations, transport substeps/CFL, CPU submission time, topology work, and shape work. P12 can reuse this accounting rather than introduce a duplicate profiler path.
- `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs` owns Foam runtime/quality authoring; `StylizedRiverEditor.Actions.cs` owns cache preparation and current validation actions; `StylizedRiverEditor.Diagnostics.cs` already displays descriptor, CFL, curvature, memory, and candidate evidence. No new Debug View is required.
- The supplied workspace has no `.git` metadata. The immutable comparison baseline is `/mnt/data/p12_work/base`, reconstructed from the user-supplied Assets snapshot plus validated P9a, P10, P10a, and documentation-only P11 packages. Full-file hashes and line-ending counts were captured before this plan edit.

### Approved file scope

Canonical documentation:

1. `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
3. `Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
4. `Docs/River_Foam_Stage6_Architecture.md`
5. `Docs/River_Rendering_Roadmap.md`

Authoring and Inspector:

6. `Game/Procedural/Rivers/StylizedRiver.cs`
7. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
8. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`
9. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Diagnostics.cs`

Runtime allocation/invalidation/diagnostics:

10. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs`
11. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs`
12. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs`
13. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs`
14. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs`
15. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.P9Diagnostics.cs`
16. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.P12Diagnostics.cs` plus `.meta`
17. `Game/Procedural/Rivers/StylizedRiverFoamGridDescriptor.cs` — contract-summary comment only
18. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Obstacles.cs` — legacy-signature comment only

No compute shader, HLSL include, render shader, source recipe, topology generator, Disturbance source, scene, prefab, material, cache asset, tag, layer, or generated asset is approved for modification. `StylizedRiverFoamRuntime.TopologyCache.cs`, `StylizedRiverFoamRuntime.RuntimeUpdates.cs`, and `StylizedRiverFoamRuntime.TopologyReplacement.cs` were reviewed as direct contracts and remain unchanged. The two comment-only scope additions above are required because activation makes their former “active runtime remains legacy” statements factually stale; no executable line in either file may change.

### Invariants and non-goals

- New existing-content default: `FixedMetricLattice` with quality-derived spacing. This material default change is explicitly authorized for P12 testing. Medium remains `0.15 m`, Low `0.25 m`, High `0.10 m`.
- `0.20 m` is an explicit candidate override, not a new quality tier.
- `LegacyNormalizedAcross` remains directly selectable for A/B comparison and rollback.
- Candidate changes may stop Foam until the assigned topology cache is rebuilt. No runtime topology generation or automatic cache write is introduced.
- Legacy formulas remain byte-equivalent on the legacy branch. Fixed formulas, kernels, resources, source recipes, birth budgets, topology rules, transport, film, shape, and rendering remain unchanged.
- No new persistent texture, buffer, component, or serialized asset is added. Two small serialized River enum fields are added; source code does not modify any existing scene or prefab instance.
- No automatic visual verdict is claimed. The P12 report proves machine-verifiable runtime/cache/transport/work contracts; the user supplies visual comparison evidence.

### File-by-file implementation sequence

1. Add public P12 mapping and fixed-cell-size enums, serialized defaults, resolved properties, and enum sanitization in `StylizedRiver.cs`. **Status: mechanically verified.**
2. Add mapping/candidate controls and cache-invalidating guidance to Foam Runtime & Quality in `StylizedRiverEditor.Foam.cs`. **Status: mechanically verified.**
3. Select fixed or legacy descriptors in `StylizedRiverFoamRuntime.Resources.cs`; track mapping/cell-size allocation ownership through `Members`, `Constants`, and `Lifecycle`. **Status: mechanically verified.**
4. Replace deferred-only diagnostic semantics with active-selection semantics in `PublicSurface` and `StylizedRiverEditor.Diagnostics.cs`. **Status: mechanically verified.**
5. Update the P9 endpoint to preserve and validate the authored active selection rather than requiring legacy. Keep all actual GPU consumer tests unchanged. **Status: mechanically verified.**
6. Add one Play Mode P12 candidate snapshot report reusing existing steady-state accounting, and expose start/reset plus write actions in `StylizedRiverEditor.Actions.cs`. **Status: mechanically verified.**
7. Update all five canonical documents with activation state, exact test workflow, evidence boundaries, and P13 handoff. **Status: mechanically verified.**
8. Run full post-change parser, symbol, source-scope, C# 9 multiline-string, enum/default, descriptor-selection, invalidation, report, package-byte, and protected-file audits. **Status: mechanically verified; final 19-file archive reproduced every source byte.**

### Acceptance criteria

- Existing rivers select fixed metric by default without any raw scene/prefab edit.
- The Inspector offers fixed/legacy selection and all four candidate spacings.
- The active descriptor exactly matches the authored selection and requested spacing.
- Changing mapping or spacing invalidates/reinitializes resources and makes an incompatible cache fail explicitly.
- Explicit cache preparation produces a cache whose descriptor matches the current active selection.
- The unchanged P9 GPU consumer regression passes for either selected active mapping.
- The P12 report requires a live complete runtime plus an explicit comparable accounting window, records descriptor/cache/field/CFL/curvature/memory/work evidence, and does not claim visual acceptance.
- No protected shader/compute/topology/source/render/serialized asset changes occur.

### Risks and validation focus

- **Cache mismatch after activation:** expected and visible; verify the startup reason is descriptor mismatch and explicit rebuild resolves it.
- **Live mapping change retaining stale resources:** prevented by allocated mapping/spacing comparisons in all resource-current, restart, and notification paths.
- **P9 false failure after activation:** prevented by selected-active ownership validation.
- **Unbounded fixed allocation:** existing descriptor maximum-dimension rejection remains authoritative; P12 adds no fallback scale change.
- **Misleading performance comparison:** P12 report requires an explicit accounting window and reports visibility percentage, elapsed time, frames, dispatches, cells, substeps, and CPU submission work.
- **C# language compatibility:** all changed C# files must parse under C# 9-compatible syntax; multiline interpolation expressions are prohibited.

### Rollback checkpoint

The exact post-P11 source in `/mnt/data/p12_work/base`. Selecting `LegacyNormalizedAcross` is the immediate runtime comparison/rollback path; reverting the P12 package restores the previous default and removes the two new serialized authoring fields.

### Post-implementation consistency and compliance audit

Source audit result before final packaging:

```text
Primary P12 audit:                         28 passed / 0 failed
Independent P12 audit:                     43 passed / 0 failed
Changed files:                             19 exact
River C# files parsed:                     90
Parser configurations:                    360
C# syntax nodes inspected:          2,053,325
River methods indexed:                  1,647
C# 9 multiline interpolation defects:       0
Duplicate exact method signatures:           0
Protected GPU/render/serialized files: 77 unchanged
Serialized River field delta:                2 exact
P9 helper/GPU method changes:                 0
Comment-only executable differences:          0
Archive files verified:                  19 / 19
Archive extraction byte mismatches:           0
```

The audit confirmed that only `RunP9ComprehensiveValidationReport` changed inside the P9 diagnostic file; every P9 GPU/helper method body remains hash-identical. `StylizedRiverFoamGridDescriptor.cs` and `StylizedRiverFoamRuntime.Obstacles.cs` differ only in comments after whitespace/comment stripping. Topology-cache ownership, steady-state accounting, topology replacement, compute, HLSL, render shaders, scenes, prefabs, materials, and cache assets remain byte-identical.

One implementation refinement was made during audit: invalidation now tracks the resolved requested fixed spacing rather than the fixed-size enum identity. Therefore `Quality Default` and an explicit candidate resolving to the same metres do not trigger a false rebuild, and fixed-size edits are irrelevant while legacy mode is active. This remains within the recorded mapping/spacing ownership plan.

Unity 6000.5.0f1 compilation, D3D11 import, explicit fixed-cache rebuild, Play Mode visual evidence, P12 snapshots, and the selected-candidate P9 rerun remain authoritative and pending.

## Implementation record — `RG-METRIC-P12b — Deposit-once automatic sources, stable committed-state ownership, and effective lateral-flux evidence`

> **Unity disposition:** The committed-state, broad-flicker, and lateral-evidence portions were retained, but the global automatic-source deposit-once policy was rejected. It silenced the accepted Object Arc/Semi-Arc Hold and Release emitter phases. The source-ownership portions below are historical implementation evidence and are superseded by the hybrid P12c contract.

### Objective and acceptance criteria

Correct the two code-audited Layer C ownership defects exposed by the active fixed-metric candidate, and add transport-facing lateral evidence without changing overall Foam quantity tuning:

1. Automatic source events must create material only from coverage newly revealed during the current material step. Previously revealed Build interiors, Hold, and Release must not deposit again. A cell that has died must remain dead unless a distinct later birth event or manual injection reaches it.
2. The renderer's previous committed state must remain a distinct texture for every transport-substep parity, including even substep counts.
3. P12 evidence must distinguish generated cell-centre lane intent from effective lateral face intent and material-weighted lateral transport.
4. Existing manual injections, source geometry, source amount parameters, transport equations, source-event schedules, overall birth budgets, topology, film, shape, final-render controls, and fixed/legacy coordinate contracts must remain unchanged.
5. Existing disk reports and adjacent clipboard-copy actions remain available.

Acceptance requires a zero-error mechanical audit, exact CPU/GPU source-event ABI agreement, unchanged kernel/resource declarations except the intentional metric-count extension, deterministic deposit-once behavioral cases, committed-state ownership cases across one through several substeps, unchanged manual-injection code, and Unity validation showing no dead-edge resurrection in Material Presence or Remaining Life.

### Reviewed evidence

- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs::SimulateFullField` runs transport/lifecycle first, then calls `DispatchAutomaticFoamSourceEvents(currentState, materialStepDuration)`, then manual births. Automatic sources therefore re-author source-space material after transport.
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs::DispatchAutomaticFoamSourceEvents` advances and dispatches every active event on every material tick until total event duration completes.
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.State.cs::FoamSourceEventGpuData` and `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Structs.hlsl::FoamSourceEventData` contain only current phase/progress; no previous deposition coverage is available to the GPU.
- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute::EvaluateFoamAutomaticSourceRasterSample` evaluates cumulative Build/Hold/Release source coverage and `ApplyFoamAutomaticSourceRasterSample` merges it with `FoamMergeBornPresence`.
- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Simulation.hlsl::FoamEncodeMaterialState` clears Presence, life moment, and pattern moment when Presence or Remaining Life reaches zero; the next active-source dispatch can therefore restore all three through `FoamMergeBornPresence`.
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs::SimulateFullField` aliases `previousState = currentState` before ping-pong transport. With an even number of substeps, the final write can return to that same texture, so previous/current presentation ownership is not guaranteed distinct.
- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Simulation.hlsl::FoamResolveLateralFaceFlux` averages adjacent cell velocities at the face. Existing P12 lane statistics measure only cell-centre intent and cannot quantify face cancellation or material-weighted lateral movement.
- The supplied active-candidate snapshot proves the current candidate uses one substep, traverses the full presentation interpolation interval, and contains nontrivial generated lane intent. It does not measure effective face flux. The visual evidence shows repeated state changes only in already-eroded source-covered edges.
- The supplied workspace has no `.git` metadata. The immutable rollback baseline is `/mnt/data/p12b_baseline/Assets`, reconstructed from the user-supplied post-P12a source. Full-file hashes and line-ending counts are captured before implementation.

### Approved file scope

Canonical documentation:

1. `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
3. `Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
4. `Docs/River_Foam_Stage6_Architecture.md`
5. `Docs/River_Rendering_Roadmap.md`

Runtime C# contracts and ownership:

6. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs`
7. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs`
8. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.State.cs`
9. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs`
10. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs`
11. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Compute.cs`
12. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Obstacles.cs`
13. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs`
14. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.TopologyReplacement.cs`
15. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs`
16. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.P7Diagnostics.cs`
17. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.P12Diagnostics.cs`
18. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Diagnostics.cs`

GPU source and transport contracts:

19. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Structs.hlsl`
20. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl`
21. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Simulation.hlsl`
22. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`

No scene, prefab, material, cache asset, source recipe, serialized River field, topology rule, film/shape path, render shader, Debug View, kernel, texture format, tag, layer, or generated asset is approved for change. One additional persistent packed-state texture is approved solely to preserve the previous committed presentation state across arbitrary substep parity. Four existing transport-metric counters are approved; no new compute buffer is added.

### File-by-file implementation sequence

1. Extend CPU/GPU automatic-source data by one `float4` carrying previous deposition phase/progress; update the exact 128-byte stride contract. **Status: implemented and mechanically validated; Unity import pending.**
2. Pack current and previous deposition progress. Object Arc/Semi-Arc deposition progress is clamped to Build; Hold and Release resolve to identical completed Build coverage and therefore zero delta. **Status: implemented and mechanically validated; Unity behavior pending.**
3. Refactor the automatic-source evaluator to compute current and previous source contribution. Use `max(0, current - previous)` as deposition permission, then merge the current absolute source target so the existing non-additive birth merge preserves authored strength. Preserve all eight family evaluators and manual injection paths. **Status: implemented; mechanically validated, Unity pending.**
4. Allocate one same-format `presentationPreviousState` texture. Copy the committed current state into it before each material update, then ping-pong simulation independently. Include allocation, clear, boundary, topology replacement, release, completeness, and memory accounting ownership. **Status: implemented and mechanically validated across one through 64 substeps; Unity behavior pending.**
5. Extend existing transport metrics with material-weighted lateral speed numerator/weight and positive/negative lateral Presence movement, accumulated once per north face. **Status: implemented and mechanically validated; Unity readback pending.**
6. Compute generated-lane face mean, RMS, opposing-face fraction, and cancellation ratio after the existing texture output is finalized; expose them with the transport metrics in P12 snapshot and Inspector diagnostics. **Status: implemented and mechanically validated; Unity snapshot pending.**
7. Extend the existing P7 source regression with CPU/GPU ABI checks and deterministic deposit-once semantics for Build, Build-to-Hold, Hold, Release, dead covered cells, and unchanged manual births. **Status: implemented and mechanically validated; Unity report pending.**
8. Update all five canonical documents, run full post-change source/diff/compliance validation, and package changed files only. **Status: complete; the final 25-file archive was extracted and reproduced every source byte.**

### Invariants, non-goals, and risks

- Deposit-once changes source ownership, not source geometry or schedule. Build duration still controls reveal timing; Hold and Release remain event-lifecycle phases but no longer maintain material.
- Positive coverage difference is used. A moving/retracting source never deletes material; transport and lifecycle remain the only post-birth state owners.
- Overall Foam quantity is deliberately not tuned in P12b. The final amount pass remains P13.
- Manual ellipse, segment, compound, and probe injections remain byte-identical.
- The presentation-state texture adds one packed-state field. At the active 428 x 45 candidate this is approximately `428 * 45 * 8 = 154,080` bytes before API overhead; exact runtime accounting must include it.
- Lateral metrics are read-only evidence. No lane amplitude, face averaging, transport coefficient, or velocity cap is changed in P12b.
- Transport metric accumulation must count each lateral face once to avoid double counting.
- The source-event ABI change must update C# stride, HLSL layout, P7 assertions, and every construction site together.
- Unity 6000.5.0f1 compilation, D3D11 compute import, actual GPU source behavior, visual flicker removal, and active-candidate P9 regression remain authoritative Unity checks.

### Validation and compliance checklist

- [x] Final diff contains only the 25 approved files after the recorded text-only scope amendment.
- [x] All changed C# files pass C# 9 delimiter/string/interpolation checks and the available tree-sitter parser; Unity compilation remains pending.
- [x] CPU and GPU automatic-source layouts are exactly eight `float4` lanes / 128 bytes.
- [x] All 23 kernels remain present in exact order; HLSL texture/buffer declarations are unchanged and only the existing metric buffer count grows.
- [x] Deterministic source cases prove positive Build-frontier deposition and zero repeated Build-interior, Hold, and Release deposition.
- [x] Dead covered-cell model remains zero when deposition permission is zero; manual injection bodies are byte-identical.
- [x] Presentation previous/current/write resources remain distinct for one through 64 transport substeps and all allocation/replacement/cleanup paths own the third texture.
- [x] Lateral metrics are finite, face-counted once, remain within fixed-point headroom, and leave transport output unchanged.
- [x] Protected source recipes, topology algorithms, film/shape/render code, scenes, prefabs, materials, cache assets, and serialized fields remain byte-identical.
- [x] Canonical documents agree on P12b state and preserve P13 amount-tuning deferral.
- [ ] Unity compile/import, P7 source regression, P12 snapshot, visual Layer C review, and P9 endpoint remain authoritative and pending with exact next actions.

### P12b scope amendment — source-phase authoring text

Post-implementation consistency review found three direct authoring/debug descriptions that explicitly state Object Arc/Semi-Arc Hold continuously replenishes material and Release clears the source path. Deposit-once ownership makes those statements false even though the serialized durations and source geometry remain unchanged. The following existing files are added for text-only correction before further implementation:

23. `Game/Procedural/Rivers/StylizedRiver.cs` — two Hold-duration tooltips only.
24. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs` — Object Contact lifecycle help text only.
25. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs` — Automatic Birth Sources description only.

No serialized field, default, GUI control, Debug View identity, recipe value, or executable source scheduling changes are authorized in these three files. Final scope is 25 files.


### P12b implementation-validation correction — target merge semantics

The first provisional shader rewrite passed the positive coverage delta directly to `FoamMergeBornPresence`. Final implementation review rejected that form before packaging: `FoamMergeBornPresence` treats source Presence as an absolute target (`max(existing, source)`), not an additive increment. Feeding a small per-tick delta would therefore leave progressively revealed feather/interior cells at only the largest single-step delta instead of the authored source strength.

The accepted implementation uses the positive current-minus-previous contribution only as deposition permission. At a newly advancing frontier it passes the current absolute source contribution to the unchanged merge. This preserves normal-strength source authoring during Build while still making identical old Build coverage, Hold, and Release non-depositing. The deterministic P7 contract now requires both the positive-difference gate and the absolute-target assignment.


### P12b final mechanical-validation record

The final audit passed `42 / 42` primary gates and an independent audit passed `44 / 44` gates. It parsed all `165` project/River C# files (`1,352,809` syntax nodes) with zero parser errors, scanned all changed C# for C# 9 string/interpolation defects, preserved all `23` kernels in exact order, preserved all `58` HLSL texture/buffer declarations, proved the eight source-family evaluator bodies and manual-injection bodies byte-identical, and exercised `101,000` deposition-progression cases, `250,000` absolute-target merge cases, `250,000` lateral-flux equivalence cases, `100,000` lane-face metric cases, and committed-state ownership for one through `64` substeps.

Validation caught and corrected two implementation issues before packaging:

1. A provisional direct-delta merge would have underfilled newly revealed source cells because `FoamMergeBornPresence` consumes an absolute target. The final code uses positive difference only as permission and supplies current absolute contribution as the target.
2. The first three-texture topology-transition detachment left the unused ping-pong write texture outside snapshot ownership. The final detachment captures that write reference and releases it unless it is one of the two snapshot textures.
3. The provisional source packing zeroed the accepted Object Arc/Semi-Arc material-step reveal feather. The final code restores the exact prior `material tick / Build duration` packing, and the audit now requires it.

The final changed-files archive contains exactly `25` `Assets/...` entries. A clean extraction reproduced every final source byte with zero mismatches.

Unity 6000.5.0f1 compilation, D3D11 compute import, real GPU source behavior, Layer C visual acceptance, P12 metric readback, and the P9 endpoint remain pending.


### P12b final consistency correction — Object Arc/Semi-Arc reveal feather packing

The final full-file reread found that the provisional CPU packing had replaced the existing Object Arc/Semi-Arc normalized material-step reveal width in `FoamSourceEventGpuData.Shore.z` with zero. The evaluator bodies were byte-identical, but their packed reveal-feather input would have changed from the accepted `material tick / Build duration` value to the shader's `0.0001` floor. That violates the recorded invariant that source geometry and Build reveal behavior remain unchanged.

Before packaging, restore the exact existing material-step-progress calculation and continue packing it into `Shore.z`. Current and previous source contributions will use the same preserved reveal width; only phase/progress history and the positive-difference deposition gate remain new. Extend the final mechanical audit to require the preserved calculation and CPU/GPU comments.

## Corrective implementation plan — `RG-METRIC-P12c — Persistent Object Emitters and Shader Import Repair`

### Status

- Read-only review: **complete**.
- Plan recorded before implementation: **complete**.
- Implementation: **complete**.
- Mechanical validation: **complete — 43/43 primary gates and 22/22 independent gates passed**.
- Unity compilation, D3D11 import, P7/P9/P12 reports, and visual lifecycle validation: **pending**.
- Mechanical/static validation: **pending**.
- Unity C# compilation, D3D11 import, P7/P9/P12 reports, and visual validation: **pending**.

### Objective

Correct the P12b regression without discarding its accepted fixes:

1. restore the accepted Object Contact Arc/Semi-Arc emitter lifecycle—progressive Build, continuously emitting full Hold, progressive source Release, then Rest;
2. retain deposit-once current-minus-previous coverage ownership for Shore Ribbon, Inward Wash, Contact Fleck, and all Free-Water source families;
3. remove the sixteen D3D11 definite-assignment warnings introduced by the extracted automatic-source contribution helper;
4. retain the dedicated previous-committed presentation texture, even-substep ownership correction, source-event ABI, effective lateral face/flux evidence, report clipboard actions, and all fixed-metric coordinate behavior;
5. leave overall Foam amount, source weights, source strengths, durations, geometry, transport, lifecycle, Film, Shape, and final rendering untuned.

### Reviewed evidence

- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs::ResolveAutomaticSourceDepositionState` currently forces Object Arc/Semi-Arc to phase `0` and clamps progress to Build for the complete event.
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs::DispatchAutomaticFoamSourceEvents` currently dispatches only when current progress exceeds previous progress, so Object Hold and Release perform no source rasterization.
- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute::EvaluateFoamAutomaticSourceRasterSample` currently applies the positive current-minus-previous gate to every source family, which also rejects Object Hold and Release.
- The accepted pre-P12b implementation in the reconstructed post-P12a source resolves Object Arc/Semi-Arc phase `0/1/2` for Build/Hold/Release and dispatches every active material tick.
- `CS_RiverFoam.compute::FoamResolveObjectRibbonPhaseMask` already implements cumulative Build, full Hold, and progressive Release, including reverse-order Semi-Arc release. No object-source geometry evaluator requires modification.
- Unity D3D11 reports sixteen warnings from `FoamEvaluateAutomaticSourceContribution`: eight conditional evaluator calls used by two kernels. The same evaluator calls imported without those warnings when they were inline before P12b.
- The user confirmed the original broad dead-edge back-and-forth flicker is solved; any remaining death-transition artifact is deferred. The accepted persistent object-emitter lifecycle takes precedence over global deposit-once ownership.

### Approved scope

Modify exactly these existing files:

1. `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
3. `Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
4. `Docs/River_Foam_Stage6_Architecture.md`
5. `Docs/River_Rendering_Roadmap.md`
6. `Game/Procedural/Rivers/StylizedRiver.cs`
7. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs`
8. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
9. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs`
10. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.P7Diagnostics.cs`
11. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.P12Diagnostics.cs`
12. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`

No file will be created, deleted, renamed, or serialized. No scene, prefab, material, cache asset, `.meta`, serialized River field, source recipe, GPU-event lane, buffer, texture, kernel, dispatch, resource declaration, render shader, or shared include will change.

### Implementation sequence

1. Restore phase resolution for Object Arc/Semi-Arc from the accepted post-P12a implementation: Build=`0`, Hold=`1`, Release=`2`, each with its own normalized progress.
2. Dispatch Object Arc/Semi-Arc on every active material tick; retain progress-change dispatch suppression for nonpersistent source families.
3. In the raster sample, classify Arc/Semi-Arc as persistent emitters. Use their current phase-shaped contribution directly. Apply positive current-minus-previous deposition gating only to nonpersistent source families.
4. Remove `FoamEvaluateAutomaticSourceContribution`. Inline the existing source-family selection for current contribution and for the nonpersistent previous contribution so D3D11 sees the same definite-assignment structure that imported cleanly before P12b. Do not change any of the eight evaluator bodies or formulas.
5. Replace the P7 global deposit-once contract with a hybrid ownership regression that requires:
   - nonpersistent Build frontier advances;
   - repeated nonpersistent Build interior deposits zero;
   - Object Build progresses in phase `0`;
   - Object Hold is phase `1` and remains dispatchable/emitting;
   - Object Release is phase `2`, remains dispatchable, and shrinks progressively;
   - Rest is represented by event completion/no dispatch;
   - current absolute birth target and production/debug evaluator identity remain unchanged.
6. Update P12 ledger wording from global deposit-once ownership to hybrid automatic-source ownership.
7. Restore Inspector/tooltips/debug descriptions and canonical status documents to the accepted persistent Object emitter lifecycle while retaining deposit-once ownership for all other automatic source families.

### Invariants and non-goals

- Preserve the exact Arc/Semi-Arc geometry, path order, reveal feather, Build/Release masks, source amount, Remaining Life, pattern, event timing, cycle scheduling, rear exclusion, and deterministic side selection.
- Preserve the P12b source-event ABI at eight `float4` lanes / 128 bytes.
- Preserve manual injection behavior byte-for-byte.
- Preserve P12b committed-state resource ownership and effective lateral metrics byte-for-byte.
- Preserve all 23 compute kernels, thread-group declarations, resource declarations, and C# `FindKernel` order.
- Do not address overall Foam quantity or minor death-transition aesthetics in this patch.

### Acceptance criteria

- Object Arc/Semi-Arc GPU data resolves correct Build/Hold/Release phase and normalized progress at boundaries and representative interior times.
- CPU dispatch does not suppress active Object Hold or Release.
- GPU contribution gating bypasses current-minus-previous only for Arc/Semi-Arc and retains it for the other six source families.
- All eight source evaluator bodies remain byte-identical.
- The warning-producing helper is absent; no equivalent helper with conditional `out` return ownership is introduced.
- P7 hybrid ownership validation and P12 ledger use accurate semantics.
- Changed C# parses under C# 9-compatible checks; no missing imports, duplicate signatures, or multiline interpolation defects exist.
- Kernel/resource/ABI parity, protected-file byte identity, deterministic phase/deposition model tests, and archive extraction byte comparison pass.
- Unity must subsequently report zero C# errors and zero shader/compute errors or warnings, P7/P9/P12 must pass, and visual review must confirm the persistent Object emitter cycle.

### P12c scope correction — source-event ABI comments

The post-implementation contract reread found two direct ABI comments that still describe positive current-minus-previous coverage as the gate for every automatic source. P12c exempts persistent Object Arc/Semi-Arc emitters, so those comments would be false even though the eight-lane layout is unchanged.

Add these existing files as comment-only scope before further implementation:

13. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.State.cs`
14. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Structs.hlsl`

Permitted change: state that previous phase/progress gates nonpersistent source families, while Object Arc/Semi-Arc use current phase-shaped persistent emission. Prohibited change: field order, lane count, type, stride, allocation, binding, or executable HLSL/C# behavior.

### P12c validation correction — P7 active-selection gate

The final validation-path reread found that P7 still requires `activeLegacy` and prints `ACTIVE LEGACY OWNERSHIP VERDICT`. That was correct while fixed allocation was inactive, but P12 now authoritatively activates either the authored Fixed Metric or Legacy selection. Requiring legacy would make the requested P7 report fail for the actual P12 fixed candidate before source validation runs.

Within the already approved `StylizedRiverFoamRuntime.P7Diagnostics.cs` scope:

- replace the legacy-only gate with `FoamGridSelectionMatchesActive` plus a created/current descriptor;
- retain the prepared fixed-candidate readiness requirement;
- run source validation for either correctly authored active mapping;
- update report/ledger wording to `Active descriptor matches authored selection`;
- do not change preparation, resources, cache ownership, source evaluation, or cleanup.


### P12c final mechanical-validation record

The final post-implementation audit passed `43 / 43` primary gates and `22 / 22` independent gates. It verified exactly `14` changed existing files, no created/deleted/renamed file, and `583` protected files byte-identical to the immutable post-P12b baseline.

The audit additionally proved:

- all `90` River C# files are lexically balanced, with zero C# 9 multiline-interpolation defects in changed code;
- Object Arc/Semi-Arc phase resolution returns Build=`0`, Hold=`1`, and Release=`2` with correct normalized progress;
- persistent Object events dispatch on every active material tick at `8`, `12`, and `16 Hz`, while the other six source families retain progress-change/deposit-frontier ownership;
- the warning-producing `FoamEvaluateAutomaticSourceContribution` helper is absent; current and nonpersistent-previous source-family selection is inline with initialized values;
- all eight automatic-source evaluator bodies plus `FoamResolveObjectRibbonPhaseMask` are byte-identical;
- the source-event ABI remains exactly eight `float4` lanes / `128` bytes in matching CPU/GPU order;
- all `23` compute kernels remain present in exact order, and resource declarations are unchanged;
- manual-injection bodies, committed-state resource ownership, topology replacement, compute binding, simulation include, source recipes, serialized River declarations, and all ten report clipboard labels remain unchanged;
- `100,000` randomized object phase-mask cases are monotonic in the accepted Build and Release directions;
- P7 now checks the active descriptor against the authored selection rather than incorrectly requiring Legacy while P12 Fixed Metric is active.

The changed-files archive must contain these exact `14` `Assets/...` entries and reproduce every final source byte on clean extraction. Unity 6000.5.0f1 compilation and D3D11 import remain authoritative for confirming that all sixteen reported warnings are gone and that Object Build/Hold/Release/Rest behavior is visually restored.


## RG-METRIC-P12i — Exact Presence Chip eligibility ownership

### Objective

Make the displayed Presence-Amplitude `Chip Eligibility Composite` mask the sole and exact production permission. Do not derive any wider or alternate production region from it.

### Accepted implementation

- Current remains arithmetic-identical, including Interior Access.
- Presence-Amplitude edge selection is `saturate(chipCandidateField * chipEligibility.edgeBand)`.
- Presence-Amplitude Interior Access and every projected-reach admission path are disabled.
- Presence-Amplitude direct hardened-mask carving remains, but its input is already clipped to the exact eligibility mask.
- The invariant `chipProductionSelection <= chipEdgeEligibility` holds per fragment.
- No amplitude compression, control, candidate formula, transport, source, lifecycle, Film, Shape, resource, kernel, dispatch, cache, scene, prefab, or material change is included.


## RG-METRIC-P12j — Clean binary Presence Chip eligibility

### Objective

Replace the noisy/fractional Presence-Amplitude eligibility signal while preserving exact Candidate × Eligibility production ownership and the complete Current compatibility path.

### Approved implementation

- Produce a transient clean silhouette from Presence-Amplitude base coverage multiplied by the existing near-death life gate before patterned erosion.
- Carry that scalar through existing render-only spatial coupling; add no texture, buffer, sample, kernel, dispatch, property, or serialized field.
- Use Euclidean screen-gradient magnitude for Presence-Amplitude edge-width estimation.
- Use binary meaningful-support permission so a permitted faint fringe can be fully removed by the unchanged analytical candidate.
- Keep `chipProductionSelection <= chipEdgeEligibility` per fragment and disable Presence-Amplitude Interior Access.
- Preserve Current arithmetic and behavior.

### Status

Mechanically implemented and validated; Unity import and visual acceptance pending.

### Exact pre-Chip rendered-mask ownership — P12k

P12j is rejected because its clean Presence/life silhouette is produced before material-pattern erosion and structural Strand shaping. No threshold or fixed offset can make that surrogate coincide with rendered Foam because the omitted stages vary spatially.

For Presence-Amplitude, P12k resolves the existing structural Strand keep first and constructs:

```text
preChipRenderedMask = saturate(foam.mask × strandKeep)
```

Eligibility uses the existing visible-support boundary `RiverWaterFoamResolveBaseCoverage(preChipRenderedMask)` with the `0.08` mask threshold and Euclidean screen-gradient normalization. Production remains exactly Candidate × Eligibility. Final and Production Chip diagnostics use exact differences from the same mask. Current remains the protected compatibility path. P12j clean-silhouette plumbing is retired.



## RG-METRIC-P12l — Binary Candidate × Eligibility implementation record

### Objective

For Presence-Amplitude only, replace fractional Chip attenuation with an exact logical region intersection and complete removal:

```text
candidateSelected = chipCandidateField >= 0.5
eligibilitySelected = chipEligibility.edgeBand >= 0.5
productionSelected = candidateSelected AND eligibilitySelected
```

### Invariants

- `Current` remains unchanged.
- P12k `preChipRenderedMask` remains the authoritative Presence-Amplitude no-Chip geometry.
- No expanded region, projected reach, Interior Access, support-intensity multiplication, transparency, interpolation, or additional permission field is permitted.
- Presence-Amplitude debug views report the exact binary masks used by production.

### Validation

- Extract and compare the complete Current branch against the immutable post-P12k baseline.
- Prove the eight binary truth-table combinations for candidate/eligibility/pre-Chip support and complete selected removal.
- Prove `productionSelected == candidateSelected * eligibilitySelected` and `finalMask == 0` for every selected pixel.
- Prove Presence-Amplitude Candidate, Eligibility, and Production debug inputs equal production masks exactly.
- Verify no C#, compute, property, resource, kernel, serialized, scene, prefab, material, cache, or `.meta` change.
- Unity import and direct debug/Final comparison remain authoritative.


## RG-METRIC-P12m — Any-Support Binary Chip Selection implementation record

### Objective

Replace P12l midpoint-contour selection with the approved absolute any-support rule for Presence-Amplitude only:

```text
candidateSelected   = chipCandidateField > 0.0 ? 1 : 0
eligibilitySelected = chipEligibility.edgeBand > 0.0 ? 1 : 0
productionSelected  = candidateSelected × eligibilitySelected
finalFoamMask       = productionSelected == 1 ? 0 : preChipRenderedMask
```

### Reviewed cause

P12l's Candidate and Eligibility sources are antialiased continuous fields. Its independent `>= 0.5` comparisons discard positive field support below each midpoint contour. The application path already removes all Foam after selection, so no application, geometry, caller, or resource change is required.

### Invariants

- `Current` remains byte-identical.
- P12k `preChipRenderedMask` remains the authoritative Presence-Amplitude no-Chip geometry.
- Candidate generation, readability/subpixel/lifecycle gates, Eligibility geometry, Edge Width, Strands, controls, transport, sources, Film, Shape, resources, kernels, dispatches, caches, scenes, prefabs, materials, properties, serialized fields, and Debug View identities remain unchanged.
- No epsilon, midpoint threshold, fractional attenuation, expanded reach, inferred permission, Interior Access, or secondary authorization field participates in Presence-Amplitude.

### Risk

Any positive antialias, readability, or subpixel tail becomes full binary authority in Presence-Amplitude. This can expose hard raster edges, candidate pop-in, or isolated distant pixels. That consequence follows the approved any-support rule and is not pre-emptively retuned in P12m.

### Validation

- Compare the Current selection/application blocks and protected shader caller byte-for-byte against the supplied pre-edit archive.
- Prove one million randomized cases and explicit `0`, smallest-positive, `1e-12`, `1e-8`, `1e-6`, `0.001`, `0.499999`, `0.5`, and `1.0` boundaries.
- Prove `productionSelected == candidateSelected * eligibilitySelected` and selected `finalFoamMask == 0`.
- Verify mode-specific Inspector descriptions, unchanged function signatures/call sites, unchanged shader properties/resources/kernels/serialized fields, balanced shader syntax/preprocessor structure, exact seven-file scope, and reproducible changed-file archive.
- Unity shader import and same-camera Candidate, Eligibility, Production, `Foam Chip And Strand Probe`, Final, and Current comparison remain authoritative.

### Status

Implementation complete in source; static consistency/compliance validation recorded in the P12m patch report. Unity import and visual acceptance pending.


## P12n implementation record — Optional Candidate-Straddle Chip Admission A/B — visually rejected

P12m corrected any-support binary thresholds but Unity evidence still showed sparse Production because complete analytical candidates were clipped by the derivative Eligibility band. P12n does not replace P12m. It adds a second selectable Presence-Amplitude route for direct comparison.

Implemented contract:

1. `Chip Application = Rendered Edge Band (Current)` preserves P12m and is the default.
2. `Chip Application = Candidate Straddle (Experimental)` is active only with Presence-Amplitude.
3. One low-frequency guarded RFloat cache stores binary admission by deterministic candidate lattice identity. Default refresh is `4 Hz`; authoring range is `1–8 Hz`.
4. Entry requires centre support `<= 0.08` and at least two of eight irregular-perimeter support contacts. Retention requires centre support `< 0.46` and at least one contact. Inactive/dormant candidates and impossible centre states return before perimeter work; the perimeter loop exits once the required count is reached.
5. The support evaluator uses interpolated previous/current state plus fixed-world-footprint pattern/lifecycle/Strand shaping. It is camera-independent and intentionally does not reproduce screen derivatives or surface deformation.
6. Final candidate geometry remains the existing render-frame analytical candidate. Experimental Production is the complete admitted candidate at that fragment, and application still removes only exact pre-Chip rendered Foam. Experimental candidate evaluation uses every positive exact pre-Chip rendered-mask pixel rather than the preserved route’s `0.08` BaseCoverage gate.
7. Cache unavailable/unsupported falls back to Rendered Edge Band. Switching away stops dispatch and invalidates hysteresis history.
8. Existing Candidate, Eligibility, Production, and final-mask debug identities are reused with route-specific meanings.

Actual project delta: `15` modified files and `4` created files, exactly matching approved scope. No scene, prefab, material, fixed spacing, Layer C state, Film, Shape, source, transport, cache asset, layer, tag, shader target, render pass, or draw call changed.

Offline validation passes: source-scope reconciliation; delimiter and preprocessor balance; new GUID uniqueness; compute kernel declaration/implementation/resolution; C# property-to-HLSL contract; render function signature/call parity; guarded lattice uniqueness/index coverage; exhaustive entry/retention Boolean equivalence; protected P12m fallback presence; byte-identical accepted Current application block. Unity compilation/import, measured GPU cost, and visual A/B result are pending.


## P12o implementation record — Original Analytical Candidates with Boundary-Anchored Eligibility

P12n is rejected because candidate-level admission allows a permitted candidate to move away from the edge and cut interior Foam. P12o removes that authority while retaining its separate A/B infrastructure.

Implemented contract:

1. `Chip Application = Rendered Edge Band (Current)` remains value `0`, default, fallback, and preserves P12m.
2. Enum value `1` is now `Boundary-Anchored Strip (Experimental)`. The original analytical Candidate loop remains the only candidate implementation in both routes.
3. The low-frequency cache is repurposed from scalar candidate admission to one ARGBFloat boundary descriptor per original candidate identity. Anchor occupies XY; Z packs lateral identity, state, and a 10-bit inward-normal angle; W stores exact longitudinal identity.
4. Initial acquisition samples the centre and stops at the first of eight deterministic ring directions with opposite binary support. That known outside/inside pair is refined by four binary-search steps, then a local four-axis probe refines and orients the inward normal.
5. Valid tracking starts from the previous anchor along the previous normal and samples intermediate points across the tracking interval so a thin ribbon cannot be skipped by two endpoints. Excessive displacement, missing outside-to-inside bracket, or normal discontinuity locks the record until dormancy resets it. The candidate cannot drag or teleport the permission strip.
6. Fragment Eligibility is an analytical local strip from approximately one antialias pixel outside through `Chip Edge Width` inward, with tangent extent bounded to the current candidate reach. Production is exact binary original Candidate × experimental Eligibility.
7. Candidate, Eligibility, Production, and final-mask debug identities are retained. Candidate must be route-identical; experimental Eligibility shows actual anchored strips; Production is their exact displayed relationship.
8. Exact pre-Chip rendered Foam remains the removal target. Current Presence Footprint, Layer C, sources, transport, Film, Shape, Strands, fixed spacing, scenes, prefabs, materials, caches, render passes, and draw calls are unchanged.

The moving analytical lattice uses a circular modulo cache. Absolute candidate identity is validated from the packed lateral coordinate and exact longitudinal coordinate, so ordinary origin movement preserves valid/locked history instead of globally reacquiring living candidates. Dimension changes still recreate history. The representable contract is longitudinal `±16,000,000` and lateral `-2048…2047` cells; unsupported identity ranges fall back to the current route.

Actual source delta remains inside the approved `17` modified paths. No file or `.meta` is created, deleted, moved, or renamed; the P12n auxiliary filenames are repurposed in place to avoid Unity asset churn.

Offline validation includes exact candidate-core and final-application comparison with P12m, exact Current branch comparison, 36-argument render signature/caller parity, compute C#/HLSL property parity, kernel declaration/resolution, circular-cache bijection and identity packing, unique in-place indexing, descriptor-state truth cases, boundary-acquisition and thin-ribbon tracking models, strict strip-depth truth, syntax/preprocessor balance, and shader-consumer audit. The pre-package audit passes `41/41` gates. A changed-files package was extracted and reproduced `17/17` source bytes; the delivery archive is rebuilt from this finalized record and rechecked. Unity compilation/import, visual A/B, and measured GPU cost remain pending.

## RG-METRIC-P12p implementation record — isolated rendered exterior-fringe Eligibility

### Decision

P12n and P12o are rejected by Unity visual evidence. The low-frequency candidate/boundary field is removed completely. The original analytical Candidate Field remains the sole candidate implementation, and one rendered Eligibility band remains the sole Presence-Amplitude permission route.

### Implementation

- Restored every P12n/P12o non-document implementation path to the immutable P12m baseline.
- Deleted `StylizedRiverFoamRuntime.ChipAdmission.cs` and its `.meta`.
- Deleted `CS_RiverFoam.ChipAdmission.hlsl` and its `.meta`.
- Removed the experimental route enum, refresh field, Inspector controls, runtime bindings, allocation/update/release logic, kernel, shader properties, descriptor texture, caller arguments, and fragment descriptor evaluation.
- Preserved the original analytical Candidate evaluation and Presence-Amplitude any-support binary Candidate × Eligibility/full-removal application.
- Changed only the Presence-Amplitude edge-coordinate source inside `RiverWaterFoamResolveChipEligibility`:

```hlsl
edgeSource = saturate(preChipRenderedMask);
exteriorFringeSource = min(edgeSource, 0.34);
exteriorEdgeCoordinate = saturate(
    (exteriorFringeSource - 0.08) / (0.34 - 0.08));
estimatedInwardPixels = exteriorEdgeCoordinate /
    max(length(float2(ddx(exteriorEdgeCoordinate),
                      ddy(exteriorEdgeCoordinate))), 0.001);
```

The authored `Chip Edge Width` smooth transition remains unchanged. Current Presence Footprint arithmetic remains byte-identical to P12m.

### Performance

P12p removes the P12o `ARGBFloat` descriptor allocation, low-frequency compute dispatch, cache lifecycle work, fragment descriptor loads, and boundary-strip arithmetic. Relative to P12m, the retained route remains arithmetic-only and adds no texture sample, loop, pass, buffer, texture, kernel, or dispatch.

### Validation state

Offline scope, reference, syntax, formula, protected-path, and package checks are required before delivery. Unity 6000.5 import, shader compilation, visual fringe removal, and GPU timing remain authoritative and pending.

## P12r implementation record — restore P12p rendered-edge Eligibility

**Outcome:** P12q is removed completely and the implementation is restored to the immutable P12p source.

**Modified existing files:** five canonical documents plus `StylizedRiver.cs`, `StylizedRiverEditor.Foam.cs`, `StylizedRiverEditor.DebugViews.cs`, `StylizedRiverFoamRuntime.Binding.cs`, `StylizedRiverFoamRuntime.Lifecycle.cs`, `StylizedRiverFoamRuntime.PublicSurface.cs`, `StylizedRiverFoamRuntime.Resources.cs`, `CS_RiverFoam.compute`, `RiverWaterFoam.hlsl`, and `SH_CleanStylizedRiver.shader`.

**Deleted files:** `StylizedRiverFoamRuntime.ChipTopology.cs`, its `.meta`, `CS_RiverFoam.ChipTopology.hlsl`, and its `.meta`.

**Restored behavior:** one original full-rate analytical Candidate Field; one per-fragment P12p rendered Eligibility band; exact `Candidate × Eligibility` Production; exact selected-pixel removal from `preChipRenderedMask`.

**Removed behavior:** all binary-topology allocation, update, erosion, texture binding, sampling, controls, defaults, memory accounting, and fallback logic.

**Validation status:** `58/58` offline exact-scope, P12p identity, deleted-file, topology-symbol removal, delimiter/preprocessor, shader-contract, and documentation gates pass. Unity 6000.5 compilation and visual acceptance remain authoritative and pending.



## RG-METRIC-P12s implementation record — optional Presence-Amplitude soft-mask reconstruction A/B

**Outcome:** source implemented; Unity compilation and visual acceptance pending.

P12s retains P12r as the default and adds one experimental render-only route:

```text
Presence-Amplitude Chip Application
0 = Exact Rendered Removal (Current)
1 = Soft-Mask Reconstruction (Experimental)
```

The original analytical Candidate evaluator is unchanged. The experimental Eligibility contract is:

```text
supportGate = preChipRenderedMask > 0 ? 1 : 0
softCoordinate = saturate(preChipSoftVisibility)
softDepthPixels = max(0, softCoordinate - SoftEdgeStart)
                  / max(fwidth(softCoordinate), 0.001)
softEdgeBand = supportGate
               × (1 - smoothstep(EdgeWidth - 0.5,
                                 EdgeWidth + 0.5,
                                 softDepthPixels))
production = continuousCandidate × softEdgeBand
```

`Soft Edge Start` is serialized in `0–0.25` with default `0.06`, matching the accepted historical Current route. Every positive exact no-Chip rendered pixel receives equal support authority; rendered amplitude does not attenuate Eligibility.

Experimental application reuses the accepted Current reconstruction exactly:

```text
postChipSoft = coherentSoftVisibility × (1 - production)
baselineHard = Harden(coherentSoftVisibility)
modifiedHard = Harden(postChipSoft)
postChipMask = hardenedShape × saturate(modifiedHard / baselineHard)
finalMask = postChipMask × structuralStrandKeep
```

The P12r route remains value `0`, serialized default, and exact final-mask removal. Current Presence Footprint ignores the selector and retains its existing accepted path. Presence-Amplitude Interior Access remains disabled.

Actual source scope is twelve modified existing files: five canonical documents, `StylizedRiver.cs`, `StylizedRiverEditor.Foam.cs`, `StylizedRiverEditor.DebugViews.cs`, runtime binding/constants, `RiverWaterFoam.hlsl`, and `SH_CleanStylizedRiver.shader`. No file or metadata is created, deleted, moved, or renamed.

No texture, sampler, buffer, kernel, dispatch, pass, draw call, loop, candidate-search expansion, fixed-grid, Layer C, Film, Shape, Strand, scene, prefab, material, cache, layer, tag, or component contract changes.

## P12t implementation record — Soft Reconstruction baseline and Layer D/E reconciliation

P12t promotes the visually accepted P12s soft-mask reconstruction to the sole Chipping application. The obsolete Exact Rendered Removal selector and branch contract are deleted across serialized authoring, runtime binding, shader properties, selection diagnostics, and final application. The original full-rate analytical Candidate evaluator and Current Presence Footprint behavior are preserved.

Inspector ownership now matches execution: Layer D contains diagnostic-only temporal evaluated-shape controls; Layer E contains Visibility & Footprint, Production Chipping, Structural Strands, and Final Composition. Presence-Amplitude Edge Start is displayed only for Presence-Amplitude; Chip Interior Access is displayed only for Current. Existing Chipping debug views are regrouped under Layer E without adding or deleting a view.

The patch adds no runtime resource, kernel, dispatch, texture, buffer, pass, draw call, loop, scene, prefab, material, cache, layer, tag, component, or fixed-grid change. The user accepted the resulting visual Chipping baseline as imperfect but sufficient; P12t is frozen and closed.


## Implementation record — `RG-METRIC-P12u — Unified Automatic Birth Reveal-Speed Contract`

### Decision

The previous automatic-source timing was not a consistent metres-per-second contract. Source families converted requested speed to duration and then applied independent caps or compression, while GPU progression consumed only normalized elapsed/duration. P12u replaces all family-specific automatic timing with one cadence-limited resolver.

### Implemented contract

1. Every automatic recipe resolves requested speed from its serialized base speed, serialized per-pattern multiplier, and existing deterministic jitter.
2. Every automatic recipe resolves duration as `max(materialStepDuration, pathDistance / requestedSpeed)`.
3. Arc/Semi-Arc apply this duration to Build only. Hold, Release, and Rest remain authored separately.
4. Fleck reveal now spans full normalized progress instead of completing in the first `18%`.
5. Contact Fleck and all Free-Water recipes now sample correlated Min/Max values across the complete deterministic range.
6. The original serialized field names and values are preserved; only Inspector labels/tooltips change to Base Reveal Speed / Reveal Speed Multiplier.
7. One Play Mode report records latest observed timing for every recipe, active-event detail, pool occupancy, and rejected starts and retains the adjacent clipboard-copy workflow.

### Performance

No new kernel, texture, buffer, dispatch class, render pass, or per-cell formula is introduced. Event-start CPU arithmetic is constant. A fixed nine-entry CPU telemetry array is added. Slow events can remain active longer and therefore increase existing raster dispatches, bounded by the unchanged 32-slot automatic-event pool.


## Implementation record — `RG-METRIC-P13A — Authoritative Birth Material and Coverage-Separated Transport`

### Decision

The former three-channel state used one scalar as both cell occupancy and material Presence. Source profiles, subcell coverage, valid-fluid clipping, transport diffusion, and birth overlap could therefore weaken authored Initial Presence and prevent new Initial Life from refreshing older weak material. P13A separates geometric occupancy from intrinsic material without adding storage.

### Persistent state contract

```text
material amount = Coverage × Presence
R = material amount
G = material amount × Remaining Life
B = material amount × Material Pattern
A = Coverage

Presence = R / A
Remaining Life = G / R
Material Pattern = B / R
```

A guarded zero-alpha fallback migrates transient pre-P13 RGB state without clearing visible material. New writes always include alpha Coverage.

### Birth and overlap contract

1. Source shape, taper, breakup, reveal progression, subcell width, family shaping, and valid-fluid clipping produce Coverage only.
2. Initial Presence and Initial Life are encoded as exact intrinsic values wherever nonzero Coverage is born.
3. Initial Presence is not passed through source-fill probability and is not multiplied by source geometry.
4. Existing weak or dying material cannot reject a fresh source. Overlap uses maximum Coverage, Presence, and Life; Pattern changes only where Coverage genuinely expands.
5. Negative topology, Neutral Lifetime, Supported Aging, and Negative Aging remain byte-identical authorities after birth.

### Transport contract

- Donor Cell remains the conservative first-order path and transports all packed moments with one donor state.
- TVD Superbee reconstructs bounded Coverage only, then re-encodes that Coverage with the selected donor's intrinsic Presence, Remaining Life, and Pattern. This prevents independent channel limiting from inventing invalid ratios.
- Mixing of physically different material through conservative flux produces explicit moment-weighted properties; ordinary movement alone does not silently attenuate a uniform material's decoded Presence or Life.
- Valid-fluid clipping reduces Coverage proportionally instead of clamping intrinsic Presence.
- Unit-capacity clipping after convergent flux also resolves Coverage coherently and preserves the decoded intrinsic ratios rather than saturating packed channels independently.

### Final visibility and Inspector contract

- `Concentration + Lifetime`: local Coverage concentration and Remaining Life both participate in visibility.
- `Lifecycle-Faithful`: meaningful Coverage establishes occupancy; continuous patterned life erosion is disabled while Layer C Life is positive, so explicit Layer C aging owns ordinary death.
- `Coverage-Only`: serialized value `Current`; Presence is stored but not used as final amplitude.
- `Presence-Amplitude`: the resolved Coverage/Life shape and its exact Presence-weighted counterpart use identical Presence-independent wake/warp/surface-coupling weights. Uniform Presence remains exactly proportional through the completed resolved mask.
- `Material Transport Scheme`, `Final Foam Visibility Mode`, and `Presence Footprint` are moved together to `Foam > Transport & Visibility Contract`. An always-visible read-only panel describes each selected mode, the combined behaviour, and Coverage/Presence/Life/Pattern meanings.

### Scope and performance

Nineteen existing files are modified: five canonical documents, six River authoring/editor/diagnostic C# files, two runtime state/diagnostic C# files, four compute contract/implementation files, and the shared River Foam include plus its sole production shader consumer. No file, metadata, scene, prefab, material, cache, texture, buffer, kernel, dispatch, pass, draw call, layer, tag, or component is created, removed, moved, or renamed.

Persistent memory and dispatch counts are unchanged. TVD arithmetic changes but remains one bounded reconstruction per interior face; cost is unmeasured. More visible/live Foam is an intentional correctness consequence and requires later explicit tuning rather than hidden suppression. Offline model/static validation passes 25/25 checks; Unity compilation, live visual/lifetime evidence, and profiler measurements remain pending.
