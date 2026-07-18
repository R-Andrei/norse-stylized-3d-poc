---
document_id: PS3D-10
title: "Project Architecture and Asset Organisation Rules"
version: 0.2
status: active
scope: project-organisation
authoritative_for: "Unity folder placement, asset ownership, naming, feature boundaries, recipes, presets, editor code, and generated-content organisation"
related_documents: [PS3D-00, PS3D-08, PS3D-09]
last_updated: 2026-07-18
---

# Project Architecture and Asset Organisation Rules

## Purpose

Keep the Unity project predictable. Do not invent a new parallel folder hierarchy whenever a feature appears.

## Core rule

> Technical systems live with the system. Configured game content lives with the content scope that uses it.

Examples:

- Shader Graph logic belongs under `Game/Rendering`.
- A configured stone material belongs under `Game/Demo/Materials/Stone`.
- Mass-generation code belongs under `Game/Procedural/Masses`.
- A prefab using a generated mass belongs under `Game/Demo/Prefabs`.

## Current canonical structure

Only create folders when they contain real assets.

```text
Assets/Game/
├── Procedural/
│   ├── Core/
│   │   ├── MeshData.cs
│   │   └── MeshBuilder.cs
│   └── Masses/
│       ├── GeneratedMass.cs
│       ├── GeneratedMassFeatureAtlasBaker.cs
│       ├── MassSurfaceFeatureGenerator.cs
│       ├── MassSurfaceFeatureGraph.cs
│       ├── MassGenerator.cs
│       ├── MassGenerator.*.cs
│       ├── MassGenerator.EdgeWear.*.cs
│       └── Editor/
│           └── GeneratedMassEditor.cs
│
├── Rendering/
│   └── PixelSurface/
│       ├── Shaders/
│       │   └── SG_PixelSurfaceLit.shadergraph
│       ├── Includes/
│       │   └── PixelCellVariation.hlsl
│       └── SubGraphs/
│           └── create only when the first reusable Sub Graph exists
│
└── Demo/
    ├── Scenes/
    ├── Prefabs/
    ├── Materials/
    │   ├── Stone/
    │   ├── Ground/
    │   ├── Wood/
    │   └── Units/
    └── VFX/
```

Do not keep empty material-category folders merely because they may be useful later.

## Folder ownership rules

### Technical domains

Use technical-domain roots for reusable behaviour:

```text
Game/Procedural/
Game/Rendering/
Game/Audio/
Game/Input/
Game/AI/
```

Typical contents:

- C# systems;
- shared algorithms;
- Shader Graphs;
- HLSL includes;
- Shader Graph Sub Graphs;
- editor tooling;
- validation and baking utilities.

Do not store scene-specific material instances, prefabs, or prototype presets here unless they are required assets of the tool itself.

### Demo content

Use `Game/Demo/` for proof-of-concept content:

- scenes;
- configured materials;
- prefabs;
- demo-specific presets;
- VFX configurations;
- generated or baked assets used only by the demo.

Create `Game/Content/` later only when production content actually exists.

### Feature-local editor code

Editor-only code stays beside its runtime feature in a nested `Editor` folder:

```text
Game/Procedural/Masses/
├── GeneratedMass.cs
├── GeneratedMassFeatureAtlasBaker.cs
├── MassSurfaceFeatureGenerator.cs
├── MassSurfaceFeatureGraph.cs
├── MassGenerator.cs
├── MassGenerator.*.cs
├── MassGenerator.EdgeWear.*.cs
└── Editor/
    └── GeneratedMassEditor.cs
```

Do not create a distant global editor hierarchy unless an editor tool genuinely serves several unrelated systems.

Large feature implementations may be split into partial files under the same feature directory. Partial-file names must describe a real responsibility boundary, for example `MassGenerator.EdgeWear.Orchestration.cs` or `MassGenerator.MeshOutput.cs`; they do not create a new architectural layer. The Generated Mass canonical file/method inventory is `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`.

### Shared code

Move code into a shared folder only after two or more real features need it.

Current shared geometry core:

```text
Game/Procedural/Core/
├── MeshData.cs
└── MeshBuilder.cs
```

Do not design a universal geometry framework before repeated needs appear.

## Vertical feature rule

A feature receives one main home.

Example:

```text
Game/Procedural/Masses/
```

contains mass-specific runtime code and editor code.

Do not reproduce the same feature across unrelated parallel trees such as:

```text
Runtime/Geometry/Rocks
Editor/Geometry/Rocks
Prototype/Recipes/Rocks
Prototype/Prefabs/Rocks
```

unless each location contains genuinely different content with a clear owner.

## Shader and material rule

A shader is reusable behaviour. A material is configured content.

```text
SG_PixelSurfaceLit
→ Game/Rendering/PixelSurface/Shaders/

M_PixelStone_Test
→ Game/Demo/Materials/Stone/
```

Organise materials by substance or use, not by shader:

```text
Materials/Stone
Materials/Wood
Materials/Ground
Materials/Metal
Materials/Bone
Materials/Cloth
Materials/Effects
```

The shared shader may support many material families. Their configured material instances remain in their content folders.

## Recipe and preset rule

### Default

Store an object's complete editable recipe inline on its authoring component:

```csharp
GeneratedMass
└── MassRecipe
```

The recipe may include seeds and all meaningful authoring controls.

### Optional preset asset

Use a `ScriptableObject` preset only when there is a demonstrated need to:

- share one configuration across many objects;
- spawn families from a common profile;
- catalogue reusable presets;
- reference the same data from several systems.

A preset is optional convenience, not a mandatory dependency for every generated object.

### Terminology

- **Recipe** — complete resolved parameters for one generated instance.
- **Archetype** — meaningful starting configuration or generation family.
- **Preset/Profile** — optional reusable asset that can populate or constrain recipes.
- **Generator** — code that turns a recipe into output.
- **Authoring component** — Unity component that owns the recipe, generated output, regeneration, and lifecycle.

## Generated-asset rule

Temporary generated meshes:

- are owned by their authoring component;
- use `HideFlags.DontSave`;
- remain local-space geometry;
- are replaced safely during regeneration;
- are not treated as imported or permanent assets.

Baked output, when implemented, should go under a content-owned location such as:

```text
Game/Demo/Generated/
```

or later:

```text
Game/Content/Generated/
```

The baking tool must never destroy or overwrite source/imported assets accidentally.

## Placement and physics rule

Geometry generation and world placement are separate systems.

```text
MassGenerator
    creates local-space geometry

future GroundPlacementResolver
    positions and optionally embeds it in terrain

physics profile
    selects static or dynamic collision representation
```

Current generated masses:

- have lowest local point at `Y = 0`;
- keep world transform scale at `1,1,1` where possible;
- use a non-convex `MeshCollider`;
- have no `Rigidbody`;
- are static environment obstacles.

Future movable masses should use a convex or compound physics proxy rather than the detailed non-convex visual collider.

## Naming rules

| Asset | Prefix | Example |
|---|---|---|
| Shader Graph | `SG_` | `SG_PixelSurfaceLit` |
| Shader Graph Sub Graph | `SGS_` | `SGS_PixelCellVariation` |
| Material | `M_` | `M_Stone_ColdGrey` |
| Prefab | `PF_` | `PF_GeneratedBoulder` |
| Scene | `SC_` | `SC_VisualPrototype` |
| Scriptable preset/profile | feature-specific | `MP_TerrainBoulder` |
| HLSL include | descriptive filename | `PixelCellVariation.hlsl` |

C# filenames match their principal public type.

Use stable names based on responsibility, not temporary implementation detail.

## Namespace rules

Current geometry namespaces:

```csharp
ProgrammaticStylized3D.Geometry
ProgrammaticStylized3D.Geometry.Masses
ProgrammaticStylized3D.Geometry.Masses.Editor
```

Reusable namespaces must remain project-generic. Do not hard-code Norse lore or a specific scene name into reusable systems.

## Moving and renaming assets

Move or rename Unity assets through the Unity Project window so `.meta` files and references are preserved.

Do not:

- move referenced assets with the operating-system file browser;
- duplicate an asset solely to relocate it;
- create a new parallel folder while leaving the old canonical asset in place.

## Decision checklist

Before creating a folder or asset, ask:

1. Is this reusable system behaviour or configured game content?
2. Which feature owns it?
3. Is it shared by more than one real feature?
4. Does an existing canonical folder already own it?
5. Is the new folder necessary now, or only hypothetical?
6. Is this a recipe, optional preset, prefab, generated temporary object, or baked asset?
7. Will a future developer know where to look without searching four parallel hierarchies?

## Current asset corrections

Canonical targets:

```text
SG_PixelFacetedLit
→ rename to SG_PixelSurfaceLit
→ Game/Rendering/PixelSurface/Shaders/

PixelCellVariation.hlsl
→ Game/Rendering/PixelSurface/Includes/

M_PixelStone_Test
→ Game/Demo/Materials/Stone/
```

The shader is generic. The stone material instance is stone-specific.
