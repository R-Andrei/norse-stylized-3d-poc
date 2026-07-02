# River Foam Stage 6 Architecture

## Purpose

Define the canonical Stage 6 architecture for the river's stylized floating surface film.

The target is not generic whitewater, bubble foam, scattered streaks, or a shader-painted mask. It is one persistent surface material that can gather into broad broken sheets, long contour ribbons, medium branches, temporary connectors, enclosed dark-water pockets, peeling strips, and detached fragments while preserving substantial open water.

This document owns the detailed Stage 6 behavioural and acceptance contract. `River_Foam_Topology_Implementation_Plan.md` owns the step-by-step implementation, inspection, rollback, and completion sequence for Foam topology only. `River_Rendering_Roadmap.md` carries only the concise milestone summary.

Latest approved decisions take priority over earlier Stage 6 plans. In particular, topology is now a **soft lifespan influence**, not a binary occupancy permission map and not a direct picture that material must copy.

## Current Status Snapshot

Accepted static topology foundation:

- shared `64 / 96 / 128` structural resolution tiers, with `96` standard/default;
- Shore Support following the instantaneous Stage 3 shoreline;
- stationary Lee Support;
- geometry-supported Pressure Support;
- field-first Major Support candidate generation and whole-river distribution;
- accepted Major `Amount`, `Size`, `Size Variation`, and `Seed` controls;
- accepted Major-to-Major Connector Support with `Amount`, `Directness`, and `Length Preference`;
- Connector endpoint gates, Major clearance halos, bounded detour rejection, and soft participation/distribution balancing;
- the initial Interior Pocket proof hosted by broad Major interiors;
- Patch 4.2 Interior Pocket and Edge Cavity classes accepted for feature progression, with population coefficients still provisional;
- Patch 4.3 Connector Weak Spans accepted after visual validation;
- Patch 4.4 Free-Water Negative Events accepted after visual tuning, including a sparser default population and broader deterministic size variation;
- exact transformed-mesh Obstacle Footprint, distinct from padded Pressure/Lee disturbance footprints;
- separate support and negative-influence diagnostics;
- field-based GPU infrastructure, chunking, sleeping, freezing, and resource lifecycle support;
- permanent per-phase Foam profiler instrumentation;
- per-river staged initialization;
- queued/coalesced post-ready boundary and obstacle rebuild scheduling;
- complete removal of the superseded lobe grammar, stale nucleus resources, provisional old Pocket path, disabled old Connector path, unused fixture, obsolete broad Foam controls, and dependent compatibility code.

Approved negative-topology expansion before runtime evolution:

- rename the aggregate negative output concept to **Negative Aging Pressure**;
- retain the current closed Major-hosted class as **Interior Pocket**;
- retain accepted Interior Pockets and Edge Cavities, accepted **Connector Weak Span** from Patch 4.3, and the implemented **Free-Water Negative Event** from Patch 4.4;
- expose one independent `0–1` Amount control per negative class, all defaulting to `0.5`; Patch 4.2 exposes Interior Pocket and Edge Cavity Amount, Patch 4.3 exposes Connector Weak Span Amount, and Patch 4.4 exposes Free-Water Event Amount;
- make each Amount activate a nested deterministic opportunity subset and control population only;
- preserve subtype identity and future evolution metadata even when classes are packed into one aggregate negative field for diagnostics/output;
- Patch 4.5 combined static validation accepted for feature progression; all population and shape coefficients remain provisional until the final Foam material proves the complete visual result;
- Patch 4.6 lively single-instance Major Support movement and morphing, Patch 4.6.1 local recycle territories, and Patch 4.6.2 combined lifetime units accepted for feature progression; exact movement and lifetime tuning remains provisional;
- Patch 4.7A hosted Interior Pocket and Edge Cavity evolution and Patch 4.7A.1 host-space footprint/parity correction are implemented and visually accepted;
- Patch 4.7B independent Free-Water local-mask evolution and Patch 4.7B.1 positive-downstream/lifetime/prepared-anchor recycle correction are implemented and visually accepted;
- Patch 4.7C.0 canonical topology field-space consolidation is implemented and visually revalidated;
- Patch 4.7C.1 Connector endpoint ownership/path data and Weak Span attachment preparation is implemented and Unity diagnostics are accepted;
- Patch 4.7C.2 complete-only identity reconstruction, separate Editor/development parity, and one-dispatch combined reconstruction scheduling are implemented and visually accepted;
- Patch 4.7C.3 endpoint-driven Connector deformation and Weak Span path/tangent following are implemented; long-run Unity validation exposed permanent population drain because relationships covered only a subset of same-host anchor states and could not rebind to different Major pairs. Patch 4.7C.3.1 completes every bounded same-host anchor combination and adds a preparation-time replacement-relationship catalogue. Long-run validation then exposed unlimited live stretch and a hard preference for retaining the same still-valid pair. Patch 4.7C.3.2 adds an exposed relative stretch-break envelope and deterministic recycle turnover. Validation exposed strong relationship concentration on a small subset of Majors. Patch 4.7C.3.3 replaces hard degree limits and first-valid selection with a strong deterministic soft distribution bias and is visually/long-run validated.
- Patch 4.8A staged same-grid replacement preparation is implemented and Unity-verified: immutable replacement requests build Major, Connector, Pocket, and a private generated-topology texture in separate frames while the accepted active topology keeps evolving. Superseding requests cancel only replacement work; complete replacements activate atomically. An identical-replacement validation path prepares and discards a full replacement without touching the visible topology.
- Patch 4.8B safe topology transition is implemented and Unity-verified. Before a complete replacement activates, the runtime captures the fully resolved generated topology—including current Major, hosted-negative, Free-Water, Connector, and Weak Span evolution—into one immutable old-state texture. Old and new generated topology then crossfade for one bounded internal second, while live Pressure, Lee, Shore, and exact Obstacle Footprint sources remain outside the fade and are composed once. A later activation during an active fade first flattens the currently visible blend into a new old-state snapshot. Domain/quality changes hold the previous complete renderer bindings during staged initialization, then remap the captured old generated field into the new mapping by global river distance and lateral metres before fading.

Patch 4.9 cache/precompute status:
- Patch 4.9A versioned exact-value cache contract and deterministic round-trip proof are implemented and await Unity validation;
- Patch 4.9B stable content fingerprints and explicit cache building, Patch 4.9C cache-first initialization, and Patch 4.9D production/cold-start validation remain unimplemented;
- topology-to-material lifespan response;
- final fragmentation, dissipation, and rendering behaviour.

The canonical implementation order is topology first: complete all static topology classes, runtime evolution, rebuild crossfade, and cache/preparation handoff before beginning separate Foam-material work.

Temporary generation may remain expensive during staged pre-gameplay proof preparation, but every such cost must be profiled and labelled as future procedural chunk/run precompute work rather than silently normalized as active-gameplay runtime.

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
| `Negative Aging Pressure` | aggregate evolving negative lifespan influence from the four classes below |
| `Interior Pocket` | closed negative area hosted inside broad Major Support while preserving a positive rim |
| `Edge Cavity` | lopsided Major-hosted negative area permitted to breach one deliberate side |
| `Connector Weak Span` | local negative section hosted by an accepted Connector relationship |
| `Free-Water Negative Event` | sparse negative area in valid water that does not require positive-support hosting |
| `Obstacle Footprint` | authoritative water-level-aware exact solid cross-section of placed/generated object meshes |
| `Support area` | an area that slows aging; it does not guarantee Foam occupancy |
| `Negative-influence area` | an area that accelerates aging; it does not guarantee immediate emptiness |

Serialization and resource compatibility are preserved:

- retained debug enum numeric values remain unchanged;
- existing texture channel packing may continue to carry one aggregate negative field;
- low-level shader property, compute-kernel, and compatibility resource identifiers may retain older `Pocket` names where renaming them would add risk without changing behaviour;
- subtype identity and evolution metadata must remain available outside the aggregate packed field;
- Inspector labels, telemetry, code comments, local variables, and canonical documentation use the terminology above wherever compatibility does not require otherwise.

## Topology Classes

### Anchored Lifespan Support

These are tied to accepted environmental causes and remain spatially stable except for their authoritative source motion:

- **Pressure Support** — upstream and shoulder support derived from accepted Static Pressure, tightened by the geometry-supported fail-closed envelope.
- **Lee Support** — attached downstream support derived from the accepted stationary Wake-source lee region.
- **Shore Support** — a thin inward strip following the instantaneous Stage 3 visible shoreline.

They are not sources of new material. Their canonical role is to slow the aging of Foam already present or transported into them.

### Evolving Lifespan Support

- **Major Support** — broad free-water support regions produced by the accepted field-first generator from actual river context.
- **Connector Support** — narrower relational support between accepted positive regions. Current accepted generation is Major-to-Major; anchored endpoints remain a possible later extension only if justified.

Major and Connector are separate logical classes and retain separate stable identity/evolution metadata.

### Evolving Negative Aging Pressure

Negative Aging Pressure is an aggregate lifecycle influence, not one universal shape grammar. It has four classes:

- **Interior Pocket** — closed Major-hosted negative influence. It should preserve positive support around the perimeter.
- **Edge Cavity** — Major-hosted negative influence biased toward one side. It may open through one deliberate edge while preserving a useful positive remainder.
- **Connector Weak Span** — Connector-hosted negative influence applied to a short path section away from endpoint gates. It weakens a relationship locally without deleting the relationship itself.
- **Free-Water Negative Event** — sparse negative influence in valid water. It may affect stray Foam later even when no positive topology occupies the same location.

All four classes remain soft lifespan influence. None cuts an immediate geometric hole or directly erases future Foam material.

### Object Footprint

- **Obstacle Footprint** — the current water-level cross-section of registered placed/generated static geometry, prepared from exact transformed meshes and cached for cheap evaluation.

Obstacle Footprint remains independent geometry information. Padded Pressure/Lee disturbance footprints must never be reused as authoritative solid exclusion.

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
NegativeInfluence = max(InteriorPocket, EdgeCavity, ConnectorWeakSpan, FreeWaterNegativeEvent)

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

Major Support is the broad, persistent, positive free-water scaffold. It is not final foam artwork, a direct occupancy command, a connector network, or a pocket mask.

A valid Major region should usually be:

- one connected filled body;
- broad enough to host visible material;
- irregular at low and medium spatial frequencies;
- capable of shoulders, tongues, broad bays, waists, unequal sides, and directional asymmetry;
- sometimes difficult to name, while still reading as one intentional support island.

A Major library shape should normally avoid:

- enclosed holes;
- disconnected islands;
- extremely thin loops or branches;
- long skeletal ribbons as the dominant population;
- straight storage-boundary edges;
- cellular starburst damage;
- tiny punctures or high-frequency noise;
- repeated obvious primitive-stamp silhouettes;
- near-ellipse results with only decorative dents.

Connector Support is responsible for narrow relationships between separate positive regions. Negative Aging Pressure and its four classes are responsible for closed holes, edge cavities, Connector fragility, and sparse free-water aging events. Major must not be forced to perform those roles.

#### Retained runtime identity and placement contract

The current deterministic placement work remains useful and should survive the representation replacement.

Each potential Major opportunity has a stable identity derived from:

- `Major Support Seed`;
- longitudinal opportunity index;
- lateral opportunity index.

For unchanged river geometry, obstacle state, settings, cache, and seed, the same opportunities must resolve to the same cached shapes and transforms.

`Major Support Amount` continues to activate a nested deterministic subset. Raising Amount adds regions without moving or reshaping already-active regions. Amount controls population, not physical size.

`Major Support Size` controls the physical scale envelope of the same active opportunities. Size must not silently alter population identity or seed assignment.

A seeded low-discrepancy size sequence remains the preferred finite-population rule. A short river should reliably contain a useful spread of small, medium, and occasional large regions without discrete runtime classes, quotas, fallback trees, or repeated candidate fitting.

The accepted lateral-placement and orientation work should be retained:

- opportunities cover the local river width rather than collapsing to the centre lane;
- long-axis orientation may be downstream, diagonal, lateral, or nearly transverse;
- strongly lateral shapes may receive a smooth inward placement bias so they are not systematically crushed against a bank;
- local river width, valid domain, Anchored Support, and Obstacle Footprint remain authoritative capacity limits.

#### Why generation moves out of active gameplay

Several runtime-analytic representations were tested. They either lacked enough representational freedom or required more per-texel checks than justified.

The approved architecture now separates three concerns:

1. **Algorithm quality must be final-quality now.** The visual proof must use field-first generation, connected-component analysis, distance fields, rejection, relational connectors, and class-specific negative-region placement close enough to the intended final result that approval means something.
2. **Temporary execution may be expensive.** During the current proof-of-style phase, this generator may run during Foam initialization, an explicit Play-mode preparation phase, or a controlled pre-gameplay loading window.
3. **Active gameplay must remain lean later.** Once the visual model is accepted, the same generator should move into a cached per-river/per-run bake so gameplay loads or samples accepted topology data rather than regenerating it.

Temporary runtime/pre-gameplay generation is not the final performance contract. Any readback, CPU search, pathfinding, distance transform, candidate rejection, or full-field topology construction in this proof path must be marked as future cache/precompute work and must not become hidden steady-state gameplay cost.

#### Active field-first topology generator

The active generator begins with scalar fields and river-context maps rather than an ellipse, centreline, primitive union, binary random walk, or simple path-descriptor proof. The completed generator must produce Major Support, Connector Support, and all four Negative Aging Pressure classes, but implementation proceeds through the separately inspectable slices defined in `River_Foam_Topology_Implementation_Plan.md`.

The first slice proves one Major candidate independently. Whole-river context is introduced only when distribution is implemented. Connector and negative-class generation do not begin until their required host topology exists and is accepted.

The completed generator gathers topology-resolution context from the actual generated river:

- valid water and boundary coverage;
- Obstacle Footprint;
- current or representative Shore Support;
- Pressure Support and Lee Support;
- local width, width asymmetry, constriction, and metric cell spacing;
- flow direction, flow speed, reverse-flow state, and foam seed/settings.

Major candidates are produced through this field-first pipeline:

1. **Safe generation domain**
   - Work in a temporary high-resolution square field, initially around `96 × 96`.
   - Reserve a mandatory empty outer margin so the storage boundary can never become part of the silhouette.

2. **Broad correlated base field**
   - Create a seeded low-frequency scalar lattice, initially around `6 × 6` control values.
   - Smoothly interpolate it across the temporary field.
   - Add a weaker secondary lattice, initially around `11 × 11`, for medium-scale contour variation.
   - Avoid independent per-cell noise; nearby values must be correlated.

3. **Coordinate warp**
   - Sample the scalar field through a separate low-frequency seeded vector warp.
   - Warp produces directional sweep, offset shoulders, uneven waists, and broad hooks without building the shape from a skeleton.
   - Optional aspect bias may stretch or broaden the domain before sampling, but no ellipse is used as the source shape.

4. **Target occupancy threshold**
   - Choose a desired occupied-area percentile rather than one global arbitrary threshold.
   - Occupancy becomes a controllable source-library property and helps produce compact, medium, and broad candidates.

5. **Connected-component cleanup**
   - Keep the largest connected positive component.
   - Remove all smaller islands.
   - Fill enclosed holes because Major Support should remain one filled host body.

6. **Structural cleanup**
   - Close tiny accidental gaps.
   - Remove one- or two-cell spikes and isolated serrations.
   - Enforce a minimum neck width appropriate to the target stored resolution.
   - Preserve broad exterior bays; do not smooth every concavity away.

7. **Candidate rejection**
   - Reject any candidate touching the safe outer margin.
   - Reject shapes below minimum area or above maximum occupancy.
   - Reject excessive ribbonness, extreme thinness, insufficient neck width, or disconnected results.
   - Reject candidates too similar to an ellipse/convex oval when contour variation is below the accepted threshold.
   - Reject excessive high-frequency perimeter noise.
   - Request another deterministic candidate seed rather than weakening the failed shape into a bland fallback.

8. **Contour extraction and finish**
   - Extract the boundary with marching squares or an equivalent contour method.
   - Simplify redundant points.
   - Apply limited relaxation to remove pixel stair-stepping while retaining controlled angular character.
   - Fit the accepted contour inside the storage margin without forcing every shape to occupy the same bounding box or area.

9. **Major field rasterization**
   - Rasterize the accepted candidate into the river topology grid as soft support or signed-distance-derived coverage.
   - Initial target resolution should be tested at `32 × 32` and `64 × 64`.
   - Bilinear sampling, threshold controls, and later cache storage must not reveal the generation grid.

The field generator is intentionally capable of producing hard-to-name shapes, but rejection and metrics steer it toward coherent support islands rather than arbitrary damage.

#### Integrated composition selection

The proof generator must not merely scatter individually good shapes. It must judge the river composition as a whole.

Candidate selection should use a bounded deterministic scoring pass that considers:

- total Major coverage and required open-water ratio;
- longitudinal spacing and local crowding;
- lateral distribution across the actual river width;
- size hierarchy, including small, medium, and occasional large supports on short rivers;
- attraction to meaningful river causes such as banks, constrictions, Pressure shoulders, Lee regions, and sheltered object context;
- penalties for obstacle overlap, bank clipping, repeated silhouettes, excessive oval similarity, and too many parallel ribbons;
- preservation of accepted deterministic identity when Amount, Size, or Seed changes.

The first implementation may use greedy selection plus limited local replacement. It must not rely on a toy path grammar or on accepting every generated candidate in order. If this scoring is too expensive during visual proof, the cost is labelled as future bake/cache work rather than removed from the visual algorithm.

#### Connector Support generation

Connector Support is generated after the positive support context exists.

The generator should:

- identify meaningful endpoints from Major regions, Pressure Support, Lee Support, and Shore Support;
- ignore isolated weak noise and neutral water as connector endpoints;
- use a bounded cost field where obstacles are blocked or strongly discouraged, existing support is cheap, open water is possible but costly, and bank/object context may bias plausible paths;
- evaluate only nearby or otherwise meaningful endpoint pairs;
- rasterize accepted connectors as soft, narrow support that remains subordinate to Major;
- avoid painting broad connector bodies through Major interiors.

Pathfinding, distance transforms, endpoint clustering, and pair scoring are allowed in the temporary proof path because this work is intended for future precompute/caching. They are prohibited from steady-state gameplay once a production cache exists.

#### Negative Aging Pressure generation

Negative Aging Pressure is generated after the positive support context exists, but its four classes use different host rules.

**Interior Pocket** generation should:

- compute broad interiors from accepted Major Support;
- choose deterministic spaced local maxima through distance analysis;
- preserve a positive rim;
- avoid important Connector cores, strong Anchored Support cores, exact obstacles, and invalid water;
- use irregular soft masks rather than uniform circles.

**Edge Cavity** generation should:

- begin from an eligible broad Major host;
- bias the negative centre toward one deterministic side;
- allow one deliberate breach direction;
- preserve a useful positive remainder on the opposite side;
- reject cavities that consume almost the whole host, leave a useless sliver, or fragment the remainder implausibly.

**Connector Weak Span** generation should:

- remain associated with one accepted Connector identity;
- choose locations away from source/destination endpoint gates;
- align the weak area to a short section of the Connector path;
- support bounded multiple spans only on sufficiently long Connectors at high Amount;
- remain negative pressure over Connector Support rather than modifying the accepted relationship graph.

**Free-Water Negative Event** generation should:

- choose sparse deterministic opportunities in valid water;
- prefer neutral or weakly supported water without making positive overlap categorically invalid;
- avoid exact obstacles and strong Anchored Support cores;
- retain stable phase, downstream/lateral movement, allowed-span, recycle, growth, and shape-morph metadata; existing provisional fade selectors are not the approved ordinary-evolution mechanism;
- remain visually subordinate at its default density.

Patch 4.4 implements this through a deterministic metric-space lattice that is independent of structural-grid cell counts. Candidate activation prefers neutral, well-contained water first while preserving later positive-overlap opportunities. Exact obstacles and invalid water are rejected or clipped during preparation; strong live Pressure, Lee, and Shore cores remain protected by the existing composition pass. Accepted events retain stable class identity, transform, phase, drift, movement-span, recycle, growth, and current provisional fade selectors. They were initially static; Patch 4.7B replaces that state with one active local-mask instance, and Patch 4.7B.1 applies the approved downstream hop, finite lifetime, and preparation-time-anchor recycle lifecycle.

Each class exposes one independent `0–1` Amount control with default `0.5`. Amount activates a nested deterministic opportunity subset and changes population only. Class-specific maximums and spacing rules keep the defaults sensible without forcing equal counts or equal coverage.

All classes output soft negative lifespan pressure rather than immediate geometric subtraction. The aggregate negative field may use the maximum or another approved non-destructive combination, while class identity remains available for separate runtime evolution.

#### Deferred cache/library format

The final production form may use a project-wide reusable shape library, a per-river/per-run baked topology cache, or a hybrid of both. The active visual proof does not require this cache to exist before the generator is tested.

A reusable Major shape library, if retained after visual validation, may start between `1,024` and `4,096` accepted shapes.

Approximate uncompressed `R8` mask memory is:

| Library | Resolution | Raw mask memory |
|---:|---:|---:|
| 1,024 shapes | `32 × 32` | 1 MB |
| 4,096 shapes | `32 × 32` | 4 MB |
| 1,024 shapes | `64 × 64` | 4 MB |
| 4,096 shapes | `64 × 64` | 16 MB |

A `Texture2DArray` is the preferred first implementation because every entry has the same resolution and runtime selection becomes an integer layer index plus one filtered sample. An atlas is acceptable only if platform/resource constraints later justify the extra UV bookkeeping and border management.

The generated project asset or per-run topology cache should store:

- mask array or atlas;
- baked Major, Connector, and four-class Negative Aging Pressure fields when using per-river cache output;
- generator version;
- generator settings/profile;
- deterministic generation seed;
- accepted source seed per entry;
- metadata table per shape;
- checksum/version information sufficient to detect stale runtime metadata.

Final production generation should be explicit through an Editor command, build step, first-boot/run-preparation step, or loading-screen bake. Active gameplay must never regenerate or validate the topology library/cache.

#### Deferred shape metadata and library curation

If a reusable shape library remains part of the final production cache, each cached mask should record enough metadata to select a balanced population without inventing named runtime shape families.

Recommended metadata:

- occupied area;
- width/height and principal aspect ratio;
- orientation-independent compactness;
- perimeter length;
- convex hull area and convexity deficit;
- concavity depth/amount;
- minimum neck width;
- contour turn count or curvature distribution;
- oval similarity;
- centroid offset;
- safe-border distance;
- source seed.

Library curation should ensure the complete reusable set spans useful metric ranges:

- compact through broad;
- low through high concavity;
- mild through strong asymmetry;
- squat through elongated;
- smoother through moderately angular;
- small-perimeter/simple through richer contour structure.

This is population curation, not six hard-coded archetype classes. Runtime or bake-time selection may query metric bands or precomputed buckets, but the shape itself remains an accepted mask rather than a named procedural family.

#### Deferred production runtime/cache contract

If production chooses a cached-mask or descriptor/cache hybrid, a runtime Major descriptor should contain only data required to place and sample one cached mask, for example:

- global river distance and lateral centre;
- strength;
- conservative world bounds;
- local frame/orientation;
- longitudinal and lateral scale;
- shape-library index;
- mirror flags;
- optional affine shear parameter;
- threshold/edge offset;
- reserved deterministic drift parameters for later movement.

The final hot raster path should perform:

1. cheap bounds rejection;
2. transform the structural texel into descriptor-local coordinates;
3. apply mirror and affine scale/orientation;
4. map into cached-mask UV;
5. perform one filtered mask sample;
6. apply threshold/edge shaping and persistent Major response;
7. apply authoritative valid-water, obstacle, bank, and Anchored Support masks.

It must not perform:

- offline random-field generation;
- connected-component search;
- hole filling;
- contour tracing;
- repeated primitive evaluation;
- per-texel hashing/noise;
- candidate retry or validation loops;
- a growing loop over every cached shape.

The descriptor lookup must remain bounded by nearby opportunities exactly as the current sparse Major raster is bounded.

#### Cheap runtime variation for cached production data

A large cached library or baked topology set should provide most shape diversity. Runtime mutation is secondary and must remain inexpensive.

Allowed initial mutations:

- rotation through the descriptor frame;
- horizontal/vertical mirroring;
- non-uniform scale;
- limited affine shear;
- small threshold offset;
- small edge-width variation.

Possible later mutation, only if profiling permits:

- one very mild analytic UV bend/warp shared by all quality tiers.

Runtime should not destroy the accepted structural properties of the cached mask. Mutation must not create holes, disconnected islands, storage-boundary clipping, or unacceptably narrow necks.

#### Static integrated topology validation before movement

The static generated topology pass is accepted only if:

- same seed, river context, obstacle state, settings, and generator version reproduce the same topology;
- Amount changes composition predictably and preserves already-accepted identity where feasible;
- Size changes physical scale, coverage, or density predictably rather than causing chaotic population changes;
- short rivers reliably show small, medium, and occasional large regions;
- shapes occupy the river laterally as well as longitudinally;
- the population contains broad coherent support islands with substantial neutral water between them;
- no visible repeated primitive stamp, ellipse family, skeleton bias, cellular damage, or storage-grid boundary remains;
- no internal holes or disconnected islands appear in Major masks;
- warm runtime uses cached data and does not regenerate or validate library shapes;
- cold and warm Play entry remain within the accepted staged-initialization behaviour.

After static validation, runtime evolution is added using the same stable topology identity. Every ordinary movement step must make positive net downstream progress, while bounded lateral or diagonal displacement is allowed and encouraged.

#### Dynamic runtime topology evolution

The generator does not make topology static. It moves expensive topology **generation** out of active gameplay, while accepted topology evolves through cheap compact-state updates and batched low-resolution field reconstruction.

The river must read as lively, chaotic, and forceful. A semi-static painting or synchronized conveyor is unacceptable. Different regions use independent deterministic rhythms, positions, shape targets, and lifetimes.

##### Single-instance slot rule

Every accepted generated region owns one logical evolution slot. During ordinary evolution, that slot has exactly one active topology instance:

- no old/new duplicate pair;
- no ordinary fade between two support placements;
- no momentary double support that could inflate the later Foam population;
- no unbounded birth/death counter;
- no per-region GameObject or managed runtime object.

A movement transition interpolates the **same instance's** position, orientation, scale, and soft shape from its current state to its target state. Only that one interpolated state is rasterized at each topology update.

##### Major Support lifecycle

The provisional Patch 4.6 lifecycle is:

1. **Spawn/current state** — the Major is fully present at one valid placement.
2. **Dwell** — it remains at that state for a deterministic `2–5 s`.
3. **Move and morph** — over roughly `1–2 s`, the same Major moves to a new state.
4. **Commit** — the target becomes current and receives a new independently selected dwell.
5. **Repeat** — elapsed time and completed hops both consume one combined occurrence-lifetime budget until that budget or downstream egress is reached.
6. **Instant recycle** — the old occurrence is removed and the same slot reappears at one valid anchor inside its persistent local recycle territory in the same topology update. No fade or overlap is required.

Each movement must:

- advance a positive distance in canonical downstream coordinates;
- permit bounded left/right displacement, so motion may be diagonal rather than axis-locked;
- change shape every hop through cheap soft-mask warping, compatible retained shape variants, non-uniform scale, shear, and/or small rotation;
- use a different deterministic rhythm from neighbouring Majors;
- preserve broadly similar integrated support so size changes do not steadily inflate or collapse total support.

The exact dwell, movement, and lifetime coefficients remain provisional. Patch 4.6.2 replaces independent hop and second limits with `Major Lifetime Units` and `Major Lifetime Unit Deviation`. Both elapsed time and completed hops consume the same budget, so unusually slow occurrences cannot remain locally persistent merely by completing few hops and unusually active occurrences cannot pass through excessive topology states before recycling. The controls default to `6` units with `±2` deterministic variation; one normal dwell-plus-move cycle of about five seconds consumes approximately one unit. A derived maximum-duration safeguard remains internal.

Each Major retains a local recycle territory centred on its original accepted longitudinal position. `Major Recycle Territory Deviation (%)` controls the permitted offset on either side, with range `0–10` and default `3`. Near-egress originals shift their effective territory upstream enough to retain a useful movement runway, preventing repeated recycling inside the final part of the river.

##### Retained evolution data

The generator or future cache must retain enough compact data for cheap evolution, including as applicable:

- class and stable slot identity;
- one accepted soft local mask plus cheap compatible warp/variant data;
- current and target river-distance, lateral position, orientation, and scale;
- deterministic dwell, movement, combined lifetime-unit budget, and cycle selectors;
- local recycle anchors, original longitudinal home territory, and downstream egress limits;
- Major-host-relative data for Interior Pockets and Edge Cavities;
- accepted Connector polyline, endpoint identities, and normalized Weak Span positions;
- bounded prevalidated spare Connector relationships where later testing proves they are required.

Runtime evolution must not:

- perform shape generation, candidate search, connected-component cleanup, pathfinding, distance transforms, host search, or rejection loops during ordinary gameplay;
- move a free-water region upstream in the longitudinal coordinate;
- duplicate one logical support slot into simultaneous old and new support instances;
- treat topology as direct visible Foam;
- detach anchored Pressure, Lee, or Shore Support from their authoritative live sources.

Anchored Support remains attached to the live bank or object source and is not part of generated free-water evolution.

#### Prototype history and rejected directions

The following experiments informed the cached-library plan:

- **Three positive lobes plus one subtractive bite:** eventually achieved useful distribution and excellent lateral orientation, but silhouettes repeatedly exposed the same underlying stamp and size hierarchy remained weak.
- **Larger lobe/cut descriptor:** considered but rejected before adoption because it increased hot raster evaluations while continuing the same primitive-union strategy.
- **Seeded harmonic radial contour:** fixed-cost and elegant, but every shape remained star-shaped around one centre and read as a smooth blob/potato.
- **`16 × 16` cellular packed mask:** produced arbitrary connected structures, but also straight storage edges, pixel damage, holes, starbursts, and shapes that were strange in the wrong way.
- **Variable-width guided skeleton:** produced more coherent shapes, but overproduced ribbons, crescents, hooks, and internal circular cuts; it was line-first instead of area-first.
- **Deformed closed contour from an ellipse:** could produce interesting extreme cases, but moderate settings collapsed toward ovaloids because the ellipse dominated and safety retries damped aggressive candidates.
- **Family-biased/refined ellipse contour:** improved labels and population bookkeeping, but still produced mostly oval families near practical defaults; it was rejected rather than ported to Unity.

The active replacement is therefore the integrated field-first topology generator. It may be implemented directly in Unity/pre-gameplay proof plumbing so the actual river, obstacle, anchored-support, positive topology, and four-class negative topology can be judged before material work begins.

### Connector Support Evolution

Static Connector Support is implemented and accepted through Patch 3.5. Ordinary endpoint movement must not trigger gameplay pathfinding.

During normal Major hops:

- retain the accepted simplified Connector polyline;
- move path points by a source-to-destination blend of the two endpoint Major transforms;
- allow a small bounded transverse deformation so the Connector does not behave like a rigid straight elastic band;
- rasterize the deformed path in the same batched topology update;
- keep endpoint gates and Major-interior clearance behaviour intact.

When a connected Major instantly recycles upstream, the old relationship must not stretch across the chunk. Patch 4.7C must therefore use bounded **prevalidated** alternatives prepared before gameplay:

- retire the invalid old relationship in the recycle update;
- assign a valid spare relationship immediately when one exists;
- reassign the Connector Weak Span to a safe normalized position on the replacement path;
- permit a temporary Connector population dip when no valid spare exists rather than running runtime pathfinding or drawing an invalid route.

Patch 4.7C.3.1 implements the replacement proof without turning ordinary gameplay into a maintained graph. Preparation retains every bounded anchor-state combination for each accepted relationship and a bounded catalogue of additional prevalidated relationships between different individual Major slots. Runtime first preserves valid current assignments, then deterministically assigns unclaimed valid catalogue entries to unavailable Connector slots while enforcing unique Major pairs. Patch 4.7C.3.3 removes the per-Major degree ceiling and controls concentration probabilistically instead. A just-released relationship is skipped for the immediate replacement selection so a failed assignment is not reclaimed in the same update. Weak Spans remain attached to their logical Connector slots and therefore follow whichever prepared relationship that slot currently owns.

Patch 4.7C.3.2 completes the live lifecycle. Each assignment or prepared endpoint-anchor variant captures its current live path length as a reference. `Connector Break Stretch Ratio` (`1.10–2.00`, default `1.45`) is the sole break envelope: exceeding `reference × ratio` releases the relationship and performs one bounded prepared-catalogue selection that excludes the old Major pair. There is no absolute-metre allowance and no fallback to the relationship that just stretched beyond the limit. That pair remains blocked for the affected logical slot until either blocked host enters another occurrence, preventing next-tick reacquisition from redefining an already implausible long path as a fresh baseline.

When either endpoint Major begins a recycled occurrence and the old relationship remains viable, one stable deterministic decision is made for that Connector occurrence. Approximately half retain the pair; approximately half request another currently valid prepared pair. Requested turnover excludes the old pair, preserves unique Major pairs, and falls back to the still-viable old relationship only when no different prepared option exists. This creates relationship churn without gameplay randomness, pathfinding, or a continuously maintained graph.

Patch 4.7C.3.3 makes relationship distribution a soft probability rather than a hard topology class. Initial generation and runtime rebinding multiply candidate weight by `0.22` for every existing endpoint connection and by `0.60` for every degree level on the busier endpoint. Prepared geometric suitability and Connector Length Preference remain in the base weight. No degree is forbidden, so a sufficiently favoured or lucky candidate may still create an occasional hub. Duplicate active Major pairs remain disallowed. Crowding also raises recycle-turnover probability by `15%` per endpoint connection above degree one, capped at `90%`, but never breaks a viable relationship solely because it is popular.

No ordinary runtime route search, geometry validation, graph expansion, retry loop, candidate generation, or GPU readback is authorized. Temporary absence remains valid only when no currently applicable prepared catalogue entry exists; Connector population must not monotonically drain after Major recycle cycles.

### Negative Aging Pressure Evolution

Negative classes have distinct runtime contracts:

- **Interior Pocket** inherits its Major host's full world-space translation, diagonal displacement, rotation, broad scale, and instant recycle. It may add a small bounded host-local offset, scale, orientation, or soft-shape change on selected host hops, but must remain inside the safe Major interior.
- **Edge Cavity** inherits its Major host movement and recycle, remains attached to the same breach side, and may add smaller bounded local variation without crossing the host or destroying the positive remainder.
- **Connector Weak Span** remains at a safe normalized position on its owning deformed Connector, away from endpoint gates, and is reassigned with the Connector when a prevalidated replacement relationship is used.
- **Free-Water Negative Event** uses the same single-instance hop model independently, but more slowly: `5–10 s` dwell and `2–4 s` movement/morph. Every ordinary hop is net-downstream with bounded lateral displacement. A combined elapsed-time/completed-hop budget gives each occurrence a finite lifetime, and lifetime or downstream egress instantly selects one of that event's preparation-time validated upstream valid-water anchors. It may vary size more strongly and rotate modestly, but never creates a duplicate old/new instance or runs a gameplay placement search.

Hosted classes should not be perfectly rigid copies of their Major. Patch 4.7A retains each accepted Interior Pocket and Edge Cavity as a Major-local soft mask, carries it through the host transform and instant recycle, and applies bounded local scale, rotation, and offset changes only on selected host hops. Interior Pockets vary more often; Edge Cavities use tighter, boundary-tangent variation so the original breach side remains coherent. Patch 4.7A.1 requires the initial evolving field to preserve the accepted static footprint, including cavity influence beyond the Major support silhouette; any hosted region that cannot be prepared remains as a static fallback rather than disappearing. Connector Weak Spans use the Patch 4.7C.2 reconstruction foundation and Patch 4.7C.3.3 soft-balanced break/turnover-aware current-or-replacement-path/tangent following when complete prepared data is available, and otherwise retain the complete accepted static fallback; Free-Water Events now use their independent Patch 4.7B.1 lifecycle. Every local state is chosen inside absolute bounded limits rather than accumulated as an unbounded random walk.

No negative class directly cuts topology or material. No class performs distance transforms, host search, path search, candidate generation, or rejection during ordinary gameplay.

### Single-instance state is not a birth/death layer

Ordinary topology evolution requires no separate Birth texture and no separate Death texture.

Each slot stores compact current and target descriptors:

```text
Current transform + current shape state
Target transform + target shape state
Movement progress
```

During a move, the descriptor interpolates toward the target and one resulting mask is rasterized. At completion, target becomes current. At recycle, the descriptor is replaced immediately with a valid state from that slot's local recycle territory in the same topology update.

This ordinary evolution model must not be confused with the **explicit full-topology rebuild** path. Patch 4.8A preserves the accepted active topology while a replacement is prepared from one immutable request snapshot. Major, Connector, Pocket, and replacement upload advance in separate frames, and no partial replacement class becomes authoritative. Patch 4.8B captures the fully resolved generated old state immediately before complete replacement activation, then performs one bounded old-to-new generated-topology crossfade. Same-mapping replacements sample the captured old texel directly. Differently mapped domain/quality replacements retain the prior complete renderer bindings while initialization runs, then reproject the captured old generated field through global river distance and lateral metres using its retained metric rows. Live Pressure, Lee, Shore, and exact Obstacle Footprint sources are never duplicated inside the generated fade. This rebuild path is not used for routine per-region movement.

### Asynchronous and Staggered Scheduling

Topology updates remain much slower than persistent Foam transport, but they must be frequent enough to keep the river lively.

The provisional evolution cadence is:

- Major dwell: independently selected `2–5 s`;
- Major movement/morph: approximately `1–2 s`;
- Free-Water dwell: independently selected `5–10 s`;
- Free-Water movement/morph: approximately `2–4 s`;
- hosted Interior Pockets and Edge Cavities follow Major movement, with bounded local changes on only some host hops;
- Connector deformation follows endpoint Major movement; relationship reassignment happens only on invalidation/recycle and uses prevalidated alternatives;
- while any region is moving, begin with approximately `5` low-resolution topology reconstruction ticks per second;
- while every region in a class is dwelling, that class performs no unnecessary field reconstruction;
- different regions and chunks use staggered phases;
- sleeping, frozen, distant, or inactive chunks perform no evolution work.

The `5 Hz` active-movement rate is a provisional proof value, not a public control. It should be reduced if the later Foam material hides coarser topology stepping, or raised only if profiling and visual testing prove it necessary.

All changing regions of one class must be processed in a bounded batched operation. Do not dispatch once per region. One dispatch may update many cells simultaneously without synchronizing their behaviour because timing, target transforms, warp state, and lifetimes remain per-slot and deterministic.

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

Obstacle Footprint is authoritative water-level-aware solid geometry. It is no longer derived from the padded/bounds-based disturbance contours used by Pressure and Lee.

Patch 4.1 implementation:

- obtains the actual registered static obstruction `MeshFilter`s after the disturbance-source registry has settled;
- intersects conservative 3 × 3 samples inside each candidate structural texel with the exact final transformed mesh;
- stores compact vertical solid intervals relative to the base river surface;
- evaluates those cached intervals against the current Stage 3 water displacement through the existing `UpdateObstacleExclusion` compute kernel;
- accepts a texel only when all conservative samples lie inside the same exact solid cross-section, so coarse resolution may make the mask slightly smaller but must not create an exterior halo;
- captures one prepared obstacle snapshot for expensive Major/Connector/Pocket generation and continues to update the independently composed live Obstacle Footprint from the cached intervals without triangle rescans.

Pressure and Lee may continue to use padded artistic disturbance footprints. Those fields are not authoritative solid exclusion and must never be substituted for Obstacle Footprint again.

The current exact triangle scan is a temporary staged pre-gameplay development fallback. The production owner must be the procedural chunk generation/building/linking phase after all generated objects have their final transforms. That phase should cache the compact interval/cell data beside the chunk/run so ordinary gameplay only loads and evaluates it. This is not assumed to be an editor-only bake because object placement is procedural.

Automatic Static Pressure remains a separate accepted system and may continue using its cached contour representation until a later shared-geometry refactor is explicitly approved. That refactor should consume the same prepared compact solid source rather than introducing another mesh scanner.

## Diagnostics

The retained Foam views are:

| View | Meaning |
|---|---|
| `Final Foam (Debug Off)` | normal current rendered foam |
| `Anchored Support` | Pressure Support, Lee Support, and Shore Support shown independently |
| `Support Classes` | Major Support, Connector Support, and combined Anchored Support |
| `Negative Influence Classes` | aggregate Negative Aging Pressure and Obstacle Footprint shown independently |
| `Support and Negative Influence` | combined lifespan support and combined negative influence, shown simultaneously without destructive composition |

Current colour encoding remains:

- `Anchored Support`: red Pressure Support, green Lee Support, blue Shore Support;
- `Support Classes`: red Major Support, green Connector Support, blue combined Anchored Support;
- `Negative Influence Classes`: red aggregate Negative Aging Pressure, blue Obstacle Footprint, magenta overlap; isolate one negative class by setting the other three negative Amount controls to zero;
- `Support and Negative Influence`: green combined support, red combined negative influence, yellow overlap.

In `Support and Negative Influence`, yellow means both influences exist. It does not mean that either field has already erased the other.

Debug views point-sample the shared structural grid so stored boundaries are not hidden by additional bilinear display blur.

Topology implementation follows a minimal inspectability rule:

- an individual Major candidate may use one compact separate preview with `Raw Field`, `Thresholded`, `Cleaned`, and `Final Support`;
- every result affected by river width, banks, flow, obstacles, placement, connectors, negative classes, composition, or movement is inspected on the actual river;
- the existing topology views are reused instead of building a general-purpose debugger;
- telemetry is limited to attempted/accepted/rejected counts, dominant rejection reasons, coverage, and generation time for the active layer;
- temporary overlays are allowed only for one specific implementation question and are removed or compiled out when no longer needed;
- `Final Foam (Debug Off)` must not trigger diagnostic readbacks or diagnostic-grade topology work.

## Current Data Contract

Current implementation texture packing remains:

```text
Topology RGBA:
    R = Major Support
    G = Connector Support
    B = aggregate Negative Aging Pressure (legacy low-level Pocket channel name may remain)
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
- obstacle geometry must remain at the same version for a short stability window before exact interval preparation and current-water footprint evaluation are accepted;
- disabling, freezing, sleeping release, or destruction cancels partial work through the normal resource-release path.

After readiness, boundary and obstacle changes no longer invoke a complete topology chain immediately. They set coalesced pending dependencies and advance through one queued maintenance phase per frame. Boundary rebuild, application to material state, obstacle stability/exact preparation, and authoritative source refresh remain separate phases. Once those live sources are current, Patch 4.8A captures the settled obstacle scalar and prepares replacement Major, Connector, Pocket, and generated upload data outside the accepted active topology. Ordinary low-rate topology evolution may continue while replacement CPU preparation runs; it is blocked only during live maintenance or the final atomic activation frame.

Patch 4.8A adds one temporary generated-topology texture per in-progress replacement. It is populated with the complete static fallback result and is never bound while incomplete. Identical validation prepares and discards this texture without activation. Patch 4.8B changes activation ownership: immediately before the complete replacement becomes authoritative, one compute pass captures the current fully resolved generated topology into a private transition texture. The replacement then takes active ownership, rebuilds compact evolution resources, and recomposes against the captured old generated field. The transition uses a linear one-second internal blend and therefore never adds old and new strengths together. Pressure, Lee, Shore, obstacle, and other live sources are evaluated only after the generated blend.

If another complete replacement activates before the fade finishes, the current visible generated blend is captured as one flattened snapshot and becomes the next transition source; the previous snapshot is retired only after queued GPU work is safely past it. For a dimension or domain-mapping change, the old complete material/topology renderer bindings remain visible during staged resource initialization. Once the new resource set is complete, those held bindings are retired and the captured generated source is remapped into the new grid by global longitudinal distance and physical lateral offset. Old coverage outside the new/old shared river interval contributes zero rather than being clamped into a false edge strip.

A global cross-river scheduler is deliberately deferred. The complete single-river pipeline and its final work categories must be accepted before multi-river arbitration is designed.

Further performance work is now formally paused after the accepted profiler instrumentation, staged initialization, and dirty-rebuild queue. Steady-state maintenance staggering, compute splitting, striping, jobs, and multi-river scheduling resume only after Major, Connector, Pocket, and combined topology validation establish the final dependency graph.

## Performance Contract

- Use shared fixed-cost fields.
- Prioritize CPU/GPU compute cost, dispatch count, and runtime latency over modest additional memory; retain compact masks, variants, anchors, and descriptors when doing so removes meaningful runtime computation.
- No continuously maintained topology graph.
- No permanent node set or pathfinding network.
- No GameObject or managed allocation per Major, Connector, negative region, or Foam patch.
- No final-shader loops over objects, structures, or sources.
- Remaining free-water topology must scale primarily with structural texel count.
- The active visual-proof generator may run expensive field-first topology construction during Foam initialization or explicit pre-gameplay preparation, but this is temporary proof plumbing. It must be profiled and labelled as future cache/precompute work.
- The active generator may use candidate retries, connected-component cleanup, hole filling, distance transforms, endpoint clustering, bounded pathfinding, rejection, and composition scoring because those operations are necessary to prove the final visual model.
- Active gameplay must not run those expensive topology-generation operations once the production pipeline exists. Gameplay should consume cached per-river/per-run fields, cached descriptors, or another accepted compact representation.
- During the temporary proof path, generation runs only on initialization or queued topology rebuilds, never every frame as ordinary steady-state maintenance.
- Connector Support and four-class Negative Aging Pressure are accepted as the static integrated baseline through Patch 4.5. Connector selection uses a soft degree-aware and longitudinal-distribution bias so wider participation is encouraged without mandatory first connections or section quotas. Interior Pocket, Edge Cavity, Connector Weak Span, and Free-Water Negative Event are accepted for feature progression; population and shape coefficients remain provisional until final Foam-material validation.
- The final runtime target remains bounded sampling/composition of accepted fields or compact descriptors. No final shader loop may scan candidate lists, shape libraries, path graphs, or growing structure collections.
- Ordinary gameplay Obstacle Footprint updates must never rescan mesh triangles. The temporary development fallback may perform exact 3 × 3 interval preparation only during staged pre-gameplay initialization or a coalesced explicit geometry rebuild. Production must load the same compact prepared data from the procedural chunk build/link cache.
- Static Pressure may retain its accepted contour source for now. Authoritative Obstacle Footprint must use exact prepared mesh intervals; the temporary Play-startup scan exists only until procedural chunk build/link preparation owns and caches that data.
- Topology updates run at lower cadence than material transport.
- One explicit Foam initialization phase advances per river per frame.
- One queued boundary/obstacle rebuild phase advances per river per frame.
- Repeated boundary and obstacle notifications coalesce instead of creating duplicate rebuild chains.
- Obstacle rasterization waits for a stable `ObstacleGeometryVersion` window during initialization and ready-state rebuilds.
- Steady-state separation of Major, Connector, and class-specific negative evolution, composition, and diagnostics is deferred by the topology-first performance pause.
- Anchored geometry data rebuilds only when its source changes.
- Static object geometry preprocessing is effectively one-time for the intended generated chunks.
- Distant, sleeping, frozen, or inactive chunks must stop unnecessary work.
- Quality tiers may change resolution and cadence, not fundamental behaviour.
- Profiling must report material states, topology states, temporary resources, dispatch cadence, active chunks, memory, and worst-case update spikes.

Deferred optimization notes:

- Foam Obstacle Footprint now waits for a short stable `ObstacleGeometryVersion` window and coalesces repeated ready-state changes before rasterization. Validate that this removes redundant startup and source-churn rebuilds without leaving stale geometry.
- Reuse the prepared interval buffers and structural scalar snapshot; do not restore the removed contour `Color[]` raster/upload path.
- Audit topology/debug work so `Final Foam (Debug Off)` does not trigger diagnostic-grade topology composition or metric refreshes.
- Tighten Static Pressure, Static Wake, and ripple-boundary dirty flags so source/profile changes rebuild only the affected textures and passes.
- Defer a global cross-river work scheduler until the accepted single-river pipeline exposes stable initialization, rebuild, maintenance, and feature-readiness work categories.
- Future Static Pressure fidelity work should consume the shared compact geometry prepared by procedural chunk building/linking, not add another runtime mesh scanner.

## Public Controls

The currently accepted normal Inspector controls are:

- `Foam Enabled`;
- `Major Support Amount` — range `0–1`; nested deterministic population;
- `Major Support Size` — range `0–1`; overall physical scale envelope;
- `Major Support Size Variation` — range `0–1`; relative spread between stable Major opportunity sizes;
- `Major Support Seed` — non-negative integer; deterministic candidate, opportunity, transform, and future evolution identity;
- `Connector Amount` — range `0–1`; accepted relationship population and bounded overlap tolerance;
- `Connector Directness` — range `0–1`; endpoint directness and one broad deterministic bend;
- `Connector Length Preference` — range `0–1`; short-biased through neutral to long-biased ranking inside a fixed safe envelope;
- `Interior Pocket Amount` — range `0–1`, default `0.5`; accepted for feature progression, with coefficient tuning deferred;
- `Edge Cavity Amount` — range `0–1`, default `0.5`; accepted for feature progression, with coefficient tuning deferred;
- `Connector Weak Span Amount` — range `0–1`, default `0.5`; accepted after Patch 4.3 visual validation;
- `Free-Water Event Amount` — range `0–1`, default `0.5`; accepted after Patch 4.4 visual tuning;
- `Foam Colour`.

For all four negative controls, `0` means none, `0.5` means a sensible category-specific baseline, and `1` means the maximum bounded population. Increasing Amount activates a nested deterministic subset and does not change size, negative strength, seed, or evolution speed.

Patch 3.4 removed `Foam Preset`, the old broad `Amount`, `Web Granularity`, `Network Evolution`, `Breakup Frequency`, `Foam Speed`, `Major Evolution Rate`, and `Major Cleanup Rate`, together with their preset, migration, compatibility, and resolver code.

Generator coefficients such as field structure scale, warp, occupancy, composition scoring, path cost, gate radius, clearance thresholds, detour limits, negative-region size, pressure strength, spacing, and evolution pacing remain implementation details until visual testing proves that normal authoring requires them.

The separate Foam-material lifecycle will later require its own base-lifetime and response authoring. Those controls are explicitly outside topology implementation.

## Implementation Sequence

The canonical topology-only rollout is maintained in `River_Foam_Topology_Implementation_Plan.md`. The summary below must remain aligned with it.

### Completed / Accepted

1. Shared Stage 6 field infrastructure and structural resolution tiers.
2. Anchored Shore, Lee, and Pressure Support.
3. Permanent profiler instrumentation, per-river staged initialization, and queued/coalesced rebuild scheduling.
4. Field-first Major candidate generation, whole-river distribution, accepted controls, and stable metadata.
5. Major-to-Major Connector Support, accepted controls, endpoint gates, clearance halos, detour limits, and soft participation distribution.
6. Complete removal of obsolete topology grammar and obsolete broad Foam controls.
7. Initial Interior Pocket proof hosted by broad Major interiors.
8. Patch 4.1 exact transformed-mesh Obstacle Footprint and procedural chunk/run preparation handoff contract.
9. Patch 4.2 Interior Pocket population and one-sided Edge Cavities accepted for feature progression; coefficient tuning deferred.
10. Patch 4.3 Connector Weak Spans accepted after visual validation.
11. Patch 4.4 Free-Water Negative Events accepted after visual tuning.
12. Patch 4.5 complete static topology accepted for feature progression; coefficients remain provisional.
13. Patch 4.6 lively single-instance Major movement/morphing accepted for feature progression.
14. Patch 4.6.1 distributed local recycle territories accepted for feature progression.
15. Patch 4.6.2 combined elapsed-time and completed-hop lifetime units accepted for feature progression; tuning remains provisional.
16. Patch 4.7A host-relative Interior Pocket and Edge Cavity evolution implemented.
17. Patch 4.7A.1 hosted-negative footprint/parity correction implemented and visually accepted.
18. Patch 4.7B independent Free-Water evolution implemented.
19. Patch 4.7B.1 positive-downstream/lifetime/recycle contract correction implemented and visually accepted.
20. Patch 4.7C.0 canonical CPU/HLSL topology field-space contract implemented and visually revalidated.
21. Patch 4.7C.1 Connector endpoint/path and Weak Span immutable preparation data implemented and diagnostically accepted.
22. Patch 4.7C.2 identity Connector/Weak Span reconstruction, debug-only parity, and combined reconstruction scheduling implemented and visually accepted.
23. Patch 4.7C.3 endpoint-driven Connector deformation, same-host recycle-variant switching, temporary absence, and Weak Span current-path/tangent following implemented; long-run validation exposed permanent relationship drain.
24. Patch 4.7C.3.1 complete anchor-state coverage and bounded replacement-relationship rebinding implemented; long-run validation exposed stretch and turnover defects.
25. Patch 4.7C.3.2 ratio-based Connector breaking and deterministic recycle relationship turnover implemented; distribution defect found.
26. Patch 4.7C.3.3 deterministic soft Connector distribution bias implemented and visually/long-run validated.
27. Patch 4.8A staged same-grid replacement preparation and atomic activation implemented and Unity-verified.
28. Patch 4.8B safe old/new generated-topology crossfade, superseding-transition flattening, and differently mapped domain/quality hold/remap transition implemented and Unity-verified.
29. Patch 4.9A versioned cache contract and deterministic round-trip proof implemented; Unity validation pending.

### Active runtime-evolution sequence

1. Validate Patch 4.9A exact round-trip proof on the actual river.
2. Patch 4.9B — stable domain/obstacle fingerprints and explicit cache building.
3. Patch 4.9C — cache-first runtime initialization.
4. Patch 4.9D — production and cold-start validation.
5. Patch 4.10 — topology completion and handoff to separate Foam-material work.

Material aging response, fragmentation, dissipation, and rendering are not topology implementation steps and must not begin before topology completion.

### Patch 4.9 cache/precompute contract

Patch 4.9 is split so the data contract is accepted before startup ownership changes:

- **4.9A — versioned contract and round-trip proof:** serialize exact 32-bit field values, the exact obstacle scalar mask, all Major local masks and recycle anchors, all Connector ownership/path/variant/catalogue data, all four Negative Aging Pressure class records/masks/attachments, mapping/settings/seed metadata, deterministic counters, format/generator versions, and a corruption checksum. The explicit Play-mode proof reconstructs fresh topology objects, requires byte-identical reserialization and initial generated channels, reports size/hash/timings, rejects corruption, and never activates the loaded graph.
- **4.9B — stable fingerprints and explicit building:** replace session-local domain/obstacle versions with stable content checksums, define authored river/chunk/run payload ownership, and add an explicit development build/regeneration path.
- **4.9C — cache-first initialization:** validate/load the payload, upload cached obstacle/topology data, reuse the accepted evolution/upload paths, and skip expensive mesh scanning, forced readback, generation, cleanup, and path searches on valid hits. Production misses must not silently trigger synchronous full generation.
- **4.9D — production validation:** prove cold/warm startup behaviour, stale-input rejection, absence of expensive generator markers on hits, bounded staged loading, and reasonable memory.

The binary payload is storage-provider agnostic. It must not be tied permanently to one Editor-only asset type, and Patch 4.8A/4.8B remain the single complete-topology activation/transition authority.

## Acceptance Gates

### Major Support

Pass only if the paused diagnostic and generator tooling show:

- the field-first generator produces convincing Major regions under ordinary settings, not only extreme tuning;
- every accepted Major region is one connected filled body with no unintended enclosed holes;
- generated shapes preserve safe margins and reveal no straight storage-boundary edges;
- the generated population includes meaningful ranges of area, aspect, compactness, asymmetry, concavity, angular character, and perimeter complexity;
- oval similarity, ribbonness, excessive thinness, minimum neck width, and high-frequency perimeter noise are explicitly measured and rejected when outside approved ranges;
- the same river seed, river context, obstacle state, settings, and generator version reproduce the same static topology;
- increasing Amount adds support opportunities without moving or reshaping already-active accepted regions unless the integrated composition rules explicitly require a documented re-solve;
- changing Size preserves active identity where feasible while changing physical scale or composition density predictably;
- short rivers visibly contain small, medium, and occasional large support regions;
- shapes distribute laterally and diagonally as well as longitudinally;
- substantial neutral water remains between broad support regions;
- temporary generation costs are profiled and clearly marked as future cache/precompute work;
- generated masks do not reveal repeated primitive stamps, harmonic blobs, cellular damage, skeleton bias, toy path descriptors, or an ellipse-dominated family;
- every ordinary hop makes positive net downstream progress, while bounded lateral or diagonal movement does not produce longitudinal upstream motion;
- scale remains stable across quality tiers, bends, width variation, chunks, and reverse flow.

### Connector Support

Pass only if connectors:

- occur between real positive support on two sides;
- do not begin or end in unsupported empty water;
- remain sparse and subordinate;
- deform continuously during ordinary endpoint movement and replace invalid relationships only through bounded prevalidated alternatives;
- avoid obvious obstacle and domain violations;
- leave and enter Major regions only through compact endpoint gates;
- do not orbit around Major perimeters to force unlikely endpoint relationships;
- remain within the accepted bounded detour ratio;
- may use bounded pathfinding or cost-field search during proof/bake generation, but do not require a persistent gameplay graph or steady-state path search.

### Negative Aging Pressure

Pass only if:

- Interior Pockets substantially overlap broad Major interiors and preserve a useful positive rim;
- Edge Cavities remain Major-hosted, breach only a deliberate side, and preserve a useful positive remainder;
- Connector Weak Spans remain associated with accepted Connector identities, stay away from endpoint gates, and create local fragility rather than invalid relationships;
- Free-Water Negative Events remain sparse, inside valid water, outside exact obstacles, and subordinate to the overall topology;
- all four `Amount` controls are deterministic, nested, and produce a sensible baseline at `0.5`;
- Anchored Support cores and exact obstacle context remain protected;
- positive and negative fields remain separate and may overlap;
- subtype identity and future evolution metadata survive aggregate packing;
- every class follows its approved asynchronous single-instance movement/morph rhythm once runtime evolution is implemented;
- no class acts as instant geometric subtraction.

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
- Do not reintroduce discrete Major size classes, archetype-family switches, toy path descriptors, or bland fallback composition tables unless the integrated field-first generator is explicitly rejected with measured evidence.

## Failure Rule

If a field-first topology slice fails its acceptance gate, stop at that slice and diagnose the smallest relevant stage: candidate generation, source context, Major placement, Connector endpoint/path rules, negative-class placement, composition/upload, runtime evolution, or cache/rebuild integration.

Do not fall back automatically to the lobe grammar, toy path descriptors, a continuously maintained graph, per-structure object pool, or another independent-noise topology. Any representation change must be justified by measured evidence and explicitly approved.

## Stage 6 Completion

Stage 6 closes only after:

1. Anchored Support inputs and current Obstacle Footprint diagnostics remain accepted;
2. Major, Connector, and all four Negative Aging Pressure classes pass separately and together;
3. terminology and authoring are coherent;
4. soft topology-to-lifetime material response passes;
5. end-of-life fragmentation and dissipation pass;
6. final rendering and public controls pass;
7. PC-first profiling, quality, lifecycle, chunking, sleeping, freeze/thaw, reverse-flow, and long-running regression pass;
8. the roadmap is updated with conservative measured results.
