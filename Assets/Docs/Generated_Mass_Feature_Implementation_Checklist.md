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
- [x] Remove shared raised-overlay visibility because legacy secondary feature meshes are no longer part of the GeneratedMass path.
- [x] Do not add new feature channels.
- [x] Do not change shader/material behavior.
- [x] Update framework docs to make feature-oriented grouping foundational.

### Patch 14C — Generated Mass Surface-Chart Feature Atlas Foundation

Patch 14C is documentation-approved as a data/debug foundation patch. It should not implement the final artistic edge-wear response.

- [x] Inspect current mesh UV/channel usage and shader data flow.
- [x] Add or select a dedicated feature-atlas UV channel separate from the existing scalar/material-mask channel. *(Patch 14C.1 uses Unity mesh channel 3 / shader `TEXCOORD3` for generated-mass feature atlas UVs.)*
- [x] Do not use `UV2.zw` or local X/Z projection as the feature-atlas foundation.
- [x] Build deterministic generated surface-chart mapping for generated masses, corrected in Patch 14C.2 to use surface-patch charts rather than per-triangle decal charts.
- [x] Preserve triangle/edge adjacency metadata so semantic features can paint across shared edges and connected regions.
- [x] Pack charts into FeatureAtlas0 with padding/gutters.
- [x] Add chart padding/gutters. Patch 14C.2 avoids global dilation and bakes ridge/crease distance fields inside packed surface-patch charts. Patch 14C.3 adds semantic gutter fill so chart boundaries near ridges/creases do not sample black.
- [x] Keep atlas resolution parameterized internally; Patch 14C.3 uses 512x512 as the current quality-oriented default while preserving 128/256/512 as future quality tiers.
- [x] Use a linear/non-sRGB mask texture with clamp sampling.
- [x] Define first packed channel layout. Patch 14C.4 splits proximity from weight: `FeatureAtlas0.R = Convex ridge proximity`, `G = Convex ridge weight`, `B = Concave crease proximity`, `A = Concave crease weight`.
- [x] Add generated feature atlas runtime/editor texture ownership and safe cleanup in edit mode and play mode.
- [x] Assign the atlas through the generated-mass renderer/material data path without mutating shared material assets.
- [x] Add main shader sampling through the dedicated feature-atlas UV channel.
- [x] Add debug view support that displays atlas-backed masks on the main mass surface. Patch 14C.4 displays ConvexEdgeWear as `Atlas0.R * Atlas0.G` and ConcaveCrease as `Atlas0.B * Atlas0.A`; Patch 14C.5 adds raw channel diagnostics for `R`, `G`, `B`, `A`, both composites, and an RGB boundary-field diagnostic view.
- [x] Bake ConvexEdgeWear semantic data into Atlas0.R/G using shared/reused convex-edge candidate logic where possible.
- [x] Paint selected convex edges into both adjacent triangle charts when both sides exist and chart texel density is sufficient; skip tiny charts rather than emitting triangle wedges.
- [x] Remove existing raised edge/crease carriers from the GeneratedMass path; ConvexEdgeWear and ConcaveCrease validation are both main-surface atlas debug channels.
- [x] Keep normal rendering unchanged.
- [x] Do not add a final edge-wear tint/value/smoothness response in this patch.
- [x] Do not add duplicate final-render feature meshes.
- [x] Do not add archetype-specific feature response modes.
- [x] Update feature contracts to reference surface-chart atlas storage where applicable.

### Framework implementation tasks

- [x] Add or formalize Generic Test Mass recipe. *(Patch 14B: added `GeneratedMassFeatureRecipe.GenericTestMass`.)*
- [x] Add explicit recipe application UI. *(Patch 14B: `Recipe & Feature Stack` inspector section.)*
- [x] Add `Apply Recipe to Feature Controls` behavior. *(Patch 14B: explicit Apply Selected Recipe button.)*
- [x] Add `Reset Feature Controls to Current Recipe` behavior. *(Patch 14B: Reset Controls to Recipe button.)*
- [x] Add Modified/Custom state when manual overrides differ from recipe defaults. *(Patch 14B: read-only Recipe Status help box.)*
- [x] Define how recipe values are stored. *(Patch 14B: built-in recipe values are code-defined for the current scaffold; future data assets remain possible.)*
- [x] Define how feature groups are displayed in the inspector. *(Patch 14B.1: existing controls are grouped by owning feature foldout: Exposure, Base / Contact, Crevice / Shelter, Dirt / Deposit, Edge Wear, and Crease / Crack Debug. Global controls remain outside the feature stack.)*
- [ ] Define how feature debug views are selected.
- [x] Implement Generated Mass Surface-Chart Feature Atlas foundation. *(Prerequisite before serious final feature work such as edge wear, pitting, cracks, frost, or water wear.)*
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

Status: Implemented: Atlas data + first generic visual response

Existing data:

- ConvexEdgeWear uses `GeneratedMassFeatureAtlas0.R` for convex ridge proximity.
- ConvexEdgeWear uses `GeneratedMassFeatureAtlas0.G` for convex ridge weight / importance.
- legacy raised overlay strips have been removed.
- Patch 14D adds the first generic normal-render material response using the existing atlas data.

Data Field / Atlas Bake controls:

- [x] Edge Wear Amount.
- [x] Edge Wear Width.
- [x] Edge Wear Coverage.
- [x] Edge Wear Softness.

Visual Response controls:

- [x] Response Strength.
- [x] Brightness Lift.
- [x] Worn Edge Tint.
- [x] Tint Influence.
- [x] Breakup.
- [ ] Smoothness Offset: deferred until colour/value response is validated.
- [ ] Falloff Contrast / Breakup Scale: deferred to avoid control clutter unless validation proves they are needed.

Debug / validation:

- [x] generated atlas channel debug view on the main mass surface.
- [x] legacy debug strip visualization removed.
- [x] common debug view remains concise.
- [x] raw boundary diagnostics live in an Advanced Feature Diagnostics foldout.
- [ ] validate normal-render edge-wear response on Generic Test Mass and Cold Grey Stone.

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

Status: Reserved: Awaiting Atlas Channel

Existing data:

- controls remain reserved for future crease/crack work.
- legacy raised overlay strips have been removed.
- visual crease debug now exists as atlas-backed main-surface data in `GeneratedMassFeatureAtlas0.B/A`; final darkening response is still deferred.

Minimum controls:

- [x] Crease Amount.
- [x] Crease Width.
- [x] Crease Length.
- [x] Crease Branching.
- [x] Crease Softness.
- [ ] final Crease / Crack Strength control if different from debug amount.

Debug / validation:

- [x] legacy debug strip visualization removed.
- [ ] add surface-integrated atlas debug/final comparison view.

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

Status: First generic response implemented in Patch 14D

Purpose:

- interpret generated convex ridge semantic fields as a surface-integrated worn-edge material response sampled by the main generated-mass shader.

Implementation tasks:

- [x] Complete feature mask atlas foundation first.
- [x] Inspect existing ConvexEdgeWear data quality.
- [x] Bake ConvexEdgeWear into atlas proximity/weight channels.
- [x] Add main-shader generic edge response.
- [x] Add reduced final response controls.
- [x] Keep raw atlas debug available while preventing the main debug selector from becoming a long control sausage.
- [ ] Validate recipe values on Generic Test Mass and Cold Grey Stone.

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

- continue from removed legacy raised crease strips and implement actual surface-integrated generic crease/seam data sampled by the main generated-mass shader.

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

### Patch 14C.2 — Surface-Patch Atlas + Convex/Concave Ridge Fields

- [x] Replace per-triangle chart atlas baking with surface-patch chart baking.
- [x] Build a quantized-position surface graph from the generated mass mesh.
- [x] Classify boundaries as open border, flat/internal, convex ridge, concave crease, or ambiguous.
- [x] Flood-fill patches across flat/internal boundaries only.
- [x] Pack one chart per surface patch rather than one chart per triangle.
- [x] Keep the original generated mass render mesh unchanged.
- [x] Assign feature atlas UVs to Unity mesh channel 3 / shader `TEXCOORD3`.
- [x] Bake convex ridge distance data into `GeneratedMassFeatureAtlas0.R`.
- [x] Bake concave crease distance data into `GeneratedMassFeatureAtlas0.G` in Patch 14C.2/14C.3; Patch 14C.4 moves concave proximity to B and concave weight to A.
- [x] Update ConvexEdgeWear debug to show the main-surface convex ridge field.
- [x] Update ConcaveCrease debug to show the main-surface concave crease field.
- [x] Keep normal rendering unchanged; final lightening/darkening response is deferred.
- [x] Keep legacy raised secondary feature meshes removed.
- [x] Leave `MeshData.cs` and `MeshBuilder.cs` untouched.

### Patch 14C.3 — Feature Graph / Ridge Field Hardening

- [x] Treat convex and concave boundaries as first-class semantic feature data rather than final-looking paint.
- [x] Add lightweight boundary-chain metadata so future effects can reason about continuous ridge/crease networks.
- [x] Keep `FeatureAtlas0.R` as clean convex ridge proximity, brightest at the ridge core with soft falloff onto neighboring patches.
- [x] Patch 14C.3 kept `FeatureAtlas0.G` as clean concave crease proximity; Patch 14C.4 splits this into `FeatureAtlas0.B = concave crease proximity` and `FeatureAtlas0.A = concave crease weight`.
- [x] Fill chart gutters semantically near ridge/crease boundaries so bilinear filtering does not pull black into the exact ridge or crease.
- [x] Remove baked decorative breakup/noise from the atlas data; coverage now controls semantic boundary eligibility rather than random line fragmentation, and final breakup belongs to material response.
- [x] Raise the current default atlas resolution to 512x512 while keeping the baker parameterized for later quality tiers.
- [x] Keep normal rendering unchanged; debug/data only.
- [x] Keep secondary feature meshes removed.
- [x] Leave `MeshData.cs` and `MeshBuilder.cs` untouched.


### Patch 14C.4 — Boundary Field Contract + Diagnostic Refinement

- [x] Keep the surface-patch atlas and surface graph architecture from Patch 14C.2/14C.3.
- [x] Split proximity from semantic importance so the atlas is no longer one final-looking mask channel.
- [x] Store `FeatureAtlas0.R = convex ridge proximity`.
- [x] Store `FeatureAtlas0.G = convex ridge weight / importance`.
- [x] Store `FeatureAtlas0.B = concave crease proximity`.
- [x] Store `FeatureAtlas0.A = concave crease weight / importance`.
- [x] Update ConvexEdgeWear debug to show `R * G` on the main mass surface.
- [x] Update ConcaveCrease debug to show `B * A` on the main mass surface.
- [x] Keep proximity fields clean and reusable; do not bake decorative breakup/noise into the semantic data.
- [x] Keep coverage as semantic boundary eligibility rather than random segment deletion.
- [x] Keep normal rendering unchanged; debug/data only.
- [x] Keep secondary feature meshes removed.
- [x] Leave `MeshData.cs` and `MeshBuilder.cs` untouched.

### Patch 14C.5 — Boundary Field Diagnostics + Proximity Refinement

- [x] Add raw atlas debug modes so the supporting data can be judged without guessing from one composite preview.
- [x] Add Convex Ridge Proximity debug: `FeatureAtlas0.R`.
- [x] Add Convex Ridge Weight debug: `FeatureAtlas0.G`.
- [x] Add Convex Ridge Composite debug: `FeatureAtlas0.R * FeatureAtlas0.G`.
- [x] Add Concave Crease Proximity debug: `FeatureAtlas0.B`.
- [x] Add Concave Crease Weight debug: `FeatureAtlas0.A`.
- [x] Add Concave Crease Composite debug: `FeatureAtlas0.B * FeatureAtlas0.A`.
- [x] Add Boundary Field Diagnostic RGB debug: `R = convex proximity`, `G = convex weight`, `B = concave proximity`.
- [x] Tighten the semantic proximity falloff so raw proximity diagnostics show a narrow ridge/crease core, visible mid-gradient, and softer outer fade instead of a broad binary strip.
- [x] Keep boundary weight stable per boundary for now. Along-ridge decorative variation remains a future material-response concern, not part of the semantic weight field.
- [x] Keep normal rendering unchanged. Patch 14C.5 is still data/debug-only.
- [x] Keep secondary feature meshes removed.
- [x] Leave `MeshData.cs` and `MeshBuilder.cs` untouched.

Future notes:

- Patch 14D interprets clean Atlas0.R/G convex data as the first generic convex edge-wear lightening response. Concave darkening remains deferred because current compact masses often lack meaningful concave boundary geometry.
- If shader response still reads as paint on hard geometry, evaluate shader-side ridge-normal support or dirty-time main-mesh bevel/chamfer generation. Do not solve that with secondary meshes.
- The graph/atlas system should remain reusable for future edge-adjacent or patch-adjacent effects, but future effects are not forced to use it if another representation is cleaner.



### Patch 14D — Convex Edge-Wear Material Response + Inspector Control Cleanup

Patch 14D begins normal-render interpretation of the 14C boundary-field data. It does not change the graph or atlas bake. The shader samples `FeatureAtlas0.R/G` once and interprets convex ridge proximity multiplied by convex ridge weight as a generic worn-edge response.

Implemented scope:

- [x] Add convex edge-wear material response driven by `FeatureAtlas0.R * FeatureAtlas0.G`.
- [x] Add response controls under `Edge Wear / Visual Response`: Response Strength, Brightness Lift, Worn Edge Tint, Tint Influence, and Breakup.
- [x] Keep Data Field / Atlas Bake controls separate from Visual Response controls in the editor.
- [x] Keep response disabled by default on existing serialized objects (`Response Strength = 0`).
- [x] Add visible recipe response defaults only for Generic Test Mass and Cold Grey Stone.
- [x] Keep ConcaveCrease as data/debug-only for now.
- [x] Keep the main debug selector concise and move raw boundary-channel inspection into Advanced Feature Diagnostics.

Deferred on purpose:

- [ ] Concave darkening response.
- [ ] Smoothness Offset.
- [ ] Falloff Contrast.
- [ ] Breakup Scale.
- [ ] Shader-side ridge-normal support.
- [ ] Generated main-mesh bevel/chamfer support.
- [ ] Pitting, water wear, frost, sacred features, and crack-network generation.

Validation target:

- `Response Strength = 0` should preserve normal rendering.
- Raising `Response Strength` should reveal lighter worn convex ridges without changing atlas data.
- `Brightness Lift`, `Tint Influence`, and `Breakup` should visibly affect only the convex ridge response.
- Existing raw debug modes should still show the same atlas fields.
