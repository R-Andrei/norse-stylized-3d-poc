# River Foam Stage 6 Architecture

## Purpose

Define the canonical Stage 6 architecture for the river's stylized floating surface film.

The target is not generic whitewater, bubble foam, scattered streaks, or a shader-painted mask. It is one persistent surface material that can gather into broad broken sheets, long contour ribbons, medium branches, temporary connectors, enclosed dark-water pockets, peeling strips, and detached fragments while preserving substantial open water.

This document owns the detailed Stage 6 implementation and acceptance contract. `River_Rendering_Roadmap.md` carries only the concise milestone summary.

Latest approved decisions take priority over earlier Stage 6 plans. In particular, topology is now a **soft lifespan influence**, not a binary occupancy permission map and not a direct picture that material must copy.

## Current Status Snapshot

Accepted and retained:

- the shared `64 / 96 / 128` structural resolution tiers, with `96` as the standard/default tier;
- the thin Shore Support strip following the instantaneous Stage 3 shoreline;
- the stationary attached Lee Support region;
- the Stage 6 Pressure Support remap, including the fail-closed geometry-supported upstream envelope that removed unsupported forward shelves;
- separate support and negative-influence diagnostics;
- water-level-aware Obstacle Footprint derived from the actual transformed generated mesh;
- field-based, fixed-cost GPU infrastructure, chunking, sleeping, freezing, and resource lifecycle support;
- permanent per-phase Foam profiler instrumentation;
- per-river staged initialization, with one explicit initialization phase advanced per `LateUpdate` and Foam remaining disabled until complete readiness;
- queued post-ready boundary/obstacle rebuild scheduling and dirty-event coalescing.

Implemented for current visual validation, but not yet accepted:

- deterministic continuous static Major Support Patch 1.

Not yet implemented or accepted:

- monotonic downstream Major Support movement;
- Connector Support;
- replacement Pocket Aging Pressure;
- combined Major + Connector + Pocket validation;
- any final topology-to-material response;
- final foam death, fragmentation, dissipation, and rendering behaviour.

The earlier class/family/candidate-retry Major generator is no longer canonical. Connector remains disabled and Pocket remains provisional. Performance work beyond the accepted staged initialization and dirty-rebuild queue is paused until the topology pipeline is complete.

Obstacle Footprint is sufficiently accurate for the present topology work. Pixel-perfect sub-cell obstacle geometry is explicitly deferred until the material solver proves it is visually necessary.

## Reference Read

The strongest reference traits are:

- a small number of long, eye-traceable structures;
- broad connected film regions broken by medium and large dark-water pockets;
- strong organisation by banks, rocks, constrictions, and sheltered downstream regions;
- a hierarchy of major sheets, medium branches, thin connectors, torn edges, and small debris;
- substantial open water between structures;
- structures that deform, merge, split, open holes, peel, and disappear locally rather than moving as one synchronized conveyor;
- convincing paused composition, with motion adding evolution rather than hiding weak topology.

Small fragments and hairline strands are secondary detail. They must not become the dominant visual identity.

## Canonical Separation of Responsibilities

Stage 6 has three distinct layers of responsibility:

1. **Topology influence** describes where foam should age more slowly or more quickly.
2. **Persistent material state** records the foam that actually exists and carries its own lifetime, amount, and structural history.
3. **Material and rendering processes** transport, split, merge, fragment, dissipate, and display that material.

These responsibilities must not be collapsed into one procedural mask.

Topology must never directly:

- create visible foam merely because a support area exists;
- erase visible foam merely because a negative-influence area appears;
- hide or reveal material as a binary switch;
- repaint a previously torn region because a support field remains high;
- become a final shader-generated foam picture.

A topology influence may move, grow, weaken, or disappear. Existing foam responds only through its own lifecycle and other approved material processes.

## Canonical Terminology

The Stage 6 topology names describe lifecycle influence rather than hard occupancy:

| Canonical name | Meaning |
|---|---|
| `Pressure Support` | anchored upstream/shoulder lifespan support derived from Static Pressure |
| `Lee Support` | anchored downstream lifespan support derived from the stationary lee source |
| `Shore Support` | anchored lifespan support in the thin moving shoreline strip |
| `Anchored Support` | the independent Pressure, Lee, and Shore support classes |
| `Major Support` | broad evolving lifespan support for dominant sheets and ribbons |
| `Connector Support` | narrower relational lifespan support between meaningful supported regions |
| `Pocket Aging Pressure` | evolving negative lifespan influence intended to encourage holes over time |
| `Obstacle Footprint` | water-level-aware solid cross-section used as geometry information and optional cheap negative/occlusion aid |
| `Support area` | an area that slows aging; it does not guarantee foam occupancy |
| `Negative-influence area` | an area that accelerates aging or supplies object-footprint information; it does not guarantee immediate emptiness |

Serialization and resource compatibility are preserved:

- the retained debug enum values remain `3`, `6`, `7`, and `8`;
- texture channel packing is unchanged;
- existing low-level shader property, compute-kernel, and compatibility resource identifiers may retain older internal names where renaming them would add risk without changing user-facing meaning;
- Inspector labels, enum member names, metrics, code comments, local variables, and canonical documentation use the terminology above.

## Topology Classes

### Anchored Lifespan Support

These are tied to accepted environmental causes and remain spatially stable except for their authoritative source motion:

- **Pressure Support** — upstream and shoulder support derived from accepted Static Pressure, tightened by the geometry-supported fail-closed envelope.
- **Lee Support** — attached downstream support derived from the accepted stationary Wake-source lee region.
- **Shore Support** — a thin inward strip following the instantaneous Stage 3 visible shoreline.

These classes are accepted.

They are not sources of new material. Their canonical role is to slow the aging of foam already present or transported into them.

### Evolving Lifespan Support

- **Major Support** — broad free-water support regions generated by one deterministic seeded continuous descriptor equation. Static generation is the active milestone; strictly downstream movement follows only after the static field is accepted.
- **Connector Support** — narrower relational support between meaningful positive regions. It is not yet implemented in the accepted pipeline and remains disabled.

Major is being completed first. Connector follows after static Major quality and downstream Major movement are accepted. Pocket still requires its own replacement pass afterward.

### Evolving Aging Pressure

- **Pocket Aging Pressure** — a negative lifespan field generated within broad Major interiors so foam there ages rapidly and can open a hole through fragmentation and dissipation.

Pocket Aging Pressure does not cut an immediate geometric hole. It creates conditions under which a hole can emerge over time.

### Object Footprint

- **Obstacle Footprint** — the current water-level cross-section of registered static geometry.

Obstacle Footprint remains a useful geometry diagnostic and a possible high-negative influence. It is not currently required to perform an expensive hard removal from every simulation process.

## Soft Foam Lifecycle Contract

### Material Owns Its Lifetime

Foam must carry persistent lifetime information. The implementation may remain field-based; this does not require one GameObject, particle object, or managed record per foam patch.

Conceptually, material carries at least:

```text
Amount
Remaining Life or Normalized Age
Integrity / Cohesion
Lightweight transported history as required
```

The exact RGBA channel assignment is not locked by this documentation pass. The existing `Amount / Freshness / Integrity / Phase` packing must be reassessed before Batch 2 because the accepted lifecycle now requires explicit persistent lifetime information.

New material receives a lifetime with controlled variation. During transport, its lifetime state moves with the material.

### Topology Modifies Aging Rate

Each material update applies a local aging rate:

```text
RemainingLife -= DeltaTime × LocalAgeRate
```

The canonical qualitative behaviour is:

- positive influence: aging slows substantially;
- neutral water: aging proceeds at the ordinary rate;
- negative influence: aging accelerates substantially.

Positive and negative fields remain separately available. They are not destructively composed into one binary support mask.

A provisional continuous response is:

```text
PositiveInfluence = max(Pressure, Lee, Shore, Major, Connector)
NegativeInfluence = Pocket

PositiveFactor = lerp(1, PositiveAgeMultiplier, PositiveInfluence)
NegativeFactor = lerp(1, NegativeAgeMultiplier, NegativeInfluence)
LocalAgeRate = PositiveFactor × NegativeFactor
```

Where:

```text
PositiveAgeMultiplier < 1
NegativeAgeMultiplier > 1
```

The exact formula, defaults, and authoring names remain subject to visual testing. The important contract is that both influences survive an overlap and both contribute to the final aging rate.

### Overlap Is Valid

Positive and negative topology may overlap.

An overlap is not automatically a generation error and must not be converted into:

```text
Positive × (1 - Negative)
```

Instead, overlapping influences compete through lifespan multipliers. Depending on tuned values, positive support may partially offset the negative pressure, the negative pressure may dominate, or the two may approximately cancel.

Relational generation is still required so the overlap is purposeful rather than statistically random.

### Topology Changes Do Not Pop Material

When a support area weakens or leaves a location:

- foam is not erased;
- its aging rate returns toward neutral or negative;
- it continues through its own lifecycle.

When a negative-influence area appears:

- foam is not instantly hidden;
- its remaining life begins decreasing faster;
- fragmentation and dissipation may intensify as it approaches death.

When a zone itself grows or decays, that field should also change gradually rather than switching identity across the entire river at once.

## Foam Death and Dissipation

Foam must not remain visually unchanged until one final frame and then disappear.

The accepted direction is an end-of-life transition in which:

- cohesion weakens;
- holes and cracks grow;
- strips and fragments separate;
- material amount begins to dissipate;
- rendering thins and fades the remaining fragments.

The previously discussed lifetime bands are illustrative, not fixed. The exact curve and thresholds must be judged in motion and may be revised.

Death cannot be purely cosmetic. The simulation must eventually reduce or disperse actual material so invisible expired foam cannot reappear later.

The renderer may add stable sub-cell breakup and fading, but it may not invent macro fragmentation unsupported by material state.

## Split and Merge Lifetime Behaviour

The field representation must preserve lifecycle history without tracking a permanent object graph.

- When material splits, transported lifetime state travels with the separated material.
- When material merges, lifetime may be combined through an amount-weighted rule or another conservative field operation.
- The exact merge rule remains provisional until Batch 2 implementation evidence exists.

No per-patch IDs, managed object lists, or evolving network graph are required.

## Cost-First Obstacle Handling

The visible rock or object already occludes most foam inside its footprint. Therefore Obstacle Footprint is not required to trigger a separate expensive hard-removal pipeline merely for hidden polish.

During Batch 2, choose the cheapest option that fits the existing simulation and render paths:

1. allow the ordinary negative aging response to clear hidden foam rapidly;
2. clip foam in rendering if that is already a cheap sample in the final shader;
3. leave hidden material untouched until it naturally ages if no cheaper visible benefit exists.

Do not add additional transport, storage-clearing, or sub-cell simulation work solely to perfect invisible foam beneath opaque geometry.

A hard simulation barrier may still be justified later if visible leakage, re-emergence, or transport through large obstacles proves to be a real defect. That is deferred polish, not a current prerequisite.

## Material Supply Contract

Topology does not create material.

Positive support may extend lifespan, but it must not directly fill a deficit or spawn foam because an area is empty.

The later Batch 2 supply system should use independently justified sources, with upstream inflow remaining the primary continuous source. Any additional replenishment, event source, or extinction-prevention source must be explicitly approved and rate-limited.

The following earlier ideas are no longer canonical:

- direct filling toward `structuralCapacity`;
- topology-driven deficit repair;
- unlimited anchored-support replenishment;
- connectors or pockets acting as material sources;
- random distributed births drawing the visible network.

## Persistent Evolving-Field Topology

The remaining free-water topology must use persistent fields rather than a pool of moving structure objects or a continuously maintained graph.

The required complexity is proportional to the number of field cells, not the number of detected structures:

```text
O(field cells)
```

Avoid:

```text
O(field cells × structures)
```

and avoid per-frame graph searches, dynamic adjacency, pathfinding, managed allocations, or per-primitive final-shader loops.

### Major Support Evolution

Major Support is one persistent scalar field stored as a ping-pong GPU state at the shared structural resolution. The raster representation remains fixed and bounded: each active descriptor contains three positive oriented lobes and one subtractive bite. The full-grid raster evaluates only those precomputed primitives and never rebuilds morphology per texel.

The canonical static generator is **deterministic, seeded, continuous, and fixed-cost**.

#### Stable identity and seed

Each possible descriptor is identified only by:

```text
Major Support Seed
longitudinal opportunity index
across-opportunity index
```

A compact unsigned integer hash expands that identity into independent deterministic values for activation rank, position jitter, size quantile, aspect ratio, orientation, spread, bend, imbalance, bite variation, and later movement parameters.

The same river geometry, obstacle state, topology settings, and `Major Support Seed` reproduce the same static Major topology. Changing the seed requests the existing queued Major-dependent rebuild chain rather than rebuilding synchronously.

#### Fixed opportunity lattice

The current Patch 1 lattice uses:

```text
2 stratified opportunities every 5 river metres
```

The two opportunities are distributed across the local usable width with independent deterministic longitudinal jitter. They are not fixed visual lanes. The lattice is independent of Amount and Size.

This gives the generator stable identities and a fixed upper work bound:

```text
3 nearby longitudinal segments × 2 opportunities = 6 descriptor checks per topology texel
```

The previous path allowed up to 24 checks and also performed candidate retries. Patch 1 removes both the retries and the larger lateral maximum.

#### Amount controls activation only

Every opportunity receives one stable activation rank `u` in `[0, 1]`.

```text
ActivationProbability = Amount × (2 - Amount)
Active = u < ActivationProbability
```

Consequences:

- `Amount = 0` disables Major Support;
- low nonzero values remain useful;
- increasing Amount adds descriptors without moving or reshaping already active descriptors;
- decreasing Amount removes the highest-ranked descriptors first;
- Amount does not alter physical size, orientation, or morphology.

There is no minimum-patch rule, class quota, parent guarantee, or fallback composition table.

#### Size uses one continuous heavy-tailed distribution

There are no small, medium, or large classes.

Each active descriptor receives one stable size quantile. The quantile is shaped by:

```text
q = uSize × sqrt(uSize)
```

and converted geometrically into a local scale multiplier. `Major Support Size` controls the global scale envelope only. Therefore the same active regions retain their positions and morphology while Size scales them continuously.

The distribution naturally produces:

- many small regions;
- a substantial middle population;
- occasional large regions;
- no class transitions, reclassification, or failed-large fallback path.

A resolution-derived minimum scale keeps the low end visible. If a location cannot fit that minimum at all, it is rejected independently of the authored Size value. This preserves patch count across Size changes wherever a meaningful descriptor is physically possible.

#### Continuous morphology

The three-lobe/one-bite descriptor is resolved from continuous parameters rather than named shape families.

- `Aspect` is logarithmically distributed around `1`, allowing long, compact, and laterally broad regions.
- `Orientation` is continuous and centre-biased, with a smaller rotation envelope for highly elongated structures.
- `Spread` moves the secondary lobes away from the centre and changes compact bodies into necked or extended regions.
- `Bend` curves both secondary lobes onto one side, producing crescents and hooks.
- `Imbalance` changes lobe sizes and offsets, producing shoulders, taper, branches, and one-sided mass.
- `Bite` is derived from the same morphology and cuts toward the concave side rather than selecting a separate family.

The raster equation remains:

```text
Body = max(Lobe0, Lobe1, Lobe2)
MajorTarget = Body × (1 - Bite × BiteStrength)
```

No family switch, archetype table, procedural contour noise, or per-texel seed decoding is required.

#### One direct capacity solve

Position, aspect, orientation, and normalized morphology are resolved once. Their projected longitudinal and lateral bounds are then known.

```text
CapacityScale = min(
    AvailableLongitudinalRoom / ProjectedLongitudinalBound,
    AvailableLateralRoom / ProjectedLateralBound)
```

The final scale is the authored deterministic scale clamped once by this capacity. There is no sequence of generate, validate, retry, shrink, reclassify, and regenerate.

The descriptor performs one centre availability sample against valid water, Obstacle Footprint, and Anchored Support. The full-grid raster still applies the authoritative domain, obstacle, and anchored masks per texel.

#### Persistence and cleanup

The persistent Major field still approaches the deterministic target with gradual growth and decay. Cleanup remains deliberately one-sided: it may remove isolated one-cell remnants and refresh the broad-interior helper channel, but it must not fill holes, expand support, blur gaps closed, or merge neighbouring regions.

Patch 1 is static. Downstream movement is a separate next milestone. It must reuse the same deterministic identity and satisfy all of these constraints:

- strictly positive downstream motion;
- different per-descriptor speeds and rhythms;
- no upstream phase or apparent upstream wrap;
- recycling only after a descriptor is fully outside the visible river domain;
- no reintroduction of per-texel lifecycle reconstruction.

Expected Patch 1 behaviours include:

- useful population throughout the Amount range;
- nested population changes when Amount changes;
- stable positions and count when Size changes;
- visible small, medium, and occasional large regions without discrete classes;
- lateral and diagonal distribution without a centre lane;
- compact, broad, necked, curved, hooked, and asymmetric silhouettes from one equation;
- substantial neutral water for future Connector Support;
- no startup hitch regression or shader warning.

### Connector Support Evolution

Connector Support is not yet implemented in the accepted topology pipeline. Its texture remains cleared/neutral and no connector generation should be treated as canonical.

Connector work begins only after:

1. deterministic static Major Support passes;
2. monotonic downstream Major movement passes.

The future connector design must remain relational and bounded. It may connect meaningful positive support on two sides of a neutral gap, but it must not paint green bodies inside broad red Major interiors, use Shore Support as an easy universal endpoint, create a maintained graph, or require unbounded searches.

Detailed connector equations are intentionally deferred until moving Major descriptors establish the real endpoint contract.

### Pocket Aging Pressure Evolution

Pocket Aging Pressure must be hosted by broad Major interior.

A pocket target is eligible only where:

- Major support is sufficiently broad;
- a positive rim can remain around the candidate;
- anchored Pressure Support, Lee Support, and Shore Support are protected;
- important connector cores are protected;
- the candidate lies within valid water.

The persistent Pocket field gradually approaches its target. It must not appear as an independent random negative mask in neutral water.

Because Pocket is a soft lifespan influence, perfect geometric containment is not required, but substantial overlap with a valid Major host is required for the field to have a meaningful purpose.

### Growth and Decay Drivers Are Not Extra Layers

“Birth” and “death” refer to operations on persistent topology state, not separate textures.

For each persistent class:

```text
CurrentState = previous persistent field
Target or Driver = deterministic function of river position, time, and accepted context
CurrentState gradually approaches the target through separate rise/fall rates
```

No dedicated Birth texture and no dedicated Death texture are required.

A deterministic driver may be evaluated procedurally. It should only be cached if profiling proves that caching is cheaper than re-evaluation.

### Asynchronous and Staggered Scheduling

Topology work runs more slowly than foam transport and is distributed across frames.

The current cadence contract is:

- Major evolution defaults to `2 Hz` and is authorable from `0.5–10 Hz`;
- Major cleanup defaults to `1 Hz` and is authorable from `0.5–10 Hz`;
- Connector targets refresh less often;
- Pocket targets refresh less often;
- persistent fields continue gradual rise/fall between target refreshes where practical;
- different chunks or regions may update on different frames;
- sleeping, frozen, distant, or inactive chunks perform no unnecessary topology work.

One compute dispatch may update many cells simultaneously without making their visual motion synchronized. Synchronization is prevented by local velocity, phase, growth, and decay variation—not by running cells on separate CPU timers.

## Accepted Anchored Inputs

### Shore Support

Shore Support is a thin strip measured inward from the instantaneous Stage 3 visible shoreline.

Current accepted validation width:

- `0.24 m` full influence;
- `0.03 m` inward fade.

Stage 3 and Stage 6 share the same shoreline evaluator. Stage 6 must not recreate a second approximate shoreline rhythm.

### Pressure Support

Pressure Support uses the accepted Stage 5 static Pressure field only as a candidate.

Stage 6 then:

- remaps the candidate into a narrow measurable boundary;
- intersects it with a fail-closed geometry-supported upstream envelope;
- lets only upstream-facing solid cells support a short row-local region;
- allows at most one penalised adjacent row for continuity on rotated or sloped silhouettes;
- scales bow depth modestly with local rock thickness;
- removes unsupported forward shelves;
- preserves contact near the object;
- leaves Stage 5 Pressure visuals and behaviour unchanged.

This accepted result must not be reopened while implementing Major, Connector, or Pocket topology.

### Lee Support

Lee Support uses the attached stationary Wake-source lee region.

It may extend downstream more artistically than Pressure extends upstream. It remains a positive lifespan influence and not a direct foam overlay or unlimited source.

### Obstacle Footprint

Obstacle Footprint is reconstructed from generated-source cached contours. Under the hard performance cap, automatic generated geometry uses a bounds-derived waterline contour at runtime; exact transformed mesh waterline extraction is deferred to editor-time or cached authoring.

Current implementation:

- reuses the disturbance runtime's mesh-derived waterline contour for each registered static obstruction;
- rasterizes that contour into the full structural-resolution Obstacle Footprint mask;
- uses one centre sample per candidate texel;
- point-samples the resulting structural-resolution mask;
- avoids convex hulls, bounds fallbacks, pressure envelopes, and padded disturbance footprints.

At `64 / 96 / 128`, the mask is a conservative grid approximation. Improving it with sub-cell cut-cell geometry is deferred until final material behaviour demonstrates a visible need.

Automatic Static Pressure support now consumes the cached footprint contour under the hard performance cap. It must not height-slice or rescan generated mesh triangles during Play startup. A future Static Pressure refactor may improve fidelity only through editor-time/cached compact geometry or by removing equal or greater runtime cost elsewhere.

## Diagnostics

The retained Foam views are:

| View | Meaning |
|---|---|
| `Final Foam (Debug Off)` | normal current rendered foam |
| `Anchored Support` | Pressure Support, Lee Support, and Shore Support shown independently |
| `Support Classes` | Major Support, Connector Support, and combined Anchored Support |
| `Negative Influence Classes` | Pocket Aging Pressure and Obstacle Footprint shown independently |
| `Support and Negative Influence` | combined lifespan support and combined negative influence, shown simultaneously without destructive composition |

Current colour encoding remains:

- `Anchored Support`: red Pressure Support, green Lee Support, blue Shore Support;
- `Support Classes`: red Major Support, green Connector Support, blue combined Anchored Support;
- `Negative Influence Classes`: red Pocket Aging Pressure, blue Obstacle Footprint, magenta overlap;
- `Support and Negative Influence`: green combined support, red combined negative influence, yellow overlap.

In `Support and Negative Influence`, yellow means both influences exist. It does not mean that either field has already erased the other.

Debug views point-sample the shared structural grid so stored boundaries are not hidden by additional bilinear display blur.

## Current Data Contract

Current implementation texture packing remains:

```text
Topology RGBA:
    R = Major Support
    G = Connector Support
    B = Pocket Aging Pressure
    A = derived structural-grid Obstacle Footprint

Anchored Sources RGBA (existing `_FoamTopologySources` resource):
    R = Pressure Support
    G = stationary Lee Support
    B = dynamic Shore Support
    A = reserved zero

Obstacle Footprint RHalf (existing `_FoamObstacleExclusion` compatibility resource):
    authoritative point-sampled current-water object footprint
```

This packing is an implementation detail. Channel positions remain stable while the semantic names above define their purpose.

The topology response must read support and negative-influence channels separately. It must not reduce them to one `Positive × (1 - Negative)` texture before lifecycle evaluation.

## Structural Resolution Policy

Stage 6 uses the same quality-scaled structural grid for persistent material, topology, guidance, and the authoritative Obstacle Footprint mask:

- `Low`: `64 × 64` cells per 32 m chunk region;
- `Medium`: `96 × 96`, standard/default;
- `High`: `128 × 128`.

Multi-chunk rivers extend the longitudinal dimension by chunk count while retaining the selected cross-river resolution.

The auxiliary fracture field may remain lower resolution if it is not authoritative topology.

Resolution changes spatial precision and cost. It must not change physical topology scale, flow direction, lifecycle rules, or authored metric widths.

## Progressive Initialization and Rebuild Scheduling

Stage 6 no longer performs the complete Foam bootstrap in one synchronous `EnsureResources()` call. The runtime advances one explicit initialization phase per `LateUpdate`. Resource loading, kernel resolution, allocations, clears, metric construction, boundary construction, obstacle construction, guidance, topology sources, Major, Pocket, composition, and initial measurements are distributed across frames. Foam remains disabled until the same complete readiness condition used by the former implementation is satisfied.

The accepted initialization safety contract is:

- no unrestricted catch-up loop;
- no more than one initialization phase per river per frame;
- domain or quality invalidation restarts from resource release rather than exposing partial resources;
- obstacle geometry must remain at the same version for a short stability window before the initial obstacle raster is accepted;
- disabling, freezing, sleeping release, or destruction cancels partial work through the normal resource-release path.

After readiness, boundary and obstacle changes no longer invoke a complete topology chain immediately. They set coalesced pending dependencies and advance through one queued rebuild phase per frame. Boundary rebuild, application to material state, obstacle stability/rasterization, source refresh, Major rebuild, cleanup, Pocket rebuild, and final composition are separate phases. Ordinary low-rate topology maintenance is suspended while this dependency chain is incomplete so maintenance cannot stack on top of rebuild work.

During the preparatory source-refresh phase, final topology output is directed to an existing scratch field while the authoritative source texture is refreshed. The previously bound final topology remains visible until the final composition phase replaces it. No new persistent texture or compute kernel is introduced by this scheduling change.

A global cross-river scheduler is deliberately deferred. The complete single-river pipeline and its final work categories must be accepted before multi-river arbitration is designed.

Further performance work is now formally paused after the accepted profiler instrumentation, staged initialization, and dirty-rebuild queue. Steady-state maintenance staggering, compute splitting, striping, jobs, and multi-river scheduling resume only after Major, Connector, Pocket, and combined topology validation establish the final dependency graph.

## Performance Contract

- Use shared fixed-cost fields.
- No continuously maintained topology graph.
- No permanent node set or pathfinding network.
- No GameObject or managed allocation per Major, Connector, Pocket, or foam patch.
- No final-shader loops over objects, structures, or sources.
- Remaining free-water topology must scale primarily with structural texel count.
- Major Support resolves a fixed-capacity deterministic descriptor buffer once per evolution tick. The Patch 1 raster checks at most six nearby descriptors per texel and must never loop over a growing structure collection.
- Descriptor generation uses one equation and one direct capacity solve. Candidate retry loops, class fallback trees, family tables, and repeated footprint scoring are prohibited.
- Connector Support remains disabled until its final bounded relational design is approved after moving Major Support.
- Foam runtime Obstacle Footprint rebuilds must never call exact mesh triangle baking. Runtime rebuilds consume generated footprint contours and rasterize a single centre sample per candidate cell; any return to 3x3 CPU sampling, exact mesh interval baking, or per-object triangle scanning is a performance regression.
- Automatic generated-source footprint and Static Pressure support refreshes must not read mesh triangles during Play startup. They use bounds-derived/cached contours unless exact data has already been authored or cached offline.
- Topology updates run at lower cadence than material transport.
- One explicit Foam initialization phase advances per river per frame.
- One queued boundary/obstacle rebuild phase advances per river per frame.
- Repeated boundary and obstacle notifications coalesce instead of creating duplicate rebuild chains.
- Obstacle rasterization waits for a stable `ObstacleGeometryVersion` window during initialization and ready-state rebuilds.
- Steady-state separation of Major evolution, cleanup, Pocket work, composition, and diagnostics is deferred by the topology-first performance pause.
- Anchored geometry data rebuilds only when its source changes.
- Static object geometry preprocessing is effectively one-time for the intended generated chunks.
- Distant, sleeping, frozen, or inactive chunks must stop unnecessary work.
- Quality tiers may change resolution and cadence, not fundamental behaviour.
- Profiling must report material states, topology states, temporary resources, dispatch cadence, active chunks, memory, and worst-case update spikes.

Deferred optimization notes:

- Foam Obstacle Footprint now waits for a short stable `ObstacleGeometryVersion` window and coalesces repeated ready-state changes before rasterization. Validate that this removes redundant startup and source-churn rebuilds without leaving stale geometry.
- Reuse the Foam Obstacle Footprint raster pixel buffer instead of allocating a fresh structural-grid `Color[]` per rebuild.
- Audit topology/debug work so `Final Foam (Debug Off)` does not trigger diagnostic-grade topology composition or metric refreshes.
- Tighten Static Pressure, Static Wake, and ripple-boundary dirty flags so source/profile changes rebuild only the affected textures and passes.
- Defer a global cross-river work scheduler until the accepted single-river pipeline exposes stable initialization, rebuild, maintenance, and feature-readiness work categories.
- Future Static Pressure fidelity work should consume shared compact geometry/footprint data from editor-time or cached authoring, not add a runtime mesh scanner.

## Public Controls

The currently accepted main Inspector controls remain:

- `Amount`
- `Web Granularity`
- `Network Evolution`
- `Major Support Amount` — default `0.56`, range `0–1`; activates a nested deterministic subset of the fixed opportunity lattice through `Amount × (2 - Amount)` and does not change existing descriptor positions or sizes
- `Major Support Size` — default `0.46`, range `0–1`; scales the same deterministic descriptors through one continuous heavy-tailed distribution and does not change their activation rank or position
- `Major Support Seed` — default `1`, non-negative integer; deterministically controls placement, scale quantile, aspect, orientation, spread, bend, imbalance, and bite
- `Major Evolution Rate (Hz)` — default `2`, range `0.5–10`
- `Major Cleanup Rate (Hz)` — default `1`, range `0.5–10`
- `Breakup Frequency`
- `Foam Speed`
- `Foam Colour`

The soft-lifecycle design also requires tunable base lifetime and support/aging-pressure response. Their exact public names, ranges, grouping, and defaults are intentionally deferred to the future lifecycle-authoring pass. Do not expose raw implementation coefficients before the lifecycle behaviour is visually proven.

## Implementation Sequence

### Completed / Accepted

1. Shared Stage 6 field infrastructure.
2. Structural resolution upgrade to `64 / 96 / 128` with `96` default.
3. Retained topology diagnostics.
4. Canonical soft-lifecycle terminology and channel compatibility.
5. Shore Support.
6. Lee Support.
7. Pressure Support, including the geometry-supported upstream envelope.
8. Current Obstacle Footprint representation.
9. Permanent profiler instrumentation.
10. Per-river staged initialization.
11. Queued/coalesced boundary and obstacle rebuild scheduling.

### Active

1. Deterministic continuous static Major Support Patch 1.
2. Validate seed reproducibility, nested Amount behaviour, Size stability, size hierarchy, lateral distribution, morphology, and cold-start performance.

### Next

1. Add strictly downstream Major Support movement using the same deterministic identity.
2. Implement Connector Support.
3. Replace Pocket Aging Pressure.
4. Test Major + Connector + Pocket together.
5. Integrate soft topology-to-lifetime response.
6. Implement and tune gradual fragmentation/dissipation at end of life.
7. Resume deferred performance work against the completed topology pipeline.
8. Finalise rendering, controls, and PC-first profiling.

Major, Connector, and Pocket must be handled one at a time. Do not batch all three into one opaque topology rewrite.

## Acceptance Gates

### Major Support

Pass only if the paused diagnostic shows:

- the same seed reproduces the same static topology;
- increasing Amount adds descriptors without moving existing descriptors;
- changing Size preserves active positions and count wherever the resolution-derived minimum physically fits;
- several broadly distributed positive regions at useful metre scales;
- independently controllable region amount and physical size;
- substantial neutral water between regions;
- slow asynchronous growth, decay, deformation, merging, and splitting;
- no visible search lattice, lanes, or bands;
- no synchronized conveyor motion;
- no systematic loss of regions merely because raw procedural preferences overlap Anchored Support or Obstacle Footprint;
- no dependence on a structure graph, tracked primitives, or per-primitive final-shader loops;
- stable scale across quality tiers, bends, width variation, chunks, and reverse flow.

### Connector Support

Pass only if connectors:

- occur between real positive support on two sides;
- do not begin or end in unsupported empty water;
- remain sparse and subordinate;
- evolve gradually rather than popping;
- avoid obvious obstacle and domain violations;
- do not require pathfinding or a persistent graph.

### Pocket Aging Pressure

Pass only if pockets:

- substantially overlap broad Major interiors;
- preserve a positive rim;
- avoid Anchored Support and important Connector Support cores;
- do not populate neutral water as unrelated random negatives;
- evolve gradually;
- act as negative lifespan pressure rather than instant geometric subtraction.

### Batch 2 Material Response

Pass only if:

- topology does not directly spawn or erase foam;
- support, neutral, and negative-influence regions visibly produce different aging rates;
- overlap produces a continuous competing response;
- existing foam survives topology changes according to its own remaining life;
- death does not pop;
- expired material actually dissipates from simulation state;
- downstream causality is preserved;
- 10-second and 60-second views retain comparable broad population and composition.

## Non-Goals

- Do not reopen accepted Stage 5 Pressure, Wake, or Impact Ripple visuals without a concrete integration defect.
- Do not integrate Impact Ripple-to-Foam behaviour in the current Stage 6 plan.
- Do not use a shader-only procedural mask to invent macro topology.
- Do not treat topology as a final occupancy picture.
- Do not make topology directly spawn, hide, reveal, or erase foam.
- Do not make banks, Pressure, Lee, Major, Connector, or Pocket unlimited emitters.
- Do not make distributed random births the primary visible shape source.
- Do not build a real evolving web graph, dynamic node network, or per-structure object conveyor.
- Do not add expensive hard obstacle handling unless a visible gameplay defect justifies it.
- Do not let tiny fragments become the dominant identity.
- Do not expose large sets of raw coefficients before behaviour is proven.
- Do not reintroduce discrete Major size classes, archetype-family switches, multi-candidate retries, or fallback composition tables unless the deterministic continuous model is explicitly rejected with measured evidence.

## Failure Rule

If the persistent field-based Major implementation fails its acceptance gate, stop and diagnose the field evolution, spatial scale, and relational rules.

Do not fall back automatically to a continuously maintained graph, per-structure object pool, or another independent-noise topology. Any representation change must be justified by measured evidence and explicitly approved.

## Stage 6 Completion

Stage 6 closes only after:

1. Anchored Support inputs and current Obstacle Footprint diagnostics remain accepted;
2. Major, Connector, and Pocket fields pass separately;
3. terminology and authoring are coherent;
4. soft topology-to-lifetime material response passes;
5. end-of-life fragmentation and dissipation pass;
6. final rendering and public controls pass;
7. PC-first profiling, quality, lifecycle, chunking, sleeping, freeze/thaw, reverse-flow, and long-running regression pass;
8. the roadmap is updated with conservative measured results.
