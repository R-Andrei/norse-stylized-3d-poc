# River Foam Active Blockers and Next Patches

## Purpose

This is the short working document for current Stage 6 Foam blockers and immediate patch order.

Use this instead of the old problem-register document. Keep it current and compact. Do not turn it into a patch diary.

Canonical architecture lives in `River_Foam_Stage6_Architecture.md`. Macro stage order lives in `River_Rendering_Roadmap.md`.

## Current working state

The Foam system is in the `4.11C` manually-born persistent material phase.

The 5.4l refactor fixed the hidden spawn-scaling issue: one manual spawn now maps to one budgeted composition event instead of several hidden progressive writers.

The 5.4m realignment removes the old pattern/complexity/density birth controls from the active manual workflow and makes manual birth a single canonical source again. The source-fill field is keyed from source controls rather than event count, so repeated starts with the same settings keep the same broad footprint. This is intentionally plainer: birth should create stable candidate material, while later material evolution owns macro/meso breakup and the renderer owns micro breakup.

Current visual/lifecycle state after 5.5b/5.5c:

- 5.5b made intrinsic macro deformation visible, but introduced an unacceptable lifecycle regression by letting `Presence` depletion shorten material life independently of `Remaining Life`;
- 5.5c repairs lifecycle authority: `Remaining Life`, Neutral Lifetime, and topology aging are the only systems allowed to decide when material dies;
- intrinsic macro deformation remains the current direction before river-disturbance coupling;
- organic breakup is still too weak or blurry;
- topology interaction, lateral drift, disturbance coupling, and obstacle behavior are not yet proven.

## Hard rules for the next patches

Do not rethink the river architecture.

Do not split thin Foam/current lines into a separate system during this milestone.

Do not add automatic anchored births yet.

Do not add new source families as a substitute for fixing manually-born material.

Do not use topology as direct Foam painting.

Do not tune random presets as a replacement for fixing material behavior.

Do not let birth-time randomness decide whether a source becomes a chip, slug, sheet, or empty gap.

Do not proceed to automatic population until manually-born material can move, morph, age, and interact correctly.

Keep the layer split explicit: birth creates stable source material; persistent material simulation owns macro/meso reconfiguration, but must not shorten lifespan outside the Remaining Life equation; the final shader owns micro breakup and crisp presentation.

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

### Accepted layer split

This is persistent material-simulation behavior, not topology and not final shader-only polish. The simulation may reconfigure stored `Presence` over time so a patch can stretch, bend, and crawl. It may not delete material independently of `Remaining Life`; visual thinning/fragmentation must either be driven by the lifecycle equation or remain presentation-only until a lifecycle-safe stored-fragmentation design is approved. The renderer remains responsible for micro fracture and final crispness.

### Acceptance gate

A manually-born patch observed over several seconds changes `Material Presence`, not only Final Foam. It should show clear macro change: altered curvature, different width distribution, and local stretch/compression. It must not grow without bounds, snap, create new topology-painted material, or disappear independently of `Remaining Life`. River disturbance coupling is explicitly deferred to a later patch after intrinsic Foam morphology is proven.

## Blocker 2 — Too blurry / weak organic breakup

### Symptom

Foam reads either as soft blobs or as noisy fragments rather than crisp stylized surface film with readable torn edges.

### Why it matters

The reference has graphic, high-readability Foam. It is stylized but not mushy. From the gameplay camera, the shape must read as broken film/ribbon, not generic cloud noise.

### Likely current cause

Edge breakup and internal masks are not structured enough. Resolution limits, filtering, and noise choice can smear the shape. Birth-time destructive noise also produces random islands instead of controlled tearing.

### Correct behavior

Foam should have crisp enough edges, controlled raggedness, visible dark-water pockets, and readable fragments. Detail should enhance the main shape rather than erase it.

### Next patch target

`4.11C.5.6 — Organic Breakup and Edge Readability`

### Acceptance gate

In Final Foam view, broad pieces keep clean graphic identity while showing torn edges, holes, and fragments. The result should not collapse into blur, dust, or round blobs.

## Blocker 3 — Topology interaction not fully proven

### Symptom

The debug topology shows support and negative regions, but the visible material response is not yet proven clearly enough across typical test cases.

### Why it matters

Topology is the main reason Foam should persist in some places and open dark pockets in others. If this does not work, later automatic population will produce incoherent results.

### Likely current causes

Bad source shapes and weak visual evolution make the aging response hard to judge. Debug colors may exaggerate regions. Some material may cross support/negative zones too briefly for the expected lifespan difference to read.

### Correct behavior

Foam in positive support should survive and remain more coherent. Foam in negative aging pressure should thin, fragment, and die faster. Overlap should resolve according to the approved aging equation.

### Next patch target

`4.11C.5.7 — Topology Aging Proof and Calibration`

### Acceptance gate

Using the Foam + Aging Topology view and Final Foam view, the same material source must show a clear lifetime/shape difference when passing through neutral, supported, negative, and overlapping regions.

## Blocker 4 — No lateral drift / obstacle sliding

### Symptom

Foam movement is mostly downstream. It does not convincingly slide around rocks, banks, logs, bridge supports, or local flow constraints.

### Why it matters

The reference Foam wraps and peels around obstacles. A purely downstream material track will clip or pass unnaturally through features.

### Likely current cause

The accepted lateral/obstacle-tangential motion layer has not been implemented yet. Old guidance/attraction systems were rejected and must not be restored.

### Correct behavior

Foam should remain net-downstream but may drift laterally and slide tangentially around obstacle/bank exclusion. This motion must be explicit, bounded, and performance-safe.

### Next patch target

`4.11C.5.8 — Controlled Lateral Drift and Obstacle Tangential Flow`

### Acceptance gate

A patch approaching an obstacle bends/slides around it instead of only clipping or passing through it. Motion remains stable through bends, reverse flow, and quality settings.

## Blocker 5 — Obstacle interaction is still clipping

### Symptom

Foam can be hard-cut by obstacle exclusion, producing an impermeable clipped look instead of a natural skirt, split, or slide.

### Why it matters

Rocks, logs, and bridge elements are central to the reference. Hard clipping breaks the illusion immediately.

### Likely current cause

Obstacle footprint is currently valid-fluid exclusion, but there is no complete companion behavior that preserves material readability near exclusion boundaries.

### Correct behavior

Obstacle footprint remains solid exclusion, but material near it should be split, redirected, peeled, or dissolved in a controlled way rather than chopped bluntly.

### Next patch target

`4.11C.5.9 — Obstacle Boundary Repair`

### Acceptance gate

Foam contacting obstacle footprint no longer produces dominant hard rectangular/capsule clipping. It either slides, splits, peels, or dies in a way that reads as water interaction.

## Recommended sequence

1. Validate `4.11C.5.5c — Lifecycle Authority Repair` in Final Foam, Material Presence, and Foam + Aging Topology views.
2. `4.11C.5.6 — Organic Breakup and Edge Readability`
3. `4.11C.5.7 — Topology Aging Proof and Calibration`
4. `4.11C.5.8 — Controlled Lateral Drift and Obstacle Tangential Flow`
5. `4.11C.5.9 — Obstacle Boundary Repair`

Only after these pass should the project continue to automatic anchored/open-water birth population.

## Deferred work

Deferred until manual material is accepted:

- anchored automatic birth events;
- open-water birth scheduling;
- spatial fairness/population control;
- mature Foam rendering polish;
- final reference-matching pass;
- production performance/regression closure.

## Maintenance rule

This document should stay short. When a blocker is solved, move its result into a brief status line and continue. Do not paste long patch histories here.
