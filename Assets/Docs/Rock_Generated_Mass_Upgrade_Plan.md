# Rock Generated Mass Upgrade Plan

## Purpose

Improve generated rock shape and surface readability while preserving the current blocky, simple, low-poly visual language.

The existing rocks already hit the intended direction: broad geometric masses, rectangular or primitive silhouettes, sparse cuts, and a pixel-like noise material. This plan adds more authored structure around that successful baseline rather than replacing it.

## Current State

Primary implementation files:

- `Assets/Game/Procedural/Masses/GeneratedMass.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`
- `Assets/Game/Procedural/Core/MeshData.cs`
- `Assets/Game/Procedural/Core/MeshBuilder.cs`
- `Assets/Game/Rendering/PixelSurface/Shaders/SG_PixelSurfaceLit.shadergraph`
- `Assets/Game/Rendering/PixelSurface/SubGraphs/SGS_PixelSurfaceCore.shadersubgraph`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelCellVariation.hlsl`
- `Assets/Game/Demo/Materials/Stone/M_PixelStone.mat`

The current generated mass system separates shape and surface well:

- `ShapeSeed` controls proportions, major planes, cuts, lean, and silhouette.
- `SurfaceSeed` controls triangulation relief and vertex-colour variation.
- `MassArchetype` controls the base family and default recipe.
- `MeshData` already supports vertex colours, UV0, optional UV2, and optional normals.
- `MeshBuilder` applies colours and recalculates normals and tangents.
- The stone material uses the generic pixel surface shader and consumes procedural pixel variation plus vertex colour.

Existing archetypes:

- `TerrainBoulder`
- `SquatBoulder`
- `StandingStone`
- `FlatSlab`
- `BrokenChunk`
- `PolishedStone`

## Design Constraints

The upgrade must:

- keep existing serialized archetype values stable;
- keep existing generated masses valid;
- avoid changing the river interaction contract;
- avoid concave or fragile collision geometry in the first pass;
- keep the output simple enough for the current elevated/isometric camera;
- preserve the blocky mass language seen in the current screenshots;
- make any new material response optional or backward-compatible.

The upgrade should not:

- turn rocks into realistic scanned boulders;
- require authored textures for the first improvement pass;
- make Shader Graph and HLSL diverge indefinitely;
- introduce a broad biome system before one material/style path is proven.

## Proposed Archetype Additions

Add new `MassArchetype` values at the end of the enum only. This preserves the integer value of existing serialized archetypes.

### 1. `LayeredStone`

Purpose: sedimentary, slate-like, or frost-split stone for riverbanks, cliffs, and cold exposed zones.

Shape language:

- broad rectangular base;
- flatter stacked proportions;
- tapered stacked silhouette;
- subtle stepped side planes;
- physical stratum shelves deferred until the mesh can be kept watertight;
- moderate vertical relief so it does not collapse back into `FlatSlab`.

Implementation direction:

- use a dedicated stacked-ring builder rather than only plane cuts;
- build 3-6 block strata with deterministic width/depth offsets;
- keep the first implementation as one closed shell;
- defer raised shelf geometry until proper face inset/extrusion or mesh merging is available;
- keep `SurfaceFacetDensity` low or medium;
- keep `EdgeCharacter` sharp or chipped depending on defaults.

Suggested defaults:

- `FormComplexity.Simple` or `Moderate`
- `SurfaceFacetDensity.Low`
- `EdgeCharacter.Chipped`
- `ShapeDiversity.Broad`
- `GroundingStyle.Embedded`
- `LeanStyle.None`

### 2. `FracturedPillar`

Purpose: a sharper standing stone variant with a stronger mythic or hostile silhouette than the current clean `StandingStone`.

Shape language:

- tall rectangular core;
- one dominant cleaved face;
- diagonal shoulder cuts;
- asymmetric top;
- sharper vertical side planes;
- still stable and mostly convex.

Implementation direction:

- reuse the plane-cut builder;
- add a pillar-specific macro profile that applies one strong side cleave and one top diagonal;
- reduce random secondary chipping unless `EdgeCharacter.Chipped`;
- preserve a broad front or side plane for readability.

Suggested defaults:

- `FormComplexity.Moderate`
- `SurfaceFacetDensity.Low`
- `EdgeCharacter.Chipped`
- `ShapeDiversity.Wild`
- `GroundingStyle.Stable`
- `LeanStyle.Pronounced`

### 3. `CarvedMarkerStone`

Purpose: an aggressive rune/shrine/story marker archetype that stays within the mass system but reads as deliberately ritual, carved, and unlike the ordinary rock families.

Shape language:

- tall, narrow marker-stone body;
- one intentionally broad, flatter presentation face;
- deep back and side bites;
- sharp asymmetric crown or broken ritual top;
- silhouette notches and an asymmetric crown that frame the presentation face;
- raised cross/bar geometry deferred until it can be merged or rendered as material/decal detail;
- strong silhouette difference from `StandingStone`, `BrokenChunk`, and `LayeredStone`.

Implementation direction:

- use a dedicated single-shell marker builder rather than only plane cuts;
- preserve a wide front/back presentation plane;
- build an asymmetric crown, flared base, and side bite into one closed extruded silhouette;
- defer shallow raised face bars until a watertight inset/extrusion or decal/material path exists;
- optionally write a future vertex/UV mask that identifies the presentation face;
- do not implement actual runes in this patch unless a separate carving system is approved.

Suggested defaults:

- `FormComplexity.Complex`
- `SurfaceFacetDensity.Low`
- `EdgeCharacter.Sharp`
- `ShapeDiversity.Wild`
- `GroundingStyle.Stable`
- `LeanStyle.None`

## Deferred Physical Detail

The first attempt at physical shelves and raised marker bars showed an important constraint: stacked or attached volumes can leave visible gaps, internal faces, or non-watertight seams when used as generated mass geometry.

Deferred details:

- raised cross/bar geometry on `CarvedMarkerStone`;
- deep inset/engraved marker symbols;
- physical layer shelves on `LayeredStone`;
- any other protruding detail that is separate from the main shell.

These should return through one of the following safer paths:

- true face inset/extrusion on an existing closed face;
- a deliberate mesh-merge or CSG-like operation;
- decal or material detail projected onto a closed shell;
- shader-side masks once the HLSL pixel surface path exists.

Until then, `LayeredStone` and `CarvedMarkerStone` should prioritize watertight closed silhouettes over physical relief.

## Surface Data Upgrade

The mesh currently writes deterministic random variation mainly through vertex colour red while green and blue remain fixed. This should become a small semantic material contract.

Recommended vertex colour contract:

- `R`: existing deterministic surface noise, preserved for compatibility.
- `G`: exposure/upward-facing/flat-surface mask.
- `B`: crevice, edge, base, or contact-darkening mask.
- `A`: reserved for future biome/material-state blending.

Initial implementation can compute these per rendered vertex in `MassGenerator.AddRenderedVertex`.

Suggested inputs:

- height within bounds;
- normal or triangle normal;
- distance to base;
- face orientation;
- deterministic seed variation;
- optional archetype bias.

Expected material result:

- flat upward planes become slightly lighter or frostier;
- lower/base areas can darken subtly;
- chipped edges and tight cut regions can receive darker tones;
- random pixel noise remains present but no longer carries all surface meaning.

## Inspector-Owned Base Colour

Generated masses should support a per-object base colour selected directly on the `GeneratedMass` component.

Purpose:

- allow different coloured stones without duplicating materials;
- keep `M_PixelStone` and later HLSL stone shaders as shared material assets;
- let individual generated objects vary by region, biome, placement, or authored scene composition;
- match the river pattern where visual tuning is owned by the object and supplied to the renderer at runtime.

Important distinction:

- this is a base colour or base tint, not the final guaranteed pixel colour;
- later effects such as snow, wetness, moss, soot, rune glow, crevice darkening, exposure brightening, or biome overlays may still modify the final rendered colour;
- the Inspector colour should be the starting stone colour that those effects layer over.

Implementation direction:

- add a serialized `Color baseColor` field to `GeneratedMass`;
- apply it with a `MaterialPropertyBlock` on the object's `MeshRenderer`;
- target `_BaseColor` for the current Shader Graph and the future HLSL shader;
- the current Shader Graph already exposes `Base Color` as `_BaseColor`, so the HLSL transition is not required for the initial per-object base-colour patch;
- do not instantiate or duplicate materials for per-object colour;
- refresh the property block after regeneration, validation, enable, and inspector changes;
- preserve shared material assignment and river interaction behaviour.

Acceptance:

- two `GeneratedMass` objects using the same shared stone material can show different base colours;
- changing the colour in the Inspector updates that object only;
- generating a new shape or surface does not reset the selected colour;
- future material effects can still layer on top of the selected base colour.

## Pixel Surface Rendering Upgrade

The current pixel variation is good but can read as evenly noisy. Keep it, but add structure.

Recommended improvements:

- add broad low-frequency blotch variation beneath the small cells;
- warp the cell lookup position slightly before hashing;
- preserve quantized tone steps;
- blend pixel variation with vertex colour semantic masks;
- expose material controls for exposure brightening, crevice darkening, and broad blotch amount.

The result should still look pixel-like. The upgrade should make the noise feel attached to the rock form.

## Shader Graph to HLSL Transition

This upgrade should include a controlled transition from the current Shader Graph pixel surface shader to a handwritten HLSL shader.

Reason:

- the river rendering roadmap already establishes an HLSL-first preference;
- material behaviour is becoming more semantic and easier to reason about in code;
- generated mesh channels need a clear, stable contract;
- Shader Graph JSON is difficult to review and patch safely.

### Proposed Target Shader

Create:

- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader`

Keep for reference during migration:

- `Assets/Game/Rendering/PixelSurface/Shaders/SG_PixelSurfaceLit.shadergraph`
- `Assets/Game/Rendering/PixelSurface/SubGraphs/SGS_PixelSurfaceCore.shadersubgraph`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelCellVariation.hlsl`

The new HLSL shader should preserve the existing material property names where practical:

- `_BaseColor`
- `_BaseMap`
- `_PixelCellSize`
- `_PixelSeed`
- `_PixelToneCount`
- `_PixelClusterStrength`
- `_PixelVariation`
- `_PixelVertexVariation`
- `_PixelEffectStrength`
- `_Smoothness`

Add new properties only after the baseline material visually matches:

- `_PixelBroadVariation`
- `_PixelWarpStrength`
- `_ExposureTintStrength`
- `_CreviceDarkenStrength`
- `_BaseDarkenStrength`

Patch 9 material profiles may add:

- `_Wetness`
- `_WetDarkenStrength`
- `_WetPixelSoftening`
- `_WetSmoothnessBoost`
- `_DirectStrength`
- `_ShadowAmbientStrength`
- `_FlatNormalStrength`
- `_FrostStrength`
- `_FrostCoverage`
- `_FrostContrast`
- `_FrostCreviceDarken`
- `_MonolithicFlatten`
- `_MonolithicSmoothnessBoost`
- `_ProfileContrast`
- `_ProfilePixelContrast`

### Migration Requirements

The full HLSL shader migration must:

- compile in URP;
- support forward lit rendering;
- receive shadows;
- respect base colour, smoothness, and pixel variation;
- consume vertex colour red in the baseline shader and the full green/blue semantic contract in the follow-up response patch;
- keep material instances easy to migrate;
- provide visual parity with `M_PixelStone` before extra features are enabled.

Do not delete the Shader Graph in the same patch. First switch only the test stone material or a duplicate material to the HLSL shader, validate, then migrate the main material.

## Rough Implementation Plan

Checklist:

- [x] Patch 1 - Document and baseline capture
- [x] Patch 2 - Vertex colour semantic contract
- [x] Patch 3 - Add `LayeredStone` closed-shell archetype
- [x] Patch 4 - Add `FracturedPillar`
- [x] Patch 5 - Add `CarvedMarkerStone` closed-shell archetype
- [x] Patch 6 - Inspector-owned base colour
- [x] Patch 7 - HLSL pixel surface baseline, provisional until Unity shader import/visual validation
- [x] Patch 8 - HLSL semantic surface response
- [x] Patch 9 - Shader-driven material profiles and variants, provisional until Unity shader import/visual validation
- [x] Patch 10 - Stylized value shaping for HLSL rocks, provisional until Unity visual tuning
- [x] Patch 11 - Mesh-authored edge and crease mask contract, provisional until Unity mask debug validation
- [x] Patch 12A - Surface mask authoring correction
- [ ] Patch 12B - Profile-aware convex edge and concave crease material response
- [ ] Patch 13 - Dirty surface mottle and material breakup
- [ ] Patch 14 - Crack and seam language

### Patch 1 - Document and Baseline Capture

Status: implemented.

Checklist status:

- [x] document current constraints and desired changes;
- [ ] capture before screenshots of representative rocks;
- [x] record current `M_PixelStone` property values;
- [ ] identify existing scene masses that cover small, large, standing, slab, broken, and polished shapes.

Work:

- document current constraints and desired changes;
- capture before screenshots of representative rocks;
- record current `M_PixelStone` property values;
- identify existing scene masses that cover small, large, standing, slab, broken, and polished shapes.

Acceptance:

- the upgrade has a clear implementation order;
- no runtime or scene behaviour changes.

### Patch 2 - Vertex Colour Semantic Contract

Status: implemented.

Checklist status:

- [x] preserve red channel behaviour as much as possible;
- [x] calculate green exposure mask;
- [x] calculate blue crevice/base/contact mask;
- [x] keep alpha at `1` until a real consumer exists;
- [x] add comments documenting the channel contract close to generation code.

Primary files:

- `MassGenerator.cs`
- optionally `MeshData.cs` only if a helper improves clarity.

Work:

- preserve red channel behaviour as much as possible;
- calculate green exposure mask;
- calculate blue crevice/base/contact mask;
- keep alpha at `1` until a real consumer exists;
- add comments documenting the channel contract close to generation code.

Acceptance:

- all existing masses regenerate;
- mesh colours remain valid for all vertices;
- current material still looks acceptable even before HLSL migration;
- river interaction and colliders are unchanged.

Rollback:

- return green/blue to `0.5` and alpha to `1`;
- leave archetype and material work untouched.

### Patch 3 - Add `LayeredStone`

Status: implemented provisionally as a closed-shell silhouette; physical shelves are deferred.

Checklist status:

- [x] append `LayeredStone` to `MassArchetype`;
- [x] add archetype defaults;
- [x] add dimensions in `GetBaseDimensions`;
- [x] add cut-depth and dimension constraints if needed;
- [x] implement a watertight stacked-ring silhouette;
- [x] defer physical shelf relief until proper inset/extrusion or mesh merging exists;
- [ ] validate at several seeds and size steps in Unity.

Primary files:

- `GeneratedMass.cs`
- `MassGenerator.cs`

Work:

- append `LayeredStone` to `MassArchetype`;
- add archetype defaults;
- add dimensions in `GetBaseDimensions`;
- add cut-depth and dimension constraints if needed;
- implement a watertight stacked-ring silhouette;
- defer physical shelf relief until proper inset/extrusion or mesh merging exists;
- validate at several seeds and size steps.

Acceptance:

- existing archetypes produce comparable shapes to before;
- `LayeredStone` reads as distinct at gameplay camera distance;
- output remains simple and stable.

Rollback:

- remove only the appended enum value and its switch branches before any scene depends on it.

### Patch 4 - Add `FracturedPillar`

Status: implemented provisionally.

Checklist status:

- [x] append `FracturedPillar` to `MassArchetype`;
- [x] add defaults and dimensions;
- [x] add a dominant cleave/diagonal top profile;
- [x] keep the result mostly convex and collision-safe;
- [ ] validate visual distinction from `StandingStone` in Unity.

Primary files:

- `GeneratedMass.cs`
- `MassGenerator.cs`

Work:

- append `FracturedPillar` to `MassArchetype`;
- add defaults and dimensions;
- add a dominant cleave/diagonal top profile;
- keep the result mostly convex and collision-safe.

Acceptance:

- visually distinct from `StandingStone`;
- does not overproduce tiny triangles;
- remains usable beside the river and as a landmark.

### Patch 5 - Add `CarvedMarkerStone`

Status: implemented provisionally as a closed-shell silhouette; physical carved relief is deferred.

Checklist status:

- [x] append `CarvedMarkerStone` to `MassArchetype`;
- [x] add defaults and dimensions;
- [x] build a single-shell marker silhouette around one broad presentation face;
- [x] defer raised face-bar geometry until it can be added without daylight gaps through the marker;
- [ ] validate readability across several seeds in Unity.

Primary files:

- `GeneratedMass.cs`
- `MassGenerator.cs`

Work:

- append `CarvedMarkerStone` to `MassArchetype`;
- add defaults and dimensions;
- build a single-shell marker silhouette around one broad presentation face;
- defer raised face-bar geometry until it can be added without daylight gaps through the marker.

Acceptance:

- produces a readable flat face across many seeds;
- remains a generated mass, not a bespoke shrine system;
- no carving dependency is introduced.

### Patch 6 - Inspector-Owned Base Colour

Status: implemented for the current Shader Graph path through `_BaseColor`.

Checklist status:

- [x] add a serialized base-colour field to `GeneratedMass`;
- [x] apply the selected colour through a `MaterialPropertyBlock`;
- [x] bind the colour to `_BaseColor`;
- [x] refresh the property block during enable, validation, regeneration, and inspector changes;
- [x] keep using the shared stone material.

Primary files:

- `GeneratedMass.cs`
- `GeneratedMassEditor.cs` only if the default inspector is not enough
- current and future stone shaders

Work:

- add a serialized base-colour field to `GeneratedMass`;
- apply the selected colour through a `MaterialPropertyBlock`;
- bind the colour to `_BaseColor`;
- refresh the property block during enable, validation, regeneration, and inspector changes;
- keep using the shared stone material.

Acceptance:

- multiple generated masses can share one material while showing different base colours;
- changing the colour on one object does not affect other objects;
- the selected colour survives shape and surface regeneration;
- later effects are still allowed to modify the final rendered colour.

### Patch 7 - HLSL Pixel Surface Baseline

Status: implemented provisionally with `SH_PixelSurfaceLit.shader` and a duplicate test material. Needs Unity import, compile, and visual comparison before switching the main stone material.

Checklist status:

- [x] create HLSL URP forward-lit shader;
- [x] preserve current material properties where practical;
- [x] include or port the current pixel hash logic;
- [ ] match current `M_PixelStone` before adding new effects;
- [x] create a temporary or duplicate stone material for visual comparison.

Primary files:

- new `SH_PixelSurfaceLit.shader`
- existing `PixelCellVariation.hlsl`
- duplicate or test material under `Assets/Game/Demo/Materials/Stone/`

Work:

- create HLSL URP forward-lit shader;
- preserve current material properties where practical;
- include or port the current pixel hash logic;
- match current `M_PixelStone` before adding new effects;
- create a temporary or duplicate stone material for visual comparison.

Acceptance:

- shader compiles;
- test material renders lit, shadowed, and stable;
- output is close to current Shader Graph material with extra effects disabled.

Rollback:

- switch material back to `SG_PixelSurfaceLit`;
- leave HLSL shader file unused until fixed.

### Patch 8 - HLSL Semantic Surface Response

Status: implemented and visually accepted for the current baseline. Further tuning can happen while creating material profiles.

Checklist status:

- [x] consume vertex colour `G` for exposure brightening;
- [x] consume vertex colour `B` for crevice/base darkening;
- [x] add low-frequency blotch variation;
- [x] add optional cell-position warp;
- [x] expose conservative controls;
- [x] guard semantic response so plain white vertex colours remain neutral;
- [x] replace hard broad-cell variation with smooth value noise after a visible axis-aligned strip appeared during Unity testing;
- [x] tune HLSL baseline lighting toward the midpoint between the original bright Shader Graph material and the darker first HLSL material at base colour `#555759`;
- [x] validate visual balance in Unity against the current Shader Graph stone material.

Primary files:

- `SH_PixelSurfaceLit.shader`
- stone material instance(s)

Work:

- consume vertex colour `G` for exposure brightening;
- consume vertex colour `B` for crevice/base darkening;
- add low-frequency blotch variation;
- add optional cell-position warp;
- expose conservative controls.

Acceptance:

- tops and broad flat planes read slightly lighter;
- bases and cut-heavy regions read slightly darker;
- pixel noise remains visible but less evenly distributed;
- no material setting breaks non-rock users of the shader.

### Patch 9 - Shader-Driven Material Profiles and Variants

Status: implemented provisionally with shader profile controls and four HLSL material variants. The HLSL shader now uses URP's PBR lighting path for Shader Graph parity instead of the earlier custom lighting pass. Needs Unity import, compile, and visual tuning.

Checklist status:

- [x] add material-profile controls to `SH_PixelSurfaceLit.shader`;
- [x] make profile controls affect surface behaviour, not only base colour;
- [x] create a small set of material instances after the HLSL shader is accepted;
- [x] keep all variants on the same shader;
- [x] tune palette, lighting, smoothness, pixel response, semantic response, and profile effects rather than importing textures;
- [x] test direct-light and shadowed-ambient controls; superseded by URP PBR parity after visual artifacts appeared;
- [x] replace the custom HLSL lighting pass with URP PBR evaluation to match the old Shader Graph material family;
- [x] keep the HLSL shader in URP specular workflow to match the old Shader Graph stone material;
- [x] keep flat-normal lighting control available but default it to `0`, because the old Shader Graph does not feed a custom normal into the Lit surface;
- [x] add an object-level stone surface profile dropdown on `GeneratedMass` that can select the HLSL profile material without manually replacing renderer materials;
- [ ] verify each variant reads differently at gameplay camera distance.

Primary files:

- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader`
- `Assets/Game/Demo/Materials/Stone/`

Created materials:

- `M_PixelStone_HLSL_ColdGrey`
- `M_PixelStone_HLSL_WetRiver`
- `M_PixelStone_HLSL_PaleFrost`
- `M_PixelStone_HLSL_BlackSacred`

Work:

- add shader controls for wetness, frost buildup, monolithic flattening, profile contrast, and pixel contrast;
- route those controls through albedo, smoothness/specular response, semantic masks, pixel variation, direct light, and shadowed ambient;
- create a small set of material instances after the HLSL shader is accepted;
- keep all variants on the same shader;
- tune palette and behavioural response rather than importing textures;
- keep per-object `GeneratedMass` base colour meaningful by treating material variants as response profiles, not just fixed colours.
- expose the accepted variants through `GeneratedMass.StoneSurfaceProfile`, with hidden material references auto-filled by the custom inspector from the existing HLSL material assets.

Suggested variants:

- cold grey stone;
- dark wet river stone;
- pale frost stone;
- black sacred/fractured stone.

Variant characteristics:

- `Cold Grey Stone`: the balanced default profile. It should stay close to the current accepted HLSL baseline, with moderate pixel variation, moderate semantic response, and neutral smoothness.
- `Dark Wet River Stone`: consistently darker across the whole surface, shinier, less crisp in pixel contrast, and less sharply crevice-carved. It should feel damp or slick rather than merely black.
- `Pale Frost Stone`: pale/cool exposed buildup, especially on upward or broad exposed faces; stronger dark crevices; drier/sharper surface response; subtle frost patterning from smooth broad noise.
- `Black Sacred Stone`: dark, monolithic, and controlled. It should reduce noisy colour range, flatten variation toward one strong tone, and optionally read smoother or more polished than ordinary fractured stone.

Inspector workflow:

- leave `Stone Surface Profile` on `Renderer Material` to preserve the current renderer assignment;
- choose `Cold Grey Stone`, `Dark Wet River Stone`, `Pale Frost Stone`, or `Black Sacred Stone` on the `GeneratedMass` component to apply the matching HLSL profile material;
- continue using `Base Color` for the object's starting tint; the selected profile controls how wetness, frost, smoothness, pixel contrast, semantic masks, and future authored masks alter that tint.

Interaction with the higher-quality material phase:

- convex edge wear should be strongest and cleanest on pale/frost and sacred profiles, moderate on cold grey, and softer/glossier on wet river stone;
- concave crease darkening should be strongest on pale frost, moderate on cold grey and sacred stone, and softened on wet river stone so damp rocks do not look sharply carved;
- dirty mottle and mineral deposits should be profile-aware: more visible on cold grey, cold/frost patterned on pale frost, lower and smoother on wet river stone, and restrained on black sacred stone;
- crack/seam language should share the same masks but use profile-specific contrast, with sacred stone favoring controlled dark lines and frost stone allowing brighter worn lips.

Acceptance:

- variants are meaningfully distinct at gameplay camera distance;
- variants differ through surface behaviour, not just colour;
- dark wet stone is shinier, more uniformly dark, and visually softer;
- pale frost stone has readable exposed frost buildup and darker crevices;
- black sacred stone is controlled, dark, and comparatively monolithic;
- existing default stone remains available.

### Patch 10 - Stylized Value Shaping

Status: implemented provisionally in the HLSL shader and HLSL stone materials. Needs Unity import, compile, and visual tuning against the old Shader Graph material.

Checklist status:

- [x] keep URP PBR lighting as the final lighting model;
- [x] add pre-lighting highlight compression so bright faces keep more pixel detail;
- [x] add object-space bottom darkening for subtle grounded rock weight;
- [x] add a broad lower-side edge darkening approximation;
- [x] expose conservative material controls for all three effects;
- [x] add tuned defaults to the baseline and four HLSL material variants;
- [x] add a `DepthNormals` pass so the HLSL rocks participate in URP SSAO/contact occlusion like the old Shader Graph material;
- [x] add the `_SCREEN_SPACE_OCCLUSION` forward-pass variant so HLSL rocks receive SSAO on their own surfaces;
- [ ] validate the defaults against the old Shader Graph stone material in Unity;
- [ ] decide whether broad edge darkening should later move to generated vertex colours for mesh-aware edge weighting.

Primary files:

- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader`
- `Assets/Game/Demo/Materials/Stone/`

Work:

- keep stylized value shaping in albedo response before `UniversalFragmentPBR`;
- restore camera-normal output for SSAO before further tuning, because the old Shader Graph material received a generated depth-normals pass automatically;
- compile the screen-space occlusion forward variant so `UniversalFragmentPBR` can consume SSAO on the rock mesh itself;
- avoid weakening global shadows, because that would affect the whole lit/shadow relationship rather than the rock surface response;
- use `_HighlightCompressStrength` and `_HighlightCompressStart` to slightly darken only strongly lit faces;
- use `_BottomDarkenStrength` and `_BottomDarkenHeight` to recreate a softer version of the old material's lower darkening;
- use `_EdgeDarkenStrength` and `_EdgeDarkenPower` to darken broad lower side faces as a shader-side approximation of edge weight.

Acceptance:

- bright/top faces retain visible pixel and broad-noise detail;
- lower rock areas gain subtle grounded darkening without looking dirty or painted-on;
- shadowed faces remain readable and are not made globally flat;
- the HLSL baseline moves closer to the old Shader Graph material without reintroducing the earlier streak artifacts.

## Higher-Quality Rock Material Phase

The next quality jump should come from authored procedural masks, not from trying to infer everything in the shader from world position and normals. The reference rocks gain most of their richness from worn convex ridges, dark recessed cracks, dirty nonuniform surface breakup, and face-aware deposits.

Proposed extended material contract:

- `Color.r`: deterministic surface variation, already implemented;
- `Color.g`: exposed/upward wear and frost buildup, already implemented;
- `Color.b`: base, sheltered side, crevice, and contact darkening, already implemented;
- `Color.a`: convex edge wear or authored ridge intensity;
- `UV2.x`: concave crease or selected crack-darkening mask;
- `UV2.y`: dirty deposit / mineral stain mask;
- `UV2.zw`: reserved for future biome-specific material state.

If `UV2` support is too large for the first pass, use `Color.a` for convex edge wear first and leave concave cracks to a later patch. Do not overload the current red variation channel; it already drives the accepted pixel/noise look.

### Patch 11 - Mesh-Authored Edge and Crease Mask Contract

Status: implemented provisionally. The mesh now emits the extended mask channels, and the HLSL shader can visualize them through the per-object `Surface Mask Debug` control. Concave crease/crack remains neutral until Patch 12.

Checklist status:

- [x] inspect generated mesh data flow and confirm whether `UV2` can be emitted safely by `MeshBuilder`;
- [x] decide final channel ownership for convex edge wear, concave crease, and dirt masks;
- [x] preserve current `Color.r/g/b` semantics;
- [x] write neutral defaults for meshes/materials without the new channels;
- [x] document the channel contract near generation and shader code;
- [x] add a debug material/control path if needed to visualize masks in Unity;
- [ ] validate the debug masks in Unity across representative archetypes and seeds.

Primary files:

- `Assets/Game/Procedural/Masses/MassGenerator.cs`
- `Assets/Game/Procedural/Masses/GeneratedMass.cs`
- `Assets/Game/Procedural/Core/MeshData.cs`
- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader`
- `Assets/Game/Procedural/Core/MeshBuilder.cs`

Work:

- introduce a stable contract for mesh-authored material masks;
- extend `MeshData.AddVertex` so generated meshes can emit UV2 masks safely;
- write a first-pass convex ridge candidate into `Color.a` by comparing face normals against the shared-position average normal;
- write a lower/sheltered dirt-deposit candidate into `UV2.y`;
- keep `UV2.x` neutral for Patch 12's real concave crease/crack work;
- expose `GeneratedMass.SurfaceMaskDebug` so individual rocks can visualize surface variation, exposure, crevice/base, convex edge wear, concave crease, or dirt deposit masks;
- avoid shader-only fake edge detection as the main solution;
- keep all new masks optional and neutral so existing generated meshes remain valid.

Acceptance:

- existing rocks still render with the current accepted look when new masks are neutral;
- the shader can read at least one new authored mask without compile/runtime errors;
- masks can be visually inspected or temporarily exaggerated for tuning.

Patch 11 Unity validation notes from mask screenshots:

- `SurfaceVariation` and `Exposure` validated as useful and spatially readable.
- `CreviceBase` was functional but too broad; it should remain a broad base/contact/side-darkening mask rather than being repurposed as crack data.
- First-pass `ConvexEdgeWear` proved the channel plumbing worked, but the mask was too face-wide and triangular/blotchy to drive visible ridge highlights directly.
- `ConcaveCrease` correctly remained neutral before Patch 12, but that means it cannot yet support reference-style dark seams.
- `DirtDeposit` was usable as a lower/sheltered baseline, but it needed less uniform gradient behaviour before profile response is added.

Decision: do not wire the first-pass convex mask into final shader response. First correct the generated masks, then validate debug views again, then add visible profile-specific edge/crease response.

### Patch 12A - Surface Mask Authoring Correction

Status: implemented, pending Unity import and mask-debug validation.

Checklist status:

- [x] keep `SurfaceVariation` and `Exposure` unchanged because validation showed they are already useful;
- [x] tighten `CreviceBase` so it behaves more like broad base/contact/low-side darkening instead of a mostly uniform pale mask;
- [x] replace the first-pass convex normal-average mask with topology/edge-based ridge candidates;
- [x] populate `UV2.x` with selected concave/fracture seam candidates instead of leaving it neutral;
- [x] add a modest authored boost into `UV2.y` so dirt/deposit remains lower/sheltered but has less uniform gradient behaviour;
- [x] keep visible final material response deferred until the corrected debug masks are validated in Unity.

Primary files:

- `MassGenerator.cs`
- `Rock_Generated_Mass_Upgrade_Plan.md`

Work:

- build an edge lookup from generated triangle topology;
- compare neighbouring face normals and face-centre orientation to classify convex ridge candidates separately from concave crease candidates;
- suppress tiny unreadable edges so incidental triangulation does not dominate the mask;
- select sparse fracture/seam candidates from readable edges using deterministic seed-driven selection, with more allowance on broken/fractured/chipped archetypes and less on polished stones;
- write corrected masks through the existing material contract: `Color.a` for convex wear, `UV2.x` for concave/selected seams, and `UV2.y` for dirt/deposit;
- avoid adding physical shelves, carved gaps, holes, or new mesh-relief geometry in this patch.

Acceptance:

- `ConvexEdgeWear` debug should become more ridge/edge-biased and less broad triangular face soup;
- `ConcaveCrease` debug should no longer be empty and should show sparse readable seam/crack candidates;
- `DirtDeposit` debug should still prefer lower/sheltered regions but should be less like a simple side gradient;
- `CreviceBase` should remain a broad base/contact darkening mask;
- normal rendering should remain close to the accepted Patch 10/11 look because the shader has not yet been made to strongly consume the new masks.

### Patch 12B - Profile-Aware Convex Edge and Concave Crease Material Response

Status: planned after Patch 12A validation.

Checklist status:

- [ ] brighten convex ridges with a controllable worn-edge colour and strength;
- [ ] darken concave creases with a separate controllable strength;
- [ ] make controls profile-aware so frost, wet, sacred, and cold-grey stones respond differently;
- [ ] keep ridge highlighting subtle enough to preserve the simple low-poly style;
- [ ] validate on squat boulders, slabs, standing stones, fractured pillars, and broken chunks.

Primary files:

- `SH_PixelSurfaceLit.shader`
- HLSL stone materials
- optionally `MassGenerator.cs` only if mask tuning is still required

Work:

- prefer light worn ridges over blanket edge darkening;
- separate convex edge wear from concave crack darkening;
- preserve the original simple blocky low-poly shape language;
- make the material profiles use the same masks differently rather than generating unrelated profile-specific masks.

Acceptance:

- major silhouette and facet edges read as intentionally worn or polished;
- inward seams/cracks read darker without looking like holes;
- the effect remains visible at gameplay camera distance;
- it does not require adding complex geometry to every rock.

### Patch 13 - Dirty Surface Mottle and Material Breakup

Status: planned.

Checklist status:

- [ ] add a smooth, irregular grime/mineral stain layer separate from square pixel variation;
- [ ] bias dirt toward lower, sheltered, or less exposed areas;
- [ ] add profile controls for dirty, wet, frost, sacred, and default stones;
- [ ] preserve the current pixel-like texture as the base style;
- [ ] avoid making the rocks look noisy, photographic, or too high-frequency.

Primary files:

- `SH_PixelSurfaceLit.shader`
- HLSL stone materials
- optionally `MassGenerator.cs` if dirt masks become mesh-authored

Work:

- layer broad cloudy variation, fine speckle, and subtle colour temperature shifts;
- treat dirt as material response, not a replacement for the base colour;
- support future biomes by keeping the controls generic.

Acceptance:

- rock faces no longer look uniformly flat or purely square-noise based;
- dirt/mottle helps faces read as stone without overwhelming the blocky geometry;
- material variants become more distinct without requiring new textures.

### Patch 14 - Crack and Seam Language

Status: planned.

Checklist status:

- [ ] choose whether cracks are mesh-authored masks, shader-procedural lines, or special archetype features;
- [ ] add thin dark crack response with optional bright worn lip;
- [ ] keep cracks sparse and large enough to read at gameplay distance;
- [ ] avoid actual holes or non-watertight geometry unless a later mesh system supports it;
- [ ] validate that cracks do not fight with SSAO, edge wear, or pixel variation.

Primary files:

- `MassGenerator.cs`
- `SH_PixelSurfaceLit.shader`
- possibly future archetype-specific helpers

Work:

- introduce controlled crack/seam language similar to the reference assets;
- use visual cracks before physical carved geometry;
- reserve complex carved/relief geometry for a later mesh-generation pass.

Acceptance:

- cracks make selected rocks feel authored rather than random;
- cracks are sparse, readable, and stable across seeds;
- no daylight gaps or open mesh faces are introduced.

## Validation Matrix

Each implementation patch should inspect:

- small `XS` or `S` rock;
- medium terrain boulder;
- large or monumental boulder;
- standing stone;
- slab;
- broken chunk;
- polished stone;
- each new archetype;
- at least one rock touching or near the river.

For each inspected rock:

- regenerate shape seed;
- regenerate surface seed;
- check collider assignment;
- check scene lighting response;
- check material under shadow and direct light;
- check silhouette from the intended elevated camera angle.

## Risk Notes

### Serialized enum risk

Do not insert new `MassArchetype` values between existing entries. Append only.

### Shader migration risk

Do not migrate all materials at once. First add the HLSL shader and one duplicate material, validate, then switch the primary stone material.

### Visual overcomplexity risk

The river became more complex than the rocks by design pressure. Rocks should not follow that path unless there is a strong gameplay or art-direction reason. The strongest rock improvements are semantic material response, a few better archetypes, and later cluster composition.

### Collision risk

Keep single generated masses mostly convex in this phase. Real cracks and shadow gaps should come later from clustered masses rather than concave single meshes.

## Deferred Follow-Up

After the above is accepted, consider a `GeneratedMassCluster` authoring component:

- places 2-3 existing generated masses deterministically;
- creates cairns, split rocks, debris groups, and natural crevice shadows;
- leaves each child mass simple and collision-stable;
- can use the same material variants and vertex colour contract.

This should be a separate system after the single-mass upgrade is proven.
