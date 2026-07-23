# Stylized Nature Tree Integration Handoff

## Status

**Asset isolation is complete and validated. `TREE-GALLERY.1` is implemented at source level: the standalone gallery component/editor foundation, explicit Ground reference, sibling/root creation utility, passive specimen metadata, complete twenty-FBX/twelve-texture audit, and clipboard report are delivered. Unity compilation, live source-vault audit, scene creation, and Play Mode validation remain pending in the user's complete project. The procedural generated-tree library remains blocked until the imported gallery baseline is accepted.**

This document is the canonical plan, architecture ledger, implementation record, and continuation handoff for the Stylized Nature tree assets, the imported comparison gallery, and the generated tree library. Each later implementation patch still requires explicit approval and must follow the ordered gates recorded below.

## Objective

Preserve the implementation-relevant Stylized Nature tree assets, establish a complete imported reference gallery as the first implementation baseline, and define a production-suitable procedural tree library that can reproduce the Common, Pine, Twisted, and Dead visual families with substantially greater controlled variation while consuming the existing Weather wind and cloud-shadow contracts.

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

No procedural-tree implementation patch may begin before the imported gallery renders all twenty source trees correctly and the gallery baseline is frozen. This ordering is mandatory because the source trees are the visual calibration targets for scale, silhouette, crown density, material response, family identity, wind response, and camera readability.

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

Family rows are separated along local Z. Variant pair cells are separated along local X. Cell size is calculated from audited combined source bounds rather than a hard-coded spacing that may overlap large crowns. Inside each cell, the imported and procedural positions are offset symmetrically from the pair centre.

#### Ground alignment

The gallery builder must use the gallery's explicitly assigned `GeneratedGround` reference and call `GeneratedGround.TrySampleBaseSurface` for the pair centre. Both comparison roots are placed at the same sampled Ground height. Hierarchy proximity does not establish the sampling target.

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
Assets/Game/Procedural/Trees/TreeGenerationRecipe.cs
Assets/Game/Procedural/Trees/TreeDefinition.cs
Assets/Game/Procedural/Trees/TreeBranchDefinition.cs
Assets/Game/Procedural/Trees/TreeFoliageClusterDefinition.cs
Assets/Game/Procedural/Trees/TreeGenerationMetrics.cs
Assets/Game/Procedural/Trees/TreeGenerator.cs
Assets/Game/Procedural/Trees/TreeMeshBuilder.cs
Assets/Game/Procedural/Trees/TreeFoliageBuilder.cs
Assets/Game/Procedural/Trees/TreeLodBuilder.cs
Assets/Game/Procedural/Trees/TreeProxyBuilder.cs
Assets/Game/Procedural/Trees/ProceduralTreeAuthoring.cs
Assets/Game/Procedural/Trees/Editor/ProceduralTreeAuthoringEditor.cs
Assets/Game/Procedural/Trees/GeneratedTreeLibrary.cs
Assets/Game/Procedural/Trees/GeneratedTreeVariant.cs
Assets/Game/Procedural/Trees/Editor/GeneratedTreeLibraryBaker.cs
Assets/Game/Procedural/Trees/Editor/GeneratedTreeLibraryValidator.cs
```

The plan intentionally separates authoring/generation from runtime rendering. No base class shared with `VegetationRendererBase` is proposed.

#### Data flow

```text
TreeFamilyProfile + TreeGenerationRecipe + seed
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

#### `TreeFamilyProfile`

One profile defines family grammar and family response, not an individual tree.

Required categories:

- identity and reference-family mapping;
- overall height and crown aspect ranges;
- trunk taper, lean, curvature, twist, flare, and ring-resolution ranges;
- branch-order limits;
- branch attachment-height ranges;
- branch count, spacing, yaw distribution, elevation, length, taper, curvature, gravity bias, and wind bias;
- family-specific crown-density envelope;
- foliage eligibility by branch order and branch position;
- foliage-cluster shape, card count, card size, spread, orientation, density, and tint ranges;
- dead-branch and breakage probabilities;
- whole-tree, branch, and foliage wind response;
- bark and foliage surface references;
- LOD budgets;
- shadow/collision proxy settings;
- accepted world-footprint range.

Profiles must contain validation ranges and must reject impossible or zero-area configurations rather than relying on the mesh builder to recover silently.

#### `TreeGenerationRecipe`

A recipe defines one reproducible variant request:

- family profile;
- deterministic seed;
- age/size class;
- optional permanent lean direction and strength;
- optional damage state;
- optional foliage-retention state;
- optional locked trunk seed;
- optional locked branch seed;
- optional locked foliage seed;
- user-authored overrides inside profile-approved ranges.

Selective locking is required so the user can keep a successful trunk while regenerating branches or foliage. Each independent subsystem must use a derived deterministic random stream rather than consuming one shared random sequence whose order changes when unrelated code changes.

Required seed streams:

```text
trunk
primary branches
secondary branches
foliage clusters
foliage cards
surface variation
LOD selection
proxy generation
```

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

`ProceduralTreeAuthoring` is an Editor-facing specimen generator, not the production forest renderer.

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
| TREE-GALLERY.1 | Complete Unity source audit for all twenty FBXs and twelve textures; add tree family enum, audit code, standalone gallery component/editor shell, sibling/root creation utility, explicit Ground reference, and clipboard report. Do not create final materials or procedural code. | Exact submesh/material mapping, bounds, geometry counts, vertex-colour evidence, texture dimensions, and importer state are known. | Source implementation complete; Unity compile, live audit, and Play Mode validation pending. |
| TREE-GALLERY.2 | Correct the three bark normal imports; implement shared bark/foliage shaders and tree includes; consume Weather wind and the URP cloud cookie; add vertex/pivot debug modes. | One representative Common, Pine, Twisted, and Dead source tree renders with correct materials, cloud shading, and controlled wind. | Planned. |
| TREE-GALLERY.3 | Implement the complete deterministic gallery builder, four family rows, twenty source specimens, twenty procedural slots, Ground alignment, Scene labels, remove/rebuild actions, and complete audit report. | All twenty imported trees are available for side-by-side comparison in the actual demo environment. | Planned. |
| TREE-GALLERY.4 | Run source-scale, normalized-height, wind, cloud, Play Mode, material, pivot, and leak validation; record accepted reference screenshots/metrics and freeze the gallery baseline. | Imported reference baseline is accepted. Generator implementation is unblocked. | Planned. |
| TREE-GEN.1 | Add family profile, generation recipe, branch/foliage definition types, deterministic seed-stream contract, generator versioning, and structural diagnostics without generating production meshes. | Deterministic tree definitions can be regenerated and compared by hash. | Planned. |
| TREE-GEN.2 | Implement curve/frame transport and swept bark mesh construction using a Dead-family vertical slice; generate normals, tangents, bark UVs, metadata, bounds, and validation reports. | One generated leafless tree renders beside `DeadTree_1` and remains within LOD0 budgets. | Planned. |
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

No blocking design question remains for beginning `TREE-GALLERY.1` after the user approves implementation.

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

#### Unavailable validation and required next action

Unity 6000.5.0f1 and the local `Assets/References/Trees/` source vault are not present in the supplied archive, so C# compilation, `AssetDatabase` execution, imported-material identity validation, MeshData inspection, texture-alpha validation, editor menu execution, Undo/scene behavior, and Play Mode behavior are not represented as passed. The supplied archive also has no `.git` directory, so live status/history/diff validation remains pending.

The next action is to apply this patch to the complete Unity project, create the standalone gallery beside the intended Ground, run `Hierarchy > Tree Reference Gallery > Inspector > Diagnostics > Run Complete Tree Source Audit`, and provide the complete copied report. `TREE-GALLERY.2` remains blocked until the project compiles and that audit has no `FAIL` entries.
