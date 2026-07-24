# Weather Inspector Cleanup Plan

## Status

**Patch:** `WEATHER-INSPECTOR-CLEANUP-V1.0`

**Current state:** source implementation complete; local consistency and scope audits passed; Unity compilation and Inspector validation pending.

## Objective

Replace the four inconsistent Weather custom Inspectors with one coherent authoring scheme:

- every editable control lives under a clearly named foldout;
- every foldout starts collapsed for each newly created Editor instance;
- every visible editable control has a specific tooltip;
- vague labels are replaced in the Inspector without renaming serialized fields;
- duplicate cloud-debug buttons are removed and the serialized debug dropdown becomes authoritative;
- stale refresh/report-preview controls are removed;
- derived values and architecture explanations are shown as read-only information;
- the LightRay Inspector states explicitly that V1.0 creates and renders no rays.

This patch changes Editor presentation only. It must not change Weather runtime calculations, serialized values, scenes, shaders, materials, render assets, hierarchy, layers, tags, or gameplay behavior.

## Approved files

### Create

- `Assets/Docs/Weather_Inspector_Cleanup_Plan.md`
- `Assets/Game/Procedural/Weather/Editor/WeatherInspectorGui.cs`

### Modify

- `Assets/Docs/Weather_System_Architecture_Provisional.md`
- `Assets/Game/Procedural/Weather/Editor/WeatherWindDomainEditor.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherWindTrailRendererEditor.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherCloudShadowControllerEditor.cs`
- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs`

No other file is approved.

## Reviewed evidence

### Repository rules

- `Assets/AGENTS.md` — complete current file reviewed. It requires a persistent plan before implementation, exact scope control, a post-change consistency audit, and explicit pending Unity validation. Git interaction is discouraged unless requested.

### Wind Domain Inspector

- `Assets/Game/Procedural/Weather/Editor/WeatherWindDomainEditor.cs` — complete file reviewed.
- `OnInspectorGUI` delegates all authored controls to `DrawDefaultInspector`, then renders always-expanded actions and status.
- Scene diagnostics depend on `WeatherWindDomain.DebugView`, `DebugSampleStepCells`, `DebugHeightOffset`, `DebugArrowScale`, `GetFieldWorldRectXZ`, `GetDebugAnchorPosition`, `SampleTargetWindXZ`, and editor-only response-texture readback.
- Runtime source fields and public read-only data were reviewed in `WeatherWindDomain.cs`, including domain placement, resolution/update budget, prevailing wind, broad variation, gusts, elastic response, mapping, debug controls, resource state, memory estimate, field origin, ring offset, and dispatch counters.

### Wind Trail Inspector

- `Assets/Game/Procedural/Weather/Editor/WeatherWindTrailRendererEditor.cs` — complete file reviewed.
- `showVisualCalibration`, `showPlacement`, `showSceneDiagnostics`, and `showReport` currently initialize to `true`.
- The Inspector embeds a large selectable full report and separately exposes copy behavior.
- The existing baseline migration and default-shader assignment execute before normal drawing and must be retained.
- Scene diagnostics are Editor-instance state and use `TryGetLastCandidate`, `TryGetTrailPoint`, and `GetTrailPointCount`; they must remain nonserialized and default off.
- All serialized authoring fields from `WeatherWindTrailRenderer.cs` were reviewed: rendering, capacity/cadence, strong-wind placement, camera-entry placement, candidate selection, streamline construction, lifecycle, shape, altitude, wobble, and opacity.
- Runtime status APIs reviewed: domain/camera/shader resolution, resource state, active count, candidate counts, mesh capacities, resolved lifecycle ranges, reset action, report generation, and baseline upgrade.

### Cloud Shadow Inspector

- `Assets/Game/Procedural/Weather/Editor/WeatherCloudShadowControllerEditor.cs` — complete file reviewed.
- `DrawSerializedProperties` iterates every visible serialized property without foldout organization.
- The serialized `debugVisualization` enum is duplicated by `Show Cloud Areas`, `Show Cloud / Opening Map`, and `Hide Cloud Debug Overlay` buttons.
- `Refresh Debug Focus` duplicates normal controller refresh behavior; `RefreshNow` and edit-preview ticking already resolve current state.
- The generated cookie preview, benchmark interface, actions, and status are always expanded.
- Benchmark public APIs in `WeatherCloudShadowBenchmark.cs` were reviewed; start, cancel/restore, progress, retained report, and report path behavior must remain unchanged.
- All serialized cloud fields and public status APIs in `WeatherCloudShadowController.cs` were reviewed: activation, cookie pattern, evolution, movement, debug focus, sun gate, debug visualization, cookie/evolution metrics, source resolution, error reporting, actions, and benchmark state support.

### LightRay Inspector

- `Assets/Game/Procedural/Weather/Editor/WeatherLightRayControllerEditor.cs` — complete file reviewed.
- It currently uses `DrawDefaultInspector`, exposes future V1 controls that have no V1.0 visual consumer, contains a redundant refresh button, and gives an ambiguous instruction referring to a cloud control on another component.
- `WeatherLightRayController.cs` source and public APIs were reviewed. V1.0 owns fixed storage and cloud-projection diagnostics but has no registration surface and no renderer; active count `0` is expected.
- `lightRaysEnabled` and `cloudEvolutionResumeThreshold` remain serialized for future V1 work but are hidden in this V1.0 cleanup because they do not currently control a visible ray lifecycle.

### Canonical architecture

- `Assets/Docs/Weather_System_Architecture_Provisional.md` reviewed for active Weather ownership, cloud diagnostics, benchmark ownership, and LightRay V1.0 status.
- `Assets/Docs/Weather_Wind_Architecture.md` reviewed for Wind Domain and Wind Trail diagnostic/report requirements.
- `Assets/Docs/Weather_Cloud_Shadow_Handoff.md` reviewed for the cloud debug-focus contract, generated-cookie preview, benchmark actions, report behavior, and frozen runtime architecture.
- `Assets/Docs/Weather_Light_Ray_Architecture.md` reviewed for V1.0 nonvisual status and mandatory later hybrid rendering.

## Invariants and non-goals

1. Preserve every serialized field name and every serialized value.
2. Do not add `FormerlySerializedAs`; no runtime field is renamed.
3. Do not call runtime reset/rebuild actions merely because a foldout opens or closes.
4. Preserve Wind Trail baseline migration and default-shader assignment behavior.
5. Preserve the cloud benchmark implementation and all start/cancel/copy operations.
6. Preserve all existing Scene-view diagnostic geometry and classification logic.
7. Do not add a Weather manager component or merge the four existing components.
8. Do not implement LightRay registration, spawning, rendering, gameplay influence, or cloud-clearance selection.
9. Do not modify runtime C#, shaders, HLSL, scenes, prefabs, materials, renderer assets, or project settings.
10. Foldout state is Editor-instance state only and is not serialized.

## Inspector-wide presentation contract

- The disabled Script reference remains first.
- Critical errors and multiple-publisher warnings may remain visible outside foldouts.
- Every ordinary section uses `WeatherInspectorGui.Foldout` and initializes closed.
- Every editable property uses an explicit `GUIContent` label and tooltip.
- Units are included where relevant: `(m)`, `(s)`, `(Hz)`, `(°)`, `(XZ)`.
- Derived information uses disabled read-only rows or help boxes.
- Actions and reports live under one collapsed `Actions & Reports` section per component.
- Status lives under one collapsed `Live Status` section per component.

## File-by-file implementation sequence

### 1. Shared helper — `WeatherInspectorGui.cs`

**Status:** complete at source level; Unity compilation pending.

Implement editor-only helpers for:

- disabled Script reference;
- consistent collapsed foldouts;
- serialized-property lookup and drawing with explicit label/tooltip;
- read-only text/int/float rows;
- consistent min/max field groups;
- missing-property error reporting;
- compact spacing and explanatory help boxes.

The helper contains no runtime state and no reflection over Weather components.

### 2. Wind Domain Inspector

**Status:** complete at source level; Unity compilation and visual Inspector validation pending.

Replace `DrawDefaultInspector` with collapsed sections:

- Domain Placement;
- Resolution & Update Budget;
- Base Wind;
- Broad Variation;
- Gust Regions;
- Elastic Visual Response;
- Debug & Diagnostics;
- Actions & Reports;
- Live Status.

Preserve simulation-hash comparison, rebuild requests, readback invalidation, Scene-view drawing, reset action, report copy, error warnings, and multiple-domain warning.

### 3. Wind Trail Inspector

**Status:** complete at source level; Unity compilation and visual Inspector validation pending.

Retain baseline migration and default shader assignment. Replace current groupings with collapsed sections:

- Appearance;
- Shape & Altitude;
- Lifecycle & Travel;
- Wobble & Local Shape;
- Population & Separation;
- Camera Entry Placement;
- Advanced Candidate Selection;
- Advanced Path Construction;
- Debug & Diagnostics;
- Actions & Reports;
- Live Status.

Remove the embedded full report preview. Keep one copy-report action. Make Scene diagnostics default off and retain its exact existing draw logic.

### 4. Cloud Shadow Inspector

**Status:** complete at source level; Unity compilation and visual Inspector validation pending.

Replace flat iteration with collapsed sections:

- Activation & Sun Source;
- Cloud Pattern;
- Cloud Motion;
- Pattern Evolution;
- Sun Availability Gate;
- Debug Visualization;
- Generated Cookie Preview;
- Actions & Reports;
- Performance Benchmark;
- Live Status.

Remove duplicate cloud-debug buttons and the redundant debug-focus refresh button. Keep only the serialized `debugVisualization` dropdown for normal overlay selection. Keep the conditional runtime-focus-clear action. Preserve edit-preview ticking, all cloud actions, benchmark behavior, preview drawing, and all error/warning states.

### 5. LightRay Inspector

**Status:** complete at source level; Unity compilation and visual Inspector validation pending.

Show an always-visible V1.0 foundation notice, then collapsed sections:

- Source Binding;
- Foundation Storage;
- Cloud Projection Diagnostic;
- Actions & Reports;
- Live Status.

Hide `lightRaysEnabled` and `cloudEvolutionResumeThreshold` until their runtime consumers exist. Remove the refresh button. Replace ambiguous cross-component instructions with the exact path:

`Weather Cloud Shadow Controller -> Debug Visualization -> Debug View -> Cloud + Sun Openings`.

Preserve Scene-view probe rendering and colour classification unchanged.

### 6. Architecture status update

**Status:** complete.

Add `WEATHER-INSPECTOR-CLEANUP-V1.0` to the active Weather architecture status and record that this patch changes Editor presentation only.

## Risks and mitigations

| Risk | Evidence | Mitigation |
|---|---|---|
| Missing a serialized field hides authored functionality | Current Cloud and Wind Domain Inspectors expose all visible fields automatically | Enumerate every serialized authored field from the runtime sources and perform an exact field-coverage audit after implementation. Future-only hidden LightRay fields are the only approved exceptions. |
| Inspector edits trigger unintended rebuilds | Wind Domain compares a simulation configuration hash; Cloud and LightRay call `RefreshNow` after changes | Preserve the same change-detection flow and action calls. Foldout state is not serialized and cannot trigger change handling. |
| Wind Trail scene tuning migrates unexpectedly | Existing editor runs baseline migration and default-shader assignment before drawing | Preserve both methods and their ordering unchanged. |
| Cloud benchmark controls regress | Benchmark uses EditorPrefs plus static runner APIs | Preserve setting keys, clamping, save/load, start, cancel/restore, progress, retained report, and path display. |
| Scene diagnostics disappear | Diagnostics are implemented in editor callbacks, not runtime fields | Preserve all callback registration and existing draw routines. |
| A custom label changes serialized data | Labels are presentation only | Use `SerializedProperty` with explicit `GUIContent`; do not rename fields. |
| Shared helper creates cross-component coupling | Helper is new | Keep it stateless and editor-only; each Inspector owns its section state and runtime decisions. |

## Acceptance criteria

- [ ] Selecting the Weather GameObject shows compact Weather components whose internal foldouts all begin collapsed. **Unity validation pending.**
- [x] Every source-exposed editable field has an explicit tooltip in the custom Inspectors. **Unity presentation validation pending.**
- [x] Cloud Debug View is the only ordinary control for showing or hiding the cloud overlay.
- [ ] No new serialized scene value changes occur merely from opening foldouts or viewing the Inspectors. The retained pre-existing Wind Trail baseline migration and missing-shader recovery remain the explicit exceptions. **Unity validation pending.**
- [x] Every existing action remains available under a clear actions or benchmark foldout, except the approved redundant refresh controls.
- [x] Wind Trail baseline migration and default shader assignment remain active in the same pre-draw order.
- [x] No runtime, shader, scene, material, renderer, or project-setting file changed; live behavior validation remains pending in Unity.
- [x] LightRay clearly states that V1.0 creates and renders no rays.
- [ ] The project compiles with zero errors. **Unity validation pending.**
- [ ] Scene diagnostics, report copying, benchmark controls, reset actions, and cloud preview continue to work. **Unity validation pending.**
- [x] Final scope contains exactly the seven approved files.

## Validation and compliance ledger

| Check | Status | Evidence / next action |
|---|---|---|
| Read-only source and architecture review | Passed | Evidence recorded above from the supplied archive plus the LightRay V1.0 files and compile fix already produced in this thread. |
| Persistent implementation plan created before code edits | Passed | This document is the first source change in the working copy. |
| Exact serialized-field coverage audit | Passed | Wind Domain 27/27; Wind Trail 50/50 authored fields plus one intentionally hidden migration field; Cloud 35/35; LightRay 11/11 visible V1.0 fields plus the two approved future-only hidden fields. |
| Static C# structure and reference scan | Passed with Unity limitation | Balanced delimiters, strings, comments, and preprocessor structure; every referenced Weather public API and serialized property was confirmed in the supplied source. Unity assemblies are unavailable, so this is not a Unity compile result. |
| Final diff scope audit | Passed | The diff contains exactly the seven approved files. |
| Runtime/non-Editor byte-identity audit | Passed | All files outside the two approved documents and five Editor files are byte-identical to the authoritative current input. |
| Unity compilation | Pending | User must compile in Unity 6000.5.0f1. |
| Inspector visual validation | Pending | User must verify collapsed defaults, tooltips, actions, and diagnostics in Unity. |

## Next work items

1. Compile the patch in Unity 6000.5.0f1.
2. Verify all Weather foldouts start collapsed and every exposed control shows its tooltip.
3. Verify Cloud Debug View is the sole normal overlay control and the overlay still follows its selected mode.
4. Verify Wind and Wind Trail Scene diagnostics, reset actions, copied reports, cookie preview, and benchmark controls still work.
5. Confirm opening and closing foldouts does not dirty the scene or change serialized values.
