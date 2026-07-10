# Ground Generation Surface Upgrade Plan

## Purpose

Define the implementation plan, patch history, and active technical roadmap for generated-ground surface work. The ground visual/design baseline is owned by `Ground_Visual_Design_and_Architecture.md`; this document implements that baseline and records how the code/assets are brought into alignment.

The current visual north star, defined in `Ground_Visual_Design_and_Architecture.md`, is:

```text
Restrained stylized terrain:
BOTW/TOTK-like base-material restraint
+ Hades-1-like painted ground accents
+ mostly 3D procedural geometry
+ reusable procedural masks and style layers instead of fully hand-painted floor art.
```

This does not mean copying any reference literally. It means borrowing the useful production grammar:

- from BOTW/TOTK: calm matte base ground, restrained noise, broad readable material regions, and scene complexity carried by geometry, lighting, props, vegetation, rocks, rivers, and atmosphere;
- from Hades 1: sparse authored-looking surface accents, short dark mound/crease lines, contact emphasis, decorative rhythm, and deliberate value grouping;
- from the existing PS3D framework: procedural masks, component-owned style authoring, shared material/property-block contracts, debugable semantic channels, and deterministic generated geometry.

The ground should remain mostly flat and combat-friendly. It should not become interesting through constant height noise, texture soup, or feature-by-feature simulation before the art language is proven. Instead, it separates and layers:

- playable shape;
- calm family/variant base material;
- broad macro patch composition;
- static semantic masks;
- reusable painted accent layers;
- contact and edge accent layers;
- sparse motif/stamp layers;
- runtime surface state later;
- future grass, snow, rain, mud, footprints, puddles, and material blending.

The desired result is a broad, readable stage floor whose surface feels designed: simple at rest, but enriched by meaningful patches, subtle hand-painted-looking accents, shore/contact response, compacted paths, damp low areas, snow or mud identity, and later runtime footprints or weather response.

## Current State

### Current Implementation Status After Patch V3E

The ground upgrade has moved beyond the original single snow-material improvement pass. The current system now has a real surface-style framework, and the design direction has pivoted from feature accumulation to a shared visual doctrine.

| Area | Status | Notes |
| --- | --- | --- |
| Ground visual doctrine | Canonical in `Ground_Visual_Design_and_Architecture.md` | The ground target is restrained stylized terrain: calm BOTW/TOTK-like base surfaces plus Hades-1-like painted accents, implemented through reusable procedural style layers. |
| Dedicated ground shader | Implemented | `SH_PixelGroundSurfaceLit.shader` owns ground rendering separately from generated masses. |
| Static semantic masks | Implemented baseline | Vertex color and UV2 carry tonal, exposure, damp/deposit, vegetation, compaction, shore, rocky/dry, and authored standing-water/puddle-potential data. |
| Ground/corridor material contract | Implemented | `GeneratedGround` resolves visual state and applies it by `MaterialPropertyBlock`; river corridors remain dependent renderers and must remain style-agnostic. |
| Component-owned surface authoring | Implemented | `GeneratedGround` exposes top-level Surface Family and Surface Variant controls. |
| Asset-backed visual families | Implemented baseline | `GroundSurfaceStyleProfile` assets own visual families such as Snowfield, Wet Mudflat, and Grassland. Families define surface identity; they do not define the global art language alone. |
| Asset-backed variants | Implemented baseline | `GroundSurfaceVariantRecipe` stores stable ids, display names, material controls, and feature recipes. Variants tune the shared style stack. |
| Feature-module recipe layer | Implemented stack baseline in Patch V3 | `GroundSurfaceFeatureRecipe` supports explicit cost classes. `GeneratedGround` now resolves the first enabled ShaderOnly recipe of each supported kind and writes explicit shader-property blocks, so variants can combine supported features. |
| Snowfield family | Implemented baseline | `GSSP_Snowfield` and `GSP_Snowfield` exist. Variants are calm baseline snow floors under the new doctrine. |
| Wet Mudflat family | Implemented baseline | `GSSP_WetMudflat` and `GSP_WetMudflat` exist. Patch Q reset the family to matte earth until explicit puddle/rut/debris features exist. |
| Grassland family | Implemented baseline in Patch V2B | `GSSP_Grassland` and `GSP_Grassland` add the missing living-ground baseline for shared feature validation. No vegetation rendering is included. |
| Style profile editor | Implemented in Patch R | Style assets have a readable custom editor with variant cards, feature summaries, duplicate support, and validation warnings. |
| Style asset live refresh | Implemented in Patch S | Editing a style asset can refresh open `GeneratedGround` users without manual scene rebuilds for material/property-block changes. |
| Ground modifier surface/height contract | Implemented in Patch T | `GroundModifier` can affect height, authored surface masks, or both; legacy Flatten compaction behavior is preserved. |
| TrampledWear proof feature | Implemented/prototyped in Patch U | `TrampledWear` reads `UV2.x` compaction/path. It is now considered an experiment/proof of the mask-to-feature route, not the active art-direction priority. |
| Runtime surface state | Deferred | Wetness, snow depth, compression, footprints, and trample maps remain future work. Runtime work must wait until the static visual language is validated. |
| Painted accent lines | Implemented foundation in Patch V3; relief debug/strengthening in Patch V3F | `PaintedAccentLines` is the first stackable doctrine layer. Patch V3D decouples line spacing from stroke size and caps strokes in world units. Patch V3E replaces straight/bar-like micro-strokes with curved terrain-fold strokes. Patch V3F exposes line, relief-body, and signed-relief debug channels, thins the contour, and strengthens side-dependent painted relief shading. |
| GeneratedGround debug views | Implemented in Patch V3B; dropdown cleanup in Patch V3C | Generated-ground debug selection is now exposed on the `GeneratedGround` component and written through renderer-local material property blocks. Material asset debug controls are fallback/internal only. |

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

- `Assets/Docs/Ground_Visual_Design_and_Architecture.md`
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
- [x] The ground originally had no authored surface profile asset; `GSP_Snowfield`, `GSP_WetMudflat`, and `GSP_Grassland` now exist.
- [x] The ground originally had no static mask contract for snow potential, wetness potential, dirt/deposit, vegetation suitability, or terrain type blending; the baseline semantic contract now exists.
- [ ] The ground still has no runtime surface state texture for rain, footprints, snow compression, grass trampling, or mud/water accumulation; this is now deliberately deferred until the static visual language is proven.
- [~] Early material output read as pale, low-contrast procedural fuzz. Baseline Snowfield, Wet Mudflat, and Grassland now exist, but final detail should come from the shared visual stack: calm base, macro patches, painted accent lines, contact accents, and sparse motifs before niche runtime features.

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
- make the first visible improvement possible without authored texture assets;
- preserve the new ground doctrine: calm base surfaces plus selective painted accents;
- use family/variant assets to tune the shared style stack rather than creating unrelated one-off feature silos;
- avoid high-frequency procedural noise as the primary source of visual interest;
- keep runtime surface state deferred until the static style pillars are proven.

The upgrade should not:

- turn the prototype ground into high-relief terrain;
- solve production terrain streaming;
- introduce destructible terrain;
- require a full vegetation system before surface profiles are useful;
- require final weather simulation before rain/snow channels are reserved;
- bake footprints into the generated mesh;
- treat every terrain type as a separate duplicated material;
- turn the generic pixel surface shader into an unreadable all-purpose monolith without contracts;
- chase Hades 2-level hand-painted floor production;
- rely on Tunic-like block/voxel simplicity as the main style target;
- make every ground family visually unrelated;
- build footprints, puddles, rain, grass trampling, or runtime wetness before the static ground language works.

## Ground Visual Doctrine - Restrained Stylized Terrain

The canonical generated-ground design baseline now lives in:

```text
Assets/Docs/Ground_Visual_Design_and_Architecture.md
```

That document is authoritative for:

- the BOTW/TOTK-like base + Hades-1-like accent direction;
- ground style pillars;
- reference interpretation;
- non-goals;
- the shared ground composition stack;
- family/variant interpretation;
- reusable style-layer architecture;
- static surface-mask contracts;
- the paused runtime-state policy;
- acceptance criteria and drift-prevention rules.

This implementation plan should not duplicate the full doctrine. It should reference the ground design document and focus on concrete implementation state, patch sequencing, known limitations, and validation.

Implementation shorthand retained here:

```text
Restrained stylized terrain
= calm base surfaces
+ broad macro patch composition
+ semantic mask response
+ Hades-1-like painted accent lines
+ contact / edge accents
+ sparse motifs
+ runtime state later.
```

Patch work that changes the visual doctrine, static mask contract, family/variant meaning, feature-layer taxonomy, or runtime-state priority must update both documents together.

## Active Roadmap After Style Doctrine Pivot

The old Patch V-Z runtime roadmap is paused. It was coherent technically, but it is now the wrong priority because the ground art direction must be proven before more niche simulation/features are added.

Patch T and Patch U remain useful:

- Patch T established the authored surface-mask contract.
- Patch U proved that a feature can consume `UV2.x` compaction/path in the shader.

However, `TrampledWear` is now classified as a proof/experiment, not the next visual cornerstone. The active roadmap is now style calibration and shared doctrine layers.

| Priority | Patch | Concrete goal |
| --- | --- | --- |
| 1 | Patch V0 — Ground Visual Doctrine Documentation | Completed. `Ground_Visual_Design_and_Architecture.md` now owns the sacred ground design baseline; this implementation plan records technical alignment. |
| 2 | Patch V1 — Style Calibration Setup | Completed as a temporary `Style Calibration` surface family with four comparison variants: Calm Base, Hades Accent Proxy, Hybrid Target Proxy, and Pixel-Faceted. |
| 3 | Patch V2 — Base Ground Simplification | Implemented as an asset/docs retune. Snowfield and Wet Mudflat now use calmer matte bases with lower pixel variation, lower patch contrast, and reduced broad noise so future accents can sit on top. |
| 4 | Patch V2B — Grassland Baseline Family | Implemented as a production `Grassland` family with Clean, Patchy, Damp, and Worn Meadow variants. Establishes the canonical three-family test set. |
| 5 | Patch V3 — Shader Feature Stack + Painted Accent Lines | Implemented. Variants now use a real shader feature stack, and Painted Accent Lines are the first stackable doctrine layer. |
| 6 | Patch V4 — Contact / Edge Accent Layer | Add localized accent response near shores, rocks, modifier boundaries, paths, banks, and object contact zones. Use existing masks first; add new generated/contact masks only when justified. |
| 7 | Patch V5 — Sparse Motif Layer | Add reusable sparse marks such as chips, cracks, scuffs, stains, snow scratches, stones, or debris hints. Avoid stamp spam. |
| 8 | Patch V6 — Feature Stack Authoring Polish | Add richer warnings, cost summaries, duplicate/combination guidance, and editor UX after more stack layers exist. |
| 9 | Later | Ground Surface Runtime State Stub | Revisit runtime wetness, snow depth, compression, footprints, and disturbance after the static visual stack is accepted. |
| 10 | Later | Footprints / Rain / Puddles / Grass Integration | Build on the runtime state contract only after the visual doctrine is stable. |
| 11 | Future | Mixed Terrain / Profile Blending | Add explicit support for blended surface families such as snow over mud, rocky scrub over soil, or worn path through snow. |

### Paused runtime roadmap

The following patches are no longer the immediate queue:

```text
Old Patch V — Ground Surface Runtime State Stub
Old Patch W — Footprint / Compression Prototype
Old Patch X — Rain / Wetness Prototype
Old Patch Y — Style/Feature Authoring Polish
Old Patch Z — Grass Integration Contract
```

They are not rejected. They are deferred because building them before the static style works invites drift and overengineering.

### Surface modifier note

- Surface-only masks are preferred when the same visual effect can be achieved without changing playable height.
- Small denivelations are acceptable for roads, wagon tracks, camp pads, puddle basins, and other authored terrain features when they remain combat-safe and camera-stable.
- Snow paths and grass paths should eventually come from snow/grass accumulation and runtime interaction systems, not be hard-baked into the base ground as final content.
- Patch T inspected the current `GroundModifier` and ground mask code before implementing the path.

## Superseded Implementation Plan Notes

The original Patch 1-12 implementation plan has been superseded by the completed Patch J-V0 work and the active doctrine roadmap above. It is no longer the active queue and should not be used to decide next work.

Historical mapping:

| Old concern | Current status |
| --- | --- |
| Separate physical shape from surface identity | Implemented through `GroundSurfaceProfile`, `GroundSurfaceStyleProfile`, variants, and `GroundModifier` surface/height split. |
| Static surface mask contract | Implemented baseline through vertex color and UV2 channels. |
| Ground material property block | Implemented through `GeneratedGround` material/property-block resolver. |
| Dedicated ground shader | Implemented as `SH_PixelGroundSurfaceLit.shader`. |
| Terrain profile asset set | Implemented baseline with Snowfield, Wet Mudflat, and Grassland. |
| Runtime state design | Deferred after doctrine pivot. Contract remains documented, but implementation is no longer the immediate milestone. |
| Footprints / rain / grass | Deferred until the static visual doctrine stack works. |
| Mixed terrain/profile blending | Future work. |

Active implementation work must follow `Active Roadmap After Style Doctrine Pivot`, not the old Patch 1-12 list.

## Patch V1 - Style Calibration Setup

Patch V1 creates a temporary development surface family for screenshot-based style comparison.

Changed assets:

```text
Assets/Game/Demo/Profiles/Ground/GSP_StyleCalibration.asset
Assets/Game/Demo/Profiles/Ground/Styles/GSSP_StyleCalibration.asset
```

`GSP_StyleCalibration` is a neutral semantic/mask-generation profile. It provides a common mask baseline for all calibration variants so the comparison is mostly about visible material/style tuning.

`GSSP_StyleCalibration` is a `GroundSurfaceStyleProfile` discovered by the existing `GeneratedGround` style-family dropdown because the editor searches:

```text
Assets/Game/Demo/Profiles/Ground/Styles
```

The family contains four variants:

| Variant id | Display name | Intent |
| --- | --- | --- |
| `calibration.calm_base` | Calm Base | Restrained BOTW/TOTK-like base-material lane. Low noise, matte finish, broad soft patches, no feature recipe. |
| `calibration.hades_accent_proxy` | Hades Accent Proxy | Stronger Hades-1-like surface rhythm using the existing `DirectionalStreaks` shader-only feature as a temporary proxy. |
| `calibration.hybrid_target_proxy` | Hybrid Target Proxy | Likely doctrine target: calm base plus restrained accent rhythm. Uses a weaker `DirectionalStreaks` proxy. |
| `calibration.pixel_faceted` | Pixel-Faceted | Pushes existing PS3D pixel/faceted material identity harder for comparison. No new feature recipe. |

Implementation boundaries:

- No new code.
- No shader changes.
- No scene changes.
- No runtime state.
- No new materials.
- No river code changes.
- No final painted accent line implementation yet.

Patch V1 is intentionally a calibration patch. It gives the project a controlled way to choose the next visual lane before Patch V2 base simplification and Patch V3 painted accent lines.


## Patch V2 - Base Ground Simplification and Calibration Cleanup

Patch V2 applies the first screenshot-driven doctrine correction after the V1 calibration pass.

Calibration findings recorded by this patch:

- `Calm Base` is the strongest foundation: readable, restrained, and appropriate as the stage floor.
- `Hades Accent Proxy` and `Hybrid Target Proxy` support the direction philosophically, but the existing `DirectionalStreaks` proxy does not create convincing Hades-1-like painted crease lines. Real accent lines remain Patch V3.
- `Pixel-Faceted` is useful as an anti-reference for the default ground style. Global pixel/faceted noise becomes too busy and should not be the primary ground read.

Patch V2 therefore retunes the real production families toward the accepted base doctrine:

```text
calm matte base
+ restrained broad patches
+ lower pixel/faceted noise
+ subtle feature response
+ no final painted accents yet
```

Changed assets:

```text
Assets/Game/Demo/Profiles/Ground/Styles/GSSP_StyleCalibration.asset
Assets/Game/Demo/Profiles/Ground/Styles/GSSP_Snowfield.asset
Assets/Game/Demo/Profiles/Ground/Styles/GSSP_WetMudflat.asset
```

Concrete asset changes:

- Renamed the calibration display label from `Pixel / Faceted` to `Pixel-Faceted` so Unity does not treat the slash as a submenu path.
- Kept the stable variant id `calibration.pixel_faceted` unchanged.
- Reduced Snowfield pixel variation, pixel effect strength, cell warp, patch blend, and overly strong directional streaks.
- Reduced Wet Mudflat pixel variation, pixel effect strength, cell warp, damp darkening, patch blend, and pooled/trampled feature intensity.
- Kept Wet Mudflat matte; no glossy puddle or water material behavior was added.
- Kept all feature work static and shader/material-control driven; no runtime state was introduced.

Patch V2 does not implement:

- real `PaintedAccentLines`;
- contact/edge accents;
- sparse motifs;
- feature-stack aggregation;
- runtime wetness, snow compression, footprints, rain, puddles, grass suppression, roads, or wagon tracks;
- new shader properties, components, scene changes, or river logic.

The success condition is not that the ground already looks like Hades. The success condition is that Snowfield and Wet Mudflat become calm, readable stage floors that can accept future painted accents without fighting base noise.

## Patch V2B - Grassland Baseline Family

Patch V2B adds a real production `Grassland` family before Patch V3 painted accent lines.

Reason:

```text
Snowfield   = pale, cold, soft, low-value ground
Wet Mudflat = dark, earthy, damp, matte ground
Grassland   = green/olive, living, medium-value ground
```

Future shared doctrine layers need this three-family test set. Testing only snow and mud biases the system toward extreme pale/dark materials and leaves no medium-value living-ground baseline.

Changed assets:

```text
Assets/Game/Demo/Profiles/Ground/GSP_Grassland.asset
Assets/Game/Demo/Profiles/Ground/Styles/GSSP_Grassland.asset
```

`GSP_Grassland` semantic intent:

- high vegetation suitability;
- moderate damp/deposit potential;
- low snow eligibility;
- moderate footprint visibility;
- soft broad tonal patches;
- moderate rain absorption;
- high grass recovery speed for later vegetation/runtime systems.

`GSSP_Grassland` variants:

| Variant id | Display name | Intent |
| --- | --- | --- |
| `grassland.clean_meadow` | Clean Meadow | Calm muted olive meadow baseline. Low noise and broad soft variation. |
| `grassland.patchy_meadow` | Patchy Meadow | Slightly more exposed earth/olive patching. Still calm and non-speckled. |
| `grassland.damp_meadow` | Damp Meadow | Cooler/darker green for river-adjacent or damp living ground. Uses a tiny `PooledWetness` proof response, not real puddles. |
| `grassland.worn_meadow` | Worn Meadow | Browner, compressed/path-capable meadow. Uses restrained `TrampledWear` so authored compaction can be tested on grassland. |

`Style Calibration` remains a temporary development family. It is not Grassland and should not be treated as production content.

Patch V2B does not implement:

- grass blades or vegetation placement;
- foliage density maps, wind animation, physics, or trampling;
- painted accent lines;
- contact/edge accents;
- sparse motifs;
- feature-stack aggregation;
- runtime wetness, snow compression, footprints, rain, puddles, roads, or wagon tracks;
- new shader properties, components, scene changes, or river logic.

The success condition is that Grassland becomes a calm third baseline for upcoming shared feature work, not that it already looks like finished grass or foliage.

## Patch V3 - Shader Feature Stack and Painted Accent Lines

Patch V3 corrects the feature architecture and adds the first real doctrine-layer visual feature.

### Feature stack contract

The serialized asset model was already list-based:

```text
GroundSurfaceVariantRecipe.features
```

Patch V3 makes the renderer honor that list as a stack:

```text
variant feature list
  -> first enabled ShaderOnly recipe per supported kind
  -> explicit MaterialPropertyBlock block per feature kind
  -> shader applies all supported layers in stable renderer-defined order
```

Supported ShaderOnly feature kinds after Patch V3:

```text
DirectionalStreaks
PooledWetness
TrampledWear
PaintedAccentLines
```

Rules:

- features are not mutually exclusive by default;
- first enabled recipe of each kind wins;
- duplicate enabled same-kind recipes are authoring mistakes;
- unsupported feature kinds may remain serialized but do not render;
- non-ShaderOnly cost classes remain reserved until their renderer path exists;
- shader composition order is not controlled by the asset list order;
- `_GroundFeatureMode` is a deprecated proof-feature compatibility property and must not be extended with new modes.

### Painted Accent Lines

`PaintedAccentLines` is a shader-only, visual-only layer for Hades-1-like ground accents.

It creates short, broken, slightly curved, dark/value-shifted stroke masks from world-space procedural cells and semantic mask gating. Patch V3D refines the raw mask after validation showed the first implementation produced large isolated strips. Patch V3E then changes the primitive from straight/micro-bar strokes into short curved terrain-fold strokes with a soft signed relief body. Scale controls accent spacing/grouping rather than raw stroke length; stroke length and thickness are capped in world units. It is intended to suggest:

```text
grass folds
mud creases
snow wrinkles
small mound lines
soft contour breaks
surface age
```

It explicitly does not add:

```text
textures
decals
height deformation
mesh changes
runtime state
footprints
puddles
grass blades
contact accents
sparse motif stamps
```

### Style asset usage

Patch V3 adds `PaintedAccentLines` recipes to the canonical families:

```text
Snowfield
Wet Mudflat
Grassland
```

It also updates Style Calibration's Hades/Hybrid lanes to use real Painted Accent Lines instead of relying only on DirectionalStreaks as a proxy.

This enables combinations such as:

```text
grassland.damp_meadow
  PooledWetness
  PaintedAccentLines

grassland.worn_meadow
  TrampledWear
  PaintedAccentLines

mudflat.trampled
  TrampledWear
  PaintedAccentLines

snowfield.wind_scoured
  DirectionalStreaks
  PaintedAccentLines
```

## Validation Plan

Validation must happen from the actual isometric/gameplay camera first. Close editor inspection is secondary. The ground is successful only if it reads as a coherent stage for characters, rivers, rocks, props, combat telegraphs, and atmosphere.

### Style Calibration Validation

Checklist:

- [ ] Select `GeneratedGround` -> `Surface Family = Style Calibration`.
- [ ] Test `Calm Base`, `Hades Accent Proxy`, `Hybrid Target Proxy`, and `Pixel-Faceted` from the same camera.
- [ ] Confirm the calm base surface is readable without looking empty.
- [ ] Confirm broad macro patches are visible but not noisy.
- [ ] Confirm the Hades and Hybrid proxy accents suggest useful authored ground rhythm without becoming procedural hatching.
- [ ] Do not expect final painted accent lines, contact accents, or sparse motifs in V1; those remain queued for V3-V5.
- [ ] Confirm pixel/faceted variation is clearly visible only in the Pixel-Faceted lane.
- [ ] Confirm ground detail does not compete with characters, VFX, hazards, dialogue presentation, or river foam.


### Base Simplification Validation

Checklist:

- [ ] Confirm the calibration variant appears as `Pixel-Faceted`, not as a nested dropdown.
- [ ] Test `Snowfield` variants from the gameplay camera and confirm they are calmer and less noisy than before.
- [ ] Test `Wet Mudflat` variants from the gameplay camera and confirm they remain matte, broad, and low-noise.
- [ ] Confirm `Wet Mudflat -> Trampled` still responds to compaction/path masks, but does not make trampled wear the main visual foundation.
- [ ] Confirm `Wind-Scoured` remains directional but no longer dominates as a fake final accent-line solution.
- [ ] Confirm the base ground may look plain; that is acceptable until Patch V3 adds real painted accent lines.

### Grassland Baseline Validation

- [ ] Confirm `GeneratedGround` exposes `Surface Family = Grassland`.
- [ ] Test `Clean Meadow`, `Patchy Meadow`, `Damp Meadow`, and `Worn Meadow` from the gameplay camera.
- [ ] Confirm Grassland reads as calm muted living ground, not a grass-blade/foliage system.
- [ ] Confirm `Damp Meadow` remains matte and does not look like puddles or glossy wet grass.
- [ ] Confirm `Worn Meadow` can be used with compaction/path masks without becoming the main style foundation.
- [ ] Confirm the river corridor still follows the selected Grassland style.
- [ ] Confirm river corridor material sync still follows the selected ground style.

### Painted Accent Lines / Feature Stack Validation

- [ ] Confirm Unity compiles after Patch V3.
- [ ] Confirm style assets can contain multiple enabled ShaderOnly features without being treated as invalid.
- [ ] Confirm `Snowfield -> Wind-Scoured` still shows DirectionalStreaks and also receives Painted Accent Lines.
- [ ] Confirm `Wet Mudflat -> Trampled` still responds to compaction/path masks and also receives Painted Accent Lines.
- [ ] Confirm `Grassland -> Damp Meadow` can show PooledWetness and Painted Accent Lines together.
- [ ] Confirm `Grassland -> Worn Meadow` can show TrampledWear and Painted Accent Lines together.
- [ ] Select `GeneratedGround -> Ground Debug -> Debug View -> Ground Painted Accent Lines` and confirm the raw line mask is visible.
- [ ] Confirm the raw mask uses small clustered strokes, not large isolated bars/crescents.
- [ ] Confirm line scale changes spacing/grouping without creating huge strokes.
- [ ] Confirm Snowfield is not scratched everywhere.
- [ ] Confirm Wet Mudflat does not turn into crack/noise texture.
- [ ] Confirm Grassland does not turn into grass-blade hair.

### Object-Level Ground Debug Validation

- [ ] Confirm `GeneratedGround` Inspector shows `Ground Debug -> Debug View`.
- [ ] Switch to `Ground Compaction Path` from the `GeneratedGround` Inspector and confirm the mask appears without editing the material asset.
- [ ] Switch to `Ground Painted Accent Lines` from the `GeneratedGround` Inspector and confirm the raw accent-line mask appears.
- [ ] Press `Clear Debug View` and confirm normal rendering returns.
- [ ] Confirm changing debug views refreshes material properties only and does not regenerate the mesh.


### Gameplay Validation

Checklist:

- [ ] Walk across the patch without distracting vertical bob.
- [ ] Fight or simulate combat movement on the patch.
- [ ] Verify hit/telegraph readability over the ground.
- [ ] Verify bridge and river crossing remain clear.
- [ ] Verify camera does not need to chase tiny height changes.
- [ ] Verify flatten/lower/raise modifiers still preserve playable spaces.
- [ ] Verify surface-only modifiers can change masks without changing height.

### Technical Validation

Checklist:

- [ ] Regenerate ground in edit mode.
- [ ] Change surface family and variant and verify material updates.
- [ ] Edit a style profile asset and verify open generated grounds refresh as expected.
- [ ] Change shape seed and verify selected style state persists.
- [ ] Verify `MeshData.Validate` passes.
- [ ] Verify UV2 count matches vertex count when used.
- [ ] Verify material property blocks do not instantiate materials.
- [ ] Verify no river corridor material-sync regressions.
- [ ] Verify shader compiles in URP.

### Debug Validation

Checklist:

- [ ] Inspect tonal patch mask.
- [ ] Inspect exposure/snow-hold mask.
- [ ] Inspect damp/deposit mask.
- [ ] Inspect vegetation suitability mask.
- [ ] Inspect shore influence mask.
- [ ] Inspect compaction/path influence mask.
- [ ] Inspect standing-water/puddle-potential mask.
- [ ] Inspect painted accent line mask from `GeneratedGround -> Ground Debug`.
- [ ] Confirm debug view changes do not regenerate terrain or require opening material assets.
- [ ] Inspect runtime wetness/snow/compression only after runtime state exists.

## Suggested Initial Tuning

The first style-calibration goal is not final snow, mud, grass, or path quality. The goal is to find the correct balance between calm base ground and selective authored-looking accents.

For the current prototype clearing:

- lower physical height detail before increasing shader detail;
- make base material response matte and restrained;
- keep smoothness/specular conservative, especially for mud;
- make material patch scale larger than individual mesh cells;
- reduce reliance on tiny pixel/noise variation;
- use broad cold/warm or pale/damp land patches for composition;
- keep accent marks sparse enough that some ground remains quiet;
- add stronger detail near shore/contact/path boundaries before distributing detail everywhere;
- test from game camera before judging close-up editor screenshots.

Possible starting values for a calm base pass:

```text
Ground shape
BroadForm: 0.25 to 0.95 for combat-safe uneven fields
Roughness: 0.15 to 0.40
SurfaceDetail: 0.02 to 0.12

Base surface response
PatchScale: 7 m to 18 m
PatchContrast: 0.10 to 0.28
PatchEdgeSoftness: 0.40 to 0.75
PixelVariation: 0.00 to 0.04 unless testing Pixel/Faceted lane
BroadVariation: 0.03 to 0.10
Smoothness: low unless the feature is explicit water/ice/wet stone
SpecularStrength: low for mud/soil/snow baselines

Painted accent line first target
Density: low
Contrast: low-to-medium
Length: short
Distribution: clustered
Curvature: subtle
Masking: biased by macro patches, shore/contact, and family tuning
```

These are only starting ranges. The real test is whether the ground looks deliberately simple rather than unfinished, and accented rather than noisy.

## Open Questions

Resolved by current architecture:

- Ground now has a dedicated shader path.
- Surface family/variant authoring now lives on `GeneratedGround` with style assets.
- Surface-only modifier authoring now belongs in `GroundModifier` through `GroundModifierSurfaceEffectMode`.
- The first semantic mesh channel contract is established.

Still open after the doctrine pivot:

- Should style calibration use a temporary `GSSP_Calibration` family, or should Snowfield/Wet Mudflat receive explicit calibration variants?
- What is the minimum shader/control change needed for `PaintedAccentLines` to feel hand-authored rather than procedural?
- Should painted accent lines be generated entirely in shader from world-space noise, from a baked/generated mask, or from a cheap hybrid?
- Which existing masks should bias accent-line density first: tonal patch, damp/deposit, shore, compaction, or modifier priority?
- How should contact/edge accents be sourced for generated masses and props: existing placement data, ground modifiers, object stamps, or a later contact-mask bake?
- How much additional editor UX is needed now that Patch V3 implements the first shader feature stack?
- How many doctrine-layer controls should be exposed in `GroundSurfaceStyleProfileEditor` before the UI becomes cluttered?
- What debug views are needed for contact accents and sparse motifs after `GroundPaintedAccentLines`?
- How much pixel/faceted breakup should remain in the final style, if any?
- What runtime state resolution is needed later for footprints from the game camera, after the static style is validated?

## Risks

### Doctrine Drift

Risk:

- new work slides back into niche features, runtime systems, or one-off material tricks before the visual language is proven.

Mitigation:

- keep this document as the canonical baseline;
- require new ground features to state which doctrine pillar they serve;
- pause work that does not improve calm base, macro patches, painted accent lines, contact accents, sparse motifs, or the feature-stack resolver.

### Too Much Height Detail

Risk:

- terrain becomes visually richer but damages camera/player comfort.

Mitigation:

- keep physical height detail low;
- put most variety in static masks and shader response;
- validate from gameplay camera first.

### Procedural Noise Masquerading As Style

Risk:

- the ground looks busy but not authored.

Mitigation:

- lower noise frequency and contrast;
- prefer broad patches and sparse accents;
- cluster marks instead of distributing them uniformly;
- add debug views for doctrine layers.

### Hades Reference Overreach

Risk:

- the project tries to match Supergiant-level hand-painted terrain production.

Mitigation:

- copy Hades 1 ground grammar, not its full authored finish;
- implement reusable procedural accent layers;
- keep base ground simple and let geometry, lighting, props, rivers, rocks, and atmosphere carry the scene.

### Tunic Reference Misuse

Risk:

- the ground becomes too primitive because simple Tunic-like surfaces are treated as the target without the rest of Tunic's block/toy-world simplification.

Mitigation:

- use Tunic only for readability lessons;
- keep the main target as restrained stylized 3D terrain with higher organic/geometric complexity.

### Shader Becomes Too Broad

Risk:

- the ground shader accumulates unrelated rock, ground, weather, vegetation, and feature assumptions.

Mitigation:

- keep a dedicated ground shader path;
- document property contracts;
- organize shader code into doctrine-layer functions;
- expose debug modes.

### Feature Silo Accumulation

Risk:

- `DirectionalStreaks`, `PooledWetness`, `TrampledWear`, and future features become mutually exclusive one-off modes.

Mitigation:

- keep using the Patch V3 shader feature stack as the canonical composition path;
- require each feature recipe to map to a doctrine layer;
- do not reintroduce mutually exclusive feature modes that cannot coexist.

### Profiles Become Premature Biome System

Risk:

- too many terrain families are added before one looks good.

Mitigation:

- calibrate the doctrine on existing Snowfield, Wet Mudflat, and Grassland first;
- add new families only to test a specific style-layer need;
- defer production biome/world assembly.

### Runtime State Overbuild

Risk:

- footprint/weather infrastructure is built before the static visual stack is proven.

Mitigation:

- keep runtime state contract documented;
- implement texture allocation only after calm base, accent lines, contact accents, and feature-stack resolving are validated.

### Mask Ambiguity

Risk:

- channels mean different things to different systems.

Mitigation:

- keep a channel contract in code comments and this document;
- expose debug views;
- avoid reusing a channel for incompatible meanings.

## Deferred Work

Defer until the static doctrine stack is proven:

- ground runtime state component;
- detailed boot-shape footprints;
- rain/wetness accumulation and drying;
- snow compression runtime;
- puddle fluid simulation or puddle rendering;
- grass rendering implementation and trampling;
- roads/wagon-track spline system;
- mixed terrain/profile blending;
- production terrain streaming;
- destructible terrain;
- full biome graph;
- authored texture painting UI;
- erosion simulation;
- persistent save/load of runtime footprint and weather state;
- large-scale weather manager;
- snow depth geometry displacement;
- triplanar authored texture sets;
- terrain LOD system.

Do not treat deferral as rejection. These systems remain useful later, but they should inherit a proven ground language rather than define it prematurely.

## Definition of Done for First Doctrine Milestone

The first doctrine milestone is complete when:

- the docs define restrained stylized terrain as the canonical target;
- `GeneratedGround` still exposes family/variant authoring;
- existing Snowfield, Wet Mudflat, and Grassland variants are retuned or calibrated under the doctrine;
- the same clearing can demonstrate at least two style-calibration lanes, including the preferred hybrid target;
- the calm base reads as intentional from the game camera;
- broad macro patches are visible but not noisy;
- a first painted accent-line prototype creates sparse Hades-1-like crease/mound marks;
- contact/edge accents are either prototyped or explicitly queued as the next doctrine layer;
- ground remains combat-safe and camera-stable;
- river corridor material sync still works;
- semantic debug views still show the mesh channel contract;
- no runtime surface state has been added merely to compensate for an undecided static style.

## Working Checklist Summary

Active checklist after the doctrine pivot:

- [x] Patch T - establish surface/height modifier contract.
- [x] Patch U - prove compaction/path mask can feed a shader feature.
- [x] Patch V0 - document and lock the new ground visual doctrine.
- [x] Patch V1 - create style calibration setup.
- [x] Patch V2 - simplify and retune calm base ground.
- [x] Patch V2B - add Grassland baseline family and establish the three-family test set.
- [x] Patch V3 - implement shader feature stack and painted accent lines.
- [x] Patch V3C - fix object-level debug dropdown labels and Unity 6.5 obsolete editor refresh warning.
- [x] Patch V3D - refine Painted Accent Lines raw mask from large strips into smaller clustered micro-strokes.
- [x] Patch V3E - replace straight line stamps with curved visual-relief terrain-fold strokes.
- [x] Patch V3F - expose painted-accent relief channels and strengthen visual relief.
- [x] Patch V3F.1 - make relief continuity and signed-side debug readable; validation still rejects the curve-distance source model.
- [x] Patch V3G - retire the curve-distance stroke model and document the generated fold-field direction.
- [x] Patch V3H - add generated fold-field data skeleton and shader sampling fallback plumbing.
- [x] Patch V3I - prototype local-space 256x256 fold-field generation at ground regeneration/dirty time.
- [x] Patch V3I.1 - correct fold body shapes from oval stamps into curved tapered ridge/fold bodies.
- [x] Patch V3I.2A - retire candidate-stamp generator and document the continuous field plan.
- [x] Patch V3I.2 - implement continuous domain-warped fold height field generation.
- [x] Patch V3I.3 - add editor/debug-only fold-field height preview mesh.
- [x] Patch V3I.3A - isolate fold-field debug data from final render and improve preview readability.
- [x] Patch V3J.0 - add a debug-only Painted Accent final visual-response proof view.
- [ ] Patch V3I.4 - correct continuous field shape if preview/prototype shows blocky/noisy terraces.
- [ ] Patch V3J - extract and polish one-sided accent lines from fold-field gradients/contours.
- [ ] Patch V3K - use generated line/body/signed channels for accepted production visual fold response.
- [ ] Patch V3L - tune Painted Accent Lines per production family after fold-field validation.
- [ ] Patch V4 - prototype contact/edge accents.
- [ ] Patch V5 - prototype sparse motif/stamp layer.
- [ ] Patch V6 - feature-stack authoring polish, warnings, and per-kind drawers.
- [ ] Later - resume runtime state design only after static doctrine validation.

Historical patch notes remain below for context.

### 2026-07-10 — Patch V3J.0: Painted Accent Final Visual-Response Proof

Patch V3J.0 adds `Ground Painted Accent Final Prototype`, a debug-only view that tests whether the existing generated fold-field channels can produce the desired painted fold/crease response before spending time tuning the continuous generator.

This patch deliberately keeps the V3I.3A normal-render isolation in place. The new prototype view is only selected through the object/material debug mode; it does not reactivate Painted Accent contribution in the normal final ground render.

Prototype response contract:

```text
R / line channel      -> narrow selected contour/crease visibility
G / body channel      -> local context gate only, not broad visible albedo noise
B / signed side       -> side polarity for crease/highlight balance
normal final render   -> unchanged/clean while generated fold field is active
```

The key validation question is not whether the current field is already well placed. The current generator may still be blocky/noisy. The question is whether the field-first representation can be shaded as narrow painted terrain folds rather than broad stains. If the prototype response is promising, continue with field-shape correction/tuning. If it still reads as stains even with G restricted to context, revise the response model/channel contract before tuning the generator.

Validation after this patch should confirm:

```text
normal final render remains clean
Ground Painted Accent Relief / Signed Relief / Lines still expose raw channels
Ground Painted Accent Final Prototype appears in the debug dropdown
the prototype emphasizes narrow crease/highlight response instead of broad G stains
Build Height Preview and Clear Height Preview still work
```


### 2026-07-09 — Patch V3I.3A: Fold Field Debug Isolation + Preview Color Readability

Patch V3I.3A fixes the issues found during V3I.3 validation.

Observed validation result:

```text
The height preview mesh was geometrically displaced and useful from a side/profile view.
The preview material was nearly one color from top view.
The normal final ground render showed fold-field-correlated noise even after clearing the preview mesh.
```

Fixes:

- The final forward pass now zeros Painted Accent final-render contribution while `_GroundPaintedAccentFoldFieldEnabled` is active. Generated fold-field data remains available to debug views and the height preview mesh, but no longer contaminates the normal final render.
- Added `Hidden/PS3D/Ground Fold Field Height Preview`, a small debug shader that reads preview mesh vertex color/body values and maps them to a visible low/mid/high height gradient.
- `GeneratedGround` now prefers this debug shader for the preview mesh material.
- The preview renderer disables shadow casting and shadow receiving.
- `ClearPaintedAccentFoldFieldHeightPreview()` now removes all child preview objects whose names begin with `__FoldFieldHeightPreview_Debug`.

V3I.3A is diagnostic/pipeline correctness only. It does not tune the field generator, alter production mesh geometry, change collision, perform final line extraction, tune families, or add runtime displacement.

Validation after this patch should confirm:

```text
normal final render stays clean after building and clearing preview
height preview is readable from top view and side view
Projected Painted Accent debug views still show the generated field
```


### 2026-07-09 — Patch V3I.3: Fold Field Height Preview Debug Mesh

Patch V3I.3 implements the planned Option B preview for the generated Painted Accent fold field. The existing Painted Accent debug views are projected channel views; this patch adds an editor/debug-only mesh that shows the G/body field as actual relief.

Implementation:

```text
GeneratedGround inspector:
  Build Height Preview
  Clear Height Preview

GeneratedGround:
  stores the latest generated G/body values returned by the fold-field generator
  builds a temporary child mesh named __FoldFieldHeightPreview_Debug
  samples the existing GroundHeightFieldSnapshot for base height
  offsets preview vertices by G * debug height scale
  clears preview mesh/material/object on request

GroundPaintedAccentFoldFieldGenerator:
  keeps the same texture generation path
  additionally returns the same smoothed body array used for the G channel
```

The preview is intentionally diagnostic-only:

```text
no production mesh displacement
no collision change
no gameplay terrain deformation
no generator tuning
no final contour extraction
no new layer or tag dependency
```

Validation should use both the projected `Ground Painted Accent Relief` view and the height preview mesh. If the preview shows blocky value-noise terraces, the next patch should tune the continuous generator before V3J. If the preview shows useful broad terrain forms, V3J line extraction can proceed.


### 2026-07-09 — Patch V3I.2: Continuous Domain-Warped Fold Height Field Prototype

Patch V3I.2 replaces the V3I.2A neutral placeholder with a continuous domain-warped scalar field generator. This is the first active generator that follows the accepted field-first model instead of a candidate/stamp model.

Implemented generator stages:

```text
GenerateRawContinuousField(...)
  local-space texel coordinates
  deterministic domain warp
  broad fractal value field
  medium fractal value field
  ridge-like fractal component
  directional continuity component

ApplySemanticSupport(...)
  multiplies the raw field by existing generated ground mask support

ShapeBodyField(...)
  computes a percentile coverage threshold from the supported field
  soft-thresholds the field into the G/body channel

SmoothBodyField(...)
  applies one light smoothing pass

BuildPixelsFromContinuousField(...)
  writes:
    R = rough edge/contour candidate from G
    G = continuous body field
    B = signed gradient polarity from G
    A = semantic support / reserved
```

The generator intentionally does not reintroduce:

```text
BuildCandidates(...)
RasterizeCandidates(...)
FoldCandidate
discrete mark spawning
ellipse stamps
curved ridge stamps
```

Validation target:

```text
Ground Painted Accent Relief
```

The relief view should now show a continuous terrain-like secondary fold field with organic raised regions and quiet negative space. `Ground Painted Accent Lines` remains rough and should not be judged as final art until V3J.

Debug tooling plan:

```text
Patch V3I.3 - Fold Field Height Preview Debug Mesh
```

V3I.3 will use Option B: an editor/debug-only preview mesh generated from the fold-field texture and displaced by the G channel. This is planned because projected texture-channel debug views are useful but not sufficient for reading the actual shape of a height-field generator. The preview must remain debug-only and must not affect mesh geometry, collision, or gameplay.

V3I.2 adds no shader rewrite, final line extraction, fake normal, mesh displacement, 3D preview implementation, family tuning, chunk bake system, or resolution tiering.


### 2026-07-09 — Patch V3I.2A: Candidate-Stamp Generator Retirement + Continuous Field Plan

Patch V3I.2A is a cleanup/redirection patch after V3I.1 validation showed that the second generator prototype was still the wrong model. V3I proved the generated local-space texture path, and V3I.1 reduced the large oval/leaf stamp issue, but the generator remained candidate/stamp based and produced sparse brush-like marks instead of a natural secondary height layer.

This patch intentionally removes the active candidate-stamp generator internals from `GroundPaintedAccentFoldFieldGenerator.cs`:

```text
BuildCandidates(...)
RasterizeCandidates(...)
FoldCandidate
candidate cell spawning
candidate curvature/asymmetry/side-lobe stamp model
```

The generator now returns a neutral 256x256 RGBA32 placeholder:

```text
R = 0
G = 0
B = 128 / neutral signed side
A = 0
```

The neutral placeholder preserves the `GeneratedGround` -> material property block -> shader data path while preventing further tuning of the rejected model. It is expected that Painted Accent debug views show no generated fold marks until V3I.2 implements the continuous field generator.

Retained architecture:

```text
GeneratedGround-owned fold texture lifecycle
local/object-space sampling
256x256 active-chunk policy
R/G/B/A texture contract
shader router and debug views
```

Next implementation target:

```text
Patch V3I.2 - continuous domain-warped scalar field generation
  GenerateBaseNoiseField(...)
  GenerateDomainWarp(...)
  ShapeContinuousBodyField(...)
  ApplySemanticSupport(...)
  SmoothBodyField(...)
  BuildPixelsFromContinuousField(...)
```

V3I.2A adds no continuous field implementation, final line extraction, shader rewrite, mesh displacement, fake normal, 3D preview, family tuning, chunk bake system, or resolution tiering.


### 2026-07-09 — Patch V3I.1: Fold Body Shape Correction

Patch V3I.1 corrects the first generated fold-field body model after validation showed that the V3I relief channel successfully came from generated local-space field data but still read as large soft oval/leaf stamps. This patch changes only the generator and docs. It does not change the shader router, material property path, debug enums, mesh generation, river code, or surface family assets.

Generator changes:

- Replaced the single-ellipse fold candidate with a short curved tapered ridge/fold primitive.
- Reduced fold candidate density for more quiet negative space.
- Increased candidate cell spacing.
- Added candidate curvature, width jitter, asymmetry, and deterministic local warp.
- Added a small optional side lobe so bodies can break away from perfect capsules.
- Kept max-composition into the body field so overlapping forms do not become noisy additive mush.
- Reduced the smoothing blur from `0.65 / 0.35` to `0.74 / 0.26`.
- Kept the line channel rough; final line extraction remains Patch V3J.

V3I.1 validation still focuses on `Ground Painted Accent Relief`. Success means the body field no longer reads as repeated smooth oval stamps and starts reading as short irregular low terrain folds/ridges. `Ground Painted Accent Lines` may remain rough.


### 2026-07-09 — Patch V3I: Local-Space 256x256 Fold Field Generator Prototype

Patch V3I is the first active implementation of the generated visual fold-field direction. It replaces the active Painted Accent Lines data source, when the feature is enabled, with a generated local-space texture owned by `GeneratedGround`.

Implemented data policy:

```text
visible authored chunk with PaintedAccentLines active:
  generate one 256x256 RGBA32 fold-field texture

hidden/offscreen/background chunks:
  disable PaintedAccentLines entirely
  no low-resolution fallback texture
```

Budget:

```text
256x256 RGBA32 = 256 KiB per active chunk
10 chunks  = 2.5 MiB
50 chunks  = 12.5 MiB
100 chunks = 25 MiB
200 chunks = 50 MiB
```

Runtime/chunk-library policy:

```text
Chunks are authored/generated in editor or at load/camp rebuild time.
The runtime map builder places, rotates, and connects reusable authored chunks.
Fold-field sampling is local/object-space so the field rotates with the chunk.
The feature is not a per-frame CPU simulation.
```

Implementation details:

- Added `GroundPaintedAccentFoldFieldGenerator`.
- The generator builds deterministic soft fold candidates in local chunk space.
- Candidates are rasterized into a scalar body field instead of deriving a body from procedural curve-distance tubes.
- The body field is semantically supported by existing ground masks.
- The body field is lightly smoothed.
- The signed channel is derived from the body-field gradient.
- The line channel is a rough temporary edge/gradient candidate and is not final line-art polish.
- The generated texture is uploaded with no mipmaps and the CPU texture copy is discarded.
- The retired V3D-V3F.1 curve-distance path remains as fallback when no active `PaintedAccentLines` feature exists.

V3I validation should judge `Ground Painted Accent Relief` first. Success means the relief/body debug view reads as terrain-field-like soft folds rather than fat tubes, scratches, or side rails. V3J will refine line extraction after the body field is accepted.

### 2026-07-09 — Patch V3H: Generated Fold Field Data Skeleton

Patch V3H adds the inactive data path for the accepted fold-field model without changing visible output. `GeneratedGround` now owns a neutral generated fold-field texture placeholder and pushes it through the same material-property-block path used by the ground renderer and river corridor renderer. The ground shader declares the fold-field texture and parameters, adds a non-retired fold-field resolver, and routes Painted Accent debug/final sampling through a new feature router.

V3H data contract:

```text
R = selected accent line mask
G = relief/body/fold-height channel
B = signed side encoded 0..1, where 0.5 is neutral
A = reserved / validity / future support
```

V3H intentionally keeps `_GroundPaintedAccentFoldFieldEnabled = 0`, so the retired curve-distance shader path remains the runtime fallback until V3I generates real fold-field data. This patch adds no noise generation, no edge extraction, no family tuning, no fake normals, no mesh channels, no material assets, and no runtime state.

### 2026-07-09 — Patch V3G: Painted Accent Direction Reset / Fold-Field Plan

Patch V3G is a redirection patch. It does not delete or disable the V3D-V3F.1 shader path, but it clearly retires that curve-distance stroke model as the final solution. The old model is kept only as fallback/comparison code until the generated fold-field replacement exists. It must not be tuned further as the chosen direction.

Rejected source model:

```text
procedural curve stroke
  -> distance-to-curve contour
  -> inflated tube-like relief body
  -> side rails derived from curve side
```

Chosen source model:

```text
generated visual fold field F(x,z)
  -> fold-height/body channel
  -> selected contour/ridge/edge line channel
  -> gradient/polarity signed-side channel
```

Retained architecture:

```text
PaintedAccentLines feature kind
GroundSurfaceVariantRecipe feature stack
GeneratedGround material-property-block ownership
object-level Ground Debug dropdown
Ground Painted Accent Lines / Relief / Signed Relief debug modes
three-channel line/body/signed-side validation contract
```

Planned implementation sequence after V3G:

```text
Patch V3H - Generated Fold Field Data Skeleton
Patch V3I - Fold Field Generator Prototype
Patch V3J - Edge/Contour Extraction
Patch V3K - Final Render Fold Response
Patch V3L - Production Family Tuning
```

This remains visual-only. No physical terrain mesh deformation, collision changes, runtime footprints/wetness, decals, contact accents, sparse motifs, or family tuning are part of V3G.

### 2026-07-09 — Patch V3F.1: Per-Stroke Relief Correction

Patch V3F.1 made the current three-channel debug contract more useful by keeping the relief body continuous instead of fragmenting it with the line breakup mask, and by making the signed-side debug view display visible polarity colors. Validation after V3F.1 showed the debug channels were useful but the underlying source model was still wrong: the line remained a procedural stroke, the relief body read as a fat distance tube, and the signed side read as parallel rails. This validation directly motivates Patch V3G's fold-field direction reset.

### 2026-07-09 — Patch V3F: Painted Accent Relief Debug + Visual Relief Strengthening

Validation after V3E showed the curved marks were directionally better but still too wide and still read as 2D painted stamps. Patch V3F keeps the feature shader-only and visual-only, but exposes the internal three-channel model directly in object-level debug:

```text
Ground Painted Accent Lines
  thin line contour / crease mask

Ground Painted Accent Relief
  broader soft relief body around the contour

Ground Painted Accent Signed Relief
  signed side field remapped from [-1, 1] to [0, 1] for debug
```

The shader now thins the contour independently from the wider relief body and uses the signed relief side more deliberately in normal rendering: one side receives painted shadow, the opposite side receives painted highlight, and the narrow contour remains the dark/tinted crease. No mesh displacement, collision, decals, textures, generated atlases, runtime state, new mesh channels, new components, contact accents, sparse motifs, or family asset retuning are included in this patch.

### 2026-07-09 — Patch V3E: Painted Accent Lines Curved Relief Model

Validation after V3D showed the raw mask was thinner but still fundamentally read as straight 2D bars/line stamps rather than the Hades-1-like curved mound/crease marks defined by the ground doctrine. Patch V3E keeps the shader feature stack and object-level debug workflow unchanged, but replaces the straight micro-stroke primitive with short irregular curved stroke paths built from several local control points. The feature now also outputs a soft signed relief body used for subtle painted shadow/highlight value shaping. This is visual relief only: no terrain mesh height, collision, decals, textures, runtime state, or generated atlases are added.

### 2026-07-09 — Patch V3D: Painted Accent Lines Mask Refinement

Refined the raw `GroundPaintedAccentLines` mask after object-level debug validation showed the first V3 generator was drawing oversized isolated strips/crescents. Patch V3D keeps the feature stack architecture unchanged and updates only the procedural mask primitive: scale now controls group spacing, stroke length/thickness are capped in world units, accents are generated as smaller micro-strokes, and a broad cluster gate prevents uniform hatching. No color-response tuning, style asset retuning, runtime state, decals, textures, mesh changes, or contact accents were added.

### 2026-07-09 — Patch V3C: GeneratedGround Debug UX Hotfix

Fixed two validation blockers from the V3/V3B workflow: object-level `GeneratedGround` debug labels no longer use slash characters that Unity treats as submenu separators, and `GroundSurfaceStyleProfileEditor` no longer uses the obsolete `FindObjectsByType` overload with `FindObjectsSortMode`. This patch does not change shader logic, style recipes, mask generation, or Painted Accent Lines behavior.

### 2026-07-09 — Patch V3B: GeneratedGround Object-Level Debug Views

Exposed ground debug selection directly on `GeneratedGround` under `Ground Debug`. The component now writes `_MaskDebugMode` through its `MaterialPropertyBlock`, so authors can validate ground masks and doctrine-layer debug views from the generated-ground object without opening shared material assets. Debug changes refresh material properties only and do not regenerate terrain.

### 2026-07-09 — Patch V3A: Generated-Mass Shader Compile Hotfix

Fixed compile isolation after Patch V3 by guarding the ground-only painted-accent-line resolver so `PS3D/Pixel Surface Lit` / generated-mass shader paths no longer compile references to ground-only uniforms.

### 2026-07-09 — Patch V3: Shader Feature Stack and Painted Accent Lines

Changed the ground feature renderer from the old single `_GroundFeatureMode` proof slot to explicit stackable shader-property blocks per supported feature kind. Added `PaintedAccentLines = 20` as the first doctrine-layer feature. Updated Snowfield, Wet Mudflat, Grassland, and Style Calibration assets so Painted Accent Lines can coexist with DirectionalStreaks, PooledWetness, and TrampledWear. Added `GroundPaintedAccentLines = 28` debug mode.

### 2026-07-09 — Patch V2B: Grassland Baseline Family

Implemented as an asset/docs patch after validation confirmed Snowfield and Wet Mudflat baselines but identified the need for a living-ground family before shared feature work.

- Added `GSP_Grassland.asset` as a semantic profile with high vegetation suitability, moderate damp/deposit response, low snow eligibility, and soft broad patches.
- Added `GSSP_Grassland.asset` as a production surface family.
- Added `Clean Meadow`, `Patchy Meadow`, `Damp Meadow`, and `Worn Meadow` variants.
- Kept the patch asset-only; no grass blades, vegetation rendering, runtime state, shader changes, river logic, or scene edits were added.
- Established Snowfield, Wet Mudflat, and Grassland as the canonical three-family test set for Patch V3 and later shared style layers.

### 2026-07-09 — Patch V2: Base Ground Simplification and Calibration Cleanup

Implemented as an asset/docs retune after the first Style Calibration screenshots.

- Renamed `Pixel / Faceted` to `Pixel-Faceted` while preserving stable id `calibration.pixel_faceted`.
- Recorded the calibration outcome: Calm Base is the best foundation; Hybrid remains the target philosophy; Pixel-Faceted should not be the default ground style; the Hades proxy is not a substitute for real painted accent lines.
- Retuned `GSSP_Snowfield.asset` toward calmer, matte, lower-noise snow variants.
- Retuned `GSSP_WetMudflat.asset` toward matte, broad, lower-noise mud variants.
- Reduced excessive pixel variation, pixel effect strength, cell warp, patch blend, damp darkening, and overly strong feature response where it fought the doctrine.
- Added no code, shader changes, runtime state, scene edits, materials, or river changes.

### 2026-07-09 — Patch V1: Style Calibration Setup

Implemented as an asset-only calibration patch after the ground doctrine was accepted.

- Added `GSP_StyleCalibration.asset` as a neutral semantic profile for visual-lane comparisons.
- Added `GSSP_StyleCalibration.asset` as a temporary style family with `Calm Base`, `Hades Accent Proxy`, `Hybrid Target Proxy`, and `Pixel-Faceted` variants.
- Used existing `GroundSurfaceStyleProfile`, `GroundSurfaceVariantRecipe`, `GroundMaterialControls`, and `GroundSurfaceFeatureRecipe` architecture.
- Used `DirectionalStreaks` only as a temporary proxy for Hades-like accent rhythm in the Hades and Hybrid variants.
- Added no code, shader changes, runtime state, scene edits, materials, or river changes.

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

Historical Patch M renderer contract:

```text
_GroundFeatureMode
_GroundFeatureStrength
_GroundFeatureScale
_GroundFeatureContrast
_GroundFeatureMaskInfluence
_GroundFeatureDirection
_GroundFeatureSeed
```

This single-slot contract is superseded by Patch V3. Current ground rendering resolves the feature list as a stack and writes explicit property blocks per supported feature kind. `_GroundFeatureMode` remains only as a hidden compatibility property and must not receive new feature modes. River corridor renderers remain style-agnostic and continue to receive the resolved parent-ground material contract through the same property block refresh path.

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

Patch N also made `Pooled Wetness` a renderable shader-only proof feature. Historically it used the single `_GroundFeatureMode` contract added in Patch M.

That path is superseded by Patch V3. `PooledWetness` is now one supported layer in the shader feature stack and can coexist with other supported ShaderOnly features such as PaintedAccentLines.

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

The documentation update records the split between semantic surface profiles, visual style profiles, variant recipes, material controls, feature recipes, and the `GeneratedGround` resolver. Its roadmap was later superseded by Patch V0, which paused the immediate runtime-state queue and made style calibration, painted accent lines, contact accents, sparse motifs, and feature-stack aggregation the active direction. Patch V3 later implemented the first shader feature stack baseline.

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


### Patch U — Trampled Wear Feature / Compaction Feature Proof

Patch U is the first feature that consumes the Patch T authored surface-mask contract directly in the ground shader.

Concrete flow:

```text
GroundModifier surface mask
→ GroundGenerator metadata pass
→ UV2.x compaction/path/flatten influence
→ GroundSurfaceFeatureKind.TrampledWear
→ shader-only feature response
```

Patch U proves the data path but does not define the final ground direction. After the doctrine pivot, `TrampledWear` is classified as a useful proof and future compaction-response layer, not the current foundation. Do not keep polishing trampled mud while the overall static style remains undecided.

Patch U intentionally does not solve final footprints, snow compression, grass suppression, puddles, runtime wetness, roads, wagon tracks, or painted accent-line language.

### Patch V0 — Ground Visual Doctrine Documentation

Patch V0 locks the new ground baseline in `Assets/Docs/Ground_Visual_Design_and_Architecture.md`:

```text
Restrained stylized terrain
= BOTW/TOTK-like base-material restraint
+ Hades-1-like painted ground accents
+ procedural masks and reusable style layers
+ family/variant tuning.
```

This patch also changes this implementation plan so it no longer acts as the sole home for ground design doctrine. It pauses the old immediate runtime-state roadmap and makes style calibration the next milestone.

Rules after Patch V0:

- family/variant architecture stays;
- families define material identity;
- variants tune the shared visual stack;
- `GroundSurfaceFeatureRecipe` entries should evolve toward reusable doctrine layers;
- painted accent lines are the first new foundational visual feature;
- contact/edge accents are the next major grounding layer;
- sparse motifs come after accent lines/contact response;
- runtime state resumes only after the static visual language is validated;
- no new niche terrain features should be prioritized until the doctrine stack is working.

Patch V0 changes documentation only.

