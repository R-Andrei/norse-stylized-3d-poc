# River Foam Active Blockers and Next Patches

## Purpose

This is the short working document for current Stage 6 Foam blockers and immediate patch order.

Use this instead of the old problem-register document. Keep it current and compact. Do not turn it into a patch diary.

Canonical architecture lives in `River_Foam_Stage6_Architecture.md`. Macro stage order lives in `River_Rendering_Roadmap.md`.

## Current working state

The Foam system is in the `4.11C` manually-born persistent material phase.

The 5.4l refactor fixed the hidden spawn-scaling issue: one manual spawn now maps to one budgeted composition event instead of several hidden progressive writers.

The 5.4m realignment removes the old pattern/complexity/density birth controls from the active manual workflow and makes manual birth a single canonical source again. The source-fill field is keyed from source controls rather than event count, so repeated starts with the same settings keep the same broad footprint. This is intentionally plainer: birth should create stable candidate material, while later material evolution owns macro/meso breakup and the renderer owns micro breakup.

Current visual/lifecycle state after 5.5b/5.5c/5.5d:

- 5.5b made intrinsic macro deformation visible, but introduced an unacceptable lifecycle regression by letting `Presence` depletion shorten material life independently of `Remaining Life`;
- 5.5c repairs lifecycle authority: `Remaining Life`, Neutral Lifetime, and topology aging are the only systems allowed to decide when material dies;
- 5.5d changes the deformation from a current-preserving union into an area-balanced intrinsic wobble so Foam can bend back and forth without monotonic footprint growth;
- intrinsic runtime morphology is accepted as good enough for now;
- 5.6 adds render-only coupling between Final Foam and the existing river surface layer: macro waves, static pressure, lee/depression, ripples, disturbance gradients, and wake energy;
- 5.6b filters Foam interior lighting so ordinary granular water-surface variation does not dirty the clean white stylized Foam body, while strong surface features can still show through at reduced strength;
- 5.7/5.7b/5.7c add stored-state disturbance/surface coupling; validation shows the path works, with a practical authored `Surface Morph Strength` range around 2-4 and approximately 2.5 as the current working setting;
- 5.8/5.8b/5.8c/5.8d proved that lateral material motion needs body-scale authority rather than only edge crawl, but the local chaotic drift implementation is now treated as an interim experiment rather than the long-term macro movement authority;
- 5.9 replaces that local chaotic drift path with the Unified Foam Motion Field: a dense scrolling lane field plus a fixed obstacle-routing override field. The field owns lateral macro movement only; downstream travel remains phase transport, Remaining Life still owns death, topology still owns support/negative aging, and birth still creates source material only;
- better source-shape spawning, topology proof/calibration, and final visual fragmentation remain future work.

## Hard rules for the next patches

Do not rethink the river architecture.

Do not split thin Foam/current lines into a separate system during this milestone.

Do not add automatic anchored births yet.

Do not add new source families as a substitute for fixing manually-born material.

Do not use topology as direct Foam painting.

Do not tune random presets as a replacement for fixing material behavior.

Do not let birth-time randomness decide whether a source becomes a chip, slug, sheet, or empty gap.

Do not proceed to automatic population until manually-born material can move, morph, age, and interact correctly.

Keep the layer split explicit: birth creates stable source material; persistent material simulation owns macro/meso reconfiguration, but must not shorten lifespan outside the Remaining Life equation; the final shader owns micro breakup, crisp presentation, Foam interior clarity, and render-only water-surface coupling. Render coupling may warp/thin/stretch the Final Foam mask near existing material, but it must not change `Material Presence`, Remaining Life, or Foam population. Ordinary high-frequency water detail should be suppressed on Foam interiors; strong waves/wakes/disturbances may imprint at reduced strength.

## Completed precondition — Manual source realignment

### Symptom

Before 5.4m, the manual birth path could produce wildly different macro results with the same settings: tiny fragments, short tadpoles, fat blobs, or disconnected islands.

### Why it matters

All later validation depends on a trustworthy manual source. If the source is already malformed, temporal morphing, topology aging, and obstacle behavior cannot be judged cleanly.

### Likely current cause

Compute-side birth injection still starts from a swept capsule/segment and applies destructive pattern/noise masks. Pattern logic is trying to create final Foam art at birth instead of creating stable source material for later persistent behavior.

### Correct behavior

Manual birth should create a bounded, stable candidate source with predictable macro footprint. Randomness may vary edge damage, local holes, and small gaps, but it must not change the broad source identity.

`Amount` remains source-only spatial fill. It must not act as life, opacity, or random macro deletion.

### Implemented target

`4.11C.5.4m — Manual Source Realignment`


### Inspector organization rule

All manual birth controls live under `Foam Debug > Manual Birth Source`. Persistent travel diagnostics live under `Material Motion`; stored/visible footprint diagnostics live under `Material Shape`. Do not scatter source controls into unrelated foldouts.

### Acceptance gate

With fixed settings and repeated starts:

- approximate footprint remains stable;
- a higher Amount is visibly more filled than a lower Amount;
- source shape is not a random chip/slug lottery;
- topology aging and lifetime probes remain unchanged;
- one spawn still starts one budgeted composition event.

### Explicit exclusions

No automatic births. No obstacle-flow feature. No final beauty pass. No new broad architecture.

## Blocker 1 — Static shape / no temporal morphing

### Symptom

Foam material is born and then remains too static in shape. It moves downstream, but the internal silhouette and breakup do not evolve enough.

### Why it matters

The reference river is alive: sheets tear, edges crawl, connections appear/disappear, and fragments peel away. Static decals moving downstream will not match that language.

### Likely current cause

Too much visual identity is defined at birth. Existing render/material breakup is not strong enough to evolve the material footprint while preserving state correctness.

### Correct behavior

Existing Foam should visibly change over time without creating hidden material and without using topology as motion guidance. Edges should crawl, interiors should breathe/break, and near-death material should visibly fragment or thin.

### Implemented targets

`4.11C.5.5 — Persistent Foam Morphing and Gradual Erosion` added the first conservative pass, but validation showed it was too local and edge-biased.

`4.11C.5.5b — Macro Material Deformation` strengthens persistent-state deformation so the stored material body bends, stretches, and changes width distribution over time.

`4.11C.5.5c — Lifecycle Authority Repair` removes the regression where simulation-side `Presence` erosion and empty-sample blending could make Foam disappear before `Remaining Life` expired.

`4.11C.5.5d — Area-Balanced Foam Wobble` removes the current-preserving union behavior that made deformation accumulate/grow. Morphing now uses opposed, normalized wobble samples so material can locally widen, narrow, bend, and relax while average occupied area remains roughly stable until lifecycle/topology actually removes it.

### Accepted layer split

This is persistent material-simulation behavior, not topology and not final shader-only polish. The simulation may reconfigure stored `Presence` over time so a patch can stretch, bend, and crawl. It may not delete material independently of `Remaining Life`; visual thinning/fragmentation must either be driven by the lifecycle equation or remain presentation-only until a lifecycle-safe stored-fragmentation design is approved. The renderer remains responsible for micro fracture and final crispness.

### Acceptance gate

A manually-born patch observed over several seconds changes `Material Presence`, not only Final Foam. It should show clear macro change: altered curvature, different width distribution, local stretch/compression, and visible back-and-forth wobble/relaxation. The average occupied area should remain roughly stable before lifecycle/topology removal. It must not grow without bounds, snap, create new topology-painted material, or disappear independently of `Remaining Life`. This blocker is accepted as good enough for now after 5.5d.

## Blocker 2 — Final Foam surface coupling and clarity

### Symptom

Material Presence now morphs acceptably, but Final Foam can still read as a flat white layer sliding over the water. After first surface coupling, the opposite problem also appears: fine water-surface/noise variation can contaminate the Foam interior and make the clean stylized white body look too granular.

### Why it matters

The reference reads as pale surface film attached to moving water, not paper floating over it and not noisy water texture painted on top of Foam. Intrinsic Foam morphology is necessary, but the final mask also needs controlled coupling to waves, pressure ridges, lee depressions, ripples, and wake energy.

### Correct behavior

Final Foam may be visually warped, stretched, compressed, or edge-modulated by macro waves, static pressure, lee/depression, ripple gradients, disturbance velocity, and transported wake energy. Foam interiors should remain clean and mostly white: ordinary high-frequency water detail is suppressed, while strong surface peaks/valleys may show through at reduced strength. Render coupling itself must not create Foam or change lifecycle state; stored-state surface coupling is limited to the approved 5.7 morphology input.

### Implemented targets

`4.11C.5.6 — Surface-Coupled Foam Rendering`

`4.11C.5.6b — Foam Surface Clarity Filter`

`4.11C.5.7 — Surface-Driven Material Morphing`

`4.11C.5.7b — Surface Morph Calibration`

`4.11C.5.7c — Surface Morph Formula Rebalance`

### Acceptance gate

In Final Foam view, existing Foam visibly reacts to the same surface disturbances that move/shape the water while the body remains stylistically clean. In Material Presence view, stored material should show approved 5.7 surface-amplified morphology, but lifetime and population must remain unchanged. `Surface Morph Strength = 0` should remove the stored-state surface response. After 5.7c, `1` should be the normal readable authored response, `2` should read strong, and `3+` should be treated as overdrive/stress-test behavior rather than the expected operating point.

### Follow-up

5.7 now samples the same river surface/disturbance fields in the persistent material simulation and uses them only as a bounded morphology-strength/bias input. 5.7b adds a single calibration control for A/B testing. 5.7c rebalances the internal response curve so mid-strength disturbance values become visually meaningful at strength `1` while overdrive remains clamped. It must remain separate from birth, topology painting, and lifecycle authority.

## Completed Blocker 3/4 — Unified Foam Motion Field

### Why 5.8 was superseded

`4.11C.5.8/5.8b/5.8c/5.8d` proved the right visual direction: stored `Material Presence` needs coherent, body-scale lateral movement instead of a stiff lane-locked strip or edge-only crawling. However, that implementation lived as a local chaotic drift resolver inside the persistent material morph function. It was hard to inspect directly, mixed macro body motion with meso/edge activity, and could not also solve obstacle routing without adding another competing steering path.

The approved correction is not to tune the local drift path. The approved correction is to remove it as an active macro movement authority and replace it with one explicit, inspectable motion-field layer.

### Implemented target

`4.11C.5.9 — Unified Foam Motion Field`

### Correct behavior

The Foam Motion Field answers one question: `at this river/Foam position, which lateral direction should stored Foam material prefer?`

The field is dense across valid river area. It should be red/orange or blue/cyan almost everywhere in its debug view, with black only where the generated lane field intentionally resolves near neutral/calm. It is not an activity mask and not an obstacle-only field.

### Data model

The implementation uses two separate logical fields because the dense lane field scrolls while obstacle routing must stay fixed in river space:

- `PS3D_RiverFoam_MotionLane` / `_FoamMotionLane` stores one signed lateral lane value in `RHalf`: `-1` for left, `0` for neutral, `+1` for right. The lane generator is a denser layered/domain-warped fractal field rather than a small set of broad bands, so the debug view should read closer to a granular turbulent noise map split into two colours.
- `PS3D_RiverFoam_ObstacleRouting` / `_FoamObstacleRouting` stores fixed obstacle override data in `RGHalf`: `R` is signed lateral routing direction, `G` is obstacle influence/override weight.

A one-texture implementation was rejected because a valid version still needs two different sample coordinates: scrolled UV for the lane field and fixed UV for obstacle routing. Packing them would not reduce the actual simulation sample count and would make the two responsibilities easier to confuse.

### Runtime performance contract

Steady-state simulation cost is deliberately small:

- two lane texture loads along X plus interpolation for smooth fractional scrolling;
- one obstacle-routing texture load at the fixed coordinate;
- no procedural multi-octave lane noise inside `SimulateFoam`;
- no local obstacle search inside `SimulateFoam`;
- no full-field rebuild because time passes;
- no CPU Foam-cell loops;
- no GPU readback.

Lane evolution is a sample-coordinate phase scroll controlled by `Motion Field Scroll Hz`, not a rebuild rate.

### Regeneration rules

The dense lane field regenerates only when its inputs change: domain/dimensions, visual seed, neutral coverage, or lane scale.

The obstacle-routing field regenerates only when obstacle/domain data changes. It is generated by grouping the existing obstacle exclusion cells into connected obstacle bodies, stamping one coherent lateral routing region per body, and then uploading the routing texture. It deliberately does not stamp per obstacle cell, because per-cell side choice creates left/right striping and indecisive routing around a single stone.

### Controls

The active controls under `Foam Debug > Material Motion` are:

- `Motion Field Strength` — lateral macro field strength; `0` disables field-driven lateral movement, `1` is normal, higher values exaggerate.
- `Motion Field Scroll Hz` — full downstream lane-field wraps per second; default `0.01`, meaning one full wrap every 100 seconds.
- `Motion Field Neutral Coverage` — approximate fraction of the dense lane field that resolves to neutral/black; default `0.10`.
- `Motion Field Lane Scale` — broadness/fineness of the generated lane pattern; changing it regenerates the lane texture only.

The old `Chaotic Drift Strength` and `Chaotic Drift Rhythm` controls are removed from active code and UI.

### Obstacle routing contract

Obstacle routing is lateral-only. It must not produce upstream compression, downstream acceleration, radial repulsion, attraction, Foam death, Foam birth, or topology painting.

Obstacle influence overrides the lane field near obstacles and blends only through its falloff edge:

`finalLateral = lerp(laneLateral, obstacleLateral, obstacleInfluence)`

Each obstacle body writes a longer, weaker upstream approach region so incoming Foam begins redirecting before contact. The near-obstacle region is stronger and reaches override authority. Direction is resolved relative to the whole obstacle body centerline, with a stable lane-informed tie-break only for centerline cases; neighboring cells inside the same rock must not alternate left/right.

### Debug contract

`Foam Motion Field` debug view shows the same fields sampled by the simulation:

- blue/cyan = leftward lateral stored-material motion;
- red/orange = rightward lateral stored-material motion;
- black = intentional neutral/calm region;
- green/yellow = obstacle override influence;
- semi-transparent white = current Foam mask overlay.

The debug view is required in Play Mode. It must not silently fall back to the final river render when selected.

### Acceptance gate

In Motion Field debug, most valid river area should show left/right directional colour, not black. Spawned Foam should appear as a translucent white overlay over the field. In Material Presence / Final validation, the broad Foam body should move laterally according to the field, obstacle regions should override the lane direction, Foam should begin weakly redirecting upstream of obstacles and route around them near contact, and there should be no upstream push, radial repulsion, compression stutter, hidden early death, or birth/topology changes.

## Blocker 5 — Source shapes still need improvement

### Symptom

Manual birth is stable enough for testing, but it can still create source material that reads too broad, fat, or blob/ribbon-like for the intended final river Foam language.

### Why it matters

Final visual fragmentation cannot fully compensate for bad source geometry. Birth should not decide the final art species, but it does need to create usable initial source shapes for the persistent simulation to evolve.

### Correct behavior

Birth should create stable but usable source shapes such as thin downstream ribbons, broken sheets, forked bands, and smaller seeded clusters. It should not return to destructive birth-time randomness or hidden chip/slug/blob lotteries.

### Next patch target

`4.11C.5.10 — Better Manual Source Shape Spawning`

### Acceptance gate

Repeated starts with identical settings remain comparable, Amount remains source-fill semantics, and the source shape is better suited for downstream evolution without pretending to be the final visual beauty pass.

## Blocker 6 — Topology proof and calibration still needed

### Symptom

The debug topology shows support and negative regions, but the visible material response is not yet proven clearly enough across typical test cases.

### Why it matters

Topology is the main reason Foam should persist in some places and open dark pockets in others. Better movement and better source shapes should make this easier to judge, but the support/negative aging contract still needs a focused proof pass.

### Correct behavior

Foam in positive support should survive and remain more coherent. Foam in negative aging pressure should thin, fragment, and die faster. Overlap should resolve according to the approved aging equation.

### Next patch target

`4.11C.5.11 — Topology Aging Proof and Calibration`

### Acceptance gate

Using the Foam + Aging Topology view and Final Foam view, the same material source must show a clear lifetime/shape difference when passing through neutral, supported, negative, and overlapping regions.

## Recommended sequence

1. Validate `4.11C.5.9 — Unified Foam Motion Field` in `Foam Motion Field`, `Material Presence`, and `Final Foam` views. Confirm the field is dense, Foam overlays in white, lateral movement is body-scale, obstacle routing overrides near obstacles, and no lifecycle/birth/topology ownership changed.
2. `4.11C.5.10 — Better Manual Source Shape Spawning`
3. `4.11C.5.11 — Topology Aging Proof and Calibration`
4. `4.11C.5.12 — Final Foam Visual Fragmentation and Reference-Matching`

Only after these pass should the project continue to automatic anchored/open-water birth population.

## Deferred work

Deferred until manual material is accepted:

- anchored automatic birth events;
- open-water birth scheduling;
- spatial fairness/population control;
- mature Foam rendering polish / final visual fragmentation;
- final reference-matching pass;
- production performance/regression closure.

## Maintenance rule

This document should stay short. When a blocker is solved, move its result into a brief status line and continue. Do not paste long patch histories here.

## 4.11C.5.9e obstacle routing correction note

The 5.9 obstacle-routing field was corrected after debug validation. Obstacle influence is now collision-risk based rather than raw proximity based. Strong override is reserved for Foam on a likely collision course directly upstream of an obstacle. Foam beside an obstacle receives only low influence and should generally be carried downstream by phase transport. The routing field uses a rounded flow-relative envelope with weak upstream approach, strong direct-front collision correction, weak side skirt, and short downstream release. Runtime simulation cost is unchanged: two lane loads plus one obstacle-routing load.
