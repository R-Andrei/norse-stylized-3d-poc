# River Foam Topology Implementation Plan

## Document Status

**Status:** Canonical step-by-step implementation plan for Stage 6 Foam topology only.

**Patch status:** Patch 0 — documentation baseline.

**Primary implementation target:**

- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.cs`

**Primary compute target:**

- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`

**Likely topology source boundary:**

- `Game/Procedural/Rivers/FoamTopology/`

**Related documents:**

- `Docs/River_Foam_Stage6_Architecture.md` — owns the full Stage 6 behavioural and visual contract.
- `Docs/River_Progressive_Initialization_and_Work_Scheduling_Plan.md` — owns initialization, rebuild scheduling, profiling, and performance safeguards.
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

1. expensive topology generation during an editor bake, loading/preparation window, or equivalent controlled phase;
2. cached topology fields and compact identity/evolution metadata;
3. cheap runtime sampling, downstream movement, fading, and crossfading;
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
- Pocket Aging Pressure generation;
- static topology composition;
- topology identity and evolution metadata;
- strictly downstream runtime topology movement;
- cheap per-layer fading and evolution;
- safe topology rebuild crossfades;
- topology cache/precompute packaging;
- topology diagnostics and topology-specific telemetry;
- cleanup of superseded topology implementations.

This plan does not cover:

- Foam material spawning or replenishment;
- Amount, Freshness, Integrity, or other persistent material-state equations;
- topology-to-material lifespan response;
- fragmentation or dissipation of Foam material;
- final Foam shading or colour;
- Impact Ripple-to-Foam integration;
- reopening accepted Stage 5 Pressure, Wake, or Impact Ripple behaviour;
- unrelated river optimization work beyond preserving the accepted scheduling and profiling contract.

---

## 3. Canonical Topology Outputs

The topology pipeline produces three independent logical outputs:

1. **Major Support**
   - broad positive lifespan-support regions;
   - connected and filled;
   - varied at low and medium spatial frequencies;
   - distributed using actual river context.

2. **Connector Support**
   - sparse relational positive support;
   - generated between meaningful positive regions or approved anchored endpoints;
   - subordinate to Major and Anchored Support.

3. **Pocket Aging Pressure**
   - negative lifespan influence hosted primarily by broad Major interiors;
   - preserves a positive rim;
   - does not directly subtract topology or material.

The pipeline also preserves live non-generated context:

- Pressure Support;
- Lee Support;
- Shore Support;
- Obstacle Footprint;
- valid-water coverage.

Anchored Support remains attached to its authoritative live source. It is not baked into the free-water drift field in a way that causes it to detach from banks or objects.

---

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
- deterministic `Amount`, `Size`, and `Seed` authoring contracts where they remain meaningful.

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
- Pocket Aging Pressure / Negative Influence;
- combined topology;
- accepted Anchored Support and Obstacle Footprint views.

A temporary overlay may show items such as connector endpoints, one attempted path, or selected pocket centres while that step is being implemented. Such overlays are implementation aids, not promised permanent authoring tools.

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
- accepted pockets;
- most common rejection reason if useful;
- generation time.

The normal `Final Foam` view must not perform diagnostic readbacks or diagnostic-grade work merely because telemetry exists.

---

## 6. Determinism and Identity Contract

Every generated free-water topology region requires stable identity from the start, even before movement is implemented.

A compact region record should preserve enough information for later cache and runtime evolution, including as applicable:

- layer class: Major, Connector, or Pocket;
- stable region or opportunity identity;
- base field or mask identity;
- river-space bounds;
- generation seed or deterministic sub-seed;
- downstream drift speed;
- phase offset;
- fade-in and fade-out envelope;
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
- the broader documents no longer imply that Major, Connector, and Pocket must be built invisibly as one large patch.

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
- no Pocket Aging Pressure;
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

Generate sparse relational support only after Major Support and Anchored Support provide meaningful endpoints.

### Exact behaviour change

Implement:

- endpoint extraction from accepted Major regions and approved anchored-support context;
- endpoint clustering or deduplication;
- bounded candidate-pair selection;
- a metric-aware traversal cost field;
- bounded A* or Dijkstra-style path search, or another explicitly approved bounded equivalent;
- obstacle and invalid-water blocking;
- penalties for unsupported open water and implausibly long paths;
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
- no Pocket generation;
- no material response;
- no runtime connector fading yet.

### Test

Test:

- two nearby Major regions with a plausible gap;
- two regions separated by an obstacle;
- regions too far apart;
- Major-to-approved anchored support;
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
- missing connectors are explainable by endpoint or path telemetry;
- searches are bounded and occur only in preparation/generation;
- connector identity is stable for unchanged inputs.

### Rollback

Remove Connector generation, metadata, and temporary overlays while retaining accepted Major topology.

---

## Step 4 — Pocket Aging Pressure

### Purpose

Generate hosted negative topology only after the positive topology is accepted.

### Exact behaviour change

Implement:

- interior distance analysis of accepted Major Support;
- eligible broad-host detection;
- deterministic spaced local maxima selection;
- protection of Major rims;
- protection of important Connector cores;
- protection of Anchored Support and obstacle context;
- irregular soft Pocket mask generation;
- stable Pocket host identity and evolution metadata.

Pocket Aging Pressure remains independent negative influence. It does not destructively subtract Major Support.

### Immediate inspection

Show Pocket Aging Pressure on the actual river through the existing negative-influence diagnostic.

A temporary selected-centre overlay may be used while tuning host selection.

Permanent telemetry is limited to:

- eligible Major hosts;
- candidate centres;
- accepted pockets;
- top rejection reason if needed;
- generation time.

### Explicit exclusions

- no instant geometric hole cutting;
- no pockets in unrelated neutral water;
- no material aging response;
- no Pocket movement or respawn yet;
- no extra negative topology class.

### Test

Test:

- broad Major hosts;
- narrow Major hosts;
- Major regions crossed by Connectors;
- Major near Pressure, Lee, Shore, and obstacles;
- low and high Major coverage;
- multiple seeds;
- repeated identical generation.

### Acceptance gate

Pass only if:

- pockets substantially overlap valid broad Major interiors;
- a positive rim remains;
- narrow hosts are rejected;
- important Connector and Anchored Support cores are protected;
- neutral water does not receive unrelated random pockets;
- Pocket identity is deterministic;
- the Pocket field remains separately available from positive support.

### Rollback

Remove Pocket generation, metadata, and temporary overlays while retaining accepted Major and Connector topology.

---

## Step 5 — Static Combined Topology Validation

### Purpose

Validate the complete static topology relationship without introducing material behaviour or runtime movement.

### Exact behaviour change

Compose and upload:

- Major Support;
- Connector Support;
- Pocket Aging Pressure;
- live Pressure Support;
- live Lee Support;
- live Shore Support;
- Obstacle Footprint;
- valid-water constraints.

Preserve independent positive and negative channels. Do not use destructive positive-times-one-minus-negative composition.

All generated layers retain stable identity and their future evolution metadata even though phase remains static in this step.

### Immediate inspection

Use the existing on-river views:

- Support Classes;
- Negative Influence Classes;
- Support and Negative Influence;
- Anchored Support;
- Obstacle Footprint.

No new generic combined debugger is added.

### Test

Run the complete static matrix:

- several river shapes and widths;
- no, few, and many obstacles;
- low/medium/high Amount and Size;
- multiple seeds;
- reverse flow;
- quality tiers;
- cold and warm Play entry;
- queued source/domain rebuilds;
- repeated generation with identical inputs.

### Acceptance gate

Pass only if:

- Major, Connector, and Pocket each pass their own gates;
- the combined composition preserves substantial neutral water;
- anchored support remains attached to authoritative sources;
- obstacle and valid-water constraints remain correct;
- positive and negative overlap remains visible and non-destructive;
- no layer hides another layer’s failure;
- generation costs are named and profiled;
- no ordinary ready-state frame regenerates topology.

### Rollback

Restore the previous composition/upload integration while retaining separately accepted generators for diagnosis.

---

## Step 6 — Major Runtime Downstream Evolution

### Purpose

Prove cheap runtime topology evolution using the same stable Major identity, without rerunning expensive generation.

### Exact behaviour change

Implement Major evolution through cheap operations such as:

- downstream coordinate offsets when sampling generated fields or cached masks;
- low-rate field advection where justified;
- per-region or per-mask phase offsets;
- gradual fade-in and fade-out;
- gradual strengthening and weakening;
- strictly downstream recycling outside the visible domain.

Movement must remain positive downstream independently of river direction storage conventions. Reverse-flow rivers must still move in their canonical downstream direction.

Major regions may drift at independent bounded speeds. They must not move as one synchronized conveyor.

No accepted Major region may pop fully in or out. Entry, exit, replacement, and recycling require a fade envelope or a later material-lifecycle response.

### Immediate inspection

Show Major Support moving on the actual river.

The only required runtime telemetry is:

- active evolving Major region count;
- minimum and maximum resolved downstream speed;
- recycle count during the current observation window;
- any detected upstream displacement violation.

### Explicit exclusions

- no Connector evolution;
- no Pocket evolution;
- no regeneration, cleanup, component analysis, or rejection during gameplay;
- no whole-field upstream wrap;
- no movement of Anchored Support.

### Test

Observe:

- ordinary forward flow;
- reverse flow;
- slow and fast Foam speed;
- short and long rivers;
- regions entering and leaving visibility;
- chunk sleeping/freezing;
- at least 60 seconds of continuous movement;
- deterministic replay from identical initial state where practical.

### Acceptance gate

Pass only if:

- every free-water displacement is downstream;
- regions move at independently paced but bounded rates;
- no synchronized conveyor dominates;
- recycling occurs only outside the visible domain;
- no region pops;
- Anchored Support remains fixed to live sources;
- no expensive generation operation appears in ordinary gameplay profiling.

### Rollback

Disable Major runtime phase evaluation and return to the accepted static generated fields without changing their cached identity.

---

## Step 7 — Connector and Pocket Runtime Evolution

### Purpose

Add layer-specific cheap evolution without treating all topology classes as the same moving mask.

### Exact behaviour change

Connector Support may:

- drift downstream at a distinct rate;
- weaken and fade more readily than Major;
- crossfade between cached/generated relationship states;
- disappear only through a gradual envelope;
- reconnect only through precomputed alternatives or an explicit preparation rebuild, not gameplay pathfinding.

Pocket Aging Pressure may:

- drift within its host bounds;
- use its own phase and speed;
- grow, weaken, or fade;
- respawn only after the prior pocket has faded;
- remain associated with a valid host identity;
- avoid becoming an unrelated negative mask in neutral water.

### Immediate inspection

Inspect Connector and Pocket evolution separately on the actual river using their existing class views.

The required telemetry is limited to:

- active evolving Connector count;
- active evolving Pocket count;
- fade/replacement count;
- host-loss or invalid-relationship count;
- any upstream displacement violation.

### Acceptance gate

Pass only if:

- Connector remains subordinate and more fragile than Major;
- Pocket remains hosted by valid positive structure;
- all free-water movement is downstream;
- no layer pops;
- no gameplay pathfinding, component cleanup, or distance transform occurs;
- Anchored Support remains live and stationary relative to its source.

### Rollback

Return Connector and Pocket to their accepted static fields while retaining Major evolution.

---

## Step 8 — Safe Topology Rebuild and Crossfade

### Purpose

Replace topology after an explicit source/domain/settings rebuild without visible whole-field popping.

### Exact behaviour change

When a rebuild is required:

1. preserve the currently active accepted topology;
2. generate the replacement through staged preparation;
3. validate and upload the replacement only after complete readiness;
4. crossfade old and new generated topology over a bounded interval;
5. keep Anchored Support live from its authoritative sources throughout;
6. release old generated data only after the crossfade completes.

Rapid repeated dirty events must coalesce. A later change may invalidate an in-progress replacement, but it must not force synchronous completion.

### Immediate inspection

Trigger controlled changes to Seed, Amount, Size, river domain, and obstacle context while viewing the topology on the river.

Required telemetry:

- rebuild state;
- old/new topology readiness;
- crossfade progress;
- coalesced invalidation count;
- cancelled replacement count.

### Acceptance gate

Pass only if:

- the current topology remains valid while replacement prepares;
- no whole topology field flashes to neutral;
- no synchronous rebuild path returns;
- repeated changes coalesce;
- old and new topology crossfade safely;
- source-attached support remains authoritative.

### Rollback

Disable replacement crossfade and restore the accepted queued static-rebuild behaviour while preserving the last valid topology.

---

## Step 9 — Production Cache and Precompute Packaging

### Purpose

Move expensive accepted generation out of ordinary Play startup and active gameplay.

### Initial packaging decision

The first production cache should store the integrated per-river or per-run topology result rather than prematurely building a large generic shape library.

The cache should contain as applicable:

- Major, Connector, and Pocket fields or masks;
- compact region identities;
- per-layer evolution metadata;
- river-space mapping and dimensions;
- generator version;
- river/domain version or checksum;
- obstacle/source checksum;
- authored settings and seed;
- diagnostics needed to detect stale or invalid data.

A reusable shape library or hybrid descriptor system remains a later optimization only if memory, variation, or movement requirements justify it.

### Exact behaviour change

Implement:

- deterministic cache serialization;
- stale-cache detection;
- load and validation during preparation;
- neutral failure behaviour when no valid cache exists;
- explicit development regeneration path;
- equivalence checks between freshly generated and loaded topology;
- production avoidance of candidate generation and relational searches during gameplay.

### Immediate inspection

Load the cached topology and inspect the same river diagnostics used for generated topology.

Required telemetry:

- cache hit/miss/stale reason;
- load time;
- generated-versus-loaded checksum;
- cache memory size;
- whether any expensive generation path executed.

### Acceptance gate

Pass only if:

- valid cached output matches the accepted generated output within the defined representation tolerance;
- runtime evolution preserves the same identities and phases;
- stale inputs invalidate the cache deterministically;
- ordinary gameplay performs no expensive topology generation;
- cache loading does not reintroduce a startup freeze;
- development regeneration remains explicit and inspectable.

### Rollback

Return to the accepted staged proof-generation path while retaining the cache format files for diagnosis.

---

## Step 10 — Topology Completion and Handoff

### Purpose

Declare the topology pipeline stable and expose a clean contract to later Foam-material work without implementing that work here.

### Required final outputs

- accepted Major Support field;
- accepted Connector Support field;
- accepted Pocket Aging Pressure field;
- accepted Anchored Support inputs;
- accepted Obstacle Footprint and valid-water constraints;
- compact stable identity and evolution metadata;
- safe static and evolving sampling contract;
- safe rebuild/crossfade contract;
- production cache/precompute contract;
- retained minimal diagnostics and telemetry;
- no superseded topology implementation remaining.

### Handoff contract

Later Foam-material work may sample:

- positive support classes independently or combined;
- Pocket Aging Pressure independently;
- Anchored Support independently;
- topology motion/evolution state where required.

Topology does not itself:

- spawn Foam material;
- erase Foam material;
- determine final colour or opacity;
- implement fragmentation;
- implement dissipation;
- own material lifetime.

### Completion gate

Topology is complete only when:

- Steps 1–9 pass;
- static and evolving topology are visually accepted on the real river;
- all stale topology code has been removed or explicitly justified;
- ordinary gameplay contains no expensive generator/search/cleanup work;
- cached topology and staged rebuilds are deterministic and inspectable;
- the topology output contract is stable enough for the separate material-lifecycle implementation.

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

- low, medium, high Amount;
- low, medium, high Size;
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

5. **Pocket rules**
   - Are hosts too narrow, distance fields wrong, or protection masks excessive?

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
10. Does it preserve strictly downstream free-water movement?
11. Does it leave Anchored Support attached to live sources?
12. What is the direct rollback file set?

A topology patch that cannot answer these questions is not ready.

---

## 13. Immediate Next Step

After Patch 0 is accepted, proceed with **Step 1 — Major Candidate Vertical Slice and Obsolete-Path Removal**.

That patch must deliver, together:

- complete reference-driven removal of obsolete topology code made unnecessary by the new candidate path;
- one deterministic field-first Major candidate generator;
- the compact four-stage candidate preview;
- the five essential candidate diagnostics;
- bounded retries and rejection reasons;
- no whole-river placement, Connector, Pocket, material response, movement, or cache work.

The purpose is to prove the core shape-generation method immediately. If the candidate family is wrong, the project stops there and changes the generator before any river distribution, relationship, or runtime-evolution work is built on top of it.
