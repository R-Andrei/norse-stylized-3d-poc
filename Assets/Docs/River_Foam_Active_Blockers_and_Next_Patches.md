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

Status: implemented and validated as plumbing; superseded/tuned by `4.11C.5.14B` for source-profile controls.

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

### 5.13D implementation notes

Implemented as a narrow compute-only tuning pass in:

```text
Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute
```

Code changes:

```text
FoamResolveVisualFilmInfluenceAtDomainUV:
  reduced supportBias from 0.90-1.18 to 0.94-1.08 so Layer B support remains a subtle bias/suppression path rather than an inflation path.

BuildFoamFilmSupport:
  kept along-flow continuity stronger than lateral widening;
  reduced cross-flow tap weights;
  added source/evidence gating for cross-flow spread;
  reduced diagonal contribution;
  raised bridge thresholds;
  lowered bridge contribution from 0.72 to 0.42.

EvaluateFoamShape:
  raised supportShape threshold from 0.18-0.62 to 0.28-0.74;
  raised sourceShape threshold slightly from 0.08-0.42 to 0.10-0.46;
  lowered sourceShape contribution from 0.72 to 0.68;
  lowered supportShape contribution from 0.88 to 0.60.
```

No runtime binding, shader sampling, debug enum, Final Foam integration, Inspector controls, environmental contact film, or persistent material transport changes were made.

Expected validation delta from `5.13C`:

```text
Foam Film Source remains material-derived and may be slightly less inflated by support.
Foam Film Support remains broader than source but should lose the thick uniform capsule look.
Foam Evaluated Shape should show more selective additions.
Foam Shape Difference should show smaller/more selective green material-derived additions.
Final Foam remains unchanged.
```

## Patch F — 4.11C.5.14A Layer C Automatic Shore/Contact Source Population

Status: implemented and validated as plumbing; superseded/tuned by `4.11C.5.14B` for source-profile controls.

### Audit result

The post-5.13D review corrected the next step. The screenshots showed that Layer D can widen a manually spawned central ribbon, but they did not prove that a new visual-only environmental/contact film product is needed. The existing architecture already intends this path:

```text
Layer B support/contact/topology
  -> Layer C material birth/source population + lifetime capture
  -> Layer D material-derived film spread / bridge / sheet support
  -> Layer E pixel-scale breakup / streaks / polish
```

Therefore, the missing piece is not a new Layer D authority. The missing piece is automatic source population that creates real persistent Layer C material near the environmental locations where foam should actually be born.

### Code evidence from the audit

Current code had these pieces before 5.14A:

```text
Manual/progressive birth exists:
  StylizedRiver.StartFoamSpawn()
  StylizedRiverFoamRuntime.StartFoamCompositionNormalized(...)
  AdvanceFoamCompositionEvents(...)
  QueueMaterialBirth(...)
  InjectFoam compute kernel

Support/lifetime capture exists:
  ComposeTopology writes shore/pressure/lee support sources.
  SimulateFoam samples topology/sources at material location.
  FoamResolveLocalAgeRate(...) slows supported material and accelerates negative-overlap material.

Automatic birth near specific places was missing:
  no source loop sampled shore/contact/wake/support candidates and queued Layer C material births.
```

### Implemented 5.14A scope

`4.11C.5.14A` adds the first conservative automatic source class:

```text
Automatic Shore/Contact Birth
```

It is deliberately small and safe:

```text
Disabled by default.
5.14A was initially controlled by Automatic Birth Enabled and an overloaded Shore Contact Birth Amount. Validation showed that this was too crude: the amount value affected density, footprint, life, amount, and compound shape together, producing large river-wide chunks at a moderate value such as 0.35. `4.11C.5.14B` replaces that with explicit source-population controls.
Runs at a low fixed source scan cadence.
Queues real Layer C material births through the existing PendingInjection / QueueMaterialBirth / InjectFoam path.
Places accepted candidates just inside the existing shore-support band.
Does not write Layer B fields.
Does not write Layer D products.
Does not make topology/support render as foam directly.
Does not switch Final Foam to _FoamShapeMask.
```

This means any visible material created by the automatic source is honest Layer C material. It then ages through the existing support/negative topology rules. Layer D may read it later as material-derived Film Source/Support.

### Changed code files

```text
Game/Procedural/Rivers/StylizedRiver.cs
Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs
Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs
Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs
Game/Procedural/Rivers/StylizedRiverFoamRuntime.RuntimeUpdates.cs
Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs
Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs
Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs
```

### Validation target

Use these views:

```text
Material Presence
Material Remaining Life
Foam Film Source
Foam Film Support
Final Foam
```

Expected result:

```text
With Automatic Birth disabled, behavior matches the previous manual-source baseline.
With Automatic Birth enabled and Shore Contact Birth Amount around 0.35, sparse material births appear near shore/contact bands.
Material Remaining Life shows supported shore births living longer than unsupported free-water material.
Foam Film Source follows those real material births; it does not show raw topology from zero material.
Foam Film Support spreads from those material births only.
Final Foam remains unchanged because final rendering still does not consume _FoamShapeMask.
```

### Forbidden follow-up regression

```text
Do not revive support-only Film Source.
Do not add a visual-only Environmental Contact Film product before source population is tested.
Do not let Layer D feed Layer C.
Do not let Layer E influence birth/lifetime.
Do not add foam entities/pocket IDs.
Do not enable automatic source population by default.
```

### Next patch after 5.14A validation

If 5.14A validates, the next likely source classes are:

```text
Obstacle/pressure contact birth.
Lee/wake birth behind registered geometry.
Major/connector support birth from prepared topology opportunities.
```

Add only one source class at a time. Each class must create real Layer C material, then let support/lifetime and Layer D/E handle survival and appearance.

## Patch F2 — 4.11C.5.14B Foam Source Population Controls / Shore Birth Profile

Status: implemented, but superseded by `4.11C.5.14C` before further visual validation.

### Result

`4.11C.5.14B` correctly identified that shore, river-body, obstacle-contact, and lee/wake foam must not share one generic birth algorithm. However, its Inspector surface was too bloated: it exposed low-level source internals such as density, per-tick budget, support threshold, inward band, radius, elongation, stroke length, initial amount, initial life, jitter, and shape mode.

That was the wrong authoring model. The architecture was right; the control surface was not.

## Patch F3 — 4.11C.5.14C Simplified Shore Spawn Controls

Status: implemented, validated as compile/control cleanup, and superseded by `4.11C.5.14D` because the resulting shore births were too sparse, same-shaped, and still too patch-like.

### Why this patch exists

The shore source class must be tested through a shore-specific algorithm and a small set of English-facing controls. The previous profile controls made the user tune implementation details rather than intent, and the results were not deterministic enough to reason about quickly.

`4.11C.5.14C` keeps the source-class architecture but hides the low-level shore recipe. Shore birth is now a deterministic sparse shoreline-stroke recipe with only these user-facing controls:

```text
Automatic Foam Birth
Spawn Preset
Shore Foam
  Coverage      how much shoreline receives foam over time
  Size          how large each shore seed/stroke is
  Strength      how visible new shore foam is at birth
  Persistence   how much initial life new shore foam receives
```

Hidden recipe rules:

```text
shore birth always uses small deterministic strokes;
compound blobs are not used for shore foam;
Coverage controls candidate acceptance/spacing and internal budget only;
Size maps to conservative radius/stroke length;
Strength maps to initial material presence;
Persistence maps to initial Remaining Life;
support capture still determines long-term survival;
Final Foam remains unchanged.
```

### Changed code files

```text
Game/Procedural/Rivers/StylizedRiver.cs
Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs
Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs
Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs
```

### Validation target

Use these views:

```text
Material Presence
Material Remaining Life
Foam Film Source
Foam Film Support
Final Foam
```

Expected result with `Automatic Foam Birth` enabled and `Spawn Preset = Shore Contact Test`:

```text
At Coverage around 0.35, shore births should be small deterministic flecks/strokes, not river-wide chunks.
Increasing Coverage should increase how much shoreline gets births over time, not each seed's footprint.
Increasing Size should make individual shore strokes larger while remaining near-shore.
Increasing Strength should make new material brighter/stronger without changing frequency.
Increasing Persistence should keep new material alive longer, but support capture should still be the main reason shore foam survives.
Final Foam remains unchanged.
```

### Next source classes after shore validation

Do not add more source classes until Shore Foam validates as controllable and non-blobby. After that, add one class at a time:

```text
River Body / Seam Birth: longer current-aligned seams in the river body.
Obstacle Contact Birth: small arcs/flecks near pressure/contact zones.
Lee / Wake Birth: downstream tapered streaks behind obstacles.
```

Each class must create real Layer C material through the existing birth/injection path and must have its own small English-facing controls.

## Patch F4 — 4.11C.5.14D Deterministic Shore Source Events

Status: implemented and visually failed; superseded by `4.11C.5.14E`.

### Why this patch exists

Validation of `4.11C.5.14C` showed that the simplified controls were better, but the hidden recipe was not. Even with all controls maxed, shore foam barely spawned; the spawned shapes were isolated, same-looking strokes, and visible birth still read as material appearing out of nowhere. The rejected direction was many faint deposits accumulating into visible foam. The accepted direction is fewer deterministic, full-strength source events that reveal shape spatially over time.

### Implemented model

`4.11C.5.14D` replaces one-shot shore stroke candidates with deterministic shore source events:

```text
Layer B/domain shore context
  -> deterministic shore source slots across both banks
  -> bounded Layer C source events
  -> existing progressive composition segments
  -> real FoamState material birth
  -> existing support/lifetime capture
  -> existing Layer D material-derived spread
```

The patch remains Layer C source population. It does not add a visual-only environmental film layer, does not make support topology render as foam, does not alter Remaining Life rules, and does not switch Final Foam to `_FoamShapeMask`.

### User-facing controls

The Source Population foldout keeps only intent-level shore controls:

```text
Automatic Foam Birth
Spawn Preset
Shore Foam
  Coverage    how much eligible shoreline can participate
  Activity    how often deterministic source events start
  Patch Size  how large each shore ribbon/tongue event is
  Pattern     Mixed / Shore Ribbons / Inward Wash
```

`Strength` and `Persistence` are no longer exposed for shore source testing. Automatic shore events use recipe-level normal-strength material values and existing support capture for survival.

### Recipes

Two shore recipes are implemented first:

```text
Shore Ribbon
  Thin, bank-parallel, opaque material source event.

Inward Wash
  Shore-attached source event that grows inward/downstream from the bank contact band.
```

`Mixed` deterministically alternates the two recipes per shore slot. Both recipes spawn real persistent material through the existing composition/injection path.

### Validation target

Use these views:

```text
Material Presence
Material Remaining Life
Foam Film Source
Foam Film Support
Final Foam
```

Expected result with `Automatic Foam Birth` enabled and `Spawn Preset = Shore Contact Test`:

```text
Coverage around 0.45, Activity around 0.45, Patch Size around 0.35, Pattern Mixed should start visible, opaque shore-attached source events over time.
Pattern Shore Ribbons should produce bank-parallel ribbon events.
Pattern Inward Wash should produce shore-attached inward/downstream tongue events.
Events should be distributed across the chunk through deterministic slots, not only one or two locations.
Events should not be faint deposits, river-wide blobs, or support-only topology.
Final Foam remains unchanged.
```

## Patch F5 — 4.11C.5.14E Automatic Source Event Rasterizer

Status: implemented in this patch; pending Unity compile/runtime validation.

### Why this patch exists

Runtime validation of `4.11C.5.14D` failed visually. With Coverage and Activity at maximum, shore foam still spawned as predictable near-shore rectangles/bars, Pattern `Shore Ribbons` and `Inward Wash` were not meaningfully distinct, and total coverage was still insufficient. The diagnosis is now explicit: `5.14D` still routed both automatic shore recipes through the existing progressive composition segment path, which produced generic `PendingInjection` / `InjectFoam` capsule stamps rather than true shore-local source shapes.

`4.11C.5.14E` keeps the approved architecture but replaces the automatic shore output mechanism:

```text
Layer B/domain shore context
  -> deterministic shore source slots across both banks
  -> bounded typed Layer C automatic source events
  -> dedicated RasterizeFoamSourceEvent compute kernel
  -> real FoamState material birth via FoamMergeBornPresence
  -> existing support/lifetime capture
  -> existing Layer D material-derived spread
  -> Final Foam unchanged
```

The old `PendingInjection` / `InjectFoam` path remains for manual/debug/simple injections. Automatic shore birth no longer depends on generic segment capsules.

### Implemented source vocabulary

The visible UI is unchanged:

```text
Automatic Foam Birth
Spawn Preset
Shore Foam
  Coverage
  Activity
  Patch Size
  Pattern: Mixed / Shore Ribbons / Inward Wash
```

Internally, shore events are now typed source records with source type, side, start/end global distance, shore inset, width, inward reach, feather, amount, life, seeds, breakup scale/strength, and curvature. The GPU rasterizer reads `_FoamCurrentShoreEdgesRead`, evaluates local inward-from-shore distance, and writes real persistent material into `_FoamStateWrite`.

Patterns are now shape-authoritative:

```text
Shore Ribbons
  bank-following analytic ribbon bands with rounded/tapered ends and deterministic edge breakup.

Inward Wash
  shore-attached tapered tongues revealed inward from the live shore edge, with downstream curvature.

Mixed
  deterministic mix of both source types.
```

### Validation target

Use these views first:

```text
Material Presence
Material Remaining Life
Foam Film Source
Foam Film Support
Final Foam
```

Expected result with `Automatic Foam Birth` enabled and `Spawn Preset = Shore Contact Test`:

```text
Coverage = 1 / Activity = 1 / Patch Size = 1 / Pattern Shore Ribbons should produce obvious thin bank-following ribbons, not generic rectangular bars.
Coverage = 1 / Activity = 1 / Patch Size = 1 / Pattern Inward Wash should produce shore-attached inward/downstream tongues that are visibly different from ribbons.
Pattern Mixed should show both source classes distributed deterministically across both banks.
Material should appear at normal strength; it should not depend on many faint deposits accumulating.
Layer D Film Source/Support should follow the new material.
Final Foam remains unchanged.
```

### Next likely work after 5.14E validation

If shore source masks validate, extend the same automatic source-event rasterizer with the next inspiration-critical source classes: open-water streamlines/sheet borders, then rock/contact arcs. Do not switch Final Foam to `_FoamShapeMask` until source material and Layer D macro spread are accepted.

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

## Patch 4.11C.5.14F — Source Formation Kinematics / Stroke Wash

Status: implemented after 5.14E validation showed the dedicated source-event rasterizer was the right direction but still formed sources too quickly and made Inward Wash read as blobs.

Facts from 5.14E validation:

- Source events were no longer generic capsule stamps, but their reveal still completed in roughly one second.
- The hardcoded source durations were replaced because they were not tied to the distance a source path had to form.
- Inward Wash previously filled the whole shore-to-current-reach area each update. Since Layer C material persists, that made the event accumulate into broad blobs/sheets.

Implemented direction:

- Added a user-facing Shore Foam `Formation Speed` control, expressed in metres per second. This controls how quickly each source forms along its own path; `Activity` still only controls how often events start.
- Source durations are now derived from source path distance divided by formation speed, with bounded minimum/maximum durations for stability.
- Inward Wash was converted from a filled growing tongue into a moving curved stroke-head. The head writes only a short stroke segment during each material update; persistent material preserves the already-drawn trail.
- Shore Ribbons continue to use area reveal, but that reveal is now paced by the distance-derived duration rather than fixed sub-second timing.

Validation target remains Material Remaining Life first, then Foam Film Source / Foam Film Support. Final Foam remains untouched.

## Patch 4.11C.5.14G — Shore Wash Stroke Refinement

Status: implemented after 5.14F validation showed Formation Speed was a major improvement and acceptable to park, but `Inward Wash` still produced chunky slab/card-like patches. Scope remains strictly Layer C shore spawning. This patch does not add static-object foam, free-water foam, Layer D tuning, or Final Foam integration.

Observed failure after 5.14F:

- Shore Ribbons are improved enough to keep moving.
- Formation Speed is good enough for now.
- Inward Wash remains the shore-spawning blocker because it still reads as broad compact slabs rather than small shore-detachment strokes.
- `Mixed` was still too contaminated by Inward Wash.

Implementation notes:

- `Inward Wash` internal dimensions were reduced from a large shore accent into a smaller detaching stroke source.
- Wash-specific head-trail limits were added so wash events no longer reuse ribbon-sized active drawing bodies.
- The wash curve now follows the shore first, then peels inward, instead of moving inward immediately from the first sample.
- Wash stroke width, feather inflation, fill-noise influence, and trail fraction were reduced to prevent blocky slabs.
- `Mixed` now heavily favours Shore Ribbon and uses Inward Wash only as an occasional accent until pure Inward Wash is acceptable.

Validation target:

- `Pattern = Shore Ribbons` should not regress.
- `Pattern = Inward Wash` should reduce or remove the large circled slab/card patches and show smaller shore-detachment strokes.
- `Pattern = Mixed` should mostly be shore ribbons with occasional small wash strokes, not blob-heavy.
- Validate in `Material Remaining Life` first. `Final Foam` remains out of scope.


## Patch 4.11C.5.14H — Foam Birth Source Authoring Framework

Status: implemented as a control/authoring pass after 5.14G validation showed shore spawning is now plausible enough to tune rather than rewrite again. Scope remains Layer C spawning only. This patch does not add object foam, free-water foam, Layer D tuning, or Final Foam integration.

Reason for the patch:

- Shore spawning now needs authorable pattern controls rather than more hardcoded recipe edits.
- Initial Remaining Life was still hardcoded per pattern; this is now exposed as `Initial Life` because it controls the normalized Remaining Life assigned to newly spawned persistent material.
- `Mixed` should control pattern composition, not total spawn density. Pattern weights are now normalized to one so changing one share recalculates the other.
- Length, width, and reach sampling must be correlated so short/fat or mismatched wash events are not produced by independent random rolls.

Implemented direction:

- Renamed the inspector source population foldout to `Foam Birth Sources`.
- Added category sections for `Shore Foam`, `Object Foam`, and `Free Water Foam`. Shore Foam is live; Object/Free Water are visible staged placeholders.
- Shore Foam now exposes normalized `Shore Ribbons` and `Inward Wash` pattern shares.
- Shore Ribbon and Inward Wash each expose Formation Speed multiplier, dimensions, Initial Life, and Breakup Strength controls.
- Runtime recipes now read those controls instead of hardcoded dimensions/life/breakup values.
- Dimension sampling now uses one correlated event scale with small axis jitter and aspect guards.

Validation target: use Material Remaining Life first. Confirm the main Shore Foam controls still work, pattern weights keep Mixed composition normalized, per-pattern Formation Speed affects only that pattern, and Initial Life changes how long newly spawned material survives under normal aging. Final Foam remains out of scope.

## Patch 4.11C.5.15A — Static Object Contact Foam Birth

Status: implemented in this patch.

This patch keeps the work scoped to Layer C spawning. It enables the Object Foam category in the Foam Birth authoring framework and adds CPU-scheduled static object/contact source events anchored from `StylizedRiverDisturbanceRuntime`'s registered static source data. The GPU source-event rasterizer remains the only automatic birth path and writes real persistent `FoamState` material through `FoamMergeBornPresence`.

Implementation decisions:

- Source scheduling uses exported static object source snapshots instead of scanning the obstacle texture for spawn candidates.
- The obstacle exclusion texture is used as a do-not-spawn-inside-solid gate.
- The static pressure texture is used as a contact relevance gate/bias for object contact arcs/flecks.
- Object Foam currently exposes Contact Arcs and Contact Flecks only. Wake tails and free-water foam remain later source classes.

Validation target: in Material Remaining Life, Object Foam should produce upstream/side contact arcs and small flecks around registered static river obstacles without filling obstacle footprints or generating full rings.

## Patch 4.11C.5.15A.1 — Object Birth Activation Wiring Fix

5.15A was intended to spawn Object Foam, but validation showed `Runtime Status: Object source population disabled` while Object Foam was enabled. The root cause was that the source-category active properties still treated `Spawn Preset` as a hard category gate. Object Foam could be enabled in its foldout but remain inactive unless the top-level preset was `ObstacleContactTest`, `Custom`, or `BalancedMixedTest`.

5.15A.1 makes source-category toggles authoritative: `Automatic Foam Birth` is the global switch, `Spawn Preset = Off` disables all automatic birth, and each category's `Enabled` toggle controls that category. Object Foam now also reports the number of static source anchors copied from the disturbance runtime, so activation failures can be separated from registration/export failures.

## Patch 4.11C.5.15A.2 — Object Contact Edge Field

Status: implemented after 5.15A.1 validation proved Object Foam events spawned, but Contact Arc/Fleck masks were visibly crude and rectangular.

Reason for the patch:

- 5.15A shaped object foam primarily from object-local half extents, which produced box-like front/side bands.
- The correct spawning architecture remains CPU-scheduled bounded object source events, but the visible mask needs to follow actual water/object contact evidence instead of a rectangle approximation.
- A GPU contact-edge field is the best quality/performance step: it is cheap at current foam resolutions, has no GPU readback, and gives the source-event rasterizer local contact confidence and normal data.

Implemented direction:

- Added `PS3D_RiverFoam_ObjectContactField`, an ARGBHalf field built from obstacle exclusion plus static pressure/contact evidence.
- The field stores contact confidence, object-to-water normal, and upstream/front-side relevance.
- Object Contact Arc and Contact Fleck rasterization now samples this field and uses contact normal/tangent space instead of drawing box-like bands from object extents alone.
- Object extents remain only as coarse event bounds/rejection windows, preserving bounded CPU scheduling and range-limited GPU dispatch.

Validation target: in Material Remaining Life, Object Contact Arcs should read as partial edge-hugging arcs or shoulder strokes rather than rectangular slabs. Contact Flecks should read as small contact slivers/fragments. No material should appear inside obstacle footprints. Shore Foam remains out of scope except for regression checks.

## Patch 4.11C.5.15A.3 / 5.15A.3.4 — Object Contact Field Recovery Note

Status: 5.15A.3 attempted to reinterpret the object-contact field as a sharper contact-edge distance authority, but the patch failed because the compute resource/file set was incomplete. The observed failure sequence was a compute import/runtime break around `_FoamObjectContactFieldRead`, followed by a C# type mismatch where a `Texture2D` fallback was passed to a `RenderTexture` helper. 5.15A.3.4 restored the stable binding path: `_FoamObjectContactFieldRead` is declared in HLSL, bound from C# for `RasterizeFoamSourceEvent`, and falls back through existing created textures without the invalid `Texture2D`/`RenderTexture` call.

Current stable state after recovery: Object Foam spawns again through the 5.15A.2 broad object-contact field. It is visually better than the first object pass, but Contact Arcs are still too symmetrical because the rasterizer evaluates full arcs around the tangent centreline. Do not retry the sharper edge-distance field correction in the same patch as source-pattern variation.

## Patch 4.11C.5.15A.4 — Object Contact Semi-Arc Pattern

Status: implemented as the next Layer C object-spawning variation patch. Scope remains Object Foam birth only; no free-water source spawning, wake-tail spawning, Layer D tuning, Final Foam composition change, or new compute resource was added.

Reason for the patch:

- The existing `ObjectContactArc` source is intentionally centred in contact tangent space, so it tends to produce symmetric bracket/full-arc shapes.
- The inspiration target and validation screenshots need object foam to sometimes appear as a one-sided shoulder mark or partial/lopsided arc.
- The already-existing `AutomaticFoamSourceEvent.Curvature` / GPU `variation.w` channel can carry signed lopsidedness, so this patch does not touch the fragile object-contact texture allocation/binding path.

Implemented direction:

- Added `ContactSemiArcs` as a third Object Foam pattern while preserving the existing serialized enum values for `Mixed`, `ContactArcs`, and `ContactFlecks`.
- Added normalized three-way Mixed weights: Contact Arcs, Contact Semi-Arcs, and Contact Flecks.
- Added per-pattern Semi-Arc controls for Formation Speed, Length, Width, Contact Offset, Initial Life, Breakup Strength, and Lopsidedness.
- Added `ObjectContactSemiArc` automatic source event type and recipe selection.
- CPU scheduling now assigns a deterministic signed lopsidedness value to semi-arc events and stores it in `Curvature` / GPU `variation.w`. Full arcs and flecks keep zero curvature/lopsidedness.
- Added `FoamEvaluateObjectContactSemiArcSource`, which replaces the full-arc `abs(tangentDistance)` support with a signed one-sided tangent window: `-backReach < tangentDistance * side < revealedForwardReach`.

Validation target: in `Material Remaining Life`, pure `Contact Arcs` should match the previous full-arc behavior, pure `Contact Flecks` should not regress, and pure `Contact Semi-Arcs` should produce visibly lopsided/one-sided object shoulder arcs without spawning inside obstacle footprints. Mixed should show all three object-source classes according to the normalized pattern weights.

## Patch 4.11C.5.15B — Free Water Lace / Fragment Birth

Status: implemented in this patch set.

This patch enables the previously reserved Free Water Foam Layer C birth category. It deliberately does **not** spawn final-render glints or broad rectangular/sheet decals. It adds two persistent-material source grammars:

- **Lace Connectors**: moving head + stroke events that draw sparse, curving, torn open-water connectors.
- **Torn Fragments**: local asymmetric patch events revealed by a short linear sweep, so fragments grow in over time instead of popping in instantly.

Free-water placement uses a bounded deterministic open-water slot lattice across longitudinal slots and five lateral lanes. Candidates are clipped to valid fluid by the existing GPU boundary/obstacle gate. A cheap static-object proximity reject keeps this source category from duplicating object-contact birth. The automatic source rasterizer now supports Y-range clipping for local free-water events while shore/object events keep full-height dispatch.

Validation target: use Material Remaining Life with Shore and Object Foam disabled first. Expected result is open-water lace connectors plus detached torn fragments, not circles, rectangles, broad slabs, or specular scratches.

## Patch 4.11C.5.15B.2 — Free Water Cross-Lace Connectors

This patch adds the missing cross-current open-water birth grammar. `4.11C.5.15B` produced only with-flow Lace Connectors plus Torn Fragments, so open-water foam read too vertically/flow-aligned compared with the inspiration footage. `5.15B.2` adds **Cross-Lace Connectors** as a third Free Water Foam pattern: a moving head+stroke source whose primary sampled axis is lateral/across-river and whose secondary bend is along flow.

Implementation notes:

- No new textures, buffers, kernels, or readbacks.
- Reuses the existing automatic source-event rasterizer and Y-range dispatch clipping.
- Packs cross-lace shape data into existing event fields: `objectData.x = centreAcrossMetres`, `objectData.y = lateral half-length`, `objectData.z = ribbon width`, `objectData.w = lateral draw sign`.
- Keeps Coverage/Activity unchanged; life tuning remains a separate validation/tuning decision.

Validation target: pure `Cross-Lace Connectors` should produce horizontal/cross-current torn ribbons in `Material Remaining Life`, not rectangles, dots, slabs, or specular glints.

