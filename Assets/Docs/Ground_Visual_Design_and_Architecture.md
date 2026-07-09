---
document_id: PS3D-GROUND-01
title: "Ground Visual Design and Architecture"
version: 0.1
status: active-baseline
scope: generated-ground-style-and-architecture
authoritative_for: "generated ground visual doctrine, style pillars, family/variant interpretation, shared ground style layers, static surface-mask contracts, and ground roadmap priority"
related_documents: [PS3D-00, PS3D-01, PS3D-02, PS3D-04, PS3D-06]
implementation_documents:
  - Ground_Generation_Surface_Upgrade_Plan.md
---

# Ground Visual Design and Architecture

## Purpose

This document is the canonical generated-ground design document.

It exists so ground direction does not have to be reconstructed from the generic visual-language docs or from an implementation patch plan. It defines the ground's visual philosophy, style target, design pillars, surface-family architecture, shader/data contracts, feature-layer strategy, and active implementation priority.

The short version is:

```text
Restrained stylized terrain:
BOTW/TOTK-like base-material restraint
+ Hades-1-like painted ground accents
+ mostly 3D procedural geometry
+ reusable procedural masks and style layers
+ family/variant tuning.
```

This is now the baseline. Future ground work must either serve this direction or explicitly document why the direction is being changed.

## Authority and Document Boundaries

This document owns the durable generated-ground design baseline.

Use it for questions such as:

- What should generated ground look like?
- How simple or noisy should ground be?
- How should BOTW/TOTK and Hades 1 influence the ground?
- How do ground families and variants relate to the shared style doctrine?
- Which ground layers are foundational and which are niche?
- What should be built before runtime footprints, puddles, grass suppression, or rain?
- How should ground features avoid becoming one-off silos?

Use `Ground_Generation_Surface_Upgrade_Plan.md` for implementation history, patch notes, exact current status, and concrete patch sequencing.

Use `Proof of Concept/01_Visual_Language_and_Rendering.md` for broader project rendering principles, palette, lighting, camera, snow, fog, and general stylized 3D language.

Use `Proof of Concept/06_Proof_of_Concept.md` for the clearing prototype scope and validation goals.

If these documents conflict on generated-ground direction, this document is the ground-specific source of truth and the other document should be patched.

## Current Decision

The ground direction is not:

```text
Tunic block-world terrain
Hades 2 hand-painted production density
realistic ARPG terrain material complexity
high-frequency procedural noise
runtime simulation first
feature-by-feature terrain special cases
```

The ground direction is:

```text
A calm, readable, mostly matte 3D stage floor,
made rich through broad patch composition,
selective painted-looking accents,
contact/edge response,
sparse reusable motifs,
and later runtime/weather/interaction state.
```

The ground should support the scene rather than become the scene's loudest element. It should look intentional from an isometric/action camera while leaving characters, hazards, VFX, rivers, rocks, structures, silhouettes, fog, and lighting room to read.

## Reference Interpretation

References are production grammar, not literal style mandates.

### BOTW/TOTK: base-material restraint

The useful lesson is that base terrain can be visually quiet. Large ground areas can rely on:

- broad color/value regions;
- low-frequency material variation;
- readable terrain forms;
- slope and shoreline relationships;
- vegetation, rocks, cliffs, props, and paths;
- lighting and atmosphere;
- composition and silhouette hierarchy.

The ground material does not need to impersonate every blade of grass, pebble, mud wrinkle, snow grain, or stain. A simple base can work if the scene around it carries enough form and meaning.

### Hades 1: authored-looking accent grammar

The useful lesson is not exact brushwork. The useful lesson is floor grammar:

- broad readable regions;
- strong value grouping;
- sparse decorative marks;
- short dark mound/crease lines;
- cracks, chips, stains, scuffs, trim, and rhythm;
- contact emphasis around walls, props, edges, and boundaries;
- ground detail that supports gameplay readability.

The small dark ground lines visible in Hades 1 are especially relevant. They read as shallow mounds, grass folds, soft contour breaks, mud creases, or hand-painted surface rhythm. They imply form without needing real height deformation.

### Tunic: readability reference only

Tunic remains useful for high-angle readability, clean silhouettes, and compositional economy.

It is not the generated-ground style target. Tunic's basic ground works because the whole world is reduced to chunky toy/block geometry. This project is allowed to use more organic rocks, rivers, generated masses, snow, mud, shorelines, foam, and terrain detail. In that context, Tunic-simple ground can look underdeveloped rather than elegant.

### Hades 2: ambition reference, not production target

Hades 2 is not a feasible generated-ground baseline. Its floor art is extremely authored and dense. It can inspire taste, but it should not define implementation expectations.

### Realistic dark ARPGs: caution reference

Realistic or high-detail ARPG ground implies heavier authored textures, scans, decals, lighting, material response, grime, and asset production. That is not the current project direction.

## Non-Goals

Generated ground must not drift into the following unless a future design decision explicitly replaces this document:

- full hand-painted terrain everywhere;
- Hades 2-level authored floor detail;
- Tunic/voxel/block terrain as the primary visual target;
- realistic scanned terrain materials;
- noisy procedural texture soup;
- constant high-frequency variation to hide weak composition;
- every ground family using unrelated shader branches;
- one-off feature silos that cannot serve multiple families;
- runtime footprints, puddles, rain, wetness, or grass trampling before the static visual language works;
- heavy terrain height noise that threatens combat readability;
- river-specific or family-specific hacks inside unrelated systems.

## Sacred Design Rule

The ground is a shared visual stack tuned by families and variants.

It is not a collection of unrelated special-case effects.

```text
Shared doctrine stack
  calm base
  broad patches
  semantic mask response
  painted accent lines
  contact / edge accents
  sparse motifs
  runtime state later

Family / variant tuning
  decides how a specific surface expresses that stack.
```

A new ground feature is valid only if it can name which layer it belongs to and how it remains reusable or deliberately scoped.

## Style Pillars

### Pillar 1 - Calm Base Surfaces

Base ground is the stage floor.

It should be:

- matte or mostly matte;
- broad;
- low-noise;
- low-to-moderate contrast;
- readable from the game camera;
- controlled by family/variant material values;
- subordinate to characters, hazards, VFX, rivers, rocks, props, silhouettes, and lighting.

The base material should not carry all terrain identity by itself. Earlier wet mud tuning showed the failure mode: broad smoothness/color changes can become plastic or playdough. The solution is not endless global material tweaking; it is a restrained base plus explicit style layers.

### Pillar 2 - Broad Macro Patch Composition

The first visible variation layer should be large and deliberate.

Good macro patches:

- use low-frequency variation;
- create broad value/color islands;
- respect the family palette;
- read from the isometric camera;
- avoid checkerboard noise;
- can be posterized or softened;
- can be biased by exposure, damp/deposit, shore, vegetation, and compaction masks;
- support composition instead of visual mush.

Bad macro patches:

- look like arbitrary Perlin noise;
- create equal activity everywhere;
- fight player/foe silhouettes;
- hide path readability;
- overpower rivers, shorelines, or combat telegraphs.

### Pillar 3 - Painted Accent Lines

Painted accent lines are the first foundational new style layer after this doctrine.

They are short, broken, Hades-1-like dark/value-shifted surface strokes. They should suggest:

- small mounds;
- grass folds;
- mud creases;
- snow wrinkles;
- soft contour breaks;
- surface age;
- hand-authored rhythm.

They are visual only. They do not require terrain deformation.

They should be:

- short;
- broken;
- slightly curved;
- clustered rather than uniform;
- low-to-medium contrast;
- darkened or value-shifted rather than pure black;
- sparse enough that quiet ground remains quiet;
- tuned per family/variant;
- reusable across snow, mud, grass, rocky dirt, shore, and path surfaces.

They must not become:

- uniform hatching;
- full-screen scribble noise;
- equal-density procedural cracks everywhere;
- hard black outlines unrelated to lighting/material;
- a mud-only or grass-only feature silo.

### Pillar 4 - Contact and Edge Accents

Ground should visually respond around meaningful geometry and semantic boundaries. This is one of the main ways an isometric scene looks authored instead of assembled.

Contact/edge accent candidates:

- rock bases;
- standing stones;
- cliffs and banks;
- river shorelines;
- bridge or crossing contact;
- path boundaries;
- modifier boundaries;
- raised/lowered terrain edges;
- structure foundations;
- camp pads and authored clearings;
- damp deposits near water;
- snow buildup near wind-protected edges.

This layer may add local darkening, dampness, deposit hints, outline-like value shifts, accent-line density changes, or surface-wear emphasis. It must not turn every object into a heavy decal blob.

### Pillar 5 - Sparse Motifs and Stamps

After accent lines and contact accents, the next detail tier is sparse motif/stamp content.

Examples:

- chips;
- cracks;
- small stones;
- dirt strokes;
- dry scuffs;
- mud stains;
- snow scrape marks;
- tiny tuft-like marks;
- leaf/debris hints;
- frost specks;
- ash specks;
- broken trim marks.

Rules:

- sparse beats dense;
- clusters beat uniform distribution;
- motifs should respond to semantic masks;
- motifs should not tile obviously;
- motifs should not be required for the base ground to look acceptable;
- each family/variant should be able to reduce or disable motif density.

### Pillar 6 - Runtime State Later

Runtime surface state remains valuable, but it is not the foundation right now.

Deferred runtime state includes:

- rain wetness;
- drying;
- snow depth;
- snow compression;
- footprints;
- grass trampling;
- mud disturbance;
- puddle growth;
- standing-water evolution;
- disturbance age.

These systems should wait until the static visual language works. Runtime state is expensive in complexity even if the texture memory is manageable. It should not be used to compensate for unresolved art direction.

### Pillar 7 - Geometry Still Matters

The ground is not only a shader plane.

The scene should also be carried by:

- terrain silhouette;
- banks and slopes;
- rivers and shore corridors;
- generated rocks/masses;
- structures and ruins;
- grass/vegetation later;
- snow banks later;
- fog and lighting;
- prop placement;
- manual composition.

A calm base material works only if these scene layers participate.

## Ground Composition Stack

The intended render/meaning stack is:

```text
Playable terrain shape
  ↓
Calm family base material
  ↓
Broad macro patch composition
  ↓
Static semantic mask response
  exposure / damp / deposit / shore / vegetation / compaction / rocky-dry
  ↓
Painted accent lines
  ↓
Contact / edge accents
  ↓
Sparse motifs and stamps
  ↓
Runtime surface state later
  wetness / snow depth / compression / footprints / mud / puddles
  ↓
Debug override
```

The ordering matters:

- the base must remain readable without detail layers;
- macro patches define the broad composition;
- semantic masks make the ground respond to generated meaning;
- accent lines add the Hades-1-like authored rhythm;
- contact accents glue objects and terrain together;
- sparse motifs add identity without noise;
- runtime state modifies the surface only after the static language is proven.

## Family and Variant Architecture

The existing family/variant architecture remains correct. It now has clearer meaning.

```text
GroundSurfaceProfile
  semantic / mask-generation profile

GroundSurfaceStyleProfile
  visual surface family

GroundSurfaceVariantRecipe
  family-local recipe

GroundMaterialControls
  calm base-material response

GroundSurfaceFeatureRecipe
  reusable style-layer tuning

GeneratedGround
  resolver, top-level authoring surface, per-object override owner
```

The family decides what kind of surface this is.

The doctrine decides how all surfaces speak visually.

The variant tunes how much of each shared style layer appears.

### GroundSurfaceProfile

`GroundSurfaceProfile` owns semantic/mask-generation intent. It should describe what the generated surface means and what static masks are produced, not the entire visual material response.

Examples:

- snowfield semantic profile;
- wet mudflat semantic profile;
- future grassland profile;
- future rocky scrub profile;
- future ash or frost profile.

### GroundSurfaceStyleProfile

`GroundSurfaceStyleProfile` owns a visual family.

Examples:

- Snowfield;
- Wet Mudflat;
- future Grassland;
- future Rocky Scrub;
- future Ash/Frost/Corruption surface.

A family should not require bespoke shader logic for basic ground language. It should tune the shared doctrine stack.

### GroundSurfaceVariantRecipe

`GroundSurfaceVariantRecipe` owns a family-local recipe.

Examples:

```text
Snowfield.clean
Snowfield.patchy
Snowfield.dirty_thawing
Snowfield.wind_scoured

WetMudflat.damp_mud
WetMudflat.waterlogged
WetMudflat.trampled
WetMudflat.frozen_thaw
```

Variants should tune:

- base color/value;
- snow/mud/damp/shore response;
- macro patch scale/contrast;
- painted accent-line density/contrast;
- contact accent strength;
- sparse motif density;
- specific mask response such as compaction or wetness.

Variants should not create unrelated one-off rendering pipelines.

### GroundMaterialControls

`GroundMaterialControls` should remain the calm base material and broad response recipe.

It is appropriate for:

- base color;
- secondary color;
- brightness/value bias;
- tint strength;
- damp darkening;
- snow tinting;
- smoothness/specular baseline;
- patch scale/contrast;
- broad material response.

It is not the right place to fake every terrain detail. If visual richness requires small strokes, contact accents, motif stamps, or runtime state, those should be explicit layers.

### GroundSurfaceFeatureRecipe

`GroundSurfaceFeatureRecipe` should evolve from one-off features into reusable style-layer tuning.

Good feature kinds are doctrine layers or reusable semantic responses:

- PaintedAccentLines;
- ContactEdgeAccents;
- SparseMotifs;
- DirectionalSurfaceMarks;
- CompactionResponse;
- PooledWetnessResponse;
- ShoreDepositResponse.

Bad feature kinds are overly narrow unless deliberately scoped:

- one exact mud decal;
- one exact snow footprint look;
- one exact puddle shape hardcoded into a family;
- a feature that only works because a single current asset happens to need it.

## Current Feature Interpretation

Existing feature work should be reclassified under the doctrine rather than discarded.

### DirectionalStreaks

Keep, but reinterpret as an early directional surface-mark proof.

It may eventually fold into Painted Accent Lines or Directional Surface Marks.

### PooledWetness

Keep as a wetness response proof, but do not treat it as final puddles.

Real puddles/standing water are later explicit features or runtime state. Wet mud base should remain mostly matte.

### TrampledWear

Keep as a compaction/path response proof.

Patch U proved this flow:

```text
GroundModifier authored surface mask
→ generated metadata
→ UV2.x compaction/path
→ shader feature response
```

After the doctrine pivot, `TrampledWear` is no longer the active cornerstone. It should not be polished deeply until the shared accent-line/contact/motif layers are working.

## Feature Stack Direction

The current renderer effectively supports a narrow first-supported-feature model. That is not the final architecture.

The target is:

```text
Variant feature recipes
  ↓
GeneratedGround resolver
  ↓
aggregated shared style-layer controls
  ↓
shader stack applies multiple known layers
```

The final variant should not choose exactly one of:

```text
DirectionalStreaks OR PooledWetness OR TrampledWear
```

It should be able to tune several shared layers:

```text
Base material response
Macro patches
Painted accent lines
Contact accents
Sparse motifs
Wetness response
Compaction response
Shore response
Runtime state response later
```

This does not require every layer to be expensive. Most foundational layers should start as shader-only or mesh-mask-driven. Generated textures and runtime state should be opt-in cost tiers.

## Static Surface Data Contract

Generated ground uses vertex colors and UV2 as static semantic masks.

Current intended contract:

```text
Vertex Color R = tonal patch variation
Vertex Color G = exposure / snow-hold potential
Vertex Color B = damp / deposit potential
Vertex Color A = vegetation suitability

UV2.x = compaction / path / flatten influence
UV2.y = river / shore influence
UV2.z = rocky / dry secondary patch
UV2.w = authored standing-water / puddle potential
```

Design rules:

- these are semantic masks, not final visual effects;
- shader/style layers interpret them;
- debug modes must remain available and trustworthy;
- do not repurpose channels silently;
- if a channel meaning changes, update this document and the implementation plan together.

## GroundModifier Contract

Ground modifiers can now affect height, authored surface masks, or both.

The accepted design is:

```text
Mode = None + Surface Effect Mode = Custom
  → surface masks only, no height change

Mode = Flatten/Lower/Raise + Surface Effect Mode = Custom
  → height change + authored surface masks

Mode = Flatten + Surface Effect Mode = AutoFromHeight
  → legacy flatten writes compaction

Surface Effect Mode = None
  → height-only modifier, no authored surface meaning
```

Design rules:

- prefer surface-only masks when visual response is enough;
- allow small height denivelations for roads, wagon tracks, camp pads, puddle basins, and intentional terrain shaping;
- keep height changes combat-safe and camera-stable;
- do not bake final snow paths or grass paths into base ground; future snow/grass systems should interpret masks/runtime state.

## River Corridor Contract

River corridors must remain style-agnostic.

Correct relationship:

```text
GeneratedGround resolves family/variant/material/feature state
→ applies MaterialPropertyBlock to ground renderer
→ refreshes dependent river corridor renderer
```

Wrong relationship:

```text
StylizedRiver knows Snowfield, WetMudflat, TrampledWear, PaintedAccentLines, etc.
```

The river may consume ground snapshots, shore masks, and material-property refreshes. It must not own ground style-family logic.

## Shader and Material Contract

Generated ground uses a dedicated ground shader path, separate from generated masses.

Important rules:

- keep ground-specific contracts explicit;
- use `MaterialPropertyBlock` for object/variant-resolved values;
- avoid duplicating materials for every variant;
- keep debug modes stable;
- do not shift existing debug enum values casually;
- avoid turning the shader into a monolith of unrelated family branches;
- aggregate shared style-layer controls rather than hardcoding family-specific features.

The shader stack should eventually expose or receive controls for:

- base material response;
- macro patch scale/contrast;
- semantic mask responses;
- painted accent lines;
- contact/edge accents;
- sparse motif density;
- wetness/compaction/shore responses;
- runtime state later.

## Cost Classes

Ground features should declare or imply cost class.

Preferred progression:

```text
ShaderOnly
  first pass for broad visual layers

MeshMaskDriven
  when generated static masks are needed

GeneratedTexture
  only when the visual cannot be achieved cleanly from mesh masks/world-space shader logic

RuntimeState
  only after static style language is accepted
```

Do not make every style pay for every feature.

## Active Roadmap

The old runtime-first roadmap is paused.

Patch T and Patch U remain useful, but they are not the active direction:

- Patch T established the authored surface-mask contract.
- Patch U proved a shader feature can consume `UV2.x` compaction/path.

The active direction is now style calibration and shared doctrine layers.

| Priority | Patch | Goal |
| --- | --- | --- |
| 1 | V0 — Ground Visual Doctrine Documentation | Completed by documentation. Establish the canonical doctrine and this design doc. |
| 2 | V1 — Style Calibration Setup | Completed as a temporary `Style Calibration` surface family with four comparison variants. |
| 3 | V2 — Base Ground Simplification | Implemented as an asset/docs retune: Snowfield and Wet Mudflat now target calmer matte, lower-noise base surfaces. |
| 4 | V3 — Painted Accent Lines | Implement the first foundational shared style layer. Shader-only first pass. |
| 5 | V4 — Contact / Edge Accent Layer | Add localized response near shores, rocks, modifier boundaries, paths, banks, and object contact zones. |
| 6 | V5 — Sparse Motif Layer | Add reusable sparse chips, cracks, scuffs, stains, snow scratches, stones, and debris hints. |
| 7 | V6 — Feature Stack Resolver | Move from first-feature selection to aggregation of known shared style layers. |
| 8 | Later | Runtime Surface State Stub | Revisit wetness, snow depth, compression, footprints, and disturbance after static style acceptance. |
| 9 | Later | Footprints / Rain / Puddles / Grass Integration | Build on runtime state only after the visual doctrine is proven. |
| 10 | Future | Mixed Terrain / Profile Blending | Blend surface families such as snow over mud, rocky scrub over soil, or worn path through snow. |

## Style Calibration Requirements

Patch V1 should not add final features. It should create a comparison setup.

The same clearing should be tested under several lanes:

```text
Calm BOTW-like base
  simple, matte, low-noise, broad color/value regions

Hades-accent lane
  same base plus stronger painted accent lines and contact marks

Hybrid target lane
  restrained base plus selective accent lines/contact response/sparse motifs

Pixel/faceted lane
  current PS3D material-space pixel identity pushed harder
```

The goal is to decide the visible lane before deeper implementation.

The likely target is the hybrid lane.

### Patch V1 Implementation

Patch V1 implements the calibration setup as assets, not as new shader code or runtime systems.

Canonical calibration assets:

```text
Assets/Game/Demo/Profiles/Ground/GSP_StyleCalibration.asset
Assets/Game/Demo/Profiles/Ground/Styles/GSSP_StyleCalibration.asset
```

`GSP_StyleCalibration` is a neutral semantic/mask-generation profile. Its purpose is to keep the generated static masks steady while the visible style variants change. It is not a production biome or final terrain identity.

`GSSP_StyleCalibration` is a temporary development surface family discovered by the existing `GeneratedGround` family dropdown. It contains four comparison variants:

| Variant id | Display name | Purpose |
| --- | --- | --- |
| `calibration.calm_base` | Calm Base | BOTW/TOTK-like restraint test: matte, quiet, broad, low-noise ground. |
| `calibration.hades_accent_proxy` | Hades Accent Proxy | Current-tool approximation of stronger Hades-1-like ground mark rhythm. Uses `DirectionalStreaks` as a proxy, not as the final accent-line implementation. |
| `calibration.hybrid_target_proxy` | Hybrid Target Proxy | Expected target lane: calm base plus restrained accent rhythm. |
| `calibration.pixel_faceted` | Pixel-Faceted | Stress test for the existing PS3D material-space pixel/faceted identity. |

The Hades and Hybrid variants intentionally use the existing `DirectionalStreaks` feature only as a calibration stand-in. Real Hades-1-like painted accent lines remain Patch V3 and must not be confused with this proxy.

Patch V1 does not add:

- `PaintedAccentLines` shader logic;
- contact/edge accent logic;
- sparse motifs;
- feature-stack aggregation;
- runtime state;
- footprints, puddles, rain, grass suppression, roads, or wagon tracks;
- scene edits or new components.

The purpose is screenshot comparison from the same clearing and camera, so the next implementation patch is guided by evidence instead of taste drift.


### Patch V2 Implementation

Patch V2 records the first calibration conclusion and retunes production families accordingly.

Calibration conclusion:

```text
Use Calm Base as the foundation.
Keep Hybrid as the target philosophy.
Do not use Pixel-Faceted as the default ground read.
Do not mistake DirectionalStreaks for real Hades-1-like painted accent lines.
```

Implemented adjustments:

- `Pixel-Faceted` is now a flat display label. The stable id remains `calibration.pixel_faceted`.
- Snowfield variants reduce fine pixel noise, patch contrast, warp, and over-strong directional streaks.
- Wet Mudflat variants reduce fine pixel noise, damp darkening, patch contrast, and feature intensity while staying matte.
- `TrampledWear` remains a useful compaction-response proof, but it is not treated as the primary ground-style foundation.

Patch V2 is still not final ground art. It is a base-floor cleanup so later doctrine layers have room to work.

Patch V2 does not add painted accent lines, contact accents, sparse motifs, runtime state, new shader properties, scene edits, river logic, or feature-stack aggregation.

## Acceptance Criteria

Ground work is successful when:

- a quiet ground area still looks intentional, not empty;
- broad patches read from the game camera;
- accent lines feel authored, not procedurally sprayed;
- contact accents make rocks/rivers/paths feel integrated;
- variants feel related by one visual language;
- snow and mud differ through tuning, not unrelated pipelines;
- the ground does not fight characters, VFX, hazards, or UI;
- debug masks correspond to visible responses;
- feature cost remains opt-in and understandable;
- screenshots make the style direction easier to choose, not harder.

Ground work is failing when:

- visual richness comes mostly from noise;
- every surface has equal detail density;
- the shader becomes a list of hardcoded family branches;
- feature work cannot explain which doctrine layer it improves;
- a quiet area looks unfinished;
- a detailed area looks like texture soup;
- runtime simulation is used to hide an unresolved static style.

## Authoring Workflow Target

The authoring flow should remain object/profile driven:

```text
Select GeneratedGround
→ choose Surface Family
→ choose Surface Variant
→ optionally override material/style controls
→ place GroundModifiers for semantic/height intent
→ regenerate
→ inspect debug masks
→ inspect final ground
```

Style-profile authoring should remain asset-backed:

```text
Open GroundSurfaceStyleProfile
→ inspect variants
→ tune material controls
→ tune feature-layer recipes
→ apply to open generated grounds
```

The UI should expose design concepts, not low-level noise clutter.

## Debug and Validation Rules

Ground debugging should remain compact and trustworthy.

Required mask/debug concepts:

- tonal patch;
- exposure/snow-hold;
- damp/deposit;
- vegetation suitability;
- compaction/path;
- shore;
- rocky/dry;
- standing-water/puddle potential;
- combined ground mask where useful.

Validation should always distinguish:

```text
mask exists and is correct
shader interprets mask correctly
style tuning looks good
```

Do not diagnose visual tuning before confirming the mask path.

## Runtime State Contract - Deferred

Runtime state remains a future layer. The current likely channel contract is:

```text
R = wetness
G = snow depth / snow amount
B = compression / footprint / trample
A = mud / standing water / disturbance age
```

This contract is not active implementation priority. It should be revisited after the static stack is accepted.

## Future Family Examples

Future ground families should be added through profiles/style assets and should obey the same doctrine.

Possible families:

- Grassland;
- Rocky Scrub;
- Frozen Dirt;
- Ash Field;
- Corrupted Ground;
- Worn Road;
- Snow-over-Mud blend;
- Riverbank/Silt;
- Sacred/Ritual Ground.

Each family should tune:

- calm base material;
- macro patch behavior;
- accent-line behavior;
- contact/edge response;
- sparse motif identity;
- semantic mask interpretation;
- runtime response later.

## Maintenance Rules

1. This document owns generated-ground visual doctrine.
2. The implementation plan records patch history and concrete work sequencing.
3. Generic visual-language docs may summarize this doctrine but should link here instead of duplicating every detail.
4. When implementation changes a channel contract, feature-layer meaning, or roadmap priority, update this document and the implementation plan together.
5. Do not allow examples to become accidental requirements. Mark them as examples if they are not committed.
6. Do not introduce a new ground feature unless it identifies its doctrine layer.
7. Do not resume runtime surface-state work until the static style calibration is accepted or the pause is explicitly lifted.

## Final Baseline Statement

Generated ground should be a restrained stylized stage: simple enough to preserve readability, rich enough to feel designed, and structured enough that every future family can share one visual language.

The base is calm. The interest comes from broad patches, meaningful masks, selective painted accents, contact response, sparse motifs, and later runtime state.

That is the ground baseline until deliberately changed.
