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
Low-res Layer D visual film source/support fields. This remains the next structural work after the 5.12 Layer E local-detail probe is validated or rejected.
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

This section is the active source of truth for the next foam work. Work items must come from this sequence:

```text
Canonical Layer A-F architecture
    -> current implemented code state
    -> Unity validation result
    -> documented next-patch plan
    -> implementation patch
```

Do not implement a new visual/compute behavior patch directly from conversation speculation. If validation reveals a new issue, document the issue here first, then implement against the documented target.

## Current validated state after 4.11C.5.13C

Validated by Unity screenshots after applying `4.11C.5.13C`:

```text
Coordinate-space stutter: fixed by 5.13B.
Support-topology contamination: fixed by 5.13C.
Final Foam: unchanged and still legacy shader-side final foam.
Foam Film Source: now follows material-derived foam instead of generic support topology.
Foam Film Support: now spreads material-derived source instead of raw topology support.
Foam Shape Difference: now reports material-derived Layer D visual spread/removal.
Foam And Aging Topology: remains the explicit support/topology debug view.
```

Important interpretation:

```text
The latest green in Foam Shape Difference is no longer support topology masquerading as foam.
It now means Layer D added visual coverage over material-derived source.
This does not mean durable material was created.
```

Current screenshots showed the corrected result is semantically clean but visually primitive: Film Support behaves like a broad low-resolution dilation around the material ribbon. That is now the real next problem.

## Patch A — Documentation lock

Status: complete.

Purpose:

```text
Replace stale Stage 1.5 / coherent-warp / three-stage ambiguity with the corrected Layer A-F acyclic architecture.
```

## Patch B — Compliance and debug truth audit

Status: complete in `4.11C.5.10`.

Implemented behavior:

```text
Added Foam Shape Difference.
Corrected stale debug/editor text.
Removed unused material-motion constants.
Gated Layer D shape evaluation dispatch to Layer D/debug use.
Documented the audit findings.
```

## Patch C — Layer E shader-side local detail probe

Status: implemented and validated as a technical proof in `4.11C.5.12`.

Validation result:

```text
The shader-side probe can create sub-cell/pixel-scale detail.
The Foam Shader Detail Difference view shows granular removals instead of simulation-cell bars.
It is not a standalone visual solution and should not be promoted to Final Foam yet.
It remains a useful future Layer E polish/detail tool after Layer D macro structure becomes credible.
```

Canonical conclusion:

```text
Layer E can own micro breakup, chipping, fray, thin cuts, and scratch/highlight detail.
Layer E cannot solve broad sheet/contact/bridge structure by itself.
```

## Patch D — Low-res Layer D Film Source / Film Support prototype

Status: implemented across `4.11C.5.13`, corrected by `5.13B` and `5.13C`, and semantically validated after `5.13C`.

Implemented products:

```text
_FoamFilmSource  — half-resolution, material-derived Layer D source field.
_FoamFilmSupport — half-resolution, directional spread/support field fed by Film Source.
_FoamShapeMask   — full-resolution domain-space visual mask.
```

Corrected contract after `5.13B` and `5.13C`:

```text
FoamState is material-space persistent truth.
Layer B support/contact/topology is domain-space external influence.
Film Source / Film Support / _FoamShapeMask are domain-space visual products.
Layer D samples FoamState through phase-corrected material coordinates but writes/samples its own products in domain coordinates.
Layer C material creates Film Source.
Layer B support can bias or suppress material-derived source/spread.
Layer B support cannot create Film Source from zero.
```

Validation result:

```text
Film Source no longer reproduces generic support topology.
Film Support is broader than Film Source and is fed by material-derived source.
Shape Difference now reports material-derived visual spread.
The result is stable and no longer pulses with the cell grid.
```

Remaining visual issue:

```text
Film Support currently behaves like blunt low-resolution dilation around the material ribbon.
It creates thick capsule-like widening rather than nuanced inspiration-river sheets.
This is a spread/threshold/tuning problem now that the semantics are clean.
```

## Patch E — 4.11C.5.13D Layer D Film Spread Shape Tune

Status: next documented implementation target.

This patch must tune the current Film Source / Film Support / Evaluated Shape formulas. It is not a new architecture and not a Final Foam integration.

### Purpose

```text
Make the material-derived Layer D spread less blunt, less uniformly inflated, and more suitable as a macro surface-film foundation.
```

### Current problem to fix

After `5.13C`, the support-topology pollution is gone. The remaining issue is that the half-resolution spread is too generic:

```text
Foam Film Source is now semantically correct but still broad/soft because it is half-resolution and thresholded.
Foam Film Support expands the source too uniformly across the river.
Foam Evaluated Shape inherits that broad support and can look like a fat capsule around the spawned material ribbon.
Foam Shape Difference shows valid material-derived additions, but the additions are too blunt and too continuous.
```

### Files to inspect before editing

Required code files:

```text
Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute
Game/Procedural/Rivers/StylizedRiverFoamRuntime.Compute.cs
Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs
Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs
Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader
Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl
```

Likely only `CS_RiverFoam.compute` needs behavior edits. Runtime binding should be touched only if inspection proves a parameter/input is missing.

Required docs after implementation:

```text
Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Docs/River_Foam_Stage6_Architecture.md
Docs/River_Rendering_Roadmap.md
Docs/Proof of Concept/08_Proof_of_Concept_Implementation_Log.md
Docs/Proof of Concept/09_Rock_And_River_Handoff.md
```

### Exact code areas to inspect

In `CS_RiverFoam.compute`:

```text
FoamResolveVisualFilmInfluenceAtDomainUV(...)
FoamResolveVisualFilmSourceAtDomainUV(...)
BuildFoamFilmSource
FoamLoadFilmSource(...)
BuildFoamFilmSupport
EvaluateFoamShape
```

The current formulas to review/tune are conceptually:

```hlsl
// Film Source
materialBody = smoothstep(presence low/high) * smoothstep(life low/high);
source = materialBody * influence.supportBias * influence.negativeSuppression;

// Film Support
along = weighted x-neighbour source samples;
across = weighted y-neighbour source samples;
diagonal = weighted diagonal source samples;
bridge = threshold(along) * threshold(across);
spread = max(sheet, bridge contribution) * supportBias * negativeSuppression;

// Evaluated Shape
baseShape = phase-corrected material presence;
sourceShape = threshold(filmSource);
supportShape = threshold(filmSupport);
visualFilm = max(source contribution, support contribution);
shapeMask = max(baseShape, visualFilm) * validFluid;
```

### Required tuning direction

Tune toward this behavior:

```text
Film Source should remain close to material truth, not become a thick source by itself.
Film Support should be broader than Film Source, but not a uniform capsule.
Along-flow continuity should be stronger than cross-flow widening.
Cross-flow widening should be conditional on nearby source/support evidence.
Bridge/fill behavior should require stronger evidence than the current prototype.
Final support contribution to _FoamShapeMask should be more conservative.
```

Concrete tuning levers:

```text
Raise or narrow Film Source presence/life smoothstep thresholds if source is too fat.
Reduce supportBias multiplier range if support exaggerates source too much.
Reduce cross-flow tap weights and/or cross-flow multiplier.
Gate across-flow spread by centre/along evidence instead of letting it widen everywhere.
Raise bridge thresholds and reduce bridge contribution.
Raise supportShape thresholds in EvaluateFoamShape.
Lower supportShape contribution so Film Support assists rather than dominates.
Keep negative suppression active and verify it still suppresses film correctly.
```

### Forbidden changes in 5.13D

```text
Do not switch Final Foam to _FoamShapeMask.
Do not reintroduce support-only Film Source.
Do not add environmental contact film yet.
Do not tune Layer E shader detail yet.
Do not add entity/pocket/connected-component tracking.
Do not mutate FoamState, Remaining Life, or Material Pattern from Layer D.
Do not add wide full-resolution neighbourhood classifiers.
Do not make shader-side wide-neighbour structural searches.
Do not add Inspector controls yet; formulas are still probe/tuning code.
```

### Acceptance criteria

`Foam Film Source`:

```text
Still follows actual material-derived foam.
No raw support topology appears where no material-derived foam exists.
Less over-thick source if current thresholding is excessive.
```

`Foam Film Support`:

```text
Still broader/smoother than Film Source.
Less uniformly inflated across the river.
More along-flow than cross-flow.
Fewer fat capsule edges around simple ribbon material.
No support-only source reappears.
```

`Foam Evaluated Shape`:

```text
Adds macro visual film coverage over material, but more selectively.
Does not look like a blunt dilation of the ribbon.
Still remains stable in domain space.
```

`Foam Shape Difference`:

```text
Green additions remain material-derived.
Green additions shrink/become more selective compared with 5.13C.
No broad support-topology shapes return.
```

`Final Foam`:

```text
Unchanged.
```

## Patch F — Future explicit environmental contact film

Status: intentionally postponed.

Reason:

```text
The inspiration river likely needs bank/rock/contact film that can exist without obvious spawned material. However, 5.13C proved that allowing generic topology support to create Film Source implicitly pollutes every Layer D-derived view. Any environmental film must therefore be added later as a separate named product with its own debug view, thresholds, and ownership rules.
```

Possible future product:

```text
_FoamEnvironmentalFilm or _FoamContactFilm
```

Rules if added:

```text
It must not be called Film Source.
It must not write FoamState.
It must be visibly separable in debug.
It must be limited to strict bank/rock/contact conditions.
It must have conservative width/intensity and must not flood the river.
```

## Patch G — Final Foam consumes _FoamShapeMask

Only after Layer D macro shape is visually accepted.

Scope:

```text
Shader broad foam structure samples _FoamShapeMask.
Legacy shader-side macro shape logic is demoted/removed.
Shader keeps Layer E local polish/thin streaks.
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
a new patch cannot state which layer owns each written texture;
Layer B support/topology creates Film Source from zero;
Layer D products are sampled with materialUV instead of fieldUV;
Final Foam is changed before Layer D is accepted;
Inspector controls are exposed for formulas still under architectural validation.
```

