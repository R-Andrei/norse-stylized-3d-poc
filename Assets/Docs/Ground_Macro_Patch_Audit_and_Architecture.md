# Ground Broad Macro Patch Audit and Architecture

## Status — 2026-07-15

**V3M Broad Macro Patch Completion is the active Ground milestone.**

Painted Accent production is complete and accepted. Ground as a whole is not complete. V4 Contact / Edge Accents remains architecturally accepted in `Ground_Contact_Edge_Accent_Audit_and_Architecture.md`, but implementation is queued until broad macro composition passes gameplay-camera visual acceptance.

V3M-A0 diagnostic evidence is captured and confirms the audit: the generated tonal mask is active but dominated by an extremely broad soft gradient, the old shader macro source is generic low-frequency noise, and its true weighted tonal influence remains weak even when displayed at `20×` gain.

V3M-A1 Unity evidence confirmed that the replacement evaluator is active and that its shaped signed regions contain genuine neutral space. V3M-A1.1 then made the macro contribution genuinely visible in the normal render, but gameplay-camera evidence showed that the fixed transitions were too hard against the calm base terrain. V3M-A1.2 is implemented and awaits Unity 6000.5.0f1 validation; it exposes authorable intensity and transition softness without changing region morphology, generated geometry, collision, profiles, styles, materials, scenes, prefabs, Painted Accent production, or runtime asset lifecycle.

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

### Pre-V3M-A1 shader broad source

Before V3M-A1, `PixelSurfaceGroundForwardPass.hlsl` independently sampled world-space value noise using `_GroundMacroPatchScale` and `_PixelSeed`. That source had no occupancy threshold, edge softness, calm-space control, or deliberate regional shaping.

The clean variants gave it only approximately one to two percent theoretical full-range tonal authority after the fine-pixel multipliers. Patchier variants were stronger, but still used the same undifferentiated value-noise morphology. V3M-A0 screenshots confirmed a single broad light/dark lobe rather than several deliberate regions.

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

V3M-A1 implements one explicit shader-side macro-region evaluator that replaces the old raw broad sample rather than stacking another procedural layer:

```text
world-space XZ regional source
→ two-axis low-frequency domain warp
→ primary region plus restrained secondary breakup
→ dark transition
→ exact neutral middle band
→ light transition
```

The world-space XZ contract preserves continuity across adjacent Ground objects and prevents vertical position from changing the region layout. CPU-generated fields retain semantic ownership and may bias macro appearance; they are not the sole visible macro authority.

The same evaluator also supplies the existing fine pixel-cell warp, keeping the shader at four value-noise evaluations for this combined work rather than adding another procedural stack. A persistent macro texture remains deferred unless measured shader quality or cost proves inadequate.

## V3M-A0 — Slim diagnostic proof

V3M-A0 intentionally avoids diagnostic sprawl. It relabels one existing view and adds only two macro-specific views. The existing Ground Semantic Combined view remains unchanged.

The renderer-local Ground debug dropdown now exposes these three macro-audit views:

```text
Ground Generated Tonal Mask
    Existing value 7, renamed to identify the CPU/generated source.

Ground Macro Raw Shader Field
    New value 30. Shows the unweighted shader broad-noise source.

Ground Macro Weighted Tonal Influence
    New value 31. Shows the signed production macro tonal contribution
    after the active Broad Variation amplitude mapping.
```

V3M-A0 initially used a fixed `20×` display gain so one-percent-scale influence was visible. V3M-A1.1 reduces this to `5×` after increasing production visibility, keeping the diagnostic readable without grossly overstating the final result. Grey is neutral, blue is negative, and orange is positive. The gain affects debug display only and never normal rendering.

No additional diagnostic panel, serialized telemetry, shared material-debug enum change, or per-frame production work is added.

Production and diagnostics now call the same Ground-only region evaluator and amplitude resolver, preventing the weighted view from drifting away from the normal-render formula.

## V3M-A0 validation gate

1. Unity and shader compilation complete without errors or warnings.
2. Normal lit Snowfield and Grassland output is unchanged when Debug View is `None`.
3. `Ground Generated Tonal Mask`, `Ground Macro Raw Shader Field`, and `Ground Macro Weighted Tonal Influence` all render from the same gameplay camera.
4. Debug-view changes remain Material-only and do not regenerate geometry, mesh application, or collider state.
5. Snowfield Clean, Snowfield Patchy, Grassland Clean, and Grassland Patchy evidence confirms whether the raw field exists but the true weighted influence remains weak.

## V3M-A1 — Shaped macro-region proof

V3M-A1 implements the approved replacement field in the Ground-only `PixelSurfaceGroundMacro.hlsl` include.

The fixed proof morphology is:

```text
macro coordinate                 world XZ / Ground Macro Patch Scale
warp frequency                   0.43 × primary coordinate
warp amount                      0.52 macro cells
primary / secondary weighting    0.86 / 0.14
secondary frequency              1.65 × primary
dark transition                  source 0.22 → 0.34
neutral band                     source 0.34 → 0.66
light transition                 source 0.66 → 0.78
```

The output is a signed region field from `-1` through an exact zero middle band to `+1`. `Ground Macro Patch Scale` remains the physical scale authority and `Broad Variation` now directly controls macro tonal amplitude. The macro contribution is no longer multiplied by `Pixel Effect Strength` or resolved profile pixel contrast; those remain fine pixel/generated-tonal controls.

The signed region also replaces the old broad value in `semanticPatch` and monolithic relief. The two-axis macro warp is reused for fine pixel-cell displacement, replacing the old three-axis warp. Total value-noise evaluation count for macro plus pixel warp remains four.

No new controls, debug views, serialized fields, generated assets, or family-specific colour treatment are introduced. The existing raw and weighted debug views call the same production evaluator.

## V3M-A1 validation gate

1. Unity and shader compilation complete without errors or warnings.
2. The raw macro view shows several irregular regions with substantial mid-grey neutral space rather than one map-wide gradient.
3. The weighted view shows separated blue and orange regions over a genuinely grey calm field.
4. Normal Snowfield and Grassland output becomes visibly flatter when Broad Variation is zero, without turning into camouflage when restored.
5. Camera movement and adjoining Ground coverage show no swimming, seam, or grid-alignment artifact.

Unity outcome:

- the shaped field and neutral occupancy passed in the weighted diagnostic;
- the generated tonal mask correctly remained unchanged because it is a separate CPU field;
- normal-render visibility failed at authored Snowfield Clean strength;
- morphology remains provisional until it can be judged in a readable normal render.

## V3M-A1.1 — Bounded visibility calibration

V3M-A1.1 keeps the accepted A1 evaluator, thresholds, coordinate contract, value-noise count, and semantic consumers unchanged. It changes only the conversion from `Broad Variation` to production tonal amplitude:

```hlsl
macroAmplitude = min(max(0.0, BroadVariation) * 2.5, 0.12);
macroTonalOffset = signedRegion * macroAmplitude;
```

This preserves zero as a complete disable, promotes authored Clean values into gameplay-readable territory, and caps stronger variants before they can become extreme. Approximate full-region limits at the current authored values are:

```text
Snowfield Clean    ±6.5%
Grassland Clean    ±9.0%
Patchier variants  capped at ±12% where applicable
```

The weighted diagnostic uses the exact same amplitude resolver and reduces its display-only gain from `20×` to `5×`. No new control, debug view, field sample, generated texture, family colour response, or asset retune is introduced.

## V3M-A1.1 validation gate

1. Unity and shader compilation complete without errors or warnings.
2. Broad Variation at zero visibly removes the independent macro contribution.
3. Restoring the authored Clean value produces readable broad composition from the gameplay camera without obvious staining.
4. Patchy variants are clearly stronger but remain below the `12%` cap and do not resemble camouflage.
5. The existing raw field is unchanged, while the weighted view remains readable at `5×` and matches the production sign and occupancy.

## V3M-A1.2 — Authorable intensity and transition softness

V3M-A1.2 keeps the accepted world-space region source, warp, occupancy, scale authority, semantic consumers, and four-noise evaluation budget. It exposes the two controls required by gameplay-camera validation:

- **Macro Patch Intensity** reuses the existing serialized `broadVariation` value for compatibility. The Ground shader resolves it through a linear `3.0x` calibration, raising the V3M-A1.1 result by 20% without an early saturation cap.
- **Macro Patch Transition Softness** controls only the transition width from full light/dark regions into the preserved neutral band. `0` gives narrow transitions, `1` gives broad seamless blending, and the new default is `0.75`.

The neutral source interval remains fixed at `0.36–0.64`. Softness maps each side's transition width from `0.06` to `0.24`, so increasing softness broadens the blend without making the entire terrain active. No new morphology, seed, scale, debug view, generated asset, scene, prefab, material, style, or profile authority is introduced.

## V3M-A1.2 validation gate

1. Unity and shader compilation complete without errors or warnings.
2. Macro Patch Transition Softness visibly widens and narrows region transitions while preserving neutral terrain.
3. The default `0.75` transition reads more seamlessly than V3M-A1.1 from the gameplay camera.
4. Macro Patch Intensity remains a monotonic zero-to-strong control and the slightly raised authored value remains readable after softening.
5. Raw and weighted debug views continue to match the production field without new views or regeneration work.

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
- Pre-A1 shader broad value: cheap and active, but generic and weak; retired by V3M-A1.
- Patchier variants: useful diagnostic calibration lanes, not a morphology solution.
- V3M-A1 shaped field: morphology/occupancy diagnostic passed, but authored-strength normal-render visibility failed.
- V3M-A1.1 bounded amplitude calibration: made the field visible, but fixed edges were too hard.
- V3M-A1.2 authorable intensity and transition softness: implemented; Unity visual acceptance pending.

### Rejected as final fixes

- Increasing all variation/noise controls without changing region morphology.
- Counting River/shore response as independent macro composition.
- Using Painted Accent density as macro composition.
- Promoting the current CPU field to sole production authority.

### Deferred

- Persistent generated macro texture, pending measured evidence that the shader route is inadequate.

## Next work items

1. Unity-validate V3M-A1.2 in Snowfield and Grassland from the same gameplay camera.
2. Tune only Macro Patch Intensity and Macro Patch Transition Softness until the blend is readable but seamless.
3. Decide whether the accepted region morphology needs any further shape refinement after the new controls are evaluated.
4. Resume V4 only after macro composition passes gameplay-camera acceptance.
