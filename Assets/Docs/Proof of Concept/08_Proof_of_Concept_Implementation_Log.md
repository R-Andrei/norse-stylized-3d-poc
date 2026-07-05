---
document_id: PS3D-08
title: "Proof of Concept Implementation Log"
version: 0.2
status: working-log
scope: proof-of-concept
authoritative_for: "current Unity implementation state, provisional decisions, known limitations, implemented filenames and functions, and the next continuation point"
related_documents: [PS3D-06, PS3D-07, PS3D-09, PS3D-10]
last_updated: 2026-06-22
---

# Proof of Concept Implementation Log

## Purpose

Record enough implementation detail to resume work without reconstructing decisions from conversation history.

---

## 2026-06-21 — Initial Unity Implementation Session

### Project setup

- Unity **6.5 (6000.5.0f1)** with URP.
- Visual Studio Code.
- New Input System only.
- Splines installed.
- Cinemachine installed.
- Prototype scene and initial folders created.

### Camera

- Cinemachine tracking camera.
- Stable world-space viewing direction.
- Field of view approximately `40`.
- Camera collision and advanced behaviour deferred.

### Primitive clearing

Temporary approximately `40 × 40 m` clearing:

- flat ground;
- rectangular river;
- manually assembled plank bridge;
- hut;
- dead tree;
- standing stone;
- placeholder rocks;
- player capsule;
- deformable-actor proxy;
- rigid-part actor proxy;
- rough combat and dialogue positions.

Primitive hut, tree, terrain, river, rocks, and actors remain placeholders.

### Atmosphere and lights

- Basic URP Lit materials created.
- Blanket blue-grey fog/lighting experiment rejected.
- Dark nights should come from limited illumination, not a global blue filter.
- Added warm hearth light.
- Added small player-centred aura light.
- Snow remains an environmental layer over substrates.

### Day-night cycle v0.1

Implemented:

- `TimeOfDayProfile`;
- `LightingModifierProfile`;
- `TimeOfDayController`;
- nine editable checkpoints;
- real-time day duration;
- manual time scrubbing and pause;
- celestial rotation;
- sun colour/intensity interpolation;
- ambient colour/intensity interpolation;
- procedural sky and reflection controls;
- optional fog;
- Region, Weather, and Additional modifier slots.

Accepted for v0.1. Artistic values remain provisional.

---

## 2026-06-22 — Geometry Architecture Revision

### Architecture decision

Rejected mandatory external `RockRecipe : ScriptableObject` workflow.

Current model:

```text
GeneratedMass
    owns an inline serializable MassRecipe

MassGenerator
    pure generation logic

MeshData / MeshBuilder
    shared geometry core

GeneratedMassEditor
    editor-only authoring controls
```

Optional `ScriptableObject` presets may be added later only when shared family/profile data is useful.

### Canonical code paths

```text
Assets/Game/Procedural/Core/
├── MeshData.cs
└── MeshBuilder.cs

Assets/Game/Procedural/Masses/
├── GeneratedMass.cs
├── MassGenerator.cs
└── Editor/
    └── GeneratedMassEditor.cs
```

Namespace:

```csharp
ProgrammaticStylized3D.Geometry
ProgrammaticStylized3D.Geometry.Masses
ProgrammaticStylized3D.Geometry.Masses.Editor
```

See `10_Project_Architecture_and_Asset_Organisation_Rules.md` for durable folder rules.

---

## 2026-06-22 — Generated Mass v0.3.2

### `MeshData.cs`

Principal type:

```csharp
MeshData
```

Important members:

- `Vertices`
- `Triangles`
- `UV0`
- `Colors`
- `AddVertex(...)`
- `AddTriangle(...)`
- `AddQuad(...)`
- `Validate()`

### `MeshBuilder.cs`

Principal function:

```csharp
MeshBuilder.ApplyToMesh(
    MeshData data,
    Mesh targetMesh,
    string meshName)
```

Applies vertices, triangles, UV0, vertex colours, normals, tangents, and bounds.

### `GeneratedMass.cs`

Principal enums:

```csharp
MassArchetype
MassScaleStep
FormComplexity
SurfaceFacetDensity
EdgeCharacter
ShapeDiversity
GroundingStyle
LeanStyle
```

Current `MassArchetype` values:

```text
TerrainBoulder
SquatBoulder
StandingStone
FlatSlab
BrokenChunk
PolishedStone
```

`MassRecipe` important fields/properties:

```text
archetype
shapeSeed
surfaceSeed
size
formComplexity
surfaceFacetDensity
edgeCharacter
shapeDiversity
grounding
lean
fineScale
widthBias
heightBias
depthBias
surfaceVariation
```

Seed range:

```csharp
MassRecipe.MinimumSeed = 1
MassRecipe.MaximumSeed = 9999
```

Seed responsibilities:

- `ShapeSeed` — proportions, major cuts, macroform, lean, silhouette.
- `SurfaceSeed` — surface triangulation, subtle relief, vertex-colour variation.

Important `MassRecipe` methods:

```csharp
SetShapeSeed(int value)
SetSurfaceSeed(int value)
ApplyArchetypeDefaults()
```

Important `GeneratedMass` methods:

```csharp
Regenerate()
CreateNewShape()
CreateNewSurface()
CreateNewVariant()
ResetRecipeToArchetype()
```

Important lifecycle helpers:

```csharp
EnsureRecipeState()
CacheComponents()
EnsureGeneratedMesh()
ClearGeneratedAssignments()
OnDestroy()
```

Generated mesh naming:

```text
GeneratedMass_<Archetype>_Shape<ShapeSeed>_Surface<SurfaceSeed>
```

Temporary mesh:

```csharp
hideFlags = HideFlags.DontSave
```

Collider:

```text
MeshCollider
Convex = false
No Rigidbody
```

Local-space placement contract:

- lowest generated point is local `Y = 0`;
- world placement remains separate;
- future terrain placement may rotate and partially embed the object;
- keep Transform scale at `1,1,1` where possible.

### `GeneratedMassEditor.cs`

Custom Inspector buttons:

```text
New Shape
New Surface
New Variant
Regenerate
Reset to Archetype
```

Main methods:

```csharp
OnInspectorGUI()
ApplyToTargets(...)
```

Supports multi-object editing and Undo.

### `MassGenerator.cs`

Entry point:

```csharp
MassGenerator.Generate(MassRecipe recipe)
```

Construction selection:

```csharp
UsesRadialBuilder(...)
```

- `PolishedStone` uses radial/geodesic generation.
- Other current archetypes use plane-cut generation.

Plane-cut pipeline:

```csharp
BuildPlaneCutMass(...)
CreateBoxExtents(...)
CreateBoxFaces(...)
SelectMacroProfile(...)
ApplyProfileCuts(...)
ApplyCut(...)
ClipPolyhedron(...)
ClipPolygon(...)
CreateOrientedFace(...)
TriangulatePolyhedron(...)
BuildSegmentedBoundary(...)
```

Current internal macro profiles:

```text
Block
Wedge
Shoulder
Ridge
Crown
```

Radial/polished pipeline:

```csharp
BuildRadialMass(...)
BuildGeodesicTopology(...)
GenerateRadialRadii(...)
RelaxRadii(...)
LimitLocalPointiness(...)
```

Shared transform/output functions:

```csharp
ResolveDimensions(...)
ApplyDimensions(...)
ApplyLean(...)
ApplyGrounding(...)
RecenterOnGround(...)
BuildMeshData(...)
AddRenderedVertex(...)
```

Determinism:

- local `System.Random`;
- seed-specific salts through `CreateRandom(int seed, int salt)`;
- no use of Unity global random state.

### Closed-mesh bug and fix

Symptom:

- structured triangular holes/slits;
- probability increased with higher `FormComplexity`.

Root cause:

```csharp
AddOrientedTriangle(...)
```

compared the squared cross-product magnitude against linear `PlaneEpsilon`, deleting small but valid triangles.

Current fix:

```csharp
RelativeTriangleAreaEpsilon
MinimumEdgeLengthSqr
```

`AddOrientedTriangle(...)` now uses a scale-relative area test based on maximum edge length.

Additional sanitation retained:

```csharp
WeldSharedVertices(...)
SanitizeAllFaces(...)
SanitizePolygon(...)
CalculatePolygonArea(...)
```

Important tolerances in v0.3.2:

```csharp
PlaneEpsilon
PointMergeDistance
PointMergeDistanceSqr
RelativeCollinearEpsilon
RelativeTriangleAreaEpsilon
MinimumEdgeLengthSqr
TinyFaceAreaEpsilon
```

Status:

- hole bug confirmed fixed after repeated high-complexity seed changes;
- shape diversity and plane-cut output accepted;
- do not weaken cuts or smooth the macroforms to solve topology bugs.

---

## 2026-06-22 — Pixel Surface Experiment v0.1

### Decision

Selected the first pixelization experiment:

> Shared material-space pixel cells while retaining standard URP Lit lighting.

Low-resolution scene rendering remains an optional later experiment.

### Canonical rendering paths

```text
Assets/Game/Rendering/PixelSurface/
├── Shaders/
│   └── SG_PixelSurfaceLit.shadergraph
├── Includes/
│   └── PixelCellVariation.hlsl
└── SubGraphs/
    └── create only when reused graph logic is extracted
```

Configured demo material:

```text
Assets/Game/Demo/Materials/Stone/
└── M_PixelStone_Test.mat
```

Current asset correction:

- rename `SG_PixelFacetedLit` to `SG_PixelSurfaceLit`;
- keep shader/HLSL under `Rendering`;
- move the configured stone material under `Demo/Materials/Stone`;
- move/rename through Unity's Project window.

### `PixelCellVariation.hlsl`

Important functions:

```hlsl
PS3D_Hash31(float3 value)

PixelCellVariation_float(
    float3 Position,
    float CellSize,
    float Seed,
    float ToneCount,
    float ClusterStrength,
    out float Variation)

PixelCellVariation_half(...)
```

Behaviour:

- snaps object-space position to cells;
- hashes detail cells;
- hashes larger clustered cells;
- blends by `ClusterStrength`;
- quantizes to `ToneCount`;
- outputs approximately `-1` to `1`.

### `SG_PixelSurfaceLit.shadergraph`

Current graph properties:

```text
Base Color
Cell Size
Pixel Variation
Pixel Seed
Tone Count
Cluster Strength
Vertex Variation
Smoothness
```

Starting values tested:

```text
Cell Size         0.18
Pixel Variation   0.14
Pixel Seed        17
Tone Count        3
Cluster Strength  0.65
Vertex Variation  0.06
Smoothness        0.02
```

Graph flow:

```text
Object-space Position
→ Custom Function: PixelCellVariation
→ multiply by Pixel Variation

Vertex Color
→ Split
→ R
→ subtract 0.5
→ multiply 2
→ multiply Vertex Variation

pixel contribution + vertex contribution + 1
→ Clamp 0.55–1.45
→ multiply Base Color RGB
→ URP Lit Base Color
```

Other Lit inputs:

```text
Metallic = 0
Smoothness = material property
Normal = mesh normals
Emission = black for current stone test
Surface = Opaque
```

Current result:

- removes uniform dullness;
- adds useful blocky material variation;
- remains generic enough for stone, ground, wood, units, and props;
- `M_PixelStone_Test` is stone-specific configuration, but `SG_PixelSurfaceLit` is generic;
- standard URP lighting remains active.

### Current limitation

Pixel cells are position-based noise and do not understand structural geometry.

Desired future structured masks:

```text
Vertex Color R = broad material variation
Vertex Color G = structural edge/exposure mask
Vertex Color B = cavity/shelter mask
Vertex Color A = reserved/material-state mask
```

Only red broad variation is currently implemented.

Future generators may author edge/cavity masks. The shader should continue to work when those masks are absent.

Do not use screen-space outlines or every-triangle wireframe accents as the primary solution.

---

## 2026-07-04 — River Foam Lifetime Probe and Lifecycle Delta Repair

### Problem being investigated

During river Foam validation, manual/progressive Foam survived far longer than `Neutral Lifetime = 1` and appeared to disappear in large synchronized regions. The Inspector also repainted inconsistently unless the mouse hovered over it.

### Diagnostic work completed

A compact Foam material probe workflow was added rather than another broad Foam rewrite. The Inspector now exposes raw material Presence / Remaining Life views and small probe buttons for configured and absolute 1-second lifetime tests. The absolute probe bypasses topology and configured lifetime scaling so it can prove whether the persistent material state itself ages.

The probes showed that raw material patches were written correctly, topology was not involved, birth refresh was idle, and lifetime values reached runtime/GPU setup correctly. However, both configured and absolute probes failed to age.

### Root cause found

`_FoamDeltaTime` is shared by material lifecycle and topology maintenance compute paths. Topology refresh could configure it with `0` immediately before the material `SimulateFoam` dispatch. As a result, the lifecycle pass dispatched but subtracted zero Remaining Life each step. The material clock recorded attempted CPU steps, but the GPU material state did not actually age.

### Implemented repair

`StylizedRiverFoamRuntime.SimulateFullField(deltaTime)` now rebinds the lifecycle compute parameters immediately before `DispatchSimulation(...)`. This keeps the repair narrow: no new lifecycle kernel, no topology rewrite, and no new production Foam behavior.

### Validation target

Use `Neutral Lifetime = 1`, both aging rates at `1`, and `Material Remaining Life` debug view. The absolute 1-second probe should now die in order: `0.33`, then `0.66`, then `1.00`, with no raw Remaining Life after roughly 1.1 seconds. If this passes, further work should move away from Inspector telemetry and back to actual Foam behavior: production birth, breakup, drift, and obstacle interaction.


## River Foam Patch 4.11C.5.4i — Support/Negative Aging Response Repair

After 5.4h restored real material aging, the next Foam pass addressed support/negative aging edge behavior rather than adding more diagnostics. Code inspection showed that topology fields were already scalar and bilinearly sampled, so the immediate issue was the local material aging formula.

The compute path now uses a shared `FoamResolveLocalAgeRate(...)` helper for both simulation and metrics. Negative Aging Pressure now suppresses positive support preservation before applying the faster negative aging multiplier. This avoids the previous case where full support plus full negative pressure could still age slower than neutral Foam.

No topology generation, transport, birth, obstacle flow, drift, or beauty shader behavior was changed in this pass.

---

## Architecture and folder rules now active

Summary:

- reusable systems under technical domains such as `Procedural` and `Rendering`;
- configured proof-of-concept content under `Demo`;
- shader behaviour and material instances do not share the same owner;
- editor code stays beside its feature in `Editor`;
- recipes are inline by default;
- presets are optional shared assets;
- organise material instances by substance/use;
- create folders only when they contain real assets;
- avoid parallel folder hierarchies for one feature;
- move and rename assets through Unity.

Full authority: `10_Project_Architecture_and_Asset_Organisation_Rules.md`.

---

## Current limitations

- No player movement controller.
- Ground remains a flat placeholder.
- River remains a rectangular placeholder.
- Bridge remains manually assembled.
- Hut and dead tree remain primitive placeholders.
- No automatic mass-to-terrain placement; future placement may sample terrain and partially embed masses.
- No movable/generated dynamic-rock physics profile.
- No mesh baking tool.
- No optional shared preset/profile assets.
- Pixel Surface shader has no snow layer yet.
- Pixel Surface shader has no geometry-authored edge or cavity masks yet.
- Pixel Surface shader has only been evaluated primarily on generated masses.
- Per-renderer pixel seed is not implemented; shared material seed changes all users.
- Weather and region lighting remain modifier extension points.
- Fog and post-processing remain intentionally minimal.

---

## Current decisions

- Generated Mass v0.3.2 is accepted as the first successful generated asset family.
- Plane-cut generation is the default for terrain stones.
- Radial generation remains only for `PolishedStone`.
- Keep shape generation, placement, and physics as separate responsibilities.
- Slight terrain embedding is acceptable for future prop placement.
- Keep standard URP lighting for the first shared pixel-surface shader.
- Treat pixel-cell variation as a generic rendering core, not a stone-only effect.
- Use separate material-family configurations or lightweight derivatives rather than one bloated universal material.
- Keep primitive scene assets unpolished until replaced by meaningful tests.
- Follow PS3D-10 for folder and asset placement.

---

## Next continuation point

1. Move/rename the Pixel Surface assets to the canonical PS3D-10 locations.
2. Test `SG_PixelSurfaceLit` on:
   - generated stone;
   - ground;
   - hut wall;
   - wood beam or roof;
   - player proxy.
3. Confirm the generic cell effect remains useful across different substances.
4. Decide whether to extract the pixel logic into a Shader Graph Sub Graph.
5. Consider geometry-authored `G` edge and `B` cavity masks only after the generic material test.
6. Build the structured twelve-mass comparison set if not already completed.
7. Then continue to the generated ground-patch milestone.

Do not begin production terrain placement, dynamic rock physics, or a universal shader framework before these tests.

## River Foam 4.11C.5.4j–5.4l — Spawn UI and Budgeting Summary

This group of patches cleaned the manual Foam spawning workflow and corrected a hidden runtime scaling problem. The old manual button wall was removed, spawning stayed under one `Foam Debug > Spawning` workflow, and 5.4l changed one user-facing spawn into one budgeted composition event instead of several hidden progressive writer events. Birth dispatches are now internally budgeted by quality tier.

Important correction: these patches did **not** solve the reference visual language. The later pattern/shape logic still produced chip/slug/blob births and should not be treated as the accepted Foam morphology direction.

## River/Foam Documentation Consolidation

The river/Foam documents were consolidated to avoid conflicting long-form plans. Active docs are now:

- `River_Rendering_Roadmap.md` — macro river stage order and Stage 6 summary;
- `River_Foam_Stage6_Architecture.md` — canonical Foam architecture and contracts;
- `River_Foam_Active_Blockers_and_Next_Patches.md` — current blockers and next patch sequence.

Older material-state, topology, progressive-scheduling, and problem-register documents should be removed after their stable content is merged into the three active docs.

Next actionable item: `4.11C.5.4m — Manual Source Realignment`, before temporal morphing or automatic Foam population.
