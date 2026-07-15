# Current shader-iteration status after accepted D.1C

D.1C imported, rendered, and passed the user’s functional Unity validation while preserving the adaptive rolled-loop `3×3` through `5×11` search. Chipping is accepted and no longer blocked on shader functionality.

No dedicated cold-compile timing or final GPU comparison was supplied for D.1C. Those measurements remain deferred performance evidence, not a blocker for the accepted visual baseline. This checklist should only become active again for the comprehensive River performance pass or a new shader-iteration regression.

## `4.11C.5.18D` compute/editor iteration impact

`5.18D` is a behaviour-preserving C# authoring/documentation closure. It does not modify any compute shader, include, fragment shader, kernel, dispatch, or shader resource. C# changes are confined to Inspector labels/tooltips, two compact read-only rows in the existing Automatic Birth Sources view, and public diagnostic accessors over already resident CPU data.

```text
shader/compute files changed = 0;
new shader variants/kernels/dispatches/resources = 0;
production candidate/source arithmetic = unchanged;
serialized source values = unchanged.
```

Required evidence is normal Unity C# import plus confirmation that equivalent settings preserve the accepted `5.18C` result. This patch does not reopen the Chipping compile-cost investigation.

## `4.11C.5.18C` compute/shader iteration impact

`5.18C` changes two compute programs and the River C# dispatch/authoring path; it does not modify the production fragment shader or the accepted Chipping candidate loops.

```text
CS_RiverFoam.compute
  cumulative debug clear/history removed;
  current-update overlap encoding retained in the existing debug texture;
  object contact search reduced from 24 neighbours to 8;
  Shore Ribbon normal thickness uses cross-river spacing only.

CS_RiverDisturbance.compute
  Static Pressure profile-count reach scaling removed;
  explicit 0.50-texel minimum raster floor;
  authored total Front Reach remains constant while crest/falloff shape varies.

resources
  new textures/channels/buffers/dispatches = 0;
  debug counters = 2 → 1;
  normal automatic-source kernel still performs no debug UAV write.
```

`5.18C` passed its Unity source/pressure/final-render validation gate and is accepted. Its retained compile evidence remains useful if a later regression appears. This patch does not reopen the accepted Chipping shader compile investigation or the deferred comprehensive River performance pass. If compute import fails, recover by checking the removed `ClearAutomaticBirthDebugTransient` kernel lookup/pragma, the one-element counter contract, and CPU/GPU `FoamSourceEventGpuData.shore.y` semantics before changing production behaviour.

## Canonical log policy

This document is the retained evidence ledger for StylizedRiver shader iteration performance. It owns the accepted findings, methods tried, patch status, and validation evidence. It has no immediate patch action and reopens only during the comprehensive River performance pass or a new regression.

`River_Editor_Loading_Performance_Full_Diagnosis.md` remains the authoritative captured incident report. `Ground_River_Regeneration_Orchestration_Manual.md` remains authoritative for the separate Ground/River ownership redesign. This checklist does not replace either document.

## Scope

This work concerns Unity Editor iteration after river shader/include changes:

- first Scene-view rendering;
- entering Play mode;
- selecting StylizedRiver with an Inspector preview context;
- warm-cache reuse.

It does not treat runtime FPS, monolithic C# compilation, Ground/River regeneration ownership, or Foam cache signature mismatches as the same problem.

## Accepted diagnosis

### Proven source facts

- [x] `RiverWaterFoamEvaluateSelectionDiagnostics` is called by the normal river fragment path.
- [x] The function owns production `chipProductionSelection` as well as diagnostic outputs; the complete function cannot simply be removed without deleting production Chipping.
- [x] The previous implementation explicitly unrolled a fixed `5 x 11` candidate rectangle: 55 compiler-visible candidate bodies per fragment variant.
- [x] Runtime `requiredDownstreamOffset` and `requiredLateralOffset` branches skipped unnecessary candidates while executing, but did not make the fixed unrolled body smaller during shader compilation.
- [x] The maximum downstream offset is forced by geometric radius reach plus cell-centre jitter.
- [x] The maximum lateral offset additionally includes up to 2.5 candidate spacings of rigid lateral travel.
- [x] The selected include is consumed only by `SH_CleanStylizedRiver.shader` in the supplied project snapshot.
- [x] The owning shader is resolved by `StylizedRiver.cs`; no unrelated shader includes the changed file.

### Exact reach contract

The search bounds remain:

```text
maximumRadiusScale = size variation ceiling x pulse ceiling
maximumShapeReachScale = bounded multi-axis morph reach
maximumRadiusReach = stabilized radius ratio x radius scale x shape reach
cellCentreReach = half-cell + authored deterministic centre jitter
requiredDownstreamOffset = floor(maximumRadiusReach + cellCentreReach), clamped 1..2
requiredLateralOffset = floor(maximumRadiusReach + lateral travel + cellCentreReach), clamped 1..5
```

At global authoring maxima, the required rectangle remains `5 x 11`. The accepted patch does not reduce that visual envelope.

### Causal conclusion

The primary source-level compile regression is the combination of:

1. a large procedural candidate body;
2. 55 explicit unrolled copies of that body;
3. repeated compilation under multiple Scene, Play, and Inspector/preview shader contexts.

Confidence that the fixed unrolled search is the dominant per-variant compile regression: **95%**.

## Cross-subsystem impact audit

- [x] Changed include: `Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl`.
- [x] Direct include consumer: `Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader` only.
- [x] Runtime owner: `Assets/Game/Procedural/Rivers/StylizedRiver.cs` resolves the river shader resource.
- [x] No Ground, Generated Mass, vegetation, generic PixelSurface, or unrelated water shader directly includes the changed file.
- [x] No shader properties, material serialization, textures, buffers, components, tags, layers, assets, or render-pass declarations are changed by Patch 1.
- [x] Unity import and production Chipping render validation passed through D.1C. Exhaustive timing/debug-mode measurement remains deferred performance evidence.

## Methods ledger

| Method | Status | Reason |
|---|---|---|
| Mechanically reduce fixed search bounds | Rejected for Patch 1 | Could clip candidates at accepted maximum lateral motion, size, pulse, stabilization, jitter, or morph reach. |
| Remove `RiverWaterFoamEvaluateSelectionDiagnostics` from production | Rejected | The function also owns production Chip selection. |
| Prewarm current variants | Rejected | Would force the pathological programs earlier rather than reduce their cost. |
| Strip URP variants first | Deferred | A single required variant must become cheap before multiplier reduction is useful; stripping also needs a complete renderer/quality/build audit. |
| Prepared compute/texture/buffer candidate field | Deferred durable option | High architecture cost and current view-dependent derivative behaviour require a separate design. |
| Separate production and diagnostic shader responsibilities | Approved follow-up candidate | Valuable after Patch 1 measurement, but larger than the minimum causal experiment. |
| Exact adaptive rolled loops | **Functionally accepted; dedicated cold-timing measurement deferred** | Preserves the complete reach contract while exposing one candidate body instead of 55 unrolled copies. |
| Presence-isovalue `Chip Edge Coverage` | **Rejected and removed by D.1A** | It selected scalar coverage, not spatial edge distance, producing non-uniform territory. |
| Derivative-normalized existing `softVisibility` | **Accepted with deferred thin-strip limitation** | Zero-resource local pixel-coordinate approximation; D.1A.1 persistent-carrier alternative was rejected and rolled back. |
| Consolidated Size/Irregularity authoring | **Unity-validated and accepted** | Replaces four uniforms with two while preserving the adaptive search ceiling. |
| Medium/large-biased readable population | **Unity-validated and accepted** | Reuses existing hashes and projected radius; no search or resource expansion. |

## Patch 1 — Exact adaptive rolled candidate search

### Intent

Preserve the exact existing candidate set for every material configuration while changing compiler-visible control flow.

### Implementation checklist

- [x] Keep the existing `requiredDownstreamOffset` calculation unchanged.
- [x] Keep the existing `requiredLateralOffset` calculation unchanged.
- [x] Replace fixed `[unroll]` loops over `-2..2` and `-5..5`.
- [x] Use `[loop]` loops bounded directly by the calculated offsets.
- [x] Remove the redundant runtime `continue` branch used to discard candidates outside the calculated rectangle.
- [x] Preserve candidate iteration order: downstream outer loop, lateral inner loop.
- [x] Preserve candidate hashes, lifecycle, motion, rotation, pulse, morph, anti-aliasing, activation, edge permission, interior permission, diagnostics, and final production selection.
- [x] Add no fields, buffers, textures, kernels, dispatches, properties, keywords, variants, or allocations.
- [x] Preserve LF line endings and source encoding.

### Expected execution rectangles

```text
minimum settings: 3 x 3 = 9 candidate evaluations
current representative settings: approximately 3 x 5 = 15 evaluations
maximum authored reach: 5 x 11 = 55 evaluations
compiler-visible candidate body: one rolled body instead of 55 explicit copies
```

The maximum runtime candidate count is intentionally unchanged. The expected Editor improvement comes from removing static body replication, not from pretending the accepted visual reach is smaller.

### Functionality that could regress

- dynamic-loop compilation on the active D3D11/URP backend;
- fragment GPU cost if the driver handles rolled uniform loops poorly;
- candidate coverage at minimum, current, or maximum bounds if loop limits are compiled incorrectly;
- production Chipping or debug modes 18–27 if candidate iteration changes unexpectedly;
- cold compile time if Unity ignores the loop attribute and still aggressively unrolls the body.

Functional acceptance is complete. These risks require renewed measurement only if the comprehensive performance pass or a new regression reopens shader iteration work.

## Validation checklist

### A. Compilation and cold-context timing

- [ ] Make one deliberate river shader/include invalidation and confirm clean D3D11 compilation.
- [ ] Record first Scene-view river render time.
- [ ] Record Play-entry time and the relevant `Editor.log` timing categories.
- [ ] Record first river Inspector-selection time with the preview context available.
- [ ] Confirm the shader compiler no longer produces repeated multi-minute waits for the tested contexts.

### B. Warm-cache reuse

- [ ] Enter Play again without changing shader source and confirm no equivalent cold compile.
- [ ] Select another object and then StylizedRiver twice; confirm the second river selection is immediate or within the agreed warm budget.
- [ ] Confirm no new river shader-cache artifact appears during an already-warm repeated selection.

### C. Functional Unity validation record

- [x] The river shader imported and rendered through D.1C without a reported shader failure.
- [x] Current production Chipping was visually accepted at the gameplay camera.
- [x] D.1A eligibility, D.1B authoring, and D.1C readable population were accepted as the current baseline.
- [x] The surviving diagnostics are `Chip Candidate Field`, `Chip Eligibility Composite`, and `Production Chip Mask`.

### D. Deferred performance measurements

These are useful for the comprehensive River performance pass but do not block the accepted Chipping baseline:

- [ ] Record deliberate cold Scene-view, Play-entry, and Inspector-preview timings.
- [ ] Record warm-cache reuse timings.
- [ ] Compare representative and maximum-reach runtime GPU cost.
- [ ] Reopen shader-iteration architecture only if those measurements or a new regression justify it.

## Performance acceptance criteria if this work resumes

A future compile/performance patch should preserve all accepted Chipping visuals and diagnostics while materially improving measured iteration or GPU cost. Do not reduce the accepted candidate reach or add prepared resources without new evidence.

## Local validation record

- [x] Clang 17 HLSL library-target parsing and LLVM code generation completed for the full changed include through a validation shim.
- [x] The compiler accepted the new `[loop]` attributes and runtime integer loop limits.
- [x] No HLSL parse or semantic errors were reported.
- [x] The only local warnings concerned existing `[branch]` attributes unsupported by this Clang build; they are unrelated to Patch 1.
- [x] Exhaustive offset-set equivalence was checked for every legal integer pair: downstream offsets `1..2`, lateral offsets `1..5`.
- [x] Changed-file line endings remain LF and the include remains ASCII.

This is not a Unity ShaderLab, URP-variant, D3D11-driver, or in-project compile. This local validation is not a substitute for Unity measurement. Functional Unity acceptance is complete; dedicated timing evidence remains deferred to the comprehensive performance pass.

## D.0 cross-patch preservation note

It must preserve the adaptive `[loop]` Chip candidate search, its runtime bounds, and its maximum `5 × 11` reach byte-for-byte. The retirement removes procedural work and adds no texture, buffer, dispatch, shader property, candidate iteration, or replacement morphology. Because any include edit invalidates dependent shader variants, record a fresh cold compile timing after D.0 and compare it with the rolled-loop recovery baseline before attributing regressions.

## Current status

```text
Source analysis: complete
Rolled-loop implementation: functionally accepted in Unity
D.1A / D.1B / D.1C visual baseline: Unity-validated and accepted
Cold Scene/Play/Inspector timing record: deferred
Final runtime GPU comparison: deferred
Active Chipping blocker: none
```

## Next decision

There is no active compile-recovery or Chipping decision. If shader iteration regresses again, resume this ledger with fresh timings. Otherwise, perform the remaining cold-context and GPU measurements during the comprehensive River performance pass.
