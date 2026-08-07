# Weather LightRay and Cloud Inspector Cleanup Plan

## Status

**Current patch:** `WEATHER-LIGHT-RAY-CLEANUP-V1.3A3-VEGETATION-SIDECAR-CLOSURE`

**Current state:** V1.3A plus the A1 footprint recovery and A2 stateless turnover correction are the accepted runtime baseline. V1.3A3 shared-vegetation closure is implemented and has passed the final static/cross-subsystem audit (`62 / 62`). Unity 6000.5.0f1 compilation and runtime validation remain pending. The serialized mandatory-preset migration remains a later bounded patch.

## Objective

Clean the current Weather LightRay and cloud authoring surfaces without changing the frozen cloud-cookie producer or the indexed vegetation-accent sidecar.

The patch will:

1. make LightRays source-agnostic at the core architecture level;
2. remove the unused preset catalog and disconnected preset-selection infrastructure;
3. remove obsolete non-shader LightRay diagnostics and Editor smoke tests;
4. separate automatic-population dependency suspension from cloud-transition spawn pausing;
5. preserve existing automatic rays through cloud-pattern transitions;
6. remove obsolete or derived automatic-population controls;
7. make the camera-ground footprint the only automatic-population region shape;
8. make the LightRay and Cloud Inspectors production authoring surfaces with one telemetry root each;
9. replace stale Weather documentation with the current agreed architecture;
10. preserve `_TEST.asset` as an intentional high-strength testing preset that future production orchestration must ignore.

## V1.3A3 — Vegetation sidecar closure plan

### Objective

Finish the already-completed migration from legacy LightRay vegetation globals/geometric diagnostics to the indexed per-additional-light sidecar without changing the protected sidecar layout, renderer ordering, ordinary vegetation lighting, cloud behavior, automatic population, surface Spot geometry, or serialized LightRay authoring.

V1.3A3 must also correct the remaining production intensity-authority defect: the indexed sidecar strength must be derived from the preset-resolved `AccentLineIntensity` value, including preset-transition interpolation, rather than directly from the legacy Controller fallback field.

### Approved V1.3A3 files

Modify:

- `Assets/Docs/Weather_Inspector_Cleanup_Plan.md`
- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Docs/Weather_System_Architecture_Provisional.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl`
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader`

Protected and reviewed but byte-identical in this patch:

- `Assets/Game/Rendering/Weather/WeatherLightRayRendererFeature.cs`

No scene, prefab, material, renderer asset, preset asset, `.meta`, layer, tag, package, project-setting, cloud-runtime, population-runtime, Anchor, or serialized-asset change is approved.

### Reviewed evidence

- The current Controller still declares and republishes six legacy Weather vegetation shader globals, an inactive published Spot position/direction state, diagnostic-mode state, and two unused legacy direction helpers. Production per-Light metadata is already supplied through the Light-to-sidecar dictionaries consumed by the renderer feature.
- The current vegetation include retains the protected two-`float4` sidecar plus legacy globals, diagnostic-only result fields, diagnostic-mode coverage bypass, diagnostic bookkeeping in both additional-light loops, and a false-colour diagnostic resolver.
- The benchmark vegetation shader has one remaining diagnostic-mode fragment return before the normal production lighting resolve.
- Repository-wide consumer search found no supported activation path for the legacy vegetation diagnostic mode and no production consumer of the legacy global Spot/direction/intensity/coverage bridge beyond the obsolete HLSL branches being removed.
- The renderer feature publishes one camera-local sidecar record per URP additional light and is the protected producer. Its record stride, ordering, zero fallback buffer, binding pass, Forward/Forward+ index alignment, and publication telemetry are not modified.
- The canonical vegetation architecture already defines the indexed sidecar as the active contract but retains historical sections that describe geometric matching as if current. Those sections must remain as history while current-state wording is corrected.

### Invariants

- `VegetationAdditionalLightAccentData` remains exactly two `float4` values in the same order and semantics.
- `parameters.w` remains the explicit Weather-LightRay identity/override flag independently of artistic intensity. Intensity `0` must therefore still publish an active override record when the Spot is a LightRay so ordinary punctual edge-accent fallback cannot reappear.
- URP `Light.direction`, attenuation, colour, cone, range, and rendering-layer filtering remain authoritative for ordinary vegetation body lighting.
- Only indexed `sourceDirectionWS` may replace the radial Spot direction for the Weather-specific stylized blade-edge side selector.
- Ordinary point/spot lights receive zero sidecar records and retain the generic punctual edge path.
- Coverage remains a stable whole-card threshold and never scales surviving radiance.
- Softness shapes only the selected Weather blade-edge profile.
- The current surface Spot Light, atmospheric renderer, cloud producer, automatic population, beam evolution, lifecycle, and storage behavior remain unchanged.

### Non-goals

- no sidecar layout or stride change;
- no renderer-feature modification;
- no new diagnostic suite or false-colour replacement;
- no vegetation material recalibration;
- no coverage, softness, attenuation, or edge-profile redesign;
- no cloud, automatic-population, source-orchestration, Anchor, preset-serialization, or `_TEST.asset` change;
- no mandatory-preset migration.

### File-by-file sequence

1. Update this canonical plan and lock the V1.3A3 scope before implementation.
2. Remove Controller legacy global-publication IDs/state/helpers/calls while preserving sidecar dictionaries, publication telemetry, surface Spots, and cached exponential scale.
3. Change the cached exponential input from the serialized fallback field to the preset-resolved `AccentLineIntensity` property.
4. Reduce the vegetation include to production body/edge result state plus the protected indexed sidecar; remove legacy globals, diagnostic bookkeeping, coverage diagnostic bypass, and false-colour resolver.
5. Remove only the obsolete diagnostic fragment-return branch from the benchmark shader.
6. Update current Weather and vegetation documentation while preserving clearly historical patch records.
7. Run cross-subsystem shader/include audit, exact diff/scope audit, dead-symbol search, sidecar-layout comparison, syntax/static checks, and protected-renderer byte comparison.
8. Record Unity compilation/runtime validation as pending with exact current Inspector routes verified from final source.

### Risks and safeguards

- **Sidecar layout regression:** compare the final HLSL record declaration with the protected C# record and require the renderer feature to remain byte-identical.
- **Zero-intensity semantic regression:** keep `parameters.w = 1` for LightRay Spots even when the resolved strength is zero; only `parameters.x` becomes zero.
- **Preset-transition regression:** resolve cache input through `AccentLineIntensity` so the cached exponential mapping follows current preset interpolation each controller tick when the resolved input changes.
- **Ordinary-light regression:** remove only Weather-diagnostic branches and retain the generic punctual-light edge path byte-for-byte where practical.
- **Shared-include regression:** repository-wide include search confirms the benchmark vegetation shader is the sole current shader consumer; final audit must repeat this search.
- **D3D12 missing-SRV regression:** the renderer feature's per-camera zero fallback binding and sidecar binding pass are protected and unchanged.

### Acceptance criteria

- all six legacy Weather vegetation shader-global names are absent from production C#/HLSL source;
- no legacy Spot position/direction publication, diagnostic-mode publication, diagnostic colour resolver, or diagnostic shader branch remains;
- no `ProductionVegetationAccentMatchingEnabled`, `SupportedVegetationAccentSpots`, or legacy vegetation direction helper remains without a live consumer;
- the sidecar remains two `float4` values with unchanged C#/HLSL semantics and renderer stride;
- the renderer feature is byte-identical to the V1.3A2 baseline;
- sidecar strength is calculated from preset-resolved `AccentLineIntensity`, not directly from the fallback field;
- intensity `0` keeps the indexed override flag active for LightRay Spots while producing zero Weather-specific edge strength;
- coverage and softness behavior are unchanged;
- ordinary punctual-light body and edge response are unchanged;
- Game and Scene View sidecar publication remains camera-local and the valid zero fallback remains bound for other cameras;
- no missing sidecar SRV warning appears in Unity;
- two or more simultaneous LightRay Spots retain vegetation accents in Unity;
- Unity 6000.5.0f1 compiles C# and vegetation shaders with zero errors.

### V1.3A3 status

- [x] Complete review surface and repository-wide consumer scan.
- [x] Record approved scope, invariants, risks, and acceptance gates in the canonical plan.
- [x] Controller legacy bridge removal and intensity-authority correction.
- [x] HLSL diagnostic/global cleanup.
- [x] Benchmark shader diagnostic-branch removal.
- [x] Current-state documentation reconciliation.
- [x] Cross-subsystem final audit and static validation (`62 / 62`).
- [ ] Unity compilation and runtime validation.

## V1.3A approved files (historical completed scope)

### Modify

- `Assets/Docs/Weather_Inspector_Cleanup_Plan.md`
- `Assets/Docs/Weather_Cloud_Shadow_Handoff.md`
- `Assets/Docs/Weather_Light_Ray_Architecture.md`
- `Assets/Docs/Weather_System_Architecture_Provisional.md`
- `Assets/Game/Procedural/Weather/WeatherLightRayController.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayPopulationRuntime.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayPreset.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayTypes.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherCloudShadowControllerEditor.cs`

### Delete

- `Assets/Game/Procedural/Weather/WeatherLightRayPresetCatalog.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRaySelectionProfile.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRaySelectionRuntime.cs`
- `Assets/Game/Procedural/Weather/WeatherLightRayPopulationProfile.cs`
- `Assets/Game/Demo/Profiles/Weather/LightRays/WeatherLightRayPresetCatalog.asset`
- `Assets/Game/Demo/Profiles/Weather/LightRays/WeatherLightRaySelection_DefaultCycle.asset`
- `Assets/Game/Demo/Profiles/Weather/LightRays/WeatherLightRayPopulation_Daylight.asset`
- `Assets/Game/Demo/Profiles/Weather/LightRays/WeatherLightRayPopulation_IndependentNight.asset`

For V1.3A itself, no scene, prefab, material, renderer asset, shader, HLSL include, layer, tag, package, or project-setting change was approved. V1.3A3 supersedes only that earlier shader/HLSL exclusion with the explicitly bounded shared-vegetation scope above.

## Reviewed evidence

The complete current source and relevant consumers/producers were reviewed before implementation:

- LightRay controller storage, source resolution, lifecycle, cloud response, population integration, reports, diagnostics, and vegetation publication;
- automatic population settings, camera-footprint resolution, candidate lifecycle, cloud sampling, spawn/update, retirement, reporting, and debug records;
- LightRay Controller Inspector authoring, telemetry, Scene diagnostics, smoke tests, and reports;
- authored Anchor and preset descriptor contracts;
- shared LightRay types and renderer source-kind consumers;
- Cloud Controller query, evolution-stability, debug, report, and benchmark contracts;
- Cloud Controller Inspector authoring, telemetry, preview, actions, and benchmark UI;
- renderer and vegetation sidecar boundaries;
- current Weather architecture, LightRay architecture, cloud handoff, and prior Inspector plan;
- catalog, selection profile/runtime, and selection-only population profile code and assets;
- repository-wide consumers of selection-only shared types.

## Settled ownership

### Core LightRay system

The core LightRay system is generic. It owns rendering, storage, lifecycle, appearance application, beam evolution, source-policy evaluation, cloud-policy evaluation, authored registration, and procedural request execution.

It does not own daylight, the Sun, Weather eligibility, quest eligibility, storytelling eligibility, or automatic preset selection.

### Preset

A preset owns appearance and shared presentation only. It must not determine source ownership, automatic eligibility, or Weather dependencies.

The legacy serialized preset `SourceKind` remains in this patch only because deleting serialized preset data belongs to the later mandatory-preset migration patch. Production automatic population must not read it.

### Runtime request

A ray request may carry a source kind, explicit direction, source-gate policy, cloud policy, movement policy, lifecycle, and placement. Those values describe that request; they do not redefine global LightRay ownership.

### Weather orchestration

A future Weather orchestration layer will decide which atmospheric population is active, which preset is eligible, which environmental dependencies matter, and which directional source is supplied at runtime. This cleanup does not implement that orchestrator and must not hardcode its future decisions into the core system.

### Current automatic population

The current automatic producer is a cloud-opening atmospheric population. It may continue receiving the currently resolved daylight directional source for cloud projection and visual direction, but its contracts, labels, preset checks, stable identity, and documentation must not define all automatic LightRays as Sun-owned.

### Cloud producer

The cloud-shadow subsystem remains the frozen producer of the globally tiled directional cookie and present/future CPU transmission queries. Cookie generation, projection, movement, receiver shading, benchmark implementation, and debug overlay behavior are non-goals.

## Required behavior changes

### Cloud transition policy

During cloud-pattern evolution below `Cloud Transition Spawn Resume`:

- existing automatic rays continue their assigned lifetimes and normal fades;
- authored and caller-created rays continue normally;
- beam evolution continues;
- automatic active-ray lifecycle, camera-region retirement, budget retirement, cooldown, and reporting continue;
- pending and new automatic candidate evaluation pauses;
- no full automatic-population retirement occurs merely because the cloud pattern is unstable;
- cloud transmission marked `EvolutionUnstable` remains usable for existing `RespectClouds` rays.

Runtime state must distinguish `Disabled`, `Suspended`, `SpawningPaused`, and `Running`.

### Automatic population region

- obtain one representative Ground Mask hit to establish a horizontal reference-plane height;
- project the viewport centre and all eight perimeter rays mathematically onto that plane;
- do not require every viewport sample to hit a finite Ground collider;
- expand the resulting polygon by Camera Margin;
- when a focus override is assigned, translate the resolved footprint in XZ so its centre follows the override;
- preserve the ground-reference height for candidate raycast origins;
- raycast each actual candidate downward against the configured Ground Mask before spawning;
- never restore a circular fallback;
- suspend only when no usable ground reference or valid camera-plane footprint can be resolved.

### Derived controls

Delete serialized authoring for:

- Fallback Active Radius;
- Qualification Duration;
- Candidate Checks Per Tick;
- Ground Search Distance.

Derive:

```text
candidate checks per update = clamp(Ray Budget × 2, 4, 64)
ground raycast distance = max(100 m, resolved camera far clipping distance)
```

The first valid six-sample evaluation may spawn a new candidate during the same population update. No hidden second evaluation remains.

### Inspector contract

The LightRay Controller Inspector roots are:

1. Core Setup
2. Source & Rendering
3. Automatic Population
4. Advanced System
5. Diagnostics
6. Runtime Status
7. Report

Only Runtime Status contains live or resolved telemetry. Diagnostics contains editable debug toggles only. Report contains one copy action.

The Cloud Controller Inspector retains its production controls, preview, actions, receiver audit entry point, and benchmark, but all ordinary live telemetry moves to its sole Runtime Status root.

## Deleted infrastructure

Delete:

- unused preset catalog code and asset;
- disconnected selection profile/runtime;
- selector-only population profiles and assets;
- Controller selection-dependency resolver;
- selection-only shared enums and structures with no remaining consumer;
- LightRay-owned CPU cloud projection probe;
- Procedural Test Pair Editor scaffolding;
- Cloud-Aware Test Ray Editor scaffolding;
- separate Beam Evolution Runtime Audit;
- Controller vegetation diagnostic suite state and Inspector actions only where removal does not touch shared shader/HLSL code.

Historical V1.3A stopped at the Controller/Inspector boundary and deliberately left the legacy vegetation shader globals and false-colour branches for a separately approved shared-shader cleanup. V1.3A3 is that bounded cleanup and preserves the indexed sidecar layout exactly.

## Preserved behavior and invariants

- `_TEST.asset` remains unchanged and present.
- All curated LightRay preset assets remain.
- manual Active Preset selection remains authoritative for now.
- runtime preset switching API remains.
- authored and procedural multi-ray rendering remains.
- beam evolution remains active throughout each slot lifecycle.
- current surface Spot Light behavior remains.
- indexed vegetation accent sidecar publication remains byte-compatible.
- cloud cookie generation, query, movement, debug overlay, receiver audit, and benchmark remain.
- automatic population remains disabled by default and still requires an explicit Ground Mask.
- no automatic time-of-day, Sun, Moon, quest, or gameplay orchestration is introduced.
- no mandatory-preset Anchor migration or serialized appearance-field deletion occurs in this patch.

## File-by-file sequence

1. Rewrite this canonical plan and lock scope.
2. Delete disconnected catalog/selection code and assets.
3. Remove selection-only shared contracts and Controller consumers.
4. Refactor automatic population state, transition policy, footprint resolution, derived work budgets, and immediate candidate spawning.
5. Remove obsolete Controller non-shader diagnostic state and update reporting.
6. Rebuild the LightRay Controller Inspector.
7. Consolidate Cloud Inspector telemetry without changing cloud runtime behavior.
8. Rewrite the LightRay and provisional Weather architecture documents.
9. Run repository-wide dead-reference, source-ownership, serialized-field, and scope audits.
10. Run available static syntax checks and prepare exact Unity validation.

## Risks and safeguards

### Serialized migration

Legacy Controller and Anchor appearance fields and preset `SourceKind` remain until the later serialized migration audit. This patch does not claim mandatory preset authority is complete.

### Source coupling

The current controller still resolves a daylight directional source through legacy serialized fields. The automatic producer may consume that runtime value, but no preset eligibility check, automatic-population label, identity hash, or system-level statement may treat it as inherent ownership.

### Transition regression

Spawn pausing must not bypass normal lifecycle and retirement work. The runtime status enum and report must expose the distinction.

### Footprint regression

A focus override translates the complete camera footprint. It must not create a circle or use the override Transform height as terrain height.

### Shared shader boundary

No shader or HLSL modification is allowed. Diagnostic CPU publication that is still referenced by the unchanged shader path may remain temporarily, but no obsolete Inspector action or report may expose it as a supported production diagnostic.

## Acceptance criteria

- no catalog, selector, selector profile, or selector-only population profile reference remains;
- `_TEST.asset` is present and unmodified;
- production automatic population does not read preset `SourceKind`;
- automatic-population labels and reports contain no Sun-owned claim;
- cloud evolution below threshold produces `SpawningPaused`, not full suspension;
- existing automatic rays are not retired by the transition pause;
- existing cloud-response rays do not receive zero openness solely because data is evolution-unstable;
- the focus override translates a complete camera footprint;
- no circular population fallback remains;
- candidate and ground-raycast budgets are derived exactly as specified;
- a new valid candidate can spawn on its first six-sample evaluation;
- the LightRay Inspector has exactly the seven approved roots and only one telemetry root;
- the Cloud Inspector has one telemetry root;
- obsolete LightRay projection/test/audit actions are absent;
- the comprehensive report remains available;
- the frozen cloud producer, renderer path, surface Spots, and vegetation sidecar are unchanged;
- all modified C# files pass available lexical, brace, and dead-reference checks;
- Unity compilation and runtime validation are explicitly reported as pending until run in Unity 6000.5.0f1.

## Patch status

- [x] Review surface completed.
- [x] Scope and ownership decisions recorded.
- [x] Catalog/selection deletion completed.
- [x] Population runtime refactor completed.
- [x] Controller cleanup completed.
- [x] LightRay Inspector cleanup completed.
- [x] Cloud Inspector telemetry cleanup completed.
- [x] Canonical architecture documents updated.
- [x] Final diff and consistency audit completed.
- [x] Available static validation completed.
- [ ] Unity compilation and runtime validation completed.

## Static closure evidence

The Archive 48 source was audited after implementation with the following results:

- all six modified C# files passed lexical string/comment and delimiter-balance checks;
- all serialized property names drawn by the rebuilt Inspectors resolve to current Controller fields;
- all Controller properties and methods used by both custom Inspectors resolve to current source members;
- all Controller calls into `WeatherLightRayPopulationRuntime` resolve to current runtime members;
- no live source reference remains to the deleted catalog, selector, selector-only population profiles, or selection-only shared types;
- no source reference remains to the removed LightRay projection probe, test-pair, cloud-aware test-ray, separate evolution audit, or vegetation diagnostic Inspector suite;
- no production automatic-population read of preset `SourceKind` remains;
- no circular population fallback or obsolete serialized population control remains;
- the cloud Controller, cloud generator, LightRay renderer feature, LightRay Anchor, shared vegetation HLSL, vegetation shader, and Anchor Inspector are byte-identical to Archive 48;
- `_TEST.asset` is byte-identical to Archive 48 with SHA-256 `e25c225dd700dc3db10cb32fab5ce8b3d97d5c93da70e3a9a892a867b1c6842d`;
- the eight approved deleted files are absent from the implementation tree.

Unity Editor compilation and runtime behavior cannot be executed in this environment and remain mandatory acceptance gates.

## V1.3A2 turnover-randomization closure

The automatic population follow-up is intentionally stateless:

- each completed cell sweep resolves a new deterministic permutation from Population Seed and turnover epoch;
- stable cell identity remains responsible for occupancy, cooldown, and duplicate prevention;
- a separate activation identity varies exact within-cell position, lifetime, beam seed, and external opening identity;
- active rays remain static;
- no recent-cell history, queue, cache, or additional Inspector control is introduced.

Acceptance requires varied replacement locations during a mostly clear-sky multi-minute test, while the same Population Seed remains reproducible for the same runtime sequence.
