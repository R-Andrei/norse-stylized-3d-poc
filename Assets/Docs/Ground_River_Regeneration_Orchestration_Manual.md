# Ground–River Regeneration Orchestration Manual

## Document status

- **Status:** Active implementation manual. GR-O1 diagnostics and GR-O3A Play-startup coalescing are Unity-validated and accepted. The Painted Accent persistent-production boundary described below is also accepted.
- **Scope:** Cross-feature GeneratedGround and StylizedRiver lifecycle, invalidation, build ordering, and future ground-dependent feature integration.
- **Audited baseline:** Live `fufu` working tree at Git HEAD `04dbc13` on 2026-07-13, including the uncommitted Ground and River changes present during the audit.
- **Primary implementation sequence:** Candidate 1 is accepted. The user explicitly authorized the bounded GR-O3A Play-startup coalescing slice before Candidate 2 because captured telemetry isolated one safe high-impact lifecycle wave. Candidate 2 remains available if broader transaction decomposition is later required. Candidates 3B/4 remain measurement-gated. Candidate 5 is optional after exact invalidation is accepted.
- **Supersession:** This document does not supersede Ground visual doctrine, River rendering architecture, or River Foam ownership. It governs only the cross-feature processing contract described here.

## Authoritative companion documents

Read these before implementing any candidate:

1. `Assets/AGENTS.md`
2. `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
3. `Assets/Docs/Ground_Generation_Surface_Upgrade_Plan.md`
4. `Assets/Docs/Ground_River_Coupled_Surface_Response_Architecture.md`
5. `Assets/Docs/River_Rendering_Roadmap.md`
6. `Assets/Docs/River_Foam_Stage6_Architecture.md`
7. `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
8. `Assets/Docs/Proof of Concept/09_Rock_And_River_Handoff.md`

If this manual conflicts with a subsystem invariant in one of those documents, the subsystem invariant remains authoritative until the conflict is explicitly reviewed and accepted.

## Purpose

GeneratedGround and StylizedRiver currently have a valid data relationship but an invalid processing relationship. Ground requires river intent to conceal broad terrain and write shore metadata. The visible river corridor requires the completed pre-river ground surface to construct its handoff. Ground features may also need final ground sampling and semantic river-bank data.

Those requirements are not inherently circular. The current lifecycle makes them appear circular because river-domain preparation, visible river output generation, Ground regeneration, runtime invalidation, and lifecycle restoration are combined into broad synchronous calls.

This manual defines a staged migration to:

- one explicit Ground transaction owner per Ground patch;
- one authoritative River Domain writer per river;
- exact output-specific fingerprints rather than whole-component serialization;
- one Ground mesh upload and collider cook per legitimate structural transaction;
- post-Ground river realization;
- bank-aware Ground feature generation without recursive regeneration;
- precise River runtime notifications that preserve Foam and disturbance ownership;
- coalesced editor lifecycle work without weakening synchronous readiness where it is required;
- a future contributor/consumer contract that does not become a global arbitrary dependency graph.

## V3S River-Coupled Ground Response boundary — updated 2026-07-16

V3S is direct Ground-shader interpretation of River-owned corridor semantics. The River corridor publishes `UV2.y` shore/waterline influence and packed UV3 values: X = Riverbed Support, Y = outward bank distance from that support boundary, and Z = corridor-bank validity. Ordinary GeneratedGround writes zero to `UV2.y` and publishes no River-coupled UV3 stream. GeneratedGround still owns all appearance controls and applies the resolved material property block to both renderers, but every application must pass an explicit role: `OrdinaryGround` writes `_GroundRiverCoupledEnabled = 0`, while `RiverCorridor` writes `1`. Material-only refresh must preserve those roles; Bank Surface Layer composition is authorized only on the corridor draw.

```text
River structural authoring
→ rebuild River Domain/corridor geometry when structurally required
→ publish current shore/waterline, Riverbed Support, and corridor-bank distance/domain channels

Ground material authoring
→ refresh Ground renderer property block with OrdinaryGround role
→ refresh River corridor renderer property block with RiverCorridor role
→ no Ground geometry or River structural rebuild
```

V3S adds no generated coverage texture, no River snapshot scan, and no post-Ground contour pass. River foam, transport, pressure, wakes, disturbances, and water optics remain separate owners.

A3B implements one Ground-owned `GroundHydrologyModifierProfile` selection and Material-only Shore wetness controls. Hydrology refresh follows the same role-aware property-block path as Bank composition: it updates ordinary Ground and all corridor renderers without rebuilding Ground geometry, River Domain, corridor geometry, collider data, Painted Accent coverage, foam, or water runtime state. The local wetness mask reads existing corridor Shore, bank distance, and bank-domain data only; it is independent from Bank Surface Layer reach and requires no River notification beyond the existing material refresh.

A4A reached Unity and proved exact Riverbed Support, custom dry Riverbed profile transport, and submerged-cover exclusion. A4A.1 and A4B are implemented Material-only corrections: normalized primary/Bank/Riverbed composition, explicit Riverbed source ownership, and exact-support Riverbed hydrology. Editing any Bank/Riverbed substrate source, custom profile, hydrology source, modifier, or strength refreshes existing ordinary-Ground and River-corridor property blocks only. It must not regenerate Ground geometry, River Domain, corridor geometry, colliders, Painted Accent coverage, foam, water state, or any generated hydrology field. A4B reads existing Riverbed Support in the shader and adds no River data or rebuild dependency.

## Corrected V4 Contact / Edge Accent consumer boundary — 2026-07-15

V4 begins after V3S and consumes only explicitly participating GroundModifier snapshots plus eligible GeneratedMass geometry. It has no River snapshot input and no River invalidation dependency.

```text
Ground snapshot + explicit modifier snapshots + eligible GeneratedMass geometry
→ editor-time Contact source snapshots
→ Ground-local Contact coverage
→ persistent Ground-owned production texture
```

River spline, width, corridor geometry, shore mask, Riverbed Support, water material, foam, or disturbance changes do not stale Contact coverage. Contact material-only changes do not invalidate Ground geometry, River Domain, River corridor geometry, Painted Accent placement, or River runtime state. Player runtime binds persistent Contact output only and performs no source evaluation.

The detailed V3S and V4 contracts are recorded in `Ground_River_Coupled_Surface_Response_Architecture.md` and `Ground_Contact_Edge_Accent_Audit_and_Architecture.md`.

## Accepted Painted Accent production boundary — 2026-07-15

GeneratedGround remains the owner of Painted Accent authoring, Edit Mode procedural preview, persistent production coverage, material binding, validation metadata, and generated-output lifecycle.

The processing boundary is:

```text
Edit Mode authoring transaction
→ build or reuse SurfaceStrokes
→ build or reuse ProjectedGlyphs and companion clusters
→ rasterize transient authoritative preview coverage
→ optionally persist through the explicit Bake Painted Accents action

Play Mode and Player transaction
→ validate the serialized persistent R8 artifact structurally
→ bind persistent coverage and stored local-XZ mapping
→ render
```

Play Mode and Player must not execute:

```text
Painted Accent SurfaceStroke generation
ProjectedGlyph generation
pair/triplet cluster solving
Painted Accent-specific River exclusion snapshot construction
coverage rasterization
procedural coverage texture creation or CPU upload
```

Pre-build validation is the exact stale-output authority. It may regenerate authoritative coverage in isolated Editor preview scenes solely to compare against persistent production data, then closes those scenes without saving. It blocks Missing, Stale, Incompatible, duplicate, shared, or ownership-mismatched output and never rebakes automatically.

Generated-output cleanup is a separate explicit Editor maintenance transaction. The all-project audit inspects loaded and saved scenes, non-build scenes, prefabs, and other asset dependencies. It may delete only reviewed assets classified as **Confirmed orphan**. It never saves scenes or prefabs and never treats exclusion from the active build profile as proof of disuse.

Material-only Painted Accent changes—Ink Colour and Ink Opacity—do not rebuild procedural stages and do not stale persistent coverage. Placement, shape, geometry, modifier, River exclusion, projection, cluster, or raster changes invalidate the preview/bake as required.

## Proven audited-baseline problem

Before the accepted GR-O3A Play-startup coalescing slice, the restoration path could execute as follows:

```text
GeneratedGround.OnEnable
  -> GeneratedGround.Regenerate
    -> StylizedRiver.CreateGroundSnapshot
      -> EnsureRiverDomain
        -> BuildRiverDomain
    -> Ground generation / mesh / collider / features
    -> StylizedRiver.RebuildCorridorFromGround

StylizedRiver.OnEnable
  -> StylizedRiver.RegenerateAll
    -> BuildRiverDomain again
    -> BuildSurface
    -> NotifyParentGround
      -> GeneratedGround.NotifyRiverChanged
        -> GeneratedGround.Regenerate again
```

The second Ground request is not always a second expensive geometry pass because existing Ground stage signatures can reject identical work. The duplicate request and duplicate ownership are nevertheless proven. The second request can become expensive when validation, migration, spline state, component state, or an over-broad River signature changes between calls.

GroundModifier has the same scaling pattern: its lifecycle notifies GeneratedGround independently. More future features using this approach would multiply requests even when most outputs are unchanged.

The current Ground River signature has two independent defects:

1. It hashes the entire serialized StylizedRiver component in the Editor, allowing unrelated material, Foam, disturbance, reflection, or debug state to invalidate Ground geometry.
2. It does not hash the contents of the immutable River ground snapshot. Spline knots live in the referenced SplineContainer, not inline in StylizedRiver serialization, so a spline-domain change can be missed by the Ground geometry signature.

## Non-negotiable invariants

Every candidate must preserve these rules.

### Ground invariants

- GeneratedGround remains the owner of its mesh, collider, GroundHeightFieldSnapshot, Ground surface metadata, Painted Accent products, coverage texture, and material application.
- Existing Ground vertex budgets, coverage resolution, visual density, deterministic placement, and accepted style behavior remain unchanged.
- Ground material or debug changes must not rebuild geometry, colliders, Painted Accent placement, projection, coverage, or River geometry.
- Ground geometry is uploaded once per accepted structural transaction.
- MeshCollider is not recooked when the committed Ground geometry is unchanged.
- Ground feature consumers do not mutate the committed base Ground surface unless they are explicitly classified as structural influence providers.
- No per-frame full-field or full-Ground rebuild is introduced.

### River invariants

- StylizedRiver remains the sole writer and owner of RiverDomainSnapshot.
- River Domain remains the authoritative coordinate contract for surface geometry, corridor geometry, projection, motion, disturbances, Foam, and rendering.
- Surface mesh, corridor render mesh, corridor collider, material properties, reflection invalidation, disturbance bindings, and Foam bindings preserve their current accepted behavior.
- The dedicated corridor continues to sample the immutable pre-river Ground surface at the terrain handoff; it must not sample the concealed trench as its source terrain.
- Natural channel variation remains shared across water surface, corridor, collider, Ground concealment, and spatial queries.
- Standalone rivers without a GeneratedGround parent continue to generate valid fallback output.
- Existing public authoring actions remain available through compatibility wrappers until a separately approved cleanup removes them.

### River Foam and disturbance invariants

- River Foam persistent material may be moved only by its persistent-state transport stage.
- Visual Foam evaluation must not mutate persistent material or Remaining Life.
- Ordinary Play startup remains cache-only for topology. No Ground or River lifecycle transaction may generate, persist, retry, or save Foam topology.
- An unchanged River Domain must not be reported as structurally changed merely because the component enabled or Ground rebuilt.
- A genuinely changed River coordinate mapping must invalidate every runtime product that depends on that mapping.
- Ground-only or corridor-only changes must not silently masquerade as River Domain changes.
- Runtime resources are invalidated by the narrowest correct event.

### Repository and implementation invariants

- Candidates are implemented and validated one at a time.
- No candidate silently absorbs another candidate's behavior changes.
- No raw scene, prefab, material, or cache-asset edits are part of these candidates unless separately approved and proven necessary.
- Existing generated Mesh objects should retain identity where practical; rebuild their contents rather than replacing references.
- New files, interfaces, components, or architectural layers require explicit approval in the candidate scope.

## Target processing model

The long-term transaction is:

```text
0. Collect and normalize authoring state
   -> migrations, validation, exact input fingerprints

1. Prepare authoritative River Domain
   -> River-owned immutable RiverDomainSnapshot
   -> no visible mesh generation

2. Build base Ground field in memory
   -> recipe + structural Ground modifiers
   -> immutable pre-river Ground surface

3. Resolve Ground influences
   -> River ground-influence snapshots
   -> future structural influence providers
   -> no Unity mesh upload

4. Finalize Ground exactly once
   -> concealment + semantic masks + mesh upload + collider
   -> publish immutable GroundBuildContext

5. Build Ground-dependent visual features
   -> Painted Accents and future bank-aware consumers

6. Realize visible River outputs
   -> surface + corridor + corridor collider
   -> material + reflection request

7. Publish precise runtime changes
   -> Domain, boundary, geometry, visual, Foam, disturbance
```

The transaction is acyclic:

```text
Ground authoring -> Base Ground Surface

River authoring -> River Domain

Base Ground Surface + River Domain
  -> River Ground Influence

Base Ground Surface + River Ground Influence
  -> Final Ground + Ground Build Context

Ground Build Context + River Domain
  -> River Surface / Corridor / Collider

Ground Build Context + semantic influences
  -> bank-aware Ground features
```

River Domain preparation is not visible River generation. It is immutable structural input preparation. Visible River realization remains after Ground is established.

## Target data contracts

The exact types and filenames are not approved by this manual. The following conceptual contracts define required responsibilities.

### River domain fingerprint

The River domain fingerprint represents only inputs that change River-space coordinates or the resolved sampled domain:

- complete spline knot and tangent content;
- spline and River transforms relevant to world-space samples;
- requested domain sample spacing;
- width and resolved left/right visible and surface widths;
- shoreline overlap used by the domain;
- surface offset;
- connected distance offset;
- reverse-flow state;
- natural variation values that affect resolved domain samples.

It must not include water color, lighting, refraction, Foam appearance, disturbance tuning, reflection controls, debug views, or other output-independent state.

The fingerprint must be deterministic and content-based. It must not rely on ordinary object `GetHashCode`, transient Editor JSON, notification count, or object-reference identity alone.

### River Ground influence fingerprint

The River Ground influence fingerprint represents the exact data consumed by Ground:

- accepted River Domain fingerprint or exact sampled content;
- Ground-local point and side data;
- visible and hidden surface widths;
- bank blend;
- depth and bed flatness;
- bank profile;
- terrain conformity;
- Ground grid spacing where it changes handoff safety;
- wet clearance;
- bank cover;
- reserved downward surface displacement;
- any future Ground-affecting River parameter.

It excludes corridor tessellation quality, water material, Foam, disturbances, reflections, and visual debug state unless one of those settings genuinely changes Ground influence data.

### Ground Build Context

The immutable transaction result supplied to post-Ground consumers contains, conceptually:

- Ground owner identity;
- committed Ground geometry revision;
- immutable pre-river base surface;
- Ground transform, patch size, resolution, and grid spacing;
- committed modifier snapshots;
- committed River Ground influence snapshots;
- semantic River-bank queries or shore influence products;
- relevant surface-mask data;
- exact contributor revisions or fingerprints;
- validity state explaining whether Ground output is ready, missing, or failed.

Consumers must not retain mutable scratch buffers from the transaction. Cached production data must remain independently owned or immutable.

### Ground build request

A request describes why processing may be needed. It is not proof that a stage must execute.

Conceptual reasons include:

- Ground enabled or output missing;
- Ground recipe changed;
- Ground modifier added, removed, moved, enabled, disabled, or edited;
- River added, removed, moved, enabled, disabled, or structurally edited;
- spline changed;
- Ground feature recipe changed;
- explicit user regeneration;
- Undo/Redo or scene restoration;
- runtime structural change.

Requests may carry conservative dirty hints. Exact fingerprints and missing-output checks remain the final execution authority.

### River change set

Runtime consumers require narrow change categories:

- **Readiness changed:** component or output became available without changing structural content.
- **Domain changed:** River coordinate mapping or sampled domain content changed.
- **Ground influence changed:** data consumed by Ground changed.
- **Surface geometry changed:** water surface topology or displacement-clearance geometry changed.
- **Corridor geometry changed:** bank/corridor/collider output changed, including a Ground-revision dependency.
- **Boundary changed:** runtime shore/boundary representation changed without necessarily changing River Domain.
- **Visual properties changed:** material, lighting, ice, motion, refraction, or debug properties changed.
- **Disturbance configuration changed:** disturbance behavior changed without a coordinate remap.
- **Foam configuration changed:** Foam behavior or presentation changed without a coordinate remap.

One transaction may publish several categories. Consumers receive only categories they own.

## Invalidation matrix

| Change | Ground geometry | Ground feature products | River surface | River corridor/collider | River runtimes |
| --- | --- | --- | --- | --- | --- |
| Ground material or debug | no | no | no | material only if required | no |
| Painted Accent Ink Colour or Ink Opacity | no | material only; persistent bake remains current | no | no | no |
| Painted Accent shape | no | Edit preview projection/coverage; persistent bake becomes stale | no | no | no Player procedural work |
| Painted Accent placement | no | Edit preview placement/projection/coverage; persistent bake becomes stale | no | no | no Player procedural work |
| Ground recipe height/topology | yes | true dependants | normally reuse | yes | boundary update if required |
| Height modifier change | yes | true dependants | normally reuse | yes | boundary update if required |
| Pure feature exclusion modifier | no | affected features | no | no | no |
| River spline transform/knot edit | yes | bank-dependent features | yes | yes | Domain change |
| River width/domain variation | yes | bank-dependent features | yes | yes | Domain and boundary change |
| River depth/bank blend/conformity | yes | bank-dependent features | only if an actual surface input changed | yes | boundary/topology revalidation as required |
| River surface/corridor quality | no | no | affected mesh | affected mesh | resource quality if required |
| Water color/lighting/ice/debug | no | no | no geometry | no geometry | visual binding only |
| Flow/motion/refraction tuning | no | no | only if displacement clearance changes geometry | only when its safety input changes | motion/runtime binding |
| Foam authoring | no | no | no | no | Foam-owned update only |
| Disturbance authoring | no | no | only if maximum reserved displacement changes | only if safety geometry changes | disturbance-owned update |
| River disabled or destroyed | restore without River influence | rebuild affected bank features | disable/clear | disable/clear | unbind/release |

The matrix is a doctrine, not a substitute for exact signatures. A setting that feeds multiple outputs must participate in each relevant output fingerprint.

# Candidate 1 — Evidence, request accounting, and shadow fingerprints

## Objective

Prove the precise lifecycle request sequence and the correct invalidation boundaries without changing generation order, output ownership, event timing, mesh contents, collider behavior, or runtime behavior.

Candidate 1 is deliberately observational. It creates evidence required to safely authorize later behavior changes.

## Required scope

Expected existing-file scope is limited to the narrowest locations that own:

- GeneratedGround regeneration timing and Inspector diagnostics;
- StylizedRiver regeneration timing and Inspector diagnostics;
- River Domain and River Ground influence fingerprint calculation;
- request-reason capture at existing Ground/River entry points.

Any new diagnostics type or fingerprint helper file requires explicit scope approval before implementation. No scene, asset, shader, compute, Foam topology cache, or serialized default changes are part of Candidate 1.

## Implementation requirements

### 1. Ground request and pass accounting

Record, for each observable lifecycle batch:

- monotonically increasing request sequence;
- caller/reason category;
- frame or Editor update identity;
- whether the request occurred during an active Ground pass;
- whether it originated from Ground, River, modifier, Inspector, Undo/Redo, or explicit action;
- pass sequence actually executed;
- stages executed by that pass;
- total time per pass;
- accumulated wall time for the request batch;
- count of requests that produced no expensive stage;
- count of corridor callbacks;
- count of Ground mesh uploads and collider cooks.

Do not emit unbounded console logs. Use compact cached diagnostics and Profiler markers. One optional development summary per restoration batch is acceptable only if it is concise and can be disabled.

### 2. River request and output accounting

Record:

- `RegenerateAll` requests;
- delayed structural requests received and coalesced by the existing River debounce;
- Domain builds and Domain version increments;
- surface mesh builds;
- corridor mesh builds;
- corridor collider assignments;
- Ground notifications;
- `DomainChanged` broadcasts;
- Foam runtime notifications;
- disturbance runtime notifications;
- reflection requests.

The accounting must distinguish request count from actual output-build count.

### 3. Shadow River Domain fingerprint

Calculate a deterministic proposed Domain fingerprint from exact Domain inputs or exact resolved sample content. Store it only in nonserialized diagnostics state.

For each current `BuildRiverDomain` call, report whether:

- the legacy code rebuilt the Domain;
- the previous and next shadow fingerprints differ;
- the Domain version increment represented a true content change or an identical rebuild.

The shadow fingerprint must not gate behavior in Candidate 1.

### 4. Shadow River Ground influence fingerprint

Calculate the proposed fingerprint from the exact immutable Ground-influence snapshot.

Compare it with the current Ground River signature and record these divergence cases:

- legacy signature changed but exact influence did not;
- exact influence changed but legacy signature did not;
- both changed;
- neither changed.

This evidence specifically validates whole-component false invalidation and spline-content blind spots.

### 5. Preserve current semantics

Candidate 1 must not:

- suppress or defer any existing regeneration;
- change `Domain.Version` behavior;
- alter `DomainChanged` timing;
- alter Ground or River signatures used for execution;
- change renderer, collider, material, reflection, Foam, or disturbance behavior;
- add persistent scene or asset state for diagnostics;
- write diagnostic data into Foam topology caches.

## Candidate 1 validation matrix

Run and record at least:

1. Script compilation/domain reload with Ground and River enabled.
2. Enter Play Mode with the assigned exact Foam cache.
3. Exit Play Mode.
4. Disable and re-enable GeneratedGround.
5. Disable and re-enable StylizedRiver.
6. Move one spline knot without changing the River transform.
7. Move the entire River transform.
8. Change River width.
9. Change River bank blend or depth.
10. Change water color.
11. Change a Foam-only visual parameter.
12. Change a Ground material/debug setting.
13. Change a Ground shape setting.
14. Change Painted Accent placement and shape independently.
15. Move, enable, disable, add, and remove a GroundModifier.
16. Undo and redo representative Ground and River structural edits.

## Candidate 1 acceptance gate

Candidate 1 passes only when:

- the full restoration request sequence is visible without console spam;
- the first expensive pass cannot be hidden by a trailing cheap request;
- exact fingerprint divergence cases are reported clearly;
- spline edits are proven to change the exact Domain/Ground influence fingerprints;
- water, Foam, reflection, and debug-only edits are proven not to change the exact Ground influence fingerprint;
- mesh, collider, material, Foam, disturbance, and reflection behavior remain identical to the baseline;
- Play startup remains cache-only with zero topology generation or persistence;
- no new steady-state allocations or per-frame diagnostics work are introduced when diagnostics are idle.

## Candidate 1 rejection conditions

Reject the candidate if diagnostics themselves materially worsen restoration, produce continuous Inspector repaint work, mutate serialized state, save assets, create console floods, change Domain versioning, or affect generation decisions.

## Candidate 1 deliverable

The validation report must contain the measured request/pass timeline and an approved output-invalidation table. Candidate 2 may not begin until that table is accepted.

# Candidate 2 — Split River build responsibilities while preserving behavior

## Objective

Create explicit internal seams for River Domain preparation, Ground influence creation, visible geometry generation, material application, and runtime notification while preserving the current external lifecycle and exact output order.

Candidate 2 is a structural refactor, not the orchestration fix. It makes Candidate 3 possible without combining call-graph surgery and scheduling behavior in one patch.

## Required scope

Expected scope centers on:

- `StylizedRiver.cs`;
- the narrow River Editor actions that invoke public rebuild methods;
- diagnostics required to expose the split stages;
- existing River snapshot/domain types only where a read-only fingerprint or result contract is required.

Ground processing order, Ground lifecycle, Foam compute/shaders, disturbance simulation, topology-cache policy, scenes, materials, and accepted River geometry algorithms remain out of scope.

## Required internal stage separation

The exact method names are implementation decisions, but the following responsibilities must be independently callable and measurable.

### A. Normalize authoring and references

Owns:

- serialized migration;
- validation and clamping;
- component caching;
- spline-container resolution;
- output-object discovery/creation;
- Water layer assignment.

It must not build Domain, meshes, colliders, or runtime fields.

### B. Prepare River Domain

Owns:

- RiverDomainSnapshot construction;
- River length and average surface height derived from the Domain;
- Domain version increment under current Candidate 2 semantics;
- current `DomainChanged` publication under current Candidate 2 semantics.

Candidate 2 does not yet narrow Domain versioning or event publication. It only isolates the responsibility so Candidate 4 can change it safely.

### C. Create Ground influence

Owns:

- conversion from River Domain samples into Ground-local immutable influence data;
- visible/surface width arrays;
- bank and terrain-conformity parameters;
- bounds needed for Ground feature rejection;
- shadow exact Ground influence fingerprint.

It must not build visible River meshes or notify Ground by itself.

### D. Build surface geometry

Owns only the existing surface mesh algorithm and surface mesh content application.

Its inputs must remain exactly those currently used by `BuildSurface`, including Domain, cross-segment resolution, longitudinal spacing, and reserved downward motion safety.

### E. Build corridor geometry and collider

Owns only:

- corridor render mesh;
- corridor collider mesh;
- corridor collider assignment;
- corridor renderer state;
- corridor diagnostics and tight-bend warning.

It continues to consume GeneratedGround through the existing sampling contract in Candidate 2. GroundBuildContext is not introduced behaviorally until Candidate 3.

### F. Apply visual properties

Owns material property-block application and output renderer material state. It preserves the current neutral disturbance/Foam binding order so the runtime components can rebind in LateUpdate.

### G. Publish downstream changes

Owns the existing calls to:

- Ground notification;
- reflection request;
- Foam runtime notification;
- disturbance runtime notification where currently applicable.

Candidate 2 preserves current broad notification behavior. Narrow change sets are Candidate 4.

## Compatibility wrapper requirements

Public methods must preserve current behavior:

- `RegenerateAll()` invokes the split stages in the existing order.
- `RebuildSurfaceOnly()` retains its current full Domain/surface/corridor behavior despite its historical name.
- `RebuildCorridorFromGround()` retains its active/enabled guard and Ground-dependent corridor rebuild behavior.
- `ClearGenerated()` retains current output clearing and Domain invalidation behavior.
- Inspector buttons and context menus continue to invoke the same public operations.

No public API is removed or renamed in Candidate 2.

## Required parity evidence

Before and after Candidate 2, capture and compare:

- exact River Domain sample count;
- per-sample position, tangent, side, up, distance, global distance, and left/right widths;
- Domain validation report;
- surface vertex/index counts, submesh count, bounds, UV ranges, and deterministic content hash;
- corridor render mesh vertex/index counts, submesh data, bounds, and deterministic content hash;
- corridor collider vertex/index counts, bounds, and deterministic content hash;
- corridor ring count, across-vertex count, integration apron width, handoff width, and Ground-height-field usage;
- Ground mesh and collider content after a River regeneration;
- material property-block values for representative liquid, frozen, motion, refraction, disturbance-disabled, and Foam-disabled states;
- reflection request behavior;
- Foam exact-cache startup result;
- disturbance field dimensions and dirty-state transitions.

Hashes are validation evidence only and must not become serialized production data unless separately approved.

## Candidate 2 acceptance gate

Candidate 2 passes only when:

- every existing public action produces the same visible and physical outputs;
- River Domain, surface, corridor, and collider parity pass;
- Ground receives the same notifications in the same cases;
- `Domain.Version` and `DomainChanged` retain current semantics;
- Foam and disturbance receive the same notifications in the same cases;
- exact-cache Play startup remains unchanged;
- no new object hierarchy, component, layer, tag, material, or serialized default is introduced;
- Candidate 1 accounting proves only stage separation changed.

## Candidate 2 rejection conditions

Reject if any mesh changes unexpectedly, the Ground handoff changes, collider timing changes, public actions diverge, River Domain events are narrowed early, Foam state or cache behavior changes, or the refactor mixes in request coalescing.

## Candidate 2 deliverable

An accepted River stage map with parity evidence. Candidate 3 must consume these seams rather than re-embedding River responsibilities into GeneratedGround.

# Candidate 3 — Ground-owned transaction and lifecycle coalescing

## Objective

Make GeneratedGround the sole owner of a Ground build transaction while keeping StylizedRiver the owner of River Domain and River output generation. Coalesce lifecycle requests, process exact committed inputs, build Ground once, and invoke post-Ground consumers once.

This candidate removes the proven duplicate ownership path.

## Architectural boundary

The transaction coordinator should initially live inside GeneratedGround or a Ground-owned non-component helper. Candidate 3 must not introduce a global scene scheduler, arbitrary dependency graph, new GameObject manager, or new required component without separate approval.

Ground owns scheduling. It does not own River Domain, River mesh algorithms, Foam, disturbances, reflection behavior, or River material behavior.

## Request model

Existing direct calls are routed into one request API carrying:

- conservative dirty hints;
- request reason;
- requesting object identity;
- immediate-versus-coalescible intent;
- missing-output or forced-regeneration intent.

Requests are accumulated into a pending batch. Reasons and hints are unioned. Exact fingerprints decide which stages execute.

### Coalescible requests

Normally coalescible:

- edit-mode `OnEnable` restoration waves;
- `OnValidate` waves;
- spline change bursts;
- modifier transform changes;
- multi-object Inspector edits;
- Undo/Redo waves;
- River enable/disable registration changes;
- multiple child feature notifications in one Editor update.

### Immediate requests

May require synchronous processing:

- explicit user Regenerate action;
- a caller using an approved `EnsureBuilt` readiness contract;
- runtime structural work that must produce collision before a known gameplay boundary;
- validation tools requiring a committed result before returning.

Immediate processing still uses the same transaction path. It does not call a second legacy generator.

## Editor lifecycle policy

During edit-mode restoration or Inspector edits:

1. Contributors register or issue requests.
2. Ground records one pending batch.
3. Processing occurs once after the current validation/enable wave settles.
4. All active contributors are read in their final validated state.
5. One transaction commits outputs.

No perpetual `Update` polling is allowed. A pending flag may be observed by an existing ExecuteAlways update only while work is actually pending, or an Editor callback may flush the batch. The selected mechanism must support Undo/Redo, assembly reload, scene close, object destruction, and exiting Play Mode safely.

## Runtime readiness policy

Candidate 3 must not blindly defer Ground collision until a later frame.

Before selecting the runtime flush point, audit whether any gameplay or runtime component requires Ground mesh/collider or River projection during `Awake`, `OnEnable`, or `Start`.

The accepted implementation must provide one of these proven contracts:

- Ground builds synchronously before those consumers run; or
- those consumers call a synchronous `EnsureBuilt`; or
- an explicit initialization barrier completes Ground/River structural transactions before gameplay systems are released.

The choice must be documented and validated. A one-frame missing collider is a rejection condition.

## Transaction stages

### 1. Guard and batch capture

- capture the pending request batch;
- mark the transaction active;
- retain new incoming requests in a separate pending accumulator;
- prevent recursive processing;
- initialize batch diagnostics.

### 2. Contributor discovery and normalization

- resolve active modifiers and Rivers;
- ensure removed or disabled contributors are absent;
- order contributors deterministically where order matters;
- normalize authoring state through Candidate 2 seams;
- do not generate visible River output.

Repeated hierarchy scans should be reduced where safe, but registration caching must remain correct through Undo/Redo, duplication, reparenting, enable/disable, scene reload, and destroyed-object cleanup.

### 3. River Domain preparation

- ask each active River to prepare or expose its authoritative Domain;
- calculate exact Domain and Ground-influence fingerprints;
- reuse immutable data when exact inputs are unchanged;
- do not yet change Domain event semantics beyond the accepted Candidate 2 baseline unless explicitly authorized as part of Candidate 4.

Candidate 3 may avoid rebuilding identical Domain data only if doing so can preserve Candidate 2 observable Domain events through an adapter. Otherwise Domain event narrowing waits for Candidate 4.

### 4. Ground snapshot collection

- collect modifier snapshots;
- collect River Ground influence snapshots;
- build exact Ground geometry and Ground feature domain signatures;
- cache reusable immutable snapshots and bounds where their fingerprints match;
- never serialize the complete StylizedRiver component as Ground geometry authority.

### 5. Ground stage execution

Use existing stable Ground stage signatures and missing-output checks to execute only true dependants:

- base/final geometry generation;
- mesh application;
- collider cook;
- surface metadata;
- Painted Accent surface strokes;
- projected glyphs;
- coverage;
- material.

Existing Ground generation and visual algorithms remain unchanged.

### 6. Publish Ground Build Context

After Ground geometry is committed, publish one immutable context representing the committed revision. Do not publish a partially updated context between mesh generation and collider assignment.

### 7. Post-Ground consumers

- execute Ground-dependent visual features against the committed context;
- invoke each active River post-Ground realization once when its true inputs changed or output is missing;
- build River surface only when its surface signature changed or output is missing;
- build River corridor/collider when River corridor inputs or committed Ground revision changed, or output is missing;
- apply visual properties only when required;
- request reflection after current surface output exists.

### 8. Complete or schedule one follow-up

After the transaction:

- clear the active guard;
- inspect requests received during processing;
- compare exact committed fingerprints;
- run at most the required follow-up transaction when inputs genuinely changed during the pass;
- do not recursively invoke processing from a River callback;
- diagnose repeated non-converging requests and stop safely rather than freezing the Editor.

A hard silent request drop is not acceptable. A permanent unbounded retry loop is also not acceptable.

## River lifecycle under Candidate 3

### River OnEnable

- migrate and validate authoring state;
- subscribe to spline changes;
- ensure required output references and runtime components safely exist;
- register with parent Ground or issue one structural request;
- do not independently execute a second Ground transaction;
- enable rendering/runtime against the committed outputs.

If Ground has already committed the exact River influence and post-Ground output revision during its own enable path, River enablement becomes a readiness/binding operation rather than a structural rebuild.

### River OnDisable and OnDestroy

- unsubscribe from spline events;
- disable/release owned runtime state according to current contracts;
- unregister from parent Ground;
- enqueue removal of Ground influence;
- cause Ground concealment, shore metadata, and bank-aware features to update once;
- avoid processing if the entire scene or parent Ground is shutting down and no committed output can be observed.

Shutdown suppression must be explicit and must not make ordinary component disable leave stale Ground.

### Standalone River

When no valid parent GeneratedGround exists, StylizedRiver executes a local transaction using Candidate 2 stages:

```text
normalize -> Domain -> surface -> fallback corridor -> material -> runtime notifications
```

The fallback must preserve current Ground-null height and normal behavior.

## Public API compatibility

- `GeneratedGround.Regenerate()` becomes an immediate request plus transaction flush.
- `GeneratedGround.NotifyModifierChanged()` and `NotifyRiverChanged()` remain temporary adapters that enqueue requests rather than processing recursively.
- `StylizedRiver.RegenerateAll()` requests the parent transaction and flushes immediately for an explicit action; standalone River uses its local path.
- `RebuildSurfaceOnly()` and `RebuildCorridorFromGround()` remain adapters until their public semantics are separately reviewed.

No external caller should need to know whether the committed result was reused or rebuilt.

## Candidate 3 diagnostics

The batch summary must report:

- request count and reasons;
- coalesced request count;
- transactions executed;
- follow-up transaction count;
- Ground stages executed;
- River Domains prepared/reused;
- River influence snapshots built/reused;
- Ground mesh uploads and collider cooks;
- Ground feature stages executed;
- River surface/corridor/collider builds;
- total batch wall time;
- any non-converging or reentrant request reason.

The summary must not be overwritten by a trailing no-op request.

## Candidate 3 validation matrix

Repeat the Candidate 1 matrix and add:

1. Scene open after assembly reload with multiple active modifiers and Rivers.
2. Rapid repeated spline edits within and across the River debounce interval.
3. Multi-object River Inspector structural edits.
4. Disable and re-enable several Rivers in one hierarchy operation.
5. Delete, Undo-delete, duplicate, and reparent a River.
6. Delete, Undo-delete, duplicate, and reparent a GroundModifier.
7. Enter and exit Play Mode with Domain Reload enabled and disabled where the project supports both.
8. Validate Ground collision during the earliest gameplay lifecycle point that consumes it.
9. Explicit Regenerate actions during a pending coalesced request.
10. A request issued during an active transaction to prove bounded follow-up behavior.
11. Standalone River generation without GeneratedGround.
12. Ground disabled while River remains enabled, and the reverse.

## Candidate 3 acceptance gate

Candidate 3 passes only when:

- one restoration wave produces one expensive Ground structural transaction;
- River enablement does not trigger a second identical Ground structural transaction;
- modifier enablement does not multiply expensive Ground transactions;
- a legitimate spline/width/bank edit rebuilds every true dependant exactly once;
- water, Foam, debug, and material-only edits do not execute Ground structural stages;
- Ground mesh/collider and River mesh/collider parity remain accepted;
- disabled/removed Rivers restore Ground correctly;
- standalone Rivers remain functional;
- runtime collider/projection readiness is preserved;
- no recursive callback or non-converging Editor loop occurs;
- Foam Play startup remains cache-only;
- Candidate 1 diagnostics show the eliminated duplicate rather than merely hiding it.

## Candidate 3 rejection conditions

Reject if work is merely delayed rather than eliminated, the collider becomes temporarily unavailable, River output appears one frame late without approval, Ground misses a spline edit, component disable leaves stale concealment, explicit actions return before committed output is ready, or a request loop can still freeze the Editor.

## Candidate 3 deliverable

One accepted Ground-owned transaction with compatibility adapters still in place. Broad runtime notifications remain until Candidate 4.

# Candidate 4 — Exact invalidation and targeted River runtime publication

## Objective

Replace broad River change broadcasts and legacy Ground River signatures with exact committed revisions and narrow runtime change publication. Preserve River Foam and disturbance ownership while eliminating unnecessary resource invalidation.

Candidate 4 is intentionally separate because changing Domain event semantics is more dangerous than changing build scheduling.

## Domain commit semantics

Prepare a candidate River Domain and calculate its exact fingerprint before publication.

### When the fingerprint changed or output was missing

- commit the new RiverDomainSnapshot;
- increment Domain version exactly once;
- update River length and average surface height;
- publish `DomainChanged` exactly once after the committed Domain is valid;
- include Domain change in the River change set;
- invalidate true Domain consumers.

### When the fingerprint is identical

- reuse the existing committed Domain;
- do not increment Domain version;
- do not publish `DomainChanged`;
- publish readiness separately if a newly enabled consumer requires rebinding;
- do not invalidate Foam topology cache validity or disturbance resources solely due to lifecycle restoration.

Candidate 1 shadow evidence must prove the fingerprint before it becomes authoritative.

## Ground influence commit semantics

The exact immutable River Ground influence fingerprint becomes the sole River-derived structural input to the Ground geometry signature.

Remove these as Ground geometry authorities:

- complete `EditorJsonUtility.ToJson(river)` output;
- broad runtime notification revision;
- River snapshot count without content;
- whole-component state unrelated to Ground influence.

Ground may still include River identity, active membership, deterministic ordering, transform-derived snapshot content, and missing-output state through the exact committed influence set.

## Required runtime notification split

### Readiness publication

Used when a runtime or renderer becomes enabled and needs the current committed products bound. It does not imply Domain, geometry, boundary, topology, or material content changed.

### Domain publication

Sent only when River-space content changed. Disturbance and Foam may resize/reallocate, remap, or revalidate topology according to their existing ownership rules.

### Boundary/corridor publication

Sent when committed Ground revision or corridor inputs changed shoreline/corridor geometry without changing River Domain.

Foam Layer A/Layer B and disturbance boundary products may become dirty. Persistent Foam material must not be reset unless the runtime proves that its coordinate mapping became incompatible.

### Visual publication

Sent for water body, lighting, ice, motion, refraction, debug, reflection, or material-property changes. Ground and structural River geometry remain untouched unless a specifically shared safety input changed.

### Foam configuration publication

Sent only to Foam-owned code for Foam authoring changes. It must not route through Ground structural invalidation.

### Disturbance configuration publication

Sent only to disturbance-owned code for pressure, wake, ripple, or simulation changes. A structural geometry rebuild occurs only when the accepted reserved-displacement or surface-safety contract truly changes.

## Foam requirements

Candidate 4 must explicitly prove:

- exact-cache Play startup still performs zero topology builds and zero cache writes;
- an identical lifecycle enable does not mark the cache stale through a false Domain change;
- a true Domain change revalidates the assigned cache and reaches the correct Exact, Stale-compatible, or Preparation Required result without automatic generation;
- a corridor/boundary-only change dirties only the necessary boundary/external influence products;
- persistent Foam Presence, Remaining Life, and Material Pattern are not reset by Ground material, River visual, reflection, or debug changes;
- committed persistent material remains owned by Layer C;
- visual Layer D/E work never feeds structural Ground or River invalidation.

## Disturbance requirements

Candidate 4 must explicitly prove:

- identical Domain reuse does not reallocate disturbance fields;
- a true Domain mapping change reallocates or remaps as currently required;
- Ground/corridor changes refresh shoreline/static boundary products where necessary;
- generated stationary source registration remains event-driven and correct;
- visual-only River changes do not dirty static pressure, wake, ripple boundary, or generated-geometry registry state;
- runtime impacts and active reservations retain their accepted lifecycle behavior.

## Reflection and material requirements

- Request reflection after a changed/committed surface or appearance state, not before output readiness.
- Preserve neutral main-material Foam/disturbance property writes followed by runtime LateUpdate binding.
- Do not rebuild River geometry for reflection resolution, mask, update cadence, or appearance changes.

## Legacy adapter retirement

Only after all targeted notifications are accepted may Candidate 4 retire or narrow:

- broad `NotifyRiverChanged` calls;
- unconditional Foam runtime notification after every broad River regeneration;
- unconditional Domain version increments for identical content;
- legacy Ground River JSON signatures;
- redundant River-to-Ground synchronous callback paths retained as Candidate 3 adapters.

Public methods may remain, but their internal meaning becomes exact request/change publication.

## Candidate 4 validation matrix

Run the complete Candidate 3 matrix plus focused runtime-state tests:

1. Enable/disable River with unchanged Domain and inspect Domain version/event count.
2. Ground-only height change with unchanged River Domain.
3. Corridor-quality-only change.
4. Spline-knot edit.
5. Connected-distance and reverse-flow edit.
6. Water material and lighting edits.
7. Motion edits that do and do not alter reserved displacement.
8. Foam presentation-only edit.
9. Foam topology-generation-input edit.
10. Disturbance visual/configuration edit.
11. Active persistent Foam before an unrelated visual change.
12. Active disturbances before an unrelated visual change.
13. Exact, stale-compatible, and missing/incompatible Foam cache startup cases.
14. Ground/River restoration followed by immediate projection queries.

## Candidate 4 acceptance gate

Candidate 4 passes only when:

- Domain version changes exactly once per true Domain content change and never for identical reuse;
- spline content can never change without changing the authoritative Domain/Ground influence fingerprints;
- unrelated serialized River fields can never change Ground geometry signatures;
- Ground-only changes rebuild River corridor/boundary dependants without false Domain invalidation;
- Foam and disturbance state survive unrelated visual and Ground-material changes;
- true structural changes still invalidate every required runtime product;
- cache-only startup and persistent-state ownership remain fully compliant;
- no consumer relies on the removed broad event as an accidental readiness signal.

## Candidate 4 rejection conditions

Reject if stale runtime resources appear, persistent Foam is reset unnecessarily, topology builds during Play, a true Domain change is missed, a boundary remains stale after Ground changes, material binding order breaks, or event narrowing causes a newly enabled runtime to remain unbound.

## Candidate 4 deliverable

The accepted production orchestration contract: single Ground transaction ownership, exact Ground/River invalidation, and narrow River runtime publication.

# Candidate 5 — Optional future Ground feature contributor/consumer contract

## Status and authorization boundary

Candidate 5 is not required to correct the current Ground–River duplicate regeneration. It begins only after Candidate 4 is accepted and only with explicit approval for any new interfaces, files, or registration architecture.

Its purpose is to prevent future bank-aware or Ground-aware features from recreating direct callback coupling.

## Objective

Formalize the proven Ground-local staged model so future systems can either influence Ground before commit or consume committed Ground afterward without calling broad regeneration APIs.

## Recommended minimal abstraction

Prefer a Ground-local contract over a global scene dependency graph.

Conceptual roles:

### Structural influence provider

May:

- read normalized authoring state;
- read the base Ground field when its approved algorithm requires it;
- return an immutable, fingerprinted influence snapshot;
- declare deterministic priority if influence order is meaningful.

May not:

- upload or mutate the Ground mesh directly;
- recook the Ground collider;
- call Ground regeneration recursively;
- read post-Ground visual products and feed them back into the same transaction;
- introduce per-frame full-field work.

StylizedRiver is one structural influence provider through its Ground influence snapshot. GroundModifier is another existing conceptual provider.

### Post-Ground consumer

May:

- read immutable GroundBuildContext;
- sample base Ground height, normals, and semantic masks;
- query River-bank distance and influence;
- build its own visual or gameplay output;
- cache output by exact input revision.

May not:

- mutate committed Ground geometry;
- request another Ground transaction as a side effect of output generation;
- depend on another consumer's private mutable output unless an explicit ordered stage is approved.

Painted Accents and visible River realization are existing conceptual post-Ground consumers, although GeneratedGround may retain direct ownership of Painted Accent stages.

## Bank semantic contract

Future features should consume semantic bank data rather than inspect River render triangles.

Minimum useful queries include:

- nearest River identity;
- signed lateral distance from River center/domain;
- distance from visible water bank;
- inside visible water, hidden overlap, handoff, or outside state;
- water surface height;
- visible and hidden half-widths;
- bank blend/influence width;
- broad shore influence;
- Ground height, normal, slope, material classification, and suitability at the same position.

The implementation may expose queries over immutable snapshots rather than bake a new global texture. New persistent fields or textures require separate performance justification.

## Feature classification rule

Every proposed feature must be classified before implementation:

1. **Structural:** changes Ground positions, normals, collision, or required vertex metadata. It belongs before Ground commit.
2. **Semantic:** writes Ground-owned masks or classifications without changing positions. It belongs in the Ground finalization pipeline with exact dependencies.
3. **Visual/gameplay consumer:** reads committed Ground/semantic data and owns separate output. It belongs after Ground commit.
4. **Material-only:** changes property bindings only and must not enter structural processing.

A feature that reads final Ground and then asks to change that same Ground introduces a cycle. It must instead be reformulated as an influence over the base Ground field, or use an explicitly approved bounded multi-phase algorithm inside one transaction.

## Registration and lifecycle requirements

- registration, change, disable, removal, destruction, duplication, reparenting, Undo/Redo, and scene reload must all update membership correctly;
- inactive providers do not contribute snapshots;
- inactive consumers do not build output;
- lifecycle waves enqueue one Ground batch rather than processing independently;
- deterministic ordering is explicit where output depends on order;
- destroyed Unity object references are cleaned without permanent polling;
- explicit user regeneration can force validation without forcing unchanged outputs to rebuild.

## Candidate 5 proof feature

Do not migrate every feature simultaneously. Select one small existing or approved future consumer that needs Ground and River-bank data but does not modify Ground geometry. Use it to prove:

- context access;
- bank semantic queries;
- exact revision invalidation;
- add/change/disable/remove lifecycle;
- no recursive Ground calls;
- no changes to Ground or River visual output.

Only after this proof should additional systems adopt the contract.

## Candidate 5 acceptance gate

Candidate 5 passes only when:

- the proof feature consumes Ground/River data without direct component callback coupling;
- adding multiple consumers does not multiply Ground structural transactions;
- disabling/removing the proof feature leaves Ground and River untouched;
- exact dependency changes rebuild only the proof feature;
- Ground/River structural changes rebuild it once when required;
- no global manager, per-frame scan, new persistent texture, or arbitrary graph solver was introduced without approval;
- existing Ground, River, Foam, disturbance, and reflection validation remains green.

# Cross-candidate validation record

Each candidate report must state:

- branch and exact commit SHA validated;
- dirty-worktree status and approved file scope;
- files changed;
- request/pass counts for restoration, Play entry, and Play exit;
- Ground stages executed;
- River stages executed;
- mesh/collider parity or intentional differences;
- Domain version/event behavior;
- Foam startup outcome and topology-build/cache-write counts;
- disturbance resource behavior;
- reflection behavior;
- known limitations;
- explicit user acceptance or rejection.

## Required restoration success criteria

At final Candidate 4 acceptance:

```text
one lifecycle restoration batch
  -> one legitimate Ground structural transaction
  -> one Ground mesh upload
  -> one Ground collider cook
  -> each true Ground feature dependant at most once
  -> each active River surface/corridor dependant at most once
  -> zero identical follow-up structural transactions
```

An unchanged trailing request may be recorded, but it should normally be coalesced before processing. It must never hide the batch's real total cost.

## Performance acceptance criteria

- No accepted visual resolution, density, topology, or quality reduction is used to claim lifecycle improvement.
- No new steady-state per-frame Ground or River orchestration work is introduced.
- No repeated whole-component JSON serialization is used as Ground invalidation authority.
- Unchanged immutable snapshots are reused where safe.
- Ground collider recooking occurs only after changed geometry.
- River surface and corridor meshes rebuild only from their exact dependencies or missing output.
- Profiling distinguishes request overhead, snapshot preparation, Ground stages, River stages, and runtime publication.
- Editor restoration time is measured as a batch, not only as the last individual call.

## Failure handling

If any candidate fails validation:

- leave the validated baseline branch untouched;
- retain diagnostics proving the failure;
- identify the exact stage and ownership boundary that failed;
- correct only the active candidate;
- repeat the complete candidate acceptance gate;
- do not begin the next candidate;
- do not compensate with reduced output quality, disabled functionality, broad event restoration, or hidden cache generation.

## Final architectural position

GeneratedGround is the scheduler and commit owner for one Ground patch transaction.

StylizedRiver remains the owner of River Domain, visible River geometry, River collision, material behavior, reflections, disturbances, and Foam integration.

River Ground influence is immutable structural input. GroundBuildContext is immutable committed output. Future systems interact through those staged contracts rather than synchronous feedback.

The intended result is not merely fewer calls. It is explicit ownership:

```text
River prepares River truth.
Ground commits Ground truth once.
Ground-dependent features consume committed truth.
River realizes visible output against committed Ground.
Runtime systems receive only the changes they actually own.
```

## GR-O1 — Editor regeneration accounting and shadow fingerprints

**Status:** Implemented and validated in Unity. The copied accounting reports isolated the Play-startup duplication and are the accepted evidence baseline for GR-O3A.

GR-O1 is an observational prerequisite for orchestration changes. It does not suppress, defer, merge, reorder, or gate any existing Ground or River work.

### Implemented evidence

- GeneratedGround records one compact completed batch containing request origins, pass count, passes with no expensive stage, per-stage execution counts, measured pass time, River notifications, corridor callbacks, and a timestamped ordered request/pass timeline.
- StylizedRiver records request origins, debounce coalescing, full/surface/corridor passes, Domain/surface/corridor outputs, collider assignments, Ground/Foam/reflection notifications, exact-content fingerprints, and a timestamped ordered request/output/pass timeline.
- The River Domain fingerprint excludes `Domain.Version`. An identical rebuild is therefore reported as unchanged content even when the version increments and publishes again.
- The Ground-influence fingerprint is calculated from the exact immutable snapshot arrays and scalar controls already produced by `CreateGroundSnapshot`. It is diagnostic only and never participates in invalidation.
- Batch completion occurs after one quiet Editor delay. A pending River debounce keeps the active batch open until its pass executes.
- Both Inspectors provide copy-to-clipboard, clear, and one-shot Console controls. The River Inspector also copies the parent Ground and River reports as one combined payload.
- No default Console output, per-frame hashing, serialized telemetry, or retained event history was introduced.

### Accepted evidence

- Edit-mode compile/reload produced five Ground requests but only one expensive structural pass: `10693.097 ms`. The four trailing passes were material/snapshot-only and together cost approximately `1.5 ms`.
- First Play entry produced three expensive Ground passes: `11866.388 ms`, `13069.546 ms`, and `10645.878 ms`, totalling `35582.229 ms`.
- The first two expensive passes were caused by two `ModifierChanged` enable notifications. The third was caused by `StylizedRiver.OnEnable -> RiverChanged`.
- River Domain and surface preparation before the Ground callback took only milliseconds. The River full-pass duration was dominated by the nested synchronous Ground regeneration.
- Moving a spline knot produced a `7.132 ms` River structural pass and a `0.417 ms` Ground snapshot/material pass with no Ground geometry, mesh, collider, accent, coverage, or corridor stage.
- Changing Foam colour produced no new structural accounting batch.
- Conclusion: the high-value defect is not harmless request count. It is generation of two temporary intermediate Ground states before the final active contributor set is available during Play startup.

### GR-O1 closure checks

- [x] Undo/Redo and ordinary edit-mode accounting remained behaviorally neutral through GR-O3A validation.
- [x] Diagnostics were retained and used to confirm the accepted one-pass startup result.

# GR-O3A — Play-startup Ground regeneration coalescing

**Status:** Unity-validated and accepted.

GR-O3A is the lowest-risk, evidence-backed slice of Candidate 3. It addresses only the measured Editor Play-entry enable wave. It does not yet replace the broader edit-mode, Undo/Redo, disable/removal, or multi-consumer transaction architecture described by full Candidate 3.

## Exact behavior

During the first two Play frames in the Unity Editor, these automatic requests may join one pending Ground startup batch:

- `GeneratedGround.OnEnable`;
- `GroundModifier.OnEnable -> NotifyModifierChanged`;
- `StylizedRiver.OnEnable -> NotifyRiverChanged`.

In the Unity Editor, the Play-startup batch may also be queued when no retained Ground output survives scene restoration. The project-wide lifecycle audit found no Ground/River consumer that reads Ground collision, Ground sampling, River projection, Foam, or disturbance data from `Awake`, `OnEnable`, or `Start`. `GeneratedGround.Start` therefore acts as the explicit initialization barrier: the final transaction completes before any ordinary `Update`, `FixedUpdate`, or `LateUpdate` consumer can run. When retained mesh/collider output does exist, it remains assigned during the pending window. Player builds retain the previous synchronous behavior; GR-O3A is an Editor iteration optimization.

The pending batch is flushed by the earliest of:

- `GeneratedGround.Start`, before ordinary `Update`/`LateUpdate` consumers;
- one Unity Editor delay callback after the current enable wave;
- an explicit or otherwise non-coalescible Ground request, which absorbs and flushes the pending requests synchronously.

At flush, GeneratedGround refreshes the final active modifier/River membership and executes the existing unchanged Ground stage-signature pipeline once. No Ground generation algorithm, mesh quality, collider quality, Painted Accent algorithm, or signature formula is changed.

When River OnEnable receives a deferred Ground result, it builds one cheap temporary corridor against the retained Ground output so the River render mesh and collider remain available. The final Ground transaction later invokes the existing `RebuildCorridorFromGround` callback, replacing that temporary corridor before normal runtime `Update`/`LateUpdate` processing. River Domain, surface, visuals, reflection, Foam notification, and public explicit regeneration semantics remain owned by StylizedRiver.

## Diagnostics added

GeneratedGround accounting now reports:

- coalesced request count;
- Play-startup flush count;
- forced-immediate flush count;
- requests queued without retained Ground output, proving when the initialization barrier was used;
- ordered `Queued PlayStartup` and `Flush PlayStartup` timeline events.

StylizedRiver accounting now reports:

- Ground notifications committed before return;
- Ground notifications deferred to the startup transaction;
- the temporary corridor build and final Ground-callback corridor build as separate output events.

## Readiness audit

The current project sources contain no Ground/River gameplay consumer that reads the Ground height snapshot, Ground collider, River projection, Foam field, or disturbance field from `Awake`, `OnEnable`, or `Start`. River Foam and disturbance processing begins in `LateUpdate`; reflection and debug consumers begin in `Update`. `GeneratedGround.Start` is therefore the accepted initialization barrier. GR-O3A retains existing Ground mesh/collider output when available and builds a temporary River corridor during the pending window, but it may also queue a cold Play-scene restoration with no retained Ground output because the final Ground transaction completes before ordinary runtime processing begins.

## Deliberately unchanged

- Edit-mode compile/reload generation remains synchronous. It already produced only one expensive pass.
- Explicit `GeneratedGround.Regenerate` and explicit River regeneration remain synchronous.
- Spline debounce and normal structural edits remain unchanged.
- Player builds remain synchronous.
- Domain versioning, `DomainChanged`, Foam invalidation, disturbance invalidation, reflection requests, and Ground signatures are not narrowed.
- Modifier and River callbacks remain compatibility adapters.

## Acceptance gate

- [ ] First Play entry reports three or more automatic requests coalesced into exactly one expensive Ground geometry/mesh/collider pass.
- [ ] The Ground report shows one Play-startup flush; any `queued without retained output` count is followed by the committed pass before frame processing begins.
- [ ] River reports one deferred Ground notification, an immediate temporary corridor, and one final `GroundCorridorChanged` corridor rebuild.
- [ ] Ground and River render meshes remain visible continuously and both colliders remain assigned.
- [ ] Foam startup remains cache-only and begins against the final Domain/corridor state.
- [ ] Spline editing retains the accepted millisecond-scale River pass and Ground no-geometry response.
- [ ] Foam/material-only edits produce no Ground structural pass.
- [ ] Explicit Regenerate actions still return only after committed output exists.

## Rejection conditions

Reject GR-O3A if the three expensive Ground passes still occur, work merely moves after the first frame, a mesh or collider disappears during startup, Foam begins before the final corridor is available, a startup request remains pending, explicit regeneration becomes asynchronous, or ordinary spline/material behavior changes.

## Methods ledger

- **Accepted:** Coalesce only the measured Editor Play-startup automatic enable wave while retaining previous mesh/collider output.
- **Accepted readiness contract:** `GeneratedGround.Start` is the initialization barrier for Editor Play startup; retained output is used when available but is not required for coalescing.
- **Accepted compatibility behavior:** Deferred River notification builds a cheap temporary corridor, followed by the existing final Ground callback.
- **Rejected:** Delete Modifier or River enable callbacks. This can leave contributor membership stale under another enable order.
- **Rejected:** Suppress requests solely because signatures previously matched. Each measured intermediate pass represented a genuinely different active contributor set.
- **Deferred:** Full Candidate 2 River stage decomposition. GR-O3A does not require it because it does not interrupt or externally schedule individual River stages.
- **Deferred:** Full edit-mode/Undo/disable/removal Candidate 3 transaction coordinator until GR-O3A evidence demonstrates whether broader scheduling work remains valuable.


# EDITOR-RELOAD-DIAG-R1 — Domain-reload wall-time and PixelSurface repair instrumentation

**Status:** Gate 1 review complete; Gate 2 plan recorded before implementation. Unity validation pending.

## Objective

Measure the current multi-minute script/domain-reload stall without changing Ground, River, Foam, cache, material, library-generation, or runtime behavior. The diagnostic must separate script compilation, assembly reload, post-reload editor callback gaps, and automatic PixelSurface detail-library repair work so the first dominant authority can be identified before any optimization is selected.

## Current evidence

- User telemetry shows one Ground accounting batch with `63252.492 ms` wall time but only `37.713 ms` of measured passes, and one River accounting batch with `42056.849 ms` wall time but only `43.467 ms` of measured passes. The final Ground/River regeneration events complete roughly forty-one seconds before their delayed accounting-completion callbacks are serviced. This falsifies “Ground/River measured regeneration work consumed the whole wall delay” for that capture.
- Historical Foam startup telemetry includes an Exact topology-cache hit with approximately `91 s` total wall time but only tens of milliseconds in named startup phases. A separate capture spent approximately `4.69 s` inside `RefreshInitialTopologySources`; those are distinct costs and must remain distinct.
- `StylizedSurfaceDetailLibraryBuilder.Initialize()` schedules `RepairAllLibraries()` after every domain load and subscribes the same repair scheduling to `EditorApplication.projectChanged`.
- `RepairAllLibraries()` scans all `StylizedSurfaceDetailLibrary` assets, calls `NeedsRebuild()`, and may call `Rebuild()`.
- `NeedsRebuild()` calculates a full content signature. `CalculateSignature()` walks every library entry and calls `AssetDatabase.GetAssetDependencyHash(...)` for every referenced source texture.
- `Rebuild()` may normalize source importers, generate texture arrays, replace generated sub-assets, save the library, call `AssetDatabase.ImportAsset(..., ForceUpdate)`, and notify all material profiles using the library.
- The existing Ground/River orchestration accounting intentionally measures regeneration passes, not arbitrary editor/AssetDatabase work occurring between delayed callbacks. Therefore a separate editor-reload timeline is required.

## Approved files

Modify:

- `Assets/Docs/Ground_River_Regeneration_Orchestration_Manual.md`
- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs`

Create:

- `Assets/Game/Rendering/PixelSurface/Editor/EditorReloadDiagnostics.cs`

No other files are approved for R1.

## Invariants

- Diagnostic-only. Do not suppress, reorder, defer, coalesce, skip, or add any existing regeneration, cache, library repair, import, or runtime operation.
- No scene, prefab, material, profile, cache asset, generated array, layer, tag, serialized default, or runtime resource is changed by the diagnostic itself.
- Automatic PixelSurface repair retains its existing schedule and rebuild decisions.
- Ground/River orchestration, River Foam topology-cache policy, D9 spawning behavior, C3A material behavior, disturbances, and player/runtime code remain untouched.
- Idle editor cost must be effectively zero: no permanent per-frame polling. Update sampling is active only for a bounded diagnostic capture.
- Diagnostic state is editor-session-only. It must not serialize into project assets.
- One report must distinguish compilation, pre-reload delay, assembly reload, post-reload callback gaps, PixelSurface repair scans, signature checks, rebuild stages, and AssetDatabase import/save work where observable.

## Implementation sequence

1. Add an editor-only reload profiler that persists a small capture timeline across assembly reload with `SessionState`, subscribes to compilation/reload events, records a bounded number of post-reload editor updates/delay callbacks, exposes an arm-next-reload action, exposes a forced-recompile capture action, and copies/logs one consolidated report after the capture settles.
2. Instrument `StylizedSurfaceDetailLibraryBuilder` without changing decisions: record schedule reason/coalescing, repair invocation start/end, `FindAssets`, per-library load, `NeedsRebuild`, signature calculation, rebuild/no-rebuild result, importer normalization, validation, array construction/application, sub-asset replacement, save/import, refreshed-library load, material notification, and total rebuild time.
3. Keep normal operation silent. Detailed stage capture executes only while R1 capture is armed; the builder performs the same calls regardless of capture state.
4. Gate 4: compare the final diff against the three-file scope, re-read the complete review surface, run delimiter/preprocessor/static symbol checks, confirm no Ground/River/Foam/cache behavior edits, and package changed files only.

## Controlled reproduction contract

R1 provides two editor actions:

- **Arm Next Reload Timeline Capture** — records the next naturally triggered script/domain reload, which is the preferred representative measurement when reproducing the user's normal code-edit workflow.
- **Force Script Recompile + Capture** — requests a clean script compilation to guarantee one diagnostic reload when a natural source change is inconvenient. This intentionally forces compilation and is therefore a diagnostic reproduction path, not a compile-performance baseline.

The consolidated report must include enough timestamps to distinguish time before `beforeAssemblyReload`, `beforeAssemblyReload` to post-reload initialization/`afterAssemblyReload`, time until the first editor update/delay callback, PixelSurface repair work, and any residual unaccounted wall gap.

## Acceptance criteria

- No project/runtime behavior changes when capture is idle.
- One armed reload yields exactly one consolidated report rather than continuous Console output.
- Report includes compilation start/finish where Unity publishes those events, assembly reload boundaries, post-reload callback/update timing, and PixelSurface repair invocation count.
- For each automatic detail library checked during the capture, report includes path/name, stale/current result, signature/decision elapsed time, and rebuild elapsed time when rebuilding occurs.
- A rebuild report decomposes importer normalization, validation, array build/apply, signature/finalization, save, forced import, reload, and material notification sufficiently to identify a dominant substage.
- Existing automatic repair still repairs stale libraries and leaves current libraries untouched exactly as before.
- No recurring update callback remains registered after capture completion.

## Risks and mitigations

- **Instrumentation overhead can distort small timings.** Mitigation: use `Stopwatch`/UTC timestamps and simple in-memory/session strings only while capture is armed; no texture reads, asset scans, or extra signatures are performed solely for diagnostics.
- **A forced clean recompilation is slower than ordinary incremental compilation.** Mitigation: label trigger mode in the report and provide the natural “arm next reload” action as the preferred workflow measurement.
- **Static state is destroyed by assembly reload.** Mitigation: persist only minimal capture state/timeline through `SessionState`; project assets remain untouched.
- **Project-changed repair may schedule another repair.** Mitigation: record every schedule request, whether it was coalesced, and every repair invocation during the same capture rather than assuming one run.
- **Diagnostic callbacks could remain active.** Mitigation: bounded update sampling and explicit unsubscribe/finalization paths.

## Validation

Pending Unity 6000.5.0f1 validation:

1. Import/compile with no errors or new warnings attributable to R1.
2. Run one natural armed reload capture after a representative River/code edit and provide the complete consolidated report.
3. If a natural edit is inconvenient, run the forced-recompile capture once and provide the complete report, clearly retaining its `CleanBuildCache` trigger label.
4. Confirm ordinary editor operation after report completion has no recurring diagnostic Console output or persistent update activity.


## EDITOR-RELOAD-DIAG-R1 implementation and Gate 4 audit record

**Source status:** implemented within the approved three-file diagnostic scope. **Unity 6000.5.0f1 validation:** pending; no Unity executable/compiler is available in the working environment.

### Implemented behavior

- Added one editor-session-only reload timeline profiler. It records compilation start/finish and per-assembly compiler-message counts, `beforeAssemblyReload`, post-reload initialization/`afterAssemblyReload` when published, the first diagnostic delay callback, a bounded post-reload Editor-update sample, and one final consolidated report.
- Capture state that must survive the domain boundary is stored only in `SessionState`. The event timeline is accumulated in memory and persisted once at `beforeAssemblyReload`, avoiding per-event SessionState string writes during expensive post-reload work.
- Added **Arm Next Reload Timeline Capture** for the representative natural code-edit workflow, **Force Script Recompile + Capture** for a guaranteed `CleanBuildCache` diagnostic reproduction, and **Copy Last Reload Timeline Report**. The forced action is explicitly labeled non-representative for compilation performance.
- The automatic PixelSurface detail-library repair records schedule reason/coalescing and each `RepairAllLibraries` invocation. During an armed capture it times library discovery/loading, rebuild decisions, signature calculation, individual dependency-hash reads, rebuild phases, changed-importer reimports, asset save, forced import, refreshed-library load, and material-profile notification.
- Existing builder decisions and project-mutating calls are not duplicated. Instrumentation observes the existing calls in place.
- High-frequency `NeedsRebuild`, `CalculateSignature`, and dependency-hash instrumentation is guarded by one cached capture-active boolean so normal Inspector/status checks do not allocate diagnostic strings or invoke timers after capture completion.
- Post-reload update sampling unsubscribes on report finalization and is hard-bounded even when the expected PixelSurface repair is not observed.

### Static audit

`42/42 PASS`:

- exact changed scope is the approved document, existing PixelSurface builder, and new editor-only profiler;
- existing builder counts for `FindAssets`, `LoadAssetAtPath`, `GetAssetDependencyHash`, `SaveAndReimport`, `SaveAssetIfDirty`, `ImportAsset`, `AddObjectToAsset`, generated-subasset destruction, signature calculation, and automatic rebuild calls are unchanged;
- the profiler contains no runtime initialization hook and no project AssetDatabase save/import/create/delete/move operation;
- Ground generation, River generation, Foam topology cache/resources, D9 birth-event implementation, Foam compute, and C3A final-render include are byte-identical to the pre-R1 source;
- C# delimiter checks pass for both modified/new source files;
- no scene, prefab, material, profile, cache asset, layer, tag, serialized default, shader, compute, or runtime source is changed.

### API verification

The implementation uses Unity-supported Editor APIs for assembly reload events and script compilation. Unity's current 6000.0 scripting reference documents `AssemblyReloadEvents.beforeAssemblyReload` / `afterAssemblyReload`, and documents `CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache)` as the force-recompile path when no tracked source change is available.

## EDITOR-RELOAD-DIAG-R1 FIX1 — C# compile correction

**Status:** Gate 1 re-review complete after failed Unity import; corrective plan recorded before C# edits.

### Observed failure

Unity 6000.5.0f1 rejected the initial R1 builder instrumentation. The first reported errors are at the two diagnostic-only interpolated strings in `NotifyMaterialsUsingLibrary(...)`: the null fallback string literal was written with backslash-escaped quotes inside the interpolation expression. In C# interpolation expressions, the nested string literal must use ordinary quotes. The downstream brace/modifier errors are parser cascade errors after those two malformed expressions.

### Corrective scope and invariants

- Keep the already-approved R1 three-file scope unchanged.
- Correct only the malformed diagnostic string expressions unless additional compile/syntax evidence identifies another R1 defect.
- Do not change PixelSurface repair decisions, scheduling, rebuild behavior, Ground/River/Foam behavior, runtime code, assets, or diagnostic semantics.
- Search both R1 C# files for the same malformed escaped-quote pattern and other suspicious interpolation/string syntax.
- Re-run delimiter and preprocessor checks, plus an interpolation-aware lexical syntax scan that fails on invalid escape/string state rather than relying only on delimiter counts.
- Compare the final C# diff against the failed R1 package and confirm that executable differences are limited to compile correction.
- Unity import remains the final compiler authority; do not describe FIX1 as validated until the corrected package imports cleanly.

### Acceptance criteria

- The two malformed interpolation expressions compile as valid C#.
- No remaining R1 C# source contains the same escaped-quote-in-interpolation defect.
- Both R1 C# files pass the strengthened static syntax scan, delimiter balance, preprocessor balance, and stale malformed-pattern search.
- R1 behavior and the original diagnostic scope are otherwise byte-for-byte/semantically unchanged.
- Unity imports with no C# errors attributable to R1.

### FIX1 implementation and Gate 4 static audit

**Source status:** the two malformed diagnostic interpolations are corrected. **Unity compiler validation remains pending.**

The failed R1 package and FIX1 were compared directly. The only executable-source differences are the two null-fallback interpolation corrections in `NotifyMaterialsUsingLibrary(...)`; `EditorReloadDiagnostics.cs` is byte-identical to the failed R1 package. No diagnostic behavior, PixelSurface repair decision, scheduling, rebuild operation, or protected subsystem was otherwise changed.

Strengthened static validation: `33/33 PASS`. In addition to the previous scope/invariant checks, FIX1 uses an interpolation-aware C# lexical scanner that reproduces the original failed-package parser break at the two Unity-reported locations and reports zero errors on the corrected R1 files. The same scanner reports zero errors across all ten PixelSurface Editor C# files and all `284` C# files under the current game source tree. Both R1 C# files also pass preprocessor balance.

The original R1 `42/42 PASS` static audit is **superseded as compile-safety evidence**: it verified scope, symbols, and delimiter counts but did not lex interpolation expressions and therefore failed to detect the malformed C# that Unity rejected. Its unaffected scope/invariant results remain historical evidence only; FIX1 `33/33 PASS` plus Unity import is the current validation authority.

The audit reconfirms unchanged counts for the existing builder calls to asset discovery/loading, dependency hashing, importer reimport, asset save/import, generated-subasset replacement/destruction, signature calculation, and automatic rebuild. Ground generation, River generation, Foam topology cache/resources, D9 birth-event code, Foam compute, and C3A final-render code remain byte-identical to the pre-R1 source.

Unity-supported assembly reload and compilation APIs used by the unchanged profiler were rechecked against the current Unity 6 scripting reference.

### Pending Unity validation

- [ ] R1 imports with no C# errors or new warnings.
- [ ] One natural armed code-edit reload produces one complete copied report.
- [ ] The report identifies compilation/reload/post-reload gaps and every PixelSurface repair invocation without continuous logging.
- [ ] If a PixelSurface library is current, the report shows `needsRebuild=False` and no rebuild mutation.
- [ ] If a PixelSurface library is stale, the existing rebuild still succeeds and the report decomposes the existing work rather than changing it.
- [ ] After finalization, ordinary Editor updates produce no recurring R1 output.


## EDITOR-RELOAD-DIAG-R1 FIX2 — Unity 6 compilation-callback correction

**Status:** Gate 1 re-review complete after Unity 6000.5.0f1 obsolete-API warnings; Gate 2 corrective plan recorded before executable edits.

### Observed Unity warning

Unity 6000.5.0f1 reports `CompilationPipeline.assemblyCompilationStarted` as obsolete and recommends `compilationStarted`, `compilationFinished`, or `assemblyCompilationFinished`. The warning also states that compilation callbacks run asynchronously to the actual compilation and should not be treated as authoritative compilation-duration measurements.

### Objective

Remove the obsolete assembly-start hook and make R1's compilation evidence semantically accurate without changing the diagnostic's reload-boundary, PixelSurface-repair, controlled-recompile, or report-finalization behavior.

### Approved FIX2 files

Modify:

- `Assets/Docs/Ground_River_Regeneration_Orchestration_Manual.md`
- `Assets/Game/Rendering/PixelSurface/Editor/EditorReloadDiagnostics.cs`

Reviewed but intentionally unchanged:

- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs`

No other files are approved for FIX2.

### Invariants and non-goals

- Remove every R1 subscription/reference to obsolete `CompilationPipeline.assemblyCompilationStarted`.
- Retain `CompilationPipeline.compilationStarted` and `CompilationPipeline.compilationFinished` only as coarse event-order markers; do not describe their timestamp difference as actual compiler execution time.
- Retain `CompilationPipeline.assemblyCompilationFinished` only for per-assembly identity plus compiler error/warning counts; do not infer per-assembly compile duration.
- Retain `AssemblyReloadEvents.beforeAssemblyReload` / `afterAssemblyReload` as reload-boundary markers.
- Retain the existing `CleanBuildCache` forced-recompile action and label it non-representative for normal incremental compile performance.
- Do not alter PixelSurface repair scheduling, decisions, rebuilding, AssetDatabase operations, Ground, River, Foam, D9 spawning, C3A rendering, runtime behavior, or serialized assets.

### Implementation sequence

1. Remove the obsolete assembly-compilation-start subscribe/unsubscribe statements and delete the now-unused start handler.
2. Rename compilation timeline text where necessary so the report explicitly identifies these as Unity compilation **event notifications**, not authoritative duration probes.
3. Keep per-assembly finished-message counting intact.
4. Gate 4: compare against FIX1, scan the complete game C# tree with the interpolation-aware lexer, check preprocessor balance, search for obsolete R1 assembly-start references, search R1 for compiler-obsolete diagnostics where statically detectable, confirm the PixelSurface builder is byte-identical to FIX1, and package only the two FIX2-changed files.

### Acceptance criteria

- Unity produces no `CS0618` warning attributable to R1's use of `CompilationPipeline.assemblyCompilationStarted`.
- No R1 source reference to `assemblyCompilationStarted` remains.
- Compilation start/finish notifications remain in the consolidated timeline but are explicitly non-authoritative timing markers.
- Per-assembly compiler error/warning counts remain available through `assemblyCompilationFinished`.
- The R1 controlled capture, reload boundaries, PixelSurface instrumentation, bounded post-reload sampling, clipboard report, and failure-finalization behavior remain otherwise unchanged.
- Unity 6000.5.0f1 import remains the final compiler/warning authority.

### FIX2 implementation and Gate 4 static audit

**Source status:** implemented within the two-file FIX2 corrective scope. **Unity 6000.5.0f1 warning validation:** pending final import.

Implementation differences versus FIX1 are intentionally limited to `EditorReloadDiagnostics.cs`: the obsolete `assemblyCompilationStarted` subscription pair and `OnAssemblyCompilationStarted(...)` handler are removed; compilation-level start/finish messages are relabeled as event notifications rather than authoritative duration boundaries; `assemblyCompilationFinished` remains solely for assembly identity and compiler-message counts; the consolidated report includes the same timing caveat. `StylizedSurfaceDetailLibraryBuilder.cs` is byte-identical to FIX1.

Static validation after implementation:

- interpolation-aware lexical scan: `284/284` current game C# files report zero lexical/string/interpolation/delimiter errors;
- preprocessor balance: `284/284` current game C# files report zero unmatched conditional directives;
- repository search reports zero remaining `assemblyCompilationStarted` references under the current game source;
- R1 profiler compilation-pipeline references are limited to `compilationStarted`, `compilationFinished`, `assemblyCompilationFinished`, and `RequestScriptCompilation`;
- the PixelSurface builder SHA-256 matches FIX1 exactly (`a91ef84ad7407da83777a392823db1a2777f93cf646ad4818366b621a618e00c`);
- the FIX2 executable diff against FIX1 contains only the obsolete-hook removal and diagnostic wording/caveat changes described above.

Unity's official current scripting reference documents `compilationStarted`, `compilationFinished`, and `assemblyCompilationFinished` as compilation-pipeline events, and documents `RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache)` as a supported forced-recompile path. The user-observed Unity 6000.5.0f1 warning is the authority for treating these event timestamps as asynchronous notification markers rather than compiler-duration measurements.

#### Pending Unity validation

- [ ] R1 FIX2 imports with no C# errors and no `CS0618` warnings attributable to the reload diagnostic.
- [ ] Forced capture produces one consolidated report with compilation event markers, reload boundaries, PixelSurface repair timings, and no obsolete assembly-start event output.
- [ ] After capture finalization, no recurring R1 diagnostic output remains.

## EDITOR-RELOAD-DIAG-R1 FIX3 — delayed-work and Play Mode observation correction

**Status:** Gate 1 review complete from the user-provided R1 FIX2 capture; Gate 2 plan recorded before executable edits.

### Observed FIX2 evidence

Capture `db372ddb` completed in `65.533 s`. It recorded one PixelSurface repair schedule but zero `RepairAllLibraries` invocations and no R1 diagnostic `delayCall` callback. R1 finalized only because the bounded `120`-update ceiling was reached `3.403 s` after `afterAssemblyReload`. Therefore FIX2 stopped observing before the delayed work it was intended to measure.

The same capture spent `56.622 s` between arm/request and `beforeAssemblyReload` under the deliberately non-representative `CleanBuildCache` stress action. That result is useful as a full-rebuild stress measurement, but it is not evidence for ordinary incremental River-code compilation performance.

### Objective

Keep R1 alive until the post-reload delayed-work boundary has actually been serviced, include Play Mode transitions in the same capture, and make a natural incremental code-change/reload followed by Play Mode the primary reproduction path. Preserve the clean-build action only as an explicitly secondary stress test.

### Approved FIX3 files

Modify:

- `Assets/Docs/Ground_River_Regeneration_Orchestration_Manual.md`
- `Assets/Game/Rendering/PixelSurface/Editor/EditorReloadDiagnostics.cs`

Reviewed but intentionally unchanged:

- `Assets/Game/Rendering/PixelSurface/Editor/StylizedSurfaceDetailLibraryBuilder.cs`

No other files are approved for FIX3.

### Invariants and non-goals

- Do not alter PixelSurface repair scheduling, repair decisions, rebuild behavior, AssetDatabase operations, Ground, River, Foam, D9 spawning, C3A rendering, runtime behavior, or serialized assets.
- Remove update-count-based successful finalization. Editor update count is not a valid proxy for delayed-work completion.
- Successful post-reload finalization requires the R1 diagnostic `delayCall` to have executed.
- Track actual queued PixelSurface repair work across scheduling/defer/start boundaries so a capture does not finalize while a repair `delayCall` remains pending.
- Natural capture requires `EnteredPlayMode` before successful finalization so one report spans incremental compilation/reload through the user's slow Start-Game transition.
- Record all four `PlayModeStateChange` values when observed.
- Preserve compilation callbacks as notification-order markers only, never compiler-duration measurements.
- Keep the clean-build capture as a secondary stress test that does not require Play Mode.
- Use a wall-clock safety timeout after `afterAssemblyReload`; timeout finalization must explicitly report unmet readiness conditions rather than pretending the capture reached a quiet successful state.
- Observation heartbeats/editor-update-gap telemetry must not reset the meaningful-work quiet timer.

### Implementation sequence

1. Add persisted R1 state for Play requirement/EnteredPlayMode, latest `afterAssemblyReload` wall time, and pending PixelSurface delayed repairs.
2. Subscribe once to `EditorApplication.playModeStateChanged` and record `ExitingEditMode`, `EnteredPlayMode`, `ExitingPlayMode`, and `EnteredEditMode` during active capture.
3. Replace the natural arm action with an explicit incremental-reload-plus-Play capture mode. Retain the clean-build capture as a clearly labeled stress action.
4. Replace the 120-update termination with readiness checks: reload boundary seen, diagnostic `delayCall` serviced, current-domain PixelSurface schedule serviced when one was observed, no queued PixelSurface repair remains, Play entered when required, and a short meaningful-work quiet window.
5. Add a `180 s` post-reload wall-clock safety timeout and sparse observation telemetry for long editor-update gaps/heartbeats without mutating the meaningful-work quiet timestamp.
6. Extend the consolidated report with Play requirement/EnteredPlayMode, diagnostic-delayCall state, pending PixelSurface repairs, and timeout/readiness evidence.
7. Gate 4: compare against FIX2, run the interpolation-aware scan and preprocessor balance over the complete game C# tree, search for obsolete callback use and removed update-count finalization, verify only the approved profiler/document changed, verify the PixelSurface builder remains byte-identical to FIX2, inspect the packaged bytes, and leave Unity import/runtime capture pending.

### Acceptance criteria

- R1 cannot successfully finalize merely because many Editor updates occurred.
- A normal natural capture remains active until its post-reload diagnostic `delayCall` has executed, observed PixelSurface delayed repair work has been serviced, Play Mode has been entered, and meaningful diagnostic activity reaches the quiet window.
- A queued/deferred PixelSurface repair prevents successful finalization until the queued work starts and clears.
- If readiness never arrives, the report survives for up to `180 s` after the latest `afterAssemblyReload` and then finalizes explicitly as a safety timeout with the unmet conditions visible.
- The timeline records Play Mode state changes and any multi-second gaps between post-reload Editor updates.
- Clean-build remains available but is labeled as a stress test and does not require Play Mode.
- No PixelSurface repair behavior or gameplay/runtime subsystem changes.
- Unity 6000.5.0f1 import and one natural incremental-reload-plus-Play capture remain the final validation authority.

### FIX3 implementation and Gate 4 static audit

**Source status:** implemented within the approved two-file FIX3 scope. **Unity 6000.5.0f1 import and live capture validation remain pending.**

Implementation replaces the FIX2 update-count cutoff with readiness-based completion. A successful natural capture now requires: a completed assembly reload boundary, the diagnostic `delayCall` to have been serviced in the final domain, any PixelSurface delayed repair scheduled in that domain to have started and no queued repair to remain, `EnteredPlayMode` for the natural incremental workflow, and a `0.75 s` meaningful-work quiet window. The capture uses a `180 s` wall-clock safety timeout after the latest `afterAssemblyReload`; timeout reports readiness state rather than claiming successful settling.

PixelSurface schedule instrumentation now tracks queued delayed repairs without altering the builder: a newly scheduled repair increments the diagnostic pending count, an already-scheduled request establishes at least one pending repair when necessary, and `RepairAllLibraries` start consumes one pending item. The pending count is cleared at `beforeAssemblyReload` because callbacks queued in the outgoing domain cannot execute afterward. The PixelSurface builder itself is byte-identical to FIX2/FIX1.

The natural action is now explicitly an incremental-reload-plus-Play capture. The existing clean-build action remains available as a clearly labeled stress test and does not require Play Mode. All observed `PlayModeStateChange` values are recorded. Post-reload update counts are retained only as observational telemetry; sparse heartbeats and editor-update-gap records do not reset the meaningful-work quiet timer.

Static validation: `34/34 PASS` before this audit record was appended. The checks prove exact changed scope versus FIX2; zero strengthened lexical/string/interpolation/delimiter errors across all `286` current C# files; balanced conditional-compilation directives across all `286`; zero `MaximumPostReloadUpdates`, `MinimumPostReloadUpdates`, or `assemblyCompilationStarted` references; natural/clean capture mode separation; Play Mode subscription/readiness; diagnostic-delayCall readiness; pending-repair accounting; `180 s` timeout; non-activity observation telemetry; report readiness fields; no AssetDatabase mutation calls in the profiler; byte-identical PixelSurface builder SHA-256 `a91ef84ad7407da83777a392823db1a2777f93cf646ad4818366b621a618e00c`; and byte-identical protected Ground, River, Foam runtime/cache, D9 birth-event, Foam compute, and C3A render sources relative to FIX2.

Official Unity 6 documentation was rechecked for the APIs relied on by FIX3: `EditorApplication.delayCall` is a one-shot callback after Inspector updates; `EditorApplication.playModeStateChanged`/`PlayModeStateChange` provide Editor Play Mode transition notifications; and `RequestScriptCompilationOptions.CleanBuildCache` explicitly performs a full script rebuild while the default request path recompiles only changed/affected scripts. Compilation callback timestamps remain notification markers only per the Unity 6000.5 warning already observed in FIX2.

#### Pending Unity validation

- [ ] FIX3 imports with no C# errors and no new warnings attributable to R1.
- [ ] One natural incremental code-edit capture records compilation/reload, services the diagnostic delay callback, observes the automatic PixelSurface repair path, records `ExitingEditMode` and `EnteredPlayMode`, and produces one consolidated report only after readiness/quiet or explicit safety timeout.
- [ ] The natural capture does not finalize in Edit Mode merely because Editor updates are frequent.
- [ ] If PixelSurface repair is deferred/rescheduled, the report retains a nonzero pending count until the subsequent invocation begins.
- [ ] After finalization, ordinary Editor operation produces no recurring R1 update/heartbeat output.
