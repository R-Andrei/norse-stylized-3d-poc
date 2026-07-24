# Stylized Nature Tree Integration Handoff

## Status

**Asset isolation, the simultaneous twenty-tree reference gallery, deterministic structural generation, and the corrected four-family bark vertical slice are implemented and live-validated. `TREE-GEN.2B` passed all twenty structures, all four bark meshes, deterministic repeatability, classified topology audits, family dependency suites, neutral bark colour, and exterior rendering. `TREE-GEN.2C` is now source-implemented as the approved compact trunk-grammar patch: the existing `Trunk Twist Degrees` control becomes visibly geometric through a non-circular trunk profile, two ridge controls define that profile, and three root-buttress controls create a lobed base that fades into the trunk. Structural generator/profile seed versions and existing twist ranges remain unchanged. Source consistency/compliance checks pass; Unity compilation, managed migration, topology, visual root/twist comparison, and Play Mode validation remain pending.**

This document is the canonical plan, architecture ledger, implementation record, and continuation handoff for the Stylized Nature tree assets, the imported comparison gallery, and the generated tree library. Each later implementation patch still requires explicit approval and must follow the ordered gates recorded below.

## Objective

Preserve the implementation-relevant Stylized Nature tree assets, establish a complete imported reference gallery as the first implementation baseline, and define a production-suitable procedural tree library that can both reproduce the Common, Pine, Twisted, and Dead source families or individual reference silhouettes and deliberately exceed them through controlled crown volume, foliage density, branch count, trunk/branch curvature, colour palettes, damage, and seeded structural variation while consuming the existing Weather wind and cloud-shadow contracts.

## Acceptance criteria

- `Assets/References/Trees/` contains the twenty Unity-targeted tree FBX files.
- The dedicated folder contains the twelve required bark and foliage texture variants.
- Every moved FBX and texture retains its existing `.meta` file and GUID.
- The dedicated folder contains a separate copy of the pack's CC0 license.
- The original pack contains a Markdown summary covering trees, grass, plants, mushrooms, rocks, shader freedom, wind/weather compatibility, and procedural-generation limits.
- This handoff records the final dedicated-tree paths, the current vegetation and Weather contracts, integration risks, non-goals, and next actions.
- Generic FBX, OBJ, glTF, preview, non-tree, and unrelated texture files remain unchanged inside the original pack.
- The completed relocation phase changed no runtime source, shader, compute shader, scene, prefab, material, profile, layer, tag, package, or ProjectSettings file.
- Final validation proves the exact move set, source removal, retained metadata GUIDs, content hash preservation, absence of duplicate GUIDs, documentation paths, and final repository scope.

### Implementation-roadmap acceptance criteria

- The first implementation patches audit and render one instance of all twenty imported trees in a deterministic four-family comparison gallery.
- Every imported specimen has a reserved side-by-side procedural slot, measured bounds, geometry/material audit data, and a stable Ground-aligned comparison position.
- Shared tree bark and foliage shaders consume the existing Weather wind field and the authoritative URP main-light-cookie cloud path without modifying either producer.
- Existing grass rendering, placement, coverage, interaction, and trample implementations remain unchanged.
- The generated-tree system uses profile-driven curve hierarchies and generates its own topology, normals, tangents, UVs, deformation metadata, foliage pivots, LODs, and proxies.
- Ordinary production trees are editor-generated and baked into a deterministic accepted variant library; runtime rendering does not regenerate their meshes.
- The initial generated library contains at least eight accepted variants for each of Common, Pine, Twisted, and Dead.
- Production placement uses authored coverage, Ground suitability, deterministic minimum-distance acceptance, stable IDs, and chunk-aware rendering.
- Future snow, wetness, seasons, damage, chopping, roots, and navigation systems can consume reserved contracts without blocking the gallery or generated-library implementation.
- Family profiles, reference-calibration presets, variant recipes, instance overrides, and deterministic seed variation remain distinct authoring layers.
- Foliage spatial volume and foliage geometry density are independently controllable; richer crowns must not require a proportional card-count increase.
- Bark and foliage colours are palette/profile data and per-instance variation inputs, not unique material assets per tree.
- Primary/secondary/tertiary branch counts, spawn distributions, trunk curvature, primary-branch curvature, droop, lean, torsion, and crown envelope are first-class authored controls.
- Changing foliage-only controls does not regenerate trunk or branch structure; independent deterministic streams and dependency fingerprints enforce selective regeneration.
- The twenty imported references may each have a calibration preset that targets their family grammar, dimensions, silhouette, branching rhythm, and crown shape without requiring topology-identical reconstruction.

## Approved scope

Create:

```text
Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md
Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md.meta
Assets/References/Stylized Nature/Stylized_Nature_Integration_Review.md
Assets/References/Stylized Nature/Stylized_Nature_Integration_Review.md.meta
Assets/References/Trees/
Assets/References/Trees.meta
Assets/References/Trees/License_Standard.txt
Assets/References/Trees/License_Standard.txt.meta
```

Move, with each existing `.meta` file:

```text
Assets/References/Stylized Nature/FBX (Unity)/CommonTree_1.fbx
Assets/References/Stylized Nature/FBX (Unity)/CommonTree_2.fbx
Assets/References/Stylized Nature/FBX (Unity)/CommonTree_3.fbx
Assets/References/Stylized Nature/FBX (Unity)/CommonTree_4.fbx
Assets/References/Stylized Nature/FBX (Unity)/CommonTree_5.fbx
Assets/References/Stylized Nature/FBX (Unity)/DeadTree_1.fbx
Assets/References/Stylized Nature/FBX (Unity)/DeadTree_2.fbx
Assets/References/Stylized Nature/FBX (Unity)/DeadTree_3.fbx
Assets/References/Stylized Nature/FBX (Unity)/DeadTree_4.fbx
Assets/References/Stylized Nature/FBX (Unity)/DeadTree_5.fbx
Assets/References/Stylized Nature/FBX (Unity)/Pine_1.fbx
Assets/References/Stylized Nature/FBX (Unity)/Pine_2.fbx
Assets/References/Stylized Nature/FBX (Unity)/Pine_3.fbx
Assets/References/Stylized Nature/FBX (Unity)/Pine_4.fbx
Assets/References/Stylized Nature/FBX (Unity)/Pine_5.fbx
Assets/References/Stylized Nature/FBX (Unity)/TwistedTree_1.fbx
Assets/References/Stylized Nature/FBX (Unity)/TwistedTree_2.fbx
Assets/References/Stylized Nature/FBX (Unity)/TwistedTree_3.fbx
Assets/References/Stylized Nature/FBX (Unity)/TwistedTree_4.fbx
Assets/References/Stylized Nature/FBX (Unity)/TwistedTree_5.fbx
Assets/References/Stylized Nature/Textures/Bark_DeadTree.png
Assets/References/Stylized Nature/Textures/Bark_DeadTree_Normal.png
Assets/References/Stylized Nature/Textures/Bark_NormalTree.png
Assets/References/Stylized Nature/Textures/Bark_NormalTree_Normal.png
Assets/References/Stylized Nature/Textures/Bark_TwistedTree.png
Assets/References/Stylized Nature/Textures/Bark_TwistedTree_Normal.png
Assets/References/Stylized Nature/Textures/Leaf_Pine.png
Assets/References/Stylized Nature/Textures/Leaf_Pine_C.png
Assets/References/Stylized Nature/Textures/Leaves_NormalTree.png
Assets/References/Stylized Nature/Textures/Leaves_NormalTree_C.png
Assets/References/Stylized Nature/Textures/Leaves_TwistedTree.png
Assets/References/Stylized Nature/Textures/Leaves_TwistedTree_C.png
```

No other file is approved for modification or relocation.

## Reviewed evidence

### Repository state and ownership

- `git status --short --branch` reported `fufu...origin/fufu [ahead 27]` and pre-existing user changes across documentation, Generated Mass, river, scene, profile, PixelSurface, and Weather files. Those changes are unrelated and must remain untouched.
- `Assets/References/` and `Assets/References.meta` are ignored by `.gitignore`. The dedicated tree assets and the pack review are therefore local project files that will not be carried by an ordinary Git commit. The tracked handoff must identify every local path, and the user must include `Assets/References/Trees/` when transferring the latest project files.
- `ProjectSettings/VersionControlSettings.asset` uses `Visible Meta Files`, and `ProjectSettings/EditorSettings.asset` uses Force Text serialization. Existing asset metadata must move with the selected files.
- `Assets/References/Trees/` did not exist before this plan.
- No pre-existing tree- or nature-specific document was found under `Assets/Docs/`.

### Pack and move-set evidence

- The local free pack contains 68 of 116 models and declares CC0 1.0 in `Assets/References/Stylized Nature/License_Standard.txt`.
- The pack contains generic FBX, Unity-labelled FBX, OBJ, glTF, textures, previews, and importer metadata.
- The twenty Unity-labelled tree FBX files total 6,738,384 bytes before metadata.
- The selected move set contains twenty FBX assets, twelve texture assets, and their thirty-two existing `.meta` files: sixty-four moved files totalling 36,024,759 bytes.
- Tree glTF material inspection identifies exactly six tree materials: `Bark_DeadTree`, `Bark_NormalTree`, `Bark_TwistedTree`, `Leaves_NormalTree`, `Leaves_Pine`, and `Leaves_TwistedTree`.
- Those materials reference nine coloured runtime textures. Three additional white foliage variants are retained because they support custom tint and seasonal workflows: `Leaf_Pine.png`, `Leaves_NormalTree.png`, and `Leaves_TwistedTree.png`.
- Generic FBX, OBJ, and glTF files are redundant source/export representations for the approved Unity import path. No installed glTF importer exists in `Packages/manifest.json`. They are not moved.
- No current project asset outside `Assets/References/Stylized Nature/` references any selected FBX or texture GUID.
- Representative FBX importer metadata uses scale `1`, file units enabled, axis conversion disabled, imported normals and vertex colours, calculated tangents, no generated LODs, and `isReadable: 0`.
- All three bark normal-map texture metadata files currently use ordinary texture type `0` with sRGB enabled. Correct normal-map classification is future integration work and is not changed during relocation.

### Current vegetation and Weather contracts

- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md` is the canonical vegetation implementation ledger. Its accepted production system uses one fixed 18-vertex, 12-triangle CrossedCards mesh per `VegetationLayer`, one 48-byte instance record, and one indirect draw per enabled layer.
- `Assets/Game/Procedural/Vegetation/VegetationRendererBase.cs` hard-codes `VegetationClusterMeshBuilder.Build`, the current vegetation shader, one submesh command, and its current instance generator. It is not a source-mesh tree renderer.
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader` expects the generated grass channel contract, including `COLOR.r` root-to-tip weighting and generated UV centreline data. It does not implement the tree texture/material contract.
- `Assets/Docs/Weather_Wind_Architecture.md` is the canonical wind document. Weather owns the shared XZ wind field, CPU samples, active response cache, and shader globals; each consumer owns its response.
- `Assets/Game/Rendering/Weather/Includes/WeatherWindField.hlsl` exposes `SampleWeatherWindResponse(float3 worldPosition)`. A future tree shader can consume this shared field without adding a tree-owned wind producer.
- The accepted default Weather field is 128 by 128 cells at 0.5 metres per cell, covering 64 by 64 metres. A larger forest needs an explicit far-field or expanded-domain decision.
- `Assets/Docs/Weather_System_Architecture_Provisional.md` identifies precipitation, temperature, seasons, fog, and lightning as undefined or unimplemented. Tree wetness, snow, frost, and seasonal tint are receiver-compatible design targets, not existing functionality.

### Planning review evidence for the imported gallery and generated library

- `Assets/Game/Procedural/Vegetation/VegetationInstanceData.cs:6-15` defines the accepted grass renderer's fixed 48-byte, three-`Vector4` instance contract. The tree renderer therefore requires a separate instance type rather than silently extending the grass stride.
- `Assets/Game/Procedural/Vegetation/VegetationRendererBase.cs:612-617` submits one current grass mesh through `Graphics.RenderMeshIndirect`, and `VegetationRendererBase.cs:1039-1043` hard-codes `VegetationClusterMeshBuilder.Build`. This is concrete evidence that the current implementation is not an arbitrary source-mesh family renderer.
- `Assets/Game/Rendering/Vegetation/Shaders/SH_StylizedVegetationBenchmark.shader:66-84` compiles `_LIGHT_COOKIES` and includes the current Weather wind response and grass-specific interaction/trample paths. The tree shaders must reuse the Weather/cloud contracts while omitting grass interaction/trample includes.
- `Assets/Game/Rendering/Weather/Includes/WeatherWindField.hlsl:42-77` exposes the active bend/velocity response through `SampleWeatherWindResponse`. This is the required tree shader input.
- `Assets/Game/Rendering/Vegetation/Includes/VegetationWindResponse.hlsl:19-63` demonstrates the accepted consumer pattern: sample Weather in the shader and apply consumer-owned stiffness/phase response without CPU deformation. The tree response must follow the same ownership model with tree-specific hierarchy metadata.
- `Assets/Game/Rendering/Vegetation/Includes/VegetationLighting.hlsl:285-306` obtains the main light at fragment world position through URP. `Assets/Docs/Weather_Cloud_Shadow_Handoff.md` locks universal gameplay-world reception to the main directional-light cookie and prohibits double application.
- `Assets/Game/Procedural/Ground/GeneratedGround.cs:2986-3054` exposes both the simple height/normal sample and the full `GroundSurfaceSample`, including exposure, damp deposit, vegetation suitability, compaction, shore influence, rocky/dry influence, and material classification. Gallery grounding uses the simple contract; production tree placement may use the full contract.
- `Assets/Game/Procedural/Vegetation/VegetationCoverageField.cs:154-176` demonstrates the accepted Ground-relative byte-mask sampling pattern. Tree placement may reuse that data pattern without altering the grass layer's ownership.
- `Assets/Docs/Proof of Concept/02_Procedural_Geometry_and_Asset_Grammars.md:203-265` defines trees as hierarchies of tapered curves and explicitly prioritizes recognizable stylized families over botanical simulation. The procedural architecture below implements that existing project direction.

### Tree asset findings

- The pack contains four tree families with five meshes each: Common, Dead, Pine, and Twisted.
- The twenty models total 170,293 imported/accessor vertices and 118,610 triangles.
- Common, Pine, and Twisted trees use separate bark and foliage materials. Dead trees use bark only.
- The models contain positions, normals, UV0, and vertex colours. They do not contain a skeleton, animations, morph targets, or authored LODs.
- Bark vertex colours form grayscale gradients correlated with height. Foliage vertex colours are effectively white.
- **Inference — Medium confidence:** bark colour is usable as a root-to-crown bend mask. Evidence is the measured grayscale/height correlation across all twenty glTF exports. A Unity vertex-colour debug material would verify or falsify the interpretation.
- Foliage is composed of many disconnected cards but has no branch attachment hierarchy. Import-time component analysis can derive card pivots and phases; it cannot recover a reliable procedural branch grammar.

### History comparison

- Recent vegetation and Weather history reviewed: `9d69a18`, `68225e2`, `08bc644`, `0122633`, `ab7b042`, `40d79bb`, and `ad214c9`.
- Current vegetation runtime/rendering source is not modified in the working tree. The canonical vegetation and Weather documents contain pre-existing user changes and remain outside this patch.
- This relocation introduces no behavior change relative to `HEAD` or the accepted interaction/wind implementations because no runtime source, shader, scene, prefab, material, or package is modified.

## Invariants

- Weather remains the sole authoritative wind producer.
- The Weather cloud controller and URP main directional-light cookie remain the sole authoritative cloud-shadow producer/path.
- Tree-family shaders own tree-specific trunk, branch, foliage, flutter, stiffness, snow, wetness, and seasonal response.
- Every tree shader receives the authoritative cloud cookie exactly once; custom duplicate cloud attenuation is prohibited.
- Existing grass rendering, placement, coverage, interaction, trample, and benchmark contracts remain unchanged.
- Imported tree assets do not become `VegetationLayer` CrossedCards.
- Imported FBXs remain visual references and optional fallbacks; procedural topology is generated from tree definitions and family profiles.
- Ordinary generated tree meshes are produced and baked in the Editor or approved dirty-time workflows, not regenerated every gameplay frame.
- No per-tree or per-vertex CPU wind simulation is introduced.
- No per-frame full-field rebuild is introduced.
- No GameObject-per-tree production rendering architecture is inferred from the twenty-object reference gallery.
- No new layer or tag is required by the planned V1 architecture.
- Existing source FBX and texture GUIDs remain unchanged.
- Generic and non-tree pack content remains intact until the user removes it.

## Non-goals

The completed relocation patch did not include the following. They are planned below as separate approved implementation patches rather than silently folded into relocation:

- Unity import-setting corrections;
- tree materials or shaders;
- the imported reference gallery;
- procedural tree generation;
- LODs, shadow proxies, colliders, runtime rendering, placement, or coverage;
- tree interaction or trample behaviour;
- Weather producer changes;
- cloud producer changes;
- Generated Mass or rock-system work;
- moving generic FBX, OBJ, glTF, previews, grass, plants, mushrooms, rocks, or unrelated textures;
- modifying `.gitignore` to track `Assets/References/Trees/`.

The current documentation-only update does not implement any of the planned runtime/editor files, shaders, profiles, scene objects, materials, generated assets, or importer changes.

## Implementation sequence

| ID | Work | Status |
| --- | --- | --- |
| TREE-HANDOFF.0 | Complete read-only pack, repository, consumer, producer, contract, status, diff, and history review; record this plan as the first write. | Complete |
| TREE-HANDOFF.1 | Create `Assets/References/Trees/` and its folder metadata. | Complete |
| TREE-HANDOFF.2 | Move the twenty Unity FBX assets and existing metadata into the dedicated folder. | Complete |
| TREE-HANDOFF.3 | Move the twelve required/tintable textures and existing metadata into the dedicated folder. | Complete |
| TREE-HANDOFF.4 | Copy the CC0 license with new metadata into the dedicated folder. | Complete |
| TREE-HANDOFF.5 | Write the complete pack summary review inside the original pack. | Complete |
| TREE-HANDOFF.6 | Update this handoff with final paths, asset inventory, integration direction, and continuation work. | Complete |
| TREE-HANDOFF.7 | Run the post-change consistency, hash, metadata, source-removal, duplicate-GUID, documentation, repository-scope, and compliance audit. | Complete |
| TREE-PLAN.1 | Formalize the imported-gallery-first implementation architecture, procedural generated-tree library, runtime rendering, placement direction, patch order, and validation gates in this canonical document. | Complete in the updated documentation artifact; implementation remains pending. |
| TREE-PLAN.2 | Freeze the live-validated complete reference gallery and lock the generated-tree authoring/variation contract: reference presets, layered ownership, crown volume versus density, branch counts, separate curvature controls, palettes, seed isolation, and selective regeneration. | Complete in this documentation patch. |

## Risks and controls

| Risk | Control |
| --- | --- |
| Files under `Assets/References/` are omitted from Git transfer. | Record the ignore rule and exact local path here; require explicit transfer of the `Trees` folder. |
| Asset GUIDs change during relocation. | Move every existing FBX/texture `.meta` beside its asset and verify the GUID and content hash after the move. |
| Duplicate license metadata creates duplicate GUIDs. | Copy the license text only and create new metadata with a new GUID. |
| Normal maps render incorrectly. | Record current ordinary-sRGB import state; correct it only in the future approved integration patch. |
| Tree scale or axis differs from project expectations. | Preserve importer settings; validate representative families in Unity before choosing family defaults. |
| The current grass renderer is generalized unsafely. | Treat trees as a separate approved source-mesh family path unless a later architecture plan proves a shared renderer. |
| High tree triangle counts make unbounded instancing expensive. | Require LOD/impostor, culling, draw-group, shadow, and target-hardware budgets before production placement. |
| Foliage wind deforms without local pivots. | Validate vertex colours and design import-time leaf-card pivot/phase preprocessing before production wind. |

## Validation plan

1. Compare the exact destination inventory with the thirty-two approved assets and require matching `.meta` files.
2. Verify every moved asset hash and metadata GUID against its captured pre-move value, and verify the corresponding source paths no longer exist.
3. Scan all project metadata for duplicate GUIDs and confirm the new folder, license, review, and handoff metadata are unique.
4. Verify generic/non-tree reference-pack inventory remains present and no unapproved project path changed.
5. Reread both Markdown documents, validate headings/fences/paths, and confirm the handoff uses the final `Assets/References/Trees/` paths.
6. Compare the final Git status and diff with the approved scope; record Unity import/runtime validation as pending because no tree runtime implementation is included.

## Future tree-integration direction

### Final dedicated tree paths

Root:

```text
Assets/References/Trees/
```

Models:

```text
Assets/References/Trees/CommonTree_1.fbx
Assets/References/Trees/CommonTree_2.fbx
Assets/References/Trees/CommonTree_3.fbx
Assets/References/Trees/CommonTree_4.fbx
Assets/References/Trees/CommonTree_5.fbx
Assets/References/Trees/DeadTree_1.fbx
Assets/References/Trees/DeadTree_2.fbx
Assets/References/Trees/DeadTree_3.fbx
Assets/References/Trees/DeadTree_4.fbx
Assets/References/Trees/DeadTree_5.fbx
Assets/References/Trees/Pine_1.fbx
Assets/References/Trees/Pine_2.fbx
Assets/References/Trees/Pine_3.fbx
Assets/References/Trees/Pine_4.fbx
Assets/References/Trees/Pine_5.fbx
Assets/References/Trees/TwistedTree_1.fbx
Assets/References/Trees/TwistedTree_2.fbx
Assets/References/Trees/TwistedTree_3.fbx
Assets/References/Trees/TwistedTree_4.fbx
Assets/References/Trees/TwistedTree_5.fbx
```

Textures and license:

```text
Assets/References/Trees/Bark_DeadTree.png
Assets/References/Trees/Bark_DeadTree_Normal.png
Assets/References/Trees/Bark_NormalTree.png
Assets/References/Trees/Bark_NormalTree_Normal.png
Assets/References/Trees/Bark_TwistedTree.png
Assets/References/Trees/Bark_TwistedTree_Normal.png
Assets/References/Trees/Leaf_Pine.png
Assets/References/Trees/Leaf_Pine_C.png
Assets/References/Trees/Leaves_NormalTree.png
Assets/References/Trees/Leaves_NormalTree_C.png
Assets/References/Trees/Leaves_TwistedTree.png
Assets/References/Trees/Leaves_TwistedTree_C.png
Assets/References/Trees/License_Standard.txt
```

Every FBX and texture path has a same-named `.meta` file containing its original GUID. The license has new metadata. The folder metadata is `Assets/References/Trees.meta`.

The pack-level review is:

```text
Assets/References/Stylized Nature/Stylized_Nature_Integration_Review.md
```

The pack-level review will be removed when the user deletes the remaining Stylized Nature pack. This handoff is the persistent tree-thread record.

The future thread should begin with a four-family vertical slice rather than all twenty production trees:

- `CommonTree_1.fbx`
- `Pine_5.fbx`
- `TwistedTree_1.fbx`
- `DeadTree_1.fbx`

The likely architecture is a separate source-mesh tree family renderer or a deliberately approved shared source-mesh vegetation layer. It should:

- use separate bark and foliage material profiles;
- consume `SampleWeatherWindResponse` while retaining Weather ownership;
- validate and use the bark vertex-colour gradient;
- bake foliage-card pivot, phase, and stiffness data when local motion is required;
- support alpha clipping, two-sided foliage, calculated tangents, and correct normal-map import;
- define conservative wind bounds;
- define multi-submesh draw ownership;
- define LOD/impostor generation, chunk culling, shadow policy, and target-hardware budgets;
- expose seeded mesh selection, transform, tint, stiffness, phase, seasonal state, and optional deterministic mesh warps;
- treat new branch topology as a separate procedural-tree-generator problem.

## Canonical tree implementation architecture and patch roadmap

### Decision summary

The accepted implementation direction is a two-stage tree programme:

1. establish an imported-tree reference gallery containing one instance of every retained source tree, with reserved side-by-side positions for procedural comparisons;
2. build an editor-generated, profile-driven procedural tree library that reuses the retained bark textures, bark normal maps, and foliage textures while generating its own branch structure, mesh topology, vertex normals, tangents, UVs, wind metadata, LODs, shadow proxies, and runtime variant library.

The imported gallery now satisfies the first-stage gate and is frozen as the visual calibration baseline. The generated system is explicitly required to support both **reference replication** and **authored expansion**. Reference calibration targets family identity, dimensions, silhouette, branching rhythm, and crown distribution; it does not require topology-identical reconstruction. Authored expansion includes richer crowns, larger foliage regions without mandatory density growth, independent foliage density, branch-count controls, separate trunk and branch curvature, colour palettes, damage, age, and controlled seed variation.

The imported meshes are reference specimens and optional fallback assets. They are not the topology source for the procedural generator. The procedural generator must reproduce the recognizable visual grammar of Common, Pine, Twisted, and Dead families without attempting to recover an unavailable authored branch hierarchy from the finished FBXs.

### Current implementation authority

This document is the canonical tree integration and generation plan.

The following existing documents remain authoritative for their owned contracts:

- `Assets/Docs/Weather_Wind_Architecture.md` owns Weather wind production and the shared wind-field contract;
- `Assets/Docs/Weather_Cloud_Shadow_Handoff.md` owns cloud-field production, universal receiver coverage, and the selected URP main-directional-light-cookie path;
- `Assets/Docs/Vegetation_Rendering_and_Interaction_Architecture.md` owns the existing grass layer, coverage, rendering, interaction, and trample implementation;
- `Assets/Docs/Proof of Concept/02_Procedural_Geometry_and_Asset_Grammars.md` owns the high-level project direction that trees are hierarchies of curves and that stylized family identity takes priority over botanical simulation.

The tree implementation must consume those contracts without changing their ownership.

### Locked system boundaries

#### Imported reference gallery

The gallery is an Editor-authored diagnostic system. It may instantiate ordinary source FBX GameObjects and use `MeshFilter` and `MeshRenderer` components because it contains only twenty controlled specimens. It is not a production forest renderer and does not establish a GameObject-per-tree runtime architecture.

#### Procedural tree generator

The generator owns structural tree descriptions, family grammars, mesh generation, foliage placement, vertex metadata, LOD construction, shadow/collision proxy construction, generation diagnostics, and editor-time baking.

#### Generated tree library

The generated library owns accepted baked variants and the metadata required to render, place, validate, and reproduce them. Runtime systems consume the library. Runtime systems do not regenerate the structural meshes during ordinary gameplay.

#### Runtime tree renderer

The runtime renderer owns source-mesh/LOD draw batching, instance buffers, conservative bounds, chunk activation, distance-band assignment, and rendering resource lifetime. It does not own world ecology, Weather production, cloud production, or procedural mesh generation.

#### Tree placement

Tree placement owns authored coverage, ecological suitability, family composition, minimum spacing, stable IDs, Ground projection, and exclusion rules. It does not own mesh construction or material response.

### Weather wind integration contract

Weather remains the sole authoritative wind producer.

Tree shaders must consume:

```hlsl
WeatherWindResponseSample SampleWeatherWindResponse(float3 worldPosition)
```

from:

```text
Assets/Game/Rendering/Weather/Includes/WeatherWindField.hlsl
```

The tree system must not introduce:

- a tree-owned wind direction;
- a tree-owned world wind texture;
- per-tree CPU wind simulation;
- per-vertex CPU deformation;
- a duplicate Weather simulation cadence;
- a full-field rebuild triggered by tree state.

The tree consumer response is split into four layers:

1. **whole-tree macro bend** — coherent displacement driven by Weather bend and limited by family/trunk stiffness;
2. **branch response** — lower-amplitude branch-order motion driven by branch stiffness, phase, and distance from the root;
3. **foliage attachment response** — foliage pivots follow their parent branch deformation;
4. **foliage flutter** — local high-frequency card motion driven by Weather energy, deterministic phase, and family-specific flutter parameters.

The imported gallery and generated trees must use the same tree wind include and the same family-level response parameters wherever their data permits. The imported trees may initially use a reduced height-weighted response because their branch hierarchy is unavailable. Generated trees must use exact generated attachment and stiffness metadata.

The current Weather field covers 64 by 64 metres at the accepted default 128 by 128 cells and 0.5 metres per cell. Gallery specimens must be placed inside the active Weather field. Production distant-tree behaviour outside the detailed field is deferred to the runtime-renderer performance phase, where the approved options are a cheaper prevailing-wind fallback or an expanded field justified by profiling. An abrupt detailed-wind-to-static boundary is not an acceptable final production result.

### Cloud-shadow integration contract

Trees are mandatory cloud-shadow receivers.

The selected cloud architecture is the URP main directional-light cookie. Tree shaders must:

- compile `_LIGHT_COOKIES`;
- evaluate the main light at the actual fragment world position through the compatible URP `GetMainLight` path;
- allow the authoritative directional cookie to attenuate direct sun exactly once;
- preserve ambient, local-light, emission, and material identity unless the tree shader explicitly owns a separate response;
- never sample a second custom cloud field;
- never apply both the cookie and a custom cloud attenuation factor.

The bark and foliage shaders must be added to the cloud receiver audit. A tree patch cannot be accepted while either shader is reported as an unsupported sun-responsive receiver.

The gallery must be validated under the existing cloud diagnostic overlay so the same cloud boundary can be compared across Ground, imported bark, imported foliage, generated bark, and generated foliage at the same world positions.

### Existing grass-system relationship

The existing `VegetationRendererBase`, `VegetationClusterMeshBuilder`, `VegetationInstanceData`, grass shader, immediate interaction field, and trample field remain unchanged.

Trees must not become `VegetationLayer` CrossedCards. The current grass renderer assumes one fixed generated cluster mesh, one grass-specific 48-byte instance record, one submesh command, grass height/diameter bounds, grass blade UVs, and grass interaction deformation. Generalizing it into a tree renderer would invalidate accepted grass assumptions and is prohibited by this plan.

Trees may reuse the following established patterns without inheriting the implementation:

- deterministic dirty-time placement;
- Ground-relative authored coverage;
- structured instance buffers;
- `Graphics.RenderMeshIndirect` resource-lifetime conventions;
- explicit diagnostics and clipboard-copyable reports;
- no normal-frame placement rebuild;
- Weather-owned wind consumed in the shader;
- URP main-light-cookie cloud reception.

### Source and tracked-production asset policy

`Assets/References/Trees/` remains the source/reference vault and is ignored by the current repository rules.

The imported gallery may resolve the twenty FBXs from that local source vault. The gallery must fail clearly and non-destructively when the folder is absent. It must not create a hidden production dependency on ignored assets.

The generated production library must not depend on ignored source FBXs. Before generated variants are considered production-ready, required reusable textures and the CC0 license record must be copied into an approved tracked tree location with new metadata. The source-vault files remain unchanged. The exact tracked asset-copy patch is part of `TREE-LIB.1` and must be approved before execution.

### Imported reference gallery architecture

#### Proposed tracked code paths

```text
Assets/Game/Procedural/Trees/TreeFamily.cs
Assets/Game/Procedural/Trees/TreeReferenceGallery.cs
Assets/Game/Procedural/Trees/TreeReferenceSpecimen.cs
Assets/Game/Procedural/Trees/Editor/TreeReferenceGalleryEditor.cs
Assets/Game/Procedural/Trees/Editor/TreeReferenceGalleryBuilder.cs
Assets/Game/Procedural/Trees/Editor/TreeSourceAssetAudit.cs
Assets/Game/Rendering/Trees/Includes/TreeCommon.hlsl
Assets/Game/Rendering/Trees/Includes/TreeWindResponse.hlsl
Assets/Game/Rendering/Trees/Includes/TreeLighting.hlsl
Assets/Game/Rendering/Trees/Shaders/SH_StylizedTreeBark.shader
Assets/Game/Rendering/Trees/Shaders/SH_StylizedTreeFoliage.shader
```

The exact scene edit proposed for the gallery is:

```text
Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity
```

No raw scene-text editing is authorized. The standalone gallery object and its scene placement must be created through Unity. The gallery is not a child of `GeneratedGround`; it is created as a sibling under the same scene parent when possible, or as a root object when the Ground itself is root-level.

#### Gallery component responsibilities

`TreeReferenceGallery` must contain only gallery configuration and status. It must not perform expensive work in `Update` or regenerate children automatically from `OnValidate`.

Required responsibilities:

- store one explicit `GeneratedGround` reference used only for surface sampling and comparison placement;
- remain independent of the Ground hierarchy and never resolve ownership from an ancestor;
- provide an explicit Editor action to assign the closest available Ground when the reference is empty;
- provide an explicit Editor creation action that places the gallery as a sibling of the selected Ground, immediately after it in hierarchy order when possible;
- expose source scale, ground alignment, family spacing, pair spacing, wind enablement, and shadow test controls;
- expose the resolved Ground and source-folder status read-only;
- retain a generated-gallery revision and last audit summary;
- provide explicit build, rebuild, remove, audit, and copy-report actions through its custom Inspector.

Required Inspector actions:

```text
Hierarchy > Tree Reference Gallery > Inspector > Actions > Build Complete Imported Gallery
Hierarchy > Tree Reference Gallery > Inspector > Actions > Rebuild Complete Imported Gallery
Hierarchy > Tree Reference Gallery > Inspector > Actions > Remove Imported Gallery Children
Hierarchy > Tree Reference Gallery > Inspector > Diagnostics > Run Complete Tree Source Audit
Hierarchy > Tree Reference Gallery > Inspector > Diagnostics > Copy Last Tree Source Audit
```

Exact foldout names may change only if this document is updated before implementation.

#### Gallery hierarchy

The deterministic generated hierarchy is:

```text
Tree Reference Gallery
├── Common
│   ├── Common_01_Pair
│   │   ├── REF_CommonTree_1
│   │   └── PROC_CommonTree_1_SLOT
│   ├── Common_02_Pair
│   ├── Common_03_Pair
│   ├── Common_04_Pair
│   └── Common_05_Pair
├── Pine
│   ├── Pine_01_Pair
│   └── ...
├── Twisted
│   ├── Twisted_01_Pair
│   └── ...
└── Dead
    ├── Dead_01_Pair
    └── ...
```

Each pair owns one imported specimen and one reserved procedural comparison transform. The procedural slot must exist from the first gallery patch even though it is empty. Generated-tree authoring patches will populate those slots without changing the imported specimen placement.

Family rows are separated along the assigned Ground's local Z axis, and variant pair cells are separated along Ground-local X. Cell size and legal placement are calculated from the audited combined source bounds and the actual Ground surface domain rather than from unchecked fixed offsets. Imported and procedural roots remain symmetric around their pair centre, but that centre may shift within the Ground domain to accommodate strongly asymmetric source silhouettes.

#### Ground alignment

The gallery builder must use the gallery's explicitly assigned `GeneratedGround` reference. It resolves legal pair positions from `GeneratedGround.TryGetSurfaceDomain`, then calls `GeneratedGround.TrySampleBaseSurface` independently for the imported root and procedural comparison root. Each side therefore follows its own actual Ground height; hierarchy proximity does not establish the sampling target.

The source pivot must be audited before an offset is applied. The report must record:

- FBX root position;
- combined renderer local bounds;
- lowest visible local Y;
- applied ground correction;
- resulting visible height;
- resulting canopy width.

Default gallery placement may correct a non-rooted source pivot by moving the imported visual so its lowest audited point meets the sampled Ground. The original uncorrected pivot information must remain in the report. The procedural comparison uses the same visible-ground baseline, not the source FBX transform origin if that origin is unsuitable.

#### Labels

Family, variant, imported/procedural status, source height, triangle count, and procedural seed labels must be drawn by the custom Editor through Scene-view handles. The gallery must not add TextMeshPro, world-space canvases, runtime text meshes, layers, or tags merely to label specimens.

#### Source audit

`TreeSourceAssetAudit` must inspect all twenty FBXs and twelve textures before material or layout assumptions are locked.

The report must include, per FBX:

- asset path and GUID;
- importer scale and axis settings;
- root transform and child hierarchy;
- renderer count;
- submesh count;
- material-slot names and ordering;
- vertex and triangle counts;
- combined bounds;
- vertex-colour presence and channel min/max/average;
- UV channel availability;
- normal and tangent availability;
- readable/import settings relevant to later preprocessing.

The report must include, per texture:

- dimensions;
- texture type;
- sRGB state;
- alpha presence;
- mipmap state;
- compression settings;
- estimated runtime memory where Unity exposes it.

The audit must fail the gallery build when a required asset is missing or an unrecognized material-slot layout would cause bark and foliage to be assigned incorrectly. It must not guess silently from renderer order.

#### Import corrections

The three retained bark normal textures must be changed to proper normal-map import classification with sRGB disabled during the first approved import-normalization patch. No other importer setting changes are permitted unless the source audit proves they are required and this document is updated first.

The twenty source FBXs remain at their preserved source importer scale during the first gallery build. Scale normalization may be added as a comparison display mode, but must not overwrite importer scale.

#### Reference material ownership

The gallery uses shared transient or approved shared reference materials, never one material instance per tree.

Material identities are:

```text
Bark_NormalTree
Bark_DeadTree
Bark_TwistedTree
Leaves_NormalTree
Leaves_Pine
Leaves_TwistedTree
```

Common and Pine may share the normal-tree bark material only after the source audit confirms their material mapping.

The imported gallery initially uses the coloured foliage textures to reproduce the supplied reference appearance. The white foliage variants are displayed in a separate material diagnostic only when assessing the tintable procedural workflow. They must not silently replace the source-reference appearance.

#### Bark shader requirements

`SH_StylizedTreeBark.shader` must provide:

- opaque URP rendering;
- bark albedo and normal-map sampling;
- imported and generated vertex-colour/UV metadata support;
- tree-specific Weather wind response;
- main-light-cookie cloud reception;
- local-light compatibility consistent with the project's stylized lighting direction;
- conservative deformed bounds support through renderer-side metadata;
- an optional vertex-data debug mode for gallery validation;
- no grass interaction or trample sampling.

#### Foliage shader requirements

`SH_StylizedTreeFoliage.shader` must provide:

- alpha clipping;
- two-sided foliage rendering with an explicit normal-orientation policy;
- coloured and white/tintable foliage texture support;
- branch-following and card-flutter wind response;
- main-light-cookie cloud reception;
- deterministic card/cluster tint variation;
- optional seasonal-retention input reserved for generated foliage;
- optional vertex/pivot debug modes;
- no alpha blending in the initial production path.

The clip threshold must be exposed and audited at gameplay-camera distance. Alpha-to-coverage or dithered transitions are separate future decisions and are not part of the first gallery patch.

#### Imported-tree wind data

The source bark vertex-colour gradient is not accepted as a wind mask until the Unity audit verifies it visually and numerically.

The gallery shader must support two explicit imported-tree mask modes:

1. `VertexColourRed` — uses the verified bark vertex-colour gradient;
2. `BoundsHeightFallback` — derives root-to-crown weighting from audited local bounds.

There must be no automatic mode ambiguity. The selected mode and evidence must be written to the audit report.

Because the imported foliage cards lack attachment metadata, the initial gallery may use a crown-level height-weighted displacement plus restrained procedural flutter. A later optional source-card preprocessing patch may derive card pivots for a more exact imported comparison, but it is not a blocker for beginning procedural generation.

#### Gallery shadow policy

The gallery must separate material, wind, and cloud comparison from shadow-cost comparison.

Initial defaults:

- bark casts and receives ordinary shadows;
- foliage receives shadows;
- foliage shadow casting is an explicit gallery toggle and defaults off until the foliage alpha-shadow pass is validated;
- no production shadow-proxy architecture is inferred from the temporary gallery setting.

#### Gallery acceptance gate

The gallery baseline is accepted only when:

- all twenty source trees build with no missing FBX, texture, material, or submesh assignment;
- all twenty sit on the same Ground reference plane without unexplained pivot offsets;
- source dimensions and geometry counts are recorded;
- bark normal maps render with corrected import classification;
- all tree surfaces receive the authoritative cloud cookie exactly once;
- Weather wind affects every intended specimen and does not move the root;
- Dead trees contain no accidental foliage assignment;
- family labels and procedural slots are correct;
- a complete clipboard-copyable audit exists;
- ordinary Play Mode causes no regeneration, duplicate children, or persistent material leaks.

### Procedural generated-tree architecture

#### Production principle

The generator produces a finite accepted library of deterministic variants in the Editor. The runtime renderer instances those baked variants. Unique procedural meshes are reserved for landmarks or explicitly approved runtime/world-generation cases.

The initial production library target is a minimum of eight accepted variants per family, for at least thirty-two generated variants total. More variants may be added without changing architecture. Rejected generation outputs do not count toward the target.

#### Proposed source paths

```text
Assets/Game/Procedural/Trees/TreeFamilyProfile.cs
Assets/Game/Procedural/Trees/TreeReferenceCalibrationPreset.cs
Assets/Game/Procedural/Trees/TreeMaterialPalette.cs
Assets/Game/Procedural/Trees/TreeGenerationRecipe.cs
Assets/Game/Procedural/Trees/TreeGenerationOverrides.cs
Assets/Game/Procedural/Trees/TreeDefinition.cs
Assets/Game/Procedural/Trees/TreeBranchDefinition.cs
Assets/Game/Procedural/Trees/TreeFoliageClusterDefinition.cs
Assets/Game/Procedural/Trees/TreeGenerationMetrics.cs
Assets/Game/Procedural/Trees/TreeGenerator.cs
Assets/Game/Procedural/Trees/TreeMeshBuilder.cs
Assets/Game/Procedural/Trees/TreeFoliageBuilder.cs
Assets/Game/Procedural/Trees/TreeLodBuilder.cs
Assets/Game/Procedural/Trees/TreeProxyBuilder.cs
Assets/Game/Procedural/Trees/ProceduralTreeInstance.cs
Assets/Game/Procedural/Trees/Editor/ProceduralTreeInstanceEditor.cs
Assets/Game/Procedural/Trees/Editor/TreeGalleryGenerationCoordinator.cs
Assets/Game/Procedural/Trees/GeneratedTreeLibrary.cs
Assets/Game/Procedural/Trees/GeneratedTreeVariant.cs
Assets/Game/Procedural/Trees/Editor/GeneratedTreeLibraryBaker.cs
Assets/Game/Procedural/Trees/Editor/GeneratedTreeLibraryValidator.cs
```

The plan intentionally separates authoring/generation from runtime rendering. No base class shared with `VegetationRendererBase` is proposed.

#### Data flow

```text
TreeFamilyProfile
    + optional TreeReferenceCalibrationPreset
    + TreeGenerationRecipe
    + TreeGenerationOverrides
    + TreeMaterialPalette
    + master seed / locked subsystem seeds
    -> resolved parameter set + dependency fingerprint
    -> TreeDefinition
        -> branch graph
        -> foliage cluster graph
        -> material assignments
        -> physical footprint
        -> generation metrics
    -> TreeMeshBuilder / TreeFoliageBuilder
        -> LOD0 bark mesh
        -> LOD0 foliage mesh
    -> TreeLodBuilder
        -> LOD1 bark/foliage
        -> LOD2 bark/foliage
    -> TreeProxyBuilder
        -> shadow proxy
        -> collision proxy metadata
    -> GeneratedTreeLibraryBaker
        -> tracked baked mesh assets
        -> deterministic fingerprints
        -> accepted variant records
```

The branch graph is the structural source of truth. Meshes are outputs and must never become the only representation available during generation or validation.

#### Generated-tree authoring goals and layered control model

The procedural system has two equally important goals:

1. **reference calibration** — produce trees recognizably derived from the imported Common, Pine, Twisted, and Dead families, including optional presets targeting each of the twenty individual references;
2. **authored expansion** — produce trees that deliberately exceed the reference pack through richer crowns, larger foliage regions, independent foliage density, more or fewer branches, stronger or weaker curvature, palette changes, age, damage, asymmetry, and environmental shaping.

The generator must not reduce these goals to one enormous mutable profile. `TREE-PLAN.3` simplifies the public authoring model and supersedes the earlier five-layer description.

The authoritative authoring chain is:

```text
TreeFamilyProfile
    -> TreeGenerationRecipe
        -> InstanceOverrides (optional)
            -> deterministic SeedVariation
```

There is no separate “reference-match mode.” The twenty comparison recipes are ordinary named recipes whose defaults and ranges are calibrated against the imported references. Imported dimensions, source identity, and comparison tolerances are reference-target metadata used by the gallery and diagnostics; they are not a user-facing authoring mode.

The existing serialized `TreeReferenceCalibrationPreset` type may remain temporarily as compatibility storage for imported target measurements while the implementation is migrated. It must not remain an independent public authoring layer, own colour-mode behavior, or duplicate recipe controls. New authoring UI and documentation must present family template, named recipe, optional instance overrides, and deterministic variation only.

##### Layer 1 — family template

A family profile is a reusable base template for Common, Pine, Twisted, or Dead. It owns:

- legal and safety ranges;
- structural budgets;
- family-default parameter ranges;
- default material palette and textures;
- default branch grammar;
- default foliage grammar;
- environmental-response defaults.

A family is not a rigid final tree. It supplies sensible starting behavior. For example:

- Common defaults to a relatively straight, moderately thin, comparatively symmetrical trunk with a higher branch/crown start;
- Pine defaults to a strong central leader, tiered attachments, and restrained radial symmetry;
- Twisted defaults to stronger centerline displacement, optional spiral/twist, irregular branching, and broader asymmetry ranges;
- Dead defaults to no living foliage, branch loss/breakage, exposed structure, and configurable symmetry rather than mandatory one-sidedness.

##### Layer 2 — named ranged recipe

A recipe begins from one family template and defines a named reusable tree concept. Examples:

```text
Short Pine
Wide Old Pine
Twisted Pine
Common — Rich Crown
Common — Bare Lower Trunk
Dead — Strong Spiral
Dead — Wind-Swept Left
```

Every recipe control may be:

- inherited from the family;
- an exact value;
- a deterministic minimum/maximum range.

The seed selects values inside recipe ranges. One recipe can therefore produce multiple related trees without becoming visually identical or escaping the intended style.

The twenty gallery comparison variants are ordinary recipes named for their imported targets. They use exact or narrow ranges where necessary to reproduce the reference family grammar, dimensions, branch rhythm, and baseline appearance. They do not activate a special rendering or colour mode.

##### Layer 3 — instance overrides

Instance overrides are sparse exact changes for one generated specimen or comparison slot. Unset fields inherit from the recipe. They exist for deliberate exceptions, not as the ordinary way to author the library.

Examples:

```text
Overall Crown Volume = 1.40
Primary Branch Count = 12
Trunk Spiral Strength = 0.65
Lowest Primary Branch Height = 0.48
Branch Arch Direction = +1.00
Branch Arch Strength = 0.35
Azimuth Symmetry = 0.85
Directional Bias Strength = 0.10
Bark Tint = authored value
```

The resolved report must identify whether every value came from the family, recipe, instance override, or seed-selected point inside a range.

##### Layer 4 — deterministic seed variation

Seed variation resolves remaining ranges and must never silently exceed family safety limits. The same complete input set and generator version must produce the same structural, foliage-intent, palette, and mesh fingerprints.

Changing one subsystem must not randomize unrelated systems. In particular:

- palette changes preserve geometry;
- foliage volume/density changes preserve trunk and branches;
- branch-shape changes preserve palette;
- branch-start-height changes intentionally rebuild branch layout but not trunk shape;
- trunk centerline/spiral changes intentionally invalidate descendants;
- symmetry or directional-bias changes invalidate branch layout but not material palette.

#### Foliage volume, crown fullness, and geometry density

Foliage **volume** and foliage **density** are separate architectural controls.

- **Volume** controls the spatial extent occupied by foliage regions.
- **Density** controls how much foliage geometry is placed inside those regions.

Richer Common-tree crowns must therefore be possible without a proportional increase in cards, triangles, or overdraw.

Required volume controls:

```text
Overall Crown Volume
Crown Width Scale
Crown Height Scale
Crown Length / branch-axis scale
Crown Start Height
Lower Crown Width
Upper Crown Width
Crown Roundness
Crown Top Taper
Crown Lobe Count
Crown Lobe Radius
Crown Lobe Irregularity
Crown Fill / gap suppression
Cluster Width Scale
Cluster Height Scale
Cluster Length Scale
Cluster Radial Spread
Card Size Scale
```

Required density controls:

```text
Foliage-eligible branch probability
Foliage Cluster Count
Clusters Per Eligible Branch
Cards Per Cluster
Cluster Occupancy
Terminal Foliage Probability
Card Retention Fraction
```

Changing only volume controls must preserve the trunk and branch graph and should preserve foliage cluster/card identities where feasible. Changing density may add or remove foliage records but must not regenerate the trunk or branch graph.

Performance reports must distinguish:

- crown spatial bounds;
- foliage-cluster count;
- card count;
- foliage triangles;
- estimated alpha-tested area/overdraw proxy.

#### Crown-envelope and family-specific foliage controls

Foliage placement is constrained by a family-owned crown envelope rather than random attachment alone.

Common requires broad, potentially rich crowns with controllable lobes, internal gaps, top taper, and asymmetric fullness. Pine requires central-leader-relative tiers, branch-tip foliage length, tier thickness, cone profile, crown start, and lower-branch retention. Twisted requires sparse directional lobes, permanent exposure bias, and strong asymmetry. Dead normally suppresses living foliage but may support authored remnant/dead foliage later without changing the branch architecture.

Generated foliage clusters must reference both their parent branch and the resolved crown envelope. Branch ownership controls wind attachment; the crown envelope controls family silhouette and spatial fullness.

#### Branch count and branching-distribution controls

Branch population is first-class authored data.

Required controls include:

```text
Primary Branch Count or Count Range
Secondary Branches Per Primary
Tertiary Branches Per Secondary
Maximum Branch Order
Branch Spawn Probability by Order
Branch Attachment Height Distribution
Minimum Attachment Spacing
Angular / yaw distribution
Branch Side Bias
Branch Tier Count
Branches Per Tier
Tier Irregularity
Terminal Split Probability
Lower Branch Retention
Missing Branch Probability
Dead Branch Probability
Break Probability
```

Counts may be exact or ranged. Exact counts are useful for calibration and authored silhouettes. Ranges are useful for variant libraries. The resolved report must state requested, accepted, rejected, removed, dead, and broken counts by branch order.

Family semantics differ:

- Common uses distributed or clustered attachments around the trunk;
- Pine uses tiers or semi-whorls around a central leader;
- Twisted uses sparse asymmetric placement and directional bias;
- Dead may generate from a living-family grammar and then apply removal/breakage, or use a dedicated dead grammar selected by the recipe.

#### Trunk and branch curvature controls

Curvature is not one generic slider. Trunk, primary branches, and higher-order branches own distinct controls.

Required trunk controls:

```text
Trunk Curvature Strength
Trunk Curve Frequency / Bend Count
Trunk Directional Drift
Permanent Lean Strength
Permanent Lean Direction
Trunk Torsion / Twist
Trunk Irregularity
Trunk Control-Point Count
Trunk Fork Probability
Trunk Fork Height
```

Required primary-branch controls:

```text
Primary Branch Curvature
Primary Branch Droop
Primary Branch Upward Bias
Primary Branch Side Sweep
Primary Branch Torsion
Primary Branch Irregularity
Primary Branch End Curl
Gravity Bias
Permanent Wind / exposure bias
```

Secondary and tertiary controls may inherit scaled versions of primary-branch values or expose explicit overrides where the family needs them.

The architecture must support combinations such as:

```text
straight trunk + curved branches
curved trunk + straighter branches
strong trunk lean + wind-shaped crown
irregular trunk + comparatively symmetrical crown
```

Curve generation must remain compatible with stable parallel-transport frames. Strong curvature or torsion may increase sample/ring counts according to the LOD budget but must not create frame flips, NaNs, zero-length segments, or detached branch junctions.

#### Bark and foliage colour architecture

Bark and foliage colours are data, not unique materials per generated tree.

`TreeMaterialPalette` owns shared material identity and authored colour ranges. Initial foliage controls:

```text
Base Foliage Colour
Foliage Highlight Colour
Foliage Shadow Colour
Hue Variation
Saturation Variation
Value Variation
Cluster Colour Variation
Top-to-Bottom Colour Gradient
Seasonal / state colour input reserve
```

Initial bark controls:

```text
Base Bark Tint
Bark Hue Shift
Bark Saturation
Bark Value
Root Darkening
Upper-Trunk Variation
Branch-Order Variation
Moss Tint reserve
Wetness response reserve
```

The retained white/tintable foliage textures are the preferred generated-tree base when visual validation confirms their alpha and value structure. Coloured foliage textures remain available for imported-reference matching and optional calibrated palettes.

Palette resolution occurs at family-template, recipe, and instance-override layers. Imported comparison target metadata does not add a palette mode. Runtime variation is supplied through property blocks or instance data. The system must not create one material asset per tree.

Colour-only changes must not regenerate structural or mesh topology. They update palette/material fingerprints only.

#### Deterministic stream isolation and selective regeneration

One shared random sequence is prohibited. Required independent streams are:

```text
TrunkShape
TrunkForks
PrimaryBranchLayout
SecondaryBranchLayout
TertiaryBranchLayout
BranchCurvature
StructuralDamage
FoliageClusterPlacement
FoliageClusterShape
FoliageCardPlacement
FoliageCardShape
MaterialVariation
LODSelection
ProxyGeneration
```

Each stream derives from:

```text
master seed
family identity
reference-calibration identity/version
recipe identity/version
generator version
stream identifier
optional locked stream seed
```

Dependency rules:

- foliage colour changes invalidate no geometry;
- bark colour changes invalidate no geometry;
- foliage volume changes invalidate foliage bounds/mesh outputs but not trunk or branch structure;
- foliage density changes invalidate foliage definitions/meshes but not trunk or branch structure;
- branch-count or branch-curvature changes invalidate affected branch orders, attached foliage, LODs, and proxies but preserve unrelated palette state;
- trunk shape changes invalidate the full descendant branch graph and all geometry/proxies;
- proxy or LOD-budget changes do not alter LOD0 structural identity.

Every generated artifact stores a dependency fingerprint. The authoring UI must clearly report which outputs are stale after a change.

#### Generated metadata reserved for authoring and rendering

The structural and mesh contracts must preserve enough metadata for later rendering and editing:

```text
normalized tree height
normalized distance along current branch
branch order
stable branch ID
parent branch ID
branch stiffness
branch phase
foliage cluster ID
foliage card ID
cluster random value
normalized crown height
cluster/card pivot
parent branch axis
seasonal-retention threshold
```

Exact channel packing remains a later mesh-patch decision, but no generated foliage system may depend on imported object-space hash cells as its authoritative identity source.

#### Authoring diagnostics and acceptance tests

`TREE-GEN.1` structural diagnostics must report both resolved parameters and ownership:

- family profile;
- calibration preset, if any;
- variant recipe;
- non-default instance overrides;
- every derived seed stream;
- resolved branch-count and curvature values;
- resolved crown-volume and foliage-density values;
- resolved bark/foliage palette values;
- dependency and structural fingerprints;
- stale-output flags.

Required determinism tests:

1. same complete inputs regenerate the same structural fingerprint;
2. foliage colour changes preserve structural and foliage-geometry fingerprints;
3. foliage-volume changes preserve trunk and branch fingerprints;
4. foliage-density changes preserve trunk and branch fingerprints;
5. branch-count changes preserve trunk and palette fingerprints;
6. trunk-curvature changes intentionally invalidate the descendant structure;
7. locked subsystem seeds remain stable while an unlocked subsystem is randomized.

#### `TreeFamilyProfile`

One profile defines family grammar and family response, not an individual tree.

Required categories:

- identity and reference-family mapping;
- hard safety ranges and author-facing default ranges;
- overall height, crown aspect, crown start, crown envelope, lobe, fullness, asymmetry, and volume ranges;
- foliage spatial-volume controls separate from foliage density/card-count controls;
- trunk taper, lean, curvature, bend frequency, directional drift, twist, flare, fork, and ring-resolution ranges;
- branch-order limits and exact/ranged count controls by order;
- branch attachment-height, spacing, tier/whorl, yaw, side-bias, and angular-distribution rules;
- primary/secondary/tertiary length, taper, curvature, droop, upward bias, side sweep, torsion, irregularity, gravity bias, and permanent wind/exposure bias;
- foliage eligibility by branch order, branch position, terminal state, and crown envelope;
- foliage-cluster width, height, length, radial spread, shape, count, card count, card size, orientation, occupancy, retention, and tint ranges;
- dead-branch, missing-branch, breakage, and foliage-retention probabilities;
- whole-tree, branch, and foliage wind response;
- default `TreeMaterialPalette` plus approved palette ranges;
- LOD budgets;
- shadow/collision proxy settings;
- accepted world-footprint range;
- dependency declarations used for selective invalidation and stale-output reporting.

Profiles must contain validation ranges and must reject impossible or zero-area configurations rather than relying on the mesh builder to recover silently.

#### `TreeGenerationRecipe`

A recipe defines one reproducible authored variant request.

Required contents:

- family profile;
- optional reference-calibration preset;
- deterministic master seed;
- generator version;
- age/size class;
- optional material palette override;
- optional permanent lean direction and strength;
- optional damage state;
- optional foliage-retention state;
- exact or ranged branch-count overrides;
- trunk and per-order branch curvature overrides;
- crown-envelope, crown-volume, and crown-fill overrides;
- foliage-density overrides independent from foliage-volume overrides;
- optional locked subsystem seeds;
- sparse `TreeGenerationOverrides` inside profile-approved ranges.

Selective locking is required so the user can keep a successful trunk while regenerating branches or foliage. Each independent subsystem uses a derived deterministic random stream rather than consuming one shared random sequence whose order changes when unrelated code changes.

A recipe must expose a resolved-parameter report and ownership trace. Authors must be able to distinguish family defaults, calibration values, recipe changes, instance overrides, and seed-selected values.

Required seed streams are defined in the deterministic stream-isolation contract above. The implementation may split streams further but may not merge unrelated structural and material streams into one order-dependent sequence.

#### `TreeReferenceCalibrationPreset`

A calibration preset is a reusable authored object that targets one imported reference or one recognizable source-derived silhouette class.

Required contents:

- source family and optional source FBX GUID/path for diagnostics;
- target height, width, crown start, and main silhouette bounds;
- target trunk lean, curvature, taper, fork, and torsion ranges;
- primary-branch count and attachment rhythm;
- major branch direction, elevation, droop, and length targets;
- target crown envelope, lobe positions, fullness, and major gaps;
- foliage-volume and density targets;
- damage/dead-branch targets;
- optional palette target used for reference matching;
- calibration tolerance and comparison metrics.

The preset is not a generated mesh cache and does not own runtime rendering.

#### `TreeGenerationOverrides`

Overrides are sparse, optional, serializable values applied after the profile, calibration preset, and recipe. They support exact author requests without duplicating entire profiles or recipes.

Each override field records whether it is unset, exact, or ranged. Overrides participate in deterministic fingerprints and validation. Unsupported or out-of-range values fail explicitly rather than being silently clamped unless the specific field documents clamping as its authoring behaviour.

#### `TreeMaterialPalette`

A palette owns shared bark/foliage texture identity and authored colour ranges. It may reference the retained/calibrated source appearance or a generated tintable workflow. Palettes are reusable across many variants and instances; they are not created per tree.

#### `TreeDefinition`

`TreeDefinition` is a pure generated description with no Unity scene ownership.

Required contents:

- family and recipe identity;
- generator-version number;
- root transform and local up axis;
- trunk branch index;
- flat branch list with parent indices;
- foliage cluster list with parent-branch indices;
- overall bounds and footprint;
- deterministic fingerprint inputs;
- warnings and rejected-feature counts.

A branch record must contain at least:

- parent branch index;
- attachment distance on parent;
- branch order;
- curve control points or sampled centreline;
- radius profile;
- local reference axis;
- stiffness;
- phase;
- material class;
- damage/break state;
- foliage-eligibility range.

A foliage-cluster record must contain at least:

- parent branch index;
- attachment position and branch distance;
- cluster orientation;
- shape and extent;
- card count and size range;
- density;
- tint/retention ranges;
- stiffness and phase.

#### Structural generation

The V1 generator uses stylized curve hierarchies, not botanical growth simulation.

The trunk and each branch are represented as sampled tapered curves. The generator must use a stable frame transport method such as parallel transport to orient successive mesh rings. Recomputing each ring directly from world up is prohibited because it causes frame flips on strongly curved or twisted branches.

Branch generation occurs from family-owned attachment rules. Each branch is created in parent-local coordinates and then resolved into tree-local space. Gravity, permanent environmental bias, and family asymmetry may alter the branch curve at generation time.

Initial branch junction policy:

- child tubes begin slightly inside or overlap the parent tube;
- junctions do not require runtime booleans;
- watertight blended forks are deferred unless visible seams fail the gameplay-camera comparison;
- hidden internal overlap triangles are accepted within recorded budgets;
- visible gaps, detached branches, and z-fighting are not accepted.

#### Mesh construction

For every sampled branch curve:

1. choose ring locations from length and curvature;
2. calculate a transported frame;
3. evaluate the radius profile;
4. emit the family/LOD radial ring resolution;
5. connect adjacent rings with consistent winding;
6. cap exposed branch ends where required;
7. calculate analytic or geometry-derived normals;
8. calculate tangents compatible with the bark normal map;
9. assign cylindrical bark UVs using physical distance;
10. assign tree wind and branch metadata.

Bark UV convention:

```text
U = angle around the local branch ring
V = accumulated physical distance along the branch * bark tiling scale
```

Each branch may receive a deterministic V offset and restrained U rotation to reduce visible repetition. UV scale must remain consistent across branches of different lengths.

Generated vertex normals and tangents are mandatory. Source FBX vertex normals cannot be transferred to materially different generated topology. The retained bark normal-map textures are reusable; the retained source mesh vertex normals are reference evidence only.

#### Generated bark vertex-data contract

Initial locked channel proposal:

```text
COLOR.r = normalized tree-root-to-vertex flexibility
COLOR.g = local branch stiffness response
COLOR.b = normalized branch order
COLOR.a = deterministic branch phase
UV2.x    = normalized distance along current branch
UV2.y    = normalized parent attachment height
UV2.z    = damage/break response reserve
UV2.w    = reserved
```

The mesh validator must verify every emitted value is finite and within its documented range. Any material change to this channel contract requires this document and both tree shaders to be updated in the same approved patch.

#### Foliage generation

Generated foliage uses the retained foliage textures on procedurally placed cards.

The V1 foliage system supports family-specific clusters made from:

- crossed cards;
- radial card fans;
- irregular compact card clouds;
- elongated branch-aligned clusters for Pine;
- sparse asymmetric clusters for Twisted.

Every card must know its exact parent attachment. Cards are generated around a cluster pivot, not scattered without branch ownership.

Initial generated foliage channel proposal:

```text
UV2.xyz  = foliage card pivot in tree-local space
UV2.w    = deterministic card phase
UV3.xyz  = parent branch axis in tree-local space
UV3.w    = local card stiffness
COLOR.r  = deterministic seasonal-retention threshold
COLOR.g  = tint variation
COLOR.b  = cluster variation
COLOR.a  = normalized tree-root-to-card flexibility
```

This contract allows the foliage vertex shader to move the pivot with the branch response and then apply local card flutter around that pivot.

#### Family grammar targets

##### Common

- moderately straight trunk with controlled curvature;
- broad crown;
- distributed branch heights rather than strict tiers;
- medium primary-branch density;
- upward young branches and more horizontal mature branches;
- high foliage density with deliberate crown gaps;
- strongest seasonal tint and retention range;
- moderate trunk and crown flexibility.

##### Pine

- dominant central leader;
- narrow tapered crown;
- repeated or semi-whorled branch tiers;
- stronger lower-branch droop;
- foliage concentrated along branch ends and outer branch lengths;
- stiffer trunk and lower macro displacement;
- finer, faster foliage flutter;
- high future snow-retention capacity.

##### Twisted

- strong trunk curvature and torsion;
- asymmetric branch distribution;
- permanent lean and directional crown bias;
- irregular taper and branch termination;
- sparse foliage;
- higher dead-branch probability;
- low placement density and strong silhouette rejection criteria.

##### Dead

- exposed branch structure;
- no foliage by default;
- irregular broken endings;
- high branch-removal and break probability;
- optional broken leader;
- minimal wind response except upper thin branches.

The long-term architecture allows a dead/damage state to be applied to living-family structural definitions. V1 may retain a dedicated Dead profile to match the provided reference family, but damage code must not be hard-coded only to that profile.

#### Procedural comparison workflow

`ProceduralTreeInstance` is the Editor-facing owner of one generated gallery specimen. The `Tree Reference Gallery` coordinator is the sole normal authoring entry point; there is no parallel standalone tree-authoring component.

Required actions:

```text
Generate Current Recipe
Regenerate Trunk
Regenerate Branches
Regenerate Foliage
Rebuild LODs
Rebuild Proxies
Place In Assigned Gallery Slot
Copy Complete Generation Report
Bake Accepted Variant To Library
```

The first generated specimen for each family must be placed beside the designated source target:

```text
CommonTree_1
Pine_5
TwistedTree_1
DeadTree_1
```

All four comparisons must use the same Ground, Weather field, cloud cookie, camera, lighting, and reference material textures.

The comparison report must record:

- source and generated visible dimensions;
- source and generated vertex/triangle counts;
- branch and foliage counts;
- LOD budgets;
- material and texture identity;
- maximum predicted wind displacement;
- generation duration;
- deterministic hash;
- visual acceptance notes entered by the user.

#### LOD generation

LOD generation must use structural knowledge rather than generic decimation as the only method.

Provisional V1 budgets per generated tree:

| Output | Vertices | Triangles | Foliage-card target |
| --- | ---: | ---: | ---: |
| LOD0 | <= 8,000 | <= 6,000 | <= 96 |
| LOD1 | <= 3,500 | <= 2,500 | <= 48 |
| LOD2 | <= 1,200 | <= 800 | <= 16 |
| Shadow proxy | <= 300 | <= 200 | no full foliage cards |

These are initial hard generation budgets, not a claim that every final visual target will fit without tuning. A requested budget increase requires measured evidence, this document to be updated, and explicit approval.

LOD policy:

- LOD0 retains full accepted trunk, branch, and foliage structure;
- LOD1 removes smallest branch order, reduces radial segments, and reduces foliage cards while preserving crown silhouette;
- LOD2 retains trunk and major branch silhouette with coarse foliage clusters;
- far impostors are deferred until gameplay-camera profiling proves they are required;
- family, variant identity, stable transform, and permanent lean never change across LODs.

#### Shadow and collision proxies

The initial production shadow policy is:

- bark and major branches use a simplified opaque shadow proxy;
- full alpha-tested foliage shadow casting is not the default production solution;
- canopy shadow representation is simplified and profile-owned;
- gallery full-foliage shadow testing is diagnostic only.

Collision output is metadata plus simple proxy geometry:

- trunk capsule or tapered-cylinder dimensions;
- optional major-root footprint;
- no foliage collision;
- no source-mesh collider;
- no branch-by-branch physics collider in V1.

The runtime collision-instantiation strategy is deferred to gameplay placement and navigation work. The generator must nevertheless emit an authoritative footprint and trunk proxy so placement does not later infer occupancy from render geometry.

#### Generated library

`GeneratedTreeLibrary` must store family entries and accepted variant records.

Each accepted variant record contains:

- family;
- recipe seed and locked sub-seeds;
- generator version;
- profile fingerprint;
- deterministic content hash;
- LOD mesh references;
- bark and foliage bounds;
- combined conservative wind bounds;
- shadow-proxy reference;
- collision/footprint metadata;
- material/profile references;
- geometry and foliage metrics;
- acceptance status and notes.

The baker must:

- generate only from explicit user action;
- rebuild only stale variants unless a full rebuild is requested;
- avoid changing accepted non-stale variants;
- clean obsolete generated subassets through an explicit cleanup action;
- produce a complete clipboard-copyable bake report;
- never regenerate the whole library from `OnValidate`, domain reload, scene load, or ordinary Play Mode entry.

The minimum V1 library acceptance target is eight approved variants in each family. Generation may create additional candidates, but only explicitly accepted variants enter the production library.

#### Production texture promotion

Before the generated library is declared production-ready, the twelve retained tree textures and a license copy must be copied to an approved tracked tree-rendering location. The generated materials and profiles must reference the tracked copies. The reference gallery may continue comparing against the original source-vault texture identities when needed.

The texture-promotion patch must:

- copy rather than move source-vault assets;
- create new unique metadata;
- classify bark normals correctly;
- record content hashes proving the copies match the retained source files;
- retain both coloured and white/tintable foliage variants until the procedural material workflow is visually accepted;
- include the CC0 license record beside or above the tracked derived assets.

### Runtime generated-tree rendering architecture

The runtime renderer is implemented only after the baked library is accepted.

Proposed paths:

```text
Assets/Game/Procedural/Trees/TreeInstanceData.cs
Assets/Game/Procedural/Trees/TreeRenderer.cs
Assets/Game/Procedural/Trees/TreeRenderBatch.cs
Assets/Game/Rendering/Trees/Includes/TreeInstance.hlsl
```

Required behaviour:

- one structured instance record per placed tree;
- source-mesh/LOD batching by accepted library variant;
- bark and foliage draw ownership kept explicit;
- chunk-level activation and frustum rejection first;
- distance-based LOD grouping;
- no MonoBehaviour `Update` per rendered tree;
- no runtime mesh generation during ordinary gameplay;
- no full-population buffer rebuild every frame;
- conservative bounds include maximum approved wind displacement;
- resource release follows existing indirect-renderer lifecycle discipline.

A provisional tree instance record may contain:

```text
world position
uniform scale
rotation/yaw
variant index
family/material index
wind phase
stiffness variation
bark tint variation
foliage tint variation
seasonal state reserve
stable world ID
```

The exact packed layout must be validated against shader stride before upload and documented beside the implementation.

The renderer should begin with CPU chunk culling because the project expects a limited active/visible chunk set. GPU per-tree culling or compaction is deferred until profiling proves CPU chunk/LOD grouping insufficient.

### Tree placement architecture

Production placement follows renderer acceptance.

Proposed paths:

```text
Assets/Game/Procedural/Trees/GroundTrees.cs
Assets/Game/Procedural/Trees/TreeLayer.cs
Assets/Game/Procedural/Trees/TreePlacementRecipe.cs
Assets/Game/Procedural/Trees/Editor/GroundTreesEditor.cs
Assets/Game/Procedural/Trees/Editor/TreeLayerEditor.cs
```

`GroundTrees` coordinates direct child tree layers and automatically resolves its parent `GeneratedGround`. A child layer must not require the user to reassign the Ground it already inherits through the hierarchy.

A `TreeLayer` owns:

- authored coverage;
- one placement recipe;
- seed;
- density/spacing controls;
- family composition;
- Ground suitability controls;
- exclusion controls;
- placement cache/revision;
- generated instance records.

A placement recipe defines an ecological composition rather than exactly one family. Example:

```text
Mixed Lowland Forest
Common 70%
Pine 20%
Dead 8%
Twisted 2%
```

Tree placement samples the complete `GroundSurfaceSample` where useful:

- height and geometry/render normals;
- surface variation;
- exposure;
- damp deposit;
- vegetation suitability;
- compaction;
- shore influence;
- rocky/dry influence;
- material classification.

Placement is deterministic dirty-time work. The initial accepted algorithm is a deterministic priority-grid minimum-distance solver:

1. subdivide the Ground domain using the smallest allowed spacing;
2. generate one deterministic jittered candidate per occupied cell;
3. calculate coverage and Ground suitability;
4. assign a deterministic priority hash;
5. process candidates in priority order;
6. reject candidates within the selected footprint radius of an accepted neighbour using a spatial hash;
7. choose family and library variant from deterministic weighted recipes;
8. emit stable IDs from layer seed and candidate cell identity.

This produces noise-like minimum-distance placement without requiring an order-sensitive unbounded rejection loop.

Initial placement exclusions include:

- outside authored coverage;
- unsuitable slope;
- strong compaction/road state;
- standing-water or river interior;
- explicit object/structure clearances;
- family-specific unsuitable exposure or substrate;
- overlap with an accepted tree footprint.

No new layer or tag is required for the initial placement system. Object exclusions must use approved project geometry/footprint contracts or explicitly assigned sources.

### Performance and memory policy

The expected first-order tree costs are foliage overdraw, shadow rendering, and total submitted LOD geometry. Instance-buffer memory and Weather field memory are secondary.

Mandatory performance controls:

- editor-baked shared variants rather than one unique mesh per ordinary tree;
- explicit LOD budgets;
- alpha-clipped foliage rather than alpha blending;
- reduced-card LODs;
- simplified shadow proxies;
- chunk culling;
- no per-tree managed update;
- no runtime generation in combat;
- no full-foliage shadow casting by default;
- matched 1440p gameplay-camera profiling on the low-end target class before production acceptance.

The generated library report must total:

- mesh memory by family and LOD;
- tracked texture memory;
- average and maximum geometry per variant;
- foliage-card counts;
- variant count;
- shadow-proxy cost.

The runtime benchmark must report at least:

- active/visible tree counts;
- trees per LOD;
- bark and foliage draw submissions;
- submitted triangles;
- instance-buffer bytes;
- CPU rebuild/culling time;
- GPU frame-time comparison with foliage and shadows independently toggled.

### Future interaction reservations

The following are explicitly deferred and are not blockers for gallery or generated-library work:

- tree chopping and branch destruction;
- falling-tree physics;
- fire propagation;
- gameplay collision pooling;
- navigation-carving strategy;
- snow accumulation implementation;
- precipitation wetness implementation;
- seasonal controller implementation;
- frost, moss, corruption, and damage overlays;
- root growth around terrain and structures;
- actor/tree contact deformation;
- unique procedural landmark generation at runtime.

The generator must preserve structural branch data, stable IDs, family identity, footprint, and material response channels so those systems can be added without replacing the library architecture.

### Ordered implementation patches

No patch may skip its predecessor's acceptance gate unless this document is updated and the deviation is explicitly approved.

| ID | Scope | Required result | Status |
| --- | --- | --- | --- |
| TREE-PLAN.1 | Update this canonical document with the gallery-first and procedural-library architecture, exact sequencing, contracts, risks, and validation gates. | Persistent implementation plan exists before code changes. | Complete in documentation patch; Unity validation not required. |
| TREE-GALLERY.1 | Complete Unity source audit for all twenty FBXs and twelve textures; add tree family enum, audit code, standalone gallery component/editor shell, sibling/root creation utility, explicit Ground reference, and clipboard report. Do not create final materials or procedural code. | Exact submesh/material mapping, bounds, geometry counts, vertex-colour evidence, texture dimensions, and importer state are known. | Complete. Unity 6000.5.0f1 source audit passed on 2026-07-23 with 20/20 models, 12/12 textures, zero failures, and three expected bark-normal warnings. |
| TREE-GALLERY.2 | Correct the three bark normal imports; implement shared bark/foliage shaders and tree includes; consume Weather wind and the URP cloud cookie; add vertex/pivot debug modes; build a four-family imported vertical slice and reserve matching procedural slots. `TREE-GALLERY.2B` adds the reusable foliage readability/shadow contract. | One representative Common, Pine, Twisted, and Dead source tree renders with correct materials, cloud shading, controlled wind, Ground alignment, foliage-cast shadows, softened foliage shadow reception, diagnostics, and removable/rebuildable Editor ownership. | Complete and live-validated. |
| TREE-GALLERY.3 / 3A | Implement the complete deterministic off-map gallery builder, twenty source specimens, twenty procedural slots, Scene labels, shadow receiver pads, remove/rebuild actions, and complete audit report. `3A` keeps all family blocks active simultaneously and spaces them progressively farther left rather than requiring family cycling. | All twenty imported trees and twenty generated comparison roots are continuously available for side-by-side inspection without consuming the playable Ground. | Complete and live-validated on 2026-07-24: 40 specimens/slots, 118,610 imported triangles, four active blocks, positive chunk clearance, Weather wind ready, cloud cookie ready. |
| TREE-GALLERY.4 | Run source-scale, wind, cloud, material, pivot, rebuild/removal, and Play Mode validation; record accepted report metrics and freeze the gallery baseline. | Imported reference baseline is accepted. Generator implementation is unblocked. | Complete through the accepted TREE-GALLERY.2B and TREE-GALLERY.3A live reports plus TREE-GALLERY.FREEZE. |
| TREE-GALLERY.FREEZE | Freeze the accepted simultaneous twenty-tree reference gallery, measured family ranges, accepted tree shaders/material defaults, Weather/cloud receiver contracts, and known source-card limitations. | Imported reference baseline is no longer an implementation target and remains available for generated comparisons. | Complete in this documentation patch after live TREE-GALLERY.3A PASS. |
| TREE-GEN.1 / 1B / 1D / 1F | Implement the deterministic family/recipe library, unified gallery workflow, branch graph, transported frames, reference-bound structural calibration, curve constraints, collapsed-branch rejection, previews, and dependency diagnostics. | All twenty slots generate deterministic constrained structures through one gallery action. | Complete and live-validated: 20/20 structures, repeatability 20/20, four family dependency suites PASS. |
| TREE-GEN.2A | Build the first Common/Pine/Twisted/Dead combined bark meshes from transported frames and reuse the accepted tree bark renderer. | Four visible generated bark representatives expose topology, junction, shading, and authoring gaps before all-slot expansion. | Live vertical slice complete but not accepted: exterior winding/culling, base closure, branch-root intersection artifacts, colour-baseline mismatch, and control gaps remain. |
| TREE-PLAN.3 | Freeze the family-template/ranged-recipe model, baseline colour policy, full control schema, observed TREE-GEN.2A defects, and exact TREE-GEN.2B acceptance gate. | No architectural or visual finding from TREE-GEN.2A is lost before the next thread. | Complete in this documentation patch. |
| TREE-GEN.2B | Correct bark mesh winding/closure/junctions and add the high-value branch arch, trunk spiral, branch-start-height, symmetry/directional-bias, and baseline colour controls. Keep the four-family vertical slice until accepted. | Four generated representatives render as outward-facing bark meshes with classified seams/embedded roots, acceptable branch junctions, matched imported-family colour baselines, deterministic geometry fingerprints, and independently proven controls. | Complete and live-validated on 2026-07-24: 20/20 structures and repeatability checks, 4/4 bark meshes, all topology gates, and all four family dependency suites PASS. Colour and exterior rendering accepted. |
| TREE-GEN.2C | Add a compact non-circular trunk cross-section, visible axial twist, and root-buttress grammar while reusing existing twist/path controls. Keep the four-family vertical slice until accepted. | Common, Pine, Twisted, and Dead preserve corrected topology while generated bases become lobed/buttressed and Twisted/Dead show actual helical surface structure. | Source implementation and consistency/compliance audit complete; Unity compilation, migration, topology, visual comparison, and Play Mode validation pending. |
| TREE-GEN.3 | Implement Common-family grammar and procedural foliage clusters/cards using retained textures and exact pivot metadata. | One generated Common tree renders beside `CommonTree_1` with accepted crown identity and wind. | Planned. |
| TREE-GEN.4 | Implement Pine-family tier/leader grammar, branch-tip foliage, and Pine response profile. | One generated Pine renders beside `Pine_5` with accepted conifer identity. | Planned. |
| TREE-GEN.5 | Implement Twisted-family curvature, torsion, asymmetry, permanent wind bias, sparse foliage, and damage controls; generalize branch break/removal state so Dead is not architecturally isolated. | One generated Twisted tree renders beside `TwistedTree_1`; damage controls remain reusable. | Planned. |
| TREE-GEN.6 | Complete generated bark/foliage wind response, conservative deformed bounds, cloud receiver audit compliance, and matched imported/generated environmental comparison. | All four generated families respond coherently to Weather and cloud shading. | Planned. |
| TREE-GEN.7 | Implement structural LOD0/1/2 generation, reduced-card foliage, shadow proxies, collision/footprint metadata, and per-output validators. | Each accepted specimen has complete bounded production outputs. | Planned. |
| TREE-LIB.1 | Approve and create tracked production tree texture/license location; copy retained textures with new GUIDs; add library asset, variant record, baker, cleanup, fingerprints, and validation. | Baked output no longer depends on ignored source textures or FBXs. | Planned. |
| TREE-LIB.2 | Generate candidates, compare them in the gallery, and accept at least eight variants per family; bake only accepted variants and record metrics. | Minimum thirty-two-variant generated V1 library is complete. | Planned. |
| TREE-RENDER.1 | Implement structured tree instances, multi-variant/LOD bark and foliage batches, indirect rendering, conservative bounds, and resource diagnostics. | Baked library variants render without GameObject-per-tree cost. | Planned. |
| TREE-PLACE.1 | Implement `GroundTrees`, tree layers, authored coverage, ecological recipes, Ground sampling, deterministic minimum-distance placement, stable IDs, and placement diagnostics. | Authored forest areas produce repeatable family compositions without overlap. | Planned. |
| TREE-PLACE.2 | Add chunk activation, CPU chunk/frustum culling, LOD assignment, and placement/render cache invalidation. | Large-map tree populations remain bounded to active/visible chunk work. | Planned. |
| TREE-PERF.1 | Run matched low-end-target 1440p CPU/GPU profiling for gallery references, generated library, LODs, foliage, shadows, and realistic active chunks; adjust only from measured evidence. | Production budgets and quality defaults are frozen. | Planned. |
| TREE-FUTURE.* | Add approved snow, wetness, seasons, damage, chopping, roots, navigation, or landmark workflows as separate plans. | Future systems consume the stable tree contracts. | Deferred. |

### Patch-level validation gates

Every implementation patch must complete the repository's four mandatory gates and update this document before a material deviation.

Tree-specific checks include:

1. compile all changed C#, shaders, and includes with no unresolved references or shader variants;
2. run the patch's complete clipboard-copyable diagnostic and record the result in this document;
3. validate deterministic output through repeated rebuild hashes where generation is involved;
4. inspect imported/generated pairs from the gameplay camera under open sun and the cloud debug overlay;
5. enter and exit Play Mode and confirm no duplicate gallery children, implicit regeneration, persistent buffers, or leaked materials;
6. compare final scope and behaviour with the accepted gallery baseline, Weather contracts, cloud receiver contract, grass invariants, budgets, and active repository state.

### Current non-blocking defaults and approval checkpoints


#### Locked next-patch Inspector cleanup

`TREE-GEN.1` also simplifies the gallery Inspector so it presents current workflows instead of historical patch stages.

Primary Actions:

```text
Rebuild Complete Reference Gallery
Remove Complete Reference Gallery
```

`Rebuild Complete Reference Gallery` creates the gallery when absent, rebuilds it when present, repairs required audited source imports, refreshes shared materials, and rebuilds all twenty imported specimens plus twenty comparison slots.

Collapsed Advanced Validation:

```text
Rebuild On-Map Four-Family Validation Slice
Remove On-Map Validation Slice
```

Collapsed Maintenance:

```text
Repair Required Tree Source Imports
```

The maintenance action is enabled only when the source/import audit reports a defect. Separate Build/Rebuild buttons and the permanently visible manual normal-correction action are removed from the ordinary workflow.

#### Locked `TREE-GEN.1` implementation boundary

`TREE-GEN.1` creates deterministic structural data and diagnostics only. It may draw curve/branch previews through Editor gizmos or handles, but it does not create final bark meshes, foliage cards, production LODs, runtime renderer buffers, forest placement, or generated variant assets.

The patch must prove the authoring contract before `TREE-GEN.2` begins mesh construction.

No blocking design question remains for live-validating `TREE-GEN.1`; `TREE-GEN.2` remains blocked until the structural and determinism reports pass in Unity.

The current proposed defaults are:

- create the gallery as an independent sibling of the selected `GeneratedGround` when they share a scene parent, or as a separate root object when the Ground is root-level;
- store an explicit Ground reference because the gallery no longer inherits Ground ownership from hierarchy;
- offer a user-invoked closest-Ground assignment action, but never silently bind or rebind the gallery from `OnValidate`, `Update`, or parent changes;
- keep the twenty FBXs in the ignored reference vault and build gallery specimens through Editor tooling;
- use coloured foliage variants for source-reference appearance;
- reserve white foliage variants for generated tint/season tests;
- use shared tree bark/foliage shaders from the gallery onward;
- enable Weather wind and cloud-cookie shading in the gallery foundation;
- default imported foliage shadow casting off until its explicit test;
- use overlapping embedded branch tubes for V1 junctions rather than delaying the generator for watertight boolean forks;
- generate and bake ordinary variants in the Editor rather than during gameplay;
- target at least eight accepted generated variants per family for V1.

Any requested change to those defaults should be recorded here before the affected implementation patch begins.

## Post-change consistency and compliance audit

### Actual affected scope

The actual scope matches the approved plan:

- created this handoff and its metadata;
- created the pack-level integration review and its metadata;
- created the sibling `Trees` folder and folder metadata;
- moved twenty Unity-labelled FBX assets and their twenty existing metadata files;
- moved twelve tree textures and their twelve existing metadata files;
- copied the original license text into the dedicated folder and assigned new metadata;
- did not modify runtime C#, editor code, shaders, compute shaders, scenes, prefabs, materials, profiles, packages, ProjectSettings, layers, tags, or unrelated documentation.

No generic FBX, OBJ, MTL, glTF, BIN, preview, grass, plant, mushroom, rock, or unrelated texture file was moved or modified.

### Intentional differences

- The approved tree assets now resolve from the flat `Assets/References/Trees/` paths listed above instead of `Assets/References/Stylized Nature/FBX (Unity)/` and `Assets/References/Stylized Nature/Textures/`.
- The selected asset contents and importer metadata are unchanged. Only filesystem paths changed.
- The dedicated license is a text-identical copy with a new GUID; duplicating the original license metadata would have created an invalid duplicate GUID.
- The pack now includes the requested Markdown summary.
- The tracked Docs folder now includes this canonical plan and continuation handoff.

### Preserved behavior and contracts

- All tree/texture asset GUIDs and importer settings are preserved.
- No existing project asset referenced any selected GUID before the move.
- Existing vegetation renderer, mesh, placement, instance, coverage, interaction, trample, benchmark, shader, and Weather contracts are unchanged.
- The original pack retains twenty generic tree FBX files, forty tree OBJ/MTL files, and forty tree glTF/BIN files.
- The original license remains in the pack.
- Current bark normal maps remain classified as ordinary sRGB textures; this known import issue is deliberately deferred to the tree-integration thread.
- Current FBX assets remain non-readable, use file units at scale one, import vertex colours, calculate tangents, and generate no LODs.

### Validation evidence

- The relocation processed exactly thirty-two assets and sixty-four asset/metadata files.
- The in-process move check compared every destination asset SHA-256, metadata SHA-256, and metadata GUID with the value captured before its move. It reported zero hash or GUID mismatches.
- The initial in-process source/destination presence expressions omitted required PowerShell grouping parentheses and emitted non-terminating `Test-Path` parameter-binding diagnostics. They did not affect the completed moves or the independent hash/GUID comparisons. A corrected standalone presence/inventory validator was run immediately afterward.
- The corrected validator reported:
  - `TARGET_ASSETS=33`: twenty FBX files, twelve PNG files, and one license;
  - `TARGET_METAS=33`;
  - `SOURCE_SELECTED_FILES_REMAIN=0`;
  - `RETAINED_GENERIC_FBX=20`;
  - `RETAINED_OBJ_AND_MTL=40`;
  - `RETAINED_GLTF_AND_BIN=40`;
  - `PROJECT_GUID_RECORDS=446`;
  - `DUPLICATE_GUID_GROUPS=0`;
  - `VALIDATION_FAILURES=0`.
- The copied license text matches the original after line-ending normalization.
- Both requested Markdown documents and their unique metadata files exist.
- The final documentation validator reported one H1 per document, twelve balanced fences in this handoff, two balanced fences in the pack review, LF-only content, every final tree path present in this handoff, `git diff --check` success, and `FINAL_VALIDATION_FAILURES=0`.
- `Assets/References/` remains ignored by the existing `.gitignore`; the dedicated tree folder must be included explicitly when the project files are handed to the next thread.

### Historical and final comparison

Relative to `HEAD` and the accepted vegetation/Weather history, no runtime behavior changed. Relative to the pre-edit working tree, the only tracked additions are this handoff and its metadata. All other tracked working-tree changes were pre-existing and remain untouched. The tree relocation and pack review are local ignored-reference changes and therefore require filesystem-level validation rather than Git diff validation.

### Performance and Unity validation

This relocation adds no active-gameplay CPU/GPU work, memory allocation, buffer, texture sampling, draw, dispatch, or update path. It adds local asset storage and Markdown only.

Unity 6000.5.0f1 import validation is pending. The next concrete action is to open the transferred project, allow Unity to import `Assets/References/Trees/`, confirm there are no import errors or missing metadata, then inspect one representative FBX from each family before changing importer settings or implementing runtime integration.

## TREE-PLAN.1 documentation-update record

### Scope

This planning patch modifies only `Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md` in the delivered artifact. It does not modify C#, shaders, compute shaders, scenes, prefabs, materials, profiles, import settings, packages, ProjectSettings, layers, tags, or the retained source assets.

### Material changes

- changed the document status from relocation-only completion to implementation-planning completion;
- added implementation-roadmap acceptance criteria;
- locked the imported-gallery-first sequence;
- defined explicit Weather wind and cloud-cookie receiver contracts;
- defined the imported gallery hierarchy, audit, materials, shaders, layout, Ground alignment, labels, actions, and acceptance gate;
- defined the procedural branch graph, family profiles, deterministic seed streams, mesh generation, normals/tangents/UVs, metadata channels, foliage generation, family grammars, LODs, proxies, library baking, runtime renderer, and placement architecture;
- added provisional generation budgets and a minimum thirty-two-variant V1 library target;
- added exact patch IDs, dependencies, results, statuses, and validation gates;
- recorded non-blocking defaults and approval checkpoints.

### Consistency result

The plan keeps Weather as the only wind producer, the Weather cloud system/main directional-light cookie as the only cloud producer/path, and the accepted grass renderer unchanged. The imported gallery is explicitly diagnostic and does not establish the production renderer. The generated library is editor-baked and does not introduce ordinary gameplay mesh regeneration.

### Pending validation

Unity validation is not applicable to this documentation-only artifact. Every Unity, shader, importer, scene, material, deterministic-generation, visual, and performance check remains attached to its corresponding planned implementation patch and must not be reported as passed before that patch is executed.



## TREE-GALLERY.1 implementation record

### Objective

Implement the read-only source audit and standalone gallery authoring foundation. The patch must establish authoritative Unity evidence for the twenty imported FBXs and twelve retained textures without creating final tree materials, shaders, rendered specimens, procedural meshes, LODs, or production placement.

### Approved files

```text
Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md
Assets/Game/Procedural/Trees.meta
Assets/Game/Procedural/Trees/TreeFamily.cs
Assets/Game/Procedural/Trees/TreeFamily.cs.meta
Assets/Game/Procedural/Trees/TreeReferenceGallery.cs
Assets/Game/Procedural/Trees/TreeReferenceGallery.cs.meta
Assets/Game/Procedural/Trees/TreeReferenceSpecimen.cs
Assets/Game/Procedural/Trees/TreeReferenceSpecimen.cs.meta
Assets/Game/Procedural/Trees/Editor.meta
Assets/Game/Procedural/Trees/Editor/TreeReferenceGalleryEditor.cs
Assets/Game/Procedural/Trees/Editor/TreeReferenceGalleryEditor.cs.meta
Assets/Game/Procedural/Trees/Editor/TreeReferenceGalleryBuilder.cs
Assets/Game/Procedural/Trees/Editor/TreeReferenceGalleryBuilder.cs.meta
Assets/Game/Procedural/Trees/Editor/TreeSourceAssetAudit.cs
Assets/Game/Procedural/Trees/Editor/TreeSourceAssetAudit.cs.meta
```

No scene, prefab, material, shader, compute shader, imported FBX, texture importer, package, ProjectSettings, layer, tag, Weather producer, cloud producer, Ground implementation, grass implementation, or generated asset is authorized.

### Reviewed evidence

- `Assets/AGENTS.md` requires a plan-first edit, strict approved scope, no raw serialized scene changes, clipboard diagnostics, final caller/producer review, and explicit pending Unity validation.
- `Assets/Game/Procedural/Ground/GeneratedGround.cs::TrySampleBaseSurface` provides the later gallery's Ground sampling contract without requiring hierarchy ownership.
- `Assets/Game/Procedural/Vegetation/GroundVegetation.cs::SynchronizeSurfaceGroundFromHierarchy` demonstrates the existing child-owned vegetation pattern that the user explicitly rejected for the independent gallery; that pattern will not be copied.
- `Assets/Game/Procedural/Weather/Editor/WeatherCloudShadowReceiverAudit.cs` and existing custom inspectors establish the project's StringBuilder report, `FindObjectsByType`, and clipboard-copy conventions.
- `Assets/References/Trees/` is absent from the supplied code archive. The audit therefore must compile without the local vault and report the missing source non-destructively when run in that archive; full asset validation requires the user's complete Unity project.
- The supplied archive contains no `.git` directory. Git status, diff-versus-HEAD, and history inspection are unavailable in this environment and remain pending in the live repository.
- Unity 6 documentation confirms editor access to non-readable imported mesh data and the read-only MeshData colour API. The implementation will not change `ModelImporter.isReadable`.

### Ownership correction

`TreeReferenceGallery` is a standalone diagnostic object. It is not a Ground child and does not represent a Ground-owned subsystem. It stores one explicit `GeneratedGround` reference for later height sampling. The Editor creation action places it immediately after the selected Ground as a sibling when possible, otherwise at the scene root. No automatic ancestor lookup, hidden rebinding, or per-frame proximity search is permitted.

### Invariants and non-goals

- The audit is read-only.
- Missing source assets produce an explicit failed report, not exceptions or partial gallery construction.
- The current grass, Weather wind, cloud, Ground, River, and rendering implementations remain unchanged.
- No source FBX or texture import setting is changed.
- No final gallery child hierarchy is generated.
- No expensive work occurs in `Update` or `OnValidate`.
- No per-tree GameObject production architecture is established.

### File-by-file sequence

| Item | File | Work | Status |
| --- | --- | --- | --- |
| TG1-PLAN | Tree handoff | Record standalone ownership, scope, evidence, invariants, sequence, and validation before code. | Complete. |
| TG1-A | `TreeFamily.cs` | Define stable family and reference-role vocabulary. | Complete at source level. |
| TG1-B | `TreeReferenceGallery.cs` | Add explicit Ground reference, audit state, source constants, status accessors, and no automatic generation. | Complete at source level. |
| TG1-C | `TreeReferenceSpecimen.cs` | Add passive imported/procedural specimen metadata for later gallery children. | Complete at source level. |
| TG1-D | `TreeReferenceGalleryBuilder.cs` | Add undo-aware standalone sibling/root creation, explicit closest-Ground assignment, and placement foundation only. | Complete at source level. |
| TG1-E | `TreeSourceAssetAudit.cs` | Inspect all required FBXs/textures and build a complete deterministic report without mutation. | Complete at source level; live asset execution pending. |
| TG1-F | `TreeReferenceGalleryEditor.cs` | Expose explicit Ground assignment, source status, audit/copy actions, and disabled future build actions. | Complete at source level. |
| TG1-VERIFY | All approved files | Run source/static checks, compare scope, reread related contracts, and record pending Unity evidence. | Source/static verification complete; Unity and live Git verification pending. |

### Acceptance criteria

- The new C# files are syntactically complete and use the existing project namespaces and Unity 6 APIs.
- The standalone gallery can be created beside a selected Ground without becoming its child.
- The gallery exposes an explicit Ground reference and never searches every frame.
- One action runs the complete audit and stores the last report, pass state, timestamp, and revision.
- One action copies the stored report to the clipboard.
- All twenty expected FBX paths and twelve expected texture paths are represented exactly once.
- Missing folders/assets, unexpected render/material layouts, and texture contract failures are reported clearly.
- Future build actions remain disabled until a successful source audit exists.
- No file outside the approved scope changes.

### Validation plan

1. Compile/import the changed C# in Unity 6000.5.0f1.
2. Create the gallery from a selected Ground and confirm it is a sibling/root object with that Ground explicitly assigned.
3. Run and copy the complete source audit; provide the complete report once.
4. Confirm repeated audits do not create objects, materials, meshes, buffers, or asset/import changes.
5. Enter and exit Play Mode and confirm no automatic audit, child generation, duplicate object, or persistent resource creation occurs.
6. Compare the live repository diff and status with the approved file list and record Unity/Git results here.


### TREE-GALLERY.1 post-implementation consistency and compliance result

#### Actual affected scope

The final source patch changes exactly fifteen approved files: the canonical handoff; two new folder metadata files; six new C# files; and their six new metadata files. No baseline file was removed. A fresh extraction of `Assets-Code-Archive(19).zip` was compared against the final work tree; no Ground, vegetation, Weather, cloud, River, rendering, scene, prefab, material, shader, compute, package, layer, tag, ProjectSettings, imported source asset, or unrelated documentation file differs.

#### Material implementation differences

- `TreeReferenceGallery` is independent of `GeneratedGround` hierarchy ownership and stores one explicit Ground reference.
- `GameObject > PS3D > Trees > Tree Reference Gallery (Standalone)` creates the gallery as the selected Ground's sibling when possible, or as a scene root when no unambiguous Ground is resolved.
- `Assign Closest Ground` is user-invoked and scene-local; there is no `Update`, `LateUpdate`, parent-change callback, or automatic proximity rebinding.
- `Place as Ground Sibling` explicitly reparents and aligns an existing gallery without creating gallery children.
- The audit contains the four fixed five-variant model families and all twelve retained textures, validates explicit bark/foliage material identities, records importer and geometry state, computes combined bounds and vertex-colour statistics, and reports the expected bark-normal import correction without mutating importers.
- The Inspector stores the last complete report and copies both new and stored reports to the clipboard. Final gallery build/rebuild/remove actions remain visibly disabled because `TREE-GALLERY.3` is not implemented.

#### Preserved contracts

- `GeneratedGround.TrySampleBaseSurface` is not called or changed in this patch; its future use remains explicit through the gallery's assigned Ground.
- `GroundVegetation` and `VegetationLayer` retain their accepted child-owned grass hierarchy. The independent gallery does not reuse that ownership rule.
- Weather wind production, `WeatherWindField.hlsl`, the cloud controller/cookie path, the cloud receiver audit, existing shaders, and all current consumers are byte-identical to the supplied archive.
- No source model or texture importer is changed. `ModelImporter.isReadable` remains untouched; the editor-only audit uses read-only mesh access.
- No runtime mesh, material, buffer, draw, compute dispatch, collider, LOD, procedural generator, or production placement work is introduced.

#### Static validation evidence

A final filesystem/source validator reported:

```text
CHANGED_FILES=15
REMOVED_FILES=0
STATIC_ERRORS=0
```

The checks covered exact approved scope; LF-only content; no trailing whitespace; unique valid GUIDs for all new metadata; balanced C# braces, parentheses, brackets, strings, chars, and comments; exact representation of four model prefixes and twelve texture filenames; absence of `Update`, `LateUpdate`, `SaveAndReimport`, asset creation/deletion, `DestroyImmediate`, obsolete instance-ID access, and legacy object search; removal of superseded parent-owned gallery wording; and preservation of every unapproved baseline file.

#### Subsequent live Unity validation

The user applied `TREE-GALLERY.1` to Unity 6000.5.0f1. After one compile-only correction to the report indentation helper, the project compiled and the complete source audit passed on 2026-07-23 with `20 / 20` FBXs, `12 / 12` textures, zero failures, and only the three expected bark-normal import warnings. The audit confirmed the exact one-renderer material layouts, source bounds, required vertex attributes, texture dimensions/alpha, and source-vault availability. This closes the `TREE-GALLERY.1` Unity/source-contract gate and unblocks `TREE-GALLERY.2`.

The supplied archive still has no `.git` directory, so repository status/history validation is not represented here. The live project remains authoritative for final diff and compile validation.


## TREE-GALLERY.2 active implementation plan — four-family rendering vertical slice

### Objective

Establish the shared imported/generated tree rendering contract and build one imported specimen for each family beside an empty procedural comparison slot. The patch must correct only the three audited bark-normal import settings, consume the existing Weather wind and cloud-cookie producers without changing them, and remain an Editor-authored diagnostic path rather than a production forest renderer.

### Live source-audit evidence

The complete Unity 6000.5.0f1 audit generated on 2026-07-23 reported `PASS`, `20 / 20` models, `12 / 12` textures, zero failures, and three expected warnings.

The audit proves:

- Common variants use one `MeshRenderer`, one mesh, two submeshes, and materials `[Bark_NormalTree, Leaves_NormalTree]`;
- Pine variants use one `MeshRenderer`, one mesh, two submeshes, and materials `[Bark_NormalTree, Leaves_Pine]`;
- Twisted variants use one `MeshRenderer`, one mesh, two submeshes, and materials `[Bark_TwistedTree, Leaves_TwistedTree]`;
- Dead variants use one `MeshRenderer`, one mesh, one submesh, and material `[Bark_DeadTree]`;
- every model contains vertex colours, UV0, normals, and tangents, and contains no UV1/UV2/UV3;
- all source FBXs remain non-readable and preserve `globalScale=1`, file units, imported normals, and calculated Mikk tangents;
- the source root transforms are identity;
- the representative bounds and lowest visible local Y values required for the vertical slice are known: Common 1 `-0.24277`, Pine 5 `-0.23509`, Twisted 1 `-0.20148`, and Dead 1 `-0.33555` metres;
- `Bark_DeadTree_Normal.png`, `Bark_NormalTree_Normal.png`, and `Bark_TwistedTree_Normal.png` are 2048² default sRGB textures and require Normal Map classification;
- coloured foliage textures contain alpha and remain the imported-reference inputs;
- the previous bark/foliage vertex-colour statistics are whole-mesh statistics because living-family bark and foliage share one mesh. The initial accepted imported wind default is therefore `BoundsHeightFallback`; `VertexColourRed` remains an explicit diagnostic mode rather than an automatically selected mask.

The supplied code archive has no `.git` directory and no local reference-vault assets. Live repository status/history comparison and Unity execution remain unavailable in this patch-construction environment; the user's audit is the authoritative source-contract evidence.

### Approved affected scope

Modify:

```text
Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md
Assets/Game/Procedural/Trees/TreeFamily.cs
Assets/Game/Procedural/Trees/TreeReferenceGallery.cs
Assets/Game/Procedural/Trees/TreeReferenceSpecimen.cs
Assets/Game/Procedural/Trees/Editor/TreeReferenceGalleryBuilder.cs
Assets/Game/Procedural/Trees/Editor/TreeReferenceGalleryEditor.cs
Assets/Game/Procedural/Weather/Editor/WeatherCloudShadowReceiverAudit.cs
```

Create:

```text
Assets/Game/Rendering/Trees.meta
Assets/Game/Rendering/Trees/Includes.meta
Assets/Game/Rendering/Trees/Includes/TreeCommon.hlsl
Assets/Game/Rendering/Trees/Includes/TreeCommon.hlsl.meta
Assets/Game/Rendering/Trees/Includes/TreeWindResponse.hlsl
Assets/Game/Rendering/Trees/Includes/TreeWindResponse.hlsl.meta
Assets/Game/Rendering/Trees/Includes/TreeLighting.hlsl
Assets/Game/Rendering/Trees/Includes/TreeLighting.hlsl.meta
Assets/Game/Rendering/Trees/Shaders.meta
Assets/Game/Rendering/Trees/Shaders/SH_StylizedTreeBark.shader
Assets/Game/Rendering/Trees/Shaders/SH_StylizedTreeBark.shader.meta
Assets/Game/Rendering/Trees/Shaders/SH_StylizedTreeFoliage.shader
Assets/Game/Rendering/Trees/Shaders/SH_StylizedTreeFoliage.shader.meta
```

The explicit Editor build action may create or update six shared gallery materials under:

```text
Assets/Game/Demo/Materials/Trees/
```

Those material assets are created through Unity `AssetDatabase`, not raw YAML editing. They may reference the ignored source-vault textures because the imported gallery is explicitly a local diagnostic system. Production generated-library textures remain deferred to `TREE-LIB.1`.

The explicit build/import-normalization action may modify only the three retained bark-normal `.meta` files through `TextureImporter`. No FBX importer or other texture importer is authorized for mutation.

No scene, prefab, package, ProjectSettings, layer, tag, Ground implementation, grass implementation, Weather producer, cloud producer, wind field include, River implementation, or source FBX is authorized for modification.

### Architectural decisions

- The gallery remains an independent sibling/root object with one explicit `GeneratedGround` reference.
- The four-family slice uses `CommonTree_1`, `Pine_5`, `TwistedTree_1`, and `DeadTree_1`.
- Family rows are packed inside the assigned Ground's actual local surface domain. The configured row spacing is treated as a preference and is compressed only as needed while preserving audited canopy clearance. Imported/procedural roots remain separated along Ground-local X, but the pair centre may shift laterally when an asymmetric imported crown would otherwise leave the Ground domain.
- `pairColumnSpacing` is the minimum imported/procedural centre separation. `comparisonPairOffset` is additional canopy-clearance padding; the actual separation is the larger of the configured minimum and audited canopy width plus this padding.
- Imported and procedural roots are sampled independently through the explicit Ground at their actual Ground-local positions. Each receives its own sampled height and normal; the builder never assumes that both sides of a comparison pair share one elevation.
- The imported visual is lifted by `-lowestVisibleLocalY * sourceScale`; the shader receives the source-root contact in object space as `(0, lowestVisibleLocalY, 0)` and transforms it to world space in every vertex invocation. This keeps wind anchored at the Ground contact even if the standalone gallery is moved, without any per-frame C# update.
- Initial imported macro deformation samples `SampleWeatherWindResponse` once at the root and applies a coherent root-anchored bend. No tree-owned wind texture, CPU simulation, or per-frame component update is introduced.
- Foliage uses restrained height-weighted fallback flutter because source card pivots are unavailable. Generated foliage will replace this fallback with exact generated attachment metadata.
- Shared tree materials are asset-backed and reused across specimens. No per-tree material instances are created.
- Foliage shadow casting is controlled by enabling/disabling the foliage material's `ShadowCaster` pass; bark shadow casting remains enabled on the shared renderer.
- Bark and foliage shaders use the three-argument URP `GetMainLight`/`UniversalFragmentPBR` path with `_LIGHT_COOKIES`, so the authoritative directional cookie is applied once. No custom cloud texture or duplicate cloud attenuation is added.
- The Weather cloud receiver audit's mandatory shader list is extended with both tree shaders. No assessment logic or producer behavior changes.

### Provisional diagnostic defaults

These are validation starting points, not frozen production tuning:

- imported wind mask: `BoundsHeightFallback`;
- debug mode: final rendering;
- foliage alpha cutoff: `0.50`;
- Common response: stiffness `0.35`, macro `0.65`, flutter `0.040`;
- Pine response: stiffness `0.65`, macro `0.45`, flutter `0.025`;
- Twisted response: stiffness `0.45`, macro `0.55`, flutter `0.030`;
- Dead response: stiffness `0.85`, macro `0.15`, flutter `0.000`;
- foliage shadow casting: off.

All values are exposed through material or gallery data and must be validated visually before they become production defaults.

### File-by-file implementation sequence

| Item | File | Work | Status |
| --- | --- | --- | --- |
| TG2-PLAN | Tree handoff | Record live audit evidence, exact scope, architecture, provisional defaults, risks, and validation before implementation. | Complete. |
| TG2-A | `TreeFamily.cs` | Add explicit imported wind-mask and tree debug-mode enums shared by the gallery and shaders. | Complete at source level. |
| TG2-B | `TreeReferenceGallery.cs` | Add mask/debug/cutoff settings plus vertical-slice revision, pass state, timestamp, report, and accessors. | Complete at source level. |
| TG2-C | `TreeReferenceSpecimen.cs` | Record Ground correction, object-space comparison-root contact, assigned material/shader summary, and source metrics for labels/audits. | Complete at source level. |
| TG2-D | `TreeCommon.hlsl` | Add shared hash, axis rotation, mask resolution, and debug-colour helpers. | Complete at source level. |
| TG2-E | `TreeWindResponse.hlsl` | Consume `WeatherWindField.hlsl`, apply root-sampled coherent bend and restrained imported foliage fallback flutter, and return mask/debug data. | Complete at source level. |
| TG2-F | `TreeLighting.hlsl` | Build URP `InputData`, apply normal orientation, and provide cookie-aware PBR tree lighting helpers. | Complete at source level. |
| TG2-G | Bark shader | Add opaque albedo/normal rendering, wind deformation in forward/shadow/depth passes, debug modes, fog, main-light shadows, local lights, and `_LIGHT_COOKIES`. | Complete at source level. |
| TG2-H | Foliage shader | Add two-sided alpha-clipped rendering, restrained flutter, front/back normal policy, wind deformation in forward/shadow/depth passes, debug modes, fog, and `_LIGHT_COOKIES`. | Complete at source level. |
| TG2-I | Gallery builder | Add explicit normal-import correction, shared material creation/update, four-family build/rebuild/remove, adaptive Ground-domain layout, independent root sampling, source-layout validation, property blocks, and clipboard report. | Complete at source level; `TREE-GALLERY.2A` correction applied after the first live build exposed an out-of-domain fixed-spacing assumption. |
| TG2-J | Gallery editor | Expose build/rebuild/remove/import-normalization/copy actions, rendering settings, status, and Scene-view specimen labels. | Complete at source level. |
| TG2-K | Cloud receiver audit | Add both tree shader names to mandatory authored receivers without changing assessment logic. | Complete at source level. |
| TG2-VERIFY | All approved files | Complete static scope/source checks and record unavailable Unity/Git validation and exact next actions. | Static validation complete; Unity/Git validation pending. |

### Risks and controls

| Risk | Control |
| --- | --- |
| Imported source root is lifted above Ground, causing wind to pivot above the trunk base or become stale after gallery movement. | Pass the source-root contact in object space through a material property block and transform it in the shader. |
| Foliage shadow toggle disables bark because both submeshes share one renderer. | Keep renderer shadow casting enabled and toggle only the foliage material `ShadowCaster` pass. |
| Shared material assets create duplicates on rebuild. | Use fixed approved asset paths and update existing materials in place. |
| Import normalization changes unrelated settings. | Change only `textureType` and `sRGBTexture` on the three audited bark-normal importers. |
| Source material ordering differs from the audit. | Revalidate material names before replacement and fail the build rather than guessing. |
| Foliage cards tear under pivot-free flutter. | Use low-amplitude fallback motion and make the fallback phase visible through a debug mode. |
| Tree shaders receive cloud attenuation twice. | Use only URP cookie-aware lighting and extend the mandatory receiver audit; do not include custom cloud sampling. |
| Tree wind changes the Weather producer. | Include the existing read-only Weather field contract only; no Weather source file or global is modified. |
| Gallery build partially succeeds. | Validate all assets/shaders/material mappings before hierarchy creation and delete the builder-owned root on build failure. |
| Generated material folder is absent. | Create the exact tracked path through `AssetDatabase.CreateFolder` from the explicit action. |

### Acceptance criteria

- The three bark normal importers are normalized through an explicit action and no other importer changes.
- Six shared material assets exist at deterministic paths and use the correct source textures and tree shaders.
- One Common, Pine, Twisted, and Dead reference renders under a builder-owned vertical-slice root.
- Every family has one empty procedural comparison slot whose root is sampled independently from the imported reference on the assigned Ground.
- Imported visuals use audited source scale and lowest-visible-Y correction.
- Bark normal mapping, foliage alpha clipping, and two-sided foliage lighting compile and render.
- Weather wind affects intended vertices while the comparison root remains fixed.
- Both tree shaders are mandatory supported cloud receivers.
- Build, rebuild, and remove affect only the builder-owned vertical-slice root and create no duplicate materials.
- A complete build report is stored and copyable.
- Play Mode does not rebuild, duplicate, or allocate gallery resources through per-frame component logic.

### Validation plan

1. Compile all changed C# and both tree shaders in Unity 6000.5.0f1.
2. Run `Hierarchy > Tree Reference Gallery > Inspector > Actions > Build Four-Family Vertical Slice`, then copy the complete build report.
3. Inspect bark normals, foliage clipping/two-sided lighting, root placement, and family scale from the gameplay camera.
4. Validate Weather motion with roots fixed and validate the same cloud boundary across Ground, bark, and foliage using the existing cloud diagnostic overlay.
5. Run the Weather cloud-shadow receiver audit and require both tree shaders to report `SUPPORTED`.
6. Rebuild, remove, enter/exit Play Mode, and confirm no duplicate children, material duplicates, source-FBX changes, or persistent allocations.

### TREE-GALLERY.2 post-implementation consistency and compliance result

#### Actual affected scope

The source patch changes exactly twenty approved files relative to the accepted `TREE-GALLERY.1` baseline: this canonical handoff; five tree runtime/editor files; the existing cloud-receiver audit; two new rendering folders and their metadata; three new shared tree includes and metadata; and two new tree shaders and metadata. No baseline file is removed.

The patch does not ship generated material YAML. The explicit gallery build action creates or updates the six approved shared material assets at deterministic paths under `Assets/Game/Demo/Materials/Trees/` through Unity `AssetDatabase`. The action may normalize only the three audited bark-normal importers by changing `textureType` to `NormalMap` and disabling sRGB. No FBX importer or unrelated texture importer is touched.

#### Material implementation differences

- The shared bark shader uses retained bark albedo/normal textures, imported tangents, opaque PBR lighting, root-sampled Weather deformation, normal-map shading, forward/shadow/depth deformation parity, fog, and `_LIGHT_COOKIES`.
- The shared foliage shader uses the coloured source foliage variants, two-sided alpha-clipped PBR lighting, explicit front/back normal orientation, restrained fallback flutter, forward/shadow/depth alpha/deformation parity, fog, and `_LIGHT_COOKIES`.
- Tree deformation consumes `SampleWeatherWindResponse` at the source-root contact. The root is stored in source object space and transformed in shader, so moving the standalone gallery does not stale the pivot and no per-frame C# update is required.
- Bounds-height weighting is the imported default because the source audit did not isolate vertex colours per submesh. Vertex-colour red remains an explicit debug/diagnostic option.
- Four source specimens (`CommonTree_1`, `Pine_5`, `TwistedTree_1`, and `DeadTree_1`) are paired with empty procedural comparison roots under one builder-owned vertical-slice hierarchy.
- The builder revalidates accepted source material names before assigning deterministic shared materials and fails rather than guessing when source contracts differ.
- Foliage shadow casting is controlled through the foliage material's `ShadowCaster` pass while the shared renderer remains shadow-capable for bark.
- Both tree shaders are added to the existing Weather cloud-shadow receiver audit's mandatory list. Assessment logic and both Weather producers remain unchanged.

#### Preserved contracts

- The gallery remains a separate sibling/root object with an explicit Ground reference; it is rejected if parented under the assigned Ground.
- Existing grass mesh, renderer, placement, coverage, interaction, trample, and shader contracts are byte-preserved.
- `WeatherWindDomain`, `WeatherWindField.hlsl`, `WeatherCloudShadowController`, cloud-cookie ownership, and shader globals are not modified.
- No runtime mesh generation, procedural branch graph, tree LOD, indirect renderer, collision, placement layer, scene, prefab, package, ProjectSettings, layer, tag, or source FBX change is introduced.
- No gallery component implements `Update`, `LateUpdate`, or `FixedUpdate`; hierarchy/material generation occurs only through explicit Editor actions.

#### Static validation evidence

The final filesystem/source validator reported:

```text
CHANGED_FILES=20
REMOVED_FILES=0
NEW_GUIDS=8
STATIC_ERRORS=0
```

The validation covered exact approved scope; preservation of every unapproved baseline file; LF-only content; no trailing whitespace; unique valid project-wide metadata GUIDs; balanced C#/HLSL/ShaderLab delimiters and lexical states; resolved local include paths; absence of duplicate ShaderLab pass declarations; no per-frame gallery methods; no `ModelImporter` mutation; explicit three-normal `TextureImporter` correction; deterministic material paths; root-space Weather sampling; foliage alpha/two-sided contracts; PBR/cloud-cookie include closure; mandatory cloud-receiver registration; and absence of pre-generated material assets in the delivered patch.

`git diff --no-index --check` emitted no whitespace diagnostics. The supplied archive has no `.git` directory, so branch/status/history validation remains pending in the complete repository.

#### Unavailable validation and required next action

Unity is not available in the supplied archive, and the ignored `Assets/References/Trees/` source vault is not included. C# compilation, ShaderLab/HLSL compilation, texture reimport, shared material creation, prefab instantiation, Ground sampling, Undo behavior, source-material revalidation, Weather motion, cloud-cookie appearance, shadow/depth behavior, Play Mode stability, and final scene/repository diff are therefore not represented as passed.

The next live gate is to apply the patch, allow Unity 6000.5.0f1 to compile, build the four-family vertical slice from the standalone gallery Inspector, and provide the complete copied vertical-slice report together with any compiler or shader errors. `TREE-GALLERY.3` remains blocked until the four-family visual, Weather, cloud, rebuild/remove, and Play Mode validation passes.



### TREE-GALLERY.2A adaptive Ground-domain layout correction

#### Live failure evidence

The first Unity build on 2026-07-24 compiled and completed importer/material/environment setup, but failed before creating specimens because the configured four-row layout placed the Common pair centre at world `(0, 0, -27)`, outside the assigned Ground's sampleable domain. Unity also reported four obsolete API warnings from the deprecated `FindObjectsByType` overload that accepts `FindObjectsSortMode`.

This was a gallery-layout defect, not a tree-source, material, Weather-wind, or cloud-cookie failure. The live report confirmed:

- all three bark normal corrections passed;
- all six shared material dependencies and both tree shaders resolved;
- Weather wind resources were ready;
- the Weather cloud cookie was ready;
- specimen creation had not begun when the fixed-spacing Ground sample failed.

#### Corrected architecture

- The builder now queries `GeneratedGround.TryGetSurfaceDomain` before creating hierarchy content.
- Audited source bounds determine each family's Ground-local row radius and imported-tree horizontal footprint.
- Four family rows are packed within the real domain. `familyRowSpacing` remains a preferred centre spacing, but it is adaptively reduced when the Ground cannot support the configured `18 m` spacing.
- Adjacent rows retain a minimum canopy gap and the complete layout fails before hierarchy mutation when even the minimum packed arrangement cannot fit.
- Each comparison pair receives a Ground-local X centre chosen from the valid interval that keeps both pair roots and the asymmetric imported source bounds inside the domain. Pair centres are not required to remain at X zero.
- The imported and procedural root positions are transformed from Ground-local coordinates and sampled independently with `TrySampleBaseSurface`. Uneven terrain no longer forces both specimens to use one pair-centre height.
- The pair root is positioned between the two independently sampled contacts, while imported/source-base correction remains separate.
- Both deprecated `FindObjectsByType<T>(FindObjectsInactive, FindObjectsSortMode)` calls are replaced with the Unity 6.5 overload `FindObjectsByType<T>(FindObjectsInactive)`. No sort order is requested or relied upon.

#### Corrected affected scope

Modify only:

```text
Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md
Assets/Game/Procedural/Trees/Editor/TreeReferenceGalleryBuilder.cs
```

No shader, material, importer, scene, prefab, Ground implementation, Weather implementation, cloud implementation, source tree, or other gallery file is changed by this correction.

#### Validation gate

1. Apply `TREE-GALLERY.2A` and allow Unity to compile.
2. Confirm the previous `FindObjectsSortMode` warnings are absent.
3. Run `Hierarchy > Tree Reference Gallery > Inspector > Actions > Build Four-Family Vertical Slice`.
4. Require `[Gallery Layout]` to report a valid Ground domain, one row offset and pair-centre X for every family, followed by successful independent root samples.
5. Continue with the original `TREE-GALLERY.2` visual, Weather, cloud, rebuild/remove, and Play Mode validation only after all eight specimens/slots are created.

### TREE-GALLERY.2A1 definite-assignment compile correction

The adaptive-layout correction initially used a short-circuit expression in which `sourceAsset == null` could bypass the call assigning the `failure` out variable. Unity correctly reported CS0165. The builder now handles a missing source asset first, then calls `TryInspectSourceAsset` in a separate branch and reads `failure` only after assignment. No runtime, shader, material, importer, scene, or layout behaviour changed.

### TREE-GALLERY.2B foliage readability and shadow foundation

#### Live visual evidence and decision

The four-family reference slice exposed two reusable rendering defects rather than source-import failures:

- overlapping imported foliage cards collapsed into broad, low-contrast canopy masses, especially for `Pine_5` from the gameplay camera and top-down views;
- the foliage material's `ShadowCaster` pass was disabled, so Ground shadows were trunk/branch-only, while ordinary realtime trunk shadows received by foliage appeared too harsh and card-like.

This patch intentionally addresses only contracts that carry into generated trees. It does not attempt to disguise every structural limitation of the imported card layouts.

#### Rendering ownership and data flow

- Weather remains the only wind producer. Existing root-sampled tree deformation is unchanged.
- The Weather directional cookie remains the only cloud-shadow source. The foliage shader continues to use the three-argument `GetMainLight` path with `_LIGHT_COOKIES`; no custom cloud texture or duplicate cloud evaluation is added.
- Foliage shadow **casting** and foliage realtime shadow **reception** are separate controls. Casting uses the existing alpha-clipped `ShadowCaster` pass. Reception modifies only `Light.shadowAttenuation`, so the cloud cookie carried by `Light.color` is not weakened.
- Imported references use stable object-space cell hashing as a temporary cluster-variation fallback. Generated foliage must later provide explicit stable cluster/card metadata rather than depending on this fallback.

#### New reusable foliage controls

`TreeReferenceGallery` now owns conservative reference defaults that are copied into the three deterministic shared foliage materials when the slice is built or rebuilt:

- `Foliage Shadow Casting` defaults to enabled;
- canopy-depth strength and power;
- direct-light orientation contrast;
- low-amplitude orientation readability under even lighting;
- two-sided underside darkening;
- stable cluster-variation strength and object-space cell scale;
- diffuse wrap;
- realtime shadow receive strength and minimum shadow floor;
- a foliage-only diagnostic selector independent from the existing tree wind/geometry diagnostic selector.

The default values are deliberately restrained: they create value separation without attempting to replace missing branch-tier spacing or card-cluster structure.

#### Foliage lighting contract

A new shared include, `TreeFoliageLighting.hlsl`, owns the reusable foliage receiver path:

1. evaluate SH ambient against the resolved two-sided foliage normal;
2. obtain the cookie- and shadow-aware main light through `GetMainLight(shadowCoord, positionWS, shadowMask)`;
3. preserve the main-light cookie in `Light.color`;
4. soften only realtime shadow attenuation through configurable receive strength and floor;
5. evaluate wrapped two-sided diffuse and restrained foliage specular;
6. evaluate additional URP lights through the existing Forward+/light-loop contracts;
7. multiply the resolved lighting by conservative canopy, orientation, underside, and cluster readability factors;
8. apply fog once after final colour resolution.

The source foliage alpha texture remains authoritative for forward, depth, and shadow clipping.

#### Diagnostics

The foliage shader now exposes:

- Final Rendering;
- Source Albedo;
- Alpha Mask;
- Front / Back Face;
- Canopy Height;
- Cluster Variation;
- Orientation Factor;
- Realtime Shadow;
- Cloud Cookie;
- Direct Light Response;
- Combined Lighting.

The Cloud Cookie view estimates cookie attenuation by comparing the cookie-modulated main-light colour returned by URP against `_MainLightColor`. Realtime Shadow remains separate and displays raw shadow-map attenuation before the configurable foliage reception softening.

#### Generated-tree metadata implication

The generated tree mesh contract must reserve stable foliage metadata sufficient to replace imported fallbacks. At minimum, future generated foliage needs stable cluster/card variation plus normalized canopy/branch information. Exact channel packing remains a `TREE-GEN` implementation decision, but generated trees must not depend on object-space hash cells as their authoritative variation source.

#### Affected scope

Modify:

```text
Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md
Assets/Game/Procedural/Trees/TreeFamily.cs
Assets/Game/Procedural/Trees/TreeReferenceGallery.cs
Assets/Game/Procedural/Trees/Editor/TreeReferenceGalleryEditor.cs
Assets/Game/Procedural/Trees/Editor/TreeReferenceGalleryBuilder.cs
Assets/Game/Rendering/Trees/Shaders/SH_StylizedTreeFoliage.shader
```

Create:

```text
Assets/Game/Rendering/Trees/Includes/TreeFoliageLighting.hlsl
Assets/Game/Rendering/Trees/Includes/TreeFoliageLighting.hlsl.meta
```

No bark shader, Ground, grass, Weather producer, cloud producer, River, source FBX, source texture, scene, prefab, package, ProjectSettings, layer, or tag change is approved.

#### Validation gate

1. Compile C# and the foliage shader in Unity 6000.5.0f1.
2. Select `Hierarchy > Tree Reference Gallery > Inspector > Reference Rendering` and leave both tree and foliage debug selectors on Final Rendering.
3. Run `Hierarchy > Tree Reference Gallery > Inspector > Actions > Rebuild Four-Family Vertical Slice` so all three shared foliage materials receive the new defaults and the `ShadowCaster` pass is enabled.
4. Require the copied report to show `Foliage ShadowCaster pass: Enabled` and list every readability/shadow parameter.
5. Confirm foliage now contributes alpha-shaped Ground shadows.
6. Confirm trunk/card shadows received by foliage remain visible but no longer collapse into near-black rectangular patches.
7. Compare even light, direct sun, and Weather cloud-shadow views from the gameplay camera and from above.
8. Step through the foliage diagnostics to identify any remaining source-albedo, card-normal, alpha, cookie, or shadow-map defects.
9. Run the existing Weather cloud-shadow receiver audit and require both tree shaders to remain supported.
10. Rebuild, remove, rebuild, and enter/exit Play Mode without duplicate hierarchy content, material duplication, or persistent errors.

`TREE-GALLERY.3` remains blocked only on this final four-family visual/runtime gate. Structural card overlap that cannot be corrected without changing generated foliage placement is recorded as a source-reference limitation rather than a reason for further imported-asset shader escalation.

### TREE-GALLERY.3 / 3A complete imported reference gallery

#### Accepted placement and visibility correction

The complete twenty-tree gallery must not consume the playable 40 by 40 metre Ground chunk. The four-family vertical slice may remain on the production Ground because it contains only four references and four reserved slots, but the complete library is built in a separate off-map zone to the **left** of the assigned `GeneratedGround` domain.

`TREE-GALLERY.3A` supersedes the initial switchable-page design. All twenty imported specimens and all twenty generated comparison slots must remain active simultaneously so every reference is immediately available after a rebuild. There is no active-family state and no family-cycling workflow.

The assigned Ground is used only to determine:

- the playable chunk's left boundary;
- the gallery's world orientation;
- a provable clearance between the chunk and the nearest complete-gallery content.

Complete-gallery trees do not sample or require the Ground surface. Their visible bases are aligned to flat builder-owned shadow receiver pads using the audited lowest-visible-Y correction.

#### Simultaneous family-block architecture

`Build Complete Imported Gallery` creates:

```text
Tree Reference Gallery
└── Complete Imported Gallery
    ├── Common Page
    │   ├── Shadow Receiver Pad
    │   ├── Common_1_Pair
    │   │   ├── REF_CommonTree_1
    │   │   └── PROC_CommonTree_1_SLOT
    │   └── Common_2...5_Pair
    ├── Pine Page
    ├── Twisted Page
    └── Dead Page
```

The four family roots remain active. Within each family block, five imported/generated pairs form audited non-overlapping rows. The family blocks share the Ground's forward orientation but are placed progressively farther left:

1. Common is nearest to the playable Ground at the configured left clearance;
2. Pine begins beyond Common's full padded width plus the configured family gap;
3. Twisted begins beyond Pine's full padded width plus the family gap;
4. Dead begins beyond Twisted's full padded width plus the family gap.

This produces one persistent off-map inspection strip. All twenty imported references and all twenty reserved generated roots can be inspected without hierarchy toggles or regeneration.

The Inspector exposes:

- `Build Complete Imported Gallery`;
- `Rebuild Complete Imported Gallery`;
- `Remove Complete Imported Gallery`;
- `Copy Last Complete Gallery Report`.

#### Adaptive block layout

Each family block is resolved independently from all five audited source bounds:

1. calculate the imported/reference separation required by the source canopy width plus the configured comparison clearance;
2. reserve the same audited bounds around the procedural comparison root;
3. pack five rows along the block Z axis using each source's actual Z extent and the configured minimum row gap;
4. calculate the complete block footprint;
5. expand that footprint by the configured pad margin;
6. position the block so its pad's right edge has the current required left clearance from the preceding boundary;
7. advance the next block's right-edge clearance by the current padded width plus `Complete Gallery Family Gap`.

This arrangement supports the highly asymmetric and much larger Twisted family while keeping all complete-gallery content outside the playable chunk and non-overlapping.

#### Shadow receiver pads

Each family block owns one lightweight flattened cube named `Shadow Receiver Pad`:

- it uses a deterministic shared `MAT_TreeGallery_ShadowPad` material based on `Universal Render Pipeline/Lit`;
- it receives realtime foliage/trunk shadows and the normal URP main-light cookie;
- it does not cast shadows;
- it has no collider;
- it is sized from the measured family-block footprint plus the configured margin;
- it does not duplicate or reference the production Ground mesh.

Separate pads remain preferable to one enormous combined mesh because each pad is already measured from its family footprint, preserves clear family grouping, and avoids filling unused space between blocks.

#### Rendering and metadata

Every imported specimen reuses the accepted TREE-GALLERY.2B contracts unchanged:

- corrected bark normal maps;
- shared bark/foliage materials;
- Weather-owned root-sampled wind;
- authoritative cloud-cookie shading;
- foliage readability controls;
- alpha-clipped foliage shadows;
- family response defaults and stable per-variant phase;
- original imported source scale.

Every reserved procedural slot records the matching family, source variant, source GUID, target audited bounds, height, width, triangle count, material layout, and comparison root. Future generated trees can populate these slots without rediscovering their reference target.

#### Diagnostics and report

The complete-gallery report records:

- source and Ground prerequisites;
- normal-map and shared-material status;
- the deterministic shadow-pad material;
- playable Ground domain, initial left clearance, and family gap;
- block and pad dimensions for every family;
- right-edge and proved chunk clearance for every family;
- total leftward extent of the simultaneous gallery strip;
- per-family height, width, and triangle ranges;
- every imported/reference pair position, separation, base correction, and triangle count;
- Weather wind and cloud-cookie readiness;
- confirmation that all four family blocks are active;
- forty total specimens/slots and the aggregate source triangle count.

Scene labels are shown for all active specimens. The full reference library therefore remains continuously inspectable after any complete-gallery rebuild.

#### Affected scope

Modify only:

```text
Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md
Assets/Game/Procedural/Trees/TreeReferenceGallery.cs
Assets/Game/Procedural/Trees/Editor/TreeReferenceGalleryEditor.cs
Assets/Game/Procedural/Trees/Editor/TreeReferenceGalleryBuilder.cs
```

The gallery action may create or update this deterministic material asset in Unity:

```text
Assets/Game/Demo/Materials/Trees/MAT_TreeGallery_ShadowPad.mat
```

No Ground mesh, Ground material, scene-owned gameplay object, grass, Weather producer, cloud producer, tree shader, source FBX, source texture, prefab, package, ProjectSettings, layer, or tag change is part of TREE-GALLERY.3A.

#### Validation gate

1. Compile TREE-GALLERY.3A in Unity 6000.5.0f1.
2. Run `Hierarchy > Tree Reference Gallery > Inspector > Actions > Rebuild Complete Imported Gallery` when the former switchable gallery already exists.
3. Require the report to show `Status: PASS`, forty specimens/slots, five variants per family, positive proved chunk clearance for every block, and confirmation that all family blocks are active simultaneously.
4. Confirm `Complete Imported Gallery` is outside the playable chunk to its left.
5. Confirm Common, Pine, Twisted, and Dead all remain visible without changing an active-family selector.
6. Confirm every family block contains five imported trees, five reserved procedural slots, and one correctly sized pad.
7. Confirm no family pads, trees, or comparison footprints overlap neighbouring blocks.
8. Confirm imported bases meet their pad surface and neither roots nor pads overlap the playable Ground.
9. Confirm pads receive trunk and alpha-clipped foliage shadows while casting none of their own.
10. Confirm Weather wind and cloud-cookie shading remain functional across the full off-map strip.
11. Rebuild, remove, rebuild, then enter and exit Play Mode without duplicates, missing materials, hierarchy mutation, or persistent errors.
12. **Satisfied:** the live TREE-GALLERY.3A report passed and `TREE-GALLERY.FREEZE` is recorded by TREE-PLAN.2. The generated-tree implementation is unblocked.

## TREE-PLAN.2 generated-tree authoring-contract documentation record

### Decision frozen

The accepted complete reference gallery is frozen after the live `TREE-GALLERY.3A` report passed with:

- twenty imported source trees;
- twenty reserved generated comparison slots;
- all four family blocks active simultaneously;
- 118,610 imported triangles;
- positive playable-chunk clearance for every family block;
- Weather wind resources ready;
- cloud cookie ready;
- accepted bark/foliage materials, foliage readability, and alpha-clipped foliage shadows.

### New locked requirements

This documentation patch adds and locks:

- reference-calibration presets distinct from family profiles;
- family profile, calibration preset, variant recipe, instance override, and seed-variation authoring layers;
- independent foliage volume and foliage density;
- crown-envelope, lobe, fullness, cluster-size, and card-size controls;
- exact/ranged branch counts and branching-distribution controls;
- separate trunk, primary-branch, secondary-branch, and tertiary-branch curvature semantics;
- reusable bark and foliage palette data with no material-per-tree architecture;
- independent deterministic random streams;
- dependency fingerprints and selective regeneration;
- generated foliage/branch metadata reservations;
- explicit determinism and invalidation tests;
- the next-patch Inspector action cleanup;
- `TREE-GEN.1` as the next implementation patch.

### Scope

This patch changes only the canonical Markdown document. It does not modify C#, shaders, materials, scenes, importers, profiles, source assets, packages, ProjectSettings, layers, or tags.

### Validation

The updated document is required to retain one H1, balanced fenced blocks, LF line endings, no trailing whitespace, and a complete ordered roadmap. The generated-tree implementation remains pending explicit execution of `TREE-GEN.1`.



## TREE-GEN.1 deterministic structural-foundation implementation record

### Objective

Implement the generated-tree authoring and structural source-of-truth layer without creating final bark meshes, foliage cards, LOD assets, proxies, runtime renderer buffers, or forest placement.

### Implemented authoring assets and layers

The patch adds the approved reusable asset/data types:

```text
TreeFamilyProfile
TreeReferenceCalibrationPreset
TreeMaterialPalette
TreeGenerationRecipe
TreeGenerationOverrides
```

`TreeFamilyProfile` owns family-safe ranges for overall form, crown volume/fullness, trunk form, branch counts/distributions, per-order curvature, foliage volume, foliage density, damage, and structural budgets. It provides explicit starter grammars for Common, Pine, Twisted, and Dead.

`TreeReferenceCalibrationPreset` stores an optional imported FBX path/GUID, target dimensions, comparison tolerance, palette override, and sparse parameter overrides. The type is twenty-capable without embedding source mesh topology.

`TreeMaterialPalette` stores shared texture identity and bark/foliage colour ranges. Palette-only changes are fingerprinted separately and do not regenerate structural topology.

`TreeGenerationRecipe` resolves the family profile, optional calibration preset, optional palette override, authored recipe overrides, master seed, age class, and optional locked subsystem seeds.

`TreeGenerationOverrides` distinguishes inherited, exact, and ranged authored values. It exposes independent controls for crown volume versus foliage density, branch counts, trunk/branch curvature, damage, and bark/foliage colours.

### Deterministic streams and fingerprints

The implementation derives independent deterministic seeds for:

```text
TrunkShape
TrunkForks
PrimaryBranchLayout
SecondaryBranchLayout
TertiaryBranchLayout
BranchCurvature
StructuralDamage
FoliageClusterPlacement
FoliageClusterShape
FoliageCardPlacement
FoliageCardShape
MaterialVariation
LODSelection
ProxyGeneration
```

Every stream derives from the master seed, family/profile identity and version, recipe identity/version, optional imported-target identity used by a comparison recipe, generator version, and stream identifier unless an explicit locked seed replaces that derivation.

The generated definition records separate dependency, trunk, branch, foliage-geometry-intent, palette, and structural fingerprints. Structural fingerprints include trunk and branch topology/frames only; palette and not-yet-built foliage geometry remain separately invalidatable.

### Structural generation

`TreeGenerator` creates:

- a sampled tapered trunk curve;
- data-driven primary branch attachment, local transported-frame yaw, tier/semi-whorl placement, azimuth symmetry, and directional bias;
- deterministic secondary and tertiary branches;
- optional trunk fork output;
- deterministic missing, dead, and broken branch states;
- stable branch IDs and backward-only parent indices;
- branch-local foliage eligibility intervals;
- conservative structural bounds and footprint metadata.

All centreline curves use stable transported frames. The implementation carries tangent, normal, binormal, radius, and normalized distance per curve sample and validates orthogonality, finiteness, length, radius, attachments, parent order, and unique stable IDs.

No final mesh vertices, triangles, materials, foliage cards, generated assets, or runtime buffers are created by the generator.

### Authoring component and diagnostics

`ProceduralTreeInstance` is attached only to managed procedural gallery slots. It owns the selected managed recipe, sparse instance overrides, structural definition, preview state, generated bark output, fingerprints, and last reports. It performs no generation from `Update`, `OnValidate`, hierarchy callbacks, scene load, domain reload, or ordinary Play Mode entry.

The `Tree Reference Gallery` Inspector provides the sole normal one-action rebuild, removal, source audit/repair, and report-copy workflows. The selected slot Inspector may expose focused recipe/override inspection and explicit regeneration for diagnostics, but it does not create separate starter profiles, palettes, or recipes outside the managed library.

The Scene preview draws the trunk and branch graph directly with Handles, optional transported frames, attachment markers, and structural bounds. It does not create one GameObject per branch.

The complete generation report includes authoring inputs, every independent seed, resolved parameter values and ownership trace, branch counts by order, rejected/dead/broken/foliage-eligible counts, control/sample counts, total length, radius range, generation time, bounds, footprint, fingerprints, warnings, and validation status.

The deterministic validation suite tests:

1. identical complete inputs reproduce the structural fingerprint;
2. foliage colour changes preserve trunk, branch, and foliage-geometry fingerprints while changing the palette fingerprint;
3. foliage-volume changes preserve trunk and branch fingerprints while changing foliage intent;
4. foliage-density changes preserve trunk and branch fingerprints while changing foliage intent where the family permits density variation;
5. primary-branch count changes preserve trunk and palette fingerprints while changing branch topology;
6. trunk-curvature changes invalidate trunk and descendant branch fingerprints;
7. any locked subsystem seed remains stable while an unlocked stream changes with the master seed.

### Gallery Inspector cleanup

The gallery's ordinary Actions section now exposes only:

```text
Rebuild Complete Reference Gallery
Remove Complete Reference Gallery
```

The on-map four-family slice is retained inside collapsed `Advanced Validation`, and source-import repair is retained inside collapsed `Maintenance`. Repair is disabled when all three audited bark-normal importers are already correct. Historical separate Build/Rebuild buttons are removed from the ordinary workflow.

### Actual affected scope

New runtime/source files:

```text
Assets/Game/Procedural/Trees/TreeAuthoringPrimitives.cs
Assets/Game/Procedural/Trees/TreeFamilyProfile.cs
Assets/Game/Procedural/Trees/TreeReferenceCalibrationPreset.cs
Assets/Game/Procedural/Trees/TreeMaterialPalette.cs
Assets/Game/Procedural/Trees/TreeGenerationRecipe.cs
Assets/Game/Procedural/Trees/TreeGenerationOverrides.cs
Assets/Game/Procedural/Trees/TreeGenerationParameters.cs
Assets/Game/Procedural/Trees/TreeBranchDefinition.cs
Assets/Game/Procedural/Trees/TreeFoliageClusterDefinition.cs
Assets/Game/Procedural/Trees/TreeGenerationMetrics.cs
Assets/Game/Procedural/Trees/TreeDefinition.cs
Assets/Game/Procedural/Trees/TreeGenerator.cs
Assets/Game/Procedural/Trees/ProceduralTreeAuthoring.cs — historical TREE-GEN.1 harness, removed by the TREE-GEN.2B cleanup
```

Historical Editor files later removed by the TREE-GEN.2B cleanup:

```text
Assets/Game/Procedural/Trees/Editor/TreeAuthoringAssetFactory.cs
Assets/Game/Procedural/Trees/Editor/ProceduralTreeAuthoringEditor.cs
```

Modified existing files:

```text
Assets/Game/Procedural/Trees/Editor/TreeReferenceGalleryBuilder.cs
Assets/Game/Procedural/Trees/Editor/TreeReferenceGalleryEditor.cs
Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md
```

Every new C# file has a unique metadata GUID. No scene, prefab, material, texture, FBX, shader, compute shader, Ground, grass, Weather producer, cloud producer, River, package, ProjectSettings, layer, or tag file is included.

### Static validation and pending live gate

Source-level validation checks balanced delimiters and lexical states, LF-only content, trailing whitespace, metadata uniqueness, exact changed-file scope, no per-frame authoring generation, no obsolete object-sort API, no material-per-tree creation, and preservation of the accepted imported-gallery rendering contracts.

The supplied archive contains no Unity Editor, so authoritative C# compilation, serialization, Scene Handles rendering, starter-asset creation, generation reports, deterministic suite, four-family previews, and Play Mode stability remain pending in Unity 6000.5.0f1.

`TREE-GEN.2` may not begin until at least one Common, Pine, Twisted, and Dead recipe has produced a passing structural report and the determinism/dependency suite passes for each family.

## TREE-GEN.1B unified gallery-authoring workflow implementation record

### Correction to the TREE-GEN.1 workflow

The former low-level standalone authoring harness was not the authoritative production workflow. Its manual starter-asset creation path exposed generator diagnostics as ordinary authoring and created unnecessary setup work. `TREE-GEN.1B` superseded that workflow, and the later TREE-GEN.2B cleanup removes the stale harness and starter assets completely.

The authoritative generated-tree path is now:

```text
Tree Reference Gallery
    -> one managed TreeGenerationLibrary asset
        -> four family profiles
        -> four material palettes
        -> twenty imported-reference calibration presets
        -> twenty generation recipes
    -> twenty existing PROC_*_SLOT objects
        -> one ProceduralTreeInstance per slot
            -> deterministic TreeGenerator output
            -> structural preview now
            -> bark mesh in TREE-GEN.2
            -> foliage mesh in TREE-GEN.3
```

There is one generator, one managed authoring library, one gallery coordinator, and one generated-tree instance component per procedural comparison slot. Isolated debugging uses the selected `ProceduralTreeInstance`; no second standalone authoring object or starter-asset workflow remains.

### Normal one-button workflow

The ordinary `Tree Reference Gallery` Inspector exposes:

```text
Rebuild Complete Tree Comparison Gallery
Remove Generated Tree Outputs
```

`Rebuild Complete Tree Comparison Gallery` performs the complete operation without a save dialog or manually created authoring objects:

1. finds or confirms the explicit reference Ground;
2. runs the complete source audit;
3. applies required source-import corrections through the existing gallery builder;
4. rebuilds all twenty imported references and twenty procedural slots outside the playable chunk;
5. creates or repairs `Assets/Game/Demo/Profiles/Trees/TreeGenerationLibrary.asset`;
6. creates missing family profiles, palettes, calibration presets, and recipes as sub-assets of that one library;
7. preserves existing managed profile/recipe authoring instead of recreating valid sub-assets;
8. binds every existing `PROC_*_SLOT` to its matching recipe;
9. generates all twenty deterministic structural definitions in those slots;
10. draws all twenty structural previews at their final comparison positions;
11. performs a repeat-fingerprint check for every slot;
12. runs the complete determinism/dependency suite once per family;
13. copies one aggregate report to the clipboard.

`Remove Generated Tree Outputs` removes generated instance components/previews while retaining the imported references, procedural slot GameObjects, managed library, profiles, palettes, calibration presets, and authored recipes.

### Managed library contract

`TreeGenerationLibrary.asset` is automatically saved at the deterministic project path:

```text
Assets/Game/Demo/Profiles/Trees/TreeGenerationLibrary.asset
```

Its profiles, palettes, calibrations, and recipes are sub-assets. Rebuilding repairs missing references and missing sub-assets but does not reset valid edited profiles, palettes, recipes, seeds, or instance-independent authoring.

Each calibration preset records its imported FBX path/GUID and audited dimensions. Initial managed calibration sets exact target height and a conservative primary-branch length ratio derived from the imported width/height relationship. The imported source remains a visual target rather than copied topology.

### Procedural slot contract

Every complete-gallery procedural slot receives `ProceduralTreeInstance`, which owns:

- managed library reference;
- selected generation recipe;
- seed;
- sparse per-instance overrides;
- generated structural definition;
- generation report and fingerprint;
- structural-preview visibility.

Selecting one slot permits switching to any recipe in the same library, changing its seed/overrides, regenerating only that tree, resetting its overrides, or copying its report. A complete gallery rebuild restores each slot to its matching imported-reference recipe.

The structural preview is intentionally a Scene-view branch graph until `TREE-GEN.2`; no bark or foliage render meshes are implied by `TREE-GEN.1B`.

### Gallery Inspector cleanup

The ordinary workflow no longer exposes separate source-import, Build, Rebuild, family-cycling, starter-asset, or per-family setup buttons. On-map four-family validation remains collapsed under `Advanced Validation`. Source repair and complete-gallery removal remain collapsed under `Maintenance`. Report-only operations remain collapsed under `Diagnostics And Reports`.

### Validation gate

`TREE-GEN.2` remains blocked until Unity validation confirms:

- the patch compiles;
- one click creates or repairs the managed library;
- the complete gallery still contains twenty imported references and twenty procedural slots;
- all twenty slots contain passing `ProceduralTreeInstance` definitions;
- all twenty structural previews appear at the off-map slot positions with Gizmos enabled;
- all twenty repeat fingerprints pass;
- Common, Pine, Twisted, and Dead dependency suites pass;
- selecting a procedural slot exposes recipe switching and per-tree regeneration;
- rebuilding is idempotent and produces no duplicate library sub-assets or slot components;
- Play Mode entry/exit causes no automatic regeneration or hierarchy mutation.


## TREE-GEN.1D structural grammar and reference-calibration correction

### Why this patch exists

Unity validation accepted the `TREE-GEN.1B` orchestration and determinism foundation:

- the managed generation library remained singular;
- all twenty procedural slots generated;
- deterministic repeat checks passed `20 / 20`;
- Common, Pine, Twisted, and Dead dependency suites passed;
- rebuild, removal, restoration, and Play Mode entry/exit were stable.

The Scene-view skeletons nevertheless exposed structure that should not be converted into bark meshes yet: uncontrolled spline curling, excessive higher-order branching, oversized Twisted/Dead/Pine bounds, and an all-trees-at-once preview that obscured direct imported/generated comparison.

`TREE-GEN.1D` is therefore the final structure-only correction gate before `TREE-GEN.2`.

### Managed profile upgrade

Managed `TreeFamilyProfile` assets upgrade from profile version `1` to version `2` during the ordinary gallery rebuild. The upgrade is deterministic and occurs only for managed profiles whose serialized version predates the current grammar.

Version `2` tightens family grammar:

- **Common:** moderate rounded branching, reduced higher-order curvature, two-to-three secondaries per primary, and zero-to-one tertiary per secondary;
- **Pine:** central-leader/tier grammar, one-to-two secondaries, zero-to-one tertiary, reduced branch sweep/curl, and shorter higher orders;
- **Twisted:** readable asymmetric trunk/primary masses with bounded irregularity rather than repeated loops;
- **Dead:** sparse exposed structure, one-to-two secondaries, no tertiary order in the managed default, and stronger branch-loss survival filtering.

Maximum managed branch budgets are reduced to family-appropriate ceilings rather than relying on the former generic `384`-branch limit.

### Structural constraint contract

Each family profile now owns explicit structural constraints:

```text
maximum trunk horizontal displacement
maximum trunk turn per control segment
maximum branch turn per control segment
maximum accumulated primary turn
maximum accumulated higher-order turn
maximum primary arc/chord ratio
maximum higher-order arc/chord ratio
minimum forward progress
maximum radial return toward the attachment
secondary survival probability
tertiary survival probability
allowed crown-envelope overshoot
```

Generation constrains control-point progression before transported frames and fingerprints are calculated.

The constraints do not erase family identity:

- Common and Pine are deliberately tighter;
- Twisted permits more accumulated turn and displacement;
- Dead permits irregular silhouette but not unconstrained spline coils.

Higher-order branches inherit a controlled fraction of the parent tangent, become progressively shorter, and use lower curvature, sweep, irregularity, curl, and twist amplitudes than primary branches.

### Imported-reference dimension calibration

`TreeReferenceCalibrationPreset` upgrades to calibration version `2` and stores:

```text
target visible height
target visible X width
target visible Z depth
target crown start
dimension tolerance
```

The library builder refreshes these values from each procedural slot's audited imported bounds.

Reference-calibrated recipes continue to set exact target height. Their primary length ratio is now derived conservatively from imported width/height using a `0.5` factor rather than the previous `0.7` factor.

Generated trunk and branch control points are constrained inside the imported target height and X/Z envelope before spline sampling. After structure creation, a bounded deterministic X/Z fit adjusts non-trunk geometry around the sampled trunk centreline until the generated width and depth approach the imported target. The trunk geometry is never scaled by that fit, and shared parent/child attachment positions receive the same centreline-relative transform. This preserves subsystem isolation: changing branch count may refit descendants but does not rescale or regenerate the trunk.

Reference-fit metrics record:

```text
generated/reference height ratio
generated/reference X-width ratio
generated/reference Z-depth ratio
dimension-tolerance result
```

The default managed tolerance is `±15%`.

### Structural diagnostics

`TreeGenerationMetrics` now records:

```text
maximum per-sample turn
maximum accumulated branch turn
maximum arc/chord ratio
backward-progress violation count
foliage-eligible crown-envelope violation count
maximum crown-envelope overshoot
reference height/width/depth ratios
reference-calibration tolerance status
```

The individual generation report and the unified twenty-tree report expose these values.

A reference-calibrated tree that exceeds its dimension tolerance fails structural generation before bark mesh work begins.

### Preview scope

The complete imported gallery remains visible at all times. Generated structural previews default to:

```text
Preview Scope: Selected Tree
Trunk: On
Primary Branches: On
Higher-Order Branches: On
Attachment Points: Off
Bounds: On
Transported Frames: Off
```

Available scope choices are:

```text
Selected Tree
Selected Family
All Trees
```

The settings live on `Tree Reference Gallery > Generated Tree Library` and are propagated to all managed `ProceduralTreeInstance` components.

Selecting a `PROC_*_SLOT` is therefore the normal direct-comparison workflow. `All Trees` remains an explicit diagnostic mode rather than the default.

### TREE-GEN.2 gate

`TREE-GEN.2` remains blocked until Unity validation confirms:

- all twenty calibrated structures generate;
- all twenty reference dimension checks pass;
- repeat fingerprints remain `20 / 20`;
- all four dependency suites remain passing;
- selected-tree preview scope behaves correctly;
- Common, Pine, Twisted, and Dead skeletons no longer show obvious loops, repeated trunk crossings, or oversized higher-order branches;
- rebuilding upgrades the managed profiles/calibrations once without creating duplicate sub-assets.

---

## TREE-GEN.2A four-family bark-mesh vertical slice

### Status

Live-validated as a useful vertical slice but **not accepted**. The four generated representatives render and expose the correct structural data path, but bark geometry correctness and missing authoring controls block expansion to all twenty slots.

`TREE-GEN.2A` is the first visible generated-tree geometry patch. It deliberately meshes only the first managed variant from each family:

```text
Common 1
Pine 1
Twisted 1
Dead 1
```

The other sixteen procedural slots retain their calibrated structural previews until the shared bark path is accepted.

### Authoritative output ownership

The existing `ProceduralTreeInstance` remains the single owner of generated output for its gallery slot. The patch does not create a second tree system.

Each representative slot owns one builder-created child:

```text
PROC_<Family>_1_SLOT
└── Generated Bark Mesh
    ├── MeshFilter
    └── MeshRenderer
```

One combined mesh contains the trunk and every accepted branch in the structural definition. Persistent managed `Mesh` sub-assets are stored inside the existing `TreeGenerationLibrary.asset` and are updated in place on rebuild; no loose per-tree mesh files or unique material assets are created.

### Bark geometry contract

`TreeBarkMeshGenerator` consumes the transported frames already frozen by `TREE-GEN.1` and builds swept tapered tubes with:

- branch-order-dependent radial resolution;
- one UV seam per branch;
- cylindrical bark UVs measured in metres along each branch;
- generated radial normals and circumferential tangents;
- closed trunk base and closed branch tips;
- overlapping branch roots at parent centreline attachments to avoid visible gaps;
- one combined submesh and one shared bark material per generated tree;
- vertex-colour metadata carrying wind mask, branch order, branch stiffness, and deterministic phase.

Default radial segments are intentionally conservative:

```text
Common:  10 / 8 / 6 / 5
Pine:    10 / 7 / 5 / 4
Twisted: 12 / 9 / 7 / 5
Dead:    10 / 8 / 6 / 5
          trunk / primary / secondary / tertiary
```

### Rendering integration

Generated bark reuses the accepted shared materials and `PS3D/Trees/Stylized Tree Bark` shader. Per-instance bark tint comes from the resolved `TreeMaterialPalette` through a `MaterialPropertyBlock`; the system does not instantiate materials per tree.

The generated renderer supplies:

```text
_TreeWindMaskMode = Vertex Colour Red
_TreeBoundsMinY / _TreeBoundsHeight
_TreeRootPositionOS
family stiffness and macro wind strength
instance phase
current tree debug mode
```

The existing Weather wind field and URP main-light cloud cookie remain authoritative. Generated bark uses the same Forward, ShadowCaster, and DepthOnly shader passes as imported bark.

### Dead-family targeted correction

Managed family profiles upgrade to version `3` in this patch. The Dead profile receives a narrow correction before meshing:

- more primary opportunities;
- one-to-three shorter secondary branches;
- shorter maximum primary length ratios;
- lower missing-branch probability;
- stronger dead-state probability without deleting the silhouette;
- no tertiary branch order.

This is not a broad structural rewrite. It only gives leafless Dead trees enough visible terminal structure while preventing a few long branches from dominating the entire silhouette.

### Foliage diagnostic warning correction

The foliage shader diagnostic property now uses the enum type directly:

```text
[Enum(ProgrammaticStylized3D.Trees.TreeFoliageDebugMode)]
```

This replaces the eleven inline label/value arguments that Unity 6.5 failed to instantiate as a material drawer. The runtime diagnostic values and shader behavior are unchanged.

### Rebuild and removal behavior

`Tree Reference Gallery > Rebuild Complete Tree Comparison Gallery` remains the sole normal build action. It now:

1. rebuilds all twenty imported references and slots;
2. upgrades/repairs the managed generation library;
3. generates all twenty calibrated structural definitions;
4. builds or updates the four representative bark meshes;
5. runs repeatability and family dependency validation;
6. copies one aggregate report.

`Remove Generated Tree Outputs` removes the scene `Generated Bark Mesh` children together with `ProceduralTreeInstance` components while retaining the managed library, recipes, and reusable mesh sub-assets.

### TREE-GEN.2A acceptance gate

The vertical slice is accepted only when Unity confirms:

- all twenty structural definitions still pass;
- repeatability remains `20 / 20`;
- all four dependency suites pass;
- exactly four generated bark meshes are reported;
- each mesh has finite vertices, normals, tangents, UVs, colours, and indices;
- no visible open tips or trunk bottoms;
- branch roots do not show obvious gaps;
- Common, Pine, Twisted, and Dead bark render beside their imported references;
- generated bark receives light, shadows, cloud-cookie shading, and Weather wind;
- the foliage material-drawer warning no longer appears.

After acceptance, the same mesh path expands to all twenty slots before foliage generation begins.

## TREE-PLAN.3 family templates, ranged recipes, and TREE-GEN.2B plan

### Status

Approved and implemented as the documentation-first portion of `TREE-GEN.2B`. This section remains the authoritative implementation and acceptance contract. Source changes are prepared, but Unity compile, aggregate diagnostic execution, and live four-family visual acceptance are still required before the patch may be frozen.

### Decisions frozen

1. **No reference-match mode.** Development comparison recipes simply use baseline values calibrated to the imported references.
2. **Family profiles are templates.** They provide defaults, allowed ranges, budgets, and grammar—not rigid final tree identities.
3. **Recipes are named ranged configurations.** A recipe inherits a family and may set every supported control to inherited, exact, or min/max range.
4. **Seeds vary recipes deterministically.** Multiple trees from one recipe may vary inside its authored ranges while retaining the recipe identity.
5. **Instance overrides remain sparse exact exceptions.** They are not a competing authoring system.
6. **Imported reference metadata is diagnostic.** Source bounds, identity, and tolerance may remain in compatibility assets, but must not create a public authoring or rendering mode.
7. **Reference gallery colours are the baseline.** Generated comparison trees must use the same family texture/material path and neutral/default tint required to match the imported tree as closely as practical.
8. **Creative colour variation remains supported.** It is used by future recipes, not by introducing a comparison-mode switch.

### Authoritative full control schema

#### Overall form and trunk

```text
Tree Height
Trunk Base Radius / Thickness
Trunk Taper
Trunk Control-Point Count
Trunk Centerline Curvature
Trunk Bend Count / Frequency
Trunk Lateral Displacement
Trunk Directional Drift
Trunk Lean Strength
Trunk Lean Direction
Trunk Spiral Strength
Trunk Spiral Turns / Frequency
Trunk Spiral Direction
Trunk Surface Torsion / Ring Rotation
Trunk Irregularity
Trunk Fork Probability
Trunk Fork Height
```

`Trunk Spiral Strength` changes the centerline silhouette. `Trunk Surface Torsion` only rotates the transported/ring frame and texture orientation. They must not be conflated.

#### Branch population and placement

```text
Primary Branch Count / Range
Secondary Branches Per Primary / Range
Tertiary Branches Per Secondary / Range
Maximum Branch Order
Lowest Primary Branch Height
Highest Primary Branch Height
Crown Start Height
Attachment-Height Distribution
Minimum Vertical Attachment Spacing
Tier Count
Branches Per Tier
Tier Irregularity
Azimuth Symmetry
Directional Bias Angle
Directional Bias Strength
One-Sidedness / Exposure Bias
Lower Branch Retention / Pruning
Missing Branch Probability
Dead Branch Probability
Break Probability
```

`Azimuth Symmetry` controls how evenly branches fill the circumference. `Directional Bias Strength` controls deliberate lopsidedness toward an authored direction. A tree may therefore be symmetrical, mildly biased, or strongly one-sided without changing family.

#### Branch launch and shape

```text
Initial Branch Elevation / Launch Angle
Branch Arch Direction (-down / +up)
Branch Arch Strength
Late Gravity Sag / Droop
Primary Branch Curvature
Primary Branch Side Sweep
Primary Branch Torsion
Primary Branch Irregularity
Primary Branch End Curl
Primary Branch Length
Primary Branch Radius / Taper
Parent-Direction Inheritance
Higher-Order Curvature Scale
Higher-Order Length Scale
Higher-Order Radius Scale
```

Initial elevation, arch direction, arch strength, and late sag are distinct responsibilities. The generator must not make one control silently alter all four.

#### Crown and foliage

The previously frozen independent crown-volume and foliage-density controls remain authoritative, including crown start, width/height, lobe count/size, fill, cluster dimensions, radial spread, card size, cluster count, cards per cluster, occupancy, terminal probability, and retention.

#### Appearance

```text
Bark Texture Family
Neutral Baseline Bark Tint
Authored Bark Tint / Hue / Saturation / Value
Root Darkening
Upper-Trunk Variation
Branch-Order Variation
Foliage Base / Highlight / Shadow Colours
Foliage Hue / Saturation / Value Variation
Cluster Colour Variation
Top-to-Bottom Gradient
```

For the twenty reference-comparison recipes, the baseline bark tint must be neutral unless the imported material itself has a non-white tint. Generated bark must use the same shared family material and texture as the imported reference. The aggregate report must list material asset, base texture, material base colour, resolved recipe tint, and final property-block tint.

### Family-template intent

The initial family templates should configure the common controls rather than require unique code paths:

| Family | Template defaults |
| --- | --- |
| Common | Relatively straight and moderately thin trunk; little/no centerline spiral; higher branch start; broad balanced azimuth distribution; moderate upward or neutral arch; rich-crown-capable foliage ranges. |
| Pine | Strong central leader; tier/semi-whorl placement; comparatively symmetrical azimuth; branch arch commonly neutral/downward but fully configurable; height-dependent tier length. |
| Twisted | Wider centerline-curvature and spiral ranges; stronger lateral displacement; broader symmetry/directional-bias ranges; sparse directional crown by default, not mandatory. |
| Dead | No living foliage; exposed readable structure; configurable low/high branch start; configurable spiral; break/removal ranges; default balance sufficient for reference matching while allowing lopsided recipes. |

Named recipes may cross family stereotypes—for example `Twisted Pine`—by changing ranges without introducing a new generator system.

### Future recipe creator

A later authoring patch should add a clear library action such as `Create Recipe From Family`, ask for a recipe name, duplicate the family defaults into inherited/ranged fields, and save it as a managed sub-asset of `TreeGenerationLibrary.asset`. It must support editing exact values and ranges and previewing deterministic variants. This tool is desirable but is not a prerequisite for correcting `TREE-GEN.2B` bark geometry.

### TREE-GEN.2A live findings

The first visible meshes are useful and structurally promising, but the vertical slice is not accepted.

#### Blocking mesh defects

1. **Exterior faces are not consistently observable.** From several angles the viewer sees the hollow interior while expected exterior faces disappear. This indicates side/cap winding and/or normal orientation is inconsistent with Unity front-face culling.
2. **Trunk bottoms appear open.** The base cap exists in source intent but does not render as a reliable outward-facing closure.
3. **Branch roots visibly clip through trunks.** Child tubes currently begin from parent centerline attachment data and can pass through or emerge from the opposite side.
4. **Branch-root geometry forms hard stepped wedges/spikes.** Independent tubes overlap without a controlled root transition or collar.
5. **Normals/tangents cannot be trusted until winding is corrected.** Geometry validation must prove face orientation rather than relying only on stored radial normals.

#### Useful control gaps revealed by the vertical slice

1. Signed branch arch direction and independent arch strength.
2. Distinct initial launch angle and late gravity sag.
3. Actual trunk centerline spiral strength/frequency, separate from frame/surface torsion.
4. Lowest branch height, highest branch height, crown start, and pruning controls.
5. Azimuth symmetry and explicit directional-bias angle/strength.
6. Neutral baseline bark colour matching for imported/generated comparisons.
7. Family defaults expressed through the same controls rather than hard-coded family-only behavior.

### TREE-GEN.2B implementation plan

`TREE-GEN.2B` remains a four-family vertical slice. Do not expand bark generation to all twenty slots and do not begin foliage until this acceptance gate passes.

#### First-patch code-audit corrections

The source audit performed before implementation adds the following binding decisions to the plan:

1. Side-wall winding is confirmed inward under the transported-frame convention and must be reversed in geometry. Existing cap winding is retained unless the geometric cap audit proves a failure; the trunk-base tangent handedness is corrected independently.
2. Primary-branch and trunk-fork azimuth must be resolved in the transported parent `Normal/Binormal` frame, not global XZ. Curved and leaned trunks otherwise produce internally inconsistent branch roots and symmetry controls.
3. Existing `TrunkTwistDegrees` data is preserved as surface torsion. New spiral controls alter trunk centreline control points before frame transport.
4. Managed profile upgrades are fieldwise. Bumping a schema version must never call a whole-profile reset on an existing matching-family profile. Managed recipe binding repair and serialized-content migration are separate operations.
5. Branch root centreline correction precedes bark collar construction. A decorative collar around a centreline that still crosses the parent is not acceptable.
6. The bark input/settings fingerprint and emitted geometry-content fingerprint are separate. The latter hashes vertex attributes and indices, and every representative is rebuilt into a temporary verification mesh to prove repeatability.
7. A failed topology or repeatability build clears the managed mesh and removes the stale scene child so an older passing mesh cannot masquerade as the current result.
8. Family identity supplies defaults only. Tiering, symmetry, directional bias, launch, arch, and sag are resolved data-driven controls rather than family-only generator branches.
9. Reference width/depth calibration may move generated branches after their initial construction. Child branches are therefore re-anchored parent-before-child after calibration fitting, and their local attachment axes are refreshed before validation and meshing.

#### Phase 1 — deterministic bark topology audit

Add mesh-build diagnostics that report at minimum:

```text
finite vertex/normal/tangent/UV/index checks
degenerate triangle count
raw boundary-edge count
position-welded UV/hard-normal seam count
expected embedded child-root loop count
unexpected exposed/open boundary-loop count
non-manifold edge count after seam classification
outward-facing side triangle count
inward-facing side triangle count
cap orientation failures
zero-area ring segments
branch-root opposite-side emergence count
branch-root loop outside-parent count
```

Raw index-boundary count is diagnostic only. Cylindrical UV seams duplicate the first/last ring vertex, hard-normal caps duplicate their ring vertices, and each deliberately uncapped child branch contributes one hidden root loop. The passing contract is therefore zero **unexpected or visibly exposed** boundary loops after position-based seam classification, exactly one expected hidden root loop per child branch, zero non-manifold classified edges, and every expected root loop fully contained inside its parent. An audit failure must fail the bark build rather than merely log a warning.

#### Phase 2 — winding, normals, tangents, and closure

Correct `TreeBarkMeshGenerator.AppendBranchTube` and `AppendCap` so:

- all side faces are front-facing from outside under normal backface culling;
- all radial normals point outward;
- trunk-base cap faces outward/downward;
- branch-tip caps face outward along the terminal tangent;
- tangent handedness is consistent after winding changes;
- there are no exposed interiors at any camera angle;
- UV seams and duplicated hard-normal cap seams are position-coincident;
- only the classified, hidden embedded child-root loops remain open;
- there are no unexpected exposed boundary loops or degenerate cap triangles.

The validator should determine orientation geometrically using face-normal dot expected radial/cap direction. Do not “fix” this with `Cull Off`, inverted culling, or a two-sided bark material.

#### Phase 3 — branch-root junction construction

The current centerline-overlap approach must be replaced by a bounded deterministic junction treatment.

Minimum acceptable vertical-slice strategy:

1. Locate the parent curve sample/frame at the child attachment parameter.
2. Resolve the child launch direction in the parent frame.
3. Compute the parent surface intersection in that radial direction using the parent radius.
4. Start the child root slightly inside that parent surface, not at the parent centerline.
5. Build two or more transition rings over a configurable blend length.
6. Apply a root-radius scale and optional parent-conforming elliptical collar.
7. Keep the child root uncapped.
8. Prevent the child tube from crossing the parent and emerging from its opposite side.
9. Preserve stable IDs, UV continuity along the child, metadata, and deterministic fingerprints.

Initial reusable bark settings should include named fields equivalent to:

```text
Branch Root Inset
Branch Root Blend Length
Branch Root Radius Scale
Branch Root Collar Strength
Branch Root Transition Ring Count
```

A true boolean union or fully welded manifold branch junction is not required for this patch if the collar/embedded-root solution has no visible holes, spikes, opposite-side emergence, or severe shading discontinuity. Boolean/welded junctions remain an optional later quality tier.

#### Phase 4 — add the missing authoring controls

Add the authoritative fields to family profiles, recipe/ranged overrides, resolved parameters, fingerprints, reports, and tests:

```text
Trunk Spiral Strength
Trunk Spiral Turns / Frequency
Trunk Spiral Direction
Primary Branch Start Height
Primary Branch End Height
Initial Branch Elevation
Branch Arch Direction
Branch Arch Strength
Late Branch Sag
Azimuth Symmetry
Directional Bias Angle
Directional Bias Strength
```

Existing ambiguous `Droop`, `UpwardBias`, `SideBias`, or `TwistDegrees` fields may be migrated or retained internally for serialization, but the resolved public responsibilities must be unambiguous. No single control may simultaneously change launch angle, arch direction, arch strength, and sag.

Update dependency fingerprints and selective-regeneration tests:

- bark/palette changes preserve structure;
- branch arch changes rebuild branch curves but preserve trunk and palette;
- branch start-height changes rebuild branch layout but preserve trunk and palette;
- symmetry/bias changes rebuild branch layout but preserve trunk and palette;
- trunk spiral changes rebuild trunk and descendants but preserve palette;
- the same seed and recipe remain deterministic.

#### Phase 5 — family and comparison-recipe calibration

Upgrade the managed library version without resetting valid unrelated user-authored values.

Configure baseline comparison recipes so:

- Common 1 uses higher branch start, balanced azimuth, little/no spiral, and reference-like arch;
- Pine 1 uses central-leader tiers and reference-like branch arch direction;
- Twisted 1 receives enough centerline spiral/lateral displacement to read as twisted while remaining within imported H/W/D tolerance;
- Dead 1 receives reference-like branch start, balance, spiral, and colour while retaining recipe ranges that permit future lopsided variants.

The twenty comparison recipes should receive neutral bark tint and the same family material path as their imported counterparts. Creative tint ranges belong to future named recipes.

#### Phase 6 — reports and concise validation

The aggregate report must add:

```text
bark topology audit per representative
boundary / non-manifold / degenerate counts
outward / inward face counts
junction settings and root failures
resolved branch start-height range
resolved arch direction / strength / sag
resolved trunk spiral strength / turns
resolved symmetry / directional bias
imported and generated material/tint comparison
mesh fingerprint
```

### TREE-GEN.2B acceptance gate

The patch passes only when all of the following are true:

- Unity compiles with zero red errors;
- all twenty structural definitions still pass;
- structural repeatability remains 20/20;
- all four bark representatives reproduce the same geometry-content fingerprint on an immediate verification rebuild;
- managed profile/recipe migration preserves pre-existing unrelated authored values and reports upgraded fields;
- all four dependency suites pass;
- exactly four representative bark meshes build;
- zero non-finite vertices/normals/tangents/UVs/indices;
- zero degenerate triangles;
- raw boundary edges are fully explained by position-coincident UV/cap seams and the exact expected embedded child-root loops;
- zero unexpected exposed/open boundary loops after seam classification;
- zero inward-facing side/cap triangles according to the topology audit;
- exterior bark is visible and interiors are hidden from every ordinary view;
- trunk bottoms and branch tips are visibly closed;
- no branch tube emerges from the opposite side of its parent;
- branch roots no longer show the current stepped wedge/spike artifacts at normal inspection distances;
- generated comparison bark colour is as close as practical to the corresponding imported family material baseline;
- branch arch can be demonstrated upward and downward with independent strength;
- trunk centerline can be demonstrated straight and strongly spiraled;
- branch start can be demonstrated low and high;
- azimuth layout can be demonstrated symmetrical and deliberately one-sided;
- Common 1, Pine 1, Twisted 1, and Dead 1 remain inside the accepted imported H/W/D tolerance;
- Weather wind, cloud-cookie shading, shadows, and depth behavior remain functional.

Only after this gate passes should the bark path expand to all twenty slots. Foliage remains `TREE-GEN.3` work.


## TREE-GEN.2B first source-patch implementation record

### Patch status

The approved documentation-first source patch is implemented in code. It is **not frozen as live-accepted** until Unity imports and compiles the files, the complete comparison-gallery action runs, and the four generated representatives pass visual inspection against the acceptance gate above.

No scene, material, shader, texture, FBX, Ground, grass, Weather, cloud, package, layer, or tag asset is modified by this source patch. `TreeGenerationLibrary.asset` remains untouched in the delivered patch; its managed sub-assets migrate through the explicit Unity gallery/library rebuild action.

### Implemented source changes

- `TreeFamilyProfile`, `TreeGenerationRecipe`, and `TreeGenerationLibrary` advance their managed schema versions.
- Existing matching-family profiles migrate fieldwise. Legacy attachment endpoints remain the limits of editable start/end ranges, and existing managed comparison recipes receive exact endpoint overrides so their pre-patch branch interval is preserved. Launch/droop intent, torsion data, seeds, palette bindings, and unrelated authored values are preserved rather than replaced by a family reset.
- New primary-only controls live in `TreePrimaryBranchSettings`; secondary and tertiary settings do not expose irrelevant symmetry, directional-bias, branch-start, or arch controls.
- Recipe, calibration, and instance override layers migrate legacy launch/droop values before generation. `TreeGenerator` consumes resolved current-schema inputs without mutating authoring assets.
- `TrunkTwistDegrees` serialized data is retained through `FormerlySerializedAs` and now has the explicit responsibility `TrunkSurfaceTorsionDegrees`.
- Actual trunk centerline spiral strength, turn count, and handedness are applied to trunk control points before structural constraints and transported-frame construction.
- Primary branches and trunk forks resolve azimuth in the transported parent frame. Tiering, symmetry, bias, launch elevation, arch, and late sag are data-driven rather than selected by family-only branches.
- Child structural centerlines begin at a bounded embedded parent-surface intersection instead of the parent centerline.
- Reference width/depth fitting is followed by a parent-before-child re-anchor pass so calibration cannot invalidate branch-root placement.
- Bark side-wall winding is corrected for the established `Normal × Binormal = Tangent` convention. Existing geometrically correct cap winding is retained, and trunk-base tangent handedness is corrected separately.
- Child bark roots use configurable inset, blend length, radius scale, collar strength, and transition-ring count. Transported frames are rebuilt after render-root adjustment.
- The topology audit validates finite streams, indices, degenerates, ring collapse, side/cap orientation, tangent basis, raw boundaries, position-welded seams, non-manifold edges, opposite-side emergence, and parent containment.
- Closed position-welded boundary components are matched spatially to their specific child-root records. A passing mesh requires every expected embedded root loop to match once, no unclassified closed loop, and no open boundary component.
- Input/settings and emitted-geometry fingerprints are separate. Immediate temporary-mesh regeneration must reproduce geometry content, counts, and branch totals.
- A topology or repeatability failure clears the managed mesh and removes the stale generated scene child.
- The twenty managed comparison recipes receive exact white bark tint only when bark tint was inherited. Existing explicit bark-tint authoring is preserved and reported.
- Aggregate diagnostics now include migration counts, material/texture/tint identity, root-transition settings, classified topology results, and bark repeatability.

### First live validation and topology-weld hotfix

The first Unity rebuild compiled and passed source audit, gallery reconstruction, managed profile/recipe migration, and all twenty calibrated structural generations. `Common 1` then failed the bark topology gate with `27/27` expected embedded roots matched, `28` welded boundary loops, and exactly one unexpected loop. All orientation, finite-data, degeneracy, non-manifold, opposite-side-root, and parent-containment checks were zero.

The failure was a classifier defect rather than an approved relaxation: `BuildWeldedVertexIds` assigned a position to one rounded quantization cell only. Two vertices within the declared weld tolerance could therefore land in adjacent cells and remain falsely separate. The hotfix uses floor-based spatial buckets, searches all neighbouring cells, and reuses a welded ID only when the actual squared Euclidean distance is within `PositionWeldTolerance`. Expected-loop counts and the zero-unexpected-loop acceptance requirement remain unchanged. If a real unmatched loop remains, the audit now reports its welded vertex count, centroid, and average radius rather than returning only a total.

### Added source files

```text
Assets/Game/Procedural/Trees/TreeBarkMeshTopologyAudit.cs
Assets/Game/Procedural/Trees/TreeBarkMeshTopologyAudit.cs.meta
```

### Modified source files

```text
Assets/Game/Procedural/Trees/TreeBranchDefinition.cs
Assets/Game/Procedural/Trees/TreeFamilyProfile.cs
Assets/Game/Procedural/Trees/TreeGenerationOverrides.cs
Assets/Game/Procedural/Trees/TreeGenerationParameters.cs
Assets/Game/Procedural/Trees/TreeGenerationRecipe.cs
Assets/Game/Procedural/Trees/TreeGenerationLibrary.cs
Assets/Game/Procedural/Trees/TreeReferenceCalibrationPreset.cs
Assets/Game/Procedural/Trees/ProceduralTreeInstance.cs
Assets/Game/Procedural/Trees/TreeGenerator.cs
Assets/Game/Procedural/Trees/TreeBarkMeshSettings.cs
Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs
Assets/Game/Procedural/Trees/Editor/TreeGenerationLibraryBuilder.cs
Assets/Game/Procedural/Trees/Editor/TreeBarkMeshAssetBuilder.cs
Assets/Game/Procedural/Trees/Editor/TreeGalleryGenerationCoordinator.cs
```

### Source-only validation completed outside Unity

- changed-file whitespace/error-marker scan;
- balanced delimiter scan across the complete tree C# source folder;
- resolved-parameter and override property-reference scan;
- changed-file duplicate control-flow scan;
- critical method-call argument-count checks;
- analytical confirmation of side/cap winding under the transported-frame convention;
- simulated seam/cap/root-loop edge classification for capped and embedded-root tubes;
- exact changed-file inventory comparison against the supplied archive.

No Unity executable or C# compiler is available in the patch environment. Unity compilation, serialized migration execution, aggregate diagnostics, rendered topology, shadow/depth behavior, Weather wind, cloud-cookie reception, and visual reference matching remain explicitly pending.

## TREE-GEN.2B stale-module cleanup and Twisted tube correction

### Live evidence

The second Unity rebuild passed source audit, complete gallery reconstruction, all twenty calibrated structural generations, and the Common/Pine bark representatives. Common reported `27/27` embedded roots, zero unexpected loops, zero inward triangles, and deterministic bark geometry `072F6C3C167488C6`. Pine reported `32/32` embedded roots, zero unexpected loops, zero inward triangles, and deterministic bark geometry `E8E0CF194DEDD597`.

`Twisted 1` then failed with exactly four inward side triangles while all finite-data, index, degeneracy, cap, tangent, boundary-loop, non-manifold, opposite-side-root, and parent-containment checks remained zero. Dead was not attempted because the four-family action correctly stops on the first bark failure.

### Stale-module audit and removal decision

The standalone `ProceduralTreeAuthoring` path is now conclusively stale. It is referenced only by its own custom Inspector and `TreeAuthoringAssetFactory`; neither participates in the managed gallery/library/slot architecture. Its three starter assets duplicate the managed Common profile, palette, and recipe stored inside `Assets/Game/Demo/Profiles/Trees/TreeGenerationLibrary.asset`, and no scene, prefab, or other asset in the supplied project archive contains their serialized fields, names, or stable starter-recipe identity.

The cleanup removes:

```text
Assets/Game/Procedural/Trees/ProceduralTreeAuthoring.cs
Assets/Game/Procedural/Trees/Editor/ProceduralTreeAuthoringEditor.cs
Assets/Game/Procedural/Trees/Editor/TreeAuthoringAssetFactory.cs
Assets/Game/Procedural/Trees/Authoring/TFP_Common.asset
Assets/Game/Procedural/Trees/Authoring/TMP_Common.asset
Assets/Game/Procedural/Trees/Authoring/TR_Common_Starter.asset
```

Their same-named `.meta` files must be removed with them. The `Authoring` folder and its folder metadata may be removed when empty.

The following similarly named modules are **not** stale and remain authoritative:

- `ProceduralTreeInstance` and its Editor are the current per-slot generated-output owner and focused diagnostics UI;
- `TreeReferenceCalibrationPreset` remains required by all twenty managed comparison variants;
- `TreeFoliageClusterDefinition` and `TreeGenerationMetrics` are current structural outputs and future foliage inputs;
- all four tree HLSL includes are referenced by the bark or foliage shaders;
- `TreeGalleryGenerationCoordinator`, `TreeGenerationLibraryBuilder`, `TreeBarkMeshAssetBuilder`, the reference gallery modules, generator, profiles, recipes, palettes, definitions, and bark modules are active.

The duplicate `TreeBarkMeshBuildResult.MeshFingerprint` alias is removed; `GeometryFingerprint` is the sole bark geometry-content fingerprint.

### Twisted correction

The fixed diagonal used by an ordinary tube quad is correct for a straight or mildly curved transported tube. On strongly skewed Twisted transitions, however, the two topologically valid diagonals can produce materially different triangle orientation. Four triangles crossed the radial-normal orientation plane even though the underlying rings, frames, caps, and boundaries were valid.

Each generated tube quad now evaluates both outward-wound diagonals and deterministically selects the diagonal with the stronger worst-triangle agreement against the generated radial normals. This does not reverse individual triangles, alter ring vertices, weaken the topology gate, or suppress audit failures. If neither diagonal produces outward geometry, the unchanged zero-inward-triangle gate still fails.

The topology report also records branch stable ID, branch order, ring, radial side, triangle half, and signed orientation for every remaining inward triangle. This turns any further failure into an exact geometric location rather than another family-level count.

### Acceptance remains pending

The cleanup and diagonal correction are source-complete only. Unity must still prove zero compiler errors, four passing bark representatives, deterministic repeatability, no unexpected loops, and correct rendered junctions/wind/cloud/shadow behaviour before `TREE-GEN.2B` is accepted.


### Complete four-family failure reporting

The unified gallery action no longer stops at the first bark-representative failure. It attempts Common, Pine, Twisted, and Dead in deterministic family order, records every passing or failing bark report, then runs all four family dependency suites and returns one aggregate final status. This does not allow partial success to pass: `Generated bark meshes` must still be `4 / 4`, every topology audit must pass, and the final status remains `FAIL` when any representative or dependency suite fails. The change exists only to avoid losing Dead diagnostics when Twisted fails first.

## TREE-GEN.2B transported-ring and dependency-test correction

### Third live validation evidence

The complete rebuild again passed the source audit, complete 40-object gallery, all twenty calibrated structural definitions, and all twenty immediate structural repeatability checks. Common 1 and Pine 1 bark meshes passed every topology category. Twisted 1 retained four inward triangles on one order-2 branch at ring `14/22`; Dead 1 retained six inward triangles across two order-1 branches at ring `10/18`. In both families every finite-data, index, degenerate, cap, tangent, boundary-loop, embedded-root, non-manifold, opposite-side-root, and parent-containment check passed.

The exact failures clustered around adjacent radial sides, including the circumference seam. This proves the remaining problem is not global winding, caps, root classification, or an unclassified hole. The two neighbouring transported rings use a valid circular cross-section but their discrete circumference correspondence can rotate far enough that both diagonals of several same-index quads fold inward.

The Pine dependency suite also exposed a test-harness defect. Its `+1` primary-count candidate happened to add a deterministically rejected branch request, so the accepted branch graph and branch fingerprint correctly remained unchanged. A selective-regeneration test must search for a valid alternate resolved count that actually changes the accepted graph rather than assuming the nearest higher request always does.

### Render-ring correction

The bark mesher now assigns a deterministic integer circumference phase to every render ring. For each ring after the first it evaluates every cyclic correspondence against the previous emitted ring, scores both outward-wound quad diagonals, and selects the phase whose weakest resulting triangle has the strongest agreement with the authored radial normals. Geometry, normal, tangent, and unwrapped bark-U generation all use the selected phase, so the correction changes discrete ring correspondence rather than reversing individual triangles or hiding the audit failure.

A second safety layer bounds rendered radius at locally sharp centreline turns. It estimates a conservative local curvature radius, reduces only radii that exceed the safe fraction, and spreads that safety limit to neighbouring rings without allowing the natural small tip radius to collapse the whole branch. The report records phase-aligned ring count and curvature-radius clamp count. The zero-inward-triangle acceptance gate remains unchanged.

`TreeBarkMeshSettings.CurrentSettingsVersion` advances so the bark input fingerprint records the changed meshing algorithm. Geometry-content repeatability remains mandatory.

### Dependency-test correction

Primary-branch-count isolation now searches deterministic lower and higher candidates across the profile-approved range, preferring removal candidates because deterministic damage may reject an added request. It passes only when the resolved count differs, trunk and palette fingerprints remain unchanged, and the accepted branch fingerprint changes. The report now records baseline count and fingerprints together with the requested/resolved alternate count and resulting fingerprints.

### Stale-module follow-up

The previous stale-module removal remains correct. No additional tree module is currently safe to delete: every remaining runtime/editor C# module is referenced by the managed gallery/library/slot architecture or produces structural data reserved by the next approved foliage phase. The active `TFP_Common` and `TMP_Common` names inside `TreeGenerationLibrary.asset` are managed library sub-assets, not the removed standalone starter assets.

### Acceptance remains pending

Unity must still prove all four bark meshes pass with `inward=0`, all four dependency suites pass, repeatability remains deterministic, and visual bark/wind/cloud/shadow behaviour is correct before `TREE-GEN.2B` is accepted.

## TREE-GEN.2C trunk cross-section, buttress, and visible axial-twist patch

### Status

Approved for implementation after `TREE-GEN.2B` live validation passed all twenty structural definitions, all four bark representatives, deterministic repeatability, topology audits, and family dependency suites. Colour parity and the exterior-rendering defect are accepted. The remaining blockers are visual grammar defects: generated trunks begin as circular tubes without reference-like buttressed roots, and existing surface torsion is visually ineffective because a circular cross-section is rotationally invariant.

### Objective

Add a compact shared trunk-cross-section grammar that produces reference-like root buttresses and visible axial twist without expanding public authoring into many micro-controls. Preserve the existing centerline spiral controls as path-shape controls, reuse the existing surface-torsion value as the principal visible twist control, add no more than two twist-shape controls, and add no more than four root-buttress controls.

### Approved public control contract

#### Trunk path spiral — existing controls, renamed only for clarity

```text
Trunk Path Spiral Strength
Trunk Path Spiral Turns
Trunk Path Spiral Direction
```

These controls continue to move the trunk centerline through space. They do not represent surface twisting.

#### Visible trunk twist — existing control plus two new controls

```text
Trunk Twist Degrees          — existing serialized surface torsion; principal twist amount and signed direction
Trunk Twist Ridge Count      — new; number of non-circular longitudinal ridges
Trunk Twist Ridge Depth      — new; ridge/valley amplitude
```

No additional public twist controls are approved. The signed `Trunk Twist Degrees` value supplies direction; ridge phase, falloff, and local irregularity remain deterministic implementation details derived from existing seed/irregularity data.

#### Root buttress — three controls

```text
Root Buttress Strength
Root Buttress Height
Root Flare Scale
```

No separate buttress-count control is added. Root buttresses reuse `Trunk Twist Ridge Count`, so the root star/flute structure transitions continuously into the upper trunk rather than becoming an unrelated radial pattern. Root asymmetry is derived deterministically from existing trunk irregularity and seed data.

### Reviewed code evidence

- `Assets/Game/Procedural/Trees/TreeGenerator.cs` already resolves `TrunkSurfaceTorsionDegrees`, `TrunkSpiralStrength`, `TrunkSpiralTurns`, and `TrunkSpiralDirection`. `CreateTrunk` applies path spiral to centerline control points, while `BuildCurveSamples` applies surface torsion to transported frames.
- `Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs` currently emits every bark ring as `sample.Position + radial * sample.Radius`. The radius is scalar and angle-independent, so every trunk ring is circular and frame torsion cannot change visible geometry.
- `TreeBarkMeshGenerator.ResolveBestRingPhase` currently changes the emitted physical ring phase. That is harmless for circular rings but would rotate a non-circular authored profile. `TREE-GEN.2C` must prevent topology correspondence repair from cancelling authored trunk twist.
- `TreeBarkMeshGenerator` currently assigns radial normals. A fluted/lobed trunk requires normals and tangents derived from the actual cross-section derivative.
- `TreeBarkMeshSettings` owns radial segment budgets. Trunk resolution must increase automatically when ridge count requires it; no additional public authoring control is approved.
- `TreeFamilyProfile.UpgradeManagedDefaults` already performs fieldwise migration for matching families. `TREE-GEN.2C` must preserve that policy and initialize only the five newly introduced profile ranges. Existing `Trunk Twist Degrees` ranges are renamed in Inspector presentation only and remain numerically untouched.
- `TreeGenerator.BuildSeedSet` derives every subsystem seed from both `TreeFamilyProfile.ProfileVersion` and `TreeGenerator.CurrentGeneratorVersion`. Increasing either version for a bark-only schema patch would reseed trunk, branch, foliage-intent, damage, and material streams even when the authored structural inputs are unchanged.
- A deeper implementation review confirmed that `TrunkSurfaceTorsionDegrees` is not merely cosmetic in `TREE-GEN.2B`: the torsioned transported trunk frame is consumed by primary-branch and fork orientation. Removing torsion from structural frame construction would alter accepted branch graphs for unchanged recipes. The existing structural use is therefore retained, and the new non-circular profile is fixed in that already-torsioned frame rather than applying the angle a second time.

### Structural-seed preservation decision

`TREE-GEN.2C` is a bark-geometry extension, not a structural-generator revision. Therefore:

- `TreeGenerator.CurrentGeneratorVersion` remains `3`;
- `TreeFamilyProfile.CurrentProfileVersion` remains `4`;
- a hidden serialized bark-grammar migration version is added to `TreeFamilyProfile`;
- the hidden bark-grammar version initializes only the five new profile ranges, preserves the existing twist range exactly, and does not participate in structural seed derivation;
- `TreeGenerationLibrary` may advance its own library-schema version because that value is not part of tree seed derivation.

This keeps all existing structural seeds and unchanged structural fingerprints stable while still allowing explicit, one-time migration of the new bark authoring data.

### Invariants

- Existing branch graphs, trunk centerlines, foliage intent, palettes, Weather wind, cloud-cookie reception, and tree shaders remain unchanged by buttress/ridge-only edits.
- `Trunk Path Spiral` remains structurally owned and continues to invalidate the trunk and descendant branch graph.
- `Trunk Twist Ridge Count`, `Trunk Twist Ridge Depth`, `Root Buttress Strength`, `Root Buttress Height`, and `Root Flare Scale` are bark-geometry-only controls. Changing them must preserve trunk-centerline and branch fingerprints while invalidating the bark input/geometry fingerprint and bark bounds.
- `Trunk Twist Degrees` retains its existing structural-frame semantics: it rotates transported trunk frames and therefore may rotate branch attachment frames as it already did in `TREE-GEN.2B`. `TREE-GEN.2C` additionally makes that same rotation visible by applying a non-circular cross-section. Unchanged twist values must preserve the accepted `TREE-GEN.2B` branch graph; the patch must not silently reinterpret or remove the existing structural effect.
- The trunk base cap and trunk tip cap use the same resolved non-circular profile as the connected side ring.
- Branch tubes remain circular in this patch. Only branch order zero consumes the new cross-section grammar.
- The zero-inward-triangle, zero-unexpected-loop, finite-data, tangent-basis, and repeatability gates remain mandatory.
- No new component, scene object, material, shader, layer, tag, package, or raw serialized asset edit is approved.

### File scope

```text
Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md
Assets/Game/Procedural/Trees/TreeFamilyProfile.cs
Assets/Game/Procedural/Trees/TreeGenerationOverrides.cs
Assets/Game/Procedural/Trees/TreeGenerationParameters.cs
Assets/Game/Procedural/Trees/TreeGenerationRecipe.cs
Assets/Game/Procedural/Trees/TreeGenerationLibrary.cs
Assets/Game/Procedural/Trees/TreeGenerator.cs
Assets/Game/Procedural/Trees/TreeBarkMeshSettings.cs
Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs
Assets/Game/Procedural/Trees/Editor/TreeGenerationLibraryBuilder.cs
Assets/Game/Procedural/Trees/Editor/TreeBarkMeshAssetBuilder.cs
Assets/Game/Procedural/Trees/Editor/TreeGalleryGenerationCoordinator.cs
```

### Implementation sequence

| Step | Work | Status |
| --- | --- | --- |
| TREE-GEN.2C.1 | Record the compact control contract, reviewed evidence, dependencies, file scope, and acceptance gate in this canonical document before code edits. | Complete. |
| TREE-GEN.2C.2 | Add fieldwise hidden bark-grammar migration, resolved parameters, sparse overrides, ownership traces, validation, and family defaults for two ridge controls and three buttress controls. Preserve structural generator/profile seed versions and all existing twist values; rename Inspector presentation without breaking serialization. | Complete in source; Unity migration pending. |
| TREE-GEN.2C.3 | Generate a deterministic non-circular trunk profile. Existing transported-frame torsion rotates that profile to produce visible axial twist; root buttress and flare envelopes fade smoothly into the upper trunk. | Complete in source; visual acceptance pending. |
| TREE-GEN.2C.4 | Preserve authored physical trunk phase by disabling physical ring-phase reassignment for branch order zero; retain the existing correspondence repair for circular branch tubes. | Complete in source; topology validation pending. |
| TREE-GEN.2C.5 | Derive trunk normals/tangents from the actual circumference derivative and longitudinal tangent; make caps use matching profile geometry. | Complete in source; shader/render validation pending. |
| TREE-GEN.2C.6 | Resolve trunk radial segments automatically from ridge count, update bark fingerprints/reporting, and add dependency tests proving bark-only controls preserve structural fingerprints. | Complete in source; aggregate diagnostic execution pending. |
| TREE-GEN.2C.7 | Complete source consistency/compliance audit and package changed files. Unity compilation, generated-asset migration, topology, and visual reference comparison remain pending until live validation. | Source audit and artifact apply-test complete; Unity validation pending. |

### Family-default intent

- Common: moderate five-to-seven ridges, visible root buttress/flare, low twist degrees.
- Pine: restrained four-to-six ridges, shallow short buttress, near-zero twist by default.
- Twisted: strong five-to-eight ridges, persistent depth, strong signed twist, prominent buttress/flare.
- Dead: uneven four-to-seven ridges, medium/strong twist, prominent but less regular buttress/flare.

These remain profile defaults, not family-only generator branches. Any recipe may override the controls within approved profile ranges.

### Acceptance criteria

1. All twenty structures retain deterministic structural fingerprints for unchanged inputs; all four bark representatives build with `repeat=PASS`, `inward=0`, and `exposedLoops=0`.
2. Changing only ridge or buttress controls preserves trunk-centerline, branch, foliage-intent, and palette fingerprints while changing bark input/geometry fingerprints. Changing `Trunk Twist Degrees` changes bark input and retains its pre-existing deterministic structural-frame/branch-layout response.
3. Common, Twisted, and Dead generated bases visibly transition from lobed/buttressed roots into the trunk without an abrupt ring or ground-level circular cylinder.
4. Twisted and Dead display helical longitudinal ridges driven primarily by `Trunk Twist Degrees`; changing its sign reverses twist handedness.
5. `Trunk Path Spiral` continues to alter the centerline independently of visible surface twist.
6. Generated normals/tangents remain finite and coherent with the bark normal map; topology and cap audits remain clean.
7. The aggregate report records the five new resolved values, effective trunk radial segments, maximum cross-section multiplier, generated root width/depth, and bark-only dependency validation.


## TREE-GEN.2C source implementation record

### Implemented control surface

The public authoring surface remains compact:

```text
Existing visible-twist control:
- Trunk Twist Degrees

New twist-shape controls:
- Trunk Twist Ridge Count
- Trunk Twist Ridge Depth

New root-buttress controls:
- Root Buttress Strength
- Root Buttress Height
- Root Flare Scale
```

No additional twist, flute, phase, asymmetry, root-count, root-width, or twist-envelope control was introduced. Buttress count reuses ridge count; root asymmetry derives deterministically from existing trunk irregularity and phase.

### Implemented data and migration contract

- `TreeFamilyProfile` retains structural profile version `4` and adds hidden bark-grammar version `1`.
- `TreeGenerator` retains structural generator version `3`.
- Existing `Trunk Twist Degrees` ranges are unchanged; only Inspector presentation is renamed.
- The hidden migration initializes only the two new ridge ranges and three new root ranges.
- `TreeGenerationLibrary` advances to version `3`; `TreeBarkMeshSettings` advances to version `4`. Neither value participates in structural seed derivation.
- New fields propagate through family profile, sparse recipe/instance overrides, resolved parameters, ownership traces, validation, dependency reporting, and bark input fingerprints.

### Implemented bark geometry

- Branch order zero uses a deterministic star-shaped radial profile; higher-order branch tubes remain circular.
- The existing torsioned transported trunk frame rotates the non-circular profile, making `Trunk Twist Degrees` visible without applying the angle twice.
- Root flare and buttress envelopes are strongest at the base and fade smoothly over normalized trunk height.
- The physical ring-phase correction remains enabled for circular branch tubes and disabled for the authored non-circular trunk.
- Trunk circumference resolution resolves to at least three vertices per ridge, clamped to twenty-four segments.
- Trunk normals and tangents derive from numerical circumference and longitudinal surface derivatives; trunk caps use the same non-circular boundary profile.
- Curvature-radius safety accounts for the maximum expanded cross-section multiplier before emitting rings.
- Reports add effective trunk segments, resolved twist/ridge/root values, maximum cross-section multiplier, and generated root width/depth.

### Changed files

```text
Assets/Docs/Stylized_Nature_Tree_Integration_Handoff.md
Assets/Game/Procedural/Trees/TreeFamilyProfile.cs
Assets/Game/Procedural/Trees/TreeGenerationOverrides.cs
Assets/Game/Procedural/Trees/TreeGenerationParameters.cs
Assets/Game/Procedural/Trees/TreeGenerationLibrary.cs
Assets/Game/Procedural/Trees/TreeGenerator.cs
Assets/Game/Procedural/Trees/TreeBarkMeshSettings.cs
Assets/Game/Procedural/Trees/TreeBarkMeshGenerator.cs
Assets/Game/Procedural/Trees/Editor/TreeGenerationLibraryBuilder.cs
Assets/Game/Procedural/Trees/Editor/TreeBarkMeshAssetBuilder.cs
Assets/Game/Procedural/Trees/Editor/TreeGalleryGenerationCoordinator.cs
```

`TreeGenerationRecipe.cs` was reviewed but required no edit because its serialized `TreeGenerationOverrides` payload automatically carries the new sparse fields. No scene, prefab, material, shader, texture, FBX, Weather, cloud, Ground, vegetation, layer, tag, package, or project-setting file changed.

### Source consistency and compliance result

- Changed-file comparison against the accepted post-`TREE-GEN.2B` archive found exactly the eleven approved modified files above.
- Lexical delimiter validation passed across all twenty-seven tree C# files.
- Whitespace, final-newline, and merge-marker scans passed.
- `BuildSeedSet`, `CreateTrunk`, `CreateForkBranch`, and all four existing family configuration methods remain byte-equivalent to the accepted source; unchanged inputs therefore retain the existing structural seed and branch-generation algorithms.
- New-control propagation checks passed from family profile through overrides, resolved parameters, generator validation/reporting, and bark generation.
- Synthetic maximum-range profile checks retained strictly positive radii with the approved three-samples-per-ridge rule.
- The unified patch applies cleanly to a pristine copy of the accepted post-`TREE-GEN.2B` source baseline, and all eleven applied files byte-match the prepared `TREE-GEN.2C` source tree.
- Unity compilation, serialized migration execution, topology audit, generated visual comparison, Weather/cloud/shadow behavior, and Play Mode remain unverified and are mandatory before acceptance.
