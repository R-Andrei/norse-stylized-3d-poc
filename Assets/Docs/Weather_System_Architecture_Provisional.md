# Weather System Architecture — Provisional

## Status

**Provisional design record.**

This document preserves Weather-system decisions made during vegetation and wind development. It is not a complete Weather specification and must be reviewed, expanded, and potentially restructured when dedicated Weather-system implementation begins.

The detailed, currently implemented wind-subsystem architecture remains documented in:

- `Assets/Docs/Weather_Wind_Architecture.md`

Where this provisional parent document and the implemented wind document differ about current wind behavior, `Weather_Wind_Architecture.md` is authoritative.

---

## 1. Current project context

```text
Engine: Unity 6000.5.0f1
Render pipeline: URP
Gameplay camera: constrained top-down isometric
Primary wind spatial domain: world-space XZ
Current implemented Weather features: shared wind domain, vegetation response field, stylized wind trails, and a frozen V0 native directional-cookie cloud-shadow system with universal receiver integration, diagnostics, low-frequency seed evolution, and automated benchmarking
Active Weather feature: LightRay `WEATHER-LIGHT-RAY-V1.2E` separates visual presets, normalized-cycle selection, explicit runtime dependencies, and reusable population policies while retaining the accepted hybrid atmospheric-ribbon and real-Spot renderer; curated profile assets and scene migration remain deferred
Active Weather authoring patch: `WEATHER-INSPECTOR-CLEANUP-V1.0` reorganizes the four Weather custom Inspectors under collapsed sections without changing runtime behavior or serialized values
Current pending Weather validation: standalone Player, low-end-PC, release-stripping, and broad lifecycle confirmation during the future project-wide Weather testing sprint; none blocks the accepted cloud-shadow V0.4 freeze
```

The current game does not require a general-purpose three-dimensional atmospheric simulation. The first Weather implementation is deliberately built around the XZ gameplay plane used by the top-down isometric game.

---

## 2. Weather ownership

Weather owns authoritative environmental wind.

Vegetation does not own wind. Vegetation consumes Weather wind and applies vegetation-specific response such as stiffness, bend weighting, and small blade-detail motion.

The same ownership rule applies to current and future consumers:

- implemented stylized wind trails;
- future gameplay movement effects in severe wind;

A consumer may apply its own response model, but it must not invent a separate authoritative wind state when the shared Weather wind is applicable.


### 2.1 Cloud-shadow ownership and universal receiver contract — V0.4 frozen

Weather owns the authoritative moving cloud-shadow field, including its world-space phase, deterministic pattern controls, coverage, transition softness, retained sunlight, movement speed, wind-direction response, and sun-availability gate. Receiver systems consume that field; they do not author independent cloud masks or movement.

The cloud-shadow requirement is universal across gameplay-world receivers. Ground, Generated Mass, Vegetation, River, actors, buildings, future houses, terrain-like materials, props, snow, ice, and other sun-responsive world surfaces must show one coherent cloud boundary at the same world position. UI, sky, diagnostics, and intentionally unlit or emissive effects may be explicitly exempted.

The canonical design, candidate comparison, receiver-audit requirement, exact continuation procedure, and acceptance gates are defined in:

- `Assets/Docs/Weather_Cloud_Shadow_Handoff.md`

The earlier Ground-only and unresolved-candidate scopes are superseded. The frozen V0 architecture is one URP main directional-light cookie assigned to the authoritative sun and consumed through every supported receiver's cookie-aware main-light path. `WeatherCloudShadowController` and its dirty-triggered cookie generator implement that contract. The controller is attached to the existing Weather object, and the user-supplied post-fix audit reports both controller and receiver gates at `PASS`, with 64/64 loaded-scene records and every mandatory authored receiver supported. The user also validated the projected debug field, global tiled coverage, debug-focus workflow, low-frequency cookie evolution, and the complete V0.3E2 benchmark suite. The hybrid shared-mask receiver system is deferred and may be reopened only if later Player-build or low-end-hardware evidence proves the native cookie cost materially unacceptable. Receivers must never apply both paths.

Cloud transmission modifies environmental sunlight only unless a separately approved global overcast response also changes ambient/sky lighting. Weather cloud shading must not mutate receiver geometry, collision, hydrology, River simulation, Vegetation interaction, Generated Mass generation, actor gameplay, or material ownership.

### 2.2 LightRay ownership and hybrid V1 architecture — continuous-beam replacement documented

Weather owns the LightRay subsystem: persistent world-space source-dependent or source-independent ray zones, procedural placement, authored and gameplay-requested registration, lifecycle, normalized-cycle preset selection, explicit dependency resolution, cloud eligibility, renderer-facing data, and gameplay influence queries. Time of Day may provide the normalized `0..1` activation cycle and the current Controller directional source, but visual presets never infer source ownership or active period. The cloud system remains authoritative for cloud transmission, measured global cloud cover, movement, and seed evolution. Gameplay systems consume LightRay influence but own healing, damage, buffs, quests, and all other effects.

The canonical architecture and current implementation order are defined in:

- `Assets/Docs/Weather_Light_Ray_Architecture.md`

The V1.0C source-neutral controller, Sun binding, CPU cloud query, and projection-focus validation remain accepted. The sampled-volume renderer and subsequent failed sparse/per-beam-impact/contact-cell presentations remain rejected by Unity evidence.

The current AF4 authored proof uses the mandatory hybrid continuous-beam architecture:

- one authoritative LightRay zone owns `2–12` complete parallel atmospheric ribbons derived from one Area Diameter;
- the ribbons use one combined mesh, a full-resolution mask, and restrained screen-space edge softening;
- one lazy pooled shadowless realtime URP Spot Light per active zone is the primary surface response, allowing Vegetation, Ground, masses, actors, props, and other additional-light-aware receivers to execute their normal material lighting;
- the prior full-resolution screen-space circular lift remains an optional complement and defaults to zero;
- one shared edge-softness value derives the real Spot inner/outer cone and optional complement boundary;
- no per-beam Unity Lights, broad visible envelope, sampled ellipse field, capsule cluster, or ribbon-only final tier is approved.

Normal procedural Sun and Moon populations remain mutually exclusive. Sun rays belong to the day window, Moon rays belong to the future night window, and both are disabled through horizon transition dead zones. The project still has no authoritative Moon source, so Moon runtime work remains blocked.

Cloud-respecting rays use the existing cloud field. Cloud-ignoring rays may imply divine light through complete cloud cover without changing the directional cookie. Timed, permanent, and externally controlled lifetimes remain approved.

`WEATHER-LIGHT-RAY-V1.1D-AF4` is source-implemented but not accepted or frozen. The real Spot Light, optional complement migration, material response, four-additional-light interaction, and 1440p cost still require Unity proof. Procedural population remains blocked until the authored proof passes.

### 2.3 Weather Inspector organization — `WEATHER-INSPECTOR-CLEANUP-V1.0`

The Weather GameObject retains four separate runtime components: Wind Domain, Wind Trail Renderer, Cloud Shadow Controller, and LightRay Controller. Their custom Inspectors use one shared editor-only presentation helper and component-owned foldout state. Every ordinary section starts collapsed, every visible editable control has an explicit label and tooltip, and derived state is displayed read-only. No foldout state is serialized.

The cleanup is presentation-only. Runtime fields, serialized field names, authored values, Weather calculations, shaders, scenes, materials, render assets, hierarchy, layers, tags, benchmark behavior, and Scene-view diagnostic geometry remain unchanged. The canonical implementation and validation ledger is:

- `Assets/Docs/Weather_Inspector_Cleanup_Plan.md`

Cloud debug visualization is controlled only by the serialized `Debug View` dropdown. The duplicate Show Cloud Areas, Show Cloud / Opening Map, Hide Cloud Debug Overlay, and Refresh Debug Focus buttons are removed. The conditional runtime-focus-clear action remains.

The LightRay Inspector organization remains governed by the cleanup patch. The current authored-anchor and renderer controls exist, but visual authority comes from `Weather_Light_Ray_Architecture.md`; the rejected V1.1C renderer controls must be replaced or redefined only through the approved continuous-beam source sequence.

---


## 2.3 LightRay preset selection and population-policy boundary — V1.2E

`WEATHER-LIGHT-RAY-V1.2E` separates four authorities:

- `WeatherLightRayPreset`: appearance only;
- `WeatherLightRaySelectionProfile`: normalized `0..1` eligibility, priority, transition stability, and explicit direction/source/cloud dependencies;
- `WeatherLightRayPopulationProfile`: reusable instance counts, spacing, cloud-data requirement, spatial cloud policy, and cloud-cover activation;
- `WeatherLightRayController`: scene bindings, cycle provider, focus/ground controls, shared storage, global automatic-ray budget, and execution.

Selection curves are normalized and are not interpreted as hours or named dayparts. A source-dependent entry is eligible only through its explicit source-availability policy. Vertical and fixed-world entries use the independent source contract and do not require a Sun or Moon light. A visual preset's legacy `SourceKind` metadata is not production authority.

Population rules independently choose whether cloud data is ignored, optional, or required and whether placement is unrestricted, clear-footprint-qualified, or a distinct cloud opening. `Optional + Clear Footprint` is the ordinary sunlight contract: cloudless or disabled cloud state behaves as clear sky, while an enabled cloud field restricts candidates to clear footprints. An enabled but invalid/unready cloud producer is never silently treated as clear.

Several rules may coexist under one selected entry and share one Controller automatic-ray budget. Authored and gameplay-created rays are never evicted. Compatible visual-preset changes preserve population handles; incompatible dependency contexts retire old automatic rays before replacement qualification.

The cloud producer remains frozen. V1.2E adds only dirty-time measured cloud cover from already generated CPU cookie pixels and consumes the existing current/future transmission queries. No GPU readback, duplicate cloud simulation, per-frame cookie scan, or receiver-path change is introduced.

## 3. Authoritative wind and visual response are separate

The architecture separates two concepts.

### 3.1 Authoritative Weather wind

The authoritative wind describes local XZ direction and strength in Weather units.

It must be queryable by gameplay without GPU readback. The current wind subsystem exposes CPU sampling through:

```text
WeatherWindDomain.SampleWindXZ(worldPosition)
WeatherWindDomain.TrySampleWindXZ(worldPosition, out wind)
```

This CPU-queryable contract is the future basis for deterministic gameplay effects such as reducing player movement speed when moving against sufficiently strong wind.

### 3.2 Consumer-specific visual response

The current vegetation system uses a separate dynamic response field that stores bend and bend velocity. This allows grass to exhibit inertia, overshoot, and settling without changing the authoritative wind value.

Other consumers may require different response behavior. The implemented stylized wind trails use authoritative Weather strength and direction for placement, a direction-locked birth backbone, and consumer-owned visual wobble, while grass applies elastic bend behavior.

Authoritative wind strength must not be expressed as grass-displacement metres.

---

## 4. Spatial model and scalability

The Weather wind domain is independent of individual grass-patch size.

A small grass patch and a large grass patch sample the same world-space wind at their own positions. The visual field uses a fixed metres-per-cell density and follows the active gameplay region rather than stretching one fixed texture over each vegetation area.

The current implementation uses one gameplay-anchor-centred XZ field. The preferred anchor is the player or the camera follow target. If no anchor is assigned, the isometric camera is projected onto a configured horizontal ground plane.

The current single moving domain is sufficient for the active visible gameplay region. Possible future requirements such as multiple widely separated players, multiple simultaneous active regions, streamed tiles, or clipmaps remain undefined and must be designed only if the game actually requires them.

---

## 5. Current wind composition

The implemented baseline combines:

- a prevailing XZ direction and base strength;
- broad multidirectional variation;
- irregular moving gust regions;
- a CPU-authoritative target-wind function;
- a matching GPU target field for visual consumers;
- a dynamic spring-response field for vegetation.

The rejected vegetation-only traveling ribbon and analytical recovery-wave system is not part of the active architecture.

The current wind generation is a baseline, not the final Weather authoring model.

---

## 6. Stylized wind trails — implemented and provisionally frozen

The Weather-owned stylized wind-trail system is implemented and provisionally frozen at `WEATHER-WIND-TRAILS-V0.9A`. The canonical implementation details, controls, migration history, diagnostics, and deferred validation scope are recorded in `Assets/Docs/Weather_Wind_Architecture.md`.

The frozen consumer:

- uses the shared authoritative Weather target-wind contract rather than vegetation spring response or an unrelated animation direction;
- captures one dominant visible Weather direction at trail birth and prevents local samples from cumulatively steering the average backbone;
- uses local target-wind samples for strength and direction-compatibility validation;
- prefers the upwind portion of the target camera, requires useful visible runway, and releases capacity after the complete body exits downwind;
- applies a smooth dense presentation curve with mandatory zero-mean lateral wobble and an optional localized larger loop;
- uses a resolved `Spawn -> Alive -> Despawn` lifecycle with pointed physical endpoints;
- uses the Weather field-anchor Y plus configurable altitude and has no `GeneratedGround` dependency;
- uses one fixed-capacity combined ribbon mesh, one hidden runtime material from a serialized shader, and one target-camera render submission;
- has an accepted baseline maximum of five active trails.

The user confirmed successful Unity compilation after V0.9A and reported that the current behavior is working substantially as expected. Comprehensive soak, profiling, Frame Debugger, cross-quality, and complete Weather-system integration testing are deliberately deferred to a later Weather testing sprint. The feature is frozen for now; additional changes should be driven by concrete faults found during that sprint.

---

## 7. Gameplay compatibility

A future region or Weather event may contain wind strong enough to affect gameplay.

The discussed example is slowing the player when moving against strong wind. A compatible implementation would query authoritative wind at the player position and compare movement direction against the wind vector.

The following remain undefined:

- thresholds for gameplay-strength wind;
- exact player slowdown formula;
- whether wind can push the player directly;
- networking or deterministic replay requirements;
- how authored level regions override or combine with global Weather.

These must be specified when gameplay wind work begins.

---

## 8. Future local wind influences

The architecture should remain compatible with bounded local influences when required. Discussed examples include:

- directional high-wind regions;
- gust events;
- vortices or swirls;
- wind corridors;
- shelter or attenuation regions;
- scripted severe-wind effects.

These are compatibility targets, not implemented systems. Their data model, blending priority, authoring workflow, and runtime limits remain provisional.

---

## 9. Current consumer contract

### Weather system

Owns:

- authoritative XZ wind direction and strength;
- world-space sampling;
- active visual field resources;
- wind-domain diagnostics.

### Vegetation system

Owns:

- grass stiffness and response amplitude;
- root-to-tip bend weighting;
- small local blade-detail motion;
- composition with future interaction and persistent-trail systems.

### Future wind-line system

Expected to own:

- line spawning and pooling;
- stylized geometry and rendering;
- visual lifetime and fading;
- advection through Weather wind samples.

### Future gameplay systems

Expected to consume CPU-authoritative wind and apply explicit gameplay rules. They must not depend on the vegetation response texture.


### Cloud-shadow receiver audit — live V0.1 complete; V0.2 source hardened

`Assets/Game/Procedural/Weather/Editor/WeatherCloudShadowReceiverAudit.cs` owns the editor-triggered compliance scan. The user ran V0.1 in `VisualFrameworkDemo` and supplied the complete report: both URP assets supported cookies, the authoritative directional sun was valid, and 64 active renderer/material records across five shaders were inventoried. V0.2 adds active-controller and assigned-cookie checks, mandatory authored-receiver checks even when a shader is absent from the loaded scene, package-owned URP Lit recognition, and direct authored-pragma verification so a fallback keyword-space entry cannot falsely prove a custom ForwardLit variant. The audit remains read-only, on-demand, and clipboard-copyable; it must not scan renderers or materials every frame.

### Cloud-shadow runtime and receivers — V0.4 frozen

`WeatherCloudShadowController` owns one generated directional cookie on the authoritative sun. `WeatherCloudShadowCookieGenerator` builds a deterministic seamless linear `R8` texture only when pattern settings are dirty. The texture uses repeat wrapping, so one configured world-space period tiles across all world positions; `Cookie World Size Metres` is a repeat period, not a finite coverage extent. Steady movement consumes bounded-cadence Weather wind direction, integrates one world-space phase, and updates the URP directional-cookie offset without traversing renderers or rebuilding the texture. The controller preserves and restores the previous sun cookie, size, and offset and disables cloud shade when the sun has no useful contribution. Map dimensions, player position, camera position, and loaded chunk count do not require additional cloud textures or simulation state.

Receivers consume the authoritative main-light cookie exactly once. Ground and Generated Mass Pixel Surface continue through `UniversalFragmentPBR`; River continues through its existing world-position-aware main-light call; Vegetation now uses the world-position-aware main-light overload while preserving ambient, local lights, edge accents, and its no-geometric-main-shadow policy. Standard URP Lit and compatible Lit Shader Graphs use their native package path. No receiver owns subsystem-local cloud state, and no simulation, geometry, material identity, fog, emission, or local-light contract is redefined by this integration.

### Cloud-shadow cookie evolution — V0.3D complete

`WeatherCloudShadowController` may periodically evolve the globally tiled cookie without changing the receiver contract. Automatic evolution runs only in Play Mode. The default schedule selects a deterministic next seed after a randomized `90–180 s` interval, generates that next pattern once, and crossfades the existing single readable `R8` cookie over `10 s` at `6 Hz`. Manual `Evolve Cloud Cookie Now` and `Complete Evolution Immediately` actions remain available for edit-preview and validation.

The controller owns three reusable CPU byte buffers—current, next, and blended—and one GPU cookie texture. `WeatherCloudShadowCookieGenerator` reuses a retained workspace for lattices, field values, opening cleanup, and queue storage, so repeated automatic seed changes do not allocate new managed arrays after the configured resolution/cell capacities have warmed. During an active blend, the controller applies a smoothstep interpolation to the byte buffers and calls `SetPixelData`/`Apply` only at the bounded evolution cadence. The world movement phase and cookie offset continue uninterrupted; receivers still perform exactly one native directional-cookie sample.

Idle evolution cost is one timer/state comparison. Preparation is one dirty `O(R²)` next-seed generation. Transition cost is one `O(R²)` byte blend and one `R²` upload per configured update. At the approved default 256²/6 Hz/10 s settings, the transition performs 60 uploads and moves approximately 3,932,160 raw texel bytes before engine/driver overhead. No extra draw, render pass, compute dispatch, material mutation, renderer traversal, per-chunk state, or receiver-shader change is introduced.

Any authored pattern edit cancels the pending transition, restores/rebuilds the authored seed safely, and reschedules. A failed next-seed generation leaves the current cookie active and reports the complete evolution error. Disabling automatic evolution prevents future schedules but does not create a half-frozen receiver path; manual actions remain explicit diagnostics.

### Cloud-shadow automated benchmark — V0.3E2 complete and retained

The Weather cloud Inspector owns one benchmark entry point backed by a transient runtime runner. The runner creates no persistent scene object and changes no scene content. It captures the active controller state, forces the debug overlay and automatic evolution off during controlled persistent windows, runs adjacent cloud-cookie-disabled/static-cookie and cloud-cookie-disabled/moving-cookie comparisons in alternating order, runs one complete forced evolution transition, runs a post-evolution moving-cookie control, and restores the captured controller state on every exit path.

The source-implemented suite uses `FrameTimingManager` for whole-frame CPU total, active main-thread, active render-thread, and GPU data where supported. Optional `ProfilerRecorder` counters capture managed allocations and rendering counters where available. Measurement arrays are allocated before timed windows. `WeatherCloudShadowCookieGenerator` exposes profiler markers around dirty generation and texture upload, while the controller records preparation and blend/upload CPU timings. The suite reports unavailable counters explicitly and does not interpret missing data as zero.

The benchmark measures the complete native directional-cookie path by controlled A/B comparison. It does not claim direct isolation of one shader sample. It does not modify receiver shaders, cookie representation, quality settings, VSync, target frame rate, time scale, camera, Vegetation density, River state, rocks, or other gameplay content. Reports are retained in memory, written automatically to `Library/WeatherCloudShadowBenchmarkDiagnostics` in the Editor or `Application.persistentDataPath` in a Player, and copied through one Inspector action. The runner has no idle presence: it is created with `HideAndDontSave` only when the suite starts and destroys itself after completion or cancellation.

The final corrected V0.3E2 stress-view run completed at 2560 × 1440, Direct3D12, `PC_RPAsset`, with explicit actual execution order and restoration `PASS`. Mean paired GPU median deltas were `+0.016 ms` for the static cookie and `+0.011 ms` for the normal moving cookie. CPU deltas reversed sign across cases and remain inconclusive within Editor noise. SetPass calls did not materially change. Evolution preparation measured `14.586 ms` once; the ten-second transition performed 60 low-cadence blend/uploads totalling `73.323 ms`, with a `1.466 ms` maximum update and no post-transition GPU residue (`0.631 ms` evolution median versus `0.633 ms` post-evolution median). This evidence accepts the native cookie architecture for V0.4. Standalone Player and low-end-PC confirmation move to the future testing sprint.

### Cloud-shadow world coverage and debug focus — V0.3B/V0.3C

The production directional cookie is a globally tiled world-space lighting mask. It is not a finite player-centred or camera-centred simulation region, and it does not require recentering, regeneration after travel, per-chunk storage, or whole-map allocation. Remote cutscenes receive the same cloud field automatically because every visible receiver samples the authoritative directional light at its own world position.

`WeatherCloudShadowController` exposes a generic debug-focus contract only for the finite diagnostic overlay and future view-local tooling. Resolution order is runtime override, Inspector override, assigned fallback camera, `Camera.main`, then controller transform. Public set/clear methods may redirect the diagnostic overlay without moving the player. The overlay follows the resolved focus by default and may match one complete cookie repeat period.

V0.3C removes the misleading coverage terminology from serialized field labels, public API, Inspector actions, and reports. Existing serialized references are preserved through `FormerlySerializedAs`. The custom Inspector displays the live resolved debug focus and source even when the serialized fields remain `None`, identifies automatic `Camera.main` resolution explicitly, and warns when a higher-priority override masks the fallback camera. These controls do not affect production cloud coverage. Source implementation is complete and user-validated; these controls remain diagnostic only.

### Cloud-shadow debug visualization — V0.3A

Before cloud placement or generation behavior is revised, `WeatherCloudShadowController` provides a dedicated diagnostic overlay. The overlay is submitted programmatically without a scene object and samples the exact active URP directional cookie on a configurable horizontal world plane. `CloudAreas` marks only clouded regions; `CloudAndOpenings` shows the complete field classification. The generated cookie is also visible as a compact Inspector preview.

This visualization is observational only. It does not create visible-cloud geometry, change the cookie generator, alter coverage or movement, traverse receivers, modify materials, or participate in lighting. It casts and receives no shadows and is exempt from the universal receiver contract as an intentionally unlit diagnostic overlay. V0.3B gives it a generic debug focus resolved from a runtime override, Inspector override, assigned camera, `Camera.main`, or controller fallback. V0.3C makes that diagnostic-only ownership explicit in the Inspector and public API. The finite overlay follows that focus for gameplay, cutscene, spectator, and editor inspection, while the production cookie remains globally tiled and independent of any focus.

### Cloud-shadow accepted freeze — V0.4

The accepted cloud-shadow baseline is:

- one Weather-owned native URP main directional-light cookie;
- globally tiled world coverage independent of player, camera, chunk count, and map size;
- one cookie-aware main-light sample per supported receiver;
- `256²` linear `R8` cookie with a `128 m` world-space repeat period;
- Weather-wind-driven movement;
- broad clouded regions with authored clearances normally around `5–7 m` or larger and approximately `1.5 m` transition softness;
- randomized seed evolution every `90–180 s` using a `10 s`, `6 Hz` smooth crossfade into the same GPU cookie;
- retained receiver audit, debug overlay, copied diagnostics, and automated benchmark suite;
- no finite coverage window, per-chunk cloud state, visible cloud geometry, fullscreen cloud-shadow pass, or hybrid receiver field.

No additional cloud-shadow implementation is currently planned. The system may reopen only for a concrete defect or materially adverse Player/low-end benchmark evidence.

### LightRay V1 architecture — AF4 hybrid real-light authored proof

The former undefined godrays boundary is superseded by `Assets/Docs/Weather_Light_Ray_Architecture.md`. The subsystem is named LightRay and supports shared Sun/Moon architecture, procedural Weather rays, permanent authored rays, and timed or externally controlled gameplay requests.

Current authoritative boundaries:

- `TimeOfDayController` remains authoritative for the current Sun and any future approved Moon source;
- Weather owns LightRay placement, lifecycle, cloud policy, renderer data, authored/runtime registration, analytical gameplay influence, and the pooled per-zone surface-light proxy;
- cloud-respecting rays consume the existing cloud field and suspend during seed evolution;
- cloud-ignoring rays may remain active through complete cloud cover without modifying the directional cookie;
- Sun and Moon procedural groups remain mutually exclusive and excluded near horizon transitions;
- one LightRay zone renders several separate complete parallel beams, not one segmented line;
- atmospheric presentation uses one combined mesh of complete world-X dense-overlap ribbons plus bounded screen-space finishing;
- primary receiver illumination uses at most one pooled shadowless realtime URP Spot Light per active zone, never one Light per beam;
- the prior full-resolution screen-space circular lift is retained only as an optional complement and defaults to zero;
- one shared surface softness derives both the Spot inner/outer cone and optional complement boundary;
- the sampled frustum/tube/ellipse renderer, broad envelope, per-beam impact lights, and screen-space-only primary surface response are rejected;
- the frozen cloud receiver-cookie path and Ground, Generated Mass, Vegetation, River, actor, and gameplay ownership remain unchanged;
- AF4 source is implemented but Unity material-response proof and profiling remain pending.

Continuation order:

```text
AF4 compile and real-Spot material-response proof
AF4 additional-light interaction and 1440p profiling
Authored-proof freeze
Procedural population only after freeze
```

---

## 10. Diagnostics policy

Wind debugging is centralized in the Weather wind domain rather than duplicated in each consumer.

The current visualization is deliberately limited to three modes:

- `Off`;
- `Wind Field`;
- `Response Error`.

`Wind Field` shows the authoritative Weather target vector used by CPU gameplay sampling and future wind-trail construction.

`Response Error` shows `actual visual bend - expected equilibrium bend`. The expected bend uses the same target-wind-to-visual-bend conversion as the compute simulation. No arrow means the vegetation spring response has caught up; arrows expose lag, overshoot, and local settling. Response data uses the existing editor-only asynchronous GPU readback.

---

## 11. Performance principles

The established priorities are:

```text
active gameplay runtime cost
> dirty or fixed-cadence update cost
> memory usage
>> storage size
```

Current principles:

- no per-grass CPU wind updates;
- no gameplay dependence on GPU readback;
- fixed-cadence low-resolution field updates are acceptable when bounded; the implemented Weather domain exposes `5–60 Hz`, with `10 Hz` as the baseline default and accepted demo-scene value;
- visual consumers may use GPU field samples;
- gameplay uses CPU-authoritative wind queries;
- field density is measured in metres per cell, not tied to vegetation-patch dimensions;
- new Weather consumers require explicit runtime and memory budgets.

---

## 12. Explicitly undefined Weather-system areas

The following have not been designed and must not be inferred from this document:

- precipitation architecture;
- visible-cloud geometry, sky-cloud rendering, and volumetric clouds;
- temperature;
- seasons;
- day/night ownership;
- fog ownership;
- lightning;
- weather-transition scheduling;
- biome-specific Weather rules;
- save/load representation;
- complete Weather authoring tools.

The moving ground/world cloud-shadow illumination contract is now defined by `Assets/Docs/Weather_Cloud_Shadow_Handoff.md`; only visible-cloud rendering and the other items above remain ownership-undefined. They may eventually belong to Weather, another subsystem, or a shared environment layer.

---

## 13. Next dedicated Weather-design work

When full Weather-system development begins, this document should be revised through a complete architecture audit covering:

- exact Weather feature scope;
- authoritative runtime state;
- authoring and regional overrides;
- event scheduling and transitions;
- consumer contracts;
- persistence;
- diagnostics;
- performance budgets;
- interaction with level generation and scene lifecycle.

Until then, this document preserves the accepted direction without pretending that unspecified Weather features have already been designed.


---

## 14. Initial scene ownership — WEATHER-V0A

The first concrete Weather hierarchy is intentionally minimal:

```text
Scene
└── Systems
    └── Weather                         [WeatherWindDomain]
```

`WeatherWindDomain` is attached directly to the `Weather` object in this initial version. Future Weather features may become child modules only after a dedicated Weather architecture review. The cloud-shadow controller completed its architecture review, source implementation, Unity attachment, receiver audit, visual validation, seed-evolution validation, and V0.3E2 benchmark. It remains attached directly to the existing Weather object; no child module or duplicate Weather owner is introduced. WEATHER-V0A wind ownership remains unchanged.

The one-time vegetation-owned compatibility publisher and migration utility were removed by `VEG-V2-INFRA.3` after scene-owned Weather was accepted. New and retained scenes use the exact `WeatherWindDomain` directly; there is no production migration or fallback ownership path.

### WEATHER-V0A accepted state

**WEATHER-V0A is frozen on 2026-07-21.** The user-validated scene contains one active and published `WeatherWindDomain` under `Systems/Weather`. The accepted report recorded a READY 128² field, 0.5 m cells, 64 × 64 m world coverage, 10 Hz updates, active compute simulation, available CPU sampling, and available future wind-line consumption. No vegetation-owned Weather provider remains in production source.

`WEATHER-WIND-V0A.1` changes only the compute helper used to wrap logical cells into the toroidal physical texture. Because the runtime producer guarantees a positive power-of-two resolution, unsigned bit masking replaces signed integer modulus without changing the Weather ownership model, field layout, simulation, or consumer contracts.


### WEATHER-V0A final cleanup

`VEG-V2-INFRA.3A` was superseded before application. `VEG-V2-INFRA.3` directly removes the obsolete compatibility subclass and migration utility while leaving `WeatherWindDomain` and all runtime Weather behavior unchanged. If a live scene still contains the obsolete test object, it is deleted manually in Unity rather than supported by additional transitional code.

## 2.4 Automatic LightRay population — `WEATHER-LIGHT-RAY-V1.2D`

The active LightRay implementation now includes a source-prepared deterministic automatic Sun population layer owned by the existing `WeatherLightRayController`. The canonical contract remains `Assets/Docs/Weather_Light_Ray_Architecture.md`.

The population layer:

- evaluates stable hashed world-space cells around the resolved Base Game camera ground footprint or an explicit focus override;
- acquires ground through an explicit user-assigned physics mask;
- samples the complete preset-sized footprint against the exact CPU-readable cloud cookie at present and bounded future times;
- uses bounded qualification, invalid grace, cooldown, and graceful procedural-handle retirement;
- keeps individual V1.2D rays static rather than making ground lights chase cloud openings;
- suspends automatic spawning and retires automatic rays during cloud seed evolution below the established resume threshold;
- protects authored and caller-created procedural rays from eviction;
- adds no cloud generator, GPU readback, scene component, shader path, material path, layer, tag, or serialized scene edit.

Automatic population is disabled by default and its Ground Mask defaults to Nothing. Project-side Unity configuration, compilation, visual validation, allocation measurement, and 0/1/3/6-ray CPU/GPU profiling remain required before production density is accepted.
