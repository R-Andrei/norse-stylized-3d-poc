# Generated Mass Feature Implementation Checklist

Status: active implementation tracker  
Created for: Patch 14A — Generated Mass Framework Documentation  
Companion document: `Generated_Mass_Framework.md`

---

## 1. How to use this checklist

This document tracks the reusable feature library for Generated Mass.

A feature should not be considered recipe-ready until it has:

- a documented semantic meaning;
- at least one user-facing control;
- debug/validation support where practical;
- final visual interpretation that does not rely on debug-only overlays;
- recipe values or usage notes;
- validation against obvious failure cases.

Status labels:

```text
Not Started
Planned
Design Ready
Implemented: Debug/Data Only
Implemented: Final Response Partial
Implemented: Final Response Ready
Recipe Ready
Deferred
Rejected
```

Important rule:

```text
Feature first.
Generic test validation second.
Archetype recipe usage third.
```

---

## 2. Framework-level checklist

### Patch 14A — Documentation foundation

- [x] Create `Generated_Mass_Framework.md`.
- [x] Create this implementation checklist.
- [x] Declare `Rock_Generated_Mass_Upgrade_Plan.md` superseded as the active planning source.
- [x] Define Generated Mass as generic compact-mass framework, not rock-only system.
- [x] Define feature library / recipe / manual override model.
- [x] Define global DOs and DON'Ts.
- [x] Define initial reusable feature library.
- [x] Define initial archetype recipe concepts.
- [x] Define generic test mass workflow.

### Patch 14B — Recipe + Feature Stack Scaffold

- [x] Add `GeneratedMassFeatureRecipe` selection.
- [x] Add built-in scaffold recipes:
  - Generic Test Mass;
  - Cold Grey Stone;
  - Wet River Stone;
  - Pale Frost Stone;
  - Black Sacred Stone;
  - Custom.
- [x] Make recipe dropdown non-destructive by default.
- [x] Add explicit Apply Selected Recipe button.
- [x] Add explicit Reset Controls to Recipe button.
- [x] Add recipe match/modified status display.
- [x] Reframe existing generated-mass controls as the initial feature stack.
- [x] Keep old shape `MassRecipe` / `MassArchetype` behavior intact for now.
- [x] Do not add new feature channels yet.

### Patch 14B.1 — Feature-Oriented Inspector Layout

- [x] Replace global Mask Shape / Mask Strength / Mask Tinting buckets with feature foldouts.
- [x] Group Exposure controls under `Exposure`.
- [x] Group Base Lift, Base response and Base tint controls under `Base / Contact`.
- [x] Group Crevice Height/Fade/Irregularity, Crevice response and Crevice tint controls under `Crevice / Shelter`.
- [x] Group Dirt Crawl Height/Coverage, Dirt response and Dirt tint controls under `Dirt / Deposit`.
- [x] Keep Edge Wear controls under `Edge Wear`.
- [x] Keep ConcaveCrease controls under `Crease / Crack Debug`.
- [x] Keep shared raised-overlay visibility clearly marked as debug-only.
- [x] Do not add new feature channels.
- [x] Do not change shader/material behavior.
- [x] Update framework docs to make feature-oriented grouping foundational.

### Patch 14C — Generated Mass Surface-Chart Feature Atlas Foundation

Patch 14C is documentation-approved as a data/debug foundation patch. It should not implement the final artistic edge-wear response.

- [ ] Inspect current mesh UV/channel usage and shader data flow.
- [ ] Add or select a dedicated feature-atlas UV channel separate from the existing scalar/material-mask channel.
- [ ] Do not use `UV2.zw` or local X/Z projection as the feature-atlas foundation.
- [ ] Build deterministic generated surface-chart mapping for generated masses.
- [ ] Preserve triangle/edge adjacency metadata so semantic features can paint across shared edges and connected regions.
- [ ] Pack charts into FeatureAtlas0 with padding/gutters.
- [ ] Implement mask dilation/gutter fill to prevent bilinear bleeding between charts.
- [ ] Keep atlas resolution parameterized internally; use 256x256 as the preferred first default and prepare for 128/256/512 later.
- [ ] Use a linear/non-sRGB mask texture with clamp sampling.
- [ ] Define first packed channel layout: `FeatureAtlas0.R = ConvexEdgeWear`, with G/B/A reserved for future features.
- [ ] Add generated feature atlas runtime/editor texture ownership and safe cleanup in edit mode and play mode.
- [ ] Assign the atlas through the generated-mass renderer/material data path without mutating shared material assets.
- [ ] Add main shader sampling through the dedicated feature-atlas UV channel.
- [ ] Add debug view support that displays Atlas0.R on the main mass surface.
- [ ] Bake ConvexEdgeWear into Atlas0.R using shared/reused convex-edge candidate logic where possible.
- [ ] Paint selected convex edges into both adjacent triangle charts when both sides exist.
- [ ] Keep existing raised edge/crease carriers debug-only and visibly separate from atlas debug validation.
- [ ] Keep normal rendering unchanged.
- [ ] Do not add a final edge-wear tint/value/smoothness response in this patch.
- [ ] Do not add duplicate final-render feature meshes.
- [ ] Do not add archetype-specific feature response modes.
- [ ] Update feature contracts to reference surface-chart atlas storage where applicable.

### Framework implementation tasks

- [x] Add or formalize Generic Test Mass recipe. *(Patch 14B: added `GeneratedMassFeatureRecipe.GenericTestMass`.)*
- [x] Add explicit recipe application UI. *(Patch 14B: `Recipe & Feature Stack` inspector section.)*
- [x] Add `Apply Recipe to Feature Controls` behavior. *(Patch 14B: explicit Apply Selected Recipe button.)*
- [x] Add `Reset Feature Controls to Current Recipe` behavior. *(Patch 14B: Reset Controls to Recipe button.)*
- [x] Add Modified/Custom state when manual overrides differ from recipe defaults. *(Patch 14B: read-only Recipe Status help box.)*
- [x] Define how recipe values are stored. *(Patch 14B: built-in recipe values are code-defined for the current scaffold; future data assets remain possible.)*
- [x] Define how feature groups are displayed in the inspector. *(Patch 14B.1: existing controls are grouped by owning feature foldout: Exposure, Base / Contact, Crevice / Shelter, Dirt / Deposit, Edge Wear, and Crease / Crack Debug. Global controls remain outside the feature stack.)*
- [ ] Define how feature debug views are selected.
- [ ] Implement Generated Mass Surface-Chart Feature Atlas foundation. *(Prerequisite before serious final feature work such as edge wear, pitting, cracks, frost, or water wear.)*
- [x] Audit existing controls and map them into feature groups. *(Patch 14B.1: existing shape/strength/tint controls have been remapped under their owning feature rather than category buckets.)*
- [ ] Keep old generated-stone material profiles available only as transitional recipes or historical presets.

---

## 3. Current existing / partial features

### SurfaceVariation

Status: Implemented: Final Response Partial

Existing data:

- deterministic per-surface/per-rock variation, currently used by shader response.

Minimum controls:

- [x] existing response is present indirectly through material/profile controls.
- [ ] formal Feature Strength control.
- [ ] formal Feature Scale control.

Debug / validation:

- [ ] confirm existing debug mode coverage.
- [ ] add/rename debug view if needed.

Recipe readiness:

- [ ] define Generic Test Mass value.
- [ ] define Cold Grey Stone value.
- [ ] define Wet River Stone value.
- [ ] define Pale Frost Stone value.
- [ ] define Black Sacred Stone value.

Notes:

- This should remain a core general feature.

---

### Exposure

Status: Implemented: Final Response Partial

Existing data:

- generated/upward/exposed mask.
- used by exposure response/tint/lift.

Minimum controls:

- [x] Exposure Strength.
- [x] Exposure Tint.
- [x] Exposure Tint Strength.
- [ ] formal Exposure Coverage/Bias control if needed.

Debug / validation:

- [x] existing mask debug expected.
- [ ] verify debug view is clear in current inspector.

Recipe readiness:

- [ ] define Generic Test Mass value.
- [ ] define Cold Grey Stone value.
- [ ] define Wet River Stone value.
- [ ] define Pale Frost Stone value.
- [ ] define Black Sacred Stone value.

Potential future uses:

- frost catch;
- dry dust;
- sun bleaching;
- exposed mineral response.

---

### CreviceBase

Status: Implemented: Final Response Partial

Existing data:

- lower/sheltered/base mask.
- used by crevice/base grounding and visual response.

Minimum controls:

- [x] Crevice Height.
- [x] Crevice Fade.
- [x] Crevice Irregularity.
- [x] Crevice Strength.
- [x] Crevice Tint.
- [x] Crevice Tint Strength.

Debug / validation:

- [x] existing mask debug expected.
- [ ] verify current debug readability after framework transition.

Recipe readiness:

- [ ] define Generic Test Mass value.
- [ ] define Cold Grey Stone value.
- [ ] define Wet River Stone value.
- [ ] define Pale Frost Stone value.
- [ ] define Black Sacred Stone value.

Potential future uses:

- dirt fill;
- ice-in-crevice;
- wetness accumulation;
- mineral deposit;
- carved seam fill.

---

### DirtDeposit

Status: Implemented: Final Response Partial

Existing data:

- deposit/stain mask.
- currently partly mesh/shader driven.

Minimum controls:

- [x] Dirt Crawl Height.
- [x] Dirt Coverage.
- [x] Dirt Deposit Strength.
- [x] Dirt Deposit Tint.
- [x] Dirt Deposit Tint Strength.
- [ ] formal Dirt Deposit Breakup control if needed.

Debug / validation:

- [x] existing mask debug expected.
- [ ] verify debug view and final response separately.

Recipe readiness:

- [ ] define Generic Test Mass value.
- [ ] define Cold Grey Stone value.
- [ ] define Wet River Stone value.
- [ ] define Pale Frost Stone value.
- [ ] define Black Sacred Stone value.

Potential future uses:

- grime;
- mineral stain;
- wet deposit;
- dust;
- moss/lichen base if later desired.

---

### ConvexEdgeWear

Status: Implemented: Debug/Data Only; blocked on Feature Mask Atlas for final response

Existing data:

- debug feature-line data exists.
- current raised overlay strips are not accepted as final rendering.
- final edge wear should be baked into the generated feature mask atlas and interpreted by the main generated-mass shader.

Minimum controls:

- [x] Edge Wear Amount.
- [x] Edge Wear Width.
- [x] Edge Wear Coverage.
- [x] Edge Wear Softness.
- [ ] final Edge Wear Strength control if different from debug amount.
- [ ] generic Edge Wear Value Shift / Tint / Smoothness Offset controls if needed.

Debug / validation:

- [x] existing debug strip visualization.
- [ ] add generated atlas channel debug/final comparison view.

Recipe readiness:

- [ ] define Generic Test Mass value.
- [ ] define Cold Grey Stone generic values.
- [ ] define Wet River Stone generic values after WaterWear / FlowPolish exist.
- [ ] define Pale Frost Stone generic values after FrostAccumulation / BrittleChipping exist.
- [ ] define Black Sacred Stone generic values after SacredPlaneControl exists.

Failure cases:

- floating raised lines;
- excessive white outlines;
- visible duplicate final-render strip meshes;
- feature response modes hardcoded for a single archetype;
- feature strips that ignore surface material identity.

---

### ConcaveCrease

Status: Implemented: Debug/Data Only

Existing data:

- debug feature-line data exists.
- current raised overlay strips are not accepted as final rendering.

Minimum controls:

- [x] Crease Amount.
- [x] Crease Width.
- [x] Crease Length.
- [x] Crease Branching.
- [x] Crease Softness.
- [ ] final Crease / Crack Strength control if different from debug amount.

Debug / validation:

- [x] existing debug strip visualization.
- [ ] add surface-integrated debug/final comparison view.

Recipe readiness:

- [ ] define generic crease meaning.
- [ ] define use in crack network.
- [ ] define use in carved seam / ruin stone.
- [ ] define use in frost stress.

Failure cases:

- floating raised seams;
- random scribbles;
- visual cracks unrelated to mass shape.

---

### Surface Mottle / Material Breakup

Status: Implemented: Final Response Partial

Existing data:

- shader-side material mottle introduced in Patch 13.
- current result is infrastructure, not sufficient archetype identity.

Minimum controls:

- [x] Stone Mottle Strength.
- [x] Stone Mottle Scale.
- [x] Stone Mottle Softness.
- [x] Stone Mottle Shelter Bias.
- [ ] rename/generalize away from `Stone` once framework implementation begins.

Debug / validation:

- [ ] add explicit mottle debug view if needed.

Recipe readiness:

- [ ] Generic Test Mass values.
- [ ] Cold Grey Stone values.
- [ ] Wet River Stone values.
- [ ] Pale Frost Stone values.
- [ ] Black Sacred Stone values.

Failure cases:

- muddy noise;
- square-only pixel pattern;
- mottle hidden by wet/frost/monolithic response.

---

## 4. Planned general material features

### MineralDeposit

Status: Planned

Feature contract:

- [ ] Define semantic meaning.
- [ ] Decide generation method.
- [ ] Decide data storage.
- [ ] Add controls.
- [ ] Add debug view.
- [ ] Add final shader interpretation.
- [ ] Add recipe values.

Minimum controls:

- [ ] Strength.
- [ ] Coverage.
- [ ] Tint / Colour.

Possible advanced controls:

- [ ] Crevice bias.
- [ ] Waterline bias.
- [ ] Streak length.
- [ ] Breakup.

Likely recipes:

- Wet River Stone;
- Broken Ruin Block;
- Ore / Mineral Chunk.

---

### DryDust

Status: Planned

Feature contract:

- [ ] Define semantic meaning.
- [ ] Decide generation method.
- [ ] Add controls.
- [ ] Add debug view.
- [ ] Add final shader interpretation.
- [ ] Add recipe values.

Minimum controls:

- [ ] Strength.
- [ ] Exposure Bias.

Possible advanced controls:

- [ ] Tint.
- [ ] Coverage.
- [ ] Breakup.

Likely recipes:

- Cold Grey Stone;
- Ruin fragments;
- dry biome variants.

---

## 5. Planned edge, crease, and fracture features

### Surface-Integrated Edge Wear

Status: Planned; depends on Generated Mass Feature Mask Atlas Foundation

Purpose:

- replace debug-only raised edge strips with final surface-integrated generic edge-wear data sampled by the main generated-mass shader.

Implementation tasks:

- [ ] Complete feature mask atlas foundation first.
- [ ] Inspect existing ConvexEdgeWear data quality.
- [ ] Bake ConvexEdgeWear into atlas channel.
- [ ] Add main-shader generic edge response.
- [ ] Add final response controls.
- [ ] Add atlas debug/final comparison view.
- [ ] Add recipe values only after Generic Test Mass validation.

Minimum controls:

- [ ] Strength.
- [ ] Width.
- [ ] Coverage.

Possible advanced controls:

- [ ] Softness.
- [ ] Breakup.
- [ ] Value Shift.
- [ ] Tint.
- [ ] Tint Strength.
- [ ] Smoothness / roughness offset.

Recipe usage notes:

- [ ] Cold Grey can initially use generic neutral abrasion values.
- [ ] Wet River should not receive special edge behavior until WaterWear / FlowPolish exist.
- [ ] Pale Frost should not receive special edge behavior until FrostAccumulation / BrittleChipping exist.
- [ ] Black Sacred should not receive special edge behavior until SacredPlaneControl exists.

Failure cases:

- [ ] floating strips.
- [ ] bright cartoon outlines.
- [ ] duplicate final-render edge meshes.
- [ ] hardcoded archetype-specific edge modes.
- [ ] same response forced onto every recipe.

---

### Surface-Integrated Crease / Seam

Status: Planned; likely depends on Generated Mass Feature Mask Atlas Foundation

Purpose:

- replace debug-only raised crease strips with actual surface-integrated generic crease/seam data sampled by the main generated-mass shader.

Implementation tasks:

- [ ] Inspect existing ConcaveCrease data quality.
- [ ] Decide whether initial generic response is darkening, shallow groove, fill, or material change.
- [ ] Decide atlas channel/storage.
- [ ] Add controls.
- [ ] Add atlas debug/final comparison view.
- [ ] Add recipe values only after Generic Test Mass validation.

Minimum controls:

- [ ] Strength.
- [ ] Width.
- [ ] Length/Density.

Possible advanced controls:

- [ ] Branching.
- [ ] Fill material.
- [ ] Depth illusion.
- [ ] Softness.

Failure cases:

- [ ] floating raised lines.
- [ ] random decorative scribbles.
- [ ] cracks that ignore mass topology.

---

### CrackNetwork

Status: Planned

Purpose:

- create coherent non-floating crack networks.

Implementation tasks:

- [ ] Define crack-generation strategy.
- [ ] Decide shader-only vs mesh-assisted vs hybrid.
- [ ] Add controls.
- [ ] Add debug view.
- [ ] Add final response.
- [ ] Validate on Generic Test Mass.

Minimum controls:

- [ ] Strength.
- [ ] Density.
- [ ] Width.

Advanced controls:

- [ ] Branching.
- [ ] Length.
- [ ] Directionality.
- [ ] Fill/darkness.
- [ ] Age/weathering.

Likely recipes:

- Pale Frost Stone;
- Broken Ruin Block;
- Bone / Fossil Fragment;
- controlled Black Sacred Stone variants.

---

### BrittleChipping

Status: Planned

Purpose:

- sharper broken/chipped regions, especially for frost, ruin, bone, and crystal-like masses.

Implementation tasks:

- [ ] Define chip mask generation.
- [ ] Decide whether chips affect geometry, shader only, or both.
- [ ] Add controls.
- [ ] Add debug view.
- [ ] Add final response.
- [ ] Validate on Generic Test Mass.

Minimum controls:

- [ ] Strength.
- [ ] Coverage.
- [ ] Chip Size.

Advanced controls:

- [ ] Edge bias.
- [ ] Fracture sharpness.
- [ ] Fresh interior tint/response.

---

## 6. Planned water and erosion features

### WaterWear

Status: Planned

Purpose:

- broad water-eroded stone behavior.

Implementation tasks:

- [ ] Define water-wear semantic mask.
- [ ] Decide edge/base/side biases.
- [ ] Add controls.
- [ ] Add debug view.
- [ ] Add final response.
- [ ] Validate on Generic Test Mass.

Minimum controls:

- [ ] Strength.
- [ ] Coverage.

Advanced controls:

- [ ] Edge softening.
- [ ] Lower/contact bias.
- [ ] Direction bias.
- [ ] Deposit interaction.

Likely recipes:

- Wet River Stone;
- shoreline stones;
- ice/water hybrid masses.

Failure cases:

- [ ] global mirror polish.
- [ ] black glass look.
- [ ] no visible stone texture.

---

### FlowPolish

Status: Planned

Purpose:

- directional smoothing/streaking from water flow or repeated abrasion.

Implementation tasks:

- [ ] Define initial direction source.
- [ ] Decide later river-flow integration contract.
- [ ] Add controls.
- [ ] Add debug view.
- [ ] Add final response.
- [ ] Validate on Generic Test Mass.

Minimum controls:

- [ ] Strength.
- [ ] Direction.

Advanced controls:

- [ ] Streak Scale.
- [ ] Streak Length.
- [ ] Roughness Reduction.
- [ ] Side/Exposure Bias.

Direction options to evaluate:

- [ ] object-space fixed direction;
- [ ] world-space direction;
- [ ] author-set direction;
- [ ] future river-flow direction.

---

### SurfacePitting

Status: Planned

Purpose:

- holes, pores, erosion pits, or void-like surface details.

Implementation tasks:

- [ ] Define pitting scale ranges.
- [ ] Decide shader-only vs mesh-assisted pitting.
- [ ] Add controls.
- [ ] Add debug view.
- [ ] Add final response.
- [ ] Validate on Generic Test Mass.

Minimum controls:

- [ ] Strength.
- [ ] Density.
- [ ] Scale.

Advanced controls:

- [ ] Depth/Darkness.
- [ ] Wetness Interaction.
- [ ] Edge/Base Bias.
- [ ] Seed/Randomness.

Likely recipes:

- Wet River Stone;
- Bone / Fossil Fragment;
- dark volcanic-like stone;
- old ruin fragments.

---

### WaterlineStain

Status: Planned

Purpose:

- repeated-water-contact staining.

Implementation tasks:

- [ ] Define whether waterline is object-local, river-derived, or author-set.
- [ ] Add controls.
- [ ] Add debug view.
- [ ] Add final response.
- [ ] Validate on Generic Test Mass.

Minimum controls:

- [ ] Strength.
- [ ] Height / Position.

Advanced controls:

- [ ] Softness.
- [ ] Breakup.
- [ ] Tint.
- [ ] River contact integration.

---

## 7. Planned frost / ice features

### FrostAccumulation

Status: Planned

Purpose:

- coherent pale frost accumulation on exposed/upward surfaces.

Implementation tasks:

- [ ] Define accumulation mask.
- [ ] Avoid triangle/facet visualization artifacts.
- [ ] Add controls.
- [ ] Add debug view.
- [ ] Add final response.
- [ ] Validate on Generic Test Mass.

Minimum controls:

- [ ] Strength.
- [ ] Coverage.

Advanced controls:

- [ ] Exposure Bias.
- [ ] Slope Threshold.
- [ ] Softness.
- [ ] Tint.
- [ ] Breakup.

Failure cases:

- [ ] saturated blue noise.
- [ ] brown/blue random patches.
- [ ] visible internal triangulation.

---

### IceInCrevices

Status: Planned

Purpose:

- pale/cold fill in crevices and cracks.

Implementation tasks:

- [ ] Define dependence on CreviceBase / ConcaveCrease / CrackNetwork.
- [ ] Add controls.
- [ ] Add debug view.
- [ ] Add final response.
- [ ] Validate on Generic Test Mass.

Minimum controls:

- [ ] Strength.
- [ ] Crevice Bias.

Advanced controls:

- [ ] Tint.
- [ ] Width.
- [ ] Softness.
- [ ] Glint/Roughness Response.

---

### FrostStress

Status: Planned

Purpose:

- brittle stress-line or fracture tendency from freezing.

Implementation tasks:

- [ ] Decide relationship to CrackNetwork.
- [ ] Add controls.
- [ ] Add debug view.
- [ ] Add final response.
- [ ] Validate on Generic Test Mass.

Minimum controls:

- [ ] Strength.
- [ ] Density.

Advanced controls:

- [ ] Branching.
- [ ] Directionality.
- [ ] Link to brittle chipping.

---

### MeltRefreezeWetness

Status: Planned

Purpose:

- cold dark wetness from melting/refreezing.

Implementation tasks:

- [ ] Define relation to WetnessAccumulation.
- [ ] Add controls.
- [ ] Add debug view.
- [ ] Add final response.
- [ ] Validate on Generic Test Mass.

Minimum controls:

- [ ] Strength.
- [ ] Shelter/Base Bias.

Advanced controls:

- [ ] Gloss response.
- [ ] Darkness.
- [ ] Frost interaction.

---

## 8. Planned sacred / stylized features

### SacredPlaneControl

Status: Planned

Purpose:

- deliberate monolithic/ritual plane language.

Implementation tasks:

- [ ] Define whether this is geometry, shader, or hybrid.
- [ ] Add controls.
- [ ] Add debug view if practical.
- [ ] Add final response.
- [ ] Validate on Generic Test Mass.

Minimum controls:

- [ ] Strength.
- [ ] Plane Consistency.

Advanced controls:

- [ ] Symmetry/Asymmetry.
- [ ] Surface Variation Suppression.
- [ ] Edge Restraint.

---

### MonolithicFlatten

Status: Implemented: Final Response Partial

Existing data:

- shader/material profile value exists from previous stone profile work.

Framework tasks:

- [ ] Rename/generalize as feature channel if needed.
- [ ] Add explicit controls in feature stack.
- [ ] Define recipe values.
- [ ] Preserve subtle surface breakup instead of erasing all texture.

Minimum controls:

- [ ] Strength.

Advanced controls:

- [ ] Preserve Edge Accent.
- [ ] Preserve Mottle.
- [ ] Roughness Response.

---

### CarvedSeam

Status: Planned / Deferred until crease/crack integration is solved

Purpose:

- intentional carved seam, groove, glyph-like or ritual line feature.

Implementation tasks:

- [ ] Wait for surface-integrated crease/crack solution.
- [ ] Define carved seam generation.
- [ ] Add controls.
- [ ] Add debug view.
- [ ] Add final response.
- [ ] Validate on Generic Test Mass.

Minimum controls:

- [ ] Strength.
- [ ] Width.

Advanced controls:

- [ ] Pattern Type.
- [ ] Depth.
- [ ] Fill Material.
- [ ] Sacred Recipe Bias.

Failure cases:

- [ ] floating raised lines.
- [ ] unreadable random glyph noise.

---

### AccentGlow / RuneGlow

Status: Deferred

Purpose:

- stylized magical or ritual accent response.

Implementation tasks:

- [ ] Do not implement until physical/surface feature framework is stable.
- [ ] Define whether this belongs in Generated Mass or a separate effect system.

Minimum controls if ever implemented:

- [ ] Strength.
- [ ] Colour.
- [ ] Emission softness.

---

## 9. Planned non-stone compact-mass features

### BonePorosity

Status: Planned / Future

Minimum controls:

- [ ] Strength.
- [ ] Density.
- [ ] Scale.

Tasks:

- [ ] Define relationship to SurfacePitting.
- [ ] Decide if this is just a recipe interpretation of SurfacePitting or a separate feature.

---

### BoneRidgeWear

Status: Planned / Future

Minimum controls:

- [ ] Strength.
- [ ] Ridge Bias.

Tasks:

- [ ] Define ridge detection or reuse existing edge/exposure data.

---

### CrystalFacetClarity

Status: Planned / Future

Minimum controls:

- [ ] Strength.
- [ ] Facet Clarity.

Tasks:

- [ ] Decide if crystals fit Generated Mass or require a specialized compact-crystal module.

---

### OreVein

Status: Planned / Future

Minimum controls:

- [ ] Strength.
- [ ] Density.
- [ ] Colour / Material Response.

Tasks:

- [ ] Define vein generation method.
- [ ] Decide relation to CrackNetwork / MineralDeposit.

---

### RuinChipping

Status: Planned / Future

Minimum controls:

- [ ] Strength.
- [ ] Edge Bias.

Tasks:

- [ ] Decide relation to BrittleChipping.
- [ ] Add recipe usage for Broken Ruin Block.

---

## 10. Initial recipe checklist

### Generic Test Mass

Status: Planned

- [ ] Create recipe definition.
- [ ] Add recipe to UI.
- [ ] Set all special archetype features off by default.
- [ ] Keep core masks visible/testable.
- [ ] Use as required validation target for new features.

### Cold Grey Stone

Status: Existing transitional profile / recipe not yet formalized

- [ ] Convert from material profile to editable recipe.
- [ ] Define SurfaceVariation value.
- [ ] Define SurfaceMottle value.
- [ ] Define Exposure value.
- [ ] Define CreviceBase value.
- [ ] Define DirtDeposit value.
- [ ] Define EdgeWear interpretation.
- [ ] Keep water/frost/sacred features off.

### Wet River Stone

Status: Existing transitional profile / recipe failed as material-only approach

- [ ] Convert from material profile to editable recipe.
- [ ] Add WaterWear value.
- [ ] Add FlowPolish value.
- [ ] Add WetnessAccumulation value.
- [ ] Add SurfacePitting value.
- [ ] Add MineralDeposit value.
- [ ] Add softened EdgeWear interpretation.
- [ ] Ensure reflectivity is local/rough, not global glass.

### Pale Frost Stone

Status: Existing transitional profile / recipe failed as material-only approach

- [ ] Convert from material profile to editable recipe.
- [ ] Add FrostAccumulation value.
- [ ] Add IceInCrevices value.
- [ ] Add FrostStress value.
- [ ] Add CrackNetwork value.
- [ ] Add BrittleChipping value.
- [ ] Keep blue tint restrained.
- [ ] Avoid triangle/facet artifacts.

### Black Sacred Stone

Status: Existing transitional profile / recipe failed as material-only approach

- [ ] Convert from material profile to editable recipe.
- [ ] Add SacredPlaneControl value.
- [ ] Add MonolithicFlatten value.
- [ ] Add restrained EdgeWear interpretation.
- [ ] Add optional future CarvedSeam value.
- [ ] Keep wet/frost features off.
- [ ] Make identity distinct from Wet River Stone.

### Ice Chunk

Status: Planned / Future

- [ ] Decide if Ice is a mass type, recipe, or both.
- [ ] Define FrostAccumulation value.
- [ ] Define CrackNetwork value.
- [ ] Define BrittleChipping value.
- [ ] Define ice clarity/translucency approach.

### Broken Ruin Block

Status: Planned / Future

- [ ] Decide if Ruin Stone is a mass type, recipe, or both.
- [ ] Define Sacred/Constructed Plane Control value.
- [ ] Define RuinChipping value.
- [ ] Define CarvedSeam value.
- [ ] Define DirtDeposit/MineralDeposit values.

### Bone / Fossil Fragment

Status: Planned / Future

- [ ] Decide if Bone/Fossil belongs in Generated Mass.
- [ ] Define BonePorosity value.
- [ ] Define BoneRidgeWear value.
- [ ] Define DirtDeposit value.
- [ ] Define CrackNetwork value.

### Ore / Mineral Chunk

Status: Planned / Future

- [ ] Decide if Ore/Mineral belongs in Generated Mass.
- [ ] Define OreVein value.
- [ ] Define SurfaceVariation value.
- [ ] Define MineralDeposit value.
- [ ] Define edge/facet response.

---

## 11. Validation checklist for every new feature

Before a feature can be used by archetype recipes:

- [ ] It has a written semantic definition.
- [ ] It has at least one control.
- [ ] It has clear default values.
- [ ] It has a debug or validation view where practical.
- [ ] It is tested on Generic Test Mass.
- [ ] Its raw mask/data is acceptable before final render tuning.
- [ ] Its final visual response is tested separately from the debug view.
- [ ] It has documented recipe values or recipe-use notes.
- [ ] It has documented failure cases.
- [ ] It does not introduce one-off archetype-specific code unless explicitly approved.
- [ ] It does not silently remove or override existing manual controls.
- [ ] It uses the generated surface-chart feature mask atlas for final surface masks where practical.
- [ ] If it is atlas-backed, it validates chart padding/dilation and does not visibly bleed between packed charts.
- [ ] It does not add unacceptable runtime cost.

---

## 12. Current immediate next implementation candidates

Do not start all of these at once. Pick one after code inspection.

Recommended order after Patch 14B.1:

1. Generated Mass Surface-Chart Feature Atlas Foundation.
2. Generic Edge Wear via Feature Atlas.
3. Surface Pitting via Feature Atlas.
4. WaterWear / FlowPolish.
5. FrostAccumulation.
6. CrackNetwork / Surface-integrated Crease.
7. SacredPlaneControl / CarvedSeam.

Current recommendation:

```text
Implement the generated surface-chart feature mask atlas foundation before adding more final visual feature complexity.
```

Rules for this next phase:

- Do not add duplicate final-render feature meshes when a feature can be baked into the atlas.
- Do not use local X/Z projection or packed scalar-mask channels as the feature-atlas foundation.
- Keep old edge/crease strip carriers debug-only unless a specific exception is approved.
- Keep feature controls generic and archetype-agnostic.
- Build recipe outcomes by combining generic feature controls, not by adding Wet/Frost/Sacred modes inside each feature.
