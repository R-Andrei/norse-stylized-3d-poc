# River Foam Active Blockers and Next Patches

## Purpose

This is the short working document for current Stage 6 Foam blockers and immediate patch order.

Canonical architecture lives in `River_Foam_Stage6_Architecture.md`. Macro stage order lives in `River_Rendering_Roadmap.md`. This document owns the active recovery queue.

This document must not preserve stale active plans. Historical patch notes may appear only as clearly superseded context.

## Current working state after 4.11C.5.9t audit

The Foam system is currently a stable, reduced manually-born persistent material baseline with a partially stale debug/UI layer.

Active and trusted:

- manual source birth creates persistent Foam material;
- Persistent Foam State stores `Presence`, `Remaining Life`, and `Material Pattern`;
- downstream phase transport moves material downstream;
- lifecycle aging and valid-fluid clipping still work;
- topology/support/negative aging still influence survival;
- static pressure/lee support still feeds topology/lifecycle where implemented;
- Foam Motion Field generation and shader debug colouring still exist;
- final rendering still draws Foam, but final presentation must not be treated as macro movement authority.

Removed, disabled, or superseded in the compute/simulation path:

- persistent neighbour-sampling morph was removed by 5.9n;
- fractional lateral row weighting is rejected because it smeared and pulsed material;
- per-cell stochastic/source-owned lateral row commit is rejected because it shredded material into ribbons;
- lateral row commit is disabled after 5.9p;
- Motion Field currently does not move persistent material;
- disturbance fields no longer drive stored-state morphing/breakup because that path was removed.

Audit findings that must be fixed before Stage 2 work:

- `Foam Motion Field` debug is supposed to overlay raw stored `Presence`, but the uploaded shader still overlays final `foam.mask` through the final foam evaluation path. This contaminates the transport diagnostic and is the first blocker.
- `Foam Motion Field + Cell Grid` is documented as present, but the uploaded code audit did not find the enum/editor/shader branch needed for that view. Either implement it or roll the docs back. The preferred direction is to implement it because cell-scale diagnosis remains useful.
- `Surface Morph Strength` is still present in serialized/editor-facing C# even though persistent stored-state morphing was removed. This is stale and misleading.
- Several Motion Field labels/tooltips still imply active lateral material movement. Current truth: Motion Field is an intent/debug/future input field only until real Stage 1 lateral transport is rebuilt.
- final rendering still performs some visible foam warp/stretch/mask shaping. This may remain temporarily, but it is Stage 3 debt and must not be treated as the long-term source of macro foam morphology.

Current missing feature families:

- safe Stage 2 Shape Evaluation product;
- baseline intrinsic morphology/living foam;
- disturbance-driven shape breakup/morph speed;
- coherent ribbon deformation;
- evaluated split/join behavior;
- real Stage 1 lateral material transport;
- final render cleanup against the evaluated shape product.

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

### Blocker 1 — Foam Motion Field debug must use raw stored Presence

#### Goal

Make `Foam Motion Field` debug show the Motion Field background plus raw Persistent Foam State `Presence`, not final `foam.mask`.

#### Why it matters

This is the highest-priority audit contradiction. The purpose of the Motion Field debug view is to compare stored material location against lateral/obstacle intent. If the overlay comes from final render mask, the view can include presentation-only warp, stretch, erosion, filtering, or other Stage 3 behavior. That makes it impossible to diagnose whether Stage 1 material actually moved.

#### Concrete first-step code target

Patch the Foam Motion Field debug branch in:

```text
Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader
```

The current branch uses final evaluated/render foam data roughly like:

```hlsl
float foamOverlay = saturate(smoothstep(0.08, 0.46, foam.mask) * 0.58);
```

The corrected branch should use raw stored presence from the foam sample/evaluation data available in that shader path, conceptually:

```hlsl
float foamOverlay = saturate(smoothstep(rawPresenceLow, rawPresenceHigh, foam.presence) * overlayStrength);
```

Exact variable names and thresholds must be chosen after inspecting the current shader structures. Do not guess them without reading the actual file.

#### Scope limits

This patch should not change foam simulation, transport, birth, topology, disturbance fields, final normal rendering, or Stage 2 behavior.

It is a debug-truth fix only.

#### Acceptance gate

- `Foam Motion Field` debug still shows the field background.
- The white/bright overlay follows raw stored material presence.
- Toggling render-side foam warp/stretch/mask polish should not move the Motion Field overlay.
- `Material Presence` and the overlay footprint in `Foam Motion Field` should agree on stored material location, allowing for different colouring/thresholding.
- Final Foam may still look different because it is final presentation.

### Blocker 2 — Foam Motion Field + Cell Grid implementation/contract alignment

#### Goal

Resolve the mismatch between docs and uploaded code around `Foam Motion Field + Cell Grid`.

#### Preferred direction

Implement the debug view because cell scale remains important for diagnosing row-level transport, cell-scale shredding, patch coherence, and future lateral movement.

#### Concrete targets

Likely files:

```text
Game/Procedural/Rivers/StylizedRiver.cs
Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs
Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader
```

Expected behavior:

- add a distinct debug enum value;
- add Inspector label/description;
- render Motion Field background;
- overlay raw stored `Presence`;
- draw faint persistent foam simulation cell boundaries;
- optionally draw brighter 8-cell blocks for scale reading.

#### Scope limits

No simulation behavior change.

### Blocker 3 — Remove or quarantine stale Surface Morph Strength UI

#### Goal

Remove misleading public control surface for the removed persistent stored-state morph path.

#### Why it matters

`Surface Morph Strength` currently suggests that stored foam morphology is active or tunable. That is false after the architecture reset. Keeping it visible invites wrong testing and wrong tuning.

#### Concrete targets

Likely files:

```text
Game/Procedural/Rivers/StylizedRiver.cs
Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs
```

#### Preferred behavior

- remove it from active Inspector drawing;
- remove active binding/public property if unused;
- preserve serialized migration only if needed to avoid breaking existing scenes;
- do not replace it with a new Stage 2 control until Stage 2 exists.

### Blocker 4 — Reword Motion Field UI as intent/debug/future input

#### Goal

Make Inspector labels, tooltips, and debug descriptions match current truth.

#### Correct wording

Motion Field currently means:

```text
intent/debug field for lateral routing and future material transport / shape deformation
```

It does not currently mean:

```text
active lateral movement of persistent foam material
```

#### Scope limits

Text/label/comment cleanup only unless the same patch is explicitly approved to change behavior.

### Blocker 5 — Final render morphology/stage ownership review

#### Goal

Classify final-shader warp/stretch/mask shaping as temporary Stage 3 debt.

#### Correct near-term behavior

Do not rip it out before Stage 2 exists, because that could make foam visually worse without providing the correct replacement. But do not build new macro morphology in final rendering.

#### Future migration

After `_FoamShapeMask` or equivalent exists, move macro shape behavior into Stage 2 and reduce Stage 3 to colour, opacity, lighting, blend, and small polish.

### Blocker 6 — Shape Evaluation foundation

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

### Blocker 7 — Intrinsic morphology

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

### Blocker 8 — Disturbance-driven morphology

#### Goal

Reconnect pressure, lee, wake, ripple, and wave inputs as Stage 2 modifiers.

#### Correct behavior

Foam should morph faster, break more, fray more, or deform more in active water while still allowing support/lifetime effects to remain Stage 1 lifecycle behavior.

Disturbance does not directly move, destroy, or spawn foam.

### Blocker 9 — Coherent deformation

#### Goal

Add smooth, bounded inverse deformation inside Stage 2.

#### Correct behavior

Ribbons should bend, compress, narrow, and stretch visually as coherent shapes. Deformation must be smooth over multiple foam cells, bounded, gradient-limited, and non-persistent.

### Blocker 10 — Evaluated split/join behavior

#### Goal

Add local evaluated breakup and reconnection.

#### Correct behavior

Ribbons may visually split, chip, fracture, and softly reconnect through Stage 2. This must not spawn persistent material or shred stored material.

### Blocker 11 — Real lateral transport redesign

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
- 5.9m transport diagnostic isolation: intended to make Motion Field debug use raw presence, but the 5.9t audit found the uploaded shader still used final `foam.mask`; this remains unresolved until Blocker 1 is patched.
- 5.9n persistent morph cleanup: accepted compute/simulation cleanup result, but the 5.9t audit found stale `Surface Morph Strength` UI/property remnants.
- 5.9p lateral commit disable: accepted stabilization result.
- 5.9q dead-weight cleanup: accepted cleanup result.
- 5.9r Foam Cell Grid debug view: intended and still desired, but the 5.9t audit did not find the required uploaded code path; resolve through Blocker 2.
- 5.9s architecture contract docs: accepted contract reset.
- 5.9t compliance audit/docs update: current source of active blocker order.

## Maintenance rule

Keep this document short and current. Do not turn it into a patch diary. Completed implementation detail belongs in the implementation log; canonical rules belong in `River_Foam_Stage6_Architecture.md`.
