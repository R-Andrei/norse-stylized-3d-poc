# Map Chunk Procedural Generation and River Network Architecture Plan

## Status

- **Document state:** Design synthesis complete; architecture approval and implementation approval pending.
- **Documentation gates:** Initial creation audit complete. Root-Docs relocation and superseded-plan retirement review, plan, implementation, and final audit gates complete. Seven-knot AutoSmooth correction is superseded by the approved nine-knot terminal-buffer replacement. Nine-knot review, planning, serialized implementation, and static audit are complete; Unity rebuild and visual acceptance remain pending.
- **Documentation scope:** This file is the decision record and proposed implementation plan for connected map chunks, Ground, Rivers, River networks, Foam continuity, and vegetation generation.
- **Current patch scope:** Replace the current seven-knot `River_Strip` spline in `VisualFrameworkDemo.unity` with the approved nine-knot conservative terminal-buffer layout. Preserve both exact boundary endpoints, use two collinear five-metre guide intervals at each boundary, place three gentle interior shaping knots, serialize every knot as AutoSmooth with tension `0.5` and world-up-derived rotation, and preserve every River setting and unrelated dirty-worktree change. Update this decision record first, then raw-edit only the approved existing scene. No C#, shader, compute, prefab, material, generated asset, cache, layer, tag, component, hierarchy, folder, dependency, or unrelated serialized default is changed.
- **Baseline commit:** `e5d9be79a1bfa8f5756713a407fa6c7227c7bc8b`.
- **Authority boundary:** Accepted requirements below record the user-directed product requirements from the current design session. Sections labelled **Recommendation** are proposed architecture, not accepted implementation authority. Sections labelled **Open decision** require a deliberate decision before the affected implementation phase.
- **Canonical integration:** If this proposal is accepted, its stable architecture decisions must be incorporated into the applicable canonical world, Ground/River, River Foam, and vegetation documents before implementation. This architecture plan does not silently supersede those documents.

## River nine-knot terminal-buffer replacement patch

### Objective

Replace the unsuccessful seven-knot terminal treatment with a nine-knot AutoSmooth spline whose outermost ten metres on each side are defined by three collinear knots. Begin meaningful lateral curvature only after the full generated River corridor footprint is inside the 40 m Ground patch.

### Acceptance criteria

1. `River_Strip` contains one open nine-knot spline in SplineContainer fileID `1626776152`.
2. Boundary knots remain exactly `(1.2796793, 0, 0.627174)` and `(3.099679, 0, 40.627174)`, which resolve through the unchanged River transform to world `Z = -20` and `Z = 20`.
3. Start knots `0`, `1`, and `2` share local X/Y `(1.2796793, 0)` at local Z `0.627174`, `5.627174`, and `10.627174`.
4. End knots `6`, `7`, and `8` share local X/Y `(3.099679, 0)` at local Z `30.627174`, `35.627174`, and `40.627174`.
5. Interior knots are `(1.3, -0.02, 15.627174)`, `(1.5, 0.02, 20.627174)`, and `(2.1, -0.03, 25.627174)`.
6. Every knot uses `TangentMode.AutoSmooth` (`Mode: 0`), tension `0.5`, and tangent lengths/rotation derived from the installed Unity Splines algorithm with world-up normal.
7. Knot Z spacing is exactly `5 m`; chord-length variation comes only from the gentle interior X/Y displacement.
8. The SplineContainer identity, River GameObject/Transform, `StylizedRiver` settings, hierarchy, layer, tag, generated outputs, and caches remain unchanged.
9. Unity validation remains pending until the user runs the complete Ground/River rebuild and verifies both terminal shores, the full centreline, and the absence or presence of the tight-bend warning.

### Approved files

| File | Operation | Status |
| --- | --- | --- |
| `Assets/Docs/Map_Chunk_Procedural_Generation_and_River_Network_Architecture_Plan.md` | Record the approved nine-knot replacement, supersede the unsuccessful seven-knot terminal treatment, and record audit evidence | Static audit passed; Unity validation pending |
| `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity` | Replace only the knot and metadata arrays of SplineContainer fileID `1626776152` | Implemented and statically verified; Unity validation pending |

### Reviewed evidence and constraints

- The live scene block at `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity:131733-131877` still contains the seven-knot AutoSmooth state validated after the previous patch.
- Current chord distances are approximately `4.90`, `6.26`, `8.70`, `11.32`, `5.04`, and `4.90 m`, a maximum/minimum ratio of approximately `2.31`.
- `River_Strip` serializes width `4.9`, bank blend `1.24`, additional shoreline overlap `0.5`, shoreline irregularity `2.86`, and quality `Medium` at `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity:131990-132015`.
- From `StylizedRiverNaturalVariationSettings.ResolveSafeShorelineAmplitude` and `ResolveShoreWidths`, the conservative theoretical maximum surface half-width is `2.45 + min(2.86, 2.45 * 0.45) + (4.9 * 0.08 + 0.5) = 4.4445 m`.
- Adding bank blend gives a maximum handoff half-width of `5.6845 m`. The selected `Standard40` / `Medium33` Ground has `1.25 m` grid spacing, so `ResolveIntegrationApronWidth` adds approximately `1.9445 m`, producing an approximate maximum outer render half-footprint of `7.629 m`.
- `StylizedRiverCorridorGeometry.BuildRefinedRings` warns when maximum surface half-width exceeds `0.80 * estimatedRadius`; the conservative theoretical warning-clear radius is therefore `4.4445 / 0.80 = 5.555625 m`.
- A two-dimensional AutoSmooth curvature probe of the approved X/Z layout estimated a minimum centreline radius of approximately `10.6 m`. This is an **Unverified inference (High confidence)** because the production result also includes Y offsets, sampled Domain spacing, natural width variation, and Ground response. The complete Unity rebuild will verify or falsify it.
- `Library/PackageCache/com.unity.splines@9b00833aca09/Runtime/Spline.cs:387-495` and `SplineUtility.cs:1004-1090` require tangent lengths and rotations to be serialized consistently with AutoSmooth mode; replacing positions or metadata alone is incomplete.
- The user supplied visual evidence that the seven-knot AutoSmooth result still clips/blends poorly at the edge and approved the nine-knot conservative replacement on 2026-07-28.
- The scene/worktree contains extensive unrelated user-owned changes. Only the exact SplineContainer knot and metadata arrays are approved for modification.

### Invariants and non-goals

- Preserve the exact boundary endpoints and unchanged River transform, so the centreline still begins and ends at the Ground bounds.
- Preserve one open spline, SplineContainer fileID `1626776152`, GameObject `River_Strip`, and all non-spline serialized data.
- Preserve every River/Ground/Foam/Painted Accent source, setting, generated output, and cache.
- Do not preserve the seven-knot interior positions or tangents; that layout is the rejected behavior being replaced.
- Do not restore Continuous, Mirrored, Broken, or Linear tangent modes.
- Do not add a runtime system, editor tool, component, dependency, folder, layer, or tag.
- Do not rebuild or save generated outputs in this patch; the user owns the explicit rebuild.
- Do not clean, reserialize, or otherwise alter unrelated scene/worktree content.

### File-by-file sequence

1. Update this plan before the serialized scene edit.
2. Replace the complete `m_Knots` list for the active spline with the nine approved knots and algorithm-derived AutoSmooth tangent/rotation values.
3. Replace the seven metadata entries with nine `Mode: 0`, `Tension: 0.5` entries.
4. Verify exact knot count, positions, spacing, modes, tensions, endpoint world positions, component identity, and unchanged non-spline River settings.
5. Compare the final scoped component with the captured seven-knot state, `HEAD`, and the accepted nine-knot plan.
6. Record static audit results here; leave Unity import/rebuild/warning/visual validation pending for the user.

### Risks and validation

| Risk | Mitigation / required result | Status |
| --- | --- | --- |
| Raw positions are updated without consistent AutoSmooth tangent/rotation data. | Serialize all nine complete `BezierKnot` values calculated from neighbor positions at tension `0.5` and world-up normal. | Passed statically: every knot contains the calculated tangent lengths/rotation and matching `Mode: 0`, `Tension: 0.5` metadata |
| Terminal segment still bows or rotates before the edge. | Require three exactly collinear X/Y knots over the outer ten metres at each end; outermost segments must have axis-aligned controls. | Static collinearity/control audit passed; Unity visual validation pending |
| The centreline loses too much authored variation. | Accept a deliberately conservative first result; shoreline irregularity remains unchanged and supplies organic bank variation. Add centreline meander only after this baseline passes. | Accepted by user for trial |
| The centreline collides visually with existing rocks/bridge composition. | Inspect the rebuilt full river and adjust only interior knots in a separately approved follow-up if necessary. | Pending Unity visual validation |
| Tight-bend warning or bank pinch remains. | Capture the complete warning and both terminal screenshots. Do not change warning thresholds or River width to conceal a geometry failure. | Pending Unity validation |
| Generated outputs become stale. | Expected after a spline-domain edit; the user will run the existing complete Ground/River rebuild. | Pending user rebuild |
| Unrelated scene/worktree content is overwritten. | Use one contextual replacement inside SplineContainer fileID `1626776152` and audit exact component fields. | Passed for this operation: only this plan and the declared SplineContainer knot/metadata arrays were edited |
| Runtime performance changes. | No runtime code changes. A longer/smoother centreline may slightly change dirty-time Domain/ring counts; measure the complete rebuild report if it exposes changed counts or warnings. | Static source audit passed; rebuild evidence pending |

### Implementation and audit record

- **Plan update:** Passed. This section is the first repository modification after approval of the nine-knot replacement.
- **Scene implementation:** Passed statically. SplineContainer fileID `1626776152` now contains the nine approved positions and complete algorithm-derived AutoSmooth tangent/rotation values.
- **Knot/metadata audit:** Passed. The active spline contains nine knots, nine `Mode: 0` entries, nine tension values of `0.5`, and remains open.
- **Spacing audit:** Passed. Chord lengths are `5.000000`, `5.000000`, `5.000081`, `5.004158`, `5.036120`, `5.099045`, `5.000000`, and `5.000000 m`; maximum/minimum ratio is approximately `1.019809`.
- **Terminal alignment audit:** Passed statically. Knots `0-2` share X/Y `(1.2796793, 0)` and knots `6-8` share X/Y `(3.099679, 0)`. The outermost endpoint/guide controls are axis-aligned.
- **Endpoint audit:** Passed. With unchanged River transform position `(9.83, 0, -20.627174)`, the boundary knots resolve to world `(11.1096793, 0, -20)` and `(12.929679, 0, 20)`.
- **Scope audit:** Passed for the performed operations. The only files edited by this replacement are this plan and the existing scene SplineContainer block. Pre-existing River source, Foam cache, Painted Accent, and other dirty-worktree changes remain user-owned and outside this patch.
- **Whitespace audit:** The changed plan and spline lines contain no trailing whitespace. Whole-scene `git diff --check` remains unsuitable as a pass gate because the pre-existing large Unity scene diff contains unrelated blank scalar lines with trailing spaces; those lines were preserved.
- **Source/architecture audit:** Passed statically. No C#, shader, compute, package, dependency, component, hierarchy, layer, tag, generated output, or cache changed. Active architecture now requires two aligned terminal guides per side.
- **Performance audit:** No active-play runtime code or dirty-time algorithm changed. Rebuilt Domain/ring counts and rebuild duration remain unmeasured until the explicit Unity rebuild.
- **Unity import/rebuild/warning/visual validation:** Pending user action. Required evidence is the complete rebuild report, the exact presence or absence of the tight-bend warning, and Scene-view screenshots of both terminal shores plus the full river.

## River terminal-guide AutoSmooth correction patch (superseded)

The seven-knot metadata/tangent correction below remains historical evidence. User visual validation showed that its bend still began too close to the edge for the complete corridor footprint. The nine-knot terminal-buffer replacement above is the active authority.

### Objective

Remove the manually constrained Continuous tangent behavior from the two added `River_Strip` terminal-guide knots while preserving their positions and the exact boundary endpoints. Recalculate each changed knot's serialized tangent lengths and rotation with Unity Splines' AutoSmooth algorithm so the spline enters and exits the guide through the ordinary neighboring-knot curve rather than a forced collinear handle pair.

### Acceptance criteria

1. `River_Strip` retains one open seven-knot spline in SplineContainer fileID `1626776152`.
2. All seven knot positions remain byte-for-byte unchanged.
3. Transition knots at indices `1` and `5` change from `TangentMode.Continuous` (`Mode: 3`) to `TangentMode.AutoSmooth` (`Mode: 0`) with unchanged tension `0.5`.
4. The two changed knots' serialized tangents and rotations equal the AutoSmooth result derived from their immediate neighbor positions and their existing up direction.
5. Both boundary endpoints and their adjacent guide knots continue to share the same local X/Y coordinates.
6. No River source, River setting, generated output, Foam cache, Painted Accent asset, layer, tag, component, hierarchy, or unrelated scene content changes.
7. Unity validation remains pending until the user runs the existing complete Ground/River rebuild and confirms the tight-bend warning and visible inside-bank pinch are absent.

### Approved files

| File | Operation | Status |
| --- | --- | --- |
| `Assets/Docs/Map_Chunk_Procedural_Generation_and_River_Network_Architecture_Plan.md` | Record the approved correction, update the terminal-guide architecture decision, and record final audit evidence | Static audit passed; Unity validation pending |
| `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity` | Change only transition knots `1` and `5` from Continuous to AutoSmooth, including their algorithm-derived tangent/rotation serialization | Implemented and statically verified; Unity validation pending |

### Reviewed evidence and constraints

- The current scene block at `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity:131733-131877` contains seven knots and mode sequence `0, 3, 0, 0, 0, 3, 0`.
- Transition knot `1` is at local position `(1.2796793, 0, 5.527174)` between boundary knot `0` at `(1.2796793, 0, 0.627174)` and interior knot `2` at `(-1.130321, -0.02, 11.307175)`.
- Transition knot `5` is at local position `(3.099679, 0, 35.727173)` between interior knot `4` at `(1.0296793, -0.06, 31.137175)` and boundary knot `6` at `(3.099679, 0, 40.627174)`.
- `Library/PackageCache/com.unity.splines@9b00833aca09/Runtime/BezierMode.cs:8-33` defines `AutoSmooth = 0` and `Continuous = 3`; Continuous keeps tangent directions parallel, while AutoSmooth derives them from neighboring knot positions.
- `Library/PackageCache/com.unity.splines@9b00833aca09/Runtime/Spline.cs:387-495` proves that `SetTangentMode` changes both metadata and knot tangent/rotation content. A metadata-only raw edit would therefore be incomplete.
- `Library/PackageCache/com.unity.splines@9b00833aca09/Runtime/SplineUtility.cs:1004-1025` and `1062-1090` define the AutoSmooth tangent and rotation calculation used for the serialized replacement values.
- `Assets/Game/Procedural/Rivers/StylizedRiverCorridorGeometry.cs:644-673` sets the tight-bend warning when the maximum generated half-width exceeds `0.80 * estimatedRadius`, where `estimatedRadius = segmentLength / turnRadians`.
- The user supplied a screenshot showing an inside-bank pinch at the constrained terminal transition and explicitly approved the AutoSmooth correction on 2026-07-28.
- The scene and worktree contain extensive unrelated user-owned changes. Only the exact SplineContainer block fields declared above are approved for modification.

### Invariants and non-goals

- Preserve all knot positions, knot count, open/closed state, SplineContainer identity, GameObject identity, and River component settings.
- Preserve exact boundary endpoint placement and axis alignment between each endpoint and its guide.
- Preserve all five pre-existing AutoSmooth knots.
- Do not change `StylizedRiver`, corridor generation, bend-warning thresholds, Ground generation, rebuild orchestration, or any generated/cached output.
- Do not attempt to guarantee a mathematically straight endpoint-to-guide Bezier segment; this correction deliberately prioritizes an unconstrained smooth transition. The post-rebuild visual boundary cut remains a required validation result.
- Do not clean, reserialize, regenerate, or otherwise alter unrelated scene/worktree content.

### File-by-file sequence

1. Update this plan before any serialized scene edit.
2. Replace only the two transition knots' tangent lengths, rotations, and metadata modes with the algorithm-derived AutoSmooth values.
3. Verify the complete seven-knot block, exact positions, mode sequence, changed-field count, and absence of edits to River source/generated outputs.
4. Re-read the reviewed source/package contracts and compare the final scoped diff with the approved plan.
5. Record static audit results here; leave Unity import, rebuild, warning, and visual shoreline validation pending for the user.

### Risks and validation

| Risk | Mitigation / required result | Status |
| --- | --- | --- |
| Metadata changes but forced tangent data remains serialized. | Recalculate tangent lengths and rotation using the installed Unity Splines AutoSmooth implementation, matching what `Spline.SetTangentMode` changes. | Passed: both guide knots contain recalculated asymmetric tangent lengths, recalculated rotations, and `Mode: 0` |
| Endpoint-to-guide segment bows laterally after smoothing. | Preserve aligned endpoint/guide positions and validate the rebuilt boundary cut visually. If unacceptable, stop and redesign guide spacing or adjacent interior position rather than restoring forced Continuous handles. | Pending Unity validation |
| Tight-bend warning persists elsewhere. | Run the complete rebuild and inspect the complete warning plus shoreline. A persistent warning requires locating its sampled segment before further geometry edits. | Pending Unity validation |
| Unrelated dirty scene content is overwritten. | Use one contextual patch within SplineContainer fileID `1626776152`; audit exact changed fields against the pre-edit capture. | Passed for this patch: actual scene changes are limited to 12 scalar tangent/rotation values and two mode scalars in the captured block |
| Generated outputs become stale. | Expected after a spline-domain edit; the user will run the existing complete Ground/River rebuild. Do not edit generated outputs in this patch. | Pending user rebuild |
| Runtime performance changes. | No per-frame algorithm or source changes. Only the rebuilt static spline shape changes; any geometry-count difference is unmeasured until rebuild. | Static source audit passed; rebuilt geometry-count effect pending user rebuild |

### Implementation and audit record

- **Plan update:** Passed. This section was the first repository modification after the correction review.
- **Scene implementation:** Passed statically. Knot `1` changed to tangent lengths `1.6996748` / `1.9214814`, rotation `(0.000810289, -0.09859845, 0.00008028447, 0.99512696)`, and `Mode: 0`. Knot `5` changed to tangent lengths `1.6297702` / `1.607688`, rotation `(-0.0030299316, 0.10571505, 0.00032211586, 0.9943918)`, and `Mode: 0`.
- **Position/mode audit:** Passed. The final component contains the original seven positions in the original order, mode sequence `0, 0, 0, 0, 0, 0, 0`, seven tension values of `0.5`, and an open spline.
- **Scope audit:** Passed for the performed operations. The only files modified by this correction are this plan and the existing scene. Pre-existing River source, Foam cache, Painted Accent, and other dirty-worktree changes remain outside this patch and were not modified by its two contextual edits.
- **Whitespace audit:** The changed plan lines and changed spline fields contain no trailing whitespace. Repository `git diff --check` remains nonzero because the pre-existing large scene diff contains unrelated Unity-authored blank scalar lines; those lines are outside the approved block and were preserved.
- **Source/architecture consistency audit:** Passed statically. No C#, shader, compute, package, dependency, hierarchy, component, layer, tag, generated output, or cache was changed. The active terminal-guide contract now consistently requires aligned guide positions with AutoSmooth tangents.
- **Performance audit:** No active-play runtime code or dirty-time algorithm changed. Static spline shape alone may change rebuilt sample/ring counts; that effect is unmeasured until the user rebuilds.
- **Unity import/rebuild/warning/visual validation:** Pending user action. Required evidence is the complete rebuild result, absence or presence of the exact tight-bend warning, and one Scene-view screenshot of both terminal shores.

## Root-Docs relocation and superseded-plan retirement patch

### Objective

Move the surviving Map Chunk architecture plan from `Assets/Docs/OrchestrationPlans/` to the `Assets/Docs/` root, retire the other two plan documents, and leave the orchestration-plan directory itself present and empty.

### Acceptance criteria

1. `Assets/Docs/Map_Chunk_Procedural_Generation_and_River_Network_Architecture_Plan.md` contains the surviving architecture plan.
2. Its existing `.meta` moves with it and retains GUID `82ec1142fc634fc6b3c634b554979552`.
3. `Ground_River_Complete_Rebuild_Action_Plan.md` and `River_Strip_Endpoint_Trim_Plan.md`, plus their `.meta` files, are deleted.
4. `Assets/Docs/OrchestrationPlans/` remains present and contains no files.
5. `Assets/Docs/OrchestrationPlans.meta` remains unchanged.
6. The surviving architecture content contains no deleted plan as an active evidence source; references inside this bounded retirement record remain historical operation evidence only.
7. Existing Ground/River rebuild behavior and current spline evidence are cited from surviving source/scene files.
8. No file outside the six plan-document paths is modified or deleted.

### Approved files

| File | Operation | Status |
| --- | --- | --- |
| `Assets/Docs/OrchestrationPlans/Map_Chunk_Procedural_Generation_and_River_Network_Architecture_Plan.md` | Update references/audit, then move to `Assets/Docs/` | Passed |
| `Assets/Docs/OrchestrationPlans/Map_Chunk_Procedural_Generation_and_River_Network_Architecture_Plan.md.meta` | Move with unchanged GUID | Passed |
| `Assets/Docs/OrchestrationPlans/Ground_River_Complete_Rebuild_Action_Plan.md` | Delete as superseded | Passed |
| `Assets/Docs/OrchestrationPlans/Ground_River_Complete_Rebuild_Action_Plan.md.meta` | Delete with its document | Passed |
| `Assets/Docs/OrchestrationPlans/River_Strip_Endpoint_Trim_Plan.md` | Delete as superseded | Passed |
| `Assets/Docs/OrchestrationPlans/River_Strip_Endpoint_Trim_Plan.md.meta` | Delete with its document | Passed |

### Reviewed evidence and constraints

- The directory contains exactly the six approved files listed above.
- Repository-wide reference search found references to the retiring plan paths only inside the three plan documents.
- `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs:3208-3223` and `Assets/Game/Procedural/Ground/Editor/GroundRiverProductionRebuildCoordinator.cs:17-242` provide surviving implementation evidence for the complete rebuild action.
- Before the correction above, `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity:131733-131877`, SplineContainer fileID `1626776152`, provided serialized evidence for the seven-knot River spline and its two forced-Continuous terminal guides. The final active state is recorded in the correction audit above.
- The orchestration-plan directory and its `.meta` are user-required preservation targets and are outside deletion scope.
- The worktree contains extensive unrelated changes; they remain user-owned and excluded.

### Invariants and non-goals

- Preserve the Map Chunk plan's content, filename, and Unity GUID except for path/reference/audit reconciliation.
- Preserve `Assets/Docs/OrchestrationPlans/` and `Assets/Docs/OrchestrationPlans.meta`.
- Preserve all source, shaders, scenes, generated outputs, caches, layers, tags, components, hierarchy, and serialized defaults.
- Do not create a replacement plan for either retired document.
- Do not clean up any unrelated worktree change.

### File-by-file sequence

1. Update the surviving document so deleted plan paths are replaced by surviving implementation/scene evidence.
2. Move the surviving Markdown file and `.meta` to `Assets/Docs/`.
3. Delete only the two superseded Markdown files and their `.meta` files.
4. Verify the retained orchestration-plan directory is empty and its `.meta` is unchanged.
5. Audit path references, GUID uniqueness, whitespace, document content, and exact approved scope.

### Risks and validation

| Risk | Mitigation / required result | Status |
| --- | --- | --- |
| The directory is deleted despite the requested preservation. | Resolve and inspect the exact directory after deletion; it must exist and be empty. | Passed: resolved directory exists with zero children |
| Unity loses the surviving document identity. | Move the existing `.meta`; GUID must remain unique and unchanged. | Passed: GUID `82ec1142fc634fc6b3c634b554979552` has one repository occurrence |
| The surviving architecture cites deleted plans. | Deleted plan names may occur only inside this retirement record, never in active architecture evidence. | Passed |
| Historical evidence is lost with the plans. | Cite the surviving coordinator/editor source and serialized spline component instead. | Passed |
| Unrelated files are altered. | Scoped status and content checks must match the approved six-file operation only. | Passed for this operation; unrelated pre-existing worktree changes remain preserved |
| Unity behavior changes. | Not applicable: documentation relocation/deletion only; verify no source or serialized scene changed. | Passed as not applicable |

### Implementation and audit record

- The surviving document and its Unity identity now exist at `Assets/Docs/Map_Chunk_Procedural_Generation_and_River_Network_Architecture_Plan.md` and `.md.meta`.
- The surviving `.meta` retains GUID `82ec1142fc634fc6b3c634b554979552`; repository search reports exactly one occurrence.
- The two superseded plan documents and their `.meta` files no longer exist.
- `Assets/Docs/OrchestrationPlans` resolves to the intended workspace path and contains zero files.
- `Assets/Docs/OrchestrationPlans.meta` remains present with folder GUID `bdeb2d6e6b25d30489f3a98c4f11a4cd`.
- Active architecture evidence now cites `GeneratedGroundEditor`, `GroundRiverProductionRebuildCoordinator`, and the current serialized `River_Strip` spline instead of either deleted plan.
- No source, shader, scene, prefab, material, generated output, cache, layer, tag, component, hierarchy, or serialized default was changed by this operation.

## Primary answer

**Recommendation (High confidence):** Build the world as a graph of independently owned map chunks connected by explicit edge contracts. Keep one `GeneratedGround` and separate child `StylizedRiver` segment objects per chunk, but compile connected River segments into logical `RiverNetwork` components. A network owns continuity-sensitive River state; chunks own generation and streaming state. Do not merge an entire multi-chunk River into one scene-wide spline by default.

The system should have four layers:

```text
World graph
  -> chooses chunk coordinates, adjacency, route roles, and River-network topology

Chunk authoring
  -> owns Ground recipe, reciprocal edge contracts, River segment layout, vegetation profile,
     seeds, manual overrides, generation fingerprints, and output state

Network compilation
  -> groups connected River segments, resolves flow, cumulative distance, shared style,
     shared time, seam properties, Foam grid alignment, and network validation

Deterministic build transaction
  -> River Domain intent
  -> base Ground
  -> River Ground influence
  -> final Ground
  -> visible River/corridor
  -> vegetation coverage and instances
  -> persistent production caches
```

This extends, rather than replaces, the repository's existing doctrine:

- chunks already conceptually contain terrain recipes, sockets, splines, zones, and placement data (`Assets/Docs/Proof of Concept/04_World_Construction_and_Generative_Assembly.md:268-303`);
- world generation is already specified as graph first and geometry second (`Assets/Docs/Proof of Concept/04_World_Construction_and_Generative_Assembly.md:345-369`);
- Ground is already the scheduler and commit owner for one patch, while River retains River Domain, geometry, collision, material, disturbances, and Foam ownership (`Assets/Docs/Ground_River_Regeneration_Orchestration_Manual.md:1232-1237`);
- the existing complete rebuild action already provides the selected-Ground production transaction without changing River source (`Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs:3208-3223` and `Assets/Game/Procedural/Ground/Editor/GroundRiverProductionRebuildCoordinator.cs:17-242`);
- current vegetation production ownership is already `GeneratedGround > GroundVegetation > VegetationLayer`, with one coverage field per layer (`Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md:5773-5780`, `Assets/Game/Procedural/Vegetation/GroundVegetation.cs:150`, and `Assets/Game/Procedural/Vegetation/VegetationLayer.cs:11-59`).

## Accepted requirements from the design session

The following requirements are treated as decided. They describe required outcomes, not yet-approved class names or files.

1. New map chunks must be quick to create and connect to existing chunks.
2. Chunk generation must create at least initial Ground, River, and vegetation output.
3. River configuration must be editable both before and after generation.
4. River layout configuration and River visual/physical characteristics are separate concerns.
5. The current River characteristics are the initial default style.
6. Initial River layouts must support:
   - opposite-edge crossings;
   - adjacent-edge turns;
   - same-edge hairpins;
   - diagonals;
   - straight passages;
   - gentle and stronger meanders;
   - manual layouts;
   - every map side through orientation/parameterization rather than duplicate assets.
7. A River endpoint must lie exactly on its chunk boundary.
8. A generated River endpoint must have two inward terminal guides collinear with the edge normal, so the outermost segment is straight and meaningful lateral curvature begins only after the complete corridor footprint is inside the chunk. The current `River_Strip` proof uses three collinear AutoSmooth knots over the outer ten metres at each boundary (`Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity`, SplineContainer fileID `1626776152`; River nine-knot terminal-buffer replacement patch above).
9. Adjacent chunks must share the exact same River connection point. Approximate visual alignment is not sufficient.
10. Standardized port locations are required, but the exact percentages or count have not been selected.
11. Port suitability must account for River/corridor width and corner clearance. A wide River must not be forced through a near-corner port that cannot contain its complete footprint.
12. A large map may contain any number of disconnected River networks. All River segments are not implicitly part of one global River.
13. A connected River must preserve visual flow, width behavior, waves, and eventually Foam across chunk boundaries.
14. The existing explicit complete Ground/River/cache rebuild remains the production rebuild transaction. Generation tooling must call or expose that transaction rather than create a competing rebuild path.
15. Expensive rebuilding must remain explicit or dirty-time work. No per-frame full-field regeneration is allowed; this also matches the current Ground/River orchestration invariant (`Assets/Docs/Ground_River_Regeneration_Orchestration_Manual.md:703`).

## What has not been decided

### Decisions required before the first implementation slice

| Open decision | Final recommendation | Why it remains open |
| --- | --- | --- |
| Initial chunk size policy | Support the existing `40 m x 40 m` square Ground first; make the data model size-aware without implementing mixed sizes in version 1. | The current proof and endpoint math use `GroundPatchSize.Standard40`; mixed sizes add port, edge-sample, vegetation-field, and streaming complexity. |
| Initial port catalogue | Measure supported corridor footprints, then define named `SlotId` values symmetrically around the midpoint. Do not commit percentages before that measurement. | The user's example percentages were illustrative. Port legality depends on complete corridor footprint plus safety margin. |
| Number of River profiles in version 1 | Start with three compatible width classes: narrow, current/default, and wide. | Exact widths and corridor reach must be measured from accepted River profiles before serialization defaults are approved. |
| Ground seam version 1 | Implement an explicit deterministic edge-height contract, not only coincident transforms. | World-space noise can align the base field, but River carve, modifiers, normals, and later biome transitions can still diverge. |
| Initial River topology | Support unbranched directed paths only: segment degree at most two. Treat a same-side hairpin as one path, not a graph cycle. | Confluences require discharge/width blending, three-way corridor geometry, Foam redistribution, and flow validation. |
| Runtime or editor generation first | Editor-time authoring and baking first. Preserve runtime-compatible data, but do not make runtime procedural generation the first milestone. | Current Ground production assets and Foam topology follow explicit Edit Mode ownership. |
| Initial Foam seam milestone | First ship exact visual/network continuity and seam-safe source behavior; add mass-conserving cross-segment transport as a separate measured patch. | Current Foam explicitly has endpoint outflow and no external inflow (`Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Simulation.hlsl:482-532`). |
| Vegetation placement continuity | Replace chunk-local RNG placement with a world-space hashed candidate lattice for generated coverage. | Current placement uses a local `System.Random(seed)` sequence (`Assets/Game/Procedural/Vegetation/VegetationRendererBase.cs:1145`), which is deterministic per layer but is not an explicit cross-chunk candidate identity contract. |
| Profile edits after generation | Use profile reference plus a stored resolved snapshot, override flags, fingerprints, and an explicit preview/reapply action. | Automatically propagating every profile edit can destroy manual spline or coverage work. |

### Decisions that can wait until later phases

- Whether runtime streaming loads scenes, prefabs, addressable content, or another chunk container.
- Whether a generated world is persisted as one world asset, per-chunk assets, or both.
- Whether River confluences, distributaries, closed loops, lakes, and waterfalls are supported.
- Whether different Ground biomes can meet directly or must use transition chunks.
- The active/near/dormant River and vegetation streaming radii.
- The maximum simultaneous active Foam-cell and disturbance-cell budgets.
- Whether connected Foam tiles are dispatched individually, in texture arrays, or through a larger atlas after profiling.
- Whether manual nonstandard ports are permitted in production builds or remain an authoring-only escape hatch.
- Whether a connected River can transition between style profiles automatically, and which properties may transition.
- Whether vegetation density, biome, and exclusion fields are stored as authored textures, generated recipes, or a hybrid after initial generation.

## Recommended data and ownership model

### 1. `MapChunkAuthoring`

**Recommendation (High confidence):** One component or serialized definition should represent each generated chunk. It should own authoring state, not every subsystem algorithm.

Conceptual data:

```text
ChunkId
GridCoordinate
WorldSeed
ChunkSize
ChunkProfile
GroundProfile
VegetationProfile
Ports[]
RiverSegments[]
NeighbourLinks[]
ManualOverrideFlags
GenerationRevision
InputFingerprints
OutputValidationState
```

Responsibilities:

- derive exact world transform from grid coordinate and chunk size;
- maintain reciprocal neighbour links;
- resolve profile references and derived seeds;
- expose create-adjacent, preview, generate, reapply, validate, and rebuild actions;
- identify dirty outputs;
- preserve manual overrides;
- request subsystem-owned work in the required order.

It must not:

- own River mesh algorithms;
- own Ground mesh algorithms;
- run one giant per-frame world rebuild;
- write Foam state directly;
- render vegetation;
- silently save scenes or overwrite manual work.

### 2. Profiles

Use separate ScriptableObject-style profiles because layout, style, and placement change independently:

| Profile | Owns |
| --- | --- |
| `MapChunkProfile` | Chunk size class, allowed sides/roles, default Ground/vegetation profiles, landmark and route policy |
| `GroundProfile` | Existing Ground recipe-compatible shape and surface settings, seam compatibility class |
| `RiverLayoutProfile` | Port topology, path character, knot count/ranges, terminal-guide policy, bend constraints |
| `RiverStyleProfile` | Width class, corridor/bank/depth settings, natural variation, water/motion settings, Foam/disturbance policy |
| `VegetationProfile` | Layer recipes, density rules, coverage generation rules, exclusions, seed namespaces |
| `RiverPortSlotSet` | Named normalized edge slots and compatibility metadata |

Layout and style must not be one asset. Regenerating a style should preserve the spline. Regenerating a layout should replace only generated interior knots after preview/Undo and must preserve reciprocal ports.

### 3. Resolved snapshots and overrides

Each generated chunk should store:

- the source profile reference;
- the source profile revision/fingerprint;
- the resolved values used for the last build;
- explicit per-field or grouped overrides;
- whether River interior knots are generated, manually edited, or locked;
- whether vegetation coverage is generated, painted, or mixed.

Post-generation reapply should show a preview of affected outputs. It must not treat a profile as a live prefab whose edits silently rewrite every authored chunk.

## Port and seam contract

### Canonical edge coordinates

Define edges as `North`, `East`, `South`, and `West`. Store a port using:

```text
Edge
SlotId
NormalizedCoordinate u in [0,1]
WorldPosition
InwardNormal
WaterSurfaceHeight
FlowDirection
WidthClass
RiverNetworkId
ConnectionId
```

The canonical coordinate direction must be stable:

- North and South: `u` increases with world `+X`;
- East and West: `u` increases with world `+Z`.

This prevents one side's `20%` from accidentally matching the opposite side's `80%`.

When a new chunk connects to an existing chunk, the existing port is authoritative. The new side copies its reciprocal `SlotId`, exact world position, surface height, network identity, and compatibility requirements.

### Slot derivation

Do not choose slot percentages from aesthetics alone. For River profile `p`:

```text
cornerDistance(u) = min(u, 1 - u) * edgeLength

requiredClearance(p) =
    maximumResolvedCorridorHalfFootprint(p)
  + cornerSafetyMargin

portIsLegal(u, p) =
    cornerDistance(u) >= requiredClearance(p)
```

`maximumResolvedCorridorHalfFootprint` must include the widest generated River/corridor state that can reach the edge, not only nominal water half-width. It must account for bank blend, hidden overlap/integration apron, natural variation, and any accepted safety reach.

**Recommendation (High confidence):** Define a symmetric master slot catalogue from the measured profile set, then attach a compatibility mask to each slot/profile pair. Narrow Rivers can use more slots. Wide Rivers can use only the central slots that satisfy the clearance equation.

The actual slot positions remain an open decision until:

1. narrow/current/wide profile footprints are measured;
2. the corner safety margin is selected;
3. the desired minimum separation between simultaneous ports is selected;
4. the 40 m chunk is tested with adjacent-side and same-side layouts.

### River terminal geometry

Every generated connection uses:

```text
boundary endpoint
  -> inward terminal guide
  -> second inward terminal guide
  -> generated interior knots
  -> second inward terminal guide
  -> inward terminal guide
  -> boundary endpoint
```

Rules:

- the endpoint lies exactly on the Ground/chunk boundary;
- the endpoint and guide share the same edge-parallel coordinate and elevation policy;
- the endpoint-to-guide segment is normal to the edge;
- terminal-buffer depth and guide spacing are derived from the complete River/corridor footprint with a configured minimum;
- the endpoint and both terminal guide positions are constrained to the edge normal, while tangent direction is AutoSmooth from neighboring knot positions;
- the interior generator cannot move endpoints or guides;
- manual editing may move interior knots but preserves locked ports unless the connection is explicitly detached.

The current proof's active serialized state contains two aligned guides per end, giving three collinear AutoSmooth knots across each ten-metre terminal buffer (`Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity`, SplineContainer fileID `1626776152`). The earlier five-knot attempt did not preserve a square corridor cut. The forced-Continuous seven-knot attempt pinched, and the seven-knot AutoSmooth correction still began curvature inside the complete corridor footprint; both are superseded by the nine-knot terminal-buffer replacement.

### Ground edge contract

Coincident chunk transforms are necessary but not sufficient.

Use one shared edge record keyed by the two reciprocal chunk sides:

```text
ChunkEdgeContract
  EdgeId
  Endpoint transforms
  Height sample spacing
  Shared height samples
  Optional shared normal/slope data
  Material/biome transition class
  River ports on this edge
  Revision/fingerprint
```

Both Grounds consume the same edge samples when generating their boundary row. One owner writes the contract per authoring transaction; both chunks consume it. A deterministic function can derive it when possible, but it must still be validated as one shared result.

The current Ground already evaluates base noise in a coordinate translated by `patchCoordinate * patchSize` (`Assets/Game/Procedural/Ground/GroundGenerator.cs:91-93` and `832-846`). That is a useful foundation for continuous base noise. **Inference (High confidence):** it is not a complete seam guarantee once chunk-local River influence, modifiers, normals, or profile transitions differ. The edge-contract test would falsify this inference if every supported modifier/River/profile combination independently produced bit-compatible boundary positions and accepted normals.

## River layout generation

### Compose topology and path character

Do not author one asset for every side combination and rotation.

Generate a River layout from:

```text
entry port
exit port
topology class
path character
style footprint
seed
chunk bounds
exclusion/reservation zones
```

Initial topology classes:

- `OppositeEdges`
- `AdjacentEdges`
- `SameEdgeHairpin`
- `Manual`

Initial path characters:

- `Straight`
- `GentleMeander`
- `StrongMeander`
- `Snake`
- `Hairpin`

All map sides are parameters. A north-to-south straight River and an east-to-west straight River are the same layout algorithm in different edge frames.

### Knot policy

Authored knot count is a shape-control budget, not a River mesh-density budget. The current Domain builder resamples by evaluated River length and requested sample spacing (`Assets/Game/Procedural/Rivers/StylizedRiverGeometry.cs:339-355`), so more authored knots are required only where the intended curvature needs them.

Generation rules:

- start with the minimum knots required by topology/path character;
- add a knot only to express an intentional bend or satisfy curvature/clearance constraints;
- enforce bend-radius and corridor-containment validation;
- reject layouts that leave the chunk footprint;
- keep all interior generated knots inside the legal River-centre region;
- preview several deterministic candidates and let the author accept or reseed;
- preserve endpoints and terminal guides during reseeding.

## Emergent River-network model

### Network graph

Represent River connectivity as a graph:

- graph vertices are reciprocal River ports;
- an internal edge is one chunk's River segment between two ports;
- an external edge connects reciprocal ports across adjacent chunks;
- a connected component is one `RiverNetwork`.

When chunks are connected or disconnected, recompute only affected connected components with a dirty-time graph traversal. Do not scan and rebuild every network per frame.

For version 1, each River-network vertex should have directed degree compatible with one unbranched path. This supports many independent networks across 100 chunks without assuming a fixed count:

```text
100 chunks
  -> connected-component compilation
  -> 2, 5, 13, 17, or any other topology-produced network count
```

The network count itself is not the main runtime cost. Active simulated cells, instance counts, draw/dispatch count, and update cadence are the main cost inputs. This is a recommendation based on the current per-River runtime ownership, not a measured 100-chunk benchmark.

### Network-owned continuity state

One `RiverNetwork` should own or resolve:

- stable `RiverNetworkId`;
- ordered segment list for unbranched networks;
- flow direction;
- network seed namespace;
- River style compatibility class;
- base width/corridor policy;
- natural-variation settings and seed;
- visual/motion settings and seed;
- shared network clock;
- cumulative global-distance origin per segment;
- Foam coefficients, quality, cell spacing, lateral lattice phase, and topology revision;
- simulation activation state;
- diagnostic and validation state.

One chunk River segment retains:

- its `SplineContainer`;
- its `StylizedRiver`;
- local generated Domain/surface/corridor/collider objects;
- Ground parent and Ground-influence participation;
- per-segment caches/resources as required by streaming;
- local authored sources and obstacles.

### Why separate segment objects should remain

**Recommendation (High confidence):** Do not merge a whole network into one giant spline/component by default.

Reasons supported by current ownership:

- Ground discovers child Rivers per patch (`Assets/Game/Procedural/Ground/GeneratedGround.cs:1714-1723`);
- Ground owns each patch transaction, while River owns its local Domain and visible/collision/runtime output (`Assets/Docs/Ground_River_Regeneration_Orchestration_Manual.md:1232-1237`);
- current Foam and disturbance resources are component-local RenderTextures (`Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs:1023-1220` and `Assets/Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Resources.cs:286-311`);
- one scene-wide River object would couple otherwise independent chunk rebuild, cache, culling, loading, and failure boundaries.

A merged spline can remain an export/debug view or an optimization for a small permanently loaded authored level. It should not be the core world representation.

### Global River distance

For ordered segments:

```text
segmentOffset[0] = networkDistanceOrigin

segmentOffset[i] =
    networkDistanceOrigin
  + sum(length of every upstream segment)
```

The River Domain already supports this concept:

- `connectedRiverDistanceOffset` exists on `StylizedRiver` (`Assets/Game/Procedural/Rivers/StylizedRiver.cs:494`);
- `ConfigureConnectedDomain(distanceOffset, isReverseFlow)` exists (`Assets/Game/Procedural/Rivers/StylizedRiver.cs:5047-5062`);
- `StylizedRiverGeometry.BuildDomain` computes `globalDistance = connectedDistanceOffset + orientedDistance` (`Assets/Game/Procedural/Rivers/StylizedRiverGeometry.cs:474-480`);
- `RiverDomainSnapshot` exposes the global-distance range (`Assets/Game/Procedural/Rivers/RiverDomainSnapshot.cs:69-70`);
- the water shader consumes global distance for motion (`Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader:482-509`, `628-662`).

This means the current River already contains much of the required spatial continuity foundation.

### Width, shape, and randomization across seams

Seam-sensitive River properties must be evaluated from network state and global distance, not independently randomized per segment.

For example:

```text
resolvedWidth(s) =
    networkBaseWidth
  + networkVariation(seed, s)
  + authoredTransition(s)
```

At a seam with global distance `sSeam`, both neighboring segments sample the same function and therefore resolve the same width and variation. The current natural variation path already receives connected/global distance and a seed during Domain construction (`Assets/Game/Procedural/Rivers/StylizedRiverGeometry.cs:474-482` and `Assets/Game/Procedural/Rivers/StylizedRiver.cs:6259`).

Rules:

- a new segment extending an existing network inherits its continuity-sensitive profile;
- a standalone segment may start a new network/profile;
- two established incompatible networks do not connect silently;
- an incompatible connection is rejected or resolved through an authored transition segment;
- a wide network does not shrink abruptly merely because the next chunk offers only a narrow corner port.

### Water elevation and flow

Add a shared port elevation contract. Reciprocal ports must agree on water surface height within an explicit tolerance.

For version 1:

- the world/hydrology graph chooses the directed source-to-outlet order;
- each port has one authoritative surface height;
- each segment interpolates a non-increasing downstream water profile;
- Ground carving follows the River profile, not the reverse at the seam;
- impossible uphill or insufficient-clearance segments fail validation instead of silently reversing flow.

Confluences and waterfalls require separate rules and are deferred.

### Shared motion time

Global distance and matching seeds are insufficient if each component advances an independent clock. Current `StylizedRiver` stores and advances `riverTime` locally (`Assets/Game/Procedural/Rivers/StylizedRiver.cs:2366`, `4206`) and publishes it as `MotionTime` (`Assets/Game/Procedural/Rivers/StylizedRiver.cs:2419`, `6604-6605`).

**Recommendation (High confidence):** Connected segments must sample a shared network clock, with a local fallback for standalone Rivers. This is a small arithmetic/binding cost; it is not a new field simulation. Exact implementation and compatibility behavior require a separate River patch review.

## Foam and disturbance continuity

### Current proven limitation

Current Foam transport treats only a physical longitudinal endpoint as open and explicitly permits no external inflow (`Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Simulation.hlsl:482-532`). Current Foam also updates from each component's `LateUpdate` (`Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs:108-507`) and owns per-component textures (`Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs:1023-1220`).

Therefore:

- exact port positions do not make persistent Foam cross the seam;
- equal seeds do not transfer upstream Foam state;
- a cumulative distance offset makes sampling coordinates continuous but does not make two simulations conservative;
- independent component update timing can create state differences even when coefficients match.

### Final Foam recommendation

Keep per-segment simulation tiles, but coordinate them through one `RiverNetworkRuntime`.

All tiles in one network must share:

- fixed metric longitudinal/lateral cell sizes;
- lateral lattice phase and row identity;
- coefficients and substep policy;
- network simulation time;
- update/commit phase;
- compatible topology/boundary representation.

The existing descriptor already exposes fixed metric spacing, lateral phase, and global lateral-row identity (`Assets/Game/Procedural/Rivers/StylizedRiverFoamGridDescriptor.cs:61-65`, `98-101`, `143-146`). Reuse that foundation.

For every connected seam and lateral row:

1. Read the previous-state upstream and downstream boundary/ghost cells.
2. Resolve one shared face velocity and one shared donor/reconstruction state.
3. Compute one seam flux.
4. Subtract that mass from the donor tile.
5. Add the identical mass to the receiver tile.
6. Commit/swap all participating tiles after all seam and interior fluxes are resolved.

The conservation condition is:

```text
upstreamMassLoss(seam, row, step)
    =
downstreamMassGain(seam, row, step)
```

This must include every transported persistent channel owned by the accepted Foam transport representation.

For higher-order/TVD transport, exchange the complete stencil width, not only one boundary texel. Current transport reconstructs donors from neighboring states, so the exact ghost-band width must be derived from the accepted kernel before implementation.

### Foam topology and source generation

**Recommendation (Medium confidence):** Compile network Foam topology and deterministic automatic sources in global River coordinates, then partition the result into per-segment cache tiles with required overlap/ghost metadata.

This avoids independently randomized seam sources and keeps topology identity stable as a segment crosses a chunk boundary. It preserves per-segment cache/streaming ownership while using one network coordinate system.

This recommendation is not yet proven against current topology-cache serialization. Verification requires a dedicated cache-format and runtime-consumer audit.

### Transitional milestone before coupled Foam

If connected chunk generation is delivered before cross-tile Foam transport:

- synchronize style, global distance, seeds, clock, quality, and grid phase;
- suppress automatic Foam births inside a measured terminal seam band;
- mark the network as `VisualContinuityOnly`;
- validate that the seam does not introduce a visible source discontinuity;
- do not claim persistent Foam transport continuity.

This is an explicit intermediate limitation, not the final solution.

### Disturbances

Disturbances and ripples need two-way boundary state exchange because waves can propagate both upstream and downstream even when material transport is directed. Use synchronized substeps and ghost-state exchange per connected seam. Do not reuse the directed Foam-inflow contract blindly.

### Performance model

Let:

- `Wi * Hi` be the active cell count of tile `i`;
- `S` be the number of active seams;
- `Hseam` be rows exchanged per seam;
- `K` be simulation substeps;
- `G` be the ghost-band width.

Base cell work is approximately:

```text
O(K * sum(Wi * Hi))
```

Seam exchange is approximately:

```text
O(K * S * Hseam * G)
```

The seam arithmetic is normally much smaller than the interior cell work, but GPU dispatch/binding/synchronization overhead may dominate for many tiny tiles. This is an **Unverified inference (Medium confidence)**. A representative multi-network GPU profile must verify it before selecting individual dispatches, texture arrays, or atlases.

Use simulation tiers:

- `Active`: full accepted update rate near gameplay;
- `Visual`: reduced or presentation-only state where accepted;
- `Dormant`: retained/frozen or safely unloaded state;
- `Unloaded`: serialized/cache state only.

Activation changes must be event-driven or streaming-driven, not a full-world per-frame scan.

## Ground generation recommendation

### Ground profile and coordinates

Generate the first version on the existing square patch contract:

- derive `patchCoordinate` from chunk grid coordinate;
- derive exact world position from coordinate and patch size;
- use a compatible Ground profile across directly connected ordinary chunks;
- derive independent Ground seeds from the world seed and chunk coordinate;
- consume shared edge samples on every connected edge.

The current Ground stores `patchCoordinate` (`Assets/Game/Procedural/Ground/GeneratedGround.cs:395`, public getter at `439`) and uses it to translate base noise (`Assets/Game/Procedural/Ground/GroundGenerator.cs:91-93`). The new tool will need a reviewed authoring API rather than raw serialized writes because the public configuration surface currently exposes `SetShapeSeed` but not a general chunk-recipe setter (`Assets/Game/Procedural/Ground/GeneratedGround.cs:450`).

### Ground seam ownership

Generation order for adjacent chunks:

1. Resolve or create the reciprocal `ChunkEdgeContract`.
2. Generate base Ground fields for dirty chunks.
3. Apply shared boundary samples.
4. Prepare River Domains and Ground influence.
5. Finalize each Ground exactly once.
6. Validate boundary position and normal tolerances.
7. Build visible River/corridor outputs.
8. Build vegetation.
9. Bake/validate persistent outputs.

When a shared edge or port changes, mark both sides dirty. Rebuild only the changed chunks and directly affected network/edge consumers, not every world chunk.

### Biome/profile transitions

Version 1 should require the same Ground seam-compatibility class across an ordinary connection. Different biomes should initially use an authored transition chunk or transition edge profile. Direct arbitrary-profile blending can follow after the shared edge contract is validated.

## Vegetation generation recommendation

### Preserve current production ownership

Keep:

```text
GeneratedGround
  -> Vegetation [GroundVegetation]
       -> one VegetationLayer per independently configured recipe/coverage field
```

This matches current production ownership (`Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md:5773-5800`; `Assets/Game/Procedural/Vegetation/GroundVegetation.cs:150`; `Assets/Game/Procedural/Vegetation/VegetationLayer.cs:11-59`).

The chunk builder should:

- create/reuse the Ground-owned vegetation root through an approved authoring API;
- create layers from `VegetationProfile`;
- generate initial coverage into each layer;
- preserve later painted coverage unless the user previews and approves regeneration;
- rebuild vegetation after committed Ground and River corridor semantics exist.

### Semantic coverage generation

Do not copy the current demo's painted coverage into new chunks.

Generate coverage from rules:

```text
base biome density
* world-space density noise
* slope suitability
* altitude/moisture suitability
* River-bank response
* path/landmark visibility exclusions
* authored zone modifiers
```

River-aware rules should query semantic River/Ground influence, not inspect rendered triangles. This follows the existing Ground/River contributor/consumer direction (`Assets/Docs/Ground_River_Regeneration_Orchestration_Manual.md`, Candidate 5 Bank semantic contract).

Potential coverage classes:

- dense forest;
- sparse forest;
- grass;
- reeds/wet-bank vegetation;
- path exclusion;
- River water/hidden corridor exclusion;
- bridge/landmark clearance;
- encounter/sightline clearance;
- authored paint override.

### Deterministic world-space placement

Use independent derived seeds:

```text
groundSeed     = Hash(worldSeed, chunkCoordinate, "ground")
riverSeed      = Hash(worldSeed, riverNetworkId, "river")
vegetationSeed = Hash(worldSeed, layerStableId, "vegetation")
propSeed       = Hash(worldSeed, chunkCoordinate, "props")
```

This matches the canonical recommendation for independently derived generation seeds (`Assets/Docs/Proof of Concept/04_World_Construction_and_Generative_Assembly.md:539-560`).

For generated vegetation placement, define a world-space candidate lattice:

```text
candidateId = (worldLatticeX, worldLatticeZ, layerStableId)
candidateRandom = Hash(worldSeed, candidateId)
```

Each chunk accepts candidates in half-open bounds:

```text
[minimumX, maximumX) x [minimumZ, maximumZ)
```

This gives adjacent chunks the same candidate identity system while ensuring exactly one chunk owns an edge candidate. It avoids duplicate or discontinuously reseeded boundary placement.

Current macro-patch variation is already evaluated from world position and documented as continuous when controls match (`Assets/Game/Procedural/Vegetation/VegetationRendererBase.cs:154-162`, `1259-1262`). Preserve that behavior.

### Vegetation rebuild policy

Vegetation rebuilds only when:

- committed Ground surface/revision changes;
- its own layer recipe changes;
- generated or painted coverage changes;
- relevant semantic exclusion inputs change;
- explicit regeneration is requested.

Material-only vegetation changes must not regenerate Ground or River. Runtime wind and interaction fields remain separate systems and must not become chunk-generation inputs.

## Editor workflow

### Recommended location

Add an editor window at:

```text
Tools > Norse > Map Chunk Builder
```

Also expose context actions on a selected `MapChunkAuthoring`:

- `Create Adjacent Chunk`
- `Adopt Existing Chunk`
- `Preview Generation`
- `Generate/Reapply`
- `Validate Connections`
- `Complete Rebuild and Cache`

The existing complete action remains available in `GeneratedGround > Regeneration and Caching`.

### Create-adjacent workflow

1. Select an existing chunk and one edge.
2. Choose `Create Adjacent Chunk`.
3. The tool derives the reciprocal grid coordinate and exact transform.
4. If connecting an existing River, the new incoming port is copied exactly.
5. Choose or generate the outgoing topology, slot, and path character.
6. Validate profile/port/corner compatibility.
7. Preview Ground, River centreline/corridor footprint, and semantic zones.
8. Accept generation with Undo support.
9. Generate Ground authoring state and River spline.
10. Generate vegetation coverage/layers.
11. Compile affected River networks.
12. Run the existing complete rebuild/cache transaction for the new chunk and any dirty neighbor.
13. Present one validation report.

### Adopt-existing workflow

The current demo chunk should be importable without rewriting its accepted spline:

- identify current Ground bounds and patch coordinate;
- create `MapChunkAuthoring` metadata;
- infer edge ports from exact boundary endpoints;
- classify the River spline as `ManualLockedInterior`;
- preserve River style and existing vegetation layers/coverage;
- assign or create a logical River network record;
- validate, but do not rebuild until explicitly requested.

## Validation requirements

### Chunk validation

- unique `ChunkId` and grid coordinate;
- exact transform derived from coordinate/size;
- no unsupported overlap;
- reciprocal neighbor links;
- compatible chunk sizes and edge lengths;
- shared edge sample count and fingerprint;
- boundary position and normal tolerance;
- valid generation/output revision state.

### River-port validation

- exact reciprocal world position;
- opposite inward normals;
- matching `SlotId`;
- compatible width/corridor footprint;
- sufficient corner clearance;
- exact or accepted-tolerance surface elevation;
- one flow-in/flow-out relationship;
- locked terminal guide normal to the edge;
- no corridor footprint outside legal chunk bounds except the exact seam handoff;
- accepted bend-radius/tight-bend result.

### River-network validation

- every segment belongs to at most one network;
- every connected component has one stable network identity;
- version 1 degree is at most two;
- no directed cycle in version 1;
- cumulative global distance is monotonic and seam-continuous;
- continuity-sensitive style and seeds match;
- connected segments use one network clock;
- Foam grid spacing/lateral phase/quality are compatible;
- cache revisions match the compiled network topology;
- unsupported confluence/profile transition fails explicitly.

### Vegetation validation

- each layer resolves its ancestor Ground;
- coverage storage is valid;
- generated coverage fingerprint matches semantic inputs;
- River/path/landmark exclusion checks pass;
- candidate IDs are unique across adjacent half-open chunk domains;
- macro-patch controls are seam-compatible where continuity is required;
- instance and bounds budgets remain accepted.

### Production validation

- the complete Ground/River rebuild succeeds for each dirty chunk;
- Painted Accent production output is current where required;
- assigned Foam topology caches are stored/validated;
- missing caches are reported according to the existing rebuild contract;
- no topology cache is built or saved automatically during Play;
- generated scenes/assets are not silently saved by the authoring action.

## Recommended implementation sequence

No phase below is approved for source implementation by this document.

### Phase 0 — Decision and measurement freeze

Status: **Pending**

1. Approve or amend this architecture.
2. Measure the maximum complete corridor half-footprint for narrow/current/wide River profiles.
3. Select the 40 m version-1 slot catalogue and corner safety margin.
4. Confirm the 40 m square-only version-1 scope.
5. Select the Ground edge height/normal tolerance.
6. Select the first vegetation profiles and density/exclusion rules.
7. Define representative performance scenes and active-network budgets.

### Phase 1 — Data model and adoption, no generation behavior change

Status: **Pending approval**

- add chunk/profile/port/network authoring data;
- add `Adopt Existing Chunk`;
- import the current demo as manual/locked;
- validate graph, reciprocal ports, and network distance;
- do not change River Foam, Ground generation, or vegetation placement.

### Phase 2 — Connected editor generation

Status: **Pending approval**

- create adjacent 40 m chunks;
- generate standard River layouts with locked terminal guides;
- create explicit Ground edge contracts;
- generate initial vegetation coverage/layers;
- invoke the existing complete rebuild transaction;
- add preview, Undo, dirty-state, and validation reports.

### Phase 3 — River visual continuity

Status: **Pending approval**

- network-owned continuity profile and seeds;
- cumulative global distance;
- shared motion clock;
- exact width/natural-variation continuity;
- water-elevation validation;
- explicit visual-only Foam seam limitation.

### Phase 4 — Foam and disturbance seam continuity

Status: **Pending approval and profiling**

- network-aligned metric grids;
- global/topology-partition cache contract;
- synchronized tile stepping;
- mass-conserving Foam seam flux and required ghost stencil;
- two-way disturbance ghost exchange;
- active/visual/dormant tiers;
- multi-network profiling and acceptance budgets.

### Phase 5 — Broader world generation and streaming

Status: **Deferred**

- route/world graph generation;
- chunk family selection;
- biome transition chunks;
- runtime loading/persistence;
- confluences and transition Rivers only after separate architecture approval.

## Proposed future source surface

This is a planning inventory, not approved file scope. Exact filenames and architecture require Gate 1 review before each phase.

Likely new areas:

```text
Assets/Game/Procedural/World/
Assets/Game/Procedural/World/Editor/
```

Likely new responsibilities:

- chunk authoring/identity;
- chunk and subsystem profiles;
- edge and River-port contracts;
- River-network definition/compiler/runtime;
- editor builder, preview, adoption, and validators;
- deterministic seed and world-lattice utilities.

Likely existing consumers requiring review or bounded changes:

- `Assets/Game/Procedural/Ground/GeneratedGround.cs`
- `Assets/Game/Procedural/Ground/GroundGenerator.cs`
- `Assets/Game/Procedural/Ground/Editor/GroundRiverProductionRebuildCoordinator.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiver.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverGeometry.cs`
- `Assets/Game/Procedural/Rivers/RiverDomainSnapshot.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamGridDescriptor.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.*.cs`
- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam*.hlsl`
- `Assets/Game/Procedural/Vegetation/GroundVegetation.cs`
- `Assets/Game/Procedural/Vegetation/VegetationLayer.cs`
- `Assets/Game/Procedural/Vegetation/VegetationRendererBase.cs`

No shared shader/include change is recommended before Phase 3 or 4. Every such change requires an explicit cross-subsystem impact audit.

## Acceptance criteria for the complete procedural system

1. A designer can create a connected 40 m adjacent chunk from a selected edge without manually calculating transforms or seam coordinates.
2. Reciprocal Ground edges and River ports match exactly and remain stable after regeneration.
3. Standard layouts cover opposite, adjacent, same-side hairpin, straight, diagonal, meander, snake, and manual cases across every side.
4. Narrow/current/wide River profiles are accepted only at slots that contain their full corridor footprint.
5. Connected segments compile into the correct number of emergent River-network connected components.
6. A River network preserves global distance, width/natural variation, water elevation, seeds, and motion time across every seam.
7. The final Foam milestone conserves transported persistent material across active connected seams within an approved numerical tolerance.
8. Separate River segments remain independently rebuildable, cacheable, and streamable by chunk.
9. Ground boundary positions/normals satisfy the approved seam tolerance after River influence and modifiers.
10. Initial vegetation is generated deterministically from Ground/River/semantic rules and may be edited afterward without involuntary loss.
11. Adjacent generated vegetation uses stable world-space candidate identity and contains no duplicate edge candidates.
12. The existing complete Ground/River/cache transaction remains authoritative and reports all updated/skipped/failed outputs.
13. Ordinary Play startup remains cache-only for production topology.
14. No full-world or full-field per-frame rebuild/scan is introduced.
15. Representative multi-network performance stays within separately approved cell, dispatch, instance, and frame-time budgets.

## Risks and mitigations

| Risk | Mitigation |
| --- | --- |
| Fixed ports make chunks repetitive. | Separate topology, path character, seeds, interior layout, terrain, vegetation, landmarks, and profile variation from the small standardized seam vocabulary. |
| Slots chosen too close to corners clip wide corridors. | Derive legality from measured full footprint plus margin; use profile compatibility masks. |
| Independent River segments visibly change width at seams. | Evaluate seam-sensitive properties from one network profile, seed, and global distance. |
| Waves drift across seams. | Use one network clock and motion seed with current global-distance shader input. |
| Foam disappears or appears at seams. | Do not claim continuity until synchronized boundary flux exists; use an explicit visual-only transitional limitation. |
| One giant merged River makes rebuilding/streaming expensive. | Keep chunk-local segment ownership and compile logical networks. |
| Ground base noise aligns but modified edges crack. | Use one explicit shared edge sample contract and validate final positions/normals. |
| Profile reapply destroys manual work. | Store generated/manual state, overrides, fingerprints, preview, and Undo; ports remain locked until explicit detachment. |
| Vegetation repeats or duplicates on edges. | Use world-space hashed candidate IDs and half-open chunk ownership. |
| Every loaded River simulates at full quality. | Use network-aware active/visual/dormant tiers and profile representative maps. |
| Confluence scope destabilizes the first system. | Restrict version 1 networks to degree-two directed paths. |
| New coordinator duplicates existing rebuild logic. | Call the existing Ground-owned production rebuild path; do not reproduce its cache/bake operations. |

## Reviewed evidence

The following evidence was read before this plan was written:

| Evidence | Relevant finding |
| --- | --- |
| `Assets/Docs/Proof of Concept/04_World_Construction_and_Generative_Assembly.md` | Canonical draft already defines semantic chunks, sockets, graph-first generation, seam hiding, derived seeds, and gameplay-before-decoration order. |
| `Assets/Docs/Ground_River_Regeneration_Orchestration_Manual.md` | Ground owns one patch transaction; River owns Domain/output; transaction order and no-perpetual-polling constraints are established. |
| `Assets/Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs` and `GroundRiverProductionRebuildCoordinator.cs` | Existing explicit Ground-owned rebuild transaction rebuilds active child Rivers, Ground, Painted Accent production, and assigned Foam caches. |
| `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity`, SplineContainer fileID `1626776152` | Current River correction preserves exact boundary endpoints and uses two aligned AutoSmooth guides per side so lateral curvature begins after the complete corridor footprint is inside the chunk. |
| `Assets/Game/Procedural/Ground/GeneratedGround.cs` | Ground stores patch coordinate, discovers descendant Rivers, builds River snapshots, and republishes corridors. |
| `Assets/Game/Procedural/Ground/GroundGenerator.cs` | Base Ground noise uses patch-coordinate-translated sampling. |
| `Assets/Game/Procedural/Rivers/StylizedRiver.cs` | River already has connected-distance offset, reverse-flow configuration, seeds, local motion time, and child/runtime ownership. |
| `Assets/Game/Procedural/Rivers/StylizedRiverGeometry.cs` | Domain is resampled by length/spacing and resolves global distance/natural widths from connected offset. |
| `Assets/Game/Procedural/Rivers/RiverDomainSnapshot.cs` | Domain is the authoritative River coordinate source and exposes a global-distance interval. |
| `Assets/Game/Procedural/Rivers/StylizedRiverFoamGridDescriptor.cs` | Fixed metric spacing, lateral lattice phase, and global row identity already exist. |
| `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Simulation.hlsl` | Current endpoint transport permits outflow and explicitly provides no external inflow. |
| `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md` and current vegetation sources | Ground-owned vegetation root/layers and independent layer coverage already exist; generated cross-chunk placement identity is not yet a contract. |

The review used the live dirty-worktree versions at the recorded baseline commit. Unrelated worktree changes were treated as user-owned and excluded from this documentation patch.

## Invariants

- Unity remains `6000.5.0f1` with URP.
- `GeneratedGround` remains Ground scheduler/commit owner per patch.
- `StylizedRiver` remains River Domain and local River-output owner.
- River network coordination does not transfer River mesh/Foam algorithms into a world/chunk component.
- Ground and River rebuild work remains explicit, staged, fingerprinted, and bounded.
- Production Foam topology remains explicit Edit Mode/cache-owned work.
- Vegetation remains Ground-owned by hierarchy and layer-owned for recipe/coverage.
- Runtime wind and interaction systems do not become procedural chunk-build inputs.
- No per-frame full-world graph scan or full-field regeneration is introduced.
- No layers, tags, required components, folders, assets, profiles, or serialized defaults are added without phase-specific approval.
- Existing unrelated dirty worktree changes remain preserved.

## Non-goals

- Implementing any source or scene change in this documentation patch.
- Selecting final slot percentages without corridor-footprint evidence.
- Supporting River confluences, distributaries, lakes, waterfalls, or directed cycles in version 1.
- Replacing current Ground, River, Foam, disturbance, vegetation, Weather, or interaction algorithms.
- Making runtime procedural world generation the first delivery.
- Automatically saving scenes or generated assets.
- Converting every existing manually authored chunk immediately.

## Initial creation file sequence (historical)

1. Create this plan as the first repository modification after review.
2. Add its required Visible Meta Files companion.
3. Audit that only these two new documentation paths were added by this patch.
4. Run Markdown/whitespace and meta-GUID checks.
5. Re-read the final document and record the documentation audit below.

## Initial creation validation and audit (historical)

| Check | Required result | Status |
| --- | --- | --- |
| Approved scope | Only this Markdown file and its `.meta` are added | Passed: scoped `git status --short` reports exactly the two new paths |
| Markdown whitespace | No whitespace defect in either new file | Passed: zero trailing-whitespace matches; both paths are untracked, so ordinary `git diff --check` has no patch input before staging |
| Markdown structure | Code fences are balanced | Passed: 40 fence delimiters |
| Meta GUID | Exactly one repository occurrence | Passed: one occurrence of `82ec1142fc634fc6b3c634b554979552` |
| Review-surface preservation | No reviewed source changes were introduced by this patch | Passed: all ten captured source SHA-256 values remained identical before and after documentation edits |
| Canonical consistency | No accepted subsystem ownership is contradicted | Passed: Ground, River, Foam/cache, and vegetation ownership remain unchanged; all new architecture is explicitly proposed |
| Source/Unity validation | Not applicable to documentation-only change | Passed as not applicable: no compilation, import-dependent source behavior, scene output, or runtime behavior changed |
| Final reread | Complete final document reread | Passed |

## Decision record

- **Accepted requirement:** Chunk seams and River ports must be exact, reusable connection contracts.
- **Accepted requirement:** The exact slot catalogue remains measurement-driven and undecided.
- **Accepted requirement:** Multiple independent River networks must emerge from chunk connectivity.
- **Recommendation:** Keep separate per-chunk River components and compile them into logical networks.
- **Recommendation:** Put every seam-sensitive River property in network/global-distance space.
- **Recommendation:** Use one shared Ground edge contract and world-space deterministic vegetation candidate lattice.
- **Recommendation:** Deliver editor-time connected generation before runtime streaming.
- **Recommendation:** Treat exact cross-segment Foam transport as its own synchronized simulation patch, not as a seed/offset tweak.
