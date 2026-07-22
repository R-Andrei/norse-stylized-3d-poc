## 2026-07-22 — GSU-M2.7C.5E.2 Inspector and refresh contract

The authoritative Ground editor baseline for this patch is the user-provided `GeneratedGroundEditor(6).cs`. Under the existing path:

```text
Hierarchy > Ground > Inspector > Material > River-Coupled Ground Response — Riverbed > Material Coverage
```

Riverbed authoring now exposes:

- `Material Strength` — existing total Riverbed material amount;
- `Material Blend Distance` — inward dry-material transition width in metres, `0–2`, default `0.35`; zero restores the historical hard boundary;
- `Material Blend Softness` — linear-to-cubic transition shape, `0–1`, default `0.75`.

The Inspector states explicitly that this is a **dry material transition**. It is independent from the existing `Wetness Transition` controls farther down the same Riverbed section. The new controls affect the complete resolved substrate response and do not change Riverbed Support, cover exclusion, River geometry, or wetness.

Candidate proof and refresh remain separate project actions:

```text
Tools > PS3D > Run Generated Mass Sparse Riverbed Assembly Proof
Tools > PS3D > Install All Sparse Riverbed Surface Candidates
```

The proof must be a passing `GSU-M2.7C.5E.2`, algorithm-version-6 run before installation. The installer owns exact canonical paths, updates existing assets rather than creating numbered copies, preserves editable `SSMP_Riverbed*` palette/material tuning and `GSLP_Riverbed*` layer tuning, verifies existing GUIDs remain stable, and copies one complete report to the clipboard. It does not assign a candidate to a scene automatically.

No new debug view or Scene overlay is added.

---

## 2026-07-21 — GSU-M2.7C.5E.1.1 Inspector correction note

The corrected M2.7C.5E.1 package does **not** replace `GeneratedGroundEditor.cs`. The previous package copied an archive-baseline editor containing vegetation-coverage APIs that were absent from the user's live branch, causing compilation failure.

Candidate assignment still uses the user's existing live Inspector path:

```text
Hierarchy > Ground > Inspector > Material > River-Coupled Ground Response — Riverbed > Riverbed Surface Source: Custom Riverbed Surface Layer > Custom Riverbed Surface Layer
```

Guaranteed palette and material-response editing is performed by selecting the corresponding generated material profile:

```text
Project > Assets/Game/Demo/Profiles/SurfaceMaterials/SparseRiverbedCandidates > SSMP_RiverbedUltraSparse / SSMP_RiverbedVerySparse / SSMP_RiverbedSparse
```

The `StylizedSurfaceMaterialProfileEditor` recognizes paired Palette Form payloads and exposes Base, Dark, Light, Cavity, Texture Form Strength, lighting, normal, cavity, roughness, and world-scale controls. Inline duplication of those controls inside the large shared Ground editor is deferred rather than risking another branch-baseline replacement.

---

## 2026-07-21 — GSU-M2.7C.5E.1 Inspector ownership note

> **Superseded delivery detail:** M2.7C.5E.1.1 removes the full `GeneratedGroundEditor.cs` replacement. Palette editing is guaranteed through the generated `SSMP_Riverbed*` assets; inline Ground-editor exposure is deferred.

M2.7C.5E.1 adds no new `GeneratedGround` field, foldout, debug view, Scene overlay, component, or prefab workflow. It creates three ordinary `GroundSurfaceLayerProfile` assets through one explicit project-level installer:

```text
Tools > PS3D > Install All Sparse Riverbed Surface Candidates
```

After installation, compare candidates through the existing path:

```text
Hierarchy > Ground > Inspector > Material > River-Coupled Ground Response — Riverbed > Riverbed Surface Source: Custom Riverbed Surface Layer > Custom Riverbed Surface Layer
```

Select `Riverbed — Ultra Sparse`, `Riverbed — Very Sparse`, or `Riverbed — Sparse`. Their shared material controls remain editable through the existing embedded material settings or by selecting the corresponding `SSMP_Riverbed*` asset. Installer reruns preserve those palette and response values.

The detail-library and material-profile editors now recognize paired prepacked Palette Form payloads as ordinary texture-form entries. No shader control, runtime override component, material-name branch, or density-specific Inspector code is introduced.

---

## 2026-07-21 — GSU-M2.7C.5D.5 Inspector ownership note

M2.7C.5D.5 adds no GeneratedGround or runtime Inspector control. The existing project-level proof action remains:

```text
Tools > PS3D > Run Generated Mass Sparse Riverbed Assembly Proof
```

The action now writes palette-neutral form, runtime-packed structural data, and three recolour previews in addition to the accepted assembly evidence. These outputs prove that Base/Dark/Light/Cavity colour authority can be separated from generated form, but they remain local under `Library/SurfaceMaterialDiagnostics/GeneratedMassSparseRiverbedAssembly`.

The existing `StylizedSurfaceMaterialProfile` palette controls and Ground material-property refresh path remain the intended runtime control surface. M2.7C.5D.5 does not create a profile, detail-library entry, Ground layer, selector item, scene object, debug view, or gameplay override. Those changes remain blocked until M2.7C.5E selects and promotes one candidate.

---

## 2026-07-21 — GSU-M2.7C.5D.4 Inspector ownership note

M2.7C.5D.4 changes only the Editor-only shared substrate generation and its validation metrics. No `GeneratedGround` foldout, runtime field, asset selector, debug view, Scene overlay, or River-specific control is added or changed. The exact-count sparse riverbed proof remains a single project-level menu action:

```text
Tools > PS3D > Run Generated Mass Sparse Riverbed Assembly Proof
```

The action now validates a more homogeneous micro-noise substrate and reports additional macro-homogeneity metrics, but it still writes evidence only under `Library/SurfaceMaterialDiagnostics/GeneratedMassSparseRiverbedAssembly` and still blocks runtime integration until a candidate is visually accepted.

Historical sections below that mention the retired handmade sparse-riverbed candidate synthesis command remain superseded context only.

---

## 2026-07-21 — M2.7C.5D.3 Inspector boundary note

M2.7C.5D.3 changes only the existing Editor menu assembly proof:

```text
Tools > PS3D > Run Generated Mass Sparse Riverbed Assembly Proof
```

The action generates one shared substrate and three nested exact-count `6 / 9 / 12` Generated Mass rock candidates, runs two deterministic suites, validates frozen sources, placement prefixes, unique-source use, coverage guardrails, scale distribution, spacing/hotspot limits, root sectors, seams, substrate statistics, output dimensions and fingerprints, writes one report, copies it to the clipboard, and writes local evidence under `Library/SurfaceMaterialDiagnostics/GeneratedMassSparseRiverbedAssembly`.

It adds no `GeneratedGround` field, foldout, selector, action button, debug view, Scene overlay, serialized asset, material/profile entry, component, layer, tag, or runtime binding. `SubstrateOnly` and all candidate outputs remain diagnostic files rather than selectable Ground resources.

The retired handmade action `Tools > PS3D > Run Sparse Riverbed Candidate Synthesis` and its two active `.cs` scripts are removed. Historical sections that name that command are superseded records and must not be treated as current workflow.

Runtime/Inspector integration remains blocked until the user visually accepts a complete substrate-first candidate.

---

## 2026-07-21 — M2.7C.5D.2 Inspector boundary note

M2.7C.5D.2 changes only the existing Editor menu assembly proof. It adds no GeneratedGround field, foldout, selector, action button, debug view, Scene overlay, serialized asset, material/profile entry, component, layer, tag, or runtime binding.

The active evidence command remains:

```text
Tools > PS3D > Run Generated Mass Sparse Riverbed Assembly Proof
```

The command now produces three replacement candidates—Very Quiet, Quiet, and Natural—using lighter periodic mud, broader weighted size variation, explicit quiet composition, and deterministic presentation reframing. The Dense M2.7C.5D.1 candidate is superseded. Runtime Inspector ownership remains reserved for M2.7C.5E after one complete tile is visually accepted.

Previous Inspector history follows below.

## 2026-07-21 — GSU-M2.7C.5D.1 inspector boundary note

M2.7C.5D.1 adds no GeneratedGround Inspector field, foldout, selector, debug view, scene overlay, serialized profile, runtime material entry or component. The new command is a project-level Editor evidence action:

`Tools > PS3D > Run Generated Mass Sparse Riverbed Assembly Proof`

It writes only local `Library/SurfaceMaterialDiagnostics/GeneratedMassSparseRiverbedAssembly` evidence. Candidate selection and all runtime/Inspector integration remain deferred to separately approved M2.7C.5E after one assembled tile is visually accepted.

## 2026-07-21 — M2.7C.5C.2.2 inspector boundary note

M2.7C.5C.2.2 adds no GeneratedGround control, foldout, debug view, scene overlay, component, material/profile asset, or runtime setting. It freezes the Editor-only isolated-rock response at unified `0.52` / fallback `0.56` and relaxes validation so deterministic historical source-geometry drift is reported as a warning rather than a hard failure. The next authorized work remains `M2.7C.5D` seamless tile assembly.

## 2026-07-21 — M2.7C.5C.2.1 inspector boundary note

This patch changes only Editor evidence calibration for the frozen river-rock library. It does not add Generated Ground Inspector controls and does not alter runtime material integration. The accepted midpoint is a baked-data decision: slightly lower unified-wear accent normalization and slightly higher fallback-wear normalization, with all geometry and non-accent channels frozen. Inspector or runtime exposure remains deferred until an assembled M2.7C.5D tile is visually accepted.

Earlier inspector audit history continues below.

## 2026-07-21 — M2.7C.5C.2 Inspector note

M2.7C.5C.2 adds no GeneratedGround control, foldout, debug view, scene overlay, component, material/profile asset, or runtime setting. It only calibrates the Editor-only evidence rendering of the frozen 18-rock library. The production-target Moderate response is intentionally subtle so internal accents do not become line artifacts when riverbed rocks are small on screen.

No Inspector or runtime integration is authorized until the later M2.7C.5D assembled tile is visually accepted and M2.7C.5E is separately approved.

Earlier inspector-audit content follows below.

## 2026-07-21 — GSU-M2.7C.5C.1 Inspector ownership

M2.7C.5C.1 remains a project-level Editor evidence correction. It reuses the existing menu action:

`Tools > PS3D > Run Generated Mass River-Rock Material Refinement`

The action now emits independent upward-exposure and directional-response evidence, broader broken root-contact sectors, visible interior wear, six-rock response close-ups, and fixed-frame burial comparison. It adds no `GeneratedGround` field, foldout, selector, action button, debug view, Scene overlay, serialized asset, profile, layer, material binding, or runtime behavior.

The frozen 18-rock library and source generation remain unchanged. Inspector/runtime ownership is still reserved for M2.7C.5E after M2.7C.5D seamless tile acceptance.

## 2026-07-21 — GSU-M2.7C.5C Inspector ownership

M2.7C.5C remains a project-level Editor evidence workflow and adds no Generated Ground Inspector controls, foldouts, debug views, serialized fields, scene overlays, or runtime bindings.

The active command is:

`Tools > PS3D > Run Generated Mass River-Rock Material Refinement`

It generates the frozen 18-rock library twice, validates exact IDs/settings and deterministic fingerprints, writes Neutral/Moderate/Strong material comparisons plus processed data audits and burial evidence under `Library/SurfaceMaterialDiagnostics/GeneratedMassRiverRockProjection`, writes one report, and copies the report to the clipboard.

Inspector/runtime ownership remains deferred:

- M2.7C.5C selects material response and burial treatment only.
- M2.7C.5D assembles and visually accepts a seamless tile without runtime Inspector integration.
- M2.7C.5E may expose the accepted material through the existing Ground material/profile architecture after explicit approval.

Earlier Inspector audit history follows below.

## 2026-07-21 — GSU-M2.7C.5B.2 Inspector ownership

The focused Uneven Broad expansion remains a project-level Editor evidence workflow. The only active command remains:

```text
Tools > PS3D > Run Generated Mass River-Rock Family Sweep
```

The command now generates 32 Terrain/Squat Uneven Broad sources, preserves seven frozen accepted IDs, writes raw and processed channel evidence, performs the accepted-anchor 4 × 4 burial comparison, runs the complete build twice, writes one report under `Library/SurfaceMaterialDiagnostics/GeneratedMassRiverRockProjection`, and copies that report to the clipboard.

M2.7C.5B.2 adds no `GeneratedGround` field, foldout, selector, action button, debug view, Scene overlay, serialized asset, material profile, layer, or runtime binding. Source selection remains evidence-only. Inspector/runtime ownership remains reserved for M2.7C.5E after source refinement and seamless tile acceptance.

The authoritative progression is M2.7C.5B.2 focused selection → M2.7C.5C selected-rock material/burial refinement → M2.7C.5D seamless tile assembly → M2.7C.5E runtime Ground integration.

Previous Inspector history follows below.

## 2026-07-20 — GSU-M2.7C.5B Inspector ownership and stale-tool retirement

M2.7C.5B remains a project-level Editor evidence workflow. The only active river-rock evidence command is:

```text
Tools > PS3D > Run Generated Mass River-Rock Family Sweep
```

It generates the labelled Terrain/Squat catalog, runs deterministic validation twice, writes the complete report and evidence under `Library/SurfaceMaterialDiagnostics/GeneratedMassRiverRockProjection`, and copies the report to the clipboard.

The old donor-extraction command and its two scripts are retired. Historical M2.7B Inspector sections below describe superseded evidence and must not be treated as an active menu or workflow. The previously retired handmade candidate-synthesis commands likewise remain absent.

This patch adds no GeneratedGround foldout, field, selector, debug view, Scene overlay, serialized recipe, or runtime binding. Stable source-rock IDs remain evidence-only until M2.7C.5C selection/refinement and M2.7C.5D tile acceptance are complete. Runtime Inspector integration is reserved for M2.7C.5E.

---

## 2026-07-20 — GSU-M2.7C.5A Inspector ownership note

The failed handmade sparse-riverbed candidate generator is retired and no Inspector integration is added. The replacement is an explicit Editor menu evidence action that projects real Generated Mass meshes into local `Library` outputs. No GeneratedGround control, serialized field, profile, layer, or runtime binding is authorized until the projected individual-rock catalog is visually accepted.

Historical Inspector material notes continue below.

## 2026-07-20 — M2.7C.4 note

This patch does not change the Generated Ground runtime inspector. It only updates the local sparse-riverbed synthesis evidence generator and the canonical planning docs. The inspector-facing implication is simply that any future inspector integration must not inherit the rejected M2.7C.3 rock grammar. If these candidates later pass visual review, the accepted M2.7C.4 grammar becomes the minimum baseline for any real inspector/runtime integration work.

Earlier inspector audit content continues below.

# GeneratedGround Inspector and Painted Accent Production Architecture


## GSU-M2.7C.3 Inspector contract — facet-owned synthesis remains a project-level evidence action

The only approved entry point remains:

```text
Tools > PS3D > Run Sparse Riverbed Candidate Synthesis
```

The action performs two complete deterministic Editor-only runs, enforces coverage and 32x32 occupied-block budgets, validates final placed-stone structure rather than source-motif averages, writes one report, copies it to the clipboard, and writes the existing candidate evidence plus `FinalStructureDebug.png` under `Library/SurfaceMaterialDiagnostics/SparseRiverbedCandidates`.

GSU-M2.7C.3 adds no `GeneratedGround` foldout, debug view, Scene overlay, component, candidate selector, density control, macro-region control, facet control, burial/contact control, seed control, or River-specific field. Candidate definitions and diagnostic thresholds remain internal evidence-generation contracts. The generated candidates cannot appear in Bank, Riverbed, road, wall, or ordinary Ground selectors until a later separately approved runtime integration patch promotes one visually accepted result.

### Superseded direction

The M2.7C.2 Inspector contract remains historical. Its single menu location and no-runtime/no-Inspector ownership are preserved, while its candidate definitions and source-motif candidate validation are superseded by M2.7C.3.

## GSU-M2.7C.2 Inspector contract — feature-rich synthesis remains a project-level evidence action

The single approved entry point remains:

```text
Tools > PS3D > Run Sparse Riverbed Candidate Synthesis
```

GSU-M2.7C.2 adds no `GeneratedGround` foldout, debug view, Scene overlay, component, candidate selector, density slider, stone-feature control, seed control, or River-specific authoring field. Crown, edge, burial, facet, and local-feature selection are deterministic Editor-generation internals and are reported through the copied synthesis report and Library-only evidence images.

The evidence candidates cannot appear in Bank, Riverbed, road, wall, or ordinary Ground selectors. Only a later separately approved integration patch may promote one accepted result through the existing reusable material/layer selector contract.

### Superseded direction

The M2.7C.1 Inspector contract remains historical only. The menu location and no-runtime/no-Inspector ownership are preserved, but its candidate definitions and smooth-crown evidence are superseded by M2.7C.2.


## GSU-M2.7C.1 Inspector contract — procedural synthesis remains a project-level evidence action

Procedural rounded-stone candidate generation remains available only through:

```text
Tools > PS3D > Run Sparse Riverbed Candidate Synthesis
```

The action performs two complete deterministic runs, validates motif bounds, fingerprints, measured coverage, quiet areas, seam metrics, and mip occupancy, writes one report, copies it to the clipboard, and writes candidate plus motif-catalog evidence under `Library/SurfaceMaterialDiagnostics/SparseRiverbedCandidates`.

GSU-M2.7C.1 adds no `GeneratedGround` foldout, debug view, Scene overlay, component, source field, candidate selector, density slider, seed control, or per-River setting. Procedural evidence candidates are not `GroundSurfaceLayerProfile` assets and cannot appear in Bank, Riverbed, road, wall, or ordinary Ground selectors. Only a separately approved later integration patch may add one accepted reusable material through the existing selector contract.

## GSU-M2.7C Inspector contract — superseded donor-stamp evidence action

Sparse riverbed candidate synthesis is invoked only through:

```text
Tools > PS3D > Run Sparse Riverbed Candidate Synthesis
```

The action runs the accepted Stone-Ground-only synthesis twice, validates deterministic fingerprints and measured coverage, writes one comprehensive report, copies it to the clipboard, and writes candidate color/mask/height/cavity/normal/roughness/repeat/mip/placement evidence under `Library/SurfaceMaterialDiagnostics/SparseRiverbedCandidates`.

M2.7C adds no `GeneratedGround` foldout, debug view, Scene overlay, component, source-map field, candidate selector, density slider, seed control, or per-River setting. The three evidence candidates are not `GroundSurfaceLayerProfile` assets and must not appear in Bank, Riverbed, road, wall, or ordinary Ground selectors. Only an explicitly accepted and later integrated M2.7D material may enter the existing surface-layer selector.

## GSU-M2.7B Inspector contract — extraction remains outside GeneratedGround

Donor extraction is a single explicit project-level Editor action:

```text
Tools > PS3D > Run Sparse Riverbed Donor Extraction
```

It writes one comprehensive report, copies the report to the clipboard, and writes accepted/rejected evidence images under `Library/SurfaceMaterialDiagnostics/SparseRiverbedDonors`. No `GeneratedGround` selection, component, top-level foldout, debug view, Scene overlay, source-map field, per-River control, or serialized extraction setting is added. Dense donor coverage is diagnostic input only; no Inspector value inherits or exposes it as final synthesis density.

M2.7B donor review is complete. Its extracted catalogs remain historical evidence only; GSU-M2.7C.1 procedural generation owns the active candidate evidence. Only a later accepted reusable Ground layer may enter the existing surface-layer selector.

## GSU-M2.7A.1 Inspector contract — no placeholder material for empty libraries

A detail library with zero logical entries exposes no selectable material entry. The required one-slice generated packed backing is internal array storage and must not appear as a `GroundSurfaceLayerProfile`, `StylizedSurfaceMaterialProfile`, detail-entry popup option, Ground selector item, debug view, or GeneratedGround control.

The existing material-profile editor continues showing no selectable detail entry when the assigned library is logically empty. Runtime resolution remains false for every stable ID, so current Ground layers without retained detail entries render through their ordinary fallback or selected retained surface rather than the internal backing slice. The retirement migration report is the only user-facing evidence for this transient empty-library state; no new GeneratedGround button, foldout, component, or scene workflow is introduced.

## GSU-M2.7A Inspector contract — direct imported candidates retired

Stone Ground 01 and Black Gravel 01 are removed from the reusable Ground-layer selector after the retirement migration succeeds. The migration never rewrites a current selection silently: if a scene, prefab, shared style, or other asset still references either retired layer/material, it aborts before mutation and copies a report listing every referencer. The author must select Pale Sand or another retained layer and rerun the cleanup.

Donor source maps remain under editor-only ArtSources and are not exposed in `GeneratedGround`, `GroundSurfaceLayerProfile`, or ordinary material-selection controls. The active procedural sparse-riverbed synthesizer owns candidate generation through dedicated Editor diagnostics and does not consume those donor maps; GeneratedGround will receive only an accepted later reusable layer. No new top-level GeneratedGround foldout, debug view, scene component, or per-River donor control is authorized by GSU-M2.7A.

The existing shared material editor and array validation infrastructure remain available for retained materials and later synthesized candidates. Direct full-cover imported material-set assets are historical only and must not reappear in selector lists.

## 2026-07-20 — GSU-M2.6 Black Gravel 01 selector contract

Black Gravel 01 adds no Inspector control or section. After the one-time import creates `GSLP_BlackGravel01`, the existing `GeneratedGround` surface-layer selector discovers it through the normal `GroundSurfaceLayerProfile` asset query and displays:

```text
Black Gravel 01 — GSLP_BlackGravel01
```

Shared material editing uses the existing single-palette and structural-detail controls. Ground-local Bank/Riverbed multipliers retain their current capability-aware visibility. No material-specific branch, debug view, top-level foldout, source-map field, or migration control is added to `GeneratedGroundEditor`.


## 2026-07-19 — GSU-M2.4.1 transition control removal

The four M2.4 material-transition controls and whole-stone diagnostic output are removed. The replacement binary substrate boundary has no author-facing tuning: authored texture-form materials use the fixed combined-support cut, while prepacked/continuous materials keep smooth interpolation. The dedicated material Inspector and GeneratedGround inline material editor therefore return to the accepted M2.3 control contract, and `Run Surface Material Validation` returns to the M2.3 palette/form/seam/control-integrity report.

No new Inspector section, debug view, material field, River multiplier, asset workflow, or validation button is introduced.

## 2026-07-19 — GSU-M2.3 single-palette control contract

The dedicated `StylizedSurfaceMaterialProfile` Inspector and GeneratedGround's inline `Shared Material Definition` expose one `Palette` section only. `Payload Mode`, `Authored Color`, and authored tint controls are hidden compatibility data. Imported material-set entries are detected automatically from the selected detail-library entry and expose only `Texture Form Strength`, `Scene Lighting Response`, and `Roughness Variation` in their structural/finish groups. Prepacked entries continue to expose packed value/form and finish variation instead.

Bank and Riverbed `This River Application` panels are capability-aware. They show a multiplier only when the resolved reusable material has a nonzero coefficient for that behavior: detail scale, texture form, scene-lighting response, normal, cavity, packed value/form, roughness or finish variation, and legacy cell influence. A selected material with no active application coefficients produces one explanatory message rather than inert controls. Shared palette colours, cavity bias, natural scale, and dry-finish baselines remain material-owned.

The existing one-button `Run Surface Material Validation` action remains the sole surface-material diagnostic workflow and clipboard handoff. Its report now includes automatic source-mode resolution, grayscale form percentiles, Dark/Base/Light band coverage, periodic seam evidence, source/form 3x3 diagnostics, and a Control Integrity section. No new top-level GeneratedGround group, debug view, scene component, prefab workflow, or manual multi-run validation is introduced.

## 2026-07-19 — GSU-M2.2 authored-colour palette controls

Inside the existing `Shared Material Definition`, Authored Color materials label the four existing fields as `Palette (Applied to Authored Color)`. Base, Dark, Light, and Cavity Color are the primary authored-surface colour controls; they grade the imported value structure instead of being bypassed at Authored Color Strength 1. The existing authored tint remains an optional secondary tint. The dedicated `StylizedSurfaceMaterialProfile` Inspector mirrors the same label, explanation, and preview response.

No new top-level foldout, profile field, material-specific branch, debug view, River control, asset workflow, or serialized migration is added. The controls continue editing the shared reusable material and therefore affect every Ground, River, road, wall, or other consumer of that profile.

## 2026-07-19 — GSU-M2.1 validation extension

The existing **Run Surface Material Validation** action remains the only GeneratedGround-facing control. It now rebuilds stale authored-colour arrays using the periodic generation algorithm, reports pre/post opposite-edge mean and p95 ratios for every authored-colour mip, fails output above the recorded thresholds, writes source-derived and generated mip 0–3 three-by-three PNGs under `Library/SurfaceMaterialDiagnostics`, saves the complete text report, and copies that report to the clipboard. Shared Bank/Riverbed material entries emit one diagnostic set even when both regions use the same material.

No new Inspector button, top-level group, foldout, debug view, serialized control, profile schema, scene workflow, or runtime diagnostic is introduced. The additional work occurs only when the existing validation action or stale-library rebuild runs in the Editor.

## 2026-07-19 — GSU-M2.0 Inspector extension

The existing River Bank and Riverbed groups now expose two additional neutral application multipliers for authored-colour strength and retained scene-lighting strength. Inline **Shared Material Definition** editing exposes payload mode, authored colour/tint, lighting preservation, and roughness response. **Run Surface Material Validation** is one Inspector action that rebuilds stale arrays, writes a report under `Library/SurfaceMaterialDiagnostics`, and copies the complete report to the clipboard. No new top-level Inspector section or debug view is introduced.


## Status

**Workstream state: complete, Unity-validated, and accepted on 2026-07-15. Broader Ground development remains active.**

This document closes only the GeneratedGround Inspector overhaul and the Painted Accent authoring, rendering, production-bake, build-validation, and generated-asset lifecycle. It does not declare the Ground visual system complete. V3M, V3R, and V3S-A4B.3 are accepted. GSU-M1 is implemented and source-audited through GSU-M1.9A.5. M1.9A through M1.9A.4 are visually superseded; M1.9A.5 is the active source-art packed-conversion evaluation and awaits Unity comparison. GSU-M1.7 retains the existing River Bank/Riverbed groups, inline shared-material editing, and neutral per-application detail multipliers; GSU-M1.7.1 corrects its editor-only `EntityId` compile blocker. GSU-M1.8 retains authority for the 256 runtime tier. M1.9A.3 changes no Inspector code. Unity recompilation, material visual/performance validation, and sequential material expansion remain pending; V4 Contact / Edge Accents remains queued afterward and excludes River sources.

The implemented V3S Inspector correction replaces the five scattered River-coupled foldouts with exactly two region-oriented groups: `River-Coupled Ground Response — River Bank` and `River-Coupled Ground Response — Riverbed`. The Bank group owns Bank substrate, coverage, cover response, Shore wetness, and the metre-authored Shore highlight band. The Riverbed group owns explicit surface-source inheritance, custom Riverbed profile authoring, submerged-cover status, Riverbed hydrology, inward edge transition, and submerged-finish suppression. Unity validation accepts this grouping through V3S-A4B.3. Inline profile editors remain collapsed by default, and the delayed asset-creation workflow introduced by A3B.1 remains mandatory. No new top-level foldout, debug view, profile schema, or asset workflow was added by A4B.3.

Accepted implementation status:

- **GI-A1 — Inspector skeleton and authority correction:** Unity-validated and accepted.
- **GI-A2 — Unified inline shared/local authoring:** Unity-validated and accepted.
- **GI-A3 / GI-A3.1 — Painted Accent visibility contract and Unity 6.5 warning cleanup:** Unity-validated and accepted.
- **GI-A4 — Diagnostics separation and cleanup:** Unity-validated and accepted.
- **PA-B1 / PA-B1.1 — One-button persistent Painted Accent bake and compile correction:** Unity-validated and accepted.
- **PA-B2 / PA-B2.1 — Baked-only Play Mode and Player rendering plus persistent-texture naming correction:** Unity-validated and accepted.
- **PA-B3 — Exact production validation and hard build enforcement:** Unity-validated and accepted.
- **PA-B4 / PA-B4.1 — Project-wide generated-asset audit, conservative orphan cleanup, and compile correction:** Unity-validated and accepted.

The latest project source is authoritative if it conflicts with this document. Patch-local status and “Next work items” sections later in this file are retained as historical sequencing evidence and are superseded by this final status.


### GSU-M1.7 material-authoring Inspector contract — implemented; Unity validation pending

The existing `River Bank` and `Riverbed` top-level groups remain unchanged. Their `GroundSurfaceLayerProfile` selectors continue to choose Ground adapters. Inside the existing `Material & Layer Settings` editor, the adapter exposes the generic `StylizedSurfaceMaterialProfile`, Ground-only cover compatibility, and a collapsible `Shared Material Definition`. The shared foldout edits palette, broad response, structural detail, cavity/form, natural scale, and dry finish directly on the reusable asset and displays an explicit warning that the change affects every Ground, River, road, wall, or other consumer. Legacy layer appearance fields remain serialized fallback data and are shown only when no generic material is assigned.

Each existing Bank and Riverbed subsection now also contains `This River Application`. It owns only neutral multipliers for detail scale, normal strength, cavity strength, value/form, finish variation, and retained legacy pixel-cell influence. Defaults are one. Riverbed keeps independent application values even when it inherits the Bank layer. These values do not duplicate palette, texture, cavity bias, or dry-finish ownership and do not affect hydrology, cover, River masks, geometry, or UV3.

`StylizedSurfaceDetailLibrary` retains its dedicated editor-only rebuild/validation Inspector with missing/stale repair. `StylizedSurfaceMaterialProfile` retains its dedicated Inspector and optional horizontal/vertical diagnostic preview, but that hidden Preview pane is not an acceptance gate. Source texture references remain editor-only; the generated array is the runtime payload. No new GeneratedGround top-level foldout, debug view, modal save dialog inside live IMGUI layout, scene component, or prefab workflow is introduced. The GSU-M1.6 statement that no River-facing controls were authorized is superseded by this contract; no material-name-specific control or shader branch is introduced.

### GSU-M1.7.1 Inspector compile correction — implemented; Unity revalidation pending

The shared-material foldout cache now uses `Dictionary<EntityId, bool>`, and `DrawStylizedSurfaceMaterialInlineEditor` retains `profile.GetEntityId()` as an `EntityId`. This removes the Unity 6000.5-obsolete implicit conversion to `int` without changing foldout behavior, serialized data, Inspector layout, or any runtime path. No other editor or runtime file is changed by this correction.


### GSU-M1.9A texture-only correction — visually rejected; historical

GSU-M1.9A does not alter the Inspector contract. `Shared Material Definition` and `This River Application` remain the authoring paths from GSU-M1.7, with `EntityId` foldout keys from GSU-M1.7.1. The hidden material Preview pane remains optional assistance only. Texture acceptance is based on packed-map/height evidence and the actual production-camera River/GeneratedGround result.

### GSU-M1.8 payload-only correction — implemented; Unity validation pending

GSU-M1.8 does not alter the Inspector contract. `Shared Material Definition` and `This River Application` remain the current authoring paths from GSU-M1.7, with `EntityId` foldout keys from GSU-M1.7.1. The default detail library and Fine Gravel importer return to 256², and only Fine Gravel material/profile payload data changes. The hidden material Preview pane remains optional diagnostic assistance and is not acceptance evidence.

### V3S-A4B.3 Inspector contract — frozen

The existing two region-oriented top-level groups remain unchanged. Under `River Bank > Shore Wetness > Wet Highlight Shaping`, A4B.3 adds only `Highlight Width` and `Highlight Feather` before the existing strength/tightness/camera controls. Under `Riverbed > Wetness > Wetness Transition`, the existing serialized transition fields are displayed as `Riverbed Edge Transition Distance` and `Riverbed Edge Transition Softness`; their behavior is an inward Riverbed-only transition. The implementation preserves local and shared-style bindings and the existing delayed asset workflow. Unity validation and user visual acceptance are complete. No new top-level foldout, debug view, profile editor, profile schema, or asset workflow was added. This Inspector contract is frozen with the A4B.3 baseline.

## Final accepted production contract

```text
GeneratedGround is the unified authoring surface
→ Edit Mode builds the authoritative mesh-free procedural preview
→ Ink Colour and Ink Opacity update through Material only
→ Bake Painted Accents creates or updates one automatically owned persistent R8 asset
→ Play Mode and Player bind only the persistent production texture
→ no runtime SurfaceStroke generation
→ no runtime ProjectedGlyph or companion-cluster solving
→ no runtime coverage rasterization or CPU upload
→ build validation blocks Missing, Stale, Incompatible, duplicate, shared, or ownership-mismatched output
→ project audit finds generated assets that no longer have a legitimate owner or reference
→ only confirmed orphans may be deleted through an explicit reviewed action
```

The ordinary author workflow is:

```text
Author in GeneratedGround
→ Bake Painted Accents
→ validate or build
```

Generated-output maintenance is:

```text
Release Production Bake when an owning Ground no longer needs it
→ save the scene manually
→ Tools > Generated Ground > Audit and Clean Painted Accent Assets...
→ review the dry-run report
→ delete only Confirmed orphan assets
```

## Non-negotiable constraints

- `GeneratedGround` must become the central Ground authoring surface.
- Shared recipes, variants, profiles, and material controls may remain their architectural owners, but authors must be able to edit the resolved values from `GeneratedGround`.
- Shared-versus-local ownership must be explicit in the Inspector.
- Do not silently duplicate, clone, or migrate shared data.
- Do not modify or package Unity scenes or prefabs.
- Do not restore the retired 3D Painted Accent ridge path.
- Preserve the accepted PA-P1 through PA-P4 generation and performance baseline.
- Production Painted Accents use a zero-setup, one-button persistent output workflow.
- Play Mode and Player use only persistent production coverage; missing or stale output never triggers runtime procedural generation.
- Player builds are blocked when required output is missing, stale, incompatible, duplicated, shared, or ownership-mismatched.
- Generated outputs are removed only through the explicit project-wide audit and confirmed-orphan cleanup workflow.

## Audit summary

The original authoring problem was not a single missing control. It combined poor information architecture, hidden shared ownership, mismatched Inspector/runtime authority, passive serialized-data mutation, and a Painted Accent rendering configuration that was nearly imperceptible in normal lit rendering. GI-A1 through PA-B4 resolved that workstream; the sections below preserve the evidence and accepted decisions.

### Proven structural problems

1. The old `GeneratedGroundEditor` order followed implementation history instead of author workflow.
2. Ground debug and regeneration accounting appeared before basic geometry.
3. Painted Accent authoring and Painted Accent diagnostics were separate top-level sections.
4. `Surface Detail`, which changes geometry, was placed under `Surface` rather than Ground shape.
5. `Patch Coordinate` was isolated in an unrelated `Advanced` foldout.
6. the main Inspector exposed local material overrides but not the active shared variant material controls.
7. ordinary shader feature controls for Directional Streaks, Pooled Wetness, and Trampled Wear were inaccessible from `GeneratedGround`.
8. diagnostics and copy actions were duplicated across sections.
9. foldouts are editor-instance booleans and do not currently persist across Inspector recreation.

Primary implementation file:

```text
Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
```

### Proven authority defect

Before GI-A1, the inline Painted Accent editor searched the selected variant's serialized feature array and returned the first entry whose kind was `PaintedAccentLines`.

Runtime uses:

```text
GroundSurfaceVariantRecipe.TryGetFirstShaderFeature(requiredKind, out feature)
```

A runtime-applicable entry must be:

- non-null;
- enabled;
- `ShaderOnly`;
- the required feature kind;
- `Strength > 0`.

Therefore, with an earlier disabled, non-shader, or zero-strength Painted Accent entry followed by a valid entry, the old Inspector edited data the renderer ignored.

Relevant files:

```text
Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
Game/Procedural/Ground/GroundSurfaceVariantRecipe.cs
Game/Procedural/Ground/GroundSurfaceFeatureRecipe.cs
```

GI-A1 resolves the actual runtime feature through `TryGetFirstShaderFeature`, maps that object back to its serialized array entry, and reports ignored or duplicate entries.

### Proven passive mutation defect

Both Ground style editors previously initialized Painted Accent compatibility fields merely because their controls were drawn.

Affected files:

```text
Game/Procedural/Ground/Editor/GeneratedGroundEditor.cs
Game/Procedural/Ground/Editor/GroundSurfaceStyleProfileEditor.cs
```

The passive writes included initialization flags and defaults for:

- companion participation/tightness;
- triplet verticality;
- triplet share and cluster region bias;
- pair/triplet layout weights;
- glyph family weights;
- stroke path wiggle.

Both editors also silently raised `Stroke Length Max` when it was below `Stroke Length Min + 0.05 m`.

GI-A1 removes those draw-time writes. Legacy recipes now show an explicit **Initialize Painted Accent Authoring Values** action. Invalid length separation is reported as a warning while runtime compatibility clamping remains unchanged.

### Proven Painted Accent visibility problem

The active demo Ground resolves Snowfield / Clean and does not use a local material override. The serialized Painted Accent configuration is approximately:

```text
Strength:       0.08
Stroke Width:   0.017 m
Ink RGB:        0.578, 0.593, 0.604
Ground RGB:     0.807, 0.870, 0.906
Patch:          40 m
Coverage:       2048 × 2048 R8
```

Relevant asset and scene evidence:

```text
Game/Demo/Profiles/Ground/Styles/GSSP_Snowfield.asset
Game/Demo/Scenes/VisualFrameworkDemo.unity
```

The scene and asset were inspected only. They must not be modified by these patches.

Before GI-A3, the render path was:

```text
coverage sample
× contract mask
× _GroundPaintedAccentLineStrength
× ink alpha
→ albedo lerp toward ink RGB
```

Relevant files:

```text
Game/Procedural/Ground/GeneratedGround.cs
Game/Rendering/PixelSurface/PixelSurfaceGroundResponse.hlsl
Game/Rendering/PixelSurface/PixelSurfaceGroundForwardPass.hlsl
```

At full coverage and full contract mask, `Strength = 0.08` limits the visible blend to eight percent. The pale ink changes the pale Snowfield base by only roughly 0.018–0.024 per colour channel.

Coverage texel size on a 40 m / 2048 texture is approximately `0.01953 m`. The authored `0.017 m` line is approximately `0.87 texel` wide before partial raster coverage and bilinear filtering.

Conclusion:

- in normal lit rendering, the active configuration is nearly imperceptible by construction;
- nonzero generated coverage counters do not prove visible final rendering;
- GI-A3 added raw-binding and contract-coverage debug views, and Unity validation confirmed the corrected production binding and visible rendering path; no separate binding/mapping blocker remains in this workstream.

### Proven control issues

Painted Accent generic fields before GI-A3:

- `Strength` controlled final shader visibility and whether the feature was runtime-applicable.
- `Scale` was included in invalidation/signatures but was not consumed by the Painted Accent generator.
- `Contrast` was copied into placement settings but was not consumed afterward.
- generic `Direction` was ignored because Painted Accents use Facing Direction Degrees.
- Ink alpha was forced to one by `PaintedAccentInkColor`; there was no real independent Ink Opacity control.

GI-A3 retains `Strength` as **Stroke Intensity** because it is genuinely consumed by the SurfaceStroke generator and contributes to generated stroke strength and slight profile amplitude. It no longer controls final albedo opacity. Dead generic Scale and Contrast values are no longer drawn for Painted Accents and no longer participate in the Painted Accent SurfaceStroke signature. Generic Direction remains hidden for Painted Accents.

Hidden but active fields not previously available in the inline Ground controls:

- Enabled;
- Mask Influence;
- Seed Offset;
- Strength/visible contribution.

Retired compatibility-only fields:

- `paintedAccentDistributionSparseFloor`;
- `paintedAccentCompositionRegionScale`;
- `paintedAccentCompositionDensityContrast`.

GI-A3.1 removes these unused private fields from the C# recipe definition. Existing serialized YAML may retain the unknown keys until the user later saves the owning style asset; the patch does not modify or include any style asset. The active distribution controls already derive the required sparse-floor and composition behavior directly from Distribution Scale and Distribution Contrast.

## Data ownership decision

`GeneratedGround` becomes the authoring façade, but underlying ownership remains coherent:

| Data | Owner |
|---|---|
| Patch/geometry recipe | individual `GeneratedGround` |
| Family and variant recipes | shared `GroundSurfaceStyleProfile` |
| default semantic profile | shared `GroundSurfaceProfile` |
| existing local material override | individual `GeneratedGround` |
| feature recipes | selected shared variant |
| generated diagnostics | individual `GeneratedGround`, read-only |
| persistent Painted Accent production output | automatically managed Ground-owned generated R8 resource |

The Inspector must state scope explicitly, for example:

```text
Editing Shared Style — Snowfield / Clean
Changes affect every GeneratedGround using this variant.
```

or:

```text
Editing Local Material Override
Changes affect this GeneratedGround only.
```

Do not duplicate every style value onto the component. Do not silently clone style assets. A future local-style override, if approved, must be one deliberate coherent model rather than many fragmented override toggles.

## Approved Inspector information architecture

### 1. Ground Overview

- Surface Family
- Surface Variant
- deterministic identity: Shape Seed and Patch Coordinate
- style/variant warnings
- resolved surface profile
- feature summary
- direct advanced style reference only as a secondary escape hatch

### 2. Ground Geometry

#### Patch Domain

- Patch Size
- Mesh Resolution
- calculated dimensions, vertex count, and triangle count

#### Base Shape

- Profile
- Broad Form
- Roughness
- Surface Detail
- Edge Blend

#### Mountain Transition

- Direction
- Height Change

### 3. Surface Appearance

- Material Variation
- resolved GroundSurfaceProfile values edited inline through their actual profile asset owner
- resolved shared variant material controls edited inline
- optional local material override controls edited inline
- explicit ownership banner for shared profile, shared variant, or local component values

### 4. Surface Features

- Directional Streaks
- Pooled Wetness
- Trampled Wear
- first runtime-applicable recipe is authoritative
- when no recipe is runtime-applicable, the first matching recipe remains editable so Enabled, Execution Path, and visible intensity can restore it
- reserved and duplicate recipe warnings remain explicit

### 5. Painted Accents

Current GI-A2 authoring exposes the resolved shared recipe while preserving runtime authority and explicit ownership.

Final intended subgroups:

- Enable and Visibility
- Distribution
- Stroke Shape
- Shape Families
- Companion Clusters
- Surface Eligibility
- Preview and Production

Debug overlays and reports do not belong in this authoring section.

### 6. Ground and Environment Interaction

- Use Modifiers
- discovered Ground Modifier count
- discovered River count
- link refresh action belongs in Regeneration and Caching because it performs a regeneration

### 7. Regeneration and Caching

- Live Regeneration
- Randomize Shape Seed
- Regenerate Ground
- Refresh Modifier and River Links + Regenerate
- later stage/cache state summaries

Button labels must state their real scope.

### 8. Debug and Diagnostics

Collapsed by default:

- Ground Material Debug
- Last Surface Mask Diagnostics
- Painted Accent Debug and Reports
- Editor Regeneration Accounting

The accepted Projected Glyph report must not depend on a Scene overlay toggle.

The transient R8 report must be called **Last Coverage Raster**, not a production bake.

## GI-A1 implementation record

GI-A1 changes only editor code and documentation.

Implemented:

1. Reordered and regrouped the main Inspector into the approved top-level skeleton.
2. Moved Shape Seed and Patch Coordinate into Ground Overview.
3. Grouped Patch Domain, Base Shape, and Mountain Transition under Ground Geometry.
4. Moved Surface Detail into Base Shape.
5. Renamed Surface to Surface Appearance.
6. Renamed Modifiers to Ground and Environment Interaction.
7. Added Regeneration and Caching with explicit action names.
8. Moved Ground debug, surface diagnostics, Painted Accent debug/reports, and accounting under Debug and Diagnostics.
9. Removed the arbitrary Advanced section and ungrouped bottom buttons.
10. Changed Painted Accent lookup to follow the exact runtime resolver.
11. Added selected-variant warnings for null, unsupported, zero-strength, and duplicate runtime-applicable feature entries.
12. Removed draw-time compatibility initialization and automatic stroke-length mutation from both Ground style editors.
13. Added explicit compatibility initialization action.
14. Renamed transient coverage reporting to Last Coverage Raster.
15. Made Accepted Projected Baseline reporting independent of the projected Scene overlay toggle.

Explicitly not included in GI-A1:

- no new Ink Opacity data field;
- no visibility tuning;
- no shared material/profile inline authoring;
- no runtime or generator algorithm changes;
- no PA signature changes;
- no persistent bake;
- no scene, prefab, material, or style asset modification.


## GI-A2 implementation record

GI-A2 changes only the GeneratedGround custom editor and this canonical document. It does not alter runtime recipes, generators, shaders, scenes, prefabs, materials, or style assets.

Implemented:

1. Added a first-class **Surface Features** top-level section between Surface Appearance and Painted Accents.
2. Added inline authoring for the resolved `GroundSurfaceProfile` asset:
   - Patch Scale;
   - Patch Contrast;
   - Patch Edge Softness;
   - Exposure Bias;
   - Damp Deposit Bias;
   - Vegetation Suitability;
   - Rocky/Dry Suitability;
   - Snow Eligibility;
   - Rain Absorption.
3. Kept `Footprint Visibility` and `Grass Recovery Speed` hidden because they still have no active Ground consumer.
4. Replaced the old local-override-only material section with resolved ownership authoring:
   - shared variant material values are editable directly when no local override is active;
   - local material values remain editable when **Use Local Material Override** is enabled;
   - ownership and consequences are displayed before the controls.
5. Added inline shared-variant authoring for:
   - Directional Streaks;
   - Pooled Wetness;
   - Trampled Wear.
6. Feature controls include Enabled, Execution Path, Intensity, Scale, Contrast, Surface Mask Influence, Pattern Seed Offset, and Direction only where the shader actually consumes it.
7. Extended Painted Accent authoring with its previously hidden active controls:
   - Enable Painted Accents;
   - Execution Path;
   - current legacy Visible Strength;
   - Surface Suitability Influence;
   - Pattern Seed Offset.
8. Preserved GI-A1 runtime authority:
   - the first runtime-applicable recipe remains the default authoring target;
   - if none is applicable, the first matching recipe is exposed so it can be restored without opening the style asset;
   - duplicate and ignored-entry warnings remain visible.
9. Shared profile edits refresh every loaded scene Ground that resolves the same profile asset.
10. Shared variant material edits refresh every loaded scene Ground using that style/variant without a local material override.
11. Shared feature and Painted Accent edits refresh every loaded scene Ground using that style/variant, including Grounds with local material overrides because feature ownership remains shared.
12. Shared-asset editing is deliberately disabled for multi-object selections whose owner may differ. Component-local common controls remain available.
13. Editor refresh searches use `FindObjectsByType` without sorting and exclude persistent asset objects; there is no per-frame scan.
14. Undo/Redo refreshes loaded scene Grounds so restored shared assets and local overrides immediately reapply their resolved generation/material state.
15. Merely drawing the Inspector still performs no serialized migration or asset mutation.

Explicitly not included in GI-A2:

- no new feature recipe is invented when a selected variant has no recipe entry at all; recipe-list structure remains an explicit advanced style operation;
- no Ink Opacity field;
- no Painted Accent visibility tuning or shader change;
- no removal of dead Painted Accent Scale/Contrast signature inputs;
- no timing-history redesign;
- no persistent production bake;
- no scene, prefab, material, profile, or style asset modification in the patch.

### GI-A2 ownership and refresh contract

| Edited value | Serialized owner | Loaded consumers refreshed | Minimum intended work |
|---|---|---|---|
| Material Variation | selected `GeneratedGround` | selected Ground | existing component validation path |
| Surface Profile fields | referenced `GroundSurfaceProfile` asset | Grounds resolving that profile | profile-aware style refresh; full mask regeneration only when required and Live Regeneration permits it |
| Shared material controls | selected variant in `GroundSurfaceStyleProfile` | Grounds using the style/variant without local material override | Material |
| Local material controls | selected `GeneratedGround` | selected Ground | Material |
| Directional/Pooled/Trampled features | selected shared variant | all Grounds using the style/variant | Material |
| Painted Accent feature values | selected shared variant | all Grounds using the style/variant | signature-driven minimum PA stage plus Material |

### GI-A2.1 correction — shared material persistence and visible storage ownership

The inline shared-variant Material Controls path previously committed its separate `SerializedObject` only when the parent Inspector's immediate GUI change check reported a change. Unity colour-picker updates can arrive through the picker window without that exact parent event reporting the change, leaving an apparently edited palette value unapplied or only dirty in memory. A script compilation or assembly reload could then restore the last value actually stored in the style asset.

GI-A2.1 makes the serialized owner authoritative and explicit:

- the shared style `SerializedObject` is applied after every Material Controls draw; its actual `ApplyModifiedProperties()` result determines refresh work;
- changed `GroundSurfaceStyleProfile` assets are marked dirty and queued for a coalesced `AssetDatabase.SaveAssetIfDirty` call;
- pending style saves are flushed before assembly reload, so code changes cannot discard recently authored palette values;
- the Material Controls section shows a direct **Stored In** line for either the shared style asset and variant or the local scene/component override;
- local overrides retain their existing scene-serialization contract and still require the scene to be saved.

This correction changes only the GeneratedGround custom editor and this canonical document. It does not modify runtime resolution, shaders, materials, styles, profiles, scenes, prefabs, defaults, or existing serialized values.

## GI-A3 implementation record

GI-A3 changes the Painted Accent recipe contract, Ground material binding and diagnostics, the Ground shader, both Ground style authoring surfaces, and this canonical document. It does not alter SurfaceStroke placement, ProjectedGlyph composition, cluster solving, coverage rasterization, scenes, prefabs, materials, or style assets.

Implemented:

1. Added a dedicated serialized `Painted Accent Ink Opacity` value with a non-mutating compatibility contract:
   - newly authored recipes initialize it explicitly to `1.00`;
   - existing recipes whose initialization flag is absent resolve to `1.00` without rewriting the asset merely because an Inspector is drawn;
   - moving the slider records an explicit authored value.
2. Renamed legacy generic Painted Accent Strength to **Stroke Intensity** in both authoring surfaces.
3. Preserved Stroke Intensity as a generation control because it is genuinely consumed by the SurfaceStroke generator and affects per-stroke strength and slight projected-profile amplitude.
4. Removed Stroke Intensity from final albedo-opacity authority. Final coverage composition now uses `_GroundPaintedAccentInkOpacity`.
5. Kept `_GroundPaintedAccentLineStrength` bound as a legacy compatibility property, but the Ground shader no longer consumes it for Painted Accent visibility.
6. Made Ink Colour and Ink Opacity Material-only. Neither participates in SurfaceStrokes, ProjectedGlyphs, or Coverage signatures.
7. Removed dead generic Painted Accent Scale and Contrast from the SurfaceStroke signature and stopped drawing Scale, Contrast, and generic Direction for Painted Accent recipes. Their serialized compatibility values remain untouched.
8. Added a read-only **Visibility and Binding Status** block to `GeneratedGround` reporting:
   - coverage resolution and generated/enabled state;
   - renderer property-block binding status;
   - local mapping versus current mesh bounds;
   - coverage texel world size;
   - authored width in metres and texels;
   - Ink Opacity;
   - estimated maximum palette contrast after opacity.
9. Binding diagnostics compare the actual renderer `MaterialPropertyBlock` against the Ground's current:
   - coverage texture;
   - coverage-enabled flag;
   - origin/size;
   - world-to-local matrix;
   - ink colour;
   - ink opacity.
10. Added debug mode **Ground Painted Accent Raw Coverage Binding**. It displays raw sampled coverage in unmistakable magenta over a dark background and bypasses:
    - contract mask;
    - Ink Opacity;
    - Ink Colour;
    - normal Ground lighting composition.
11. Renamed the previous mode 28 Inspector label to **Ground Painted Accent Contract Coverage** so its contract-mask multiplication is explicit.
12. Added actionable warnings for:
    - no runtime-applicable recipe;
    - missing or disabled coverage;
    - stale MaterialPropertyBlock binding;
    - mapping mismatch;
    - authored width below one coverage texel;
    - low estimated final palette contrast.

### GI-A3 compatibility and visual consequence

The active Snowfield / Clean recipe previously used `Strength = 0.08` as both generated Stroke Intensity and final blend opacity. GI-A3 deliberately preserves the generated intensity at `0.08` but resolves existing Ink Opacity to `1.00`.

Therefore:

```text
Generation authority: unchanged
Maximum final opacity: 0.08 → 1.00
Ink colour: unchanged
Coverage texture: unchanged for unchanged authoring values
```

This is an intentional visible correction, not procedural-generation parity. Deterministic SurfaceStroke, ProjectedGlyph, cluster, and coverage counters should remain unchanged for unchanged inputs.

### GI-A3 shader impact audit

Changed shader files belong to the Ground PixelSurface path. `PixelSurfaceGroundMaterialProperties.hlsl` is included only by `SH_PixelGroundSurfaceLit.shader`; Painted Accent coverage helpers are guarded by `PS3D_PIXELSURFACEGROUND_MATERIAL_PROPERTIES`.

Expected impact:

- Ground Painted Accent final colour and debug modes: changed deliberately;
- Ground non-Painted-Accent material behavior: unchanged;
- River shaders: unchanged;
- Generated Mass shaders: unchanged;
- generic PixelSurface shaders without Ground material properties: unchanged.

### GI-A3.1 warning cleanup

After Unity compilation exposed warnings, the follow-up cleanup:

- removed the three retired, unread compatibility backing fields from `GroundSurfaceFeatureRecipe`;
- replaced all new `FindObjectsByType<T>(FindObjectsInactive, FindObjectsSortMode)` calls in `GeneratedGroundEditor` with Unity 6.5's non-sorting `FindObjectsByType<T>(FindObjectsInactive)` overload;
- preserved the same inclusion of inactive loaded Grounds and did not introduce ordering dependence;
- did not modify or reserialize any scene, prefab, material, profile, or style asset.

### GI-A3 validation boundary

Source inspection and static validation can prove the new property contract, signatures, and shader source path. Only Unity can finally prove:

- shader compilation on the target URP version;
- the renderer's live property block reports Current;
- raw mode 29 shows the expected coverage positions;
- contract mode 28 shows the coverage after vertex-contract masking;
- normal lit Scene and Game views show clearly discernible lines;
- Ink Colour and Ink Opacity execute Material-only regeneration;
- deterministic generation counters remain unchanged.

## GI-A4 implementation record

GI-A4 is a diagnostics-presentation patch. It changes `GeneratedGround` timing retention, the `GeneratedGround` Inspector report layout, and this canonical document. It does not alter Ground generation, Painted Accent placement, ProjectedGlyph composition, coverage rasterization, material rendering, scenes, prefabs, materials, profiles, or style assets.

Implemented:

1. The latest regeneration report now contains only stages actually executed by that pass. A Material-only refresh no longer displays retained SurfaceStrokes children beneath a `0.00 ms` parent.
2. Detailed Painted Accent timing is retained in three explicit historical records:
   - Last Completed SurfaceStrokes Timing;
   - Last Completed ProjectedGlyphs Timing;
   - Last Completed Coverage Timing.
3. Historical records update only when the corresponding stage actually completes and survive later Material-only or cache-hit passes.
4. Painted Accent placement, projected-baseline, and coverage-statistics reports remain available beside their matching timing records.
5. Reading or copying the Accepted Projected Baseline report is now observational. It no longer calls the generators or silently performs expensive work.
6. Scene overlays are separated from reports under **Painted Accent Scene Debug** and **Painted Accent Reports**.
7. Regeneration Accounting now contains accounting only. Current timing and Painted Accent historical timing are no longer duplicated inside it.
8. Duplicate clipboard actions were removed. The canonical actions are:
   - one copy action beside each report;
   - one **Copy All Painted Accent Reports** action;
   - one **Copy All Ground Diagnostics** action.
9. Surface-mask diagnostics now have their own adjacent copy action.
10. PA-P1 through PA-P4 timing and workload telemetry is preserved in the retained ProjectedGlyphs timing and projected-baseline reports.

### GI-A4 timing semantics

| Report | Meaning |
|---|---|
| Current Regeneration Timing | Only the most recent pass and only stages it executed |
| Last Completed SurfaceStrokes Timing | Most recent pass that actually rebuilt SurfaceStrokes |
| Last Completed ProjectedGlyphs Timing | Most recent pass that actually rebuilt ProjectedGlyphs |
| Last Completed Coverage Timing | Most recent pass that actually rasterized/uploaded coverage |
| Last Placement Result | Retained output statistics from the latest generated placement |
| Accepted Projected Baseline | Retained accepted/rejected/quota/workload statistics; read-only |
| Last Coverage Raster | Retained texture/texel/width statistics |
| Editor Regeneration Accounting | Editor request/pass batching and stage counts only |

This separation is intentional: current-pass timing must never visually imply that retained historical substage work ran again.

## Remaining phases

### GI-A3 — Painted Accent visibility contract

Implemented in the current patch. Unity validation remains required before acceptance.

### GI-A4 — Diagnostics cleanup

Implemented in the current patch. Unity validation remains required before acceptance.

### PA-B1 — One-button persistent output

Implemented. Each Ground owns an automatically managed persistent R8 coverage asset through a scene-GUID and Ground-ID scoped generated-output contract.

The author sees only:

```text
Bake status: Missing / Current / Stale / Incompatible
[Bake Painted Accents]
```

There is no asset reference, save dialog, manual folder creation, texture assignment, recipe Inspector, or material editing.

### PA-B2 — Baked-only runtime

At runtime:

- no SurfaceStroke generation;
- no ProjectedGlyph generation;
- no cluster composition;
- no coverage rasterization;
- no CPU coverage upload;
- no silent procedural fallback.

### PA-B3 — Stale detection and build enforcement

- exact persistent-output signature;
- generated-resource compatibility/version check;
- build validation failure for missing or stale output;
- actionable repair message.

## Validation requirements

For every later code patch:

1. Run an actual available parser/compiler validation over every changed C# file; do not claim Unity compilation unless Unity ran.
2. Scan changed C# files for malformed multiline strings, duplicate signatures/locals, missing helpers, and call/arity mismatches.
3. Preserve original line endings and run `git diff --check`.
4. Validate Undo/Redo, shared asset dirtiness, scene dirtiness, and multi-object behavior.
5. Validate minimum regeneration scope for every edited control.
6. Confirm no scene or prefab is modified or included.

## Methods tried ledger

### Accepted

- mesh-free projected Painted Accent glyphs;
- authoritative pair/triplet cluster quotas;
- PA-P1 conservative swept-width broad phase;
- PA-P2 near-parallel broad phase and segment metadata;
- PA-P3 cheap-before-geometry pruning;
- PA-P4 deterministic external conflict index and incremental reconciliation;
- transient 2048 × 2048 R8 coverage as the current preview/render input;
- `GeneratedGround` as the unified authoring façade;
- runtime-authoritative feature resolution in the Inspector;
- unified inline authoring of resolved shared profile, shared/local material, and supported shader-feature values;
- explicit owner-aware refresh of loaded consumers after shared asset edits;
- safe single-object gating for ambiguous shared-asset editing;
- explicit compatibility initialization rather than passive Inspector mutation;
- dedicated material-only Painted Accent Ink Opacity with non-mutating `1.00` compatibility fallback;
- Stroke Intensity retained as genuine generation authority rather than mislabeled visibility;
- raw-coverage binding debug mode separated from contract-masked coverage debug;
- direct renderer property-block and local-mapping diagnostics;
- dead Painted Accent Scale/Contrast invalidation removed; retired unread compatibility backing fields removed from code while existing asset YAML is left untouched;
- current-pass timing separated from retained completed-stage telemetry;
- diagnostic report reads made observational rather than hidden generation triggers;
- one compact canonical Ground diagnostics hierarchy and clipboard path;
- one-button, zero-setup production output requirement;
- persistent scene-GUID/Ground-ID-scoped R8 coverage output;
- exact coverage-byte and local-mapping stale detection.

### Rejected or retired

- 3D raised Painted Accent ridge as production output;
- source-space angle-heavy composition and large rotations used to fake stepping;
- runtime procedural generation as the final production architecture;
- manual production asset references or drag-and-drop;
- treating nonzero coverage telemetry as proof of visible lines;
- arbitrary Inspector grouping by source-file ownership;
- passive serialized migration during Inspector drawing.

### Deferred opportunities — not blockers

- A future Ground-output project may justify packing additional generated control channels beside the accepted R8 Painted Accent coverage. That is a separate optimization and format-design decision, not unfinished Painted Accent work.
- A future full-style local override model may be useful. The accepted current contract intentionally keeps style, variant, profile, and feature ownership shared while presenting them through the unified GeneratedGround authoring façade.

## Next work items

1. **None required for this workstream.** The GeneratedGround Inspector and Painted Accent production architecture are closed and accepted.
2. Use **Audit and Clean Painted Accent Assets...** after deleting Grounds/scenes or releasing production bakes so obsolete generated outputs do not accumulate.
3. Treat future channel packing, full-style local overrides, or broader Ground/River performance work as separately scoped projects with new evidence and approval.

## GSU-M1.9A.1 — Fine Gravel Packed-Source A/B Evaluation — visually rejected; historical

**Status:** Rejected by Unity evidence; no longer actionable.

This temporary test installed `Fine Gravel A - Direct Normal` and `Fine Gravel B - Strong Form`, both produced from image-generated normal-style candidates. Neither candidate had genuinely periodic edge neighbourhoods, and their RGB fields did not constitute coherent packed slope data. Unity exposed visible repeat bands, malformed relief, flattening, and generally inadequate stone form. Do not validate, tune, or promote either A1 payload. GSU-M1.9A.3 overwrites those temporary payloads while retaining their serialized GUID/stable-ID plumbing only for safe migration.
---

# PA-B1.1 compiler correction

The first PA-B1 package contained one C# scope error in `GeneratedGroundEditor.DrawPaintedAccentStrokeControls`:

```text
GeneratedGroundEditor.cs(1629,59): CS0103
The name 'generatedGround' does not exist in the current context
```

The Preview and Production subsection call referenced a pattern variable that existed only inside earlier conditional blocks. PA-B1.1 resolves the selected single `GeneratedGround` explicitly at method scope and draws Preview and Production only while the parent Painted Accents foldout is expanded. No runtime, bake, signature, texture-ownership, scene, prefab, material, shader, or style-data behavior changes.

Validation policy update: Tree-sitter parsing is syntax validation only and cannot prove local-symbol resolution. Future editor patches must supplement it with targeted symbol/scope checks for newly introduced local identifiers, while Unity compilation remains the authoritative compile gate.

# PA-B1 implementation — persistent Painted Accent production output

## Status

Implemented after GI-A4 validation.

PA-B1 added the persistent production artifact and one-button authoring workflow. PA-B2 subsequently made that persistent output authoritative in Play Mode and Player.

## Production asset contract

Each `GeneratedGround` stores hidden ownership metadata for one automatically managed Painted Accent production texture:

```text
Bake identifier
Persistent R8 texture reference
Exact coverage-output signature
Bake-format revision
Coverage origin/size
Covered-texel diagnostics
```

The author never edits those fields directly.

The editor creates or updates the asset at:

```text
Assets/Game/Generated/Ground/PaintedAccents/
<scene-guid>/
GG_PaintedAccentCoverage_<ground-id>.asset
```

The scene GUID prevents a copied or renamed scene from overwriting another scene's output. The Ground identifier prevents two Grounds in the same scene from sharing one output. A duplicated Ground receives a new identifier on its next bake. If a copied scene inherits a reference to the original scene's texture, the Inspector reports the output as ownership-incompatible until rebaked into the copied scene's folder.

An unsaved scene cannot be baked because it has no stable scene GUID.

## One-button workflow

The `GeneratedGround` Inspector exposes:

```text
Painted Accents
└── Preview and Production
    Live Preview: Current / Stale / Missing
    Renderer Source: Live procedural preview (PA-B1)
    Production Bake: Missing / Current / Stale / Incompatible
    [Bake Painted Accents]
```

The button:

1. validates that the target is an Edit Mode instance in a saved scene;
2. refreshes modifier and River discovery;
3. executes the authoritative Ground/Painted Accent regeneration path;
4. validates readable R8 live coverage;
5. creates or updates the persistent texture automatically;
6. records the exact coverage-output signature and mapping metadata;
7. marks the Ground and generated texture dirty and saves only the generated asset.

There is no save dialog, asset-reference field, material edit, or external recipe step. Unity will mark the scene instance dirty through Undo/serialization; the patch itself does not modify or include a scene or prefab.

## Exact stale-detection signature

PA-B1 hashes the actual authoritative live output rather than reconstructing a second parallel list of procedural inputs.

The SHA-256 signature covers:

```text
Bake-format revision
Coverage-baker revision
Coverage texture width and height
Coverage origin and local-XZ size
Every R8 coverage texel
```

This is stable across Editor restarts and independent of object-discovery ordering. Any procedural, geometry, modifier, River, placement, clustering, profile, or raster change that alters production coverage or mapping changes the signature after regeneration.

The following intentionally do **not** invalidate the bake because they do not alter coverage:

```text
Ink Colour
Ink Opacity
Ground palette and lighting response
Debug view
Diagnostic foldouts or reports
World transform changes that preserve the same local coverage mapping
```

If a procedural source setting changes while live regeneration is disabled, the existing live-preview signature reports Stale before baking. The Bake action first performs authoritative regeneration and then hashes the resulting coverage.

## Runtime boundary

PA-B1 originally retained procedural rendering while the persistent artifact was validated. The final accepted contract is now:

```text
Edit Mode: live procedural preview
Play Mode and Player: persistent production coverage only
Build: exact PA-B3 production validation required
```

## Methods-tried ledger update

### Accepted

- Native Unity `.asset` Texture2D output in R8 format.
- Scene-GUID-scoped generated-output folders.
- Stable generated-output identifier stored on `GeneratedGround`.
- Exact SHA-256 signature of coverage bytes and local mapping.
- Same-scene duplicate-identifier detection before writing an asset.
- Copied-scene ownership mismatch detection.
- Explicit one-button bake; no automatic build-time mutation.
- Targeted generated-asset save rather than a project-wide save.

### Rejected

- Input-list hashing that could become unstable through discovery order or miss future implementation changes.
- Using transient cache signatures containing generation revisions as persistent stale detection.
- Naming output only from the GameObject name.
- A project-global output path with no scene ownership namespace.
- Exposing a texture reference or path selector to the author.
- Switching runtime to baked-only behavior before persistent output is validated.

## Next work items

1. Validate asset creation, update, same-scene duplication safety, copied-scene ownership detection, and status persistence across an Editor restart.
2. Confirm Ink Colour and Ink Opacity remain Material-only and do not stale the production bake.
3. Implement PA-B2 baked-only runtime after PA-B1 is confirmed.
4. Implement PA-B3 build validation after baked-only runtime is confirmed.

---

# PA-B2 implementation — baked-only Play Mode and Player rendering

## Status

Implemented after PA-B1.1 persistent-output validation.

PA-B2 changes only the Painted Accent renderer source and execution boundary. Edit Mode retains the authoritative procedural preview and one-button bake. Play Mode and Player builds bind the serialized persistent R8 production texture and do not execute the procedural Painted Accent pipeline.

## Execution contract

```text
Edit Mode
→ SurfaceStrokes
→ ProjectedGlyphs and companion solving
→ coverage raster and upload
→ live procedural coverage bound to the Ground renderer

Play Mode and Player
→ validate persistent production texture structurally
→ bind persistent R8 coverage and stored local mapping
→ render
```

Play Mode and Player skip:

```text
Painted Accent SurfaceStroke generation
ProjectedGlyph generation
pair/triplet cluster solving
Painted Accent river-exclusion spline snapshot construction
coverage rasterization
procedural coverage texture creation or upload
procedural debug snapshot generation
```

Ground geometry, collider generation, surface masks, ordinary material features, modifier snapshots required by Ground geometry, and River corridor integration remain unchanged.

## Runtime source validation

The active Painted Accent recipe requires production coverage when it resolves as an enabled Shader Only feature with Stroke Intensity above zero.

Runtime accepts the stored output only when:

```text
persistent texture reference exists
stored coverage signature is non-empty
bake-format revision matches
texture format is R8
texture dimensions are positive
stored local-XZ mapping has positive size
```

A valid output binds the persistent texture with its stored origin/size and the Ground's current world-to-local matrix.

If the feature is disabled or has no runtime-applicable recipe, production output is Not Required and coverage is disabled without an error.

If required output is missing or incompatible, the renderer binds neutral black coverage, disables Painted Accents, and emits one compact error per distinct failure reason:

```text
No procedural runtime fallback was executed.
```

This prevents hidden startup computation and makes invalid production data visible immediately.

## Stale-data boundary

PA-B2 validates the serialized artifact structurally at runtime and deliberately does not regenerate live coverage merely to recompute staleness. PA-B3 supplies the accepted exact Missing/Stale/Incompatible enforcement before every Player build.

PA-B3 now enforces exact current production output before a Player build. Ink Colour and Ink Opacity remain Material-only and continue to use the current persistent coverage without rebaking.

## Inspector behavior

Preview and Production now reports:

```text
Edit Mode:
  Live Preview: Current / Stale / Missing
  Renderer Source: Live procedural preview (Edit Mode)

Play Mode:
  Edit Preview: Suspended during Play Mode
  Renderer Source: Persistent production coverage (PA-B2)
  Runtime Coverage: Current / Not Required / Missing / Incompatible
  Production Artifact: Available (structural validation) / Missing / Incompatible
```

The one-button bake remains disabled in Play Mode.

## Methods-tried ledger update

### Accepted

- Compile-time/runtime source split: Edit Mode live preview, Play/Player persistent output.
- Persistent texture sampled directly; no runtime copy or CPU upload.
- Runtime structural validation with a neutral failure state.
- One compact error per distinct production failure.
- No procedural fallback for missing or incompatible output.
- Painted Accent-only River exclusion snapshots omitted in production mode.

### Rejected

- Runtime regeneration as a safety fallback.
- Copying persistent coverage into a transient runtime Texture2D.
- Treating an invalid bake as enabled coverage.
- Recomputing full live coverage in Play Mode merely to determine staleness.

## Next work items

1. Validate that entering Play Mode performs no SurfaceStrokes, ProjectedGlyphs, Coverage raster, or Coverage upload stages.
2. Confirm the renderer source is persistent production coverage and the visible result matches Edit Mode.
3. Remove or invalidate the production texture temporarily and confirm one clear error, neutral coverage, and no procedural fallback.
4. Implement PA-B3 build enforcement for Missing, Stale, Incompatible, duplicate-identifier, and ownership-mismatch output.

---

# PA-B2.1 correction — persistent texture main-object naming

## Status

Implemented after PA-B2 validation exposed a Unity asset-save warning during a repeat bake.

## Proven defect

The generated asset filename used the complete stable Ground identifier:

```text
GG_PaintedAccentCoverage_<32-character-ground-id>.asset
```

but the `Texture2D.name` stored inside that asset used only the first eight identifier characters. Unity requires the main object's name to match the asset filename stem when saving a native `.asset`, and emitted:

```text
Main Object Name does not match filename
```

This did not indicate a coverage-generation or PA-B2 runtime-source failure. It was a persistent-asset naming inconsistency in `GroundPaintedAccentProductionBaker`.

## Accepted correction

A single `BuildAssetName(identifier)` helper is now authoritative for both:

```text
asset filename stem
Texture2D main-object name
```

The full 32-character identifier is retained in both locations. A repeat bake automatically renames an already-created truncated main object before saving it, so no manual deletion or reassignment is required. Asset path, ownership identifier, coverage bytes, mapping, signature, and runtime binding remain unchanged.

## PA-B2 diagnostics interpretation

In Play Mode, these records are expected to be empty after a domain reload:

```text
Last Completed SurfaceStrokes
Placement Report
Last Completed ProjectedGlyphs
Projected Baseline
Last Completed Coverage
Coverage Report
```

PA-B2 deliberately skips those procedural Painted Accent stages. The authoritative validation evidence is:

```text
Renderer Source: Persistent production coverage (PA-B2)
Runtime Coverage: Current
SurfaceStrokes stage count: 0
ProjectedGlyphs stage count: 0
Coverage stage count: 0
```

Ground geometry, collider, material, snapshots, and River-corridor stages may still execute at Play startup; PA-B2 only removes the procedural Painted Accent pipeline.

## Next work items

1. Confirm a repeat bake produces no main-object-name warning and leaves Production Bake Current.
2. Confirm Play Mode continues to report persistent production coverage with all three procedural Painted Accent stages at zero.
3. Proceed to PA-B3 build enforcement only after this correction is validated.

---

# PA-B3 implementation — production-bake build enforcement

## Status

Unity-validated and accepted after PA-B2.1 persistent naming and baked-only runtime validation.

PA-B3 closes the production-safety gap: an enabled Player build can no longer proceed while a build-scene `GeneratedGround` requires Painted Accents but lacks a current, compatible, uniquely owned persistent output.

## Shared validation contract

The selected-Ground preflight and build preprocessor use the same validator. For every Ground whose selected variant resolves an enabled Shader Only Painted Accent recipe with nonzero Stroke Intensity, validation proves:

```text
saved scene with a stable AssetDatabase GUID
valid 32-character Ground production identifier
no same-scene duplicate identifier
persistent texture reference exists and resolves as an AssetDatabase main asset
asset path matches the scene-GUID and Ground-ID ownership contract
no second Ground references the same production asset
bake-format revision matches
texture is readable R8 with positive dimensions
stored local-XZ mapping is valid
Texture2D main-object name matches the asset filename
stored signature matches the persistent texture bytes and mapping
fresh authoritative Edit Mode coverage matches the stored production signature
```

Ink Colour and Ink Opacity remain excluded because they do not change coverage.

A Ground with no runtime-applicable Painted Accent recipe reports `Not Required` and does not block the build.

## Isolated build-scene validation

The validator reads the active Unity 6.5 Build Profile scene list through `BuildProfile.GetScenesForBuild`; when no active profile exists it falls back to `EditorBuildSettings.scenes`. Each enabled scene is opened through `EditorSceneManager.OpenPreviewScene`, validated in isolation, and closed through `ClosePreviewScene` in a `finally` block.

The validator does not:

```text
save a scene
save a prefab
modify a production asset
rebake automatically
reuse only the currently open scene
silently disable invalid output
```

Authoritative procedural coverage may be regenerated inside the temporary preview scene solely to compute exact staleness. The temporary scene is discarded without saving.

## Build enforcement

`GroundPaintedAccentBuildPreprocessor` implements `IPreprocessBuildWithReport` and runs before the Player build. Any invalid required output throws one grouped `BuildFailedException` containing scene, Ground hierarchy path, status, reason, asset path when available, and the corrective action.

Blocking categories are:

```text
Missing
Stale
Incompatible
Ownership Mismatch
Duplicate Identifier
Shared Production Asset
Validation Failed
Scene Unavailable
```

There is no warning-only path and no automatic repair during build.

## Inspector preflight

Two explicit actions are available:

```text
Painted Accents > Preview and Production
  Validate Production Bake

Debug and Diagnostics
  Validate Painted Accent Production in Build Scenes
```

The selected-Ground action refreshes authoritative live coverage and compares it to the stored output without writing an asset. The build-scenes action executes the exact project-wide contract used by the build preprocessor.

## Methods-tried ledger update

### Accepted

- One shared validator for manual preflight and build blocking.
- Exact signature validation of both persistent bytes and current authoritative coverage.
- Isolated preview-scene loading for build-scene inspection.
- Same-scene duplicate identifier and texture-sharing checks.
- Cross-build-scene production-asset conflict detection.
- One grouped build failure rather than per-Ground Console flooding.
- Explicit validation only; no build-time mutation or rebake.

### Rejected

- Validating only loaded scenes or Inspector targets.
- Trusting the serialized signature without hashing the persistent asset.
- Structural-only build validation.
- Runtime stale detection through procedural regeneration.
- Automatic build-time rebake or scene save.
- Warning-only builds with invalid production coverage.

## Next work items

1. Compile and validate PA-B3 in Unity 6000.5.0f1.
2. Test Current, Stale, Missing, Incompatible, duplicated Ground, copied scene, shared asset, disabled feature, and multiple-failure build cases.
3. Confirm validation leaves scenes and prefabs unsaved and unmodified.
4. Perform the final Ground/Painted Accent architecture and documentation audit after PA-B3 acceptance.


---

# PA-B4 implementation — generated-asset audit and conservative cleanup

## Status

Unity-validated and accepted after PA-B3 production validation and the generated-output lifecycle gap were confirmed. PA-B4.1 supplied the final missing `System` namespace import with no behavioral change.

PA-B4 provides an explicit one-run workflow for finding and deleting persistent Painted Accent outputs that no longer have a legitimate project owner or reference. It does not delete assets during bake or build validation.

## Project-wide audit command

The canonical command is:

```text
Tools
└── Generated Ground
    └── Audit and Clean Painted Accent Assets...
```

The same workflow is reachable from `GeneratedGround > Debug and Diagnostics` through:

```text
Audit Generated Painted Accent Assets
Copy Generated Asset Audit
Delete Confirmed Painted Accent Orphans
```

The audit scans:

```text
all imported assets beneath Assets/Game/Generated/Ground/PaintedAccents
all loaded scenes, including unsaved scenes in memory
all saved project scenes, including scenes excluded from the active build profile
all direct project-asset dependencies beneath Assets
```

Saved scenes not already loaded are opened only as isolated preview scenes and are closed without saving.

## Classification contract

Every imported file beneath the managed generated-output root is classified as exactly one of:

```text
Active and referenced
Referenced but no longer required
Ownership mismatch
Shared incorrectly
Confirmed orphan
Unknown / unsafe
```

A confirmed orphan must satisfy all of these conditions:

```text
path is beneath the exact managed generated-output root
path matches the scene-GUID / Ground-ID naming contract
main asset is an R8 Texture2D
no GeneratedGround in any loaded or saved project scene claims it
no project asset directly references it
ownership and dependency scans completed without failure
```

Malformed assets, non-R8 assets, externally referenced outputs, ownership mismatches, and shared outputs are retained and reported. “Not in the active build profile” is never treated as proof that an asset is unused.

## Deletion safety

Deletion is an explicit two-step action inside a dedicated report window:

```text
Run Audit
→ review the complete report and every exact orphan path
→ Delete Confirmed Orphans
→ review the exact deletion set again
→ Confirm Delete
```

Immediately before deletion, the tool runs the full audit again. Deletion proceeds only when the fresh confirmed-orphan set exactly matches the reviewed set.

Deletion is blocked when:

```text
a loaded scene contains unsaved changes
a loaded persistent project asset contains unsaved changes
the audit was cancelled
a scene or dependency scan failed
the confirmed-orphan set changed during the safety re-audit
```

The cleanup command never saves or modifies a scene or prefab. It never deletes an asset outside the exact managed root, and it never deletes an unknown or ambiguous file.

## Per-Ground release workflow

`Painted Accents > Preview and Production` now includes:

```text
Release Production Bake
```

This is an explicit Undo-recorded scene edit that clears:

```text
production texture reference
production identifier
stored coverage signature
bake-format revision
stored local-XZ mapping
covered-texel diagnostics
runtime production status
```

The generated texture itself is intentionally left untouched. The user saves the scene manually, then runs the project-wide audit. Once no saved project reference remains, the texture becomes a confirmed orphan and can be deleted safely.

## Methods-tried ledger update

### Accepted

- One shared audit implementation for the Tools menu and Inspector actions.
- All-project-scene ownership scan, not build-scene-only cleanup.
- Direct AssetDatabase dependency scan for arbitrary scene, prefab, and asset references.
- Dry-run report with exact paths before deletion.
- Fresh full re-audit immediately before deletion.
- Hard deletion block for unsaved loaded scenes or persistent assets.
- Explicit per-Ground release followed by manual scene save and later orphan cleanup.
- Unknown and malformed generated-root contents are reported and retained.

### Rejected

- Automatic deletion during rebake.
- Automatic deletion during Player build validation.
- Treating exclusion from Build Settings as proof of disuse.
- Deleting by filename or age alone.
- Opening and saving scenes automatically to clear references.
- Deleting an asset still referenced by a Ground whose Painted Accents are disabled.

## Next work items

1. Compile and run the PA-B4 audit in Unity 6000.5.0f1.
2. Validate deleted Ground, deleted scene, disabled feature, released bake, duplicate Ground, copied scene, non-build scene, external reference, and malformed generated-root cases.
3. Confirm the cleanup tool never dirties or saves a scene merely by auditing.
4. After PA-B4 acceptance, perform the final Ground/Painted Accent architecture and documentation closure audit.


# PA-B4.1 compile hotfix — missing System namespace

Unity compilation exposed one unresolved framework symbol in `GroundPaintedAccentGeneratedAssetAudit.cs`:

```text
CS0103: The name 'StringComparer' does not exist in the current context
```

The audit report sorts confirmed orphan paths with `StringComparer.OrdinalIgnoreCase`, but the file did not import `System`. PA-B4.1 adds the missing `using System;` directive. No audit classification, deletion, scene handling, generated-asset ownership, or runtime behavior changed.

Validation for this hotfix also scans the complete PA-B4 editor file set for unqualified `System` framework symbols whose source file lacks the required namespace import. Unity compilation remains the authoritative semantic validation gate.

# V3S-A4B.2 Inspector extension — implemented and source-audited

The two accepted region foldouts remain authoritative. A4B.2 adds only these nested groups:

```text
River-Coupled Ground Response — River Bank
└── Shore Wetness
    └── Wet Highlight Shaping
        ├── Wet Highlight Strength
        ├── Wet Highlight Tightness
        ├── Camera-Centred Bias
        └── Vertical Falloff

River-Coupled Ground Response — Riverbed
└── Wetness
    ├── Wetness Transition
    │   ├── Riverbed-to-Bank Blend Distance
    │   └── Riverbed-to-Bank Blend Softness
    └── Submerged Finish
```

The modifier selectors, delayed Create/Duplicate workflow, local/shared ownership, substrate controls, cover controls, and all unrelated Ground groups remain unchanged. No top-level foldout or debug control is added. The Riverbed transition controls disable while local Riverbed hydrology is unavailable; a zero distance preserves exact-support behavior.

Implementation verification confirms all six controls are bound in both local-override and shared-style paths, preserve the two accepted region foldouts, and retain the delayed asset-creation workflow. Subsequent Unity validation accepted the retained highlight-shaping controls, while A4B.3 superseded the broad-band/outward-transition interpretation with the frozen thin waterline band and inward Riverbed transition.


# Inspector and Painted Accent workstream closure — 2026-07-15

The GeneratedGround Inspector and Painted Accent production workstream is accepted. No known correctness, authoring, runtime-generation, build-safety, or generated-asset-lifecycle blocker remains in that scope. This is not a closure of GeneratedGround or the Ground visual roadmap. River-coupled placement and hydrology are Unity-accepted and frozen through A4B.3: the two region-oriented `River Bank` and `Riverbed` groups, delayed Surface Layer/Hydrology asset workflows, normalized substrate ownership, cover response, hydrology, submerged finish, thin waterline highlight band, and inward Riverbed transition remain authoritative. GSU-M1 adds no new top-level River group. Each existing `GroundSurfaceLayerProfile` selector exposes an optional generic `StylizedSurfaceMaterialProfile` reference and Ground-only cover compatibility. GSU-M1.7 adds a warned `Shared Material Definition` foldout inside that existing editor and a `This River Application` group of neutral Bank/Riverbed multipliers. Generic material palette, packed detail, cavity bias, natural scale, and dry finish remain owned by the reusable asset; application multipliers do not move ownership into River. GSU-M1.8 returns the default runtime detail tier to 256² without changing this UI; GSU-M1.9A.5 replaces only the two rejected temporary Fine Gravel texel payloads and changes no Inspector contract. The optional material Preview pane is diagnostic only. V4 Contact / Edge Accents remains queued after reusable-material expansion and family acceptance and excludes River sources. Legacy hydrological fields remain removed from active Surface Layer authoring, and the obsolete Shore Damp control remains hidden.

The accepted maintenance rules are:

- author and preview through `GeneratedGround`;
- rebake after any change that alters generated coverage or local mapping;
- do not rebake for Ink Colour or Ink Opacity changes;
- allow PA-B3 to block invalid Player builds rather than bypassing validation;
- release obsolete per-Ground production ownership explicitly, save the scene manually, then run the PA-B4 audit;
- delete only assets classified as **Confirmed orphan** by the fresh reviewed audit;
- do not manually delete, rename, relocate, or reassign generated production assets as an ordinary workflow.

Any later change to the texture format, packing contract, authoring ownership model, or generation algorithm must increment the relevant revision/signature contract and be treated as a new separately validated patch series.

## GSU-M1.9A.3 — Source-Preserved Integrable Stone Form — visually rejected; historical

**Status:** Superseded by GSU-M1.9A.4 after Unity exposed macro size segregation and insufficient contour definition.

GSU-M1.9A.1 is visually rejected. Its image-generated candidates were neither genuinely seamless nor valid coherent packed slope fields. GSU-M1.9A.2 remained an offline deterministic investigation only and proved periodic conversion, but its distance-cap reconstruction concentrated useful slope near stone rims and left large interiors too uniform. GSU-M1.9A.3 replaced the two temporary A/B texel payloads while deliberately retaining their existing GUIDs, library stable IDs, importer settings, and Ground-layer references so an installed A1 evaluation is upgraded without orphaning serialized selections. The legacy temporary filenames and stable IDs are cleanup debt only; they must be deleted when a winner replaces canonical `fine-gravel`.

The two historical A3 candidates were rebuilt deterministically from the user-supplied rounded-stone source rather than generated as final textures:

- **`Fine Gravel A3 - Source Preserved`** keeps restrained relief while preserving source-derived silhouettes, size distribution, neutralized internal stone-body cues, localized crowns, irregular shoulders, and hierarchical crevices.
- **`Fine Gravel A3 - Vertical Form`** uses the identical periodic stone layout, B cavity, and A variation, but increases coherent body slope and localized crown amplitude. It was the leading A3 candidate for stronger roundness and verticality before Unity rejected the shared layout and contour treatment.

The non-periodic source boundaries are moved to the centre one axis at a time; only stones intersecting each centre repair band are removed and repacked from extracted source silhouettes on a toroidal 1024² authoring canvas. This preserves most of the supplied layout while making opposite edge neighbourhoods continuous. Each stone uses a continuous side profile plus one or two localized crowns, source-body variation with its directional plane removed, and restrained microstructure. Broad whole-stone white plateaus are prohibited. The final 256² R/G channels are derived from one periodic height field, so the slopes are internally coherent and integrable; B contains a soft contact shoulder and narrower deep gap core; A contains non-directional per-stone and internal form variation.

Offline validation includes 3×3 shader-reference tiling, 256/128/64/32 mip tests, numerical wrap-to-adjacent ratios, per-stone height-distribution evidence, and a CPU reference that reproduces the current packed-detail decode, palette, cavity bands, flat-ground normal perturbation, and material values. The reference uses a simplified ambient/diffuse lighting term and is not claimed to reproduce the complete URP pass. Unity production-camera rendering remains authoritative.

Runtime architecture and cost do not change: three temporary 256² RGBA32 mipmapped slices remain during evaluation, only the selected substrate slice is sampled, and there is no new shader sample, ALU branch, draw call, renderer, mesh data, River data, or runtime CPU process. No C#, HLSL, ShaderLab, River source, scene, prefab, canonical Fine Gravel assignment, or unrelated material changes in this patch.

**Unity gate:** rebuild `SSDL_DefaultSurfaceDetails`, historically required comparison of the two A3 choices with identical shared/application values from the same close and production cameras, include dry and wet views, and judge body roundness, internal variation, coherent common light direction, cavity width, repetition, seam visibility, and mip survival. Select a winner or reject both; do not tune the shader to hide a deficient packed source.

## GSU-M1.9A.4 — Balanced Toroidal Mix and Hard Rock Contour

**Status:** Visually superseded by GSU-M1.9A.5 after Unity exposed persistent macro cross bias, excessive micro-fillers, and insufficient authored worn-edge definition.

GSU-M1.9A.3 is visually rejected. Its source-preserved reconstruction improved interior verticality, but Unity evidence exposed two remaining packed-source defects: the repaired source layout segregated large and small stones into repeatable macro regions that formed visible cross/square patterns when tiled, and the stone-to-gap transition remained too gradual, causing individual forms to read as soft dirt mounds rather than hard rocks. A4 replaces only the two temporary A/B packed payloads while retaining their GUIDs, stable IDs, importer settings, Ground adapters, and serialized selections. The temporary legacy filenames remain cleanup debt until one candidate replaces canonical `fine-gravel`.

A4 starts from the coherent periodic A3 vertical height/form data and applies only deterministic, periodic operations. Two independently phase-warped copies of the same source layout are combined without alpha-blended ghosting: the second copy contributes only substantial stone bodies inside genuinely low regions of the first. This breaks the previous tile-axis size bands and interleaves large, medium, and small forms more chaotically while preserving a single coherent height field. Both active candidates use exactly the same redistributed layout, coverage, cavity topology, and non-directional form variation:

- **`Fine Gravel A4 - Balanced Mix`** uses a moderately compressed contact wall and restrained slope amplitude.
- **`Fine Gravel A4 - Hard Rock Contour`** uses a narrower contact wall, stronger edge-normal energy, stronger stone-side cavity shoulder, and stronger neutral edge/body separation. It is the leading candidate for the requested hard-rock delimitation, but no winner is declared before Unity evidence.

The mixed layout covers approximately `58.6%` of the tile. High regions remain localized rather than broad plateaus: Balanced Mix places about `2.43%` of stone pixels above `0.90` height and `9.31%` above `0.75`; Hard Rock Contour places about `2.75%` above `0.90` and `11.38%` above `0.75`. Mean edge-gradient energy is approximately `1.94×` the inner-body gradient for Balanced Mix and `2.37×` for Hard Rock Contour. The final R/G slopes are re-derived from each periodic height field; B remains a hierarchical deep-gap core plus narrow stone-side contact shoulder; A remains lighting-neutral.

Runtime architecture and cost do not change: three temporary 256² RGBA32 mipmapped slices remain during evaluation, only the selected substrate slice is sampled, and there is no new shader sample, ALU branch, draw call, renderer, mesh data, River data, or runtime CPU process. No C#, HLSL, ShaderLab, River source, scene, prefab, canonical Fine Gravel assignment, or unrelated material changes in this patch.

**Unity gate:** rebuild `SSDL_DefaultSurfaceDetails`, compare the two A4 choices with identical shared/application values from the same close and production cameras, include dry and wet views, and judge local size mixing, absence of the prior cross/square macro pattern, hard contour readability, internal form, cavity width, repetition, seam visibility, and mip survival. Select a winner or reject both; do not tune the shader to conceal a deficient packed source.


## GSU-M1.9A.5 — Source-Art Packed Conversion, Macro Rebalance, and Worn Edge Accent

**Status:** Implemented and source-audited; Unity comparison pending.

### Objective

Replace the visually rejected A4 temporary Fine Gravel payloads with two controlled 256² candidates derived from the user-approved worn-rock source image. Preserve the reusable one-sample packed-detail architecture while correcting the three Unity-observed defects: repeated cross/square macro size segregation, excessive tiny-stone noise at gameplay distance, and insufficient hard-rock edge definition.

### Reviewed evidence

- Unity A4 repeat evidence shows a stable cross-like macro region where small stones concentrate through the tile centre while larger stones dominate surrounding regions.
- Unity close and production-camera evidence shows improved internal verticality but weak stone delimitation; rocks read as soft mounds because the packed source lacks an explicit bright worn-rim signal.
- The approved source image contains a better large/medium/small hierarchy, fewer micro-fillers, dark crevices, hard contours, and visible worn edge highlights. It is a beauty source only and must not be sampled directly as packed material data.
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl` already maps positive A-channel variation toward the material light colour and maps B to contact/deep-cavity bands; therefore A can carry a lighting-neutral worn-rim accent without adding a shader sample or material-name branch.

### Approved files

- the five canonical Ground documents;
- `Assets/Game/Demo/Profiles/SurfaceMaterials/SSDL_DefaultSurfaceDetails.asset`;
- the two existing temporary `SSMP_FineGravel_AB_*` assets;
- the two existing temporary `GSLP_FineGravel_AB_*` assets;
- the two existing temporary packed PNG payloads.

Importer metadata, GUIDs, stable IDs, C#, HLSL, ShaderLab, River source, scenes, prefabs, canonical `fine-gravel`, and unrelated materials are outside scope.

### Implementation sequence

1. Treat the approved image as authoring source only. Extract stone silhouettes, neutralized surface character, crevice structure, and worn-edge cues.
2. Repack extracted source stones on a 1024² toroidal authoring canvas with local size-class balancing. Medium stones dominate; large stones remain distributed; the smallest filler class is capped and used only where necessary.
3. Construct coherent per-stone height with localized crowns, irregular internal form, and compressed contact walls. Derive final R/G slopes only after area-downsampling to 256².
4. Derive B as hierarchical contact shoulder plus deep crevice core. Derive A as neutral body variation plus an explicit narrow positive worn-rim band; no directional sunlight or cast shadow is baked.
5. Produce two candidates from the identical periodic layout: a restrained worn-edge candidate and a stronger worn-edge/contour candidate. Retain the temporary A/B GUID and stable-ID plumbing for safe serialized migration.
6. Validate 3×3 repeat, 256/128/64/32 mip survival, edge-neighbourhood continuity, size-density balance, small-stone share, packed-channel ranges, and a CPU reference matching current packed-detail decode. Package only after those checks pass.

### Invariants and non-goals

- Runtime remains one packed sample per active detailed substrate.
- Runtime resolution remains 256² RGBA32 with the existing mip/import contract.
- No geometry, parallax, displacement, extra draw call, renderer, runtime CPU process, or River contract change.
- The explicit rim is an authored material-form cue in A, not a world-light direction and not a replacement for URP lighting.
- A5 is rejected historical evidence. GSU-M2.0 is the active material gate; canonical Fine Gravel remains unfrozen until the imported authored-colour candidate passes Unity comparison and explicit user acceptance.

### Acceptance criteria

- No visible cross, square, quadrant, or axis-aligned size-density pattern in 3×3 repeats or Unity gameplay views.
- Medium stones dominate and tiny filler stones no longer create distant visual noise.
- Individual stones show a clearly delimited hard contour and restrained bright worn rim.
- Internal stone form and verticality remain at least as strong as the accepted part of A4.
- No broad seam band at full resolution or lower mips.
- No runtime architecture or cost regression.

### Implementation result

Historical A5 replaced the two temporary A4 texel payloads in place and retained their existing GUIDs, stable IDs, importer settings, Ground adapters, and serialized selections. Its choices, **`Fine Gravel A5 - Worn Edge`** and **`Fine Gravel A5 - Strong Rim`**, are visually rejected because the packed-only conversion discarded the authored colour and broad form that made the source attractive. They remain cleanup debt only and are not active validation candidates. GSU-M2.0 supersedes them with **`Fine Gravel — Imported Stone Ground 01`**.

The source is cropped to a low-discontinuity 1024² region, projected to a periodic luminance field, segmented into stone bodies, stripped of broad directional lighting planes per stone, and converted into coherent height, slope, cavity, neutral source character, and worn-rim data. The two candidates share one layout, height field, cavity topology, and source character; only packed slope amplitude, A-channel rim strength, and matching generic profile strengths differ.

The runtime tile contains `109` recognized stones after removal of sub-runtime fragments: `31` large, `41` medium, and `37` small by count. Small stones occupy about `0.94%` of runtime texels, medium stones about `9.78%`, and large stones about `51.10%`; total stone coverage is about `61.82%`. The smallest class therefore remains available as sparse filler without recreating the distant micro-pebble carpet. R/G are derived after final 256² downsampling, B remains the hierarchical crevice/contact signal, and A contains neutralized source texture plus a narrow positive worn-rim cue.

Static 256/128/64/32 packed and shader-reference tests report a worst wrap-to-ordinary-adjacency ratio of approximately `1.29`; no exact Unity seam, mip, lighting, dry/wet, or production-camera acceptance is claimed until the project test. Runtime architecture and nominal cost remain unchanged: three temporary 256² slices during evaluation, one packed sample for the selected substrate, no new shader branch, draw call, geometry, renderer, or runtime CPU process.

**Unity gate:** rebuild `SSDL_DefaultSurfaceDetails`, compare **Worn Edge** and **Strong Rim** with identical shared/application values from the same close and production cameras, include dry and wet views, and judge macro repetition, distant noise, worn-rim readability, internal form, cavity width, and mip stability. Promote neither candidate until explicit visual acceptance.
