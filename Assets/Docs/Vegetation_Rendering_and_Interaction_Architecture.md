# Vegetation Rendering and Interaction Architecture

## Status

**Planning baseline — 2026-07-18**

This document defines the initial production architecture, measurable performance targets, visual contract, and validation gates for dense interactive vegetation in the Norse Stylized 3D PoC.

It is a design and implementation target, not a claim that the vegetation system is already implemented or Unity-validated.

```text
Engine: Unity 6.5
Render pipeline: URP
Primary target: PC
Target output: 2560 × 1440
Target frame rate: 60 FPS
Target frame budget: 16.67 ms
Camera: constrained top-down isometric gameplay camera
Vegetation goal: tall, dense, continuous-looking grass patches
Interaction goal: wind and actor disturbance across the full visible field
```

---

## Active implementation plan — V0 dense-grass benchmark

**Status: Implemented locally — Unity import, compilation, visual review, and profiling pending**

### Objective

Implement an isolated dense-grass benchmark that selects a viable cluster representation at the production camera and establishes a replaceable wind-consumer boundary. V0 does not integrate with GeneratedGround, actors, trails, trees, flames, or the future Weather/Wind system.

### Reviewed evidence

| Evidence | Finding | Status |
| --- | --- | --- |
| `AGENTS.md` | Documentation must be updated before implementation; new folders and files require approval; one comprehensive clipboard report is required by project workflow. | Reviewed |
| `Docs/Vegetation_Rendering_and_Interaction_Architecture.md` pre-edit baseline | Defines 1440p/60 FPS targets, 12/16/20 clusters per m², three geometry candidates, no baseline grass shadow pass, and benchmark-first sequencing. | Reviewed |
| `Docs/Stylized_Vegetation_Architecture.md` | Describes wind as a shared field rather than vegetation-owned state. | Reviewed |
| `Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader` | Confirms URP shader conventions used by the project. V0 does not modify shared PixelSurface shaders. | Reviewed |
| `Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs` | Confirms the project convention of Inspector-triggered comprehensive diagnostics copied through `EditorGUIUtility.systemCopyBuffer`. | Reviewed |
| Unity 6000.5 `Graphics` API documentation | `Graphics.RenderMeshIndirect` is the current GPU-instanced indirect rendering API; obsolete indirect APIs must not be used. | Reviewed |
| Supplied archive | No `.git` directory is present, so Git status, history, and comparison with `HEAD` are unavailable. The supplied archive is the authoritative pre-edit baseline for this patch. | Limitation recorded |

### Approved V0 files

```text
Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Docs/Stylized_Vegetation_Architecture.md
Game/Procedural/Vegetation/VegetationBenchmark.cs
Game/Procedural/Vegetation/VegetationInstanceData.cs
Game/Procedural/Vegetation/VegetationClusterMeshBuilder.cs
Game/Procedural/Vegetation/VegetationBenchmarkWindProvider.cs
Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs
Game/Rendering/Vegetation/Includes/VegetationCommon.hlsl
Game/Rendering/Vegetation/Includes/VegetationWindResponse.hlsl
Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader
corresponding `.meta` files
```

The user approved the new Vegetation folders and files in the preceding architecture review.

### V0 invariants and non-goals

- Weather/Wind owns authoritative wind direction, strength, gust propagation, visible stylized gusts, and cross-system wind state.
- Vegetation owns only blade response to external wind input.
- `VegetationBenchmarkWindProvider` is temporary, test-only, and replaceable. It must not become a production weather manager.
- No GeneratedGround, River, tree, flame, scene, prefab, material, layer, tag, or renderer-pipeline asset changes.
- No per-cluster GameObjects, Transforms, colliders, or normal-frame placement rebuilds.
- No grass ShadowCaster pass.
- No actor interaction or persistent trails in V0.
- No shared shader/include changes.

### File-by-file implementation sequence

| Item | File(s) | Required result | Status |
| --- | --- | --- | --- |
| V0.0 | Both vegetation documents | Record approved scope, wind ownership, evidence, risks, acceptance, later patches, and validation; mark the earlier exploratory document as superseded where it conflicts. | Complete |
| V0.1 | `VegetationInstanceData.cs` | Define one explicit packed CPU/GPU instance record and stride. | Complete |
| V0.2 | `VegetationClusterMeshBuilder.cs` | Deterministically build opaque-strip, crossed-card, and hybrid benchmark meshes with reported vertex/triangle counts. | Complete |
| V0.3 | `VegetationBenchmarkWindProvider.cs` | Publish temporary external wind globals; clearly identify test-only ownership. | Complete |
| V0.4 | HLSL includes and benchmark shader | Decode instances, apply vegetation-specific response to external wind, render URP forward lighting, and omit ShadowCaster. | Complete; Unity shader import pending |
| V0.5 | `VegetationBenchmark.cs` | Generate deterministic rectangular placement, create/release buffers and runtime material, submit `RenderMeshIndirect`, and expose one comprehensive report. | Complete; Unity runtime validation pending |
| V0.6 | `VegetationBenchmarkEditor.cs` | Provide explicit rebuild and clipboard-copy controls without expensive automatic rebuilds. | Complete; Unity Inspector validation pending |
| V0.6a | `VegetationBenchmark.cs`, `VegetationBenchmarkEditor.cs` | Run the complete 3 geometry × 3 density matrix from one Inspector action, consolidate all structural reports, and restore the original live configuration. | Complete; Unity Inspector validation pending |
| V0.7 | All approved files | Complete syntax/static checks, scope audit, final reread, and record limitations and Unity validation status. | Local audit complete; Unity compilation/profiling pending |

### V0 acceptance criteria

- Deterministic presets for 12, 16, and 20 clusters/m².
- Three geometry candidates can be compared under the same area, density, camera, lighting, and external wind input.
- Every rendered blade uses root-anchored response to external wind across the whole benchmark field.
- With no test provider active, the grass remains static; vegetation does not synthesize authoritative wind.
- GPU indirect instance submission; no per-instance GameObject or normal-frame CPU placement work.
- Grass shadow casting is disabled and the shader contains no ShadowCaster pass.
- One Inspector button copies density, area, instance count, geometry counts, draw calls, buffer bytes, deterministic hash, bounds, wind-provider status, and current settings.
- No normal-frame managed allocation attributable to placement generation.
- Changed C# and HLSL files pass all available local syntax/static checks. Unity compilation and 1440p profiling remain pending until imported into Unity 6000.5.0f1.

### Risks

- Exact GPU cost and visual density cannot be established outside Unity and the selected low-end reference PC.
- `RenderMeshIndirect` requires compute-shader-capable hardware. The benchmark must report unsupported platforms and avoid rendering rather than silently switching architecture.
- Broad card candidates can be overdraw-bound; equivalent apparent coverage is required for a fair comparison.
- The final Weather/Wind GPU contract is intentionally undefined. V0 uses narrow global inputs that can be replaced later.

### V0 post-implementation consistency and compliance audit

**Local audit result: Pass with Unity validation pending.**

| Check | Evidence | Result |
| --- | --- | --- |
| Approved scope | Final files are limited to the two vegetation documents, approved Vegetation runtime/editor/shader files, and corresponding `.meta` files. No Ground, River, Weather, scene, prefab, material, layer, tag, or renderer-pipeline file changed. | Pass |
| Wind ownership | `VegetationBenchmarkWindProvider.cs` is explicitly V0-only; shader globals are named external wind inputs; `VegetationWindResponse.hlsl` applies response only; no `VegetationWind.hlsl` or production weather manager was added. | Pass |
| Rendering API | `VegetationBenchmark.cs` uses `Graphics.RenderMeshIndirect`, `GraphicsBuffer.IndirectDrawIndexedArgs`, a structured instance buffer, one command, compute-support rejection, `RenderParams.worldBounds`, `GetEntityId()`, shadow casting off, and receiving shadows off. | Pass by source inspection; Unity execution pending |
| Instance contract | CPU data is three `Vector4` values with declared 48-byte stride; runtime stride is checked before buffer creation; HLSL declares the same three `float4` values in the same order. | Pass by source inspection |
| Normal-frame work | `LateUpdate` updates one matrix, creates one value-type `RenderParams`, and submits one indirect draw. Placement arrays, meshes, materials, reports, and buffers are created only during explicit rebuild/enable lifecycle. Provider `Update` publishes fixed shader globals only. | Pass by source inspection; profiler confirmation pending |
| Resource lifecycle | Structured buffer, indirect argument buffer, runtime material, and runtime mesh are released on disable/destroy and before rebuild. `DestroyImmediate` is not called from `OnValidate`. | Pass by source inspection |
| Shadow contract | `RenderParams.shadowCastingMode` is `Off`; receive shadows is false; benchmark shader contains only `ForwardLit` and no `ShadowCaster` pass. | Pass by source inspection |
| Diagnostics | Inspector provides density presets, explicit rebuild, one current-case report button, and one complete 3 × 3 comparison button. The matrix report includes every geometry/density case, isolates failures, states that it does not measure GPU time, and restores the original live configuration. | Pass by source inspection |
| Documentation consistency | Canonical document owns implementation status and V0–V5 plan. Earlier exploratory document now identifies the canonical replacement and external Weather/Wind ownership. | Pass |
| Static file checks | Delimiter scans passed for every changed C#/HLSL/shader file; every asset has a `.meta`; all GUIDs in the supplied `Game` tree are unique; includes resolve to created files. | Pass |
| Git comparison | Supplied archive has no `.git` directory. No status, `HEAD`, history, or commit comparison was possible. | Recorded limitation |
| Unity validation | Unity 6000.5.0f1 is not available in this execution environment. C# compilation, shader import, Scene view rendering, 1440p profiling, and visual candidate comparison are not yet verified. | Pending — import patch and run V0 comprehensive validation |

The patch must not be treated as performance-accepted or representation-frozen until Unity compilation succeeds and the three geometry candidates are measured at 2560 × 1440 on the selected reference PC.

### Planned follow-on patches

| Patch | Scope | Wind ownership | Status |
| --- | --- | --- | --- |
| V1 | Production static renderer: vegetation profile, GeneratedGround read-only sampling bridge, deterministic placement, chunks, culling, LOD, static diagnostics. | Consume external wind contract only. | Planned |
| V2 | Stylized internal lighting and vegetation patch-edge ground shadow after a cross-subsystem Ground shader audit. | Use external main-light and wind data; no wind simulation. | Planned |
| V3 | Full-visible-domain immediate actor interaction using a low-frequency swept interaction field. | Add actor deformation independently to external wind response. | Planned |
| V4 | Persistent broad trail/compression field with recovery; no grass footprints. | Compose trails with external wind and immediate interaction. | Planned |
| V5 | Production authoring, mass/river exclusions, quality tiers, ecosystem integration, and replacement of the V0 test provider by the canonical Weather/Wind system. | Canonical Weather/Wind system becomes the sole authoritative producer. | Planned |

---

## 1. Canonical Visual Goal

Vegetation-bearing regions should read as coherent masses of tall grass rather than a sparse scatter of isolated tufts.

Where grass exists:

- grass should occupy most of the eligible surface;
- patches should read as thick and difficult to see through;
- exposed ground should be limited to deliberate gaps, worn paths, object clearances, banks, and transitions;
- all visible grass should move under wind;
- every visible actor capable of disturbing grass should bend it;
- actors should leave broad temporary trails through tall grass;
- trails should recover gradually;
- grass should not use detailed footstep stamps;
- detailed footprints remain a snow, dirt, mud, ash, or sand concern.

The system may use clustered geometry, shared fields, supporting ground shading, and distance-dependent representation, but visible grass must remain genuinely deformable geometry rather than a flat non-interactive grass texture.

---

## 2. Core Architectural Decision

The vegetation system should use:

```text
deterministic vegetation placement
+ GPU-instanced grass clusters
+ full-screen vertex wind
+ full-visible-domain actor interaction
+ persistent trail/compression field
+ fake internal and ground-edge shadowing
+ no per-blade objects or physics
```

The rendering unit is a **grass cluster**, not an individual blade object.

Each cluster contains several visible blade strips and is rendered as one GPU instance. Every blade vertex inside every visible cluster responds to wind and interaction fields.

“Fully interactive grass” means:

- every visible blade can deform;
- every visible actor can affect grass;
- trails persist after actors pass;
- the entire visible vegetation field participates.

It does **not** mean:

- every blade has unique CPU state;
- every blade checks every actor;
- every blade has a collider;
- every blade runs an independent spring simulation.

---

## 3. Grass Form Targets

### 3.1 Initial production range

Use the following as first implementation targets:

| Property | Initial target | Allowed prototype range |
| --- | ---: | ---: |
| Grass height | 0.80 m | 0.60–1.20 m |
| Cluster footprint diameter | 0.24 m | 0.18–0.35 m |
| Visible blades per cluster | 8 | 6–12 |
| Vertical blade segments | 2 | 2–3 |
| Triangles per cluster | 32 | 20–60 |
| Cluster height variation | ±20% | ±10–30% |
| Cluster width variation | ±15% | ±5–25% |
| Random yaw range | 0–360° | fixed full rotation |
| Root-anchored bend weight | 0 at root | mandatory |
| Tip bend weight | 1 at tip | mandatory |

These values are starting targets, not universal content rules. Reed beds, scrub, flowers, and broad-leaf vegetation may use separate profiles.

### 3.2 Geometry preference

The preferred baseline is narrow opaque or near-opaque blade strips rather than broad crossed alpha cards.

Reason:

- dense grass at 1440p is likely to be limited by overdraw before raw triangle count;
- narrow geometry provides cleaner silhouettes;
- narrow geometry bends more naturally;
- broad cards tend to shimmer and reveal their planes from an isometric camera;
- shadow and alpha passes become disproportionately expensive at high coverage.

Alpha clipping may remain available for blade-tip shaping or selected species, but it should not be the default assumption for the entire field.

---

## 4. Density and Coverage Targets

### 4.1 Patch-level visual target

Inside a fully vegetated patch:

| Measure | Target |
| --- | ---: |
| Apparent vegetated area | 85–98% |
| Deliberate visible bare-ground gaps | 2–15% |
| Central dense-zone cluster retention | 100% baseline |
| Transition-zone cluster retention | 35–85% |
| Edge transition width | 0.50–1.50 m |
| Object exclusion margin | profile-driven, initial 0.10–0.35 m |

The target is not mathematically uniform packing. It is visually continuous coverage with controlled holes and transitions.

### 4.2 Initial cluster density

Start with:

```text
12–20 clusters per square metre
```

for the densest tall-grass profile.

At an initial 8 blades per cluster, this represents approximately:

```text
96–160 visible blade shapes per square metre
```

before distance reduction.

The first prototype should test at least:

```text
12 clusters/m²
16 clusters/m²
20 clusters/m²
```

The production density should be chosen from gameplay-camera appearance and measured GPU cost, not from close-up inspection.

### 4.3 Distribution rules

Dense vegetation should use three macro states:

```text
bare
transition
dense
```

Avoid weakly sprinkling grass across all surfaces.

Within dense areas:

- cluster density should remain high;
- height and lean should vary;
- occasional intentional openings should be spatially coherent;
- patch borders should become shorter and thinner before terminating;
- compaction and trails should reduce height and uprightness before eliminating geometry.

---

## 5. Rendering Architecture

### 5.1 Ownership

```text
VegetationProfile
    species/family appearance
    height
    density
    stiffness
    wind response
    trail recovery
    cluster mesh and material references

VegetationPlacement
    deterministic accepted cluster records
    chunk ownership
    exclusions and placement validation

VegetationRenderer
    visible chunk selection
    GPU instance buffers
    indirect rendering
    LOD selection
    renderer diagnostics

Weather / Wind System (external dependency)
    authoritative direction and strength
    gust propagation and regional wind state
    visible stylized gust effects
    shared inputs for vegetation, trees, flames, and particles

VegetationWindResponse
    consumes external wind inputs
    applies blade stiffness and height weighting
    adds deterministic response variation without owning wind state

VegetationInteractionDomain
    full-visible-domain immediate bending
    persistent trail/compression
    recovery

VegetationShader
    blade deformation
    stylized lighting
    internal darkness
    trail response
    edge fading
```

### 5.2 Rendering path

Preferred first implementation:

```text
chunked instance data
→ GraphicsBuffer
→ RenderMeshIndirect
→ URP vegetation shader
```

Do not create one Renderer, GameObject, or Transform per cluster.

### 5.3 Draw-call targets

Initial targets for the complete visible vegetation system:

| Metric | Target | Hard prototype warning |
| --- | ---: | ---: |
| Vegetation draw calls | ≤ 16 | > 32 |
| Vegetation materials visible at once | ≤ 4 | > 8 |
| Main grass-family draw calls | 1–4 | > 8 |
| Per-cluster CPU submissions | 0 | any |
| Per-frame Transform updates | 0 | any |

Separate draw calls for major mesh LODs or species families are acceptable. Material proliferation is not.

---

## 6. Visible Instance and Geometry Budgets

These are initial test budgets for the worst-case gameplay camera.

| Quality target | Visible clusters | Approx. visible triangles |
| --- | ---: | ---: |
| Minimum acceptable dense scene | 20,000 | 400k–800k |
| Expected production target | 30,000–50,000 | 600k–1.6M |
| Stress-test ceiling | 70,000 | 1.4M–2.8M |

The preferred production result is not automatically the lowest cluster count. A denser low-overdraw mesh may outperform a lower-count alpha-card solution.

### 6.1 Vegetation GPU budget

At 2560 × 1440, 60 FPS:

```text
Total frame budget: 16.67 ms
Vegetation target GPU cost: ≤ 2.5 ms
Vegetation acceptable ceiling: 3.5 ms
Vegetation failure threshold: > 4.0 ms in ordinary worst-case gameplay
```

The vegetation GPU budget includes:

- visible grass rendering;
- wind deformation;
- immediate interaction sampling;
- persistent trail sampling;
- vegetation-specific culling work;
- vegetation interaction-field update passes;
- fake vegetation edge-shadow support if vegetation-owned.

It excludes unrelated ground, river, character, post-processing, and UI cost.

### 6.2 CPU budget

```text
Vegetation main-thread target: ≤ 0.35 ms
Vegetation main-thread ceiling: 0.75 ms
No normal-frame placement generation.
No per-instance managed allocation.
No per-instance MonoBehaviour update.
```

Placement generation and authoring rebuilds may be slower in Edit Mode, but Play Mode should use already prepared deterministic instance data.

---

## 7. Full-Screen Wind

Every visible blade should move under Weather-owned wind.

### 7.1 Wind ownership and response composition

`Assets/Docs/Weather_Wind_Architecture.md` is the canonical wind architecture. Weather owns the world-space XZ target field, CPU wind queries, gameplay-anchor-centred response cache, update cadence, and future authored wind influences. Vegetation consumes the shared response field and applies only vegetation-specific response:

```text
Weather XZ response-field sample
+ deterministic stiffness variation
+ blade-height weighting
+ restrained blade-detail flutter
→ blade displacement
```

The old `VegetationBenchmarkWindProvider` analytical gust/recovery implementation is superseded. Its class remains only as a hidden migration subclass so the existing scene component does not become missing.

### 7.2 Performance constraints

Initial shader target:

- one bilinear Weather response-field sample per vegetation vertex;
- vegetation must not generate world gust timing, direction, or recovery bands;
- no expensive procedural macro-noise evaluation per blade;
- no GPU readback and no per-instance CPU wind updates;
- roots remain fixed;
- tips receive full response;
- taller blades receive stronger displacement;
- wind movement remains coherent through the shared field.

### 7.3 Wind update cadence

The Weather XZ field updates at a configurable fixed cadence, initially `16 Hz`. The vegetation vertex shader samples the latest response field every rendered frame. Bilinear field sampling and the stored spring state provide spatially continuous motion between field cells; runtime profiling must determine whether temporal field interpolation is required later.

---

## 8. Actor Interaction

### 8.1 Full-visible-domain requirement

The interaction domain follows the visible camera footprint, not the player.

It must cover:

```text
camera-visible world footprint
+ 20% minimum horizontal margin
```

Every relevant actor inside that domain contributes interaction.

Visible enemies at the edge of the screen must bend grass and leave trails exactly as the player does.

### 8.2 Actor interactor record

Initial compact interactor data:

```text
current position
previous position
radius
movement direction
speed
weight
interaction strength
recovery class
```

Each update writes a swept capsule or equivalent continuous path between previous and current position. This avoids gaps at low update rates.

### 8.3 Immediate bend field

Purpose:

- current actor displacement;
- outward grass separation;
- directional lean;
- temporary strong compression while occupied.

Initial target:

| Property | Target |
| --- | ---: |
| Resolution | 256 × 256 |
| Coverage | visible footprint + 20% margin |
| Update rate | 8 Hz |
| Allowed test range | 4–12 Hz |
| Format target | 2-channel signed direction + strength, or equivalent packed format |
| Visual interpolation | mandatory |

A lower update rate is acceptable if swept stamping and shader interpolation keep movement smooth.

### 8.4 Persistent trail field

Purpose:

- broad flattened trails;
- delayed grass recovery;
- repeated path reinforcement;
- lingering combat disturbance.

Initial target:

| Property | Target |
| --- | ---: |
| Resolution | 256 × 256 |
| Update rate | 4 Hz |
| Allowed test range | 2–8 Hz |
| Minimum trail lifetime | 2 s |
| Default trail lifetime | 6 s |
| Allowed profile range | 2–20 s |
| Recovery | smooth, non-linear |
| Footstep detail | prohibited for grass baseline |

The first implementation may use one compression channel plus one age/recovery channel.

### 8.5 Trail shape

A normal actor trail should produce:

```text
central lowered channel
+ temporary outward side bend
+ mild directional lean behind movement
+ gradual inward recovery
```

Initial dimensions for a human-scale actor:

| Property | Target |
| --- | ---: |
| Trail width | actor radius × 1.4 |
| Immediate bend radius | actor radius × 1.8 |
| Persistent core | actor radius × 1.1 |
| Maximum flattening | 65% of standing height |
| Default recovery half-life | 2.0 s |

Large creatures and attacks may override these values.

---

## 9. Interaction Capacity Targets

The full-visible-domain system should support:

```text
1 player
+ 24 ordinary enemies
+ 8 large or high-strength interactors
+ 16 transient gameplay interaction stamps
```

within the normal target budget.

Initial capacity target:

| Metric | Target |
| --- | ---: |
| Persistent actor interactors | 32 |
| Transient stamps per interaction update | 16 |
| Total active records per update | ≤ 48 |
| Ordinary update overflow | none |
| Graceful stress ceiling | 96 records |

At the stress ceiling, lower-priority interactors may be merged spatially or deferred, but visible major actors must retain correct interaction.

---

## 10. Lighting and Shadow Contract

### 10.1 Real-time shadow policy

Baseline tall grass should:

```text
Cast Shadows: Off
Receive Shadows: optional, quality-dependent
```

Do not render dense grass into the main shadow map in the baseline implementation.

The grass should create depth through stylized shading rather than accurate per-blade shadows.

### 10.2 Internal grass shading

Required baseline cues:

- dark roots;
- mid-value bodies;
- slightly brighter tips;
- denser clusters darker near their base;
- stable, softened directional lighting;
- restrained per-cluster variation;
- no harsh card-face lighting.

Initial height-gradient target:

| Blade region | Lighting multiplier |
| --- | ---: |
| Root 0–20% | 0.55–0.70 |
| Middle 20–75% | 0.80–1.00 |
| Tip 75–100% | 0.95–1.10 |

These are artistic response ranges, not physical exposure values.

### 10.3 Patch-edge ground shadow

Grass needs a soft directional anchoring shadow only where the patch meets exposed ground.

Preferred method:

```text
vegetation coverage
→ directional offset opposite main light
→ subtract occupied vegetation region
→ soften
→ apply only to exposed ground beside patch edge
```

Initial targets:

| Property | Target |
| --- | ---: |
| Edge-shadow length | 0.20–0.60 m |
| Edge-shadow opacity | 0.10–0.30 |
| Edge-shadow softness | 0.15–0.45 m |
| Interior-ground shadow evaluation | disabled or visually negligible |
| Shadow-map vegetation pass | none |

The edge shadow should communicate height and grounding without producing a visible dark halo around every isolated cluster.

### 10.4 Shader complexity target

Grass fragment shader target:

- one main texture atlas sample maximum for baseline grass;
- alpha clipping avoided where possible;
- no per-pixel multi-octave noise;
- no per-pixel interaction-field sampling unless proven necessary;
- interaction should normally occur in the vertex stage;
- fog and URP integration must remain compatible.

---

## 11. Distance and LOD Contract

All visible grass remains wind-animated and interaction-aware.

LOD may reduce:

- blades per cluster;
- vertical segments;
- cluster count;
- lighting complexity.

LOD must not disable:

- wind;
- immediate actor bending;
- persistent trail compression.

Initial camera-relative zones:

| Zone | Screen/world intent | Representation |
| --- | --- | --- |
| Near | central gameplay/action region | full mesh and density |
| Middle | majority of visible field | reduced mesh or modest density reduction |
| Far | outer screen margin | simplified cluster and reduced density |

Initial retention targets:

| Zone | Cluster retention | Triangle retention |
| --- | ---: | ---: |
| Near | 100% | 100% |
| Middle | 70–85% | 50–75% |
| Far | 40–60% | 25–50% |

Transitions must use stable deterministic selection and dithered or temporally stable fading. Camera movement must not cause boiling, random reshuffling, or obvious density popping.

---

## 12. Culling and Chunking

Initial chunk target:

```text
vegetation chunk size: 8 × 8 m
prototype range: 4 × 4 m to 16 × 16 m
```

Each chunk should contain:

- deterministic accepted cluster instances;
- conservative world bounds including maximum wind bend;
- profile/material grouping metadata;
- LOD-ready instance subsets or deterministic selection keys.

Required culling:

- camera frustum culling;
- chunk bounds expanded for maximum blade height and wind;
- optional GPU instance compaction after baseline profiling;
- no per-frame CPU iteration over all world vegetation.

Occlusion culling is optional and should not be assumed necessary for the first implementation because the constrained isometric camera and mostly open vegetation fields may limit its value.

---

## 13. Memory Targets

Initial runtime vegetation budget for all active visible and nearby chunks:

```text
Total vegetation runtime memory target: ≤ 64 MB
Acceptable ceiling: 96 MB
```

This includes:

- placement instance buffers;
- visible/compacted instance buffers;
- interaction textures;
- trail textures;
- vegetation lookup textures;
- indirect arguments;
- vegetation-specific temporary GPU buffers.

It excludes shared URP resources and general ground textures.

Suggested instance record target:

```text
16–24 bytes per cluster
```

Prefer packed representation for:

- position;
- yaw;
- scale;
- profile/variant;
- stiffness;
- wind phase;
- deterministic variation.

---

## 14. Ground-Support Illusion Policy

Supporting ground treatment is allowed but may not replace the grass geometry.

Acceptable support:

- darker root-zone colour;
- low vegetation-colour coverage beneath blades;
- reduced bright soil visibility;
- coverage-driven edge shadow;
- subtle contact darkening;
- trail-compatible ground tinting where appropriate.

Unacceptable substitution:

- a flat grass texture carrying most of the patch while sparse geometry decorates it;
- non-interactive distant grass that visibly stops responding;
- billboard-only distant fields that contradict nearby blade motion;
- trails represented only as ground colour with unchanged upright grass.

The geometry must remain the dominant visible vegetation read.

---

## 15. Quality Tiers

### Low

Target platform minimum.

```text
2560 × 1440 at 60 FPS remains the acceptance target.
```

Suggested reductions:

- lower cluster retention;
- 20–32 triangles per cluster;
- 256² immediate and persistent fields;
- 4–8 Hz immediate interaction;
- 2–4 Hz persistent trail update;
- no grass shadow casting;
- simplified lighting;
- maximum two visible grass materials.

### Medium

- higher cluster retention;
- 24–48 triangles per cluster;
- 8 Hz immediate interaction;
- 4 Hz trail update;
- richer gust variation;
- up to four visible vegetation materials.

### High

- maximum authored density;
- richer cluster meshes;
- optional 512² trail field only if measured and visibly justified;
- optional receive-shadow or enhanced lighting;
- no assumption that real grass shadow casting becomes enabled.

Quality tiers should reduce representation cost, not remove full-screen interaction.

---

## 16. Prototype Benchmark

The first technical prototype must use a deliberately hostile scene:

```text
one camera-sized dense tall-grass field
+ grass covering at least 85% of eligible ground
+ player moving continuously
+ 24 enemies moving through separate and overlapping routes
+ 4 large interactors
+ full-screen wind
+ persistent trails
+ patch edges visible against bare ground
+ 2560 × 1440
+ Low quality target
```

Test three cluster families:

1. narrow opaque blade strips;
2. alpha-clipped crossed cards;
3. hybrid strip/card clusters.

Test densities:

```text
12 clusters/m²
16 clusters/m²
20 clusters/m²
```

Record:

- total frame GPU time;
- vegetation GPU time;
- vegetation main-thread time;
- visible cluster count;
- visible triangle count;
- overdraw;
- draw calls;
- interaction update cost;
- trail update cost;
- field memory;
- shadow cost, expected zero for grass casting;
- visual coverage;
- visible trail continuity;
- visible wind continuity;
- LOD popping or temporal boiling.

---

## 17. Acceptance Gates

### 17.1 Visual acceptance

From the actual gameplay camera:

- dense regions cover at least 85% of their intended area;
- patches read as tall grass masses rather than isolated X cards;
- no broad card planes are obvious during ordinary camera movement;
- all visible regions show coherent wind;
- visible enemies deform the grass regardless of screen position;
- actor trails form continuous channels without stamp gaps;
- trails remain visible after the actor passes;
- trails recover smoothly;
- roots remain visually anchored;
- patch edges feel grounded despite grass shadow casting being disabled;
- interior grass does not require accurate real-time shadows to read as deep;
- far grass does not visibly become static.

### 17.2 Performance acceptance

At 2560 × 1440 on the selected low-end reference PC:

```text
Average FPS: ≥ 60
1% low target: ≥ 50 FPS
Vegetation GPU: ≤ 2.5 ms target
Vegetation GPU ceiling: ≤ 3.5 ms
Vegetation main thread: ≤ 0.35 ms target
Vegetation main-thread ceiling: ≤ 0.75 ms
Vegetation runtime memory: ≤ 64 MB target
Vegetation runtime-memory ceiling: ≤ 96 MB
```

A temporary stress spike is acceptable during deliberate benchmark overload. Ordinary worst-case gameplay must stay inside the ceilings.

### 17.3 Interaction acceptance

With 32 persistent interactors and 16 transient stamps:

- no missing visible major-actor interaction;
- no discontinuous trails caused by low update frequency;
- no per-actor × per-cluster CPU loop;
- no per-frame managed allocation;
- immediate interaction update remains ≤ 0.30 ms GPU target;
- persistent trail update remains ≤ 0.20 ms GPU target;
- interaction textures remain stable while the camera domain scrolls;
- no visible full-domain clearing or snapping.

### 17.4 Production acceptance

- no GameObject per grass cluster;
- no Transform per grass cluster;
- no collider per grass cluster;
- no grass shadow-caster pass in the baseline;
- deterministic placement for a fixed seed and profile;
- stable LOD selection;
- interaction available in all visible LODs;
- profile-driven density, height, stiffness, wind response, and recovery;
- one comprehensive validation report should expose the relevant counts, timings, memory, field settings, and acceptance verdict.

---

## 18. Failure Conditions

The architecture should be reconsidered if any of these remain true after reasonable optimization:

- dense grass requires more than 4.0 ms GPU in ordinary worst-case gameplay;
- 1440p overdraw dominates despite narrow geometry;
- patch density must fall below a convincing continuous read;
- far grass must become visibly static to meet budget;
- interaction fields require 512² or higher merely to avoid basic trail artifacts;
- grass shadow casting becomes necessary for acceptable grounding;
- ordinary gameplay needs more than 96 MB vegetation runtime memory;
- actor interaction requires CPU iteration over visible clusters;
- camera movement causes persistent LOD boiling or instance reshuffling.

Failure should trigger measured redesign, not gradual accumulation of special cases.

---

## 19. Recommended Implementation Sequence

### V0 — Benchmark harness

- fixed isometric camera;
- dense test field;
- instrumentation;
- three cluster geometry approaches;
- 12/16/20 clusters per square metre;
- no interaction yet.

### V1 — Indirect vegetation renderer

- deterministic placement records;
- chunking;
- GPU instance buffers;
- frustum culling;
- full-screen rendering;
- first LOD pass.

### V2 — Stylized lighting and patch-edge shadowing

- root darkness and tip lift;
- stable softened lighting;
- coverage-driven patch-edge ground shadow after cross-subsystem audit;
- no grass ShadowCaster pass;
- continue consuming external wind inputs only.

### V3 — Immediate actor interaction

- camera-visible interaction domain;
- swept actor stamping;
- 4/8/12 Hz comparison;
- full-screen sampling by all grass.

### V4 — Persistent trails

- compression field;
- recovery;
- repeated route accumulation;
- 2/4/8 Hz comparison;
- large-creature overrides.

### V5 — Fake grass shadowing

- internal height gradient;
- density/root darkness;
- patch-edge ground shadow;
- confirm grass shadow casting remains disabled.

### V6 — Production profiles and authoring

- grass family profiles;
- density and height controls;
- stiffness and recovery;
- species mixtures;
- transition and exclusion controls;
- production diagnostics.

---

## 20. Current Decision Summary

The accepted planning direction is:

```text
URP
+ tall grass
+ dense patch coverage
+ actual deformable blade geometry
+ GPU-instanced clusters
+ full-screen response to external Weather/Wind input
+ all-visible-actor interaction
+ broad persistent trails
+ low-frequency field updates
+ no grass footprints
+ no per-blade CPU simulation
+ no baseline real-time grass shadows
+ shader-based internal depth
+ coverage-based patch-edge ground shadow
+ 1440p / 60 FPS low-end target
```

The first implementation should prove the visual density and 1440p GPU budget before expanding into additional vegetation families.

### V0 automated timed comparison suite

The benchmark Inspector provides one Play Mode action that runs the complete
geometry-candidate × density-preset matrix. Each case is rebuilt, warmed up,
measured across rendered frames, and appended to one retained report. The suite
restores the previously selected geometry, density, and render-enabled state in
a `finally` path. A second button copies the last completed suite report.

CPU frame timing is always reported from measured Play Mode frames. GPU timing
is included only when `FrameTimingManager` supplies valid samples; the report
must identify unavailable GPU timing rather than infer it from CPU frame time.

## V0 automated benchmark suite — paired-baseline upgrade

The V0 benchmark Inspector and automated suite now expose the same meaningful controls rather than hiding capabilities inside the suite.

Inspector controls include explicit selection of every geometry candidate (`OpaqueStrips`, `CrossedCards`, and `Hybrid`), every density preset (12, 16, and 20 clusters/m²), rendering enable/disable, manual rebuild, the single-case report, the structural nine-case matrix, and the complete timed suite. The ordinary serialized fields continue to expose field dimensions, seed, geometry dimensions, variation ranges, colours, camera, and suite settings.

The complete timed suite remains one press to execute and one optional press to copy. It now:

- runs every geometry × density case without manual case switching;
- performs a configurable number of passes per case;
- alternates whether the render-disabled baseline or vegetation-enabled measurement runs first, reducing ordering drift;
- warms up every measurement window;
- aggregates whole-frame CPU and genuine `FrameTimingManager` GPU samples;
- reports average, median, p95, minimum, maximum, and standard deviation;
- calculates estimated vegetation CPU/GPU median deltas against adjacent render-disabled baselines;
- marks whether each delta separates from the observed combined noise estimate;
- emits a confidence-aware ranking and refuses to declare a winner when differences remain within noise;
- records resolution, graphics API, GPU, VSync, target frame rate, and Editor/player context;
- optionally requests one screenshot per configuration under `Library/VegetationBenchmarkCaptures`, never under `Assets`;
- restores the original geometry, density, and render-enabled state in a `finally` path.

These measurements remain comparative whole-frame estimates. A standalone development build and Unity Profiler GPU inspection remain the authority for a final production performance decision.

---

## Active implementation — V1A paintable coverage and dense concurrent candidates

**Status:** IMPLEMENTED — UNITY VALIDATION PENDING

### Objective

Add Ground-owned, editor-paintable vegetation coverage and consume it through deterministic vegetation placement while retaining OpaqueStrips, CrossedCards, and Hybrid as concurrent benchmark candidates. Full-coverage grass must be capable of concealing nearly all Ground without authoring grass over the entire screen.

### Approved files

- `Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Game/Procedural/Ground/GeneratedGround.cs`
- `Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs`
- `Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`

### Reviewed evidence

- `GeneratedGround.TrySampleBaseSurface(...)` already provides authoritative world height, normals, and semantic suitability from `baseSurface`.
- `GeneratedGroundEditor.OnSceneGUI()` already owns Ground Scene-view diagnostics and is the correct existing editor integration point.
- `VegetationBenchmark.BuildInstances(...)` currently places uniformly on a flat local field and is independent of Ground.
- The V0 suite already compares all three geometry candidates and restores user settings.
- The latest paired-baseline run found all candidate timing differences within whole-frame noise; no geometry candidate is authorized for removal.
- The supplied archive contains no `.git` directory. HEAD/history/working-tree comparison is unavailable; the supplied archive plus the latest accepted V0 patch is the authoritative pre-edit baseline.

### Invariants

- Ground owns authored coverage; Vegetation only consumes it.
- Coverage is scalar authorization: 0 means excluded, 1 means full local authorization.
- Placement remains deterministic and geometry-independent.
- All three geometry candidates remain available in the Inspector and automated suites.
- No per-frame placement rebuild.
- Painting changes only the compact serialized coverage field and its revision.
- No scene, prefab, material, layer, tag, River, Weather, or Generated Mass asset changes.

### Implementation sequence

1. **COMPLETE** — Record V1A scope, evidence, invariants, risks, and validation in this canonical plan.
2. **COMPLETE** — Add compact serialized vegetation coverage to `GeneratedGround`, including sampling, fill, clear, paint, diagnostics, mapping, and revision APIs.
3. **COMPLETE** — Add Ground Inspector and Scene-view paint/erase controls with brush preview and explicit activation.
4. **COMPLETE** — Add optional Ground-driven height and coverage consumption to `VegetationBenchmark` while retaining flat full-field fallback.
5. **COMPLETE** — Expand dense presets and expose candidate fullness/coverage controls consistently in the Inspector.
6. **COMPLETE** — Extend reports and the one-press suite with authored/full coverage diagnostics while preserving restoration.
7. **COMPLETE — SOURCE AUDIT PASSED; UNITY VALIDATION PENDING** — Perform source, scope, serialization, deterministic-placement, editor-input, and resource-lifecycle audit.

### Acceptance criteria

- Grass is generated only where Ground coverage authorizes it when Ground integration is enabled.
- Paint, erase, fill, and clear are available from the existing GeneratedGround Inspector.
- Brush editing is explicit and cannot accidentally alter unrelated Ground authoring.
- All accepted placements sample Ground height successfully or are counted as rejected.
- Geometry candidates share the same placement hash for the same Ground mask, density, and seed.
- Dense presets can reach near-total Ground concealment without removing lower reference presets.
- The one-press suite retains all geometry candidates and restores geometry, density, rendering, and coverage-scenario state.
- Unity compilation, Scene-view painting, screenshot output, and Play Mode suite execution remain required project validation.

### Post-implementation audit

- Final modified-file scope matches the approved five files.
- Ground coverage is serialized as one byte per texel at a default 128² resolution (16,384 bytes before Unity serialization overhead).
- Coverage painting is dirty-time only; runtime placement rebuilds only when explicitly requested or when existing benchmark lifecycle requests a rebuild.
- Geometry candidate is not included in placement generation, so all three candidates retain identical accepted placements for the same seed, density, mask, and transform.
- The dense automated suite uses 20 / 35 / 50 clusters per m² and runs authored plus forced-full coverage when a Ground source is assigned.
- The Inspector retains 12 / 16 / 20 reference presets and adds 35 / 50 dense presets.
- Delimiter balance, introduced symbol references, namespace imports, forbidden `GetInstanceID`, and the C# 9 `RenderMeshIndirect` call form were checked locally.
- Unity compilation, Scene-view raycast painting, undo persistence, Ground-height placement, all 18 timed cases, screenshots, and visual concealment remain pending in Unity 6000.5.0f1.

## V1A.1 Corrective authoring contract

- Geometry has one authoritative Inspector control: the serialized `Geometry` dropdown. The duplicate geometry buttons are removed.
- Density has one authoritative Inspector control: `Density Per Square Metre`. The duplicate density preset buttons are removed.
- Any ordinary benchmark Inspector change rebuilds the current preview through the same path, except while the automated suite owns the benchmark.
- Assigning an uninitialized Ground coverage mask no longer rejects every placement. Until initialization, it behaves as full authorization and presents a warning.
- Ground coverage authoring exposes explicit `Initialize Empty` and `Initialize Full` actions. After initialization these become `Clear Empty` and `Fill Full`.
- Scene painting intersects the selected Generated Ground surface directly through its height field and does not require a collider or depend on unrelated scene colliders.
- Coverage painting updates the visual overlay during the stroke and rebuilds vegetation benchmarks that reference that Ground once when the stroke ends.
- Authoring workflow: initialize Empty for selective patches or Full for subtractive authoring; enable Scene Painting; left-drag to paint; enable Erase to remove; assign the same Generated Ground to the benchmark and enable Ground Coverage.

## V1A.4 Scene painting callback audit and corrective plan

### Status

- **Read-only audit: COMPLETE**
- **Persistent plan: COMPLETE**
- **Implementation: COMPLETE**
- **Post-change audit: COMPLETE**
- **Unity validation: PENDING**

### Objective

Make vegetation coverage painting reliably receive Scene-view input and render a brush/status overlay when one `GeneratedGround` is selected and `Enable Scene Painting` is active.

### Observed failure

- User validation after V1A.2 and V1A.3 reports no brush, no painting, and ordinary Scene selection/navigation behavior.
- V1A.3 added an unconditional paint-mode status box inside `HandleVegetationCoveragePainting` during `EventType.Repaint`, but the user still observed no status box.
- Therefore the direct mesh hit test is not the first active failure. The vegetation paint handler is not being reached through the current callback path in the validated editor state.

### Reviewed evidence

- `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs`
  - `OnEnable` currently registers only `Undo.undoRedoPerformed`.
  - `OnDisable` currently unregisters only `Undo.undoRedoPerformed`.
  - `OnSceneGUI` is the sole caller of `HandleVegetationCoveragePainting` and `DrawVegetationCoverageOverlay`.
  - `HandleVegetationCoveragePainting` already claims default control, preserves Alt navigation, draws a status box, draws a wire-disc brush, records undo, paints, and rebuilds dependent vegetation once on mouse-up.
- `Assets/Game/Procedural/Ground/GeneratedGround.cs`
  - `TryRaycastVegetationCoverageSurface` intersects the generated mesh directly and has no collider dependency.
  - Coverage storage, UV mapping, painting, revisioning, fill, and clear APIs are present.
- Existing Ground and Mass editor Scene overlays continue to use `OnSceneGUI`; they are read-only debug overlays and do not prove that the new interactive paint path is dispatched in the user's current editor state.
- Repository rule source: `AGENTS.md`, mandatory gates 1–4.

### Approved files

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs`

No runtime Ground data, vegetation renderer, shader, scene, prefab, material, layer, tag, or asset format changes are approved for this correction.

### Invariants and non-goals

- Preserve existing Painted Accent Scene overlays.
- Preserve the direct generated-mesh hit test.
- Preserve Alt Scene navigation.
- Paint only the single editor target and do not introduce global painting components or tools.
- Rebuild dependent vegetation only once after a completed changed stroke.
- Do not add per-frame Ground or vegetation rebuilds.
- Do not alter mask serialization or coverage semantics.

### Implementation sequence

1. **COMPLETE** — Register an explicit `SceneView.duringSceneGui` callback in `OnEnable` and unregister it in `OnDisable`.
2. **COMPLETE** — Route vegetation painting and vegetation coverage overlay rendering through that registered callback.
3. **COMPLETE** — Remove the vegetation paint calls from `OnSceneGUI` to prevent duplicate event processing; retain existing Painted Accent debug overlay behavior there.
4. **COMPLETE** — Request Scene repaint on mouse movement while paint mode is active so the brush follows the cursor without requiring another editor repaint source.
5. **COMPLETE** — Keep input capture limited to non-Alt left-button interaction while paint mode is active.
6. **COMPLETE** — Run final scope, symbol, callback lifecycle, duplicate-processing, and syntax audits; record results here.

### Acceptance criteria

- Enabling Scene Painting with one Generated Ground selected visibly shows the paint status overlay in the Scene view.
- Moving the cursor over the generated mesh shows the brush disc.
- Left-drag changes coverage; ordinary object selection does not occur during the stroke.
- Alt navigation remains functional.
- Mouse-up rebuilds dependent vegetation once when a stroke was active.
- Disabling Scene Painting restores normal Scene selection behavior.
- Existing Painted Accent overlays remain unchanged.

### Risks and validation

- Duplicate callback subscription after editor recreation: prevented by unsubscribe-before-subscribe and `OnDisable` cleanup.
- Duplicate paint processing: prevented by removing vegetation handling from `OnSceneGUI`.
- Locked Inspector or stale editor target: callback validates exactly one target and a non-null `GeneratedGround` each invocation.
- Unity compilation and actual Scene interaction remain required project validation.


### V1A.4 post-change consistency and compliance audit

- Final diff is limited to the approved canonical plan and `GeneratedGroundEditor.cs`.
- `GeneratedGround.cs`, vegetation runtime code, shaders, assets, scenes, prefabs, layers, and tags are unchanged.
- The interactive vegetation path now has one owner: `SceneView.duringSceneGui`. `OnSceneGUI` retains only the pre-existing Painted Accent/debug overlay path.
- Callback lifecycle is symmetric: unsubscribe-before-subscribe in `OnEnable`, unsubscribe in `OnDisable`.
- Mouse-move repaint is active only while the selected Ground has vegetation paint mode enabled.
- Existing direct generated-mesh raycast, brush drawing, input capture, Undo, dirty marking, and one-rebuild-on-mouse-up behavior are retained unchanged.
- Static source checks passed for balanced delimiters, single callback registration site, single interactive paint call site, and no prohibited instance-ID API use.
- Unity 6000.5.0f1 compilation and Scene interaction remain pending and are the only unresolved validation items.

## V1A.5 Scene-view vegetation authoring preview

### Status

- **Read-only audit: COMPLETE**
- **Persistent plan: COMPLETE**
- **Implementation: COMPLETE**
- **Post-change audit: COMPLETE**
- **Unity validation: PENDING**

### Objective

Render the current vegetation benchmark in Unity Scene views so coverage painting, density, geometry, and scene composition can be judged without relying exclusively on the assigned gameplay camera.

### Reviewed evidence

- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
  - The component is `[ExecuteAlways]`, builds resources in `OnEnable`, and submits its indirect draw in `LateUpdate`.
  - `LateUpdate` assigns `RenderParams.camera = targetCamera`; when `Main Camera` is assigned, the indirect draw is restricted to that gameplay camera.
  - The component has no Scene-camera submission path.
  - `renderBenchmark`, `resourcesReady`, bounds, material, mesh, indirect arguments, and instance data are already sufficient for another camera-specific submission without rebuilding.
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
  - Inspector changes and explicit rebuilds already call `SceneView.RepaintAll`, but repaint requests do not submit the vegetation draw to the Scene camera.
  - The timed suite exposes `SuiteRunning`, which can suppress Scene-view preview so benchmark measurements do not include the added editor preview submission.
- Unity API contract:
  - `RenderPipelineManager.beginCameraRendering` runs before Unity renders an individual SRP camera.
  - `CameraType.SceneView` identifies Scene-view cameras and excludes Preview, Reflection, and ordinary Game cameras.
- The supplied archive has no `.git` directory. HEAD/history/working-tree comparison remains unavailable; the assembled accepted V1A.4 patch state is the authoritative pre-edit baseline.

### Approved files

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`

### Invariants and non-goals

- Preserve the assigned gameplay-camera render path.
- Do not broaden vegetation rendering to Preview, Reflection, VR, or unrelated Game cameras.
- Do not rebuild placement per Scene-view frame.
- Reuse the existing mesh, material, instance buffer, indirect arguments, and bounds.
- Scene-view preview is enabled by default but explicitly user-controllable in the benchmark Inspector.
- Timed suites suppress Scene-view preview for their full duration.
- No shader, wind-provider, Ground, scene, prefab, material, layer, or tag changes.
- Static Scene-view wind is not part of this patch; the existing temporary external wind input remains unchanged.

### Implementation sequence

1. **COMPLETE** — Added one serialized `Scene View Preview` toggle and an enable/disable-maintained active benchmark registry; no per-frame scene search is used.
2. **COMPLETE** — Extracted indirect submission into `SubmitIndirectRender(Camera)`, used by the existing gameplay path and Scene-view preview path.
3. **COMPLETE** — Added an editor-only `RenderPipelineManager.beginCameraRendering` bridge filtered strictly to `CameraType.SceneView`.
4. **COMPLETE** — Preview is suppressed while `SuiteRunning`, when the toggle is disabled, when resources/rendering are unavailable, and when `targetCamera == null` already requests all-camera rendering.
5. **COMPLETE** — The comprehensive report and Inspector status state preview enablement and suite suppression.
6. **COMPLETE — SOURCE AUDIT PASSED; UNITY VALIDATION PENDING** — Completed scope, lifecycle, duplicate-submission, camera-filter, syntax, and resource audits.

### Acceptance criteria

- With `Scene View Preview` enabled, grass is visible in Scene view in Edit Mode and while painting coverage.
- The assigned Main Camera continues to render the same vegetation in Game view.
- Preview, Reflection, and Inspector thumbnail cameras do not receive the Scene-view submission.
- Disabling `Scene View Preview` hides only the Scene-view copy.
- The complete timed suite suppresses Scene-view preview and restores ordinary preview behavior when the suite ends.
- Scene preview uses existing resources and creates no per-frame placement rebuild or persistent allocation.


### V1A.5 post-change consistency and compliance audit

- Final modified-file scope matches the approved canonical plan, vegetation runtime component, and vegetation custom editor only.
- Gameplay rendering still submits once from `LateUpdate` with the configured `targetCamera`; its resource ownership and rebuild lifecycle are unchanged.
- Scene-view rendering reuses the existing mesh, material, instance buffer, indirect arguments, transformed bounds, layer, and entity ID; it performs no placement rebuild and creates no per-camera persistent resource.
- The editor bridge filters `camera.cameraType == CameraType.SceneView`; Unity Preview, Reflection, VR, and ordinary Game cameras are excluded from this additional submission.
- The bridge iterates an enable/disable-maintained active benchmark registry. `OnEnable` deduplicates registration; `OnDisable` and `OnDestroy` remove registration.
- `SuiteRunning` suppresses the Scene-view submission for the complete automated suite. Ordinary preview resumes after the suite clears its existing state.
- When `targetCamera == null`, the existing all-camera submission remains authoritative and the explicit Scene-view submission is skipped to avoid duplicate rendering.
- Inspector enable/disable-render actions now repaint Scene views immediately. Ordinary serialized changes already rebuild and repaint through the existing custom editor path.
- The comprehensive report records Scene-view preview enablement and whether a currently running suite suppresses it.
- No Ground, shader, wind provider, scene, prefab, material, layer, tag, River, Weather, or Generated Mass file changed.
- Static checks passed for balanced delimiters, required namespace imports, single `beginCameraRendering` subscription site, strict Scene-camera filtering, shared render-method call sites, C# 9-compatible `RenderMeshIndirect` invocation, and no prohibited instance-ID API.
- Unity 6000.5.0f1 compilation and visual Scene/Game camera validation remain pending and are the only unresolved validation items.

## V1B candidate-preserving silhouette stability and scale controls

### Status

- **Read-only audit: COMPLETE**
- **Persistent plan: COMPLETE**
- **Implementation: COMPLETE — SOURCE LEVEL**
- **Post-change audit: COMPLETE — SOURCE LEVEL**
- **Unity validation: BLOCKED by the current missing `GeneratedGround` vegetation coverage contract until the Ground thread restores it**

### Objective

Improve grass readability from the fixed isometric gameplay camera by preventing long needle-like, sub-pixel tips, exposing explicit scale controls, and adding a restrained distance-width stabilization path for all three geometry candidates. Preserve concurrent OpaqueStrips, CrossedCards, and Hybrid development and comparison.

### Reviewed evidence

- `Assets/Game/Procedural/Vegetation/VegetationClusterMeshBuilder.cs`
  - `AddStrip` currently hard-codes `taper = Mathf.Lerp(1f, 0.12f, t * t)` across the full blade height.
  - Opaque strips derive width from `clusterDiameter * 0.07`; CrossedCards derive width from `clusterDiameter * 0.72`; Hybrid uses separate hard-coded fractions.
  - The mesh stores only root-to-tip in vertex color and no centerline data, so symmetric shader-side width expansion cannot currently be performed without extending vertex attributes.
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`
  - CrossedCards/Hybrid apply a second hard-coded fragment silhouette taper: `allowedWidth = lerp(0.92, 0.10, input.uv.y * input.uv.y)`.
  - No authored tip width, taper start, or width-stabilization controls exist.
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
  - Existing size controls are `clusterDiameter`, `grassHeight`, and per-instance `widthScaleRange`; there is no authoritative master blade-width control.
  - Existing timed suite varies geometry, density, and coverage only.
  - `RenderSceneViewPreview` currently calls `SubmitIndirectRender(sceneViewCamera)` twice. This is a proven duplicate Scene-view submission defect inside the approved V1B runtime file and must be corrected as part of the consistency pass.
  - Current code references missing Ground APIs (`VegetationCoverageInitialized`, `VegetationCoverageRevision`, `CalculateVegetationCoverageFraction`, `TrySampleVegetationCoverage`). The Ground thread owns restoration; V1B must not stub, remove, or redefine that contract.
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
  - Inspector uses the serialized fields as authoritative and rebuilds after changes.
  - Scene-view preview is driven through the existing SRP SceneView camera bridge.
- `Assets/Game/Rendering/Vegetation/Includes/VegetationCommon.hlsl`
  - Vertex transformation currently scales all mesh XZ offsets around the instance origin; it has no per-strip centerline input.
- `Assets/Game/Rendering/Vegetation/Includes/VegetationWindResponse.hlsl`
  - Wind displaces world positions after object-to-world transformation and uses root-to-tip weighting. It does not own silhouette width and should remain otherwise unchanged.
- AA state is global/camera rendering configuration, not vegetation-owned state. V1B records active AA state but does not modify URP assets or camera serialized settings.
- The supplied archive has no `.git` directory. HEAD/history/working-tree comparison is unavailable. The supplied archive is the authoritative pre-edit baseline.

### Approved files

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/VegetationClusterMeshBuilder.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationCommon.hlsl`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

`VegetationWindResponse.hlsl` is reviewed but is not approved for modification unless implementation evidence proves ordering changes are required. Ground, URP assets, cameras, scenes, prefabs, materials, layers, tags, River, Weather, and Generated Mass remain outside scope.

### Invariants and non-goals

- Keep all three geometry candidates active in the Inspector and suite.
- Preserve identical placement data and deterministic hashes when only geometry or silhouette profile changes.
- No conventional near/mid/far LOD system.
- No TAA and no automatic AA-setting mutation.
- No additional draw calls, instance buffers, or per-frame placement rebuilds.
- Root centerlines remain fixed under width stabilization; only lateral offsets expand.
- Scene and Game cameras use the active rendering camera for stabilization. Perspective cameras use the configured distance ramp; orthographic cameras apply the capped stabilization uniformly because projected size is distance-invariant.
- Existing Ground coverage calls remain unchanged pending the Ground-thread repair.
- Timed suite remains one-run and one-copy.

### Implementation sequence

1. **COMPLETE** — Record reviewed evidence, blocker, exact scope, invariants, risks, acceptance criteria, and validation plan here before code edits.
2. **COMPLETE** — Add authoritative `Master Blade Width`, `Tip Width Ratio`, `Taper Start`, and restrained distance-width stabilization controls to `VegetationBenchmark`; clamp and report them.
3. **COMPLETE** — Extend cluster mesh generation with a general width profile and centerline UV data while preserving current topology and candidate identities.
4. **COMPLETE** — Extend the vertex shader/common include to expand lateral offsets symmetrically around stored centerlines using the active camera, then apply existing wind response.
5. **COMPLETE** — Replace the hard-coded card fragment taper with the authored taper/tip controls.
6. **COMPLETE** — Add three deterministic silhouette profiles and an 18-case timed matrix: 3 geometries × 3 profiles × 2 densities (35/50), forced-full stress coverage, with full state restoration.
7. **COMPLETE** — Record AA state, profile settings, and verified screenshot existence/size in reports.
8. **COMPLETE** — Remove the duplicate Scene-view indirect submission and confirm one gameplay plus one Scene-view submission path.
9. **COMPLETE — SOURCE AUDIT PASSED; UNITY VALIDATION BLOCKED** — Complete final scope, caller/consumer, CPU/GPU contract, shader layout, resource, deterministic-placement, C# syntax, HLSL syntax, and documentation audits.

### Canonical silhouette profiles

- **CurrentCompatible** — current-equivalent master width, near-current taper/tip, stabilization off.
- **StableSilhouette** — approximately 1.25× current effective width, tip width ratio 0.12, taper start 0.68, stabilization enabled, maximum multiplier 1.20.
- **BroadStress** — clearly wider silhouette, tip width ratio 0.22, taper start 0.78, stabilization enabled, maximum multiplier 1.35.

These values are suite test profiles, not frozen production defaults.

### Acceptance criteria

- All three candidates visibly respond to the same master width, tip width, taper, and stabilization controls.
- The fixed isometric gameplay view shows fewer isolated/interrupted one-pixel tips than the current-compatible profile.
- CrossedCards do not expose obvious rectangular card boundaries under the stable profile.
- OpaqueStrips materially reduce thin-slice pixel noise under the stable profile.
- Hybrid remains coherent and retains both strip/card contributions.
- Roots and centerlines do not move laterally because of stabilization.
- Geometry/profile changes do not alter placement hash for the same seed/density/coverage.
- Scene-view preview matches Game-view geometry controls and performs one explicit Scene-camera submission.
- Suite executes 18 cases automatically, restores every modified field, and retains one report.
- Report records AA state and confirms screenshot files exist with nonzero size after capture.

### Risks and validation

- Wider geometry can increase rasterized coverage/overdraw; timed GPU deltas and screenshots are required.
- Centerline vertex data must be bound consistently between C# mesh UV channel and HLSL attributes.
- Card mesh taper and fragment silhouette taper must use the same semantics to avoid contradictory shapes.
- Stabilization must use the active rendering camera and must not depend on the serialized gameplay camera.
- Unity compilation remains blocked until the Ground coverage contract is restored. This blocker must remain explicit; V1B must not be represented as Unity-validated before that repair and a clean compile.


### V1B post-change consistency and compliance audit

- Final modified-file scope is limited to the six approved files. `VegetationWindResponse.hlsl`, Ground runtime/editor files, URP assets, cameras, scenes, prefabs, materials, layers, tags, River, Weather, and Generated Mass are unchanged.
- `VegetationBenchmark` now exposes one authoritative master blade width, tip-width ratio, taper start, width-stabilization toggle, perspective start distance, and capped maximum multiplier. Inspector parity is provided by the existing default serialized Inspector and automatic rebuild path.
- `VegetationClusterMeshBuilder.Build` has one updated caller and one updated definition. Opaque strip geometry uses the authored width profile directly; card candidates retain broad card geometry and use the same authored profile in fragment silhouette clipping.
- UV1 stores each strip/card row centerline XZ. HLSL consumes `TEXCOORD1`; width stabilization expands only the lateral offset around that centerline. Root/centerline positions remain unchanged.
- Perspective cameras use the configured distance ramp. Orthographic cameras apply the capped multiplier uniformly because projected size does not vary with camera distance. The active rendering camera supplies `_WorldSpaceCameraPos` and `unity_OrthoParams`; no serialized-camera transform is used in the shader.
- Existing wind response remains unmodified and is applied after stabilized vertex transformation. Wind ownership remains external.
- The timed suite now constructs exactly 18 cases: 3 geometry candidates × 3 canonical silhouette profiles × densities 35/50, with forced-full coverage stress. It saves/restores every silhouette field, geometry, density, render state, and coverage override in `finally`.
- Reports include silhouette settings, QualitySettings MSAA, reflected URP pipeline MSAA/render scale when available, camera post-process AA when available, resolution, graphics API, and GPU. Screenshot capture waits up to two seconds for a non-empty file and reports verified byte size or an explicit failure.
- The duplicate `SubmitIndirectRender(sceneViewCamera)` call found in the supplied baseline was removed; one Scene-camera submission remains. Gameplay submission remains unchanged.
- Placement generation does not read geometry or silhouette settings, so deterministic placement hashes remain invariant when only these controls change.
- Source checks passed for balanced delimiters, single builder caller/definition parity, C#/HLSL property-name parity, UV1/TEXCOORD1 parity, one Scene-view submission, no `GetInstanceID`, no `ref renderParams`, unchanged wind include, and archive scope.
- The supplied archive has no Git metadata, so HEAD/history/working-tree comparisons were unavailable and are recorded as such.
- **Known blocker:** the supplied current `GeneratedGround.cs` still lacks `VegetationCoverageInitialized`, `VegetationCoverageRevision`, `CalculateVegetationCoverageFraction`, and `TrySampleVegetationCoverage`. The existing vegetation callers therefore prevent a clean Unity compile until the Ground thread restores the accepted contract. No stubs or call removals were introduced.
- Unity 6000.5.0f1 C# compilation, shader import, visual silhouette comparison, root anchoring, screenshot verification, and the 18-case suite remain pending after the Ground contract is restored. The patch is not Unity-validated.


## V1B.1 Play Mode Ground-startup synchronization

### Status

- **Read-only audit: COMPLETE**
- **Persistent plan: COMPLETE**
- **Implementation: COMPLETE — SOURCE LEVEL**
- **Post-change audit: COMPLETE — SOURCE LEVEL**
- **Unity validation: PENDING**

### Proven defect

- `VegetationBenchmark.OnEnable()` rebuilt immediately on Play Mode entry.
- `GeneratedGround.OnEnable()` only requested regeneration; its Play Mode startup regeneration is flushed from `GeneratedGround.Start()`.
- `VegetationBenchmark.BuildInstances()` rejects every candidate when `GeneratedGround.TrySampleBaseSurface()` returns false.
- The observed Play Mode Inspector state was `Ready: 0 instances`, proving the rebuild completed with an empty indirect instance set rather than a camera-rendering failure.
- No retry existed after Ground regeneration completed.

### Objective and acceptance criteria

Wait cheaply for the assigned Generated Ground to expose a valid center surface sample, then rebuild vegetation exactly once. Do not add per-frame full-field rebuilds or modify Ground-owned lifecycle code.

Acceptance requires:

- Edit Mode vegetation remains unchanged.
- Entering Play Mode no longer leaves the benchmark at zero instances solely because Ground regenerated later.
- Waiting performs one cheap Ground sample per frame and one eventual vegetation rebuild.
- The retry stops when the component is disabled, Ground integration is removed, a timed suite starts, or 120 frames elapse.
- A timeout produces an explicit build error instead of silently retaining zero instances.

### Approved files

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`

### Implementation and compliance result

- `OnEnable()` still performs the normal immediate rebuild, then schedules synchronization only when Play Mode, Ground integration, zero accepted instances, and Ground sampling rejection evidence all coincide.
- The synchronization coroutine probes `coverageGround.transform.position`, which corresponds to local Ground center and avoids rebuilding while Ground is not ready.
- When the center sample becomes valid, the coroutine clears its ownership and calls `RebuildBenchmark()` once.
- `OnDisable()` stops and clears the coroutine symmetrically.
- No Ground files, shaders, mesh generation, placement rules, suite matrices, serialized assets, camera settings, or geometry controls changed.
- Source review confirmed no continuous full-field rebuild path was introduced.
- Unity compilation and Play Mode validation remain pending.

## VEG-WIND-V0.1 — Coherent traveling gust waves — SUPERSEDED BY WEATHER-WIND-V0

### Status

- **Read-only audit: COMPLETE**
- **Persistent plan: COMPLETE**
- **Implementation: COMPLETE — SOURCE LEVEL**
- **Post-change audit: COMPLETE — SOURCE LEVEL**
- **Unity validation: PENDING**

### Reviewed evidence

- `Assets/Game/Procedural/Vegetation/VegetationBenchmarkWindProvider.cs` publishes one normalized direction, a positive steady strength, gust strength, spatial frequency, travel speed, and time.
- `Assets/Game/Rendering/Vegetation/Includes/VegetationWindResponse.hlsl`, `EvaluateVegetationBenchmarkGust`, adds `instancePhase * 2π` to the world-space gust phase. `VegetationInstanceData` supplies a different deterministic phase per cluster. This makes adjacent clusters occupy unrelated points of the gust cycle and directly violates the existing Section 7 requirement that neighbouring clusters remain coherent.
- `ApplyVegetationWindResponse` adds only positive steady and positive gust displacement along the prevailing direction. The current calm state therefore cannot sway through negative displacement or laterally.
- `SH_StylizedVegetationBenchmark.shader`, `Vert`, applies wind after stabilized world transformation. This ordering is correct and remains unchanged.
- `VegetationBenchmark.BuildComprehensiveReport` currently reports only provider count and generic temporary-source state; it does not record the active test-wave parameters.
- The supplied archive contains no `.git` metadata. HEAD, history, and working-tree comparisons are unavailable. The supplied archive is the current source baseline.

### Objective

Replace the decorrelated per-cluster test motion with a cheap analytic wind pattern containing:

1. broad signed calm sway that moves through positive and negative displacement along the prevailing axis;
2. a smaller broad signed lateral sway so calm grass does not move on one strict line;
3. a small positive prevailing bias;
4. a coherent positive-direction gust band shared by neighbouring clusters;
5. low-frequency front distortion that prevents a perfectly straight ruler-like gust edge without reintroducing per-cluster timing noise.

This remains a temporary benchmark source. Weather retains ownership of future authoritative wind state.

### Approved files

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmarkWindProvider.cs`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationWindResponse.hlsl`

No Ground, mesh, editor, scene, prefab, material, camera, URP, River, Weather, Generated Mass, layer, or tag file is approved.

### Invariants and non-goals

- Do not change placement generation, candidate geometry, instance layout, draw count, Ground sampling, width stabilization, or suite matrices.
- Remove per-instance timing from the macro gust and calm waves. Instance variation may affect response magnitude only.
- Roots remain fixed through existing root-to-tip weighting.
- Gust displacement remains aligned with the prevailing direction and never reverses the gust itself.
- Calm sway may cross zero and bend in both directions; a smaller perpendicular component provides multi-directional calm motion.
- Use analytic low-frequency functions only. Add no textures, buffers, compute work, CPU instance updates, or full-field rebuilds.
- Preserve freeze-time inspection.
- Keep controls explicit in metres and metres per second where applicable.

### File-by-file implementation sequence

1. **COMPLETE** — Record the audit, scope, invariants, controls, risks, and validation requirements in this canonical plan before implementation edits.
2. **COMPLETE** — Replaced ambiguous frequency-only provider controls with calm sway, prevailing bias, gust spacing/width/speed, and front-distortion controls; published one additional global parameter vector plus one lateral-ratio scalar and exposed read-only values for diagnostics.
3. **COMPLETE** — Replaced the randomized sine gust with a world-space periodic smooth pulse; removed `instancePhase` from wave timing; added broad signed longitudinal and lateral calm sway.
4. **COMPLETE** — Extended the comprehensive report with the currently publishing temporary provider and all wind-wave settings.
5. **COMPLETE — SOURCE AUDIT PASSED; UNITY VALIDATION PENDING** — Completed scope, symbol, property parity, delimiter, prohibited-API, instance-layout, draw-path, and source syntax audits.

### Default test profile

- Prevailing direction: `(1.0, 0.25)`.
- Prevailing bias: `0.06`.
- Calm sway strength: `0.14`.
- Calm wave scale: `11 m`.
- Calm travel speed: `0.65 m/s`.
- Calm lateral ratio: `0.35`.
- Gust strength: `0.52`.
- Gust spacing: `18 m`.
- Gust width: `6 m`.
- Gust travel speed: `4 m/s`.
- Gust-front distortion: `1.5 m` over a `14 m` lateral scale.

These are benchmark defaults, not production Weather values.

### Performance analysis

- Active CPU: unchanged except the provider publishes one additional global vector each update.
- Active GPU vertex cost: replaces the existing sine gust with one periodic pulse and adds low-frequency calm/front functions. No texture sample, buffer lookup, draw, or instance update is added.
- Active GPU fragment cost: unchanged because geometry coverage and material shading are unchanged.
- Dirty-time CPU, placement, and memory: unchanged.
- Storage: small source/document increase only.

### Acceptance criteria

- Adjacent clusters enter and leave the gust at nearly the same time.
- A visible gust band travels across the field in the configured direction.
- Calm grass sways through both positive and negative displacement and includes a restrained perpendicular component.
- No conspicuous isolated cluster moves outside the shared macro pattern because of random timing.
- Stiffness and blade variation still create small response-amplitude differences.
- Freeze time produces a stable inspectable wave pattern.
- Instance count, deterministic placement hash, instance stride, draw count, geometry, and Ground integration remain unchanged.
- Comprehensive report records the active provider and all temporary wind-wave controls.

### Risks

- Excessive lateral sway can look directionless; the default is capped to a minority of longitudinal calm sway.
- Excessive distortion can make gust timing look noisy; distortion is measured in metres and clamped below half the gust spacing.
- A pulse that is too narrow can look like a hard scan line; smooth edges and a six-metre default width avoid this.
- Unity shader compilation and visual tuning remain required before baseline freeze.


### VEG-WIND-V0.1 post-change consistency and compliance audit

- Actual modified files exactly match the approved four-file scope: the canonical vegetation document, `VegetationBenchmarkWindProvider.cs`, `VegetationBenchmark.cs`, and `VegetationWindResponse.hlsl`. No shader pass, Ground, mesh, editor, scene, prefab, material, camera, URP, River, Weather, Generated Mass, layer, or tag file changed.
- The provider retains temporary external ownership and now publishes direction, prevailing bias, calm strength, gust strength/spacing/width/speed, front distortion/scale, calm scale/speed, lateral ratio, and inspection time. Existing direction and gust-strength serialization remain compatible. Obsolete `steadyStrength`, `gustSpatialFrequency`, and `gustTravelSpeed` serialized names are intentionally not reused because their previous meanings and units are incompatible with the new controls.
- The old `instancePhase * 2π` gust offset is absent. `instancePhase` remains in the function signature only to preserve the existing shader call and instance-layout contract; it does not participate in gust or calm timing.
- Calm displacement is signed along the prevailing axis and signed along its perpendicular. Gust displacement remains nonnegative and aligned with the prevailing direction. Stiffness and `bladeVariation` affect magnitude only.
- Roots remain fixed because all horizontal displacement is multiplied by the existing squared root-to-tip weight. Wind still executes after width-stabilized world transformation.
- The gust front is a smooth periodic pulse in world metres with low-frequency lateral distortion. No texture, compute dispatch, per-frame instance update, placement rebuild, additional draw call, new buffer, or instance-stride change was introduced.
- `VegetationBenchmark.BuildComprehensiveReport` now records the provider object and every new control, including freeze state. Clipboard functionality remains unchanged.
- C#/HLSL global names match for `_VegetationExternalWindDirectionStrength`, `_VegetationExternalWindGustParameters`, `_VegetationExternalWindVariationParameters`, `_VegetationExternalWindCalmLateralRatio`, `_VegetationExternalWindTime`, and `_VegetationExternalWindSourceActive`.
- Delimiter and targeted source scans passed. No `GetInstanceID`, `DestroyImmediate`, sorted `FindObjectsByType`, raw serialized-asset edit, new component type, instance-layout edit, placement edit, or render-submission edit was introduced.
- Analytic sampling confirmed the calm longitudinal and lateral functions cross positive and negative values, adjacent positions receive close phase values, and the gust crest advances in the configured positive wind direction. This is mathematical/source validation, not Unity visual validation.
- Unity 6000.5.0f1 C# compilation, HLSL import, Play Mode appearance, control tuning, and runtime profiling remain pending.

## VEG-WIND-V0.2 — Elastic gust response and varied wind events — SUPERSEDED BY WEATHER-WIND-V0

### Status

- **Read-only audit: COMPLETE**
- **Persistent plan: COMPLETE**
- **Implementation: COMPLETE — SOURCE LEVEL**
- **Post-change audit: COMPLETE — SOURCE LEVEL**
- **Unity validation: PENDING**

### Reviewed evidence and defect

- V0.1 correctly removed full-cycle per-cluster macro timing and produced a coherent traveling world-space gust.
- User validation found the result too geometric: one repeated thick band appeared to switch grass from unbent to bent and directly back to calm.
- The V0.1 pulse directly controlled displacement. It supplied no separate equilibrium bend, active oscillation, elastic overshoot, or diminishing recovery response.
- One primary sinusoidal front distortion and fixed width/strength made successive events visibly repeat the same shape.
- A persistent per-instance spring simulation is unnecessary for this benchmark. Distance behind the moving trailing edge provides an analytic time-since-release value, allowing stateless damped recovery.

### Objective

Replace the direct activation ribbon with a layered cheap analytic response:

1. the gust envelope establishes a moving downwind equilibrium bend;
2. grass oscillates longitudinally and laterally around that bent equilibrium while the gust is active;
3. after the trailing edge, grass first overshoots upwind and then performs diminishing spring-like swings before settling into calm sway;
4. event-index hashing varies gust width, strength, oscillation frequency, and response character coherently per gust rather than randomly per cluster;
5. two broad front-distortion scales prevent one repeated line profile while retaining neighbourhood coherence.

### Approved and actual modified files

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmarkWindProvider.cs`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationWindResponse.hlsl`

No Ground, mesh, instance-layout, editor, shader-pass, material, scene, prefab, camera, URP, Weather, River, Generated Mass, layer, or tag file changed.

### Implementation

- `VegetationBenchmarkWindProvider` now publishes explicit event variation, active oscillation, secondary-front, calm-suppression, and recovery controls through three additional global vectors.
- The HLSL gust evaluator returns a layered sample containing active envelope, directional equilibrium bend, longitudinal active oscillation, lateral active oscillation, and signed recovery oscillation.
- The active envelope is a peaked smooth crest with no flat plateau. A subtle pressure term varies the bend within the event.
- `instancePhase` is used only as a restrained sub-cycle offset for blade flutter and recovery. It does not shift macro gust arrival or departure.
- A deterministic hash of the shared gust event index varies event strength, width, and oscillation frequency. Every cluster inside one event receives the same event character.
- Recovery age is calculated from distance past the moving trailing edge divided by travel speed. Recovery uses a signed damped sinusoid whose first meaningful swing is upwind.
- Calm sway remains signed and multidirectional. It is reduced during the active crest rather than disabled, allowing the strong gust motion to dominate without a discontinuous mode switch.
- The comprehensive clipboard report records every V0.2 control.

### Default test values

- Calm suppression during gust: `0.60`.
- Gust width: `7.5 m`.
- Gust strength variation: `0.22`.
- Gust width variation: `0.24`.
- Gust oscillation strength: `0.20`.
- Gust oscillation frequency: `2.15 Hz`.
- Gust lateral oscillation ratio: `0.28`.
- Secondary front distortion: `0.55 m` at `6.5 m` scale.
- Recovery overshoot: `0.24`.
- Recovery frequency: `2.05 Hz`.
- Recovery damping: `1.35`.
- Recovery duration: `2.4 s`.

These remain temporary visual-test defaults, not production Weather values.

### Performance analysis

- Active CPU: three additional shader-global vector publications per provider update; no per-instance CPU work.
- Active GPU vertex cost: additional analytic hash, low-frequency sine, active flutter, and damped recovery operations. No texture or buffer sample is added.
- Active GPU fragment cost: unchanged; geometry and material coverage are unchanged by this patch.
- Draw count, instance count, instance stride, placement generation, dirty rebuild cost, Ground sampling, and persistent memory are unchanged.
- No performance exception is requested. Runtime profiling remains required before production freeze.

### Acceptance criteria

- The gust no longer reads as a uniform thick line switching grass on and off.
- Grass under strong wind repeatedly swings around a clearly downwind-biased equilibrium rather than holding one fixed bend.
- The trailing edge produces an initial upwind overshoot followed by multiple visibly diminishing swings.
- Calm signed multidirectional sway resumes smoothly after recovery.
- Successive gusts visibly vary in width, strength, and energetic character while remaining coherent events.
- Neighbouring clusters still share macro arrival/departure timing; no return to arbitrary field-wide random activation occurs.
- Roots, placement hash, instance count, draw count, and memory remain unchanged.
- Freeze Time produces a stable spatial envelope; changing Frozen Time reveals the active and recovery regions predictably.

### Post-change consistency and compliance audit

- Actual modified-file scope exactly matches the four approved files.
- C# property IDs and HLSL global declarations match for all nine wind globals.
- Existing global names and V0.1 serialized fields remain intact; new controls are additive.
- Macro gust phase contains no full-cycle per-instance offset. `instancePhase` contributes only a capped `0.55`-radian local flutter offset and a smaller recovery offset.
- Event-index variation is coherent by construction because it is derived from the shared periodic event coordinate, not the instance record.
- Recovery is stateless and bounded by configured duration plus the available inter-gust spatial interval. No history texture, compute dispatch, state buffer, or coroutine was introduced.
- Wind remains multiplied by the existing squared root-to-tip weight and remains after width-stabilized world transformation.
- Placement, mesh construction, instance data, render submission, suite case construction, Ground integration, and candidate selection are untouched.
- Targeted delimiter, symbol, property-parity, and prohibited-API scans passed. Unity 6000.5.0f1 compilation, HLSL import, Play Mode visual validation, and runtime profiling remain pending.

## VEG-WIND-V0.3 — Decorrelated elastic recovery — SUPERSEDED BY WEATHER-WIND-V0

### Status

- **Read-only audit: COMPLETE**
- **Persistent plan: COMPLETE**
- **Implementation: COMPLETE — SOURCE LEVEL**
- **Post-change audit: COMPLETE — SOURCE LEVEL**
- **Unity validation: PENDING**

### Reviewed evidence and defect

- User validation accepted the V0.2 main gust but found its recovery visually resolved into several sharply defined weaker waves following the main event.
- Source review confirmed recovery age was derived from distance behind the trailing edge and fed into one nearly shared damped sinusoid. Equal-distance strips therefore reached nearly equal recovery phase and formed coherent secondary bands.
- V0.2 only applied a very small phase offset and did not materially vary recovery frequency, overshoot, damping, duration, or lateral response per cluster.

### Objective

Preserve a readable shared first recoil caused by the passing gust, then progressively decorrelate the later settling response so grass clusters recover at different speeds, amplitudes, damping rates, durations, phases, and lateral directions without disturbing macro gust coherence.

### Approved and actual modified files

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmarkWindProvider.cs`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationWindResponse.hlsl`

No Ground, mesh, instance-layout, editor, shader-pass, material, scene, prefab, camera, URP, Weather, River, Generated Mass, layer, or tag file changed.

### Implementation

- Added one recovery-variation global vector containing master recovery variation, lateral recovery amount, dephasing, and initial recoil share.
- Recovery now begins with one explicit upwind half-lobe. This preserves the readable release reaction and prevents random clusters from initially moving in physically contradictory directions.
- After that first recoil, frequency, overshoot, damping, and duration receive bounded cluster-level variation derived from existing instance data. No new instance fields or buffers are required.
- Recovery phase disorder ramps in progressively during settling rather than appearing at release.
- A smaller lateral settling component uses a slightly different frequency, phase, and faster damping so blade tips trace irregular shrinking paths rather than one-dimensional synchronized arcs.
- The final quarter of recovery duration fades smoothly to zero instead of ending at a hard step.
- Main gust arrival, width, event character, and active-gust timing remain unchanged and spatially coherent.
- The comprehensive clipboard report now records all V0.3 recovery controls.

### Default test values

- Recovery variation: `0.55`.
- Recovery lateral amount: `0.22`.
- Recovery dephasing: `0.70`.
- Initial recoil share: `0.62`.

These remain temporary visual-test defaults.

### Performance analysis

- Active CPU: one additional global vector publication per provider update; no per-instance CPU animation.
- Active GPU vertex cost: bounded scalar variation math, smooth dephasing/fade terms, and one additional lateral recovery sine/decay path.
- Active GPU fragment cost, draw count, instance count, instance stride, mesh, placement, Ground sampling, and persistent memory are unchanged.
- No texture sample, texture, compute dispatch, state buffer, history field, or additional render submission was introduced.

### Acceptance criteria

- The main gust retains its accepted appearance.
- The first recoil remains broadly readable as a consequence of release from the gust.
- Later recovery no longer forms clearly defined parallel secondary waves behind the gust.
- Neighbouring clusters progressively drift in recovery timing and strength rather than becoming random immediately.
- Some clusters settle faster, others slower, and a restrained lateral component breaks rigid hinge motion.
- Recovery fades smoothly into calm sway with no visible terminal cutoff.
- Placement hash, instance count, draw count, roots, and memory remain unchanged.

### Post-change consistency and compliance audit

- Actual modified-file scope exactly matches the four approved files.
- C# property ID and HLSL declaration match for `_VegetationExternalWindRecoveryVariationParameters`.
- The existing recovery parameter vector remains unchanged; V0.3 is additive and preserves serialized V0.2 controls.
- Macro gust evaluation and event-level variation are untouched. Cluster variation is applied only after the shared gust trailing edge.
- The first recoil has no random phase and is always upwind. Local dephasing begins only in the later settling stage.
- Existing instance phase and blade variation values are reused; no instance-layout change occurred.
- Recovery remains analytic, stateless, duration-bounded, root-weighted, and vertex-only.
- Targeted delimiter, symbol, global-parity, and prohibited-API scans passed. Unity 6000.5.0f1 compilation, HLSL import, Play Mode visual validation, and runtime profiling remain pending.

## WEATHER-WIND-V0 — Shared XZ wind domain and vegetation response field

### Status

- **Read-only review: COMPLETE**
- **Persistent Weather plan: COMPLETE**
- **Implementation: COMPLETE**
- **Post-change audit: COMPLETE AT SOURCE LEVEL**
- **Unity validation: PENDING**

### Authority

`Assets/Docs/Weather_Wind_Architecture.md` is the canonical Weather wind architecture and active implementation ledger. This vegetation document records only the consumer-side contract and historical vegetation experiments.

### Proven reason for replacement

User validation rejected V0.1–V0.3 because a traveling-front model and recovery derived from distance behind that front continued to produce visible main and follower bands. Source evidence is `Assets/Game/Rendering/Vegetation/Includes/VegetationWindResponse.hlsl`, where gust and recovery phase are functions of along-wind distance, event spacing, and trailing-edge distance. Parameter variation cannot remove the ordered equal-age contours inherent in that model.

### Consumer contract

- Vegetation samples a Weather-owned gameplay-anchor-centred XZ response texture.
- The response field stores bend and bend velocity and is updated as a damped spring toward a separate target-wind texture.
- Vegetation keeps root-to-tip weighting, per-cluster stiffness, and restrained high-frequency blade detail only.
- Vegetation does not own prevailing direction, gust shape, recovery timing, field coordinates, or update cadence.
- The field is fixed-density in metres per cell and is independent of vegetation-patch size.
- Future stylized wind lines consume the Weather target field. Future gameplay consumes CPU Weather samples.

### Approved implementation scope

Create:

- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherWindDomain.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherWindDomainEditor.cs`
- `Assets/Game/Rendering/Weather/Includes/WeatherWindField.hlsl`
- `Assets/Game/Rendering/Weather/Resources/PS3DWeather/Compute/CS_WeatherWindField.compute`
- corresponding new `.meta` files

Modify:

- this document;
- `Assets/Docs/Stylized_Vegetation_Architecture.md`;
- `Assets/Game/Procedural/Vegetation/VegetationBenchmarkWindProvider.cs`;
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`;
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`;
- `Assets/Game/Rendering/Vegetation/Includes/VegetationWindResponse.hlsl`.

No Ground, River, vegetation mesh, instance-layout, vegetation shader pass, scene, prefab, material, camera, URP asset, layer, or tag changes.

### Vegetation acceptance criteria

- No V0.1–V0.3 analytical gust/recovery global remains in active vegetation HLSL.
- Grass samples one shared Weather response field and retains fixed roots.
- Placement hash, instance count, stride, mesh, draw count, Ground sampling, and silhouette controls remain unchanged.
- The existing scene's legacy provider component continues loading through a hidden migration subclass but contains no old wind behavior.
- Vegetation diagnostics report Weather-domain resource and publisher status.
- Unity validation confirms coherent irregular wind and the absence of hard main or follower ribbons.


### WEATHER-WIND-V0 vegetation-side post-change audit

- `VegetationWindResponse.hlsl` now consumes exactly one shared Weather response sample and contains no analytical gust front, trailing-edge age, event spacing, recovery band, or `_VegetationExternalWind*` global.
- Vegetation retains only root weighting, per-instance stiffness/variation, and restrained high-frequency detail motion.
- `VegetationBenchmark.cs` and its Inspector report the active Weather domain and embed the one-button Weather report; the old provider-specific parameter report is removed.
- Placement, deterministic hashes, instance stride, mesh generation, geometry candidates, Ground coverage, rendering submission, and timed-suite matrices are untouched.
- The legacy scene component remains loadable solely through the migration subclass. Replacing that component with `WeatherWindDomain` through Unity later is a serialized-scene cleanup, not a behavior dependency.
- Unity compilation, compute/shader import, Play Mode visual validation, and benchmark profiling remain pending.


---

## VEG-V1B.2 — Reliable Edit-Mode rendering control and startup synchronization

### Status

- **Read-only audit: COMPLETE**
- **Persistent plan: COMPLETE**
- **Implementation: COMPLETE — SOURCE LEVEL**
- **Post-change audit: COMPLETE — SOURCE LEVEL**
- **Unity validation: PENDING**

### Objective

Make vegetation render reliably in both Scene and Game views while Unity is not in Play Mode, without requiring one prior Play Mode run. Replace the two rendering action buttons with one serialized two-option dropdown that defaults to `Enabled`.

### Proven defects and evidence

- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs > OnEnable()` immediately calls `RebuildBenchmark()`.
- `Assets/Game/Procedural/Ground/GeneratedGround.cs > OnEnable()` also performs regeneration. Component enable ordering is not guaranteed; the user-provided Inspector screenshot shows `Ready: 0 instances` with authored Ground coverage present, matching a vegetation build that sampled Ground before its `baseSurface` became valid.
- `VegetationBenchmark.ScheduleGroundStartupRetryIfNeeded()` explicitly exits when `Application.isPlaying` is false. The accepted V1B.1 repair therefore cannot recover this same ordering failure in Edit Mode.
- `VegetationBenchmark.LateUpdate()` is the only gameplay-camera submission path. `[ExecuteAlways]` does not make `LateUpdate()` a reliable per-rendered-camera callback in Edit Mode.
- `VegetationBenchmarkSceneViewPreview.HandleBeginCameraRendering()` filters strictly to `CameraType.SceneView`; the Edit Mode Game camera receives no explicit SRP callback submission.
- `VegetationBenchmarkEditor.OnInspectorGUI()` exposes `Enable Rendering` and `Disable Rendering` as two separate buttons even though both mutate the same serialized Boolean.

### Approved files

Modify only:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`

### Invariants and non-goals

- Preserve Play Mode indirect rendering and timed-suite behavior.
- Preserve deterministic placement, Ground coverage acceptance, instance layout, mesh generation, shader behavior, Weather sampling, and draw count.
- Do not rebuild the full vegetation field every editor frame.
- Edit Mode recovery may perform one cheap Ground-centre sample per rendered editor camera until Ground becomes valid, then exactly one full rebuild for that Ground surface revision.
- Scene-view preview remains independently user-controllable.
- The global rendering dropdown controls both Scene and Game submissions and defaults to `Enabled` through the existing serialized `true` default.
- Do not modify Ground, Weather, shader, compute, scene, prefab, material, camera, URP, layer, or tag files.

### Implementation sequence

1. **COMPLETE** — Hid the backing `renderBenchmark` Boolean from the default Inspector while preserving its serialized Boolean representation, `true` default, reports, timed-suite save/restore, and runtime setter.
2. **COMPLETE** — Added one custom `Rendering` popup with exactly `Enabled` and `Disabled`; removed both rendering action buttons.
3. **COMPLETE** — Added bounded Edit Mode Ground-readiness synchronization. A zero-instance build caused by Ground sampling records a pending state; editor rendering probes the Ground centre and rebuilds once for the first valid surface revision.
4. **COMPLETE** — Moved Edit Mode Scene/Game submissions to the existing SRP camera callback. `LateUpdate()` now submits only during Play Mode.
5. **COMPLETE** — Filtered editor submissions to Scene cameras and the assigned Game camera, or all Game cameras only when no target camera is assigned. Preview and Reflection cameras remain excluded.
6. **COMPLETE — SOURCE LEVEL** — Preserved existing report fields, updated the canonical record, and completed scope, lifecycle, duplicate-submission, serialization, reference, and lexical checks. Unity validation remains pending.

### Performance analysis

- Active Play Mode CPU/GPU cost: unchanged.
- Edit Mode steady-state cost: one existing indirect draw per eligible Scene/Game camera.
- Edit Mode startup race only: one cheap Ground-centre sample per editor camera render until Ground becomes valid, followed by one ordinary O(candidate-count) rebuild.
- No additional textures, buffers, instances, compute dispatches, persistent allocations, or fragment work.

### Acceptance criteria

- With `Rendering = Enabled`, grass appears in Scene and Game views after script/domain reload without entering Play Mode.
- `Rendering = Disabled` hides both views immediately; returning to `Enabled` resumes them without requiring Play Mode.
- The Inspector exposes one `Rendering` dropdown with only `Enabled` and `Disabled`; new/default components begin enabled.
- Scene View Preview still independently controls only Scene-view submission.
- An initialized-empty coverage mask may intentionally remain at zero instances without triggering repeated rebuilds.
- Play Mode startup synchronization, timed-suite restoration, reports, placement hashes, and draw topology remain unchanged.


### Post-change consistency and compliance audit

- Final modified-file scope matches the approved three files exactly.
- `renderBenchmark` remains a serialized Boolean with a `true` declaration default. `[HideInInspector]` removes only the duplicate default-Inspector checkbox; the custom two-option popup remains the single rendering control.
- Timed-suite code still saves, mutates, and restores the same `renderBenchmark` field. Comprehensive reporting still records the same value.
- Play Mode still submits through `LateUpdate()` exactly once with the configured target camera. The new `Application.isPlaying` guard only removes the unreliable Edit Mode use of that path.
- Edit Mode submission is centralized in one `RenderPipelineManager.beginCameraRendering` subscription. It accepts only `CameraType.SceneView` and `CameraType.Game`; Preview and Reflection cameras remain excluded.
- Scene View Preview still controls only Scene-view submission. The global Rendering value is enforced by the shared `SubmitIndirectRender()` path for both Scene and Game cameras.
- The edit startup synchronization performs no full-field rebuild per frame. It samples one Ground-centre point while a proven zero-instance Ground rejection is pending, records the attempted Ground surface revision, and performs one rebuild when valid.
- An initialized-empty vegetation coverage mask does not schedule synchronization because its candidates are rejected by coverage, not by invalid Ground samples.
- Placement generation, deterministic hash inputs, cluster mesh construction, instance stride, buffers, shader properties, Weather field sampling, Ground implementation, and suite matrices are unchanged.
- Direct contract verification confirmed `GeneratedGround.VegetationCoverageSurfaceRevision` and `GeneratedGround.TrySampleBaseSurface(...)` exist in the current supplied source.
- Static checks passed for balanced C# delimiters and terminated strings/comments, required namespace coverage, one rendering popup, removal of both action buttons, one editor preview call site, absence of the superseded `RenderSceneViewPreview` call, Scene/Game camera filtering, and no full rebuild inside `LateUpdate()`.
- No C# compiler with Unity assemblies is available in this environment. Unity 6000.5.0f1 compilation and visual Scene/Game validation remain pending.

---

## VEG-V1C — Shared URP Sun, Ambient, and Local-Light Response

### Status

**Unity C# compilation, shader import, and visible local-light response validated by the user on 2026-07-20. Full time-of-day sweep and dense-field GPU profiling remain pending.**

### Objective

Replace the vegetation benchmark shader's fixed ambient floor and main-light-only approximation with a cheap stylized URP lighting path that responds to:

- the ambient colour and intensity published through Unity `RenderSettings` by the existing time-of-day system;
- the URP main directional light, including its live direction, colour, and intensity;
- URP additional point and spot lights with distance and spot attenuation.

Vegetation must not reference `TimeOfDayController` directly. The controller already publishes the scene lighting state through `RenderSettings.sun`, ambient settings, and the URP light system. Vegetation consumes that shared renderer contract.

### Acceptance criteria

1. Time-of-day changes in Edit Mode and Play Mode visibly change grass ambient colour and brightness.
2. Rotating the directional sun changes the direct-light response across grass.
3. Deep night no longer leaves grass at a fixed bright-green floor.
4. Existing local point/spot lights visibly brighten and tint nearby grass with normal URP attenuation.
5. Lighting-only Inspector changes update the runtime material without rebuilding placement, instance buffers, or cluster geometry.
6. Draw count, instance stride, deterministic placement, Weather wind integration, and no-shadow baseline remain unchanged.

### Approved files

Create:

- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl`
- corresponding `.meta`

Modify:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

No changes are authorized for `TimeOfDayController`, time-of-day profiles, scene lights, Weather wind, Ground, vegetation meshes, instance data, scenes, prefabs, cameras, URP assets, layers, or tags.

### Reviewed evidence

| Evidence | Finding | Consequence |
| --- | --- | --- |
| `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`, `Frag` | Current lighting is `_AmbientStrength` plus `GetMainLight()` wrapped diffuse. There is no SH ambient sample and no additional-light loop. | Grass cannot follow time-of-day ambient and cannot react to point/spot lights. |
| Same shader, properties | `_AmbientStrength` defaults to `0.85`, producing a large fixed brightness floor independent of scene ambient state. | Deep-night grass remains visibly bright. |
| `Assets/Game/Scripts/Environment/Lighting/TimeOfDayController.cs`, `ApplySun` and `ApplyAmbientAndReflections` | Existing system rotates the celestial rig and publishes sun colour/intensity plus flat ambient colour/intensity through standard Unity state. | No direct vegetation-to-time-of-day dependency is required. |
| `Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterLighting.hlsl` | Project already uses `SampleSH`, `GetMainLight`, and Forward/Forward+ additional-light loops in URP. | Vegetation can follow the same project-proven URP integration pattern with foliage-specific diffuse response. |
| `Assets/Docs/Stylized_Vegetation_Architecture.md`, rendering and V2 sections | The exploratory architecture already reserves stylized lighting and no-ShadowCaster patch-edge shading as a later layer. | Record the accepted shared URP lighting contract while preserving its non-authoritative historical role. |
| `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`, `RebuildBenchmark` | Runtime material currently receives only colours and silhouette parameters. | New lighting parameters require explicit material-property publication. |
| `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`, `OnInspectorGUI` | Any default-Inspector change currently invokes `RebuildBenchmark()`. | Lighting tuning would unnecessarily rebuild tens of thousands of placements; configuration-change classification is required. |
| Supplied source archive | No `.git` directory is present. | Branch, HEAD, working-tree diff, and history comparisons are unavailable and must not be invented. |

### Lighting model

The first lighting pass remains non-PBR and texture-free:

```text
authored root/body/tip colour
× max(
    ambient SH × Ambient Response
    + main directional light × Sun Response
    + additional point/spot lights × Local Light Response,
    Minimum Night Visibility)
```

Direct lights use a two-sided wrapped diffuse response suitable for thin `Cull Off` vegetation cards:

```text
wrapped = saturate((abs(dot(normalWS, lightDirectionWS)) + wrap) / (1 + wrap))
```

The existing normal-up bias becomes an exposed material control. Light-colour influence interpolates between luminance-only response and full light colour.

### Initial controls and defaults

- Ambient Response: `1.0`
- Sun Response: `1.0`
- Local Light Response: `1.0`
- Minimum Night Visibility: `0.04`
- Diffuse Wrap: `0.35`
- Normal Up Bias: `0.42`
- Light Colour Influence: `1.0`

These are initial tunable defaults, not a frozen production profile.

### File-by-file implementation sequence

1. Update the exploratory stylized-vegetation document with the accepted shared URP lighting ownership and first-pass constraints.
2. Create `VegetationLighting.hlsl` with ambient SH, main-light, additional-light, light-colour, and two-sided wrapped-diffuse helpers.
3. Update the vegetation shader with Forward/Forward+ additional-light variants, wind-deformed world position in varyings, new material controls, and the shared lighting include.
4. Add serialized lighting controls, material-property IDs, a material-only refresh method, configuration hashes, validation clamps, and lighting diagnostics to `VegetationBenchmark`.
5. Update the custom Inspector to rebuild only when geometry/placement configuration changes and to refresh only material properties when lighting changes.
6. Complete source contract checks and record the final compliance result here.

### Invariants and non-goals

- Grass shadow casting remains off.
- Real-time shadow receiving remains off and no shadow variants or shadow-map samples are added.
- No PBR, specular, reflection-probe, normal-map, translucency, or subsurface model is added.
- No new debug view is added.
- No new draw, instance buffer, texture sample, compute dispatch, or CPU runtime update is added.
- Weather wind and lighting remain independent shared systems.

### Performance assessment

Active fragment work increases by one SH ambient evaluation, one main-light evaluation, and an additional-light loop. The variable cost is proportional to visible additional lights affecting grass pixels. No fragment textures, PBR terms, shadow-map samples, new passes, or geometry changes are added.

Lighting control edits become material-only dirty work. Geometry, placement, and buffer rebuild cost remains unchanged and is not invoked for lighting-only edits.

### Validation plan

- Static C#/HLSL property-name and include parity.
- Additional-light and Forward+ variant/loop parity against the current project river implementation.
- Changed-file parser and delimiter checks.
- Search for unresolved new symbols and required namespaces/imports.
- Unity C# compile and vegetation shader import.
- Edit Mode and Play Mode visual checks across midday, deep night, sun rotation, and local-light enable/disable.

### Post-change compliance status

**Source audit result: PASS with Unity validation pending.**

Actual changed files:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl.meta`

Scope reconciliation:

- The canonical plan, runtime, editor, shader, new include, and exploratory vegetation document changed as approved.
- No Ground, Weather, time-of-day, scene-light, mesh, instance-layout, scene, prefab, material, camera, URP, layer, or tag file changed.
- `Stylized_Vegetation_Architecture.md` was added to the declared scope before modification after the documentation review confirmed it is the relevant high-level vegetation background document.

Implemented differences:

- Removed the fixed `_AmbientStrength = 0.85` and `_DirectStrength` shader path.
- Added ambient SH, main directional light, and Forward/Forward+ point/spot-light evaluation.
- Added wind-deformed world position to fragment varyings for local-light attenuation.
- Added two-sided wrapped diffuse, normal-up bias, light-colour influence, and minimum-night visibility controls.
- Added material-only lighting publication and diagnostics to `VegetationBenchmark`.
- Added Inspector configuration classification so lighting-only edits do not rebuild placement, mesh, or buffers.
- Preserved one indirect draw, 48-byte instance data, placement hashes, Weather wind sampling, no ShadowCaster pass, and `receiveShadows = false`.

Source checks completed:

- Exact changed-file scope and no-removal check: passed.
- C#/ShaderLab/HLSL delimiter, comment, and string checks: passed for every changed source file.
- C# material-property ID to ShaderLab property/CBUFFER parity: passed for all seven new controls.
- Old `_AmbientStrength` and `_DirectStrength` references: none remain in active vegetation runtime or shader source.
- Forward/Forward+ loop scaffolding parity with the project river lighting implementation: passed, including `InputData.normalizedScreenSpaceUV` for the URP light-loop macro.
- New include path and `.meta` GUID uniqueness: passed.
- New C# references require no additional namespace beyond existing `UnityEngine` and `UnityEngine.Rendering`; unresolved-reference search found none.

Unity validation update:

- Unity 6000.5.0f1 C# compilation and vegetation shader import passed in the user's project.
- Visible local point-light response passed user screenshot validation on 2026-07-20.
- A complete time-of-day ambient/sun sweep and dense-field GPU timing remain pending.

Known retained limitation:

- Wind bends vertex positions but not normals. Strong-bend lighting-normal correction remains deferred unless visual validation proves it is required.

---

## VEG-V1C.1 — Unity 6.5 EntityId Compatibility Fix

### Status

**Unity 6000.5.0f1 compilation validated by the user on 2026-07-20; the obsolete EntityId conversion no longer blocks the vegetation shader test.**

### Objective

Remove the obsolete implicit conversion from `UnityEngine.EntityId` to `int` introduced in `VegetationBenchmark.ComputeRebuildConfigurationHash()` while preserving the rebuild-configuration identity check and all rendering ownership behavior.

### Approved files

Modify:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`

No shader, editor, lighting, Weather, Ground, scene, prefab, material, camera, URP, layer, tag, mesh, buffer, or instance-layout changes are authorized.

### Reviewed evidence

| Evidence | Finding | Consequence |
| --- | --- | --- |
| Unity compiler output | `VegetationBenchmark.cs(493,46)` reports CS0619 because `EntityId.implicit operator int(EntityId)` is obsolete in Unity 6000.5.0f1. | The ternary expression currently forces `coverageGround.GetEntityId()` into `int`. |
| `VegetationBenchmark.cs`, `ComputeRebuildConfigurationHash()` | `CombineHash` accepts `int`; the Ground identity expression is `coverageGround != null ? coverageGround.GetEntityId() : 0`. | Replace the obsolete conversion with an explicit hash of the `EntityId` value. |
| `VegetationBenchmark.cs`, `SubmitIndirectRender()` | `RenderParams.entityId = gameObject.GetEntityId()` assigns `EntityId` directly to its intended field. | This valid assignment must remain unchanged. |
| Full vegetation source search | Exactly two `GetEntityId()` calls exist: one valid `RenderParams.entityId` assignment and one invalid hash conversion. | Scope is one code expression plus documentation. |

### Implementation plan

1. Replace the Ground configuration-hash expression with `coverageGround.GetEntityId().GetHashCode()` so `CombineHash` still receives an `int` without invoking the obsolete conversion operator.
2. Search the complete changed file and supplied vegetation source for any remaining implicit `EntityId` to `int` conversions.
3. Run C# lexical/delimiter checks and reconcile the final diff with the two-file scope.
4. Record Unity compilation as pending until the user recompiles in Unity 6000.5.0f1.

### Invariants and performance

- `RenderParams.entityId` continues to receive `gameObject.GetEntityId()` directly.
- Rebuild configuration semantics remain identity-based; only the conversion mechanism changes.
- No runtime rendering, placement, lighting, memory, draw-call, or fragment cost changes.
- No new API or dependency is introduced.

### VEG-V1C.1 post-change consistency and compliance audit

**Source result: PASS. Unity 6000.5.0f1 compilation subsequently passed in the user's project on 2026-07-20.**

Actual modified files:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`

Final behavior change:

- `ComputeRebuildConfigurationHash()` now hashes `coverageGround.GetEntityId()` through `EntityId.GetHashCode()` instead of invoking the obsolete implicit `EntityId` to `int` conversion.

Preserved behavior:

- `RenderParams.entityId` still receives `gameObject.GetEntityId()` directly.
- The rebuild hash still changes when the assigned Ground identity changes.
- Placement, rendering, lighting, wind, coverage sampling, buffers, instance layout, draw count, and Inspector behavior are unchanged.

Checks completed:

- Exact two-file scope: passed.
- Full vegetation source search found exactly two `GetEntityId()` uses: the valid direct `RenderParams.entityId` assignment and the explicit `GetHashCode()` use in the rebuild hash.
- Search found no remaining conditional or cast-based implicit `EntityId` to `int` conversions in the vegetation source.
- C# delimiter, comment, string, and character-literal lexical check: passed.
- Unity's `EntityId` API documents `GetHashCode()` as the supported hash-code operation for an `EntityId`; no new compatibility shim or obsolete instance-ID API was introduced.

Validation update:

- The user successfully recompiled and visually validated the VEG-V1C lighting path on 2026-07-20; CS0619 no longer blocks the project.

---

## VEG-V1C.2 — Light-Directional Stylized Blade Edge Accent

### Status

**Superseded by VEG-V1C.3. Unity compilation and shader import passed in the user project; visual suitability failed because even strength `1.0` and width `0.20` produced mainly a broad bright wash rather than clear edge lines.**

### Objective

Add a restrained manga/cartoon-like direct-light accent to the visible light-facing edge of each grass blade while slightly reducing the broad direct-light fill. The effect must respond to the live URP main light and additional point/spot lights, remain absent from ambient SH, and preserve the exact accepted VEG-V1C lighting result when its master strength is `0`.

### Acceptance criteria

1. `Stylized Edge Accent = 0` reproduces the current VEG-V1C direct-light result without broad-fill suppression or edge addition.
2. The default accent is subdued: broad sun/local-light response is only slightly reduced, while a thin brighter line appears on the blade edge facing each direct light.
3. Nearby strong point/spot lights produce the clearest coloured edge accent with normal URP attenuation; opposite blade edges do not receive the same full accent.
4. The sun produces a weaker field-wide directional accent without creating a permanent ambient outline.
5. The accent follows the actual clipped card silhouette and the geometry edges of opaque strips, with derivative softening to limit shimmer.
6. Lighting-control changes remain material-only and do not rebuild placement, meshes, instance buffers, or deterministic hashes.

### Approved files

Modify:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

No new files are authorized. No changes are authorized for time-of-day, Weather, Ground, vegetation mesh generation, instance layout, scene, prefab, material asset, camera, URP asset, layer, tag, shadow pass, or compute code.

### Reviewed evidence

| Evidence | Finding | Consequence |
| --- | --- | --- |
| `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`, `Frag` | UV0 spans `0..1` across every generated strip/card. Card candidates clip a tapering silhouette in the fragment shader. | A normalized silhouette coordinate can generate a geometry-free edge mask that follows both actual strip edges and clipped card edges. |
| `Assets/Game/Procedural/Vegetation/VegetationClusterMeshBuilder.cs`, `AddStrip` | Vertex order stores left vertices at `uv.x = 0`, right vertices at `uv.x = 1`; the mesh normal is `cross(up, right)`. | The blade lateral axis can be reconstructed from the unflipped world normal, and signed UV side can select the edge facing each light. No mesh or instance-data change is required. |
| `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl` | Ambient, sun, and local-light terms are already separated. All direct lights pass through one helper. | Broad-fill suppression and edge addition can be applied only to direct lights while leaving ambient/time-of-day SH unchanged. |
| `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`, lighting fields/hash/refresh/report | VEG-V1C already has a material-only lighting configuration path. | Two new lighting controls can use the same path without rebuilding vegetation. |
| `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`, `OnInspectorGUI` | Lighting hash changes call only `RefreshLightingMaterialProperties()`. | No editor architecture change is required beyond ensuring the new controls participate in the lighting hash. |
| `Assets/Game/Scripts/Environment/Lighting/TimeOfDayController.cs`, `ApplySun` / `ApplyAmbientAndReflections` | Sun and ambient continue to publish through standard Unity/URP state. | The new accent must remain a vegetation shader response and must not add a direct time-of-day dependency. |
| Supplied source archive and accepted patch sequence | No `.git` directory is available. Current source was reconstructed from the supplied archive plus accepted VEG-V1C and VEG-V1C.1 patches. | Git status, HEAD, branch, and historical diff checks are unavailable and must remain explicitly pending rather than invented. |

### Lighting model

For every direct light:

```text
broad direct = accepted VEG-V1C direct term × (1 - 0.35 × accent)
edge direct  = light colour × attenuation × response
               × edge mask × light-facing-side mask
               × subdued accent gain
```

Ambient SH and Minimum Night Visibility are unchanged. The edge term uses the same HDR light colour and URP attenuation as the broad direct term.

### Controls and defaults

- Stylized Edge Accent: range `0..1`, default `0.22`.
- Edge Accent Width: range `0.01..0.35`, default `0.10` of normalized half-blade width.

The two controls are intentionally coupled for the first suitability test. Additional fill/edge controls are not added unless visual validation proves the coupled control insufficient.

### File-by-file implementation sequence

1. Update this canonical plan before any implementation edit.
2. Update `Stylized_Vegetation_Architecture.md` with the accepted directional-edge design and constraints.
3. Add shader properties, material CBUFFER values, silhouette-coordinate calculation, derivative-softened edge mask, and blade-lateral reconstruction to the vegetation shader.
4. Extend `VegetationLighting.hlsl` so direct-light evaluation applies the coupled fill reduction and light-facing edge term to both sun and local lights; keep ambient unchanged.
5. Add serialized controls, property IDs, validation clamps, lighting-hash entries, material publication, and report output to `VegetationBenchmark`.
6. Reread and audit the complete final files, verify property/signature parity and zero-strength equivalence, then record the post-change result here.

### Invariants and non-goals

- No camera/Fresnel rim light.
- No two-edge ambient outline.
- No texture sample, geometry, draw call, buffer, compute dispatch, instance-layout, or placement change.
- No ShadowCaster pass or real-time shadow receiving.
- No change to the accepted ambient SH, sun/local-light ownership, day/night response, wind, or Ground behavior.
- No new debug view.

### Risks and validation

- Dense subpixel grass can shimmer if the edge threshold is hard. The shader must use `fwidth`-based softening.
- Card clipping changes the visible horizontal edge; the edge coordinate must normalize against the same effective half-width used by `clip`.
- Light-facing edge selection depends on consistent mesh UV and lateral orientation; static source checks must verify the mesh-builder convention and shader reconstruction.
- Fragment cost increases by one derivative-based edge mask plus scalar direct-light work. Dense-field GPU profiling remains required.
- Unity 6000.5.0f1 C# compilation and URP shader import cannot be executed in this environment and remain pending.


### VEG-V1C.2 post-change consistency and compliance audit

**Source result: PASS. Unity 6000.5.0f1 compilation, URP shader import, visual suitability testing, and dense-field GPU profiling remain pending.**

Actual modified files:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

Implemented differences:

- Added `Stylized Edge Accent` with default `0.22` and `Edge Accent Width` with default `0.10`.
- Added both controls to validation, the material-only lighting hash, runtime material publication, and comprehensive reporting.
- Added a derivative-softened silhouette-edge mask. Opaque strips use their geometry UV edge; card candidates normalize against the same effective half-width used by the existing fragment `clip`.
- Reconstructed the positive blade-lateral direction from the unflipped mesh normal using the reviewed `normal = cross(up, right)` mesh-builder convention.
- Applied broad-fill suppression and the light-facing edge term only inside shared direct-light evaluation, so both the sun and additional point/spot lights use the same path while ambient SH remains unchanged.
- Updated the custom Inspector guidance and exploratory vegetation architecture without adding controls, views, assets, or dependencies outside the approved design.

Preserved behavior and invariants:

- At `Stylized Edge Accent = 0`, direct-light algebra reduces to the accepted VEG-V1C term: broad scale is `1`, edge contribution is `0`, and ambient/minimum-night logic is unchanged.
- Default `0.22` produces a broad direct-fill multiplier of `0.923` and a bounded maximum edge gain coefficient of `0.165` before light colour, attenuation, response, edge mask, and diffuse support.
- Left/right source verification confirmed `uv.x = 0/1`, mesh normal `cross(up, right)`, and shader reconstruction `cross(normal, up)` select opposite edges for opposite lateral light directions.
- Placement hash, mesh generation, 48-byte instance layout, buffers, one indirect draw, wind sampling, Ground integration, time-of-day ownership, no-ShadowCaster baseline, and `receiveShadows = false` are unchanged.
- No texture sample, new pass, geometry, draw call, compute dispatch, CPU runtime update, debug view, scene/prefab/material asset, camera, URP, layer, or tag change was introduced.

Checks completed:

- Exact six-file changed scope: passed.
- C#, ShaderLab, and HLSL delimiter, string, character, and comment lexical checks: passed.
- C# property-ID, ShaderLab property, material CBUFFER, lighting hash, and material refresh parity for both controls: passed.
- Rebuild-hash exclusion and material-only Inspector classification: passed.
- Card clip/edge-coordinate parity and `fwidth` antialiasing presence: passed.
- Direct-light helper signature/call parity across main, Forward+, and additional-light loops: passed.
- No active `ShadowCaster`, texture sample, or obsolete EntityId conversion regression found.
- Numerical zero-accent comparison over representative randomized inputs matched VEG-V1C within floating-point precision.

Limitations and next actions:

- Unity assemblies and the URP shader compiler are unavailable in this environment, so Unity compilation/import cannot be claimed.
- Visual validation must determine whether the coupled fill suppression and edge gain are suitably subdued and whether the line remains stable at gameplay distance.
- Dense-field GPU timing is required because the change adds one derivative-based edge mask per fragment and scalar work per direct light.

---

## VEG-V1C.3 — Strong Stylized Light Edge Lines

### Status

**Rejected by user visual validation and superseded by VEG-V1C.4.** The strong additive formula is retained only for eligible punctual local lights; the VEG-V1C.3 sun/directional eligibility and residual edge-selection behavior are invalid.

### Objective

Replace the underpowered VEG-V1C.2 accent equation with a materially stronger graphic edge-light model. Strong nearby point/spot lights must be able to produce near-white light-facing blade edges at close range, while low-to-medium-distance grass retains recognizable highlighted striations instead of collapsing into only a broad light wash. The master accent value of `0` must remain an exact fallback to accepted VEG-V1C broad lighting.

### Acceptance criteria

1. `Stylized Edge Accent = 0` preserves the accepted VEG-V1C body-light result exactly and produces no edge overlay.
2. At `Stylized Edge Accent = 1`, a close strong local light can drive the selected blade edge to an HDR near-white result without requiring an albedo-bright green body.
3. The base equation is substantially stronger than VEG-V1C.2 even before new controls are tuned: the edge term is no longer multiplied by grass albedo and its gain rises nonlinearly with the master accent.
4. The light-facing side remains dominant. Opposite edges may retain only a subdued residual response when the light is nearly normal to the blade plane, preventing unstable disappearance across differently oriented cards.
5. A configurable derivative-based screen-space persistence term keeps edge structure visible at low-to-medium distance without adding geometry or textures.
6. New controls remain material-only and do not rebuild placement, meshes, instance buffers, or deterministic placement data.

### Approved files

Modify:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

No new files are authorized. No changes are authorized for time-of-day, Weather, Ground, vegetation mesh generation, instance layout, scene, prefab, material assets, camera, URP assets, shadow passes, layers, tags, or compute code.

### Reviewed evidence

| Evidence | Finding | Consequence |
| --- | --- | --- |
| User validation screenshot, 2026-07-20 | At width `0.20` and strength `1.0`, the result reads mainly as a broad bright patch; individual edge lines are not clearly distinguishable. | Tuning alone is insufficient. The base equation must change. |
| `VegetationLighting.hlsl`, `VegetationEvaluateDirectLight` | VEG-V1C.2 caps edge gain at `0.75 × accent` and returns it inside the lighting multiplier later multiplied by grass albedo. | The edge cannot approach white reliably on dark/green albedo and remains subordinate to body lighting. |
| `SH_StylizedVegetationBenchmark.shader`, `Frag` | Final colour is currently `heightColor.rgb × lighting`. The edge mask already follows clipped card silhouettes and strip geometry edges. | Keep the validated mask, but separate direct edge radiance from albedo-modulated body lighting. |
| `VegetationClusterMeshBuilder.cs`, `AddStrip` | UV0 and normals provide stable left/right and lateral conventions across all candidates. | No geometry or instance-data change is required. |
| `VegetationBenchmark.cs`, lighting hash/material refresh | Existing lighting controls update only runtime material properties. | Two new controls can remain dirty-time/material-only. |
| `VegetationBenchmarkEditor.cs`, lighting-hash branch | Lighting changes avoid full rebuilds. | Preserve this path and update guidance/version labels only. |
| Supplied source archive and accepted patch sequence | No `.git` directory is available. The current state is reconstructed from supplied archives. | Git status, branch, HEAD, and historical-diff checks are unavailable and remain explicitly pending. |

### Strong edge-light model

The accepted ambient and body-light path remains albedo-modulated:

```text
body colour = grass albedo × (ambient + broad sun + broad local lights)
```

The new stylized edge is separate direct radiance:

```text
edge colour = whitened direct-light colour
              × attenuation × response
              × persistent silhouette-edge mask
              × light-facing-side weight
              × diffuse support
              × nonlinear accent gain

final colour = body colour + edge colour
```

Base equation changes relative to VEG-V1C.2:

- Broad direct fill is reduced only to `0.75` at master accent `1`, rather than `0.65`, preserving readable bodies while the line carries the stylization.
- Edge gain is `accent × lerp(1.5, 4.0, accent)`, producing `4.0` at maximum instead of `0.75`.
- Edge radiance is added after albedo multiplication, allowing near-white/HDR highlights on green blades.
- Light-facing selection is shaped rather than multiplied directly by the raw lateral dot, so moderately oblique lights still generate readable lines.
- Screen-space persistence expands only the edge mask as derivatives grow; it does not alter placement, geometry, or the body-light footprint.

### Controls and defaults

Existing:

- `Stylized Edge Accent`: `0..1`, default remains `0.22`.
- `Edge Accent Width`: `0.01..0.35`, default remains `0.10`.

New:

- `Edge Highlight Whiteness`: `0..1`, default `0.75`. Preserves HDR intensity while moving direct-light chroma toward white.
- `Edge Distance Persistence`: `0..1`, default `0.65`. Controls derivative-based minimum screen-space edge presence.

No separate sharpness control is added in this patch. The base mask is made decisively narrower/stronger through the new formula and persistence shaping to avoid unnecessary control bloat.

### File-by-file implementation sequence

1. Record this canonical plan before any implementation edit. **Complete.**
2. Update `Stylized_Vegetation_Architecture.md` with the stronger separate-radiance edge model and two new controls. **Complete.**
3. Extend `VegetationBenchmark.cs` with property IDs, serialized controls, validation, lighting hash, material publication, and report output. **Complete.**
4. Update `VegetationBenchmarkEditor.cs` guidance/version labels while preserving the material-only update path. **Complete.**
5. Refactor `VegetationLighting.hlsl` to return albedo-modulated body lighting separately from additive edge radiance, apply nonlinear gain, whitened HDR colour, and stronger side selection to main and additional lights. **Complete.**
6. Update the shader properties/CBUFFER, build the persistent silhouette mask, pass new parameters, and compose final colour as body plus edge. **Complete.**
7. Complete cross-file property/signature parity checks, zero-accent equivalence, source lexical checks, final-scope audit, and record the post-change result here. **Complete.**

### Invariants and non-goals

- No camera/Fresnel rim lighting.
- No ambient outline.
- No texture sample, geometry, pass, draw call, buffer, compute dispatch, instance-layout, or placement change.
- No ShadowCaster pass or real-time shadow receiving.
- No time-of-day or local-light ownership change.
- No new debug view.
- Existing card clipping and width-stabilization behavior remain unchanged.

### Risks and validation

- A stronger HDR edge can bloom excessively around very bright lights. The master accent and whiteness remain exposed, and visual validation must include bloom-on and bloom-off inspection.
- Derivative persistence can broaden subpixel blades into bright slivers. The effect is bounded by the master accent and the `0..1` persistence control; medium-distance stability must be checked from the gameplay camera.
- Multiple overlapping local lights can accumulate additive edges. Existing URP light attenuation remains active, but dense-field local-light overlap requires visual and GPU validation.
- Unity 6000.5.0f1 compilation and URP shader import cannot be run in this environment and remain pending.


### VEG-V1C.3 post-change consistency and compliance audit

**Source result: PASS. Unity 6000.5.0f1 compilation, URP shader import, visual validation, and dense-field GPU profiling remain pending.**

Actual modified files:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

Implemented differences:

- Replaced VEG-V1C.2's albedo-multiplied edge bonus with a separate additive direct-edge radiance term composed after body albedo lighting.
- Increased the base edge equation from a maximum coefficient of `0.75` to nonlinear `accent × lerp(1.5, 4.0, accent)`. The unchanged default `0.22` now yields `0.451` before masks, attenuation, response, and diffuse support; maximum strength yields `4.0`.
- Changed broad direct-fill retention from `0.65` at maximum accent to `0.75`; the stronger line, rather than excessive body darkening, now carries the graphic effect.
- Replaced raw lateral-dot edge strength with shaped light-facing selection. Sampled side weights are approximately `0.253` at lateral incidence `0.1`, `0.519` at `0.2`, `0.725` at `0.5`, and `1.0` at `1.0`, while the opposite edge resolves to zero for those incidences.
- Added `Edge Highlight Whiteness`, default `0.75`, which preserves the direct light's peak HDR magnitude while moving its chroma toward white.
- Added `Edge Distance Persistence`, default `0.65`, which uses `fwidth` to provide a bounded effective edge width up to `0.42` normalized units as blades become narrow on screen.
- Added both controls to C# validation, material-property IDs, the material-only lighting hash, runtime publication, shader properties/CBUFFER, and comprehensive report output.
- Updated custom Inspector guidance and version labels without changing the established rebuild/material-only decision path.

Preserved behavior and invariants:

- At `Stylized Edge Accent = 0`, broad direct fill is exactly `1.0`, edge gain is exactly `0`, and final colour reduces to accepted VEG-V1C body lighting.
- Ambient SH, Minimum Night Visibility, sun/local-light ownership, URP Forward/Forward+ loops, attenuation, time-of-day response, fog, wind, Ground integration, placement, geometry, 48-byte instance layout, indirect draw count, shadow-casting policy, and `receiveShadows = false` are unchanged.
- Existing clipped-card silhouette normalization and opaque-strip UV edges remain the source of the edge mask.
- New controls are excluded from `ComputeRebuildConfigurationHash()` and included in `ComputeLightingConfigurationHash()`, so their Inspector changes refresh only the runtime material.
- No new file, texture sample, geometry, shader pass, draw call, buffer, compute dispatch, runtime CPU update, scene/prefab/material asset, camera, URP asset, layer, tag, or debug view was introduced.

Checks completed:

- Exact six-file changed scope against VEG-V1C.2: passed.
- Complete final-file reread plus direct producer/consumer review (`VegetationClusterMeshBuilder.AddStrip`, `VegetationCommon.hlsl`, material refresh, shader fragment path): passed.
- C#, ShaderLab, and HLSL delimiter, string, character, and comment lexical checks: passed.
- C# property-ID, ShaderLab property, material CBUFFER, lighting hash, material refresh, and report parity for both new controls: passed.
- Direct-light helper signature/call parity across main, Forward+, and additional-light loops: passed.
- Zero-accent numerical equivalence and new gain calculations: passed.
- Search found no texture sampling, `ShadowCaster` pass, geometry/instance-layout change, or obsolete `EntityId` conversion regression.
- No `.git` directory is present in the supplied source, so Git status, branch, HEAD, and historical-diff checks remain unavailable and were not claimed.

Limitations and concrete next actions:

- Unity assemblies and the URP shader compiler are unavailable in this environment. Apply the patch and report the complete Console output if compilation/import fails.
- Visual validation must test close, gameplay-medium, and far distance under a strong local light at accent `0`, default `0.22`, and maximum `1.0`; tune Whiteness and Distance Persistence only after confirming the new base formula is visibly stronger.
- Dense-field GPU timing remains required because the derivative edge mask and per-light additive edge operations execute across high screen coverage.

## VEG-V1C.4 — Local-Light-Only Stylized Edge Accent

**Status: Source implementation and post-change source audit complete. VEG-V1C.3 visual result rejected. Unity compilation, URP shader import, and visual validation pending.**

### Objective

Retain the stronger additive edge-radiance model introduced by VEG-V1C.3, but restrict it to genuinely strong punctual local-light influence. The main sun and Forward+ additional directional lights must remain body-lighting-only and must never create stylized blade-edge lines. Local point/spot accents must become exactly zero below an explicit local-energy threshold and must select only the blade edge that clearly faces the light.

### Acceptance criteria

1. With all local point/spot lights disabled, `Stylized Edge Accent > 0` produces no stylized edge line at any sun direction or time of day.
2. The main sun and Forward+ additional directional lights affect ordinary body lighting only; accent strength does not globally reduce their broad fill.
3. A point/spot light produces an accent only where its attenuated local energy exceeds `Local Edge Activation Threshold`.
4. The opposite silhouette edge receives zero accent, and nearly perpendicular light/blade orientations receive zero accent rather than a symmetric residual.
5. Strong nearby punctual lights retain the VEG-V1C.3 additive HDR-capable, whitened edge radiance and medium-distance persistence.
6. `Stylized Edge Accent = 0` remains numerically equivalent to accepted VEG-V1C body lighting.
7. Lighting-control edits remain material-only and do not rebuild geometry, placement, or instance buffers.

### Approved files

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

No scene, prefab, material, time-of-day, Weather, Ground, geometry, instance-layout, camera, URP asset, layer, or tag changes.

### Reviewed evidence

| Evidence | Finding | Required correction |
| --- | --- | --- |
| User screenshots after VEG-V1C.3 | White lines appear across almost the entire field, including grass outside any strong local-light region. | Treat VEG-V1C.3 as rejected and remove global/directional edge eligibility. |
| `VegetationLighting.hlsl`, `VegetationEvaluateLighting` | `GetMainLight()` passes `stylizedEdgeAccent` into `VegetationEvaluateDirectLight`, and `mainResult.edge` initializes `result.edgeAccent`. | Main light must call a body-only path or pass edge eligibility `0`. |
| `VegetationLighting.hlsl`, Forward+ directional loop | `URP_FP_DIRECTIONAL_LIGHTS_COUNT` lights also call the same edge evaluator. | Forward+ additional directional lights must remain body-only. |
| `VegetationLighting.hlsl`, `VegetationLightFacingEdge` | `smoothstep(-0.05, 0.25, orientedSide)` plus `lerp(0.45, 1.0, abs(lateralDot))` deliberately preserves a residual at weak/perpendicular orientation. | Replace with strict side and orientation gates that return zero for opposite or ambiguous directions. |
| `VegetationLighting.hlsl`, `VegetationEvaluateDirectLight` | Edge radiance is attenuated but has no explicit minimum local-energy eligibility. | Add `Local Edge Activation Threshold` and smooth internal transition; below threshold result must be exactly zero. |
| `SH_StylizedVegetationBenchmark.shader` | Screen-space persistence is applied before light eligibility and therefore preserves every generated edge mask. | Keep persistence as silhouette stabilization only; multiply it by validated punctual-light eligibility in the lighting include. |
| VEG-V1C.3 vs VEG-V1C.2 diff | Separate additive whitened edge radiance and stronger nonlinear gain are the intended improvements; sun eligibility and residual side support caused the global failure. | Preserve strong additive formula only inside eligible punctual-light evaluation. |

### Invariants and non-goals

- Ambient SH never creates edge accents.
- Sun and all directional lights never create edge accents.
- Punctual local-light body illumination remains unchanged except for the existing coupled body-fill reduction when the local accent is enabled.
- No light-type inference from attenuation is used for main/Forward+ directional paths; eligibility is explicit at each call site.
- No shadow receiving, shadow casting, texture sampling, PBR, geometry, or normal-deformation work is added.
- `Edge Distance Persistence` cannot create an accent without local-light energy, strict edge-side selection, and master accent strength.

### File-by-file implementation sequence

1. **Canonical plan:** record this objective, evidence, scope, invariants, risks, and validation before code changes. **Status: complete.**
2. **`VegetationBenchmark.cs`:** add serialized `Local Edge Activation Threshold`, validation, shader property ID, lighting hash, material publication, and report output. **Status: complete.**
3. **`VegetationBenchmarkEditor.cs`:** update material-only help text/contract; preserve no-rebuild lighting edit path. **Status: complete.**
4. **`SH_StylizedVegetationBenchmark.shader`:** add matching property/CBUFFER member and pass threshold to the lighting include. Preserve existing edge mask/persistence math. **Status: complete.**
5. **`VegetationLighting.hlsl`:** add explicit edge eligibility, body-only sun/directional calls, strict side selection, and punctual local-energy gating. **Status: complete.**
6. **`Stylized_Vegetation_Architecture.md`:** supersede the invalid global V1C.3 edge contract with local-light-only ownership. **Status: complete.**
7. **Post-change audit:** compare the final six-file diff against this scope, re-read all callers/contracts, run lexical/property/signature checks, and record Unity validation as pending. **Status: complete.**

### Risks and controls

- **Threshold too high:** valid lamp accents may disappear. Control: expose one material-only threshold with default `0.35` Weather/URP light-energy units.
- **Threshold too low:** weak outer falloff may retain accents. Control: smooth transition begins at the threshold and reaches full activation above an internal threshold-relative softness.
- **Card orientation popping:** strict side gates may pop when lateral alignment crosses zero. Control: use smooth gates with a dead zone, not a hard sign step.
- **Forward+/Forward divergence:** directional and punctual loops may accidentally share eligibility. Control: explicit `allowEdgeAccent` argument at every call site and signature-parity audit.
- **Regression to global body darkening:** accent master could reduce sun fill indirectly. Control: broad-fill suppression is multiplied by explicit punctual edge eligibility.

### Required validation and compliance checks

- Exact six-file scope.
- C#, ShaderLab, and HLSL lexical/delimiter checks.
- C# property ID / shader property / CBUFFER / publication / hash / report parity.
- Main light and Forward+ directional calls use edge eligibility `0`; punctual light loop uses `1`.
- Strict edge function has no nonzero residual term.
- Local-energy activation returns zero below threshold.
- `Stylized Edge Accent = 0` algebra preserves VEG-V1C body lighting.
- No shader pass, texture sample, instance layout, geometry, draw, or rebuild-path changes.
- Unity C# compilation, URP shader import, and visual validation remain pending until tested in the user project.


### VEG-V1C.4 post-change consistency and compliance audit

**Source result: PASS. Unity 6000.5.0f1 compilation, URP shader import, and user visual validation remain pending.**

Actual modified files:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

Implemented differences from rejected VEG-V1C.3:

- Added explicit edge eligibility at every direct-light call site. The main sun and Forward+ additional directional loop pass `0`; the punctual additional-light loop passes `1`.
- Directional broad fill is no longer reduced by the accent master because body-fill suppression is multiplied by explicit edge eligibility.
- `result.edgeAccent` begins at zero and receives contributions only from the punctual additional-light loop.
- Added `Local Edge Activation Threshold`, default `0.35`, to the serialized lighting controls, validation, material property ID, lighting-only configuration hash, runtime publication, shader property/CBUFFER, and comprehensive report.
- Added a smooth punctual-light energy gate using light luminance, distance/spot attenuation, local-light response, and diffuse support. The gate is exactly zero below the configured threshold.
- Replaced VEG-V1C.3's permissive side selector with a strict orientation dead zone and positive-side gate. Opposite edges and nearly perpendicular light/blade orientations resolve to zero.
- Retained the stronger VEG-V1C.3 nonlinear gain, additive post-albedo edge radiance, whiteness, and derivative distance persistence only inside eligible punctual-light evaluation.
- Updated both vegetation documents and marked VEG-V1C.3 rejected/superseded.

Preserved behavior and invariants:

- Ambient SH, main-sun body response, additional-light body response, time-of-day integration, fog, wind, Ground coverage, placement, geometry, 48-byte instance layout, indirect rendering, shadow-casting policy, and real-time shadow receiving remain unchanged.
- `Stylized Edge Accent = 0` yields broad-fill scale `1` and edge gain `0` for every light, preserving accepted VEG-V1C body lighting.
- The existing silhouette edge mask and screen-space persistence calculation are unchanged; they cannot produce visible output without punctual-light eligibility and energy activation.
- `Local Edge Activation Threshold` is excluded from rebuild configuration and included in the lighting-only hash, so Inspector changes refresh only the runtime material.
- No new file, texture sample, shader pass, geometry, draw call, buffer, compute dispatch, runtime CPU update, scene/prefab/material asset, camera, URP asset, layer, tag, or debug view was introduced.

Checks completed:

- Exact six-file scope against VEG-V1C.3: passed.
- Complete final code-file reread plus direct geometry/normal contract review (`VegetationClusterMeshBuilder.AddStrip`, `VegetationCommon.hlsl`): passed.
- C#, ShaderLab, and HLSL delimiter, string, character, and comment lexical checks: passed.
- C# property-ID, serialized field, validation, lighting hash, material publication, shader property, CBUFFER, function argument, and report parity: passed.
- Main-light and Forward+ directional eligibility `0`, punctual loop eligibility `1`: passed.
- Old symmetric residual expressions are absent; strict side/orientation gates are present: passed.
- Numerical samples confirm opposite/perpendicular edges return zero and local energy below threshold returns zero: passed.
- Search found no texture sampling, `ShadowCaster` pass, geometry/instance-layout change, or obsolete `EntityId` conversion regression.
- No `.git` directory is present in the supplied source archive, so Git status, branch, HEAD, and historical commit checks remain unavailable and were not claimed.

Limitations and concrete next actions:

- Unity assemblies and the URP shader compiler are unavailable in this environment. Apply the patch and provide the complete Console output if C# compilation or shader import fails.
- User visual validation must first disable all local point/spot lights and confirm that no stylized white edge remains under the sun. Then enable one local light and verify that accents appear only in its strong inner influence and disappear before the outer falloff.
- User visual validation confirmed the local-light-only eligibility, but also confirmed that `Edge Distance Persistence` was ineffective and the valid local accent falloff was too broad. VEG-V1C.5 supersedes those two tuning details.


## VEG-V1C.5 — Concentrated Local Edge Falloff

**Status: superseded by VEG-V1C.6 for Unity 6.5 cluster-loop compatibility, bounded near-light attenuation, and restrained low/medium master response. Historical source implementation only; V1C.5 was not frozen.**

### Objective

Preserve the validated VEG-V1C.4 local-light-only edge ownership while correcting two user-validated tuning defects:

1. `Edge Distance Persistence` has no visible effect at practical authored widths because its derivative width is normally smaller than `Edge Accent Width`, so `max(authoredWidth, persistentWidth)` selects the authored width.
2. The valid punctual edge accent remains visible across too much of a small light's range because it reuses ordinary URP attenuation without additional stylized concentration.

### Accepted behavior

- Remove `Edge Distance Persistence` completely from serialized controls, C# publication, lighting hashes, shader properties/CBUFFER, diagnostics, and current architecture guidance.
- Add `Local Edge Falloff Power`, range `1..8`, default `3`.
- Apply the power curve only to punctual-light edge attenuation:

```text
edge attenuation = pow(saturate(URP punctual attenuation), Local Edge Falloff Power)
                   × max(1, URP punctual attenuation)
                   × Local Light Response
```

- Use the shaped edge attenuation for both local-edge activation energy and final additive edge radiance.
- Preserve ordinary punctual-light blade-body illumination on the unmodified URP attenuation path.
- Preserve near-light intensity: attenuation `1` remains `1`, and any URP near-source attenuation above `1` is retained instead of being flattened by the power curve.
- Preserve VEG-V1C.4 ownership: ambient and directional lights never produce edge accents.
- Preserve `Stylized Edge Accent = 0` as the exact accepted VEG-V1C body-lighting fallback.
- Use derivatives only to antialias the authored edge-width boundary; do not expand edge width with distance.

### Control roles

- `Local Edge Falloff Power`: controls how rapidly the valid edge accent weakens inside a punctual light's range.
- `Local Edge Activation Threshold`: controls the final minimum shaped local energy required before any accent is allowed.

These controls are complementary. Falloff Power shapes the curve; Activation Threshold removes the remaining weak tail.

### Scope

Modified files:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

No scene, prefab, material asset, time-of-day, Weather, Ground, geometry, instance layout, buffer, draw-call, camera, URP, layer, tag, or debug-view changes.

### Risks and validation

- A very high falloff power may make the edge region too small. Start at default `3`; compare values `1`, `3`, and `5` before changing the activation threshold.
- Because activation uses the same shaped energy, increasing falloff power narrows both the visible intensity and eligibility region. This is intentional.
- Multiple overlapping punctual lights still add independently, but each contribution follows its own shaped attenuation.
- Validate with one isolated local light and the light-range gizmo visible. The strongest inner area should remain near-white at strong accent settings, followed by a visibly faster continuous falloff and little or no outer-range edge response.

### VEG-V1C.5 post-change consistency and compliance audit

**Source result: PASS. Unity 6000.5.0f1 compilation, URP shader import, user visual validation, and dense-field GPU profiling remain pending.**

Implemented differences from VEG-V1C.4:

- Removed `_EdgeDistancePersistence`, `EdgeDistancePersistenceId`, `edgeDistancePersistence`, its validation, lighting-hash entry, runtime publication, shader property/CBUFFER member, report text, and current Inspector guidance.
- Added `_LocalEdgeFalloffPower`, `LocalEdgeFalloffPowerId`, serialized `localEdgeFalloffPower = 3`, range/clamp `1..8`, lighting-only hash entry, material publication, shader property/CBUFFER member, and report output.
- Replaced derivative-driven effective-width expansion with the authored `Edge Accent Width`; `fwidth` remains only as boundary antialiasing.
- Added separate `edgeDistanceAttenuation = pow(saturate(rawAttenuation), power) × max(1, rawAttenuation) × response` inside punctual edge evaluation. Values at or above `1` preserve the existing strong near-source energy; sub-`1` values receive the steeper falloff.
- Local edge activation energy and final edge radiance use the shaped attenuation; ordinary body contribution remains on `light.distanceAttenuation × response`.
- Updated custom Inspector guidance and diagnostic version labels to VEG-V1C.5.

Preserved invariants:

- Sun and Forward+ directional calls retain explicit edge eligibility `0`; the punctual light loop retains eligibility `1`.
- Strict light-facing edge selection and local activation threshold remain unchanged.
- The strong nonlinear gain, additive post-albedo edge radiance, and whiteness control remain unchanged.
- `Stylized Edge Accent = 0` continues to produce zero edge gain and unchanged body lighting.
- Lighting controls remain excluded from `ComputeRebuildConfigurationHash()` and included in `ComputeLightingConfigurationHash()`, so edits remain material-only.
- No texture sample, shadow pass, geometry, instance data, draw, compute, runtime CPU update, or ownership change was introduced.


## VEG-V1C.6 — Unity 6.5 Cluster Lighting and Controlled Local Edge Response

**Status: user-validated lighting baseline; superseded by VEG-V1C.7 only for screen-space edge stability and the expanded authored width ceiling. Target-hardware GPU profiling remains pending.**

### Objective

Correct the vegetation shader's Unity 6.5 Forward+/clustered-light-loop contract and reduce the user-observed near-light eye-searing edge response without adding a redundant second intensity control.

### User-authorized scope

Modify exactly:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

No new file, serialized control, scene, prefab, material asset, URP asset, time-of-day, Weather, Ground, geometry, instance layout, buffer, draw call, layer, tag, or debug-view change is authorized.

### Reviewed evidence

| Evidence | Finding | Status |
| --- | --- | --- |
| `Assets/AGENTS.md` | Requires complete read-only review, canonical plan as the first write, exact scope, final full-file reread, cross-subsystem shader audit, and explicit pending Unity validation. | Reviewed completely |
| Fresh `Assets-Code-Archive(5).zip` | Contains 314 traversal-safe entries and the complete V1C.5 six-file implementation. No `.git` directory exists, so branch, HEAD, status, history, and pre-existing working-tree changes are unavailable. | Reviewed; limitation recorded |
| User screenshot supplied with approval | At `Stylized Edge Accent = 0.33`, close punctual light produces an excessively bright near-white/HDR response. The user approved a moderate response reduction while retaining the current maximum at `1.0`. | User-observed evidence |
| `SH_StylizedVegetationBenchmark.shader` | Uses `#pragma multi_compile _ _FORWARD_PLUS`, while Unity 6000.5 documentation specifies `_CLUSTER_LIGHT_LOOP` for Forward+/clustered custom additional-light loops. | Defect confirmed by source and official documentation |
| `VegetationLighting.hlsl` | Uses `#if USE_FORWARD_PLUS`; Unity 6000.5 documentation specifies `#if USE_CLUSTER_LIGHT_LOOP`. Main/directional eligibility is already `0`; punctual eligibility is already `1`. | Defect confirmed; ownership invariant preserved |
| Unity 6000.5 Manual, “Render additional lights in a shader in URP” | Requires `_CLUSTER_LIGHT_LOOP`, `USE_CLUSTER_LIGHT_LOOP`, a Forward+ non-main-directional loop, and the regular `LIGHT_LOOP_BEGIN` loop for Forward compatibility. | Primary external evidence: https://docs.unity3d.com/6000.5/Documentation/Manual/urp/use-built-in-shader-methods-additional-lights-fplus.html |
| `Assets/Settings/PC_Renderer.asset` | Serialized `m_RenderingMode: 2`; the supplied handoff treats this renderer as the project Forward+ dependency. The patch does not modify this asset. | Read-only dependency reviewed |
| `VegetationLighting.hlsl > VegetationEvaluateDirectLight` | V1C.5 preserves attenuation values above `1`, applies powered attenuation to activation and final radiance, and maps accent `0.33` to approximately `0.767` gain. | Current formula reviewed |
| `VegetationBenchmark.cs > BuildComprehensiveReport` | Report heading remains `[Vegetation V1C.2 Benchmark Report]` despite V1C.5 Inspector labels. | Diagnostic defect confirmed |
| `VegetationClusterMeshBuilder.AddStrip`, `VegetationCommon.hlsl`, `VegetationWindResponse.hlsl`, `TimeOfDayController`, `GeneratedGround` sampling contracts, and `WeatherWindDomain` publication contracts | The patch does not require geometry, normal, wind, Ground, time-of-day, or instance-data changes. | Related contracts reviewed |
| Repository search for `VegetationLighting.hlsl` and its symbols | The include is consumed only by the vegetation benchmark shader. Unrelated River and PixelSurface shaders also contain legacy Forward+ symbols but are outside this approved patch. | Cross-subsystem audit complete |

### Acceptance criteria

1. Vegetation uses Unity 6.5 `_CLUSTER_LIGHT_LOOP` and `USE_CLUSTER_LIGHT_LOOP` symbols while retaining both the Forward+ non-main-directional loop and ordinary additional-light loop.
2. Main sun and every directional light remain body-only; only point/spot lights can produce the graphic edge accent.
3. Edge-specific distance attenuation is bounded to `0..1`; ordinary punctual body lighting retains unmodified URP attenuation.
4. `Local Edge Falloff Power` shapes final edge radiance once. `Local Edge Activation Threshold` evaluates unpowered normalized punctual energy so the two controls do not compound the power curve.
5. `Stylized Edge Accent` remains the only master strength control. Its response becomes gentler below `1`, while `0` remains exact body-only fallback and `1` retains gain `4`.
6. Coupled punctual body-fill restraint uses the same shaped accent response, reducing unintended body suppression at low/medium master values.
7. Comprehensive report and Inspector log identifiers use `VEG-V1C.6` consistently.
8. Lighting edits remain material-only and never enter `ComputeRebuildConfigurationHash()`.

### Formula changes

Approved edge attenuation:

```text
normalized edge attenuation = saturate(URP punctual attenuation)
shaped edge attenuation = pow(normalized edge attenuation, Local Edge Falloff Power)
                          × Local Light Response
```

Approved activation energy:

```text
activation energy = punctual-light luminance
                    × normalized edge attenuation
                    × Local Light Response
                    × edge diffuse support
```

Approved master response:

```text
accent response = accent × lerp(0.125, 1.0, accent)
edge gain = 4 × accent response
```

Reference gains:

| Accent | V1C.5 gain | V1C.6 target gain |
| ---: | ---: | ---: |
| 0.22 | 0.451 | 0.279 |
| 0.33 | 0.767 | 0.546 |
| 0.50 | 1.375 | 1.125 |
| 1.00 | 4.000 | 4.000 |

### Invariants and non-goals

- Do not restore sun/directional edge eligibility.
- Do not add another edge-intensity control.
- Do not alter edge width, whiteness, strict side selection, silhouettes, normals, fog, wind, placement, coverage, rendering submission, or shadow policy.
- Do not apply edge falloff shaping to ordinary punctual body illumination.
- Do not change unrelated River or PixelSurface Forward+ symbols in this patch.
- Do not claim Unity compilation, visual success, or performance success from source checks.

### File-by-file implementation sequence

| Item | File(s) | Required result | Status |
| --- | --- | --- | --- |
| V1C.6-0 | This document | Record evidence, approved scope, formulas, invariants, risks, sequence, and validation before implementation. | Complete |
| V1C.6-1 | `SH_StylizedVegetationBenchmark.shader` | Replace legacy Forward+ variant symbol with `_CLUSTER_LIGHT_LOOP`; populate the official `InputData.viewDirectionWS` field used by the clustered path. | Complete; Unity import pending |
| V1C.6-2 | `VegetationLighting.hlsl` | Replace `USE_FORWARD_PLUS`; separate normalized activation attenuation from powered final attenuation; bound the edge path; implement shaped master response and matching body-fill restraint. | Complete; visual validation pending |
| V1C.6-3 | `VegetationBenchmark.cs`, `VegetationBenchmarkEditor.cs` | Update tooltip wording and all current diagnostic/log identifiers to V1C.6 without adding serialized fields or changing rebuild classification. | Complete; Unity compilation pending |
| V1C.6-4 | `Stylized_Vegetation_Architecture.md` and this document | Record V1C.6 as the current contract and mark V1C.5 attenuation/gain details superseded. | Complete |
| V1C.6-5 | All approved files and related contracts | Run final scope diff, full-file reread, lexical/property/signature checks, formula samples, prohibited-symbol scans, and archive packaging. Unity compile/import and visual/performance validation remain pending. | Source audit complete; Unity validation pending |

### Risks and performance

- The active fragment cost remains one `pow` per processed punctual light. The formula changes add no texture sample, loop, branch, pass, draw call, geometry, buffer, or runtime CPU work.
- Replacing a broken/obsolete cluster keyword can restore additional punctual-light evaluation in Forward+, so measured GPU cost may increase relative to a variant that omitted those lights. This is required correctness, not a new visual feature.
- Light colour/intensity remains HDR-capable. Bounding only distance attenuation and reducing low/medium master response should reduce the screenshot's near-light spike, but Unity visual validation is required.
- Multiple overlapping punctual lights still add independently.

**PERFORMANCE EXCEPTION — user-approved correctness/quality cost:** the existing per-punctual-light custom edge path remains active because the user requires local-light-reactive graphic edges. V1C.6 does not add a new asymptotic cost; target-hardware profiling remains mandatory before freeze.

### Validation and compliance plan

- Verify exact six-file diff and no serialized-asset changes.
- Verify `_FORWARD_PLUS` and `USE_FORWARD_PLUS` are absent from the vegetation shader/include; `_CLUSTER_LIGHT_LOOP` and `USE_CLUSTER_LIGHT_LOOP` are present.
- Verify main and non-main directional call sites pass edge eligibility `0`; punctual loop passes `1`.
- Verify `rawDistanceAttenuation` cannot multiply edge radiance above normalized `1`, and activation uses normalized rather than powered attenuation.
- Verify gain calculations numerically at `0`, `0.22`, `0.33`, `0.5`, and `1`.
- Verify report/log version parity, shader/C# property parity, delimiter/string/comment integrity, and no new namespace/import requirement.
- Unity 6000.5 C# compilation, shader import, isolated-light screenshots, material-only behavior, and GPU profiling remain pending in the user project.


### VEG-V1C.6 post-change consistency and compliance audit

**Source result: PASS. Unity 6000.5 C# compilation, URP shader import, visual validation, and target-hardware profiling remain pending and are not claimed.**

Actual modified files exactly match the approved six-file scope:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

Implemented differences from V1C.5:

- Replaced the vegetation shader variant `_FORWARD_PLUS` with Unity 6.5 `_CLUSTER_LIGHT_LOOP` and replaced `USE_FORWARD_PLUS` with `USE_CLUSTER_LIGHT_LOOP` in the vegetation include.
- Added `InputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS)` alongside the existing position, normal, and normalized screen-space fields.
- Replaced unbounded edge-distance preservation with `normalizedEdgeAttenuation = saturate(max(0, light.distanceAttenuation))` and powered final attenuation bounded to `0..1` before `Local Light Response`.
- Separated unpowered activation attenuation from powered final attenuation. `Local Edge Activation Threshold` now evaluates normalized punctual energy; `Local Edge Falloff Power` shapes final edge radiance once.
- Replaced `accent × lerp(1.5, 4, accent)` with `4 × accent × lerp(0.125, 1, accent)`. Gain remains `0` at accent `0` and `4` at accent `1`; gain at `0.33` changes from `0.76725` to `0.54615`.
- Applied the same shaped master response to coupled punctual body-fill restraint. Body-fill scale is now approximately `0.966` at accent `0.33` instead of `0.918`, while accent `1` remains `0.75`.
- Updated the comprehensive report heading and all current Inspector action log identifiers to V1C.6.
- Updated Inspector tooltips and both canonical vegetation documents. No serialized field was added, removed, or renamed.

Preserved behavior and invariants:

- Main sun and Forward+ non-main directional calls retain explicit edge eligibility `0`; the ordinary punctual loop retains eligibility `1`.
- Ordinary punctual body lighting remains `light.distanceAttenuation × Local Light Response` and is not bounded or powered by the graphic edge controls.
- Ambient SH, time-of-day integration, edge width, whiteness, strict light-facing side selection, fog, wind, Ground coverage, placement, geometry, 48-byte instance layout, indirect rendering, shadow policy, and render controls are unchanged.
- `Stylized Edge Accent = 0` produces `accentResponse = 0`, body-fill scale `1`, and edge gain `0`, preserving the accepted V1C body path.
- All lighting controls remain excluded from `ComputeRebuildConfigurationHash()` and included in `ComputeLightingConfigurationHash()`; edits remain material-only.
- River and PixelSurface shaders still contain their pre-existing legacy Forward+ symbols. They were identified by the required cross-subsystem audit and deliberately left untouched because they are outside this vegetation-only authorization.

Checks completed:

- Safe archive extraction and no `.git` metadata: confirmed. Git branch, HEAD, status, history, and comparison against repository commits remain unavailable.
- Exact archive-to-final scope comparison: six approved files changed, zero files added or deleted.
- Complete pre-edit implementation review plus final exact diff and final changed-region reread: passed; no unexplained change exists outside the recorded plan.
- C#, HLSL, and ShaderLab delimiter/string/character/comment lexical checks: passed.
- Vegetation legacy-symbol scan: `_FORWARD_PLUS` and `USE_FORWARD_PLUS` absent; `_CLUSTER_LIGHT_LOOP` and `USE_CLUSTER_LIGHT_LOOP` present.
- Direct-light call audit: exactly three calls with edge eligibility order main `0`, cluster directional `0`, punctual `1`.
- C# property ID, serialized field, validation, lighting hash, material publication, shader property, CBUFFER, function argument, and report parity: passed for all existing edge controls.
- Numerical formula checks at accent `0`, `0.22`, `0.33`, `0.5`, and `1`, plus raw attenuation values through `10`: passed.
- Prohibited scope scan: no texture sample, shadow pass, geometry, instance layout, buffer, draw, compute dispatch, runtime CPU update, scene, prefab, material asset, URP asset, Ground, Weather, layer, tag, or debug-view change.

Concrete pending validation:

1. Import in Unity 6000.5.0f1 and provide the complete Console output if any C# or shader error appears.
2. With all point/spot lights disabled, set Accent and Whiteness to `1` and rotate the sun; no graphic white edge may appear.
3. With one isolated point light, compare Accent `0.33` and `1.0` at fixed falloff/threshold. `0.33` should be materially calmer; `1.0` should retain the former maximum character without the old distance-attenuation multiplier.
4. Compare Falloff Power `1`, `3`, and `5`; the final edge should narrow continuously while the body-light range remains unchanged.
5. Change edge controls and copy the comprehensive report. Instance count and deterministic hash must remain unchanged, and the report must begin `[Vegetation V1C.6 Benchmark Report]`.
6. Profile Accent `0` versus active edge at 1440p with one and worst representative local-light counts before freezing V1C.


## VEG-V1D — Continuous Calm Sway and Wind Detail

**Status: source implementation and post-change audit complete; user visual validation passed. Target-hardware profiling remains pending.**

### Objective

Remove the user-observed low-frame-rate appearance from the small longitudinal and lateral grass sway while preserving the existing calm back-and-forth motion, the current detail frequencies, the current approximate calm amplitudes, the 16 Hz Weather simulation, and the existing one-sample vegetation wind path.

### User-authorized scope

Modify exactly:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationWindResponse.hlsl`

No C#, compute shader, Weather generation, response integration, field cadence, field resolution, shader pass, geometry, instance layout, buffer, draw call, scene, prefab, material asset, Ground, lighting, layer, tag, or debug-view change is authorized.

### Reviewed evidence

| Evidence | Finding | Status |
| --- | --- | --- |
| `Assets/AGENTS.md` | Requires complete read-only review, the canonical plan as the first write, exact scope, full final reread, source validation, and explicit pending Unity validation. | Reviewed completely |
| Fresh `Assets-Code-Archive(5).zip` plus approved `VEG-V1C.6` overlay | Reconstructs the current user-tested source. The archive contains no `.git` directory, so branch, HEAD, status, history, and working-tree comparisons are unavailable. | Reviewed; limitation recorded |
| User-supplied approximately 10-second grass clip | Small grass motion visibly holds and advances in short steps, reading as low-frame-rate rocking before or during weak wind. | User-observed evidence |
| User validation after V1C.6 | The preceding clustered-light and local-edge patch works in the live project; this update is limited to wind-response detail motion. | User-observed evidence |
| `VegetationWindResponse.hlsl::ApplyVegetationWindResponse` | Detail phase uses `_WeatherWindFieldTiming.x` alone. Detail amplitude uses `bendMagnitude * 1.35 + length(weather.velocity) * 0.08`. | Defect source confirmed |
| `WeatherWindDomain.Update` | `simulationTime` advances only inside the fixed-step loop. Default `updateRateHz = 16`, so the detail phase advances only once per 0.0625 seconds. | Producer reviewed |
| `WeatherWindDomain.PublishShaderGlobals` | `_WeatherWindFieldTiming = (simulationTime, min(simulationAccumulator, fixedStep), fixedStep, maximumVisualBendMetres)`. The already-published accumulator can reconstruct continuous Weather time without a new C# property. | Producer contract reviewed |
| `WeatherWindField.hlsl::SampleWeatherWindResponse` | Macro bend already predicts continuously as `state.xy + state.zw * predictionTime`; the proposed change does not alter its texture sample or prediction. | Shared consumer contract reviewed |
| `SH_StylizedVegetationBenchmark.shader::Vert` | `ApplyVegetationWindResponse` is called once per rendered vegetation vertex. No other source consumes this include. | Direct caller reviewed |
| Base archive comparison | `VegetationWindResponse.hlsl` is unchanged by V1C.6; the stepped phase and velocity-amplitude coupling are pre-existing Weather-wind behavior rather than a lighting regression. | Historical comparison complete |

### Proven timing defect

Current detail phase:

```text
detailPhase = simulationTime × 2π × detailFrequency + spatial/instance phase
```

At the default 16 Hz Weather cadence:

```text
fixed step = 1 / 16 = 0.0625 seconds
```

The current detail-frequency range is `1.75..2.55 Hz`, so one fixed step advances the oscillator by:

```text
2π × 1.75 × 0.0625 = 0.687 rad = 39.4 degrees
2π × 2.55 × 0.0625 = 1.001 rad = 57.4 degrees
```

At a 60 Hz render rate, one Weather phase value is therefore held for approximately `60 / 16 = 3.75` rendered frames before the next 39.4–57.4 degree jump.

### Approved implementation formula

Continuous Weather-consumer time:

```text
continuousWindTime = max(0, simulationTime + simulationAccumulator)
```

The sum is continuous across a normal fixed update:

```text
before step: S + (h + r)
after step:  (S + h) + r
```

Both equal `S + h + r`. The detail oscillator therefore advances each rendered update without changing the fixed Weather simulation.

Calm-preserving detail energy:

```text
CalmDetailEnergy = 0.078
windDrivenDetailEnergy = saturate(bendMagnitude × 1.35)
detailEnergy = max(CalmDetailEnergy, windDrivenDetailEnergy)
```

The fixed-step `length(weather.velocity) × 0.08` term is removed only from micro-detail amplitude. Macro response prediction continues to use response velocity in `WeatherWindField.hlsl`.

At the calm floor:

```text
longitudinal amplitude = 0.078 × 0.035 = 0.00273 m = 2.73 mm
lateral amplitude = 0.078 × 0.018 = 0.001404 m = 1.404 mm
```

The existing `1.75..2.55 Hz` frequency range, per-instance phase, spatial phase, longitudinal multiplier `0.035`, and lateral multiplier `0.018` remain unchanged.

### Acceptance criteria

1. Calm grass retains continuous back-and-forth and lateral sway at approximately the current amplitude and frequency.
2. Detail phase uses `_WeatherWindFieldTiming.x + _WeatherWindFieldTiming.y`, not stepped simulation time alone.
3. Micro-detail amplitude no longer depends on fixed-step response velocity.
4. Macro bend, macro prediction, response field, response velocity storage, spring parameters, Weather target generation, and 16 Hz default cadence remain unchanged.
5. The shader still performs exactly one Weather response texture sample and two sine evaluations per vegetation vertex.
6. No serialized control or runtime resource is added.
7. The final diff contains exactly the three approved files.

### Invariants and non-goals

- Do not suppress calm sway.
- Do not change detail frequency, longitudinal amplitude coefficient, lateral amplitude coefficient, per-instance phase, or spatial phase.
- Do not increase Weather update cadence.
- Do not add current/previous response interpolation or a second response-texture sample.
- Do not use Unity `_Time`; detail remains synchronized to the Weather domain's time/reset lifecycle.
- Do not alter the macro response equation or remove response velocity from macro prediction.
- Do not add a calm/gust control in this patch.
- Do not claim Unity shader compilation, visual success, or measured GPU performance from source checks.

### File-by-file implementation sequence

| Item | File(s) | Required result | Status |
| --- | --- | --- | --- |
| V1D-0 | This document | Record reviewed evidence, approved scope, formulas, invariants, risks, sequence, and validation before implementation. | Complete |
| V1D-1 | `Weather_Wind_Architecture.md` | Record the continuous consumer-time contract, calm-detail preservation, and unchanged Weather simulation/resource contract. | Complete |
| V1D-2 | `VegetationWindResponse.hlsl` | Replace stepped phase time with continuous Weather time; replace velocity-coupled detail energy with the approved calm floor and bend-driven energy. | Complete; Unity shader import and visual validation pending |
| V1D-3 | All approved files and direct contracts | Run final exact-scope diff, complete final-file reread, HLSL lexical checks, symbol/formula checks, numerical checks, and package the changed files. | Source audit complete; Unity validation pending |

### Risks and performance

- A constant calm floor means an active Weather domain retains subtle detail sway even when local macro bend is nearly zero. This is intentional and explicitly approved because calm back-and-forth sway must remain.
- Removing the velocity contribution can reduce short transition-driven micro-detail amplitude. Strong gust detail still reaches the existing maximum through `saturate(bendMagnitude × 1.35)`.
- Under a severe frame hitch where the Weather accumulator is deliberately clamped after the maximum fixed-step count, continuous consumer time may also discard backlog. This matches the existing bounded catch-up policy and avoids an unbounded visual jump.
- Active GPU asymptotic cost is unchanged: one response texture sample and two sine evaluations per vegetation vertex.
- Arithmetic changes remove one `length(weather.velocity)` operation and one velocity multiply/add, and add one timing addition plus one scalar `max`. Expected measured cost is neutral to marginally lower; exact compiler output and target-GPU timing remain unverified.
- CPU work, compute dispatch count, memory, bandwidth, buffers, draw calls, and serialized data remain unchanged.

### Validation and compliance plan

- Verify exact three-file diff and no added/deleted files.
- Verify the detail phase contains `_WeatherWindFieldTiming.x + _WeatherWindFieldTiming.y` and no longer uses `.x` alone.
- Verify `length(weather.velocity)` and `velocityMagnitude` are absent from `VegetationWindResponse.hlsl`, while response velocity remains used in `WeatherWindField.hlsl` macro prediction.
- Verify one response sample and exactly two detail `sin` calls remain.
- Verify calm-floor and displacement calculations numerically.
- Run available HLSL lexical/delimiter checks and scan all references to the modified symbols.
- Unity 6000.5 shader import, live calm/gust comparison, and target-hardware profiling remain pending in the user project.


### VEG-V1D post-change consistency and compliance audit

**Source result: PASS. Unity 6000.5 shader import, live motion validation, and target-hardware profiling remain pending and are not claimed.**

Actual modified files exactly match the approved three-file scope:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationWindResponse.hlsl`

Implemented differences from the V1C.6 source baseline:

- Added `PS3D_VEGETATION_CALM_DETAIL_ENERGY = 0.078` as the fixed calm micro-detail floor.
- Replaced `detailEnergy = saturate(bendMagnitude × 1.35 + length(weather.velocity) × 0.08)` with `max(0.078, saturate(bendMagnitude × 1.35))`.
- Removed `velocityMagnitude` and its fixed-step influence from the small longitudinal/lateral detail amplitude.
- Added `continuousWindTime = max(0, _WeatherWindFieldTiming.x + _WeatherWindFieldTiming.y)`.
- Changed only the detail oscillator phase to use `continuousWindTime` instead of completed fixed-step simulation time alone.

Preserved behavior and contracts:

- `WeatherWindField.hlsl` still predicts macro bend as `state.xy + state.zw × predictionTime`; response velocity remains part of macro motion.
- `WeatherWindDomain.cs`, the compute shader, response texture formats, 16 Hz default cadence, field generation, spring integration, CPU gameplay sampling, and timing publication are byte-for-byte unchanged.
- `SH_StylizedVegetationBenchmark.shader` remains byte-for-byte unchanged and still calls `ApplyVegetationWindResponse` once per rendered vegetation vertex.
- One response-field texture sample and exactly two sine evaluations remain in the vegetation wind include.
- Detail frequency `1.75..2.55 Hz`, longitudinal coefficient `0.035`, lateral coefficient `0.018`, per-instance phase, spatial phase, root-to-tip weighting, stiffness response, and vertical compensation remain unchanged.
- No C#, serialized field, texture, buffer, draw call, compute dispatch, shader pass, geometry, instance data, scene, prefab, material asset, Ground, lighting, layer, tag, or debug-view change was introduced.

Source checks completed:

- Safe reconstruction from the supplied archive plus V1C.6 patch and absence of `.git` metadata: confirmed. Git branch, HEAD, status, history, and working-tree comparison remain unavailable.
- Exact baseline-to-final scope comparison: three approved files changed; zero files added or deleted.
- Complete final reread of all three modified files and reread/hash comparison of the direct caller, timing producer, and shared response sampler: passed.
- HLSL comments, strings, parentheses, braces, brackets, include guard, control characters, and modified-symbol checks: passed.
- Formula audit confirmed continuous `.x + .y` timing, removal of the stepped `.x`-only phase, removal of micro-detail velocity magnitude, one response sample, and two sine calls.
- Numerical checks confirmed old 16 Hz phase jumps of `39.375..57.375 degrees`, render-rate phase increments of `10.5..15.3 degrees` at 60 FPS, raw calm amplitudes of `2.73 mm` longitudinal and `1.404 mm` lateral before stiffness/root weighting, and fixed-step continuity of `simulationTime + accumulator`.
- Thirty-six automated source/formula/scope checks passed.
- A standalone Clang HLSL syntax attempt could not run because the installed compiler lacks its required `hlsl.h` default header. Unity's shader compiler is unavailable in this environment; shader compilation remains pending rather than passed.

Performance result by source inspection:

- Asymptotic vertex work is unchanged: one field sample and two sine evaluations.
- Removed work: one `float2 length`, one velocity scaling multiply, and one addition in detail-energy calculation.
- Added work: one scalar timing addition and one scalar `max` for the calm floor.
- CPU work, Weather compute work, memory, bandwidth, resource count, and draw count are unchanged.
- Expected measured GPU difference is neutral to marginally lower; this remains an inference until target-hardware profiling.

Concrete pending validation:

1. Import in Unity 6000.5.0f1 and provide the complete Console error if the vegetation shader fails to compile.
2. Record the same calm-to-gust interval. Calm sway must remain, but its small forward/backward and lateral phase must advance continuously instead of holding and jumping.
3. Confirm strong gust amplitude and macro spring movement remain visually unchanged apart from removal of the detail stutter.
4. Profile the same dense field before/after only if the visual result is accepted; no meaningful regression is expected from the source operation count.


**Subsequent VEG-V1D user result:** the continuous calm-sway patch compiled/rendered in Unity and the user confirmed that it works. The target-hardware performance measurement remains pending.


## VEG-V1C.7 — Screen-Space Edge Accent Stability and Expanded Width Range

**Status: user-rejected after Unity/game-camera validation; superseded by VEG-V1C.8. V1C.7 removed or weakened coherent 1.2–3 px accents instead of isolating only unstable one-pixel accents.**

### Objective

Remove game-camera pixel noise and temporal sparkle from stylized punctual-light blade-edge accents when the authored accent band projects to approximately one pixel or less. Preserve coherent close and medium-distance accents, preserve ordinary local-light body illumination, and expand the authored `Edge Accent Width` control ceiling from `0.35` to `0.50` so the user can test approximately `0.40` in combination with stability rejection.

### User-authorized scope

Modify exactly:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

No `VegetationLighting.hlsl`, Weather, wind, geometry-builder, instance-layout, scene, prefab, material asset, camera, URP asset, Ground, layer, tag, draw-call, buffer, texture, or debug-view change is authorized.

### Reviewed evidence

| Evidence | Finding | Status |
| --- | --- | --- |
| `Assets/AGENTS.md` | Requires complete read-only review, canonical plan as the first project write, exact scope, final complete-file reread, cross-module audit, and explicit pending Unity validation. | Reviewed completely |
| Current reconstructed project (`Assets-Code-Archive(5).zip` + accepted VEG-V1C.6 + VEG-V1D patches) | No `.git` directory exists. Branch, HEAD, status, history, and comparison with repository commits are unavailable. The reconstructed current tree is the authoritative local baseline for this patch. | Reviewed; limitation recorded |
| User-supplied game-camera screenshots `ff995378-136d-410e-affb-dd5d0b714035.png`, `b676b613-e227-47f0-a673-8d5ae56942d4.png`, and `40a87277-e44a-414a-a387-21e5b4b183f4.png` | Outer and angle-dependent accent fragments collapse into isolated white pixels and short broken streaks. The user explicitly prefers removing the accent over retaining pixel noise. | User-observed evidence |
| `SH_StylizedVegetationBenchmark.shader > Frag` | Current edge width is clamped to `0.01..0.35`. Current antialiasing uses `clamp(fwidth(edgeDistance) * 0.35, 0.0005, 0.10)` and clamps smoothstep bounds to `0..1`; distant/high-derivative features are therefore underfiltered and never explicitly rejected when subpixel. | Defect confirmed by source inspection |
| `SH_StylizedVegetationBenchmark.shader > Frag` | `edgeDistance` is normalized from `0` at the visible silhouette to `1` at the blade centre. For clipped cards, `signedSilhouettePosition` is divided by the actual clipped half-width first. Therefore `edgeWidth / fwidth(edgeDistance)` is a direct per-fragment estimate of projected accent-band width in pixels. | Contract confirmed by source inspection |
| `VegetationClusterMeshBuilder.AddStrip` | UV.x is `0` and `1` on the two strip edges for all three geometry candidates. This preserves the normalized edge-distance contract used by the shader. | Direct producer reviewed; no edit required |
| `VegetationLighting.hlsl` | The lighting include consumes only the scalar `edgeMask`. Directional eligibility remains `0`, punctual eligibility remains `1`, and ordinary local-light body illumination is independent of the proposed screen-space stability gate. | Direct consumer reviewed; no edit required |
| `VegetationBenchmark.cs` | `Edge Accent Width` is serialized, clamped, hashed only by `ComputeLightingConfigurationHash`, and published only through `RefreshLightingMaterialProperties`. The new pixel-stability control must follow the same material-only path and remain excluded from the rebuild hash. | Current control lifecycle reviewed |
| `VegetationBenchmarkEditor.cs` | `DrawDefaultInspector` plus rebuild/lighting hash comparison already supports material-only controls. Current labels are `V1C.6` and must advance to `V1C.7`. | Direct editor caller reviewed |
| `VegetationWindResponse.hlsl` and VEG-V1D docs | Wind motion is independent of the edge-mask computation and must remain byte-for-byte unchanged. | Cross-feature impact reviewed |

### Approved formula

Current normalized inputs:

```text
edgeDistance = 0 at the visible blade edge
edgeDistance = 1 at the blade centre
edgeDerivative = max(fwidth(edgeDistance), 0.0005)
edgeWidth = clamp(Edge Accent Width, 0.01, 0.50)
```

Projected accent width estimate:

```text
projectedEdgePixels = edgeWidth / edgeDerivative
```

Derivative antialiasing:

```text
edgeAntialias = max(edgeDerivative × 0.50, 0.0005)
edgeMask = 1 - smoothstep(
    edgeWidth - edgeAntialias,
    edgeWidth + edgeAntialias,
    edgeDistance)
```

Do not clamp the two smoothstep boundaries to `0..1`. A negative lower boundary is valid and allows a feature narrower than one pixel to resolve to partial coverage before stability rejection.

Screen-space stability gate:

```text
minimumStablePixels = clamp(Minimum Stable Accent Pixels, 0.5, 2.0)
pixelStability = smoothstep(
    minimumStablePixels,
    minimumStablePixels + 0.5,
    projectedEdgePixels)
pixelStability = pixelStability²
stableEdgeMask = edgeMask × pixelStability
```

New serialized/material control:

```text
Minimum Stable Accent Pixels
range: 0.5..2.0
initial default: 1.0
```

At the default:

| Projected width | Stability multiplier |
| ---: | ---: |
| `≤ 1.0 px` | `0` |
| `1.10 px` | approximately `0.011` |
| `1.25 px` | `0.25` |
| `1.40 px` | approximately `0.803` |
| `≥ 1.50 px` | `1` |

The gate suppresses only the graphic edge term. It does not suppress the blade geometry, authored albedo, ambient response, sun response, or punctual body-light response.

### Acceptance criteria

1. `Edge Accent Width` accepts `0.01..0.50`; its existing serialized value is not changed automatically.
2. `Minimum Stable Accent Pixels` exists with range `0.5..2.0`, default `1.0`, validation clamp, material-only hash entry, runtime material publication, shader property/CBUFFER parity, report output, and no rebuild-hash entry.
3. The edge boundary uses uncapped `fwidth × 0.5` antialiasing and unclamped smoothstep boundaries.
4. Accent bands projecting to `1.0 px` or less are fully rejected at the default; bands reach full strength at `1.5 px` or more.
5. Close and sufficiently wide accents retain the existing V1C.6 lighting intensity, whiteness, falloff, activation, and light-facing-side behavior.
6. Ambient and directional lights remain ineligible; punctual body lighting remains unchanged.
7. VEG-V1D wind source and all geometry/instance contracts remain unchanged.
8. The final diff contains exactly the five approved files.

### Invariants and non-goals

- Do not restore distance-based edge-width persistence or widen subpixel lines to keep them visible.
- Do not introduce camera-distance thresholds, LOD infrastructure, temporal dithering, post-processing, TAA dependency, or extra geometry.
- Do not alter `VegetationLighting.hlsl`, the local-light response formula, edge gain, whiteness, falloff power, activation threshold, or directional/punctual ownership.
- Do not change the current `Edge Accent Width` serialized value; only raise its legal ceiling.
- Do not add a texture sample, additional-light loop, draw call, shader pass, buffer, or runtime allocation.
- Do not include unrelated Forward+/cluster-symbol corrections found in Ground or River shaders.
- Do not claim Unity shader compilation, visual success, or measured performance from source checks.

### File-by-file implementation sequence

| Item | File(s) | Required result | Status |
| --- | --- | --- | --- |
| V1C.7-0 | This document | Record objective, evidence, exact scope, formula, acceptance, invariants, sequence, risks, and validation before any implementation edit. | Complete |
| V1C.7-1 | `VegetationBenchmark.cs` | Add/publish/hash/report the material-only stable-pixel control, raise width range/clamp to `0.50`, and advance comprehensive-report version. | Complete; Unity Inspector/material validation pending |
| V1C.7-2 | `VegetationBenchmarkEditor.cs` | Advance labels/help text to V1C.7 and state that width/stability changes remain material-only. | Complete; Unity Inspector validation pending |
| V1C.7-3 | `SH_StylizedVegetationBenchmark.shader` | Add property/CBUFFER control, raise width ceiling, replace capped derivative filtering, estimate projected pixel width, and multiply the edge mask by the approved stability gate. | Complete; Unity shader import and visual validation pending |
| V1C.7-4 | `Stylized_Vegetation_Architecture.md` | Make V1C.7 the current accent contract and document deliberate subpixel suppression plus expanded authored width range. | Complete |
| V1C.7-5 | All approved files and direct contracts | Perform exact-scope diff, complete final-file reread, property/hash/publication parity checks, formula/numerical checks, lexical checks, direct-caller/consumer/producer audit, and package changed files. | Source audit complete; Unity validation pending |

### Risks and performance

- A high `Minimum Stable Accent Pixels` value can remove accents from many game-camera blades. This is intentional and user-tunable; default `1.0` follows the user's preference to remove unstable pixels.
- Raising `Edge Accent Width` toward `0.50` can make close accents broad. The screen-space gate does not counteract an intentionally broad nearby line; the user will test approximately `0.40` visually.
- `fwidth` is already present. Incremental fragment work is one division/reciprocal, one `smoothstep`, and two scalar multiplies. No texture, light-loop, geometry, memory, CPU, compute, or draw cost is added.
- Because grass can cover a large part of the screen, the fragment arithmetic is not treated as free. Target-hardware profiling remains required after visual acceptance.

### Validation and compliance plan

- Verify exact five-file diff and zero added/deleted project files.
- Verify C# field, property ID, clamp, lighting hash, material publication, report, shader property, and CBUFFER names match exactly.
- Verify the new control and expanded width are absent from `ComputeRebuildConfigurationHash()`.
- Verify current directional/punctual eligibility sequence and `VegetationLighting.hlsl` hash remain unchanged.
- Verify the shader contains the approved antialiasing and stability formulas and no old `0.35`, `0.10` antialias cap, or clamped smoothstep boundaries in the edge-mask path.
- Run C# parser/compiler checks available in the environment, HLSL/ShaderLab lexical and delimiter checks, numerical gate checks, and final full-file reread.
- Unity 6000.5 shader import, game-camera noise comparison, close-camera width test at approximately `0.40`, and target-hardware profiling remain pending in the user project.


### VEG-V1C.7 post-change consistency and compliance audit

**Source result: PASS. Unity 6000.5 C#/shader compilation, user game-camera visual validation, and target-hardware GPU profiling remain pending and are not claimed.**

Actual modified files exactly match the approved five-file scope:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

Implemented differences from the accepted VEG-V1C.6 lighting source baseline:

- Raised the serialized, validation, shader-property, and shader-clamp ceiling for `Edge Accent Width` from `0.35` to `0.50`. The existing serialized/default value remains `0.10`; live values such as the user's `0.35` are not changed automatically.
- Added material-only `Minimum Stable Accent Pixels`, range `0.5..2.0`, default `1.0`, with C# property ID, serialized field, validation clamp, lighting hash, material publication, comprehensive report output, ShaderLab property, and Unity material CBUFFER member.
- Replaced capped `fwidth × 0.35` filtering with uncapped `max(fwidth × 0.50, 0.0005)` filtering and removed `0..1` clamps from the smoothstep boundaries.
- Added `projectedEdgePixels = edgeWidth / edgeDerivative` and the approved squared stability transition from the configured minimum to `minimum + 0.5` pixels.
- Multiplied only `edgeMask` by the stability value. Ordinary blade albedo and ambient, sun, and punctual body-light contributions remain outside the screen-space gate.
- Advanced comprehensive-report and Inspector diagnostic labels to VEG-V1C.7.
- Updated the exploratory architecture so VEG-V1C.7 is the current local-edge contract.

Preserved behavior and contracts:

- `VegetationLighting.hlsl` is byte-for-byte unchanged. Main and additional directional lights retain eligibility `0`; the punctual loop retains eligibility `1`.
- VEG-V1C.6 local-light intensity, bounded attenuation, activation threshold, whiteness, nonlinear master response, body-fill restraint, and light-facing-side selection are unchanged.
- `VegetationWindResponse.hlsl`, including the user-validated VEG-V1D continuous calm-sway behavior, is byte-for-byte unchanged.
- `VegetationClusterMeshBuilder.cs`, `VegetationInstanceData.cs`, and `VegetationCommon.hlsl` are byte-for-byte unchanged. UV.x remains `0/1` on visible strip edges.
- The new control is absent from `ComputeRebuildConfigurationHash()` and present only in `ComputeLightingConfigurationHash()`, so edits refresh the runtime material without rebuilding placement, geometry, or buffers.
- No scene, prefab, material asset, Weather, Ground, camera, URP asset, layer, tag, texture, buffer, draw call, shader pass, geometry, instance-layout, or compute change was introduced.

Source checks completed:

- Exact baseline-to-final scope comparison: five approved files changed; zero project files added or deleted.
- Complete reread and C-like lexical checks for the final C# runtime file, C# editor file, and ShaderLab/HLSL file: strings/comments terminated; parentheses, braces, brackets, preprocessor conditionals, and control characters passed.
- Markdown fence/control-character checks passed for both modified architecture documents.
- C#/shader parity passed for `_MinimumStableAccentPixels`, width range, property ID, field/default, validation clamp, lighting hash, material publication, report, ShaderLab property, and CBUFFER member.
- Rebuild-hash exclusion and lighting-hash inclusion passed.
- Formula audit passed for the derivative floor, uncapped antialiasing, unclamped boundaries, projected-width estimate, stable-pixel clamp, squared transition, and edge-mask multiplication.
- Numerical checks passed: default stability is `0` at `≤1.0 px`, approximately `0.010816` at `1.1 px`, `0.25` at `1.25 px`, approximately `0.802816` at `1.4 px`, and `1` at `≥1.5 px`.
- Direct unchanged-contract hashes passed for lighting, wind, cluster geometry, instance layout, and common vertex helpers.
- Seventy-six automated source/scope/formula checks passed.
- No Unity C# compiler, Unity shader importer, Roslyn compiler, or standalone HLSL environment with Unity/URP includes is available here. Compilation remains pending rather than passed.

Performance result by source inspection:

- The existing one `fwidth` remains; no derivative instruction is added.
- Incremental fragment work is one projected-width division/reciprocal, one additional `smoothstep`, and two scalar multiplies.
- No texture sample, light iteration, sine operation, vertex work, CPU update, compute dispatch, memory allocation, buffer, draw, or shader pass is added.
- Because dense grass can cover many pixels, the added fragment arithmetic is not treated as free. Exact target-GPU cost remains unverified until profiling.

Concrete pending validation:

1. Import in Unity 6000.5.0f1 and provide the complete Console error if C# or the vegetation shader fails to compile.
2. From the gameplay camera, compare `Minimum Stable Accent Pixels = 0.5`, `1.0`, and `1.5`; isolated one-pixel sparkle should be removed progressively while ordinary local body lighting remains.
3. Set `Edge Accent Width = 0.40` and verify that coherent nearby lines become slightly broader without restoring noisy distant single-pixel accents.
4. Confirm that disabling all point/spot lights still produces no stylized edge accent under sun/directional lighting.
5. Copy the comprehensive report and confirm the heading is `[Vegetation V1C.7 Benchmark Report]` and the new stable-pixel value is present.
6. Profile the accepted settings at 1440p in the dense gameplay case; compare GPU frame time against V1C.6/V1D before production freeze.


## VEG-V1C.8 — Angle-Correct Stable Accent Rejection

**Status: user-rejected after Unity/game-camera validation; superseded by VEG-V1C.9. V1C.8 still preserved many noisy fragments because it differentiated a saturated post-clip edge-distance field and tested authored width before per-light side narrowing.**

### Objective

Correct the user-rejected V1C.7 screen-space stability implementation. Restore the accepted V1C.6 edge-mask shape, preserve the expanded `Edge Accent Width` ceiling of `0.50`, and reject only accent bands whose true perpendicular screen-space width is approximately one pixel or less. Do not weaken coherent 1.2–3 px accents, and do not alter ordinary local-light body illumination or any V1C.6/V1D behavior.

### User-authorized scope

Modify exactly:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

No `VegetationLighting.hlsl`, Weather, wind, geometry-builder, instance-layout, scene, prefab, material asset, camera, URP asset, Ground, layer, tag, draw-call, buffer, texture, or debug-view change is authorized.

### Read-only review evidence

| Evidence | Finding | Status |
| --- | --- | --- |
| `Assets/AGENTS.md` | Requires a complete read-only review, canonical plan as the first write, exact approved scope, post-change full-file reread, direct producer/consumer audit, and explicit pending Unity validation. | Reviewed completely before this section was written |
| Current reconstructed tree at `/mnt/data/veg_v1c7_work/current` | Contains the user-tested V1C.7 source plus the accepted V1C.6 and V1D changes. No `.git` directory exists; branch, HEAD, status, history, and repository diff are unavailable. | Reviewed; limitation recorded |
| User screenshots `36e32eb8-a36b-4c6a-b540-0b5a5ad78623.png`, `1e99ee46-b78c-461b-9ccb-0e22da15e72b.png`, `d90d6cae-f580-40e4-99aa-65bb3e53d2ff.png`, and before-reference `6a71c07c-58f9-487d-957a-8575bbf45fce.png` | Close Scene-view accents remain coherent, but the game-camera V1C.7 result loses many previously continuous strokes and leaves shorter/dotted remnants. The user reports that 2–3 px accents appear reduced toward one-pixel remnants. | User-observed rejection evidence |
| `SH_StylizedVegetationBenchmark.shader > Frag` V1C.7 | Uses `edgeWidth / fwidth(edgeDistance)`, a `1.0..1.5 px` transition, squares the stability value, and replaces the accepted V1C.6 filter with uncapped `fwidth × 0.50`. | Defect confirmed by source inspection |
| V1C.7 stability curve | At default `1.0`, an estimated `1.10 px` line receives approximately `0.0108`, `1.20 px` receives approximately `0.124`, `1.25 px` receives `0.25`, and only `1.50 px` reaches full strength. This suppresses many valid partially covered strokes rather than only one-pixel noise. | Mathematical defect confirmed |
| HLSL derivative definition used by current code | `fwidth(x) = abs(ddx(x)) + abs(ddy(x))`. This L1 gradient can be `sqrt(2)` larger than the true perpendicular L2 gradient at diagonal orientations, underestimating projected width by up to `1/sqrt(2) ≈ 0.707`. | Angle bias confirmed mathematically |
| Accepted pre-V1C.7 shader in `/mnt/data/veg_v1c7_work/pre_v1c7` | Uses `clamp(fwidth(edgeDistance) × 0.35, 0.0005, 0.10)` and clamped smoothstep boundaries. The before screenshot corresponds to this coherent edge-mask shape. | Historical accepted comparison reviewed |
| `VegetationClusterMeshBuilder.AddStrip` | UV.x remains `0` and `1` at strip edges for all candidates; the normalized edge-distance contract is unchanged. | Direct producer reviewed; no edit required |
| `VegetationLighting.hlsl` | Consumes only `edgeMask`; directional eligibility remains `0`, punctual eligibility remains `1`, and local body illumination is independent of the stability mask. | Direct consumer reviewed completely; no edit required |
| `VegetationBenchmark.cs` | `Minimum Stable Accent Pixels` is serialized, clamped, lighting-hashed, material-published, and reported; it remains excluded from the rebuild hash. Only wording/version changes are required. | Complete runtime file reviewed |
| `VegetationBenchmarkEditor.cs` | `DrawDefaultInspector` and lighting-hash routing already make the control material-only. Help/report labels require V1C.8 wording. | Complete editor file reviewed |
| `VegetationWindResponse.hlsl` and V1D contract | Independent of fragment edge masking and must remain unchanged. | Cross-feature impact reviewed |

### Defect derivation

V1C.7 estimates projected width as:

```text
estimatedPixels = edgeWidth / (abs(ddx(edgeDistance)) + abs(ddy(edgeDistance)))
```

For a screen-space gradient with equal X and Y components:

```text
L1 gradient = |g| + |g| = 2|g|
L2 gradient = sqrt(g² + g²) = sqrt(2)|g|
L1 / L2 = sqrt(2)
```

Therefore:

```text
estimatedPixelsV1C7 = truePixels / sqrt(2)
```

A true `1.60 px` diagonal band may be classified as:

```text
1.60 / sqrt(2) = 1.13 px
```

The V1C.7 squared gate at `1.13 px` retains only a few percent of the edge energy. This explains coherent diagonal strokes disappearing or leaving only their strongest raster sample.

### Approved V1C.8 formula

Restore the accepted V1C.6 antialias mask while retaining the `0.50` width ceiling:

```hlsl
float edgeDerivativeX = ddx(edgeDistance);
float edgeDerivativeY = ddy(edgeDistance);
float edgeDerivativeAA = max(
    abs(edgeDerivativeX) + abs(edgeDerivativeY),
    0.0005);
float edgeWidth = clamp(_EdgeAccentWidth, 0.01, 0.50);
float edgeAntialias = clamp(
    edgeDerivativeAA * 0.35,
    0.0005,
    0.10);
float edgeMask = 1.0 - smoothstep(
    max(0.0, edgeWidth - edgeAntialias),
    min(1.0, edgeWidth + edgeAntialias),
    edgeDistance);
```

Measure true perpendicular screen-space width with the isotropic L2 gradient, but compare in squared space to avoid `sqrt` and division:

```hlsl
float edgeGradientSquared = max(
    edgeDerivativeX * edgeDerivativeX +
    edgeDerivativeY * edgeDerivativeY,
    0.00000025);

float minimumStablePixels = clamp(
    _MinimumStableAccentPixels,
    0.5,
    2.0);
float fullyStablePixels = minimumStablePixels + 0.20;

float edgeWidthSquared = edgeWidth * edgeWidth;
float minimumWidthSquared =
    minimumStablePixels * minimumStablePixels *
    edgeGradientSquared;
float fullyStableWidthSquared =
    fullyStablePixels * fullyStablePixels *
    edgeGradientSquared;

float pixelStability = smoothstep(
    minimumWidthSquared,
    fullyStableWidthSquared,
    edgeWidthSquared);
edgeMask *= pixelStability;
```

The comparison is equivalent to testing `edgeWidth / length(gradient)` without calculating a square root or reciprocal.

At default `Minimum Stable Accent Pixels = 1.0`:

| True perpendicular width | V1C.8 stability |
| ---: | ---: |
| `≤ 1.00 px` | `0` |
| `1.05 px` | approximately `0.138` |
| `1.10 px` | approximately `0.466` |
| `1.15 px` | approximately `0.824` |
| `≥ 1.20 px` | `1` |

No second squaring of `pixelStability` is permitted.

### Acceptance criteria

1. The legal `Edge Accent Width` range remains `0.01..0.50`; current serialized values remain unchanged.
2. The V1C.6 edge-antialias mask is restored exactly except that the already-approved `0.50` width ceiling remains.
3. Projected-width rejection uses `ddx`/`ddy` L2 magnitude in squared space, not `fwidth` as the width denominator.
4. At the default stable-pixel setting, true widths at or below `1.0 px` are rejected and widths at or above `1.2 px` are untouched.
5. The stability value is applied once; it is not squared.
6. `Minimum Stable Accent Pixels` remains material-only and excluded from the rebuild hash.
7. V1C.6 local-light ownership, intensity, bounded attenuation, falloff, activation, whiteness, body-fill behavior, and clustered-light compatibility remain unchanged.
8. V1D wind, geometry, instance data, rendering lifecycle, and ordinary body lighting remain unchanged.
9. Diagnostic/report identifiers use V1C.8 consistently.
10. The final project diff contains exactly the five authorized files.

### Invariants and non-goals

- Do not remove the stability control or return completely to V1C.6; retain selective rejection for genuine one-pixel/subpixel accents.
- Do not widen unstable bands, restore distance persistence, add temporal dithering, add camera-distance fades, or add LOD infrastructure.
- Do not alter `VegetationLighting.hlsl` or any light-response formula.
- Do not alter the serialized default or current user value for `Edge Accent Width` or `Minimum Stable Accent Pixels`.
- Do not add texture samples, light loops, shader passes, buffers, geometry, draw calls, runtime allocations, or instance rebuilds.
- Do not include unrelated Ground/River shader work.
- Do not claim Unity compilation, visual success, or measured GPU performance from source checks.

### File-by-file implementation sequence

| Item | File(s) | Required result | Status |
| --- | --- | --- | --- |
| V1C.8-0 | This document | Record objective, evidence, scope, derivation, approved formula, acceptance, invariants, risks, sequence, and validation before implementation. | Complete |
| V1C.8-1 | `SH_StylizedVegetationBenchmark.shader` | Restore V1C.6 filter shape, retain width `0.50`, replace V1C.7 angle-biased broad suppression with isotropic squared-space `1.0..1.2 px` rejection. | Complete; Unity shader import and visual validation pending |
| V1C.8-2 | `VegetationBenchmark.cs` | Update stable-pixel tooltip/report identifier to the V1C.8 contract; preserve property lifecycle and defaults. | Complete; Unity Inspector/material validation pending |
| V1C.8-3 | `VegetationBenchmarkEditor.cs` | Update help text and current log identifiers to V1C.8. | Complete; Unity editor compilation pending |
| V1C.8-4 | `Stylized_Vegetation_Architecture.md` | Make V1C.8 the current contract and mark V1C.7 filtering rejected/superseded. | Complete |
| V1C.8-5 | All approved files and direct contracts | Run exact-scope diff, complete final-file reread, lexical/property/hash/formula/numerical checks, unchanged-contract hashes, and package changed files. | Source audit complete; Unity validation pending |

### Risks and mitigations

- A threshold of `1.0` still intentionally removes exactly one-pixel bands. Mitigation: `Minimum Stable Accent Pixels` remains tunable down to `0.5` without rebuilding instances.
- Restoring the V1C.6 filter can reintroduce some subpixel energy before the stability gate. Mitigation: the isotropic `1.0..1.2 px` gate acts after the restored mask and rejects the intended unstable range.
- Derivatives can be undefined across divergent fragment execution. The shader already computes derivatives after the same geometry clip path; V1C.8 does not introduce a new divergence pattern and reuses both derivatives for AA and stability.
- Dense vegetation makes fragment arithmetic performance-sensitive. Mitigation: V1C.8 removes the V1C.7 reciprocal/division and extra stability-square multiply, adds no derivative beyond the existing X/Y derivatives underlying `fwidth`, and adds no sampling or loops.

### Validation plan

1. Unity 6000.5 import/compile with no vegetation C# or shader errors.
2. Gameplay-camera before/after comparison at `Edge Accent Width = 0.40`, `Minimum Stable Accent Pixels = 1.0`; coherent 1.2–3 px lines should return while isolated one-pixel noise remains suppressed.
3. Compare stable-pixel values `0.5`, `1.0`, and `1.5`; changes must be material-only and progressively reject more narrow accents.
4. Disable all point/spot lights and confirm no stylized edge accent remains under directional lighting.
5. Copy the comprehensive report and confirm `[Vegetation V1C.8 Benchmark Report]` plus the stable-pixel value.
6. Profile the accepted setting at 1440p against V1C.6/V1D and V1C.7 before production freeze.


### VEG-V1C.8 post-change consistency and compliance audit

**Outcome:** source implementation matches the approved V1C.8 plan and exact five-file scope. Unity compilation/import and visual/performance validation are unavailable in this environment and remain pending.

Actual modified project files relative to the user-tested V1C.7 baseline:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

No project file was added or deleted. No `.git` directory is present, so repository branch, HEAD, status, history, and commit comparison remain unavailable.

Implemented differences from V1C.7:

- Restored the accepted V1C.6 edge boundary filter: L1 derivative for AA, `×0.35`, clamp `0.0005..0.10`, and clamped smoothstep boundaries.
- Retained the expanded `Edge Accent Width` legal ceiling of `0.50` and preserved all serialized defaults/current values.
- Replaced `edgeWidth / fwidth(edgeDistance)` with explicit `ddx`/`ddy` derivatives and an isotropic L2 gradient comparison in squared space.
- Narrowed the default transition from `1.0..1.5 px` to `1.0..1.2 px`.
- Removed the second `pixelStability` squaring operation.
- Updated the stable-pixel tooltip to state true perpendicular width and the `+0.2 px` full-strength interval.
- Advanced comprehensive-report and Inspector action identifiers to V1C.8.
- Marked V1C.7 rejected/superseded in both architecture documents.

Behavior confirmed unchanged by exact diff and SHA-256 comparison:

- `VegetationLighting.hlsl`: `b8cad1b117d536f6d0f824869f0b833a4d1e6629ddf03aa475d355496d0f2d04`; directional/punctual eligibility remains `0, 0, 1`.
- `VegetationWindResponse.hlsl`: `09d72057979a31a21138793165183fc8500a9e2aeef8e5693f7a86aaa540cb92`; the user-validated V1D continuous time remains.
- `VegetationClusterMeshBuilder.cs`: `4174984e9e3ebb4692935600d97200a2eaaf7e6171e75d0f5b0a81b7936b6856`; UV edge production remains `0/1`.
- `VegetationInstanceData.cs`: `88bd7f30b87dac3bc9dd75d9aa9ce45b272775959c93312b9e1ec5ba51efaa62`; instance layout remains unchanged.
- `VegetationCommon.hlsl`: `4271470c1050021608ec852e4418bff4cd842d6a6389f70a24f3ae2a3a07bdd2`; vertex transformation remains unchanged.
- V1C.6 local-light intensity, attenuation, activation, whiteness, body-fill restraint, light-facing side selection, `_CLUSTER_LIGHT_LOOP`, and ordinary body lighting are unchanged.
- The stable-pixel control remains included once in `ComputeLightingConfigurationHash`, excluded from `ComputeRebuildConfigurationHash`, and published through `RefreshLightingMaterialProperties` only.

Source and mathematical validation:

- Exact baseline-to-final scope: five approved files changed; zero added; zero deleted.
- Complete final shader/editor review and complete runtime-file comparison against the fully reviewed baseline plus exact diff were completed. Both final architecture documents were checked by exact diff and Markdown validation.
- C-like lexical checks passed for final runtime C#, editor C#, shader, lighting include, and wind include: strings/comments, parentheses, braces, brackets, preprocessor balance, and control characters.
- Markdown fence and control-character checks passed for both modified documents.
- C#/shader property, range, clamp, hash, publication, report, ShaderLab property, and CBUFFER parity passed.
- V1C.7 denominator, broad `+0.5 px` interval, stability squaring, and projected-width division are absent.
- V1C.8 explicit `ddx`/`ddy`, restored AA mask, L2 squared gradient, `+0.20 px` interval, and one stability multiplication are present.
- Numerical checks passed: default stability is `0` at `≤1.0 px`, approximately `0.13752` at `1.05 px`, `0.46593` at `1.10 px`, `0.82415` at `1.15 px`, and `1` at `≥1.20 px`.
- The diagonal regression example passed: a true `1.60 px` band was classified as approximately `1.13137 px` by V1C.7 L1 math but remains fully stable under V1C.8.
- One hundred and two automated source/scope/formula checks passed.

Performance result by source inspection:

- V1C.8 uses the explicit `ddx` and `ddy` operations already underlying the removed `fwidth` call.
- V1C.8 removes the V1C.7 projected-width reciprocal/division and the extra stability-square multiplication.
- Added arithmetic is a squared-gradient sum and squared-threshold products; no `sqrt`, vector `length`, texture sample, light iteration, sine operation, vertex work, CPU update, compute dispatch, buffer, draw call, allocation, or shader pass was added.
- Expected cost relative to V1C.7 is approximately neutral to marginally lower. This is an analytical result, not a target-GPU measurement.

Available compiler limitation:

- Unity 6000.5, the Unity C# compiler, and the URP shader importer are unavailable here.
- No Roslyn/mcs/dotnet compiler or standalone HLSL compiler with Unity/URP include resolution is installed.
- Unity compilation and shader import are pending and are not represented as passed.

Concrete pending validation:

1. Import in Unity 6000.5.0f1 and provide the complete Console error if vegetation C# or the shader fails to compile.
2. From the gameplay camera at `Edge Accent Width = 0.40` and `Minimum Stable Accent Pixels = 1.0`, confirm coherent pre-V1C.7 strokes return while isolated one-pixel noise remains suppressed.
3. Compare `Minimum Stable Accent Pixels = 0.5`, `1.0`, and `1.5`; changes must update the material without rebuilding instances.
4. Disable all point/spot lights and confirm directional lighting produces no stylized edge accent.
5. Copy the comprehensive report and confirm `[Vegetation V1C.8 Benchmark Report]` and the stable-pixel value.
6. Profile the accepted configuration at 1440p against V1C.6/V1D and V1C.7 before production freeze.


## VEG-V1C.9 — Pre-Clip Linear Footprint and Per-Light Effective Width Gate

**Status: source implementation and post-change audit complete. Unity 6000.5 compilation, game-camera visual validation, and target-hardware GPU profiling remain pending.**

### Objective

Replace the user-rejected VEG-V1C.8 screen-space accent gate with a mathematically valid projected-width calculation and a punctual-light-specific effective-width test. The patch must remove false preservation caused by differentiating a saturated edge-distance function, account for the narrowing produced by the light-facing side selector, preserve the accepted VEG-V1C.6 edge-mask shape and `0.50` width ceiling, and leave ordinary local-light body illumination unchanged.

### User-authorized scope

Modify exactly:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl`

No Weather, wind, geometry-builder, instance-layout, scene, prefab, material asset, camera, URP asset, Ground, layer, tag, texture, buffer, draw-call, shader-pass, or debug-view change is authorized.

### Read-only review evidence

| Evidence | Finding | Status |
| --- | --- | --- |
| `Assets/AGENTS.md` | Requires complete read-only review, canonical plan as the first write, exact scope, implementation traceability, post-change full-file reread, direct producer/consumer audit, and explicit pending Unity validation. | Reviewed completely before this section was written |
| Current reconstructed tree at `/mnt/data/veg_v1c8_work/current` | Contains the user-tested VEG-V1C.8 source plus accepted VEG-V1C.6 clustered lighting and VEG-V1D continuous wind detail. No `.git` directory exists; branch, HEAD, status, history, and repository diff are unavailable. | Reviewed; limitation recorded |
| User screenshot `e31bc82f-bd80-4b6f-8912-b845202f522c.png` | Game-camera output still contains many isolated bright pixels and short bright fragments after VEG-V1C.8. | User-observed rejection evidence |
| `SH_StylizedVegetationBenchmark.shader > Frag` | VEG-V1C.8 computes `ddx`/`ddy` from `edgeDistance = 1 - saturate(abs(signedSilhouettePosition))`, after the taper clip path, and applies one global stability value before light evaluation. | Confirmed source defect |
| Saturated derivative derivation | When a pixel quad straddles the saturated edge, the measured derivative can approach zero as the inner sample approaches the boundary even when the true linear silhouette gradient remains finite. The resulting projected-width estimate can become arbitrarily large and preserve a subpixel band. | Confirmed mathematically |
| `VegetationLighting.hlsl > VegetationLightFacingEdge` | The final edge is multiplied by `smoothstep(0.05, 0.35, signedPosition × lateralLightDirection)` after the global width gate. At the selector midpoint, `signedPosition = 0.20 / lateralAlignment`; glancing punctual lights can therefore reduce the visibly strong band well below the authored width. | Confirmed by source and equation |
| `VegetationClusterMeshBuilder.AddStrip` | Every strip/card writes UV.x `0` and `1` at its two lateral vertices. The fragment shader's linear signed coordinate is the authoritative normalized cross-blade coordinate. | Direct producer reviewed; no edit required |
| `VegetationCommon.hlsl` and width stabilization | Width stabilization changes projected geometry before rasterization but does not alter UV.x. Fragment derivatives of the normalized signed coordinate therefore incorporate actual camera distance, orientation, taper, and stabilization. | Shared contract reviewed; no edit required |
| `VegetationLighting.hlsl` | Directional call sites pass edge eligibility `0`; the punctual additional-light loop passes `1`. Body and edge contributions are already separate. The per-light stability test belongs only in the eligible edge path. | Direct consumer reviewed completely; edit required |
| `VegetationBenchmark.cs` | `Minimum Stable Accent Pixels` is serialized, clamped, included only in the lighting hash, material-published, and reported. The default and lifecycle are valid; wording/version only must change. | Complete runtime file reviewed |
| `VegetationBenchmarkEditor.cs` | `DrawDefaultInspector` plus rebuild/lighting hash comparison already routes the control as material-only. Current V1C.8 labels require V1C.9 wording. | Complete editor caller reviewed |
| `VegetationWindResponse.hlsl` | VEG-V1D continuous timing and calm-sway behavior are independent of fragment edge stability and must remain byte-identical. | Cross-feature impact reviewed |
| `WeatherWindDomain`, `GeneratedGround`, `TimeOfDayController`, URP renderer/assets | These remain producers of wind, placement, sun/ambient, and clustered lights. No requested behavior requires editing them. | Related producer audit complete; no edit required |

### Confirmed defect 1 — saturated derivative is not a valid footprint

Current VEG-V1C.8 differentiates:

```text
edgeDistance = max(0, 1 - abs(signedSilhouettePosition))
```

Near one edge, let the true linear coordinate across adjacent screen samples be:

```text
s0 = 1 - epsilon
s1 = 1 - epsilon + g, with s1 > 1
```

The true linear gradient magnitude is `g`. The saturated edge-distance samples are:

```text
d0 = epsilon
d1 = 0
```

VEG-V1C.8 therefore measures `epsilon` instead of `g`. Its inferred projected width is proportional to:

```text
edgeWidth / epsilon
```

As `epsilon -> 0`, the inferred width approaches infinity although the true projected width remains `edgeWidth / g`. A subpixel band can therefore pass the stability gate at full strength depending only on subpixel sample placement.

Worked example:

```text
true signed-coordinate change per pixel g = 0.50
edge width w = 0.40
inside-sample saturated distance epsilon = 0.05
true projected width = 0.40 / 0.50 = 0.80 px
VEG-V1C.8 inferred width = 0.40 / 0.05 = 8.00 px
```

### Confirmed defect 2 — the global gate ignores per-light narrowing

The selected-side term is:

```text
selectedSide = smoothstep(0.05, 0.35, x * L)
```

where `x` is signed position on the selected half of the blade and `L = abs(dot(bladeLateral, lightDirection))`.

The smoothstep midpoint occurs at `x * L = 0.20`, so the region with at least half side-selection weight starts at:

```text
xHalf = 0.20 / max(L, epsilon)
```

The authored edge band starts at:

```text
xAuthored = 1 - edgeWidth
```

The effective half-strength-or-stronger band is:

```text
effectiveBandWidth = max(0, 1 - max(xAuthored, xHalf))
```

For `edgeWidth = 0.40`:

| Lateral alignment `L` | Effective normalized width |
| ---: | ---: |
| `0.50` | `0.40` |
| `0.30` | approximately `0.333` |
| `0.25` | `0.20` |
| `0.20` | `0.00` |

A globally measured `1.6 px` authored band can therefore become a `0.8 px` effective bright band at `L = 0.25`. VEG-V1C.8 preserves it because its gate runs before the per-light selector.

### Approved VEG-V1C.9 shader contract

#### A. Build a linear normalized coordinate and footprint before clip

```hlsl
float rawSignedSilhouettePosition = input.uv.x * 2.0 - 1.0;
float visibleHalfWidth = 1.0;

if (_GeometryCandidate > 0.5)
{
    float taperRange = max(0.0001, 1.0 - _TaperStart);
    float taperT = saturate((input.uv.y - _TaperStart) / taperRange);
    taperT *= taperT * (3.0 - 2.0 * taperT);
    float allowedWidth = lerp(
        0.92,
        max(0.02, _TipWidthRatio * 0.92),
        taperT);
    visibleHalfWidth = max(
        0.0001,
        allowedWidth - _AlphaCutoff * 0.08);
}

float signedSilhouettePosition =
    rawSignedSilhouettePosition / visibleHalfWidth;

float2 signedSilhouetteGradient = float2(
    ddx(signedSilhouettePosition),
    ddy(signedSilhouettePosition));

float signedGradientSquared = max(
    dot(signedSilhouetteGradient, signedSilhouetteGradient),
    0.00000025);
float pixelsPerSignedUnit = rsqrt(signedGradientSquared);

if (_GeometryCandidate > 0.5)
{
    clip(
        visibleHalfWidth -
        abs(rawSignedSilhouettePosition));
}
```

The derivative source is linear and unsaturated. The derivative instructions execute before `clip`; `_GeometryCandidate` is a material-uniform branch.

#### B. Preserve the accepted VEG-V1C.6 edge-mask shape

```hlsl
float edgeDistance =
    1.0 - saturate(abs(signedSilhouettePosition));
float edgeDerivativeAA = max(
    abs(signedSilhouetteGradient.x) +
    abs(signedSilhouetteGradient.y),
    0.0005);
float edgeWidth = clamp(_EdgeAccentWidth, 0.01, 0.50);
float edgeAntialias = clamp(
    edgeDerivativeAA * 0.35,
    0.0005,
    0.10);
float edgeMask = 1.0 - smoothstep(
    max(0.0, edgeWidth - edgeAntialias),
    min(1.0, edgeWidth + edgeAntialias),
    edgeDistance);
```

No global `pixelStability` multiplication remains in the main fragment shader.

#### C. Compute the effective width inside each eligible punctual-light evaluation

`VegetationLightFacingEdge` must expose the same `lateralAlignment` used by the side selector. The direct-light evaluator then computes:

```hlsl
float authoredBandStart = 1.0 - edgeWidth;
float halfWeightSidePosition =
    0.20 / max(lateralAlignment, 0.0001);
float effectiveBandStart = max(
    authoredBandStart,
    halfWeightSidePosition);
float effectiveBandWidth = max(
    0.0,
    1.0 - effectiveBandStart);
float effectiveBandPixels =
    effectiveBandWidth * pixelsPerSignedUnit;

float minimumStablePixels = clamp(
    minimumStableAccentPixels,
    0.5,
    2.0);
float pixelStability = smoothstep(
    minimumStablePixels,
    minimumStablePixels + 0.20,
    effectiveBandPixels);
```

The final edge term multiplies `pixelStability` exactly once. Directional lights remain ineligible and return zero edge output. Ordinary body lighting never uses this value.

### Acceptance criteria

1. The derivative source is `signedSilhouettePosition`, not saturated `edgeDistance`.
2. `ddx`/`ddy` and `pixelsPerSignedUnit` are calculated before the card `clip` statement.
3. The accepted VEG-V1C.6 edge-mask filter and `0.50` width ceiling are preserved.
4. The main fragment shader contains no global stable-pixel multiplier.
5. `VegetationLightFacingEdge` exposes the exact lateral alignment used by `orientationGate` and `selectedSide`.
6. Effective width is computed per direct light from authored width, `0.20 / lateralAlignment`, and `pixelsPerSignedUnit`.
7. `Minimum Stable Accent Pixels` uses a `+0.20 px` smooth transition and multiplies the final edge exactly once.
8. Main and clustered directional lights remain edge-ineligible; punctual additional lights remain eligible.
9. Ordinary ambient, sun, and punctual body lighting remain independent of pixel stability.
10. `Edge Accent Width` remains `0.01..0.50`; both serialized defaults remain unchanged.
11. Both edge controls remain material-only and excluded from the rebuild hash.
12. VEG-V1D wind, geometry generation, instance layout, rendering lifecycle, and all unrelated shader paths remain unchanged.
13. Diagnostic/report identifiers use VEG-V1C.9 consistently.
14. The final project diff contains exactly the six authorized files.

### Invariants and non-goals

- Do not alter edge accent strength, whiteness, activation threshold, local falloff power, bounded attenuation, body-fill response, or light ownership.
- Do not add a distance fade, temporal dithering, screen-space buffer, post-process, alpha-to-coverage, MSAA requirement, LOD system, or geometry widening.
- Do not change `Minimum Stable Accent Pixels` default `1.0` or `Edge Accent Width` default `0.10`.
- Do not change the user's current serialized values.
- Do not add texture samples, lighting iterations, shader passes, buffers, geometry, draw calls, runtime allocations, or instance rebuilds.
- Do not include similarly named River/Ground shader symbols.
- Do not claim Unity compilation, visual success, or measured GPU performance from source checks.

### File-by-file implementation sequence

| Item | File(s) | Required result | Status |
| --- | --- | --- | --- |
| V1C.9-0 | This document | Record objective, evidence, exact scope, derivation, approved code contract, acceptance, invariants, risks, sequence, and validation before implementation. | Complete |
| V1C.9-1 | `SH_StylizedVegetationBenchmark.shader` | Compute linear pre-clip footprint, preserve V1C.6 mask, remove global V1C.8 gate, and pass edge width/pixel scale/stability threshold into lighting evaluation. | Complete; Unity shader import and visual validation pending |
| V1C.9-2 | `VegetationLighting.hlsl` | Expose lateral alignment, compute per-light effective width/stability, apply it once to eligible edge radiance, and preserve every body/directional contract. | Complete; Unity shader import and visual validation pending |
| V1C.9-3 | `VegetationBenchmark.cs` | Update tooltip/report identifier only; preserve field lifecycle, defaults, hash routing, and publication. | Complete; Unity editor/runtime compilation pending |
| V1C.9-4 | `VegetationBenchmarkEditor.cs` | Update help text and current action identifiers to V1C.9. | Complete; Unity editor compilation pending |
| V1C.9-5 | `Stylized_Vegetation_Architecture.md` | Make V1C.9 the current contract and mark V1C.8 rejected/superseded. | Complete |
| V1C.9-6 | All approved files and direct contracts | Perform exact-scope diff, complete final-file reread, property/signature/call-site/eligibility/formula checks, lexical checks, unchanged-contract hashes, package changed files, and record pending Unity checks. | Source audit complete; packaging complete; Unity validation pending |

### Risks and mitigations

- `pixelsPerSignedUnit = rsqrt(gradientSquared)` can become large for nearly constant interpolants. Mitigation: clamp squared gradient to `2.5e-7`; large values correctly mean a wide stable projected band.
- `0.20 / lateralAlignment` becomes large for glancing lights. Mitigation: clamp the denominator to `1e-4`; the resulting effective width becomes zero, matching the existing side-selector behavior.
- The width test approximates the visible strong band using the selected-side midpoint; it does not identify small fragments caused solely by depth occlusion between unrelated cards. Mitigation: this patch first corrects both proven mathematical defects. A camera-distance removal remains the approved low-cost fallback only if noise remains after Unity validation.
- Dense vegetation makes per-light fragment arithmetic performance-sensitive. Mitigation: calculate `pixelsPerSignedUnit` once per fragment; add no texture sample or light iteration; per eligible light add only scalar max/divide/multiply/smoothstep work. Directional calls remain edge-ineligible.
- Signature changes can desynchronize shader call sites. Mitigation: audit every `VegetationEvaluateDirectLight`, `VegetationEvaluateLighting`, and `VegetationLightFacingEdge` call and validate exact argument parity.

### Performance analysis

Per fragment, VEG-V1C.9 replaces the V1C.8 squared-width global gate with:

- one `rsqrt` from the already required two derivatives;
- no global squared-width `smoothstep`;
- no global edge-mask stability multiplication.

Per evaluated direct light, the new contract adds:

- one exposed lateral-alignment scalar from an already computed dot product;
- one reciprocal/division for `0.20 / lateralAlignment`;
- several scalar `max`/subtract/multiply operations;
- one `smoothstep` and one final edge multiplication.

The cost scales with visible grass fragments times evaluated lights. No texture sample, loop, pass, draw, buffer, allocation, vertex operation, or compute dispatch is added. Exact target-GPU cost is unmeasured and must be profiled after visual acceptance.

### Validation plan

1. Unity 6000.5 import/compile with no vegetation C# or shader errors.
2. Gameplay-camera comparison at the current `Edge Accent Width` and `Minimum Stable Accent Pixels = 1.0`; false single-pixel preservation should reduce without reproducing the broad V1C.7 accent removal.
3. Compare stable-pixel values `0.5`, `1.0`, and `1.5`; changes must be material-only and progressively reject per-light glancing/sliver accents.
4. Disable all point/spot lights and rotate the sun; no stylized white edge accent may appear.
5. Copy the comprehensive report and confirm `[Vegetation V1C.9 Benchmark Report]` and the stable-pixel value.
6. Profile the accepted configuration at 1440p against V1C.8 before production freeze.

### VEG-V1C.9 post-change consistency and compliance audit

**Outcome:** source implementation matches the approved VEG-V1C.9 plan and exact six-file scope. Unity compilation/import, gameplay-camera visual success, and measured GPU performance are unavailable in this environment and remain pending.

Actual modified project files relative to the user-tested VEG-V1C.8 baseline:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl`

No project file was added or deleted.

#### Implemented differences from VEG-V1C.8

- `SH_StylizedVegetationBenchmark.shader` now builds `rawSignedSilhouettePosition`, resolves taper width, normalizes the signed coordinate, executes `ddx`/`ddy` and `rsqrt` before `clip`, and uses that unsaturated gradient for both antialiasing and pixels-per-signed-unit.
- The V1C.8 global squared-space stability block was removed completely. `edgeMask` is no longer globally multiplied by a width gate.
- The accepted VEG-V1C.6 edge-mask shape remains `edgeDerivativeAA × 0.35`, clamped to `0.0005..0.10`, with clamped smoothstep boundaries and the `0.50` edge-width ceiling.
- `VegetationLighting.hlsl` now exposes `lateralAlignment` from the exact dot product already used by the side selector.
- `VegetationPerLightEdgeStability` computes the band that remains at least half-strength after the `0.05..0.35` side smoothstep: `0.20 / lateralAlignment`, intersected with the authored edge band.
- Eligible punctual edge radiance multiplies the resulting `1.0..1.2 px` stability once. Ambient, sun, directional, and punctual body-light terms do not use this gate.
- `VegetationBenchmark.cs` preserves both serialized defaults and the existing material-only field lifecycle; only tooltip and comprehensive-report version changed.
- `VegetationBenchmarkEditor.cs` advances action labels/help text to V1C.9 without changing rebuild routing.
- Both architecture documents mark V1C.8 rejected/superseded and identify V1C.9 as current.

#### Preserved behavior and contracts

Byte-identical relative to VEG-V1C.8:

- `VegetationWindResponse.hlsl`, including VEG-V1D continuous calm sway;
- `VegetationClusterMeshBuilder.cs`, including UV.x `0/1` edge production;
- `VegetationInstanceData.cs`, including the 48-byte instance stride;
- `VegetationCommon.hlsl`, including width stabilization;
- `WeatherWindDomain.cs`;
- `GeneratedGround.cs`;
- `TimeOfDayController.cs`;
- `PC_Renderer.asset` and `PC_RPAsset.asset`.

Lighting ownership remains:

```text
main directional eligibility = 0
clustered directional eligibility = 0
punctual additional-light eligibility = 1
```

VEG-V1C.6 bounded attenuation, activation, whiteness, nonlinear master gain, body-fill response, clustered-light keywords, and post-albedo additive edge composition remain unchanged.

#### Mathematical verification

For `Edge Accent Width = 0.40`, the implemented effective normalized band is:

| Lateral alignment | Effective width |
| ---: | ---: |
| `0.50` | `0.40` |
| `0.30` | `0.333333` |
| `0.25` | `0.20` |
| `0.20` | `0.00` |

A `1.6 px` authored band at alignment `0.25` becomes `0.8 px` and receives zero stability at the default threshold. The same `1.6 px` band at alignment `0.50` remains fully stable.

The implemented `smoothstep(1.0, 1.2, effectivePixels)` produces:

| Effective width | Stability |
| ---: | ---: |
| `1.00 px` | `0.00000` |
| `1.05 px` | `0.15625` |
| `1.10 px` | `0.50000` |
| `1.15 px` | `0.84375` |
| `1.20 px` | `1.00000` |

The V1C.8 counterexample was reproduced: with true linear gradient `0.50`, edge width `0.40`, and saturated inner distance `0.05`, V1C.8 can infer `8.0 px` while the true width is `0.8 px`. V1C.9 uses the linear gradient and returns `0.8 px`.

#### Source validation evidence

`/mnt/data/veg_v1c9_work/VEG-V1C.9_Source_Validation.txt` records **141/141 passing checks**, including:

- exact six-file scope and no added/deleted project files;
- C#, HLSL, ShaderLab, preprocessor, and Markdown lexical/delimiter checks;
- serialized field/property/CBUFFER/hash/publication/report parity;
- derivative-before-clip ordering and removal of saturated derivative usage;
- removal of the V1C.8 global stability block;
- exact function signature and call-argument parity for all changed lighting functions;
- directional/punctual eligibility sequence `0, 0, 1`;
- byte-identical unrelated producers and consumers;
- numerical effective-width, stability, and V1C.8 counterexample checks;
- no new texture sampling, shader pass, light loop, distance fade, alpha-to-coverage, or temporal dithering.

No Unity compiler is available. The installed standalone Clang reports that its required HLSL standard header `hlsl.h` is absent, so it cannot provide a meaningful Unity/URP shader compile. Unity 6000.5 shader import and C# compilation remain mandatory next actions.

#### Performance reconciliation

Relative to VEG-V1C.8:

- per fragment: two derivatives remain; the old global squared-width smoothstep/multiplication are removed; one `rsqrt` calculates pixels per signed unit;
- per eligible punctual light: one reciprocal/division, scalar band-width arithmetic, one smoothstep, and one edge multiplication are added;
- no texture sample, lighting iteration, shader pass, buffer, geometry, draw call, allocation, vertex operation, or compute dispatch is added.

The cost now scales with visible grass fragments times eligible punctual lights rather than applying the full stability smoothstep globally to every fragment. Whether this is faster or slower on target hardware depends on compiler inlining/branching and active light count; the result is unmeasured and must not be represented as a performance pass.

#### Pending Unity validation

1. Import/compile in Unity 6000.5 with no vegetation C# or shader errors.
2. Compare the same game-camera/local-light setup at the current edge width and stable threshold `1.0`; noise should reduce without the broad line loss of V1C.7.
3. Compare stable thresholds `0.5`, `1.0`, and `1.5`; changes must remain material-only.
4. Disable all point/spot lights and rotate the sun; no stylized edge accent may remain.
5. Copy the comprehensive report and confirm `[Vegetation V1C.9 Benchmark Report]`.
6. Profile the accepted setting at 1440p before production freeze.


## VEG-V1E — Wind-Deformed Lighting Normals

**Status: source implementation and post-change audit complete. Unity 6000.5 compilation, visual validation, and target-GPU profiling remain pending.**

### Objective

Make ambient, sun, and punctual body lighting react continuously to the existing Weather-driven blade bend by deforming the lighting normal from the same analytical displacement already used by the vertex position. Preserve the accepted VEG-V1C.9 punctual-edge contract and VEG-V1D wind motion. Do not add real self-shadowing, shadow maps, an additional Weather sample, fragment-light loops, geometry, buffers, or draw calls.

### User-authorized scope

Modify exactly:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationWindResponse.hlsl`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

No `VegetationLighting.hlsl`, Weather compute/domain, mesh builder, instance layout, Ground, time-of-day, scene, prefab, material asset, URP asset, camera, layer, tag, or shadow-policy change is authorized.

### Read-only review evidence

| Evidence | Finding | Status |
| --- | --- | --- |
| `Assets/AGENTS.md` | Requires a complete review, the canonical plan as the first write, exact scope, final caller/producer reread, and explicit pending Unity validation. | Read completely |
| Reconstructed source: `Assets-Code-Archive(5).zip` overlaid in order with VEG-V1C.6, VEG-V1D, VEG-V1C.7, VEG-V1C.8, and VEG-V1C.9 | Reconstructs the user-tested VEG-V1C.9 + VEG-V1D state. All archives passed traversal checks. | Reviewed |
| `.git` search under the reconstructed tree | No `.git` directory exists. Branch, HEAD, status, history, and comparison with repository commits are unavailable. | Limitation recorded |
| `SH_StylizedVegetationBenchmark.shader::Vert` | Position is deformed by `ApplyVegetationWindResponse`, but `output.normalWS` is still only `TransformVegetationNormalToWorld(input.normalOS, yaw)`. The rendered geometry bends while the lighting normal remains static. | Defect confirmed |
| `VegetationWindResponse.hlsl::ApplyVegetationWindResponse` | The function already computes the complete macro + longitudinal + lateral XZ displacement, then multiplies it by response and `rootToTip²`. The pre-`rootToTip²` full-tip displacement can be returned without another Weather sample or sine. | Producer reviewed |
| `WeatherWindField.hlsl::SampleWeatherWindResponse` and `WeatherWindDomain::PublishShaderGlobals` | The accepted VEG-V1D path samples one response texture and predicts bend continuously. This update does not alter its sampling, timing, field cadence, or producer contract. | Shared producer/consumer reviewed |
| `VegetationClusterMeshBuilder::AddStrip` | Each row uses centerline height `H t` and static mesh normal `cross(up,right)`. `input.color.r` stores `t`, so `input.positionOS.y / max(t, ε)` recovers the blade-specific mesh height for non-root rows. | Geometry contract reviewed |
| `VegetationCommon.hlsl` | Instance `scale.y` is the blade-height multiplier. The rendering architecture assumes world-Y-up and Weather XZ displacement. | Transform contract reviewed |
| `VegetationLighting.hlsl` | Ambient SH and all direct-light body terms consume `inputData.normalWS`. The punctual edge selector reconstructs blade lateral direction from the unflipped normal before `Normal Up Bias`; a correctly deformed normal therefore updates body lighting without changing light ownership or adding a light loop. | Direct consumer reviewed |
| User validation | VEG-V1D motion works and VEG-V1C.9 is materially better. Those accepted sources are the preservation baseline. | User-observed evidence |

### Proven current mismatch

Current vertex behavior:

```text
P_rendered = P_static + A(t)
N_rendered = N_static
```

where the accepted wind displacement is quadratic along the blade:

```text
A(t) = A_tip × t²
```

The deformed centerline tangent is therefore:

```text
dP/dt = H × up + 2t × A_tip
```

After dividing by blade height `H`, the dimensionless tangent is:

```text
T = up + (2t / H) × A_tip
```

For blade lateral direction `R`, the wind-responsive surface normal is:

```text
N_wind = cross(T, R)
```

At `t = 0`, `T = up`, so the root retains the original normal. Increasing bend rotates the upper normal continuously with the same macro and detail displacement that moves the blade.

### Approved implementation contract

1. Extend `ApplyVegetationWindResponse` with `out float2 fullTipDisplacementXZ`.
2. If the Weather response is inactive, set the out value to zero and preserve the current early return.
3. Compute the existing macro + detail displacement once, multiply it by the existing stiffness/variation response, return that value as the full-tip displacement, then multiply by `rootToTip²` for the current position displacement.
4. Add material-only `Wind Normal Response`, range `0..1`, default `0.70`.
5. Recover per-vertex blade height as:

```text
bladeHeight = max((positionOS.y / max(rootToTip, 0.0001)) × instanceHeightScale, 0.05)
```

6. Reconstruct the current blade lateral direction from the yaw-transformed base normal:

```text
R = cross(N_static, up)
```

7. Compute:

```text
slope = A_tip × (2 × rootToTip / bladeHeight) × WindNormalResponse
T = (slope.x, 1, slope.y)
N_wind = cross(T, R)
```

8. Pass `N_wind` through the existing fragment normalization, face flip, and `Normal Up Bias` path. Do not normalize it in the vertex shader.
9. Include the new setting in validation, the lighting-only hash, runtime-material publication, Inspector help, and the comprehensive report. It must remain excluded from the geometry/rebuild hash.
10. Update diagnostic identifiers to `VEG-V1E`.

### Acceptance criteria

1. `Wind Normal Response = 0` reproduces the current static-normal lighting path.
2. `Wind Normal Response = 1` applies the complete analytical wind slope; default `0.70` moderates it.
3. Root normals remain unchanged; response increases continuously toward the tip.
4. Calm sway, strong gust motion, Weather timing, one response-texture sample, and two sine evaluations remain unchanged.
5. Ambient, sun, and punctual body lighting react to bend. The result remains light-directional rather than applying a fixed concave/convex tint.
6. Punctual edge ownership remains main directional `0`, clustered directional `0`, punctual `1`; VEG-V1C.9 width stability remains unchanged.
7. No new fragment operation, light loop, texture sample, shader pass, draw call, buffer, geometry, or runtime allocation is added.
8. The final diff contains exactly the six approved files.

### Performance model

Added per vegetation vertex:

- one blade-height reciprocal/division;
- scalar slope multiplications;
- one cross product for the wind-responsive normal.

Unchanged:

- one Weather response-field texture sample;
- two sine evaluations;
- 16 Hz Weather compute cadence;
- fragment-light loops and fragment shader control flow;
- geometry, instance count, 48-byte instance layout, buffers, draw calls, and memory.

At the existing 60,000-cluster/48-vertex stress case, the new arithmetic executes for approximately `2.88 million` vertices per frame. The result is expected to be a small vertex-stage increase, but target-GPU timing is unmeasured and remains mandatory before production freeze.

### Risks and non-goals

- This is analytical normal deformation, not real self-shadowing or a guaranteed light-independent “concave dark / convex bright” rule.
- The current geometry contains authored static centerline lean while its stored mesh normals remain flat. This patch responds to dynamic Weather displacement only; reconstructing static-lean normals is outside scope.
- The architecture assumes vegetation remains world-Y-up and Weather displacement remains in world XZ, matching the current renderer.
- Strong response can over-rotate normals or cause excessive lighting change; the new material-only control provides a zero-to-full response range.
- Do not change VEG-V1C.9 accent filtering, VEG-V1D motion constants, `Normal Up Bias`, or light response defaults in this patch.

### File-by-file implementation sequence

| Item | File(s) | Required result | Status |
| --- | --- | --- | --- |
| V1E-0 | This document | Record review, formula, scope, invariants, risks, and validation before implementation. | Complete |
| V1E-1 | `VegetationWindResponse.hlsl` | Return the already-computed response-scaled full-tip displacement without changing position motion. | Complete |
| V1E-2 | `SH_StylizedVegetationBenchmark.shader` | Add property/CBUFFER value, call the new signature, calculate the analytical normal, and retain the existing fragment lighting path. | Complete |
| V1E-3 | `VegetationBenchmark.cs` | Add serialized control, property ID, clamp, lighting hash, material publication, report value, and VEG-V1E heading. | Complete |
| V1E-4 | `VegetationBenchmarkEditor.cs` | Update material-only help and diagnostic identifiers. | Complete |
| V1E-5 | `Stylized_Vegetation_Architecture.md` | Record wind-deformed normals as the current accepted implementation direction rather than a deferred decision. | Complete |
| V1E-6 | All approved files and direct contracts | Run exact-scope, signature, property parity, lexical, mathematical, preservation, and caller/producer checks; record results here. | Complete |

### Required post-change validation

- Exact six-file scope and no added/deleted project files.
- C# delimiter/string/comment checks and newly introduced symbol/import audit.
- ShaderLab/HLSL delimiter, preprocessor, property/CBUFFER/signature/call parity.
- Verify `ApplyVegetationWindResponse` still samples Weather once and evaluates two sine functions.
- Verify VEG-V1C.9 lighting include is byte-identical and eligibility remains `0, 0, 1`.
- Verify the new control is included in the lighting hash and excluded from the rebuild hash.
- Verify `Wind Normal Response = 0` algebraically yields the static base normal.
- Unity 6000.5 C# compilation, URP shader import, visual comparison, and GPU profiling remain pending when Unity is unavailable.


### VEG-V1E post-change consistency and compliance audit

**Outcome:** the implementation matches the approved six-file plan. Source-level validation passed. Unity C# compilation, URP shader import, live visual validation, and target-GPU measurement are unavailable in this environment and remain pending.

#### Exact final scope

Modified exactly:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationWindResponse.hlsl`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

No project file was added or deleted. No `.git` metadata exists in the supplied tree, so branch, HEAD, status, history, and repository-commit comparison remain unavailable.

#### Final implementation reconciliation

- `VegetationWindResponse.hlsl` assigns `fullTipDisplacementXZ = 0` before the inactive early return, samples the Weather response once, retains exactly two sine evaluations, returns the existing macro/detail vector multiplied by the existing response, and preserves the accepted `rootToTip²` position displacement.
- `SH_StylizedVegetationBenchmark.shader` adds `_WindNormalResponse`, recovers the blade-specific height from `positionOS.y / rootToTip` and instance height scale, derives the quadratic-bend tangent, crosses it with the base blade lateral direction, and sends the resulting unnormalized normal through the existing fragment normalization, face flip, and `Normal Up Bias` path.
- `VegetationBenchmark.cs` adds the `0..1` serialized control with default `0.70`, clamps it, includes it only in the lighting configuration hash, publishes it to the runtime material, reports it, and updates the report heading to `V1E`.
- `VegetationBenchmarkEditor.cs` records the setting as material-only and updates action logs to `V1E`.
- `Stylized_Vegetation_Architecture.md` removes wind-deformed normals from deferred work and records the implemented analytical contract.

#### Preserved direct contracts

The following files are byte-identical to the accepted VEG-V1C.9 + VEG-V1D baseline:

- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationCommon.hlsl`
- `Assets/Game/Rendering/Weather/Includes/WeatherWindField.hlsl`
- `Assets/Game/Procedural/Weather/WeatherWindDomain.cs`
- `Assets/Game/Procedural/Vegetation/VegetationClusterMeshBuilder.cs`
- `Assets/Game/Procedural/Vegetation/VegetationInstanceData.cs`

Punctual-edge eligibility remains main directional `0`, clustered directional `0`, punctual additional light `1`. Unity 6.5 `_CLUSTER_LIGHT_LOOP` / `USE_CLUSTER_LIGHT_LOOP` compatibility remains present. VEG-V1C.9 edge stability and VEG-V1D continuous calm sway are unchanged.

#### Mathematical checks

- The old and new position formulas were compared over 10,000 random vectors, responses, and root-to-tip values. Maximum floating-point comparison error was `2.22e-16`; the position behavior is mathematically unchanged.
- For any horizontal unit base normal `N`, `Wind Normal Response = 0` gives `cross(up, cross(N, up)) = N`; 1,000 randomized orientations produced zero numerical error.
- The root slope is exactly zero because the slope multiplier contains `rootToTip`.
- With height `0.8 m`, response `0.70`, and tip displacement `0.057 / 0.25 / 0.50 m`, the analytical normal rotations are approximately `5.70 / 23.63 / 41.19 degrees`.

#### Source validation evidence

`/mnt/data/veg_v1e_work/VEG-V1E_Source_Validation.txt` records **66/66 passing checks**, including:

- exact six-file scope and no added/deleted project files;
- C#, HLSL, ShaderLab, preprocessor, comment/string, and delimiter checks;
- C# property ID, field, clamp, lighting hash, material publication, report, shader property, and CBUFFER parity;
- `ApplyVegetationWindResponse` signature/call parity and assignment on every return path;
- one Weather response sample and two sine evaluations;
- analytical normal formula, fragment normalization, face flip, and `Normal Up Bias` preservation;
- byte-identical direct producer/consumer contracts;
- edge eligibility sequence `0, 0, 1` and clustered-light compatibility;
- no new shadow sampling, shader pass, buffer, texture sample, `_Time`, geometry, or runtime allocation.

No Unity assemblies or C# compiler are available. The installed Clang cannot meaningfully compile the Unity/URP HLSL source because Unity package includes and the required HLSL default header are unavailable. These limitations are not treated as passes.

#### Performance reconciliation

Added per rendered vegetation vertex:

- one reciprocal/division for blade height;
- scalar slope arithmetic;
- one cross product for the deformed normal;
- one response-scaled `float2` value exposed from the existing wind calculation.

Unchanged:

- one Weather response texture sample;
- two sine evaluations;
- Weather resolution and 16 Hz cadence;
- fragment operations and light-loop count;
- geometry, 48-byte instance layout, buffers, draw calls, memory, and shadow policy.

This is an analytically small vertex-stage increase, not a measured performance pass. Profiling at 1440p and dense stress configurations remains mandatory.

#### Pending Unity validation

1. Import in Unity 6000.5 and provide the complete Console output if any vegetation C# or shader error appears.
2. Compare `Wind Normal Response = 0`, `0.70`, and `1.0` under a fixed directional light while the grass moves; `0` must reproduce static-normal lighting and higher values must increase bend-reactive shading.
3. Repeat with one point light and `Stylized Edge Accent = 0` to isolate ordinary body-light response from the graphic edge system.
4. Confirm calm sway and strong-gust geometry match VEG-V1D and that changing `Wind Normal Response` does not rebuild or alter instance count.
5. Restore the accepted edge settings and confirm point/spot-only VEG-V1C.9 accents remain correctly owned and filtered.
6. Copy the comprehensive report and confirm `[Vegetation V1E Benchmark Report]` and `Wind normal response` are present.

## VEG-V1E.1 — Exaggerated Wind Normal Response Range

### Status

**Implementation authorized by the user on 2026-07-21.** Source implementation and post-change audit complete. Unity 6000.5 compilation/import and live visual validation remain pending.

### Objective

Expand the existing material-only `Wind Normal Response` control from `0..1` to `0..4` so the user can exaggerate bend-reactive lighting without adding shader stages, samples, interpolators, fragment work, geometry, buffers, or serialized migration. Preserve the existing default `0.70` and every existing serialized value exactly.

### User-observed evidence

- Unity validation confirmed VEG-V1E compiles and functions.
- The user reported that `Wind Normal Response = 1.0` is visually too weak and reads approximately like a desired `0.25` setting.
- The user explicitly approved expanding the current response range without further runtime cost.

### Reviewed source evidence

- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
  - serialized field: `[Range(0f, 1f)] private float windNormalResponse = 0.70f;`
  - validation: `windNormalResponse = Mathf.Clamp01(windNormalResponse);`
  - control already participates only in `ComputeLightingConfigurationHash()` and `RefreshLightingMaterialProperties()`; it is absent from the placement/rebuild hash.
  - report currently identifies `V1E` and prints `Wind normal response`.
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`
  - property range is `Range(0, 1)` with default `0.70`.
  - vertex normal slope currently multiplies `saturate(_WindNormalResponse)`.
  - the control changes only the scalar analytical normal slope; it does not affect position deformation.
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
  - report action logs currently identify `V1E`.
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
  - current contract documents `0..1`, where `1` is the complete analytical slope.
- Direct producer/consumer review:
  - `VegetationWindResponse.hlsl` still returns one response-scaled full-tip displacement and is not changed by this update.
  - `VegetationLighting.hlsl` consumes the interpolated normal in the existing ambient/main/additional-light paths and is not changed by this update.
  - no Git metadata exists in the supplied source tree; branch, `HEAD`, history, and working-tree classifications are unavailable. The accepted VEG-V1E reconstructed tree is the comparison baseline.

### Mathematical contract

Current slope multiplier:

```text
slope = (2 × rootToTip / bladeHeight) × fullTipDisplacement × response
```

VEG-V1E.1 preserves that formula and expands only:

```text
response = clamp(WindNormalResponse, 0, 4)
```

Control interpretation:

- `0`: exact prior static-normal result.
- `1`: complete analytical wind slope from VEG-V1E.
- `2`: twice the analytical slope.
- `3`: strong stylized exaggeration.
- `4`: maximum supported exaggeration.

The resulting normal angle remains naturally bounded by normalization:

```text
angle = atan(baseSlope × response)
```

Existing values from `0..1` retain exactly the same scalar and therefore exactly the same shader result.

### Approved file scope

Modify only:

1. `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
2. `Assets/Docs/Stylized_Vegetation_Architecture.md`
3. `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
4. `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
5. `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

### Invariants and non-goals

- Preserve the serialized field name and default `0.70`; do not migrate or rewrite existing scene/prefab values.
- Preserve exact VEG-V1E behavior for every value in `0..1`.
- Preserve position deformation, Weather sampling, continuous calm sway, spring response, lighting ownership, accent filtering, geometry, instance data, draw calls, and shader variants.
- Keep the control material-only; changing it must not rebuild instances.
- Do not change `VegetationWindResponse.hlsl`, `VegetationLighting.hlsl`, Weather code, geometry builders, scenes, prefabs, materials, URP assets, Ground, layers, or tags.
- Do not add a second response control or hidden multiplier.

### File-by-file implementation plan

| ID | File | Change | Status |
|---|---|---|---|
| V1E.1-0 | This document | Record review, objective, scope, formula, invariants, risks, and validation before implementation. | Complete |
| V1E.1-1 | `VegetationBenchmark.cs` | Expand the Inspector range and validation clamp to `0..4`; preserve default, property ID, lighting-only hash, material publication, and report value; update report identifier to `V1E.1`. | Complete |
| V1E.1-2 | `SH_StylizedVegetationBenchmark.shader` | Expand ShaderLab range to `0..4` and replace `saturate` with `clamp(..., 0, 4)` in the existing slope multiplier. | Complete |
| V1E.1-3 | `VegetationBenchmarkEditor.cs` | Update diagnostic action identifiers to `V1E.1`; preserve all Inspector behavior. | Complete |
| V1E.1-4 | `Stylized_Vegetation_Architecture.md` | Record the expanded stylized range while preserving `1` as the analytical reference point. | Complete |
| V1E.1-5 | All approved files and direct contracts | Run exact-scope, range/property parity, formula, serialized-default preservation, rebuild-hash exclusion, lexical, and diff checks; record the post-change audit here. | Complete |

### Risks and mitigations

- **Risk:** values above `1` can rotate normals strongly and may produce exaggerated light/dark transitions.
  - **Mitigation:** the effect is user-controlled, bounded at `4`, and naturally angle-bounded by normal normalization. Default and existing values remain unchanged.
- **Risk:** changing only the Inspector range while retaining `Clamp01` or shader `saturate` would make values above `1` ineffective.
  - **Mitigation:** validate parity across C# attribute, C# clamp, ShaderLab property, and HLSL clamp.
- **Risk:** accidental inclusion in the placement hash would rebuild dense grass while tuning.
  - **Mitigation:** confirm the field remains only in `ComputeLightingConfigurationHash()` and runtime material publication.
- **Risk:** diagnostic text can misidentify the current source state.
  - **Mitigation:** update report and action identifiers to `V1E.1` in the approved files.

### Performance expectation

Runtime work is structurally unchanged. VEG-V1E already performs the scalar multiplication. Replacing a `0..1` clamp with a `0..4` clamp does not add a texture sample, sine, cross product, interpolator, fragment operation, light iteration, draw call, buffer, allocation, or compute dispatch. Exact GPU code generation remains compiler-dependent and must not be represented as measured without target profiling.

### Validation requirements

- Exact five-file final scope; no added/deleted project files.
- C# range, validation clamp, ShaderLab range, and HLSL clamp all equal `0..4`.
- Default remains `0.70` in C# and ShaderLab.
- Existing `0..1` inputs produce identical scalar response before and after.
- Field remains in lighting hash/material publication and absent from rebuild hash.
- Wind-response include, lighting include, geometry, Weather, scenes, prefabs, materials, and URP assets remain unchanged.
- C#, ShaderLab, HLSL/preprocessor, delimiter, string/comment, and property-parity checks pass where available.
- Unity 6000.5 compilation/import and visual confirmation remain pending unless run in the user project.


### VEG-V1E.1 post-change consistency and compliance audit

#### Actual affected files

Exactly the five approved files changed:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

No project file was added or deleted. No scene, prefab, material, URP asset, Ground file, Weather file, geometry builder, instance-data file, lighting include, or wind-response include changed.

#### Intentional source differences

- `VegetationBenchmark.cs`
  - Inspector range changed from `0..1` to `0..4`.
  - validation changed from `Mathf.Clamp01` to `Mathf.Clamp(..., 0, 4)`.
  - default remains `0.70`.
  - serialized field name, property ID, lighting-only hash participation, material publication, and report value remain unchanged.
  - comprehensive report identifier changed from `V1E` to `V1E.1`.
- `SH_StylizedVegetationBenchmark.shader`
  - ShaderLab range changed from `0..1` to `0..4`.
  - slope scalar changed from `saturate(_WindNormalResponse)` to `clamp(_WindNormalResponse, 0, 4)`.
  - the tangent, cross product, interpolated normal, fragment normalization, face flip, and `Normal Up Bias` path remain unchanged.
- `VegetationBenchmarkEditor.cs`
  - four diagnostic action identifiers changed from `V1E` to `V1E.1`.
- `Stylized_Vegetation_Architecture.md`
  - current dynamic-normal control contract now records `0..4`, with `1` retaining the analytical reference and values above `1` documented as stylized exaggeration.

#### Mathematical preservation evidence

For every existing serialized value `x` in `[0,1]`:

```text
old = saturate(x) = x
new = clamp(x, 0, 4) = x
```

A numerical sweep of 1,001 values across `[0,1]` produced maximum scalar difference `0`. Existing scenes therefore preserve the exact response scalar. Values `1.25`, `2`, `2.5`, `3`, and `4` pass through unchanged; values above `4` clamp to `4`.

The response still modifies only the analytical normal slope:

```text
normalSlopeScale = 2 × rootToTip × reciprocal(bladeHeight) × response
```

Position deformation is unchanged because `VegetationWindResponse.hlsl` is byte-identical to the accepted VEG-V1E baseline.

#### Source validation evidence

`/mnt/data/veg_v1e1_work/VEG-V1E.1_Source_Validation.txt` records **57/57 passing checks**, including:

- exact five-file scope and no added/deleted project files;
- C# Inspector range, validation clamp, serialized default, property ID, lighting hash, material publication, and report parity;
- ShaderLab property range/default, CBUFFER property, and HLSL clamp parity;
- absence of the obsolete response `Clamp01` and shader `saturate` paths;
- field absence from the placement/rebuild hash;
- mathematical identity for all existing `0..1` values and bounded pass-through through `4`;
- byte-identical wind response, lighting include, geometry builder, instance layout, and Weather domain contracts;
- C#, ShaderLab/HLSL, preprocessor-delimiter, comment/string, and Markdown-fence checks;
- no changed scene, prefab, material, or URP asset.

No Unity assemblies, Unity shader importer, or compatible standalone URP HLSL environment are available. Unity compilation/import is not represented as passed.

#### Performance reconciliation

Runtime structure is unchanged:

- no added vertex or fragment instruction block;
- no added texture sample, sine, cross product, interpolator, lighting iteration, shader variant, draw call, buffer, allocation, or compute dispatch;
- one scalar bounded response remains in the existing vertex slope multiplication;
- changing the control remains material-only and does not rebuild instances.

Exact machine-code equivalence of the old and new clamp operation is compiler-dependent and unmeasured. The source introduces no additional rendering stage or data dependency.

#### Pending Unity validation

1. Import in Unity 6000.5 and provide the complete Console error if vegetation C# or shader compilation fails.
2. Compare `Wind Normal Response = 1`, `2.5`, and `4` under fixed sun/local-light conditions while wind is active; values above `1` must visibly strengthen only lighting-normal rotation.
3. Set the control back to its previous value and confirm the prior VEG-V1E appearance returns exactly.
4. Confirm changing the control does not rebuild or alter instance count, geometry, or wind motion.
5. Copy the comprehensive report and confirm `[Vegetation V1E.1 Benchmark Report]` and the selected wind-normal value are present.

## VEG-V1E.2 — Bend-Side Body Shading Response

**Status: source implementation complete; Unity 6000.5 compilation and visual validation pending.**

### Objective

Add an explicit low-cost bend-side body-shading response so a rendered blade face on the concave side of Weather-driven curvature darkens and the opposite convex face brightens. Preserve the existing `Wind Normal Response` range `0..4` as a separate normal-tilt control. The new effect must increase body-form contrast rather than merely translating the wrapped-diffuse boundary.

### User-observed evidence and diagnosis

- Unity validation confirmed VEG-V1E.1 compiles and the analytical normal response functions.
- The user reported that values above `1` read primarily as a normal/shadow offset: the darker boundary moves down the blade but does not materially expand or deepen.
- Current shader evidence: `SH_StylizedVegetationBenchmark.shader::Vert` multiplies the analytical normal slope by `_WindNormalResponse`; `Frag` then evaluates two-sided wrapped diffuse through `VegetationLighting.hlsl` and applies `Normal Up Bias`. The scalar can rotate the normal but has no independent contrast amplitude.
- Mathematical evidence: for a fixed lighting transition slope `s*`, the current transition height is `t* = s*H / (2AG)`. Increasing normal gain `G` lowers `t*`; it moves the transition instead of independently controlling darkness or brightness.
- `VegetationLighting.hlsl::VegetationTwoSidedWrappedDiffuse` uses `abs(dot(N,L))` and a wrap floor. It deliberately does not encode concave-versus-convex face identity.
- `VegetationWindResponse.hlsl::ApplyVegetationWindResponse` already returns response-scaled `fullTipDisplacementXZ`. No additional Weather sample or wind-function output is required.
- `VegetationClusterMeshBuilder.cs::AddStrip` assigns a stable horizontal base normal `cross(up,right)` and consistent front-face winding. This supports deriving rendered bend side from the base normal, full-tip displacement, and `SV_IsFrontFace`.
- No `.git` directory exists in the supplied source tree. Branch, `HEAD`, history, and working-tree classifications are unavailable. The accepted VEG-V1E.1 reconstructed tree is the comparison baseline.

### Approved and reviewed file scope

Implementation may modify only:

1. `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
2. `Assets/Docs/Stylized_Vegetation_Architecture.md`
3. `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
4. `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
5. `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

Reviewed dependency that must remain byte-identical unless new evidence requires a plan update:

6. `Assets/Game/Rendering/Vegetation/Includes/VegetationWindResponse.hlsl`

The initial expected scope included the wind include, but complete review confirmed VEG-V1E already exposes all required displacement data. Modifying it would be unnecessary.

### Control contract

Add one material-only control:

```text
Wind Bend Shading Response
Range: 0..2
Default: 1.0
```

Interpretation:

- `0`: no bend-side multiplier; VEG-V1E.1 body result.
- `1`: designed response, up to `30%` concave darkening and `12%` convex brightening at full bend/height response.
- `2`: maximum supported exaggeration, up to `60%` concave darkening and `24%` convex brightening.

One master control is used instead of separate darkening/brightening controls to keep the Inspector compact and preserve a deliberately darker-than-brighter ratio that avoids flashing.

### Mathematical contract

At each vertex, derive the signed bend component normal to the undeformed card:

```text
inverseBladeHeight = rootToTip / max(scaledVertexHeight, epsilon)
signedNormalBendRatio = dot(baseNormalWS.xz, fullTipDisplacementXZ) × inverseBladeHeight
```

For every non-root vertex, `scaledVertexHeight = bladeHeight × rootToTip`, so the ratio is exactly `dot(...) / bladeHeight`. At the root, both `rootToTip` and the derived reciprocal are zero, preventing the temporary `0.05 m` fallback used by the normal-slope path from creating an exaggerated interpolated bend ratio.

This scalar is positive when the front face bends toward its base normal (front concave), negative when it bends away (front convex), and approaches zero when bend lies within the card plane. It therefore incorporates both curvature magnitude and the card-relative bend direction without a normalization or extra vector length.

In the fragment shader:

```text
faceSign = frontFace ? +1 : -1
renderedSignedBend = signedNormalBendRatio × faceSign
bendActivation = smoothstep(0.03, 0.30, abs(renderedSignedBend))
heightResponse = t²(3 - 2t)
shade = clamp(WindBendShadingResponse, 0, 2) × bendActivation × heightResponse
```

Rendered-side weights:

```text
concave = step(0, renderedSignedBend)
convex = 1 - concave
```

Body multiplier:

```text
bodyMultiplier = 1
    - 0.30 × concave × shade
    + 0.12 × convex  × shade
```

Apply the multiplier only to `heightColor.rgb × resolved body lighting`; add the punctual graphic edge accent afterward unchanged.

Bounds at the maximum response `2`:

```text
minimum body multiplier = 1 - 0.30 × 2 = 0.40
maximum body multiplier = 1 + 0.12 × 2 = 1.24
```

### Invariants and non-goals

- Preserve the existing `Wind Normal Response` `0..4` range, default `0.70`, and analytical normal formula.
- Keep bend-side shading independent of normal-tilt gain so the user can use a lower normal value and stronger body-form response.
- Preserve one Weather response sample and two sine evaluations per vegetation vertex.
- Preserve Weather cadence, wind geometry, V1D continuous sway, V1C.9 edge filtering, punctual-only edge ownership, lighting loops, cluster compatibility, geometry, instance stride, draw calls, and buffers.
- Keep the new field in the lighting hash/material publication only; it must not enter the rebuild hash.
- Do not add shadows, self-shadow tracing, textures, extra light loops, additional Weather reads, scene/prefab/material/URP edits, layers, tags, or new files.
- Do not multiply `vegetationLighting.edgeAccent`; the new response is body shading only.
- Do not alter `VegetationLighting.hlsl`; the response is intentionally light-independent stylized curvature contrast applied after resolved body lighting.

### File-by-file implementation plan

| ID | File | Change | Status |
|---|---|---|---|
| V1E.2-0 | This document | Record evidence, objective, scope, formula, invariants, risks, and validation before implementation. | Complete |
| V1E.2-1 | `VegetationBenchmark.cs` | Add property ID and serialized `0..2` material-only control with default `1`; clamp, lighting hash, material publication, report value, and `V1E.2` report identifier. | Complete |
| V1E.2-2 | `SH_StylizedVegetationBenchmark.shader` | Add property/CBUFFER field, derive and interpolate signed normal bend ratio from existing full-tip displacement, evaluate face-aware bend response, and multiply body lighting only. | Complete |
| V1E.2-3 | `VegetationBenchmarkEditor.cs` | Update material-only help text and diagnostic identifiers to `V1E.2`. | Complete |
| V1E.2-4 | `Stylized_Vegetation_Architecture.md` | Record separate normal-tilt and bend-side contrast responsibilities and fixed asymmetric response. | Complete |
| V1E.2-5 | All approved files and reviewed dependencies | Run exact-scope, property/signature parity, hash classification, formula bounds, front/back sign, lexical, preservation, and diff checks; record the post-change audit here. | Complete |

### Risks and mitigations

- **Risk:** incorrect front/back sign would brighten the concave side and darken the convex side.
  - **Mitigation:** validate against `AddStrip` winding and worked vector cases: `baseNormal=(0,0,-1), bend=(0,0,-A)` must yield positive front-face signed bend; the back face must invert it.
- **Risk:** recovering blade height as `positionY / rootToTip` requires a fallback at the root; using that fallback in an interpolated bend ratio would overstate the root value.
  - **Mitigation:** derive reciprocal height as `rootToTip / max(positionY × scaleY, epsilon)`, which is exactly `1/H` for non-root rows and exactly zero at the root.
- **Risk:** the effect could flash during very small sign changes near zero bend.
  - **Mitigation:** `smoothstep(0.03,0.30,abs(...))` suppresses the effect near zero; side selection is irrelevant while activation is zero.
- **Risk:** excessive darkening can crush colour under night lighting.
  - **Mitigation:** default response is `1`, maximum response is `2`, and the multiplier is analytically bounded to `0.40..1.24` before fog.
- **Risk:** applying the multiplier to the accent would undo the separately tuned punctual edge contract.
  - **Mitigation:** multiply only `heightColor × lighting` and add `edgeAccent` afterward.
- **Risk:** tuning could trigger dense instance rebuilds.
  - **Mitigation:** include the new field only in `ComputeLightingConfigurationHash` and runtime material publication.

### Performance expectation

Incremental vertex work:

- one `float2` dot product against the already available base normal and full-tip displacement;
- one reciprocal reuse through the already calculated blade height;
- one scalar varying.

Incremental fragment work:

- one face-sign multiply;
- one absolute value;
- one `smoothstep`;
- one root-to-tip smooth polynomial;
- one `step` and several scalar multiplies/additions.

No texture sample, sine, cross product, lighting iteration, shadow sample, draw call, buffer, allocation, compute dispatch, or geometry change is added. The new varying adds interpolation bandwidth and the fragment arithmetic applies to visible grass pixels, so the cost is low but not zero. Target profiling remains required before production freeze.

### Validation requirements

- Exact five-file final modification set; wind-response include and lighting include remain byte-identical.
- C# field/property ID/clamp/hash/material/report and ShaderLab property/CBUFFER parity.
- New field absent from `ComputeRebuildConfigurationHash`.
- `Wind Bend Shading Response = 0` yields body multiplier exactly `1` for all inputs.
- Front/back signs invert exactly; in-plane bending produces zero signed bend ratio.
- Response `2` cannot produce a body multiplier below `0.40` or above `1.24`.
- Position deformation, normal slope, Weather sample count, sine count, edge accent composition, eligibility `0,0,1`, and V1C.9 filtering remain unchanged.
- C#, HLSL/ShaderLab, preprocessor, delimiters, comments/strings, Markdown fences, and property-parity checks pass where available.
- Unity 6000.5 compilation/import and visual confirmation remain pending unless run in the user project.


### VEG-V1E.2 post-change consistency and compliance audit

#### Actual affected files

Exactly five project files changed:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

The approved/reviewed `VegetationWindResponse.hlsl` dependency did not change because VEG-V1E already exposes response-scaled full-tip displacement. No file was added or deleted. No scene, prefab, material, URP asset, Weather file, lighting include, geometry builder, instance-data file, Ground file, layer, or tag changed.

#### Intentional source differences

- `VegetationBenchmark.cs`
  - adds `_WindBendShadingResponse` property ID;
  - adds serialized `Wind Bend Shading Response`, range `0..2`, default `1`;
  - clamps it to `0..2`;
  - includes it only in `ComputeLightingConfigurationHash`;
  - publishes it through `RefreshLightingMaterialProperties`;
  - reports it and updates the comprehensive identifier to `V1E.2`.
- `SH_StylizedVegetationBenchmark.shader`
  - adds ShaderLab/CBUFFER parity for the new control;
  - adds one scalar `TEXCOORD6` varying;
  - derives a root-safe reciprocal blade height as `rootToTip / max(positionY × scaleY, epsilon)`;
  - preserves the VEG-V1E analytical normal slope exactly for every mesh row;
  - projects existing full-tip displacement onto the undeformed base normal;
  - flips the signed ratio by `SV_IsFrontFace`;
  - applies the documented `0.03..0.30` activation, smooth height response, `30%/12%` asymmetric body multiplier;
  - applies the multiplier only to body colour × resolved body lighting, then adds the punctual edge accent unchanged.
- `VegetationBenchmarkEditor.cs`
  - identifies bend-side body shading as material-only;
  - updates four action-log identifiers to `V1E.2`.
- `Stylized_Vegetation_Architecture.md`
  - separates normal tilt from explicit bend-side contrast and records the current bounds and cost.

#### Mathematical and preservation evidence

- `Wind Bend Shading Response = 0` makes `bendShade = 0`, so `bendBodyMultiplier = 1` for every bend, face, and blade height.
- At response `2`, full concave response is `1 - 0.30 × 2 = 0.40`; full convex response is `1 + 0.12 × 2 = 1.24`. An exhaustive numerical sweep over response `0..2`, signed bend `-0.5..0.5`, and root-to-tip `0..1` stayed inside those bounds.
- The root height factor is exactly zero, so body shading is unchanged at the root.
- Worked winding cases confirm that bending toward the front base normal yields positive/concave front response, bending away yields negative/convex front response, and the back face inverts both signs.
- Bending within the card plane has zero projection onto the base normal and therefore no false concave/convex response.
- For non-root vertices, `rootToTip / (bladeHeight × rootToTip) = 1 / bladeHeight`; at the root it is zero. This preserves the prior analytical normal slope while preventing an exaggerated interpolated root bend ratio.
- `VegetationWindResponse.hlsl`, `VegetationLighting.hlsl`, `VegetationClusterMeshBuilder.cs`, `VegetationInstanceData.cs`, and `WeatherWindDomain.cs` are byte-identical to the accepted VEG-V1E.1 baseline.
- Weather sampling remains one response-field sample per vertex and detail motion remains two sine evaluations.
- Punctual-edge eligibility remains directional `0`, clustered directional `0`, punctual additional light `1`; Unity 6.5 cluster-loop compatibility and VEG-V1C.9 effective-width filtering remain unchanged.

#### Source validation evidence

`/mnt/data/veg_v1e2_work/VEG-V1E.2_Source_Validation.txt` records **88/88 passing checks**, including:

- exact five-file scope and no added/deleted project files;
- complete C#/ShaderLab/CBUFFER property lifecycle parity;
- lighting-only hash classification and rebuild-hash exclusion;
- signed bend transport, face inversion, activation, height response, asymmetric coefficients, and body-only composition;
- response-zero identity, root identity, full-strength values, analytical bounds, card-winding sign cases, in-plane zero response, reciprocal-height equivalence, and prior normal-slope preservation;
- byte-identical wind, lighting, geometry, instance, and Weather contracts;
- unchanged Weather sample count, sine count, light-loop constructs, edge eligibility, serialized assets, and cluster keyword;
- balanced C#, ShaderLab/HLSL, preprocessor-delimiter, and Markdown structures.

A standalone Clang HLSL syntax attempt was made, but the installed compiler lacks its required `hlsl.h` default header. Unity assemblies and the Unity URP shader importer are unavailable. Unity compilation/import is therefore explicitly pending and is not represented as passed.

#### Performance reconciliation

The patch adds one scalar interpolator, one vertex `float2` dot product and scalar operations, and one low-cost fragment response block. It adds no texture sample, sine, cross product, light loop, shadow sample, shader pass, geometry, draw call, buffer, allocation, or compute dispatch. The effect remains material-only and does not rebuild instances. Exact GPU cost is unmeasured and must be included in the later dense-field 1440p profiling pass.

#### Pending Unity validation

1. Import in Unity 6000.5 and provide the complete Console error if vegetation C# or shader compilation fails.
2. Set `Wind Normal Response` near `1`, compare `Wind Bend Shading Response = 0`, `1`, and `2` under fixed sun lighting, and confirm the new control deepens/expands concave darkness and adds restrained convex brightness rather than moving only the boundary.
3. Set `Stylized Edge Accent = 0` during the comparison to isolate body shading, then restore the accepted edge settings and confirm the accent is unchanged.
4. Confirm the root remains stable, calm/gust geometry is unchanged, and changing either lighting control does not rebuild or alter instance count.
5. Copy the comprehensive report and confirm `[Vegetation V1E.2 Benchmark Report]` plus both wind-lighting response values are present.

## VEG-V1F — Baked Grass Macro Patch Composition

### Status

**Source implemented and audited — Unity import, visual validation, rebuild timing, and GPU profiling pending.**

### Objective

Add broad, deterministic, world-space light/dark grass patches so dense vegetation does not read as tens of thousands of statistically identical clusters. Preserve the existing independent per-cluster colour variation as the micro layer. Evaluate the macro field only when vegetation instances are rebuilt, store one signed patch value per cluster in the already-unused `VariationPhase.w` channel, and apply only low-cost colour scaling during rendering.

### User decision and Ground-pattern computation finding

The user approved an independent grass-owned field unless reusing Ground's exact patch pattern saves computation.

It does not save active runtime computation in the current architecture:

- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundMacro.hlsl::PS3D_EvaluateGroundMacroRegion()` evaluates Ground macro noise only inside the Ground shader and publishes no reusable CPU field or texture.
- `Assets/Game/Procedural/Ground/GeneratedGround.cs` publishes Ground macro parameters to its material but exposes no CPU macro-region sampling contract.
- Exact Ground matching would therefore require either duplicating equivalent noise during vegetation generation or adding a new cross-subsystem Ground sampling/cache contract.
- The approved independent field and a hypothetical exact Ground field can both be baked once per accepted grass cluster, so neither has a per-frame runtime-compute advantage. The independent field avoids the additional Ground dependency.

**Decision:** implement the independent grass-owned field now. Ground coupling remains optional future work and is not part of VEG-V1F.

### Approved file scope

```text
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Docs/Stylized_Vegetation_Architecture.md
Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs
Assets/Game/Rendering/Vegetation/Includes/VegetationCommon.hlsl
Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader
```

No scene, prefab, material asset, Ground source, Ground shader, Weather source, geometry builder, instance-layout source, renderer asset, layer, or tag change is authorized.

### Read-only review evidence

- `Assets/AGENTS.md` was read completely before editing. It requires the canonical plan to be the first write, exact scope, full post-change reread, and explicit pending Unity validation.
- The supplied/reconstructed current source is VEG-V1E.2 over VEG-V1C.9 and VEG-V1D. No `.git` directory exists, so branch, `HEAD`, history, and working-tree status are unavailable. The extracted current tree is the source baseline.
- `VegetationBenchmark.cs::BuildInstances()` currently generates independent `colorVariation` and `bladeVariation` values but no spatially coherent macro value.
- `VegetationInstanceData.cs` remains a three-`Vector4`, 48-byte record. `VariationPhase.w` is written as `0` by the constructor and has no current CPU or HLSL consumer.
- `VegetationCommon.hlsl::DecodeVegetationInstance()` currently decodes only `variationPhase.xyz`; it has one caller in `SH_StylizedVegetationBenchmark.shader::Vert()`.
- `SH_StylizedVegetationBenchmark.shader::Frag()` currently evaluates the independent micro scale `lerp(0.90, 1.10, colorVariation)` per fragment.
- `VegetationBenchmarkEditor.cs::OnInspectorGUI()` classifies fields by `ComputeRebuildConfigurationHash()` and `ComputeLightingConfigurationHash()`. Pattern-layout controls must be rebuild-owned; dark/light strengths can remain material-only.
- `PixelSurfaceGroundMacro.hlsl` uses a warped procedural source and derives one signed region as `lightRegion - darkRegion`. VEG-V1F adopts that signed-field composition concept, not Ground's exact pattern or shader evaluator.

### Control contract

Add a `Grass Macro Patch Composition` Inspector group:

```text
Grass Patch Scale                 0.5..12 m, default 4.5
Grass Patch Pattern Seed          unrestricted integer, default 0
Grass Patch Transition Softness   0..1, default 0.75
Average Grass Patch Separation    >= 0, default 1.0
Dark Patch Strength               0..0.5, default 0.12
Light Patch Strength              0..0.5, default 0.08
```

Ownership:

- Scale, pattern seed, transition softness, and separation alter the baked per-cluster patch field and belong in the rebuild hash.
- Dark and light strengths alter only runtime colour composition and belong in the material-only lighting hash.
- Strength `0` disables that tonal side without changing placement or rebuilding.

### Procedural field contract

For each accepted cluster at world position `p`, calculate one signed value `P in [-1, 1]` during `BuildInstances()`:

1. Divide world XZ by `Grass Patch Scale`.
2. Evaluate two deterministic 2D value-noise channels for low-frequency warp.
3. Evaluate a secondary region source and use it to bend the primary region coordinate.
4. Derive dark/neutral/light boundaries from `Average Grass Patch Separation`.
5. Derive transition width from `Grass Patch Transition Softness`.
6. Return `P = clamp(lightRegion - darkRegion, -1, 1)`.

The CPU evaluator uses integer lattice hashing and cubic interpolation. It is world-space continuous for adjacent vegetation fields that use the same patch controls. It does not call `Mathf.PerlinNoise`, allocate per sample, or depend on the placement RNG sequence.

Store `P` in `VegetationInstanceData.VariationPhase.w` after construction. `VegetationInstanceData.cs` and the 48-byte stride remain unchanged.

### Render composition contract

Decode `macroPatch` from `variationPhase.w` in the vertex shader. Compute one constant-per-cluster colour scale:

```text
darkMask = max(-P, 0)
lightMask = max(P, 0)
macroScale = 1 - darkMask * DarkPatchStrength
               + lightMask * LightPatchStrength
microScale = lerp(0.90, 1.10, saturate(colorVariation))
colourScale = microScale * macroScale
```

Pass `colourScale` through the existing scalar `TEXCOORD3` slot and multiply the complete root/base/tip authored gradient by it in the fragment shader.

This preserves root-to-tip colour structure and keeps the punctual edge accent outside the macro-body scaling path.

### Invariants and non-goals

- Preserve the 48-byte instance stride and existing three-`Vector4` CPU/GPU layout.
- Preserve placement count, position, yaw, scale, stiffness, phase, blade variation, wind, wind-deformed normals, bend-side shading, lighting ownership, edge-accent filtering, and all geometry.
- Preserve the existing placement RNG sequence. Macro patch evaluation must not consume values from `System.Random`.
- No runtime procedural-noise evaluation in the vertex or fragment shader.
- No new texture, texture sample, buffer, draw call, render pass, GameObject, component, or memory allocation.
- No exact Ground-pattern following and no Ground-coupling control in this patch.
- No patch-driven height, width, stiffness, density, wind response, hue shift, or saturation change.

### File-by-file implementation sequence

| Item | File(s) | Required result | Status |
| --- | --- | --- | --- |
| V1F.0 | This document | Record evidence, scope, controls, formulas, invariants, risks, and validation before code edits. | Complete |
| V1F.1 | `VegetationBenchmark.cs` | Add controls, validation, hash ownership, deterministic CPU field evaluation, per-instance storage, statistics, material publication, and report output. | Complete; Unity validation pending |
| V1F.2 | `VegetationCommon.hlsl` | Decode `variationPhase.w` as signed macro patch without changing layout or sampling. | Complete; Unity validation pending |
| V1F.3 | Benchmark shader | Add two material strengths, calculate combined macro/micro scale in the vertex stage, reuse `TEXCOORD3`, and reduce the fragment colour operation to one multiply. | Complete; Unity validation pending |
| V1F.4 | `VegetationBenchmarkEditor.cs` | Update help/report identifiers and clearly distinguish rebuild-owned pattern controls from material-only strengths. | Complete; Unity validation pending |
| V1F.5 | `Stylized_Vegetation_Architecture.md` | Record the accepted spatial macro-variation contract and ownership. | Complete; Unity validation pending |
| V1F.6 | All approved files and direct dependencies | Complete scope, layout, determinism, formula, lexical, and preservation checks; record Unity-only validation as pending. | Complete; Unity validation pending |

### Performance model

At 60,000 accepted clusters:

- Dirty/rebuild CPU: 60,000 deterministic macro-field evaluations. Each evaluation contains four 2D value-noise calls; each 2D call interpolates four integer-hash lattice values. This is approximately 960,000 lattice hashes per full rebuild, plus scalar interpolation and threshold math.
- Active runtime memory: no increase because `VariationPhase.w` already occupies the existing 48-byte record.
- Vertex bandwidth: no increase because the complete `variationPhase` `float4` is already fetched.
- Vertex arithmetic: two masks and several scalar multiply/add operations once per vertex.
- Fragment arithmetic: the existing `saturate + lerp` micro-colour calculation is removed; the fragment keeps one multiply by an interpolated scale. Expected fragment cost is neutral to lower.
- Draw calls, texture samples, Weather samples, sine evaluations, light loops, geometry, and buffers remain unchanged.

The rebuild CPU time is unmeasured and must be reported honestly after Unity profiling. It is intentionally traded for negligible active-frame cost.

### Risks and mitigations

- Broad patches may be visually too strong in dense grass. Defaults are restrained at 12% darkening and 8% brightening.
- A scale that is too small will become mottled noise. The default is 4.5 m and the minimum is 0.5 m for deliberate stress testing only.
- Independent grass and Ground patches may overlap without correlation. This is accepted for the first implementation; optional Ground influence is a later visual decision.
- Existing serialized components may receive default values for newly added fields only after Unity deserialization/import. Unity validation must confirm the Inspector values and report.

### Validation requirements

1. Unity 6000.5 imports C# and shader changes without errors or pink rendering.
2. With dark/light strengths at `0`, the rendered result matches VEG-V1E.2 apart from the report identifier.
3. At defaults, broad coherent patches are visible from the gameplay camera while per-cluster micro variation remains.
4. Changing scale/seed/softness/separation rebuilds and changes the deterministic hash; changing only dark/light strengths updates immediately without rebuilding or changing the instance count/hash.
5. The report confirms 48-byte stride, patch settings, dark/neutral/light instance counts, signed-value range/mean, and no new runtime resource.
6. Profile one 50-clusters/m² rebuild and compare active GPU frame time with strengths `0` versus defaults.

### VEG-V1F post-change consistency and compliance audit

#### Actual affected files

```text
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Docs/Stylized_Vegetation_Architecture.md
Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs
Assets/Game/Rendering/Vegetation/Includes/VegetationCommon.hlsl
Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader
```

The actual six-file delta matches the approved scope exactly. No file was added or deleted.

#### Implemented differences

- Added the six approved Grass Macro Patch Composition controls with the recorded defaults and ownership.
- Added a deterministic CPU world-XZ field using two warp noises, one secondary region noise, one primary region noise, integer lattice hashing, and cubic interpolation.
- Stored the signed result in the existing `VariationPhase.w` channel after construction. `VegetationInstanceData.cs` remains byte-identical, three `Vector4` values, and 48 bytes.
- Added dark/neutral/light counts and signed min/mean/max to the comprehensive report.
- Added HLSL decoding of `variationPhase.w`.
- Reused scalar `TEXCOORD3` for a combined macro/micro colour scale calculated in the vertex stage.
- Replaced the prior fragment `saturate + lerp` micro calculation with one multiply by the interpolated scale.
- Updated Inspector help and report/log identifiers to VEG-V1F.
- Added the ownership and performance contract to the higher-level stylized vegetation architecture.

#### Corrected implementation issue found during source audit

The first local implementation draft used `Mathf.SmoothStep(edge0, edge1, value)` as though it were HLSL `smoothstep(edge0, edge1, value)`. Unity's `Mathf.SmoothStep(from, to, t)` instead interpolates between output values using a normalized `t`. Before delivery, this was replaced with `EvaluateGrassSmoothStep(edge0, edge1, value)`, which explicitly calculates `t = saturate((value - edge0) / (edge1 - edge0))` and then `t²(3-2t)`. The final source contains no incorrect `Mathf.SmoothStep` use in the macro evaluator.

#### Preservation evidence

The following files are byte-identical to the VEG-V1E.2 baseline:

- `VegetationInstanceData.cs`
- `VegetationClusterMeshBuilder.cs`
- `VegetationWindResponse.hlsl`
- `VegetationLighting.hlsl`
- `WeatherWindDomain.cs`
- `CS_WeatherWindField.compute`
- `GeneratedGround.cs`
- `PixelSurfaceGroundMacro.hlsl`

The final source preserves:

- placement RNG draw order; the macro evaluator consumes no `System.Random` values;
- instance position, count, yaw, scale, stiffness, phase, blade variation, geometry, wind, deformed normals, bend-side shading, and lighting;
- one indirect draw, one response-field sample per vertex, two wind sine evaluations, one ShaderLab pass, and Unity 6.5 cluster-light-loop compatibility;
- punctual edge accent composition outside the patch-scaled body colour;
- zero-strength equivalence: with both patch strengths at zero, the colour scale is exactly the previous `lerp(0.90, 1.10, colorVariation)` result.

#### Mathematical and numerical evidence

At default strengths:

```text
dark macro scale minimum     = 1 - 0.12 = 0.88
light macro scale maximum    = 1 + 0.08 = 1.08
combined minimum with micro  = 0.90 × 0.88 = 0.792
combined maximum with micro  = 1.10 × 1.08 = 1.188
```

A deterministic numerical reproduction over the default `40 × 30 m` domain produced approximately:

```text
dark samples     28.6%
neutral samples  39.1%
light samples    32.3%
minimum / maximum signed value  -1 / +1
```

This proves the default field contains all three region classes and reaches both full plateaus. It does not prove the Unity visual result is accepted.

#### Source validation evidence

`VEG-V1F_Source_Validation.txt` records **67/67 passed checks**, including:

- exact approved scope and no added/deleted files;
- C#/ShaderLab/HLSL property, range, default, hash, and material-publication parity;
- unchanged 48-byte CPU/GPU instance layout;
- `VariationPhase.w` write/decode/caller parity;
- deterministic field formula, four value-noise calls, custom edge smoothstep, signed bounds, and no placement-RNG consumption;
- existing scalar interpolator reuse and removal of the previous per-fragment micro calculation;
- zero-strength mathematical equivalence and default value bounds;
- preservation hashes for Ground, Weather, geometry, wind, lighting, and instance-layout dependencies;
- balanced C#, HLSL, ShaderLab delimiters and absence of conflict markers.

No Unity compiler, URP shader importer, or target GPU is available in this environment. C# compilation, shader import, runtime visuals, rebuild duration, and frame-time cost remain pending.

#### Performance reconciliation

- Persistent instance memory remains unchanged at `instanceCount × 48 bytes`.
- No texture, texture sample, buffer, pass, draw call, or fragment noise was added.
- Dirty-time generation performs approximately 16 lattice hashes per accepted cluster: about 960,000 at 60,000 clusters.
- The vertex shader gains small scalar mask/composition arithmetic.
- The fragment shader removes its existing micro-colour `saturate + lerp` and retains one scalar multiply. The active GPU difference is expected to be small, but this is an unverified expectation until profiled.

#### Pending Unity validation

1. Import in Unity 6000.5 and provide the complete Console error if C# or shader compilation fails.
2. Confirm `Dark Patch Strength = 0` and `Light Patch Strength = 0` reproduce the VEG-V1E.2 body colour.
3. At defaults, capture the gameplay camera and confirm broad coherent dark, neutral, and light grass regions without fine mottling.
4. Confirm scale/seed/softness/separation rebuild and change the deterministic hash, while strength-only edits update immediately without changing the hash or instance count.
5. Copy the comprehensive report and verify the VEG-V1F heading, 48-byte stride, patch settings, region counts, and signed statistics.
6. Measure one 50-clusters/m² rebuild duration and compare active GPU frame time at strengths `0/0` versus defaults.

