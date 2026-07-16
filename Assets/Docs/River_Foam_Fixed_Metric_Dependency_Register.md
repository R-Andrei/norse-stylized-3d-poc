# River Foam Fixed-Metric Grid Migration — Dependency Register

## 1. Document identity

- Date: 2026-07-17
- Work type: Canonical dependency register and static pre-implementation audit
- Implementation status: No runtime implementation; static `RG-METRIC-P0` review complete for the supplied snapshot; live repository and Unity baseline pending
- Canonical repository path: `Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
- Source snapshot: User-supplied `Assets(71).zip`, used as the authoritative supplied-file snapshot for documentation patch 01
- Prior design sources:
  - `River Foam Fixed-Metric Resolution Handoff`
  - Follow-up audit response accepting the coordinate-contract corrections
- Persistent changes in documentation patch 01: this register, the fixed-metric upgrade plan, their Unity `.meta` files, and the reconciled active River Foam queue; no code, shader, scene, prefab, material, cache, or generated-data changes

## 2. Purpose

This document is the standalone dependency checklist for migrating River Foam from its current grid—fixed approximately in downstream metres but normalized across each local river row—to a fixed-metric, centreline-relative river-space lattice.

It identifies every dependency discoverable by static inspection of the supplied source snapshot that must be:

1. updated during the migration;
2. resolved by an explicit design decision;
3. regression-tested even if its code remains unchanged; or
4. deferred explicitly to the later strip/pooling architecture.

This is not an implementation plan and does not authorize code changes.

## 3. Completeness statement and limits

### 3.1 Verdict on the earlier dependency list

The earlier list was **not exhaustive**. It covered the primary allocation, topology, source, compute, render, and cache path, but it did not fully enumerate several indirect contracts:

- topology morphology and cell-count thresholds;
- motion-lane generation and obstacle-routing morphology;
- the half-resolution visual-occupancy field;
- quality-linked birth budgets and cadence;
- topology replacement and state remapping;
- build preflight and development cache tooling;
- manual injection and isolated-life probes;
- disturbance-field integration;
- resolution-dependent diagnostics and metrics;
- authoring labels and serialized unit semantics;
- runtime reallocation, flow reversal, and domain-change behavior;
- curvature-dependent physical area on wide bends;
- the complete future strip boundary, renderer-indirection, and scheduling contract.

### 3.2 What “complete” means here

Within the supplied source snapshot, this register is intended to be an **exhaustive static dependency inventory** for the fixed-metric migration. It includes:

- all River Foam C# files;
- all River Foam compute and render shader files;
- all direct users of the current CPU field-space helper;
- all externally visible Foam consumers found in the supplied `Game/` tree;
- cache, editor, preflight, diagnostics, scene, river-domain, disturbance, obstacle, and documentation integration points.

It cannot prove dependencies that are absent from the supplied snapshot, created dynamically by external packages, injected through unpublished tooling, or visible only in runtime data. The archive contains no `.git` metadata and no complete project root, package manifest, Library, or current `Editor.log`. A final pre-implementation repository audit must repeat the reference scan against the live workspace.

### 3.3 Static execution record for documentation patch 01

The complete 94-path file register in this document was re-read from the supplied snapshot before the first documentation patch was produced.

| Check | Result |
|---|---|
| Registered paths present | 94 of 94 |
| Missing paths | 0 |
| Aggregate bytes reviewed | 3,804,092 |
| Aggregate lines reviewed | 95,910 |
| Existing line endings | 62 LF files and 32 CRLF files |
| Normalized-lateral indicator files | 22 |
| Field-dimension indicator files | 32 |
| Relevant spacing indicator files | 20 |
| Cache-contract indicator files | 6 |
| Shared-quality indicator files | 18 |
| Curvature indicator files | 12 |

This establishes static inventory completeness only. Git-only dependencies, package dependencies outside the archive, runtime registrations, generated caches, active serialized state, and Unity-only behavior remain pending live verification.

## 4. Classification legend

| Code | Meaning |
|---|---|
| **U** | Mandatory implementation update for the contiguous fixed-metric Stage 1 |
| **D** | Mandatory design decision before implementation |
| **T** | Mandatory regression or integration test; code change is not automatically justified |
| **F** | Future strip/pooling dependency; not part of contiguous Stage 1 unless separately approved |
| **R** | Mandatory code review because a change is conditional on the final descriptor or unit policy |

A dependency can carry multiple classifications, such as **U/D/T**.

## 5. Migration invariant

Every system must use one authoritative metric-grid descriptor. No subsystem may independently reconstruct lateral Foam UV from local river width or derive spacing from a different length.

Conceptual descriptor:

```text
mappingContractVersion
columnsPer32MetreChunk
resolvedDxMetres
resolvedDyMetres
lateralLatticeOriginMetres
localGlobalYBase
rowCount
fieldOrStripStartMetres
fieldOrStripLengthMetres
validLengthMetres
```

Cell centre:

```text
s = fieldOrStripStart + (x + 0.5) * resolvedDx

globalY = localGlobalYBase + localY
n = lateralLatticeOrigin + (globalY + 0.5) * resolvedDy
```

“Global” means shared by strips belonging to one river or connected river network. It does **not** mean a world-aligned XZ lattice.

## 6. Top-level dependency matrix

| Dependency area | Class | Why it is affected | Minimum acceptance condition |
|---|---:|---|---|
| Grid allocation and dimensions | U/D/T | Current width and height encode two different physical semantics | Resolved `dx/dy`, origins, dimensions, and valid length come from one descriptor |
| CPU field-space conversion | U/T | Y currently means normalized cross-river fraction | CPU metre-to-cell and cell-to-metre round trips match the metric lattice |
| Compute coordinate conversion | U/T | Compute independently reconstructs normalized lateral position | GPU cell centres match CPU positions within tolerance |
| Production renderer sampling | U/R/T | Renderer divides lateral metres by local surface half-width | Render sampling addresses the same lattice as simulation |
| Topology generation | U/R/T | Morphology and placement use current field cells and normalized across values | Generated topology preserves intended physical shapes and valid-bank clipping |
| Boundary generation | U/D/T | Boundary feather and bank support use quality-specific cell counts | Physical boundary thickness is explicitly preserved or deliberately changed |
| Obstacle exclusion | U/T | Mesh occupancy is rasterized into current cell coordinates | Exact-mesh footprint aligns with the metric field at all widths and bends |
| Obstacle routing | U/D/T | Approach, closure, margins, and BFS operate in field cells | Routing envelopes preserve intended physical reach and no forbidden wrap occurs |
| Motion lane | U/D/T | Noise aspect, smoothing, and scroll use field dimensions/cells | Physical lane scale and scroll speed remain stable across river widths |
| Automatic birth-source dispatch | U/T | CPU culls event Y ranges using normalized lateral coordinates | Every event dispatch covers exactly the metric cells intersecting its physical bounds |
| Automatic source geometry | U/D/T | Several widths and feathers are cell-relative | Each parameter is classified as metres, cells, or sampling support |
| Manual injection and probes | U/R/T | Normalized source coordinates and cell dispatch ranges depend on old mapping | Manual strokes, ellipses, compounds, and probes land at correct physical positions |
| Persistent material transport | U/D/T | Current neighbour rows are normalized-fraction neighbours | Transport follows equal lateral metre positions and remains conservative |
| CFL and substep policy | U/T | Resolved `dx/dy` change stability terms | Runtime reports correct spacing, CFL, and substep count for every quality candidate |
| Curvilinear area/face metrics | D/R/T | Wide curved rivers have `J ≈ 1 - κn`, while solver uses `dx*dy` | Approximation bound or corrected metrics are explicitly adopted |
| Topology replacement transition | U/T | Current-to-previous mapping uses old normalized field coordinates | Rebuild/replacement preserves state without lateral jumps or smearing |
| Half-resolution visual occupancy | U/R/T | Film dimensions and represented-cell area derive from the structural field | Layer D remains physically aligned, conservative enough, and visually stable |
| Shape evaluation and boundary application | U/R/T | Full/half field mapping and cell-relative features change | Final shape, breakup, and clipping remain stable at the selected metric scale |
| Disturbance-field integration | T/R | Foam samples independently normalized Pressure/Wake/Ripple fields | Sampling at each metric Foam cell resolves the same world/river point |
| River-domain and geometry inputs | T/R | Centreline distance, normals, left/right widths, and curvature define the lattice | Asymmetric widths, bends, flow reversal, and domain changes remain valid |
| Quality policy | D/U/T | Shared quality enum also controls non-Foam systems | Foam gets a separate metric mapping without altering geometry/disturbance quality semantics |
| Birth budgets and capacities | D/R/T | Fixed events-per-step do not scale automatically with water area | Density and saturation remain acceptable from 5 m through wide-river cases |
| Resource lifetime and reallocation | U/T | Grid descriptor changes invalidate textures, buffers, and cached state | Reallocation is deterministic and state-loss behavior is explicit |
| GPU/CPU data-layout parity | U/T | Descriptor or source structs may gain/change fields | C# and HLSL strides, ordering, and values match exactly |
| Cache package and fingerprints | U/D/T | Existing caches encode normalized-grid products | Old caches miss deterministically and new caches fingerprint every mapping parameter |
| Cache build/preflight tooling | U/R/T | Build requires exact prepared cache artifacts | Editor preparation, release preflight, and stale reasons remain correct |
| Diagnostics and metrics | U/D/T | Many values are texel counts or cell perimeters | Physical area/length is reported where cell counts are no longer comparable |
| Memory/work accounting | U/T | Dimensions and later strips change resource and dispatch counts | Reported bytes, cells, dispatches, and iteration rates equal actual allocations/work |
| Debug views | U/T | Cell overlays and automatic-source views assume normalized Y | Overlays line up with world geometry and expose the metric descriptor |
| Inspector/authoring units | U/D/T | Help text still describes 64/96/128 across-river rows and cell-based widths | Labels and serialized migration semantics are unambiguous |
| Documentation | U | Architecture and roadmap describe the old coordinate contract | Canonical docs and active queue match the implemented design |
| Scene/prefab assets | T only | Existing serialized settings are validation inputs | No raw scene/prefab edit; existing scene continues to load and behave correctly |
| Future strip allocation | F/D/T | Required for local-width scaling and long rivers | Shared global-Y lattice, boundaries, pools, budgets, and renderer lookup are defined |

## 7. Detailed dependency register

### 7.1 Grid ownership, allocation, and runtime state

**Class: U/D/T**

Dependencies:

- quality-to-grid mapping;
- 32-metre chunk quantization;
- valid versus padded downstream length;
- lateral lattice origin and global-Y base;
- texture dimensions;
- maximum texture and cache dimensions;
- structural and half-resolution film dimensions;
- initialization signatures;
- allocated-quality tracking;
- resource rebuild and release rules;
- public field dimension and spacing properties.

Required decisions:

1. Whether requested metric sizes are rounded by changing `columnsPer32MetreChunk` and using `resolvedDx = 32 / columns`.
2. How the lateral lattice phase is selected and kept stable across rebuilds.
3. How far left/right the contiguous Stage 1 field allocates beyond local banks.
4. What happens when fixed metric dimensions exceed hardware or cache limits. Silent spatial degradation is not acceptable.
5. Whether quality changes clear material state or attempt a resampling transition.

Mandatory tests:

- exact 32 m, just under, and just over 32 m lengths;
- final padded chunk;
- asymmetric left/right widths;
- 5, 10, 20, and 40 m widths;
- texture-limit and cache-limit failure paths;
- disabled/enabled lifecycle;
- repeated initialization without leaks;
- quality switching;
- domain-version changes.

Primary files:

- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.RuntimeUpdates.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.cs`

### 7.2 Canonical CPU field-space mapping

**Class: U/T**

The current helper maps Y texels to `Across01`, then maps that normalized value through each row’s own left/right width. This must become fixed signed lateral metres.

Every direct caller must be migrated or explicitly retired:

- `StylizedRiverFoamConnectorTopologyGenerator.cs`
- `StylizedRiverFoamMajorTopologyGenerator.cs`
- `StylizedRiverFoamPocketTopologyGenerator.cs`
- `StylizedRiverFoamTopologyFieldSpace.cs`
- `RiverObstacleExclusionResolver.cs`
- `StylizedRiverFoamRuntime.Evolution.Connector.cs`
- `StylizedRiverFoamRuntime.Evolution.Major.cs`
- `StylizedRiverFoamRuntime.Injection.cs`
- `StylizedRiverFoamRuntime.Obstacles.cs`
- `StylizedRiverFoamRuntime.Resources.cs`
- `StylizedRiverFoamRuntime.RuntimeUpdates.cs`
- `StylizedRiverFoamRuntime.Topology.cs`

Mandatory mapping tests:

- cell centre to metres;
- metres to fractional cell;
- metres to nearest cell;
- left and right asymmetric widths;
- negative and positive lateral positions;
- exact lattice boundaries;
- out-of-field and out-of-bank positions;
- CPU/GPU parity;
- normal and reversed flow direction;
- strip-compatible global-Y indexing even in one-strip Stage 1.

### 7.3 Metric-row and CPU/GPU ABI contracts

**Class: U/R/T**

`FoamMetricRow`, automatic source-event structs, obstacle interval cells, and compute uniforms are shared CPU/GPU contracts. A new descriptor can be passed as uniforms, embedded in per-row data, or represented through a separate buffer, but the choice must be explicit.

Dependencies:

- `FoamMetricRow` spacing and topology data;
- source-event centre, extents, and cell-relative fields;
- object source `LateralCellSpacingMetres` or equivalent;
- `ShoreRibbonThicknessCells` and variation fields;
- buffer stride constants;
- shader property IDs;
- dispatch dimensions;
- fallback/neutral resources.

Primary files:

- `StylizedRiverFoamRuntime.State.cs`
- `StylizedRiverFoamRuntime.Binding.cs`
- `StylizedRiverFoamRuntime.Compute.cs`
- `CS_RiverFoam.Structs.hlsl`
- `CS_RiverFoam.Resources.hlsl`

Mandatory tests:

- `Marshal.SizeOf`/stride parity where available;
- source-event field ordering;
- metric-row buffer content inspection;
- neutral/fallback binding;
- no stale property reuse after reallocation.

### 7.4 CPU topology generators and topology morphology

**Class: U/D/T**

Topology is not merely positioned on the field; it contains morphology expressed in cells. Changing cell size can alter support widths, connector lengths, pocket rejection, local masks, dilation/erosion, junction continuity, and candidate ranking.

Dependencies:

- metric-position arrays;
- nearest-cell and fractional-cell conversion;
- normalized across anchors retained in topology records;
- local candidate masks;
- half extents expressed in cells;
- connector paths and widths;
- pocket masks and boundaries;
- major/connector/pocket overlap;
- boundary edge-cell constants;
- obstacle-aware topology generation.

Primary files:

- `FoamTopology/StylizedRiverFoamMajorCandidate.cs`
- `FoamTopology/StylizedRiverFoamMajorCandidateGenerator.cs`
- `FoamTopology/StylizedRiverFoamMajorTopology.cs`
- `FoamTopology/StylizedRiverFoamMajorTopologyGenerator.cs`
- `FoamTopology/StylizedRiverFoamConnectorTopology.cs`
- `FoamTopology/StylizedRiverFoamConnectorTopologyGenerator.cs`
- `FoamTopology/StylizedRiverFoamPocketTopology.cs`
- `FoamTopology/StylizedRiverFoamPocketTopologyGenerator.cs`

Required decisions:

- which morphology distances should preserve physical metres;
- which values genuinely represent raster connectivity and should remain cells;
- whether normalized across anchors remain authoring data while raster placement becomes metric;
- whether candidate budgets need area scaling.

Mandatory tests:

- topology continuity at changing width;
- support shape physical dimensions;
- major/connector/pocket counts and area;
- no one-cell gaps introduced by rounding;
- no topology outside banks;
- narrow-river and wide-river equivalence in metres;
- obstacle-adjacent topology;
- deterministic output and cache parity.

### 7.5 Boundary and shore support

**Class: U/D/T**

Boundary generation currently uses quality-specific edge thicknesses in cells and per-row lateral spacing. The fixed metric lattice changes their physical width.

Dependencies:

- bank-edge feather;
- valid-water mask;
- shore support and shore negative aging;
- current shore-edge extraction;
- row-local left/right widths;
- padded downstream area;
- shape-stage boundary application.

Primary files:

- `StylizedRiverFoamRuntime.Topology.cs`
- `StylizedRiverFoamRuntime.Obstacles.cs`
- `StylizedRiverFoamMajorTopologyGenerator.cs`
- `CS_RiverFoam.Topology.hlsl`
- `CS_RiverFoam.Support.hlsl`
- `CS_RiverFoam.compute` kernels:
  - `BuildCurrentShoreEdges`
  - `ComposeTopology`
  - `ApplyBoundary`

Mandatory decision:

- preserve the current physical shore band, preserve current cell count, or redesign it deliberately.

Mandatory tests:

- both banks independently;
- asymmetric banks;
- tight bends;
- width transitions;
- no Foam outside the valid surface;
- no new dark/empty bank seam;
- shore birth and shore support remain aligned.

### 7.6 Obstacle exclusion rasterization

**Class: U/T**

Exact-mesh obstacle occupancy is converted into field cells. Every cell interval and cache artifact depends on the old coordinate mapping.

Dependencies:

- world/river-space obstacle projection;
- metric-to-cell conversion;
- compact interval encoding;
- GPU obstacle-cell buffer;
- exclusion texture update/readback;
- obstacle fingerprints;
- obstacle geometry version and stable registry timing;
- topology-cache inclusion.

Primary files:

- `RiverObstacleExclusionResolver.cs`
- `StylizedRiverFoamRuntime.Obstacles.cs`
- `StylizedRiverFoamRuntime.TopologyCache.cs`
- `CS_RiverFoam.compute` kernels:
  - `ClearObstacleExclusion`
  - `UpdateObstacleExclusion`
  - `BuildFoamObjectContactField`

Mandatory tests:

- rotated and sloped silhouettes;
- very small obstacles;
- obstacles touching banks;
- hidden renderer but active simulation obstacle;
- exact front/back/side occupancy;
- no one-cell holes;
- stable cache fingerprinting;
- rebuild after obstacle transform/version changes.

### 7.7 Obstacle routing and pressure support

**Class: U/D/T**

Routing currently uses cell-space BFS and multiple reach/margin constants derived from field dimensions or fixed cell counts. The metric migration can radically change their physical extent.

Dependencies:

- approach cells;
- front cells and front closure;
- lateral margins;
- contact cells;
- upstream support search;
- pressure envelope thickness;
- obstacle routing texture;
- obstacle slowdown and minimum downstream factor;
- flow direction.

Primary files:

- `StylizedRiverFoamRuntime.Obstacles.cs`
- `CS_RiverFoam.Motion.hlsl`
- `CS_RiverFoam.Support.hlsl`
- `CS_RiverFoam.compute`

Mandatory decisions:

- convert physical routing reaches to metres;
- retain only connectivity/search support in cells;
- define physical behavior around obstacles independently of river width and quality.

Mandatory tests:

- long and short objects;
- rotated objects;
- no full O-wrap around objects;
- correct C/∪ routing;
- no upstream spawn or motion leakage;
- narrow and wide rivers;
- forward and reversed flow;
- routing continuity through width changes.

### 7.8 Motion lane field

**Class: U/D/T**

The motion lane is a Foam-grid texture. Its procedural pattern, smoothing, aspect correction, scroll, CPU readback, and obstacle integration all depend on field dimensions.

Dependencies:

- normalized U/V noise input;
- `fieldWidth / fieldHeight` aspect treatment;
- lane scale and wavelength;
- smoothing radius in cells;
- scroll metres-to-cells conversion;
- field signature and rebuild rules;
- full-field CPU data/readback;
- sampling in transport and rendering.

Primary files:

- `StylizedRiverFoamRuntime.Obstacles.cs`
- `StylizedRiverFoamRuntime.Compute.cs`
- `CS_RiverFoam.Motion.hlsl`
- `RiverWaterFoamVelocity.hlsl`

Mandatory decisions:

- author lane scale in physical metres or normalized river proportions;
- preserve current visual frequency or current numerical settings;
- determine whether wide rivers should contain more independent lanes.

Mandatory tests:

- physical wavelength across widths;
- downstream scroll speed;
- no stationary/repainted canonical velocity bug;
- no zero-speed regions;
- obstacle-routing blend;
- renderer and simulation sample the same field.

### 7.9 Automatic birth scheduling and budgets

**Class: D/R/T**

The event scheduler is not directly a coordinate transform, but its fixed per-step event budgets and source capacities determine Foam density. A wider metric field has more cells and more physical area, while current budgets remain tied only to quality.

Dependencies:

- Low/Medium/High birth budgets;
- maximum automatic events per dispatch;
- pattern selection weights;
- formation cadence;
- held/active source scheduling;
- per-source event lifetime;
- event suppression and overlap.

Primary files:

- `StylizedRiverFoamRuntime.Constants.cs`
- `StylizedRiverFoamRuntime.BirthEvents.cs`
- `StylizedRiver.cs`

Mandatory decision:

- keep births authored per river, scale by active surface area/length, or establish explicit area-independent composition rules.

Mandatory tests:

- equal visual density on 5, 10, 20, and 40 m widths;
- event-cap saturation;
- no cadence changes caused solely by extra cells;
- pattern weights remain respected;
- deterministic seed behavior.

### 7.10 Automatic source geometry

**Class: U/D/T**

Every automatic source family must be reviewed separately. The migration cannot be validated by checking Arc alone.

Source types:

1. Shore Ribbon
2. Inward Wash
3. Object Contact Arc
4. Object Contact Semi-Arc
5. Object Contact Fleck
6. Free Water Lace Connector
7. Free Water Cross-Lace Connector
8. Free Water Torn Fragment

Primary files:

- `StylizedRiverFoamRuntime.State.cs`
- `StylizedRiverFoamRuntime.BirthEvents.cs`
- `StylizedRiverFoamRuntime.Injection.cs`
- `StylizedRiverFoamRuntime.Evolution.*.cs`
- `CS_RiverFoam.Evolution.hlsl`
- `CS_RiverFoam.Noise.hlsl`
- `CS_RiverFoam.compute` source evaluators and raster kernels

Per-parameter decision rule:

- **Metres:** visually meaningful length, width, offset, inward reach, trail length, and physical feather.
- **Cells:** raster support, connectivity, minimum sample coverage, or strictly discrete operations.
- **Normalized:** only proportions that intentionally scale with host/source geometry or local river width.

Mandatory source-family tests:

- physical bounding length and width;
- minimum one-cell footprint;
- build, hold, and release progression;
- source continuity;
- orientation invariance;
- flow reversal;
- bank clipping;
- obstacle clipping;
- no detached fragments unless intended;
- no cell-shaped rectangular blocks at accepted metric quality;
- no accidental physical widening of Shore Ribbon;
- accepted Arc/Semi-Arc C-shape preserved.

### 7.11 Manual injection and isolated probes

**Class: U/R/T**

Manual ellipse, stroke, compound, clear-range, and isolated-life-probe paths can bypass automatic-source preparation. They must use the same metric descriptor.

Dependencies:

- normalized centre coordinates;
- metric radii and stroke endpoints;
- dispatch range culling;
- clear range;
- source read/write textures;
- isolated probe cell placement;
- debug source masks.

Primary files:

- `StylizedRiverFoamRuntime.Injection.cs`
- `StylizedRiverFoamRuntime.BirthTransfer.cs`
- `StylizedRiverFoamRuntime.BirthDiagnostics.cs`
- `StylizedRiverEditor.Actions.cs`
- `CS_RiverFoam.compute` kernels:
  - `ClearRange`
  - `InjectFoam`
  - `WriteIsolatedLifeProbe`
  - `ClearAutomaticBirthDebugAll`

Mandatory tests:

- placement at centre and both banks;
- physical ellipse dimensions;
- long strokes across width changes;
- clear exact region;
- probe lifetime unaffected by position;
- no old normalized-Y assumptions remain.

### 7.12 Persistent material transport

**Class: U/D/T**

The fixed metric Y lattice corrects the current hidden squeeze/stretch in which equal Y indices on adjacent rows can represent different lateral metre positions. Transport must nevertheless be revalidated fully.

Dependencies:

- neighbour addressing;
- downstream and lateral velocity conversion to cells;
- per-cell area;
- face lengths and conservative flux;
- obstacle footprint and routing;
- boundary clipping;
- endpoint outflow;
- presence/life/pattern moment conservation;
- CFL/substep calculation;
- metrics and fixed-point accumulation.

Primary files:

- `StylizedRiverFoamRuntime.RuntimeUpdates.cs`
- `StylizedRiverFoamRuntime.Compute.cs`
- `StylizedRiverFoamRuntime.Lifecycle.cs`
- `CS_RiverFoam.Simulation.hlsl`
- `CS_RiverFoam.Transport.hlsl`
- `CS_RiverFoam.Motion.hlsl`
- `CS_RiverFoam.compute` kernel `SimulateFoam`

Mandatory decisions:

- whether Stage 1 retains rectangular `dx*dy` area on bends;
- accepted `max(abs(curvature * lateralOffset))` approximation bound;
- whether wide/high-curvature sections are rejected, subdivided, or corrected using the curvilinear Jacobian and face metrics.

Mandatory tests:

- mass conservation with no birth/death;
- life and pattern-moment conservation;
- constant-width straight river;
- widening/narrowing river;
- left and right bends;
- 40 m width stress case;
- endpoint outflow;
- obstacle diversion;
- one- and multi-substep cases;
- flow reversal;
- no lateral jump at topology replacement.

### 7.13 Topology replacement and previous-state remapping

**Class: U/T**

The runtime can replace topology while preserving or transitioning state. The transition shader currently reconstructs positions using the existing normalized coordinate model.

Dependencies:

- current descriptor;
- previous descriptor;
- previous/current dimensions and lengths;
- current-to-previous metric mapping;
- state snapshots;
- topology-transition textures;
- change detection and lifetime.

Primary files:

- `StylizedRiverFoamRuntime.TopologyReplacement.cs`
- `StylizedRiverFoamRuntime.Lifecycle.cs`
- `StylizedRiverFoamRuntime.Compute.cs`
- `CS_RiverFoam.TopologyTransition.hlsl`
- `CS_RiverFoam.compute`

Mandatory tests:

- width changes;
- domain extension/shortening;
- quality changes;
- obstacle-triggered topology replacement;
- no state teleportation, duplication, loss, or lateral scale distortion;
- correct behavior when descriptors are incompatible and state must be cleared.

### 7.14 Half-resolution visual occupancy and film fields

**Class: U/R/T**

Layer D uses half-resolution fields derived from structural width and height. It computes represented cell count and physical area, advances visual occupancy, and feeds shape evaluation/rendering.

Dependencies:

- `filmWidth` and `filmHeight` rounding;
- full-to-film and film-to-full mapping;
- represented structural-cell count at odd edges;
- visual occupancy cell area;
- film source and support;
- visual occupancy advection;
- visual occupancy texture binding;
- debug descriptions that assume one film texel represents four structural cells.

Primary files:

- `StylizedRiverFoamRuntime.Resources.cs`
- `StylizedRiverFoamRuntime.Compute.cs`
- `StylizedRiverFoamRuntime.PublicSurface.cs`
- `CS_RiverFoam.compute` kernels:
  - `BuildFoamFilmSource`
  - `BuildFoamFilmSupport`
  - `AdvanceFoamVisualOccupancy`
  - `EvaluateFoamShape`
- `RiverWaterFoam.hlsl`
- `StylizedRiverEditor.DebugViews.cs`

Mandatory tests:

- odd structural dimensions;
- bank-edge film texels representing fewer than four valid cells;
- integrated area parity;
- visual occupancy transport alignment;
- no block artifacts reintroduced at half resolution;
- shape remains visually stable at every candidate metric size.

### 7.15 Shape evaluation, breakup, and noise

**Class: R/D/T**

Some shape/noise features use cell spacing as a physical minimum or scale. A more isotropic structural grid changes their frequency, feather, and apparent breakup.

Dependencies:

- feature size clamping by cell spacing;
- per-cell versus rendered-pixel breakup;
- support search distances;
- strand/chip/fragment scales;
- field aspect;
- temporal morph cadence;
- boundary clipping.

Primary files:

- `CS_RiverFoam.Noise.hlsl`
- `CS_RiverFoam.Evolution.hlsl`
- `CS_RiverFoam.Support.hlsl`
- `CS_RiverFoam.Topology.hlsl`
- `CS_RiverFoam.compute`
- `RiverWaterFoam.hlsl`

Mandatory tests:

- no structural-cell holes exposed;
- Layer E detail remains rendered-pixel detail;
- accepted chip/strand scales at camera distance;
- no anisotropic noise stretch;
- no change to lifecycle state from visual-only effects.

### 7.16 Production rendering

**Class: U/R/T**

The production renderer currently derives Foam field Y from `lateralMetres / surfaceHalfWidth`. That is incompatible with fixed metric Y.

Dependencies:

- field UV reconstruction;
- metric offset to UV conversion;
- longitudinal valid versus padded range;
- lateral origin/global-Y base;
- visual warp and stretch;
- persistent state sampling;
- topology and shape sampling;
- motion-lane/obstacle-routing sampling;
- film occupancy sampling;
- boundary clipping;
- debug render modes.

Primary files:

- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl`
- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoamVelocity.hlsl`
- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader`
- `StylizedRiverFoamRuntime.Binding.cs`

Mandatory tests:

- simulation/render sample parity;
- no half-cell offset;
- both banks and asymmetric widths;
- padded endpoint clipping;
- visual warp is expressed in metres correctly;
- static and moving cameras;
- all production and debug Foam modes;
- no change to unrelated river lighting, refraction, colour, or disturbance rendering.

### 7.17 Disturbance-field integration

**Class: T/R, not automatically U**

Foam consumes Static Pressure, Static Wake, Ripple, and related disturbance fields that retain their own dimensions and normalized coordinate mapping. They should not be converted merely because Foam changes.

Dependencies:

- disturbance texture dimensions;
- Foam-cell centre to disturbance UV conversion;
- external-field binding and neutral fallback;
- obstacle registry readiness and geometry version;
- static pressure support for sources;
- wake/lee support and negative aging;
- ripple/wave sampling;
- Disturbance quality remains separate despite sharing `StylizedRiverQuality`.

Integration files to test:

- `StylizedRiverDisturbanceRuntime.cs`
- `StylizedRiverDisturbanceRuntime.Binding.cs`
- `StylizedRiverDisturbanceRuntime.Resources.cs`
- `StylizedRiverDisturbanceRuntime.StaticPressure.cs`
- `StylizedRiverDisturbanceRuntime.StaticWake.cs`
- `StylizedRiverDisturbanceRuntime.Ripple.cs`
- `StylizedRiverDisturbanceRuntime.PublicSurface.cs`
- `StylizedRiverDisturbanceRuntime.GeneratedSources.cs`
- `StylizedRiverDisturbanceRuntime.SourcePathMath.cs`
- `StylizedRiverFoamRuntime.Injection.cs`
- `StylizedRiverFoamRuntime.Topology.cs`
- `StylizedRiverFoamRuntime.TopologyCache.cs`
- `CS_RiverFoam.Sampling.hlsl`
- `CS_RiverFoam.compute`

Mandatory tests:

- Foam and disturbance fields with different dimensions;
- same physical point sampled in both fields;
- Static Pressure front support;
- Static Wake lee support;
- Ripple and wave influence;
- neutral fallback when Disturbance is absent;
- no change to Disturbance field allocation or performance unless separately approved.

### 7.18 River-domain, geometry, and orientation inputs

**Class: T/R, not automatically U**

Foam depends on the river domain for centreline position, cumulative distance, tangent/normal, left/right widths, valid length, curvature, and flow orientation.

Integration files to test:

- `RiverDomainSnapshot.cs`
- `StylizedRiverGeometry.cs`
- `StylizedRiverCorridorGeometry.cs`
- `StylizedRiver.cs`
- `StylizedRiverDomainDebug.cs`

Mandatory tests:

- centreline distance monotonicity;
- asymmetric left/right widths;
- width variation;
- tight curves;
- reversed flow direction;
- changed spline/domain version;
- endpoints and padding;
- river renderer/corridor mesh matches the metric lattice physically;
- no corridor geometry modification is introduced by the Foam patch.

### 7.19 Shared quality enum and policy

**Class: U/D/T**

`StylizedRiverQuality` is shared by Foam, Disturbance, and corridor geometry. Reinterpreting the enum itself would derail unrelated systems.

Required policy:

- keep the shared enum and serialized values;
- introduce a Foam-specific mapping from quality to requested metric cell size;
- do not modify Disturbance or corridor geometry quality constants as part of this patch;
- record that the new Foam mapping is a fidelity upgrade, not a metric-equivalent rename of 64/96/128.

Primary files:

- `StylizedRiver.cs`
- `StylizedRiverFoamRuntime.Constants.cs`
- `StylizedRiverFoamRuntime.Resources.cs`
- `StylizedRiverEditor.Authoring.cs`

Mandatory integration tests:

- Low/Medium/High Foam dimensions;
- Disturbance dimensions unchanged;
- corridor mesh tessellation unchanged;
- existing serialized Medium scene loads without migration damage.

### 7.20 Cache asset, codec, fingerprints, and runtime cache state

**Class: U/D/T**

Existing cache artifacts encode products generated under the normalized-lateral contract.

Dependencies:

- asset storage contract;
- binary payload format;
- generator contract;
- generation fingerprint contract;
- field width/height/length;
- metric-grid mapping version;
- resolved `dx/dy`;
- lattice origin and global-Y base;
- topology arrays and obstacle exclusion;
- maximum dimension 8192;
- maximum cell count;
- runtime cache hit/miss reasons;
- stale settings, domain, obstacle, and generator detection.

Primary files:

- `FoamTopology/StylizedRiverFoamTopologyCacheAsset.cs`
- `FoamTopology/StylizedRiverFoamTopologyCacheCodec.cs`
- `FoamTopology/StylizedRiverFoamTopologyFingerprints.cs`
- `StylizedRiverFoamRuntime.TopologyCache.cs`

Mandatory decisions:

- payload-format bump only if serialized layout changes;
- generator and generation-fingerprint contract bumps are mandatory;
- exact failure policy for fields exceeding contiguous cache limits;
- whether one-strip descriptors are serialized now to ease Stage 2.

Mandatory tests:

- deterministic old-cache rejection;
- new cache encode/decode round trip;
- corruption and dimension limits;
- exact hit on unchanged inputs;
- stale reason for every descriptor change;
- no raw-editing of generated cache assets.

### 7.21 Cache preparation, development coordination, and build preflight

**Class: U/R/T**

The editor workflow requires exact prepared cache artifacts before builds. A mapping-contract change must propagate through all tooling and messages.

Primary files:

- `Editor/StylizedRiverEditor.Actions.cs`
- `Editor/StylizedRiverFoamBuildPreflight.cs`
- `Editor/StylizedRiverFoamDevelopmentCacheCoordinator.cs`
- `Editor/StylizedRiverEditor.Diagnostics.cs`
- `Editor/StylizedRiverEditor.Foam.cs`

Mandatory tests:

- explicit cache preparation;
- development auto/rebuild coordination, if enabled;
- stale cache diagnostics;
- build preflight pass with valid cache;
- build preflight failure with normalized-grid cache;
- no hidden scene reserialization;
- correct handling of obstacle registry readiness.

### 7.22 Diagnostics, metrics, and telemetry

**Class: U/D/T**

Several current diagnostics are resolution-dependent. Raw affected-texel counts, visible perimeter counts, and cell counts cannot be compared across grids without physical normalization.

Dependencies:

- field dimensions;
- spacing min/max;
- CFL and substeps;
- affected source texels;
- topology cell counts and ratios;
- perimeter cell count;
- integrated physical areas;
- transport conservation;
- cells and dispatches per second;
- memory estimates;
- cache state;
- debug summaries.

Primary files:

- `StylizedRiverFoamRuntime.BirthDiagnostics.cs`
- `StylizedRiverFoamRuntime.PublicSurface.cs`
- `StylizedRiverFoamRuntime.Compute.cs`
- `StylizedRiverEditor.Diagnostics.cs`
- `StylizedRiverEditor.DebugViews.cs`
- `CS_RiverFoam.compute` metrics kernels

Required diagnostic additions or corrections:

- requested and resolved `dx/dy`;
- lattice origin and local global-Y interval;
- allocated versus valid cells;
- dispatch-rounded thread envelope;
- invalid/out-of-bank occupancy percentage;
- physical affected source area;
- physical perimeter estimate or clearly resolution-specific perimeter label;
- maximum `abs(curvature * lateralOffset)`;
- contiguous cache-limit headroom;
- CFL components separately;
- Stage 1 whole-rectangle waste.

### 7.23 Memory and performance accounting

**Class: U/T**

The field owns multiple full-resolution, half-resolution, buffer, upload, readback, and transition resources. Cell count is not a complete cost model.

Mandatory measurements:

- allocated texture bytes;
- buffer bytes;
- CPU arrays/readbacks;
- kernel dispatch counts;
- launched threads;
- material-update cells per second;
- CFL and substeps;
- CPU submission time;
- GPU time;
- cache generation time;
- topology build time;
- source preparation time.

Mandatory comparison cases:

- current normalized Medium baseline;
- metric candidates 0.25, 0.20, 0.15, and 0.10 m;
- 5, 10, 20, and 40 m widths;
- straight and curved domains;
- active and idle/held states;
- visible and offscreen behavior as currently implemented.

No Stage 1 report may claim cost scales with active local water area. Until strips exist, cost still scales with the contiguous rectangle’s total length and maximum lateral extent.

### 7.24 Inspector and serialized authoring semantics

**Class: U/D/T**

The current editor describes quality in old structural terms and exposes several values in cells.

Primary files:

- `StylizedRiver.cs`
- `Editor/StylizedRiverEditor.Authoring.cs`
- `Editor/StylizedRiverEditor.Foam.cs`
- `Editor/StylizedRiverEditor.UI.cs`
- `Editor/StylizedRiverEditor.cs`

Required decisions:

- whether old serialized cell-based values are migrated, reinterpreted, or deprecated;
- whether metre-based replacements need compatibility fields;
- exact inspector labels and tooltips;
- whether resolved cell size is shown read-only by quality.

Mandatory tests:

- old scene loads without value reset;
- no prefab/scene reserialization caused only by opening inspector;
- units are explicit;
- values persist over domain reload and code recompilation;
- debug and runtime use identical values.

### 7.25 Debug views

**Class: U/T**

Every field-space debug visualization must be validated, including views that are not part of the production renderer.

Dependencies:

- structural cell grid;
- topology layers;
- obstacle footprint;
- routing and motion lane;
- automatic birth sources;
- current/live versus cumulative source display;
- visual occupancy field;
- persistent material state;
- shape output;
- cache diagnostics.

Primary files:

- `Editor/StylizedRiverEditor.DebugViews.cs`
- `Editor/StylizedRiverEditor.Diagnostics.cs`
- `StylizedRiverDomainDebug.cs`
- `SH_CleanStylizedRiver.shader`
- `RiverWaterFoam.hlsl`

Mandatory tests:

- overlays align with river geometry at both banks;
- cell grid is physically square/isotropic at selected target where intended;
- hidden rocks remain valid simulation obstacles;
- live Automatic Source view does not display stale cumulative cells;
- no upstream shell around objects;
- half-resolution occupancy overlay maps correctly.

### 7.26 Runtime scheduling, state ownership, and change handling

**Class: U/T**

Dependencies:

- material update cadence by quality;
- active/held/idle state;
- runtime initialization order;
- disturbance/obstacle readiness;
- cache startup validation;
- domain and geometry version observation;
- pending obstacle rebuild stabilization;
- renderer binding after resource replacement;
- topology replacement ownership;
- enable/disable and destruction cleanup.

Primary files:

- `StylizedRiverFoamRuntime.cs`
- `StylizedRiverFoamRuntime.Lifecycle.cs`
- `StylizedRiverFoamRuntime.RuntimeUpdates.cs`
- `StylizedRiverFoamRuntime.TopologyCache.cs`
- `StylizedRiverFoamRuntime.TopologyReplacement.cs`
- `StylizedRiverFoamSimulation.cs`

Mandatory tests:

- startup with valid cache;
- startup with stale cache;
- missing Disturbance runtime;
- obstacle registry arriving late;
- repeated enable/disable;
- quality and domain changes during Play Mode;
- no duplicate initialization or leaked render textures;
- renderer never samples released or mismatched fields.

### 7.27 Documentation and plan ownership

**Class: U**

Canonical documents that must be updated if implementation is approved:

- `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
- `Docs/River_Foam_Stage6_Architecture.md`
- `Docs/River_Rendering_Roadmap.md`
- `Docs/handoff.md`, if it is the active canonical handoff

Historical documents may retain old behavior as history but should not be silently rewritten unless the repository policy requires it.

The active blocker document’s stale H.4 versus H.6.2 queue must be reconciled before adding the metric-grid patch.

### 7.28 Scene and generated-asset policy

**Class: T only unless separately authorized**

Validation input:

- `Game/Demo/Scenes/VisualFrameworkDemo.unity`

Requirements:

- do not raw-edit or reserialize the scene as part of the code migration;
- do not modify prefabs without explicit authorization;
- regenerate Foam topology caches only through the approved editor workflow;
- document any manual inspector action the user must perform;
- compare against the scene’s current Medium baseline.

## 8. Compute-kernel dependency checklist

Every kernel in `CS_RiverFoam.compute` must be classified and tested. None may be assumed unaffected merely because its main purpose is not coordinate conversion.

| Kernel | Stage 1 status | Required focus |
|---|---:|---|
| `ClearRange` | U/T | Metric range/addressing and exact clearing |
| `InjectFoam` | U/T | Manual source placement and physical extent |
| `RasterizeFoamSourceEvent` | U/T | Metric cell centres, source widths, event bounds |
| `RasterizeFoamSourceEventDebug` | U/T | Exact parity with production source raster |
| `WriteIsolatedLifeProbe` | U/T | Probe cell placement |
| `ClearAutomaticBirthDebugAll` | T | Dimensions and complete clear |
| `BuildCurrentShoreEdges` | U/T | Bank edge and metric thickness |
| `ComposeTopology` | U/T | Topology layers and bank clipping |
| `CaptureGeneratedTopology` | U/T | Cache/readback dimensions and parity |
| `BuildEvolvingMajorSupport` | U/R/T | Local extents and physical support widths |
| `ClearObstacleExclusion` | T | Complete dimensions |
| `UpdateObstacleExclusion` | U/T | Metric obstacle intervals |
| `BuildFoamObjectContactField` | U/T | Cell-neighbour physical gradients and pressure alignment |
| `ResetTopologyMetrics` | T | Buffer reset |
| `MeasureTopologyMetrics` | U/D/T | Physical area/perimeter semantics |
| `ResetTransportMetrics` | T | Buffer reset |
| `SimulateFoam` | U/D/T | Conservative transport, CFL, area, curvature policy |
| `BuildFoamFilmSource` | U/T | Full-to-half mapping and area |
| `BuildFoamFilmSupport` | U/T | Half-resolution support alignment |
| `AdvanceFoamVisualOccupancy` | U/T | Metric advection and represented area |
| `EvaluateFoamShape` | U/R/T | Structural/film alignment and noise scale |
| `ApplyBoundary` | U/T | Valid-bank clipping and padding |

## 9. Complete file-level register for the supplied snapshot

The following is the file-level audit boundary. “Review/Test” does not mean the file will necessarily be modified; it means it may not be excluded from validation.

### 9.1 Mandatory update or conditional update — River Foam runtime

- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Binding.cs`
- **R/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthDiagnostics.cs`
- **U/D/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs`
- **R/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthTransfer.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Compute.cs`
- **U/D/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs`
- **U/R/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Evolution.Connector.cs`
- **U/R/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Evolution.FreeWater.cs`
- **R/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Evolution.HostedNegative.Pose.cs`
- **U/R/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Evolution.HostedNegative.cs`
- **U/R/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Evolution.Major.cs`
- **U/R/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Evolution.Shared.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs`
- **U/D/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Obstacles.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.RuntimeUpdates.cs`
- **U/D/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.State.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Topology.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.TopologyCache.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.TopologyReplacement.cs`
- **R/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.cs`
- **R/T** `Game/Procedural/Rivers/StylizedRiverFoamSimulation.cs`

### 9.2 Mandatory update or conditional update — topology and obstacle conversion

- **R/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamConnectorTopology.cs`
- **U/D/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamConnectorTopologyGenerator.cs`
- **R/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamMajorCandidate.cs`
- **R/D/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamMajorCandidateGenerator.cs`
- **R/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamMajorTopology.cs`
- **U/D/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamMajorTopologyGenerator.cs`
- **R/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamPocketTopology.cs`
- **U/D/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamPocketTopologyGenerator.cs`
- **U/R/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyCacheAsset.cs`
- **U/D/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyCacheCodec.cs`
- **U/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyFieldSpace.cs`
- **U/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyFingerprints.cs`
- **U/T** `Game/Procedural/Rivers/RiverObstacleExclusionResolver.cs`

### 9.3 Mandatory update or conditional update — compute and rendering

- **U/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Coordinates.hlsl`
- **U/R/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Evolution.hlsl`
- **U/R/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Motion.hlsl`
- **R/D/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Noise.hlsl`
- **U/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl`
- **U/R/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Sampling.hlsl`
- **U/D/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Simulation.hlsl`
- **U/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Structs.hlsl`
- **U/R/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Support.hlsl`
- **U/R/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Topology.hlsl`
- **U/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.TopologyTransition.hlsl`
- **R/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Transport.hlsl`
- **U/D/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`
- **U/T** `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl`
- **R/T** `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoamVelocity.hlsl`
- **U/R/T** `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader`

### 9.4 Mandatory update or conditional update — authoring, diagnostics, and cache tooling

- **U/R/T** `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`
- **U/T** `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Authoring.cs`
- **U/T** `Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs`
- **U/T** `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Diagnostics.cs`
- **T** `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Disturbances.cs`
- **U/D/T** `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
- **R/T** `Game/Procedural/Rivers/Editor/StylizedRiverEditor.UI.cs`
- **R/T** `Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs`
- **U/R/T** `Game/Procedural/Rivers/Editor/StylizedRiverFoamBuildPreflight.cs`
- **U/R/T** `Game/Procedural/Rivers/Editor/StylizedRiverFoamDevelopmentCacheCoordinator.cs`
- **U/D/T** `Game/Procedural/Rivers/StylizedRiver.cs`

### 9.5 Mandatory integration test — upstream river/domain/obstacle inputs

These files should not be modified automatically. They are required integration-test dependencies because Foam consumes their output.

- **T/R** `Game/Procedural/Rivers/RiverDomainSnapshot.cs`
- **T/R** `Game/Procedural/Rivers/StylizedRiverGeometry.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverCorridorGeometry.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDomainDebug.cs`
- **T/R** `Game/Procedural/Rivers/RiverDisturbanceFootprintResolver.cs`
- **T/R** `Game/Procedural/Rivers/StylizedRiverDisturbanceEmitter.cs`

### 9.6 Mandatory integration test — Disturbance subsystem

- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Binding.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Compute.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Constants.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.ContinuousSources.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Contracts.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Diagnostics.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Dispatch.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.GeneratedSources.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Impact.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Members.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.PublicSurface.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Resources.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Ripple.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.SourcePathMath.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.State.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.StaticPressure.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.StaticWake.cs`

### 9.7 Mandatory validation input — scene/assets

- **T, no raw edit** `Game/Demo/Scenes/VisualFrameworkDemo.unity`
- **T, regenerate through tooling only** all River Foam topology cache assets referenced by validation scenes
- **T** all materials using `SH_CleanStylizedRiver.shader`
- **T** all river instances with Foam enabled, not only the demo instance

### 9.8 Canonical documentation

- **U** `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
- **U** `Docs/River_Foam_Stage6_Architecture.md`
- **U** `Docs/River_Rendering_Roadmap.md`
- **R/U** `Docs/handoff.md`

## 10. Future strip/pooling-only dependency register

These dependencies are not required to prove the one-strip metric coordinate contract, but they are mandatory before claiming scalable local-width allocation or active-area cost scaling.

### 10.1 Strip descriptor and ownership — F/D

- strip start/end in centreline metres;
- columns and local global-Y interval;
- shared lattice phase and `dy`;
- overlap/ghost border ownership;
- endpoint and inter-strip boundary roles;
- domain version and generation fingerprint per strip.

### 10.2 Resource representation — F/D

- independent textures;
- texture arrays;
- atlas or packed pages;
- allocation buckets;
- pooling and reuse;
- fragmentation and memory caps;
- per-strip neutral resources;
- transition resources.

### 10.3 Cross-strip transport — F/D/T

- ghost-cell copies by matching global Y;
- cells present only in one strip’s wider interval;
- conservative flux ownership;
- flow reversal;
- simultaneous versus ordered dispatch;
- state transfer on strip activation/deactivation;
- topology and obstacle continuity.

### 10.4 Renderer lookup and indirection — F/D/T

- world/river point to strip index;
- strip-local UV;
- boundary blending;
- film field lookup;
- debug lookup;
- no seams or double samples.

### 10.5 Scheduling and budgets — F/D/T

- active strip detection;
- visible/offscreen/frozen states;
- update cadence by distance/activity;
- global active-cell cap;
- global memory cap;
- dispatch batching;
- many-river fairness;
- pre-generation and cache loading policy.

### 10.6 Strip cache format — F/D/T

- descriptor table;
- per-strip payloads;
- partial loading;
- compatibility and versioning;
- maximum strip count;
- exact cache hit/miss reporting;
- cache regeneration and build preflight.

### 10.7 Connected river components — F/D/T

Not needed for Stage 1, but required before state crosses independently authored components:

- endpoint connection identity;
- compatible downstream `dx`;
- compatible lateral lattice phase and `dy`;
- centreline endpoint alignment;
- tangent orientation;
- left/right handedness;
- reversed component direction;
- width mismatch;
- junction and branching ownership;
- conservative material transfer.

## 11. Mandatory visual-regression matrix

Before implementation, capture a baseline. After implementation, repeat the same matrix.

### 11.1 Source views

- Automatic Birth Source — live, not cumulative
- Arc only
- Semi-Arc only
- Fleck only
- Shore Ribbon only
- Inward Wash only
- each Free Water pattern independently
- all patterns at production weights

### 11.2 Topology and support views

- final composed topology
- major support
- connector support
- pocket support/negative aging
- shore support
- obstacle exclusion
- object contact field
- motion lane
- obstacle routing

### 11.3 State and rendering views

- persistent Presence
- Remaining Life
- Material Pattern
- visual occupancy/film
- evaluated shape
- production Foam render
- velocity/motion debug
- cache state and topology diagnostics

### 11.4 Geometry cases

- straight constant-width river
- widening river
- narrowing river
- asymmetric bank widths
- left bend
- right bend
- S-bend
- approximately 5 m width
- 10 m width
- 20 m width
- 40 m width
- very short river
- multi-chunk river
- field near contiguous cache limit
- forward and reversed flow

### 11.5 Obstacle cases

- small rock
- long rock
- rotated rock
- bank-adjacent rock
- multiple nearby rocks
- rock renderer hidden but simulation active
- thin obstacle interval
- obstacle crossing a future strip boundary

## 12. Mandatory numerical and performance validation

| Category | Required evidence |
|---|---|
| CPU/GPU coordinate parity | Sampled cell-centre positions and round trips |
| Source placement | Physical source bounds versus affected cell bounds |
| Conservation | Presence/life/pattern mass before/after transport |
| CFL | Downstream term, lateral term, total, and substeps |
| Curvature | Maximum `abs(κn)` and selected policy outcome |
| Topology | Physical areas, lengths, continuity, invalid-cell count |
| Memory | Actual resource bytes and CPU-side arrays |
| Compute | Dispatch count and launched thread envelope |
| Timing | CPU submission and GPU duration |
| Cache | Generation time, package size, hit/miss reason |
| Rendering | Frame time and exact sample alignment |
| Scalability | 5/10/20/40 m and increasing length comparisons |

## 13. Explicit non-dependencies and scope protections

The following must not be changed merely to make the Foam migration easier:

- Ground shaders or Ground generation;
- river corridor material-response behavior;
- Disturbance field resolution or simulation architecture;
- scene or prefab serialized data;
- tags, layers, components, folders, or asset names;
- accepted Arc/Semi-Arc source path logic except where a separately documented metric-unit conversion is required;
- inter-river state transfer in Stage 1;
- world-aligned XZ simulation.

Any newly discovered need to modify one of these areas is a scope-expansion event requiring plan amendment and approval.

## 14. Definition of dependency-complete implementation

The fixed-metric migration is not dependency-complete until:

1. every **U** item is implemented or explicitly removed from scope with evidence;
2. every **D** item has a recorded decision and acceptance criterion;
3. every **T** item has a recorded result;
4. all old normalized-lateral cache artifacts are rejected deterministically;
5. CPU, compute, renderer, debug, topology, obstacle, and source mappings agree;
6. all source families preserve approved physical behavior;
7. topology and routing morphology are no longer accidentally tied to old cell dimensions;
8. Disturbance and corridor systems are verified unchanged;
9. no scene or prefab was modified;
10. the canonical architecture and active-plan documents match the implemented state;
11. a final live-workspace reference scan finds no remaining unauthorized normalized-lateral Foam mapping.

## 15. Final conclusion

The fixed-metric change is a River Foam coordinate-system migration, not a resolution toggle. Its dependencies include every producer, transformer, cache, diagnostic, and consumer of Foam field coordinates, plus integration tests for the river-domain and Disturbance systems that provide external data.

The prior audit identified the most dangerous dependencies but did not enumerate all of them. This register is the required standalone checklist for planning and validating the update against the supplied source snapshot.
