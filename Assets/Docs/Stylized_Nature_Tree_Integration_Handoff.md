# Stylized Nature Tree System — Canonical Architecture and Implementation Plan

## TREE-CONTROLS.4H12H2 — Remove Standalone Trunk Ridges

Status: implemented; Unity compilation and runtime validation pending.

### Objective

Remove the unrequested standalone trunk `Ridge Count` and `Ridge Depth` feature. Recipe-only trunks must be circular outside the root/buttress influence. Any persistent ridging is owned exclusively by the root system: `Root Count` determines lobe count, Root Reach/Thickness/Height determine their shape, and Buttress Persistence determines how far the root-derived lobes persist up the trunk.

### Acceptance criteria

- No recipe-only Ridge Count or Ridge Depth controls remain in descriptors, recipe ranges, resolved controls, fingerprints, reports, or response-suite matrices.
- The bark evaluator applies no independent trunk-ridge radial modulation.
- At Buttress Persistence `0.0`, the trunk becomes circular immediately above the protected root envelope, apart from authored taper, centreline shape, twist orientation, and small roughness.
- At Buttress Persistence `1.0`, root-count buttress lobes may persist to the tip.
- Root Count is the sole lobe-count control.
- Existing root geometry, branch structure, axial twist, and topology validation remain intact.

### Approved files

- `TreeControlDescriptorRegistry.cs`
- `TreeRecipeControlRanges.cs`
- `TreeResolvedControls.cs`
- `TreeGenerationParameters.cs`
- `TreeGenerationOverrides.cs`
- `TreeFamilyProfile.cs`
- `TreeGenerator.cs`
- `TreeBarkMeshGenerator.cs`
- `TreeBarkMeshSettings.cs`
- `TreeCuratedRecipeDefinitions.cs`
- `TreeControlResponseSuite.cs`
- `ProceduralTreeInstanceEditor.cs`
- `TreeBarkMeshAssetBuilder.cs`
- `TreeGalleryGenerationCoordinator.cs`
- this canonical document

### Implementation and audit result

- Removed recipe-only Ridge Count and Ridge Depth from descriptors, recipe ranges, resolved controls, curated definitions, fingerprints, reports, response cases, and ownership counts.
- Removed legacy family/override ridge fields and tests.
- Removed all independent trunk-ridge radial modulation from bark generation.
- Trunk radial sampling is now driven by Root Count only.
- Renamed the visible control to Buttress Persistence and corrected its direction: `0.0` is earliest circularization; `1.0` carries root-owned lobes to the tip.
- Removed obsolete ridge-interval gallery validation and telemetry.
- Static source audit found no remaining standalone ridge symbols in the procedural-tree module.

### Implemented source invariants

- The accepted TREE-ROOTS.4B root mass, immediate quadratic foot amplitude, TREE-ROOTS.3 anchored-foot direction, continuous bark roll, and mixed-resolution stitcher are byte-for-byte unchanged from the pre-4C source in the audited method bodies.
- The contact boost cannot exceed the radial segment count already available from the authored production trunk ceiling. For the common five-root and six-root Twisted cases this permits at most 50 and 60 sides respectively; no global radial maximum was increased.
- Direct boost demand becomes zero once `footShapeEnvelope <= 0.25` or root-only amplitude is at/below `0.35`, and ordinary contour-owned radial resolution then resumes.
- The boost is resolution-only: it changes no vertex target position equation, root envelope, root control value, topology threshold, axial render-sample density, branch radial policy, UV rule, material, or runtime component.
- Normal gameplay receives no new per-frame work. The only expected cost is additional generated bark vertices/triangles in a small number of lowest trunk rings on roots with sufficiently strong contact demand.

### Non-goals

- No scene, prefab, material, recipe asset, shader, layer, or tag edits.
- No replacement decorative trunk-ridge feature.
- No topology-threshold weakening.


## Status

**Authoritative current patch:** `TREE-CONTROLS.4H1 — Evidence-Backed Response-Suite Repairs`.

TREE-CONTROLS.4R1H1 preserves the Archive-32 resolved generator, bark, recipe, shader, and validation behavior from TREE-CONTROLS.4R1 while correcting the first Unity 6000.5.0f1 compile failures in two Editor-only files. Unity compilation, the full 42-control response suite, and the rebuilt curated gallery remain required live validation gates.

The accepted live authoring and generation path for curated or manually assigned standalone recipes is:

```text
Standalone Tree Recipe intervals
    -> stable master/slot seed
    -> exact TreeResolvedControls snapshot
    -> ProceduralTreeInstance
    -> recipe-only TreeGenerator path
    -> generated structure and bark
```

The recipe-only path performs **zero behavioral reads** from `TreeFamilyProfile` and `TreeReferenceCalibrationPreset`. The old family/calibration/override path remains temporarily callable only as explicit compatibility evidence while the new gallery path is validated; it is no longer the normal curated-tree workflow. The existing `TreeGenerationLibrary` reference may still be carried by gallery children solely as a persistent generated-mesh subasset container; it is not consulted for recipe identity or structural behavior.

The explicit curated-gallery rebuild converts all twenty procedural comparison slots into recipe spawners. After that operation succeeds, each spawner owns recipe selection and a stable seed, while its `GeneratedTree` child owns the exact editable controls and generated output. Existing scenes are not silently rewritten on import, reload, or `OnValidate`.

`TREE-GEN.2C.3H5` remains rejected. Legacy compatibility generation retains the H5R1-restored H4 root equations. Recipe-only trees use the same accepted H4 body/foot envelopes plus independent Root Reach and Root Thickness semantics; they do not use the rejected H5 envelope.

## Objective

Provide one understandable tree-authoring interface that exists in two representations:

- standalone recipes expose a min-max interval for every creative control;
- every generated or spawned tree exposes the same controls as exact editable values.

Recipes are independent archetypes. A recipe does not inherit behavioral values from a family, calibration asset, palette asset, or another recipe. Names and tags such as `Alder Standard`, `Norway Spruce Drooping`, `Wych Elm Leaning`, or `Tall Dead Snag` are author-defined labels, not code-level parents.

## Acceptance criteria

The recipe-only migration is complete only when all of the following are true:

1. A generated tree does not require a family profile to resolve creative controls.
2. A recipe contains a min-max interval for every exposed creative control.
3. A `ProceduralTreeInstance` contains the same controls as exact editable values.
4. Existing instances are snapshots; later recipe edits do not silently mutate them.
5. Recipes can be created and copied through explicit Inspector buttons.
6. Recipe copies receive a new stable identity while preserving intervals and appearance.
7. Spawners can reference one recipe or a weighted list of recipes.
8. Every feature category is a collapsed-by-default foldout.
9. Every foldout begins with one readonly explanatory field.
10. Every control has one canonical label, tooltip, stable ID, hard domain, and unit contract.
11. Inert, ignored, duplicate, and technical-only controls are not shown as normal creative controls.
12. Control sampling is stable by control ID and does not depend on field order, recipe name, or schema version.
13. Root Reach, Root Thickness, Root Height, and Root Count are independently measurable.
14. The twenty imported specimens remain visual references rather than becoming twenty public recipes; any exact legacy snapshots needed for migration proof remain internal diagnostics until legacy assets are removed.
15. Weather wind, cloud-shadow reception, ordinary shadows, depth, topology gates, repeatability, and deterministic branch IDs remain intact.

## Repository and Unity constraints

- Unity `6000.5.0f1`, URP.
- Generation remains an editor/bake or explicitly approved pre-generation operation. Ordinary gameplay consumes generated or cached assets.
- No per-frame CPU tree geometry generation or vertex deformation.
- Weather remains the wind producer; trees remain Weather consumers.
- Cloud shadows remain URP main-light-cookie driven and are applied exactly once.
- `TREE-CONTROLS.1` through `TREE-CONTROLS.3` did not authorize raw layer, tag, package, scene, prefab, material, shader, or project-setting changes. `TREE-CONTROLS.4` changes only the shared tree bark shader/include source so Dead Branch Chance has a real visible and wind-stiffness response; it does not edit serialized material assets.
- No raw serialized Unity asset edit is authorized by these source patches. Curated recipe assets are created through explicit Unity Editor actions; gallery scene changes occur only through explicit build/spawn actions.
- Existing bark topology and repeatability gates remain strict.
- Existing provisional geometry-budget exception remains unresolved. The recipe-only quality preset does not add another tessellation increase, but the curated gallery now builds bark for all twenty slots and some recipes may generate denser branch hierarchies.

## Imported reference gallery

The imported gallery remains the visual calibration baseline:

- 20 imported FBX references: Common, Pine, Twisted, and Dead, five each;
- 20 procedural comparison slots;
- an explicit operation that maps all twenty stable procedural comparison slots to the curated catalog;
- one incremental, cancellable curated recipe rebuild/report workflow;
- the previous legacy rebuild retained only under Advanced Validation as compatibility evidence.

The words Common, Pine, Twisted, and Dead remain useful as imported-reference categories, tags, search terms, and historical compatibility identifiers. They do not remain behavioral inheritance layers in the target authoring system.

## Current accepted generated-geometry baseline

`TREE-GEN.2C.3H5R1` restores H4 behavior:

```text
Root body envelope: 1 - u^2(3 - 2u)
Root foot envelope: (1 - u)^2
Half-height angular shoulder width: 0.800
Samples per buttress lobe: 10
Common 1 root target:  5 / 0.720 / 0.160 / 1.390
Pine 1 root target:    5 / 0.300 / 0.160 / 1.180
Twisted 1 root target: 5 / 0.880 / 0.220 / 1.520
Dead 1 root target:    6 / 0.840 / 0.200 / 1.480
```

These are legacy-compatibility values expressed through the old Count/Strength/Height/Flare contract. They are not the final root-control semantics.

`TreeGenerator.CurrentGeneratorVersion = 6` and `TreeBarkMeshGenerator.BarkAlgorithmVersion = 19` define the current recipe-only control contract. Legacy compatibility roots retain the accepted H4 equations. Recipe-only Root Thickness uses `0.5` as the exact H4 six-root neutral breadth, lower values narrow, and higher values broaden. Requested width is absolute; emitted support is clamped and reported only when Root Count would otherwise force neighbouring roots to overlap. Root Reach remains the exact crest amplitude, Root Height remains the vertical envelope, and valleys remain true zero boundaries. `TreeBarkMeshTopologyAudit` remains unchanged.

## Target authoring hierarchy

### Standalone recipe

A recipe owns:

- display name;
- description;
- searchable tags;
- immutable stable identity;
- bark material reference;
- bark-tint interval;
- min-max intervals for every structural control;
- optional advanced seed locks only after the legacy seed model is migrated.

A recipe has no behavioral parent. `Min = Max` expresses an exact recipe value.

### Recipe catalog

A `TreeRecipeCatalog` is an index only. It owns a list of available recipe assets for:

- recipe discovery;
- search and tag filtering;
- spawner dropdowns;
- runtime inclusion;
- Create New Recipe and Copy Selected Recipe actions.

The catalog does not contribute values and is not an inheritance layer.

## Initial curated public recipe catalog

### Decision

The twenty imported specimens remain reference evidence. They are not converted into twenty permanent public recipes.

The proposed initial public catalog contains **13 recipes**:

- Alder Standard
- Alder High-Crown
- Alder Windswept
- Norway Spruce Standard
- Norway Spruce High-Crown
- Norway Spruce Tall
- Norway Spruce Drooping
- Wych Elm Upright
- Wych Elm Leaning
- Dead Alder
- Dead Norway Spruce
- Dead Wych Elm
- Tall Dead Snag

The old `Common` label is replaced with **Alder**. The conifer references are named **Norway Spruce**, because their layered conical silhouettes are spruce-like rather than pine-like. The twisted deciduous archetype is named **Wych Elm**, because its broad crown and contorted old trunk are more coherent with an elm archetype than an evergreen yew.

These are stylized archetype names, not promises of botanical simulation.

### Important implementation limit

The current project still does not generate procedural foliage. Structural intervals below can prepare branch placement, exposed lower trunk, asymmetry and damage, but the foliage-specific target notes cannot be validated until foliage generation exists.

Root Reach and Root Thickness are live on the recipe-only path. Root Reach changes ground-level radial projection. Root Thickness requests an absolute angular support and changes the measurable crest/shoulder breadth without changing crest reach; emitted support is clamped only when Root Count would otherwise create overlap. The legacy path continues to interpret Count/Strength/Height/Flare only for compatibility evidence.

Directional recipes use local direction `0°`. Future gameplay spawners may randomize whole-object yaw, preserving correlation between trunk lean and directional branch bias without requiring cross-control random correlation. The gallery retains its authored comparison orientation.

### Reference grouping

| Recipe | Intended reference grouping | Future foliage target |
|---|---|---|
| Alder Standard | Broadleaf references 1–2; reference 3 may also fit. | Dense rounded crown distributed through the upper 65–75% of the branch band. |
| Alder High-Crown | Broadleaf reference 4. | Foliage mostly above 55–65% of tree height; lower accepted branches remain exposed or dead. |
| Alder Windswept | Broadleaf reference 5; reference 3 may partly fit. | Foliage occupancy biased toward the same local direction as Lean Direction and Directional Bias. |
| Norway Spruce Standard | Conifer references 1, 2 and 5. | Continuous conical foliage tiers from low trunk to tip. |
| Norway Spruce High-Crown | Conifer reference 3. | Lower 35–50% of trunk exposed; upper crown remains conical. |
| Norway Spruce Tall | Conifer reference 4. | Narrower, vertically extended conical crown. |
| Norway Spruce Drooping | A deliberate additional archetype emphasizing downward branch launch. | Foliage follows the downward branch silhouette while retaining small tip upturns. |
| Wych Elm Upright | Twisted references 2–5. | Large separated crown masses following forks and upper primary branches. |
| Wych Elm Leaning | Twisted reference 1 and the close leaning specimen. | Crown mass biased in the local lean direction. |
| Dead Alder | The shorter, broad, irregular dead references. | No foliage. |
| Dead Norway Spruce | The tall conifer-like dead references with downward tiers. | No foliage. |
| Dead Wych Elm | The most contorted, forked dead broadleaf reference. | No foliage. |
| Tall Dead Snag | The last tall sparse dead specimens. | No foliage. |

### Alder Standard

**Reference intent:** Broadleaf references 1–2; reference 3 may also fit.

**Foliage target:** Dense rounded crown distributed through the upper 65–75% of the branch band.

#### Overall Form

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Height | 6.5 | 9.5 | m |
| Trunk Base Radius | 0.28 | 0.48 | m |
| Trunk Taper | 0.72 | 0.86 | 0–1 |

#### Trunk Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Amount | 0.08 | 0.22 | 0–1 |
| Lean Amount | 0 | 0.06 | height fraction |
| Lean Direction | 0 | 0 | degrees |

#### Trunk Spiral and Twist

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Path Spiral Radius | 0 | 0.015 | height fraction |
| Signed Path Spiral Turns | 0 | 0.2 | turns |
| Axial Twist | -20 | 20 | degrees |

#### Roots

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Root Count | 5 | 6 | integer |
| Root Reach | 0.5 | 0.72 | local radius |
| Root Thickness | 0.42 | 0.55 | normalized breadth |
| Root Height | 0.13 | 0.19 | height fraction |

#### Primary Branch Placement

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Primary Branch Count | 10 | 16 | integer |
| Branch Start Height | 0.22 | 0.3 | 0–1 |
| Branch End Height | 0.84 | 0.94 | 0–1 |
| Branch Symmetry | 0.45 | 0.72 | 0–1 |

#### Primary Branch Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Length | 0.28 | 0.42 | height ratio |
| Branch Thickness | 0.32 | 0.48 | parent-radius ratio |
| Branch Elevation | 6 | 24 | degrees |
| Branch Curvature | 0.1 | 0.24 | 0–1 |

#### Branch Hierarchy

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Maximum Branch Order | 3 | 3 | integer |
| Secondary Density | 1.6 | 2.5 | children/primary |
| Tertiary Density | 0.6 | 1.4 | children/secondary |
| Child Scale | 0.38 | 0.5 | ratio/order |

#### Damage

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Missing Branch Chance | 0 | 0.04 | 0–1 |
| Dead Branch Chance | 0 | 0.03 | 0–1 |
| Broken Branch Chance | 0 | 0.02 | 0–1 |

#### Appearance

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bark Tint | #FFFFFF | #FFFFFF | sRGB hex |

#### Advanced Trunk Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Frequency | 0.8 | 1.6 | cycles |
| Trunk Drift | 0.02 | 0.08 | 0–1 |
| Trunk Roughness | 0.02 | 0.08 | 0–1 |
| Ridge Count | 4 | 6 | integer |
| Ridge Depth | 0.04 | 0.1 | 0–1 |

#### Advanced Branch Distribution

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Directional Bias | 0 | 0.1 | 0–1 |
| Directional Bias Angle | 0 | 0 | degrees |
| Tier Spacing | 0 | 0 | height fraction |

#### Advanced Primary Branch Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Arch | -0.08 | 0.12 | signed |
| Late Sag | 0.03 | 0.14 | 0–1 |
| Tip Upturn | 0.02 | 0.12 | 0–1 |
| Side Sweep | -0.08 | 0.08 | signed |

#### Advanced Forking

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Fork Chance | 0.02 | 0.12 | 0–1 |

### Alder High-Crown

**Reference intent:** Broadleaf reference 4.

**Foliage target:** Foliage mostly above 55–65% of tree height; lower accepted branches remain exposed or dead.

#### Overall Form

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Height | 8 | 11 | m |
| Trunk Base Radius | 0.28 | 0.48 | m |
| Trunk Taper | 0.76 | 0.9 | 0–1 |

#### Trunk Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Amount | 0.18 | 0.34 | 0–1 |
| Lean Amount | 0.02 | 0.1 | height fraction |
| Lean Direction | 0 | 0 | degrees |

#### Trunk Spiral and Twist

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Path Spiral Radius | 0.01 | 0.04 | height fraction |
| Signed Path Spiral Turns | 0.1 | 0.35 | turns |
| Axial Twist | -20 | 20 | degrees |

#### Roots

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Root Count | 5 | 6 | integer |
| Root Reach | 0.5 | 0.72 | local radius |
| Root Thickness | 0.42 | 0.55 | normalized breadth |
| Root Height | 0.13 | 0.19 | height fraction |

#### Primary Branch Placement

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Primary Branch Count | 7 | 11 | integer |
| Branch Start Height | 0.52 | 0.66 | 0–1 |
| Branch End Height | 0.88 | 0.98 | 0–1 |
| Branch Symmetry | 0.35 | 0.62 | 0–1 |

#### Primary Branch Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Length | 0.32 | 0.48 | height ratio |
| Branch Thickness | 0.3 | 0.46 | parent-radius ratio |
| Branch Elevation | 8 | 28 | degrees |
| Branch Curvature | 0.14 | 0.3 | 0–1 |

#### Branch Hierarchy

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Maximum Branch Order | 3 | 3 | integer |
| Secondary Density | 1.2 | 2 | children/primary |
| Tertiary Density | 0.3 | 0.9 | children/secondary |
| Child Scale | 0.38 | 0.5 | ratio/order |

#### Damage

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Missing Branch Chance | 0.08 | 0.16 | 0–1 |
| Dead Branch Chance | 0.08 | 0.18 | 0–1 |
| Broken Branch Chance | 0.04 | 0.1 | 0–1 |

#### Appearance

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bark Tint | #FFFFFF | #FFFFFF | sRGB hex |

#### Advanced Trunk Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Frequency | 0.8 | 1.6 | cycles |
| Trunk Drift | 0.05 | 0.12 | 0–1 |
| Trunk Roughness | 0.02 | 0.08 | 0–1 |
| Ridge Count | 4 | 6 | integer |
| Ridge Depth | 0.04 | 0.1 | 0–1 |

#### Advanced Branch Distribution

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Directional Bias | 0 | 0.1 | 0–1 |
| Directional Bias Angle | 0 | 0 | degrees |
| Tier Spacing | 0 | 0 | height fraction |

#### Advanced Primary Branch Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Arch | -0.08 | 0.12 | signed |
| Late Sag | 0.02 | 0.1 | 0–1 |
| Tip Upturn | 0.04 | 0.15 | 0–1 |
| Side Sweep | -0.08 | 0.08 | signed |

#### Advanced Forking

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Fork Chance | 0.1 | 0.25 | 0–1 |

### Alder Windswept

**Reference intent:** Broadleaf reference 5; reference 3 may partly fit.

**Foliage target:** Foliage occupancy biased toward the same local direction as Lean Direction and Directional Bias.

#### Overall Form

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Height | 7 | 10 | m |
| Trunk Base Radius | 0.28 | 0.48 | m |
| Trunk Taper | 0.72 | 0.86 | 0–1 |

#### Trunk Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Amount | 0.16 | 0.3 | 0–1 |
| Lean Amount | 0.12 | 0.24 | height fraction |
| Lean Direction | 0 | 0 | degrees |

#### Trunk Spiral and Twist

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Path Spiral Radius | 0 | 0.015 | height fraction |
| Signed Path Spiral Turns | 0 | 0.2 | turns |
| Axial Twist | -20 | 20 | degrees |

#### Roots

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Root Count | 5 | 6 | integer |
| Root Reach | 0.55 | 0.8 | local radius |
| Root Thickness | 0.4 | 0.52 | normalized breadth |
| Root Height | 0.14 | 0.2 | height fraction |

#### Primary Branch Placement

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Primary Branch Count | 9 | 14 | integer |
| Branch Start Height | 0.24 | 0.34 | 0–1 |
| Branch End Height | 0.82 | 0.94 | 0–1 |
| Branch Symmetry | 0.18 | 0.4 | 0–1 |

#### Primary Branch Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Length | 0.3 | 0.46 | height ratio |
| Branch Thickness | 0.32 | 0.48 | parent-radius ratio |
| Branch Elevation | 6 | 24 | degrees |
| Branch Curvature | 0.16 | 0.32 | 0–1 |

#### Branch Hierarchy

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Maximum Branch Order | 3 | 3 | integer |
| Secondary Density | 1.6 | 2.5 | children/primary |
| Tertiary Density | 0.6 | 1.4 | children/secondary |
| Child Scale | 0.38 | 0.5 | ratio/order |

#### Damage

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Missing Branch Chance | 0 | 0.04 | 0–1 |
| Dead Branch Chance | 0 | 0.03 | 0–1 |
| Broken Branch Chance | 0 | 0.02 | 0–1 |

#### Appearance

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bark Tint | #FFFFFF | #FFFFFF | sRGB hex |

#### Advanced Trunk Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Frequency | 0.8 | 1.6 | cycles |
| Trunk Drift | 0.02 | 0.08 | 0–1 |
| Trunk Roughness | 0.02 | 0.08 | 0–1 |
| Ridge Count | 4 | 6 | integer |
| Ridge Depth | 0.04 | 0.1 | 0–1 |

#### Advanced Branch Distribution

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Directional Bias | 0.55 | 0.8 | 0–1 |
| Directional Bias Angle | 0 | 0 | degrees |
| Tier Spacing | 0 | 0 | height fraction |

#### Advanced Primary Branch Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Arch | 0.05 | 0.2 | signed |
| Late Sag | 0.05 | 0.18 | 0–1 |
| Tip Upturn | 0.02 | 0.12 | 0–1 |
| Side Sweep | 0.1 | 0.28 | signed |

#### Advanced Forking

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Fork Chance | 0.02 | 0.12 | 0–1 |

### Norway Spruce Standard

**Reference intent:** Conifer references 1, 2 and 5.

**Foliage target:** Continuous conical foliage tiers from low trunk to tip.

#### Overall Form

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Height | 7.5 | 11.5 | m |
| Trunk Base Radius | 0.22 | 0.4 | m |
| Trunk Taper | 0.82 | 0.93 | 0–1 |

#### Trunk Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Amount | 0.02 | 0.1 | 0–1 |
| Lean Amount | 0 | 0.035 | height fraction |
| Lean Direction | 0 | 0 | degrees |

#### Trunk Spiral and Twist

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Path Spiral Radius | 0 | 0.008 | height fraction |
| Signed Path Spiral Turns | 0 | 0.1 | turns |
| Axial Twist | -8 | 8 | degrees |

#### Roots

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Root Count | 5 | 5 | integer |
| Root Reach | 0.24 | 0.38 | local radius |
| Root Thickness | 0.28 | 0.4 | normalized breadth |
| Root Height | 0.08 | 0.14 | height fraction |

#### Primary Branch Placement

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Primary Branch Count | 20 | 30 | integer |
| Branch Start Height | 0.16 | 0.24 | 0–1 |
| Branch End Height | 0.9 | 0.98 | 0–1 |
| Branch Symmetry | 0.82 | 0.96 | 0–1 |

#### Primary Branch Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Length | 0.24 | 0.36 | height ratio |
| Branch Thickness | 0.18 | 0.3 | parent-radius ratio |
| Branch Elevation | -10 | 4 | degrees |
| Branch Curvature | 0.04 | 0.14 | 0–1 |

#### Branch Hierarchy

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Maximum Branch Order | 3 | 3 | integer |
| Secondary Density | 0.8 | 1.5 | children/primary |
| Tertiary Density | 0.3 | 0.8 | children/secondary |
| Child Scale | 0.32 | 0.42 | ratio/order |

#### Damage

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Missing Branch Chance | 0 | 0.03 | 0–1 |
| Dead Branch Chance | 0.02 | 0.07 | 0–1 |
| Broken Branch Chance | 0 | 0.03 | 0–1 |

#### Appearance

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bark Tint | #FFFFFF | #FFFFFF | sRGB hex |

#### Advanced Trunk Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Frequency | 0.4 | 1 | cycles |
| Trunk Drift | 0 | 0.03 | 0–1 |
| Trunk Roughness | 0.01 | 0.04 | 0–1 |
| Ridge Count | 4 | 6 | integer |
| Ridge Depth | 0.03 | 0.08 | 0–1 |

#### Advanced Branch Distribution

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Directional Bias | 0 | 0.04 | 0–1 |
| Directional Bias Angle | 0 | 0 | degrees |
| Tier Spacing | 0.07 | 0.11 | height fraction |

#### Advanced Primary Branch Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Arch | -0.05 | 0.05 | signed |
| Late Sag | 0.12 | 0.28 | 0–1 |
| Tip Upturn | 0 | 0.05 | 0–1 |
| Side Sweep | -0.03 | 0.03 | signed |

#### Advanced Forking

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Fork Chance | 0 | 0.02 | 0–1 |

### Norway Spruce High-Crown

**Reference intent:** Conifer reference 3.

**Foliage target:** Lower 35–50% of trunk exposed; upper crown remains conical.

#### Overall Form

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Height | 8.5 | 12.5 | m |
| Trunk Base Radius | 0.22 | 0.4 | m |
| Trunk Taper | 0.82 | 0.93 | 0–1 |

#### Trunk Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Amount | 0.02 | 0.1 | 0–1 |
| Lean Amount | 0 | 0.035 | height fraction |
| Lean Direction | 0 | 0 | degrees |

#### Trunk Spiral and Twist

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Path Spiral Radius | 0 | 0.008 | height fraction |
| Signed Path Spiral Turns | 0 | 0.1 | turns |
| Axial Twist | -8 | 8 | degrees |

#### Roots

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Root Count | 5 | 5 | integer |
| Root Reach | 0.24 | 0.38 | local radius |
| Root Thickness | 0.28 | 0.4 | normalized breadth |
| Root Height | 0.08 | 0.14 | height fraction |

#### Primary Branch Placement

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Primary Branch Count | 14 | 22 | integer |
| Branch Start Height | 0.4 | 0.56 | 0–1 |
| Branch End Height | 0.91 | 0.99 | 0–1 |
| Branch Symmetry | 0.82 | 0.96 | 0–1 |

#### Primary Branch Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Length | 0.24 | 0.38 | height ratio |
| Branch Thickness | 0.18 | 0.3 | parent-radius ratio |
| Branch Elevation | -10 | 4 | degrees |
| Branch Curvature | 0.04 | 0.14 | 0–1 |

#### Branch Hierarchy

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Maximum Branch Order | 3 | 3 | integer |
| Secondary Density | 0.8 | 1.5 | children/primary |
| Tertiary Density | 0.3 | 0.8 | children/secondary |
| Child Scale | 0.32 | 0.42 | ratio/order |

#### Damage

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Missing Branch Chance | 0.08 | 0.18 | 0–1 |
| Dead Branch Chance | 0.1 | 0.22 | 0–1 |
| Broken Branch Chance | 0.04 | 0.1 | 0–1 |

#### Appearance

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bark Tint | #FFFFFF | #FFFFFF | sRGB hex |

#### Advanced Trunk Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Frequency | 0.4 | 1 | cycles |
| Trunk Drift | 0 | 0.03 | 0–1 |
| Trunk Roughness | 0.01 | 0.04 | 0–1 |
| Ridge Count | 4 | 6 | integer |
| Ridge Depth | 0.03 | 0.08 | 0–1 |

#### Advanced Branch Distribution

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Directional Bias | 0 | 0.04 | 0–1 |
| Directional Bias Angle | 0 | 0 | degrees |
| Tier Spacing | 0.08 | 0.12 | height fraction |

#### Advanced Primary Branch Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Arch | -0.05 | 0.05 | signed |
| Late Sag | 0.1 | 0.25 | 0–1 |
| Tip Upturn | 0 | 0.05 | 0–1 |
| Side Sweep | -0.03 | 0.03 | signed |

#### Advanced Forking

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Fork Chance | 0 | 0.02 | 0–1 |

### Norway Spruce Tall

**Reference intent:** Conifer reference 4.

**Foliage target:** Narrower, vertically extended conical crown.

#### Overall Form

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Height | 11.5 | 15.5 | m |
| Trunk Base Radius | 0.24 | 0.42 | m |
| Trunk Taper | 0.86 | 0.95 | 0–1 |

#### Trunk Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Amount | 0.02 | 0.08 | 0–1 |
| Lean Amount | 0 | 0.035 | height fraction |
| Lean Direction | 0 | 0 | degrees |

#### Trunk Spiral and Twist

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Path Spiral Radius | 0 | 0.008 | height fraction |
| Signed Path Spiral Turns | 0 | 0.1 | turns |
| Axial Twist | -8 | 8 | degrees |

#### Roots

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Root Count | 5 | 5 | integer |
| Root Reach | 0.26 | 0.4 | local radius |
| Root Thickness | 0.28 | 0.4 | normalized breadth |
| Root Height | 0.08 | 0.14 | height fraction |

#### Primary Branch Placement

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Primary Branch Count | 24 | 36 | integer |
| Branch Start Height | 0.2 | 0.3 | 0–1 |
| Branch End Height | 0.94 | 0.99 | 0–1 |
| Branch Symmetry | 0.88 | 0.98 | 0–1 |

#### Primary Branch Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Length | 0.2 | 0.32 | height ratio |
| Branch Thickness | 0.16 | 0.28 | parent-radius ratio |
| Branch Elevation | -10 | 4 | degrees |
| Branch Curvature | 0.04 | 0.14 | 0–1 |

#### Branch Hierarchy

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Maximum Branch Order | 3 | 3 | integer |
| Secondary Density | 0.8 | 1.5 | children/primary |
| Tertiary Density | 0.3 | 0.8 | children/secondary |
| Child Scale | 0.32 | 0.42 | ratio/order |

#### Damage

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Missing Branch Chance | 0 | 0.03 | 0–1 |
| Dead Branch Chance | 0.02 | 0.07 | 0–1 |
| Broken Branch Chance | 0 | 0.03 | 0–1 |

#### Appearance

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bark Tint | #FFFFFF | #FFFFFF | sRGB hex |

#### Advanced Trunk Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Frequency | 0.4 | 1 | cycles |
| Trunk Drift | 0 | 0.03 | 0–1 |
| Trunk Roughness | 0.01 | 0.04 | 0–1 |
| Ridge Count | 4 | 6 | integer |
| Ridge Depth | 0.03 | 0.08 | 0–1 |

#### Advanced Branch Distribution

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Directional Bias | 0 | 0.04 | 0–1 |
| Directional Bias Angle | 0 | 0 | degrees |
| Tier Spacing | 0.07 | 0.1 | height fraction |

#### Advanced Primary Branch Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Arch | -0.05 | 0.05 | signed |
| Late Sag | 0.12 | 0.28 | 0–1 |
| Tip Upturn | 0 | 0.05 | 0–1 |
| Side Sweep | -0.03 | 0.03 | signed |

#### Advanced Forking

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Fork Chance | 0 | 0.02 | 0–1 |

### Norway Spruce Drooping

**Reference intent:** A deliberate additional archetype emphasizing downward branch launch.

**Foliage target:** Foliage follows the downward branch silhouette while retaining small tip upturns.

#### Overall Form

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Height | 8 | 12 | m |
| Trunk Base Radius | 0.22 | 0.4 | m |
| Trunk Taper | 0.82 | 0.93 | 0–1 |

#### Trunk Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Amount | 0.02 | 0.1 | 0–1 |
| Lean Amount | 0 | 0.035 | height fraction |
| Lean Direction | 0 | 0 | degrees |

#### Trunk Spiral and Twist

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Path Spiral Radius | 0 | 0.008 | height fraction |
| Signed Path Spiral Turns | 0 | 0.1 | turns |
| Axial Twist | -8 | 8 | degrees |

#### Roots

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Root Count | 5 | 5 | integer |
| Root Reach | 0.24 | 0.38 | local radius |
| Root Thickness | 0.28 | 0.4 | normalized breadth |
| Root Height | 0.08 | 0.14 | height fraction |

#### Primary Branch Placement

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Primary Branch Count | 18 | 28 | integer |
| Branch Start Height | 0.18 | 0.28 | 0–1 |
| Branch End Height | 0.88 | 0.97 | 0–1 |
| Branch Symmetry | 0.82 | 0.96 | 0–1 |

#### Primary Branch Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Length | 0.28 | 0.42 | height ratio |
| Branch Thickness | 0.18 | 0.3 | parent-radius ratio |
| Branch Elevation | -28 | -14 | degrees |
| Branch Curvature | 0.06 | 0.18 | 0–1 |

#### Branch Hierarchy

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Maximum Branch Order | 3 | 3 | integer |
| Secondary Density | 0.8 | 1.5 | children/primary |
| Tertiary Density | 0.3 | 0.8 | children/secondary |
| Child Scale | 0.32 | 0.42 | ratio/order |

#### Damage

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Missing Branch Chance | 0 | 0.03 | 0–1 |
| Dead Branch Chance | 0.02 | 0.07 | 0–1 |
| Broken Branch Chance | 0 | 0.03 | 0–1 |

#### Appearance

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bark Tint | #FFFFFF | #FFFFFF | sRGB hex |

#### Advanced Trunk Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Frequency | 0.4 | 1 | cycles |
| Trunk Drift | 0 | 0.03 | 0–1 |
| Trunk Roughness | 0.01 | 0.04 | 0–1 |
| Ridge Count | 4 | 6 | integer |
| Ridge Depth | 0.03 | 0.08 | 0–1 |

#### Advanced Branch Distribution

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Directional Bias | 0 | 0.04 | 0–1 |
| Directional Bias Angle | 0 | 0 | degrees |
| Tier Spacing | 0.07 | 0.12 | height fraction |

#### Advanced Primary Branch Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Arch | -0.05 | 0.05 | signed |
| Late Sag | 0.38 | 0.62 | 0–1 |
| Tip Upturn | 0.02 | 0.08 | 0–1 |
| Side Sweep | -0.03 | 0.03 | signed |

#### Advanced Forking

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Fork Chance | 0 | 0.02 | 0–1 |

### Wych Elm Upright

**Reference intent:** Twisted references 2–5.

**Foliage target:** Large separated crown masses following forks and upper primary branches.

#### Overall Form

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Height | 9 | 14 | m |
| Trunk Base Radius | 0.55 | 0.95 | m |
| Trunk Taper | 0.58 | 0.78 | 0–1 |

#### Trunk Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Amount | 0.18 | 0.34 | 0–1 |
| Lean Amount | 0 | 0.06 | height fraction |
| Lean Direction | 0 | 0 | degrees |

#### Trunk Spiral and Twist

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Path Spiral Radius | 0.08 | 0.15 | height fraction |
| Signed Path Spiral Turns | 0.75 | 1.1 | turns |
| Axial Twist | 260 | 380 | degrees |

#### Roots

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Root Count | 5 | 6 | integer |
| Root Reach | 0.7 | 0.95 | local radius |
| Root Thickness | 0.38 | 0.5 | normalized breadth |
| Root Height | 0.18 | 0.25 | height fraction |

#### Primary Branch Placement

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Primary Branch Count | 10 | 15 | integer |
| Branch Start Height | 0.18 | 0.28 | 0–1 |
| Branch End Height | 0.82 | 0.94 | 0–1 |
| Branch Symmetry | 0.52 | 0.76 | 0–1 |

#### Primary Branch Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Length | 0.32 | 0.48 | height ratio |
| Branch Thickness | 0.38 | 0.55 | parent-radius ratio |
| Branch Elevation | 10 | 32 | degrees |
| Branch Curvature | 0.2 | 0.38 | 0–1 |

#### Branch Hierarchy

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Maximum Branch Order | 3 | 3 | integer |
| Secondary Density | 2.5 | 4 | children/primary |
| Tertiary Density | 1 | 2 | children/secondary |
| Child Scale | 0.4 | 0.52 | ratio/order |

#### Damage

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Missing Branch Chance | 0.03 | 0.08 | 0–1 |
| Dead Branch Chance | 0.02 | 0.06 | 0–1 |
| Broken Branch Chance | 0.02 | 0.06 | 0–1 |

#### Appearance

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bark Tint | #FFFFFF | #FFFFFF | sRGB hex |

#### Advanced Trunk Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Frequency | 1.2 | 2.2 | cycles |
| Trunk Drift | 0.05 | 0.13 | 0–1 |
| Trunk Roughness | 0.04 | 0.12 | 0–1 |
| Ridge Count | 5 | 8 | integer |
| Ridge Depth | 0.12 | 0.24 | 0–1 |

#### Advanced Branch Distribution

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Directional Bias | 0 | 0.12 | 0–1 |
| Directional Bias Angle | 0 | 0 | degrees |
| Tier Spacing | 0 | 0 | height fraction |

#### Advanced Primary Branch Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Arch | 0.08 | 0.28 | signed |
| Late Sag | 0.02 | 0.12 | 0–1 |
| Tip Upturn | 0.06 | 0.18 | 0–1 |
| Side Sweep | -0.14 | 0.14 | signed |

#### Advanced Forking

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Fork Chance | 0.1 | 0.28 | 0–1 |

### Wych Elm Leaning

**Reference intent:** Twisted reference 1 and the close leaning specimen.

**Foliage target:** Crown mass biased in the local lean direction.

#### Overall Form

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Height | 9 | 15 | m |
| Trunk Base Radius | 0.55 | 0.95 | m |
| Trunk Taper | 0.58 | 0.78 | 0–1 |

#### Trunk Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Amount | 0.26 | 0.48 | 0–1 |
| Lean Amount | 0.18 | 0.32 | height fraction |
| Lean Direction | 0 | 0 | degrees |

#### Trunk Spiral and Twist

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Path Spiral Radius | 0.1 | 0.2 | height fraction |
| Signed Path Spiral Turns | 0.85 | 1.25 | turns |
| Axial Twist | 300 | 460 | degrees |

#### Roots

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Root Count | 5 | 6 | integer |
| Root Reach | 0.78 | 1.05 | local radius |
| Root Thickness | 0.38 | 0.52 | normalized breadth |
| Root Height | 0.2 | 0.28 | height fraction |

#### Primary Branch Placement

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Primary Branch Count | 9 | 14 | integer |
| Branch Start Height | 0.18 | 0.3 | 0–1 |
| Branch End Height | 0.8 | 0.94 | 0–1 |
| Branch Symmetry | 0.28 | 0.55 | 0–1 |

#### Primary Branch Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Length | 0.34 | 0.52 | height ratio |
| Branch Thickness | 0.38 | 0.55 | parent-radius ratio |
| Branch Elevation | 10 | 32 | degrees |
| Branch Curvature | 0.24 | 0.44 | 0–1 |

#### Branch Hierarchy

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Maximum Branch Order | 3 | 3 | integer |
| Secondary Density | 2.5 | 4 | children/primary |
| Tertiary Density | 1 | 2 | children/secondary |
| Child Scale | 0.4 | 0.52 | ratio/order |

#### Damage

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Missing Branch Chance | 0.03 | 0.08 | 0–1 |
| Dead Branch Chance | 0.02 | 0.06 | 0–1 |
| Broken Branch Chance | 0.02 | 0.06 | 0–1 |

#### Appearance

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bark Tint | #FFFFFF | #FFFFFF | sRGB hex |

#### Advanced Trunk Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Frequency | 1.2 | 2.2 | cycles |
| Trunk Drift | 0.05 | 0.13 | 0–1 |
| Trunk Roughness | 0.04 | 0.12 | 0–1 |
| Ridge Count | 5 | 8 | integer |
| Ridge Depth | 0.12 | 0.24 | 0–1 |

#### Advanced Branch Distribution

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Directional Bias | 0.3 | 0.55 | 0–1 |
| Directional Bias Angle | 0 | 0 | degrees |
| Tier Spacing | 0 | 0 | height fraction |

#### Advanced Primary Branch Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Arch | 0.08 | 0.28 | signed |
| Late Sag | 0.02 | 0.12 | 0–1 |
| Tip Upturn | 0.06 | 0.18 | 0–1 |
| Side Sweep | 0.1 | 0.28 | signed |

#### Advanced Forking

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Fork Chance | 0.14 | 0.35 | 0–1 |

### Dead Alder

**Reference intent:** The shorter, broad, irregular dead references.

**Foliage target:** No foliage.

#### Overall Form

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Height | 6.5 | 10.5 | m |
| Trunk Base Radius | 0.28 | 0.52 | m |
| Trunk Taper | 0.65 | 0.84 | 0–1 |

#### Trunk Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Amount | 0.12 | 0.3 | 0–1 |
| Lean Amount | 0 | 0.1 | height fraction |
| Lean Direction | 0 | 0 | degrees |

#### Trunk Spiral and Twist

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Path Spiral Radius | 0 | 0.015 | height fraction |
| Signed Path Spiral Turns | 0 | 0.2 | turns |
| Axial Twist | -20 | 20 | degrees |

#### Roots

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Root Count | 5 | 6 | integer |
| Root Reach | 0.5 | 0.72 | local radius |
| Root Thickness | 0.42 | 0.55 | normalized breadth |
| Root Height | 0.13 | 0.19 | height fraction |

#### Primary Branch Placement

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Primary Branch Count | 8 | 15 | integer |
| Branch Start Height | 0.18 | 0.3 | 0–1 |
| Branch End Height | 0.78 | 0.94 | 0–1 |
| Branch Symmetry | 0.3 | 0.62 | 0–1 |

#### Primary Branch Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Length | 0.3 | 0.48 | height ratio |
| Branch Thickness | 0.26 | 0.44 | parent-radius ratio |
| Branch Elevation | 5 | 28 | degrees |
| Branch Curvature | 0.12 | 0.3 | 0–1 |

#### Branch Hierarchy

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Maximum Branch Order | 2 | 3 | integer |
| Secondary Density | 0.8 | 1.8 | children/primary |
| Tertiary Density | 0 | 0.6 | children/secondary |
| Child Scale | 0.34 | 0.46 | ratio/order |

#### Damage

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Missing Branch Chance | 0.12 | 0.25 | 0–1 |
| Dead Branch Chance | 0.85 | 1 | 0–1 |
| Broken Branch Chance | 0.15 | 0.35 | 0–1 |

#### Appearance

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bark Tint | #FFFFFF | #FFFFFF | sRGB hex |

#### Advanced Trunk Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Frequency | 0.8 | 1.6 | cycles |
| Trunk Drift | 0.02 | 0.08 | 0–1 |
| Trunk Roughness | 0.02 | 0.08 | 0–1 |
| Ridge Count | 4 | 6 | integer |
| Ridge Depth | 0.04 | 0.1 | 0–1 |

#### Advanced Branch Distribution

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Directional Bias | 0 | 0.1 | 0–1 |
| Directional Bias Angle | 0 | 0 | degrees |
| Tier Spacing | 0 | 0 | height fraction |

#### Advanced Primary Branch Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Arch | -0.08 | 0.12 | signed |
| Late Sag | 0.03 | 0.14 | 0–1 |
| Tip Upturn | 0.02 | 0.12 | 0–1 |
| Side Sweep | -0.08 | 0.08 | signed |

#### Advanced Forking

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Fork Chance | 0.08 | 0.2 | 0–1 |

### Dead Norway Spruce

**Reference intent:** The tall conifer-like dead references with downward tiers.

**Foliage target:** No foliage.

#### Overall Form

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Height | 8 | 14 | m |
| Trunk Base Radius | 0.2 | 0.42 | m |
| Trunk Taper | 0.84 | 0.96 | 0–1 |

#### Trunk Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Amount | 0.02 | 0.1 | 0–1 |
| Lean Amount | 0 | 0.035 | height fraction |
| Lean Direction | 0 | 0 | degrees |

#### Trunk Spiral and Twist

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Path Spiral Radius | 0 | 0.008 | height fraction |
| Signed Path Spiral Turns | 0 | 0.1 | turns |
| Axial Twist | -8 | 8 | degrees |

#### Roots

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Root Count | 5 | 5 | integer |
| Root Reach | 0.24 | 0.38 | local radius |
| Root Thickness | 0.28 | 0.4 | normalized breadth |
| Root Height | 0.08 | 0.14 | height fraction |

#### Primary Branch Placement

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Primary Branch Count | 14 | 26 | integer |
| Branch Start Height | 0.18 | 0.3 | 0–1 |
| Branch End Height | 0.92 | 0.99 | 0–1 |
| Branch Symmetry | 0.82 | 0.98 | 0–1 |

#### Primary Branch Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Length | 0.28 | 0.42 | height ratio |
| Branch Thickness | 0.16 | 0.28 | parent-radius ratio |
| Branch Elevation | -30 | -14 | degrees |
| Branch Curvature | 0.04 | 0.14 | 0–1 |

#### Branch Hierarchy

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Maximum Branch Order | 2 | 2 | integer |
| Secondary Density | 0.4 | 1.2 | children/primary |
| Tertiary Density | 0 | 0 | children/secondary |
| Child Scale | 0.28 | 0.4 | ratio/order |

#### Damage

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Missing Branch Chance | 0.18 | 0.35 | 0–1 |
| Dead Branch Chance | 0.95 | 1 | 0–1 |
| Broken Branch Chance | 0.2 | 0.45 | 0–1 |

#### Appearance

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bark Tint | #FFFFFF | #FFFFFF | sRGB hex |

#### Advanced Trunk Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Frequency | 0.4 | 1 | cycles |
| Trunk Drift | 0 | 0.03 | 0–1 |
| Trunk Roughness | 0.01 | 0.04 | 0–1 |
| Ridge Count | 4 | 6 | integer |
| Ridge Depth | 0.03 | 0.08 | 0–1 |

#### Advanced Branch Distribution

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Directional Bias | 0 | 0.04 | 0–1 |
| Directional Bias Angle | 0 | 0 | degrees |
| Tier Spacing | 0.08 | 0.13 | height fraction |

#### Advanced Primary Branch Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Arch | -0.05 | 0.05 | signed |
| Late Sag | 0.35 | 0.6 | 0–1 |
| Tip Upturn | 0 | 0.04 | 0–1 |
| Side Sweep | -0.03 | 0.03 | signed |

#### Advanced Forking

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Fork Chance | 0 | 0 | 0–1 |

### Dead Wych Elm

**Reference intent:** The most contorted, forked dead broadleaf reference.

**Foliage target:** No foliage.

#### Overall Form

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Height | 8.5 | 14.5 | m |
| Trunk Base Radius | 0.5 | 1 | m |
| Trunk Taper | 0.55 | 0.8 | 0–1 |

#### Trunk Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Amount | 0.22 | 0.45 | 0–1 |
| Lean Amount | 0.05 | 0.22 | height fraction |
| Lean Direction | 0 | 0 | degrees |

#### Trunk Spiral and Twist

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Path Spiral Radius | 0.08 | 0.18 | height fraction |
| Signed Path Spiral Turns | 0.75 | 1.2 | turns |
| Axial Twist | 260 | 440 | degrees |

#### Roots

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Root Count | 5 | 6 | integer |
| Root Reach | 0.7 | 0.95 | local radius |
| Root Thickness | 0.38 | 0.5 | normalized breadth |
| Root Height | 0.18 | 0.25 | height fraction |

#### Primary Branch Placement

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Primary Branch Count | 8 | 14 | integer |
| Branch Start Height | 0.18 | 0.3 | 0–1 |
| Branch End Height | 0.8 | 0.94 | 0–1 |
| Branch Symmetry | 0.3 | 0.6 | 0–1 |

#### Primary Branch Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Length | 0.34 | 0.54 | height ratio |
| Branch Thickness | 0.34 | 0.54 | parent-radius ratio |
| Branch Elevation | 8 | 34 | degrees |
| Branch Curvature | 0.2 | 0.45 | 0–1 |

#### Branch Hierarchy

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Maximum Branch Order | 2 | 3 | integer |
| Secondary Density | 1.2 | 2.5 | children/primary |
| Tertiary Density | 0.2 | 0.8 | children/secondary |
| Child Scale | 0.38 | 0.5 | ratio/order |

#### Damage

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Missing Branch Chance | 0.15 | 0.3 | 0–1 |
| Dead Branch Chance | 0.9 | 1 | 0–1 |
| Broken Branch Chance | 0.18 | 0.4 | 0–1 |

#### Appearance

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bark Tint | #FFFFFF | #FFFFFF | sRGB hex |

#### Advanced Trunk Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Frequency | 1.2 | 2.2 | cycles |
| Trunk Drift | 0.05 | 0.13 | 0–1 |
| Trunk Roughness | 0.04 | 0.12 | 0–1 |
| Ridge Count | 5 | 8 | integer |
| Ridge Depth | 0.12 | 0.24 | 0–1 |

#### Advanced Branch Distribution

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Directional Bias | 0.15 | 0.4 | 0–1 |
| Directional Bias Angle | 0 | 0 | degrees |
| Tier Spacing | 0 | 0 | height fraction |

#### Advanced Primary Branch Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Arch | 0.08 | 0.28 | signed |
| Late Sag | 0.02 | 0.12 | 0–1 |
| Tip Upturn | 0.06 | 0.18 | 0–1 |
| Side Sweep | 0.06 | 0.22 | signed |

#### Advanced Forking

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Fork Chance | 0.15 | 0.35 | 0–1 |

### Tall Dead Snag

**Reference intent:** The last tall sparse dead specimens.

**Foliage target:** No foliage.

#### Overall Form

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Height | 12 | 18 | m |
| Trunk Base Radius | 0.26 | 0.52 | m |
| Trunk Taper | 0.88 | 0.97 | 0–1 |

#### Trunk Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Amount | 0.03 | 0.12 | 0–1 |
| Lean Amount | 0 | 0.06 | height fraction |
| Lean Direction | 0 | 0 | degrees |

#### Trunk Spiral and Twist

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Path Spiral Radius | 0 | 0.01 | height fraction |
| Signed Path Spiral Turns | 0 | 0.1 | turns |
| Axial Twist | -15 | 15 | degrees |

#### Roots

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Root Count | 4 | 5 | integer |
| Root Reach | 0.32 | 0.5 | local radius |
| Root Thickness | 0.3 | 0.42 | normalized breadth |
| Root Height | 0.1 | 0.16 | height fraction |

#### Primary Branch Placement

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Primary Branch Count | 10 | 18 | integer |
| Branch Start Height | 0.28 | 0.42 | 0–1 |
| Branch End Height | 0.94 | 0.99 | 0–1 |
| Branch Symmetry | 0.76 | 0.94 | 0–1 |

#### Primary Branch Shape

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Length | 0.2 | 0.36 | height ratio |
| Branch Thickness | 0.16 | 0.28 | parent-radius ratio |
| Branch Elevation | -8 | 10 | degrees |
| Branch Curvature | 0.03 | 0.12 | 0–1 |

#### Branch Hierarchy

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Maximum Branch Order | 2 | 2 | integer |
| Secondary Density | 0.3 | 0.9 | children/primary |
| Tertiary Density | 0 | 0 | children/secondary |
| Child Scale | 0.28 | 0.38 | ratio/order |

#### Damage

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Missing Branch Chance | 0.25 | 0.45 | 0–1 |
| Dead Branch Chance | 1 | 1 | 0–1 |
| Broken Branch Chance | 0.3 | 0.55 | 0–1 |

#### Appearance

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bark Tint | #FFFFFF | #FFFFFF | sRGB hex |

#### Advanced Trunk Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Bend Frequency | 0.3 | 0.8 | cycles |
| Trunk Drift | 0 | 0.04 | 0–1 |
| Trunk Roughness | 0.01 | 0.05 | 0–1 |
| Ridge Count | 4 | 6 | integer |
| Ridge Depth | 0.04 | 0.1 | 0–1 |

#### Advanced Branch Distribution

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Directional Bias | 0 | 0.05 | 0–1 |
| Directional Bias Angle | 0 | 0 | degrees |
| Tier Spacing | 0.08 | 0.14 | height fraction |

#### Advanced Primary Branch Detail

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Branch Arch | -0.05 | 0.05 | signed |
| Late Sag | 0.08 | 0.22 | 0–1 |
| Tip Upturn | 0 | 0.04 | 0–1 |
| Side Sweep | -0.03 | 0.03 | signed |

#### Advanced Forking

| Control | Min | Max | Unit |
|---|---:|---:|---|
| Fork Chance | 0 | 0.04 | 0–1 |

### Catalog policy

- Do not create one recipe per imported specimen.
- Keep imported specimens as comparison references.
- If migration proof requires exact snapshots, store them as internal migration fixtures outside the public recipe catalog.
- Create the thirteen approved baseline assets through **Create Missing Initial Curated Recipes**, then author them normally through the recipe Inspector; `Create New Recipe` and `Copy This Recipe` remain available for later recipes.
- Validate at least 10 deterministic seeds per recipe before freezing its interval ranges.
- Do not create a separate downward-branch dead recipe initially; `Dead Norway Spruce` already owns that silhouette. Add another only if it proves visually distinct.

### Procedural tree instance

A spawned or generated `ProceduralTreeInstance` owns:

- readonly source recipe reference;
- master seed;
- exact resolved controls;
- exact editable object-level values;
- generated definition and persistent mesh references;
- explicit reapply, resample, and regenerate actions.

Recipe edits affect newly sampled trees only. An existing instance changes only through an explicit author action.

## Deterministic control sampling

Every control has a permanent stable identifier, for example:

```text
tree.height
tree.trunk.base-radius
tree.root.reach
tree.root.thickness
tree.branch.primary.count
```

Sampling is independent per control:

```text
sample01 = Hash(masterSeed, stableControlId)
resolved = Lerp(recipeMinimum, recipeMaximum, sample01)
```

Required properties:

- inserting or reordering serialized fields does not reroll another control;
- renaming an asset does not reroll a tree;
- copying a recipe preserves output for the same master seed;
- schema versions do not enter creative-value seed derivation;
- integer ranges sample inclusively;
- angle ranges can cross zero;
- color intervals sample deterministically between two endpoint colors.

## Canonical control interface

The same section names, ordering, notes, tooltips, stable IDs, units, and hard domains apply to recipes and instances.

- Recipe representation: min-max interval.
- Instance representation: exact value.

### Overall Form

Readonly section note: Height and base radius establish the tree's absolute scale. Taper controls the fraction of safe base-to-tip radius reduction and cannot enter an unresponsive clamp region. None of these controls changes root angular thickness.

1. Height
2. Trunk Base Radius
3. Trunk Taper

### Trunk Shape

Readonly section note: Bend Amount creates lateral trunk curvature. Lean Amount moves the whole trunk coherently in Lean Direction. Lean Direction has no visible effect when Lean Amount is zero.

1. Bend Amount
2. Lean Amount
3. Lean Direction

### Trunk Spiral and Twist

Readonly section note: Path Spiral moves the trunk centreline. Axial Twist rotates only the bark/cross-section frame around that centreline and does not rotate structural branch attachment azimuths. Signed Path Spiral Turns uses sign for handedness and magnitude for revolutions.

1. Path Spiral Radius
2. Signed Path Spiral Turns
3. Axial Twist

### Roots

Readonly section note: Root Count changes count. Root Reach changes ground-level radial projection. Root Thickness requests absolute angular breadth without changing crest reach; emitted support is clamped only when necessary to prevent adjacent roots from overlapping. Root Height changes vertical transition extent. The final geometry retains visible ground widening and true zero valleys.

1. Root Count
2. Root Reach
3. Root Thickness
4. Root Height

### Primary Branch Placement

Readonly section note: Count requests primary branches. Start and End Height define an ordered normalized trunk band; recipe ranges are constrained so an independently sampled End cannot fall below Start. Symmetry blends random azimuths toward even distribution.

1. Primary Branch Count
2. Branch Start Height
3. Branch End Height
4. Branch Symmetry

### Primary Branch Shape

Readonly section note: Length is relative to tree height. Thickness is relative to parent radius. Elevation controls launch angle. Curvature controls centreline bending after launch.

1. Branch Length
2. Branch Thickness
3. Branch Elevation
4. Branch Curvature

### Branch Hierarchy

Readonly section note: Maximum Branch Order enables primary, secondary, and tertiary structure. Fractional density deterministically controls the chance of one additional child. Child Scale controls successive-order length and derived thickness.

1. Maximum Branch Order
2. Secondary Density
3. Tertiary Density
4. Child Scale

### Damage

Readonly section note: Missing removes candidates before geometry creation. Dead preserves geometry and makes generated bark darker and more wind-stiff while reserving the state for later foliage exclusion. Broken shortens or truncates accepted branches.

1. Missing Branch Chance
2. Dead Branch Chance
3. Broken Branch Chance

### Appearance

Readonly section note: Bark Tint is opaque per-tree RGB data applied through a renderer property block; alpha is fixed to one. Shared materials continue to own textures, normals, smoothness, and specular response.

1. Bark Tint

### Advanced Trunk Detail

Readonly section note: Bend Frequency changes bend-cycle count. Drift is coherent cumulative movement. Roughness is local jitter. Ridge Count and Ridge Depth affect non-circular cross-section detail and may affect mesh sampling cost.

1. Bend Frequency
2. Trunk Drift
3. Trunk Roughness
4. Ridge Count
5. Ridge Depth

### Advanced Branch Distribution

Readonly section note: Directional Bias blends branches toward one preferred horizontal direction. Tier Spacing creates explicit bands across the full authored Start/End interval; zero disables tiering.

1. Directional Bias
2. Directional Bias Angle
3. Tier Spacing

### Advanced Primary Branch Detail

Readonly section note: Branch Arch is one signed mid-branch bend. Late Sag affects the latter branch. Tip Upturn begins only after 72 percent branch distance. Side Sweep bends laterally relative to the primary branch plane. Curvature, Arch, Sag, and Side Sweep are inherited by higher orders at reduced strength.

1. Branch Arch
2. Late Sag
3. Tip Upturn
4. Side Sweep

### Advanced Forking

Readonly section note: Fork Chance controls whether one structural trunk fork is created. The compact recipe-only fork form uses a fixed 68 percent placement independent of the primary branch band; a separate placement control remains deferred until reference matching proves it necessary.

1. Fork Chance

## Controls deliberately absent from the normal interface

The following are not part of the target creative interface:

- inert crown controls before procedural foliage exists;
- inert procedural foliage controls before `TREE-GEN.3`;
- false secondary/tertiary fields that the current generator ignores;
- family identity as behavioral data;
- behavioral calibration overrides;
- Age Class until it owns implemented behavior;
- schema and compatibility versions;
- stable identities as editable fields;
- source FBX paths and GUIDs;
- topology thresholds;
- radial segment counts;
- ring counts;
- branch-root topology settings;
- LOD and quality budgets;
- structural safety clamps.

Quality and structural-safety settings remain separate technical profiles or internal build settings. They do not sit beside creative controls.

## Root-control compatibility boundary

Legacy evidence continues to store `Root Buttress Count / Strength / Height / Flare`. Curated recipe generation does not translate those fields at runtime. It directly consumes `Root Count / Reach / Thickness / Height` through the independent recipe-only root equation documented below.

The historical approximation

```text
old radial extension = 0.65 * old Strength + 0.75 * (max(1, old Flare) - 1)
```

remains useful only when comparing old and new authored values. It is not the live curated-root formula and does not authorize reintroducing the old coupled Strength/Flare interface.

## Recipe management workflow

### Recipe Inspector header

Every standalone recipe Inspector contains:

- Recipe Name;
- Description;
- Tags;
- readonly Stable ID;
- Bark Material;
- Create New Recipe;
- Copy This Recipe.

### Create New Recipe

Creates a standalone recipe asset with:

- a new stable identity;
- neutral valid starter intervals;
- no family parent;
- no calibration parent;
- no palette parent;
- optional registration in the active catalog.

### Copy This Recipe

Creates a deep copy of intervals, description, tags, appearance, and material references, but assigns a new stable identity. For the same master seed, the copy samples the same controls because recipe identity does not participate in control sampling.

### Starter recipe defaults

`Create New Recipe` uses the following deliberately neutral foundation intervals. These values drive the recipe-only generator after explicit sampling/application. They are not Common, Pine, Twisted, or Dead family defaults.

| Section | Control | Starter interval |
| --- | --- | --- |
| Overall Form | Height | 6–10 m |
| Overall Form | Trunk Base Radius | 0.25–0.50 m |
| Overall Form | Trunk Taper | 0.75–0.90 |
| Trunk Shape | Bend Amount | 0.05–0.20 |
| Trunk Shape | Lean Amount | 0–0.08 of height |
| Trunk Shape | Lean Direction | 0–360° |
| Trunk Spiral and Twist | Path Spiral Radius | 0–0.02 of height |
| Trunk Spiral and Twist | Signed Path Spiral Turns | -0.50–0.50 revolutions |
| Trunk Spiral and Twist | Axial Twist | -20–20° |
| Roots | Root Count | 4–5 |
| Roots | Root Reach | 0.25–0.50 local radii |
| Roots | Root Thickness | 0.45–0.65 normalized breadth; 0.5 is H4 neutral |
| Roots | Root Height | 0.10–0.20 of tree height |
| Primary Branch Placement | Primary Branch Count | 10–18 |
| Primary Branch Placement | Branch Start Height | 0.22–0.32 of height |
| Primary Branch Placement | Branch End Height | 0.82–0.94 of height |
| Primary Branch Placement | Branch Symmetry | 0.45–0.80 |
| Primary Branch Shape | Branch Length | 0.25–0.42 of tree height |
| Primary Branch Shape | Branch Thickness | 0.25–0.45 of parent radius |
| Primary Branch Shape | Branch Elevation | -10–25° |
| Primary Branch Shape | Branch Curvature | 0.08–0.25 |
| Branch Hierarchy | Maximum Branch Order | 2–3 |
| Branch Hierarchy | Secondary Density | 1–2 children per eligible primary |
| Branch Hierarchy | Tertiary Density | 0.50–1.50 children per eligible secondary |
| Branch Hierarchy | Child Scale | 0.35–0.50 |
| Damage | Missing Branch Chance | 0–0.04 |
| Damage | Dead Branch Chance | 0–0.03 |
| Damage | Broken Branch Chance | 0–0.02 |
| Appearance | Bark Tint | white–white |
| Advanced Trunk Detail | Bend Frequency | 0.80–1.80 cycles |
| Advanced Trunk Detail | Trunk Drift | 0–0.08 |
| Advanced Trunk Detail | Trunk Roughness | 0.01–0.08 |
| Advanced Trunk Detail | Ridge Count | 4–6 |
| Advanced Trunk Detail | Ridge Depth | 0.02–0.08 |
| Advanced Branch Distribution | Directional Bias | 0–0.15 |
| Advanced Branch Distribution | Directional Bias Angle | 0–360° |
| Advanced Branch Distribution | Tier Spacing | 0–0; disabled |
| Advanced Primary Branch Detail | Branch Arch | -0.15–0.15 |
| Advanced Primary Branch Detail | Late Sag | 0.05–0.25 |
| Advanced Primary Branch Detail | Tip Upturn | 0–0.12 |
| Advanced Primary Branch Detail | Side Sweep | -0.08–0.08 |
| Advanced Forking | Fork Chance | 0–0.05 |

## Inspector behavior

- Every feature foldout defaults to collapsed.
- Foldout state is Editor session state and does not dirty recipes or scenes.
- Every foldout begins with one disabled multiline Section Notes field.
- Every field has one canonical tooltip from `TreeControlDescriptorRegistry`.
- Recipe rows use min/max numeric fields and an interval slider where applicable.
- Instance rows use one exact field.
- Angle intervals may wrap through zero.
- Conditional controls may be disabled or hidden when their parent feature is inactive.

## Spawner contract

A spawner will support:

### Single recipe

```text
Recipe: Norway Spruce Tall
```

### Weighted recipe list

```text
Norway Spruce Tall      weight 3
Norway Spruce Standard  weight 2
Norway Spruce Drooping  weight 1
```

The selected recipe, master seed, and exact resolved controls are stored on the spawned instance. Runtime selection does not imply runtime mesh generation; generation windows remain separately governed by performance policy.

## Live recipe-only generation contract

### Exact-control authority

For a recipe-driven tree, `TreeResolvedControls` is the sole creative input to `TreeGenerator`.

```text
TreeGenerationRecipe.ControlRanges
    -> TreeResolvedControls.ResolveFrom(recipe ranges, master seed)
    -> TreeGenerator.Generate(exact controls, seed, source recipe identity)
```

The exact path does not resolve or fit against:

- `TreeFamilyProfile`;
- behavioral calibration overrides;
- legacy recipe overrides;
- legacy instance overrides;
- palette inheritance.

A reference grouping label may remain on gallery instances for diagnostics, imported-reference pairing, and mesh naming. It contributes no creative values and does not enter recipe-only branch IDs.

### Consolidated semantic mapping

All 42 exposed controls have a live consumer.

Key mappings:

```text
Signed Path Spiral Turns:
    turns     = abs(value)
    handedness = sign(value)

Branch Arch:
    direction = sign(value)
    strength  = abs(value)

Secondary/Tertiary Density:
    floor(density) children
    + deterministic fractional chance of one additional child per parent

Child Scale:
    child length ratio = Child Scale
    child radius ratio = Child Scale ^ 1.25

Tier Spacing:
    zero = continuous branch placement
    positive = repeated vertical tiers over the authored branch band
```

`Tip Upturn` is a positive vertical curl with zero displacement before normalized branch distance `0.72`; it uses a smooth late window over the final 28 percent. `Late Sag` remains a separate downward late-branch envelope.

### Root contract

Recipe-only roots use the restored H4 envelopes:

```text
u = normalizedDistance / RootHeight
bodyEnvelope = 1 - u^2(3 - 2u)
footEnvelope = (1 - u)^2
```

The independent controls are:

```text
requestedSupportDegrees:
    Thickness 0.10 -> 18 degrees
    Thickness 0.50 -> 60 degrees
    Thickness 1.00 -> 112 degrees

sectorDegrees = 360 / RootCount
emittedSupportDegrees = min(requestedSupportDegrees, sectorDegrees)
verticalSupport = emittedSupportDegrees * H4ShoulderWidth(bodyEnvelope)
profilePower = lerp(4, 12, inverseLerp(0.5, 1.0, Thickness))
q = angularDistanceToNearestRoot / (verticalSupport / 2)
basis = max(0, 1 - q^profilePower)
bodyMask = basis^2
footMask = basis^3
bodyContribution = RootReach * 0.28 * bodyEnvelope * bodyMask
footContribution = RootReach * 0.72 * footEnvelope * footMask
```

At `Thickness = 0.5`, six roots reproduce the accepted H4 60-degree support and original `q^4` profile exactly. At the ground crest, body plus foot equals `Root Reach` for every Thickness. Root Count does not change the requested width; it may clamp emitted support only when overlap would otherwise occur, and that clamp is reported. Root Height changes only the vertical envelopes. Neighbouring supports terminate at zero rather than inflating valleys.

The 28/72 split is an internal fixed quality distribution, not an exposed fifth root control.

### Quality and safety policy

Recipe identity is separate from technical quality and validity clamps. The recipe-only generator uses one non-authoring runtime policy for:

- maximum branch count;
- curve samples;
- minimum branch length/radius;
- turn, arc/chord, forward-progress, and parent-return limits;
- crown-envelope safety.

Bark meshing uses one recipe-only quality preset rather than choosing tessellation behavior from an imported-reference family label. Recipe-only bark retains one neutral renderer-level Weather-wind baseline (`stiffness 0.55`, `macro strength 0.45`). Dead branches additionally carry per-vertex dead state and branch stiffness; the shared bark wind include consumes those values so Dead Branch Chance visibly darkens/desaturates bark and makes dead branches less wind-responsive. A renderer property-block gate enables this metadata only for newly generated recipe-only bark, so imported references, foliage, and legacy compatibility meshes cannot reinterpret unrelated vertex alpha as dead state. No family label contributes wind behavior.

## Gallery recipe-spawner architecture

Every procedural comparison slot owns this hierarchy:

```text
PROC_<reference>_SLOT
├── TreeReferenceSpecimen
├── TreeRecipeSpawner
└── GeneratedTree
    ├── ProceduralTreeInstance
    └── Generated Bark Mesh
```

`TreeRecipeSpawner` owns:

- one curated recipe;
- one stable spawn seed;
- imported-reference grouping/index metadata;
- stable slot identity;
- generated-child reference;
- explicit spawn/rebuild actions and last report.

It does not duplicate the 42 controls.

`ProceduralTreeInstance` owns:

- readonly source recipe;
- master seed;
- all 42 exact editable values;
- generated structure and bark;
- explicit Apply Recipe, Randomize and Apply, and Regenerate From Exact Controls actions.

Recipe edits do not silently mutate an existing generated child.

### Stable slot mapping

| Imported reference slot | Curated recipe |
|---|---|
| Common 1 | Alder Standard |
| Common 2 | Alder Standard |
| Common 3 | Alder Standard |
| Common 4 | Alder High-Crown |
| Common 5 | Alder Windswept |
| Pine 1 | Norway Spruce Standard |
| Pine 2 | Norway Spruce Standard |
| Pine 3 | Norway Spruce High-Crown |
| Pine 4 | Norway Spruce Tall |
| Pine 5 | Norway Spruce Standard |
| Twisted 1 | Wych Elm Leaning |
| Twisted 2 | Wych Elm Upright |
| Twisted 3 | Wych Elm Upright |
| Twisted 4 | Wych Elm Upright |
| Twisted 5 | Wych Elm Upright |
| Dead 1 | Dead Alder |
| Dead 2 | Dead Wych Elm |
| Dead 3 | Dead Norway Spruce |
| Dead 4 | Dead Norway Spruce |
| Dead 5 | Tall Dead Snag |

`Norway Spruce Drooping` remains a valid public recipe without being forced onto an imported reference that does not match it closely.

Each slot seed is derived from the gallery seed and stable `family:index` reference identity. Recipe name, recipe asset GUID, catalog position, generation order, and neighboring slots do not enter that derivation.

### Incremental rebuild contract

**Rebuild Curated Recipe Comparison Gallery** processes one slot per Editor update and:

1. resolves the curated recipe by permanent stable identity;
2. creates or reuses the slot spawner;
3. removes the obsolete direct legacy instance from the slot;
4. creates or reuses `GeneratedTree`;
5. samples all 42 exact controls;
6. verifies same-seed exact resampling;
7. generates the recipe-only structure;
8. verifies same-seed structural repeatability;
9. builds and repeat-validates bark/topology;
10. records slot and aggregate reports.

The operation:

- remains responsive across Editor updates;
- displays current slot, percentage, and ETA;
- is explicitly cancellable;
- writes a partial report after every completed slot;
- preserves completed slot outputs;
- is safe to rerun or resume without duplicating generated children;
- never starts from `OnValidate`, import, reload, or ordinary scene updates.

Report path:

```text
Library/PS3D/Trees/CuratedGallery/TreeCuratedGalleryGenerationReport.txt
```

## Legacy compatibility boundary

The old generator overload and old library assets remain temporarily present for explicit comparison evidence and rollback safety. They are not used by the curated gallery operation.

The Advanced Validation foldout retains **Legacy Full Rebuild (Compatibility Evidence)**. This command may read family/calibration data because that is its explicit historical purpose. The primary curated workflow requires zero behavioral family/calibration reads.

No legacy asset may be deleted until the curated gallery passes and the user accepts the new structural results.

## Implementation record

### TREE-CONTROLS.1 — Recipe-only foundation

Implemented and Unity-validated:

- standalone recipe interval schema;
- exact instance schema;
- 13 collapsed feature foldouts with readonly notes;
- canonical labels/tooltips/stable IDs;
- catalog, recipe create/copy tools;
- duplicate-using hotfix.

### TREE-CONTROLS.2 — Curated public catalog

Implemented and Unity-validated:

- thirteen curated definitions;
- 546/546 approved interval values;
- explicit create-missing, validate, reset, report-copy, and report-folder actions;
- 13/13 assets created and registered;
- zero missing bark material bindings;
- imported references retained as references rather than public recipe copies.

### TREE-CONTROLS.3 — Live recipe spawning and gallery switchover

Source implementation complete. The first live gallery run produced 19 complete passes and one isolated `Norway Spruce Tall` bark topology failure at Pine 4.

### TREE-CONTROLS.3H1 — rejected adaptive-subdivision recovery

The Pine 4 structural definition, exact controls, deterministic repeat, and zero-legacy-read gates all passed. Its only original defect consisted of twelve inward side triangles on the last three trunk strips (`ring 46–48`) while all finite-data, index, degeneracy, seam, cap, embedded-root, manifold, and containment checks passed.

H1 attempted to subdivide only those unsafe terminal strips. Live evidence disproved that model: the unsafe conical surface was intrinsic rather than under-sampled. Each midpoint pass reproduced two unsafe children from each unsafe parent, reaching `unsafeStrips=96` after `93` inserted samples. H1 therefore remains rejected as a topology solution. Its transactional bark commit and truthful independent gallery counters remain accepted.

### TREE-CONTROLS.3H2 — deterministic terminal tip closure

H2 replaces recursive midpoint subdivision with a bounded terminal-closure contract:

- `TreeBarkMeshTopologyAudit` and its existing `0.05` side-orientation requirement remain unchanged;
- the generator identifies unsafe trunk strips before vertex emission;
- correction is allowed only when every unsafe strip forms one contiguous suffix ending at the final trunk strip;
- any interior or separated unsafe strip still fails and is never hidden;
- terminal unsafe rings are removed until the remaining side-surface prefix passes the unchanged preflight;
- the removed suffix is replaced by one deterministic tapered cap from the last safe ring toward the projected original terminal endpoint;
- the closure receives its own area and forward-orientation preflight before emission;
- authored roll across the zero-radius closure is counted as completed at the apex, where axial roll is geometrically undefined;
- the bark build remains transactional, preserving any previous valid mesh on failure;
- gallery counters remain independent and the complete failure reason remains visible;
- already-passed algorithm-14 and algorithm-15 checkpoints remain reusable because both newer algorithms change geometry only for trunks that failed the unchanged topology audit.

Implemented source responsibilities:

- recipe-only exact generation overload and non-authoring safety policy;
- stable recipe-only seed streams and branch IDs independent of family/recipe names;
- all consolidated structural semantics, including density, child scale, tier spacing, signed spiral/arch, tip upturn, and independent root reach/thickness;
- recipe bark material and bark tint application;
- recipe-only bark quality settings;
- `TreeRecipeSpawner` plus its explicit Inspector actions;
- deterministic twenty-slot curated assignment table;
- procedural slot builder integration;
- generated child ownership and exact object controls;
- incremental/cancellable gallery coordinator with partial reports, ETA, resume safety, deterministic checks, bark checks, and zero-legacy-read gates;
- legacy rebuild moved under Advanced Validation.

No foliage geometry is generated by this patch.

## Current source ownership

### Recipe and exact-control authoring

```text
Assets/Game/Procedural/Trees/TreeGenerationRecipe.cs
Assets/Game/Procedural/Trees/TreeRecipeCatalog.cs
Assets/Game/Procedural/Trees/TreeRecipeControlRanges.cs
Assets/Game/Procedural/Trees/TreeResolvedControls.cs
Assets/Game/Procedural/Trees/TreeControlDescriptorRegistry.cs
Assets/Game/Procedural/Trees/TreeCuratedRecipeDefinitions.cs
```

### Live recipe generation

```text
Assets/Game/Procedural/Trees/TreeGenerator.cs
Assets/Game/Procedural/Trees/TreeGenerationParameters.cs
Assets/Game/Procedural/Trees/TreeGenerationRuntimePolicy.cs
Assets/Game/Procedural/Trees/TreeDefinition.cs
Assets/Game/Procedural/Trees/TreeBranchDefinition.cs
```

### Spawning and gallery

```text
Assets/Game/Procedural/Trees/TreeRecipeSpawner.cs
Assets/Game/Procedural/Trees/TreeCuratedGalleryAssignment.cs
Assets/Game/Procedural/Trees/ProceduralTreeInstance.cs
Assets/Game/Procedural/Trees/TreeReferenceGallery.cs
Assets/Game/Procedural/Trees/TreeReferenceSpecimen.cs
Assets/Game/Procedural/Trees/Editor/TreeRecipeSpawnerEditor.cs
Assets/Game/Procedural/Trees/Editor/TreeCuratedGalleryUtility.cs
Assets/Game/Procedural/Trees/Editor/TreeCuratedGalleryGenerationCoordinator.cs
Assets/Game/Procedural/Trees/Editor/TreeReferenceGalleryBuilder.cs
Assets/Game/Procedural/Trees/Editor/TreeReferenceGalleryEditor.cs
```

### Bark

```text
Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs
Assets/Game/Procedural/Trees/TreeBarkMeshSettings.cs
Assets/Game/Procedural/Trees/TreeBarkMeshTopologyAudit.cs
Assets/Game/Procedural/Trees/Editor/TreeBarkMeshAssetBuilder.cs
```

### Legacy compatibility evidence

```text
Assets/Game/Procedural/Trees/TreeFamilyProfile.cs
Assets/Game/Procedural/Trees/TreeReferenceCalibrationPreset.cs
Assets/Game/Procedural/Trees/TreeGenerationOverrides.cs
Assets/Game/Procedural/Trees/TreeGenerationLibrary.cs
Assets/Game/Procedural/Trees/TreeMaterialPalette.cs
Assets/Game/Procedural/Trees/Editor/TreeGalleryGenerationCoordinator.cs
Assets/Game/Procedural/Trees/Editor/TreeGenerationLibraryBuilder.cs
```

### TREE-CONTROLS.4R1 — Archive-32 resolved merge

The resolved delivery uses the user-supplied `Assets-Code-Archive(32).zip` as its exact base. Archive 32 was verified as a coherent TREE-CONTROLS.3H2 source state: generator version `5`, bark algorithm `16`, thirteen curated recipe assets, recipe spawners, stable gallery assignments, and the existing generation-library code remained present.

TREE-CONTROLS.4R1 overlays the final sixteen TREE-CONTROLS.4 project paths onto that exact baseline. It modifies fourteen existing source/document/shader files and creates `TreeControlResponseSuite.cs` plus its `.meta`. It deletes, moves, or raw-edits no serialized asset. The following generation-library and curated-authoring work is explicitly preserved byte-for-byte from Archive 32:

```text
Assets/Game/Procedural/Trees/TreeGenerationLibrary.cs
Assets/Game/Procedural/Trees/Editor/TreeGenerationLibraryBuilder.cs
Assets/Game/Procedural/Trees/TreeCuratedRecipeDefinitions.cs
Assets/Game/Procedural/Trees/TreeRecipeSpawner.cs
Assets/Game/Procedural/Trees/TreeCuratedGalleryAssignment.cs
Assets/Game/Procedural/Trees/Editor/TreeReferenceGalleryBuilder.cs
Assets/Game/Procedural/Trees/Editor/TreeRecipeSpawnerEditor.cs
Assets/Game/Demo/Profiles/Trees/TreeRecipeCatalog.asset
Assets/Game/Demo/Profiles/Trees/Recipes/Curated/*.asset
```

The generated mesh container `Assets/Game/Demo/Profiles/Trees/TreeGenerationLibrary.asset` was not present in Archive 32 and is not part of the resolved patch. If it exists in the live project, applying this source patch does not overwrite it; the first explicit generator-6/bark-18 curated rebuild is expected to refresh stale generated mesh subassets inside that container.

### TREE-CONTROLS.4R1H1 — Unity 6.5 Editor API compatibility hotfix

**Objective:** restore zero-error, zero-new-warning compilation under Unity `6000.5.0f1` without changing any generator, bark geometry, control semantics, recipe data, shader behavior, serialized asset, or response-suite workload.

**Reviewed evidence:**

- `TreeControlRangeDrawer.DrawColorRange` passed string labels to the five-argument `EditorGUILayout.ColorField` call. Unity 6000.5.0f1 resolves the supported labelled overload through `GUIContent`, as already used by `TreeResolvedControlsDrawer`.
- `TreeControlResponseSuite.CollectRepresentatives` used the deprecated sorted `FindObjectsByType` overload even though no ordering is required.
- The same method stored `GetEntityId()` results in `HashSet<int>`, invoking obsolete `EntityId -> int` conversion. `ProceduralTreeInstance` already demonstrates direct `EntityId.Equals` use.

**Approved files and sequence:**

1. Update this canonical plan and implementation record.
2. In `TreeControlRangeDrawer.cs`, use `GUIContent` labels for the opaque Min/Max colour fields while retaining eyedropper enabled, alpha hidden, HDR disabled, and forced alpha `1`.
3. In `TreeControlResponseSuite.cs`, use the unsorted `FindObjectsByType<T>(FindObjectsInactive)` overload and store direct `EntityId` values in the duplicate-prevention set.
4. Audit the exact three-file scope, source compatibility, and package inventory.

**Invariants and non-goals:**

- No generated structure, root, branch, bark, shader, recipe, seed, schema, version, or fingerprint equation changes.
- No response-suite case count, representative selection contract, output path, checkpoint, progress, ETA, or cancellation change.
- No serialized Unity asset, scene, prefab, material, layer, tag, or project-setting change.
- Representative discovery remains intentionally unsorted; matching is by stable recipe identity and stable slot identity, not enumeration order.

**Acceptance criteria:**

- Unity compilation reports none of the `ColorField`, deprecated `FindObjectsByType`, or `EntityId` conversion diagnostics supplied for TREE-CONTROLS.4R1.
- No new C# warnings are introduced by the two changed Editor files.
- The recipe range Inspector still displays Min and Max RGB colour fields with alpha hidden and stored alpha forced to one.
- The response suite still discovers exactly the four required stable representatives after a successful curated gallery rebuild.

**Performance:** editor-only API correction. Unsorted discovery avoids unnecessary sorting and is the lower-cost Unity 6.5 API. The `HashSet<EntityId>` retains expected `O(1)` duplicate checks and avoids lossy/obsolete integer conversion. No active-gameplay CPU, GPU, memory, or storage behavior changes.

**Status:** source hotfix implemented; Unity compilation remains pending user validation.

### TREE-CONTROLS.4 — control-contract repair

The exhaustive static audit traced all 42 controls from Inspector exposure through recipe sampling, exact snapshots, generator/bark equations, fingerprints, reports, and shader consumption. Thirteen controls required updates and are repaired in this patch:

1. Root Thickness: H4-neutral breadth plus lower/narrower and higher/broader response.
2. Root Count: count-independent requested width with explicit non-overlap clamp telemetry.
3. Height: stable normalized trunk sample basis.
4. Bend Frequency: true zero and no sampling reroll.
5. Axial Twist: bark-only roll, independent structural attachments.
6. Trunk Taper: continuous safe tip-radius mapping.
7. Branch Start Height: ordered recipe interval contract.
8. Branch End Height: ordered recipe interval contract.
9. Tier Spacing: explicit full-band tiers.
10. Tip Upturn: final-28-percent window.
11. Dead Branch Chance: visible bark and per-branch wind-stiffness response.
12. Fork Chance: fixed independent placement.
13. Bark Tint: opaque RGB only.

`TreeResolvedControls.CurrentSchemaVersion` and `TreeRecipeControlRanges.CurrentSchemaVersion` remain `1` because TREE-CONTROLS.4 does not change the serialized field schema. Existing exact snapshots and recipe ranges must therefore remain initialized and must not be resampled merely because control semantics were repaired. `TreeGenerator.CurrentGeneratorVersion = 6` invalidates stale generated structures; `TreeBarkMeshGenerator.BarkAlgorithmVersion = 19` invalidates stale bark meshes after the bounded root-transition repair. The generator seed-compatibility version remains unchanged because stable random streams and branch IDs are not intentionally rerolled.

The new `TreeControlResponseSuite` is editor-only and explicit. It discovers one generated representative for Alder, Norway Spruce, Wych Elm, and Dead, then runs baseline/low/neutral/high cases one bounded case per Editor update. It never mutates the selected tree, reports progress and ETA, supports cancellation, closes cleanly on assembly reload/editor shutdown, and checkpoints both TXT and CSV output after every case.

### Historical: TREE-CONTROLS.3H3 — superseded Root Thickness response attempt

The first recipe-only root mapping technically forwarded Root Thickness but only stretched a pointed angular falloff. A buttress therefore retained a single-angle outer crest even at `Root Thickness = 1.0`, and its visible ground silhouette could remain a thin spike. H3 attempted to replace that profile with:

- a Thickness-controlled rounded crest plateau;
- a Thickness-controlled smooth shoulder terminating at the count-derived sector valley;
- unchanged Root Reach amplitude at the crest;
- unchanged root count, root height, seed streams, structural graph, topology thresholds, and legacy H4 compatibility path.

Algorithm 17 added two direct measurements to every bark report:

- ground half-extension full angular width in degrees;
- ground half-extension chord width in metres.

These measurements are calculated from the root-only contribution at 50% of the authored crest reach. They provide a concrete sensitivity contract: with Reach, Count, Height, base radius, and seed held fixed, increasing Thickness must increase both width measurements while the crest multiplier remains unchanged.

H3 remained insufficient because its maximum width still did not restore accepted H4 breadth and width remained sector-relative. TREE-CONTROLS.4 supersedes the H3 geometry while retaining and expanding the direct width telemetry.

## TREE-CONTROLS.4 implementation plan and validation gates

### Stage 1 — root contract repair

- Restore accepted H4 breadth at `Root Thickness = 0.5`.
- Make Root Thickness request an absolute angular support and broaden the crest above neutral.
- Keep Root Reach crest amplitude and Root Height vertical extent invariant.
- Clamp emitted support only to prevent Root Count overlap, report requested/emitted support and clamp state, and retain zero valleys.

### Stage 2 — trunk independence repair

- Use one stable 12-point normalized recipe-only trunk basis so Height and Bend Frequency cannot change random sample keys.
- Allow Bend Frequency `0` to produce no bend cycles.
- Keep structural attachment frames untwisted; apply Axial Twist only during bark cross-section construction.
- Remap recipe-only Trunk Taper continuously from base radius to a safe tip floor of the larger of the absolute minimum radius and 4% of base radius, eliminating the base-radius-dependent dead slider region while preserving the legacy compatibility equation unchanged.

### Stage 3 — branch-placement and detail repair

- Guarantee recipe Branch End Height minimum is not below Branch Start Height maximum.
- Emit explicit tiers spanning the complete authored band without dropping the final tier.
- Place the compact v1 trunk fork at a fixed `0.68` height independently of the branch band.
- Confine Tip Upturn to a smooth final-28-percent window.

### Stage 4 — damage, appearance, and validation

- Encode dead state and per-branch stiffness in bark vertex metadata; consume it in the shared bark wind include and bark fragment shading.
- Hide Bark Tint alpha and force all stored/sampled alpha to one.
- Clarify higher-order inheritance in Curvature, Arch, Late Sag, and Side Sweep tooltips.
- Run the incremental 42-control response suite across Alder, Norway Spruce, Wych Elm, and Dead representatives.

### Live validation requirements

1. Unity compilation must complete with zero C# errors and no new warnings.
2. Rebuild the curated recipe gallery. Generator 6 and bark algorithm 18 invalidate earlier checkpoints, so the first run must rebuild all twenty slots and finish `20/20` with zero legacy behavioral reads.
3. On any generated child, run **Run 42-Control Response Suite**. The suite must remain responsive, show current representative/control/sample, elapsed time and ETA, support immediate cancellation, and checkpoint partial TXT/CSV output under `Library/PS3D/Trees/ControlResponse`.
4. Require four representatives, 42 controls, baseline/low/neutral/high cases, zero failed generation/bark cases, zero failed control invariants, and final `Status: PASS`.
5. Root-specific gates: neutral Thickness restores H4; width is monotonic; Reach does not change angular width; Height does not change ground reach/width; Count preserves requested width and reports only necessary overlap clamps.
6. Trunk-specific gates: Height and Bend Frequency retain the same trunk control-point count; Height retains normalized trunk shape; Axial Twist changes bark roll but not trunk/branch structural fingerprints; Taper remains monotonic and topology-valid.
7. Placement/detail gates: every sampled branch band is ordered; positive Tier Spacing reaches the final authored tier; Tip Upturn produces zero displacement before `t=0.72`; Fork Chance is independent of branch-band values.
8. Damage/appearance gates: Dead Chance changes dead counts and bark vertex output; Bark Tint alpha remains exactly one.
9. Reapply the same recipe and seed and require identical exact, structural, and bark fingerprints.
10. Enter Play Mode and verify living/dead bark still consumes Weather wind, cloud shadows, ordinary shadows, and depth correctly.

## Performance

The curated gallery operation is editor-only, incremental, and explicit. It does not add ordinary per-frame CPU generation.

The operation generates twenty structures and twenty bark meshes across Editor updates. Bark geometry cost is expected to differ from the legacy four-representative baseline because all twenty slots now receive generated bark. This is a deliberate gallery-validation cost, not a runtime spawn policy.

The provisional complete-tree geometry budget remains unresolved. Recipe-only root width and reach do not increase radial segment count beyond the existing count/ridge-driven quality rules, but dense hierarchy recipes may produce more branches. The aggregate report and later performance tournament must measure those costs before runtime deployment.

## Unresolved work

- Compile and run TREE-CONTROLS.4 in Unity.
- Require the rebuilt 20-slot curated gallery and complete 42-control response suite to pass before recipe tuning resumes.
- Fix any live-only response-suite or topology failure revealed by that evidence; do not tune around a failed control contract.
- After the control system passes, tune Wych Elm roots and Dead Alder structure/twist, then update the corresponding recipe intervals.
- Implement procedural foliage and its compact control interface.
- Add general gameplay/editor spawners and weighted recipe lists after the gallery vertical slice remains stable.
- Remove the legacy compatibility architecture only after curated generation is accepted.
- Establish production LOD and geometry budgets.

## Historical boundary

The imported gallery, deterministic structure generator, bark topology audit, axial twist, path spiral, H4 root envelopes, branch-root transitions, repeatability, Weather wind consumption, and cloud-shadow reception predate this recipe-only redesign.

H5 attempted to reduce root width by changing the ground mass and vertical profile. It removed explicitly required ground widening and was rejected. Recipe-only Root Thickness therefore acts only on the measurable crest/shoulder breadth, while Root Reach retains the ground-level radial foot.

This document supersedes earlier tree plans or handoffs that describe family profiles, behavioral calibration presets, or one-recipe-per-reference conversion as the intended workflow.

## TREE-CONTROLS.4H1 — Evidence-backed response-suite repairs

**Status:** source implementation complete; static audit complete; Unity validation pending.

### Objective

Resolve only the three defect families demonstrated by the completed 672-case TREE-CONTROLS.4 response suite:

1. recipe-only root-zone bark candidates can produce unsafe strip 0 at high Root Reach, high Root Thickness, or very low Root Height;
2. Tip Upturn changes final curve samples before normalized branch distance `0.72` because Catmull-Rom interpolation propagates control-point displacement backward;
3. recipe-only fork branches still receive half of trunk Axial Twist, allowing Axial Twist to change structural branch geometry when a fork is accepted.

### Acceptance criteria

- The five recorded bark-failure anchors generate topology-valid bark without weakening `TreeBarkMeshTopologyAudit` or reducing authored root controls.
- Root-zone refinement is bounded, deterministic, and limited to unsafe strips whose complete interval lies within the authored root-height transition.
- Tip Upturn preserves every emitted sample at `t <= 0.72` exactly and produces measurable displacement only in the suffix.
- Recipe-only fork construction receives zero structural twist; Axial Twist changes bark roll only.
- Generator seed-compatibility remains version `3`.
- The full response suite reaches `672/672` case passes and `168/168` representative/control passes.

### Approved files

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeGenerator.cs`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`

No scene, prefab, material, recipe asset, shader, topology-audit source, layer, tag, package, or project setting is modified.

### Reviewed evidence

- Complete 672-case report: 667 case passes, five bark failures, ten failed representative/control summaries.
- All five bark failures report `firstUnsafe=0`; affected anchors are Norway Spruce Root Thickness high, Wych Elm Root Reach high, Wych Elm Root Thickness high, Wych Elm Root Height low, and Dead Root Thickness high.
- Tip Upturn violates the prefix invariant for all four representatives.
- Wych Elm Axial Twist changes the branch fingerprint while the trunk fingerprint remains fixed.
- `TreeGenerator.CreateForkBranch` forwards `TrunkSurfaceTorsionDegrees * 0.5f` into structural fork sampling even on the recipe-only path.
- `TreeGenerator.CreateBranch` applies recipe-only Tip Upturn to Catmull-Rom control points before global curve sampling.
- `TreeBarkMeshGenerator.RefineTrunkRenderSamples` bounds root-height and twist steps but does not preflight and refine unsafe root strips.

### Invariants and non-goals

- Preserve the accepted absolute Root Thickness mapping, Root Reach crest amplitude, Root Height ownership, and Root Count overlap clamp.
- Preserve terminal trunk-tip closure behavior and all topology thresholds.
- Preserve legacy generation behavior exactly.
- Do not change recipe values, control domains, seed derivation, schema versions, or runtime shaders.
- Do not globally increase trunk resolution.

### File sequence

1. `TreeGenerator.cs`: isolate recipe-only Tip Upturn at final samples and remove recipe-only fork structural twist.
2. `TreeBarkMeshGenerator.cs`: add bounded topology-driven root-strip midpoint refinement and increment the bark algorithm version.
3. Re-read the complete review surface, audit the final diff, run static checks, then perform focused Unity validation followed by the full suite.

### Risks

- Rebuilding suffix frames must not alter copied prefix samples.
- Root refinement must reject non-root interior defects rather than hiding them.
- Added root rings must remain bounded and generation-only; ordinary gameplay receives only the resulting mesh.

### Validation status

- Source implementation: complete.
- Static consistency checks: passed; Unity compilation pending.
- Focused five-anchor bark validation: pending in Unity.
- Tip Upturn four-representative validation: pending in Unity.
- Wych Elm Axial Twist structural comparison: pending in Unity.
- Full 672-case suite: pending in Unity.

## TREE-CONTROLS.4H2 — Root Transition Reparameterization and Tip-Window Test Isolation

**Status:** source implementation in progress; Unity validation pending.

### Evidence

The bark-algorithm-19 response suite completed 672 cases but preserved all five root failures. Failure-driven root subdivision expanded unsafe-strip counts from 1–4 to 13–52, proving the failing root surface was intrinsically folded rather than merely undersampled. The same run cleared Wych Elm Axial Twist structural invariance, so the recipe-only fork correction is retained. Tip Upturn continued to fail all representatives after final-sample isolation; source tracing showed the suite compared higher-order branches in world space even when a parent branch's valid tip displacement moved their attachment frame.

### Objective

1. Replace failure-driven root subdivision with a deterministic two-phase transition: root-lobe amplitude collapses before transported trunk-frame adoption and bark-only axial roll begin.
2. Preserve full authored root reach at the ground crest and preserve the existing Root Count/Thickness support contract.
3. Keep low Root Height valid through deterministic root-height sampling rather than recursive topology-failure subdivision.
4. Retain the accepted recipe-only fork Axial Twist isolation.
5. Validate Tip Upturn on primary branches whose parent trunk is invariant, avoiding false failures caused by legitimate inherited movement of higher-order branch roots.

### Approved files

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeGenerator.cs`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeControlResponseSuite.cs`

### Invariants

- Topology thresholds and terminal-tip closure remain unchanged.
- No recipe value, seed stream, seed-compatibility version, shader, scene, prefab, material, or serialized recipe asset changes.
- Root Reach crest amplitude remains exact at normalized distance zero.
- Root lobes reach zero before trunk-frame rotation and axial bark roll are introduced.
- Axial Twist remains bark-only for recipe-only generation.
- Tip Upturn positions through normalized distance `0.72` remain unchanged on primary branches; higher-order branches may translate with a moved parent attachment and are not valid world-space isolation witnesses.
- Ordinary gameplay performs no procedural regeneration.

### Implementation sequence

1. Remove bark-algorithm-19 failure-driven root-strip subdivision.
2. Advance the bark algorithm version to invalidate generated bark.
3. Collapse recipe-only root geometry over the first 72% of Root Height.
4. Hold the root surface frame world-up through the collapse phase, then adopt the transported trunk frame over the remainder of Root Height.
5. Begin recipe-only bark axial roll only above Root Height while still reaching the authored total twist at the trunk tip.
6. Ensure short Root Height receives deterministic longitudinal samples across both phases.
7. Restrict the Tip Upturn pre-window positional invariant to primary branches.
8. Rerun the five exact root failures, all four Tip Upturn checks, and the complete 672-case suite.

### Acceptance

- Five prior root cases produce valid bark with no non-terminal unsafe strips.
- All four Tip Upturn representative checks pass without reducing the `0.72` boundary or displacement threshold.
- Wych Elm Axial Twist remains structurally invariant.
- Full suite reaches `672/672` cases and `168/168` representative/control checks.
- Curated gallery subsequently reaches `20/20` complete passes.

### TREE-CONTROLS.4H2 implementation result

**Source status:** implemented. **Unity status:** pending.

Actual modified files:

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeControlResponseSuite.cs`

`TreeGenerator.cs` was reviewed and remained unchanged; the accepted H1 recipe-only fork Axial Twist isolation is preserved.

Material source changes:

- bark algorithm advanced from 19 to 20;
- failure-driven root-strip subdivision removed;
- recipe-only root body/foot envelopes now collapse by 72% of authored Root Height;
- the root surface frame remains world-up during lobe collapse, adopts the transported trunk frame during the final 28% of Root Height, and receives no axial bark roll until above Root Height;
- short root heights receive sixteen deterministic longitudinal intervals across Root Height, bounded by a `0.00025` minimum step;
- Tip Upturn pre-window validation now compares primary branches only, because higher-order branch roots legitimately move with affected parent suffixes.

Pending validation:

- C# compilation in Unity 6000.5.0f1;
- five exact root regression cases;
- four Tip Upturn representative checks;
- Wych Elm Axial Twist retention;
- complete 672-case response suite;
- twenty-slot curated gallery rebuild.

## TREE-CONTROLS.4H3 — Safe Root Transition Envelopes

**Status:** source implementation in progress; Unity validation pending.

### Evidence

The bark-algorithm-20 suite completed 672 cases. Tip Upturn and Axial Twist passed all representatives. Seven root-sensitive bark cases remained: wide/high-reach roots failed at strip 0, while Root Height `LOW` failed near the collapse/adoption boundary at strips 8–9. Unsafe counts were reduced to 1–3, validating the two-phase direction but proving that the phase envelopes still need geometric safety.

### Objective

1. Preserve the two-phase root transition while adding a protected ground plateau and zero-slope lobe-collapse envelope.
2. Separate authored visible root-height ownership from a bounded mesh-only frame-adoption tail.
3. Ensure longitudinal refinement covers the complete effective mesh-transition interval.
4. Add explicit transition telemetry to bark reports and repeatability checks.

### Approved files

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeBarkMeshAssetBuilder.cs`

### Invariants

- Root Reach, Root Thickness, Root Count, and authored Root Height values remain unchanged.
- The ground crest retains full authored root amplitude.
- Topology thresholds and terminal-tip closure remain unchanged.
- Tip Upturn and recipe-only Axial Twist fixes remain unchanged.
- No scene, prefab, material, recipe asset, shader, seed stream, schema, layer, tag, package, or project setting changes.
- Added work remains deterministic bark-generation work only.

### Implementation sequence

1. Advance bark algorithm version to 21.
2. Hold full root amplitude through the first 10% of authored Root Height.
3. Collapse root amplitude with a quintic smootherstep from 10% through 72% of authored Root Height.
4. Finish transported-frame adoption over an effective mesh-transition tail that is at least `max(0.08 m, 0.18 × trunk base radius)` in physical length, converted to normalized trunk distance.
5. Begin bark-only axial roll only after the effective transition tail.
6. Refine longitudinal samples through the full effective transition interval.
7. Report authored height, effective transition height, safety tail, plateau end, and collapse end.
8. Rerun the seven focused root cases, then the full 672-case suite.

### Acceptance

- All seven bark-algorithm-20 root failures pass topology.
- Root Height `LOW` retains its authored value while reporting a bounded mesh-only safety tail.
- Neutral/high Root Thickness keeps monotonic breadth.
- Full suite reaches `672/672` cases and `168/168` representative/control checks.

### TREE-CONTROLS.4H3 implementation result

**Source status:** implemented. **Unity status:** pending.

Actual modified files:

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeBarkMeshAssetBuilder.cs`

Material source changes:

- bark algorithm advanced from 20 to 21;
- recipe-only root amplitude remains full through 10% of authored Root Height and then collapses with a quintic smootherstep through 72%;
- transported-frame adoption completes over an effective mesh-transition interval with a physical safety tail of at least `max(0.08 m, 0.18 × trunk base radius)`;
- bark-only axial roll begins only after the effective transition interval;
- deterministic longitudinal refinement covers the complete effective transition interval using twenty target intervals;
- bark reports and repeatability checks now include authored root height, effective transition height, safety tail, plateau end, and lobe-collapse end.

Pending validation:

- C# compilation in Unity 6000.5.0f1;
- seven exact bark-algorithm-20 root regression cases;
- complete 672-case response suite;
- twenty-slot curated gallery rebuild after the suite passes.

## TREE-CONTROLS.4H4 — Root Ring Correspondence Diagnostic

**Status:** diagnostics-only source implementation in progress; Unity validation pending.

### Evidence

The bark-algorithm-21 suite completed all 672 cases with five remaining root-topology failures. Four fail at strip 0 for high Root Thickness or Root Reach; Wych Elm Root Height `LOW` fails at strip 4. Envelope and sampling changes have altered unsafe-strip counts without clearing the broad-root defect, so another geometry change would be speculative without direct ring/triangle evidence.

### Objective

1. Preserve all Bark 21 geometry and accepted control behavior unchanged.
2. Capture the exact ring-frame, root-envelope, angular-phase, vertex, and triangle-orientation evidence for the first unsafe trunk strip before bark generation aborts.
3. Preserve partial root-transition telemetry in failed build results and failure reports.
4. Produce evidence sufficient for a conceptual explanation of whether the defect is caused by ring centre motion, frame/basis rotation, angular phase mismatch, radial-profile change, or their combination.

### Approved files

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`

### Invariants

- No geometry equation, sample count, topology threshold, control mapping, seed stream, shader, scene, prefab, material, recipe asset, or serialized data changes.
- Bark algorithm remains version 21 because generated geometry is byte-for-byte intended to remain unchanged.
- Existing transactional preservation of the previous valid bark mesh remains unchanged.
- Diagnostics run only during explicit bark generation and only emit detailed evidence when a trunk strip is unsafe.

### Implementation sequence

1. Add a bounded unsafe-strip diagnostic record for the first unsafe strip and its worst circumference segment.
2. Record ring normalized positions, centres, radii, transported frames, resolved surface frames, root envelopes, frame-adoption envelope, bark-roll progress, angular phase, cross-section multipliers, vertex coordinates, both diagonal orientation scores, and the selected worst score.
3. Append the diagnostic record to the existing non-terminal unsafe-strip failure string.
4. Assign authored/effective root-transition telemetry to the build result before branch meshing so failed builds do not return zero transition values.
5. Run focused failures only; do not rerun the full 672-case suite until the diagnostic evidence is reviewed.

### Acceptance

- Unity compiles with zero C# errors.
- Each focused root failure remains behaviorally unchanged but includes a complete `[Root Ring Correspondence Diagnostic]` section.
- The diagnostic identifies one first-unsafe strip, one worst circumference segment, and both candidate diagonal orientation minima.
- No previously passing case changes pass/fail status because this patch is diagnostics-only.

## TREE-CONTROLS.4H5R1 — Revert Audit Regression and Shadow Contour Diagnostics

**Status:** source implemented; Unity validation pending.

### Evidence

`TREE-CONTROLS.4H5` changed the authoritative root-zone orientation predicate and caused a catastrophic regression from 5 failed cases to 109. Ordinary Alder, Norway Spruce, and Dead bark baselines accumulated roughly 1,900–2,464 weak-side failures while all finite, index, degeneracy, manifold, embedded-root, and boundary-loop checks remained clean. The H5 predicate therefore did not provide a valid independent winding reference and was applied too broadly.

### Implementation

- Reverted `TreeBarkMeshGenerator.cs` and `TreeBarkMeshTopologyAudit.cs` to the H4/Bark-21 acceptance behavior.
- Removed H5 local-contour pass/fail logic, audit metadata, and authoritative contour self-intersection gate.
- Retained all H1–H4 geometry, Tip Upturn, Axial Twist, transition telemetry, and transactional failure diagnostics.
- Added shadow-only measurements to `[Root Ring Correspondence Diagnostic]`:
  - signed area and inferred winding of both projected ring contours;
  - non-authoritative contour self-intersection status;
  - orientation scores against an outward direction derived independently from each ring's signed contour winding;
  - the rejected H5 local-sweep score for direct comparison.
- Shadow measurements cannot select diagonals, alter topology counts, or change pass/fail status.
- Bark algorithm remains 21 because generated geometry and authoritative validation behavior return to H4.

### Validation

1. Unity compiles with zero C# errors.
2. Baseline bark for Alder, Norway Spruce, and Dead no longer reports mass weak-side failures.
3. The full response suite returns to the H4 baseline shape: only the five known root-sensitive cases may fail.
4. Each focused failure includes the new shadow contour-winding and local-sweep lines for evidence-based predicate design.

## TREE-CONTROLS.4H6 — Strictly Scoped Contour-Winding Root Validation

**Status:** source implemented; Unity validation pending.

### Evidence

The H5R1 shadow diagnostics restored the Bark-21 baseline (`667/672`) and showed that all five remaining failed root quads have non-self-intersecting projected contours and positive independently derived contour-winding orientation scores. The four broad-root failures score approximately `0.992–1.000`; Wych Elm Root Height `LOW` scores `0.235`, still above the existing `0.05` acceptance threshold. The legacy centre-radial scores are negative because a concave root-lobe wall may legitimately face partly toward the trunk centre.

### Implementation

- Added an explicit root-contour scope predicate requiring all of:
  - recipe-only controls;
  - trunk branch order zero;
  - both adjacent rings inside the effective root transition;
  - nonzero root-body or root-foot influence.
- The generator preflight and emitted side-triangle audit metadata now use the same signed-contour-winding expected direction for explicitly scoped root strips.
- Quad diagonal selection uses that same independent contour expectation only for scoped root strips; all ordinary trunk and branch quads retain the established authored-normal predicate.
- Projected root contours remain a hard preflight failure when either adjacent ring self-intersects.
- The final topology audit consumes per-triangle scope and expected-direction metadata generated from the actual ring contours; it does not attempt to re-infer root scope from broad branch metadata.
- Audit reports now expose radial-validated and contour-validated side-triangle counts so any accidental scope expansion is immediately visible.
- Geometry equations, control mappings, ring counts, seeds, topology thresholds, and Bark algorithm version remain unchanged (`21`).

### Validation

1. Unity compiles with zero C# errors.
2. The five focused root cases pass without projected contour self-intersections.
3. Audit reports show a bounded nonzero contour-triangle count only for recipe-only trunk root-transition strips; all other side triangles remain radial-validated.
4. The full response suite reaches `672/672` cases and `168/168` representative/control checks.


## TREE-CONTROLS.4H7 — Bounded Root-Profile Collapse

**Status:** source implemented; Unity validation pending.

### Evidence

H6 reached `670/672`. The only remaining failures were Alder and Wych Elm at Root Height `LOW = 0.01`. Their projected root contours were non-self-intersecting and outward-facing under the H6 contour predicate, but the root body and foot envelopes changed by approximately `0.213` and `0.328` across one short strip. The remaining defect is therefore collapse rate and longitudinal resolution, not contour orientation.

### Implementation

- Bark algorithm advanced to `22` because generated root-transition geometry changes.
- Recipe-only root lobe collapse now ends at a radius-relative safe distance: `max(0.72 × authored Root Height, max(0.04 m, 0.12 × trunk base radius) / tree height)`.
- The authored Root Height value, ground root footprint, Root Reach, Root Thickness, and root crest at the ground remain unchanged. Only residual lobe amplitude may finish fading above the authored interval.
- Trunk root-zone pre-sampling now targets at least 24 longitudinal intervals across the plateau-to-collapse span, while retaining the existing effective-transition and axial-twist limits. This bounds the smootherstep envelope change before topology is emitted rather than recursively subdividing failed strips.
- H6 contour-winding validation and its strict root-strip scope remain unchanged.
- Non-finite shadow local-sweep diagnostic scores now print `undefined` instead of `Infinity` or `NaN`.

### Validation

1. Unity compiles with generator `6` and bark algorithm `22`.
2. Alder and Wych Elm Root Height `LOW` pass with no projected root-ring self-intersections.
3. Authored Root Height remains `0.01`; the reported lobe-collapse end may be larger and root-zone interval count must increase deterministically.
4. The full response suite reaches `672/672` cases and `168/168` representative/control checks.

## TREE-CONTROLS.4H8 — Root-collapse strategy tournament

Status: implementation prepared; Unity validation pending.

Objective: replace serial one-variable root-height fixes with one incremental, cancellable tournament that evaluates multiple deterministic collapse strategies against the two remaining low-Root-Height failures.

Approved files:

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeRootCollapseTournament.cs`
- `Assets/Game/Procedural/Trees/Editor/ProceduralTreeInstanceEditor.cs`

Acceptance criteria:

- Run six strategies across Alder Standard and Wych Elm Leaning at Root Height `0.01`, `0.02`, and `0.04` in one incremental run (36 bark builds).
- Keep production Bark 22 geometry unchanged while tournament candidates are evaluated through a scoped build-only override.
- Rank strategies by complete topology success, then worst analytical body/foot envelope delta, then vertex cost.
- Preserve authored ground reach, thickness, root count, seed, and all non-root controls.
- Checkpoint TXT and CSV after every case; remain responsive and immediately cancellable.
- Emit a clear winner or state that no candidate passed all cases.

Candidate methods:

1. Production H7 baseline.
2. Longer physical collapse with the existing smootherstep/foot-square profile.
3. Strong physical collapse with denser sampling.
4. Moderate physical collapse with smoothstep instead of smootherstep.
5. Moderate physical collapse with a slower foot fade.
6. Hybrid longer collapse, dense sampling, smoothstep body, and slower foot fade.

Invariants and non-goals:

- No scene, prefab, material, shader, recipe, serialized asset, layer, tag, or project-setting changes.
- No authoritative production strategy change in this patch.
- H6 contour validation remains unchanged.
- The tournament override is synchronous and scoped to one build; production `Build` calls use the production strategy.

Validation: run `ProceduralTreeInstance > Exhaustive Control Validation > Run Root Collapse Tournament`, submit the complete TXT and CSV, then implement only the ranked winner in the following patch.


## TREE-CONTROLS.4H9 — Wych Elm correspondence tournament and Alder fallback record

The completed H8 36-case root-collapse tournament established two separate conclusions that must remain recoverable even if later work changes direction.

### Alder fallback, preserved as a validated contingency

Alder Standard failed only at Root Height `0.01` under production H7. Every extended-collapse candidate passed Alder at `0.01`, `0.02`, and `0.04`. The validated fallback families are therefore:

| Strategy | Minimum physical collapse | Radius factor | Intervals | Curve | Foot exponent | Alder result |
|---|---:|---:|---:|---|---:|---:|
| LongerPhysical | 0.08 m | 0.24 R | 32 | smootherstep | 2.0 | 3/3 PASS |
| StrongPhysical | 0.12 m | 0.36 R | 48 | smootherstep | 2.0 | 3/3 PASS |
| SmoothstepModerate | 0.08 m | 0.24 R | 32 | smoothstep | 2.0 | 3/3 PASS |
| SlowerFootFade | 0.08 m | 0.24 R | 32 | smootherstep | 1.35 | 3/3 PASS |
| Hybrid | 0.10 m | 0.30 R | 48 | smoothstep | 1.35 | 3/3 PASS |

If later correspondence work unexpectedly regresses Alder, `LongerPhysical` is the least aggressive documented fallback: it passed all three Alder anchors with lower deformation than production and fewer behavioral changes than the stronger variants. `Hybrid` remains the lowest-deformation fallback if minimum envelope delta takes priority over simplicity. These are fallback records only; no family-specific production branch is authorized by this document.

### Wych Elm finding

All six collapse-rate strategies failed all three Wych Elm anchors. Increasing physical distance, interval count, changing the interpolation curve, slowing foot fade, and combining those changes moved the first unsafe strip upward but did not remove it. The failure therefore follows the end of the root-collapse region and is now treated as ring correspondence/frame-and-phase adoption rather than collapse rate. Further collapse-rate tuning is retired.

### H9 tournament contract

The existing incremental tournament runner is repurposed into a 15-case Wych Elm-only tournament:

- Root Height anchors: `0.01`, `0.02`, `0.04`.
- Strategies: `ProductionH7`, `ExactZeroBeforeAdoption`, `PerfectCircularBridge`, `ContinuousPhaseCarry`, `CircularBridgePlusPhaseCarry`.
- `ExactZeroBeforeAdoption` holds the stable root frame until the root profile is mathematically zero, then performs adoption over a separate tail.
- `PerfectCircularBridge` additionally suppresses trunk ridges throughout frame adoption, creating a circular correspondence bridge.
- `ContinuousPhaseCarry` carries root angular phase into the ridge phase and blends to trunk phase only with frame adoption.
- `CircularBridgePlusPhaseCarry` combines the circular bridge and continuous phase carry.
- Production Bark 22 remains unchanged unless a strategy passes all three Wych Elm anchors.
- Ranking requires 3/3 topology passes before deformation and vertex cost are considered.
- The run remains incremental, cancellable, responsive, and checkpointed after every case.

Outputs move to `Library/PS3D/Trees/RootCorrespondenceTournament/`.

## TREE-CONTROLS.4H10 — Combined Wych Elm root-transition tournament

The completed H9 15-case correspondence tournament produced no 3/3 winner, but it isolated the remaining defect to the combination of physical collapse distance and frame adoption.

### H9 evidence retained

- `ExactZeroBeforeAdoption`, `PerfectCircularBridge`, and `CircularBridgePlusPhaseCarry` each passed Wych Elm at Root Height `0.04` and failed at `0.01` and `0.02`.
- `ContinuousPhaseCarry` failed all three anchors and increased unsafe-strip counts; it is retired and must not be promoted.
- Circular ridge suppression produced no measurable topology benefit beyond `ExactZeroBeforeAdoption`; it remains only as one combined fallback candidate.
- The failure moved toward the collapse/adoption boundary when exact-zero adoption was enabled, proving that frame adoption is one part of the defect.
- H8 already proved that collapse-rate changes alone solve Alder but not Wych Elm. H9 proved that correspondence changes alone solve only the moderate-height Wych Elm anchor. The next candidate must combine both mechanisms.

### Alder fallback preservation

The H8 fallback table in H9 remains authoritative. In particular:

- `LongerPhysical` (`0.08 m`, `0.24 R`, 32 intervals, smootherstep, foot exponent `2.0`) is the least aggressive documented Alder-safe fallback.
- `Hybrid` (`0.10 m`, `0.30 R`, 48 intervals, smoothstep, foot exponent `1.35`) is the lowest-deformation documented Alder-safe fallback.
- Neither fallback is promoted by H10; production Bark 22 remains unchanged while the tournament runs.

### H10 tournament contract

The existing incremental tournament runner is repurposed into one final 12-case Wych Elm combined tournament:

- Representative: Wych Elm Leaning only.
- Root Height anchors: `0.01`, `0.02`, `0.04`.
- Strategies:
  1. `LongerPhysicalExactZero` — `0.08 m`, `0.24 R`, 32 intervals, smootherstep, foot exponent `2.0`, exact-zero-before-adoption.
  2. `StrongPhysicalExactZero` — `0.12 m`, `0.36 R`, 48 intervals, smootherstep, foot exponent `2.0`, exact-zero-before-adoption.
  3. `HybridExactZero` — `0.10 m`, `0.30 R`, 48 intervals, smoothstep, foot exponent `1.35`, exact-zero-before-adoption.
  4. `LongerPhysicalExactZeroCircularBridge` — the least-aggressive physical profile plus exact-zero adoption and ridge suppression through the circular bridge.
- Ranking requires 3/3 topology passes before deformation or vertex cost are considered.
- If multiple strategies pass 3/3, prefer the simplest and lowest-cost candidate; `LongerPhysicalExactZero` is the expected first choice.
- Production Bark 22 geometry is unchanged unless a complete winner is later promoted explicitly.
- The runner remains incremental, cancellable, responsive, and checkpoints TXT/CSV after every case.

Outputs:

- `Library/PS3D/Trees/RootCombinedTournament/TreeRootCombinedTournamentReport.txt`
- `Library/PS3D/Trees/RootCombinedTournament/TreeRootCombinedTournament.csv`

### Acceptance

- Unity compiles with zero C# errors.
- The tournament completes `12/12` cases.
- A winner is reported only if one strategy passes all three Wych Elm anchors.
- No production build changes behavior when no tournament override is active.
- After winner promotion, the complete 672-case response suite must reach `672/672` and `168/168` before gallery tuning resumes.

## TREE-CONTROLS.4H11 — Root/trunk boundary equivalence tournament

The completed H10 combined tournament rejected all tested collapse-distance, curve, interval-count, exact-zero-adoption, and circular-bridge combinations as complete Wych Elm solutions. Every candidate passed Root Height `0.04` and failed `0.01` and `0.02`; the first unsafe strip moved with the candidate transition endpoint. This is retained as evidence that the remaining defect follows the evaluator handoff itself rather than the rate of root-profile collapse.

### Current problem contract

The recipe-only trunk uses a specialized root-zone surface evaluator near the ground and the ordinary trunk surface evaluator above it. The two evaluators are not guaranteed to emit identical corresponding vertices at their handoff. More longitudinal samples only relocate that seam. The next tournament therefore tests an explicit positional continuity contract instead of another parameter blend.

### H11 tournament contract

The incremental runner now executes 15 Wych Elm cases:

- Root Height anchors: `0.01`, `0.02`, `0.04`.
- All candidates use the documented least-aggressive Alder-safe physical profile: `0.08 m`, `0.24 R`, 32 collapse intervals, smootherstep, foot exponent `2.0`, and exact-zero-before-adoption.
- Boundary strategies:
  1. `CurrentBoundaryLongerPhysical` — control case; no explicit seam correction.
  2. `RootExtensionOneRing` — carry the exact final root-ring cross-section into the first post-boundary ring, then resume ordinary trunk evaluation.
  3. `ExactSeamMorphTwo` — copy the final root-ring offsets exactly at the boundary and morph actual vertex positions to the ordinary trunk target over two rings.
  4. `ExactSeamMorphFour` — the same explicit positional morph over four rings.
  5. `ExactSeamMorphEight` — the same explicit positional morph over eight rings; visual continuity is preferred over vertex economy.

The copied seam uses the actual final root-ring vertex offsets, not a second independent evaluation instructed to approximate the same circle. Each morph interpolates actual corresponding vertex positions from that copied root cross-section to the independently evaluated ordinary-trunk positions. Existing topology, self-intersection, manifold, degeneracy, tangent, cap, and finite-value audits remain authoritative.

### Boundary evidence

For every strategy and anchor, the report records `BoundaryMaximumMismatch`: the largest positional difference between the carried final-root cross-section and the independently evaluated first ordinary-trunk ring at the handoff. It also records the number of morph rings actually used. Ranking requires all three topology cases to pass; among complete winners, lower seam mismatch is preferred, while longer morphs are acceptable because the project prioritizes decorative tree quality over minimal vertex count.

Production Bark 22 remains unchanged when no tournament override is active. No boundary method is promoted automatically.

Outputs:

- `Library/PS3D/Trees/RootBoundaryTournament/TreeRootBoundaryTournamentReport.txt`
- `Library/PS3D/Trees/RootBoundaryTournament/TreeRootBoundaryTournament.csv`

### Acceptance

- Unity compiles with zero C# errors.
- The tournament completes `15/15` incrementally and remains cancellable.
- A winner is emitted only for `3/3` Wych Elm topology passes.
- The selected strategy must retain the existing strict topology and contour self-intersection checks.
- After explicit production promotion, the complete response suite must reach `672/672` and `168/168` before gallery tuning resumes.

## TREE-CONTROLS.4H11H1 — Candidate-aware root/trunk transition geometry

Status: implementation in progress.

### Objective

Correct the H11 experiment so every non-control candidate constructs and validates the same explicit transition geometry before unsafe-strip preflight. The experiment is quality-first: additional deterministic bark rings and vertices are permitted when they improve continuity and topology correctness. Vertex count is only a distant tie-breaker after complete topology and visual acceptance.

### Reviewed evidence

- `TreeBarkMeshGenerator.AppendBranchTube` currently calls `PrepareTopologySafeTrunkTip` before H11 position morphing, so low-height failures abort before candidate geometry exists.
- H11 modifies existing ring positions rather than inserting the documented 1/2/4/8 transition intervals.
- H11 changes positions after `BuildSurfaceVertex` but retains stale circumference tangents and substitutes radial normals.
- Final topology-audit failures return before boundary metrics and generated counts are copied into `TreeBarkMeshBuildResult`.
- The tournament report therefore cannot distinguish an inactive candidate from an activated candidate that failed final audit.

### Approved files

Modify:

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeRootCollapseTournament.cs`

Conditional only if existing per-triangle expected-direction records prove insufficient:

- `Assets/Game/Procedural/Trees/TreeBarkMeshTopologyAudit.cs`

No scene, prefab, material, recipe, shader, serialized-asset, layer, tag, component, or Inspector-control changes are approved.

### Implementation contract

1. Before trunk topology preflight, replace the ordinary root/trunk handoff span with a deterministic candidate transition containing the strategy's requested interval count.
2. Evaluate candidate positions through one shared helper used by both preflight and final mesh emission.
3. The first candidate endpoint preserves the authoritative final root cross-section; the final endpoint exactly reaches the ordinary trunk evaluator.
4. Recompute normals and circumference tangents from final candidate positions; do not retain stale evaluator tangents or radial substitute normals.
5. Keep H6 contour validation strictly scoped. Candidate strips use expected directions derived from their final ring contours through existing audit-record arrays where possible.
6. Record activation, requested/emitted intervals, evaluated mismatch, generated counts, failure stage, and full topology counters before every return.
7. Classify inactive non-control candidates as `NOT_TESTED`, not `FAIL`.
8. Keep production Bark 22 unchanged when no tournament override is active.

### Performance exception

This work adds deterministic dirty/build-time geometry only. Up to eight explicit transition intervals at 60 radial segments may add approximately 488 seam-duplicated vertices and 960 side triangles per affected trunk. This is accepted for sparse decorative trees. No per-frame regeneration or recurring allocation is permitted.

### Validation

- Unity compilation with zero C# errors.
- Corrected incremental 15-case Root Boundary Tournament.
- Every non-control case must report candidate activation and emitted intervals equal to requested intervals.
- Every failure must report a concrete failure stage and structured topology counters.
- At least one strategy must pass Wych Elm Root Height `0.01`, `0.02`, and `0.04` before production promotion.
- Full `672/672` and `168/168` validation remains blocked until a winner is explicitly promoted.

### H11H1 implementation audit

Implemented files match the approved scope:

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeRootCollapseTournament.cs`

`TreeBarkMeshTopologyAudit.cs` was reviewed but not modified. Existing per-triangle contour-selection and expected-direction arrays are sufficient: explicit transition strips now populate them from the final generated ring contours.

Material implementation changes:

- Candidate transition samples are inserted before `PrepareTopologySafeTrunkTip`, so unsafe-strip preflight evaluates candidate geometry rather than the superseded ordinary handoff.
- The 1/2/4/8 strategy values now create that many explicit intermediate transition rings between the authoritative final root sample and a physically separated ordinary-trunk endpoint.
- One position evaluator is shared by preflight and final emission.
- Carried root contours are transported through each sample's resolved trunk surface frame before positional morphing.
- Candidate normals and tangents are rebuilt from the final neighboring candidate positions.
- Explicit transition strips use contour expectations derived from their final generated contours; H6 scope outside those strips is unchanged.
- Build results preserve candidate activation, requested/emitted rings, mismatch evaluation, generated counts, failure stage, and final topology counters on failures.
- The tournament supports `PASS`, `FAIL`, `NOT_TESTED`, and `ERROR`; inactive non-control candidates are excluded from winner ranking.
- Production Bark 22 remains unchanged without an active tournament strategy.

Static verification completed:

- changed-file scope matches the declaration;
- no merge markers;
- balanced braces and parentheses in both changed C# files;
- `git diff --check` reports no whitespace errors;
- runtime/editor visibility remains valid through public build-result fields and the existing public tournament enum/API;
- no scene, prefab, recipe, material, shader, serialized asset, layer, tag, component, or Inspector UI changes.

Pending Unity verification:

- Unity 6000.5.0f1 compilation;
- corrected 15-case tournament execution;
- topology and visual winner evidence.

## TREE-CONTROLS.4H11H2 — Root Height Safe-Domain Sweep

Status: implementation in progress.

### Objective

Stop escalating boundary-morph complexity until the actual supported Root Height domain is measured. Run the production Bark 22 path across the four curated representatives, seven true Root Height values (`0.020` through `0.050` in `0.005` increments), and four root-shape profiles (recipe baseline, high reach, high thickness, and high reach plus high thickness). The sweep must identify the lowest value at which every tested configuration passes without changing or remapping the authored value.

### Acceptance criteria

- 112 deterministic cases complete incrementally across Editor updates.
- The Editor remains responsive; the run is cancellable and preserves partial TXT/CSV reports.
- Every case uses the ordinary production bark path with no tournament morph override.
- Reports preserve representative, profile, true authored Root Height, pass/fail, topology counters, vertex count, and complete failure text.
- The summary reports the lowest tested Root Height at which all 16 representative/profile combinations pass, or states that no tested common minimum exists.
- No Inspector minimum is changed by this patch; the measured result will authorize a subsequent contract patch.

### Approved files

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/Editor/TreeRootCollapseTournament.cs`
- `Assets/Game/Procedural/Trees/Editor/ProceduralTreeInstanceEditor.cs`

### Invariants and non-goals

- Preserve generator version 6, Bark algorithm 22, H1 Axial Twist, H2 Tip Upturn, and H6 scoped contour validation.
- Do not modify production bark geometry, recipes, scenes, prefabs, materials, shaders, or serialized assets.
- Do not hide, normalize, or remap Root Height values.
- Do not select the final Inspector minimum before Unity evidence exists.

### Performance

The sweep performs 112 deterministic Editor-time builds, one per Editor update. It adds no runtime code path and no generated vertices to production trees.

### H11H2 implementation and audit result

Implemented in the three approved files. The existing Inspector tournament section was replaced rather than expanded. The runner now executes 112 production Bark 22 builds: four curated representatives × four root-shape profiles × seven true Root Height values. It uses one build per Editor update, supports cancellation, flushes CSV after every case, and rewrites the partial TXT report after every case. No production geometry, control range, recipe, serialized asset, or runtime path changed.

Static consistency checks passed: balanced delimiters, no merge markers, production `TreeBarkMeshGenerator.Build(...)` signature verified, `TreeResolvedControls.RootReach` and `RootThickness` accessors verified, and runner/UI references remain within the existing editor assembly. Unity compilation and sweep execution are pending in Unity 6000.5.0f1.

## TREE-CONTROLS.4H11H3 — Wych Elm Extended Root Height Sweep

Status: implemented; Unity compilation and execution pending.

### Objective

Measure the first practical true Root Height at which Wych Elm passes the production Bark 22 path before deciding whether to cap the Wych recipe range or resume geometry repair. The previous four-representative sweep proved no common passing value through `0.050` because Wych Elm failed every tested profile, while Alder, Norway Spruce, and Dead already have substantially lower valid floors.

### Acceptance criteria

- Run exactly 32 deterministic cases: Wych Elm × four root-shape profiles × eight true Root Height values.
- Test authored values `0.055`, `0.060`, `0.070`, `0.080`, `0.100`, `0.125`, `0.150`, and `0.200` without remapping or hidden clamping.
- Use recipe baseline, Root Reach `2.0`, Root Thickness `1.0`, and both extremes together.
- Execute one bark build per Editor update; remain cancellable and checkpoint partial TXT/CSV output after every case.
- Report the first tested height where all four profiles pass and the first passing height per profile.
- Do not change the Inspector or serialized Root Height minimum in this patch.
- If the common passing floor is materially higher than the other representatives, treat that as evidence to repair Wych Elm rather than immediately accepting the cap.

### Approved files

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/Editor/TreeRootCollapseTournament.cs`
- `Assets/Game/Procedural/Trees/Editor/ProceduralTreeInstanceEditor.cs`

### Invariants and non-goals

- Preserve generator version 6 and Bark algorithm 22.
- Use only the ordinary production bark path.
- Do not modify bark geometry, topology validation, recipes, scenes, prefabs, materials, shaders, or serialized assets.
- Do not select or apply a Root Height minimum before reviewing the sweep and visual plausibility.

### Performance

The sweep performs 32 Editor-time builds, one per Editor update. It adds no runtime path and no production geometry.

## TREE-CONTROLS.4H11H4 — Wych Elm Root-Frame Strategy Tournament and Unsafe Visual Preview

Status: implementation in progress.

### Objective

Determine whether Wych Elm's isolated preflight failure below Root Height 0.125 is caused by root-frame release timing, longitudinal sampling, or contour correspondence. Provide a temporary visual build of the rejected production mesh so the defect can be inspected directly instead of inferred only from numeric diagnostics.

### Approved files

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeRootCollapseTournament.cs`
- `Assets/Game/Procedural/Trees/Editor/ProceduralTreeInstanceEditor.cs`

### Evidence reviewed

- H11H3 Wych-only sweep: every profile fails through Root Height 0.100 and passes at 0.125; Reach and Thickness do not alter the threshold.
- Current generator: root-frame influence fades independently after visible root body/foot collapse.
- Current preflight rejects the single unstable strip before a mesh can be inspected.

### Tournament matrix

- Root Height: `0.030`, `0.050`, `0.100`
- Profiles: baseline, Reach High, Thickness High, Reach + Thickness High
- Strategies:
  - Production
  - Immediate frame release after root collapse
  - Bounded delayed frame release
  - Dense frame-adoption resampling
  - Transported contour blend across the frame-adoption interval

Total: 60 incremental cases.

### Visual preview contract

- Build the Wych Elm baseline at Root Height `0.050` using production geometry.
- Preserve the rejected bark mesh only under an explicit tournament-only unsafe-preview scope.
- Create one temporary `HideFlags.DontSaveInEditor` Scene object beside the selected tree.
- Reuse the selected tree's bark material when available.
- Never save the preview object or weaken ordinary production preflight/audit rejection.

### Invariants

- Bark algorithm remains 22.
- No recipe, scene, prefab, material, shader, layer, tag, or serialized asset changes.
- Diagnostics remain incremental, cancellable, responsive, and checkpointed.
- Vertex count is not a ranking criterion before topology validity and visual quality.
- Unsafe preview behavior is inaccessible outside the explicit Editor diagnostic entry point.

### Validation

- Unity compilation.
- 60/60 tournament completion.
- Each strategy/profile/height result preserves full failure stage and audit counters.
- Temporary visual preview appears, is not saved, and production generation remains rejected for the same case.

### H11H4 implementation audit

Implemented files match the approved four-file scope. Production Bark 22 remains the default when no tournament strategy is active. The tournament contains 60 incremental cases. The unsafe visual build is thread-scoped, preserves rejected geometry only for the explicit Editor command, creates a `HideFlags.DontSaveInEditor` object, and does not save or modify the scene. Static delimiter, obsolete-strategy-reference, and changed-file-scope checks passed. Unity compilation, tournament execution, and visual inspection remain pending.

## TREE-CONTROLS.4H12 — Recipe-Owned Buttress Transition

Status: implementation in progress.

### Objective

Restore buttress persistence as an explicit recipe-only control instead of hiding the root-to-ordinary-trunk endpoint inside bark-generation constants. `Root Height` continues to own the ground-level root/body envelope. The new `Buttress Transition` control owns how early the buttress-body contour and root-aligned trunk frame complete their transition into the ordinary trunk contour and transported frame. The ground-only root-foot envelope remains owned by `Root Height`.

### Control contract

- Displayed and serialized range: `0.0–1.0`; values are true and are not remapped in the Inspector.
- `0.0`: buttress/root-frame character may persist toward the trunk tip.
- `0.25`: transition completion is approximately three quarters of the way up the trunk.
- `0.50`: transition completion is approximately halfway up the trunk.
- `1.0`: transition completes at the earliest safe endpoint, while preserving the authored Root Height and the existing physical safety tail.
- Higher values always complete the transition earlier.
- Root Count, Root Reach, Root Thickness, Root Height, trunk path, branch structure and ground-level root geometry remain independent.
- Existing recipe assets migrate to the exact range `1.0–1.0` to preserve current behavior. Authors may then widen or change the interval deliberately.

The transition endpoint is resolved as:

```text
earliestSafeEnd = existing effective root-transition endpoint
latestEnd = 1.0
buttressBodyEnd = lerp(1.0, existing root-body collapse endpoint, ButtressTransition)
rootFrameEnd = lerp(1.0, earliest safe frame endpoint, ButtressTransition)
```

The existing smooth frame-adoption curve is evaluated between the root-collapse endpoint and `completionEnd`. This patch does not weaken topology validation or introduce per-frame work.

### Acceptance criteria

1. The recipe and resolved-instance Inspectors expose `Buttress Transition` in the Roots section with the documented true `0–1` range.
2. Existing recipes resolve to `1.0` after schema migration.
3. At `0`, the measured buttress/root-frame completion endpoint moves toward the tip; at `0.5` it moves toward mid-trunk; at `1` it equals the current earliest safe transition endpoint.
4. Changing only Buttress Transition preserves ground Root Reach, Root Thickness, Root Height, root count, tree structure and branch attachments.
5. The exhaustive response suite treats the subsystem as 43 controls and validates monotonic endpoint movement plus ground-root invariance.
6. Production topology validation remains authoritative.

### Approved files

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeControlDescriptorRegistry.cs`
- `Assets/Game/Procedural/Trees/TreeRecipeControlRanges.cs`
- `Assets/Game/Procedural/Trees/TreeResolvedControls.cs`
- `Assets/Game/Procedural/Trees/TreeGenerationParameters.cs`
- `Assets/Game/Procedural/Trees/TreeGenerator.cs`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/TreeCuratedRecipeDefinitions.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeControlResponseSuite.cs`
- `Assets/Game/Procedural/Trees/Editor/ProceduralTreeInstanceEditor.cs`

### Implementation and audit result

- Removed recipe-only Ridge Count and Ridge Depth from descriptors, recipe ranges, resolved controls, curated definitions, fingerprints, reports, response cases, and ownership counts.
- Removed legacy family/override ridge fields and tests.
- Removed all independent trunk-ridge radial modulation from bark generation.
- Trunk radial sampling is now driven by Root Count only.
- Renamed the visible control to Buttress Persistence and corrected its direction: `0.0` is earliest circularization; `1.0` carries root-owned lobes to the tip.
- Removed obsolete ridge-interval gallery validation and telemetry.
- Static source audit found no remaining standalone ridge symbols in the procedural-tree module.

### Implemented source invariants

- The accepted TREE-ROOTS.4B root mass, immediate quadratic foot amplitude, TREE-ROOTS.3 anchored-foot direction, continuous bark roll, and mixed-resolution stitcher are byte-for-byte unchanged from the pre-4C source in the audited method bodies.
- The contact boost cannot exceed the radial segment count already available from the authored production trunk ceiling. For the common five-root and six-root Twisted cases this permits at most 50 and 60 sides respectively; no global radial maximum was increased.
- Direct boost demand becomes zero once `footShapeEnvelope <= 0.25` or root-only amplitude is at/below `0.35`, and ordinary contour-owned radial resolution then resumes.
- The boost is resolution-only: it changes no vertex target position equation, root envelope, root control value, topology threshold, axial render-sample density, branch radial policy, UV rule, material, or runtime component.
- Normal gameplay receives no new per-frame work. The only expected cost is additional generated bark vertices/triangles in a small number of lowest trunk rings on roots with sufficiently strong contact demand.

### Non-goals

- No scene, prefab, material, shader or curated recipe asset edits.
- No Bark algorithm promotion based solely on this control plumbing.
- No topology-threshold relaxation.
- No hidden Root Height clamp or remapping.
- No attempt to declare the Wych Elm topology defect solved before the response and visual validation gates pass.

### Performance

The control adds one scalar to recipe resolution and one interpolation in deterministic bark generation. It adds no recurring update, buffer or geometry by itself.

### TREE-CONTROLS.4H12 implementation record

Status: source implementation complete; Unity compilation and runtime validation pending.

Implemented:

- Added the forty-third recipe control, `Buttress Transition`, to the Roots descriptor section.
- Advanced recipe-range and resolved-control schemas from 1 to 2.
- Migrated existing recipe ranges to the exact interval `1.0–1.0` and re-resolved schema-1 instance snapshots.
- Added the control to deterministic sampling, fingerprints, resolved parameters, validation and reports.
- Preserved the existing behavior at `1.0`.
- Split the ground-only root-foot envelope from the buttress-body envelope. Root feet still collapse over the Root Height-owned lower region; the buttress body and root-aligned frame use the authored Buttress Transition endpoint.
- Kept axial bark roll independent by retaining its previous earliest-safe start rather than delaying it with long buttress persistence.
- Updated the exhaustive suite to 43 controls and added monotonic endpoint plus ground-root invariance checks.

Static audit:

- Actual changed files match the approved ten-file scope.
- No scenes, prefabs, materials, shaders, recipe assets, layers, tags or unrelated modules changed.
- All modified C# files have balanced braces and parentheses.
- The descriptor registry contains 43 controls.
- Every new descriptor property has a matching recipe-range and resolved-control serialized field.
- Production topology validation remains unchanged and authoritative.

Pending validation:

- Unity compilation under 6000.5.0f1.
- Inspector migration and true-value display.
- Wych Elm visual endpoint checks at `0.0`, `0.25`, `0.50`, and `1.0`.
- Focused Buttress Transition response evidence.
- Full 43-control suite only after the focused visual and topology check is accepted.

## TREE-CONTROLS.4H12H1 — Live Bark Mesh Commit Repair

Status: implementation in progress.

### Objective

Fix exact-control regeneration so the visible `Generated Bark Mesh` renderer receives the newly generated native mesh buffers immediately. The current builder generates a new candidate, reports its new geometry fingerprint and counts, then copies it into the managed destination with `EditorUtility.CopySerialized`. For `Mesh` objects this leaves the currently rendered destination's native vertex/index state stale in the Editor, so reports change while the visible tree does not.

### Reviewed evidence

- `ProceduralTreeInstanceEditor.Generate(...)` calls `RegenerateFromExactControls()` and then `TreeBarkMeshAssetBuilder.BuildOrUpdate(...)`.
- `TreeBarkMeshAssetBuilder.BuildOrUpdate(...)` builds a distinct candidate mesh, validates it, calls `CommitCandidateMesh(candidateMesh, mesh)`, assigns `filter.sharedMesh = mesh`, and records the candidate build report.
- `CommitCandidateMesh(...)` currently uses `EditorUtility.CopySerialized(candidate, destination)`.
- User evidence: Buttress Transition changes from `0.409` to `1.000`; structural and bark reports, mesh counts, root-transition metrics and geometry fingerprint all change, while the visible tree does not.

### Acceptance criteria

1. Committing a candidate copies native vertex, normal, tangent, colour, UV and index buffers through the Unity Mesh API.
2. Destination vertex/index counts equal the candidate counts before the build is accepted.
3. The `MeshFilter` is explicitly rebound after commit so Scene and Game views refresh immediately.
4. Existing managed mesh identity, asset reference, name and hide flags are preserved.
5. No production geometry, recipe values, scene objects, materials or validation thresholds change.

### Approved files

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/Editor/TreeBarkMeshAssetBuilder.cs`

### Implementation and audit result

- Removed recipe-only Ridge Count and Ridge Depth from descriptors, recipe ranges, resolved controls, curated definitions, fingerprints, reports, response cases, and ownership counts.
- Removed legacy family/override ridge fields and tests.
- Removed all independent trunk-ridge radial modulation from bark generation.
- Trunk radial sampling is now driven by Root Count only.
- Renamed the visible control to Buttress Persistence and corrected its direction: `0.0` is earliest circularization; `1.0` carries root-owned lobes to the tip.
- Removed obsolete ridge-interval gallery validation and telemetry.
- Static source audit found no remaining standalone ridge symbols in the procedural-tree module.

### Implemented source invariants

- The accepted TREE-ROOTS.4B root mass, immediate quadratic foot amplitude, TREE-ROOTS.3 anchored-foot direction, continuous bark roll, and mixed-resolution stitcher are byte-for-byte unchanged from the pre-4C source in the audited method bodies.
- The contact boost cannot exceed the radial segment count already available from the authored production trunk ceiling. For the common five-root and six-root Twisted cases this permits at most 50 and 60 sides respectively; no global radial maximum was increased.
- Direct boost demand becomes zero once `footShapeEnvelope <= 0.25` or root-only amplitude is at/below `0.35`, and ordinary contour-owned radial resolution then resumes.
- The boost is resolution-only: it changes no vertex target position equation, root envelope, root control value, topology threshold, axial render-sample density, branch radial policy, UV rule, material, or runtime component.
- Normal gameplay receives no new per-frame work. The only expected cost is additional generated bark vertices/triangles in a small number of lowest trunk rings on roots with sufficiently strong contact demand.

### Non-goals

- No Buttress Transition mapping changes.
- No topology changes.
- No recipe or scene edits.
- No additional diagnostic suite.

### Validation

- Regenerate one Wych Elm at Buttress Transition `0.000`, then `1.000`, from a fixed camera.
- Confirm the visible mesh changes immediately and the renderer's shared mesh vertex count matches the latest report.
- Confirm both builds retain topology `PASS` where generation succeeds.

### TREE-CONTROLS.4H12H1 implementation record

Status: source implementation complete; Unity compilation and live Scene-view validation pending.

Implemented:

- Replaced `EditorUtility.CopySerialized` mesh transfer with explicit native Mesh API buffer replacement.
- Copies vertices, normals, tangents, colours, UV0, every submesh index buffer/topology and bounds.
- Preserves managed mesh identity, name and hide flags.
- Uploads the destination mesh without discarding CPU data.
- Rejects the commit if destination vertex or primary index counts differ from the validated candidate.
- Explicitly clears and reassigns `MeshFilter.sharedMesh` after commit to force an immediate renderer refresh.

Audit:

- Actual changed files match the approved two-file scope.
- No generation formula, topology threshold, recipe value, scene object, material or serialized recipe asset changed.
- The exact-control regeneration call chain remains unchanged except for destination mesh commit/rebind.
- C# delimiter and whitespace checks passed.
- Unity API compilation and visible regeneration remain pending.

## TREE-CONTROLS.4H12H2H1 — Standalone-ridge removal compile hotfix

Removed two orphaned `AppendBranchTube` parameters (`ridgePitchDegrees` and `ridgeIntervalsTraversed`) that remained in the call and method signature after the standalone trunk-ridge feature was deleted. The parameters were unused; no geometry, control, migration, or validation behavior changed.

## TREE-CONTROLS.4H13 — Wych Root Height Minimum Sweep

### Objective

Add one focused, explicitly named validation action that measures the lowest production-valid Wych Elm Root Height after removal of standalone trunk ridges. The suite must not ask the operator to infer a test name or reuse an unrelated tournament.

### Acceptance criteria

- Inspector action is named `Run Wych Root Height Minimum Sweep`.
- The selected `ProceduralTreeInstance` supplies the exact Wych Elm recipe snapshot and seed.
- Buttress Persistence is fixed at `0.000` for every case.
- Root Height values are `0.020`, `0.025`, `0.030`, `0.035`, `0.040`, and `0.050`.
- Profiles are recipe baseline, Reach High (`2.000`), Thickness High (`1.000`), and Reach + Thickness High.
- Exactly 24 production bark builds run incrementally, one per Editor update.
- The suite is cancellable, reports progress and ETA, and preserves partial TXT/CSV output.
- Output files are `TreeWychRootHeightMinimumSweepReport.txt` and `TreeWychRootHeightMinimumSweep.csv` under `Library/PS3D/Trees/WychRootHeightMinimumSweep`.
- The report identifies the first Root Height that passes all four profiles, or reports that no common minimum exists in the tested domain.
- No generator, bark geometry, recipe, scene, prefab, material, or serialized asset behavior changes.

### Approved files

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/Editor/ProceduralTreeInstanceEditor.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeWychRootHeightMinimumSweep.cs`

### Reviewed evidence

- `TreeRootCollapseTournament` provides the accepted incremental/cancellable/checkpointed Editor-update execution pattern.
- `TreeBarkMeshGenerator.BuildForRootCollapseTournament(..., Production)` exercises the production bark path while retaining detailed topology evidence.
- `ProceduralTreeInstanceEditor.DrawActions` owns the current response-suite and root-tournament controls.
- Standalone ridge controls are absent from the current 41-control source, and `Buttress Persistence` is root-owned.

### Invariants and non-goals

- Production topology validation remains unchanged.
- The selected instance is not mutated.
- Root Count, seed, branch structure, recipe identity, and all unrelated controls remain unchanged.
- This patch does not set the final Root Height hard minimum or alter curated recipe ranges.
- This patch does not run the exhaustive 41-control suite.

### Implementation sequence

1. Add this canonical plan entry. **Complete**
2. Add the focused incremental suite and deterministic report contract. **Complete**
3. Add one exact Inspector section/action plus cancel/copy/open controls. **Complete**
4. Audit the final diff, stale labels, control counts, suite conflicts, and static compilation surface. **Complete**
5. Unity compile and runtime execution. **Pending operator validation**

### Static audit result

- Final source scope matches the three approved files.
- The focused action, cancel action, report-copy action, and output-folder action use one consistent suite name.
- The new suite and existing exhaustive/root-frame suites are mutually disabled in the Inspector.
- The selected instance is cloned; no serialized instance or recipe values are changed.
- C# delimiter balance passed for both modified Editor files.
- Unity compilation and execution remain pending operator validation.


## TREE-CONTROLS.4H13A — Persistence-Aware Wych Root Height Sweep

### Objective

Expand the existing Wych Root Height Minimum Sweep so the minimum is measured across realistic Buttress Persistence values rather than only the `0.000` edge case.

### Acceptance criteria

- Reuse the existing Inspector action `Run Wych Root Height Minimum Sweep`; do not add another button or section.
- Buttress Persistence values are `0.000`, `0.100`, `0.200`, `0.300`, `0.400`, `0.500`, `0.600`, and `0.700`.
- Root Height values remain `0.020`, `0.025`, `0.030`, `0.035`, `0.040`, and `0.050`.
- Profiles remain recipe baseline, Reach High (`2.000`), Thickness High (`1.000`), and Reach + Thickness High.
- Exactly 192 production bark builds run incrementally, one per Editor update.
- The suite remains cancellable, reports progress and ETA, and checkpoints partial TXT/CSV output.
- CSV records Buttress Persistence for every case.
- TXT reports the first Root Height passing all four profiles separately for each Buttress Persistence value.
- TXT also reports whether one common Root Height passes every tested persistence/profile combination.
- No generator, bark geometry, recipe, scene, prefab, material, or serialized asset behavior changes.

### Approved files

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/Editor/ProceduralTreeInstanceEditor.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeWychRootHeightMinimumSweep.cs`

### Reviewed evidence

- `TREE-CONTROLS.4H13` currently fixes Buttress Persistence at `0.000`, so its `0.050` result does not establish validity for realistic authored persistence values.
- The existing suite already provides the accepted incremental, cancellable, checkpointed execution path and production bark build.
- The existing Inspector action and output names are sufficient and must be reused to avoid diagnostic UI clutter.

### Invariants and non-goals

- Production topology validation remains unchanged.
- The selected instance and recipe are not mutated.
- Root Count, seed, branch structure, recipe identity, and unrelated controls remain unchanged.
- This patch does not set the final Root Height minimum or alter curated recipes.
- This patch does not add a visual gallery or serialized preview objects.

### Implementation sequence

1. Add this canonical plan entry. **Complete**
2. Extend the suite matrix and result model with Buttress Persistence. **Complete**
3. Add per-persistence and all-persistence report summaries. **Complete**
4. Update the existing Inspector description without adding controls. **Complete**
5. Audit final diff, stale fixed-`0.000` wording, counters, and static compilation surface. **Complete**
6. Unity compilation and execution. **Pending operator validation**

### Static audit result

- Final source scope matches the three approved files.
- The existing Inspector action and output names are unchanged; no diagnostic controls were added.
- The matrix count is `8 × 6 × 4 = 192` cases.
- Every case writes its resolved Buttress Persistence to CSV and TXT.
- The report computes one minimum for each tested persistence and one minimum common to all eight persistence values.
- The selected instance and recipe remain cloned/read-only.
- No production generator, bark geometry, topology threshold, recipe, scene, prefab, material, or serialized asset changed.
- Stale fixed-`0.000` and 24-case Inspector wording was removed.
- Modified C# brace and parenthesis balance passed.
- Unity compilation and execution remain pending operator validation.

## TREE-CONTROLS.4H13B — Extended Persistence-Aware Root Height Domain

### Objective

Replace the already-exhausted low Root Height sweep domain with a range that brackets the current Wych Elm recipe value and can establish a valid minimum for realistic Buttress Persistence values.

### Acceptance criteria

- Reuse the existing Inspector action `Run Wych Root Height Minimum Sweep`; do not add another button or section.
- Buttress Persistence values remain `0.000`, `0.100`, `0.200`, `0.300`, `0.400`, `0.500`, `0.600`, and `0.700`.
- Replace Root Height values with `0.050`, `0.075`, `0.100`, `0.125`, `0.150`, `0.175`, `0.200`, and `0.225`.
- Profiles remain recipe baseline, Reach High (`2.000`), Thickness High (`1.000`), and Reach + Thickness High.
- Exactly 256 production bark builds run incrementally, one per Editor update.
- Existing cancellation, progress/ETA, partial TXT/CSV checkpointing, selected-instance immutability, and report names remain unchanged.
- No production generator, bark geometry, topology threshold, recipe, scene, prefab, material, or serialized asset changes.

### Approved files

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/Editor/ProceduralTreeInstanceEditor.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeWychRootHeightMinimumSweep.cs`

### Reviewed evidence

- The completed 192-case H13A report found `0.050` valid only at Buttress Persistence `0.000`; every tested nonzero persistence failed throughout `0.020`–`0.050`.
- The current Wych Elm authored Root Height is approximately `0.212`, so the replacement range must bracket that value rather than continue sampling only the exhausted lower domain.
- The existing suite already satisfies the incremental, cancellable, checkpointed production-build contract and its Inspector/output names must remain unchanged.

### Invariants and non-goals

- Production topology validation remains unchanged.
- The selected instance and recipe remain read-only clones.
- Root Count, seed, branch structure, recipe identity, and unrelated controls remain unchanged.
- This patch does not select a final minimum or alter curated recipes.
- This patch does not address tree geometry density or performance; that requires a separately approved production-geometry plan.

### Implementation sequence

1. Add this canonical plan entry. **Complete**
2. Replace the Root Height sweep values and matrix capacity. **Complete**
3. Update existing Inspector/report wording and expected case count. **Complete**
4. Audit final diff, stale range/count text, and static compilation surface. **Complete**
5. Unity compilation and execution. **Pending operator validation**

## TREE-GEOMETRY.1 — Procedural Tree Geometry Efficiency Audit

### Objective

Add one bounded, incremental audit that attributes current bark geometry and serialized-structure cost across all twenty procedural comparison-gallery trees, then compares the unchanged current production policy with conservative and aggressive diagnostic-only efficiency policies under a fixed isometric capture contract.

### Acceptance criteria

- Production `TreeBarkMeshSettings.CreateRecipeOnlyDefaults()` and `CreateVerticalSliceDefaults(...)` continue to select the exact current geometry policy.
- The audit runs exactly twenty procedural comparison-gallery trees through three policies: Current, Conservative, and Aggressive.
- At most one bounded tree/policy generation/build case starts per Editor update; its asynchronous capture may complete on a later update before the next case begins.
- The run is cancellable, reports progress and ETA, and checkpoints Markdown plus aggregate and per-branch CSV files after every completed case.
- Every candidate runs the existing topology audit without weakened thresholds.
- The report records structural branch/control-point/sample counts, serialized structure estimates, bark counts by trunk/root/ordinary-trunk/branch order/cap/seam category, radial and axial sampling telemetry, generation/build/audit/upload times, mesh-memory estimates, renderer/shadow state, and current-versus-known-recent-baseline aggregate deltas.
- Each policy writes a deterministic PNG capture using the exact `Main Camera` rotation, projection matrix, aspect, clipping planes, and selected-gallery reference distance when available. Captures use a temporary unlit material inside an isolated preview Scene, submit `AsyncGPUReadback` without synchronous waits or `ReadPixels`, and record silhouette deviation from Current only when the fixed projection is neither empty nor border-clipped.
- No scene object, prefab, material, recipe, generated library mesh, serialized asset, layer definition, tag, runtime behavior, or production default is modified.
- Traditional distance-based LOD remains explicitly excluded. Spatial loading/unloading remains a separate recommendation from loaded-tree mesh reduction.
- The exhausted Wych Root Height sweep Inspector block is replaced rather than adding another permanent diagnostic section.

### Approved files

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeBarkMeshSettings.cs`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeGeometryEfficiencyAudit.cs`
- `Assets/Game/Procedural/Trees/Editor/ProceduralTreeInstanceEditor.cs`

### Reviewed evidence

- `TreeBarkMeshSettings.ResolveRadialSegments(...)` currently assigns the complete trunk `max(authored trunk sides, Root Count × 10)` radial count, clamped to 64.
- `TreeBarkMeshGenerator.RefineTrunkRenderSamples(...)` derives a dense step from the short lower-root collapse span and applies it to every source segment whose start lies below the full Buttress Persistence transition endpoint.
- Non-trunk branches retain their complete structural sample lists while branch-order radial counts remain close enough that small tertiary branches can approach primary-branch mesh cost.
- `TreeBranchDefinition` serializes complete control points plus transported-frame samples; each sample contains position, tangent, normal, binormal, radius, and normalized distance.
- `TreeBarkMeshAssetBuilder.BuildOrUpdate(...)` uses ordinary defaults and performs a second full build for repeatability before committing the validated candidate.
- The complete comparison gallery owns exactly twenty `ProceduralTreeInstance` slots beneath `TreeReferenceGalleryBuilder.CompleteGalleryRootName`.
- The supplied handoff reports the recent aggregate comparison `18,585 vertices / 30,734 triangles` versus current `249,677 / 419,680`; this historical aggregate is evidence only and is not represented as a per-category old-revision measurement.

### Invariants and non-goals

- Current production geometry, fingerprints, topology thresholds, and generated mesh commit behavior remain unchanged.
- Diagnostic policies never write candidate meshes into the generation library or scene.
- Fixed-camera acceptance uses the exact project `Main Camera` projection when available. Every candidate is centered without auto-fitting, so camera-space scale and clipping remain production evidence rather than per-tree presentation framing.
- This patch does not choose a new production policy, change recipe ranges, fix the non-monotonic Root Height topology defect, remove root-topology tournament code, implement streaming, or modify serialized structure retention.
- The audit may recommend those later actions only from measured evidence.

### Implementation sequence

1. Record this canonical plan before source edits. **Complete**
2. Add diagnostic-only policy selection and accounting/timing telemetry while preserving Current behavior. **Complete — static verification passed**
3. Add the incremental twenty-tree × three-policy audit, checkpointed reports, captures, and silhouette comparison. **Complete — static verification passed**
4. Replace the exhausted Wych Root Height Inspector block with the geometry-efficiency audit controls. **Complete — static verification passed**
5. Run static compilation-surface, scope, default-preservation, non-blocking capture, and report-contract audits. **Complete — 28 / 28 checks passed**
6. Unity compilation and audit execution. **Pending operator validation**

### Implemented contract

- `TreeBarkMeshSettings` now owns a non-serialized `TreeBarkMeshEfficiencyPolicy`. Ordinary factories remain `Current`; only `CreateEfficiencyAuditDefaults(...)` enables Conservative/Aggressive behavior and audit telemetry.
- `TreeBarkMeshGenerator.Build(...)` collects per-branch category accounting, stage timings, and estimated vertex/index payload only when audit telemetry is enabled. Production builds do not allocate those records or stopwatches.
- Current radial resolution, trunk subdivision algebra, settings version `8`, bark algorithm version `22`, and Current input-fingerprint composition remain unchanged.
- Conservative contains dense root-collapse axial sampling to the actual root-lobe collapse domain, applies bounded branch-ring reduction, and lowers diagnostic radial maxima. Aggressive pushes the same diagnostic boundaries further. Neither policy is selected by production code.
- `TreeGeometryEfficiencyAudit` runs `20 × 3 = 60` cases through `EditorApplication.update`, one bounded generation/build case per update. Each visual capture is submitted through `AsyncGPUReadback` and polled on later Editor updates; no synchronous GPU readback or wait is used. It blocks or cancels on conflicting tree jobs, supports cancellation after the current bounded case, reports progress/ETA, and checkpoints after every completed case.
- Output is restricted to `Library/PS3D/Trees/GeometryEfficiencyAudit`: one Markdown report, a 75-column aggregate CSV, a 38-column per-branch CSV, and per-policy PNG captures.
- Current-vs-existing generated mesh count parity and fresh structural-fingerprint parity are recorded. Historical `18,585 / 30,734` evidence is compared only after all twenty Current cases pass; old revisions are not represented as remeasured category data.
- Aggregate geometry categories are additive: root-lobe, Buttress Persistence, ordinary-trunk, and branch-order side geometry plus separately reported caps. Seam duplicates remain an explicitly identified subset rather than a second additive category.
- Renderer accounting records all child renderers, estimated active material draws, shadow-casting renderer and draw counts, and source-mesh triangle estimates for shadow-casting renderers.
- The prior Wych Root Height sweep Inspector block is replaced with one Run/Cancel action and one existing-report row. The underlying historical sweep implementation remains untouched.

### Static evidence

- Approved-scope diff: exactly the five files listed above.
- C# delimiter scan: all four modified/new C# files passed.
- Production factory body comparison: `CreateRecipeOnlyDefaults()` and `CreateVerticalSliceDefaults(...)` match the supplied archive.
- Current radial policy: exhaustive parity across tested branch orders, Root Counts, and authored segment extrema passed.
- Current trunk subdivision: 100,000 deterministic randomized interval cases matched the previous subdivision algebra.
- CSV schema: aggregate header/row `75 / 75`; branch header/row `38 / 38`.
- Safety scan: no `AssetDatabase`, `Undo`, layer/tag mutation, scene save, prefab save, generated-mesh commit path, `ReadPixels`, `WaitForCompletion`, blocking wait, or temporary-render-texture early release exists in the audit. Captures use `EditorSceneManager.NewPreviewScene()`, `AsyncGPUReadback.Request(...)`, and `ClosePreviewScene(...)`; the capture RenderTexture remains alive until the request completes.
- Unity compilation, execution, topology results, timings, screenshots, and measured reduction percentages remain unverified until the operator runs the audit.

### Operator validation

1. Select any generated procedural tree beneath `Tree Reference Gallery/Complete Imported Gallery`, then run `Procedural Tree Instance Inspector → Procedural Tree Geometry Efficiency Audit → Run Geometry Efficiency Audit`.
2. Confirm the audit window advances from `0 / 60` to `60 / 60`, remains responsive during the `awaiting asynchronous GPU capture` phase, shows elapsed time and ETA, and allows `Cancel After Current Bounded Case` without losing completed rows.
3. Open `Library/PS3D/Trees/GeometryEfficiencyAudit/TreeGeometryEfficiencyAudit.md`; require all twenty Current cases to pass topology and existing-mesh count parity, then review Conservative/Aggressive topology, category totals, and fixed-camera capture warnings.
4. Submit the complete Markdown report, both CSV files, and the `Captures` folder if any Current parity failure, topology failure, empty capture, border clipping, or visible silhouette/branch degradation is reported.

## TREE-GEOMETRY.1B — Capture Isolation and Baseline-Integrity Correction

### Objective

Correct the completed TREE-GEOMETRY.1 audit after Unity evidence proved that its visual captures rendered the loaded gameplay scene instead of only the candidate tree, and ensure the three-policy tournament measures the exact serialized gallery structure rather than silently substituting a freshly regenerated structure that usually differs from the visible/generated instance.

### Acceptance criteria

- The diagnostic camera is explicitly assigned to the `EditorSceneManager.NewPreviewScene()` scene through `Camera.scene` before `Camera.Render()`.
- Captures contain only the temporary candidate tree rendered with the selected Main Camera rotation, projection, aspect, clipping planes, and reference distance; no gameplay scene, player area, vegetation, or unrelated renderer may enter the image.
- The three geometry policies build the exact `ProceduralTreeInstance.GeneratedDefinition` stored on each comparison-gallery slot so Current-versus-existing-mesh parity and policy deltas isolate bark policy rather than structural-regeneration drift.
- Fresh deterministic regeneration remains measured separately for generation time and structural-fingerprint parity; a mismatch remains visible evidence but does not replace the serialized structure used by the geometry tournament.
- The final report explicitly gates interpretation on Current generated-mesh parity and complete Current/Conservative fixed-camera silhouettes.
- Existing production factories, bark topology thresholds, geometry policies, scenes, prefabs, materials, recipes, generated mesh assets, layers, and tags remain unchanged.
- Output remains incremental, cancellable, checkpointed, and asynchronous with no synchronous GPU readback or blocking wait.

### Approved files

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/Editor/TreeGeometryEfficiencyAudit.cs`

### Reviewed evidence

- The completed 60-case report recorded all sixty captures as border-clipped and the operator observed that the PNGs showed the Editor/gameplay area around the player instead of the generated tree.
- `BeginSilhouetteCapture(...)` created and populated a preview Scene but never assigned that Scene to the Camera; moving the Camera GameObject into the preview Scene did not establish the explicit camera scene-rendering contract.
- The completed report measured only `1 / 20` fresh structural fingerprint matches and `0 / 20` Current generated-mesh count matches because `PrepareDefinition(...)` assigned the fresh regeneration to `Target.Definition` and used it for all policy builds.
- `TreeBarkMeshAssetBuilder` builds the production bark mesh from `ProceduralTreeInstance.GeneratedDefinition` using `CreateRecipeOnlyDefaults()` or `CreateVerticalSliceDefaults(...)`; the audit must use the same stored definition to make Current parity meaningful.

### Invariants and non-goals

- This correction does not promote Conservative or Aggressive geometry.
- It does not resolve structural-regeneration drift; it reports that drift separately.
- It does not auto-fit or zoom the camera per tree; production-scale clipping remains valid failure evidence.

### Implementation sequence

1. Record this correction and exact two-file scope. **Complete**
2. Bind the capture Camera to the created preview Scene. **Complete**
3. Use the serialized generated definition for all policy builds while preserving fresh-regeneration telemetry. **Complete**
4. Add explicit baseline-parity and visual-evidence decision gates. **Complete**
5. Audit source scope, production-path preservation, asynchronous capture, CSV schema, and compilation surface. **Complete**
6. Unity compilation and corrected 60-case execution. **Pending operator validation**


## TREE-GEOMETRY.2 — Patch 1 Axial Sampling Containment

### Objective

Promote the measured axial-efficiency correction into the single production bark representation without mixing in radial-density changes. Dense lower-root sampling must stop at the actual root-lobe collapse boundary, and non-trunk branches must retain samples according to deterministic geometric error rather than fixed structural sample counts.

### Production changes

- `TreeBarkMeshSettings` version advances from `8` to `9`.
- `TreeBarkMeshGenerator.BarkAlgorithmVersion` advances from `22` to `23`.
- Production `Current` retains the exact existing radial policy:
  - trunk: `max(authored trunk sides, Root Count × 10)`, capped at `64`;
  - primary / secondary / tertiary: existing authored values.
- Dense 24-interval lower-root refinement is confined to `CalculateEffectiveRootCollapseHeight(...)`.
- A source span crossing the root-collapse boundary is split exactly at that normalized boundary before dense refinement. The lower span receives root-collapse density; the upper span does not.
- Buttress Persistence and ordinary trunk spans add rings only when required by:
  - trunk surface torsion;
  - tangent change;
  - radius/taper change;
  - body/foot root-envelope change.
- Circular non-trunk branches are simplified deterministically:
  - branch-root transition rings are protected;
  - the final two tip rings are protected;
  - production minimum rings are `12 / 10 / 8` for primary / secondary / tertiary branches;
  - further samples are retained whenever position, radius, or tangent interpolation error exceeds the production threshold.
- The simplifier never changes the structural branch graph, control points, branch attachment data, or radial segment count.
- Bark longitudinal UV distance is retained from the complete pre-simplification curve and interpolated through inserted trunk rings, preventing texture tiling from stretching when axial rings are removed.
- Existing topology repair and the final bark topology audit remain unchanged and execute after adaptive simplification.

### Bounded validation policies

The existing geometry-efficiency audit remains exactly `20 × 3 = 60` cases but now isolates Patch 1:

1. **Legacy Current** — exact pre-Patch-1 axial behavior and the same radial counts.
2. **Current** — Patch 1 production behavior and the same radial counts.
3. **Axial Aggressive** — the same radial counts with `14` root-collapse intervals, `16°` torsion steps, and more permissive branch error thresholds. Diagnostic-only.

The Legacy Current policy must match every existing generated gallery mesh by vertex and triangle count. Current and Axial Aggressive are compared against the Legacy Current silhouette captured from the same serialized `GeneratedDefinition`.

### Acceptance gates

- Legacy Current existing-mesh count parity: `20 / 20`.
- Legacy Current topology: `20 / 20`.
- Patch 1 Current topology: `20 / 20`.
- Legacy Current and Patch 1 Current valid fixed-camera silhouettes: `20 / 20` each.
- No missing branches, collapsing tips, root-transition cracks, shading seams, or meaningful fixed-camera silhouette loss.
- Run the existing 256-case Root Height × Buttress Persistence suite after the twenty-tree audit. Start it from the selected Wych Elm `ProceduralTreeInstance` component context menu through `Run Patch 1 Root Regression Matrix`; progress, ETA, and cancellation appear conditionally in the Inspector while it runs. Do not accept any new topology regression or encode the irregular historical pass matrix into authoring constraints.
- Radial density remains unchanged and is not judged by this patch.

### Approved files

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeBarkMeshSettings.cs`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeGeometryEfficiencyAudit.cs`
- `Assets/Game/Procedural/Trees/Editor/ProceduralTreeInstanceEditor.cs`

### Invariants and non-goals

- No scene, prefab, material, recipe, serialized tree definition, generated mesh asset, layer, or tag is modified by the patch.
- The audit writes only beneath `Library/PS3D/Trees/GeometryEfficiencyAudit` and never commits candidate meshes.
- Traditional distance LOD remains excluded.
- Mixed-resolution root-to-circular-trunk radial stitching is Patch 2.
- Serialized-structure reduction and spatial streaming remain separate work items.
- Fresh deterministic structural-regeneration drift is still reported separately and is not used as the geometry tournament input.

### Implementation state

1. Preserve a diagnostic Legacy Current policy with exact pre-patch geometry. **Complete**
2. Confine dense root sampling to the root-lobe collapse boundary. **Complete**
3. Add adaptive persistence/trunk refinement and deterministic branch simplification. **Complete**
4. Preserve production radial counts and topology thresholds. **Complete**
5. Retarget the sixty-case audit to Legacy Current / Current / Axial Aggressive. **Complete**
6. Add separate adaptive-shape insertion telemetry and update CSV/report contracts. **Complete**
7. Static source and contract audit. **Complete — 40 / 40 checks passed**
8. Unity compilation and twenty-tree audit. **Complete — 60 / 60 topology cases passed; Production Current reduced vertices by 58.08% and triangles by 60.81% versus Legacy Current**
9. Operator fixed-camera comparison of the rendered twenty-tree gallery against the preserved pre-patch captures. **Complete — accepted as visually no worse and possibly improved**
10. Focused 256-case Root Height × Buttress Persistence regression suite. **Retained as follow-up root-topology evidence; not encoded into authoring constraints**


## TREE-GEOMETRY.2B — Production Acceptance and Diagnostic Cleanup

### Decision

Patch 1 axial containment is accepted as the production bark-mesh policy. `TreeBarkMeshEfficiencyPolicy.Current` remains the only policy selected by ordinary recipe-only and family-default production factories. Bark settings version `9` and bark algorithm version `23` remain unchanged because this closure does not modify runtime geometry.

### Accepted evidence

- Twenty procedural gallery trees were built under Legacy Current, Production Current, and Axial Aggressive from identical serialized structural definitions.
- All `60 / 60` builds passed the existing bark topology audit.
- Production Current reduced aggregate bark geometry from `506,879 / 916,305` vertices/triangles to `212,491 / 359,097`: `58.08%` fewer vertices and `60.81%` fewer triangles.
- Production Current reduced measured aggregate mesh-build time by approximately `73%` relative to Legacy Current in the submitted Editor audit.
- Structural branch population remained unchanged: `1,218` branches with stable IDs across all policies.
- The operator rendered Production Current into the actual twenty-tree gallery and compared it against preserved pre-patch captures at the real game view. The accepted result looked no worse than Legacy Current and in some cases appeared better.
- Radial density remains unchanged. Mixed-resolution root-to-circular-trunk radial ownership remains a separate Patch 2 investigation.

### Cleanup

- Removed the temporary `Render Patch 1 On Complete Gallery` Inspector action and its one-purpose incremental coordinator after the accepted gallery meshes were committed by the operator.
- Retained the reusable geometry-efficiency audit for Legacy/Production/Axial comparisons and the focused root-regression context-menu diagnostic.
- Removed no production mesh, topology, deterministic-generation, or validation code.
- No scene, prefab, material, recipe, serialized structure, generated mesh asset, layer, or tag is modified by this source cleanup.

### Next work items

1. Run the retained 256-case Root Height × Buttress Persistence regression when resuming the known non-monotonic root-topology defect.
2. Design Patch 2 mixed-resolution trunk radial ownership only after defining a topology-clean deterministic stitch between dense root contours and the ordinary circular upper trunk.
3. Keep serialized-structure reduction and spatial streaming/unloading as separate workstreams.
4. Do not introduce traditional distance-based tree LOD meshes.

## TREE-GEOMETRY.3 — Contour-Owned Radial Resolution Exploration

### Objective

Reduce the remaining bark geometry after accepted Patch 1 axial containment without changing the production policy until topology and visual evidence approve a radial candidate. Buttress Persistence remains authoritative: radial density follows the contour that actually exists at each ring rather than assuming that an upper trunk is circular because of height alone.

### Diagnostic policies

1. **Production Current** — accepted Patch 1 axial sampling with the existing uniform trunk radial count (`max(authored, Root Count × 10)`, capped at `64`) and existing branch-order radial counts.
2. **Radial Conservative** — Patch 1 axial rings plus contour-owned trunk radial tiers and conservative radius-aware branch sides.
3. **Radial Aggressive** — lower diagnostic radial targets establishing the visible faceting boundary; never selected by production factories.

`TreeBarkMeshSettings.CurrentSettingsVersion` remains `9` and `TreeBarkMeshGenerator.BarkAlgorithmVersion` remains `23` because ordinary production `Current` geometry and its production factories are unchanged. Promotion, if approved later, requires a separate closure decision.

### Buttress Persistence contract

- Each trunk ring measures the actual maximum root-only contour contribution.
- While that contribution exceeds the policy release threshold, the ring remains lobe-owned and uses a root-count-compatible radial tier.
- Lobe-owned tiers preserve integral samples per lobe. Conservative targets use up to `6 / 5 / 4 / 3` samples per lobe as contour amplitude weakens; Aggressive uses `5 / 4 / 3`.
- Resolution may decline only one approved tier per axial interval. It never jumps directly from a dense root contour to a cheap circular tube.
- If Persistence `1.000` keeps visible lobes near the trunk tip, those rings must remain lobe-owned. Circular resolution is permitted only after the authored root-only contribution becomes negligible.
- The contour formula, root count, phase, Root Reach, Root Thickness, Root Height, and Buttress Persistence are not altered to satisfy an optimization target.

### Mixed-resolution stitching

- Adjacent equal-resolution rings retain the existing quad triangulation path exactly.
- Adjacent lobe-owned rings with different root-compatible counts are stitched independently within each lobe sector using a deterministic angular zipper.
- After the contour is circular, a deterministic whole-ring angular zipper is used.
- Stitch triangles are passed through the existing outward-orientation selection and the complete production topology audit.
- Radial-transition count, mixed-resolution strips, stitch triangles, and minimum/maximum/average radial counts are recorded per branch and per tree.

### Radius-aware branch sides

- Patch 1 axial branch samples and structural branch IDs remain unchanged.
- Primary, secondary, and tertiary radial counts are resolved from branch maximum radius relative to trunk base radius, bounded by the authored branch-order count.
- Conservative targets are approximately `5–7 / 4–5 / 3–4`; Aggressive targets are lower and diagnostic-only.
- No branch is reduced below three sides, and no branch is removed.

### Audit and visual workflow

- `TreeGeometryEfficiencyAudit` remains `20 × 3 = 60` incremental cases and now compares Production Current / Radial Conservative / Radial Aggressive from identical serialized `GeneratedDefinition` inputs.
- The capture silhouette detector evaluates only the white candidate mask, preventing unrelated green preview vegetation from producing false border-clipping failures.
- Aggregate and per-branch CSVs include radial ranges, transitions, mixed-resolution strips, stitch triangles, and root-lobe/persistence/circular radial averages.
- During evaluation, a bounded gallery renderer committed Radial Conservative to all twenty existing serialized structures for direct review. TREE-GEOMETRY.3B removes that one-purpose renderer after acceptance.
- Ordinary production factories remain on Current until the operator accepts the rendered candidate.

### Root regression matrix

- The existing Wych root suite is expanded to compare Production Current and Radial Conservative.
- Persistences are `0.000–0.700` in `0.100` steps plus `1.000`.
- Eight Root Heights and four Reach/Thickness profiles produce `576` incremental cases.
- Acceptance rule: every case that passes Production Current must also pass Radial Conservative. Candidate repairs are allowed; candidate-introduced failures are not.
- The suite remains Editor-responsive, cancellable, checkpointed, and reports the regression gate explicitly.

### Approved source scope

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeBarkMeshSettings.cs`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeBarkMeshAssetBuilder.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeGeometryEfficiencyAudit.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeWychRootHeightMinimumSweep.cs`
- `Assets/Game/Procedural/Trees/Editor/ProceduralTreeInstanceEditor.cs`

### Invariants

- No scene, prefab, material, recipe, serialized structural definition, layer, tag, or shader is modified by the source patch.
- Candidate gallery rendering changes only the managed generated bark mesh assets and can restore Production Current from the same serialized structures.
- Traditional distance-based LOD remains excluded.
- Serialized-structure reduction and spatial streaming/unloading remain separate workstreams.


## TREE-GEOMETRY.3B — Production Radial Promotion and Diagnostic Cleanup

### Decision

Contour-owned radial resolution is accepted as the production bark representation. Ordinary production `TreeBarkMeshEfficiencyPolicy.Current` now combines the accepted Patch 1 adaptive axial sampling with the accepted Radial Conservative trunk and branch radial behavior.

### Accepted evidence

- Production Current mesh parity before promotion: `20 / 20`.
- Production Current, Radial Conservative, and Radial Aggressive topology audit: `60 / 60` passed.
- Radial Conservative reduced the accepted Patch 1 gallery from `212,491 / 359,097` vertices/triangles to `130,601 / 205,229`: `38.54%` fewer vertices and `42.85%` fewer triangles.
- Combined versus Legacy Current, the accepted production representation reduces `506,879 / 916,305` to `130,601 / 205,229`: `74.23%` fewer vertices and `77.60%` fewer triangles.
- The operator rebuilt and reviewed the actual twenty-tree gallery from game-like camera distances and angles and found no meaningful visual degradation.
- All `20 / 20` bark-only candidate gallery commits passed topology.
- The `576 / 576` Current-versus-Conservative root regression matrix completed. All `288` Production Current passing cases also passed Radial Conservative, with `0` new candidate failures. Persistence `1.000` passed all Root Heights and all four reach/thickness profiles.

### Production changes

- `TreeBarkMeshSettings.CurrentSettingsVersion`: `9 → 10`.
- `TreeBarkMeshGenerator.BarkAlgorithmVersion`: `23 → 24`.
- `Current` now enables contour-owned trunk radial resolution and radius-aware branch radial resolution using the accepted conservative thresholds.
- Buttress Persistence remains authoritative. Persistent lobes remain root-count-compatible through the tip when Persistence `1.000` keeps the authored contour lobed.
- `RadialConservative` remains only as a compatibility/diagnostic alias of the accepted production radial behavior.
- `RadialAggressive` remains diagnostic-only.

### Cleanup

- Removed the temporary `Render Radial Conservative` and `Restore Production Current` Inspector controls and their incremental gallery-render coordinator.
- Removed the completed `Run Radial Root Regression Matrix` control and its one-purpose `TreeWychRootHeightMinimumSweep` implementation.
- Removed the temporary policy-override mesh-asset commit API from `TreeBarkMeshAssetBuilder`; ordinary production mesh generation now supplies the accepted result directly through `Current`.
- Retained the reusable geometry-efficiency audit, retargeted to Production Current / Legacy Pre-Patch-1 / Radial Aggressive.
- No scene, prefab, material, recipe, serialized tree definition, generated mesh asset, shader, layer, tag, or project setting is modified by this source closure. The already-reviewed gallery meshes remain untouched.

### Next work items

1. Audit bark mesh index format and vertex channels against actual shader requirements.
2. Repair deterministic structural regeneration before reducing serialized `TreeDefinition` retention.
3. Implement spatial streaming, culling, and unloading for world-scale tree populations.
4. Do not introduce traditional distance-based tree LOD meshes.

## TREE-ROOTS.1 — Root Control and Shape Evaluation

### Objective

Establish direct visual evidence for the production root-control contracts before changing the root generator or tuning curated recipe values against imported references. This stage is diagnostic-only: it creates temporary validation definitions, temporary bark meshes, and isolated captures without modifying recipes, exact-control snapshots, gallery meshes, scenes, prefabs, materials, or serialized tree structures.

### Representative set

The evaluation resolves four current curated gallery representatives by exact recipe identity and stable slot:

1. Alder Standard — Common.
2. Norway Spruce Standard — Pine.
3. Wych Elm Leaning — Twisted.
4. Dead Alder — Dead.

Each representative uses its current serialized exact-control snapshot, master seed, family, transform rotation, and scale. Validation regeneration is deterministic and unsaved.

### Case matrix

Each representative receives nine cases:

1. Baseline.
2. Root Thickness `0.10`.
3. Root Thickness `1.00`.
4. Root Reach `0.00`.
5. Root Reach `2.00`.
6. Root Height `0.01`.
7. Root Height `0.40`.
8. Buttress Persistence `0.00`.
9. Buttress Persistence `1.00`.

Wych Elm additionally receives:

- Root Thickness `1.00` + Root Reach `2.00`.
- Root Thickness `1.00` + Buttress Persistence `1.00`.
- Root Reach `2.00` + Root Height `0.01`.

Total: `39` cases and `78` captures.

### Capture contract

- Every successful case receives a neutral lit close-root three-quarter capture over a ground plane.
- Every successful case also receives an isolated full-tree capture using the enabled Main Camera's exact rotation and projection at the selected tree's current reference distance.
- Captures use isolated preview Scenes, `Camera.Render`, and polled `AsyncGPUReadback`; no synchronous GPU readback or blocking wait is permitted.
- The workflow advances incrementally across Editor updates, displays progress and ETA, supports cancellation, and checkpoints Markdown, CSV, HTML, and completed PNGs after each case.

### Superseded historical Inspector path

The following temporary diagnostic path is retained only as implementation history and is no longer present in the current Inspector:

`Procedural Tree Instance Inspector → Root Quality Evaluation → Run 39-Case Root Quality Board`

Output:

- `Library/PS3D/Trees/RootQualityEvaluation/TreeRootQualityEvaluationBoard.html`
- `Library/PS3D/Trees/RootQualityEvaluation/TreeRootQualityEvaluationReport.md`
- `Library/PS3D/Trees/RootQualityEvaluation/TreeRootQualityEvaluation.csv`
- `Library/PS3D/Trees/RootQualityEvaluation/Captures`

### Evaluation questions

- Root Thickness must visibly alter each buttress lobe's mass without uniformly inflating the circular trunk core or acting as Root Reach.
- Root Reach must alter horizontal footprint without materially changing angular thickness, Root Height, or persistence.
- Root Height must alter the vertical ground-root envelope without changing ground reach or thickness.
- Buttress Persistence must control how far the lobed contour survives up the trunk while preserving the ground footprint and authored Root Height.
- Root tips must remain rounded rather than flat, needle-like, or triangular.
- Buttress shoulders must blend into the trunk without a hard ninety-degree ledge or a wide melted pedestal.
- Common, Pine, Twisted, and Dead roots must remain family-appropriate while sharing the same dependable control semantics.

### Closure rule

Do not tune curated recipe root ranges against imported trees until the operator reviews the board and the generator-level root defects are corrected. Recipe tuning must not compensate for weak, coupled, or misleading root controls.

## TREE-ROOTS.2 — Continuous Root-to-Trunk Twist

### Status

**Accepted for production on 2026-07-30. Implementation, Unity execution, focused visual review, and twenty-tree topology/performance validation are complete. Persistent curated-gallery/library regeneration is the immediate next operation; broad control-bound safety testing follows only after that regeneration.**

### Objective

Replace the recipe-only delayed bark-roll distribution with one authoritative continuous base-to-tip roll field so root bodies, buttress crests, persistence-owned lobes, and the upper trunk share one uninterrupted axial phase. Keep the ground ring anchored at zero roll, preserve the existing root-frame stabilization and structural branch placement, and make generation, axial refinement, diagnostics, and validation consume the same phase equation.

### Acceptance criteria

1. Recipe-only axial twist begins continuously above normalized trunk distance `0.000`, with the exact ground ring remaining at zero roll.
2. No phase reset or angular handoff occurs at the ground plateau, root-lobe collapse, earliest root transition, persistence transition, circular release, or trunk tip.
3. Twist `0` preserves current geometry behavior.
4. Positive and negative axial twist produce coherent opposite-handed root-to-tip spirals.
5. Persistence `0.000`, `0.500`, and `1.000` remain topology-safe; Persistence `1.000` stays lobe-owned wherever the authored contour remains visible.
6. The maximum authored adjacent-ring roll is measured directly and remains within the active production twist-step threshold.
7. Requested versus measured total axial twist remains within the existing two-degree build gate.
8. The ordinary twenty-tree geometry audit remains topology-complete with no material vertex or triangle inflation.
9. No scene, prefab, recipe, material, exact-control snapshot, generated gallery mesh, layer, tag, or runtime component is modified by the focused evaluation workflow.
10. Active-gameplay CPU, GPU, draw-call, vertex-format, and persistent-memory costs remain unchanged.

### Approved files

Modify only:

- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeGeometryEfficiencyAudit.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeGalleryGenerationCoordinator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeRootQualityEvaluation.cs`
- `Assets/Game/Procedural/Trees/Editor/ProceduralTreeInstanceEditor.cs`
- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`

No file creation, deletion, move, scene edit, prefab edit, recipe edit, material edit, layer edit, or tag edit is approved.

### Reviewed evidence

- Current production versions are bark algorithm `24`, bark settings `10`, and structural generator `6`.
- Recipe-only structural generation passes zero torsion into structural curve sampling and applies authored `TrunkSurfaceTorsionDegrees` later in bark-frame resolution; legacy generation embeds torsion in transported structural samples.
- Current recipe-only bark-frame resolution maps axial roll from `CalculateEarliestRootTransitionHeight(...)` to `1.000`, producing zero authored bark roll throughout the lower root domain and compressing the full requested twist into the remaining upper trunk.
- Root-lobe phase is stable per branch and does not vary with height; world-space helical progression is therefore owned by the per-ring surface frame and must not be duplicated in the root-mask phase.
- The current adaptive twist-refinement equation already assumes a linear `0.000 → 1.000` roll distribution, while current geometry delays that roll. Refinement and geometry therefore disagree.
- The root-ring correspondence diagnostic uses `CalculateEffectiveRootTransitionHeight(...)` for roll reporting while production geometry uses the earliest transition, so the diagnostic does not describe the emitted roll field.
- Existing total-twist validation can pass despite the distribution defect because it checks only final accumulated twist.
- Existing gallery validation divides total requested twist by total trunk intervals; this is an average, not the actual maximum local roll step.
- Historical root-transition work intentionally delayed roll to avoid unsafe strips. Continuous roll must therefore retain topology auditing and direct local-step telemetry rather than relying on visual evidence alone.

### Invariants and non-goals

- Preserve the stabilized recipe-only root tangent/normal frame and apply authored roll after that no-roll frame is resolved.
- Preserve fixed local root-lobe identity; do not add height-varying roll to `ResolveRootPhase(...)` or `EvaluateButtressMasks(...)`.
- Preserve structural branch attachment azimuths and all `TreeGenerator` output.
- Preserve accepted axial/radial sampling thresholds, contour-owned radial tiers, mixed-resolution stitching, UV side ordering, topology rules, and root-control meanings.
- Do not add a Root Twist control or a second root-foot phase envelope.
- Do not switch twist semantics from normalized curve distance to normalized arc length in this update.
- Do not perform recipe tuning, root-shape tuning, additional geometry reduction, foliage work, normals redesign, streaming work, or distance-based LOD work.

### File-by-file sequence

1. **Canonical plan — complete.** The approved contract, reviewed evidence, risks, and validation gates were recorded before code edits.
2. **Bark generator — implemented and accepted.** One authoritative normalized-distance roll function now drives recipe-only surface-frame rotation, adaptive twist subdivision, correspondence diagnostics, phase telemetry, and terminal-tip roll completion. The stabilized no-roll root frame remains separate and root-mask phase remains unchanged. Total-twist measurement references the exact no-roll surface frame for recipe-only trees while preserving the prior legacy measurement path. Bark algorithm version is `25`; bark settings remain `10`; structural generator remains `6`.
3. **Geometry audit — implemented and passed.** The completed audit reported `60 / 60` policy cases and `20 / 20` Production Current topology passes. Production Current measured `131252` vertices and `206391` triangles, a `651`-vertex (`0.50%`) and `1162`-triangle (`0.57%`) increase over the immediately preceding accepted aggregate; this is accepted as non-material. All requested/measured production twist errors rounded to `0.000`, and the maximum adjacent-ring authored roll was `8.982°` against the `10.000°` limit.
4. **Gallery coordinator — implemented and audit-validated.** The invalid total-twist/ring-count average gate was replaced by the build result’s actual maximum authored adjacent-ring roll and active-policy limit.
5. **Focused evaluation — complete; behavior accepted.** The evaluator completed all eight Wych Elm cases. Twist `0`, authored twist, `+460°`, `-460°`, and `+460°` with Persistence `0.000`, `0.500`, and `1.000` all passed and produced the required captures. The operator accepted the continuous base-to-tip behavior as visually compelling and production-worthy. The deliberate Cartesian-extreme stress case (`Twist 460°`, Root Thickness `1.000`, Root Reach `2.000`, Root Height `0.010`, Persistence `1.000`) exposed nine non-terminal unsafe strips; this is recorded as future control-bound evidence, not a rejection of the accepted production behavior or a precondition for library regeneration.
6. **Inspector — implemented and exercised.** The existing Root Quality Evaluation section exposes `Run Continuous Twist Board`, with matching cancel/open/copy/folder controls.
7. **Static and consistency audit — complete.** The final review surface was reread; the worktree differs from the supplied archive in exactly the six approved files; all five modified C# files passed lexical structure checks; CSV header/value parity is `35 / 35` for the focused board and `94 / 94` for the geometry aggregate; `48 / 48` static compliance checks passed; no delayed root-height roll equation or deprecated sorted object search remains; no scene, prefab, recipe, material, layer, tag, or generated-asset write was added.
8. **Unity validation — complete for behavior acceptance.** The focused board and ordinary twenty-tree geometry audit completed. Continuous twist is accepted for production. Full persistent curated-gallery/library regeneration is now required before any broad control-bound audit or recipe/root-shape tuning continues.

### Affected modules

- Recipe-only bark surface-frame orientation.
- Trunk adaptive axial refinement.
- Axial-twist build telemetry and total-twist measurement.
- Root-ring correspondence diagnostics.
- Geometry-efficiency aggregate reporting.
- Gallery validation gates.
- Temporary root visual-evaluation workflow and Inspector UI.

### Risks and mitigations

- **Root-strip inversion:** historical delayed-roll policy avoided unsafe root strips. Mitigation: preserve dense root sampling, use the existing twist-step threshold, report the actual adjacent-ring authored phase delta, and require complete topology audits.
- **Double twist:** adding roll to both the surface frame and root-mask phase would apply the same rotation twice. Mitigation: leave root-mask phase unchanged.
- **Root-foot orbiting:** a separate foot phase could detach ground contact. Mitigation: keep the ground ring at exact zero roll and add no foot-specific attenuation unless later evidence proves it necessary.
- **Measurement contamination:** transported-frame or root-frame adoption can be mistaken for axial roll. Mitigation: measure emitted radial orientation against the exact no-roll surface frame.
- **Diagnostic drift:** independent roll formulas can diverge again. Mitigation: centralize the expected roll calculation and consume it in geometry, refinement, correspondence reporting, and telemetry.
- **Geometry inflation:** topology repair could insert or preserve more rings. Mitigation: compare twenty-tree ring, vertex, triangle, and build-time results against the accepted production baseline.

### Validation and compliance status

- Repository instructions: reviewed.
- Current source and direct bark-generation consumers: reviewed.
- Historical root-transition and geometry-optimization records: reviewed.
- Implementation: complete and accepted.
- Static/API audit: `48 / 48` passed.
- Unity execution: completed by the operator; no compile blocker was reported.
- Focused visual board: complete; seven production-relevant cases passed and were visually accepted. One deliberate all-extremes stress case failed topology and is deferred to the later systematic bounds program.
- Twenty-tree topology/performance audit: complete; Production Current `20 / 20`, all policies `60 / 60`, maximum adjacent-ring roll `8.982° / 10.000°`.
- Persistent curated-gallery/library regeneration: pending and required immediately after applying the accepted bark algorithm.
- Broad control-bound safety testing: explicitly deferred until the persistent library regeneration completes.

### Implementation result

- Recipe-only authored roll is now `TrunkSurfaceTorsionDegrees × Clamp01(normalizedDistance)`. The exact ground ring remains at zero phase and every elevated ring participates continuously.
- Root-frame stabilization still resolves the tangent and no-roll normal first. Authored roll is applied afterward. Structural branch azimuths, generated tree definitions, and legacy/non-recipe structural twist remain unchanged.
- `ResolveRootPhase(...)` and buttress-mask phase were not modified, preventing double application of the same axial rotation.
- Adaptive subdivision now measures the difference between the shared roll function at each span endpoint rather than using an independent approximation.
- The root-ring correspondence diagnostic now reports the same roll field emitted by production geometry instead of an unrelated persistence-derived transition.
- Build telemetry records first nonzero emitted roll distance, phase at the ground plateau, root-collapse end, earliest transition, and effective transition, plus the maximum adjacent-ring authored phase step and its interval.
- Bark generation fails before mesh emission if the actual emitted-ring sampling exceeds the active policy’s authored twist-step threshold.
- The gallery gate consumes the direct adjacent-ring metric rather than dividing total twist by ring count.
- The focused board reports the same distribution telemetry and retains topology auditing before either capture is attempted.
- No settings schema, structural generator version, author-facing control, runtime component, vertex channel, draw call, persistent buffer, recipe, scene, prefab, material, layer, or tag was changed.

### Actual changed-file reconciliation

- Modified: exactly the six approved files.
- Created: none.
- Deleted: none.
- Moved or renamed: none.
- Unapproved or unrelated differences versus the supplied archive: none.

### Static audit evidence

- Complete final review surface reread: passed.
- Approved scope comparison: passed.
- C# lexical delimiter/string/comment checks: `5 / 5` passed.
- Focused-board CSV header/value parity: `35 / 35`.
- Geometry aggregate CSV header/value parity: `94 / 94`.
- Static policy, version, API, symbol, UI-label, prohibited-write, and centralized-roll checks: `48 / 48` passed.
- Bark settings version: `10`, unchanged.
- Structural generator version: `6`, unchanged.
- Bark algorithm version: `25`.
- Unity compilation: unavailable in the archive-only environment and not represented as passed.

### Current verified Inspector path

`Procedural Tree Instance Inspector → Root Quality Evaluation → Run Continuous Twist Board`

### Acceptance evidence

1. The focused evaluator completed `8 / 8` scheduled cases. Seven production-relevant cases passed topology and produced both close-root and exact game-camera captures.
2. The operator accepted the continuous root-to-trunk visual behavior and explicitly approved production promotion before further bounds work.
3. Twist `0`, authored twist, `+460°`, and `-460°` preserved identical `12404 / 18978` geometry counts in the focused representative; handedness reversed correctly and requested/measured twist matched.
4. Persistence `0.000`, `0.500`, and `1.000` passed the focused topology and capture workflow.
5. The ordinary geometry audit completed `60 / 60` cases with Production Current topology `20 / 20`, generated-mesh parity `20 / 20`, and maximum production adjacent-ring roll `8.982° / 10.000°`.
6. The all-extremes stress failure is retained as a bounds-audit seed. It does not override behavior acceptance because safe Cartesian bounds have not yet been established for all tree families and all controls.

### Immediate production regeneration

After applying bark algorithm `25`, run the existing incremental curated-gallery rebuild. Its checkpoint compatibility gate requires the current bark algorithm version, so persistent bark checkpoints generated with algorithm `24` are rejected and rebuilt. The operation regenerates all twenty stable curated slots, re-samples their exact controls, re-validates deterministic structure, rebuilds persistent bark mesh subassets, saves each completed checkpoint, and produces a resumable report.

Do not begin systematic control-bound sweeps, change advertised limits, add dynamic clamping, or tune recipes against the new shape until the complete persistent regeneration reports `20 / 20` successful slots.

## TREE-SPIRAL.1 — Height-Preserving Signed Path Spiral

### Status

**IMPLEMENTED / UNITY VALIDATION PENDING**

### Objective

Correct recipe-only Signed Path Spiral so increasing signed revolutions changes the horizontal trunk centreline without increasing, reducing, or otherwise remapping the authored vertical height. The full public `-3.0 ... +3.0` revolution interval must remain deterministic and structurally finite at the full `0.0 ... 0.5 × Height` Path Spiral Radius interval.

### Approved files

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeGenerator.cs`
- `Assets/Game/Procedural/Trees/TreeControlDescriptorRegistry.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeControlResponseSuite.cs`

### Reviewed evidence

- Recipe-only trunk construction emits twelve stable normalized non-spiral shape anchors, then currently adds the path spiral directly to those sparse anchors.
- The shared polyline turn constraint reconstructs constrained points from the raw three-dimensional segment length. When a high-radius spiral is constrained toward the prior direction, horizontal arc length is converted into forward/vertical displacement and the trunk exceeds the authored height.
- The recipe-only control contract advertises Signed Path Spiral Turns from `-3` to `+3` and Path Spiral Radius from `0` to `0.5 × Height`.
- The recipe-only runtime policy allows at most `64` trunk samples and a `38°` maximum adjacent trunk-segment turn.
- Static stress reconstruction showed that dense analytic points plus uniform horizontal scaling are not sufficient when high Bend Frequency, Drift, Roughness, Lean, Path Spiral Radius, and Signed Path Spiral Turns interact: the unconstrained sampled curve can still exceed the `38°` local-turn contract. The active path therefore requires a fixed-height sampled-turn limiter rather than the legacy three-dimensional length-preserving limiter.
- Persistent curated-gallery checkpoint reuse requires the stored structural generator version to equal the current generator version. This structural correction therefore requires a generator version increment so existing persistent definitions cannot be reused silently.

### Acceptance criteria

1. The recipe-only trunk centreline begins at `y = 0` and ends at `y = authored Height` for every active path-spiral value.
2. Every emitted recipe-only path-spiral sample follows `y = authored Height × normalizedDistance` within floating-point tolerance; Signed Path Spiral cannot change vertical progression.
3. The full signed-turn and radius control intervals generate finite, monotonically rising trunk samples.
4. Adjacent emitted trunk tangent turn remains at or below the active `38°` recipe-only policy limit.
5. Positive and negative turns remain deterministic opposite-handed structural responses.
6. Zero Path Spiral Radius preserves the existing non-spiral generation path. Zero Signed Path Spiral Turns with nonzero radius remains a valid non-revolving radial centreline sweep and must still preserve authored height.
7. Branch attachment, root, foliage, and bark systems continue to consume the corrected trunk samples without new runtime work, per-frame state, or author-facing controls.
8. Current structural generator version increments from `6` to `7`; seed compatibility remains `3`; bark algorithm remains `25`; bark settings remain `10`.
9. Persistent gallery regeneration is mandatory because all version-6 structural checkpoints become incompatible.

### Invariants and non-goals

- Preserve the original twelve deterministic non-spiral shape anchors and their random keys.
- Densify only the recipe-only centreline representation required to resolve the authored spiral; do not resample random shape noise.
- Preserve exact authored vertical progression after interpolation.
- Do not alter legacy/profile-driven generation.
- Do not alter Axial Twist, continuous root-to-trunk bark roll, root geometry, branch algorithms, recipe assets, scenes, prefabs, materials, layers, or tags.
- Do not clamp the public signed-turn range to hide the defect.
- Do not add a permanent diagnostic action or parallel validation suite.

### Implementation sequence

1. Split recipe-only trunk anchor construction into stable non-spiral anchors plus an analytically evaluated path-spiral offset.
2. Determine a deterministic turn-aware centreline point count from authored revolutions, the existing trunk turn limit, and the existing per-branch sample budget.
3. Evaluate the non-spiral anchor curve at that point count, add the analytic spiral at each normalized position, and force exact authored `y` at every point.
4. Bypass the three-dimensional polyline reconstruction only for active recipe-only path spirals; preserve each authored vertical coordinate and enforce horizontal safety with one deterministic uniform horizontal scale across the complete centreline rather than pointwise projection that can create clamp-boundary kinks.
5. Force exact linear vertical sampling in the active recipe-only path-spiral `BuildCurveSamples` call, then constrain sampled segment direction with exact fixed `y` increments at `0.75 ×` the public local-turn limit so central-difference sample tangents retain safety margin. Reapply one uniform horizontal envelope scale after that constraint.
6. Add generation-time structural validation for endpoint height, normalized vertical progression, monotonicity, finiteness, and maximum adjacent tangent turn.
7. Extend the existing exhaustive control suite so Path Spiral Radius and Signed Path Spiral Turns exercise their combined maximum interaction and report trunk tip height, sample count, and maximum trunk turn.
8. Update the control description and canonical status.
9. Perform final scope, version, deterministic-key, API, CSV-schema, lexical, and source-diff audits. Unity compilation, full control-suite execution, gallery regeneration, and visual review remain operator validation gates.

### Risks and mitigations

- **Sparse-turn aliasing:** twelve anchors cannot represent three complete revolutions cleanly. Mitigation: preserve the twelve non-spiral random anchors, then add the analytic spiral on a deterministic turn-aware dense centreline.
- **Clamp-boundary tangent spikes:** pointwise radial or elliptical projection can preserve height but create abrupt local direction changes where only some points touch the safety envelope. Mitigation: resolve one uniform horizontal scale from all active-path points and all applicable horizontal envelopes, then apply that scale to the complete centreline before sampling.
- **Combined-shape local turns:** high non-spiral bend, drift, roughness, and lean can add local curvature on top of the analytic spiral even after turn-aware densification. Mitigation: constrain the final sampled path sequentially with fixed authored `y` increments and a `0.75 ×` internal segment-turn target, then validate the resulting central-difference tangents against the full public `38°` limit.
- **Structural reroll:** changing trunk samples changes branch attachment frames and fingerprints. Mitigation: increment the structural generator version while preserving seed compatibility and stable random keys.
- **Sample-budget exhaustion:** the public extreme must fit the existing sample budget. Mitigation: derive density from half the active turn limit and cap it at the existing `64`-sample budget; generation validation rejects any unresolved over-limit result rather than silently deforming it.
- **Legacy regression:** the shared turn constraint also serves branches and legacy trees. Mitigation: leave that shared function unchanged and select the height-preserving path only for active recipe-only path spirals.
- **Diagnostic drift:** a visual response alone would not prove height preservation. Mitigation: enforce the contract in production structure validation and expose direct centreline metrics in the existing control suite.

### Implementation result

- `TreeGenerator.CurrentGeneratorVersion` is `7`; seed compatibility remains `3`.
- Active recipe-only Path Spiral preserves exact authored vertical control-point and sample progression from `0` to `Height`.
- The original twelve deterministic non-spiral shape anchors and their random keys remain authoritative; an analytic spiral is evaluated on a deterministic turn-aware dense centreline.
- Active-path horizontal safety uses one uniform scale across all points and both the runtime displacement envelope and any active reference-calibration ellipse.
- Final active-path samples use a fixed-height local-turn limiter at `0.75 ×` the production segment-turn policy, then reapply the uniform horizontal envelope before frames and branch attachments are derived.
- The legacy/profile trunk path, shared branch turn constraint, Axial Twist, bark algorithm `25`, bark settings `10`, recipe assets, scenes, prefabs, materials, layers, and tags are unchanged.
- The existing control-response suite now reports emitted trunk controls, sample count, tip height, and maximum adjacent sample-tangent turn; Path Spiral controls exercise the full `0.50 × Height` / `±3` interaction.

### Validation status

- Review gate: complete.
- Canonical plan: complete.
- Implementation: complete.
- Static/source audit: `34 / 34` passed.
- Mathematical reconstruction: authored height remained exact at the tested signed-turn values; the old sparse three-dimensional constraint reproduced height growth at high turns.
- Deterministic static stress reconstruction: `5,000` all-trunk-control-maximum seeds produced a maximum reconstructed adjacent sample-tangent turn of `35.478°`, below the public `38°` gate. This is supporting static evidence, not Unity execution.
- Unity compilation: pending.
- Exhaustive control response suite: pending.
- Persistent curated-gallery regeneration: pending until source validation passes.
- Visual confirmation at signed turns `1.5`, `3.0`, `-1.5`, and `-3.0`: pending.


## TREE-ROOTS.3 — Twisted Buttress Body and Ground-Anchored Root Foot

### Status

**Source implementation and static audit complete. Unity compilation, focused visual review, and production topology validation are pending.**

### Objective

Preserve the accepted continuous base-to-tip axial bark roll for the proximal buttress body while preventing the distal root foot from orbiting around the trunk. Recipe-only roots must retain a stable ground-space outward direction through the foot-collapse domain so large Root Reach and Root Thickness values produce a continuous outward-to-ground silhouette rather than an airborne hook that curls back toward the trunk.

### Acceptance criteria

1. The exact ground ring remains unchanged in reach and phase: Root Reach continues to resolve as the full ground-level crest extension.
2. The proximal buttress body continues to follow the accepted continuous axial twist field.
3. The distal foot retains the base root sector's ground-space direction and releases continuously into the twisted body as the foot envelope collapses.
4. Positive and negative axial twist retain opposite handedness without reversing or orbiting the distal foot.
5. Root Thickness broadens the root support while retaining a rounded/tapered crest; the high-thickness profile must not become a flat sector-wide block.
6. Root Count, Root Reach, Root Thickness, Root Height, and Buttress Persistence remain the only author-facing root controls; no hidden clamp, terrain query, runtime dependency, or new control is introduced.
7. Legacy/profile-driven root generation remains unchanged.
8. Existing contour-owned radial resolution, mixed-resolution stitching, UV side ordering, normals/tangents derived from final geometry, topology gates, and production geometry budgets remain active.
9. The focused evaluation uses temporary unsaved structures and meshes, advances incrementally, remains cancellable, and does not alter recipes, exact controls, gallery meshes, scenes, prefabs, materials, layers, or tags.
10. Active-gameplay CPU, GPU, draw-call, vertex-format, and persistent-state costs remain unchanged.

### Approved files

Modify only:

- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeRootQualityEvaluation.cs`
- `Assets/Game/Procedural/Trees/Editor/ProceduralTreeInstanceEditor.cs`
- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`

No file creation, deletion, move, scene edit, prefab edit, recipe edit, material edit, layer edit, tag edit, settings-schema change, or structural-generator change is approved.

### Reviewed evidence

- Pre-edit recipe-only root geometry was emitted as one rotated radial surface: `sample centre + twisted radial × sample radius × (1 + body contribution + foot contribution)`.
- Pre-edit body and foot amplitudes were separate scalars, but both used the same continuously twisting radial direction. The distal foot therefore orbited with the trunk and could retract toward the body instead of preserving its base-sector ground direction.
- The exact ground ring already has zero authored roll and a stabilized world-up/right root frame. It is therefore a deterministic anchor for every root sector and preserves the current ground footprint without a new control.
- The recipe-only foot envelope is already strongest at the ground and collapses before the body/persistence envelope. It can drive a continuous anchored-foot-to-twisted-body release without changing Root Height or Buttress Persistence semantics.
- Pre-edit high Root Thickness reduced the root-mask profile power from `4` toward `2`. At fixed emitted support this reduces off-centre mask mass for `0 < q < 1`; after Root Count sector saturation, increasing Thickness can therefore stop widening support and make the half-mass root profile narrower. Width broadening already comes from requested angular support.
- Final trunk normals and tangents are derived from neighbouring final surface positions. A spatial foot correction in the shared surface evaluator therefore remains authoritative for positions, normals, tangents, topology preflight, caps, captures, and production mesh output.

### Invariants and non-goals

- Preserve `TREE-ROOTS.2` continuous axial roll and do not add a second height-varying root phase.
- Preserve Root Reach as exact ground-level radial projection and Root Thickness as angular breadth.
- Preserve the fixed local root-lobe identity and the existing root body/foot amplitude split.
- Anchor only the recipe-only distal foot; do not change legacy roots.
- Use the tree-local base plane and deterministic base root frame; do not query terrain.
- Do not add separate root meshes, bones, per-root runtime objects, per-frame deformation, or author-facing grounding controls.
- Do not alter Signed Path Spiral, structural branches, foliage, recipes, shaders, materials, scenes, prefabs, layers, tags, or geometry-efficiency policy thresholds.
- Do not perform broad control-bound closure in this patch. The focused board validates representative production and high-root profiles; full cross-family bounds work remains later.

### File-by-file implementation sequence

1. **Canonical plan — complete.** Record the approved architecture, evidence, invariants, risks, and validation contract before code changes.
2. **Bark surface model — source complete; Unity validation pending.** Recipe-only body/base displacement remains in the fully twisted surface frame. The foot contribution uses the deterministic base-plane root direction and receives only the authored roll released by `1 - footEnvelope`; it is fully anchored at the ground/plateau and exactly rejoins the twisted body as the foot contribution reaches zero.
3. **Rounded thickness profile — source complete; Unity validation pending.** Thickness `0.5` remains the exact `q^4` profile. The high-thickness exponent now interpolates from `4` toward `2`, and the width diagnostic uses the same profile, so width broadening remains support-owned rather than becoming a flat crest.
4. **Version and deterministic rebuild — complete.** Bark algorithm `25` became `26`. Bark settings remain `10`; structural generator remains `7`.
5. **Focused evaluation — source complete; execution pending.** The temporary eight-case board now covers the authored profile, the operator's `400° / 1.2 Reach / 0.9 Thickness / 0.2874 Height / 0.5 Persistence` profile at both zero and authored twist, isolated Reach and Thickness stress, combined positive/negative twist stress, and Persistence `0`/`1`. The former low-Height bounds case is intentionally excluded because cross-family bounds work is deferred.
6. **Inspector — source complete; execution pending.** The existing Root Quality Evaluation section now exposes the grounded-foot run/cancel/report/board/folder actions without adding another diagnostic section.
7. **Audit — static phase complete; Unity phase pending.** The final diff is confined to the four approved files. Static scope, version, lexical, API, CSV-schema, neutral-profile, endpoint-identity, and representative vector-cancellation checks pass. Unity compilation, the focused board, and the normal production geometry/topology audit remain operator gates.

### Implemented source delta

- Recipe-only final root displacement is now the vector sum of a fully twisted body/base offset and a separately directed foot extension. The foot direction is defined in the exact deterministic base root frame and releases continuously into the authored twist as the existing foot envelope collapses.
- Ground identity is preserved algebraically: at normalized distance `0`, authored roll is `0` and the body and foot directions are identical, so the complete ground footprint and Root Reach magnitude remain unchanged.
- Foot-collapse identity is preserved algebraically: when the foot contribution reaches `0`, the emitted position exactly reduces to the accepted continuously twisted body surface.
- Twist-`0` recipe trees remain position-equivalent throughout the foot domain because the stabilized root frame and anchored foot frame coincide there.
- Legacy/profile-driven roots retain the prior scalar cross-section path and do not enter the grounded-foot branch.
- The root-mask profile and its half-extension diagnostic now share the same high-thickness exponent interpolation from `4` toward `2`; thickness `0.5` remains exact.
- The evaluator output and UI are retargeted rather than duplicated. It still uses temporary hidden preview objects, asynchronous capture readback, incremental progress, cancellation, and checkpointed output.

### Static audit evidence

- Approved-file scope: `4 / 4`, with no creation, deletion, move, or serialized-asset edit.
- Source and mathematical reconstruction checks: `33 / 33` passed.
- Focused CSV schema: `35 / 35` columns aligned.
- Representative approved profiles (`400° / Reach 1.2 / Thickness 0.9` and `±460° / Reach 2 / Thickness 1`) never reduced the combined offset below `1.000 ×` the local trunk radius during the tested root domain; no body/foot vector cancellation below the base surface was found.
- Maximum body-to-foot angular separation in those profiles was `26.400°`; the direction release remains bounded before the foot contribution vanishes.
- Twenty-one straight-trunk contour reconstructions covering all focused non-authored profiles at `30`, `36`, and `60` radial samples produced no ring self-intersection; every reconstructed longitudinal quad retained a non-zero triangulation, with minimum best-diagonal triangle area `0.00131951` in normalized-radius units.
- Unity compilation and live generated topology are unavailable in this environment and remain explicitly pending.

### Risks and mitigations

- **Foot/body kink:** independently directed displacement can create a discontinuity. Mitigation: keep the ground ring identical and multiply the anchoring influence by the existing smooth foot envelope so it reaches zero before the foot contribution disappears.
- **Contour inversion:** strong twist plus large Reach can cross adjacent strips. Mitigation: all topology preflight and final topology audit paths consume the same final surface evaluator; no topology threshold is weakened.
- **Ground-foot shrinkage:** vector addition can reduce upper-foot radial magnitude under twist. Mitigation: Root Reach remains exact at the ground ring; only elevated foot sections trade orbiting for anchored continuity.
- **Thickness regression:** changing the high-thickness profile could alter the neutral accepted root. Mitigation: keep the exact thickness-`0.5` exponent and change only the high-thickness interpolation.
- **Diagnostic drift:** an evaluator that only checks topology would miss the silhouette defect. Mitigation: use close-root captures from both twist handednesses and the exact operator profile in addition to the ordinary topology result.
- **Performance regression:** an extra per-vertex frame calculation could increase dirty-generation cost. Mitigation: use constant-time vector arithmetic in the existing build-only surface evaluator, add no rings solely for grounding, and retain the normal production geometry audit afterward.

## TREE-ROOTS.4A — Root Mass + Foot-Trajectory Candidate

Status: candidate evaluation COMPLETE and superseded by TREE-ROOTS.4B production promotion. The Unity candidate board completed 15 / 15 cases; the preferred immediate-quadratic/no-plateau candidate passed every focused topology case and was visually accepted. The 2% fallback showed no topology or geometry-count advantage in matched cases. This section remains historical evidence for the promotion decision.

### Objective

Evaluate a topology-safe root-shape candidate that fixes two verified response defects without rewriting the accepted root-transition architecture:

1. Root Thickness currently saturates at the Root Count sector boundary and then changes its angular profile in a way that can reduce half-mass breadth at high values.
2. The recipe-only foot amplitude retains a protected 10% Root Height plateau and a smooth delayed collapse, which produces the observed rounded/vertical outer nose instead of an immediate long ground-entering slope.

The candidate must preserve the accepted continuous axial roll, stabilized lower root frame, grounded-foot direction, Root Reach crest amplitude, Root Height ownership, Buttress Persistence, contour-owned radial resolution, mixed-resolution stitching, and topology gates.

### Acceptance criteria

- Production `Build` remains behaviorally identical to Bark Algorithm 26 when no candidate is active.
- The candidate uses one fixed `q^4` angular basis for recipe-only root masks; high Thickness broadening is owned by requested support rather than by changing the profile exponent.
- Candidate requested support extends the existing mapping continuously beyond Thickness `1.0`: `0.10 -> 18°`, `0.50 -> 60°`, `1.00 -> 112°`, `2.00 -> 216°`.
- Emitted individual-root support remains clamped to one Root Count sector. Requested width beyond that sector is converted into a bounded shared lower-base merge term rather than overlapping neighbouring root lobes.
- Shared base merge is zero at the root crest, increases toward valleys, is bounded so no valley exceeds the crest contribution, follows the grounded foot direction, and fades with the candidate foot-shape envelope.
- The preferred candidate foot-shape envelope is `(1-u)^2` from the ground to the existing effective foot-collapse endpoint, with no amplitude plateau. A fallback candidate retains only a 2% Root Height plateau before the same quadratic collapse.
- Ground-foot directional anchoring continues to use the accepted production foot envelope. Candidate foot shape and foot anchoring are intentionally separate functions.
- The exact ground crest remains `1 + Root Reach`; the existing effective foot-collapse endpoint remains unchanged; no terrain query or new spatial root mesh is introduced.
- Candidate Thickness values above the public `1.0` limit are evaluated only through an isolated bark-build override. Public descriptors, resolved-control clamps, recipes, settings versions, and ordinary production generation remain unchanged.
- Candidate builds pass the existing topology preflight/final audit and do not require weakened thresholds.
- The focused board compares Production Current, preferred no-plateau candidate, and 2%-plateau fallback with close-root and game-context captures while remaining incremental, cancellable, checkpointed, and unsaved.

### Approved files

Modify only:

- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeRootQualityEvaluation.cs`
- `Assets/Game/Procedural/Trees/Editor/ProceduralTreeInstanceEditor.cs`
- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`

No file creation, deletion, move, scene edit, prefab edit, recipe edit, material edit, layer edit, tag edit, settings-schema change, structural-generator change, public control-range change, or Bark Algorithm promotion is approved.

### Reviewed evidence

- Current production is structural generator `7`, Bark Algorithm `26`, bark settings `10`.
- Current production geometry audit completed `60 / 60` cases with Production Current topology `20 / 20`; the root-shape candidate must preserve these gates after promotion, but this candidate patch does not alter Production Current.
- Recipe-only Root Thickness currently requests `18°` at `0.10`, `60°` at `0.50`, and `112°` at `1.00`, then clamps emitted support to `360 / Root Count`.
- For five roots, support therefore saturates at approximately Thickness `0.615`; for six roots it saturates at `0.500`. Additional Thickness cannot widen the emitted lobe under the current contract.
- The current high-thickness mask changes profile power from `4` toward `2`. At fixed support this reduces off-centre mask mass for `0 < q < 1`; the implementation and historical prose describing this response are inconsistent and the source equation is authoritative.
- Recipe-only root amplitude currently holds its full foot value through `Root Height * 0.10` and then applies the active smootherstep collapse raised to the production foot exponent. The grounded-foot directional release introduced by TREE-ROOTS.3 uses this same foot envelope as its anchor weight.
- The production root sampler already guarantees 24 lower-root collapse intervals under Current and also limits root-envelope changes to `0.075`; a quadratic candidate envelope does not intrinsically require a global tessellation increase.
- The accepted root contour owns all final positions used by finite-difference normals/tangents, topology preflight, mixed-resolution stitching, and final topology audit; the candidate therefore remains inside the existing validation architecture.

### Invariants and non-goals

- Do not change Bark Algorithm `26`, settings `10`, structural generator `7`, or public Root Thickness range in this candidate patch.
- Do not alter continuous axial roll, Signed Path Spiral, root phase identity, root count, Root Reach semantics, Root Height endpoints, Buttress Persistence, branch structure, foliage, shaders, materials, scenes, prefabs, layers, tags, or geometry-efficiency thresholds.
- Do not add hidden production clamps, terrain conformance, separate root meshes, bones, runtime objects, per-frame deformation, or author-facing controls.
- Do not promote a candidate on topology alone. Close-root silhouette evidence is mandatory because the current fixed-camera geometry audit cannot measure silhouette deviation for the present gallery framing.

### File-by-file implementation sequence

1. **Canonical plan — complete.** Record the candidate equations, production isolation, evidence, invariants, risks, and validation contract before code changes.
2. **Candidate bark path — complete.** Added a thread-scoped root-shape candidate strategy and isolated candidate build entry point. Ordinary production Build remains Bark Algorithm 26 and candidate state is restored through `try/finally`.
3. **Candidate response equations — complete.** Added fixed-q4 support, bounded shared-base merge, preferred quadratic no-plateau foot shape, 2%-plateau fallback, and separate production anchor-envelope resolution. Shared-base merge fills only the deficit between authored root contribution and the candidate foot target, so it cannot grow a ground valley beyond the root crest.
4. **Focused evaluation — complete in source.** Retargeted the temporary root-quality board to 15 Production / preferred / fallback cases covering the operator profile, Thickness `0.5`, `1.0`, candidate-only `1.5` / `2.0`, Reach `2.0`, opposite twist, and Persistence `0` / `1` without changing public control clamps.
5. **Inspector — complete.** Retargeted the existing Root Quality Evaluation actions to the candidate board; no new section was added.
6. **Audit — complete for available static/source checks.** Final source comparison, scope/API/schema/math checks, and production-helper equivalence passed. Unity compilation, generated-mesh topology execution, and preview captures remain pending.

### Risks and mitigations

- **Strip-0 regression after removing the 10% amplitude plateau:** test both no-plateau and 2%-plateau candidates through unchanged topology gates; promote the smallest plateau only if required by evidence.
- **Excess valley mass:** compute merge only from requested support beyond one sector, bound it below crest amplitude, and multiply by `(1 - bodyMask)` so the root crest never grows beyond Root Reach.
- **Base mass orbiting under twist:** add merge to the grounded-foot contribution, not the fully twisted body contribution.
- **Candidate under-sampling:** make adaptive sampling and root-contour validation consume the candidate foot-shape envelope while keeping foot directional anchoring on the production envelope.
- **Candidate state leaking into production builds:** use thread-scoped state with `try/finally` restoration and restore any temporary resolved Thickness override after every candidate build.
- **Misleading Thickness > 1 test:** report requested candidate Thickness separately from the public exact-control value and do not change serialized controls or recipe ranges.
- **Performance regression:** candidate adds only constant-time scalar/vector calculations to build-time root evaluation and no per-frame work; no new rings are requested solely for base merge.


### Candidate implementation result

- Production Bark Algorithm remains `26`; settings remain `10`; structural generator remains `7`.
- Candidate state is thread-scoped and restored after every isolated build. Candidate Thickness above `1.0` is applied only to the temporary resolved bark parameters and restored afterward.
- Candidate requested support is `18°` at Thickness `0.10`, `60°` at `0.50`, `112°` at `1.00`, and `216°` at `2.00`; individual support remains sector-clamped.
- Requested width beyond one sector resolves to `excess / (1 + excess)` shared-base merge. The implementation applies this only to the remaining deficit toward `Root Reach × footShapeEnvelope`, multiplied by `(1 - bodyMask)`, so candidate merge cannot make a ground valley exceed the crest.
- Candidate angular masks use fixed `q^4`. Production high-thickness `q^4 → q^2` behavior remains unchanged outside the candidate.
- Preferred foot shape is immediate `(1-u)^2`; fallback holds full amplitude for `Root Height × 0.02` and then uses the same quadratic.
- TREE-ROOTS.3 directional anchoring remains driven by the unchanged production foot envelope in both candidates. Candidate foot shape and anchor release are separate.
- Candidate input fingerprints receive a candidate-only strategy marker; ordinary Production Current fingerprints remain unchanged.
- The focused evaluator reports requested/evaluated Thickness, requested/emitted support, support clamping, shared-base merge, shape-plateau endpoint, half-mass width, geometry counts, topology outcome, and both captures.

### Candidate static/source audit

- Result: `48 / 48` checks passed.
- Exact modified-file scope: four approved files only.
- C# lexical delimiter/string/comment checks passed for all three modified C# sources.
- Production root-envelope implementation is exact against the pre-candidate implementation after helper renaming.
- Production ground half-extension telemetry implementation is exact against the pre-candidate implementation after helper renaming.
- Focused CSV header/data schema alignment: `42 / 42`.
- No deprecated `FindObjectsSortMode` use was introduced.
- Public Root Thickness range owners, structural generator, bark settings, and bark algorithm version were not modified.
- Mathematical sweep over Root Count `3…8`, candidate Thickness `0.10…2.00`, and dense angular samples proved candidate ground merge never exceeds the crest contribution.
- Candidate half-mass width was monotonic across Thickness `0.10…2.00` for every Root Count `3…8`.
- Five-root operator reconstruction at Thickness `0.9`: Production half-mass width `38.811°`; candidate half-mass width `52.205°`; candidate shared-base merge `0.291339`.
- Thickness `0.5` retains the exact H4 ground half-mass width (`41.411534°`) before candidate foot-shape evolution above the ground.
- Preferred and fallback quadratic foot-shape envelopes are monotonic, equal `1` at their ground/plateau start, and reach `0` at the unchanged effective foot-collapse endpoint.
- Changed-files overlay reconstruction against the accepted pre-candidate source: PASS.
- Unified-patch application reconstruction against the accepted pre-candidate source: PASS.

### Candidate validation status

- Review gate: complete.
- Canonical plan gate: complete.
- Candidate source implementation: complete.
- Static/source/math audit: `48 / 48` passed.
- Unity compilation: completed by the operator's candidate evaluation run.
- 15-case root-mass candidate board: `15 / 15` completed.
- Generated candidate topology evidence: all 15 focused cases PASS.
- Close-root operator-profile comparison: preferred no-plateau candidate visually accepted for promotion.
- Production promotion / Bark Algorithm `27`: explicitly authorized and implemented under TREE-ROOTS.4B.
- Public Root Thickness `0.10–2.00` promotion: explicitly authorized and implemented under TREE-ROOTS.4B.

## TREE-ROOTS.4B — Production Root-Mass Promotion

Status: production source implementation complete; static/source/math/compliance audit complete; Unity compilation and production regression execution pending. TREE-ROOTS.4A preferred no-plateau behavior is now ordinary recipe-only Production Current under Bark Algorithm 27.

### Objective

Promote the accepted TREE-ROOTS.4A preferred root-mass response into ordinary recipe-only production bark generation without changing the accepted structural tree generator, continuous axial-twist architecture, grounded-foot directional anchoring, radial ownership/stitching policy, topology thresholds, Root Reach semantics, Root Height semantics, or Buttress Persistence semantics.

### Accepted evidence

- TREE-ROOTS.4A completed 15 / 15 focused cases.
- Preferred no-plateau cases passed at Thickness 0.5, 1.0, 1.5, and 2.0; Reach 2.0; Reach 2.0 plus Thickness 2.0; Twist 0; Twist -400; Persistence 0; and Persistence 1.
- Preferred and fallback candidates emitted identical geometry counts for matched inputs, so the 2% amplitude plateau provided no demonstrated topology benefit.
- The operator accepted the preferred candidate direction and explicitly authorized production promotion.

### Approved files

- `TreeBarkMeshGenerator.cs`
- `TreeControlDescriptorRegistry.cs`
- `TreeGenerator.cs`
- `TreeRecipeControlRanges.cs`
- `TreeResolvedControls.cs`
- `TreeControlResponseSuite.cs`
- `TreeRootQualityEvaluation.cs`
- `ProceduralTreeInstanceEditor.cs`
- `Stylized_Nature_Tree_Integration_Handoff.md`

No scene, prefab, recipe asset, material, layer, tag, terrain system, tree-structure algorithm, or bark settings asset is approved for modification.

### Production contract

1. Recipe-only Root Thickness public hard range becomes 0.10–2.00 everywhere the control contract is enforced.
2. Thickness 0.50 retains the accepted H4 ground breadth anchor.
3. Recipe-only angular masks use one fixed q^4 basis across the complete Thickness range.
4. Requested support remains 18 degrees at Thickness 0.10 and 60 degrees at Thickness 0.50, then continues linearly at 104 degrees per Thickness unit to 216 degrees at Thickness 2.00.
5. Emitted individual support remains bounded by one Root Count sector; support beyond that sector becomes bounded shared lower-base merge rather than overlapping root lobes.
6. Shared-base merge fills only the deficit toward Root Reach times the foot-shape envelope, multiplied by the valley mask; a merged valley may not exceed the root crest contribution.
7. Recipe-only foot-shape amplitude uses immediate quadratic collapse `(1-u)^2` from ground to the existing effective foot-collapse endpoint. There is no production amplitude plateau.
8. Ground-foot directional anchoring remains driven by the accepted TREE-ROOTS.3 production envelope; shape amplitude and directional release remain separate functions.
9. Legacy compatibility roots remain unchanged.
10. Bark Algorithm advances 26 -> 27. Structural generator remains 7. Bark settings remain 10. Resolved/range schema versions remain 2 because no serialized field layout changes.

### Control-response validation contract

Root Thickness response validation must reflect its two-stage meaning:

- low/neutral Thickness must increase measurable individual half-extension breadth;
- high Thickness may saturate individual support because Root Count bounds one sector;
- after support saturation, Ground Root Base Merge must increase monotonically;
- Root Thickness must not change Root Reach crest amplitude;
- the suite high sample becomes Thickness 2.00.

The response CSV must include Ground Root Base Merge so the second stage is direct evidence rather than inferred from geometry.

### Focused regression workflow

Retarget the temporary root-quality evaluator from candidate comparison to production-only regression. Remove candidate/fallback strategy branching and candidate-only thickness override. Keep one compact Wych Elm regression board covering the accepted operator profile, Thickness 0.5 / 1.0 / 1.5 / 2.0, Reach 2.0 plus Thickness 2.0, Twist 0 / -400, and Persistence 0 / 1. All builds use ordinary Production Current.

### Implemented source invariants

- The accepted TREE-ROOTS.4B root mass, immediate quadratic foot amplitude, TREE-ROOTS.3 anchored-foot direction, continuous bark roll, and mixed-resolution stitcher are byte-for-byte unchanged from the pre-4C source in the audited method bodies.
- The contact boost cannot exceed the radial segment count already available from the authored production trunk ceiling. For the common five-root and six-root Twisted cases this permits at most 50 and 60 sides respectively; no global radial maximum was increased.
- Direct boost demand becomes zero once `footShapeEnvelope <= 0.25` or root-only amplitude is at/below `0.35`, and ordinary contour-owned radial resolution then resumes.
- The boost is resolution-only: it changes no vertex target position equation, root envelope, root control value, topology threshold, axial render-sample density, branch radial policy, UV rule, material, or runtime component.
- Normal gameplay receives no new per-frame work. The only expected cost is additional generated bark vertices/triangles in a small number of lowest trunk rings on roots with sufficiently strong contact demand.

### Non-goals

- No exhaustive all-family safe-bound certification in this patch.
- No further root-foot trajectory tuning.
- No new terrain-conforming root system.
- No new root controls.
- No recipe retuning.
- No geometry-efficiency policy change.

### Risks and gates

- High Thickness may expose previously untested topology failures outside the focused Wych cases. The existing topology audit remains authoritative; this patch does not weaken it.
- Public range expansion can be ineffective if any independent 1.0 clamp remains. Static audit must enumerate every Root Thickness hard clamp and prove 2.0 consistency.
- Candidate state must be completely removed from ordinary bark generation; no thread-scoped root-mass strategy or candidate fingerprint marker may remain.
- Production root telemetry must report the promoted merge factor and zero foot-shape amplitude plateau.
- Final source audit must confirm no change to TREE-ROOTS.3 directional anchoring equations.

### Implementation sequence

1. **Canonical plan — complete.** Recorded before code changes.
2. **Promote bark equations — complete in source.** Removed root-mass candidate strategy state/build entry point and candidate fingerprint marker. Fixed-q4 support, bounded shared-base merge, and immediate-quadratic recipe foot-shape amplitude are now ordinary Production Current. Legacy root behavior remains unchanged. Bark Algorithm is `27`.
3. **Promote public Thickness range — complete in source.** Descriptor, exact-control clamp, recipe-range clamp, resolved-control clamp, and recipe-only generator validation now agree on `0.10–2.00`. Serialized schema versions remain `2`.
4. **Update response suite — complete in source.** High sample is `2.00`; CSV directly reports evaluated Thickness, Ground Root Base Merge, and foot-shape plateau endpoint; response gates validate individual breadth before saturation, increasing merge after saturation, and unchanged Root Reach crest amplitude.
5. **Retarget focused evaluator — complete in source.** Candidate/fallback strategy branching and temporary Thickness override were removed. The existing evaluator now runs ten ordinary Production Current regression cases.
6. **Inspector text — complete in source.** Existing Root Quality Evaluation actions now describe the production root-mass regression; no new Inspector section was added.
7. **Static/compliance audit — complete.** Final checks cover candidate machinery removal, clamp consistency, version/schema invariants, exact TREE-ROOTS.3 anchor preservation, CSV schema alignment, mathematical monotonicity/bounds, and approved-file scope.
8. **Unity validation — pending.** Compile, run focused production root regression, run exhaustive control response, run normal twenty-tree geometry audit, then rebuild the curated gallery only after those pass.

### TREE-ROOTS.4B implementation result

- Production versions: structural generator `7`, Bark Algorithm `27`, bark settings `10`, resolved/range schemas `2`.
- Recipe-only Root Thickness hard range is `0.10–2.00` at every public/resolution/validation owner.
- Recipe-only masks use fixed `q^4` over the complete Thickness range. Requested width remains `18°` at `0.10`, `60°` at `0.50`, and continues linearly to `216°` at `2.00`.
- Individual support remains Root Count sector-bounded. Excess requested width becomes bounded shared-base merge and only fills the remaining deficit toward the authored foot target, so a valley cannot exceed the root crest.
- Recipe-only foot-shape amplitude is immediate `(1-u)^2` from ground to the existing effective foot-collapse endpoint. Production shape plateau is therefore zero.
- TREE-ROOTS.3 ground-foot directional anchoring remains a separate unchanged envelope. The accepted continuous axial-roll path and Signed Path Spiral path are unchanged.
- Candidate-only thread state, candidate build entry point, temporary Thickness override, candidate fingerprint marker, and fallback strategy were removed.
- The focused production regression board contains ten Wych Elm cases: operator profile; Thickness `0.5`, `1.0`, `1.5`, `2.0`; Reach `2.0` plus Thickness `2.0`; Twist `0` and `-400`; Persistence `0` and `1`.
- No new per-frame work, runtime object, mesh channel, terrain query, control, or geometry-efficiency policy was added. The promoted calculations remain constant-time build-only root-surface arithmetic.

### TREE-ROOTS.4B static/source audit

- Final closure result: `80 / 80` static/source/math/compliance checks passed after documentation finalization.
- Candidate machinery search: no remaining root-mass candidate strategy/build/fingerprint symbols in current tree source.
- Root Thickness hard-range owners agree on maximum `2.00`; no stale recipe-only `1.00` clamp remains in the promoted surface.
- Versions are exactly generator `7`, bark `27`, bark settings `10`, resolved-controls schema `2`, recipe-ranges schema `2`.
- Exact-source comparison confirmed TREE-ROOTS.3 `ResolveGroundAnchoredRootFootRadial`, the production root-envelope helper used for directional anchoring/body ownership, and authoritative trunk-roll resolution were not altered by promotion.
- Control-response CSV header/data alignment: `41 / 41`. Production root-regression CSV header/data alignment: `42 / 42`.
- Mathematical sweep over Root Counts `3…8` and Thickness `0.10…2.00` confirmed monotonic base merge, monotonic half-extension breadth, preserved Thickness `0.50` H4 support anchor, requested support `216°` at `2.00`, and no shared-base contribution exceeding the root crest.
- No deprecated `FindObjectsSortMode` use was introduced in modified Editor sources.
- Unity compilation, ten-case production regression, exhaustive control-response execution, twenty-tree geometry audit, and persistent gallery rebuild remain pending operator validation.


## TREE-ROOTS.4C — Local Ground-Contact Radial Boost

Status: source implementation complete; static/compliance validation complete; Unity compilation, focused visual/topology evaluation, and twenty-tree production audit pending.

### Objective

Increase circumferential resolution only in the lowest recipe-root contact zone so the ground silhouette of heavy buttresses no longer exposes obvious polygon corners, while preserving the accepted TREE-ROOTS.4B root-mass equations, TREE-ROOTS.3 grounded-foot direction, continuous axial twist, mixed-resolution topology, ordinary upper-trunk resolution, and branch resolution.

### Operator evidence

- Twisted gallery trees 1–5 all show visibly faceted ground-contact silhouettes; Twisted 3 and 4 are the strongest examples.
- The visible defect is localized to the lowest root/earth contact contour. Upper buttress and trunk surfaces are not the target of this patch.
- The operator explicitly approved local lower-root radial densification and requested Twisted 1–5 as the primary focused validation set.

### Reviewed implementation evidence

- `BuildRingRadialSegments` owns per-ring trunk circumferential resolution and already emits Root Count-compatible radial counts while lobes are active.
- Production Current currently resolves at most six samples per lobe from lobe amplitude, which yields 30 sides for five-root rings and 36 sides for six-root rings.
- `ResolveRadialSegments` already supplies an authored production trunk ceiling equivalent to Root Count × 10, capped at 64; the localized boost can therefore reuse existing authored radial budget without increasing the global ceiling.
- `ResolveNextLowerRadialTier` already supports deterministic decreasing Root Count-compatible radial tiers, and `AppendMixedResolutionStrip` already stitches adjacent unequal ring counts sector-by-sector while both rings remain lobe-owned.
- TREE-ROOTS.4B foot-shape amplitude is immediate quadratic `(1-u)^2`; this provides an existing deterministic measure of how strongly a ring still belongs to the distal root-foot/contact shape.

### Approved files

- `TreeBarkMeshGenerator.cs`
- `TreeGeometryEfficiencyAudit.cs`
- `TreeRootQualityEvaluation.cs`
- `ProceduralTreeInstanceEditor.cs`
- `Stylized_Nature_Tree_Integration_Handoff.md`

No settings asset, scene, prefab, recipe, material, root-shape control, structural generator, shader, layer, tag, or terrain system is approved for modification.

### Production contract

1. Bark Algorithm advances from `27` to `28`; structural generator remains `7`; bark settings remain `10`.
2. The boost applies only to recipe-only Production Current trunk rings that are still lobe-owned.
3. Existing root-shape equations, Root Reach, Root Thickness, Root Height, Buttress Persistence, root phase, grounded-foot directional anchoring, and axial-roll equations remain unchanged.
4. The ordinary production samples-per-lobe decision remains the baseline. The contact boost may raise that local decision but never above the existing authored Root Count-compatible radial ceiling.
5. Ground-contact demand is derived from actual root-only amplitude: light roots receive little or no increase; heavy roots approach the available ten-samples-per-lobe ceiling.
6. Boost influence is strongest at the ground and fades with the existing recipe foot-shape envelope; it is zero once foot-shape amplitude reaches 0.25 or less, so upper root/persistence/trunk rings retain ordinary production resolution.
7. Every boosted lobe-owned ring remains an exact multiple of Root Count, preserving lobe-sector correspondence for mixed-resolution stitching.
8. No global trunk, branch, cap, UV, normal, tangent, topology threshold, or axial sampling density is increased.

### Focused validation contract

Retarget the temporary root-quality evaluator to eight production cases:

- Twisted 1
- Twisted 2
- Twisted 3
- Twisted 4
- Twisted 5
- one Common control
- one Pine control
- one Dead control

Each case uses its existing exact-control snapshot without overrides and produces a close low root-contact capture plus exact game-camera capture. Reports must include ground radial segments, boosted-ring count, last boosted normalized distance, total vertices/triangles, and topology result.

### Acceptance criteria

1. Twisted 1–5 complete and pass topology.
2. Their lowest visible root-contact silhouettes no longer show the current large straight polygon facets/corners at ordinary close inspection.
3. Ground-contact radial segments increase materially on the Twisted cases while the boost releases before the upper root/trunk region.
4. Common/Pine/Dead controls remain visually stable and do not receive disproportionate geometry growth.
5. Production twenty-tree topology remains 20/20.
6. No root-shape, twist, persistence, branch, UV, normal, or mixed-resolution stitch regression is introduced.
7. Aggregate geometry growth remains localized and justified; no global radial-density restoration is permitted.

### Risks

- Too much density over too much vertical extent would erase the contour-owned radial optimization. Mitigation: use root-foot amplitude as the release signal and preserve the authored radial ceiling.
- Non-Root Count-compatible intermediate counts would break sector-aligned stitching. Mitigation: resolve only integer samples-per-lobe and multiply by Root Count.
- A hard radial jump could increase stitch cost or expose topology issues. Mitigation: retain the existing deterministic decreasing tier resolver.
- Light-root families could pay unnecessary cost. Mitigation: scale the ground target by actual root-only amplitude rather than family identity.

### Implementation sequence

1. **Canonical plan — complete.** Recorded before code modification.
2. **Localized production radial rule — complete in source.** Production Current recipe-only lobe-owned trunk rings retain the ordinary 3–6 samples-per-lobe result as baseline. Ground-contact demand may raise that result toward the existing authored ceiling using `smoothstep(0.35, 0.85, rootOnlyAmplitude) × smoothstep(0.25, 1.0, footShapeEnvelope)`. The requested value is rounded as integer samples per lobe and therefore remains exactly Root Count-compatible. The ceiling is `min(10, authoredMaximum / RootCount)` samples per lobe, so the existing 64-side/global authored limit is never raised. Bark Algorithm is `28`; structural generator remains `7`; bark settings remain `10`.
3. **Telemetry — complete in source.** Trunk accounting records the actual ground-ring radial count, number of rings whose direct contact-demand request exceeds the ordinary baseline, and the last normalized distance at which that direct boost is requested.
4. **Focused eight-tree board — complete in source.** The existing temporary evaluator now uses exact-control snapshots for Twisted 1–5 plus Common 1, Pine 1, and Dead 1, produces close-root plus exact game-camera captures, runs normal production topology checks, and reports contact radial telemetry and geometry cost. It remains incremental, cancellable, and unsaved.
5. **Geometry audit telemetry — complete in source.** The per-branch geometry-efficiency CSV now exposes ground-contact radial segments, directly boosted-ring count, and direct-boost release distance. The aggregate policy schema is otherwise unchanged.
6. **Inspector text — complete in source.** The existing Root Quality Evaluation section is retargeted to the Ground-Contact Radial Board; no new Inspector section was added.
7. **Static/compliance audit — complete.** Final source closure passed 64/64 checks: exact five-file scope, Bark/Generator/Settings versions, recipe-only Production Current gating, unchanged root-shape/twist/ground-anchor/mixed-stitch methods, Root Count-compatible monotonic radial counts for Root Counts 3–8, zero boost for low foot/amplitude demand, CSV schema alignment, lexical structure, and no deprecated `FindObjectsSortMode` use. The changed-files overlay and unified patch were each independently applied to the accepted pre-4C base and reproduced all five audited modified files byte-for-byte.
8. **Unity validation — pending.** Unity compilation is unavailable in the delivery environment. Compile, run the focused eight-tree board, then run the normal twenty-tree geometry audit before any persistent gallery rebuild.

### Implemented source invariants

- The accepted TREE-ROOTS.4B root mass, immediate quadratic foot amplitude, TREE-ROOTS.3 anchored-foot direction, continuous bark roll, and mixed-resolution stitcher are byte-for-byte unchanged from the pre-4C source in the audited method bodies.
- The contact boost cannot exceed the radial segment count already available from the authored production trunk ceiling. For the common five-root and six-root Twisted cases this permits at most 50 and 60 sides respectively; no global radial maximum was increased.
- Direct boost demand becomes zero once `footShapeEnvelope <= 0.25` or root-only amplitude is at/below `0.35`, and ordinary contour-owned radial resolution then resumes.
- The boost is resolution-only: it changes no vertex target position equation, root envelope, root control value, topology threshold, axial render-sample density, branch radial policy, UV rule, material, or runtime component.
- Normal gameplay receives no new per-frame work. The only expected cost is additional generated bark vertices/triangles in a small number of lowest trunk rings on roots with sufficiently strong contact demand.

### Non-goals

- No further Root Thickness, Root Reach, foot-trajectory, base-merge, or twist tuning.
- No global trunk radial increase.
- No branch radial increase.
- No distance LOD.
- No exhaustive control-bound certification.
- No recipe retuning or persistent gallery regeneration until validation passes.

## TREE-GALLERY.1 — Recenter Source Recipe From Gallery Instance

Status: TREE-GALLERY.1A compile hotfix implemented and cross-partial static audit complete after Unity reported CS0111; Unity compilation and live Editor validation remain pending.

### Objective

Add one explicit Editor authoring operation that treats a currently generated gallery tree as a visual target for its assigned curated recipe. The operation recenters the recipe's existing control intervals around the instance's current exact controls while preserving each interval's existing width whenever the authoritative control domain permits it. The generated instance remains disposable and no live or persistent instance-to-gallery synchronization is introduced.

### Acceptance criteria

1. A generated procedural comparison instance can be used to recenter the curated recipe that the gallery assignment maps to that slot.
2. Ordinary float ranges preserve their pre-operation width exactly, except that the requested center may shift at hard-domain boundaries while the width remains unchanged.
3. Integer ranges preserve their existing integer span; midpoint error of at most one half-step is permitted when parity prevents an exact integer midpoint.
4. Angle ranges preserve their existing circular span and may wrap through zero; a full 360-degree range remains unchanged.
5. Bark Tint preserves each RGB channel span and endpoint direction while moving the interval center as close as possible to the instance color inside 0..1; alpha remains one.
6. Branch Start Height and Branch End Height preserve both existing widths and the recipe contract that every sampled End is at or above every sampled Start. The pair is recentered jointly when independent recentering would overlap.
7. The operation modifies only the target TreeGenerationRecipe asset's ControlRanges. It does not change recipe identity, catalog membership, slot mapping, seeds, materials, scene objects, generated meshes, instance exact controls, or gallery rebuild behavior.
8. Shared recipes are explicit before mutation: the confirmation identifies every curated gallery slot mapped to the same stable recipe identity.
9. Undo restores the recipe asset's previous ranges.
10. The operation never automatically rebuilds the gallery after a recipe is recentered.
11. Existing author-modified curated recipes remain compatible with the curated-catalog validation policy; only the explicit approved-baseline reset operation may intentionally restore code-defined baseline ranges.

### Approved files

- `Assets/Game/Procedural/Trees/TreeRecipeControlRanges.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeCuratedGalleryUtility.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeReferenceGalleryEditor.cs`
- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`

No recipe asset, catalog asset, scene, prefab, material, generated mesh, structural generator, bark generator, shader, layer, tag, or project setting is approved for source-patch modification.

### Reviewed evidence

- The live control registry contains 41 controls: 35 float, 3 integer, 2 angle, and 1 color.
- TreeRecipeControlRanges owns all serialized recipe intervals and the current hard-domain validation, including the coupled Branch Start Height / Branch End Height ordering contract.
- TreeResolvedControls owns the exact per-instance values sampled from those intervals.
- TreeGenerationRecipe exposes its nested ControlRanges and explicit curated-baseline reset remains a separate operation.
- TreeCuratedGalleryAssignment owns the stable family/index to recipe-identity mapping; multiple slots may intentionally share one recipe.
- TreeCuratedGalleryUtility already resolves the active catalog and configures slot spawners from that mapping.
- TreeReferenceGalleryEditor owns the Curated Recipe Generation authoring surface and rebuild action.
- ProceduralTreeInstanceEditor confirms generated children expose editable exact controls and recipe edits do not silently mutate those existing snapshots.
- TreeCuratedRecipeCatalogBuilder preserves author-modified recipes during create-missing/validate operations and resets them only through the explicit approved-baseline reset action.

### Production/editor invariants

- The gallery remains an Editor/dev comparison tool; this patch adds no runtime dependency or per-frame work.
- Rebuild Curated Recipe Comparison Gallery remains recipe-range + deterministic-slot-seed driven and continues to resample exact controls normally.
- The instance is only an explicit one-shot authoring source. No dirty-state tracking, override state, bidirectional sync, or hidden exact gallery preset is added.
- The authoritative target recipe is resolved from the selected instance's procedural comparison slot through the gallery assignment and catalog, not from a manually changed instance Recipe field that the next rebuild would overwrite.
- Existing range widths are the authorial variation budget and are never silently collapsed to exact values.

### Implementation sequence

1. **Canonical plan — complete.** This section was the first project modification after the review surface and repository instructions were reloaded.
2. **Transactional range recentering — complete in source.** `TreeRecipeControlRanges.RecenterFromResolvedControls` clones and upgrades the current ranges, recenters all 41 live controls against `TreeControlDescriptorRegistry`, validates the candidate, verifies every authored span is preserved, and commits only after the complete transaction passes. Float, integer, circular-angle, color, and the coupled Branch Start/End pair each use type-appropriate span-preserving recentering.
3. **Gallery target resolution — complete in source.** `TreeCuratedGalleryUtility.TryResolveRecipeAuthoringTarget` requires the source to be the generated child owned by a procedural comparison slot in the current gallery, resolves the destination through `TreeCuratedGalleryAssignment` plus the gallery's assigned catalog, and enumerates every curated slot sharing that recipe identity. It deliberately does not auto-assign catalog bindings.
4. **Gallery Inspector authoring action — complete in source.** Editor-session-only selection memory retains the most recently selected generated gallery instance while the operator selects the gallery object. The Curated Recipe Generation surface shows the source slot, mapped recipe, and shared consumers, warns on instance/mapped-recipe mismatch, asks for confirmation, records Undo, recenters only the recipe ranges, marks/saves that recipe asset, and does not rebuild the gallery.
5. **Static/source/math audit — complete.** Closure checks cover the exact four-file scope, 41-control coverage, transactional commit, range-span preservation, branch-band ordering, wrapped/full-circle angles, signed RGB spans, mapped-recipe resolution, shared-consumer reporting, session-only selection state, Undo/dirty/save calls, and absence of automatic rebuild. Randomized mathematical probes passed for float, integer, circular-angle, color, and ordered-pair recentering. All 13 currently serialized curated recipe assets have Branch Start/End widths that fit the joint ordered-domain preservation contract.
6. **Unity validation — pending.** Unity compilation and live Editor Undo/asset/rebuild behavior cannot be executed in the delivery environment. Compile, recenter one shared Wych Elm recipe from a tuned Twisted instance, inspect width preservation, verify Undo, then rebuild the curated gallery and confirm deterministic recipe sampling still operates normally.

### Implemented source invariants

- The source instance is never persisted as a gallery override. It is read only when the operator explicitly invokes the recenter action.
- The destination recipe is the recipe the curated rebuild would actually assign to that family/variant slot; a manually changed instance recipe cannot redirect the write.
- A failure during recentering leaves the target `ControlRanges` unchanged because calculation/validation occurs on cloned candidates and only the final validated candidate is copied into the live range object.
- Uninitialized target ranges fail closed. Older initialized serialized range schema is normalized on the temporary baseline clone before recentering; the live recipe is not partially upgraded before the transaction passes.
- Ordinary range width is never reduced to an exact instance value. When the desired center is too close to a hard boundary, the complete interval shifts inside the legal domain.
- Integer ranges preserve integer span. If even/odd parity prevents the exact instance value from being the mathematical midpoint, the nearest valid integer placement is used and reported as center-constrained.
- Full-circle angular ranges remain full-circle. Non-full-circle angle ranges retain their circular span and may wrap through zero.
- Bark Tint preserves each RGB channel's signed endpoint span; alpha remains one.
- Branch Start Height and Branch End Height are solved jointly when necessary so both widths survive and `End.Minimum >= Start.Maximum` remains true after validation.
- The button records/saves only the mapped `TreeGenerationRecipe`; it creates no scene, prefab, mesh, material, catalog, seed, generator, bark, or runtime change.
- `Rebuild Curated Recipe Comparison Gallery` remains unchanged and continues to resample exact controls from the edited recipe ranges using each slot's deterministic seed.

### Audit result

- **TREE-GALLERY.1A compile hotfix — implemented/static-audited:** Unity exposed that `TreeRecipeControlRanges` is split across partial declarations and the TREE-GALLERY.1 patch had added a second `Approximately(Color, Color)` member even though an identical private helper already existed in `TreeCuratedRecipeDefinitions.cs`. The duplicate declaration was removed only; `SameColorRange` now binds to the existing helper on the combined partial type. Cross-partial class-level method-signature audit reports zero duplicate signatures and exactly one `Approximately(Color, Color)` declaration. No recenter math, gallery authoring behavior, serialized asset, or public contract changed.
- **TREE-GALLERY.1 audit correction:** the earlier 44/44 static closure did not inspect member-signature collisions across every partial declaration, so it was insufficient as a compilation preflight. TREE-GALLERY.1A adds that cross-partial check; Unity compilation is still the authoritative remaining gate.
- Final static/source/math/packaging closure: **44/44 PASS**.
- Live registry coverage: **41 controls = 35 float + 3 integer + 2 angle + 1 color**.
- Randomized math probes: float 10,000; integer 10,000; circular angle 10,000; color 10,000; ordered Branch Start/End pair 20,000 — all PASS.
- Current curated serialized recipes checked for ordered-pair feasibility: **13/13 PASS**.
- Changed-files overlay reconstruction against the accepted pre-edit source: **PASS**.
- Unified-patch reconstruction against the accepted pre-edit source: **PASS**.
- Repository limitation: this extracted delivery workspace contains no `.git` metadata, so HEAD/branch/SHA verification is unavailable here. The patch is audited against explicit pre-edit copies of the accepted TREE-ROOTS.4C source; Unity compilation remains pending.

### Risks and mitigations

- **Shared-recipe surprise:** one tuned instance may affect several gallery slots on the next rebuild. Mitigation: mandatory consumer list in the confirmation dialog.
- **Boundary compression:** independently clamping min/max would shrink ranges. Mitigation: clamp the desired center/interval position instead, preserving span.
- **Branch-band invalidation:** independent recentering can make Branch End overlap Branch Start. Mitigation: solve the two intervals jointly while preserving both widths and the all-samples ordering contract.
- **Angle discontinuity:** linear min/max arithmetic is wrong across zero. Mitigation: preserve circular angular span and allow wrapped intervals.
- **Wrong destination recipe:** a user may manually change the instance Recipe field. Mitigation: resolve the destination from the stable gallery slot assignment, and report any instance/mapped-recipe mismatch before mutation.
- **Unintended baseline reset:** curated definitions still contain approved initial ranges. Mitigation: this patch does not call ConfigureCuratedDefinition; the existing explicit baseline-reset operation remains the only reset path.

### Non-goals

- No exact per-slot gallery presets.
- No instance override persistence.
- No live recipe/instance binding.
- No Sync All operation or averaging across instances.
- No new public creative controls.
- No gallery rebuild, regeneration, or recipe resampling triggered by the recenter button.
- No changes to recipe ranges other than their centers/positions under the width-preservation contract.


## TREE-TRUNK.1 — Lean Direction Removal + Lean/Bend Contract Diagnostics

Status: source implementation complete; static/consistency audit 85/85 PASS. Unity compilation, 40-control response execution, and Lean/Bend diagnostic execution remain operator validation gates.

### Objective

1. Remove Lean Direction as a live tree control and generation parameter. Lean Amount becomes a canonical tree-local +X displacement; operators use whole-object yaw when a different world direction is desired.
2. Preserve the current Lean Amount, Bend Amount, Bend Frequency, root-frame, root-foot, twist, Path Spiral, and bark geometry behavior otherwise.
3. Add an incremental, cancellable Lean/Bend interaction diagnostic that measures the current structural-centreline versus bark-surface-frame contract across persistence, root, twist, and Path Spiral combinations before any corrective Lean/Bend/root-frame patch is designed.

### Acceptance criteria

- The live control registry contains 40 controls and no Lean Direction descriptor, exact-control field, recipe range, recenter operation, generation override, resolved parameter, fingerprint input, report field, curated-definition comparison, or Inspector condition remains.
- Recipe-only and legacy generation both use one canonical tree-local +X lean direction. No automatic whole-object rotation is introduced.
- Existing initialized exact snapshots and recipe ranges migrate without resampling surviving controls; removal of Lean Direction is the only intentional control-schema loss.
- No Lean Amount/Bend Amount/Bend Frequency response equation, root-frame envelope, grounded-foot anchoring, axial twist, Path Spiral, root mass, or radial-resolution calculation is changed by this patch.
- The focused diagnostic advances at most one bounded case per Editor update, supports cancellation, displays progress/ETA, checkpoints report/CSV evidence, builds temporary unsaved meshes only, and runs existing bark topology validation for every completed case.
- The diagnostic reports structural and bark-frame measurements at fixed normalized trunk heights plus the current root collapse/transition landmarks, including tangent mismatch, azimuth mismatch, frame/root envelopes, tip displacement, curvature, and first height where tangent mismatch falls below 5 and 1 degrees.
- The diagnostic matrix includes Lean x Buttress Persistence, Bend x Buttress Persistence, the Bend Amount/Frequency-zero dependency, combined Lean+Bend, light/heavy root profiles, axial twist handedness, and Path Spiral interaction.

### Approved source scope

Modify only:

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeControlDescriptorRegistry.cs`
- `Assets/Game/Procedural/Trees/TreeResolvedControls.cs`
- `Assets/Game/Procedural/Trees/TreeRecipeControlRanges.cs`
- `Assets/Game/Procedural/Trees/TreeGenerationParameters.cs`
- `Assets/Game/Procedural/Trees/TreeGenerationOverrides.cs`
- `Assets/Game/Procedural/Trees/TreeFamilyProfile.cs`
- `Assets/Game/Procedural/Trees/TreeGenerator.cs`
- `Assets/Game/Procedural/Trees/TreeCuratedRecipeDefinitions.cs`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeControlRangeDrawer.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeResolvedControlsDrawer.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeControlResponseSuite.cs`
- `Assets/Game/Procedural/Trees/Editor/ProceduralTreeInstanceEditor.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeBarkMeshAssetBuilder.cs`

Create only:

- `Assets/Game/Procedural/Trees/Editor/TreeTrunkResponseDiagnosticSuite.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeTrunkResponseDiagnosticSuite.cs.meta`

No scene, prefab, material, recipe asset, catalog asset, gallery asset, layer, tag, package, shader, runtime component, generator-version, bark-version, or bark-settings-version change is approved.

### Reviewed evidence

- The current registry exposes Lean Amount and Lean Direction as separate Trunk Shape controls; Lean Direction is an angle control with stable ID `tree.trunk.lean-direction`.
- Exact controls sample and fingerprint Lean Direction independently. Recipe ranges store, validate, recenter, copy, and compare an independent Lean Direction interval.
- Recipe-only parameter resolution copies Lean Direction into the structural parameter block. Legacy family/profile generation samples and overrides a separate trunk-lean-yaw value.
- Current structural trunk generation computes Bend, Drift, Roughness, and Lean as additive horizontal centreline offsets. Lean magnitude is `Lean Amount * Height * t^1.35`; Bend is gated by positive Bend Frequency and uses the existing sinusoidal two-axis curve.
- Current bark root-frame resolution blends the structural tangent toward tree-local up using `EvaluateRootFrameEnvelope`. The same persistence-aware root envelope also controls fixed-root-normal adoption; Buttress Persistence can therefore extend frame stabilization well beyond the root collapse region.
- Current grounded root-foot direction is independently anchored in the tree-local right/up frame and released only through the existing foot-anchor envelope. This patch records that relationship but does not modify it.
- The existing exhaustive control suite is incremental/cancellable but primarily validates fingerprints and broad geometry invariants; it does not directly quantify structural-tangent versus bark-surface-tangent error for Lean/Bend combinations.
- The current curated definitions and serialized recipe baselines use local zero-degree Lean Direction, so canonical +X lean matches their intended local orientation; stale serialized fields in existing Unity assets are not raw-edited by this source patch and become ignored removed fields until Unity reserializes those assets.

### Invariants and non-goals

- Do not fix the suspected persistence/tangent coupling in TREE-TRUNK.1. The suite must measure the current behavior first.
- Do not retune Lean Amount, Bend Amount, Bend Frequency, Drift, Roughness, root controls, twist, or Path Spiral.
- Do not change topology thresholds, root sampling density, root radial density, mixed-resolution stitching, materials, foliage, branch response, or runtime update cost.
- The diagnostic is Editor-only and temporary-output-only; no diagnostic result is serialized into tree assets or instances.
- Existing historical documentation may describe Lean Direction as historical behavior, but the active control contract after this patch treats it as removed and obsolete.

### File-by-file implementation sequence

1. **Canonical plan — complete.** This plan was the first project modification after the review gate.
2. **Control/schema removal — source complete.** Lean Direction is removed from the 40-control live registry, exact controls, recipe ranges/recentering, curated definitions/comparison, drawers, response suite, fingerprints and reports. Exact-control and recipe-range schemas are version 3; initialized surviving values migrate in place without resampling.
3. **Structural parameter removal — source complete.** Recipe-only and legacy Lean now use canonical tree-local +X. The old resolved parameter/profile range/override/sampling/validation/report ownership for Lean Direction is removed.
4. **Production frame telemetry — source complete.** Read-only trunk-frame telemetry calls the existing production root-frame, surface-frame, foot-anchor, envelope and roll methods. Static comparison proves those production methods are byte-identical to the accepted pre-patch source.
5. **Focused diagnostic runner — source complete.** The Editor-only runner contains 35 topology-validated cases, one case per Editor update, cancellation/reload/quit handling, progress/ETA, per-case checkpointing, a 36-column case CSV and a 24-column per-height sample CSV. It records effective tree/root context, structural Lean tip expectation/error, maximum horizontal displacement, structural/surface curvature, frame mismatch and root-transition envelopes.
6. **Inspector action — source complete.** The 40-control suite and Lean/Bend diagnostic have explicit run/cancel/report actions and mutually exclude the other long-running tree diagnostics.
7. **Audit — static complete; Unity pending.** Final scope is exactly the approved 17 files (15 modified, two created), with no asset/scene/prefab/material change. Static/consistency audit is 85/85 PASS: 40 live controls; 40 resolved fields; 40 recipe-range fields; 40 recenter operations; 40 curated comparisons; zero live Lean Direction symbols/stable IDs in tree C#; exact CreateTrunk equivalence aside from canonical +X direction; exact production bark-frame/root method equivalence; CSV alignment; unique meta GUID; cross-partial duplicate guard; no deprecated FindObjectsSortMode. Unity compilation and live suite execution remain operator gates.


### Implementation result and static evidence

- Live creative-control count: **40** (`35 float / 3 integer / 1 angle / 1 color`). Lean Direction is no longer a live descriptor, serialized exact-control member, recipe range, recenter target, generation parameter, legacy override/profile range, fingerprint/report value, curated-definition comparison, or Inspector dependency.
- Lean Amount now uses canonical tree-local **+X**. The Bend, Drift, Roughness and Lean magnitude equations are otherwise unchanged from the accepted pre-patch `CreateTrunk` implementation.
- Existing initialized exact snapshots are treated as authored snapshots across schema migration and are upgraded/clamped in place; uninitialized snapshots still resolve from recipe ranges. Recipe ranges upgrade in place and preserve all surviving ranges.
- The 13 curated recipe assets are **byte-identical** to the pre-patch source. Their historical serialized Lean Direction values are all `0..0`; because no serialized asset mutation was approved, those now-unknown YAML keys remain historical text until Unity reserializes the assets, but they have no live source field or generation consumer.
- Generator / bark / bark-settings algorithm versions remain **7 / 28 / 10**. The exact-control and recipe-range schema versions are **3 / 3**.
- The focused diagnostic matrix totals **35 cases**: Neutral×Persistence (3), Lean×Persistence (9), Bend×Persistence (6), Bend Frequency zero (1), combined Lean+Bend×Persistence (6), root interaction (3), axial twist interaction (3), and Path Spiral interaction (4).
- Static/consistency audit: **85 / 85 PASS**. Changed-files overlay reconstruction against the accepted pre-edit snapshot is **PASS**; unified-patch apply/check and byte-for-byte reconstruction are **PASS**.
- No Unity compiler is available in the delivery environment, so compilation and generated diagnostic evidence are explicitly pending rather than inferred.

### Risks and mitigations

- **Serialized migration loss:** increasing schema version could resample all exact controls. Mitigation: initialized pre-current snapshots are upgraded in place and clamped; only uninitialized snapshots sample from ranges.
- **Legacy compatibility drift:** removing a legacy yaw range changes old profile behavior. Mitigation: preserve Lean strength and every other legacy control while fixing Lean to canonical local +X, matching the explicitly accepted redundancy-removal contract.
- **Diagnostic drift:** duplicating private frame equations could produce misleading evidence. Mitigation: expose read-only telemetry from the bark generator that calls the same production frame/envelope methods used for mesh emission.
- **Diagnostic cost/editor freeze:** bark topology builds can take noticeable time. Mitigation: one case per Editor update, cancellation, ETA, and incremental flush after every case.
- **Premature repair:** measured data may suggest a different Lean/Bend fix than the current hypothesis. Mitigation: TREE-TRUNK.1 changes no Lean/Bend/root-frame equation other than deleting Lean Direction.


## TREE-TRUNK.2 — Lean/Bend Contract Repair

### Status

**Implementation and delivery-side static/source audit complete. Unity compilation, focused runtime diagnostics, broad regression, production geometry/topology validation, visual review, and measured performance remain operator gates.**

### Objective

Repair the two independently proven contracts behind the current Lean/Bend failure without changing the authored Lean or Bend equations:

1. Recipe-only non-Path-Spiral trunk safety limiting must preserve the authored base, tip, and vertical profile instead of reconstructing a new three-dimensional chain that can move the Bend-only tip, change authored Height, or corrupt Lean's endpoint.
2. Recipe-only root-frame tangent stabilization must release at the existing persistence-independent earliest safe root transition, while Buttress Persistence continues to control persistent root/buttress azimuth and frame adoption.

### Reviewed evidence

- Current structural generator version is `7`. `CreateTrunk` authors exact `y = Height × t`, Bend is multiplied by `sin(pi × t)` and therefore contributes zero at base/tip, and Lean uses canonical tree-local `+X`.
- Ordinary non-Path-Spiral trunks currently route through the shared three-dimensional `ConstrainPolylineTurns` reconstruction. That method rebuilds every next point from the previously constrained point and a rotated segment direction, so once limiting occurs neither authored endpoint nor authored vertical progression is preserved.
- The accepted active Path Spiral route already bypasses that reconstruction and preserves authored vertical progression; it must remain unchanged.
- Current bark algorithm version is `28`. `ResolveTrunkBaseSurfaceFrame` currently uses the persistence-aware root-frame envelope both to Slerp the structural tangent toward `Vector3.up` and to adopt the transported structural normal.
- `CalculateEarliestRootTransitionHeight` already provides a persistence-independent certified release boundary; `CalculateEffectiveRootTransitionHeight` extends only the persistence-aware frame transition toward the tip.
- The focused 35-case diagnostic established that Lean-only endpoint response is correct, strong Bend acquires endpoint/height drift after authoring, and Buttress Persistence can delay rendered-vs-structural tangent agreement through most of the trunk.
- The generic branch limiter, Path Spiral route, grounded root foot, root mass/thickness/reach formulas, local contact radial-density work, axial twist, and mixed-resolution stitching are accepted behavior and are non-goals.

### Approved files

Modify only:

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeGenerator.cs`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeTrunkResponseDiagnosticSuite.cs`

Create/Delete/Move/Rename: none. Serialized assets: none.

### Acceptance criteria

1. Lean-only structural endpoint response remains unchanged.
2. Recipe-only non-Path-Spiral Bend-only trunks preserve the authored base/tip and exact authored `Height`.
3. Recipe-only non-Path-Spiral Lean+Bend trunks preserve the authored Lean endpoint; Bend changes only intermediate curvature.
4. Existing trunk segment-turn and accumulated-turn policy limits remain enforced without using destructive point reconstruction on the recipe-only non-Path-Spiral trunk path.
5. Shapes that already satisfy the policy remain byte-for-byte equivalent at the control-point level apart from the generator-version value.
6. Active recipe-only Path Spiral routing and implementation remain unchanged.
7. Generic branch and legacy `ConstrainPolylineTurns` behavior remain unchanged.
8. Recipe-only bark tangent stabilization reaches zero by `CalculateEarliestRootTransitionHeight` for every Buttress Persistence value.
9. Buttress Persistence continues to affect the existing root-frame/azimuth adoption and buttress-body persistence.
10. Grounded root foot, root envelopes/mass, radial density, axial roll/twist, mixed-resolution stitching, and topology thresholds remain unchanged.
11. Focused diagnostics expose separate tangent-safety and root-frame envelopes plus explicit endpoint, height, and transition invariants without weakening topology validation.
12. Structural generator version advances `7 → 8`; bark algorithm version advances `28 → 29`; control count, exact-control schema, recipe-range schema, and bark settings remain unchanged.
13. No per-frame/runtime update work, new persistent state, new component, tag, layer, dependency, or serialized asset edit is introduced.

### Generator algorithm

For recipe-only, non-Path-Spiral trunk control points only:

1. Keep the authored `CreateTrunk` equations unchanged.
2. Preserve authored base, tip, and every authored Y coordinate.
3. Let `t_i = i / (N - 1)` and define the endpoint chord in XZ as `C_i = lerp(baseXZ, tipXZ, t_i)`.
4. Define each authored interior residual `R_i = authoredXZ_i - C_i`.
5. Candidate scale `s` produces `candidateXZ_i = C_i + s × R_i`, with original Y retained and endpoints copied exactly.
6. Validate a candidate against the existing maximum horizontal displacement, maximum segment-turn, and maximum accumulated-turn requirements without mutating the candidate.
7. If `s = 1` passes, leave the authored points unchanged.
8. Otherwise run a fixed deterministic 10-iteration bisection on `s ∈ [0, 1]` and keep the largest passing candidate.
9. If even `s = 0` cannot satisfy a hard policy because the authored endpoint itself is invalid, preserve the endpoint/Y contract and emit a generation warning rather than silently changing the public endpoint semantics.
10. Keep the legacy/profile and branch reconstruction paths unchanged. Keep the dedicated active Path Spiral route unchanged.

### Bark algorithm

1. Add a persistence-independent tangent-safety envelope for recipe-only bark frames.
2. Envelope is `1` through the current effective root-collapse end.
3. Between collapse end and `CalculateEarliestRootTransitionHeight`, release with the existing smootherstep shape.
4. Envelope is `0` at/above the earliest root transition.
5. Use this envelope only for the structural-tangent-to-up Slerp.
6. Keep `EvaluateRootFrameEnvelope` persistence-aware and continue using it for fixed-root-normal/transported-normal adoption and the existing persistent root/buttress frame semantics.
7. Do not alter effective buttress-body persistence or grounded-foot behavior.
8. Expose both envelope values through trunk-frame diagnostic telemetry.

### Focused diagnostic update

Retain all 35 current cases and existing topology auditing. Add/report:

- `TangentSafetyEnvelope` separately from `RootFrameEnvelope`.
- Final structural Y and authored-height error.
- Bend-only endpoint horizontal error.
- Lean+Bend endpoint error relative to the authored Lean endpoint for non-spiral cases.
- Tangent mismatch at/after the earliest root transition.
- Explicit invariant verdict text while retaining raw structural/surface vectors and existing measurements.

Known low-Root-Height topology failure remains an external known failure and must be reported separately rather than hidden or repaired in this update.

### Performance model

The new structural limiter runs only during deterministic generation/dirty work on the recipe-only non-Path-Spiral trunk. With the current recipe-only trunk control-point count of 12 and a fixed 10-iteration bisection only when the authored shape fails, work is bounded `O(N × I)` over a very small N. Candidate validation must avoid per-iteration heap allocations. The bark change adds two scalar envelope evaluations during mesh build. No active-play per-frame, render-loop, physics, AI, shader, draw-call, or vertex-format cost is introduced.

### Validation/compliance plan

- Static/source: exact four-file diff; lexical/bracket checks; no stale version strings; CSV header/value parity; no deprecated API; no serialized assets.
- Preservation: compare pre/post source for generic `ConstrainPolylineTurns`, active Path Spiral helpers/routing, grounded-foot/root contribution/radial-density/twist methods, and topology thresholds.
- Algorithmic: reconstruct representative control-point cases and verify base/tip/Y preservation plus policy compliance.
- Unity focused diagnostic: require previously topology-valid cases to remain valid; Lean endpoints remain exact; Bend-only tip and height errors collapse to numerical tolerance; non-spiral Lean+Bend endpoint equals authored Lean endpoint; tangent mismatch is approximately zero by earliest root transition for all Persistence values while RootFrameEnvelope still differs with Persistence.
- Broad regression: rerun the 40-control response suite and compare failure set; do not require unrelated known Root Height/Persistence suite defects to disappear.
- Production validation: run the existing geometry/topology workload and visually inspect current Twisted gallery trees for root-transition folding or changed accepted root contact.
- Performance: compare same-workload generation/build timing and mesh counts before/after; runtime architecture must remain recurrence-free.

### Implementation sequence/status

1. Canonical plan first modification — **complete**.
2. Generator endpoint/height-preserving recipe-only non-spiral limiter and version bump — **complete**.
3. Bark tangent-safety/root-frame envelope split, telemetry, and version bump — **complete**.
4. Focused diagnostic verification contract — **complete**.
5. Final source/diff/consistency/performance audit — **complete for delivery-side static/source evidence; Unity/measured performance pending**.
6. Unity compilation and operator validation — **pending**.


### Implementation result and static/source evidence

- Structural generator version is now `8`; bark algorithm version is now `29`. Exact-control schema and recipe-range schema remain `3`; no control/schema/settings change was made.
- Recipe-only non-Path-Spiral trunks now route through `ConstrainRecipeOnlyTrunkPreservingEndpointAndHeight`. The authored candidate is returned untouched when it already satisfies the hard horizontal, per-segment-turn, and accumulated-turn policies. Failing authored candidates use one preallocated chord/residual/candidate set and a fixed 10-iteration residual-scale bisection; base, tip, and every authored Y coordinate are copied exactly.
- The existing shared `ConstrainPolylineTurns` implementation is byte-identical to the pre-update source. The accepted active Path Spiral helpers/routing, including height-preserving horizontal-envelope scaling, are unchanged.
- A static mathematical reconstruction of the diagnostic trunk equations over 128 primary-curve orientations per representative case confirmed: Lean `0.15/0.30/0.60`, Bend `0.50`, Lean `0.30` + Bend `0.50`, and Bend `1.00` with frequency `0` retain full residual scale `1.0`; Bend `1.00` resolves to scale `0.6875`; Lean `0.60` + Bend `1.00` resolves between approximately `0.725586` and `0.907227` depending on curve orientation. Every reconstructed candidate preserved the authored tip exactly, preserved every Y exactly, and satisfied the `0.65 × Height` horizontal envelope, `38°` segment-turn limit, and `190°` accumulated-turn limit. This is algorithmic/static evidence, not Unity execution.
- Recipe-only bark now computes `TangentSafetyEnvelope` independently of Buttress Persistence: `1` through effective root collapse, smootherstep release through the existing earliest safe transition, and `0` at/after that transition. The existing persistence-aware `RootFrameEnvelope` remains byte-identical and still controls fixed-root-normal/transported-normal adoption.
- The focused suite still contains the same 35 cases and still requires the existing production bark topology audit before contract checks. It now emits a 42-column summary CSV and 25-column sample CSV; source-level header/value parity is exact. New evidence includes final structural Y, authored-height error, Bend-only endpoint error, Lean+Bend endpoint error, earliest-transition tangent mismatch, invariant status, and separate tangent-safety/root-frame envelopes. Topology result and contract result are reported independently.
- Lexical delimiter/state audit passes on all three edited C# files. No stale `TREE-TRUNK.1` runtime label remains in the focused suite. No hardcoded source dependency expects Generator `7` or Bark `28`.
- Archive-overlay scope audit against the supplied source reports exactly four changed project files, matching the approved scope; zero files are missing and zero unapproved files are added.
- Preservation audit confirms the shared branch/legacy turn limiter, root collapse calculation, earliest/effective root transitions, effective buttress-body end, persistence-aware root-frame envelope, and grounded root-foot radial resolver are unchanged. Active Path Spiral source was separately diff-checked and unchanged.
- No new `FindObjectsSortMode`, `OnValidate` mutation, layer, tag, component, dependency, serialized asset edit, per-frame callback, shader work, or runtime persistent state was introduced. The suite's existing temporary-mesh `DestroyImmediate` remains Editor-runner cleanup and is not called from `OnValidate`.
- Performance status remains analytical: the new limiter allocates three arrays only when an authored recipe-only non-spiral trunk first fails policy; its 10 bisection passes reuse those arrays and operate on the current 12 control points. Bark adds one scalar envelope evaluation during build. No active-gameplay recurring path was added. Unity profiler/build-time measurements remain pending.
- Final direct-consumer audit found one stale Inspector help string in the existing procedural-tree instance Editor: it still describes TREE-TRUNK.1 as measurement-only and says Lean/Bend behavior is not repaired. That Editor file is outside the approved TREE-TRUNK.2 four-file scope, so it was not modified. This is a documentation/UI-copy inconsistency only; the diagnostic runner and production code use the TREE-TRUNK.2 contract. A scope-approved follow-up should update that help text before closure.
- No Unity compiler/runtime is available in the delivery environment. Compilation, the updated focused 35-case run, 40-control regression comparison, production geometry/topology audit, Twisted visual inspection, and same-workload timing remain required operator evidence.

### Risks and fail-closed rules

- If the endpoint itself violates a hard displacement policy, do not silently move it; preserve the authored contract and report the conflict.
- If the new tangent release causes root-transition topology failures in previously passing cases, do not restore Persistence-driven tangent locking wholesale; inspect the certified collapse/earliest-transition interval.
- If Buttress Persistence loses frame/azimuth effect, restore the existing persistence-aware normal-adoption contract; do not move it onto the tangent-safety envelope.
- If active Path Spiral or generic branch output changes, treat it as a regression and revert the routing leak.
- If another file is required, stop implementation, amend this plan, and obtain scope approval before editing it.

## TREE-TRUNK.2A — Root-Frame Transition Topology Diagnostic

Status: diagnostic-stage implementation approved; production geometry behavior intentionally frozen pending first-unsafe-strip evidence.

### Objective

Explain the three TREE-TRUNK.2 topology regressions that occur only under strong Lean+Bend with nonzero Buttress Persistence before changing the production frame algorithm. Preserve the validated Generator 8 endpoint/height solver and Bark 29 tangent-release contract while exposing the exact geometry/frame state at the first unsafe trunk strip.

### Observed Unity evidence

The operator-completed 35-case TREE-TRUNK.2 focused run reports 31 PASS / 4 FAIL. The structural contract is repaired: Lean-only endpoints remain exact, Bend-only endpoint displacement is zero at Bend 0.50 and 1.00, final structural Y remains the authored 11 m, and non-spiral Lean+Bend preserves the Lean-authored endpoint. Persistence-independent tangent release also reaches zero mismatch at the earliest transition in topology-valid cases.

Topology failures are concentrated as follows:

- Lean 0.60 + Bend 1.00 / Persistence 0.0: PASS.
- Lean 0.60 + Bend 1.00 / Persistence 0.5: FAIL, firstUnsafe=39, unsafeCount=1.
- Lean 0.60 + Bend 1.00 / Persistence 1.0: FAIL, firstUnsafe=39, unsafeCount=1.
- Light roots / strong Lean+Bend / Persistence 1.0: FAIL, firstUnsafe=29, unsafeCount=1; this case already failed before TREE-TRUNK.2 and remains a known external topology defect.
- Authored roots / strong Lean+Bend / Persistence 1.0: FAIL, firstUnsafe=39, unsafeCount=1.
- Heavy roots / strong Lean+Bend / Persistence 1.0: PASS.

Inference — High confidence: the three new failures are more likely in the Bark tangent/root-frame decoupling than the Generator solver because the same strong Lean+Bend structural contract passes at Persistence 0.0 and fails only when the persistence-aware root frame remains active. Heavy roots passing shows the issue is transition-geometry dependent rather than a universal Persistence>0 failure. This inference must be verified with first-unsafe-strip frame/correspondence telemetry before any production correction.

### Approved diagnostic-stage scope

Modify only:

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeTrunkResponseDiagnosticSuite.cs`

Create/delete/move/serialized assets: none.

### Invariants and non-goals

- No Generator 8 behavior change.
- No Lean, Bend, Bend Frequency, Path Spiral, Axial Twist, root mass, root reach/thickness, grounded-foot, contact-resolution, topology threshold, or branch constraint change.
- No change to `EvaluateRootTangentSafetyEnvelope`, `EvaluateRootFrameEnvelope`, `ResolveTrunkBaseSurfaceFrame`, or any production frame/vertex calculation in this diagnostic stage.
- No bark/generator/settings version bump.
- Do not restore Persistence-driven tangent locking.
- Do not tune or smooth the tangent envelope from inference alone.

### Diagnostic implementation

1. Preserve the complete bark-build failure text in the focused suite instead of reducing it to the first line.
2. Extend the existing root-ring correspondence diagnostic with current->next TangentSafetyEnvelope, structural-to-resolved-surface tangent mismatch, and the collapse/earliest/effective transition/buttress-body landmarks.
3. Keep the existing root-frame envelope, ring-frame rotation, contour orientation, local-sweep, cross-section multiplier, and quad coordinates in the same failure payload.
4. Keep topology validation unchanged; diagnostics must explain the failure rather than suppress it.
5. Report detailed topology evidence in the text report and escaped CSV field so one rerun is sufficient to identify the production repair candidate.

### Acceptance criteria

- Production geometry output is byte-equivalent before/after this diagnostic stage.
- A rerun of the focused suite preserves the same pass/fail set unless nondeterminism is independently proven.
- Every failed bark-build case includes a complete `[Root Ring Correspondence Diagnostic]` block rather than only `firstUnsafe`/`unsafeCount`.
- The detailed block identifies whether the unsafe strip lies inside/after tangent release and quantifies how much persistence-aware root-frame adoption remains there.
- Static audit confirms only the approved three files changed.

### Status

1. Review and plan — complete.
2. Diagnostic instrumentation — pending.
3. Static preservation/scope audit — pending.
4. Unity rerun/evidence collection — pending.
5. Production repair design — blocked on step 4 evidence.

### Diagnostic-stage implementation result

- `BuildRootRingCorrespondenceDiagnostic` now reports TangentSafetyEnvelope current->next, structural-to-resolved-surface tangent mismatch current->next, and the effective collapse / earliest transition / effective transition / buttress-body landmarks in addition to its existing ring-frame, contour, orientation, root-envelope, root-frame, roll, multiplier, and quad telemetry.
- The focused suite now preserves the complete Bark build failure payload instead of reducing Bark topology failures to `FirstFailureLine`. The text report therefore contains the full `[Root Ring Correspondence Diagnostic]` block for each failed Bark build; the existing escaped CSV Failure field retains the same payload.
- No production frame, envelope, vertex, topology-threshold, Generator, Path Spiral, root, twist, or branch behavior was edited. Bark remains algorithm version 29 and Generator remains version 8.
- Static delimiter checks pass for both edited C# files. Diff against the accepted TREE-TRUNK.2 changed-files package shows the Bark delta is confined to `BuildRootRingCorrespondenceDiagnostic`; the focused-suite delta is confined to preserving full Bark failure text and TREE-TRUNK.2A report labels.
- Unity compilation and rerun remain pending operator evidence. The production repair design remains blocked until the detailed first-unsafe-strip output is available.

### Updated status

1. Review and plan — complete.
2. Diagnostic instrumentation — complete.
3. Static preservation/scope audit — complete.
4. Unity rerun/evidence collection — pending.
5. Production repair design — blocked on step 4 evidence.


---

## TREE-TRUNK.2B — Exhaustive Root-Frame / Ring-Correspondence Candidate Search

Status: implementation approved for diagnostic search only. The rejected Bark 30 resolved-surface-tangent production refinement is reverted by using the accepted Bark 29 TREE-TRUNK.2A diagnostic state as this update's source baseline. No candidate tested by this suite is promoted to production geometry in this update.

### Objective

Replace serial one-hypothesis debug patches with one broad, bounded, cancellable Editor search that evaluates a full Cartesian product of plausible repair families against both failing and passing control cases. Record enough per-candidate and per-case geometry/topology/contract/cost evidence to reject bad repair families, identify robust leads, and expose interactions among candidate mechanisms before any production repair is chosen.

### Approved files

Modify only:

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeTrunkResponseDiagnosticSuite.cs`

Create/delete/move/serialized assets: none.

### Source baseline and revert requirement

- Source baseline is the accepted TREE-TRUNK.2A diagnostic state: Generator 8 / Bark 29.
- The rejected Bark 30 adaptive resolved-surface-tangent sampling experiment is not carried into production behavior.
- Preserve TREE-TRUNK.2 endpoint/height repair, persistence-independent tangent release, expanded unsafe-strip telemetry, topology thresholds, Path Spiral, Axial Twist, roots, grounded foot, contact sampling, and branch behavior.

### Search cases

Keep the existing 35-case verification suite unchanged as the baseline gate. After those cases, run the candidate matrix on the targeted case set containing:

1. strong Lean+Bend / Persistence 0.0 — known passing control;
2. strong Lean+Bend / Persistence 0.5 — TREE-TRUNK.2 failure;
3. strong Lean+Bend / Persistence 1.0 — TREE-TRUNK.2 failure;
4. light roots / strong Lean+Bend — known pre-existing failure control;
5. authored roots / strong Lean+Bend — TREE-TRUNK.2 failure;
6. heavy roots / strong Lean+Bend — known passing root-transition control;
7. Path Spiral +2 interaction — passing control in Bark 29, regression detector for Bark 30-style over-refinement;
8. Path Spiral -2 interaction — passing control in Bark 29, opposite-handed regression detector.

### Full factorial candidate dimensions

Every combination of the following finite diagnostic policies must be evaluated; no policy is production behavior in this update.

**A. Surface tangent policy**

1. `CurrentSafetyRelease` — TREE-TRUNK.2 persistence-independent tangent safety envelope.
2. `PersistenceLockedControl` — old persistence-aware tangent lock, retained only as a diagnostic control for topology-vs-readability tradeoff.
3. `StructuralTangent` — no root tangent lock, diagnostic upper bound on structural fidelity.

**B. Surface normal / azimuth policy**

1. `CurrentPersistentBlend` — current fixed-root-normal -> transported-normal blend using RootFrameEnvelope.
2. `TransportedOnly` — transported structural normal projected into the selected surface-tangent plane.
3. `FixedRootOnly` — tree-local +X projected into the selected surface-tangent plane.
4. `ParallelTransportPersistentBlend` — sequentially parallel-transport the previous resolved normal to the next selected surface tangent, then blend toward the current persistent root azimuth; tests whether independent per-ring reprojection is the correspondence defect.

**C. Root body radial basis**

1. `ResolvedSurfaceFrame` — current body radial basis.
2. `StructuralFrame` — root body radial uses the structural transported frame while the grounded foot remains unchanged.
3. `GroundAzimuthFrame` — root body azimuth remains tree-local around world/tree up, diagnostic bound on root-frame persistence independent of trunk lean.

**D. Sweep orthogonalization**

1. `None` — current emitted offset.
2. `BodyOffsetToLocalSweepPlane` — project only the non-foot body offset onto the plane perpendicular to the local center-to-center sweep.
3. `EntireOffsetToLocalSweepPlane` — project the complete emitted radial offset onto that plane; diagnostic only because this may disturb grounded-foot character.

**E. Ring correspondence policy**

1. `SameIndex` — production fixed side-index stitching.
2. `BestCyclicPerStrip` — evaluate every cyclic ring phase shift and select the topology-best phase independently per strip.
3. `PropagatedCyclic` — choose phase sequentially with continuity cost so ring phase changes remain globally coherent; tests whether correspondence rather than shape is the root cause.

**F. Longitudinal diagnostic subdivision**

1. `x1` — accepted Bark 29 sample density.
2. `x2` — diagnostic-only interpolation of every render span.
3. `x4` — diagnostic-only interpolation of every render span.

**G. Diagnostic radial resolution**

1. `x1` — production radial segments.
2. `x2` — diagnostic-only doubled radial samples; tests whether circumferential resolution can cure a candidate or merely hides/moves failures.

The full Cartesian product is 3 x 4 x 3 x 3 x 3 x 3 x 2 = 1,944 candidate configurations per targeted case, 15,552 candidate-case evaluations for the eight-case matrix.

### Candidate evidence contract

For every candidate-case evaluation record at minimum:

- topology pass/fail and unsafe strip count;
- minimum best-diagonal orientation score and strip/side location;
- contour self-intersection count;
- ring count, radial segments, estimated trunk vertex/triangle cost;
- maximum selected surface-tangent, normal, and binormal step;
- maximum structural-to-selected-surface tangent mismatch;
- tangent mismatch at/after earliest safe root transition;
- maximum same-side longitudinal backtracking against local centre sweep;
- minimum same-side longitudinal advance;
- maximum radial-direction angular change at corresponding sides;
- selected cyclic phase shift per strip and maximum/total phase motion for cyclic policies;
- maximum cross-section multiplier;
- maximum emitted-position deviation from current Bark 29 production geometry on the same diagnostic samples;
- root-foot displacement deviation from current Bark 29 geometry;
- number of diagnostic subdivision/radial samples and candidate evaluation milliseconds.

Aggregate each candidate configuration across all targeted cases with:

- pass count / fail count;
- whether every TREE-TRUNK.2 regression case passes;
- whether all previously passing controls remain pass;
- whether the known light-root external failure changes;
- worst topology score;
- worst tangent-contract violation;
- worst geometry deviation;
- worst root-foot deviation;
- total geometry-cost multiplier;
- total/mean evaluation time.

### Ranking / interpretation rules

The suite may calculate rankings for navigation but must not silently select a production winner. Hard rejection flags:

1. fails any previously passing control;
2. does not repair all TREE-TRUNK.2 regression cases;
3. violates persistence-independent tangent release at/after the earliest safe transition;
4. produces contour self-intersection;
5. materially displaces the grounded root foot;
6. exceeds existing production topology safety threshold.

Among non-rejected candidates, report Pareto-style tradeoffs for geometry deviation, added geometry cost, root-frame deviation, correspondence phase motion, and evaluation cost. Keep raw rows for every candidate so alternative weighting can be applied later without rerunning the suite.

### Output contract

Preserve the existing three TREE-TRUNK verification outputs and add matrix outputs from the same existing Inspector runner:

- candidate configuration summary CSV — one row per 1,944 configuration;
- candidate-case CSV — one row per 15,552 candidate-case evaluation;
- candidate worst-strip CSV — one row per candidate-case with the worst strip/side metrics;
- text report section listing search dimensions, total evaluations, hard-rejection counts by reason, best non-rejected candidates, single-factor effects, pairwise interactions, and candidate families that can be discarded.

The runner remains incremental and cancellable, updates progress/ETA, writes checkpoint rows as work completes, and performs bounded work per Editor update. No new Inspector action is required.

### Invariants / non-goals

- No production candidate is enabled by this patch.
- Bark stays version 29; Generator stays version 8.
- No change to production mesh output from the accepted TREE-TRUNK.2A diagnostic state.
- Do not alter topology thresholds to make candidates pass.
- Do not alter Lean/Bend equations, Generator endpoint solver, Path Spiral, Axial Twist, root contribution formulas, grounded foot, root contact radial boost, mixed-resolution stitching, or branch constraints.
- Candidate-only geometry may intentionally violate production contracts; the suite must report those violations rather than suppress them.
- The search is exhaustive over the finite candidate dimensions above, not a claim to enumerate every mathematically imaginable tree-meshing algorithm.

### Validation and audit

1. Static audit must prove production Bark geometry code is byte-equivalent to the accepted Bark 29 diagnostic baseline outside diagnostic-only helpers/telemetry access.
2. Existing 35-case baseline output must remain 31 PASS / 4 FAIL on the same operator/source before interpreting candidate results; any drift invalidates the matrix comparison.
3. Matrix row counts must equal 1,944 configurations and 15,552 candidate-case rows when complete.
4. CSV header/value parity and unique candidate IDs must pass static checks.
5. Search must remain Editor-responsive, cancellable, checkpointed, and expose progress/ETA.
6. Final source diff must contain only the three approved files.

### Status

1. Review — complete.
2. Canonical search plan — complete.
3. Bark 30 production experiment revert — complete by baselining from accepted Bark 29 diagnostic source.
4. Diagnostic candidate evaluator — pending.
5. Full-factorial runner/reporting — pending.
6. Static preservation/scope audit — pending.
7. Unity matrix execution — pending operator evidence.
8. Production repair selection — intentionally deferred until matrix evidence is reviewed.

### TREE-TRUNK.2B implementation result

- The rejected Bark 30 production sampling experiment is reverted by source selection: this patch is based on accepted Bark 29 TREE-TRUNK.2A diagnostic source. Bark remains 29; Generator remains 8.
- Existing production Bark methods are unchanged. The runtime source delta is diagnostic-only public option/result types plus diagnostic-only candidate evaluation helpers. Static method-body comparison confirms the accepted implementations of trunk refinement, topology preflight, unsafe-strip detection, base surface-frame resolution, both root envelopes, production trunk surface position, and grounded-foot radial resolution are byte-identical.
- Existing 35-case baseline definitions, per-case generation, measurement, invariant checks, and isolated-trunk setup are byte-identical. The runner now executes that baseline first and refuses to start the matrix unless it reproduces 31 PASS / 4 FAIL.
- The candidate matrix contains exactly 1,944 configurations and exactly eight target cases, for 15,552 candidate-case evaluations. It evaluates the full Cartesian product declared above.
- Each candidate-case records topology, worst strip/side orientation, contour self-intersections, longitudinal advance/backtracking, correspondence radial angle, phase shift/motion, surface-frame step metrics, earliest-transition tangent contract, cross-section multiplier, production-geometry deviation, grounded-foot deviation, geometry estimates, and evaluation time.
- The candidate configuration summary records hard-rejection reasons, repaired-regression count, passing-control regressions, known-light-root outcome, worst contract/deviation metrics, geometry cost, and evaluation cost. Completion reporting includes single-factor and all pairwise dimension interactions plus up to 20 non-rejected candidates sorted for navigation only.
- The reference candidate is the accepted Bark 29 production geometry contract. Before continuing beyond its eight rows, the scorer requires passing controls to PASS and the three TREE-TRUNK.2 regressions plus known light-root case to FAIL; any mismatch aborts the matrix as diagnostically invalid.
- All candidate behavior is diagnostic-only. No candidate is selected, enabled, serialized, cached, or applied to production geometry by this patch.
- Static CSV parity passes: candidate configuration summary 29/29 columns, candidate-case 37/37 columns, candidate worst-strip 17/17 columns.
- Static delimiter checks pass for both modified C# files. No rejected Bark 30 production sampling symbol/version remains. No deprecated API was introduced.
- Final scope audit: exactly the three approved project files differ from the accepted TREE-TRUNK.2A diagnostic baseline; no files were created, deleted, moved, or serialized.
- Unity compilation and execution of the 15,552-row matrix remain pending operator evidence.

### TREE-TRUNK.2B updated status

1. Review — complete.
2. Canonical search plan — complete.
3. Bark 30 revert — complete.
4. Diagnostic candidate evaluator — complete.
5. Full-factorial runner/reporting — complete.
6. Static preservation/scope audit — complete.
7. Unity baseline + matrix execution — pending operator evidence.
8. Production repair selection — intentionally deferred until the completed matrix is reviewed.

### TREE-TRUNK.2B staged-search runtime correction

Status: implementation approved after operator evidence showed the original 1,944-configuration / 15,552-evaluation full-factorial search projected an unacceptable multi-hour Editor runtime. This correction preserves the same diagnostic repair families and raw evidence contract, but replaces blind Cartesian enumeration with evidence-gated stages designed to finish within five minutes on the operator machine.

#### Objective and acceptance criteria

- Preserve the Bark 29 / Generator 8 production baseline and all diagnostic-only candidate geometry policies.
- Keep the existing 35-case production baseline and Bark-29 reference-candidate fail-closed checks.
- Stage A screens the production reference plus every single-factor alternative across all seven candidate dimensions against all eight targeted cases.
- Stage B retains at most one evidence-leading alternative per dimension, then evaluates every pairwise combination of those dimension leaders on a six-case interaction set containing all three TREE-TRUNK.2 regressions and three previously passing controls.
- Stage C builds a bounded set of merged multi-factor finalists from the best Stage-B pairs and re-evaluates them on all eight targeted cases.
- Hard-cap candidate-case work at 320 evaluations and enforce a 4 minute 30 second whole-suite search budget so the runner finishes or stops with explicit TIME_BUDGET_REACHED evidence before five minutes.
- Preserve raw candidate-case and worst-strip CSV rows for every actually evaluated configuration; report stage, screening disposition, and incomplete/time-budget status explicitly.
- Do not promote any candidate to production behavior in this update.

#### Search pruning rules

Stage-A alternatives are eligible to lead a dimension only when they preserve every previously passing targeted control, introduce no contour self-intersection, preserve the earliest-transition tangent contract and grounded-foot contract, and improve at least one regression signal relative to the Bark-29 reference: regression topology passes, total regression unsafe-strip count, or worst regression orientation score. If no alternative in a dimension improves the reference, that dimension is discarded from Stage B.

For each surviving dimension, retain the single best alternative ordered by regression pass count descending, regression unsafe-strip count ascending, worst regression orientation descending, geometry deviation ascending, then evaluation cost ascending. Stage B evaluates every pair of distinct surviving dimension leaders. Stage C retains up to four best Stage-B pairs under the same safety constraints, merges their non-default factor choices into unique compatible configurations, and evaluates at most eight unique finalists on the full eight-case set. Conflicting choices for the same dimension are not merged.

#### Bounded-work model

- Stage A: 15 configurations x 8 cases = 120 candidate-case evaluations maximum (reference plus 14 single-factor alternatives).
- Stage B: at most seven dimension leaders, therefore at most 21 pairs x 6 cases = 126 evaluations maximum.
- Stage C: at most eight finalists x 8 cases = 64 evaluations maximum.
- Candidate maximum: 310 evaluations; with 35 baseline cases the planned workload is 345 case/evaluation units, versus the rejected 15,587-unit run.
- A 320-candidate-evaluation hard cap protects against future dimension growth; a 270-second wall-clock cap protects the operator's five-minute budget independently of per-case cost.

#### Approved files and invariants

Keep the existing TREE-TRUNK.2B approved file scope. This runtime correction is expected to modify only the canonical tree handoff and focused trunk diagnostic suite; Bark diagnostic candidate geometry remains unchanged unless implementation review proves a required diagnostic contract change. No serialized assets, production geometry, topology thresholds, controls, Generator behavior, or Bark version change is authorized.

#### Validation

1. Static audit must prove Bark production/candidate geometry code is byte-identical to the pre-correction TREE-TRUNK.2B diagnostic state.
2. Unity must reproduce the 31 PASS / 4 FAIL production baseline before staged search begins.
3. The Bark-29 reference candidate must reproduce the expected targeted topology pattern.
4. The staged run must report its actual evaluated count, elapsed time, stage transitions, discarded dimensions, leaders, pairwise candidates, finalists, and whether the time/evaluation budget was reached.
5. Operator target: complete in under five minutes; if the 270-second guard fires, the partial outputs are still authoritative and the report must identify exactly which stages completed.

#### Staged-search implementation result

- The original full-factorial runner has been replaced by an evidence-gated Screening -> Pairwise -> Finalist state machine in the focused trunk diagnostic suite.
- Screening contains exactly 15 configurations: Bark-29 reference plus all 14 single-factor alternatives across the seven existing diagnostic candidate dimensions. All eight targeted cases are evaluated, for 120 candidate-case evaluations.
- Screening retains at most one leader per dimension. A leader must preserve all passing controls, contour validity, earliest-transition tangent contract, and grounded-foot contract, and must improve at least one regression signal versus Bark 29: regression pass count, regression unsafe-strip count, or worst regression orientation.
- Pairwise evaluates every pair of distinct surviving dimension leaders against six cases: all three TREE-TRUNK.2 regressions plus strong Lean+Bend/P0, heavy roots, and Path Spiral +2 controls. With seven leaders this is at most 21 configurations / 126 evaluations.
- Finalist selection ranks safe pairwise outcomes, retains the best four, adds compatible unique merges of those pairs, caps the finalist set at eight, and re-evaluates every finalist on all eight targeted cases. Maximum finalist work is 64 evaluations.
- Maximum planned candidate work is therefore 310 evaluations. A separate 320-evaluation hard guard catches future accidental search growth.
- The whole suite checks a 270-second wall-clock guard before each Editor tick's next baseline/candidate evaluation. If reached, it exits with `TIME_BUDGET_REACHED` and preserves partial report/CSV evidence rather than continuing toward the five-minute operator limit.
- Progress/ETA now identify the active stage, actual completed/planned candidate count, conservative projected remaining work, and remaining wall-clock budget.
- Candidate summary/case/worst-strip outputs now include stage and source provenance. Candidate aggregates additionally record regression unsafe-strip count and worst regression orientation so partial topology improvement can advance to interaction testing without requiring a single-factor candidate to solve every regression outright.
- The existing Bark diagnostic candidate evaluator is unchanged. Generator remains 8 and Bark remains 29; no production geometry behavior, topology threshold, serialized asset, control, or cache/version contract changes in this correction.

#### Staged-search static audit

- Planned-work proof: 15 x 8 = 120 screening evaluations; C(7,2) x 6 = 126 maximum pairwise evaluations; 8 x 8 = 64 maximum finalist evaluations; total = 310 <= 320 hard cap.
- CSV parity check: candidate configuration summary 34 fields / 34 emitted values; candidate-case 39 / 39; worst-strip 19 / 19.
- Lexical delimiter/state audit of the modified focused diagnostic passes with balanced parentheses, braces, and brackets outside comments/strings.
- SHA-256 comparison against the pre-correction TREE-TRUNK.2B reconstruction confirms `TreeBarkMeshGenerator.cs` and `TreeGenerator.cs` are byte-identical; the runtime production/candidate geometry implementation therefore did not change in this correction.
- Unity compilation and operator runtime remain pending. Acceptance requires the production baseline to reproduce 31 PASS / 4 FAIL, the Bark-29 reference candidate to reproduce the targeted reference topology pattern, and the staged search to terminate in under five minutes or hit its explicit 270-second evidence-preserving guard.

## TREE-TRUNK.2C — Ground-Azimuth Root Body Basis

Status: implementation approved from the completed TREE-TRUNK.2B staged candidate search. This update promotes only the winning single-factor `GroundAzimuthFrame` root-body basis from diagnostic candidate `C0007` into production Bark geometry. No other candidate factor is authorized.

### Objective

Repair the remaining strong Lean+Bend / Buttress Persistence trunk topology failures without changing the accepted Generator 8 endpoint/height contract or TREE-TRUNK.2 persistence-independent tangent release. Promote the smallest candidate proven by the staged search to repair all targeted regressions while preserving all targeted passing controls.

### Operator evidence and selection rationale

- Production baseline reproduced 35 completed cases with 31 PASS / 4 FAIL, matching the accepted Bark 29 state.
- The staged candidate search completed 170 / 170 planned candidate-case evaluations within its bounded search budget.
- Screening candidate `C0007` changed only `BodyBasisPolicy` from `ResolvedSurfaceFrame` to `GroundAzimuthFrame`. It passed 8 / 8 targeted cases, repaired all 3 TREE-TRUNK.2 regression cases, produced 0 regression unsafe strips, preserved all passing controls, passed the known light-root case in the candidate evaluator, preserved the earliest-transition tangent contract, produced 0 grounded-foot deviation, and retained 1.0x estimated geometry cost.
- Finalists that additionally changed normal policy and/or cyclic correspondence also passed 8 / 8 but did not improve the reported topology or geometry metrics over the ground-azimuth body-basis family. Those extra factors are therefore excluded from production promotion.
- The rejected Bark 30 longitudinal sampling experiment remains superseded and is not restored.

### Approved files

Modify only:

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeTrunkResponseDiagnosticSuite.cs`

Create/delete/move/serialized assets: none.

### Production contract

1. Keep Generator 8 unchanged.
2. Keep the current persistence-independent root tangent-safety envelope unchanged.
3. Keep the current persistence-aware surface-normal / root-frame blend unchanged.
4. Keep same-index ring stitching and current longitudinal/radial sampling unchanged.
5. Keep grounded root-foot radial resolution and anchoring unchanged.
6. For recipe-only trunk surface generation, resolve the root-body radial basis in the stable tree-local ground azimuth frame used by diagnostic candidate `C0007`, including authored trunk surface roll around tree-local up.
7. Preserve non-recipe/legacy trunk surface behavior unchanged.
8. Bump Bark algorithm version 29 -> 30 because emitted production trunk/root vertex positions change and cached/generated output must invalidate.
9. Do not promote transported-only normals, cyclic correspondence, sweep projection, extra longitudinal subdivision, extra radial resolution, structural tangent, persistence-locked tangent, or structural body basis.

### Implementation sequence

1. Add one private production helper that reproduces the `C0007` ground-azimuth root-body radial calculation from the diagnostic evaluator.
2. In recipe-only trunk surface position evaluation, use that helper for the root-body offset while leaving the existing grounded foot offset path unchanged.
3. Leave the legacy/non-recipe path on its existing resolved surface-frame radial calculation.
4. Update the focused verification suite/report identity to TREE-TRUNK.2C production verification without changing the established 35 cases or invariant thresholds.
   The prior TREE-TRUNK.2B staged candidate search remains compiled as diagnostic evidence code but is no longer auto-run by the focused production verification, because its Bark-29 reference assumptions are superseded once C0007 is promoted. Existing candidate CSV evidence must not be overwritten by the production verification run.
5. Run final source/scope/preservation audits and package only the approved changed files.

### Acceptance criteria

- Unity compilation: zero compiler errors attributable to this patch.
- Focused 35-case run: no TREE-TRUNK.2C regression in endpoint, authored height, or earliest-transition tangent contracts; targeted strong Lean+Bend P0.5/P1 and authored-root/P1 topology failures must pass.
- Candidate evidence predicts the light-root strong Lean+Bend case may also pass; treat that as a desirable result, not a weakened topology threshold.
- Previously passing Lean-only, Bend-only, moderate Lean+Bend, heavy-root, Twist, and Path Spiral cases remain PASS.
- No topology threshold change is permitted.
- After focused acceptance, run the 40-control response suite and production geometry-efficiency/topology audit as broad regression gates.
- Active-gameplay recurring cost remains unchanged; the basis change is deterministic mesh-build work with no added vertices/triangles by construction.

### Invariants / non-goals

- Do not change Lean Amount, Bend, Bend Frequency, Path Spiral, Axial Twist, root contribution formulas, root thickness/reach, root contact radial boost, mixed-resolution stitching, branch constraints, or the Generator 8 endpoint-preserving solver.
- Do not alter Buttress Persistence semantics or the TREE-TRUNK.2 tangent-safety release.
- Do not add controls, serialized data, dependencies, tags, layers, components, or Inspector actions.
- Do not retain rejected Bark 30 adaptive resolved-surface-tangent sampling behavior.
- Do not remove the TREE-TRUNK.2B candidate evaluator in this patch; it remains diagnostic evidence unless a later cleanup is separately approved.

### Performance model

The promoted C0007 factor changes only the basis used to resolve the existing recipe-only root-body radial vector. It adds no sample, ring, radial segment, triangle, texture, shader, per-frame update, or runtime allocation contract. Candidate evidence reported geometry-cost multiplier 1.0 and estimated vertex/triangle counts unchanged relative to Bark 29. Actual Unity generation/build timing remains a post-implementation validation item.

### Implementation result and delivery-side evidence

- Bark algorithm version advances 29 -> 30. Recipe-only production trunk surface generation now replaces only the root-body radial basis with the ground-azimuth basis proven by candidate `C0007`; the existing grounded-foot radial remains additive and unchanged. Legacy/non-recipe output remains on the pre-existing resolved surface frame.
- The production helper uses tree-local up, canonical +X ground azimuth, and authored trunk-surface roll around tree-local up, matching the `GroundAzimuthFrame` candidate equation. No normal-policy, tangent-policy, sweep-projection, correspondence, longitudinal-subdivision, or radial-resolution candidate factor is promoted.
- The focused verification runner now executes only the established 35 production cases. The superseded staged search implementation remains compiled for historical/diagnostic evidence but is not auto-run and its prior candidate CSV evidence is not overwritten.
- Exact source-scope comparison against the accepted staged-search baseline reports 3 modified files, 0 additions, and 0 deletions.
- Generator 8 is byte-identical to the accepted staged-search baseline. `ResolveTrunkBaseSurfaceFrame`, `ResolveTrunkSurfaceFrame`, `EvaluateRootTangentSafetyEnvelope`, `EvaluateRootFrameEnvelope`, `EvaluateTrunkRootContributions`, and `ResolveGroundAnchoredRootFootRadial` are byte-identical. The established `BuildCases`, `RunCase`, summary writer, and sample writer are also byte-identical.
- Lexical delimiter checks pass for both modified C# files. Unity compilation/runtime and measured generation timing remain pending operator evidence.

### Status

1. Review — complete.
2. Canonical production plan — complete.
3. Production Bark implementation — complete.
4. Focused verification runner update — complete.
5. Static preservation/scope audit — complete.
6. Unity focused validation — pending operator evidence.
7. Broad control/geometry/performance regression — pending focused acceptance.


## TREE-TRUNK.2D — Axial-Twist Validator Semantics Audit

Status: implementation approved as a diagnostic-only audit after TREE-TRUNK.2C production geometry looked visually coherent while the focused suite failed 22/35 cases almost entirely on the absolute axial-twist validator.

### Objective

Determine whether TREE-TRUNK.2C introduced visually/materially incorrect axial roll, or whether the existing recipe-only axial-twist validator is measuring a frame-convention offset that is no longer equivalent to authored twist after promotion of the ground-azimuth root-body basis.

### Approved files

Modify only:

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeTrunkResponseDiagnosticSuite.cs`

Create/delete/move/serialized assets: none.

### Reviewed evidence

- TREE-TRUNK.2C operator run completed 35 cases with 13 PASS / 22 FAIL. Nearly every Bend/Lean+Bend/Path-Spiral case failed before topology/contract measurement because `MeasureGeneratedTrunkAxialTwist` reported a non-zero absolute roll offset even when requested twist was zero.
- The same run measured approximately +5.888 degrees for the zero-twist moderate Lean+Bend case, +409.748 degrees at requested +400, and -397.414 degrees at requested -400. Relative to the zero-twist baseline, the control response is therefore approximately +403.860 / -403.302 degrees, close to the authored +/-400-degree deltas even though the absolute zero reference moved.
- Current recipe-only `MeasureGeneratedTrunkAxialTwist` resolves the old zero-roll surface frame per ring, projects emitted side-0 radial into that tangent plane, and accumulates the signed angle from that old-frame normal. TREE-TRUNK.2C deliberately emits the root-body radial from the ground-azimuth basis, so that absolute measurement now includes frame-convention/curvature offset.
- `TreeControlResponseSuite` already validates Axial Twist response by comparing measured twist between control anchors rather than requiring the absolute measured value to equal the authored value.

### Diagnostic contract

1. Keep Generator 8 and Bark 30 production geometry unchanged.
2. Keep the existing production absolute axial-twist rejection unchanged for normal builds in this audit.
3. Add a diagnostic-only Bark build path that records the same requested/measured absolute twist but can bypass only the `abs(measured-requested) > 2 degrees` early rejection so the remaining production topology audit can execute.
4. The focused 35-case suite must retain the normal production build result, but when failure is solely the absolute axial-twist gate it must rerun the same generated tree through the diagnostic-only bypass and report whether the mesh would otherwise pass topology and TREE-TRUNK.2 endpoint/height/tangent contracts.
5. Add a bounded differential-twist audit over representative curved trunks. For each structural/root configuration, generate twist -400, 0, +400 with identical non-twist controls and compare measured deltas relative to that configuration's zero-twist baseline.
6. Report absolute zero-reference offset, +400/-400 differential response, differential error from authored +/-400, bypassed topology result, and structural fingerprint invariance. Do not change production validation thresholds in this patch.
7. Keep the run short; the differential audit is a small fixed matrix and must not restore the superseded exhaustive/staged candidate search.

### Acceptance / decision criteria

- If zero-twist absolute offsets are non-zero but +400/-400 differentials remain near +/-400 across representative Bend, strong Lean+Bend, and Path-Spiral configurations, and bypassed meshes pass production topology, classify the old absolute twist gate as stale for the TREE-TRUNK.2C frame convention.
- If differential response itself materially deviates from authored twist, structural fingerprints change with twist, or bypassed meshes still fail topology, classify TREE-TRUNK.2C geometry as genuinely incorrect and do not weaken the validator.
- No production validator change is authorized by this audit; any validator replacement requires a separate evidence-backed production patch and explicit approval.

### Invariants / non-goals

- No root-body basis change, tangent-release change, Persistence change, root-foot change, ring-stitching change, sampling change, Lean/Bend/Path-Spiral change, or Generator change.
- No topology threshold change.
- No serialized data, controls, Inspector actions, dependencies, tags, layers, or components.
- Do not treat visual appearance alone as proof; the decision must combine differential twist response with topology and structural invariance.

### TREE-TRUNK.2D implementation result and static audit

- Added a diagnostic-only Bark build entry point that temporarily bypasses only the absolute `abs(measured-requested) > 2 degrees` axial-twist rejection and restores the prior thread-local state in `finally`. Normal `Build` behavior remains unchanged when that entry point is not used.
- The existing recipe-only absolute measurement equation, ground-azimuth body basis, tangent/root-frame equations, root contributions, grounded foot, topology thresholds, stitching, and sampling are unchanged.
- The focused suite keeps the normal TREE-TRUNK.2C production build as the primary result. When and only when that build fails on the absolute axial-twist gate, it reruns the identical generated definition through the diagnostic bypass, records requested/measured/absolute-error telemetry, and reports the unchanged downstream topology and Lean/Bend contract outcome separately.
- Added a fixed 18-build differential semantics matrix: six curved scenarios x twist {-400, 0, +400}. The report records each scenario's zero-twist frame offset, measured +/-400 delta relative to that zero baseline, differential error, topology of all three bypass builds, and trunk/branch structural fingerprint invariance.
- The additional diagnostic workload is bounded at 18 builds after the existing 35-case run; no exhaustive candidate search is re-enabled.
- Generator remains version 8 and Bark remains version 30. SHA-256 comparison confirms the Generator source is unchanged from the accepted TREE-TRUNK.2B reconstruction. Source comparison confirms the production trunk surface-position equation, TREE-TRUNK.2C ground-azimuth body-basis helper, grounded root-foot helper, base/rolled surface-frame equations, both root envelopes, and `MeasureGeneratedTrunkAxialTwist` itself are unchanged.
- Lexical delimiter checks pass for both modified C# files. No project-side compiler is available in the supplied extracted workspace, so Unity compilation and runtime evidence remain pending.

### TREE-TRUNK.2D decision gate

Run the focused diagnostic once. The resulting report is sufficient for the next decision:

- If cases blocked by the absolute gate become topology/contract PASS under the diagnostic bypass and the six differential triplets preserve structure with measured deltas near +/-400, the absolute recipe-only twist gate is stale under the TREE-TRUNK.2C frame convention and should be replaced by a baseline-relative/differential contract in a separately approved production patch.
- If bypassed topology/contract still fails or the differential triplets do not preserve authored roll response, TREE-TRUNK.2C geometry remains invalid and must be corrected rather than weakening validation.

## TREE-TRUNK.2E — Axial-Twist Validator Reference Repair

Status: implementation approved after TREE-TRUNK.2D proved that TREE-TRUNK.2C production geometry passes downstream topology/Lean-Bend contracts when the stale absolute axial-twist gate is bypassed, while differential Axial Twist response remains close to authored values on ordinary Bend and moderate Lean+Bend cases.

### Objective

Replace the recipe-only absolute axial-twist validator's old transported-surface-frame reference with the zero-roll reference implied by the TREE-TRUNK.2C ground-azimuth body-frame convention. Keep production geometry unchanged and retain the differential semantics audit as a cross-check.

### Approved files

Modify only:

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeTrunkResponseDiagnosticSuite.cs`

Create/delete/move/serialized assets: none.

### Reviewed evidence

- TREE-TRUNK.2D operator evidence: 22 normal production cases failed only at the absolute axial-twist gate; every one of those same generated trees passed the diagnostic bypass build, unchanged downstream topology audit, and Lean/Bend contract checks.
- The old recipe-only `MeasureGeneratedTrunkAxialTwist` projects side-0 emitted radial into the resolved surface-tangent plane and accumulates signed angle from the old zero-roll surface normal. That frame convention no longer matches TREE-TRUNK.2C recipe-only output, whose body radial is authored in a ground-azimuth basis around tree-local up.
- TREE-TRUNK.2D differential evidence: Bend 0.50 and Bend 1.00 respond within about 0.6 degrees of authored +/-400; moderate Lean+Bend is within about 3.9 degrees; stronger curved/spiralled cases remain topology PASS but the old absolute measurement becomes increasingly contaminated by the frame-reference mismatch.
- The generic control-response suite already treats Axial Twist as a response/delta contract and requires structural trunk/branch fingerprints to remain invariant.

### Production contract

1. Keep Generator 8, Bark 30 production vertex positions, TREE-TRUNK.2C ground-azimuth body basis, tangent release, Persistence frame blending, root-foot anchoring, stitching, sampling, and topology thresholds unchanged.
2. For recipe-only axial-twist measurement, use tree-local up as the measurement axis and canonical tree-local +X as the zero-roll radial reference, matching the basis used by `ResolveGroundAzimuthRootBodyRadial`.
3. Measure the emitted side-0 radial after removing any vertical component, unwrap signed angle around tree-local up ring-to-ring, and preserve the existing tip-closure authored-roll compensation.
4. Keep the existing absolute requested-vs-measured tolerance unchanged. This patch changes the reference, not the threshold.
5. Legacy/non-recipe axial-twist measurement remains unchanged.
6. Retain the TREE-TRUNK.2D bypass and differential audit temporarily as verification evidence; normal production builds remain authoritative and should no longer need the bypass when the new reference is correct.

### Acceptance criteria

- The established 35-case focused suite should complete without false absolute-twist rejections; expected outcome is 35/35 PASS if no independent topology/contract defect remains.
- Zero-twist Bend, Lean+Bend, root-interaction, and Path-Spiral cases should measure near zero under the repaired reference.
- Twist -400 / 0 / +400 differential triplets should remain structurally invariant and report response close to authored +/-400; any remaining material differential error must be reported rather than hidden by threshold changes.
- Production mesh positions, normals, tangents, topology, vertex/triangle counts, and structural fingerprints must be byte/measurement-equivalent to TREE-TRUNK.2D for identical controls.

### Non-goals

- No geometry/frame/basis change.
- No tolerance increase or topology-threshold change.
- No Lean/Bend/Path-Spiral equation change.
- No control/schema/default/serialized-data change.
- No removal of the differential audit until the repaired production validator is operator-validated.

### Performance model

The recipe-only validator changes only the reference axis used while walking existing trunk rings after mesh emission. Complexity remains O(number of trunk rings), with no additional geometry, allocations, per-frame work, or runtime gameplay cost. The differential audit remains Editor-only and fixed-size.

### Implementation sequence

1. Replace only the recipe-only branch of generated axial-twist measurement with ground-azimuth-reference accumulation; leave legacy measurement untouched.
2. Update focused diagnostic/report labels and decision text to TREE-TRUNK.2E while retaining the bypass/differential cross-check.
3. Audit production geometry helpers and emitted surface equations against TREE-TRUNK.2D; they must remain unchanged.
4. Run static scope/schema/lexical checks and package only the approved files.
5. Unity focused validation remains pending operator evidence.

### TREE-TRUNK.2E implementation result and static audit

- Recipe-only generated axial-twist measurement now uses canonical tree-local +X as the zero-roll radial reference and tree-local up as the signed-angle axis. The emitted side-0 radial is projected into the ground plane and unwrapped ring-to-ring. This matches the TREE-TRUNK.2C production body-frame convention and removes transported-surface-frame curvature from the authored-roll measurement.
- The existing requested-vs-measured absolute tolerance remains 2 degrees. Legacy/non-recipe measurement remains unchanged. Existing terminal-tip closure compensation remains unchanged.
- Bark remains version 30 because production vertex positions/topology are unchanged; this patch repairs validation semantics rather than geometry/cache determinism.
- The TREE-TRUNK.2D diagnostic bypass and 18-build differential audit remain active as cross-checks. Report/status labels now identify TREE-TRUNK.2E validation.
- Source-preservation audit against TREE-TRUNK.2D confirms `EvaluateTrunkSurfacePosition`, `ResolveGroundAzimuthRootBodyRadial`, `ResolveGroundAnchoredRootFootRadial`, `ResolveTrunkBaseSurfaceFrame`, `ResolveTrunkSurfaceFrame`, `EvaluateRootTangentSafetyEnvelope`, `EvaluateRootFrameEnvelope`, and `EvaluateTrunkRootContributions` are byte-identical. Only the recipe-only branch of `MeasureGeneratedTrunkAxialTwist` changes in production code.
- Lexical delimiter counts are balanced in both modified C# files. Generator remains version 8. No Unity compiler/runtime is available in the extracted workspace; focused operator validation remains pending.

### TREE-TRUNK.2E status

1. Review — complete.
2. Canonical plan — complete.
3. Validator reference repair — complete.
4. Diagnostic-label/cross-check update — complete.
5. Static scope/preservation audit — complete.
6. Unity focused validation — pending operator evidence.
7. Broad control/geometry/performance regression — pending focused acceptance.


## TREE-TRUNK.2F — Production Gallery Closure Audit

Status: implemented in source; static scope/preservation audit passed. Unity compilation and the single twenty-tree operator run are pending.

### Objective

Close TREE-TRUNK.2 with one compact production-relevant regression audit instead of running the generic 40-control matrix or the three-policy geometry tournament. The audit must fresh-generate and production-build each of the twenty curated procedural comparison trees exactly once, proving that Generator 8 and Bark 30 remain valid across the real curated gallery while recording the geometry-cost evidence needed to detect an accidental regression.

### Approved files

Modify only:

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/Editor/ProceduralTreeInstanceEditor.cs`

Create only:

- `Assets/Game/Procedural/Trees/Editor/TreeTrunkProductionClosureAudit.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeTrunkProductionClosureAudit.cs.meta`

No production generator, bark, settings, recipe, scene, prefab, material, shader, layer, tag, component, or serialized gallery asset changes are authorized.

### Reviewed evidence

- TREE-TRUNK.2E operator evidence completed the focused structural/root-frame contract at 35 / 35 PASS with zero topology failures. The repaired axial-twist reference measured `-400 / 0 / +400` to essentially machine precision across Bend, strong Lean+Bend, and Path Spiral +/-2.
- The existing 40-control response suite is intentionally not the next gate: it executes four samples for every exposed creative control across four representatives, spending most of its work on foliage, branch, palette, and unrelated controls that TREE-TRUNK.2 did not modify.
- The existing geometry-efficiency audit is intentionally not the next gate: it executes every gallery tree under Current, LegacyCurrent, and RadialAggressive policies and includes silhouette-capture comparison machinery. TREE-TRUNK.2 needs only Current production topology/cost evidence.
- The accepted prior twenty-tree Production Current aggregate recorded in this document is `131252` vertices and `206391` triangles. The subsystem still has no hard complete-tree geometry budget, so the closure audit must report aggregate delta rather than invent a new pass/fail budget.
- TREE-TRUNK.2C changed the recipe-only root-body basis but was designed to add no samples, rings, radial segments, vertices, or triangles by construction. TREE-TRUNK.2E changed validation semantics only.

### Audit contract

1. Locate the complete procedural comparison-gallery root beneath the selected tree's gallery and require exactly twenty `ProceduralTreeInstance` targets.
2. Require every target to expose an initialized recipe-only exact-control snapshot.
3. For each target, run one fresh `TreeGenerator.GenerateExactForValidation(...)` call from its current exact controls, seed, source identity, and family.
4. Build one temporary bark mesh from that fresh definition with the Current recipe-only production settings plus geometry-audit telemetry only. No Legacy, aggressive, conservative, screenshot, capture, or candidate policy is run.
5. Require fresh generation PASS, bark build PASS, and topology audit PASS. The normal Bark 30 axial-twist validator remains authoritative and may not be bypassed.
6. Record per tree: family/variant/recipe identity, fresh-vs-serialized structural fingerprint parity, requested/measured/error axial twist, vertices, triangles, estimated mesh bytes, render-ring/root-zone telemetry, build time, and existing generated-mesh count parity when a comparable stored definition/mesh is available.
7. Report aggregate vertices/triangles/estimated mesh bytes and delta versus the accepted `131252 / 206391` Production Current aggregate. This delta is evidence only because no canonical hard complete-tree geometry budget exists.
8. Run incrementally at one tree per Editor update, remain cancellable, display progress/ETA, checkpoint report/CSV output, and perform no scene or asset mutation.

### Acceptance criteria

- Exactly 20 / 20 curated procedural targets are discovered.
- 20 / 20 fresh structural generations pass.
- 20 / 20 Current bark builds pass the normal production validator and complete topology audit.
- No target reports a nonzero Bark axial-twist error outside the existing production tolerance.
- The report includes aggregate geometry and explicit delta versus `131252 / 206391`; any material increase is surfaced for review rather than hidden by an invented threshold.
- Existing generated-mesh count parity is reported only when meaningful; stale/missing stored gallery output must be identified rather than treated as proof of a production-code failure.
- No additional stress matrix is run: the accepted TREE-TRUNK.2E 35-case suite already covers strong Lean/Bend, Persistence extremes, light/authored/heavy roots, axial twist, and Path Spiral. Repeating those cases would add runtime without closing a new uncertainty.

### Invariants / non-goals

- Do not modify Generator 8, Bark 30, TREE-TRUNK.2C geometry, TREE-TRUNK.2E axial-twist measurement, topology thresholds, efficiency policies, control domains, recipes, or exact-control values.
- Do not run or embed the generic 40-control suite as part of this audit.
- Do not run LegacyCurrent, RadialAggressive, RadialConservative, or silhouette captures.
- Do not regenerate or commit gallery meshes.
- Do not add a new gameplay/runtime path or recurring work.

### Implementation sequence

1. Add the bounded twenty-tree Editor-only closure runner with TXT/CSV checkpoint output.
2. Add one Inspector section with run/cancel/progress/copy/open controls and mutual-exclusion guards against the existing long-running tree diagnostics.
3. Perform source-preservation and scope audit: Generator and Bark must be byte-identical to TREE-TRUNK.2E; only the approved Editor/document files may differ.
4. Run available static checks and package the approved changed files. Unity compilation and the single twenty-tree operator run remain the live validation gate.

### Risks and mitigations

- **Stale gallery output mistaken for code regression:** fresh generation/build is authoritative; stored fingerprint/count parity is labeled separately and only interpreted when comparable.
- **Audit creep:** exactly one Current build per target; no policy tournament, control sweep, screenshots, or duplicate TREE-TRUNK.2E stress cases.
- **Editor stall:** one case per Editor update with cancellation, ETA, and partial output.
- **Hidden geometry-cost regression:** record per-tree and aggregate vertices/triangles/bytes plus accepted-aggregate delta, while avoiding an unsupported hard budget.

### Validation plan

- Unity compile after installing the patch.
- Run the new TREE-TRUNK.2 production closure audit once from a procedural gallery tree.
- Acceptance target: 20 / 20 generation PASS, 20 / 20 production bark/topology PASS; then review the reported aggregate geometry delta and any stale stored-gallery parity warnings.


### TREE-TRUNK.2F implementation result and static audit

- Added one Editor-only incremental closure runner that discovers exactly twenty procedural gallery instances and executes one fresh exact-control generation plus one Current recipe-only bark build per tree.
- The bark build uses the production recipe-only settings with geometry-audit telemetry enabled; it does not run LegacyCurrent, conservative/aggressive policies, silhouette capture, screenshots, candidate search, or the already-accepted TREE-TRUNK.2E stress matrix.
- Core case PASS requires fresh generation PASS, normal Bark 30 build PASS, and complete topology-audit PASS. The TREE-TRUNK.2D axial-twist bypass is not used.
- Per-tree TXT/CSV evidence includes structural fingerprint parity, normal axial-twist requested/measured/error values, vertices/triangles/estimated payload, build/topology time, trunk/root sampling telemetry, and stored generated-mesh count parity when the stored structure is actually comparable.
- Aggregate reporting records current vertices/triangles/estimated payload and, only after all twenty cases complete, the delta against the accepted `131252 / 206391` aggregate. No unsupported complete-tree pass/fail geometry budget was introduced.
- The Inspector now exposes one dedicated TREE-TRUNK.2 closure section with run/cancel/progress/ETA/copy/open controls. Existing long-running tree diagnostics are mutually excluded while the closure audit is active.
- Generator 8, Bark 30, and the TREE-TRUNK.2E focused diagnostic suite are byte-identical to the accepted pre-2F state. SHA-256 preservation checks passed for all three files.
- Final source scope is exactly the canonical document, the procedural-tree Inspector, the new closure-audit source, and its script meta file. No production geometry/settings/runtime/serialized asset changed.
- Lexical delimiter checks pass for both modified C# files. The new audit statically contains one Current policy reference and no LegacyCurrent/RadialAggressive/AxialAggressive policy execution, no silhouette/capture API, and no axial-twist validator bypass.
- No C# compiler or Unity runtime is available in the extracted workspace. Unity compilation and the single twenty-tree operator run remain pending.

### TREE-TRUNK.2F decision gate

Run only this closure audit next. If it reports 20 / 20 fresh generation + production bark/topology PASS, review the aggregate geometry delta and stored-gallery parity warnings. Do not run the generic 40-control or three-policy geometry suites merely to close TREE-TRUNK.2; use them later only when a separate change creates uncertainty in the controls or policy comparisons they uniquely test.

## TREE-TRUNK.2G — Twisted-1 Render-Ring Centre Collapse Diagnostic

Status: implementation approved as a diagnostic-only follow-up after curated gallery regeneration exposed one Bark topology failure on the deterministic Twisted 1 production slot. No production geometry change is authorized in this patch.

### Objective

Reproduce the exact `Wych Elm Leaning` / seed `1617923258` production structure once and run exactly one normal recipe-only Bark build while capturing the render-ring centreline at the points where Bark transforms source branch samples. Identify the exact branch and pipeline stage that first creates the single zero-length consecutive render-ring-centre segment reported by the production topology audit.

### Approved files

Modify only:

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/ProceduralTreeInstanceEditor.cs`

Create only:

- `Assets/Game/Procedural/Trees/Editor/TreeTrunkRingCollapseDiagnostic.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeTrunkRingCollapseDiagnostic.cs.meta`

No Generator algorithm, Bark geometry equation, Bark topology threshold, recipe/control value, scene, prefab, material, mesh asset, layer, tag, or serialized gallery object change is authorized.

### Reviewed evidence

- Operator curated-gallery regeneration completed deterministic structural generation for all twenty slots, but Twisted 1 alone failed Bark topology. Its sole reported topology defect was `zero-length ring segments: 1`; invalid indices, degenerate triangles, finite-stream failures, side/cap orientation failures, non-manifold edges, embedded-root matching, and unexpected boundary loops were all zero/clean.
- Current Bark code increments the zero-length counter from consecutive final `RenderSample.Position` values, not circumference-edge vertex lengths. The audit therefore proves one final Bark render-ring centre pair coincides within the generator epsilon.
- `BuildRenderSamples` is the first Bark stage that can move branch centres: for child branches it applies branch-root transition inset/root-side clamping, then rebuilds transported frames and curvature-radius safety.
- Trunk render samples can subsequently be refined and tip-trimmed. Non-trunk samples can subsequently be reduced by adaptive circular sampling and repaired by collapsed-strip removal. The diagnostic must capture these existing stages from the actual production build rather than reimplementing them in a second geometry path.
- The failing gallery build samples exact controls directly from the mapped curated recipe at seed `1617923258`, then generates with the recipe-only exact-control path and builds Bark with `CreateRecipeOnlyDefaults()`. The diagnostic must reproduce those inputs directly and must not depend on a stale stored gallery instance.

### Diagnostic contract

1. Resolve the current curated recipe catalog and the Twisted variant-1 stable recipe identity; require the mapped recipe to resolve to `Wych Elm Leaning`.
2. Sample a fresh exact-control snapshot from that recipe at seed `1617923258` and run exactly one structural generation using the recipe-only exact-control generator path.
3. Run exactly one normal Bark 30 build with recipe-only production settings. Do not bypass topology, axial-twist validation, or any production repair/reduction stage.
4. During that same Bark build, capture consecutive centre distances for every branch at: source samples before Bark adjustment, immediately after `BuildRenderSamples`, after trunk refinement when applicable, after trunk tip preparation when applicable, after non-trunk efficiency reduction when applicable, and after non-trunk topology-collapse removal/final render-sample preparation.
5. Report every zero-length pair at each captured stage and identify the first stage where a pair appears. For the final failing pair report stable branch ID, branch order, parent index, parent attachment distance, local reference axis, sample indices, normalized distances, positions, radii, tangents/normals/binormals, previous/next neighbour distances, and source-vs-render positional deltas where a same-index source sample exists.
6. Preserve the normal Bark build result and full topology report beside the diagnostic evidence. The diagnostic itself may not mutate any scene, asset, recipe, generated mesh, or stored exact controls.

### Acceptance criteria

- Exactly one fresh structure generation and one normal production Bark build are executed.
- The normal Bark build reproduces `ZeroLengthRingSegmentCount == 1`; if it does not, report the mismatch and stop diagnosis rather than guessing.
- The report identifies one exact branch/sample pair and the earliest Bark pipeline stage at which the pair becomes zero-length.
- Source-stage evidence explicitly determines whether the duplicate already existed in Generator output.
- No production geometry, topology threshold, sampling/reduction policy, or version number changes.

### Implementation sequence

1. Add diagnostic-only stage capture around the existing Bark render-sample pipeline; capture data only while the dedicated diagnostic entry point is active.
2. Add one Editor-only one-shot runner that resolves the current curated Twisted-1 recipe, resamples the exact failing seed, performs one generation plus one normal Bark build, and writes/copies a focused TXT report.
3. Add one compact Inspector action and copy/open controls, mutually excluded with existing long-running tree diagnostics.
4. Audit source preservation against TREE-TRUNK.2F: production Bark equations and normal build behavior must be unchanged when diagnostic capture is inactive; Generator 8 remains untouched.
5. Run static scope/lexical checks and package only the approved files. Unity compilation and one operator diagnostic run remain the live validation gate.

### Risks and mitigations

- **False reproduction from stale gallery state:** resolve recipe + seed directly and resample exact controls; do not reuse stored instance controls.
- **Diagnostic reimplementation diverges from production:** instrument the actual `AppendBranchTube` render-sample pipeline during the one normal Bark build rather than creating a second Bark geometry algorithm.
- **Instrumentation changes normal builds:** use thread-static diagnostic capture that is null outside the dedicated entry point and audit production output code for byte-equivalent geometry equations.
- **Over-testing:** one structural generation and one Bark build only; no gallery rerun, 40-control suite, efficiency policy tournament, or unrelated stress matrix.

### Validation plan

- Unity compile after installing the patch.
- Run the dedicated Twisted-1 ring-collapse diagnostic once.
- Acceptance target: reproduce the one zero-length render-ring-centre pair and identify its stable branch/sample indices plus first introducing Bark stage; submit the complete focused report for the production-fix decision.

### TREE-TRUNK.2G implementation result and static audit

- Added a thread-static diagnostic capture that is null during ordinary Bark builds. The dedicated wrapper calls the existing normal `Build(...)` exactly once and only records source/render-sample centreline state while that build executes.
- Stage capture records Generator source samples, post-`BuildRenderSamples`, post-trunk refinement, post-trunk tip preparation, post-branch efficiency reduction, post-branch topology-collapse removal, and final render samples. Every pair tested uses the exact same squared-distance threshold as the production zero-length counter.
- Duplicate detail includes branch identity/order/parent/attachment/reference axis, pair indices, normalized distances, positions, radii, transported frames, neighbouring centre distances, and nearest/same-index source-position deltas. The summary ranks pipeline stages independently of branch iteration order so a later branch's earlier-stage duplicate cannot be mislabeled as a later-stage origin.
- Added one Editor-only synchronous runner hard-targeted to curated Twisted variant 1 / `Wych Elm Leaning` / seed `1617923258`. It resolves the current catalog without assigning or dirtying gallery state, samples one fresh exact-control snapshot, runs one recipe-only `TreeGenerator.Generate(...)`, then one normal Bark build with `CreateRecipeOnlyDefaults()` through the diagnostic wrapper.
- The runner does not call the axial-twist bypass, tournament/unsafe preview builders, gallery mesh builder, Undo, `SetDirty`, or any scene/asset mutation API. It writes one TXT report, copies it to the clipboard, and classifies the run as `REPRODUCED` only when the normal Bark build fails with a topology result containing exactly one zero-length ring segment.
- Added one compact Inspector section with run/copy/open actions. The run action is disabled while the existing long-running tree generation/audit/diagnostic jobs are active.
- Scope audit reports exactly five changed files: the canonical document, Bark source, procedural-tree Inspector, new diagnostic source, and its script meta. Generator 8, Bark topology audit, Bark settings, and the accepted focused/closure diagnostic sources remain unchanged.
- Bark algorithm version remains 30. The Bark diff contains diagnostic declarations/wrapper/callback additions only; no existing production geometry equation, topology condition, repair/reduction policy, or threshold line is replaced or deleted.
- Lexical delimiter checks pass for all three modified/created C# sources. Static scan confirms exactly one generator call in the one-shot runner, exactly one diagnostic Bark-wrapper call there, and exactly one normal `Build(...)` call inside that wrapper.
- No C# compiler or Unity runtime is available in the extracted workspace. Unity compilation and the single operator diagnostic run remain pending.

### TREE-TRUNK.2G decision gate

Run only the Twisted-1 ring-collapse diagnostic next. If it reproduces exactly one final zero-length ring-centre pair, use the reported branch/sample/stage evidence to design the smallest production repair. If reproduction mismatches, do not patch Bark from inference; reconcile the diagnostic inputs with the live curated-gallery generation state first.

## TREE-TRUNK.2H — Dense-Root Boundary Spacing Repair

Status: implementation approved; plan recorded before code changes. Unity compilation and the exact Twisted-1 verification run remain pending.

### Objective

Repair the single Twisted-1 Bark topology failure by preventing trunk refinement from inserting the non-legacy dense-root-domain boundary sample when that interpolated boundary lies within Bark's existing zero-length ring-centre threshold of one of the source span endpoints. Preserve the semantic dense/non-dense boundary by assigning the whole source span to the side whose endpoint is not separated from the boundary by a topology-valid distance.

### Approved files

Modify only:

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeTrunkRingCollapseDiagnostic.cs`

Create/delete/move/serialized assets: none.

### Reviewed evidence

- The exact `Wych Elm Leaning` / seed `1617923258` diagnostic reproduces Generator 8 PASS followed by Bark 30 FAIL with exactly one zero-length render-ring-centre segment.
- Generator source samples and post-`BuildRenderSamples` trunk samples contain zero zero-length pairs. The first and only pair appears immediately after trunk refinement on trunk stable branch `683310912`, pair `28 -> 29`.
- The failing pair is at normalized distances `0.222178 -> 0.222222` with physical centre distance `0.000552713 m`. Its neighbouring distances are approximately `0.099744 m` before and `0.200126 m` after.
- `RefineTrunkRenderSamples` explicitly interpolates and inserts the non-legacy dense-root sampling boundary whenever one source span straddles that normalized boundary. It currently checks normalized straddling only; it does not check whether the interpolated boundary is within the existing Bark zero-length centre threshold of either source endpoint.
- Final Bark branch accounting counts a zero-length centre segment when consecutive `RenderSample.Position` values have squared distance `<= Epsilon`, where Bark `Epsilon` is `0.000001f` (1 mm physical distance threshold). The observed `0.552713 mm` inserted pair is therefore guaranteed to fail the unchanged topology contract.
- `AppendRefinedTrunkSpan` adds its terminal sample and all ordinary subdivisions uniformly over the supplied span; no production evidence implicates Generator output, root/body radial deformation, topology thresholds, or non-trunk sampling.

### Production contract

1. Keep Generator 8, TREE-TRUNK.2C ground-azimuth root/body geometry, TREE-TRUNK.2E axial-twist reference, topology thresholds, dense-root normalized boundary, adaptive refinement limits, branch sampling, root feet, and all authored controls unchanged.
2. When a non-legacy source span straddles `denseRootSamplingEnd`, interpolate the existing boundary sample exactly as before.
3. If the interpolated boundary is topology-validly separated from both source endpoints (`sqrDistance > Epsilon`), retain the existing two-span split and inserted-boundary-ring behavior unchanged.
4. If the boundary lies within `Epsilon` of the lower endpoint only, do not insert a boundary ring; refine the original source span once using the post-boundary/non-dense classification.
5. If the boundary lies within `Epsilon` of the upper endpoint only, do not insert a boundary ring; refine the original source span once using the pre-boundary/dense classification. This is the observed Twisted-1 repair path.
6. If both endpoints lie within `Epsilon` of the boundary, do not insert a boundary ring; classify the whole tiny source span by which side occupies the larger normalized fraction, avoiding any new topology-invalid centre while minimizing semantic boundary displacement.
7. Count the explicit boundary ring in root-refinement telemetry only when that ring is actually inserted.
8. Because emitted production sample topology can change for affected inputs, advance Bark algorithm version `30 -> 31`; Bark settings and Generator versions remain unchanged.

### Acceptance criteria

- The exact Twisted-1 diagnostic fresh generation remains deterministic and unchanged.
- `POST_BUILD_RENDER_SAMPLES`, `POST_TRUNK_REFINEMENT`, later trunk stages, and final production Bark topology all report zero zero-length ring-centre pairs for the target case.
- The normal production Bark build for the target case passes without bypassing any validator or topology rule.
- No source Generator samples, topology threshold, axial-twist rule, root/body basis, root-foot equation, or non-trunk sampling logic changes.
- A final curated-gallery regeneration should return 20 / 20 Bark PASS before TREE-TRUNK.2 is closed.

### Non-goals

- No topology-threshold relaxation.
- No generic removal/merging of close rings after refinement.
- No retuning of dense-root sampling density or root-collapse intervals.
- No changes to Lean, Bend, Persistence, Axial Twist, Path Spiral, radial resolution, branch sampling, or recipes.
- No broad 40-control or multi-policy efficiency suite as part of this repair.

### Performance model

The repair adds two squared-distance comparisons only on the single source span that crosses the non-legacy dense-root boundary. It can remove one otherwise-invalid explicit boundary ring in endpoint-near cases and adds no per-frame work, allocations, textures, or gameplay runtime cost.

### Implementation sequence

1. Add the endpoint-spacing guard around the existing dense-root boundary insertion in trunk refinement, preserving the current split path byte-for-byte when both endpoint distances exceed `Epsilon`.
2. Advance Bark algorithm version to 31 because affected production sample topology changes.
3. Update the exact Twisted-1 diagnostic acceptance/report semantics from reproduction (`REPRODUCED`) to repair verification: normal Bark PASS, zero final zero-length segments, and zero captured post-refinement/final duplicates.
4. Audit the final diff against TREE-TRUNK.2G and verify all unrelated production equations, thresholds, and diagnostic capture remain unchanged.
5. Run available static checks and package only the approved files. Unity compile plus one exact Twisted-1 diagnostic run remain the live validation gate.

### Risks and mitigations

- **Boundary semantics drift:** classify the unsplit source span according to which endpoint is coincident with the boundary; displacement is bounded by the same <=1 mm interval that the topology contract already forbids as a separate ring.
- **Hidden broad sampling change:** alter only the non-legacy boundary-crossing branch; ordinary spans and topology-valid boundary splits remain unchanged.
- **Stale cached bark:** Bark algorithm version advances because a production render-ring can be removed for affected inputs.
- **False acceptance:** the dedicated diagnostic still captures the actual production pipeline and requires a normal Bark PASS with zero final zero-length centre segments.

### Validation plan

- Unity compile after installing the patch.
- Run the existing dedicated Twisted-1 ring-collapse diagnostic once; expected repair result is normal Bark PASS, zero post-refinement/final duplicate pairs, and `Status: PASS`.
- If that exact case passes, regenerate the curated twenty-tree gallery once; required closure result is 20 / 20 Bark PASS.

### TREE-TRUNK.2H implementation result and static audit

- Bark algorithm version advances `30 -> 31` because the repair can remove one production trunk render ring for source spans whose dense-root boundary interpolation would otherwise violate the existing centre-spacing topology invariant.
- `RefineTrunkRenderSamples` now measures the interpolated dense-root boundary against both source-span endpoint positions using the same squared-distance `Epsilon` contract used by final Bark ring-centre accounting. Topology-valid boundary splits retain the existing two-span path unchanged.
- Endpoint-near boundary crossings no longer emit the explicit boundary ring. A boundary near the upper endpoint keeps the original source span in dense-root sampling; a boundary near the lower endpoint keeps it post-boundary/non-dense; a boundary within threshold of both endpoints uses the larger normalized-domain side. Explicit boundary-ring telemetry increments only on actual insertion.
- The exact Twisted-1 diagnostic now verifies the repair rather than reproduction: success requires a normal production Bark PASS with zero final zero-length ring-centre segments. Existing stage capture remains active so the operator report can confirm zero duplicates immediately after trunk refinement and at final render samples.
- Preservation audit: Generator source, Bark settings, Bark topology audit, procedural-tree Inspector, focused Lean/Bend suite, and production-gallery closure audit are byte-identical to TREE-TRUNK.2G. Within Bark, `AppendRefinedTrunkSpan`, trunk/root surface equations, GroundAzimuth body basis, grounded root foot, tangent/root-frame envelopes, axial-twist measurement, `BuildRenderSamples`, and trunk-tip preparation are byte-identical. Only Bark versioning, the dense-root boundary-crossing guard, and diagnostic/report semantics change.
- Static lexical delimiter checks pass for both modified C# files. No C# compiler or Unity runtime is available in the extracted workspace, so Unity compilation and the exact one-case operator verification remain pending.

### TREE-TRUNK.2H status

1. Review — complete.
2. Canonical plan — complete before code modification.
3. Dense-root boundary spacing repair — complete in source.
4. Bark version invalidation — complete.
5. Exact Twisted-1 verification semantics update — complete.
6. Static scope/preservation/lexical audit — complete.
7. Unity compilation and exact Twisted-1 diagnostic — pending operator evidence.
8. Twenty-tree curated gallery regeneration — pending only after the exact case passes.

## TREE-TRUNK.2I — Acceptance Cleanup and Diagnostic Retirement

Status: implementation approved after the repaired Generator 8 / Bark 31 curated gallery completed 20 / 20 Bark PASS. This patch is cleanup-only: production tree geometry and accepted validation contracts are frozen.

### Objective

Close TREE-TRUNK.2 by removing temporary diagnostic/audit infrastructure created solely to discover, compare, or verify the now-accepted repair sequence. Perform a deep dependency cleanup rather than only removing Inspector controls: delete one-off runners, remove their Bark hooks and state, remove the superseded TREE-TRUNK.2B candidate-search engine and TREE-TRUNK.2D axial-twist bypass/differential instrumentation, then retain the useful general Lean/Bend contract suite and the accepted production implementations.

### Approved files

Modify:

- `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md`
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeTrunkResponseDiagnosticSuite.cs`
- `Assets/Game/Procedural/Trees/Editor/ProceduralTreeInstanceEditor.cs`

Delete:

- `Assets/Game/Procedural/Trees/Editor/TreeTrunkProductionClosureAudit.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeTrunkProductionClosureAudit.cs.meta`
- `Assets/Game/Procedural/Trees/Editor/TreeTrunkRingCollapseDiagnostic.cs`
- `Assets/Game/Procedural/Trees/Editor/TreeTrunkRingCollapseDiagnostic.cs.meta`

No serialized scene, prefab, material, recipe, layer, tag, component, dependency, or folder change is authorized.

### Reviewed evidence

- Complete repository instructions were reloaded before review.
- Complete current canonical handoff, Bark generator, Generator 8, focused trunk suite, procedural-tree Inspector editor, both one-off TREE-TRUNK.2F/2G-H runners, Bark settings, tree definition, procedural-tree instance, curated-gallery coordinator, gallery coordinator, bark asset builder, 40-control suite, geometry-efficiency audit, root-quality evaluation, and root-collapse tournament were reviewed.
- Whole-tree symbol search shows the TREE-TRUNK.2B candidate option/result types and candidate evaluator are consumed only by the focused trunk suite; no production or unrelated tool consumes them.
- The TREE-TRUNK.2D axial-twist absolute-gate bypass entry point is consumed only by the temporary semantics/bypass logic in the focused trunk suite.
- The TREE-TRUNK.2G render-ring centreline capture entry point/state is consumed only by the one-off Twisted-1 runner.
- The TREE-TRUNK.2F closure runner is consumed only by its temporary Inspector section and job-interlock checks.
- `RootTrunkBoundaryCandidateActivated`, the existing root-collapse tournament path, trunk-frame diagnostic telemetry, and root-ring correspondence failure diagnostics predate the temporary candidate search and remain live consumers; they are not cleanup targets.
- The retained production delta from the accepted pre-candidate state is intentionally limited to Generator 8 endpoint-preserving trunk behavior, Bark 31 ground-azimuth root-body basis, repaired recipe-only axial-twist measurement reference, and the dense-root boundary-spacing guard.
- Operator evidence: the exact Twisted-1 repair verification passed, followed by a full curated-gallery regeneration with 20 / 20 Bark PASS.

### Invariants / non-goals

1. Keep Generator version 8 and its source byte-identical.
2. Keep Bark algorithm version 31.
3. Keep emitted production Bark geometry byte-equivalent for identical input/settings to the accepted TREE-TRUNK.2H state.
4. Keep the ground-azimuth recipe-only root-body basis, persistence-independent tangent-safety release, grounded root-foot path, repaired recipe-only axial-twist reference, dense-root boundary-spacing guard, topology thresholds, and all authored control semantics unchanged.
5. Keep the general 35-case Lean/Bend contract suite, its incremental/cancellable Editor behavior, endpoint/height/tangent/topology contracts, and rich root-frame failure evidence.
6. Remove diagnostic candidate-policy enumeration/evaluation code rather than leaving unreachable helpers/types compiled.
7. Remove axial-twist bypass/differential-audit code rather than leaving a hidden validation escape hatch compiled.
8. Remove render-ring centreline capture state/callbacks/wrapper rather than leaving dormant per-stage hooks in the Bark production pipeline.
9. Remove closure/ring one-off Inspector controls and their cross-job interlock references completely.
10. Do not run or modify the generic 40-control suite or multi-policy geometry audit merely for this cleanup.

### File-by-file implementation sequence

1. Canonical handoff: record this approved plan first, then later record exact cleanup results, preservation evidence, and final accepted TREE-TRUNK.2 state.
2. Bark generator: start from the accepted pre-candidate diagnostic surface and reapply only the accepted production changes; this removes the candidate-search engine, axial-twist bypass, ring-centre capture machinery, and every private helper used only by those temporary paths while preserving existing permanent diagnostics.
3. Focused trunk suite: return to the retained TREE-TRUNK.2A-era 35-case runner/evidence surface, remove candidate CSV/state/search code and TREE-TRUNK.2D bypass/differential work, then update report identity/contract text to the accepted Generator 8 / Bark 31 closure state without changing cases or thresholds.
4. Procedural-tree Inspector editor: remove both one-off audit/diagnostic sections and all job-interlock references introduced solely for them; preserve the pre-existing diagnostic/audit controls byte-for-byte where practical.
5. Delete the two one-off runner source files and their metadata.
6. Run whole-tree reference searches for every removed type/API/identifier family; any remaining reference is a blocker unless proven historical documentation only.
7. Compare retained production methods and Generator source against TREE-TRUNK.2H; run static delimiter/symbol checks and package only approved changes/deletions.

### Acceptance criteria

- No C# reference remains to the deleted one-off runner classes.
- No C# reference remains to candidate-search policy/result types, the candidate evaluator, candidate CSV/state helpers, axial-twist validation bypass state/API, twist-semantics audit types/runner, or render-ring centreline capture state/API.
- No private helper remains whose only caller belonged to one of those removed temporary systems.
- Generator 8 is byte-identical to TREE-TRUNK.2H.
- Bark 31 production method bodies/equations for geometry, frame resolution, axial-twist validation, dense-root boundary repair, topology audit, and root-collapse tournament behavior are preserved; only diagnostic-only branches/hooks and unreachable helpers are removed.
- The retained focused 35-case suite still calls the normal production Bark build and contains the same case definitions and invariant thresholds as the accepted pre-candidate focused suite.
- The procedural-tree Inspector contains no TREE-TRUNK.2F closure or Twisted-1 one-off UI/job state.
- Static C# lexical/delimiter checks pass.
- Unity compilation after installing the cleanup patch is the only required operator validation. No additional suite rerun is required unless compilation or static preservation evidence exposes a behavioral change.

### Risks and mitigations

- **Deleting a diagnostic hook still used elsewhere:** whole-tree reference search before and after each removal; preserve any symbol with a non-temporary consumer.
- **Accidentally reverting accepted production behavior while stripping Bark diagnostics:** reconstruct Bark from the accepted pre-candidate diagnostic state and reapply only the three later accepted Bark production deltas, then compare targeted production method bodies against TREE-TRUNK.2H.
- **Changing focused-suite semantics while removing temporary work:** base the retained suite on the accepted pre-candidate 35-case implementation, preserving case construction, measurements, tolerances, topology gate, and Editor cadence.
- **Leaving dead helper code under removed public surfaces:** run a post-removal method/type reference audit and delete helpers that have no remaining caller and were introduced by the retired systems.

### Status

1. Review — complete.
2. Canonical cleanup plan — complete before implementation edits.
3. Deep temporary diagnostic removal — complete.
4. Production-preservation reconstruction — complete.
5. Dead-helper/reference audit — complete.
6. Final canonical acceptance update — complete.
7. Static/scope/compliance audit — complete.
8. Unity compilation — pending operator validation.

### TREE-TRUNK.2I implementation result and acceptance state

- The temporary production-gallery closure runner and Twisted-1 ring-collapse runner are deleted together with their metadata and all Inspector/job-interlock references.
- Bark no longer compiles the TREE-TRUNK.2B candidate-policy enums/options/results, the candidate topology evaluator, the TREE-TRUNK.2D axial-twist validation bypass, or the TREE-TRUNK.2G render-ring centreline capture state/callback/wrapper. The associated private helper families were removed with their only callers.
- The focused trunk suite no longer contains candidate-search state/CSV writers/stage selection, candidate geometry controls, legacy-gate bypass handling, or the 18-build differential twist-semantics audit. It is restored to the accepted 35-case Lean/Bend contract runner and retains the richer permanent root-frame failure evidence.
- The procedural-tree Inspector editor is byte-identical to its pre-TREE-TRUNK.2F/G state, so the one-off closure/ring sections and their cross-job disable checks are fully gone while the pre-existing permanent diagnostics remain unchanged.
- Generator 8 is byte-identical to the accepted TREE-TRUNK.2H state.
- Bark remains algorithm 31. Exact method-body comparison against TREE-TRUNK.2H confirms the accepted dense-root boundary-spacing guard, recipe-only ground-azimuth root-body basis, grounded root-foot radial path, tangent-safety envelope, root-frame envelope, root contribution equations, recipe-only axial-twist measurement reference, legacy twist measurement, normal render-sample construction, and topology-safe trunk-tip preparation are byte-identical.
- The Bark cleanup diff against TREE-TRUNK.2H consists only of temporary candidate/capture/bypass type and helper removal, removal of dormant capture callbacks, removal of duplicate diagnostic-only pre-failure twist telemetry assignment, and restoration of the normal unconditional 2-degree axial-twist production gate. With the old bypass flag false during every normal production build, accepted production behavior is unchanged.
- Whole-tree C# reference search returns zero references to the deleted runner classes, candidate-search policy/result types, candidate evaluator, axial-twist bypass API/state, twist-semantics audit types, or ring-centre capture API/state.
- Deep symbol audit removed 10 Bark temporary types, 28 Bark temporary methods, 18 Bark temporary fields, 7 focused-suite temporary types, and 47 focused-suite temporary methods. Pre-existing root-collapse tournament telemetry and permanent trunk-frame/root-ring diagnostics remain because they have live non-temporary consumers or predate this repair sequence.
- Final source-scope comparison against TREE-TRUNK.2H reports four modified project files, zero additions, and four deletions, exactly matching the approved cleanup scope.
- Static C# lexical scanning reports balanced parentheses, brackets, and braces with no unterminated string/comment state in all modified C# files.
- Unity compilation is unavailable in the extracted workspace and remains the only operator validation required for this cleanup. No TREE-TRUNK.2 suite rerun is required unless compilation exposes a cleanup error.

### TREE-TRUNK.2 accepted baseline

TREE-TRUNK.2 is accepted at Generator 8 / Bark 31. The accepted behavior consists of endpoint/height-preserving recipe-only trunk Bend/Lean generation, persistence-independent tangent safety, ground-azimuth recipe-only root-body geometry with grounded root feet kept separate, axial-twist validation measured in the same ground-azimuth convention, and dense-root boundary insertion that respects Bark's existing minimum ring-centre spacing invariant. The focused 35-case Lean/Bend contract suite remains the permanent targeted regression tool.

TREE-TRUNK.2B candidate search, TREE-TRUNK.2D validator-semantics bypass/differential audit, TREE-TRUNK.2F production-gallery closure audit, and TREE-TRUNK.2G/H Twisted-1 ring-collapse capture are historical evidence only and are superseded as active tooling by this acceptance cleanup.
