# River Foam Active Blockers and Next Patches

## Purpose

This is the short working document for current Stage 6 Foam blockers and immediate patch order.

Canonical architecture lives in `River_Foam_Stage6_Architecture.md`. Macro stage order lives in `River_Rendering_Roadmap.md`. This document owns the active recovery queue.

This document must not preserve stale active plans. Historical patch notes may appear only as clearly superseded context.

## Current working state after 4.11C.5.9y.2

The Foam system now has the correct two-product slot, but Stage 2 is deliberately reset to a truthful pass-through baseline while the next field-based morphology approach is designed.

Active and trusted:

- manual source birth creates persistent Foam material;
- Persistent Foam State stores `Presence`, `Remaining Life`, and `Material Pattern`;
- downstream phase transport moves durable material downstream;
- lifecycle aging and valid-fluid clipping still work;
- topology/support/negative aging still influence actual Remaining Life;
- static pressure/lee support still feeds topology/lifecycle where implemented;
- `Foam Motion Field` debug overlays raw stored `Presence`;
- `Foam Motion Field + Cell Grid` exists as a diagnostic view;
- `Foam Evaluated Shape` exists and reads `_FoamShapeMask`;
- Stage 2 currently writes `_FoamShapeMask` as clipped pass-through persistent `Presence`;
- `_FoamTime` is refreshed immediately before Stage 2 evaluation so later animated shape work does not inherit the lower material-step cadence.

Completed alignment work since the 5.9t audit:

- Motion Field debug was corrected to use raw stored `Presence`;
- Motion Field + Cell Grid was implemented;
- stale `Surface Morph Strength` UI/control remnants were quarantined;
- Motion Field Inspector wording now describes intent/debug/future input rather than active lateral material motion;
- `_FoamShapeMask` and `Foam Evaluated Shape` debug were added;
- rejected 5.9y/5.9y.1 morphology experiments are superseded.

Rejected or superseded Stage 2 experiments:

- dense interior hole cutting: rejected because it produced marbled/scratched interiors not present in the reference river;
- whole-body/life thinning as a default look: rejected as a baseline because it made broad ribbons collapse visually before Stage 1 lifecycle actually removed them;
- tiny local edge-fray retune: rejected because it spent compute for practically no visible effect;
- naive multi-radius edge classification: rejected as a default because radius 1/3/5 box sampling costs roughly `179` presence samples per cell, or about `2.93M` samples per 128×128 field per evaluation.

Current missing feature families:

- field-based coherent deformation;
- cheap mathematical bridge/break/merge-like visual shaping;
- disturbance-driven shape breakup/morph speed;
- final render consumption of `_FoamShapeMask`;
- real Stage 1 lateral material transport;
- mature final Foam rendering polish.

Important architecture clarification:

The current foam architecture is field-based, not pocket/entity based. Stage 2 does not track Foam pocket IDs, connected components, child fragments, or per-pocket properties. It evaluates scalar/vector fields at foam-grid resolution. Merge/split behavior should first be pursued through formulas and field operations, not tracked Foam entities.

## Current canonical architecture summary

Foam has two foam data products:

1. `Persistent Foam State` — durable material: Presence, Remaining Life, Material Pattern.
2. `Evaluated Foam Shape` — current visible shape derived from Persistent Foam State.

Foam has three processing stages:

1. `Stage 1 — Persistent State Update` writes Persistent Foam State. It owns birth, transport, lifecycle, and valid-fluid clipping. Only this stage moves stored foam material.
2. `Stage 2 — Shape Evaluation` writes Evaluated Foam Shape. It owns coherent deformation, morphology, breakup, split/join appearance, and disturbance-reactive shape animation. It does not write back to Persistent Foam State.
3. `Stage 3 — Rendering` draws Evaluated Foam Shape. It owns colour, lighting, opacity, blending, and small final polish. It does not create macro movement or macro shape behavior.

Input fields such as Motion Field, Disturbance Fields, and Topology/Support Fields provide data. They do not directly mutate Foam by themselves.

## Hard rules for next patches

Do not reintroduce neighbour-sampled persistent morphing.

Do not reintroduce fractional lateral row weighting.

Do not reintroduce per-cell stochastic lateral row commit.

Do not make the final shader the source of macro bending, macro stretching, fake downstream shedding, fake lateral drift, split/join, or obstacle routing.

Do not let any input field directly mutate foam.

Do not use topology as direct Foam painting or steering.

Do not add automatic anchored/open-water births as a substitute for fixing manually-born material behavior.

Do not restore old chaotic drift as a hidden stored-state morph path.

Do not proceed to real lateral transport until the stable baseline, Stage 2 product, and debug views are compliant with the architecture contract.

## Immediate next work after 5.9y.2

### Work item 1 — Field-based coherent deformation prototype

#### Goal

Make `Foam Evaluated Shape` visibly differ from `Material Presence` by coherently bending/offsetting the evaluated mask, without moving Persistent Foam State.

#### Correct model

Use field math, not pocket IDs:

```hlsl
float2 deformationCells = ResolveSmoothFoamDeformation(coordinate, time, materialPattern);
float shape = SamplePersistentPresenceBilinear(coordinate - deformationCells);
```

The deformation field must be smooth over multiple foam cells, bounded, and cheap. It should make whole ribbons/sheets bend together because neighbouring cells receive similar offsets. It must not use per-cell random row shifts, neighbour-written feedback, or entity tracking.

#### Cost target

The intended prototype should be close to:

```text
1 low-frequency vector/noise lookup
+ 1 bilinear persistent-presence sample
≈ 4–5 texture/state samples per foam cell
≈ 80k samples per 128×128 field evaluation
```

This is much cheaper than naive radius-1/3/5 classification:

```text
9 + 49 + 121 = 179 samples per cell
≈ 2.93M samples per 128×128 field evaluation
```

#### Scope limits

- Do not switch final rendering to `_FoamShapeMask` yet.
- Do not add pocket IDs or connected-component tracking.
- Do not add naive multi-radius sampling.
- Do not mutate Persistent Foam State or Remaining Life.

### Work item 2 — Cheap visual bridge/break field

#### Goal

After coherent deformation is validated, add formula-driven visual merge/split approximation to Stage 2.

Preferred directions:

- low-resolution blurred presence/life field;
- mip-filtered presence/life field if texture setup supports it cleanly;
- approximate morphological closing/opening only if implemented through cheap helper fields, not direct wide-kernel sampling.

#### Correct behavior

Visual bridge/break may use Remaining Life as read-only metadata:

- newer/stronger foam can visually bridge/hold together more;
- older/weaker foam can visually tear more;
- Stage 2 must not change actual Remaining Life.

### Work item 3 — Final Foam consumes `_FoamShapeMask`

Only after `Foam Evaluated Shape` is visually useful and directionally aligned, switch Final Foam from the old render-side macro mask to `_FoamShapeMask`. Stage 3 should then keep colour, opacity, lighting, blend, and small polish only.

### Work item 4 — Disturbance-driven Stage 2 response

Reconnect pressure, lee, wake, ripple, and wave inputs as shape modifiers after the base field deformation/bridge-break model is accepted. Disturbance should increase deformation, breakup, edge activity, or morph speed, not directly move/destroy/spawn durable material.

## Deferred work

Deferred until manual material, evaluated shape, and transport are accepted:

- automatic anchored birth events;
- open-water birth scheduling;
- spatial fairness/population control;
- mature final Foam rendering polish;
- production performance/regression closure.

## Historical notes retained for context only

The following patch families are historical and must not be treated as active architecture:

- 5.5-5.7 stored-state morphing: useful visual proof, rejected implementation authority.
- 5.8 local chaotic drift: proof that macro lateral motion is needed, rejected as hidden morph/movement authority.
- 5.9j/5.9k/5.9l/5.9o lateral commit attempts: rejected because row-weight and per-cell commit variants smeared or shredded material.
- 5.9m transport diagnostic isolation: intended to make Motion Field debug use raw presence; the later 5.9u code patch completed the raw-presence debug correction.
- 5.9n persistent morph cleanup: accepted compute/simulation cleanup result; later 5.9w quarantined the stale `Surface Morph Strength` UI/property remnants.
- 5.9p lateral commit disable: accepted stabilization result.
- 5.9q dead-weight cleanup: accepted cleanup result.
- 5.9r Foam Cell Grid debug view: intended diagnostic; later 5.9v implemented the missing code path.
- 5.9s architecture contract docs: accepted contract reset.
- 5.9t compliance audit/docs update: superseded by the 5.9y.2 active order in this document.

## Maintenance rule

Keep this document short and current. Do not turn it into a patch diary. Completed implementation detail belongs in the implementation log; canonical rules belong in `River_Foam_Stage6_Architecture.md`.
