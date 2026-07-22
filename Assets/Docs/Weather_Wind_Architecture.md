# Weather Wind Architecture

## Status

**Active implementation plan — WEATHER-WIND-V0**

```text
Engine: Unity 6000.5.0f1
Render pipeline: URP
Camera: constrained top-down isometric gameplay camera
Authoritative spatial domain: world-space XZ only
Visual response cache: gameplay-anchor-centred 2D XZ field
Initial field resolution: 128 × 128
Initial cell size: 0.5 m
Initial covered area: 64 × 64 m
Initial update cadence: 10 Hz
```

This document is the canonical architecture and implementation ledger for shared Weather-owned wind. Wind is not owned by vegetation. Vegetation, future stylized wind lines, and gameplay systems consume the Weather wind contract according to their own response rules.

The provisional parent Weather-system record is `Assets/Docs/Weather_System_Architecture_Provisional.md`. This document remains authoritative for the implemented wind subsystem.

The current patch implements only the minimum reusable wind domain and vegetation response integration. It does not implement the complete Weather system, stylized wind-line rendering, player movement resistance, or authored regional weather logic.

---

## 1. Core ownership

### Weather owns

- prevailing XZ direction and base strength;
- broad XZ turbulence and irregular gust regions;
- the CPU-queryable authoritative wind sample;
- the GPU target-wind field used by visual consumers;
- the gameplay-anchor-centred dynamic response cache;
- world-space field coordinates, update cadence, and resource lifecycle.

### Vegetation owns

- stiffness and family response amplitude;
- root-to-tip bend weighting;
- small blade-detail flutter;
- composition with later interaction and trail fields.

### Future consumers

- Stylized wind lines sample or advect through the Weather target-wind field.
- Gameplay samples the CPU Weather wind function in Weather strength units; it must not read back the GPU response texture or interpret grass-bend metres as wind strength.
- Other visual systems may sample the target field or define their own response cache.

---

## 2. Domain scalability

The wind field does not belong to a grass patch and is never stretched to match vegetation area.

The initial visual field uses fixed world density:

```text
world coverage = resolution × metres per cell
128 × 0.5 m = 64 m per axis
```

A 5 m² grass patch samples a small part of the same field. A 500 m² patch samples a larger part. Several separate patches sample their corresponding world positions. Vegetation size and density do not change wind resolution.

The field follows an explicit gameplay anchor, normally the player or camera follow target, in whole-cell increments. When no anchor is assigned, the isometric camera forward ray is projected onto a configurable horizontal XZ plane; the field is never centred on the camera's horizontally offset transform unless projection is impossible. A toroidal ring offset preserves overlapping cells when the anchor moves. Only newly exposed rows or columns are initialized. Large teleports reset the field.

This initial implementation is one gameplay-anchor-centred field. Future extensions may add multiple player-centred fields, streamed tiles, or clipmap levels without changing the vegetation sampling contract.

---

## 3. Data model

### 3.1 CPU authoritative sample

`WeatherWindDomain.SampleWindXZ(worldPosition)` returns the current world-space XZ wind vector from the same configured procedural model used to build the GPU target field. Its magnitude is expressed in dimensionless Weather strength units, not grass-bend metres. The initial prototype clamps this authoritative vector to a configurable maximum of `1.0`.

The CPU sample is intended for later deterministic gameplay use such as:

- movement resistance when moving against severe wind;
- wind-line spawning decisions.

The CPU model remains the gameplay source of truth. GPU textures are visual caches and must not become authoritative gameplay state.

### 3.2 GPU target field

The target texture stores the current authoritative Weather wind vector:

```text
RG = target wind XZ in Weather strength units
```

This texture is suitable for future wind-line advection and visual-event decisions. It does not contain vegetation displacement.

It is generated procedurally in world space from:

- prevailing wind;
- broad smooth turbulence;
- lower-frequency directional variation;
- irregular moving gust regions.

There are no periodic ribbon fronts, event-index recovery bands, or distance-behind-wave calculations.

### 3.3 GPU response field

Two ping-pong response textures store:

```text
RG = current visual bend XZ
BA = bend velocity XZ
```

The target wind is converted to a bounded visual displacement before spring integration:

```text
targetBend = targetWind × (maximumVisualBendMetres / maximumWindStrength)
```

Each cell is then a damped spring driven toward that visual target:

```text
acceleration = omega² × (targetBend - bend) - 2 × dampingRatio × omega × velocity
velocity += acceleration × dt
bend += velocity × dt
```

The compute kernel uses up to four bounded integration substeps when the configured response frequency and fixed update interval require them. This preserves the cheap fixed-cadence field while preventing the explicit spring from becoming unstable at the exposed parameter limits.

Stable world-space variation changes response frequency and damping per region. Recovery, overshoot, and local settling therefore emerge from stored velocity rather than from analytical trailing waves.

---

## 4. Initial procedural wind model

The initial model is deliberately broad and cheap:

- a low prevailing XZ vector;
- two smooth world-space value-noise channels for multidirectional calm variation;
- an advected broad gust mask formed from two value-noise scales;
- gust strength biased toward the prevailing direction;
- no hard pulse edges and no repeated line-shaped event profile.

Calm movement may locally point in multiple XZ directions. Strong gust regions remain directionally biased toward the prevailing wind.

This model is a reusable Weather baseline, not a final authored weather-event system. Later Weather influences may add bounded directional regions, vortices, corridors, radial impulses, shelter attenuation, and scripted severe-wind events.

---

## 5. Performance contract

### Initial resource budget at 128 × 128

- Target field, `RGHalf`: approximately 64 KiB.
- Two response fields, `ARGBHalf`: approximately 256 KiB total.
- Approximate persistent field memory: 320 KiB, excluding Unity object overhead.

### Runtime work

- One full 128 × 128 compute update at the configured cadence; the baseline default is 10 Hz and the approved Inspector range is 5–60 Hz.
- One additional recenter dispatch only when the gameplay anchor crosses a field-cell boundary.
- One bilinear response-field sample per vegetation vertex.
- No per-grass CPU updates.
- No GPU readback.
- No new vegetation draw calls.
- No instance-layout or placement changes.

The field update cadence, resolution, and cell size remain Inspector controls and must be reported by diagnostics.

---

## 6. WEATHER-WIND-V0 implementation plan

### Objective

Replace the rejected vegetation-owned analytical gust/recovery system with a Weather-owned shared XZ wind domain and a gameplay-anchor-centred dynamic response field. Preserve the existing vegetation scene component through a hidden migration shim while removing all old analytical behavior and controls.

### Reviewed evidence

| Evidence | Finding | Status |
| --- | --- | --- |
| `Assets/AGENTS.md` | Requires read-only review, persistent plan before code, exact scope, post-change audit, and honest Unity validation status. | Reviewed |
| `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md` | Already assigns authoritative wind to Weather and limits vegetation to response. V0.1–V0.3 are rejected visual experiments based on traveling analytical fronts. | Reviewed |
| `Assets/Docs/Stylized_Vegetation_Architecture.md` | Requires one shared external Weather/Wind field and fixed-cost shared-field consumers. | Reviewed |
| `VegetationBenchmarkWindProvider.cs` | Publishes vegetation-owned global analytical gust, calm, oscillation, and recovery parameters every update. | Reviewed |
| `VegetationWindResponse.hlsl` | Derives active and recovery motion from distance to a traveling front, which user validation rejected as visibly mathematical. | Reviewed |
| `VegetationBenchmark.cs` and `VegetationBenchmarkEditor.cs` | Diagnostics and status UI directly depend on the temporary provider and require migration to Weather-domain reporting. | Reviewed |
| `VisualFrameworkDemo.unity` | Contains an active `VegetationBenchmarkWindProvider`; deleting the class would create a missing scene script. A behavior-free migration shim is required because raw scene replacement is not necessary. | Reviewed |
| River compute-field implementation | Confirms project conventions for `Resources.Load<ComputeShader>`, random-write half textures, bilinear filtering, explicit dispatches, and deterministic resource release. | Reviewed |
| Supplied archive | Contains no `.git` directory and no `.meta` files. Git status/history and original GUIDs are unavailable. New assets require generated `.meta` files; existing script paths are preserved. | Limitation recorded |

### Approved files

Create:

- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherWindDomain.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherWindDomainEditor.cs`
- `Assets/Game/Rendering/Weather/Includes/WeatherWindField.hlsl`
- `Assets/Game/Rendering/Weather/Resources/PS3DWeather/Compute/CS_WeatherWindField.compute`
- corresponding new `.meta` files

Modify:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmarkWindProvider.cs`
- `Assets/Game/Procedural/Vegetation/VegetationBenchmark.cs`
- `Assets/Game/Procedural/Vegetation/Editor/VegetationBenchmarkEditor.cs`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationWindResponse.hlsl`

No Ground, River, vegetation mesh, instance-layout, vegetation shader pass, scene, prefab, material, camera, URP asset, layer, or tag changes.

### Invariants and non-goals

- XZ only. No 3D voxel field and no airborne-effect architecture.
- Weather owns the field; vegetation only samples response.
- Field density is specified in metres per cell and is independent of grass-patch size.
- No analytical ribbon gust or distance-behind-front recovery remains active.
- No per-instance CPU wind updates.
- No GPU readback for gameplay.
- No wind-line renderer or player movement modifier in this patch.
- No scene raw edit. The legacy provider class remains only as a hidden subclass of `WeatherWindDomain` so the existing component continues to load.
- No change to vegetation placement, deterministic hash, instance stride, geometry candidates, or draw submission.

### File-by-file implementation sequence

| Item | File(s) | Required result | Status |
| --- | --- | --- | --- |
| W0.0 | Weather and vegetation architecture documents | Record ownership, domain scaling, field math, exact scope, risks, acceptance, superseded behavior, and validation. | Complete |
| W0.1 | `WeatherWindDomain.cs` | Own CPU wind sampling, field resources, toroidal gameplay-anchor following, fixed-cadence simulation, shader globals, diagnostics, and cleanup. | Complete |
| W0.2 | Weather compute shader | Generate target XZ wind and integrate damped response/velocity fields. | Complete |
| W0.3 | Weather HLSL include | Provide one global world-space field-sampling contract for visual consumers. | Complete |
| W0.4 | Vegetation wind include | Remove analytical gust/recovery globals and consume the Weather response field with only restrained blade-detail flutter. | Complete |
| W0.5 | Legacy provider | Replace old implementation with a hidden migration subclass only. | Complete |
| W0.6 | Vegetation diagnostics/editor | Replace temporary-provider reporting with Weather-domain status and report data. | Complete |
| W0.7 | Weather editor | Add reset and copy-report actions and clear resource/status feedback. | Complete |
| W0.8 | All approved files | Run scope, symbol, property, lifecycle, memory, syntax, and cross-subsystem audits; record Unity checks as pending where unavailable. | Complete at source level; Unity validation pending |

### Acceptance criteria

- Grass no longer uses any V0.1–V0.3 analytical gust or recovery equation.
- Nearby grass samples a continuous shared world-space response field.
- Broad irregular wind regions produce coherent movement without visible line fronts or follower waves.
- Recovery and overshoot result from stored response velocity and vary spatially through stable response parameters.
- Calm response can move in multiple XZ directions; strong wind remains biased toward the configured prevailing direction.
- Gameplay-anchor movement preserves overlapping field state through toroidal scrolling.
- A 5 m² and 500 m² vegetation area receive the same metres-per-cell wind quality when inside the visual domain.
- CPU `SampleWindXZ` exists for later gameplay integration.
- Target texture and shared HLSL contract expose authoritative Weather strength vectors for later stylized wind-line advection; the separate response texture stores visual bend in metres.
- Existing `VegetationBenchmarkWindProvider` scene component does not become a missing script and contains no old wind implementation.
- Vegetation draw count, placement hash, instance stride, mesh, Ground integration, and candidate controls remain unchanged.
- Inspector provides one Weather report-copy button.

### Risks

- Unity compute import and resource-path resolution cannot be proven outside Unity.
- `ARGBHalf`/`RGHalf` support must be checked at runtime; unsupported hardware must disable the field with a concrete error.
- Explicit spring integration must remain stable across allowed update rates and parameter ranges.
- Gameplay-anchor-centred coverage must be at least as large as the visible ground footprint plus margin; otherwise field edges become visible.
- One response sample per vegetation vertex adds texture bandwidth and requires target-hardware profiling.
- The legacy migration component retains its historical class name until the scene is intentionally migrated through Unity.

### Validation plan

1. Unity compilation and compute/shader import with zero errors.
2. Weather domain report confirms resources, 128 × 128 resolution, 0.5 m cells, 64 × 64 m world coverage, and one active publisher.
3. Play Mode visual test confirms irregular coherent movement, multidirectional calm response, and non-banded spring recovery.
4. Move the gameplay anchor across several cell boundaries and confirm no whole-field reset or visible seam. Also verify the camera-projection fallback centres the field on the viewed ground rather than the isometric camera transform.
5. Run the existing vegetation timed suite and confirm draw count, instance hash, and resource restoration remain valid.
6. Profile compute dispatch and vegetation GPU cost at 1440p on target-class hardware.


### Post-change consistency and compliance audit

- Actual implementation scope matches the approved runtime/editor/rendering/document set. No Ground, River, vegetation mesh, instance-layout, shader-pass, scene, prefab, material, camera, URP asset, layer, or tag file changed.
- The old `_VegetationExternalWind*` global family and all traveling-front/recovery equations are absent from active game code.
- `VegetationBenchmarkWindProvider` now contains only a hidden obsolete subclass of `WeatherWindDomain`; the existing scene script reference remains loadable without retaining any old wind behavior.
- The Weather target field stores authoritative dimensionless wind-strength vectors. The separate response field stores visual bend in metres plus bend velocity, preventing gameplay and wind-line consumers from inheriting grass-displacement units.
- CPU target-wind evaluation and compute target-wind evaluation use the same hashes, noise scales, travel parameters, thresholds, direction fallback, and magnitude clamp.
- C# shader-global identifiers match the Weather HLSL declarations. C# compute parameter and kernel names match the compute shader declarations.
- The compute resource path resolves to `Assets/Game/Rendering/Weather/Resources/PS3DWeather/Compute/CS_WeatherWindField.compute`.
- Toroidal recenter mapping was mathematically exercised across resolutions 32, 64, 128, and 256 with randomized offsets and deltas; preserved logical cells mapped to identical physical texels.
- Spring integration was exercised across the exposed update-rate, frequency, and damping ranges. Bounded one-to-four-step integration remained finite and clamped throughout the test matrix.
- The isometric anchor audit corrected field ownership from camera-transform XZ to an explicit gameplay anchor, with camera-forward projection onto a configurable horizontal plane as fallback.
- Lexical delimiter, comment/string termination, control-character, GUID uniqueness, old-global absence, property parity, and exact-scope scans passed for all changed source files.
- New field memory at the default 128 × 128 configuration remains approximately 320 KiB. Default compute cadence is one 16,384-cell simulation dispatch at 16 Hz, plus a branch-heavy recenter dispatch only when the anchor crosses a 0.5 m cell boundary.
- No performance exception is approved or required. The principal unresolved cost is one bilinear response-texture sample per vegetation vertex and must be profiled on target-class hardware.
- Unity 6000.5.0f1 C# compilation, compute import, vegetation shader import, Play Mode visual behavior, toroidal seam behavior, and GPU profiling remain pending because Unity is unavailable in this environment.

---

## 7. Superseded temporary system

`VEG-WIND-V0.1`, `VEG-WIND-V0.2`, and `VEG-WIND-V0.3` are historical rejected experiments. Their traveling gust, event-index character, distance-behind-edge recovery, and per-cluster analytical settling logic are not part of the active architecture after WEATHER-WIND-V0.

The historical sections remain in the vegetation document only as decision evidence and must be clearly marked superseded.

---

## 8. Future compatible work

The following are deliberately outside WEATHER-WIND-V0 but use the same Weather ownership:

- stylized drawn wind lines advected through the target field;
- bounded local vortices and directional wind regions;
- scripted severe-wind events;
- player movement resistance from CPU wind samples;
- Weather severity and visual-event thresholds;
- multiple active camera/player domains if later required.


---

## 9. Weather wind debug visualization

The active Weather wind domain exposes exactly three visualization modes. This supersedes the earlier `Target Wind` / `Response Wind` pair, which normalized two proportional vectors into nearly identical arrows:

- `Off`
- `Wind Field`
- `Response Error`

Both active modes render the same Scene-view frame of reference:

- a rectangular outline of the active XZ field coverage;
- a marker at the resolved field anchor position;
- arrow glyphs sampled at a configurable cell step;
- arrow direction from the sampled XZ vector;
- arrow length and colour jointly encoding magnitude.

`Wind Field` displays the authoritative Weather target vector. It is the field used by CPU gameplay sampling and intended future visual consumers such as stylized wind-line advection.

`Response Error` displays:

```text
actual visual bend - expected equilibrium bend
```

The expected equilibrium bend uses the same conversion as the compute simulation:

```text
expected bend = target wind × (maximum visual bend / maximum wind strength)
```

No arrow means the vegetation response has caught up with the current target. Error arrows expose lag, overshoot, and local spring settling without duplicating the target-field display. The response data is obtained through a small editor-only asynchronous GPU readback while the component is selected.

Debug visualization settings are diagnostic-only. Changing the mode, sample step, height offset, or arrow scale must not recreate the field or reset simulation time and spring state.

The visualization is drawn only in Scene view when the domain component is selected. It is not a gameplay overlay.


---

## 10. WEATHER-WIND-V0.1.1 — Debug compile fix and provisional Weather-system record

### Status

**Source implementation complete; Unity validation pending.** This section records the compile correction and documentation addition requested after WEATHER-WIND-V0.1.

### Objective

- Restore Unity C# compilation for the two debug-vector sampler lambdas.
- Preserve the existing three-mode debug design without changing runtime wind, compute simulation, field sampling, or vegetation behavior.
- Create a dedicated, explicitly provisional parent Weather-system architecture document that records only decisions already discussed and points to this wind document for the implemented subsystem detail.
- Link this detailed wind subsystem document to the new provisional parent and remove speculative consumer examples that were not part of the accepted top-down XZ design discussion.

### Reviewed evidence

| Evidence | Finding | Status |
| --- | --- | --- |
| Unity Console | `WeatherWindDomainEditor.cs(125,51)` and `(145,51)` report CS0748; `(125,59)` and `(145,59)` report CS1676. | Reviewed |
| `Assets/Game/Procedural/Weather/Editor/WeatherWindDomainEditor.cs` | Both `VectorSampler` lambdas mix implicit parameters with an explicitly typed `out Vector2 vector`, which C# rejects. The delegate itself correctly requires `out Vector2`. | Reviewed |
| `Assets/Game/Procedural/Weather/WeatherWindDomain.cs` | Debug contracts, field data, CPU target sampling, response texture exposure, and three-mode enum are present. No runtime change is required for the compile correction. | Reviewed |
| `Assets/Game/Procedural/Vegetation/VegetationBenchmarkWindProvider.cs` | Existing scene compatibility remains a hidden subclass of `WeatherWindDomain`; the editor continues to target derived types. | Reviewed |
| `VegetationBenchmark.cs` and `VegetationBenchmarkEditor.cs` | Consumers depend only on Weather-domain publication and resource status, not on debug-lambda implementation. | Reviewed |
| `Assets/Docs/Weather_Wind_Architecture.md` | Contains the implemented wind-subsystem architecture and WEATHER-WIND-V0.1 debug design; it needs a parent Weather-system document, not speculative expansion inside this subsystem record. | Reviewed |
| Repository metadata | Supplied source has no `.git` directory. Branch, HEAD, working-tree diff, and history are unavailable and will not be invented. | Limitation recorded |

### Approved files

Modify:

- `Assets/Game/Procedural/Weather/Editor/WeatherWindDomainEditor.cs`
- `Assets/Docs/Weather_Wind_Architecture.md`

Create:

- `Assets/Docs/Weather_System_Architecture_Provisional.md`
- `Assets/Docs/Weather_System_Architecture_Provisional.md.meta`

No runtime Weather-domain, compute, HLSL, vegetation, Ground, scene, prefab, camera, URP, layer, or tag changes.

### Invariants and non-goals

- Keep exactly three debug modes: `Off`, `Target Wind`, and `Response Wind`.
- Do not change editor readback cadence, arrow rendering, field math, resource ownership, or runtime behavior.
- The new Weather-system document is explicitly provisional until dedicated Weather-system work begins.
- Record only established decisions: Weather ownership, XZ wind domain, CPU gameplay contract, GPU visual caches, vegetation consumption, future stylized wind lines, possible severe-wind gameplay interaction, and future local wind influences.
- Do not define precipitation, seasons, temperature, cloud simulation, full event scheduling, biome rules, or other unapproved Weather details.
- Limit named current/future consumers in this document to the discussed vegetation, stylized wind-line, and gameplay integrations.

### File-by-file implementation sequence

| Item | File | Required result | Status |
| --- | --- | --- | --- |
| W0.1.1-A | `Weather_Wind_Architecture.md` | Record this plan before implementation, link the finished provisional parent document, and remove unsupported speculative consumer examples. | Complete |
| W0.1.1-B | `WeatherWindDomainEditor.cs` | Make all four lambda parameters explicit in both `VectorSampler` call sites, preserving identical behavior. | Complete |
| W0.1.1-C | `Weather_System_Architecture_Provisional.md` + `.meta` | Add the provisional parent architecture with only discussed ownership, consumer, scalability, and future-integration decisions. | Complete |
| W0.1.1-D | All approved files | Run exact-scope diff, syntax checks, GUID checks, and available compiler/parser validation; record Unity compilation as pending until user test. | Complete at source level; Unity validation pending |

### Acceptance criteria

- The four reported CS0748/CS1676 errors are removed by valid C# lambda syntax.
- No behavior or control changes occur in the debug visualizer.
- The dedicated Weather-system document clearly labels itself provisional and distinguishes implemented facts from future possibilities.
- `Weather_Wind_Architecture.md` remains the detailed canonical record for the implemented wind subsystem.
- Final patch contains only the four approved paths.


### Post-change consistency and compliance audit

- Actual changed paths exactly match the four approved paths.
- `WeatherWindDomainEditor.cs` differs from WEATHER-WIND-V0.1 only at the two failing lambda declarations. Each lambda now explicitly declares `int logicalX`, `int logicalY`, `Vector2 worldXZ`, and `out Vector2 vector`, matching the existing `VectorSampler` delegate.
- Debug modes, response readback cadence, arrow rendering, field sampling, runtime Weather resources, compute code, HLSL, vegetation integration, and scene compatibility are unchanged.
- `Weather_System_Architecture_Provisional.md` is explicitly labeled provisional, links to the detailed wind subsystem record, preserves the discussed XZ ownership and consumer architecture, and marks undefined Weather areas rather than inventing them.
- `Weather_Wind_Architecture.md` now links to the provisional parent record and limits named consumers to vegetation, stylized wind lines, and gameplay.
- Exact-scope comparison, lambda/delegate signature parity, C# lexical delimiter/string/comment checks, runtime/compute byte-identity checks, metadata GUID uniqueness, and control-character scans passed.
- No C# compiler with Unity assemblies is available in this environment. Unity 6000.5.0f1 compilation remains pending and must confirm that the reported CS0748/CS1676 errors are gone and reveal any subsequent errors previously masked by them.

---

## WEATHER-WIND-V0.1.2 — Visible Scene Debug Overlay

**Status:** Implementation approved; pending source implementation and Unity validation.

### Objective

Make the existing three-mode Scene-view wind debug visualization reliably visible over dense vegetation and ground geometry without adding new debug modes.

### User evidence

- Screenshot with `Debug View = Target Wind` shows no arrows, anchor marker, or field outline.
- Screenshot with `Debug View = Response Wind` shows no arrows, anchor marker, or field outline.
- Inspector values show `Debug Height Offset = 0.15` while the visible grass canopy is substantially taller than 0.15 m.

### Source evidence

- `WeatherWindDomainEditor.DrawVectorField` places arrows at `resolvedAnchorY + debugHeightOffset`.
- `WeatherWindDomainEditor.DrawVectorField` sets `Handles.zTest = CompareFunction.LessEqual`.
- `DrawDomainOutline` places the outline near the field plane and inherits depth-tested handle rendering.
- The response readback requests the native `ARGBHalf` render texture and interprets the returned byte stream as `Color[]`; explicit `RGBAFloat` conversion is safer for the editor CPU visualization contract.

### Approved files

Modify only:

- `Assets/Game/Procedural/Weather/Editor/WeatherWindDomainEditor.cs`
- `Assets/Docs/Weather_Wind_Architecture.md`

### Implementation plan

1. Replace selection-dependent `OnSceneGUI` reliance with a symmetric `SceneView.duringSceneGui` subscription owned by the active custom editor.
2. Restrict drawing to the selected Weather wind domain and preserve the existing `Off`, `Target Wind`, and `Response Wind` modes.
3. Draw arrows, anchor marker, and domain bounds with `CompareFunction.Always` so dense grass cannot occlude the diagnostic overlay.
4. Add a compact Scene-view legend stating the active mode and arrow encoding; do not add another view.
5. Request response-field readback with explicit `TextureFormat.RGBAFloat` conversion before reading `Color[]`.
6. Preserve all runtime field generation, compute, vegetation sampling, gameplay contracts, and update cadence.

### Acceptance criteria

- Target and response arrows are visible over dense grass at the existing `0.15 m` height offset.
- A selected-domain legend appears in Scene view and identifies the active mode.
- Field bounds and anchor marker remain visible even when occluded by scene geometry.
- Exactly three debug modes remain.
- No runtime or compute-field code changes.
- Unity compilation and both Scene-view modes remain pending until tested in Unity.

### Performance

- Runtime gameplay cost: unchanged.
- Editor Target mode: existing CPU procedural samples and handle drawing only.
- Editor Response mode: existing asynchronous readback, now explicitly converted to `RGBAFloat`; approximately 256 KiB per 128 × 128 readback at the existing 10 Hz maximum editor cadence.
- No new textures, buffers, dispatches, draw calls, or runtime overlays.

### WEATHER-WIND-V0.1.2 post-implementation audit

**Status:** Source implementation complete; Unity compilation and visual validation pending.

#### Actual changed files

- `Assets/Game/Procedural/Weather/Editor/WeatherWindDomainEditor.cs`
- `Assets/Docs/Weather_Wind_Architecture.md`

The actual scope matches the approved scope. No runtime, compute, HLSL, vegetation, scene, prefab, camera, Ground, River, layer, or tag file changed.

#### Intentional differences from V0.1.1

- Scene rendering is registered through `SceneView.duringSceneGui` with symmetric editor enable/disable subscription rather than relying only on `OnSceneGUI` dispatch.
- Drawing is restricted to the selected Weather-domain GameObject.
- Handle depth testing is temporarily set to `CompareFunction.Always` and restored afterward, so ground and grass cannot hide the diagnostic overlay.
- A compact GUI legend identifies the active view and arrow encoding.
- Response readback explicitly converts the render texture to `TextureFormat.RGBAFloat` before `Color[]` interpretation.

#### Preserved behavior

- Debug modes remain exactly `Off`, `Target Wind`, and `Response Wind`.
- Arrow sampling density, height offset, scale, target sampling, response sampling, field bounds, and anchor resolution remain controlled by the existing Weather domain.
- Runtime Weather field generation, spring simulation, toroidal scrolling, vegetation sampling, gameplay CPU sampling, and resource budgets are unchanged.

#### Source validation

- Exact two-file diff confirmed.
- Both wind-vector lambdas retain fully explicit parameter declarations and explicit `out Vector2` parameters.
- `SceneView.duringSceneGui` subscription and unsubscription are symmetric.
- `Handles.zTest` is restored in a `finally` block.
- Response readback uses explicit `RGBAFloat` conversion and still remains editor-only.
- C# delimiters, comments, and string literals pass local structural checks.
- Unity assemblies are unavailable in the patch environment; Unity C# compilation and Scene-view rendering remain pending.

---

## WEATHER-WIND-V0.1.3 — Distinct Response Error View and Non-Resetting Debug Switching

**Status:** Source implementation complete; Unity compilation and live visual validation pending.

### Objective

- Replace the diagnostically redundant `Response Wind` view with `Response Error` while retaining the three-mode limit.
- Preserve the current wind simulation state, simulation time, response textures, and spring velocity when changing any debug visualization setting, including switching to or from `Off`.
- Keep the authoritative Weather field, response simulation, vegetation sampling, compute kernels, and gameplay wind contract unchanged.

### Reviewed evidence

| Evidence | Finding | Status |
| --- | --- | --- |
| User comparison | `Target Wind` and `Response Wind` look and behave effectively identically. | Reviewed |
| `WeatherWindDomainEditor.DrawTargetWind` | Target arrows are normalized by `MaximumWindStrength`. | Reviewed |
| `WeatherWindDomainEditor.DrawResponseWind` | Response arrows are normalized by `MaximumVisualBendMetres`. | Reviewed |
| `CS_WeatherWindField.compute::TargetWindToVisualBend` | Expected equilibrium response is `targetWind × (maximumVisualBendMetres / maximumWindStrength)`, so independent normalization cancels the visual scale difference at equilibrium. | Reviewed |
| `WeatherWindDomainEditor.OnInspectorGUI` | Every Inspector change calls `RequestRebuild`, including debug-mode, step, height, and arrow-scale changes. | Reviewed |
| `WeatherWindDomain.OnValidate` | Every serialized change sets `resourcesDirty = true`, so changing debug-only fields recreates the field and resets simulation state. | Reviewed |
| Repository metadata | Supplied project source has no `.git` directory; branch, HEAD, working-tree diff, and history remain unavailable. | Limitation recorded |

### Approved files

Modify only:

- `Assets/Game/Procedural/Weather/WeatherWindDomain.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherWindDomainEditor.cs`
- `Assets/Docs/Weather_Wind_Architecture.md`

### Invariants and non-goals

- Keep exactly three debug modes: `Off`, `Wind Field`, and `Response Error`.
- Preserve enum numeric values so existing serialized `Target Wind` and `Response Wind` selections migrate respectively to `Wind Field` and `Response Error` without scene edits.
- `Response Error` means `actual response bend - expected equilibrium bend` in visual-bend metres.
- Debug-only changes must not set `resourcesDirty`, recreate render textures, reset `simulationTime`, clear spring velocity, or alter the published field.
- Simulation-affecting Inspector changes may continue to rebuild the field.
- No compute, HLSL, vegetation, scene, prefab, camera, URP, Ground, River, layer, or tag changes.

### File-by-file implementation sequence

| Item | File | Required result | Status |
| --- | --- | --- | --- |
| W0.1.3-A | `Weather_Wind_Architecture.md` | Record this evidence, scope, invariants, implementation sequence, and validation plan before source edits. | Complete |
| W0.1.3-B | `WeatherWindDomain.cs` | Rename debug enum labels without changing numeric values; expose response-error sampling; distinguish simulation configuration from debug-only configuration in `OnValidate`. | Complete |
| W0.1.3-C | `WeatherWindDomainEditor.cs` | Rebuild only when simulation configuration changes; render `Wind Field` and `Response Error`; update help and legend text. | Complete |
| W0.1.3-D | All approved files | Run exact-scope comparison, syntax checks, enum-value checks, state-reset-path audit, and record Unity validation as pending. | Complete at source level; Unity validation pending |

### Acceptance criteria

- Debug modes are exactly `Off`, `Wind Field`, and `Response Error`.
- `Wind Field` displays the authoritative target vector.
- `Response Error` displays only the spring deviation from the expected target bend; zero-length arrows mean the response has caught up.
- Switching modes or changing debug step, height, or arrow scale preserves simulation time and existing response texture state.
- Changing field resolution, cell size, wind generation, response, anchor, or mapping settings still requests a rebuild when required.
- Final diff contains only the three approved files.

### Validation plan

- Static C# structural and delegate/lambda checks.
- Verify enum numeric values remain `0`, `1`, and `2`.
- Verify debug fields are excluded from the simulation-configuration hash.
- Verify editor rebuild decisions compare simulation hashes before and after Inspector editing.
- Verify response error uses the same target-to-bend conversion as the compute shader.
- Unity compilation and live state-preservation comparison remain pending until user validation.


### WEATHER-WIND-V0.1.3 post-implementation consistency and compliance audit

**Actual changed files**

- `Assets/Game/Procedural/Weather/WeatherWindDomain.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherWindDomainEditor.cs`
- `Assets/Docs/Weather_Wind_Architecture.md`

The actual scope exactly matches the approved scope.

**Intentional implementation differences**

- Debug enum values remain `Off = 0`, `WindField = 1`, and `ResponseError = 2`; existing serialized numeric selections therefore migrate without scene edits.
- `Response Error` is calculated as `actual response bend - expected equilibrium bend`. The expected bend uses the same `maximumVisualBendMetres / maximumWindStrength` conversion as `CS_WeatherWindField.compute::TargetWindToVisualBend`.
- The response readback records the simulation timestamp associated with the request, so the CPU target comparison uses the matching procedural wind time rather than a later editor frame.
- Error arrows use a distinct magenta range and a diagnostic normalization of 35% of maximum visual bend, making ordinary lag and overshoot visible without adding another control or debug mode.
- `OnValidate` now compares a hash of simulation-affecting configuration only. Debug mode, sample step, height offset, and arrow scale are excluded.
- The custom editor compares the simulation hash before and after Inspector editing and calls `RequestRebuild` only when that hash changes. Switching to or from `Off`, `Wind Field`, or `Response Error` therefore does not recreate textures or reset simulation time and spring velocity.

**Preserved behavior**

- The Weather target field, compute kernels, 16 Hz default response simulation, toroidal recentering, CPU gameplay sampling, vegetation field sampling, and resource formats are unchanged.
- The explicit `Reset Weather Wind Field` action still intentionally resets the simulation.
- Simulation-affecting Inspector changes still mark the field dirty and rebuild it.
- Debug rendering remains Scene-view-only and selected-domain-only.
- The view count remains exactly three.

**Source validation**

- Exact three-file scope comparison passed.
- C# lexical state, delimiter, string, comment, and invalid-control-character checks passed for both changed source files.
- Debug enum names and numeric values passed static verification.
- All `VectorSampler` lambdas retain fully explicit parameter declarations and required `out Vector2` parameters.
- Static configuration audit confirmed all simulation fields are included in the configuration hash and all four debug-only fields are excluded.
- Static reset-path audit confirmed debug-only Inspector changes do not call `RequestRebuild` and do not set `resourcesDirty` through `OnValidate`.
- Response-error formula parity with the compute shader passed.
- No compute, HLSL, vegetation, scene, prefab, camera, URP, Ground, River, layer, tag, or metadata file changed.
- Unity 6000.5.0f1 compilation and live state-preservation comparison remain pending because Unity assemblies and the project editor are unavailable in this environment.

**Performance**

- Active gameplay CPU/GPU cost is unchanged.
- Inspector simulation-hash comparison and response-error target sampling are editor-only.
- `Response Error` reuses the existing asynchronous response-texture readback and adds only procedural target evaluation for the displayed arrow samples.
- No textures, buffers, compute dispatches, runtime draw calls, instance data, or persistent memory were added.


## VEG-V1D — Continuous vegetation detail consumer time

**Status: vegetation include implementation and source audit complete; Unity shader import, live visual validation, and profiling pending.**

### Objective

Clarify and implement the vegetation consumer's use of the existing Weather timing vector so small calm/detail sway animates continuously between fixed 16 Hz response-field updates without changing Weather ownership, simulation cadence, response integration, resources, or gameplay sampling.

### Approved scope

This shared update modifies only:

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Game/Rendering/Vegetation/Includes/VegetationWindResponse.hlsl`

`WeatherWindDomain.cs`, `WeatherWindField.hlsl`, and `CS_WeatherWindField.compute` are reviewed dependencies and remain unchanged.

### Existing timing contract

`WeatherWindDomain.PublishShaderGlobals` publishes:

```text
_WeatherWindFieldTiming.x = completed fixed-step simulation time
_WeatherWindFieldTiming.y = residual accumulator since the latest fixed step, clamped to one fixed step
_WeatherWindFieldTiming.z = fixed-step duration
_WeatherWindFieldTiming.w = maximum visual bend metres
```

The response sampler already uses `.y` as the per-render prediction interval:

```text
predictedBend = storedBend + storedVelocity × min(accumulator, fixedStep)
```

VEG-V1D additionally defines the continuous visual-consumer clock:

```text
continuous Weather consumer time = max(0, timing.x + timing.y)
```

Across a normal fixed update, time moves from the accumulator into completed simulation time, so the sum is invariant:

```text
S + (h + r) = (S + h) + r
```

This clock is for cheap vegetation-local oscillator phase only. It is not authoritative Weather state and does not alter CPU gameplay wind sampling or the GPU target/response field.

### Vegetation detail response contract

- Calm back-and-forth and lateral detail remain active while the Weather field is active.
- The approved calm detail-energy floor is `0.078`.
- Wind-driven detail energy remains `saturate(bendMagnitude × 1.35)`.
- Final detail energy is the greater of the calm floor and wind-driven energy.
- Fixed-step response velocity no longer directly modulates the micro-detail amplitude.
- Response velocity remains stored and remains required for spring integration and render-time macro-bend prediction.
- Detail frequency, phase variation, displacement coefficients, field sampling, and root-to-tip weighting remain unchanged.

### Performance and resource result

- Weather update cadence remains `16 Hz` by default.
- Compute dispatch count and kernel work are unchanged.
- Target and response texture formats/counts are unchanged.
- Vegetation retains one response-field sample per vertex.
- Vegetation retains two sine evaluations per vertex.
- No CPU upload, shader global, texture, buffer, draw call, or serialized field is added.
- The vegetation include removes one response-velocity vector length and replaces it with one scalar time addition and one scalar maximum. Expected cost is neutral to marginally lower; target-GPU profiling remains pending.

### Acceptance and validation

- Continuous detail phase must use timing `.x + .y`.
- Macro prediction must remain in `WeatherWindField.hlsl` and continue using stored response velocity.
- Weather reset/rebuild continues to reset both the fixed simulation and its visual-consumer phase together.
- No Weather runtime/editor/compute file may change in VEG-V1D.
- Unity shader import, live visual comparison, and performance measurement remain pending until applied in Unity 6000.5.0f1.


### VEG-V1D implementation result

- `VegetationWindResponse.hlsl` now uses `max(0, timing.x + timing.y)` for vegetation-local detail phase.
- Calm detail energy is preserved with a `0.078` floor; wind-driven energy remains bend-based and saturates at `1`.
- Fixed-step response velocity no longer directly changes micro-detail amplitude, but it remains fully active in response integration and macro-bend prediction.
- No Weather runtime, editor, compute, resource, cadence, target-field, response-field, or gameplay-query code changed.
- Source scope, timing-contract, formula, and lexical checks passed. Unity shader import and visual validation remain pending.

---

## WEATHER-V0A — Scene-Owned Weather Root and Legacy Wind Migration

### Status

**WEATHER-V0A accepted and frozen on 2026-07-21.** User validation reported one READY and published scene-owned domain, exactly one active Weather domain, the migrated 128 × 128 field at 0.5 m/cell and 10 Hz, active compute simulation, intact CPU sampling, and intact future wind-line consumer availability. The legacy provider remains disabled rollback data until the later cleanup stage.

### Objective and acceptance criteria

The target scene ownership is:

```text
Scene
└── Systems
    ├── Weather                         [WeatherWindDomain]
    └── Diagnostics
        └── Vegetation Benchmark        [VegetationBenchmarkRunner]
```

The update is accepted only when:

1. One explicit Inspector action creates or reuses `Systems/Weather`, adds or reuses an exact `WeatherWindDomain`, copies all 27 serialized wind-domain configuration properties, disables but does not delete the legacy `VegetationBenchmarkWindProvider`, and reports the transaction.
2. The destination component contains the same anchor, camera, field, prevailing-wind, noise, gust, response, mapping, and debug configuration as the legacy source.
3. The migration preserves whether the legacy provider was actively publishing. An active source produces one active destination; an inactive source produces a disabled destination and an explicit report warning rather than silently enabling wind.
4. The migration is idempotent. A previously migrated `Systems/Weather` destination is reported without creating duplicate roots or domains.
5. The transaction uses one Undo group and reverts on exceptions. The legacy source remains present for rollback.
6. `WeatherWindDomain.cs`, compute/HLSL resources, vegetation renderer code, scenes, prefabs, materials, layers, tags, and URP assets remain unchanged.

### Approved file scope

Create:

- `Assets/Game/Procedural/Weather/Editor/WeatherWindInfrastructureMigration.cs` and `.meta`

Modify:

- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Docs/Weather_System_Architecture_Provisional.md`
- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Weather/Editor/WeatherWindDomainEditor.cs`

No other project file is authorized.

### Read-only review evidence

- `Assets/AGENTS.md` was read completely. It requires this persistent plan as the first project write, exact file scope, no raw scene editing, source and dependency rereads, and pending Unity checks where Unity is unavailable.
- The supplied current workspace has no `.git` directory. Branch, `HEAD`, status, diff, and history are unavailable. `WeatherWindDomain.cs`, `WeatherWindDomainEditor.cs`, and `VegetationBenchmarkWindProvider.cs` are byte-identical between the supplied INFRA.1B baseline and current workspace before this update.
- `WeatherWindDomain` owns 27 serialized configuration properties and all wind runtime resources. Its `OnEnable`, `OnDisable`, and `Update` contracts select one published domain and release/reassign global resources when ownership changes. No runtime edit is required.
- `VegetationBenchmarkWindProvider` is a hidden, obsolete subclass with no behavior of its own. Its serialized fields are inherited directly from `WeatherWindDomain`, so an editor-only explicit property copy can preserve the complete configuration without retaining vegetation ownership.
- `WeatherWindDomainEditor` already detects the exact legacy provider type and warns that it should be replaced. It is the narrowest existing Inspector surface for the migration action.
- The supplied `VisualFrameworkDemo.unity` contains the legacy provider on `VegetationTest` with all 27 serialized fields. The source scene object is inactive in the supplied archive, so migration must preserve `isActiveAndEnabled` rather than assuming the source currently publishes wind. The scene remains read-only; the hierarchy changes only when the user invokes the Editor action.
- INFRA.1B already creates or reuses the scene root `Systems` and direct child `Diagnostics`. WEATHER-V0A must reuse that root and create a direct `Weather` sibling without depending on vegetation migration code.
- Vegetation consumers query only the static `WeatherWindDomain.ActiveDomainCount`, `PublishedDomain`, shader globals, and CPU sample contract. They do not depend on the legacy subclass type.

### Explicit serialized-property migration allowlist

```text
fieldAnchor
targetCamera
fieldPlaneY
fieldResolution
cellSizeMetres
updateRateHz
maximumStepsPerFrame
prevailingDirection
baseStrength
broadNoiseScaleMetres
broadNoiseTravelSpeed
turbulenceStrength
gustNoiseScaleMetres
gustTravelSpeed
gustStrength
gustThreshold
gustSoftness
seed
responseFrequencyHz
responseDampingRatio
responseVariation
maximumWindStrength
maximumVisualBendMetres
debugView
debugSampleStepCells
debugHeightOffset
debugArrowScale
```

This list contains all 27 current serialized configuration properties. Runtime textures, compute handles, ring offsets, simulation state, counters, error strings, static domain ownership, and reports are not serialized and are not copied.

### Migration transaction

1. Validate that the selected source is the exact legacy provider type, belongs to a loaded scene, and is not a persistent asset.
2. Inspect all loaded `WeatherWindDomain` components without sorting. Reject ambiguous active domains or an occupied `Systems/Weather` hierarchy that cannot safely receive the exact base component.
3. Begin one Undo group.
4. Create or reuse the scene-root `Systems` object and its direct `Weather` child; normalize only newly created organizational transforms.
5. Create or reuse an exact `WeatherWindDomain` destination. Keep a newly created destination inactive while configuration is copied so the legacy source remains the sole publisher during setup.
6. Copy the explicit 27-property allowlist through `SerializedObject`, apply it without Undo duplication, and validate every property and type.
7. Record whether the source was active and whether it was the published domain, disable the legacy source, request the destination rebuild, then enable the destination only when the source was active.
8. Build and copy a report containing source/destination paths, copied-property count, active-state preservation, active-domain count, published-domain result, and rollback statement.
9. Collapse the Undo group. On exception, revert the complete group and report the failure.

A disabled source remains disabled at the destination. This prevents the migration from silently changing a scene that currently has no active wind. The destination Inspector can then be enabled deliberately.

### Runtime and performance model

- This update adds no runtime class, frame loop, serialized runtime field, shader global, texture, buffer, compute dispatch, draw call, or gameplay query.
- The active destination uses the existing `WeatherWindDomain` implementation and therefore retains the current 128² default field, approximately 320 KiB persistent texture budget, and fixed-cadence simulation cost.
- During explicit Editor migration, a temporary second component may exist while inactive. No second active publisher or persistent duplicate field is permitted after the transaction.
- The disabled legacy component retains serialized rollback data but allocates no active Weather resources.

No `PERFORMANCE EXCEPTION` is active.

### File-by-file implementation sequence

| ID | File | Work | Status |
| --- | --- | --- | --- |
| WEATHER-V0A.0 | `Weather_Wind_Architecture.md` | Record evidence, exact scope, property allowlist, migration transaction, performance, and validation before code changes. | Complete |
| WEATHER-V0A.1 | `WeatherWindInfrastructureMigration.cs` | Implement preflight, hierarchy creation/reuse, explicit property copy, active-state preservation, Undo rollback, idempotence, and clipboard report. | Complete at source level; Unity validation pending |
| WEATHER-V0A.2 | `WeatherWindDomainEditor.cs` | Replace the passive legacy warning with migration status and one explicit migration button while preserving all current debug/editor behavior. | Complete at source level; Unity validation pending |
| WEATHER-V0A.3 | Weather and vegetation architecture documents | Freeze INFRA.1B from user evidence and record scene-owned Weather ownership and temporary disabled-shim rollback state. | Complete |
| WEATHER-V0A.4 | Approved files and frozen dependencies | Run exact-scope, parse, property-list, hierarchy, Undo, active-state, and byte-identity checks; record Unity validation as pending. | Complete at source level; Unity validation pending |

### Validation plan

Static/source validation must confirm:

- exact two-created / five-modified / zero-deleted scope;
- C# lexical, delimiter, preprocessor, and Tree-sitter parse success for both changed editor sources;
- the migration allowlist equals all 27 serialized fields in `WeatherWindDomain` exactly once;
- the migration requires the exact legacy subclass, uses no sorted object search, creates only `Systems/Weather`, and contains one Undo rollback path;
- source active state controls destination enabled state and the source is disabled, not destroyed;
- `WeatherWindDomain.cs`, the compatibility shim, compute/HLSL, vegetation runtime/editor consumers, scene, prefab, material, URP, layer, and tag files are byte-identical to the pre-change baseline.

Unity validation is limited to migration, hierarchy/state inspection, wind report comparison, Undo, and vegetation motion. Unity compilation and live behavior remain pending until the patch is imported into Unity 6000.5.0f1.

### Implementation result and post-change consistency audit

#### Actual affected files

Created exactly as declared:

- `Assets/Game/Procedural/Weather/Editor/WeatherWindInfrastructureMigration.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherWindInfrastructureMigration.cs.meta`

Modified exactly as declared:

- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Docs/Weather_System_Architecture_Provisional.md`
- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md`
- `Assets/Docs/Stylized_Vegetation_Architecture.md`
- `Assets/Game/Procedural/Weather/Editor/WeatherWindDomainEditor.cs`

Deleted, moved, renamed, scene, prefab, material, shader, compute, runtime Weather, vegetation runtime, layer, tag, and URP files: none.

#### Implemented behavior

- The legacy provider Inspector now exposes one explicit `Migrate Legacy Wind Provider to Systems/Weather` action.
- The migration requires the exact compatibility subclass and one loaded scene source. It rejects duplicate `Systems` roots, duplicate `Weather` children, occupied destination objects, additional same-scene domains, and additional active domains in other loaded scenes.
- The action creates or reuses the scene-root `Systems` object and direct `Weather` child, creates or reuses an exact `WeatherWindDomain`, and copies the complete explicit 27-property configuration allowlist through `SerializedObject`.
- Newly created destinations remain inactive during configuration. The legacy source is disabled, not destroyed. `RequestRebuild` occurs before the destination is enabled, preventing an avoidable second field initialization.
- Destination enablement follows the legacy source's actual active state. The report separately records whether the source was the published static domain. An inactive source produces a disabled destination and an explicit warning.
- Repeated invocation after migration returns an already-migrated report without overwriting the destination or creating duplicate hierarchy objects.
- Both preflight failures and transactional failures copy a complete failure report. Transactional exceptions revert the single Undo group.
- INFRA.1B is frozen in the vegetation ledgers from the user's accepted two-layer inventory evidence.

#### Source validation

`WEATHER-V0A_Source_Validation.txt` reports **43/43 passed**:

- exact two-created / five-modified / zero-deleted scope;
- lexical delimiter and preprocessor balance for both changed editor sources;
- Tree-sitter C# parse with zero error or missing nodes for both changed editor sources;
- valid unique new `.meta` GUID;
- all 27 serialized `WeatherWindDomain` properties matched exactly and appeared once in the migration allowlist;
- exact legacy-type requirement, unsorted domain discovery, one `Systems/Weather` hierarchy, one Undo rollback path, disabled-not-destroyed source, active-state preservation, idempotence, exact base destination, clipboard reporting, and Inspector action;
- existing Weather debug/reset/report actions preserved;
- `WeatherWindDomain.cs`, the compatibility shim, vegetation runtime and editor consumers, Weather HLSL/compute, vegetation wind HLSL, and `VisualFrameworkDemo.unity` byte-identical to the pre-change baseline.

The complete final versions of both changed editor sources, the unchanged Weather runtime producer, compatibility shim, vegetation consumer status paths, canonical Weather documents, INFRA.1B freeze section, and source scene evidence were reread after implementation. The final diff matches every approved plan item and contains no undeclared change.

A Unity executable, Unity reference assemblies, and a C# compiler against Unity assemblies are unavailable in this environment. Unity import/compilation, `SerializedObject` field transfer in the Editor, active-domain handoff, hierarchy Undo/redo, exact source/destination report comparison, and visible vegetation motion are therefore **pending user validation**.

#### Performance reconciliation

- Active gameplay code is byte-identical. The destination uses the existing wind field, textures, fixed-cadence compute work, shader globals, and CPU sample contract.
- The new code is Editor-only and has no frame loop.
- Explicit migration performs one 27-property copy and scene hierarchy transaction. A newly created component is inactive during setup, so it does not allocate a second active field.
- The disabled compatibility source retains serialized rollback data and contributes no active field resources.

**Status:** frozen from user validation. The accepted report recorded `Status: READY`, `Published domain: Yes`, `Active Weather domains: 1`, 327,680 estimated texture bytes, and active simulation dispatches.



## WEATHER-WIND-V0A.1 — Unsigned Power-of-Two Toroidal Wrap

### Status

**Implemented at source level on 2026-07-21; Unity D3D11 validation pending.** This correction is delivered inside `VEG-V2-INFRA.2` because the accepted WEATHER-V0A domain exposed a D3D11 shader warning in all three compute kernels.

### User evidence

Unity emitted the same warning for `InitializeField`, `RecenterField`, and `SimulateField`:

```text
integer modulus may be much slower, try using uints if possible
CS_WeatherWindField.compute(85) (on d3d11)
```

The common source was `PhysicalCell(int2 logicalCell)`, which used signed `%` for toroidal texture addressing.

### Corrected contract

`WeatherWindDomain.OnValidate()` normalizes the scalar field resolution with `Mathf.ClosestPowerOfTwo` and clamps it to 32–256. `SetCommonComputeParameters()` publishes equal positive resolution components and the normalized non-negative ring offset. Under that invariant, positive modulo and a power-of-two bit mask are exactly equivalent:

```text
(value % resolution) == (value & (resolution - 1))
```

`PhysicalCell` now converts the logical cell, ring offset, and resolution to `uint2`, adds them, applies `& (resolution - 1u)`, and converts the wrapped coordinate back to `int2`. Signed integer modulus is removed completely.

### Cross-subsystem impact audit

- `WeatherWindDomain.cs` is unchanged; resolution normalization, recenter offset updates, dispatch count, texture allocation, and simulation cadence are unchanged.
- `InitializeField`, `RecenterField`, and `SimulateField` still call one shared `PhysicalCell` helper and write the same physical texels.
- `WeatherWindField.hlsl` and `VegetationWindResponse.hlsl` are byte-identical. Consumer UV mapping, prediction, bend clamping, and vegetation response are unchanged.
- No texture format, buffer, shader global, CPU gameplay query, noise input, response equation, or dispatch count changed.

Exhaustive source validation compared modulo and bit-mask results for every valid logical coordinate and offset at resolutions 32, 64, 128, and 256; all combinations matched. The remaining required evidence is a Unity D3D11 import with no modulus warning and an unchanged READY Weather report.


## WEATHER-V0A Production Ownership — VEG-V2-INFRA.3 Final Cleanup

**Source cleanup complete on 2026-07-21; Unity import validation pending.** The accepted Weather owner remains the exact scene-level `Systems/Weather` `WeatherWindDomain`. `VEG-V2-INFRA.3A` was superseded before application; no live-retirement transaction remains.

`VEG-V2-INFRA.3` deletes the obsolete `VegetationBenchmarkWindProvider` compatibility subclass and the one-time Weather migration utility. The `WeatherWindDomain` runtime producer, compute field, shader globals, CPU queries, report/debug controls, active-state rules, and consumer behavior are unchanged. If the obsolete `VegetationTest` object still exists in the live scene, the user deletes it directly before or immediately after applying this source cleanup.

Final Weather ownership contains no vegetation-specific publisher or migration path. Exactly one active and published `WeatherWindDomain` remains the authoritative contract for vegetation and future Weather consumers.

---

## WEATHER-WIND-TRAILS-V0.0 — Canonical trail architecture and Weather cadence baseline

**Status:** Cadence source change and provisional-document reconciliation complete at source level on 2026-07-22; Unity import/compilation and Inspector validation pending. Wind-trail runtime, shader, editor diagnostics, and scene setup remain unimplemented.

### Objective and acceptance criteria

Record the approved V0 architecture for Weather-owned stylized wind trails before implementation, align the current Weather cadence control with the accepted scene baseline, and reconcile the provisional Weather diagnostics text with the implementation.

This patch passes when:

- `WeatherWindDomain` exposes `Update Rate Hz` over `5–60 Hz` with `10 Hz` as the new-component baseline;
- the existing serialized `VisualFrameworkDemo` value of `10 Hz` remains untouched;
- the current Inspector debug modes remain exactly `Off`, `Wind Field`, and `Response Error`;
- the provisional Weather document describes those implemented diagnostics accurately;
- no trail runtime component, shader, scene component, material asset, Ground dependency, renderer feature, layer, tag, or package is introduced in this patch.

### User-approved architectural decisions

- Weather remains the sole authoritative wind owner. Trails consume `WeatherWindDomain.SampleTargetWindXZ` and never consume vegetation response bend or velocity as authoritative flow.
- V0 trail height uses the Weather field anchor Y plus a configurable altitude range and shallow deterministic vertical deviation. V0 has no `GeneratedGround` dependency.
- The future trail renderer is attached directly to the existing `Systems/Weather` object. It resolves its co-located domain once with `GetComponent<WeatherWindDomain>()`, uses `domain.TargetCamera`, and uses `domain.FieldAnchor`/resolved anchor state. It does not expose redundant Weather-domain or camera assignments.
- The future renderer runs only while its co-located domain exists, is active and enabled, has ready resources, and equals `WeatherWindDomain.PublishedDomain`.
- V0 uses one hidden runtime material created from a serialized Shader reference with `HideFlags.HideAndDontSave`. No `.mat` asset or Weather Materials folder is created. The serialized Shader reference provides build retention; `Shader.Find` alone is not the contract.
- Production rendering uses one fixed-capacity combined camera-facing ribbon mesh and one transparent URP pass. Trail centreline geometry is generated only when a slot changes; head/tail travel and fading are shader-side.
- Spawn selection uses bounded strong-wind and spacing scoring over a deterministic jittered candidate lattice. Streamlines use one captured `SimulationTime` and repeated explicit-time CPU target samples, with midpoint/RK2 integration and bounded early termination.
- The current debug modes and Inspector option names remain unchanged.

### Reviewed evidence before the first write

- `Assets/AGENTS.md` was read completely. It requires a read-only dependency review, this persistent canonical plan as the first write, exact scope, implementation strictly from the plan, and a post-change audit.
- `Assets/Docs/Weather_Wind_Trails_Implementation_Handoff.md` was reviewed as the continuation proposal. Its core target/response separation and combined-ribbon recommendation remain valid, while the Ground dependency and material-asset proposal are superseded by the user-approved decisions above.
- `Assets/Game/Procedural/Weather/WeatherWindDomain.cs` was read completely. `updateRateHz` is serialized under `Field Resolution`, currently declares `Range(8f, 30f)`, defaults to `16f`, is clamped to `8f–30f` in `OnValidate`, participates in `SimulationConfigurationHash`, drives the fixed step in `Update` and `PublishShaderGlobals`, and is included in the comprehensive report.
- `Assets/Game/Procedural/Weather/Editor/WeatherWindDomainEditor.cs` was read completely. It uses `DrawDefaultInspector`, so changing the serialized field's `Range` exposes the approved range without a custom control or editor modification. It currently implements `Off`, `Wind Field`, and `Response Error` and rebuilds only when the simulation hash changes.
- `Assets/Game/Rendering/Weather/Resources/PS3DWeather/Compute/CS_WeatherWindField.compute`, `Assets/Game/Rendering/Weather/Includes/WeatherWindField.hlsl`, and `Assets/Game/Rendering/Vegetation/Includes/VegetationWindResponse.hlsl` were read. They consume the published fixed-step timing contract and require no cadence-specific source change.
- `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity` was inspected read-only. The accepted `Systems/Weather` domain serializes `fieldResolution: 128`, `cellSizeMetres: 0.5`, and `updateRateHz: 10`; this patch does not edit the scene.
- The supplied project snapshot contains no `.git` directory. Branch, `HEAD`, status, diff, and history cannot be independently verified from this archive and are not represented as current evidence.

### Approved files for this patch

Modify:

- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherWindDomain.cs`
- `Assets/Docs/Weather_System_Architecture_Provisional.md`

Create, delete, move, rename, generate, metadata, scene, prefab, material, shader, compute, HLSL, vegetation, Ground, River, URP, layer, and tag changes: none.

### File-by-file implementation sequence

| Item | File | Required result | Status |
| --- | --- | --- | --- |
| WTL-V0.0-A | `Weather_Wind_Architecture.md` | Record this objective, evidence, exact scope, approved architecture, cadence decision, risks, validation, and statuses before source edits. | Complete |
| WTL-V0.0-B | `WeatherWindDomain.cs` | Change only the serialized cadence range and default plus its matching `OnValidate` clamp: `5–60 Hz`, default `10 Hz`. Preserve timing, catch-up, hashing, resource, report, and consumer contracts. | Complete at source level; Unity validation pending |
| WTL-V0.0-C | `Weather_System_Architecture_Provisional.md` | Replace stale `Target Wind` / `Response Wind` wording with the implemented `Wind Field` / `Response Error` definitions and record the configurable 5–60 Hz cadence with 10 Hz baseline. | Complete |
| WTL-V0.0-D | All approved files and frozen dependencies | Reconcile exact scope, reread complete final files and affected contracts, run structural/static checks, and record Unity validation honestly. | Complete at source level; Unity validation pending |

### Cadence contract and performance

The approved serialized range is `5–60 Hz`; the default for newly added or reset component data is `10 Hz`. Existing scenes retain their serialized value. The current demo scene already uses `10 Hz`, so no scene migration or scene write is required.

For a 128 × 128 field, simulation-cell evaluations per second are:

```text
5 Hz  = 81,920 cell updates/s
10 Hz = 163,840 cell updates/s
16 Hz = 262,144 cell updates/s
30 Hz = 491,520 cell updates/s
60 Hz = 983,040 cell updates/s
```

This excludes recenter dispatches and per-cell spring substeps. The range permits explicit quality/performance testing; `10 Hz` remains the baseline because it is the accepted scene state and continuous render-time response prediction already exists in `WeatherWindField.hlsl`. `60 Hz` is a ceiling, not a recommended default. No performance exception is approved.

### Prospective wind-trail implementation scope after this patch

The next approved implementation must be separately declared from the final current source state. The intended V0 files are:

Create:

- `Assets/Game/Procedural/Weather/WeatherWindTrailRenderer.cs` and `.meta`
- `Assets/Game/Procedural/Weather/Editor/WeatherWindTrailRendererEditor.cs` and `.meta`
- `Assets/Game/Rendering/Weather/Shaders/SH_WeatherWindTrails.shader` and `.meta`

Modify through Unity only:

- `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity` to add exactly one trail renderer to the existing `Systems/Weather` GameObject and serialize the shader reference

Modify for status/evidence:

- `Assets/Docs/Weather_Wind_Architecture.md`

Explicitly excluded unless later evidence and approval expand the plan:

- material asset or Materials folder;
- `GeneratedGround` source or reference;
- `WeatherWindDomain` simulation changes beyond the cadence control in this patch;
- Weather compute/HLSL contracts;
- vegetation source or shaders;
- renderer features, URP assets, layers, tags, packages, or new hierarchy children.

### Risks and validation

- Raising cadence increases full-field compute work approximately linearly. Unity profiling at representative `5`, `10`, `30`, and `60 Hz` values is required before treating the upper range as a production quality tier.
- `maximumStepsPerFrame = 4` remains unchanged. At high configured cadence and low frame rate, the existing bounded catch-up policy can discard accumulated simulation time; this patch intentionally does not redesign timing behavior.
- Unity 6000.5.0f1 import/compilation is unavailable in this archive environment. Required user validation is: confirm the Inspector slider spans `5–60`, the current scene remains `10`, changing the value rebuilds the simulation through the existing hash path, and the Weather report prints the selected rate.

### Post-change consistency and compliance audit

Actual affected files exactly match the approved scope:

- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherWindDomain.cs`
- `Assets/Docs/Weather_System_Architecture_Provisional.md`

Created, deleted, moved, renamed, generated, metadata, scene, prefab, material, shader, compute, HLSL, vegetation, Ground, River, URP, layer, and tag files: none.

Intentional source differences:

- `WeatherWindDomain.updateRateHz` now declares `Range(5f, 60f)` and initializes new component data at `10f` instead of the historical `Range(8f, 30f)` / `16f` default.
- `WeatherWindDomain.OnValidate` now clamps the cadence to `5f–60f`.
- The current architecture summary and current runtime-work section identify `10 Hz` as the baseline and `5–60 Hz` as the Inspector range. Historical sections that accurately describe earlier 16 Hz work remain historical.
- The provisional parent now documents the approved but unimplemented wind-trail V0 architecture, the absence of Ground/material-asset dependencies, the implemented `Wind Field` / `Response Error` diagnostics, and the cadence range/default.

Preserved contracts:

- Both fixed-step calculations remain `1f / Mathf.Max(1f, updateRateHz)`.
- `updateRateHz` remains part of `SimulationConfigurationHash`, so Inspector cadence changes use the existing rebuild path.
- The comprehensive report continues to print the selected rate.
- `maximumStepsPerFrame`, accumulator truncation, simulation time, resource allocation, texture formats, shader-global timing, CPU target sampling, compute kernels, vegetation response, debug enum values, and Scene serialization are unchanged.
- The accepted demo scene remains serialized at `updateRateHz: 10`; no raw scene write occurred.

Static validation passed **28/28** checks: exact source patterns, removal of the old range/default, fixed-step/hash/report preservation, debug enum preservation, C# lexical and delimiter balance, canonical-plan uniqueness, documentation wording, balanced Markdown fences, LF-only content, and byte identity for `WeatherWindDomainEditor.cs`, `CS_WeatherWindField.compute`, `WeatherWindField.hlsl`, `VegetationWindResponse.hlsl`, and `VisualFrameworkDemo.unity`.

The supplied snapshot contains no `.git` directory, Unity executable, Unity reference assemblies, or C# compiler configured against Unity. Git comparison, Unity 6000.5.0f1 import/compilation, live Inspector range, rebuild behavior, and report output are pending and are not represented as passed.


---

## WEATHER-WIND-TRAILS-V0.1 — Runtime placement, streamline, and mesh core

**Status:** Runtime source and metadata implementation complete on 2026-07-22. Exact-scope, structural, dependency-freeze, bounded-allocation, and analytical placement/path checks passed. The user reported Unity import/compilation with no errors after applying V0.1. Live component installation, Play Mode behavior, and allocation profiling remain pending.

### Objective and acceptance criteria

Implement the bounded runtime core for Weather-owned wind trails without rendering, editor tooling, material creation, or scene installation. The new component must resolve the co-located published `WeatherWindDomain`, select camera-relevant strong-wind seeds with active/cooldown separation, build bidirectional midpoint/RK2 streamlines from one captured Weather simulation-time snapshot, prepare one fixed-capacity combined ribbon mesh contract, expire/recycle fixed slots, and expose allocation-on-demand diagnostics for the later custom Inspector.

This patch passes at source level when:

- the component resolves only `GetComponent<WeatherWindDomain>()`, `domain.TargetCamera`, and the domain's resolved anchor state; no hierarchy-wide search or serialized Weather/camera/Ground reference exists;
- runtime operation is gated by a non-null active co-located domain, `domain.ResourcesReady`, and `WeatherWindDomain.PublishedDomain == domain`;
- all candidate, path, trail, cooldown, vertex, and index storage is allocated only during initialization/reconfiguration and reused thereafter;
- each candidate sweep captures one `domain.SimulationTime` and uses only `domain.SampleTargetWindXZ(worldXZ, capturedTime)` for candidate and path decisions;
- accepted seeds satisfy the configured absolute wind floor and active/cooldown separation;
- a deterministic jittered lattice and deterministic weighted choice among the strongest eligible subset avoid rigid placement and absolute-maximum pinning;
- each accepted path is built backward and forward around the seed with midpoint/RK2 integration, bounded point count, field/view/curvature/self-approach termination, minimum path length, and forward tangent-to-target-wind validation;
- active/cooldown state clears when the Weather simulation-configuration hash changes, `SimulationTime` rewinds, or the field origin jumps by at least half the field span on either XZ axis, preventing paths from surviving a rebuild, manual field reset, resource recreation, or large anchor teleport with stale flow provenance;
- Y placement uses only the resolved Weather anchor Y, deterministic altitude, and shallow deterministic vertical deviation;
- one fixed combined mesh reserves two vertices per centreline point and six indices per segment, with cumulative path distance and lifecycle/presentation data required by the later shader;
- no rendering submission, runtime material, shader, custom editor, scene, Ground, vegetation, compute, shared HLSL, URP, layer, tag, package, or hierarchy change occurs.

### Reviewed evidence before this plan write

- `Assets/AGENTS.md` was read completely. It requires this persistent plan as the first modifying action, exact scope, strict plan traceability, bounded runtime work, and a final consistency/compliance audit.
- `Assets/Docs/Weather_Wind_Trails_Implementation_Handoff.md` and `WEATHER-WIND-TRAILS-V0.0` were reread. The approved V0 removes Ground and material-asset dependencies, attaches directly to `Systems/Weather`, and consumes CPU target wind rather than vegetation response.
- `Assets/Game/Procedural/Weather/WeatherWindDomain.cs` was read completely. The required public contracts are `PublishedDomain`, `FieldAnchor`, `TargetCamera`, `ResourcesReady`, `MaximumWindStrength`, `SimulationTime`, `SimulationConfigurationHash`, `GetFieldWorldRectXZ()`, `GetDebugAnchorPosition()`, and `SampleTargetWindXZ(Vector2, float)`.
- `Assets/Game/Procedural/Weather/Editor/WeatherWindDomainEditor.cs` was read completely. Existing Weather diagnostics remain separate and unchanged; the future trail editor must consume runtime diagnostic APIs rather than reconstructing target-wind state.
- `Assets/Game/Rendering/Weather/Includes/WeatherWindField.hlsl`, `Assets/Game/Rendering/Weather/Resources/PS3DWeather/Compute/CS_WeatherWindField.compute`, and `Assets/Game/Rendering/Vegetation/Includes/VegetationWindResponse.hlsl` were read completely. They confirm target/response separation and require no change for CPU runtime placement.
- The supplied source snapshot has no `.git` directory or Unity executable. Git history/status and Unity compilation cannot be produced in this environment and must remain pending.

### Approved files for this patch

Create:

- `Assets/Game/Procedural/Weather/WeatherWindTrailRenderer.cs`
- `Assets/Game/Procedural/Weather/WeatherWindTrailRenderer.cs.meta`

Modify:

- `Assets/Docs/Weather_Wind_Architecture.md`

Create/delete/move/rename/generate beyond the declared C# metadata companion, shader, material, editor, scene, prefab, Ground, vegetation, compute, HLSL, River, URP, renderer-feature, layer, tag, package, or hierarchy changes: none.

### Runtime configuration and bounded budgets

Initial serialized defaults:

```text
Maximum active trails: 8
Candidate lattice: 8 × 8 = 64 candidates
Spawn attempts: 4/s
Maximum candidate sweeps per frame: 2 (fixed internal catch-up cap)
Strongest weighted subset: 6 candidates
Minimum authoritative wind strength: 0.18
Strength score exponent: 2.0
Minimum active/cooldown separation: 6 m
Separation cooldown: 1.5 s
Maximum centreline points: 24
Integration step: 0.5 m
Minimum path wind strength: 0.12
Minimum completed path length: 3.5 m
Maximum turn per segment: 55 degrees
Minimum final tangent/wind alignment: 0.35
Lifetime: 1.5–3.0 s
Width: 0.04–0.10 m
Presentation speed data: 2–5 m/s
Visible tail-length data: 2.5–5.0 m
Altitude above resolved Weather anchor: 1.0–2.5 m
Maximum vertical deviation: 0.15 m
```

At the baseline maximum, storage is 8 × 24 × 2 = 384 vertices and 8 × 23 × 6 = 1,104 unsigned 16-bit indices (368 triangles). At four successful spawns per second, the intended analytical target-wind cost remains hundreds of CPU function evaluations per second rather than a per-frame field scan. The actual count is reported by the runtime component and must be verified in Unity.

### File-by-file implementation sequence

| Item | File | Required result | Status |
| --- | --- | --- | --- |
| WTL-V0.1-A | `Weather_Wind_Architecture.md` | Record objective, exact scope, evidence, defaults, runtime algorithms, budgets, risks, and validation before source creation. | Complete |
| WTL-V0.1-B | `WeatherWindTrailRenderer.cs` | Implement lifecycle/dependency gating, fixed storage, deterministic candidates, strength/separation selection, cooldowns, RK2 paths, expiry/recycling, combined mesh preparation, diagnostics, and profiler markers. No draw or material work. | Complete at source level; Unity validation pending |
| WTL-V0.1-C | `WeatherWindTrailRenderer.cs.meta` | Add one unique Unity C# metadata companion. | Complete |
| WTL-V0.1-D | Approved files and frozen dependencies | Reread final source and contracts, reconcile exact scope, run structural/static checks, record intentional differences and unavailable Unity evidence. | Complete at source level; Unity validation pending |

### Runtime algorithm contract

Candidate work runs only at the configured spawn cadence and only while a free trail slot exists. An `N × N` lattice covers `domain.GetFieldWorldRectXZ()`. Each cell receives deterministic XZ jitter and altitude from the local trail seed, spawn epoch, cell coordinates, and independent hash channels. Candidates behind the camera or outside an expanded viewport are rejected before target-wind sampling. Remaining candidates sample the explicit captured simulation time, reject values below the absolute floor, calculate nearest active/cooldown seed distance, reject values below the minimum, and combine normalized strength and additional spacing preference into one score.

The component retains a fixed top subset without sorting or managed collections. Selection is deterministic weighted randomness over that subset. Every attempted sweep increments the spawn epoch, including failed sweeps, so repeated failures do not evaluate one permanently identical lattice.

The component tracks `domain.SimulationConfigurationHash`, `domain.SimulationTime`, and `domain.FieldOriginXZ` while ready. A configuration-hash change or backwards `SimulationTime` movement clears active and cooldown slots before new sampling; the latter covers `ResetField()` and resource recreation when configuration values are unchanged. Normal toroidal recenter movement preserves active world-space trails; a field-origin jump of at least half the field span on either axis is treated as a large anchor teleport and clears active/cooldown state.

For an accepted seed, the component integrates backward with `-normalize(W)` and forward with `normalize(W)`. Each segment uses midpoint/RK2:

```text
d0 = normalize(W(p, capturedTime))
dm = normalize(W(p + 0.5 × step × d0, capturedTime))
pNext = p + step × dm
```

The backward side is reversed when assembling the final path, so final centreline order remains downwind from tail to head. The completed path is rejected if too short or if any final XZ segment has target-wind alignment below the configured floor. All samples use the captured time.

### Mesh contract for V0.2

The component allocates one dynamic mesh with one interleaved vertex stream and one fixed index buffer. Each point contributes left/right vertices containing:

```text
POSITION: centreline world position
NORMAL: centreline tangent
TEXCOORD0.x: signed half-width
TEXCOORD0.y: cumulative path distance in metres
TEXCOORD1: birth time, inverse lifetime, presentation speed, visible tail length
COLOR: opacity, normalized accepted strength, deterministic variation, active flag
```

Inactive or unused slot vertices collapse to zero position with zero active alpha. Only the affected fixed slot range is uploaded when a trail spawns, expires, or is cleared. Bounds cover the current Weather field rectangle and approved altitude/deviation range. V0.1 performs no draw submission and creates no material.

### Risks and validation

- The analytical CPU path function is continuous while vegetation samples a bilinear GPU cache. Matching procedural inputs are proven by existing Weather architecture; exact float equality is not. Unity Scene diagnostics later compare paths with `Wind Field` arrows.
- Viewport rejection depends on the current camera and altitude. The runtime report records visible, calm, spacing, and path rejection counts so an over-restrictive default is diagnosable.
- Combined transparent-mesh sorting is a V0.2 visual risk, not a V0.1 runtime-core blocker.
- Fixed arrays avoid intentional steady-state managed allocations. Unity Profiler remains required because engine API internals cannot be proven allocation-free by source inspection alone.
- Unity import/compilation passed by user report after V0.1. Component installation, deterministic live replay, visual debug, and Profiler evidence remain pending until the later editor/scene patch makes the runtime component directly testable in the live project.

### Post-change consistency and compliance audit

Actual affected files exactly match the approved V0.1 scope:

Create:

- `Assets/Game/Procedural/Weather/WeatherWindTrailRenderer.cs`
- `Assets/Game/Procedural/Weather/WeatherWindTrailRenderer.cs.meta`

Modify:

- `Assets/Docs/Weather_Wind_Architecture.md`

Created, deleted, moved, renamed, generated, shader, material, custom-editor, scene, prefab, Ground, vegetation, compute, shared-HLSL, River, URP, renderer-feature, layer, tag, package, and hierarchy changes beyond that declaration: none.

Implemented runtime behavior:

- one co-located `WeatherWindDomain` lookup occurs at lifecycle resolution; camera state is read from `domain.TargetCamera` without `Camera.main` or hierarchy searches;
- runtime readiness requires component mesh resources, the active/enabled co-located domain, `domain.ResourcesReady`, `WeatherWindDomain.PublishedDomain == domain`, and a resolved camera;
- a deterministic jittered lattice evaluates only expanded-viewport candidates, samples one captured `SimulationTime`, applies the authoritative wind floor, active/cooldown seed separation, strength/spacing scoring, a fixed top subset, and deterministic weighted choice;
- midpoint/RK2 integration builds backward and forward sides, reverses the backward side into final downwind order, terminates on fixed bounds, and validates every final segment against explicit-time target wind;
- fixed arrays own all trail, cooldown, candidate, scratch, vertex, and index state; array allocations occur only in `EnsureResources()` and are replaced only after configuration dirtiness;
- the fixed mesh uses 16-bit indices and the V0.2 vertex contract. The baseline capacity is 384 vertices, 1,104 indices, and 368 triangles; one slot upload is 48 vertices at the 24-point baseline;
- no material is created and no rendering command is submitted;
- active and cooldown state clears when the source becomes unavailable, the Weather configuration hash changes, Weather simulation time rewinds, or field origin jumps by at least half the field span. Normal toroidal recenter increments preserve active world-space trails.

Static validation passed **59/59** checks after the final lifecycle revision. Checks covered C# delimiter/string/comment balance, exact plan uniqueness and scope, one co-located `GetComponent`, absence of hierarchy/Ground/render/material/LINQ dependencies, published-domain/resource gates, explicit-time target samples, deterministic top-subset logic, active/cooldown separation, RK2 and alignment validation, configuration/time-rewind/teleport resets, fixed mesh formulas and semantics, slot-range upload, profiler/report contracts, allocation confinement, `OnValidate` destruction prohibition, metadata GUID format/uniqueness, LF/trailing-whitespace rules, and byte identity of frozen Weather-domain, provisional-document, Weather editor, compute, Weather HLSL, vegetation HLSL, and scene dependencies.

A separate analytical replay ported the current CPU target-wind/hash equations and current `VisualFrameworkDemo` camera/Weather serialized values. At simulation times `0`, `5`, `10`, `20`, `39.3`, and `60` seconds, the 8 × 8 lattice produced 13 expanded-viewport candidates per snapshot and respectively `8`, `4`, `7`, `2`, `3`, and `8` candidates above the `0.18` floor. All 32 above-floor candidates in that replay produced paths satisfying the source algorithm's `3.5 m` minimum and `0.35` alignment floor; completed lengths were `6.0–11.5 m`. This verifies that the initial values are not analytically self-blocking for the current serialized scene. It does not prove Unity camera API parity, component compilation, runtime allocations, or final visual quality.

Preserved dependencies are byte-identical to the V0.0 patched state or supplied pristine snapshot as applicable:

- `WeatherWindDomain.cs`
- `Weather_System_Architecture_Provisional.md`
- `WeatherWindDomainEditor.cs`
- `CS_WeatherWindField.compute`
- `WeatherWindField.hlsl`
- `VegetationWindResponse.hlsl`
- `VisualFrameworkDemo.unity`

The supplied snapshot contains no `.git` directory, Unity executable, Unity reference assemblies, or configured C# compiler. Git status/history/diff remain unavailable. The user subsequently reported Unity import/compilation with no errors for V0.1; actual live Mesh API behavior, deterministic live replay, zero-GC proof, and Profiler measurements remain pending and are not represented as passed.


---

## WEATHER-WIND-TRAILS-V0.2 — Transparent ribbon rendering and runtime material

**Status:** Runtime rendering source, shader source, and metadata implementation complete at source level on 2026-07-22. Exact-scope, structural, shader-contract, and frozen-dependency checks passed. Unity shader import/compilation, live draw submission, visual validation, and profiling remain pending.

### Objective and acceptance criteria

Add the first rendering implementation for the accepted V0.1 trail mesh without changing trail placement, path construction, Weather simulation, vegetation response, scene ownership, or serialized scene state. The runtime component must create one hidden material from its serialized shader reference, animate the travelling head/tail window shader-side, and submit at most one transparent mesh draw for the resolved Weather target camera while active trails exist.

This patch passes at source level when:

- `WeatherWindTrailRenderer` creates and destroys exactly one `HideFlags.HideAndDontSave` runtime material together with its existing runtime mesh resources;
- rendering uses the existing interleaved world-space mesh contract without changing its vertex stride, attributes, capacity formulas, slot upload policy, or path algorithms;
- the shader expands each centreline vertex into a camera-facing ribbon, applies a travelling head/tail visibility window from cumulative path distance and lifecycle data, produces soft cross-width edges and bounded lifetime fading, and writes premultiplied transparent colour;
- one `Graphics.RenderMesh` submission is issued only in Play Mode, only for the resolved target camera, only while the component/domain/material/mesh are ready, and only while at least one trail is active;
- the pass uses `Cull Off`, `ZWrite Off`, `ZTest LEqual`, `Blend One OneMinusSrcAlpha`, no lighting, no camera depth-texture sample, no opaque-texture sample, no renderer feature, and no material asset;
- the supplied scene, Weather producer/editor/compute/HLSL, vegetation consumer, URP assets, layers, tags, packages, Ground, River, and hierarchy remain unchanged.

### Reviewed evidence before the first write

- `Assets/AGENTS.md` was reread completely. It requires this persistent plan as the first modifying write, exact scope, implementation from the recorded contract, frozen-dependency comparison, and honest Unity validation status.
- `WEATHER-WIND-TRAILS-V0.0`, `WEATHER-WIND-TRAILS-V0.1`, and the complete current `WeatherWindTrailRenderer.cs` were reread. V0.1 already stores world-space centre position, world-space tangent, signed half-width, cumulative path distance, birth time, inverse lifetime, presentation speed, visible tail length, opacity, accepted strength, deterministic variation, and active state in one 52-byte interleaved vertex. No vertex-layout change is required.
- `VegetationRendererBase.SubmitIndirectRender` and `RebuildVegetation` were inspected as current project examples for `RenderParams`, camera-restricted SRP submission, `GetEntityId()`, disabled shadows, and hidden runtime-material lifecycle.
- `SH_StylizedVegetationBenchmark.shader`, the current River and Pixel Surface URP shaders, and the shared URP Core include conventions were inspected for project pass tags and ShaderLab structure.
- Unity's current `Graphics.RenderMesh` scripting contract was checked against the official Unity documentation: one frame submission accepts `RenderParams`, mesh, submesh, and object-to-world transform. The planned world-space mesh uses `Matrix4x4.identity`.
- `PC_RPAsset.asset`, `Mobile_RPAsset.asset`, `PC_Renderer.asset`, and `Mobile_Renderer.asset` were inspected. Mobile does not request camera depth or opaque textures; the V0.2 shader therefore relies only on ordinary `ZTest` and does not sample either texture.
- The supplied project snapshot still contains no `.git` directory, Unity executable, Unity reference assemblies, or configured compiler. Git comparison and Unity import/compile/render evidence remain unavailable in this environment.

### Approved files for this patch

Modify:

- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherWindTrailRenderer.cs`

Create:

- `Assets/Game/Rendering/Weather/Shaders.meta`
- `Assets/Game/Rendering/Weather/Shaders/SH_WeatherWindTrails.shader`
- `Assets/Game/Rendering/Weather/Shaders/SH_WeatherWindTrails.shader.meta`

Delete, move, rename, generate, material, custom-editor, scene, prefab, Ground, vegetation, Weather producer/editor/compute/shared-HLSL, River, URP, renderer-feature, layer, tag, package, and hierarchy changes: none.

### Invariants and non-goals

- `WeatherWindDomain` remains the sole wind producer. Rendering consumes only V0.1-generated trail data and does not resample or alter Weather fields.
- The V0.1 candidate, spacing, cooldown, RK2, reset, expiry, fixed-array, fixed-index, and slot-range-upload contracts remain unchanged.
- World-space vertex positions and tangents remain world-space. The render transform is identity.
- The serialized `trailShader` reference is the build-retention and shader-selection contract. No `Shader.Find`, material asset, Resources load, Addressables dependency, or renderer feature is added.
- Runtime material property changes do not create per-frame `MaterialPropertyBlock`, arrays, strings, collections, or materials.
- V0.2 renders only in Play Mode. Edit-mode and Scene-view rendering belong to the later editor-diagnostics patch.
- Rendering is restricted to `domain.TargetCamera`; no `Camera.main`, hierarchy search, all-camera callback, or additional camera is introduced.
- No soft-particle intersection fade is added because Mobile does not provide a camera depth texture. Ordinary opaque-depth occlusion is the V0 contract.
- No motion vectors, shadows, light probes, lighting, fog, bloom integration, or post-process dependency is introduced.

### File-by-file implementation sequence

| Item | File | Required result | Status |
| --- | --- | --- | --- |
| WTL-V0.2-A | `Weather_Wind_Architecture.md` | Record objective, evidence, exact scope, rendering/material/shader contract, budgets, risks, and validation before implementation. | Complete |
| WTL-V0.2-B | `WeatherWindTrailRenderer.cs` | Add visual controls, shader-property IDs, hidden material lifecycle, render readiness/error reporting, one bounded Play-Mode `Graphics.RenderMesh` submission, and render counters/Profiler marker without changing V0.1 mesh/path algorithms. | Complete at source level; Unity validation pending |
| WTL-V0.2-C | `Shaders.meta`, `SH_WeatherWindTrails.shader`, and `.meta` | Add the approved Weather shader folder metadata and one URP unlit transparent camera-facing ribbon shader matching the existing vertex contract. | Complete at source level; Unity validation pending |
| WTL-V0.2-D | Approved files and frozen dependencies | Reread complete final files, compare exact scope and frozen hashes, run structural/semantic/static checks, and record Unity validation honestly. | Complete at source level; Unity validation pending |

### Runtime material and submission contract

`EnsureResources()` treats the serialized shader and supported runtime material as part of readiness. After the fixed mesh is created, it creates exactly one material:

```text
new Material(trailShader)
name = PS3D Weather Wind Trails Runtime Material
hideFlags = HideFlags.HideAndDontSave
```

The material receives colour, edge softness, head/tail distance softness, lifetime fade fractions, strength-opacity influence, and deterministic-variation influence only when resources/configuration are rebuilt. `_TrailPresentationTime` is the only per-frame material property update.

After expiry, spawning, and bounds maintenance, the component submits only when all of the following hold:

```text
Application.isPlaying
runtime readiness is true
activeTrailCount > 0
trailMesh != null
runtimeMaterial != null
resolvedCamera != null
```

The draw contract is:

```text
RenderParams.camera = resolvedCamera
RenderParams.worldBounds = trailMesh.bounds
RenderParams.layer = gameObject.layer
RenderParams.entityId = gameObject.GetEntityId()
shadowCastingMode = Off
receiveShadows = false
Graphics.RenderMesh(..., submesh 0, Matrix4x4.identity)
```

One call is permitted per active frame. No draw is submitted with zero active trails.

### Shader contract

The shader reads the V0.1 attributes exactly:

```text
POSITION: world-space centreline position
NORMAL: world-space centreline tangent
TEXCOORD0.x: signed half-width in metres
TEXCOORD0.y: cumulative centreline distance in metres
TEXCOORD1: birth time, inverse lifetime, presentation speed, visible tail length
COLOR: opacity, normalized strength, deterministic variation, active flag
```

The vertex stage normalizes the tangent, calculates view direction from the centre to the active camera, calculates a perpendicular ribbon side from tangent and view direction with deterministic fallback axes for near-parallel vectors, offsets by signed half-width, and transforms the resulting world position with `TransformWorldToHClip`.

The fragment stage calculates:

```text
age = presentationTime - birthTime
age01 = age × inverseLifetime
headDistance = max(0, age) × presentationSpeed
tailDistance = headDistance - visibleTailLength
```

It multiplies a soft head gate, soft tail gate, cross-width edge mask, fade-in, fade-out, vertex opacity, active flag, bounded strength influence, bounded variation influence, and `_TrailColor.a`. Output RGB is premultiplied by final alpha and uses `Blend One OneMinusSrcAlpha`.

### Initial visual defaults

```text
Trail colour: slightly cool off-white (0.92, 0.97, 1.00, 1.00)
Cross-width edge softness: 0.35
Head softness: 0.35 m
Tail softness: 0.55 m
Lifetime fade-in fraction: 0.12
Lifetime fade-out fraction: 0.28
Strength-opacity influence: 0.25
Variation-opacity influence: 0.12
```

These are initial calibration values, not visually accepted final values. The later Inspector/scene patch exposes them for capture-based tuning.

### Performance and resource budget

The baseline remains 384 vertices, 1,104 unsigned 16-bit indices, and 368 triangles. The vertex/index memory and slot uploads are unchanged from V0.1. V0.2 adds:

- one hidden runtime `Material` per enabled trail component;
- one material float update per active frame;
- at most one transparent draw submission per active frame and resolved target camera;
- one unlit vertex/fragment pass over the projected ribbon pixels;
- no new texture, buffer, compute dispatch, renderer feature, camera texture, material asset, or per-trail draw.

The dominant unmeasured cost is transparent pixel coverage/overdraw. CPU draw-submission and geometry costs are bounded by one call and the fixed mesh. No performance exception is approved.

### Risks and validation

- Camera-facing cross products can degenerate when tangent and view direction are nearly parallel. The shader must use a bounded fallback axis and static checks must confirm the fallback exists.
- One combined transparent mesh sorts as one renderer. Sparse six-metre placement reduces overlap but does not prove correct crossing order; Unity visual capture is required.
- Bright off-white premultiplied trails can bloom strongly in HDR. V0.2 adds bounded colour/opacity controls but does not tune against a final capture.
- A missing or unsupported serialized shader must produce `NOT READY`, no material, no draw, and a concrete report error without affecting Weather.
- Unity validation must confirm shader import on PC and Mobile URP configurations, one trail draw in Frame Debugger, correct opaque-depth occlusion, no material leak after enable/disable, and zero steady-state `GC.Alloc` after warm-up.


### Post-change consistency and compliance audit

Actual affected files exactly match the approved V0.2 scope.

Modified:

- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherWindTrailRenderer.cs`

Created:

- `Assets/Game/Rendering/Weather/Shaders.meta`
- `Assets/Game/Rendering/Weather/Shaders/SH_WeatherWindTrails.shader`
- `Assets/Game/Rendering/Weather/Shaders/SH_WeatherWindTrails.shader.meta`

Deleted, moved, renamed, generated, material, custom-editor, scene, prefab, Ground, vegetation, Weather producer/editor/compute/shared-HLSL, River, URP, renderer-feature, layer, tag, package, and hierarchy changes beyond that declaration: none.

Implemented runtime behavior:

- the serialized shader is now required and checked for platform support before mesh/material resources are considered ready;
- one `HideFlags.HideAndDontSave` runtime material is created from that shader and destroyed with the existing runtime mesh on disable, destruction, reconfiguration, missing-shader transition, or unsupported-shader transition;
- eight bounded visual controls are serialized on the component and applied to the runtime material only during resource/configuration rebuild; `_TrailPresentationTime` is the only property changed for each submitted frame;
- `SubmitTrailRender()` is called after expiry, domain-state reconciliation, candidate spawning, and bounds maintenance, but submits only in Play Mode with at least one active trail and non-null mesh, material, and resolved target camera;
- one `RenderParams` value restricts rendering to `resolvedCamera`, uses the existing mesh bounds, disables shadows, preserves the component GameObject layer and entity identity, and submits submesh zero with `Matrix4x4.identity` because the mesh stores world-space positions;
- the report now records shader support, runtime-material readiness, Play-Mode target-camera rendering, total render submissions, and the active count in the latest submission;
- V0.1 candidate selection, separation, cooldown, path integration, reset rules, vertex/index formulas, fixed buffers, and slot-range uploads are unchanged.

Implemented shader behavior:

- one URP `UniversalForward` transparent unlit pass uses `Cull Off`, `ZWrite Off`, `ZTest LEqual`, and premultiplied `Blend One OneMinusSrcAlpha`;
- the shader consumes exactly the existing five vertex attributes with no added stream or stride change;
- the vertex stage treats centre position and tangent as world-space data, calculates a camera-facing perpendicular, includes an explicit fallback-axis path for near-parallel tangent/view vectors, offsets by signed half-width, and transforms directly to clip space;
- the fragment stage rejects inactive/expired geometry, moves a soft head/tail window along cumulative path distance, softens cross-width edges with derivatives, applies bounded lifetime, strength, and deterministic-variation opacity, and outputs premultiplied off-white colour;
- the shader does not include lighting, sample `_CameraDepthTexture` or `_CameraOpaqueTexture`, require a renderer feature, or create any texture dependency.

Static validation passed **122/122** checks before the final documentation reconciliation. The checks covered exact file existence and scope, LF/trailing-whitespace hygiene, C# and ShaderLab/HLSL lexical and delimiter balance, one material creation/destruction contract, missing/unsupported shader gates, one Play-Mode/active-trail `Graphics.RenderMesh` call without an explicit argument modifier, target-camera restriction, identity transform, world bounds, disabled shadows, `GetEntityId()`, absence of `Shader.Find`, `Camera.main`, hierarchy, Ground, vegetation, depth-texture, opaque-texture, and lighting dependencies, render-state tokens, camera-facing fallback, premultiplied output, travelling window, lifetime fade, complete C#/shader property parity, unchanged five-attribute/52-byte mesh contract, valid unique folder/shader metadata GUIDs, and canonical-plan path coverage.

SHA-256 comparison confirmed these reviewed dependencies remain byte-identical to the V0.1 input state:

- `WeatherWindDomain.cs`
- `WeatherWindDomainEditor.cs`
- `WeatherWindField.hlsl`
- `CS_WeatherWindField.compute`
- `VegetationWindResponse.hlsl`
- `VisualFrameworkDemo.unity`
- `PC_RPAsset.asset`
- `Mobile_RPAsset.asset`
- `PC_Renderer.asset`
- `Mobile_Renderer.asset`

The first Unity compilation of V0.2 reported `CS9194` at the `Graphics.RenderMesh` call because the installed Unity API metadata exposes the first parameter as a readonly `in` parameter while the project compiles with C# language version 9.0. C# 9 does not permit an explicit `ref` argument for that parameter. The call now passes the local `RenderParams` value without an explicit argument modifier. This preserves the same rendering data and is compatible with the installed compiler/API contract. Unity recompilation, shader import, one-draw Frame Debugger evidence, camera-facing/depth-occlusion visual behavior, transparent sorting, HDR/bloom response, material lifecycle, steady-state allocation, and CPU/GPU timing remain pending and are not represented as passed.

## WEATHER-WIND-TRAILS-V0.2A — C# 9 RenderMesh invocation correction

**Status:** Accepted on 2026-07-22. Unity recompilation passed with no reported errors after removing the explicit `ref` modifier.

### Reported failure

Unity compilation reported:

```text
Assets\Game\Procedural\Weather\WeatherWindTrailRenderer.cs(2013,21): error CS9194: Argument 1 may not be passed with the 'ref' keyword in language version 9.0. To pass 'ref' arguments to 'in' parameters, upgrade to language version 12.0 or greater.
```

The failure is limited to the call-site argument modifier. It does not indicate a problem with the `RenderParams` contents, mesh contract, shader contract, or rendering architecture.

### Expected and actual affected files

Modify:

- `Assets/Game/Procedural/Weather/WeatherWindTrailRenderer.cs`
- `Assets/Docs/Weather_Wind_Architecture.md`

Create, delete, move, rename, generate, metadata, shader, scene, material, Weather producer, vegetation, Ground, compute, shared-HLSL, URP, renderer-feature, layer, tag, package, and hierarchy changes: none.

### Correction

The V0.2 call:

```csharp
Graphics.RenderMesh(ref renderParams, trailMesh, 0, Matrix4x4.identity);
```

was corrected to:

```csharp
Graphics.RenderMesh(renderParams, trailMesh, 0, Matrix4x4.identity);
```

The installed Unity API exposes the first parameter as readonly `in`; omitting the argument modifier is valid C# 9 call syntax and avoids the unsupported `ref`-to-`in` conversion. No language-version change is required or authorized.

### Consistency and validation

The correction changes no field values, bounds, camera restriction, layer, entity identity, shadow settings, submesh, transform, material properties, draw count, mesh data, or lifecycle behavior. Static checks confirm there is exactly one `Graphics.RenderMesh` call, no remaining `ref renderParams` token, balanced C# structure, and exact two-file scope. Unity recompilation remains required before V0.2A is accepted.


---

## WEATHER-WIND-TRAILS-V0.3 — Inspector and Scene diagnostics

**Status:** Historical V0.3 source record. Its Scene text legend and copy-only report workflow were corrected by `WEATHER-WIND-TRAILS-V0.3A`; current editor behavior is defined by the V0.3A section below.

### Objective and acceptance criteria

Add one compact custom Inspector for `WeatherWindTrailRenderer` and selected-object Scene-view diagnostics for the runtime data already exposed by V0.1–V0.2A. The patch must not change runtime trail placement, path integration, mesh/shader contracts, rendering behavior, scene state, Weather simulation, vegetation response, or serialized defaults.

This patch passes at source level when:

- the Inspector preserves every existing serialized renderer control through the standard serialized Inspector drawing path;
- one Actions group provides `Reset Wind Trail Simulation` and `Copy Wind Trail Report`, with clipboard copying as the primary report handoff path;
- one compact `Show Scene Diagnostics` control gates all editor visualization instead of adding multiple runtime debug modes or a separate debug component;
- selected-object Scene diagnostics draw the last candidate sweep using status-specific markers and active trails using their current centreline points;
- the Scene legend states that points represent the last candidate sweep and lines represent active generated paths;
- editor callbacks subscribe and unsubscribe symmetrically, editor code remains under the existing `Editor` folder and namespace, and no `UnityEditor` reference enters runtime code;
- the current runtime component, shader, scene, Weather producer/editor/compute/HLSL, vegetation consumer, Ground, River, URP assets, layers, tags, packages, and hierarchy remain unchanged.

### Reviewed evidence before the first write

- `Assets/AGENTS.md` was reread completely. It requires a persistent canonical plan before implementation, exact approved scope, full post-change reread, and honest Unity validation status.
- The complete current `WeatherWindTrailRenderer.cs` was reread after the user-confirmed V0.2A C# compilation pass. Its public diagnostics surface already provides `BuildComprehensiveReport`, `ResetTrailSimulation`, `TryGetLastCandidate`, `TryGetTrailPoint`, `GetTrailPointCount`, active/candidate counts, resource readiness, and current error state. No runtime-source expansion is required for V0.3.
- `WeatherWindDomainEditor.cs` was reread completely as the subsystem pattern for custom Inspector actions, clipboard reporting, selected-object `SceneView.duringSceneGui` drawing, balanced callback lifecycle, and Play Mode repainting.
- `VegetationInteractionDomainEditor.cs` and relevant `VegetationLayerEditor.cs` action/report patterns were inspected as supporting editor conventions.
- The current runtime and shader contracts from V0.1, V0.2, and V0.2A were compared. Editor diagnostics can consume public CPU data only; no GPU readback, mesh readback, field resampling, material access, or render-path change is required.
- The supplied project snapshot contains no `.git` directory, Unity executable, Unity reference assemblies, or configured Unity compiler. Git comparison and Unity compilation remain unavailable in this patch environment.

### Approved files for this patch

Modify:

- `Assets/Docs/Weather_Wind_Architecture.md`

Create:

- `Assets/Game/Procedural/Weather/Editor/WeatherWindTrailRendererEditor.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherWindTrailRendererEditor.cs.meta`

Delete, move, rename, generate, runtime-source, shader, material, scene, prefab, Weather producer/editor/compute/shared-HLSL, vegetation, Ground, River, URP, renderer-feature, layer, tag, package, and hierarchy changes: none.

### Inspector contract

The custom Inspector uses the normal serialized drawing path for all existing trail controls. It adds only:

```text
Weather Wind Trail Actions
  Reset Wind Trail Simulation
  Copy Wind Trail Report

Scene Diagnostics
  Show Scene Diagnostics
```

The Inspector also shows a compact readiness summary derived from the runtime component's existing public status and counts. It does not duplicate every report field, create a second debug interface, expose material internals, or add temporary tuning controls.

### Scene diagnostic contract

Diagnostics draw only while the trail component's GameObject is the active selection and `Show Scene Diagnostics` is enabled.

Candidate markers use the last completed candidate sweep:

```text
OutsideViewport = muted grey
BelowWindFloor = blue
TooClose = orange
Eligible = green
Selected = yellow
```

Active trail centrelines are drawn as bright cyan-white line segments with compact endpoint markers. The most recently selected candidate remains the explicit yellow seed marker for the latest completed sweep. The diagnostics use the component's public CPU arrays through `TryGetLastCandidate`, `GetTrailPointCount`, and `TryGetTrailPoint`; they do not inspect the runtime mesh or material and do not alter simulation state.

The Scene legend states:

```text
Points = last candidate sweep.
Lines = active generated paths.
```

### Performance and non-goals

All added work is editor-only and selected-object-only. Candidate drawing is bounded by the serialized lattice capacity (maximum 16 × 16 = 256 points), and path drawing is bounded by 16 trails × 48 points. No player-build code, runtime allocation, draw call, shader pass, compute dispatch, GPU readback, scene component, or serialized runtime field is added.

The patch does not install the component into the demo scene. Scene installation and first visual tuning remain `WEATHER-WIND-TRAILS-V0.4`.

### Risks and validation

- `SceneView.duringSceneGui` must be unsubscribed in `OnDisable`; static checks and Unity selection/reload testing must verify no duplicate callback.
- Diagnostics may be visually dense at the maximum 16 × 16 lattice. The single Inspector toggle is the approved suppression mechanism; no additional filtering controls are added in V0.3.
- The report must copy the exact runtime-generated string. Editor code must not reconstruct or summarize report telemetry.
- Unity validation must confirm editor compilation, Inspector control preservation, clipboard copy, reset behavior, Scene markers/paths, callback cleanup across selection/domain reload, and absence of new runtime errors.


### Post-change consistency and compliance audit

Actual affected files exactly match the approved V0.3 scope.

Modified:

- `Assets/Docs/Weather_Wind_Architecture.md`

Created:

- `Assets/Game/Procedural/Weather/Editor/WeatherWindTrailRendererEditor.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherWindTrailRendererEditor.cs.meta`

Deleted, moved, renamed, generated, runtime-source, shader, material, scene, prefab, Weather producer/editor/compute/shared-HLSL, vegetation, Ground, River, URP, renderer-feature, layer, tag, package, and hierarchy changes: none.

Implemented editor behavior:

- the custom Inspector preserves all existing serialized renderer controls through `DrawDefaultInspector`;
- a compact status section reports runtime readiness, resolved Weather domain and camera, active/capacity counts, last visible/eligible candidate counts, mesh capacity, and the component's existing concrete error string;
- `Reset Wind Trail Simulation` calls the runtime component's existing bounded reset API and is disabled until runtime resources exist;
- `Copy Wind Trail Report` copies the exact `BuildComprehensiveReport()` result to `EditorGUIUtility.systemCopyBuffer` and logs one contextual confirmation;
- one non-serialized `Show Scene Diagnostics` toggle controls all Scene-view visualization; no runtime debug enum, component, field, material control, or second diagnostic interface was added;
- Scene diagnostics run only for the selected component GameObject, show the last candidate sweep with status-specific point colours, show active trail centrelines and endpoints through the existing public CPU diagnostic APIs, restore `Handles.zTest` after drawing, and repaint continuously only during Play Mode;
- `SceneView.duringSceneGui` subscription is idempotent in `OnEnable` and removed in `OnDisable`.

Static validation passed **35/35** checks before this final documentation update. Checks covered custom-editor binding, balanced Scene callback lifecycle, default serialized Inspector preservation, reset and clipboard actions, the single Scene-diagnostics toggle, candidate/path public-API usage, selected-object gating, Play Mode repainting, depth-test restoration, legend wording, all candidate-status mappings, absence of `UnityEditor` from runtime code, C# structural balance, namespace/class declarations, LF/trailing-whitespace hygiene, valid unique metadata GUID, canonical-plan presence, and SHA-256 identity of the V0.2A runtime component and all V0.2 shader/metadata files.

The complete final editor file and relevant public runtime diagnostic surface were reread after implementation. No runtime trail placement, path integration, lifecycle, fixed-buffer, mesh, material, shader, render-submission, Weather, vegetation, or scene behavior changed. Unity editor compilation, Inspector rendering, clipboard behavior, reset behavior, selected-object candidate/path drawing, callback cleanup across selection and domain reload, and Scene-view visual density remain pending and are not represented as passed.

---

## WEATHER-WIND-TRAILS-V0.3A — Inspector workflow correction

**Status:** Editor workflow correction and source-level post-change audit complete on 2026-07-22. Unity editor compilation and live shader-assignment/report/Scene validation remain pending.

### Objective and acceptance criteria

Correct two editor-workflow faults reported during the first live V0.3 validation:

1. The Scene view currently renders a text legend/status panel even though the same information belongs in the component Inspector.
2. A newly attached `WeatherWindTrailRenderer` remains `NOT READY` because its serialized `trailShader` reference is empty, while the V0.3 validation instructions omitted the required manual shader assignment.

Observed evidence:

- The user-provided Scene-view capture shows the `Weather Wind Trail Diagnostics` text panel rendered over the Scene view.
- The user-provided runtime report states `Serialized trail shader: None`, `Runtime material ready: No`, and `A serialized wind-trail shader is required.` after the component was attached to the existing Weather object.
- `WeatherWindTrailRenderer.cs` declares the private serialized field `trailShader` and requires it in `CanRun`; the exact shader asset already exists at `Assets/Game/Rendering/Weather/Shaders/SH_WeatherWindTrails.shader`.
- `WeatherWindTrailRendererEditor.cs` currently calls `DrawLegend` from `DuringSceneGui` and provides report copying without displaying the complete report in the Inspector.

This correction passes at source level when:

- the exact default shader asset is assigned to an empty `trailShader` serialized property when the component Inspector resolves that asset;
- the assigned reference remains serialized on the scene component, preserving build retention without runtime `Shader.Find` dependency;
- the Scene view contains only optional candidate markers and active-path geometry, with no GUI text panel;
- a dedicated Inspector report section displays the exact `BuildComprehensiveReport()` string and provides its adjacent `Copy Report` action;
- `Reset Wind Trail Simulation` remains available as the only simulation action;
- no runtime, shader, scene-YAML, material-asset, Weather-domain, vegetation, Ground, renderer, layer, tag, or hierarchy code changes occur.

### Approved files

Modify:

- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Game/Procedural/Weather/Editor/WeatherWindTrailRendererEditor.cs`

Create, delete, move, rename, metadata, runtime-source, shader, material, raw scene/prefab, Weather producer/compute/HLSL, vegetation, Ground, River, URP, renderer-feature, layer, tag, package, and hierarchy changes: none.

### Implementation sequence

1. Add an editor-only exact-path resolver for `Assets/Game/Rendering/Weather/Shaders/SH_WeatherWindTrails.shader`.
2. When the selected component's serialized `trailShader` property is empty and the asset resolves, record Undo, assign the shader through `SerializedObject`, apply the property, and mark the component dirty so the scene stores the reference.
3. Remove the Scene GUI legend call and implementation. Preserve the optional selected-object candidate markers and active path lines.
4. Move report copying into a new Inspector report section that displays the exact runtime-generated report in a selectable scroll area.
5. Reread the final editor source and relevant runtime serialized/report contracts; run structural, scope, forbidden-dependency, exact-path, and whitespace checks.

### Invariants and non-goals

- Do not change `WeatherWindTrailRenderer.cs`, the shader, or any serialized runtime default.
- Do not create a material asset or use `Shader.Find` as runtime fallback.
- Do not reconstruct report telemetry in editor code; display and copy the exact runtime-generated report.
- Do not remove candidate/path Scene diagnostics; remove only the Scene text overlay.
- Do not raw-edit the demo scene. The editor assignment marks the existing component dirty and Unity serializes the reference when the user saves normally.
- Do not add a second debug component, runtime debug enum, or preview-specific Inspector.

### Risks and validation

- Automatic assignment must occur only when the serialized shader reference is empty and the exact shader asset exists; an explicit user-assigned shader must never be overwritten.
- The editor must tolerate the shader asset being temporarily unavailable during import and retry on the next Inspector draw.
- Unity validation must confirm that opening the component Inspector assigns `SH_WeatherWindTrails`, changes the report to shader/material-ready in Play Mode, shows the full report in the Inspector, copies the exact report, and leaves the Scene view free of diagnostic text.


### Post-change consistency and compliance audit

Actual affected files exactly match the approved V0.3A scope.

Modified:

- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Game/Procedural/Weather/Editor/WeatherWindTrailRendererEditor.cs`

Created, deleted, moved, renamed, metadata, runtime-source, shader, material, raw scene/prefab, Weather producer/compute/HLSL, vegetation, Ground, River, URP, renderer-feature, layer, tag, package, and hierarchy changes: none.

Implemented corrections:

- the custom Inspector resolves the exact shader asset at `Assets/Game/Rendering/Weather/Shaders/SH_WeatherWindTrails.shader` only when the component's serialized `trailShader` property is empty;
- assignment uses `AssetDatabase.LoadAssetAtPath<Shader>`, `Undo.RecordObject`, the existing serialized property, `ApplyModifiedProperties`, and `EditorUtility.SetDirty`, so an existing user-assigned shader is preserved and no runtime `Shader.Find` fallback is added;
- the Inspector status now shows the resolved trail shader;
- the full exact `BuildComprehensiveReport()` output appears in a dedicated selectable, scrollable `Weather Wind Trail Report` foldout with its adjacent `Copy Report` button;
- `Reset Wind Trail Simulation` remains in the Actions section, while report copying is no longer duplicated there;
- the Scene diagnostic text legend/status panel and all `Handles.BeginGUI`/GUI-area drawing were removed;
- optional selected-object candidate markers and active centreline geometry remain behind the existing `Show Scene Diagnostics` toggle.

Static validation passed **27/27** checks. Checks covered exact two-file scope, exact shader asset path, empty-only assignment, Undo and serialized-property persistence, absence of `Shader.Find`, exact runtime-report display and copy, removal of the Scene GUI legend, preservation of candidate/path geometry and callback lifecycle, balanced C# structure, LF-only content, and trailing-whitespace hygiene. The existing project contains a verified `EditorGUILayout.SelectableLabel` usage pattern in `VegetationBenchmarkRunnerEditor.cs`; the V0.3A report uses the same API with the text-area style inside a bounded Inspector scroll view.

The complete final editor source and the relevant runtime `trailShader`, `TrailShader`, `BuildComprehensiveReport`, `ResourcesReady`, and `RuntimeReady` contracts were reread. No runtime trail placement, candidate selection, streamline integration, mesh, material, shader, render submission, Weather, vegetation, or scene behavior changed. Unity editor compilation, automatic shader assignment on the existing component, scene-dirty persistence, runtime material creation, Inspector report display/copy, and absence of Scene-view text remain pending and are not represented as passed.

---

## WEATHER-WIND-TRAILS-V0.4 — Play Mode ownership and visual-calibration Inspector

**Status:** Plan recorded on 2026-07-22 before implementation. Runtime/editor implementation and source-level audit pending.

### Objective and acceptance criteria

Finalize the installed V0 trail workflow for visual evaluation without changing the accepted wind simulation, placement algorithm, streamline geometry, shader, or serialized visual defaults. This patch combines the small Play Mode-ownership correction with a compact calibration-oriented Inspector because a standalone ownership patch is not justified.

User-provided runtime evidence after V0.3A confirms:

- the component is installed on the existing Weather object and resolves the co-located published `WeatherWindDomain` and `Main Camera`;
- the exact serialized shader resolves as `PS3D/Weather/Weather Wind Trails`, is supported, and creates the hidden runtime material;
- the fixed mesh allocates 384 vertices and 1,104 indices;
- user-reported Unity import evidence before applying this V0.4 archive found that the V0.2/V0.3A interleaved vertex attributes were supplied in the non-standard order `Position, Normal, TexCoord0, TexCoord1, Color`; Unity automatically reordered them to `Position, Normal, Color, TexCoord0, TexCoord1` and emitted a warning;
- candidate/path generation is active, accepted seeds respect the 6 m minimum separation with reported examples above 9 m, generated paths report alignment `1`, and one bounded render submission occurs per active frame;
- the component currently continues candidate/path simulation in Edit Mode because `[ExecuteAlways]` remains on the runtime component, even though rendering is Play Mode-only.

The patch passes at source level when:

- `WeatherWindTrailRenderer` no longer uses `[ExecuteAlways]` and performs no resource allocation, spawning, expiration, path integration, mesh upload, or render submission outside Play Mode;
- the report and Inspector distinguish `Editor idle` from a runtime readiness failure;
- the reset action and Scene candidate/path diagnostics are available only during Play Mode;
- the custom Inspector replaces the unstructured default field dump with three existing-control groups: `Visual Calibration`, `Placement & Density`, and collapsed `Advanced Generation`;
- every serialized field remains exposed exactly once, with no new tuning field, preview interface, debug component, shader property, or materially different default;
- the current shader, scene, Weather domain/editor/compute/HLSL, vegetation, Ground, URP assets, layers, tags, packages, and hierarchy remain unchanged.

### Approved affected files

Modify:

- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherWindTrailRenderer.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherWindTrailRendererEditor.cs`

Create, delete, move, rename, metadata, shader, material, raw scene/prefab, Weather producer/editor/compute/shared-HLSL, vegetation, Ground, River, URP, renderer-feature, layer, tag, package, and hierarchy changes: none.

The demo-scene component installation is already user-completed through Unity and evidenced by the supplied runtime reports. The supplied source archive does not contain that latest serialized scene state, and repository rules prohibit reconstructing it through a raw YAML edit. This patch therefore does not deliver or overwrite `VisualFrameworkDemo.unity`.

### Reviewed evidence before the first write

- `Assets/AGENTS.md` was reread completely.
- The complete current `WeatherWindTrailRenderer.cs`, `WeatherWindTrailRendererEditor.cs`, and `SH_WeatherWindTrails.shader` were reread.
- The V0.1–V0.3A canonical sections and the original handoff V0.4 scene-installation requirement were reviewed.
- The user-provided reports show successful shader/material readiness, active trails, fixed mesh capacity, candidate filtering, separation, valid path construction, and active render submissions. The first report also proves Edit Mode simulation occurred because it contains spawn attempts, active trails, and mesh uploads with zero render submissions.
- The current source snapshot contains no Git metadata or Unity executable; Git comparison and Unity compilation are unavailable in this environment.

### Implementation sequence

1. Remove `[ExecuteAlways]`; add explicit Play Mode guards to enable/update/reset/resource paths and make `RuntimeReady` Play Mode-owned.
2. Update the runtime report to use `EDITOR IDLE` outside Play Mode and suppress stale runtime errors there.
3. Replace `DrawDefaultInspector` with explicit serialized groups containing every existing field exactly once.
4. Disable reset and Scene diagnostics outside Play Mode while preserving automatic exact-path shader assignment and the full selectable/copyable report.
5. Reread the complete final runtime/editor sources, compare all frozen dependencies, and record exact scope and validation results below.

### Invariants and non-goals

- Do not change the candidate algorithm, spacing, RK2 integration, path limits, vertex semantics/stride/capacity, material creation, shader equations, render call, Weather cadence, or any serialized tuning default. The interleaved field and descriptor declaration order may be corrected to Unity's required standard attribute order without changing those semantics.
- Do not add a visual preset, temporary preview, second report interface, new debug enum, or additional Inspector control.
- Do not raw-edit the scene. The current user-owned scene installation remains authoritative.
- Do not claim visual acceptance without a Game-view capture. This patch prepares the compact calibration surface; appearance tuning remains evidence-driven.
- Do not claim Frame Debugger, GC, CPU, GPU, desktop/Mobile, transparency sorting, bloom, or depth-occlusion validation without Unity evidence.

### Required validation

- Unity must compile the runtime and editor changes without new errors.
- Outside Play Mode, the report must show `EDITOR IDLE`, runtime resources/counters must remain inactive, reset must be disabled, and Scene diagnostics must draw nothing.
- In Play Mode, the component must return to `READY`, generate/render trails, and preserve the fixed capacities and one-submit-per-active-frame behavior.
- The Inspector must expose every existing field exactly once under the three approved groups, preserve automatic shader assignment, and retain the full report/copy workflow.
- A Game-view capture is required before changing visual defaults or shader behavior.

### Post-change consistency and compliance audit

**Status:** Runtime/editor source implementation and source-level audit complete on 2026-07-22. Unity compilation and live behavior remain pending.

Actual affected files exactly match the approved V0.4 scope.

Modified:

- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherWindTrailRenderer.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherWindTrailRendererEditor.cs`

Created, deleted, moved, renamed, metadata, shader, material, raw scene/prefab, Weather producer/editor/compute/shared-HLSL, vegetation, Ground, River, URP, renderer-feature, layer, tag, package, and hierarchy changes: none.

Implemented behavior:

- `[ExecuteAlways]` was removed from `WeatherWindTrailRenderer`;
- `OnEnable` allocates the fixed arrays, mesh, and hidden runtime material only while `Application.isPlaying`;
- `Update`, `ResetTrailSimulation`, `CanRun`, and `EnsureResources` now have explicit Play Mode ownership guards, preventing Edit Mode spawning, expiration, path integration, mesh upload, or render submission;
- `RuntimeReady` is false outside Play Mode without representing editor idleness as a runtime fault;
- the report header is now `Weather Wind Trails V0.4 Calibration Report`; outside Play Mode it reports `EDITOR IDLE` and `No (Play Mode only)`, resolves the co-located Weather domain/camera for useful editor context, and suppresses stale runtime error output;
- the Inspector no longer emits the unstructured `DrawDefaultInspector` field list;
- every one of the 41 existing serialized fields is exposed exactly once under `Visual Calibration`, `Placement & Density`, or collapsed `Advanced Generation`; no serialized field or default was added, removed, or changed;
- exact-path automatic shader assignment, runtime status, full selectable report, adjacent `Copy Report`, and the single Scene-diagnostics toggle remain;
- reset is disabled outside Play Mode, and Scene candidate/path geometry neither draws nor repaints outside Play Mode;
- the interleaved `TrailVertex` field order and `SetVertexBufferParams` descriptor order are both now `Position, Normal, Color, TexCoord0, TexCoord1`, matching Unity's required standard order while preserving the same shader semantics, 52-byte stride, 384-vertex capacity, and 1,104-index topology; this prevents Unity from silently adjusting the layout and removes the reported warning.

Static validation passed **87/87** checks before this final documentation update. Checks covered exact three-file scope; absence of `[ExecuteAlways]`; Play Mode gates on runtime readiness, enable, update, reset, `CanRun`, and resource creation; editor-idle report/status behavior; absence of `DrawDefaultInspector`; presence and default expansion state of the three Inspector groups; exact-once exposure of all 41 serialized fields; preserved exact-path shader assignment, report copy, and selected-object diagnostics; disabled Edit Mode reset/Scene diagnostics; matching standard `Position, Normal, Color, TexCoord0, TexCoord1` declaration order in both `TrailVertex` and `SetVertexBufferParams`; preserved 52-byte stride and shader semantic/property contract; balanced C# braces/parentheses; LF-only and trailing-whitespace hygiene; and SHA-256 identity of the shader, Weather domain/editor/compute/HLSL, vegetation wind consumer, demo scene, and provisional Weather document.

The complete final runtime and editor files were reread after implementation. Candidate selection, scoring, spacing, cooldown, RK2 path integration, fixed vertex/index contract, slot uploads, material properties, shader equations, render submission, Weather simulation, vegetation response, scene state, and all serialized defaults remain unchanged.

Unity compilation, `EDITOR IDLE` Inspector/report behavior, absence of Edit Mode allocations/counters, Play Mode return to `READY`, grouped Inspector rendering, reset/Scene gating, Game-view appearance, Frame Debugger one-draw evidence, steady-state GC, CPU/GPU timing, depth occlusion, transparency sorting, bloom response, and desktop/Mobile compatibility remain pending and are not represented as passed. A Game-view capture is required before any visual-default or shader-tuning patch.

---

## WEATHER-WIND-TRAILS-V0.5 — Calm visual baseline, uniform body opacity, and occasional broad waves

**Status:** Runtime, editor, shader, migration, and source-level post-change audit complete on 2026-07-22. Unity compilation and live visual/performance validation remain pending.

### Objective and acceptance criteria

Calibrate the first visible wind-trail implementation from the user-provided Game-view evidence and expose the two newly requested artistic behaviours as ordinary Inspector controls.

Observed evidence:

- The user-confirmed V0.4 implementation compiles, no longer emits the non-standard vertex-layout warning, and renders visible wind trails.
- The supplied Game-view capture shows one narrow, pale-grey trail with low body opacity and a short visible extent relative to the vegetation field.
- The user reports that trails move too quickly, exist for only about two to three seconds, are too transparent, fade across their body edges, appear too frequently, attract too much attention through repeated spawning/despawning, and are too short on average.
- The user requires no more than two to three visible trails at once, approximately four-to-seven-second lifetimes, slower presentation than the underlying gust pattern, a whiter and less transparent body, longer average visible and generated length, and an occasional broader wave.
- The user explicitly requires controls for uniform body opacity and occasional larger waves rather than hard-coding either behaviour.

The patch passes at source level when:

- `Uniform Body Opacity` is an exposed serialized control. When enabled, opacity remains spatially constant across the trail body; width/head/tail boundaries use geometry taper and minimal raster anti-aliasing rather than broad alpha gradients. When disabled, the existing soft alpha-body mode remains available and `Edge Softness` remains editable.
- `Occasional Broad Wave Chance` and `Occasional Broad Wave Strength` are exposed serialized controls. A deterministic subset of accepted trails receives one bounded lateral XZ macro-wave, with zero endpoint offset, after authoritative streamline integration.
- Broad-wave deformation is accepted only when the resulting path remains inside the Weather field/camera limits, avoids self-approach, and satisfies the existing target-wind alignment threshold; otherwise the undeformed authoritative path is retained.
- The recommended serialized baseline becomes: maximum active trails `3`; spawn attempts `1/s`; separation `8 m`; cooldown `3 s`; lifetime `4–7 s`; presentation speed `1.2–2.0 m/s`; visible tail `5–8 m`; maximum centreline points `32`; minimum completed path `4 m`; white colour; opacity `0.95`; uniform body opacity enabled; short lifetime fades `0.03/0.05`; strength/variation opacity influence `0`; broad-wave chance `0.22`; broad-wave strength `0.45 m`.
- Presentation speed is bounded against generated path length, visible-tail length, and lifetime so a short trail cannot completely leave its centreline substantially before its configured lifetime ends.
- Existing installed components migrate once from exact V0.4 baseline values to the V0.5 baseline while preserving any field whose serialized value no longer equals the old baseline.
- The migration is versioned and hidden; it does not add a permanent preset/debug action or overwrite later user tuning.
- The existing Play Mode-only ownership, co-located Weather resolution, fixed arrays, candidate selection, spacing, RK2 integration, mesh layout, hidden runtime material, one render submission, report/copy workflow, and Scene diagnostics remain intact.

### Approved affected files

Modify:

- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherWindTrailRenderer.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherWindTrailRendererEditor.cs`
- `Assets/Game/Rendering/Weather/Shaders/SH_WeatherWindTrails.shader`

Create, delete, move, rename, metadata, scene, prefab, material asset, Weather producer/editor/compute/shared-HLSL, vegetation, Ground, River, URP, renderer-feature, layer, tag, package, and hierarchy changes: none.

### Reviewed evidence before the first implementation edit

- `Assets/AGENTS.md` was reread completely.
- The complete fixed V0.4 `WeatherWindTrailRenderer.cs` and `WeatherWindTrailRendererEditor.cs` were reread.
- The complete current `SH_WeatherWindTrails.shader` was reread and its C#/shader property contract was compared with `ApplyRuntimeMaterialProperties` and `TrailVertex`.
- The V0.1–V0.4 canonical sections, current fixed vertex layout, current report, current Inspector grouping, and current Play Mode ownership were reviewed.
- The supplied source snapshot contains no Git metadata or Unity executable; Git comparison and Unity compilation are unavailable in this environment and remain pending.

### File-by-file implementation sequence

1. `WeatherWindTrailRenderer.cs`
   - add the versioned exact-old-default migration;
   - apply the calibrated initializers and validation clamps;
   - add uniform-body and broad-wave serialized controls, property IDs, configuration hashing, report fields, and runtime material publication;
   - add fixed broad-wave scratch/state storage without steady-state allocation;
   - apply deterministic lateral deformation after RK2 integration, validate it, and fall back to the undeformed path when invalid;
   - recompute path distances, tangents, and alignment after accepted deformation;
   - cap presentation speed against useful path occupancy;
   - preserve the standard interleaved vertex order and all Play Mode-only lifecycle gates.
2. `WeatherWindTrailRendererEditor.cs`
   - expose `Uniform Body Opacity` in Visual Calibration;
   - keep `Edge Softness` visible but disabled while uniform mode is enabled;
   - expose broad-wave chance and strength together in Visual Calibration;
   - invoke/persist the versioned migration through Undo when the Inspector first observes an old installed component;
   - preserve automatic shader assignment, status, report/copy, reset, and Scene diagnostics.
3. `SH_WeatherWindTrails.shader`
   - add `_UniformBodyOpacity` with a default of enabled;
   - in uniform mode, keep spatial alpha constant across the visible body, clip outside the head/tail window, taper physical ribbon width near the head/tail, and retain only a narrow derivative-based edge anti-alias transition;
   - in soft mode, preserve the existing head/tail/edge alpha masks;
   - retain short whole-trail lifetime fades and premultiplied transparent output.
4. Reread all complete final files, compare every frozen dependency and contract, run structural/property/scope/allocation/default/migration checks, and record the final actual-file and pending-Unity evidence below.

### Invariants and non-goals

- Weather target wind remains the authoritative centreline source. Broad waves are bounded presentation deformation, not a second wind simulation.
- Do not sample vegetation response, Ground, colliders, depth textures, opaque textures, or GPU readback.
- Do not add per-frame path rebuilding, managed collections, material instances per trail, new GameObjects, renderer features, or scene objects.
- Do not change the vertex semantic order `Position, Normal, Color, TexCoord0, TexCoord1`, 52-byte stride, unsigned 16-bit indices, or one combined mesh.
- Do not raw-edit the already user-modified scene. The versioned component migration must update exact old defaults through normal Unity serialization.
- Do not make every trail broad-wave-shaped. The default chance is deliberately low and independently editable.
- Do not claim visual acceptance, occupancy balance, lifetime feel, transparency quality, bloom response, Frame Debugger count, GC, CPU/GPU cost, or desktop/Mobile compatibility without new Unity evidence.

### Performance model and validation

At the new default capacity, fixed geometry decreases from V0.4's `8 × 24 × 2 = 384` vertices and `8 × 23 × 6 = 1,104` indices to `3 × 32 × 2 = 192` vertices and `3 × 31 × 6 = 558` indices. Broad-wave work is `O(S)` only for a successful spawn and uses fixed scratch arrays already sized to centreline capacity. No new per-frame or per-spawn managed allocation is permitted.

Required Unity validation:

1. Compile C# and shader code with no new errors or warnings; confirm the standard vertex-layout warning remains absent.
2. Confirm the installed component migrates exact V0.4 defaults once to the V0.5 values while a deliberately edited test value remains unchanged.
3. In Play Mode, verify no more than three trails are active, trails typically persist four to seven seconds, and the report shows bounded broad-wave use and the reduced `192 / 558` default mesh capacity.
4. Toggle `Uniform Body Opacity` and confirm enabled mode has a solid body with only narrow edge anti-aliasing, while disabled mode restores editable soft edge/head/tail alpha behaviour.
5. Set broad-wave chance temporarily to `1`, confirm visibly broader but still wind-aligned paths, then restore `0.22` and confirm only an occasional trail receives the deformation.
6. Capture the normal Game view and the complete report for the next visual/performance decision.


### Post-change consistency and compliance audit

Actual affected files exactly match the approved V0.5 scope.

Modified:

- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherWindTrailRenderer.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherWindTrailRendererEditor.cs`
- `Assets/Game/Rendering/Weather/Shaders/SH_WeatherWindTrails.shader`

Created, deleted, moved, renamed, metadata, scene, prefab, material asset, Weather producer/editor/compute/shared-HLSL, vegetation, Ground, River, URP, renderer-feature, layer, tag, package, and hierarchy changes: none.

Implemented runtime and baseline changes:

- the calm baseline now uses at most `3` active trails, `1` spawn attempt per second, `8 m` minimum separation, and `3 s` post-expiry occupancy cooldown;
- lifetime is `4–7 s`, presentation speed is `1.2–2.0 m/s`, visible tail length is `5–8 m`, completed paths require at least `4 m`, and centreline capacity is `32` points;
- a generated speed is capped to `(pathLength + tailLength) / lifetime`, preventing a short trail's visible window from fully leaving its path substantially before the configured lifetime expires;
- colour is white, per-trail opacity is `0.95`, lifetime fade fractions are `0.03/0.05`, and default strength/variation opacity influences are zero;
- `Uniform Body Opacity` is serialized and enabled by default. The runtime publishes `_UniformBodyOpacity`; the shader uses physical head/tail width taper, hard visible-window clipping, and derivative-width edge anti-aliasing in uniform mode. Disabling it restores the previous soft head/tail/cross-width alpha masks and re-enables `Edge Softness` in the Inspector;
- `Occasional Broad Wave Chance` and `Occasional Broad Wave Strength` are serialized at `0.22` and `0.45 m`. A deterministic accepted subset receives one endpoint-zero lateral XZ wave after RK2 construction;
- broad-wave paths are remeasured and resampled against the authoritative target wind. A deformed path is retained only if it remains inside the safe field/camera region, does not self-approach, and meets the existing minimum segment alignment; otherwise fixed scratch storage restores the undeformed path;
- active, total, and last-path broad-wave diagnostics are included in the exact runtime report;
- a hidden serialized baseline version upgrades exact V0.4 values once. Integer/float/colour replacements occur only when the current value still equals the old baseline; non-default user tuning is preserved. New uniform/broad-wave controls receive their approved first baseline. The custom Inspector records Undo and marks the component dirty when it observes an old installed component;
- all new broad-wave arrays are allocated only during the existing resource build and released with the existing resources. No managed collection or reference-object allocation was added to candidate, path, spawn, update, or render paths.

Inspector changes:

- `Uniform Body Opacity` appears in `Visual Calibration`;
- `Edge Softness` remains visible but is disabled while uniform opacity is enabled;
- broad-wave chance and strength appear together under `Occasional Broad Waves`;
- all other non-hidden serialized fields remain exposed exactly once;
- exact-path shader assignment, Play Mode status, complete selectable report, adjacent copy action, reset action, and selected-object Scene diagnostics remain intact.

Performance and contract reconciliation:

- default fixed geometry decreases from `384` vertices / `1,104` indices to `192` vertices / `558` indices because the capacity changes from eight 24-point trails to three 32-point trails;
- broad-wave deformation is `O(S)` only for a successful spawn selected by the chance control and uses one fixed `Vector2[S]` backup array plus existing fixed path arrays;
- the C#/shader material-property contract includes `_UniformBodyOpacity` with no missing property;
- the standard mesh declaration order remains `Position, Normal, Color, TexCoord0, TexCoord1`, the vertex stride remains 52 bytes, indices remain unsigned 16-bit, and render submission remains one combined `Graphics.RenderMesh` call;
- Play Mode-only ownership, co-located published Weather resolution, target-wind sampling, deterministic candidate placement, separation, RK2 integration, hidden single material, and target-camera-only rendering remain unchanged.

Pre-documentation static validation passed **85/85** checks. A final post-documentation audit passed **80/80** checks and analytically replayed the default `0.45 m` broad wave across 4–15.5 m straight paths; the worst segment alignment was `0.8325`, above the configured `0.35` floor. Checks covered exact four-file scope; C#/ShaderLab structural balance; calibrated defaults; versioned exact-old-value migration; fixed scratch storage and release; no reference allocation in broad-wave construction; deterministic chance; endpoint envelope; bounds/self-approach/alignment fallback; lifespan-aware speed cap; complete C#/shader property parity; uniform-mode window clipping, width taper, narrow edge anti-aliasing, and retained soft fallback; standard vertex/descriptor order; exact-once Inspector exposure; disabled edge-softness editing in uniform mode; broad-wave controls; migration Undo/persistence; Play Mode ownership; one material construction; one render submission; absence of Ground/vegetation-response dependencies; report coverage; LF-only content; and no trailing whitespace.

The complete final runtime, editor, shader, and canonical V0.5 section were reread after implementation. No undeclared dependency or file appeared. Unity C# compilation, shader import, one-time scene-component migration, uniform/nonuniform visual comparison, forced/default broad-wave frequency, active occupancy, four-to-seven-second observed lifetime, Game-view appearance, vertex-layout warning absence, Frame Debugger draw count, steady-state GC, CPU/GPU timing, transparency sorting, bloom response, depth occlusion, and desktop/Mobile compatibility remain pending and are not represented as passed.

---

## WEATHER-WIND-TRAILS-V0.6 — Length-resolved spawn/alive/despawn lifecycle and pointed endpoints

**Status:** Runtime, editor, shader, migration, and source-level audit complete on 2026-07-22. Unity compilation and live visual validation remain pending.

### Objective and user-approved behavior

Replace the finite moving-window/lifetime model that could clamp against the end of a centreline, shrink with a flat front, or disappear while still visibly present. Every accepted trail now owns one guaranteed complete lifecycle:

```text
length-resolved spawn -> authored alive phase -> length-resolved despawn
```

The user approved these requirements:

- spawn and despawn duration are not directly authored because trails have different visible lengths;
- normal travel speed remains inside the authored travel-speed range;
- a separate lifecycle tip-speed allowance may temporarily let an endpoint move up to `travel speed + allowance` during growth or contraction;
- alive duration is directly authored at a default range of `7–11 s`;
- Inspector diagnostics show the resolved spawn, despawn, and total-duration intervals produced by the current controls;
- both visible endpoints remain physically pointed; no trail may shrink against a flat front or vanish mid-air;
- a trail is accepted only when its generated path can contain its complete spawn, alive, and despawn sequence.

### Approved affected files

Modify:

- `Assets/Docs/Weather_Wind_Architecture.md`
- `Assets/Game/Procedural/Weather/WeatherWindTrailRenderer.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherWindTrailRendererEditor.cs`
- `Assets/Game/Rendering/Weather/Shaders/SH_WeatherWindTrails.shader`

Create, delete, move, rename, metadata, scene, prefab, material asset, Weather producer/editor/compute/shared-HLSL, vegetation, Ground, River, URP, renderer-feature, layer, tag, package, and hierarchy changes: none.

### Lifecycle equations and invariant

For one accepted trail:

```text
v = resolved normal travel speed
E = resolved lifecycle tip-speed allowance, clamped below v
L = resolved visible body length
A = resolved alive duration

spawn duration   S = L / (v + E)
despawn duration D = L / (2E)
```

Spawn phase:

```text
tail speed = 0
head speed = v + E
```

Alive phase:

```text
tail speed = v
head speed = v
```

Despawn phase:

```text
tail speed = v + E
head speed = v - E
```

The despawn endpoints therefore approach at `2E`, while their centre continues at normal speed `v`. `E` is clamped to at most `0.9v`, so the head always continues forward and never stops or reverses.

The required usable centreline length is:

```text
requiredPath = vA + L/2 + vL/(2E)
```

The hard runtime invariant is:

> An active trail always has enough generated centreline for its complete spawn, alive, and despawn sequence. The shader never clamps a moving head to the path endpoint, and CPU expiry occurs only after the two pointed endpoints have met.

### Resolution and fallback order

For each successful streamline, the runtime samples deterministic desired values from the authored ranges and resolves them against actual usable path length:

1. retain the desired body length when minimum speed can support at least the minimum alive duration;
2. otherwise reduce body length, never below the authored minimum;
3. retain the desired alive duration when possible, otherwise reduce it, never below the authored minimum;
4. select the highest speed not exceeding the deterministic desired speed that fits the path, never below the authored minimum and never above the authored maximum;
5. reject the candidate if the minimum body length, minimum alive duration, and minimum speed still cannot fit.

No fit operation increases normal travel speed. Spawn/despawn endpoint speed can exceed normal speed only by the separately authored and bounded allowance.

### Path construction

The selected visible candidate is now the actual spawn point. Streamline construction integrates forward only from that seed through the authoritative target wind using the existing captured-time RK2 contract. Paths may continue outside the camera viewport, but remain inside the Weather field safety rectangle and retain all wind-strength, turn, self-approach, broad-wave fallback, and segment-alignment checks.

Default centreline capacity increases from `32` to `80` points at the existing `0.5 m` integration step, allowing approximately `39.5 m` of generated path. At the default controls, the complete lifecycle requires approximately `13.4–29.3 m`, depending on resolved length, speed, and alive duration.

### Shader and endpoint geometry

The old global lifetime fades and head/tail alpha-softness controls are removed. Lifecycle values are supplied per trail in two vertex `float4` attributes:

```text
lifecycleMotion = birth time, travel speed, body length, alive duration
lifecycleTiming = spawn duration, despawn duration, pointed-end length, total lifetime
```

The shader resolves exact head and tail arc distances for all three phases. Both endpoints taper physical ribbon width to zero over the authored pointed-end distance. Vertices outside the visible interval are centreline-clamped to the exact endpoint before expansion, and the interpolated distance is clamped with them. This prevents the fragment cutoff from exposing a full-width flat cross-section between fixed centreline samples.

`Uniform Body Opacity` remains editable. Enabled mode keeps body alpha spatially uniform apart from narrow raster edge anti-aliasing; disabled mode restores editable cross-width softness, but endpoint shape remains physically pointed in both modes. There is no whole-trail fade or emergency mid-air disappearance.

### Defaults and Inspector

Default lifecycle controls:

```text
Alive Duration:                 7–11 s
Travel Speed:                  1.0–1.5 m/s
Visible Body Length:           5.5–8.5 m
Lifecycle Tip Speed Allowance: 0.75 m/s
Pointed End Length:            0.75 m
Centreline Capacity:           80 points
```

The Inspector exposes no editable spawn/despawn duration. It displays read-only resolved intervals derived from body length, speed, allowance, and alive duration. With defaults these are approximately:

```text
Spawn:         2.44–4.86 s
Despawn:       3.67–5.67 s
Total lifetime 13.11–21.52 s
```

The full runtime report records both the configured intervals and the actual body length, normal speed, effective allowance, spawn duration, alive duration, despawn duration, total lifetime, and required path length of the last accepted trail.

### Migration

Serialized baseline version advances from `1` to `2`. `FormerlySerializedAs` preserves the installed component's old lifetime and visible-tail values as the new alive-duration and visible-body fields before migration. Exact V0.5 defaults migrate once:

```text
Centreline points 32 -> 80
Alive duration    4–7 -> 7–11 s
Travel speed      1.2–2.0 -> 1.0–1.5 m/s
Body length       5–8 -> 5.5–8.5 m
```

Values changed away from the exact old defaults remain untouched. New allowance and pointed-end controls receive their declared first defaults. The custom Inspector retains Undo and dirty-state persistence for the migration.

### Performance and fixed resources

At the default `3 × 80` capacity:

```text
Vertices: 3 × 80 × 2 = 480
Indices:  3 × 79 × 6 = 1,422
Triangles: 474
Vertex stride: 68 bytes
```

The stride rises from 52 to 68 bytes because of the second lifecycle `float4`; the complete fixed vertex payload remains approximately 31.9 KiB, plus approximately 2.8 KiB of 16-bit indices. Path integration and lifecycle fitting occur only on bounded spawn attempts. The speed fit uses a fixed 20-iteration scalar binary search and creates no managed objects. No per-frame path rebuild, new draw, new material, or new simulation dispatch is introduced.

The standard vertex declaration order remains:

```text
Position, Normal, Color, TexCoord0, TexCoord1, TexCoord2
```

### Source-level validation and pending Unity evidence

Source-level checks confirm:

- exact four-file scope;
- balanced C# and ShaderLab/HLSL structure;
- no stale direct lifetime, lifetime-fade, head-softness, tail-softness, path-viewport, backward-path, or old trail-lifetime runtime references;
- versioned exact-default migration and `FormerlySerializedAs` coverage;
- forward-only authoritative RK2 paths beginning at the visible candidate;
- no path-viewport termination after seed acceptance;
- required-path fitting never raises speed above the sampled authored value;
- endpoint allowance remains below normal speed;
- CPU total-lifetime expiry matches shader total lifetime;
- complete C#/shader lifecycle attribute and material-property parity;
- physical endpoint taper and exact endpoint-clamped vertex placement;
- standard vertex/descriptor ordering with `TexCoord2` appended;
- fixed arrays and no managed allocation in lifecycle fitting;
- Inspector exposure of all active controls and read-only resolved timing;
- LF-only content and no trailing whitespace.

Unity compilation, shader import, baseline migration, the absence of the vertex-layout warning, actual resolved report values, guaranteed full phase sequencing, front/tail pointedness throughout spawn and despawn, absence of mid-air disappearance, occupancy behavior under longer total lifetimes, Frame Debugger draw count, steady-state GC, CPU/GPU timing, depth behavior, transparency sorting, bloom response, and desktop/Mobile compatibility remain pending and are not represented as passed.
