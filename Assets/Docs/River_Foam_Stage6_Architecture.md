# River Foam Stage 6 Architecture

## Purpose

Define the canonical Stage 6 architecture for the river's stylized floating surface film.

The target is not generic whitewater, bubble foam, scattered streaks, or a shader-painted mask. It is one persistent surface material that gathers into torn contour networks: broad broken sheets, long ribbons, enclosed dark-water pockets, medium branches, thin temporary connectors, peeling strips, and secondary fragments.

This document owns the detailed Stage 6 implementation and acceptance contract. `River_Rendering_Roadmap.md` should carry only the concise milestone summary.

## Reference Read

The strongest reference traits are:

- a small number of long, eye-traceable structural ribbons;
- broad connected film regions broken by medium and large dark-water pockets;
- strong organisation by banks, rocks, constrictions, and sheltered downstream regions;
- a hierarchy of major sheets, medium branches, hair-thin connectors, torn edges, and small debris;
- substantial open water between structures;
- topology that remains convincing when motion is paused;
- persistent structures that stretch, peel, split, reconnect, and shed fragments over time.

The rejected/current sparse result is dominated by isolated almond-shaped marks, short strokes, and similarly sized fragments. Motion alone cannot correct that composition.

## Canonical Goal

Stage 6 must create a persistent surface-film network with three distinct responsibilities:

1. **Structural Topology** defines where major film structures are supported, where connectors are permitted, where open-water pockets are protected, where material tends to converge, and where material can remain captured.
2. **Persistent Material State** records what film actually exists at the current moment.
3. **Material Processes** transport, supply, capture, preserve, damage, tear, peel, merge, and remove that material.

These responsibilities must not be collapsed into one procedural mask.

A topology field is not the final foam picture. High structural capacity means that material can survive or organise there; it does not mean the solver may paint the region white immediately.

## Visual Hierarchy

The ordinary result should contain all of the following at once:

1. A few dominant contour ribbons or broken sheets carrying most of the visual identity.
2. Medium branches that divide broad regions and help enclose water pockets.
3. Sparse thin connectors and junctions that may appear, weaken, and fail.
4. Torn edges, peeling tips, necks, splinters, and small detached fragments.
5. Large areas of open water.

The network is not required to remain globally connected. It should move between connected, partly connected, and fragmented states. Tiny fragments are secondary debris and must never become the main visual identity.

## Architectural Model

### Structural Topology

The topology representation must expose conceptually separate outputs, even if several are packed into shared textures:

- **Major Capacity** — support for broad rafts, sheets, and dominant ribbons.
- **Connector Capacity** — weaker support for medium branches, narrow links, and junctions.
- **Pocket Exclusion** — protected open-water negative space that ordinary convergence, capture, and repair may not fill.
- **Obstacle Exclusion** — the conservative registered-solid cross-section at the current Stage 3 water level, which no positive topology or material may occupy.
- **Convergence Preference** — direction and strength encouraging existing material toward supported structures.
- **Capture and Residence** — slowdown, survival, and temporary structural support near boundaries and accepted interaction regions.
- **Permitted Supply** — locations and limits under which genuinely new material may enter.

These values have different meanings and must not be treated as aliases for one desired-density mask.

### Habitat Versus Visible Foam

Structural topology is the foam habitat, not the rendered foam layer.

The topology field answers where foam is allowed to organise, survive, connect, or be protected from filling. It should carry the broad composition language of the reference: supported major sheets, possible connector routes, capture regions near environmental causes, and protected dark-water pockets. It is not expected to contain every final frayed edge, white streak, or turbulent particle-scale detail.

The final reference-like effect is produced by three stacked scales:

1. **Topology / habitat scale** defines where foam can plausibly exist. Major Capacity supports broad broken sheets. Connector Capacity supports curved links, branches, and junction routes. Capture classes support residence near accepted Pressure, lee, and shore contexts. Pocket Exclusion protects holes and open-water cutouts, while Obstacle Exclusion removes the conservative solid cross-section at the current Stage 3 water level.
2. **Material occupancy scale** decides what foam actually exists right now. Amount, Freshness, Integrity, advection, convergence, decay, tearing, peeling, and merging determine whether a supported region is full, partial, damaged, empty, or recovering.
3. **Rendering / filament scale** turns occupied material into the visible surface. Shader thresholds, directional erosion, flow-stretched noise, edge breakup, brightness falloff, and integrity/freshness variation create the thin white filaments, torn rims, gaps, and frayed tips.

Therefore:

```text
Positive Support = max(Major, Connector, Capture)
Negative Support = max(Pocket Exclusion, Obstacle Exclusion)
Foam Support = Positive Support * (1 - Negative Support)
Visible Foam = render(Material State shaped by Foam Support)
```

High support does not guarantee visible foam. Low or zero support should make material decay, tear, or fail to repair. Pocket Exclusion and Obstacle Exclusion remove support even if a positive class is present.

Connector Capacity must include genuinely thin, curved subregions, because persistent long filaments need coherent routes to inhabit. These routes should be softer and usually wider than the final visible strands; they are permission corridors, not finished white lines. The material layer should occupy them intermittently, and rendering should narrow, erode, and break them into frayed visible filaments.

If connector topology contains no thin routes, the material and shader can make temporary noisy edges but cannot reliably form long curved bridges. If connector topology is too continuous, clean, parallel, or river-length, the material will inherit that highway structure. Thinness alone is not enough: connector support must also be broken, uneven, locally disappearing, and allowed to fork, merge, and fail.

### Persistent Material State

The existing persistent state remains suitable unless implementation evidence proves otherwise:

```text
R = Amount
G = Freshness
B = Integrity
A = material phase / provenance
```

- **Amount** is the long-lived material quantity.
- **Freshness** is short-lived source history and limited recent-material variation.
- **Integrity** describes structural health and accumulated damage.
- **Phase / provenance** provides lightweight transported history for asynchronous damage, compatible merging, and weak seams.

Temporary forward and reverse states used by corrected advection remain transient simulation resources, not additional visible foam layers.

### Material Processes

Material changes only through explicit processes:

- downstream transport;
- metric-aware convergence;
- legitimate supply;
- boundary or interaction capture;
- persistence and decay;
- strain and integrity damage;
- hole opening and neck failure;
- peeling and fragment release;
- conservative overlap-based merging.

The material solver must retain meaningful history. A region recently torn open must not be repainted solely because topology capacity remains high.

## Capacity, Capture, and Supply

These concepts are deliberately separate.

### Capacity

Capacity answers:

> How much material could plausibly survive and organise here?

Capacity alone creates no material.

### Capture

Capture answers:

> How slowly should existing material move, and how long should it survive here?

Capture may attract nearby material and support Integrity. It must not act as an unlimited source. Shore capture must remain capacity-limited rather than becoming an excessively thick permanent white outline. Object-related positive residence comes from Pressure and Lee, not from an always-on obstacle halo.

### Supply

Supply answers:

> Under what legitimate condition may new Amount enter the field?

Primary supply should come from:

- upstream inflow;
- bounded replenishment in underfilled capture regions;
- a weak network-repair allowance where supported structure already has nearby donor material;
- explicitly approved continuous Stage 5 inputs where appropriate.

Distributed random births may remain only as a weak extinction-prevention mechanism. They must not draw the network.

### Bounded Deficit Response

The solver may calculate a local deficit:

```text
deficit = max(0, structuralCapacity - existingMaterial)
```

The deficit does not authorize direct filling. Recovery is permitted only when a legitimate supply or donor condition exists, and it must remain rate-limited.

Protected pockets have near-zero capacity and no ordinary repair supply. Major structures may have high capacity and long residence. Connectors should generally have lower capacity, lower repair strength, and shorter residence so they remain fragile.

## Priority Rules

Conflicts are resolved in this order:

1. Valid river domain and solid exclusion.
2. Downstream-only material causality.
3. Protected negative space.
4. Boundary, obstacle, Pressure, and lee capture.
5. Major sheets and dominant contour ribbons.
6. Medium branches, connectors, and junctions.
7. Tearing, fragments, edge irregularity, and rendering detail.

A lower-priority rule may not violate a higher-priority rule.

Examples:

- connector repair may not cross a protected pocket;
- topology recovery may not construct an upstream-growing bridge;
- a broad raft may not fill a projected obstacle;
- fragmentation may not erase all captured structure and depend on random births to redraw it;
- rendering roughness may not invent macro holes or branches.

## Preserved Contracts

- `RiverDomainSnapshot`, metric spacing, connected/global distance, width variation, bends, reverse flow, and surface frames remain authoritative.
- Chunk storage padding beyond `Domain.LocalLength` is invalid domain. It contributes no topology, supply, population measurement, transport retention, or rendering.
- Stage 5 remains visually closed.
- Stage 6 may read accepted Pressure, stationary Wake/lee, boundary, and registered-obstacle data without changing Stage 5 response.
- Impact Ripple-to-Film behaviour is deferred. Preserve a future integration hook, but do not include Impact in Batch 1 or Batch 2 acceptance.
- No coherent material feature, repair front, or merge front may travel upstream.
- The final water shader samples a fixed number of shared fields and never loops over active sources.
- Foam continues to respect freeze, Amount zero, quality tiers, active chunks, sleeping, delayed release, scene reload, and resource cleanup.
- Macro topology comes from topology and material fields. The shader may add only stable sub-cell contour treatment.

## Concrete Topology Generators

Every generator must be defined from available project data. Vague fluid terms are not implementation specifications.

### Shore Structure

Derived from:

- real left and right shoreline distance;
- shoreline tangent in river space;
- animated visible-shore position where already available;
- intermittent capacity modulation in metric coordinates.

Purpose:

- produce broken contour bands and bank-following ribbons;
- attract and retain material without producing continuous white outlines.

### Object Interaction Regions

Derived from:

- the exact transformed generated mesh exposed by each registered static solid;
- a one-time conservative solid-interval bake over only the full-resolution Foam texels touched by that mesh;
- the complete current Stage 3 surface-height evaluator at each retained sample;
- the accepted static Pressure representation upstream;
- the accepted attached stationary Lee representation downstream.

Purpose:

- split approaching material through water-level-aware Obstacle Exclusion;
- create upstream and shoulder capture through Pressure;
- support downstream residence and peeling through Lee;
- never create an unconditional positive halo around the object;
- never place material inside the solid footprint.

### Bend Organisation

Derived from:

- signed river-centreline curvature;
- normalized or metric lateral position;
- local river width and downstream tangent.

Purpose:

- bias dominant ribbons and broad divisions through bends;
- remain stable across spline-knot spacing, width variation, connected offsets, and quality tiers.

This is the concrete replacement for unspecified “flow-separation lanes” where no full fluid separatrix solver exists.

### Broad Free-Water Rafts

Derived from:

- low-frequency anisotropic fields in global metric river coordinates;
- structures elongated primarily along the river;
- deterministic regional identity with slow asynchronous evolution.

Purpose:

- provide broad film capacity away from boundaries;
- create a small number of dominant sheets rather than uniform fine webbing.

### Pocket Exclusion

Derived from:

- stable low-frequency metric-space exclusion regions;
- local raft context;
- size limits expressed in metres rather than texture cells;
- optional geometry-informed shaping near bends and obstacles.

Purpose:

- preserve medium and large dark-water holes;
- prevent convergence and repair from turning broad capacity into a white slab.

### Branches and Connectors

Derived from:

- relationships between nearby major structures;
- metric distance and orientation;
- downstream-causal donor availability;
- pocket and solid constraints.

Purpose:

- create medium branches and sparse fragile links;
- remain subordinate to major structures;
- fail naturally under damage and insufficient donor material.

Connectors must not be inserted merely because two occupied cells exist on opposite sides of an empty gap.

### Pressure Shoulders — Batch 1B

Derived from the accepted Stage 5 Pressure texture through explicit remapping and thresholds.

Purpose:

- provide stable local organisation and capture around accepted stationary Pressure shoulders;
- never alter the Stage 5 Pressure field.

### Stationary Lee Organisation — Batch 1B

Derived from the accepted stationary Wake/lee representation.

Purpose:

- provide strong sheltered capture and residence;
- support later downstream peeling and fragment release;
- avoid converting the complete Wake field into a separate foam overlay.

## Anchored and Evolving Topology

### Anchored Structure

The following should remain spatially stable while their underlying geometry or Stage 5 source remains unchanged:

- shore organisation;
- obstacle footprints and exclusion;
- bend tendencies;
- Pressure shoulders;
- stationary lee regions.

They may vary in local strength but must not visibly regenerate as unrelated patterns.

### Slowly Evolving Structure

The following may change gradually:

- broad free-water rafts;
- pocket shapes;
- secondary branches;
- connector candidates.

Evolution must be asynchronous and regional. The full network must never switch procedural identity at once. Structural lifetimes should be measured in seconds rather than frames.

All topology uses global metric river coordinates so apparent scale does not change with texture resolution, river width, chunk boundaries, or quality.

## Canonical Public Controls

The normal Inspector exposes only:

- `Amount`
- `Web Granularity`
- `Network Evolution`
- `Breakup Frequency`
- `Foam Speed`
- `Foam Colour`

Conceptual mapping:

| Public control | Primary responsibilities |
|---|---|
| `Amount` | total supported material population and bounded supply |
| `Web Granularity` | major raft, pocket, branch, and connector scale |
| `Network Evolution` | slow free-water topology change and wandering |
| `Breakup Frequency` | damage cadence, hole opening, neck failure, and fragment shedding |
| `Foam Speed` | downstream material transport |
| `Foam Colour` | lit tint and alpha |

Capture strength, residence, connector durability, pocket protection, convergence rates, persistence, repair strength, and contour sharpness remain internal initially.

Existing serialized fields may remain hidden as compatibility inputs. New public controls must not be added unless testing proves that the six canonical controls cannot cover a required artistic range.

## Reusable Existing Infrastructure

The retarget should preserve useful existing work where it satisfies the new contract:

- river-owned persistent material fields;
- corrected downstream transport;
- global metric coordinate use;
- active chunks, quality tiers, sleeping, freezing, and resource release;
- projected obstacle contours and solid exclusion;
- boundary and Stage 5 read-only inputs;
- population reduction;
- persistent fracture/damage support;
- fixed-cost final shader sampling.

Existing guidance and supply logic may be replaced or substantially reduced if they continue to produce sparse strokes or paint topology indirectly.

The former Stage 6.1 result is an implementation baseline, not an accepted visual target.

## Batch 1A — Geometry-Driven Topology Proof

**Status:** implemented in code. The first Unity visual review rejected the initial noise-dominated distribution because connectors and pockets concentrated into isolated longitudinal areas and raw boundary bands dominated their diagnostic. A geometry-hierarchy correction is implemented; Unity revalidation is pending.

The implementation adds one simulation-neutral `RGBAHalf` topology field at guidance resolution. Its channels are `Major Capacity`, `Connector Capacity`, `Pocket Exclusion`, and a derived guidance-resolution copy of `Obstacle Exclusion`. The authoritative obstacle mask is a separate full-resolution `RHalf` texture. A companion texture retains canonical Pressure, Lee, and Shore capture separately; its alpha channel is reserved zero. Each positive class is computed independently from its own source data plus valid fluid-domain masking, and each negative class remains independently inspectable until final composition. The existing persistent material solver does not read these fields during Batch 1A.

### Objective

Prove that a field-based topology representation can create the correct paused composition before material simulation is changed.

### Scope

Implement debug-only or simulation-neutral topology using only:

- river geometry and metric coordinates;
- shore structure;
- projected obstacle footprints;
- bend organisation;
- broad free-water rafts;
- pocket exclusion;
- medium branches and sparse connectors.

Do not integrate Pressure, Wake/lee, or Impact yet.

Do not retune material transport, tearing, shader thresholds, population control, or rendering to hide topology defects.

### Retained Debug Views

`Final Foam (Debug Off)` returns to the normal rendered result. The Inspector exposes only four Foam diagnostics:

- `Capture Zones`
- `Positive Zone Classes`
- `Negative Zone Classes`
- `Positive and Negative Zones`

Each selected view displays its own colour legend and interpretation directly beneath the selector. All former Foam diagnostics and their shader/runtime branches have been removed. Additional views are added only when a concrete diagnostic question requires them.

### Implemented Batch 1A scope

- Construct overlapping metric topology regions across the complete valid river length so every region receives bounded opportunities for major sheets and contour ribbons.
- Use deterministic regional variation only to deform this hierarchy; noise cannot remove topology from most of the river.
- Generate connectors relationally between the end centres of neighbouring major regions rather than as an independent thresholded line field.
- Generate one or more bounded pocket candidates per region and accept them only inside the interior of broad-sheet support.
- Read the authoritative shore field as an independent positive capture class and the projected stationary-obstacle solid footprint as an independent negative exclusion class. Neither is gated, boosted, attenuated, or reshaped by Major or Connector capacity.
- Exclude all storage padding beyond `Domain.LocalLength`.
- Expose the four retained diagnostics, the normal Debug Off state, and topology resolution/memory diagnostics.
- Expose only the six canonical public controls.
- Leave material transport, population supply, capture response, tearing, merging, and final foam rendering unchanged.

### Batch 1A Acceptance

Test a mostly straight river, a strong bend, width variation, an obstacle cluster, connected distance offsets, reverse flow, and each quality tier.

Pass only if paused views show:

- several long eye-traceable dominant structures;
- broad rafts or sheets and narrow ribbons coexisting;
- medium and large protected dark-water pockets;
- banks and rocks clearly organising the composition;
- medium branches and sparse connectors subordinate to major forms;
- substantial open water;
- stable physical scale across width, quality, chunks, and reverse flow;
- no dominance by isolated ovals, short parallel lanes, stipple, or uniform fine webbing;
- no contribution from padded storage beyond `Domain.LocalLength`.

If these conditions fail, stop before Batch 1B.

## Batch 1B — Accepted Interaction Inputs and Diagnostics

**Status:** implemented in code. Compact diagnostics, canonical capture storage, and the independent-zone cleanup are implemented; Unity revalidation is pending.

### Objective

Integrate stable read-only Stage 5 organisation and establish measurable topology behaviour after the representation passes Batch 1A.

### Scope

- Add Pressure-shoulder organisation.
- Add stationary lee capture and organisation.
- Keep Impact deferred.
- Add final topology composition rules using the canonical priority order.
- Validate and complete the six canonical public-control mappings across accepted topology sources.
- Validate update cadence, quality scaling, memory, chunk activity, sleeping, freeze, and resource release.
- Keep persistent material visually unchanged except where required to display diagnostic comparisons.

### Implemented Batch 1B Scope

- Add one same-resolution `RGBAHalf` source-class field retaining `Pressure`, stationary `Lee`, and `Shore` positive capture separately; alpha is reserved zero.
- Preserve the cached CPU boundary texture red/green material-solver contract. Its blue and alpha channels are reserved zero. Registered solids expose their exact transformed generated mesh. Stage 6 performs a one-time conservative line/triangle solid-interval bake only for touched full-resolution Foam texels, then reconstructs a point-sampled runtime Obstacle Exclusion mask from those cached intervals at the current Stage 3 water level. Canonical Shore Capture comes exclusively from the dynamic Stage 3 edge rows.
- Sample only the accepted Stage 5 static Pressure target and stationary Wake-source lee channel; transported Wake energy and Impact Ripple are excluded.
- Store every positive class independently, combine them with an unweighted maximum, combine Pocket Exclusion and Obstacle Exclusion with an unweighted maximum, and apply the combined negative topology exactly once at final composition.
- Keep the source fields diagnostic-only: neither topology texture is consumed by material transport, supply, capture, tearing, merging, or final Foam rendering in Batch 1B.
- Add low-rate asynchronous GPU topology metrics over the valid river domain. Padded storage is excluded.
- Neutralise the legacy Impact-to-Foam reinforcement value so Impact has no Stage 6 influence during this retarget.
- Preserve the six canonical public controls; no new authoring controls are introduced.

### Diagnostics

The normal rendered state and retained diagnostics are:

| View | Meaning and colour encoding |
|---|---|
| `Final Foam (Debug Off)` | Normally lit visible Foam from the current persistent material solver. No Foam diagnostic colour encoding is active. |
| `Capture Zones` | Canonical independent positive capture values. Red = Pressure; green = stationary attached Lee; blue = the fixed-width band following the instantaneous Stage 3 visible shoreline. Shore uses `0.24 m` full capture plus a `0.03 m` inward fade from the current left/right edge. Each class comes only from its own source plus valid domain/solid masking. Overlaps mix directly. |
| `Positive Zone Classes` | Red = independent Major Capacity; green = independent Connector Capacity; blue = the maximum of the exact Pressure, Lee, and Shore values shown separately in Capture Zones. No class weighting is applied. Overlaps mix; black means no positive support. Negative topology is not shown. |
| `Negative Zone Classes` | Red = Pocket Exclusion; blue = Obstacle Exclusion from the conservative registered-solid cross-section at the current Stage 3 water level; magenta = overlap. No obstacle expansion or surrounding halo is added. |
| `Positive and Negative Zones` | Green = the unweighted maximum of Major, Connector, and combined Capture support before subtraction; red = the unweighted maximum of Pocket Exclusion and Obstacle Exclusion; yellow = overlap where combined negative topology removes positive support; black = neither. |

### Canonical Independent-Zone Contract

Every positive class is generated and stored independently. A positive class may use only its own source data, valid river-domain masking, fluid coverage, and solid exclusion inherent to that source. No positive class may gate, boost, attenuate, shift, cut, or otherwise reshape another positive class.

The sole structural exception is Connector endpoint validation: a connector may use Major structures to establish that its endpoints exist, because that relationship defines a connector. Major values do not attenuate the connector along its path, and Pocket or Capture fields do not pre-clip Connector capacity.

The canonical source rules are:

- **Major Capacity:** generated from its own free-water structure rules and river metrics; object-related positive or negative classes do not reshape its generated value.
- **Connector Capacity:** generated from its own connector/branch paths after endpoint validation; it is not weighted against Major, Capture, Pocket, or Obstacle Exclusion fields.
- **Shore Capture:** a fixed metric band measured inward from the instantaneous Stage 3 visible shoreline, not from the static normal or maximum shoreline allowance. Stage 3 and Stage 6 share the same macro-wave, river-space noise, shore-specific profile, lateral-reach, asymmetry, and attenuation functions. A compact `RGHalf` row texture stores current signed left/right visible edges for every topology column. The current validation band is `0.24 m` of full capture plus a `0.03 m` inward fade. No random longitudinal gating and no Major/Connector context.
- **Obstacle Exclusion:** generated directly from the exact transformed generated mesh, not from the padded disturbance footprint, a convex hull, an object-bounds rectangle, or the Static Pressure envelope. During the one-time geometry bake, Stage 6 examines only candidate full-resolution Foam texels covered by the projected mesh. Nine sample lines per texel are intersected with the actual mesh triangles along the local river Up direction. Sorted entry/exit pairs retain up to two exact solid-height intervals per sample, with a tiny vertical inset; ambiguous odd intersection counts are rejected rather than approximated. At runtime, the complete current Stage 3 surface height is evaluated independently at all nine samples. The texel is accepted only when every sample lies inside a cached solid interval. The authoritative `RHalf` mask is point-sampled and therefore deliberately errs smaller rather than extending outside the visible generated rock. No hull, contour padding, profile interpolation, or bounds fallback is allowed. The guidance-resolution topology alpha channel is only a derived copy for composition and metrics. Static Pressure still performs an independent accepted geometry scan; a future refactor should derive its directional pressure envelope from this same exact solid-volume source data instead of duplicating mesh calculations.
- **Pressure Capture:** the direct magnitude available from the accepted static Pressure texture, with no hidden importance weight or external context multiplier.
- **Lee Capture:** the direct positive attached-lee value available from the accepted stationary Wake source, with no hidden importance weight or external context multiplier.
- **Pocket Exclusion:** stored separately from Obstacle Exclusion; both negative classes are combined and applied exactly once after all positive classes are combined.

### Shared Stage 3 Shoreline Contract

Shore Capture does not infer the shoreline from the normal allowance, maximum overlap, a static interpolation factor, or a second procedural wave. Stage 3 exposes the current visible edge through a shared evaluator:

1. use the authoritative global river distance and current motion time;
2. evaluate the exact Stage 3 centre carrier plus shore-specific height scale, length scale, lateral reach, stable wave-to-wave size variation, metric transition length, slope-continuous start/middle/end profile, left/right asymmetry, and shore attenuation used by the water shader;
3. test positive displacement against the corridor's mandatory hidden bank-cover profile between normal and maximum surface widths, while respecting the current per-wave reach limit;
4. store the outermost visible left and right edges in a compact row texture;
5. construct Shore Capture inward from those moving edges in world metres;
6. remove registered solid cells using the dedicated full-resolution Obstacle Exclusion mask evaluated from the current Stage 3 water height and the cached exact-mesh solid intervals.

The free-water topology remains low-rate, but the current edge and capture-source composition refresh at the ordinary Foam update cadence so visible shore motion is not quantised to the `4/6/8 Hz` topology-generation cadence. The current intermediate Stage 3 controls are `Shore Wave Height Scale`, `Shore Wave Length Scale`, `Shore Wave Reach`, `Shore Wave Transition Length`, `Shore Wave Size Variation`, `Shore Side Asymmetry`, and `Shore Wave Profile Variation`. Size Variation produces stable deterministic differences between successive travelling waves; Profile Variation shapes an individual wave through slope-continuous start/middle/end interpolation. Transition Length also drives the final zero-slope shoreline activation envelope: signed shore height is flattened around zero crossings, and resolved lateral reach approaches both the normal shoreline and the maximum allowance with zero derivative. Stage 6 receives only that resolved shoreline result through the shared evaluator and does not reinterpret those controls. Future packet-based shore-wave work must replace or extend this evaluator rather than adding a second approximation in Stage 6.

Composition is deliberately simple:

```text
Positive Support = max(Major, Connector, Pressure, Lee, Shore)
Negative Support = max(Pocket Exclusion, Obstacle Exclusion)
Composed Topology = Positive Support × (1 - Negative Support)
```

Underlying texture encoding is:

```text
Topology RGBA:         Major Capacity, Connector Capacity, Pocket Exclusion, derived guidance-resolution Obstacle Exclusion
Topology Sources RGBA: Pressure, stationary Lee, dynamic Shore band, reserved zero
Current Shore Edges RG: signed left visible edge, signed right visible edge
Boundary RGBA: legacy fluid coverage, legacy material attraction, reserved zero, reserved zero
Obstacle Exact-Mesh Samples: cached solid-height interval pairs plus exact Stage 3 water-evaluation parameters for nine positions in each retained texel
Obstacle Candidate Cells: full-resolution texel coordinate plus exact-sample offset
Obstacle Exclusion RHalf: authoritative full-resolution point-sampled current-water solid mask
```

No class-specific diagnostic contrast curve is applied. Every selected diagnostic repeats its legend in a context-sensitive Inspector help box. Source-specific shape construction still contains explicit geometric thresholds and widths; those define the source itself and are not cross-class importance modifiers. Any future normalization or weighting must be introduced explicitly, documented, and validated as a separate decision.

Required runtime metrics:

- major-capacity coverage;
- connector-capacity coverage;
- protected-pocket coverage;
- protected-pocket violation;
- visible material coverage;
- material deficit inside supported regions;
- shore occupancy;
- Pressure/lee occupancy;
- perimeter ratio;
- active chunks;
- dispatch count and cadence;
- allocated memory;
- sleeping and release state.

A connected-component count is optional and must not be treated as a success metric by itself.

### Batch 1B Acceptance

- Pressure and lee visibly organise topology without becoming separate overlays.
- Protected pockets survive all lower-priority composition stages.
- Topology evolution is slow, regional, and asynchronous.
- Anchored sources do not visibly drift or regenerate.
- The six public controls produce meaningful, non-duplicated changes.
- Quality changes affect resolution and cadence without changing topology scale or identity.
- Stage 5 visuals remain unchanged.
- Impact has no influence on Stage 6.

## Batch 1C — Finite Structure Topology

**Status:** implemented in code; Unity compilation and visual validation pending.

Batch 1C replaces the rejected continuous-chain grammar rather than tuning it. The topology is now generated through four ordered low-rate passes:

1. **Major structures:** deterministic finite rafts and contour ribbons are rasterised with explicit enable/disable state, physical start/end extents, pointed tapers, lateral drift, overlap, and absence. There is no always-on river-length primary sheet.
2. **Pocket validation:** pocket candidates belong to broad host structures and are accepted only when an eight-sample surrounding ring has sufficient broad support, remains inside fluid, avoids solids, and fits the host structure.
3. **Connectors and branches:** connectors require two separate accepted major-structure endpoints and an open middle span. Deliberate diagonal branches and occasional asymmetric forks grow from accepted parent structures. Connector interiors are suppressed where major capacity already exists.
4. **Composition:** Major, Connector, Pressure, Lee, and Shore remain independent positive classes. Pocket and water-level-aware Obstacle Exclusion remain independent negative classes and combine only at final composition.

Three simulation-neutral working textures retain major structures, validated pockets, and connector/branch candidates. The existing final topology texture remains:

```text
R = finite Major Capacity
G = relational Connector / Branch Capacity
B = validated Pocket Exclusion
A = derived guidance-resolution water-level-aware Obstacle Exclusion
```

The normal Foam view menu contains the Debug Off state and only the four documented diagnostics. Added metrics are composed-topology coverage, open-span coverage, pocket-interior coverage, major-capacity coverage, connector-in-major overlap, and source occupancy.

Batch 1C does not change material transport, autonomous supply, capture response, tearing, merging, Impact integration, or final Foam rendering. Topology is still built only for Batch 1 diagnostics; Batch 2 must move topology updates into the ordinary material-work schedule before material consumes it.

### Batch 1C Acceptance

- Major support consists of finite rafts and ribbons with visible starts, ends, tapers, overlaps, and genuine unsupported spans.
- The composition does not default to one continuous river-length highway.
- Protected pockets read as enclosed interior holes rather than gaps between parallel lanes.
- Connectors visibly bridge separate structures; connector-in-major overlap remains low away from endpoints.
- Diagonal branches, asymmetric forks, and irregular joins coexist with long eye-traceable structures.
- Conservative current-water solid cross-sections appear as Obstacle Exclusion, while Pressure and Lee separately provide object-related positive organisation outside the solid.
- Substantial open water remains before material simulation is connected.
- The compact documented debug views remain sufficient; no obsolete per-channel F1/F2 menu is restored.

## Batch 2 — Persistent Material Response

### Objective

Make actual material inhabit, transport through, damage, and leave the accepted topology without becoming a repainted stencil.

### Material Supply

- Upstream inflow is the primary continuous source.
- Capture zones may receive bounded replenishment only while underfilled.
- Network repair requires nearby donor material or a tightly bounded repair allowance.
- Random autonomous births are weak extinction prevention only.
- Supply is suppressed in protected pockets, solid cells, invalid padding, saturated broad interiors, and unsupported gaps.

### Convergence and Capture

- Existing material may converge toward supported structures.
- Convergence remains metric-aware and downstream-causal.
- Capture slows material without reversing it.
- Capture supports residence and Integrity temporarily.
- Material must be able to peel away and leave capture zones.
- High capacity does not guarantee immediate occupancy.

### Tearing and Fragmentation

Tearing should operate primarily on connected material:

- open holes inside broad film;
- damage and collapse narrow necks;
- peel bank- and obstacle-attached strips;
- reopen weak seams;
- release splinters and one-to-three-cell fragments;
- preserve short fragment survival without making fragments dominant.

`Breakup Frequency` controls the cadence and opportunity for damage events, not a second global Amount lifetime.

### Conservative Merging

- Material merges through real overlap or extremely short donor-causal convergence.
- Reconnection redistributes existing Amount.
- Empty gaps are not inflated into bridges.
- Merge or repair fronts may not advance upstream.
- Phase disagreement may form a weaker seam that can reopen later.

### Rendering

- lit off-white rather than emissive flat white;
- Amount/Integrity-driven opacity and apparent thickness;
- Freshness variation limited to recent material;
- slight transmission/refraction suppression beneath strong film;
- crisp contours with stable sub-cell irregularity;
- no shader-created macro holes, branches, fragments, or threshold flicker.

### Batch 2 Acceptance

At minimum, test paused and moving gameplay-camera views at 10 seconds and 60 seconds.

Pass only if:

- broad population remains comparable rather than collapsing or filling uncontrollably;
- several connected paths are traceable by eye;
- major sheets, dominant ribbons, medium branches, thin connectors, holes, and secondary fragments coexist;
- small fragments remain secondary;
- banks, rocks, Pressure shoulders, and lee regions capture and release material visibly;
- torn regions do not repaint immediately;
- material stretches, peels, tears, reconnects, and leaves while topology remains structurally influential;
- no coherent feature or merge/repair front appears to travel upstream;
- no row/column lattice, checker pattern, stipple cloud, dotted perforation row, synchronized scalloping, or shader-threshold breakup is visible;
- Impact remains deferred and visually inactive;
- Stage 5 behaviour remains unchanged;
- freeze, Amount zero, reverse flow, quality switching, sleeping, delayed release, obstacle registration/removal, scene reload, and long-running stability remain correct.

## Performance and Lifecycle Requirements

- Use shared fixed-cost fields; no final-shader per-source loops.
- Expensive topology work should run at a lower cadence than material transport where practical.
- Anchored geometry-derived inputs should rebuild only when their source changes.
- Free-water evolution should be regional and staggered.
- Quality tiers may change resolution and cadence, not topology scale or transport rules.
- Invalid padded storage must remain inert.
- Allocation must remain within hardware texture limits or fail clearly according to the runtime allocation contract.
- Profiling must report material states, topology states, temporary transport states, fracture resources, dispatch cadence, active chunks, and total memory.

## Non-Goals

- Do not reopen accepted Stage 5 Pressure, Wake, or Impact Ripple visuals without a concrete integration defect.
- Do not integrate Impact Ripple-to-Film behaviour in the current Stage 6 plan.
- Do not use a shader-only procedural mask to invent macro topology.
- Do not treat capacity as a final picture that material must directly copy.
- Do not make banks or obstacles unlimited emitters.
- Do not make distributed random births the primary shape source.
- Do not expose internal coefficients as public controls before proving they require independent authorship.
- Do not continue coefficient tuning around a topology representation that fails Batch 1A.
- Do not let tiny fragments become the dominant identity.

## Failure Gate

If Batch 1A cannot produce a convincing paused composition with the field-based topology representation, stop.

The next implementation must change representation to a low-rate GPU graph/ribbon topology rasterised into the same shared capacity and material fields. The persistent material simulation should remain field-based.

Do not respond with another breakup, supply, Voronoi, or noise-coefficient patch around a failed representation.

## Stage 6 Completion

Stage 6 closes only after:

1. Batch 1A topology plumbing is accepted.
2. Batch 1B interaction integration and diagnostics are accepted.
3. Batch 1C finite-structure topology is visually accepted.
4. Batch 2 persistent material response is accepted.
5. Final rendering and six-control authoring are accepted.
6. PC-first performance, quality, lifecycle, and long-running regression pass.
7. The roadmap is updated with a conservative validated summary.
