# Weather LightRay Architecture and V1 Implementation Plan

## A. Identity and status

**Architecture identifier:** `WEATHER-LIGHT-RAY-V1`

**Current implementation patch identifier:** `WEATHER-LIGHT-RAY-V1.2E-B`

**Decision date:** 2026-07-26

**Status:** `WEATHER-LIGHT-RAY-V1.2E-B` curated Selection and Population assets are serialized and statically audited on top of the V1.2E-A source. Unity import and runtime validation remain pending. The scene is intentionally unchanged, and V1.2D2 vegetation-accent contracts remain frozen.

### A.1 Historical patch plan — `WEATHER-LIGHT-RAY-V1.1D-AH1`

**Objective:** Expose one controller-owned `LightRay Vegetation Accent Coverage` control that changes how many stable vegetation blade/card candidates participate in the matched Weather LightRay accent response. Surviving accent lines retain the existing AF5G/AF5H intensity.

**Approved implementation scope:**

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Docs/Stylized_Vegetation_Architecture.md
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl
Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader
```

The benchmark shader is included because inspection proved that whole-blade/card stability requires one noninterpolated candidate value generated from stable instance/card data in the vertex stage. Fragment world position alone would vary across a blade and under wind deformation, causing partial selection or shimmer.

**Control contract:**

```text
LightRay Vegetation Accent Coverage
Range: 0..1
Default: 1
0 = no matched-LightRay vegetation accent candidates participate
1 = current full participation
```

For stable candidate hash `h` and authored coverage `C`, production participation is `h <= C`, with exact endpoint overrides so `C = 0` selects none and `C = 1` selects all. The test occurs only after `accentOverrideSelected` proves that the evaluated additional light is the geometrically matched Weather LightRay Spot. Diagnostic mode bypasses coverage filtering so geometric-match evidence remains complete.

**Invariants:**

- ordinary directional, point, and Spot light vegetation response remains unchanged;
- `Hut_Warm_Point` remains unchanged;
- LightRay Spot body illumination remains unchanged;
- surviving LightRay accent radiance remains unchanged;
- AG atmospheric contact fading remains unchanged;
- no material property, scene, prefab, light, pass, texture, buffer, render target, or shader variant is added.

**Performance:** One vertex-stage integer hash is generated per rendered vegetation vertex and carried as a noninterpolated scalar; the fragment path adds one matched-LightRay-only uniform threshold gate. No additional light fetch or geometric Spot match is introduced.

**Validation status:** Source implementation and static audit complete; Unity C#/shader compilation, visual density sweep, wind/camera stability, and ordinary-light isolation remain pending.

### A.2 Previous patch — `WEATHER-LIGHT-RAY-V1.1D-AG`

**Objective:** Remove the shared horizontal low-end cutoff produced when all finite atmospheric ribbons terminate at the authored contact plane while retaining substantial mask opacity. Preserve visible shaft-to-ground continuity by extending geometry below the authored contact plane and driving opacity to zero before the actual mesh boundary.

**Approved files:**

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs
Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs
Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayMask.shader
```

**Reviewed evidence:**

- `SH_WeatherLightRayMask.shader` ends every ribbon at `uv.y = 0`, exactly at the authored ground-contact plane.
- The same shader retains `0.58–0.84` opacity at that finite geometry edge through `groundContactFloor`, producing the visible synchronized line at high atmospheric intensity.
- `_WeatherLightRayBeamShape2.w` is already occupied by maximum adjacent-overlap ratio and cannot be reused.
- `_WeatherLightRayEvolution0.w` is written as `0` and has no reader; AG assigns it to `Contact Plane Opacity`.
- The mask already samples and reconstructs scene depth; AG adds no depth sample or render pass.

**Controls:**

1. Existing serialized `groundFade` is relabelled **Ground Contact Fade Length**; range remains `0.001–0.49`, default remains `0.12`, and existing authored values are preserved.
2. New serialized **Contact Plane Opacity** uses range `0–1` and default `0.35`. It affects only atmospheric-mask opacity at the authored contact plane; it does not affect the real Spot or material lighting.

**Mathematical contract:** For authored beam height `H` and resolved per-beam fade fraction `f`, `Lf = Hf`. The fade is split into `0.65Lf` above the contact plane and `0.35Lf` below it. Geometry extends to `-0.35Lf`. Opacity is `1` at and above `+0.65Lf`, equals the authored contact-plane opacity at `0`, and is exactly `0` at `-0.35Lf`. Per-beam fade-length variation is narrowed to `0.85–1.15`; contact opacity receives only deterministic `±8%` modulation.

**Invariants:**

- AF5H vegetation accent matching, caching, diagnostics, and reporting remain unchanged.
- AF5G `0.2 * (1001^c - 1)` calibration remains unchanged.
- Real Spot intensity, geometry, footprint, and Rendering Layer mask remain unchanged.
- Beam side profiles, upper fade, evolution, cloud policies, softening pass, composite, and screen-space surface complement remain unchanged.
- No scene, prefab, material, renderer asset, layer, tag, pass, texture, draw call, shader variant, or depth sample is added.

**Implementation sequence:**

1. Record this plan before code changes. **Complete.**
2. Preserve `groundFade`, add `contactPlaneOpacity`, and extend descriptor construction. **Complete.**
3. Expose the two controls and precise ownership tooltips. **Complete.**
4. Pack contact opacity into `_WeatherLightRayEvolution0.w`; audit every writer and reader. **Complete.** `_WeatherLightRayEvolution0.w` had one writer set to zero and no prior reader; AG is its only reader.
5. Extend ribbon geometry, pass signed axial fraction, replace the nonzero-floor fade, and prevent below-contact receiver preservation. **Complete.**
6. Update controller diagnostics/reporting and current identifier. **Complete.**
7. Re-read the full affected surface, compare the final diff with this scope, and run static checks. **Complete.** Exactly the approved eight files differ from AF5H.
8. Compile and validate Raw, Softened, and Final views in Unity. **Pending — requires the user’s Unity project.**

**Performance model:** One extra descriptor float and clamp; one additional packed constant; tiny per-vertex arithmetic for the extended axial coordinate; bounded per-fragment smooth interpolation and multiplication in already covered mask pixels. No new allocations or scaling loops.

**Non-goals:** Generic blur enlargement, contact haze, per-beam surface lobes, randomized contact heights, a new depth-aware intersection system, atmospheric noise at the foot, multi-zone vegetation work, or any AF5H/AF5G change.

The V1.0C source-neutral controller and validated cloud-projection foundation remain accepted. The following renderer presentations are explicitly rejected and retained only as historical evidence:

- V1.1A/B/C sampled frustum/tube evaluation: circles, ovals, capsule chains, disconnected bars, random convergence, broad white envelope, and nonfunctional surface illumination;
- V1.1D-A literal sparse ribbons: complete geometry but flat white bars, projected overlap, weak softening, and no usable surface-contact proof;
- V1.1D-AB per-beam impact model: one isolated surface lobe per atmospheric beam, detached beam bottoms, mandatory barcode-like gaps, fixed `3–5` proof count, and Raw/Softened views that remained nearly identical;
- V1.1D-AC independently authored Beam Count, Beam Packing, and Footprint Radius: beam span covered only part of the footprint, the camera-facing cross-axis acquired vertical displacement, per-beam drift broke endpoint anchoring, and Raw/Softened remained perceptually redundant.
- V1.1D-AD/AD1 contact proofs: camera/depth-owned contact, then a shared lower contact envelope that fused the beam family into one slab.

No future patch may restore those failed ownership or presentation models without new evidence and explicit approval.

The approved current model is **one authoritative LightRay zone driven by one area diameter**. That single area value derives the shared circular surface footprint and candidate beam count. A deterministic world-X dense-overlap layout distributes unequal beam widths across the exact diameter with positive overlap between every adjacent pair. The atmospheric beams are internal visual structure and never own separate Unity Lights. One pooled shadowless URP Spot Light per active zone is the primary material-lighting response; the former screen-space circular lift is an optional zero-default complement.

```text
One LightRay zone
    ├─ one world-space centre and incoming-light direction
    ├─ one authored Light Ray Area Diameter
    ├─ one derived circular surface footprint
    ├─ one pooled shadowless realtime URP Spot Light
    ├─ one optional zero-default screen-space complement
    └─ 2–12 derived complete parallel atmospheric beam ribbons
         with exact world-X dense overlap, unequal widths,
         asymmetric edge transmission, opacity, softness, and fades
```

The beams contain no mandatory geometric separator gaps. Their visible differences come from stable interval width, bounded adjacent overlap, whole-beam transmission, guaranteed left/right profile modes, phase, and fade variation rather than from broken lengths or independently randomized top and bottom positions.

The approved V1 visual architecture is:

```text
Authoritative persistent world-space LightRay zone
    +
one area-driven exact-diameter world-X dense-overlap layout
    +
2–12 complete parallel asymmetric beam ribbons in one combined draw
    +
full-resolution bounded atmospheric mask
    +
five-sample edge-directed secondary halo
    +
one pooled shadowless realtime URP Spot Light per active zone
    +
optional zero-default full-resolution screen-space circular complement
    +
full-resolution warm atmospheric composite
```

The ribbons are atmospheric-mask inputs, not directly visible low-poly final geometry. Screen-space integration remains mandatory for atmospheric presentation. Surface lighting is hybrid: a real Spot Light is primary, while the screen-space circular lift is optional and defaults to zero.

This document is the canonical architecture, implementation plan, failure register, and validation ledger for Weather-owned stylized celestial light rays. It supersedes the undefined godrays exploration boundary and all stale sampled-frustum, sampled-tube, ellipse-slice, broad-envelope, per-beam-impact, and mandatory-gap contracts.

The feature name is **LightRay**. “Godray,” “sunray,” and “moonray” describe authored or source-specific presentations; they are not separate runtime systems.

## B. Governing source hierarchy

Source authority is resolved in this order:

1. `Assets/AGENTS.md` governs repository workflow, evidence, scope, validation, and Unity constraints.
2. This document governs LightRay ownership, visual architecture, policies, prospective implementation scope, and validation.
3. `Assets/Docs/Weather_System_Architecture_Provisional.md` remains the parent Weather-system record.
4. `Assets/Docs/Weather_Cloud_Shadow_Handoff.md` remains authoritative for the frozen cloud-shadow producer and receiver-cookie contract.
5. `Assets/Docs/Weather_Wind_Architecture.md` remains authoritative for Weather wind.
6. `TimeOfDayController` and its future approved moon extension remain authoritative for celestial source state.

Where this document and the earlier godrays exploration handoff differ, this document is authoritative.

---

## C. Documentation-patch review evidence

The following current source and Unity evidence was reviewed before replacing the stale renderer architecture.

| Evidence | Exact finding | Architectural consequence |
|---|---|---|
| User-supplied V1.1C Game and debug captures | Final output contained a broad saturated shape before settling; Raw Strand Field and Post-Scatter Strands showed oval/capsule clusters and intermittent bars; Surface Illumination remained black. | The current renderer is rejected as a visual proof. The next architecture must create complete beams directly rather than infer them from sampled cross-sections. |
| `SH_WeatherLightRayMask.shader` — atmospheric integration | `sampleCount` is `8`; each covered pixel samples eight positions along the camera ray and calls `WeatherLightRayEvaluateStrands` at every position. | Thin beams appear as isolated depth slices when their width is small relative to the distance between samples. Increasing blur cannot restore geometry that is absent between samples. |
| `WeatherLightRayCommon.hlsl` — strand limit and shape | `WEATHER_LIGHT_RAY_MAX_STRANDS` is `8`. Each strand uses independently generated base/top centres, anisotropic major/minor radii, random cuts, and an elliptical distance mask. | The math directly produces ellipses, capsules, random convergence, and broken-length clusters. These contracts are retired. |
| `SH_WeatherLightRayScatter.shader` — post-processing | The current pass samples seven forward taps and combines directional maximum and weighted average. | It can stretch or soften existing oval marks but cannot create an uninterrupted beam that the source mask never contained. |
| `WeatherLightRayRenderPass.cs` — current proxy and resolution | The pass uses a 12-sided proxy, quarter-linear resolution (`DownsampleDivisor = 4`), one mask pass, one scatter pass, and one composite pass. | Render Graph ownership, quarter-resolution resources, and three-stage hybrid integration remain useful; the proxy mesh and mask math are replaced. |
| `WeatherLightRayController.cs` and authored anchor | The source-neutral controller, cloud policies, source gates, lifetime policies, stable handles, authored descriptor, and primary renderable snapshot already exist. | The replacement reuses controller and behavior ownership rather than restarting the entire subsystem. The per-zone descriptor is revised to produce parallel beam axes. |
| `WeatherCloudShadowController` and V1.0C projection probe | CPU cloud sampling was compiled and visually aligned with the shader Cloud + Sun Openings overlay after shared-focus correction. | Cloud eligibility remains accepted and unchanged. The renderer replacement does not modify the frozen cookie or CPU cloud query. |
| `Assets/Settings/PC_RPAsset.asset` | The PC pipeline requests depth and opaque textures and supports HDR. | Continuous ribbons can use depth-aware soft intersection, and quarter-resolution surface evaluation can reconstruct visible positions without receiver changes. |
| Project target | Unity 6000.5.0f1, URP, constrained top-down isometric gameplay camera, 2560 × 1440 low-end-PC 60 FPS target. | Camera-facing ribbons are a viable stylized specialization, but exact Render Graph, overdraw, and Player cost still require live profiling. |

No external runtime code or asset is copied by this plan. Reference images establish visual principles only: several long separated shafts, restrained warm atmosphere, soft scene integration, and gentle temporal variation.

## D. User-approved objective

Create a configurable, scalable, low-cost Weather LightRay system that:

- produces stylized but subtle shafts descending from above;
- creates a brighter-than-normal world zone where each ray reaches visible surfaces and the ground;
- supports procedural sunrays during the day and future moonrays during the night;
- excludes both source families near their horizon transition periods;
- works under clear sky;
- respects cloud openings by default when clouds participate;
- can ignore clouds for divine, artistic, authored, or gameplay-requested rays;
- can imply a hole through full cloud coverage without modifying the cloud cookie;
- supports timed, permanent, and externally controlled rays;
- remains mostly stationary, with subtle internal intensity evolution rather than obvious sweeping movement;
- reserves optional movement without requiring it in V1;
- exposes stable world-space influence for future gameplay mechanics;
- does not implement healing, damage, buffs, quests, or other gameplay effects inside Weather;
- uses hybrid world-space plus screen-space rendering from the first visible version;
- remains compatible with the constrained top-down isometric camera and remote camera cuts;
- preserves the frozen cloud-shadow receiver contract.

---

## E. Terminology

- **LightRay:** one authoritative world-space zone and its visual presentation.
- **Sunray:** a LightRay using the Sun source profile.
- **Moonray:** a LightRay using the Moon source profile.
- **Procedural ray:** Weather-populated ray selected from deterministic world-space candidates.
- **Authored ray:** persistent or configured ray registered by a scene authoring component.
- **Gameplay-requested ray:** timed or externally controlled ray requested through a runtime API.
- **Source profile:** source-specific colour, intensity, shape, fluctuation, gating, and presentation settings.
- **Cloud-respecting:** placement and continued presentation require valid cloud transmission.
- **Cloud-ignoring:** cloud state does not affect eligibility, lifetime, or intensity.
- **Source-gated:** the celestial source must be available and above its configured elevation/intensity thresholds.
- **Source-override:** the ray may remain active without the natural celestial-source gate.
- **Permanent:** no automatic lifetime expiry. Other policies may still hide the ray.
- **Externally controlled:** a caller owns release and optional parameter changes through a stable handle.
- **Beam cluster:** the variable set of complete atmospheric sub-beams owned by one LightRay zone.
- **Beam axis:** one straight world-space line segment parallel to the active celestial-light direction.
- **Beam ribbon:** one camera-facing quad spanning one complete beam axis. It is drawn only into the atmospheric mask.
- **Light Ray Area Diameter:** the one authored area control; it derives footprint radius, beam count, exact centre pitch, and first-to-last beam-axis span.
- **Atmosphere mask:** low-resolution bounded-opacity data produced by the continuous beam ribbons.
- **Shared footprint:** one horizontal circular world-space surface-light field centred on the LightRay zone contact point. Its radius and the atmospheric beam count/pitch are derived from the same Light Ray Area Diameter.
- **Surface influence:** low-resolution depth-reconstructed data describing visible surface and ground brightening inside the shared footprint.
- **Cloud compensation:** additional stylized surface lift used by cloud-ignoring rays to suggest that divine light penetrates or opens dense clouds.

## F. Core ownership and invariants

### F.1 Weather owns

Weather owns:

- procedural LightRay population;
- active-zone lifecycle;
- cloud eligibility and transition suspension;
- deterministic placement seed and distribution;
- source arbitration between Sun and Moon;
- renderer-facing active-ray data;
- authored and gameplay-request registration;
- stable world-space influence queries;
- LightRay diagnostics and benchmark tooling.

### F.2 Time of Day owns

Time of Day owns:

- authoritative sun direction, colour, intensity, and availability;
- future authoritative moon direction, colour, intensity, and availability;
- day, night, and horizon transition state.

LightRay must consume those values. It must not calculate an independent sun orbit or invent a moon orbit.

### F.3 Cloud system owns

The cloud system owns:

- cloud pattern;
- cloud transmission;
- global world phase;
- wind-driven translation;
- seed evolution;
- the native directional-cookie receiver path.

LightRay may query cloud transmission. It must not mutate the cookie, produce literal holes, duplicate the cloud pattern, or alter receiver materials.

### F.4 Gameplay owns effects

Gameplay systems own:

- healing;
- damage;
- mana restoration;
- buffs and debuffs;
- quest triggers;
- AI reactions;
- scoring or progression.

Weather exposes ray influence only.

### F.5 Hard invariants

- No unpooled procedural GameObject per atmospheric beam.
- No per-beam material.
- No per-beam Unity Light; at most one pooled shadowless realtime Spot Light may represent one active LightRay zone.
- No per-ray trigger collider by default.
- No full-scene renderer or material traversal per frame.
- No GPU readback for placement or gameplay.
- No modification of the frozen cloud receiver-cookie path.
- No new layer or tag without separate approval.
- No simultaneous procedural Sun and Moon source groups in V1.
- No visible ribbon-only LightRay quality tier in V1; the screen-space integration remains mandatory.
- No Compatibility Mode render-pass implementation unless a verified Unity 6 blocker forces a separately approved deviation.

---

## G. Celestial source abstraction

### G.1 Source kinds

The initial type model reserves:

```text
Sun
Moon
```

A future custom celestial source is not required by V1 and must not be added speculatively.

### G.2 Source state contract

The controller consumes one resolved source state:

```text
Source kind
World-space incoming-light direction
Colour
Intensity
Availability
Elevation
Source-profile reference
```

The exact C# representation is selected during `WEATHER-LIGHT-RAY-V1.0`, but the data contract is fixed.

### G.3 Mutual exclusivity

Normal runtime arbitration is:

```text
Day window        -> Sun source may own LightRay presentation
Night window      -> Moon source may own LightRay presentation
Horizon dead zone -> neither procedural source is active
```

Sun and Moon procedural rays never render simultaneously.

The dead zone is controlled by source-profile elevation and intensity thresholds. It must prevent highly diagonal horizon shafts and avoid source ambiguity during sunrise and sunset.

### G.4 Authored exceptions

A permanent authored Sun ray may explicitly ignore the natural source gate. The approved V1 renderer still supports only one enhanced source group per camera.

Therefore an exceptional authored source must do one of the following:

- reuse the currently active source profile;
- force the camera’s LightRay source group to Sun;
- exist only when no conflicting Moon group is active.

Simultaneous heterogeneous Sun and Moon enhancement groups are out of scope because the user does not require that case.

### G.5 Moon limitation

The supplied project has no authoritative moon light. V1 types and renderer data are source-neutral from the first patch, but the first runtime implementation binds Sun only.

Moon runtime support begins only after a separately reviewed Time-of-Day moon contract exists.

---

## H. Source profiles

A `WeatherLightRaySourceProfile` asset is the planned owner of source-specific presentation and gating.

Initial profile groups:

### H.1 Source gate

- minimum source intensity;
- minimum source elevation;
- fade range above the minimum elevation;
- maximum presentation lean;
- horizon dead-zone participation.

### H.2 Population

- desired procedural count or density;
- maximum active procedural count;
- minimum spacing;
- radius range;
- height range;
- lifetime range;
- cluster bias;
- offscreen activation margin.

### H.3 Continuous-beam appearance

- source colour and warmth;
- atmospheric beam intensity;
- one Light Ray Area Diameter, supported authored range `0.60–6.60 m`;
- derived beam count, supported runtime range `2–12`;
- derived centre pitch, never greater than `0.60 m`;
- beam width as a ratio of derived pitch;
- per-beam width, intensity, and softness variation;
- across-width edge softness;
- upper fade length;
- ground-contact transition length with a nonzero contact floor;
- bounded internal brightness variation;
- bounded width breathing;
- projected-width-scaled depth-aware screen-space softening strength.

Beam length is controlled primarily by the LightRay zone height and active source direction. Random hard truncation, independently randomized top positions, independently randomized bottom positions, and visible holes along a beam are not part of the approved Sun profile.

### H.4 Surface and footprint appearance

- one circular footprint radius derived as half of Light Ray Area Diameter;
- one normalized Real Surface Light Intensity controlling the per-zone URP Spot Light;
- one Optional Screen-Space Complement intensity, default `0`;
- one shared Surface Edge Softness controlling both paths;
- source/profile/warmth-derived light colour;
- no Ground/Object intensity split, core-emphasis control, or independent cloud-compensation control.

The surface field and atmospheric layout share one area invariant. Changing Light Ray Area Diameter changes footprint radius, beam count, centre pitch, and exact first-to-last span together. Atmospheric opacity remains independently controllable, and sub-beams never own isolated surface-light circles.

A subtle atmospheric cluster may therefore produce a readable illuminated ground and object zone without becoming an opaque white band.

### H.5 Lifecycle

- fade-in duration;
- hold duration range;
- fade-out duration;
- cloud-covered fade duration;
- source-gate fade duration;
- permanent-ray return fade duration;
- reuse cooldown.

The initial Sun profile and future Moon profile may differ, but they use the same controller, renderer, cloud evaluator, lifecycle, and query contracts.

---

## I. Authoritative LightRay instance model

Each active ray is represented by central controller data, not a scene object.

Required authoritative fields:

```text
Stable instance ID
Source kind
Origin kind: Procedural / Authored / GameplayRequested
Cloud policy: RespectClouds / IgnoreClouds
Lifetime policy: Timed / Permanent / ExternallyControlled
Source-gate policy: RequireActiveSource / IgnoreSourceGate
Movement policy: Static in V1
World-space ground/contact centre
World-space source direction
Zone height
Gameplay influence axes or radius
Light Ray Area Diameter
Derived beam count and centre pitch
Beam width-to-pitch ratio range
Per-beam width/intensity variation
Upper fade length
Ground-contact fade length
Atmospheric intensity multiplier
Real per-zone Spot Light intensity
Optional screen-space complement intensity
Shared surface edge softness
Warmth/colour multiplier
Bounded fluctuation settings
Spawn time
Fade-in duration
Hold or expiry time
Fade-out duration
Lifecycle state
Current authoritative intensity
Current cloud viability
Gameplay channel or identifier
Deterministic variation seed
```

The controller or renderer derives the individual parallel beam axes from the stable zone centre, source direction, beam count, spread, and deterministic seed. Authored and procedural rays use this same descriptor.

Renderer-only fluctuation is derived from stable seed and time. It does not alter authoritative gameplay influence unless a later plan explicitly couples them.

### I.1 Origin kinds

```text
Procedural
Authored
GameplayRequested
```

### I.2 Cloud policies

```text
RespectClouds
IgnoreClouds
```

### I.3 Lifetime policies

```text
Timed
Permanent
ExternallyControlled
```

### I.4 Source-gate policies

```text
RequireActiveSource
IgnoreSourceGate
```

### I.5 Movement policies

V1 implements only:

```text
Static
```

Reserved future modes:

```text
CloudLocked
LimitedWander
FollowTarget
```

These reserved names do not authorize implementation.

---

## J. Clear-sky procedural distribution

### J.1 Deterministic world cells

Procedural candidate placement uses hashed world-space cells rather than camera-relative random points.

Each cell deterministically derives candidate positions from:

- global LightRay seed;
- integer cell coordinate;
- candidate index within the cell;
- stable blue-noise or hash offsets.

This preserves placement under ordinary camera motion and prevents visible reshuffling.

### J.2 Active region

The controller evaluates cells around a resolved gameplay or camera focus plus an offscreen margin.

Remote camera cuts may change the evaluated region immediately, but existing world-space candidates remain deterministic. Population may repopulate gradually after a cut.

### J.3 Count semantics

Desired count is a target, not a guarantee.

The controller must not:

- force rays into invalid cloud regions;
- violate minimum spacing;
- repeatedly stack rays in one clearing;
- create unstable short-lived rays solely to satisfy count.

Dense or fast cloud coverage may reduce the active cloud-respecting population.

### J.4 Distribution controls

Initial controls:

- seed;
- desired count;
- maximum count;
- minimum spacing;
- cell size;
- candidates per cell;
- mild cluster bias;
- offscreen margin;
- spawn evaluation rate;
- repopulation rate.

V1 does not add numerous named distribution modes before visual evidence requires them.

---

## K. World and surface validation

Candidate validation is staged and bounded.

### K.1 Cheap rejection

Before physics queries:

- source gate passes;
- count budget has capacity;
- candidate lies in the active region;
- spacing passes;
- cloud policy passes or is ignored;
- cloud-transition policy allows evaluation.

### K.2 Ground acquisition

The candidate then resolves a world surface through configured raycasts.

Required controls:

- explicit physics mask;
- maximum search distance;
- maximum accepted slope;
- optional height range;
- optional water allowance.

No new layer or tag is assumed. The correct mask must be selected during implementation approval and Unity integration.

### K.3 Major-geometry sun or moon occlusion

Optional source-direction occlusion may reject candidates under substantial geometry.

The initial planned test uses a small bounded sample set from the footprint toward the source. It must not treat grass, characters, or small transient props as placement blockers by default.

The exact mask and blocker classes remain an implementation approval item.

### K.4 Validation cadence

- Ground and static occlusion checks occur at spawn or authored registration.
- Permanent rays may recheck at a low cadence when explicitly configured.
- No per-frame physics query is permitted for every ray.

---

## L. Cloud integration

### L.1 Source-neutral transmission contract

LightRay requires a Weather-owned query conceptually equivalent to:

```text
TrySampleCloudTransmission(
    world position,
    celestial source direction or transform,
    out transmission)
```

The query must:

- sample the exact current generated cloud field;
- use the same world period and world phase as cloud shadows;
- project through the supplied celestial-source direction;
- return retained direct-light transmission in `[0, 1]`;
- avoid GPU readback;
- avoid a second procedural cloud evaluation;
- remain valid when the sun cookie is not installed, including future nighttime Moon use;
- expose a stable/unstable result during cloud seed evolution.

The exact CPU projection must be validated against the live shader path `SampleMainLightCookie(positionWS)` before procedural placement is accepted.

### L.2 No-cloud behavior

Cloud-respecting rays treat the environment as clear when no usable Weather cloud field participates.

Examples:

- no published cloud controller;
- cloud shadows intentionally disabled as a clear-sky state;
- no generated cookie available after an explicit fallback decision.

An error state must be reported rather than silently interpreted as clear sky if a published enabled cloud controller fails generation.

### L.3 Footprint-wide eligibility

A candidate samples:

- centre;
- perimeter points;
- an optional intermediate ring for large rays.

The full usable footprint must meet the configured transmission threshold and safety margin.

### L.4 Ordinary motion prediction

Outside seed evolution, cloud motion is predictable translation.

A candidate is tested at bounded future times covering the required minimum visible lifetime:

```text
spawn
end of fade-in
middle of hold
near beginning of fade-out
```

The candidate is accepted only when the required footprint samples remain valid for the configured forecast window.

The forecast runs only at spawn-evaluation cadence. It is not a per-frame simulation.

### L.5 Seed-evolution suspension

Dual-pattern evaluation is not required in V1.

When `WeatherCloudShadowController.EvolutionInProgress` becomes true:

#### Procedural cloud-respecting rays

- stop spawning;
- enter graceful fade-out;
- release their active slots after fade-out;
- remain suspended during the unstable transition.

#### Permanent authored cloud-respecting rays

- remain registered;
- fade out;
- are not destroyed;
- reevaluate after the resume threshold;
- fade back in only if the location is valid under the mostly established new pattern.

#### Gameplay-requested cloud-respecting rays

- follow their caller-selected lifetime;
- visually suspend or fade according to the same cloud rule unless the request explicitly uses `IgnoreClouds`.

#### Cloud-ignoring rays

- remain fully active;
- do not participate in transition suspension;
- do not sample outgoing or incoming masks.

Default resume threshold:

```text
Evolution progress >= 0.80
```

This is an initial centralized engineering default and remains tunable before runtime freeze.

### L.6 Ignore-cloud presentation

`IgnoreClouds` does not modify or puncture the directional cookie.

Instead, the LightRay system keeps the ray active through its approved cloud policy and applies:

- normal atmospheric shaft radiance;
- one real per-zone Spot Light through receiver additional-light paths;
- an optional zero-default screen-space complement when explicitly authored.

The system does not mutate the directional cookie or add an independent cloud-compensation multiplier. Cloud-ignoring presentation remains stylized, but real receiver lighting is now evaluated by each material rather than reconstructed from final screen colour.

---

## M. Lifecycle and persistence

### M.1 Timed ray

```text
PendingValidation -> FadeIn -> Hold -> FadeOut -> Released
```

The visible minimum lifetime excludes rejected candidates. A ray is not shown until placement and cloud stability pass.

### M.2 Permanent ray

A permanent ray has no expiry. Its current visibility remains controlled by:

- source gate;
- cloud policy;
- author enable state;
- global LightRay enable state;
- explicit renderer/camera eligibility.

A permanent ray may disappear and later return without losing its stable authored identity.

### M.3 Externally controlled ray

A caller receives a stable handle and may:

- release the ray;
- set target intensity;
- update position when the approved movement policy permits it;
- query current state.

V1 does not permit arbitrary callers to mutate shared source profiles.

### M.4 Failed prediction

If a cloud-respecting ray becomes invalid outside a seed transition:

- it does not disappear instantly;
- it enters the configured covered fade-out;
- gameplay influence follows the same authoritative intensity fade;
- it releases or remains registered according to lifetime policy.

---

## N. Mandatory hybrid continuous-beam rendering architecture

### N.1 Outcome

One LightRay zone owns three coordinated presentations:

1. a variable cluster of complete parallel atmospheric beams;
2. one shared circular real-light footprint produced by a pooled shadowless URP Spot Light;
3. one optional zero-default screen-space circular complement.

The renderer does not estimate thin rays by checking isolated depths inside a broad frustum, and it does not treat each atmospheric beam as an independent ground light.

```text
Atmosphere:  \\\ \\\ \\\   complete parallel shafts with varied density
Surface:            ( one shared circular illuminated zone )
```

Continuity is guaranteed by geometry before screen-space processing. Screen-space work softens and composites the atmospheric mask and, only when explicitly enabled, evaluates the optional complement. The primary surface response is a real per-zone Spot Light updated by the controller.

### N.2 Renderer Feature and camera eligibility

The existing Unity 6 URP Render Graph ownership remains:

```text
WeatherLightRayRendererFeature : ScriptableRendererFeature
WeatherLightRayRenderPass : ScriptableRenderPass
```

Render only for the designated gameplay base camera unless an explicit runtime camera contract selects another base camera. Scene preview, reflection, shadow, overlay, thumbnail, and unrelated secondary cameras remain excluded by default.

The intended injection remains after transparents and before post-processing so Vegetation, River, particles, colour grading, and bloom can participate in the final image. The live Unity package remains authoritative for the exact `RenderPassEvent` and camera-colour replacement API.

### N.3 Beam-cluster world-space contract

One active LightRay has one authored area diameter `A`. The immutable descriptor derives the footprint and candidate-beam count:

```text
R = A / 2
segmentCount = ceil(A / 0.60 m)
N = segmentCount + 1
referencePitch = A / segmentCount
```

Every beam shares the normalized incoming-light direction `D`. The contact-layout axis is the fixed world-X axis:

```text
G = (1, 0, 0)
```

This is an approved presentation constraint for the isometric gameplay camera. The zone centre, circle, source direction, lifecycle, and cloud policy remain world-space. Camera motion must not rotate, resize, or relayout the beam family.

The diameter is covered by `N` beam intervals with positive overlap between every adjacent pair. Let `q_i > 0` be deterministic beam-width weights and let `a_i` be deterministic local overlap ratios:

```text
0.28 <= a_i <= 0.50
h_i = min(q_i, q_(i+1)) * a_i
S   = sum(q_i) - sum(h_i)
k   = A / S
w_i = k * q_i
o_i = k * h_i
```

One or two deterministic group boundaries may increase `a_i` up to `0.60`; this creates denser groups, never empty holes.

The intervals are laid out sequentially from the negative-X diameter endpoint:

```text
L_0 = -A / 2
R_i = L_i + w_i
L_(i+1) = R_i - o_i
C_i = (L_i + R_i) / 2

base point B_i = zone base + G * C_i
top point  T_i = B_i - D * zone height
```

Because `k * (sum(q) - sum(h)) = A`:

```text
L_0       = -A / 2
R_(N - 1) = +A / 2
L_(i+1)   < R_i for every adjacent pair
```

The first visible beam geometry begins at one footprint endpoint, the last visible beam geometry ends at the opposite endpoint exactly, and there is no geometric gap between neighbours. There is no independent packing, contact-cell, lower-slab, or camera-relative layout.

Per-beam variation may change width weights, overlap ratios, intensity, guaranteed profile mode, left/right softness, left/right transmission, phase, upper fade, ground transition, and longitudinal density. It must not change the exact outer diameter endpoints, source direction, or continuity along any individual beam.

### N.4 Combined camera-facing ribbon mesh

All beams are emitted in one retained combined mesh and one draw.

Each beam uses one quad:

```text
4 vertices
6 indices
```

Capacity examples:

```text
2 beams  =  8 vertices, 12 indices
4 beams  = 16 vertices, 24 indices
10 beams = 40 vertices, 60 indices
12 beams = 48 vertices, 72 indices
```

Each quad spans continuously from `T_j` to `B_j`. Its width axis is fixed world X and its geometric width is the dense-overlap interval width `w_j` from top to ground contact. The lower edge is not widened, replaced by a contact cell, or blended into a zone-wide slab.

The contact diameter does not rotate with the camera. Camera movement may change projection and foreground occlusion only; it must not change layout axis, beam positions, widths, overlaps, height, footprint, or apparent authored length.

The ribbon mesh is rendered only into an offscreen LightRay mask. It is not the final directly composited material surface.

### N.5 Continuous beam-profile shader

Ribbon coordinates are:

```text
U = across beam width
V = upper end to ground-contact end
```

The atmospheric profile is continuous and intentionally asymmetric:

```text
beam alpha =
    asymmetric left/right density(U, side softness, side transmission, peak bias)
    * per-beam transmission
    * upper fade(V, per-beam variation)
    * ground transition toward a nonzero contact floor(V, U)
    * bounded low-frequency variation(V, time, stable seed)
```

Required properties:

- each beam keeps one uninterrupted geometric interval from top through ground contact;
- the lower geometry is the same width as the rest of that beam and never expands into a shared strip;
- the across-width peak has no flat opaque plateau;
- left and right edges may have different softness and transmission;
- some beams are wider, narrower, dimmer, brighter, harder, or softer than their neighbours;
- upper fade and ground transition are independently varied without hard cuts;
- atmospheric density remains nonzero at the authored ground-contact end so shafts visibly reach the surface;
- time variation never reduces a beam to disconnected islands;
- width breathing may change apparent interior density above contact but must not move the packed interval endpoints;
- beam-base drift is forbidden;
- no circle, oval, capsule, sampled cross-section, contact cell, or shared lower opacity envelope is part of the atmospheric definition.

The one surface footprint remains separate from beam geometry. It must not be represented by a lower atmospheric slab or individual impact sprites.

### N.6 Pass 1 — continuous atmospheric beam mask

Current authored-proof target:

```text
Full camera-resolution R16F
```

The pass:

- draws the combined ribbon cluster once;
- samples camera depth for soft intersection and foreground occlusion;
- writes a continuous atmospheric mask;
- uses bounded opacity-union accumulation so overlap cannot explode to white;
- permits densely packed or partially overlapping beams;
- does not integrate isolated camera-ray samples;
- does not evaluate an elliptical tube field;
- does not render a visible broad envelope.

The exact accumulation format remains subject to live Render Graph validation. It must not require one draw call per beam.

### N.7 Pass 2 — beam softening and optional screen-space complement

The full-resolution atmospheric stage performs the bounded secondary softening task. The optional surface complement remains part of the full-resolution composite and is skipped at its default zero intensity.

#### Atmospheric softening

- read the full-resolution continuous beam mask;
- apply a normalized five-tap cross-width filter with a small projected-width-derived radius;
- add only positive outer halo energy instead of brightening beam interiors;
- use configurable strength as a secondary finishing control;
- preserve the dominant beam direction and never introduce along-beam streak reconstruction;
- avoid depth-weight discontinuity artefacts, broad bundle envelopes, or attempts to manufacture the aperture pattern.

Continuity, width variation, grouped gaps, and left/right asymmetry already exist in Pass 1. The softener is not responsible for creating the reference look. If its separate debug result remains visually redundant or introduces artefacts, the pass and debug view are removed rather than expanded again.

#### Primary real surface response

`WeatherLightRayController` owns one lazy hidden realtime Spot Light per active zone slot. It is placed vertically above the footprint centre, aimed downward, configured without shadows or cookie, and updated from Area Diameter, shared softness, source colour/intensity, lifecycle intensity, and the normalized Real Surface Light Intensity. Receiver materials execute their existing URP additional-light paths; no LightRay receiver shader modification is required.

#### Optional screen-space complement

When Optional Screen-Space Complement is greater than zero, the full-resolution composite may reconstruct visible world position, evaluate the same circular footprint and shared softness, and apply the retained bounded AF3 screen-light response. It is explicitly secondary, defaults to zero, and must not be treated as material lighting.

There is no beam-count surface loop, receiver-type split, cloud-compensation term, core-emphasis multiplier, union of per-beam ground lobes, private surface texture, or surface upsample path.

### N.8 Pass 3 — full-resolution composite

The composite:

- reads active camera colour and camera depth;
- reads the full-resolution Raw and Softened atmospheric masks;
- evaluates the optional circular screen-space complement only when its intensity is nonzero;
- adds restrained warm atmospheric radiance;
- leaves primary material lighting to the controller-owned real Spot Light;
- exposes Raw, Softened, Surface Illumination, and Final Composite debug views; the Surface view diagnoses footprint geometry/complement rather than the real Light contribution;
- writes one destination camera-colour texture;
- replaces frame camera colour when supported instead of performing an unnecessary copy-back.

The composite must preserve scene readability. Pure white is not the default Sun colour, atmospheric opacity is not increased merely to make the footprint visible, and isolated circles beneath individual beams are forbidden.

### N.9 No broad visible envelope

The rejected sampled renderer used a broad envelope that appeared as a white ribbon or searchlight body. The approved renderer has no normal bundle-wide atmospheric envelope contribution.

Densely packed beam halos may overlap naturally, but they remain traceable to varied complete beams rather than a single filled cone. A future separate connective haze requires visual evidence and approval.

### N.10 Temporal behavior

No history buffer or temporal reprojection is required for the current authored proof.

Stability comes from:

- persistent world-space beam axes;
- deterministic per-beam seeds;
- continuous UV-based profiles;
- slow bounded brightness and softness variation;
- small bounded width breathing;
- no camera-relative respawn;
- no random beam truncation;
- one footprint and exact beam span that change together only through Light Ray Area Diameter.

Temporal accumulation may be reconsidered only if the continuous low-resolution mask visibly shimmers after ordinary depth-aware upsampling.

## O. Visual behavior

### O.1 Required zone structure

One LightRay is one coherent illuminated zone containing several complete parallel atmospheric shafts. Depending on authored scale, the zone may use a small cluster or a dense cluster:

```text
small zone:   \\ \\ \\ \\
dense zone:  \\\\\\\\\\\\\\
```

These are multiple complete parallel beams, not disconnected pieces of one beam. The shafts may overlap or nearly meet. Beam spacing must not force a regular barcode.

The ground/surface response is one shared circular field centred on the zone, not one circle per beam.

### O.2 Sunray appearance

The initial Sun presentation must read as:

- subtle;
- gentle;
- warm cream or lightly golden;
- translucent;
- directional;
- formed by complete long uninterrupted beams;
- varied in width, opacity, softness, and visible fade depth;
- capable of dense partial overlap without becoming one opaque white slab;
- visibly connected to the ground-contact plane;
- integrated with foreground occlusion;
- accompanied by one readable but soft circular ground and visible-surface lift.

The environment remains clearly visible through the atmosphere. The surface zone should usually carry more perceptual evidence of sunlight than the atmospheric shafts themselves.

### O.3 Rejected appearances and recorded causes

The following are explicit failures:

- circular or oval lens-flare-like chains caused by sampled cross-sections;
- capsule clusters or broken bars caused by sparse depth sampling and random length cuts;
- one filled cone, prism, cylinder, blob, ribbon, or searchlight body;
- one beam represented as disconnected diagonal pieces;
- several beams that converge because top and bottom offsets are independently randomized;
- sparse, evenly spaced, equal-width white poles that read as a barcode;
- mandatory dark gaps between every beam;
- one isolated circular ground lobe or impact marker per atmospheric beam;
- beam bottoms fading to zero before reaching the authored contact plane;
- Raw and Softened debug views that are perceptually identical;
- broad white screen wash or explosive additive overlap;
- opaque moon-laser appearance;
- rhythmic whole-cluster pulsing;
- animation that cuts visible holes into beam bodies.

One controllable shared circular surface field is required. It must not be implemented as a literal emissive decal that ignores scene depth; it is depth-reconstructed visible-surface illumination.

### O.4 Subtle internal evolution

Each complete beam may vary independently in:

- brightness phase;
- width;
- softness;
- upper and lower transition depth;
- small width breathing;
- low-amplitude longitudinal texture;
- no beam-base drift; only appearance parameters evolve.

The approved motion is slow and bounded. It must not:

- break continuity;
- cause isolated pieces to blink;
- move the footprint centre;
- change the area-derived footprint radius;
- move first/last beam contacts away from the diameter endpoints;
- visibly reshuffle the cluster;
- produce large camera-facing waves.

### O.5 Source direction and stylized lean

The natural Sun profile follows the authoritative Sun direction. A small presentation lean remains optional only if bounded and clearly documented. Authored divine rays may override the source gate but still use one coherent direction and the same zone/footprint contract.

### O.6 Brightness semantics

Atmospheric opacity and surface illumination are independent controls.

- Atmospheric beams should remain restrained and translucent.
- The shared footprint may be stronger and more readable.
- Overlapping beams use bounded accumulation and must not multiply into saturated white.
- Derived Beam Count must not multiply the total surface illumination because the footprint belongs to the zone, not to each beam.

## P. Gameplay-facing contract

Weather exposes influence. It does not execute gameplay effects.

Planned public concepts:

```text
TrySampleLightRayInfluence(worldPosition, out influence)
GetActiveLightRayZones(destination)
TryGetLightRay(stableId, out snapshot)
RequestLightRay(request, out handle)
ReleaseLightRay(handle)
```

### P.1 Influence data

A query result may contain:

- combined influence;
- strongest-ray influence;
- source kind;
- stable instance ID;
- gameplay channel;
- current authoritative intensity;
- origin kind;
- cloud policy.

### P.2 Spatial evaluation

Gameplay uses analytical circle or ellipse checks against active world-space zones.

It must not inspect:

- screen-space masks;
- render textures;
- beam-ribbon meshes;
- material state;
- physics trigger callbacks.

### P.3 Query cadence

Consumers choose a reasonable cadence. Continuous per-frame queries for every actor are not assumed.

A future centralized gameplay influence manager may batch queries if actor counts require it.

### P.4 Visual/gameplay consistency

The visual surface influence and gameplay footprint derive from the same centre, axes, lifecycle intensity, and source state.

Shader-only internal fluctuation remains visual unless explicitly promoted to authoritative intensity in a later approved plan.

---

## Q. Authored and gameplay-requested rays

### Q.1 Authored anchor

Planned component:

```text
WeatherLightRayAnchor
```

The component registers data with the central controller. It does not render or simulate independently.

Planned fields:

- source profile;
- cloud policy;
- source-gate policy;
- lifetime policy;
- permanent or timed configuration;
- transform and local offset;
- gameplay influence radius or axes;
- zone height;
- Light Ray Area Diameter;
- beam width-to-pitch ratio range;
- derived beam count and centre pitch diagnostics;
- upper and ground-contact fades;
- atmospheric, surface-light, warmth, and cloud-compensation multipliers;
- bounded fluctuation controls;
- gameplay channel;
- start-enabled state.

### Q.2 Permanent landmark use

A monument, symbolic landmark, shrine, or important NPC may own a permanent anchor.

Permanent means no expiry. Cloud and source policies remain independent.

### Q.3 Gameplay request

A runtime request may specify:

- source kind/profile;
- world position;
- radius and height;
- timed or externally controlled lifetime;
- cloud policy;
- source-gate policy;
- gameplay channel;
- visual multipliers.

### Q.4 Divine override

A gameplay-requested or authored ray may use:

```text
IgnoreClouds
IgnoreSourceGate
```

This is the explicit “the gods are powerful enough” path. It is not the default procedural Weather policy.

---

## R. Quality and performance contract

### R.1 Mandatory hybrid quality tiers

Every enabled quality tier uses:

- continuous world-space beam ribbons;
- a low-resolution bounded atmospheric mask;
- one low-resolution shared visible-surface footprint;
- depth-aware cross-width softening;
- full-resolution composite.

Quality tiers may scale the maximum permitted area/count pair, mask resolution, softening radius/taps, active-zone count, cloud-validation quality, and offscreen bounds. They may not restore the rejected sampled tube/frustum renderer or per-beam surface lobes.

### R.2 Rejected-renderer arithmetic baseline

At 2560 × 1440, quarter-linear resolution is:

```text
640 × 360 = 230,400 pixels
```

The rejected V1.1C atmospheric mask performed:

```text
8 camera-ray samples
× up to 8 strand evaluations
= up to 64 complex strand evaluations per covered pixel
```

Approximate maximum complex evaluations:

| Broad proxy screen coverage | Rejected sampled-strand evaluations |
|---:|---:|
| 25% | ~3.69 million |
| 50% | ~7.37 million |
| 100% | ~14.75 million |

Those evaluations included hashes, trigonometric evolution, independently interpolated top/base centres, elliptical distances, taper, cuts, and fades. The old directional scatter added seven texture taps per quarter-resolution pixel, approximately `1.61 million` reads when full-screen.

### R.3 Continuous-beam atmospheric cost target

The current atmospheric mask evaluates one simple continuous profile per covered ribbon fragment. One zone remains one draw regardless of the area-derived Beam Count.

Mesh capacity:

```text
2 beams  =  8 vertices, 12 indices
4 beams  = 16 vertices, 24 indices
10 beams = 40 vertices, 60 indices
12 beams = 48 vertices, 72 indices
```

The main variable cost is transparent overdraw and full-resolution mask bandwidth, not vertex count. The aperture partition limits geometric overlap, but wide soft profiles can still overlap in screen space. Area Diameter, derived count, overlap ratios, width weights, and combined screen coverage must all be reported. No GPU-time claim is made before profiling.

### R.4 Unified surface-influence cost target

Surface influence is evaluated directly in the existing full-resolution composite and exits before depth reconstruction outside conservative projected LightRay bounds. Its core evaluation is constant with respect to Beam Count:

```text
one bounds test
+ one depth reconstruction for in-bounds pixels
+ one horizontal point-to-centre distance
+ one bidirectional circular falloff
+ one bounded authored intensity multiplication
```

The rejected V1.1D-AB path performed one point-to-segment check per atmospheric beam and unioned separate lobes. That loop is removed. Changing from four to ten atmospheric beams must not multiply surface arithmetic. AF3 also removes the former quarter-resolution surface pass, private surface texture, four-sample surface upsample, and depth-weight loop.

### R.5 Softening cost target

At `2560 × 1440`, a full-resolution five-tap pass is:

```text
3,686,400 × 5
= ~18.43 million texture reads
```

The filter remains cross-width only, normalized, halo-only, and bounded to `2–12` pixels. It no longer performs per-tap scene-depth reconstruction. Its cost must be justified by a clean and materially useful Softened view; a visually redundant pass is rejected overhead.

### R.6 Memory and pass target

Current transient targets are approximately:

```text
Full-resolution atmospheric mask: R16F
Full-resolution softened mask: R16F
Full-resolution composite destination: active camera format
```

AF3 removes the private surface texture entirely. Exact allocation and Render Graph aliasing must be reported from Unity. Steady one-zone proof work:

- one combined beam-cluster mask draw;
- one full-resolution five-tap softening pass;
- one full-resolution composite that evaluates the bounded surface field directly;
- no separate surface raster pass;
- no compute dispatch;
- no per-beam draw call;
- no per-ray Unity Light;
- no recurring managed allocation after warm-up.

### R.7 Expected performance conclusion

The current model is expected to be materially cheaper in arithmetic than the sampled-volume renderer and cheaper in surface arithmetic than V1.1D-AB because the per-beam surface loop is gone.

The whole effect is not expected to improve by the same mathematical ratio because atmospheric overdraw, texture bandwidth, Render Graph overhead, softening, and full-resolution composite remain. Exact milliseconds are unknown until implemented and profiled.

### R.8 Primary performance risks

Primary risks are:

- dense transparent-ribbon overdraw at large Area Diameter / high derived Beam Count;
- full-screen five-tap softening and two full-resolution R16F atmospheric textures;
- conservative surface bounds for a large area-derived footprint;
- full-resolution camera-colour bandwidth.

The system must report and constrain:

- active LightRay count;
- Area Diameter;
- derived beam count and pitch;
- width-ratio range;
- approximate combined screen coverage;
- derived footprint radius and projected bounds;
- mask resolution;
- softening taps/radius;
- pass execution;
- transient memory.

Maximum ray count alone is insufficient evidence.

### R.9 Acceptance budgets

The existing project targets remain:

```text
Incremental GPU median at Medium maximum-ray case: <= 1.0 ms
Incremental GPU high-percentile stress delta: <= 1.5 ms
Incremental CPU median: <= 0.20 ms
Recurring managed allocation after warm-up: 0 B/frame attributable to LightRay
```

These budgets are not claimed as passed by source or static validation.

## S. Diagnostics and benchmark policy

### S.1 Normal renderer debug views

The authored proof exposes:

```text
Final Composite
Raw Continuous Beams
Softened Continuous Beams
Surface Illumination
```

`Raw Continuous Beams` must show complete parallel shafts and the direct point-sampled quarter-resolution mask. `Softened Continuous Beams` must be perceptually softer while preserving direction and avoiding a broad envelope. `Surface Illumination` must show one contiguous field whose size follows Light Ray Area Diameter.

Normal Surface Illumination diagnostics:

- green/cyan: unified receiver influence;
- red: one shared footprint boundary and exact diameter line;
- yellow: the two diameter endpoint markers touched by the first and last beam axes;
- blue: one authored footprint centre.

Per-beam impact circles and per-beam red base markers are rejected normal diagnostics because they imply the wrong ownership model. Temporary private implementation diagnostics for depth validity or projected bounds may exist during repair work but must not become permanent Inspector clutter without evidence.

### S.2 Inspector and report

The LightRay controller Inspector must provide:

- one comprehensive diagnostic report;
- one copy-to-clipboard action;
- explicit source, cloud, lifecycle, renderer, camera, and quality state;
- active-zone Area Diameter, derived radius/count/pitch, and width-ratio records;
- current projected bounds and approximate coverage;
- pass/resource state;
- complete error text.

The authored anchor must expose one Light Ray Area Diameter, Beam Width / Pitch Ratio, Beam Softness Variation, and the existing atmosphere/surface controls. Rejected count, packing, footprint-radius, absolute-width, and lateral-drift values remain hidden legacy serialization only.

### S.3 Benchmark suite

One button must run the complete suite and restore captured state on every exit path.

Required cases:

- LightRay disabled baseline;
- enabled with zero active rays;
- one area that derives `4` beams (`1.80 m`);
- one area that derives `10` beams (`5.40 m`);
- maximum supported `12`-beam / `6.60 m` stress zone;
- minimum and maximum Light Ray Area Diameter proof;
- exact endpoint/diameter alignment proof;
- minimum and maximum width-ratio stress;
- clear-sky rays;
- cloud-respecting rays;
- cloud-ignoring rays under full shade;
- seed-evolution suspension and resume;
- heavy Vegetation, River, and Generated Mass view;
- camera cut or retarget;
- permanent, timed, and externally controlled lifetimes.

### S.4 Render validation tools

Required checks:

- Frame Debugger;
- Render Graph Viewer;
- GPU Profiler;
- gameplay-camera final capture;
- all four normal debug captures;
- standalone Player benchmark;
- representative low-end PC capture.

## T. Patch history and next authorized sequence

### Historical rejected stages

- `WEATHER-LIGHT-RAY-V1.1A/B/C`: sampled-volume renderer; rejected.
- `WEATHER-LIGHT-RAY-V1.1D-A`: continuous ribbon foundation; structurally useful, presentation rejected.
- `WEATHER-LIGHT-RAY-V1.1D-AB`: camera-readable ribbons and first surface pass; rejected because it assigned one surface lobe to each atmospheric beam and retained sparse, detached presentation.
- `WEATHER-LIGHT-RAY-V1.1D-AC`: unified footprint attempt; rejected because area, count, packing, and footprint remained independently authorable, beam contacts used a nonhorizontal axis, base drift broke anchoring, and Raw/Softened remained redundant.
- `WEATHER-LIGHT-RAY-V1.1D-AD`: exact area-driven centreline layout; rejected because depth still owned visible contact and centre axes did not define visible outer edges.
- `WEATHER-LIGHT-RAY-V1.1D-AD1`: visible-edge contact-cell proof; rejected because the shared lower contact envelope widened and fused every beam into a bright slab, the camera-derived contact axis produced the wrong composition, and global symmetric softening could not replace aperture-like variation.

Historical ledgers remain below as evidence of what was tried, what failed, and why. They are not alternative current architectures.

### `WEATHER-LIGHT-RAY-V1.1D-AE` — rejected world-X gap partition

The AE proof retained exact world-X outer endpoints and constant beam width through contact, but it explicitly reserved `12%` of the area for empty separators. Unity evidence rejected the resulting barcode gaps and showed that its probabilistic left/right variation remained visually near-symmetric. AE is historical evidence, not the current source contract.

### `WEATHER-LIGHT-RAY-V1.1D-AF` — dense-overlap asymmetric authored proof

Current source scope:

- preserve one Area Diameter, one footprint, area-derived count, exact world-X outer endpoints, and one combined draw;
- replace mandatory gaps with positive overlap between every adjacent beam;
- keep every beam width/profile unchanged through ground contact instead of widening into a lower slab;
- guarantee a repeating mixture of hard-left/soft-right, soft-left/hard-right, unequal-bilateral, and directional-transmission profiles;
- bound raw per-beam transmission below full white so atmospheric shafts remain subordinate to surface light;
- retain full-resolution atmosphere and contact-depth protection from AD1;
- retain screen softening only as a small edge-directed secondary halo;
- retain one shared surface field, source/lifecycle/cloud ownership, and no procedural spawning.

Status: dense-overlap atmospheric structure remains current. AF3 supersedes its surface-control and surface-composite contract; Unity validation remains pending.

### `WEATHER-LIGHT-RAY-V1.1D-AH` — calibration, profiling, and authored-proof freeze

Deferred until the active V1.1D-AG contact-fade patch is compiled and visually accepted.

Proposed scope:

- calibrate warm atmospheric intensity, footprint lift, contact floor, width weights, overlap ratios, and asymmetric transmission against the approved references;
- benchmark disabled, zero-zone, derived four-beam, derived ten-beam, twelve-beam, minimum-area, maximum-area, and heavy-scene cases;
- verify allocations, pass order, transient memory, projected coverage, and Raw/Softened usefulness;
- freeze the authored proof or document rejection before procedural population.

Only after the authored proof and AG contact-fade validation pass may the future sequence resume:

```text
V1.2 procedural distribution and cloud-safe population
V1.3 runtime requests and gameplay influence queries
V1.4 Moon source integration after an authoritative Moon contract exists
V1.5 complete benchmark and V1 freeze
```

## U. V1 acceptance criteria

### U.1 Architecture

- One central Weather controller owns active LightRay zones.
- Sun and future Moon share one source-neutral architecture.
- Cloud, source, lifetime, origin, and movement policies remain independent.
- The cloud receiver-cookie and validated CPU-query path remain unchanged.
- Authored and future procedural systems use the same zone, beam-cluster, and shared-footprint contract.
- Atmospheric beams are internal visual structure and never independent surface lights.

### U.2 Beam structure

- One LightRay Area Diameter derives `2–12` complete parallel beams.
- `1.80 m` and `5.40 m` authored areas derive four and ten beams through the same renderer path.
- Every beam is straight and parallel to the active source direction.
- The exact diameter is partitioned into unequal beam intervals plus a bounded `12%` grouped aperture-gap budget.
- Width, whole-beam transmission, left/right softness, left/right visibility, phase, and fade variation prevent a uniform barcode.
- Beams remain continuous from upper fade through visible ground contact.
- No sampled discs, circles, ovals, capsules, lens-flare chains, or random hard cuts remain.
- No broad filled envelope, searchlight cone, or saturated white ribbon dominates.

### U.3 Surface and visual quality

- Exactly one shared circular surface field belongs to each LightRay zone.
- Light Ray Area Diameter controls footprint radius, beam count, and the exact first-visible-edge to last-visible-edge span as one invariant.
- No isolated per-beam impact circles remain.
- Sun beams are warm, subtle, gentle, translucent, and directional.
- The scene remains readable through the atmosphere.
- Internal evolution is slow, independent, and non-mechanical.
- Ground and visible objects receive a readable soft lift.
- Surface lift does not require opaque atmosphere.
- Foreground geometry interrupts or soft-fades beams coherently.
- Cloud-respecting and cloud-ignoring policies remain visually understandable.

### U.4 Camera and behavior

- The authored zone centre and circular footprint remain world-space and camera-independent.
- The presentation contact diameter is fixed parallel to world X; camera movement must not rotate or relayout it.
- Every beam remains parallel to the source direction and every base remains on the authored horizontal contact plane.
- Remote camera cuts do not move authored zones or footprint centres.
- Source horizon gates disable extreme procedural beams.
- Lifetime and gameplay influence remain synchronized with visual intensity.

### U.5 Performance

- One combined beam-cluster draw, one full-resolution five-tap softening pass, one quarter-resolution constant-cost surface pass, and one full-resolution composite are the intended authored-proof work.
- No recurring managed allocation after warm-up.
- No per-beam draw call.
- No per-beam surface loop.
- No full-resolution multi-beam distance loop without measured justification.
- Section R budgets pass or the architecture is reopened before procedural spawning.

### U.6 Validation

- Unity 6000.5.0f1 compilation passes.
- No shader warning or unsupported Render Graph use remains.
- Raw, Softened, Surface Illumination, and Final Composite outputs are all useful and materially distinct.
- Surface Illumination proves one world-X footprint diameter whose endpoints align with the first and last visible beam geometry edges at multiple area sizes.
- Frame Debugger and Render Graph Viewer confirm expected pass order and resources.
- Standalone Player and low-end-PC measurements are recorded before freeze.

## V. Risks and mitigation

| Risk | Cause | Required mitigation |
|---|---|---|
| Fixed world-X ribbon becomes edge-on | A future camera orientation looks nearly along world X. | The current isometric camera contract accepts world X; reopen orientation only with a new camera requirement. |
| Ribbon/card edges become visible | Insufficient asymmetric density profile or halo. | Use per-side density/softness/transmission and a small five-tap halo-only filter. |
| Cluster becomes one opaque slab | Shared lower envelopes, excessive overlap, or saturated transmission. | Forbid shared contact slabs, partition a fixed beam/gap budget, and keep whole-beam transmission at or below `1`. |
| Cluster becomes a barcode | Equal intervals, equal gaps, symmetric edges, or uniform transmission. | Use deterministic width weights, grouped gap weights, peak bias, and independent left/right softness/transmission. |
| Beam ends appear detached | Lower alpha reaches zero before the contact plane. | Retain a nonzero contact floor and validate against visible Ground and object depth. |
| Surface influence becomes separate spots | Surface ownership follows atmospheric beams. | Evaluate one zone-centred circular field with no beam loop; derive both atmosphere layout and radius from Area Diameter. |
| Shared footprint lights unrelated elevated geometry | The field is depth-reconstructed and receiver-agnostic. | Keep separate Ground/object strengths, bound authored radius, and audit receiver-specific integration separately if evidence requires it. |
| Softening costs bandwidth or scratches depth edges | Radius is excessive or per-tap depth weights are discontinuous. | Use a `2–12 px` five-tap halo-only filter and remove it if the result remains redundant. |
| Overdraw becomes expensive | Derived count or width ratios produce excessive overlap. | Report Area Diameter, derived count/pitch, width ratios, projected coverage, and quality-tier limits. |
| Full-resolution composite dominates cost | Camera-colour bandwidth remains regardless of simpler beam math. | Replace frame camera colour instead of copy-back and inspect Render Graph aliasing. |
| Source angle creates extreme screen lengths | Low-elevation Sun or Moon. | Preserve source elevation gate and bounded visual lean. |
| Cloud compensation looks like an overlay | Stylized compensation is material-agnostic. | Keep it separately controlled, bounded, and inactive in normal clear sunlight. |
| Procedural work begins before visual proof | Pressure to add population despite unresolved renderer quality. | Keep procedural spawning blocked until the V1.1D-AF structural proof and V1.1D-AG profiling/freeze pass. |

## W. Documentation-patch exact scope

`WEATHER-LIGHT-RAY-DOC-V1.1D-CONTINUOUS-BEAMS` changes Markdown only.

Modify:

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Docs/Weather_System_Architecture_Provisional.md
Assets/Docs/Weather_Cloud_Shadow_Handoff.md
```

No C#, shader, HLSL, scene, prefab, material, profile, Renderer Feature, renderer asset, RP asset, layer, tag, Ground, Generated Mass, Vegetation, River, actor, or gameplay file change is authorized.

---

## X. Documentation-patch consistency and compliance audit

### X.1 Required documentation corrections

- Replace the sampled frustum/tube architecture rather than appending a competing implementation.
- Replace proxy-volume, eight-sample, ellipse-field, independent top/base, broad-envelope, and morphological-scatter contracts.
- Record several separate complete parallel beams as the current visual authority.
- Record the direct performance comparison between rejected and proposed approaches.
- Replace stale V1.0/V1.1 status text in parent Weather documentation.
- Preserve accepted controller, cloud, source, lifecycle, authored, and gameplay-zone decisions.

### X.2 Preserved behavior

- Cloud Shadow V0.4 remains frozen.
- The validated LightRay CPU cloud query and projection focus remain accepted.
- Weather wind, wind trails, Time of Day, Inspector organization, and receiver integration remain unchanged.
- At the time of that documentation patch, V1.1C code remained present and was documented as rejected. It has since been replaced by the later implementation ledgers; this sentence is historical evidence, not current status.

### X.3 Validation for this documentation patch

- exact three-file scope;
- Markdown heading and fence balance;
- UTF-8, NUL-byte, and final-newline checks;
- cross-reference consistency;
- stale current-architecture term scan;
- source-file hash comparison confirming no non-document file changed.

No Unity validation is required for this documentation-only patch.

> **Historical-ledger rule:** Sections Y through AG record the exact state, evidence, and decisions of earlier patches. They intentionally preserve obsolete counts, taps, controls, and statuses as failure history. They are not alternate current contracts. Sections A through V and the latest AH ledger govern the current implementation.

## Y. `WEATHER-LIGHT-RAY-V1.0` implementation ledger

**Status:** source patch prepared; local scope and structural audits passed; Unity compilation and Scene-view projection validation remain pending.

### Y.1 Objective

Create the nonvisual LightRay foundation without adding a Renderer Feature, visible shaft, scene attachment, authored anchor, procedural spawning, gameplay effect, layer, tag, or serialized renderer-asset edit.

The patch must:

- establish source-neutral public enums, handles, source state, cloud samples, and snapshots;
- establish one central fixed-capacity Weather LightRay controller;
- bind the current authoritative Sun through an explicit override or `RenderSettings.sun`;
- reserve Moon in the type system while reporting it unavailable;
- add a source-neutral CPU cloud-transmission query to the frozen cloud controller;
- sample the exact retained readable cookie rather than regenerating cloud noise or reading back the GPU;
- expose evolution stability separately from sampled transmission;
- add a controlled world-point projection diagnostic that can be overlaid on the existing shader-sampled cloud debug map;
- preserve all existing cloud generation, movement, cookie assignment, receiver behavior, wind behavior, Time-of-Day behavior, and benchmark behavior.

### Y.2 Approved file scope

Create:

```text
Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs
Assets/Game/Procedural/Weather/WeatherLightRaySourceProfile.cs
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
```

Modify:

```text
Assets/AGENTS.md
Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Docs/Weather_System_Architecture_Provisional.md
Assets/Docs/Weather_Cloud_Shadow_Handoff.md
```

No scene, prefab, material, renderer asset, RP asset, shader, HLSL, Time-of-Day source, Weather wind source, receiver, gameplay, Ground, Generated Mass, Vegetation, or River file is approved.

### Y.3 Read-only evidence reviewed before runtime edits

| File and symbols | Finding | Constraint |
|---|---|---|
| `Assets/AGENTS.md` — mandatory workflow and Unity constraints | The user explicitly removed the former Git requirement. Archive-only review, persistent planning, scope control, and post-change audit remain mandatory. | Do not use Git. Record the archive baseline and all unavailable Unity validation honestly. |
| `WeatherCloudShadowController.cs` — `TickController`, `EnsureCookie`, `UpdateCookieEvolution`, `ApplyCookieToCapturedSun`, `RestoreCapturedSunState` | One readable repeated `R8` cookie, world phase, source-local projected offset, and evolution state already exist. | Add queries only. Do not alter generation, movement, assignment, gates, or restoration. |
| `WeatherCloudShadowCookieGenerator.cs` — `GeneratePixels`, `CreateTexture`, `UploadPixels` | The texture stores retained direct-light transmission and remains CPU-readable with bilinear filtering and repeat wrap. | Query `GetPixelBilinear`; do not run a second noise evaluator or GPU readback. |
| `SH_WeatherCloudShadowDebugOverlay.shader` — `SampleMainLightCookie(input.positionWS)` | The existing overlay displays the exact active shader cookie at world positions. | Use controlled CPU markers over this overlay for projection comparison; do not modify the shader. |
| `WeatherCloudShadowControllerEditor.cs` | Existing cloud diagnostics and benchmark actions are centralized and do not need modification for the LightRay foundation. | Keep cloud Inspector behavior unchanged. |
| `WeatherCloudShadowBenchmark.cs` — controller state capture and restore calls | The benchmark depends only on existing controller fields and methods. | New query state must not enter or disturb benchmark capture/restoration. |
| `TimeOfDayController.cs` — `ApplySun`, `RenderSettings.sun` | One authoritative directional Sun is configured; there is no Moon source. | V1.0 binds Sun only and does not edit Time of Day. |
| `TimeOfDayProfile.cs` — Sun-only lighting state and zero night intensity | No Moon direction, colour, intensity, or availability exists. | Moon remains a reserved unavailable source kind. |
| `Weather_System_Architecture_Provisional.md`, `Weather_Cloud_Shadow_Handoff.md`, this document | Weather owns LightRay; cloud V0.4 is frozen; hybrid visual work begins only at V1.1. | V1.0 is nonvisual and cannot attach or edit `PC_Renderer`. |

### Y.4 Fixed contracts for this patch

- Cloud query input uses a directional `Light`, allowing future Sun or Moon ownership without hardcoding source kind.
- The CPU cookie plane uses the source light's local X/Y axes, the configured world period, the controller's world-space phase projected into those axes, and the captured original Sun cookie offset only when querying that same Sun.
- `CloudShadowsEnabled == false` returns an explicit clear-sky sample with transmission `1`.
- No published cloud controller is interpreted by the LightRay controller as clear sky, matching section L.2.
- A published enabled controller with failed or missing cookie generation returns an explicit unavailable/error result and is not silently treated as clear.
- Seed evolution returns the currently uploaded blended cookie transmission with an unstable status. The LightRay population layer will suspend cloud-respecting rays in V1.2; V1.0 only exposes the state.
- Storage capacity defaults to `16`: the provisional High visual budget is `12`, leaving four nonvisual slots for authored or gameplay rays. This is capacity, not a target active count.
- No runtime API creates active rays in V1.0. Handles and snapshots exist so later patches do not redesign data ownership.

### Y.5 File-by-file implementation sequence

1. Add source-neutral types and immutable public snapshots.
2. Add the source-profile asset contract without creating any profile asset.
3. Add the central controller, Sun resolver, Moon-unavailable state, fixed storage, nonallocating snapshot reads, cloud-query forwarding, report, and probe-grid contract.
4. Add the custom Inspector and Scene-view controlled-point markers.
5. Add the cloud controller's CPU projection and transmission query.
6. Update parent Weather and cloud handoff documents with the exact nonvisual result.
7. Run scope diff, full modified-file reread, syntax/static checks, stale-reference scans, and package the changed files only.

### Y.6 Acceptance criteria

- All approved files are the only changed files.
- Existing cloud cookie assignment and movement code are byte-identical outside query-only additions and report lines.
- The query returns `[0,1]` transmission from the current generated texture.
- Querying the active Sun uses an offset numerically equal to the installed cookie offset while the Sun gate is active.
- Evolution stability is reported without changing evolution timing or uploads.
- The LightRay controller creates no scene objects, materials, meshes, textures, renderer passes, colliders, or per-frame allocations after storage initialization.
- Active count remains zero because V1.0 contains no spawn or registration surface.
- Moon reports unavailable rather than inventing runtime state.
- Unity compilation and visual marker-versus-overlay validation are marked pending until applied to the live project.

### Y.7 Validation and compliance status

- [x] Exact archive-baseline scope diff passes: only the nine files listed in Y.2 differ.
- [x] Complete modified-file reread passes.
- [x] UTF-8, NUL-byte, final-newline, delimiter, comment/string-state, preprocessor-balance, and own-type declaration checks pass for every changed C# file.
- [x] No forbidden serialized asset, shader, HLSL, renderer, RP asset, layer, tag, scene, prefab, material, Time-of-Day, wind, receiver, gameplay, Ground, Generated Mass, Vegetation, or River change exists.
- [x] SHA-256 comparison confirms the cloud generator, cloud benchmark, cloud debug shader, Weather wind files, Time-of-Day files, `PC_Renderer.asset`, and `PC_RPAsset.asset` remain byte-identical to the supplied archive.
- [x] The cloud-controller diff is restricted to query/report additions and the `ShadedTransmission` accessor; generation, movement, evolution, assignment, gate, restoration, benchmark, and receiver code is unchanged.
- [ ] Unity 6000.5.0f1 C# compilation is pending application to the live project; no Unity executable or standalone C# compiler is present in the archive environment.
- [ ] Scene-view CPU-marker versus shader-overlay projection validation is pending application to the live project.

### Y.8 Intentional final differences from the supplied archive

- `Assets/AGENTS.md` removes only the obsolete Git-status/history requirement and explicitly discourages Git interaction unless requested.
- Four LightRay source files establish nonvisual types, source profile, central zero-population storage, source binding, reports, and the controlled Scene-view probe.
- `WeatherCloudShadowController.cs` adds a read-only CPU projection/transmission query and report evidence. It does not mutate cloud state when queried.
- The three Weather documents record the exact V1.0 source result and pending Unity evidence.
- No `.meta` file is fabricated. Unity must generate metadata for the four new source files when the patch is imported into the live project.

### Y.9 Remaining validation actions

1. Compile in Unity 6000.5.0f1 and require zero C# errors.
2. Add `WeatherLightRayController` to the existing Weather object through the Unity Editor; do not raw-edit the scene.
3. Enable the existing Cloud / Opening Map, select the LightRay controller, and confirm CPU probe classifications align with the shader overlay while cloud movement advances.
4. Copy the LightRay report and require the active-Sun query-offset delta to remain `0` while the Sun gate is active.
5. Trigger cloud seed evolution and confirm samples report `EvolutionUnstable` without changing cloud evolution behavior.
6. Disable Cloud Shadows and confirm probe samples report `ClearSky` with transmission `1`.

---

## Z. `WEATHER-LIGHT-RAY-V1.0B` projection-probe visibility correction

**Status:** Unity compilation and high-contrast marker visibility confirmed by user screenshots. Same-centre overlay comparison remained blocked because the probe independently followed the physical Main Camera; that placement defect is addressed by V1.0C.

### Z.1 Objective

Make the existing nonvisual CPU cloud-projection probe unambiguous in Scene view without changing LightRay runtime state, cloud sampling, cloud generation, cloud projection, or any rendered Weather effect.

The current CPU query is producing valid data, but the Scene diagnostic cannot be reliably seen against the cloud overlay. The correction must make every sample visible at ordinary Scene-view distances and must provide a direct action for framing the sampled region.

### Z.2 Approved file scope

Modify:

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
```

No runtime C#, shader, HLSL, scene, prefab, material, renderer asset, RP asset, layer, tag, cloud-controller, Time-of-Day, wind, River, Ground, Vegetation, Generated Mass, gameplay, or serialized asset change is approved.

### Z.3 Read-only evidence reviewed before implementation

| Evidence | Finding | Constraint |
|---|---|---|
| User Scene-view screenshot supplied after `WEATHER-INSPECTOR-CLEANUP-V1.0` | The cloud overlay is clearly visible, but no 5 × 5 probe marker grid or strong probe boundary is discernible. | The diagnostic presentation must change; the cloud query itself must not be altered based on this screenshot. |
| User foundation report — `25 / 0 / 0` usable/unstable/failed samples, transmissions from `0.051` through `1.000`, maximum installed-cookie offset delta `0` | CPU queries execute successfully and include both clouded and open samples. | Do not change CPU projection or transmission calculations in V1.0B. |
| `WeatherLightRayControllerEditor.cs` — complete current Inspector and `OnSceneGUI` | Markers use a world-space radius, lie on the sample plane, use the same cyan/magenta colours as the overlay, and use ordinary depth testing. The boundary is a thin white line. | Replace only editor drawing and editor instructions/actions. |
| `WeatherLightRayController.cs` — projection fields, `GetProjectionProbeWorldPosition`, `TryGetProjectionProbeSample` | The controller already provides every required sample position and result. `projectionProbeMarkerRadiusMetres` is diagnostic-only state consumed by the Editor. | No runtime/controller edit is required. Preserve the existing serialized field. |
| `WeatherCloudTransmissionSample` — status, transmission, stability, cloud-field flag | The Editor can distinguish clear sky, stable cloud samples, unstable evolution, unavailable/error state, and numeric transmission. | Use existing state only; do not add a new query contract. |
| `WeatherCloudShadowControllerEditor.cs` — Debug Visualization controls | The comparison overlay remains `Debug View -> Cloud + Sun Openings`, with cyan openings and magenta cloud by default. | Probe markers must use contrasting colours rather than duplicating overlay colours. |
| `WeatherInspectorGui.cs` | The cleaned Inspector uses collapsed foldouts and explicit controls. | Keep the cleanup conventions and all foldouts collapsed by default. |

### Z.4 Invariants and non-goals

- The cloud transmission values, UVs, offsets, source binding, probe world positions, report output, and sample count remain unchanged.
- V1.0B does not create or render LightRays.
- The existing serialized marker-radius field remains present to avoid serialized migration; its Inspector label becomes a screen-relative marker scale because the Scene diagnostic will no longer use metres.
- Markers are Editor-only and must remain visible through scene geometry.
- The probe boundary must remain Editor-only and must not create scene objects or serialized state.
- Transmission labels are optional Editor-instance state and default off.
- No new runtime allocation, update, or rendering path is introduced.

### Z.5 File-by-file implementation sequence

1. Update this ledger with the approved evidence, scope, invariants, acceptance criteria, and validation plan.
2. Update `WeatherLightRayControllerEditor.cs`:
   - retain the existing probe controls and sample API;
   - relabel the marker-radius field as a screen-relative marker scale;
   - add a nonserialized `Show Transmission Labels` Editor toggle, default off;
   - add `Frame Projection Probe in Scene View`;
   - draw camera-facing, constant-screen-size markers using `HandleUtility.GetHandleSize`;
   - draw markers and boundary with `CompareFunction.Always` and restore prior handle state;
   - use dark outer, white middle, and high-contrast classified inner discs;
   - use green for open/clear samples, orange for clouded samples, yellow for unstable evolution, and red for failed/unavailable queries;
   - draw a strong yellow sampled-region boundary;
   - optionally label each sample with numeric transmission.
3. Reread the final Editor file and affected runtime contracts, compare final behavior with the pre-edit implementation, run available static checks, and record results here.

### Z.6 Acceptance criteria

- Exactly the two files in Z.2 differ from the post-cleanup LightRay baseline.
- Unity compilation has no errors.
- With the Weather object selected and the probe enabled, all 25 default markers remain clearly visible over cloud overlay, terrain, River, Vegetation, and other geometry at the tested Scene-view distance.
- Marker apparent size remains approximately stable while zooming the Scene view.
- Open/clear samples are green; clouded samples are orange; unstable samples are yellow; failed/unavailable samples are red.
- Every marker has a dark outer ring and white separating ring.
- The sampled area is enclosed by a strong yellow boundary visible through geometry.
- `Show Transmission Labels` defaults off and displays each sample's transmission when enabled.
- `Frame Projection Probe in Scene View` frames the current probe centre and span.
- CPU sample transmission, UV, offset, position, status, and report output remain unchanged.
- No scene object, material, shader, runtime controller, cloud setting, or serialized tuning value is changed by the patch.

### Z.7 Validation plan

1. Compile in Unity 6000.5.0f1 and require zero errors.
2. Enable `Cloud + Sun Openings` and the CPU probe; confirm the 5 × 5 marker grid and yellow boundary remain visible through the scene.
3. Zoom substantially in and out; confirm marker apparent size remains useful rather than shrinking with world distance.
4. Toggle transmission labels and confirm numeric values correspond to the copied foundation report.
5. Use `Frame Projection Probe in Scene View` and confirm the current probe region is framed.
6. Copy the foundation report and confirm sample values and maximum query-offset delta are unchanged.

### Z.8 Current status

- [x] User approved the diagnostic correction.
- [x] Complete current LightRay Inspector reviewed.
- [x] Direct projection-position, sampling, sample-status, cloud-overlay, and shared Inspector contracts reviewed.
- [x] Objective, scope, invariants, implementation sequence, risks, and acceptance criteria recorded before code edits.
- [x] Editor implementation complete.
- [x] Static consistency/compliance audit complete.
- [x] Unity compilation complete.
- [x] Scene-view marker and boundary visibility complete.
- [ ] Same-centre CPU-versus-overlay comparison complete; transferred to V1.0C because V1.0B used the physical Main Camera centre while the cloud overlay used the player focus.

### Z.9 Post-implementation consistency and compliance audit

- Exact diff against the accepted post-cleanup baseline contains only `Weather_Light_Ray_Architecture.md` and `WeatherLightRayControllerEditor.cs`.
- Complete final `WeatherLightRayControllerEditor.cs` and its projection-position, sample, sample-status, cloud-overlay, and shared Inspector contracts were reread.
- The final Editor still calls `TryGetProjectionProbeSample` with the same indices and consumes the same returned world position and sample; no CPU transmission, UV, offset, source, or status calculation changed.
- Marker drawing now uses `HandleUtility.GetHandleSize`, camera-facing discs, `CompareFunction.Always`, dark/white contrast rings, classified inner colours, a double-stroke yellow boundary, optional labels, and a Scene-view frame action.
- `Handles.color` and `Handles.zTest` are restored through `finally`.
- All Weather runtime files, cloud files, shaders, HLSL, scenes, materials, render assets, Time-of-Day files, wind files, gameplay files, and serialized assets are byte-identical to the accepted post-cleanup baseline.
- UTF-8, NUL-byte, final-newline, C# delimiter/comment/string/preprocessor balance, Markdown fence balance, required-symbol, duplicate-method, and stale-instruction scans passed.
- Unity compilation is unavailable in the archive environment and remains pending.
- Scene-view visibility and CPU-versus-overlay agreement remain pending live Unity validation.

### Z.10 Intentional final differences

- `Marker Radius (m)` is presented as `Marker Screen Scale`; the underlying serialized property is retained unchanged.
- Transmission labels are Editor-instance state, default off, and do not dirty or serialize the scene.
- Stable cloud-field samples are classified at the midpoint between configured shaded transmission and full transmission: samples nearer full transmission are green; samples nearer shaded transmission are orange. Numeric labels remain available for exact boundary inspection.
- Unstable samples are yellow and failed/unavailable samples are red.
- The sample region uses a black outer line plus yellow inner line and draws through geometry.


---

## AA. `WEATHER-LIGHT-RAY-V1.0C` projection-focus correction

**Status:** implemented and accepted from the user's centred probe screenshot; superseded as active work by V1.1.

### AA.1 Objective

Make the CPU cloud-projection probe use the same effective world-space centre as the published Cloud Shadow debug overlay by default. Preserve the explicit LightRay probe override as highest priority and preserve the existing assigned-camera, `Camera.main`, and controller-transform fallbacks when no published Cloud Shadow Controller exists.

This correction addresses diagnostic placement only. It does not change cloud transmission, cookie projection, sample UVs, source binding, LightRay storage, cloud rendering, or any future procedural LightRay population rule.

### AA.2 Approved file scope

Modify:

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
```

No shader, HLSL, scene, prefab, material, renderer asset, RP asset, Time-of-Day, wind, River, Ground, Vegetation, Generated Mass, gameplay, layer, tag, or serialized asset edit is approved.

### AA.3 Read-only evidence reviewed before implementation

| Evidence | Finding | Constraint |
|---|---|---|
| User V1.0B Scene screenshots | The 5 × 5 high-contrast markers and yellow boundary are now clearly visible. Marker classifications appear to correspond to nearby cyan opening and magenta cloud regions. The entire probe square is physically behind the gameplay area. | V1.0B visibility drawing is accepted and must remain unchanged. Correct only centre resolution. |
| User V1.0 foundation report | `Projection focus: Main Camera | source: AutomaticMainCamera`; probe centre was `(0.100, 0.000, -30.600)`. | The LightRay probe is using the physical isometric camera Transform position, which is offset behind the player. |
| Cloud Inspector shown in the validation screenshot | The Cloud Shadow debug overlay reports `Resolved Focus: Player_Blockout`, `Focus Source: Inspector Override`, and a different resolved world position. | The comparison systems currently use different centres. The probe must inherit the cloud overlay centre instead of independently resolving `Camera.main`. |
| `WeatherCloudShadowController.cs` — `ResolveDebugOverlayCentre`, `UpdateResolvedDebugFocus`, `ResolvedDebugFocus`, `ResolvedDebugFocusPosition` | The cloud overlay already resolves runtime override, Inspector override, assigned fallback camera, `Camera.main`, and controller fallback, then optionally uses a manual overlay anchor. The exact effective overlay centre is private. | Expose read-only effective overlay focus and centre contracts only. Do not change cloud focus ordering or overlay rendering. |
| `WeatherLightRayController.cs` — `ResolveProjectionFocus` | Current order is explicit probe override, assigned probe fallback camera, `Camera.main`, controller transform. It ignores the published Cloud Shadow Controller's resolved overlay centre. | Insert the published cloud overlay centre after the explicit probe override and before independent camera fallbacks. |
| `WeatherLightRayControllerEditor.cs` — projection controls, source formatting, report/status display, V1.0B Scene drawing | The editor still describes the fallback camera as the immediate fallback and has no source label for cloud-overlay inheritance. V1.0B marker rendering is isolated from focus resolution. | Update labels, help text, and source formatting only. Preserve all V1.0B drawing code byte-for-byte where practical. |
| Complete reference scan across supplied Weather files | `ResolvedDebugFocus*` is consumed only by the Cloud Inspector; `ResolvedProbe*` is consumed only by the LightRay Inspector/report/Scene diagnostic. | Adding read-only cloud properties and one new runtime-only probe-source enum member has no serialized migration surface. |

### AA.4 Invariants and non-goals

- Explicit `Probe Focus Override` remains the highest-priority probe centre.
- When a published Cloud Shadow Controller exists, the probe uses its exact effective debug-overlay centre, including resolved focus, manual overlay anchor, or cloud-controller fallback.
- The LightRay assigned fallback camera and `Camera.main` are used only when no published Cloud Shadow Controller exists.
- The cloud controller's existing focus priority, debug-follow switch, manual anchor behavior, and rendered overlay centre remain unchanged.
- Probe sample Y remains controlled by `projectionProbeSampleHeightMetres`.
- CPU sample positions change only by the corrected XZ centre. Sampling equations, transmission values for a given world point, UV projection, cookie offsets, statuses, and reports remain otherwise unchanged.
- V1.0C does not create or render LightRays and does not define future procedural population anchoring.
- No serialized field is added, removed, renamed, or retuned.
- V1.0B marker appearance, through-geometry behavior, labels, boundary, and framing implementation remain unchanged except that framing uses the corrected centre automatically.

### AA.5 File-by-file implementation sequence

1. Record this objective, evidence, scope, invariants, acceptance criteria, and validation plan before runtime edits.
2. Add read-only `EffectiveDebugOverlayFocus` and `EffectiveDebugOverlayCentre` properties to `WeatherCloudShadowController`, derived from the exact existing overlay-centre logic.
3. Add a runtime-only `CloudDebugOverlay` probe-focus source and update `WeatherLightRayController.ResolveProjectionFocus` to use the published cloud controller after the explicit LightRay override.
4. Update the LightRay Inspector's fallback tooltip, read-only explanation, and focus-source formatting to state the exact priority and identify cloud-overlay inheritance.
5. Reread all modified files and affected consumers, compare the final diff against this scope, run available static checks, and record results below.

### AA.6 Acceptance criteria

- Exactly the four files listed in AA.2 differ from the accepted V1.0B plus Inspector-cleanup baseline.
- Unity compilation has zero errors.
- With no explicit LightRay probe override and a published Cloud Shadow Controller, the LightRay report shows `CloudDebugOverlay` as the focus source.
- The LightRay `Resolved Focus` names the cloud overlay's effective focus or anchor, and the probe centre XZ matches the Cloud Shadow Controller's effective overlay centre XZ.
- When the cloud overlay follows `Player_Blockout`, the probe square is centred over the same gameplay area rather than the physical isometric camera position.
- Explicit LightRay probe override still takes priority.
- Removing or disabling the published Cloud Shadow Controller restores the existing assigned-camera, `Camera.main`, and controller-transform fallback order.
- `Frame Projection Probe in Scene View` frames the corrected centre without code changes to its drawing implementation.
- V1.0B marker visibility and green/orange classification remain unchanged.
- Cloud overlay movement, focus selection, rendering, and serialized settings remain unchanged.

### AA.7 Validation plan

1. Compile in Unity 6000.5.0f1 and require zero errors.
2. Leave `Probe Focus Override` empty, keep the cloud overlay focused on the player, and confirm the probe square and overlay share the same gameplay-area centre.
3. Copy the LightRay report and confirm the focus source is `CloudDebugOverlay` and the centre matches the Cloud Shadow effective overlay centre in XZ.
4. Assign a temporary explicit Probe Focus Override and confirm it takes priority; clear it and confirm cloud-overlay inheritance returns.
5. Disable the Cloud Shadow Controller and confirm the probe falls back to its assigned camera or `Camera.main`.
6. Reconfirm green markers align with cyan openings and orange markers align with magenta cloud while the cloud field moves.

### AA.8 Current status

- [x] User approved the focus correction.
- [x] Complete current LightRay controller, LightRay Inspector, cloud focus/overlay implementation, affected Inspector consumers, and canonical plan reviewed.
- [x] Objective, evidence, approved scope, invariants, implementation sequence, acceptance criteria, and validation plan recorded before code edits.
- [x] Runtime implementation complete.
- [x] Editor wording and source formatting complete.
- [x] Static consistency/compliance audit complete.
- [ ] Unity compilation complete.
- [ ] Live centre, override-priority, fallback, and moving-overlay validation complete.

### AA.9 Post-implementation consistency and compliance audit

- Exact diff against the accepted V1.0B plus Weather Inspector cleanup baseline contains only the four files approved in AA.2.
- Complete final versions of `WeatherCloudShadowController.cs`, `WeatherLightRayController.cs`, `WeatherLightRayControllerEditor.cs`, and this canonical plan were reread together with the Cloud Inspector consumers of the existing resolved-focus properties.
- `WeatherCloudShadowController` adds only two read-only properties. `EffectiveDebugOverlayCentre` calls the exact private method already used by `RenderDebugOverlay`; `EffectiveDebugOverlayFocus` mirrors the same follow-focus, manual-anchor, and controller-fallback branches without changing them.
- `WeatherLightRayController.ResolveProjectionFocus` preserves the explicit LightRay override as first priority, then consumes the published cloud overlay's focus and centre, then preserves the previous assigned-camera, `Camera.main`, and controller fallbacks.
- `resolvedProbeCentre.y` remains overwritten by `projectionProbeSampleHeightMetres`; only the default XZ centre source changes.
- The V1.0B Scene marker drawing, classification, depth mode, boundary, labels, and framing code are unchanged. Framing already reads `ResolvedProbeCentre` and therefore follows the corrected centre without a separate code path.
- No serialized field, default, scene object, shader, HLSL, material, renderer asset, RP asset, Time-of-Day file, wind file, gameplay file, layer, or tag changed.
- Changed-file scope, UTF-8, NUL-byte, final-newline, C# delimiter/comment/string/preprocessor balance, Markdown fence balance, required-symbol, focus-priority, and no-new-serialized-field checks passed.
- No Unity executable or standalone C# compiler is available in the supplied-file environment. Unity compilation and live behavior remain pending and are not represented as passed.

### AA.10 Intentional final differences

- The Cloud Shadow Controller now publishes the Transform and world centre that its debug overlay effectively uses. These contracts are read-only and do not alter cloud behavior.
- The LightRay diagnostic now reports `CloudDebugOverlay` when it inherits that centre.
- `Probe Fallback Camera` is now accurately documented as a fallback used only when no explicit probe override and no published Cloud Shadow Controller exist.
- LightRay Live Status now displays the resolved probe-focus source in addition to the focus object.


---

## AB. `WEATHER-LIGHT-RAY-DOC-V1.1D-CONTINUOUS-BEAMS` documentation ledger

### AB.1 Unity evidence that invalidated the sampled renderer

The accepted screenshots demonstrated:

- isolated oval and circular atmospheric marks;
- capsule-like clusters resembling a camera flare;
- occasional bars only where adjacent camera-depth samples happened to overlap;
- Raw Strand Field and Post-Scatter Strand views that remained structurally almost identical;
- a Surface Illumination view that remained black;
- a broad white envelope/searchlight result during some states;
- no resemblance to complete long uninterrupted beams.

### AB.2 Code-driven cause

The rejected atmospheric mask used:

```text
8 camera-ray samples per covered pixel
× up to 8 elliptical strand evaluations per sample
```

The shader therefore visualized isolated cross-sections at discrete depths instead of directly rendering complete beams. Independent top/base strand centres, random cuts, anisotropic ellipse masks, and post-process maximum extension reinforced the clustered lens-flare appearance.

The surface path required valid reconstructed depth and also depended on broad proxy coverage. The persistent black debug output showed that this path was not delivering usable visible-surface influence.

### AB.3 Approved replacement contract

- one LightRay zone owns `3–5` separate beams for the proof;
- all beam axes are straight and parallel;
- each beam is one continuous camera-facing ribbon from upper fade to ground-contact fade;
- all ribbons are combined into one mesh and one mask draw;
- no independently randomized top/base centres;
- no per-beam random length cuts that create broken rays;
- no sampled elliptical cross-sections;
- no visible broad envelope;
- surface influence is calculated independently from atmosphere using point-to-segment distance;
- screen-space softening is short and cannot merge beam gaps;
- the final result remains hybrid and depth-aware.

### AB.4 Documentation replacement rule

The sampled-volume implementation details formerly stored in the V1.1, V1.1A/B, and V1.1C ledgers have been removed from the canonical document. They are not alternative approved choices.

The only retained historical statement is that V1.1C code currently exists and is rejected. Future implementation must follow sections N, O, R, S, T, U, V, and this ledger.

### AB.5 Current authorization state

The documentation correction was accepted on 2026-07-25. Source implementation is authorized only through the active `WEATHER-LIGHT-RAY-V1.1D-A` ledger below.

### AB.6 Documentation-patch status

Prepared from the current supplied V1.1C baseline. No runtime file is intentionally changed.

---

## AC. `WEATHER-LIGHT-RAY-V1.1D-A` continuous atmospheric beam implementation ledger

### AC.1 Objective

Replace the rejected sampled frustum/tube renderer with one retained combined mesh containing three to five separate continuous camera-facing beam ribbons. Preserve the accepted controller, source, lifecycle, authored registration, cloud-query, camera-filtering, and Render Graph foundations.

### AC.2 Acceptance criteria

- One authored LightRay produces `3–5` complete parallel beams in one mesh draw.
- Each beam uses the same world-space axis from ground contact to upper fade; top and bottom centres are not randomized independently.
- Beam width, spacing, edge softness, upper fade, ground fade, intensity variation, and subtle coherent evolution are continuous UV-driven controls.
- No ellipse sampling, camera-ray depth slicing, random length cuts, capsule masks, broad envelope haze, or source-aligned maximum-extension scatter remains active.
- The quarter-resolution softening pass is limited to one screen-space texel across beam width and cannot intentionally join separated beams.
- Surface illumination remains explicitly pending `V1.1D-B`; the normal renderer does not claim it is implemented.
- The supplied authored proof remains limited to one active authored zone. Procedural spawning remains blocked.

### AC.3 Approved file scope

Runtime and editor files authorized for modification:

```text
Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs
Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs
Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayMask.shader
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayScatter.shader
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayComposite.shader
Assets/Docs/Weather_Light_Ray_Architecture.md
```

Reviewed but intentionally unchanged unless implementation evidence invalidates the plan:

```text
Assets/Game/Procedural/Weather/WeatherLightRaySourceProfile.cs
Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs
Assets/Game/Rendering/Weather/WeatherLightRayRendererFeature.cs
Assets/Settings/PC_Renderer.asset
Assets/Settings/PC_RPAsset.asset
Assets/Docs/Weather_System_Architecture_Provisional.md
Assets/Docs/Weather_Cloud_Shadow_Handoff.md
Assets/Docs/Weather_Inspector_Cleanup_Plan.md
```

### AC.4 Review evidence

The complete approved review surface was read from the user-supplied archive before this ledger was written. Confirmed evidence:

- `SH_WeatherLightRayMask.shader` performed eight camera-ray samples and evaluated elliptical strand cross-sections instead of rasterizing complete beams.
- `WeatherLightRayCommon.hlsl` generated independent top/base positions, random length cuts, taper, clustering, and broad-envelope density.
- `WeatherLightRayRenderPass.cs` rendered one 12-sided frustum proxy and uploaded the rejected strand/envelope contract.
- `SH_WeatherLightRayScatter.shader` used seven source-aligned maximum-extension taps that could stretch sampled marks but could not restore missing continuous geometry.
- `WeatherLightRayTypes.cs`, `WeatherLightRayAnchor.cs`, both custom Inspectors, and the controller report still exposed the rejected vocabulary.
- `WeatherLightRayRendererFeature.cs`, the controller storage/lifecycle paths, and the validated cloud query do not depend on the sampled geometry and remain reusable.

### AC.5 Fixed defaults and migration rules

The rejected tuning values are not authoritative and are not migrated when their meaning changes. Approved continuous-beam defaults:

| Control | Default | Contract |
|---|---:|---|
| Beam Count | `5` | Clamped to `3–5` for the authored proof. |
| Beam Width Range | `0.45–0.75 m` | World-space full ribbon width. |
| Beam Spacing | `1.05 m` | World-space centre-to-centre separation. |
| Beam Intensity Variation | `0.18` | Stable per-beam variation; never zeroes a beam. |
| Beam Edge Softness | `0.55` | Continuous across-width feathering. |
| Upper Fade | `0.10` | Fraction of beam length. |
| Ground Fade | `0.12` | Fraction of beam length. |
| Atmospheric Intensity | `0.28` | Retains the previous authored proof brightness baseline. |
| Screen Softening | `0.35` | One-texel cross-width expansion blend. |

One-to-one serialized concepts may use `FormerlySerializedAs`, including Strand Count → Beam Count, Shaft Intensity → Atmospheric Intensity, and Per-Strand Phase Variation → Per-Beam Phase Variation. Width, spread, envelope, random-length, taper, and cluster fields are deliberately replaced rather than semantically mis-migrated.

### AC.6 Invariants and non-goals

Preserve:

- stable handles and fixed-capacity central storage;
- source-neutral Sun/Moon descriptors and source gates;
- Timed, Permanent, and Externally Controlled lifecycle behavior;
- authored anchor registration and one-active-authored-zone proof limit;
- cloud-respecting/cloud-ignoring policy and validated CPU projection query;
- designated Base Game camera filtering;
- quarter-resolution mask, quarter-resolution softening texture, and full-resolution composite ownership through Render Graph.

Not part of this patch:

- procedural placement or population;
- Moon-light resolution;
- quarter-resolution point-to-segment surface illumination;
- cloud compensation on visible surfaces;
- final visual calibration, quality tiers, GPU timing, or proof freeze;
- scene, prefab, renderer-asset, layer, tag, or component changes.

### AC.7 File-by-file sequence

1. Replace the descriptor and authored-anchor controls with the continuous-beam contract.
2. Update controller diagnostics and both custom Inspectors to remove rejected vocabulary and mark surface illumination pending.
3. Replace the 12-sided frustum with a retained exact-count combined ribbon mesh; rebuild it only when beam count changes.
4. Replace the common shader include and mask shader with direct UV-profile ribbon rasterization and depth occlusion.
5. Replace the seven-tap source-aligned scatter with a one-texel cross-width edge softener.
6. Simplify the composite to consume continuous atmosphere only while preserving debug enum numeric values.
7. Run compile-oriented static checks, stale-symbol scans, shader delimiter/include checks, final-scope diff audit, and canonical-document consistency review.

### AC.8 Risks and mitigations

| Risk | Mitigation |
|---|---|
| Stable world-space beam offsets project with reduced separation from some camera/source alignments. | Use a deterministic axis perpendicular to the celestial direction and validate the intended isometric camera plus practical extremes before changing architecture. |
| Billboard width axis degenerates when the camera view aligns with the beam axis. | Fall back to the stable perpendicular basis axis. |
| Camera enters a ribbon and produces a large wash. | Apply per-beam camera-to-segment proximity fade; do not restore a broad envelope. |
| Quarter-resolution filtering reconnects gaps. | Use only immediate cross-width neighbours and bounded expansion; never blur along beam direction. |
| Existing rejected serialized values produce misleading controls. | Do not migrate semantically incompatible width/spread/envelope fields; use explicit new world-space defaults. |
| Surface debug remains black during this patch. | Label it pending `V1.1D-B`; do not present it as implemented or use it in the final composite. |

### AC.9 Validation plan

Source validation available in the archive workspace:

- C# stale-symbol and constructor/caller scan;
- balanced delimiter and preprocessor scan for changed shaders/includes;
- search proving sampled-frustum, ellipse, random-length, envelope-haze, and seven-tap maximum-extension code is removed from active runtime files;
- cross-subsystem include-impact audit;
- final changed-file scope audit.

Unity validation required after patch application:

1. Compile the project with no C# or shader errors.
2. Inspect Raw Continuous Beams and confirm three to five complete separated shafts.
3. Inspect Softened Continuous Beams and confirm only restrained edge feathering with preserved gaps.
4. Inspect Final Composite in Scene and Game cameras and confirm no circles, capsules, broad white envelope, disconnected bars, or camera-intersection wash.

### AC.10 Implementation result

The source implementation follows the approved sequence and changes exactly the eleven files listed in AC.3:

- `WeatherLightRayDescriptor` and `WeatherLightRayAnchor` now use an explicit continuous-beam contract: `3–5` beams, world-space width range and spacing, bounded intensity variation, UV edge softness, independent upper/ground fades, atmosphere strength, one-texel softening, and coherent evolution.
- Meaningful one-to-one serialized migrations are retained for Strand Count, Shaft Intensity, Scatter Softness, and Per-Strand Phase Variation. Semantically incompatible sampled-volume width, spread, envelope, random-length, taper, and cluster data are deliberately not reinterpreted.
- Controller reports and both custom Inspectors use continuous-beam terminology. Surface controls and the Surface Illumination debug view are explicitly marked pending `V1.1D-B`.
- `WeatherLightRayRenderPass` owns one retained exact-count mesh with four vertices and six indices per beam. It rebuilds only when the clamped `3–5` beam count changes and submits the complete bundle in one draw.
- The mask shader directly rasterizes complete camera-facing world-space ribbons. Every beam uses one shared base/top axis, stable seeded width/intensity, continuous across-width and longitudinal profiles, depth occlusion, and camera-axis proximity fading.
- The former scatter shader identity is retained for renderer-feature/material compatibility, but its active pass is now a bounded centre/left/right cross-width softener with depth discontinuity rejection. It contains no source-aligned maximum-extension loop.
- The composite depth-aware upsamples raw and softened R16 masks, exposes Raw Continuous Beams and Softened Continuous Beams diagnostics, leaves Surface Illumination black by design, and applies only the warm atmospheric contribution in Final Composite.

### AC.11 Intentional final differences and preserved foundations

Intentional differences from the rejected V1.1C source:

- old debug enum numeric values remain stable while labels change from Raw Strand Field/Post-Scatter Strands to Raw Continuous Beams/Softened Continuous Beams;
- the shader asset named `SH_WeatherLightRayScatter.shader` remains in place so `WeatherLightRayRendererFeature` and existing material references do not require asset or renderer changes, but it no longer implements directional scattering;
- surface descriptor values remain serialized for the shared authored/procedural contract but are not uploaded or consumed by the V1.1D-A atmosphere composite;
- the one-active-authored-zone proof limit remains unchanged.

Reviewed foundations confirmed byte-identical to the supplied baseline:

```text
Assets/Game/Procedural/Weather/WeatherLightRaySourceProfile.cs
Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs
Assets/Game/Rendering/Weather/WeatherLightRayRendererFeature.cs
Assets/Settings/PC_Renderer.asset
Assets/Settings/PC_RPAsset.asset
Assets/Docs/Weather_System_Architecture_Provisional.md
Assets/Docs/Weather_Cloud_Shadow_Handoff.md
Assets/Docs/Weather_Inspector_Cleanup_Plan.md
```

No scene, prefab, material, renderer asset, Render Pipeline asset, layer, tag, component, cloud-query, Time-of-Day, wind, or gameplay file changed.

### AC.12 Static validation and compliance evidence

The final supplied-file workspace passed the following checks after implementation:

- exact final diff contains the eleven approved files and no others;
- all changed files are UTF-8, contain no NUL bytes, end with a newline, and pass balanced C#/HLSL/Shader delimiter and preprocessor checks;
- the immutable descriptor declares `37` fields, its constructor accepts `37` arguments, and the authored anchor supplies `37` arguments in the same contract;
- all `37` serialized authored-anchor controls are resolved by the custom Inspector and the Inspector references no missing field;
- debug enum serialized values remain `0`, `1`, `2`, and `4`;
- mesh construction remains exactly four vertices/six indices per beam and clamps the proof to `3–5` beams;
- only the Mask, Scatter/Softening, and Composite shaders include `WeatherLightRayCommon.hlsl`;
- active runtime source contains no sampled-frustum intersection, elliptical strand evaluation, random strand-length/position, broad-envelope haze, or seven-tap directional-extension implementation;
- the softening shader performs exactly three mask reads: centre, immediate left, and immediate right;
- all eleven changed files and the eight preserved foundation files were reread after implementation against the supplied baseline.

No Unity executable, standalone C# compiler, or shader compiler is available in the supplied-file environment. Static checks do not substitute for Unity compilation or visual validation.

### AC.13 Pending Unity validation and current blocker

`SOURCE IMPLEMENTATION PRESENT — UNITY VALIDATION PENDING`.

The current blocker is evidence, not an identified source inconsistency. The patch must remain unfrozen until Unity 6000.5.0f1 confirms:

1. zero C# and shader compilation errors;
2. Raw Continuous Beams shows three to five complete separated parallel shafts;
3. Softened Continuous Beams feathers only the immediate cross-width edges and preserves gaps;
4. Final Composite has no circles, capsules, disconnected bars, broad white envelope, or camera-intersection wash;
5. Surface Illumination remains intentionally black and clearly labelled pending `V1.1D-B`.

---

## AD. `WEATHER-LIGHT-RAY-V1.1D-AB` camera-readable atmosphere and surface-contact proof ledger

### AD.1 Objective

Correct the continuous-ribbon proof exposed by the first Unity captures without returning to sampled tubes or broad envelope haze. The patch must make the complete bundle camera-readable, replace literal white-quad presentation with bounded atmospheric density, provide meaningful gap-preserving softening, and implement the first quarter-resolution depth-reconstructed surface illumination and ground-contact diagnostic.

### AD.2 Unity evidence requiring this patch

The user-supplied 2560 × 1440 captures on 2026-07-25 establish:

- Raw Continuous Beams and Softened Continuous Beams are almost indistinguishable.
- Each ribbon faces the camera, but beam centres are distributed along a different stable world axis; projected beam spacing compresses and the ribbons overlap.
- The atmospheric profile contains broad flat white interiors and hard rectangular lower ends.
- Beam width, opacity, fade, and perceived length variation are too similar to read as separate natural shafts.
- The final composite contains no visible surface illumination because V1.1D-A deliberately left that path black.
- The selected authored anchor cannot prove contact alignment in Game view because scene gizmos are not rendered by the gameplay camera.

The continuous-ribbon architecture remains accepted as the source geometry. The current atmosphere presentation and missing surface proof are rejected.

### AD.3 Acceptance criteria

- One shared camera-readable world-space cross-axis controls both beam-centre spacing and ribbon width orientation.
- The bundle retains `3–5` complete parallel axes and visible projected gaps at the intended isometric camera.
- Across-width density has no flat opaque plateau; every beam uses a bounded centre-to-edge falloff with a restrained core and broader faint halo.
- Mask overlap uses bounded opacity union rather than unbounded additive accumulation.
- Per-beam seeded width, intensity, upper fade, ground fade, and longitudinal density differ visibly while each beam remains uninterrupted.
- Foreground depth intersection fades continuously instead of using a binary discard.
- The quarter-resolution softening pass uses a normalized five-tap cross-width kernel with depth rejection and does not intentionally join separated beams.
- Quarter-resolution surface influence reconstructs visible world positions from camera depth and evaluates all active beam segments using bounded point-to-segment falloff.
- Ground-facing surfaces use `Ground Light Intensity`; other opaque visible surfaces use `Surface Light Intensity`; cloud compensation is applied only to cloud-ignoring rays.
- `Surface Illumination` displays non-black false-colour influence plus explicit beam-base and bundle-centre contact markers.
- Final Composite applies atmospheric and surface contributions independently so subtle shafts can coexist with readable ground/object light.
- Cloud projection, controller lifecycle, source gates, authored registration, renderer feature, scenes, prefabs, renderer assets, layers, tags, and components remain unchanged.

### AD.4 Approved file scope

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs
Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayMask.shader
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayScatter.shader
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayComposite.shader
```

Reviewed and intentionally unchanged:

```text
Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs
Assets/Game/Procedural/Weather/WeatherLightRaySourceProfile.cs
Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs
Assets/Game/Rendering/Weather/WeatherLightRayRendererFeature.cs
Assets/Settings/PC_Renderer.asset
Assets/Settings/PC_RPAsset.asset
Assets/Docs/Weather_System_Architecture_Provisional.md
Assets/Docs/Weather_Cloud_Shadow_Handoff.md
Assets/Docs/Weather_Inspector_Cleanup_Plan.md
```

### AD.5 Reviewed implementation evidence

The complete current V1.1D-A implementation and direct consumers were reread before this ledger was written.

- `SH_WeatherLightRayMask.shader`: beam centres use `bundleAxis` from `WeatherLightRayBuildStableBasis`, while each ribbon width uses an independently calculated camera-facing `widthAxis`; the mismatch explains projected compression. The width profile is constant until `1 - BeamEdgeSoftness`, depth occlusion uses `discard`, and the pass uses `Blend One One`.
- `SH_WeatherLightRayScatter.shader`: centre/left/right maximum expansion performs only three reads and does not produce a materially distinct softened result.
- `SH_WeatherLightRayComposite.shader`: Surface Illumination returns black and Final Composite consumes atmosphere only.
- `WeatherLightRayRenderPass.cs`: only mask and softened R16F quarter-resolution resources exist; no surface texture or surface-evaluation pass exists.
- `WeatherLightRayAnchor.cs` and both Inspectors: the six surface controls already serialize the required V1.1D-B contract but remain labelled pending.
- `WeatherLightRayTypes.cs`: the immutable descriptor already carries `GroundLightMultiplier`, `VisibleSurfaceLightMultiplier`, `CloudCompensationMultiplier`, `FootprintEdgeSoftness`, `FootprintIrregularity`, and `CoreEmphasis`; no descriptor expansion is required.
- `WeatherLightRayRendererFeature.cs`: the existing three materials and one render pass can support an additional material pass and Render Graph texture without an asset or feature change.

### AD.6 Fixed implementation decisions and defaults

No serialized default is changed in this patch. Existing authored values remain authoritative:

| Control | Existing default | Active use in V1.1D-AB |
|---|---:|---|
| Beam Count | `5` | Exact active beam count, clamped `3–5`. |
| Beam Width Range | `0.45–0.75 m` | Full atmospheric ribbon width and basis for surface radius. |
| Beam Spacing | `1.05 m` | Centre-to-centre separation along the shared camera-readable cross-axis. |
| Beam Intensity Variation | `0.18` | Stable bounded per-beam atmosphere and surface variation. |
| Beam Edge Softness | `0.55` | Core-to-halo density shape; no flat plateau. |
| Upper / Ground Fade | `0.10 / 0.12` | Base fade lengths with stable bounded per-beam variation. |
| Atmospheric Intensity | `0.28` | Composite-only atmospheric strength. |
| Screen Softening | `0.35` | Blend toward the normalized five-tap cross-width result. |
| Ground / Surface Light | `0.42 / 0.28` | Independent depth-reconstructed receiver lift. |
| Cloud Compensation | `0.45` | Additional bounded lift only for `IgnoreClouds`. |
| Footprint Edge Softness | `0.42` | Point-to-segment radial transition. |
| Footprint Irregularity | `0.20` | Low-amplitude continuous radius modulation; never holes. |
| Core Emphasis | `0.20` | Lower-segment/contact emphasis. |

The shared bundle axis is calculated once per camera and ray:

```text
upwardAxis = -rayDirection
bundleAxis = normalize(cross(cameraForward, upwardAxis))
```

When degenerate, a stable perpendicular fallback is used. The same `bundleAxis` is uploaded to atmosphere, surface, diagnostics, and softening projection.

Atmospheric accumulation uses bounded opacity union:

```text
out = source + destination × (1 - source)
```

Surface lobes use the same bounded union so overlap does not linearly explode.

### AD.7 File-by-file sequence

1. Upload the shared camera-readable bundle axis and existing surface descriptor parameters from `WeatherLightRayRenderPass`.
2. Add one quarter-resolution R16F surface texture and a depth-reconstructed surface raster pass using the existing Composite material’s second pass.
3. Centralize stable per-beam width, intensity, phase, fade variation, base position, and point-to-segment helpers in `WeatherLightRayCommon.hlsl`.
4. Make the mask use the shared axis, continuous core/halo density, bounded opacity-union blending, stronger non-breaking longitudinal variation, and soft foreground depth fading.
5. Replace three-tap maximum expansion with a normalized five-tap depth-aware cross-width filter.
6. Add surface-mask upsampling, false-colour footprint/contact diagnostics, and independent atmosphere/surface final contributions in the Composite shader.
7. Remove V1.1D-B-pending labels from the authored and controller Inspectors/reports and identify the active patch as V1.1D-AB.
8. Run stale-symbol, uniform-contract, shader delimiter/preprocessor, descriptor/Inspector, Render Graph resource-use, cross-subsystem include-impact, and exact-scope audits.

### AD.8 Risks and mitigations

| Risk | Mitigation |
|---|---|
| `cameraForward` nearly aligns with the beam axis. | Use a stable perpendicular fallback and normalize once on CPU. |
| Five-tap softening closes narrow projected gaps. | Filter only along the uploaded bundle cross-axis, use a normalized kernel, retain depth rejection, and blend by existing Softening Strength. |
| Surface pass lights geometry far above or below the intended zone. | Use distance to the finite base-to-top segment, not an infinite line. |
| Surface overlap becomes overbright. | Combine beam lobes with bounded opacity union and clamp the final scalar lift. |
| Cloud-ignoring compensation overexposes the scene. | Apply a bounded fraction of the existing compensation control only when the descriptor policy is `IgnoreClouds`. |
| Transparent receivers lack their own camera depth. | Record this existing approximation; validate opaque Ground/player/rocks first. Receiver-specific transparent support remains outside this patch. |
| Full-screen quarter-resolution surface evaluation wastes work. | Reject pixels outside a conservative projected bundle bound in shader; profile exact GPU cost before freeze. |

### AD.9 Validation plan

Static checks available in the supplied-file workspace:

- exact changed-file scope and final diff audit;
- C# member/caller and shader uniform upload/consumer scan;
- balanced C#/HLSL/Shader delimiter and preprocessor scan;
- search proving no sampled tube, ellipse slice, random hard length cut, broad envelope, binary atmosphere discard, or unbounded atmosphere blend remains active;
- verification that only the three LightRay shaders consume the shared include;
- verification that surface controls remain serialized and constructor alignment is unchanged.

Required Unity 6000.5.0f1 evidence:

1. zero C# and shader errors;
2. Raw Continuous Beams shows projected separation and visible per-beam variation;
3. Softened Continuous Beams is visibly softer while gaps remain open;
4. Surface Illumination shows green/cyan receiver influence, red individual beam-base markers, and a blue bundle-centre marker at the authored contact zone;
5. Final Composite shows restrained warm atmosphere plus readable Ground and opaque-object illumination;
6. no Render Graph warnings, invalid depth wash, or large allocation appears.

### AD.10 Status before source edits

This ledger was recorded as `PLAN RECORDED — IMPLEMENTATION PENDING` before any runtime source was changed.

### AD.11 Implementation result

`SOURCE IMPLEMENTED — UNITY VALIDATION PENDING`.

The supplied-file implementation now:

- uploads one camera-readable bundle cross-axis and uses it for beam spacing, ribbon width, softening direction, surface evaluation, and diagnostics;
- replaces flat ribbon interiors with continuous centre-to-edge density and per-beam fade variation;
- replaces unbounded additive atmospheric accumulation with bounded opacity union;
- replaces binary depth rejection with a continuous foreground-intersection fade;
- replaces three-tap maximum expansion with a normalized five-tap depth-aware cross-width filter;
- allocates one additional quarter-resolution `R16_SFloat` surface-influence texture;
- evaluates finite beam-segment influence from reconstructed camera depth for Ground and other visible opaque receivers;
- exposes green/cyan receiver influence, red individual beam-base markers, and a blue authored bundle-centre marker in `Surface Illumination`;
- composites atmosphere and surface lift independently;
- updates the authored/controller Inspectors and diagnostic report from V1.1D-A/V1.1D-B-pending wording to V1.1D-AB.

The final static audit confirms exactly the ten approved files changed, all shared shader uniforms have matching CPU uploads and consumers, only the three LightRay shaders consume the rewritten include, the new surface texture is quarter-resolution R16F, and the stale V1.1D-A atmosphere-only presentation is no longer active. No serialized default, descriptor constructor, cloud-query code, renderer feature, renderer asset, scene, prefab, layer, tag, or component was changed.

Unity compilation and visual/runtime evidence are unavailable in the supplied-file workspace. Therefore this patch is not accepted, frozen, or cleared for procedural placement.

---


## AE. `WEATHER-LIGHT-RAY-V1.1D-AC` unified zone footprint and dense beam-cluster ledger

### AE.1 Objective

Replace the rejected V1.1D-AB interpretation of one surface lobe per atmospheric beam with the approved zone-owned model:

- one LightRay zone owns one contiguous circular surface-illumination footprint;
- a variable number of complete parallel atmospheric beams provide internal visual structure inside that zone;
- beam bases remain inside the shared zone and visibly continue to the ground-contact plane;
- beam width, intensity, softness, and phase vary independently without becoming separate light sources;
- Raw and Softened diagnostics must be materially distinguishable.

This ledger is the first repository modification for V1.1D-AC. Runtime implementation remains pending until this plan is recorded.

### AE.2 Unity evidence and rejected findings

User-supplied final-composite, Raw, Softened, and Surface Illumination captures from V1.1D-AB establish the following failures:

1. `Surface Illumination` renders one isolated circular lobe per beam. This exposes a wrong ownership model: atmospheric sub-beams were incorrectly treated as independent surface lights.
2. The final composite therefore reads as several separate beam impacts rather than one authored LightRay zone.
3. The ground-end profile reaches zero at `V = 0`, so the shafts visually lift away from the ground and terminate above separate spots instead of entering the shared contact area.
4. The fixed `3–5` count proof is insufficient for authored footprint scale. Small zones may require approximately four beams while larger zones may require approximately ten.
5. Centre-to-centre metre spacing creates a sparse barcode. The visual target requires denser packing, partial overlap, and internal variation rather than mandatory clear gaps between every beam.
6. Raw and Softened captures are nearly identical. The existing five-tap one-texel filter radius is too small to produce useful atmospheric integration.
7. Red per-beam base markers visually reinforce the rejected per-beam-impact interpretation and are not suitable as the normal surface-light diagnostic.

These failures are not accepted tuning outcomes. They are recorded as architecture constraints to prevent restoration of per-beam surface lobes, mandatory sparse gaps, zero-opacity ground endpoints, or ineffective softening.

### AE.3 Approved replacement contract

One authored LightRay descriptor owns two related but distinct structures.

#### Shared zone

- one authored world-space contact centre;
- one explicit circular footprint radius in metres;
- one footprint edge-softness control;
- one Ground intensity and one visible-object intensity;
- one bounded cloud-compensation contribution;
- one surface field evaluated from horizontal distance to the shared contact centre, not distance to each beam axis.

#### Atmospheric beam cluster

- `2–12` complete parallel beams in one retained combined mesh and one draw;
- one normalized `Beam Packing` control derives dense centre spacing from representative beam width and clamps the cluster inside the shared footprint;
- no per-beam surface-light circles;
- stable per-beam width, opacity, softness, fade, and phase variation;
- partial overlap is valid and expected;
- no random hard cuts, sampled discs, broad envelope, or disconnected pieces;
- lower atmospheric density retains a nonzero contact floor so shafts visibly reach the authored ground plane.

The shared footprint is the authoritative surface-light model. Atmospheric beams may modulate visual density but do not own separate ground illumination.

### AE.4 Approved file scope

Modify only:

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs
Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs
Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayMask.shader
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayScatter.shader
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayComposite.shader
```

Reviewed and intentionally unchanged:

```text
Assets/Game/Procedural/Weather/WeatherLightRaySourceProfile.cs
Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs
Assets/Game/Rendering/Weather/WeatherLightRayRendererFeature.cs
Assets/Settings/PC_Renderer.asset
Assets/Settings/PC_RPAsset.asset
Assets/Docs/Weather_System_Architecture_Provisional.md
Assets/Docs/Weather_Cloud_Shadow_Handoff.md
Assets/Docs/Weather_Inspector_Cleanup_Plan.md
```

No scene, prefab, material, renderer asset, layer, tag, component, folder, or external dependency is added or modified.

### AE.5 Reviewed evidence before implementation

The complete current V1.1D-AB implementation, direct producer/consumer contracts, canonical Weather documents, and repository rules were reread before this ledger was written.

- `WeatherLightRayTypes.cs`: `BeamCount` is clamped to `3–5`; `BeamSpacingMetres` is an absolute centre-spacing contract; the descriptor lacks one explicit shared footprint radius.
- `WeatherLightRayAnchor.cs`: the authored component exposes `Beam Spacing` and six surface controls whose active implementation is per-beam; the existing serialized spacing value must not be reinterpreted silently.
- `WeatherLightRayCommon.hlsl`: `WeatherLightRayEvaluateSurfaceInfluence` loops over all beams, evaluates distance to each finite beam segment, and unions separate lobes; `WeatherLightRayEvaluateContactMarkers` draws one base marker per beam.
- `SH_WeatherLightRayMask.shader`: `groundFade` is `smoothstep(0, fadeLength, V)`, therefore atmospheric density is exactly zero at the authored ground contact.
- `SH_WeatherLightRayScatter.shader`: five taps are separated by one quarter-resolution texel and blended by the existing strength; supplied captures show no meaningful visual difference.
- `WeatherLightRayRenderPass.cs`: mesh count, HLSL maximum, screen bounds, and reports are all fixed to `3–5` and absolute metre spacing.
- `SH_WeatherLightRayComposite.shader`: the normal surface debug combines the separate surface lobes with red per-beam markers and a blue centre marker.

### AE.6 Fixed implementation decisions and defaults

New active serialized controls:

| Control | Default | Range / semantics |
|---|---:|---|
| Beam Count | `5` | `2–12`; exact active atmospheric beam count. Existing authored value remains `5`. |
| Beam Packing | `0.30` | `0–1`; `0` is strong overlap, `1` approaches one representative beam width between centres. Cluster extent is clamped inside the shared footprint. |
| Beam Softness Variation | `0.35` | `0–0.75`; stable per-beam variation around the global Beam Edge Softness. |
| Footprint Radius | `2.40 m` | `0.10–20 m`; one circular zone-owned surface-light radius. |

The existing Beam Intensity Variation control remains serialized at `0.18`. The current shader converts it to a perceptual amplitude with `sqrt(saturate(value))`, producing approximately `±0.424` stable variation at the existing proof value. This non-linear mapping is intentional for the authored proof because the prior bounded-union captures made linear `±0.18` differences visually negligible. `0` still produces no variation. This mapping must be reassessed during calibration rather than silently treated as a linear percentage.

Existing serialized `beamSpacingMetres` data is retained only in a hidden legacy field and is not reinterpreted as packing. This avoids silently converting the existing `1.05 m` value into a different unit or meaning.

Existing active defaults remain unchanged unless explicitly listed above. `Ground Fade` retains its serialized value and becomes a contact-transition length with a nonzero lower opacity floor rather than a fade to zero.

Derived centre spacing:

```text
representativeWidth = lerp(minWidth, maxWidth, 0.60)
preferredSpacing = representativeWidth * lerp(0.42, 0.98, beamPacking)
footprintLimitedSpacing = (2 * footprintRadius * 0.82) / max(1, beamCount - 1 + 2 * lateralDriftStrength)
centreSpacing = min(preferredSpacing, footprintLimitedSpacing)
```

The formula permits partial overlap by default and keeps all beam bases inside the shared footprint.

### AE.7 File-by-file implementation sequence

1. Expand the immutable descriptor and authored component with Beam Packing, Beam Softness Variation, and Footprint Radius; raise the active count limit to `2–12`; retain obsolete metre spacing only as hidden serialized legacy data.
2. Update both Inspectors and controller diagnostics to describe one zone, one footprint, variable beam count, dense packing, and V1.1D-AC status.
3. Raise combined-mesh and HLSL capacity to twelve beams; upload footprint radius and packing-derived spacing without changing the renderer feature or renderer assets.
4. Centralize derived dense spacing and per-beam softness variation in `WeatherLightRayCommon.hlsl`.
5. Replace per-beam surface-lobe evaluation with one horizontal circular footprint centred on the authored contact point; retain normal-based Ground/object multipliers and cloud compensation.
6. Replace per-beam contact markers with one footprint boundary marker and one centre marker.
7. Change the lower atmospheric profile from zero-opacity termination to a nonzero contact floor while retaining a controllable transition length.
8. Increase the normalized depth-aware softening radius so Raw and Softened views become materially distinct without introducing along-beam scatter or broken continuity.
9. Update the canonical architecture sections and this ledger with exact final differences, static evidence, and pending Unity validation.

### AE.8 Acceptance criteria

- Surface Illumination shows one contiguous circular field, not one lobe per beam.
- Footprint Radius visibly changes the size of that one field.
- Beam Count supports at least `4` and `10` in the same authored contract without another renderer path.
- Default packing permits neighbouring shafts to overlap or nearly meet; no mandatory barcode gaps remain.
- Width, intensity, and softness differences remain visible between beams.
- Every beam remains complete and parallel and visibly reaches the ground-contact plane.
- Raw and Softened debug views are materially different; Softened remains depth-aware and does not restore a broad envelope.
- Final Composite uses subtle atmospheric shafts plus one readable warm surface zone.
- Cloud query, source gates, lifecycle, registration, Render Graph ownership, and source-neutral descriptors remain intact.
- No C# errors, shader errors, Render Graph warnings, or recurring managed allocations attributable to this patch are accepted.

### AE.9 Risks and mitigations

| Risk | Mitigation |
|---|---|
| Twelve-beam loop increases surface cost. | Remove the per-beam surface loop entirely; the shared footprint is constant-cost. Atmospheric mesh remains only `48` vertices and `72` indices at twelve beams. |
| Dense packing collapses into one opaque white ribbon. | Retain bounded opacity-union blending, stronger per-beam opacity/softness variation, and independently controllable packing. |
| Shared circular footprint lights unrelated elevated geometry. | Keep separate Ground/object multipliers and validate authored radius in the target scene; no receiver-specific shader integration is added in this patch. |
| Beam bases fall outside the footprint. | Clamp derived centre spacing so the outer base centres remain within `82%` of the footprint radius. |
| One horizontal base plane mismatches uneven terrain beneath outer beams. | Retain depth occlusion and a nonzero contact floor for this patch, validate on sloped/uneven Ground, and open a separate beam-specific contact-depth correction only if floating or burial remains visible. |
| Ground contact becomes a hard rectangular cut. | Retain the existing transition-length control but fade toward a nonzero contact floor rather than zero; validate contact against depth and ground angle. |
| Wider softening merges the entire cluster. | Use a normalized finite kernel, depth rejection, bounded radius, and the existing strength control; Raw remains available for structural proof. |
| Legacy spacing data is misread as packing. | Preserve it in a hidden legacy field and introduce a distinct serialized Beam Packing field. |

### AE.10 Validation plan

Static checks available in the supplied-file workspace:

- exact changed-file scope and final diff audit;
- descriptor constructor/producer/consumer alignment;
- serialized field and custom Inspector property resolution;
- HLSL maximum-beam, mesh-capacity, uniform-upload, and loop-bound alignment;
- proof that surface evaluation has no per-beam loop or per-beam lobe union;
- proof that ground contact no longer reaches zero solely because `V = 0`;
- proof that only the three LightRay shaders consume the shared include;
- balanced C#/HLSL/Shader delimiter and preprocessor scan;
- stale wording and rejected-contract search.

Required Unity 6000.5.0f1 evidence:

1. compile with zero C#, shader, and Render Graph errors;
2. Surface Illumination at two different Footprint Radius values shows one correctly sized contiguous circle;
3. Beam Count `4` and `10` both render complete parallel shafts inside the same zone contract;
4. Beam Packing visibly transitions from overlapping/dense to more separated without moving the shared footprint;
5. beam bottoms visibly contact the ground and do not terminate above isolated circles;
6. Softened Continuous Beams is visibly softer than Raw Continuous Beams while retaining directional structure;
7. Final Composite shows one shared illuminated area with varied internal atmospheric shafts; repeat once over uneven or sloped Ground to check that outer beam bottoms do not visibly float or bury.

### AE.11 Status before runtime edits

`PLAN RECORDED — IMPLEMENTATION PENDING`.

This checkpoint is intentionally retained to prove that the architecture, scope, defaults, migration rules, and acceptance criteria were recorded before runtime modification.

### AE.12 Implementation result

`SOURCE IMPLEMENTED — UNITY VALIDATION PENDING`.

The supplied-file implementation now:

- expands Beam Count from the rejected `3–5` proof limit to `2–12` while retaining one combined mesh and one draw;
- replaces active absolute Beam Spacing with normalized Beam Packing and retains the old serialized metre value only as hidden legacy data;
- adds stable per-beam softness variation and one explicit Footprint Radius;
- derives dense centre spacing from representative width, packing, count, drift allowance, and footprint extent;
- keeps every atmospheric beam straight, parallel, continuous, and camera-readable;
- retains nonzero lower atmospheric density so beam geometry reaches the contact plane;
- replaces the per-beam surface loop and lobe union with one constant-cost horizontal circular field;
- replaces per-beam impact markers with one red footprint boundary and one blue centre marker;
- replaces the ineffective five-tap softener with a normalized seven-tap depth-aware cross-width filter;
- updates both Inspectors, controller reports, active architecture sections, risks, benchmarks, acceptance criteria, and failure history.

No cloud-query, source-profile, renderer-feature, renderer-asset, Render Pipeline asset, scene, prefab, material, layer, tag, component, or external dependency was modified.

### AE.13 Recorded failings and future prohibitions

The following findings are now permanent constraints unless later evidence explicitly reopens them:

1. A per-beam surface lobe is not a valid approximation of one illuminated LightRay zone.
2. Beam Count must not multiply surface brightness or create additional surface circles.
3. Mandatory equal gaps produce a barcode and are not part of the visual contract.
4. Ground alpha reaching zero at the beam endpoint creates detached shafts.
5. A screen-space pass that is visually indistinguishable from Raw is unjustified cost.
6. Stable variation must affect width, opacity, softness, and fades; random hard cuts remain forbidden.
7. Legacy serialized values must not be silently reinterpreted under new units or semantics.
8. Debug visualization must reinforce real ownership: one zone boundary and centre, not misleading per-beam impact markers.

### AE.14 Static validation and compliance evidence

The supplied-file static audit passed after implementation:

- exact changed-file scope: `11/11` approved files;
- immutable descriptor constructor and authored producer alignment: `38/38` arguments;
- visible serialized anchor controls and custom Inspector property resolution: `38/38`;
- mesh/HLSL beam limits aligned at `2–12`, with `48` vertices and `72` indices at maximum count;
- `_WeatherLightRayBeamShape2` and unified surface parameters are declared and uploaded consistently;
- surface influence contains no beam loop, beam-base lookup, point-to-segment union, or per-beam lobe ownership;
- ground contact no longer fades to zero solely because `V = 0`;
- the softener uses seven normalized depth-aware cross-width taps;
- only the three LightRay shaders consume `WeatherLightRayCommon.hlsl`;
- no rejected sampled-frustum, ellipse, broad-envelope, random-cut, or directional-extension symbol remains active;
- no per-frame managed collection allocation was introduced in parameter/bounds construction;
- delimiter, preprocessor, UTF-8, NUL-byte, final-newline, Markdown-fence, unchanged-invariant, and stale-active-wording scans passed.

Static validation cannot establish Unity compilation, shader backend acceptance, Render Graph correctness, depth reconstruction, or visual quality.

### AE.15 Pending Unity validation and blocker

This patch is not frozen or accepted. The supplied workspace cannot run Unity 6000.5.0f1, D3D11/D3D12 shader compilation, Render Graph execution, Frame Debugger, GPU Profiler, or gameplay-camera visual proof.

Required acceptance evidence remains exactly the seven Unity checks in AE.10, including complete Console output and captures of Raw, Softened, Surface Illumination, and Final Composite.

## AF. `WEATHER-LIGHT-RAY-V1.1D-AD` exact area-driven layout plan

### AF.1 Approval and scope

User approval authorizes the mathematical replacement described in the preceding audit. This ledger is the first repository modification for V1.1D-AD. No runtime file is modified before this plan.

Approved files:

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs
Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs
Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayMask.shader
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayScatter.shader
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayComposite.shader
```

No renderer feature, renderer asset, Render Pipeline asset, scene, prefab, material, source profile, cloud-query implementation, layer, tag, component, new file, or external dependency is authorized.

### AF.2 Reviewed evidence

The complete current V1.1D-AC descriptor, authored producer, custom Inspectors, controller registration/report path, Render Graph pass, common HLSL contract, mask, softening, composite, renderer-feature consumer, source profile, and this canonical document were reviewed before implementation.

Exact failures:

1. `WeatherLightRayRenderPass.BuildDerivedBeamSpacing` computes a preferred width-based spacing and caps it at `1.64R / (N - 1 + 2d)`. Therefore the first-to-last centre span is always less than `1.64R`, or less than `82%` of the footprint diameter.
2. `BuildBundleAxis` uses `cross(cameraForward, upwardAxis)` without a ground-plane projection. The resulting axis may have nonzero `Y`, so `base_i = centre + axis * offset_i` moves beam contacts above and below the authored horizontal contact plane.
3. `WeatherLightRayGetBeamBase` adds a different sinusoidal drift to each beam, so exact endpoint anchoring cannot remain true over time.
4. The surface field measures XZ distance around the authored centre while beam bases use the full three-dimensional displaced axis. The footprint and beam contacts therefore do not share one geometry.
5. Raw and Softened both receive a four-sample low-resolution upsample, while the current seven-tap pass blends only weakly toward a narrow filter. Their effective edge-width difference is too small to justify separate debug views.

### AF.3 Approved constants and derived layout

The one active area control is:

```text
Light Ray Area Diameter D
range: 0.60 m to 6.60 m
default: 4.80 m
```

Approved fixed density and capacity:

```text
maximum centre pitch p* = 0.60 m
minimum beam count = 2
maximum beam count = 12
maximum diameter = (12 - 1) * 0.60 m = 6.60 m
```

Derived layout:

```text
R = D / 2
segmentCount = ceil(D / p*)
N = segmentCount + 1
s = D / segmentCount = D / (N - 1)
```

The `ceil` rule guarantees:

```text
s <= p*
```

Exact centreline contacts for beam index `i = 0 .. N - 1`:

```text
t_i = i / (N - 1)
P_i = C + G * D * (t_i - 0.5)
```

Therefore:

```text
P_0     = C - G * R
P_N-1   = C + G * R
distance(P_0, P_N-1) = D
```

The approved interpretation is **centreline anchoring**: the first and last beam axes touch opposite footprint-diameter endpoints. Visible ribbon half-width may extend outside the mathematical circle; the circle and beam-axis span remain exact.

### AF.4 Ground contact axis

Let:

```text
F = gameplay camera forward
U = upward beam direction = -rayDirection
Y = world ground normal = (0, 1, 0)
```

Start from the camera-readable cross-axis:

```text
Q = cross(F, U)
```

Project it onto the authored horizontal contact plane:

```text
G_raw = Q - Y * dot(Q, Y)
G = normalize(G_raw)
```

Stable sign:

```text
if dot(G, cameraRight) < 0: G = -G
```

Fallback order when the projection is degenerate:

1. horizontal projection of camera right;
2. horizontal projection of a stable basis perpendicular to `U`;
3. world right.

This guarantees:

```text
dot(G, Y) = 0
P_i.y = C.y for every beam
```

The authored anchor world-horizontal plane is authoritative for this patch. Projection onto arbitrary terrain is deferred and requires separate evidence.

### AF.5 Width contract

Absolute independent beam widths are replaced by a ratio of derived pitch:

```text
w_i = s * widthRatio_i
```

Active default ratio range:

```text
1.00 to 1.25
```

This is the smallest default change that removes mandatory geometric gaps while preserving the previous `0.75 m` maximum width at the approved `0.60 m` pitch. Ratio values remain authorable visual controls; they do not affect area, count, pitch, or footprint geometry.

Old Beam Count, Beam Packing, Footprint Radius, absolute Beam Width Range, and Lateral Drift fields remain hidden legacy serialization only. They are not reinterpreted and do not influence runtime layout.

### AF.6 Contact invariants

Ground-contact positions do not drift. Temporal evolution may change width, opacity, softness, fades, and longitudinal density only.

Required numerical invariants:

```text
abs(dot(P_i - C, Y)) < 1e-5
abs(distance(P_0, C) - R) < 1e-4
abs(distance(P_N-1, C) - R) < 1e-4
abs(distance(P_0, P_N-1) - D) < 1e-4
```

Surface Illumination diagnostics must show one footprint boundary, one diameter line, both endpoint markers, and the zone centre so the invariant can be visually checked without inferring contact from atmosphere.

### AF.7 Raw and Softened final attempt

Raw must sample the unfiltered quarter-resolution mask directly. It must not use the same four-sample depth-aware prefilter as Softened.

The softening axis is normalized in quarter-resolution pixel space rather than viewport space:

```text
deltaPixels = (deltaUV.x * quarterWidth, deltaUV.y * quarterHeight)
directionPixels = normalize(deltaPixels)
uvStep = (directionPixels.x / quarterWidth,
          directionPixels.y / quarterHeight)
```

The filter radius is derived from projected representative beam width, not a fixed arbitrary texel radius.

Let `M` be the raw mask and `B` the normalized cross-width blur. The softened mask is bounded opacity union:

```text
S = 1 - (1 - M) * (1 - strength * B)
```

This guarantees:

```text
0 <= S <= 1
S >= M
strength = 0 -> S = M
```

Acceptance requirement:

```text
Softened 10–90% edge width >= 1.5 * Raw 10–90% edge width
```

If Unity captures still show no material distinction, the Softened debug view and filter pass must be removed rather than retained as redundant cost.

### AF.8 File-by-file implementation sequence

1. Replace independent descriptor inputs with area diameter, derived layout values, and beam-width ratios in `WeatherLightRayTypes.cs`.
2. Migrate authored serialization safely in `WeatherLightRayAnchor.cs`; expose only area diameter and visual ratio controls while preserving rejected values as hidden legacy data.
3. Update authored and controller Inspectors plus reports to display area diameter, derived radius, count, pitch, and width range.
4. Replace width/packing spacing math with exact area layout and a horizontal ground-contact axis in `WeatherLightRayRenderPass.cs`.
5. Replace beam-base placement and remove base drift in `WeatherLightRayCommon.hlsl`; add diameter/endpoint diagnostics.
6. Keep mask continuity but consume exact contacts and pitch-derived widths in `SH_WeatherLightRayMask.shader`.
7. Implement projected-width-scaled bounded halo union in `SH_WeatherLightRayScatter.shader`.
8. Make Raw genuinely raw and Softened directly display the filtered result in `SH_WeatherLightRayComposite.shader`.
9. Re-read all reviewed files, compare against V1.1D-AC, run scope/contract/static audits, and record exact results here.

### AF.9 Acceptance criteria

- One active Light Ray Area Diameter control derives footprint radius, beam count, beam pitch, and exact first-to-last span.
- Default `D = 4.80 m` derives `R = 2.40 m`, `N = 9`, and `s = 0.60 m`.
- First and last beam centre axes touch opposite circle-diameter endpoints.
- Every beam base has the authored centre `Y` coordinate; no base-position evolution remains.
- Beam widths are pitch-relative and default to touching/overlapping rather than mandatory gaps.
- Surface Illumination remains one circle and displays the exact diameter/endpoints/centre diagnostics.
- Raw and Softened are materially distinct under the quantitative edge-width requirement.
- Controller, lifecycle, source, cloud, registration, camera eligibility, and Render Graph ownership remain unchanged.
- No C# errors, shader errors, Render Graph warnings, or recurring managed allocations attributable to this patch are accepted.

### AF.10 Status before runtime edits

`PLAN RECORDED — IMPLEMENTATION PENDING`.

This checkpoint is retained as evidence that the mathematical contract, defaults, migration rules, file scope, and acceptance criteria were recorded before runtime modification.

### AF.11 Implementation result

`SOURCE IMPLEMENTED — UNITY VALIDATION PENDING`.

The supplied-file implementation now:

- adds `WeatherLightRayAreaLayout` as the one immutable diameter-to-radius/count/pitch contract;
- clamps Light Ray Area Diameter to `0.60–6.60 m`, with default `4.80 m` deriving radius `2.40 m`, count `9`, and pitch `0.60 m`;
- replaces active absolute widths with pitch-relative width ratios `1.00–1.25`;
- preserves rejected count, width, packing, footprint-radius, spacing, and lateral-drift values as hidden legacy serialization only;
- computes one horizontal contact axis by projecting the camera-readable cross-axis onto world up and applying stable fallbacks/sign;
- places beam `i` at `C + G * D * (i / (N - 1) - 0.5)`, with no base-position drift;
- uses the derived count for the retained combined mesh and the derived pitch for width scale;
- retains one circular depth-reconstructed surface field whose radius is `D / 2`;
- adds footprint boundary, exact diameter, endpoint, and centre diagnostics;
- normalizes the softening direction in quarter-resolution pixel space;
- derives softening radius from projected representative beam width;
- makes Raw a direct point-sampled mask and Softened a bounded core-plus-halo union.

No renderer feature, renderer asset, Render Pipeline asset, scene, prefab, material, source profile, cloud-query code, layer, tag, component, new file, or dependency changed.

### AF.12 Static and mathematical audit

The supplied-file audit passed:

- exact changed-file scope: `11/11` approved files;
- immutable descriptor constructor and authored producer alignment: `35/35` arguments;
- custom Inspector serialized-property resolution: `35/35` active fields;
- derived samples: `0.60 m -> 2`, `1.80 m -> 4`, `4.80 m -> 9`, `5.40 m -> 10`, `6.60 m -> 12`, with pitch never exceeding `0.60 m`;
- exact normalized endpoint formula is present and per-beam base drift is absent;
- CPU contact-axis construction contains the required world-horizontal projection and stable sign/fallbacks;
- descriptor and renderer contain no active Beam Packing, independent Footprint Radius, absolute Beam Width Range, or Lateral Drift contract;
- C# shader property IDs and HLSL uniforms align for Area Diameter and Ground Contact Axis;
- Raw no longer uses the shared four-sample prefilter; Softened uses bounded opacity union;
- diameter and endpoint diagnostics are present in both common HLSL and Composite;
- only the three LightRay shaders consume the shared include;
- delimiter, NUL-byte, UTF-8, and final-newline checks passed.

Static validation cannot prove Unity compilation, D3D11/D3D12 shader acceptance, Render Graph correctness, actual depth contact, projected marker alignment, or the required Raw/Softened edge-width ratio.

### AF.13 Pending Unity acceptance

The patch remains unaccepted until Unity proves:

1. zero C#, shader, and Render Graph errors;
2. Area Diameter `1.80 m`, `4.80 m`, and `5.40 m` derives `4`, `9`, and `10` beams respectively;
3. Surface Illumination shows one circle with a diameter line whose endpoints align with the first and last beam axes;
4. every beam reaches the authored horizontal contact plane without floating or burial caused by the old nonhorizontal axis;
5. Softened has at least `1.5x` the visible Raw edge width and remains direction-preserving;
6. Final Composite remains one coherent area-driven zone.

### AF.14 Historical follow-on recorded at that point

- Run the V1.1D-AD Unity structural proof and correct only evidence-backed compile, contact, alignment, or softening failures.
- Begin the next calibration stage only after V1.1D-AD structural acceptance; procedural placement remained blocked.

## AG. V1.1D-AD1 — Contact-owned beam geometry and full-resolution atmospheric depth

### AG.1 Status before runtime edits

`PLAN RECORDED BEFORE RUNTIME EDITS — SOURCE IMPLEMENTED, UNITY VALIDATION PENDING`.

This section is the first modification after the V1.1D-AD Unity evidence. Runtime code must not change until this plan exists.

### AG.2 Observed failures

The V1.1D-AD Unity captures prove the following:

1. Area Diameter correctly derives the footprint radius and beam count.
2. The visible beam bottoms do not coincide with the shared footprint diameter.
3. Moving the followed player along world Z changes the apparent visible shaft length even though the authored LightRay zone does not move.
4. The visible beam/ground boundary is stair-stepped and unstable.
5. Raw and Softened are technically different but remain too similar to justify the current filter cost confidently.

These findings reject the assumption that an exact analytical beam-centre line is sufficient while a quarter-resolution manual scene-depth comparison remains authoritative over the visible contact.

### AG.3 Reviewed evidence and exact failure mechanism

Reviewed implementation surface:

```text
Assets/AGENTS.md
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs
Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
Assets/Game/Rendering/Weather/WeatherLightRayRendererFeature.cs
Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs
Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayMask.shader
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayScatter.shader
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayComposite.shader
Assets/_Recovery/0.unity
```

Relevant current equations:

```text
beam centre i:
B_i = C + G * D * (i / (N - 1) - 0.5)

visible bottom edge of beam i:
E_i(x) = B_i + G * x * w_i / 2, x in [-1, 1]
```

The first centre is `C - G * R`, but its outer visible edge is:

```text
C - G * (R + w_0 / 2)
```

The last outer visible edge is:

```text
C + G * (R + w_last / 2)
```

Therefore centreline anchoring mathematically guarantees that the visible contact envelope exceeds the footprint diameter whenever beam width is nonzero. It cannot satisfy the observed visual requirement that the first visible beam edge and last visible beam edge define the circle diameter.

The current mask is quarter resolution:

```text
maskWidth  = cameraWidth  / 4
maskHeight = cameraHeight / 4
```

The mask fragment then compares reconstructed scene distance against interpolated ribbon distance:

```text
foregroundSeparation = ribbonDistance - sceneDistance
depthFade = 1 - smoothstep(0.015, fadeRange, foregroundSeparation)
```

This makes the visible lower endpoint a screen-depth result rather than a contact-geometry result. Because the gameplay camera follows the player, player Z movement translates the camera. The set of low-resolution samples whose camera rays encounter ground before the ribbon changes, so the apparent visible beam length changes even when the zone remains fixed.

The stair-step is also expected: the contact rejection decision is made on the quarter-resolution atmospheric mask and then enlarged during composite.

### AG.4 Corrected ownership model

The shared LightRay zone owns one exact visible contact envelope. The footprint and atmospheric beam bottoms must derive from the same interval:

```text
contact interval = [-R, +R] along horizontal contact axis G
D = 2R
```

The interval is divided into `N` exact contact cells:

```text
cellWidth = D / N
cell centre i = -R + (i + 0.5) * cellWidth
```

Beam base centre:

```text
B_i = C + G * (-R + (i + 0.5) * cellWidth)
```

Beam bottom edges:

```text
left_i  = B_i - G * cellWidth / 2
right_i = B_i + G * cellWidth / 2
```

Endpoint proof:

```text
left_0 = C - G * R
right_(N-1) = C + G * R
```

Adjacent-cell proof:

```text
right_i = left_(i+1)
```

Therefore the visible lower beam envelope is exactly the footprint diameter, with no gaps, no overshoot, and no independent tuning state.

This supersedes V1.1D-AD centreline endpoint anchoring. The derived beam count remains unchanged; only the meaning of the contact layout changes from centre endpoints to visible contact-cell endpoints.

### AG.5 Atmospheric width above contact

The exact contact-cell width is not required to become the full atmospheric width for the entire shaft.

```text
contactWidth = D / N
atmosphericWidth_i = BeamPitch * widthRatio_i
```

The lower ribbon edge uses `contactWidth`. The upper ribbon edge uses the varied atmospheric width. This keeps the ground envelope exact while retaining independent atmospheric width variation above contact.

Width breathing must not change the lower contact edge. It may affect the upper atmospheric width only.

The lower contact band must also use one shared diameter envelope rather than letting every individual beam profile fade to zero at each contact-cell boundary. For beam index `i` and local across coordinate `u in [0, 1]`:

```text
contactCoordinate = (i + u) / N
globalAcross = abs(2 * contactCoordinate - 1)
```

A narrow global edge fade is applied only near `globalAcross = 1`. Internal cell boundaries remain continuous. Over a short derived world-height band above contact, the profile transitions from the shared contact envelope to the individual atmospheric beam profile. This prevents mathematically adjacent cells from still appearing visually separated because both local beam profiles reached zero at their common edge.

### AG.6 Contact visibility and depth ownership

Scene depth may occlude atmospheric shafts, but it must not redefine the ground receiver contact.

The atmospheric mask moves from quarter resolution to full camera resolution. Surface illumination remains quarter resolution.

At each beam fragment:

```text
rawDepth -> reconstructed scene position P_scene
base plane height = C.y
heightAboveContact = P_scene.y - C.y
```

Ground-like receiver classification uses both receiver height and reconstructed normal:

```text
nearContactPlane = 1 - smoothstep(0.12 m, 0.40 m, heightAboveContact)
groundFacing = smoothstep(0.45, 0.78, abs(normalWS.y))
receiverWeight = max(nearContactPlane, groundFacing)

contactPreserveHeight = max(0.75 m, 2 * contactCellWidth)
contactPreserveWeight = 1 - smoothstep(
    0,
    contactPreserveHeight,
    axialHeightAboveBase)

receiverWeight = max(receiverWeight, contactPreserveWeight)
occluderWeight = 1 - receiverWeight
```

The geometry-owned contact-preservation band is deliberately independent from sampled scene depth. It guarantees that low receiver geometry, grass cards, and small contact-area depth discontinuities cannot shorten the shaft before it reaches the authored base line. True foreground occlusion resumes above the derived band.

The ordinary manual depth comparison remains available for true foreground occluders:

```text
depthFade = lerp(1, depthTestFade, occluderWeight)
```

Consequences:

- ground and bridge-like upward-facing receiver surfaces do not truncate the shaft before contact;
- player, rock, monument, and other non-ground foreground geometry may still occlude the shaft;
- camera translation no longer lets the ground plane redefine visible beam length;
- full-resolution mask evaluation removes the quarter-resolution staircase from the atmospheric contact edge.

### AG.7 Raw and Softened final acceptance attempt

Raw remains the unfiltered full-resolution mask.

Softened uses a visibly broader full-resolution cross-width halo. Its radius is derived from projected representative beam width in full-resolution pixels:

```text
radiusPixels = clamp(
    projectedBeamWidthPixels * lerp(0.35, 0.75, strength),
    4,
    32)
```

The filter remains cross-width only and bounded by opacity union:

```text
S = 1 - (1 - M) * (1 - strength * B)
```

Acceptance remains objective:

```text
Softened 10–90% edge width >= 1.5 * Raw 10–90% edge width
```

If the Unity evidence still fails this threshold, remove the Softened debug view and softening pass in the next patch. Do not retain another weak duplicate.

### AG.8 Approved file scope

Runtime and canonical documentation changes are limited to:

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs
Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayMask.shader
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayScatter.shader
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayComposite.shader
```

No descriptor serialization, renderer feature, renderer asset, Render Pipeline asset, scene, prefab, material, cloud-query code, layer, tag, component, new file, or dependency is approved.

### AG.9 Non-goals

This patch does not:

- project each beam onto arbitrary terrain height;
- add physics raycasts or CPU receiver queries;
- change Area Diameter, beam-count, maximum-pitch, lifetime, source, cloud, or registration contracts;
- perform final visual colour/intensity calibration;
- enable procedural spawning;
- claim performance acceptance before profiling.

### AG.10 File-by-file implementation sequence

1. Update authored/controller wording and diagnostics to identify visible-edge contact cells, contact-cell width, full-resolution atmosphere, and V1.1D-AD1 status.
2. Change beam-base centres from endpoint-centred spacing to exact contact-cell centres in `WeatherLightRayCommon.hlsl`.
3. In `SH_WeatherLightRayMask.shader`, use exact contact-cell width at the lower edge, retain varied width at the upper edge, exempt ground-like receivers from depth truncation, and preserve true foreground occlusion.
4. Split atmospheric and surface render descriptors in `WeatherLightRayRenderPass.cs`: full-resolution Raw/Softened atmosphere, quarter-resolution surface influence.
5. Recalculate softening direction/radius in full-resolution pixel space and increase the bounded radius to the documented projected-width range.
6. Update `SH_WeatherLightRayScatter.shader` only as needed to consume the full-resolution radius and produce the required visible halo.
7. Update `SH_WeatherLightRayComposite.shader` so quarter-resolution surface upsampling uses an explicit surface texel size rather than the atmospheric-mask texel size.
8. Re-read the complete review surface, compare against V1.1D-AD, run exact scope/contract/static audits, and record final evidence here.

### AG.11 Acceptance criteria

- Area Diameter still derives the same footprint radius and beam count.
- First visible lower beam edge equals one footprint endpoint.
- Last visible lower beam edge equals the opposite footprint endpoint.
- Adjacent lower beam edges meet exactly without geometric gaps.
- Moving the followed player only along Z does not change the visible shaft-ground contact line except where the player itself genuinely occludes a shaft.
- Atmospheric contact no longer has quarter-resolution staircase pixels.
- Ground and upward-facing bridge receiver surfaces do not truncate the beam before contact.
- Non-ground foreground objects may still occlude the atmospheric shaft.
- Raw is a full-resolution unfiltered mask.
- Softened meets the `1.5x` edge-width criterion or is explicitly rejected for removal.
- Surface illumination remains one shared circle.
- No C#, shader, Render Graph, allocation, or cross-subsystem regression is accepted.

### AG.12 Implemented source result

`SOURCE IMPLEMENTED — UNITY COMPILATION AND VISUAL VALIDATION PENDING`.

The implementation follows the recorded sequence without scope expansion.

#### Contact ownership

`WeatherLightRayCommon.hlsl` now derives the lower contact cell width and cell-centred beam bases from the one area diameter:

```text
contactCellWidth = D / N
B_i = C + G * D * ((i + 0.5) / N - 0.5)
```

The lower ribbon width is exactly `contactCellWidth`. The upper ribbon width remains the varied atmospheric width. Therefore the visible lower geometry satisfies:

```text
left_0 = C - G * D/2
right_(N-1) = C + G * D/2
right_i = left_(i+1)
```

The existing `BeamPitchMetres = D / (N - 1)` value remains only the density/width-reference pitch inherited from V1.1D-AD. It is no longer the physical spacing between contact-cell centres. The actual lower contact-centre spacing is `D / N`. Inspector and report wording identifies both values separately to prevent them being confused again.

#### Continuous lower opacity

At contact, per-beam local width profiles no longer independently fade to zero at every internal cell boundary. The mask computes one zone-wide contact coordinate:

```text
contactCoordinate = (beamIndex + localU) / N
globalAcross = abs(2 * contactCoordinate - 1)
```

Only the two outer zone edges feather. A short derived height band transitions from this continuous shared profile to each beam's independent atmospheric profile. This preserves one continuous diameter at ground contact while retaining separate shafts above it.

#### Depth ownership

The atmospheric mask and softened mask now use full camera resolution. The surface-influence texture remains quarter resolution.

The mask still evaluates ordinary scene-depth occlusion, but contact visibility is protected by:

- a geometry-owned lower axial band, independent from sampled scene depth;
- near-contact receiver height classification;
- full-resolution reconstructed receiver-normal classification for low ground-facing surfaces.

Ordinary depth rejection resumes above that band. This is intentionally a structural contact proof, not final receiver projection onto arbitrary terrain.

#### Softening

Raw is a point-sampled, full-resolution unfiltered mask. Softened is a separate full-resolution seven-tap cross-width halo with radius:

```text
radiusPixels = clamp(
    projectedRepresentativeWidthPixels * lerp(0.35, 0.75, strength),
    4,
    32)
```

The quarter-resolution surface texel size is carried independently in `SofteningParameters.zw`; it is no longer inferred from the atmospheric texture.

### AG.13 Intentional differences from V1.1D-AD

The final source diff intentionally changes only these behaviours:

1. lower beam centres use contact-cell centres instead of circle-end centreline anchors;
2. lower visible widths tile the footprint diameter exactly;
3. lower contact opacity is one continuous zone envelope rather than repeated per-beam zero-edge profiles;
4. contact visibility is protected from low receiver depth while foreground occlusion remains above the contact band;
5. atmospheric mask and softening textures are full resolution;
6. surface influence remains quarter resolution and receives its own texel size;
7. softening uses a materially larger projected-width-derived radius;
8. Inspector and copied diagnostics distinguish density-reference pitch from visible contact-cell width.

The following remain unchanged:

- one Area Diameter control and its beam-count derivation;
- area range `0.60–6.60 m`;
- maximum beam count `12`;
- source, cloud, lifetime, registration, movement, and evolution contracts;
- one unified surface footprint;
- surface-light evaluation and colour/intensity controls;
- Render Graph pass order;
- authored one-zone proof limit;
- procedural spawning block.

### AG.14 Static and mathematical audit

Available source-level checks pass:

```text
Approved changed-file scope: 9 / 9
Shared HLSL include consumers: 3 / 3 intended LightRay shaders
NUL/final-newline hygiene: PASS
Delimiter balance: PASS
Whitespace/error-marker check: PASS
Atmospheric textures full resolution: PASS
Surface texture quarter resolution: PASS
Surface texel split: PASS
Contact-cell centre formula: PASS
Shared contact-opacity envelope: PASS
Geometry-owned contact-preservation band: PASS
```

Worked contact-envelope results:

```text
D = 0.60 m, N = 2,  cell = 0.300000 m, envelope = [-0.30, +0.30]
D = 1.80 m, N = 4,  cell = 0.450000 m, envelope = [-0.90, +0.90]
D = 4.80 m, N = 9,  cell = 0.533333 m, envelope = [-2.40, +2.40]
D = 5.40 m, N = 10, cell = 0.540000 m, envelope = [-2.70, +2.70]
D = 6.60 m, N = 12, cell = 0.550000 m, envelope = [-3.30, +3.30]
```

For every sample:

```text
first outer edge = -D/2
last outer edge = +D/2
maximum adjacent-cell edge gap = 0
```

At `2560 x 1440`, each full-resolution `R16_SFloat` atmospheric texture is approximately `7.03 MiB`; Raw and Softened together are approximately `14.06 MiB`. The quarter-resolution surface texture is approximately `0.44 MiB`. These are descriptor-size calculations only. Actual GPU allocation, aliasing, bandwidth, and timing remain unverified until Unity profiling.

### AG.15 Known risks and pending proof

The patch is not accepted or frozen until Unity proves all of the following:

- C# and all three shader backends compile without error or warning;
- full-resolution depth derivatives produce stable receiver normals on the target D3D11/D3D12 path;
- the geometry-owned contact band prevents camera-follow/player-Z movement from shortening the contact line;
- the protected contact band does not draw objectionably over genuine foreground object lower sections;
- first and last visible lower edges align with the footprint diameter in Surface and Final views;
- contact no longer shows quarter-resolution staircase artefacts;
- Softened reaches the documented `1.5x` edge-width threshold;
- Render Graph has no pass, texture, or resource-lifetime warning;
- 1440p GPU timing and temporary texture memory remain acceptable.

If Raw and Softened still fail the objective edge-width test, the next patch removes the softening pass and Softened debug view. It must not receive another tuning-only reprieve.

### AG.16 V1.1D-AD1 validation sequence

1. Compile and inspect the complete Console for C#, shader, material, and Render Graph errors.
2. Use Surface Illumination to verify one circle, one diameter, and exact endpoint markers.
3. Use Final Composite to verify that every lower visible beam cell reaches and tiles that diameter.
4. Move the followed player only along world Z and verify the contact line remains fixed except where the player itself genuinely occludes a shaft above the protected band.
5. Compare Raw and Softened at `Screen Softening = 1`; measure or capture the visible cross-width edge difference.
6. Profile full-resolution Raw/Softened atmosphere at `2560 x 1440` before acceptance.



## AH. V1.1D-AE — X-aligned aperture-distributed shafts

### AH.1 Status and gate order

Status when this section was first written: `PLAN RECORDED — IMPLEMENTATION NOT YET STARTED`.

Current status: `SOURCE IMPLEMENTED — SUPPLIED-FILE AUDIT PASSED — UNITY VALIDATION PENDING`.

The required order is:

1. review the complete current implementation and direct contracts;
2. record the observed failures, equations, invariants, scope, risks, and file sequence here;
3. implement only the recorded plan;
4. re-read the complete review surface and audit the final diff;
5. leave Unity compilation, rendering, and profiling explicitly pending when unavailable.

### AH.2 Reviewed evidence

The following complete current files and direct contracts were read before implementation:

```text
Assets/AGENTS.md
Assets/Docs/Weather_Light_Ray_Architecture.md

Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs
Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Procedural/Weather/WeatherLightRaySourceProfile.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs

Assets/Game/Rendering/Weather/WeatherLightRayRendererFeature.cs
Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs
Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayMask.shader
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayScatter.shader
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayComposite.shader
```

Observed Unity evidence supplied after V1.1D-AD1:

- Raw shows independent upper shafts expanding into one continuous white lower strip.
- Max Softened introduces edge/depth artefacts and remains only a secondary variation of Raw.
- Shafts remain too regular in width, opacity, spacing, and bilateral softness compared with the repeated visual reference.
- The contact line is diagonally oriented; the approved authored composition requires a world-X line.
- The reference shows aperture-like groups: small ordinary gaps, occasional larger occluder gaps, unequal widths, unequal transmissions, and left/right edge asymmetry.

### AH.3 Exact failure mechanisms

#### Shared lower envelope

`SH_WeatherLightRayMask.shader` currently interpolates geometry from `contactCellWidth` at `V = 0` to the atmospheric width at `V = 1`, then replaces every local beam profile with one zone-wide `contactEnvelope` near the bottom:

```text
vertexWidth = lerp(contactCellWidth, beamWidth, V)
widthProfile = lerp(individualWidthProfile, contactEnvelope, contactBlend)
```

This necessarily widens or narrows beams near contact and removes internal beam identity. The white lower slab is the direct expected output, not a calibration error.

#### Uniform-cell abstraction

The contact layout divides the diameter into equal cells `A / N`. Equal centres, equal lower widths, and a shared opacity envelope produce a barcode. Per-beam upper width variation cannot correct the lower geometry or grouped spacing.

#### Symmetric beam profile

The current profile begins with:

```text
across = abs(2U - 1)
```

All density, boundary, and contact-floor terms are therefore left/right symmetric. One scalar softness cannot produce a shaft that is harder on one side and more diffuse on the other.

#### Softening used beyond its valid role

The current seven-tap radius reaches up to `32` full-resolution pixels and uses reconstructed scene-depth weights at every offset. At maximum strength this can create discontinuity artefacts around occluders. A symmetric post-filter also cannot create unequal widths, grouped apertures, per-side transmission, or per-side softness.

#### Wrong layout axis

`WeatherLightRayRenderPass.BuildGroundContactAxis` derives the line from camera forward and light direction. The resulting horizontal axis is mathematically valid but visually wrong for this authored isometric composition. The approved invariant is now explicit:

```text
contactAxis = world X = (1, 0, 0)
```

### AH.4 Approved aperture-partition mathematics

Let:

```text
A     = authored area diameter
N     = area-derived beam count
gamma = 0.12 total diameter fraction reserved for aperture gaps
q_i   = deterministic positive beam-width weight, i = 0 .. N - 1
r_i   = deterministic positive gap weight, i = 0 .. N - 2
```

Budgets:

```text
W = A * (1 - gamma)
G = A * gamma
```

Normalized widths and gaps:

```text
w_i = W * q_i / sum(q)
g_i = G * r_i / sum(r)
```

Sequential layout:

```text
L_0 = -A/2
L_i = -A/2 + sum(k < i, w_k + g_k)
C_i = L_i + w_i/2
R_i = L_i + w_i
```

Exact proof:

```text
R_(N-1)
= -A/2 + sum(w) + sum(g)
= -A/2 + W + G
= -A/2 + A
= +A/2
```

Therefore:

```text
first visible geometric edge = -A/2
last visible geometric edge  = +A/2
```

The internal distribution may vary without violating the shared footprint diameter.

### AH.5 Deterministic width and gap weights

Beam width weights combine the existing authored ratio range with an automatic natural-variation term:

```text
authorWeight_i  = lerp(widthRatioMin, widthRatioMax, hashWidth_i)
naturalWeight_i = lerp(0.72, 1.28, hashShape_i)
q_i             = authorWeight_i * naturalWeight_i
```

The values are normalized, so they redistribute the fixed beam budget rather than changing the area diameter.

Gap weights use a small common range plus deterministic grouped boosts:

```text
r_i = lerp(0.55, 1.45, hashGap_i)
```

For sufficiently large `N`, one primary and one secondary deterministic gap receive bounded multipliers before normalization. This creates aperture groups while preserving the total `12%` gap budget. No beam is deleted and no hard longitudinal cut is introduced.

Worked default budget for `A = 4.80 m`, `N = 9`:

```text
beam budget = 4.224 m
gap budget  = 0.576 m
average beam width before weighting = 0.469333 m
average gap before weighting        = 0.072000 m
```

The exact individual values depend on the stable variation seed, but their total always remains `4.80 m`.

### AH.6 Asymmetric per-beam density

Each beam remains one constant-width quad from top through contact. Its profile uses signed across coordinate:

```text
s = 2U - 1
```

Stable per-beam values derive:

- left softness;
- right softness;
- peak bias;
- weaker-side choice and transmission;
- whole-beam transmission;
- upper and ground fade scales;
- evolution phase.

For peak bias `b`, the normalized distance is evaluated against the relevant side extent:

```text
q = s - b
extent = q < 0 ? 1 + b : 1 - b
d = abs(q) / max(extent, epsilon)
```

The chosen side softness controls the density exponent. A side-transmission gradient makes one side more or less visible without breaking the beam. The profile still reaches zero only at its own geometric interval boundary.

Whole-beam intensity variation is downward-bounded; it no longer generates values above `1` that immediately saturate overlap.

### AH.7 Ground-contact rule

The rejected contact-cell and shared-lower-envelope logic is removed.

Required geometry:

```text
vertexWidth(V) = packedBeamWidth
```

Required opacity:

```text
widthProfile(V = 0) = the same asymmetric individual profile used above contact
```

Ground transition may attenuate intensity toward a nonzero floor, but it must not widen the geometry, erase internal aperture gaps, or substitute one zone-wide profile.

The V1.1D-AD1 full-resolution mask and lower depth-preservation logic remain unless Unity evidence disproves them. This patch changes contact shape, not the separately proven player-Z stability fix.

### AH.8 World-X presentation invariant

The renderer supplies:

```text
GroundContactAxisWorld = (1, 0, 0, 0)
```

The same axis controls:

- packed beam intervals;
- ribbon width direction;
- footprint-diameter diagnostics;
- projected softening direction;
- projected bounds.

Camera movement no longer rotates the beam family.

### AH.9 Secondary softening contract

Softening remains a small finishing tool, not the source of natural variation.

The pass changes to:

- full-resolution five-tap normalized filter;
- no reconstructed-depth weighting inside the filter;
- projected average-width radius bounded to `2–12` pixels;
- halo-only addition:

```text
B = normalized filtered mask
H = max(0, B - M)
S = saturate(M + H * haloGain * strength)
```

This preserves beam interiors and adds only outward feather. The mask already owns foreground occlusion. Removing depth weights avoids the observed discontinuity scratches at maximum strength.

If Raw and Softened remain functionally redundant after this structurally correct beam family, removal remains the next action. The pass receives no further architecture expansion.

### AH.10 Approved changed-file scope

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs
Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayMask.shader
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayScatter.shader
```

No serialized scene, prefab, material, renderer asset, pipeline asset, layer, tag, component, cloud code, lifecycle logic, source profile, renderer feature, or surface-composite algorithm is in scope.

### AH.11 File-by-file implementation sequence

1. Add shared derived aperture-budget and average-width contracts to `WeatherLightRayTypes.cs` without changing serialized descriptor shape.
2. Update controller diagnostics and both custom Inspectors to remove contact-cell wording and report aperture budget, average width, and fixed world-X layout.
3. Replace camera-derived contact-axis construction and softening radius math in `WeatherLightRayRenderPass.cs`; remove the softening pass's unused depth dependency.
4. Replace contact-cell layout with the exact packed partition and asymmetric appearance helpers in `WeatherLightRayCommon.hlsl`.
5. Remove lower width interpolation and shared contact envelope in `SH_WeatherLightRayMask.shader`; use constant packed width and asymmetric density through contact.
6. Replace the seven-tap depth-weighted filter with the bounded five-tap halo-only filter in `SH_WeatherLightRayScatter.shader`.
7. Re-read all reviewed files, compare the final diff to V1.1D-AD1, run static/math/scope audits, and record exact results here.

### AH.12 Acceptance criteria

- Area Diameter still derives one footprint and the same candidate beam count.
- The first beam's left geometric edge equals `-A/2`; the last beam's right geometric edge equals `+A/2`.
- Sum of beam widths plus aperture gaps equals `A` within floating-point tolerance.
- Contact axis and diameter diagnostic are parallel to world X.
- Every beam keeps its own width/profile through contact; there is no lower shared slab.
- Internal gaps remain visible at contact and are small/grouped rather than uniformly tiled.
- Beam widths, whole-beam transmissions, left/right softness, and left/right visibility differ deterministically.
- No beam is cut into disconnected longitudinal segments.
- Player-Z movement does not change authored contact length except genuine foreground occlusion above the protected lower band.
- Softened introduces a clean small halo without the maximum-strength scratches shown after V1.1D-AD1.
- Surface illumination remains one circle and does not gain per-beam lobes.
- No C#, shader, Render Graph, allocation, or cross-subsystem regression is accepted.

### AH.13 Risks and non-goals

Risks:

- a `12%` aperture budget may still require visual adjustment after Unity proof;
- deterministic grouped gaps imitate aperture structure but do not query arbitrary authored overhead meshes;
- full-resolution atmosphere remains a memory/bandwidth risk requiring profiling;
- protected lower depth may still draw over genuine low foreground geometry;
- shader-loop partition calculation adds bounded vertex ALU, maximum `12` beams.

Non-goals:

- procedural spawning;
- arbitrary overhead-occluder ray tracing;
- Moon authoring;
- terrain-height projection per beam;
- new renderer features or materials;
- surface-light redesign;
- post-processing or bloom calibration;
- performance freeze.

### AH.14 Implementation result

`SOURCE IMPLEMENTED — UNITY VALIDATION PENDING`.

The supplied-file implementation follows the recorded sequence without changing the serialized descriptor shape:

- `WeatherLightRayAreaLayout` now owns the fixed `12%` aperture-gap budget and reports average atmospheric width and average gap diagnostics;
- the renderer sends width weights rather than independent absolute widths, fixes `GroundContactAxisWorld` to world X, and derives the secondary halo radius from projected average packed width;
- the softening Render Graph pass no longer declares or samples camera depth;
- common HLSL performs one deterministic normalized aperture partition whose beam-width sum plus gap sum is exactly the authored diameter;
- one primary and, for larger counts, one secondary gap receive bounded deterministic boosts before normalization, creating grouped apertures without deleting a beam;
- every ribbon uses its packed width unchanged from `V = 0` through `V = 1`; the rejected lower interpolation and shared contact envelope are absent;
- per-beam density now has independent left/right softness, stable peak bias, one weaker side, downward-bounded whole-beam transmission, and unchanged continuous longitudinal support;
- width breathing alters apparent density only and cannot move packed interval edges;
- Softened now adds only a five-tap outward halo and cannot replace or brighten beam interiors through a symmetric full-mask blur;
- controller reports and both custom Inspectors expose area-derived count/density pitch, average packed width, average aperture gap, world-X axis, and width-weight semantics.

Intentional differences from V1.1D-AD1:

1. equal contact cells are replaced by unequal normalized beam intervals plus explicit grouped gaps;
2. camera/light-derived horizontal axis is replaced by fixed world X;
3. lower geometry widening and the shared lower opacity envelope are removed;
4. bilateral beam profiles are replaced by signed asymmetric profiles;
5. seven depth-weighted softening taps and `4–32 px` radius are replaced by five mask-only taps and `2–12 px` radius;
6. rejected contact-cell language is removed from active diagnostics while the historical AG evidence remains intact.

Behavior intentionally retained:

- one area diameter still derives footprint radius and candidate beam count;
- one circular surface-illumination field remains independent of atmospheric beam count;
- registration, lifecycle, cloud transmission, source gating, camera eligibility, combined-mesh ownership, full-resolution atmospheric textures, lower contact-depth protection, and final composite logic are unchanged;
- no scene, prefab, material, renderer feature, renderer asset, Render Pipeline asset, layer, tag, component, source profile, cloud implementation, or dependency changed.

### AH.15 Mathematical audit

The deterministic partition was reproduced outside Unity with the same hash, weight, normalization, and sequential-cursor equations.

For variation seed `7319`:

| Diameter | Beam count | Width range | Gap range | First edge | Last edge |
|---:|---:|---:|---:|---:|---:|
| `0.60 m` | `2` | `0.235–0.293 m` | `0.072 m` | `-0.300 m` | `+0.300 m` |
| `1.80 m` | `4` | `0.335–0.453 m` | `0.038–0.131 m` | `-0.900 m` | `+0.900 m` |
| `4.80 m` | `9` | `0.391–0.530 m` | `0.042–0.120 m` | `-2.400 m` | `+2.400 m` |
| `5.40 m` | `10` | `0.382–0.548 m` | `0.042–0.118 m` | `-2.700 m` | `+2.700 m` |
| `6.60 m` | `12` | `0.396–0.569 m` | `0.041–0.141 m` | `-3.300 m` | `+3.300 m` |

A stress audit evaluated `202,407` seed/diameter cases across the supported diameter range. Results:

```text
nonpositive beam widths: 0
nonpositive aperture gaps: 0
maximum endpoint error: 2.220446049250313e-15 m
minimum observed beam width: 0.12124561222466981 m
minimum observed gap: 0.015706466679534542 m
```

The measured endpoint error is floating-point accumulation noise; no tested partition violated `sum(widths) + sum(gaps) = A` or the exact endpoint invariant within the stated tolerance.

### AH.16 Supplied-file and compliance audit

The post-implementation audit passed the checks available in this workspace:

- exact changed-file scope: `9/9` approved files;
- immutable descriptor constructor and authored producer: `35/35` arguments;
- custom Anchor Inspector active serialized-property resolution: `35/35`, with no active field omitted;
- common-HLSL uniforms and CPU property uploads: `16/16` aligned;
- shared include consumers: exactly the intended Mask, Scatter, and Composite shaders;
- delimiter, NUL-byte, UTF-8/final-newline checks: passed for all eight changed source/shader files;
- active LightRay runtime contains no contact-cell layout, shared lower contact envelope, lower width interpolation, seven-tap softener, depth-weighted softening sample, or camera-relative contact-axis calculation;
- no new managed collection or per-frame geometry allocation was introduced;
- the maximum combined mesh remains `48` vertices and `72` indices for `12` beams;
- the bounded vertex partition work is at most two twelve-entry loops per vertex; no fragment-stage partition loop was introduced;
- V1.1D-AD1 was used as the supplied pre-edit baseline and every intentional behavioral difference is listed in AH.14.

Cross-subsystem audit: `WeatherLightRayCommon.hlsl` is consumed only by the three private LightRay shaders, so the shared-include edit cannot affect Ground, River, Vegetation, Generated Mass, Tree, cloud-cookie, or other project shaders directly. Unity shader compilation remains required to prove backend acceptance.

### AH.17 Pending validation

Unity 6000.5.0f1, D3D11/D3D12 compilation, Render Graph execution, gameplay-camera output, Frame Debugger, GPU timing, and actual temporary allocation are unavailable in this workspace. They remain pending and must not be represented as passed.

The focused Unity proof must determine:

1. whether the packed world-X beam geometry visually spans the intended footprint diameter;
2. whether every individual profile remains separate through contact with no white lower slab;
3. whether grouped gaps and asymmetric widths/transmissions resemble aperture-driven sunlight rather than a regular barcode;
4. whether the five-tap halo is clean at maximum strength and materially useful at normal strength;
5. whether retained lower contact-depth protection preserves player-Z stability without drawing over genuine foreground objects;
6. whether full-resolution atmospheric bandwidth is acceptable at `2560 x 1440`.

## Next work items

- Run the focused V1.1D-AE Unity structural and visual proof and correct only evidence-backed compilation, partition, contact, asymmetry, or halo failures.
- After structural acceptance, calibrate warmth, atmospheric subtlety, surface-light balance, and profile the authored proof before procedural placement is enabled.

### AH.18 V1.1D-AE1 shader-include compilation hotfix

Unity D3D11 compilation exposed one shared-include ownership error and one loop-variable warning immediately after the V1.1D-AE source patch:

```text
Hidden/PS3D/Weather LightRay Scatter:
undeclared identifier 'SampleSceneDepth'
WeatherLightRayCommon.hlsl(350)

Hidden/PS3D/Weather LightRay Scatter:
loop control variable 'index' conflicts with a previous declaration
WeatherLightRayCommon.hlsl(197)
```

Root cause:

- V1.1D-AE intentionally removed scene-depth declaration and sampling from the Scatter pass because softening is now mask-only.
- `WeatherLightRayCommon.hlsl` still compiled the depth-dependent footprint-marker and surface-influence helper definitions for every include consumer.
- HLSL resolves function bodies during compilation even when the Scatter pass never calls those helpers, so the undeclared `SampleSceneDepth` reference remained a compile error.
- Two sequential unrolled layout loops reused the identifier `index`; the D3D11 compiler retained the first loop variable in the enclosing scope and warned that the second declaration shadowed it.

Correction:

- depth-dependent marker and surface-evaluation helpers are compiled only when `WEATHER_LIGHT_RAY_ENABLE_DEPTH_EVALUATION` is defined;
- the Composite shader defines that symbol immediately after including `DeclareDepthTexture.hlsl` and before including the shared LightRay HLSL;
- the Scatter shader does not define the symbol, so its preprocessed shared include contains zero `SampleSceneDepth` references and remains a genuinely depth-independent softening pass;
- the two layout loops now use distinct `weightIndex` and `precedingIndex` identifiers.

This hotfix changes no beam partition, profile, contact, softening, surface-light, lifecycle, cloud, or composite mathematics. It only restores correct shader dependency boundaries and removes the compiler warning.

Static preprocessor proof:

```text
Scatter shared-include variant:
SampleSceneDepth references: 0
WeatherLightRayEvaluateSurfaceInfluence definitions: 0
brace balance: 0

Composite shared-include variant:
SampleSceneDepth references: 2
WeatherLightRayEvaluateSurfaceInfluence definitions: 1
brace balance: 0
```

Unity D3D11/D3D12 recompilation remains required before V1.1D-AE visual validation resumes.

## V1.1D-AF — Dense overlapping asymmetric shaft correction

Status: `SOURCE IMPLEMENTED — UNITY VALIDATION PENDING`.

### AF.1 Triggering evidence and rejected V1.1D-AE assumptions

Unity feedback rejected the V1.1D-AE atmospheric layout immediately:

- visible black gaps remained between nearly every neighbouring shaft;
- individual shafts still read as bilaterally softened bars rather than directional atmospheric density;
- the result remained too regular and too bright relative to the accepted well reference;
- the accepted world-X contact axis and area-derived beam count remain valid and must not regress.

The code review found two direct causes:

1. `WeatherLightRayAreaLayout.ApertureGapBudgetFraction = 0.12` explicitly reserved twelve percent of every authored area for empty geometric gaps. `WeatherLightRayGetBeamLayout()` then partitioned that budget between every neighbouring pair, guaranteeing the rejected barcode separation.
2. `WeatherLightRayGetBeamVariation()` stored different left/right softness values, but the mask still built both sides from one centred normalized distance and only weakly attenuated one side. At the serialized defaults (`Beam Edge Softness = 0.55`, `Beam Softness Variation = 0.35`, `Beam Intensity Variation = 0.18`), the asymmetry was too small to be visually structural.

Rejected from active architecture:

- any positive mandatory empty-gap budget between every beam;
- aperture grouping represented primarily by black geometric space;
- random left/right parameters that are mathematically present but visually negligible;
- relying on the screen-space softener to manufacture the reference structure;
- returning to a shared lower slab, contact-cell widening, camera-relative contact line, or independent footprint/count controls.

### AF.2 Retained invariants

The following V1.1D-AD/AE decisions remain authoritative:

- one authored Area Diameter controls footprint radius and derived beam count;
- one shared circular surface-light footprint remains independent of atmospheric beam count;
- the contact-layout axis is exactly world X: `G = (1, 0, 0)`;
- beam axes remain parallel to the same source direction;
- every beam keeps one constant geometric width from top through contact;
- first and last visible outer geometry edges must remain exactly `-A/2` and `+A/2`;
- per-beam appearance may vary, but centres and geometric contact edges do not drift;
- the post softener remains secondary and may not create the shaft family.

### AF.3 Dense-overlap layout mathematics

Let:

- `A` be the authored area diameter;
- `N` be the existing area-derived beam count;
- `q_i > 0` be the deterministic raw width weight for beam `i`;
- `a_i` be the deterministic adjacent-overlap ratio for pair `i, i+1`.

No geometric gap is permitted. Adjacent intervals overlap by construction:

```text
0.28 <= a_i <= 0.50
```

One or two deterministic group boundaries may use a bounded larger overlap, clamped to `0.60`. This creates denser shaft groups rather than empty holes. The initial `0.20–0.42` candidate was rejected during the pre-implementation numerical profile audit because the weakest default-seed overlap valley fell to approximately `0.067` mask transmission; the strengthened range raises that default-seed minimum overlap-valley transmission above `0.11` before longitudinal and lifecycle modulation.

The unscaled overlap between adjacent raw widths is:

```text
h_i = min(q_i, q_(i+1)) * a_i
```

The raw covered span is:

```text
S = sum(q_i) - sum(h_i)
```

Because every `a_i <= 0.60` and every `h_i < min(q_i, q_(i+1))`, `S` remains strictly positive. Scale all widths and overlaps by:

```text
k = A / S
w_i = k * q_i
o_i = k * h_i
```

Place intervals sequentially:

```text
L_0 = -A/2
R_i = L_i + w_i
L_(i+1) = R_i - o_i
C_i = (L_i + R_i) / 2
```

Proof:

```text
R_(N-1)
= -A/2 + sum(w_i) - sum(o_i)
= -A/2 + kS
= +A/2
```

For every adjacent pair:

```text
L_(i+1) = R_i - o_i < R_i
```

Therefore:

- no pair contains a geometric gap;
- every pair has positive overlap;
- the first and last outer edges remain exact;
- width variation and grouping do not change the footprint span.

### AF.4 Guaranteed asymmetric beam profiles

For local beam coordinate `u in [0,1]`, each beam receives one deterministic profile mode. The seed offsets the mode assignment, but the beam index guarantees a repeating mixture rather than relying on chance:

1. hard-left / soft-right;
2. soft-left / hard-right;
3. unequal bilateral softness;
4. directional transmission gradient with a displaced density peak.

Each mode produces independent feather widths `f_L`, `f_R`, peak coordinate `p`, and transmissions `t_L`, `t_R`.

The edge support is:

```text
E_L(u) = smoothstep(0, f_L, u)
E_R(u) = 1 - smoothstep(1 - f_R, 1, u)
E(u)   = E_L(u) * E_R(u)
```

The displaced core uses separate normalized distances on each side of `p`:

```text
d(u) = (p - u) / p               when u < p
     = (u - p) / (1 - p)         otherwise
C(u) = exp2(-d(u)^2 * c)
```

The directional transmission is:

```text
T(u) = lerp(t_L, t_R, u)
```

Final cross-width density:

```text
P(u) = saturate(E(u) * C(u) * T(u))
```

The default control values must yield visibly different left/right edge widths and transmissions. The implementation must not merely perturb two nearly identical symmetric parameters.

### AF.5 Intensity hierarchy

Atmospheric shafts must remain subordinate to surface illumination. Per-beam mask transmission is bounded below full white even before the final Atmospheric Intensity multiplier:

```text
0.25 <= beam transmission <= 0.82
```

`Beam Intensity Variation = 0` still produces one uniform authored transmission. Increasing the control expands the deterministic range downward and upward within the above bounds. Overlap uses bounded opacity union, so dense groups become readable without unbounded additive white slabs.

### AF.6 Secondary softening contract

The screen-space pass remains a small halo tool only:

- it may broaden existing asymmetric raw edges;
- it may not fill the entire overlap field into one slab;
- it may not introduce depth scratches;
- it may not be the source of left/right asymmetry.

The filter will use the stronger neighbouring source side at each pixel and add only the positive exterior halo. This preserves the asymmetric raw profile better than a centred symmetric blur. Radius remains full-resolution and bounded.

### AF.7 Approved file scope and sequence

Approved modifications:

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs
Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayMask.shader
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayScatter.shader
```

Implementation sequence:

1. replace gap-budget constants and diagnostics with bounded adjacent-overlap policy;
2. replace HLSL gap partition with exact dense-overlap partition;
3. replace probabilistically weak profile variation with guaranteed deterministic profile modes;
4. update the mask to evaluate independent left/right feather, displaced core, and directional transmission;
5. reduce mask transmission range so the final atmosphere cannot read as eye-searing spotlights;
6. make the post softener edge-directed and secondary;
7. audit exact endpoint span, positive overlaps, profile diversity, uniforms, shader consumers, allocation behavior, and changed-file scope.

### AF.8 Acceptance criteria

Source/math acceptance:

- `R_(N-1) - L_0 = A` within floating-point tolerance;
- every `o_i > 0` and every `L_(i+1) < R_i`;
- no active gap-budget symbol or gap-diagnostic wording remains;
- all four asymmetric profile modes occur for populations of at least four beams;
- no beam geometry changes width with height;
- world-X contact axis remains unchanged;
- no new per-frame allocation or additional draw call is introduced.

Unity acceptance:

- Raw shows no black mandatory separator between every pair;
- width, opacity, peak position, and left/right edge softness visibly differ between shafts;
- beams remain individually readable through contact without a lower slab;
- atmosphere is subordinate to the single surface-light footprint;
- Softened adds only a modest clean halo and preserves the asymmetry already visible in Raw;
- moving the followed player on Z does not alter authored layout or beam length except for genuine foreground occlusion.

### AF.9 Risks and non-goals

Risks:

- excessive overlap can visually collapse into a broad ribbon if per-beam transmission remains too high;
- full-resolution mask bandwidth remains pending profiling;
- deterministic profile modes imitate aperture variation but do not ray trace arbitrary overhead meshes;
- lower contact-depth protection remains unchanged and still requires foreground validation.

Non-goals:

- procedural spawning;
- overhead-mesh aperture tracing;
- terrain-height projection per beam;
- surface-footprint redesign;
- new serialized controls or changed scene/prefab values;
- renderer-feature or material changes;
- performance freeze.

### AF.10 Implementation result

`SOURCE IMPLEMENTED — UNITY VALIDATION PENDING`.

The implementation follows the AF plan rather than the rejected V1.1D-AE gap partition:

- the active `12%` empty-gap budget and all active average-gap diagnostics are removed;
- adjacent beam geometry now overlaps by construction, while the first and last outer edges remain exactly at the authored diameter endpoints;
- deterministic width weights remain unequal, but normalization is performed against `sum(widths) - sum(overlaps)` rather than `sum(widths) + sum(gaps)`;
- one or two deterministic pair boundaries receive stronger overlap, producing denser groups rather than holes;
- four profile modes are assigned by beam ordinal with a seed offset, guaranteeing hard-left/soft-right, soft-left/hard-right, unequal bilateral, and directional-gradient profiles for populations of at least four beams;
- the mask evaluates separate left and right feathers, a displaced core, and side-to-side transmission instead of one centred absolute-distance profile;
- whole-beam transmission is downward-bounded before the authored Atmospheric Intensity multiplier so dense overlap cannot become an unbounded white slab;
- the screen-space stage is now an edge-directed secondary halo with a `1.5–8 px` radius, not the structural source of asymmetry;
- world-X contact orientation, one shared surface footprint, constant beam width through contact, full-resolution atmosphere, lifecycle, cloud policy, and source gating remain unchanged.

No serialized field, scene, prefab, material, renderer feature, renderer asset, Render Pipeline asset, layer, tag, component, source profile, or cloud-query implementation changed.

### AF.11 Mathematical audit

The final dense-overlap equations were reproduced outside Unity using the same hash, width-weight, overlap-ratio, normalization, and cursor logic.

Default authored proof (`Area Diameter = 4.80 m`, variation seed `7319`, nine derived beams):

```text
beam width range:       0.558–0.974 m
adjacent overlap range: 0.189–0.468 m
first outer edge:       -2.400 m
last outer edge:        +2.400 m
profile modes:          1,2,3,0,1,2,3,0,1
```

The default-seed combined bounded-union mask has no mandatory zero separator between adjacent intervals. Before longitudinal/lifecycle modulation:

```text
weakest internal overlap valley: approximately 0.112
maximum combined cross-section:   approximately 0.744
```

A stress audit evaluated `120,000` seed/diameter cases (`10,000` seeds across twelve representative diameters):

```text
invalid/nonpositive layouts: 0
maximum endpoint error:       3.552713678800501e-15 m
minimum positive overlap:     0.051093700273954164 m
minimum positive beam width:  0.16265984865615013 m
```

The endpoint error is floating-point accumulation noise. Every tested neighbouring pair satisfied `L_(i+1) < R_i`; no tested pair contained a geometric gap.

At the default controls, the guaranteed profile family produced left/right feather pairs ranging approximately from `0.092/0.294` to `0.294/0.092`, side transmissions ranging approximately from `0.40` to `0.98`, and per-beam mask transmissions ranging approximately from `0.34` to `0.67`. These are structural asymmetries, not near-identical random perturbations.

### AF.12 Supplied-file and compliance audit

Available static checks passed:

- exact approved changed-file scope: `9/9`;
- descriptor constructor and authored producer: `35/35` arguments aligned;
- active Anchor Inspector properties: `35/35`; only the seven documented legacy serialized fields remain intentionally hidden;
- common-HLSL uniforms and CPU property IDs: `16/16` aligned;
- common include consumers: exactly the intended Mask, Scatter, and Composite shaders;
- HLSL function parameter names: no duplicate parameter declarations;
- source/shader delimiter, NUL-byte, UTF-8, final-newline, conflict-marker, and diff-whitespace checks: passed;
- no active aperture-gap-budget symbol or average-gap runtime diagnostic remains;
- no new managed collection, per-frame geometry allocation, draw call, render texture, or dependency was introduced;
- combined mesh maximum remains `48` vertices and `72` indices for twelve beams;
- Scatter remains depth-independent; depth-dependent helpers are still compiled only for Composite.

Unity C# compilation, D3D11/D3D12 shader compilation, Render Graph execution, visual proof, player-Z stability, and GPU timing are unavailable in this workspace and remain explicit blockers. The patch is not accepted or frozen.

## Next work items

- Run the focused V1.1D-AF Unity proof and correct only evidence-backed compilation, dense-overlap, asymmetry, contact, or secondary-halo failures.
- After AF structural acceptance, perform V1.1D-AG atmospheric/surface calibration and 1440p profiling before procedural placement is enabled.

---

## V1.1D-AF1 — Restrained authored intensity defaults

Status: `SOURCE IMPLEMENTED — UNITY VALIDATION PENDING`.

### AF1.1 Evidence and reason

The first accepted-looking AF atmospheric structure still rendered with the legacy proof defaults:

```text
Atmospheric Intensity:       0.28
Ground Light Intensity:      0.42
Visible Surface Intensity:   0.28
Cloud Compensation:          0.45
Core Emphasis:               0.20
```

These values were appropriate only for early high-contrast diagnostics. They are not sensible presentation defaults. In particular, the surface path evaluates:

```text
surface = zoneInfluence * (receiverIntensity + 0.35 * cloudCompensation)
zoneInfluence centre scale = 1 + CoreEmphasis
```

For a cloud-ignoring ray on a ground-facing receiver, the legacy maximum centre lift was therefore approximately:

```text
(0.42 + 0.35 * 0.45) * 1.20
= 0.693
```

That can add approximately 69.3% of the ray colour at the footprint centre before scene-specific colour and exposure interactions. The user-observed footprint consequently read as an eye-catching white lamp rather than slight environmental illumination.

### AF1.2 New defaults

The authoritative `WeatherLightRayAnchor` defaults are now:

```text
Atmospheric Intensity:       0.12
Ground Light Intensity:      0.05
Visible Surface Intensity:   0.08
Cloud Compensation:          0.05
```

The new cloud-ignoring ground-centre maximum is approximately:

```text
(0.05 + 0.35 * 0.05) * 1.20
= 0.081
```

This is an approximately 88.3% reduction from the legacy maximum surface lift. Atmospheric intensity is reduced by approximately 57.1%. These are deliberately conservative defaults intended to establish subtle visibility before author tuning.

### AF1.3 Safe migration rule

Changing C# field initializers alone does not modify already serialized scene or prefab instances. `WeatherLightRayAnchor` therefore performs one narrow migration:

- migrate only when all four intensity values still equal the exact legacy default quartet;
- replace that quartet with the new restrained defaults;
- do not modify an anchor if any one of the four values has been manually changed.

The migration runs before registration in `OnEnable` and before validation clamping in `OnValidate`, so the current authored proof receives the new defaults without a scene-file patch while manually tuned content remains untouched.

### AF1.4 Scope

Changed files:

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs
```

No renderer, shader, descriptor contract, source profile, scene, prefab, material, cloud implementation, lifecycle rule, beam layout, or surface-footprint mathematics changed.


---

## V1.1D-AF2 — Drastically restrained surface-light defaults and reliable migration

Status: `SOURCE IMPLEMENTED — UNITY VALIDATION PENDING`.

### AF2.1 Observed failure

The AF1 proof remained visually almost unchanged after the intended default reduction. The atmospheric shafts were only slightly too strong, while the shared ground footprint remained approximately three times stronger than the desired subtle environmental lift.

The AF1 defaults were:

```text
Atmospheric Intensity:       0.12
Ground Light Intensity:      0.05
Visible Surface Intensity:   0.08
Cloud Compensation:          0.05
```

The active surface equation remains:

```text
surface = zoneInfluence * (receiverIntensity + 0.35 * cloudCompensation)
zoneInfluence centre scale = 1 + CoreEmphasis
```

At `Core Emphasis = 0.20`, the AF1 maximum cloud-ignoring ground-centre lift was:

```text
(0.05 + 0.35 * 0.05) * 1.20
= 0.081
```

The observed footprint is still much too prominent. A target near one third of that lift is therefore justified by the current visual evidence.

### AF2.2 Migration failure finding

AF1 migrated only when all four serialized values simultaneously matched the complete legacy quartet. If any one value had previously been edited, the entire migration was skipped. This means an otherwise untouched Ground Light value could remain at `0.42`, and explains why a defaults-only patch could appear visually unchanged.

This is a high-confidence inference from:

- the exact all-four conjunction in `WeatherLightRayAnchor.MigrateLegacyIntensityDefaults()`;
- the user-observed near-identical result after AF1;
- Unity serialization retaining existing component values instead of replacing them with new field initializers.

The replacement migration must operate per field and only recognize known historical default values. It must also run once through an explicit hidden migration version so authors can later intentionally set any numerical value without it being repeatedly rewritten.

### AF2.3 Approved defaults

```text
Atmospheric Intensity:       0.09
Ground Light Intensity:      0.015
Visible Surface Intensity:   0.025
Cloud Compensation:          0.01
```

The new maximum cloud-ignoring ground-centre lift is:

```text
(0.015 + 0.35 * 0.01) * 1.20
= 0.0222
```

This is:

```text
0.0222 / 0.081 = 0.2741
```

or approximately a `72.6%` reduction from AF1, leaving about `27.4%` of its previous maximum centre lift. Atmospheric intensity is reduced only `25%`, matching the observation that the shafts themselves require a modest rather than drastic reduction.

### AF2.4 Migration contract

Add one hidden serialized migration version. For components predating AF2:

- Atmospheric Intensity migrates from either `0.28` or `0.12` to `0.09`.
- Ground Light Intensity migrates from either `0.42` or `0.05` to `0.015`.
- Visible Surface Intensity migrates from either `0.28` or `0.08` to `0.025`.
- Cloud Compensation migrates from either `0.45` or `0.05` to `0.01`.
- Any value not equal to a known historical default remains unchanged.
- After this one pass, the migration version is advanced and no later `OnValidate` call may reinterpret intentionally authored values.

This deliberately trades the inability to distinguish an author who manually chose exactly a historical default from an untouched default. The current authored proof must be updated reliably, and exact historical-default values are the narrowest evidence-backed migration set.

### AF2.5 Scope and sequence

Approved files:

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs
```

Sequence:

1. Record the observed failure, equations, defaults, migration issue, and acceptance criteria here.
2. Replace AF1 constants with AF2 constants.
3. Replace all-four migration with versioned per-field migration.
4. Audit the complete Anchor producer and downstream descriptor/surface equation for unchanged behavior.
5. Package only the two approved files.

Non-goals:

- no shader or renderer changes;
- no footprint-radius or falloff-shape changes;
- no beam-layout changes;
- no scene or prefab raw edit;
- no change to Core Emphasis;
- no attempt to calibrate final values beyond establishing restrained defaults.

### AF2.6 Acceptance criteria

- New and known-default existing anchors expose `0.09 / 0.015 / 0.025 / 0.01`.
- The footprint is visibly far weaker than AF1 and no longer dominates the shafts.
- The shafts remain readable but slightly less intense.
- Values that do not equal a known historical default remain untouched.
- No renderer, shader, descriptor, scene, prefab, material, cloud, or lifecycle behavior changes.


### AF2.7 Implementation result

Implemented exactly the approved two-file scope:

- `WeatherLightRayAnchor` defaults are now `0.09 / 0.015 / 0.025 / 0.01`.
- A hidden `intensityDefaultsVersion` gates migration after one successful pass.
- Migration is per field rather than one all-or-nothing quartet.
- Each field recognizes both the original diagnostic default and the AF1 default.
- Values outside those exact known-default sets remain unchanged.
- `OnEnable` migrates before controller registration; `OnValidate` migrates before clamping and refresh.

The descriptor producer still forwards the four serialized values unchanged to `WeatherLightRayDescriptor`. The renderer still uploads them through `SurfaceParameters0`, and the surface shader equation is unchanged.

### AF2.8 Static audit

Available source checks passed:

```text
Changed-file scope:                  2 / 2
Descriptor constructor arguments:    unchanged
Renderer/shader files changed:       0
Known atmospheric migrations:        0.28, 0.12 -> 0.09
Known ground migrations:             0.42, 0.05 -> 0.015
Known visible-surface migrations:    0.28, 0.08 -> 0.025
Known cloud migrations:              0.45, 0.05 -> 0.01
Unknown authored sample preservation: PASS
Delimiter/conflict/whitespace checks: PASS
```

Worked maximum ground-centre lift:

```text
(0.015 + 0.35 * 0.01) * 1.20
= 0.0222
```

Unity C# compilation, serialization persistence, Inspector migration, and final visual calibration remain pending. The patch is not accepted or frozen until those checks pass.

---

## V1.1D-AF3 — Unified bounded surface illumination controls

Status: `SOURCE IMPLEMENTED — UNITY VALIDATION PENDING`.

### AF3.1 Triggering Unity evidence

The current AF2 proof exposes five active surface controls:

```text
Ground Light Intensity
Visible Object Light Intensity
Cloud Compensation Intensity
Footprint Edge Softness
Footprint Core Emphasis
```

The supplied Inspector capture shows a representative authored state of approximately:

```text
Ground Light Intensity:        0.02
Visible Object Light Intensity: 0.00
Cloud Compensation Intensity:  0.20
Footprint Edge Softness:       0.10
Footprint Core Emphasis:       0.67
```

The final result remains an opaque-looking cream overlay that suppresses the Ground material. The surface response is still excessive even when the nominal Ground Light value is near zero. This is explained directly by the current equation:

```text
surfaceInfluence =
    radial
    * lifecycleIntensity
    * (1 + centreWeight * CoreEmphasis)
    * (receiverIntensity + 0.35 * CloudCompensation)
```

For the captured centre case:

```text
(0.02 + 0.35 * 0.20) * (1 + 0.67)
= 0.09 * 1.67
= 0.1503
```

The nominal `0.02` Ground Light setting therefore produces a possible centre influence of approximately `0.15` before the HDR ray colour is additively applied. The control does not own the visible result; two other controls can dominate it.

The current composite then performs:

```text
final = scene + HDRRayColour * surfaceInfluence
```

That is a colour overlay, not a bounded lighting response. HDR ray colour can replace local material identity and can saturate the receiving surface at high settings. The user has rejected any configuration in which the illuminated footprint can become a flat cream disc.

The current edge equation is also one-sided:

```text
innerRadius = R * lerp(0.86, 0.24, softness)
radial = 1 - smoothstep(innerRadius, R, distance)
```

Increasing softness moves the full-strength boundary inward while keeping the zero boundary at `R`. The apparent lit area shrinks instead of feathering around the authored beam diameter.

### AF3.2 Approved active authoring contract

The surface section exposes exactly two active controls:

```text
Surface Illumination Intensity   [0, 1]
Surface Edge Softness            [0, 1]
```

No active Ground/Object split, Cloud Compensation, or Core Emphasis remains.

`Surface Illumination Intensity` has exact endpoint semantics:

```text
I = 0 -> no surface-light contribution
I = 1 -> full bounded authored response
```

The same intensity applies to every visible opaque receiver inside the one zone-owned footprint. Receiver type does not change the authored strength.

`Surface Edge Softness` has exact endpoint semantics:

```text
S = 0 -> hard footprint edge at radius R
S = 1 -> maximum bidirectional feather centred on radius R
```

The footprint radius remains the beam-family alignment radius. Softness may not move the 50% contour away from that radius.

### AF3.3 Bidirectional footprint mathematics

Let:

```text
R = authored footprint radius derived from Light Ray Area Diameter
D = horizontal distance from visible world position to footprint centre
S = saturate(Surface Edge Softness)
H = 0.35 * R * S
```

For `S = 0`:

```text
radial(D) = 1 when D < R
radial(D) = 0 when D >= R
```

For `S > 0`:

```text
radial(D) = 1 - smoothstep(R - H, R + H, D)
```

Properties:

```text
radial(R) = 0.5
full-strength radius = R - H
zero-influence radius = R + H
```

At maximum softness:

```text
H = 0.35R
transition = [0.65R, 1.35R]
```

The transition therefore expands both inward and outward. The 50% edge remains aligned with the first/last beam envelope at `R`; softness cannot visually shrink the nominal footprint.

### AF3.4 Bounded albedo-preserving lighting mathematics

The full-resolution composite evaluates:

```text
T = saturate(lifecycleIntensity * radial * SurfaceIlluminationIntensity)
```

The composite must not add raw HDR ray colour. It derives a chromaticity-only Sun tint:

```text
C = max(0, HDRRayColour)
Cunit = C / max(max(C.r, C.g, C.b), epsilon)
```

For the current scene colour `B`, use a bounded screen-light target with a fixed full-power response `K = 0.28`:

```text
Bsafe  = saturate(B)
Target = 1 - (1 - Bsafe) * (1 - K * Cunit)
Lift   = max(0, Target - Bsafe)
Result = B + T * Lift
```

Per channel:

```text
0 <= Lift <= K * (1 - Bsafe)
```

Therefore even at `T = 1`:

- the response cannot replace the receiver with raw Sun colour;
- existing texture and albedo differences remain visible;
- already-HDR scene channels are not pushed further by this surface pass;
- the slider remains linear and predictable over `[0, 1]`.

The maximum full-power screen response `K = 0.28` is fixed implementation calibration, not another author control.

### AF3.5 Full-resolution edge ownership

The pre-AF3 surface field is generated at quarter resolution and then reconstructed through four linearly filtered surface samples plus depth weighting in the full-resolution composite. That pipeline necessarily softens `S = 0` and can produce a coarse edge even when the radial equation is a hard step. It cannot satisfy the approved exact endpoint semantics.

AF3 removes the private quarter-resolution surface texture and its dedicated Render Graph pass. `WeatherLightRayEvaluateSurfaceInfluence(screenUV)` is evaluated directly in the existing full-resolution composite. The function exits before depth reconstruction outside the conservative surface bounds, so full world-position reconstruction is limited to the affected screen rectangle.

This change:

- makes `S = 0` a true full-resolution hard radial threshold;
- makes the Surface debug view and Final Composite consume the identical scalar without an upsample path;
- removes one `R16_SFloat` texture, one raster pass, four surface-texture samples, and the former per-sample depth-weighting loop;
- adds one bounded depth reconstruction per full-resolution pixel inside the surface bounds.

The net GPU result remains unverified until Unity profiling, but the change removes the known filtering contradiction and does not add memory or draw calls.

### AF3.6 Serialization and migration

Add one active serialized field:

```text
surfaceIlluminationIntensity = 0.20
```

Retain the four rejected serialized values only as hidden legacy data so existing scenes/prefabs deserialize safely:

```text
groundLightIntensity
surfaceLightIntensity
cloudCompensationIntensity
coreEmphasis
```

They no longer enter the descriptor or shader equation.

A hidden one-time surface-control migration version initializes every pre-AF3 anchor to the safe new `0.20` active intensity. There is no meaningful one-to-one conversion from the rejected four-control equation because Cloud Compensation and Core Emphasis could dominate the nominal intensity independently. After migration, later authored values are preserved.

Existing `edgeSoftness` serialization is retained, but its range becomes `[0, 1]` and its runtime meaning changes from inward-only shrinkage to the approved bidirectional transition.

### AF3.7 Approved file scope

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs
Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs
Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayComposite.shader
```

No beam layout, atmosphere mask, softening shader, cloud query, lifecycle, registration, source profile, scene, prefab, material, renderer asset, layer, tag, component, or dependency change is approved.

### AF3.8 File-by-file implementation sequence

1. Replace the descriptor surface contract with one intensity plus edge softness.
2. Add the active Anchor intensity, hide legacy controls, allow zero edge softness, and add one-time migration.
3. Reduce the Anchor Inspector surface foldout to exactly two controls with explicit endpoint descriptions.
4. Update the controller read-only diagnostic so it reports the same two active surface values rather than the rejected Ground/Object split.
5. Upload only active intensity, softness, and derived footprint geometry to the surface shader.
6. Replace receiver classification, cloud compensation, and core emphasis with the bidirectional radial equation.
7. Remove the quarter-resolution surface texture/pass and evaluate the bounded surface scalar directly in the full-resolution composite.
8. Replace raw HDR additive colour with bounded albedo-preserving screen-light response.
9. Update controller diagnostics to report the two active surface controls.
10. Re-read the complete changed surface and descriptor path, compare the final diff to this scope, and run available static/mathematical checks.

### AF3.9 Acceptance criteria

- Inspector surface foldout contains exactly one intensity and one softness control.
- Intensity `0` is pixel-identical to surface illumination disabled.
- Intensity `1` produces a strong but material-preserving lift; grass, wood, rock, and other receiver identity remain visible.
- Cloud policy cannot amplify the authored surface intensity.
- Ground-facing and object-facing receivers use the same authored intensity.
- Softness `0` produces a hard edge at the derived radius.
- Softness `1` feathers both inward and outward, with the 50% contour still at the derived radius.
- Surface debug and final composite use the same footprint scalar.
- Atmospheric rendering is unchanged.

### AF3.10 Non-goals

- no atmospheric intensity or beam-profile calibration;
- no footprint shape change from circle to ellipse;
- no receiver normals or physically based relighting;
- no shadows cast by the surface footprint;
- no procedural placement;
- no profiling/freeze claim.

### AF3.11 Implementation result

Implemented exactly the approved nine-file surface scope.

Active authoring is now:

```text
Surface Illumination Intensity   [0, 1], default 0.20
Surface Edge Softness            [0, 1], retained serialized default 0.42
```

The rejected Ground/Object split, Cloud Compensation, and Core Emphasis fields remain hidden legacy serialization only. They no longer enter `WeatherLightRayDescriptor`, CPU shader parameters, HLSL influence, controller reports, or either Inspector.

The descriptor now carries only:

```text
SurfaceIlluminationIntensity
FootprintEdgeSoftness
```

The renderer uploads:

```text
SurfaceParameters0.x = SurfaceIlluminationIntensity
SurfaceParameters0.y = FootprintEdgeSoftness
SurfaceParameters1.x = derived FootprintRadius
```

The surface scalar is evaluated directly in the full-resolution Composite shader. The private `_WeatherLightRaySurfaceTexture`, quarter-resolution descriptor, dedicated Surface pass, four-sample surface reconstruction, and depth-weight helper were removed.

The final composite no longer adds raw HDR Sun colour to receivers. It normalizes ray chromaticity and applies the fixed `K = 0.28` bounded screen-light target. Atmospheric contribution is unchanged and remains independently controlled by Atmospheric Intensity.

A one-time hidden `surfaceControlsVersion` initializes all pre-AF3 anchors to the safe active intensity `0.20`. Later authored values are not rewritten. Existing `edgeSoftness` serialization is preserved, but `0` is now valid and its runtime interpretation is the bidirectional profile documented above.

### AF3.12 Final source and mathematical audit

Available checks passed:

```text
Changed-file scope:                         9 / 9 exact
Descriptor constructor/call alignment:      32 / 32
Anchor surface controls exposed:            2 / 2
Rejected descriptor surface identifiers:    0 active references
Private surface texture references:          0
Dedicated surface-pass references:           0
Raw/softened atmospheric files changed:      0
Conflict markers / NUL / trailing space:     PASS
Final newline / UTF-8 checks:                PASS
```

Radial endpoint proof:

```text
S = 0: D < R -> 1, D >= R -> 0
S > 0: radial(R - H) = 1
       radial(R)     = 0.5
       radial(R + H) = 0
H = 0.35 * R * S
```

The bounded response was stress-tested over `100,000` random colour samples:

```text
maximum Lift-bound violation: 0
minimum full-power per-channel contrast derivative: approximately 0.72
```

The derivative follows directly from:

```text
Target(B) = B + K * Cunit * (1 - B)
dTarget/dB = 1 - K * Cunit >= 1 - 0.28 = 0.72
```

At full surface power, at least `72%` of local per-channel source contrast remains before later camera tone mapping. This is the mathematical guard against the rejected flat cream decal result.

The bracket-count warning produced by the lightweight source scanner for `WeatherLightRayController.cs` is unchanged from the supplied pre-AF3 file and is caused by scanner handling of existing source text; direct before/after delimiter counts are identical. All actually edited delimiter sets balance.

### AF3.13 Pending Unity proof

The patch is not accepted or frozen. Unity 6000.5.0f1 must still verify:

- C# and D3D shader compilation;
- Render Graph execution after removal of the Surface pass and texture;
- exact zero-intensity invisibility;
- material identity at intensity `1`;
- hard-edge behavior at softness `0`;
- bidirectional feather and radius alignment at softness `1`;
- GPU cost of direct in-composite depth reconstruction inside projected bounds.

## Next work items

- Run the focused AF3 two-control surface proof and correct only evidence-backed control, edge, material-preservation, compilation, or Render Graph failures.
- Resume atmospheric calibration and profiling only after AF3 surface behavior is accepted.

## V1.1D-AF4 — Hybrid real Spot Light surface-response proof

### AF4.1 Status before source edits

**Gate state:** review and mathematical planning complete; runtime source edits have not started at the time this ledger is written.

**Triggering Unity evidence:** user-supplied AF3 Final Composite captures show that the full-resolution post-composite surface contribution still behaves as a translucent colour overlay. Vegetation inside the footprint changes final RGB but does not execute the project vegetation punctual-light response, including the directional blade body response and stylized light-facing edge accents visibly produced by the existing `Hut_Warm_Point` Light.

The user-approved correction is:

```text
retain atmospheric beam ribbons unchanged
+
add one actual shadowless URP Spot Light per active LightRay zone
+
retain the AF3 screen-space surface contribution only as an optional complement
+
default that screen-space complement to zero
```

The existing screen-space path is not deleted. It is rejected only as the primary material-lighting solution.

### AF4.2 Review surface and exact evidence

The following complete active surface path and direct contracts were reviewed before this plan:

- `Assets/AGENTS.md` — all four implementation gates, explicit component approval, no scene/prefab raw edits, no `DestroyImmediate` from `OnValidate`, cross-subsystem shader impact audit, and Unity validation requirements.
- `Assets/Docs/Weather_Light_Ray_Architecture.md` — current AF3 contract, failed renderer history, one-zone ownership, lifecycle/source/cloud contracts, performance budgets, diagnostics, and current screen-space surface implementation.
- `Assets/Docs/Weather_System_Architecture_Provisional.md` — parent Weather ownership and stale LightRay cross-reference requiring synchronization.
- `Assets/Docs/Weather_Cloud_Shadow_Handoff.md` — frozen cloud-cookie contract and stale screen-space-only LightRay cross-reference requiring synchronization.
- `Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs` — `WeatherLightRayDescriptor` currently carries one `SurfaceIlluminationIntensity` plus `FootprintEdgeSoftness`; `WeatherLightRaySnapshot.CurrentIntensity` already contains source gate, cloud gate, external gate, and lifecycle fade.
- `Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs` — AF3 exposes one surface intensity and one shared softness; `surfaceControlsVersion == 1`; the old Ground/Object/Cloud controls are hidden legacy data.
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs` — fixed slot storage, authored owner mapping, lifecycle update, source-state resolution, and one-active-authored-ray proof; no current real-light ownership exists.
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs` and `WeatherLightRayControllerEditor.cs` — AF3 surface Inspector/report vocabulary is screen-space-only and must be made explicit.
- `Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs` — uploads AF3 surface intensity and softness to the existing full-resolution composite; this path can remain as the optional complement with no shader architecture change.
- `Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl` and `SH_WeatherLightRayComposite.shader` — the AF3 surface field reconstructs visible position from depth and modifies already-rendered RGB; it cannot invoke receiver material lighting.
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl` — `GetAdditionalLight(...)` feeds punctual-light direction, distance attenuation, body response, and `edgeAccent`; this is the path required to reproduce the `Hut_Warm_Point` style.
- `Assets/_Recovery/0.unity` — `Hut_Warm_Point` is a realtime Point Light, intensity `3`, range `3`, shadows disabled, default culling mask, rendering layer `1`.
- `Assets/Settings/PC_RPAsset.asset` — additional lights are per-pixel, per-object limit `4`, additional-light shadows supported, and the project already compiles receiver additional-light variants.

### AF4.3 Corrected ownership model

One LightRay zone owns two independent surface contributions:

```text
Primary surface response:
    one actual shadowless realtime URP Spot Light
    → receiver materials execute their normal additional-light path

Optional complement:
    existing full-resolution screen-space circular lift
    → default intensity 0
    → retained only for restrained artistic fill if later useful
```

Hard prohibitions:

- never create one Unity Light per atmospheric beam;
- never use the optional screen-space complement as the default or sole material-lighting solution;
- never add a scene/prefab Light component for this proof;
- never let the real Light alter the frozen cloud-cookie implementation;
- never enable additional-light shadows in this proof;
- never create or destroy hidden Light objects from `OnValidate`.

The runtime controller owns a lazily created, hidden, nonserialized Spot Light proxy per active slot. The current proof still permits only one active authored zone, so normal proof cost is one shadowless additional light.

### AF4.4 Active authoring contract and migration

The surface foldout becomes exactly:

```text
Real Surface Light Intensity          [0, 1], default 0.20
Optional Screen-Space Complement      [0, 1], default 0.00
Surface Edge Softness                 [0, 1], retained default 0.42
```

`Real Surface Light Intensity` is the primary control. `0` disables the runtime Spot Light. `1` means the calibrated proof maximum defined in AF4.6.

`Optional Screen-Space Complement` drives the existing AF3 composite scalar without changing its shader equation. It defaults to `0` and therefore exits before depth reconstruction.

`Surface Edge Softness` is shared by both paths so the author does not manage contradictory footprint boundaries.

Serialization migration:

1. rename the existing serialized `surfaceIlluminationIntensity` field to `screenSpaceSurfaceIntensity` with `FormerlySerializedAs`;
2. add `surfaceSpotLightIntensity = 0.20`;
3. increment `surfaceControlsVersion` from `1` to `2`;
4. every pre-AF4 anchor is migrated once to `screenSpaceSurfaceIntensity = 0` and `surfaceSpotLightIntensity = 0.20`, explicitly disabling the old primary overlay as approved by the user;
5. later authored values are preserved after version `2`.

### AF4.5 Spot Light footprint geometry

Let:

```text
R = Area Diameter / 2
S = Surface Edge Softness in [0, 1]
H = max(1.5 m, 2R)
F = 0.35 R S
Ri = max(0, R - F)
Ro = R + F
```

The Spot Light is placed at:

```text
L = footprint centre + worldUp * H
```

and aimed vertically downward. The vertical axis is intentional: a tilted cone intersecting a horizontal receiver plane produces an ellipse, contradicting the approved circular footprint.

The derived cone angles are:

```text
innerSpotAngle = 2 atan(Ri / H)
outerSpotAngle = 2 atan(Ro / H)
```

Endpoint semantics:

- `S = 0`: `Ri = Ro = R`; inner and outer cone angles coincide at the authored radius, producing the hardest Spot edge Unity permits;
- `S = 1`: full-strength cone reaches `0.65R`, the transition is centred approximately around `R`, and the outer cone reaches `1.35R`;
- the same `0.35R` bidirectional softness convention remains shared with the optional screen-space complement.

The required range is based on the outer receiver edge:

```text
dmax = sqrt(H^2 + Ro^2)
range = 1.5 dmax
```

The `1.5` margin prevents Unity range attenuation from reaching zero at the authored footprint edge while keeping the shadowless light bounds finite.

### AF4.6 Normalized intensity mathematics

The existing `Hut_Warm_Point` proof uses Light intensity `3` near its receivers. Use that as the initial one-metre reference, not as a direct Spot intensity copy.

Assuming the receiver additional-light path is approximately inverse-square near the cone centre, compensate for Spot height:

```text
Iref = 3
Iunity = U * CurrentIntensity * SourceIntensity * ColourPeak * Iref * H^2
```

where:

- `U` is `Real Surface Light Intensity` in `[0, 1]`;
- `CurrentIntensity` already includes lifecycle, source gate, cloud policy, and external visibility;
- `SourceIntensity` is the resolved celestial source intensity, or `1` for an approved source-gate override fallback;
- `ColourPeak` preserves HDR colour-multiplier energy after normalizing the Light colour;
- `H^2` compensates for the derived height so the same normalized control remains approximately stable across Area Diameter.

At `U = 1`, the centre-plane proof target is approximately the `Hut_Warm_Point` one-metre intensity. At the default `U = 0.20`, the target is approximately 20% of that response. This mapping is mathematically grounded but remains **Unverified — Medium confidence** until Unity compares vegetation and receiver response directly.

The Light colour uses the same source/profile/descriptor/warmth chain as the atmospheric renderer, normalized to a valid Light chromaticity; the removed magnitude is restored through `ColourPeak` in `Iunity`.

### AF4.7 Runtime pooling and lifecycle

`WeatherLightRayController` owns a parallel lazy proxy array keyed by runtime slot index.

Rules:

- proxy storage only grows when central slot capacity grows; no per-frame managed allocation;
- a hidden `HideAndDontSave` GameObject and `Light` component are created only when a slot first needs nonzero real-light output;
- creation is suppressed during `OnValidate`; the next normal edit-preview `Update`, `OnEnable`, or explicit `RefreshNow` may create it;
- inactive/released/zero-intensity slots disable their Light immediately but retain the proxy for reuse;
- controller disable/destroy removes all hidden proxies outside `OnValidate`;
- Light configuration is realtime Spot, shadows off, no cookie, bounce intensity zero, default object culling, rendering layer `1`, and vertically downward orientation;
- every controller tick updates position, cone angles, range, colour, and intensity from the immutable snapshot and resolved source state.

### AF4.8 Performance and cross-subsystem impact

Primary new steady-state cost for the current proof:

```text
one shadowless per-pixel URP additional Spot Light
```

The PC pipeline permits four additional per-pixel lights per object. The LightRay Spot therefore consumes one of those four slots on affected receivers. The proof remains limited to one active authored zone; procedural population and overlapping-zone policy remain blocked.

Cross-subsystem effects are intentional:

- Vegetation receives its existing punctual-light body and edge-accent response;
- Ground, Generated Mass, trees, actors, bridge, rocks, River, and other additional-light-aware receivers use their existing material light path;
- materials without additional-light support will not receive the real Spot and may use the optional complement only if explicitly authored above zero;
- no shared receiver shader/include is modified by AF4;
- no cloud, Time of Day, wind, River, vegetation, Ground, Generated Mass, scene, prefab, material, renderer asset, layer, or tag is modified.

No performance claim is accepted until 1440p profiling compares:

1. Spot intensity `0`, complement `0`;
2. Spot active, complement `0`;
3. Spot active plus a small optional complement.

### AF4.9 Approved file scope

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Docs/Weather_System_Architecture_Provisional.md
Assets/Docs/Weather_Cloud_Shadow_Handoff.md
Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs
Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs
```

No shader, shared receiver include, source profile, renderer feature, scene, prefab, material, render-pipeline asset, layer, tag, package, folder, or new source file is approved.

### AF4.10 File-by-file implementation sequence

1. Update current canonical invariants/status and mark AF3 post-composite lighting as optional complementary presentation rather than primary lighting.
2. Update the parent Weather and cloud handoff cross-references so no current document claims screen-space-only surface ownership.
3. Split the descriptor surface contract into real Spot intensity, optional complement intensity, and shared softness.
4. Add serialized controls and one-time migration to the Anchor; default the complement to zero.
5. Update Anchor Inspector and live status with explicit primary/optional wording.
6. Add lazy per-slot Spot Light proxy ownership, update, disable, cleanup, and diagnostics to the Controller.
7. Update Controller Inspector/report with active Spot object, derived height/angles/range/intensity, and optional complement value.
8. Route only the optional complement intensity into the unchanged AF3 composite shader parameters.
9. Re-read all changed files and direct consumers, compare final diff to AF3 and this ledger, run compile-oriented static checks, constructor/producer/Inspector alignment, no-allocation checks, and patch-application byte verification.

### AF4.10A Implementation result

Source implementation completed inside the approved nine-file scope.

- `WeatherLightRayDescriptor` now carries `SurfaceSpotLightIntensity`, `ScreenSpaceSurfaceIntensity`, and shared `FootprintEdgeSoftness`.
- `WeatherLightRayAnchor` exposes Real Surface Light Intensity, Optional Screen-Space Complement, and shared softness. A version-2 migration sets every pre-AF4 complement to `0` and the new real-light intensity to `0.20` once.
- `WeatherLightRayRenderPass` routes only `ScreenSpaceSurfaceIntensity` into the unchanged AF3 composite parameters.
- `WeatherLightRayController` owns a parallel lazy per-slot proxy array. Each proxy is a hidden `HideAndDontSave` realtime Spot Light with shadows disabled, no cookie, bounce zero, rendering layer `1`, and no per-frame allocation.
- Spot creation is suppressed during `OnValidate`; existing proxies may be updated or disabled there, while normal controller updates create them when required.
- Slot release disables and retains the proxy for reuse. Controller disable/destroy removes all hidden proxies outside `OnValidate`.
- Runtime diagnostics report enabled Spot count, Light object, derived height, inner/outer footprint radii, and applied intensity.
- No shader, receiver include, source profile, renderer feature, scene, prefab, material, pipeline asset, layer, tag, or component asset was changed.

### AF4.11 Acceptance criteria

- The old screen-space contribution is zero by default on existing and new anchors.
- `Real Surface Light Intensity = 0` creates no enabled Light contribution.
- A nonzero real intensity produces normal material lighting, including vegetation punctual-light edge accents comparable in kind to `Hut_Warm_Point`.
- Changing Area Diameter updates Spot height, inner angle, outer angle, and range while retaining one circular nominal footprint.
- Softness `0` is the hardest achievable cone boundary; softness `1` widens the inner-to-outer transition bidirectionally around the nominal radius.
- Lifecycle, source gate, cloud policy, and external visibility fade both atmosphere and the real Spot through the same `CurrentIntensity`.
- The optional complement can be enabled independently but defaults to zero and never replaces the real-light result.
- Only one hidden Spot is enabled for the one-active-authored-zone proof.
- No additional-light shadows, scene/prefab changes, per-beam Lights, per-frame allocations, or shared receiver shader changes occur.
- Unity Console is clear; vegetation, Ground, bridge, rock, and actor response are visually inspected; the per-object four-light limit and GPU cost are profiled before freeze.

### AF4.12 Non-goals

- no atmospheric beam recalibration;
- no beam layout/profile/softening change;
- no tilted or elliptical real-light footprint;
- no Spot cookie;
- no additional-light shadows;
- no procedural population or overlapping-zone policy;
- no attempt to make materials lacking additional-light support respond automatically;
- no freeze or performance acceptance claim in this source patch.

### AF4.13 Final source and mathematical audit

Available source checks passed:

```text
Changed-file scope:                         9 / 9 exact
Descriptor constructor/call alignment:      33 / 33
Anchor Inspector serialized properties:     33 / 33
Controller Inspector serialized properties: 14 / 14
Obsolete active surface identifier uses:    0
Shader files changed:                        0
Scene/prefab/material/pipeline changes:      0
Raw brace/parenthesis balance:               PASS
Conflict markers / trailing whitespace:      PASS
Changed-files ZIP byte verification:        9 / 9
Clean-baseline patch application:            9 / 9 byte-identical
```

The Spot geometry was evaluated at Area Diameters `0.60`, `1.80`, `4.80`, and `6.60 m`, and softness values `0`, `0.42`, and `1`:

```text
height range:             1.50–6.60 m
inner full cone angle:    14.814–53.130 degrees
outer full cone angle:    22.620–68.039 degrees
range margin over dmax:   exactly 1.5x
invalid inner>outer cases: 0
nonfinite geometry cases:  0
```

At the default Area Diameter `4.80 m`, softness `0.42`, and normalized real-light intensity `0.20`, the source equation derives approximately:

```text
height:                   4.80 m
inner footprint radius:   2.047 m
nominal footprint radius: 2.400 m
outer footprint radius:   2.753 m
inner Spot angle:         46.196 degrees
outer Spot angle:         59.669 degrees
range:                    8.300 m
pre-source intensity:     13.824
```

The final Unity intensity is still multiplied by lifecycle/current gate, resolved source intensity, and normalized colour peak. The inverse-square compensation is a proof calibration, not an accepted physical-unit claim.

The only new managed allocations are the lazy proxy array when central capacity first grows and one hidden GameObject/Light pair when an active slot first requires nonzero real-light output. The controller update path reuses those proxies and performs no per-frame collection allocation.

### AF4.14 Pending Unity proof

AF4 remains **source-implemented, Unity-unverified**. Required evidence:

- C# compilation against Unity 6000.5.0f1, including `Light.innerSpotAngle`, `Light.renderingLayerMask`, and hidden proxy cleanup;
- one enabled hidden Spot for one active authored zone and none when Real Surface Light Intensity is zero;
- vegetation body response and stylized edge accents comparable in kind to `Hut_Warm_Point`;
- material-correct response on Ground, Generated Mass, bridge wood, rocks, and the player;
- optional screen-space complement exactly absent at its default zero value and independently usable above zero;
- Area Diameter and Surface Edge Softness update the real cone without creating an elliptical ground footprint;
- no additional-light-shadow cost or warnings;
- interaction with the URP four-additional-lights-per-object limit;
- 2560 x 1440 GPU comparison for real Spot off/on and Spot plus restrained complement.

## V1.1D-AF5 — Vegetation-aware LightRay Spot edge-accent override

### AF5.1 Status and trigger

**Status before source edits:** canonical plan recorded; implementation not started.

**Triggering Unity evidence:** the AF4 real Spot produces valid broad URP body lighting, but its vertical fragment-to-light direction does not activate the production vegetation shader's lateral blade-edge gate. `Hut_Warm_Point` produces the approved stylized accents because its low point-light direction has a strong horizontal component. Raising Spot intensity cannot solve a directional dot product that remains near zero.

### AF5.2 Objective

Keep the accepted one-real-Spot-per-zone architecture and preserve its ordinary body lighting on every receiver. For the production vegetation shader only, identify the LightRay Spot and substitute a coherent horizontal LightRay/source direction **only** in the existing stylized edge-accent selector.

The patch must add no second light, no vegetation footprint loop, no texture field, no new authoring control, and no receiver changes outside vegetation.

### AF5.3 Reviewed source and evidence

The complete active AF4/AF4A LightRay controller, descriptor producer, Spot proxy lifecycle, production vegetation shader, `VegetationLighting.hlsl`, PC URP asset, canonical Weather LightRay document, and canonical vegetation architecture were reviewed before this plan.

Exact active facts:

- `WeatherLightRayController.GetOrCreateSurfaceSpotLight()` creates one hidden realtime shadowless Spot and currently assigns `renderingLayerMask = 1`.
- `WeatherLightRayController.UpdateSurfaceSpotLight()` places the Spot vertically above the footprint and points it straight down.
- `VegetationEvaluateDirectLight()` uses the real `light.direction` for both body diffuse and `VegetationLightFacingEdge()`.
- `VegetationLightFacingEdge()` gates the accent with `smoothstep(0.15, 0.45, abs(dot(bladeLateralWS, lightDirectionWS)))`.
- The production vegetation shader already pays for the Spot in the existing additional-light loop.
- `PC_RPAsset.asset` enables per-pixel additional lights, four lights per object, and Rendering Layers.
- All supplied renderer/light masks currently use only default bit `1`.

### AF5.4 Light identity without a serialized project-layer edit

AF5 reserves one runtime-only Rendering Layer bit:

```text
Default receiver bit D = 1
LightRay identity bit T = 1 << 30
LightRay Spot mask M = D | T
```

The Spot continues to affect every default-layer receiver because:

```text
M & D = D != 0
```

The extra bit is used only as shader identity. AF5 does **not** rename Rendering Layers and does not modify `UniversalRenderPipelineGlobalSettings.asset`, scenes, prefabs, materials, or renderer masks.

The vegetation shader adds the `_LIGHT_LAYERS` variant and follows URP's normal matching rule with `GetMeshRenderingLayer()` and `IsMatchingLightLayer()`. This preserves correct Rendering Layer filtering while allowing the LightRay identity bit to be read from `Light.layerMask`.

### AF5.5 Accent-direction mathematics

`WeatherLightRaySnapshot.RayDirectionWorld` is the visible beam travel direction toward the receiver. The desired coherent direction from a blade toward the apparent celestial source is:

```text
d_source = -normalize(RayDirectionWorld)
```

Project it onto the world-horizontal plane:

```text
h = d_source - Up * dot(d_source, Up)
```

When `dot(h, h) > 1e-6`:

```text
accentDirectionWS = normalize(h)
accentOverrideActive = 1
```

Fallback order when the presentation direction is nearly vertical:

1. horizontal projection of `sourceState.DirectionToSourceWorld`;
2. if still degenerate, publish override inactive and preserve the real Spot direction.

The controller publishes one global vector:

```text
_WeatherLightRayVegetationAccentDirectionWS =
    float4(accentDirectionWS, accentOverrideActive)
```

If more than one Spot is enabled, the direction associated with the largest applied Spot intensity is published. Current policy forbids simultaneous procedural Sun and Moon populations, and rays of one source family share a celestial direction, so this remains coherent for the approved proof.

### AF5.6 Shader ownership

For ordinary lights:

```text
edgeDirection = light.direction
```

For a tagged LightRay Spot with an active published direction:

```text
edgeDirection = _WeatherLightRayVegetationAccentDirectionWS.xyz
```

Only `VegetationLightFacingEdge()` and its pixel-stability calculation consume `edgeDirection`.

The following continue to use the real URP Spot data unchanged:

- wrapped body diffuse;
- light colour;
- distance and cone attenuation;
- local activation energy;
- edge falloff power;
- edge radiance colour and intensity;
- body-fill restraint;
- light-list culling and four-light-per-object budget.

This is a stylized directional override for the existing punctual accent, not a second light or custom footprint.

### AF5.7 Approved file scope

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Docs/Stylized_Vegetation_Architecture.md
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl
Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader
```

No other file may change. No scene, prefab, material, URP asset, Rendering Layer name, renderer mask, serialized authoring field, component, tag, layer, dependency, texture, pass, draw, or light may be added.

### AF5.8 File-by-file implementation sequence

1. Record this plan before source edits.
2. Add the runtime identity-bit constants and vegetation-accent global property to the controller.
3. Tag only hidden LightRay Spot proxies with `Default | LightRayIdentity`.
4. During Spot updates, derive the strongest active horizontal source direction and publish it once; publish inactive when no valid enabled Spot exists.
5. Reset the global on controller deactivation/disable.
6. Add `_LIGHT_LAYERS` compilation to the production vegetation Forward pass.
7. Add proper URP Rendering Layer matching to both additional-light loops.
8. Add the LightRay identity test and substitute only the edge-accent direction.
9. Update both vegetation canonical documents with the narrow exception to the ordinary punctual-direction contract.
10. Re-read complete final files and direct consumers; audit exact scope, preprocessor variants, light-mask constants, CPU/HLSL bit equality, global reset, unchanged body-light math, no new allocations, and no unapproved assets.

### AF5.9 Performance model

Incremental steady-state cost over AF4:

- one `Shader.SetGlobalVector` per controller tick;
- one Rendering Layer mask test and one branch/select per eligible punctual-light evaluation;
- one additional `_LIGHT_LAYERS` shader variant dimension;
- no additional `GetAdditionalLight`, light, loop, texture sample, render pass, draw call, buffer, or per-frame allocation.

The real Spot evaluation already exists in AF4. AF5 changes only the edge-direction input for the tagged light. GPU timing remains required; no millisecond claim is accepted from source inspection.

### AF5.10 Acceptance criteria

- Project compiles with zero C# or shader errors/warnings from AF5.
- `Hut_Warm_Point` retains byte-equivalent ordinary-light direction behavior.
- The LightRay Spot retains identical body illumination, attenuation, colour, footprint, and lifecycle.
- Vegetation inside the LightRay Spot shows the same **kind** of light-facing stylized edge accents as the point-light proof.
- LightRay accents remain coherent across the footprint instead of rotating radially around the overhead Spot.
- Setting Real Surface Light Intensity to zero disables both body response and the special accent because the tagged Spot is disabled.
- Disabling the LightRay controller clears the global override and leaves all ordinary punctual lights unchanged.
- No new light, texture, pass, draw call, allocation, scene/prefab/material edit, or serialized control appears.

### AF5.11 Non-goals and risks

- no change to atmospheric shafts or optional screen-space complement;
- no attempt to create directional-light edge accents generally;
- no global loosening of the vegetation edge gate;
- no second vegetation-only point light;
- no multi-zone custom field;
- no Rendering Layer-name asset edit;
- no performance freeze before live profiling.

Primary risks requiring Unity proof:

- the reserved runtime bit must survive URP's additional-light upload in both Forward and clustered paths;
- enabling `_LIGHT_LAYERS` must not expose an existing renderer/light-mask mismatch;
- a near-vertical celestial source produces no horizontal override by design and falls back to the real Spot direction;
- coherent source-facing accents are intentionally stylized and may require later visual calibration, but AF5 adds no new tuning control.

### AF5.12 Implementation result

Source implementation completed exactly inside the approved six-file scope.

- `WeatherLightRayController` reserves runtime Rendering Layer identity bit `1 << 30`, keeps default bit `1`, and assigns the combined mask to hidden Spot proxies at creation and every update.
- The controller derives the horizontal direction toward the visible LightRay source from `-RayDirectionWorld`, falls back to the resolved celestial `DirectionToSourceWorld`, selects the strongest enabled Spot when more than one slot is active, and publishes `_WeatherLightRayVegetationAccentDirectionWS` once per controller tick.
- Only the published controller writes the global. Non-published controllers disable their own proxies without clearing another controller's direction. The published controller clears the global when it has no runtime storage/valid Spot and during deactivation.
- `SH_StylizedVegetationBenchmark.shader` now compiles `_LIGHT_LAYERS` for the Forward fragment path.
- `VegetationLighting.hlsl` applies Unity's Rendering Layer matching rule in both clustered-directional and regular additional-light loops.
- The tagged LightRay Spot substitutes the published horizontal direction only for `VegetationLightFacingEdge()` and the dependent pixel-stability calculation.
- Wrapped body diffuse, colour, distance/cone attenuation, activation energy, edge falloff, body-fill restraint, HDR edge colour, additional-light count, and loop ownership remain unchanged.
- No new vegetation or LightRay control, Light, texture, buffer, render pass, draw call, component, scene/prefab/material edit, Rendering Layer-name edit, or renderer-mask edit was introduced.

### AF5.13 Final source and compliance audit

Available static evidence passed. The dedicated AF5 source audit reports `70 / 70` checks passed.

```text
Changed-file scope:                         6 / 6 exact
CPU/HLSL identity bit:                     bit 30 / bit 30
Spot mask default intersection:            (1 | 1<<30) & 1 = 1
Spot identity test:                        (1 | 1<<30) & (1<<30) != 0
Spot mask assignment sites:                creation + update
Additional GetAdditionalLight calls:        unchanged at 2
Additional light loops:                     unchanged at 1 regular + 1 clustered directional
New texture samples:                        0
New serialized fields or controls:          0
Global inactive/reset paths:                no storage, zero valid Spots, published deactivation
Non-published-controller global clearing:    absent
Rendering Layer matching sites:             both additional-light loops
Body-light direction use:                   unchanged real light.direction
Edge-direction override scope:               tagged eligible punctual edge only
Conflict markers / trailing whitespace:     none
Source delimiter balance:                   pass
Markdown fences:                            balanced
```

The horizontal-projection proof was exercised for vertical, oblique, and shallow incoming directions. Every valid override has `y = 0` and unit length; a vertical source correctly publishes no override and falls back to the real Spot direction.

Unity's URP lighting source uses `GetMeshRenderingLayer()` plus `IsMatchingLightLayer(light.layerMask, meshRenderingLayers)` around additional-light evaluation, and its `Light` structure carries a `uint layerMask`. AF5 follows that established pattern rather than inventing a separate filtering path.

### AF5.14 Pending Unity proof

AF5 remains **source-implemented, Unity-unverified**. Required evidence:

- zero C# and shader errors/warnings after import;
- LightRay Spot body lighting and footprint remain unchanged from AF4;
- the same vegetation patch shows coherent LightRay edge accents comparable in kind to `Hut_Warm_Point`;
- `Hut_Warm_Point` and every ordinary point/spot light retain their prior radial light-direction behavior;
- moving vegetation across the LightRay footprint does not rotate the accent direction around the overhead Spot;
- Real Surface Light Intensity `0` disables both body and LightRay-specific edge response;
- controller disable clears the override;
- 2560 × 1440 GPU median comparison shows no material regression beyond normal frame variance.


## V1.1D-AF5A — Consolidated vegetation-accent diagnostic suite

### AF5A.1 Status and trigger

**Status before source edits:** canonical plan recorded; diagnostic implementation not started.

**Triggering Unity evidence:** AF5's real Spot produces broad vegetation body lighting, while the same grass shows no LightRay-specific stylized edge accents at `Real Surface Light Intensity = 1`. `Hut_Warm_Point` still produces the accepted edge accents on the same vegetation shader. The AF5 CPU report can say the override is active, but that report does not prove the live vegetation shader variant received the identity bit or selected the override branch.

### AF5A.2 Objective

Add one consolidated diagnostic suite to the Weather LightRay Controller Inspector. The suite must use one run/stop button and one copy-results button. It must diagnose the complete CPU-to-GPU path without adding scattered material controls, per-object debug components, new lights, new render passes, or persistent scene/prefab changes.

The suite must answer these questions independently:

1. Does a real LightRay Spot exist and remain enabled?
2. Does its CPU `renderingLayerMask` contain both the default receiver bit and the LightRay identity bit?
3. Is a valid horizontal accent direction published globally?
4. Did the live vegetation Forward variant compile with `_LIGHT_LAYERS`?
5. Did any additional light reach the fragment?
6. Did the fragment receive the LightRay identity bit?
7. Did the LightRay match the vegetation renderer's Rendering Layer mask?
8. Did the shader select the published accent direction?
9. Did the selected path produce nonzero real edge radiance on blade-edge fragments?

### AF5A.3 Diagnostic ownership and UI

All new controls live in one `Vegetation Accent Diagnostic Suite` foldout on `WeatherLightRayControllerEditor`.

The foldout contains exactly:

- one dynamic `Run Vegetation Accent Diagnostic Suite` / `Stop Vegetation Accent Diagnostic Suite` button;
- one `Copy Vegetation Accent Diagnostic Results` button;
- read-only suite state, latest run identifier, CPU preflight verdict, and the false-colour legend.

No diagnostic control is added to vegetation materials, vegetation instances, LightRay anchors, renderer assets, scenes, or prefabs.

### AF5A.4 CPU diagnostic report

The suite records and copies one structured report containing:

- application/play state, published-controller ownership, active ray count, enabled Spot count, and render camera;
- expected default bit, identity bit, and combined Spot mask in decimal and hexadecimal;
- every allocated runtime Spot's existence, enabled state, type, intensity, range, inner/outer angle, position, forward direction, culling mask, Rendering Layer mask, and required-bit checks;
- controller-cached accent direction and the actual global vector returned by `Shader.GetGlobalVector`;
- vector validity, magnitude, horizontal `Y` error, and active `W` marker;
- current source ray direction and derived horizontal direction for the primary renderable ray;
- production vegetation shader existence and support state;
- current diagnostic global mode and the GPU false-colour legend;
- a CPU preflight verdict that may pass only when a valid enabled Spot, correct mask bits, and valid active global direction are all present.

CPU preflight is explicitly not a GPU verdict.

### AF5A.5 GPU false-colour classification

The production vegetation shader receives one global diagnostic mode. When inactive, final vegetation output is byte-equivalent to AF5. When active, vegetation fragments return one classification colour instead of normal shading.

The classification priority is:

```text
Magenta = `_LIGHT_LAYERS` is absent in the live vegetation variant.
Red     = `_LIGHT_LAYERS` is active, but no additional light reached this fragment.
Orange  = additional light data exists, but no LightRay identity bit was seen.
Purple  = the identity bit was seen, but the LightRay failed the mesh Rendering Layer match.
Yellow  = identity and layer match succeeded, but the published global direction is inactive or invalid.
Cyan    = identity, layer match, and global direction succeeded, but the override was not selected.
Dark blue = override selected, but the tagged Spot produced no body radiance at this fragment.
Blue    = override selected and body radiance exists, but actual edge radiance is zero here.
Green   = override selected and actual LightRay edge radiance is nonzero on this blade-edge fragment.
```

The shader records diagnostic flags during the existing additional-light loops. It does not add another `GetAdditionalLight`, another light loop, texture sample, draw call, buffer, UAV, readback, renderer feature, or render pass.

### AF5A.6 Shader data contract

`VegetationLightingResult` gains diagnostic-only fields recording:

```text
lightLayersVariantActive
anyAdditionalLightSeen
lightRayIdentitySeen
lightRayLayerMatch
publishedDirectionActive
accentOverrideSelected
lightRayBodyLuminance
lightRayEdgeLuminance
```

The LightRay identity test is evaluated before Rendering Layer rejection so the suite can distinguish identity absence from receiver-mask mismatch. Normal lighting evaluation remains unchanged.

### AF5A.7 Approved file scope

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl
Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader
```

No other file may change. No scene, prefab, material, URP asset, Rendering Layer name, renderer mask, serialized authoring field, component, tag, layer, dependency, texture, pass, draw, buffer, or light may be added.

### AF5A.8 File-by-file implementation sequence

1. Record this plan before source edits.
2. Add controller-owned diagnostic session state, global mode publication/reset, CPU preflight, and structured result report.
3. Add the single run/stop and copy-results UI inside one Inspector foldout.
4. Extend the existing vegetation lighting result with diagnostic flags gathered from the existing light loops.
5. Add one false-colour resolver and one final-fragment diagnostic override controlled by the global mode.
6. Preserve normal AF5 lighting output exactly while diagnostics are inactive.
7. Re-read the complete final review surface and audit exact scope, no added light fetches/loops/samples/passes, global reset ownership, report completeness, diagnostic colour reachability, and unchanged normal rendering path.

### AF5A.9 Acceptance criteria

- Project compiles with zero AF5A C# or shader errors/warnings.
- The Weather Inspector exposes exactly one diagnostic run/stop button and one copy-results button in one foldout.
- Starting the suite enables the vegetation false-colour view; stopping or disabling the published controller restores normal vegetation rendering.
- The copied report includes all CPU Spot masks, global direction state, shader support, suite state, and the colour legend.
- The false-colour view distinguishes every failure boundary listed in AF5A.5.
- AF5A adds no extra Light, light fetch, light loop, texture sample, render pass, draw call, buffer, readback, allocation in the steady-state controller tick, scene/prefab/material edit, or serialized control.
- With the diagnostic suite inactive, the AF5 body-light and accent code remains behaviorally unchanged.

### AF5A.10 Non-goals

- no attempt to repair the AF5 identity path in this patch;
- no accent-threshold, intensity, softness, colour, or Light geometry tuning;
- no permanent false-colour material state;
- no automatic screenshot capture or blocking GPU readback;
- no profiling or authored-proof freeze before the failed AF5 condition is identified from the report and false-colour evidence.

### AF5A.11 Implementation result

Source implementation completed exactly inside the approved five-file scope.

- `WeatherLightRayController` owns one nonserialized diagnostic session, one global false-colour mode, one CPU preflight, and one structured result report.
- The controller report enumerates every runtime Spot and its mask bits, compares cached and actual shader-global accent vectors, reports the primary ray/source directions, checks the production vegetation shader, and includes the complete GPU legend.
- `WeatherLightRayControllerEditor` adds one `Vegetation Accent Diagnostic Suite` foldout containing exactly one dynamic run/stop button and one copy-results button. No diagnostic control was added elsewhere.
- `VegetationLighting.hlsl` records the live `_LIGHT_LAYERS` variant, additional-light presence, identity-bit presence, mesh-layer match, published-direction state, override selection, tagged-Spot body luminance, and tagged-Spot edge luminance while the diagnostic mode is active.
- `SH_StylizedVegetationBenchmark.shader` returns the diagnostic false colour before normal fog/composite only while the global mode is active. The established AF5 final-colour formula remains the normal inactive path.
- The published controller clears the diagnostic global during disable/deactivation. A non-published controller cannot clear another controller's active suite.
- No scene, prefab, material, URP asset, Rendering Layer name, renderer mask, serialized field, light, texture, buffer, UAV, readback, render pass, draw call, or dependency was added.

### AF5A.12 Final source and compliance audit

Available static evidence passed:

```text
Changed-file scope:                        5 / 5 exact
Diagnostic controls:                       1 run/stop + 1 copy-results
Additional GetAdditionalLight calls:       unchanged at 2
Additional-light loops:                    unchanged at 1 regular + 1 clustered directional
New texture samples:                       0
New Light / pass / draw / buffer / readback: 0
CPU/HLSL identity bit:                     bit 30 / bit 30
_LIGHT_LAYERS production variant:          retained
Global reset ownership:                    published controller only
New serialized diagnostic fields:          0
GPU classification boundaries:             9 / 9 represented
Normal vegetation final-colour formula:     retained
Conflict markers / trailing whitespace:    none
Source delimiter balance:                  pass
Markdown fences:                           balanced
```

The diagnostic accumulation is guarded by the uniform global diagnostic mode. Normal AF5 rendering remains the inactive output path; the suite is not intended as a permanent runtime visualization.

### AF5A.13 Pending Unity proof

AF5A remains **source-implemented, Unity-unverified**. Required evidence:

- zero C# and vegetation-shader errors/warnings after import;
- exactly one consolidated diagnostic foldout with the two approved buttons;
- starting the suite replaces vegetation shading with one of the documented classification colours;
- stopping the suite or disabling the Weather controller restores normal vegetation shading immediately;
- copied results include CPU preflight, Spot masks, global direction, shader state, and legend;
- a screenshot of the same grass patch under the LightRay Spot plus the copied report identifies the first failed AF5 boundary;
- no scene/prefab dirtying or persistent material state is produced by running/stopping the suite.


## V1.1D-AF5B — Geometric runtime-Spot identification

### AF5B.1 Triggering evidence and exact failure boundary

The AF5A consolidated suite produced the following evidence on the same vegetation shader and material:

- CPU preflight passed;
- one enabled LightRay Spot existed and reached the vegetation;
- the Spot's CPU `renderingLayerMask` contained `0x40000001`;
- the published horizontal accent direction was valid and active;
- vegetation inside the Spot rendered **orange** in the GPU classification;
- `Hut_Warm_Point` continued to generate the accepted stylized blade accents on the same grass.

Under the AF5A legend, orange means that additional-light data reached the fragment but the LightRay identity bit did not appear in the GPU `Light.layerMask`. Therefore the failed boundary is not vegetation lighting, local-light eligibility, intensity, edge masks, or the published horizontal vector. The failure is specifically the assumption that an unregistered high CPU Rendering Layer bit can be used as reliable shader identity.

AF5's bit-30 identity contract is rejected. Rendering Layers remain ordinary receiver filtering only.

### AF5B.2 Approved production contract

The controller publishes the strongest enabled LightRay surface Spot as two globals:

```text
_WeatherLightRayVegetationAccentSpotPositionWS.xyz = Spot world position
_WeatherLightRayVegetationAccentSpotPositionWS.w   = Spot range in metres
_WeatherLightRayVegetationAccentDirectionWS.xyz    = normalized horizontal direction toward the LightRay source
_WeatherLightRayVegetationAccentDirectionWS.w      = active marker
```

For each additional light already evaluated by vegetation at world position `P`, the shader derives:

```text
V = publishedSpotPosition - P
r² = dot(V, V)
expectedDirection = V / sqrt(r²)
evaluatedDirection = normalize(light.direction)
agreement = dot(expectedDirection, evaluatedDirection)
```

The light is classified as the published LightRay Spot only when:

```text
publishedSpotRange > 0
0 < r² <= publishedSpotRange²
agreement >= 0.999
```

The `0.999` threshold corresponds to approximately `2.56°` maximum directional disagreement. This is narrow enough to reject ordinary nearby lights while tolerating normal shader precision on the PC target.

For the matched Spot only:

```text
bodyLightDirection = light.direction
edgeAccentDirection = published horizontal LightRay direction
```

All colour, distance attenuation, cone attenuation, activation, falloff, edge stability, body response, and ordinary-light behaviour remain unchanged. `Hut_Warm_Point` and all nonmatching lights continue to use their real `Light.direction` for both body and edge response.

### AF5B.3 Multi-zone rule

The controller continues to publish only the strongest enabled LightRay surface Spot. This keeps the shader contract constant-cost and avoids a per-fragment zone loop. Other active LightRay Spots still contribute normal URP body lighting, but only the published strongest Spot receives the coherent horizontal vegetation edge override.

### AF5B.4 Diagnostic-suite update

The existing single run/stop button and copy-results button remain the sole diagnostic UI. The false-colour stages are updated:

```text
Magenta  published Spot position/range inactive or invalid
Red      no additional light reached the fragment
Orange   additional lights exist, but none geometrically match the published Spot
Purple   geometric Spot match succeeded but failed normal mesh Rendering Layer filtering
Yellow   Spot matched but the horizontal accent direction is inactive or invalid
Cyan     Spot and direction matched but override selection did not occur
Dark blue override selected but the matched Spot produced no body radiance
Blue     matched Spot produces body radiance but no edge radiance
Green    actual LightRay edge radiance is emitted
```

The copied report now includes the published Spot position and range, the live Spot positions and ranges, the CPU/global position delta, the directional-match threshold, and the horizontal direction. CPU preflight verifies that an enabled runtime Spot matches the published position/range within `0.001 m`.

### AF5B.5 Performance model

AF5B removes the failed bit test and adds, per eligible additional-light evaluation:

- one world-position subtraction;
- one squared-distance calculation and range comparison;
- one reciprocal square root for expected direction;
- one normalization of the evaluated light direction;
- one dot product and threshold comparison.

It adds no Light, no extra `GetAdditionalLight`, no extra light loop, no texture sample, no render pass, no draw call, no buffer, and no per-frame managed allocation. The work scales with the additional lights already evaluated by the vegetation shader, not with the number of LightRay beams.

### AF5B.6 Approved file scope

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Docs/Stylized_Vegetation_Architecture.md
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl
```

No scene, prefab, material, renderer asset, Rendering Layer name, serialized authoring control, Light component count, shader texture, pass, draw, buffer, or dependency changes.

### AF5B.7 Implementation result and pending proof

Source implementation now:

- publishes the selected Spot position and range alongside the horizontal accent direction;
- resets both globals when no valid published Spot exists;
- returns runtime Spot proxies to the default receiver Rendering Layer mask instead of using bit 30 as identity;
- geometrically identifies the Spot inside the existing additional-light evaluation;
- preserves the real Spot direction for body lighting;
- substitutes the horizontal source direction only for the matched Spot's vegetation edge selector;
- retains normal URP Rendering Layer filtering;
- updates the consolidated suite and copied report to prove geometric matching and real edge output.

AF5B remains unaccepted until Unity proves:

1. zero C# and shader errors/warnings;
2. diagnostic vegetation inside the LightRay Spot progresses from the former orange result to blue/green;
3. green appears specifically on blade-edge fragments where actual LightRay edge radiance is nonzero;
4. normal rendering shows `Hut_Warm_Point` unchanged and the LightRay Spot producing the same kind of stylized blade accent;
5. Spot off/on profiling at `2560 × 1440` confirms acceptable vegetation GPU cost.


## V1.1D-AF5C — Controller-owned shared accent-line intensity

### Decision

LightRay-specific accent responses require one controller-owned global master that is independent from the real surface-light intensity. It must not become a vegetation-only material control. Vegetation is the first consumer; GeneratedMass, river, trees, and other LightRay-aware receivers may reuse the same global later.

Active control:

```text
Weather Light Ray Controller
→ Hybrid Renderer
→ Accent Line Intensity [0, 1]
```

Contract:

```text
0.0 = disable LightRay-specific accent responses
0.5 = preserve the accepted AF5B receiver baseline
1.0 = permit up to 2x the AF5B baseline inside each receiver's existing safety cap
```

The mapping is deliberately independent from `Real Surface Light Intensity`. The real Spot continues to own body illumination, attenuation, cone geometry, colour, and material-light participation. Accent Line Intensity scales only the additional stylized accent response selected for the matched LightRay Spot. Ordinary lights, including `Hut_Warm_Point`, never read this multiplier.

### Vegetation implementation

The controller publishes `_WeatherLightRayAccentLineIntensity`. Once the geometric Spot match selects the LightRay vegetation override, the existing receiver-local edge gain is modified as:

```text
baseGain = 4 × vegetationAccentResponse
lightRayGain = 2 × saturate(controllerAccentIntensity)
resolvedGain = min(4, baseGain × lightRayGain)
```

The established vegetation gain cap of `4` remains authoritative. This provides useful downward and upward tuning without allowing the global Weather control to bypass receiver safety limits. Body lighting and non-LightRay punctual lights remain byte-for-byte on their prior equations.

### Scope and future ownership

AF5C introduces one serialized controller value and one generic shader global. It does not add a Light, light evaluation, texture sample, pass, draw, buffer, zone loop, or per-frame allocation. Future receivers should consume the same global only after implementing their own bounded LightRay accent response; they must retain receiver-local caps and must not reinterpret the control as general illumination intensity.

### Validation boundary

AF5C passes only when:

1. `Accent Line Intensity = 0` removes LightRay-specific vegetation accents while preserving Spot body illumination;
2. `0.5` reproduces AF5B;
3. `1.0` makes the matched LightRay blade accents materially stronger without changing `Hut_Warm_Point`;
4. the diagnostic suite still reaches green on actual edge-radiance fragments;
5. the comprehensive and diagnostic reports expose both the authored value and published shader global.


## V1.1D-AF5D — Expanded shared accent-line response range

### Evidence and correction

Unity validation proved that the AF5C `0..1` mapping was severely underpowered. At `1.0`, the matched LightRay vegetation accent remained only subtly stronger than the AF5B baseline. The requested calibration is that the former AF5C maximum should occur near `0.2`, while the former `0.5` result should occur below `0.1`.

The AF5C mapping is therefore superseded. The active controller contract remains normalized and independent from real Spot intensity, but its receiver multiplier becomes:

```text
LightRay accent multiplier = 12 × saturate(controller Accent Line Intensity)
```

Reference points:

```text
0.000000 = LightRay-specific accent disabled
0.083333 = 1x AF5B baseline; equivalent to former AF5C 0.5
0.166667 = 2x AF5B baseline; equivalent to former AF5C 1.0
1.000000 = up to 12x AF5B baseline before receiver-local caps
```

The default is `1 / 12`, preserving the AF5B baseline for newly created controllers. Existing serialized values are not silently rewritten; authored controllers should be recalibrated against the new scale.

Vegetation still resolves:

```text
baseGain = 4 × vegetationAccentResponse
lightRayGain = 12 × saturate(controllerAccentIntensity)
resolvedGain = min(4, baseGain × lightRayGain)
```

The established receiver-local cap remains authoritative. This patch changes only the shared LightRay accent range. It does not change Spot body lighting, surface-light intensity, geometric Spot matching, ordinary punctual-light behavior, or `Hut_Warm_Point`. Future GeneratedMass, river, tree, and other consumers will read the same expanded controller global while retaining their own local caps.

### Validation boundary

1. `Accent Line Intensity = 0` removes only LightRay-specific accent radiance.
2. Approximately `0.083` reproduces the AF5B baseline.
3. Approximately `0.167–0.2` reproduces or slightly exceeds the former AF5C maximum.
4. Values above `0.2` provide materially stronger response until the vegetation-local gain cap is reached.
5. Real Spot body illumination and `Hut_Warm_Point` remain unchanged across the sweep.


## V1.1D-AF5E — orders-of-magnitude shared accent range

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

## V1.1D-AF5F — proportional half-strength accent response

Unity visual proof found the uncapped AF5E range slightly excessive at the accepted top setting. AF5F preserves the exact normalized slider shape and `0 = off`, but multiplies the complete resolved response by `0.5`:

```text
relativeScale(c) = c <= 0 ? 0 : 0.5 * (1001^c - 1)

0.00 -> 0x
0.03 -> approximately 0.12x the former AF5D maximum
0.10 -> approximately 0.50x
0.20 -> approximately 1.49x
0.50 -> approximately 15.32x
1.00 -> 500x
```

This is an exact proportional reduction at every slider value: the same authored value now produces half the AF5E accent radiance. Geometric Spot matching, real Spot body lighting, atmospheric shafts, ordinary punctual lights, and `Hut_Warm_Point` are unchanged. The multiplication remains controller-side, so no per-fragment operation or shader resource is added.



## V1.1D-AF5G — capped 200x shared accent range

Unity validation found the AF5F upper half still excessive. AF5G preserves the exact normalized exponential slider shape and `0 = off`, but reduces the complete resolved response to `40%` of AF5F:

```text
relativeScale(c) = c <= 0 ? 0 : 0.2 * (1001^c - 1)

0.00 -> 0x
0.03 -> approximately 0.046x the former AF5D maximum
0.10 -> approximately 0.20x
0.20 -> approximately 0.60x
0.50 -> approximately 6.13x
1.00 -> 200x
```

The upper endpoint is now exactly `200x` the former AF5D reference instead of AF5F's `500x`. Every slider value produces `40%` of its AF5F radiance. The geometric Spot match, real Spot body lighting, atmospheric shafts, ordinary punctual lights, and `Hut_Warm_Point` remain unchanged. AF5H caches this controller-side mapping and reevaluates it only when the effective authored value or enable state changes; receiver shaders consume only the resolved global scale.

## V1.1D-AF5H — Accent evaluation cleanup and freeze preparation

### Objective and approved scope

AF5H is a bounded cleanup patch. It does not recalibrate AF5G. The active response remains:

```text
relativeScale(c) = c <= 0 ? 0 : 0.2 * (1001^c - 1)
```

Approved modified files:

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md
Assets/Docs/Stylized_Vegetation_Architecture.md
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl
```

Reviewed direct consumer: `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`. It is the only shader including `VegetationLighting.hlsl` and requires no direct edit.

### Invariants

- real URP Spot direction remains authoritative for vegetation body lighting;
- only the geometrically matched Weather Spot may substitute the horizontal Weather direction for the blade-edge selector;
- ordinary punctual lights and `Hut_Warm_Point` remain on the existing vegetation-local gain equation;
- `Accent Line Intensity = 0` disables only Weather-specific accent output, not Spot body lighting;
- the geometric threshold remains `0.999`;
- the Spot Rendering Layer mask remains the default receiver bit;
- no scene, prefab, material, renderer asset, Light count, pass, texture, buffer, shader variant, layer, tag, or serialized migration is added;
- one active authored LightRay remains the deliberate current vegetation-accent capacity.

### Planned implementation

1. Cache normalized and resolved accent values on the controller. Recompute `Mathf.Pow` only when the effective authored value or enabled state changes; keep shader-global publication allocation-free.
2. Gate vegetation geometric Spot matching before its subtraction/range/normalization/dot work when resolved production output is zero, while forcing the match path when the diagnostic suite is active.
3. Replace stale AF5B/AF5D current-state report and Inspector labels with AF5H while retaining `former AF5D maximum` solely as the mathematical reference-unit name.
4. Report the single supported published vegetation-accent Spot and clarify that storage capacity does not imply multi-zone matching.
5. Keep the new-controller source default at provisional `0.03`, report the current serialized value separately, and perform no serialized migration.
6. Reconcile current-state summaries in all three canonical documents and record static, compile, visual, and profiling evidence.

### Performance model

Before AF5H, the published controller evaluates one `Mathf.Pow` every update even when the slider is unchanged. AF5H moves that operation to dirty/change-triggered work. Before AF5H, `Accent Line Intensity = 0` still executes geometric matching for eligible vegetation punctual lights. AF5H bypasses that production work through a uniform early condition; diagnostics deliberately retain it. Nonzero production GPU cost and AF5G visual output remain unchanged.

### Validation and completion status

- canonical plan recorded before code: **complete**;
- controller cache implementation: **complete; static source audit passed**;
- zero-output shader bypass with diagnostic preservation: **complete; static source audit passed**;
- identifiers, reports, single-zone/default clarification: **complete; static source audit passed**;
- cross-subsystem include audit: **complete; only `SH_StylizedVegetationBenchmark.shader` includes the shared file and requires no direct edit**;
- static source audit: **complete; scope, symbol, current-label, include, and brace/parenthesis checks passed**;
- Unity C#/shader compilation: **pending user validation**;
- Unity visual and diagnostic validation: **pending user validation**;
- target-resolution profiling: **pending user validation**.


### AF5H implementation and static audit result

Actual modified files exactly match the approved six-file scope. No scenes, prefabs, materials, renderer assets, shader assets, layers, tags, Light counts, passes, textures, buffers, variants, or serialized defaults were changed.

Implemented source deltas:

- `WeatherLightRayController` caches the effective normalized value and resolved exponential scale; the sole `Mathf.Pow` call is reachable only through cache refresh after an effective-value change or explicit dirty mark;
- `OnEnable` and `OnValidate` dirty the cache, while effective-value comparison also detects `lightRaysEnabled` changes;
- reports consume the cached value and now identify AF5H, the AF5G response equation, the provisional `0.03` source default, the current serialized value, production/diagnostic matching state, and the exact one-Spot capacity;
- registration errors now explain why only one active authored LightRay is supported;
- `VegetationLighting.hlsl` bypasses production geometric matching at zero resolved output, but diagnostic mode forces matching;
- the punctual additional-light diagnostic path reuses the match already produced by `VegetationEvaluateDirectLight` when the receiver layer matches, avoiding duplicate geometry work; a separate diagnostic match remains only for the layer-mismatch case required to distinguish purple from orange;
- all current runtime, Inspector, action, and copied-report identifiers now use AF5H; `former AF5D maximum` remains only as the mathematical reference-unit name.

Cross-subsystem audit:

```text
VegetationLighting.hlsl direct including shaders: 1
Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader
Direct shader edit required: No
New shader keywords or variants: None
```

Static checks passed:

- final C# brace and parenthesis counts are balanced;
- all current-state AF5B/AF5D labels were removed from runtime and Editor source;
- the obsolete AF4 single-ray registration message was removed;
- exactly one `Mathf.Pow` call remains, inside `EvaluateSharedAccentLineRelativeScale` invoked by dirty cache refresh;
- all `VegetationMatchesWeatherLightRaySpot` callers were reviewed;
- final changed-file comparison against `Assets-Code-Archive(30).zip` reports exactly the approved six files.

Unity C# compilation, shader compilation, visual behavior, diagnostic colours, and target-resolution performance remain pending because Unity is unavailable in the patch environment.


## WEATHER-LIGHT-RAY-V1.1D-AI — Conserved atmospheric beam evolution

Status: source implementation complete; Unity compilation, visual validation, and profiling pending.

AI adds two Weather-controller-owned controls under Hybrid Renderer: **Beam Evolution Strength** (`0..1`, default `0.15`) and **Beam Evolution Speed** (`0..1`, default `0.25`). The feature evolves only atmospheric ribbons; it does not animate the real surface Spot, vegetation accents, beam count, area diameter, or authored contact plane.

Width evolution preserves both the sum of final beam widths and the sum of adjacent overlaps, keeping the covered span fixed. Intensity evolution renormalizes every frame to preserve the frozen sum of per-beam intensity weights. Left/right softness, peak bias, and side transmission use small opposed changes so the configuration shifts without a shared full-zone pulse. Strength `0` or speed `0` freezes temporal evolution.

The implementation uses deterministic seeded low-frequency oscillators and bounded vertex-path normalization over at most twelve beams. It adds no texture, buffer, render pass, draw call, managed allocation, or shader variant.

---

## WEATHER-LIGHT-RAY-V1.1D-AI1 — Per-Anchor Seeded Configuration Evolution

Status: implementation in progress; Unity validation pending.

AI1 replaces AI's controller-global oscillator modulation and the older disconnected Anchor evolution fields with one per-Anchor seeded configuration morph. Each Anchor owns an evolution preset, custom strength/speed, and starting variation seed. The renderer generates two complete deterministic beam configurations from Seed A and Seed B, normalizes each endpoint to the same total beam width and total intensity weight, then eases between them. Width and intensity conservation therefore hold throughout the interpolation. Beam count, outer footprint, ground-contact locations, real Spot lighting, and vegetation accent behavior remain unchanged.

Speed contract for Custom mode:

- 1.00 -> approximately 3 seconds per seed transition
- 0.75 -> approximately 6 seconds
- 0.50 -> approximately 12 seconds
- 0.25 -> approximately 24 seconds
- 0.00 -> frozen

The exact duration mapping is `3 * 2^(4 * (1 - speed))` seconds for nonzero speed. Built-in presets are Static, Subtle, Living, and Custom. The obsolete controller-owned Beam Evolution Strength/Speed controls and AI temporal oscillator code are removed rather than hidden.

---

## WEATHER-LIGHT-RAY-V1.1D-AI2A — Persistent seed evolution, controller defaults, and beam-quality guarantees

Status: implementation in progress; Unity validation pending.

Objective:

- replace AI1 render-time inferred seed progression with explicit per-runtime-slot state;
- continue seed transitions indefinitely through current/next seed promotion;
- add controller-owned evolution defaults with per-anchor override;
- preserve the exact 3/6/12/24-second speed mapping;
- guarantee visibly soft beam edges and substantial width variation for every seed;
- remove the obsolete AI1 absolute-time progression path.

Approved files:

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs`
- `Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs`
- `Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl`
- `Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayMask.shader`

Invariants:

- total beam width, outer footprint, total normalized intensity, beam count, Spot lighting, vegetation response, and ground-contact behavior remain unchanged;
- controller settings are defaults; an anchor may explicitly override preset/strength/speed while retaining its own seed sequence;
- no per-frame managed allocation, new pass, texture, render target, light, or shader variant;
- no generated beam side may fall below the universal softness floor;
- every configuration with at least three beams must contain a deliberate narrow-to-wide hierarchy.

Implementation sequence:

1. Extend descriptor/snapshot contracts with override state and resolved persistent evolution state.
2. Add controller default preset/strength/speed and compact Inspector exposure.
3. Add anchor override toggle and resolved-setting construction.
4. Advance explicit runtime-slot current/next seed, elapsed time, duration, blend, and transition counter in `TickController`.
5. Remove render-pass absolute-time seed inference and publish snapshot state directly.
6. Enforce deterministic ranked width spread, bounded intensity weights, and final edge-softness floors in shared HLSL.
7. Reconcile reports, live status, identifiers, and documentation.
8. Audit exact scope, all descriptor/snapshot constructors, shader parameter readers/writers, and obsolete AI1 symbols.

Acceptance:

- at speed 1, at least ten consecutive transitions complete within approximately 35 seconds without freezing;
- controller defaults affect non-overridden anchors and anchor override isolates the instance;
- speed values 1/0.75/0.5/0.25 resolve to approximately 3/6/12/24 seconds and 0 freezes;
- no tested seed produces hard mesh-like edges;
- 3-, 7-, and 12-beam configurations always show clear width hierarchy while preserving total width;
- recurring managed allocation remains 0 B/frame.

### AI2 implementation result

Status: source implementation complete; Unity compilation, runtime visual validation, and profiling pending.

Implemented behavior:

- each runtime slot now owns explicit current seed, next seed, elapsed transition time, resolved duration, eased blend, and completed-transition count;
- transition completion promotes the next seed and deterministically generates another destination inside a hitch-safe `while` loop;
- controller `Beam Evolution Defaults` supply preset/strength/speed to non-overridden anchors;
- anchors expose `Override Controller Evolution`; their `Variation Seed` remains independent in both modes;
- the render pass consumes snapshot seed state directly and no longer infers sequence indices from presentation time;
- generated width distributions combine a stable per-anchor ranked hierarchy with smaller destination-seed changes, preserving substantial variation even during adverse rank swaps;
- intensity endpoint weights are bounded before normalization and retain a constant total;
- generated and interpolated side softness is clamped to `max(0.055, baseFeather * 0.65)`;
- the AI1 absolute-time progression functions were removed.

Static evidence:

- speed equation returns exactly 3/6/12/24 seconds for 1/0.75/0.5/0.25 and freezes at zero;
- worst-case reversed dynamic width ranking retains an approximate 1.8 widest:narrowest hierarchy before restrained jitter for 3, 7, and 12 beams;
- C#/HLSL delimiter checks passed for all changed source files;
- no `ResolveSeedEvolution`, LightRay `AdvanceSeed`, or LightRay presentation-time sequence logic remains;
- actual changed scope is the architecture document plus seven source files; `SH_WeatherLightRayMask.shader` required no direct edit because it consumes the corrected shared include contract.


## AI2A — Render-authoritative perpetual seed clock

AI2A removes rendered evolution's dependency on mutable controller-side seed-promotion state. The mask include derives the current cycle, next cycle, and eased blend directly from Unity shader time, the resolved transition duration, and the Anchor variation seed. Consequently, each completed transition automatically enters the next deterministic seed pair; there is no promotion flag or elapsed accumulator in the rendering path that can stop after one cycle.

Controller runtime state now mirrors the same absolute-cycle contract only for Inspector status and diagnostics. A seed-derived phase offset prevents separate Anchors from sharing transition boundaries. Static or zero-speed configurations remain frozen. Existing width/intensity conservation and edge-softness guarantees remain unchanged.

---

## WEATHER-LIGHT-RAY-V1.1D-AI3A — Scalable architecture freeze and deterministic static baseline

**Status:** AI3A source implementation and static audit complete; Unity compilation and visual validation pending.

### Supersession

This section supersedes the LightRay atmospheric beam-evolution implementation described by AI, AI1, AI2, AI2A, AI2A1, and AI2A2. Those sections remain historical evidence only. Their shader-clock lifecycle, duplicate CPU status lifecycle, and fixed-loop seeded morph are not production architecture.

### Objective

AI3A removes the failed evolution execution paths while preserving the accepted static LightRay renderer. It establishes the exact architecture that AI3B must implement atomically:

- multiple LightRay zones may be visible in one camera frame;
- ordinary simultaneous zones share the one active celestial source: sun by day or moon by night;
- zones are not authored to overlap materially, but every visible zone contributes to one shared scalar `R16_SFloat` atmospheric mask;
- the shared mask is softened once and composited once per camera;
- each zone retains independent geometry, derived beam count, authored variation seed, future endpoint pair, transition phase, local intensity, and buffer range;
- CPU owns zone discovery, resolved settings, transition lifecycle, deterministic endpoint generation, dirty state, and publication versions;
- the renderer owns persistent dynamically growing `GraphicsBuffer` allocation, upload, binding, disposal, visibility iteration, mask accumulation, softening, and composite;
- GPU code reads endpoint A/B records and performs interpolation and rendering only;
- beam count remains derived from zone width and the maximum-centre-pitch policy;
- there is no accepted production beam-count ceiling;
- no separate diagnostic shader, synchronous GPU readback, or duplicate lifecycle is permitted.

### AI3A temporary compatibility boundary

The existing `2..12` layout, single-zone snapshot publication, mesh capacity, and fixed shader-loop capacity remain temporarily operational only so AI3A can provide a deterministic static comparison baseline. They are legacy blockers, not accepted design. AI3B removes them together in one atomic renderer/transport change. AI3A must not partially expose counts that the current mesh and shader cannot consume.

Serialized controller and Anchor evolution fields remain temporarily for data migration safety, but AI3A does not expose them as active Inspector controls and does not advance or render their failed lifecycle. The only atmospheric beam configuration rendered by AI3A is the deterministic configuration generated from the Anchor's authored `Variation Seed`.

### AI3A approved files

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs`
- `Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs`
- `Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl`

No scene, prefab, material, renderer asset, layer, tag, serialized default, shader variant, render target, or third-party dependency is approved for modification.

### AI3A implementation sequence

1. Record this canonical architecture and temporary compatibility boundary before code edits. **Complete.**
2. Replace controller lifecycle advancement with deterministic static authored-seed publication. **Complete; static source audit passed.**
3. Stop render-pass publication of changing seed/blend state. **Complete; static source audit passed.**
4. Remove shader-time lifecycle and force static authored-seed consumption. **Complete; static source audit passed.**
5. Remove failed evolution controls and live transition rows from the Controller and Anchor Inspectors while preserving serialized fields. **Complete; static source audit passed.**
6. Re-audit every changed producer, consumer, property writer/reader, shared-include consumer, and accepted LightRay/vegetation behavior. **Complete; shared include consumers reviewed.**
7. Run available static checks and record Unity validation as pending where Unity is unavailable. **Complete; Unity validation remains pending.**

### AI3A acceptance criteria

- no LightRay atmospheric evolution depends on `_Time`, `Time.realtimeSinceStartupAsDouble`, changing cycle seeds, or an independently calculated CPU status clock;
- the renderer publishes the authored variation seed as both endpoint seeds with blend `0`;
- current static beam count, area clamp, shape, softness, ground contact, atmospheric colour, surface Spot, and vegetation accent contracts remain unchanged;
- obsolete evolution controls and current/next seed transition rows are absent from the active LightRay Inspectors;
- no per-frame allocation, texture, buffer, pass, draw, shader variant, or readback is added;
- Unity 6000.5.0f1 C# and shader compilation, accepted visual comparison, Play-entry timing, and runtime allocation checks remain mandatory user validation.

### AI3B atomic replacement contract

AI3B removes every legacy `12` dependency together: authoring diameter clamp, CPU count clamp, mesh capacity, render-pass clamp, HLSL maximum and loops, and single-zone publication. It introduces reusable multi-zone CPU publication, renderer-owned persistent zone/beam buffers, CPU-generated endpoint A/B records, per-zone draws into the shared scalar mask, one softening sequence, one composite, and a two-action production-state audit (`Run`, `Copy`).


### AI3A implementation and static audit result

Actual modified files match the approved six-file AI3A scope. No scene, prefab, material, renderer asset, serialized default, layer, tag, render target, buffer, draw call, pass, or shader variant was added or changed.

Implemented deltas:

- `WeatherLightRayController.UpdateEvolutionState` now publishes the authored variation seed as current and next seed, with zero duration, zero blend, and zero completed transitions; it no longer reads realtime or advances cycles;
- `WeatherLightRayRenderPass.BuildShaderParameters` publishes the authored seed as both endpoints and publishes zero evolution strength, speed, duration, and blend;
- `WeatherLightRayCommon.hlsl` contains no LightRay `_Time` clock, phase offset, cycle-seed progression, or transition easing; static seed access resolves directly from the authored reference seed;
- the Controller Inspector no longer exposes failed Beam Evolution defaults or transition status rows;
- the Anchor Inspector replaces the failed evolution foldout with one `Beam Variation` foldout containing only `Variation Seed` and an explicit AI3A migration notice;
- legacy serialized evolution fields and snapshot fields remain intact only to avoid destructive serialized migration before AI3B.

Static audit evidence:

- targeted search found no LightRay `WeatherLightRayGetEvolutionClock`, `EvolutionPhase01`, `EvolutionSeedForCycle`, LightRay `NextEvolutionSeed`, or `_Time.y` lifecycle reference;
- `SH_WeatherLightRayMask.shader`, `SH_WeatherLightRayScatter.shader`, and `SH_WeatherLightRayComposite.shader` remain the direct consumers of `WeatherLightRayCommon.hlsl`; no direct edits were required outside the common include;
- the temporary `MaximumBeamCount = 12`, matching HLSL capacity, single-zone snapshot path, and current mesh contract remain intentionally unchanged until atomic AI3B replacement;
- Unity C# compilation, shader compilation, visual comparison, Play-entry timing, and runtime allocation validation are pending because Unity is unavailable in the patch environment.

## WEATHER-LIGHT-RAY-V1.1D-AI3B — Atomic scalable multi-zone evolution implementation

**Status:** Implemented; Play Mode continuous evolution user-validated. Superseded as the active patch status by AI3C final freeze.

### Objective

Replace the temporary AI3A one-zone/12-beam static compatibility path in one atomic production change. AI3B must support every active compatible LightRay zone, derive beam count from authored diameter and the 0.60 m maximum centre-pitch policy without an artistic hard cap, generate deterministic endpoint A/B records on the CPU at dirty boundaries, interpolate those endpoints on the GPU, and accumulate all visible zones into the existing shared scalar atmospheric mask before one softening sequence and one composite.

### Approved files

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs`
- `Assets/Game/Rendering/Weather/WeatherLightRayRendererFeature.cs`
- `Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs`
- `Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl`
- `Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayMask.shader`

### Reviewed evidence

- `WeatherLightRayAreaLayout` clamps diameter and count through `MaximumBeamCount = 12`.
- `WeatherLightRayRendererFeature` requests one primary renderable snapshot.
- `WeatherLightRayRenderPass` stores one snapshot, builds one count-specific mesh, publishes one set of zone globals, and draws once.
- `WeatherLightRayCommon.hlsl` clamps count to 12 and performs fixed unrolled seeded ranking and normalization loops.
- The controller already owns one runtime slot and one real surface Spot proxy per active slot; the previous authored-registration rejection, not the Spot storage, prevents simultaneous authored zones.

### Invariants

- No scene, prefab, material, renderer asset, layer, tag, or project-setting edits.
- All ordinary simultaneous zones use the same active sun-or-moon source contract and one shared scalar mask.
- Zone identity, lifecycle timing, seeds, and transition progression remain CPU authoritative.
- Renderer-owned persistent `GraphicsBuffer` instances grow geometrically and are reused.
- Endpoint data uploads only when zone membership, beam count, or endpoint seeds change. The small zone-state buffer may update once per rendered frame for current blend values.
- No synchronous GPU readback, wait, or blocking diagnostic shader.
- Ground-contact fade, real surface Spot lighting, vegetation accent behavior, atmospheric colour, and existing composite remain unchanged except for supporting more than one zone.

### Implementation sequence

1. Remove the authored one-zone registration rejection and restore controller/Anchor evolution controls.
2. Replace static AI3A seed publication with duration-mapped CPU transitions and deterministic seed advancement.
3. Remove the 12-beam diameter/count clamp and derive count from diameter/pitch with numeric-overflow protection only.
4. Replace one-primary-snapshot renderer setup with reusable multi-snapshot publication.
5. Replace count-specific meshes with procedural six-vertex quads indexed by `SV_VertexID`.
6. Add renderer-owned persistent beam endpoint and zone-state buffers with geometric capacity growth and explicit disposal.
7. Generate endpoint A/B records on the CPU only when the endpoint signature changes; upload the small zone-state records per rendered frame.
8. Draw every compatible active zone into one mask pass; soften once and composite once.
9. Replace HLSL seeded loops with direct structured-buffer endpoint loads and interpolation.
10. Audit all shared-include consumers and package only approved source files.

### Acceptance criteria

- A 40 m diameter resolves to 68 beams under the 0.60 m maximum pitch policy.
- No CPU, Inspector, mesh, render-pass, or HLSL dependency on a 12-beam maximum remains.
- Two or more active authored zones register, retain independent seeds/transitions/buffer ranges, and render in the same frame.
- Endpoint buffer allocation occurs only on initialization/growth; endpoint upload occurs only when membership/count/seeds change.
- One mask clear/draw pass contains all compatible zone draws; softening and composite each execute once.
- No managed allocation after stable capacities warm up in the production update/render path.
- Unity compilation, shader compilation, Play Mode visual validation, multi-zone validation, wide-zone validation, and Profiler evidence remain required before freeze.

### AI3B implementation and static audit result

**Source status:** Implemented and statically audited. Play Mode continuous evolution was subsequently user-validated. AI3C freezes the architecture and retains project-side multi-zone audit and profiling as validation evidence, not planned architecture work.

Implemented deltas:

- `WeatherLightRayAreaLayout.Calculate` now preserves the authored diameter and derives `ceil(diameter / 0.60) + 1` beams, with finite-input and integer-overflow protection only; the former `MaximumBeamCount` and `MaximumDiameterMetres` contracts are removed.
- `WeatherLightRayAnchor` no longer applies a maximum diameter Range/clamp.
- authored registration no longer rejects a second active Anchor; the existing slot array and per-slot real surface Spot storage remain authoritative.
- controller evolution is CPU authoritative again: endpoints advance through deterministic xorshift seeds, speed uses `3 × 2^(4 × (1-speed))`, and smoothstep blend is published in the immutable snapshot.
- Controller and Anchor Inspectors expose controller defaults, per-Anchor override, preset/custom strength/speed, and the independent variation seed.
- the Renderer Feature publishes a reusable snapshot array rather than one primary render snapshot.
- the Render Pass owns persistent geometrically growing beam and zone `GraphicsBuffer` instances, stable CPU staging arrays, explicit disposal, frustum-tested draw selection, and endpoint-signature dirty uploads.
- active compatible zones retain stable buffer ranges independent of camera visibility; entering/leaving the frustum changes only the visible draw list and the small zone-state upload.
- the mask pass issues one procedural draw per visible zone into one shared `R16_SFloat` target; softening and composite remain one pass each per camera.
- the mask shader uses `SV_VertexID` to generate six vertices per beam and reads endpoint A/B records through structured buffers; all fixed-cap ranking, normalization, and unrolled beam loops are removed.
- endpoint generation normalizes each endpoint's covered span to the exact authored diameter and its intensity sum to the exact beam count. Linear interpolation therefore preserves both endpoint totals throughout the transition.
- the runtime audit exposes exactly two actions, `Run Beam Evolution Runtime Audit` and `Copy Beam Evolution Runtime Audit`, and reports active snapshots, visible/buffered zones, total beams, buffer capacities, upload counters, seeds, blend, duration, transition count, pitch, and intensity without GPU readback.

Static audit evidence:

- targeted source search found no `MaximumBeamCount`, `MaximumDiameterMetres`, `WEATHER_LIGHT_RAY_MAX_BEAMS`, `2..12` render clamp, count-specific `DrawMesh`, or fixed HLSL beam loop in the production LightRay paths;
- the only beam draw is procedural and uses `beamCount × 6` vertices;
- the mask include's structured-buffer declarations are guarded by `WEATHER_LIGHT_RAY_ENABLE_BEAM_BUFFER`, so scatter and composite consumers retain their existing shader target and do not require buffer bindings;
- C# `BeamRecord` is six `Vector4` values (96 bytes), matching the HLSL record's six `float4` values; `ZoneRecord` is one `Vector4` (16 bytes), matching the HLSL `float4` buffer element;
- a 40 m diameter resolves mathematically to `ceil(40 / 0.60) + 1 = 68` beams;
- numerical endpoint checks at 2, 8, 68, and 1000 beams preserve left/right boundaries at `-diameter/2` and `+diameter/2` within floating-point tolerance and preserve intensity sums at the beam count within floating-point tolerance;
- no scene, prefab, material, renderer asset, layer, tag, project setting, or third-party dependency was modified.


## WEATHER-LIGHT-RAY-V1.1D-AI3C — Final cleanup and architecture freeze

**Status:** Accepted production architecture. Play Mode evolution has been user-validated; final Unity compilation, multi-zone audit capture, and Profiler measurements remain project-side validation evidence rather than blockers to the architectural freeze.

### Objective

Freeze the AI3B production architecture after successful user validation that LightRay beams now evolve continuously over time. AI3C performs only identifier, documentation, and stale-status cleanup. It does not introduce another rendering architecture, quality tier, distance policy, or speculative optimization.

### Approved files

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs`
- `Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs`

### Reviewed evidence

- User validation confirms the AI3B replacement solved the original defect: atmospheric LightRay beams now visibly and continuously evolve over time in Play Mode.
- Static source audit already confirmed removal of the 12-beam CPU/HLSL limit, fixed HLSL beam loops, count-specific mesh drawing, and single-zone renderer dependence.
- The production renderer maintains stable active-zone buffer ranges and uses camera-frustum rejection only to avoid issuing draws for zones outside the camera.
- Legacy `TryGetPrimaryRenderableRay` remains used by focused reports and vegetation/surface diagnostics; it is not part of renderer publication and is intentionally retained.

### Frozen camera and scale policy

The game uses a close isometric top-down camera. Every LightRay zone visible to that camera is valid production content. Therefore the frozen architecture has:

- no distance-based beam-count LOD;
- no distant-zone freeze or reduced update cadence;
- no distance-based endpoint suppression;
- no indirect-rendering requirement;
- no hidden quality fallback that changes authored beam density.

Camera-frustum rejection remains permitted and required: a zone completely outside the camera does not need an atmospheric draw. This is ordinary visibility rejection, not a distance LOD or distant-zone policy. Active offscreen zones retain their authoritative lifecycle and stable buffer range, so entering the camera does not reset or regenerate their evolution merely because visibility changed.

### Final implementation state

- CPU owns per-zone lifecycle, deterministic seed advancement, endpoint generation, normalization, dirty state, and immutable snapshot publication.
- Renderer-owned persistent beam and zone `GraphicsBuffer` instances grow geometrically and are reused.
- GPU reads endpoint A/B records and interpolates them with the CPU-published blend.
- All visible compatible zones draw into one shared scalar atmospheric mask.
- Softening executes once per camera and composite executes once per camera.
- At the AI3C freeze, beam count remained derived from authored diameter and the fixed 0.60 m maximum centre-pitch policy. AI4A supersedes that fixed policy with direct authored Beam Spacing while retaining derived count and no artistic hard cap.
- Runtime diagnostics remain limited to `Run Beam Evolution Runtime Audit` and `Copy Beam Evolution Runtime Audit`; no diagnostic shader or GPU readback exists.
- No scene, prefab, material, renderer asset, layer, tag, project setting, or third-party dependency is changed by AI3C.

### Non-goals

- distance LOD;
- distant-zone freezing;
- reduced update cadence by camera distance;
- indirect drawing;
- GPU endpoint generation;
- a separate Edit Mode evolution driver;
- new artistic controls or changed defaults.

### Freeze decision

AI3B remains the accepted production LightRay evolution architecture and AI3C closed that implementation line. The subsequently demonstrated seed-composition and granularity defect is addressed separately by AI4A without reopening the AI3 transport architecture.

---

## WEATHER-LIGHT-RAY-V1.1D-AI4A — Beam granularity and per-seed composition

### Objective

Correct the accepted AI3C renderer's remaining seed-composition defect without changing its lifecycle, persistent-buffer, multi-zone, shared-mask, contact, vegetation, or real Spot-light architecture.

The current fixed 0.60 m centre-pitch policy produces too many individually readable shafts in small authored zones, while endpoint generation normalizes independently uniform random width and intensity weights too strongly. AI4A exposes the pitch as a direct per-Anchor authoring control and changes endpoint generation to produce a stronger unequal hierarchy between beams.

### Acceptance criteria

- `Beam Spacing` is a direct per-Anchor metre control.
- Resolved beam count remains derived from area diameter and spacing: `ceil(diameter / spacing) + 1`, with a minimum of two beams and no artistic maximum.
- The default spacing is 1.05 m, preserving the previously serialized legacy spacing value and yielding approximately 5–7 beams for the common few-metre authored zone.
- Wider zones automatically resolve to proportionally more beams; no fixed 4–7 limit is introduced.
- Each deterministic seed produces substantially broader width, intensity, transparency, and left/right profile variation than AI3C.
- Weak beams may become genuinely faint; one or more beams may dominate a seed.
- Total endpoint brightness remains bounded, but is not forced to an exact per-beam mean of 1.0.
- Endpoint A/B interpolation remains continuous and uses the existing AI3 buffer records and shader path.
- No new distance LOD, distant-zone freeze, indirect rendering, diagnostic shader, GPU readback, scene edit, prefab edit, material edit, layer, tag, or project setting is introduced.

### Approved files

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs`
- `Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs`

### Reviewed evidence

- `WeatherLightRayAreaLayout.Calculate(float)` uses the fixed `MaximumCentrePitchMetres = 0.60f`, so count is not author-controllable.
- `WeatherLightRayAnchor` retains a hidden serialized legacy spacing value of 1.05 m, which can be promoted without losing existing serialized data.
- `WeatherLightRayRenderPass.GenerateEndpoint` currently uses independent uniform random width and intensity samples and then scales total intensity to exactly `beamCount`, compressing visual hierarchy between beams.
- AI3C's persistent buffers, procedural draw path, shared mask, softening, composite, and evolution lifecycle are accepted and remain unchanged.

### Implementation sequence

1. Promote the preserved spacing value into the direct `Beam Spacing` authoring field and pass it through the descriptor/layout contract.
2. Update all layout displays and reports to use the resolved spacing.
3. Replace uniform width/intensity sampling with deterministic skewed distributions, correlated dominance, stronger asymmetric softness/transmission, and bounded endpoint-level total-energy variation.
4. Keep endpoint record stride and HLSL consumption unchanged.
5. Audit every `WeatherLightRayAreaLayout.Calculate` caller and the final diff.

### Risks and invariants

- Existing serialized Anchors must retain their preserved 1.05 m spacing value.
- Width layout must continue to cover the exact authored diameter endpoints.
- Intensity variation must not create negative values or uncontrolled brightness spikes.
- Evolution strength zero freezes progression on the authored deterministic seed; it does not flatten the seed's width, intensity, transparency, or asymmetry composition.
- No per-frame allocations or additional buffer uploads may be introduced.

### Status

- Plan: implemented.
- Implementation: AI4A source complete and statically audited.
- Unity validation: pending in Unity 6000.5.0f1 URP.

---

# WEATHER-LIGHT-RAY-V1.2A — Preset Authority, Asset Creation, and Authoring Cleanup

**Status:** Implementing under explicit user approval.  
**Visual baseline:** `WEATHER-LIGHT-RAY-V1.1D-AI4A`, accepted and frozen.  
**Scene policy:** `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity` and the separately supplied scene file are read-only evidence. This patch must not modify or assign scene objects.

## Objective

Replace the ambiguous three-level visual-authoring hierarchy with one authoritative `WeatherLightRayPreset` ScriptableObject. The controller selects one global preset; authored Anchors retain only placement, lifecycle, seed, and local intensity/geometry overrides. All simultaneous rays share the active preset because the atmospheric renderer accumulates them into one shared scalar mask and performs one global soften/composite sequence.

## Approved acceptance criteria

- Create the preset type, custom Inspector, preset catalog, eight preset assets, and catalog asset.
- `Sun — Warm` must reproduce the accepted values serialized in the supplied `VisualFrameworkDemo.unity` evidence.
- The Controller must expose preset selection, runtime capacity, operational state, and diagnostics—not duplicate visual controls.
- The Anchor must expose placement, lifecycle, seed, local intensity, and an explicit optional local spacing override—not shared visual controls.
- Shared composite and beam-generation settings must resolve from the Controller's active preset, never from first-visible-zone slot order.
- Existing serialized Controller and Anchor fields remain for migration compatibility but are hidden from normal authoring and ignored when a valid active preset is assigned.
- No scene, prefab, material, renderer asset, shader, layer, tag, or project setting may be modified.
- No new per-frame allocation, render pass, render target, GPU readback, or shader variant may be introduced.

## Supplied-scene evidence for `Sun — Warm`

The uploaded scene serializes the accepted Anchor values as:

- Source: Sun
- Height: `27.7 m`
- Maximum visual lean: `25.2°`
- Area diameter: `4.91 m`
- Beam spacing: `0.95 m`
- Beam width ratio: `1.00–1.25`
- Beam intensity variation: `0.60`
- Beam edge softness: `0.45`
- Beam softness variation: `0.45`
- Upper fade: `0.49`
- Ground fade: `0.20`
- Contact-plane opacity: `0.15`
- Colour multiplier: `(0.9811321, 0.9021989, 0.66180134, 1)`
- Warmth contribution: `0.85`
- Atmospheric intensity: `0.20`
- Softening strength: `0.55`
- Camera-intersection fade: `0.92`
- Surface Spot intensity: `0.40`
- Screen-space surface intensity: `0.00`
- Footprint edge softness: `1.00`
- Evolution: Subtle, strength `0.35`, speed `0.25`

The Controller evidence supplies shared vegetation accent defaults:

- Accent-line intensity: `0.40`
- Vegetation accent coverage: `0.30`

Legacy serialized values retained in the preset schema for compatibility/reference are ground-light `6.34`, surface-light `0.03`, cloud compensation `8.76`, and core emphasis `7.24`; these are not separate active renderer paths in AI4A.

## Preset assets

Create under `Assets/Game/Demo/Profiles/Weather/LightRays/`:

- `WeatherLightRay_SunWarm.asset`
- `WeatherLightRay_SunClear.asset`
- `WeatherLightRay_SunIntense.asset`
- `WeatherLightRay_SunHazy.asset`
- `WeatherLightRay_MoonCold.asset`
- `WeatherLightRay_MoonWhite.asset`
- `WeatherLightRay_MoonSubtle.asset`
- `WeatherLightRay_BloodMoon.asset`
- `WeatherLightRayPresetCatalog.asset`

`Sun — Warm` is the migration/reference preset. Other assets are editable starting configurations, not frozen art direction.

## Runtime ownership

- `WeatherLightRayPreset`: all shared appearance, beam composition, evolution, surface response, and default spawn geometry.
- `WeatherLightRayController`: active preset, catalog, capacity, source binding, population, publication, diagnostics.
- `WeatherLightRayAnchor`: authored placement, policies/lifecycle, variation seed, local intensity multiplier, optional local beam-spacing override.
- Renderer: consumes Controller-resolved shared settings and per-zone local geometry/state.

## Missing-preset compatibility

A valid active preset is authoritative. When absent, legacy serialized fields remain the compatibility fallback and the Controller Inspector must report the missing assignment clearly. This patch does not assign a preset in the scene.

## File scope

### Create

- `Assets/Game/Procedural/Weather/WeatherLightRayPreset.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayPresetCatalog.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayPresetEditor.cs`
- eight preset assets and one catalog asset under `Assets/Game/Demo/Profiles/Weather/LightRays/`, with required `.meta` files

### Modify

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs`
- `Assets/Game/Rendering/Weather/WeatherLightRayRendererFeature.cs`
- `Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs`

## Non-goals

- No procedural spawn API in V1.2A.
- No cloud-clearance provider or weather-selection logic.
- No scene assignment or migration write.
- No per-ray preset override in the normal workflow.
- No renderer/shader redesign.

## Validation status

- Source implementation: pending.
- Static consistency audit: pending.
- Unity 6000.5.0f1 compilation and runtime validation: pending project-side execution.

## V1.2A implementation result

**Source status:** Implemented; Unity compilation and runtime validation remain pending.

The complete review proved that no `WeatherLightRayTypes`, renderer-feature, render-pass, HLSL, or shader modification was required. Preset authority is applied while the Controller rebuilds each active authored descriptor; therefore all published snapshots already carry identical shared presentation values before reaching the unchanged renderer. This removes first-visible-zone ambiguity without changing the validated AI4A GPU transport.

### Actually affected source files

Created:

- `Assets/Game/Procedural/Weather/WeatherLightRayPreset.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayPresetCatalog.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayPresetEditor.cs`
- eight preset assets and `WeatherLightRayPresetCatalog.asset`, with `.meta` files

Modified:

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs`

### Implemented behaviour

- A valid Controller `Active Preset` replaces all shared Anchor/Controller appearance and evolution values.
- Missing preset assignment retains the old serialized values as an explicit compatibility fallback.
- Anchor-local intensity always multiplies the resolved atmospheric intensity.
- Anchor-local beam spacing applies only when `Override Preset Beam Spacing` is enabled.
- Preset switches can be immediate or presentation-blended through `TrySetActivePreset`; colour, warmth, atmospheric intensity, softening, camera fade, surface response, and vegetation-accent values blend globally without resetting ray handles or evolution state.
- Endpoint-generation properties switch once at the preset boundary and are not interpolated every frame, avoiding repeated endpoint uploads during presentation blending.
- The Controller and Anchor Inspectors no longer expose duplicate shared visual/evolution controls.
- No scene assignment was performed.

### Static audit result

- C# delimiter balance passed for every affected source file.
- Preset/catalog asset GUID references are internally consistent.
- `Sun — Warm` values match the supplied scene-value ledger.
- No render pass, render target, shader, buffer layout, or per-frame collection was added.
- Unity 6000.5.0f1 compile and visual migration validation remain pending project-side execution.

---

# WEATHER-LIGHT-RAY-V1.2B — Direct Procedural Runtime Population

## Status

Implementation authorized on 2026-07-28. Unity runtime validation is pending.

## Objective

Add a GameObject-free procedural LightRay population API on top of the accepted V1.2A preset authority and AI4A rendering architecture. The capability must spawn, update, validate, and release stable runtime handles without changing the renderer, shader buffer layout, preset assets, scene, authored Anchor workflow, or cloud-query logic.

## Approved files

Modify:

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs`

Create:

- none planned.

## Reviewed evidence

- `WeatherLightRayController.RuntimeSlot` currently owns authored Anchor identity, lifecycle, evolution, and snapshots.
- `TryRegisterOrUpdateAuthoredRay` allocates stable generation-checked slots.
- `UpdateRegisteredRays` currently releases any active slot without an enabled authored Anchor.
- `WeatherLightRayDescriptor` already contains every renderer-facing local and shared parameter required by a procedural ray.
- `WeatherLightRayPreset.ApplyTo` already resolves the active global preset plus local intensity and optional beam-spacing override.
- `WeatherLightRayHandle` already provides stable slot/generation validation.

## Accepted contracts

### Runtime API

The controller exposes:

- `TrySpawnProceduralRay`
- `TryUpdateProceduralRay`
- `TrySetProceduralRayVisible`
- `TryReleaseProceduralRay`
- `IsValid`

### Spawn request

A procedural request owns only local/runtime data:

- base centre;
- local ray direction override, or active source direction when omitted;
- area diameter;
- optional height, lean, and beam-spacing overrides;
- variation seed;
- local intensity multiplier;
- cloud policy and source-gate policy;
- lifecycle policy and fade/hold durations;
- gameplay channel;
- movement policy;
- external identity and priority metadata.

Shared colour, softness, beam composition, surface response, and evolution come from the controller's active preset.

### Lifecycle

- `Timed`: fades in, holds, fades out, and automatically releases after expiry.
- `Permanent`: remains until explicitly released.
- `ExternallyControlled`: visibility is driven by `TrySetProceduralRayVisible` or an update request and uses fade durations for transitions.

### Capacity

Capacity exhaustion is explicit and non-destructive. No existing slot is silently replaced. The returned error includes active count and capacity. Priority is stored for the future phenomenon but does not trigger automatic eviction.

### Performance

- No authored Anchor GameObject or authoring component is created for procedural rays. Existing controller-owned surface Spot proxies remain unchanged.
- No managed allocation occurs per active procedural ray per frame.
- Existing slot arrays, snapshot staging, beam buffers, and zone buffers remain reused.
- Procedural descriptor resolution is allocation-free and `O(active procedural rays)` so live preset edits and preset blends remain authoritative. Expensive endpoint regeneration remains protected by the renderer's existing dirty signature.
- The existing per-frame lifecycle and small zone-buffer update remain unchanged in complexity.

## Non-goals

- no cloud provider or cloud-opening query contract;
- no automatic weather selection;
- no candidate ranking, hysteresis, or population scheduler;
- no indirect rendering, LOD, distant-zone freezing, or renderer/shader changes;
- no scene, prefab, material, layer, tag, or project-setting modifications.

## Implementation sequence

1. Add immutable procedural spawn/update request contracts and lifecycle metadata.
2. Generalize runtime slots to distinguish authored and procedural ownership.
3. Add allocation, validation, update, visibility, and release APIs.
4. Route authored and procedural slots through the same source gate, cloud gate, lifecycle, evolution, snapshot, surface-light, and renderer paths.
5. Automatically release expired timed procedural slots.
6. Add concise procedural population information to the existing runtime audit/report.
7. Re-audit handle generation, slot release, preset resolution, allocations, and unchanged authored behavior.

## Acceptance criteria

- Two procedural rays can be spawned without Anchor GameObjects.
- Returned handles remain valid until release or timed expiry.
- Updating position, size, direction, intensity, seed, and lifecycle data does not replace the handle.
- Externally controlled visibility fades without respawning.
- Timed rays release automatically after fade-in + hold + fade-out.
- Capacity exhaustion rejects the new request without modifying active rays.
- Authored Anchors continue to register, update, and release unchanged.
- All procedural rays inherit the active global preset.
- No renderer/shader/buffer-layout changes and no recurring managed allocation are introduced.

## Validation status

- Static source audit: pending.
- Unity C# compilation: pending.
- Play Mode procedural spawn/update/release: pending.
- GC and capacity validation: pending.

## V1.2B implementation audit

### Implemented files

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs`

No new file, scene, prefab, material, preset asset, renderer asset, shader, layer, tag, or project setting was modified.

### Implemented behaviour

- Added immutable spawn and update request contracts.
- Added stable direct spawn, update, visibility, release, and handle-validation APIs.
- Added authored/procedural slot ownership without changing handle generation.
- Routed procedural slots through the existing preset, source gate, optional cloud transmission, lifecycle, evolution, surface Spot, snapshot, buffer, and renderer paths.
- Timed procedural slots automatically release after expiry.
- Capacity exhaustion rejects only the new request and reports used/capacity values.
- Runtime Inspector and audit output distinguish authored and procedural populations.
- Added finite/range validation for all procedural spatial, intensity, override, and lifecycle inputs.

### Intentional performance detail

Procedural descriptor resolution executes once per active procedural slot per controller tick. It constructs value types only and performs no managed allocation. This is required so live preset edits and bounded preset transitions remain authoritative. Endpoint generation and GPU endpoint uploads remain controlled by the existing renderer dirty signature and are not forced every frame by position, lifecycle, or presentation-only changes.

### Static verification

Passed:

- final source scope matches the four approved files;
- C# brace and parenthesis balance for all modified source files;
- no trailing whitespace in modified source files;
- no renderer, shader, structured-buffer, render-target, or draw-contract change;
- no per-ray Anchor GameObject/component creation path;
- stale handles are rejected by slot generation;
- authored release cannot match a procedural slot through a null Anchor;
- timed zero-duration requests are rejected before allocation;
- invalid finite/range inputs are rejected before slot mutation.

Pending:

- Unity 6000.5.0f1 C# compilation;
- Play Mode spawn/update/visibility/release behaviour;
- timed automatic expiry;
- capacity rejection under a full slot array;
- Profiler confirmation of zero recurring managed allocation after warmup.

## WEATHER-LIGHT-RAY-V1.2B1 — Preset/source separation and procedural smoke-test hotfix

### Objective

Correct two V1.2A/V1.2B regressions before cloud-aware work:

1. An appearance preset must not overwrite an authored or procedural ray's actual Sun/Moon source binding. Assigning a Moon or Blood Moon appearance asset while the current authored ray is Sun-backed must change appearance only; it must not make the ray unavailable merely because the separate Moon source implementation does not yet exist.
2. The direct procedural API must have a complete Inspector-driven validation path. Users must not need to invent a temporary MonoBehaviour to call `TrySpawnProceduralRay`.

### Approved files

Modify:

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayPreset.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs`
- the eight V1.2A preset assets under `Assets/Game/Demo/Profiles/Weather/LightRays/`

No scene, prefab, material, renderer, shader, layer, tag, or project-setting change is approved.

### Contract correction

- `WeatherLightRayPreset.SourceKind` is catalog/authoring metadata describing the intended family of the appearance asset. It is not runtime source ownership.
- Authored rays retain `WeatherLightRayAnchor.SourceKind`.
- Procedural spawn requests explicitly carry `WeatherLightRaySourceKind`, defaulting to Sun for source compatibility.
- `WeatherLightRayPreset.ApplyTo` preserves `localDescriptor.SourceKind`.
- A Blood Moon preset may therefore be previewed on a currently Sun-backed authored ray without becoming invisible. A future weather phenomenon will pair the same appearance preset with a Moon-backed procedural request when a real Moon source exists.

### Inspector smoke-test contract

`WeatherLightRayController > Actions & Reports` receives one Play-Mode-only toggle action:

- `Spawn Procedural Test Pair` when no valid test pair exists.
- `Release Procedural Test Pair` while the pair exists.

The action calls the real production APIs, creates two externally controlled procedural rays beside the primary authored Anchor (or beside the Controller when no Anchor exists), and logs both handles. It introduces no runtime component, scene object, or serialized state.

### Warning cleanup

The four unused `legacy*` fields in `WeatherLightRayPreset` and their serialized keys in the newly created preset assets are removed. They were never read by production code and were not migration fields from an older preset schema.

### Acceptance criteria

- Assigning `Moon — Blood Moon` changes the visible appearance of the existing Sun-backed Anchor without changing its source kind or hiding it.
- The source status continues to report the Anchor as Sun-backed unless its own source binding is deliberately changed.
- The Controller Inspector can spawn two distinct procedural handles and release them through one explicit toggle action.
- `Central Storage > Procedural Slots` changes from 0 to 2 and back to 0.
- No CS0414 warnings remain from `WeatherLightRayPreset`.
- No renderer, shader, buffer, lifecycle, or evolution behaviour changes.

### Status

Implementation in progress. Unity compilation and runtime validation pending.

### V1.2B1 implementation audit

Implemented:

- Preset application preserves each ray descriptor's source kind.
- Procedural spawn requests carry an explicit source kind; existing callers default to Sun.
- The Controller's procedural descriptor builder uses the request source kind rather than the appearance preset family.
- `Actions & Reports` contains one Play-Mode-only procedural test-pair toggle that calls the production spawn/release APIs directly.
- The four unused preset legacy fields and their keys in all eight generated preset assets were removed.
- The comprehensive report label now identifies V1.2B1.

Static audit:

- No renderer, shader, buffer layout, scene, prefab, material, layer, tag, or project-setting file changed.
- C# delimiter counts are balanced in every modified source file.
- No remaining source or asset reference exists to the removed four legacy preset fields.
- No production call still derives procedural source kind from `activePreset.SourceKind`.
- Unity compilation and runtime validation remain pending in Unity 6000.5.0f1.

## WEATHER-LIGHT-RAY-V1.2C — Cloud-aware capability contracts

### Objective

Add cloud-aware LightRay spawning contracts without implementing cloud detection, weather selection, candidate ranking, scheduling, or hysteresis. Cloud systems resolve openings; the LightRay controller converts accepted openings into the existing GameObject-free procedural runtime path.

### Approved files

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs`

### Contracts

- `IWeatherLightRayCloudClearanceProvider` resolves one query into one `WeatherLightRayCloudOpening`.
- `WeatherLightRayCloudQuery` carries source, search origin/direction, desired diameter bounds, identity hint, and minimum confidence.
- `WeatherLightRayCloudOpening` carries stable identity, centre, direction, diameter, clearance strength, edge-softness signal, confidence, and data version.
- `WeatherLightRayCloudSpawnSettings` converts an opening into an ordinary `WeatherLightRaySpawnRequest` while inheriting the active global preset.
- `TrySpawnCloudAwareRay` queries a provider once and spawns only when resolution succeeds.
- `TrySpawnFromResolvedCloudOpening` bypasses provider work when the caller already resolved an opening.
- `TrySpawnOrUpdateResolvedCloudOpening` preserves an existing valid handle and updates it in place; invalid handles spawn a new ray.
- `TryUpdateFromResolvedCloudOpening` updates a known procedural handle without resetting lifecycle unless explicitly requested.

### Invariants

- Both cloud-aware paths terminate in the existing V1.2B procedural spawn/update APIs.
- No GameObject or Anchor is created.
- No cloud texture, simulation, or weather-state selection is implemented here.
- Stable opening identity is copied to the procedural request `ExternalIdentity`.
- Clearance strength multiplies the configured local intensity and is clamped to `[0,1]`.
- Opening diameter and optional spacing remain geometric inputs; shared appearance remains active-preset-owned.
- Capacity exhaustion rejects only the new spawn and never evicts an existing ray.
- No renderer, shader, buffer, scene, prefab, material, layer, tag, or project-setting change.

### Validation support

The Controller Inspector receives one Play Mode action that uses an in-memory deterministic provider stub and the production cloud-aware APIs. It spawns one resolved opening, updates the same handle to a moved opening, then releases it. This is validation tooling only and creates no scene object.

### Non-goals

- Automatic cloud-opening detection.
- Population scheduling or prioritization.
- Spawn/despawn hysteresis.
- Sun-versus-moon weather selection.
- Cloud-field GPU readback.
- Per-frame scans or managed allocation paths.

### Status

Implementation in progress. Unity compilation and runtime validation pending.

### V1.2C implementation audit

Implemented files match the approved scope:

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs`

Implemented production contracts:

- `IWeatherLightRayCloudClearanceProvider.TryResolveOpening`.
- `WeatherLightRayCloudQuery`.
- `WeatherLightRayCloudOpening`.
- `WeatherLightRayCloudSpawnSettings`.
- `WeatherLightRayController.TrySpawnCloudAwareRay`.
- `WeatherLightRayController.TrySpawnFromResolvedCloudOpening`.
- `WeatherLightRayController.TryUpdateFromResolvedCloudOpening`.
- `WeatherLightRayController.TrySpawnOrUpdateResolvedCloudOpening`.

The opening conversion copies stable identity into `ExternalIdentity`, multiplies local intensity by clearance strength once, preserves source kind, and routes through the existing V1.2B procedural validation, slot, lifecycle, evolution, surface-light, snapshot, and rendering path. Resolved openings default to `IgnoreClouds` through spawn settings so an already accepted opening is not implicitly sampled and attenuated a second time; callers may explicitly request the existing runtime cloud policy.

The Controller Inspector includes one three-state Play Mode smoke-test action:

1. `Spawn Cloud-Aware Test Ray` resolves through a deterministic in-memory provider and calls `TrySpawnCloudAwareRay`.
2. `Update Cloud-Aware Test Ray` moves and resizes the resolved opening through `TrySpawnOrUpdateResolvedCloudOpening` and verifies the handle remains unchanged.
3. `Release Cloud-Aware Test Ray` calls the production release API.

No GameObject, component, scene object, asset, cloud detector, weather selector, renderer path, shader, GPU buffer, readback, per-frame scan, or automatic population policy was added.

Static source checks passed for balanced delimiters, approved-file-only scope, no trailing whitespace, and absence of renderer/shader/serialized-asset changes. Unity 6000.5.0f1 compilation and Play Mode validation remain pending.

### V1.2C final status

Source implementation complete pending Unity validation. The next subsystem is the automatic cloud-opening phenomenon: batch candidate extraction, stable identity matching, population budgeting, hysteresis, and weather-driven preset/source selection. Those policies are explicitly outside V1.2C.

## WEATHER-LIGHT-RAY-V1.2C1 — Source-direction inheritance hotfix

### Objective

Correct the cloud-aware Inspector smoke test so ordinary cloud-aware rays inherit the active celestial source direction instead of forcing world-down from an unrotated authored Anchor transform. Formalize the zero-vector override contract for procedural and resolved-opening APIs.

### Approved files

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs`

No scene, prefab, material, preset asset, renderer, shader, GPU buffer, layer, tag, or project-setting change is approved.

### Reviewed evidence

- `WeatherLightRayControllerEditor.RunCloudAwareTestStep` currently derives the smoke-test direction from `-anchor.transform.up`. The accepted test Anchor has no transform rotation, so this produces a world-down explicit direction override.
- `WeatherLightRayController.UpdateRuntimeSlot` already implements the intended runtime contract: a non-zero override is normalized and used; a zero vector inherits `sourceState.RayDirectionWorld`.
- `WeatherLightRayCloudOpening.RayDirectionWorld` is copied into `WeatherLightRaySpawnRequest.RayDirectionWorld`, so a zero opening direction correctly reaches the existing source-inheritance branch.

### Contract

- `Vector3.zero` in `WeatherLightRaySpawnRequest.RayDirectionWorld`, `WeatherLightRayCloudOpening.RayDirectionWorld`, and `WeatherLightRayCloudQuery.PreferredRayDirectionWorld` means no per-instance override; inherit the active source direction.
- A finite non-zero vector is an explicit per-instance direction override.
- `MaximumVisualLeanDegrees` remains a presentation limit after base source/override direction resolution; it is not a replacement direction source.
- The cloud-aware smoke test must use zero direction for both spawn and update so it verifies the ordinary inherited-source path.

### Non-goals

- No change to authored Anchor transforms or scene data.
- No change to Sun/Moon source resolution.
- No change to ray geometry, evolution, presets, cloud provider contracts, buffers, rendering, or lifecycle.
- No automatic weather or cloud phenomenon logic.

### Acceptance criteria

- The cloud-aware smoke-test ray and the existing authored ray share the same active-source incline.
- Updating the cloud opening changes centre, diameter, and intensity only; it does not change incline or replace the handle.
- An explicit non-zero direction override remains supported.
- No scene or serialized asset is modified.
- Unity compiles without new C# errors or warnings.

### Status

Implementation in progress. Unity compilation and Play Mode validation pending.

### V1.2C1 implementation audit

Implemented:

- `RunCloudAwareTestStep` now supplies `Vector3.zero` for both the resolved opening direction and the query preference. The smoke test therefore exercises the ordinary active-source inheritance path instead of forcing `-anchor.transform.up`.
- `WeatherLightRaySpawnRequest.RayDirectionWorld`, `WeatherLightRayCloudOpening.RayDirectionWorld`, and `WeatherLightRayCloudQuery.PreferredRayDirectionWorld` now document zero as no explicit override and finite non-zero vectors as intentional overrides/preferences.
- Runtime source-direction resolution remains unchanged: `WeatherLightRayController.UpdateRuntimeSlot` inherits `sourceState.RayDirectionWorld` when the override magnitude is effectively zero.
- Report, audit, and log identifiers now identify V1.2C1.

Final scope audit:

- Modified only the four approved files.
- No scene, prefab, material, preset asset, renderer, shader, GPU buffer, layer, tag, or project-setting file changed.
- No geometry, preset, evolution, lifecycle, cloud-provider, or source-resolution algorithm changed.
- C# delimiter and trailing-whitespace static checks passed.
- Unity 6000.5.0f1 compilation and Play Mode validation remain pending.

### V1.2C1 final status

Source hotfix complete pending Unity validation. The automatic cloud-opening phenomenon remains the next subsystem after this validation passes.

---

## WEATHER-LIGHT-RAY-V1.2C2 — Multi-Spot Vegetation Accent Identity

### Objective

Replace the single published-Spot geometric vegetation-accent identity with a scalable identity carried by a registered URP Rendering Layer bit, so every enabled authored or procedural LightRay surface Spot can retain vegetation accent response simultaneously.

### Approved identity contract

- Default receiver bit: Rendering Layer bit 0 (`0x01`).
- Weather LightRay Spot identity: registered Rendering Layer bit 7 (`0x80`).
- Every runtime LightRay surface Spot uses `0x81`.
- Vegetation continues to apply ordinary URP Rendering Layer receiver filtering.
- After a light passes receiver filtering, vegetation recognizes any LightRay Spot through `(light.layerMask & 0x80u) != 0u`.
- One shared horizontal Sun/Moon accent direction remains globally published because ordinary simultaneous rays share one celestial source direction.
- The previously failed unregistered high-bit identity (`0x40000000`) remains rejected and is not restored.

### Acceptance criteria

1. Every enabled runtime LightRay Spot carries both the default receiver bit and registered identity bit 7.
2. Two or more authored/procedural rays can retain vegetation accent response simultaneously.
3. Updating one procedural/cloud-aware ray cannot steal accent identity from another ray.
4. No per-fragment loop over active ray records, GPU buffer, readback, extra render pass, or scene/project-setting change is introduced.
5. Ordinary non-LightRay additional lights retain their existing vegetation behaviour.
6. Runtime diagnostics list each Spot mask and fail when an enabled LightRay Spot lacks either required bit.

### Approved files

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl`

### Non-goals

- No scene, prefab, material, renderer asset, project setting, Rendering Layer name, Unity Layer, or tag change.
- No change to LightRay atmospheric rendering, spawning, lifecycle, evolution, presets, cloud-aware contracts, or Spot geometry.
- No support for incompatible per-ray accent directions within one active celestial-source population.

### Performance contract

The production geometric match (position subtraction, range test, normalization, and direction dot product) is replaced by one integer bit test on each additional light already evaluated by vegetation. No additional light iteration is added.

### Status

- Plan recorded before implementation: complete.
- Controller identity mask and diagnostics: pending.
- Vegetation HLSL identity match: pending.
- Cross-subsystem documentation: pending.
- Static audit: pending.
- Unity validation: pending.

### V1.2C2 implementation result

- Every runtime LightRay surface Spot now uses Rendering Layer mask `0x81`.
- Vegetation production matching now tests registered identity bit 7 after ordinary receiver-layer filtering.
- The former single-Spot geometric production match is removed from HLSL; the published Spot position remains diagnostic reference data only.
- Runtime diagnostics enumerate each Spot mask and validate both required bits.
- No scene, project setting, renderer, material, preset, spawning, lifecycle, evolution, or GPU buffer contract changed.
- Static source audit passed; Unity compilation and simultaneous multi-Spot visual validation remain pending.

## WEATHER-LIGHT-RAY-V1.2C3 — Indexed Vegetation Accent Data

**Status:** Approved implementation plan; implementation follows this section.

### Objective

Replace the failed Rendering Layer identity path with one direct vegetation-accent sidecar record per URP additional-light index. The vegetation shader must perform an O(1) indexed read inside the additional-light loop and must not search a separate LightRay list or geometrically identify Weather lights.

### Accepted contract

- URP remains authoritative for light direction, colour, attenuation, shadows, visibility, and per-object light selection.
- The sidecar supplies only project-specific vegetation-accent controls unavailable in URP's `Light` data: strength, coverage, softness, and explicit-override weight.
- Ordinary lights receive the established generic vegetation response.
- Runtime Weather LightRay Spot proxies receive the active preset's accent controls.
- Forward uses `GetPerObjectLightIndex(loopLightIndex)` to resolve the global additional-light record index.
- Forward+ uses the cluster light index directly; URP's punctual loop has already added the additional-directional offset.
- CPU publication follows `RenderingData.lightData.visibleLights` order while skipping `mainLightIndex`, matching URP's additional-light data order.
- No shader-side LightRay search, position comparison, Rendering Layer identity bit, GPU readback, or additional light-evaluation loop is permitted.

### Data layout

One 16-byte `float4` / `Vector4` record per additional light:

- `x`: resolved accent-strength scale;
- `y`: accent participation coverage;
- `z`: accent softness;
- `w`: explicit Weather override weight (`0` generic, `1` Weather LightRay).

### Approved files

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayPreset.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Rendering/Weather/WeatherLightRayRendererFeature.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl`
- existing LightRay preset assets under `Assets/Game/Demo/Profiles/Weather/LightRays/`

### Invariants

- No scene, prefab, material, renderer asset, layer, tag, or project-setting modification.
- No new render target, draw call, shader light loop, or per-frame managed allocation.
- Runtime buffers and CPU arrays grow geometrically and are reused.
- Existing atmospheric rendering, LightRay spawning, cloud-aware APIs, surface Spot lighting, and ordinary local-light accents remain unchanged.
- The V1.2C2 bit-7 identity contract is superseded and must be removed from production and documentation.

### Validation gates

1. Ordinary local test light retains its vegetation accent.
2. Authored and procedural LightRay Spots produce accents simultaneously.
3. Updating the procedural ray does not remove the authored ray's accent.
4. Active preset strength, coverage, and softness affect every LightRay Spot consistently.
5. Runtime diagnostics report sidecar visible-light count, Weather override count, capacity, and no index overflow.
6. Unity Profiler shows no recurring managed allocation from sidecar publication after warmup.

### V1.2C3 implementation result and final source audit

**Status:** Implemented; Unity compilation and runtime validation pending.

Implemented:

- renderer-owned persistent `GraphicsBuffer` with 16-byte records;
- geometrically growing reusable CPU record array and GPU capacity;
- publication in URP additional-light order, skipping `mainLightIndex`;
- Controller registration from each enabled runtime LightRay Spot `Light` entity ID to the active preset's resolved strength, coverage, and softness;
- Forward index resolution through `GetPerObjectLightIndex` and Forward+ direct cluster index resolution;
- ordinary lights retain zero override weight and the established generic local-light accent response;
- LightRay Spot Rendering Layer mask returned to the normal receiver mask;
- failed V1.2C2 registered-bit identity and single-Spot production matching removed;
- diagnostics record additional-light count, Weather override count, buffer capacity, and index mismatch state.

Final static audit:

- C# and HLSL delimiter counts balance in every modified source file;
- no remaining production reference to the bit-7 identity constant or geometric LightRay search;
- no shader-side loop over LightRay records;
- one direct sidecar read is issued only inside the existing URP additional-light evaluation;
- no new render target, draw call, GPU readback, scene edit, prefab edit, material edit, layer/tag change, or project-setting change;
- Unity 6000.5.0f1 compilation, Forward/Forward+ runtime index agreement, visual response, and Profiler allocation checks remain project-side validation gates.

## WEATHER-LIGHT-RAY-V1.2C3B — EntityId Construction and Guaranteed Pre-Opaque Sidecar Binding

### Objective

Correct the two project-reported V1.2C3A blockers without changing the indexed accent architecture:

1. construct the `Dictionary<EntityId, Vector4>` with the same `EntityId` key type;
2. guarantee `_VegetationAdditionalLightAccentData` is command-buffer bound before vegetation draws for every relevant Base Game camera, including zero-record and unavailable-controller cases.

### Approved files

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Rendering/Weather/WeatherLightRayRendererFeature.cs`

### Reviewed evidence

- Project compile error: `WeatherLightRayController.cs(232,13)` assigns `new Dictionary<int, Vector4>()` to `Dictionary<EntityId, Vector4>`.
- Project D3D12 validation error: the vegetation benchmark shader reaches a draw without an SRV bound for `_VegetationAdditionalLightAccentData`.
- V1.2C3A currently calls `Shader.SetGlobalBuffer` from feature setup/publication code. That does not prove a RenderGraph command establishes the binding before opaque vegetation rendering.
- The existing LightRay atmospheric pass uses RenderGraph command-buffer global binding successfully.

### Invariants

- Preserve direct indexed sidecar lookup; do not add a LightRay search loop.
- Preserve one `float4` record per URP additional-light index.
- Preserve zero record count when no sidecar entries exist while still binding a valid one-element fallback SRV.
- Do not modify shaders, scenes, materials, presets, renderer assets, layers, tags, or project settings.
- Add no per-frame managed allocation.

### Implementation sequence

1. Correct the dictionary constructor key type.
2. Add a dedicated pre-opaque RenderGraph binding pass owned by `WeatherLightRayRendererFeature`.
3. Build/upload the sidecar in `AddRenderPasses`, then enqueue the binding pass for every Base Game camera before opaque rendering.
4. Bind the one-record fallback buffer and count zero for invalid-controller, non-target-camera, and zero-additional-light cases.
5. Keep the atmospheric LightRay pass scheduling unchanged.
6. Audit every return path for a guaranteed binding-pass enqueue before vegetation.

### Non-goals

- No change to accent appearance, accumulation, preset controls, LightRay registration, cloud-aware spawning, or renderer/shader data layouts.

### Validation

- Unity compiles without the `Dictionary<int, Vector4>` assignment error.
- D3D12 reports no missing `_VegetationAdditionalLightAccentData` SRV before or after spawning rays.
- Vegetation renders with zero active LightRays and with authored/procedural LightRays.
- Existing V1.2C3 indexed accent behaviour remains unchanged.

### Status

- Review: complete.
- Plan: approved by the user request to correct the reported blockers.
- Implementation: in progress.
- Unity validation: pending project-side execution.

### V1.2C3B implementation result and source audit

Implemented:

- corrected `vegetationAccentOverridesByLight` construction to `new Dictionary<EntityId, Vector4>()`;
- added `VegetationAccentBindingPass` at `BeforeRenderingOpaques`;
- the sidecar buffer is prepared before pass enqueue and command-buffer bound inside RenderGraph;
- every Base Game camera receives either the populated sidecar or a valid one-record fallback with published count zero;
- atmospheric LightRay rendering remains restricted to the resolved LightRay camera and remains scheduled after transparents.

Final source audit:

- no `Dictionary<int, Vector4>` constructor remains in the vegetation accent registry;
- no V1.2C3 sidecar path relies on `Shader.SetGlobalBuffer` for pre-opaque vegetation correctness;
- the binding pass is enqueued before every early return that can occur after Base Game camera acceptance;
- fallback data uses persistent geometrically grown storage and adds no per-frame managed allocation;
- renderer, shader, preset, scene, material, layer, tag, and project-setting contracts are unchanged.

Unity compilation and D3D12 runtime validation remain pending in Unity 6000.5.0f1.

## WEATHER-LIGHT-RAY-V1.2C3C — Per-Camera Guaranteed Vegetation Accent Binding

### Objective

Eliminate the remaining D3D12 missing-SRV warning without changing the accepted indexed vegetation-accent architecture. Every camera executed by the installed `WeatherLightRayRendererFeature` must receive a valid `_VegetationAdditionalLightAccentData` buffer binding before opaque vegetation draws. Only the resolved Base Game LightRay camera may publish populated additional-light records; all other cameras bind a dedicated one-record zero fallback with record count zero.

### Approved files

Modify:

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Game/Rendering/Weather/WeatherLightRayRendererFeature.cs`

No shader, Controller, scene, prefab, material, preset, renderer asset, layer, tag, or project-setting modification is approved.

### Reviewed evidence

- Project runtime warning remains: `Fragment Shader "PS3D/Vegetation/Stylized Vegetation Benchmark" requires a buffer (SRV) "_VegetationAdditionalLightAccentData" at index 2, but none provided.`
- `WeatherLightRayRendererFeature.AddRenderPasses` returns for every non-Base Game camera before enqueuing `VegetationAccentBindingPass`.
- `VegetationLighting.hlsl` declares `_VegetationAdditionalLightAccentData` for the benchmark shader's `UniversalForward` pass; a zero count prevents indexed reads but does not remove the D3D12 resource-binding requirement.
- V1.2C3B owns only one `vegetationAccentBuffer`. `PrepareEmptyVegetationAccentSidecar` writes the fallback zero record into that populated buffer rather than using a distinct fallback resource.
- `PC_Renderer.asset` contains the active LightRay Renderer Feature. The atmospheric pass must remain restricted to the resolved Base Game LightRay camera, but sidecar resource safety must not share that restriction.

### Acceptance criteria

1. Every camera processed by the installed Renderer Feature enqueues the pre-opaque sidecar binding pass.
2. The resolved Base Game LightRay camera binds the populated sidecar and actual additional-light record count.
3. Every other camera binds a dedicated valid one-record zero fallback and count zero.
4. Non-target cameras never clear, overwrite, resize, or otherwise mutate the populated sidecar buffer.
5. D3D12 emits no missing `_VegetationAdditionalLightAccentData` SRV warning before spawning rays, while authored and procedural rays coexist, after procedural updates/releases, or on Play Mode exit.
6. Existing direct indexed lookup, ordinary local-light response, simultaneous authored/procedural LightRay accents, atmospheric rendering, diagnostics, and camera selection remain unchanged.

### Invariants and non-goals

- Preserve one `float4` per URP additional-light index and the existing Forward/Forward+ index contract.
- Preserve the Controller's `EntityId`-keyed override registry.
- Do not add a shader-side LightRay search, another light loop, GPU readback, render target, draw call, managed per-frame allocation, or serialized asset change.
- Do not broaden atmospheric LightRay rendering to Scene, Preview, Reflection, overlay, or unrelated cameras.
- Do not modify the shared vegetation shader/include unless implementation evidence invalidates this plan; such evidence would require a plan update and renewed approval.
- Mobile renderer support remains outside this patch because `Mobile_Renderer.asset` does not contain this Renderer Feature and serialized renderer modification is not approved.

### Implementation sequence

1. Record this plan before code changes. **Complete.**
2. Add a persistent dedicated one-record fallback `GraphicsBuffer`, initialized once with `Vector4.zero` and independent of the populated sidecar. **Complete.**
3. Reorder `AddRenderPasses` so binding setup/enqueue occurs before the camera-specific atmospheric early return. **Complete.**
4. Publish populated records only for the resolved Base Game LightRay camera; bind fallback/count zero for every other camera. **Complete.**
5. Dispose and recreate both buffers safely while preserving geometric growth and reusable CPU storage for populated records. **Complete.**
6. Re-read and audit the complete final review surface, compare the final diff with the approved two-file scope, and run available static checks. **Complete.**
7. Validate Unity compilation and the complete D3D12 lifecycle in Unity 6000.5.0f1. **Pending — project-side.**

### Performance model

- Resolved Base Game player-camera work remains one pre-opaque global buffer bind and one global integer bind plus the existing `O(V)` sidecar publication for `V` visible additional lights.
- Each additional Editor or secondary camera using this Renderer Feature receives one constant-cost `O(1)` binding pass with no draw, render target, shader instruction, or sidecar upload.
- Persistent memory increases by one structured buffer containing one 16-byte record plus graphics-API allocation overhead.
- No performance exception is required.

### Risks

- A camera rendered through a different renderer that does not install this Renderer Feature can still lack a sidecar binding. That renderer-wide support problem is outside the approved source-only C3C scope and must not be hidden by raw serialized-asset edits.
- Unity RenderGraph compilation/runtime behavior must confirm that the uncullable global-state pass executes before the benchmark vegetation draw for Scene, Preview, overlay, and Base Game cameras.

### Validation

1. Enter Play Mode with Game and Scene views active and verify the Console contains no missing-SRV warning before procedural spawning.
2. Spawn the existing cloud-aware procedural test ray, update it repeatedly, and verify authored plus procedural vegetation accents remain visible.
3. Release the procedural ray and exit Play Mode; verify the warning never appears at any lifecycle stage.
4. If the warning remains, submit the complete Console entry plus whether Game, Scene, Inspector Preview, or another camera was rendering at the first occurrence.

### Status

- Review: complete.
- Plan: approved by the user's explicit instruction to proceed with the agreed warning correction.
- Implementation: complete.
- Static audit: complete.
- Unity compilation and D3D12 runtime validation: pending project-side execution.


### V1.2C3C implementation result and source audit

Implemented:

- added `vegetationAccentFallbackBuffer`, a dedicated persistent one-record structured buffer initialized once with `Vector4.zero`;
- retained `vegetationAccentBuffer` exclusively for populated resolved-camera records and geometric capacity growth;
- moved sidecar binding setup and enqueue ahead of all camera-specific atmospheric early returns;
- every camera processed by this Renderer Feature now binds either the populated buffer with its record count or the fallback buffer with count zero;
- zero-additional-light publication records diagnostics without allocating or mutating the populated buffer;
- both buffers are independently disposed and recreated by the Renderer Feature lifecycle;
- atmospheric rendering remains restricted to the resolved Base Game LightRay camera and remains scheduled after transparents.

Final source audit:

- exactly the two approved files differ from the supplied Archive 43 baseline;
- the binding pass enqueue occurs before every `AddRenderPasses` return path;
- no production reference to `PrepareEmptyVegetationAccentSidecar` remains;
- non-target cameras do not upload, clear, resize, or select the populated sidecar;
- C# lexical delimiter counts balance and both changed files remain UTF-8 with LF endings;
- no shader, Controller, scene, prefab, material, preset, renderer asset, layer, tag, project-setting, render-target, draw-call, GPU-readback, or per-frame managed-allocation change was introduced;
- Git `HEAD` comparison and Unity compilation are unavailable because the supplied archive contains no `.git` metadata or complete Unity project root. Runtime validation remains required in Unity 6000.5.0f1.


## WEATHER-LIGHT-RAY-V1.2D — Deterministic Automatic Sun Population

### Status

- Review: complete against user-supplied `Assets-Code-Archive(43).zip` plus the accepted V1.2C3C changed files.
- Plan: approved by the user's explicit instruction to proceed on 2026-07-29.
- Implementation: source complete in the approved eight-file scope.
- Static audit: complete and passed; evidence recorded below.
- Unity compilation, runtime, visual, allocation, CPU, and GPU validation: pending project-side execution and required before runtime acceptance.

### Objective

Automatically maintain a bounded, stable population of cloud-respecting Sun LightRays around the resolved gameplay camera or an explicit population focus. Candidate placement is deterministic in world-space cells. Candidates acquire real ground, validate the complete preset-sized footprint against the exact CPU-readable Weather cloud cookie at the present and bounded future times, qualify through hysteresis, and enter the existing GameObject-free procedural handle path. Individual V1.2D rays remain static; cloud motion causes graceful retirement and replacement rather than ground-space chasing.

### Approved files

Modify:

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Docs/Weather_System_Architecture_Provisional.md`
- `Assets/Docs/Weather_Cloud_Shadow_Handoff.md`
- `Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs`

Create:

- `Assets/Game/Procedural/Weather/WeatherLightRayPopulationRuntime.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayPopulationRuntime.cs.meta`

No scene, prefab, material, preset asset, renderer asset, shader, layer, tag, project-setting, cloud-cookie-generator, `WeatherLightRayTypes.cs`, `WeatherLightRayPreset.cs`, `WeatherLightRaySourceProfile.cs`, or `WeatherLightRayAnchor.cs` change is approved.

### Reviewed evidence

- `WeatherLightRayController` already owns stable generation-checked authored and procedural slots, the active preset, source states, lifecycle smoothing, surface Spot proxies, cloud-aware spawn/update APIs, and the comprehensive report.
- `TrySpawnOrUpdateResolvedCloudOpening` preserves a valid procedural handle and converts a resolved opening into the accepted runtime request path.
- `WeatherCloudShadowController.TrySampleCloudTransmission` samples the exact readable generated cookie and publishes seed-evolution stability; `TryProjectCloudCookieUv` proves the authoritative world/source projection.
- `WeatherCloudShadowController` already owns current phase, resolved wind direction, movement speed, current cookie offset, current seed, and evolution progress. A future-time query can reuse this data without GPU readback, texture regeneration, or cloud-generator modification.
- `WeatherLightRayPreset.DefaultAreaDiameterMetres` supplies the authoritative automatic footprint size.
- Canonical sections J–L require deterministic world cells, staged ground validation, footprint-wide cloud eligibility, bounded ordinary-motion forecasting, and suspension during seed evolution.
- The V1.2C1 cloud-aware smoke test passed project-side spawn, stable-handle update, and release. V1.2C3C passed project-side without the reported missing-SRV warning recurring during that lifecycle.

### Accepted serialized controls and initial defaults

All controls remain on the existing `WeatherLightRayController`; no new component is added.

```text
Automatic Population Enabled       false
Population Seed                    7331
Population Focus Override          None
Ground Mask                        Nothing (must be assigned deliberately)
Desired Automatic Rays             3
Maximum Automatic Rays             6
Minimum Spacing                    12 m
Offscreen Margin                   10 m
Fallback Active Radius             40 m
Evaluation Rate                    4 Hz
Candidate Checks Per Tick          8
Minimum Clearance Strength         0.75
Qualification Duration             0.50 s
Invalid Grace Duration             0.75 s
Minimum Viable Opening Duration    4.0 s
Maximum Ground Slope               50 degrees
Ground Search Distance             500 m
Show Population Candidates         false
```

The desired count is a target, not a guarantee. The maximum is a configurable rendering budget, not an architectural maximum. Automatic population is disabled by default and the Ground Mask defaults to Nothing so importing source cannot silently add runtime rays or guess project physics ownership.

### Runtime ownership and data flow

`WeatherLightRayController` owns serialized configuration, authoritative source/preset/controller references, the scheduler tick, automatic-ray slot protection, report publication, and editor-facing state.

`WeatherLightRayPopulationRuntime` is a non-component internal scheduler that owns preallocated candidate state, deterministic cell enumeration, stable identity-to-handle association, qualification, invalid grace, retirement, cooldown, bounded operation counters, and debug records.

`WeatherCloudShadowController` remains the frozen cloud producer and gains only a query-only future-time transmission overload. It does not own candidates, handles, budgets, or LightRay lifecycle.

```text
resolved focus and active radius
    -> deterministic world cells
    -> cheap region / count / spacing rejection
    -> one ground raycast
    -> 13-point preset footprint
    -> present + bounded future cloud samples
    -> qualification hysteresis
    -> TrySpawnOrUpdateResolvedCloudOpening
    -> static active ray
    -> invalid grace
    -> TrySetProceduralRayVisible(false)
    -> fade to zero
    -> TryReleaseProceduralRay
```

### Candidate and identity contract

- V1.2D produces one candidate per deterministic world cell.
- Cell size is derived from the configured minimum spacing.
- Stable identity is a non-zero 64-bit hash of population seed, Sun source kind, integer cell X/Z, and candidate index.
- Candidate X/Z offsets are deterministic and do not use `UnityEngine.Random`.
- Camera motion changes only the evaluated cell set; it does not reshuffle positions inside retained world cells.
- Returning to a region restores the same candidate identities.
- Candidate state is bounded and preallocated. The scheduler performs no full-world or full-cookie scan.

### Focus and active-region contract

- Explicit Population Focus Override has priority.
- Without an override, the resolved Base Game camera's centre viewport ground hit is the focus.
- Camera corner ground hits derive a bounded visible radius plus Offscreen Margin.
- If corner projection is incomplete, Fallback Active Radius is used around the valid centre focus.
- Missing camera, missing Ground Mask, or missing centre ground hit suspends automatic spawning and gracefully retires automatic rays.
- The Controller transform is not an automatic fallback population location.

### Ground contract

- New candidates perform one downward `Physics.Raycast` using the explicit Ground Mask and `QueryTriggerInteraction.Ignore`.
- Ground search is bounded by Ground Search Distance.
- Candidates exceeding Maximum Ground Slope are rejected.
- Static active rays retain their validated ground point and do not repeat ground raycasts every frame.
- No layer or tag is added or inferred.

### Cloud-footprint and forecast contract

- Footprint radius is half `ActivePreset.DefaultAreaDiameterMetres`.
- Each complete evaluation uses 13 points: centre, four half-radius cardinal points, and eight perimeter points.
- Forecast times are: now, end of the 0.75-second automatic fade-in, midpoint between fade-in and the configured minimum viable duration, and the minimum viable duration.
- The candidate clearance is the minimum normalized opening weight over all footprint positions and forecast times:

```text
normalizedOpen = saturate((transmission - shadedTransmission) /
                          max(0.0001, 1 - shadedTransmission))
```

- The candidate passes only when the minimum normalized opening weight meets Minimum Clearance Strength.
- Future sampling advances the existing cookie world phase analytically by resolved wind direction multiplied by movement speed and future seconds. It does not regenerate pixels, read back the GPU, or evaluate a second noise field.
- Clear-sky status remains transmission 1. A published enabled cloud controller with missing cookie data is an error/suspension, not silent clear sky.

### Population and lifecycle contract

- V1.2D supports automatic Sun population only. Moon population remains blocked by the absence of an authoritative Moon source.
- Automatic rays use `OriginKind.Procedural`, `LifetimePolicy.ExternallyControlled`, `MovementPolicy.Static`, `SourceGatePolicy.RequireActiveSource`, and runtime `CloudPolicy.IgnoreClouds` because the scheduler already owns complete footprint and forecast validation.
- Authored and caller-created procedural rays are never evicted. Automatic spawning uses only free central slots.
- Desired count is enforced as a target. If desired or maximum count decreases, excess automatic rays farthest from the current focus retire first.
- Pending candidates require the configured qualification duration and at least two valid evaluations.
- Active candidates tolerate invalid results for Invalid Grace Duration before retirement.
- Retirement uses `TrySetProceduralRayVisible(false)`, waits for the existing fade to reach zero, then releases the slot and enters bounded cooldown.
- Disabling automatic population, globally disabling Weather LightRays, or losing required focus/source/cloud state retires automatic rays only.

### Seed-evolution contract

- While cloud evolution is active below `CloudEvolutionResumeThreshold`, automatic spawning stops and all active automatic rays retire.
- Qualification resumes only at or above the configured threshold against the mostly established blended cookie.
- No dual current/next LightRay population is maintained in V1.2D.
- Existing authored and caller-created `RespectClouds` rays treat `EvolutionUnstable` below the resume threshold as closed and fade through the existing gate. `IgnoreClouds` rays remain unaffected.

### Inspector and diagnostics contract

- Add one collapsed `Automatic Population` section to the existing custom Inspector.
- Expose only the accepted serialized controls and concise derived state.
- Extend the existing comprehensive report with enabled/suspension state, focus, active radius, seed, desired/maximum/active/pending/retiring/cooldown counts, free slots, cloud seed/evolution, cadence/budget, per-tick candidate checks/raycasts/cloud samples, stable identities/handles, and rejection totals.
- Add one optional Scene-view candidate display. Do not add separate test buttons or reports per rejection family.
- Retain the existing V1.2C1 cloud-aware smoke-test actions until V1.2D passes project-side validation; remove obsolete test UI only in a later approved cleanup.

### Performance contract

- Scheduler cadence is bounded and independent of frame rate.
- New-candidate work is capped by Candidate Checks Per Tick.
- Active automatic rays are revalidated at the same bounded cadence.
- No full cookie scan, GPU readback, cloud regeneration, per-frame physics query per ray, LINQ, iterator allocation, or recurring managed allocation is permitted after scheduler storage initialization.
- Each complete candidate or active-ray evaluation performs at most 52 CPU cookie samples: 13 footprint points times four forecast times.
- With eight new checks, six active rays, and 4 Hz, the analytical upper example is `(8 + 6) * 52 * 4 = 2912` CPU bilinear cookie samples per second plus at most `8 * 4 = 32` new-candidate ground raycasts per second.
- Automatic count remains a GPU/rendering budget because every visible ray adds one zone record/draw contribution and one real Spot proxy. Profile 0, 1, 3, and 6 automatic rays before production density is accepted.
- Target validation gates remain median incremental GPU <= 1.0 ms, high-percentile incremental GPU <= 1.5 ms, median CPU increment <= 0.20 ms, and 0 B/frame recurring managed allocation under the canonical 1440p comparison.

### File-by-file implementation sequence

1. Record this canonical plan before code. **Complete.**
2. Add future-time cloud transmission sampling by refactoring projection to accept an analytically advanced world phase. **Source complete; static audit passed; Unity validation pending.**
3. Create the bounded population runtime and metadata. **Source complete; deterministic/hash/storage/static checks passed; Unity validation pending.**
4. Add Controller configuration, scheduler integration, slot/spacing helpers, seed-evolution gating correction, report state, and cleanup. **Source complete; static audit passed; Unity validation pending.**
5. Add the collapsed Inspector section, live state, report version update, and optional Scene-view markers. **Source complete; serialized-property and API-reference checks passed; Unity validation pending.**
6. Update parent Weather and frozen cloud handoff documents with the query-only boundary and LightRay ownership. **Complete.**
7. Re-read the final review surface, compare exact scope, run static checks, package changed files only, and record results here. **Complete for source/static/package preparation; package hash recorded in delivery.**
8. Run Unity 6000.5.0f1 compile/runtime/visual/performance validation. **Pending — project-side blocker before runtime acceptance.**

### Acceptance criteria

1. Enabling automatic population with a valid Ground Mask produces zero to Desired Count stable Sun rays only at qualifying cloud-clear footprints.
2. Invalid cloud, terrain, spacing, source, capacity, or evolution conditions reduce count rather than forcing placement.
3. Authored and non-automatic procedural rays remain registered and are never evicted.
4. Stable world cells preserve candidate identities under ordinary camera motion and after revisiting a region.
5. Automatic rays remain static; invalid openings fade and release rather than sliding.
6. Entire footprints and future residence windows pass cloud clearance before spawn.
7. Cloud seed evolution suspends spawning and retires automatic rays below the resume threshold.
8. Disabling automatic population retires automatic rays only.
9. Work remains bounded by the configured cadence and candidate budget with no full-cookie scan or GPU readback.
10. Comprehensive diagnostics expose operation counts, reasons, identities, handles, and state without new diagnostic-button clutter.
11. Existing V1.2C3C vegetation accent binding remains warning-free and simultaneous authored/procedural accent response remains intact.


### V1.2D source implementation and compliance audit — 2026-07-29

#### Actual affected files

Modified:

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Docs/Weather_System_Architecture_Provisional.md`
- `Assets/Docs/Weather_Cloud_Shadow_Handoff.md`
- `Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs`

Created:

- `Assets/Game/Procedural/Weather/WeatherLightRayPopulationRuntime.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayPopulationRuntime.cs.meta`

The actual eight-file source/document scope exactly matches the approved V1.2D plan. No scene, prefab, material, preset asset, renderer asset, shader, include, cloud generator, layer, tag, or project-setting file changed.

#### Material implementation evidence

- `WeatherCloudShadowController.TrySampleCloudTransmissionAtTimeOffset` reuses the existing readable cookie and source-local projection while advancing only the controller-owned world phase analytically. Present-time sampling delegates to the same path with a zero-second offset. Invalid/non-finite forecast times, distances, and phases fail explicitly.
- `WeatherLightRayPopulationRuntime` owns one deterministic candidate per hashed world cell, grow-only preallocated candidate storage, stable identity-to-handle state, bounded pending/new evaluation, full-footprint forecast sampling, qualification, invalid grace, retirement, cooldown, operation counters, cumulative rejection counts, and Scene-view debug records. Candidate storage never shrinks while handles may still be tracked; reducing the configured budget retires excess rays through the normal lifecycle.
- `WeatherLightRayController` owns disabled-by-default serialized settings, explicit Ground Mask, source/camera/cloud resolution, free-slot and cross-origin spacing checks, bounded scheduler invocation, global-LightRay suspension, seed-evolution gating for automatic and existing `RespectClouds` rays, comprehensive reporting, and automatic-ray shutdown.
- `WeatherLightRayControllerEditor` exposes one collapsed Automatic Population section, concise live state, one optional candidate overlay, and the existing comprehensive report. The established V1.2C1 smoke-test actions remain temporarily available until V1.2D project-side validation succeeds.

#### Performance and lifecycle audit

- Scheduler candidate-array scans and cloud/physics work occur only at `Evaluation Rate`; the per-frame Controller call performs only settings assembly, storage-presence check, and cadence comparison.
- New-candidate work remains capped by `Candidate Checks Per Tick`; active-ray revalidation is bounded by the configured automatic maximum and central storage capacity.
- Complete cloud evaluation remains at most 13 footprint positions times four forecast times per evaluated candidate or active ray. Early threshold failure reduces actual samples.
- No cookie scan, GPU readback, cloud regeneration, shader work, render target, LINQ, iterator pipeline, thread, task, blocking wait, or per-tick collection allocation was introduced.
- Candidate storage is grow-only after initialization to prevent tracked-handle loss when the automatic maximum is reduced. Excess active rays retire farthest-first; authored and caller-created procedural slots are not evicted.
- Automatic population is Play-Mode-only, disabled by default, and requires an explicit non-empty Ground Mask.

#### Static verification

Passed in the supplied Archive 43 plus accepted V1.2C3C source reconstruction:

- exact eight-file scope comparison against the reconstructed pre-V1.2D baseline;
- complete C# delimiter/string/comment lexical scan for all four changed C# files;
- UTF-8 decoding and NUL checks for all eight changed files;
- Editor serialized-property name to Controller field audit;
- Editor/runtime public and internal API reference audit;
- unique new `.meta` GUID search;
- prohibited population-path scan for LINQ, GPU readback, blocking waits, and cloud-generator edits;
- deterministic stable-identity sample check over 40,401 cells with zero duplicates and zero invalid identities;
- final source re-read and scope reconciliation.

Unity 6000.5.0f1 compilation, Play Mode cloud/ground behavior, D3D12 warning regression, allocation profiling, and 0/1/3/6-ray CPU/GPU measurements remain pending because Unity is unavailable in the patch workspace. Source implementation must not be described as runtime-accepted until those checks pass.

#### Project-side validation contract

1. In `Weather LightRay Controller Inspector -> Automatic Population`, assign the real Ground Mask, enable `Enabled`, set `Desired Ray Count = 3`, `Maximum Automatic Ray Count = 3`, and enable `Show Population Candidates`.
2. Enter Play Mode and confirm zero to three static Sun rays appear only at clear candidate footprints, authored/gameplay rays remain present, candidate markers remain stable while moving the camera, and no `_VegetationAdditionalLightAccentData` D3D12 warning appears.
3. In `Weather Cloud Shadow Controller Inspector -> Actions & Reports`, click `Start Pattern Evolution Now`; confirm automatic rays fade/release below the 80% resume threshold and qualification resumes only after the threshold.
4. Return the camera to its original region, click `Weather LightRay Controller Inspector -> Actions & Reports -> Copy LightRay V1.2D Report`, and provide the complete report plus any Console error. Performance acceptance additionally requires comparable 0/1/3/6-ray CPU, GPU, GC, draw-count, and visible-light captures.

## WEATHER-LIGHT-RAY-V1.2D1 — Definite-Assignment Compile Hotfix

**Date:** 2026-07-29  
**Status:** Source hotfix complete; Unity compilation pending.

### Failure evidence

Unity reported `CS0165` for the `clearance` and `dataVersion` locals in `WeatherLightRayPopulationRuntime.RevalidateActiveCandidates`. The locals were declared as `out` variables inside the right-hand operand of a short-circuit `insideRegion && TryEvaluateCloudFootprint(...)` expression. When `insideRegion` was false, the method call did not execute, so C# definite-assignment analysis correctly treated both locals as unassigned even though their reads were guarded by `if (valid)`.

### Approved scope

Modified:

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayPopulationRuntime.cs`

No runtime contract, serialized field, cloud query, shader, renderer, scene, prefab, material, layer, tag, or project-setting change is authorized.

### Correction

Initialize `clearance` and `dataVersion` before the short-circuit expression, then pass the existing locals to `TryEvaluateCloudFootprint`. This preserves the exact evaluation and lifecycle behavior while satisfying C# definite-assignment rules for the outside-region path.

### Audit

The remaining `TryEvaluateCloudFootprint` call sites were inspected. Their `out` values are scoped to method calls that execute unconditionally within their containing condition and are not read on a short-circuited path. No equivalent compile defect was found.

### Validation

1. Allow Unity to recompile and confirm both `CS0165` errors are absent.
2. Run the existing V1.2D automatic-population validation unchanged.

## WEATHER-LIGHT-RAY-V1.2D2 — Vegetation Accent Control Contract Closure

### Status

- Review: complete against the V1.2D1 merged source.
- Plan: approved by the user, with runtime diagnostic expansion explicitly removed from scope.
- Implementation: source-complete.
- Unity compilation and visual validation: pending project-side execution.

### Objective

Restore the intended independent contracts of the Weather LightRay vegetation accent controls and make those contracts explicit at the production code boundaries so later changes cannot plausibly reinterpret them:

1. `Accent Line Intensity` is authored by the active `WeatherLightRayPreset` and scales only Weather-specific vegetation edge radiance.
2. `Vegetation Accent Coverage` selects a stable whole-card subset across the complete illuminated footprint with no centre/rim bias.
3. `Vegetation Accent Softness` shapes only the selected blade-edge profile; it must not change card participation, LightRay footprint eligibility, source direction, or body illumination.
4. Weather Spot body lighting retains the real URP punctual-light direction, attenuation, colour, and cone.
5. Weather blade-edge selection uses the LightRay/celestial source's normalized horizontal direction, never the radial fragment-to-Spot direction.
6. Every eligible camera publishes a sidecar aligned with that camera's own URP additional-light ordering. Atmospheric LightRay compositing remains restricted to the resolved Base Game camera.

### Approved files

Modify only:

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Rendering/Weather/WeatherLightRayRendererFeature.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl`

No Editor diagnostics, scene, prefab, material, preset asset, renderer asset, layer, tag, project-setting, cloud-population, cloud-generator, Light component, draw-call, render-target, or GPU-readback change is approved.

### Reviewed evidence

- `WeatherLightRayController.RefreshSharedAccentLineCacheIfRequired` reads the serialized fallback field `accentLineIntensity` instead of the preset-resolved `AccentLineIntensity` property, so active-preset intensity edits do not control the published indexed sidecar scale.
- `WeatherLightRayController.UpdateSurfaceSpotLights` publishes only one `float4` per Light, containing strength, coverage, softness, and override weight. It omits the already available horizontal source direction returned by `TryResolveVegetationAccentDirection`.
- `VegetationLighting.hlsl` therefore uses `light.direction` for both body lighting and Weather blade-edge selection. Because the Spot is directly above the footprint centre, its horizontal direction component is zero at the centre and strongest near the rim, producing the observed radial eligibility ring.
- Coverage thresholding itself is stable whole-card hashing, but it occurs after the radial direction gate. It can only choose a subset of the rim that survived the wrong direction contract.
- Softness currently exponentiates `facingEdge`, changing directional/card eligibility rather than the selected edge-line profile.
- `WeatherLightRayRendererFeature.AddRenderPasses` publishes populated records only for the resolved Base Game camera and binds an empty fallback for Scene View. This removed unsafe cross-camera data leakage but also removed Weather-specific Scene View accent parity.
- The historical AF5B contract already requires real Spot direction for body lighting and horizontal LightRay source direction only for the stylized blade-edge selector.

### Protected production-code contracts

The implementation must add prominent module/function comments at every ownership boundary. Those comments are part of the accepted architecture and must state the following prohibitions explicitly:

- Do not source preset-owned accent values from hidden Controller fallback fields while an active preset exists.
- Do not use the punctual Spot's radial `Light.direction` for Weather blade-edge selection.
- Do not use Coverage to scale radiance or to apply a radial/spatial mask.
- Do not use Softness to change the participating-card set, direction gate, attenuation, or LightRay footprint.
- Do not change the GPU sidecar layout in C# without the matching HLSL layout change, and vice versa.
- Do not reuse one camera's additional-light ordering for another camera.
- Do not broaden atmospheric LightRay rendering while adding Scene View sidecar parity.
- Do not add a shader-side LightRay search, another light loop, or geometric Spot matching.

### GPU record contract

One structured record remains aligned one-to-one with a URP additional-light index:

```text
parameters.x = resolved Weather accent radiance scale
parameters.y = stable whole-card participation coverage [0, 1]
parameters.z = selected blade-edge profile softness [0, 1]
parameters.w = explicit Weather override active
sourceDirectionWS.xyz = normalized horizontal direction from vegetation toward the LightRay source
sourceDirectionWS.w = valid direction flag
```

Ordinary lights receive a zero record and retain the established generic punctual-light path.

### Implementation sequence

1. Record this canonical plan before code changes. **Complete.**
2. Repair preset authority in the Controller cache and legacy global publication. **Complete.**
3. Replace the Controller's one-`Vector4` override registry value with parameters plus per-LightRay source direction, with protected function/module comments. **Complete.**
4. Expand the renderer/HLSL structured sidecar record to the mirrored two-`float4` contract. **Complete.**
5. Publish camera-correct sidecars for Base Game and Scene View cameras while retaining safe fallback binding for unrelated cameras. **Complete.**
6. Use source direction only for Weather edge selection, keep real Spot direction for body lighting, preserve ordinary-light behaviour, and move softness to the final blade-edge profile. **Complete.**
7. Update the two vegetation architecture documents and record the final six-file source/compliance audit. **Complete.**
8. Validate Unity compilation and the complete visual control matrix. **Pending — project-side.**

### Acceptance criteria

1. Intensity `0` removes Weather-specific vegetation edge radiance while the Spot's body illumination remains.
2. Increasing Intensity produces a clearly monotonic Weather edge-radiance increase from the active preset.
3. Coverage `0` selects no Weather-accent cards.
4. Coverage `0.5` selects an approximately half-sized stable card subset distributed through the complete footprint rather than only its rim.
5. Coverage `1` permits every directionally eligible card throughout the complete footprint.
6. Sweeping Softness does not change the selected-card set or create/remove a radial centre exclusion; it changes only the edge-line profile on selected blades.
7. Game and Scene views both show the same Weather-specific control response, subject only to their normal camera/rendering differences.
8. Ordinary local lights remain unchanged in Game and Scene views.
9. Multiple authored/procedural LightRays retain independent direct indexed overrides with no shader-side search.
10. The D3D12 sidecar SRV warning remains absent.
11. No recurring managed allocation or additional light loop is introduced.

### Performance model

- Controller work remains `O(A)` while updating `A` active surface Spots; source direction is already resolved from existing slot/source data and adds only bounded scalar work per active Spot.
- Renderer publication remains `O(V)` for `V` visible additional lights per eligible camera.
- GPU work remains one direct structured-buffer read per evaluated additional light. Record stride increases from 16 to 32 bytes to carry the required source direction; no extra lookup, loop, texture sample, draw, or render target is added.
- Scene View gains the same bounded `O(V)` publication required for correct visual parity. Preview/reflection/unrelated cameras continue to bind the constant zero fallback.
- Softness adds only scalar edge-profile shaping within the existing Weather override branch.
- No performance exception is required; the additional 16 bytes per evaluated additional-light record are the minimal metadata needed to preserve the accepted direction contract without a search.

### Validation

1. In the active Weather LightRay preset, test `Accent Line Intensity` at `0`, `0.25`, and `1` while keeping Coverage `1` and Softness `0.5`; verify monotonic edge response in both Game and Scene views while broad Spot illumination remains unchanged.
2. Set Intensity to a clearly visible value and Softness `0.5`; test Coverage `0`, `0.5`, and `1`. Verify none, a stable approximately half-card subset across the whole footprint, and full participation respectively, with no centre/rim selection bias.
3. Keep Intensity and Coverage fixed; test Softness `0`, `0.5`, and `1`. Verify the same cards remain selected and only their edge profile changes.
4. Confirm the ordinary local-light test retains its previous response in both views, simultaneous authored/procedural LightRays still respond, and the Console contains no `_VegetationAdditionalLightAccentData` warning.


### V1.2D2 source implementation and compliance audit — 2026-07-29

#### Actual affected files

Modified exactly:

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Rendering/Weather/WeatherLightRayRendererFeature.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl`

No Editor diagnostic, scene, prefab, material, preset asset, renderer asset, layer, tag, project-setting, cloud-population, cloud-generator, Light component, draw-call, render-target, or GPU-readback file changed.

#### Material implementation

- `WeatherLightRayController` now resolves the cached accent scale from the preset-authoritative `AccentLineIntensity` property and publishes the preset-authoritative coverage property to the retained legacy global.
- The Controller's per-Light registry now stores one inseparable parameters/source-direction pair. Each enabled proxy resolves its own normalized horizontal LightRay/source direction while retaining the real Spot for body lighting.
- `WeatherLightRayRendererFeature` mirrors the HLSL record as two sequential `Vector4` values with a 32-byte structured stride. It keeps persistent camera-local buffers and binding passes for Game and Scene View cameras, publishes records from each camera's own `RenderingData.lightData.visibleLights` order, and preserves the one-record zero fallback for unrelated cameras.
- Atmospheric mask/scatter/composite rendering remains restricted to the resolved Base Game camera.
- `VegetationLighting.hlsl` uses URP `Light.direction` for body lighting and the sidecar source direction only for Weather blade-edge selection. Invalid Weather direction suppresses Weather edge output rather than falling back to a radial Spot direction.
- Stable whole-card Coverage remains a threshold over the existing instance/card hash and is no longer spatially constrained by the Spot-centred direction field.
- Ordinary punctual-light directional shaping remains the established exponent `1.125`.
- Weather Softness no longer modifies direction/card eligibility. It shapes only the selected edge profile with exponent `2^(2 - 4s)`, giving `4` at `s=0`, `1` at `s=0.5`, and `0.25` at `s=1`.
- Prominent protected contract comments now exist at the Controller preset/registry/direction boundaries, the renderer module/publication boundary, and the shared HLSL module/evaluation boundary.
- No new diagnostic suite, Inspector button, or runtime regression machinery was added.

#### Cross-subsystem impact audit

`VegetationLighting.hlsl` is included only by `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader` in the supplied source. Directional main-light body response remains unchanged. Ordinary additional lights retain zero sidecar records, real URP direction, prior `1.125` directional shaping, prior attenuation, prior activation, and the unmodified authored `edgeMask`. Only explicit Weather override records select the new source-direction and edge-profile paths.

#### Static verification

Passed:

- exact six-file scope comparison against the V1.2D1 merged source;
- complete UTF-8 decoding, LF line endings, and NUL checks for all six files;
- balanced C#/HLSL parentheses, brackets, braces, strings, chars, and comment-state lexical scan;
- Controller/renderer API-reference audit for the expanded override signature;
- C#/HLSL two-`float4` layout and 32-byte stride consistency check;
- source checks proving preset-authoritative intensity/coverage publication;
- source checks proving per-Light direction resolution and camera-local Game/Scene publication;
- source checks proving Coverage reads only `parameters.y` and Softness reads only `parameters.z` in the final edge-profile stage;
- prohibited-path audit confirming no shader-side LightRay search, extra light loop, GPU readback, scene/asset edit, or Editor diagnostic addition.

Unity 6000.5.0f1 C# compilation, HLSL compilation, Game/Scene visual parity, control sweeps, D3D12 warning regression, and profiling remain pending because Unity is unavailable in the patch workspace. The source patch is not runtime-accepted until those project-side checks pass.

## WEATHER-LIGHT-RAY-V1.2E — Preset Selection, Activation, and Population Policy Foundation

### Status and gate order

- User approval: granted on 2026-07-29.
- Review: complete against the merged V1.2D2 source, direct callers, consumers, Time Of Day normalized-cycle provider, cloud transmission/evolution producer, renderer contracts, Inspector, and canonical Weather documents.
- This section is the mandatory first repository modification before runtime or asset-class edits.
- Implementation status: source implementation and static audit complete; Unity validation pending.

### Objective

Separate four authorities that V1.2D incorrectly coupled:

1. `WeatherLightRayPreset`: visual presentation only.
2. `WeatherLightRaySelectionProfile`: normalized-cycle eligibility, selection stability, and runtime dependencies.
3. `WeatherLightRayPopulationProfile`: reusable automatic-instance rules, cloud requirements, placement policy, and rule budgets.
4. `WeatherLightRayController`: scene bindings, normalized-cycle source, ground/focus controls, global storage and automatic-ray budget, source resolution, and execution.

No runtime selection rule may infer source ownership, cloud policy, or active period from `WeatherLightRayPreset.SourceKind`.

### Approved files

Modify:

- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Docs/Weather_System_Architecture_Provisional.md`
- `Assets/Docs/Weather_Cloud_Shadow_Handoff.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayPreset.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayPopulationRuntime.cs`
- `Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs`

Create:

- `Assets/Game/Procedural/Weather/WeatherLightRaySelectionProfile.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRaySelectionProfile.cs.meta`
- `Assets/Game/Procedural/Weather/WeatherLightRayPopulationProfile.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayPopulationProfile.cs.meta`
- `Assets/Game/Procedural/Weather/WeatherLightRaySelectionRuntime.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRaySelectionRuntime.cs.meta`

No scene, prefab, material, preset asset, renderer asset, shader, include, layer, tag, cloud generator, or project-setting change is approved.

### Reviewed evidence

- `TimeOfDayController.NormalizedTime` already exposes the required normalized `0..1` cycle and remains the only Time Of Day dependency.
- V1.2D `WeatherLightRayPopulationRuntime.ResolveSuspensionReason` and automatic opening construction are hard-coded to `Sun`; this is the coupling being removed.
- `WeatherLightRayController.TrySetActivePreset` already provides bounded presentation transitions and remains the presentation switch path.
- `WeatherCloudShadowController` already provides CPU current/future transmission queries and owns the generated-cookie pixel arrays; no GPU readback or duplicate cloud simulation is required.
- V1.2D2 vegetation accent sidecar layout, camera publication, source-direction ownership, preset authority, coverage, and softness are frozen and outside this patch.

### Accepted data contracts

#### Visual preset

`WeatherLightRayPreset` remains appearance-only. Its serialized `SourceKind` is retained as explicitly obsolete compatibility metadata in V1.2E and must not be read by production selection, source gating, or population. Removal is deferred until curated asset migration.

#### Selection profile

Each enabled entry owns:

- stable serialized ID;
- visual preset reference;
- normalized `0..1` activation curve;
- priority and authored selection weight;
- transition, minimum-hold, and cooldown durations;
- direction mode: controller directional source, vertical, or fixed world direction;
- source-availability policy: ignore, require, or multiply activation;
- cloud-projection mode: none or cloud-controller directional source;
- population profile reference.

Runtime evaluates highest-priority eligible entries, then highest effective weight. The active entry is retained through minimum hold and challenger hysteresis. No periodic random reroll is permitted.

#### Normalized-cycle source

Controller mode:

- Time Of Day: explicit reference or one unambiguous unsorted `FindObjectsByType<TimeOfDayController>` result;
- Manual normalized value;
- External runtime override.

Curves and Inspector values remain normalized `0..1`; no hour labels or named dayparts are introduced.

#### Population profile

A reusable profile contains one or more rules. Every rule owns stable ID, priority, desired/max counts, spacing, cloud-data requirement, spatial cloud policy, cloud-cover activation curve, clear-footprint threshold, distinct-opening contrast, and surrounding radius.

Cloud-data requirement:

- Ignored: no cloud query; only Any Position is valid.
- Optional: missing/disabled cloud producer is clear sky; enabled-but-unready/error data suspends the rule.
- Required: published, enabled, ready and stable cloud data is mandatory.

Spatial policy:

- Any Position;
- Clear Footprint: retain bounded 13-position x 4-time forecast validation;
- Distinct Cloud Opening: Clear Footprint plus bounded surrounding-cloud contrast; it requires cloud data.

Common sunlight uses Optional + Clear Footprint so the same rule works in cloudless skies and in openings when clouds exist.

#### Source-independent rays

Add `WeatherLightRaySourceKind.Independent = 2` without changing serialized Sun/Moon values. Independent rays use vertical or fixed direction, neutral source colour, and no directional-source gate unless an explicit dependency requires cloud projection.

#### Continuity and budgets

- Same population-profile asset plus same dependency signature preserves automatic handles during visual-preset transitions.
- Dependency-signature change retires old automatic rays before new qualification.
- Multiple active rules share the Controller global automatic-ray budget and free central slots.
- Authored and caller-created procedural rays are never evicted.
- Stable identity includes population-profile stable ID, population-rule stable ID, resolved dependency signature, population seed, cell coordinate, and the candidate index (implicit zero in V1.2E because one candidate is authored per cell).
- Selection-entry identity and visual-preset identity are deliberately excluded so appearance-only entry changes with the same population profile and dependency signature preserve handles and world candidates.

### Cloud-cover measurement

Measured normalized cloud cover is computed only when current or next cookie pixels are generated. During evolution, interpolate current and next measured cover using the same smooth progress. Steady query is O(1). No per-frame cookie scan or GPU readback is allowed.

### Backward compatibility

- New Controller default: Manual preset-control mode.
- Manual mode preserves the current active preset and legacy V1.2D single-rule automatic population fields and behaviour.
- Selection Profile mode activates only when explicitly selected and assigned.
- Automatic population remains disabled by default and Ground Mask remains Nothing.

### Production-code documentation hard stop

Module/function-level comments must explicitly protect:

- visual preset versus runtime dependency ownership;
- normalized-cycle semantics;
- enabled-but-invalid cloud data not being treated as clear sky;
- cloud-ignored rules never executing cloud qualification;
- authored/gameplay slot protection;
- compatible appearance-only transitions preserving population;
- no per-frame cookie scans or GPU readbacks;
- V1.2D2 vegetation accent contracts remaining untouched.

### File sequence

1. Add the accepted architecture section to this document.
2. Add shared enums and source-independent source kind.
3. Add selection and population profile assets.
4. Add selector runtime.
5. Add dirty-time measured cloud cover.
6. Generalize population runtime from one Sun rule to resolved selection/dependency/rule settings while preserving manual mode.
7. Integrate Controller configuration, execution, state reporting, and explicit normalized-cycle provider.
8. Integrate compact Inspector authoring and live state without adding diagnostics suites or test buttons.
9. Update parent Weather and cloud documents.
10. Audit exact scope, legacy/manual parity, no-allocation steady path, source-kind consumers, generated metadata, and all production-code comments.

### Acceptance criteria

1. Activation curves and manual/external cycle values use normalized `0..1` only.
2. Visual presets do not determine source, cloud, or active-period policy.
3. Source-dependent entries become ineligible when required dependencies fail; vertical/fixed independent entries can run without Sun or Moon.
4. Optional + Clear Footprint works with no active clouds and with clear openings under clouds.
5. Distinct Cloud Opening produces no candidates under an effectively clear sky.
6. Ignored + Any Position does no cloud transmission work and survives cloud evolution.
7. Enabled-but-invalid cloud data is never silently treated as clear sky.
8. Compatible appearance-only transitions preserve automatic handles and positions.
9. Dependency changes retire and requalify population.
10. Multiple rules share one Controller budget and never evict authored/gameplay rays.
11. Manual mode preserves accepted V1.2D behaviour.
12. V1.2D2 vegetation accent behaviour and SRV binding remain unchanged.
13. No GPU readback, no per-frame cookie scan, no new render pass, no recurring managed allocation after initialization.

### Performance model

- Selection: O(entries) at bounded selector cadence, default 4 Hz.
- Rule allocation: O(active rules) at population cadence.
- Clear Footprint: 52 CPU texture samples per full candidate evaluation.
- Distinct Cloud Opening: maximum 68 CPU texture samples per full candidate evaluation.
- Cloud-cover measurement: O(cookie pixels) at generation/evolution preparation only; O(1) steady query.
- GPU cost changes only through the accepted number of active LightRay instances under existing budgets.

### Non-goals

- No curated Selection/Population assets or scene assignment in V1.2E.
- No Moon-light contract.
- No Time Of Day hour authoring.
- No shader/rendering changes.
- No connected-component cloud opening extraction.
- No deletion of legacy preset `SourceKind` or manual population controls before migration validation.



### V1.2E implementation result and final source audit

Source implementation completed within the approved fifteen-file scope.

Implemented production behaviour:

- `WeatherLightRaySelectionProfile` owns normalized `0..1` eligibility curves, priority, authored weight, transition/hold/cooldown stability, explicit direction/source availability, cloud-projection dependency, and a reusable population-profile reference.
- `WeatherLightRayPopulationProfile` owns one or more independently budgeted rules with cloud-data requirement, spatial cloud policy, measured-cover activation, spacing, clearance, and distinct-opening contrast.
- `WeatherLightRaySelectionRuntime` evaluates at the profile cadence with no periodic random reroll and no recurring managed allocation after profile initialization.
- `WeatherLightRayController` defaults to Manual mode, supports Time Of Day/manual/external normalized-cycle providers, resolves source-independent vertical/fixed entries, preserves compatible populations, and fully retires incompatible populations before replacement qualification.
- Candidate identity derives from population-profile stable ID, rule stable ID, dependency signature, seed, cell, and the implicit zero candidate index; visual preset and selection-entry identity do not participate.
- `WeatherLightRayPopulationRuntime` consumes only Controller-resolved source/cloud policy, supports Ignored/Optional/Required cloud data and Any/Clear/Distinct placement, and retains the bounded 52-sample clear-footprint or 68-sample distinct-opening maximum.
- `WeatherCloudShadowController` measures normalized cloud cover only when current/next CPU cookie pixels are generated and returns an O(1) interpolated value during evolution.
- The custom Inspector exposes compact selection and reusable-population configuration without adding a diagnostic suite or new test actions.
- Manual mode retains the validated V1.2D Sun/Required/Clear-Footprint path until curated profile migration is explicitly approved.

Actual affected files exactly match the approved list above. No scene, prefab, material, preset asset, renderer asset, shader/include, cloud generator, layer, tag, or project setting changed.

Static audit completed:

- exact fifteen-file scope versus the V1.2D2 merged source;
- C# lexical state and balanced delimiter checks across all nine affected C# files;
- unique new `.meta` GUID and archive-path checks;
- Controller/Editor serialized-property wiring checks;
- `WeatherLightRayPopulationRuntime.Settings` constructor/call-site argument-count checks;
- production-source search confirming no selection or population read of `WeatherLightRayPreset.SourceKind`;
- source-kind consumer audit for the appended serialized-safe `Independent = 2` value;
- stable-identity audit confirming visual preset and selection entry are excluded;
- cloud-policy audit confirming Ignored/Any performs zero cloud samples and Optional distinguishes absent/disabled from enabled-but-unready producers;
- performance-boundary audit confirming no GPU readback, no per-frame cookie scan, no new render pass, and no shader change;
- UTF-8, NUL, unsafe archive-path, and package byte-comparison checks.

Unity 6000.5.0f1 C# compilation, Inspector asset authoring, normalized-cycle switching, compatible/incompatible population transitions, cloud-policy matrix, allocation profiling, and D3D12 regression validation remain project-side pending.

## WEATHER-LIGHT-RAY-V1.2E-A — Unity 6.5 Unsorted Time-of-Day Discovery API Closure

### Status and scope

- Trigger: Unity 6000.5.0f1 emitted `CS0618` for the deprecated `FindObjectsByType<T>(FindObjectsSortMode)` overload in `WeatherLightRayController.ResolveTimeOfDayController`.
- Objective: preserve the accepted one-shot, active-object, unsorted Time Of Day discovery contract while using the current Unity 6.5 overload.
- Approved files:
  - `Assets/Docs/Weather_Light_Ray_Architecture.md`
  - `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- Non-goals: no selection, population, Time Of Day, source, cloud, rendering, vegetation, Inspector, serialized-default, or lifecycle behaviour change.

### Reviewed evidence and invariant

- `ResolveTimeOfDayController` performs discovery only once per enable/refresh and accepts a discovered controller only when exactly one active `TimeOfDayController` exists.
- Unity 6000.5.0f1 deprecates the overload taking `FindObjectsSortMode`; the replacement no-argument overload is unsorted and excludes inactive objects, matching the existing discovery contract.
- Explicitly assigned controllers remain subject to the existing `isActiveAndEnabled` gate.
- Zero or multiple discovered controllers remain a suspension condition; no arbitrary controller selection is permitted.

### Implementation sequence and acceptance

1. Record this closure plan before code modification.
2. Replace only the deprecated discovery call with `FindObjectsByType<TimeOfDayController>()`.
3. Re-audit discovery reset sites, ambiguity reporting, and the final two-file diff.
4. Unity acceptance: both `CS0618` warnings disappear and normalized-cycle selection behaviour remains unchanged.

### Performance

No runtime cost change. Discovery remains one allocation and one unsorted scene query per enable/explicit refresh, never per frame or selection tick.

### Implementation result and audit

- Replaced only `FindObjectsByType<TimeOfDayController>(FindObjectsSortMode.None)` with `FindObjectsByType<TimeOfDayController>()`.
- Discovery remains one-shot per `OnEnable`, `OnValidate`, or `RefreshNow`; no selection-tick or frame query was introduced.
- Explicit-reference handling, active/enabled checks, zero/multiple-controller ambiguity handling, and normalized-cycle evaluation are unchanged.
- Final diff contains exactly the two approved files. Static source search confirms `WeatherLightRayController.cs` no longer references `FindObjectsSortMode`.
- Unity 6000.5.0f1 compilation remains project-side pending; acceptance requires both reported `CS0618` warnings to be absent.


## WEATHER-LIGHT-RAY-V1.2E-B — Curated Selection and Population Assets

### Status and gate order

- User approval: granted on 2026-07-29 to create and wire the reusable Selection and Population assets while leaving the scene asset untouched.
- Review: complete against the V1.2E-A merged source, `WeatherLightRaySelectionProfile`, `WeatherLightRayPopulationProfile`, `WeatherLightRaySelectionRuntime`, Controller selection/population consumption, the existing curated LightRay visual preset GUIDs, and the current normalized-cycle contract.
- This section is the mandatory first repository modification before serialized asset creation.
- Implementation status: planned; serialized asset creation, exact-reference audit, and Unity import validation pending.

### Objective

Provide a usable curated asset set so project-side validation requires only assigning one supplied Selection Profile to the existing `WeatherLightRayController`; the user must not manually reconstruct profile entries, population rules, curves, or asset references.

### Approved files

Modify:

- `Assets/Docs/Weather_Light_Ray_Architecture.md`

Create:

- `Assets/Game/Demo/Profiles/Weather/LightRays/WeatherLightRayPopulation_Daylight.asset`
- `Assets/Game/Demo/Profiles/Weather/LightRays/WeatherLightRayPopulation_Daylight.asset.meta`
- `Assets/Game/Demo/Profiles/Weather/LightRays/WeatherLightRayPopulation_IndependentNight.asset`
- `Assets/Game/Demo/Profiles/Weather/LightRays/WeatherLightRayPopulation_IndependentNight.asset.meta`
- `Assets/Game/Demo/Profiles/Weather/LightRays/WeatherLightRaySelection_DefaultCycle.asset`
- `Assets/Game/Demo/Profiles/Weather/LightRays/WeatherLightRaySelection_DefaultCycle.asset.meta`

No scene, prefab, material, existing preset asset, preset catalog, renderer asset, script, shader/include, layer, tag, or project-setting edit is approved.

### Curated asset contracts

#### Daylight population profile

Contains two simultaneous rules sharing the Controller global automatic-ray budget:

1. `Common Clear-Footprint Rays`
   - priority `10`;
   - desired `2`, maximum `3`;
   - minimum spacing `12 m`;
   - cloud data `Optional`;
   - spatial policy `Clear Footprint`;
   - cloud-cover activation flat at `1`;
   - minimum clearance `0.75`.

2. `Dramatic Cloud Openings`
   - priority `20` so the single dramatic slot is allocated before the common rule when both qualify;
   - desired and maximum `1`;
   - minimum spacing `16 m`;
   - cloud data `Required`;
   - spatial policy `Distinct Cloud Opening`;
   - cloud-cover activation begins at zero under clear sky and reaches full weight by moderate cloud cover;
   - minimum clearance `0.78`;
   - minimum surrounding contrast `0.20`;
   - surrounding sample radius `3 m`.

This profile proves the accepted requirement that ordinary sunlight can populate both cloudless conditions and cloud openings without duplicating the visual preset, while a separate bounded rule can require recognizable surrounding cloud structure.

#### Independent-night population profile

Contains one `Independent Night Rays` rule:

- priority `0`;
- desired `2`, maximum `3`;
- minimum spacing `12 m`;
- cloud data `Ignored`;
- spatial policy `Any Position`;
- no cloud qualification or evolution dependency.

This profile validates source-independent night-capable rays without inventing a Moon-light contract.

#### Default-cycle selection profile

Uses normalized `0..1` curves only and wires four entries:

- `Night — Independent Cold`: `WeatherLightRay_MoonCold`, source-independent vertical direction, independent-night population, wraparound night curve, priority `10`.
- `Dawn — Sun Clear`: `WeatherLightRay_SunClear`, Controller directional Sun with required availability, daylight population, narrow early-cycle transition curve, priority `20`.
- `Day — Sun Warm`: `WeatherLightRay_SunWarm`, identical Sun/cloud dependencies and daylight population, broad mid-cycle curve, priority `10`.
- `Dusk — Sun Hazy`: `WeatherLightRay_SunHazy`, identical Sun/cloud dependencies and daylight population, narrow late-cycle transition curve, priority `20`.

Dawn, Day, and Dusk intentionally reference the same population profile and dependency signature so visual transitions preserve stable automatic handles and world positions. Night intentionally changes dependency signature and population profile, causing the accepted fade/requalify transition.

### Serialized-reference invariants

- Every asset and every entry/rule receives a unique stable GUID/ID.
- Selection entries reference existing visual preset asset GUIDs from `WeatherLightRayPresetCatalog.asset`; no existing preset is copied or modified.
- Selection entries reference the newly created Population Profile asset GUIDs.
- Script references use the committed V1.2E Selection/Population profile script GUIDs.
- Curves are serialized in Force Text YAML with normalized `0..1` keys only.
- No asset references the scene or any scene object.

### Validation and acceptance

1. Unity imports all six serialized files with no missing-script or broken-reference warnings.
2. `WeatherLightRaySelection_DefaultCycle` displays four correctly referenced entries and no null preset/population references.
3. `WeatherLightRayPopulation_Daylight` displays both accepted rules and valid cloud-policy combinations.
4. `WeatherLightRayPopulation_IndependentNight` displays one Ignored + Any Position rule.
5. Assigning only the supplied Selection Profile to the existing Controller is sufficient to perform the V1.2E runtime validation; no manual profile authoring is required.
6. Exact final diff contains only this document and the six new serialized asset/meta files.

### Performance

Serialized asset creation has no additional runtime algorithmic cost. The curated daylight profile requests at most four rule-owned rays before the Controller global budget is applied; the independent-night profile requests at most three. Actual runtime cost remains bounded by the existing Controller global automatic-ray maximum and free central slots.

### V1.2E-B implementation result and static audit

Serialized asset creation completed within the approved seven-file scope.

Created:

- `WeatherLightRayPopulation_Daylight` with the accepted Optional + Clear Footprint common rule and Required + Distinct Cloud Opening dramatic rule.
- `WeatherLightRayPopulation_IndependentNight` with one Ignored + Any Position source-independent rule.
- `WeatherLightRaySelection_DefaultCycle` with normalized-cycle Night, Dawn, Day, and Dusk entries wired to the existing Moon Cold, Sun Clear, Sun Warm, and Sun Hazy visual presets and the two new population profiles.

Intentional continuity:

- Dawn, Day, and Dusk share one population-profile asset and identical source/cloud dependency fields, so their visual transitions retain the same population dependency signature and may preserve stable handles.
- Night uses a vertical independent dependency and separate population profile, so transition into or out of night follows the accepted retire/requalify path.
- The daylight profile’s priority ordering allocates its single dramatic-opening maximum before the common-rule maximum when both are active; the Controller global automatic-ray budget remains authoritative.

Static audit completed:

- exact diff contains this document plus six new asset/meta files only;
- every new asset GUID and every serialized profile/entry/rule stable ID is unique;
- all script references match the committed V1.2E profile script GUIDs;
- all visual-preset references match GUIDs already recorded in `WeatherLightRayPresetCatalog.asset`;
- every Selection Profile population reference resolves to one of the new asset GUIDs;
- all activation and cloud-cover curve keys remain within normalized `0..1` coordinates and values;
- all cloud requirement/spatial-policy combinations satisfy `OnValidate` invariants;
- no scene, prefab, existing preset/catalog, script, shader, renderer, material, layer, tag, or project-setting file changed;
- UTF-8, NUL, archive-path, and serialized-reference checks passed.

Unity 6000.5.0f1 import and runtime validation remain pending. Acceptance requires the three assets to import without missing-script/broken-reference warnings and the supplied Selection Profile to operate after assignment to the existing scene Controller. The scene remains intentionally unmodified.

## WEATHER-LIGHT-RAY-V1.2E-B1 — Dedicated Ground-Layer Binding

### Status and approval

- User approval: granted on 2026-07-29 for the exact project-configuration change recorded here.
- Review: complete against current working state and commit `e5d9be79a1bfa8f5756713a407fa6c7227c7bc8b`.
- Implementation: complete for the approved serialized configuration; visible Unity validation remains pending.

### Objective and acceptance criteria

Create one explicit physics ownership boundary for automatic LightRay ground projection without changing population enablement, counts, selection, clouds, rendering, generated geometry, or descendant object layers.

Acceptance requires:

1. user layer index `8` is named `Ground`;
2. `Blockout > Ground_Blockout` changes from layer `0` to layer `8`;
3. its direct descendants retain their reviewed layers: `GM_Test = 0`, `River_Strip = 4`, and `Vegetation = 0`;
4. `Systems > Weather > WeatherLightRayController.automaticPopulationGroundMask` changes from `0` to `256` (`1 << 8`);
5. automatic population remains disabled and all ray-count/tuning values remain unchanged;
6. Force Text serialization and unrelated working-tree changes are preserved.

### Approved and reviewed files

| Exact file | Role, reviewed evidence, and approved action |
|---|---|
| `Assets/Docs/Weather_Light_Ray_Architecture.md` | Canonical plan and validation ledger. Append this plan first, then record audit evidence and pending Unity checks. |
| `ProjectSettings/TagManager.asset` | Complete file reviewed. Index `8` is the first empty user-layer slot; name that slot `Ground` and change nothing else. |
| `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity` | Complete target GameObject, its components and direct children, and the complete Controller component were reviewed. Change only the ground GameObject layer and Controller mask bits. |
| `Assets/Game/Procedural/Weather/WeatherLightRayController.cs` | `automaticPopulationGroundMask`, `BuildLegacyAutomaticPopulationSettings`, `BuildSelectionPopulationSettings`, and `BuildDisabledPopulationSettings` prove the serialized mask is the shared runtime input. Read-only. |
| `Assets/Game/Procedural/Weather/WeatherLightRayPopulationRuntime.cs` | `ResolveSuspensionReason`, `TryResolveFocus`, `TryRaycastViewportGround`, and `TryAcquireGround` prove a non-empty mask is mandatory and bounds both focus and candidate raycasts. Read-only. |
| `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs` | `DrawAutomaticPopulation` exposes the mask and warns when it is empty. Read-only. |
| `Assets/Game/Procedural/Rivers/StylizedRiver.cs` | Corridor creation and refresh copy the parent Generated Ground layer to the generated corridor; this is a documented regeneration risk, not an approved edit. |
| `Assets/Docs/Weather_System_Architecture_Provisional.md` | Confirms explicit user-assigned physics-mask ownership and that the earlier source patch intentionally added no layer or scene edit. Read-only. |

### Invariants, non-goals, and risks

- Do not recursively change child layers.
- Do not enable automatic population or change its desired/global maximum counts.
- Do not edit scripts, shaders, materials, prefabs, renderer assets, generated geometry, tags, or collision-matrix settings.
- The Ground GameObject owns both renderer and collider; the layer change therefore applies to both. Current camera/light culling must be checked in Unity.
- A later River corridor creation or refresh may copy layer `8` to the generated corridor. Validate river-area placement after any such regeneration.
- Unity may be open with unsaved scene state. The exact serialized target values must be rechecked after import/save to detect an overwrite.

### File sequence and validation

1. Record this plan before serialized or project-setting edits. **Complete.**
2. Name user layer index `8` and verify all other layer names and indices are byte-equivalent. **Complete.**
3. Change only the approved ground GameObject layer and Controller mask. **Complete.**
4. Compare exact target blocks before/after, verify direct-child layers and unrelated target-component fields, and inspect the scoped diff. **Complete.**
5. Allow Unity 6000.5.0f1 to import, then confirm layer display, Inspector binding, collision/render continuity, and no missing-reference, C#, shader, or D3D12 SRV error. **Pending.**

### Implementation and audit result

- `ProjectSettings/TagManager.asset`: the only intentional difference is user layer index `8`, changed from empty to `Ground`; all other layer names, tag data, sorting layers, and collision-matrix data remain unchanged.
- `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity`: the only intentional differences for this configuration are `Ground_Blockout.m_Layer: 0 -> 8` and `WeatherLightRayController.automaticPopulationGroundMask.m_Bits: 0 -> 256`.
- The direct descendants remain non-recursive: `GM_Test = 0`, `River_Strip = 4`, and `Vegetation = 0`.
- `automaticPopulationEnabled` remains `0`; desired count remains `3`; global maximum remains `6`. Enabling population and changing the maximum to `3` remain part of the separate runtime-validation setup, not this approved configuration change.
- The reviewed Controller, population runtime, Controller Inspector, and river implementation files are byte-identical to the pre-edit review hashes; no script, shader, material, prefab, renderer, geometry, tag, or collision-matrix edit was made.
- Targeted serialized assertions passed: exactly one project layer is named `Ground`, exactly one GameObject in the target scene uses layer `8`, the Controller mask is exactly bit `8`, and the three direct-child layers remain unchanged.
- Force Text line structure was preserved: the scene retains `244340` lines and changes by only two bytes, corresponding to the two approved scalar replacements.
- Unity 6000.5.0f1 observed and imported the modified scene successfully. The latest compilation completed successfully before that import, and no later C# compilation, shader, D3D12, or SRV error appears in `Logs/Editor.log`.
- The repository-wide scoped `git diff --check` is not clean because the pre-existing working-tree diff contains trailing whitespace outside these three configuration changes. The three edited serialized lines and this new plan section pass targeted trailing-whitespace checks; unrelated whitespace was preserved.
- Pending Unity evidence: confirm the named layer and non-recursive GameObject assignment in the Inspector, confirm the Controller mask displays only `Ground`, verify the ground still renders/collides, and run the requested automatic-population Play Mode validation. Any visible Inspector mismatch, overwrite from unsaved scene state, missing reference, or runtime error must be reported before acceptance.
