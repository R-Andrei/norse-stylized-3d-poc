# River Progressive Initialization and Work Scheduling Plan

## Document Status

**Status:** Retained performance architecture and progress record. Steps 1–3 are implemented and accepted. Further performance scheduling is intentionally paused while the topology pipeline is completed.

**Current code baseline:** per-river staged Foam bootstrap, permanent profiler instrumentation, and queued/coalesced dirty rebuilds.

**Primary implementation target:**

- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.cs`

**Current compute target:**

- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`

**Related architecture documents:**

- `Docs/River_Foam_Stage6_Architecture.md`
- `Docs/River_Foam_Topology_Implementation_Plan.md`
- `Docs/River_Rendering_Roadmap.md`

This plan exists because the current river runtime can concentrate too much CPU work, resource creation, GPU dispatch work, and first-use shader work into one frame. The resulting freeze has repeatedly returned when the Foam topology implementation became more complex, even when steady-state runtime cost remained acceptable.

The objective is not merely to fix one particular Major Support version. The objective is to make the complete river framework resistant to startup and rebuild hitches as future features are added.

The implementation must proceed slowly. Each step below is intentionally narrow, independently testable, and reversible. No later step should be started until the previous step passes its acceptance gate.

## Implementation Status Update — 1 July 2026

The rollout order has been deliberately revised after profiling the first working staged bootstrap. The single-river pipeline is not mature enough to justify a global cross-river scheduler yet. Multi-river arbitration is therefore deferred until the complete per-river initialization, dirty rebuild, steady-state topology, and feature-readiness pipeline is understood and accepted.

Current progress:

- **Step 1 — Instrumentation:** implemented and validated. The original hitch-free baseline showed approximately `29.5 ms` inside `RiverFoam.EnsureResources.Total` in one startup frame, with the individual boundary, obstacle, topology, clear, guidance, and diagnostic phases exposed separately.
- **Step 2 — Per-river staged bootstrap:** implemented and validated. Initialization now advances one explicit phase per `LateUpdate`; Foam remains disabled until complete readiness. The observed early phases were generally below roughly `2.5 ms` instead of recreating the former combined burst.
- **Step 3 — Dirty-event queue and obstacle rebuild coalescing:** implemented and accepted. Boundary and obstacle changes no longer execute a complete rebuild chain immediately.
- **Performance pause:** active. Steady-state maintenance staggering, compute-asset splitting, striped dispatch, CPU jobs, and global cross-river scheduling are deferred until Major Support, Connector Support, all four Negative Aging Pressure classes, static combined topology, runtime evolution, rebuild crossfade, and cache/preparation packaging are implemented and validated.
- **Current implementation milestone:** Patch 4.1 exact Obstacle Footprint is accepted together with the initial Interior Pocket proof. The approved next sequence adds Edge Cavities, Connector Weak Spans, and Free-Water Negative Events before static combined validation. Exact transformed-mesh interval preparation currently runs only in the staged pre-gameplay obstacle phase as a temporary fallback. Production ownership belongs to procedural chunk generation/building/linking after final object placement, with compact data cached for gameplay loading.
- **Global cross-river scheduling:** intentionally deferred. It will be designed only after the final one-river work categories, dependencies, interruption rules, and costs are known.

The current sequencing rule is now: preserve the accepted performance foundation, prove each topology slice visually before building the next one, keep every expensive proof-stage operation profiled and named, then resume performance work against the real completed pipeline. Do not invent scheduler categories for features that are still provisional.

---

## Performance Pause Decision — 30 June 2026

The first three performance steps solved the immediate architectural failure mode:

1. startup work is measurable by named profiler phase;
2. Foam initialization advances progressively rather than completing in one frame;
3. post-ready boundary and obstacle changes are queued, coalesced, and advanced one phase per frame.

Those protections remain permanent. They are not being reverted or bypassed.

Further performance engineering is paused because the free-water topology pipeline is not yet complete. The topology-only implementation now proceeds through the canonical slices in `River_Foam_Topology_Implementation_Plan.md`:

1. accepted field-first Major candidate and whole-river Major distribution;
2. accepted Connector Support;
3. accepted initial Interior Pocket and exact Obstacle Footprint;
4. Patch 4.2 Interior Pocket Amount and Edge Cavities;
5. Patch 4.3 Connector Weak Spans;
6. Patch 4.4 Free-Water Negative Events;
7. Patch 4.5 static combined topology validation;
8. strictly downstream class-specific topology evolution;
9. safe replacement-topology crossfade;
10. production procedural chunk/run cache and precompute packaging.

Topology-to-material aging response and final material lifetime integration remain separate Stage 6 work only after topology completion.

Topology-to-material aging response and final material lifetime integration remain separate Stage 6 work after the relevant topology outputs are accepted.

Optimising ordinary topology maintenance, splitting compute assets, striping full-grid kernels, or designing global multi-river arbitration before those dependencies are real would formalise provisional work categories and likely create avoidable rewrites. Performance work resumes only after the combined topology pipeline has a stable visual and dependency contract.

The pause is not permission to ignore cost. Every topology patch must still preserve these accepted constraints:

- proof-stage generation work is profiled with named markers and labelled as future cache/precompute work;
- full-grid kernels do not reconstruct complex topology grammar during steady-state gameplay;
- no unbounded candidate retries, path searches, or graph searches in gameplay; proof/bake searches must be bounded;
- no immediate synchronous rebuild path is reintroduced;
- cold-start profiling is mandatory after compute changes;
- startup hitch regressions block acceptance.

## 1. Executive Decision

The river framework will move from a monolithic, synchronous initialization model to a progressively scheduled work model.

The long-term architecture has four layers:

1. **Per-river staged initialization**
   - One river no longer allocates, clears, constructs, and dispatches every subsystem in one call.
   - Initialization is broken into explicit dependency-ordered work phases.

2. **Per-river dirty rebuild scheduling**
   - Boundary and obstacle changes enqueue dependency-ordered work instead of invoking a synchronous rebuild chain.
   - Repeated notifications coalesce, obstacle versions must settle, and only one queued rebuild phase executes per frame.

3. **Staggered steady-state maintenance**
   - Major, Connector, and class-specific negative evolution, composition, measurements, obstacle refreshes, and other periodic tasks are separated where dependencies permit.
   - Slow topology features do not need to complete every dependent pass in a single frame.

4. **Compiler and dispatch isolation**
   - Expensive compute subsystems are eventually separated into smaller compute assets.
   - Long structural fields are eventually processed by chunk or stripe when full-field dispatches become significant.

A global cross-river scheduler remains a valid future layer, but it is not part of the current numbered rollout. It must be designed from the accepted final per-river pipeline rather than from provisional work categories.

The first implementation passes will preserve the current visual result exactly. Progressive visual activation, compute-file splitting, striped dispatch, Burst jobs, and other larger changes are later steps.

---

## 2. Why This Work Is Required

### 2.1 Historical baseline: initialization was monolithic

Before Step 2, `StylizedRiverFoamRuntime.LateUpdate()` called `EnsureResources()` and resource initialization performed nearly the entire Foam bootstrap before returning. The list below records the measured baseline that justified the staged implementation; it no longer describes the current execution model.

The historical sequence included, in one synchronous call path:

1. releasing old resources;
2. loading `CS_RiverFoam.compute`;
3. resolving all compute kernels with `FindKernel`;
4. validating the river domain;
5. calculating chunk count and structural dimensions;
6. creating all material-state render textures;
7. creating all guidance and topology render textures;
8. creating shoreline and obstacle textures;
9. creating both fracture textures;
10. creating neutral fallback resources;
11. explicitly clearing topology textures;
12. allocating population and topology metric buffers;
13. allocating the Major Support descriptor buffer;
14. allocating active-chunk arrays;
15. building the metric buffer;
16. rebuilding and uploading the boundary texture;
17. rebuilding and uploading the obstacle-exclusion cache;
18. clearing four material-state fields;
19. clearing both fracture fields;
20. building the guidance field;
21. building the complete initial topology field;
22. measuring the initial population;
23. resetting all accumulators;
24. returning the runtime as ready.

The topology bootstrap invoked from this path is itself composite. `BuildTopologyField(0f)` currently performs:

1. refresh current shoreline and anchored topology sources;
2. update the obstacle mask;
3. compose source topology;
4. build or load Major Support;
5. build or load Connector Support;
6. build or load the active Negative Aging Pressure classes;
7. refresh shoreline and sources again;
8. compose the final topology field;
9. optionally measure topology diagnostics.

Even if each individual operation appears acceptable in isolation, concentrating all of them into the same frame creates a large worst-case spike.

### 2.2 First-use shader cost is separate from normal GPU cost

A compute kernel may be cheap once running but expensive when first loaded, compiled, translated, or turned into a GPU program by the graphics driver.

The recent failed Major Support versions demonstrated this distinction:

- steady-state topology cadence was low;
- the descriptor count remained bounded;
- nevertheless, a more complex kernel reintroduced a long Play-mode startup freeze.

Staggering ordinary dispatches cannot divide one pathological shader compilation into smaller pieces. Therefore the architecture needs both:

- work staggering for ordinary initialization and maintenance;
- strict kernel-complexity limits and later compute-asset isolation for first-use compilation.

### 2.3 Multiple rivers will multiply the problem

The intended world contains authored chunks and may contain several short rivers. A per-river solution that allows every river to perform one heavy initialization step per frame is insufficient: four rivers can still perform four heavy steps in the same frame.

A shared scheduler will eventually be required when multiple independent rivers or simultaneously activating chunks actually compete for work. It is deferred until the accepted per-river pipeline defines the real work categories and priorities.

### 2.4 Rebuild events can recreate startup-like spikes

The issue is not limited to entering Play mode. The current runtime can synchronously rebuild substantial work when:

- the river domain changes;
- quality changes;
- boundary data becomes dirty;
- generated geometry changes;
- `ObstacleGeometryVersion` changes;
- Foam is disabled and later enabled;
- a frozen river returns to active simulation;
- diagnostics request topology not already available.

The same scheduling system must eventually handle both startup work and dirty rebuild work.

---

## 3. Goals

### 3.1 Primary goals

The system must:

- eliminate large river-induced startup freezes;
- avoid concentrating all river initialization into one frame;
- allow river features to become ready gradually;
- preserve deterministic results;
- preserve current visual behaviour during the first scheduling patches;
- preserve the fixed-cost, shared-field architecture;
- provide enough profiling detail to identify exactly which phase causes any future hitch;
- coalesce repeated dirty events rather than rebuilding the same data repeatedly;
- stop inactive, frozen, sleeping, or distant river work when safe;
- remain suitable for low-to-medium-spec desktop hardware.

### 3.2 Secondary goals

The design should also make it easier to:

- initialize future procedural river chunks progressively;
- provide the measured dependency model needed for a later cross-river scheduler;
- prioritize visible or nearby rivers later;
- warm compute kernels in controlled order;
- update long rivers by structural chunk or stripe;
- isolate experimental Foam topology kernels from core material simulation;
- add future Connector Support without recreating a monolithic bootstrap;
- collect accurate worst-frame data rather than only average timings.

---

## 4. Non-Goals

The initial scheduling work will not:

- redesign Major Support shapes;
- add Major Support motion;
- enable Connector Support;
- redesign Negative Aging Pressure classes;
- integrate topology into material lifespan;
- change Foam rendering;
- change public Inspector controls;
- change the structural resolution policy;
- add a per-patch GameObject system;
- add managed records for each Foam structure;
- add a topology graph;
- move Unity texture creation or upload APIs to worker threads;
- immediately convert all CPU work to Burst jobs;
- immediately split every compute kernel into separate files;
- immediately stripe every full-field dispatch.

Those tasks remain separate and must not be bundled into the first scheduling patches.

---

## 5. Permanent Safety Rules

These rules apply to every implementation step in this document.

### 5.1 One behavioural category per patch

A patch must not simultaneously change:

- scheduling;
- visual generation;
- simulation mathematics;
- texture formats;
- public controls;
- descriptor layouts;
- compute-kernel grammar.

For example, a patch that introduces progressive initialization must not also change Major shapes.

### 5.2 Preserve the last known stable baseline

Before each step:

- save the exact two or more source files being changed;
- record the current expected startup behaviour;
- record the expected visual result;
- keep a direct rollback package.

### 5.3 No silent fallback to synchronous completion

If the progressive state machine encounters a phase that cannot run yet, it must remain pending. It must not call the old monolithic bootstrap as a convenience fallback.

### 5.4 No unbounded catch-up loops

Initialization work must not use a loop that continues processing phases until a time budget is exhausted unless that loop has a strict small iteration cap. A delayed frame must not cause the runtime to execute every missed phase immediately.

### 5.5 No hidden work in property getters or bindings

Shader binding, diagnostics display, and readiness queries must not trigger expensive allocation or rebuild work implicitly.

### 5.6 Expensive work must have a named profiler marker

Every phase capable of creating a visible frame spike must have an individual marker. “Foam Update” alone is not sufficient.

### 5.7 Cold and warm startup are different tests

Every compute change must be tested in at least two conditions:

- **cold first use:** after the compute shader or relevant C# script has recompiled;
- **warm reuse:** subsequent Play entry without changing the shader.

A kernel that only behaves acceptably after it has been cached is not automatically acceptable.

### 5.8 Future topology work is blocked by the scheduling gate

No further expansion of the topology generator or proof path should be considered complete until at least:

- instrumentation is present;
- per-river staged initialization is working;
- the startup test is repeatable;
- the heaviest phase can be identified from the Profiler.

---

## 6. Terminology

### Initialization phase

One dependency-ordered stage of bringing a river runtime from unallocated to ready.

### Work unit

A single schedulable operation. A phase may contain one work unit or a small sequence of demonstrably cheap work units.

### Heavy work unit

A work unit that can allocate large GPU resources, upload a structural texture, dispatch across the full structural field, or trigger first-use shader creation.

### Light work unit

A bounded operation expected to be negligible relative to one frame, such as updating a small amount of bookkeeping.

### Readiness level

A guarantee that a defined subset of resources contains valid, intentionally initialized data.

### Dirty dependency

A resource or field that must be rebuilt because one of its authoritative inputs changed.

### Coalescing

Combining several repeated dirty notifications into one eventual rebuild.

### Cold kernel

A compute kernel that has not yet been first-used in the current graphics-program state and may incur shader or driver setup cost.

### Warm kernel

A compute kernel whose first-use setup has already happened for the current runtime state.

### Stripe

A bounded longitudinal range of a structural field processed separately from the rest of the field.

---

## 7. Architectural Overview

The complete target is:

```text
River becomes active
        │
        ▼
Per-river initialization state machine
        │ submits one eligible work request
        ▼
Global river work scheduler
        │ grants a bounded frame token
        ▼
One initialization or maintenance work unit executes
        │
        ▼
Readiness state advances or dirty dependency clears
        │
        ▼
Renderer binds real or neutral resources according to readiness
```

The architecture separates three questions that are currently mixed together:

1. **What work must happen?**
   - Defined by the per-river state machine and dirty dependency graph.

2. **When may it happen?**
   - Defined by the global scheduler and frame budgets.

3. **What may render before it is done?**
   - Defined by readiness levels and neutral bindings.

---

## 8. Per-River Initialization State Machine

### 8.1 Replace the current meaning of `EnsureResources()`

The current method means:

> Ensure everything exists and is completely initialized before returning true.

The target meaning should become:

> Ensure initialization has been requested, advance at most the work currently permitted, and report the current readiness level.

The original monolithic method should not remain as a second code path.

A possible final API shape is:

```csharp
private bool EnsureInitializationRequested();
private bool TryAdvanceInitialization(RiverWorkPermit permit);
private bool IsMaterialReady { get; }
private bool IsTopologyReady { get; }
private bool IsFullyReady { get; }
```

The exact names are not approved by this document; they are illustrative. The implementation step must first inspect existing naming and choose the smallest consistent change.

### 8.2 Proposed initialization states

The state machine should be explicit rather than inferred from null texture checks.

```text
Inactive
ValidatePrerequisites
LoadComputeAsset
ResolveKernelHandles
ResolveDomainAndDimensions
AllocateMaterialTextures
AllocateGuidanceAndTopologyTextures
AllocateAuxiliaryTextures
InitializeNeutralResources
AllocateBuffers
BuildMetricBuffer
BuildBoundaryData
WaitForObstacleSourceSettle
BuildObstacleExclusion
ClearTopologyResources
ClearMaterialStateA
ClearMaterialStateB
ClearAdvectedState
ClearReverseState
ClearFractureA
ClearFractureB
BuildGuidance
BuildCurrentShoreEdges
ComposeAnchoredSources
BuildMajorDescriptors
RasterizeMajor
CleanupMajor
BuildPocket
ComposeFinalTopology
InitializeMetrics
Ready
Failed
```

Not every state must consume a separate frame forever. The key is that the states exist independently so profiling can prove which ones are cheap enough to combine.

### 8.3 Detailed phase table

| Phase | Main work | Depends on | Produces | Initial classification |
|---|---|---|---|---|
| Validate prerequisites | Confirm river, domain, hardware formats, Foam enabled | River component | Valid/failed decision | Light |
| Load compute asset | Load current compute resource | Supported hardware | Compute asset reference | Potential cold/medium |
| Resolve kernels | Resolve all kernel indices | Compute asset | Valid kernel table | Potential cold/medium |
| Resolve dimensions | Calculate chunks, resolutions, lengths | Valid domain | Field dimensions | Light |
| Allocate material textures | Create state A/B, advected, reverse | Dimensions | Material textures | Heavy allocation |
| Allocate guidance/topology textures | Create guidance, topology, sources, generated positive fields, aggregate negative field, and required class working resources | Dimensions | Topology textures | Heavy allocation |
| Allocate auxiliary textures | Create shore edges, obstacle, fracture A/B | Dimensions | Auxiliary textures | Heavy allocation |
| Initialize neutral resources | Create neutral disturbance fallback | Texture support | Safe fallback texture | Light/medium |
| Allocate buffers | Metrics, exact obstacle intervals, and accepted topology metadata buffers | Capacities | GPU/CPU buffers | Medium allocation |
| Build metric buffer | Build and upload river metric rows | Domain/dimensions | Metric buffer contents | CPU/upload heavy |
| Build boundary data | Build and upload valid-water mask | Domain/metric data | Boundary texture contents | CPU/upload heavy |
| Wait for obstacle settle | Observe disturbance geometry version stability | Disturbance runtime | Stable-enough version | No heavy work |
| Build obstacle exclusion | Prepare exact transformed-mesh solid intervals, evaluate current-water mask, and capture the topology-generation snapshot | Stable final placed sources, boundary | Cached interval buffers, obstacle texture, CPU scalar snapshot | Temporary CPU-heavy pre-gameplay preparation |
| Clear topology resources | Clear topology/source/generated positive/aggregate negative textures | Allocated textures | Known zero topology | Heavy group; may split |
| Clear material states | Clear each material-state texture | Allocated material textures | Known empty material | GPU dispatch per field |
| Clear fracture fields | Clear fracture A/B | Fracture textures | Known neutral fracture | GPU dispatch per field |
| Build guidance | Dispatch guidance kernel | Metric/boundary resources | Guidance field | Full-grid heavy |
| Build current shore edges | Dispatch shoreline evaluator | Metric data | Current shore-edge texture | Medium/full-width |
| Compose anchored sources | Update obstacle mask and compose shore/Pressure/Lee sources | Shore, disturbance, obstacle | Topology source field | Full-grid heavy |
| Build Major topology | Run accepted preparation generator or load cached result, then upload | Metrics, boundary, sources | Major Support and metadata | Temporary CPU-heavy proof work / cached production load |
| Build Connector topology | Run bounded relationship preparation or load cached result, then upload | Major, boundary, obstacles | Connector Support and metadata | Temporary CPU-heavy proof work / cached production load |
| Build Negative Aging Pressure | Run active class generators or load cached result, then upload aggregate field and subtype metadata | Major, Connector, anchored cores, obstacles | Aggregate negative field and class metadata | Temporary CPU-heavy proof work / cached production load |
| Compose final topology | Compose all current topology classes | All topology inputs | Final topology field | Full-grid heavy |
| Initialize metrics | Reset and optionally measure diagnostics/population | Ready fields | Initial counters | Optional/deferred |
| Ready | Mark complete and enter normal scheduling | Required phases complete | Runtime active | Light |

### 8.4 No visual activation changes in the first staged patch

The safest first version of staged initialization should keep the Foam renderer disabled until the same complete set of resources that currently defines readiness is available.

This means:

- work is distributed over frames;
- the river water remains visible;
- Foam appears when initialization completes;
- the visual result at completion remains identical to the baseline.

Progressively exposing partial Foam features is valuable, but it is a separate later patch. It should not be mixed into the first state-machine implementation because that would change both scheduling and rendering behaviour at once.

### 8.5 Later progressive visual activation

After the state machine is proven, readiness-aware binding may allow:

1. water with Foam disabled;
2. material Foam with neutral topology;
3. anchored topology becoming visible;
4. Major Support becoming visible;
5. Connector and Negative Aging Pressure work becoming visible;
6. diagnostics becoming available last.

Before that is implemented, the material contract must define neutral values for every not-yet-ready texture. The runtime must never sample uninitialized allocation contents.

---

## 9. Readiness Levels and Neutral Resource Contract

### 9.1 Proposed readiness levels

```text
None
ComputeReady
ResourcesAllocated
DomainDataReady
BoundaryReady
ObstacleReady
MaterialStateReady
GuidanceReady
AnchoredTopologyReady
MajorReady
DependentTopologyReady
FinalTopologyReady
FullyReady
```

These levels are cumulative guarantees, not just labels.

### 9.2 Required neutral resources

Any shader-visible resource that can be bound before its authored data is ready requires a defined neutral fallback.

Examples:

- material state: zero Amount/Freshness/Integrity/Phase;
- topology: zero positive support and zero aging pressure;
- topology sources: zero Shore/Pressure/Lee influence;
- Major: zero support;
- Pocket: zero aging pressure;
- Connector: zero support;
- obstacle exclusion: no exclusion unless fail-closed safety requires the opposite for a particular pass;
- disturbance source: existing neutral disturbance texture;
- fracture: neutral unfractured state.

The neutral meaning must be checked against each kernel and shader before progressive binding is implemented. A generic black texture is not automatically safe for every semantic.

### 9.3 Binding rules

- In the initial staged-bootstrap patch, continue to use `BindDisabled()` until `FullyReady`.
- In a later progressive-activation patch, bind real textures only when their readiness guarantee is satisfied.
- Never infer readiness solely from a non-null texture reference.
- Never render from a resource in the same frame it was merely allocated unless it was explicitly cleared or authored first.

---

## 10. Work Classification and Frame Budgets

### 10.1 Work categories

A simple classification is sufficient initially:

- **Light CPU** — bookkeeping, state transitions, small arrays.
- **Heavy CPU/upload** — boundary construction, obstacle rasterization, metric upload.
- **GPU allocation** — large RenderTexture or ComputeBuffer creation.
- **Light GPU dispatch** — sparse descriptor build or small one-dimensional pass.
- **Heavy GPU dispatch** — full structural-grid pass.
- **Cold-kernel risk** — first use of a kernel or newly changed compute asset.

### 10.2 Initial budget policy

The first implementation should use conservative count-based budgets, not a complex adaptive timing system.

Recommended initial rules:

- no more than one GPU allocation phase per frame;
- no more than one heavy CPU/upload phase per frame;
- no more than one heavy full-grid dispatch per frame;
- no more than one cold-kernel-risk dispatch per frame;
- light work may accompany one heavy unit only when profiling proves it is negligible;
- initialization must never execute an unrestricted “finish remaining phases” loop.

These are starting constraints, not final performance truths.

### 10.3 Why count-based budgets come first

A millisecond budget sounds more precise but can hide several problems:

- Unity main-thread API calls cannot always be interrupted once started;
- a shader-program creation stall may occur inside one dispatch call;
- elapsed CPU time does not directly report queued GPU cost;
- adaptive loops can accidentally execute several phases during a fast frame and produce a later GPU bubble.

After instrumentation is stable, measured budgets can supplement the simple token rules.

---

## 11. Deferred Global Cross-River Scheduler

### 11.1 Why it is a separate implementation step

Per-river staging solves one river performing all work at once. It does not solve several rivers each performing one heavy phase during the same frame.

The global scheduler must not be implemented merely because the bootstrap state machine now works. The complete single-river pipeline—including dirty rebuild interruption, steady-state topology scheduling, feature readiness, and eventual Connector/Pocket dependencies—must be accepted first. Building the scheduler from the current incomplete pipeline would formalize provisional work categories and likely require a rewrite.

This section remains a future design reference, not authorization for the next patch.

### 11.2 Initial scheduler behaviour

The first scheduler version should be deliberately simple:

- registered active river runtimes form a round-robin queue;
- one river receives the heavy initialization token per frame;
- light state transitions may proceed without the heavy token;
- a river that cannot use its granted token yields it;
- disabled or destroyed runtimes unregister safely;
- static scheduler state resets correctly when entering Play mode, including configurations where domain reload behaviour may differ.

### 11.3 No camera priority in the first scheduler

Visible or nearby river priority is useful later, especially for procedural chunks. It should not be in the first scheduler patch because it introduces dependencies on:

- camera selection;
- renderer bounds;
- visibility state;
- potentially multiple cameras;
- scene-view behaviour in the Editor.

The first scheduler should be fair and deterministic. Priority can be added after correctness.

### 11.4 Future priority order

A later scheduler may prefer:

1. the river currently visible to the gameplay camera;
2. a river near the player or camera;
3. a river needed for an active diagnostic view;
4. a newly activated river chunk ahead of the player;
5. background rivers;
6. off-screen or distant rivers.

This must be added only with explicit project-approved rules for camera and player ownership.

### 11.5 Proposed file boundary

The scheduler will probably justify one new C# file under:

- `Game/Procedural/Rivers/`

A likely responsibility-oriented name would be similar to:

- `StylizedRiverRuntimeWorkScheduler.cs`

The exact file name and whether the scheduler is static, service-based, or component-owned must be confirmed during that implementation step. This document does not authorize adding a hidden GameObject or component automatically.

---

## 12. Dirty Dependencies and Rebuild Coalescing

### 12.1 Current problem

The current runtime may rebuild immediately when inputs change. One important example is:

```text
ObstacleGeometryVersion changes
→ rebuild obstacle exclusion immediately
→ if topology debug is active, rebuild topology immediately
```

The code already acknowledges that generated disturbance sources may still be settling, causing redundant startup rebuilds.

### 12.2 Target behaviour

Dirty notifications should set flags and enqueue dependencies, not perform heavy work immediately.

Example:

```text
Obstacle source changed
→ mark ObstacleData dirty
→ wait for version to settle
→ rebuild ObstacleData once
→ mark AnchoredSources dirty
→ mark MajorValidation dirty if required
→ mark FinalTopology dirty
→ schedule each dependent phase separately
```

### 12.3 Proposed dependency categories

- `DomainLayout`
- `MetricData`
- `BoundaryData`
- `ObstacleData`
- `CurrentShoreEdges`
- `AnchoredSources`
- `MajorDescriptors`
- `MajorField`
- `MajorCleanup`
- `PocketField`
- `ConnectorField`
- `FinalTopology`
- `PopulationMetrics`
- `TopologyMetrics`
- `GuidanceField`
- `FractureField`

The implementation does not need a general-purpose graph library. A small explicit dependency table is preferable and easier to audit.

### 12.4 Obstacle-version settling

A safe initial coalescing strategy is frame-based rather than time-based:

1. observe a changed `ObstacleGeometryVersion`;
2. record it as the pending version;
3. wait until the same version has remained unchanged for a small fixed number of frames;
4. perform one obstacle rebuild;
5. if the version changes during the wait, restart the settle count;
6. if the version changes while a rebuild is pending downstream, coalesce it into one new pending rebuild.

The exact settle-frame count must be selected during implementation and exposed only as an internal constant unless authoring evidence justifies a public control.

### 12.5 Boundary changes

A dirty boundary should no longer call `BuildTopologyField(0f)` synchronously.

It should schedule:

1. boundary rebuild;
2. obstacle revalidation if obstacle masking depends on boundary validity;
3. guidance rebuild;
4. current-shore rebuild;
5. anchored-source composition;
6. generated topology validation or rebuild;
7. dependent topology rebuild;
8. final composition;
9. optional diagnostics.

Each dependency may occur on a different frame.

---

## 13. Steady-State Phase Staggering

### 13.1 Current topology maintenance grouping

When Major evolution or cleanup becomes due, the current runtime can perform, within one simulation iteration:

1. refresh dynamic topology sources;
2. evolve Major;
3. clean Major;
4. build dependent topology;
5. refresh and compose dynamic topology again.

This grouping is visually convenient but unnecessarily concentrated for fields that change slowly.

### 13.2 Target maintenance queue

A dependency-aware queue should spread slow topology work across frames:

```text
Frame N:     refresh current shore and anchored inputs
Frame N + 1: prepare or load Major Support
Frame N + 2: clean Major
Frame N + 3: build Pocket and, later, Connector targets
Frame N + 4: compose final topology
Frame N + 5: collect optional topology metrics
```

The precise delay is not important as long as:

- dependencies are respected;
- the visual field evolves gradually;
- no stale field is mistaken for current authoritative geometry;
- debug readouts report their age/readiness correctly.

### 13.3 Independent phase offsets

Periodic systems should not share the same accumulator phase by default.

Systems to offset include:

- guidance refresh;
- Major evolution;
- Major cleanup;
- Pocket update;
- future Connector update;
- fracture update;
- population measurement;
- topology metrics;
- obstacle-version checks.

A stable phase derived from river identity or registration order may be used later. The first steady-state scheduling patch may use explicit sequencing instead of randomized offsets for easier debugging.

### 13.4 Diagnostics must not force production-grade work

The current code contains a TODO noting that Final Foam may accidentally force diagnostic-grade topology composition or metric refreshes.

Rules:

- metrics run only when their diagnostic consumer is active or when a development profiling mode explicitly requests them;
- displaying one topology class should not force unrelated measurements;
- normal final Foam must not run reset/readback diagnostic kernels every topology tick;
- debug activation may schedule missing fields progressively rather than synchronously rebuilding all of them.

---

## 14. Compute Shader First-Use and Compilation Strategy

### 14.1 Staggering cannot repair an oversized kernel

If one kernel causes a long graphics-program creation stall, running it on frame 20 instead of frame 1 still freezes frame 20.

Therefore the scheduler is not permission to let kernels grow without limit.

### 14.2 Kernel-complexity contract

Hot or first-use-sensitive full-grid kernels must avoid unnecessary:

- deeply nested branches;
- large inlined helper trees;
- signed integer division and modulus;
- repeated hash and noise reconstruction;
- unbounded or compiler-unfriendly loops;
- excessive dynamic family decoding;
- rebuilding descriptor data per texel that could be precomputed sparsely.

The Major Support failure established a preferred direction:

- complex region construction belongs in the sparse descriptor-build pass;
- full-grid rasterization should remain a bounded primitive evaluator.

### 14.3 Controlled warm-up

After the scheduler exists, first-use dispatches may be ordered deliberately:

- clear kernel;
- guidance kernel;
- shoreline/source kernels;
- any remaining generated-topology upload/composition kernel;
- Major raster kernel;
- cleanup;
- dependent topology;
- material simulation kernels.

A warm-up dispatch must use valid minimal or neutral resources. It must not write visible production state unintentionally.

One kernel should be warmed per frame initially.

### 14.4 Compute asset split

The current `CS_RiverFoam.compute` contains core material simulation, topology, diagnostics, obstacle work, fracture work, and Major Support. A future isolation pass should divide responsibilities.

A possible target is:

```text
CS_RiverFoam_Core.compute
    clears
    injection
    advection
    simulation
    boundaries
    population
    fracture

CS_RiverFoam_TopologySources.compute
    current shore edges
    obstacle update
    anchored source composition
    topology composition
    topology diagnostics

CS_RiverFoam_Major.compute
    generated topology preparation/upload
    Major rasterization
    Major cleanup

CS_RiverFoam_Connectors.compute
    future Connector descriptor generation
    future Connector rasterization
```

Shared coordinate, river-metric, and sampling helpers should live in shared `.hlsl` includes rather than being duplicated.

This split is intentionally late in the rollout because it changes resource loading, kernel ownership, and binding code. It must not be combined with the first scheduler patch.

### 14.5 Benefits of the split

- editing Major does not force the complete Foam compute asset through the same first-use path;
- core Foam simulation remains isolated from experimental topology code;
- unused subsystems can remain unloaded until needed;
- the scheduler can warm one subsystem at a time;
- compiler failures are easier to localize;
- kernel inventories become easier to audit.

---

## 15. Structural Grid Striping

### 15.1 Why striping is later

Feature-level staggering removes most startup concentration for the current short river. Long rivers and multiple chunks may still make one full-field dispatch too large.

Striping should be implemented only after profiler evidence identifies full-grid dispatch cost as a remaining problem.

### 15.2 Range-aware dispatch contract

Eligible kernels should eventually accept a bounded longitudinal work range, conceptually:

```text
_FoamWorkStartX
_FoamWorkWidth
```

The scheduler can then process:

- one 32 m structural chunk per frame; or
- a smaller fixed-width stripe per frame.

### 15.3 Candidate kernels

Potential candidates include:

- material and topology clears;
- guidance construction;
- Major rasterization;
- Major cleanup;
- Pocket construction;
- future Connector rasterization;
- topology composition;
- population and topology measurements.

### 15.4 Determinism requirements

Striped processing must not alter results based on stripe order.

A kernel is safe to stripe when:

- each output cell depends only on immutable inputs for that update; or
- the required halo is explicitly provided; or
- the pass uses ping-pong state and does not read partially updated output from neighbouring stripes.

Cleanup or neighbourhood passes may need overlap/halo columns. These must be designed explicitly rather than assuming chunk borders are independent.

### 15.5 Completion state

A partially processed field must track:

- current generation/version;
- completed stripe range;
- source versions used;
- whether it is safe to bind as authoritative;
- whether a newer dirty event invalidated the partial work.

The first striped implementation should keep the old complete field bound until the full new generation finishes, then swap atomically.

---

## 16. CPU Work and Buffer Reuse

### 16.1 Boundary and obstacle work

`RebuildBoundaryTexture()` includes CPU-side construction and upload work. `RebuildObstacleExclusionCache()` temporarily includes exact transformed-mesh interval preparation plus one current-water mask readback for CPU topology generation. The latter is intentionally pre-gameplay proof plumbing and must move into procedural chunk generation/building/linking with cached compact output.

The order should be:

1. stagger them into separate frames;
2. add profiler markers;
3. reuse temporary pixel buffers;
4. only then consider Burst/Job System conversion if the measured CPU calculation remains significant.

### 16.2 Pixel-buffer reuse

Patch 4.1 removes the old contour `Color[]` raster/upload path. The temporary fallback retains reusable compact interval buffers plus one structural float snapshot used by CPU topology generation.

Production should serialize or otherwise cache the compact prepared cells/intervals per generated chunk or run after final object placement. Gameplay may evaluate the current water height against those intervals, but it must not reconstruct them from triangles.

### 16.3 Jobs and Burst

Pure geometry/raster calculations may later run on worker threads, but Unity object access and texture upload remain main-thread boundaries.

A future job-based flow could be:

1. capture immutable compact source data on the main thread;
2. schedule pure raster calculation;
3. poll completion in later frames;
4. upload the completed pixel buffer on the main thread;
5. enqueue dependent GPU work.

This introduces lifetime, cancellation, and domain-change complexity. It should not be attempted unless instrumentation proves the CPU raster calculation itself is a meaningful remaining spike.

---

## 17. Instrumentation Plan

### 17.1 Required profiler markers

The first code patch after this document should add markers without changing behaviour.

Recommended marker groups:

```text
RiverFoam.LateUpdate
RiverFoam.EnsureResources.Total
RiverFoam.Init.ReleaseOldResources
RiverFoam.Init.LoadCompute
RiverFoam.Init.ResolveKernels
RiverFoam.Init.ResolveDimensions
RiverFoam.Init.AllocateMaterialTextures
RiverFoam.Init.AllocateTopologyTextures
RiverFoam.Init.AllocateAuxiliaryTextures
RiverFoam.Init.AllocateBuffers
RiverFoam.Init.BuildMetricBuffer
RiverFoam.Init.BuildBoundary
RiverFoam.Init.WaitObstacleStability
RiverFoam.Init.BuildObstacleExclusion
RiverFoam.Init.ClearMaterial
RiverFoam.Init.ClearFracture
RiverFoam.Init.BuildGuidance
RiverFoam.Init.BuildTopology.Total
RiverFoam.Topology.RefreshSources
RiverFoam.Topology.BuildMajorDescriptors
RiverFoam.Topology.RasterMajor
RiverFoam.Topology.CleanupMajor
RiverFoam.Topology.BuildPocket
RiverFoam.Topology.Compose
RiverFoam.Diagnostics.MeasurePopulation
RiverFoam.Diagnostics.MeasureTopology
RiverFoam.Rebuild.BuildBoundary
RiverFoam.Rebuild.ApplyBoundary
RiverFoam.Rebuild.WaitObstacleStability
RiverFoam.Rebuild.BuildObstacleExclusion
RiverFoam.Rebuild.RefreshTopologySources
RiverFoam.Rebuild.RebuildMajor
RiverFoam.Rebuild.CleanupMajor
RiverFoam.Rebuild.BuildPocket
RiverFoam.Rebuild.ComposeTopology
```

Names may be adjusted to match project style, but every major phase must remain separately identifiable.

### 17.2 Runtime diagnostics

Development-only diagnostics should expose:

- current initialization phase;
- current readiness level;
- number of frames spent initializing;
- last phase duration on CPU;
- worst phase duration since activation;
- pending dirty dependencies;
- pending obstacle version and settle count;
- number of heavy scheduler permits received once a future cross-river scheduler exists;
- current stripe progress once striping exists;
- whether topology and metrics are running for production or debug reasons.

These values do not need to become public gameplay controls.

### 17.3 Optional threshold warnings

After baseline measurements are collected, development builds may warn when:

- one initialization phase exceeds a chosen CPU threshold;
- one river remains stuck in the same phase unexpectedly;
- a resource is bound before its readiness guarantee;
- obstacle geometry keeps changing and prevents settling;
- more than one heavy river phase runs in one frame once cross-river scheduling becomes relevant;
- a diagnostic metric path runs while debug is off.

Thresholds must be calibrated from real profiling. They should not be guessed and treated as platform guarantees.

---

## 18. Implementation Roadmap

The steps below are deliberately smaller than the earlier combined proposal.

## Step 0 — Planning Document

**Status:** this document.

**Files changed:** one new Markdown document only.

**Behaviour change:** none.

**Acceptance gate:** the plan is reviewed and accepted before code changes begin.

---

## Step 1 — Instrumentation Only

**Status:** implemented and validated.

### Purpose

Identify the true cost distribution of the current hitch-free baseline and provide permanent regression visibility.

### Files expected to change

- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.cs`

No compute file should change.

### Exact behaviour change

- add profiler markers around existing phases;
- optionally record development-only phase timings;
- do not change call order;
- do not change dispatch count;
- do not change resource lifetime;
- do not change rendering or simulation.

### Explicit exclusions

- no state machine;
- no scheduler;
- no new file;
- no kernel changes;
- no visual changes;
- no obstacle settling yet.

### Test

Profile:

1. cold Play entry after script/compute recompile;
2. second warm Play entry;
3. Foam debug off;
4. Support Classes debug on;
5. Low, Medium, and High quality;
6. current short river;
7. any available multi-chunk river.

### Acceptance gate

- startup remains hitch-free at the current baseline level;
- the dominant phases are visible separately;
- markers add no meaningful cost;
- no visual or simulation change occurs.

### Rollback

Revert the single runtime file.

---

## Step 2 — Per-River Staged Bootstrap, Renderer Disabled Until Complete

**Status:** implemented and validated.

### Purpose

Spread one river’s existing initialization sequence across frames while preserving exact completed output.

### Files expected to change

- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.cs`

No compute file and no new scheduler file yet.

### Exact behaviour change

- introduce explicit initialization state;
- replace monolithic completion in `EnsureResources()` with at most one heavy phase per frame;
- keep `BindDisabled()` until the old complete-ready condition is reached;
- preserve all existing resource formats and kernels;
- preserve the final initialized result;
- use the current river alone as the scheduler owner.

### Explicit exclusions

- no global cross-river coordination;
- no partial visual activation;
- no compute split;
- no dirty-event coalescing beyond what is necessary to keep initialization valid;
- no steady-state phase changes;
- no Major changes.

### Important implementation rule

The first version should prefer too many small phases over aggressive phase grouping. Cheap phases may be merged later from profiler evidence.

### Test

- observe initialization phase progression;
- verify no single frame performs the old full sequence;
- verify Foam appears only after complete readiness;
- compare final textures and diagnostics against baseline;
- enable/disable Foam repeatedly;
- change quality;
- trigger a domain rebuild;
- enter and exit full freeze.

### Acceptance gate

- no startup freeze;
- no null-resource errors;
- no uninitialized-texture flicker;
- final visual output matches the baseline;
- initialization completes deterministically;
- disabling during initialization releases safely;
- domain change during initialization restarts or invalidates safely.

### Rollback

Revert the runtime file to Step 1.

---

## Step 3 — Dirty-Event Queue and Obstacle Rebuild Coalescing

**Status:** implemented; focused validation pending.

### Purpose

Prevent source churn and boundary changes from causing immediate full rebuild chains after initialization, and reduce redundant obstacle rebuilding while generated disturbance sources are still settling.

### Files changed

- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.cs`
- this plan and the two canonical river/Foam status documents

No compute shader changed.

### Exact behaviour change

- ready-state boundary changes enqueue a dependency-ordered rebuild instead of calling the former monolithic topology helper;
- obstacle-version changes enqueue obstacle work instead of rebuilding immediately;
- both initialization and ready-state obstacle work require a small stable-version window before rasterization;
- repeated notifications coalesce into boolean pending dependencies rather than creating multiple queued copies;
- only one rebuild phase advances per `LateUpdate`;
- regular topology maintenance is suspended while the queued rebuild is incomplete, preventing scheduled maintenance from stacking on top of rebuild work;
- the currently bound final topology remains visible during the preparatory source-refresh pass by directing its temporary topology output to an existing scratch field;
- boundary rebuilds apply the new boundary to both material-state textures in a separate later phase;
- the former private synchronous `BuildTopologyField()` fallback is removed.

### Rebuild sequence

Boundary-dependent work:

```text
Build boundary data
→ apply boundary to material states
→ wait for pending obstacle stability when required
→ rebuild obstacle exclusion when required
→ refresh anchored/source topology into scratch
→ rebuild/load Major Support
→ rebuild/load Connector Support
→ rebuild/load affected Negative Aging Pressure classes
→ compose and measure final topology
```

Obstacle-only work may finish after obstacle exclusion when topology debug is inactive, preserving the prior production behaviour. When topology rebuild is required, it continues from source refresh through final composition.

### Explicit exclusions

- no global cross-river scheduler;
- no steady-state Major scheduling change;
- no compute split;
- no topology mathematics change;
- no Major shape, amount, size, or movement change;
- no disturbance-runtime optimization.

### Test

- allow initialization to finish and confirm the final result is unchanged;
- trigger one generated-source/boundary change and inspect one `RiverFoam.Rebuild.*` phase per frame;
- trigger several source changes in consecutive frames and confirm they collapse into one eventual chain;
- change `ObstacleGeometryVersion` repeatedly and confirm obstacle rasterization waits until the version is stable;
- test Support Classes debug both enabled and disabled;
- confirm ordinary topology maintenance does not run in the same frames as queued rebuild phases;
- confirm disabling, freezing, domain rebuilding, or destroying the river clears pending work safely.

### Acceptance gate

- no complete topology chain executes directly from `EnsureResources()` or an event callback;
- no more than one `RiverFoam.Rebuild.*` phase advances per river per frame;
- repeated source notifications do not cause repeated complete rebuilds;
- the final topology matches the pre-Step-3 result;
- stale obstacle data does not remain indefinitely;
- no null-resource errors or visible uninitialized data occur.

### Rollback

Revert the runtime file to Step 2 and restore the preceding document revision.

---

## Step 4 — Staggered Steady-State Topology Maintenance

**Status:** deferred by the topology-first performance pause. Do not implement until Major, Connector, Pocket, and combined topology validation are complete.

### Purpose

Spread periodic topology work across frames rather than grouping every dependent pass whenever Major evolution or cleanup becomes due.

### Files expected to change

- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.cs`

No compute change should be necessary for the first version.

### Exact behaviour change

- introduce an explicit topology maintenance queue;
- separate source refresh, Major evolution, Major cleanup, dependent topology, composition, and diagnostics;
- ensure Major evolution and cleanup do not automatically execute in the same frame;
- run metrics only when needed.

### Explicit exclusions

- no new topology classes;
- no Connector implementation;
- no Major shape or motion work;
- no stripe ranges.

### Test

- long observation of Support Classes debug;
- verify gradual updates remain visually acceptable;
- compare average and worst-frame timings;
- confirm Final Foam debug-off path does not run unnecessary metrics;
- confirm dirty dependencies interrupt or supersede maintenance safely.

### Acceptance gate

- no frame executes the entire topology chain by default;
- visual topology remains stable and understandable;
- metrics are no longer coupled to ordinary composition unless requested;
- worst-frame topology maintenance cost is reduced.

---

## Step 5 — Readiness-Aware Progressive Feature Activation

### Purpose

Allow Foam features to appear gradually rather than keeping all Foam disabled until every optional subsystem is ready.

### Files expected to change

Likely:

- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.cs`
- possibly the water/Foam binding code if neutral semantics are not already sufficient.

Any material or shader file change must be stated explicitly before implementation.

### Exact behaviour change

Possible sequence:

1. material state becomes available;
2. guidance becomes available;
3. anchored sources appear;
4. Major appears;
5. dependent topology appears;
6. metrics remain last and invisible.

### Explicit exclusions

- no topology mathematics change;
- no compute split;
- no Major redesign.

### Acceptance gate

- no uninitialized data is visible;
- each feature fades or appears acceptably;
- partial readiness never causes popping from garbage values;
- the fully ready result remains identical.

This step may be deferred if the all-at-once Foam appearance after a short progressive initialization is already acceptable.

---

## Step 6 — Compute Subsystem Split and Controlled Warm-Up

### Purpose

Reduce first-use compilation coupling and isolate experimental topology kernels from core Foam simulation.

### Files expected to change

- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.cs`
- existing `CS_RiverFoam.compute`
- new compute files under `Game/Rendering/Water/Resources/PS3DRiver/Compute/`
- shared `.hlsl` include files only if required to avoid duplication.

### Exact behaviour change

- split kernel ownership without changing shader mathematics;
- load assets progressively;
- warm one subsystem or cold kernel at a time;
- preserve all texture and buffer contracts.

### Explicit exclusions

- no Major visual redesign in the same patch;
- no descriptor-layout change;
- no public control change;
- no striped dispatch yet.

### Acceptance gate

- output matches the pre-split baseline;
- cold-start profiling identifies each compute subsystem independently;
- changing Major code no longer causes unnecessary core-kernel first-use work;
- no duplicate helper implementation drifts between files.

---

## Step 7 — Longitudinal Stripe Dispatch

### Purpose

Bound the cost of one full-field pass for long or multi-chunk rivers.

### Files expected to change

- runtime scheduling code;
- only the compute kernels selected from profiler evidence;
- possibly shared compute includes.

### Exact behaviour change

- add work-range parameters;
- process one chunk or stripe per allowed work unit;
- keep old complete output bound until a full new generation completes;
- swap atomically.

### Acceptance gate

- stripe order does not change final output;
- boundaries between stripes are invisible;
- neighbourhood kernels use correct halos;
- one long river cannot monopolize a frame;
- short rivers do not pay unnecessary complexity.

---

## Step 8 — CPU Buffer Reuse and Optional Jobs

### Purpose

Remove remaining measured CPU allocation or raster spikes.

### Likely sequence

1. reuse boundary/obstacle pixel arrays;
2. avoid repeated temporary allocations;
3. profile again;
4. jobify only pure heavy calculations that remain significant.

### Acceptance gate

- no correctness change;
- no unsafe Unity object access from worker threads;
- cancellation and domain invalidation are handled;
- upload work remains scheduled and measurable.

---

## Deferred Milestone — Global Cross-River Work Scheduler

This milestone has no current step number. Revisit it only after the single-river pipeline has accepted initialization, dirty rebuilds, steady-state topology maintenance, readiness semantics, and the actual Stage 6 dependency graph. At that point profiling with more than one real river/chunk must demonstrate frame contention before implementation begins.

Expected future files remain one runtime integration change plus one explicitly approved scheduler file under `Game/Procedural/Rivers/`. No hidden component or GameObject is authorized by this document.

---

## 19. Test Matrix

Every meaningful scheduling milestone should be tested against the following matrix.

### 19.1 Startup state

- cold shader/script state after recompile;
- warm second Play entry;
- Editor Play mode;
- standalone development build when practical.

### 19.2 River count

- one river;
- two rivers;
- several short rivers;
- rivers enabled simultaneously;
- rivers enabled progressively.

### 19.3 River size

- one chunk;
- several chunks;
- maximum practical test length within texture limits.

### 19.4 Quality

- Low;
- Medium;
- High.

### 19.5 Feature state

- Foam disabled;
- Foam enabled with no autonomous material;
- autonomous material active;
- Support Classes debug active;
- Final Foam debug off;
- full freeze;
- unfreeze;
- inactive/sleeping runtime.

### 19.6 Dirty events

- domain change during initialization;
- quality change during initialization;
- obstacle version changes once;
- obstacle version changes repeatedly;
- generated source added/removed/changed;
- boundary becomes dirty;
- river disabled or destroyed while queued.

### 19.7 Validation results to record

- worst CPU frame;
- worst phase marker;
- whether `Shader.CreateGPUProgram` or equivalent shader creation dominates;
- number of frames to full readiness;
- number of heavy river work units per frame;
- allocation count during initialization;
- visual readiness order;
- final texture dimensions and memory;
- final support-class result;
- whether diagnostics caused extra work.

---

## 20. Failure Diagnosis Guide

### Symptom: one frame still freezes, but only after a shader edit

Likely direction:

- cold graphics-program creation;
- one kernel is too complex;
- compute split or kernel simplification is required.

Do not respond by merely moving the dispatch to a later frame.

### Symptom: several medium spikes occur across initialization

Likely direction:

- one phase is still too broad;
- texture allocations or clears need finer work units;
- boundary/obstacle CPU work may need separation or reuse.

### Symptom: one river is fine, several rivers hitch

Likely direction:

- global scheduler token count is wrong;
- heavy work is bypassing the scheduler;
- some initialization happens before registration.

### Symptom: fields appear with garbage or flash

Likely direction:

- readiness guarantee is incorrect;
- an allocated texture was bound before clear/write;
- neutral fallback semantics are wrong.

### Symptom: topology updates late or in the wrong order

Likely direction:

- dirty dependency propagation is incomplete;
- a stale generation was marked current;
- composition occurred before its source field completed.

### Symptom: obstacle field rebuilds repeatedly at startup

Likely direction:

- geometry version settling is not working;
- the version source changes after each dependent operation;
- an event callback still calls rebuild directly.

### Symptom: average cost is low but periodic spike remains

Likely direction:

- evolution, cleanup, composition, and metrics are still sharing a frame;
- accumulators align periodically;
- full-field dispatch needs phase separation or striping.

### Symptom: scheduler stalls forever

Likely direction:

- a state has an unmet dependency with no failure transition;
- a queued river failed to yield or re-register;
- static scheduler state was not reset;
- a domain/source version changes continuously.

---

## 21. Regression Prevention Rules

To stop the freeze from repeatedly returning, future river patches must answer these questions before implementation:

1. Does this change add a new initialization phase?
2. Does it add a new texture or buffer allocation?
3. Does it add a new first-use compute kernel?
4. Does it increase one kernel’s compile complexity substantially?
5. Does it add a new full-grid dispatch?
6. Does it make an existing sparse pass run per texel?
7. Does it cause a dirty event to rebuild work immediately?
8. Does it align another periodic task with Major evolution or cleanup?
9. Does it add a diagnostic readback to normal gameplay?
10. Can it be scheduled separately without changing results?
11. Which profiler marker will expose its cost?
12. What is the direct rollback file set?

A patch that cannot answer these questions is not ready to implement.

### 21.1 Mandatory cold-start check for compute changes

Any modification to a compute shader used by the river must be tested immediately after shader recompilation. A warm-cache test alone is insufficient.

### 21.2 Mandatory visual/performance separation

A visual topology change and a scheduling/performance change must not be introduced together unless the visual change is impossible without the scheduling foundation and the user explicitly approves the combined risk.

### 21.3 Mandatory bounded hot kernels

The full-grid kernel must not reconstruct complex topology data that can be generated in the proof/bake path and consumed later as accepted fields, compact descriptors, or other bounded cached data. This rule applies especially to Major, Connector, Interior Pocket, Edge Cavity, Connector Weak Span, and Free-Water Negative Event topology.

---

## 22. Relationship to the Foam Topology Roadmap

`River_Foam_Topology_Implementation_Plan.md` is the canonical topology-only patch sequence. This scheduling document constrains how those patches execute; it does not redefine their visual or implementation order.

The accepted scheduling foundation now supports topology development:

1. profiler instrumentation;
2. per-river staged initialization;
3. queued/coalesced dirty rebuilds.

Accepted topology work:

1. field-first Major candidate and whole-river Major distribution;
2. Connector Support and its authoring/distribution refinements;
3. initial Interior Pocket proof;
4. exact transformed-mesh Obstacle Footprint with temporary staged preparation and future procedural chunk/run ownership.

Remaining topology slices:

1. Patch 4.2 Interior Pocket Amount and Edge Cavities;
2. Patch 4.3 Connector Weak Spans;
3. Patch 4.4 Free-Water Negative Events;
4. Patch 4.5 complete static topology validation;
5. strictly downstream Major evolution;
6. class-specific Connector and Negative Aging Pressure evolution;
7. safe generated-topology rebuild crossfade;
8. production procedural chunk/run cache and precompute packaging;
9. handoff to separate Foam-material work.

All four negative classes are preparation-time generators. Interior/edge host analysis, Connector span selection, and Free-Water opportunity curation may be expensive during the proof path, but ordinary gameplay may only perform bounded cached sampling, offsets, fades, strength changes, and recycling.

Every slice must expose a real result immediately. Candidate-local work may use one compact preview; every river-dependent result must be shown on the actual river. Diagnostics remain minimal and must not force production-grade work in `Final Foam (Debug Off)`.

Only after the topology sequence is stable does the deferred performance roadmap resume:

1. stagger ordinary steady-state topology evolution where profiling still justifies it;
2. isolate compute assets where cold first-use compilation remains a problem;
3. add striped full-grid processing where completed long-river workloads require it;
4. design global cross-river scheduling from the accepted final per-river work graph.

No performance task may be used as a reason to combine the positive and four negative classes into one opaque rewrite.

## 23. Deferred Decisions

The following choices should not be made until their implementation step:

- exact scheduler class/file name;
- static service versus scene-owned service;
- exact per-frame token counts after profiling;
- whether resource allocation is one texture per frame or grouped by subsystem;
- obstacle-version settle-frame count;
- whether partial Foam visual activation is worthwhile;
- which kernels require compute-asset separation first;
- which full-grid kernels require striping;
- stripe width and halo policy;
- whether camera/visibility priority is needed;
- whether CPU jobs are justified;
- whether timing thresholds should emit warnings in Editor only or development builds too.

Deferring these decisions is intentional. They depend on measurements from the earlier, safer steps.

---

## 24. Definition of Success

The scheduling architecture is successful when:

- entering Play mode does not produce a large river-induced freeze;
- cold shader first use is either acceptably bounded or isolated to a clearly identified kernel;
- several rivers initialize without stacking heavy work in one frame;
- each river’s initialization progress is observable;
- no resource is sampled before it is valid;
- domain and obstacle changes queue bounded rebuild work instead of performing complete synchronous rebuilds;
- Major, Connector, class-specific negative evolution, composition, and metrics are not routinely concentrated in one frame;
- long rivers can later be striped without changing results;
- future visual topology work has explicit cost markers and a cold-start regression gate;
- the final fully ready river matches the approved visual baseline unless a separate visual patch intentionally changes it.

---

## 25. Immediate Next Step

Major Support, Connector Support, the initial Interior Pocket proof, and Patch 4.1 exact transformed-mesh Obstacle Footprint are accepted.

The next implementation is **Patch 4.2 — Major-hosted negative topology**:

- introduce `Interior Pocket Amount` and preserve the accepted current Interior Pocket population at default `0.5`;
- introduce `Edge Cavity Amount`, default `0.5`;
- generate deterministic lopsided cavities that may breach one Major edge while preserving a viable positive remainder;
- retain subtype, host, and future evolution metadata;
- run all expensive host analysis and candidate selection only in staged preparation or explicit dirty rebuilds.

Patches 4.3 and 4.4 then add Connector Weak Spans and Free-Water Negative Events. Patch 4.5 validates the complete static topology. Runtime evolution, rebuild crossfade, and cache/preparation packaging follow before any Foam-material implementation.
