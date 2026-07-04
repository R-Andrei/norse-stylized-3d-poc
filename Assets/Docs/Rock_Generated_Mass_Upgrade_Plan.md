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
- [x] Patch 12B - Semantic surface mask rewrite
- [x] Patch 12C - Area mask recovery and line-mask deferral
- [x] Patch 12C.2 - Area mask hard clamp after Unity validation
- [x] Patch 12D - Shader-space area mask debug
- [x] Patch 12E - Irregular area mask shaping for `CreviceBase` and `DirtDeposit`
- [x] Patch 12F - Contact/crawl area masks for `CreviceBase` and `DirtDeposit`
- [x] Patch 12F.2 - Contact/crawl area mask tuning after first Unity validation
- [x] Patch 12F.3 - Contact/crawl area mask second tuning after Unity validation
- [x] Patch 12F.4 - Contact/crawl structural correction with deposit skeleton
- [ ] Patch 12G - Edge/crack representation decision and prototype
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
- `UV2.z`: selected convex edge localization band;
- `UV2.w`: selected concave crease localization band.

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

Status: Patch 12A.1 produced poor Unity validation and was corrected by Patch 12A.2.

Checklist status:

- [x] keep `SurfaceVariation` and `Exposure` unchanged because validation showed they are already useful;
- [x] tighten `CreviceBase` so it behaves more like broad base/contact/low-side darkening instead of a mostly uniform pale mask;
- [x] replace the first-pass convex normal-average mask with topology/edge-based ridge candidates;
- [x] populate `UV2.x` with selected concave/fracture seam candidates instead of leaving it neutral;
- [x] add a modest authored boost into `UV2.y` so dirt/deposit remains lower/sheltered but has less uniform gradient behaviour;
- [x] keep visible final material response deferred until the corrected debug masks are validated in Unity.

Primary files:

- `MassGenerator.cs`
- `SH_PixelSurfaceLit.shader`
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

Patch 12A.1 Unity validation notes:

- `CreviceBase` still read as too flat/pale and did not clearly communicate base/contact darkening on the tested boulder.
- `ConvexEdgeWear` became an almost uniform mid-value instead of readable ridge/edge data.
- `ConcaveCrease` still did not produce useful sparse seams.
- `DirtDeposit` remained mostly a broad side/base gradient.
- Most importantly, the shader was reading the generated mask vector from `TEXCOORD1`, while `MeshBuilder` writes its `UV2` list to Unity UV channel 2 / `TEXCOORD2`. This meant the new `UV2.x/y` mask debug modes were not reading the generated data reliably.

Patch 12A.2 correction:

- keep `MeshBuilder` unchanged to avoid disturbing river/ground systems that already use Unity channel 2;
- make `SH_PixelSurfaceLit.shader` read generated material masks from `TEXCOORD2`;
- write triangle-local barycentric coordinates into `UV2.zw` for generated masses;
- use the barycentric helper only in mask debug for `ConvexEdgeWear` and `ConcaveCrease`, so these modes display line/edge-localized candidates instead of whole-face fills;
- loosen and simplify the edge candidate calculation so readable sharp edges on mostly convex generated shells produce useful wear data;
- keep `CreviceBase` and `DirtDeposit` tighter and lower/base-biased;
- still defer final visible material response until this corrected debug pass is validated.

Patch 12A.2 Unity validation notes:

- `CreviceBase` still read as a flat pale wash, so it was not yet a useful base/contact mask.
- `ConvexEdgeWear` and `ConcaveCrease` exposed the core conceptual failure: the barycentric helper drew raw triangle topology. This produced wireframe/fan patterns instead of selected rock ridges or sparse cracks.
- `DirtDeposit` behaved like generic faceted mottle, duplicating `SurfaceVariation` instead of collecting near lower/sheltered/base regions.
- Decision: stop using generic triangle-edge barycentric debug for semantic masks. The generator must choose which edges are actual rock features, then write separate localization bands for those selected features only.

### Patch 12B - Semantic Surface Mask Rewrite

Status: implemented, pending Unity validation.

Checklist status:

- [x] remove generic barycentric triangle-edge debug from `ConvexEdgeWear` and `ConcaveCrease`;
- [x] keep the corrected Unity UV channel path from Patch 12A.2;
- [x] change `UV2.z`/`UV2.w` from raw triangle barycentric helper data into selected semantic edge localization bands;
- [x] rewrite `CreviceBase` as a lower/base/shelter area mask with less global pale baseline;
- [x] rewrite `DirtDeposit` as low/base/shelter-driven buildup with deterministic position-patch breakup instead of per-triangle mottle;
- [x] select a limited feature-edge budget for `ConvexEdgeWear` so it cannot show the full mesh triangulation;
- [x] select a smaller sparse seam/crack edge budget for `ConcaveCrease`;
- [x] keep final visible material response deferred until the debug views prove the masks are spatially correct.

Primary files:

- `MassGenerator.cs`
- `SH_PixelSurfaceLit.shader`
- `Rock_Generated_Mass_Upgrade_Plan.md`

Work:

- generate edge candidates from topology, but do not automatically treat every triangle boundary as a material feature;
- score candidate convex wear using readable edge length, face-normal difference, height/exposure, base suppression, and deterministic breakup;
- sort and budget convex candidates by archetype, form complexity, and edge character so only the strongest selected ridges receive wear data;
- score concave/crease candidates separately with side-face, mid-height, vertical/diagonal orientation, and sparse deterministic selection;
- avoid selecting concave seams that are merely the same edge already chosen as ordinary convex wear unless their crease score dominates;
- write selected convex edge localization to `UV2.z` and selected concave crease localization to `UV2.w`;
- debug modes multiply strength by the selected semantic localization band, not by all triangle edges;
- compute `DirtDeposit` mostly from low vertical position, side shelter, crevice/base support, and deterministic position patches.

Acceptance:

- `CreviceBase` should show a clear lower/base/contact bias, not a uniform pale wash;
- `ConvexEdgeWear` should show selected meaningful ridges/corners, not wireframe/fan triangulation;
- `ConcaveCrease` should show sparse selected seam/crack candidates, not every triangle edge and not a full blank result on all archetypes;
- `DirtDeposit` should visibly prefer lower/sheltered/base areas, with patch breakup, and should not duplicate general faceted `SurfaceVariation`;
- normal rendering should remain close to the current look because profile-specific visible response is still not enabled.

Patch 12B Unity validation notes:

- `CreviceBase` still read as a nearly uniform pale area, not as lower/contact/sheltered broad grounding.
- `ConvexEdgeWear` no longer exposed every triangle edge, but it produced large triangular wedge fills on a few selected faces. This still failed because edge wear is a line-like feature and cannot be represented cleanly by a coarse interpolated scalar mask.
- `ConcaveCrease` failed for the same reason: it produced wedge-shaped face regions rather than sparse crack/seam paths. The tested simple convex shell also has little true concavity, so fake cracks must not be inferred from arbitrary triangle edges.
- `DirtDeposit` was less broken than the line masks, but still read mostly as mild faceted variation instead of bottom-biased buildup.

Decision: stop patching `ConvexEdgeWear` and `ConcaveCrease` as ordinary scalar area masks. Broad area masks can remain in vertex colour / UV2 scalar channels, but line-like features need a later dedicated representation: generated overlay strips, actual mesh-authored relief, or per-edge/per-triangle metadata that can draw a narrow selected edge without flooding whole triangles.

### Patch 12C - Area Mask Recovery and Line-Mask Deferral

Status: implemented, pending Unity validation.

Checklist status:

- [x] keep `SurfaceVariation` and `Exposure` unchanged because they already validated well;
- [x] rewrite `CreviceBase` as a broad lower/contact/shelter area mask with a much darker default body value;
- [x] rewrite `DirtDeposit` as environmental lower/base buildup with broad patch breakup, not generic faceted mottle;
- [x] intentionally neutralize `ConvexEdgeWear`, `ConcaveCrease`, and their localization channels for now;
- [x] document that scalar/interpolated masks are not suitable for narrow edge/crack features on these coarse generated rocks;
- [x] keep final visible material response deferred until area masks are acceptable and the line-feature representation is chosen.

Primary files:

- `MassGenerator.cs`
- `SH_PixelSurfaceLit.shader`
- `Rock_Generated_Mass_Upgrade_Plan.md`

Work:

- remove active output for `Color.a`, `UV2.x`, `UV2.z`, and `UV2.w` from generated masses so the bad wedge-like debug views do not get mistaken for valid data;
- keep the shader debug modes available, but expect `ConvexEdgeWear` and `ConcaveCrease` to be neutral/black after regeneration;
- make `CreviceBase` depend mostly on low object height, side/downward orientation, and lower sheltered surfaces;
- make `DirtDeposit` depend mostly on low object height, side shelter, broad position-based patching, and crevice/base support;
- avoid touching shader/material profile response until the masks are semantically correct.

Acceptance:

- `CreviceBase` should no longer be a uniform pale wash; it should be strongest near lower/base/contact/sheltered regions and weak on most upper/mid exposed faces;
- `DirtDeposit` should visibly prefer the lower rim and lower sheltered sides, with some patchy upward crawl;
- `ConvexEdgeWear` and `ConcaveCrease` should be neutral/black for now, because their previous non-neutral output was invalid;
- normal rendering should remain close to the current accepted look because the shader still does not consume the deferred line masks.


Patch 12C Unity validation notes:

- `ConvexEdgeWear` and `ConcaveCrease` correctly became neutral/black after regeneration. This validates the line-mask deferral decision: the invalid wedge/triangulation outputs are no longer being presented as usable mask data.
- `CreviceBase` still failed. It remained a mostly uniform pale wash across the tested boulder instead of concentrating near the lower/base/contact/sheltered region.
- `DirtDeposit` also failed. It was slightly different from `CreviceBase`, but still read as broad pale body coverage rather than lower-rim / lower-side buildup.
- Diagnosis: the line masks are now safely deferred, but the area formulas still carried too much usable value through the upper/mid body. The next correction should hard-clamp both area masks toward the bottom/lower sheltered portion before any visible material response is enabled.

### Patch 12C.2 - Area Mask Hard Clamp

Status: implemented, pending Unity validation.

Checklist status:

- [x] keep `ConvexEdgeWear` and `ConcaveCrease` neutral/black;
- [x] remove unused semantic edge lookup construction from the active mesh generation path while line masks are deferred;
- [x] make `CreviceBase` much stricter, with upper/mid body suppression and a narrow base/contact emphasis;
- [x] make `DirtDeposit` much stricter, with lower-rim buildup, limited patchy upward crawl, and strong exposed upper-face suppression;
- [x] keep shader/material visible response unchanged.

Primary files:

- `MassGenerator.cs`
- `Rock_Generated_Mass_Upgrade_Plan.md`

Acceptance:

- `CreviceBase` debug should become mostly dark through the upper/mid visible body and clearly stronger around the base/lower sheltered region.
- `DirtDeposit` debug should be mostly dark on exposed upper/mid faces, with lower-rim/lower-side patches.
- `ConvexEdgeWear` and `ConcaveCrease` should remain black/neutral until a proper line representation is chosen.

### Patch 12D - Shader-Space Area Mask Debug

Status: implemented after Patch 12C.2 validation showed that baked vertex/interpolated area masks still smeared too much across large low-poly triangles.

Reason for patch:

- `ConvexEdgeWear` and `ConcaveCrease` were already correctly deferred/neutralized after the failed line-mask attempts.
- `CreviceBase` and `DirtDeposit` were conceptually area masks, but storing tight lower/base features as coarse vertex data still caused whole-face interpolation and pale global washes.
- The representation problem was smaller than the line-mask problem, but still real: a large triangular side face cannot hold a tight base band accurately if only its vertices carry the mask.

Checklist status:

- [x] add hidden per-object generated mass shader properties for local minimum Y, local height, and mask seed;
- [x] set those properties from `GeneratedMass` through the existing `MaterialPropertyBlock`;
- [x] compute `CreviceBase` in the shader from object-space height and local normal rather than from the baked vertex colour blue channel;
- [x] compute `DirtDeposit` in the shader from object-space lower-rim/side logic plus deterministic object-space patch noise;
- [x] keep `ConvexEdgeWear` and `ConcaveCrease` black/neutral until a proper line representation is chosen;
- [x] keep the generated/baked channels intact as broad support data, but stop relying on them for tight area debug.

Primary files:

- `GeneratedMass.cs`
- `SH_PixelSurfaceLit.shader`
- `Rock_Generated_Mass_Upgrade_Plan.md`

Work:

- `GeneratedMass` now sends `_GeneratedMassLocalMinY`, `_GeneratedMassLocalHeight`, and `_GeneratedMassMaskSeed` to the material property block.
- `SH_PixelSurfaceLit.shader` now uses object-space Y normalized by the generated mesh bounds to calculate per-pixel lower/base masks.
- `CreviceBase` debug now displays the shader-space lower/contact/shelter calculation.
- `DirtDeposit` debug now displays a shader-space lower-rim buildup with patchy upward crawl.
- Normal semantic crevice/base darkening now uses the shader-space crevice/base mask, so broad over-darkening from bad vertex data should be reduced.

Acceptance:

- `CreviceBase` should be mostly dark on upper/mid body faces and visibly stronger near the base/lower sheltered sides.
- `DirtDeposit` should be mostly dark on exposed upper/mid faces, with lower-rim/lower-side buildup and some irregular upward patches.
- `ConvexEdgeWear` and `ConcaveCrease` should remain black/neutral.
- No final profile-aware edge/crack response is added yet.

### Patch 12E - Irregular Area Mask Shaping

Status: implemented after Patch 12D validation showed that shader-space area masks had the correct broad location but still looked too much like clean horizontal height bands.

Reason for patch:

- `SurfaceVariation` and `Exposure` were accepted as good enough support masks.
- `ConvexEdgeWear` and `ConcaveCrease` remain deliberately black/neutral until a proper line-feature representation exists.
- `CreviceBase` and `DirtDeposit` were technically working after Patch 12D, but their visual debug output was too straight, two-tone, and height-band driven.
- The desired reference direction needs lower/contact/shelter masks that are shaped by rock planes and irregular buildup, not a smooth stripe around the base.

Checklist status:

- [x] keep area masks shader-space instead of returning to coarse vertex/interpolated data;
- [x] add hidden `_GeneratedMassLocalXZScale` so object-space noise scales more consistently across differently proportioned rocks;
- [x] make `CreviceBase` combine a narrow contact component, wider lower-side shelter, normal/side weighting, and warped thresholds;
- [x] make `DirtDeposit` use lower/base potential as a gate for positive patch buildup rather than displaying a continuous bright lower band;
- [x] keep `ConvexEdgeWear` and `ConcaveCrease` neutral;
- [x] document that these are still debug masks and require Unity validation before material response.

Primary files:

- `GeneratedMass.cs`
- `SH_PixelSurfaceLit.shader`
- `Rock_Generated_Mass_Upgrade_Plan.md`

Work:

- `GeneratedMass` now sends `_GeneratedMassLocalXZScale` through the existing material property block alongside local min Y, height, and seed.
- `SH_PixelSurfaceLit.shader` now normalizes mask-noise coordinates by local XZ size and local height instead of height alone.
- `CreviceBase` now uses threshold warping, side/shelter weighting, a narrow contact core, and a wider softer lower-side component.
- `DirtDeposit` now treats lower/base/shelter as an allowed region and uses deterministic patch coverage to create positive deposit patches with irregular upward crawl.
- The debug target is no longer a perfectly clean bright band; it should contain more midtones and a less-straight upper boundary.

Acceptance:

- `CreviceBase` upper/mid body should remain mostly dark, with a less-straight lower/base transition and visible midtones on lower sheltered sides.
- `CreviceBase` should not turn into moss/dirt blobs; it should remain broad grounding/shelter information.
- `DirtDeposit` should be mostly dark on exposed upper/mid faces, with positive irregular patches near the lower rim and some uneven upward crawl.
- `DirtDeposit` should not look like a continuous bright base stripe with holes cut out of it.
- `ConvexEdgeWear` and `ConcaveCrease` should remain black/neutral.
- If Unity validation still shows straight bands, the next adjustment should target the shader-space noise/threshold shaping, not return to baked vertex masks.

### Patch 12F - Contact/Crawl Area Masks

Status: implemented as a shader-only correction after Patch 12E validation showed that `CreviceBase` and `DirtDeposit` were still technically working but not yet decent.

Reason for patch:

- `SurfaceVariation` and `Exposure` remain good enough and should not be touched during this patch.
- `ConvexEdgeWear` and `ConcaveCrease` remain black/neutral until a real line-feature representation is designed.
- Patch 12E still used warped height thresholds as the spine of both area masks, so it could only move or soften the lower band.
- The next correction needed to change the model: `CreviceBase` should be contact plus lower sheltered side planes with an irregular boundary, while `DirtDeposit` should be broken base rim plus base-connected upward crawl.

Checklist status:

- [x] keep the existing hidden generated-mass shader inputs: local min Y, local height, local XZ scale, and mask seed;
- [x] keep the patch shader-only, with no C# or mesh-generation changes;
- [x] leave `SurfaceVariation` and `Exposure` logic unchanged;
- [x] leave `ConvexEdgeWear` and `ConcaveCrease` debug output black/neutral;
- [x] rewrite `CreviceBase` as contact core plus lower-side shelter with an object-size/tallness-aware irregular boundary;
- [x] rewrite `DirtDeposit` as broken contact rim plus base-connected crawl patches using multi-scale deterministic noise;
- [x] document that no material-profile response should be added until Unity validation accepts the debug masks.

Primary files:

- `SH_PixelSurfaceLit.shader`
- `Rock_Generated_Mass_Upgrade_Plan.md`

Work:

- Added helper functions for generated-mass tallness and size factors so object height can influence how far the base/shelter effect may rise without making the whole rock bright.
- `CreviceBase` now computes a narrow contact core, an irregular sine/noise boundary in normalized object-space XZ, side/downward/not-upward shelter weighting, and subtle broad/facet breakup. Height still gates the mask, but it is no longer the entire model.
- `DirtDeposit` now computes a low-frequency crawl-height field per normalized object-space XZ area. Deposit is allowed below that local crawl frontier, then medium/high noise breaks the coverage so patches remain base-connected instead of becoming detached blobs.
- `DirtDeposit` also keeps a broken contact rim, but suppresses exposed/upward regions through shelter weighting.
- No material-profile colours, dirt tinting, moss, frost, wetness response, edge wear, or crack response were added. This remains debug-mask work only.

Acceptance:

- `SurfaceVariation` should remain unchanged: broad/faceted stone variation.
- `Exposure` should remain unchanged: top/upward surfaces bright.
- `CreviceBase` should show strongest values at contact/base, medium values on lower sheltered side planes, and an irregular non-horizontal upper boundary.
- `CreviceBase` should not look like dirt, moss, isolated blobs, or a clean horizontal airbrushed band.
- `DirtDeposit` should show broken lower-rim deposits and uneven upward crawl that remains visibly connected to the base/lower area.
- `DirtDeposit` should not show detached round airbrush blobs, a continuous bright stripe, or full-body wash.
- `ConvexEdgeWear` and `ConcaveCrease` should remain black/neutral.
- If Unity validation still fails, the next correction should tune the contact/crawl model itself, not return these masks to baked vertex interpolation and not proceed to material response.

### Patch 12F.2 - Contact/Crawl Area Mask Tuning

Status: implemented as a follow-up shader-only tuning pass after Unity validation of Patch 12F showed that the model change was correct but the balance was still wrong.

Reason for patch:

- `CreviceBase` was still reading as a broad lower belt. The upper boundary was no longer perfectly straight, but the lower-side shelter component remained too generous and too soft.
- `DirtDeposit` was too starved. Instead of showing base-connected crawl, it only produced a few small chips near the bottom.
- The next step was therefore not a new system and not material response. It was a narrow tuning correction to the same shader-space contact/crawl model.

Checklist status:

- [x] keep the patch shader-only, with no C# or mesh-generation changes;
- [x] keep `SurfaceVariation` and `Exposure` unchanged;
- [x] keep `ConvexEdgeWear` and `ConcaveCrease` black/neutral;
- [x] tune `CreviceBase` to reduce continuous lower-band coverage and strengthen irregular lower-side shaping;
- [x] tune `DirtDeposit` to increase broken lower-rim presence and make the upward crawl field more visible while remaining base-connected;
- [x] document that this is still debug-mask work only, not material-profile response.

Primary files:

- `SH_PixelSurfaceLit.shader`
- `Rock_Generated_Mass_Upgrade_Plan.md`

Work:

- `CreviceBase` now uses a lower average rise, a sharper boundary feather, stronger shelter gating, reduced additive side-mid fill, and slightly stronger irregular coverage breakup. This is intended to stop the debug view from reading as one continuous pale belt around the rock.
- `CreviceBase` contact remains present, but the mask should now rely less on broad lower-face fill and more on uneven sheltered lower planes.
- `DirtDeposit` now raises the local crawl-height range, lowers the internal patch threshold, strengthens the broken contact rim, and removes the overly aggressive fade-in that was starving the crawl field right above the base.
- `DirtDeposit` still remains governed by base-connected crawl plus shelter weighting, so the goal is more readable crawl, not a continuous bright stripe.

Acceptance:

- `CreviceBase` should no longer look like a mostly continuous soft lower band. It should break more unevenly and stay strongest at the immediate contact/base zone.
- `DirtDeposit` should read more clearly as broken base deposits with visible upward crawl, rather than only a few tiny pale fragments.
- `SurfaceVariation` and `Exposure` should remain unchanged.
- `ConvexEdgeWear` and `ConcaveCrease` should remain black/neutral.
- If Unity validation still fails after this tuning pass, the next conversation should reassess the contact/crawl construction again rather than proceeding to material response.

### Patch 12F.3 - Contact/Crawl Area Mask Second Tuning

Status: implemented as a second shader-only tuning pass after Unity validation of Patch 12F.2.

Reason for patch:

- `CreviceBase` improved but still read as a mostly continuous pale belt around the lower rock perimeter.
- `DirtDeposit` became model-correct and base-connected, but the visible deposits were too large, heavy, and blobby.
- The fix is still tuning, not a new representation and not material response.

Checklist status:

- [x] keep the patch shader-only, with no C# or mesh-generation changes;
- [x] keep `SurfaceVariation` and `Exposure` unchanged;
- [x] keep `ConvexEdgeWear` and `ConcaveCrease` black/neutral;
- [x] reduce `CreviceBase` wraparound continuity by lowering broad lower-side fill and adding stronger interruption;
- [x] preserve `DirtDeposit` base-connected crawl while reducing blob mass and increasing internal breakup;
- [x] keep this as debug-mask work only.

Primary files:

- `SH_PixelSurfaceLit.shader`
- `Rock_Generated_Mass_Upgrade_Plan.md`

Work:

- `CreviceBase` now has a slightly lower rise, sharper boundary feather, stronger shelter gate, lower side-shelter weight, reduced side-mid fill, and a stronger interruption field. The intended result is less continuous belt behavior and more uneven lower sheltered regions.
- `DirtDeposit` now has a slightly lower crawl-height range, tighter patch threshold, more high-frequency breakup contribution, lower rim/crawl intensity, and stronger upper suppression. The intended result is to keep the successful base-connected crawl behavior while making the deposit shapes less massive.
- No material-profile response, edge wear, crack response, geometry relief, or C# plumbing changes were added.

Acceptance:

- `CreviceBase` should still be strongest near the base/contact zone, but should no longer form a clean wraparound pale band.
- `DirtDeposit` should remain readable as base-connected upward crawl, but with smaller and more broken visible patches than Patch 12F.2.
- `SurfaceVariation` and `Exposure` should remain unchanged.
- `ConvexEdgeWear` and `ConcaveCrease` should remain black/neutral.
- Do not proceed to visible material response until these debug masks are accepted.

### Patch 12F.4 - Contact/Crawl Structural Correction With Deposit Skeleton

Status: implemented as a shader-only structural correction after Unity validation of Patch 12F.3.

Reason for patch:

- `CreviceBase` was thinner than earlier versions, but still visually dominated by a smooth lower height belt.
- `DirtDeposit` had oscillated between two bad states: Patch 12F.2 was too chunky and blobby, while Patch 12F.3 became too sparse and chippy.
- This indicated that `DirtDeposit` needed a clearer internal crawl skeleton instead of making the same area-noise field decide coverage, breakup, mass, and upward direction all at once.

Checklist status:

- [x] keep the patch shader-only, with no C# or mesh-generation changes;
- [x] keep `SurfaceVariation` and `Exposure` unchanged;
- [x] keep `ConvexEdgeWear` and `ConcaveCrease` black/neutral;
- [x] make `CreviceBase` less dominated by smooth height by lowering broad fill and strengthening interruption;
- [x] add a deterministic crawl-skeleton field to `DirtDeposit`;
- [x] keep `DirtDeposit` base-connected while using erosion and fine breakup to avoid both huge blobs and tiny isolated chips;
- [x] keep this as debug-mask work only.

Primary files:

- `SH_PixelSurfaceLit.shader`
- `Rock_Generated_Mass_Upgrade_Plan.md`

Work:

- `CreviceBase` received another controlled reduction in broad lower fill: lower rise, sharper feather, stronger shelter gate, stronger patch interruption, lower lower-side weight, and stronger upper suppression.
- `DirtDeposit` now separates responsibilities inside the mask:
  - a broken base rim anchors deposits at contact;
  - a deterministic object-space crawl skeleton creates upward paths;
  - a crawl-height field keeps those paths base-connected;
  - erosion and high-frequency breakup reduce mass without starving the whole mask.
- The goal is to stop the previous oscillation between large blobs and tiny chips by giving the deposit mask a visible crawl structure.

Acceptance:

- `CreviceBase` should still ground the rock at the lower/contact area, but it should be less like a clean continuous belt than Patch 12F.3.
- `DirtDeposit` should show base-connected upward crawl paths with broken internal edges, not broad solid blobs and not only tiny isolated chips.
- `SurfaceVariation` and `Exposure` should remain unchanged.
- `ConvexEdgeWear` and `ConcaveCrease` should remain black/neutral.
- Do not proceed to visible material response until these debug masks are accepted.

### Patch 12G - Edge/Crack Representation Decision and Prototype

Status: planned after `CreviceBase` and `DirtDeposit` reach a decent debug state.

Checklist status:

- [ ] choose between generated overlay strips, true mesh-authored relief, or per-edge/per-triangle shader metadata for line-like features;
- [ ] prototype convex ridge wear without flooding entire triangles;
- [ ] prototype sparse concave cracks/seams without showing raw triangulation;
- [ ] keep line features sparse, readable at gameplay distance, and consistent with the simple blocky low-poly style;
- [ ] only after the representation works, add profile-aware visible material response.

Primary files:

- likely `MassGenerator.cs`;
- possibly a new generated overlay/line mesh helper if overlay strips are selected;
- `SH_PixelSurfaceLit.shader` only after the representation is stable;
- HLSL stone materials only when final visible response is added.

Work:

- prefer light worn ridges over blanket edge darkening;
- separate convex edge wear from concave crack darkening;
- do not use every triangle edge or interpolated scalar wedge masks as the visual basis;
- preserve the original simple blocky low-poly shape language;
- make material profiles use the same future line data differently rather than generating unrelated profile-specific masks.

Acceptance:

- major silhouette and facet edges read as intentionally worn or polished;
- inward seams/cracks read darker without looking like holes;
- the effect remains visible at gameplay camera distance;
- it does not require making every rock highly detailed or realistic.

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
