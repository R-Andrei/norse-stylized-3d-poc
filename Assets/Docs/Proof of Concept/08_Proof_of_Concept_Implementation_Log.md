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

This group of patches cleaned the manual Foam spawning workflow and corrected a hidden runtime scaling problem. The old manual button wall was removed, manual birth stayed in a dedicated Foam Debug workflow, and 5.4l changed one user-facing spawn into one budgeted composition event instead of several hidden progressive writer events. Birth dispatches are now internally budgeted by quality tier.

Important correction: these patches did **not** solve the reference visual language. The later pattern/shape logic still produced chip/slug/blob births and should not be treated as the accepted Foam morphology direction.

## River/Foam Documentation Consolidation

The river/Foam documents were consolidated to avoid conflicting long-form plans. Active docs are now:

- `River_Rendering_Roadmap.md` — macro river stage order and Stage 6 summary;
- `River_Foam_Stage6_Architecture.md` — canonical Foam architecture and contracts;
- `River_Foam_Active_Blockers_and_Next_Patches.md` — current blockers and next patch sequence.

Older material-state, topology, progressive-scheduling, and problem-register documents should be removed after their stable content is merged into the three active docs.

Next actionable item: `4.11C.5.4m — Manual Source Realignment`, before temporal morphing or automatic Foam population.

## River Foam 4.11C.5.4m — Manual Source Realignment

Manual Foam birth was simplified back to a stable source contract. The active Inspector workflow is now `Foam Debug > Manual Birth Source` rather than pattern-driven spawning. Pattern, Complexity, and Density were removed from the active manual controls because they were obsolete birth-time art controls and made the same settings produce chip/slug/blob macro identities.

Birth injection now writes a canonical moving source using source Amount, Initial Remaining Life, Half Width, and optional Source Path Motion. Destructive pattern/composition masks were removed from the compute birth path, and source-fill seeding is derived from source controls instead of event count so repeated identical starts are comparable. This does not solve final Foam beauty; it gives later temporal morphing, organic breakup, topology calibration, and obstacle tests a trustworthy material source.

Next actionable item: `4.11C.5.5 — Temporal Morphing and Material Shape Evolution`.


## River Foam 4.11C.5.4m-hotfix — Inspector Control Cleanup

The Foam Debug Inspector organization was corrected after the manual source realignment. All manual birth controls now live under `Foam Debug > Manual Birth Source`, with subgroups for Source Position, Source Material, Source Shape, Source Path Motion, Actions, and State. Persistent downstream travel diagnostics are labelled `Material Motion`; stored/visible footprint diagnostics are labelled `Material Shape`.

No runtime Foam behavior changed in this hotfix.

Next actionable item: `4.11C.5.5 — Temporal Morphing and Material Shape Evolution`.

## River Foam 4.11C.5.5 — Persistent Foam Morphing and Gradual Erosion

Added the first persistent material-simulation shape evolution pass. Manual birth remains a stable source; topology remains influence-only; the final shader remains responsible for micro breakup and presentation. The compute simulation now lets stored Foam `Presence` subtly reconfigure over time through flow-aligned/lateral sampling, then applies slower edge-, age-, and topology-biased erosion. This is intended to make `Material Presence` itself change shape instead of relying only on final shader masking.

No manual birth controls, automatic population, topology generation, obstacle sliding, or Inspector layout were changed.

Next actionable item after validation: `4.11C.5.6 — Organic Breakup and Edge Readability`.

## River Foam 4.11C.5.5b — Macro Material Deformation

Validation showed that 5.5 mostly produced edge roughening and gradual erosion while the stored Foam body still read as a rigid strip. 5.5b strengthens the persistent material simulation layer: `Material Presence` is now backtraced from several low-frequency flow-space deformation samples so existing Foam can visibly bend, stretch, locally widen/narrow, and alter its silhouette over time. Erosion remains a slower separate trend.

This is still intrinsic Foam morphology only. River disturbance/wave coupling is intentionally documented as a later explicit step, not hidden inside this patch. No manual birth controls, topology generation, automatic population, obstacle sliding/clipping, or Inspector layout were changed.


## River Foam 4.11C.5.5c — Lifecycle Authority Repair

Validation of 5.5b showed a project-breaking regression: Foam could disappear in 1–2 seconds even with long Neutral Lifetime and strong support. The audit found two independent non-lifecycle death paths introduced by the morphing work: explicit simulation-side `Presence` erosion and morph blending against empty samples that could reduce existing material below visibility before `Remaining Life` expired.

5.5c removes simulation-side `Presence` erosion and preserves existing-cell `Presence` during morphing. Stored morphing may still bend/stretch/reconfigure material, but material death is again controlled only by `Remaining Life`, whose delta comes from Neutral Lifetime and topology aging influence. No Inspector, birth, topology, automatic population, or obstacle behavior changed.

### 4.11C.5.5d — Area-Balanced Foam Wobble

Adjusted persistent Foam morphology after validation showed 5.5b/5.5c still tended to stretch/grow rather than wobble around a stable average footprint. The simulation morph pass now uses opposed, normalized intrinsic wobble samples instead of preserving current Presence as a union. This keeps lifecycle authority with Remaining Life while allowing visible back-and-forth bending, local widening/narrowing, and relaxation. River wave/static-pressure/lee/ripple/disturbance coupling remains a later dedicated patch layered on top of intrinsic morphology, not a substitute for it.


## River Foam 4.11C.5.6 — Surface-Coupled Foam Rendering

Intrinsic runtime Foam morphology from 5.5d is accepted as good enough for now. 5.6 adds the first render-layer coupling between Final Foam and the existing river surface systems. The Foam renderer now receives macro wave/current influence, disturbance height/gradients/velocity, and transported wake energy/gradients from the water shader path. These values warp, stretch, compress, and edge-modulate the Final Foam mask so existing material reads as attached to waves, static pressure, lee/depression, ripples, and wakes.

This is presentation-only coupling. `Material Presence`, Remaining Life, manual birth, topology, and population are unchanged. A later dedicated patch may couple stored material motion to disturbances after render coupling is validated.

## River Foam 4.11C.5.6b — Foam Surface Clarity Filter

Validation of 5.6 showed that render-layer surface coupling works, but fine water-surface/detail variation can appear too strongly inside solid Foam and make the clean stylized white body look noisy. 5.6b keeps surface coupling render-only, but filters Foam interior lighting: ordinary granular water variation is heavily suppressed on high-coverage Foam, while strong waves/wakes/disturbances can still imprint at reduced strength. `Material Presence`, lifecycle, topology, birth controls, and stored-state morphology are unchanged.

Next actionable item: `4.11C.5.7 — Surface-Driven Material Morphing`, where the existing river surface/disturbance fields should amplify persistent Foam morphology without becoming a death/lifecycle authority.

## River Foam 4.11C.5.7 — Surface-Driven Material Morphing

Added stored-state surface coupling to the persistent Foam simulation. `SimulateFoam` now receives the existing ripple, transported wake, static wake/lee, and static pressure fields and uses them as bounded inputs to the accepted area-balanced material wobble. Disturbed water can now make `Material Presence` itself wobble, bend, and reconfigure more strongly.

This patch does not change manual birth controls, automatic population, topology generation, Foam color, render-side clarity filtering, or lifecycle authority. Surface fields do not spawn Foam and do not reduce `Remaining Life`; material death remains controlled by the approved topology aging path.

Validation result: surface-driven material morphing works, but the useful authored range is currently around `2.0-4.0`, with approximately `2.5` as the practical working value. Further formula polish is deferred.

Next actionable item: `4.11C.5.8 — Chaotic Intermittent Foam Drift`.

## River Foam 4.11C.5.7b — Surface Morph Calibration

Validation of 5.7 suggested that the disturbance-to-material connection was likely present but too subtle to judge by eye. 5.7b adds one explicit `Surface Morph Strength` control under Foam material motion. `0` disables stored-state surface response for A/B comparison, `1` preserves the conservative 5.7 baseline, and higher values strengthen the bounded agitation/bias response from waves, pressure, lee, and wake fields.

The calibration only affects persistent material morphing strength and direction bias. It does not change manual birth controls, automatic population, topology generation, Foam color, render-side clarity filtering, or lifecycle authority. Surface fields still do not spawn Foam and do not reduce `Remaining Life`.

Validation result: surface-driven material morphing works, but the useful authored range is currently around `2.0-4.0`, with approximately `2.5` as the practical working value. Further formula polish is deferred.

Next actionable item: `4.11C.5.8 — Chaotic Intermittent Foam Drift`.

## River Foam 4.11C.5.7c — Surface Morph Formula Rebalance

Validation of 5.7b proved the stored-state surface morph path works, but the response was undertuned: `Surface Morph Strength = 5` read more like an acceptable strong setting than an overdriven debug/stress value. 5.7c rebalances the internal formula rather than hiding a larger multiplier behind the control. Low/mid wave, wake, pressure, and lee signals are lifted, wake/lee/pressure gradients are weighted more strongly, and edge mobility receives more of the visible response.

The intended meaning after this pass is: `0` disables stored-state surface response for A/B testing, `1` is the normal readable authored response, `2` is strong, and `3+` is overdrive/stress-test territory. The change still affects only persistent material morphing strength/direction. It does not change Foam birth, automatic population, topology generation, Foam color, render-side clarity filtering, or lifecycle authority. Surface fields still do not spawn Foam and do not reduce `Remaining Life`.

Validation result: surface-driven material morphing works, but the useful authored range is currently around `2.0-4.0`, with approximately `2.5` as the practical working value. Further formula polish is deferred.

Next actionable item: `4.11C.5.8 — Chaotic Intermittent Foam Drift`.

## River Foam 4.11C.5.8 — Chaotic Intermittent Foam Drift

Added true stored-state chaotic drift to the persistent Foam simulation. The implementation stays inside the existing `SimulateFoam` pass rather than adding a new compute dispatch or texture. `Material Presence` now receives a bounded backtrace offset from a deterministic, coherent, intermittent drift field: Foam remains net-downstream through the existing phase transport, but can sometimes drift laterally, sometimes enter calm/no-drift intervals, shear locally, and show small resistance/compression moments.

Two material-motion controls were added: `Chaotic Drift Strength` controls the amount of intermittent material drift, and `Chaotic Drift Rhythm` controls how frequently coherent drift events become active. `Strength = 0` is the A/B disabled path. The drift does not change Remaining Life, birth/source semantics, topology generation, automatic population, or final visual fragmentation. A small valid-fluid safety attenuation prevents obvious lateral dumping into invalid water, but this is not obstacle steering; obstacle-based tangential movement remains a separate future patch.

Next actionable item after validation: `4.11C.5.9 — Obstacle-Based Tangential Foam Movement`.

## River Foam 4.11C.5.8b — Chaotic Drift Calibration

Validation of 5.8 proved the chaotic intermittent drift path works, but the authored range was undertuned: maximum Strength/Rhythm settings read closer to a normal usable effect than exaggerated stress behavior. 5.8b keeps the same two controls and the same single-pass persistent simulation architecture, but recalibrates the internal formula. The activity gate now wakes up more often at normal rhythm values while preserving true calm intervals; low/mid activity is lifted; lateral impulse, shear, and resistance are stronger at `Strength = 1`; and the existing one-pass material gather now blends in the drifted center sample so the stored Material Presence is less anchored to its current cell.

This remains a calibration patch only. It does not add obstacle steering, change downstream phase transport, modify Remaining Life, spawn Foam, alter topology generation, or touch final visual fragmentation. After this patch, testing should restart from `Chaotic Drift Strength = 1` and `Chaotic Drift Rhythm = 1`; old max settings should be treated as stress values, not the expected normal operating point.

Next actionable item: `4.11C.5.9 — Obstacle-Based Tangential Foam Movement`.

## River Foam 4.11C.5.8c — Macro Drift Rebalance

Validation of 5.8b proved the chaotic intermittent drift path was now strong enough, but the visible response was weighted too much toward local edge tearing. Large Strength values produced crawling, shredded borders while the broader Foam patch still felt comparatively anchored.

5.8c keeps the same two controls, the same existing `SimulateFoam` pass, and no new textures or dispatches. The HLSL drift resolver now separates chaotic drift into macro body transport, meso shear, and edge detail. `FoamApplyPersistentMaterialMorph` uses the macro backtrace as the primary advected base first, then samples meso/edge deformation around that base. Edge tearing remains available, but it is reduced to secondary border detail instead of being the dominant expression of chaotic drift.

This patch does not add obstacle steering, change downstream phase transport, modify Remaining Life, spawn Foam, alter topology generation, or touch final visual fragmentation. Validate in Material Presence from `Chaotic Drift Strength = 0`, then `1`, then `2`, with `Rhythm = 1`; success means the patch body meanders as a broader mass and edges no longer dominate the effect.

Next actionable item after validation: `4.11C.5.9 — Obstacle-Based Tangential Foam Movement`.

## River Foam 4.11C.5.8d — Macro Authority Calibration

Validation of 5.8c showed clear improvement, but the motion still read too much like micro/edge animation: edges were lively enough, while the larger stored Foam patch body still did not meander strongly enough.

5.8d keeps the same controls, the same existing `SimulateFoam` pass, and no new textures or dispatches. The HLSL calibration now routes `Chaotic Drift Strength` more strongly into broad macro backtrace/body transport, trusts the macro-advected base more during active drift events, and reduces meso shear plus edge detail amplitudes so edge tearing becomes a light secondary layer rather than the visible driver of the effect.

This patch remains strictly a persistent material-motion calibration. It does not add obstacle steering, change downstream phase transport, modify Remaining Life, spawn Foam, alter topology generation, add controls, or touch final visual fragmentation. Validate in Material Presence from `Chaotic Drift Strength = 0`, then `1`, then `2`, with `Rhythm = 1`; success means the broader body movement is now the dominant read and edge motion no longer overwhelms macro drift.


## River Foam 4.11C.5.9 — Unified Foam Motion Field

This patch replaces the accepted-but-interim 5.8d local chaotic drift path as the active macro lateral movement authority. The previous 5.8 series remains useful work history: it proved that stored Foam must move as a broad body rather than only through edge crawl. However, the local chaotic drift resolver mixed macro, meso, edge, activity, rhythm, and resistance logic inside the material morph function, which made it hard to inspect directly and unsuitable as the long-term obstacle-routing foundation.

5.9 introduces an explicit Foam Motion Field. The field answers one question only: at this river/Foam position, which lateral direction should stored Foam material prefer? It does not own downstream travel, birth, lifetime, topology, automatic population, final colour, or final visual breakup.

### Field texture model

The implementation uses two separate fields because their coordinates and update rules are different:

- `PS3D_RiverFoam_MotionLane` / `_FoamMotionLane` is the dense lane field. It stores one signed lateral value in `RHalf`: negative for left, positive for right, and zero for intentional neutral/calm. It covers the valid river domain densely; black in the debug view means the generated value resolved near neutral, not that nothing wrote to the field. After validation feedback, generation was corrected from a few broad value-noise bands to a denser layered/domain-warped fractal field so the debug view reads like granular turbulent noise split into two colours.
- `PS3D_RiverFoam_ObstacleRouting` / `_FoamObstacleRouting` is the fixed obstacle-routing field. It stores signed lateral route direction and route influence in `RGHalf`. It does not contain any downstream or upstream component.

A packed one-texture design was rejected for this patch. The lane field must scroll through downstream sample coordinates, while obstacle routing must stay fixed around actual obstacle cells. A valid packed texture would still need two logical samples with different coordinates, so packing would not reduce the true simulation sample count and would make future debugging easier to confuse.

### Performance contract

The steady-state simulation cost is intentionally fixed and small:

- two lane loads along X plus interpolation, so lane scrolling is smooth at fractional cell offsets;
- one obstacle-routing load at the fixed storage coordinate;
- no procedural multi-octave lane noise in `SimulateFoam`;
- no local obstacle search in `SimulateFoam`;
- no full-field rebuild because time passes;
- no CPU Foam-cell loops;
- no GPU readback.

`Motion Field Scroll Hz` is a phase-scroll speed, not a rebuild rate. The default is `0.01`, meaning one complete field wrap every 100 seconds. The lane texture regenerates only when its dirty inputs change: dimensions/domain, visual seed, neutral coverage, or lane scale. The obstacle routing texture regenerates only when obstacle/domain data changes.

### Controls

The active Material Motion controls are:

- `Motion Field Strength` — field-driven lateral macro movement strength. `0` disables field-driven lateral movement, `1` is the normal authored value, and higher values exaggerate the response for validation.
- `Motion Field Scroll Hz` — full downstream lane-field wraps per second. This advances sample phase only.
- `Motion Field Neutral Coverage` — approximate fraction of the generated lane field that resolves near neutral/black. Default is `0.10`.
- `Motion Field Lane Scale` — broadness/fineness of the generated lane pattern. Changing it regenerates the lane texture only.

The old `Chaotic Drift Strength` and `Chaotic Drift Rhythm` controls, serialized fields, compute uniforms, and HLSL resolver are removed from active code. There is no hidden compatibility drift path.

### Obstacle routing

Obstacle routing is generated by CPU stamping from the existing obstacle exclusion cells and uploaded to the obstacle-routing texture. After validation feedback, the routing generator was corrected to group occupied obstacle cells into connected obstacle bodies and stamp one coherent field per body. It is lateral-only. It must not create upstream compression, downstream acceleration, radial repulsion, attraction, early Foam death, Foam birth, topology painting, or final-render masking.

The resolver uses obstacle influence as an override weight:

`finalLateral = lerp(laneLateral, obstacleLateral, obstacleInfluence)`

Each connected obstacle body writes a longer upstream approach region with weaker influence, so incoming Foam begins redirecting before contact. Near the obstacle, influence becomes strong and overrides the dense lane field. At the falloff edge, the lane field blends back in. Direction is resolved relative to the obstacle body centerline, with a stable lane-informed tie-break for centerline cases, so a single rock does not alternate left/right in stripes.

### Debug view

A new `Foam Motion Field` debug view is added. In Play Mode it shows the same textures used by the simulation:

- blue/cyan = leftward stored-material motion;
- red/orange = rightward stored-material motion;
- black = intentional neutral/calm lane value;
- green/yellow = obstacle override influence;
- semi-transparent white = current Foam mask overlay.

This debug view is intended to be dense. Most of the valid river should be red/orange or blue/cyan. Mostly black output means the field generation or binding is wrong, not merely a tuning preference. Spawned Foam should remain visible as a translucent white overlay so field direction and actual material can be validated together.

### Preserved ownership boundaries

This patch preserves the Stage 6 ownership split:

- manual birth creates source material only;
- downstream phase transport still owns ordinary downstream movement;
- the Foam Motion Field owns lateral macro movement only;
- stored-state surface morphing remains a bounded morphology input;
- topology still influences support and negative aging only;
- `Remaining Life` still owns material death;
- the final shader still owns presentation, except for the diagnostic Motion Field debug branch.

The motion field must not spawn Foam, erase Foam, decide lifetime, paint topology, or act as a final visual substitute.

### Validation checklist

Validate first in `Foam Motion Field` debug view. The river should be mostly left/right directional colour with only limited neutral black seams or pockets. Obstacle influence should appear locally around obstacle cells and their upstream approach regions. Foam should overlay in semi-transparent white.

Then validate in `Material Presence` and `Final Foam`: the broad stored body should receive lateral field movement, obstacles should override the lane field near contact, and there should be no upstream push, compression stutter, radial repulsion, early death, birth changes, or topology changes.

Next actionable item after 5.9 validation: `4.11C.5.10 — Better Manual Source Shape Spawning`.

## 2026-07-05 — River Foam 4.11C.5.9e obstacle routing envelope correction

Validation of the 5.9c/5.9d motion-field debug view showed that the dense lane field was now broadly correct, but the static obstacle override field was still too crude. The connected-component routing removed per-cell left/right striping, but the component stamp still produced rectangle-like override slabs, overly strong influence far upstream, excessive influence beside objects that would have passed safely, and continued steering after Foam had cleared the front/side of an obstacle.

5.9e keeps the approved runtime architecture unchanged: `SimulateFoam` still performs two scrolling lane loads plus one fixed obstacle-routing load, with no procedural obstacle math, no local obstacle search, no full-field rebuild from scrolling, and no additional runtime texture samples. The correction is dirty-time only: the CPU-generated obstacle-routing texture now writes a flow-relative collision-risk envelope instead of a broad proximity/rectangle override.

The corrected obstacle rule is: likely collision course gets redirected strongly; Foam passing beside an obstacle receives only weak or no redirection; Foam that has cleared the obstacle releases quickly back to the dense lane field. Influence is shaped by upstream/near/release flow zones, lateral collision-corridor overlap, rounded falloff, and a short downstream release. Directly upstream cells aligned with the obstacle footprint can still reach full override, but side-near cells are deliberately capped low so phase transport can carry them downstream beside the object.

## 2026-07-05 — River Foam 4.11C.5.9f collision-shadow obstacle routing correction

Validation of the 5.9e obstacle-routing envelope showed that the field was still too proximity-driven: it produced outward/rectangular side edges and continued to influence material that should simply phase-transport past the obstacle. 5.9f narrows the obstacle override into a projected upstream collision shadow. The bounds are still used as a cheap dirty-time iteration window, but the written influence is now shaped by collision-corridor overlap, upstream approach distance, a short front-corner skirt, and a tiny downstream release.

The runtime contract is unchanged. `SimulateFoam` still samples two lane values plus one obstacle-routing value and performs no obstacle search, no procedural obstacle math, and no field rebuild from lane scrolling. The obstacle-routing texture generation remains dirty-time CPU work only.


## 2026-07-06 — River Foam 4.11C.5.9g obstacle shadow ramp correction

Validation of 5.9f showed the obstacle routing field was much closer, but three shaping problems remained: a tiny residual field behind obstacles, a weak dip immediately before the obstacle exclusion/negative zone, and a far-approach ramp that became too visible too early. 5.9g keeps the approved two-field motion architecture and changes only dirty-time obstacle-routing texture generation. The downstream release tail is removed, the final valid upstream cells receive a direct-front contact band so they are the strongest part of the collision shadow, and the approach ramp now eases in more slowly before rising sharply near actual collision risk. Runtime simulation cost remains unchanged: two motion-lane loads plus one obstacle-routing load.

## 2026-07-06 — River Foam 4.11C.5.9h field shape calibration

Validation of 5.9g showed that the overall motion-field architecture was working, but two field-content issues remained. First, the dense lane field could still produce large same-direction regions where one lateral colour dominated almost the full river width for a long downstream span. Second, the obstacle collision shadow could still soften immediately before the actual obstacle boundary because its leading-edge logic was based on component-wide bounds rather than row-specific obstacle contact.

5.9h keeps the runtime contract unchanged: `SimulateFoam` still performs two scrolling lane-field loads plus one fixed obstacle-routing load, with no runtime noise evaluation, no runtime obstacle search, no procedural obstacle math, and no field rebuild from lane scrolling. The changes are dirty-time generation only.

The lane generator now gives more authority to medium/high-frequency sign-flipping layers, uses stronger domain warp, and adds additional breaker noise so broad low-frequency regions can no longer decide the sign over huge stretches by themselves. The goal is more granular red/blue intermixing while preserving a coherent scrolling field.

The obstacle routing generator now stores connected-component ids during flood fill and resolves the upstream leading edge per row. The collision shadow is one-sided: the far upstream tip is softened, but the obstacle-facing end is not. The last valid cells before the obstacle footprint remain the strongest part of the collision shadow, and cells at or past the row-specific obstacle leading edge write no routing influence. Runtime cost remains unchanged.

## 2026-07-06 — River Foam 4.11C.5.9i obstacle front-contact closure

Validation of 5.9h showed the Unified Foam Motion Field was close enough to begin functional Foam testing, but two final obstacle-shadow polish issues remained. The main issue was that the collision shadow could end too abruptly one or two cells before the obstacle/negative topology zone. A secondary issue was the appearance of tiny near-zero routing strips outside the main shadow.

5.9i keeps the approved runtime contract unchanged: `SimulateFoam` still samples two scrolling lane-field values and one fixed obstacle-routing value, with no runtime noise evaluation, no runtime obstacle search, no procedural obstacle math, and no rebuild from lane scrolling. The change is dirty-time obstacle-routing generation only.

The direct-front contact band now extends one to two cells toward the obstacle-facing boundary while remaining gated by the collision corridor. This closes the visual/functional gap between the strongest routing region and the obstacle/negative topology zone without translating the entire approach shadow upstream and without restoring a broad side halo. Very small obstacle-routing influences are also discarded so meaningless sliver artifacts do not appear in the debug view or produce tiny lateral impulses.

## 2026-07-06 — River Foam 4.11C.5.9j motion-field material advection fix

After 5.9i, the Foam Motion Field debug shape was acceptable enough to begin functional testing. Testing showed the field did affect Foam at high strength, but the stored material stretched/expanded in the field direction instead of moving as a coherent body. Code inspection found that the simulation sampled the motion field in stored texture coordinates while topology, obstacle exclusion, boundary coverage, and debug visualization used the visible phase-shifted coordinate. The morph pass also blended current material with the advected source sample, which preserved old cells while filling new cells.

5.9j corrects those mechanics. The motion-field resolver now samples at the visible/world `worldSampleCoordinate`. The macro field transport base now uses the advected source sample directly instead of a current/advected blend, and current-cell presence is removed from the movement validity vote. Remaining Life is still the only death authority; this changes relocation semantics rather than adding erosion. The lane generator now stores very low signed motion in neutral coverage regions instead of broad exact-zero areas, preventing black/neutral field patches from acting as hard lateral stops without adding a stored lateral momentum texture. Runtime cost remains the same: two lane loads plus one obstacle-routing load, no runtime noise, no runtime obstacle search, and no field rebuild from scrolling.
