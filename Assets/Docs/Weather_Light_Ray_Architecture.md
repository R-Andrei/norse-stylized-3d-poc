# Weather LightRay Architecture

## Status

**Architecture identifier:** `WEATHER-LIGHT-RAY-CLEANUP-V1.3A5-AUTHORITY-CLOSURE`

**Current state:** V1.3A/A1/A2/A3/A4 are runtime-accepted. V1.3A5 is the authority-closure overlay: it deletes the source-level Anchor/Controller appearance, evolution, and migration fallbacks that A4 no longer consumes, preserves the validated per-ray preset/renderer/vegetation contracts, and replaces the last global preset-transition getter side effect with explicit tick-time cleanup. Unity 6000.5.0f1 compilation and runtime validation of A5 remain required in the live project.

A5 does not raw-edit scenes, prefabs, preset assets, or materials. Historical values stored in the deleted source fields are no longer runtime authority and are intentionally not preserved as permanent dead serialization.

---

## 1. Core principle

LightRays are generic visual and gameplay infrastructure.

The same runtime must support, without pretending every ray is sunlight:

- weather-driven atmospheric beams;
- authored environmental beams;
- quest and objective highlighting;
- nighttime scenes;
- scripted storytelling;
- ritual, magical, artificial, or otherwise nonphysical presentation;
- gameplay-requested zones with externally controlled visibility and lifetime.

The LightRay system does not own daylight, the Sun, the Moon, Weather eligibility, quest eligibility, time-of-day policy, or automatic preset selection.

A particular producer or ray may use the current Sun, Moon, an explicit direction, or another directional provider. That relationship is request/orchestration policy, not a core LightRay ownership contract.

---

## 2. Authority model

### 2.1 Controller

`WeatherLightRayController` owns:

- fixed-capacity runtime slots;
- authored/procedural registration and handles;
- ray lifecycle, fades, and release;
- default preset inheritance;
- per-slot preset resolution;
- beam-layout evolution;
- source/cloud policy resolution;
- renderer-facing snapshots and beam data;
- pooled surface Spot Lights;
- indexed vegetation-accent publication;
- the current cloud-opening atmospheric population integration;
- compact runtime telemetry and one comprehensive copied report.

The Controller does **not** make one preset the mandatory appearance of every ray. Its serialized `activePreset` field remains for compatibility but is user-facing as **Default Preset**.

### 2.2 Resolved per-ray preset

Every active ray resolves exactly one preset:

```text
resolved preset = per-ray Preset Override, when assigned
                  otherwise Controller Default Preset
```

If neither exists, that ray cannot become active and must report an actionable configuration/spawn error.

This is inheritance, not duplicate authority:

- Controller **Default Preset** is the inherited default;
- Anchor/request **Preset Override** selects another complete preset for that ray;
- raw appearance sliders are not duplicated onto the Anchor/request.

### 2.3 Appearance preset

`WeatherLightRayPreset` owns the complete shared visual presentation of rays that resolve to it:

- colour and warmth;
- atmospheric intensity and softening;
- camera-intersection fade;
- beam spacing, width, variation, edge softness, and fade shape;
- contact opacity;
- surface Spot presentation;
- footprint softness;
- vegetation accent intensity, coverage, and softness;
- beam-evolution preset, strength, and speed;
- default spawn geometry.

A preset does **not** own:

- Sun/Moon/source eligibility;
- Weather eligibility;
- time-of-day dependency;
- cloud dependency;
- quest/gameplay eligibility;
- lifecycle policy.

A4 removes the obsolete C# `WeatherLightRayPreset.SourceKind` authority. Existing serialized preset asset keys are not raw-edited in A4; unknown legacy YAML can be removed later during the approved serialized cleanup.

### 2.4 Anchor/request-local policy

An authored Anchor or runtime spawn request retains request-specific state such as:

- optional Preset Override;
- source kind or explicit direction;
- source availability gate;
- cloud response;
- lifecycle policy and durations;
- placement and geometry overrides;
- external visibility;
- variation seed;
- local intensity multiplier.

These describe one ray/request. They do not redefine the preset or Controller.

### 2.5 Downstream authority rule

Once a runtime slot has resolved its preset and descriptor, downstream systems consume the resolved slot/snapshot state.

They must not ask the Controller which preset is currently default in order to render or illuminate that ray.

Specifically:

- atmospheric mask drawing reads the ray descriptor/snapshot;
- surface Spot Lights read the ray snapshot;
- vegetation sidecar publication reads the ray descriptor/snapshot;
- presentation grouping reads resolved snapshot parameters;
- reports may display the resolved preset reference for telemetry.

This rule prevents Controller-global appearance authority from re-entering the system indirectly.

---

## 3. Authoring contract

### 3.1 Default preset

Unity authoring path:

**Weather LightRay Controller Inspector → Core Setup → Default Preset**

The serialized backing field remains `activePreset` to avoid unnecessary serialization churn.

Automatic atmospheric rays currently inherit this default. Future Weather orchestration may supply a deliberate preset without changing the core inheritance model.

### 3.2 Authored per-ray preset

Unity authoring path:

**Weather LightRay Anchor Inspector → Binding and Policy → Preset Override**

Behavior:

- `None` → inherit Controller Default Preset;
- assigned preset → use that preset for this Anchor.

The same section exposes **Source Kind**, because source policy belongs to the ray, not the preset.

### 3.3 Procedural/gameplay per-ray preset

`WeatherLightRaySpawnRequest` carries an optional `PresetOverride` reference.

- `null` → inherit Controller Default Preset;
- assigned → resolve that preset for the request.

This is CPU-side request state. `WeatherLightRayCloudSpawnSettings` forwards the same optional override through the cloud-aware procedural helper, so cloud-aware gameplay spawns do not lose per-ray preset authority before they become a spawn request. It does not alter GPU buffer layouts or introduce a shader-side preset lookup.

### 3.4 Vegetation response authoring

Vegetation LightRay response remains authored only on the preset asset.

Unity authoring path:

**Project → `Assets/Game/Demo/Profiles/Weather/LightRays/` → select a `WeatherLightRayPreset` → Inspector → Surface Response**

The user-facing controls are:

- **Vegetation Accent Intensity**;
- **Vegetation Accent Coverage**;
- **Vegetation Accent Softness**.

No Controller or Anchor duplicates are introduced. If two ray styles need different vegetation response, they use different presets.

`Local Intensity Multiplier` remains an instance-level strength scalar; it is not a second vegetation-authoring surface.

### 3.5 Test preset

`Assets/Game/Demo/Profiles/Weather/LightRays/_TEST.asset` remains unchanged. It is intentionally exaggerated for testing. Future production Weather orchestration must exclude it from automatic production selection rather than deleting or weakening it.

---

## 4. Runtime preset resolution and transitions

### 4.1 Slot-local resolved state

Each runtime slot records enough state to identify its resolved presentation authority:

- current resolved preset;
- previous resolved preset when participating in a transition;
- whether the preset is inherited from the Controller default;
- transition start/duration where applicable.

The numerical descriptor remains the renderer/receiver authority. The preset references are retained for resolution, transition tracking, grouping/reporting identity, and diagnostics—not as GPU data.

### 4.2 Controller default transitions

When runtime API code transitions the Controller Default Preset:

- rays inheriting the Controller default participate in that default transition;
- explicit override rays do not change;
- an inherited ray created while that transition is active joins the in-progress transition rather than popping directly to the destination presentation.

### 4.3 Override changes

A4 treats a change to an explicit per-ray override as an immediate preset switch. It does not add another transition-duration authoring surface.

If future gameplay requires authored cross-fades between explicit overrides, slot-local transition state already provides the correct architectural place to add that deliberately.

### 4.4 No legacy runtime fallback

A4 stopped constructing production appearance from legacy Anchor/Controller appearance and evolution values. A5 removes those obsolete serialized source fields, migration/version helpers, fallback-only Controller properties, and misleading Anchor layout convenience properties.

The retained Anchor serialization is request-local only: binding/preset override, source/cloud/gate policy, lifecycle, placement/geometry, spacing override, variation seed, and local intensity. Every active ray still resolves a preset before production presentation becomes active.

---

## 5. Source resolution

### 5.1 Source policy is per ray

`SourceKind` remains request-local policy. It answers how that ray obtains its source/directional state, for example:

- `Sun` → use the approved current Sun provider;
- `Moon` → use the approved Moon provider when available;
- `Independent` → use request/local independent direction semantics.

It is not a preset property.

### 5.2 Resolve before rendering

The Controller resolves each ray's numeric render-facing source state before publishing the snapshot, including:

- ray direction;
- direction to source;
- source colour contribution;
- source intensity/gate result;
- renderable effective intensity/visibility.

The atmospheric renderer therefore does not choose one global source family and does not discard rays whose `SourceKind` differs from the first visible ray.

The renderer consumes resolved numbers. Semantic source kind remains useful for policy and telemetry but is not a renderer ownership switch.

---

## 6. Atmospheric renderer: presentation groups

### 6.1 Why grouping exists

Individual ray mask drawing is already per ray, but a scalar R16 mask cannot retain separate final colours/softening state after unrelated rays are merged.

A4 therefore replaces the old model:

```text
all selected rays → one mask → first ray softening/composite
```

with:

```text
compatible rays → presentation group A → mask/soften/composite A
compatible rays → presentation group B → mask/soften/composite B
...
```

### 6.2 Grouping is by resolved final-pass compatibility

The group boundary is not simply `Preset` object identity and is not `SourceKind` identity.

A ray joins an existing group only when the parameters that must be shared after individual mask drawing are compatible. The signature includes resolved final-pass presentation such as:

- final atmospheric colour;
- softening strength;
- softening radius;
- softening direction;
- screen-space surface presentation when active.

Per-ray values already consumed during mask drawing remain per draw and do not force a new group merely because they differ, including:

- position/area;
- beam layout and seed;
- local intensity/lifecycle;
- beam evolution state;
- camera fade;
- individual zone geometry.

A ray with active screen-space surface presentation is isolated unless its complete required final-pass surface state can safely share the same composite.

### 6.3 Common-case cost

The expected weather case remains one group:

```text
several compatible SunWarm atmospheric rays → one mask/soften/composite sequence
```

Additional full-screen sequences are paid only when genuinely incompatible presentations coexist, for example a weather presentation plus a distinct quest presentation.

The grouping implementation is bounded by slot capacity and uses reused/preallocated storage. It introduces no LINQ, dictionary grouping, recent-history cache, or unbounded per-frame collection.

### 6.4 RenderGraph execution

Groups execute sequentially against the camera colour target. Temporary mask/softened resources are transient per recorded sequence and the camera-colour dependency is chained deterministically through the groups.

The existing beam buffers, zone buffers, shaders, and RenderGraph integration are extended rather than replaced.

### 6.5 Debug views

Non-final diagnostic views remain a combined diagnostic representation. Presentation grouping is production FinalComposite behavior; debug modes are not required to reproduce independent colour compositing for every group.

---

## 7. Vegetation sidecar

### 7.1 Protected GPU contract

Production vegetation response uses the indexed per-URP-additional-light sidecar.

The protected record remains exactly two `float4` values:

```text
float4 sourceDirection
float4 parameters
```

with the existing semantics:

```text
parameters.x = resolved vegetation accent scale
parameters.y = coverage
parameters.z = softness
parameters.w = LightRay override identity
```

No A4 HLSL or vegetation-shader change is required.

### 7.2 Per-ray values

A4 adds the three vegetation presentation values to `WeatherLightRayDescriptor`:

- Vegetation Accent Intensity;
- Vegetation Accent Coverage;
- Vegetation Accent Softness.

`WeatherLightRayPreset.ApplyTo()` resolves/interpolates them with the rest of preset appearance. The Controller builds each LightRay Spot's sidecar record from that ray's descriptor instead of copying one Controller-default record to every Spot.

This allows simultaneous rays to drive different vegetation response while keeping the validated indexed GPU layout unchanged.

### 7.3 Zero-intensity identity

`parameters.w = 1` remains independent of accent intensity.

A LightRay whose vegetation accent intensity is zero is still identified as a LightRay-controlled additional light. Its Weather-specific edge contribution becomes zero instead of falling back to the ordinary punctual-light edge-accent route.

### 7.4 Historical closure

V1.3A3 removed the obsolete global/geometric vegetation bridge, false-colour diagnostic path, and legacy diagnostic shader state. A4 builds on that sole indexed-sidecar authority; it does not reintroduce global LightRay vegetation controls.

---

## 8. Surface Spot Lights

Pooled shadowless URP Spot Lights remain per active zone where enabled by the resolved descriptor.

A4 resolves their presentation from the ray snapshot, including resolved source colour/intensity and preset-owned surface response. Surface Spot rendering no longer needs to ask which Controller preset/source family owns the current frame.

The Spot lifecycle, pool, cone/range calculus, and URP additional-light integration otherwise remain unchanged.

---

## 9. Current automatic atmospheric population

The current automatic producer is one cloud-opening atmospheric LightRay consumer. It is not the definition of all automatic LightRays.

It currently receives:

- Controller Default Preset inheritance;
- a runtime-resolved directional dependency used for cloud projection/ray direction;
- cloud transmission queries;
- render camera;
- Ground Mask;
- population authoring settings.

It does not inspect preset source-family metadata.

A4 does not change its accepted A1/A2 placement and turnover calculus beyond the preset field being correctly understood as a default/inherited preset.

### 9.1 Camera footprint

Automatic placement uses the complete eight-sample camera footprint projected against a horizontal ground-reference plane.

- One representative Ground Mask hit establishes reference height.
- Viewport centre/perimeter rays intersect that plane mathematically.
- Camera Margin expands the polygon in XZ.
- Focus Override translates the footprint; it does not create a circle.
- Actual candidates are still raycast against Ground Mask before spawn.
- There is no circular fallback.

### 9.2 Derived work limits

```text
candidate checks per population update = clamp(Ray Budget × 2, 4, 64)
ground raycast distance = max(100 m, resolved render-camera far clip)
```

### 9.3 Six-sample qualification

A candidate may spawn during its first valid complete evaluation using:

- current centre;
- four current surrounding samples;
- one predicted future centre sample based on assigned lifetime.

### 9.4 Stateless turnover diversity

A2 preserves stable cell identity but changes traversal and activation identity per turnover epoch.

Each later activation can therefore receive a different:

- within-cell position;
- lifetime;
- variation seed;
- opening identity.

No recent-cell history, queue, rejection cache, or other memory-backed no-repeat system exists.

---

## 10. Cloud transition policy

Cloud evolution publishes a blended usable cookie and marks samples `EvolutionUnstable` during transition.

Below **Cloud Transition Spawn Resume**:

- existing automatic rays continue lifetime/fades/evolution;
- ordinary region/budget/source/lifecycle retirement remains active;
- authored/gameplay rays can continue consuming the blended cloud representation;
- only new atmospheric candidate evaluation pauses;
- transition state alone does not retire the active population.

Runtime population state distinguishes:

- `Disabled`;
- `Suspended`;
- `SpawningPaused`;
- `Running`.

Beam-layout evolution is independent of cloud-cookie evolution.

---

## 11. Cloud-shadow subsystem boundary

The cloud-shadow subsystem remains a stable producer. It owns:

- deterministic directional-cookie generation;
- cookie movement/evolution;
- blended-cookie publication;
- present/future CPU transmission queries;
- stability state;
- receiver integration;
- debug overlay, receiver audit, and benchmark.

LightRays consume these outputs and apply their own population/request policy. A4 does not redesign or modify the cloud runtime.

---

## 12. Inspector architecture

### 12.1 LightRay Controller

Root categories remain:

1. Core Setup
2. Source & Rendering
3. Automatic Population
4. Advanced System
5. Diagnostics
6. Runtime Status
7. Report

Only Runtime Status contains live telemetry.

A4 changes the Core Setup authoring label from **Active Preset** to **Default Preset** while keeping the serialized backing field for compatibility.

Runtime Status exposes presentation-group count and per-ray resolved-preset information through the comprehensive report rather than creating a new development-dashboard root.

### 12.2 LightRay Anchor

The authored Anchor Inspector exposes, under Binding and Policy:

- Controller Override;
- Preset Override;
- Edit Mode Preview;
- Source Kind;
- cloud/source-gate policy.

Legacy appearance/evolution fields are not restored to the user-facing Inspector.

### 12.3 LightRay Preset

The preset Inspector is explicit rather than `DrawDefaultInspector()`-driven. It presents appearance sections deliberately and does not expose obsolete source ownership.

Surface Response contains the three vegetation controls under their final user-facing names.

### 12.4 Cloud Controller

The Cloud Inspector retains production authoring, cookie preview, receiver audit, benchmark, actions, and one Runtime Status telemetry surface. Cloud runtime behavior is unchanged.

---

## 13. Authority-closure boundary

A5 removes the obsolete source-level serialized Anchor/Controller appearance, evolution, and migration fields because A4 runtime validation established that they no longer construct active presentation. No scene, prefab, material, or preset asset is raw-edited by A5.

The supplied archive still does not expose authoritative live scene/prefab serialization, so A5 does not claim recoverability of historical values that may have existed only in those now-ignored fields. The accepted closure policy is to stop carrying permanent dead serialization solely for archival recovery.

The runtime invariant after A5 is strict: every active authored/procedural ray resolves one Preset Override or the Controller Default Preset; missing both is an actionable configuration/spawn failure.

---

## 14. Deleted cleanup infrastructure

The accepted cleanup series has already removed:

- preset catalog code/asset;
- disconnected preset-selection runtime/profile;
- selector-only population profiles/assets and shared selection contracts;
- LightRay-owned CPU cloud-projection diagnostic;
- Procedural Test Pair Editor scaffolding;
- Cloud-Aware Test Ray Editor scaffolding;
- standalone Beam Evolution Runtime Audit;
- old Vegetation Accent Diagnostic Suite;
- legacy global/geometric vegetation diagnostic shader bridge and false-colour path.

It preserves:

- actual LightRay presets, including `_TEST.asset`;
- procedural spawn/update/release APIs;
- beam evolution;
- cloud transmission APIs;
- renderer and pooled surface-light resources;
- indexed vegetation sidecar;
- cloud debug/audit/benchmark tools.

---

## 15. A5 validation contract

Unity 6000.5.0f1 validation must establish all of the following:

1. C# and shader import/compilation complete with zero errors.
2. Existing inherited automatic atmospheric rays spawn, turn over, and preserve A1/A2 cloud/population behavior.
3. An authored ray with a different Preset Override and Source Kind can coexist with inherited weather rays; presentation-group behavior remains as accepted in A4.
4. Vegetation underneath simultaneous rays continues to use each ray's descriptor-specific intensity/coverage/softness with no missing-SRV warning.
5. Controller Default Preset transitions through the runtime API still affect inherited rays but not explicit override rays, including rays joining an in-progress default transition.
6. After a completed default transition, the Controller carries no stale global previous-preset bookkeeping while per-slot transition completion remains independent.
7. The Anchor Inspector exposes only the retained request-local authoring fields and runtime telemetry; no deleted appearance/evolution field reappears.
8. The comprehensive report contains per-ray preset blend and vegetation values and no redundant Controller-global vegetation summary.
9. No scene/prefab/material/preset asset serialization is modified by A5; `_TEST.asset` remains unchanged.

---

## 16. Historical patch ledger

### V1.3A — Inspector/dead-infrastructure cleanup

- removed disconnected preset catalog/selector architecture;
- removed obsolete non-shader diagnostics/smoke tests;
- established production Inspector roots and one telemetry root;
- corrected cloud-transition policy to pause only new spawning;
- removed obsolete/derived population controls;
- made LightRays generic rather than preset/Sun-owned;
- reorganized stale Weather documentation.

### V1.3A1 — camera-footprint recovery

- replaced the over-strict requirement that every viewport sample hit a finite Ground collider;
- established one Ground reference plane and mathematical viewport projection;
- retained actual candidate Ground Mask validation;
- preserved camera-shaped population region with no circular fallback.

### V1.3A2 — stateless turnover randomization

- replaced fixed row-major candidate traversal with deterministic shuffled/permuted traversal per turnover epoch;
- separated stable cell identity from changing activation identity;
- changed within-cell position, lifetime, variation seed, and opening identity on later activations;
- added no recent-cell memory/cache.

### V1.3A3 — vegetation sidecar closure

- removed obsolete Weather vegetation shader globals and diagnostic path;
- removed false-colour diagnostic branch/bookkeeping;
- retained the indexed two-`float4` per-additional-light sidecar as sole production metadata path;
- corrected preset-resolved vegetation intensity authority.

### V1.3A4 — per-ray preset authority and presentation grouping

- Controller preset becomes an inherited Default Preset rather than global appearance ownership;
- authored/runtime rays may provide a Preset Override;
- source presentation resolves per ray before rendering;
- final atmospheric compositing partitions visible rays into compatible presentation groups;
- vegetation accent values resolve per ray and reuse the existing indexed sidecar;
- legacy serialized fallback data remains intact pending the separate live-project migration audit.

### V1.3A5 — authority closure

- deletes obsolete Anchor/Controller presentation, evolution, and migration serialization that A4 no longer consumes;
- retains only request-local Anchor serialization and per-ray preset resolution;
- removes misleading Anchor layout convenience properties based on unresolved local spacing;
- removes Controller-global fallback presentation properties and redundant vegetation summary telemetry;
- moves Controller-default transition expiry from the removed global blend getter into explicit tick-time cleanup;
- preserves A4 renderer grouping, source resolution, population, cloud, surface Spot, and indexed vegetation contracts.

---

## 17. Next work items

1. Compile and runtime-validate V1.3A5 against the validation contract above.
2. After validation, freeze the Weather/LightRay cleanup as closed.
3. Design future Weather orchestration separately; it may choose atmospheric eligibility/presets at runtime and must ignore `_TEST.asset` for production automatic selection.
