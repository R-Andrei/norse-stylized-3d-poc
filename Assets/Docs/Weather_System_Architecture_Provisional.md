# Weather System Architecture

## Status

**Current documentation revision:** `WEATHER-SYSTEM-ARCHITECTURE-2026-07-31`

This document defines current subsystem ownership and integration boundaries. Detailed implementation contracts remain in the subsystem documents:

- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Docs/Weather_Cloud_Shadow_Handoff.md`
- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Docs/Weather_Inspector_Cleanup_Plan.md`

The future high-level Weather orchestration layer is not yet implemented.

## 1. Architectural rule

Weather is a set of cooperating systems, not one monolithic effect.

Each subsystem owns its own data and runtime behavior. Cross-system relationships are supplied deliberately at runtime rather than embedded as hidden ownership assumptions.

## 2. Current subsystems

### 2.1 Wind

The Wind Domain owns authoritative world-space XZ wind direction and strength and exposes CPU sampling for gameplay and visual consumers.

Consumers may implement their own response dynamics. Vegetation bend response and stylized wind trails are not the authoritative wind value.

The current wind and wind-trail contracts remain documented in `Weather_Wind_Architecture.md`.

### 2.2 Cloud shadows

The Cloud Shadow Controller owns the globally tiled URP directional-light cookie used to attenuate direct environmental sunlight.

It owns:

- deterministic cloud-pattern generation;
- coherent phase movement driven by Weather wind or fallback direction;
- low-frequency seed evolution and blended-cookie publication;
- present and future CPU transmission queries;
- Sun availability gating for cookie installation;
- debug visualization;
- receiver audit;
- performance benchmark.

The cloud receiver path is frozen. Adjacent systems may query cloud data but must not duplicate or modify cookie generation or receiver attenuation.

### 2.3 LightRays

LightRays are generic and are not inherently owned by the Sun, Moon, clouds, time of day, or Weather.

The system supports atmospheric weather rays, authored beams, quests, objectives, nighttime scenes, scripted storytelling, and gameplay-controlled presentation.

A specific runtime request or producer may receive:

- a directional source;
- a source availability gate;
- a cloud policy;
- lifecycle and placement policy;
- an appearance preset.

Those dependencies apply to that request. They do not become global LightRay ownership.

The current automatic implementation is one cloud-opening atmospheric producer. Its current Controller wiring may supply the resolved daylight directional source, but production code does not read preset source-family metadata and does not define all automatic rays as sunlight.

### 2.4 Time of day and celestial sources

Time of Day and lighting systems remain authoritative for any celestial-light direction, colour, intensity, and availability they publish.

LightRay requests may consume those values when their owner chooses. The LightRay core does not require them.

The future Weather orchestrator may use time-of-day and celestial state as eligibility inputs, but that policy is not embedded in visual presets.

## 3. Future Weather orchestration

A future orchestration layer is expected to decide:

- active Weather state and transitions;
- which atmospheric consumers are enabled;
- eligible LightRay presets and populations;
- runtime dependencies such as cloud openness, directional source, time of day, or storm state;
- production exclusion of test-only assets such as `_TEST.asset`;
- transition sequencing between Weather presentations.

This layer does not yet exist. The deleted LightRay selector and catalog were disconnected speculative infrastructure and are not the foundation for future orchestration.

The future design must be reviewed as a new bounded feature rather than reconstructed from deleted selector assumptions.

## 4. Cross-system contracts

### Wind to cloud

The cloud system may sample authoritative Weather wind direction at a bounded cadence and apply a configured angular offset. Cloud movement remains coherent world-phase translation.

### Cloud to LightRay

The cloud system exposes query-only present and future direct-light transmission plus stability status.

The current atmospheric LightRay producer uses those queries for candidate placement. Existing LightRays may use the blended cookie during cloud evolution. Below the configured transition threshold, new atmospheric spawning pauses while existing rays continue normally.

### Directional light to cloud

The cloud cookie is installed only on the resolved authoritative directional Sun when its intensity and elevation pass the cloud system’s gate. This is a cloud-shadow implementation contract, not a LightRay ownership rule.

### Directional source to LightRay

A LightRay request or producer may be supplied a runtime directional source for orientation, source gating, cloud projection, or surface presentation. Other rays may use custom, vertical, scripted, or source-independent direction.

### LightRay to gameplay

Gameplay systems may request rays or consume LightRay influence. They own healing, damage, buffs, quest logic, objective state, and other gameplay consequences. The visual system does not own those effects.

## 5. Inspector organization

Each Weather component retains a dedicated custom Inspector.

- Authoring controls remain grouped by subsystem responsibility.
- Resolved and live state is consolidated under one Runtime Status root per Controller.
- Development-only LightRay smoke tests and obsolete diagnostics are removed.
- The cloud receiver audit and performance benchmark remain because they validate the frozen production cloud path.
- No Inspector foldout state is serialized.

The exact LightRay cleanup contract is in `Weather_Inspector_Cleanup_Plan.md`.

## 6. Current non-goals

The current cleanup does not implement:

- a high-level Weather state machine;
- automatic time-of-day preset selection;
- hardcoded Sun/Moon LightRay ownership;
- visible cloud geometry or volumetrics;
- new wind gameplay effects;
- mandatory LightRay preset migration across scenes and prefabs;
- vegetation diagnostic shader removal;
- a new cloud receiver path.

## 7. Test-only LightRay preset

`Assets/Game/Demo/Profiles/Weather/LightRays/_TEST.asset` is intentionally retained as an obvious high-strength testing preset.

Future production orchestration must exclude it from automatic selection. It is not a cleanup candidate.

## 8. Validation status

The cloud-shadow V0 producer and receiver path are frozen based on its recorded Unity validation and benchmark evidence.

The V1.3A LightRay/Inspector cleanup requires fresh Unity 6000.5.0f1 compilation and runtime validation after application to the live project.

Serialized mandatory-preset migration remains blocked until the live project’s complete `.meta`, scene, prefab, Controller, and Anchor state is audited.
