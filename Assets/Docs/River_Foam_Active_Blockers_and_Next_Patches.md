# River Foam Active Blockers and Next Patches

## Purpose

This is the short working document for current Stage 6 Foam blockers and immediate patch order.

Use this instead of the old problem-register document. Keep it current and compact. Do not turn it into a patch diary.

Canonical architecture lives in `River_Foam_Stage6_Architecture.md`. Macro stage order lives in `River_Rendering_Roadmap.md`.

## Current working state

The Foam system is in the `4.11C` manually-born persistent material phase.

The recent 5.4l refactor fixed the hidden spawn-scaling issue: one manual spawn pattern now maps to one budgeted composition event instead of several hidden progressive writers. That was necessary, but it did not solve visual quality.

Current visual failure:

- repeated manual births can still produce tiny chips, fat slugs, or disconnected blobs with the same settings;
- birth-time pattern/noise logic is still deciding macro shape identity;
- Foam does not yet read like the reference river's broad broken sheets, contour ribbons, connectors, and peeling strips.

## Hard rules for the next patches

Do not rethink the river architecture.

Do not split thin Foam/current lines into a separate system during this milestone.

Do not add automatic anchored births yet.

Do not add new source families as a substitute for fixing manually-born material.

Do not use topology as direct Foam painting.

Do not tune random presets as a replacement for fixing material behavior.

Do not let birth-time randomness decide whether a source becomes a chip, slug, sheet, or empty gap.

Do not proceed to automatic population until manually-born material can move, morph, age, and interact correctly.

## Blocker 0 — Manual source realignment

### Symptom

The current manual birth path can produce wildly different macro results with the same settings: tiny fragments, short tadpoles, fat blobs, or disconnected islands.

### Why it matters

All later validation depends on a trustworthy manual source. If the source is already malformed, temporal morphing, topology aging, and obstacle behavior cannot be judged cleanly.

### Likely current cause

Compute-side birth injection still starts from a swept capsule/segment and applies destructive pattern/noise masks. Pattern logic is trying to create final Foam art at birth instead of creating stable source material for later persistent behavior.

### Correct behavior

Manual birth should create a bounded, stable candidate source with predictable macro footprint. Randomness may vary edge damage, local holes, and small gaps, but it must not change the broad source identity.

`Amount` remains source-only spatial fill. It must not act as life, opacity, or random macro deletion.

### Next patch target

`4.11C.5.4m — Manual Source Realignment`

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

### Next patch target

`4.11C.5.5 — Temporal Morphing and Material Shape Evolution`

### Acceptance gate

A manually-born patch observed over several seconds changes silhouette and interior structure while remaining recognizably the same material body. It must not inflate, snap, or disappear independently of Remaining Life.

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

1. `4.11C.5.4m — Manual Source Realignment`
2. `4.11C.5.5 — Temporal Morphing and Material Shape Evolution`
3. `4.11C.5.6 — Organic Breakup and Edge Readability`
4. `4.11C.5.7 — Topology Aging Proof and Calibration`
5. `4.11C.5.8 — Controlled Lateral Drift and Obstacle Tangential Flow`
6. `4.11C.5.9 — Obstacle Boundary Repair`

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
