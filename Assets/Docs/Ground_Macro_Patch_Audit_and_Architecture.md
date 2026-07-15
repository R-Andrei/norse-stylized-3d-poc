# Ground Broad Macro Patch Audit and Architecture

## Status — 2026-07-15

**V3M Broad Macro Patch Completion is the active Ground milestone.**

Painted Accent production is complete and accepted. Ground as a whole is not complete. V4 Contact / Edge Accents remains architecturally accepted in `Ground_Contact_Edge_Accent_Audit_and_Architecture.md`, but implementation is queued until broad macro composition passes gameplay-camera visual acceptance.

V3M-A0 is implemented in source and awaits Unity 6000.5.0f1 validation. It changes documentation and slim renderer-local diagnostics only. It does not change normal lit output, generated geometry, collision, profiles, styles, materials, scenes, prefabs, Painted Accent production, or runtime asset lifecycle.

## Mission

The authoritative static Ground stack remains:

```text
playable terrain shape
→ calm family/variant base material
→ broad macro patch composition
→ semantic surface-mask response
→ Painted Accent lines
→ Contact / Edge Accents
→ sparse motifs and stamps
→ runtime surface state later
```

Broad macro composition must create several restrained, independent, gameplay-readable regions across a normal Ground area. River-following tint, local shadows, thin Painted Accent marks, and mathematically changing but visually imperceptible colour do not satisfy this layer.

Semantic region masks and visual macro composition are separate responsibilities. Exposure, damp/deposit, vegetation, compaction, shore, rocky/dry, and standing-water fields may bias macro appearance, but they do not replace an independent macro source.

## Proven current implementation

The current Ground result is driven by two separately authored broad sources and one downstream semantic composite.

### CPU-generated tonal source

`GroundGenerator.BuildSurfaceMetadata()` writes tonal variation into vertex colour R. Its most region-like helper is `EvaluateBroadSurfacePatch()`, which includes domain warp, two regional frequencies, posterization, and edge softness.

The shaped field is nevertheless a minority contributor:

```csharp
float tonalField =
    tonalPatch * 0.68f +
    broadPatch * 0.20f +
    tonalDetail * 0.12f;

surfaceVariations[index] =
    Mathf.Lerp(compatibleVariation, profiledVariation, 0.35f);
```

The direct `broadPatch` authority is therefore only `0.20 × 0.35 = 0.07` before Surface Variation and Patch Contrast attenuation. The active 40 m / 33-vertex Ground also samples CPU metadata at 1.25 m intervals, which is adequate for broad gradients but limits irregular smaller boundaries.

### Shader broad source

`PixelSurfaceGroundForwardPass.hlsl` independently samples world-space value noise using `_GroundMacroPatchScale` and `_PixelSeed`. This source is not shaped by occupancy, threshold, posterization, edge softness, calm-space control, or the CPU Ground seed and patch coordinate.

The current clean variants give it only approximately one to two percent theoretical full-range tonal authority after actual multipliers. Patchier variants are stronger, but still use the same undifferentiated value-noise morphology.

### Downstream semantic composite

The shader combines the CPU tonal mask and shader broad value into `semanticPatch`, then uses that composite only through restrained family-local responses. Similar target colours and low blend coefficients suppress the remaining visual separation.

## Root cause

```text
two separately authored broad sources
+ no single final macro-composition authority
+ generic shader morphology
+ diluted CPU morphology
+ weak final visual amplitude
+ overlapping control names
= active but gameplay-invisible macro composition
```

The blocker is not missing data or a broken material-property assignment. The generated tonal mask and shader field both exist. Their final visual authority and composition contract are insufficient.

## Accepted architecture direction

The preferred production candidate is one explicit shader-side macro-region evaluator that replaces the current raw broad sample rather than stacking another procedural layer.

The evaluator should eventually provide:

```text
2D XZ regional source
→ controlled domain warp
→ primary and secondary regional scales
→ occupancy/contrast shaping
→ edge softness
→ restrained breakup
```

The coordinate contract must preserve continuity across adjacent Ground objects. World-space XZ is the default recommendation unless patch-continuous local coordinates can guarantee the same continuity.

CPU-generated fields retain semantic ownership and may bias macro appearance. They should not remain the sole visible macro authority because their current contribution is diluted, mesh-resolution limited, and coupled to Ground regeneration signatures.

A persistent macro texture is deferred. It should be considered only if measured shader quality or cost proves inadequate.

## V3M-A0 — Slim diagnostic proof

V3M-A0 intentionally avoids diagnostic sprawl. It relabels one existing view and adds only two macro-specific views. The existing Ground Semantic Combined view remains unchanged.

The renderer-local Ground debug dropdown now exposes these three macro-audit views:

```text
Ground Generated Tonal Mask
    Existing value 7, renamed to identify the CPU/generated source.

Ground Macro Raw Shader Field
    New value 30. Shows the unweighted shader broad-noise source.

Ground Macro Weighted Tonal Influence
    New value 31. Shows the signed broad-noise contribution after
    Broad Variation, resolved profile pixel contrast, and Pixel Effect Strength.
```

The weighted view uses a fixed `20×` display gain so one-percent-scale influence is visible. Grey is neutral, blue is negative, and orange is positive. The gain affects debug display only and never normal rendering.

No additional diagnostic panel, serialized telemetry, shared material-debug enum change, or per-frame production work is added.

The Ground-only debug include mirrors the current normal-render macro formula so the production forward pass and shared Ground response include remain untouched. V3M-A1 must update the diagnostic mirror when it replaces the production field.

## V3M-A0 validation gate

1. Unity and shader compilation complete without errors or warnings.
2. Normal lit Snowfield and Grassland output is unchanged when Debug View is `None`.
3. `Ground Generated Tonal Mask`, `Ground Macro Raw Shader Field`, and `Ground Macro Weighted Tonal Influence` all render from the same gameplay camera.
4. Debug-view changes remain Material-only and do not regenerate geometry, mesh application, or collider state.
5. Snowfield Clean, Snowfield Patchy, Grassland Clean, and Grassland Patchy evidence confirms whether the raw field exists but the true weighted influence remains weak.

## V3M-A1 boundary

After A0 evidence is accepted:

1. replace the current raw shader broad sample with one shaped 2D macro-region evaluator;
2. do not stack a second macro field;
3. reuse or replace existing warp work so noise cost remains near the current shader budget;
4. establish gameplay-readable value separation independently from fine pixel variation;
5. keep style/profile asset tuning outside the first morphology proof;
6. discuss and approve the exact field shape before implementation.

## Acceptance criteria

Macro composition is accepted only when:

- disabling it makes the Ground visibly flatter from the gameplay camera;
- several broad independent regions read across a 40 m Ground;
- regions retain meaningful calm space and controlled irregular silhouettes;
- the result does not resemble camouflage, stains, or equal-activity Perlin noise;
- Snowfield and Grassland both read clearly;
- regions do not merely follow Rivers, modifiers, or semantic masks;
- Painted Accent lines remain a separate smaller-scale layer;
- there is no obvious tiling, seam, grid alignment, shimmer, or inappropriate runtime cost.

## Methods-tried ledger

### Accepted and retained

- Calm family/variant base materials.
- Existing semantic masks and their family-local responses.
- Painted Accent lines as a separate completed layer.
- V4 Contact / Edge Accent architecture, queued after V3M.
- Shader-side replacement as the preferred first production candidate.

### Partially useful

- `EvaluateBroadSurfacePatch()`: useful morphology, insufficient final authority and resolution.
- Current shader broad value: cheap and active, but generic and weak.
- Patchier variants: useful diagnostic calibration lanes, not a morphology solution.

### Rejected as final fixes

- Increasing all variation/noise controls without changing region morphology.
- Counting River/shore response as independent macro composition.
- Using Painted Accent density as macro composition.
- Promoting the current CPU field to sole production authority.

### Deferred

- Persistent generated macro texture, pending measured evidence that the shader route is inadequate.

## Next work items

1. Unity-validate V3M-A0 and capture the three slim macro views for Snowfield and Grassland clean/patchy variants.
2. Confirm normal output and regeneration behavior are unchanged.
3. Review the evidence and approve the V3M-A1 field-shaping contract.
4. Resume V4 only after macro composition passes gameplay-camera acceptance.
