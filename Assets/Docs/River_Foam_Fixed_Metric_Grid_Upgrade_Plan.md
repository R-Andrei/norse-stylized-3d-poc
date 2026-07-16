# River Foam Fixed-Metric Grid Upgrade Plan

## 0. Document control

| Field | Value |
|---|---|
| Proposed canonical repository path | `Assets/Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md` |
| Companion dependency register | `Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md` |
| Date | 2026-07-17 |
| Project | Norse Stylized 3D PoC |
| Engine | Unity 6000.5.0f1, URP |
| Work type | Canonical architecture and implementation plan |
| Architecture status | Fixed-metric, centreline-relative river-space lattice accepted as the direction |
| Implementation status | **`RG-METRIC-P0` static snapshot review complete; live Git/Unity baseline blocked; `RG-METRIC-P1` documentation scope lock complete** |
| Code authorization | **First documentation-only patch authorized on 2026-07-17; no runtime, compute, shader, serialized-asset, or generated-cache implementation was authorized or performed by this patch** |
| Persistent game-file changes made while producing this document | Documentation only: this plan, its companion dependency register, and the active River Foam queue |
| Source snapshot used | User-supplied `Assets(71).zip` |
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
| Runtime baseline capture | `BLOCKED`: supplied archive contains no runnable project root, current `Editor.log`, profiler capture, or Unity screenshots |
| Canonical queue reconciliation | Complete for documentation patch 01 |
| Runtime implementation | Not started |
| Unity validation | Not started |
| Strip-pool production architecture | Planned future phase; not implemented |

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
rows = ceil(5/0.15) = 34
allocated texels = 7,276
8x8 launched envelope = 216x40 = 8,640
current Medium = 96x96 = 9,216
```

This predicts approximately 21% fewer allocated texels and 6.25% fewer launched threads per 32-metre region, before accounting for valid-mask waste, resource mix, cache behavior, and GPU access patterns.

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
| `RG-METRIC-P2` | Descriptor, quality mapping, and CPU/GPU contract foundation | `NOT STARTED` |
| `RG-METRIC-P3` | One-strip allocation and canonical CPU field-space mapping | `NOT STARTED` |
| `RG-METRIC-P4` | Cache contracts, cache tooling, and initialization compatibility | `NOT STARTED` |
| `RG-METRIC-P5` | Metric rows, topology, boundary, and obstacle exclusion | `NOT STARTED` |
| `RG-METRIC-P6` | Obstacle routing, Motion Lane, and external-field integration | `NOT STARTED` |
| `RG-METRIC-P7` | Automatic/manual source migration and unit policy | `NOT STARTED` |
| `RG-METRIC-P8` | Persistent transport, CFL, curvature, and topology replacement | `NOT STARTED` |
| `RG-METRIC-P9` | Film occupancy, shape evaluation, and production rendering | `NOT STARTED` |
| `RG-METRIC-P10` | Diagnostics, inspector semantics, and documentation | `NOT STARTED` |
| `RG-METRIC-P11` | Mechanical verification and full consistency audit | `NOT STARTED` |
| `RG-METRIC-P12` | Unity candidate sweep and visual/performance selection | `NOT STARTED` |
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

**Blocking conclusion:** `RG-METRIC-P0` remains `BLOCKED` until the live workspace provides Git evidence and the required Unity/runtime baseline package. No `RG-METRIC-P2` implementation edit is permitted before that evidence is recorded here or in an approved linked baseline record.

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
5. Recorded the fixed-metric program as active only at the baseline/scope-lock gate. Runtime code remains unmodified and blocked by the incomplete live portion of `RG-METRIC-P0`.

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

### 17.7 `RG-METRIC-P6` — Obstacle routing, Motion Lane, and external-field integration

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

### 17.8 `RG-METRIC-P7` — Automatic/manual source migration and unit policy

**Objective:** Migrate every source family and authoring unit without preserving accidental cell anisotropy.

**Required work:**

- classify and record every serialized/public Foam source parameter;
- migrate CPU event preparation and dispatch culling to metric ranges;
- migrate GPU event raster cell centres and local normal spacing;
- establish physical core/feather behavior for Arc/Semi-Arc;
- migrate Shore Ribbon cell-authored thickness/variation deliberately;
- validate all eight automatic source families;
- migrate manual ellipse, stroke, compound, clear-range, isolated probe, birth transfer, and debug source paths;
- retain lifecycle, weights, seeds, event ownership, and pattern authority unless explicitly tuned.

**Verification:** physical bounds, minimum footprint, progression, continuity, flow reversal, clipping, event-cap saturation, no rectangular macro blocks, exact debug/production source parity.

**Stop conditions:** accepted Arc/Semi-Arc shape cannot be preserved at candidate scale; a serialized field would change meaning without compatibility; one source family requires an unplanned state/resource change.

**Rollback checkpoint:** source-metric baseline before transport and render closure.

### 17.9 `RG-METRIC-P8` — Persistent transport, CFL, curvature, and topology replacement

**Objective:** Make material movement numerically correct under fixed metric coordinates.

**Required work:**

- use descriptor spacing for velocity conversion and CFL;
- validate conservative flux and endpoint outflow;
- add curvature diagnostics and adopt bounded or corrected policy;
- update transport metrics physical interpretation;
- migrate current/previous descriptor mapping for topology replacement;
- define clear/remap behavior for incompatible descriptors;
- validate flow reversal and multi-substep behavior.

**Verification:** no-birth/death conservation; moments; widening/narrowing; bends; obstacle diversion; endpoint outflow; topology replacement; candidate CFL; 40 m curvature stress.

**Stop conditions:** conservation exceeds tolerance; an extra substep violates budget; curvature policy is unresolved for required cases; state replacement teleports/duplicates/loses material.

**Rollback checkpoint:** metric source deposition baseline before persistent evolution.

### 17.10 `RG-METRIC-P9` — Film occupancy, shape evaluation, and production rendering

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

### 17.11 `RG-METRIC-P10` — Diagnostics, inspector semantics, and documentation

**Objective:** Make the new contract observable and remove stale authoring/documentation semantics.

**Required work:**

- add compact descriptor, waste, cache, CFL, curvature, source-area, memory, and dispatch diagnostics;
- update existing debug views rather than proliferating redundant views;
- update inspector labels/tooltips and compatibility messaging;
- keep serialized values stable unless migration is approved;
- update active blockers, Stage 6 architecture, rendering roadmap, handoff, and this plan statuses;
- mark superseded normalized-lateral instructions explicitly.

**Verification:** inspector persistence; no reserialization on inspection; debug/world alignment; documentation cross-reference audit; console remains concise.

**Stop conditions:** diagnostics require expensive continuous readback beyond accepted policy; inspector changes reset values; documentation contradicts code.

**Rollback checkpoint:** fully working code with old/new diagnostics compared before docs are finalized.

### 17.12 `RG-METRIC-P11` — Mechanical verification and full consistency audit

**Objective:** Prove the implementation is internally consistent before asking the user for Unity visual validation.

**Required work:**

- run an available real C# parser/compiler over every changed C# file;
- scan changed C# for required namespaces and malformed multiline strings;
- preserve line endings;
- parse/code-generate changed HLSL functions and verify kernel declarations/resources;
- verify all CPU/GPU struct strides and property bindings;
- search for old normalized structural-Y formulas and duplicate spacing derivations;
- compare final diff with approved file scope and every plan item;
- reread complete changed files and affected callers/consumers/producers;
- compare against baseline, HEAD, and accepted/superseded implementations;
- update plan status/evidence.

**Completion condition:** zero unresolved reference, syntax, ABI, stale-formula, scope, or documentation defects.

**Stop conditions:** Unity-only validation remains pending, but mechanical failures are blockers and cannot be deferred.

**Rollback checkpoint:** mechanically verified candidate package.

### 17.13 `RG-METRIC-P12` — Unity candidate sweep and visual/performance selection

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

### 17.14 `RG-METRIC-P13` — Final tier tuning, cache freeze, and contiguous baseline closure

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
