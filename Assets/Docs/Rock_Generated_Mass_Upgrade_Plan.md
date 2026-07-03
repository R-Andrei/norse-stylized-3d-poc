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
- 2-4 horizontal or near-horizontal bias cuts;
- subtly stepped side planes;
- occasional thin cap plane;
- restrained vertical relief.

Implementation direction:

- reuse the plane-cut builder;
- add an archetype-specific macro profile or post-profile cut pass;
- bias cut normals toward horizontal layering;
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

- `FormComplexity.Simple`
- `SurfaceFacetDensity.Low`
- `EdgeCharacter.Sharp`
- `ShapeDiversity.Broad`
- `GroundingStyle.Stable`
- `LeanStyle.Subtle`

### 3. `CarvedMarkerStone`

Purpose: a simple rune/shrine/story marker archetype that stays within the mass system but creates a usable face for later decals, carvings, or glowing symbols.

Shape language:

- upright block or squat monolith;
- one intentionally broad, flatter front face;
- restrained side cuts;
- mild crown or wedge top;
- slightly ceremonial symmetry without becoming perfectly artificial.

Implementation direction:

- reuse the plane-cut builder;
- introduce an archetype-specific "presentation face" bias;
- keep one side from receiving deep random cuts;
- optionally write a future vertex/UV mask that identifies the presentation face;
- do not implement actual runes in this patch unless a separate carving system is approved.

Suggested defaults:

- `FormComplexity.Simple`
- `SurfaceFacetDensity.Low`
- `EdgeCharacter.Worn` or `Sharp`
- `ShapeDiversity.Restrained`
- `GroundingStyle.Stable`
- `LeanStyle.None`

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

The HLSL shader must:

- compile in URP;
- support forward lit rendering;
- receive shadows;
- respect base colour, smoothness, and pixel variation;
- consume vertex colours using the new contract;
- keep material instances easy to migrate;
- provide visual parity with `M_PixelStone` before extra features are enabled.

Do not delete the Shader Graph in the same patch. First switch only the test stone material or a duplicate material to the HLSL shader, validate, then migrate the main material.

## Rough Implementation Plan

### Patch 1 - Document and Baseline Capture

Status: this document.

Work:

- document current constraints and desired changes;
- capture before screenshots of representative rocks;
- record current `M_PixelStone` property values;
- identify existing scene masses that cover small, large, standing, slab, broken, and polished shapes.

Acceptance:

- the upgrade has a clear implementation order;
- no runtime or scene behaviour changes.

### Patch 2 - Vertex Colour Semantic Contract

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

Primary files:

- `GeneratedMass.cs`
- `MassGenerator.cs`

Work:

- append `LayeredStone` to `MassArchetype`;
- add archetype defaults;
- add dimensions in `GetBaseDimensions`;
- add cut-depth and dimension constraints if needed;
- implement a layered macro profile or archetype-specific cut pass;
- validate at several seeds and size steps.

Acceptance:

- existing archetypes produce comparable shapes to before;
- `LayeredStone` reads as distinct at gameplay camera distance;
- output remains simple and stable.

Rollback:

- remove only the appended enum value and its switch branches before any scene depends on it.

### Patch 4 - Add `FracturedPillar`

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

Primary files:

- `GeneratedMass.cs`
- `MassGenerator.cs`

Work:

- append `CarvedMarkerStone` to `MassArchetype`;
- add defaults and dimensions;
- bias generation around one broad presentation face;
- optionally reserve a future mask for that face, but do not implement carvings yet.

Acceptance:

- produces a readable flat face across many seeds;
- remains a generated mass, not a bespoke shrine system;
- no carving dependency is introduced.

### Patch 6 - HLSL Pixel Surface Baseline

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

### Patch 7 - HLSL Semantic Surface Response

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

### Patch 8 - Material Variants

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
