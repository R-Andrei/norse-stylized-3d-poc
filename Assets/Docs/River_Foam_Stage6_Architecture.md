# PS3D River Stage 6 — Integrated Filament-Network Foam Architecture

## Status

- **Stage:** 6 — Foam and Surface Tracing
- **Current implementation:** Stage 6.1 Cohesive Web and Fragment Correction implemented in code; Unity compilation and focused visual validation pending
- **Supersedes:** the original F1/F2A sequence, F2A.2–F2A.7, the first integrated F1–F4 solver, coverage-only population control, cardinal-grid fracture/reconnection, one-pass diffusive advection, and weak attraction-only boundary behaviour
- **Current implementation scope:** persistent material, lane-aware population, metric multi-scale filament guidance, corrected transport, persistent coherent fracture state, donor-causal merging, animated-shore capture, projected-obstacle interaction, stationary Pressure/lee capture, Wake and Impact reinforcement
- **Remaining work after acceptance:** final visual/material tuning, quality profiling, regressions, and Stage 6 closure
- **Authority:** current project files remain authoritative

The latest Unity validation established that the first integrated solver had reached the correct broad category—an evolving connected field—but still exposed its construction. It produced too much visible area, too few pockets, broad sheet-like structures, cardinal row/column fracture patterns, synchronized scalloping, smooth edges, apparent upstream connection growth, and inadequate shore/lee retention.

Stage 6.1 follows the Integrated Dynamics Correction visual review. It keeps the strongest still-image result but replaces the remaining sparse-line guidance, animated per-pixel breakup, and velocity-only upstream guard with metric web guidance, persistent coherent damage, stable rendering, and full donor-causal transport rules.

---

## 1. Visual target

Foam is a stylised, persistent surface material that organises into an ever-changing partial network.

The desired vocabulary includes:

- many small and medium dark-water pockets;
- thin and medium branches;
- extremely narrow temporary connectors;
- forks and junctions;
- occasional broad nodes;
- detached ribbons, islands, splinters, and tiny fragments;
- rough, cracked, asymmetric edges;
- merging through actual material convergence;
- later re-separation along structurally weak seams;
- strong but bounded retention at shores, obstacles, stationary Pressure shoulders, and lee depressions;
- peeling and shredded release from captured regions.

The network is not permanently connected. It should continually move between connected, partly connected, and fragmented states.

The dominant sequence is:

```text
persistent material
→ multi-scale gathering into narrow filaments
→ downstream stretching and lateral shear
→ real overlap and merging
→ edge nicks and oblique crack growth
→ weak-seam and neck failure
→ tiny and large detached fragments
→ capture at boundaries and lee regions
→ shredded release and later reconnection elsewhere
```

The system must not read as:

- isolated one-second blobs;
- a static translated web;
- a scrolling texture;
- broad white sheets with a few large holes;
- a coverage target satisfied by one or two Foam continents;
- a procedural visibility mask;
- row/column/checker fracture;
- timed dotted perforations;
- material or connection fronts moving upstream;
- blur-like dissolution;
- temporary no-Foam strips or pockets;
- permanent shore outlines or complete rock halos.

---

## 2. Non-negotiable movement contract

The authoritative river domain determines downstream direction.

The solver may:

- accelerate downstream transport;
- slow downstream transport;
- nearly stall material in strong capture zones;
- move material laterally;
- bend downstream paths;
- circulate laterally around a retained region.

The solver may never produce negative velocity along the authoritative downstream axis.

Every secondary contribution—guidance, shore attraction, obstacle deflection, stationary Pressure, lee capture, Wake, Ripple response, and phase drift—is combined first. The final longitudinal velocity is then projected onto the authoritative downstream direction and clamped to a non-negative magnitude.

Reverse-flow rivers use the opposite texture-X sign, but the same rule: all visible motion remains downstream relative to the active river flow.

Connections may form only through:

1. actual advection/convergence of persistent material; or
2. conservative local overlap and phase mixing after material physically meets.

The former rule that inserted material between opposing cardinal neighbours has been removed. No bridge may grow upstream or appear across an empty gap without donor material.

---

## 3. Preserved cross-stage contracts

The correction retains accepted systems that are independent of the rejected Foam dynamics:

- authoritative `RiverDomainSnapshot` coordinates;
- metric-aware world-space transport through bends and width changes;
- reverse-flow support;
- river-owned resource lifecycle;
- quality-scaled material and guidance resolutions;
- fixed simulation cadence per quality;
- freeze clearing;
- Amount-zero decay, sleeping, and delayed resource release;
- corrected per-vertex projected stationary-obstacle contours;
- registered generated-geometry lifecycle;
- Stage 5 Pressure, Wake, Impact Ripple, and static-source contracts;
- fixed-cost final water-shader sampling;
- compact canonical authoring controls.

Stage 5 remains visually closed. Stage 6 receives read-only access to accepted Stage 5 textures. It does not write to or reinterpret their visual response.

---

## 4. Persistent material state

The shared material state remains two `RGBAHalf` ping-pong textures:

```text
R = Amount
G = Freshness
B = Integrity
A = material phase / provenance
```

### Amount

Amount is persistent tracer material.

It:

- is transported through the authoritative river domain;
- gathers into lanes;
- overlaps and merges;
- separates through real state loss;
- survives substantially longer than the initial source imprint;
- decays according to Persistence and capture conditions.

### Freshness

Freshness represents recent creation or reactivation.

It decays much faster than Amount. This keeps the initial generated appearance short-lived while allowing the material itself to travel for many seconds.

Freshness affects:

- early structural resistance;
- restrained brightness variation;
- response to newly supplied material;
- Wake, lee, and Impact reactivation;
- the transition from new material to damage-prone material.

Tiny maintenance supply is mass-weighted and cannot reset an old structure to fully fresh.

### Integrity

Integrity is persistent structural resistance.

It is damaged by:

- age;
- directional strain;
- exposed tips;
- low directional support;
- narrow necks;
- phase disagreement;
- evolving-guidance shear;
- oblique crack propagation;
- internal defects;
- turbulent Wake and strong impacts.

Low Integrity does not immediately remove material. It increases susceptibility to:

- continuous edge shredding;
- tip peeling;
- crack propagation;
- internal pitting;
- nonlinear neck collapse.

### Material phase / provenance

Phase is a transported scalar marker, not a particle identifier.

It provides:

- independent material-specific timing;
- local fracture-orientation variation;
- weak seams where differently sourced material meets;
- conservative phase mixing after real overlap;
- reopening of merged seams when motion remains incoherent.

---

## 5. Corrected transport

The rejected solver used one-pass bilinear semi-Lagrangian advection. It was stable but numerically diffusive:

- narrow strands broadened;
- sharp notches softened;
- one-to-three-cell fragments vanished;
- rough edges converged toward smooth contours.

The correction uses a bounded MacCormack/BFECC-style sequence for every active region:

1. **Forward advection:** persistent state is traced backward through the resolved downstream-only velocity into a temporary `RGBAHalf` texture.
2. **Reverse estimate:** the forward result is advected with the opposite integration sign into a second temporary `RGBAHalf` texture.
3. **Bounded correction and evolution:** the estimated error is applied to Amount, Freshness, and Integrity, then clamped to the original bilinear neighbourhood before population, topology, damage, capture, reinforcement, and decay are evaluated.

Phase follows the amount-weighted forward result rather than receiving a linear correction across its circular range.

Persistent textures:

```text
State A          RGBAHalf
State B          RGBAHalf
Forward scratch  RGBAHalf
Reverse scratch  RGBAHalf
```

The additional dispatch and memory cost is intentional. Preserving thin strands, cracks, and tiny fragments is central to the feature and cannot be delegated to final-shader noise.

---

## 6. Perimeter-aware population control

The previous population controller measured visible area only. A few broad sheets were therefore the cheapest way to satisfy the target.

The GPU-only raw metrics buffer now stores eight values per Foam chunk:

```text
quantised Amount sum
visible-cell count
valid fluid-cell count
guidance-lane cell count
perimeter-cell count
broad-interior-cell count
shore-capture visible count
visible guidance-lane count
```

The D3D11 signed integer-division warning has been removed by using unsigned chunk indexing.

The active controller evaluates:

- visible coverage;
- perimeter coverage;
- perimeter-to-visible ratio;
- broad-interior coverage;
- available guidance-lane area;
- occupied guidance-lane ratio;
- target coverage from Amount.

Initial target range:

- very low Amount: approximately `3.5%`;
- ordinary Flowing settings: generally within the lower-middle portion of the range;
- maximum Amount: approximately `28%`.

These remain implementation calibration values rather than new user controls.

Supply is reduced when:

- broad interiors consume too much area;
- the local neighbourhood is already populated;
- a location is inside an enclosed hole or recent separation;
- the current chunk already meets its target.

Supply is favoured when:

- measured population is below target;
- a useful guidance lane is under-populated;
- the broad neighbourhood has capacity;
- the region lacks network presence.

Population control never deletes material to satisfy its target. Excess population falls through downstream transport, structural failure, and natural decay.

---

## 7. Multi-scale filament guidance

The guidance field remains an invisible, low-resolution `RGBAHalf` texture.

Current contract:

```text
R,G = direction toward the nearest useful filament lane
B   = lane strength
A   = branch/junction overlap
```

The field is generated from three independently evolving structural scales:

### Coarse network

- establishes a limited number of large river divisions;
- contributes broad composition;
- is deliberately weaker than the previous coarse network.

### Medium network

- carries most visible branches;
- defines normal pocket sizes;
- produces forks and ordinary junctions.

### Fine incomplete network

- creates secondary connectors;
- produces smaller pockets;
- adds short and interrupted branches;
- is gated by smoothly evolving regional noise so it does not become a uniform micro-grid.

The three scales use separate spatial frequencies, seeds, and temporal rates. Cellular centres move on independent regional phases rather than one global pulse.

The guidance field is never rendered and never directly writes Amount. It modifies persistent-material velocity. When guidance evolves, Foam must physically move, stretch, converge, detach, or decay.

Distance to the lane is retained while building the gradient. This allows narrow attraction zones instead of the former broad normalized basins.

---

## 8. Directionally rotated topology

The rejected topology used decisive left/right and up/down pairs. That exposed the texture lattice through rows, columns, checker patterns, and dotted perforations.

All cardinal bridge construction has been removed.

The active solver evaluates a rotated directional frame derived from:

- transported phase;
- smoothly varying river-space noise;
- local material coordinates;
- Agitation.

Eight bilinearly sampled directions are evaluated around each cell, grouped into four opposing axes that are not aligned to texture X/Y unless the local rotation happens to match them.

The solver derives:

- directional average support;
- strongest supported axis;
- orthogonal support;
- edge exposure;
- neckness;
- tipness;
- phase stress;
- broad local population.

This directional frame changes gradually through material space. Structural failure therefore follows the actual weakest local direction rather than repeating horizontal and vertical motifs.

---

## 9. Persistent coherent fracture and tearing

Small fracture remains a primary visual language, but independent material-pixel destruction is rejected.

### Fracture state

Two half-resolution `RGHalf` ping-pong textures store:

```text
R = accumulated structural damage
G = crack coherence / persistence
```

The field is advected downstream, updated below the main material rate, and never rendered directly. It is persistent state rather than a temporary deletion mask.

### Damage sources

Connected damage accumulates from:

- material age;
- weak directional support;
- narrow necks;
- exposed edges;
- phase disagreement at merged seams;
- guidance-direction shear;
- turbulent Wake;
- strong Impact activity;
- low-frequency regional defect opportunity.

The regional signal controls where damage begins, but the stored state controls how it grows and survives. Changing procedural noise cannot restore removed material or blink a crack out of existence.

### Coherent propagation

Fracture propagation samples a phase-rotated local frame in the lower-resolution field. Neighbouring damage pulls a crack front through connected regions, preventing unrelated full-resolution cells from choosing independent failure times.

The main material solver uses fracture damage and coherence to drive:

- irregular edge bites;
- peeling tips and shelves;
- internal fissures;
- nonlinear neck collapse;
- coherent small-group detachment.

The expected progression is:

```text
connected stress region
→ persistent Integrity loss
→ asymmetric indentation or fissure
→ coherent edge group peels
→ neck weakens
→ fragment separates and survives downstream
```

### Fragment survival

Small detached groups receive reduced natural decay and reduced continuing tear loss. They remain identifiable long enough to travel before shrinking cleanly.

No animated per-cell shred noise, stippled threshold cloud, timed fracture strip, deletion pocket, complete row, or checker gate remains.

---

## 10. Merging and reconnection

There is no explicit mass-creating bridge rule.

Structures merge when corrected transport and convergent guidance bring their Amount fields into real overlap.

After overlap:

- compatible phase groups mix gradually;
- incompatible phase groups retain a weak seam;
- continuing differential movement can reopen the seam;
- coherent movement can stabilise it.

This supports temporary connectivity without broad dilation.

Supply is strongly suppressed inside small enclosed gaps and cannot be used as a hidden bridge constructor.

---

## 11. Shore, obstacle, Pressure, and lee capture

Attraction alone was insufficient. The correction implements capture as a complete local material regime.

### Animated shores

The existing boundary texture stores:

```text
R = valid fluid coverage
G = boundary-attraction/capture band
```

Inside the animated shore-capture band:

- attraction toward the visible edge is stronger;
- downstream velocity is reduced sharply but remains non-negative;
- lateral/tangential movement remains possible;
- Amount decay is reduced;
- limited Integrity support resists immediate washout;
- old exposed captured edges still shred and peel.

Capture capacity remains bounded. The result must be intermittent branches and pockets, not a continuous white outline.

### Projected stationary obstacles

The corrected polygon remains authoritative:

1. resolve the cached source-local contour;
2. reconstruct every world-space contour vertex;
3. project each vertex through `RiverDomainSnapshot`;
4. rasterise the projected polygon and projected bounds.

Material:

- remains excluded from solid cells;
- is attracted toward selected perimeter regions;
- splits around real shoulders;
- can remain temporarily at side contacts;
- releases from downstream regions.

### Stationary Pressure

Stage 6 receives read-only access to the accepted Stage 5 static Pressure texture.

Pressure height and gradients provide:

- modest upstream/shoulder capture;
- bounded local steering;
- additional retention near attached pressure regions.

Stage 5 Pressure rendering and simulation are unchanged.

### Lee depression

The accepted stationary Wake source lee channel is the strongest static capture influence.

Inside a lee:

- downstream motion can approach a stall but never reverse;
- retention is much stronger than in open water;
- decay is reduced;
- material receives limited structural support;
- local capacity and ongoing edge damage prevent a permanent white pool.

### Release

Captured material is not released as one intact sliding patch.

Age, exposed edges, local shear, turbulence, and Fragmentation cause:

- rear-edge cracking;
- tiny shedding;
- peeling branches;
- eventual larger release;
- replacement by newly arriving material.

---

## 12. Wake and Impact reinforcement

Wake and Impact activity modify the same persistent material.

### Wake

Strong Wake may:

- stretch existing strands;
- reinforce downstream branch paths;
- increase local edge damage;
- freshen and strengthen a limited amount of existing material;
- cooperate with lee-release topology.

Weak Wake remains subtle.

### Impact

Strong Ripple activity may:

- push and shear existing material;
- weaken links;
- create temporary forks;
- add a bounded amount of fresh material;
- shed small fragments.

Every longitudinal motion contribution still passes through the downstream-only clamp.

No separate Foam overlay is rendered.

---

## 13. Rendering contract

Macro topology comes only from the persistent simulation.

The final water shader uses:

- Amount;
- Freshness;
- Integrity;
- phase;
- derivative-aware threshold extraction;
- Foam Colour alpha;
- transported multi-scale edge roughness.

Shader detail is restricted to a narrow silhouette band and contains no time-varying threshold term. It may add:

- sub-cell nicks;
- phase-varied roughness;
- small brightness variation;
- weak-Integrity raggedness.

It may not create:

- macro holes;
- complete cracks;
- branches;
- connections;
- deletion masks;
- upstream apparent motion.

The former two shared sine waves were replaced with phase-offset value-noise/FBM detail so neighbouring edges do not share one geometry or tempo.

---

## 14. Canonical controls

Normal authoring remains:

```text
Enabled
Preset
Amount
Fragmentation
Persistence
Agitation
Sharpness
Foam Colour
```

### Amount

Controls:

- target visible population;
- supply capacity;
- broad visual strength.

Maximum Amount does not mean complete coverage.

### Fragmentation

Controls:

- Integrity damage;
- edge shredding;
- crack propagation;
- weak-seam failure;
- neck collapse;
- connection survival.

It does not control global lifetime.

### Persistence

Controls:

- Amount lifetime;
- fragment lifetime;
- long-distance survival.

It does not suppress splitting.

### Agitation

Controls:

- guidance evolution;
- lateral motion;
- differential shear;
- crack activity rate;
- network reconfiguration.

It cannot reverse downstream movement.

### Sharpness

Controls final edge hardness only.

### Foam Colour

Controls lit tint and canonical maximum opacity through alpha.

No normal-facing Webness, Cohesion, Integrity, Capture, or Transport control is introduced.

---

## 15. Debugging and diagnostics

Retained and added debug views:

```text
Amount
Freshness
Final Mask
Integrity
Phase
Guidance
Capture
```

- **Guidance** shows lane direction and strength.
- **Capture** combines boundary, stationary Pressure, and lee influences.

Runtime diagnostics report:

- material resolution;
- guidance resolution;
- update rate;
- corrected-advection status;
- active chunks;
- pending injections;
- reservations;
- dispatch and cell-iteration peaks;
- estimated memory.

Population metrics remain GPU-only in normal production. They are consumed directly by the supply controller.

---

## 16. Quality and performance model

Quality tiers independently scale:

- material resolution;
- guidance resolution;
- simulation rate;
- field memory;
- cell iterations.

All tiers retain corrected transport. Low quality does not return to the visibly destructive one-pass solver.

Expected additional cost compared with the rejected integrated solver:

- two additional `RGBAHalf` scratch textures;
- forward and reverse advection dispatches;
- denser multi-scale guidance;
- eight-value population metrics per chunk;
- additional directional topology samples.

The target remains desktop PC first, including low-to-medium hardware. The cost is justified only if the corrected solver visibly preserves thin topology and chaotic tearing.

No CPU readback, per-obstacle final-shader loop, or individual Foam particle simulation is introduced.

---

## 17. Focused Unity acceptance

### Compilation and warnings

- Unity compiles without errors.
- The former D3D11 integer-division warning in `MeasurePopulation` is absent.
- No new compute warning appears.

### Downstream authority

- No coherent Foam feature travels upstream.
- No merge front grows upstream.
- Shore/lee capture may stall but never reverse material.
- Reverse-flow rivers retain the same downstream-only rule in the opposite world direction.

### Macro topology

- Many small and medium pockets coexist.
- Thin connectors are common.
- Broad white sheets are uncommon in open water.
- Wider nodes occur mainly at junctions and capture zones.
- Maximum Amount preserves substantial open water.
- Ten-second and sixty-second population remain broadly comparable.

### Chaotic tearing

- No row, column, checker, or regular dotted fracture is visible.
- Small fractures occur continuously and asynchronously.
- Jagged cracks propagate obliquely.
- One-to-three-cell fragments visibly detach where resolution permits.
- Neighbouring structures use different tempos and geometry.
- High Fragmentation looks violent without simply emptying the river.

### Merging

- Connections form through actual convergence.
- No material appears across an empty gap without supply or donor overlap.
- Merged seams may stabilise or reopen.
- No grid-aligned dilation is visible.

### Capture

- Animated shores retain irregular branches without a continuous outline.
- Projected obstacles split and capture material around their real geometry.
- Pressure shoulders receive secondary accumulation.
- Lee depressions retain material much longer than open water.
- Captured Foam eventually shreds and peels away.
- Capture does not drain the complete centre of the river.

### Lifecycle

- Initial source character changes quickly.
- material survives for many seconds according to Persistence;
- Amount zero stops supply and returns to sleeping/release;
- full freeze clears and suppresses Foam;
- thaw restarts cleanly;
- quality switching, scene reload, geometry registration/removal, and reverse flow remain stable.

---

## 18. Remaining Stage 6 work

After the Integrated Dynamics Correction passes focused validation:

1. tune the Subtle, Flowing, and Whitewater presets around the accepted dynamics;
2. finalise lit Foam material response, opacity, refraction suppression, and normal detail;
3. profile Low/Medium/High quality on representative hardware;
4. run long-duration population, freeze, reverse-flow, obstacle, Wake, and Impact regressions;
5. remove temporary investigation text and close Stage 6.

Detached spray, droplets, mist, and splash particles remain Stage 7. Final reflection integration remains Stage 8.


## 15. Stage 6.1 — Cohesive Web and Fragment Correction

**Status:** implemented; Unity validation pending.

The first Integrated Dynamics Correction improved the absolute still-image quality but exposed two remaining root defects in motion: the guidance field produced mostly elongated lanes rather than a sufficiently connected web, and high-frequency simulation plus shader threshold noise broke edges into a flickering particle cloud.

Stage 6.1 implements the following replacement contracts:

1. **Metric multi-scale guidance**
   - Network cells are constructed from global river distance and across-river metres rather than normalized texture coordinates.
   - Medium-scale lanes dominate normal topology; coarse lanes provide broad structure and fine lanes provide incomplete connectors.
   - Guidance updates at `4/6/8 Hz` for Low/Medium/High while material transport remains `12/20/30 Hz`.

2. **Lane-aware population control**
   - Population metrics now distinguish valid fluid area, visible area, perimeter, broad interior, available guidance-lane cells, and occupied guidance-lane cells.
   - Supply prioritizes unoccupied network lanes before thickening existing material.
   - Broad interiors and enclosed holes suppress birth.

3. **Persistent coherent fracture state**
   - Two half-resolution `RGHalf` ping-pong textures store accumulated damage and crack coherence.
   - The fracture field is advected downstream and updated at `8/10/12 Hz` for Low/Medium/High.
   - Stress comes from age, weak directional support, necks, phase seams, guidance shear, Wake, and Impact activity.
   - The main material field loses Integrity and Amount from the persistent fracture state rather than high-frequency independent cell noise.

4. **Stable rendering silhouette**
   - Time-animated edge thresholding is removed.
   - Rendering may add only small temporally stable phase-offset contour roughness.
   - A stable Amount field therefore produces a stable visible outline.

5. **Donor-causal downstream behaviour**
   - Longitudinal transport remains nonnegative in the authoritative flow direction.
   - Corrected advection is additionally limited by its physically valid donor region.
   - Distributed supply is suppressed immediately upstream of existing material.
   - Wake, Pressure, lee, and boundary reinforcement require existing material.
   - Merging only mixes already overlapping material and cannot insert Amount across a gap.

6. **Coherent fragment survival**
   - Small detached groups receive reduced natural decay and reduced tearing long enough to remain identifiable while travelling.
   - Fracture begins as connected low-resolution damage rather than unrelated one-pixel dropout.

### Stage 6.1 acceptance

- The autonomous result contains clearly more transverse/diagonal connectors and partial pockets than parallel lanes.
- Additional Amount populates more skeleton before substantially widening existing branches.
- No stippled particle cloud or independently phasing edge pixels remain.
- Small tears detach as coherent groups rather than visual noise.
- No visible merge front or material group travels upstream.
- The Fracture debug view shows connected damage regions rather than full-river deletion masks.
- Ten-second and sixty-second population remain broadly comparable.
- Shore and lee capture remain active without becoming permanent solid reservoirs.

---

## 16. Performance and cleanup audit

### Current GPU resource model

Per active river runtime, Stage 6.1 allocates:

- four full-resolution `RGBAHalf` material textures: two persistent ping-pong states plus forward and reverse corrected-advection intermediates;
- one low-resolution `RGBAHalf` guidance texture;
- two half-resolution `RGHalf` fracture textures;
- one CPU-generated boundary `RGHalf` texture;
- one tiny neutral Stage 5 fallback texture;
- one metric-row structured buffer;
- one tiny per-chunk raw population buffer.

The dominant memory is the four full-resolution material textures. The coherent fracture addition costs one quarter of a full-resolution `RGBAHalf` texture in total: each half-resolution `RGHalf` texture is one eighth of a full-resolution `RGBAHalf` texture, and there are two.

Ignoring tiny buffers and the one-pixel fallback, approximate texture memory is:

```text
4 × full-resolution RGBAHalf material states  = 32 × W × H bytes
1 × full-resolution RGHalf boundary           =  4 × W × H bytes
2 × half-resolution RGHalf fracture states    ≈  2 × W × H bytes
1 × low-resolution RGBAHalf guidance          =  8 × GW × GH bytes
```

For an approximately `160 m` river (`5` 32-m chunks), this is roughly `0.20 MB` at Low, `0.45 MB` at Medium, and `0.79 MB` at High before small buffers and Unity object overhead. Memory is therefore not the primary risk; repeated full-resolution texture sampling is.

### Dispatch cadence

While autonomous Amount is above zero, the main material solver runs three dispatches per simulation step:

```text
forward advection
reverse estimate
bounded correction + supply + topology + capture
```

Lower-frequency work is deliberately staggered:

```text
guidance:   4 / 6 / 8 Hz
population: 4 / 6 / 8 Hz, two dispatches per update
fracture:   8 / 10 / 12 Hz
```

This replaces the previous behaviour where guidance and full population measurement ran at every `12/20/30 Hz` material step. At Medium quality, the change reduces guidance dispatch frequency by `70%` and population-measurement frequency by `70%`, offsetting much of the new fracture-field cost.

### Expected bottlenecks

1. **Main full-resolution compute bandwidth** — the three corrected-advection passes dominate cost and scale with field width × field height × update rate.
2. **Neighbour sampling inside `SimulateFoam`** — directional support, broad support, phase stress, and external Stage 5 sampling are ALU/texture heavy.
3. **Always-active autonomous chunks** — any nonzero Amount keeps the full river field active by design; this is the largest scalability risk across many simultaneously visible rivers.
4. **Water-fragment shader cost** — fixed and source-count independent, but every water pixel still samples the two material states; debug-only guidance/fracture/boundary samples are compiled into conditional paths and should be verified by GPU profiling.

### Systems deliberately consolidated

- Ambient generation, merging, fragmentation, shore retention, obstacle behaviour, Wake, and Impact all modify one material state rather than separate rendered Foam layers.
- The accepted Stage 5 textures are sampled directly; no duplicate Foam-specific Wake, Pressure, Ripple, or obstacle field is allocated.
- One combined boundary texture carries both fluid coverage and environmental capture.
- Population metrics use one small raw buffer rather than CPU readback or a separate occupancy texture.
- Material phase doubles as lightweight provenance and asynchronous timing; no particle identity buffer exists.

### Deprecated or compatibility-only code

- `StylizedRiverFoamSimulation` is an inert migration guard. It disables itself and allocates no simulation resources. Remove attached instances from scenes/prefabs after migration, but keep the type until old serialized references are no longer needed.
- Hidden serialized Foam fields on `StylizedRiver` remain for one-time authoring migration and compatibility. They add negligible runtime cost but considerable maintenance clutter. After project scenes are resaved and backward compatibility is intentionally dropped, they should be removed in one dedicated serialization migration—not piecemeal during visual work.
- `_FoamOpacity` remains as a hidden serialized shader property, but it is no longer placed in the runtime constant buffer or rebound every frame; Foam Colour alpha is the only opacity input. The property and hidden legacy field can be deleted together in a future serialization migration.
- Manual injection reservations and diagnostic shapes remain useful for testing. They are dormant in production and do not justify removal yet.

### Current conflicts and resolved conflicts

Resolved in Stage 6.1:

- shader edge noise no longer fights the simulated topology;
- per-cell stochastic deletion no longer fights coherent fragment survival;
- guidance/population no longer reward only area and broad sheets;
- source birth can no longer imitate upstream merging near an existing branch;
- correction sharpening can no longer advance Amount beyond the upstream/lateral donor region.

Remaining architectural tension:

- corrected advection preserves thin structures but costs two extra full-resolution passes;
- autonomous river-wide population conflicts with chunk sleeping because Foam must be able to exist throughout the domain;
- environmental retention and aggressive fragmentation intentionally oppose each other and require visual balancing, although they now operate through shared Integrity/damage state rather than contradictory creation/deletion masks.

### Higher-performance alternatives

1. **Single-pass advection**
   - Cheapest option: remove reverse/correction passes.
   - Rejected for Medium/High because it visibly broadens strands and destroys small fragments.
   - Viable only as an explicit low-end quality mode after profiling, not as the production default.

2. **Two-pass BFECC approximation**
   - Could combine reverse estimation and correction in one specialized pass using additional local samples.
   - Saves one dispatch but may increase per-thread sampling and register pressure. It is a profiling candidate, not an assumed win.

3. **Packed lower-precision state**
   - `ARGBHalf` is robust but generous. Packing Freshness/Integrity/phase into normalized 8-bit channels could reduce bandwidth.
   - High risk of visible threshold instability and phase banding; only worth prototyping after visual acceptance.

4. **Shared atlas across rivers**
   - Multiple nearby rivers could share one allocation and dispatch schedule.
   - This improves allocation/dispatch overhead but substantially complicates lifecycle, variable dimensions, sleeping, and authoring. It is worthwhile only when profiling shows many simultaneous rivers are the real bottleneck.

5. **Distance-based cadence and resolution**
   - Highest-value future optimization.
   - Keep nearby gameplay rivers at full quality; reduce material rate, fracture rate, and guidance rate for distant chunks/rivers; freeze fully off-screen domains.
   - Must preserve deterministic state or accept a bounded catch-up step when reactivated.

6. **Precomputed/static guidance basis**
   - The expensive Voronoi skeleton could be rebuilt only when its next target is needed and interpolated for longer.
   - Stage 6.1 already lowers cadence; further reduction is likely safe if motion remains sufficiently alive.

### Recommended profiling order

1. Capture GPU timings for each compute kernel at Low/Medium/High on representative river lengths.
2. Test one, three, and six simultaneously active rivers.
3. Measure the main water draw separately from Foam compute.
4. Compare corrected advection against a diagnostic one-pass mode to quantify its true cost.
5. Test off-screen and distance-based throttling before considering state packing or an atlas.

The static workload counters remain estimates of dispatched cells, not measured GPU milliseconds. Stage 6 cannot be called performance-closed until Unity GPU Profiler or RenderDoc timings are recorded on the target low-to-medium desktop baseline.
