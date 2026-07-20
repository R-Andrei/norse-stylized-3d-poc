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
Initial update cadence: 16 Hz
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

- One full 128 × 128 compute update at 16 Hz.
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
