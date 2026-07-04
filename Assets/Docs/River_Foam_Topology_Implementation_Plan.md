# River Foam Topology Implementation Plan

## Document Status

**Status:** Canonical step-by-step implementation plan for Stage 6 Foam topology only.

**Patch status:** Topology work through Patch 4.10B is complete and Unity-validated. Major, Connector, all four Negative Aging Pressure classes, evolution, replacement transitions, cache/precompute packaging, exact Obstacle Footprint, semantic sampling, and neutral-safe bindings remain accepted. Material work after 4.10B does not reopen topology generation. Patches 4.11C.1 and 4.11C.2 prove the material-owned progressive source and transfer boundary; the remaining material-state correction is split into 4.11C.3–4.11C.7 and is owned by `River_Foam_Material_State_Correction_Implementation_Plan.md`. No correction patch changes topology generators, cache payloads, topology identities, evolution rules, or material-facing topology channels.

**Primary implementation target:**

- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.*.cs`

**Primary compute target:**

- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`

**Likely topology source boundary:**

- `Game/Procedural/Rivers/FoamTopology/`

**Related documents:**

- `Docs/River_Foam_Stage6_Architecture.md` — owns the full Stage 6 behavioural and visual contract.
- `Docs/River_Progressive_Initialization_and_Work_Scheduling_Plan.md` — owns initialization, rebuild scheduling, profiling, and performance safeguards.
- `Docs/River_Foam_Material_State_Correction_Implementation_Plan.md` — owns the detailed 4.11C.3–4.11C.7 material correction.
- `Docs/River_Rendering_Roadmap.md` — owns the concise river-wide milestone summary.

This document owns the implementation order, patch boundaries, inspection requirements, tests, acceptance gates, and rollback expectations for **Foam topology only**.

It does not own material supply, persistent Foam material state, topology-to-material aging response, fragmentation, dissipation, or final Foam rendering. Those systems may consume the accepted topology outputs later, but they are outside this plan.

No topology implementation step may begin until the previous step has produced an immediately inspectable result and passed its acceptance gate.

---

## 1. Executive Decision

Foam topology will be implemented as a sequence of small vertical slices.

Each slice must end with:

1. a real usable topology result;
2. one clear visual inspection surface appropriate to that result;
3. a small amount of telemetry sufficient to explain obvious failures;
4. a bounded test matrix;
5. a direct rollback path.

The implementation must not build a large invisible foundation before showing results. It must also not build a general-purpose topology debugger whose complexity rivals the topology system itself.

The inspection rule is contextual:

- an individual Major candidate may be shown in a compact separate preview because its local shape generation does not depend on river placement;
- any result involving distribution, banks, width, flow, obstacles, anchored support, connectors, pockets, composition, or movement must be shown on the actual river;
- accepted existing river diagnostics are reused wherever possible;
- temporary overlays may be added only when they answer one specific implementation question and should be removed or compiled out after that question is resolved.

The intended production architecture remains:

1. expensive topology generation during the procedural chunk generation/building/linking phase, a loading/preparation window, an editor tool where applicable, or another equivalent controlled pre-gameplay phase;
2. cached topology fields and compact identity/evolution metadata;
3. cheap runtime sampling plus asynchronous single-instance movement, shape morphing, and instant bounded recycling;
4. no expensive candidate generation, connected-component cleanup, pathfinding, distance transforms, or rejection loops during ordinary gameplay.

During development, expensive generation may temporarily run through the accepted staged initialization path so the visual algorithm can be proven before cache packaging is finalized.

---

## 2. Scope

This plan covers:

- topology source-context preparation;
- field-first Major candidate generation;
- deterministic Major opportunity identity;
- whole-river Major placement and composition;
- Connector Support generation;
- four-class Negative Aging Pressure generation;
- static topology composition;
- topology identity and evolution metadata;
- net-downstream runtime topology movement with bounded lateral/diagonal displacement;
- cheap asynchronous per-class movement and shape morphing without ordinary duplicate-instance crossfades;
- safe topology rebuild crossfades;
- topology cache/precompute packaging;
- topology diagnostics and topology-specific telemetry;
- cleanup of superseded topology implementations.

This plan does not cover:

- Foam material spawning or replenishment;
- emitter Amount, persistent Presence, Remaining Life, Material Pattern, or other material-state equations;
- topology-to-material lifespan response;
- fragmentation or dissipation of Foam material;
- final Foam shading or colour;
- Impact Ripple-to-Foam integration;
- reopening accepted Stage 5 Pressure, Wake, or Impact Ripple behaviour;
- unrelated river optimization work beyond preserving the accepted scheduling and profiling contract.

---

## 3. Canonical Topology Outputs

The topology pipeline produces three independent logical output groups:

1. **Major Support**
   - broad positive lifespan-support regions;
   - connected and filled;
   - varied at low and medium spatial frequencies;
   - distributed using actual river context.

2. **Connector Support**
   - sparse relational positive support;
   - generated between meaningful positive regions or approved anchored endpoints;
   - subordinate to Major and Anchored Support.

3. **Negative Aging Pressure**
   - aggregate negative lifespan influence;
   - remains logically separate from positive support;
   - consists of four independently generated and independently paced source classes:
     - **Interior Pocket** — closed negative area hosted firmly inside broad Major Support while preserving a positive rim;
     - **Edge Cavity** — lopsided Major-hosted negative area that may breach one deliberate side and create a bite, bay, crescent, or open cavity;
     - **Connector Weak Span** — short negative section hosted by one accepted Connector relationship, away from its endpoint gates;
     - **Free-Water Negative Event** — sparse valid-water negative area that does not require a positive-support host and is intended to affect stray material later.

The four negative classes retain separate class identity, stable opportunity identity, and evolution metadata. They may be packed into one aggregate negative field for the current diagnostic/output contract, but implementation must not flatten away the metadata required for class-specific runtime evolution.

The pipeline also preserves live non-generated context:

- Pressure Support;
- Lee Support;
- Shore Support;
- exact water-level-aware Obstacle Footprint;
- valid-water coverage.

Anchored Support remains attached to its authoritative live source. It is not baked into the free-water drift field in a way that causes it to detach from banks or objects.

### Negative amount-control contract

Each negative class has one independent normal authoring control:

- `Interior Pocket Amount`;
- `Edge Cavity Amount`;
- `Connector Weak Span Amount`;
- `Free-Water Event Amount`.

Every control uses range `0–1` and default `0.5`:

- `0` activates none of that class;
- `0.5` reproduces a sensible category-specific baseline;
- `1` activates the maximum bounded population for that class.

Increasing one Amount activates a nested deterministic subset of stable opportunities. It must not reshuffle already-active opportunities. Amount controls population only; it does not silently alter shape size, negative strength, seed, or evolution speed. A value of `0.5` does not imply equal counts or equal coverage across the four classes: each class has its own bounded opportunity pool and visual density budget.

## 4. Current Baseline and Replacement Boundary

### 4.1 Retained infrastructure

The following existing work remains useful unless implementation proves otherwise:

- `RiverDomainSnapshot` and the accepted river-space mapping contract;
- structural resolution tiers and metric-row preparation;
- valid-water and boundary information;
- water-level-aware Obstacle Footprint generation;
- Pressure, Lee, and Shore source generation;
- topology output textures and accepted channel compatibility;
- existing topology diagnostic rendering;
- staged per-river initialization;
- queued and coalesced rebuild scheduling;
- resource lifecycle, sleeping, freezing, and chunk ownership;
- existing profiler infrastructure;
- deterministic Major `Amount`, `Size`, `Size Variation`, and `Seed` authoring contracts, plus the accepted Connector `Amount`, `Directness`, and `Length Preference` contracts; the four approved negative Amount controls are introduced only in their respective implementation subpatches.

### 4.2 Known obsolete topology implementation

The following are confirmed non-canonical and must be removed as part of the replacement work:

- the current three-lobe/one-bite Major grammar;
- the associated C# and HLSL nucleus descriptors;
- Major reconstruction kernels and helpers that exist only for that grammar;
- the current provisional Pocket derivation and its dedicated helpers;
- the disabled Connector implementation and its dead helper path;
- the unused hand-authored topology fixture and its `.meta` file.

### 4.3 Complete stale-code removal rule

The list above is the known minimum, not the complete removal boundary.

Every topology patch must perform a reference-driven audit and remove any code made obsolete by the accepted replacement, including where applicable:

- unused structs and enums;
- dead compute kernels and `#pragma kernel` declarations;
- buffers, textures, ping-pong resources, capacities, IDs, and bindings used only by removed logic;
- initialization and rebuild states used only by removed passes;
- profiler markers that no longer correspond to real work;
- metrics that describe removed behaviour;
- Inspector text, tooltips, help boxes, and debug descriptions that describe the old implementation;
- stale comments and documentation;
- serialized implementation-specific fields that no longer have canonical meaning;
- fixture, fallback, migration, or compatibility code with no verified active consumer;
- duplicate paths retained only “just in case.”

Removal is based on actual references and behaviour, not only on known names.

The cleanup must not remove shared accepted infrastructure merely because it was previously used by the obsolete topology path. Output textures, diagnostics, source fields, scheduling, and authoring controls are retained when they still serve the new pipeline.

No permanent old/new dual implementation is allowed. A temporary compatibility bridge may exist only when a real serialized-data or live-resource dependency requires it, and the exact reason and removal condition must be documented in the patch.

---

## 5. Inspectability Contract

### 5.1 General rule

Any patch that creates or transforms topology data must expose the resulting data in the same patch.

A patch may not be accepted on the basis that a later patch will make its output visible.

### 5.2 Individual Major candidate preview

An individual Major candidate may use one compact preview inside the existing river Inspector or another existing editor surface.

The preview should expose only the stages needed to judge the candidate generator:

- `Raw Field`;
- `Thresholded`;
- `Cleaned`;
- `Final Support`.

The preview must not require:

- a separate scene;
- a new GameObject;
- a new runtime component;
- a generic node debugger;
- a permanent texture-inspection framework.

The essential candidate telemetry is:

- accepted or rejected;
- primary rejection reason;
- occupied area;
- minimum neck width;
- one compact shape-quality measure such as compactness or oval similarity.

Additional metrics may be logged during development, but they should not become permanent Inspector clutter unless repeated failures prove them necessary.

### 5.3 River-dependent inspection

Anything affected by river context must be displayed on the actual river.

The permanent topology views should remain small in number and reuse the accepted diagnostic path:

- Major Support;
- Connector Support;
- aggregate Negative Aging Pressure;
- combined topology;
- accepted Anchored Support and Obstacle Footprint views.

A temporary overlay may show items such as Connector endpoints, one attempted path, or selected negative-region centres while that step is being implemented. Such overlays are implementation aids, not promised permanent authoring tools.

### 5.4 Minimal telemetry

Telemetry exists to answer obvious failure questions, not to create a statistics dashboard.

For Major composition, retain only:

- opportunities attempted;
- accepted regions;
- rejected regions;
- the most common rejection reasons;
- accepted Major coverage;
- generation time.

For Connectors, retain only:

- eligible endpoints;
- pair/path attempts;
- accepted connectors;
- most common rejection reason;
- generation time.

For Pockets, retain only:

- eligible Major hosts;
- candidate centres;
- accepted negative regions by active class;
- most common rejection reason if useful;
- generation time.

The normal `Final Foam` view must not perform diagnostic readbacks or diagnostic-grade work merely because telemetry exists.

---

## 6. Determinism and Identity Contract

Every generated free-water topology region requires stable identity from the start, even before movement is implemented.

A compact region record should preserve enough information for later cache and runtime evolution, including as applicable:

- layer class: Major, Connector, Interior Pocket, Edge Cavity, Connector Weak Span, or Free-Water Negative Event;
- stable region or opportunity identity;
- base field or mask identity;
- river-space bounds;
- generation seed or deterministic sub-seed;
- downstream drift speed;
- phase offset;
- dwell duration, movement duration, hop/lifetime, shape-morph, and recycle selectors;
- allowed movement span or recycle interval;
- anchoring strength to Pressure, Lee, Shore, obstacle, or bank context;
- optional per-layer evolution rhythm;
- host identity for Pocket where required;
- endpoint or relationship identity for Connector where required.

These records are compact value data. They must not become GameObjects, scene components, a continuously maintained gameplay graph, or a source of per-frame managed allocation.

For unchanged river geometry, obstacle state, settings, generator version, and seed:

- candidate generation must be deterministic;
- accepted opportunity identity must be deterministic;
- static topology must be deterministic;
- runtime evolution phases must be reproducible unless an explicitly approved run-time random source is later introduced.

`Major Support Amount` preserves nested activation: increasing Amount adds later-ranked opportunities without changing already-active identity wherever the accepted composition constraints permit.

`Major Support Size` changes physical scale without silently reassigning seed identity or population order.

---

## 7. Performance and Scheduling Contract

The accepted scheduling foundation remains mandatory:

- topology generation advances through staged initialization or another explicit preparation window;
- dirty changes queue and coalesce rather than rebuilding synchronously;
- expensive work has named profiler markers;
- cold compute first use is tested separately from warm runs;
- no unbounded retry, search, or catch-up loop is allowed;
- no hidden generation occurs in property getters, material binding, or normal rendering;
- no ordinary gameplay frame performs candidate search, connected-component cleanup, pathfinding, distance transforms, contour analysis, or rejection loops;
- inactive, frozen, sleeping, and distant rivers do no unnecessary topology evolution work.

The proof path may temporarily perform CPU generation, GPU readback, pathfinding, distance transforms, and validation during initialization. Every such operation must be identified as proof/precompute work and must have a production removal or cache destination.

No performance patch may be mixed into a topology-appearance patch unless the appearance result cannot be produced safely without it and the combined risk is explicitly approved.

---

## 8. Patch Safety Rules

Before each implementation patch:

1. inspect the current supplied baseline;
2. list every file to be changed or deleted;
3. state why each file is required;
4. state the exact visible behaviour change;
5. identify the inspection view and telemetry delivered by the patch;
6. identify all code paths intended for removal;
7. define the acceptance test and rollback file set;
8. confirm that no unrelated Stage 5 or full-Foam behaviour is being changed.

During implementation:

- preserve unrelated code;
- do not invent new layers, tags, components, GameObjects, or scene setup;
- do not add broad public controls before the behaviour is proven;
- do not silently reinterpret existing controls;
- keep retries and searches bounded;
- keep diagnostics proportional to the question being answered;
- remove obsolete code in the same patch that makes it obsolete whenever safe.

After implementation:

- compile all changed C# and HLSL paths;
- search for orphan references to removed symbols;
- test cold and warm Play entry if compute code changed;
- run the step-specific visual test;
- record the small required telemetry;
- stop if the step fails rather than hiding it with the next layer.

---

## 9. Implementation Roadmap

## Step 0 — Documentation Baseline

### Purpose

Create one canonical topology-only implementation plan and align the broader documents with the incremental, inspectable rollout.

### Files changed

- `Docs/River_Foam_Topology_Implementation_Plan.md` — new canonical topology implementation plan.
- `Docs/River_Foam_Topology_Implementation_Plan.md.meta` — Unity asset metadata.
- `Docs/River_Foam_Stage6_Architecture.md` — reference the plan and align topology sequencing, inspectability, stale cleanup, and runtime evolution wording.
- `Docs/River_Progressive_Initialization_and_Work_Scheduling_Plan.md` — reference the plan and replace the former integrated-first next step with incremental topology slices.
- `Docs/River_Rendering_Roadmap.md` — concise milestone alignment.

### Exact behaviour change

None. Documentation only.

### Acceptance gate

- topology-only scope is explicit;
- every topology step has an immediate inspection surface;
- candidate-local and river-dependent inspection are separated correctly;
- stale-code removal is exhaustive rather than limited to the named examples;
- runtime evolution metadata and rules are recorded before implementation;
- the broader documents no longer imply that Major, Connector, and Negative Aging Pressure must be built invisibly as one large patch.

### Rollback

Restore the previous documentation files and remove the new plan and `.meta` file.

---

## Step 1 — Major Candidate Vertical Slice and Obsolete-Path Removal

### Purpose

Prove or reject the field-first Major shape generator before whole-river placement, while removing the superseded topology implementation rather than carrying two systems forward.

### Expected files

The exact set must be re-verified against the supplied baseline before editing. The likely set is:

- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.cs`;
- `Game/Procedural/Rivers/StylizedRiver.cs`;
- `Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs`;
- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`;
- new topology-only C# data/generator files under `Game/Procedural/Rivers/FoamTopology/`;
- delete `Game/Procedural/Rivers/StylizedRiverFoamTopologyFixture.cs` and its `.meta` file;
- remove any additional files or symbols found by the stale-code audit.

### Exact behaviour change

Implement one deterministic local Major candidate pipeline:

1. allocate or reuse one temporary local field, initially around `96 × 96`;
2. reserve a mandatory empty outer margin;
3. generate a broad correlated scalar field;
4. add a weaker medium-frequency correlated field;
5. sample through a low-frequency coordinate warp;
6. select occupancy by target percentile;
7. keep the largest connected component;
8. remove smaller islands;
9. fill enclosed holes;
10. close only tiny accidental gaps;
11. remove short spikes and isolated serrations;
12. measure minimum neck width;
13. reject invalid candidates through a bounded deterministic retry count;
14. convert the accepted body into a soft final support mask.

At the same time, remove all obsolete topology code made unnecessary by this vertical slice, including the known lobe/nucleus path, Pocket stub, Connector stub, fixture, and any additional dead dependencies found during reference tracing.

The accepted Anchored Support, Obstacle Footprint, topology output contract, and staged scheduling foundation remain intact.

### Immediate inspection

Add one compact candidate preview to the existing editor surface with:

- Raw Field;
- Thresholded;
- Cleaned;
- Final Support.

Show only:

- accepted/rejected;
- primary rejection reason;
- occupied area;
- minimum neck width;
- compactness or oval similarity.

### Explicit exclusions

- no whole-river placement;
- no Connector generation;
- no Pocket generation;
- no material aging response;
- no runtime movement;
- no cache asset;
- no broad Inspector control panel;
- no separate debug scene or component.

### Test

Test a deterministic seed set that includes at least:

- several ordinary seeds;
- several rejected seeds;
- low, medium, and high target occupancy;
- moderate and strong warp;
- repeat generation with the same seed;
- quality of the final soft edge at the intended stored resolution.

### Acceptance gate

Pass only if:

- the preview exposes every required stage immediately;
- identical inputs reproduce the same candidate and rejection result;
- accepted shapes are one connected filled body;
- accepted shapes do not touch the storage boundary;
- accepted shapes contain no unintended holes or detached islands;
- the ordinary seed population is not dominated by ellipses, capsules, starbursts, cellular damage, or skeletal ribbons;
- the generator can reject bad candidates without falling back to bland primitive shapes;
- retries are bounded;
- obsolete topology symbols and dead resources are removed cleanly;
- the project compiles with no orphan kernel or C# references.

### Rollback

Restore the previous runtime, editor, river, and compute files; restore the fixture files; remove the new topology source files.

---

## Step 2 — Whole-River Major Distribution

### Purpose

Place accepted Major candidates into the actual generated river and prove the distribution, identity, scale hierarchy, and context response before adding relational or negative topology.

### Exact behaviour change

Implement:

- stable longitudinal/lateral opportunity identity;
- deterministic activation rank;
- deterministic candidate assignment;
- seeded physical scale and orientation;
- actual river-space placement;
- valid-water and obstacle rejection;
- bank and width-capacity handling;
- bounded composition scoring;
- accumulation into the existing Major Support output;
- compact identity/evolution metadata for every accepted region.

Composition scoring should consider only the terms required to prevent obvious failure:

- open-water preservation;
- local crowding;
- longitudinal spacing;
- lateral distribution;
- size hierarchy;
- bank and obstacle clipping;
- repeated silhouette or orientation penalties;
- meaningful attraction to approved river context.

Do not implement a highly general optimizer. Start with a deterministic bounded greedy pass that preserves nested Amount identity wherever possible.

### Immediate inspection

Show the actual accumulated Major Support on the real river through the existing topology diagnostic path.

The permanent telemetry is limited to:

- attempted opportunities;
- accepted regions;
- rejected regions;
- top rejection reasons;
- Major coverage;
- generation time.

A temporary accepted/rejected opportunity overlay may be used only if the actual field and counts cannot explain a placement failure.

### Explicit exclusions

- no Connector Support;
- no Negative Aging Pressure;
- no topology-to-material response;
- no runtime drift;
- no production cache;
- no replacement of accepted Pressure, Lee, Shore, or Obstacle logic.

### Test

Test:

- short and longer rivers;
- straight and curved rivers;
- narrow, wide, asymmetric, and constricted sections;
- no obstacles, one obstacle, and several obstacles;
- low, medium, and high Amount;
- low, medium, and high Size;
- multiple seeds;
- reverse flow;
- repeated generation with identical inputs;
- increasing Amount from low to high and comparing earlier identities.

### Acceptance gate

Pass only if:

- Major regions read as broad intentional support islands in river context;
- substantial open water remains;
- short rivers contain a visible small/medium/occasional-large hierarchy;
- regions occupy lateral and diagonal orientations as well as downstream ones;
- banks and obstacles constrain placement without crushing the population into the centre lane;
- identical inputs reproduce identical static Major topology;
- Amount changes are nested and predictable;
- Size changes physical scale predictably;
- rejected-placement telemetry explains low or missing coverage;
- no obvious repeated primitive family dominates the river.

### Rollback

Remove the whole-river placement and metadata integration while retaining the accepted Step 1 candidate generator and preview.

---

## Step 3 — Connector Support

### Purpose

Generate sparse relational support between disconnected accepted Major components. Anchored-support endpoints remain a later optional extension and must not be approximated on the CPU merely to increase connection count.

### Exact behaviour change

Implement:

- endpoint extraction from accepted Major regions;
- endpoint clustering or deduplication;
- bounded candidate-pair selection;
- a metric-aware traversal cost field;
- bounded A* or Dijkstra-style path search, or another explicitly approved bounded equivalent;
- obstacle and invalid-water blocking;
- penalties for unsupported open water and implausibly long paths;
- blocked Major clearance halos outside compact source/destination endpoint gates;
- rejection of routes that orbit a Major perimeter or exceed a bounded detour ratio;
- removal of path segments already covered by broad positive support;
- narrow soft Connector rasterization;
- stable connector relationship identity and evolution metadata.

Shore Support must not become a universal cheap endpoint that causes a green fringe network around the full river.

### Immediate inspection

Show Connector Support on the actual river using the existing support-class diagnostic.

During implementation, one temporary overlay may show:

- eligible endpoints;
- one selected pair;
- one attempted or accepted path.

Permanent telemetry is limited to:

- eligible endpoints;
- path attempts;
- accepted connectors;
- top rejection reason;
- generation time.

### Explicit exclusions

- no persistent gameplay graph;
- no per-frame pathfinding;
- no Connector drawing through broad Major interiors;
- no perimeter-following route that wraps around most of a source or destination Major region to reach an unlikely endpoint;
- no Pocket generation;
- no material response;
- no runtime connector fading yet.

### Test

Test:

- two nearby Major regions with a plausible gap;
- two regions separated by an obstacle;
- regions too far apart;
- dense Major composition;
- sparse Major composition;
- bends, width changes, and reverse flow;
- several seeds and repeated identical generation.

### Acceptance gate

Pass only if:

- connectors begin and end at meaningful positive support;
- connectors remain sparse and subordinate;
- accepted paths avoid obstacles and invalid water;
- broad Major interiors are not repainted as Connector;
- endpoint gates and Major clearance halos prevent perimeter-orbiting routes;
- routed length remains a bounded detour over the selected endpoint gap;
- missing connectors are explainable by endpoint or path telemetry;
- searches are bounded and occur only in preparation/generation;
- connector identity is stable for unchanged inputs.

### Rollback

Remove Connector generation, metadata, and temporary overlays while retaining accepted Major topology.

---

## Step 4 — Four-Class Negative Aging Pressure

### Purpose

Complete the negative-topology vocabulary before any runtime evolution or Foam-material work begins.

Patch 4 and Patch 4.1 established the accepted starting baseline:

- deterministic Interior Pockets hosted by broad Major interiors;
- positive-rim, Connector-core, Anchored-core, valid-water, and obstacle protection;
- exact transformed-mesh Obstacle Footprint prepared and cached during staged pre-gameplay preparation;
- no material response and no runtime evolution.

The remaining work is intentionally split into small, inspectable subpatches.

### Patch 4.2 — Major-hosted negative topology

**Implementation status:** Accepted for feature progression. Population coefficients remain deliberately provisional and may be tuned after all topology classes are working.

Implemented:

- `Interior Pocket Amount`, range `0–1`, default `0.5`, with the existing accepted Interior Pocket population preserved approximately at the default;
- `Edge Cavity Amount`, range `0–1`, default `0.5`;
- stable nested activation for both classes;
- Edge Cavity candidates derived from valid broad Major hosts but biased toward one selected side;
- one deliberate permitted breach direction rather than removal of the rim in every direction;
- rejection when the surviving positive host becomes a useless sliver, loses meaningful area, or is fragmented into an implausible remainder;
- separate subtype and host identity plus future evolution metadata.

Immediate inspection remains on the actual river through the existing negative-influence views. Setting one Amount to zero must isolate the other class without requiring a new generic debugger.

Minimal telemetry per class:

- eligible hosts;
- attempted opportunities;
- accepted regions;
- dominant rejection reason;
- generation time.

The implemented Interior Pocket population is mathematically preserved at Amount `0.5` for the previously eligible baseline candidates: the accepted candidate score order, two-per-host cap, spacing, radius, and raster grammar remain unchanged at that value. Values below `0.5` activate a nested stable subset. Values above `0.5` may activate smaller secondary opportunities and a bounded third opportunity per sufficiently broad host.

Edge Cavities derive from broad Major-hosted interior maxima, choose one deterministic true aggregate-Major boundary side, bias their irregular negative field toward that side, and require both inside-host and outside-host coverage. Connector cores and unrelated Major hosts remain protected. A cavity is rejected when the aggregate negative field would consume more than the bounded host fraction or leave too little positive remainder. Stable metadata retains the negative class, host identity, transform, breach direction, and future evolution selectors.

### Patch 4.3 — Connector Weak Spans

**Implementation status:** Accepted after Unity visual validation.

Implemented:

- `Connector Weak Span Amount`, range `0–1`, default `0.5`;
- retention of each already-computed accepted Connector simplified metric path so later slices do not reconstruct geometry or run pathfinding again;
- stable association with one accepted Connector identity;
- placement inside the usable middle of the Connector and away from source and destination endpoint gates;
- short irregular negative spans aligned to the Connector path rather than generic circular pockets;
- deterministic variation between partial weakening and a near-complete local cut;
- one primary opportunity per eligible Connector, plus one secondary opportunity only on sufficiently long Connectors; all primary opportunities precede secondary opportunities in the nested activation order, so additional spans appear only at higher Amount;
- stable span identity, owning Connector identity, transform, and future along-Connector evolution metadata;
- concise eligible-Connector and Selected/Feasible telemetry.

A Weak Span does not delete, reroute, or regenerate the Connector relationship. It remains independent negative pressure over positive Connector Support and is clipped away from strong Major interiors, invalid water, and exact obstacles.

### Patch 4.4 — Free-Water Negative Events

**Implementation status:** Accepted after Unity visual tuning; default population and size spread remain provisional for later final-material validation.

Implemented:

- `Free-Water Event Amount`, range `0–1`, default `0.5`;
- deterministic opportunities generated from fixed metric-space spacing rather than structural-grid cell counts;
- valid-water placement without requiring a Major or Connector host;
- stable ranking that activates neutral, well-contained water first while retaining later positive-overlap opportunities;
- irregular soft masks with mild flow-axis elongation and bounded orientation variation rather than generic circles;
- exact-obstacle rejection, invalid-water clipping, bounded within-class spacing, and category-specific density so the default remains sparse;
- continued strong live Pressure, Lee, and Shore core protection through the existing composition pass rather than a new CPU reconstruction or readback;
- stable event identity plus downstream/lateral movement, phase, allowed-span, recycle, growth, and current provisional fade selectors; ordinary Patch 4.7B evolution will use the approved single-instance dwell/move/morph contract rather than duplicate-instance fading;
- concise `Free-Water Opportunities` and `Free-Water Selected / Feasible` telemetry in the existing negative-topology Inspector section;
- immediate visibility through the existing `Negative Influence Classes` and `Support and Negative Influence` diagnostics;
- no new compute kernel, material pass, pathfinding pass, runtime maintenance pass, or scheduling phase.

Free-Water Negative Events are topology only. They do not erase anything directly. Patch 4.7B/4.7B.1 now evolves their retained local masks through the approved single-instance downstream/lifetime/recycle lifecycle.

### Patch 4.5 — Static negative and combined topology validation

**Implementation status:** Accepted for feature progression after combined control/seed testing; exact tuning remains provisional.

Validate all four negative classes together with:

- Major Support;
- Connector Support;
- live Pressure, Lee, and Shore Support;
- exact Obstacle Footprint;
- valid-water constraints.

Every negative class must also be isolatable by setting the other three Amount controls to zero. No class may hide another class's failure.

### Shared explicit exclusions

- no material aging response;
- no direct geometric subtraction from Major or Connector Support;
- no runtime movement, fading, respawn, or recycling yet;
- no gameplay distance transforms, candidate search, path search, or rejection loops;
- no additional negative class without a separate approved plan.

### Shared acceptance gate

Pass only if:

- Interior Pockets remain firmly hosted and preserve a useful positive rim;
- Edge Cavities create deliberate open-sided negative structure without destroying their host;
- Connector Weak Spans read as local fragility rather than invalid Connector generation;
- Free-Water Negative Events remain sparse, valid-water-bound, obstacle-safe, and visually subordinate;
- every Amount control is deterministic, nested, and produces a sensible default at `0.5`;
- the aggregate negative field remains separately available from positive support;
- subtype identity and future evolution metadata remain available after aggregate packing;
- generation occurs only in staged preparation or explicit dirty rebuilds.

### Rollback

Each subpatch must be removable independently while retaining all previously accepted positive and negative classes.

---

## Step 5 — Static Combined Topology Validation

**Status:** Accepted for feature progression as Patch 4.5; exact coefficients remain provisional.

### Purpose

Validate the complete static topology relationship without introducing Foam material behaviour or runtime movement. This step corresponds to Patch 4.5 after all four negative classes exist.

### Exact behaviour change

Compose and upload:

- Major Support;
- Connector Support;
- aggregate Negative Aging Pressure from Interior Pockets, Edge Cavities, Connector Weak Spans, and Free-Water Negative Events;
- live Pressure Support;
- live Lee Support;
- live Shore Support;
- exact Obstacle Footprint;
- valid-water constraints.

Preserve independent positive and negative channels. Do not use destructive positive-times-one-minus-negative composition.

All generated classes retain stable class identity, region/opportunity identity, and future evolution metadata even though phase remains static in this step.

### Immediate inspection

Use the existing on-river views:

- Support Classes;
- Negative Influence Classes;
- Support and Negative Influence;
- Anchored Support;
- Obstacle Footprint.

No new generic combined debugger is added. Isolate each negative class by setting the other three negative Amount controls to zero.

### Test

Run the complete static matrix:

- several river shapes and widths;
- no, few, and many obstacles;
- low/medium/high Major and Connector controls;
- `0`, `0.5`, and `1` for every negative Amount control;
- each negative class in isolation;
- all four negative classes at their defaults;
- multiple seeds;
- reverse flow;
- quality tiers;
- cold and warm Play entry;
- queued source/domain rebuilds;
- repeated generation with identical inputs.

### Acceptance gate

Pass only if:

- Major and Connector retain their accepted gates;
- all four negative classes pass the Step 4 shared gate;
- the combined composition preserves substantial neutral water;
- anchored support remains attached to authoritative sources;
- exact obstacle and valid-water constraints remain correct;
- positive and negative overlap remains visible and non-destructive;
- no layer hides another layer's failure;
- default `0.5` negative settings produce a sensible aggregate rather than overwhelming the river;
- generation costs are named and profiled;
- no ordinary ready-state frame regenerates topology.

### Rollback

Restore the previous composition/upload integration while retaining separately accepted generators for diagnosis.

---

## Patch 4.6 — Lively Single-Instance Major Evolution

**Status:** Implemented; visual and performance acceptance pending. The accepted soft Major masks and bounded per-slot local recycle anchors are retained during preparation, while gameplay advances one active instance per slot and reconstructs the low-resolution Major field only during movement. No runtime generation or pathfinding was added.

### Purpose

Prove the runtime evolution foundation with Major Support only: frequent asynchronous movement, visible shape change, stable population, spatially distributed instant recycling, and no expensive generation during gameplay.

### Core invariant

Each accepted Major owns one logical slot and exactly one active topology instance. Ordinary evolution must never rasterize simultaneous old and new copies of the same slot.

### Exact behaviour change

Each Major occurrence follows this provisional lifecycle:

1. remain at its current state for a deterministic `2–5 s` dwell;
2. select a target state with positive downstream displacement and bounded lateral/diagonal displacement;
3. over roughly `1–2 s`, move and morph the same instance to that target;
4. commit the target and select a new independent dwell;
5. repeat while elapsed time and completed hops consume one combined lifetime-unit budget, until that budget or downstream egress is reached;
6. remove that occurrence and place the same slot immediately at a valid anchor inside its persistent local recycle territory in the same topology update.

Patch 4.6.2 exposes `Major Lifetime Units` (`1–20`, default `6`) and `Major Lifetime Unit Deviation` (`0–10`, default `2`). Each occurrence receives the base budget plus deterministic `±` variation, with a minimum of one unit. Elapsed time and completed hops both spend that budget; a normal five-second dwell-plus-move cycle consumes approximately one unit. This replaces the independent time-or-hop limits because either factor alone could allow locally persistent clumps or excessive state churn. An internal maximum-duration safeguard remains as a pathological-case ceiling.

Every hop must change the Major's shape. Use retained soft local masks plus cheap deterministic warp/variant parameters, non-uniform scale, shear, and small rotation. Preparation may retain several compatible shape variants when spending modest memory eliminates runtime generation. The summed support of variants should remain broadly comparable so morphing does not steadily change total support population.

Movement rules:

- longitudinal displacement is always positive in canonical downstream coordinates;
- lateral displacement may be left or right;
- diagonal movement is encouraged;
- no whole-field wrap is permitted;
- each slot recycles around its original accepted longitudinal percentage;
- `Major Recycle Territory Deviation (%)` exposes the permitted `±` deviation with range `0–10` and default `3`;
- near-egress original positions shift upstream enough to preserve a useful movement runway;
- identical input state and elapsed time reproduce identical slot behaviour.

### Runtime representation

Retain or add only compact value data:

- stable slot identity;
- accepted local sparse mask and compatible warp/variant data;
- current and target transform/shape descriptors;
- dwell, move, combined lifetime-unit, and cycle selectors;
- per-slot local recycle-anchor catalogue, original home territory, and downstream egress limit.

During movement, interpolate the descriptor and rasterize one current state. Do not blend two complete support placements.

### Scheduling and performance contract

- begin with about `5` topology reconstruction ticks per second only while at least one Major is moving;
- perform no Major field reconstruction while all Majors are dwelling;
- batch all active Major slots into bounded field work; never dispatch once per Major;
- use no runtime candidate search, component analysis, distance transform, rejection, pathfinding, CPU texture construction, or managed allocation;
- sleeping, frozen, distant, and inactive chunks perform no evolution work;
- prioritize lower CPU/GPU compute and latency over small additional memory usage.

### Immediate inspection

Show Major Support movement and morphing on the actual river.

Required telemetry:

- active Major slot count;
- dwelling and moving counts;
- minimum/maximum resolved dwell and move durations;
- topology reconstruction tick count;
- recycle count and crowded-anchor fallbacks;
- longitudinal upstream-displacement violations;
- per-tick CPU/GPU profiler markers and runtime allocation count.

### Explicit exclusions

- no Connector evolution;
- no Interior Pocket, Edge Cavity, Weak Span, or Free-Water evolution;
- no Anchored Support movement;
- no additional public evolution controls beyond the Patch 4.6.1 recycle-territory deviation control and Patch 4.6.2 lifetime-unit controls;
- no explicit full-topology rebuild transition;
- no cache packaging redesign.

### Test

Observe:

- forward and reverse river configurations;
- several seeds and Major populations;
- short and long rivers;
- different quality tiers;
- at least several minutes of asynchronous dwell/move/recycle behaviour;
- slots initially near the downstream edge;
- chunk sleeping/freezing and reactivation;
- deterministic replay from identical initial state;
- cold and warm profiling.

### Acceptance gate

Pass only if:

- no Major remains static longer than its approved provisional dwell envelope;
- different Majors visibly move at different rhythms;
- every hop makes positive net downstream progress;
- lateral/diagonal movement is present without bank leakage;
- every hop visibly changes shape;
- there is exactly one active instance per slot;
- recycling is instantaneous, remains near each slot's original longitudinal territory, avoids crowded anchors when possible, and cannot collapse the population into one shared river section;
- long-term slot count remains constant;
- no synchronized conveyor dominates;
- no expensive generation operation or managed allocation appears during ordinary gameplay;
- measured tick cost is low enough to justify extending the architecture.

### Rollback

Disable Major evolution and return to the accepted Patch 4.5 static Major field while retaining the prepared metadata for diagnosis.

---

## Patch 4.7 — Class-Specific Dependent Evolution

Patch 4.7 is deliberately split so each relationship problem is proven separately.

### Patch 4.7A — Hosted Interior Pocket and Edge Cavity evolution

**Implementation status:** implemented; Patch 4.7A.1 correctness correction applied and visually accepted.

Interior Pockets and Edge Cavities are stored in Major-host-local space. At the Patch 4.7A boundary, the accepted static negative field was separated into evolving hosted negatives and independent negatives, leaving Weak Spans and Free-Water Events unchanged until their own later slices. Free-Water Events are now handled by Patch 4.7B.1. Weak Spans use the Patch 4.7C.2 reconstruction foundation and Patch 4.7C.3.1 current-or-replacement-path/tangent following when complete prepared data is available, and retain the complete accepted static fallback otherwise.

They inherit:

- the host's complete downstream and lateral movement;
- host rotation and broad scale;
- the host's instant upstream recycle;
- the host's lifecycle identity.

They may add bounded independent variation:

- small host-local offset;
- modest scale/orientation change;
- compatible soft-shape variant or warp;
- deterministic participation on only some host hops.

Interior Pockets must remain inside the safe Major interior. Edge Cavities must remain attached to the original breach side and preserve a viable positive host remainder. Local variation uses absolute bounded targets, not an accumulating random walk.

The evolving field must initially reproduce the accepted static hosted-negative footprint closely. Edge Cavities retain pressure outside the Major support silhouette, and any accepted hosted region that cannot be prepared for evolution remains present through a static fallback. Initial parity measurement is Editor/development-diagnostic only and adds no normal-run readback step.

**Acceptance:** hosted negatives never detach, switch hosts, flip breach sides, escape safe bounds, duplicate during movement, or require runtime host search.

### Patch 4.7B — Slower Free-Water Negative Event evolution

**Implementation status:** Patch 4.7B independent local-mask evolution and Patch 4.7B.1 canonical downstream/lifetime/recycle correction are implemented and visually accepted.

Each Free-Water Event remains one logical slot with one active instance. The corrected runtime state follows the same single-instance lifecycle pattern as Major Support, with slower independent timings and no host relationship:

- `5–10 s` independently selected dwell;
- `2–4 s` movement/morph;
- every ordinary hop has positive longitudinal displacement;
- bounded lateral/diagonal motion may vary direction without producing longitudinal upstream travel;
- stronger grow/shrink and modest orientation variation than Major Support;
- one deterministic occurrence budget consumed by both elapsed time and completed hops;
- downstream egress and exhausted lifetime both trigger an instant recycle;
- recycle selects from a strictly bounded set of preparation-time validated upstream valid-water anchors retained with that event;
- the original and recycled poses use one mask instance only, with no overlapping old/new copy and no ordinary fade requirement;
- active movement reconstructs at the existing slow `2 Hz` proof cadence; dwelling-only Free-Water state performs no unnecessary reconstruction.

Preparation now retains per-event recycle anchors in canonical local river coordinates. Anchor validation uses the accepted local pressure mask against fluid coverage, domain bounds, and exact obstacle exclusion during preparation. Normal gameplay only advances compact descriptors and selects among retained anchors; it performs no placement search, rejection loop, retry, readback, or preservation test. The Inspector distinguishes ordinary prepared anchors from the validated safe-home fallback anchor. If any accepted event cannot prepare at least one valid anchor, Free-Water evolution remains unavailable for that build and the complete accepted static Free-Water field stays authoritative; the `Prepared / Accepted` mismatch exposes that unavailable state rather than losing an event.

**Acceptance:** events remain sparse, obstacle-safe, valid-water-bound, visibly slower than Majors, differently paced, shape-changing, incapable of ordinary upstream hops, and incapable of downstream-edge recycle loops. Recycle count, prepared-anchor count, anchor fallbacks, observed timing ranges, and upstream violations are shown in the existing Inspector diagnostics.

### Patch 4.7C.0 — Canonical topology field-space contract

Introduce one deliberately small CPU utility and matching HLSL formulas for the coordinate operations currently duplicated across Major, hosted-negative, Free-Water, and Connector preparation/runtime code:

- canonical texel-centre coordinates;
- topology UV and field bounds;
- longitudinal river distance;
- normalized and metric lateral coordinates;
- inverse river-to-field mapping;
- scalar-field bilinear sampling and clamping conventions.

Migrate existing implementations without redesigning feature-specific shape generation. Connector paths remain path-specific and Major/negative masks remain mask-specific; only their common river/field coordinate contract is shared. This is a behaviour-preserving refactor. Any visible change or parity difference is treated as evidence of an old convention disagreement and investigated explicitly. Debug-only diagnostics are permitted; normal gameplay gains no validation, readback, retry, or additional search work.

**Implementation status:** implemented and visually revalidated. `StylizedRiverFoamTopologyFieldSpace.cs` now owns CPU texel-centre UV, field spacing, longitudinal distance, normalized/metric lateral conversion, inverse metric-to-cell mapping, shared metric-position generation, and scalar bilinear sampling. Major, Connector, Pocket/Free-Water, exact obstacle, and runtime metric/boundary paths use this contract. `CS_RiverFoam.compute` mirrors the texel-centre, UV-to-texel, containing-texel, lateral conversion, and cell-centred bilinear-coordinate formulas. Feature-specific Major masks, negative masks, Connector routing, and obstacle interval logic remain specialized. No normal-runtime validation, retry, search, or readback was added.

**Acceptance:** existing Major, hosted-negative, and Free-Water identity poses reconstruct unchanged within the accepted diagnostic tolerance, and all migrated code uses one texel-centre and inverse-mapping convention.

### Patch 4.7C.1 — Connector and Weak Span preparation data

Extend immutable preparation data without enabling movement yet.

Connector preparation must retain:

- individual source and destination Major-slot ownership rather than only merged static component identity;
- each endpoint gate in its owning Major's local prepared frame;
- the accepted bounded/resampled path points;
- normalized cumulative arc length along that path;
- a strictly bounded set of prevalidated path alternatives for relevant endpoint recycle-anchor combinations;
- an explicit unavailable state for endpoint/path combinations that cannot be prepared correctly.

Endpoint ownership is resolved during preparation by the individual Major contribution at the accepted gate. Ordinary gameplay must not infer ownership, search hosts, or rebuild relationships.

Weak Span preparation must retain:

- owning Connector stable identity;
- normalized distance along the accepted path;
- longitudinal and lateral physical radii;
- strength, seed, orientation/tangent requirements, and any shape parameters needed for exact reconstruction;
- endpoint-clearance limits required to remain gate-safe after deformation or replacement.

**Implementation status:** implemented. Each accepted Connector now retains its unchanged authoritative static polyline plus a bounded runtime polyline of at most `48` points, normalized cumulative arc length, and two preparation-time endpoint bindings selected from the strongest individual Major-mask contribution at each accepted gate. Gate offsets are stored in the owning Major candidate's principal-axis cell frame. Patch 4.7C.1 initially retained a deliberately small subset of deterministic recycle alternatives. Patch 4.7C.3.1 corrects that limitation by retaining every Cartesian combination of the identity state and all actual prepared recycle anchors for both endpoints. Each alternative is a prewarped accepted path and is retained either as available or with an explicit failure reason after bounded valid-water, exact-obstacle, and stretch checks. No gameplay pathfinding or runtime geometry validation is added.

Each accepted Connector Weak Span now retains its Connector identity, normalized path distance, normalized endpoint-safe interval, physical along/across radii, strength, deterministic evolution seed, accepted tangent, and accepted orientation. Records remain explicit when their owning Connector preparation is unavailable or their path position is invalid. Inspector diagnostics report prepared/accepted Connectors, owned/unresolved endpoints, retained point count, available/unavailable recycle variants, and prepared/unavailable Weak Span attachments. The static Connector and Weak Span fields remain authoritative in this subpatch.

**Acceptance:** every accepted Connector endpoint and Weak Span either has complete immutable runtime data or is reported explicitly as unavailable. No runtime raster path changes in this subpatch. Unity preparation diagnostics passed in the validation scene: `11 / 11` Connectors prepared, `22 / 0` endpoints owned/unresolved, `11 / 11` Weak Span attachments prepared, and zero unavailable attachments. Bounded recycle alternatives included both available and explicitly unavailable combinations as designed.

### Patch 4.7C.2 — Identity reconstruction and parity

Move Connector Support and Connector Weak Span rasterization into the prepared runtime reconstruction path while evaluating every record at its accepted identity/static pose.

The identity result must reproduce:

- the accepted static Connector Support field;
- the accepted static Connector Weak Span field;
- their existing composition with Major Support, all negative classes, Anchored Support, and Obstacle Footprint.

Editor/development-only parity compares reconstructed identity fields against the accepted static source fields. Normal gameplay performs no readback or comparison. When prepared identity reconstruction is available, the old static Connector/Weak Span channel stops being authoritative; unavailable records remain explicit rather than silently disappearing.

Descriptor advancement for Major, hosted-negative, Free-Water, Connector, and Weak Span state should be collected before reconstruction so one applicable topology tick performs at most one combined upload/rebuild instead of duplicate full-field reconstruction.

**Implementation status:** implemented and visually accepted. Connector identity records and flattened retained metric paths are uploaded once after preparation. The shared evolving-topology compute pass reconstructs complete Connector Support from those identity paths and reconstructs Weak Span pressure from normalized path attachment, physical radii, accepted strength, and deterministic boundary noise. Static Connector/Weak Span channels are omitted only when the corresponding complete identity reconstruction is available; otherwise the accepted static fields remain authoritative as a complete-only fallback. Separate Editor/development-only asynchronous parity readbacks compare Connector Support and Weak Span pressure against their accepted static source fields. Normal runs perform no readback or comparison. Major and Free-Water descriptors now advance first and request at most one combined evolving-field reconstruction per applicable update tick.

**Acceptance:** identity parity passes before any Connector movement is enabled, and the runtime path introduces no per-Connector dispatch, allocation, search, or validation loop.

### Patch 4.7C.3 — Connector Support and Weak Span evolution

Ordinary endpoint motion uses the accepted simplified Connector polyline:

- source-near points follow the source Major gate;
- destination-near points follow the destination Major gate;
- middle points blend the two endpoint transforms through normalized cumulative arc length;
- a small bounded transverse deformation prevents rigid-band motion;
- endpoint gates and Major-clearance rules remain valid;
- the Weak Span samples its normalized path position and follows the deformed path tangent;
- no gameplay pathfinding or ownership search occurs.

When either endpoint Major instantly recycles:

- the Connector switches in the same topology update to the prevalidated path variant associated with the active endpoint recycle-anchor combination;
- the Weak Span follows its owning replacement path by normalized distance;
- an unavailable combination produces temporary Connector/Weak Span absence rather than an invalid long stretch, emergency bridge, runtime route search, or retry;
- the relationship becomes eligible to reappear when a valid prepared combination is active.

Preparation-time path alternatives remain strictly bounded and profiled. Cross-Major replacement relationships are not introduced unless testing proves same-slot endpoint-anchor variants insufficient. Connector evolution must not become a continuously maintained graph.

**Implementation status:** implemented; Unity visual validation pending. Each Connector stores direct indices to its two resolved Major evolution slots. On every applicable combined topology rebuild, the runtime resolves each endpoint gate from the current interpolated Major pose and the retained Major-local gate coordinate. The accepted path remains active while both hosts remain in their identity occurrences. After either host recycles, the runtime selects only the exact prevalidated path variant matching the two hosts' retained recycle-anchor indices. Missing or rejected combinations set the Connector record's point count to zero, producing temporary Connector and Weak Span absence without route search, retry, stretching, or a duplicate instance.

For active relationships, the selected bounded polyline is deformed between the two current endpoint gates by normalized cumulative arc length. A small deterministic transverse displacement is enveloped away from the gates so the interior does not behave as a rigid band. The flattened path buffer is updated in place, cumulative arc length is renormalized, and attached Weak Spans sample their retained normalized path positions and the current segment tangent. Connector and Weak Span descriptors upload inside the existing shared topology reconstruction; no per-Connector compute dispatch or managed runtime collection is added. Inspector telemetry reports active/temporarily absent relationships, identity/recycle-variant use, variant switches, absence/reappearance events, and active/absent Weak Spans. Normal gameplay performs no relationship validation readback.

**Acceptance:** ordinary deformation stays attached, recycle switching never stretches across a relocated endpoint, Weak Spans remain gate-safe and tangent-aligned, unavailable combinations disappear cleanly and later reappear when a valid prepared combination returns, no runtime route search occurs, and Connector cost remains subordinate.

**Long-run finding:** the first 4.7C.3 implementation did not satisfy the complete recycle contract. It retained at most three of up to six Major recycle anchors per endpoint and at most twelve combinations, while runtime Majors could select any prepared anchor. A Connector whose host entered an unprepared combination became absent. Because each Connector remained permanently tied to its original pair, repeated recycle cycles caused the visible Connector population to monotonically drain.

### Patch 4.7C.3.1 — Connector relationship recycle correction

Preparation now performs two bounded additions:

- every accepted relationship retains all combinations of identity plus every actual prepared recycle anchor at both endpoints; with the current maximum of six anchors per Major this is at most `48` non-identity variants per relationship;
- a bounded catalogue of additional prevalidated relationships between different individual Major slots is prepared from valid Connector opportunities that were not selected into the initial static population. These catalogue entries retain the same bounded path, endpoint ownership, complete anchor-state matrix, valid-water, exact-obstacle, and stretch checks as accepted relationships.

Runtime keeps the fixed logical Connector/Weak Span slot population. During each applicable combined topology update it:

1. retains every currently valid relationship assignment first;
2. releases only assignments whose exact current anchor-state variant is unavailable;
3. scans the bounded prepared catalogue in deterministic slot-specific order;
4. assigns only unclaimed relationships between unclaimed Major pairs;
5. enforces the prepared per-Major Connector degree for replacement assignments;
6. skips the just-released relationship during the immediate replacement selection;
7. updates the existing fixed GPU path record in place and lets the slot's Weak Span follow the newly assigned path;
8. remains temporarily absent only when no currently applicable prepared catalogue entry exists.

This is bounded catalogue selection, not gameplay routing. No pathfinding, component search, geometry validation, rejection retry, graph expansion, managed collection construction, per-Connector dispatch, or GPU readback occurs during ordinary runtime.

**Implementation status:** implemented; Unity long-run validation pending. Inspector telemetry reports prepared accepted/replacement relationship counts, accepted and replacement anchor variants, original/replacement active assignments, relationship rebinds, anchor-variant switches, temporary absence/reappearance, and active/absent Weak Spans.

**Acceptance:** after every Major slot has recycled repeatedly, Connector population remains statistically stable rather than collapsing to zero; visible rebinds connect different current Major targets without duplicates, excessive per-Major concentration, stretched emergency bridges, detached remnants, or old/new overlap. Weak Spans follow the replacement paths. Temporary absence is bounded and recovers when a prepared assignment is available.

### Patch 4.7C.3.2 — Connector break and relationship turnover

Long-run validation of Patch 4.7C.3.1 showed that complete prepared relationship coverage alone was insufficient:

- a currently valid relationship was preserved regardless of how far ordinary Major motion stretched its live deformed path;
- a host recycle changed only the prepared anchor-state variant when the same Major pair remained valid, creating a hard preservation bias rather than visible relationship turnover.

Patch 4.7C.3.2 adds one exposed runtime control:

- **Connector Break Stretch Ratio** ranges from `1.10` to `2.00` and defaults to `1.45`;
- the reference length is captured whenever a Connector receives a relationship or enters a different prepared endpoint-anchor variant;
- the Connector breaks when its current live deformed length exceeds `reference length × ratio`;
- the rule is deliberately relative-only: there is no additional absolute-metre allowance or prepared-safe-minimum clamp.

A stretch break releases the old relationship immediately and performs one bounded deterministic catalogue selection in the same topology update. The released Major pair is excluded. The old stretched relationship is never used as a fallback; its Major pair remains blocked for that Connector slot until either blocked host begins a different occurrence, and temporary absence remains valid when no different prepared pair is available.

Whenever either endpoint Major begins a recycled occurrence while the current relationship remains valid, the Connector makes one deterministic pseudorandom decision derived from stable slot identity and the new occurrence counts:

- approximately half retain the current Major pair and use its exact prepared anchor-state variant;
- approximately half request a different currently valid prepared Major pair;
- a requested turnover excludes the previous pair and preserves unique Major pairs; endpoint concentration is handled by the Patch 4.7C.3.3 soft distribution bias rather than a hard degree ceiling;
- when no different relationship is available, the still-viable previous relationship is retained rather than deleting the Connector needlessly.

Weak Spans remain attached to their logical Connector slots, so a stretch break or successful recycle turnover moves the Weak Span to the replacement path and current tangent without creating a second instance. Runtime work remains bounded prepared-catalogue selection plus the path-length calculation already required for cumulative arc-length output. No pathfinding, geometry validation, retry loop, candidate generation, GPU readback, or managed runtime collection is added.

**Implementation status:** implemented; Unity visual and long-run validation pending. Inspector telemetry reports the exposed ratio, stretch breaks, recycle retain decisions, turnover requests, successful turnovers, no-alternative retains, relationship rebinds, and temporary absence/reappearance.

**Acceptance:** over repeated recycle cycles, retain decisions and turnover requests approach an approximately even distribution subject to deterministic sampling; successful turnovers visibly connect different Major pairs when alternatives exist; a relationship cannot exceed the configured relative stretch envelope; stretch-broken paths never reclaim the same pair as an emergency fallback; Weak Spans follow every replacement; and the Connector population remains stable without stretched bridges, duplicate pairs, systematic hub concentration, old/new overlap, runtime search, or readback.

### Patch 4.7C.3.3 — Soft Connector distribution bias

Unity validation of Patch 4.7C.3.2 confirmed stretch breaking and relationship turnover, but exposed a distribution defect: most Major patches remained unconnected while a small number repeatedly accumulated several relationships. The cause was structural rather than visual randomness. Initial generation used only tiny additive degree adjustments, and runtime rebinding accepted the first valid prepared candidate under a hard per-Major degree ceiling.

Patch 4.7C.3.3 replaces both policies with the same deterministic weighted distribution rule:

- every existing endpoint connection multiplies candidate weight by `0.22`;
- concentration on the busier endpoint applies an additional `0.60 ^ max(endpoint degrees)` multiplier;
- prepared geometry and the authored Connector Length Preference remain part of the base candidate weight;
- weighted selection uses stable topology/slot identity, so the result remains deterministic for one seed;
- no degree is categorically forbidden, preserving a small but non-zero chance for occasional hubs;
- duplicate active relationships between the exact same Major pair remain forbidden because they do not add a distinct relationship.

Initial Connector generation applies the weighting while selecting candidate component pairs. Endpoint alternatives for the same component pair share that pair's probability mass, preventing a pair with more retained endpoint alternatives from gaining an accidental multiplicity advantage. Runtime rebinding applies the same load and hub multipliers to the bounded prepared relationship catalogue and performs one deterministic weighted draw.

Recycle turnover remains approximately `50%` for relationships whose endpoints each have degree one or less. Crowding raises turnover probability by `15` percentage points for every endpoint connection above degree one, capped at `90%`. This is still a probability rather than a forced cleanup rule: a hub may persist, and no relationship breaks solely because an endpoint is popular. Stretch breaking remains the only hard live-viability break.

**Implementation status:** implemented; Unity visual and long-run distribution validation pending. Inspector telemetry reports the active Major degree histogram (`0 / 1 / 2 / 3+`), maximum active degree, and turnover requests caused specifically by the crowding boost.

**Acceptance:** across repeated generation seeds and long recycle runs, degree-zero Major patches should fall substantially while occasional degree-three-or-higher hubs remain possible. Rebinding should favour less-used endpoints without creating a rigid matching pattern, the relationship population should remain stable, unique pairs and stretch limits must remain intact, and no runtime pathfinding, geometry validation, retry loop, GPU readback, or managed candidate construction may be introduced.

### Shared inspection and telemetry

Inspect each Patch 4.7 class separately on the actual river, then together.

Limit telemetry to:

- slot counts by class;
- dwelling/moving/recycle counts;
- host-loss, breach-side, and invalid-relationship violations;
- Connector spare-use and no-spare counts;
- upstream longitudinal displacement violations;
- batched update ticks, dispatches, CPU/GPU time, and allocations.

### Shared rollback

Each subpatch may return its classes to their accepted Patch 4.5 static fields while retaining earlier accepted evolution slices.

---

## Patch 4.8 — Safe Explicit Topology Rebuild Transition

### Purpose

Replace complete generated topology after an explicit source/domain/settings rebuild without allowing partially rebuilt Major, Connector, and Negative classes to become authoritative. This remains separate from ordinary per-slot evolution, which uses one active instance and no old/new support duplication.

### Patch 4.8A — staged same-grid replacement preparation and atomic activation

**Status:** implemented and Unity-verified.

Patch 4.8A replaces the previous in-place Major → Connector → Pocket rebuild tail for same-grid topology changes.

When Seed, Amount, Size, Connector settings, Negative settings, or prepared obstacle context changes while the active field dimensions remain valid:

1. capture one immutable replacement request containing the current river domain reference, quality, field dimensions, authored topology settings, seed, and a private copy of the prepared obstacle scalar field;
2. leave the currently accepted topology and all of its ordinary evolution active;
3. build replacement Major Support, Connector Support, and all four Negative classes in separate frames;
4. prepare a separate generated-topology texture containing the complete static fallback result;
5. make no active topology object, evolution buffer, or bound generated texture point at the replacement while preparation is incomplete;
6. activate only the complete replacement in one commit step, then rebuild compact evolution records and compose the new topology before the frame is bound;
7. retire the previous generated-topology texture only after the atomic commit.

A newer target signature cancels only the in-progress replacement. The accepted active topology is not cancelled or cleared. Repeated frames observing the same target do not duplicate work; a genuinely newer request increments coalesced/cancelled telemetry and restarts from the newest immutable snapshot.

Boundary and authoritative Obstacle Footprint refresh remain live source maintenance. After obstacle preparation stabilises, the new obstacle scalar is captured by the replacement request instead of rebuilding active generated classes in place.

Patch 4.8A includes an Editor/development validation command, **Prepare Identical Topology Replacement**. It prepares an identical replacement through every staging phase and discards it without activation. This proves that replacement preparation itself has no visible side effect even when the active topology has already evolved away from its identity pose.

Patch 4.8A deliberately covers **same-grid generated topology replacement**. A domain or quality change that alters field mapping or dimensions still follows the complete resource-initialization path. Preserving and transitioning between two differently mapped complete resource sets belongs to Patch 4.8B rather than being hidden inside this subpatch.

Required telemetry:

- replacement build phase;
- replacement readiness;
- last request reason;
- request and activation counts;
- coalesced and cancelled replacement counts;
- completed identical preparations.

Patch 4.8A passes only if:

- old generated topology remains visible and continues ordinary evolution throughout preparation;
- no partially rebuilt class becomes active;
- settings and obstacle changes activate only after Major, Connector, Pocket, and replacement upload are complete;
- rapid successive changes cancel/restart only replacement work and settle on the latest target;
- identical preparation completes without any visible topology change;
- activation produces no neutral frame, although a direct old/new shape discontinuity remains expected until 4.8B.

### Patch 4.8B — safe old/new generated-topology transition

**Status:** implemented and Unity-verified.

Patch 4.8B retains one fully resolved old generated-topology snapshot when a complete 4.8A replacement activates. The snapshot is not merely the static upload: one dedicated compute pass resolves the currently visible Major, hosted-negative, Free-Water, Connector, and Weak Span generated classes, including any active ordinary evolution, into one immutable RGB generated-topology texture.

The complete new topology becomes the target only after preparation. Composition then performs a bounded internal one-second linear crossfade:

```text
Generated = lerp(ResolvedOldGenerated, ResolvedNewGenerated, progress)
FinalTopology = ComposeLiveSourcesOnce(Generated)
```

This prevents both a neutral-frame flash and additive old/new double strength. Pressure, Lee, Shore, exact Obstacle Footprint, static wake, and other authoritative live sources remain outside the generated fade and are sampled/composed only once against the blended generated field.

Same-mapping replacements load the retained old texel directly. For a domain or quality change whose dimensions or mapping differ:

1. capture the current fully resolved generated field and retain its metric rows, global start, field length, and valid length;
2. detach and hold the prior complete renderer bindings while the normal staged resource initialization creates the new complete field set;
3. after readiness, release the held renderer bindings and remap each current topology texel into the old snapshot by global river distance and physical lateral metres;
4. treat positions outside the old valid river interval or old lateral water width as zero old coverage rather than clamping an invalid edge strip;
5. crossfade the remapped old generated field into the new generated target.

If another complete topology activates before the current fade completes, one capture pass first flattens the currently visible old/new generated blend into a fresh transition snapshot. The superseding replacement then fades from that flattened state; it never forces the prior transition to finish synchronously and never chains shader sampling through several historical fields.

Required telemetry:

- transition state: idle, holding previous mapping, or crossfading;
- current transition progress and fixed proof duration;
- started and completed transition counts;
- differently mapped transition count;
- superseding flattened-transition count.

Patch 4.8B passes only if:

- same-grid Seed/Amount/Size/Connector/negative replacements fade continuously from the complete visible old generated topology to the complete new topology;
- old and new support do not sum above their ordinary class ranges and no neutral/black generated frame appears;
- live Pressure, Lee, Shore, and Obstacle Footprint react authoritatively during the fade rather than being frozen or duplicated;
- rapid complete activations flatten the current visible blend and restart smoothly without a synchronous finish or multi-history sample chain;
- quality or domain changes keep the previous complete renderer result visible throughout staged initialization, then fade through physical river-space remapping once the new resource set is ready;
- old renderer resources, captured textures, and retained metric buffers are released after the transition or normal lifecycle cancellation;
- no normal-runtime GPU readback, path search, retry loop, or per-region transition dispatch is added.

### Rollback

Disable staged replacement and return same-grid topology changes to the last accepted active topology. Do not restore partial in-place Major/Connector/Pocket authority.

---

## Patch 4.9 — Production Cache and Precompute Packaging

Patch 4.9 is deliberately split so serialization correctness is proven before startup ownership changes.

### Patch 4.9A — versioned cache contract and deterministic round-trip proof

**Status:** implemented and Unity-verified.

#### Purpose

Prove that the complete accepted prepared topology can survive exact deterministic binary serialization before any asset ownership or normal-startup behaviour changes.

#### Implemented payload contract

The versioned payload stores exact 32-bit values for:

- field width, height, field length, valid field length, global river interval, local length, requested sample spacing, reverse-flow state, sample count, quality, topology-generation settings, and seed;
- the exact scalar Obstacle Footprint mask consumed by generation;
- complete Major support fields, accepted region identities/selectors, local masks, shape metrics, and every prepared recycle anchor;
- complete Connector support fields, accepted relationships, endpoint ownership, accepted/prepared paths, normalized cumulative lengths, every same-host anchor-state path variant, and the bounded replacement-relationship catalogue;
- all six retained Negative Aging Pressure scalar fields, all four accepted region classes, hosted masks and variants, Free-Water masks and recycle anchors, and Weak Span Connector attachment/path metadata;
- deterministic counters and rejection tables required to reconstruct the existing diagnostics.

Volatile generation timings are intentionally excluded from payload identity. Format version and generator-contract version are written explicitly, every collection is length-bounded, and an end-of-payload checksum rejects corrupt or truncated data neutrally.

Two internal raw reconstruction paths preserve already-canonical normalized Pocket breach vectors and Weak Span tangents bit-for-bit rather than normalizing accepted data a second time.

#### Explicit proof behaviour

The Play-mode Inspector exposes `Validate Topology Cache Round Trip`. The proof:

1. captures the active immutable prepared topology and obstacle scalar field;
2. serializes the same source twice and requires byte-identical output;
3. deserializes into fresh topology objects;
4. serializes the reconstructed graph again and requires exact payload equality;
5. reconstructs initial generated topology channels and compares them bit-for-bit;
6. compares every obstacle scalar value bit-for-bit;
7. corrupts one payload byte and requires neutral rejection;
8. reports state, run/pass counts, payload size/hash, serialization time, load time, verification time, and the exact failure reason.

The reconstructed topology is never activated. Patch 4.9A changes no normal startup phase, renderer binding, replacement transition, evolution cadence, or visible topology.

#### Acceptance gate

Pass only if:

- the explicit proof reports `Passed` on the validated river;
- repeated proof runs on unchanged active topology produce the same payload byte count and hash;
- every stable identity, mask value, path, variant, anchor, attachment, and evolution selector survives exact reconstruction;
- reconstructed initial generated channels match the active prepared topology exactly;
- a corrupt payload fails without replacing or clearing active topology;
- no topology generation, renderer activation, resource replacement, or startup-path change occurs because the proof ran.

### Patch 4.9B — stable fingerprints and explicit cache building

**Status:** implemented and Unity-verified.

#### Implemented stable keys

The versioned payload now embeds three deterministic 128-bit input fingerprints:

- **Domain:** exact IEEE-754 bits for the complete resampled domain contract—length, requested spacing, connected offset, reverse-flow state, sample count, and every sample centre, surface point, frame vector, local/oriented/global distance, left/right visible and generated widths, normalized distance, and spline time. `RiverDomainSnapshot.Version` is excluded.
- **Obstacle sources:** exact transformed world-space vertex and triangle data for the same sorted static generated meshes consumed by obstacle baking. Per-source fingerprints are sorted and combined as a multiset, so session-local `EntityId` and `ObstacleGeometryVersion` do not participate. No second intersection/rasterization implementation was introduced.
- **Generation inputs:** quality, field dimensions/lengths, Shore Motion, seed, Major amount/size/variation/recycle territory, Connector amount/directness/length preference, and all four negative-class amounts. Exact float bits are used rather than the old rounded 32-bit in-session signature.

A fourth combined key hashes those three fingerprints for concise authored river/cache identity. Existing session-local versions remain only as cheap live scheduling invalidation; they are not persistent cache keys.

#### Implemented ownership and tooling

`StylizedRiverFoamTopologyCacheAsset` is a runtime-readable storage provider containing the opaque versioned payload plus provider metadata. The codec does not reference UnityEditor or the asset, so future chunk/run files can store identical bytes. `StylizedRiver` owns an optional explicit asset reference.

The authored-river asset reference is established explicitly in Edit Mode. When no asset is assigned, the Inspector can create and assign an empty `StylizedRiverFoamTopologyCacheAsset`; creation uses a unique project path and does not run generation or fingerprinting.

In a fully initialized Play-mode river with an asset already assigned, the Inspector can:

1. **Build / Update Topology Cache Asset** — capture stable fingerprints, run the accepted 4.9A round-trip/corruption proof, serialize the complete prepared graph, overwrite the assigned asset payload, save it, and report payload/generator versions, size/hash/build time, and all input keys.
2. **Validate Assigned Topology Cache** — validate storage/payload versions and checksum, require metadata to match embedded bytes, recompute current stable inputs, and report `Hit Candidate` or a precise miss reason: unassigned, empty, unsupported storage, invalid payload, metadata mismatch, stale domain, stale obstacles, stale settings, or combined-key mismatch.

The loaded graph is never activated. No ordinary Play startup reads, validates, deserializes, uploads, or consumes the cache in 4.9B.

#### Acceptance gate

Pass only if:

- an empty cache asset can be created and persistently assigned in Edit Mode without generation;
- building updates that assigned asset in Play Mode with non-zero bytes, supported payload/generator versions, a stable payload hash, three populated fingerprints, a combined key, and `Built`;
- immediate validation reports `Hit Candidate`;
- rebuilding unchanged accepted topology reproduces the same payload size/hash and input fingerprints;
- changing only the domain, exact obstacle geometry/transform, or a generation setting reports the corresponding stale reason;
- restoring the changed input returns to a hit candidate without regenerating or activating topology because of validation;
- corrupt or mismatched asset bytes fail neutrally and active topology/rendering remain unchanged;
- ordinary Play entry remains the accepted proof-generation path and performs no automatic asset load or cache validation.

### Patch 4.9C — cache-first runtime initialization

**Implementation status:** implemented.

The staged startup lifecycle now resolves the assigned cache after obstacle-source registration stabilizes and before obstacle baking or topology generation. Generated owners expose prepared exact world-triangle fingerprints; the river combines those values using the unchanged 4.9B obstacle-set contract, so a startup hit does not reread or transform mesh triangles merely to prove freshness.

On a valid hit the runtime:

- validates storage, payload checksum/metadata, format/generator versions, domain, exact obstacle-source, generation, and combined fingerprints;
- deserializes the complete immutable Major, Connector, and four-class Negative Aging Pressure graph;
- uploads the exact cached obstacle scalar field directly;
- initializes the existing Major/hosted-negative/Free-Water/Connector/Weak-Span reconstruction resources;
- reuses the accepted generated-topology upload, evolving-field, and renderer-composition paths;
- skips transformed-mesh rescanning, obstacle interval baking, forced GPU readback, candidate generation, cleanup, pathfinding, replacement-catalogue construction, and negative-region searches.

A release build remains strict: an unassigned, empty, unsupported, corrupt, metadata-mismatched, stale-domain, stale-obstacle, stale-settings, incomplete, or provider-unavailable cache remains neutral and never silently runs the expensive preparation path. Editor and Development orchestration is refined separately by Patch 4.9C.1.

Live Pressure, Lee, Shore, wake, disturbance, obstacle-source registration, and final renderer composition remain authoritative runtime systems rather than serialized topology ownership.

### Patch 4.9C.1 — automatic development cache orchestration

**Implementation status:** implemented and Unity-verified.

Routine development becomes Play-only. An Editor-only coordinator scans loaded authored rivers before Play Mode, creates or reuses one deterministic cache asset beside the saved scene, and assigns it persistently when missing. The runtime then:

- loads an exact matching cache immediately;
- installs a structurally valid stale cache as the visible previous topology when domain mapping and field dimensions remain compatible;
- automatically runs the existing staged preparation path for missing, invalid, incompatible, stale-setting, or stale-obstacle development caches;
- routes a compatible stale replacement through the accepted 4.8A/4.8B complete-replacement and crossfade lifecycle;
- round-trip validates every completed generated topology before requesting persistence;
- automatically updates the assigned cache asset in the Editor without requiring create, build, validate, or explicit-generate buttons;
- keeps manual cache actions inside advanced diagnostics only.

The runtime captures the stable combined input key present at Play entry. A generated result is persisted only when its completed key still matches that persistent entry key. Inspector changes made only during Play Mode may still generate and display a session replacement, but they cannot overwrite the persistent Edit Mode cache.

A compatible stale cache remains visible during regeneration. A first-ever river with no payload, a corrupt payload, or a domain/dimension-incompatible payload has no safe prior field to display and may remain neutral only until its automatic development generation completes. Release behaviour is unchanged and cache-only.

### Patch 4.9D — production and cold-start validation

**Implementation status:** implemented and Unity-verified.

The runtime now records one bounded startup-validation window for every full Foam resource initialization. It reports total staged wall time, the slowest individual initialization phase, phase count, cache-install count, estimated active Foam memory, and explicit execution counts for obstacle baking plus Major, Connector, and Pocket generation. A direct persistent-cache hit must complete with all four expensive-preparation counters at zero; the existing Profiler markers remain available for independent confirmation.

A strict Edit-mode release validator deserializes the assigned payload without activating Foam or allocating its GPU simulation field. It verifies storage and payload versions, checksum and metadata agreement, complete graph/obstacle-field content, the current resampled domain, exact prepared obstacle-source fingerprints, generation inputs, and the combined stable key. It never falls back to triangle scanning, topology generation, asset creation, cache assignment, or cache mutation.

An Editor build preprocessor runs that validator over every enabled Foam river in every enabled Build Settings scene before a non-development player build. Missing, empty, corrupt, unsupported, metadata-mismatched, incomplete, stale-domain, stale-obstacle, stale-settings, combined-key, inactive-input, or unavailable-runtime cases reject the build with scene and hierarchy paths. Development builds retain the accepted automatic orchestration. A manual `Tools > Programmatic Stylized 3D > Rivers > Validate Release Foam Caches` command runs the identical gate without starting a build. Automatic cache assignment is suspended while preflight temporarily opens build scenes, so validation cannot modify authored content.

#### Acceptance gate

Pass only if:

- a first development run may report expensive preparation while automatically creating/updating its persistent payload;
- the next unchanged Play entry reports one direct cache hit, one cache installation, and zero obstacle/Major/Connector/Pocket preparation executions;
- the Profiler contains cache resolve/install markers but no obstacle bake or topology-generator markers on that hit;
- total staged startup, slowest-step duration, payload size, and estimated memory remain reasonable across Low, Medium, and High quality rivers;
- stale domain, obstacle, quality/settings, version, metadata, and corrupt-payload cases each fail with the correct reason;
- the manual release preflight passes for current caches and fails without creating or changing assets after one cache is removed or made stale;
- a non-development build is rejected by the same failure, while a Development build retains automatic development recovery;
- restoring/rebuilding the cache makes the preflight and release build pass again.

### Rollback

4.9A can be rolled back by removing the codec, raw reconstruction helpers, Inspector proof, and proof telemetry; active topology behaviour is otherwise untouched. Later 4.9 slices may return to the accepted staged proof-generation path while retaining the versioned payload files for diagnosis.

---

## Patch 4.10 — Topology Completion and Material-System Handoff

**Current material-state note:** Patch 4.10A froze topology resources and ownership. Its historical description of the then-current Amount/Integrity/Phase material state is superseded by the Presence/Remaining-Life/Material-Pattern contract implemented through 4.11C.3–4.11C.7. The topology textures and cache contract themselves do not change.

Patch 4.10 is intentionally split. Patch 4.10A freezes the contract before code changes. Patch 4.10B makes the smallest runtime and diagnostic changes required to conform to that contract and then closes topology.

### Patch 4.10A — Frozen material-facing topology contract

**Implementation status:** documented and accepted.

#### Purpose

Declare exactly what later Foam-material work may read, who owns each value, how it is sampled, and what must remain valid across cache installation, replacement, disable, and shutdown. This patch changes documentation only and does not change visible Foam, generated topology, cache bytes, fingerprints, or runtime behaviour.

#### Frozen resources and channels

| Material binding | Channel | Canonical meaning | Ownership |
|---|---:|---|---|
| `_FoamTopology` | R | Major Support | generated, evolving topology |
|  | G | Connector Support | generated, evolving topology |
|  | B | aggregate Negative Aging Pressure | generated, evolving topology |
|  | A | reserved zero | no current owner |
| `_FoamTopologySources` | R | Pressure Support | live anchored source |
|  | G | Lee Support | live anchored source |
|  | B | Shore Support | live anchored source |
|  | A | reserved zero | no current owner |
| `_FoamObstacleExclusion` | R | exact current Obstacle Footprint | canonical material and validity source |
| `_FoamBoundary` | existing boundary channels | valid-water and boundary information | domain/boundary runtime |
| `_FoamPrevious` / `_FoamCurrent` | RGBA | provisional persistent Foam material state | material lifecycle, not topology |

All material-facing topology, anchored-source, obstacle, and boundary values use normalized `0–1` ranges and the canonical Foam field mapping. Persistent material-state channel meanings remain outside the topology contract. The final material contract is Presence, Presence-weighted Remaining Life, Presence-weighted Material Pattern, and reserved zero as defined by 4.11C.3–4.11C.7.

#### Negative-class decision

The first material-lifecycle implementation receives **aggregate Negative Aging Pressure only**. Interior Pocket, Edge Cavity, Connector Weak Span, and Free-Water Negative Event identities remain retained in the complete cache payload, class-specific runtime evolution data, and diagnostics. They are not four independently bound material channels and Patch 4.10 does not allocate another runtime texture for them.

A later material-facing subtype expansion requires visible evidence that one aggregate response is insufficient and a separately approved patch defining memory, binding, transition, cache-compatibility, and authoring consequences.

#### Ownership boundary

- **Cache/preparation** owns immutable generated identities, prepared geometry, local masks, paths, anchors, variants, exact obstacle data, deterministic metadata, and serialization.
- **Topology runtime** owns generated evolution, live anchored-source composition, complete replacement activation, generated-topology crossfades, and normalized material-facing influence fields.
- **Material lifecycle** owns Presence, Remaining Life, Material Pattern, birth-event scheduling, transport, future breakup state, and material death. Emitter Amount remains source-only and is discarded after birth conversion.
- **Rendering** owns colour, opacity, visible edge treatment, and fine visual detail.

The topology runtime does not spawn or erase material, implement fragmentation or dissipation, determine final colour/opacity, or own material lifetime. The material-owned birth scheduler may sample the frozen topology outputs as bounded candidate weights or trajectory context, but it may not ask topology to fill material Presence, expose support masks as continuous emitters, or modify the accepted topology/cache contract.

#### Sampling and mapping contract

- A same-resolution compute consumer should load the corresponding integer texel directly.
- Rendering, diagnostics, and differently mapped consumers should sample through the canonical Foam field mapping and physical river-space coordinates.
- Major Support, Connector Support, Anchored Support, and aggregate Negative Aging Pressure remain independent inputs.
- The destructive composition `Positive × (1 - Negative)` is forbidden as a material-facing pre-pass.
- `_FoamObstacleExclusion` is the canonical obstacle source. `_FoamTopology.a` is reserved zero and has no obstacle compatibility role.
- Valid-water decisions remain owned by the boundary/domain contract rather than inferred from topology support.

#### Replacement and binding contract

Only generated Major, Connector, and aggregate negative topology participate in the Patch 4.8 generated-topology transition. Pressure, Lee, Shore, exact Obstacle Footprint, boundary/valid-water state, and persistent material state remain live authorities outside that blend.

Normal readiness, direct cache installation, compatible stale-cache hold, replacement activation/crossfade, dimension-changing reinitialization, resource reallocation, disabled Foam, frozen/inactive rivers, component disable, and destruction must all leave a complete valid material binding set. Disabled or unavailable fields use neutral black fallbacks and safe mapping values; stale released textures are never part of the contract.

#### Acceptance gate

Pass only if the canonical Stage 6 architecture, topology plan, river roadmap, and progressive-initialization plan agree on:

- the exact resources and channels above;
- aggregate-only material-facing Negative Aging Pressure;
- retained internal subtype identity;
- ownership of cache, topology runtime, material lifecycle, and rendering;
- same-grid and mapped sampling rules;
- generated-only transition ownership;
- canonical Obstacle Footprint ownership;
- complete neutral-safe binding requirements;
- no visual, generator, cache-format, or serialized-data change in 4.10A.

### Patch 4.10B — Runtime hardening, obsolete-proof cleanup, and topology closure

**Implementation status:** complete and Unity-validated; topology generation closed.

#### Implemented changes

- added one canonical HLSL semantic sampling representation for Major Support, Connector Support, aggregate Negative Aging Pressure, Pressure Support, Lee Support, Shore Support, combined Anchored Support, canonical Obstacle Footprint, and valid fluid;
- preserved the frozen texture packing rather than adding a speculative negative-subtype texture;
- hardened renderer binding paths so previous/current material state, topology, sources, boundary, obstacle, mapping, interpolation, and material parameters are valid or neutral before resource release and after disable/shutdown;
- declared `_FoamObstacleExclusion` canonical. Patch 4.11C.5 later removes the obsolete `_FoamTopology.a` compatibility copy and reserves alpha as zero;
- removed the obsolete destructive `LegacyNetSupport` proof helper and only the diagnostics that depended exclusively upon it;
- preserved removed metric-buffer slots as reserved to avoid unrelated layout churn;
- updated aggregate-negative comments, Inspector labels, public diagnostic names, and the completed debug-gating TODO;
- retained cache telemetry, topology-class coverage, evolution/identity diagnostics, release-cache diagnostics, and the minimal views required by material integration.

#### Explicit exclusions

Patch 4.10B does not change:

- Major, Connector, Interior Pocket, Edge Cavity, Weak Span, or Free-Water generation;
- topology evolution, placement, lifetimes, population, shape, or authoring coefficients;
- cache codec, payload version, fingerprints, provider ownership, or existing cache assets;
- field-space mapping or structural resolution;
- replacement timing or crossfade shape;
- material aging, supply, birth, breakup, motion, death, or final rendering;
- accepted Stage 5 Pressure, Wake, Shore, Ripple, or disturbance behaviour.

#### Completion gate

Topology closes only when:

- existing persistent caches still direct-hit with zero expensive preparation;
- all topology and normal Final Foam views remain visually unchanged;
- settings- or obstacle-triggered replacement crossfades generated topology while live Pressure, Lee, Shore, Obstacle Footprint, boundary, and material bindings remain authoritative;
- disable/re-enable, freeze/thaw, inactive state, reallocation, component disable, and destruction produce no stale-frame, missing-resource, released-texture, or invalid-mapping binding;
- the strict release-cache validator still passes, and a deliberately unassigned or stale cache still fails without mutation;
- compute import and all kernel lookups remain clean;
- obsolete destructive-support diagnostics are gone while retained diagnostics remain sufficient;
- the four canonical documents record topology as complete and identify separate material-lifecycle work as the next milestone.

---

## 10. Test Matrix

Every relevant step should select from this matrix rather than inventing an unrelated test set.

### River geometry

- short straight river;
- longer straight river;
- broad bend;
- tight bend;
- narrow-to-wide transition;
- asymmetric width;
- constriction;
- multi-chunk river where available.

### Obstacles and anchored context

- no obstacle;
- one small obstacle;
- one broad obstacle;
- several obstacles;
- Pressure-dominant context;
- Lee-dominant context;
- Shore-dominant context;
- mixed anchored support.

### Authoring

- low, medium, high Major Amount;
- low, medium, high Major Size and Size Variation;
- low, medium, high Connector Amount, Directness, and Length Preference;
- `0`, `0.5`, and `1` for Interior Pocket Amount;
- `0`, `0.5`, and `1` for Edge Cavity Amount;
- `0`, `0.5`, and `1` for Connector Weak Span Amount;
- `0`, `0.5`, and `1` for Free-Water Event Amount;
- each negative class isolated;
- several ordinary seeds;
- repeated identical seed;
- reverse flow;
- low, medium, high structural quality.

### Runtime state

- cold Play entry after compute change;
- warm Play entry;
- live settings rebuild;
- river-domain rebuild;
- obstacle rebuild;
- repeated dirty changes;
- sleeping chunk;
- frozen river;
- long observation window for movement and recycling.

---

## 11. Failure Diagnosis Order

When topology fails, diagnose in this order:

1. **Candidate generation**
   - Are individual Major shapes valid before river placement?

2. **Source context**
   - Are valid water, metric rows, obstacles, and anchored inputs correct?

3. **Major placement**
   - Are valid candidates rejected, clipped, crowded, or distributed poorly?

4. **Connector rules**
   - Are endpoints absent, pair selection wrong, or traversal costs invalid?

5. **Negative Aging Pressure rules**
   - Are Interior Pocket or Edge Cavity hosts invalid, distance fields wrong, or protection masks excessive?
   - Are Weak Spans associated with the wrong Connector or too close to endpoint gates?
   - Are Free-Water opportunities too dense, outside valid water, or blocked by exact obstacle/anchored-core rules?

6. **Composition/upload**
   - Are correct CPU/generated fields packed or sampled incorrectly?

7. **Runtime evolution**
   - Are identities, phase, direction, fading, or recycling incorrect?

8. **Cache/rebuild**
   - Is stale data loaded, mapping mismatched, or crossfade incomplete?

Do not skip directly to another shape representation because the combined view looks bad. Use the smallest inspection surface that can isolate the failure.

---

## 12. Regression Prevention Rules

Future topology patches must answer:

1. What topology data is created or transformed?
2. Where is that exact result immediately visible?
3. Is the result candidate-local or river-dependent?
4. Which existing diagnostic can be reused?
5. What is the minimum telemetry needed to explain failure?
6. Which stale code becomes removable in this patch?
7. Does the patch add active-gameplay generation or search work?
8. Does it add a cold compute kernel?
9. Does it preserve deterministic identity?
10. Does every ordinary free-water hop preserve positive net downstream movement while allowing bounded lateral/diagonal displacement?
11. Does it leave Anchored Support attached to live sources?
12. What is the direct rollback file set?

A topology patch that cannot answer these questions is not ready.

---

## 12.1 Patch 4.11C.5 Material-Integration Correction

Patch 4.11C.5 changes no topology generator, cache payload, identity, evolution rule, or replacement transition. It removes obsolete material-side behaviour that had violated the topology ownership boundary:

- topology and shore fields no longer steer persistent material;
- there is no procedural material-guidance network;
- positive support and Negative Aging Pressure modify Remaining Life only;
- `_FoamTopology.a` is written as zero and never copied into obstacle validity;
- exact `_FoamObstacleExclusion` remains the sole solid mask;
- the combined `Foam + Aging Topology` view displays green positive support, red negative pressure, yellow overlap, blue obstacles, and the exact cyan/white final Foam mask.

This clarification is material integration only. Topology generation remains closed.

---

## 13. Immediate Next Step

Topology generation remains complete and closed after Patch 4.10B.

Material-owned **Patch 4.11C.3 — Source Quantity and Birth-Merge Correction** is Unity-validated. **Patch 4.11C.4 — Persistent Material-State Migration** completed the atomic channel migration but failed visual acceptance because obsolete material transport overrode the source footprint and visible lifetime. **Patch 4.11C.5 — Material Footprint Preservation and Unified Lifecycle Diagnostics** removes those obsolete systems and has passed the major lifetime validation gate. **Patch 4.11C.5.1 — Material Flow Speed and Visual Residue Cleanup** adds material-speed control and suppresses low-coverage transport crumbs without changing topology ownership. **Patch 4.11C.5.2 — Transport Temporal Continuity** raises the material cadence and adds timing diagnostics without changing topology ownership. C.6 lifetime authority/presentation and C.7 closure remain blocked.

These patches may sample the accepted topology outputs but may not:

- add or change a topology generator;
- change cache payloads, fingerprints, or versions;
- reinterpret topology identities as continuous emitters;
- collapse positive support and Negative Aging Pressure;
- write or treat `_FoamTopology.a` as obstacle data; alpha is reserved zero;
- change topology evolution or replacement transitions.

Detailed implementation requirements belong to `River_Foam_Material_State_Correction_Implementation_Plan.md`. Patch 4.11D remains blocked until C.7 is accepted.

---

# Patch 4.11C.5.2b Note — Debug Layer Reorganization

C.5.2b reorganizes the Foam Inspector diagnostics into named explanatory foldouts instead of one flat topology/runtime list. This is a validation-workflow correction only. It does not change Foam simulation, transport, lifetime, topology generation, source birth, residue handling, or rendering behaviour.

The current recovery sequence remains:

1. transport temporal continuity;
2. residue suppression and shape conservation;
3. topology aging proof and interaction calibration;
4. controlled lateral drift and obstacle tangential flow.

Transport debugging now begins in the `Transport / Motion` foldout, where material step duration, steps last frame, render interpolation alpha, estimated cells per step, transport substeps, compression passes, and material flow speed are visible near the top of the Foam Inspector.
