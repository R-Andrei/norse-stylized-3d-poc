# Weather LightRay Architecture and V1 Implementation Plan

## A. Identity and status

**Architecture identifier:** `WEATHER-LIGHT-RAY-V1`

**Documentation patch identifier:** `WEATHER-LIGHT-RAY-DOC-V0.1`

**Decision date:** 2026-07-24

**Status:** V1.0C cloud-projection alignment is accepted. V1.1 compiled and rendered one authored hybrid ray, but the visual proof was rejected because the shader produced one broad translucent region rather than distinguishable sunlight shafts. `WEATHER-LIGHT-RAY-V1.1A/B` source implementation and supplied-file static audit are complete: the authored anchor now uses the shared per-ray behavior contract and the hybrid renderer now produces a sparse multi-strand bundle with a faint envelope and separate surface illumination. Unity compilation, visual calibration, and performance validation remain pending.

This document is the canonical architecture, implementation plan, and validation ledger for Weather-owned stylized celestial light rays. It supersedes the undefined godrays exploration boundary recorded in:

- `Assets/Docs/Weather_System_Architecture_Provisional.md`;
- `Assets/Docs/Weather_Cloud_Shadow_Handoff.md`;
- the external `Weather_Godrays_Exploration_Handoff_2026-07-23.md` continuation handoff.

The feature name is **LightRay**. “Godray,” “sunray,” and “moonray” describe authored or source-specific presentations; they are not separate runtime systems.

The V1 visual target is a mandatory hybrid representation:

```text
Authoritative persistent world-space LightRay zones
    +
world-space proxy volumes used only to bound rendering work
    +
depth-aware screen-space shaft, surface-light, and ground-footprint presentation
```

A directly visible low-poly prism is not an accepted sole rendering path. There is no world-space-only quality tier in the approved V1 target.

---

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

The following read-only evidence was reviewed before this plan was written.

| Evidence | Exact finding | Effect on the LightRay plan |
|---|---|---|
| `Assets/AGENTS.md` — mandatory implementation workflow | Requires complete review, persistent plan before implementation, exact approved scope, post-change audit, and honest pending validation. | This documentation patch precedes all runtime edits. Every future LightRay patch must update this plan before implementation. |
| `Assets/Docs/Weather_System_Architecture_Provisional.md` — sections 2.1, 9, 11, and former “Godrays adjacency boundary” | Weather owns cloud transmission; `TimeOfDayController` owns the sun; 1440p low-end-PC 60 FPS is the target; the earlier godray representation was undefined. | LightRay becomes a separate Weather presentation and gameplay-zone subsystem without changing the cloud receiver path. |
| `Assets/Docs/Weather_Cloud_Shadow_Handoff.md` — sections F, L, O, P, and S | `WEATHER-CLOUD-SHADOW-V0.4` is frozen on one native URP directional cookie. The cookie is globally tiled, Weather-wind-driven, readable `R8`, and evolved by a bounded crossfade. | LightRay consumes cloud state but does not replace, puncture, or double-apply the receiver cookie. |
| `Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs` — `GeneratedCookie`, `CurrentCookieOffset`, `CookieWorldSizeMetres`, `ResolvedSun`, `EvolutionInProgress`, `EvolutionProgress`, `TickController`, `UpdateCookieEvolution`, `ApplyCookieToCapturedSun` | The controller already exposes the active texture, sun, projected offset, world period, and transition state. The world-space phase and exact CPU transmission sampling are not public contracts. | A minimal source-neutral cloud-transmission query must be added and validated against the installed cookie projection. The frozen receiver path remains unchanged. |
| `Assets/Game/Procedural/Weather/WeatherCloudShadowCookieGenerator.cs` — `GeneratePixels`, `CreateTexture`, `UploadPixels` | The cloud field is retained sunlight transmission in a readable seamless `R8` texture. `1` is open light and lower values are cloud shade. | LightRay cloud eligibility can use the exact generated field without a second cloud simulation or GPU readback. |
| `Assets/Game/Scripts/Environment/Lighting/TimeOfDayController.cs` — `ApplyCurrentState`, `ApplySun`, `RenderSettings.sun` | The current controller rotates and configures one directional sun. It does not expose a moon source. | Sun is the only implementable celestial source in the first runtime slice. The LightRay abstraction must support Moon from the first type design without inventing moon state. |
| `Assets/Game/Scripts/Environment/Lighting/TimeOfDayProfile.cs` — `TimeOfDayLightingState`, default checkpoints | The current lighting state contains only sun colour and intensity. Night checkpoints set sun intensity to zero. | Moon direction, colour, intensity, and availability require a separately approved Time-of-Day contract before moonray runtime integration. |
| `Assets/Settings/PC_Renderer.asset` | The PC Universal Renderer currently contains SSAO as its only serialized Renderer Feature. | LightRay will be the project’s first custom Weather screen-space Renderer Feature. Attachment must be performed and validated in Unity, not raw-edited speculatively. |
| `Assets/Settings/PC_RPAsset.asset` — `m_RequireDepthTexture: 1`, `m_RequireOpaqueTexture: 1`, `m_SupportsHDR: 1` | PC URP already requests depth and opaque textures and supports HDR. | The hybrid pass may consume camera depth without changing the current PC RP Asset requirement. Exact frame resources must still be verified in Render Graph Viewer. |
| Source search for `ScriptableRendererFeature` and `ScriptableRenderPass` | No project-owned custom Renderer Feature or Render Pass exists in the supplied archive. | V1 must establish project conventions rather than copy a local implementation. |
| Unity 6 URP Render Graph documentation | Unity 6 custom Renderer Features enqueue `ScriptableRenderPass` instances; `RecordRenderGraph` declares frame resources; Render Graph manages transient resources; a blit destination may replace `resourceData.cameraColor` to avoid a copy-back pass. | V1 uses the Unity 6 Render Graph path, not Compatibility Mode. Pass count and resource declarations must remain minimal. |
| Supplied `Assets-Code-Archive(20).zip` | The archive contains no `.git` directory, package manifest, project settings directory, or existing `.meta` files for prospective assets. | Branch, HEAD, working-tree drift, exact URP package revision, GUIDs, and live Unity compilation are unverified. The live repository must be audited before runtime work. |

Reviewed external Unity references:

- [Example of a complete Scriptable Renderer Feature in URP](https://docs.unity3d.com/6000.1/Documentation/Manual/urp/renderer-features/create-custom-renderer-feature.html)
- [Write a render pass using the Render Graph system in URP](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/render-graph-write-render-pass.html)
- [Read or write to a texture in a render pass in URP](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/render-graph-read-write-texture.html)
- [Introduction to the Render Graph system in URP](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/render-graph-introduction.html)
- [Optimize a Render Graph](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/render-graph-optimize.html)

---

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
- **Proxy volume:** coarse world-space geometry used only to bound GPU rasterization. It is not the visible final shaft surface.
- **Atmosphere mask:** low-resolution screen-space data describing shaft radiance/density.
- **Surface influence:** screen-space data describing visible surface and ground brightening inside the world-space LightRay zone.
- **Cloud compensation:** additional stylized surface lift used by cloud-ignoring rays to suggest that divine light penetrates or opens dense clouds.

---

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

- No per-ray procedural GameObject.
- No per-ray material.
- No per-ray Unity Light.
- No per-ray trigger collider by default.
- No full-scene renderer or material traversal per frame.
- No GPU readback for placement or gameplay.
- No modification of the frozen cloud receiver-cookie path.
- No new layer or tag without separate approval.
- No simultaneous procedural Sun and Moon source groups in V1.
- No visible world-space-only LightRay quality tier in V1.
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

### H.3 Shaft appearance

- source colour;
- shaft intensity;
- atmospheric density;
- taper;
- base and top radius relationship;
- internal-band count;
- internal-band contrast;
- fluctuation strength;
- fluctuation speed;
- vertical phase drift;
- edge softness.

### H.4 Surface and footprint appearance

- surface-light strength;
- centre emphasis;
- radial falloff;
- irregular boundary strength;
- cloud-compensation strength;
- maximum additive lift;
- colour contribution;
- optional ground-contact emphasis.

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
World-space base centre
World-space direction
Height
Base ellipse axes
Top ellipse axes
Visual intensity multiplier
Surface-light multiplier
Cloud-compensation multiplier
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

Instead, the LightRay composite adds:

- normal shaft radiance;
- surface-light boost;
- configurable cloud-compensation lift.

This can imply that divine light creates or pierces an opening even under complete cloud shade. The approximation is intentionally stylized and does not attempt to reconstruct each receiver’s missing physical BRDF contribution.

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

## N. Mandatory hybrid rendering architecture

### N.1 Outcome

The V1 renderer is not “a visible prism plus optional blur.”

The coarse world-space proxy exists only to bound affected pixels. The final shaft and surface-light appearance is produced by analytic world-space evaluation in screen-space passes.

This prevents proxy facets from defining the final image and provides depth integration without real volumetric lights.

### N.2 Renderer Feature

Planned component:

```text
WeatherLightRayRendererFeature : ScriptableRendererFeature
```

Planned pass implementation:

```text
WeatherLightRayRenderPass : ScriptableRenderPass
```

The feature uses Unity 6 Render Graph through `RecordRenderGraph`.

Compatibility Mode is not the planned implementation.

### N.3 Camera eligibility

By default, render only for the designated gameplay base camera.

Skip unless separately enabled:

- Scene preview cameras;
- reflection cameras;
- shadow cameras;
- overlay cameras;
- editor thumbnails;
- unrelated secondary cameras.

Remote gameplay or cutscene cameras may become the designated camera through an explicit runtime focus/target contract.

### N.4 Injection point

Initial planned injection:

```text
After transparents and before post-processing
```

Reason:

- Vegetation, River, particles, and other transparent presentation should be present before the final LightRay composite.
- Existing post-processing, colour grading, and bloom should process the LightRay result.

The exact `RenderPassEvent` must be verified against Unity 6000.5.0f1 in the live project before code is frozen.

### N.5 GPU ray data

A fixed-capacity GPU buffer stores compact renderer data for active rays.

Planned characteristics:

- no allocation after warm-up;
- one fixed maximum capacity per quality tier;
- update only when authoritative ray state changes materially;
- lifecycle timing and subtle fluctuation evaluated in shader where practical;
- one active celestial source group per camera.

### N.6 Proxy geometry

Use one retained expanded tapered proxy mesh and one instanced draw.

The proxy:

- encloses the analytic LightRay volume;
- is not directly composited as the final visible surface;
- does not write depth;
- exists only to limit screen pixels that execute analytic volume evaluation;
- uses conservative expansion so the analytic soft boundary is not clipped.

An eight- or twelve-sided proxy is a provisional engineering choice, not a visible art constraint.

### N.7 Analytic volume

The shader evaluates a tapered elliptical world-space volume defined by:

- base centre;
- direction;
- height;
- base ellipse axes;
- top ellipse axes;
- bounded source lean;
- soft radial and vertical falloff.

The world-space volume, not proxy triangles, defines:

- shaft body;
- footprint;
- visible-surface influence;
- gameplay-zone correspondence.

### N.8 Pass 1 — LightRay mask

Initial render target:

```text
Quarter-linear-resolution RGBA16F
```

At 2560 × 1440 this is 640 × 360.

Planned channels:

```text
R = atmospheric shaft density/radiance weight
G = visible-surface and ground light influence
B = cloud-bypass compensation influence
A = stylized core/contact emphasis
```

The mask pass:

- draws active proxy instances once;
- samples camera depth;
- reconstructs visible world position;
- analytically tests ray-volume intersection;
- estimates view-ray travel through the volume for atmosphere density;
- evaluates surface influence at the scene-depth position;
- applies deterministic irregularity and internal bands;
- accumulates overlapping rays with bounded blending;
- uses no per-receiver material change.

### N.9 Pass 2 — directional atmospheric scatter

Initial render target:

```text
Quarter-linear-resolution R16F
```

The pass:

- reads mask R;
- reads camera depth;
- scatters along the projected active celestial-source direction;
- uses depth rejection to reduce bleeding across major depth discontinuities;
- uses a bounded sample count selected by quality tier;
- does not scatter surface channels G/B/A;
- uses one direction because Sun and Moon groups are mutually exclusive.

This is localized shaft enhancement, not a whole-screen radial sunburst.

### N.10 Pass 3 — composite and camera-colour replacement

The composite:

- reads the current active camera colour;
- reads camera depth;
- reads the original LightRay mask and scattered atmosphere;
- performs depth-aware upsampling;
- adds atmospheric shaft colour;
- applies surface-light and cloud-compensation lift;
- applies source-profile clamps and colour;
- writes one destination camera-colour texture;
- updates Render Graph frame data to use that destination as camera colour instead of blitting back.

The exact Unity 6000.5 API must follow the live package source. The plan explicitly avoids an unnecessary destination-to-camera copy when Render Graph supports frame-data replacement.

### N.11 Surface and ground footprint

The ground footprint is generated from the same world-space analytic zone as gameplay influence.

At every visible scene-depth sample inside the volume, the mask computes:

- radial footprint falloff;
- centre emphasis;
- irregular boundary modulation;
- source and ray intensity;
- cloud-compensation contribution when applicable.

This naturally affects ground, rocks, characters, structures, River presentation, and Vegetation pixels in the composite without requiring each material to implement a LightRay receiver path.

This is a stylized screen-space lighting contribution. It does not claim physically correct normals, shadows, or material response.

### N.12 Transparent-receiver limitation

The pass runs after transparents, but camera depth may still describe the opaque surface behind transparent pixels.

Therefore River and Vegetation surface lift is approximate. The V1 validation scene must explicitly test whether the result is visually coherent.

If the approximation fails materially, any receiver-specific integration requires a new cross-subsystem shader audit and plan. It is not pre-authorized.

### N.13 Sky pixels

Atmospheric shaft contribution may appear where the analytic volume intersects the view ray even when no finite surface depth exists.

Surface-light and ground-footprint channels require a valid scene-depth position and must not brighten the sky as though it were ground.

### N.14 Temporal behavior

V1 uses no history buffer and no temporal reprojection.

Stability comes from:

- persistent world-space zones;
- deterministic ray seeds;
- shader-time fluctuation tied to stable instance data;
- bounded low-frequency internal animation;
- no camera-relative respawn.

Temporal accumulation may be considered only if V1 exhibits unacceptable low-resolution shimmer.

---

## O. Visual behavior

### O.1 Stationary default

The authoritative base footprint remains stationary in V1.

Cloud-respecting rays are selected only where forecasted openings remain valid long enough.

### O.2 Subtle internal evolution

Each ray may contain several deterministic internal bands with varied:

- phase;
- width;
- vertical position;
- intensity;
- fluctuation frequency.

The broad ray remains stable while small regions brighten and dim.

Initial target behavior:

- no obvious rhythmic global pulse;
- no rapid scrolling texture;
- no large lateral sweep;
- no visible card intersection;
- no hard proxy silhouette.

### O.3 Optional movement reservation

Actual ray movement is not part of the first implementation.

The data model retains movement policy so future work can add:

- cloud-locked drift;
- limited wander;
- target following.

Any moving footprint must update gameplay influence and cloud validation from the same authoritative state.

### O.4 Source direction and stylized lean

The visible ray follows the source direction only within a configurable maximum lean.

At low elevation the source is gated off before extreme diagonal shafts appear.

### O.5 Brightness semantics

Two contributions are distinct:

1. **Surface light boost** — makes the world zone brighter than ordinary local lighting.
2. **Cloud compensation** — additional lift for cloud-ignoring rays so full overcast can still appear penetrated by divine light.

Neither modifies the cloud cookie.

### O.6 Irregularity

Gameplay influence may remain an analytic ellipse while the visible boundary uses deterministic modulation.

This avoids obvious repeated perfect circles without making gameplay queries expensive or ambiguous.

---

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
- proxy meshes;
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
- radius, ellipse, height, taper, and intensity overrides;
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

Every enabled tier uses the hybrid renderer.

The tiers scale:

- active-ray budget;
- mask resolution;
- scatter sample count;
- cloud forecast samples;
- footprint samples;
- optional occlusion samples.

They do not replace the hybrid result with a directly visible prism.

### R.2 Provisional engineering tiers

These are starting budgets, not frozen art defaults.

| Tier | Maximum visible rays | Mask scale | Scatter taps | Cloud forecast quality |
|---|---:|---:|---:|---|
| Low | 4 | 1/4 linear | 4 | centre + perimeter, three time samples |
| Medium | 8 | 1/4 linear | 6 | centre + perimeter, four time samples |
| High | 12 | 1/2 linear | 8 | additional ring and time sample |

A global Off state remains available. Off is not a visual quality tier.

### R.3 Quarter-resolution transient memory

At 2560 × 1440:

```text
RGBA16F 640 × 360 mask  ~= 1.76 MiB
R16F    640 × 360 scatter ~= 0.44 MiB
Known LightRay mask/scatter total ~= 2.20 MiB
```

Render Graph transient lifetime and aliasing are expected to reduce persistent allocation. The full-resolution composite destination inherits the active camera format; its exact memory must be reported from the live Render Graph and must not be guessed from the archive.

### R.4 Draw and pass target

Planned V1 steady visible work:

- one instanced proxy-volume mask draw;
- one directional scatter pass;
- one full-resolution composite pass;
- no compute dispatch;
- no per-ray draw call;
- no persistent full-screen LightRay texture;
- no recurring managed allocation after warm-up.

### R.5 CPU target

- lifecycle and source arbitration: `O(active rays)`;
- procedural candidate evaluation at a bounded low cadence;
- cloud sampling only during candidate/revalidation work;
- no per-frame physics validation;
- no renderer traversal;
- fixed-capacity buffers after warm-up.

### R.6 Acceptance budgets

V1 does not freeze on Editor evidence alone.

Target acceptance at 2560 × 1440 on a representative low-end PC Player build:

```text
Incremental GPU median at Medium maximum-ray case: <= 1.0 ms
Incremental GPU high-percentile stress delta: <= 1.5 ms
Incremental CPU median: <= 0.20 ms
Recurring managed allocation after warm-up: 0 B/frame attributable to LightRay
```

These are project budgets. If representative hardware proves them unrealistic, the plan must be updated before relaxing them.

### R.7 Screen-coverage risk

Transparent and screen-space cost scales with covered pixels, not only ray count.

The controller and renderer must report:

- active count;
- approximate screen coverage;
- mask resolution;
- scatter taps;
- pass execution;
- GPU buffer capacity.

Maximum count alone is not sufficient performance evidence.

---

## S. Diagnostics and benchmark policy

### S.1 Inspector and report

The LightRay controller Inspector must provide:

- one comprehensive diagnostic report;
- one copy-to-clipboard action;
- explicit source, cloud, lifecycle, renderer, camera, and quality state;
- active-ray records with stable IDs and policies;
- current cloud-transition suspension state;
- current render resources and budgets;
- complete error text.

### S.2 Compact debug modes

Initial planned debug modes:

```text
Off
World Zones
Cloud Eligibility
Render Mask
Surface Influence
```

Do not add more modes without evidence that these are insufficient.

### S.3 Benchmark suite

One button must run the complete suite. Do not require separate manual benchmark runs for each case.

Required cases:

- LightRay disabled baseline;
- enabled with zero active rays;
- one ray;
- Medium maximum rays;
- High maximum rays;
- clear-sky procedural rays;
- cloud-respecting rays;
- cloud-ignoring rays under full shade;
- seed-evolution suspension and resume;
- heavy Vegetation, River, and Generated Mass stress view;
- camera cut or remote-camera retarget;
- authored permanent ray;
- timed gameplay-requested ray.

The suite must report actual execution order and restore captured state on every exit path.

### S.4 Render validation tools

Required checks:

- Frame Debugger;
- Render Graph Viewer;
- GPU Profiler;
- game-camera visual capture;
- full-resolution and mask debug screenshots;
- Player-build benchmark;
- representative low-end PC capture.

---

## T. Prospective implementation sequence

Runtime work is authorized only through the active implementation ledger below. The user-provided archive is the authoritative source baseline. Git interaction is not required and is discouraged unless the user explicitly requests it.

### `WEATHER-LIGHT-RAY-V1.0` — contracts and nonvisual foundation

Objective:

- establish source-neutral types and central ownership;
- add the exact cloud-transmission query;
- expose no visible world-space-only prototype as an accepted stage.

Prospective files:

```text
Create:
  Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs
  Assets/Game/Procedural/Weather/WeatherLightRaySourceProfile.cs
  Assets/Game/Procedural/Weather/WeatherLightRayController.cs
  Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs

Modify:
  Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs
  Assets/Docs/Weather_Light_Ray_Architecture.md
  Assets/Docs/Weather_System_Architecture_Provisional.md
```

Required result:

- source kinds, policies, handles, snapshots, and fixed-capacity active storage exist;
- Sun binds through the authoritative current light;
- Moon remains explicitly unavailable;
- cloud transmission can be sampled from CPU without changing the cookie receiver path;
- CPU projection is compared against the shader cookie at controlled points;
- no Renderer Feature, scene component, or visible shaft is added yet.

### `WEATHER-LIGHT-RAY-V1.1` — first visible hybrid vertical slice

Objective:

- deliver the first visible LightRay only as the mandatory hybrid result;
- validate authored stationary Sun rays before procedural population.

Prospective files:

```text
Create:
  Assets/Game/Rendering/Weather/WeatherLightRayRendererFeature.cs
  Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs
  Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl
  Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayMask.shader
  Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayScatter.shader
  Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayComposite.shader
  Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs

Modify through Unity Editor after source compilation:
  Assets/Settings/PC_Renderer.asset

Modify:
  Assets/Game/Procedural/Weather/WeatherLightRayController.cs
  Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
  Assets/Docs/Weather_Light_Ray_Architecture.md
```

Required result:

- one active Sun source group;
- authored stationary rays;
- instanced proxy mask draw;
- quarter-resolution analytic mask;
- depth-aware directional scatter;
- full-resolution surface/shaft composite;
- subtle internal fluctuation;
- visible ground footprint;
- no directly visible proxy-only fallback;
- no procedural spawning yet;
- no gameplay effect yet.

### `WEATHER-LIGHT-RAY-V1.2` — procedural placement and cloud behavior

Objective:

- support clear sky and stable cloud openings;
- support cloud-ignoring procedural/artistic mode;
- suspend cloud-respecting rays during seed evolution.

Prospective modified files:

```text
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs
Assets/Docs/Weather_Light_Ray_Architecture.md
```

Possible additional source file if the controller would otherwise become unmaintainable:

```text
Assets/Game/Procedural/Weather/WeatherLightRayPlacement.cs
```

Creation requires explicit plan update before implementation.

Required result:

- deterministic world-cell candidates;
- count, spacing, radius, lifetime, and distribution controls;
- footprint-wide cloud sampling;
- ordinary-motion forecast;
- evolution fade/suspend/resume at the centralized threshold;
- gradual repopulation;
- `RespectClouds` and `IgnoreClouds` behavior;
- full-overcast divine presentation without cookie mutation.

### `WEATHER-LIGHT-RAY-V1.3` — requests, permanence, and gameplay queries

Objective:

- support authored permanent rays and future gameplay integration without implementing gameplay effects.

Prospective files:

```text
Modify:
  Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs
  Assets/Game/Procedural/Weather/WeatherLightRayController.cs
  Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs
  Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
  Assets/Docs/Weather_Light_Ray_Architecture.md
```

Required result:

- timed, permanent, and externally controlled lifetimes;
- stable request handles and release;
- source-gate override;
- analytic influence queries;
- gameplay channel data;
- permanent cloud-respecting hide/return behavior;
- authored ignore-cloud and divine override workflow.

### `WEATHER-LIGHT-RAY-V1.4` — Moon source integration

Prerequisite:

- authoritative moon direction, colour, intensity, and availability exist in the Time-of-Day system.

Prospective files cannot be frozen until that upstream architecture is reviewed. Expected affected areas:

```text
Time-of-Day source contract
WeatherLightRay source binding
Moon WeatherLightRaySourceProfile asset
LightRay diagnostics and benchmark
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Docs/Weather_System_Architecture_Provisional.md
```

Required result:

- Moon uses the same placement, cloud, lifecycle, renderer, authored, and gameplay contracts;
- Sun and Moon remain mutually exclusive;
- horizon dead zones disable both;
- no second scatter direction runs in the same camera frame.

### `WEATHER-LIGHT-RAY-V1.5` — diagnostics, benchmark, and freeze

Prospective files:

```text
Create:
  Assets/Game/Procedural/Weather/WeatherLightRayBenchmark.cs

Modify:
  Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
  Assets/Game/Procedural/Weather/WeatherLightRayController.cs
  Assets/Game/Rendering/Weather/WeatherLightRayRendererFeature.cs
  Assets/Docs/Weather_Light_Ray_Architecture.md
  Assets/Docs/Weather_System_Architecture_Provisional.md
```

Required result:

- one-button complete suite;
- copied and automatically retained report;
- actual execution-order evidence;
- state restoration;
- Frame Debugger and Render Graph evidence;
- 1440p Player low-end-PC budget decision;
- accepted V1 visual and performance freeze or documented rejection.

---

## U. V1 acceptance criteria

### U.1 Architecture

- One central Weather controller owns all active rays.
- Sun and Moon share one source-neutral architecture.
- Only one celestial source group renders per camera.
- Cloud, source, lifetime, origin, and movement policies are independent.
- Cloud receiver-cookie behavior is unchanged.

### U.2 Visual

- Final shafts do not reveal coarse proxy facets under the gameplay camera.
- Rays read as light arriving from above and reaching the world.
- Surface and ground zones are clearly brighter than ordinary surroundings.
- Internal evolution is subtle and non-mechanical.
- Clear-sky rays work.
- Cloud-respecting rays remain inside stable openings for their accepted lifetime.
- Ignore-cloud rays remain visible through complete cloud shade.
- Seed evolution does not create frantic spawning or abrupt one-frame disappearance.
- Permanent authored rays behave consistently with their policies.

### U.3 Camera

- Ordinary movement does not reshuffle world positions.
- Remote camera cuts do not move existing authored world rays.
- Offscreen margin prevents obvious edge spawning.
- Horizon source states disable procedural rays.
- The designated gameplay camera is the only default render target.

### U.4 Gameplay readiness

- World-space influence matches visual centre, shape, and lifecycle intensity.
- Stable handles and snapshots are available.
- No collider is required.
- No healing or other gameplay rule exists in Weather.

### U.5 Performance

- One proxy draw, one scatter, and one composite are the intended steady render work.
- No recurring managed allocation after warm-up.
- No per-frame physics validation.
- No receiver traversal.
- Low-end Player budgets in section R pass or the architecture is reopened before freeze.

### U.6 Validation

- Unity 6000.5.0f1 compilation passes.
- No shader warning or unsupported Render Graph resource use remains.
- `PC_Renderer` feature attachment is valid.
- Render Graph Viewer confirms expected passes and transient resources.
- Frame Debugger confirms injection after transparents and before post-processing.
- Cloud CPU sampling matches the active cookie projection.
- State restoration passes for benchmark and component disable/re-enable.

---

## V. Risks and mitigation

| Risk | Evidence or cause | Required mitigation |
|---|---|---|
| Proxy facets become visible | Coarse geometry may clip or define the mask if the analytic volume is not conservative. | Expand the proxy, evaluate soft shape analytically, and validate at camera extremes. |
| Low-resolution shimmer | Quarter-resolution mask and moving camera may expose edge instability. | Depth-aware upsample, stable world seeds, conservative softness, and only then consider temporal history. |
| Transparent receivers brighten incorrectly | Camera depth may represent opaque geometry behind River or Vegetation. | Validate heavy River/Vegetation scenes. Receiver-specific changes require a separate audit. |
| Full-resolution composite cost | The destination camera texture may dominate transient bandwidth. | Replace frame camera colour instead of blitting back; inspect Render Graph; evaluate input attachments only after correctness. |
| Screen coverage exceeds count-based budget | Large rays may cover most of the frame. | Report and constrain approximate screen coverage in addition to active count. |
| Cloud sampling drifts from cookie | CPU projection is not currently a public verified contract. | Build a direct CPU-versus-shader projection diagnostic before procedural placement. |
| Large rays rarely fit openings | Footprint-wide forecast shrinks usable clearings. | Permit lower count, smaller radii, shorter cloudy lifetimes, or explicit ignore-cloud policy; do not silently retune frozen clouds. |
| Fast wind reduces valid stationary rays | Future cloud translation may close openings. | Desired count remains a target, not a guarantee. |
| Overcast divine ray looks like a flat overlay | Post-light compensation is stylized rather than material-aware. | Tune centre, colour, atmospheric connection, and bounded compensation; do not claim physical restoration. |
| Permanent forced Sun ray conflicts with Moon | V1 supports one source group. | Avoid the conflict, force one source group, or fake the local event as day. |
| Moon architecture is invented prematurely | No moon owner exists. | Reserve types only; block runtime Moon work until Time of Day defines the source. |
| Renderer Feature API drift | The supplied archive does not contain installed URP package source. | Defer Renderer Feature implementation to V1.1; inspect the live Unity package before editing renderer code. V1.0 contains no Renderer Feature dependency. |

---

## W. Documentation-patch exact scope

`WEATHER-LIGHT-RAY-DOC-V0.1` changes only Markdown.

Create:

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
```

Modify:

```text
Assets/Docs/Weather_System_Architecture_Provisional.md
Assets/Docs/Weather_Cloud_Shadow_Handoff.md
```

No C#, shader, HLSL, scene, prefab, material, profile asset, Renderer Feature, renderer asset, RP asset, layer, tag, Ground, Generated Mass, Vegetation, River, actor, or gameplay file changes are authorized by this patch.

---

## X. Documentation-patch consistency and compliance audit

**Status:** complete at documentation/source-text level. The later user instruction removes Git interaction from the required workflow.

### X.1 Actual changed files

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
Assets/Docs/Weather_System_Architecture_Provisional.md
Assets/Docs/Weather_Cloud_Shadow_Handoff.md
```

### X.2 Intentional documentation changes

- Replace the undefined godrays boundary with the approved Weather LightRay architecture.
- Make hybrid world-space plus screen-space rendering mandatory for V1.
- Rename the subsystem from Godrays to LightRay.
- Record source-neutral Sun/Moon abstraction and mutual exclusivity.
- Record cloud-respecting and cloud-ignoring policies.
- Record transition suspension and `0.80` resume threshold.
- Record timed, permanent, and externally controlled lifetimes.
- Record authored and gameplay-requested divine overrides.
- Record analytic gameplay-zone queries.
- Record the Unity 6 Render Graph pass design, budgets, diagnostics, implementation sequence, risks, and acceptance gates.

### X.3 Preserved behavior

- `WEATHER-CLOUD-SHADOW-V0.4` remains frozen and unchanged.
- Weather wind and wind trails remain unchanged.
- Time of Day remains Sun-only in current runtime source.
- No scene hierarchy, layer, tag, component, material, shader, or renderer asset changes occur.
- No runtime or Editor cost is introduced.

### X.4 Validation performed

- Exact documentation scope check.
- Markdown heading and code-fence balance check.
- UTF-8 and NUL-byte check.
- Stale “undefined godrays” status scan in the two modified parent documents.
- Cross-reference check for the new canonical file.
- Source-file hash comparison confirming all reviewed runtime and renderer assets remain unchanged.

### X.5 Pending evidence

- Installed URP package version and API source.
- Unity asset GUID generation for future files.
- Unity compilation and visual validation.
- Renderer Feature attachment and Render Graph behavior.
- Player and low-end-PC performance.

No Unity validation is required for this documentation-only patch.

---

## Next work items

- Compile `WEATHER-LIGHT-RAY-V1.1A/B` in Unity 6000.5.0f1 and resolve the complete current compiler/shader error set, if any.
- Validate Timed, Permanent, and Externally Controlled lifecycle modes from the authored anchor.
- Validate that the rebuilt effect reads as several subtle warm shafts with visible gaps, not one white ribbon or filled cone.
- Calibrate the authored proof across Ground, Vegetation, River, rocks, the player, camera movement, and Sun-angle changes.
- Measure disabled, zero-ray, and one-ray GPU cost before procedural placement work.
- Define the authoritative Moon contract before Moon runtime integration.


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

## AB. `WEATHER-LIGHT-RAY-V1.1` first visible authored hybrid Sun ray

**Status:** code-driven plan recorded before implementation; source implementation and static audit complete; Unity validation pending.

### AB.1 Objective

Deliver one visible, stationary, authored Sun LightRay through the mandatory hybrid renderer. The patch must prove the central registration path, one analytic world-space zone, one conservative proxy draw, quarter-resolution depth-aware atmosphere processing, and a full-resolution surface/ground composite before procedural population is added.

The V1.1 vertical slice is deliberately limited to **one active authored ray**. This is a renderer-validation limit, not the final storage or population limit. Multiple-ray instancing remains V1.2 work after the first ray's image quality and cost are measured.

### AB.2 Read-only evidence reviewed before implementation

| Evidence | Exact finding | V1.1 constraint |
|---|---|---|
| `Assets/AGENTS.md` — complete current file | Requires supplied-file-only review, canonical plan as the first edit, exact scope, post-change reread, and honest pending Unity validation. | This section is the first V1.1 edit. No Git or external project repository interaction is permitted. |
| User V1.0C screenshot | The 5 × 5 probe is centred over the gameplay area and green/orange classifications visibly agree with cyan/magenta cloud regions. | Freeze the CPU cloud-projection contract. Do not alter cloud generation, projection, cookie assignment, or diagnostic focus in V1.1. |
| `WeatherLightRayController.cs` — `RuntimeSlot`, `TryGetSnapshot`, `CopyActiveSnapshots`, `TickController`, `ResolveSourceStates` | Fixed storage and immutable snapshots exist, but there is no registration surface and `activeRayCount` must remain zero. Sun resolves from override or `RenderSettings.sun`; Moon is explicitly unavailable. | Add authored registration and slot updates without replacing the central controller or inventing Moon state. |
| `WeatherLightRayTypes.cs` — `WeatherLightRayHandle`, `WeatherLightRaySnapshot`, policy enums | The source-neutral handle and policy model already exists. The snapshot has world centre, direction, height, base/top axes, visual/surface/cloud multipliers, lifecycle fields, intensity, cloud transmission, gameplay channel, and seed. | Extend the snapshot only with renderer data that cannot be derived from existing fields: colour multiplier, edge/core shaping, and fluctuation controls. |
| `WeatherLightRaySourceProfile.cs` — `EvaluateAvailability`, `MaximumPresentationLeanDegrees`, `ElevationFadeRange` | Source profiles already define source gate, colour multiplier, maximum lean, and a fade range, although V1.0 only used the hard availability gate. | V1.1 clamps visual direction to profile maximum lean and computes a source-gate fade across the profile elevation range. |
| `WeatherCloudShadowController.cs` — `TrySampleCloudTransmission`, `ShadedTransmission`, `EvolutionInProgress` | Exact readable-cookie transmission is available at controlled world positions; the query is validated against the installed Sun cookie. | `RespectClouds` uses current centre transmission. Forecast, footprint sampling, and transition suspension remain V1.2. `IgnoreClouds` does not alter the cookie. |
| `TimeOfDayController.cs` — `ApplySun`; scene `Main Camera` and Cinemachine setup | Time of Day assigns `RenderSettings.sun`. The gameplay camera is a perspective `MainCamera` driven by Cinemachine and physically offset from the player. | Source binding stays Sun-owned. Rendering targets the resolved gameplay camera, never a player-relative camera transform approximation. |
| `Assets/Settings/PC_RPAsset.asset` | Depth texture, opaque texture, HDR, and light cookies are enabled. | V1.1 may reconstruct world position from camera depth and write an HDR camera-colour destination without modifying the RP asset. |
| `Assets/Settings/PC_Renderer.asset` | SSAO is the only current Renderer Feature. | LightRay is the first project-owned Renderer Feature. It must be attached through Unity after compilation; the source patch must not raw-edit `PC_Renderer.asset`. |
| Project-wide source search | No project-owned `ScriptableRendererFeature`, `ScriptableRenderPass`, or `RecordRenderGraph` implementation exists. | V1.1 establishes the local convention. No unrelated renderer architecture is copied or changed. |
| `RiverWaterDepth.hlsl` — `SampleSceneDepth`, `ComputeWorldSpacePosition`, reversed-Z handling | The project already reconstructs visible world position from URP camera depth using `UNITY_MATRIX_I_VP`. | LightRay shaders reuse the same depth-validity and reconstruction convention. |
| `SH_WeatherCloudShadowDebugOverlay.shader` and `SH_WeatherWindTrails.shader` | Weather shaders target 3.5, use URP Core/RealtimeLights includes, and use explicit transparent/depth states. | New shaders follow the same project naming, target, include, and explicit-state conventions. |

### AB.3 Approved file scope

Create:

```text
Assets/Game/Procedural/Weather/WeatherLightRayAnchor.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayAnchorEditor.cs
Assets/Game/Rendering/Weather/WeatherLightRayRendererFeature.cs
Assets/Game/Rendering/Weather/WeatherLightRayRenderPass.cs
Assets/Game/Rendering/Weather/Includes/WeatherLightRayCommon.hlsl
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayMask.shader
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayScatter.shader
Assets/Game/Rendering/Weather/Shaders/SH_WeatherLightRayComposite.shader
```

Modify:

```text
Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs
Assets/Game/Procedural/Weather/WeatherLightRayController.cs
Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs
Assets/Docs/Weather_Light_Ray_Architecture.md
```

Manual Unity action after source compilation:

```text
Add WeatherLightRayRendererFeature to Assets/Settings/PC_Renderer.asset.
```

`PC_Renderer.asset` is not part of the source archive patch and must not be raw-edited.

No other file is approved.

### AB.4 Runtime ownership and registration contract

`WeatherLightRayAnchor` is an authored data source only. It does not create a renderer, material, mesh, light, collider, child object, or render texture.

Registration order:

```text
Anchor explicit controller override
    -> WeatherLightRayController.PublishedController
    -> unavailable until a controller publishes
```

The controller owns the handle generation and slot. The anchor owns its serialized authored settings and retries registration when publication changes or a stale handle is detected.

V1.1 registration rules:

- only `WeatherLightRayOriginKind.Authored` is created;
- only `WeatherLightRaySourceKind.Sun` is accepted;
- only `WeatherLightRayLifetimePolicy.Permanent` is implemented;
- only `WeatherLightRayMovementPolicy.Static` is implemented;
- the second distinct active anchor is rejected with an explicit V1.1 one-ray-limit error;
- disabling or destroying the anchor releases its slot;
- disabling/replacing the published controller invalidates stale handles and anchors re-register on their next update;
- no managed allocation occurs per frame after the fixed slot array exists.

### AB.5 Authored ray controls

The anchor exposes collapsed Inspector sections for:

```text
Binding
  Controller Override
  Preview in Edit Mode
  Cloud Policy: Respect Clouds / Ignore Clouds

Shape
  Radius (m)
  Top Radius Scale
  Height (m)
  Maximum visual direction comes from the Sun source profile

Appearance
  HDR Colour Multiplier
  Shaft Intensity
  Surface Light Intensity
  Cloud Compensation Intensity
  Edge Softness
  Core Emphasis
  Fluctuation Strength
  Fluctuation Speed

Visibility Response
  Fade-In Duration (s)
  Fade-Out Duration (s)
  Variation Seed
```

The base centre is the anchor Transform position. V1.1 performs no ground raycast and no automatic placement adjustment.

Initial authored calibration defaults are deliberately provisional and exposed for immediate visual tuning:

```text
Cloud Policy                     Ignore Clouds
Ground Radius                    3 m
Top Radius Scale                 0.55
Height                           18 m
HDR Colour Multiplier            white
Atmospheric Shaft Intensity      0.65
Visible-Surface Light Intensity  0.85
Cloud Compensation Intensity     0.90
Volume Edge Softness             0.35
Core / Ground Contact Emphasis   0.30
Internal Fluctuation Strength    0.08
Internal Fluctuation Speed       0.22
Fade In / Fade Out               0.75 s / 0.75 s
Variation Seed                   7319
```

These values are not a frozen art baseline. V1.1 validation must establish the first accepted defaults from the gameplay camera.

### AB.6 Source, cloud, and intensity equations

The presentation direction is the authoritative Sun ray direction clamped toward world down by the source profile's `MaximumPresentationLeanDegrees`.

Source gate weight:

```text
0 when the source is unavailable
otherwise saturate((elevation - minimumElevation) / elevationFadeRange)
```

The fallback profile uses the existing V1.0 minimum elevation and a `0.15` fade range.

For `RespectClouds`, centre transmission is normalized against the configured fully clouded transmission:

```text
cloudOpenWeight = saturate(
    (sampleTransmission - shadedTransmission)
    / max(1 - shadedTransmission, 0.0001))
```

For `IgnoreClouds`:

```text
cloudOpenWeight = 1
```

Target authoritative intensity:

```text
globalEnable * sourceGateWeight * cloudOpenWeight
```

The slot moves toward the target with the authored fade-in or fade-out duration. Visual, surface, and future gameplay consumers read the same smoothed `CurrentIntensity`.

V1.1 does not implement cloud forecast, perimeter sampling, seed-transition suspension, timed expiry, external requests, or gameplay influence queries.

### AB.7 Analytic volume and proxy contract

The V1.1 analytic zone is a finite circular truncated cone:

```text
base centre = anchor Transform position
axis upward = -presentation ray direction
height = authored height
base radius = authored radius
top radius = base radius * top radius scale
```

The existing ellipse fields are populated with equal X/Y axes. True independent ellipse axes remain reserved.

A retained twelve-sided unit cylinder/cone proxy is generated by the render pass and transformed conservatively using the maximum of base and top radius multiplied by `1 / cos(pi / 12)`. This circumscribes the analytic circular footprint rather than clipping it to an inscribed polygon. The proxy:

- draws once;
- uses no depth write;
- is never directly composited as visible geometry;
- only bounds quarter-resolution mask fragments;
- encloses the complete soft analytic volume.

### AB.8 Render camera gate

The controller adds an optional explicit Render Camera Override and otherwise resolves `Camera.main`.

The Renderer Feature runs only when all conditions are true:

- a published controller exists;
- LightRays are enabled;
- one primary renderable snapshot exists;
- camera type is Game;
- camera render type is Base;
- camera equals the controller's resolved render camera;
- source is Sun and has non-zero authoritative intensity.

Scene, Preview, Reflection, overlay-stack, thumbnail, and unrelated secondary cameras are skipped.

### AB.9 Mandatory three-pass Render Graph pipeline

Injection point:

```text
RenderPassEvent.AfterRenderingTransparents
```

This is before post-processing in the current planned URP path so colour grading and bloom process the result.

#### Pass 1 — `Weather LightRay Mask`

Resource:

```text
quarter-linear-resolution RGBA16F
clear = black
MSAA = 1
```

Channels:

```text
R = integrated atmospheric shaft density
G = visible-surface and ground influence
B = ignore-cloud compensation influence
A = core/contact emphasis
```

The pass draws the proxy once, reads camera depth, reconstructs visible world position, integrates eight deterministic samples through the finite analytic volume along the camera ray, evaluates the visible surface point, and writes the four bounded channels.

#### Pass 2 — `Weather LightRay Scatter`

Resource:

```text
quarter-linear-resolution R16F
clear = black
```

The pass reads mask R and camera depth, applies six taps along the projected active Sun-ray direction, and rejects taps across large depth differences. It does not blur G/B/A.

#### Pass 3 — `Weather LightRay Composite`

Resource:

```text
full-resolution camera-format destination
```

The pass reads current camera colour, mask, scatter, and camera depth; performs depth-aware/bilinear upsampling; adds shaft colour, surface lift, cloud compensation, and core emphasis; preserves camera alpha; and replaces Render Graph `cameraColor` with the destination rather than copying back.

V1.1 contains no Compatibility-Mode `Execute` path and no accepted world-space-only fallback.

Fixed vertical-slice implementation constants:

```text
Proxy sides                       12
Mask downsample                   4 × linear dimensions
Mask format                       RGBA16F
Scatter format                    R16F
Atmosphere integration samples    8
Directional scatter taps          6
Composite destination             full-resolution camera format
Temporal history                  none
```

The composite currently weights atmospheric contribution by `0.55` and core/contact contribution by `0.35`; surface and cloud-compensation channels retain unit composite weight. These are shader calibration constants, not user-facing controls, and require gameplay-camera validation.

### AB.10 Shader data contract

Per-camera data is uploaded without a structured buffer because V1.1 supports one ray:

```text
BaseCentreHeight       = xyz base centre, w height
RayDirectionBaseRadius = xyz downward direction, w base radius
TopShape               = x top radius, y edge softness, z core emphasis, w cloud-policy flag
Colour                 = HDR source colour * anchor colour multiplier
Intensity              = x authoritative intensity, y shaft multiplier, z surface multiplier, w cloud-compensation multiplier
Fluctuation            = x strength, y speed, z stable seed phase, w presentation time
ScatterDirection       = projected top-to-base screen direction and texel step
CloudParameters        = shaded transmission and source/cookie availability flags
DebugMode              = final, mask, surface, compensation, or scatter
```

The mask shader may sample the existing main-light cookie only to estimate ignore-cloud compensation. It does not write or alter the cookie.

### AB.11 Inspector and diagnostics

`WeatherLightRayControllerEditor` changes the V1.0 notice to the exact V1.1 state and adds collapsed sections for:

- Hybrid Renderer: global enable, render camera override, debug view;
- Active Authored Ray: handle, anchor, cloud policy, intensity, transmission, and source state;
- existing source, storage, projection, report, and live-status sections.

`WeatherLightRayAnchorEditor` uses the shared `WeatherInspectorGui`; every foldout starts collapsed and every editable control has a concrete tooltip.

Compact V1.1 render debug modes:

```text
Final Composite
Atmospheric Mask
Surface Influence
Cloud Compensation
Scattered Atmosphere
```

No duplicate show/hide buttons are added.

### AB.12 File-by-file implementation sequence

1. Record AB.1–AB.16 before code edits.
2. Extend source/snapshot contracts and add the compact render-debug enum.
3. Implement anchor registration, one-anchor enforcement, source/cloud intensity, fades, primary-renderable snapshot, and render-camera resolution in the central controller.
4. Implement the authored anchor and collapsed custom Inspector.
5. Update the controller Inspector and report for the real V1.1 state.
6. Implement the shared HLSL analytic-volume/depth helpers and the mask, scatter, and composite shaders.
7. Implement the retained proxy mesh, three Render Graph passes, camera gate, material/resource lifetime, and camera-colour replacement.
8. Reread every changed file and affected producer/consumer, compare exact scope, run all available static checks, and record the result in AB.15.

### AB.13 Invariants and non-goals

- Frozen cloud generation, cookie movement, evolution, receiver integration, diagnostics, and benchmark behavior remain unchanged.
- Time of Day remains Sun-only; no Moon light or night behavior is invented.
- No scene, prefab, material, profile asset, renderer asset, RP asset, layer, tag, Ground, River, Vegetation, Generated Mass, actor, or gameplay file is raw-edited.
- No real-time Unity `Light`, per-ray GameObject renderer, collider, compute shader, history texture, temporal reprojection, physics query, or receiver traversal is introduced.
- V1.1 does not implement procedural spawning, camera-footprint population, multiple rays, timed expiry, gameplay effects, movement, or cloud forecasting.
- The diagnostic probe size remains Editor-only and does not define renderer coverage.

### AB.14 Risks

| Risk | Evidence | Mitigation |
|---|---|---|
| Unity 6000.5 Render Graph API mismatch | The supplied archive contains no installed URP package source and no project-owned Renderer Feature example. | Keep API surface minimal, isolate it in two files, compile in Unity immediately, and correct only concrete compiler errors. |
| Quarter-resolution edge shimmer | No temporal history is used. | Conservative softness, bilinear upsample, stable world-space data; temporal work remains blocked unless live evidence requires it. |
| Transparent surfaces use opaque depth | River/Vegetation depth may not represent the visible transparent pixel. | Validate explicitly; receiver changes require a separate cross-subsystem shader plan. |
| Full-screen composite cost | One full-resolution HDR destination is unavoidable in this architecture. | One destination only, no copy-back, measure GPU cost before multi-ray work. |
| One-ray implementation hides future batching defects | V1.1 intentionally avoids structured buffers and instancing. | Treat it only as the visual vertical slice; V1.2 must replace the single-ray upload/draw with fixed-capacity instancing before procedural population. |

### AB.15 Validation and compliance plan

Static/source checks available here:

- exact changed-file scope;
- C# delimiter, string, comment, and preprocessor balance;
- HLSL include/guard/function and brace balance;
- shader pass/name/include consistency;
- no `Execute` compatibility path;
- no scene/asset/settings changes;
- no per-frame collection creation in controller, anchor, feature, or pass;
- final complete-file reread and reference scan.

Required Unity checks:

1. Compile C# and all three shaders with zero errors or warnings.
2. Add `WeatherLightRayRendererFeature` to `PC_Renderer` and confirm exactly three LightRay passes in Render Graph/Frame Debugger for the designated Base Game camera only.
3. Add one anchor, use `IgnoreClouds`, and confirm a soft shaft plus bright ground/visible-surface zone with no visible proxy facets.
4. Switch to `RespectClouds` and confirm the ray follows the current validated cloud transmission without cookie changes.
5. Cycle all compact render debug modes and capture the final gameplay-camera result plus mask/scatter views.
6. Measure 2560 × 1440 incremental GPU/CPU cost and recurring allocation for disabled, zero-ray, and one-ray cases.

### AB.16 Acceptance criteria

- The exact approved source files compile in Unity 6000.5.0f1.
- One authored permanent Sun ray registers with a stable handle and appears in controller diagnostics.
- A second authored anchor is explicitly rejected in V1.1.
- The designated Base Game camera alone executes the feature.
- The proxy is not directly visible.
- Final output includes a soft depth-integrated shaft and brighter visible surfaces/ground.
- `IgnoreClouds` remains visible under cloud shade without modifying the cookie.
- `RespectClouds` intensity follows current cloud transmission.
- Internal fluctuation is subtle and deterministic.
- No recurring managed allocation is attributed to the feature after warm-up.
- Renderer asset attachment is performed through Unity and is not included as a raw text edit.
- Unity visual and performance evidence remains pending until the user runs the patch; source-only checks are not represented as runtime acceptance.

### AB.17 Current implementation status

- [x] User approved V1.1 and the code-driven plan was persisted before renderer/shader implementation.
- [x] Source-neutral contracts extended with source gate weight, renderer shaping, fluctuation, colour, and compact debug mode.
- [x] One-anchor registration, stable handles, explicit second-anchor rejection, permanent/static snapshot generation, source/cloud intensity, fades, and render-camera resolution implemented.
- [x] `WeatherLightRayAnchor` and its collapsed custom Inspector implemented.
- [x] Controller Inspector and report updated for the real V1.1 state.
- [x] Shared analytic-volume HLSL, mask, scatter, and composite shaders implemented.
- [x] Twelve-sided retained proxy, quarter-resolution mask/scatter, full-resolution composite, Base Game camera gate, and Render Graph camera-colour replacement implemented.
- [x] Exact changed-file scope and available source/static checks pass.
- [ ] Unity C# and shader compilation complete.
- [ ] `WeatherLightRayRendererFeature` attached to `PC_Renderer` through Unity.
- [ ] Final image, debug views, cloud policies, allocations, and 2560 × 1440 cost validated.

### AB.18 Post-implementation consistency and compliance audit

- Exact comparison against the accepted V1.0C plus Weather Inspector cleanup baseline contains only the twelve files approved in AB.3.
- `WeatherCloudShadowController.cs`, cloud cookie generation, cloud debug overlay, Time of Day, Weather wind, Wind Trails, River, Vegetation, Ground, Generated Mass, scenes, prefabs, materials, `PC_Renderer.asset`, and `PC_RPAsset.asset` remain byte-identical.
- The V1.0C CPU cloud query, projection formula, probe focus, high-contrast markers, report samples, and cloud cookie offset comparison remain unchanged except for V1.1 report wording.
- The controller allocates no per-frame collection. The fixed slot array is retained; registration scans the fixed capacity and the authored anchor reuses one handle.
- The Renderer Feature performs no per-ray GameObject, Unity `Light`, collider, compute dispatch, physics query, receiver traversal, or temporal-history allocation.
- The retained proxy mesh and three materials are created with the Renderer Feature and destroyed through `Dispose`; no mesh or material is created per frame.
- The Render Graph path contains exactly the named Mask, Scatter, and Composite passes; no Compatibility-Mode `Execute` implementation exists.
- The shader/property contract is exact across C# and HLSL. All project-owned include paths resolve inside the supplied tree; all three hidden shader names match the Renderer Feature lookup strings.
- A numerical translation of the finite-frustum intersection equation was checked for a central camera ray, a miss, a camera-inside case, and a vertical ray. The accepted cases produced ordered positive exit intervals; the miss produced no interval.
- UTF-8, NUL-byte, final-newline, C#/HLSL/shader delimiter, comment, string, preprocessor, Markdown-fence, serialized-property-reference, and no-stale-nonvisual-wording checks pass.
- **Unverified:** Unity 6000.5 / URP 17.5 Render Graph API compilation. The supplied archive contains no installed package source or Unity executable. Concrete compiler output is required before the source patch may be frozen.
- **Unverified:** visual quality, depth integration over transparent River/Vegetation content, player/prop surface lift, cloud compensation, lack of proxy facets, recurring allocation, and GPU cost. These require the six AB.15 Unity checks.



## AC. `WEATHER-LIGHT-RAY-V1.1A/B` shared descriptor and structured-sunshaft recovery

**Status:** code-driven plan was persisted before implementation. Runtime descriptor/lifecycle, authored Inspector, Render Graph data packing, structured mask, strand-preserving scatter, and rebalanced composite source changes are complete. Supplied-file static compliance audit passed. Unity compilation, visual proof, tuning, and performance validation remain pending.

### AC.1 User-approved visual target

The accepted target is a **subtle warm bundle of sunlight shafts**, using the supplied Honkai: Star Rail screenshots and additional real-time/pixel-art references only for visual principles. No source image or asset is copied.

Required visual properties:

- one LightRay zone contains several distinguishable shafts;
- empty or near-empty gaps remain visible between shafts;
- shaft widths, offsets, lengths, intensities, and phases vary;
- the broad containing envelope is almost invisible and exists only as faint atmospheric continuity;
- the visible shafts are warm cream/gold, never neutral-white by default;
- edges are soft but internal structure survives filtering;
- independent shafts brighten, dim, breathe, and drift extremely slowly;
- the whole bundle remains stationary by default;
- ground and visible surfaces receive a stronger but still soft, irregular light contribution;
- the environment remains clearly visible through the atmospheric effect;
- foreground depth interrupts and silhouettes the shaft bundle.

Intensity hierarchy:

```text
ground and directly illuminated visible surfaces
    strongest and most readable

individual sunlight shafts
    visible, directional, translucent

broad atmospheric envelope
    barely visible
```

### AC.2 Rejection criteria

Any of the following fails the visual proof:

- one broad white cone, cylinder, prism, ribbon, or blurred blob;
- uniformly filled opacity across the containing volume;
- searchlight, moon-laser, spotlight, or weapon-beam presentation;
- pure-white default colour;
- broad screen washout;
- a perfectly circular bright decal disconnected from shaft structure;
- synchronized whole-bundle pulsing;
- rapid flicker or obvious repetitive oscillation;
- directional scatter that merges all strand gaps;
- requiring high atmospheric opacity merely to make the ground brighter.

### AC.3 Read-only evidence reviewed before implementation

| Evidence | Exact finding | Recovery constraint |
|---|---|---|
| `Assets/AGENTS.md` — complete file | Requires supplied-file review, canonical-plan-first editing, exact scope, and post-change reread. | This AC section is the first modification. No Git interaction is used. |
| `WeatherLightRayTypes.cs` — `WeatherLightRaySnapshot` | The snapshot already carries source, origin, cloud, lifetime, source-gate, and movement enums, but it stores only broad-volume appearance values. | Introduce one immutable shared per-ray descriptor that authored, procedural, and gameplay creation can all populate. |
| `WeatherLightRayAnchor.cs` — complete file | The anchor exposes cloud policy, one circular taper, broad shaft/surface strengths, and fade durations. It exposes no source, lifetime, source-gate, strand, scatter, footprint, or detailed evolution controls. | Expand the anchor to the full per-instance proof control surface. |
| `WeatherLightRayController.cs` — `UpdateAuthoredSlot` | The snapshot constructor hardcodes `Sun`, `Permanent`, `RequireActiveSource`, and `Static`; `HoldOrExpiryTime` is positive infinity. | Source, lifetime, and source-gate policy must come from the shared descriptor. Static remains the only implemented movement mode and is reported explicitly. |
| `WeatherLightRayController.cs` — `RuntimeSlot` | Runtime state stores spawn time, last update time, and one smoothed intensity, but no authored lifecycle revision. | Add restartable timed lifecycle state without reallocating slots. |
| `SH_WeatherLightRayMask.shader` — lines 93–120 in the accepted V1.1 source | Eight view-ray samples integrate `WeatherLightRayEvaluateVolume`, which is the complete filled tapered volume, into one atmospheric density. | Replace filled density with a sparse deterministic strand field plus a separately weak envelope. |
| `WeatherLightRayCommon.hlsl` — `WeatherLightRayEvaluateVolume` and `WeatherLightRayFluctuation` | The common include provides only radial volume occupancy and one combined sinusoidal fluctuation. | Add ray-local coordinates, deterministic hashes, strand placement, per-strand length/width/intensity, and independent evolution. |
| `SH_WeatherLightRayScatter.shader` — complete file | Six taps average the red atmospheric channel along the projected source direction. | Scatter only the strand channel, shorten the span, and blend toward the filtered result so gaps remain. |
| `SH_WeatherLightRayComposite.shader` — final contribution | `max(mask.r, scatter)` multiplied by broad shaft strength dominates the output; surface and ground are secondary. | Composite separate strand, envelope, surface, and cloud-compensation channels using the required intensity hierarchy. |
| `WeatherLightRayRenderPass.cs` — `BuildShaderParameters` | One ray is uploaded through scalar/vector globals; the pass already owns quarter-resolution RGBA16F mask, R16F scatter, and full-resolution composite resources. | Keep the accepted three-pass hybrid plumbing and one-ray limit; expand the data packing only. |
| `WeatherLightRayRendererFeature.cs` — complete file | The feature runs only on the designated Base Game camera and invokes the three-pass Render Graph path. | Preserve the camera gate, source registration, pass ordering, and no-world-space-only fallback. |
| `Assets/Settings/PC_RPAsset.asset` | Depth texture, opaque texture, HDR, and light cookies are enabled. | Continue depth reconstruction and HDR composite without changing the RP asset. |
| User V1.1 screenshot | The implemented effect is one large translucent soft-white region; the River black area is a separate River debug view. | Treat only the white LightRay region as the failed baseline; do not diagnose or alter River. |

### AC.4 Approved file scope

Modified source:

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

Reviewed but byte-identical:

```text
Assets/Game/Procedural/Weather/WeatherLightRaySourceProfile.cs
Assets/Game/Procedural/Weather/WeatherCloudShadowController.cs
Assets/Game/Rendering/Weather/WeatherLightRayRendererFeature.cs
Assets/Settings/PC_Renderer.asset
Assets/Settings/PC_RPAsset.asset
```

No scene, prefab, material, layer, tag, River file, cloud-generation file, Time-of-Day file, or render asset may change.

### AC.5 Shared per-ray descriptor

`WeatherLightRayDescriptor` becomes the immutable visual/behavior contract used by the authored anchor now and by future procedural/gameplay creation later.

Descriptor groups:

```text
Identity and policy
    SourceKind
    OriginKind
    CloudPolicy
    LifetimePolicy
    SourceGatePolicy
    MovementPolicy

Shape
    Height
    BaseEllipseAxes
    TopEllipseAxes
    VisualEnvelopeRadiusScale
    VisualEnvelopeEdgeSoftness
    MaximumVisualLeanDegrees

Strand bundle
    StrandCount
    StrandWidthRange
    StrandSpread
    StrandPositionVariation
    StrandIntensityVariation
    StrandLengthVariation
    StrandTaper
    StrandEdgeSoftness
    StrandClusterBias

Atmosphere and colour
    ColourMultiplier
    WarmthContribution
    StrandIntensity
    EnvelopeHazeIntensity
    ScatterLength
    ScatterSoftness
    HeightFade
    CameraIntersectionFade

Surface and ground
    GroundLightMultiplier
    VisibleSurfaceLightMultiplier
    CloudCompensationMultiplier
    FootprintEdgeSoftness
    FootprintIrregularity
    CoreEmphasis

Subtle evolution
    IntensityFluctuationStrength
    IntensityFluctuationSpeed
    WidthBreathingStrength
    LateralDriftStrength
    PatternEvolutionSpeed
    PerStrandPhaseVariation

Lifecycle and integration
    FadeInDuration
    HoldDuration
    FadeOutDuration
    GameplayChannel
    VariationSeed
```

`WeatherLightRaySnapshot` stores the descriptor plus runtime state: handle, base centre, resolved presentation direction, lifecycle state, spawn time, current intensity, and current cloud transmission.

### AC.6 Authored anchor control surface

The authored anchor exposes the same per-ray values future procedural creation will supply.

#### Binding and policy

- Controller Override
- Update in Edit Mode
- Light Source: Sun / Moon-ready
- Cloud Policy: Respect Clouds / Ignore Clouds
- Source Gate: Require Active Source / Ignore Source Gate
- Movement: read-only Static in this recovery patch

#### Lifecycle

- Lifetime Policy: Timed / Permanent / Externally Controlled
- Fade-In Duration
- Hold Duration, shown for Timed
- Fade-Out Duration
- External Visibility, shown for Externally Controlled
- Restart Timed Lifecycle action

Timed lifecycle:

```text
FadingIn -> Holding -> FadingOut -> Inactive
```

Permanent lifecycle remains active until source/cloud policy suspends it or the component is disabled.

Externally Controlled lifecycle fades toward the anchor's External Visibility state. Public methods may set that state and restart timed lifecycle without recreating the slot.

#### Shape

- Ground Radius
- Height
- Top Radius Scale
- Visual Envelope Radius Scale
- Visual Envelope Edge Softness
- Maximum Visual Lean

#### Internal ray structure

- Strand Count, clamped to 1–8 for the V1 one-ray shader
- Strand Width Range, normalized against local bundle radius
- Strand Spread / Separation
- Strand Position Variation
- Strand Intensity Variation
- Strand Length Variation
- Strand Taper
- Strand Edge Softness
- Strand Cluster Bias

#### Atmospheric appearance

- HDR Colour Multiplier
- Warmth Contribution, blending the resolved source colour toward a warm cream/gold Sun presentation
- Strand Intensity
- Envelope Haze Intensity
- Scatter Length
- Scatter Softness
- Height Fade
- Camera Intersection Fade

#### Surface illumination

- Ground Light Intensity
- Visible Object Light Intensity
- Cloud Compensation Intensity
- Footprint Edge Softness
- Footprint Irregularity
- Core / Ground Contact Emphasis

#### Subtle evolution

- Intensity Fluctuation Strength
- Intensity Fluctuation Speed
- Width Breathing Strength
- Lateral Drift Strength
- Pattern Evolution Speed
- Per-Strand Phase Variation
- Variation Seed

All foldouts remain collapsed by default and every visible control requires a specific tooltip.

### AC.7 Lifecycle evaluation

The runtime slot stores one smoothed **gate weight** rather than treating the current intensity as the lifetime itself.

Gate target:

```text
source gate weight
× cloud-open weight
× external-visibility request when applicable
```

Source gate weight:

```text
Require Active Source -> source availability weight
Ignore Source Gate    -> 1
```

Cloud-open weight remains the accepted normalized transmission equation for `Respect Clouds`; `Ignore Clouds` remains 1 and may use the existing compensation channel.

Timed lifecycle weight for elapsed time `t`:

```text
0 <= t < fadeIn
    t / fadeIn

fadeIn <= t < fadeIn + hold
    1

fadeIn + hold <= t < fadeIn + hold + fadeOut
    1 - (t - fadeIn - hold) / fadeOut

t >= total
    0
```

Final authoritative intensity:

```text
smoothed gate weight × lifecycle weight
```

On the first update of a Timed ray, the gate weight is initialized directly to the current source/cloud target because the timed lifecycle already owns the authored fade-in. This prevents accidentally squaring the initial fade curve. Permanent and Externally Controlled rays still use the gate fade for their initial appearance.

A lifecycle revision from the anchor resets spawn time and restarts the timed sequence without reallocating or changing the stable handle.

### AC.8 Structured shaft field

The analytical volume remains the finite tapered world-space bound. It is no longer filled with visible density.

For every ray-march sample:

1. transform the world position into LightRay-local axial and two-dimensional radial coordinates;
2. evaluate a faint radial envelope;
3. evaluate up to eight deterministic strand centre lines;
4. assign each strand stable width, offset, intensity, length, taper, and phase from the per-ray seed and strand index;
5. apply small independent time-varying width, intensity, and lateral offset changes;
6. combine strands with a maximum-dominant limited sum so adjacent strands do not fill their gaps;
7. apply top/bottom atmospheric height fading and camera-intersection attenuation.

Deterministic strand layout rules:

- the strand index provides an angular base distribution;
- `Strand Position Variation` perturbs that distribution without fully random overlap;
- `Strand Spread` controls radial separation inside the bundle;
- `Strand Cluster Bias` pulls strands toward the centre or distributes them farther outward;
- strand 0 remains the stable central primary shaft;
- peripheral strands use a deterministic angular distribution plus a cluster-dependent minimum radial separation, preventing multiple default strands from collapsing into one central blob;
- non-primary strands may shorten according to `Strand Length Variation`;
- widths are local-radius fractions, so authored radius changes preserve relative detail.

### AC.9 Mask channel contract

The quarter-resolution `RGBA16F` mask becomes:

```text
R = direct structured strand atmosphere
G = faint broad envelope haze
B = ground/object-classified illumination influence after separate authored strengths
A = divine cloud-compensation influence after the same ground/object classification
```

The surface channel is evaluated separately from atmospheric opacity. It combines a soft irregular footprint with stronger local contribution beneath individual strands. Depth-derived geometric orientation blends between independent Ground Light and Visible Object Light strengths before the B/A channels are written; approximately horizontal surfaces use the ground control, while steeper visible geometry uses the object control.

### AC.10 Strand-preserving scatter

The R16F pass reads only mask R.

- filtering remains aligned with projected celestial-light direction;
- span is controlled by per-ray Scatter Length;
- Scatter Softness blends between the original strand mask and the filtered result;
- depth rejection remains active;
- no lateral blur is introduced;
- the broad envelope never seeds the scatter pass.

This pass may soften and extend individual rays along their direction, but it may not merge lateral gaps into one filled region.

### AC.11 Composite contract

The full-resolution composite applies four independent contributions:

```text
structured strand atmosphere × Strand Intensity
faint envelope haze          × Envelope Haze Intensity
ground/object influence      preweighted by independent Ground/Object strengths
cloud compensation           preweighted by Cloud Compensation Intensity
```

The strand and envelope channels use the resolved source colour, blend it toward warm cream/gold by `Warmth Contribution`, then apply the authored HDR colour multiplier. This lets existing white V1.1 anchors adopt the new warm baseline without silently overwriting their serialized colour. The surface contribution may be stronger than the atmospheric contribution. Existing scene colour is always preserved and the contribution remains additive/HDR-safe.

### AC.12 Debug views

Compact renderer debug modes become:

```text
Final Composite
Strand Atmosphere
Surface Influence
Cloud Compensation
Scattered Strands
Envelope Haze
```

No duplicate show/hide buttons are added.

### AC.13 Provisional defaults for new anchors

These are implementation-start defaults, not a frozen art baseline:

```text
Colour Multiplier:           white multiplier over the resolved source colour
Warmth Contribution:        0.35 toward warm cream/gold
Visual Envelope Softness:    0.65
Strand Count:                5
Strand Width Range:          0.07–0.16 of local radius
Strand Spread:               0.72
Strand Intensity:            0.28
Envelope Haze Intensity:     0.025
Ground Light Intensity:      0.42
Visible Object Light:         0.28
Cloud Compensation:          0.45
Intensity Fluctuation:       0.06 at slow speed
Width Breathing:             0.035
Lateral Drift:               0.025
Pattern Evolution:           slow
```

Existing serialized V1.1 anchor values are preserved. New shader semantics prevent the old broad-volume intensity from recreating a filled white ribbon, but visual calibration remains required.

### AC.14 File-by-file implementation sequence

1. `WeatherLightRayTypes.cs`: add the shared descriptor and update snapshot forwarding/state.
2. `WeatherLightRayAnchor.cs`: add the complete per-ray controls, descriptor construction, external visibility, and timed restart revision.
3. `WeatherLightRayController.cs`: consume the descriptor, resolve source kind/gate, evaluate lifecycle, and publish the new snapshot.
4. `WeatherLightRayAnchorEditor.cs`: expose the complete collapsed control scheme and contextual lifecycle actions.
5. `WeatherLightRayControllerEditor.cs`: update V1.1 recovery wording, debug modes, and report explanations.
6. `WeatherLightRayCommon.hlsl`: add local basis, deterministic hashes, strand field, envelope, evolution, and surface-footprint helpers.
7. `SH_WeatherLightRayMask.shader`: emit the new RGBA mask channels.
8. `SH_WeatherLightRayScatter.shader`: preserve strand gaps while applying controlled directional softness.
9. `WeatherLightRayRenderPass.cs`: upload the expanded descriptor vectors and scatter controls.
10. `SH_WeatherLightRayComposite.shader`: apply the required intensity hierarchy and updated debug views.
11. Reread every changed file and affected producer/consumer; compare the final diff with this section; run all available static checks; record Unity compilation and visual checks as pending.

### AC.15 Invariants and non-goals

- The one-active-authored-ray renderer limit remains for this recovery patch.
- The mandatory hybrid three-pass path remains; no prism-only fallback is accepted.
- Cloud generation, movement, cookie projection, V1.0C CPU sampling, and debug-overlay focus remain unchanged.
- No procedural population, multi-ray batching, gameplay healing, Moon binding, cloud prediction, or moving footprint is implemented.
- No River behavior or River debug state is changed.
- No Renderer asset is raw-edited.
- No layer, tag, collider, child renderer, local light, or per-ray material is created.

### AC.16 Acceptance criteria

#### Behavior

- Timed, Permanent, and Externally Controlled modes are selectable and observable on one authored anchor.
- Timed lifecycle can restart without changing the handle.
- Respect Clouds, Ignore Clouds, Require Active Source, and Ignore Source Gate are not hardcoded.
- The snapshot descriptor matches the anchor values.

#### Visual

- at least three internal shafts are visibly distinguishable at practical settings;
- gaps remain between those shafts;
- no broad white ribbon, cone, or laser dominates;
- default colour reads as gentle warm sunlight;
- the envelope is weaker than the strand bundle;
- surface/ground illumination is readable without requiring opaque atmospheric haze;
- independent changes are slow, subtle, and unsynchronized;
- foreground depth interrupts the shaft field;
- the result remains stable under camera movement.

#### Technical

- all three Render Graph passes execute without warnings;
- no per-frame managed allocation is added by the controller or anchor;
- `PC_Renderer.asset`, `PC_RPAsset.asset`, scenes, prefabs, materials, cloud runtime, and River files remain byte-identical;
- Unity compilation and gameplay-camera validation pass before the recovery is accepted.

### AC.17 Validation plan

1. Compile with zero C#, shader, and Render Graph errors.
2. Validate Timed, Permanent, and Externally Controlled behavior on the existing authored anchor; confirm timed restart preserves its handle.
3. Use the six renderer debug views to confirm separate strand, envelope, surface, compensation, and scatter channels.
4. Validate the final look against the AC.1 target across Ground, Vegetation, River, rocks, player, camera motion, and multiple Sun angles.
5. Compare Respect Clouds and Ignore Clouds; confirm cloud projection and cookie behavior remain unchanged.
6. Measure disabled, zero-ray, and one-ray GPU cost at 2560 × 1440 and inspect Frame Debugger for exactly the intended three passes.

### AC.18 Current implementation status

- [x] User approved the revised visual and behavioral direction.
- [x] Complete supplied-file read-only audit performed.
- [x] Code-driven findings and exact approved scope persisted before implementation.
- [x] Shared descriptor implemented.
- [x] Authored lifecycle and full per-ray control surface implemented.
- [x] Structured multi-strand mask implemented.
- [x] Strand-preserving scatter implemented.
- [x] Rebalanced composite implemented.
- [x] Post-change consistency and compliance audit complete.
- [ ] Unity compilation complete.
- [ ] Visual and performance proof accepted.

### AC.19 Post-implementation source audit

The implementation was reread against the accepted V1.1 supplied-file baseline after all edits.

Exact changed-file set:

```text
Assets/Docs/Weather_Light_Ray_Architecture.md
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
```

Implemented source facts:

- `WeatherLightRayDescriptor` contains 46 constructor inputs covering source/policy, shape including envelope softness, eight-strand structure, colour/warmth, atmosphere, independent ground/object illumination, evolution, lifecycle, gameplay channel, and seed.
- `WeatherLightRayAnchor.BuildDescriptor()` supplies the same 46 inputs; its Inspector exposes 46 serialized controls under collapsed sections with explicit tooltips.
- the controller no longer hardcodes Sun, Permanent, or Require Active Source for authored rays; it consumes the descriptor, preserves the stable handle, and supports restartable Timed, Permanent, and Externally Controlled behavior;
- a Timed ray retains its slot after expiry so the authored proof can be restarted without reallocating or changing its handle; its first gate update is initialized to the active policy target so the timed fade-in is not multiplied by a duplicate gate fade;
- the mask pass performs eight analytical view-ray samples and emits structured strands, faint envelope, orientation-classified ground/object illumination, and cloud compensation into the documented RGBA16F channels;
- the strand field uses one central shaft plus deterministic separated peripheral shafts, independent width/intensity/length variation, slow phase-separated intensity change, width breathing, and small axial lateral drift;
- the R16F scatter pass uses seven source-aligned, depth-aware taps and reads only the strand channel;
- the composite preserves the existing scene colour and applies strand, envelope, surface, and cloud-compensation strengths independently;
- Sun colour is blended toward the restrained warm target `(1.00, 0.76, 0.46)` by authored `Warmth Contribution`, then multiplied by the authored HDR colour; Moon does not receive this Sun-specific blend;
- the existing three-pass Render Graph architecture, one-authored-ray proof limit, Base Game camera gate, fixed controller storage, and validated V1.0C cloud query remain intact.

Static checks passed:

- exact 11-file scope;
- balanced C#/HLSL/Shader delimiters, comments, strings, and preprocessor blocks;
- descriptor constructor and anchor invocation both contain 46 ordered arguments;
- every serialized property requested by `WeatherLightRayAnchorEditor` exists on the runtime component;
- every C# shader property ID resolves to the matching HLSL global;
- shader names and include paths remain compatible with the existing Renderer Feature;
- Markdown fences, UTF-8, NUL-byte safety, and final newlines passed;
- `WeatherCloudShadowController.cs`, `WeatherLightRaySourceProfile.cs`, `WeatherLightRayRendererFeature.cs`, `PC_Renderer.asset`, `PC_RPAsset.asset`, scenes, prefabs, materials, River files, layers, and tags remain byte-identical.

Unavailable validation:

- Unity 6000.5.0f1 C# and shader compilation;
- URP Render Graph execution and warning state;
- gameplay-camera visual comparison with the user references;
- lifecycle behavior in Play Mode;
- recurring allocation and 2560 × 1440 GPU cost.

These are explicitly pending and must not be treated as passed from the source audit.

### AC.20 Exact Unity validation breadcrumbs

1. `Hierarchy → LightRay_Test → Weather Light Ray Anchor → Lifecycle` — test `Timed`, `Permanent`, and `Externally Controlled`; use `Actions → Restart Timed Lifecycle` for Timed.
2. `Hierarchy → LightRay_Test → Weather Light Ray Anchor → Internal Ray Structure` — confirm practical settings show multiple separated shafts; start from the documented defaults.
3. `Hierarchy → LightRay_Test → Weather Light Ray Anchor → Atmospheric Appearance` — tune `Sun Warmth Contribution`, `Strand Intensity`, and `Envelope Haze Intensity`; the envelope must remain much weaker than strands.
4. `Hierarchy → LightRay_Test → Weather Light Ray Anchor → Surface Illumination` — tune `Ground Light Intensity` and `Visible Object Light Intensity` independently from atmospheric opacity.
5. `Hierarchy → Weather → Weather LightRay Controller → Hybrid Renderer → Render Debug View` — inspect all six channels, then return to `Final Composite`.
6. `Window → Analysis → Frame Debugger` and `Window → Analysis → Profiler` — confirm the intended Mask, Scatter, and Composite passes and measure disabled, zero-ray, and one-ray states at 2560 × 1440.

