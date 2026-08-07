# Vegetation Rendering and Interaction Architecture

## Status

**Canonical architecture and implementation ledger — interaction milestone accepted through 2026-07-22**

This document defines the production architecture, measurable performance targets, visual contract, implementation history, and validation gates for dense interactive vegetation in the Norse Stylized 3D PoC.


## Weather cloud-shadow receiver contract — source implemented; Unity validation pending

Vegetation is a mandatory receiver of the Weather-owned cloud-shadow illumination field. Grass, future shrubs, and trees must share the same world-space cloud boundary as Ground, Generated Mass, River, actors, and buildings. Vegetation owns only its material and lighting response; it does not own cloud pattern, phase, direction, speed, coverage, or sun gating.

Cloud integration must attenuate the environmental-sun term without changing ambient minimum visibility, local-light response, punctual-light edge accents, wind deformation, immediate interaction, persistent trample, coverage authoring, instance placement, geometry selection, or recovery simulation. Weather wind remains authoritative for motion; cloud movement may consume the same wind direction but must not use the Vegetation spring-response field.

V0 uses the authoritative sun's URP directional-light cookie. `SH_StylizedVegetationBenchmark.shader` now compiles `_LIGHT_COOKIES`, and `VegetationLighting.hlsl` now resolves the main light through the world-position-aware overload with a neutral shadow mask. This preserves the existing no-geometric-main-shadow policy while enabling cookie projection; Unity import and visual validation remain pending. Vegetation must consume the cookie-aware main-light path exactly once; it must not evaluate a separate procedural cloud field per vertex or update cloud values per instance. Dense transparent Vegetation remains the primary performance benchmark because fragment overdraw can amplify the cookie sample cost. The highest accepted density, normal gameplay camera, target resolution, and realistic visible-patch load must be profiled after complete implementation. No per-instance CPU update, field rebuild, or recurring allocation is allowed. A receiver-compliance audit must identify any Vegetation shader that lacks cookie support.

Exact representation and files remain governed by `Assets/Docs/Weather_Cloud_Shadow_Handoff.md`. A per-instance or vertex-optimized hybrid path is fallback only if measured cookie cost is materially unacceptable.

The production ownership and interaction stack is implemented through `VEG-V2-INTERACT.2B.2`. The user has directly accepted the immediate response, Ground-owned historical trails, delayed asymmetric recovery, and circle/cone/line ability stamps in Unity. Historical patch sections retain their original source-validation limitations and failed intermediate approaches as evidence; `VEG-V2-CLOSE.1` is the authoritative closure status.

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
| V3 | Full-visible-domain immediate actor interaction using a swept scene field, fixed-world direction modes, and displaced-grass Weather suppression. | Compose actor deformation with external wind response without trail history. | Implemented at source level by INTERACT.1/1A; Unity validation pending |
| V4 | Opt-in Ground-owned trample history for selected large movers and queued circle, cone-sector, and line ability stamps, with timed or session-persistent recovery. | Compose explicit history with external wind and immediate interaction; ordinary player movement remains history-free. | Moving trails user-validated through INTERACT.2A.2; ability stamps complete at source level in INTERACT.2B, Unity validation pending |
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

The obsolete vegetation-owned wind provider and migration subclass were deleted by `VEG-V2-INFRA.3`. Vegetation consumes only the scene-owned Weather domain.

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

### 8.1 Two distinct interaction lifetimes

Vegetation interaction is split by ownership and lifetime. Ordinary actor displacement must not create persistent world history by default. Historical trampling is explicitly opt-in.

```text
Immediate scene domain
    current actor bodies and swept movement
    camera/anchor-centred world XZ field
    fast response and recovery
    no persistent trail state

Ground-owned historical trample domain
    opt-in large-object swept trails
    fixed Ground-local history shared by all recipes on that Ground
    timed or session-persistent recovery
    queued circle, cone-sector, and line ability stamps
```

Both domain update rates are independent user controls with the accepted range `5–60 Hz`. INTERACT.1 owns the scene immediate cadence. INTERACT.2A owns the Ground historical cadence. Ordinary interactors default to `Trail Mode = Off`, so enabling immediate interaction never silently creates history.

### 8.2 Immediate visible-domain field

`VEG-V2-INTERACT.1` implements one scene-owned `VegetationInteractionDomain`. It follows an explicit gameplay anchor or a fallback camera-ground projection and initially uses a 256² `ARGBHalf` response field at 0.25 m/cell, covering 64 × 64 m. `VEG-V2-INTERACT.1B` adds a configurable central recenter margin: the toroidal field remains stationary while the anchor stays inside that margin, then recentres in one accumulated cell shift. This preserves overlapping response cells while avoiding a recenter dispatch for ordinary quarter-metre anchor motion.

The fixed update rate is configurable from `5–60 Hz`, default `20 Hz`. Previous/current field interpolation remains the response path while cells are actively supported. Releasing cells bypass stale previous-state interpolation and apply analytical render-time decay from the latest committed state using the configured immediate recovery time. Swept capsules also expose a `Sweep Tail Retention` control so the previous end of a low-cadence sweep can release more strongly than the current actor end without creating disconnected gaps. These mechanisms reduce the visual wake at 10–15 Hz without increasing the compute update rate or creating stored trail history.

Every production vegetation layer samples the same field. Shader cost is fixed with respect to actor count: actor loops occur only in the bounded compute update, never per blade or per cluster.

### 8.3 Interactor contract

Any moving or stationary GameObject may receive `VegetationInteractor`. No player class, Rigidbody, CharacterController, tag, or layer is required. Motion is derived from transform positions sampled by the published domain.

Interactor controls are divided into footprint, direction shaping, movement response, and capacity priority:

```text
radius
horizontal bend strength
temporary flatten strength
direction mode: Radial / World X Biased / Hybrid
world-X bias
world-Z strength
movement-direction influence
full movement-response speed
maximum sweep distance
priority
```

The fixed directional basis is world space: lateral is world `±X`, and map-up/down is world `±Z`. Actor rotation, facing, and camera rotation do not redefine those axes. `Radial` preserves the accepted radial plus optional movement-directed behavior. `World X Biased` ignores movement direction and redirects the radial result toward world `±X`; `Hybrid` redirects the accepted radial/movement result. `World X Bias` controls redirection toward pure world `±X`, while `World Z Strength` independently multiplies the final biased Z component before normalization. Setting `World Z Strength` to zero therefore removes Z displacement regardless of the X-bias value.

Each fixed update submits a swept capsule from the previous sampled XZ position to the current position. Movement beyond the configured sweep limit is treated as a teleport and does not draw a long path through the field. A stationary actor still produces radial or fixed-world-X parting according to its selected direction mode. Cells near the exact actor X centreline use a deterministic world-cell side choice so the split remains stable rather than flickering.

### 8.4 Ordinary actor behavior

The player and ordinary enemies use immediate displacement only unless gameplay explicitly enables a later trample mode. Expected behavior is:

```text
actor present
    per-interactor radial, fixed-world-X-biased, or hybrid parting
    optional movement-directed lean only in Radial/Hybrid modes
    independently attenuated world-Z displacement in world-X modes
    temporary flattening

actor leaves
    the swept tail is attenuated toward the previous sample by Sweep Tail Retention
    releasing cells decay analytically every rendered frame through the immediate recovery time
    no persistent trail remains
```

The field can still retain at most one fixed-step of source-position uncertainty because actor occupancy is sampled at the configured cadence. INTERACT.1B removes the additional full-strength swept-tail hold and stale previous-field interpolation that previously made this uncertainty resemble a visible trail.

### 8.5 Per-layer response

Every `VegetationLayer` exposes independent immediate-response controls:

| Control | Purpose |
| --- | --- |
| Interaction Bend Response | horizontal response multiplier |
| Interaction Flatten Response | temporary vertical flatten multiplier |
| Interaction Height Exponent | how much of the lower blade remains rigid |
| Maximum Interaction Bend | world-space bend cap and conservative bounds expansion |
| Interaction Normal Response | lighting-normal response to interaction bend |
| Wind Influence On Displaced Grass | Weather response retained as effective interaction displacement approaches full strength; `1` preserves normal wind and `0` can suppress it completely at full displacement |

This allows tall soft and short stiff recipes to react differently while sharing the same physical footprint. Roots remain planted. Flattening is capped so a blade cannot be pushed below its root height by interaction alone. Interaction is sampled once before Weather deformation; the same interpolated sample drives wind attenuation and immediate displacement without duplicating field texture reads.

### 8.6 Ground-owned historical trample field

`VEG-V2-INTERACT.2A` adds one optional `VegetationTrampleDomain` to the existing Ground `Vegetation` root. `VEG-V2-INTERACT.2A.1` adds delayed timed recovery, and `VEG-V2-INTERACT.2A.2` replaces the symmetric return with the accepted asymmetric slow–fast–slow curve. The domain owns two fixed Ground-local `ARGBHalf` deformation textures, one fixed Ground-local `RGFloat` timing texture, and one bounded writer buffer. Every recipe under that Ground samples the same stored deformation footprint through per-material binding immediately before draw; multiple Grounds therefore keep independent history without global texture conflicts. The timing texture is compute-only and adds no vegetation-shader sample.

Only `VegetationInteractor` components whose `Trail Mode` is not `Off` write history. The historical sampler uses domain-owned movement history, independent from the immediate domain cadence. Writers accumulate movement until `Trail Stamp Spacing`, reject movement below `Minimum Trail Speed`, and submit a continuous swept capsule between accepted stamps. Timed writes carry independent delay and recovery-duration values. Restamping a cell resets or extends its hold phase; a weaker overlapping write cannot shorten an existing longer hold or recovery duration. The historical field never edits recipe coverage or rebuilds vegetation.

Trail modes are:

| Mode | Meaning |
| --- | --- |
| Off | no historical writes; default for player and ordinary actors |
| Timed | stored bend/flatten state remains fully held for `Recovery Delay Seconds`, then follows a fixed asymmetric slow–fast–slow return over `Recovery Duration Seconds`: approximately 15% restored at 50% time, 90% restored at 90% time, and fully restored at completion |
| Session Persistent | zero recovery while the runtime field exists; cleared by Reset Field, Ground/scene reload, or component teardown; no save-file permanence is claimed |

The field update rate is independently configurable from `5–60 Hz`, default `12 Hz`. Each Ground permits exactly one active trample domain. Duplicate domains are invalid and do not publish resources.

Every `VegetationLayer` exposes historical-response controls:

| Control | Purpose |
| --- | --- |
| Trample Bend Response | horizontal stored-bend multiplier |
| Trample Flatten Response | stored flatten multiplier |
| Trample Height Exponent | lower-blade rigidity under stored state |
| Maximum Trample Bend | world-space historical bend cap and conservative bounds expansion |
| Trample Normal Response | lighting-normal response to historical deformation |
| Wind Influence On Trampled Grass | Weather retained as historical deformation approaches full strength |

Historical deformation composes with Weather and immediate displacement in the vertex stage. The shader samples the historical field once, with one previous/current bilinear pair, and contains no writer loop.

`VEG-V2-INTERACT.2B` adds gameplay-authored one-shot stamps to the same historical field. The public API supports `Circle`, `Cone`, and `Line`. Circle and cone share one radial-sector implementation: a 360-degree arc is a circle, while a smaller arc is a cone facing an authored world-XZ direction. Line is a width-controlled capsule between authored world endpoints. Every stamp independently specifies bend, flattening, displacement mode, recovery mode, delay, duration, deterministic edge irregularity, noise scale, seed, and priority. Requests are queued on intersecting Ground domains and consumed once by the next historical fixed step; no temporary actor, coverage edit, vegetation rebuild, new historical texture, or vegetation-shader sample is required.

Ability displacement modes are `Radial Outward`, `Fixed World Direction`, `Away From Centreline`, and `Flatten Only`. For overlapping ability stamps, the stronger bend direction is retained rather than averaging opposed vectors toward zero; flattening takes the maximum. Session-persistent state dominates timed state, while timed restamping can extend but cannot shorten the existing delay or recovery duration.

`VegetationTrampleStampTester` is a dedicated optional test harness intended for temporary attachment to the player or another scene object. It stores editable named configurations, uses the attached transform as the stamp origin and facing basis, previews the selected shape with Gizmos, and exposes Inspector actions for selected stamping, previous/next/random configuration selection, and bounded randomized variants. The tester calls the exact public runtime stamp API and is not required by gameplay code.

Ordinary actors remain history-free by default. Persistent state must not be stored in the camera-centred immediate field because recentering would discard it.

---

## 9. Interaction Capacity Targets

INTERACT.1 should support the normal visible-domain load:

```text
1 player
+ 24 ordinary enemies
+ 8 large or high-strength immediate interactors
+ headroom for additional moving objects
```

The immediate domain defaults to 48 uploaded records and exposes a `1–96` capacity control. Active interactors register directly; no per-frame hierarchy search occurs. When candidates exceed capacity, priority is ordered first and distance to the field anchor second. The comprehensive report exposes registered, in-domain candidate, uploaded, and overflow counts.

INTERACT.2B ability stamps use a separate bounded request queue and structured buffer owned by each historical Ground domain. The default budget is 128 pending requests and 32 uploaded one-shot stamps per historical fixed step. When no ability requests are queued, the existing stamp loop executes zero iterations and recurring historical texture ownership remains unchanged.

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

- lower cluster retention or authored density;
- retain the fixed 18-vertex / 12-triangle CrossedCards cluster;
- 256² immediate field;
- immediate interaction control remains available across `5–60 Hz`; begin Low-quality tuning at 10 Hz;
- Ground historical trail/trample control remains `5–60 Hz`; begin Low-quality tuning at 8–10 Hz;
- no grass shadow casting;
- simplified lighting;
- maximum two visible grass materials.

### Medium

- higher cluster retention or authored density;
- retain the fixed 18-vertex / 12-triangle CrossedCards cluster;
- immediate interaction control remains `5–60 Hz`; begin Medium-quality tuning at 20 Hz;
- Ground historical trail/trample control remains independently adjustable across `5–60 Hz`; begin Medium-quality tuning at 12–16 Hz;
- richer gust variation;
- up to four visible vegetation materials.

### High

- maximum authored density;
- fixed production CrossedCards geometry;
- immediate interaction control remains `5–60 Hz`; begin High-quality tuning at 30 Hz;
- optional 512² historical trample field only if measured and visibly justified;
- optional receive-shadow or enhanced lighting;
- no assumption that real grass shadow casting becomes enabled.

Quality tiers should reduce representation cost, not remove full-screen interaction.

---

## 16. Interaction Stress Validation

The immediate interaction prototype must use a deliberately hostile visible-domain scene:

```text
one camera-sized dense CrossedCards grass field
+ grass covering at least 85% of eligible ground
+ player moving continuously with no persistent trail mode
+ 24 enemies moving through separate and overlapping routes
+ 4 large immediate interactors
+ full-screen Weather wind
+ patch edges visible against bare ground
+ 2560 × 1440
+ Low quality target
```

Run the immediate field at `10`, `20`, and `30 Hz` while retaining the user-exposed `5–60 Hz` control. Record:

- total frame GPU time;
- vegetation GPU time;
- vegetation main-thread time;
- immediate interaction compute time;
- uploaded and overflow interactor counts;
- interaction texture and buffer memory;
- visible spatial continuity during fast movement;
- visible temporal stepping at each tested update rate;
- recovery duration after actors leave;
- field stability while the domain recentres;
- draw calls, visible cluster count, and visible triangle count.

Persistent trails and ability stamps are excluded from INTERACT.1 stress validation and receive a separate INTERACT.2 budget.

---

## 17. Acceptance Gates

### 17.1 Visual acceptance

From the actual gameplay camera:

- dense regions cover at least 85% of their intended area;
- patches read as tall grass masses rather than isolated X cards;
- no broad card planes are obvious during ordinary camera movement;
- all visible regions show coherent wind;
- visible enemies deform the grass regardless of screen position;
- swept immediate displacement has no spatial gaps at the selected update rate;
- ordinary player/enemy displacement recovers without a persistent trail;
- response and recovery remain temporally smooth at 10 Hz;
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

### 17.3 Immediate interaction acceptance

With 32 ordinary and large immediate interactors inside the visible domain:

- no missing visible major-actor interaction;
- no spatial gaps in swept displacement at 10 Hz;
- no persistent player trail after the configured immediate recovery interval;
- no per-actor × per-cluster CPU loop;
- no per-frame hierarchy scan or managed allocation in the steady update path;
- immediate interaction update remains ≤ 0.30 ms GPU target;
- uploaded/overflow telemetry is correct at the configured capacity;
- response textures remain stable while the camera/anchor domain scrolls;
- no visible full-domain clearing or snapping.

### 17.4 Historical trample acceptance

With one active `VegetationTrampleDomain` and opt-in moving writers:

- ordinary interactors whose Trail Mode is Off never enter the historical candidate list;
- timed trails form continuous swept footprints at 5–15 Hz, remain fully held for their configured recovery delay, then follow the fixed asymmetric slow–fast–slow curve: approximately 15% restored at 50% recovery time, 90% restored at 90% time, and fully restored at completion;
- session-persistent trails remain until Reset Field or runtime-domain teardown;
- all recipes on one Ground share the same stored footprint while retaining independent visual response;
- no coverage mutation, vegetation rebuild, instance-buffer rewrite, per-cluster CPU loop, or shader-side writer loop;
- duplicate active trample domains for one Ground are rejected;
- uploaded/overflow telemetry and the 64-byte writer-buffer memory report are correct;
- the default 256² historical textures remain approximately 1 MiB per Ground.

With queued ability requests:

- a 360-degree radial sector covers the complete circle, while smaller arcs reject cells outside the authored cone half-angle;
- line requests use a width-controlled capsule with rounded endpoints;
- deterministic edge irregularity remains stable for fixed world position, scale, and seed;
- requests are consumed once on the next historical fixed step and do not repeat on later steps;
- stronger overlapping ability bend directions are retained instead of cancelling through vector averaging;
- session-persistent state dominates timed state, while timed restamping cannot shorten an existing delay or recovery duration;
- queue acceptance, rejection/replacement, pending count, uploaded count, and 96-byte record memory are reported accurately.

### 17.5 Production acceptance

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

**Implemented, source-audited, and user visually validated — rebuild timing and GPU profiling pending.**

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


---

## VEG-V1G — Mature Foundation Benchmark Suite

**Status:** IMPLEMENTED AND SOURCE-AUDITED — UNITY RUN PENDING

### Objective

Replace the obsolete V1B silhouette-profile timed matrix with one mature vegetation-foundation suite that the user runs once in Play Mode, waits for, and then copies as one consolidated report. The suite must compare all retained geometry candidates at the two accepted dense stress levels while separately measuring the incremental whole-frame cost of the accepted visual foundation features.

### Approved files

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`

### Read-only review evidence

- `Assets/AGENTS.md` was read completely. It requires a persistent pre-edit plan, exact scope, one comprehensive Inspector-triggered report where practical, source validation, and honest Unity-only pending checks.
- `VegetationBenchmark.cs > RunTimedComparisonSuite()` currently runs 18 cases: three geometry candidates × three historical silhouette profiles × densities 35/50. It mutates master width, taper, tip width, and width stabilization, so it no longer measures the user-accepted current visual configuration.
- `VegetationBenchmark.cs > MeasureSuiteWindow()` already supplies warm-up, repeated whole-frame CPU samples, genuine `FrameTimingManager` GPU samples when available, and an optional adjacent render-disabled baseline.
- `VegetationBenchmarkEditor.cs > OnInspectorGUI()` already provides one run button, one copy button, progress, and Play Mode gating. This interaction contract should be retained rather than duplicated.
- `VegetationClusterMeshBuilder.cs > VegetationBenchmarkGeometry` still defines exactly `OpaqueStrips`, `CrossedCards`, and `Hybrid`.
- The accepted dense stress levels remain 35 and 50 clusters/m².
- The mature visual foundation controls now include punctual edge accent, wind-normal tilt, explicit bend-side shading, and grass macro patch strengths. Each is material-only and can be disabled without changing placement or mesh structure.
- The supplied source tree contains no `.git` directory. Branch, HEAD, history, and working-tree comparisons are unavailable; the supplied V1F tree is the authoritative pre-edit baseline.

### Test matrix

The suite runs six timed cases:

```text
3 geometry candidates
× 2 densities: 35 / 50 clusters per m²
= 6 mature full-configuration cases
```

Every case preserves the user's complete accepted visual configuration. The suite does not attempt feature-cost ablation by setting material controls to zero. Source audit proved those zero values do not compile out the corresponding shader instructions: macro-patch arithmetic, wind-normal arithmetic, bend-shading arithmetic, and most edge-accent work still execute. Reporting their timing differences as feature costs would therefore be invalid.

### Execution contract

- One Inspector button starts the complete suite in Play Mode.
- One Copy button copies the last completed report.
- The suite uses forced full coverage for comparable stress.
- Each of the six geometry/density configurations is rebuilt once and measured with the complete accepted visual foundation.
- Every measurement retains configurable warm-up, measurement duration, pass count, alternating enabled/disabled ordering, and real GPU timing when `FrameTimingManager` supplies it.
- Optional screenshots produce one representative capture for each of the six cases.
- The suite saves the report under `Library/VegetationBenchmarkDiagnostics` and retains it in memory for clipboard copying.
- A `finally` path restores geometry, density, rendering, and the coverage override, then rebuilds the original resources. Visual controls are never mutated by the suite.

### Report requirements

The consolidated report must include:

- current resolution, render scale, graphics API, GPU, AA, VSync, target frame rate, and Editor/player context;
- suite settings and an explicit warning when the run is not 2560 × 1440;
- per-case resource readiness, rebuild duration, instance count, candidate/rejection counts, per-cluster and total geometry, draw count, 48-byte instance memory, deterministic hash, accepted visual-control values, and timing statistics;
- paired enabled/disabled CPU and GPU median deltas with noise estimates;
- confidence-aware ranking of the six geometry/density outcomes, with no winner claim inside noise;
- final restoration state and saved report path.

### Invariants and non-goals

- Do not change geometry generation, placement, instance layout, shader behavior, Weather, Ground, lighting formulas, scenes, prefabs, materials, URP assets, layers, or tags.
- Do not add a second suite or additional manual case steps.
- Do not force Game View resolution through unsupported Editor internals; report the actual resolution and warning instead.
- Do not describe whole-frame deltas as isolated shader timings.
- Do not select or remove a geometry candidate automatically.
- Do not add per-frame profiling work outside an active user-triggered suite.

### Performance model

- Normal gameplay cost is unchanged because all timing capture and profile switching execute only while the explicit suite coroutine is active.
- The suite performs six full rebuilds plus one restoration rebuild.
- At default settings, each timed case performs three enabled measurement windows and, when baseline interleaving is enabled, three disabled windows.
- The run duration is approximately:

```text
6 cases × passes × windows × (warm-up + measurement)
```

With the current defaults and paired baselines this is approximately `6 × 3 × 2 × 2.75 s = 99 s`, plus rebuilds and screenshots. Users may reduce passes or durations in the existing serialized controls.

### Design correction after shader audit

The initial V1G plan proposed five material-value ablations. That design was rejected before delivery. `SH_StylizedVegetationBenchmark.shader` performs the macro-patch, wind-normal, and bend-shading arithmetic without compile-time feature branches, and `VegetationLighting.hlsl` enters the eligible punctual edge path based on light class rather than `Stylized Edge Accent` strength. Setting those strengths to zero changes output but does not reliably remove the instruction cost. A valid cost-ablation suite would require retained benchmark shader variants or benchmark-only branches, which would add shader complexity and production variant cost. The user requested a practical foundation suite, so V1G measures the complete accepted configuration only and leaves per-feature shader isolation outside scope.

### Implementation sequence

| ID | File | Work | Status |
|---|---|---|---|
| V1G.0 | This document | Record objective, evidence, exact scope, matrix, invariants, performance model, and validation before implementation. | Complete |
| V1G.1 | `VegetationBenchmark.cs` | Replace silhouette profiles with six mature full-configuration geometry/density cases; add compact case evidence, rebuild timing, report saving, complete restoration, confidence-aware ranking, and V1G identifiers. | Complete; Unity run pending |
| V1G.2 | `VegetationBenchmarkEditor.cs` | Replace obsolete labels/help with one mature suite Run button, one Copy button, progress, saved-path status, and V1G identifiers. | Complete; Unity run pending |
| V1G.3 | Approved files and direct dependencies | Run scope, restoration, report, lexical, shader-audit, and preservation checks; record Unity compilation and runtime profiling as pending. | Complete; Unity run pending |

### Acceptance criteria

1. The Inspector exposes exactly one mature timed-suite Run action and one last-report Copy action.
2. A default run executes six mature full-configuration cases without manual intervention and performs six structural rebuilds before restoration.
3. The suite never mutates the accepted visual controls.
4. The report includes per-case structural evidence, paired timing, confidence-aware ranking, environment data, 1440p warning, and saved path.
5. The original benchmark state is restored even after a failed case or exception.
6. No ordinary rendering, placement, shader, Weather, Ground, geometry, or instance-layout behavior changes.

### Validation requirements

- Source-level validation: exact three-file scope, no added/deleted project files, old silhouette suite symbols absent, six-case matrix present, accepted visual controls never assigned by the suite, complete restoration fields, report save/copy contract, and balanced C# delimiters.
- Unity-only validation: C# compilation, one Play Mode run, correct progress, six completed cases, six representative screenshots when enabled, report saved under `Library`, clipboard copy, valid GPU samples where supported, and exact restoration of geometry/density/render state.


### VEG-V1G post-change consistency and compliance audit

#### Actual affected files

```text
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs
```

The actual three-file delta matches the approved scope exactly. No project file was added or deleted.

#### Implemented differences

- Replaced the obsolete 18-case silhouette-profile timed matrix with six mature full-configuration cases: `OpaqueStrips`, `CrossedCards`, and `Hybrid`, each at 35 and 50 clusters/m².
- Preserved the user's current accepted visual values for every case. The suite assigns no edge-accent, wind-normal, bend-shading, or grass-patch strength.
- Retained configurable warm-up, measurement duration, pass count, alternating rendered/disabled ordering, `FrameTimingManager` GPU sampling, and whole-frame CPU sampling.
- Added one structural rebuild per matrix case, measured rebuild duration, compact structural evidence, one optional screenshot per case, and a confidence-aware six-case ranking.
- Added a 2560 × 1440 target warning without changing Game View resolution through unsupported Editor APIs.
- Added automatic report saving to `Library/VegetationBenchmarkDiagnostics/Vegetation_V1G_Foundation_Benchmark_Suite_Report.txt`, retained the report in memory, exposed the saved path, and preserved one Copy action.
- Updated Inspector labels and help to one `Run Complete Foundation Benchmark Suite` action and one `Copy Last Foundation Suite Report` action.
- Added last rebuild duration to the ordinary comprehensive report.
- Restored geometry, density, render-enabled state, and the forced-coverage override in `finally`, followed by one restoration rebuild.

#### Shader-audit correction

The first V1G design draft proposed material-value feature ablations. It was rejected before delivery because source inspection proved those values do not remove the corresponding shader instructions:

- `SH_StylizedVegetationBenchmark.shader` always calculates macro-patch masks and colour composition.
- The shader always calculates wind-normal slope and bend-side inputs; zero response only multiplies the result down.
- Bend-side fragment arithmetic remains present at response zero.
- `VegetationLighting.hlsl` enters the punctual edge path based on light eligibility, not `Stylized Edge Accent` strength.

No benchmark shader variants or benchmark-only production branches were added. The final suite reports only valid complete-configuration comparisons.

#### Preservation evidence

The following direct dependencies are byte-identical to the V1F baseline:

- `VegetationClusterMeshBuilder.cs`
- `VegetationInstanceData.cs`
- `SH_StylizedVegetationBenchmark.shader`
- `VegetationWindResponse.hlsl`
- `VegetationLighting.hlsl`
- `WeatherWindDomain.cs`
- `GeneratedGround.cs`

Normal Play Mode rendering remains in the existing `LateUpdate > SubmitIndirectRender` path. Profiling work executes only inside the explicit user-triggered suite coroutine.

#### Source validation evidence

`VEG-V1G_Source_Validation.txt` records **71/71 passed checks**, including:

- exact approved scope and no added/deleted files;
- absence of obsolete silhouette-suite and rejected feature-ablation symbols;
- exact three-geometry × two-density matrix;
- six rebuilds plus restoration and no visual-control assignment by the suite;
- report environment, resolution warning, timing, ranking, saving, path, and clipboard contracts;
- rebuild-duration recording on all early-return and normal paths;
- Editor Play Mode gating, progress, Run/Copy actions, and saved-path display;
- byte-identical shader, geometry, instance, Ground, and Weather dependencies;
- source evidence for rejecting invalid zero-value feature-cost ablations;
- balanced C# delimiters, clean lexical state, no conflict markers, and no obsolete instance-ID API.

No Unity compiler or runtime is available in this environment. C# compilation, actual coroutine execution, screenshot generation, file saving, `FrameTimingManager` GPU sample availability, clipboard copying, and restoration behavior remain pending in Unity 6000.5.0f1.

#### Performance reconciliation

- Ordinary gameplay cost is unchanged.
- The suite performs six matrix rebuilds plus one restoration rebuild only after explicit user action.
- With current defaults and paired baselines, the timed windows total approximately 99 seconds; rebuilds and screenshots add additional time.
- Whole-frame deltas remain comparative estimates. A standalone development build and Unity Profiler/Frame Debugger remain authoritative for final production selection.

#### Unity validation

1. Enter Play Mode and run `Run Complete Foundation Benchmark Suite`; confirm progress reaches six of six without Console errors.
2. Confirm the report records the actual resolution and either passes 2560 × 1440 or emits the target-resolution warning.
3. Confirm six cases and six rebuilds are reported, with screenshots present when enabled and valid GPU samples or an explicit unavailable result.
4. Confirm geometry, density, and render-enabled state return to their pre-run values and visual controls remain unchanged.
5. Press `Copy Last Foundation Suite Report` and paste the complete report; also confirm the saved path under `Library/VegetationBenchmarkDiagnostics` exists.

---

## VEG-V1H — Ground-Owned Vegetation Placement Domain

**Status:** IMPLEMENTED AND SOURCE-AUDITED — UNITY VALIDATION PENDING

### Objective

Correct the authored vegetation placement domain so a `VegetationBenchmark` using an assigned `GeneratedGround` can generate visible grass across the complete Ground patch rather than only inside the legacy manually positioned `fieldSize` rectangle. Preserve the fixed `40 × 30 m` benchmark domain for the explicit V1G forced-full-coverage performance suite.

### User-observed defect and acceptance

- The Ground vegetation-coverage overlay spans the complete map chunk and painting outside the visible grass rectangle changes red coverage points to green.
- Visible grass remains restricted to the current `VegetationBenchmark.fieldSize` rectangle.
- The accepted fix must make ordinary Ground-integrated placement use the complete assigned Ground domain and derive render bounds from generated instances.
- Full-patch candidate generation cost is accepted. The user states production chunks will not contain 100% grass coverage and does not consider the additional rejected dirty-time candidates a blocker.
- Hierarchy, vegetation-child creation, coverage ownership, and multiple grass-family infrastructure are explicitly deferred to the next design discussion. This patch must not pre-emptively implement that architecture.

### Approved file scope

```text
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Docs/Stylized_Vegetation_Architecture.md
Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs
```

No Ground runtime/editor source, scene, prefab, shader, material, Weather, layer, tag, or serialized project asset is approved for modification.

### Read-only review evidence

- `Assets/AGENTS.md` was read completely. It requires the canonical plan to be the first write, exact scope, complete post-change reread, evidence, and honest Unity-only pending checks.
- The supplied/reconstructed tree has no `.git` directory. Branch, HEAD, status, history, and comparison with repository commits are unavailable. The current V1G tree is the authoritative pre-edit baseline.
- Pre-edit SHA-256:
  - `VegetationBenchmark.cs`: `6da2bd1e5239cb8a35da51d3058aafd6e8d58a71bf4a536dc0eaf8e1fc15c3cd`
  - `VegetationBenchmarkEditor.cs`: `1e889e85a2e58175df9802e8fe66ed11546c2f57de43dcbc64885307e3031ccb`
  - this document: `51310f54d185c9b9461baab1466a72ab8882d50c4325edaafb9f7631bd480e5b`
  - `Stylized_Vegetation_Architecture.md`: `31f49a64de257c121bfb8884fff84e90e4095074fb9a6023a5573277139e6ec6`
- `VegetationBenchmark.cs > BuildInstances()` currently calculates candidate count and positions exclusively from `fieldSize`, transforms them through the vegetation object's transform, and only afterward queries Ground coverage and height. This proves painted coverage outside `fieldSize` can never receive candidates.
- `VegetationBenchmark.cs > RebuildBenchmark()` currently derives `localBounds` from `fieldSize`, so the indirect-draw culling domain is tied to the same obsolete rectangle.
- `GeneratedGround.cs > PatchSize` exposes the resolved local Ground domain size. `TrySampleVegetationCoverage()` and `TrySampleBaseSurface()` transform world points into Ground-local space and support the project's expected translated, yaw-rotated, and scaled Ground usage. Arbitrary pitched/rolled Ground is outside the current height-sampling contract because `TrySampleBaseSurface()` returns world Y height rather than a complete projected world point.
- `GeneratedGround.cs > TryResolveVegetationCoverageDomain()` uses the generated base-surface half size when available and otherwise falls back to `PatchSize / 2`.
- `GeneratedGroundEditor.cs > RebuildVegetationBenchmarksUsingGround()` already rebuilds every benchmark whose `CoverageGround` matches the edited Ground. No Ground-side source change is needed for coverage or regeneration propagation.
- `VegetationBenchmark.cs > RunTimedComparisonSuite()` sets `suiteForceFullCoverage = true` for every V1G case. This existing private state can preserve the fixed `fieldSize` stress domain while ordinary authored rendering uses Ground ownership.
- `VegetationInstanceData` stores local instance position in `PositionYaw.xyz`; no layout change is required.

### Placement-domain contract

The authoritative domain is selected as follows:

```text
if useGroundCoverage && coverageGround != null && !suiteForceFullCoverage:
    domain owner = GeneratedGround
    local XZ extent = coverageGround.PatchSize square
    candidate world position = coverageGround.transform.TransformPoint(groundLocalXZ)
else:
    domain owner = VegetationBenchmark
    local XZ extent = fieldSize
    candidate world position = transform.TransformPoint(benchmarkLocalXZ)
```

The Ground-owned candidate count uses the actual transformed world area:

```text
world area = patchSize² × |cross(groundTransform.rightVector, groundTransform.forwardVector)|
```

where the transformed unit basis vectors include Ground scale. This preserves `densityPerSquareMetre` under translated, rotated, and scaled Ground transforms. Candidate sampling remains uniform in Ground-local XZ.

After Ground height is sampled, every accepted world position is converted into the vegetation object's local coordinates before packing the instance record. The vegetation object does not need to share the Ground transform in this patch.

### Bounds contract

- Non-empty placement: derive local bounds from every accepted instance base and its maximum local blade top, then expand horizontally for cluster footprint, width stabilization, and maximum Weather bend and vertically by the existing safety margin.
- Empty placement: use the resolved placement domain transformed into vegetation-local space as a conservative fallback, then apply the same wind/height expansion.
- Render submission continues to transform the resulting local bounds through `transform.localToWorldMatrix` exactly once.

### Benchmark-suite preservation

- V1G timed suite cases continue setting `suiteForceFullCoverage = true`.
- Forced suite cases therefore use the legacy `fieldSize`, currently `40 × 30 m`, for candidate extent and count regardless of Ground size or authored coverage. When a Ground remains assigned, the existing suite behavior still samples its base-surface height and may reject candidates outside that Ground; this patch does not redesign V1G height ownership.
- Suite restoration clears the override and rebuilds the ordinary authored Ground-owned domain.
- No timing matrix, pass count, visual setting, shader, or report-save workflow changes are permitted beyond accurate placement-domain reporting.

### Invariants and non-goals

- No change to Ground coverage storage or painting.
- No automatic child-object creation or hierarchy migration.
- No support for multiple vegetation coverage channels or grass families yet.
- No scene/prefab edits and no automatic transform reassignment.
- No change to deterministic placement RNG ordering inside a selected domain.
- No instance stride, shader, geometry, Weather, lighting, patching, or draw-call change.
- No per-frame placement/bounds recomputation.
- `fieldSize` remains serialized because it is still the fallback and V1G benchmark domain; Inspector text must state when it is ignored by ordinary Ground-integrated placement.

### File-by-file implementation sequence

| ID | File | Required work | Status |
| --- | --- | --- | --- |
| V1H.0 | This document | Record evidence, exact scope, domain/bounds contracts, invariants, performance, and validation before source edits. | Complete |
| V1H.1 | `VegetationBenchmark.cs` | Add a resolved placement-domain helper; generate Ground-owned candidates during ordinary Ground integration; preserve forced suite domain; derive candidate count from resolved world area; calculate instance-derived local bounds with empty-domain fallback; expose/report the resolved domain. | Complete; Unity validation pending |
| V1H.2 | `VegetationBenchmarkEditor.cs` | Clarify the active domain in status/help text and identify `fieldSize` as fallback/benchmark-only while Ground ownership is active. | Complete; Unity validation pending |
| V1H.3 | `Stylized_Vegetation_Architecture.md` | Record Ground-owned authored placement and explicitly defer multi-family coverage/hierarchy architecture. | Complete |
| V1H.4 | Approved files and read-only dependencies | Run exact-scope, API/signature, deterministic-domain, suite-preservation, bounds, lexical/static, and post-change reread checks. Record Unity compilation and visual validation as pending. | Source audit complete; Unity validation pending |

### Performance model

- Normal-frame CPU/GPU work is unchanged. Placement and bounds remain rebuild-only.
- No new buffers, textures, samples, draw calls, passes, per-frame loops, or instance bytes are introduced.
- Candidate count scales with the resolved world area. For an unscaled `40 × 40 m` Ground at `50 clusters/m²`, the generator tests `80,000` candidates instead of the legacy `60,000` candidates from `40 × 30 m`.
- Coverage-empty regions are still evaluated and rejected during the monolithic rebuild. The user explicitly accepts this dirty-time overhead for the current foundation; chunking/coverage-aware generation remains later infrastructure work.
- Bounds calculation adds one linear pass over accepted instances during rebuild. `BuildInstances()` already performs one linear generation pass; this does not affect normal-frame cost.

### Risks and mitigations

- **Ground scale:** use transformed X/Z basis cross-product for world area; do not assume scale one.
- **Vegetation/Ground transform mismatch:** generate in Ground local space, sample in world space, and pack in vegetation local space.
- **Empty authored coverage:** calculate conservative fallback bounds from the resolved domain; the indirect draw still submits zero instances.
- **Suite regression:** select Ground ownership only when `suiteForceFullCoverage` is false and verify six V1G cases remain fixed-domain.
- **Deterministic hash change:** Ground-owned placement intentionally changes positions and candidate count relative to the defective rectangle. Repeated rebuilds with unchanged Ground/domain/settings must remain deterministic.

### Validation requirements

Source validation:

1. Exact four-file scope and no added/deleted project files.
2. Ground-owned ordinary domain and fixed forced-suite domain are mutually explicit.
3. Ground world-area calculation includes transformed X/Z basis vectors.
4. Accepted positions are packed in vegetation-local coordinates and Ground coverage/height are sampled in world space.
5. Local bounds are generated from accepted instances, with a resolved-domain fallback for zero instances.
6. V1G suite restoration, one-draw/48-byte contracts, shaders, Ground, Weather, geometry, and instance layout remain unchanged.

Unity-only validation:

- C# compilation in Unity 6000.5.0f1.
- Paint coverage outside the former rectangle and confirm visible grass appears across the complete Ground domain.
- Move the vegetation GameObject independently, rebuild the vegetation benchmark, and confirm Ground-owned placement realigns with the assigned Ground. Automatic hierarchy/transform lifecycle handling is deferred.
- Run the V1G suite and confirm its cases still report/use `40 × 30 m` forced benchmark placement, then restore to Ground ownership.


### VEG-V1H post-change consistency and compliance audit

#### Actual affected files

```text
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Docs/Stylized_Vegetation_Architecture.md
Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs
```

The final four-file delta matches the approved scope exactly. No file was added or deleted.

#### Implemented differences

- Added a single resolved placement-domain contract to `VegetationBenchmark`.
- Ordinary Ground-integrated placement now resolves a square domain from `coverageGround.PatchSize`, generates candidate XZ positions in Ground-local coordinates, transforms them to world space for coverage and height sampling, and converts accepted positions into vegetation-local coordinates for the existing instance record.
- Ground-owned candidate count now uses transformed world area from the cross product of Ground-local X and Z basis vectors. A unit-scale `40 × 40 m` Ground therefore generates `1,600 × density` candidates.
- Fallback and V1G forced-suite placement preserve the serialized `fieldSize` path and its existing random-call ordering.
- Replaced `fieldSize`-derived indirect bounds with bounds calculated from accepted instance bases and maximum blade tops, expanded by the existing maximum horizontal bend/footprint and vertical safety margin. Zero-instance builds use a conservative transformed-domain fallback.
- Added placement-domain ownership, resolved extent/area, and configured fallback field information to the comprehensive report and per-case V1G evidence.
- Added Inspector help/status that identifies the active placement owner and states when `fieldSize` is ignored.
- Recorded Ground-owned authored placement in the stylized architecture while explicitly deferring automatic child creation, vegetation-owned painting, and multi-family coverage representation.

#### Preserved behavior and contracts

The following files are byte-identical to the V1G baseline:

- `GeneratedGround.cs`
- `GeneratedGroundEditor.cs`
- `VegetationInstanceData.cs`
- `VegetationClusterMeshBuilder.cs`
- `SH_StylizedVegetationBenchmark.shader`
- `VegetationWindResponse.hlsl`
- `VegetationLighting.hlsl`

The 48-byte instance stride, one indirect draw, shaders, lighting, wind, macro patches, coverage storage, coverage painting, Ground height API, geometry, V1G timings, screenshots, report saving, and suite restoration remain unchanged. The timed suite still sets `suiteForceFullCoverage` before each case and clears it before the restoration rebuild.

#### Source validation evidence

A local source-validation script recorded **38/38 passed checks**, including:

- exact four-file scope and no added/deleted project files;
- mutually exclusive Ground-owned ordinary placement and forced-suite/fallback placement;
- Ground patch-size ownership and transformed-basis world-area calculation;
- world-space Ground coverage/height sampling and vegetation-local instance packing;
- fixed suite override assignment and restoration;
- instance-derived bounds and zero-instance transformed-domain fallback;
- removal of the obsolete `fieldSize` culling-bounds formula;
- report and Inspector ownership evidence;
- byte-identical Ground, instance-layout, geometry, shader, wind, and lighting dependencies;
- balanced C# delimiters, clean lexical state, and balanced preprocessor directives.

No C# compiler or Unity runtime is available in this environment. Unity 6000.5.0f1 compilation, actual Ground painting across the former boundary, render-culling validation, and V1G suite execution remain pending.

#### Performance reconciliation

- Normal-frame work is unchanged.
- No shader, buffer, memory-stride, draw-call, texture, sampling, or compute cost was added.
- Rebuild work now scales with the complete resolved Ground world area, as required to populate the previously unreachable region.
- Bounds add one rebuild-only linear pass over accepted instances.
- Coverage-empty candidates remain dirty-time rejection work in the current monolithic foundation. The user explicitly accepted this cost; coverage-aware chunk generation is deferred to the upcoming infrastructure work.

#### Unity validation

1. Confirm Unity imports and compiles without vegetation errors.
2. Paint coverage outside the former red rectangle and confirm visible grass appears across the complete assigned Ground patch.
3. Confirm the comprehensive report states `GeneratedGround-owned authored domain`, the Ground's resolved size/area, and instance-derived local bounds.
4. Move either object, rebuild once, and confirm placement aligns to the assigned Ground rather than the old vegetation rectangle.
5. Run the V1G suite and confirm each case reports the configured `40 × 30 m` forced benchmark placement domain, then confirm restoration returns to Ground ownership.

## VEG-V2-INFRA.1A — Production Vegetation Layer Extraction

### Status

**Corrected full patch prepared and source-validated on 2026-07-21; Unity validation pending.** This is the first migration patch. It establishes the production component boundary and layer-owned coverage data without changing the current scene hierarchy or deleting legacy Ground coverage. The corrected revision makes the `GeneratedGround` ancestor authoritative for every production `VegetationLayer`; no manual Ground assignment is exposed or accepted.

### Core architecture decision

The production and diagnostic responsibilities are separate:

```text
Scene
├── Systems
│   ├── Weather                         [WeatherWindDomain]
│   └── Diagnostics
│       └── Vegetation Benchmark        [future VegetationBenchmarkRunner]
│
└── GeneratedGround                     [GeneratedGround]
    └── Vegetation                      [future GroundVegetation]
        ├── Grass_Default               [VegetationLayer]
        ├── Grass_Tall_Dark             [VegetationLayer]
        └── Grass_Short_Bright          [VegetationLayer]
```

One `VegetationLayer` component lives directly on each named recipe GameObject. A layer is one independently configured recipe plus one independently paintable coverage field. It is not restricted to a distinct species or family. Multiple layers may use the same geometry/family and differ only in height, width, density, colour, stiffness, wind response, lighting, macro patches, or other recipe settings.

### Hierarchy-owned Ground contract

A production `VegetationLayer` resolves its surface owner exclusively from the nearest `GeneratedGround` ancestor through `GetComponentInParent<GeneratedGround>(true)`, including inactive hierarchy owners. This supports both a direct child and the approved `GeneratedGround/Vegetation/<recipe>` nesting. The Ground reference is not an ordinary serialized authoring choice on a layer and must not appear as an editable Inspector field.

Reparenting a layer must refresh the resolved ancestor and rebuild. Reparenting outside every `GeneratedGround` must release generated resources, report a clear hierarchy error, and render no fallback field. The manual serialized Ground reference remains only on the transitional `VegetationBenchmark`, because that existing scene component predates the production hierarchy and must preserve its current serialized setup until migration.

There will ultimately be exactly one scene-level vegetation benchmark runner. Production layers do not own benchmark matrices, screenshots, timing coroutines, or candidate rankings. `WeatherWindDomain` remains scene-owned and vegetation remains only a consumer. The Weather hierarchy may be expanded into child modules later; this patch does not select or implement that future split.

### Objective

Extract the current production placement, GPU-resource, rendering, material, wind-consumption, and rebuild lifecycle from `VegetationBenchmark` into a shared production renderer base. Add a functional `VegetationLayer` component with its own serialized R8-equivalent byte coverage field. Preserve the existing `VegetationBenchmark` scene component and V1G suite unchanged as a compatibility/diagnostic component until explicit scene migration.

### Approved scope

Create:

- `Assets/Game/Procedural/Vegetation/VegetationRendererBase.cs`
- `Assets/Game/Procedural/Vegetation/VegetationRendererBase.cs.meta`
- `Assets/Game/Procedural/Vegetation/VegetationLayer.cs`
- `Assets/Game/Procedural/Vegetation/VegetationLayer.cs.meta`
- `Assets/Game/Procedural/Vegetation/VegetationCoverageField.cs`
- `Assets/Game/Procedural/Vegetation/VegetationCoverageField.cs.meta`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationLayerEditor.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationLayerEditor.cs.meta`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationRendererEditorPreview.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationRendererEditorPreview.cs.meta`

Modify:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Procedural/Ground/GeneratedGround.cs`

No scene, prefab, material, shader, geometry-builder, instance-layout, Weather, layer, tag, or URP asset changes are authorized.

### Read-only review evidence

- `Assets/AGENTS.md` was read completely. It requires a documented plan before implementation, exact scope, final caller/consumer review, and explicit pending Unity validation.
- The first prepared INFRA.1A revision exposed the inherited serialized `coverageGround` field in `VegetationLayerEditor` and offered `AssignSurfaceGround()`. That contradicted the approved child-owned hierarchy. The corrected plan removes that public assignment path, hides the inherited Ground/use-surface controls for layers, synchronizes from `GetComponentInParent<GeneratedGround>(true)` before build, rebuilds after parent changes, and blocks rendering when no ancestor Ground exists.
- Git metadata is absent from the supplied archive. Branch, `HEAD`, working-tree status, and history are unavailable; no replacement clone is permitted or used.
- `VegetationBenchmark.cs` currently combines production state and behavior with the V1G timed suite. Production fields occupy the domain, Ground, geometry, variation, patch, lighting, runtime, GPU-resource, placement, render, and rebuild sections. Timed-suite state and methods are interleaved in the same component.
- `VegetationBenchmarkEditor.cs` currently owns both production Inspector behavior and benchmark-suite controls. Its Edit-Mode SRP callback enumerates only `VegetationBenchmark.ActiveBenchmarks`, so production layers require a renderer-base registry.
- `GeneratedGround.cs` currently exposes grass-specific coverage storage and mapping methods. Patch 1 retains the legacy field but adds generic Ground surface-domain helpers required by layer-owned masks.
- `VegetationInstanceData.cs` is a 48-byte `3 × Vector4` record. This patch does not change it.
- `VegetationClusterMeshBuilder.cs`, the vegetation shaders/includes, Weather field, and shared Ground height sampler are direct production dependencies and must remain behaviorally unchanged.
- Current accepted source is VEG-V1H: the transitional `VegetationBenchmark` uses its assigned complete Ground domain, while the V1G suite forces the fixed `40 × 30 m` fallback domain. Production `VegetationLayer` ownership is stricter: its nearest `GeneratedGround` ancestor is authoritative.

### Production renderer split

`VegetationRendererBase : MonoBehaviour` owns:

- fallback domain, density, seed, Ground surface reference, and placement threshold;
- geometry and silhouette configuration;
- instance variation and macro patch composition;
- body lighting, wind-normal, bend-side shading, and punctual edge controls;
- target camera, rendering enablement, and Scene preview enablement;
- generated mesh, runtime material, instance/indirect buffers, bounds, deterministic hash, statistics, and error state;
- Ground-owned domain resolution, surface sampling, instance generation, bounds, resource lifecycle, material-only refresh, runtime draw submission, Edit-Mode synchronization, and configuration hashes.

It does not own:

- benchmark warm-up/measurement settings;
- timed-suite coroutine or progress;
- screenshots, report files, ranking, or comparison matrices.

`VegetationBenchmark` derives from `VegetationRendererBase`, retains its existing serialized V1G fields and diagnostic methods, and exposes compatibility names used by its current Inspector. Diagnostic-only virtual hooks force the suite fallback domain and suppress editor rendering while the suite owns the component.

`VegetationLayer` derives from `VegetationRendererBase`, has no benchmark state, and owns one `VegetationCoverageField`.

### Layer coverage contract

`VegetationCoverageField` stores:

```text
resolution: 128 by default, clamped to 8–512
pixels: one byte per texel
revision: incremented after mutation
initialized: explicit empty/full/import state
```

Raw storage at 128² is 16,384 bytes per layer. It is serialized CPU data only. It creates no runtime texture and is sampled only during rebuild-time candidate acceptance.

The field supports initialization, fill, average calculation, bilinear world-space sampling through the hierarchy-resolved Ground domain, editor painting, texel-world-position queries, and later legacy import. New `VegetationLayer` components initialize empty through `Reset`; no automatic migration runs from `OnValidate`.

### Generic Ground surface contract

Add non-grass-specific wrappers without removing legacy APIs:

- `SurfaceGeometryRevision`
- `TryGetSurfaceDomain(out halfSize, out domainSize)`
- `TryWorldToSurfaceNormalizedXZ(worldPosition, out normalizedX, out normalizedZ)`
- editor-only `TryGetSurfaceWorldPosition(normalizedX, normalizedZ, out worldPosition)`
- editor-only `TryRaycastGeneratedSurface(ray, out worldPosition)`

The existing vegetation coverage storage and methods remain untouched as rollback/migration data. The generic helpers reuse the same current Ground domain and generated-surface raycast implementation.

### Editor-preview contract

A shared `VegetationRendererEditorPreview` SRP callback enumerates `VegetationRendererBase.ActiveRenderers`. It renders both the legacy benchmark and new production layers in Edit Mode. It excludes Preview and Reflection cameras and respects each renderer's rendering, Scene preview, target-camera, and diagnostic-ownership state.

### File-by-file sequence

1. Update both canonical architecture documents with this ownership model, staged migration, invariants, and performance model.
2. Add generic Ground domain/raycast wrappers while retaining all legacy coverage behavior.
3. Add `VegetationCoverageField` and validate its mapping/storage independently.
4. Extract production state and behavior into `VegetationRendererBase`.
5. Convert `VegetationBenchmark` into a diagnostic subclass while preserving current serialized field names, public Inspector contract, V1G suite, and current scene behavior.
6. Add `VegetationLayer` and its Inspector with layer-owned coverage controls sufficient to create and test a layer manually. Resolve the Ground exclusively from the hierarchy, hide the transitional manual Ground/use-surface fields, rebuild on parent changes, and render nothing when no Ground ancestor exists. Full Scene painting migration is deferred to INFRA.1B.
7. Replace the benchmark-only Edit-Mode callback with the shared renderer callback.
8. Perform exact scope, serialization-name, inheritance, API, lifecycle, static/lexical, and dependency-preservation audits.

### Invariants and non-goals

- Existing `VegetationBenchmark` serialized production values must retain their names and meaning after moving into the base class.
- Existing scene hierarchy and active component behavior must not change in this patch.
- V1G suite matrix, fixed forced domain, report path, screenshot behavior, state restoration, and current public buttons remain available.
- VEG-V1H Ground-owned authored placement remains the ordinary default. For production layers, hierarchy ownership is mandatory and no local fallback domain is permitted when the ancestor Ground is missing.
- One Weather field sample, two detail sine calls, 48-byte instance stride, one indirect draw per renderer, shader behavior, and geometry output remain unchanged.
- No layer hierarchy creation, migration, coverage copy, Ground painting removal, benchmark-runner object, or Weather object migration occurs in INFRA.1A.
- No per-frame mask sampling, layer registry scan, or Ground-driven full-field rebuild is added.

### Performance model

- A layer-owned 128² mask adds 16 KiB serialized CPU data.
- No mask GPU texture, vertex sample, fragment sample, buffer, draw call, or compute dispatch is added.
- Each enabled renderer retains one indirect draw and one 48-byte-per-instance buffer.
- Extracting shared code changes ownership only; it must not add active-frame work to the legacy benchmark.
- Coverage evaluation remains rebuild-only O(candidate count).

### Acceptance criteria

1. Existing `VegetationBenchmark` compiles and renders equivalently with its V1G suite operational.
2. A manually added `VegetationLayer` beneath a `GeneratedGround` automatically resolves that ancestor, exposes it read-only, initializes/fills its own mask, and renders accepted instances from that mask without manual assignment.
3. `VegetationLayer` contains no suite fields or suite actions.
4. Both renderer types use one shared production implementation and one shared Edit-Mode camera callback.
5. Legacy Ground coverage remains intact and current benchmark fallback behavior remains unchanged.
6. No approved dependency changes and the 48-byte instance contract remains intact.

### Required validation

- Parse/compile every changed C# file with the strongest available local compiler or parser.
- Verify inherited serialized production field names remain exactly present once and suite fields remain only on `VegetationBenchmark`.
- Verify all `VegetationBenchmarkEditor` calls resolve after extraction.
- Verify `VegetationLayer` coverage revision participates in its rebuild hash, zero/uninitialized mask behavior is explicit, manual Ground assignment is absent, hierarchy resolution is authoritative, parent changes rebuild, and a missing ancestor produces no rendered fallback.
- Verify exact added/modified file scope and no serialized Unity assets changed.
- Mark Unity 6000.5 compilation, Inspector serialization preservation, actual Edit-Mode rendering, V1G execution, and manual layer rendering pending when Unity is unavailable.


### VEG-V2-INFRA.1A post-change consistency and compliance audit

#### Actual affected files

Created:

- `Assets/Game/Procedural/Vegetation/VegetationRendererBase.cs` and `.meta`
- `Assets/Game/Procedural/Vegetation/VegetationLayer.cs` and `.meta`
- `Assets/Game/Procedural/Vegetation/VegetationCoverageField.cs` and `.meta`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationLayerEditor.cs` and `.meta`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationRendererEditorPreview.cs` and `.meta`

Modified:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Procedural/Ground/GeneratedGround.cs`

No files were deleted. No scene, prefab, material, shader, URP asset, layer, or tag was modified.

#### Implemented ownership split

- `VegetationRendererBase` now owns the full production renderer lifecycle: serialized recipe and lighting settings, Ground domain and height integration, deterministic placement, macro-patch baking, mesh/material/buffer creation, bounds, indirect submission, Edit-Mode synchronization, material-only refresh, and rebuild/material hashes.
- `VegetationBenchmark` derives from the shared base and now contains only its timed-suite settings/state, comparison/report methods, compatibility API, and diagnostic hooks. The legacy public names used by current callers and the Inspector remain available, including `RebuildBenchmark`, `SetRenderBenchmark`, and `ActiveBenchmarks`.
- `VegetationLayer` derives from the same base, contains no benchmark suite state, and owns one hidden serialized `VegetationCoverageField`. Its surface Ground is resolved exclusively from the nearest hierarchy ancestor; the inherited transitional Ground/use-surface properties are hidden from the layer Inspector and cannot override that ownership.
- `VegetationLayer.OnEnable`, `OnValidate`, explicit rebuild validation, and direct-parent changes synchronize the nearest ancestor before production work. `AssignSurfaceGround()` no longer exists. A missing ancestor fails the shared rebuild before mesh/material/buffer creation, leaving resources released and reporting the hierarchy error instead of generating the fallback rectangle.
- `VegetationRendererEditorPreview` replaces the benchmark-only SRP callback and renders every active `VegetationRendererBase` in Edit Mode while respecting target-camera, preview, rendering, and diagnostic-ownership rules.

#### Layer coverage result

- Default resolution: `128 × 128`.
- Raw serialized storage: `16,384` bytes per layer.
- Uninitialized layer coverage is treated explicitly as empty by the layer sampling hook; it does not inherit the legacy Ground full-coverage fallback.
- `Initialize Empty`, `Initialize Full`, `Clear Empty`, `Fill Full`, and resolution-preserving resize are available in `VegetationLayerEditor`.
- Coverage mapping uses the hierarchy-resolved Ground's generic normalized XZ surface domain. Coverage remains CPU-side rebuild data and creates no texture or normal-frame sample.
- Full Scene painting and legacy-mask import UI remain deliberately deferred to `VEG-V2-INFRA.1B`.

#### Ground API result

`GeneratedGround` now exposes generic surface helpers while retaining every legacy vegetation-coverage property and method:

- `SurfaceGeometryRevision`
- `TryGetSurfaceDomain`
- `TryWorldToSurfaceNormalizedXZ`
- editor-only `TryGetSurfaceWorldPosition`
- editor-only `TryRaycastGeneratedSurface`

The generic wrappers reuse the existing Ground domain, base-surface sampling, and generated-mesh raycast path. Legacy Ground coverage storage and painting remain byte-preserved except for these additive APIs.

#### Serialization and compatibility evidence

The source validator compared the pre-extraction `VegetationBenchmark` against the final base plus subclass:

- all 50 pre-existing serialized field names are still present exactly once;
- all 50 field types and initializer values are unchanged;
- the 45 production fields reside on `VegetationRendererBase`;
- the five V1G suite fields reside only on `VegetationBenchmark`;
- the existing benchmark public API names are retained;
- the V1G timed-suite, structural-matrix, environment-report, screenshot, save, and comprehensive-report method bodies are byte-identical to the V1H source;
- unchanged production methods including indirect submission, material publication, macro-patch evaluation, value noise, and bounds transformation are byte-identical;
- resource release differs only by `private` to `protected` accessibility so the shared lifecycle can own it.

Moving serialized fields into an inherited Unity component is source-compatible by name, but actual existing-scene value retention remains a required Unity validation because Unity serialization cannot be executed in this environment.

#### Preserved runtime contracts

The following direct dependencies are byte-identical to V1H:

- `VegetationInstanceData.cs`
- `VegetationClusterMeshBuilder.cs`
- `SH_StylizedVegetationBenchmark.shader`
- `VegetationWindResponse.hlsl`
- `VegetationLighting.hlsl`
- `WeatherWindDomain.cs`

Therefore the 48-byte instance stride, geometry output, shader variants, one Weather sample per vertex, wind/detail equations, lighting equations, one indirect draw per renderer, and Weather ownership are unchanged by source.

The base introduces no new `Update`, `LateUpdate`, or `FixedUpdate` on `VegetationLayer`. Layer coverage creates no GPU resource. Normal-frame work for the current benchmark is unchanged except for dispatch through the shared base class.

#### Source validation evidence

`VEG-V2-INFRA.1A_Source_Validation.txt` records **73/73 passed checks**, including:

- exact added/modified scope and no deletions;
- Tree-sitter C# parse success for every changed source file;
- balanced preprocessor directives;
- serialized field name/type/default preservation;
- benchmark public API preservation;
- diagnostic method preservation;
- production method preservation where no ownership hook was required;
- shared inheritance and Edit-Mode registry;
- explicit empty layer coverage and revision hashing;
- no public/manual layer Ground assignment, hierarchy ancestor resolution, synchronization before enable/validation/build, parent-change rebuild, hidden fallback/manual fields, read-only resolved-owner status, and missing-owner build rejection;
- generic Ground API presence and legacy Ground API retention;
- byte-identical geometry, instance, shader, wind, lighting, and Weather dependencies;
- no scene/prefab/material/asset change.

No Unity or standalone C# compiler with Unity assemblies is available. Tree-sitter proves syntactic parse only; it does not prove Unity compilation, serialization, Inspector behavior, GPU resource creation, or rendering.

#### Performance reconciliation

- Current `VegetationBenchmark` retains the same mesh, buffer, draw, vertex, fragment, Weather, and lighting work.
- A `VegetationLayer` adds one 16 KiB default serialized mask plus the same resources proportional to the grass it intentionally renders.
- Coverage sampling remains one CPU bilinear sample per placement candidate during rebuild only.
- No runtime mask texture, additional vertex/fragment sample, compute dispatch, global scan, or per-frame rebuild was added.

#### Pending Unity validation

1. Confirm Unity 6000.5 compiles all changed C# files and imports the existing vegetation shader without errors.
2. Confirm the existing `VegetationBenchmark` retains all Inspector values and produces the same visible grass, instance count, deterministic hash, and comprehensive report after the inherited-field extraction.
3. Run the current V1G suite once and confirm its six cases, fixed forced domain, screenshots, report save, and restoration remain functional.
4. Add one `VegetationLayer` beneath the current Ground (directly or under a `Vegetation` child), confirm the Inspector resolves that ancestor automatically with no editable Ground field, use `Initialize Full`, and confirm it renders from the complete Ground domain; reparent it outside the Ground and confirm it releases its grass and reports the hierarchy error.
5. Confirm Scene and Game views render both component types in Edit Mode through the shared callback.
6. Confirm changing one layer's coverage resolution or fill changes only that layer's rebuild hash and resources.

## VEG-V2-INFRA.1A-SCENE.1 — Grass_Tall recipe parity with VegetationTest

### Status

**Implemented and source-audited on 2026-07-21; Unity scene deserialization, rebuild, visual comparison, and profiling pending.**

### Objective and acceptance criteria

Copy the current non-wind grass recipe from the disabled `VegetationTest` object's `VegetationBenchmark` component to `GroundBLockout/Vegetation/Grass_Tall`'s `VegetationLayer` in `VisualFrameworkDemo`. Acceptance requires the two components to have identical shared placement-density, geometry, silhouette, size/variation, macro-patch, colour, and non-wind lighting values after the edit. `Grass_Tall` must retain its layer-owned coverage payload, hierarchy-owned Ground relationship, runtime camera/rendering/preview state, and its distinct wind-normal and wind-bend shading values. No other scene object or serialized value may change.

### Approved scope

Modify only:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity`

No C#, shader, include, prefab, material, coverage migration, Weather, Ground, layer, tag, hierarchy, component, transform, or project-setting change is approved.

### Read-only review evidence

- `Assets/AGENTS.md` was read completely. It requires this persistent plan to be the first write, exact scope, preservation of existing changes, a final caller/consumer audit, and explicit pending Unity validation.
- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md` was reviewed through the canonical Sections 1–20, VEG-V1H, and the complete active VEG-V2-INFRA.1A plan. INFRA.1A establishes that `VegetationBenchmark` and `VegetationLayer` share production fields through `VegetationRendererBase`, while each layer owns independent coverage and hierarchy-resolved Ground ownership.
- `Assets/Docs/Stylized_Vegetation_Architecture.md` was read completely. Section 16 permits multiple layers to share geometry/family settings while retaining independent coverage and wind response.
- `Assets/Docs/handoff.md` was read completely. It contains handoff-production requirements and no conflicting active vegetation implementation decision.
- `Assets/Game/Procedural/Vegetation/VegetationRendererBase.cs`, `VegetationLayer.cs`, `VegetationCoverageField.cs`, `VegetationBenchmark.cs`, `VegetationClusterMeshBuilder.cs`, `VegetationInstanceData.cs`, `Editor/VegetationLayerEditor.cs`, and `Editor/VegetationRendererEditorPreview.cs` were read completely. The shared serialized recipe is owned by `VegetationRendererBase`; `VegetationLayer.coverage` is separate; the two explicit wind-lighting fields are `windNormalResponse` and `windBendShadingResponse`.
- Direct repository references were searched. `VegetationLayerEditor.DrawProductionProperties()` exposes the shared production recipe but hides `fieldSize`, `coverageGround`, `useGroundCoverage`, and `coverage`. `VegetationLayer.SynchronizeSurfaceGroundFromHierarchy()` makes the ancestor Ground authoritative. `VegetationRendererBase.ComputeRebuildConfigurationHash()` and `ComputeLightingConfigurationHash()` distinguish rebuild fields from material-only fields.
- In `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity`, `VegetationTest` is inactive (`m_IsActive: 0`) and its `VegetationBenchmark` begins at file ID `902325200`. `Grass_Tall` is active and its `VegetationLayer` begins at file ID `2090205568`. Both currently reference Ground file ID `593135561`, use seed `7319`, and already share the same stiffness range.
- The scene already contains user-owned working-tree changes. `git status --short` reports it modified; `git diff --numstat` reports `131` inserted and `4` deleted lines. The current `Grass_Tall` object, component, and 128² coverage payload are part of that pre-existing working-tree delta and must be preserved.
- `HEAD` (`0122633ca1057bb3270e462f93142ab4effb09c3`, `Refine river foam and vegetation systems`) contains the same current `VegetationTest` non-wind source values but no `Grass_Tall` production layer. The current scene is therefore the authoritative target baseline, while the benchmark values are corroborated by `HEAD`.
- Relevant history inspected: `0122633`, `ab7b042`, `40d79bb`, `1a6d06e`, `456d195`, `c9a2a0f`, `3a57fda`, and `9f78898`. No accepted historical version supersedes the current benchmark recipe or the INFRA.1A ownership contract.

### Exact mapping and preserved values

Copy every differing shared non-wind recipe value from `VegetationTest` through `localEdgeActivationThreshold`. This changes 21 serialized values: density, minimum coverage threshold, geometry candidate, grass height, master blade width, taper start, height/width variation, four macro-patch composition values plus dark/light strengths, three colours, and five punctual-edge styling values.

The following are already equal and remain unchanged: `fieldSize`, `seed`, `coverageGround`, `useGroundCoverage`, `clusterDiameter`, `tipWidthRatio`, all three width-stabilization values, `stiffnessRange`, `grassPatchPatternSeed`, ambient/sun/local responses, minimum night visibility, diffuse wrap, normal-up bias, light-colour influence, and minimum stable accent pixels.

Preserve intentionally distinct values:

- `Grass_Tall.windNormalResponse = 0.7`
- `Grass_Tall.windBendShadingResponse = 1`
- `Grass_Tall.targetCamera = null`
- `Grass_Tall.renderBenchmark = true`
- `Grass_Tall.sceneViewPreview = true`
- the complete `Grass_Tall.coverage` object, including resolution, byte payload, revision, and initialized state

### Invariants and non-goals

- Do not copy benchmark-suite fields; they do not exist on `VegetationLayer`.
- Do not copy `VegetationTest`'s camera or wind-provider component state.
- Do not initialize, fill, resize, repaint, or otherwise rewrite layer coverage.
- Do not change object active state, component enabled state, hierarchy, transforms, Ground ownership, rendering state, layers, or tags.
- Do not normalize or reserialize the complete scene. Apply only targeted Force-Text scalar/vector/colour substitutions inside component file ID `2090205568`.
- Preserve all unrelated user-owned worktree changes in the scene and existing INFRA.1A source/document changes.

### File-by-file implementation sequence

| ID | File | Required work | Status |
| --- | --- | --- | --- |
| SCENE.1.0 | This document | Record authorization, evidence, exact mapping, invariants, risks, performance, and validation before the scene edit. | Complete |
| SCENE.1.1 | `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity` | Replace only the 21 differing non-wind shared recipe values on component file ID `2090205568`. | Complete; Unity validation pending |
| SCENE.1.2 | Both approved files and read-only dependencies | Audit exact scope, YAML structure, source/target parity, preserved coverage/wind/runtime values, Git diff, architecture compliance, and available Unity validation. | Source audit complete; Unity validation pending |

### Performance impact and risks

- Active runtime code paths, shader instructions, draw topology, instance stride, and per-frame CPU work do not change.
- Dirty-time rebuild cost increases because density changes from `16` to `35` clusters/m² and geometry changes from `OpaqueStrips` to `CrossedCards`. Candidate generation remains O(Ground world area × density); accepted instance-buffer memory remains O(accepted instances × 48 bytes). This is an intentional copy of the user-selected benchmark recipe, not a new algorithm.
- GPU vertex/fragment cost may change with the copied density and geometry. No performance claim is made without Unity profiling. Validate the authored layer at 2560 × 1440 under the existing vegetation budget before treating the recipe as production-accepted.
- Raw scene editing can damage the serialized coverage payload if the replacement span is too broad. Mitigation: patch only short named scalar/vector/colour lines before `coverage:` and verify the payload hash is unchanged.
- Unity may normalize inherited serialized fields on import. Unity 6000.5.0f1 scene load/save and visual inspection remain required validation.

### Validation requirements

1. Parse the two component mappings from the final YAML and prove all shared non-wind recipe values match, excluding the documented wind/runtime/coverage fields.
2. Prove `Grass_Tall` wind fields, runtime fields, hierarchy references, active/enabled state, and complete coverage payload match the captured pre-edit state.
3. Inspect `git diff --check`, the targeted scene diff, and repository status; confirm only the two approved files changed during this update and no unrelated delta was overwritten.
4. Reread both modified files plus the current renderer base, layer, benchmark, editor, coverage, geometry, instance, and Ground contracts after the edit.
5. In Unity 6000.5.0f1, load `VisualFrameworkDemo`, confirm clean scene deserialization/compilation, rebuild `Grass_Tall`, and visually compare its non-wind grass appearance with the disabled benchmark recipe while retaining independent wind and coverage.

### Post-change consistency and compliance audit

#### Actual affected files

Modified:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity`

Created, deleted, moved, renamed, generated, or metadata files: none.

This matches the predeclared and user-approved scope. Repository status after the update contains the same pre-existing INFRA.1A source/untracked-file set observed before this update; no additional path appeared.

#### Intentional scene differences

Only component file ID `2090205568` received the planned 21 substitutions:

- density `16 → 35` and minimum coverage `0.02 → 0.03`;
- geometry `OpaqueStrips (0) → CrossedCards (1)`;
- grass height `0.8 → 0.9`, master blade width `0.021 → 0.025`, and taper start `0.68 → 0.382`;
- height range `(0.8, 1.2) → (0.95, 1.35)` and width range `(0.85, 1.15) → (1.0, 1.3)`;
- macro scale `4.5 → 1.45`, transition softness `0.75 → 1`, separation `1 → 1.02`, dark strength `0.12 → 0.3`, and light strength `0.08 → 0.35`;
- root, base, and tip colours now equal the benchmark values;
- edge accent `0.22 → 0.4`, edge width `0.1 → 0.4`, whiteness `0.75 → 0.22`, local falloff power `3 → 2`, and activation threshold `0.35 → 0`.

A final parser compared 40 shared non-wind fields from `fieldSize` through `localEdgeActivationThreshold` and reported `NON_WIND_MISMATCHES=0`.

#### Preserved state and contracts

- `Grass_Tall` remains active; component file ID `2090205568` remains enabled and uses the same `VegetationLayer` script GUID.
- Its transform remains the sole child of `Vegetation` transform file ID `1011255917`; that object remains beneath Ground transform file ID `593135560`.
- Ground file ID `593135561`, seed `7319`, stiffness `(0.12, 0.48)`, camera `{fileID: 0}`, rendering `1`, and Scene preview `1` are unchanged.
- Distinct wind values remain `windNormalResponse: 0.7` and `windBendShadingResponse: 1`; benchmark values `2.5` and `2` were not copied.
- Coverage remains `128 × 128`, 32,768 serialized hexadecimal characters representing 16,384 bytes, revision `4`, initialized `1`. Its final ASCII-payload SHA-256 is `be9b795e95c7b4940cc40e023cfda1b4f22ed97d8c44d8ebc45625db532a2b12`. A pre-edit digest was not captured, so preservation is established by the exact named-line patch, which did not include or replace `coverage:`, and by the unchanged pre/post resolution, revision, initialization, and visible payload boundary; a cryptographic pre/post equality claim is not made.
- The final renderer base, layer, benchmark, coverage, editor, geometry builder, instance data, and Ground contracts were reread/checked after the scene edit. No implementation dependency changed.
- `HEAD` still corroborates all 40 source benchmark values. The target layer remains a working-tree-only INFRA.1A scene addition, so no HEAD target comparison exists.

#### Validation results and pending checks

- Final source/target field parser: **pass**, 40 compared non-wind fields and zero mismatches.
- Target wind/runtime/hierarchy/coverage parse: **pass** for the preserved values listed above.
- Canonical-plan `git diff --check`: **pass**. Whole-scene `git diff --check` reports existing trailing whitespace on the pre-existing Unity YAML line `m_Name: ` at scene line 14178. That line existed before SCENE.1 and was not edited; removing Unity's serialized empty-name spacing would be unrelated raw-scene cleanup and is intentionally not performed.
- Project version is `6000.5.0f1 (88b47c5e7076)`. No `Unity` command is available on `PATH`, so scene import/deserialization, compilation, rebuild behavior, visual equivalence, and performance profiling are **pending**.
- Runtime performance is **unverified**. The copied density raises candidate count by `35 / 16 = 2.1875×` for the same Ground world area before coverage rejection. Actual accepted-instance count and CrossedCards GPU cost require the existing 2560 × 1440 Unity benchmark/profiler workflow.

## VEG-V2-INFRA.1B — Ground Vegetation Root, Layer Painting, Safe Migration, and Benchmark Runner Shell

### Status

**Frozen on 2026-07-21 after user acceptance.** This update created the approved Ground-owned hierarchy, moved ordinary coverage painting to the selected production layer, provided explicit Undo-supported migration, and introduced a diagnostics-only scene runner shell. The user accepted the update and authorized WEATHER-V0A after reviewing the final two-layer scene inventory.

### Objective and acceptance criteria

The target hierarchy is:

```text
GeneratedGround                          [GeneratedGround]
└── Vegetation                           [GroundVegetation]
    ├── Grass_Default                    [VegetationLayer]
    └── <additional independent recipes> [VegetationLayer]
```

The update is accepted only when:

1. `GroundVegetation` and every `VegetationLayer` resolve the nearest ancestor `GeneratedGround`; neither exposes a manually assignable Ground field.
2. One explicit migration action creates or reuses the direct `Vegetation` child, imports the legacy Ground mask and brush settings into production layers, copies only production renderer settings, rebuilds each new layer, and disables but does not delete the legacy benchmark.
3. Uninitialized legacy coverage becomes an explicitly full production-layer mask because the legacy behavior authorizes the complete Ground in that state.
4. Scene painting belongs to the selected `VegetationLayer`; intermediate drag stamps mutate only that layer's CPU mask and exactly one rebuild occurs when a changed stroke completes.
5. Layer masks may overlap, and duplicate-as-empty copies the recipe and brush settings but initializes an independent empty mask.
6. `VegetationBenchmarkRunner` inventories and reports the enabled production stack without inheriting from the renderer or owning meshes, materials, buffers, masks, or per-frame work.
7. Legacy Ground bytes and the legacy benchmark component remain available for rollback until `VEG-V2-INFRA.3`.

### Approved file scope

Create:

- `Assets/Game/Procedural/Vegetation/GroundVegetation.cs` and `.meta`
- `Assets/Game/Procedural/Vegetation/Editor/GroundVegetationEditor.cs` and `.meta`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationInfrastructureMigration.cs` and `.meta`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmarkRunner.cs` and `.meta`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkRunnerEditor.cs` and `.meta`

Modify:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationLayer.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationLayerEditor.cs`
- `Assets/Game/Procedural/Ground/GeneratedGround.cs`
- `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`

No scene, prefab, material, shader, geometry-builder, instance-layout, Weather, layer, tag, or URP asset change is authorized.

### Read-only review evidence

- The supplied `Assets-Code-Archive(8).zip` contains 323 relative entries, has no traversal paths, and has SHA-256 `dd250bd0fb30e9d602a28856f9dc9edf14ec4452a026a3baad53926fbb0df948`. No `.git` directory exists; branch, `HEAD`, diff, and history are unavailable.
- `Assets/AGENTS.md` was read completely. It requires this persistent plan as the first write, exact scope, no raw scene editing, no per-frame full-field rebuilds, a final consistency audit, and explicit pending Unity checks.
- `VegetationLayer.PaintCoverage()` currently calls `RebuildVegetation()` after every changed stamp. `GeneratedGroundEditor.HandleVegetationCoverageSceneGUI()` already separates drag stamps from one end-of-stroke rebuild, but currently owns the legacy Ground mask and globally scans legacy benchmarks. INFRA.1B ports the stroke lifecycle to the layer and removes normal Ground painting ownership.
- `VegetationCoverageField` already supports byte-mask initialization, resize, fill, bilinear sampling, painting, texel world positions, and `ImportLegacy`; no field-format change is required.
- `GeneratedGround` retains the exact legacy mask, initialized flag, resolution, revision, and brush settings. A new editor-only snapshot API will copy those bytes for migration without exposing mutable internal storage.
- `VegetationRendererBase` contains 45 serialized production fields. Migration will copy the explicit allowlist below and will not copy `coverageGround` or `useGroundCoverage`, because hierarchy resolution is authoritative on `VegetationLayer`.
- `VegetationBenchmark` retains only suite state beyond the inherited production fields. The existing suite remains transitional and is not rewritten in this update.
- `VisualFrameworkDemo.unity` still contains the legacy `VegetationTest` benchmark and initialized Ground coverage. It is read-only source evidence; migration changes the hierarchy only through Unity Editor Undo APIs when the user invokes the action.

### Explicit production-property migration allowlist

```text
fieldSize
densityPerSquareMetre
seed
minimumCoverage
geometry
clusterDiameter
grassHeight
masterBladeWidth
tipWidthRatio
taperStart
enableWidthStabilization
widthStabilizationStartDistance
widthStabilizationMaximumMultiplier
heightScaleRange
widthScaleRange
stiffnessRange
grassPatchScale
grassPatchPatternSeed
grassPatchTransitionSoftness
averageGrassPatchSeparation
darkPatchStrength
lightPatchStrength
rootColor
baseColor
tipColor
ambientResponse
sunResponse
localLightResponse
minimumNightVisibility
diffuseWrap
normalUpBias
windNormalResponse
windBendShadingResponse
lightColourInfluence
stylizedEdgeAccent
edgeAccentWidth
minimumStableAccentPixels
edgeHighlightWhiteness
localEdgeFalloffPower
localEdgeActivationThreshold
targetCamera
renderBenchmark
sceneViewPreview
```

This list copies 43 production properties. `coverageGround` and `useGroundCoverage` are deliberately excluded. Suite fields, runtime resources, statistics, coroutines, and reports are not serialized production properties and are not copied.

### Hierarchy and migration transaction

1. Preflight one selected `GeneratedGround`, the legacy snapshot, matching legacy benchmarks, and the explicit property allowlist.
2. Begin one Undo group.
3. Create or reuse the direct `Vegetation` child, add or reuse `GroundVegetation`, and normalize the organizational transform to local zero/identity/one.
4. Create one direct recipe child per matching legacy benchmark, copy the explicit production allowlist, import initialized bytes exactly or create an explicit full mask for the uninitialized legacy fallback, copy brush settings, and rebuild.
5. Disable each migrated legacy benchmark without deleting its component, GameObject, suite state, or Ground mask.
6. Create or reuse one scene diagnostics hierarchy and one `VegetationBenchmarkRunner` shell.
7. emit a migration report containing source/destination names, copied-property count, coverage mode, pre/post instance counts, errors, and rollback statement.
8. Collapse the Undo group. On an exception, revert the group where possible and log the full error instead of silently leaving a partial migration.

The new recipe GameObjects use local position zero, identity rotation, and unit scale. Ground-owned world placement remains authoritative. A deterministic hash change caused by the new local transform basis is not itself a migration failure; visible world output and structural counts require Unity validation.

### Layer painting lifecycle

1. The selected `VegetationLayer` Inspector owns paint mode, radius, strength, erase mode, and overlay state.
2. Mouse down registers one complete Undo snapshot of that layer and begins a stroke.
3. Mouse down/drag calls a no-rebuild mask-stamp API on the selected layer only.
4. The overlay cache invalidates using layer coverage revision, Ground surface revision, resolution, and Ground transform hash.
5. Mouse up, Escape, Alt navigation, selection loss, editor disable, or mouse-leave completes the stroke.
6. A changed stroke calls the layer's explicit completion method once; that method performs exactly one `RebuildVegetation()`.
7. Undo/redo rebuilds the selected layer only. No global renderer scan occurs.

The overlay remains capped at `min(32, resolution)²`, at most 1,024 points.

### Runtime and memory model

- `GroundVegetation` and `VegetationBenchmarkRunner` contain no `Update`, `LateUpdate`, or `FixedUpdate`.
- A migrated single layer retains one `LateUpdate` render submission and one indirect draw through `VegetationRendererBase`; the manager and runner add no normal-frame work.
- Each additional enabled recipe intentionally adds one renderer, one draw, one mesh/material pair, and one 48-byte-per-instance buffer.
- Each default layer mask remains 16,384 serialized bytes. Legacy bytes are retained temporarily, so migration duplicates 16 KiB per 128² Ground until cleanup.
- Paint stamp cost is proportional to the texels inside the brush bounding box. The full placement rebuild remains O(Ground world area × density) and runs once per changed stroke.
- Migration and diagnostics may allocate temporary editor-only byte arrays and strings because they execute only on explicit user actions.

No `PERFORMANCE EXCEPTION` is active.

### File-by-file implementation sequence

| ID | File | Work | Status |
|---|---|---|---|
| INFRA1B.0 | Both architecture documents | Record evidence, exact scope, property allowlist, migration transaction, painting lifecycle, performance, and validation before code changes. | Complete |
| INFRA1B.1 | `VegetationLayer.cs` | Add layer-owned brush state, no-rebuild stamp API, one completion rebuild, import/settings helpers, and preserve hierarchy-only Ground ownership. | Complete |
| INFRA1B.2 | `VegetationLayerEditor.cs` | Port selected-layer Scene painting, overlay, Undo, and one-rebuild-per-stroke behavior. | Complete |
| INFRA1B.3 | `GroundVegetation.cs` and editor | Add direct-child coordination, create/duplicate/rebuild/validate/copy-report actions, no per-frame methods. | Complete |
| INFRA1B.4 | `GeneratedGround.cs` and editor | Add editor-only copied legacy snapshot; replace normal Ground painting UI with migration/rollback status and root actions; remove the global paint-rebuild scan. | Complete |
| INFRA1B.5 | Migration utility | Implement preflight, explicit production-property copy, exact/full coverage import, brush copy, legacy disable, runner creation, report, Undo, and exception rollback. | Complete |
| INFRA1B.6 | Runner and editor | Add diagnostics-only layer inventory, aggregate structural report, clipboard action, and duplicate-runner warning. | Complete |
| INFRA1B.7 | Legacy benchmark and editor | Mark the component as transitional and expose the explicit migration route without changing V1G suite behavior. | Complete |
| INFRA1B.8 | Approved files and direct dependencies | Run parser/compiler, namespace/reference, scope, no-per-frame, no-manual-Ground, stamp/rebuild, legacy-retention, unchanged-dependency, and final consistency checks. | Source checks complete; Unity compile pending |

### Validation requirements

Source validation must prove:

- exact expected/actual file scope and valid `.meta` GUID files;
- every changed C# file parses and all preprocessor blocks balance;
- every new symbol resolves within the current namespaces and referenced assemblies;
- no manual Ground field exists on `GroundVegetation` or `VegetationLayer`;
- neither the manager nor runner contains `Update`, `LateUpdate`, or `FixedUpdate`;
- the no-rebuild stamp path contains no `RebuildVegetation()` call and the completion path contains one;
- migration copies only the explicit 43-property allowlist and preserves legacy data/components;
- Ground normal painting no longer registers a Scene callback or globally rebuilds benchmarks;
- shaders, geometry, instance layout, Weather, scenes, prefabs, materials, URP assets, layers, and tags remain byte-identical.

Unity validation remains pending until the user imports the patch. The final delivery must provide no more than six material checks: compile/import, migrate, paint one layer, overlap two layers, duplicate-as-empty, and copy the migration/stack report.


### Freeze evidence

The user supplied the final `Vegetation INFRA.1B Scene Layer Inventory` from `VisualFrameworkDemo` and explicitly approved moving to the next patch. The report contained one active runner and two ready enabled layers:

- `Grass_Default`: 19,694 instances, 236,328 submitted triangles, 945,312 instance-buffer bytes, 35.0% coverage.
- `Grass_Short`: 4,633 instances, 55,596 submitted triangles, 222,384 instance-buffer bytes, 8.3% coverage.
- Enabled-stack totals: 24,327 instances, 291,924 submitted triangles, and 1,167,696 instance-buffer bytes.

The report is internally exact: both recipes submit 12 triangles per instance, both buffers use the preserved 48-byte instance contract, and every aggregate equals the sum of its two layers. This proves stack discovery, layer readiness, aggregate accounting, geometry accounting, and instance-buffer accounting for the reported scene. The user's explicit acceptance freezes INFRA.1B. The report does not independently measure one-rebuild-per-stroke profiler behavior or Undo internals; those remain accepted through the user's completed workflow rather than separate telemetry.

### Implementation result and final source audit

#### Actual affected files

Created exactly as declared:

- `Assets/Game/Procedural/Vegetation/GroundVegetation.cs` and `.meta`
- `Assets/Game/Procedural/Vegetation/Editor/GroundVegetationEditor.cs` and `.meta`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationInfrastructureMigration.cs` and `.meta`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmarkRunner.cs` and `.meta`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkRunnerEditor.cs` and `.meta`

Modified exactly as declared:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationLayer.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationLayerEditor.cs`
- `Assets/Game/Procedural/Ground/GeneratedGround.cs`
- `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`

Deleted, moved, renamed, scene, prefab, material, shader, Weather, geometry, instance-layout, layer, tag, and URP files: none.

#### Implemented behavior

- `GroundVegetation` is a lightweight direct-child coordinator. It resolves its nearest Ground from hierarchy, enumerates direct recipe children only, exposes create/duplicate/rebuild/report authoring through its editor, and contains no frame loop or shared mask.
- `VegetationLayer` now owns persistent brush state and separates `PaintCoverageStamp()` from `CompleteCoverageStroke()`. Scene drag stamps do not rebuild; a changed completed stroke rebuilds the selected layer exactly once.
- `VegetationLayerEditor` owns Scene painting and the capped revision-cached overlay. It registers one complete Undo snapshot at stroke start and performs no global renderer scan.
- `GeneratedGround` exposes an editor-only copied legacy snapshot. Its normal Inspector no longer owns coverage painting; it shows production-root status and the explicit migration/rollback path while retaining all legacy bytes and brush fields.
- Migration uses one Undo group, creates/reuses the approved hierarchy, copies the explicit 43-property production allowlist, imports initialized bytes exactly or creates the explicit full fallback, copies brush state, rebuilds the destination, disables the legacy benchmark without deleting it, creates/reuses the diagnostics runner shell, and reverts the group on exceptions.
- `VegetationBenchmarkRunner` provides structural layer inventory and aggregate counts only. It does not inherit the renderer, allocate rendering resources, or implement the deferred timing suite.
- The legacy benchmark and V1G suite remain intact and are labelled transitional.

#### Validation result

`VEG-V2-INFRA.1B_Source_Validation.txt` reports **86/86 passed**:

- exact 10-created / 8-modified / zero-deleted scope;
- lexical delimiter and preprocessor balance over all 11 changed C# sources;
- Tree-sitter C# parse with zero error or missing nodes for all 11 changed C# sources;
- valid unique new `.meta` GUIDs;
- hierarchy ownership, direct-child management, no manager/runner frame loops, diagnostics-only runner, one-rebuild-per-stroke structure, no layer-editor global scan, 32 × 32 overlay cap, legacy retention, explicit 43-property allowlist, Undo rollback, and full-mask fallback;
- frozen renderer base, coverage field, geometry builder, instance layout, Weather files, vegetation shaders/includes, and `VisualFrameworkDemo.unity` byte-identical to the supplied baseline;
- `GeneratedGroundEditor.cs` CRLF convention preserved.

A Unity executable, Unity reference assemblies, and a C# compiler against Unity assemblies are unavailable in this environment. Unity import/compilation, serialization, Undo behavior, rendered equivalence, exact instance-count comparison, one-rebuild profiler evidence, multi-layer overlap, and duplicate-as-empty behavior are therefore **pending user validation**. The source is not described as Unity-compiled until that evidence is supplied.

#### Performance reconciliation

- Active gameplay: `GroundVegetation` and `VegetationBenchmarkRunner` add no normal-frame method. Replacing one enabled legacy renderer with one enabled production layer preserves the existing renderer submission model: one layer, one `LateUpdate` submission, one indirect draw.
- Dirty-time painting: intermediate stamps mutate the selected byte mask only; one full layer rebuild occurs at changed-stroke completion.
- Memory: each default production mask is 16 KiB. Legacy rollback data remains temporarily, intentionally duplicating that mask until INFRA.3.
- Additional enabled recipes intentionally add their own geometry, material, buffer, and draw. No active-frame mask sampling, hierarchy scan, or full-field manager rebuild was introduced.

**Status:** frozen by user acceptance on 2026-07-21. WEATHER-V0A is the next authorized update.

## VEG-V2-INFRA.2 + WEATHER-WIND-V0A.1 — Stack-Aware Timing Runner and D3D11 Wind-Wrap Warning Fix

### Status

**Source implementation and compliance audit complete on 2026-07-21; Unity validation pending.** INFRA.1B and WEATHER-V0A are frozen from user evidence. The stack-aware runner and unsigned Weather wrap correction are implemented within the declared scope; Unity compilation, D3D11 warning clearance, timed execution, screenshot/report saving, and interruption restoration remain to be validated in Unity 6000.5.0f1.

**Partial supersession:** the controlled-run density tiers, case composition, and production-density setter described in this INFRA.2 section are superseded by `VEG-V2-INFRA.2A` below. The D3D11 Weather wrap correction, timing measurement mechanics, report statistics, screenshot flow, and restoration architecture remain current.

### Objective and acceptance criteria

Replace the transitional component-local V1G timing entry point with the single scene-level `VegetationBenchmarkRunner`. The runner must benchmark either the exact currently enabled authored layer stack or one explicitly selected controlled layer, use adjacent render-disabled baselines, restore every modified layer state, save one report, retain it for clipboard copy, and never render vegetation itself.

The same update removes the D3D11 compute warning emitted by `CS_WeatherWindField.compute` for signed integer modulus in `PhysicalCell`. The replacement must preserve toroidal addressing exactly for the current power-of-two Weather field contract and must not change wind simulation values, field resolution, recenter behavior, texture layout, runtime C# dispatch logic, or consumer sampling.

Acceptance requires:

1. exactly one active scene-level runner remains the diagnostics owner;
2. enabled-stack mode measures all scoped layers that are active, enabled, and have rendering enabled without mutating geometry, density, coverage, or recipe settings;
3. controlled-layer mode isolates one selected in-scope active layer, runs the existing three-geometry by two-density matrix against its authored coverage/domain, disables sibling rendering only for measurement, and restores geometry, density, and all sibling render flags;
4. every timed pass can interleave an adjacent all-render-disabled baseline and reports whole-frame CPU/GPU statistics plus noise-aware deltas;
5. stopping, disabling, destroying, or completing the runner restores captured state;
6. the legacy `VegetationBenchmark` Inspector no longer exposes or starts the component-local suite and directs timing work to the scene runner;
7. inventory and timed reports remain copyable, and the timed report is also saved under `Library/VegetationBenchmarkDiagnostics`;
8. `PhysicalCell` contains no signed integer modulus and wraps identically for all valid logical cells and offsets under the documented power-of-two resolution invariant;
9. no active-gameplay work is added outside an explicitly running benchmark coroutine, and no per-frame methods are added to the runner.

### Approved file scope

Modify only:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Docs/Weather_System_Architecture_Provisional.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmarkRunner.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkRunnerEditor.cs`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Weather/Resources/PS3DWeather/Compute/CS_WeatherWindField.compute`

Create, delete, move, rename, scene, prefab, material, runtime Weather C#, vegetation renderer/base/layer, HLSL include, layer, tag, and URP changes: none authorized.

### Read-only review evidence

- `Assets/AGENTS.md` was read completely. It requires the canonical plan as the first write, exact scope, complete final reread, caller/consumer audit, and unavailable Unity checks marked pending.
- `VegetationBenchmarkRunner.cs` currently contains structural inventory only. `BuildLayerInventoryReport()` explicitly reports that timing is deferred to INFRA.2; the class contains no renderer inheritance and no frame-loop method.
- `VegetationBenchmark.cs` currently owns the V1G timing coroutine at `BeginTimedComparisonSuite`, `RunTimedComparisonSuite`, and `MeasureSuiteWindow`. The old suite mutates only its own geometry/density/render flag, runs three geometries at densities 35/50, interleaves render-disabled baselines, captures screenshots, saves a report, and restores state in `finally`.
- `VegetationBenchmarkEditor.cs` is the only caller of `BeginTimedComparisonSuite()` and currently exposes the legacy run/copy/progress UI. The migrated legacy component remains disabled for rollback after INFRA.1B.
- `VegetationRendererBase.cs` exposes `SetGeometryCandidate`, `SetDensityPreset`, `SetRenderingEnabled`, `RebuildVegetation`, structural counters, build duration, placement summary, and render state. These existing public APIs are sufficient; no renderer/base/layer source edit is required.
- `VegetationInfrastructureMigration.CreateOrReuseSceneRunner()` creates exactly one `Systems/Diagnostics/Vegetation Benchmark` runner and does not configure a per-Ground runner.
- Accepted INFRA.1B user evidence reported one active runner, two ready enabled layers, 24,327 aggregate instances, exact 12-triangle-per-instance accounting, and exact 48-byte-per-instance buffer accounting.
- Accepted WEATHER-V0A user evidence reported `Status: READY`, one active/published domain, 128² resolution, 0.5 m cells, active simulation dispatches, and intact CPU/consumer contracts.
- `CS_WeatherWindField.compute` currently implements `PhysicalCell` as `(logicalCell + _FieldOffset) % _FieldResolution`, producing the supplied D3D11 warning for all three kernels.
- `WeatherWindDomain.OnValidate()` normalizes `fieldResolution` through `Mathf.ClosestPowerOfTwo` and clamps it to 32–256. `SetCommonComputeParameters()` sends equal positive resolution components and a positive normalized ring offset. Therefore `uint2` addition followed by `& (resolution - 1u)` is equivalent to positive modulo for every valid field cell.
- `WeatherWindDomain.cs`, `WeatherWindField.hlsl`, and `VegetationWindResponse.hlsl` were reviewed as producer and consumers. No consumer depends on the arithmetic form used inside `PhysicalCell`; only the resulting wrapped physical coordinate is observable.
- No `.git` directory exists in the supplied archive workspace. Branch, `HEAD`, history, and working-tree comparison are unavailable. The authoritative source was reconstructed from `Assets-Code-Archive(8).zip`, the accepted INFRA.1B patch, and the accepted WEATHER-V0A patch; all archives were traversal-safe.

### Runner measurement contract

`VegetationBenchmarkRunner` will expose two modes:

- **Enabled Stack:** snapshot every scoped layer, then measure exactly the layers whose GameObjects/components are active and whose renderer flag was enabled at suite start. No layer rebuild occurs in this mode.
- **Controlled Layer Matrix:** require one selected in-scope active `VegetationLayer`; suppress all siblings, enable only the controlled layer for vegetation windows, and run `OpaqueStrips`, `CrossedCards`, and `Cards` at densities 35 and 50 against the layer's existing Ground domain and authored mask. The runner rebuilds only the controlled layer once per matrix case and once for restoration.

A baseline window sets rendering disabled on all scoped layers. A vegetation window reapplies the captured enabled stack or enables only the controlled layer. Toggling uses the existing `SetRenderingEnabled()` path and does not rebuild. Each pass alternates baseline-first and vegetation-first ordering when interleaving is enabled.

The report must include scope, mode, layer inventory, environment, warm-up/measurement settings, structural state for every measured case, CPU/GPU sample statistics, median delta, combined standard-deviation noise estimate, screenshot status, restoration result, and saved report path.

### Restoration and failure contract

Before timing, capture each scoped layer reference and its renderer flag. Controlled mode also captures the selected layer's geometry and density. State restoration must execute from the coroutine `finally` block and from runner disable/destruction protection when an active suite exists. Restoration reapplies all renderer flags and rebuilds the controlled layer only if its geometry or density changed. The final report must identify restoration success or exact errors. The runner must not leave siblings hidden after an interrupted test.

The legacy `VegetationBenchmark.BeginTimedComparisonSuite()` becomes a rejected compatibility entry point that records a superseded status and returns false. Its retained private V1G implementation remains rollback-only until INFRA.3; the legacy Inspector removes run/copy/progress controls and points to `Systems > Diagnostics > Vegetation Benchmark`.

### Weather compute change and cross-subsystem audit

`PhysicalCell` will convert logical cell, offset, and resolution to `uint2`, then wrap with a bit mask:

```hlsl
uint2 resolution = (uint2)_FieldResolution;
uint2 wrapped = ((uint2)logicalCell + (uint2)_FieldOffset) & (resolution - 1u);
return (int2)wrapped;
```

This is valid only because both resolution components are positive powers of two. That invariant is established in `WeatherWindDomain.OnValidate()` and is unchanged. `InitializeField`, `RecenterField`, and `SimulateField` continue calling the same helper. Texture formats, physical coordinates, noise inputs, response integration, and writes remain unchanged. The Weather HLSL sampling include and vegetation wind consumer are unaffected and must remain byte-identical.

### File-by-file implementation sequence

1. Record this plan in the canonical vegetation ledger. **Complete.**
2. Extend `VegetationBenchmarkRunner.cs` with scope/mode configuration, state snapshots, one coroutine timing implementation, reports, screenshots, robust statistics, restoration, and structural inventory reuse. **Complete at source level; Unity validation pending.**
3. Replace the shell Inspector with one-run, one-copy, progress, report path, controlled-layer validation, and inventory actions. **Complete at source level; Unity validation pending.**
4. Disable the legacy benchmark timing entry point and remove legacy suite controls from its Inspector while preserving migration, rendering, structural report, and rollback code. **Complete.**
5. Replace signed modulus in `CS_WeatherWindField.compute` with unsigned power-of-two wrapping. **Complete at source level; Unity D3D11 validation pending.**
6. Freeze accepted INFRA.1B and WEATHER-V0A evidence and record the warning fix in the canonical conceptual and Weather documents. **Complete.**
7. Run exact-scope, parser, preprocessor, API/reference, restoration, report, no-frame-loop, compute-contract, and unchanged-dependency validation. **Complete — 56/56 source checks passed.**
8. Reread every changed file and direct caller/producer/consumer, reconcile the final diff, and record the post-change audit here. **Complete at source level.**

### Runtime and performance model

Outside an explicitly running suite, the runner has no `Update`, `LateUpdate`, or `FixedUpdate`, performs no layer scans, and adds no rendering or allocation. Inventory/report work is explicit Editor action work.

Enabled-stack timing changes only renderer flags and therefore adds no rebuild cost. Controlled-layer timing performs six explicit dirty-time rebuilds plus one restoration rebuild. Measurement uses whole-frame `FrameTimingManager` and `Time.unscaledDeltaTime`, matching the accepted V1G methodology. It does not claim isolated draw-call or per-feature shader cost.

The compute change replaces signed modulo in every weather cell dispatch with unsigned bitwise masking. It reduces arithmetic cost under the existing power-of-two contract and changes no memory allocation, dispatch count, update cadence, or sample count.

### Validation requirements

Static validation must prove exact nine-file scope, C# parse and preprocessor balance, no new frame-loop method on the runner, both benchmark modes, state capture/restoration paths, no renderer inheritance, legacy entry-point rejection, report save/copy paths, no signed `%` in `PhysicalCell`, power-of-two producer evidence, and byte identity of `WeatherWindDomain.cs`, Weather/vegetation HLSL consumers, vegetation renderer/base/layer, scene, prefabs, and materials.

Unity validation is limited to: clean import with no D3D11 modulus warning; enabled-stack run and report copy; controlled-layer matrix with sibling restoration; deliberate runner disable during a suite and restoration confirmation; report file existence; unchanged Weather report and visible wind behavior.


### VEG-V2-INFRA.2 + WEATHER-WIND-V0A.1 post-change consistency and compliance audit

#### Actual affected files

Modified exactly as declared:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Docs/Weather_System_Architecture_Provisional.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmarkRunner.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkRunnerEditor.cs`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Weather/Resources/PS3DWeather/Compute/CS_WeatherWindField.compute`

Created, deleted, moved, renamed, scene, prefab, material, asset, shader, HLSL include, runtime Weather C#, vegetation renderer/base/layer, layer, tag, and URP files: none.

#### Implemented runner behavior

- `VegetationBenchmarkRunner` remains a plain diagnostics `MonoBehaviour`; it does not inherit `VegetationRendererBase`, own vegetation resources, or render vegetation.
- **Enabled Stack** mode captures every scoped layer and measures exactly the layers that were active, component-enabled, and rendering-enabled at suite start. Its case path changes renderer flags only and performs zero rebuilds.
- **Controlled Layer Matrix** mode requires one selected active in-scope layer, captures its original geometry/density and every scoped render flag, suppresses siblings, and runs all current geometry enum values at densities 35 and 50 against the layer's actual Ground domain and authored coverage.
- Every measurement pass can alternate baseline-first ordering. Baseline windows disable rendering on every captured layer; vegetation windows restore the captured enabled stack or enable only the controlled layer.
- Reports include the complete layer inventory, structural counts, placement and coverage state, environment, whole-frame CPU/GPU statistics, median deltas, combined noise estimates, screenshot verification, case ranking, completion state, restoration result, and file path.
- The runner saves `Library/VegetationBenchmarkDiagnostics/Vegetation_INFRA2_Stack_Aware_Benchmark_Report.txt` and retains the same report in memory for one Inspector clipboard action.
- The coroutine `finally` restores every captured render flag and conditionally restores/rebuilds the controlled recipe. `OnDisable` and `OnDestroy` also stop and restore an active suite, covering interruption even when coroutine disposal does not execute its `finally` block.
- Runner configuration is disabled in the Inspector while timing is active. Exactly one run button, one report-copy button, and the structural inventory actions remain.
- The legacy `VegetationBenchmark.BeginTimedComparisonSuite()` now rejects execution with a superseded status. Its five serialized V1G controls are hidden, and the legacy Inspector no longer exposes timed run/copy/progress UI. Retained private suite code remains rollback-only until INFRA.3.

#### Weather warning correction

`CS_WeatherWindField.compute::PhysicalCell` no longer performs signed `%`. It uses `uint2` addition and `& (resolution - 1u)`, then returns the wrapped `int2`. The three kernels still use the same helper.

The required invariant remains in unchanged `WeatherWindDomain.OnValidate()`: field resolution is the closest power of two clamped to 32–256. Exhaustive validation compared modulo and bit-mask wrapping for every valid scalar logical coordinate and offset at all four accepted resolutions; every result matched.

`WeatherWindDomain.cs`, `WeatherWindField.hlsl`, `VegetationWindResponse.hlsl`, texture formats, field origin/offset publication, recenter logic, noise evaluation, response integration, dispatch cadence, CPU queries, and scene state are unchanged.

#### Accepted freeze evidence

INFRA.1B remains frozen from the user's two-layer inventory: one active runner, two ready independent layers, 24,327 aggregate instances, 291,924 submitted triangles, and 1,167,696 instance-buffer bytes with exact 12-triangle and 48-byte per-instance accounting.

WEATHER-V0A is frozen from the user's READY report: one active and published domain, 128² resolution, 0.5 m cells, 64 × 64 m coverage, 10 Hz update rate, 327,680 estimated texture bytes, active simulation dispatches, CPU sample availability, and future wind-line consumer availability.

#### Source validation

`VEG-V2-INFRA.2_Source_Validation.txt` reports **56/56 passed**:

- exact zero-created / nine-modified / zero-deleted scope;
- lexical delimiter, preprocessor, and Tree-sitter C# parse success for all four changed C# files;
- no runner frame loop or renderer inheritance;
- both measurement modes, controlled 3 × 2 matrix, shared render-state toggling, captured recipe/render state, `finally` restoration, disable/destroy restoration, conditional restoration rebuild, report save, clipboard action, and Inspector lock;
- rejected legacy timed entry point, hidden legacy controls, removed legacy timed Inspector UI, and direct runner guidance;
- no modulus in `PhysicalCell`, unsigned power-of-two masking, and unchanged use by all three kernels;
- unchanged power-of-two runtime producer contract;
- byte identity of `VegetationRendererBase.cs`, `VegetationLayer.cs`, `GroundVegetation.cs`, vegetation migration tooling, `WeatherWindDomain.cs`, Weather Editor, both Weather/vegetation HLSL consumers, and `VisualFrameworkDemo.unity`;
- exhaustive bitmask/modulo equivalence across all accepted field resolutions.

The complete final versions of all nine changed files were reread. Direct callers and dependencies were reread: `VegetationInfrastructureMigration.CreateOrReuseSceneRunner`, `VegetationRendererBase` diagnostic setters/counters, `VegetationLayer`, `WeatherWindDomain` resolution/offset/dispatch code, `WeatherWindField.hlsl`, and `VegetationWindResponse.hlsl`. The final diff matches the approved plan and contains no undeclared behavior change.

A Unity executable, Unity reference assemblies, the Unity compute compiler, and D3D11 shader import are unavailable in this environment. Unity compilation, absence of the three supplied modulus warnings, actual stack timing, controlled-layer visual isolation, screenshot/report file creation, interruption restoration, and unchanged Weather output are **pending user validation**.

#### Performance reconciliation

Outside an explicitly running suite, the runner performs no scan, allocation, frame callback, draw, dispatch, or rebuild. Enabled-stack timing changes render flags only. Controlled mode performs six selected-layer rebuilds and one conditional restoration rebuild; this is explicit diagnostic dirty-time work.

The Weather correction replaces signed integer modulus per compute cell with unsigned bit masking. It adds no memory, dispatch, branch, simulation step, texture access, or CPU work. No `PERFORMANCE EXCEPTION` is active.

## VEG-V2-INFRA.2A — Arbitrary Production Density and Authored-Configuration Benchmark Coverage

### Status

**Source implementation and compliance audit complete on 2026-07-21; Unity validation pending.** Production layers now retain arbitrary positive integer density values, and the scene-level controlled benchmark now includes the exact authored stack, independent `20/35/50` tiers, and one non-duplicate exact controlled configuration when required.

### Objective and acceptance criteria

Production vegetation density is a recipe value, not a benchmark preset. A layer authored at `37`, `43`, or any other positive integer must retain that exact value through Inspector validation, rebuilding, benchmark capture, and benchmark restoration.

The scene-level benchmark owns independent standard density tiers of `20`, `35`, and `50` clusters/m². In `Controlled Layer Matrix` mode, one complete run must include:

1. one **Current Authored Stack** case that measures all active, enabled, rendering scoped layers exactly as captured, including mixed densities such as `35` and `43` in the same case;
2. one controlled-layer matrix over every current geometry candidate at `20`, `35`, and `50` clusters/m²;
3. one additional **Current Controlled Configuration** case only when the selected layer's exact geometry/density pair is not already represented by the standard matrix;
4. exact restoration of every captured renderer flag plus the controlled layer's original arbitrary density and geometry;
5. no production-density snapping to `12/16/20/35/50` anywhere in normal layer authoring;
6. no added normal-frame work, no additional active renderer, and no change to coverage, geometry generation, instance layout, shaders, Weather, or scene ownership.

### Approved file scope

Modify only:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationRendererBase.cs`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmarkRunner.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkRunnerEditor.cs`

Create, delete, move, rename, scene, prefab, material, shader, HLSL, Weather, coverage, geometry, instance-layout, layer, tag, and URP changes: none authorized.

### Read-only review evidence

- `Assets/AGENTS.md` was read completely. It requires this canonical plan as the first write, exact scope, implementation traceability, complete final reread, caller/consumer comparison, and unavailable Unity checks marked pending.
- `VegetationRendererBase.cs::OnValidate()` currently executes `densityPerSquareMetre = NormalizeDensity(densityPerSquareMetre)`. `NormalizeDensity()` maps every value to `12`, `16`, `20`, `35`, or `50`; therefore `37` becomes `35` and `43` becomes `50` whenever Unity validates the component.
- `VegetationRendererBase.cs::SetDensityPreset()` uses the same normalization, so controlled benchmark restoration also cannot preserve arbitrary production values.
- `VegetationLayerEditor.cs::DrawProductionProperties()` exposes the serialized `densityPerSquareMetre` property directly and rebuilds when its configuration hash changes. No editor-side preset popup exists; the snapping fault is entirely in the renderer base validation path.
- `VegetationLayer.cs::OnValidate()` calls `base.OnValidate()`, so every production layer inherits the snapping fault. `VegetationBenchmark.cs::OnValidate()` also calls the base path, but the transitional benchmark may continue using the same unrestricted positive density contract.
- `VegetationBenchmarkRunner.cs::BuildCases()` currently produces one enabled-stack case only in `EnabledStack` mode, while `ControlledLayerMatrix` produces only `3 geometries × densities 35/50`. It therefore does not include the authored stack in a complete controlled run, does not test density `20`, and does not separately preserve a non-tier selected configuration as a measured case.
- `VegetationBenchmarkRunner.cs::ApplyMeasurementRenderState()` currently selects stack-versus-controlled visibility only from the runner mode. To measure the authored stack and controlled cases in one controlled run, case-level measurement scope must be explicit.
- `VegetationBenchmarkRunner.cs::CaptureLayerState()` already captures exact integer density and geometry. `RestoreCapturedState()` already conditionally restores and rebuilds the selected layer; changing the setter to arbitrary-positive clamping is sufficient to preserve exact values.
- `VegetationBenchmarkRunnerEditor.cs` currently describes the controlled matrix as three geometries at `35` and `50`. Its guidance must match the new authored-stack plus `20/35/50` contract.
- `VegetationBenchmark.cs` contains rollback-only structural matrices at `20/35/50` and direct field restoration. It is not an active caller of the scene runner and requires no change for this update.
- No `.git` directory exists in the supplied workspace. Branch, `HEAD`, history, and working-tree comparison are unavailable. The current source is the accepted INFRA.2 workspace produced from the supplied archive and accepted patch chain.

### Invariants and non-goals

- Production density remains an integer and is clamped only to a concrete safety minimum of `1 cluster/m²`.
- Existing serialized densities `12`, `16`, `20`, `35`, and `50` remain byte-for-value compatible.
- The benchmark standard tiers are diagnostics-owned constants and do not constrain layer authoring.
- The authored-stack case never rebuilds or changes recipe settings.
- The additional authored controlled-layer case performs no rebuild because it measures the already captured configuration before matrix mutation.
- Matrix cases may rebuild only the selected controlled layer.
- Sibling layer recipe settings and coverage are never modified.
- No runtime frame loop, draw, dispatch, allocation, or rebuild is added outside an explicitly running diagnostic coroutine.
- Legacy rollback code is not cleaned up in this patch.

### File-by-file implementation sequence

1. `VegetationRendererBase.cs`
   - add an Inspector minimum of `1` to `densityPerSquareMetre`;
   - replace preset normalization with positive integer clamping;
   - expose `SetDensityPerSquareMetre(int)` as the production/diagnostic setter;
   - retain `SetDensityPreset(int)` as a compatibility forwarding alias until legacy cleanup.
2. `VegetationBenchmarkRunner.cs`
   - define diagnostics-owned tiers `20/35/50`;
   - add case-level measurement scope so one controlled run can alternate authored-stack and isolated-controlled cases;
   - always prepend one exact authored-stack case in controlled mode;
   - add the selected layer's exact current geometry/density case only when no standard matrix case matches it;
   - expand the matrix from `3 × 2` to `3 × 3`;
   - use the arbitrary-density setter for mutation and restoration;
   - keep baseline windows all-render-disabled and preserve complete captured-state restoration.
3. `VegetationBenchmarkRunnerEditor.cs`
   - update Inspector guidance to state that a controlled run includes the authored stack, three geometries at `20/35/50`, and a non-duplicate exact authored selected-layer case.
4. `Stylized_Vegetation_Architecture.md`
   - record arbitrary positive production density and diagnostics-owned standard tiers as the conceptual contract.
5. Complete static validation, exact scope comparison, full final reread, direct caller/consumer audit, and record the post-change evidence in this section.

### Risks and validation

- **Risk:** an authored-stack case inside controlled mode could accidentally isolate the selected layer. **Mitigation:** case-level measurement scope controls render-state application, resource validation, and structural reporting.
- **Risk:** the exact authored controlled-layer case could duplicate a standard matrix case. **Mitigation:** compare both geometry and density before adding the extra case.
- **Risk:** interruption after arbitrary-density mutation could restore through the old snapping alias. **Mitigation:** restoration uses the new arbitrary-positive setter and validates the final integer value structurally.
- **Risk:** extreme authored density can create excessive candidate counts. The user explicitly requested arbitrary density; this patch keeps a minimum only and does not silently invent a maximum. Performance remains the author's responsibility and is visible through candidate, instance, buffer, and timing reports.

Required source checks:

- exact five-file modified scope and zero created/deleted files;
- C# lexical/preprocessor/parser success for all changed C# sources;
- no production preset normalization remains;
- arbitrary values `37` and `43` remain unchanged under normalization/setter logic;
- standard benchmark tiers are exactly `20/35/50`;
- controlled mode includes one authored-stack case and nine matrix cases;
- exact authored controlled case is included only when not duplicated;
- case-level visibility selects the captured stack for the authored-stack case and only the selected layer for controlled cases;
- restoration uses the arbitrary-density setter and retains the exact captured integer;
- no new `Update`, `LateUpdate`, or `FixedUpdate` in the runner;
- frozen coverage, geometry, instance-layout, shaders, Weather, migration tooling, and scene files remain byte-identical.

Unity validation remains pending until the patch is imported into Unity 6000.5.0f1.


### Implementation result

#### Arbitrary production density

`VegetationRendererBase.densityPerSquareMetre` now has an Inspector minimum of `1`. `OnValidate()` calls `ClampDensity()`, which performs only `Mathf.Max(1, density)`. Values such as `37` and `43` therefore remain unchanged.

`SetDensityPerSquareMetre(int)` is the authoritative production and diagnostic mutation API. The previous public `SetDensityPreset(int)` name remains as a compatibility forwarding alias but no longer snaps values. Existing serialized preset-era values remain unchanged.

#### Complete controlled benchmark composition

`VegetationBenchmarkRunner` owns the standard density tiers:

```text
20 / 35 / 50 clusters/m²
```

`ControlledLayerMatrix` now builds cases in this order:

1. `Current Authored Stack` — all captured active, enabled, rendering scoped layers with their exact recipes and mixed densities;
2. `Current Controlled Configuration` — included only when the selected layer's exact geometry/density pair is absent from the standard matrix;
3. nine standard controlled cases — every current geometry candidate at `20`, `35`, and `50` clusters/m².

A selected layer already authored at one standard tier produces ten cases: one stack case plus nine matrix cases. A selected layer authored at a non-tier value such as `43` produces eleven cases by adding one exact controlled case.

Each case now carries explicit measurement scope. Authored-stack vegetation windows restore the captured stack flags; controlled vegetation windows enable only the selected layer. Baseline windows still disable all scoped layers. Resource validation and structural reporting use the same case scope.

Restoration uses `SetDensityPerSquareMetre()` and therefore reapplies the exact captured arbitrary integer. The report identifier and saved filename are versioned as INFRA.2A.

### Post-implementation consistency and compliance audit

`VEG-V2-INFRA.2A_Source_Validation.txt` reports **56/56 passed**:

- exact zero-created / five-modified / zero-deleted scope;
- lexical delimiter, preprocessor, and Tree-sitter C# parse success for all three changed C# files;
- production density minimum `1`, removal of preset snapping, exact representative retention of `37` and `43`, and compatibility alias forwarding;
- diagnostics-owned tiers exactly `20/35/50`;
- one authored-stack case in both modes, nine standard matrix cases in controlled mode, and one conditional non-duplicate authored controlled case;
- case-level stack-versus-controlled rendering, all-disabled baseline behavior, matching resource/structure scope, and exact arbitrary-density restoration;
- no runner `Update`, `LateUpdate`, or `FixedUpdate`;
- updated Inspector guidance and conceptual architecture;
- byte identity of `VegetationLayer`, its editor and coverage field, geometry builder, instance layout, transitional benchmark/editor, Weather runtime and compute field, vegetation shader/include, and `VisualFrameworkDemo.unity`.

The complete final versions of all five changed files were reread. Direct affected callers and consumers were reread: `VegetationLayer.OnValidate()`, `VegetationLayerEditor.DrawProductionProperties()`, `VegetationInfrastructureMigration`'s explicit serialized-property allowlist, transitional `VegetationBenchmark` density capture/restoration, and the runner's mutation/restoration paths. The final diff matches the approved plan and introduces no undeclared subsystem change.

A Unity executable and Unity reference assemblies are unavailable in this environment. Unity import/compilation, Inspector retention of `37`/`43`, ten-versus-eleven timed case execution, exact mixed-stack reporting, screenshot/report saving, interruption restoration, and runtime profiling are **pending user validation**.

### Performance reconciliation

Normal gameplay cost is unchanged. Density remains part of existing dirty-time candidate generation and active instance count; this patch adds no frame callback, draw, dispatch, buffer field, or shader work. Arbitrary high density may intentionally increase candidate count according to `Ground world area × authored density`; no silent maximum was introduced because the user explicitly requested unrestricted recipe values.

The controlled diagnostic matrix increases from six to nine standard rebuild cases and adds one zero-rebuild authored-stack case plus, for non-tier selected recipes, one zero-rebuild exact controlled case. This additional work occurs only during an explicitly started benchmark. No `PERFORMANCE EXCEPTION` is active.

## VEG-V2-INFRA.3A — Safe Live Legacy Retirement

### Status

**SUPERSEDED BEFORE UNITY APPLICATION on 2026-07-21.** This staged retirement transaction was rejected as unnecessary transitional machinery. Its source is deleted by `VEG-V2-INFRA.3`; the user removes the obsolete scene test GameObject directly in Unity.

### Objective and acceptance criteria

1. Provide one scene-level cleanup action on the existing `VegetationBenchmarkRunner` Inspector, plus a non-modifying preview report.
2. Refuse destructive cleanup unless the loaded runner scene contains exactly one benchmark runner, at least one valid production `GroundVegetation` stack with initialized valid layer coverage, exactly one active and published exact `WeatherWindDomain`, no active legacy `VegetationBenchmark`, and no active legacy `VegetationBenchmarkWindProvider`.
3. Clear the legacy Ground coverage byte arrays and authoring state only for Grounds that have a valid production vegetation root and at least one valid direct recipe layer. Retain the current Ground serialized field declarations until INFRA.3B.
4. Clear each production layer's `legacyMigrationSourceId` because rollback source identity is no longer needed after retirement.
5. Remove disabled legacy vegetation renderer and wind-provider components through Undo. Delete an obsolete test GameObject hierarchy only when every object in that hierarchy contains no component other than `Transform`, `VegetationBenchmark`, and `VegetationBenchmarkWindProvider`; otherwise remove only the legacy components and retain unrelated objects and children.
6. Preserve `Systems/Weather`, the exact production `WeatherWindDomain`, `Systems/Diagnostics/Vegetation Benchmark`, all production layers, coverage masks, renderer resources, benchmark settings/reports, and unrelated scene objects.
7. Make the action idempotent. A repeated preview or retirement after successful cleanup must report that no legacy objects or data remain rather than recreating or deleting production infrastructure.
8. Use one Undo group and revert it on transactional failure. Mark the scene dirty only after a successful transaction. Copy both preview and final reports to the clipboard.
9. Add no runtime/frame-loop work, no renderer or Weather behavior change, no raw scene/prefab/material edit, and no layer, tag, shader, compute, HLSL, URP, or geometry change.

### Approved file scope

Create:

- `Assets/Game/Procedural/Vegetation/Editor/VegetationLegacyRetirement.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationLegacyRetirement.cs.meta`

Modify:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Docs/Weather_System_Architecture_Provisional.md`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkRunnerEditor.cs`

Delete, move, rename, runtime C#, scene, prefab, material, shader, compute, HLSL, coverage implementation, geometry, instance-layout, layer, tag, and URP changes: none authorized.

### Read-only review evidence

- `Assets/AGENTS.md` was read completely. It requires this canonical plan as the first write, exact scope, implementation traceability, complete post-change reread, and unavailable Unity checks marked pending.
- The supplied workspace has no `.git` directory. Branch, `HEAD`, status, diff, and history are unavailable. The current source is the accepted INFRA.2A workspace reconstructed from the supplied archive and accepted patch chain.
- `VegetationInfrastructureMigration.MigrateGround()` disables legacy `VegetationBenchmark` components but deliberately retains them and the Ground byte mask for rollback. It records the source identity in `VegetationLayer.legacyMigrationSourceId`.
- `WeatherWindInfrastructureMigration.MigrateLegacyProvider()` disables but does not destroy the exact `VegetationBenchmarkWindProvider` and creates/reuses one exact `WeatherWindDomain` under `Systems/Weather`.
- `VegetationBenchmarkRunnerEditor` is the authoritative scene-level diagnostics Inspector and currently has no cleanup action. The runner itself contains no cleanup responsibility and must remain unchanged.
- `GeneratedGround` still serializes `vegetationCoverageResolution`, `vegetationCoveragePixels`, `vegetationCoverageRevision`, `vegetationCoverageInitialized`, `vegetationCoveragePaintMode`, `vegetationCoverageBrushRadius`, `vegetationCoverageBrushStrength`, `vegetationCoverageEraseMode`, and `showVegetationCoverageOverlay`. INFRA.3A may clear these values through `SerializedObject` without changing the runtime type; declarations are removed only in INFRA.3B.
- `VegetationLayer` exposes `CoverageInitialized`, `CoverageStorageValid`, `SurfaceGround`, `LegacyMigrationSourceId`, and `SetLegacyMigrationSourceId()`, sufficient for preflight and retirement without modifying runtime vegetation code.
- `WeatherWindDomain` exposes `ActiveDomainCount` and `PublishedDomain`; exact base-type comparison separates the production domain from the obsolete compatibility subclass.
- The user's accepted evidence reported two ready production layers, one active runner, and one READY/published Weather domain. INFRA.2A timing remains pending and will run only after cleanup and later dead-code deletion.

### Implementation sequence

| Item | File | Change | Status |
|---|---|---|---|
| INFRA.3A.0 | This document | Record objective, exact scope, evidence, transaction, non-goals, and validation before code changes. | Complete |
| INFRA.3A.1 | `VegetationLegacyRetirement.cs` | Implement scene inventory, strict preflight, preview report, one-group Undo retirement, safe pure-legacy hierarchy deletion, component-only fallback, Ground legacy-state clearing, migration-ID clearing, idempotence, failure rollback, scene dirtying, and clipboard reporting. | Complete at source level; Unity validation pending |
| INFRA.3A.2 | `VegetationBenchmarkRunnerEditor.cs` | Add Legacy Retirement preview/copy/execute controls, disable execution during timing, and require an explicit destructive confirmation dialog. | Complete at source level; Unity validation pending |
| INFRA.3A.3 | Conceptual vegetation and Weather documents | Record accepted freeze evidence, the retirement boundary, and INFRA.3B as the required next cleanup stage before final benchmarking. | Complete |
| INFRA.3A.4 | Approved files and frozen dependencies | Run exact-scope, syntax, serialized-property, preflight, Undo, idempotence, no-runtime-loop, and byte-identity checks; record Unity validation as pending. | Complete at source level; Unity validation pending |

### Transaction and rollback

The retirement utility inventories the runner's loaded scene before opening Undo. Preflight validates the production replacements and every serialized Ground property that will be cleared. The action then opens one Undo group, records each Ground and production layer, clears legacy Ground storage/authoring state and migration source IDs, removes disabled legacy components, and deletes only pure-legacy object hierarchies. An exception reverts the complete Undo group and copies a failure report. Successful completion collapses the group, marks the scene dirty, preserves the runner and production systems, and copies the final report.

### Performance and memory model

The new utility is Editor-only and runs only on explicit button presses. Preview performs bounded scene inventory and validation. Retirement clears temporary duplicate Ground masks and disabled compatibility components, reducing serialized scene data and editor object count. It adds no active-gameplay compute, draw, dispatch, allocation, mask sample, hierarchy scan, or frame callback. No `PERFORMANCE EXCEPTION` is active.

### Validation plan

Static validation must prove exact file scope, C# parse/preprocessor balance, unique `.meta` GUID, no runtime/frame-loop code, strict exact-domain and inactive-legacy checks, complete serialized Ground property allowlist, one Undo rollback path, pure-legacy hierarchy protection, component-only fallback, migration-ID clearing, idempotence, scene dirtying, and byte identity of runtime vegetation, Weather, compute, shader, HLSL, and source scene files.

Unity validation must confirm compile/import, a READY preview report, successful retirement with the obsolete test hierarchy removed, preserved production layer/Weather/runner behavior, Undo/redo, and a saved cleaned scene. INFRA.3B must not delete legacy script or field definitions until that evidence is supplied.


### VEG-V2-INFRA.3A post-change consistency and compliance audit

#### Actual affected files

Created exactly as declared:

- `Assets/Game/Procedural/Vegetation/Editor/VegetationLegacyRetirement.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationLegacyRetirement.cs.meta`

Modified exactly as declared:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Docs/Weather_System_Architecture_Provisional.md`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkRunnerEditor.cs`

Deleted, moved, renamed, runtime C#, scene, prefab, material, shader, compute, HLSL, coverage implementation, geometry, instance-layout, layer, tag, and URP files: none.

#### Implemented behavior

- The scene-level runner Inspector now exposes `Preview Legacy Retirement`, `Retire Legacy Vegetation and Wind Infrastructure`, and one clipboard action for the latest report.
- Preview is non-modifying and reports `READY`, `BLOCKED`, or `ALREADY RETIRED`. Cleanup controls are disabled during Play Mode and during a timed suite; execution also requires an explicit confirmation dialog.
- Preflight inventories the complete runner scene with unsorted `FindObjectsByType` calls. It requires exactly one runner including inactive objects, at least one valid direct `GeneratedGround > Vegetation [GroundVegetation] > [VegetationLayer]` stack, initialized valid layer coverage, no layers outside a valid production stack, exactly one exact production `WeatherWindDomain`, that domain as the only active and published Weather domain, and no active legacy renderer or compatibility wind provider.
- The Ground serialized-property allowlist contains all nine current legacy coverage/authoring fields. Retirement clears the byte array, initialization/paint/erase/overlay state, resets legacy brush values, and increments the legacy revision only on Grounds with valid production stacks.
- Production `VegetationLayer.legacyMigrationSourceId` values are cleared through the existing runtime API with Undo recording. Production coverage, recipes, instances, rendering flags, and hierarchy are unchanged.
- A GameObject hierarchy is deleted only when every component in the complete hierarchy is `Transform`, `VegetationBenchmark`, or the exact legacy wind-provider type. Any object with another component or a missing component is retained and only its disabled legacy components are removed.
- The transaction uses one Undo group, records Grounds and layers, uses Undo destruction for objects/components, reverts completely on exception, marks the scene dirty only after success, preserves the production runner and Weather domain, and is idempotent without creating an empty Undo transaction after cleanup.
- The compatibility wind-provider type is detected by exact full type name instead of a direct obsolete C# type reference, avoiding a new obsolete-symbol compiler warning in the cleanup utility.
- Legacy script definitions, migration utilities, Ground declarations/methods, and obsolete Inspector sections remain until INFRA.3B. This is intentional so the live scene can be cleaned and saved before source deletion.

#### Source validation

`VEG-V2-INFRA.3A_Source_Validation.txt` reports **66/66 passed**:

- exact two-created / five-modified / zero-deleted scope and unique valid `.meta` GUID;
- lexical delimiter, preprocessor, and Tree-sitter C# parse success for both changed C# files;
- Editor-only utility with no `Update`, `LateUpdate`, or `FixedUpdate`;
- no direct obsolete compatibility type reference;
- strict runner, hierarchy, coverage, Weather, and inactive-legacy preflight;
- exact nine-property Ground legacy allowlist and complete reset operations;
- Undo recording, full rollback, pure-hierarchy protection, component-only fallback, scene dirtying, clipboard reporting, and idempotence;
- Inspector preview, confirmation, execution, timing/Play-Mode lock, and report-copy actions;
- byte identity of all runtime vegetation files, runner runtime, legacy runtime components, migration producers, Weather runtime/editor/migration, compute/HLSL/shader files, `GeneratedGround` runtime/editor, and `VisualFrameworkDemo.unity`.

The complete final versions of the new utility, changed runner Inspector, and four changed architecture documents were reread. Direct producers and consumers were reread: `VegetationInfrastructureMigration`, `WeatherWindInfrastructureMigration`, `VegetationBenchmark`, `VegetationBenchmarkWindProvider`, `VegetationLayer`, `GroundVegetation`, `VegetationBenchmarkRunner`, `WeatherWindDomain`, and the legacy Ground serialized state and Inspector section. The final diff matches the approved plan and contains no undeclared behavior change.

A Unity executable and Unity reference assemblies are unavailable in this environment. Unity import/compilation, live scene preflight, serialized Ground clearing, object/component Undo destruction, pure-hierarchy selection, Undo/redo, scene saving, and preserved production vegetation/Weather behavior are **pending user validation**.

#### Performance reconciliation

The utility and its Inspector controls are Editor-only. They add no active frame callback, scan, draw, dispatch, rebuild, buffer, texture, or shader work. Successful retirement removes temporary serialized Ground mask bytes and disabled compatibility scene components. Runtime vegetation, Weather, and benchmark code are byte-identical to the INFRA.2A baseline. No `PERFORMANCE EXCEPTION` is active.

---

## VEG-V2-INFRA.3 — Production-Only Vegetation Cleanup

### Status

**Authorized and active on 2026-07-21.** This update supersedes the unvalidated `VEG-V2-INFRA.3A` staged retirement utility. The user will delete the obsolete scene test GameObject directly; source must contain no migration, rollback, compatibility, or retirement machinery for deleted systems.

### Objective and acceptance criteria

Retain only the current production vegetation architecture and its active diagnostics:

```text
GeneratedGround
└── Vegetation                         [GroundVegetation]
    └── recipe GameObjects             [VegetationLayer]

Systems
├── Weather                            [WeatherWindDomain]
└── Diagnostics
    └── Vegetation Benchmark           [VegetationBenchmarkRunner]
```

Acceptance requires:

1. Delete the legacy combined `VegetationBenchmark`, its custom editor, and the vegetation-owned Weather compatibility subclass.
2. Delete vegetation migration, Weather migration, and rejected live-retirement utilities.
3. Remove legacy Ground-owned vegetation mask storage, sampling, painting, snapshot, migration, and Inspector code.
4. Remove migration identifiers and import-only APIs from `VegetationLayer` and `VegetationCoverageField`.
5. Remove renderer fallback-domain, optional-Ground, legacy Ground-mask, diagnostic-subclass, and preset-density compatibility paths that have no production consumer after the legacy renderer is deleted.
6. Preserve useful recipe creation and duplicate-as-empty behavior in one production-named Editor utility.
7. Preserve `VegetationLayer`, `GroundVegetation`, `VegetationBenchmarkRunner`, `WeatherWindDomain`, geometry construction, instance layout, shaders, compute field, layer painting, arbitrary densities, and stack-aware benchmarking.
8. Do not raw-edit scenes, prefabs, materials, shaders, layers, tags, or URP assets.

### Read-only review evidence

- The supplied workspace contains no `.git` directory; branch, `HEAD`, status, diff, and history are unavailable.
- `VegetationBenchmark.cs` is the only remaining subclass that consumes `ForceFallbackPlacementDomain`, `FallbackPlacementOwnerLabel`, the default Ground-mask coverage hooks, `DiagnosticOperationRunning`, and `StopDiagnosticOperations`. The scene runner is independent and does not inherit `VegetationRendererBase`.
- `VegetationInfrastructureMigration.cs` has only two still-useful post-migration responsibilities: create an empty recipe layer and duplicate a recipe as an empty layer. These will move to `VegetationLayerAuthoring` with the explicit production-property copy allowlist.
- `VegetationLayer.legacyMigrationSourceId`, `ImportCoverage`, `SetLegacyMigrationSourceId`, and `VegetationCoverageField.ImportLegacy` are referenced only by migration/retirement utilities.
- `GeneratedGround` legacy vegetation fields and methods are referenced only by the deleted legacy renderer, migration/retirement utilities, and the obsolete Ground Inspector migration section. Production `VegetationCoverageField` already uses the generic `TryGetSurfaceDomain`, `TryWorldToSurfaceNormalizedXZ`, `TryGetSurfaceWorldPosition`, and `TryRaycastGeneratedSurface` APIs.
- `WeatherWindInfrastructureMigration` is referenced only by the legacy-type branch in `WeatherWindDomainEditor`. Production Weather diagnostics and reset actions do not depend on it.
- `VegetationLegacyRetirement` is referenced only by the scene runner Inspector section introduced by the rejected INFRA.3A patch.
- `SetDensityPreset` has no caller; `SetDensityPerSquareMetre` is the current arbitrary-density benchmark control.

### Approved file scope

Create:

```text
Assets/Game/Procedural/Vegetation/Editor/VegetationLayerAuthoring.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationLayerAuthoring.cs.meta
```

Modify:

```text
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Docs/Stylized_Vegetation_Architecture.md
Assets/Docs/Weather_Wind_Architecture.md
Assets/Docs/Weather_System_Architecture_Provisional.md
Assets/Game/Procedural/Vegetation/VegetationRendererBase.cs
Assets/Game/Procedural/Vegetation/VegetationLayer.cs
Assets/Game/Procedural/Vegetation/VegetationCoverageField.cs
Assets/Game/Procedural/Vegetation/Editor/GroundVegetationEditor.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationLayerEditor.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkRunnerEditor.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationRendererEditorPreview.cs
Assets/Game/Procedural/Ground/GeneratedGround.cs
Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
Assets/Game/Procedural/Weather/Editor/WeatherWindDomainEditor.cs
```

Delete:

```text
Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs
Assets/Game/Procedural/Vegetation/VegetationBenchmarkWindProvider.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationInfrastructureMigration.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationInfrastructureMigration.cs.meta
Assets/Game/Procedural/Vegetation/Editor/VegetationLegacyRetirement.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationLegacyRetirement.cs.meta
Assets/Game/Procedural/Weather/Editor/WeatherWindInfrastructureMigration.cs
Assets/Game/Procedural/Weather/Editor/WeatherWindInfrastructureMigration.cs.meta
```

The live Unity project must also delete the `.meta` files paired with the three older legacy scripts when they exist; those metas are absent from the supplied archive.

### Invariants and non-goals

- Every production layer requires the nearest `GeneratedGround` ancestor and always uses that Ground's complete surface domain and height field.
- Every production layer always evaluates its own independent `VegetationCoverageField`; no full-field fallback or Ground-owned mask remains.
- Density remains any positive integer. The runner alone owns `20/35/50` standard tiers.
- Layer painting retains no-rebuild stamps and exactly one rebuild at the end of a changed stroke.
- Duplicate-as-empty copies recipe and brush settings but not coverage pixels or runtime resources.
- The scene-level runner remains diagnostics-only and retains all INFRA.2A state restoration behavior.
- Weather simulation and ownership remain unchanged.
- Historical names such as `VegetationBenchmarkGeometry` and the current shader asset name are retained because they are active serialized/runtime contracts, not obsolete systems.
- The obsolete scene `VegetationTest` object is deleted manually through Unity before or immediately after applying the source patch; scene YAML is not edited by this patch.

### File-by-file implementation sequence

| Item | File(s) | Work | Status |
|---|---|---|---|
| INFRA.3.0 | This document | Record exact evidence, scope, invariants, deletion boundary, and validation before code edits. | Complete |
| INFRA.3.1 | `VegetationLayerAuthoring.cs`, `GroundVegetationEditor.cs`, `VegetationLayerEditor.cs` | Extract useful create/duplicate/copy behavior from the migration utility and update callers. | Complete at source level; Unity validation pending |
| INFRA.3.2 | `VegetationRendererBase.cs`, `VegetationLayer.cs`, `VegetationCoverageField.cs` | Remove fallback, legacy coverage, diagnostic-subclass, import, migration-ID, and compatibility APIs; preserve production build/render behavior. | Complete at source level; Unity validation pending |
| INFRA.3.3 | `GeneratedGround.cs`, `GeneratedGroundEditor.cs` | Delete Ground-owned vegetation mask state and migration UI; retain and rename generic surface-domain/raycast infrastructure used by layer coverage. | Complete at source level; Unity validation pending |
| INFRA.3.4 | `WeatherWindDomainEditor.cs`, `VegetationBenchmarkRunnerEditor.cs`, `VegetationRendererEditorPreview.cs` | Remove migration, rejected retirement UI, and the deleted diagnostic-ownership preview exclusion while preserving production actions, timed benchmark UI, and Edit-Mode rendering. | Complete at source level; Unity validation pending |
| INFRA.3.5 | Deleted files | Remove legacy renderer/editor, wind subclass, migration utilities, and retirement utility. | Complete at source level; live scene object deletion pending |
| INFRA.3.6 | Architecture documents | Mark INFRA.3A superseded and record production-only final ownership. | Complete |
| INFRA.3.7 | Final audit | Verify exact scope, C# structure, reference closure, serialized production field preservation, no legacy symbols, no scene/shader/runtime Weather changes, and pending Unity compilation. | Complete at source level; 75/75 passed |

### Performance contract

This patch removes inactive compatibility code and serialized Ground mask storage. It adds no runtime loop, draw, sample, allocation, or GPU resource. Production placement still evaluates one authored mask sample and one Ground height sample per rebuild candidate. Active rendering remains one indirect draw per enabled layer. The default 128² Ground legacy mask removal saves 16,384 serialized bytes per Ground after Unity resaves the scene; the layer-owned 128² mask remains unchanged.

### Validation requirements

- Every retained C# file must pass structural parser and preprocessor checks.
- No retained source may reference deleted types or legacy Ground vegetation APIs.
- `VegetationLayerAuthoring` must copy the complete current production recipe allowlist exactly once and exclude coverage storage/runtime state.
- `VegetationRendererBase` must have no fallback placement domain, Ground-owned coverage sampling, `SetDensityPreset`, or diagnostic-subclass hooks.
- `GeneratedGround` must retain generic surface-domain and generated-surface raycast APIs while containing no `vegetationCoverage*` fields or methods.
- Frozen geometry, instance layout, shader, compute, Weather runtime, benchmark runner runtime, and scene files must remain byte-identical unless explicitly listed.
- Unity import, missing-script cleanup, serialization stripping, and live benchmark execution remain pending until applied in Unity 6000.5.0f1.



### VEG-V2-INFRA.3 post-change consistency and compliance audit

**Source implementation and audit complete on 2026-07-21; Unity import and runtime validation pending.**

Actual scope matched the declared amended scope exactly:

- 14 modified files;
- 2 created files;
- 9 supplied files deleted;
- no undeclared project file changed.

The live project must additionally delete the `.meta` files paired with `VegetationBenchmark.cs`, `VegetationBenchmarkEditor.cs`, and `VegetationBenchmarkWindProvider.cs` if those metas exist. They were absent from the supplied source archive and therefore could not appear in the reconstructed deletion diff.

Implemented final ownership:

- `VegetationLayer` is the only `VegetationRendererBase` subclass and always resolves its nearest `GeneratedGround` ancestor.
- Every layer always uses the Ground surface domain and its own `VegetationCoverageField`; no rectangle fallback, optional Ground path, or Ground-owned mask remains.
- `VegetationLayerAuthoring` retains only production create-empty and duplicate-as-empty behavior. Its 42-property copy allowlist exactly matches all current serialized production recipe fields except the hierarchy-derived Ground reference.
- `GeneratedGround` retains only generic surface-domain, normalized-XZ, editor surface-position, generated-surface raycast, and surface-revision APIs required by layer authoring.
- `VegetationBenchmarkRunner` remains the sole timing/inventory diagnostic and was byte-identical to the accepted INFRA.2A baseline.
- `WeatherWindDomain` remains the sole Weather publisher and was byte-identical to the accepted WEATHER-V0A baseline.
- The rejected scene-retirement UI, vegetation migration UI, Weather migration UI, migration IDs, import APIs, legacy combined renderer/editor, and vegetation-owned wind subclass are absent.

`VEG-V2-INFRA.3_Source_Validation.txt` reports **75/75 passed**. The validation covered:

- exact modified, added, and deleted file scope;
- lexical C# structure and preprocessor balance for every changed source and every retained vegetation C# file;
- exact retained vegetation source inventory;
- complete absence of deleted types, migration/retirement terminology, legacy Ground coverage identifiers, fallback-domain APIs, diagnostic-subclass hooks, and preset-density compatibility;
- exact 42-property production authoring allowlist and exclusion of coverage/runtime storage;
- hierarchy-owned Ground validation, safe invalid-domain reporting, layer-owned sampling, and one-rebuild-per-stroke structure;
- retention of all generic Ground surface APIs required by layer coverage and painting;
- byte identity for the scene runner runtime, `GroundVegetation`, geometry builder, 48-byte instance contract, Weather runtime, compute field, Weather/vegetation HLSL, vegetation shader, and source scene.

No Unity executable or Unity reference assemblies were available in this environment. Unity compilation, serialized-field stripping, missing-script cleanup, Edit-Mode rendering, layer painting, Weather publication, and timed benchmark execution remain pending. Before importing this source deletion, remove the obsolete `VegetationTest` GameObject from the live scene if it still exists; otherwise Unity will correctly display missing-script components for the deleted classes until that object is removed.

## VEG-V2-FOUNDATION.1 — CrossedCards production freeze and benchmark validity

**Status:** Source complete; Unity import and validation pending.

### Objective

Freeze the user-selected and structurally cheapest `CrossedCards` cluster as the only production vegetation geometry, remove the unused `OpaqueStrips` and `Hybrid` generation/benchmark paths, retain arbitrary positive per-layer density, and prevent the timed benchmark from presenting invalid or physically meaningless performance rankings.

### Acceptance criteria

- `VegetationLayer` exposes no geometry candidate and always builds the accepted three-card crossed cluster.
- `VegetationClusterMeshBuilder` contains only the production CrossedCards construction path: three cards, 18 vertices, and 12 triangles per cluster.
- The vegetation shader always applies the accepted card silhouette taper/clip path and contains no geometry-candidate material property or runtime branch.
- Controlled benchmarking includes one exact current-authored-stack case, standard selected-layer density cases at `20`, `35`, and `50` clusters/m², and one additional exact selected-layer density only when it is not a standard tier.
- The runner no longer captures, mutates, or restores geometry.
- A target-resolution mismatch invalidates performance ranking. Timing samples remain available as measurements at the actual resolution but cannot select a winner.
- Negative enabled-minus-disabled deltas are labelled as baseline fluctuation and never treated as separated vegetation cost or as a performance win.
- Existing coverage, deterministic placement, 48-byte instance layout, Weather response, lighting, layer painting, arbitrary density, and state-restoration behavior remain unchanged.

### Exact approved scope

Modify:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationRendererBase.cs`
- `Assets/Game/Procedural/Vegetation/VegetationClusterMeshBuilder.cs`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmarkRunner.cs`
- `Assets/Game/Procedural/Vegetation/GroundVegetation.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationLayerAuthoring.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkRunnerEditor.cs`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

Create, delete, move, rename, scene, prefab, material, Weather, coverage, instance-layout, layer, tag, and URP changes: none authorized.

### Read-only review evidence

- `Assets/AGENTS.md` was read completely. It requires this canonical plan as the first write, exact scope, complete final reread, cross-subsystem shader audit, and unavailable Unity checks marked pending.
- The supplied workspace contains no `.git` directory. Branch, `HEAD`, status, diff, and history are unavailable. The current source was reconstructed from `Assets-Code-Archive(8).zip` plus the accepted INFRA.1B, WEATHER-V0A, INFRA.2, INFRA.2A, and INFRA.3 patch chain.
- Both user-supplied INFRA.2A reports completed `10 / 10` cases, performed nine controlled structural rebuilds, and restored all renderer flags and controlled recipe settings. The reports measured `CrossedCards` at 12 triangles per cluster, `Hybrid` at 24, and `OpaqueStrips` at 32.
- The user had already selected CrossedCards visually. The accepted authored stack contains 28,282 CrossedCards instances and 339,384 submitted triangles. The same instance count would submit 678,768 Hybrid triangles or 905,024 OpaqueStrips triangles by direct multiplication.
- Both timed runs were executed at `509 × 1285` rather than the required `2560 × 1440`, in Unity Editor Play Mode on an RTX 3080 Ti. Their reports explicitly state that timing applies only to the measured resolution.
- `VegetationBenchmarkRunner.IsSeparated()` currently uses `abs(delta) >= noise`, so negative enabled-minus-disabled deltas can be labelled separated. The Grass_Default report ranked negative GPU deltas as apparent wins even though they represent paired-window baseline fluctuation rather than vegetation cost.
- `VegetationRendererBase` currently serializes `VegetationBenchmarkGeometry`, includes it in rebuild hashing, passes it to `VegetationClusterMeshBuilder`, and publishes `_GeometryCandidate` to the material.
- `VegetationClusterMeshBuilder` currently contains three candidate paths. CrossedCards uses three cards, producing 18 vertices and 12 triangles; the OpaqueStrips and Hybrid methods are otherwise unneeded by production.
- `SH_StylizedVegetationBenchmark.shader` uses `_GeometryCandidate > 0.5` to select the accepted card taper/clip path. Removing the C# geometry property without hardwiring this path would render uncut rectangular cards, so the shader must change in the same patch.
- `VegetationLayerAuthoring` still copies the serialized `geometry` property, and `GroundVegetation` plus `VegetationBenchmarkRunner` still report geometry. These are direct consumers of the field and are included in scope.
- Direct dependencies reviewed and intentionally unchanged include `VegetationLayer.cs`, `VegetationCoverageField.cs`, `VegetationInstanceData.cs`, `VegetationRendererEditorPreview.cs`, `VegetationLayerEditor.cs`, `VegetationCommon.hlsl`, `VegetationWindResponse.hlsl`, and `VegetationLighting.hlsl`.

### Implementation sequence

| Item | File | Change | Status |
|---|---|---|---|
| FOUNDATION.1.0 | This document | Record scope, evidence, invariants, shader impact, implementation, and validation before code changes. | Complete |
| FOUNDATION.1.1 | `VegetationClusterMeshBuilder.cs` | Delete the geometry enum and unused strip/hybrid builders; retain one fixed three-card CrossedCards mesh path. | Complete at source level; Unity validation pending |
| FOUNDATION.1.2 | `VegetationRendererBase.cs`, `VegetationLayerAuthoring.cs`, `GroundVegetation.cs` | Remove serialized geometry ownership, API, hashing, material publication, recipe-copy entry, and variable-geometry reporting. | Complete at source level; Unity validation pending |
| FOUNDATION.1.3 | `SH_StylizedVegetationBenchmark.shader` | Remove `_GeometryCandidate` and always execute the accepted card silhouette taper and clip behavior. | Complete at source level; Unity validation pending |
| FOUNDATION.1.4 | `VegetationBenchmarkRunner.cs`, editor | Reduce controlled cases to density tiers plus a non-duplicate exact density, remove geometry mutation/restoration, invalidate ranking on resolution mismatch, and reject negative deltas as cost evidence. | Complete at source level; Unity validation pending |
| FOUNDATION.1.5 | Conceptual documentation | Freeze CrossedCards and replace historical active guidance that still describes concurrent production candidates. | Complete |
| FOUNDATION.1.6 | Approved files and direct dependencies | Run exact-scope, parser/preprocessor, symbol/reference, mesh-contract, shader-branch, case-count, ranking-validity, restoration, and dependency-identity checks; record Unity validation as pending. | Complete at source level; Unity validation pending |

### Performance and compatibility model

Removing candidate geometry does not increase active runtime cost. The production mesh remains the accepted 18-vertex / 12-triangle CrossedCards cluster. The shader removes one uniform and two dynamic geometry-condition checks while retaining the exact CrossedCards taper/clip equations. Layer instance counts, 48-byte buffers, one indirect draw per enabled layer, placement candidate count, and dirty-time rebuild scaling remain unchanged.

Existing serialized `geometry` values become unused stale YAML entries after the field is removed; Unity ignores unknown serialized fields. No scene or prefab text edit is required. The retained shader asset name is historical only and is not renamed in this patch.

### Validation plan

Static validation must prove exact nine-file scope, C# parse/preprocessor balance, absence of `VegetationBenchmarkGeometry`, `OpaqueStrips`, `Hybrid`, `SetGeometryCandidate`, and `_GeometryCandidate` from active vegetation source/shader, fixed mesh counts of 18 vertices / 12 triangles, density tiers `20 / 35 / 50`, controlled case counts of four for a standard authored density and five for a non-tier density, no geometry capture/restoration, positive-only separation logic, explicit target-resolution ranking invalidation, and unchanged instance layout, coverage, Weather, HLSL includes, and scene files.

Unity validation must confirm clean import/compile, existing layers preserve visual CrossedCards output and arbitrary densities, inventory reports fixed production geometry/structural totals, controlled benchmark case counts are correct, settings restore after completion/interruption, and a non-2560 × 1440 run states that performance ranking is invalid.


### VEG-V2-FOUNDATION.1 post-change consistency and compliance audit

#### Actual affected files

Modified exactly as declared:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationRendererBase.cs`
- `Assets/Game/Procedural/Vegetation/VegetationClusterMeshBuilder.cs`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmarkRunner.cs`
- `Assets/Game/Procedural/Vegetation/GroundVegetation.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationLayerAuthoring.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkRunnerEditor.cs`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

Created, deleted, moved, renamed, scene, prefab, material, Weather, coverage, instance-layout, layer, tag, and URP files: none.

#### Implemented behavior

- `VegetationBenchmarkGeometry`, `OpaqueStrips`, `Hybrid`, and `SetGeometryCandidate()` are removed from active vegetation source.
- `VegetationClusterMeshBuilder.Build()` always creates the accepted three-card cluster. Its fixed structural contract is 18 vertices and 12 triangles.
- `VegetationRendererBase` no longer serializes, hashes, exposes, or publishes a geometry candidate. Existing unknown serialized `geometry` entries are ignored by Unity after import.
- `_GeometryCandidate` is removed from the shader property block and material CBUFFER. The former CrossedCards taper, pre-clip derivative, silhouette clip, lighting, wind, and edge-accent path now executes unconditionally.
- `Tip Width Ratio` and `Taper Start` are material-only controls under the sole CrossedCards path. They moved from the structural rebuild hash into the material hash and are updated through `RefreshLightingMaterialProperties()`.
- Production recipe duplication copies all 41 active serialized production properties except hierarchy-owned `coverageGround`; there is no geometry property.
- Layer-stack and scene-inventory reports identify the fixed CrossedCards geometry and preserve density, instance, triangle, buffer, coverage, and readiness telemetry.
- Controlled benchmarking now produces four cases for a standard selected density: one current authored stack plus `20 / 35 / 50`. A non-tier density produces five cases by adding one exact current controlled-density case without a redundant rebuild.
- The runner captures and restores only renderer flags and exact controlled density. Geometry mutation/restoration and six unused geometry cases are gone.
- Target resolution is captured at suite start. A run outside `2560 × 1440` prints `Performance ranking: INVALID`, retains unordered timing rows for the actual resolution, and forbids winner selection.
- Separation requires a finite positive delta at least as large as the combined noise estimate. Negative deltas are labelled `INCONCLUSIVE — NEGATIVE DELTA / BASELINE FLUCTUATION` and never rank as wins.

#### Cross-subsystem shader impact audit

The modified shader is created at runtime only by `VegetationRendererBase` through `Shader.Find("PS3D/Vegetation/Stylized Vegetation Benchmark")`. No material, prefab, other subsystem source, or additional shader references this shader asset directly. `VegetationCommon.hlsl`, `VegetationWindResponse.hlsl`, and `VegetationLighting.hlsl` remain byte-identical. The change therefore affects only production vegetation layers and removes the inactive candidate selector while preserving the previously selected CrossedCards branch equations.

#### Source validation

`VEG-V2-FOUNDATION.1_Source_Validation.txt` reports **68/68 passed**:

- exact nine-modified / zero-created / zero-deleted scope;
- Tree-sitter C# parse and preprocessor balance for all six changed C# files;
- absence of all removed geometry symbols from active vegetation source and shader;
- fixed three-card, 18-vertex, 12-triangle mesh contract;
- material-only tip/taper hashing and refresh behavior;
- unconditional card taper and clip shader path with balanced delimiters;
- exact `20 / 35 / 50` density tiers and deterministic four/five-case construction;
- density-only capture/restoration;
- positive-only separation, negative-delta classification, and target-resolution ranking invalidation;
- complete 41-property production recipe-copy allowlist matched exactly against active serialized renderer fields;
- byte identity of layer coverage, 48-byte instance layout, editor preview/painting, all vegetation HLSL includes, Weather runtime/compute, and the supplied source scene.

Unity 6000.5.0f1 is unavailable in this environment. Shader import, Unity compilation, existing-scene deserialization, visual identity, timed-run case counts, interruption restoration, and runtime reports remain pending and require the validation steps supplied with the patch.

## VEG-V2-INTERACT.1 — Multi-Actor Immediate Grass Displacement

**Status:** source implementation complete and statically validated; Unity validation pending. Direction shaping and displaced-grass Weather composition are refined by `VEG-V2-INTERACT.1A`.

### Objective

Add one scene-owned, camera/anchor-centred XZ interaction domain that converts active `VegetationInteractor` components into a bounded immediate deformation field. Every production `VegetationLayer` samples the same field in the vertex shader. Ordinary actors bend and temporarily flatten grass while present or moving, then the response recovers without writing persistent trail state.

This update implements immediate displacement only. Short-lived authored trails, ability-driven irregular-area/line stamps, timed recovery measured in seconds or minutes, permanent trampling, Ground-owned history, and save/load are deferred to `VEG-V2-INTERACT.2`.

### Accepted controls and ownership

- Immediate interaction domain owner: one scene-level `VegetationInteractionDomain`.
- Immediate field space: moving world-space XZ domain centred on an explicit anchor or fallback camera-ground projection.
- Immediate field update-rate control: `5–60 Hz`, default `20 Hz`.
- Future Ground-owned trail/trample field update-rate control: also `5–60 Hz`; its default and memory representation remain deferred to INTERACT.2.
- Ordinary player/enemy interactors do not create persistent history by default.
- Any object may receive `VegetationInteractor`; no player-specific dependency is introduced.
- Actor movement is represented by a swept capsule between the previous and current interaction samples so low update rates do not create spatial gaps.
- Shader cost remains fixed with respect to actor count: active actors are resolved into one shared field before rendering; vegetation never loops over actors.

### Read-only review evidence

- The reconstructed source is `Assets-Code-Archive(8).zip` overlaid in accepted order with INFRA.1B, WEATHER-V0A, INFRA.2, INFRA.2A, INFRA.3 deletion state, and FOUNDATION.1. The supplied workspace contains no `.git` directory, so branch, `HEAD`, status, diff, and history are unavailable.
- `VegetationRendererBase.cs` owns production material properties, conservative local bounds, structural/material hashes, one indirect draw per layer, and the fixed CrossedCards runtime material. It has no interaction controls or shader properties.
- `SH_StylizedVegetationBenchmark.shader` currently applies only `ApplyVegetationWindResponse`; its analytical normal and bend-side shading use the Weather tip displacement only.
- `VegetationWindResponse.hlsl` samples the shared Weather response field and returns world-space tip displacement. It remains unchanged; interaction is composed as a separate field and response include.
- `VegetationLayerEditor.cs` automatically draws all visible base serialized recipe fields, compares structural and material hashes, rebuilds only for structural changes, and refreshes the runtime material for material-only changes. No editor change is required for the new recipe controls.
- `VegetationLayerAuthoring.cs` copies an explicit production-property allowlist; new interaction response fields must be added there so duplicate-as-empty preserves the complete recipe.
- `WeatherWindDomain.cs`, `WeatherWindField.hlsl`, and `CS_WeatherWindField.compute` establish the accepted bounded-field pattern: explicit anchor/camera resolution, power-of-two toroidal recentering, fixed-rate update accumulation, global shader publication, and no hierarchy scan. Weather remains a separate owner and is not modified.
- Existing canonical sections 8.1–8.5 and quality-tier guidance are stale: they state that ordinary visible actors leave trails and restrict immediate/persistent rates to `4–12 Hz` and `2–8 Hz`. This update must replace those active statements rather than append contradictory guidance.

### Approved file scope

Create:

```text
Assets/Game/Procedural/Vegetation/VegetationInteractionDomain.cs
Assets/Game/Procedural/Vegetation/VegetationInteractionDomain.cs.meta
Assets/Game/Procedural/Vegetation/VegetationInteractor.cs
Assets/Game/Procedural/Vegetation/VegetationInteractor.cs.meta
Assets/Game/Procedural/Vegetation/Editor/VegetationInteractionDomainEditor.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationInteractionDomainEditor.cs.meta
Assets/Game/Rendering/Vegetation/Includes/VegetationInteractionField.hlsl
Assets/Game/Rendering/Vegetation/Includes/VegetationInteractionField.hlsl.meta
Assets/Game/Rendering/Vegetation/Resources.meta
Assets/Game/Rendering/Vegetation/Resources/PS3DVegetation.meta
Assets/Game/Rendering/Vegetation/Resources/PS3DVegetation/Compute.meta
Assets/Game/Rendering/Vegetation/Resources/PS3DVegetation/Compute/CS_VegetationInteractionField.compute
Assets/Game/Rendering/Vegetation/Resources/PS3DVegetation/Compute/CS_VegetationInteractionField.compute.meta
```

Modify:

```text
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Docs/Stylized_Vegetation_Architecture.md
Assets/Game/Procedural/Vegetation/VegetationRendererBase.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationLayerAuthoring.cs
Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader
```

Delete, move, rename, scene, prefab, material, Weather, Ground, coverage, instance-layout, layer, tag, and URP files: none.

Scope amendment before folder-meta creation: Visible Meta Files requires committed `.meta` files for the new `Resources`, `PS3DVegetation`, and `Compute` folders. These three folder metas are added to the create list; no implementation behavior changes.

### Implementation contract

1. `VegetationInteractor` maintains a static active registry and non-serialized previous fixed-sample position. Serialized controls define radius, bend strength, flatten strength, movement-direction influence, full movement-response speed, teleport/sweep limit, and priority. It derives motion from transforms and requires no Rigidbody, CharacterController, player type, tag, or layer.
2. `VegetationInteractionDomain` maintains exactly one published scene field, a reusable filtered/sorted actor list, one fixed-stride GPU actor buffer, two `ARGBHalf` response textures, a power-of-two toroidal offset, and a `5–60 Hz` fixed-step accumulator. It does not call `FindObjectsByType` in its update loop.
3. The compute field evaluates a swept capsule for each uploaded actor, records normalized bend/flatten response, applies separate attack and recovery time constants, preserves overlapping cells during recenter, and clears newly exposed cells. INTERACT.1A expands direction shaping from the original radial/movement path to explicit `Radial`, fixed `World X Biased`, and `Hybrid` modes. It contains no persistence beyond the short immediate response.
4. The shader samples previous/current response textures and interpolates between fixed updates. INTERACT.1A samples this interpolated state once before Weather, uses it to attenuate Weather according to the recipe, then applies interaction deformation. The root remains planted; horizontal bend and vertical flattening are weighted by blade height. Weather and interaction tip displacements are combined for deformation normals and bend-side shading.
5. Per-layer recipe controls are material-only except maximum interaction bend, which participates in conservative bounds and therefore in the structural hash. Duplicate-as-empty copies every interaction response control.
6. The domain Inspector exposes reset and clipboard-report actions. Selected domain/interactor gizmos show field bounds, anchor, radius, and movement direction without GPU readback.

### Initial defaults and bounds

| Control | Initial value | Allowed range |
| --- | ---: | ---: |
| Field resolution | 256² | 64²–512² power of two |
| Cell size | 0.25 m | 0.10–2.0 m |
| World coverage | 64 × 64 m | derived |
| Recenter margin | 1.5 m | 0.25–8 m, clamped below half-field extent |
| Update rate | 20 Hz | 5–60 Hz |
| Maximum fixed steps/frame | 4 | 1–8 |
| Maximum uploaded interactors | 48 | 1–96 |
| Immediate response time | 0.06 s | 0.01–1.0 s |
| Immediate recovery time | 0.18 s | 0.01–2.0 s |
| Sweep tail retention | 0.10 | 0–1 |
| Default actor radius | 0.55 m | 0.05–20 m |
| Default actor bend strength | 1.0 | 0–2 |
| Default actor flatten strength | 0.20 | 0–1 |
| Default maximum layer bend | 0.65 m | 0–3 m |

### Performance model

At 256², two `ARGBHalf` response textures consume `2 × 256 × 256 × 8 = 1,048,576` bytes. The current 48-record actor buffer uses `48 × 48 = 2,304` bytes after INTERACT.1A direction shaping. One compute dispatch runs per fixed interaction step; each field cell loops only over the uploaded actor count. Rendering adds two bilinear field samples per vegetation vertex. INTERACT.1B keeps active-cell interpolation and adds analytical release decay without another texture read, actor loop, CPU per-instance update, coverage rebuild, or instance-buffer mutation.

The first patch deliberately chooses direct bounded actor iteration over spatial binning. The accepted capacity ceiling is 48 ordinary records, and the report exposes uploaded/overflow counts. Spatial binning is justified only if measured interaction compute cost exceeds the budget under the documented stress scene.

### Validation plan

INTERACT.1 static validation proved the original exact 32-byte actor record, power-of-two field constraints, `5–60 Hz` clamps, no per-frame hierarchy scan, no persistent/trail state, no instance-layout change, complete recipe-copy coverage, conservative bounds inclusion, and fixed Weather/interaction composition. INTERACT.1A supersedes only the actor-record and direction-composition portions with the current 48-byte record and single-sample Weather-suppression path.

Unity validation must confirm clean import/compile, one active domain, player-agnostic interactor registration, stationary radial parting, continuous movement displacement at 10 Hz, no lingering trail after recovery, independent per-layer response controls, correct field recentering, restoration after domain disable, and a copied comprehensive report.

### Implementation sequence

| Item | Files | Status |
| --- | --- | --- |
| INTERACT.1.0 | Canonical plan and stale-guidance replacement | Complete |
| INTERACT.1.1 | `VegetationInteractor.cs` | Complete at source level; Unity validation pending |
| INTERACT.1.2 | `VegetationInteractionDomain.cs`, editor | Complete at source level; Unity validation pending |
| INTERACT.1.3 | Compute resource and HLSL sampling include | Complete at source level; Unity validation pending |
| INTERACT.1.4 | Renderer controls, recipe-copy allowlist, shader composition and bounds | Complete at source level; Unity validation pending |
| INTERACT.1.5 | Conceptual document reconciliation | Complete |
| INTERACT.1.6 | Source validation and post-change audit | Complete at source level; Unity validation pending |


### VEG-V2-INTERACT.1 post-change audit

**Actual scope:** matched the approved scope exactly: thirteen files created, five files modified, and no files deleted. The three folder `.meta` files added by the pre-write scope amendment are included in that count. No scene, prefab, material, Ground, Weather, coverage, instance-layout, benchmark-runner, layer, tag, or URP file changed.

**Implemented runtime contract:**

- `VegetationInteractor` is transform-driven, player-agnostic, and has no `Update`, `LateUpdate`, or `FixedUpdate`. It registers explicitly and supplies a fixed-sample swept capsule with radius, bend, temporary flattening, movement-direction influence, teleport limit, and priority.
- `VegetationInteractionDomain` owns one published moving XZ field, accepts a `5–60 Hz` fixed update rate, defaults to `20 Hz`, uploads at most the configured actor capacity, recentres toroidally, interpolates previous/current fixed-step responses in the shader, and stores no trail or gameplay-history state.
- At the INTERACT.1 freeze, the compute kernel used two `float4` values / 32 bytes per actor. INTERACT.1A expands the current contract to three `float4` values / 48 bytes for direction mode and fixed-axis controls; unsigned power-of-two wrapping, bounded actor iteration, and response/recovery timing remain unchanged.
- Every production layer receives material controls for bend, flattening, root-to-tip response, maximum world-space bend, and interaction-normal response. Maximum bend is a final shader cap and conservative bounds expansion; the other response controls are material-only.
- Weather deformation remains separate. The shader applies Weather and then interaction, combines both horizontal tip displacements for lighting, and includes temporary flattening in the deformation normal while respecting a zero interaction-normal response.
- Duplicate-as-empty copies the complete production recipe including all five interaction-response controls.

**Performance reconciliation:** the default 256² field uses two `ARGBHalf` textures (`1,048,576` bytes total) plus a 48-record structured actor buffer (`1,536` bytes). Active CPU work is registry filtering, deterministic priority/distance sorting, one bounded upload, and fixed-step dispatch scheduling. Active GPU field work is one 256² compute dispatch per interaction step with at most the uploaded actor count per cell. Vegetation adds two bilinear field samples per vertex but no actor loop, CPU instance mutation, coverage mutation, or vegetation rebuild. No `PERFORMANCE EXCEPTION` is active; Unity profiling under the documented stress scene remains mandatory before increasing field resolution or actor capacity.

**Validation evidence:** the INTERACT.1 source validator passed `104 / 104` checks for its original radial/movement contract. INTERACT.1A separately validates the current 48-byte direction record, fixed-world-X modes, independent world-Z attenuation, one interaction sample path, and displaced-grass Weather control while preserving the original field, sweep, bounds, and frozen dependencies. Unity assemblies and an executable Unity Editor are unavailable in this environment, so Unity compilation, shader/compute import, runtime dispatch, visual response, recentering, and profiling remain pending and are not claimed.

## VEG-V2-INTERACT.1A — World-X Direction Bias and Displaced-Grass Wind Suppression

**Status:** source implementation complete; post-change source audit passed; Unity validation pending.

### Objective

Refine the immediate interaction field without adding history. Each `VegetationInteractor` must choose between the accepted current radial behavior and fixed-map-axis side-biased behavior. World-X-biased interaction must favor displacement along world `±X`, independently attenuate world-Z displacement, and remain independent of actor facing, actor movement direction, and camera rotation. Each vegetation recipe must also control how much Weather deformation remains while that recipe is currently displaced.

This patch does not add short-lived trails, ability stamps, timed recovery history, permanent trampling, Ground-owned interaction textures, save/load, or actor-specific gameplay dependencies. Those remain owned by `VEG-V2-INTERACT.2`.

### Acceptance criteria

1. `VegetationInteractor` exposes `Radial`, `World X Biased`, and `Hybrid` direction modes.
2. `World X Bias` and `World Z Strength` are explicit `0..1` controls. The axis basis is fixed world X/Z; actor rotation and movement direction do not redefine it.
3. `Radial` preserves the accepted current radial plus optional movement-directed behavior.
4. `World X Biased` ignores movement direction for displacement direction and blends the radial result toward a stable `±X` result. `Hybrid` starts from the accepted radial/movement result and then applies the same world-X bias.
5. Grass near the interactor X centreline receives a deterministic world-space left/right choice rather than an unstable zero vector or flickering split.
6. Every `VegetationLayer` exposes `Wind Influence On Displaced Grass`: `1` preserves current Weather response; values toward `0` suppress Weather proportionally to effective interaction bend/flatten strength.
7. Interaction is sampled once per vertex path for both wind suppression and deformation. The existing previous/current field interpolation remains two texture samples total, not four.
8. The interaction field remains transient, actor bounded, and independent from vegetation instance rebuilding.

### Read-only review evidence

- The authoritative source was reconstructed from `Assets-Code-Archive(8).zip` plus the accepted INFRA.1B, WEATHER-V0A, INFRA.2, INFRA.2A, INFRA.3 deletion state, FOUNDATION.1, and INTERACT.1 patches. The workspace has no `.git` directory; branch, `HEAD`, status, diff, and history are unavailable.
- `VegetationInteractor.cs::VegetationInteractorSample` currently contains start/end XZ, radius, bend, flatten, speed-derived movement blend, and priority. The serialized actor controls provide only radial/movement shaping.
- `VegetationInteractionDomain.cs::GpuInteractorRecord` and `CS_VegetationInteractionField.compute::VegetationInteractorRecord` are exactly two `float4` values / 32 bytes. The fourth parameter currently stores movement blend; no world-axis mode or axis controls exist.
- `CS_VegetationInteractionField.compute::EvaluateImmediateTarget` computes radial displacement and optionally blends it toward swept movement direction. It has no fixed world-X basis or deterministic centreline side selection.
- `SH_StylizedVegetationBenchmark.shader::Vert` applies Weather before calling `ApplyVegetationInteractionResponse`; interaction is therefore unavailable when Weather displacement is calculated.
- `VegetationInteractionField.hlsl::SampleVegetationInteraction` already returns the interpolated bend and flatten field. Passing that sample into the deformation function allows both wind suppression and interaction deformation without an additional field sample.
- `VegetationRendererBase.cs` owns material-only interaction response properties and hashes. `VegetationLayerAuthoring.cs` copies an explicit production-property allowlist; the new recipe control must be added to both paths.
- Current active documentation still describes radial plus movement-directed parting as the ordinary behavior and does not document fixed world-X mode or displaced-grass Weather suppression. Those active sections must be replaced in place and the INTERACT.1 contract marked refined by INTERACT.1A.

### Approved file scope

Modify:

```text
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Docs/Stylized_Vegetation_Architecture.md
Assets/Game/Procedural/Vegetation/VegetationInteractor.cs
Assets/Game/Procedural/Vegetation/VegetationInteractionDomain.cs
Assets/Game/Procedural/Vegetation/VegetationRendererBase.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationLayerAuthoring.cs
Assets/Game/Rendering/Vegetation/Includes/VegetationInteractionField.hlsl
Assets/Game/Rendering/Vegetation/Resources/PS3DVegetation/Compute/CS_VegetationInteractionField.compute
Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader
```

Create, delete, move, rename, scene, prefab, material, Weather implementation, Ground, coverage, geometry, instance-layout, layer, tag, and URP files: none.

### Implementation contract

1. Add a serialized public direction-mode enum and `World X Bias` / `World Z Strength` controls to `VegetationInteractor`. Preserve speed-derived movement blend for `Radial` and `Hybrid`; `World X Biased` ignores it for direction shaping.
2. Expand the CPU/GPU interaction record from 32 to 48 bytes using a third `float4`: mode, world-X bias, world-Z strength, and reserved zero. Update stride validation and memory reporting.
3. In compute, retain the current swept-capsule footprint and falloff. Build a deterministic world-X side from the signed radial X offset; within a narrow centreline band use a stable world-cell hash. Blend the mode source direction toward pure world `±X` by `World X Bias`, then multiply the resulting world-Z component by `World Z Strength` before final normalization. This makes the two controls independent: `World Z Strength = 0` always removes final Z displacement.
4. Refactor `VegetationInteractionField.hlsl` so the shader samples once, derives effective interaction strength, and passes the sample into position deformation.
5. Add material-only `Wind Influence On Displaced Grass` to `VegetationRendererBase`, its lighting hash, runtime material upload, shader property/CBUFFER, and duplicate-recipe allowlist.
6. In the vertex shader, sample interaction before Weather, derive effective layer displacement strength from bend and flatten responses, attenuate the complete Weather-deformed position and full-tip Weather displacement, then apply interaction deformation and combined normal logic.
7. Replace the active interaction documentation in both architecture documents and mark prior radial-only guidance as refined, not concurrently authoritative.

### Performance and compatibility model

The actor record grows from 32 to 48 bytes. At the default 48-interactor capacity the buffer grows from `1,536` to `2,304` bytes, an increase of `768` bytes. Compute dispatch dimensions, field textures, actor loop ceiling, update cadence, field sampling count, draw count, instance buffer, and placement rebuild behavior remain unchanged. Shader work adds bounded scalar/vector shaping and a Weather interpolation multiplier; interaction texture sampling remains the existing previous/current pair.

Existing serialized actors receive enum zero (`Radial`), `World X Bias = 0.85`, and `World Z Strength = 0.20` from field initializers for newly added data. Because enum zero preserves the current direction path, existing scene behavior remains radial until the user selects another mode. Existing vegetation recipes receive `Wind Influence On Displaced Grass = 1`, preserving current Weather behavior.

### Validation plan

Static validation must prove exact file scope, C#/HLSL/compute delimiter and preprocessor balance, exact 48-byte CPU/GPU record agreement, enum/mode parity, `0..1` clamps, stable centreline selection, no actor-relative basis in world-X mode, one interaction sample call in the vertex path, complete recipe-copy coverage, material-only hashing, unchanged field textures/update limits, and byte identity of Ground, Weather, instance layout, geometry, coverage, benchmark runner, and scene files.

Unity validation must confirm radial parity, fixed world-X behavior independent of actor rotation/movement direction, controllable world-Z inhibition, stable centreline splitting, `Wind Influence On Displaced Grass = 1` parity, visible suppression toward `0`, immediate recovery with no trail, and exact control locations.

### Implementation sequence

| Item | Files | Status |
| --- | --- | --- |
| INTERACT.1A.0 | Canonical plan and active-guidance replacement | Complete |
| INTERACT.1A.1 | Interactor mode and 48-byte upload contract | Complete at source level; Unity validation pending |
| INTERACT.1A.2 | Compute world-X shaping and centreline stability | Complete at source level; Unity validation pending |
| INTERACT.1A.3 | Single-sample shader refactor and wind suppression | Complete at source level; Unity validation pending |
| INTERACT.1A.4 | Recipe control, hash, material upload, duplication | Complete at source level; Unity validation pending |
| INTERACT.1A.5 | Documentation reconciliation | Complete |
| INTERACT.1A.6 | Source validation and final compliance audit | Complete at source level; Unity validation pending |


### VEG-V2-INTERACT.1A post-change consistency and compliance audit

**Actual scope:** exactly the nine approved files were modified; no files were created, deleted, moved, or renamed. Ground, Weather runtime/compute, coverage, vegetation instance data, cluster geometry, benchmark runner, scene, prefab, material, layer, tag, and URP files remained byte-identical to the reconstructed accepted baseline.

**Implemented behavior:**

- `VegetationInteractor` now exposes `Direction Mode`, `World X Bias`, and `World Z Strength`. Enum zero is `Radial`, preserving existing serialized behavior. `World X Biased` ignores actor movement direction; `Hybrid` starts from the accepted radial/movement result. All fixed-axis shaping uses world X/Z and no actor-forward or camera-relative basis.
- The actor upload contract is three `Vector4` / `float4` values, 48 bytes. The added vector stores mode, world-X bias, world-Z strength, and reserved zero. At the default capacity of 48 records, buffer memory is `2,304` bytes.
- The compute field preserves the existing swept-capsule footprint, influence falloff, overlap accumulation, response/recovery, toroidal recentering, and actor ceiling. Near the exact X centreline, a deterministic hash of the absolute world field cell chooses `-X` or `+X`.
- `World X Bias` blends the applicable source direction toward pure world `±X`. `World Z Strength` then multiplies the final biased Z component before normalization, so zero always removes Z displacement independently of X bias.
- The shader samples the interpolated interaction state once. That sample determines effective per-recipe displacement strength, attenuates the complete Weather-deformed position and full-tip Weather vector, and is then passed directly into immediate deformation. The previous/current interaction field remains exactly two bilinear texture reads total.
- `Wind Influence On Displaced Grass` is a material-only recipe control. Its default `1` preserves the accepted Weather response. At full effective interaction, `0` can remove Weather displacement; intermediate values retain the requested fraction. The control participates in the material hash/upload and duplicate-as-empty allowlist but not the structural rebuild hash.

**Performance reconciliation:** field texture memory, compute dispatch dimensions, actor loop ceiling, `5–60 Hz` cadence, draw count, vegetation instance layout, and placement rebuild behavior are unchanged. The actor buffer increases by `768` bytes at the default 48-record capacity. Shader texture sampling does not increase; added work is bounded direction shaping plus one Weather-retention interpolation. No `PERFORMANCE EXCEPTION` is active.

**Validation evidence:** the dedicated INTERACT.1A source validator passed `72 / 72` checks. It verified exact scope, delimiter/preprocessor balance, enum parity, `0..1` controls and clamps, exact 48-byte CPU/GPU layout, stable world-cell centreline selection, actor-movement independence in World-X mode, independent final-Z suppression, swept-capsule preservation, unchanged field resources/update range, one interaction sample call, two total field texture reads, material-only Weather-retention hashing, complete recipe copying, numerical direction invariants, documentation reconciliation, and byte identity of thirteen frozen dependencies. Unity 6000.5.0f1 and Unity reference assemblies are unavailable here; C# compilation, shader/compute import, scene serialization, visual direction response, gust suppression, and runtime profiling remain pending and are not claimed.


## VEG-V2-INTERACT.1B — Recenter Hysteresis and Low-Cadence Immediate Release Compensation

**Status:** Planned and authorized. Canonical plan recorded before implementation.

### Objective and acceptance criteria

Preserve the accepted INTERACT.1A immediate-displacement behavior at low update rates while removing two avoidable costs/artifacts:

1. ordinary anchor movement must not dispatch a full-field recenter at almost every simulation step;
2. grass behind a moving immediate interactor must begin visible recovery without waiting for stale fixed-step interpolation to complete;
3. the correction must not increase the `5–60 Hz` update rate, add persistent history, add actor loops to the vegetation shader, add field textures, change vegetation instance data, or change the existing interactor/recipe contracts.

Acceptance requires:

- a serialized `Recenter Margin` control on `VegetationInteractionDomain`, clamped to the usable half-field extent;
- no recenter while the anchor remains inside the current field-centre margin;
- one accumulated toroidal recenter when the margin is exceeded;
- a serialized `Sweep Tail Retention` control where `1` preserves the old uniform swept capsule and `0` allows the previous sweep endpoint to contribute zero target strength while the current endpoint remains full;
- current endpoint occupancy and stationary interactor behavior remain unchanged;
- releasing cells use analytical render-time decay controlled by the existing `Immediate Recovery Time` rather than interpolating from the older stronger field;
- active cells retain previous/current interpolation for smooth low-rate response;
- diagnostics distinguish Edit Mode inactivity from runtime failure and report recenter margin, sweep-tail retention, render-time recovery compensation, and recenter-dispatch percentage.

### Read-only review evidence

- User runtime reports for accepted INTERACT.1A showed `112 / 128 = 87.5%` and `312 / 339 ≈ 92.0%` recenter-to-simulation dispatch ratios at `15 Hz`, with a 256² field and 0.25 m cells. `VegetationInteractionDomain.Update` calls `RecenterIfNeeded()` every rendered frame, and `ComputeDesiredOriginCell()` centres the field to the anchor at single-cell precision; any 0.25 m cell crossing therefore dispatches `RecenterField`.
- `CS_VegetationInteractionField.compute::EvaluateImmediateTarget` computes `segmentT` but applies the same target influence along the complete previous-to-current swept capsule. At low cadence, the previous endpoint is therefore held as strongly as the actor's current endpoint for the entire next fixed interval.
- `CS_VegetationInteractionField.compute::SimulateField` stores zero in state `.w`; no current-target support marker is available to the renderer.
- `VegetationInteractionField.hlsl::SampleVegetationInteraction` always returns `lerp(previousState, currentState, interpolation)`. When a cell begins recovery, the older stronger state continues contributing for the full interpolation interval before the next fixed step.
- `VegetationInteractionDomain.PublishShaderGlobals` currently publishes interpolation, fixed-step duration, simulation time, and enabled state. Interaction HLSL does not consume simulation time, so the third timing component can carry immediate recovery time without another global vector.
- `VegetationInteractionDomainEditor` uses `DrawDefaultInspector`, so domain controls require no bespoke serialized-property code. Its current runtime status labels non-playing resources as `NOT READY`, which is ambiguous despite Edit Mode intentionally clearing the field.
- `VegetationInteractor`, `VegetationRendererBase`, the vegetation shader, Weather, Ground, coverage, instance data, geometry, benchmark runner, scenes, prefabs, materials, layers, tags, and URP settings do not need modification for this correction.

### Approved file scope

Modify only:

```text
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Docs/Stylized_Vegetation_Architecture.md
Assets/Game/Procedural/Vegetation/VegetationInteractionDomain.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationInteractionDomainEditor.cs
Assets/Game/Rendering/Vegetation/Includes/VegetationInteractionField.hlsl
Assets/Game/Rendering/Vegetation/Resources/PS3DVegetation/Compute/CS_VegetationInteractionField.compute
```

Create, delete, move, rename, scene, prefab, material, interactor, vegetation recipe, Weather, Ground, coverage, instance-layout, geometry, benchmark, layer, tag, and URP files: none.

### Implementation contract

1. Add `Recenter Margin` under the immediate field controls, exposed across `0.25–8 m` and dynamically clamped below the field half-extent. Initial resource creation remains exactly centred. Runtime recentering compares the anchor cell with the current field centre and keeps the current origin while both axis deltas remain inside the margin. When exceeded, recenter to the latest centred origin in one toroidal shift.
2. Add `Sweep Tail Retention` under immediate response, range `0–1`, default `0.10`. Compute multiplies moving swept-capsule influence by a smooth previous-to-current weight from that retention to `1`. Stationary capsules do not apply the longitudinal attenuation.
3. Store current target support in state `.w` during `SimulateField`. The support marker is current-step metadata, not historical trample state.
4. Publish `(interpolation, fixedStep, recoveryTime, enabled)` through `_VegetationInteractionFieldTiming`.
5. In `SampleVegetationInteraction`, active/supporting cells retain previous/current interpolation. Releasing cells start from the latest committed current state and apply `exp(-renderAge / recoveryTime)` every rendered frame, where `renderAge = interpolation × fixedStep`. This changes no field sample count.
6. Update the report/editor wording and recenter gizmos to describe the actual hysteretic field bounds.
7. Update active documentation in place and freeze INTERACT.1A as user-validated, while retaining persistent/history interaction as deferred INTERACT.2 work.

### Performance and compatibility model

- Recenter hysteresis reduces full 256² recenter dispatch frequency; it adds only bounded CPU integer comparisons per rendered frame.
- Sweep-tail attenuation adds a few scalar operations inside the existing bounded compute actor loop.
- Render-time recovery adds one exponential for releasing sampled vertices; field reads remain exactly two bilinear samples and there is still no actor loop in the vegetation shader.
- Field memory, actor-buffer memory, dispatch dimensions, draw count, instance data, placement rebuild behavior, and update-rate range remain unchanged.
- `Sweep Tail Retention = 1` reproduces the prior uniform swept capsule. Existing response/recovery controls remain authoritative. No `PERFORMANCE EXCEPTION` is active.

### Validation plan

Static validation must prove exact six-file scope, C#/HLSL/compute balance, dynamic margin clamp, no resource rebuild hash dependency for tuning controls, hysteresis before recenter dispatch, stationary-sweep parity, tail weight endpoints, state-W support marker, unchanged two field reads, active interpolation versus release decay branches, recovery-time timing publication, report fields, documentation reconciliation, and byte identity of frozen dependencies.

Unity validation must confirm at 10–15 Hz: normal interaction parity, substantially reduced recenter-dispatch percentage, continuous fast-motion coverage, visibly earlier grass recovery behind the actor, no durable trail, `Sweep Tail Retention = 1` old-behavior parity, and Edit Mode report status `INACTIVE — PLAY MODE SIMULATION NOT RUNNING`.

### Implementation sequence

| Item | Files | Status |
| --- | --- | --- |
| INTERACT.1B.0 | Canonical plan and active-guidance update | Complete |
| INTERACT.1B.1 | Recenter margin and hysteretic origin selection | Complete at source level; Unity validation pending |
| INTERACT.1B.2 | Swept-tail target attenuation | Complete at source level; Unity validation pending |
| INTERACT.1B.3 | Current-support marker and render-time recovery | Complete at source level; Unity validation pending |
| INTERACT.1B.4 | Diagnostics/editor wording and gizmo reconciliation | Complete at source level; Unity validation pending |
| INTERACT.1B.5 | Documentation reconciliation | Complete |
| INTERACT.1B.6 | Source validation and final compliance audit | Complete at source level; Unity validation pending |


### VEG-V2-INTERACT.1B post-change consistency and compliance audit

**Actual scope:** exactly the six approved files were modified. No files were created, deleted, moved, or renamed. `VegetationInteractor`, vegetation recipes/renderer/shader, Weather, Ground, coverage, instance layout, geometry, benchmark runner, scenes, prefabs, materials, layers, tags, and URP files remained byte-identical to the accepted INTERACT.1A baseline.

**Implemented behavior:**

- `VegetationInteractionDomain` exposes `Recenter Margin`, default `1.5 m`. Resource creation still centres the 256² field exactly. Runtime origin selection retains the current origin while the anchor remains within the rounded-up margin in both axes, then shifts once to the latest centred origin. The margin is dynamically clamped to remain at least two cells inside the half-field extent.
- `Sweep Tail Retention`, default `0.10`, is uploaded through `_ResponseParameters.z`. Moving swept capsules apply a smooth previous-to-current multiplier from the configured retention to full strength. Stationary actor footprints bypass this attenuation. `1` reproduces the former uniform swept target; `0` allows the previous endpoint to release completely while current actor occupancy remains full.
- The compute state `.w` now carries a binary current-target-support marker. It is transient interpolation metadata only and does not add trail history.
- `_VegetationInteractionFieldTiming` now publishes interpolation, fixed-step duration, immediate recovery time, and enabled state. Supported cells retain previous/current interpolation. Unsupported cells use the latest committed current state and apply exact exponential decay over the rendered fraction of the next fixed interval. The existing compute recovery update and render-time continuation obey the same time constant.
- Runtime reports now distinguish intentional Edit Mode inactivity from failure and report margin, sweep retention, release compensation, and cumulative recenter percentage. Selected-domain gizmos display the actual hysteretic field bounds, its central recenter margin, and the current anchor.

**Performance reconciliation:** field memory, actor-buffer memory, update cadence, compute thread dimensions, actor ceiling, draw count, vegetation instance data, and field sample count remain unchanged. Hysteresis is bounded CPU integer math and is expected to remove most recenter compute dispatches observed in the accepted 15 Hz reports. Sweep weighting adds bounded scalar math inside the existing compute actor loop. Releasing vertices add one exponential after the existing two field reads; there is no shader actor loop or extra texture sample. No `PERFORMANCE EXCEPTION` is active.

**Consistency reconciliation:** active architecture sections and the initial-default table now describe the 48-byte INTERACT.1A actor record, recenter margin, sweep-tail retention, and low-cadence release behavior. The concise architecture document was updated in place. Historical plans remain labelled by patch identity and do not override the active interaction contract. INTERACT.1A is frozen as user-validated for fixed-world-X shaping and displaced-grass wind suppression; INTERACT.2 remains the separate owner of intentional trail/ability history.

**Validation evidence:** the dedicated INTERACT.1B source validator passed `84 / 84` checks. It verified exact scope, delimiter/preprocessor balance, dynamic margin clamping, absence of resource-hash churn, hysteretic origin selection, accumulated recenter deltas, stationary-sweep parity, sweep-weight endpoints, support metadata, unchanged two-texture sampling, active interpolation and analytical release branches, exact exponential continuation, diagnostic reconciliation, documentation consistency, and byte identity of seventeen frozen dependencies. Unity 6000.5.0f1 and Unity reference assemblies are unavailable in this environment; C# compilation, compute/shader import, actual recenter reduction, visual wake reduction, and runtime profiling remain pending and are not claimed.

## VEG-V2-INTERACT.2A — Ground-Owned Opt-In Trample History

### Objective and acceptance criteria

Add intentional historical grass deformation without changing the accepted INTERACT.1B ordinary-player behavior. Immediate displacement remains scene-owned, camera-anchored, and history-free. Historical trample state is opt-in per `VegetationInteractor`, fixed to one `GeneratedGround` domain, shared by every recipe under that Ground, and updated at an independently configurable `5–60 Hz`.

Acceptance requires:

- `Trail Mode = Off` by default and therefore no historical player trail unless explicitly enabled;
- `Timed` trails recover using the interactor-authored recovery time;
- `Session Persistent` trails remain until the field, Ground, or scene resets and do not claim save-file persistence;
- moving trail writers use their own sample history and swept-capsule stamping, so immediate and historical cadences cannot corrupt one another;
- all direct vegetation layers on one Ground sample one shared fixed Ground-local field;
- no coverage mutation, vegetation rebuild, instance-buffer rewrite, per-cluster CPU loop, or shader-side actor loop;
- per-layer bend, flatten, wind-retention, normal, height, and maximum-bend controls;
- one clipboard report and selected-domain bounds gizmo;
- ability-driven irregular discs and line/capsule requests remain deferred to INTERACT.2B.

### Approved scope

Create:

```text
Assets/Game/Procedural/Vegetation/VegetationTrampleDomain.cs
Assets/Game/Procedural/Vegetation/VegetationTrampleDomain.cs.meta
Assets/Game/Procedural/Vegetation/Editor/VegetationTrampleDomainEditor.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationTrampleDomainEditor.cs.meta
Assets/Game/Rendering/Vegetation/Includes/VegetationTrampleField.hlsl
Assets/Game/Rendering/Vegetation/Includes/VegetationTrampleField.hlsl.meta
Assets/Game/Rendering/Vegetation/Resources/PS3DVegetation/Compute/CS_VegetationTrampleField.compute
Assets/Game/Rendering/Vegetation/Resources/PS3DVegetation/Compute/CS_VegetationTrampleField.compute.meta
```

Modify:

```text
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Docs/Stylized_Vegetation_Architecture.md
Assets/Game/Procedural/Vegetation/GroundVegetation.cs
Assets/Game/Procedural/Vegetation/Editor/GroundVegetationEditor.cs
Assets/Game/Procedural/Vegetation/VegetationInteractor.cs
Assets/Game/Procedural/Vegetation/VegetationRendererBase.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationLayerAuthoring.cs
Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader
```

No scene, prefab, material, `GeneratedGround`, coverage, instance-layout, cluster-geometry, Weather, immediate-field, layer/tag, or URP file is authorized.

### Reviewed evidence and constraints

- `Assets/AGENTS.md` was read completely. It requires this persistent plan as the first write, exact scope, source/caller/consumer review, a final compliance audit, and honest Unity-only pending checks.
- `VegetationInteractionDomain.Update` and `VegetationInteractor.CaptureSample` prove that the accepted immediate domain consumes one cadence-specific previous-position history. A historical domain running independently cannot reuse that mutable history; `VegetationTrampleDomain` therefore owns a separate per-interactor probe/stamp history without changing the accepted immediate sampler.
- `GeneratedGround.TryGetSurfaceDomain` and `TryWorldToSurfaceNormalizedXZ` expose the fixed square Ground domain. The historical field may map world positions through the Ground transform without changing Ground code.
- `VegetationRendererBase.SubmitIndirectRender` owns each layer's runtime material immediately before draw. This is the correct point to bind the trample textures and Ground mapping per layer, allowing multiple Grounds without global texture ownership.
- `VegetationLayerEditor.DrawProductionProperties` iterates inherited visible serialized properties, so new recipe controls require no editor-file change. `VegetationLayerAuthoring` uses an explicit allowlist and must copy every new recipe control.
- The immediate field remains two global interpolated ARGBHalf textures and is not modified. The historical field uses its own two fixed-domain ARGBHalf textures and one bounded trail-writer buffer.

### Implementation sequence

1. Replace active documentation that still labels `GroundVegetation` as coordination-only and V4 as wholly future work. Preserve the immediate/history ownership distinction.
2. Keep the accepted immediate sampler unchanged. Add `Off`, `Timed`, and `Session Persistent` trail modes plus radius, bend, flatten, recovery, minimum speed, stamp spacing, and priority controls; store the independent trail probe/stamp history inside each Ground trample domain.
3. Add `VegetationTrampleDomain` on the existing `Vegetation` root. Resolve the nearest `GroundVegetation` and `GeneratedGround`; allocate a fixed Ground-domain field only in Play Mode; collect only opt-in moving writers; upload bounded swept-capsule records; update at `5–60 Hz`; and expose reset/report/gizmo diagnostics.
4. Add a compute field whose original INTERACT.2A state stored horizontal bend, flattening, and a per-cell exponential recovery rate. **Superseded by INTERACT.2A.1:** timed state now uses a separate delay/duration timing texture and smooth eased recovery; session-persistent state remains explicit and dominant.
5. Bind the correct domain textures and world-to-Ground mapping to each layer material immediately before draw. Add a shader include that samples the field once and combines historical deformation with Weather and immediate interaction.
6. Add material-only trample bend/flatten/height/normal/wind-retention controls plus one structural maximum-bend control. Expand conservative bounds by immediate plus trample maxima and copy all controls during duplicate-as-empty.
7. Add the Ground-root Inspector action to add/select the optional trample domain, and include field status in the Ground stack report.
8. Run exact-scope, parser/delimiter/preprocessor, CPU/GPU stride, cadence, separate-history, no-allocation steady-loop, mapping, recovery, shader-sample, property/hash/upload/copy, frozen-dependency, and documentation-consistency checks. Record the post-change audit here.

### Performance and memory model

At the default `256²` resolution, two ARGBHalf historical textures require `256 × 256 × 8 × 2 = 1,048,576` bytes. A bounded 64-byte record for 48 opt-in trail writers requires `3,072` bytes. Compute work is one `256²` dispatch only on fixed historical steps, not every rendered frame. The vertex shader adds two bilinear historical texture reads per vegetation vertex and no actor loop. Ordinary interactors with `Trail Mode = Off` do not enter the historical candidate list.

### Non-goals and risks

- No ability-stamp API, irregular edge noise, cone, disc, ellipse, or line attack is included; INTERACT.2B owns those writers.
- No save/load persistence is claimed. `Session Persistent` lasts only while the runtime field exists.
- No automatic component or scene mutation occurs. Existing Ground vegetation roots receive an Inspector action to add the optional field explicitly.
- The field is square because the current Ground surface contract is square. A future chunked world may replace this with chunk-owned fields.
- Multiple Grounds are supported by per-material binding; duplicate domains targeting the same Ground are invalid and reported.

### Validation requirements

Static validation must prove exact scope, no immediate-field byte changes, domain-owned separate trail histories, default trail mode Off, `5–60 Hz` clamp, fixed Ground mapping, exact CPU/GPU record stride, no per-frame hierarchy scan, no coverage/rebuild calls, timed and session-persistent recovery semantics, one trample sample in the vertex path, complete material hash/upload/copy coverage, bounds expansion, and byte identity of frozen Ground, Weather, coverage, instance-layout, geometry, immediate compute/include, benchmark runner, and scene files.

Unity validation must confirm: ordinary player parity with no trail; a large timed writer leaves a continuous trail and recovers; a session-persistent writer remains until Reset Field; 5/10/15/60 Hz controls are accepted; two recipes share the footprint but respond independently; disabling/removing the domain returns historical response to zero; and the clipboard report matches the configured Ground and writer counts.

### Status

| Item | Status |
| --- | --- |
| INTERACT.2A.0 Canonical plan and active documentation reconciliation | Complete |
| INTERACT.2A implementation item 1 — separate histories and opt-in controls | Complete; user validated |
| INTERACT.2A implementation item 2 — fixed Ground-owned field | Complete; user validated; original timed-recovery model superseded by 2A.1 |
| INTERACT.2A implementation item 3 — per-layer binding and shader composition | Complete; user validated |
| INTERACT.2A implementation item 4 — Ground-root authoring and diagnostics | Complete; user validated |
| INTERACT.2A implementation item 5 — source validation and compliance | Complete |



### VEG-V2-INTERACT.2A post-change consistency and compliance audit

**Accepted starting state:** the user validated INTERACT.1B after moving for an extended period at `15 Hz`: `1,091` simulation dispatches, `51` recenter dispatches, and a `4.7%` recenter ratio. The user also confirmed the low-cadence grass-responsiveness problem was fixed. INTERACT.1B is therefore frozen; its domain, editor, HLSL include, and compute kernel remain byte-identical in this patch.

**Actual scope:** exactly the eight approved files were created and exactly the eight approved files were modified. No file was deleted, moved, or renamed. `GeneratedGround`, vegetation coverage, instance data, CrossedCards geometry, immediate-interaction implementation, Weather runtime/compute, benchmark runner, scene, prefab, material, layer, tag, and URP files remained byte-identical to the reconstructed accepted baseline.

**Implemented ownership and authoring:**

- `VegetationTrampleDomain` is an optional component on the existing Ground-owned `Vegetation` root and requires `GroundVegetation`. It resolves the nearest ancestor `GeneratedGround`, rejects duplicate active domains for the same Ground, and owns one fixed Ground-local historical field shared by every direct vegetation recipe under that Ground.
- `GroundVegetationEditor` exposes `Add Historical Trample Domain` or selects the existing component. No automatic scene mutation occurs.
- `VegetationInteractor` now exposes an opt-in `Historical Trample Trail` section. `Trail Mode` defaults to `Off`; the available modes are `Timed` and `Session Persistent`. Radius, bend, flattening, recovery time, minimum speed, stamp spacing, and priority are independently authored. Immediate sampling and its accepted cadence-specific history are unchanged.
- Each trample domain owns a separate per-interactor probe/stamp history. Historical updates therefore cannot corrupt the immediate domain's previous-position state when the two domains use different rates.

**Field and recovery behavior:**

- The historical field uses two `256²` ARGBHalf textures by default, a bounded 64-byte writer record, and an independent `5–60 Hz` fixed cadence. It does not recenter; world positions are mapped into the owning Ground's fixed local XZ domain.
- Only interactors with a non-Off trail mode, sufficient movement speed, and sufficient accumulated stamp distance upload writers. The domain writes swept capsules and applies a teleport guard, preventing disconnected trail gaps without creating pathological long sweeps.
- **Superseded timed model:** the original INTERACT.2A implementation stored an exponential recovery rate. INTERACT.2A.1 replaces it with a fully held recovery delay followed by a smooth eased recovery duration. Session-persistent state remains until field reset, domain teardown, Ground teardown, or scene reset and does not claim save-file persistence.
- Overlapping writers combine horizontal bend and maximum flattening. Existing session-persistent state is not weakened by later timed state.

**Rendering integration:**

- `VegetationRendererBase` binds the matching Ground's historical field immediately before each indirect draw. This supports multiple Grounds without global texture ownership and binds a neutral field when no matching domain is ready.
- Each recipe exposes `Trample Bend Response`, `Trample Flatten Response`, `Trample Height Exponent`, `Maximum Trample Bend`, `Trample Normal Response`, and `Wind Influence On Trampled Grass`. Only maximum bend is structural because it expands conservative bounds; the remaining controls are material-only. Duplicate-as-empty copies all six values.
- The vegetation vertex path samples the historical field once as one previous/current pair, combines Weather retention from immediate and historical interaction, then applies immediate and historical deformation. Historical bend and flattening contribute to deformation normals. There is no writer loop in the vegetation shader.

**Performance reconciliation:** default persistent historical GPU memory is `1,048,576` bytes for two `256²` ARGBHalf textures plus `3,072` bytes for 48 writer records. Compute performs one full-field dispatch only on historical fixed steps. Ordinary interactors with `Trail Mode = Off` are skipped before upload. No coverage mutation, vegetation rebuild, instance-buffer rewrite, per-cluster CPU interaction loop, new draw, or shader-side writer loop was introduced. The vertex path adds two bilinear historical texture reads per vegetation vertex. No `PERFORMANCE EXCEPTION` is active.

**Validation evidence:** the dedicated INTERACT.2A source validator passed `100 / 100` checks. It verified exact scope, no deletions, C#/HLSL/compute delimiter and preprocessor balance, trail-mode/default/control clamps, unchanged immediate sampler, fixed Ground ownership, independent histories, absence of recenter and hierarchy scans, opt-in/minimum-speed/stamp-spacing/teleport guards, duplicate-domain rejection, exact 64-byte CPU/GPU writer parity, timed and session-persistent recovery semantics, swept capsules, absence of ability APIs, exactly two historical texture reads and one shader sample call, Ground-local mapping, Weather/immediate/trample composition, deformation-normal response, per-Ground material binding, complete property/hash/upload/copy/bounds coverage, valid unique metas, documentation reconciliation, and byte identity of seventeen frozen dependencies. Unity 6000.5.0f1 and Unity reference assemblies are unavailable here; C# compilation, compute/shader import, scene serialization, visual trail continuity/recovery, session-persistent reset behavior, multiple-Ground binding, and runtime profiling remain pending and are not claimed.


## VEG-V2-INTERACT.2A.1 — Delayed Historical Trample Recovery

**Status:** canonical plan active; implementation pending at plan-write time.

### Objective and acceptance criteria

Replace the accepted INTERACT.2A immediate exponential historical recovery with an explicit two-phase timed model:

```text
stamp / restamp
    fully held for Recovery Delay Seconds
    smooth eased return during Recovery Duration Seconds
    restored
```

Acceptance requires:

- `VegetationInteractor` exposes `Recovery Delay Seconds` and `Recovery Duration Seconds`; the obsolete single `Trail Recovery Time Seconds` control is removed;
- default timed behavior is `6 s` hold plus `2 s` recovery;
- delay accepts `0–300 s`; recovery duration accepts `0.05–30 s`;
- session-persistent trails ignore both timed controls and retain current reset semantics;
- a timed cell does not lose bend or flatten strength during its hold phase;
- recovery uses a smoothstep-derived remaining-weight curve rather than immediate exponential decay or linear interpolation;
- restamping restarts or extends the hold phase, while weaker overlapping writes cannot shorten an existing longer delay or recovery duration;
- the existing 64-byte CPU/GPU writer record is retained by using the already-reserved persistence vector components;
- deformation sampling remains the existing two deformation-texture reads and one `SampleVegetationTrample` call; the new timing state is compute-only;
- no vegetation rebuild, coverage mutation, actor hierarchy scan, ability stamp, save-file persistence, immediate-field change, or per-frame CPU trail decay is introduced.

### Reviewed evidence

- `Assets/Game/Procedural/Vegetation/VegetationInteractor.cs`: `trailRecoveryTimeSeconds` is the sole timed authoring value and defaults to `8`; it is serialized under `Historical Trample Trail`.
- `Assets/Game/Procedural/Vegetation/VegetationTrampleDomain.cs`: `UploadTrailWriters` converts the single value into an exponential recovery rate (`4.6051702 / seconds`) and stores it in `DirectionParameters.w`; the writer record already contains unused `PersistenceParameters.yzw`.
- `Assets/Game/Rendering/Vegetation/Resources/PS3DVegetation/Compute/CS_VegetationTrampleField.compute`: state `.w` currently stores the recovery rate, and every timed cell begins exponential decay on the first simulation step without current support.
- `Assets/Game/Rendering/Vegetation/Includes/VegetationTrampleField.hlsl`: the vegetation shader consumes only deformation `xyz`; recovery metadata is compute-only.
- `Assets/Game/Procedural/Vegetation/Editor/VegetationTrampleDomainEditor.cs`: the default Inspector already surfaces serialized interactor controls through `VegetationInteractor`; no custom interactor editor change is required.
- The supplied user validation states INTERACT.2A works as described but the immediate slow recovery appears mechanical; the accepted target example is `6 s` delay plus `2 s` recovery.
- No Git metadata exists in the supplied reconstructed workspace. The accepted INTERACT.2A source tree is the comparison baseline.

### Approved scope

Modify only:

```text
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Docs/Stylized_Vegetation_Architecture.md
Assets/Game/Procedural/Vegetation/VegetationInteractor.cs
Assets/Game/Procedural/Vegetation/VegetationTrampleDomain.cs
Assets/Game/Rendering/Vegetation/Resources/PS3DVegetation/Compute/CS_VegetationTrampleField.compute
```

Create/delete: none. Scenes, prefabs, materials, Ground runtime, immediate interaction, Weather, coverage, instance data, geometry, vegetation shader/includes, layers, tags, and URP settings remain frozen.

### Implementation sequence

1. Replace the single serialized interactor recovery value with delay and duration controls, public accessors, tooltips, defaults, and validation clamps.
2. Keep the 64-byte writer layout; upload persistent flag, delay, and duration through `PersistenceParameters.xyz`.
3. Add one fixed Ground-local `RGFloat` compute timing texture. Store a signed phase clock and recovery duration per cell while keeping deformation in the existing previous/current `ARGBHalf` pair.
4. During timed simulation, hold deformation while the phase clock is positive. After it crosses zero, calculate previous and next smoothstep remaining weights and multiply deformation by their ratio so the stored state follows one non-linear recovery curve without preserving a separate original-strength texture.
5. On restamp, preserve or strengthen deformation, restart/extend delay, and retain the longer recovery duration. Persistent state continues to dominate timed state.
6. Update memory/report text and current architecture language. Remove active documentation that describes immediate exponential recovery or one combined recovery-time control.
7. Run exact-scope, syntax, CPU/GPU layout, timing-curve, restamp, persistence, frozen-dependency, and documentation checks. Reread all final changed files and affected consumers.

### Performance and risk model

- Default timing allocation: `256² × RGFloat = 524,288 bytes`; total historical textures become `1,572,864 bytes` instead of `1,048,576 bytes`.
- Writer stride and upload capacity remain `64 bytes × 48 = 3,072 bytes`.
- The compute dispatch count, update cadence, vegetation texture reads, material bindings, instance buffers, and draw count remain unchanged.
- The added per-cell work is a small fixed set of scalar operations in the existing historical compute pass.
- **Resolved design risk:** `RGHalf` cannot reliably decrement a 300-second phase clock at 5–60 Hz because half-float spacing becomes larger than one fixed step. The timing texture therefore uses `RGFloat`. Validation will numerically exercise curve progression, boundary crossing, and restamping across the full authored ranges.
- **Risk:** in-place timing UAV access must stay one-thread-per-cell with no cross-cell reads. The compute kernel already assigns one dispatch thread to one cell, so no ordering dependency is introduced.

### Validation requirements

- Exact five-file diff and no new/deleted files.
- C#/compute delimiter and preprocessor balance.
- No obsolete `TrailRecoveryTimeSeconds`/`trailRecoveryTimeSeconds` references.
- New control defaults and clamps exactly match the plan.
- CPU/GPU writer records remain four `float4`/`Vector4` fields and 64 bytes.
- Timing texture is created, initialized, validated, bound, reported, and released.
- Timed cells hold exactly through delay, follow the smoothstep remaining curve during duration, and clear at completion.
- Restamping cannot reduce existing deformation or shorten the existing delay/duration.
- Session-persistent cells do not enter timed recovery.
- Vegetation trample HLSL and shader remain byte-identical.
- Immediate interaction, Ground, Weather, coverage, instance data, geometry, scene, and other frozen dependencies remain byte-identical.
- Unity compilation, compute import, visual hold/recovery, and runtime memory/profile validation remain pending when Unity is unavailable.

### Plan status

| Item | Status |
| --- | --- |
| INTERACT.2A.1.0 Canonical plan and active-document reconciliation | Complete |
| INTERACT.2A.1.1 Interactor delay/duration controls | Complete at source level; Unity validation pending |
| INTERACT.2A.1.2 Compute timing state and eased recovery | Complete at source level; Unity validation pending |
| INTERACT.2A.1.3 Restamp/persistence semantics | Complete at source level; Unity validation pending |
| INTERACT.2A.1.4 Diagnostics and memory reconciliation | Complete at source level; Unity validation pending |
| INTERACT.2A.1.5 Source validation and final compliance audit | Complete at source level; Unity validation pending |


### VEG-V2-INTERACT.2A.1 post-change consistency and compliance audit

**Actual scope:** exactly the approved five modified files; no created or deleted files.

**Intentional implementation differences from INTERACT.2A:**

- `VegetationInteractor` removes the single `Trail Recovery Time Seconds` field and exposes `Recovery Delay Seconds` (`0–300`, default `6`) plus `Recovery Duration Seconds` (`0.05–30`, default `2`). `Off` and `Session Persistent` semantics are unchanged.
- The 64-byte writer record is unchanged. `PersistenceParameters.x` remains the session-persistent flag; `.y` and `.z` now carry recovery delay and recovery duration. `DirectionParameters.w` is reserved/zero instead of containing an exponential rate.
- `VegetationTrampleDomain` adds one compute-only `RGFloat` timing texture and includes it in allocation, validation, initialization, dispatch binding, reporting, and release. Default historical texture memory is now `1,572,864 bytes`; default writer-buffer memory remains `3,072 bytes`.
- The compute field stores deformation in the accepted previous/current `ARGBHalf` pair and stores a signed phase clock plus recovery duration in the timing texture. Positive phase holds state unchanged. Negative phase measures recovery progress. A ratio of consecutive `1 - smoothstep(progress)` weights advances the stored deformation along one smooth absolute recovery curve without another original-strength texture.
- Restamping preserves or strengthens deformation, restarts or extends the hold phase, and retains the longer existing recovery duration. Session-persistent state dominates timed state and bypasses timed recovery.
- The trample shader include, production vegetation shader, per-layer response controls, material binding, draw path, immediate interaction, Ground ownership, coverage, instance data, geometry, Weather, and scene remain byte-identical.

**Precision correction recorded before final implementation:** the planned `RGHalf` timer was rejected after numerical review showed that half-float spacing near a 300-second delay can exceed a 5–60 Hz timestep. `RGFloat` advances correctly at the maximum delay at 5, 12, and 60 Hz.

**Validation evidence:** the dedicated INTERACT.2A.1 source validator passed `80 / 80` checks. It verified exact scope, no created/deleted files, C#/compute delimiter and preprocessor balance, obsolete-control removal, exact defaults/ranges/clamps, unchanged 64-byte CPU/GPU writer layout, complete RGFloat timing-texture lifecycle, compute-only timing ownership, smoothstep recovery, removal of exponential historical decay, full hold behavior, recovery-ratio math, restamp delay/duration extension, persistent dominance, completion clearing, absence of ability APIs, numerical progression at delay/duration/rate extremes, RGFloat countdown precision, active-document reconciliation, and byte identity of eighteen frozen dependencies.

Unity 6000.5.0f1 and Unity reference assemblies are unavailable in this environment. C# compilation, `RGFloat` random-write support/import on the project target APIs, scene serialization of the replacement controls, visual six-second hold/two-second recovery, restamping during recovery, session-persistent behavior, reported memory, and runtime profiling remain pending and are not claimed.

## VEG-V2-INTERACT.2A.2 — Asymmetric Slow–Fast–Slow Trample Recovery

**Status:** source implementation and static validation complete; Unity validation pending.

### Objective and acceptance criteria

Replace the accepted INTERACT.2A.1 symmetric recovery mapping with one fixed asymmetric slow–fast–slow return curve. Recovery delay, recovery duration, timed/session-persistent ownership, restamping, timing storage, update cadence, and vegetation sampling remain unchanged.

The normalized recovery target is:

```text
recovery time       restored deformation
0%                  0%
50%                 15%
90%                 90%
100%                100%
```

Acceptance requires:

- grass remains fully held for the existing authored `Recovery Delay Seconds`;
- during the first half of `Recovery Duration Seconds`, only approximately 15% of the stored bend/flatten deformation is removed;
- from 50–90% of the recovery duration, restoration accelerates from approximately 15% to 90%;
- during the final 10%, the remaining approximately 10% settles to the original state;
- the mapping is monotonic and continuously differentiable through the 50% and 90% boundaries;
- no piecewise-linear corners, zero-velocity pauses at internal boundaries, overshoot, or negative recovery velocity;
- the curve is fixed rather than adding technical per-writer curve controls;
- restamping, persistent-state dominance, writer layout, timing texture, compute cadence, memory, deformation textures, vegetation shader reads, draw count, and layer response controls remain unchanged.

### Reviewed evidence

- `Assets/AGENTS.md` was read completely. It requires this persistent plan as the first project write, exact scope, caller/consumer review, a final compliance audit, and explicit Unity-only pending checks.
- `Assets/Game/Rendering/Vegetation/Resources/PS3DVegetation/Compute/CS_VegetationTrampleField.compute::RecoveryRemainingWeight` currently returns `1 - smoothstep(0, 1, progress)`, which restores exactly 50% of deformation at 50% normalized recovery time. This does not match the accepted user target of approximately 15% restored at that point.
- `CS_VegetationTrampleField.compute::SimulateField` advances stored deformation through the ratio of consecutive absolute remaining weights. Replacing only `RecoveryRemainingWeight` preserves the accepted delay, restamp, completion, and timing-texture architecture.
- `Assets/Game/Procedural/Vegetation/VegetationInteractor.cs::recoveryDurationSeconds` describes a generic smooth eased curve. Its tooltip must be reconciled with the new fixed asymmetric mapping.
- `Assets/Game/Procedural/Vegetation/VegetationTrampleDomain.cs::BuildReport` identifies INTERACT.2A.1 and only reports generic smooth recovery. It must identify INTERACT.2A.2 and report the accepted 50%/15%, 90%/90% curve contract.
- `Assets/Game/Rendering/Vegetation/Includes/VegetationTrampleField.hlsl` and `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader` consume only the existing deformation textures and remain frozen.
- No Git metadata exists in the reconstructed supplied workspace. `/mnt/data/veg_interact2a1_workspace/current` is the accepted INTERACT.2A.1 source baseline. The pre-edit SHA-256 values for the interactor, trample domain, and trample compute files are `9f874e1e...f39f4d7`, `7c08aba5...43229caf`, and `2f97c75d...8fb7709d` respectively.
- User validation states INTERACT.2A.1 works, but its recovery animation remains too evenly distributed. The accepted target is asymmetric slow–fast–slow behavior with approximate key points at `(0,0)`, `(0.5,0.15)`, `(0.9,0.9)`, and `(1,1)`.

### Approved scope

Modify only:

```text
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Docs/Stylized_Vegetation_Architecture.md
Assets/Game/Procedural/Vegetation/VegetationInteractor.cs
Assets/Game/Procedural/Vegetation/VegetationTrampleDomain.cs
Assets/Game/Rendering/Vegetation/Resources/PS3DVegetation/Compute/CS_VegetationTrampleField.compute
```

Create/delete: none. Scenes, prefabs, materials, Ground runtime, immediate interaction, Weather, coverage, instance data, geometry, vegetation shader/includes, timing/deformation formats, layers, tags, and URP settings remain frozen.

### Implementation sequence

1. Replace active architecture wording that describes only generic symmetric easing with the fixed asymmetric key-point contract. Update the concise architecture in place rather than adding conflicting guidance.
2. Update the interactor recovery-duration tooltip and historical-domain report/version text without changing serialized fields, defaults, ranges, or writer upload data.
3. Replace `RecoveryRemainingWeight` with a restored-weight function composed of three cubic-Hermite intervals through `(0,0)`, `(0.5,0.15)`, `(0.9,0.9)`, and `(1,1)`. Use continuous normalized-progress slopes `0`, `0.5`, `1.0`, and `0` at those four knots. Return `1 - restoredWeight` to the existing ratio-based state update.
4. Numerically validate exact key points, monotonicity, range `[0,1]`, derivative continuity at 50% and 90%, zero endpoint slopes, no overshoot, ratio progression at supported update rates, unchanged delay, and exact completion.
5. Run exact-scope, syntax/preprocessor, stale-text, frozen-dependency, and documentation consistency checks. Reread all final changed files and direct consumers and record the post-change audit here.

### Performance and compatibility model

- No texture, buffer, record, property, shader sample, draw, dispatch, update-rate, or memory change.
- The existing compute pass replaces one `smoothstep` with one of three bounded cubic-Hermite evaluations selected by normalized recovery progress.
- Work remains one thread per historical field cell at the configured fixed cadence. No new loop or branch depends on writer count.
- Serialized `Recovery Delay Seconds` and `Recovery Duration Seconds` remain byte-compatible because no field name, type, order, default, or range changes.
- **Risk:** poorly selected Hermite tangents could overshoot or reverse. The fixed slopes are subject to exhaustive numerical validation over `[0,1]`; implementation is blocked if any sampled derivative is negative or any value exits `[0,1]`.

### Validation requirements

- Exact five-file diff; no created/deleted files.
- C#/compute delimiter and preprocessor balance.
- Existing delay/duration fields, defaults, ranges, accessors, writer layout, timing texture, and persistence semantics unchanged.
- Exact recovered values at normalized times `0`, `0.5`, `0.9`, and `1` within floating-point tolerance.
- Nonnegative derivative and values inside `[0,1]` over at least 10,001 samples.
- Left/right derivative continuity at internal boundaries within tolerance and zero endpoint derivatives.
- Stored remaining deformation decreases monotonically and reaches zero for representative `5`, `12`, and `60 Hz` updates and minimum/default/maximum recovery durations.
- Vegetation trample include, production vegetation shader, renderer, Ground, immediate interaction, Weather, coverage, instance data, geometry, scene, and other frozen dependencies byte-identical.
- Unity compilation, compute import, and visual curve validation remain pending when Unity is unavailable.

### Plan status

| Item | Status |
| --- | --- |
| INTERACT.2A.2.0 Canonical plan and reviewed evidence | Complete |
| INTERACT.2A.2.1 Active-document reconciliation | Complete |
| INTERACT.2A.2.2 Asymmetric compute recovery mapping | Complete at source level; Unity validation pending |
| INTERACT.2A.2.3 Tooltip and diagnostic reconciliation | Complete |
| INTERACT.2A.2.4 Source validation and final compliance audit | Complete at source level; Unity validation pending |


### VEG-V2-INTERACT.2A.2 post-change consistency and compliance audit

**Actual scope:** exactly the approved five files were modified. No file was created, deleted, moved, or renamed. No scene, prefab, material, Ground runtime, immediate-interaction, Weather, coverage, instance-layout, geometry, shader/include, layer, tag, or URP file changed.

**Intentional implementation differences from INTERACT.2A.1:**

- `CS_VegetationTrampleField.compute::RecoveryRemainingWeight` no longer uses the symmetric `1 - smoothstep(0,1,progress)` mapping. It now evaluates a fixed restored-weight curve through `(0,0)`, `(0.5,0.15)`, `(0.9,0.9)`, and `(1,1)`, then returns the remaining weight to the unchanged ratio-based state update.
- The curve uses three cubic-Hermite intervals with shared normalized-time slopes `0`, `0.5`, `1.0`, and `0` at the four knots. Internal left/right slopes therefore match at 50% and 90%, while the start and finish have zero slope. The mapping has no internal stop, linear corner, overshoot, or reversal.
- Representative restored values are `4.375%` at 25% time, `15%` at 50%, `50%` at 70%, `90%` at 90%, and `96.25%` at 95%. This implements the accepted restrained first half, accelerated middle, and gentle final settling.
- `VegetationInteractor` serialized recovery fields, defaults, ranges, public accessors, and writer upload contract are unchanged; only the recovery-duration tooltip was reconciled.
- `VegetationTrampleDomain` runtime behavior is unchanged; only the report identity and recovery-curve description now identify INTERACT.2A.2.
- Recovery delay, restamping, persistent-state dominance, completion clearing, `RGFloat` timing state, deformation textures, update cadence, field memory, writer memory, material bindings, vegetation texture reads, draw count, and per-layer response remain unchanged.

**Performance reconciliation:** the historical compute pass replaces one symmetric `smoothstep` with one bounded three-branch cubic-Hermite mapping. No allocation, upload, dispatch, draw, texture sample, writer loop, per-frame CPU work, or memory is added. No `PERFORMANCE EXCEPTION` is active.

**Final reread and dependency audit:** the complete final versions of all five modified files were reread. `VegetationTrampleField.hlsl`, `SH_StylizedVegetationBenchmark.shader`, `VegetationRendererBase`, `GroundVegetation`, the immediate interaction domain/compute/include, `GeneratedGround`, Weather runtime/compute, coverage, instance data, cluster geometry, and `VisualFrameworkDemo.unity` were rechecked as direct consumers or frozen contracts and remain byte-identical to the accepted INTERACT.2A.1 baseline.

**Validation evidence:** the dedicated INTERACT.2A.2 validator passed `78 / 78` checks. It verified exact scope, no created/deleted files, C#/compute delimiter and preprocessor balance, unchanged serialization and runtime contracts outside tooltip/report text, exact compute isolation to the recovery mapping, exact key points, values inside `[0,1]`, monotonicity over 100,001 samples, derivative continuity at 50% and 90%, zero endpoint derivatives, restrained/middle/final curve behavior, monotonic ratio progression and exact completion at representative `5`, `12`, and `60 Hz` cadences and minimum/default/maximum recovery durations, active-document reconciliation, and byte identity of eighteen frozen dependencies.

Unity 6000.5.0f1, Unity reference assemblies, and a standalone HLSL compiler are unavailable in this environment. Unity C# compilation, compute-shader import, GPU execution, and visual confirmation of the asymmetric recovery remain pending and are not claimed.


## VEG-V2-INTERACT.2B — Circle, Cone, and Line Ability Trample Stamps

**Status:** user-validated and accepted in Unity after the corrective `VEG-V2-INTERACT.2B.1` and `VEG-V2-INTERACT.2B.2` patches.

### Objective and acceptance criteria

Add explicit gameplay-driven historical trample events without temporary actors, coverage mutation, or vegetation rebuilds. The runtime API must support exactly three user-facing shapes:

- `Circle`: radial footprint with 360-degree coverage;
- `Cone`: the same radial-sector footprint with authored coverage below 360 degrees and an authored world-XZ facing direction;
- `Line`: a width-controlled capsule between authored world-space start and end positions.

Each request must independently control bend strength, flatten strength, displacement mode, recovery mode, recovery delay, recovery duration, deterministic edge irregularity, irregularity scale, seed, and priority. Timed requests must use the accepted delayed asymmetric recovery curve. Session-persistent requests remain until field reset or runtime teardown.

Acceptance requires:

- one public plain-data stamp request with validated factory helpers for circle, cone, and line;
- one static submission entry point that forwards the request to every intersecting active Ground trample domain and returns the accepted-domain count;
- bounded per-domain queueing with no hierarchy scan and no allocation in the historical compute step;
- one separate GPU ability-stamp buffer consumed once on the next fixed historical update;
- circle/cone radial-sector evaluation, capsule line evaluation, deterministic seeded edge breakup, and no sterile perfect border when irregularity is nonzero;
- displacement modes `Radial Outward`, `Fixed World Direction`, `Away From Centreline`, and `Flatten Only`;
- stronger overlapping bend direction retained, maximum flatten retained, session persistence dominant, and timed schedules extended but never shortened;
- no new deformation/timing texture, no new vegetation shader read, no draw, instance-layout, coverage, or recipe change;
- a dedicated optional `VegetationTrampleStampTester` component and custom Inspector that exercise the exact runtime API from a player-attached object;
- editable named test configurations, selected/previous/next/random configuration actions, and bounded randomized variants rather than unconstrained random values;
- complete clipboard diagnostics covering queue capacity, pending/uploaded/dropped requests, supported shapes, and buffer memory.

### Reviewed evidence

- `Assets/AGENTS.md` was read completely. It requires this persistent plan as the first project write, exact scope, complete caller/consumer review, final consistency audit, and explicit Unity-only pending validation.
- `Assets/Game/Procedural/Vegetation/VegetationTrampleDomain.cs::Update` currently uploads opt-in moving trail writers and dispatches one `SimulateField` kernel at an independently configurable `5–60 Hz`. It owns fixed Ground-local deformation/timing textures and is the correct owner for queued ability requests.
- `VegetationTrampleDomain.cs::ActiveDomains`, `ResolveOwnership`, `SweptCircleIntersectsGround`, and `FindDomainForGround` establish active-domain registration and Ground-local ownership without hierarchy scans. The ability submission API can route against this registry and conservatively test shape bounds against each Ground.
- `VegetationTrampleDomain.cs::GpuWriterRecord` and `CS_VegetationTrampleField.compute::VegetationTrampleWriterRecord` form the accepted 64-byte moving-trail contract. Ability requests require a separate record and buffer so trail serialization and validated writer behavior remain unchanged.
- `CS_VegetationTrampleField.compute::EvaluateTarget` currently loops only over moving trail writers, writes bend/flatten targets, and reuses the validated delay/duration/persistence state machine. Ability evaluation can feed the same `TrampleTarget` without changing deformation texture ownership or recovery semantics.
- `CS_VegetationTrampleField.compute::RecoveryRestoredWeight` is the user-validated INTERACT.2A.2 asymmetric curve and remains frozen.
- `Assets/Game/Rendering/Vegetation/Includes/VegetationTrampleField.hlsl` and `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader` consume only the existing historical deformation textures and require no modification.
- `Assets/Game/Procedural/Vegetation/VegetationInteractor.cs` remains the moving-object owner. Ability events are separate and must not be represented by temporary interactors.
- `Assets/Game/Procedural/Vegetation/Editor/VegetationTrampleDomainEditor.cs` exposes reset/report actions. It may report queue state but test-shape controls belong to a dedicated tester component as explicitly requested.
- No Git metadata exists in the reconstructed supplied workspace. `/mnt/data/veg_interact2a2_workspace/current` is the accepted source baseline. Pre-edit SHA-256 values are `ca309cb3...51d985d6` for `VegetationTrampleDomain.cs`, `d4de5c36...8b3a9841` for its editor, and `bde9ed6a...45ab1ab` for the compute shader.
- User validation states INTERACT.2A.2 works as expected. The requested next contract is circle, cone-as-partial-circle, and line, with a player-attached button-driven test script and sensible configuration switching/randomization.

### Approved scope

Create:

```text
Assets/Game/Procedural/Vegetation/VegetationTrampleStamp.cs
Assets/Game/Procedural/Vegetation/VegetationTrampleStamp.cs.meta
Assets/Game/Procedural/Vegetation/VegetationTrampleStampTester.cs
Assets/Game/Procedural/Vegetation/VegetationTrampleStampTester.cs.meta
Assets/Game/Procedural/Vegetation/Editor/VegetationTrampleStampTesterEditor.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationTrampleStampTesterEditor.cs.meta
```

Modify:

```text
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Docs/Stylized_Vegetation_Architecture.md
Assets/Game/Procedural/Vegetation/VegetationTrampleDomain.cs
Assets/Game/Procedural/Vegetation/Editor/VegetationTrampleDomainEditor.cs
Assets/Game/Rendering/Vegetation/Resources/PS3DVegetation/Compute/CS_VegetationTrampleField.compute
```

Delete/move/rename: none. Scenes, prefabs, materials, `VegetationInteractor`, `GroundVegetation`, Ground runtime, immediate interaction, Weather, coverage, instance data, geometry, vegetation shader/includes, layers, tags, and URP settings remain frozen.

### Implementation sequence

1. Replace active deferred-ability wording with the exact circle/cone/line contract and update the concise architecture in place.
2. Add public enums and a validated `VegetationTrampleStampRequest` plain-data struct with `CreateCircle`, `CreateCone`, and `CreateLine` factories. Circle/cone share radial-sector data; line stores world start/end and width.
3. Extend each active `VegetationTrampleDomain` with a bounded pending queue, separate structured GPU stamp buffer, static submission routing, conservative Ground-intersection checks, deterministic priority/sequence ordering, and queue/upload/drop diagnostics. Consume uploaded requests exactly once at the next fixed step.
4. Extend the existing compute target evaluation with a separate ability-stamp loop. Implement deterministic value-noise border breakup, radial-sector and capsule influence, the four displacement modes, strongest-direction retention, maximum flattening, and the existing persistence/timing schedule rules. Keep moving-writer and recovery code unchanged.
5. Add `VegetationTrampleStampTester` with serializable named configurations, default circle/cone/line presets, transform-relative origin/facing, selected and randomized stamping, previous/next/random selection, bounded variation controls, Gizmo previews, last-result diagnostics, and a custom Inspector with Play-Mode buttons.
6. Update the domain Inspector/report for ability queue status. Run exact-scope, C#/compute syntax, record-stride, shape-math, queue, deterministic-noise, overlap, stale-text, frozen-dependency, and documentation checks. Reread all final changed files and direct consumers and record the post-change audit here.

### Performance and compatibility model

- Existing historical textures remain two `ARGBHalf` deformation textures plus one `RGFloat` timing texture. Vegetation shader sampling and draw behavior are unchanged.
- Moving trail writers retain the accepted 64-byte record and buffer. Ability stamps use a separate 96-byte record, default 32-record upload buffer (`3,072` bytes), and default 128-request CPU queue.
- The existing full-field compute dispatch gains a second loop only when `_StampCount > 0`; when no ability request is queued, that loop executes zero iterations. One-shot requests are removed from the pending queue when uploaded and are not resubmitted on later steps.
- Submission uses the static active-domain registry and conservative shape bounds; it does not call `FindObjectsByType`, scan layers, rebuild vegetation, or mutate coverage.
- Test presets and randomization run only on explicit Inspector actions. The tester is optional and contains no `Update`, `LateUpdate`, or `FixedUpdate`.
- **Risk:** opposed stamp directions can cancel if accumulated. Ability bend selection must compare weighted magnitudes and retain the stronger vector. Moving-trail accumulation remains unchanged.
- **Risk:** random borders can flicker if time-dependent. Irregularity must use only absolute world position, authored scale, and seed.
- **Risk:** queue overflow can silently lose gameplay events. Submission and reporting must count rejected/full-queue requests, and priority ordering must be deterministic.

### Validation requirements

- Exact eleven-file scope: six created and five modified; no deletions.
- C#/compute delimiter, preprocessor, namespace, and serialized-field checks.
- Public request factories produce finite, clamped, normalized shape data and preserve exact user-facing circle/cone/line semantics.
- Circle 360-degree coverage accepts all directions; cone coverage rejects positions outside the authored half-angle; line uses closest-point capsule distance including rounded ends.
- Irregularity zero reproduces the analytic shape; nonzero irregularity is deterministic for fixed world position/seed and varies for different seeds.
- Ability records are exactly 96 bytes in C# and HLSL; existing moving-writer records remain exactly 64 bytes.
- Queue capacity, deterministic priority/sequence ordering, one-step consumption, upload counts, and overflow/rejection reporting are structurally verified.
- Existing delayed asymmetric recovery, session persistence, moving trails, timing/deformation formats, update rate, shader sampling, and layer response remain unchanged.
- Tester has no frame loop, calls the exact runtime API, creates sensible default configurations, bounds randomized variation, and exposes Inspector buttons plus Gizmo preview.
- Direct consumers and frozen dependencies remain byte-identical. Unity compilation, compute import, runtime queueing, visual shape/irregularity, multi-Ground routing, and tester-button behavior remain pending when Unity is unavailable.

### Plan status

| Item | Status |
| --- | --- |
| INTERACT.2B.0 Canonical plan and reviewed evidence | Complete |
| INTERACT.2B.1 Active-document reconciliation | Complete |
| INTERACT.2B.2 Public stamp API and domain queue | Complete and user-validated after 2B.1/2B.2 corrections |
| INTERACT.2B.3 Compute circle/cone/line evaluation | Complete and user-validated after 2B.2 |
| INTERACT.2B.4 Dedicated test component and Inspector | Complete and user-validated |
| INTERACT.2B.5 Source validation and final compliance audit | Complete; accepted by user runtime validation |

### INTERACT.2B post-change consistency and compliance audit

**Actual scope:** matched the approved eleven-file declaration exactly: six created files, five modified files, and no deletions. No scene, prefab, material, Ground runtime, `VegetationInteractor`, immediate-interaction, Weather, coverage, instance-layout, vegetation shader/include, layer, tag, or URP file changed.

**Intentional implementation differences from INTERACT.2A.2:**

- added the validated public `VegetationTrampleStampRequest` API and `Circle`, `Cone`, and `Line` shape contracts;
- added one bounded per-Ground CPU request queue and one separate 96-byte-record GPU ability buffer;
- added one ability-evaluation loop to the existing historical compute dispatch, active only when `_StampCount > 0`;
- added deterministic world-space edge irregularity and the four accepted displacement modes;
- treated a 360-degree cone arc as complete circle coverage without angular breakup, and used true closest-centreline vectors at rounded line ends;
- added the optional player-attached tester, named presets, bounded randomized variants, Gizmo previews, and Play-Mode Inspector actions;
- extended only the historical-domain report and Inspector status with ability queue/buffer telemetry.

**Preserved behavior:** the 64-byte moving-writer record and complete moving-writer loop, delayed asymmetric recovery function, historical textures and formats, update cadence, session-persistent precedence, layer response, vegetation shader sampling, immediate interaction, Weather composition, Ground ownership, scene content, and production rendering paths remain unchanged. One-shot requests are removed from the queue immediately after upload and therefore cannot repeat on subsequent fixed steps.

**Validation evidence:** the dedicated INTERACT.2B validator passed `178 / 178` checks before this audit entry. It verified exact scope, C#/compute lexical and preprocessor balance, valid unique metas, request enums/factories/clamps, 96-byte C#/HLSL ability-record parity, unchanged 64-byte writer parity, deterministic bounded queue ordering and consumption, absence of hierarchy/layer scans, tester actions/presets/randomization and absence of frame loops, circle/cone/line analytic inclusion tests, full 360-degree coverage, rounded line caps, deterministic seeded irregularity, strongest-direction handling, frozen moving-writer and recovery blocks, active-document reconciliation, and byte identity of twelve direct frozen dependencies. The final delivered validator is rerun after this audit entry and records `179 / 179` checks.

The original source-validation environment lacked Unity. Subsequent user runtime validation confirmed the circle, cone, and line tester behavior after the 2B.1 buffer correction and the 2B.2 D3D11 evaluator correction. Multi-Ground routing was not separately reported and remains unmeasured rather than assumed.

## VEG-V2-INTERACT.2B.1 — Ability Stamp Buffer Binding and D3D11 Shape Initialization Fix

**Status:** `_Stamps` buffer correction accepted in Unity; the aggregate shape-initialization attempt was insufficient and is superseded by `VEG-V2-INTERACT.2B.2`.

### Objective and acceptance criteria

Correct two Unity-runtime faults reported after applying INTERACT.2B:

- `Compute shader (CS_VegetationTrampleField): Property (_Stamps) at kernel index (1) is not set` during `SimulateField` dispatch when no one-shot ability stamp has been uploaded yet;
- D3D11 warning `use of potentially uninitialized variable (EvaluateAbilityStampShape)` at the ability-shape evaluation call.

Acceptance requires:

- the `_Stamps` structured buffer contains one deterministic zero record immediately after resource creation and whenever a fixed simulation step has zero uploaded ability stamps;
- `_Stamps` remains bound unconditionally before every `SimulateField` dispatch;
- `_StampCount == 0` continues to execute zero ability-loop iterations and cannot change the historical field;
- `AbilityStampEvaluation` is explicitly zero-initialized before any shape-specific branch or early return, so every returned member is provably initialized on D3D11;
- circle, cone, line, trail, delayed recovery, queue, memory, update cadence, tester, layer response, shader sampling, and Ground ownership remain unchanged.

### Reviewed evidence

- `Assets/AGENTS.md` was read completely. It requires this persistent plan as the first project write, exact scope, strict implementation from the plan, and a final consistency/compliance audit.
- User runtime evidence reports `_Stamps` unset at `SimulateField` dispatch and D3D11 reporting a potentially uninitialized `EvaluateAbilityStampShape` return value. The warnings persist after leaving Play Mode, so they are not treated as import-state noise.
- `Assets/Game/Procedural/Vegetation/VegetationTrampleDomain.cs::EnsureResources` creates `stampBuffer` and `stampUploadRecords`, but `UploadAbilityStamps` returns immediately when the pending queue is empty and therefore never uploads initial contents to the structured buffer.
- `VegetationTrampleDomain.cs::DispatchSimulation` already binds `_Stamps` unconditionally. This binding remains required; the corrective change primes the buffer rather than making binding conditional.
- `Assets/Game/Rendering/Vegetation/Resources/PS3DVegetation/Compute/CS_VegetationTrampleField.compute::EvaluateAbilityStampShape` assigns individual members before several early returns, but the D3D11 compiler does not prove the complete structure is initialized. Explicit aggregate zero-initialization removes that ambiguity without changing shape math.
- The current authoritative source is `/mnt/data/veg_interact2b_workspace/current`; no `.git` metadata is present in the supplied workspace.

### Approved scope

Modify only:

```text
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Game/Procedural/Vegetation/VegetationTrampleDomain.cs
Assets/Game/Rendering/Vegetation/Resources/PS3DVegetation/Compute/CS_VegetationTrampleField.compute
```

Create/delete/move/rename: none. Tester, request API, domain editor, interactors, Ground, immediate interaction, vegetation shaders/includes, Weather, scenes, prefabs, materials, instance data, geometry, layers, tags, and URP settings remain frozen.

### Implementation sequence

1. Record this corrective plan and reviewed runtime evidence.
2. Add a private zero-stamp-buffer priming helper in `VegetationTrampleDomain` that writes one default `GpuStampRecord` into slot zero.
3. Invoke the helper after stamp-buffer allocation and on every no-stamp upload step; keep `_StampCount = 0` and unconditional `_Stamps` binding unchanged.
4. Explicitly aggregate-initialize `AbilityStampEvaluation` to zero before assigning its facing fallback and entering shape branches.
5. Run exact-scope, syntax, stride, zero-record, unconditional-binding, zero-count-no-op, D3D11-initialization, frozen-behavior, and frozen-dependency checks. Reread all modified files and direct consumers and record the final audit here.

### Performance and compatibility model

- No new allocation or buffer is added. The existing default stamp buffer remains `32 × 96 = 3,072` bytes.
- Priming writes one 96-byte record at resource creation and one 96-byte record on fixed steps with no queued stamps. This is a bounded safety upload and does not add a dispatch, texture read, vegetation draw, instance update, or queue scan.
- `_StampCount` remains zero on no-stamp steps, so the compute ability loop performs zero iterations and the primed record is never evaluated.
- Explicit HLSL structure initialization compiles to deterministic zero defaults and does not change accepted shape outputs because every used member is subsequently assigned exactly as before.

### Validation requirements

- Exact three-file modified scope and no created/deleted files.
- C# and compute delimiter/preprocessor balance.
- `stampBuffer.SetData(... count: 1)` or equivalent deterministic zero-record upload exists after allocation and in the zero-pending path.
- `_Stamps` remains bound outside any positive-count condition before every `SimulateField` dispatch.
- `_StampCount` remains `0` when no stamp is uploaded.
- `AbilityStampEvaluation evaluation = (AbilityStampEvaluation)0;` or equivalent full initialization occurs before all branches and early returns.
- Ability-shape equations, request record stride, queue capacity, moving writers, recovery mapping, texture formats, and direct frozen dependencies remain byte-identical.
- Unity compilation, D3D11 compute import, absence of the runtime error, and absence of the shader warning remain pending when Unity is unavailable.

### Plan status

| Item | Status |
| --- | --- |
| INTERACT.2B.1.0 Canonical plan and reviewed evidence | Complete |
| INTERACT.2B.1.1 Zero-record stamp-buffer priming | Complete and user-validated |
| INTERACT.2B.1.2 Explicit D3D11 shape-result initialization | Superseded; did not remove the warning |
| INTERACT.2B.1.3 Source validation and final compliance audit | Complete; corrective follow-up recorded |


### INTERACT.2B.1 post-change consistency and compliance audit

**Actual scope:** matched the approved three-file declaration exactly: one canonical Markdown document, `VegetationTrampleDomain.cs`, and `CS_VegetationTrampleField.compute` were modified. No file was created, deleted, moved, or renamed. No Weather, scene, prefab, material, Ground, interactor, tester, domain-editor, shader/include, coverage, instance-data, geometry, layer, tag, or URP file changed.

**Intentional implementation differences from INTERACT.2B:**

- the existing ability-stamp graphics buffer is primed with one explicit zero `GpuStampRecord` immediately after allocation;
- no-stamp fixed steps set the uploaded count to zero and re-prime slot zero before returning;
- `_Stamps` remains bound unconditionally before every `SimulateField` dispatch and `_StampCount` remains zero on no-stamp steps;
- `EvaluateAbilityStampShape` now aggregate-zero-initializes the complete `AbilityStampEvaluation` result before assigning the accepted facing fallback and entering the unchanged shape branches.

**Preserved behavior:** request shapes, shape equations, seeded irregularity, displacement modes, queue capacity and ordering, 96-byte stamp records, 64-byte moving-writer records, historical textures and formats, delayed asymmetric recovery, moving trails, update cadence, session persistence, layer response, tester actions, vegetation shader sampling, immediate interaction, Ground ownership, and memory capacity remain unchanged. The primed record is not evaluated while `_StampCount == 0`.

**Performance reconciliation:** no allocation, buffer, texture, dispatch, draw, shader sample, instance update, or queue scan was added. The corrective safety path uploads one 96-byte zero record at resource creation and on fixed steps with no queued ability request. No `PERFORMANCE EXCEPTION` is active.

**Final reread and dependency audit:** the complete final versions of all three modified files were reread. The request API, tester and tester editor, trample-domain editor, interactor, Ground vegetation root, trample HLSL include, production vegetation shader, renderer, immediate interaction runtime/compute/include, coverage, instance data, cluster geometry, Weather runtime/compute, and `VisualFrameworkDemo.unity` were rechecked as direct consumers or frozen contracts and remain byte-identical to the accepted INTERACT.2B baseline.

**Validation evidence:** the dedicated INTERACT.2B.1 validator passed `52 / 52` checks before this audit entry. It verified exact scope, C#/compute delimiter and preprocessor balance, deterministic zero-record upload after allocation and in the empty-queue path, zero stamp count, unconditional buffer binding before dispatch, full HLSL result initialization, exact isolation of both source changes, retained record strides and queue/recovery/shape contracts, absence of Weather scope, and byte identity of every unscoped project file. The final delivered validator was rerun after this audit entry and records `54 / 54` checks.

The original source-validation environment lacked Unity. User runtime evidence after this patch no longer reported the `_Stamps` binding error, so the zero-record/binding correction is accepted. The aggregate initialization did not remove the D3D11 warning and is retained only as a failed historical attempt.


## VEG-V2-INTERACT.2B.2 — D3D11 Scalar Ability-Shape Evaluation Fix

**Status:** user-validated and accepted in Unity; circle, cone, and line stamping works as expected and the D3D11 warning no longer appears.

### Objective and acceptance criteria

Remove the remaining D3D11 compute warning:

```text
Shader warning in 'CS_VegetationTrampleField': use of potentially uninitialized variable (EvaluateAbilityStampShape) at kernel SimulateField
```

Acceptance requires:

- the ability-shape evaluator no longer returns `AbilityStampEvaluation` or any aggregate structure;
- every shape-evaluation output is explicitly initialized before shape logic;
- shape evaluation has one scalar return value and no early return paths;
- the ability displacement resolver consumes explicit scalar/vector arguments rather than an aggregate evaluation record;
- circle, cone, line, seeded edge irregularity, displacement modes, overlap rules, delayed recovery, persistent recovery, queues, buffers, textures, update cadence, and shader consumers remain unchanged;
- Unity D3D11 compute import produces no potentially-uninitialized warning.

### Reviewed evidence

- `Assets/AGENTS.md` was read completely. It requires this canonical plan as the first project write, exact scope, implementation strictly from the plan, and a recorded final consistency/compliance audit.
- User runtime evidence after INTERACT.2B.1 still reports `use of potentially uninitialized variable (EvaluateAbilityStampShape)` at `SimulateField`. Therefore `(AbilityStampEvaluation)0` did not satisfy Unity's D3D11/FXC definite-assignment analysis.
- `Assets/Game/Rendering/Vegetation/Resources/PS3DVegetation/Compute/CS_VegetationTrampleField.compute::EvaluateAbilityStampShape` currently returns `AbilityStampEvaluation` through multiple early-return branches. The aggregate is zero-cast at function entry, but the warning remains at the call in `EvaluateTarget`.
- `CS_VegetationTrampleField.compute::ResolveAbilityDisplacementDirection` consumes only `radialVector`, `centrelineVector`, and `facingDirection`; the aggregate record is not an architectural contract and can be removed without changing the stamp buffer or public API.
- `VegetationTrampleStamp.cs`, `VegetationTrampleDomain.cs`, `VegetationTrampleStampTester.cs`, both relevant editors, and the complete current compute file were read as the producer, uploader, caller, and test consumers. Their data layout and runtime contracts require no change.
- INTERACT.2B and INTERACT.2B.1 sources were compared. INTERACT.2B.1 changed only zero-buffer priming and aggregate zero initialization; the buffer correction remains accepted and frozen.
- No `.git` metadata or standalone D3D11 HLSL compiler is present in the supplied workspace. Historical comparison therefore uses the accepted patch archives and current reconstructed source.

### Approved scope

Modify only:

```text
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Game/Rendering/Vegetation/Resources/PS3DVegetation/Compute/CS_VegetationTrampleField.compute
```

Create/delete/move/rename: none. C# runtime, request API, tester, editors, Ground, immediate interaction, vegetation shaders/includes, Weather, scenes, prefabs, materials, coverage, instance data, geometry, layers, tags, and URP settings remain frozen.

### Implementation sequence

1. Record this plan and the failed INTERACT.2B.1 runtime evidence.
2. Remove `AbilityStampEvaluation` and replace the aggregate-return evaluator with `float EvaluateAbilityStampShape(..., out float2 radialVector, out float2 centrelineVector, out float2 facingDirection)`.
3. Explicitly initialize all three vector outputs and the scalar influence at function entry.
4. Replace all early returns with guarded branches and one final scalar return.
5. Change `ResolveAbilityDisplacementDirection` to accept the three explicit vectors.
6. Change the ability-stamp loop to initialize local vectors, call the scalar evaluator, and pass explicit vectors to the resolver.
7. Run exact-scope, syntax, function-contract, single-return, no-aggregate, shape-equation, frozen-behavior, and frozen-dependency checks; reread both modified files and direct contracts; record the final audit here.

### Invariants and non-goals

- The accepted `_Stamps` zero-record priming and unconditional buffer binding remain unchanged.
- No stamp record, buffer stride, queue limit, texture format, memory allocation, dispatch count, shader sample, or gameplay API changes.
- No geometry or visual tuning change is authorized. Circle, cone, and line coverage must be mathematically identical to INTERACT.2B.1.
- No Weather work is included.

### Risks and validation

- Risk: changing control flow could alter boundary behavior. Mitigation: preserve every threshold, noise sample, radius/width calculation, cone-angle calculation, and influence equation verbatim; validate source equivalence with deterministic CPU mirrors over representative inputs.
- Risk: D3D11 may still reject `out` definite assignment. Mitigation: initialize caller locals and assign every `out` value at evaluator entry; use one final return and no aggregate result.
- Unity compilation and D3D11 compute import remain required runtime validation because no Unity or FXC compiler is available in this environment.

### Plan status

| Item | Status |
| --- | --- |
| INTERACT.2B.2.0 Canonical plan and reviewed evidence | Complete |
| INTERACT.2B.2.1 Scalar single-return shape evaluator | Complete and user-validated |
| INTERACT.2B.2.2 Explicit-vector displacement resolver and caller | Complete and user-validated |
| INTERACT.2B.2.3 Source validation and final compliance audit | Complete; accepted by user runtime validation |


### INTERACT.2B.2 post-change consistency and compliance audit

**Actual scope:** matched the approved two-file declaration exactly. Only the canonical vegetation architecture document and `CS_VegetationTrampleField.compute` changed. No file was created, deleted, moved, or renamed. No C# runtime, request API, tester, editor, Ground, immediate-interaction, vegetation-shader/include, Weather, scene, prefab, material, coverage, instance-data, geometry, layer, tag, or URP file changed.

**Intentional difference from INTERACT.2B.1:** the `AbilityStampEvaluation` aggregate and aggregate-return function were removed. `EvaluateAbilityStampShape` now returns only scalar influence, assigns three explicit `out float2` values at entry, uses guarded branches with one final return, and has no early-return path. `ResolveAbilityDisplacementDirection` and the stamp loop consume those explicit vectors directly. Caller locals are also initialized before the call.

**Preserved behavior:** every circle, cone, and line threshold, seeded-noise source, radius/width calculation, cone-angle calculation, edge-irregularity multiplier, smoothstep influence equation, displacement mode, overlap rule, recovery rule, queue/buffer contract, texture format, update cadence, and shader consumer remains unchanged. A deterministic 20,000-case CPU mirror comparison of the INTERACT.2B.1 and INTERACT.2B.2 shape-control flows produced zero mismatches and zero numeric error.

**Accepted prior correction retained:** INTERACT.2B.1 zero-record stamp-buffer priming, zero `_StampCount` behavior, and unconditional `_Stamps` binding remain byte-identical. The current user report contains only the D3D11 shape warning; the earlier missing-buffer runtime error was not reintroduced.

**Performance reconciliation:** no allocation, texture, buffer, dispatch, loop iteration, shader sample, draw, instance update, or runtime API was added. The patch changes only compute-shader local control flow and parameter passing. No `PERFORMANCE EXCEPTION` is active.

**Final reread and dependency audit:** both modified files and the complete final compute shader were reread. `VegetationTrampleStamp.cs`, `VegetationTrampleDomain.cs`, `VegetationTrampleStampTester.cs`, `VegetationTrampleStampTesterEditor.cs`, and `VegetationTrampleDomainEditor.cs` were reread as producer, uploader, caller, and test contracts and remain byte-identical to INTERACT.2B.1. The accepted INTERACT.2B and INTERACT.2B.1 patch sources were compared directly.

**Validation evidence:** the dedicated INTERACT.2B.2 validator passed `74 / 74` checks before this audit entry. It verified exact two-file scope, no creates/deletes, no Weather change, compute delimiter and kernel balance, absence of the aggregate evaluation type, scalar single-return evaluation, explicit output initialization at both callee and caller, explicit-vector resolver wiring, preservation of all shape constants/equations and critical buffer/recovery contracts, isolation of the compute diff to the planned evaluator/resolver/caller regions, byte identity of every unscoped project file, and 20,000 deterministic old/new shape cases with zero mismatch. The final delivered validator is rerun after this audit entry.

The source-validation environment lacked Unity and FXC. The user subsequently confirmed that the patch works as expected; this closes the D3D11 warning and ability-shape runtime validation for the tested configuration.

## VEG-V2-CLOSE.1 — Accepted Vegetation Interaction Milestone Closure

**Status:** documentation-only closure complete; user-accepted interaction milestone frozen.

### Objective and acceptance criteria

Close the current Vegetation V2 production-ownership and interaction milestone after direct user validation of the immediate field, Ground-owned historical trails, delayed asymmetric recovery, circle/cone/line ability stamps, the empty-stamp buffer correction, and the final D3D11 scalar shape-evaluation correction. This closure changes documentation only. It must not alter runtime C#, editor code, compute shaders, vegetation shaders/includes, serialized assets, scenes, prefabs, materials, Ground, Weather, coverage, instance layout, geometry, layers, tags, or URP settings.

Acceptance requires:

- active status text identifies `VEG-V2-INTERACT.1B` through `VEG-V2-INTERACT.2B.2` as user-validated and accepted;
- the accepted runtime contracts are summarized without replacing their detailed historical plans and audits;
- the proposed large interaction stress benchmark is explicitly not scheduled because the user states realistic simultaneous actor scale is approximately five and at most ten, while a hundred on-screen interactable actors would already be constrained by non-vegetation systems;
- no unsupported performance claim is introduced: source cost models remain analytical, the user-observed behavior remains user evidence, and no new profiler measurement is claimed;
- the next vegetation implementation objective remains unselected for the new thread rather than being silently replaced by benchmarking, LOD, culling, snow, or another speculative feature;
- all vegetation runtime and rendering files remain byte-identical to the accepted `VEG-V2-INTERACT.2B.2` source baseline.

### Reviewed evidence

- `Assets/AGENTS.md` was read completely. It requires a read-only review, a persistent canonical plan as the first project write, exact affected-file scope, implementation strictly from the plan, and a recorded final consistency/compliance audit.
- The supplied source set contains no `.git` directory. The local interaction reconstruction is `/mnt/data/veg_2b2_workspace/current`, derived from the user-supplied `Assets-Code-Archive(8).zip` and accepted patch overlays through `VEG-V2-INTERACT.2B.2`; no replacement Git clone was used. Because `VEG-V2-INFRA.3` represented deletions through an external apply script, the non-destructive local overlay still contains legacy benchmark files that the accepted cleanup declared obsolete. Those stale local files are outside this closure patch and do not override the user's applied Unity project state.
- The user supplied an INTERACT.1B runtime report showing `51` recenter dispatches over `1,091` simulation dispatches (`4.7%`) after the previous `88–92%` ratio, and stated that grass responsiveness was fixed.
- The user directly confirmed that INTERACT.2A worked as described, that delayed recovery worked, that the asymmetric slow–fast–slow curve worked as expected, and that circle/cone/line ability stamps worked as expected after INTERACT.2B.2.
- The final D3D11 warning was eliminated only after replacing the aggregate shape-evaluation return with scalar influence plus explicitly initialized vector outputs. The earlier aggregate-zero initialization attempt in INTERACT.2B.1 was insufficient and remains historical evidence, not an active recommendation.
- The user rejected a 48/100-actor interaction stress benchmark as disproportionate to the expected gameplay scale and requested a small closure patch plus an exhaustive continuation handoff.

### Expected affected files

Modify:

```text
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Docs/Stylized_Vegetation_Architecture.md
```

Create/delete/move/rename project files: none. Generated delivery artifacts are outside the project source and are listed in the patch manifest and handoff.

### Implementation sequence

1. Reconcile the canonical document's active status and INTERACT.2B/2B.1/2B.2 status wording with the user's runtime acceptance.
2. Add one concise accepted-milestone summary, preserve the detailed historical plans, and record the rejected stress-benchmark proposal as out of the current continuation scope.
3. Reconcile the exploratory stylized architecture's production sequence so the complete interaction stack is no longer described as Unity-validation-pending.
4. Compare every runtime/rendering vegetation file against the pre-closure baseline and require byte identity.
5. Generate a changed-files-only documentation patch, manifest, source-validation report, and exhaustive continuation handoff.
6. Reread both final documents and record the actual affected-file reconciliation and final compliance audit here.

### Performance and scope conclusion

This update executes only at documentation authoring/import time. It adds no active-gameplay CPU or GPU work, no dirty-triggered rebuild, no memory allocation, no buffer, no texture, no draw, no shader sample, and no build/runtime storage other than a small Markdown increase. The highest-priority active-gameplay cost is unchanged. At the stated realistic scale of approximately five to ten actors, no additional interaction stress benchmark is required by current user acceptance. This is a scope decision, not a measured proof of unlimited scalability. No `PERFORMANCE EXCEPTION` is active.

### Plan status

| Item | Status |
| --- | --- |
| CLOSE.1.0 Canonical closure plan and evidence | Complete |
| CLOSE.1.1 Canonical status reconciliation | Complete |
| CLOSE.1.2 Exploratory architecture reconciliation | Complete |
| CLOSE.1.3 Runtime byte-identity and documentation validation | Complete |
| CLOSE.1.4 Final compliance audit and handoff | Complete |

### VEG-V2-CLOSE.1 post-change consistency and compliance audit

**Actually affected project files:** matched the declared two-file scope exactly. `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md` and `Assets/Docs/Stylized_Vegetation_Architecture.md` were modified. No project file was created, deleted, moved, renamed, generated, or modified unexpectedly.

**Intentional documentation differences:** the canonical status now identifies the production ownership and interaction stack as implemented through `VEG-V2-INTERACT.2B.2`; INTERACT.2B is marked user-validated after its two corrective patches; INTERACT.2B.1 distinguishes the accepted zero-buffer binding correction from its failed aggregate-initialization attempt; INTERACT.2B.2 is marked user-validated; and the exploratory architecture's production sequence now matches the accepted state. The closure records the user's realistic five-to-ten-actor expectation and the explicit decision not to schedule a 48/100-actor stress benchmark.

**Preserved implementation:** every file under `Assets/Game/Procedural/Vegetation` and `Assets/Game/Rendering/Vegetation` is byte-identical to the pre-closure `VEG-V2-INTERACT.2B.2` source baseline. No active-gameplay path, dirty-triggered path, serialization contract, shader contract, buffer, texture, allocation, dispatch, draw, update cadence, queue, recovery curve, coverage field, instance layout, geometry, Ground integration, Weather integration, tester behavior, scene, prefab, material, layer, tag, or URP setting changed.

**Performance reconciliation:** this patch adds only Markdown text. Active-gameplay CPU/GPU cost, dirty-triggered runtime cost, memory, and runtime I/O are unchanged. Repository/build storage increases only by the size of the added documentation text. The decision not to add a large synthetic stress suite avoids new editor tooling and test-only runtime machinery; it does not constitute a measured scalability claim. No `PERFORMANCE EXCEPTION` is active.

**Validation evidence:** the dedicated closure validator passed all checks before this audit entry. It verified no creates/deletes, exact two-file changed scope, byte identity of both vegetation runtime/rendering trees, accepted-status wording, preserved failed-attempt history, recorded `51 / 1,091 = 4.7%` runtime evidence, recorded five-to-ten actor scope, explicit rejection of the large stress benchmark, an unselected next implementation objective, balanced Markdown fences, one H1 per document, and no generated artifacts inside the project. The validator is rerun after this audit entry, and the delivered report contains the final count.

**Unity validation:** no Unity validation is required for this documentation-only closure because no Unity-imported implementation or serialized asset changed. The user-provided runtime acceptance is preserved as evidence and is not reclassified as automated or profiler validation.

## VEG-V1C.10 — Weather LightRay punctual-edge direction override

**Status:** source-implemented and statically audited; Unity visual and GPU proof pending.

### Objective

Preserve the accepted `VEG-V1C.9` punctual-light body, attenuation, activation, stability, colour, and edge-radiance contracts. Add one narrow exception: a uniquely tagged Weather LightRay Spot may use the horizontal LightRay/source direction for the blade-edge side selector while retaining the real Spot direction for ordinary body lighting.

### Mathematical contract

For the tagged Spot only:

```text
sourceDirection = -normalize(LightRay travel direction)
horizontal = sourceDirection - Up * dot(sourceDirection, Up)
edgeDirection = normalize(horizontal), when horizontal length² > 1e-6
```

All ordinary point/spot lights continue to use `Light.direction`. The LightRay Spot's body diffuse also continues to use `Light.direction`.

### Identification and layer correctness

- runtime identity bit: `1 << 30`;
- LightRay Spot mask: `Default | identity`;
- no Rendering Layer-name or renderer-mask asset edit;
- production shader compiles `_LIGHT_LAYERS`;
- both additional-light loops apply `IsMatchingLightLayer(light.layerMask, GetMeshRenderingLayer())` before evaluating a light;
- only a matching light carrying the identity bit may select the global LightRay accent direction.

### Approved scope

```text
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Docs/Stylized_Vegetation_Architecture.md
Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl
Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader
```

The Weather controller changes that publish and tag the light are governed by `WEATHER-LIGHT-RAY-V1.1D-AF5`. Vegetation materials, renderers, placement, geometry, wind, interaction, trample, coverage, instance buffers, and authoring controls remain frozen.

### Implementation sequence

1. Add the LightRay global direction and identity-bit helper to `VegetationLighting.hlsl`.
2. Resolve a separate edge direction only for eligible tagged punctual lights.
3. Leave body diffuse and every attenuation/activation calculation on the real `Light` unchanged.
4. Add `_LIGHT_LAYERS` and proper matching to both additional-light loops.
5. Update the concise production architecture summary.
6. Audit compiled paths, ordinary-light preservation, and exact scope.

### Performance and acceptance

AF5 adds no additional light evaluation or texture read. It adds a light-layer test and branch/select inside the already-running punctual loop plus a shader variant keyword. The proof passes only when the LightRay Spot creates coherent stylized blade accents without changing `Hut_Warm_Point`, sun/directional ownership, local body lighting, or existing edge stability.

### VEG-V1C.10 implementation and audit result

Implemented in the declared vegetation scope.

- `_LIGHT_LAYERS` is compiled for the production Forward fragment pass.
- Both additional-light loops query the mesh Rendering Layer and skip nonmatching lights with the standard URP layer test.
- The runtime-only LightRay identity bit is `1u << 30` in HLSL and matches the controller's `1 << 30` constant.
- Only eligible punctual lights call the edge-direction resolver. Ordinary lights return `Light.direction`; a tagged LightRay Spot with an active global returns the published horizontal source direction.
- `VegetationTwoSidedWrappedDiffuse` and all attenuation, colour, activation, gain, and stability equations remain unchanged.
- `GetAdditionalLight` count remains two source calls: clustered-directional and regular punctual loops.
- No vegetation material property, authoring control, texture sample, instance record, field, geometry, interaction, trample, wind, coverage, draw, or allocation changed.

Static audit passed exact four-file vegetation scope, balanced source/preprocessor structure, both layer-match sites, CPU/HLSL bit equality, unchanged light-fetch count, unchanged body-light direction, no new texture sampling, and ordinary-light fallback. The complete cross-subsystem AF5 audit reports `70 / 70` checks passed. Unity compilation and visual/performance acceptance remain pending.



## Historical LightRay vegetation identity sequence — superseded

The following `VEG-V1C.11` through `WEATHER-LIGHT-RAY-V1.1D-AH1` sections are retained as implementation history. Their geometric Spot matching, global shader publication, diagnostic-mode, and diagnostic coverage-bypass behavior are superseded. They must not be read as the current production path. The authoritative current path begins at **Active contract — indexed per-additional-light accent sidecar** below and is closed by `WEATHER-LIGHT-RAY-CLEANUP-V1.3A3-VEGETATION-SIDECAR-CLOSURE`.

## VEG-V1C.11 — Geometric Weather LightRay Spot match

AF5A runtime evidence rejected the `VEG-V1C.10` high Rendering Layer identity bit: the Spot reached vegetation, but the GPU additional-light structure did not expose bit 30. The vegetation shader must not use unregistered Rendering Layer bits as custom light identity.

The replacement publishes one runtime Spot position/range and identifies that Spot by comparing the evaluated additional light's fragment-to-light direction against `normalize(publishedSpotPosition - positionWS)`. A match requires the fragment to be inside the published range and directional agreement of at least `0.999`.

Only the matched Spot's blade-edge selector uses the published horizontal LightRay/source direction. Body lighting, attenuation, colour, activation, stability, and every ordinary point/spot light remain unchanged. The normal URP Rendering Layer match still filters receivers but no longer identifies the Weather light.

The consolidated Weather-controller diagnostic suite remains the only visual diagnostic surface. Orange means no geometric match; green blade-edge strips prove actual matched-Spot edge radiance.


## VEG-V1C.12 — Shared Weather LightRay accent-line master

The Weather LightRay Controller now owns one normalized `Accent Line Intensity`. Vegetation consumes it only after the AF5B geometric Spot match has selected the LightRay-specific horizontal edge direction.

```text
receiver base gain = 4 × vegetation accent response
LightRay multiplier = 2 × controller Accent Line Intensity
final gain = min(4, receiver base gain × LightRay multiplier)
```

Therefore `0` disables only the LightRay-specific blade accent, `0.5` preserves the AF5B baseline, and `1` allows up to twice that baseline without exceeding the established vegetation gain cap. The Spot's normal body lighting and every ordinary point/spot light remain unchanged. No vegetation material property or recipe control is added.


### VEG-V1C.13 — Expanded Weather LightRay accent master

AF5C's controller range was visually underpowered. The matched LightRay vegetation multiplier is now `12 × controller Accent Line Intensity`, still bounded by the established vegetation-local edge-gain cap. Approximately `0.083` preserves AF5B, approximately `0.167` reproduces the former AF5C maximum, and `1` provides up to `12x` AF5B before the local cap. The control continues to affect only the geometrically matched LightRay Spot accent; body illumination and ordinary punctual lights are unchanged.


### VEG-V1C.14 — orders-of-magnitude Weather LightRay accent range

Unity proof showed that AF5D remained severely underpowered because the controller's larger linear multiplier still saturated against the matched-LightRay `min(4, ...)` edge-gain ceiling. The active contract therefore removes that LightRay-specific ceiling and uses an exponential normalized mapping while preserving `0 = off`:

```text
relativeScale(c) = c <= 0 ? 0 : 1001^c - 1

0.00 -> 0x
0.03 -> approximately the former low baseline
0.10 -> approximately 1x the former AF5D maximum
0.20 -> approximately 3x
0.50 -> approximately 31x
1.00 -> 1000x
```

Vegetation first evaluates the former AF5D full-scale gain (`min(4, baseGain * 12)`) only as a reference unit, then multiplies it by the exponential shared-controller scale. This affects only the geometrically matched Weather LightRay Spot accent contribution. Spot body lighting, atmospheric shafts, ordinary punctual lights, and `Hut_Warm_Point` remain unchanged. Future GeneratedMass, river, tree, and other receiver implementations should consume the same normalized controller value but retain receiver-specific equations.

The exponential mapping is evaluated once on the controller CPU and published as `_WeatherLightRayAccentLineResolvedScale`; vegetation performs only a multiply with the resolved scale. The normalized `_WeatherLightRayAccentLineIntensity` remains published as the shared cross-system authoring value. No per-fragment `pow` was added.

### VEG-V1C.15 — proportional AF5F accent reduction

After Unity visual proof found the uncapped AF5E result slightly excessive, AF5F preserves the exponential controller shape but halves the resolved scale at every value:

```text
relativeScale(c) = c <= 0 ? 0 : 0.5 * (1001^c - 1)
```

The matched Weather LightRay vegetation accent therefore receives exactly half the AF5E radiance for the same normalized controller value. The geometric Spot match, body-light path, ordinary punctual-light accents, and vegetation material controls remain unchanged. The reduction is calculated once on the Weather controller CPU.



### VEG-V1C.16 — 200x maximum Weather LightRay accent range

Unity validation found AF5F's upper half still excessive. AF5G preserves the normalized exponential slider shape and reduces the resolved scale to `40%` of AF5F at every value:

```text
relativeScale(c) = c <= 0 ? 0 : 0.2 * (1001^c - 1)
```

Reference points are approximately `0.046x` at `0.03`, `0.20x` at `0.10`, `0.60x` at `0.20`, `6.13x` at `0.50`, and exactly `200x` at `1.00`, all relative to the former AF5D maximum. The matched LightRay accent path is the only consumer changed; real Spot body lighting, geometric matching, ordinary punctual-light accents, and vegetation material controls remain unchanged.

### VEG-V1C.17 — AF5H cached response and zero-output match bypass

`WEATHER-LIGHT-RAY-V1.1D-AF5H` is cleanup and freeze preparation; it does not recalibrate AF5G. The active matched-LightRay response remains exactly:

```text
relativeScale(c) = c <= 0 ? 0 : 0.2 * (1001^c - 1)
```

The Weather controller now caches the effective normalized value and resolved exponential scale. `Mathf.Pow` runs only when the authored value or LightRay-enabled state changes; stable controller updates republish cached values without allocations.

The vegetation include checks a uniform production/diagnostic gate before geometric Spot identity work. When the resolved production scale is zero and diagnostics are inactive, the shader retains the evaluated URP light direction and skips the published-position subtraction, range test, normalizations, and direction-agreement dot product. When the consolidated diagnostic suite is active, matching is forced even at zero artistic output so identity and branch classification remain testable. At zero scale, a successful diagnostic match may remain blue because final edge radiance is intentionally zero; this is not a geometric-match failure.

AF5H retains one published vegetation-accent Spot position/range and therefore supports one simultaneously matched active authored LightRay zone. Central slot capacity is storage only and does not authorize multi-zone vegetation matching. The source default for newly created controllers remains provisional `0.03`; existing serialized controllers retain their stored value and no migration is performed. Ordinary punctual lights, Spot body lighting, atmospheric rendering, geometric threshold `0.999`, and the default receiver Rendering Layer mask remain unchanged. Unity compilation, visual comparison, diagnostics, and target-resolution profiling remain pending.


### VEG-V1C.18 — AH LightRay-only accent participation coverage

`WEATHER-LIGHT-RAY-V1.1D-AH` adds one Weather-controller-owned `LightRay Vegetation Accent Coverage` value in the `0..1` range, defaulting to `1`. It does not scale radiance. Instead, each vegetation blade/card receives a stable noninterpolated candidate hash generated from indirect instance ID and one discrete crossed-card index. Only the geometrically matched Weather LightRay Spot applies the threshold. Ordinary directional lights, ordinary punctual lights, `Hut_Warm_Point`, and LightRay body illumination do not read the coverage value.

Historical VEG-V1C.18 participation was deterministic: `0` selected no candidates, `1` selected all candidates, and intermediate values selected approximately that fraction. Its diagnostic mode bypassed the participation filter for geometric/radiance classification. That diagnostic bypass is superseded and removed by Weather LightRay cleanup V1.3A3; stable whole-card production coverage remains.


### WEATHER-LIGHT-RAY-V1.1D-AH1 — whole-card LightRay coverage identity

AH1 corrected the coverage-selection unit without changing the controller or response equation. The AH centreline input varied between the root, middle, and tip vertex rows, allowing separate triangles of one card to be accepted independently. The benchmark crossed-card mesh contains six vertices per card, so the vertex shader derives `cardIndex = SV_VertexID / 6` and hashes `instanceId + cardIndex`. Every triangle and longitudinal segment of an accepted card retains the complete existing LightRay accent response; rejected cards contribute no LightRay accent. Ordinary lights, body illumination, wind deformation, and accent brightness remain unchanged. The former diagnostic coverage bypass described by this historical patch is removed in Weather LightRay cleanup V1.3A3.

---

## Historical and rejected — Rendering Layer LightRay identity

The attempted registered Rendering Layer bit-7 identity did not produce reliable Weather vegetation accents and is superseded. Runtime LightRay Spots retain the default receiver mask. Rendering Layers remain receiver filtering only and must not be restored as project-specific LightRay metadata.

## Active contract — indexed per-additional-light accent sidecar

Weather LightRay vegetation tuning does not depend on a Rendering Layer identity bit or geometric Spot matching. A pre-render CPU publication step writes one two-`float4` structured record in each camera's own URP additional-light order. The vegetation additional-light loop resolves the same global light index (`GetPerObjectLightIndex` in Forward, direct cluster index in Forward+) and performs one O(1) structured-buffer read.

Record layout:

```text
parameters = strength scale, stable whole-card coverage, edge-profile softness, override weight
sourceDirectionWS = normalized horizontal direction toward the LightRay source, direction-valid flag
```

Ordinary lights receive a zero record and retain the established generic local-light accent response. Weather LightRay Spot proxies receive active-preset values plus their source direction. No nested LightRay search, position comparison, extra light loop, geometric Spot match, or GPU readback is permitted.


### WEATHER-LIGHT-RAY-CLEANUP-V1.3A3 — sidecar closure

V1.3A3 completes removal of the superseded global/geometric diagnostic bridge. Production Weather-LightRay vegetation metadata now has one path only: the indexed per-additional-light sidecar described above. The Controller no longer publishes legacy global Spot position, horizontal direction, intensity, resolved scale, coverage, or diagnostic-mode state; the vegetation include no longer carries diagnostic-only result fields or false-colour resolution; and the benchmark shader no longer contains a diagnostic fragment-return branch.

The protected C#/HLSL record remains exactly two `float4` values. `parameters.w` is an identity/override flag rather than an intensity test: Weather LightRay Spot records retain `w = 1` when resolved intensity is `0`, so Weather-specific edge radiance becomes zero without re-entering the generic punctual-light edge path. Production intensity scale is resolved from the active LightRay preset contract, including preset-transition interpolation; coverage and softness retain their existing semantics. The renderer feature, URP additional-light ordering, camera-local buffer binding, zero fallback record, body-lighting authority, and ordinary-light path are unchanged.


## WEATHER-LIGHT-RAY-V1.2D2 — Vegetation accent control contract closure

The active Weather LightRay vegetation path is a direct indexed per-additional-light sidecar. One GPU record contains two `float4` values and is aligned to each camera's own URP additional-light ordering:

```text
parameters = resolved intensity scale, stable whole-card coverage, edge-profile softness, override active
sourceDirectionWS = normalized horizontal direction toward source, direction valid
```

Protected receiver contracts:

- URP `Light.direction`, attenuation, colour, cone, and range remain authoritative for vegetation body lighting.
- Only a Weather override may use `sourceDirectionWS` for the stylized blade-edge side selector.
- The punctual Spot's radial fragment direction must never be used as the Weather edge-selector direction; doing so creates centre rejection and rim-biased accents.
- Intensity is resolved from the active Weather LightRay preset and scales only Weather edge radiance.
- Coverage is a deterministic whole-card threshold based on instance/card identity. `0` selects none, `1` selects all directionally eligible cards, and intermediate values produce a stable spatially unbiased subset. Coverage does not scale surviving radiance.
- Softness shapes only the selected blade-edge profile after card participation and direction selection. It must not alter the participating-card set, directional gate, attenuation, LightRay footprint, or body illumination. `0.5` preserves the authored vegetation edge mask.
- Ordinary local lights retain the established generic punctual-light path and receive zero Weather records.
- Game and Scene View cameras publish independent sidecars from their own visible-light ordering. Atmospheric LightRay compositing remains Game-camera-only.

The C# and HLSL record layouts are one contract. A change to either layout requires a matching change to both `WeatherLightRayRendererFeature.cs` and `VegetationLighting.hlsl`. Shader-side LightRay searches, geometric matching, and Rendering Layer identity are prohibited.
