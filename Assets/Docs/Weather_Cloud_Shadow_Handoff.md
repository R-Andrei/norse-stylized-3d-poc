# Weather Cloud-Shadow Architecture

## Status

**Stable implementation:** `WEATHER-CLOUD-SHADOW-V0.4-FREEZE`

**Current documentation revision:** `WEATHER-CLOUD-SHADOW-CROSS-SYSTEM-2026-07-31`

The cloud-shadow producer and receiver path are accepted and frozen. The current LightRay cleanup updates only the cross-system consumption policy and Cloud Inspector telemetry organization. It does not redesign cloud generation, movement, projection, receiver shading, debug overlay, receiver audit, or benchmark behavior.

## 1. Purpose

The cloud-shadow system creates one coherent moving direct-sun transmission field across gameplay-world receivers.

Transmission convention:

- `1` means open direct sunlight;
- lower positive values represent retained direct sunlight beneath cloud;
- the configured clouded-transmission floor prevents uncontrolled blackout.

The implementation uses a URP main directional-light cookie on the authoritative Sun.

## 2. Frozen ownership

The Cloud Shadow Controller owns:

- deterministic R8 cookie generation;
- globally tiled world-space projection;
- coherent cookie phase and movement;
- authoritative Weather-wind sampling or fallback movement direction;
- low-frequency deterministic seed evolution;
- blended-cookie upload during evolution;
- Sun intensity/elevation gate for installing the cookie;
- present-time CPU transmission query;
- future-time CPU transmission query;
- evolution-stability status;
- finite diagnostic overlay;
- cookie preview;
- receiver-compliance audit;
- performance benchmark and restoration.

It does not own:

- visible cloud geometry;
- volumetrics;
- LightRay storage or rendering;
- LightRay candidate identity, budgets, lifecycle, or placement policy;
- gameplay effects;
- Weather state orchestration.

## 3. Receiver contract

The authoritative directional cookie is consumed through compatible receivers’ normal main-light cookie path.

Production receivers must not apply a second custom cloud attenuation on top of the cookie. Double application is prohibited.

The receiver scope includes sun-responsive gameplay-world materials such as Ground, banks, riverbed, generated masses, vegetation, River, characters, buildings, props, snow, ice, and future compatible world materials.

Explicitly unlit, emissive, UI, diagnostic, depth-only, shadow-caster, motion-vector, and selection passes are outside the visible receiver requirement unless their role changes.

## 4. Performance status

The frozen V0 benchmark record reported, for the validated 2560 × 1440 Direct3D12 Editor Play Mode stress view:

- restoration `PASS`;
- alternating paired benchmark order;
- mean paired GPU median delta of `+0.016 ms` for the static cookie;
- mean paired GPU median delta of `+0.011 ms` for the moving cookie;
- no SetPass regression;
- no post-evolution residual cost.

That evidence did not justify a hybrid receiver system. Standalone Player and low-end-hardware confirmation remain part of the broader future testing sprint rather than the current cleanup.

## 5. CPU transmission query contract

The cloud Controller exposes query-only sampling of the retained readable cookie.

Queries:

- project a supplied world position into the same directional-cookie domain;
- sample the current blended R8 cookie with repeat behavior;
- may project a bounded future-time phase offset;
- return explicit availability and stability status;
- do not regenerate the cloud pattern;
- do not read back the GPU;
- do not create a second field;
- have no idle cost when no consumer calls them.

During seed evolution, usable samples report `EvolutionUnstable` while representing the currently blended cookie.

## 6. LightRay integration boundary

LightRays are a separate generic system. They are not inherently Sun-owned or Weather-owned.

The current cloud-opening atmospheric LightRay producer is one consumer of the cloud query API. Its owner supplies a runtime directional source and placement policy. Other LightRays may ignore clouds, use another source, use custom direction, or exist for gameplay and storytelling purposes unrelated to sunlight.

The cloud Controller does not decide:

- whether an atmospheric population should be active;
- which LightRay preset is eligible;
- how many rays exist;
- how candidates are identified or spaced;
- how long rays live;
- how cloud instability affects spawning.

Those are LightRay producer or future Weather-orchestration decisions.

## 7. Cloud transition consumption policy

Cloud evolution continues publishing the blended cookie.

The current LightRay atmospheric producer uses this policy below its configured `Cloud Transition Spawn Resume` threshold:

- existing rays continue normally;
- their fades and beam evolution continue;
- normal region, budget, source, and lifecycle retirement continues;
- authored/gameplay rays may consume usable blended samples;
- new atmospheric candidate evaluation pauses;
- the transition does not retire the existing automatic population.

This replaces the obsolete transition-wide suspension assumption. It does not change the cloud producer.

## 8. Cloud Inspector contract

The custom Cloud Inspector retains:

- activation and Sun-cookie authoring;
- cloud-pattern controls;
- movement controls;
- evolution controls;
- Sun availability gate;
- debug visualization controls;
- generated-cookie preview;
- manual actions and copied report;
- receiver audit entry point;
- performance benchmark.

Ordinary resolved and live state is consolidated under one `Runtime Status` root. Benchmark-specific result presentation remains with the benchmark workflow.

## 9. Protected implementation boundaries

The current LightRay cleanup must not alter:

- `WeatherCloudShadowCookieGenerator` behavior;
- cookie format or receiver installation;
- movement math;
- seed-evolution generation or upload cadence;
- present/future query projection math;
- debug overlay rendering;
- receiver audit logic;
- benchmark execution or restoration;
- shader receiver contracts.

Any future change to those surfaces requires separate evidence and approval.

## 10. Validation after adjacent cleanup

After applying the LightRay/Inspector cleanup, confirm in Unity 6000.5.0f1 that:

1. the cloud cookie still generates and moves;
2. the Sun gate still installs and removes the cookie correctly;
3. present and future CPU queries remain usable;
4. seed evolution completes and restores stable status;
5. debug overlay and cookie preview still function;
6. receiver audit remains available;
7. benchmark controls and restoration remain available;
8. no cloud runtime source file changed outside the explicitly approved cleanup scope.
