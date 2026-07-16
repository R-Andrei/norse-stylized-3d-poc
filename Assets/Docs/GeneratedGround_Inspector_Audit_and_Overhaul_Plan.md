# GeneratedGround Inspector and Painted Accent Production Architecture

## Status

**Workstream state: complete, Unity-validated, and accepted on 2026-07-15. Broader Ground development remains active.**

This document closes only the GeneratedGround Inspector overhaul and the Painted Accent authoring, rendering, production-bake, build-validation, and generated-asset lifecycle. It does not declare the Ground visual system complete. V3M Broad Macro Patch Completion and V3R Ground Elevation Readability are accepted. The active Ground milestone is V3S River-Coupled Ground Response, recorded in `Ground_River_Coupled_Surface_Response_Architecture.md`; V4 Contact / Edge Accents remains queued afterward and excludes River sources.

Accepted implementation status:

- **GI-A1 — Inspector skeleton and authority correction:** Unity-validated and accepted.
- **GI-A2 — Unified inline shared/local authoring:** Unity-validated and accepted.
- **GI-A3 / GI-A3.1 — Painted Accent visibility contract and Unity 6.5 warning cleanup:** Unity-validated and accepted.
- **GI-A4 — Diagnostics separation and cleanup:** Unity-validated and accepted.
- **PA-B1 / PA-B1.1 — One-button persistent Painted Accent bake and compile correction:** Unity-validated and accepted.
- **PA-B2 / PA-B2.1 — Baked-only Play Mode and Player rendering plus persistent-texture naming correction:** Unity-validated and accepted.
- **PA-B3 — Exact production validation and hard build enforcement:** Unity-validated and accepted.
- **PA-B4 / PA-B4.1 — Project-wide generated-asset audit, conservative orphan cleanup, and compile correction:** Unity-validated and accepted.

The latest project source is authoritative if it conflicts with this document. Patch-local status and “Next work items” sections later in this file are retained as historical sequencing evidence and are superseded by this final status.

## Final accepted production contract

```text
GeneratedGround is the unified authoring surface
→ Edit Mode builds the authoritative mesh-free procedural preview
→ Ink Colour and Ink Opacity update through Material only
→ Bake Painted Accents creates or updates one automatically owned persistent R8 asset
→ Play Mode and Player bind only the persistent production texture
→ no runtime SurfaceStroke generation
→ no runtime ProjectedGlyph or companion-cluster solving
→ no runtime coverage rasterization or CPU upload
→ build validation blocks Missing, Stale, Incompatible, duplicate, shared, or ownership-mismatched output
→ project audit finds generated assets that no longer have a legitimate owner or reference
→ only confirmed orphans may be deleted through an explicit reviewed action
```

The ordinary author workflow is:

```text
Author in GeneratedGround
→ Bake Painted Accents
→ validate or build
```

Generated-output maintenance is:

```text
Release Production Bake when an owning Ground no longer needs it
→ save the scene manually
→ Tools > Generated Ground > Audit and Clean Painted Accent Assets...
→ review the dry-run report
→ delete only Confirmed orphan assets
```

## Non-negotiable constraints

- `GeneratedGround` must become the central Ground authoring surface.
- Shared recipes, variants, profiles, and material controls may remain their architectural owners, but authors must be able to edit the resolved values from `GeneratedGround`.
- Shared-versus-local ownership must be explicit in the Inspector.
- Do not silently duplicate, clone, or migrate shared data.
- Do not modify or package Unity scenes or prefabs.
- Do not restore the retired 3D Painted Accent ridge path.
- Preserve the accepted PA-P1 through PA-P4 generation and performance baseline.
- Production Painted Accents use a zero-setup, one-button persistent output workflow.
- Play Mode and Player use only persistent production coverage; missing or stale output never triggers runtime procedural generation.
- Player builds are blocked when required output is missing, stale, incompatible, duplicated, shared, or ownership-mismatched.
- Generated outputs are removed only through the explicit project-wide audit and confirmed-orphan cleanup workflow.

## Audit summary

The original authoring problem was not a single missing control. It combined poor information architecture, hidden shared ownership, mismatched Inspector/runtime authority, passive serialized-data mutation, and a Painted Accent rendering configuration that was nearly imperceptible in normal lit rendering. GI-A1 through PA-B4 resolved that workstream; the sections below preserve the evidence and accepted decisions.

### Proven structural problems

1. The old `GeneratedGroundEditor` order followed implementation history instead of author workflow.
2. Ground debug and regeneration accounting appeared before basic geometry.
3. Painted Accent authoring and Painted Accent diagnostics were separate top-level sections.
4. `Surface Detail`, which changes geometry, was placed under `Surface` rather than Ground shape.
5. `Patch Coordinate` was isolated in an unrelated `Advanced` foldout.
6. the main Inspector exposed local material overrides but not the active shared variant material controls.
7. ordinary shader feature controls for Directional Streaks, Pooled Wetness, and Trampled Wear were inaccessible from `GeneratedGround`.
8. diagnostics and copy actions were duplicated across sections.
9. foldouts are editor-instance booleans and do not currently persist across Inspector recreation.

Primary implementation file:

```text
Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
```

### Proven authority defect

Before GI-A1, the inline Painted Accent editor searched the selected variant's serialized feature array and returned the first entry whose kind was `PaintedAccentLines`.

Runtime uses:

```text
GroundSurfaceVariantRecipe.TryGetFirstShaderFeature(requiredKind, out feature)
```

A runtime-applicable entry must be:

- non-null;
- enabled;
- `ShaderOnly`;
- the required feature kind;
- `Strength > 0`.

Therefore, with an earlier disabled, non-shader, or zero-strength Painted Accent entry followed by a valid entry, the old Inspector edited data the renderer ignored.

Relevant files:

```text
Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
Game/Procedural/Ground/GroundSurfaceVariantRecipe.cs
Game/Procedural/Ground/GroundSurfaceFeatureRecipe.cs
```

GI-A1 resolves the actual runtime feature through `TryGetFirstShaderFeature`, maps that object back to its serialized array entry, and reports ignored or duplicate entries.

### Proven passive mutation defect

Both Ground style editors previously initialized Painted Accent compatibility fields merely because their controls were drawn.

Affected files:

```text
Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
Game/Procedural/Ground/Editor/GroundSurfaceStyleProfileEditor.cs
```

The passive writes included initialization flags and defaults for:

- companion participation/tightness;
- triplet verticality;
- triplet share and cluster region bias;
- pair/triplet layout weights;
- glyph family weights;
- stroke path wiggle.

Both editors also silently raised `Stroke Length Max` when it was below `Stroke Length Min + 0.05 m`.

GI-A1 removes those draw-time writes. Legacy recipes now show an explicit **Initialize Painted Accent Authoring Values** action. Invalid length separation is reported as a warning while runtime compatibility clamping remains unchanged.

### Proven Painted Accent visibility problem

The active demo Ground resolves Snowfield / Clean and does not use a local material override. The serialized Painted Accent configuration is approximately:

```text
Strength:       0.08
Stroke Width:   0.017 m
Ink RGB:        0.578, 0.593, 0.604
Ground RGB:     0.807, 0.870, 0.906
Patch:          40 m
Coverage:       2048 × 2048 R8
```

Relevant asset and scene evidence:

```text
Game/Demo/Profiles/Ground/Styles/GSSP_Snowfield.asset
Game/Demo/Scenes/VisualFrameworkDemo.unity
```

The scene and asset were inspected only. They must not be modified by these patches.

Before GI-A3, the render path was:

```text
coverage sample
× contract mask
× _GroundPaintedAccentLineStrength
× ink alpha
→ albedo lerp toward ink RGB
```

Relevant files:

```text
Game/Procedural/Ground/GeneratedGround.cs
Game/Rendering/PixelSurface/PixelSurfaceGroundResponse.hlsl
Game/Rendering/PixelSurface/PixelSurfaceGroundForwardPass.hlsl
```

At full coverage and full contract mask, `Strength = 0.08` limits the visible blend to eight percent. The pale ink changes the pale Snowfield base by only roughly 0.018–0.024 per colour channel.

Coverage texel size on a 40 m / 2048 texture is approximately `0.01953 m`. The authored `0.017 m` line is approximately `0.87 texel` wide before partial raster coverage and bilinear filtering.

Conclusion:

- in normal lit rendering, the active configuration is nearly imperceptible by construction;
- nonzero generated coverage counters do not prove visible final rendering;
- GI-A3 added raw-binding and contract-coverage debug views, and Unity validation confirmed the corrected production binding and visible rendering path; no separate binding/mapping blocker remains in this workstream.

### Proven control issues

Painted Accent generic fields before GI-A3:

- `Strength` controlled final shader visibility and whether the feature was runtime-applicable.
- `Scale` was included in invalidation/signatures but was not consumed by the Painted Accent generator.
- `Contrast` was copied into placement settings but was not consumed afterward.
- generic `Direction` was ignored because Painted Accents use Facing Direction Degrees.
- Ink alpha was forced to one by `PaintedAccentInkColor`; there was no real independent Ink Opacity control.

GI-A3 retains `Strength` as **Stroke Intensity** because it is genuinely consumed by the SurfaceStroke generator and contributes to generated stroke strength and slight profile amplitude. It no longer controls final albedo opacity. Dead generic Scale and Contrast values are no longer drawn for Painted Accents and no longer participate in the Painted Accent SurfaceStroke signature. Generic Direction remains hidden for Painted Accents.

Hidden but active fields not previously available in the inline Ground controls:

- Enabled;
- Mask Influence;
- Seed Offset;
- Strength/visible contribution.

Retired compatibility-only fields:

- `paintedAccentDistributionSparseFloor`;
- `paintedAccentCompositionRegionScale`;
- `paintedAccentCompositionDensityContrast`.

GI-A3.1 removes these unused private fields from the C# recipe definition. Existing serialized YAML may retain the unknown keys until the user later saves the owning style asset; the patch does not modify or include any style asset. The active distribution controls already derive the required sparse-floor and composition behavior directly from Distribution Scale and Distribution Contrast.

## Data ownership decision

`GeneratedGround` becomes the authoring façade, but underlying ownership remains coherent:

| Data | Owner |
|---|---|
| Patch/geometry recipe | individual `GeneratedGround` |
| Family and variant recipes | shared `GroundSurfaceStyleProfile` |
| default semantic profile | shared `GroundSurfaceProfile` |
| existing local material override | individual `GeneratedGround` |
| feature recipes | selected shared variant |
| generated diagnostics | individual `GeneratedGround`, read-only |
| persistent Painted Accent production output | automatically managed Ground-owned generated R8 resource |

The Inspector must state scope explicitly, for example:

```text
Editing Shared Style — Snowfield / Clean
Changes affect every GeneratedGround using this variant.
```

or:

```text
Editing Local Material Override
Changes affect this GeneratedGround only.
```

Do not duplicate every style value onto the component. Do not silently clone style assets. A future local-style override, if approved, must be one deliberate coherent model rather than many fragmented override toggles.

## Approved Inspector information architecture

### 1. Ground Overview

- Surface Family
- Surface Variant
- deterministic identity: Shape Seed and Patch Coordinate
- style/variant warnings
- resolved surface profile
- feature summary
- direct advanced style reference only as a secondary escape hatch

### 2. Ground Geometry

#### Patch Domain

- Patch Size
- Mesh Resolution
- calculated dimensions, vertex count, and triangle count

#### Base Shape

- Profile
- Broad Form
- Roughness
- Surface Detail
- Edge Blend

#### Mountain Transition

- Direction
- Height Change

### 3. Surface Appearance

- Material Variation
- resolved GroundSurfaceProfile values edited inline through their actual profile asset owner
- resolved shared variant material controls edited inline
- optional local material override controls edited inline
- explicit ownership banner for shared profile, shared variant, or local component values

### 4. Surface Features

- Directional Streaks
- Pooled Wetness
- Trampled Wear
- first runtime-applicable recipe is authoritative
- when no recipe is runtime-applicable, the first matching recipe remains editable so Enabled, Execution Path, and visible intensity can restore it
- reserved and duplicate recipe warnings remain explicit

### 5. Painted Accents

Current GI-A2 authoring exposes the resolved shared recipe while preserving runtime authority and explicit ownership.

Final intended subgroups:

- Enable and Visibility
- Distribution
- Stroke Shape
- Shape Families
- Companion Clusters
- Surface Eligibility
- Preview and Production

Debug overlays and reports do not belong in this authoring section.

### 6. Ground and Environment Interaction

- Use Modifiers
- discovered Ground Modifier count
- discovered River count
- link refresh action belongs in Regeneration and Caching because it performs a regeneration

### 7. Regeneration and Caching

- Live Regeneration
- Randomize Shape Seed
- Regenerate Ground
- Refresh Modifier and River Links + Regenerate
- later stage/cache state summaries

Button labels must state their real scope.

### 8. Debug and Diagnostics

Collapsed by default:

- Ground Material Debug
- Last Surface Mask Diagnostics
- Painted Accent Debug and Reports
- Editor Regeneration Accounting

The accepted Projected Glyph report must not depend on a Scene overlay toggle.

The transient R8 report must be called **Last Coverage Raster**, not a production bake.

## GI-A1 implementation record

GI-A1 changes only editor code and documentation.

Implemented:

1. Reordered and regrouped the main Inspector into the approved top-level skeleton.
2. Moved Shape Seed and Patch Coordinate into Ground Overview.
3. Grouped Patch Domain, Base Shape, and Mountain Transition under Ground Geometry.
4. Moved Surface Detail into Base Shape.
5. Renamed Surface to Surface Appearance.
6. Renamed Modifiers to Ground and Environment Interaction.
7. Added Regeneration and Caching with explicit action names.
8. Moved Ground debug, surface diagnostics, Painted Accent debug/reports, and accounting under Debug and Diagnostics.
9. Removed the arbitrary Advanced section and ungrouped bottom buttons.
10. Changed Painted Accent lookup to follow the exact runtime resolver.
11. Added selected-variant warnings for null, unsupported, zero-strength, and duplicate runtime-applicable feature entries.
12. Removed draw-time compatibility initialization and automatic stroke-length mutation from both Ground style editors.
13. Added explicit compatibility initialization action.
14. Renamed transient coverage reporting to Last Coverage Raster.
15. Made Accepted Projected Baseline reporting independent of the projected Scene overlay toggle.

Explicitly not included in GI-A1:

- no new Ink Opacity data field;
- no visibility tuning;
- no shared material/profile inline authoring;
- no runtime or generator algorithm changes;
- no PA signature changes;
- no persistent bake;
- no scene, prefab, material, or style asset modification.


## GI-A2 implementation record

GI-A2 changes only the GeneratedGround custom editor and this canonical document. It does not alter runtime recipes, generators, shaders, scenes, prefabs, materials, or style assets.

Implemented:

1. Added a first-class **Surface Features** top-level section between Surface Appearance and Painted Accents.
2. Added inline authoring for the resolved `GroundSurfaceProfile` asset:
   - Patch Scale;
   - Patch Contrast;
   - Patch Edge Softness;
   - Exposure Bias;
   - Damp Deposit Bias;
   - Vegetation Suitability;
   - Rocky/Dry Suitability;
   - Snow Eligibility;
   - Rain Absorption.
3. Kept `Footprint Visibility` and `Grass Recovery Speed` hidden because they still have no active Ground consumer.
4. Replaced the old local-override-only material section with resolved ownership authoring:
   - shared variant material values are editable directly when no local override is active;
   - local material values remain editable when **Use Local Material Override** is enabled;
   - ownership and consequences are displayed before the controls.
5. Added inline shared-variant authoring for:
   - Directional Streaks;
   - Pooled Wetness;
   - Trampled Wear.
6. Feature controls include Enabled, Execution Path, Intensity, Scale, Contrast, Surface Mask Influence, Pattern Seed Offset, and Direction only where the shader actually consumes it.
7. Extended Painted Accent authoring with its previously hidden active controls:
   - Enable Painted Accents;
   - Execution Path;
   - current legacy Visible Strength;
   - Surface Suitability Influence;
   - Pattern Seed Offset.
8. Preserved GI-A1 runtime authority:
   - the first runtime-applicable recipe remains the default authoring target;
   - if none is applicable, the first matching recipe is exposed so it can be restored without opening the style asset;
   - duplicate and ignored-entry warnings remain visible.
9. Shared profile edits refresh every loaded scene Ground that resolves the same profile asset.
10. Shared variant material edits refresh every loaded scene Ground using that style/variant without a local material override.
11. Shared feature and Painted Accent edits refresh every loaded scene Ground using that style/variant, including Grounds with local material overrides because feature ownership remains shared.
12. Shared-asset editing is deliberately disabled for multi-object selections whose owner may differ. Component-local common controls remain available.
13. Editor refresh searches use `FindObjectsByType` without sorting and exclude persistent asset objects; there is no per-frame scan.
14. Undo/Redo refreshes loaded scene Grounds so restored shared assets and local overrides immediately reapply their resolved generation/material state.
15. Merely drawing the Inspector still performs no serialized migration or asset mutation.

Explicitly not included in GI-A2:

- no new feature recipe is invented when a selected variant has no recipe entry at all; recipe-list structure remains an explicit advanced style operation;
- no Ink Opacity field;
- no Painted Accent visibility tuning or shader change;
- no removal of dead Painted Accent Scale/Contrast signature inputs;
- no timing-history redesign;
- no persistent production bake;
- no scene, prefab, material, profile, or style asset modification in the patch.

### GI-A2 ownership and refresh contract

| Edited value | Serialized owner | Loaded consumers refreshed | Minimum intended work |
|---|---|---|---|
| Material Variation | selected `GeneratedGround` | selected Ground | existing component validation path |
| Surface Profile fields | referenced `GroundSurfaceProfile` asset | Grounds resolving that profile | profile-aware style refresh; full mask regeneration only when required and Live Regeneration permits it |
| Shared material controls | selected variant in `GroundSurfaceStyleProfile` | Grounds using the style/variant without local material override | Material |
| Local material controls | selected `GeneratedGround` | selected Ground | Material |
| Directional/Pooled/Trampled features | selected shared variant | all Grounds using the style/variant | Material |
| Painted Accent feature values | selected shared variant | all Grounds using the style/variant | signature-driven minimum PA stage plus Material |

### GI-A2.1 correction — shared material persistence and visible storage ownership

The inline shared-variant Material Controls path previously committed its separate `SerializedObject` only when the parent Inspector's immediate GUI change check reported a change. Unity colour-picker updates can arrive through the picker window without that exact parent event reporting the change, leaving an apparently edited palette value unapplied or only dirty in memory. A script compilation or assembly reload could then restore the last value actually stored in the style asset.

GI-A2.1 makes the serialized owner authoritative and explicit:

- the shared style `SerializedObject` is applied after every Material Controls draw; its actual `ApplyModifiedProperties()` result determines refresh work;
- changed `GroundSurfaceStyleProfile` assets are marked dirty and queued for a coalesced `AssetDatabase.SaveAssetIfDirty` call;
- pending style saves are flushed before assembly reload, so code changes cannot discard recently authored palette values;
- the Material Controls section shows a direct **Stored In** line for either the shared style asset and variant or the local scene/component override;
- local overrides retain their existing scene-serialization contract and still require the scene to be saved.

This correction changes only the GeneratedGround custom editor and this canonical document. It does not modify runtime resolution, shaders, materials, styles, profiles, scenes, prefabs, defaults, or existing serialized values.

## GI-A3 implementation record

GI-A3 changes the Painted Accent recipe contract, Ground material binding and diagnostics, the Ground shader, both Ground style authoring surfaces, and this canonical document. It does not alter SurfaceStroke placement, ProjectedGlyph composition, cluster solving, coverage rasterization, scenes, prefabs, materials, or style assets.

Implemented:

1. Added a dedicated serialized `Painted Accent Ink Opacity` value with a non-mutating compatibility contract:
   - newly authored recipes initialize it explicitly to `1.00`;
   - existing recipes whose initialization flag is absent resolve to `1.00` without rewriting the asset merely because an Inspector is drawn;
   - moving the slider records an explicit authored value.
2. Renamed legacy generic Painted Accent Strength to **Stroke Intensity** in both authoring surfaces.
3. Preserved Stroke Intensity as a generation control because it is genuinely consumed by the SurfaceStroke generator and affects per-stroke strength and slight projected-profile amplitude.
4. Removed Stroke Intensity from final albedo-opacity authority. Final coverage composition now uses `_GroundPaintedAccentInkOpacity`.
5. Kept `_GroundPaintedAccentLineStrength` bound as a legacy compatibility property, but the Ground shader no longer consumes it for Painted Accent visibility.
6. Made Ink Colour and Ink Opacity Material-only. Neither participates in SurfaceStrokes, ProjectedGlyphs, or Coverage signatures.
7. Removed dead generic Painted Accent Scale and Contrast from the SurfaceStroke signature and stopped drawing Scale, Contrast, and generic Direction for Painted Accent recipes. Their serialized compatibility values remain untouched.
8. Added a read-only **Visibility and Binding Status** block to `GeneratedGround` reporting:
   - coverage resolution and generated/enabled state;
   - renderer property-block binding status;
   - local mapping versus current mesh bounds;
   - coverage texel world size;
   - authored width in metres and texels;
   - Ink Opacity;
   - estimated maximum palette contrast after opacity.
9. Binding diagnostics compare the actual renderer `MaterialPropertyBlock` against the Ground's current:
   - coverage texture;
   - coverage-enabled flag;
   - origin/size;
   - world-to-local matrix;
   - ink colour;
   - ink opacity.
10. Added debug mode **Ground Painted Accent Raw Coverage Binding**. It displays raw sampled coverage in unmistakable magenta over a dark background and bypasses:
    - contract mask;
    - Ink Opacity;
    - Ink Colour;
    - normal Ground lighting composition.
11. Renamed the previous mode 28 Inspector label to **Ground Painted Accent Contract Coverage** so its contract-mask multiplication is explicit.
12. Added actionable warnings for:
    - no runtime-applicable recipe;
    - missing or disabled coverage;
    - stale MaterialPropertyBlock binding;
    - mapping mismatch;
    - authored width below one coverage texel;
    - low estimated final palette contrast.

### GI-A3 compatibility and visual consequence

The active Snowfield / Clean recipe previously used `Strength = 0.08` as both generated Stroke Intensity and final blend opacity. GI-A3 deliberately preserves the generated intensity at `0.08` but resolves existing Ink Opacity to `1.00`.

Therefore:

```text
Generation authority: unchanged
Maximum final opacity: 0.08 → 1.00
Ink colour: unchanged
Coverage texture: unchanged for unchanged authoring values
```

This is an intentional visible correction, not procedural-generation parity. Deterministic SurfaceStroke, ProjectedGlyph, cluster, and coverage counters should remain unchanged for unchanged inputs.

### GI-A3 shader impact audit

Changed shader files belong to the Ground PixelSurface path. `PixelSurfaceGroundMaterialProperties.hlsl` is included only by `SH_PixelGroundSurfaceLit.shader`; Painted Accent coverage helpers are guarded by `PS3D_PIXELSURFACEGROUND_MATERIAL_PROPERTIES`.

Expected impact:

- Ground Painted Accent final colour and debug modes: changed deliberately;
- Ground non-Painted-Accent material behavior: unchanged;
- River shaders: unchanged;
- Generated Mass shaders: unchanged;
- generic PixelSurface shaders without Ground material properties: unchanged.

### GI-A3.1 warning cleanup

After Unity compilation exposed warnings, the follow-up cleanup:

- removed the three retired, unread compatibility backing fields from `GroundSurfaceFeatureRecipe`;
- replaced all new `FindObjectsByType<T>(FindObjectsInactive, FindObjectsSortMode)` calls in `GeneratedGroundEditor` with Unity 6.5's non-sorting `FindObjectsByType<T>(FindObjectsInactive)` overload;
- preserved the same inclusion of inactive loaded Grounds and did not introduce ordering dependence;
- did not modify or reserialize any scene, prefab, material, profile, or style asset.

### GI-A3 validation boundary

Source inspection and static validation can prove the new property contract, signatures, and shader source path. Only Unity can finally prove:

- shader compilation on the target URP version;
- the renderer's live property block reports Current;
- raw mode 29 shows the expected coverage positions;
- contract mode 28 shows the coverage after vertex-contract masking;
- normal lit Scene and Game views show clearly discernible lines;
- Ink Colour and Ink Opacity execute Material-only regeneration;
- deterministic generation counters remain unchanged.

## GI-A4 implementation record

GI-A4 is a diagnostics-presentation patch. It changes `GeneratedGround` timing retention, the `GeneratedGround` Inspector report layout, and this canonical document. It does not alter Ground generation, Painted Accent placement, ProjectedGlyph composition, coverage rasterization, material rendering, scenes, prefabs, materials, profiles, or style assets.

Implemented:

1. The latest regeneration report now contains only stages actually executed by that pass. A Material-only refresh no longer displays retained SurfaceStrokes children beneath a `0.00 ms` parent.
2. Detailed Painted Accent timing is retained in three explicit historical records:
   - Last Completed SurfaceStrokes Timing;
   - Last Completed ProjectedGlyphs Timing;
   - Last Completed Coverage Timing.
3. Historical records update only when the corresponding stage actually completes and survive later Material-only or cache-hit passes.
4. Painted Accent placement, projected-baseline, and coverage-statistics reports remain available beside their matching timing records.
5. Reading or copying the Accepted Projected Baseline report is now observational. It no longer calls the generators or silently performs expensive work.
6. Scene overlays are separated from reports under **Painted Accent Scene Debug** and **Painted Accent Reports**.
7. Regeneration Accounting now contains accounting only. Current timing and Painted Accent historical timing are no longer duplicated inside it.
8. Duplicate clipboard actions were removed. The canonical actions are:
   - one copy action beside each report;
   - one **Copy All Painted Accent Reports** action;
   - one **Copy All Ground Diagnostics** action.
9. Surface-mask diagnostics now have their own adjacent copy action.
10. PA-P1 through PA-P4 timing and workload telemetry is preserved in the retained ProjectedGlyphs timing and projected-baseline reports.

### GI-A4 timing semantics

| Report | Meaning |
|---|---|
| Current Regeneration Timing | Only the most recent pass and only stages it executed |
| Last Completed SurfaceStrokes Timing | Most recent pass that actually rebuilt SurfaceStrokes |
| Last Completed ProjectedGlyphs Timing | Most recent pass that actually rebuilt ProjectedGlyphs |
| Last Completed Coverage Timing | Most recent pass that actually rasterized/uploaded coverage |
| Last Placement Result | Retained output statistics from the latest generated placement |
| Accepted Projected Baseline | Retained accepted/rejected/quota/workload statistics; read-only |
| Last Coverage Raster | Retained texture/texel/width statistics |
| Editor Regeneration Accounting | Editor request/pass batching and stage counts only |

This separation is intentional: current-pass timing must never visually imply that retained historical substage work ran again.

## Remaining phases

### GI-A3 — Painted Accent visibility contract

Implemented in the current patch. Unity validation remains required before acceptance.

### GI-A4 — Diagnostics cleanup

Implemented in the current patch. Unity validation remains required before acceptance.

### PA-B1 — One-button persistent output

Implemented. Each Ground owns an automatically managed persistent R8 coverage asset through a scene-GUID and Ground-ID scoped generated-output contract.

The author sees only:

```text
Bake status: Missing / Current / Stale / Incompatible
[Bake Painted Accents]
```

There is no asset reference, save dialog, manual folder creation, texture assignment, recipe Inspector, or material editing.

### PA-B2 — Baked-only runtime

At runtime:

- no SurfaceStroke generation;
- no ProjectedGlyph generation;
- no cluster composition;
- no coverage rasterization;
- no CPU coverage upload;
- no silent procedural fallback.

### PA-B3 — Stale detection and build enforcement

- exact persistent-output signature;
- generated-resource compatibility/version check;
- build validation failure for missing or stale output;
- actionable repair message.

## Validation requirements

For every later code patch:

1. Run an actual available parser/compiler validation over every changed C# file; do not claim Unity compilation unless Unity ran.
2. Scan changed C# files for malformed multiline strings, duplicate signatures/locals, missing helpers, and call/arity mismatches.
3. Preserve original line endings and run `git diff --check`.
4. Validate Undo/Redo, shared asset dirtiness, scene dirtiness, and multi-object behavior.
5. Validate minimum regeneration scope for every edited control.
6. Confirm no scene or prefab is modified or included.

## Methods tried ledger

### Accepted

- mesh-free projected Painted Accent glyphs;
- authoritative pair/triplet cluster quotas;
- PA-P1 conservative swept-width broad phase;
- PA-P2 near-parallel broad phase and segment metadata;
- PA-P3 cheap-before-geometry pruning;
- PA-P4 deterministic external conflict index and incremental reconciliation;
- transient 2048 × 2048 R8 coverage as the current preview/render input;
- `GeneratedGround` as the unified authoring façade;
- runtime-authoritative feature resolution in the Inspector;
- unified inline authoring of resolved shared profile, shared/local material, and supported shader-feature values;
- explicit owner-aware refresh of loaded consumers after shared asset edits;
- safe single-object gating for ambiguous shared-asset editing;
- explicit compatibility initialization rather than passive Inspector mutation;
- dedicated material-only Painted Accent Ink Opacity with non-mutating `1.00` compatibility fallback;
- Stroke Intensity retained as genuine generation authority rather than mislabeled visibility;
- raw-coverage binding debug mode separated from contract-masked coverage debug;
- direct renderer property-block and local-mapping diagnostics;
- dead Painted Accent Scale/Contrast invalidation removed; retired unread compatibility backing fields removed from code while existing asset YAML is left untouched;
- current-pass timing separated from retained completed-stage telemetry;
- diagnostic report reads made observational rather than hidden generation triggers;
- one compact canonical Ground diagnostics hierarchy and clipboard path;
- one-button, zero-setup production output requirement;
- persistent scene-GUID/Ground-ID-scoped R8 coverage output;
- exact coverage-byte and local-mapping stale detection.

### Rejected or retired

- 3D raised Painted Accent ridge as production output;
- source-space angle-heavy composition and large rotations used to fake stepping;
- runtime procedural generation as the final production architecture;
- manual production asset references or drag-and-drop;
- treating nonzero coverage telemetry as proof of visible lines;
- arbitrary Inspector grouping by source-file ownership;
- passive serialized migration during Inspector drawing.

### Deferred opportunities — not blockers

- A future Ground-output project may justify packing additional generated control channels beside the accepted R8 Painted Accent coverage. That is a separate optimization and format-design decision, not unfinished Painted Accent work.
- A future full-style local override model may be useful. The accepted current contract intentionally keeps style, variant, profile, and feature ownership shared while presenting them through the unified GeneratedGround authoring façade.

## Next work items

1. **None required for this workstream.** The GeneratedGround Inspector and Painted Accent production architecture are closed and accepted.
2. Use **Audit and Clean Painted Accent Assets...** after deleting Grounds/scenes or releasing production bakes so obsolete generated outputs do not accumulate.
3. Treat future channel packing, full-style local overrides, or broader Ground/River performance work as separately scoped projects with new evidence and approval.

---

# PA-B1.1 compiler correction

The first PA-B1 package contained one C# scope error in `GeneratedGroundEditor.DrawPaintedAccentStrokeControls`:

```text
GeneratedGroundEditor.cs(1629,59): CS0103
The name 'generatedGround' does not exist in the current context
```

The Preview and Production subsection call referenced a pattern variable that existed only inside earlier conditional blocks. PA-B1.1 resolves the selected single `GeneratedGround` explicitly at method scope and draws Preview and Production only while the parent Painted Accents foldout is expanded. No runtime, bake, signature, texture-ownership, scene, prefab, material, shader, or style-data behavior changes.

Validation policy update: Tree-sitter parsing is syntax validation only and cannot prove local-symbol resolution. Future editor patches must supplement it with targeted symbol/scope checks for newly introduced local identifiers, while Unity compilation remains the authoritative compile gate.

# PA-B1 implementation — persistent Painted Accent production output

## Status

Implemented after GI-A4 validation.

PA-B1 added the persistent production artifact and one-button authoring workflow. PA-B2 subsequently made that persistent output authoritative in Play Mode and Player.

## Production asset contract

Each `GeneratedGround` stores hidden ownership metadata for one automatically managed Painted Accent production texture:

```text
Bake identifier
Persistent R8 texture reference
Exact coverage-output signature
Bake-format revision
Coverage origin/size
Covered-texel diagnostics
```

The author never edits those fields directly.

The editor creates or updates the asset at:

```text
Assets/Game/Generated/Ground/PaintedAccents/
<scene-guid>/
GG_PaintedAccentCoverage_<ground-id>.asset
```

The scene GUID prevents a copied or renamed scene from overwriting another scene's output. The Ground identifier prevents two Grounds in the same scene from sharing one output. A duplicated Ground receives a new identifier on its next bake. If a copied scene inherits a reference to the original scene's texture, the Inspector reports the output as ownership-incompatible until rebaked into the copied scene's folder.

An unsaved scene cannot be baked because it has no stable scene GUID.

## One-button workflow

The `GeneratedGround` Inspector exposes:

```text
Painted Accents
└── Preview and Production
    Live Preview: Current / Stale / Missing
    Renderer Source: Live procedural preview (PA-B1)
    Production Bake: Missing / Current / Stale / Incompatible
    [Bake Painted Accents]
```

The button:

1. validates that the target is an Edit Mode instance in a saved scene;
2. refreshes modifier and River discovery;
3. executes the authoritative Ground/Painted Accent regeneration path;
4. validates readable R8 live coverage;
5. creates or updates the persistent texture automatically;
6. records the exact coverage-output signature and mapping metadata;
7. marks the Ground and generated texture dirty and saves only the generated asset.

There is no save dialog, asset-reference field, material edit, or external recipe step. Unity will mark the scene instance dirty through Undo/serialization; the patch itself does not modify or include a scene or prefab.

## Exact stale-detection signature

PA-B1 hashes the actual authoritative live output rather than reconstructing a second parallel list of procedural inputs.

The SHA-256 signature covers:

```text
Bake-format revision
Coverage-baker revision
Coverage texture width and height
Coverage origin and local-XZ size
Every R8 coverage texel
```

This is stable across Editor restarts and independent of object-discovery ordering. Any procedural, geometry, modifier, River, placement, clustering, profile, or raster change that alters production coverage or mapping changes the signature after regeneration.

The following intentionally do **not** invalidate the bake because they do not alter coverage:

```text
Ink Colour
Ink Opacity
Ground palette and lighting response
Debug view
Diagnostic foldouts or reports
World transform changes that preserve the same local coverage mapping
```

If a procedural source setting changes while live regeneration is disabled, the existing live-preview signature reports Stale before baking. The Bake action first performs authoritative regeneration and then hashes the resulting coverage.

## Runtime boundary

PA-B1 originally retained procedural rendering while the persistent artifact was validated. The final accepted contract is now:

```text
Edit Mode: live procedural preview
Play Mode and Player: persistent production coverage only
Build: exact PA-B3 production validation required
```

## Methods-tried ledger update

### Accepted

- Native Unity `.asset` Texture2D output in R8 format.
- Scene-GUID-scoped generated-output folders.
- Stable generated-output identifier stored on `GeneratedGround`.
- Exact SHA-256 signature of coverage bytes and local mapping.
- Same-scene duplicate-identifier detection before writing an asset.
- Copied-scene ownership mismatch detection.
- Explicit one-button bake; no automatic build-time mutation.
- Targeted generated-asset save rather than a project-wide save.

### Rejected

- Input-list hashing that could become unstable through discovery order or miss future implementation changes.
- Using transient cache signatures containing generation revisions as persistent stale detection.
- Naming output only from the GameObject name.
- A project-global output path with no scene ownership namespace.
- Exposing a texture reference or path selector to the author.
- Switching runtime to baked-only behavior before persistent output is validated.

## Next work items

1. Validate asset creation, update, same-scene duplication safety, copied-scene ownership detection, and status persistence across an Editor restart.
2. Confirm Ink Colour and Ink Opacity remain Material-only and do not stale the production bake.
3. Implement PA-B2 baked-only runtime after PA-B1 is confirmed.
4. Implement PA-B3 build validation after baked-only runtime is confirmed.

---

# PA-B2 implementation — baked-only Play Mode and Player rendering

## Status

Implemented after PA-B1.1 persistent-output validation.

PA-B2 changes only the Painted Accent renderer source and execution boundary. Edit Mode retains the authoritative procedural preview and one-button bake. Play Mode and Player builds bind the serialized persistent R8 production texture and do not execute the procedural Painted Accent pipeline.

## Execution contract

```text
Edit Mode
→ SurfaceStrokes
→ ProjectedGlyphs and companion solving
→ coverage raster and upload
→ live procedural coverage bound to the Ground renderer

Play Mode and Player
→ validate persistent production texture structurally
→ bind persistent R8 coverage and stored local mapping
→ render
```

Play Mode and Player skip:

```text
Painted Accent SurfaceStroke generation
ProjectedGlyph generation
pair/triplet cluster solving
Painted Accent river-exclusion spline snapshot construction
coverage rasterization
procedural coverage texture creation or upload
procedural debug snapshot generation
```

Ground geometry, collider generation, surface masks, ordinary material features, modifier snapshots required by Ground geometry, and River corridor integration remain unchanged.

## Runtime source validation

The active Painted Accent recipe requires production coverage when it resolves as an enabled Shader Only feature with Stroke Intensity above zero.

Runtime accepts the stored output only when:

```text
persistent texture reference exists
stored coverage signature is non-empty
bake-format revision matches
texture format is R8
texture dimensions are positive
stored local-XZ mapping has positive size
```

A valid output binds the persistent texture with its stored origin/size and the Ground's current world-to-local matrix.

If the feature is disabled or has no runtime-applicable recipe, production output is Not Required and coverage is disabled without an error.

If required output is missing or incompatible, the renderer binds neutral black coverage, disables Painted Accents, and emits one compact error per distinct failure reason:

```text
No procedural runtime fallback was executed.
```

This prevents hidden startup computation and makes invalid production data visible immediately.

## Stale-data boundary

PA-B2 validates the serialized artifact structurally at runtime and deliberately does not regenerate live coverage merely to recompute staleness. PA-B3 supplies the accepted exact Missing/Stale/Incompatible enforcement before every Player build.

PA-B3 now enforces exact current production output before a Player build. Ink Colour and Ink Opacity remain Material-only and continue to use the current persistent coverage without rebaking.

## Inspector behavior

Preview and Production now reports:

```text
Edit Mode:
  Live Preview: Current / Stale / Missing
  Renderer Source: Live procedural preview (Edit Mode)

Play Mode:
  Edit Preview: Suspended during Play Mode
  Renderer Source: Persistent production coverage (PA-B2)
  Runtime Coverage: Current / Not Required / Missing / Incompatible
  Production Artifact: Available (structural validation) / Missing / Incompatible
```

The one-button bake remains disabled in Play Mode.

## Methods-tried ledger update

### Accepted

- Compile-time/runtime source split: Edit Mode live preview, Play/Player persistent output.
- Persistent texture sampled directly; no runtime copy or CPU upload.
- Runtime structural validation with a neutral failure state.
- One compact error per distinct production failure.
- No procedural fallback for missing or incompatible output.
- Painted Accent-only River exclusion snapshots omitted in production mode.

### Rejected

- Runtime regeneration as a safety fallback.
- Copying persistent coverage into a transient runtime Texture2D.
- Treating an invalid bake as enabled coverage.
- Recomputing full live coverage in Play Mode merely to determine staleness.

## Next work items

1. Validate that entering Play Mode performs no SurfaceStrokes, ProjectedGlyphs, Coverage raster, or Coverage upload stages.
2. Confirm the renderer source is persistent production coverage and the visible result matches Edit Mode.
3. Remove or invalidate the production texture temporarily and confirm one clear error, neutral coverage, and no procedural fallback.
4. Implement PA-B3 build enforcement for Missing, Stale, Incompatible, duplicate-identifier, and ownership-mismatch output.

---

# PA-B2.1 correction — persistent texture main-object naming

## Status

Implemented after PA-B2 validation exposed a Unity asset-save warning during a repeat bake.

## Proven defect

The generated asset filename used the complete stable Ground identifier:

```text
GG_PaintedAccentCoverage_<32-character-ground-id>.asset
```

but the `Texture2D.name` stored inside that asset used only the first eight identifier characters. Unity requires the main object's name to match the asset filename stem when saving a native `.asset`, and emitted:

```text
Main Object Name does not match filename
```

This did not indicate a coverage-generation or PA-B2 runtime-source failure. It was a persistent-asset naming inconsistency in `GroundPaintedAccentProductionBaker`.

## Accepted correction

A single `BuildAssetName(identifier)` helper is now authoritative for both:

```text
asset filename stem
Texture2D main-object name
```

The full 32-character identifier is retained in both locations. A repeat bake automatically renames an already-created truncated main object before saving it, so no manual deletion or reassignment is required. Asset path, ownership identifier, coverage bytes, mapping, signature, and runtime binding remain unchanged.

## PA-B2 diagnostics interpretation

In Play Mode, these records are expected to be empty after a domain reload:

```text
Last Completed SurfaceStrokes
Placement Report
Last Completed ProjectedGlyphs
Projected Baseline
Last Completed Coverage
Coverage Report
```

PA-B2 deliberately skips those procedural Painted Accent stages. The authoritative validation evidence is:

```text
Renderer Source: Persistent production coverage (PA-B2)
Runtime Coverage: Current
SurfaceStrokes stage count: 0
ProjectedGlyphs stage count: 0
Coverage stage count: 0
```

Ground geometry, collider, material, snapshots, and River-corridor stages may still execute at Play startup; PA-B2 only removes the procedural Painted Accent pipeline.

## Next work items

1. Confirm a repeat bake produces no main-object-name warning and leaves Production Bake Current.
2. Confirm Play Mode continues to report persistent production coverage with all three procedural Painted Accent stages at zero.
3. Proceed to PA-B3 build enforcement only after this correction is validated.

---

# PA-B3 implementation — production-bake build enforcement

## Status

Unity-validated and accepted after PA-B2.1 persistent naming and baked-only runtime validation.

PA-B3 closes the production-safety gap: an enabled Player build can no longer proceed while a build-scene `GeneratedGround` requires Painted Accents but lacks a current, compatible, uniquely owned persistent output.

## Shared validation contract

The selected-Ground preflight and build preprocessor use the same validator. For every Ground whose selected variant resolves an enabled Shader Only Painted Accent recipe with nonzero Stroke Intensity, validation proves:

```text
saved scene with a stable AssetDatabase GUID
valid 32-character Ground production identifier
no same-scene duplicate identifier
persistent texture reference exists and resolves as an AssetDatabase main asset
asset path matches the scene-GUID and Ground-ID ownership contract
no second Ground references the same production asset
bake-format revision matches
texture is readable R8 with positive dimensions
stored local-XZ mapping is valid
Texture2D main-object name matches the asset filename
stored signature matches the persistent texture bytes and mapping
fresh authoritative Edit Mode coverage matches the stored production signature
```

Ink Colour and Ink Opacity remain excluded because they do not change coverage.

A Ground with no runtime-applicable Painted Accent recipe reports `Not Required` and does not block the build.

## Isolated build-scene validation

The validator reads the active Unity 6.5 Build Profile scene list through `BuildProfile.GetScenesForBuild`; when no active profile exists it falls back to `EditorBuildSettings.scenes`. Each enabled scene is opened through `EditorSceneManager.OpenPreviewScene`, validated in isolation, and closed through `ClosePreviewScene` in a `finally` block.

The validator does not:

```text
save a scene
save a prefab
modify a production asset
rebake automatically
reuse only the currently open scene
silently disable invalid output
```

Authoritative procedural coverage may be regenerated inside the temporary preview scene solely to compute exact staleness. The temporary scene is discarded without saving.

## Build enforcement

`GroundPaintedAccentBuildPreprocessor` implements `IPreprocessBuildWithReport` and runs before the Player build. Any invalid required output throws one grouped `BuildFailedException` containing scene, Ground hierarchy path, status, reason, asset path when available, and the corrective action.

Blocking categories are:

```text
Missing
Stale
Incompatible
Ownership Mismatch
Duplicate Identifier
Shared Production Asset
Validation Failed
Scene Unavailable
```

There is no warning-only path and no automatic repair during build.

## Inspector preflight

Two explicit actions are available:

```text
Painted Accents > Preview and Production
  Validate Production Bake

Debug and Diagnostics
  Validate Painted Accent Production in Build Scenes
```

The selected-Ground action refreshes authoritative live coverage and compares it to the stored output without writing an asset. The build-scenes action executes the exact project-wide contract used by the build preprocessor.

## Methods-tried ledger update

### Accepted

- One shared validator for manual preflight and build blocking.
- Exact signature validation of both persistent bytes and current authoritative coverage.
- Isolated preview-scene loading for build-scene inspection.
- Same-scene duplicate identifier and texture-sharing checks.
- Cross-build-scene production-asset conflict detection.
- One grouped build failure rather than per-Ground Console flooding.
- Explicit validation only; no build-time mutation or rebake.

### Rejected

- Validating only loaded scenes or Inspector targets.
- Trusting the serialized signature without hashing the persistent asset.
- Structural-only build validation.
- Runtime stale detection through procedural regeneration.
- Automatic build-time rebake or scene save.
- Warning-only builds with invalid production coverage.

## Next work items

1. Compile and validate PA-B3 in Unity 6000.5.0f1.
2. Test Current, Stale, Missing, Incompatible, duplicated Ground, copied scene, shared asset, disabled feature, and multiple-failure build cases.
3. Confirm validation leaves scenes and prefabs unsaved and unmodified.
4. Perform the final Ground/Painted Accent architecture and documentation audit after PA-B3 acceptance.


---

# PA-B4 implementation — generated-asset audit and conservative cleanup

## Status

Unity-validated and accepted after PA-B3 production validation and the generated-output lifecycle gap were confirmed. PA-B4.1 supplied the final missing `System` namespace import with no behavioral change.

PA-B4 provides an explicit one-run workflow for finding and deleting persistent Painted Accent outputs that no longer have a legitimate project owner or reference. It does not delete assets during bake or build validation.

## Project-wide audit command

The canonical command is:

```text
Tools
└── Generated Ground
    └── Audit and Clean Painted Accent Assets...
```

The same workflow is reachable from `GeneratedGround > Debug and Diagnostics` through:

```text
Audit Generated Painted Accent Assets
Copy Generated Asset Audit
Delete Confirmed Painted Accent Orphans
```

The audit scans:

```text
all imported assets beneath Assets/Game/Generated/Ground/PaintedAccents
all loaded scenes, including unsaved scenes in memory
all saved project scenes, including scenes excluded from the active build profile
all direct project-asset dependencies beneath Assets
```

Saved scenes not already loaded are opened only as isolated preview scenes and are closed without saving.

## Classification contract

Every imported file beneath the managed generated-output root is classified as exactly one of:

```text
Active and referenced
Referenced but no longer required
Ownership mismatch
Shared incorrectly
Confirmed orphan
Unknown / unsafe
```

A confirmed orphan must satisfy all of these conditions:

```text
path is beneath the exact managed generated-output root
path matches the scene-GUID / Ground-ID naming contract
main asset is an R8 Texture2D
no GeneratedGround in any loaded or saved project scene claims it
no project asset directly references it
ownership and dependency scans completed without failure
```

Malformed assets, non-R8 assets, externally referenced outputs, ownership mismatches, and shared outputs are retained and reported. “Not in the active build profile” is never treated as proof that an asset is unused.

## Deletion safety

Deletion is an explicit two-step action inside a dedicated report window:

```text
Run Audit
→ review the complete report and every exact orphan path
→ Delete Confirmed Orphans
→ review the exact deletion set again
→ Confirm Delete
```

Immediately before deletion, the tool runs the full audit again. Deletion proceeds only when the fresh confirmed-orphan set exactly matches the reviewed set.

Deletion is blocked when:

```text
a loaded scene contains unsaved changes
a loaded persistent project asset contains unsaved changes
the audit was cancelled
a scene or dependency scan failed
the confirmed-orphan set changed during the safety re-audit
```

The cleanup command never saves or modifies a scene or prefab. It never deletes an asset outside the exact managed root, and it never deletes an unknown or ambiguous file.

## Per-Ground release workflow

`Painted Accents > Preview and Production` now includes:

```text
Release Production Bake
```

This is an explicit Undo-recorded scene edit that clears:

```text
production texture reference
production identifier
stored coverage signature
bake-format revision
stored local-XZ mapping
covered-texel diagnostics
runtime production status
```

The generated texture itself is intentionally left untouched. The user saves the scene manually, then runs the project-wide audit. Once no saved project reference remains, the texture becomes a confirmed orphan and can be deleted safely.

## Methods-tried ledger update

### Accepted

- One shared audit implementation for the Tools menu and Inspector actions.
- All-project-scene ownership scan, not build-scene-only cleanup.
- Direct AssetDatabase dependency scan for arbitrary scene, prefab, and asset references.
- Dry-run report with exact paths before deletion.
- Fresh full re-audit immediately before deletion.
- Hard deletion block for unsaved loaded scenes or persistent assets.
- Explicit per-Ground release followed by manual scene save and later orphan cleanup.
- Unknown and malformed generated-root contents are reported and retained.

### Rejected

- Automatic deletion during rebake.
- Automatic deletion during Player build validation.
- Treating exclusion from Build Settings as proof of disuse.
- Deleting by filename or age alone.
- Opening and saving scenes automatically to clear references.
- Deleting an asset still referenced by a Ground whose Painted Accents are disabled.

## Next work items

1. Compile and run the PA-B4 audit in Unity 6000.5.0f1.
2. Validate deleted Ground, deleted scene, disabled feature, released bake, duplicate Ground, copied scene, non-build scene, external reference, and malformed generated-root cases.
3. Confirm the cleanup tool never dirties or saves a scene merely by auditing.
4. After PA-B4 acceptance, perform the final Ground/Painted Accent architecture and documentation closure audit.


# PA-B4.1 compile hotfix — missing System namespace

Unity compilation exposed one unresolved framework symbol in `GroundPaintedAccentGeneratedAssetAudit.cs`:

```text
CS0103: The name 'StringComparer' does not exist in the current context
```

The audit report sorts confirmed orphan paths with `StringComparer.OrdinalIgnoreCase`, but the file did not import `System`. PA-B4.1 adds the missing `using System;` directive. No audit classification, deletion, scene handling, generated-asset ownership, or runtime behavior changed.

Validation for this hotfix also scans the complete PA-B4 editor file set for unqualified `System` framework symbols whose source file lacks the required namespace import. Unity compilation remains the authoritative semantic validation gate.

# Inspector and Painted Accent workstream closure — 2026-07-15

The GeneratedGround Inspector and Painted Accent production workstream is accepted. No known correctness, authoring, runtime-generation, build-safety, or generated-asset-lifecycle blocker remains in that scope. This is not a closure of GeneratedGround or the Ground visual roadmap. V3S River-Coupled Ground Response is the active milestone; V4 Contact / Edge Accents is queued after V3S and excludes River sources. V3S-A2A extends the main `GeneratedGround` authoring façade with automatic Bank/Riverbed Surface Layer dropdowns, inline reusable-profile editing, and in-place create/duplicate actions; routine River-coupled authoring must not require Project-window asset navigation. V3S-A2B adds the adjacent `River-Coupled Ground Response — Bank Composition` foldout. V3S-A2C.1 retains one master Bank Material Strength plus clearly labelled `Core Bank` and `Outer Bank Extension` groups. The outer group states that distance begins at the Riverbed Support edge and travels across the generated River corridor toward its terrain handoff. Extension, Strength, and Fade remain authored entirely from `GeneratedGround`; Strength/Fade disable automatically while Extension is zero. All controls remain disabled until a Bank Surface Layer is selected and are stored through the same shared-style/local-override ownership path.

The accepted maintenance rules are:

- author and preview through `GeneratedGround`;
- rebake after any change that alters generated coverage or local mapping;
- do not rebake for Ink Colour or Ink Opacity changes;
- allow PA-B3 to block invalid Player builds rather than bypassing validation;
- release obsolete per-Ground production ownership explicitly, save the scene manually, then run the PA-B4 audit;
- delete only assets classified as **Confirmed orphan** by the fresh reviewed audit;
- do not manually delete, rename, relocate, or reassign generated production assets as an ordinary workflow.

Any later change to the texture format, packing contract, authoring ownership model, or generation algorithm must increment the relevant revision/signature contract and be treated as a new separately validated patch series.

