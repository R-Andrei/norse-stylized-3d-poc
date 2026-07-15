# Ground Broad Macro Patch Audit and Architecture

## Status — 2026-07-15

**V3M Broad Macro Patch Completion is the active Ground milestone.**

Painted Accent production is complete and accepted. Ground as a whole is not complete. V4 Contact / Edge Accents remains architecturally accepted in `Ground_Contact_Edge_Accent_Audit_and_Architecture.md`, but implementation is queued until broad macro composition passes gameplay-camera visual acceptance.

V3M-A0 diagnostic evidence is captured and confirms the audit: the generated tonal mask is active but dominated by an extremely broad soft gradient, the old shader macro source is generic low-frequency noise, and its true weighted tonal influence remains weak even when displayed at `20×` gain.

V3M-A1 Unity evidence confirmed that the replacement evaluator is active and that its shaped signed regions contain genuine neutral space. V3M-A1.1 then made the macro contribution genuinely visible in the normal render, V3M-A1.2 exposed authorable intensity and transition softness, and V3M-A1.3 added independent pattern-seed and locally varying average-separation controls. V3M-A1.3.1 stabilized seed occupancy, V3M-A1.3.2 restored meaningful seed diversity through pattern-window translation, and V3M-A1.3.3 replaced unsafe raw-seed shader coordinates with CPU-hashed bounded scroll; Unity accepted the resulting seed precision, pattern variation, controls, occupancy, and final render. V3M-A1.3.4 is implemented and awaits Unity 6000.5.0f1 validation; it removes the remaining frequent circular-island tendency by converting the higher-frequency secondary field from additive threshold authority to contour-only coordinate distortion while retaining the four-noise budget.

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

## V3M-A1.3 — Pattern, separation, and expanded calibration authoring

V3M-A1.3 retains the accepted four-noise world-XZ evaluator and the `3.0x` linear tonal calibration while exposing the remaining authoring controls required by gameplay-camera review:

- **Macro Patch Scale** remains the physical metre scale.
- **Macro Patch Pattern Seed** is an integer independent from the pixel seed. Seed `0` preserves the current underlying sample arrangement; other values offset the two warp samples and the primary/secondary region samples independently, changing positions and silhouettes without additional noise evaluations. Adjacent Grounds should share the same seed when continuity is required.
- **Macro Patch Intensity** now uses a `0.00–0.75` slider. There is no hidden artistic cap after the slider; extreme values may deliberately overdrive or clip the tonal result.
- **Macro Patch Transition Softness** remains `0–1`. The transition width now resolves linearly from `0.06` at zero—preserving the accepted hard-edge endpoint—to `0.35` at one. This replaces the previous `0.06–0.24` range without a squared response or extra morphology sample.
- **Average Patch Separation** is a non-negative float with default `1.0`. It controls the average neutral interval between dark and light regions. Separation is varied locally with the existing warp field rather than applied as a perfectly uniform global threshold.

The local neutral gap is resolved as:

```hlsl
localGap = clamp(
    AveragePatchSeparation * 0.28 + macroWarpX * 0.08,
    0.0,
    0.98);
```

At separation `0`, some locations collapse to no finite neutral gap while others retain up to approximately `0.08`; this permits local dark/light contact without forcing contact everywhere. At separation `1`, the gap varies approximately from `0.20` to `0.36`, preserving the previous `0.28` average. Higher values make patches increasingly sparse. Softness only controls transition width; separation only controls the locally varying neutral interval.

No new debug views, texture assets, CPU mask work, scene/prefab edits, or shader noise evaluations are introduced.

## V3M-A1.3 validation gate

1. Unity and shader compilation complete without errors or warnings.
2. Pattern Seed produces clearly different deterministic layouts; restoring `0` restores the prior underlying arrangement.
3. Average Patch Separation at `0`, `1`, and `2` produces local contact, current-average spacing, and visibly sparser regions respectively, without becoming globally uniform.
4. Transition Softness `0` preserves the narrow endpoint while `1` blends materially farther into the terrain than V3M-A1.2.
5. Macro Patch Intensity remains Material-only and usable across the full `0–0.75` slider range.

## V3M-A1.3.1 — Pattern-seed occupancy stabilization

Unity validation of V3M-A1.3 confirmed that Pattern Seed changed patch arrangement successfully, but some seeds produced far more neutral dead space than others at identical authoring values. Pattern Seed should change positions and silhouettes, not act as an uncontrolled coverage control.

V3M-A1.3.1 changes only the existing evaluator's seed participation and field composition:

- the dominant primary region sample keeps a stable base realization and is reshaped indirectly by the seed-dependent domain warp;
- the secondary seeded sample becomes centred boundary distortion instead of being averaged with the primary field, preventing it from compressing the source range toward neutral;
- local separation variation uses the product of both centred warp channels instead of one warp channel, reducing one-sided whole-Ground spacing bias while retaining regional separation differences.

The resolved source and local gap are:

```hlsl
regionalSource = saturate(
    primaryRegion + (secondaryRegion - 0.5) * 0.14);

localSeparationVariation =
    macroWarpX * macroWarpY * 0.08;

localGap = clamp(
    AveragePatchSeparation * 0.28 +
    localSeparationVariation,
    0.0,
    0.98);
```

Pattern Seed continues to change both warp fields and the secondary contour field, so layouts remain deterministic and visibly distinct. No control values, defaults, debug modes, generated assets, shader properties, or noise-evaluation counts change.

## V3M-A1.3.1 validation gate

1. Unity and shader compilation complete without errors or warnings.
2. Seeds `0`, `1`, `2`, `3`, `7`, `13`, `29`, `67`, and `5727` retain visibly different deterministic layouts at identical authoring values.
3. No tested seed produces dramatically more neutral dead space than the others.
4. Average Patch Separation still produces local contact at low values and progressively more calm terrain at higher values.
5. Scale, Intensity, Transition Softness, debug views, and Material-only invalidation behavior remain unchanged.

## V3M-A1.3.2 — Seed-window pattern translation

Unity validation of V3M-A1.3.1 confirmed the occupancy correction but showed that Pattern Seed retained too much of the same dominant topology. The stable primary realization was only deformed by seeded warp and secondary contours, so major shapes mainly shifted at their boundaries rather than being replaced by a meaningfully different visible arrangement.

V3M-A1.3.2 keeps the stable primary realization and adds a deterministic two-dimensional translation in macro-cell space before all four existing samples are evaluated:

```hlsl
patternScroll =
    frac(PatternSeed * float2(0.381966011, 0.707106781)) *
    4.5;

patternCoordinate =
    worldXZ / MacroPatchScale + patternScroll;
```

Seed `0` resolves to zero translation and preserves the current seed-0 field. Other seeds select a different window spanning up to 4.5 macro cells in each axis. The translated coordinate feeds both the seeded domain warp and the stable primary region source; the existing seeded secondary contour source remains active. Pattern Seed therefore changes the visible field through window selection, warp deformation, and contour breakup instead of only wobbling one dominant arrangement.

The translation is measured in macro cells, so its world distance follows Macro Patch Scale automatically. Grounds using the same Pattern Seed remain continuous in world space. No new property, control, debug mode, generated asset, hash/noise sample, scene, prefab, profile, style, or material edit is introduced. The evaluator remains at four value-noise calls.

A source-level replica over a representative 40 m Ground and the nine established validation seeds selected the 4.5-cell translation range because it materially reduced inter-seed field correlation while retaining a representative active-coverage spread below eight percentage points. This numerical check is supporting evidence only; Unity gameplay-camera validation remains authoritative.

The reported seed-5727 smoothing artifact is explicitly outside this patch. It must be retested in the existing raw field, weighted influence, and normal render after pattern translation before its failure stage is classified.

## V3M-A1.3.2 validation gate

1. Unity and shader compilation complete without errors or warnings.
2. Seeds `0`, `1`, `2`, `3`, `7`, `13`, `29`, `67`, and `5727` produce materially different major patch arrangements, not merely boundary wobble.
3. No tested seed returns to the pre-A1.3.1 extreme dead-space inconsistency.
4. Seed `0` preserves the current seed-0 arrangement and all existing Scale, Intensity, Transition Softness, and Average Patch Separation behavior.
5. Seed `5727` is compared in the raw, weighted, and normal views before any separate smoothing correction is approved.

## V3M-A1.3.3 — Bounded seed-key precision hardening

Unity validation of V3M-A1.3.2 accepted the pattern variation, controls, and final render, but proved a remaining numerical defect: authored Pattern Seed magnitudes around `2000` began producing visible transition stepping, and seeds `22000`, `52000`, and `152000` produced progressively larger rectangular bands directly in the raw macro field. The raw field evidence proves that the failure occurs inside the evaluator rather than final tonal composition.

The cause is the direct use of the authored seed magnitude in spatial noise coordinates. Terms such as `PatternSeed * float3(41.41, 13.13, 31.73)` grow into the millions for large seeds. Single-precision shader coordinates then lose the fractional resolution required by the value-noise `floor`/`frac` interpolation, so the field becomes spatially quantized.

V3M-A1.3.3 keeps the full authored integer seed range but resolves it once on the CPU into a bounded deterministic two-dimensional scroll:

```text
Authored int Pattern Seed
→ stable uint avalanche hash
→ two independent 24-bit unit values
→ bounded 0–4.5 macro-cell scroll
```

Seed `0` resolves to zero scroll and preserves the accepted seed-0 pattern. Nonzero seeds select a different bounded window of the same stable world-space primary, warp, and secondary fields:

```hlsl
patternCoordinate =
    worldXZ / MacroPatchScale +
    GroundMacroPatchSeedScroll.xy;
```

Scroll remains the pattern-selection mechanism that Unity validation already accepted in V3M-A1.3.2. The raw authored seed no longer enters any shader spatial coordinate, warp offset, or secondary contour offset. Pattern translation remains below `4.5` macro cells regardless of whether the authored seed is `2`, `152000`, negative, or near the C# integer limits. Using scroll only also avoids adding bounded contour-offset arithmetic that is unnecessary for the accepted pattern diversity and could widen seed-to-seed occupancy variation.

The scroll is calculated only when GeneratedGround reapplies material properties. Runtime evaluation still uses exactly four value-noise calls, removes the former per-pixel large-seed multiplications, and adds no texture, generated asset, per-frame rebuild, or per-pixel seed hash. Existing Scale, Pattern Seed, Intensity, Transition Softness, Average Patch Separation, diagnostics, and serialized authoring data remain unchanged. Nonzero seeds deterministically select new windows under the hardened mapping; exact historical nonzero-seed layouts are not preserved.

## V3M-A1.3.3 validation gate

1. Unity and shader compilation complete without errors or warnings.
2. Seeds `0`, `1`, `67`, `2000`, `5727`, `22000`, `52000`, `152000`, `2147483647`, and `-2147483648` all produce smoothly interpolated raw and weighted fields.
3. Increasing seed magnitude never causes larger blocks, stair steps, rectangular bands, or degraded transition precision.
4. Seed `0` preserves its accepted arrangement; nonzero seeds retain material pattern diversity and reasonably stable occupancy.
5. Scale, Intensity, Transition Softness, Average Patch Separation, normal rendering, and the four-noise evaluation budget remain otherwise unchanged.

## V3M-A1.3.4 — Contour-only secondary morphology

Unity validation of V3M-A1.3.3 accepted bounded seed precision, seed variation, authoring controls, occupancy, and final rendering, but gameplay and macro-debug inspection exposed frequent small circular light and dark islands across many seeds. The repeated islands originate in the higher-frequency secondary sample directly adding `±0.07` scalar authority to the primary source. Local secondary extrema can therefore cross the light/dark thresholds independently, and smooth value-noise extrema naturally produce rounded isolated components.

V3M-A1.3.4 retains the same primary and secondary samples but changes the secondary field from scalar contribution to coordinate-only contour distortion:

```hlsl
secondaryCentered = secondaryRegion - 0.5;
contourDirection = float2(
    0.34 + warp.y * 0.12,
    -0.26 + warp.x * 0.12);
primaryCoordinate =
    regionCoordinate +
    contourDirection * secondaryCentered;
regionalSource = ValueNoise(primaryCoordinate);
```

The secondary field can still bend, dent, and locally offset primary-region boundaries, but it no longer has independent authority to create a light or dark island inside otherwise neutral terrain. The varying warp contribution avoids one uniform contour direction. This correction does not guarantee that every naturally closed primary region is non-circular; the acceptance target is a substantial reduction in frequent artificial circles, not a topology-expensive zero-circle guarantee.

The patch retains world-XZ continuity, CPU-hashed bounded seed scroll, Pattern Seed variation, Scale, Intensity, Transition Softness, Average Patch Separation, diagnostics, and the four-value-noise evaluation budget. It adds no control, shader property, texture, generated asset, scene, prefab, profile, style, material edit, CPU rebuild, or per-frame work.

## V3M-A1.3.4 validation gate

1. Unity and shader compilation complete without errors or warnings.
2. Representative seeds materially reduce frequent small circular light/dark islands in the raw, weighted, and normal views.
3. Broad connected regions retain irregular silhouettes, dents, protrusions, and useful pattern diversity.
4. Small, large, negative, and integer-limit seeds remain smooth and free of the pre-A1.3.3 precision artifacts.
5. Scale, Intensity, Transition Softness, Average Patch Separation, occupancy, and the four-noise evaluation budget remain otherwise accepted.

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
- V3M-A1.2 authorable intensity and transition softness: superseded by V3M-A1.3 after the user required a wider intensity range, stronger maximum softness, independent pattern selection, and nonuniform separation authoring.
- V3M-A1.3 pattern and separation authoring: controls and overall appearance accepted, but seed occupancy was inconsistent; superseded by V3M-A1.3.1's occupancy stabilization.
- V3M-A1.3.1 occupancy stabilization: active coverage became consistent, but major seed layouts remained too similar because the dominant primary realization only deformed in place; superseded by V3M-A1.3.2's seed-window translation.
- V3M-A1.3.2 seed-window translation: pattern variation, authoring controls, and final rendering accepted, but raw authored seed magnitudes caused increasing single-precision noise-coordinate quantization; superseded by V3M-A1.3.3's CPU-hashed bounded seed scroll.
- V3M-A1.3.3 bounded seed scroll: high-seed precision, seed diversity, controls, occupancy, and final rendering accepted, but the additive higher-frequency secondary source produced frequent small circular threshold islands; superseded by V3M-A1.3.4's contour-only secondary morphology.

### Rejected as final fixes

- Increasing all variation/noise controls without changing region morphology.
- Counting River/shore response as independent macro composition.
- Using Painted Accent density as macro composition.
- Promoting the current CPU field to sole production authority.

### Deferred

- Persistent generated macro texture, pending measured evidence that the shader route is inadequate.

## Next work items

1. Unity-validate V3M-A1.3.4 across representative small, large, negative, and integer-limit Pattern Seed values using the raw, weighted, and normal views.
2. Confirm frequent isolated circular islands decline materially while broad irregular region diversity and occupancy remain accepted.
3. Confirm seed magnitude remains precision-safe and all five Macro Patch Composition controls retain their accepted behavior.
4. Mark V3M macro composition visually accepted if no further blocker appears.
5. Resume V4 Contact / Edge Accents only after that acceptance.
