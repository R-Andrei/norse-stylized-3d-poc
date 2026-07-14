# Accepted source patch — `4.11C.5.17D.0` Final-Edge Fray Retirement

Fray is retired after gameplay-camera validation showed no useful independent visual category. The patch removes every Fray-only production helper, procedural field, property, binding, serialized authoring control, Inspector entry, and debug view. It also removes the dead transient `breakupField` / runtime `_FoamBreakupScale` path that no surviving mechanic consumed. No scene, prefab, material, or other serialized Unity asset is modified; stale serialized Fray values are harmless and any manual cleanup remains owner-controlled.

The active Layer E order is now:

```text
coherent Foam → analytical Chipping → structural Strands → composition
```

Compatibility retained only for surviving Chip data:

```text
legacyFoamBreakupScale
  hidden FormerlySerializedAs source used by historical Chip migration only;

foamChipFraySelectionTuningVersion alias
  historical serialized name retained only to preserve Chip migration state.
```

Neither compatibility alias has a Fray runtime path. Unity import and visual validation remain required.

# Next active patch — `4.11C.5.17D.1` Chipping Readability Audit

Audit Chipping from the production isometric camera before changing its equations. Capture one compact diagnostic record covering projected candidate size, visible removal, edge/interior authority, sub-readable candidates, and later Strand occlusion. Use the evidence to decide whether the next implementation needs stronger medium/large bias, a higher projected-size floor, revised edge admission, or simpler authoring controls.

Remaining-Life morphology integration stays blocked until Chipping and Strands are accepted as the complete readable Layer E vocabulary.

# Deferred validation — `4.11C.5.17B.2D2B-B.2L` Decoupled Shape Cadence and Transition

B.2K geometry is implemented and visually meaningful, but Unity evidence showed that the existing Shape Change Speed control behaved primarily as target cadence: contour configurations could switch too quickly even when the desired geometric extent was correct. B.2L remains the deferred Chipping timing validation patch; Fray was later retired by D.0.

B.2L retains B.2K geometry and exposes:

```text
Shape Change Cadence (changes/s)
Shape Transition Time (seconds)
Resolved Shape Timing = effective transition / hold
```

Cadence selects the next deterministic target. Transition Time independently controls actual interpolation duration. Targets are separated by a constant golden-angle step in the existing two-axis coefficient plane, so every transition has equal geometric travel. Quintic interpolation gives zero velocity at both endpoints; candidate phase offsets prevent synchronized field-wide switching.

Validation gates:

1. Amount changes geometric extent only;
2. Cadence changes how often new targets are selected;
3. Transition Time changes how slowly the actual contour crosses between targets;
4. increasing Transition Time does not change Candidate Radius, Size Pulse, lifecycle, movement, or rotation;
5. transitions are continuous at cadence boundaries and show no popping;
6. confirm D.0 retirement does not alter Chip geometry, timing, lifecycle, movement, or search reach.

# River Foam Active Blockers and Next Patches

## Purpose

This is the short working document for current Stage 6 Foam blockers and immediate patch order.

Canonical architecture lives in `River_Foam_Stage6_Architecture.md`. Macro stage order lives in `River_Rendering_Roadmap.md`. This document owns the active recovery queue.

This document must not preserve stale active plans. Historical patch notes may appear only as clearly superseded context.

---


# Accepted — `4.11C.5.17B.P1` River-only startup gate

Unity validation passed for exact, stale-compatible, missing/incompatible, explicit-preparation, and full-scene workflows. Ordinary Play is cache-only: exact and stale-compatible payloads install without generation or persistence, while unusable caches stop in a stable **Preparation Required** state. GeneratedGeometryRegistry restoration bursts no longer trigger hidden topology generation, delayed replacement, cache mutation, or Play `SaveAssets`.

The River→Ground regeneration issue remains fully deferred to a separate cross-feature thread.

# Accepted — `4.11C.5.17B.P3` explicit cache pipeline cleanup

Unity validation confirmed one final generated-topology GPU publication, one normal serialization, strict cache validation, exhaustive integrity proof, clean temporary-resource release, and zero Play generation/persistence. The remaining Edit-to-Play obstacle fingerprint drift is external geometry-restoration evidence; River Foam correctly installs that payload as stale-compatible without rebuilding or saving.

# Accepted — `4.11C.5.17B.P4` steady-state work accounting

Unity validation measured populated visible, populated offscreen, and empty-field work. Offscreen work currently remains essentially identical to visible work, and empty fields still perform meaningful topology/readback work. Those optimization opportunities are deliberately deferred until the River is feature-complete. P4 remains available as the final-state profiling baseline.

# Accepted — `4.11C.5.17B.2D2B-B.2F/B.2G` lifecycle, rigid evolution, and view readability

Unity validation accepted the independent four-stage candidate lifecycle, rigid motion/variation controls, and bounded projected-size readability. Formation/Dissolve still reach zero, Dormant Time remains empty, motion no longer uses a deforming coordinate warp, and the close/far camera comparison now retains readable Chips with the available view-LOD controls.

# Accepted — `4.11C.5.17B.2D2B-B.2J` Simplified Orthogonal Chip Permission

Unity validation accepted the two-path permission model. `Chip Edge Coverage` remains functional at Interior Access `0`; `Chip Interior Access` independently adds established-body candidate authority; and removing the failed third admission gate restored predictable production and diagnostics.

# Accepted foundation — `4.11C.5.17B.2D2B-B.2K` Multi-Axis Contour Geometry

Unity evidence confirmed that B.2K provides the required meaningful geometric silhouette change. Its remaining defect was timing ownership: the former cycles-per-second control also determined how abruptly the contour crossed between configurations. B.2L addresses timing only and leaves the accepted B.2K geometry, size separation, and `5×11` search bound intact.

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

## Historical accepted state through `4.11C.5.17B.2D2A — Presence-Space Chip and Fray Reconstruction`

The zero-memory presentation audit passed decisively:

```text
Final Foam with residual prediction
  stuttered beside rocks and banks;

Foam Committed Final Preview
  did not stutter;

Foam Evaluated Final Preview
  did not stutter.
```

The committed conservative state is stable. Point-velocity residual backtracing was the active presentation fault and is retired rather than replaced with another history texture. Normal Final Foam now uses the current committed Layer C state directly while retaining both accepted visibility policies, surface coupling, colour, lighting, opacity, and fog.

Removed active ownership:

```text
_FoamRenderAdvectionSeconds and CPU bookkeeping;
render-side ResolveRenderedFoamVelocity;
obstacle-influence residual confidence fade;
downstream/lateral residual travel backtrace;
Foam Committed Final Preview.
```

`Foam Evaluated Final Preview` remains available as a diagnostic-only Layer D comparison. Serialized debug value `16` is retired and resolves safely to Final Foam. The two Layer C state textures remain required simulation ping-pong storage.

Cost:

```text
new allocations / fields / channels / buffers / kernels / dispatches = 0;
new texture memory = 0 bytes;
production fragment velocity reconstruction and related texture reads removed.
```

### Accepted validation result

Unity validation confirmed:

- clean C# and D3D11 compilation;
- normal Final Foam matches the former Committed Final Preview;
- rock- and shore-constrained stutter is gone in both visibility policies;
- open-water Foam remains acceptable at the tested material cadence;
- Material Presence, Remaining Life, topology, conservative transport, and temporal occupancy are unchanged;
- `Foam Evaluated Final Preview` remains available and unchanged;
- serialized debug value `16` displays normal Final Foam.

### Accepted Inspector and diagnostics state

River Inspector redesign R1–R5 is Unity-validated and accepted:

```text
all sections collapsed by default;
authoring grouped by River feature and Foam Layers A–E;
one exclusive Debug Views hub over the existing serialized enums;
legacy conflicts reported without silent modification;
read-only stable-height Runtime Diagnostics;
Generated Status separated from authoring;
all mutating/test tools centralized under Actions;
constant repaint limited to visible live diagnostic leaves;
Editor implementation split into focused partial files.
```

The temporary redesign plan is retired by R6 after this contract is recorded canonically.

### Accepted capacity audit and deferred limitation — `4.11C.5.16E.3C`

`4.11C.5.16E.3 — Transport Presence Capacity-Loss Attribution Audit` is Unity-validated and accepted. The actual Inspector path is:

```text
Runtime Diagnostics
  Foam
    Layer C — Material & Lifecycle
      Transport Accounting
```

Controlled automatic-source tests produced:

| Test | Total Presence capacity loss | Unit overflow | Boundary capacity | Obstacle capacity | Peak raw Presence | Peak local excess |
|---|---:|---:|---:|---:|---:|---:|
| Open water only | 0.067% | 0.000% | 0.067% | 0.000% | 0.7727 | 0.0215 |
| Shoreline only | 0.484% | 0.222% | 0.262% | 0.000% | 1.2046 | 0.2046 |
| Obstacle only | 0.747% | 0.479% | 0.267% | 0.000% | 1.2246 | 0.2246 |

All three captures reported zero state-validity loss, zero minimum-state cutoff loss, zero obstacle-capacity loss, and zero signed attribution residual. The result proves that the active defect is receiver-capacity overflow in valid cells plus fractional shoreline capacity, not material entering rock footprints, lifecycle ownership, packed-state validity, or epsilon cleanup.

`4.11C.5.16E.3C — Capacity Audit Closure + Deferred Limitation Record` changes no transport or rendering behavior. The original `0.10%` capacity-loss value remains the engineering target. For the current PoC, measured loss below a temporary `1.00%` review threshold is consciously tolerated because the exact correction options have disproportionate recurring cost relative to the demonstrated visual impact. Any capture above `1.00%`, visible material disappearance, or materially different content must reopen the numerical issue.

Deferred correction options:

```text
three-pass receiver-acceptance solve
  exact and conservative;
  approximately 2–3× current Layer C transport work;

single-pass directional vacancy cap
  zero new dispatches and memory;
  estimated +10–25% Layer C transport cost;
  risk of visible near-capacity congestion;

fractional shoreline storage relaxation
  approximately zero recurring cost;
  partial visual A/B candidate only;
  does not fix ordinary unit-capacity overflow and may create shoreline reservoirs.
```

Capacity-hit counters are cell-substep samples. Category counts may overlap when one sample exceeds both unit and fractional boundary capacity; `Total` is the union of all capacity-hit samples.

Cost of the retained attribution remains:

```text
new textures / persistent fields / channels / kernels / dispatches / readbacks = 0;
transport metric buffer = 23 uints / 92 bytes;
additional memory over the original audit buffer = 44 bytes per active Foam runtime;
new HLSL arithmetic and atomics run only during the existing 4 Hz diagnostic capture.
```

### Approved next-stage plan — `4.11C.5.17P`

The inspiration comparison is refreshed before final rendering work. The production river is not expected to copy the reference one-to-one. Its accepted macro result already contains the required family resemblance: broad predominantly horizontal bands, lateral travel, split/merge behavior, obstacle-driven convergence, and stronger shore accumulation. Slightly fatter ribbons and greater bank accumulation are acceptable consequences of the current field resolution and source grammar. Layer C and the existing motion system remain the macro authority.

The remaining target is local rendered character: foam should read as pale, substantially opaque, chipped, frayed, and energetic while preserving the accepted macro ribbons.

The approved Layer E order is:

```text
committed Layer C material and selected visibility policy
  -> local rendered morphology
  -> interior colour / opacity composition
  -> post-breakup edge contrast
  -> final water lighting, fog, and reflection/refraction composition
```

Layer E reads upstream data and writes screen pixels only. It adds no persistent damage state, transported channel, compute pass, or feedback into Layers C or D.

### `5.17A / 5.17A.1 — Layer E Interior Composition`

`5.17A` failed visual validation. The controls were bound correctly, but `Interior Fill Strength` operated after the visible mask had already been hardened near full coverage, `Interior Opacity Floor` was incorrectly capped by `Foam Colour` alpha, and `Edge Emphasis Strength` added only a weak second whitening treatment instead of controlling the existing rim produced by the edge-versus-interior lighting transition.

`4.11C.5.17A.1 — Interior Composition Authority Correction` replaces that failed control model. Its authoring surface is:

```text
Foam Colour                 base RGB/tint and base opacity before the interior floor
Interior Opacity Floor      absolute minimum opacity for established Foam body
Edge Contrast               signed control over the existing edge-versus-interior lighting contrast
```

`Interior Fill Strength` is removed. The current visibility path already supplies a hardened established body, so a second fill remap had no useful authority and risked duplicating later morphology. `Interior Opacity Floor` may now exceed `Foam Colour` alpha, but it is gated by `smoothstep(0.42, 0.82, incoming mask)` and therefore cannot create Foam in weak fringe or outside the incoming silhouette. `Edge Contrast` ranges from `-1` to `+1`: negative values suppress the existing bright rim toward filtered interior lighting, zero preserves the accepted pre-5.17A lighting exactly, and positive values intensify the existing edge.

Production Final Foam and Foam Evaluated Final Preview share the corrected composition helper. Neutral values are:

```text
Interior Opacity Floor = 0
Edge Contrast           = 0
```

At neutral values, opacity reduces exactly to `smoothstep(0.08, 0.46, mask) × Foam Colour alpha` and lighting reduces exactly to the pre-5.17A edge/interior transition. The correction adds arithmetic only: no texture sample, branch, loop, neighbourhood stencil, persistent resource, compute dispatch, readback, material-state change, or silhouette expansion.

### `5.17B / 5.17B.1 — Rejected Hardened-Mask Breakup`

`4.11C.5.17B` and `4.11C.5.17B.1` are both visually rejected. The decisive blind comparison used Breakup Scale `0` with Chip/Fray `0` versus `1`; the difference was weak enough that the images were identified backwards. Stronger threshold constants did not solve the ownership error.

Both patches applied breakup after the visibility signal had already been hardened:

```hlsl
float hardVisible = smoothstep(0.22, 0.58, softVisibility);
float fringe = smoothstep(0.06, 0.34, softVisibility) * 0.34;
float hardenedMask = saturate(max(hardVisible, fringe));
```

Most visible body pixels therefore entered the breakup helper near `1.0`. The old equations altered mainly the narrow antialiased transition, while `5.17A.1` Interior Opacity Floor concealed partial erosion that did not reach zero. Breakup Scale also weakened the result at broader settings because the broad/diagonal composite had a compressed centre-weighted distribution. No further hardened-mask threshold recalibration is allowed.

### Historical `5.17B.2 — Pre-Hardening Binary Edge Cuts`

> Superseded by D2A findings and the active `2D2B-A` control-divorce/selection-diagnostic patch. This records the older implementation and is not current authoring guidance.

Historical status: Unity-validated for visible authority at that time. Their neutral-versus-maximum comparison is unmistakable. Chip, Fray, and Breakup Scale are provisionally accepted as useful reference controls; D1 now extracts their lineification family before D2 assigns Chip and Fray distinct visual roles.

The public authoring surface is unchanged:

```text
Chip Strength   0–1; default 0
Fray Strength   0–1; default 0
Breakup Scale   0–1; default 0.5
```

`RiverWaterFoamPatternedMask` now preserves its continuous pre-hardening `softVisibility` transiently while leaving the accepted hardening equation numerically unchanged. `RiverWaterFoamResult` carries that scalar through the existing visual warp, stretch, surface-break, stored-retention, and freeze coupling beside the existing hardened mask. Layer E then performs antialiased binary survival tests against `softVisibility` and multiplies the result into the hardened mask. Selected chips and short cuts may therefore reach true zero coverage; fray uses a shallower threshold. Exact saturated soft cores remain protected. The post-breakup result is removal-only and always satisfies `postBreakupMask <= hardenedMask`, so Interior Opacity Floor cannot refill a removed pixel.

The stable pattern path still evaluates exactly the same broad, diagonal, mid, and fine noise calls. Only the transient Chip/Fray outputs are normalized before Scale interpolation: mid and broad chip fields receive separate contrast normalization, as do fine and mid fray fields. Static distribution analysis shows the selected-field means remain effectively matched across Scale endpoints, so Scale changes feature size/frequency without silently collapsing authority. The accepted combined visibility pattern is unchanged.

Production Final Foam, Foam Evaluated Final Preview, Foam Shader Detail Probe, and Foam Shader Detail Difference continue to use the same breakup helper. The Probe shows the exact production post-breakup silhouette. Difference remains removal-only: black is unchanged and magenta/red is removed coverage; green remains zero. The evaluated preview supplies its evaluated shape to the same binary helper without promoting Layer D to production.

Neutral Chip and Fray values return the exact accepted hardened mask, and Breakup Scale alone does nothing. The fixed proof still reads no Remaining Life, Support, Negative Topology, surface-energy multiplier, river-location multiplier, or additional time input. It adds no texture sample, procedural-noise call, texture, buffer, persistent field, compute kernel, dispatch, readback, shader property, or C# binding. Incremental cost is fragment arithmetic plus one transient scalar and possible register pressure.

Unity validation passed the authority requirement: Chip and Fray are clearly visible at maximum strength and Breakup Scale visibly changes their result. Same-state comparison later proved that the useful lineification came from the hidden anisotropic band breaker combined with Chip and Fray, not from the explicit `5.17B.2A` periodic Strand path.

### Superseded `5.17B.2A` / rejected `5.17B.2B`

`5.17B.2A` proved that a separately controllable Strand authoring group was useful, but its periodic lane generator was not the desired effect and is retired by D1. `5.17B.2B` is visually rejected and its Fragmentation Strength, Fragment Size, and Fragment Reach controls are removed; it only shaved the same partial-presence perimeter rather than splitting medium regions.

### `5.17B.2C — State-Preserving Foam Authoring`

Status: Unity-validated and accepted.

The River Inspector no longer treats every serialized edit as a structural river change. `OnValidate()` still clamps settings, keeps required outputs/runtimes present, and applies live material values, but it no longer queues `RegenerateAll()`. The custom Inspector now requests the existing debounced full rebuild only when one of the structural authoring sections changes:

```text
Setup
River Domain
Channel Shape
Shoreline Safety
Natural Variation
Surface Mesh
```

Spline edits remain structural through the existing spline-change callback. Water rendering, surface motion, refraction, runtime-disturbance tuning, Foam Layers A–E, debug selection, and diagnostics no longer rebuild the River domain merely because a value changed. Layer E rendering values continue through the existing per-frame property binding, so they preserve the active Layer C textures.

`Runtime & Quality` also exposes the non-persistent Play Mode diagnostic **Hold Foam State**. While held, the runtime preserves the allocated Layer C material and existing Layer D products, skips topology evolution, births, aging, transport, and Layer D temporal advancement, discards elapsed wall time, and continues binding the current textures plus live Layer E properties. Pending manual/automatic work remains queued and resumes without catch-up when the hold is released. The toggle is not serialized authoring data and resets when the component is enabled or disabled.

This patch adds no texture, buffer, shader property, compute kernel, dispatch, readback, or persistent simulation field. Structural or resource-allocation changes may still legitimately rebuild and clear state; Hold Foam State is for same-domain rendering comparisons, not for preserving material across incompatible domain changes.

### `5.17B.2D1 / D1A — Lineification Extraction`

`5.17B.2D1` is visually rejected. It reconstructed Strands from a coherent-to-lineified delta, a fixed canonical Chip/Fray pattern pair, broad group suppression, and a derivative threshold over the already-cut removal mask. Unity close-ups showed that this was not the current Chip-plus-Fray morphology: it produced short screen-aligned dashes, square steps, discontinuous groups, and severe pixel quantisation. The failed reconstruction and its transient Strand field are removed.

`4.11C.5.17B.2D1A — Exact Lineification Extraction` is Unity-validated for exact same-state Chip-plus-Fray versus Strand equivalence. The neutral body and separate stable lineified soft signal from D1 are retained. Current Chip and Fray remain unchanged as the reference. Strand Strength now literally calls the same current Chip and Fray survival helpers with:

```text
same lineified soft visibility
same Breakup-Scale-selected Chip pattern
same Breakup-Scale-selected Fray pattern
same fwidth antialiasing footprint
same exact-core protection
```

Therefore, at the same Breakup Scale, these configurations are intended to be mathematically equivalent:

```text
A: Chip Strength = 1, Fray Strength = 1, Strand Strength = 0
B: Chip Strength = 0, Fray Strength = 0, Strand Strength = 1
```

The proof deliberately adds no spacing reinterpretation, width reinterpretation, curvature warp, grouping mask, delta reconstruction, or post-threshold screen-space culling. Strand Spacing, Strand Width, and Strand Curvature remain serialized for the later shaping step but are visibly disabled in the Inspector and have no shader authority during D1A. Production Final Foam and Foam Evaluated Final Preview use the same exact helper.

D1A removes the failed reconstruction and was Unity-validated for exact Chip-plus-Fray versus Strand equivalence.

### `5.17B.2D1B — Strand Shaping and Projected Detail Floor`

Status: visually rejected and superseded by D1D. Spacing and Width behaved as inverse occupancy controls, Curvature had negligible authority, and projected filtering did not remove one-pixel noise.

D1B restores Strand Spacing, Width, and Curvature together with source-stage projected-detail control. Reference values remain `Spacing 0.55`, `Width 0.50`, and `Curvature 0.55`. At those values and resolved camera distances, the dedicated Strand signal/pattern pair reduces to D1A. Fine and medium source bands automatically fall back to existing coarser bands when their projected river-space density becomes unresolved. This filtering occurs before Chip/Fray survival thresholds; no finished removal mask is thresholded again.

Acceptance gate:

1. Spacing must reduce/increase visible Strand density without creating a new seed-like pattern family.
2. Width must change channel breadth rather than merely opacity.
3. Curvature must broadly bend/reorganize the same stable family without time crawling.
4. At gameplay distance, fine line groups must become fewer/coarser before they become square, dotted, dashed, or shimmering pixel noise.
5. Reference settings must remain close to the validated D1A morphology at near/resolved views.
6. Current Chip and Fray remain unchanged; D2 is blocked until this Strand pass is accepted.

Resource contract: no new texture sample, noise/hash evaluation, texture, buffer, persistent field, compute work, dispatch, readback, Layer C mutation, or Layer D mutation. Expected cost is transient fragment arithmetic/register pressure.


### `5.17B.2D1C — Strand Spatial Controls and Resolution Cutoff`

D1C is visually rejected. Spacing, Width, and Curvature changed only a secondary anisotropic band while the decisive extracted Chip/Fray candidate patterns remained fixed. Unity therefore found all three controls effectively inert, and one-pixel noise remained. The audit also proved that projected frequency omitted transported Material Pattern phase and that visible warped/lead/trail Strand shapes could be paired with the stored candidate pattern.

### Historical — `5.17B.2D1D — Strand Control Model Reset and Coherent Pattern Transport`

D1D is Unity-validated and accepted. Its Strand Strength, Scale, Density, and Reach controls now produce viable controlled lineification without excessive visual artefacting.

The active Strand controls are:

```text
Strength — total Strand authority
Scale    — broad/medium/fine hierarchy
Density  — candidate prevalence only
Reach    — inward depth/eligibility only
```

Scale builds a dedicated Strand pattern pair from existing bands. Density changes only derivative-antialiased candidate thresholds. Reach changes anisotropic attenuation, presence reach, and maximum soft-depth cuts. The old Spacing/Width/Curvature fields migrate to the new names and no longer exist as active shader properties.

Resolution includes river-coordinate footprint plus transported Material Pattern seed-phase footprint. Fine and medium sources fall back hierarchically; unresolved broad authority returns to coherent Foam. Stored, visual, lead, and trail Strand patterns travel with their owning soft shape and `max` winner. No finished-mask dithering or post-cut culling is used.

Immediate acceptance tests:

1. Held-state Chip `1` + Fray `1` capture remains unchanged before/after D1D.
2. Scale changes subdivision while preserving related broad organization.
3. Density changes prevalence without materially changing cut depth.
4. Reach changes inward penetration without materially changing candidate count.
5. Fine detail simplifies to broad structures, then coherent Foam, before one-pixel noise.
6. Rock/wake regions show fewer crossed pattern/shape mismatches.

### Historical — `5.17B.2D2 — visually rejected`

D2 changed thresholds and pattern hierarchy but still treated `coherentSoftVisibility` as edge depth. Unity showed nearly the same elongated contour excavation as the former implementation. The scalar contains internal morphology valleys and cannot identify true perimeter distance. D2 is closed; no constant-only follow-up is allowed.

### Historical — `5.17B.2D2A — Presence-Space Chip and Fray Reconstruction`

D2A is implemented with Unity validation pending. Accepted D1D Strands are frozen. Chip and Fray now consume a transient base-material edge depth rather than coherent visibility valleys. Chip contributes a medium broad-pattern depth requirement; Fray adds a shallow fine perturbation to the same requirement. The modified soft body is re-hardened, then mapped back onto the accepted coupled production mask through a removal-only ratio.

Immediate acceptance tests:

1. Chip alone creates identifiable edge-connected medium notches with no long internal channels.
2. Fray alone preserves the major body and adds only shallow perimeter teeth.
3. Chip plus Fray retains the same Chip notches while Fray roughens their rims.
4. Strand-only output remains visually unchanged from accepted D1D.
5. Neutral Chip/Fray output is exact.
6. If the model fails, escalate to a narrow neighbour stencil or edge-distance field; do not recalibrate visibility thresholds again.

### Lifetime and topology rule

Direct Support or Negative Topology breakup multipliers are explicitly excluded from `5.17A` and the first `5.17B` proof. Layer C already converts topology into Remaining Life through the configured aging rates. Supported foam therefore remains visually younger for much longer, neutral foam follows normal lifetime, and negative foam expires rapidly. Current proof tuning is Neutral Lifetime `7.5 s`, Supported Aging Rate `0.08×`, and Negative Aging Rate `7.5×`; these values describe the validation setup and are not silently promoted to new project defaults. The first `5.17B` proof does not read Remaining Life at all; it isolates the fixed morphology vocabulary before temporal progression is introduced. `5.17C` will then use Remaining Life as the initial and sole temporal fragility signal, allowing the existing lifecycle system to prove whether it supplies enough differentiation without hidden additional help.

Do not sample support/negative topology directly for breakup unless later Unity evidence shows that Remaining Life alone is insufficient and the user separately approves that coupling. Layer E must never modify Remaining Life.

### Later polish

Remaining-Life morphology progression is deferred until the surviving Chip and Strand vocabulary is accepted from the production camera. Fray is not part of that future vocabulary. Any later detached flecks, streak remnants, micro-bubbles, or optical glints require a separate evidence-driven proposal and must not recreate Fray under another name.

### Performance contract

```text
new persistent textures / fields / channels = 0
new compute kernels / dispatches / readbacks = 0
cost location = fragment shader only
wide neighbourhood sampling = rejected by default
```

Reuse the existing shader-detail probe and available samples where practical. Profile before accepting any broad sampling stencil.

### Immediate next steps

1. Complete D.0 Unity import and Fray-off visual-equivalence validation.
2. Run D.1 Chipping Readability Audit from the production camera.
3. Measure projected candidate size, visible removal, edge/interior authority, sub-readable candidates, and Strand occlusion in one compact record.
4. Design the Chipping rework from that evidence; do not resume Fray or Remaining-Life morphology work first.

## Active and trusted foundations

Trusted foundations:

```text
Persistent Foam State stores Presence, Remaining Life, and Material Pattern.
Manual/source birth creates durable material.
Conservative local 2D donor-cell transport moves durable material through the canonical velocity field.
Lifecycle aging and valid-fluid clipping remain Layer C-owned.
Topology/support/negative aging influences Layer C where implemented.
Motion Lane and Obstacle Routing remain raw Layer B inputs. Their canonical resolved physical velocity drives Layer C FoamState transport and Layer D temporal occupancy transport. Production Final Foam no longer reconstructs this velocity for residual presentation.
_FoamShapeMask now combines committed Presence with the advected Layer D temporal occupancy product in diagnostics.
Foam Evaluated Shape debug can display _FoamShapeMask.
Foam Shape Difference debug compares _FoamShapeMask against raw persistent Presence.
Final Foam consumes committed Layer C state directly and still does not consume _FoamShapeMask or temporal occupancy.
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
shader-side wide-neighbour structural foam search;
5.16D–5.16D.2 occupancy-native automated macro breakup;
a separate persistent visual-damage field or packed damage channel;
manual macro-tear proof tooling.
```

## Rejected `5.16D–5.16D.2` experiment summary

The experiment deliberately added no persistent memory. It reused temporal occupancy to calculate strain/weakness erosion, inferred slow-healing cuts, local Material Presence suppression, and Selector/Drive/Cut Evidence diagnostics. D3D11 also exposed and corrected a shared `float2`/`float4` zero-literal regression during the test series.

Final Unity evidence:

```text
no clear connected macro tear;
no persistent neck opening;
no convincing sheet split;
high Strength mostly removed dim support film;
visible lane changes came from existing advection rather than relocation by breakup;
evaluated-final preview showed a coordinate-mismatch expiry illusion.
```

The entire experiment is removed in `5.16D.R`. It is historical evidence, not a dormant feature to retune.

# Superseded implementation and blocker history

The sections below preserve implementation history only. They are not the active queue and must not override the `5.16D.R` retirement state or the `5.16E` visibility/precision validation gate above.

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

# Superseded blocker history

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


# Superseded patch-order history

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



---

# `4.11C.5.16A` implementation record

Implemented files and responsibilities:

```text
StylizedRiver.cs
  physical velocity controls, one-time legacy tuning migration, new public contract values

StylizedRiverFoamRuntime.RuntimeUpdates.cs
  single CPU authority for base signed/unsigned Foam downstream speed

StylizedRiverFoamRuntime.Obstacles.cs
  physical lane-phase advection independent of total river length

StylizedRiverFoamRuntime.Compute.cs / CS_RiverFoam.Resources.hlsl
  shared velocity scalar binding for future compute consumers

RiverWaterFoamVelocity.hlsl
  canonical pure resolved-velocity math shared by compute and shader debug

CS_RiverFoam.Motion.hlsl
  raw lane/obstacle sampling plus canonical resolver call

SH_CleanStylizedRiver.shader
  existing Motion Field views display resolved downstream/lateral velocity, slowdown, and obstacle influence
```

Current limitation intentionally retained:

```text
CommitPhaseTransport still performs one global downstream column shift and preserves Y.
No compute kernel currently changes FoamState according to local resolved velocity.
```

# `4.11C.5.16A.1` implementation record

Status: implemented; Unity compile/import and visual validation required.

Changed responsibilities:

```text
StylizedRiver.cs
  preserves serialized foamMotionFieldLaneScale but changes its authored meaning to Direction Change Frequency;
  adds foamMotionFieldAcrossRiverCoherence with a 0.5–4.0 range and default 1.0;
  exposes independent runtime properties and clamps both controls.

StylizedRiverEditor.cs
  removes both calls to the transient DrawFoamTransportWarnings helper and removes the helper;
  retains fixed-height Motion, Next Debug Section, Material Tick, and Status rows;
  exposes Direction Change Frequency and Across-River Coherence separately.

StylizedRiverFoamRuntime.Obstacles.cs
  separates downstream and lateral noise-frequency scales across warps, all main octaves, breakers, and cross-cuts;
  keeps SmoothMotionLaneAcrossWidth unchanged;
  increments the lane algorithm signature from 2 to 3 and hashes both controls;
  does not alter ResolveObstacleRoutingFieldSignature.

SH_CleanStylizedRiver.shader
  composes neutral/lateral/obstacle hue first, then multiplies by downstream-speed brightness;
  prevents obstacle yellow from independently re-brightening slowed or stagnant regions.
```

Resource and ownership proof:

```text
no new texture;
no new buffer;
no new kernel;
no new dispatch;
no material-state write;
no spawning change;
no lifetime-rule change.
```


# `4.11C.5.16B` implementation record

Status: implemented in source; Unity import/compile and visual validation pending.

Changed movement path:

```text
removed CPU global phase accumulation and integer column commit;
removed CommitPhaseTransport compute authority;
SimulateFoam now evaluates shared longitudinal/lateral donor-cell face fluxes;
packed material moments move together;
lifecycle samples the current cell directly and ages by substep delta;
automatic and queued births target the completed transported texture;
shader residual movement backtraces through canonical local velocity in X and Y.
```

Numerical and diagnostic path:

```text
8 / 12 / 16 Hz material cadence retained;
CFL target 0.90;
maximum 64 substeps;
unsafe step is retained and reported, not velocity-clamped;
compact 12-counter fixed-point GPU metrics buffer;
asynchronous Presence/life/pattern accounting readback;
0.25% unaccounted-error gate;
0.10% Presence clamp-loss gate.
```

Explicitly unchanged:

```text
RiverWaterFoamVelocity.hlsl canonical semantics;
CS_RiverFoam.Motion.hlsl raw field sampling contract;
source grammars and authored defaults;
lifetime-control meanings;
Layer D formulas and ownership;
Ground and Generated Mass systems;
full-resolution Foam texture count.
```
