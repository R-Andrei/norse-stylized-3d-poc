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
- [ ] Patch 8 - HLSL semantic surface response
- [ ] Patch 9 - Material variants

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

Status: not started.

Checklist status:

- [ ] consume vertex colour `G` for exposure brightening;
- [ ] consume vertex colour `B` for crevice/base darkening;
- [ ] add low-frequency blotch variation;
- [ ] add optional cell-position warp;
- [ ] expose conservative controls.

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

### Patch 9 - Material Variants

Status: not started.

Checklist status:

- [ ] create a small set of material instances after the HLSL shader is accepted;
- [ ] keep all variants on the same shader;
- [ ] tune palette and semantic response rather than importing textures.

Primary files:

- `Assets/Game/Demo/Materials/Stone/`

Work:

- create a small set of material instances after the HLSL shader is accepted;
- keep all variants on the same shader;
- tune palette and semantic response rather than importing textures.

Suggested variants:

- cold grey stone;
- dark wet river stone;
- pale frost stone;
- black sacred/fractured stone.

Acceptance:

- variants are meaningfully distinct at gameplay camera distance;
- existing default stone remains available.

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
