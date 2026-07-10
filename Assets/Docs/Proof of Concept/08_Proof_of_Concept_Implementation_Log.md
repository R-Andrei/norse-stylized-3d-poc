---
document_id: PS3D-08
title: "Proof of Concept Implementation Log"
version: 0.2
status: working-log
scope: proof-of-concept
authoritative_for: "chronological implementation history; current state only in latest entries and canonical architecture docs"
related_documents: [PS3D-06, PS3D-07, PS3D-09, PS3D-10]
last_updated: 2026-07-07
---

# Proof of Concept Implementation Log

## Purpose

Record enough implementation detail to resume work without reconstructing decisions from conversation history.

## Current-state reading rule

This is a chronological log. Older entries record what was implemented or believed at that time. For active River Foam architecture, use `Docs/River_Foam_Stage6_Architecture.md` first and `Docs/River_Foam_Active_Blockers_and_Next_Patches.md` second. Any older log entry implying active stored-state morphing, active lateral row commit, active field-driven lateral material movement, or final shader macro stretch as intended Foam behavior is superseded.

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
- semi-transparent white = raw stored Material Presence overlay, not final Foam mask.

This debug view is intended to be dense. Most of the valid river should be red/orange or blue/cyan. Mostly black output means the field generation or binding is wrong, not merely a tuning preference. Spawned Foam should remain visible as a translucent white raw-presence overlay so field direction and stored material can be validated together without final-mask warp/stretch contamination.

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

## 2026-07-06 — River Foam 4.11C.5.9k phase transport lateral commit

Functional testing after 5.9j showed that Foam still expanded/stretched sideways instead of moving cleanly. The important correction was recognizing that the project already had a true movement model: downstream phase transport commits stored material by copying source cells to destination cells. The motion-field path, however, still lived inside the morph pass as a lateral backtrace sample, so it could smear or refill material instead of relocating it.

5.9k moves explicit field-driven lateral movement into `CommitPhaseTransport`. When a downstream phase commit occurs, each destination cell now gathers from the source column's current row and immediate lateral neighbours. Each source candidate samples the Unified Foam Motion Field at the same visible phase-shifted coordinate convention used by topology and debug, computes a clamped sub-cell lateral landing row, and contributes to the destination only if that source actually lands there. This keeps the operation bounded to three rows, avoids atomics/scatter writes, and makes the lateral field part of real material transport rather than an additive morphology effect. Larger lateral drift accumulates over repeated downstream commits as Foam advances through the field.

The morphology pass no longer samples the motion field for macro lateral movement. It remains responsible for local wobble/reconfiguration and continues to respect the lifecycle contract: Remaining Life owns death, topology owns support/negative aging, and birth creates stable source material only. The dense lane field also receives additional downstream cross-cutting breakup and a higher minimum weak-motion magnitude so Foam advancing downstream is less likely to stay inside one long same-colour ribbon or encounter hard zero-motion strips. No momentum texture, extra field texture, runtime obstacle search, or field rebuild from scrolling was added.

## 2026-07-06 — River Foam 4.11C.5.9m transport diagnostic isolation

Validation after 5.9l showed no reliable lateral material movement: Foam still appeared to pulse and stretch downstream in the Foam Motion Field view. The audit found that the debug view itself was not a clean material diagnostic because it overlaid final `foam.mask`, which includes render-only surface warp and lead/trail stretch. The audit also found that `FoamApplyPersistentMaterialMorph` still spatially resampled neighbouring persistent material every simulation tick, meaning the simulation had a second stored-state deformation path running immediately after phase transport.

5.9m makes two narrow diagnostic changes. First, Foam Motion Field debug now overlays raw stored `Material Presence` from `foam.presence` instead of final `foam.mask`. Second, the persistent morph function now bypasses stored-state spatial resampling and returns the current packed material state clipped to valid fluid; Remaining Life aging still happens in the caller, and final Foam rendering can still apply presentation-only mask breakup/warp. No birth, topology, obstacle routing, lane generation, phase transport, inspector controls, or final normal rendering behavior was changed.

The validation goal is now binary: if raw Material Presence moves laterally, the previous failure was caused by morph/debug contamination; if raw Material Presence still does not move laterally while phase commit telemetry is nonzero, the remaining bug is inside the phase-transport/motion-field sampling path.


## 2026-07-06 — River Foam 4.11C.5.9n persistent morph cleanup

After the 5.9m isolation patch, the remaining cleanup target was the stale persistent morph machinery itself. The audit showed that the old stored-state morph helpers were no longer active after the bypass, but they still existed in `CS_RiverFoam.Simulation.hlsl` and could be reconnected accidentally. 5.9n removes that dead neighbour-resampling machinery and replaces the old morph function with `FoamPreservePersistentMaterialState`, which clamps current packed material and clips it to valid fluid only.

The 5.9n intent was also to remove the unused `Surface Morph Strength` control and compute binding because persistent stored-state surface morphing is no longer an active layer. A later 5.9t audit found that stale C#/Inspector-facing remnants still exist in the uploaded baseline, so that UI/property cleanup remains an active blocker. At the time of 5.9n, this patch did not change phase transport, lane generation, obstacle routing, topology, birth, lifecycle math, or final shader presentation. The then-active source-owned lateral row commit was later rejected and disabled by 5.9p after validation showed it shredded foam at cell scale. The durable result from 5.9n is only the persistent morph cleanup: persistent simulation preserves/clips/ages material and no longer owns morphology.

---

## 2026-07-07 — River Foam 4.11C.5.9p Disable Lateral Commit Shredder

Validation after restoring source-owned lateral row commit showed that the upstream/downstream pulsing was reduced, but the foam still fragmented violently into many small ribbons. The source was the per-texel lateral row-shift decision inside phase transport. Each stored foam texel independently decided whether to stay or move laterally, which shredded visible foam patches at cell scale instead of moving them coherently.

5.9p disables the lateral row-shift path entirely and restores phase transport to downstream-only material movement. This removes the active shredder. It intentionally leaves real lateral material transport missing for now.

Accepted result:

- violent lateral commit tearing stops;
- downstream phase transport remains active;
- persistent morph cleanup remains active;
- Motion Field remains generated/debug-visible but does not move stored material.

## 2026-07-07 — River Foam 4.11C.5.9q Dead Weight Cleanup

Removed only confirmed dead weight:

- `_FoamFlowSpeed` compute uniform and C# bindings;
- unused `RiverWaterResolveFoamColour(...)` helper;
- unused `RiverWaterResolveBodyLighting(...)` helper.

Kept intact:

- Material Flow Speed / `FoamMaterialFlowSpeedMultiplier` downstream transport behavior;
- Foam Colour inspector control and active filtered colour path;
- river lighting controls and active shadow-policy body-lighting path;
- motion-field infrastructure;
- disturbance/static/dynamic field infrastructure.

## 2026-07-07 — River Foam 4.11C.5.9r Foam Cell Grid Debug View

5.9r intended to add a Foam Motion Field + Cell Grid debug view.

The intended view shows:

- Motion Field background;
- obstacle-routing influence;
- raw stored Foam `Presence` overlay;
- actual persistent foam simulation cell/grid boundaries.

This is a diagnostic tool only. It should not alter simulation, transport, morphology, disturbance, or rendering behavior.

A later 5.9t audit did not find the required enum/editor/shader branch in the uploaded baseline, so the cell-grid view remains an active implementation/contract-alignment blocker.

## 2026-07-07 — River Foam Architecture Contract Reset

The Stage 6 Foam docs were rewritten around the new canonical contract:

```text
Two foam data products:
  Persistent Foam State
  Evaluated Foam Shape

Three processing stages:
  Stage 1 — Persistent State Update
  Stage 2 — Shape Evaluation
  Stage 3 — Rendering
```

Important current truth:

- Stage 1 owns birth, transport, lifecycle, and clipping.
- Stage 2 will own coherent deformation, morphology, breakup, and disturbance-reactive visible shape animation.
- Stage 3 owns colour, lighting, opacity, blending, and small final polish.
- Motion Field, Disturbance Fields, and Topology/Support Fields are inputs, not independent foam mutators.
- Desired reference-river tearing belongs to Stage 2 evaluated shape behavior.
- The previous broken tearing was persistent material shredded by cell-scale transport and remains rejected.

Superseded historical entries in this log remain as history only. Any old entry implying active stored-state morphing, active lateral row commit, active field-driven lateral material movement, or final shader macro stretch as the intended Foam behavior is superseded by `River_Foam_Stage6_Architecture.md`.

## 2026-07-07 — River Foam 4.11C.5.9t Compliance Audit Documentation Update

A post-contract audit compared the uploaded foam code against the 5.9s two-product/three-stage architecture contract. No behavior patch was applied in this documentation step.

The audit confirmed that the core persistent simulation is mostly in the intended stripped-down state: persistent material birth remains, downstream phase transport remains active, lifecycle/valid-fluid clipping remain active, persistent neighbour-sampled morphing is removed from the compute simulation path, and the rejected lateral row-commit shredder remains disabled.

The audit also found several contract mismatches in the debug/UI layer that must be fixed before Stage 2 Shape Evaluation work begins:

- `Foam Motion Field` debug is supposed to overlay raw stored Foam `Presence`, but the uploaded shader still overlays final `foam.mask`. This contaminates the transport diagnostic with final-render presentation behavior and is the first required code fix.
- `Foam Motion Field + Cell Grid` is documented as present, but the uploaded code audit did not find the enum/editor/shader branch needed for that view. The preferred correction is to implement it because grid-scale diagnosis remains useful.
- `Surface Morph Strength` still exists in C#/Inspector-facing surfaces even though persistent stored-state morphing is no longer active.
- Some Motion Field labels/tooltips still imply active lateral material movement even though Motion Field is currently only an intent/debug/future input field.
- final shader foam warp/stretch/mask shaping remains temporary Stage 3 debt and should not be expanded as the source of macro morphology.

The active blocker order is now updated in `River_Foam_Active_Blockers_and_Next_Patches.md`. The first implementation item is a narrow debug-truth patch: make `Foam Motion Field` overlay raw stored `Presence` rather than final `foam.mask`.

## 2026-07-07 — River Foam 4.11C.5.9y.2 Reset Shape Morphology and Fix Stage 2 Time

Validation of 5.9y and 5.9y.1 showed that the Stage 2 product exists, but the first morphology attempts were not the right direction. 5.9y proved that `_FoamShapeMask` could generate non-pass-through shape data, but the dense interior hole cuts produced marbled/scratched foam interiors that did not match the reference river. 5.9y.1 removed that interior fragmentation but left only very weak local edge dimming, which spent compute for practically no visible benefit.

5.9y.2 resets Stage 2 morphology to a truthful pass-through baseline: `Foam Evaluated Shape` now writes clipped Persistent `Presence` into `_FoamShapeMask`. The product/debug path remains, but the rejected edge/noise morphology is removed so future work does not build on misleading behavior.

The patch also fixes Stage 2 time binding. `_FoamTime` is now refreshed immediately before `DispatchEvaluateShape()`, instead of relying only on the material simulation configuration path. This prevents future animated Stage 2 shape logic from accidentally inheriting the lower material-update cadence.

The accepted design direction after this reset is field-based and formula-driven, not pocket/entity tracking. Stage 2 should not introduce pocket IDs, connected-component tracking, or per-pocket properties unless field-based deformation fails. The next visual target is coherent field deformation: sample Persistent Presence through a smooth bounded vector field so neighbouring cells in a ribbon receive similar offsets. Naive radius 1/3/5 edge classification is rejected as a default because it costs `179` samples per cell, or about `2.93M` samples for a 128×128 field evaluation. Preferred next algorithms should target about `4–5` samples per cell or use low-resolution/mip-filtered helper fields for bridge/break behavior.

## 2026-07-07 — River Foam 4.11C.5.9z Stage 2 coherent deformation prototype

5.9z turns `Foam Evaluated Shape` from a pass-through product into the first active Stage 2 coherent-deformation prototype. `EvaluateFoamShape` now writes `_FoamShapeMask` by inverse-sampling Persistent Foam Presence through a smooth, bounded deformation field. The field combines low-frequency mathematical motion with read-only Motion Field / obstacle-routing intent so broad sheets and ribbons can bend coherently without pocket IDs or connected-component tracking.

Persistent Foam State remains unchanged: Stage 2 does not write `_FoamStateWrite`, does not move durable Presence, and does not mutate Remaining Life or Material Pattern. Final Foam also remains unchanged and still does not consume `_FoamShapeMask`; validation should compare `Material Presence` against `Foam Evaluated Shape`. The C# runtime now binds the Motion Field lane/routing textures, Motion Field strength, lane scroll, seed, and current time before dispatching `EvaluateFoamShape`.

## 2026-07-07 — River Foam Canonical Architecture Lock

After validation of 4.11C.5.9z, the Foam planning direction was corrected before further implementation. The coherent deformation prototype proved the `_FoamShapeMask` dispatch/binding/product slot, but user comparison of `Material Presence` and `Foam Evaluated Shape` showed no meaningful visible difference. The diagnosis is that a small inverse-sampled coordinate warp can only affect contours of broad solid masks; it cannot create reference-like broad sheet support, visual bridges, pinches, bank/rock skirts, or structural film behavior by itself.

The new canonical architecture is documented in `Docs/River_Foam_Stage6_Architecture.md`. It replaces the ambiguous earlier `Stage 1.5` language with a strict acyclic layer graph:

```text
Layer A — River Domain
Layer B — External Influence Fields
Layer C — Persistent Foam Material
Layer D — Visual Foam / Film Evaluation
Layer E — Shader Composition
Layer F — Scheduling, Quality, Debug
```

The most important correction is dependency ownership. `Layer B` contains foam-agnostic external influence fields: support, contact, suppression, exclusion, motion intent, wake/pressure/ripple context, and similar environmental fields. It may feed `Layer C` and `Layer D`, but it must not read `FoamState`, `_FoamShapeMask`, or any Layer D helper field. Foam-derived sheet/source/support fields belong inside `Layer D` only. This prevents the circular dependency where support fields are computed from foam and then feed the persistent foam simulation again.

Persistent material truth remains `Layer C`. It alone owns durable Presence, Remaining Life, Material Pattern, birth, death, and real material movement. Visual broad film remains `Layer D`. It may visually widen, connect, pinch, soften, bend, and fragment foam, but it writes only visual products such as `_FoamShapeMask`; it must never write persistent material state. Shader composition remains `Layer E`. It should own final color, opacity, soft edges, local procedural breakup, thin streaks, and rendering polish, but must not own broad structural foam connectivity or feed back into compute.

The final solution is therefore not an entity database and not a pure shader trick. It is a fixed-grid mathematical field pipeline: persistent material → foam-agnostic influence → visual film support/shape → shader polish. Local procedural math is still valuable for chipping, fray, cuts, and thin streaks, but true context-aware broad sheet/bridge behavior requires low-resolution Layer D helper fields because a local-only function cannot know whether an empty cell is between two nearby foam bodies or isolated in open water.

Active next work after the docs lock is not another shape-tuning patch. The next implementation should be a compliance/debug pass: audit Layer B/C/D/E read-write boundaries and add a `Foam Shape Difference` debug view so future work can immediately show whether `_FoamShapeMask` differs from raw persistent `Presence`. After that, test local procedural breakup as the cheapest possible visual detail layer, then add low-resolution Layer D film-source/sheet-support helpers for broad structural behavior.

## 2026-07-07 — River Foam 4.11C.5.10 compliance/debug cleanup

This was the first implementation pass after the Foam canonical architecture lock. No final-render Foam behavior was intentionally changed. The purpose was to record the source audit findings, clean stale claims, and make Layer D truth visible before any new visual behavior work.

Source audit findings:

```text
Layer A/B/C/D/E/F ownership is broadly compatible with the canonical acyclic graph.
Layer B external influence generation was not found reading FoamState or _FoamShapeMask.
Layer C persistent material was not found reading _FoamShapeMask.
Layer D still contains the 5.9z coordinate-warp prototype; it writes only _FoamShapeMask and remains a failed/superseded visual approach, not the future solution.
Layer E Final Foam still uses legacy shader-side macro shaping and does not consume _FoamShapeMask.
Old comments/labels overstated accepted disturbance material transport and described Foam Evaluated Shape as pass-through.
Unused wake/pressure material-motion constants remained from abandoned disturbance transport experiments.
```

Implemented cleanup:

```text
Added StylizedRiverFoamDebugView.FoamShapeDifference = 8.
Added editor labels and descriptions for Foam Shape Difference.
Added shader debug branch: black means _FoamShapeMask matches raw Material Presence, green means evaluated shape adds coverage, magenta/red means evaluated shape removes coverage.
Updated Foam Evaluated Shape description to identify the current 5.9z coordinate-warp as a failed/superseded prototype.
Updated Water Body help text to describe the Layer A-F split and stop claiming active lateral disturbance material transport.
Removed unused WakeMotionInfluence, PressureMotionInfluence, and TransportMaximumAxisCourant constants.
Replaced IsEvaluatedShapeDebugActive with IsShapeProductDebugActive so both Evaluated Shape and Shape Difference request the Layer D product.
Gated DispatchEvaluateShape behind Layer D debug use, because Final Foam still does not consume _FoamShapeMask.
```

Known remaining caveats:

```text
No low-res Film Source / Film Support helpers exist yet.
Transition-hold fallback for _FoamShapeMask may still be product-imprecise during topology transitions.
Shader-side Final Foam still owns legacy macro shaping until Layer D earns the production switch.
```

Current active direction after later validation: the Layer D local-breakup probe was tested in `4.11C.5.11` and retired in `4.11C.5.11B` because it exposed cell/ribbon artifacts. The next active implementation direction is a Layer E shader-side local-detail probe, followed by low-res Layer D Film Source / Film Support for broad structural sheet behavior.



## 2026-07-07 — River Foam 4.11C.5.10B Retire Failed Shape Warp Baseline

Validation of `4.11C.5.10` confirmed that the new `Foam Shape Difference` view works and that the 5.9z coordinate-warp prototype was numerically active: the difference view showed strong green/magenta signed bands where `_FoamShapeMask` differed from raw persistent `Presence`. However, the normal `Material Presence` and `Foam Evaluated Shape` views still looked and behaved basically identical. Final Foam also remained unchanged, as intended, because Final Foam still does not consume `_FoamShapeMask`.

The conclusion is now recorded as evidence rather than speculation:

```text
5.9z changed values.
5.9z did not create useful visible structure.
A coordinate warp can produce signed differences without changing the readable foam silhouette/behavior.
```

`4.11C.5.10B` therefore retires the failed 5.9z warp as active code. `EvaluateFoamShape` is reset to pass-through clipped persistent `Presence`, and the coordinate-warp helper functions are removed. `DispatchEvaluateShape()` no longer binds Motion Field lane or obstacle-routing textures for the baseline shape pass because the pass-through baseline does not read them.

This restores a clean Layer D baseline:

```text
Material Presence ~= Foam Evaluated Shape
Foam Shape Difference ~= black
```

That baseline is intentional. `4.11C.5.11` then tested the local procedural breakup idea inside Layer D and `4.11C.5.11B` retired it after validation showed cell/ribbon-shaped artifacts. Future Layer D work must now prove broad structural benefit explicitly through low-resolution Film Source / Film Support; fine breakup should be tested in Layer E shader composition.

## 2026-07-07 — River Foam 4.11C.5.11 Local Procedural Breakup Probe

`4.11C.5.11` implements the first intentionally isolated Layer D local-breakup probe after `4.11C.5.10B` restored the clean pass-through baseline. The goal is to test the cheapest possible no-neighbour visual breakup layer before adding low-resolution structural Film Source / Film Support fields.

Implementation facts:

```text
EvaluateFoamShape remains the only writer of _FoamShapeMask.
Persistent FoamState is not written by the probe.
Remaining Life and Material Pattern are read but not modified.
Final Foam is not changed and still does not consume _FoamShapeMask.
No pocket IDs, connected components, entity records, neighbour FoamState sampling, Motion Field bias, obstacle-routing bias, topology support reads, or low-res helper fields are introduced.
```

New compute-side local helpers in `CS_RiverFoam.compute`:

```text
FoamResolveMaterialPhysicalPosition(...)
FoamEvaluateLocalBreakupField(...)
FoamEvaluateLocalProceduralBreakupShape(...)
```

`DispatchEvaluateShape()` now binds `_FoamTime`, `_FoamSeed`, `_FoamGlobalStart`, `_FoamFieldLength`, and `_FoamMetricRows` to the shape-evaluation kernel because the local procedural field is animated and physical-space based. This is still gated behind Layer D debug use, so the probe does not run in normal Final Foam view.

Validation expectation:

```text
Material Presence = raw persistent material truth.
Foam Evaluated Shape = local procedural breakup product.
Foam Shape Difference = non-black where the probe removes visible shape coverage, mostly around contours/fragile areas.
Final Foam = unchanged.
```

The probe is not expected to solve broad bank-hugging film, contact sheets, bridge/rejoin behavior, or context-aware merging. Those remain the job of future low-resolution Layer D Film Source / Film Support fields if local-only breakup proves insufficient.

## 2026-07-07 — River Foam 4.11C.5.11B Retire Layer D Local Breakup Probe

Validation of `4.11C.5.11` confirmed that the local breakup probe was active: `Foam Shape Difference` became clearly non-black, mostly magenta/removal. However, the removals appeared as long simulation-cell or ribbon-shaped gaps. The result exposed `_FoamShapeMask` cell scale instead of producing the granular, almost atomic breakup seen in the inspiration river.

Conclusion:

```text
Layer D local-only breakup is rejected as the fine-fragmentation solution.
The failure is not inactivity; it is a layer/resolution mismatch.
_FoamShapeMask is appropriate for macro film structure, not per-pixel granular edge damage.
Fine breakup, tiny cuts, and thin streaks belong in Layer E shader composition.
Layer D should focus on broad sheets, contact support, bridge/pinch/split, and smooth macro shape.
```

`4.11C.5.11B` retires the active Layer D local-breakup code. `EvaluateFoamShape` is reset to pass-through clipped persistent `Presence`, and the local helpers are removed from `CS_RiverFoam.compute`:

```text
FoamResolveMaterialPhysicalPosition(...)
FoamEvaluateLocalBreakupField(...)
FoamEvaluateLocalProceduralBreakupShape(...)
```

`DispatchEvaluateShape()` no longer binds `_FoamTime`, `_FoamSeed`, `_FoamGlobalStart`, `_FoamFieldLength`, or `_FoamMetricRows` for the baseline shape pass. The intended validation state returns to:

```text
Material Presence ~= Foam Evaluated Shape
Foam Shape Difference = black or effectively black
Final Foam = unchanged
```

Current active implementation direction after this cleanup:

```text
1. Test Layer E shader-side local detail for sub-cell chipping/fray/thin streaks.
2. Add low-res Layer D Film Source / Film Support for broad sheet/contact/bridge behavior.
3. Integrate accepted macro support into _FoamShapeMask.
4. Switch Final Foam to _FoamShapeMask only after Layer D visibly outperforms current final foam.
```

## 2026-07-08 — River Foam 4.11C.5.12 Layer E Shader-Side Local Detail Probe

`4.11C.5.12` implements the first shader-side Layer E local-detail probe after `4.11C.5.11B` restored the clean pass-through Layer D baseline. This patch deliberately does not mutate `FoamState`, does not mutate `_FoamShapeMask`, and does not affect Final Foam. It only adds debug views that sample the clean Layer D mask and apply local rendered-pixel procedural detail in the water shader.

Added debug modes:

```text
Foam Shader Detail Probe
Foam Shader Detail Difference
```

Code-level ownership notes:

```text
StylizedRiverFoamDebugView now includes FoamShaderDetailProbe = 9 and FoamShaderDetailDifference = 10.
IsShapeProductDebugActive includes those modes so the pass-through _FoamShapeMask is current while the shader detail probe samples it.
RiverWaterFoamResult now exposes materialPattern to shader debug/detail code.
RiverWaterFoamEvaluateShaderLocalDetailProbe(...) lives in RiverWaterFoam.hlsl and applies local, no-neighbour detail using river metres, material UV, material pattern, Remaining Life, time, sharpness, and surface energy.
SH_CleanStylizedRiver.shader handles debug modes 9 and 10 by comparing the shader-detailed result against the base _FoamShapeMask.
```

Validation expectation:

```text
Material Presence and Foam Evaluated Shape remain the clean Layer C/Layer D baseline.
Foam Shape Difference remains black unless Layer D changes.
Foam Shader Detail Probe shows only Layer E pixel-scale detail over _FoamShapeMask.
Foam Shader Detail Difference shows granular local removal/addition relative to _FoamShapeMask.
Final Foam remains unchanged.
```

This probe is not intended to solve broad sheets, bank-hugging film, contact support, bridge/rejoin, or macro split/merge. Those remain the job of future low-resolution Layer D Film Source / Film Support.



## 2026-07-08 — River Foam 4.11C.5.13 Low-Resolution Layer D Film Source / Film Support

Implemented the first structural Layer D helper-field prototype. The runtime now allocates half-resolution RHalf textures for `_FoamFilmSource` and `_FoamFilmSupport`, builds Film Source from persistent material plus external support/contact fields, spreads Film Support directionally with fixed low-cost taps, and uses the result in `EvaluateFoamShape` to write `_FoamShapeMask`. Added `Foam Film Source` and `Foam Film Support` debug views.

The dependency contract remains intact: Layer D reads Layer C and Layer B but writes only visual products. No persistent `FoamState`, Remaining Life, Material Pattern, Layer B fields, or Final Foam output is mutated by this patch.

### 2026-07-08 — River Foam 4.11C.5.13B Layer D domain-space sampling fix

After validating the first Film Source / Film Support prototype, the Layer D debug views were found to pulse in sync with the material cell grid. The root cause was coordinate ownership: Layer D domain-support products were sampled through materialUV, which includes residual phase travel and snaps after integer material commits.

`4.11C.5.13B` fixes this by treating `_FoamFilmSource`, `_FoamFilmSupport`, and `_FoamShapeMask` as domain-space visual products. Build/evaluate kernels sample persistent FoamState from phase-corrected material coordinates but sample Layer B support/contact fields in domain coordinates. Shader debug views now sample Layer D products using `foam.fieldUV`. Final Foam remains unchanged.


### 2026-07-08 — River Foam 4.11C.5.13C Material-Gated Film Source

Implemented `4.11C.5.13C` as a semantic correction to the Layer D film pipeline. Validation after `5.13B` showed the coordinate-space stutter was fixed, but many Layer D-derived debug views still reproduced support topology shapes. Source audit found the root cause in `BuildFoamFilmSource`: support topology could become Film Source directly.

The patch changes Film Source to be material-derived only. Layer B support/contact/topology now biases or suppresses material-derived source/spread but cannot create visual film from zero. `BuildFoamFilmSupport` now binds topology and topology-source textures for bias/suppression during spread. Final Foam remains unchanged.


### 2026-07-08 — River Foam 4.11C.5.13C Unity validation and 5.13D documentation plan

Validated `4.11C.5.13C` in Unity. The material-gated Film Source correction worked: support topology no longer appears as material-derived Film Source, and Layer D-derived views no longer reproduce the support-topology shapes that previously contaminated Film Source, Film Support, Evaluated Shape, Shape Difference, and shader-detail probes.

Current correct interpretation:

```text
Foam Film Source = half-resolution material-derived visual source.
Foam Film Support = half-resolution spread/support field fed by Film Source.
Foam Evaluated Shape = full-resolution domain-space visual interpretation.
Foam Shape Difference = signed difference against material presence.
Final Foam = unchanged legacy final shader path.
```

Remaining issue after validation: Film Support is semantically clean but visually primitive. It behaves like a broad low-resolution dilation/capsule around the material ribbon. This is now the next active problem.

Documented next target: `4.11C.5.13D — Layer D Film Spread Shape Tune`.

`5.13D` must tune source thresholds, cross-flow spread, along-flow continuity, bridge thresholds, and final support contribution. It must not change Final Foam, reintroduce support-only Film Source, add environmental contact film, mutate persistent material, add entity tracking, or tune shader-side detail.

### 2026-07-09 — River Foam 4.11C.5.13D Layer D Film Spread Shape Tune

Implemented `4.11C.5.13D` as a narrow Layer D compute tuning pass. The patch edits only `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute` on the code side.

The patch keeps the `5.13C` material-gated Film Source contract intact: persistent material creates Film Source, and Layer B support/contact/topology can only bias or suppress material-derived film. It does not switch Final Foam to `_FoamShapeMask`, does not add environmental contact film, does not add Inspector controls, and does not mutate `FoamState`.

Changed behavior:

```text
FoamResolveVisualFilmInfluenceAtDomainUV:
  supportBias reduced from 0.90-1.18 to 0.94-1.08.

BuildFoamFilmSupport:
  along-flow continuity remains dominant;
  cross-flow spread is weaker and gated by local/axial source evidence;
  diagonal spread is weaker;
  bridge thresholds are stricter;
  bridge contribution is lower.

EvaluateFoamShape:
  supportShape threshold is stricter;
  supportShape contribution is lower;
  sourceShape threshold/contribution are slightly more conservative.
```

Expected validation:

```text
Foam Film Source remains material-derived.
Foam Film Support is still broader than source but less uniformly capsule-like.
Foam Evaluated Shape adds macro visual coverage more selectively.
Foam Shape Difference shows smaller and more selective green additions than 5.13C.
Final Foam remains unchanged.
```


### 2026-07-09 — River Foam 4.11C.5.14A Layer C Automatic Shore/Contact Source Population

Audited the current birth/spawn architecture after validating that one central manual ribbon is not enough to judge the full foam plan. The audit result is that manual/progressive birth exists, support/lifetime capture exists, Layer B support/contact topology exists, and Layer D material-derived spread exists. The missing piece is automatic source population: no current code path samples shore, rock, wake, pressure, connector, or support fields to create persistent material births automatically.

`4.11C.5.14A` adds the first conservative automatic source class without adding a new visual-film authority. The new path is disabled by default and creates real Layer C `FoamState` material through the existing `PendingInjection` / `QueueMaterialBirth` / `InjectFoam` path. Support topology then preserves or suppresses the born material through the existing Remaining Life rules.

Implemented code changes:

```text
StylizedRiver:
  FoamAutomaticBirthEnabled
  FoamAutomaticShoreBirthAmount

StylizedRiverEditor:
  Source Population foldout under Foam Debug / Controls.
  Runtime accepted/rejected/budget/total diagnostics.

StylizedRiverFoamRuntime:
  low-rate automatic shore/contact candidate scan;
  conservative per-quality birth budget;
  alternating shore-side candidate placement;
  sparse stochastic acceptance;
  real PendingInjection queueing through existing material birth pipeline;
  status counters and sleep/material-work gating.
```

Implementation constraints:

```text
Automatic birth is off by default.
Support/topology still cannot render as foam from zero material.
No Final Foam integration changed.
No new Environmental Contact Film product was added.
No pocket IDs, entity tracking, or connected-component database was added.
Layer D remains material-derived visual interpretation only.
```

Expected validation:

```text
With Automatic Birth disabled, behavior matches 5.13D.
When enabled in Play Mode, Material Presence begins receiving sparse shore/contact material births.
Material Remaining Life should show supported shore/contact births living longer than unsupported spill.
Foam Film Source and Foam Film Support should derive from those real births.
Final Foam remains unchanged until explicitly integrated later.
```

### 2026-07-09 — River Foam 4.11C.5.14B Source Population Controls / Shore Birth Profile

Validated `4.11C.5.14A` enough to prove the architecture path: automatic source population can create real Layer C material, support/lifetime capture can preserve shore material, and the material/film debug views respond correctly. The validation also exposed a design problem: `Shore Contact Birth Amount` was overloaded and produced large blocky chunks at a moderate value such as `0.35` because it controlled density, footprint, amount, life, elongation, and compound shape together.

`4.11C.5.14B` correctly moved to source-class-specific spawning, but exposed too many low-level controls in the Inspector. This was rejected as authoring bloat before further visual validation.

### 2026-07-09 — River Foam 4.11C.5.14C Simplified Shore Spawn Controls

`4.11C.5.14C` keeps the same Layer C material-birth architecture and the same source-class-specific plan, but simplifies shore spawning to a deterministic shore recipe with four plain controls:

```text
Coverage      shoreline coverage/frequency over time
Size          individual shore seed/stroke footprint
Strength      initial material visibility
Persistence   initial Remaining Life before support capture dominates
```

Implemented code changes:

```text
StylizedRiver:
  Replaced the low-level shore birth fields with Coverage, Size, Strength, and Persistence.

StylizedRiverEditor:
  Source Population foldout now shows Automatic Foam Birth, Spawn Preset, and the four Shore Foam controls only.

StylizedRiverFoamRuntime.BirthEvents:
  Shore Contact Birth now resolves a hidden deterministic stroke recipe from the four controls.
  Compound shore births are no longer available through the shore test path.
  Candidate randomness no longer uses wall-clock time; candidate variation is deterministic from river seed, candidate identity, and repeat cycle.

StylizedRiverFoamRuntime.Constants:
  Shore candidate spacing is wider and acceptance is lower to avoid constant overpopulation.
```

Implementation constraints:

```text
Automatic birth remains globally gated.
Shore Contact Birth is still the only implemented automatic source class.
River Body, Obstacle Contact, Lee/Wake, and full mixed spawning are not implemented yet.
Support/topology still cannot render as foam from zero material.
No Final Foam integration changed.
No Environmental Contact Film product was added.
No entity tracking, pocket IDs, or connected-component database was added.
```

Expected validation:

```text
With Automatic Foam Birth disabled, behavior matches the previous baseline.
With Automatic Foam Birth enabled and Spawn Preset set to Shore Contact Test, Material Presence / Material Remaining Life should show small deterministic shore flecks/strokes.
At Coverage around 0.35, births should not become river-wide chunks.
Changing Coverage should change how much shoreline receives births over time, not the footprint of each seed.
Changing Size should change footprint while keeping material near-shore.
Changing Strength should affect material visibility.
Changing Persistence should affect initial survival before support capture.
Foam Film Source and Foam Film Support should derive from real material births.
Final Foam remains unchanged.
```

### 2026-07-09 — River Foam 4.11C.5.14D Deterministic Shore Source Events

Validation of `4.11C.5.14C` showed that the simplified shore UI was an improvement, but the hidden shore recipe was still wrong. Even with all visible shore controls at maximum, it produced too little material, the events were isolated and same-shaped, and visible births still read as one-shot material placement.

The patch keeps the accepted architecture and replaces the implementation model. Automatic shore birth remains Layer C source population: it creates real persistent `FoamState` material through the existing progressive composition / injection path, and existing support/lifetime capture determines survival. No visual-only environmental film layer was added, support topology still does not render as foam by itself, Layer D is unchanged, and Final Foam remains unchanged.

Implemented changes:

```text
Source Population UI now exposes:
  Coverage
  Activity
  Patch Size
  Pattern

Implemented shore patterns:
  Mixed
  Shore Ribbons
  Inward Wash
```

`Coverage` controls deterministic slot eligibility along both banks. `Activity` controls how often new shore source events start. `Patch Size` controls the scale of each event. `Pattern` selects the deterministic recipe. Strength/Persistence are no longer exposed for shore testing; automatic shore recipes use normal-strength material values and existing support capture for survival.

The source implementation now uses deterministic shore slots distributed across both banks. Accepted slots start short progressive source events rather than one-shot patch injections. Events spawn normal-strength material but reveal their area spatially over the event duration, avoiding the rejected many-faint-deposits model.

Validation target: in `Material Presence` and `Material Remaining Life`, `Pattern = Shore Ribbons` should show bank-parallel opaque source events; `Pattern = Inward Wash` should show shore-attached inward/downstream tongues; `Pattern = Mixed` should deterministically alternate both. Events should distribute across the chunk over time, not appear only in one or two places, and Final Foam should remain unchanged.


### 2026-07-09 — River Foam 4.11C.5.14E Automatic Source Event Rasterizer

Validation of `4.11C.5.14D` failed visually. With Coverage and Activity at maximum, shore foam still read as predictable rectangular bars near the shore, Pattern `Shore Ribbons` and `Inward Wash` were not clearly different, and coverage remained insufficient. The implementation diagnosis was that both patterns still became generic progressive composition segments and then `PendingInjection` / `InjectFoam` segment capsules.

`4.11C.5.14E` keeps the accepted Layer C source-population architecture but replaces automatic shore output with a dedicated source-event rasterizer. Added a bounded automatic source-event buffer, GPU `FoamSourceEventData`, and `RasterizeFoamSourceEvent`. Automatic shore slots now create typed `ShoreRibbon` or `InwardWash` events; the compute kernel reads `_FoamCurrentShoreEdgesRead`, evaluates shore-local analytic masks, and writes real persistent `FoamState` material through `FoamMergeBornPresence`.

The manual/debug `PendingInjection` / `InjectFoam` path remains intact. Layer D Film Source/Support formulas and Final Foam remain unchanged.

Validation target: in `Material Remaining Life`, `Pattern = Shore Ribbons` should show thin bank-following ribbons rather than rectangular segment stamps; `Pattern = Inward Wash` should show shore-attached inward/downstream tongues; `Pattern = Mixed` should show both. Then confirm `Foam Film Source` and `Foam Film Support` follow the new material while `Final Foam` remains unchanged.

### Patch 4.11C.5.14F — Source Formation Kinematics / Stroke Wash

Implemented after the first dedicated automatic source-event rasterizer produced better shore attachment but still formed too fast and made Inward Wash read as broad blobs.

Changes:

- Added `foamShoreFoamFormationSpeedMetresPerSecond` to `StylizedRiver` and surfaced it in the Source Population / Shore Foam inspector as `Formation Speed`.
- Added formation speed and moving-head trail data to `AutomaticFoamSourceEvent` / `FoamSourceEventGpuData`.
- Replaced fixed 0.45–1.10 second source durations with distance-derived durations based on path length divided by formation speed.
- Reworked `FoamEvaluateInwardWashSource` from a filled shore-to-reach mask to a sampled moving curved stroke-head.
- Reduced Inward Wash stroke width and source fill feature size so the class reads closer to a curved filament instead of a filled patch.

Validation focus: Material Remaining Life first. Final Foam intentionally remains unchanged.

### Patch 4.11C.5.14G — Shore Wash Stroke Refinement

Implemented after 5.14F validation showed the formation speed control was a major improvement but Inward Wash still produced compact slab/card-like patches.

Changed:

- Added wash-specific head-trail constants so Inward Wash no longer reuses ribbon-sized drawing bodies.
- Reduced Inward Wash length, width, inward reach, lifetime, breakup scale, and breakup strength.
- Changed the Inward Wash curve so it follows the shore first and then peels inward.
- Reduced wash stroke/feather inflation and source-fill influence in the compute rasterizer.
- Reduced `Mixed` Inward Wash weight from 38% to 12%.

Validation target: in `Material Remaining Life`, `Pattern = Shore Ribbons` should not regress; `Pattern = Inward Wash` should show smaller shore-detachment strokes instead of broad slabs; `Pattern = Mixed` should be mostly ribbons with occasional small wash strokes. Final Foam remains unchanged and out of scope.


### Patch 4.11C.5.14H — Foam Birth Source Authoring Framework

Implemented after 5.14G validation showed Shore Foam spawning was crude but now plausible enough to tune through controls. This patch turns the shore source recipe from hardcoded experimental values into an explicit authoring framework.

Changed:

- Source Population inspector is now `Foam Birth Sources`.
- Added category foldouts for `Shore Foam`, `Object Foam`, and `Free Water Foam`. Shore Foam is implemented; Object and Free Water are visible disabled placeholders for future source classes.
- Added normalized Shore Foam pattern shares for `Shore Ribbons` and `Inward Wash`. Editing either share updates the other so the sum remains one.
- Added per-pattern controls for Formation Speed, Length, Width, Shore Offset / Inward Reach, Initial Life, and Breakup Strength.
- Replaced hardcoded shore recipe dimensions, initial life, breakup strength, and mixed-pattern ratio with these controls.
- Changed event dimension sampling to use correlated event scale plus small per-axis jitter and aspect guards.

Important definition: `Initial Life` is the normalized Remaining Life assigned to newly spawned persistent FoamState material. It is not event duration; formation duration remains path-distance divided by formation speed.

Validation target: Material Remaining Life first. Confirm Shore Ribbon and Inward Wash controls affect only their own patterns, Mixed pattern shares remain normalized, and Final Foam remains unchanged.

### 4.11C.5.15A — Static Object Contact Foam Birth

Enabled the Object Foam source category for Layer C birth validation. Static object sources are exported from the disturbance runtime and scheduled on CPU as bounded source events. The existing source-event rasterizer now supports Object Contact Arc and Object Contact Fleck types and writes real FoamState material, gated by obstacle exclusion and static pressure contact support.

### 4.11C.5.15A.1 — Object Birth Activation Wiring Fix

Fixed Object Foam activation after validation showed the runtime status remained `Object source population disabled` even with Object Foam enabled. Shore/Object category active properties no longer use hidden preset-specific gates. The preset now only globally disables automatic birth when set to Off; source categories are controlled by their own Enabled toggles. Added Object Foam source anchor diagnostics to the inspector.

### 4.11C.5.15A.2 — Object Contact Edge Field

Implemented after Object Foam successfully spawned but showed rectangular/slab-like contact patches. Added a GPU object contact field built from obstacle exclusion and static pressure. The field stores contact confidence, contact normal, and front/side relevance. Object Contact Arc and Contact Fleck source events now shape against this field in contact normal/tangent space, while object extents remain coarse scheduling/bounding data only.

Validation target: Material Remaining Life should show less rectangular object contact foam, with arcs/flecks hugging actual obstacle contact regions and no material inside obstacle footprints.

### 4.11C.5.15A.3 / 5.15A.3.4 — Object Contact Field Recovery

The attempted object-contact edge-distance correction failed because compute resource declarations/bindings and C# fallbacks were not consistently updated as a complete file set. Recovery restored the stable object-contact field path and fixed the `_FoamObjectContactFieldRead` declaration/binding/fallback sequence. Current Object Foam therefore remains based on the 5.15A.2 broad contact field rather than a sharper distance-edge field.

### 4.11C.5.15A.4 — Object Contact Semi-Arc Pattern

Added `Contact Semi-Arcs` to Object Foam as a third Layer C source recipe. Mixed Object Foam now has normalized three-way weights for Contact Arcs, Contact Semi-Arcs, and Contact Flecks. Semi-Arc events use existing source-event data and store deterministic signed lopsidedness in `Curvature` / GPU `variation.w`; no new compute texture or binding was added. The compute rasterizer adds `FoamEvaluateObjectContactSemiArcSource`, using a one-sided tangent interval so contact foam can appear as a lopsided shoulder arc instead of only a symmetric bracket.

Validation remains in `Material Remaining Life`: compare pure Contact Arcs, pure Contact Semi-Arcs, pure Contact Flecks, and Mixed. Final Foam remains unchanged.

### 4.11C.5.15B — Free Water Lace / Fragment Birth

Implemented Free Water Foam as a real automatic Layer C birth source category. Added Lace Connector head+stroke events and Torn Fragment progressive swept patch events. Added bounded deterministic open-water slot scheduling, two-pattern authoring controls, runtime diagnostics, and Y-range dispatch clipping for local automatic source events. This patch intentionally avoids spawning final-render glints or rectangular/sheet decals as persistent foam material.

### 4.11C.5.15B.2 — Free Water Cross-Lace Connectors

Added Cross-Lace Connectors as the third Free Water Foam birth pattern. The existing Lace Connector samples its path along the flow axis, so free-water birth produced mostly vertical/with-flow marks. Cross-Lace uses the same timed head+stroke insertion mechanic but samples along the lateral river axis, producing horizontal/cross-current ribbons while only bending slightly along flow. Coverage/Activity were intentionally left unchanged; any longer survival should be validated through Initial Life / lifetime tuning separately.

