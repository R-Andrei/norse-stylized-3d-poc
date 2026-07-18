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

VegetationWind
    global direction
    gust structure
    per-instance phase inputs

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

Every visible blade should move under wind.

### 7.1 Wind composition

```text
finalWind =
    prevailing directional bend
  + broad moving gust
  + medium spatial variation
  + deterministic per-cluster phase
  + blade-height weighting
  + stiffness response
```

### 7.2 Performance constraints

Initial shader target:

- no more than two wind-noise evaluations per vertex;
- prefer analytic or texture-assisted low-frequency functions;
- no expensive high-octave procedural noise per blade;
- roots remain fixed;
- tips receive full response;
- taller blades receive stronger displacement;
- wind movement must remain coherent across neighbouring clusters.

### 7.3 Wind update cadence

Wind is evaluated every rendered frame.

Global wind parameters may update more slowly, but the vertex shader must produce smooth frame-rate motion across the entire screen.

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

### V2 — Full-screen wind

- global direction;
- gust field;
- per-instance phase;
- root anchoring;
- low-cost stylized lighting.

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
+ full-screen wind
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
