# River Foam Active Blockers and Next Patches

## Purpose

This is the short working document for current Stage 6 Foam blockers and immediate patch order.

Canonical architecture lives in `River_Foam_Stage6_Architecture.md`. Macro stage order lives in `River_Rendering_Roadmap.md`.

This document must not preserve stale active plans. Historical patch notes may appear only as clearly superseded context.

## Current working state after 4.11C.5.9r

The Foam system is currently a stable, reduced manually-born persistent material baseline.

Active and trusted:

- manual source birth creates persistent Foam material;
- Persistent Foam State stores `Presence`, `Remaining Life`, and `Material Pattern`;
- downstream phase transport moves material downstream;
- lifecycle aging and valid-fluid clipping still work;
- topology/support/negative aging still influence survival;
- static pressure/lee support still feeds topology/lifecycle where implemented;
- Foam Motion Field generation/debug still exists;
- Foam Motion Field + Cell Grid debug exists and shows the actual persistent foam simulation grid;
- final rendering still draws Foam, but final presentation must not be treated as macro movement authority.

Removed, disabled, or superseded:

- persistent neighbour-sampling morph was removed by 5.9n;
- `Surface Morph Strength` was removed because stored-state morphing is no longer active;
- fractional lateral row weighting is rejected because it smeared and pulsed material;
- per-cell stochastic/source-owned lateral row commit is rejected because it shredded material into ribbons;
- lateral row commit is disabled after 5.9p;
- Motion Field currently does not move persistent material;
- disturbance fields no longer drive stored-state morphing/breakup because that path was removed.

Current missing feature families:

- safe Stage 2 Shape Evaluation product;
- baseline intrinsic morphology/living foam;
- disturbance-driven shape breakup/morph speed;
- coherent ribbon deformation;
- evaluated split/join behavior;
- real Stage 1 lateral material transport;
- final render cleanup against the new product/stage contract.

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

## Immediate next work

### Blocker 1 — Architecture/code compliance audit

#### Goal

Check current code against `River_Foam_Stage6_Architecture.md` before rebuilding features.

#### Why it matters

The system recently failed because multiple paths acted like movement or morph authority at the same time. Before adding Stage 2, the codebase must be checked for stale active paths, misleading controls, debug contradictions, and final-render macro behavior that violates the new contract.

#### Audit targets

- Stage 1: verify only birth/transport/lifecycle/clipping write Persistent Foam State.
- Stage 2: confirm no safe Evaluated Foam Shape product exists yet.
- Stage 3: identify any final shader macro stretch/warp that should move into Stage 2 or be reduced later.
- Input fields: confirm Motion Field and Disturbance Fields are data providers only.
- Debug: verify views clearly distinguish raw persistent state, motion field, grid, future evaluated shape, and final render.
- UI/docs: verify controls and labels do not claim inactive behavior is active.

#### Acceptance gate

Produce a short compliance report identifying:

- contract-compliant code;
- contract-risky code;
- stale/dead code;
- missing products/stages;
- safe next implementation target.

No feature behavior should be changed during this audit unless a compile/dead-weight cleanup is explicitly approved.

### Blocker 2 — Shape Evaluation foundation

#### Goal

Create the Stage 2 evaluated-shape product with minimal behavior first.

#### Correct behavior

A future patch should introduce an evaluated shape output, likely a compact `_FoamShapeMask` or equivalent. First pass should prove:

- Persistent Foam State remains stable;
- Evaluated Foam Shape is derived from Persistent Foam State;
- debug can show raw Persistent Foam State and Evaluated Foam Shape separately;
- final render can consume Evaluated Foam Shape rather than independently inventing macro shape behavior.

#### Explicit exclusions

No aggressive deformation, breakup, split/join, or lateral transport in the foundation patch.

### Blocker 3 — Intrinsic morphology

#### Goal

Restore baseline living foam everywhere without requiring disturbance.

#### Correct behavior

Stage 2 should add:

- slow breathing;
- edge fray;
- small holes;
- chipping;
- subtle ribbon/body wobble;
- pattern-stable variation.

This must affect Evaluated Foam Shape only.

### Blocker 4 — Disturbance-driven morphology

#### Goal

Reconnect pressure, lee, wake, ripple, and wave inputs as Stage 2 modifiers.

#### Correct behavior

Foam should morph faster, break more, fray more, or deform more in active water while still allowing support/lifetime effects to remain Stage 1 lifecycle behavior.

Disturbance does not directly move, destroy, or spawn foam.

### Blocker 5 — Coherent deformation

#### Goal

Add smooth, bounded inverse deformation inside Stage 2.

#### Correct behavior

Ribbons should bend, compress, narrow, and stretch visually as coherent shapes. Deformation must be smooth over multiple foam cells, bounded, gradient-limited, and non-persistent.

### Blocker 6 — Evaluated split/join behavior

#### Goal

Add local evaluated breakup and reconnection.

#### Correct behavior

Ribbons may visually split, chip, fracture, and softly reconnect through Stage 2. This must not spawn persistent material or shred stored material.

### Blocker 7 — Real lateral transport redesign

#### Goal

Rebuild Stage 1 lateral material movement after the evaluated shape layer is stable.

#### Correct behavior

Lateral movement must move durable material coherently through the Motion Field. It must not use fractional row weighting or per-cell random stay/move decisions.

Likely future direction: accumulated lateral phase/residual or an equivalent patch-coherent transport method.

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
- 5.9m/5.9n persistent morph isolation/cleanup: accepted cleanup result.
- 5.9p lateral commit disable: accepted stabilization result.
- 5.9q dead-weight cleanup: accepted cleanup result.
- 5.9r Foam Cell Grid debug view: accepted debug foundation.

## Maintenance rule

Keep this document short and current. Do not turn it into a patch diary. Completed implementation detail belongs in the implementation log; canonical rules belong in `River_Foam_Stage6_Architecture.md`.
