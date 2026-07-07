# River Foam Active Blockers and Next Patches

## Purpose

This is the short working document for current Stage 6 Foam blockers and immediate patch order.

Canonical architecture lives in `River_Foam_Stage6_Architecture.md`. Macro stage order lives in `River_Rendering_Roadmap.md`. This document owns the active recovery queue.

This document must not preserve stale active plans. Historical patch notes may appear only as clearly superseded context.

---

# Current working state after architecture lock

Stage 6 now has a corrected canonical dependency graph:

```text
Layer A — River Domain
Layer B — External Influence Fields
Layer C — Persistent Foam Material
Layer D — Visual Foam / Film Evaluation
Layer E — Shader Composition
Layer F — Scheduling, Quality, Debug
```

The old `Stage 1.5` language is retired because it blurred two different things:

```text
External foam-agnostic support/motion/contact fields = Layer B.
Foam-derived visual sheet-support helper fields = Layer D internals.
```

The non-circular rule is now explicit:

```text
Layer B may feed Layer C and Layer D.
Layer B must not read Layer C or Layer D.
Layer D may read Layer C, but must never write Layer C.
Layer E must never feed back into compute/simulation.
```

## Active and trusted foundations

Trusted foundations:

```text
Persistent Foam State stores Presence, Remaining Life, and Material Pattern.
Manual/source birth creates durable material.
Downstream phase transport moves durable material downstream.
Lifecycle aging and valid-fluid clipping remain Layer C-owned.
Topology/support/negative aging influences Layer C where implemented.
Motion Field, obstacle routing, topology, pressure/wake/ripple fields are Layer B-style inputs, not foam movers by themselves.
_FoamShapeMask exists as the Layer D product slot.
Foam Evaluated Shape debug can display _FoamShapeMask.
Foam Shape Difference debug compares _FoamShapeMask against raw persistent Presence.
Final Foam still does not consume _FoamShapeMask.
```

## Superseded/rejected work

Rejected or superseded as active direction:

```text
persistent stored-state morph as visual breakup;
5.8 chaotic drift as hidden stored-state morphology;
fractional lateral row weighting;
per-cell stochastic/source-owned row commit;
5.9y dense interior hole morphology;
5.9y.1 tiny local edge-fray as the main morphology direction;
5.9z coordinate warp as the final visual shape solution;
naive full-res radius 1/3/5 wide-neighbour classifier as default;
pocket IDs / connected components / foam entity database;
shader-side wide-neighbour structural foam search.
```

## 5.9z validation conclusion

5.9z added a Stage D coherent coordinate-warp prototype:

```text
basePresence = persistent presence at current cell
deformedPresence = persistent presence sampled at currentCell - small deformation
_FoamShapeMask = lerp(basePresence, deformedPresence, blend)
```

It correctly preserved material truth and proved the dispatch/binding/product slot, but validation showed almost no visible difference between `Material Presence` and `Foam Evaluated Shape`.

Reason:

```text
one-to-two-cell deformation affects mostly the contour;
solid foam interiors sample as solid foam after displacement;
blend-to-base damped the result;
coordinate warp cannot create visual bridge/pinch/sheet support;
debug lacked a Shape Difference view.
```

Do not spend future patches merely tuning 5.9z stronger. 5.10 added the missing difference diagnostic and cleanup; 5.10 validation proved the warp was active but visually useless; 5.10B retires the warp and resets Layer D to pass-through. The next visual work must implement a structurally different Layer D component or a deliberately isolated local procedural breakup probe.

---

# 4.11C.5.10 code-audit findings

Status: implemented as the first cleanup patch after the canonical architecture lock.

Findings recorded by the source audit:

```text
Layer A/B/C/D/E/F ownership is broadly compatible with the canonical graph.
No current circular dependency was found: Layer B does not appear to read FoamState or _FoamShapeMask to build external influence fields; Layer C does not read _FoamShapeMask; Layer D writes only _FoamShapeMask; Layer E renders pixels only.
The current Layer D implementation was still the failed 5.9z coordinate-warp prototype during the 5.10 audit. 5.10B retires it and resets Layer D to pass-through clipped Persistent Presence.
Final Foam still uses legacy shader-side macro shaping and does not consume _FoamShapeMask.
Editor labels/comments overstated disturbance-driven material transport and pass-through evaluated shape behavior.
Unused wake/pressure transport constants remained from earlier material-motion experiments.
Layer D evaluation was being dispatched every frame even though Final Foam does not consume it.
Transition-hold binding may still show raw persistent state as the shape mask fallback; this is known and non-urgent.
```

Cleanup performed in 5.10:

```text
Added Foam Shape Difference debug view.
Updated Foam Evaluated Shape and Shape Difference descriptions.
Updated Water Body help text to use Layer A-F ownership language and stop claiming active lateral disturbance material transport.
Removed unused disturbance-material-motion constants.
Gated DispatchEvaluateShape so the current Layer D product runs only when a Layer D debug product is requested.
Documented that current Layer D remains a superseded visual prototype until the real film/source/support work begins.
```

The Shape Difference view compares:

```text
raw Persistent Presence
vs
_FoamShapeMask
```

It must not compare final `foam.mask` against `_FoamShapeMask` because final shader masks include presentation logic.

---

# 4.11C.5.10B validation and reset

Validation after 5.10 showed:

```text
Foam Shape Difference displayed strong green/magenta signed differences.
Material Presence and Foam Evaluated Shape still looked basically identical.
Final Foam stayed unchanged.
```

Conclusion:

```text
The 5.9z coordinate warp was numerically active.
It did not produce useful broad visual shape behavior.
It should not remain as the active Layer D baseline because it pollutes future comparisons.
```

5.10B cleanup:

```text
EvaluateFoamShape is reset to pass-through clipped Persistent Presence.
The unused coordinate-warp helper functions are removed.
DispatchEvaluateShape no longer binds Motion Field / obstacle-routing inputs because the baseline shape pass does not read them.
Foam Shape Difference remains available and should now be mostly black until a new Layer D component is added.
```

4.11C.5.11 then implemented an isolated local procedural breakup probe on top of the clean baseline. Validation showed that the probe was active, but it was the wrong layer for fine fragmentation.

5.11 validation facts:

```text
Foam Shape Difference became clearly non-black, mostly magenta/removal.
The removals appeared as long cell/ribbon-shaped holes, not granular foam breakup.
The effect exposed the foam-field cell structure instead of hiding it.
Material Presence and Final Foam remained separate as intended.
```

Conclusion:

```text
Layer D local-only procedural removal is rejected as the fine fragmentation solution.
The problem was not that the probe was inactive; the problem was that _FoamShapeMask cell resolution is the wrong scale for atomic/chipped edge detail.
Fine fragmentation, tiny cuts, and thin streaks belong in Layer E shader composition where the unit is a rendered pixel, not a foam simulation cell.
Layer D should remain responsible for macro film structure only: broad sheets, contact support, bridge/pinch/split, and smooth shape foundation.
```

5.11B cleanup:

```text
EvaluateFoamShape is reset again to pass-through clipped Persistent Presence.
The local-breakup helper functions are removed from CS_RiverFoam.compute.
DispatchEvaluateShape no longer binds _FoamTime, _FoamSeed, _FoamGlobalStart, _FoamFieldLength, or _FoamMetricRows for the baseline shape pass.
Foam Shape Difference should again be fully black or nearly black until a new Layer D structural component is intentionally added.
```

---

# Active blockers

## Blocker 1 — Fine breakup belongs in Layer E, not Layer D cells

Current problem:

```text
4.11C.5.11 proved that local no-neighbour breakup inside _FoamShapeMask creates visible difference, but the difference is cell/ribbon-shaped because Layer D writes a foam-field texture, not final pixels.
```

Required future system:

```text
A shader-side Layer E local-detail probe that samples the clean Layer D mask and applies sub-cell/per-pixel procedural edge breakup, granular cuts, thin scratches, and highlight streaks without writing FoamState or _FoamShapeMask.
```

Strict limit:

```text
Layer E may create local visual detail and thin streaks.
Layer E must not own broad structural connectivity and must never feed back into compute.
```

## Blocker 2 — Broad sheet/support behavior requires Layer D helpers

Current problem:

```text
Local-only math cannot reliably know whether an empty cell is between two nearby foam fields or alone in open water.
```

Required future system:

```text
Low-res Layer D visual film source/support fields.
```

This should target:

```text
broad pale sheets
small-gap visual bridging
bank/rock/contact skirts
flow-aware sheet elongation
weak pinch zones
```

This is a fixed-grid mathematical field solution, not an entity system.

## Blocker 3 — Final Foam still uses legacy shader macro shaping

Current problem:

```text
Layer E still owns the player-facing broad Foam shape through legacy shader-side mask logic. This is acceptable temporarily because Layer D is not ready, but it is not the final architecture.
```

Required future system:

```text
Switch Final Foam to _FoamShapeMask only after Layer D visibly outperforms the current final render. Then demote shader-side macro shape logic to local polish/thin streaks only.
```

## Blocker 4 — Transition-hold ShapeMask fallback is product-imprecise

Current problem:

```text
During topology transition hold, the runtime may still bind persistent state where _FoamShapeMask is expected. This is not known to break normal play, but evaluated-shape debug during transition should be treated cautiously.
```

Required future fix:

```text
Either preserve a transition snapshot ShapeMask or bind a clear fallback and document that Layer D debug is unavailable during transition hold.
```

---

# Immediate patch order

## Patch A — Documentation lock

Status: complete in docs update.

Purpose:

```text
Replace stale Stage 1.5 / coherent-warp / three-stage oversimplification with the corrected Layer A-F acyclic architecture.
```

## Patch B — Compliance and debug truth audit

Status: complete in 4.11C.5.10.

Implemented behavior:

```text
new debug enum: FoamShapeDifference
shader debug branch compares _FoamShapeMask to raw persistent Presence
editor label/description added
stale material-motion descriptions corrected
unused disturbance transport constants removed
Layer D shape evaluation dispatch gated to Layer D debug use
no final rendering change
no low-res helper textures yet
```

## Patch C — Layer E shader-side local detail probe

Scope:

```text
Retest the cheap local-only visual-breakup idea at the correct resolution: shader pixels, not _FoamShapeMask cells.
Sample the clean Layer D mask and apply local procedural edge breakup, granular cuts, thin scratches, and highlight/streak detail in a debug/final-shader path.
No new entity system.
No wide neighbourhood sampling.
No persistent material mutation.
No _FoamShapeMask mutation.
```

Acceptance:

```text
Fine breakup should read as sub-cell/per-pixel foam detail rather than simulation-grid holes.
It may improve edge chipping, fray, thin cuts, and streaks.
It is not expected to solve broad bridge/sheet/contact structure.
If it still reads as noise or dirt, reject it quickly and do not keep tuning it endlessly.
```

## Patch D — Low-res Layer D Film Source / Film Support prototype

Scope:

```text
Add _FoamFilmSourceHalf and _FoamFilmSupportHalf or equivalent.
Downsample material + external support.
Apply cheap directional spread/bridge support.
Expose debug views for Film Source and Film Support.
Do not feed these fields back into Layer B or Layer C.
```

Acceptance:

```text
Foam Film Support visibly shows broad sheet/contact/bridge support that is not merely a coordinate-warped copy of Presence.
```

## Patch E — Full Layer D _FoamShapeMask integration

Scope:

```text
Combine Persistent Presence + Film Support + local breakup + exclusion into _FoamShapeMask.
```

Acceptance:

```text
Foam Evaluated Shape visibly differs from Material Presence in broad structural ways.
No durable material corruption.
No circular dependencies.
```

## Patch F — Final Foam consumes _FoamShapeMask

Only after Patch E is accepted.

Scope:

```text
Shader broad foam structure samples _FoamShapeMask.
Legacy shader-side macro shape logic is demoted/removed.
Shader keeps local polish/thin streaks.
```

---

# Current stop rules

Stop and reassess if:

```text
a proposed Layer B field reads FoamState;
a proposed Layer C change reads ShapeMask;
a proposed Layer D helper feeds Layer C;
a shader effect requires wide neighbourhood sampling over screen pixels;
a debug view uses final foam.mask while claiming to show raw material;
a new feature creates a second authority over material movement;
a new patch cannot state which layer owns each written texture.
```

