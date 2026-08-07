# Weather LightRay Architecture

## Status

**Architecture identifier:** `WEATHER-LIGHT-RAY-CLEANUP-V1.3A`

**Current state:** the generic multi-ray renderer, lifecycle, beam evolution, surface-light response, vegetation-accent publication, cloud-aware request path, and cloud-opening atmospheric population exist. The V1.3A cleanup removes disconnected selection infrastructure and obsolete non-shader diagnostics, corrects cloud-transition population policy, and establishes the current Inspector and ownership contracts.

Unity 6000.5.0f1 compilation and runtime validation of V1.3A remain required in the live project.

## 1. Core principle

LightRays are a generic visual and gameplay-capable system.

The core system must support, without architectural pretense that every ray is sunlight:

- weather-driven atmospheric beams;
- authored environmental beams;
- quest and objective highlighting;
- nighttime scenes;
- scripted storytelling;
- ritual, magical, artificial, or otherwise nonphysical presentation;
- gameplay-requested zones with externally controlled visibility and lifetime.

The LightRay system does not own daylight, the Sun, the Moon, Weather eligibility, quest eligibility, time-of-day policy, or automatic preset selection.

A runtime request may use a directional source that happens to be the current Sun. That is an input to that request or producer, not ownership of the LightRay architecture.

## 2. Ownership boundaries

### 2.1 LightRay Controller

The Controller owns:

- fixed-capacity runtime storage;
- authored and procedural registration;
- ray lifecycle and fades;
- appearance-preset application;
- beam-layout evolution;
- renderer-facing zone and beam data;
- pooled surface Spot Lights;
- vegetation-accent publication;
- source-policy and cloud-policy evaluation for individual requests;
- the current cloud-opening atmospheric population integration;
- compact runtime telemetry and one comprehensive copied report.

The Controller does not decide which game or Weather feature is allowed to request a ray.

### 2.2 Appearance preset

`WeatherLightRayPreset` owns shared visual presentation:

- colour and warmth;
- atmospheric intensity and softening;
- camera-intersection fade;
- beam spacing, width, variation, edge softness, and fade shape;
- contact opacity;
- surface response;
- vegetation accent intensity, coverage, and softness;
- beam-evolution preset, strength, and speed;
- default spawn geometry.

A preset does not own:

- source eligibility;
- Sun, Moon, or time-of-day dependency;
- cloud dependency;
- automatic-population eligibility;
- quest or gameplay eligibility;
- lifecycle policy.

The serialized preset `SourceKind` is legacy compatibility data only. Production request and population code must not read it. Its removal belongs to the later serialized preset-migration patch.

### 2.3 Runtime request

A runtime request may carry request-specific policy such as:

- source kind or explicit direction;
- source availability gate;
- cloud response;
- movement policy;
- lifecycle policy and durations;
- placement and geometry overrides;
- external visibility.

These values describe one request. They do not redefine the global system.

### 2.4 Weather orchestration

A future Weather orchestration layer will decide at runtime:

- whether an atmospheric LightRay population is appropriate;
- which preset is eligible;
- which environmental dependencies apply;
- which directional source is supplied;
- whether cloud openness, time of day, storm state, or another condition matters;
- when populations start, transition, or stop.

V1.3A deliberately does not implement this orchestrator. It removes disconnected speculative selector infrastructure so the future orchestration contract can be designed intentionally.

### 2.5 Cloud-shadow producer

The cloud-shadow subsystem owns:

- deterministic directional-cookie generation;
- cookie movement;
- seed evolution and blended-cookie publication;
- present and future CPU transmission queries;
- evolution stability status;
- receiver integration, debug overlay, receiver audit, and benchmark.

LightRays consume those query APIs. They do not modify cloud generation or receiver shading.

## 3. Current atmospheric automatic producer

The current automatic producer is one cloud-opening atmospheric LightRay implementation. It is not the definition of all automatic LightRays.

It currently receives:

- the active appearance preset;
- a runtime-resolved directional source used for cloud projection and ray direction;
- the cloud transmission provider;
- a gameplay render camera;
- a Ground Mask;
- population authoring settings.

The current Controller wiring may supply the resolved daylight directional source. The population runtime treats that value as a supplied dependency; it does not inspect preset source-family metadata and does not establish a Sun-owned core contract.

Future Weather orchestration may replace, reconfigure, or supplement this producer.

## 4. Automatic-population spatial contract

### 4.1 Camera footprint

Automatic placement uses one complete eight-sample camera footprint projected onto a horizontal ground-reference plane.

- The runtime obtains one representative hit from the configured Ground Mask to establish ground height.
- The camera viewport centre is intersected mathematically with that horizontal plane to resolve the footprint centre.
- Eight perimeter rays are intersected with the same plane to resolve the polygon.
- Camera Margin expands each perimeter point outward in XZ.
- Candidate eligibility uses the polygon, not a radius.
- Each actual spawn candidate is still raycast downward against the configured Ground Mask before it may spawn.

A viewport edge does not need to land on a Ground collider. Automatic population suspends only when no usable ground reference can be found or the camera rays cannot form a valid forward plane intersection. There is no circular fallback.

### 4.2 Focus override

A Population Focus Override translates the already resolved camera footprint in XZ.

It does not:

- create a circular search region;
- change the footprint shape;
- remove the render-camera requirement;
- use the override Transform height as terrain height.

### 4.3 Derived work limits

The following are derived rather than serialized authoring controls:

```text
candidate checks per population update = clamp(Ray Budget × 2, 4, 64)
ground raycast distance = max(100 m, resolved render-camera far clip)
```

### 4.4 Qualification

A candidate that passes its first complete six-sample cloud evaluation may spawn during that same population update. No hidden second evaluation or qualification timer remains.

The six samples are:

- current centre;
- four current surrounding samples;
- one predicted future centre sample based on the assigned lifetime.

World-cell identity, spacing, budget, cooldown, ground slope, and storage constraints still apply.

## 5. Cloud transition policy

Cloud evolution publishes a blended cookie and marks transmission samples `EvolutionUnstable` while the transition is in progress.

Below `Cloud Transition Spawn Resume`:

- existing automatic rays continue their assigned lifetimes;
- existing rays continue normal fade and beam evolution;
- region, budget, source, and explicit lifecycle retirement still operate;
- authored and gameplay-requested rays continue using usable blended cloud samples;
- new atmospheric candidate evaluation pauses;
- no transition-only full-population retirement occurs.

At or above the threshold, new candidate evaluation resumes.

Runtime state is explicit:

- `Disabled` — authoring gate off;
- `Suspended` — a required dependency, usable ground reference, or valid camera-plane footprint is unavailable;
- `SpawningPaused` — existing population operates but new spawning is paused for cloud transition;
- `Running` — lifecycle and new candidate evaluation operate normally.

Beam-layout evolution is independent of cloud-pattern evolution and continues throughout each ray slot lifecycle.

## 6. Rendering and receiver response

### 6.1 Beam rendering

Each active zone may render multiple separate continuous parallel beam ribbons derived from one area descriptor and seeded layout.

The current renderer contract supports overlapping authored and procedural zones and preserves continuous beam evolution between layout seeds.

### 6.2 Surface response

A pooled shadowless URP Spot Light may be assigned per active zone for receiver-material lighting. Source direction and zone geometry determine its resolved presentation.

The cleanup does not change the renderer feature or surface-light implementation.

### 6.3 Vegetation accents

Production vegetation response uses the indexed per-additional-light sidecar. Each active LightRay Spot may publish:

- source direction;
- accent intensity;
- coverage;
- softness.

The protected GPU layout remains two `float4` values per indexed additional light.

Legacy geometric-match and false-colour diagnostic shader infrastructure still exists after V1.3A and is scheduled for a separate shader cleanup. V1.3A removes its Controller Inspector suite and report exposure but does not alter shared HLSL or shader resources.

## 7. Preset authority and migration boundary

Procedural population already requires an Active Preset. Authored Anchors still contain legacy duplicate appearance and evolution fields for serialized compatibility.

A later bounded patch must:

1. inspect all live Controllers, Anchors, scenes, prefabs, and preset references;
2. compare saved fallback values against assigned presets;
3. obtain exact approval for serialized asset modifications;
4. migrate meaningful values;
5. require an Active Preset for authored rays;
6. remove Controller and Anchor duplicate appearance authority;
7. remove the legacy preset `SourceKind` field.

Archive 48 does not include the complete `.meta`, scene, and prefab state required to prove that migration safe. V1.3A therefore preserves those fields and does not claim mandatory preset authority is complete.

## 8. Presets and test assets

All curated preset assets remain.

`Assets/Game/Demo/Profiles/Weather/LightRays/_TEST.asset` is intentional. It is a deliberately strong and obvious testing preset and must remain unchanged. Future production Weather orchestration must exclude it from automatic production selection rather than deleting or weakening it.

The unused preset catalog and disconnected selector assets are deleted in V1.3A. Direct manual Active Preset selection and explicit runtime switching remain available.

## 9. Inspector architecture

### 9.1 LightRay Controller

The Controller Inspector has exactly seven roots:

1. Core Setup
2. Source & Rendering
3. Automatic Population
4. Advanced System
5. Diagnostics
6. Runtime Status
7. Report

Only Runtime Status contains resolved or live telemetry.

Diagnostics contains editable debug controls only:

- Render Debug View;
- Show Population Debug.

Report contains one action:

- Copy LightRay Report.

The obsolete projection probe, procedural test pair, cloud-aware test ray, separate beam-evolution audit, vegetation diagnostic suite, and first-authored-ray telemetry root are removed.

### 9.2 Cloud Controller

The Cloud Inspector retains production authoring, cookie preview, actions, receiver audit, and benchmark controls. Ordinary live state is consolidated under its Runtime Status root. The frozen cloud runtime is unchanged.

## 10. Deleted V1.3A infrastructure

V1.3A deletes:

- `WeatherLightRayPresetCatalog` code and asset;
- disconnected `WeatherLightRaySelectionProfile` and runtime;
- selector-only LightRay population profile code and assets;
- selector-only shared enums and dependency structures;
- Controller selection-dependency resolver;
- LightRay-owned CPU projection-probe infrastructure;
- Procedural Test Pair Editor scaffolding;
- Cloud-Aware Test Ray Editor scaffolding;
- separate Beam Evolution Runtime Audit;
- Controller/Inspector vegetation diagnostic suite exposure.

It does not delete:

- actual LightRay presets;
- `_TEST.asset`;
- production procedural APIs;
- cloud transmission query APIs;
- beam evolution;
- renderer or surface-light resources;
- indexed vegetation sidecar;
- cloud debug, receiver audit, or benchmark.

## 11. Validation contract

V1.3A requires live Unity validation in Unity 6000.5.0f1:

1. project compiles with zero errors;
2. the LightRay Inspector shows exactly the seven documented roots;
3. obsolete controls and actions are absent;
4. normal automatic atmospheric population still spawns and retires rays;
5. cloud evolution below threshold reports `SpawningPaused` and does not retire existing rays solely because of the transition;
6. moving Population Focus Override translates the camera footprint without changing its shape;
7. authored and procedural multi-ray rendering, surface Spots, beam evolution, and vegetation response remain functional;
8. no D3D12 missing-SRV warning returns;
9. the copied report exposes runtime state, derived budgets, counts, and actionable errors;
10. the cloud cookie producer, receiver shading, debug overlay, audit, and benchmark remain unchanged.

## 12. Deferred work

- serialized mandatory-preset and Anchor migration;
- preset `SourceKind` removal;
- legacy vegetation diagnostic HLSL/shader removal;
- deliberate Weather orchestration and production preset eligibility;
- standalone Player and broader Weather-system validation where already deferred by the frozen subsystem documents.

## 13. V1.3A2 stateless turnover randomization

V1.3A2 corrects deterministic location repetition in automatic atmospheric population without adding recent-cell history or any persistent no-repeat cache.

### 13.1 Shuffled cell traversal

The active camera-footprint bounding cells are no longer visited in fixed row-major order. Each complete traversal uses a deterministic permutation resolved from:

- Population Seed;
- the current turnover epoch;
- the current active cell count.

The implementation stores only the current traversal cursor, permutation offset, coprime step, cell count, seed, and epoch. It does not allocate or retain a shuffled-cell array. Completing a traversal advances the epoch and resolves a new permutation.

### 13.2 Cell identity versus activation identity

Each population cell retains a stable cell identity for occupancy, cooldown, and duplicate prevention. Each attempted activation receives a separate activation identity derived from the stable cell identity and current turnover epoch.

The activation identity owns:

- the exact randomized position inside the cell;
- the assigned lifetime;
- the procedural beam variation seed;
- the external procedural opening identity for that activation.

An active ray remains static. Randomization occurs only when a replacement activation is created. Reusing a cell in a later traversal therefore does not recreate the same exact position, lifetime, or beam layout.

### 13.3 Explicit non-goal

V1.3A2 adds no recent-cell queue, no short-term spatial history, no rejection cache, and no new Inspector control. Candidate Reuse Delay remains the only time-based cell reuse prohibition. The stateless traversal and activation variation are validated first before considering any memory-backed anti-repeat policy.
