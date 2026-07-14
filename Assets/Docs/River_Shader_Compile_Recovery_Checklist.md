# River Shader Compile Recovery Progress Log and Checklist

> **D.0 cross-impact note — 2026-07-14**
>
> Final-Edge Fray retirement edits `RiverWaterFoam.hlsl` but preserves the exact adaptive `[loop]` Chip candidate bounds and iteration body introduced by Patch 1. D.0 removes the Fray helper, procedural fields, and dead transient carriers without adding replacement shader work. Unity must record a fresh cold compile after shader invalidation; any regression must be compared against the rolled-loop baseline rather than the former unrolled implementation.

## Canonical log policy

This document is the active progress ledger for recovering StylizedRiver shader iteration performance. It owns the accepted findings, methods tried, patch status, validation evidence, and immediate next action.

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
- [ ] Unity validation must still confirm that all river debug modes and production Foam paths compile and render correctly.

## Methods ledger

| Method | Status | Reason |
|---|---|---|
| Mechanically reduce fixed search bounds | Rejected for Patch 1 | Could clip candidates at accepted maximum lateral motion, size, pulse, stabilization, jitter, or morph reach. |
| Remove `RiverWaterFoamEvaluateSelectionDiagnostics` from production | Rejected | The function also owns production Chip selection. |
| Prewarm current variants | Rejected | Would force the pathological programs earlier rather than reduce their cost. |
| Strip URP variants first | Deferred | A single required variant must become cheap before multiplier reduction is useful; stripping also needs a complete renderer/quality/build audit. |
| Prepared compute/texture/buffer candidate field | Deferred durable option | High architecture cost and current view-dependent derivative behaviour require a separate design. |
| Separate production and diagnostic shader responsibilities | Approved follow-up candidate | Valuable after Patch 1 measurement, but larger than the minimum causal experiment. |
| Exact adaptive rolled loops | **Implemented; awaiting Unity validation** | Preserves the complete reach contract while exposing one candidate body instead of 55 unrolled copies. |

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

The patch is not accepted until Unity evidence addresses those risks.

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

### C. Production visual equivalence

- [ ] Compare normal Final Foam before and after at current serialized settings.
- [ ] Validate Chipping with Activation `0` and `1`.
- [ ] Validate minimum `3 x 3`, representative `3 x 5`, and maximum `5 x 11` reach configurations.
- [ ] At maxima, test Candidate Radius, Maximum View Scale, Size Irregularity, Size Pulse, Shape Change, Distribution Irregularity, and Lateral Motion together.
- [ ] Confirm no missing candidates, clipping, popping, pink output, or changed lifecycle/motion identity.

### D. Diagnostic equivalence

- [ ] Validate surviving Layer E debug views `18–21` and `24–26`; retired values `22`, `23`, and `27` must resolve safely to Final.
- [ ] Confirm Candidate Field, Activated Candidates, Edge Eligibility, Interior Authority, Potential Eligibility, Final Selection, and Chip-removed output remain coherent.
- [ ] Confirm surviving Strand output is unchanged outside the production boundary effects caused by Chipping.

### E. Runtime GPU guard

- [ ] Compare representative river GPU cost before and after at current settings.
- [ ] Compare maximum `5 x 11` settings before and after.
- [ ] Reject or redesign the patch if Editor compile recovery creates a severe sustained GPU regression.

## Acceptance criteria

Patch 1 is accepted only when all of the following are true:

- [ ] D3D11 compiles without shader errors.
- [ ] Cold Scene, Play, and Inspector contexts are materially faster than the captured multi-minute baseline.
- [ ] Warm reuse does not repeat cold shader work without a source/configuration change.
- [ ] Current and extreme Chip visuals remain equivalent within normal rasterization tolerance.
- [ ] Diagnostic views remain correct.
- [ ] Runtime GPU cost remains acceptable.

## Local validation record

- [x] Clang 17 HLSL library-target parsing and LLVM code generation completed for the full changed include through a validation shim.
- [x] The compiler accepted the new `[loop]` attributes and runtime integer loop limits.
- [x] No HLSL parse or semantic errors were reported.
- [x] The only local warnings concerned existing `[branch]` attributes unsupported by this Clang build; they are unrelated to Patch 1.
- [x] Exhaustive offset-set equivalence was checked for every legal integer pair: downstream offsets `1..2`, lateral offsets `1..5`.
- [x] Changed-file line endings remain LF and the include remains ASCII.

This is not a Unity ShaderLab, URP-variant, D3D11-driver, or in-project compile. Unity validation remains mandatory before acceptance.

## D.0 cross-patch preservation note

D.0 removes Final-Edge Fray and its dead transient carriers from the same include. It must preserve the adaptive `[loop]` Chip candidate search, its runtime bounds, and its maximum `5 × 11` reach byte-for-byte. The retirement removes procedural work and adds no texture, buffer, dispatch, shader property, candidate iteration, or replacement morphology. Because any include edit invalidates dependent shader variants, record a fresh cold compile timing after D.0 and compare it with the rolled-loop recovery baseline before attributing regressions.

## Current status

```text
Source analysis: complete
Patch 1 implementation: complete
Local HLSL parser/code-generation validation: complete
Unity D3D11 validation: pending
Cold timing validation: pending
Visual equivalence validation: pending
Runtime GPU comparison: pending
```

## Next decision after validation

1. If Patch 1 removes the multi-minute stalls and runtime cost remains acceptable, accept it as the immediate recovery baseline.
2. If compile time remains excessive, isolate production Chip selection from diagnostic-only outputs next.
3. If the rolled loop causes unacceptable GPU cost, design a coarse prepared candidate index/field rather than reducing accepted visual reach blindly.
4. Only after one required variant is cheap, audit and constrain the actual URP shader variant surface.
