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
Current implemented Weather feature: shared wind domain and vegetation response field
```

The current game does not require a general-purpose three-dimensional atmospheric simulation. The first Weather implementation is deliberately built around the XZ gameplay plane used by the top-down isometric game.

---

## 2. Weather ownership

Weather owns authoritative environmental wind.

Vegetation does not own wind. Vegetation consumes Weather wind and applies vegetation-specific response such as stiffness, bend weighting, and small blade-detail motion.

The same ownership rule is intended to apply to future consumers:

- stylized drawn wind lines;
- gameplay movement effects in severe wind;

A consumer may apply its own response model, but it must not invent a separate authoritative wind state when the shared Weather wind is applicable.

---

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

Other consumers may require different response behavior. For example, a stylized wind line should follow the authoritative flow, while grass applies elastic bend behavior.

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

## 6. Stylized wind-line compatibility

A Weather-owned stylized wind-trail V0 architecture is now approved and recorded in `Assets/Docs/Weather_Wind_Architecture.md` under `WEATHER-WIND-TRAILS-V0.0`; runtime implementation and Unity validation remain pending.

The trails consume the shared authoritative target wind rather than an unrelated animation direction or the vegetation spring-response field. Candidate selection favours strong, separated locations, and centreline construction advances through repeated explicit-time XZ target-wind samples so trail direction agrees with Weather gameplay sampling and vegetation's target source.

V0 uses the Weather field-anchor Y plus a configurable altitude range, one fixed-capacity combined camera-facing ribbon mesh, and one hidden runtime material created from a serialized Shader reference. It does not depend on `GeneratedGround`, create a material asset, add a renderer feature, or create a second wind simulation.

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
- cloud systems;
- temperature;
- seasons;
- day/night ownership;
- fog ownership;
- lightning;
- weather-transition scheduling;
- biome-specific Weather rules;
- save/load representation;
- complete Weather authoring tools.

They may eventually belong to Weather, another subsystem, or a shared environment layer. No ownership decision has been made here.

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

`WeatherWindDomain` is attached directly to the `Weather` object in this initial version. Future Weather features may become child modules only after a dedicated Weather architecture review; WEATHER-V0A does not create speculative precipitation, cloud, temperature, event, or regional modules.

The one-time vegetation-owned compatibility publisher and migration utility were removed by `VEG-V2-INFRA.3` after scene-owned Weather was accepted. New and retained scenes use the exact `WeatherWindDomain` directly; there is no production migration or fallback ownership path.

### WEATHER-V0A accepted state

**WEATHER-V0A is frozen on 2026-07-21.** The user-validated scene contains one active and published `WeatherWindDomain` under `Systems/Weather`. The accepted report recorded a READY 128² field, 0.5 m cells, 64 × 64 m world coverage, 10 Hz updates, active compute simulation, available CPU sampling, and available future wind-line consumption. No vegetation-owned Weather provider remains in production source.

`WEATHER-WIND-V0A.1` changes only the compute helper used to wrap logical cells into the toroidal physical texture. Because the runtime producer guarantees a positive power-of-two resolution, unsigned bit masking replaces signed integer modulus without changing the Weather ownership model, field layout, simulation, or consumer contracts.


### WEATHER-V0A final cleanup

`VEG-V2-INFRA.3A` was superseded before application. `VEG-V2-INFRA.3` directly removes the obsolete compatibility subclass and migration utility while leaving `WeatherWindDomain` and all runtime Weather behavior unchanged. If a live scene still contains the obsolete test object, it is deleted manually in Unity rather than supported by additional transitional code.
