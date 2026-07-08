# Ground Generation Surface Upgrade Plan

## Purpose

Improve the generated ground so it reads as intentional, stylized terrain while preserving stable isometric gameplay.

The current ground should remain mostly flat and combat-friendly. This plan does not try to make the terrain visually interesting by adding lots of physical height noise. Instead, it separates:

- playable shape;
- authored surface identity;
- static generated masks;
- runtime surface state;
- future grass, snow, rain, mud, footprints, and material blending.

The desired result is a broad plane that is easy to walk and fight on, but whose surface looks like uneven land made of meaningful patches: snow cover, exposed dirt, damp low areas, compacted paths, moss, rocky patches, grass suitability, and later footprints or weather response.

## Current State

### Current Implementation Status After Patch T

The ground upgrade has moved beyond the original single snow-material improvement pass. The current system now has a real surface-style framework:

| Area | Status | Notes |
| --- | --- | --- |
| Dedicated ground shader | Implemented | `SH_PixelGroundSurfaceLit.shader` owns ground rendering separately from generated masses. |
| Static semantic masks | Implemented baseline | Vertex color and UV2 carry tonal, exposure, damp/deposit, vegetation, compaction, shore, rocky/dry, and authored standing-water/puddle-potential data. |
| Ground/corridor material contract | Implemented | `GeneratedGround` resolves visual state and applies it by `MaterialPropertyBlock`; river corridors remain dependent renderers. |
| Component-owned surface authoring | Implemented | `GeneratedGround` exposes top-level Surface Family and Surface Variant controls. |
| Asset-backed visual families | Implemented baseline | `GroundSurfaceStyleProfile` assets own visual families such as Snowfield and Wet Mudflat. |
| Asset-backed variants | Implemented baseline | `GroundSurfaceVariantRecipe` stores stable ids, display names, material controls, and feature recipes. |
| Feature-module recipe layer | Implemented baseline | `GroundSurfaceFeatureRecipe` supports explicit cost classes and the first shader-only features. |
| Snowfield family | Implemented baseline | `GSSP_Snowfield` and `GSP_Snowfield` exist. Variants are temporary art baselines. |
| Wet Mudflat family | Implemented baseline | `GSSP_WetMudflat` and `GSP_WetMudflat` exist. Patch Q intentionally reset the family to matte earth until explicit puddle/rut/debris features exist. |
| Style profile editor | Implemented in Patch R | Style assets now have a readable custom editor with variant cards, feature summaries, duplicate support, and validation warnings. |
| Ground modifier surface/height contract | Implemented in Patch T | `GroundModifier` can now affect height, authored surface masks, or both; legacy Flatten compaction behavior is preserved. |
| Runtime surface state | Not started | Wetness, snow depth, compression, footprints, and trample maps remain future work. |
| Explicit path/rut/track features | Contract ready | Patch T provides authored compaction, damp/deposit boost, and standing-water potential; visual feature modules still need to consume those masks. |

Current conceptual split:

```text
GroundSurfaceProfile
  semantic / mask-generation profile

GroundSurfaceStyleProfile
  visual family asset

GroundSurfaceVariantRecipe
  variant recipe inside a visual family

GroundMaterialControls
  material / shader response recipe

GroundSurfaceFeatureRecipe
  optional feature-module recipe with explicit cost class

GeneratedGround
  resolver, top-level authoring surface, and per-object override owner
```

Future terrain families must be added as style/profile assets, not as new hardcoded `GeneratedGround` enum branches.

Primary implementation files:

- `Assets/Game/Procedural/Ground/GeneratedGround.cs`
- `Assets/Game/Procedural/Ground/GroundGenerator.cs`
- `Assets/Game/Procedural/Ground/GroundModifier.cs`
- `Assets/Game/Procedural/Ground/GroundHeightFieldSnapshot.cs`
- `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs`
- `Assets/Game/Procedural/Ground/Editor/GroundSurfaceStyleProfileEditor.cs`
- `Assets/Game/Procedural/Ground/GroundSurfaceProfile.cs`
- `Assets/Game/Procedural/Ground/GroundSurfaceStyleProfile.cs`
- `Assets/Game/Procedural/Ground/GroundSurfaceVariantRecipe.cs`
- `Assets/Game/Procedural/Ground/GroundSurfaceFeatureRecipe.cs`
- `Assets/Game/Procedural/Ground/GroundMaterialControls.cs`
- `Assets/Game/Procedural/Core/MeshData.cs`
- `Assets/Game/Procedural/Core/MeshBuilder.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverGroundSnapshot.cs`
- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelGroundSurfaceLit.shader`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundForwardPass.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMaterialProperties.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelCellVariation.hlsl`
- `Assets/Game/Demo/Materials/Ground/M_PixelFrozenDirt.mat`

Related art and system documents:

- `Assets/Docs/Rock_Generated_Mass_Upgrade_Plan.md`
- `Assets/Docs/Proof of Concept/01_Visual_Language_and_Rendering.md`
- `Assets/Docs/Proof of Concept/05_Project_Application_Norse_Game.md`
- `Assets/Docs/Proof of Concept/06_Proof_of_Concept.md`

The original ground implementation already had useful foundations, and these remain relevant:

- `GroundRecipe` controls patch size, resolution, patch coordinate, transition slope, broad shape, roughness, surface detail, edge blending, and material variation.
- `GroundModifier` supports deterministic flatten, raise, and lower regions for authored traversal and scene composition.
- `StylizedRiverGroundSnapshot` lets rivers conceal broad ground below the dedicated river corridor.
- `GroundHeightFieldSnapshot` lets other systems sample pre-river height, normals, render normals, surface variation, and reserved material classification.
- `MeshData` supports vertex colors and optional UV2 data.
- `SH_PixelSurfaceLit.shader` already has generic pixel surface features such as broad variation, warped cell lookup, profile contrast, wetness, frost, semantic brightening/darkening, and material profile controls.

Original limitations that motivated this upgrade. Items marked as implemented are kept here as historical context rather than active blockers:

- [~] Ground shape and ground surface began coupled inside `GroundRecipe`; semantic and visual style ownership are now split, but future path/compaction work still needs clearer authored modifier rules.
- [x] `GroundProfile` only describes the heightfield family, not the material family. Material family ownership now lives in `GroundSurfaceStyleProfile`.
- [x] `BuildSurfaceMetadata` originally wrote one broad variation value and left material classification at `0`; it now writes semantic masks.
- [x] `BuildMeshData` originally wrote neutral vertex color channels; it now writes the documented vertex color/UV2 surface contract.
- [x] The ground originally had no object-owned material property block equivalent to `GeneratedMass`; `GeneratedGround` now applies resolved material controls through `MaterialPropertyBlock`.
- [x] The ground originally had no authored surface profile asset; `GSP_Snowfield` and `GSP_WetMudflat` now exist.
- [x] The ground originally had no static mask contract for snow potential, wetness potential, dirt/deposit, vegetation suitability, or terrain type blending; the baseline semantic contract now exists.
- [ ] The ground still has no runtime surface state texture for rain, footprints, snow compression, grass trampling, or mud/water accumulation.
- [~] Early material output read as pale, low-contrast procedural fuzz. Baseline Snowfield and Wet Mudflat now exist, but final detail still needs explicit feature modules and runtime state.

## Design Constraints

The upgrade must:

- preserve mostly flat, combat-stable gameplay terrain;
- avoid camera/player bobbing caused by excessive height variation;
- keep `GroundProfile` useful for broad physical shape only;
- add a separate surface/material profile system;
- keep existing generated ground scenes valid;
- keep river handoff behavior intact;
- keep ground modifier behavior intact;
- support future grass, wind response, rain response, snow accumulation, player footprints, and terrain type selection;
- prefer deterministic generated masks for static terrain identity;
- reserve runtime maps for changing state such as wetness, snow depth, footprints, and grass compression;
- keep shader contracts explicit and documented;
- avoid a large biome/world streaming system in the first pass;
- make the first visible improvement possible without authored texture assets.

The upgrade should not:

- turn the prototype ground into high-relief terrain;
- solve production terrain streaming;
- introduce destructible terrain;
- require a full vegetation system before surface profiles are useful;
- require final weather simulation before rain/snow channels are reserved;
- bake footprints into the generated mesh;
- treat every terrain type as a separate duplicated material;
- turn the generic pixel surface shader into an unreadable all-purpose monolith without contracts.

## Core Direction

The ground system should be split into four layers.

### 1. Playable Shape

Owned by:

- `GroundRecipe`
- `GroundProfile`
- `GroundModifier`
- river concealment

Purpose:

- define walkable height;
- preserve combat and camera stability;
- create broad readable slopes only where useful;
- allow authored flattening around combat, structures, paths, and crossings;
- conceal broad ground beneath river corridor geometry.

Rule:

- playable height should remain simple enough that an isometric far camera can ignore small terrain variation.

### 2. Static Surface Identity

Owned by:

- generated surface masks;
- selected `GroundSurfaceProfile`;
- optional patch coordinate;
- optional authored masks later.

Purpose:

- determine what the ground is made of;
- create broad stylized patches;
- decide where snow, dirt, wetness, moss, grass, rock, or sand should appear;
- provide deterministic masks to the shader, grass system, and weather system.

Rule:

- this layer should create most of the visible unevenness.

### 3. Runtime Surface State

Owned by:

- one or more world-space patch state textures;
- weather systems;
- player/enemy footprints;
- combat impacts;
- grass interaction;
- snow/rain/mud update logic.

Purpose:

- store changing states such as wetness, snow depth, compression, footprints, trampling, mud, and disturbance age.

Rule:

- this layer should not require mesh regeneration.

### 4. Rendering and Presentation

Owned by:

- ground surface shader;
- material property blocks;
- profile assets;
- state texture bindings;
- debug views.

Purpose:

- combine profile colors, generated masks, runtime maps, lighting, and pixel surface style into the final look.

Rule:

- visual richness should come from a small number of meaningful masks, not arbitrary noise.

## Proposed Architecture

### `GroundSurfaceProfile`

Create a new ScriptableObject profile type:

- `Assets/Game/Procedural/Ground/GroundSurfaceProfile.cs`

Suggested asset path:

- `Assets/Game/Demo/Profiles/Ground/`

Suggested asset names:

- `GSP_Snowfield.asset`
- `GSP_WetSoil.asset`
- `GSP_FertileSoil.asset`
- `GSP_DrySoil.asset`
- `GSP_MossyGround.asset`
- `GSP_RockyScrub.asset`
- `GSP_FrozenMud.asset`
- `GSP_DesertSand.asset`

`GeneratedGround` should expose one selected profile in the Inspector. In the first pass, a single selected profile is enough. Later, mixed terrain can blend multiple profiles using generated or authored masks.

Purpose:

- select terrain type from the Inspector;
- provide defaults for shader properties;
- control generated mask bias;
- tell future systems how grass, snow, rain, footprints, and mud should behave.

Suggested serialized fields:

- display name or profile id;
- base color;
- secondary patch color;
- exposed highlight color;
- wet color;
- snow color;
- moss/vegetation color;
- rocky/dry color;
- patch scale;
- patch contrast;
- patch edge softness;
- pixel variation strength;
- broad variation strength;
- cell warp strength;
- exposure brightening strength;
- deposit/damp darkening strength;
- snow eligibility;
- default snow amount;
- rain absorption;
- wetness darkening;
- wetness smoothness boost;
- mud formation;
- footprint visibility;
- footprint persistence;
- grass suitability;
- grass recovery speed;
- grass bend multiplier;
- rocky scatter suitability;
- dry dust response;
- gameplay friction modifier placeholder;
- surface audio/material tag placeholder.

The first implementation does not need to consume every field. The profile should reserve the concepts now so future systems have a stable home.

### `GroundSurfaceMasks`

Add a small data container generated alongside the heightfield.

Possible implementation:

- arrays inside `GroundGenerator.Generate`;
- copied into `GroundHeightFieldSnapshot`;
- written to vertex color and/or UV2.

Suggested channels:

```text
Vertex Color R = tonal patch variation
Vertex Color G = exposure / snow-hold / frost accumulation potential
Vertex Color B = damp/deposit/low-area potential
Vertex Color A = vegetation suitability or primary profile blend

UV2 X = compacted/path/flatten modifier influence
UV2 Y = river/shore wetness influence
UV2 Z = rocky/dry patch influence
UV2 W = reserved for authored mask or secondary terrain blend
```

If UV2 usage feels too broad for the first pass, start with vertex colors only and document UV2 as the next expansion. `MeshData` and `MeshBuilder` already support optional UV2, so no core mesh infrastructure should be needed.

### `GroundSurfaceRuntimeState`

Add later as a separate runtime component.

Suggested component:

- `GroundSurfaceStateRuntime`

Purpose:

- allocate and own per-ground-patch runtime state textures;
- expose world-to-state texture mapping;
- receive writes from weather, footprints, combat impacts, and grass/player interaction;
- bind textures to renderer material property blocks;
- optionally decay or diffuse wetness/footprints over time.

Suggested texture channel contract:

```text
Runtime State Texture 0
R = wetness amount
G = snow depth / snow cover amount
B = compression / footprint / trample amount
A = mud / standing water / disturbance age selector
```

Optional second texture later:

```text
Runtime State Texture 1
R = recent directional disturbance X packed or encoded
G = recent directional disturbance Z packed or encoded
B = grass bend impulse amount
A = reserved
```

Do not add this runtime component in the first visual patch unless needed. The plan should prepare static contracts so the runtime layer can be added without changing the conceptual model.

### Ground-Specific Shader Path

The current `SH_PixelSurfaceLit.shader` is generic but has many generated-mass-specific assumptions. Ground should either get:

- a dedicated `SH_GroundSurfaceLit.shader`; or
- a clearly separated ground mode inside `SH_PixelSurfaceLit.shader`.

Preferred long-term direction:

- create `Assets/Game/Rendering/PixelSurface/Shaders/SH_GroundSurfaceLit.shader` or `Assets/Game/Rendering/Ground/Shaders/SH_GroundSurfaceLit.shader`;
- share generic pixel/noise helper includes with stone;
- keep generated-mass local-height logic out of the ground path;
- bind ground profile values through material and property block.

The first patch can still reuse the existing pixel surface shader if it is faster, but the contract should not remain ambiguous.

## Terrain Type Model

Terrain types should be profile assets rather than hardcoded enum-only values.

Reason:

- designers can tune values without recompiling;
- new terrain families do not require enum migration;
- profile assets can reference material settings, grass settings, wetness behavior, snow behavior, and footstep behavior;
- mixed terrain can later blend profile assets.

An enum can exist as a convenience preset selector, but the source of truth should be profile assets.

### Suggested Initial Profiles

#### Snowfield

Visual language:

- pale blue-white base;
- broad soft snow islands;
- subtle exposed dirt or ice patches;
- restrained pixel variation;
- damp/shore areas slightly darker and smoother.

System behavior:

- high snow eligibility;
- footprints visible;
- compression persists moderately;
- grass mostly suppressed or buried;
- rain may darken, slush, or melt depending on future temperature state.

#### Wet Soil

Visual language:

- dark cold brown/grey;
- damp low patches;
- glossy riverbank response;
- compacted paths visible;
- snow only in exposed cold pockets.

System behavior:

- high rain absorption;
- wetness persists;
- footprints visible and dark;
- grass can grow but bends/tramples visibly;
- mud can form in low or compacted areas.

#### Fertile Soil

Visual language:

- richer dark earth;
- green moss/grass suitability patches;
- less pale frost unless weather demands it;
- strong grass density support.

System behavior:

- high vegetation suitability;
- moderate rain absorption;
- footprints visible through grass compression more than color alone;
- mud possible under heavy rain.

#### Dry Soil

Visual language:

- grey/tan cold dirt;
- dusty exposed patches;
- low wetness by default;
- sharper patch edges than snow.

System behavior:

- lower grass density;
- rain darkens quickly at first but may dry faster;
- footprints dusty or light rather than dark;
- mud requires sustained rain.

#### Mossy Ground

Visual language:

- muted green-brown;
- soft patch borders;
- dark damp recesses;
- low reflective wet sheen.

System behavior:

- high dampness retention;
- grass and moss overlays likely;
- footprints compress vegetation and darken slightly;
- snow can sit on top but breaks around traffic.

#### Rocky Scrub

Visual language:

- grey stone flecks;
- sparse soil pockets;
- low vegetation bands;
- stronger contrast around rock/dirt boundaries.

System behavior:

- low footprint visibility except in soil pockets;
- low grass density overall;
- rain pools in cracks;
- snow catches on exposed flat patches.

#### Frozen Mud

Visual language:

- blue-grey mud;
- wet dark low patches;
- pale frost on exposed ridges;
- occasional slick/ice-like highlights.

System behavior:

- footprints may crack/compress snow/frost;
- rain can create slick surfaces;
- grass mostly suppressed;
- good near rivers and cold banks.

#### Desert Sand

Visual language:

- warmer pale sand or ash;
- wind-shaped patch bands;
- low wetness by default;
- footprints bright/dark depending on slope and light.

System behavior:

- very high footprint visibility;
- low grass density;
- rain absorption can create temporary dark patches;
- wind can soften footprints later.

This list is exploratory. Implement only enough profiles to prove the profile system first.

## Playable Shape Policy

The ground mesh should support visual variety without destabilizing the camera, controller, or combat.

Recommended shape rules:

- default combat patches should stay within a very low local height amplitude;
- broad slopes are allowed only when authored or clearly intentional;
- `SurfaceDetail` should not be used as the main visual interest knob;
- high-frequency physical detail should remain subtle;
- paths, hut/camp areas, bridge approaches, and combat arenas should be flattened by modifiers;
- river banks can have visible shape because they are local focal areas;
- the shader can fake smaller bumps through color, stylized lighting, or normals later.

Suggested new controls:

- `Playable Flatness` or `Height Safety`
- `Visual Surface Patchiness`
- `Surface Detail Height`
- `Patch Contrast`
- `Patch Scale`

`Surface Detail Height` should remain low. `Patch Contrast` and `Patch Scale` should carry most visual richness.

## Static Mask Generation

Static masks should be generated from meaningful inputs.

Inputs:

- local ground position;
- world or patch coordinate;
- base height;
- relative height within patch;
- surface normal;
- slope;
- distance to river handoff;
- river side/bank influence;
- modifier influence;
- flatten/path influence;
- deterministic broad noise;
- deterministic cellular or value-noise patches;
- selected `GroundSurfaceProfile`.

Recommended generated masks:

### Tonal Patch Mask

Purpose:

- broad color islands and stylized uneven land patches.

Implementation:

- use low-frequency value noise or cellular noise;
- warp coordinates with another lower-frequency field;
- quantize or posterize to a small number of levels;
- keep edges soft enough to avoid noisy checkerboard patterns;
- scale and contrast from `GroundSurfaceProfile`.

### Exposure/Snow-Hold Mask

Purpose:

- determine where snow, frost, light dusting, or exposed highlights can sit.

Implementation:

- favor upward-facing normals;
- favor higher or exposed areas;
- reduce near wet shorelines if profile says snow melts there;
- break up with broad patch noise;
- keep stable across regeneration for a seed.

### Damp/Deposit Mask

Purpose:

- determine where dirt darkening, dampness, mud, or soil deposits should sit.

Implementation:

- favor lower relative heights;
- favor flatter basins;
- favor river/shore proximity;
- favor compacted/path areas depending on profile;
- oppose exposure where appropriate;
- use broad masks, not per-vertex random flecks.

### Vegetation Suitability Mask

Purpose:

- future grass and moss placement.

Implementation:

- profile bias controls base density;
- reduce in riverbed/concealment areas;
- reduce on rocky/dry patches;
- reduce in heavy snow unless grass is allowed to poke through;
- increase on fertile or mossy profiles;
- preserve route/combat readability by allowing modifier suppression.

### Shore Influence Mask

Purpose:

- river-aware ground material response.

Implementation:

- during `ApplyRivers` or a new mask pass, compute distance to river handoff;
- write a soft band around shore/corridor;
- let profile decide whether shore means wet, muddy, icy, mossy, eroded, or snowy.

### Compaction/Path Mask

Purpose:

- support authored paths, flattened combat areas, structure pads, and later foot traffic.

Implementation:

- extend `GroundModifierSnapshot` or add a surface-only modifier mode;
- track flatten modifier weight in a separate mask;
- let paths reduce grass, reduce snow, darken damp soil, or brighten dry dust depending on profile.

## Runtime Surface State

Runtime state is for changes after generation.

Future writers:

- player footsteps;
- enemy footsteps;
- rolling/dashing bodies;
- combat impacts;
- rain splashes;
- snow accumulation;
- snow melt;
- grass/trample interaction;
- river splash or overflow;
- magic/corruption effects.

Future readers:

- ground shader;
- grass renderer/simulation;
- footprint/decal system;
- audio/footstep system;
- gameplay surface queries;
- VFX spawn logic.

Recommended rules:

- runtime state should be world-space and patch-local;
- texture mapping should be stable for a ground patch;
- systems should write through one API rather than each binding textures manually;
- state decay should be profile-driven;
- generation should not erase runtime state unless explicitly regenerated;
- editor regeneration can reset runtime state unless play-mode persistence is later required.

### Footprints

Footprints should probably not be regular mesh deformation.

Preferred model:

- a footprint writer stamps compression into runtime state;
- compression darkens or lightens the shader depending on profile;
- compression bends/flattens grass;
- snow profiles show compacted snow and exposed dirt;
- mud profiles show darker wet impressions;
- sand/dry soil profiles show bright/dark rimmed impressions;
- rocky profiles show weak footprints except in soil pockets.

Important:

- footprints need direction and shape eventually, not only a circular stamp.
- first pass can use circular/ellipse stamps.
- avoid solving detailed boot-shape decals until the runtime state path works.

### Rain and Wetness

Rain should write wetness, but terrain profiles decide what wetness means.

Examples:

- snowfield: wetness creates slush/darker snow or melts to exposed dirt;
- wet soil: wetness darkens and increases smoothness quickly;
- fertile soil: wetness darkens soil and boosts moss/grass richness;
- dry soil: wetness creates temporary dark patches with faster drying;
- moss: wetness stays longer and increases saturation/gloss subtly;
- rocky scrub: wetness gathers in cracks and soil pockets;
- desert sand: wetness creates strong temporary dark patches, then dries.

### Snow Accumulation

Snow should write snow depth/coverage, not replace the base terrain type.

Rules:

- profile controls whether snow can accumulate;
- exposure mask controls where it accumulates first;
- compaction mask can reduce snow or change its shade;
- wetness can melt or darken snow;
- grass can be hidden, partially buried, or poke through depending on depth.

### Grass and Wind Interaction

Grass should be downstream of the ground profile and masks.

Ground should provide:

- vegetation suitability;
- snow cover;
- wetness;
- compression/trample;
- profile grass defaults.

Grass system should provide:

- wind bending;
- player/enemy bending;
- density and placement;
- optional color response to ground profile.

Shared runtime state should let one player movement event:

- bend grass;
- stamp compression;
- reveal a footprint;
- alter snow/mud/dust presentation.

## Inspector and Authoring UX

`GeneratedGround` should remain easy to use from the Inspector.

Recommended Inspector sections:

### Generation

- shape seed;
- live regeneration;
- new shape button;
- regenerate button.

### Playable Shape

- patch size;
- resolution;
- profile: flat, rolling, basin, ridge, uneven;
- broad form;
- roughness;
- surface detail height;
- transition direction and height;
- edge blend.

### Surface Profile

- selected `GroundSurfaceProfile`;
- surface seed if split from shape seed;
- patch scale;
- patch contrast;
- material variation;
- snow amount override;
- wetness override;
- grass density override;
- mask debug mode.

### Modifiers

- use modifiers;
- found modifiers;
- found river channels;
- future: surface-only modifiers count.

### Runtime State

Later, optional section:

- allocate runtime maps;
- clear wetness;
- clear footprints;
- clear snow compression;
- debug state texture.

### Debug Views

Ground debug should include:

- tonal patch mask;
- exposure/snow-hold mask;
- damp/deposit mask;
- vegetation suitability mask;
- shore influence mask;
- compaction/path mask;
- runtime wetness;
- runtime snow;
- runtime compression/footprints;
- final composite.

Debug views are important because a surface system with many masks can become impossible to tune by eye if every channel is hidden.

## Shader and Material Direction

The visible upgrade should move from "random pixel noise" to "stylized patch composition."

Shader goals:

- keep pixel-like quantized tonal variation;
- add broad hand-authored-looking patches;
- use generated masks as semantic inputs;
- use profile colors rather than arbitrary per-material duplication;
- support snow/wetness/dampness/compression in reserved channels;
- keep debug modes.

First visible shader features:

- broad patch color blend;
- exposure tint or snow overlay;
- damp/deposit darkening;
- shore wetness darkening;
- restrained pixel cell variation on top;
- optional profile contrast controls.

Future shader features:

- runtime wetness map;
- runtime snow map;
- runtime footprint/compression map;
- profile blending;
- grass/ground color harmonization;
- puddle or standing-water mask;
- stylized normals or fake small relief;
- profile-specific edge/noise behavior.

Material policy:

- prefer shared material plus property blocks and profile data;
- do not create one duplicated material per terrain instance;
- terrain type assets should control the look;
- material instances are acceptable only for distinct shader families or demo comparison.

## Interaction With River Work

The river corridor owns visible river water, bed, foam, and shoreline geometry. The broad ground should not try to become the riverbed, but it should react visually to the river.

Ground should receive or derive:

- distance to river/corridor;
- shore influence band;
- wet bank mask;
- snow suppression or slush near water;
- moss/dampness boost for suitable profiles;
- grass suppression inside the corridor;
- grass/vegetation change near the bank if desired.

Do not couple ground surface masks to foam internals. Foam is a water material system. Ground only needs the river domain/shore relationship.

## Interaction With Rock/Mass Work

The rock upgrade plan already defines a semantic material direction:

- deterministic surface variation;
- exposure/upward mask;
- crevice/base/deposit masks;
- broad low-frequency blotches;
- warped cell lookup;
- material profile controls.

Ground should borrow this philosophy but not copy rock implementation literally.

Differences:

- rocks have object-space height and side/base crevice masks;
- ground is mostly horizontal and patch-based;
- rocks can rely on local mesh bounds;
- ground needs world/patch-space masks, river proximity, modifier influence, and runtime state.

Shared principle:

- generated geometry should send semantic signals to the shader instead of expecting noise alone to carry style.

## Data Contracts

### Generated Mesh Contract - First Target

```text
UV0
X = normalized local patch X
Y = normalized local patch Z

Vertex Color
R = tonal patch variation
G = exposure / snow-hold potential
B = damp/deposit potential
A = vegetation suitability

UV2
X = compaction/path/flatten influence
Y = river/shore influence
Z = rocky/dry secondary patch
W = reserved secondary profile blend or authored mask
```

### Snapshot Contract

`GroundHeightFieldSnapshot` should eventually expose:

- base height;
- base normal;
- render normal;
- tonal patch variation;
- material classification or primary profile blend;
- exposure/snow-hold;
- damp/deposit;
- vegetation suitability;
- shore influence;
- compaction/path influence.

This helps future placement systems ask useful questions:

- can grass grow here?
- will snow accumulate here?
- is this point damp?
- is this point in a path/combat pad?
- is this near the river?
- what terrain material is dominant?

### Runtime State Texture Contract

```text
R = wetness
G = snow depth / snow amount
B = compression / footprint / trample
A = mud / standing water / disturbance age
```

### Material Property Contract

Suggested ground material properties:

- `_GroundBaseColor`
- `_GroundSecondaryColor`
- `_GroundExposureColor`
- `_GroundDampColor`
- `_GroundSnowColor`
- `_GroundMossColor`
- `_GroundPatchScale`
- `_GroundPatchContrast`
- `_GroundPatchEdgeSoftness`
- `_GroundSnowAmount`
- `_GroundWetness`
- `_GroundGrassSuitability`
- `_GroundFootprintStrength`
- `_GroundProfileBlend`
- `_GroundRuntimeStateTex`
- `_GroundRuntimeStateTex_TexelSize`
- `_GroundWorldToStateScaleOffset`
- `_GroundMaskDebugMode`

If the first implementation reuses `_BaseColor` and existing pixel properties, document the migration path to ground-specific names.

## Current Roadmap After Patch T

The original patch list below is retained for historical context, but the active roadmap is now organized around the asset-backed surface-style architecture introduced by Patches J through T.

Patch T implemented the modifier contract needed before path, rut, puddle, and trampled-wear features can become credible. `GroundModifier` can now affect height, authored surface masks, or both.

| Priority | Patch | Concrete goal |
| --- | --- | --- |
| 1 | Patch U — Trampled Wear Feature | Add a feature recipe that interprets compaction/path masks for trampled mud, worn earth, and future compacted snow/grass paths. |
| 2 | Patch V — Ground Surface Runtime State Stub | Add the no-cost API and binding contract for wetness, snow depth, compression, footprints, and disturbance state. |
| 3 | Patch W — Footprint / Compression Prototype | Stamp compression into runtime state and let Snowfield/Wet Mudflat interpret it differently. |
| 4 | Patch X — Rain / Wetness Prototype | Add wetness accumulation and drying through runtime state, not full-surface permanent material gloss. |
| 5 | Patch Y — Style/Feature Authoring Polish | Improve style/profile editing further only after more real content exposes actual authoring pain. |
| 6 | Patch Z — Grass Integration Contract | Connect vegetation suitability, runtime compression, and future grass rendering/trampling. |
| 7 | Future | Mixed Terrain / Profile Blending | Add explicit support for blended surface families such as snow over mud, rocky scrub over soil, or worn path through snow. |

Surface modifier note:

- Surface-only masks are preferred when the same visual effect can be achieved without changing playable height.
- Small denivelations are acceptable for roads, wagon tracks, camp pads, and other authored terrain features when they remain combat-safe and camera-stable.
- Snow paths and grass paths should eventually come from snow/grass accumulation and runtime interaction systems, not be hard-baked into the base ground as final content.
- Patch T inspected the current `GroundModifier` and ground mask code before implementing the path.

## Implementation Plan

### Patch 1 - Document, Baseline, and Safety Values

Status: not started.

Goal:

- capture the current ground behavior;
- avoid tuning blindly;
- establish safe gameplay height defaults.

Checklist:

- [ ] Add this plan document.
- [ ] Record current `Ground_Blockout` scene recipe values.
- [ ] Record current `M_PixelFrozenDirt` material values.
- [ ] Decide first-pass combat-safe recommended values for `BroadForm`, `Roughness`, and `SurfaceDetail`.
- [ ] Add a short note to the plan after the first visual comparison.
- [ ] Verify no existing ground scenes fail to regenerate.

Acceptance:

- the team has an agreed baseline and does not confuse mesh relief with surface richness.

### Patch 2 - Separate Shape From Surface Profile

Status: partially implemented on 2026-07-05.

Goal:

- add `GroundSurfaceProfile` without changing visible behavior yet.

Checklist:

- [x] Create `GroundSurfaceProfile.cs`.
- [x] Add a `GroundSurfaceProfile` serialized field to `GeneratedGround`.
- [x] Add a default/fallback profile behavior when no asset is assigned.
- [x] Update `GeneratedGroundEditor` with a `Surface Profile` section.
- [ ] Create a folder for demo ground profiles.
- [ ] Create `GSP_Snowfield.asset` as the first profile through Unity asset creation.
- [x] Keep all existing `GroundRecipe` serialized fields valid.
- [x] Do not require profile assets for old scenes to load.

Acceptance:

- `GeneratedGround` exposes a surface profile selector;
- existing ground regenerates with fallback behavior;
- assigning `GSP_Snowfield` changes no gameplay geometry.

### Patch 3 - Static Surface Mask Contract

Status: mostly implemented on 2026-07-05; river corridor metadata continuity corrected on 2026-07-08; Unity compile/scene validation still required.

Goal:

- upgrade generated surface metadata from one random variation channel to semantic masks.

Checklist:

- [x] Add an internal mask-generation path inside `GroundGenerator`.
- [x] Compute tonal patch variation from broad warped patch noise when a profile is assigned.
- [x] Compute exposure/snow-hold potential.
- [x] Compute damp/deposit potential.
- [x] Compute vegetation suitability.
- [x] Keep the old `SurfaceVariation` meaning approximately compatible through vertex color R.
- [x] Write semantic masks to vertex color G/B/A.
- [x] Reserve/write UV2 for path, shore, rocky, and authored masks.
- [x] Update `GroundHeightFieldSnapshot` to retain the important vertex-color mask values.
- [x] Update `GroundHeightFieldSnapshot` to retain secondary UV2 surface masks for dependent generated geometry.
- [x] Update river corridor geometry to copy sampled ground R/G/B/A and UV2 mask contracts instead of old neutral placeholder values.
- [x] Update comments documenting the vertex color/UV2 contract.

Acceptance:

- mask debug values can be inspected;
- existing material still renders;
- generated masks are stable for a seed and patch coordinate;
- visual improvement is possible without raising mesh height.

### Patch 4 - Modifier and River Surface Influence

Status: partially implemented on 2026-07-05; river corridor UV2 continuity corrected on 2026-07-08.

Goal:

- let authored flatten/path regions and river proximity affect surface masks.

Checklist:

- [x] Track flatten modifier influence during surface metadata generation.
- [x] Keep physical height modification separate from surface compaction/path influence.
- [ ] Add optional surface-only modifier mode or defer with a documented placeholder.
- [x] Compute river/shore influence separately from concealed trench height.
- [x] Reserve/write UV2 X for compaction/path influence.
- [x] Reserve/write UV2 Y for river/shore influence.
- [x] Make profile settings begin to bias shore and compaction mask response.
- [x] Ensure river concealment still does not affect visible render normals incorrectly.
- [x] Preserve corridor terrain-normal blending while freeing UV2 from the old terrain-integration meaning.

Acceptance:

- paths/combat pads can become visually compacted without needing more height changes;
- river banks can become damp/slushy/mossy by profile;
- river geometry ownership remains unchanged.

### Patch 5 - Ground Material Property Block

Status: partially implemented on 2026-07-05; river corridor renderer/property-block continuity corrected on 2026-07-08.

Goal:

- let each generated ground patch supply profile-driven material settings without duplicating material assets.

Checklist:

- [x] Add `MaterialPropertyBlock` support to `GeneratedGround`.
- [x] Expose the same profile material binding for dependent renderers such as generated river corridors.
- [ ] Bind base/profile colors.
- [~] Bind patch scale/contrast values. Contrast is bound; patch scale remains generated-data-only for now.
- [~] Bind wetness/snow/grass defaults. Static snow/damp/vegetation/rocky response is bound; runtime wetness is still deferred.
- [ ] Bind seed or patch coordinate values.
- [x] Refresh property block on enable, validate, regenerate, and profile change.
- [x] Keep shared material assignment intact.
- [x] Reapply the ground surface contract to the river corridor after corridor material assignment/rebuild.
- [ ] Add debug mode binding if available. Debug remains a material/shader setting for now.

Acceptance:

- two ground patches can use the same shared material but different profiles/colors;
- changing the profile updates only that ground patch;
- regeneration does not reset the selected profile.


### Patch B - Dedicated Ground Surface Shader Migration

Status: implemented in the dedicated ground shader split patch.

- [x] Created `SH_PixelGroundSurfaceLit.shader` as `PS3D/Pixel Ground Surface Lit`.
- [x] Added ground-only forward/material/debug include files.
- [x] Reused shared pixel-cell, ground-mask, and color utility includes.
- [x] Kept generated masses on `PS3D/Pixel Surface Lit`.
- [x] Migrated `M_PixelFrozenDirt` to the dedicated ground shader.
- [x] Removed generated-mass feature atlas and generated-mass local mask dependencies from the ground shader path.
- [x] Validated in Unity that generated ground and river corridor still visually match.


### Patch C - Ground Surface Mask Quality Pass

Status: implemented and Unity-validated.

Goal:

- improve the generated static masks before doing final snowfield/dampness art tuning.

Checklist:

- [x] Remove per-vertex random tonal sampling from the active surface mask path.
- [x] Add smoother multi-octave surface patch sampling for tonal, exposure, damp/deposit, and rocky/dry masks.
- [x] Rebalance exposure so snow-hold/debug data has more useful large-scale variation.
- [x] Rebalance damp/deposit so it is less dominated by broad height bands and shore alone.
- [x] Change shore influence from a full-width binary river band to a waterline/bank-weighted mask with softer bed and outer-bank falloff.
- [x] Preserve the existing vertex color and UV2 channel contract.
- [x] Validate `GroundTonal`, `GroundExposure`, `GroundDampDeposit`, `GroundShore`, and `GroundCombined` in Unity.

Acceptance:

- `GroundTonal` no longer shows obvious triangle/random-vertex artifacts;
- `GroundExposure` has useful but not noisy snow-hold variation;
- `GroundDampDeposit` loses rectangular/column-like dominance;
- `GroundShore` remains aligned to the river but is less brutally dominant;
- river corridor and generated ground remain visually continuous.

### Patch D - Ground Mask Contrast and Shore Restraint Pass

Status: implemented in code; Unity debug-view validation pending.

Goal:

- make the generated masks more useful before final snowfield/dampness material tuning by increasing exposure readability and preventing shore influence from dominating the combined ground surface data.

Checklist:

- [x] Increase profile-driven tonal patch contrast slightly without reintroducing per-vertex random artifacts.
- [x] Rebalance exposure so the dedicated exposure patch contributes more than broad height/up-facing terms alone.
- [x] Apply a centered contrast curve to exposure masks so `GroundExposure` reads more clearly from the gameplay camera.
- [x] Reduce shore contribution inside damp/deposit so river-adjacent areas do not overpower the entire damp mask.
- [x] Narrow and soften the shore influence band by reducing bank width, bed strength, outer-bank strength, and waterline-band amplitude.
- [x] Preserve the vertex color and UV2 channel contracts used by generated ground and river corridor meshes.
- [ ] Validate `GroundTonal`, `GroundExposure`, `GroundDampDeposit`, `GroundShore`, and `GroundCombined` in Unity.

Acceptance:

- `GroundExposure` is visibly more informative but not noisy;
- `GroundShore` remains aligned to the river/corridor handoff but no longer dominates the whole combined mask;
- `GroundDampDeposit` remains broad and soft while becoming less shore-led;
- `GroundCombined` shows balanced mask data rather than a river-band diagnostic;
- final mode remains at least as good as the prior patch;
- generated ground and river corridor remain visually continuous.


### Patch 6 - First Ground Shader Response

Status: partially implemented on 2026-07-05.

Goal:

- make the generated masks visibly useful.

Checklist:

- [x] Decide whether to add `SH_GroundSurfaceLit.shader` or a ground mode in `SH_PixelSurfaceLit.shader`. Current implementation uses a dedicated `PS3D/Pixel Ground Surface Lit` shader built from shared pixel-surface includes.
- [x] Reuse `PixelCellVariation.hlsl` or split shared pixel helpers cleanly.
- [x] Read vertex color R/G/B/A.
- [x] Read UV2 X/Y if written. UV2 Z is also read for rocky/dry response.
- [x] Add broad profile patch color blending.
- [x] Add exposure/snow-hold tint.
- [x] Add damp/deposit darkening.
- [x] Add shore influence response.
- [x] Keep small pixel cell variation restrained.
- [x] Add mask debug modes.
- [x] Create or update a ground material for the new shader path. `M_PixelFrozenDirt` now uses `PS3D/Pixel Ground Surface Lit`.

Acceptance:

- snowy field no longer reads as uniform pixel fuzz;
- broad land patches are visible from the isometric camera;
- texture richness does not depend on high physical terrain relief;
- debug modes clearly show each mask.

### Patch 7 - Terrain Profile Asset Set

Status: partially superseded by Patches L through R.

Goal:

- prove that multiple terrain families can be selected from the Inspector without duplicating materials or adding hardcoded `GeneratedGround` terrain-family branches.

Current result:

- `GroundSurfaceProfile` now owns semantic/mask-generation tendencies.
- `GroundSurfaceStyleProfile` now owns visual surface families.
- `GroundSurfaceVariantRecipe` now owns variants inside a family.
- `GeneratedGround` now exposes top-level Surface Family and Surface Variant controls.

Checklist:

- [x] Create `GSP_Snowfield`.
- [x] Create `GSSP_Snowfield`.
- [x] Create `GSP_WetMudflat`.
- [x] Create `GSSP_WetMudflat`.
- [x] Make style families selectable from `GeneratedGround` without manual asset dragging for common profiles.
- [x] Keep river corridor material response dependent on the parent ground, not on its own style state.
- [ ] Create future style/profile pairs such as Rocky Ground, Mossy Ground, Dry Dust, or Fertile Soil only after the current authoring and feature contracts remain stable.
- [ ] Add a demo comparison area or duplicate ground patch for visual checks.

Acceptance:

- selecting a different surface family changes terrain identity without changing the ground material asset;
- at least two terrain families are proven through the same style/profile architecture;
- shared shader/material-property-block architecture remains intact;
- future families can be added as assets before requiring new code.

### Patch 8 - Runtime State Design Stub

Status: not started.

Goal:

- add the minimal API shape for future weather/footprint/grass interaction without implementing full simulation.

Checklist:

- [ ] Create a `GroundSurfaceStateRuntime` component stub or design note.
- [ ] Define world-to-state mapping.
- [ ] Define runtime texture dimensions based on patch size and desired texels per metre.
- [ ] Define channel contract in code comments.
- [ ] Add methods such as `AddWetness`, `AddSnow`, `StampCompression`, and `ClearState` as stubs or no-op placeholders if appropriate.
- [ ] Do not add expensive updates until a writer exists.
- [ ] Decide whether runtime maps are allocated only in play mode or also in edit/debug mode.

Acceptance:

- future systems have a clear component/API target;
- the shader contract can reserve runtime texture bindings;
- no active performance cost is introduced if runtime state is disabled.

### Patch 9 - Footprint Prototype

Status: not started.

Goal:

- prove that player movement can alter ground surface visually without mesh regeneration.

Checklist:

- [ ] Implement runtime state texture allocation.
- [ ] Implement world-space stamp writing for compression.
- [ ] Bind runtime state texture to the ground material.
- [ ] Add simple circular or elliptical footprint stamps.
- [ ] Make `Snowfield` show compacted footprints.
- [ ] Make `WetSoil` show darker wet impressions.
- [ ] Make `DrySoil` show lighter/dusty impressions if profile supports it.
- [ ] Add decay/persistence controlled by profile.
- [ ] Add debug view for compression channel.

Acceptance:

- walking over ground leaves visible profile-appropriate marks;
- marks do not require mesh changes;
- marks align with world position and patch mapping.

### Patch 10 - Rain/Wetness Prototype

Status: not started.

Goal:

- prove that weather can write into the same runtime state model.

Checklist:

- [ ] Add wetness writes to runtime state.
- [ ] Add drying/decay controlled by profile.
- [ ] Make wetness darken and/or smooth ground based on profile.
- [ ] Let wetness interact with snow amount in a simple way.
- [ ] Add debug view for wetness channel.
- [ ] Add an editor/play-mode test button to apply rain to the whole patch.
- [ ] Add localized rain/splash writer if needed.

Acceptance:

- wetness response differs between snow, soil, moss, rock, and dry profiles;
- wetness is visible but does not obliterate the terrain profile;
- runtime texture decay is stable.

### Patch 11 - Grass Integration Contract

Status: not started.

Goal:

- make ground profiles and masks useful to the future grass system.

Checklist:

- [ ] Expose vegetation suitability sampling from `GeneratedGround`.
- [ ] Expose snow/wetness/compression state sampling if runtime maps exist.
- [ ] Define how grass placement uses `GroundSurfaceProfile`.
- [ ] Define how grass color uses ground profile colors.
- [ ] Define how grass bending writes or reads runtime compression.
- [ ] Reserve combat/path suppression behavior.
- [ ] Document grass density expectations per initial profile.

Acceptance:

- grass placement can be derived from ground data instead of a separate unrelated noise field;
- player interaction can affect both grass and ground state through shared concepts.

### Patch 12 - Mixed Terrain and Authored Masks

Status: not started.

Goal:

- support patches that are not a single uniform terrain type.

Checklist:

- [ ] Add optional secondary `GroundSurfaceProfile`.
- [ ] Use a generated blend mask or authored mask to mix primary and secondary profiles.
- [ ] Support profile blend in shader.
- [ ] Add profile blend to snapshot data.
- [ ] Allow modifiers to bias terrain blend for paths, camp pads, banks, or rocky zones.
- [ ] Keep single-profile workflow simple.

Acceptance:

- one ground patch can blend snowfield with exposed dirt or mossy bank without separate meshes;
- the Inspector remains usable.

## Validation Plan

### Visual Validation

Checklist:

- [ ] Test from the actual game camera.
- [ ] Test close editor inspection only after camera validation.
- [ ] Compare old snowfield material against new profile material.
- [ ] Confirm broad patching is visible but not noisy.
- [ ] Confirm pixel variation is secondary, not the main read.
- [ ] Confirm paths/banks/flat areas have believable material response.
- [ ] Confirm rocks, river, and ground still belong to the same palette.

### Gameplay Validation

Checklist:

- [ ] Walk across the patch without distracting vertical bob.
- [ ] Fight or simulate combat movement on the patch.
- [ ] Verify hit/telegraph readability over the ground.
- [ ] Verify bridge and river crossing remain clear.
- [ ] Verify camera does not need to chase tiny height changes.
- [ ] Verify flatten modifiers still preserve playable spaces.

### Technical Validation

Checklist:

- [ ] Regenerate ground in edit mode.
- [ ] Change surface profile and verify material updates.
- [ ] Change shape seed and verify selected profile persists.
- [ ] Verify `MeshData.Validate` passes.
- [ ] Verify UV2 count matches vertex count when used.
- [ ] Verify material property block does not instantiate materials.
- [ ] Verify no river corridor rebuild regressions.
- [ ] Verify shader compiles in URP.

### Debug Validation

Checklist:

- [ ] Inspect tonal patch mask.
- [ ] Inspect exposure/snow-hold mask.
- [ ] Inspect damp/deposit mask.
- [ ] Inspect vegetation suitability mask.
- [ ] Inspect shore influence mask.
- [ ] Inspect compaction/path influence mask.
- [ ] Inspect runtime wetness if implemented.
- [ ] Inspect runtime snow if implemented.
- [ ] Inspect runtime compression if implemented.

## Suggested Initial Tuning

For the current snowy prototype clearing:

- keep `GroundProfile.Uneven` if desired, but lower physical amplitude;
- keep `BroadForm` modest for combat spaces;
- keep `SurfaceDetail` low enough that it does not read as bumpy navigation;
- make material patch scale larger than individual mesh cells;
- reduce reliance on tiny pixel noise;
- introduce broad cold/warm or pale/damp land patches;
- darken near river/shore where appropriate;
- use profile snow amount rather than baking snow into the base color alone.

Possible starting values:

```text
Ground shape
BroadForm: 0.35 to 1.25 for combat-safe uneven fields
Roughness: 0.25 to 0.55
SurfaceDetail: 0.05 to 0.22

Snowfield surface profile
PatchScale: 6 m to 14 m
PatchContrast: 0.18 to 0.35
PatchEdgeSoftness: 0.35 to 0.65
PixelVariation: 0.02 to 0.06
BroadVariation: 0.04 to 0.10
SnowAmount: 0.65 to 0.95
Wetness: 0.0 to 0.15 baseline
```

These are only starting points. The real test is readability from the isometric camera.

## Open Questions

- Should ground have a dedicated shader now, or should the first patch extend `SH_PixelSurfaceLit.shader`?
- Should `GroundSurfaceProfile` live under `Procedural/Ground` or under a broader rendering/material-profile namespace?
- Should surface-only modifiers be part of `GroundModifier`, or should they be a separate `GroundSurfaceModifier` component?
- Should vegetation suitability be vertex color A in the first mask patch, or should A remain reserved for terrain blending?
- Should shore influence be generated by ground from river snapshots, or should the river provide a richer surface-response snapshot?
- How much runtime state resolution is needed for footprints from the game camera?
- Should footprints be stored in the runtime state texture only, or combined with decal meshes for close-up/debug views?
- Should terrain profile assets also define footstep audio and gameplay friction, or should they expose separate tags for other systems?
- Should snow/rain simulation be local per patch or driven by a global weather manager that writes to registered patches?

## Risks

### Too Much Height Detail

Risk:

- terrain becomes visually richer but damages camera/player comfort.

Mitigation:

- keep physical height detail low;
- put most variety in static masks and shader response;
- validate from gameplay camera first.

### Shader Becomes Too Broad

Risk:

- the generic pixel surface shader accumulates unrelated rock, ground, weather, and vegetation assumptions.

Mitigation:

- create a dedicated ground shader path or cleanly separated include functions;
- document property contracts;
- keep debug modes.

### Profiles Become Premature Biome System

Risk:

- too many terrain types are added before one looks good.

Mitigation:

- implement `Snowfield` first;
- add only enough additional profiles to prove the architecture;
- defer production biome/world assembly.

### Runtime State Overbuild

Risk:

- footprint/weather infrastructure is built before any visible use case needs it.

Mitigation:

- reserve the contract early;
- implement texture allocation only when adding footprints or rain prototype.

### Mask Ambiguity

Risk:

- channels mean different things to different systems.

Mitigation:

- keep a channel contract in code comments and this document;
- expose debug views;
- avoid reusing a channel for incompatible meanings.

## Deferred Work

Defer until the basic profile/mask/shader path is proven:

- production terrain streaming;
- destructible terrain;
- full biome graph;
- authored texture painting UI;
- detailed boot-shape footprints;
- puddle fluid simulation;
- erosion simulation;
- vegetation rendering implementation;
- persistent save/load of runtime footprint and weather state;
- large-scale weather manager;
- snow depth geometry displacement;
- triplanar authored texture sets;
- terrain LOD system.

## Definition of Done for First Milestone

The first milestone is complete when:

- `GeneratedGround` has a surface profile selector;
- the snowfield profile drives material settings;
- generated ground writes semantic masks beyond random red-channel variation;
- the shader visibly uses those masks;
- the current clearing ground reads as broad stylized land patches from the game camera;
- physical terrain remains comfortable for isometric movement and combat;
- river shore influence is at least reserved, ideally visible;
- future runtime state channels for wetness, snow, footprints, and grass are documented in code or this plan;
- at least one debug view can show the new masks.

## Working Checklist Summary

- [ ] Patch 1 - Document, baseline, and safety values.
- [~] Patch 2 - Separate shape from surface profile. Core code implemented; demo asset still pending Unity asset creation.
- [~] Patch 3 - Static surface mask contract. Core code implemented; corridor metadata continuity corrected; Unity validation still required.
- [~] Patch 4 - Modifier and river surface influence. Flatten, shore mask influence, and corridor UV2 continuity implemented; surface-only modifier mode still pending.
- [~] Patch 5 - Ground material property block. First profile-to-material binding implemented and shared with generated river corridors; color/seed/debug binding still deferred.
- [~] Patch 6 - First ground shader response. Dedicated ground shader, final response, and debug modes implemented; material asset tuning still pending.
- [x] Patch C - Ground surface mask quality pass. Implemented and Unity debug-view validated.
- [x] Patch D - Ground mask contrast and shore restraint pass. Implemented and Unity debug-view validated; shore/exposure needed one more focused correction.
- [x] Patch E - Shore semantic correction and exposure/combined debug balance. Implemented and Unity debug-view validated; audit showed shore still used the wrong owner/model.
- [x] Patch F - Ground shore model and mask diagnostics. Implemented and Unity-validated: generated-ground shore statistics are now low and broad; diagnostics exposed exposure saturation as the next blocker.
- [x] Patch G - Exposure distribution normalization and corridor-shore restraint. Implemented and Unity-validated: exposure now has a healthy p05/p95 distribution and corridor-side shore is restrained enough for material-response work.
- [~] Patch H - Ground snowfield visual response pass. Implemented: the dedicated ground shader now uses the validated tonal/exposure/damp/shore masks for stronger snow tint, broad patch identity, and damp/deposit darkening. Unity validation pending.
- [x] Patch J - Ground visual presets and component-owned material controls. Implemented: generated-ground material response controls now live on `GeneratedGround`, apply through material property blocks, and refresh river corridors without requiring geometry rebuild.
- [~] Patch K - Surface Type / Surface Variant architecture and stronger snowfield recipes. Implemented in code/docs; Unity visual tuning pending.
- [ ] Patch 7 - Terrain profile asset set.
- [ ] Patch 8 - Runtime state design stub.
- [ ] Patch 9 - Footprint prototype.
- [ ] Patch 10 - Rain/wetness prototype.
- [ ] Patch 11 - Grass integration contract.
- [ ] Patch 12 - Mixed terrain and authored masks.

### 2026-07-08 — Patch I: Ground Visual Scale Cleanup

Implemented after the first snowfield visual-response pass made the final ground read as too granular from the isometric camera. The ground masks were kept unchanged; the fix is limited to the dedicated ground shader/material response.

- Added `_GroundMacroPatchScale` to `PS3D/Pixel Ground Surface Lit` so macro snowfield variation is measured in terrain metres instead of deriving from `_PixelCellSize * 8`.
- Reduced `M_PixelFrozenDirt` fine pixel variation/warp to avoid repeated mottling across the ground plane.
- Reworked snow response so `_GroundSnowBrightness` handles value lift and `_GroundSnowTintStrength` controls value-preserving hue shift toward `_FrostColor`.
- No generated-ground mask generation, river corridor, water, foam, or generated-mass shader code changed.

### 2026-07-08 — Patch J: Ground Visual Presets and Component-Owned Material Controls

Implemented after the snowfield baseline became visually acceptable but too difficult to author through the shared material asset. The ground material asset remains a shared shader backend; per-ground visual response is now owned by `GeneratedGround` and pushed through renderer `MaterialPropertyBlock`s.

- Added the first `GroundVisualPreset` implementation with `Clean Snowfield`, `Patchy Snowfield`, `Dirty / Thawing Snowfield`, and `Wind-Scoured Snowfield` options.
- Added serialized `GroundMaterialControls` on `GeneratedGround` for pixel variation, broad variation, vertex variation, cell warp, patch blend, macro patch scale, snow tint, snow brightness, damp darkening, and frost colour.
- Extended `GeneratedGround.ApplySurfaceProfileMaterialProperties(Renderer)` so these visual controls are applied per renderer through property IDs for `_PixelVariation`, `_PixelBroadVariation`, `_PixelVertexVariation`, `_PixelWarpStrength`, `_GroundPatchBlendStrength`, `_GroundMacroPatchScale`, `_GroundSnowTintStrength`, `_GroundSnowBrightness`, `_GroundDampDarkenStrength`, and `_FrostColor`.
- Added a `GeneratedGroundEditor` preset dropdown and compact `Advanced Material Controls` foldout under the existing Surface section. Changing presets writes the bundled values into the serialized controls; manually editing a control marks the preset as `Custom`.
- Added a generation-signature guard in `GeneratedGround.OnValidate()` so material-only control edits refresh material property blocks instead of forcing a ground/corridor geometry regeneration.
- Added `StylizedRiver.RefreshCorridorMaterialProperties()` so ground visual changes can resync the existing river corridor renderer without rebuilding corridor meshes.
- No material duplication, mask generation changes, generated-mass shader changes, or river geometry changes were introduced.

### 2026-07-08 — Patch K: Surface Type / Surface Variant Architecture

Implemented after visual validation showed that the Patch J presets were too similar and that `Dirty / Thawing Snowfield` appeared as a nested Unity menu item because `/` was interpreted as a submenu separator. Patch K starts the long-term surface-style architecture while keeping the current implementation cheap and reversible.

Current authoring model:

```text
GeneratedGround
  Surface Profile        -> semantic/mask-generation asset
  Surface Type           -> visual family, currently Snowfield
  Snowfield Variant      -> Clean / Patchy / Dirty Thawing / Wind-Scoured / Custom
  Advanced Material Controls -> per-object visual recipe overrides
```

Important architecture decisions:

- `Surface Type` is intentionally not called biome. A biome is a world/ecology concept; this control is a renderer/terrain-surface family.
- `GroundSurfaceProfile` remains the source for generated mask tendencies such as exposure, damp/deposit, vegetation suitability, rocky/dry suitability, snow eligibility, and rain absorption.
- `GroundSurfaceType` and the per-type variant select a final visual recipe that interprets those masks.
- Variant edits are visual-only and must continue to refresh material property blocks without rebuilding ground or river-corridor geometry.
- The current enum-backed implementation is a stepping stone. The expected final form is asset-backed `GroundSurfaceStyleProfile` / variant assets once more than one surface type exists and once the required feature vocabulary is known.

Patch K changes:

- Replaced the flat `Ground Visual Preset` authoring concept with `Surface Type` plus `Snowfield Variant`.
- Renamed `Dirty / Thawing Snowfield` to `Dirty Thawing`, removing the slash that caused Unity to display a nested dropdown submenu.
- Expanded `GroundMaterialControls` so variants now drive a full visual recipe instead of only ten mild material values.
- Added per-ground control over base colour, frost colour, damp/rocky/vegetation tint colours and tint strengths, pixel cell size, tone count, cluster strength, pixel effect strength, profile contrast scales, semantic response scales, wetness/finish controls, frost response, monolithic flattening, smoothness, and specular strength.
- Added ground shader properties for `_GroundDampTint`, `_GroundDampTintStrength`, `_GroundRockyDryTint`, `_GroundRockyDryTintStrength`, `_GroundVegetationTint`, and `_GroundVegetationTintStrength`.
- Updated `PixelSurfaceGroundForwardPass.hlsl` so damp, rocky/dry, and vegetation responses can shift hue through value-preserving tint targets instead of being fixed hard-coded colour multipliers.
- Strengthened the four snowfield recipes so they are intentionally more distinct at game-camera distance: clean is quiet/cold, patchy increases rocky/dry and macro contrast, dirty thawing increases warm damp/shore/wet response and lowers snow purity, and wind-scoured suppresses dirt/detail while flattening into larger cold plates.

Near-term limitation:

- Wind-scoured ground still lacks true directional streak geometry/noise. The current recipe can make it cleaner, colder, flatter, and broader, but a convincing scoured/swept snowfield will need a directional surface-feature module later.

Future architectural target:

```text
GeneratedGround
  GroundSurfaceProfile         // mask generation / terrain semantic tendencies
  GroundSurfaceStyleProfile    // visual surface family, e.g. Snowfield, Mudflat, Rocky Ground
  Style Variant                // Clean, Patchy, Dirty Thawing, Wind-Scoured, etc.
  Advanced Overrides           // local per-object deviation from the selected variant
```

Do not add dozens of hardcoded surface types indefinitely. When the second or third surface type is introduced, move from enum recipes to style-profile assets so new ground families can be authored without expanding `GeneratedGround.cs` into a preset registry.

---

## Patch J-L Implementation Update: Ground Visual Authoring and Style Profiles

### Patch J — GeneratedGround material controls

Patch J moved the normal ground visual authoring path from material-asset editing to the `GeneratedGround` component.

Implemented direction:

- `GeneratedGround` owns per-ground visual material controls.
- The shared ground material remains a backend/default asset.
- Ground visual values are applied through `MaterialPropertyBlock`.
- River corridor renderers receive the resolved parent-ground property block instead of owning a separate ground style.
- Visual-only control changes refresh material properties without requiring ground or corridor mesh regeneration.

This established the correct renderer path:

```text
GeneratedGround resolves visual controls
→ applies MaterialPropertyBlock to its renderer
→ refreshes child StylizedRiver corridor material properties
```

Material duplication is intentionally avoided.

### Patch K — Surface Type / Snowfield Variant bridge

Patch K replaced the flat temporary `Ground Visual Preset` concept with an explicit hierarchy:

```text
Surface Type: Snowfield
Snowfield Variant: Clean / Patchy / Dirty Thawing / Wind-Scoured / Custom
```

It also expanded snowfield variants from small value tweaks into fuller visual recipes controlling palette, semantic response, pixel/macro variation, wetness, frost, smoothness, and specular response.

Patch K was a bridge, not the final architecture. Its enums made the hierarchy clearer, but hardcoded terrain families and hardcoded recipe switches would not scale to muddy, rocky, waterlogged, desert, or future feature-heavy surface families.

### Patch L — Ground Surface Style Profile architecture

Patch L introduces the asset-backed architecture that future ground families should use.

The conceptual split is now:

```text
GroundSurfaceProfile
  Semantic/mask-generation profile.
  Controls generated surface-mask tendencies such as exposure,
  damp/deposit, vegetation suitability, rocky/dry suitability,
  snow eligibility, and rain absorption.

GroundSurfaceStyleProfile
  Visual surface family asset.
  Owns a default GroundSurfaceProfile and a list of variant recipes.
  Example: Snowfield.

GroundSurfaceVariantRecipe
  One named visual recipe inside a style profile.
  Uses a stable id such as snowfield.clean or snowfield.dirty_thawing.
  Owns GroundMaterialControls.

GroundMaterialControls
  Renderer/material response recipe.
  Contains palette, pixel/macro variation, semantic response,
  weather/finish, and shader response values.

GeneratedGround
  Resolver and per-object override owner.
  Selects a GroundSurfaceStyleProfile and variant id, optionally overrides
  the semantic profile and/or material controls, then pushes the resolved
  result through MaterialPropertyBlock.
```

Current asset path:

```text
Assets/Game/Demo/Profiles/Ground/Styles/GSSP_Snowfield.asset
```

Current Snowfield variant ids:

```text
snowfield.clean
snowfield.patchy
snowfield.dirty_thawing
snowfield.wind_scoured
```

The Inspector now treats style data as asset-owned by default:

```text
Surface Style Profile: Snowfield
Surface Variant: Clean / Patchy / Dirty Thawing / Wind-Scoured
Override Surface Profile: optional
Advanced Material Overrides: optional local custom copy
```

Important behavior:

- Selecting a variant uses the recipe from the style asset.
- Advanced material overrides are local to the selected `GeneratedGround` object.
- Enabling material override copies the currently resolved recipe first, so local edits start from the selected variant.
- Existing Patch K enum data is retained only for migration and compatibility.
- The active material recipe should no longer be hardcoded inside `GeneratedGround` for future styles.

### Rules for future ground families

Do not add future terrain families as hardcoded enums in `GeneratedGround`.

Do not add large `switch` blocks for Mudflat, Rocky Ground, Desert, Waterlogged Ground, and similar families.

Do not duplicate material assets per variant.

Do not make river corridors own ground style state. They should continue to receive the resolved parent-ground renderer contract through material property blocks.

Do not merge `GroundSurfaceProfile` and `GroundSurfaceStyleProfile` yet. The semantic mask-generation profile and the visual style family are related but not the same layer.

The expected path for a new visual family is:

```text
Create a GroundSurfaceStyleProfile asset
→ assign or create its default GroundSurfaceProfile
→ add variant recipes
→ only add code if a truly new shader/feature module is needed
```

### Next architecture step after Patch L

The next scalable addition should be feature-module support inside style variants, not another hardcoded terrain-family branch.

Potential future variant feature modules:

- directional snow streaks;
- melt patches;
- pebble or scree scatter;
- mud crust cracks;
- wet pooled lowlands;
- trampled path wear;
- frosted rock dust.

Each future feature should declare whether it is shader-only, mesh-mask driven, texture/atlas driven, or runtime-state driven, so styles only pay for the features they actually use.

### Patch M — Surface Variant Feature Module Foundation

Patch M adds the first feature-module layer inside the asset-backed ground style architecture.

The important architectural change is that a `GroundSurfaceVariantRecipe` is no longer only a material-control preset. It can now own optional `GroundSurfaceFeatureRecipe` entries. This lets a variant define a small feature vocabulary without adding terrain-family branches to `GeneratedGround`.

The new feature data types are:

```text
GroundSurfaceFeatureKind
  Names reusable feature modules such as Directional Streaks, Melt Patches,
  Pooled Wetness, Pebble Scatter, Mud Crust Cracks, Trampled Wear, and
  Frosted Rock Dust.

GroundSurfaceFeatureCostClass
  Declares the broad cost bucket: Shader Only, Mesh Mask Driven,
  Generated Texture, or Runtime State.

GroundSurfaceFeatureRecipe
  A per-variant feature entry containing kind, enabled state, cost class,
  strength, scale, contrast, mask influence, direction, and seed offset.
```

Patch M intentionally implements only one renderable proof feature:

```text
Directional Streaks
  Cost class: Shader Only
  Owner: GroundSurfaceVariantRecipe
  Resolver: GeneratedGround
  Renderer path: MaterialPropertyBlock
  Shader path: Pixel Ground Surface Lit
```

Directional Streaks exists because wind-scoured snow, sand, ash, and dust cannot be represented convincingly by colour and macro-noise sliders alone. The first implementation is deliberately cheap: it uses world-position noise, a stable direction vector, the existing pixel seed, and the selected variant's feature recipe. It does not allocate textures, add atlases, change generated mesh data, or create runtime state.

The renderer contract added by Patch M is:

```text
_GroundFeatureMode
_GroundFeatureStrength
_GroundFeatureScale
_GroundFeatureContrast
_GroundFeatureMaskInfluence
_GroundFeatureDirection
_GroundFeatureSeed
```

`GeneratedGround` resolves the selected style variant, picks the first enabled shader-only feature, and pushes those values through the existing material-property-block path. If no shader-only feature is active, it writes neutral feature values. River corridor renderers remain style-agnostic and continue to receive the resolved parent-ground material contract through the same property block refresh path.

Current Snowfield feature usage:

```text
Clean
  Weak Directional Streaks, mostly masked to snow/exposure.

Patchy
  Mild Directional Streaks, still secondary to patch variation.

Dirty Thawing
  No directional streak feature in Patch M; its identity remains damp/melt-biased.

Wind-Scoured
  Strong Directional Streaks, broad scale, lower semantic masking.
```

Patch M does not implement melt patches, pebble scatter, mud cracks, trampled wear, or frosted rock dust yet. Pooled Wetness is implemented in Patch N as the second shader-only proof feature. Remaining kinds are valid feature kinds in the asset contract, but each should only become renderable when it has a concrete cost model and visual need.

Rules after Patch M:

- Do not add a new hardcoded enum branch to `GeneratedGround` for every future terrain family.
- Do not add all features to every style at full runtime cost.
- Do not add generated textures or atlases until a feature demonstrably needs them.
- Do not make river corridors understand style names or feature kinds.
- Keep feature recipes variant-owned and renderer application resolved by `GeneratedGround`.
- Keep the material-property-block path as the final per-renderer contract.

The next architectural proof should be a second style family or a second cheap feature, not a large feature explosion. A good next candidate is either a minimal Mudflat/Waterlogged style using existing material controls, or a shader-only Pooled Wetness feature if the snowfield/river-adjacent ground needs more expressive thaw/melt response.

### Patch N — Second Surface Family Proof and Pooled Wetness

Patch N proves that the Patch L/M architecture can add a second visual ground family without adding a hardcoded terrain-family branch to `GeneratedGround`.

New assets:

```text
Assets/Game/Demo/Profiles/Ground/GSP_WetMudflat.asset
Assets/Game/Demo/Profiles/Ground/Styles/GSSP_WetMudflat.asset
```

`GSP_WetMudflat` is the semantic/mask-generation profile for wet mud: high damp/deposit tendency, high rain absorption, low snow eligibility, and high footprint visibility. It reuses the existing generated ground vertex/UV2 mask contract; no new mesh channels are added.

`GSSP_WetMudflat` is the visual style profile. Its variants are:

```text
mudflat.damp_mud
  balanced damp mud, moderate pooled wetness.

mudflat.waterlogged
  darker, wetter, smoother, strongest pooled wetness.

mudflat.trampled
  higher contrast, compacted-looking mud response, moderate pooled wetness.

mudflat.frozen_thaw
  colder thawing mud, partial frost response, lighter pooled wetness.
```

Patch N also makes `Pooled Wetness` a renderable shader-only feature. It uses the same feature property-block contract added in Patch M:

```text
_GroundFeatureMode
_GroundFeatureStrength
_GroundFeatureScale
_GroundFeatureContrast
_GroundFeatureMaskInfluence
_GroundFeatureDirection
_GroundFeatureSeed
```

Feature mode values are currently:

```text
0 = no shader-only feature
1 = Directional Streaks
2 = Pooled Wetness
```

Pooled Wetness is deliberately cheap. It uses world-position procedural noise, damp/deposit mask, shore mask, rocky/dry suppression, the feature recipe seed, and the selected variant's strength/scale/contrast/mask influence. It darkens and damp-tints local pools and adds local smoothness/specular response in the ground shader. It does not allocate textures, add atlases, generate new mesh data, or create runtime state.

The important architectural result is this workflow:

```text
new style family
→ new GroundSurfaceProfile asset
→ new GroundSurfaceStyleProfile asset
→ variant recipes with material controls and feature recipes
→ GeneratedGround resolves selected style/variant generically
→ MaterialPropertyBlock pushes the resolved contract
```

No terrain-family switch was added to `GeneratedGround`. The river corridor remains style-agnostic and continues to receive the parent ground's resolved material-property block.

Rules after Patch N:

- Add future ground families as `GroundSurfaceStyleProfile` assets, not `GeneratedGround` enum branches.
- Add future visual vocabulary as `GroundSurfaceFeatureRecipe` entries, with explicit cost class.
- Keep shader-only features cheap and procedural until a feature proves it needs texture/atlas/state support.
- Do not make river corridor code understand surface style names.
- Do not polish every variant before proving the architecture; visual tuning belongs after the contract is stable.

The next recommended step is authoring UX: create a compact custom editor for `GroundSurfaceStyleProfile` assets so variant IDs, material controls, and feature recipes are easier to edit and validate before many more styles are added.


### Patch O — Generated Ground Surface Authoring UX

Patch O moves the normal surface-family workflow to the top of the `GeneratedGround` Inspector.

Patch L and Patch M made ground styles asset-backed, but Patch N exposed an authoring problem: users had to manually drag `GroundSurfaceStyleProfile` assets onto the generated ground object and scroll down to find the style and variant controls. That is acceptable for a technical proof, but not for regular level-authoring.

Patch O keeps the asset-backed architecture and changes only the authoring path.

The top of the `GeneratedGround` Inspector now begins with:

```text
Ground Surface
  Surface Family
  Surface Variant
  Override Surface Profile
  Resolved Surface Profile
  Feature Summary
  Advanced Style Asset
```

`Surface Family` is an editor-populated dropdown. The editor discovers `GroundSurfaceStyleProfile` assets from:

```text
Assets/Game/Demo/Profiles/Ground/Styles
```

and falls back to all project `GroundSurfaceStyleProfile` assets if none are found in that folder. This means normal authoring can switch between families such as `Snowfield` and `Wet Mudflat` without manually dragging assets.

`Surface Variant` is populated from the selected style profile's variant recipes. Switching family assigns the chosen style asset, validates the stored variant id, and falls back to the first valid variant if the previous id does not exist in the new family.

Patch O also adds top-level authoring validation warnings for:

- missing style profile;
- missing default surface profile on a style;
- missing or empty variant lists;
- stored variant id not present in the selected style;
- duplicate variant ids inside a style asset.

The raw style asset reference still exists under `Advanced Style Asset` for custom or externally stored profiles, but it is no longer the primary workflow.

Patch O does not change rendering, shader behavior, feature recipes, material controls, river corridor logic, or generated mesh data. The architecture remains:

```text
GeneratedGround top-level authoring selection
→ GroundSurfaceStyleProfile asset
→ GroundSurfaceVariantRecipe
→ optional local overrides
→ MaterialPropertyBlock
→ ground renderer and child river corridor renderers
```

Rules after Patch O:

- Surface family and variant selection should remain at the top of `GeneratedGround`.
- Do not make normal users manually drag style assets for common families.
- Keep the raw style asset field as an advanced escape hatch.
- Add new surface families as style assets discoverable by the editor dropdown.
- Keep river corridors style-agnostic.
- Keep visual tuning separate from authoring UX patches.

The next recommended step is a dedicated `GroundSurfaceStyleProfile` editor if nested variant/material/feature editing remains awkward after the number of style assets grows. That editor should improve authoring of style assets themselves, not move style ownership back into `GeneratedGround`.


### Patch P — Wet Mudflat Material Sanity Pass

Patch P is a small visual sanity pass for the first non-snowfield style family created by Patch N.

Patch N intentionally proved that `GroundSurfaceStyleProfile` assets can define a second surface family and that `Pooled Wetness` can run as a shader-only feature without textures, atlases, runtime state, or new mesh channels. The first values were deliberately broad proof values, and validation showed that Wet Mudflat was much too glossy: the darker variants read closer to oil, tar, polished plastic, or wet metal than mud.

Patch P keeps the same architecture and changes only wet mud material response and Wet Mudflat recipe values.

Changed response rules:

```text
Pooled Wetness shape contrast:
  reduced from 1.0–4.25 to 0.85–3.10

Pooled Wetness albedo darkening:
  reduced from 0.20 + Strength × 0.28
  to           0.12 + Strength × 0.18

Pooled Wetness damp tint addition:
  reduced from pooled × 0.58
  to           pooled × 0.32

Pooled Wetness albedo blend:
  reduced from pooled × 0.88
  to           pooled × 0.62

Global wetness darkening:
  reduced from Wetness × WetDarkenStrength × 0.36
  to           Wetness × WetDarkenStrength × 0.26

Smoothness contribution:
  reduced from Smoothness + Wetness × WetSmoothnessBoost + PooledWetness × 0.24
  to           Smoothness + Wetness × WetSmoothnessBoost × 0.55 + PooledWetness × 0.10

Specular wetness multiplier:
  reduced from 1.25 at full Wetness
  to           1.08 at full Wetness

Specular pooled-wetness multiplier:
  reduced from 1.38 at full Pooled Wetness
  to           1.12 at full Pooled Wetness
```

Wet Mudflat recipe values were also pulled back:

```text
Damp Mud:
  lower Wetness, WetSmoothnessBoost, Smoothness, Specular Strength, and Pooled Wetness strength.

Waterlogged:
  remains the wettest mudflat variant, but no longer uses extreme global smoothness/specular values.

Trampled:
  remains higher-contrast and compacted, but its wet finish is reduced so it reads more like walked mud than oil.

Frozen Thaw:
  remains colder and partially frosted, with the weakest pooled-wetness finish among the wet variants.
```

Patch P does not change style-family discovery, variant selection, `GeneratedGround` authoring UX, river corridor refresh logic, mesh generation, semantic mask generation, material property names, feature asset contracts, textures, atlases, or runtime state.

Rules after Patch P:

- Wet Mudflat may still need major future shader/features work, but baseline variants should not be mirror-glossy.
- Keep wet ground response mostly matte unless a specific feature intentionally requests stronger shine.
- Do not solve future mud quality by raising global smoothness/specular back to extreme values.
- Prefer local pooled-wetness breakup and semantic masks over full-surface reflectivity.

The next recommended step is either a dedicated `GroundSurfaceStyleProfile` editor, if asset editing remains painful, or a focused new feature/family proof once Wet Mudflat is visually stable enough to stop distracting from architecture validation.



### Patch Q — Wet Mudflat Matte Baseline Reset

Patch Q follows Patch P after validation showed the opposite failure: after reducing the mirror/oil response, the Wet Mudflat variants still read like smooth plastic or playdough because the style was still trying to imply an entire muddy scene through full-surface colour, smoothness, and wetness.

The architectural decision after this validation is important:

```text
Mud ground should not be globally reflective.
The earth body should be mostly matte.
Future reflectivity should come from explicit local features such as puddles, wet stones, water-filled ruts, potholes, and standing-water patches, not from making the whole terrain surface shiny.
```

Patch Q therefore resets Wet Mudflat to a conservative matte-earth baseline. The four variants are allowed to be somewhat samey for now. Their names describe future feature targets, not fully delivered final art.

Changed recipe direction:

```text
Damp Mud:
  ordinary damp brown earth, low wetness, very low specular.

Waterlogged:
  darker and more moisture-biased, but still mostly matte earth until explicit puddle/standing-water features exist.

Trampled:
  slightly darker, higher variation and contrast, but not glossy.

Frozen Thaw:
  colder and paler, with restrained frost and low wet finish.
```

Changed shader response:

```text
Pooled Wetness is now treated as a matte damp-earth breakup cue, not as a water/puddle substitute.
Its smoothness and specular contributions are reduced to minimal values.
```

Rules after Patch Q:

- Do not attempt to make final mud variants using only full-surface colour/smoothness/specular controls.
- Keep baseline earth surfaces matte unless an explicit feature owns the local reflective surface.
- Future waterlogged quality should come from features such as `StandingWaterPuddles`, water-filled ruts, potholes, debris scatter, and terrain/prop context.
- It is acceptable for early Wet Mudflat variants to look similar if they remain plausible ground.


### Patch R — Ground Plan Reconciliation and Style Profile Editor

Patch R reconciles the ground roadmap with the architecture that now exists after Patches J through Q and adds a custom editor for `GroundSurfaceStyleProfile` assets.

The documentation update records the current split between semantic surface profiles, visual style profiles, variant recipes, material controls, feature recipes, and the `GeneratedGround` resolver. It also replaces the old active roadmap with the current next-step roadmap: surface path/compaction authoring, trampled wear, runtime state, footprints, rain/wetness, grass integration, and mixed terrain blending.

The `GroundSurfaceStyleProfile` editor makes style assets practical to edit before more surface families are added. It adds:

- readable variant cards instead of a raw variant array as the primary editing view;
- stable ID and display-name editing per variant;
- compact feature summaries per variant;
- material-control and feature foldouts;
- Add Variant, Duplicate Variant, Remove Variant, and Add Feature actions;
- warnings for missing default surface profiles;
- warnings for empty or duplicate variant IDs;
- warnings for enabled `None` features;
- informational warnings for reserved feature kinds or cost classes that do not currently render.

Patch R does not change visuals, shader behavior, generated mesh data, river corridor logic, material values, style-family discovery, textures, atlases, or runtime state.

Rules after Patch R:

- Keep `GeneratedGround` as the top-level level-authoring surface.
- Keep `GroundSurfaceStyleProfile` as the style-family asset, edited through its custom editor.
- Do not add new terrain families as `GeneratedGround` enum branches.
- Do not infer final muddy/snowy/rocky detail from global material controls alone; add explicit feature modules when needed.
- For path/compaction work, prefer visual-only masks where equally effective, but allow small safe height changes where the terrain feature justifies them.


### Patch S — Ground Style Asset Live Refresh

Patch S fixes an authoring gap introduced by the asset-backed style workflow. After Patch R, `GroundSurfaceStyleProfile` assets were much easier to edit, but editing a style asset did not immediately update open `GeneratedGround` instances that referenced that asset.

The intended authoring behavior is now:

```text
Edit GSSP_Snowfield or GSSP_WetMudflat
→ open GeneratedGround objects using that style refresh their resolved style state
→ material and shader-only feature edits reapply MaterialPropertyBlock values
→ child river corridors receive the same refreshed ground material contract
```

Patch S adds automatic delayed refresh from `GroundSurfaceStyleProfileEditor` whenever serialized style data changes, plus an explicit `Apply To Open Generated Grounds` button for manual refresh.

The refresh path intentionally calls `GeneratedGround.RefreshSurfaceStyleState()` rather than rebuilding unconditionally. Material-control and shader-only feature edits should remain material-property-block updates. If the resolved semantic `GroundSurfaceProfile` changes and the generated ground is configured to regenerate on validation, the existing generation-signature path performs the necessary regeneration.

Patch S does not change visuals, style assets, shader code, mesh data, river corridor code, textures, atlases, runtime state, or modifier behavior.

### Patch T — Ground Modifier Surface/Height Contract

Patch T separates two concepts that were previously coupled inside `GroundModifier`:

```text
Does this modifier change playable terrain height?
Does this modifier write authored ground-surface meaning?
```

Before Patch T, `Flatten` was the only modifier mode that wrote the `UV2.x` compaction/path mask, and there was no way to author a path, damp/deposit boost, or standing-water/puddle potential without using an ordinary height modifier.

Patch T adds:

```text
GroundModifierMode.None
GroundModifierSurfaceEffectMode.AutoFromHeight
GroundModifierSurfaceEffectMode.None
GroundModifierSurfaceEffectMode.Custom
Surface Compaction Strength
Surface Damp/Deposit Strength
Surface Standing Water Strength
```

The generated ground mask contract is now:

```text
Vertex Color R = tonal surface variation
Vertex Color G = exposure / accumulation eligibility
Vertex Color B = damp/deposit potential, including authored modifier boost
Vertex Color A = vegetation suitability
UV2.x = compaction/path/flatten influence
UV2.y = shore influence
UV2.z = rocky/dry patch
UV2.w = authored standing-water / puddle potential
```

Legacy behavior is preserved: existing `Flatten` modifiers using `AutoFromHeight` continue to write compaction/path influence. `Raise` and `Lower` keep their height behavior.

Authoring rules after Patch T:

- Use `Mode = None` with `Surface Effect Mode = Custom` for pure visual/path/damp/standing-water masks.
- Use `Flatten`, `Lower`, or `Raise` with `Surface Effect Mode = Custom` when a road, wagon rut, camp pad, drainage dip, or puddle basin needs both a small height change and explicit surface metadata.
- Use `Surface Effect Mode = None` for physical height edits that should not imply path, damp, or standing-water surface meaning.
- Keep denivelations small and combat-safe unless a later gameplay/navigation pass explicitly approves stronger terrain deformation.

Patch T does not add final trampled rendering, puddle rendering, splines, footprints, runtime wetness, atlases, textures, or new mesh channels. It only establishes the static authored modifier contract that future features can read.

