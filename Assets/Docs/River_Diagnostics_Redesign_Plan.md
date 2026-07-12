# River Diagnostics and Inspector Redesign Plan

> **Document status:** Temporary implementation plan.
>
> **Created:** 2026-07-12.
>
> **Authority:** This document owns the River Inspector and diagnostics redesign until the redesign is implemented, Unity-validated, accepted, and recorded in the canonical River documents.
>
> **Required cleanup:** Delete this document and its `.meta` file after the accepted redesign has been summarized in the appropriate canonical River documents. It must not become a permanent parallel architecture document.

---

## 1. Purpose

The `StylizedRiver` Inspector has accumulated production controls, debug selectors, runtime telemetry, test harnesses, cache tools, validation warnings, generated-state summaries, and historical explanatory text in one continuous custom Inspector.

The immediate objective is to redesign that Inspector before adding the planned Foam Presence capacity-loss diagnostics.

The redesign must make the River tooling:

- quiet and compact by default;
- organized by feature ownership;
- explicit about editable versus read-only information;
- consistent with the accepted Foam Layer A–E architecture;
- safe for existing serialized rivers;
- easier to extend without restoring the current diagnostic bloat;
- cheaper to repaint while diagnostics are not being viewed.

This is an Editor-tooling redesign. It must not alter river generation, rendering, simulation, foam transport, lifecycle, source behavior, shader output, tuning defaults, or serialized values unless the user explicitly changes a control.

---

## 2. Relationship to canonical River documents

Canonical architecture remains in:

```text
Assets/Docs/River_Rendering_Roadmap.md
Assets/Docs/River_Foam_Stage6_Architecture.md
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/Proof of Concept/08_Proof_of_Concept_Implementation_Log.md
Assets/Docs/Proof of Concept/09_Rock_And_River_Handoff.md
```

This temporary document does not replace those files. It defines only the implementation plan for restructuring River authoring and diagnostic tooling.

After the redesign is accepted:

1. Record the resulting Inspector/debug contract concisely in the canonical documents.
2. Remove obsolete Inspector descriptions from those documents if necessary.
3. Delete this temporary plan and its `.meta` file.

---

## 3. Scope

### 3.1 Included

The redesign includes:

- all major River features, not only Foam;
- closed-by-default top-level and nested foldouts;
- separation of authoring, debug presentation, runtime diagnostics, generated status, and actions;
- Foam authoring and diagnostics organized by accepted Layers A–E;
- one central, exclusive River debug-view controller;
- explicit handling of legacy scenes with multiple debug enums active;
- standardized read-only diagnostic rows;
- stable Inspector height while live values update;
- narrower constant-repaint ownership;
- removal of duplicate controls and duplicate debug selectors;
- removal of stale heuristic navigation and patch-history prose from the Inspector;
- maintainable Editor source organization;
- preservation of multi-object editing and Undo behavior.

### 3.2 Explicitly excluded

The redesign must not include:

- Foam Presence capacity-loss attribution;
- Foam transport or lifecycle changes;
- source birth changes;
- debug-enum deletion or renumbering;
- shader-branch removal;
- new simulation fields, textures, buffers, kernels, or dispatches;
- Layer D production promotion;
- material/default retuning;
- scene, prefab, material, profile, or cache migration;
- new runtime components, layers, tags, or dependencies;
- Ground or Generated Mass changes.

The Presence investigation resumes only after this redesign is accepted.

---

# 4. Audited current state

## 4.1 The Inspector is a monolith

Current implementation:

```text
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs
```

The supplied source is `3,988` lines and references `269` distinct serialized properties through `Find("...")` calls.

That file owns all of the following:

```text
River setup
River domain
Channel geometry
Natural variation
Shoreline safety
Surface mesh
Surface motion
Refraction
Runtime disturbances
Foam
Water body
Generated status
Runtime telemetry
Debug view selection
Cache tooling
Test harnesses
Generation actions
```

The problem is not the existence of many controls. The problem is that distinct ownership categories are interleaved in one visual and code hierarchy.

**Finding:** Proven.

**Confidence:** High.

---

## 4.2 Major features are always drawn

`OnInspectorGUI()` currently calls the major draw methods unconditionally:

```csharp
DrawSetup();
DrawRiverDomain();
DrawChannel();
DrawNaturalVariation();
DrawAdvancedShoreline();
DrawSurfaceMesh();
DrawSurfaceMotion();
DrawRefraction();
DrawRuntimeDisturbances();
DrawFoam();
DrawWaterBody();
DrawAdvancedBody();
```

Source:

```text
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs:82-109
```

Only selected subsections use foldouts. The real systems remain permanently visible, producing a very long Inspector before any detailed diagnostics are opened.

**Finding:** Proven.

**Confidence:** High.

---

## 4.3 Existing Foam foldouts are open by default

The current Editor initializes these foldouts as open:

```csharp
private bool showFoamValidationOverview = true;
private bool showFoamManualBirthSource = true;
private bool showFoamAutomaticSourcePopulation = true;
private bool showFoamBirthShoreFoam = true;
```

Source:

```text
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs:11-37
```

A newly opened Inspector therefore exposes live overview telemetry, the manual birth harness, automatic-source controls, and Shore Foam controls without deliberate user action.

This violates the required quiet-by-default behavior.

**Finding:** Proven.

**Confidence:** High.

---

## 4.4 Authoring foldouts can trigger continuous repaint

`RequiresConstantRepaint()` currently returns true when several Foam authoring or test foldouts are open, including:

```text
Manual Birth Source
Automatic Source Population
```

Source:

```text
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs:46-70
```

The runtime reports repaint eligibility whenever Foam state or related activity exists:

```text
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs:640-652
```

Because some of those foldouts are open by default, the Inspector can repaint continuously while showing controls whose labels do not require live updates.

The redesign must make constant repaint depend only on visible live diagnostics or genuinely animated previews.

**Finding:** Proven.

**Confidence:** High.

---

## 4.5 Authoring controls are duplicated inside diagnostics

The main Foam authoring section draws:

```text
Neutral Lifetime
Supported Aging Rate
Final Foam Visibility Mode
Negative Aging Rate
```

The `Lifetime + Topology` diagnostic section draws the same editable controls again.

This causes duplicate ownership and puts `Final Foam Visibility Mode`, a Layer E rendering control, inside a lifecycle diagnostic section.

The redesigned Inspector must expose every production setting in one authoritative authoring location only. Diagnostics may display resolved values, but those rows must be read-only.

**Finding:** Proven.

**Confidence:** High.

---

## 4.6 Foam debug selection is implemented twice

The full Foam debug-view popup is implemented in both:

```text
DrawFoamViewModeSection()
DrawFoamMaterialProbeSection()
```

Both methods maintain their own label arrays, enum-value arrays, selection lookup, and popup logic.

The redesign must have one authoritative debug selector. A test/probe section may show its recommended view and offer an explicit activation action, but it must not duplicate the full selector.

**Finding:** Proven.

**Confidence:** High.

---

## 4.7 Five independent debug systems compete for one shader result

The River serializes five debug-view enums:

```text
StylizedRiverBodyDebugView
StylizedRiverMotionDebugView
StylizedRiverRefractionDebugView
StylizedRiverDisturbanceDebugView
StylizedRiverFoamDebugView
```

Source:

```text
Assets/Game/Procedural/Rivers/StylizedRiver.cs:100-201
```

Together they expose `49` non-Final diagnostic modes:

```text
Water Body     12
Surface Motion  5
Refraction      6
Disturbances   10
Foam           16
```

The shader resolves these systems sequentially through early returns:

```text
Foam
Disturbances
Refraction
Surface Motion
Water Body
```

Relevant source range:

```text
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader:851-1587
```

Therefore multiple serialized debug selections can be active while only the highest-priority one is visible. Lower-priority selections then appear broken or ineffective.

The current Inspector exposes these selectors in separate feature sections and provides neither exclusivity nor a central explanation of hidden active modes.

**Finding:** Proven.

**Confidence:** High.

---

## 4.8 Authoring, telemetry, previews, and actions are interleaved

Examples:

- River Domain mixes authoring, runtime/generated status, component setup, and contract validation.
- Runtime Disturbances mixes Pressure/Wake/Ripple authoring, debug selection, live field metrics, memory estimates, clear actions, and test-impact actions.
- Foam mixes topology authoring, velocity authoring, lifecycle controls, source population controls, cache generation, live metrics, debug selection, candidate previews, manual material injection, isolated life probes, and resource diagnostics.

The redesign must separate these by purpose rather than by the order in which they were historically added.

**Finding:** Proven.

**Confidence:** High.

---

## 4.9 Inspector prose contains patch history

The Foam section includes long HelpBoxes referencing historical patch identifiers and development workflow history, for example `Stage 6.2` and `Patch 4.9C.1`.

Historical patch context belongs in documentation. Inspector text should explain the current contract, required setup, or immediate consequence of a control.

**Finding:** Proven.

**Confidence:** High.

---

## 4.10 Heuristic navigation is stale

`ResolveFoamLikelyProblem()` can return:

```text
Open Material Motion
```

while the active UI terminology has moved toward `Foam Velocity`.

The heuristic also depends on hard-coded thresholds and section names. It duplicates information already present in runtime status and creates another maintenance surface.

The redesign should remove the `Next Debug Section` recommendation instead of porting it.

Source:

```text
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs:3224-3256
```

**Finding:** Proven.

**Confidence:** High.

---

## 4.11 Dead Editor state is present

`showFoamDiagnostics` is declared but unused.

This is a small issue, but it confirms that the current structure has accumulated obsolete state.

**Finding:** Proven.

**Confidence:** High.

---

# 5. Redesign principles

## 5.1 Quiet by default

Every top-level and nested feature foldout must be collapsed when the Inspector instance is created.

No production setting, test harness, debug selector, or diagnostic report should begin expanded.

The permanent header may show only a compact identity and a one-line state summary.

---

## 5.2 One location per responsibility

Each item must belong to exactly one of these categories:

```text
Authoring
Debug presentation
Runtime diagnostics
Generated status
Actions and test tools
```

A setting must not be repeated inside diagnostics. A debug selector must not be repeated inside a probe. A test action must not appear among production controls.

---

## 5.3 Organize Foam by accepted Layers A–E

Foam authoring and diagnostics must use the accepted architecture:

```text
Layer A — Topology and Support
Layer B — Canonical Velocity
Layer C — Persistent Material and Lifecycle
Layer D — Evaluated Visual Shape
Layer E — Shader Composition
```

Cross-layer runtime resources belong under a separate `Runtime & Quality` or `Runtime Resources` group rather than being forced into a numbered layer.

---

## 5.4 One debug presentation authority

The Inspector must present one exclusive River debug controller.

The existing serialized debug enums remain intact for compatibility, but choosing a debug view through the new controller sets every other subsystem to `Final`.

This prevents invisible conflicting selections without requiring a runtime or shader migration.

---

## 5.5 Diagnostics are read-only

All runtime and generated-state values must use a standard read-only row style.

Editable `PropertyField` controls must not appear in diagnostic foldouts.

---

## 5.6 Live diagnostic layouts remain stable

Opening a diagnostic panel may create a fixed list of rows. Asynchronous or conditional data changes the row values, not the number or position of rows.

Unavailable values display `—`, `Unavailable`, or `Awaiting readback` in the existing row.

Warnings should normally occupy a fixed status row rather than appearing and disappearing as variable-height HelpBoxes.

---

## 5.7 Existing serialized data remains authoritative

The redesign is Editor-only in its first phase.

It must not:

- rename serialized fields;
- renumber enum values;
- force-migrate scene data;
- alter values merely because the Inspector opened;
- silently clear legacy debug conflicts.

---

## 5.8 Extension must have an ownership rule

New diagnostics may be added only under the owning feature and layer.

The planned Presence capacity-loss attribution belongs under:

```text
Runtime Diagnostics
  Foam
    Layer C — Persistent Material and Lifecycle
      Transport Accounting
```

It must not create another top-level Foam debug block.

---

# 6. Target top-level Inspector hierarchy

The permanent header remains compact:

```text
Stylized River
State: Ready / Requires Regeneration / Runtime Active / Setup Error
```

Everything else is a closed foldout:

```text
▸ Setup
▸ River Domain
▸ Channel Shape
▸ Shoreline Safety
▸ Natural Variation
▸ Surface Mesh
▸ Water Body & Lighting
▸ Surface Motion
▸ Refraction
▸ Runtime Disturbances
▸ Foam
▸ Debug Views
▸ Runtime Diagnostics
▸ Generated Status
▸ Actions
```

This order follows the production dependency and authoring flow:

```text
Setup
→ geometry and banks
→ water body appearance
→ motion
→ optics
→ interaction fields
→ foam
→ debug and diagnostics
→ actions
```

`Water Body & Lighting` moves before Motion, Refraction, Disturbances, and Foam. The current placement after Foam is not retained.

---

# 7. Authoring hierarchy by feature

## 7.1 Setup

Contains only:

```text
Spline Container
Live Regeneration
```

No status, validation button, or generated metric belongs here.

---

## 7.2 River Domain

Contains only:

```text
Sample Spacing
Reverse Flow
Connected Distance Offset
```

Move elsewhere:

```text
Domain State                 → Runtime Diagnostics / Domain & Geometry
Actual Spacing               → Runtime Diagnostics / Domain & Geometry
Global Range                 → Runtime Diagnostics / Domain & Geometry
Proof Harness                → Actions / Domain Validation
Validate Domain Contract     → Actions / Domain Validation
```

---

## 7.3 Channel Shape

Contains:

```text
Water Width
Bed Depth
Bed Flatness
Bank Blend
Bank Profile
Terrain Conformity
```

Generated widths, corridor handoff measurements, and derived geometry values move to diagnostics or generated status.

---

## 7.4 Shoreline Safety

The existing advanced shoreline/corridor values become a normal, closed feature foldout:

```text
Additional Shoreline Overlap
Wet Clearance
Bank Cover
Reserved Downward Displacement
```

These are meaningful integration controls and should not be hidden under an ambiguous `Advanced` section.

---

## 7.5 Natural Variation

Contains only variation authoring controls.

Resolved geometry values such as effective bed roughness or visible width move to read-only diagnostics.

---

## 7.6 Surface Mesh

Contains:

```text
Quality
Surface Offset
```

Generated mesh counts and memory estimates belong under `Generated Status`.

---

## 7.7 Water Body & Lighting

Nested foldouts, all closed:

```text
▸ Surface State
▸ Liquid Body
▸ Frozen Body
▸ Lighting Response
▸ Advanced Material
```

`Body Debug View` moves to the central `Debug Views` hub.

Material override and highly specialized response controls belong under `Advanced Material`, not under a validation-labelled section.

---

## 7.8 Surface Motion

Nested foldouts, all closed:

```text
▸ General Flow
▸ Macro Waves
▸ Detail Motion
▸ Current Accents
▸ Shore Motion
▸ Shore Wave Profile
```

Move elsewhere:

```text
Motion Debug View             → Debug Views / Surface Motion
Resolved Surface Row Spacing  → Runtime Diagnostics / Domain & Geometry
Resolved Downward Clearance   → Runtime Diagnostics / Domain & Geometry
```

---

## 7.9 Refraction

Nested foldouts, all closed:

```text
▸ Liquid Refraction
▸ Shore & Depth Protection
▸ Frozen Distortion
```

`Refraction Debug View` moves to the central Debug Views hub.

---

## 7.10 Runtime Disturbances

Nested authoring groups, all closed:

```text
▸ Master & Preset
▸ Pressure Response
▸ Wake Response
▸ Impact Ripples
```

Remove from authoring:

```text
Disturbance Debug View
Runtime status rows
Dispatch counts
Source counts
Dirty/rebuild state
Memory estimates
Clear action
Test impact actions
```

Move them to:

```text
Debug Views / Disturbances
Runtime Diagnostics / Disturbances
Actions / Disturbance Test Events
Actions / Runtime Clear and Reset
```

---

# 8. Foam authoring hierarchy

The top-level `Foam` foldout contains only production authoring organized by architectural ownership.

```text
▸ Layer A — Topology & Support
▸ Layer B — Canonical Velocity
▸ Layer C — Persistent Material & Lifecycle
▸ Layer D — Evaluated Shape
▸ Layer E — Rendering
▸ Runtime & Quality
```

Every layer and nested subsection starts closed.

---

## 8.1 Layer A — Topology & Support

### Cache assignment

```text
Topology Cache Asset
```

Cache build state, hash information, candidate previews, resource counts, and validation results do not belong in authoring.

### Major Support

```text
Amount
Size
Size Variation
Recycle Territory Deviation
Lifetime Units
Lifetime Unit Deviation
Seed
```

### Connectors

```text
Amount
Directness
Length Preference
Break Stretch Ratio
```

### Negative Topology

```text
Interior Pocket Amount
Edge Cavity Amount
Connector Weak Span Amount
Free-Water Event Amount
```

Cache diagnostics and candidate previews move to:

```text
Runtime Diagnostics / Foam / Layer A
Actions / Foam Layer A Cache Tools
```

---

## 8.2 Layer B — Canonical Velocity

Production controls:

```text
Downstream Speed Ratio
Maximum Lateral Speed Ratio
Lane Advection Ratio
Direction Change Frequency
Across-River Coherence
Low Lateral Motion Coverage
Obstacle Slowdown Strength
Obstacle Minimum Downstream Factor
```

These are currently buried among Foam debug tooling. They must become ordinary Layer B authoring controls.

Runtime CFL, substep, resolved speed, lane phase, and cell-travel values remain read-only diagnostics.

---

## 8.3 Layer C — Persistent Material & Lifecycle

Nested foldouts:

```text
▸ Lifecycle
▸ Automatic Birth Sources
▸ Source Pattern Tuning
```

### Lifecycle

One authoritative editable copy of:

```text
Neutral Lifetime
Supported Aging Rate
Negative Aging Rate
```

The same fields must not appear again inside runtime diagnostics.

### Automatic Birth Sources

```text
Automatic Foam Birth
Spawn Preset
```

Nested source categories:

```text
▸ Shore Sources
▸ Object Sources
▸ Free-Water Sources
```

Each category owns only its relevant production settings.

### Pattern detail hierarchy

Pattern-specific controls remain available but are nested under closed pattern-detail foldouts. Example:

```text
Shore Sources
  Coverage
  Activity
  Global Size
  Formation Speed
  Pattern Mode
  Pattern Mix

  ▸ Shore Ribbon Details
  ▸ Inward Wash Details
```

Equivalent nested detail groups apply to Object and Free-Water patterns.

### Manual source tools

The Manual Birth Source interface is not production authoring. Move it to:

```text
Actions
  Foam Layer C Test Sources
```

---

## 8.4 Layer D — Evaluated Shape

Production/experimental authoring controls:

```text
Visual Occupancy Build Time
Visual Occupancy Release Time
```

Layer D area ratios, temporal comparisons, source/support/target state, resource status, and shape-difference reports remain diagnostics.

---

## 8.5 Layer E — Rendering

Authoring controls:

```text
Final Foam Visibility Mode
Foam Colour
```

Future accepted shader-local controls such as Interior Fill Strength or Micro Tear Strength belong here.

`Final Foam Visibility Mode` must not remain in lifecycle diagnostics.

---

## 8.6 Runtime & Quality

This group contains only real production scheduling or quality controls that support multiple Foam layers.

Read-only resource allocation, texture formats, dispatch state, and memory values do not belong here. They move to `Runtime Diagnostics / Foam / Runtime Resources`.

---

# 9. Central Debug Views hub

## 9.1 Required UI

One top-level foldout:

```text
▸ Debug Views
```

Inside:

```text
Debug Feature
Debug Layer or Category
Debug View
Active Rendered View
Description
```

`Debug Layer or Category` appears only when useful for the selected feature. For Foam it exposes Layers A–E and advanced comparison groups. For simpler systems it may be omitted.

A permanent action is available inside the foldout:

```text
Reset All Debug Views
```

---

## 9.2 Exclusive selection behavior

The existing serialized fields remain the storage authority:

```text
bodyDebugView
motionDebugView
refractionDebugView
disturbanceDebugView
foamDebugView
```

When the user selects a non-Final view through the new hub:

1. Set the selected feature's existing enum field to the selected view.
2. Set the other four debug enum fields to `Final`.
3. Apply through `SerializedObject` so Undo, prefab overrides, and multi-object editing continue to work.

When the user selects `Final Render`:

1. Set all five fields to `Final`.

No new serialized master debug enum is required for the first redesign.

---

## 9.3 Legacy conflict handling

Existing scenes may contain more than one non-Final debug field.

Opening the Inspector must not silently change those fields.

When multiple active fields are detected, show fixed read-only rows:

```text
Conflict State        Multiple debug views are active
Rendered View         Foam / Material Presence
Hidden Active Views   Water Body / Vertical Depth; Motion / Macro Height
Shader Priority       Foam overrides Disturbances, Refraction, Motion, and Body
```

Provide one explicit action:

```text
Normalize to Rendered View
```

That action keeps the currently visible highest-priority view and resets hidden views to Final.

Once the user chooses any view through the new hub, exclusivity becomes automatic.

---

## 9.4 Multi-object editing

The custom Editor supports `[CanEditMultipleObjects]` and must continue to do so.

Requirements:

- use `SerializedProperty.hasMultipleDifferentValues` for debug fields;
- display a mixed state when selected rivers differ;
- selecting a view applies the exclusive configuration to all selected rivers with Undo support;
- `Reset All Debug Views` applies to all selected rivers;
- per-runtime telemetry does not attempt to merge unrelated runtimes;
- when multiple rivers are selected, runtime diagnostic values display `Select one river for live diagnostics` while authoring remains editable.

---

# 10. Debug-view catalog and bloat policy

## 10.1 Water Body

Keep all existing views during the first redesign:

```text
Vertical Depth
Depth Blend
Transmission
Body Coverage
Scene Colour
Depth Validity
Surface Coverage
Combined Lighting
Ambient Lighting
Sun Lighting
Local Lighting
Freeze Amount
```

They move from Water Body authoring to `Debug Views / Water Body`.

No enum or shader branch is removed in this redesign.

---

## 10.2 Surface Motion

Keep:

```text
Bank Mask
Macro Height
Surface Normal
Current Accent
Liquid Factor
```

They move to `Debug Views / Surface Motion`.

---

## 10.3 Refraction

Keep:

```text
Refracted Scene
Offset
Depth Influence
Shore Mask
Sample Validity
Ice Diffusion
```

They move to `Debug Views / Refraction`.

---

## 10.4 Disturbances

Keep:

```text
Height
Velocity
Normal
Intensity
Field Coordinates
Static Pressure Target
Static Wake Source
Wake Energy
Final Wake Geometry Height
Ripple Boundary
```

Suggested categories:

```text
Primary Field
  Height
  Velocity
  Normal
  Intensity
  Field Coordinates

Static Pressure and Wake
  Static Pressure Target
  Static Wake Source
  Wake Energy
  Final Wake Geometry Height

Ripple Validation
  Ripple Boundary
```

---

## 10.5 Foam by layer

### Layer A — Topology

```text
Foam + Aging Topology
```

### Layer B — Velocity

```text
Foam Motion Field
Foam Motion Field + Cell Grid
```

### Layer C — Persistent Material

```text
Material Presence
Material Remaining Life
Progressive Birth Source
```

`Progressive Birth Source` must be labelled as a test-source view rather than general material truth.

### Layer D — Evaluated Shape

Primary:

```text
Foam Evaluated Shape
Foam Evaluated Final Preview
```

Advanced internals:

```text
Foam Film Source
Foam Film Support
Foam Instantaneous Film Target
Foam Temporal Occupancy
```

Comparisons:

```text
Foam Shape Difference
Foam Temporal Difference
```

### Layer E — Rendering

```text
Foam Shader Detail Probe
Foam Shader Detail Difference
```

### Final

```text
Final Foam
```

---

## 10.6 First-redesign retirement policy

Do not delete debug enum values or shader branches in the first redesign.

Instead, debloat by hierarchy:

- primary views are immediately visible within their category;
- internal views are under `Advanced Internals`;
- difference views are under `Comparisons`;
- test-source views are labelled explicitly;
- one selector replaces all duplicate selectors.

This preserves investigative capability for the upcoming Presence work.

---

## 10.7 Later retirement candidates

After the redesigned tools have been used in practice, a separate cleanup may consider:

### Motion Field + Cell Grid

Possible future representation:

```text
View: Motion Field
Overlay: Cell Grid
```

This would require runtime/shader interface work and is therefore excluded now.

### Difference views

```text
Shape Difference
Temporal Difference
Shader Detail Difference
```

These remain valuable but should be assessed for distinct evidence after the redesign.

### Layer D internals

```text
Film Source
Film Support
Film Target
Temporal Occupancy
```

Keep while Layer D remains an active diagnostic comparison architecture.

### Views that must remain through Presence troubleshooting

```text
Material Presence
Material Remaining Life
Foam Motion Field
Foam + Aging Topology
Foam Evaluated Shape
Foam Evaluated Final Preview
```

---

# 11. Runtime Diagnostics hub

One top-level closed foldout:

```text
▸ Runtime Diagnostics
```

All content is read-only.

When not in Play Mode, runtime-only rows remain present and display `Not in Play Mode` or `—` as appropriate.

When multiple rivers are selected, live panels display `Select one river for live diagnostics` without hiding or resizing the section.

---

## 11.1 Domain & Geometry

```text
Domain State
Sample Count
Actual Spacing
Global Range
Generated Water Width
Collider Handoff Width
Integration Apron
Corridor Render Width
Surface Row Spacing
Downward Motion Clearance
```

---

## 11.2 Disturbances

Nested closed groups:

```text
▸ Summary
▸ Dispatches
▸ Sources & Rebuild State
▸ Memory
```

This receives all live status currently interleaved with Disturbance authoring.

---

## 11.3 Foam

Nested closed groups:

```text
▸ Summary
▸ Layer A — Topology & Cache
▸ Layer B — Velocity
▸ Layer C — Material & Lifecycle
▸ Layer D — Evaluated Shape
▸ Runtime Resources
▸ Advanced Internals
```

### Summary

Keep only high-value overview rows:

```text
State
Active Rendered Debug View
Stored Material Area
Visible Foam Area
Transport Status
Primary Warning
```

Remove the heuristic:

```text
Next Debug Section
```

### Layer A — Topology & Cache

```text
Cache State
Cache Validation
Cache Hash / Revision
Candidate Counts
Support / Negative Topology Metrics
Topology Readback State
```

### Layer B — Velocity

```text
Resolved Speed
Lane Phase
Material Tick Rate
Maximum CFL
Substeps Used / Required
Cell Travel
Safety Status
```

### Layer C — Material & Lifecycle

Subgroups:

```text
▸ Transport Accounting
▸ Lifecycle Authority
▸ Birth Activity
▸ Isolated Probe State
```

Transport Accounting owns:

```text
Presence Accounting
Life Moment Accounting
Pattern Moment Accounting
Unaccounted Error
Capacity / Clamp Loss
```

The planned Presence capacity-loss attribution will extend this exact subgroup.

Lifecycle Authority owns read-only resolved status. It must not redraw editable lifecycle controls.

### Layer D — Evaluated Shape

```text
Stored / Visible Area
Perimeter Ratio
Temporal Sheet State
Shape Comparison State
Source / Support / Target State
```

### Runtime Resources

```text
Layer C State Textures
Layer D Shape Textures
Chunk / Runtime State
Dispatch State
Estimated Memory
```

### Advanced Internals

Only low-frequency or highly specialized telemetry belongs here. It starts closed and must not be used as a dumping ground for ordinary metrics.

---

# 12. Generated Status hub

One closed foldout:

```text
▸ Generated Status
```

Contains read-only generation results that exist outside Play Mode as well as during runtime:

```text
Surface Mesh State
Vertex Count
Triangle Count
Bounds
Corridor State
Collider Handoff State
Material / Shader Compatibility
Regeneration Requirement
```

This prevents generated-state labels from being scattered through authoring sections.

---

# 13. Actions hub

One top-level closed foldout:

```text
▸ Actions
```

Nested closed groups:

```text
▸ Generation
▸ Domain Validation
▸ Disturbance Test Events
▸ Foam Layer A Cache Tools
▸ Foam Layer C Test Sources
▸ Foam Lifecycle Probes
▸ Runtime Clear & Reset
```

## 13.1 Generation

```text
Regenerate
Clear Generated
```

## 13.2 Domain Validation

```text
Add Domain Proof Harness
Validate Domain Contract
```

## 13.3 Disturbance Test Events

```text
Emit Test Impact
Emit Opposite-Sign Impact
Emit Overlapping Pair
Emit Near Shore
```

## 13.4 Foam Layer A Cache Tools

```text
Create Cache Asset
Build / Update Cache
Validate Assigned Cache
Candidate Preview Controls
```

## 13.5 Foam Layer C Test Sources

```text
Emit Patch
Start Progressive Ribbon
Stop Source
```

## 13.6 Foam Lifecycle Probes

```text
Clear + Emit Configured Life Probe
Clear + Emit Absolute 1-Second Probe
```

## 13.7 Runtime Clear & Reset

```text
Clear Disturbance Field
Clear Foam Material
Reset All Debug Views
```

Actions that modify state must have clear tooltips and appropriate disabled states. Destructive actions may use a warning or confirmation where justified.

---

# 14. Read-only diagnostic presentation standard

Create one shared Editor helper conceptually equivalent to:

```csharp
DrawReadOnlyRow(
    GUIContent label,
    string value,
    MessageType? status = null);
```

The exact signature may differ, but all read-only telemetry must follow one visual contract.

## 14.1 Row rules

- left column: stable labelled key;
- right column: non-editable value;
- tooltip on every non-obvious metric;
- explicit units;
- consistent precision;
- copyable values where practical;
- `—` for not applicable or unavailable data;
- `Awaiting readback` for pending asynchronous diagnostics;
- no disabled text fields that look editable;
- no `PropertyField` for runtime telemetry.

## 14.2 Stable-height rules

Do not conditionally insert and remove ordinary rows.

Example while a readback is pending:

```text
Presence Accounting     Awaiting readback
Life Accounting         —
Pattern Accounting      —
Capacity Loss            —
```

After completion, only values change.

Warnings should normally use fixed rows:

```text
Primary Warning         Presence capacity loss exceeds 0.10%
```

Use variable-height HelpBoxes only for:

- missing required setup;
- destructive-action warnings;
- errors that require explanatory remediation;
- legacy debug conflicts when the fixed rows cannot convey sufficient detail.

---

# 15. Foldout-state strategy

Do not continue adding one Boolean field per foldout.

Use an Editor-only section identifier:

```csharp
private enum InspectorSection
{
    Setup,
    RiverDomain,
    ChannelShape,
    ShorelineSafety,
    NaturalVariation,
    SurfaceMesh,
    WaterBody,
    SurfaceMotion,
    Refraction,
    Disturbances,
    Foam,
    DebugViews,
    RuntimeDiagnostics,
    GeneratedStatus,
    Actions,
    // Nested sections...
}
```

Track open sections in an instance-owned set:

```csharp
HashSet<InspectorSection> openSections;
```

Rules:

- absence means closed;
- the set starts empty;
- foldout state is not serialized into `StylizedRiver`;
- foldout state is not static across Inspectors;
- reopening/recreating the Inspector returns to the required collapsed default;
- multi-object selection uses the same Editor-only visibility state.

This replaces the current collection of `show...` Boolean fields.

---

# 16. Constant-repaint strategy

`RequiresConstantRepaint()` must no longer depend on production-authoring foldouts.

It may return true only when all relevant conditions are satisfied:

1. Play Mode is active.
2. Exactly one river is selected.
3. The owning runtime exists.
4. A visible panel contains live values or an animated preview.
5. The runtime reports state that can meaningfully change.

Candidate live panels:

```text
Runtime Diagnostics / Disturbances
Runtime Diagnostics / Foam
Actions / Foam Layer A Candidate Preview
```

Ordinary authoring foldouts must never request constant repaint merely because they are open.

The redesign should preserve the runtime's existing eligibility signal initially and narrow Editor-side consumption. Runtime API cleanup may follow separately if the signal is still too broad.

---

# 17. Editor source organization

The current `3,988`-line Editor should be split into partial Editor-only files in the existing folder.

Proposed files:

```text
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Authoring.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Disturbances.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Diagnostics.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.UI.cs
```

All new files require Unity `.meta` files.

## 17.1 `StylizedRiverEditor.cs`

Owns:

```text
CustomEditor attributes
Editor instance state
OnInspectorGUI
Top-level ordering
SerializedObject update/apply flow
OnDisable cleanup
RequiresConstantRepaint routing
```

## 17.2 `Authoring.cs`

Owns:

```text
Setup
Domain
Channel
Shoreline safety
Variation
Surface mesh
Water body
Motion
Refraction
```

## 17.3 `Disturbances.cs`

Owns Disturbance authoring groups.

## 17.4 `Foam.cs`

Owns Foam Layers A–E production authoring and automatic-source pattern controls.

## 17.5 `DebugViews.cs`

Owns:

```text
Central debug hub
View catalogs
Layer/category mapping
Description text
Conflict detection
Exclusive selection
Reset and normalize behavior
```

## 17.6 `Diagnostics.cs`

Owns all read-only Domain, Disturbance, Foam, generated-state, resource, memory, and status panels.

## 17.7 `Actions.cs`

Owns generation, validation, cache, test-event, source, probe, clear, and reset actions.

## 17.8 `UI.cs`

Owns shared Editor-only presentation helpers:

```text
Foldouts
Read-only rows
Min/max controls
Indented groups
Fixed status rows
Section spacing
Mixed-value handling helpers
```

This is a code-organization change only. It must not move runtime classes or create a new folder.

---

# 18. Implementation sequence

The redesign may be delivered as one reviewed patch, but implementation should proceed in this order to reduce risk.

Current implementation progress:

```text
R1 — Structural shell: Unity-validated and accepted
R2 — Authoring ownership: Unity-validated and accepted
R3 — Central debug hub: Unity-validated and accepted
R4 — Diagnostics and actions: Unity-validated and accepted
R5 — Repaint and code split: implemented; Unity validation pending
```

R1 established the closed-by-default structural shell and was Unity-validated by the user. R2 established authoring ownership and was also Unity-validated: non-Foam systems are grouped by feature, Foam production controls are owned by Layers A–E, duplicate lifecycle/rendering controls are removed from diagnostics, and Foam test harnesses are under Actions. R3 was Unity-validated and accepted: it replaces all local debug selectors with one exclusive central controller, preserves existing serialized enum fields, reports legacy conflicts without changing them on Inspector open, supports multi-object mixed values, and removes the duplicate Foam selector from the lifecycle probe. R4 was Unity-validated and accepted: it replaces the preserved transition telemetry with stable read-only Domain, Disturbance, and Foam diagnostic panels; centralizes generated status and all remaining test/cache/reset actions; and removes stale heuristic navigation. R5 now limits constant repaint to visible live Disturbance or Foam diagnostic panels, removes the remaining Boolean foldout state, and splits the Editor into the approved partial files.

## Step R1 — Structural shell

1. Introduce the section-key/open-set foldout system.
2. Create the top-level collapsed hierarchy.
3. Preserve the current draw methods behind the new foldouts temporarily.
4. Verify `serializedObject.Update()` and `ApplyModifiedProperties()` still occur once per Inspector pass.
5. Verify no property values change on open.

## Step R2 — Authoring ownership

1. Move non-Foam controls into the target feature groups.
2. Move Foam production controls into Layers A–E.
3. Remove duplicate lifecycle/rendering controls from diagnostics.
4. Move manual/test controls out of authoring.
5. Replace patch-history HelpBoxes with concise current-contract tooltips or short text.

R2 implementation result:

- Water Body, Surface Motion, Refraction, and Runtime Disturbances now expose closed nested authoring groups.
- Foam authoring is split into Runtime & Quality and Layers A–E.
- Layer B velocity controls, Layer C lifecycle controls, and Layer D occupancy timings have one production-authoring home.
- Automatic birth sources remain Layer C authoring; their runtime counters were removed from the authoring surface.
- Manual Layer C sources and lifecycle probes moved under Actions.
- Existing Foam telemetry moved under the Runtime Diagnostics transition area.
- Local debug selectors remain temporary and are removed by R3.
- No runtime, shader, scene, prefab, material, or serialized field changed.

## Step R3 — Central debug hub

1. Remove local debug selectors from Body, Motion, Refraction, Disturbances, and Foam sections.
2. Implement one exclusive selector using existing serialized enum fields.
3. Add mixed-value handling.
4. Add legacy conflict reporting.
5. Add `Normalize to Rendered View`.
6. Add `Reset All Debug Views`.
7. Remove the duplicate Foam selector from Material Probe.

R3 implementation result:

- The Debug Views top-level section now owns Water Body, Surface Motion, Refraction, Disturbance, and Foam debug selection.
- Selecting one non-Final view resets the other four serialized debug fields to Final.
- Final Render and Reset All Debug Views clear every debug substitution.
- Foam views are grouped by Layers A–E, with Layer D split into Primary, Advanced Internals, and Comparisons.
- Disturbance views are grouped into Primary Field, Static Pressure & Wake, and Ripple Validation.
- Existing scenes with multiple active fields are reported through Active Rendered View, Conflict State, Hidden Active Views, and the exact shader priority.
- Normalize to Rendered View preserves the shader-winning view on each selected river and clears hidden lower-priority fields.
- Mixed multi-object selections remain visible and any explicit selection applies an exclusive configuration to all selected rivers.
- Local selectors and the duplicate Foam Material Probe selector were removed.
- No runtime, shader, scene, prefab, material, serialized field, or enum value changed.

## Step R4 — Diagnostics and actions

1. Add the standard read-only row helper.
2. Move telemetry into the Runtime Diagnostics hierarchy.
3. Move generated values into Generated Status.
4. Move buttons and test harnesses into Actions.
5. Remove `Next Debug Section` and stale heuristic routing.
6. Keep diagnostic row heights stable.

R4 implementation result:

- Runtime Diagnostics now owns closed read-only Domain & Geometry, Disturbance, and Foam panels.
- Disturbance telemetry is split into Summary, Dispatches, Sources & Rebuild State, and Memory.
- Foam telemetry is split into Summary, Layers A–D, Runtime Resources, and Advanced Internals; Layer C further owns Transport Accounting, Lifecycle Authority, Birth Activity, and Isolated Probe State.
- The shared `DrawReadOnlyRow` presentation uses labelled, selectable non-editable values with stable rows for pending, unavailable, edit-mode, and multi-object states.
- Generated Status now owns generation, mesh, corridor, compatibility, bend-safety, deferred-reflection, and layer results.
- Actions now owns Generation, Domain Validation, Disturbance Test Events, Foam Layer A Cache Tools, Foam Layer C Test Sources, Foam Lifecycle Probes, and Runtime Clear & Reset.
- Runtime counters, cache previews, test events, clear actions, and peak resets no longer appear inside authoring sections.
- `Next Debug Section` and its threshold-based routing helper were removed.
- Existing serialized field names, enum values, runtime behavior, shaders, scenes, prefabs, materials, Ground, and Generated Mass remain unchanged.

## Step R5 — Repaint and code split

1. Restrict constant repaint to visible live panels.
2. Split the Editor into the approved partial files.
3. Remove obsolete `show...` fields and dead state.
4. Run static searches for duplicate property drawing and duplicate debug labels.
5. Confirm only Editor files and documentation changed.

R5 implementation result:

- `RequiresConstantRepaint()` now returns true only in Play Mode, for one selected River, while Runtime Diagnostics and a real live Disturbance or Foam leaf panel are visible.
- Ordinary authoring, Debug Views, Generated Status, Actions, and closed diagnostic parents do not request constant repaint.
- Layer C only repaints when one of its actual live child panels is open; opening the empty Layer C parent alone is insufficient.
- The remaining Foam source and pattern foldouts use the shared `InspectorSection` set and therefore start closed without dedicated `show...` Boolean fields.
- `StylizedRiverEditor.cs` now owns only class identity, Inspector state, repaint routing, top-level ordering, cleanup, and the creation menu.
- Authoring, Disturbances, Foam, Debug Views, Diagnostics, Actions, and shared UI are split into the seven approved partial Editor files, each with a Unity `.meta` file.
- All 269 serialized property names and all existing debug enum controls remain present with unchanged runtime ownership.
- No runtime, shader, compute, scene, prefab, material, Ground, or Generated Mass file changed.

---

# 19. Initial implementation file scope

Expected redesign scope:

```text
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Authoring.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Disturbances.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Diagnostics.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.UI.cs
```

And their `.meta` files.

Canonical documentation will be updated after the implementation result is accepted.

The following should not require modification for the first redesign:

```text
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.*.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/**
```

If implementation discovers that runtime or shader changes are actually required, stop and present the exact evidence and revised scope before changing them.

---

# 20. Serialized compatibility and safety requirements

The redesign passes compatibility only if:

- existing serialized control values remain unchanged after merely opening and closing the Inspector;
- existing debug enum values remain valid;
- retired Foam serialized value `16` retains its current safe behavior;
- prefab overrides remain attached to the same serialized fields;
- Undo/Redo works for all authoring and debug changes;
- multi-object authoring continues to work;
- legacy multi-debug conflicts are reported but not silently modified;
- choosing a new debug view intentionally normalizes the selected rivers;
- no scene, prefab, material, or profile is raw-edited.

---

# 21. Performance expectations

The redesign adds no runtime rendering or simulation cost.

Expected effects:

```text
New runtime allocations       0
New simulation fields         0
New compute dispatches        0
New shader branches           0
New serialized runtime fields 0
```

Editor benefits should include:

- less layout work while most sections are closed;
- fewer continuous repaints;
- no live telemetry polling for hidden diagnostic panels where avoidable;
- less duplicated debug-selector code;
- a smaller maintenance surface for future diagnostics.

No claim of measurable Editor speedup should be made until observed in Unity, but the repaint and visibility rules are structurally cheaper.

---

# 22. Static verification checklist

Before Unity validation:

1. Confirm every `Find("...")` property still has one intended editable authoring location.
2. Confirm no production setting is drawn twice.
3. Confirm every debug enum is controlled by the central debug hub only.
4. Confirm all five debug fields are reset when selecting `Final Render`.
5. Confirm a non-Final selection resets the other four fields.
6. Confirm legacy conflict detection follows shader priority exactly.
7. Confirm all new partial declarations match the existing namespace and class modifiers.
8. Confirm all new Editor files have `.meta` files.
9. Confirm no runtime or shader file changed.
10. Confirm no serialized field was renamed.
11. Confirm all obsolete `show...` fields and stale heuristic methods are removed if no longer referenced.
12. Confirm `RequiresConstantRepaint()` contains no ordinary authoring-foldout conditions.
13. Confirm no conflict markers or whitespace errors.
14. Confirm Ground and Generated Mass files are untouched.

---

# 23. Unity validation checklist

## 23.1 Compilation and rendering invariance

1. Open the project and confirm clean C# compilation.
2. Confirm no HLSL files changed and no shader recompile error is introduced.
3. Open an existing tuned River and take a reference screenshot before interacting with controls.
4. Open and close all redesigned sections without changing values.
5. Confirm the rendered river, disturbances, and Foam remain identical.

## 23.2 Collapsed-default behavior

6. Select a River in a fresh Inspector instance.
7. Confirm every top-level section is collapsed.
8. Expand Foam and confirm every Layer A–E subsection is collapsed.
9. Expand Runtime Diagnostics and confirm every nested diagnostic group is collapsed.
10. Deselect and reselect the River; confirm the required collapsed default returns.

## 23.3 Authoring discoverability

11. Locate and edit one representative control from every top-level production feature.
12. Confirm every control appears in one location only.
13. Confirm Final Foam Visibility Mode appears under Layer E only.
14. Confirm lifecycle controls appear under Layer C only.
15. Confirm Foam velocity controls appear under Layer B rather than diagnostics.
16. Confirm manual birth and probe controls appear only under Actions.

## 23.4 Debug hub

17. Activate one Water Body debug view and confirm it renders.
18. Activate one Motion view and confirm Body resets to Final.
19. Activate one Refraction view and confirm Motion resets to Final.
20. Activate one Disturbance view and confirm Refraction resets to Final.
21. Activate Material Presence and confirm all non-Foam debug fields are Final.
22. Use `Reset All Debug Views` and confirm normal rendering returns.
23. Create or load a legacy configuration with two active debug fields.
24. Confirm the Inspector reports the rendered and hidden views without silently changing them.
25. Use `Normalize to Rendered View` and confirm only the shader-winning view remains active.

## 23.5 Read-only diagnostics and layout stability

26. Enter Play Mode and open Foam runtime diagnostics.
27. Confirm labels are visibly read-only and cannot be edited.
28. Confirm units and unavailable states are explicit.
29. Observe asynchronous telemetry updates and confirm the Inspector does not shift vertically.
30. Trigger and clear a known warning; confirm the fixed warning row changes value without expanding/collapsing the panel.
31. Close runtime diagnostics and confirm the Inspector no longer repaints continuously merely because Foam authoring is open.

## 23.6 Actions and multi-object editing

32. Confirm generation, validation, test, cache, source, probe, clear, and reset buttons are grouped under Actions.
33. Confirm disabled states remain correct outside Play Mode or without required runtime state.
34. Select multiple rivers and confirm production authoring still supports mixed values.
35. Confirm live diagnostics request a single selected river rather than presenting misleading merged telemetry.
36. Apply a debug view to multiple selected rivers and confirm all selected objects receive the exclusive configuration with Undo support.

---

# 24. Acceptance gates

The redesign is accepted only when all of the following are true:

1. Every top-level and nested section starts collapsed.
2. The Inspector opens to a compact header rather than a long form.
3. All normal River settings are grouped by feature.
4. Foam settings and diagnostics follow Layers A–E.
5. Authoring, debug selection, telemetry, generated status, and actions are visibly separate.
6. Every production control has one authoritative editable location.
7. Runtime and generated metrics use proper read-only rows.
8. Live rows update without layout churn.
9. One central debug controller prevents new conflicting selections.
10. Legacy conflicts are visible and explicitly normalizable.
11. All existing debug views remain available in the first redesign.
12. No river rendering, simulation, generation, tuning, or serialized value changes without user interaction.
13. Constant repaint is limited to visible live panels.
14. Multi-object editing and Undo remain functional.
15. The Editor source is split into maintainable partial files.
16. The upcoming Presence attribution has a clear home under Layer C Transport Accounting.

---

# 25. Follow-up after redesign acceptance

After the Inspector redesign is validated and accepted:

1. Record the accepted Inspector/debug contract in the canonical River documents.
2. Delete this temporary plan and its `.meta` file.
3. Resume the behavior-neutral Presence capacity-loss attribution audit.
4. Add the new metrics only under:

   ```text
   Runtime Diagnostics
     Foam
       Layer C — Material & Lifecycle
         Transport Accounting
   ```

5. Use the redesigned Debug Views hub for Material Presence, Motion Field, topology, and evaluated-shape comparisons.
6. Consider debug-view enum/shader retirement only in a later, separately approved cleanup.

---

# 26. Next work items

- Unity-validate R5 compilation, partial-file loading, collapsed Foam source foldouts, and visible-panel repaint behavior.
- Complete R6 by recording the accepted Inspector/debug contract in canonical River documentation.
- Delete this temporary plan and its `.meta` file after R6 is accepted.
- Resume Foam `5.16E.3` Presence capacity-loss attribution under Layer C Transport Accounting.
