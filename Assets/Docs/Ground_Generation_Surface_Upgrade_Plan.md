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

Primary implementation files:

- `Assets/Game/Procedural/Ground/GeneratedGround.cs`
- `Assets/Game/Procedural/Ground/GroundGenerator.cs`
- `Assets/Game/Procedural/Ground/GroundModifier.cs`
- `Assets/Game/Procedural/Ground/GroundHeightFieldSnapshot.cs`
- `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs`
- `Assets/Game/Procedural/Core/MeshData.cs`
- `Assets/Game/Procedural/Core/MeshBuilder.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverGroundSnapshot.cs`
- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelCellVariation.hlsl`
- `Assets/Game/Demo/Materials/Ground/M_PixelFrozenDirt.mat`

Related art and system documents:

- `Assets/Docs/Rock_Generated_Mass_Upgrade_Plan.md`
- `Assets/Docs/Proof of Concept/01_Visual_Language_and_Rendering.md`
- `Assets/Docs/Proof of Concept/05_Project_Application_Norse_Game.md`
- `Assets/Docs/Proof of Concept/06_Proof_of_Concept.md`

The current ground implementation already has useful foundations:

- `GroundRecipe` controls patch size, resolution, patch coordinate, transition slope, broad shape, roughness, surface detail, edge blending, and material variation.
- `GroundModifier` supports deterministic flatten, raise, and lower regions for authored traversal and scene composition.
- `StylizedRiverGroundSnapshot` lets rivers conceal broad ground below the dedicated river corridor.
- `GroundHeightFieldSnapshot` lets other systems sample pre-river height, normals, render normals, surface variation, and reserved material classification.
- `MeshData` supports vertex colors and optional UV2 data.
- `SH_PixelSurfaceLit.shader` already has generic pixel surface features such as broad variation, warped cell lookup, profile contrast, wetness, frost, semantic brightening/darkening, and material profile controls.

Current limitations:

- Ground shape and ground surface are coupled inside `GroundRecipe`.
- `GroundProfile` only describes the heightfield family, not the material family.
- `BuildSurfaceMetadata` writes one broad variation value and leaves material classification at `0`.
- `BuildMeshData` writes vertex color as `R = surface variation`, `G = 0.5`, `B = 0.5`, `A = 1`.
- The ground has no object-owned material property block equivalent to `GeneratedMass`.
- The ground has no authored surface profile asset.
- The ground has no static mask contract for snow potential, wetness potential, dirt/deposit, vegetation suitability, or terrain type blending.
- The ground has no runtime surface state texture for rain, footprints, snow compression, grass trampling, or mud/water accumulation.
- The current material can read as pale, low-contrast procedural fuzz because it receives little semantic information from the mesh.

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

Status: mostly implemented on 2026-07-05; Unity compile/scene validation still required.

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
- [x] Update comments documenting the vertex color/UV2 contract.

Acceptance:

- mask debug values can be inspected;
- existing material still renders;
- generated masks are stable for a seed and patch coordinate;
- visual improvement is possible without raising mesh height.

### Patch 4 - Modifier and River Surface Influence

Status: partially implemented on 2026-07-05.

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

Acceptance:

- paths/combat pads can become visually compacted without needing more height changes;
- river banks can become damp/slushy/mossy by profile;
- river geometry ownership remains unchanged.

### Patch 5 - Ground Material Property Block

Status: partially implemented on 2026-07-05.

Goal:

- let each generated ground patch supply profile-driven material settings without duplicating material assets.

Checklist:

- [x] Add `MaterialPropertyBlock` support to `GeneratedGround`.
- [ ] Bind base/profile colors.
- [~] Bind patch scale/contrast values. Contrast is bound; patch scale remains generated-data-only for now.
- [~] Bind wetness/snow/grass defaults. Static snow/damp/vegetation/rocky response is bound; runtime wetness is still deferred.
- [ ] Bind seed or patch coordinate values.
- [x] Refresh property block on enable, validate, regenerate, and profile change.
- [x] Keep shared material assignment intact.
- [ ] Add debug mode binding if available. Debug remains a material/shader setting for now.

Acceptance:

- two ground patches can use the same shared material but different profiles/colors;
- changing the profile updates only that ground patch;
- regeneration does not reset the selected profile.

### Patch 6 - First Ground Shader Response

Status: partially implemented on 2026-07-05.

Goal:

- make the generated masks visibly useful.

Checklist:

- [x] Decide whether to add `SH_GroundSurfaceLit.shader` or a ground mode in `SH_PixelSurfaceLit.shader`. Current implementation uses a ground mode in the shared pixel-surface shader.
- [x] Reuse `PixelCellVariation.hlsl` or split shared pixel helpers cleanly.
- [x] Read vertex color R/G/B/A.
- [x] Read UV2 X/Y if written. UV2 Z is also read for rocky/dry response.
- [x] Add broad profile patch color blending.
- [x] Add exposure/snow-hold tint.
- [x] Add damp/deposit darkening.
- [x] Add shore influence response.
- [x] Keep small pixel cell variation restrained.
- [x] Add mask debug modes.
- [ ] Create or update a ground material for the new shader path. The current patch uses per-renderer property blocks instead of duplicating material assets.

Acceptance:

- snowy field no longer reads as uniform pixel fuzz;
- broad land patches are visible from the isometric camera;
- texture richness does not depend on high physical terrain relief;
- debug modes clearly show each mask.

### Patch 7 - Terrain Profile Asset Set

Status: not started.

Goal:

- prove that multiple terrain types can be selected from the Inspector.

Checklist:

- [ ] Create `GSP_Snowfield`.
- [ ] Create `GSP_WetSoil`.
- [ ] Create `GSP_FertileSoil`.
- [ ] Create `GSP_DrySoil`.
- [ ] Create `GSP_MossyGround`.
- [ ] Create `GSP_RockyScrub`.
- [ ] Optionally create `GSP_FrozenMud`.
- [ ] Optionally create `GSP_DesertSand`.
- [ ] Tune each with the same shader and generated mask contract.
- [ ] Add a demo comparison area or duplicate ground patch for visual checks.

Acceptance:

- selecting a different profile changes terrain identity without changing mesh geometry;
- at least four terrain profiles read differently from the game camera;
- shared shader/material architecture remains intact.

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
- [~] Patch 3 - Static surface mask contract. Core code implemented; Unity validation still required.
- [~] Patch 4 - Modifier and river surface influence. Flatten and shore mask influence implemented; surface-only modifier mode still pending.
- [~] Patch 5 - Ground material property block. First profile-to-material binding implemented; color/seed/debug binding still deferred.
- [~] Patch 6 - First ground shader response. Shared shader ground mode, final response, and debug modes implemented; material asset tuning still pending.
- [ ] Patch 7 - Terrain profile asset set.
- [ ] Patch 8 - Runtime state design stub.
- [ ] Patch 9 - Footprint prototype.
- [ ] Patch 10 - Rain/wetness prototype.
- [ ] Patch 11 - Grass integration contract.
- [ ] Patch 12 - Mixed terrain and authored masks.

