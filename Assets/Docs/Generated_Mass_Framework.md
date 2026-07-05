# Generated Mass Framework

Status: active framework definition  
Created for: Patch 14A — Generated Mass Framework Documentation  
Supersedes as active planning source: `Rock_Generated_Mass_Upgrade_Plan.md`

---

## 1. Purpose

The Generated Mass system should not be treated as a rock-only generator.

The long-term goal is a reusable framework for compact procedural masses: rocks, boulders, ice chunks, sacred monoliths, ruin fragments, ore chunks, bone-like chunks, crystal-like chunks, and other compact stylized objects that share the same broad topology problem.

The framework should provide:

- a shared compact-mass generation core;
- a reusable semantic feature library;
- editable feature controls;
- archetype recipes built from those controls;
- manual authoring after recipe application;
- debug views that validate feature data before final rendering;
- shader/material interpretation that can vary by mass type and archetype.

The core principle:

```text
Generated Mass is generic.
Feature modules and recipes provide specialization.
Archetypes are editable recipes, not sealed presets.
```

---

## 2. Why the old rock-profile direction is being dropped

The previous rock upgrade plan was useful for getting from a single generated rock material toward semantic masks and HLSL material response. It is no longer the right active planning document.

The failed/limited result of the material-profile approach is now clear:

```text
Same generated mass
+ different material smoothness/colour/tint values
= stock material variant, not a true archetype.
```

Patch 13 and Patch 13B confirmed that material knobs alone cannot produce convincing Wet River Stone, Pale Frost Stone, or Black Sacred Stone identities. Wet stone needs water-worn edges, flow-polished surfaces, pits, deposits, and local wetness. Frost stone needs coherent frost accumulation, ice/crevice behavior, brittle cracking, and chipping. Sacred stone needs deliberate planar control and restrained symbolic/monolithic language.

Those are not simple material values. They are feature recipes.

From this point forward, `Rock_Generated_Mass_Upgrade_Plan.md` should be treated as historical context only. The active design source is this Generated Mass Framework document plus the implementation checklist.

---

## 3. Scope: what belongs in Generated Mass

Use the Generated Mass Framework when the object is fundamentally a compact procedural volume/mass.

Good candidates:

- rocks and boulders;
- river stones;
- ice chunks;
- sacred standing stones or monolith-like chunks;
- ruin fragments and carved stone blocks;
- ore chunks;
- crystal chunks, if they remain compact masses;
- bone-like chunks or fossil fragments;
- compact clay, mud, or mineral clods;
- other stylized chunks with roughly convex or fractured mass topology.

Use a separate generator when the object has a fundamentally different structural topology.

Separate-generator candidates:

- trees, roots, vines, and branching plants;
- characters, creatures, limbs, or anatomy-driven forms;
- buildings with rooms, doors, windows, or internal structure;
- terrain chunks;
- rivers and water surfaces;
- ropes, cloth, chains, or long deformable objects;
- tools, weapons, and authored-proportion props;
- anything whose primary shape language is not compact mass topology.

Shared feature concepts can still be reused outside Generated Mass later, but the mass generator should not become a universal procedural object generator.

Rule:

```text
Same compact-mass topology -> Generated Mass Framework.
Different structural topology -> separate generator, with shared concepts only where useful.
```

---

## 4. Core terminology

### Generated Mass

A compact procedural object built from the shared mass-generation pipeline.

A Generated Mass owns or references:

- seed and variant state;
- base mesh/topology settings;
- semantic feature data;
- feature controls;
- archetype recipe selection;
- manual overrides;
- shader/material interpretation settings;
- debug/validation modes.

### Mass Type / Mass Family

A broad family that describes the material/structural domain.

Examples:

- Stone / Rock;
- Ice;
- Ruin Stone / Carved Stone;
- Bone / Fossil;
- Ore / Mineral;
- Crystal;
- Organic Clod, if later needed.

Mass Type should determine which feature groups are relevant, but it should not hide all manual controls forever.

### Feature Channel

A reusable semantic feature or mask.

Examples:

- Exposure;
- CreviceBase;
- DirtDeposit;
- ConvexEdgeWear;
- ConcaveCrease;
- SurfacePitting;
- FlowPolish;
- FrostAccumulation;
- CrackNetwork;
- SacredPlaneControl.

A feature channel is data. It is not automatically a final visual effect.

Bad interpretation:

```text
ConvexEdgeWear = draw a raised bright strip.
```

Correct interpretation:

```text
ConvexEdgeWear = this area has semantic edge-wear data.
The active recipe/material decides how that data appears.
```

### Feature Control

A user-facing control for a feature.

Every accepted feature must expose at least one meaningful control. Usually the minimum is Strength or Coverage. More controls are allowed when they are genuinely useful.

Feature controls must stay generic. Do not add archetype-specific response modes inside a generic feature. A feature may expose controls such as Strength, Width, Coverage, Breakup, Value Shift, Tint, Tint Strength, Smoothness Offset, or Roughness Offset. A feature should not expose controls such as Wet River Mode, Frost Catch Mode, or Sacred Accent Mode. Those outcomes must come from recipes combining generic controls and generic feature channels.

### Feature Mask Atlas

A generated, packed mask texture or texture set created during mass generation and sampled by the main mass shader.

The atlas is the preferred long-term storage for surface feature data that should appear on the main mass material without duplicate visible feature meshes.

The atlas must use a dedicated feature-atlas UV channel. Do not cram feature-atlas coordinates into the existing scalar/material-mask channel. The intended data separation is:

```text
Existing material/scalar mask channel
  broad generated-mass scalar masks such as crease reserve, dirt/deposit, and future scalar values

Dedicated feature-atlas UV channel
  generated chart coordinates for packed feature-mask textures
```

The foundation should use generated surface-chart mapping, not local X/Z projection. Local projection can validate a texture sample path, but it is not a foundation for vertical faces, steep facets, edge wear, cracks, pitting, frost patches, water streaks, stains, or carved seams. Patch 14C should therefore establish deterministic per-surface/per-triangle chart coordinates, chart packing, adjacency metadata, padding, and mask dilation as part of the generated-mass feature pipeline.

Initial Patch 14C atlas contract:

```text
GeneratedMassFeatureAtlas0
  R = Convex Edge Wear
  G = Concave Crease / Crack candidate reserve
  B = Surface Pitting reserve
  A = future broad/special feature reserve
```

Possible later atlas:

```text
GeneratedMassFeatureAtlas1
  R = Water Wear
  G = Flow Polish
  B = Frost Accumulation
  A = Sacred / Stylized / reserved
```

The exact channel packing may change after implementation pressure, but the architectural rule should not: expensive feature discovery and mask generation happen once during generation; runtime shader work should be cheap sampling and generic response application.

Mask textures should be treated as data, not colour art. The initial implementation should use a linear/non-sRGB mask texture, clamp sampling, chart padding/gutters, and dilation to avoid bilinear bleeding between packed charts. A 256x256 Atlas0 is the preferred first default, with the baker written as if 128/256/512 quality tiers can be exposed later.

### Archetype Recipe

A saved set of feature-control values.

Examples:

- Generic Test Mass;
- Cold Grey Stone;
- Wet River Stone;
- Pale Frost Stone;
- Black Sacred Stone;
- Frozen Chunk;
- Broken Ruin Block;
- Bone Fragment;
- Ore Chunk.

An archetype is the result of applying a recipe. It is not a sealed behavior branch.

### Manual Override

A user edit made after applying a recipe.

Manual authoring must remain possible. The user should be able to apply a Wet River Stone recipe and then increase pitting, reduce wetness, add light frost, or disable flow polish.

### Shader / Material Interpretation

The visual interpretation of feature data.

The same generic feature data can contribute to different recipe outcomes, but the feature itself must remain archetype-agnostic. Do not implement `if WetRiver`, `if PaleFrost`, or `if BlackSacred` branches inside a generic feature.

Preferred model:

```text
Feature channel: ConvexEdgeWear
Generic controls: Strength, Width, Coverage, Breakup, Value Shift, Tint, Tint Strength, Smoothness Offset
Recipe outcome: dry abrasion, water-eroded edge, frost catch, or dark accent emerges from recipe values and companion features.
```

Example recipe outcomes:

```text
Cold Grey Stone -> Edge Wear + light value shift + low tint = dry abrasion.
Wet River Stone -> Edge Wear + Water Wear + Flow Polish + local wetness = water-rounded erosion.
Pale Frost Stone -> Edge Wear + Frost Accumulation + Brittle Chipping = frosted/brittle edge.
Black Sacred Stone -> Edge Wear + Sacred Plane Control + Monolithic Flatten = restrained planar accent.
```

---

## 5. Global rules: DOs and DON'Ts

### DO

- Build reusable feature channels before building archetype-specific final looks.
- Test each feature on a generic test mass before using it in recipes.
- Keep features semantic first and visual second.
- Give every accepted feature at least one user-facing control.
- Keep manual authoring possible after applying a recipe.
- Use foldouts or grouped UI so advanced controls exist without flooding the main inspector.
- Maintain debug views for masks and feature channels where practical.
- Prefer shared masks, packed feature atlases, and fixed-cost representations over per-pixel/per-effect loops when possible.
- Precompute expensive feature discovery during mass generation; keep runtime shader work cheap.
- Preserve performance on low-to-medium desktop hardware.
- Let archetypes combine features rather than duplicating feature logic.
- Document which features are debug-only, final-ready, deferred, or rejected.
- Preserve existing accepted controls unless an explicit replacement plan is approved.
- Make old systems historical/deferred rather than silently deleting them.

### DON'T

- Do not hardcode one-off WetRiver/Frost/Sacred hacks inside the shader or generator.
- Do not add archetype-specific response modes inside generic features. Use recipes that combine generic feature controls instead.
- Do not make archetypes sealed presets that hide all underlying controls.
- Do not silently overwrite manual edits when changing an archetype dropdown.
- Do not treat material colour/smoothness values as sufficient archetype identity.
- Do not turn debug masks into final rendering without a deliberate interpretation pass.
- Do not use raised/floating feature strips as final cracks or edge wear.
- Do not add duplicate final-render feature meshes when feature data can be baked into the main generated mass mask atlas and interpreted by the main shader. Debug meshes may still exist as debug-only validation tools.
- Do not add dozens of always-visible sliders to the main inspector.
- Do not change mesh-generation behavior for an archetype until the feature contract is documented.
- Do not add new layers, tags, components, or project architecture without explicit approval.
- Do not remove old controls or serialized fields unless a migration/removal plan is approved.
- Do not continue feature implementation if validation shows the underlying feature data is wrong.

---

## 6. Architecture overview

The framework should be organized as layered responsibilities.

```text
Generated Mass Core
  Seed, base mesh, topology, scale, embedding, variant buttons.

Semantic Feature Library
  Reusable masks and feature data: exposure, crevice, edge wear, pitting, frost, cracks, etc.

Feature Bake Step
  Computes expensive topology-aware or spatial feature data once during generation.

Generated Feature Mask Atlas
  Packed feature masks sampled by the main generated-mass shader.

Feature Controls
  Editable values for each feature: strength, coverage, scale, softness, direction, breakup, tint, response.

Archetype Recipes
  Preset sets of feature-control values.

Manual Overrides
  User edits after recipe application.

Shader / Material Interpretation
  Cheap main-shader sampling and generic response application.

Debug / Validation Views
  Feature masks and final-response debug modes.
```

No single layer should do everything.

### 6.1 Generated Feature Mask Atlas foundation

The feature mask atlas is now a prerequisite for serious final feature work.

Patch 14C should implement the real foundation, not a temporary projection. The approved direction is:

```text
Generated mass main mesh
+ dedicated feature-atlas UV channel
+ deterministic generated surface-chart mapping
+ packed generated FeatureAtlas0 texture
+ main-shader debug sampling
+ no final visual response yet
```

The framework should prefer this runtime pattern:

```text
Generation time:
  inspect mesh/topology/spatial data
  build surface-chart mapping for the generated mass
  preserve triangle/edge adjacency metadata for feature painting
  pack charts into a generated atlas with padding/gutters
  compute semantic feature masks from mesh/topology/spatial data
  paint masks into all affected charts, including both sides of selected shared edges when needed
  dilate/gutter-fill mask data to prevent filtering bleed
  assign a dedicated feature-atlas UV channel to the mass
  assign one or more generated mask textures to the mass renderer

Runtime:
  main generated-mass shader samples packed masks through the dedicated feature-atlas UV channel
  debug modes can display individual atlas channels directly on the main mass surface
  feature controls and recipes decide how sampled masks affect colour, value, roughness, smoothness, tint, or other surface response in later patches
```

This is preferred over duplicate final-render meshes because one generated mass should not accumulate one visible helper mesh per feature. Debug carriers may still exist for validation, but final features should generally live in the main material path.

This is also preferred over pure procedural shader guessing because the shader cannot cheaply know mesh adjacency, selected convex edges, coherent crack networks, pitting placement, or river-contact history per pixel unless that data was generated and supplied.

This is also preferred over local X/Z projection. A projection shortcut may sample a texture, but it is not a scalable generated-mass feature foundation. It fails on vertical faces and steep facets, and it creates a fragile mapping contract for future edge wear, cracks, pitting, frost, water streaks, deposits, and carved seams.

The intended tradeoff is deliberate:

```text
Spend more CPU/work during generation.
Use modest additional memory for packed masks.
Keep frame-time shader/render cost predictable and low.
```

The first implementation milestone should create the surface-chart atlas foundation and bake one initial feature mask: Convex Edge Wear into FeatureAtlas0.R. This first patch should be data/debug-only. Normal rendering should remain unchanged; the final edge-wear colour/value/smoothness response belongs in the next patch after the atlas path is validated.

Feature mask atlas rules:

- use a dedicated feature-atlas UV channel separate from existing scalar/material-mask data;
- use generated surface-chart mapping rather than local X/Z projection;
- preserve adjacency metadata so features can paint coherently across shared edges and connected surface regions;
- pack multiple feature masks together where possible;
- include chart padding/gutters and dilation from the first implementation;
- sample from the main generated-mass shader, not separate final overlay materials;
- keep feature channels generic and archetype-agnostic;
- keep debug views available for individual channels;
- avoid duplicate final feature meshes unless there is a proven need;
- use real geometry only when the feature must affect silhouette or actual collision.

Initial implementation details to prefer:

```text
Atlas0 default resolution: 256x256
Internal baker resolution parameter: prepare for 128 / 256 / 512 later
Texture interpretation: linear/non-sRGB mask data
Wrap mode: Clamp
Filtering: bilinear is acceptable when padding/dilation is present
Mip usage: deliberate later decision; do not rely on mips for the debug foundation
```

---

## 7. Recipe application model

Changing a recipe selection should not silently destroy manual edits.

Preferred UI behavior:

```text
Archetype Recipe
  Selected Recipe: Wet River Stone
  [Apply Recipe to Feature Controls]
  [Reset Feature Controls to Current Recipe]
```

Recommended states:

- Recipe Clean: feature controls match the selected recipe.
- Modified / Custom: at least one control differs from the selected recipe.
- Custom Recipe: no active built-in recipe, user-authored values.

Rules:

- Selecting a recipe in a dropdown should not immediately overwrite controls unless explicitly approved later.
- Applying a recipe should set the feature controls to known values.
- Manual edits should be allowed after applying.
- Reset should restore the selected recipe values.
- Recipe application should be deterministic and seed-stable.
- Recipe values should be documented.

### Patch 14B implementation note

Patch 14B adds the first code-level scaffold for this model.

Current inspector workflow after Patch 14B.1:

```text
Recipe & Feature Stack
  Feature Recipe
  Recipe Status
  [Apply Selected Recipe]
  [Reset Controls to Recipe]

Core Shape Recipe
  Existing mesh/shape recipe controls

Rendering & Profile
  Current material/profile controls

Feature Stack
  Exposure
    Strength
    Tint
    Tint Strength

  Base / Contact
    Base Lift
    Strength
    Tint
    Tint Strength

  Crevice / Shelter
    Height
    Fade
    Irregularity
    Strength
    Tint
    Tint Strength

  Dirt / Deposit
    Crawl Height
    Coverage
    Strength
    Tint
    Tint Strength

  Shared Edge / Crease Debug Visibility
    Debug Line Visibility

  Edge Wear
    Amount
    Width
    Coverage
    Softness

  Crease / Crack Debug
    Amount
    Width
    Length
    Branching
    Softness

Colour / Lighting Interpretation
  Overall generated-mass tint and lighting hue authority
```

Current built-in scaffold recipes:

- Generic Test Mass;
- Cold Grey Stone;
- Wet River Stone;
- Pale Frost Stone;
- Black Sacred Stone;
- Custom.

Changing `Feature Recipe` is intentionally inert. It does not overwrite manual values until the user explicitly presses Apply or Reset. Patch 14B recipes currently remap existing controls only: stone material profile, base colour, feature-shape values, feature-response values, tint strengths, lighting tint influence, and current edge/crease debug controls. New feature channels such as pitting, water wear, frost stress, crack network, and sacred plane control remain future work.

Patch 14B.1 corrects the inspector organization to match the framework rule that features are the reusable units. Controls are no longer presented as global buckets such as all shape controls, all strength controls, and all tint controls. Existing controls are grouped under their owning feature instead: Exposure, Base / Contact, Crevice / Shelter, Dirt / Deposit, Edge Wear, and Crease / Crack Debug. Future controls should be added under their feature foldout unless they are genuinely global.

The older shape `MassRecipe` / `MassArchetype` path still exists and still controls base proportions, seeds, major cuts, edge character, grounding, and lean. Patch 14B does not replace that shape system; it establishes the separate feature-recipe scaffold that future patches can merge or coordinate with the shape recipe more deliberately.

---

## 8. Feature contract

Every accepted feature should eventually be documented with this contract:

```text
Feature Name
Semantic Meaning
Category
Applies To
Generation Method
Data Storage
Minimum Controls
Advanced Controls
Debug View
Shader Interpretation
Recipe Values
Performance Notes
Validation Criteria
Failure Cases
Current Status
```

A feature is not ready for recipe use until it has:

- a clear semantic meaning;
- at least one control;
- a validation/debug method where practical;
- documented failure cases;
- one or more intended recipe uses.

---

## 9. Current baseline feature data

The current generated-mass stone work already provides or partially provides these semantic channels.

### SurfaceVariation

Current role:

- deterministic per-rock/per-surface variation;
- currently used by the shader for broad material variation.

Future role:

- general surface breakup input for many mass types.

### Exposure

Current role:

- upward/exposed surface mask;
- used for exposure lift/tint response.

Future role:

- snow/frost catch;
- dust catch;
- sun bleaching;
- dry abrasion;
- exposed mineral response.

### CreviceBase

Current role:

- lower/sheltered/base accumulation mask;
- used for crevice/base grounding and darkening.

Future role:

- dirt fill;
- ice-in-crevice;
- wetness accumulation;
- mineral deposit;
- moss/lichen if later desired.

### DirtDeposit

Current role:

- deposit/stain mask using existing generated data and shader expansion.

Future role:

- mineral residue;
- wet deposits;
- grime;
- dust;
- organic accumulation.

### ConvexEdgeWear

Current role:

- exists as debug/validation data;
- raised visual strips are debug-only and not accepted as final rendering.

Future role:

- generic edge-localized mask for abrasion, erosion, frost, chipping, or stylized accent outcomes created by recipes;
- should be baked into the generated feature mask atlas for final rendering rather than shown as a raised/floating strip.

### ConcaveCrease

Current role:

- exists as debug/validation data;
- raised visual strips are debug-only and not accepted as final rendering.

Future role:

- actual surface-integrated cracks/seams;
- ice-in-crease;
- dirt accumulation;
- carved seam basis;
- fracture language.

### Stone Mottle / Material Breakup

Current role:

- shader-side broad material breakup introduced in Patch 13;
- currently useful as infrastructure but not sufficient as a complete archetype identity.

Future role:

- general profile-independent surface breakup feature;
- recipe-controlled mottle/deposit/noise response.

---

## 10. Default reusable feature library

This section lists the default/base features currently worth considering. More may be added later.

### 10.1 Core mass features

#### Seed / Variant

Semantic meaning:

- deterministic variation identity.

Minimum controls:

- Shape Seed;
- Surface Seed;
- Variant Seed or combined variant buttons.

Notes:

- Must remain deterministic.
- Recipe application should not randomly change seeds unless explicitly requested.

#### Base Shape Complexity

Semantic meaning:

- broad mass topology detail level.

Minimum controls:

- complexity level;
- facet complexity;
- roundness;
- scale/size.

Potential recipe use:

- sacred monoliths may use lower complexity and stronger plane control;
- brittle/frost masses may use sharper fracture tendency;
- river stones may use rounder silhouettes.

#### Facet / Plane Language

Semantic meaning:

- how visible and deliberate planar faces are.

Minimum controls:

- facet strength or planar strength;
- facet scale/detail.

Potential advanced controls:

- dominant plane bias;
- plane flattening;
- asymmetry.

#### Embedded Base / Contact Shape

Semantic meaning:

- how the lower/contact area is shaped and interpreted.

Minimum controls:

- base/contact lift;
- embed/contact influence.

Potential recipe use:

- river stones may have stronger lower wetness/deposit response;
- ruin chunks may have dirt accumulation at base;
- ice chunks may have melt/wet edges at base.

### 10.2 General material features

#### Surface Variation

Semantic meaning:

- broad non-specific material breakup.

Minimum controls:

- strength;
- scale.

Advanced controls:

- softness;
- contrast;
- profile bias.

#### Surface Mottle

Semantic meaning:

- broad visible mottling independent of a specific feature like crack or frost.

Minimum controls:

- strength;
- scale;
- softness.

Advanced controls:

- shelter bias;
- exposed bias;
- tint/colour response.

#### Dirt / Deposit Accumulation

Semantic meaning:

- grime, mineral, dust, residue, or environmental accumulation.

Minimum controls:

- strength;
- coverage.

Advanced controls:

- crawl height;
- breakup;
- tint;
- wet/dry interpretation;
- crevice/base bias.

#### Mineral Deposit

Semantic meaning:

- non-dirt mineral residue such as pale streaks, brown iron stains, chalky residue, or river minerals.

Minimum controls:

- strength;
- coverage.

Advanced controls:

- colour/tint;
- vertical streaking;
- crevice bias;
- waterline bias.

#### Dry Dust

Semantic meaning:

- pale dry accumulation on upward/exposed surfaces.

Minimum controls:

- strength;
- exposure bias.

Advanced controls:

- tint;
- breakup;
- slope threshold.

### 10.3 Edge, crease, and fracture features

#### Convex Edge Wear

Semantic meaning:

- semantic worn/chipped/abraded edge regions.

Minimum controls:

- strength;
- width.

Advanced controls:

- coverage;
- softness;
- breakup;
- generated feature atlas channel / response data.

Important:

- The existing raised edge strips are debug-only.
- Final edge wear must be surface-integrated through the main generated-mass shader.
- Long-term final edge wear should use the generated feature mask atlas, not duplicate final-render meshes.
- Edge Wear must stay generic; water erosion, frost catch, and sacred accents are recipe outcomes created by combining generic features.

#### Concave Crease

Semantic meaning:

- semantic concave seam/crease/fracture lines.

Minimum controls:

- strength;
- width.

Advanced controls:

- length;
- branching;
- softness;
- depth/darkness;
- fill material.

Important:

- The existing raised crease strips are debug-only.
- Final creases must not float above the surface.

#### Crack Network

Semantic meaning:

- surface-integrated crack patterns.

Minimum controls:

- strength;
- density;
- width.

Advanced controls:

- branching;
- length;
- depth;
- age/weathering;
- fill material;
- directionality.

Potential recipes:

- frost stress;
- ruin fragments;
- bone/fossil fragments;
- sacred carved cracks, if controlled.

#### Brittle Chipping

Semantic meaning:

- sharp chipped/broken regions.

Minimum controls:

- strength;
- coverage.

Advanced controls:

- chip size;
- edge bias;
- fracture sharpness.

Potential recipes:

- frost stone;
- broken ruin stone;
- bone/fossil;
- crystal chunks.

#### Abrasion

Semantic meaning:

- worn surface areas from dry contact or repeated rubbing.

Minimum controls:

- strength;
- coverage.

Advanced controls:

- edge bias;
- directionality;
- roughness response.

### 10.4 Water and erosion features

#### Water Wear

Semantic meaning:

- broad evidence of water erosion.

Minimum controls:

- strength;
- coverage.

Advanced controls:

- edge softening;
- lower/contact bias;
- direction bias;
- deposit interaction.

Potential recipes:

- wet river stones;
- shoreline stones;
- ice/water hybrid masses.

#### Flow Polish

Semantic meaning:

- directional smoothing/streaking caused by water flow or repeated directional abrasion.

Minimum controls:

- strength;
- direction.

Advanced controls:

- streak scale;
- streak length;
- roughness reduction;
- exposed/side bias;
- river-flow integration later.

Important:

- Initial implementation may use object/world direction.
- Later river integration may use local river flow direction.

#### Surface Pitting

Semantic meaning:

- small holes, pores, erosion pits, or voids in the surface response.

Minimum controls:

- strength;
- density;
- scale.

Advanced controls:

- depth/darkness;
- wetness interaction;
- edge/base bias;
- randomness/seed.

Potential recipes:

- wet river stone;
- bone/fossil;
- volcanic/dark stone;
- old ruin fragments.

#### Waterline Stain

Semantic meaning:

- horizontal or flow-adjacent staining from repeated water contact.

Minimum controls:

- strength;
- height/position.

Advanced controls:

- softness;
- breakup;
- tint;
- river contact integration later.

### 10.5 Cold, ice, and frost features

#### Frost Accumulation

Semantic meaning:

- pale frost/snow/ice residue gathered on exposed surfaces.

Minimum controls:

- strength;
- coverage.

Advanced controls:

- exposure bias;
- slope threshold;
- softness;
- tint;
- breakup.

Important:

- Should be mostly pale/neutral, not saturated blue.
- Should not reveal internal triangulation as a feature.

#### Ice In Crevices

Semantic meaning:

- pale/cold fill in concave or sheltered cracks/crevices.

Minimum controls:

- strength;
- crevice bias.

Advanced controls:

- tint;
- width;
- softness;
- glint/roughness response.

#### Frost Stress

Semantic meaning:

- subtle brittle stress lines or fracture tendency caused by freezing.

Minimum controls:

- strength;
- density.

Advanced controls:

- crack linkage;
- directionality;
- branching;
- surface integration depth.

#### Melt / Refreeze Wetness

Semantic meaning:

- cold wet/dark zones caused by melting and refreezing.

Minimum controls:

- strength;
- base/shelter bias.

Advanced controls:

- gloss response;
- darkness;
- frost interaction.

### 10.6 Stylized, cultural, and authored-looking features

#### Sacred Plane Control

Semantic meaning:

- deliberate monolithic or ritualized planar mass language.

Minimum controls:

- strength;
- plane consistency.

Advanced controls:

- symmetry/asymmetry;
- edge restraint;
- surface variation suppression;
- carve compatibility.

#### Monolithic Flatten

Semantic meaning:

- reduces noisy breakup and emphasizes solid unified planes.

Minimum controls:

- strength.

Advanced controls:

- preserve edge accents;
- preserve subtle mottle;
- roughness response.

#### Carved Seam

Semantic meaning:

- intentional carved groove/seam/glyph-like feature.

Minimum controls:

- strength;
- width.

Advanced controls:

- pattern type;
- depth;
- branching;
- fill material;
- sacred recipe bias.

Important:

- Future-only until cracks/creases are surface-integrated.
- Must not be implemented as floating raised lines.

#### Accent Glow / Rune Glow

Semantic meaning:

- stylized magical accent response.

Minimum controls:

- strength;
- colour.

Status:

- Future-only.
- Not part of the current physical stone feature pass.

### 10.7 Non-stone compact-mass features

#### Bone Porosity

Semantic meaning:

- porous pitted bone/fossil-like surface.

Minimum controls:

- strength;
- density;
- scale.

#### Bone Ridge Wear

Semantic meaning:

- worn ridges and exposed raised bone surfaces.

Minimum controls:

- strength;
- ridge bias.

#### Crystal Facet Clarity

Semantic meaning:

- sharper, cleaner planar facets and internal colour/opacity response.

Minimum controls:

- strength;
- facet clarity.

#### Ore Vein

Semantic meaning:

- mineral veins or metallic/nonmetallic inclusions.

Minimum controls:

- strength;
- density;
- colour/material response.

#### Ruin Chipping

Semantic meaning:

- broken corners and damaged carved stone block edges.

Minimum controls:

- strength;
- edge bias.

---

## 11. Initial recipe concepts

These are not final shipped archetypes. They are starting recipe definitions for future implementation.

### Generic Test Mass

Purpose:

- validate feature channels one by one.

Recipe intent:

```text
SurfaceVariation: medium
Exposure: medium
CreviceBase: medium
DirtDeposit: medium
ConvexEdgeWear: testable
ConcaveCrease: testable
Special water/frost/sacred features: off
```

### Cold Grey Stone

Purpose:

- default believable stylized stone.

Recipe intent:

```text
SurfaceVariation: medium
SurfaceMottle: medium
Exposure: medium
CreviceBase: medium
DirtDeposit: low-medium
ConvexEdgeWear: low-medium dry abrasion
SurfacePitting: low
WaterWear: off
Frost: off
SacredPlaneControl: off
```

### Wet River Stone

Purpose:

- damp water-worn stone, not polished glass.

Recipe intent:

```text
WaterWear: high
FlowPolish: high
WetnessAccumulation: high
SurfacePitting: medium
MineralDeposit: medium
DirtDeposit: medium-high
ConvexEdgeWear: softened erosion
CreviceBase: medium-high
Global reflectivity: controlled, local, rough
Frost: off
SacredPlaneControl: off
```

### Pale Frost Stone

Purpose:

- cold brittle frosted stone, not blue random noise.

Recipe intent:

```text
FrostAccumulation: high
IceInCrevices: medium-high
FrostStress: medium
CrackNetwork: medium
BrittleChip: medium
DirtDeposit: low
Wetness: none or low
Blue tint: very restrained
```

### Black Sacred Stone

Purpose:

- dark monolithic ritual stone, not wet stone.

Recipe intent:

```text
SacredPlaneControl: high
MonolithicFlatten: medium-high
SurfaceVariation: low-medium
SurfaceMottle: subtle
ConvexEdgeWear: restrained
CarvedSeam: future optional
CrackNetwork: low or controlled
Wetness: off
Frost: off
```

### Ice Chunk

Purpose:

- compact frozen mass, distinct from frosted stone.

Recipe intent:

```text
Crystal/ice clarity: medium
FrostAccumulation: medium-high
CrackNetwork: medium-high
BrittleChip: high
Wet/melt edge: low-medium
DirtDeposit: low
```

### Broken Ruin Block

Purpose:

- compact fragment of carved or constructed stone.

Recipe intent:

```text
Sacred/constructed plane control: medium
RuinChipping: high
CarvedSeam: medium
DirtDeposit: medium-high
Moss/Lichen: future optional
CrackNetwork: medium
WaterWear: environment-dependent
```

### Bone / Fossil Fragment

Purpose:

- compact bone-like mass, if needed later.

Recipe intent:

```text
BonePorosity: high
BoneRidgeWear: medium-high
DirtDeposit: medium
CrackNetwork: medium
Frost/Water/Sacred: off unless hybrid recipe
```

### Ore / Mineral Chunk

Purpose:

- compact stone with mineral inclusions.

Recipe intent:

```text
SurfaceVariation: medium
OreVein: medium-high
CrystalFacetClarity: optional
EdgeWear: low-medium
DirtDeposit: low-medium
```

---

## 12. Generic Test Mass workflow

All new features should be validated on a generic test mass before they are used in recipes.

Workflow:

```text
1. Define the feature contract.
2. Implement the feature mask/data.
3. Add at least one control.
4. Add or reuse a debug view.
5. Validate the feature on Generic Test Mass.
6. Validate final visual interpretation separately from the raw mask.
7. Only then add recipe values for archetypes.
```

Do not build a WetRiver-only or Frost-only hack before the generic feature exists.

---

## 13. Inspector and authoring model

The inspector should expose all important feature controls without becoming unreadable.

The foundational rule is that **features own their controls**. Do not split one feature across global buckets such as all shape controls, all strength controls, all tint controls, and all debug controls. That layout hides the mental model of the system and will not scale once features have different knobs.

Recommended grouping pattern:

```text
Recipe & Feature Stack
Core Shape
Rendering & Material Profile

Feature Stack
  Exposure
  Base / Contact
  Crevice / Shelter
  Dirt / Deposit
  Edge Wear
  Crease / Crack Debug
  Future Feature Name

Colour / Lighting Interpretation
River Interaction, if relevant
Variant Controls
```

Feature foldouts should contain that feature's own controls. For example, Crevice / Shelter owns Height, Fade, Irregularity, Strength, Tint and Tint Strength. Dirt / Deposit owns Crawl Height, Coverage, Strength, Tint and Tint Strength. Future features such as Surface Pitting or Flow Polish should follow the same pattern instead of adding global category sections.

Rules:

- Main controls should be visible or easily discoverable.
- Advanced controls should be folded but not inaccessible.
- Every recipe-controlled feature should be manually editable.
- Recipe application should be explicit.
- Debug-only features should be clearly marked.
- Deprecated/historical controls should not silently remain active without explanation.

---

## 14. Performance rules

Generated Mass should prioritize runtime performance and predictable cost.

Rules:

- Prefer generation-time computation over runtime per-frame cost when possible.
- Prefer semantic masks and packed channels over many independent expensive shader evaluations.
- Avoid per-pixel loops or heavy procedural functions unless the visual payoff is substantial.
- Use debug views to validate masks before increasing shader complexity.
- Keep feature data seed-stable.
- Do not increase mesh density only to solve a shader/material problem unless explicitly justified.
- Allow modest memory use if it substantially reduces runtime computation.
- Future quality tiers are acceptable; the first implementation should remain reasonable for low-to-medium desktop PCs.

---

## 15. Immediate roadmap after this document

Recommended next steps after Patch 14B.1:

```text
Patch 14A
Create framework documentation and implementation checklist. Status: implemented.

Patch 14B
Create the Generic Test Mass workflow and feature recipe scaffold. Status: implemented as the first non-destructive recipe UI and code-level feature-control scaffold.

Patch 14B.1
Correct the generated-mass inspector to be feature-oriented rather than category-oriented. Existing controls are grouped under feature foldouts so future feature channels have a stable authoring pattern. Status: implemented.

Patch 14C
Generated Mass Surface-Chart Feature Atlas Foundation. Create the dedicated feature-atlas UV channel, generated surface-chart atlas mapping, FeatureAtlas0 texture, Atlas0.R ConvexEdgeWear data/debug bake, shader binding, and main-surface debug preview. Normal rendering remains unchanged. This is now a prerequisite before serious final surface-feature implementation.

Patch 14D
Generic Edge Wear via Feature Atlas. Interpret the already-baked ConvexEdgeWear atlas channel through the main generated-mass shader using generic controls.

Patch 14E+
Implement reusable features one by one using the atlas where practical, validating each on Generic Test Mass before adding archetype recipe values.
```

Do not resume WetRiver/Frost/Sacred profile tuning until reusable feature channels exist.

Current recommended implementation order:

1. Generated Mass Surface-Chart Feature Atlas Foundation.
2. Generic Edge Wear via Feature Atlas.
3. Surface Pitting via Feature Atlas.
4. WaterWear / FlowPolish.
5. FrostAccumulation.
6. CrackNetwork / Surface-integrated Crease.
7. SacredPlaneControl / CarvedSeam.

Current recommendation:

```text
Implement the feature mask atlas foundation before adding more final visual feature complexity.
```
