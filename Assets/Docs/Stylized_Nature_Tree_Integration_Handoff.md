# Stylized Nature Tree Integration Handoff

## Status

**Complete. Tree assets are isolated, both requested documents are written, and the post-change source/filesystem audit passed. Unity import and runtime integration remain pending future-thread work.**

This document is the canonical plan and continuation handoff for isolating the tree subset of the local Stylized Nature reference pack and for a later tree-integration thread. It does not authorize tree runtime implementation.

## Objective

Preserve the implementation-relevant Stylized Nature tree assets in a dedicated local folder before the user removes the remaining reference pack, and record the exact current project contracts that the future tree-integration thread must respect.

## Acceptance criteria

- `Assets/References/Trees/` contains the twenty Unity-targeted tree FBX files.
- The dedicated folder contains the twelve required bark and foliage texture variants.
- Every moved FBX and texture retains its existing `.meta` file and GUID.
- The dedicated folder contains a separate copy of the pack's CC0 license.
- The original pack contains a Markdown summary covering trees, grass, plants, mushrooms, rocks, shader freedom, wind/weather compatibility, and procedural-generation limits.
- This handoff records the final dedicated-tree paths, the current vegetation and Weather contracts, integration risks, non-goals, and next actions.
- Generic FBX, OBJ, glTF, preview, non-tree, and unrelated texture files remain unchanged inside the original pack.
- No runtime source, shader, compute shader, scene, prefab, material, profile, layer, tag, package, or ProjectSettings file changes.
- Final validation proves the exact move set, source removal, retained metadata GUIDs, content hash preservation, absence of duplicate GUIDs, documentation paths, and final repository scope.

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
- Tree-family shaders own tree-specific trunk, branch, foliage, flutter, stiffness, snow, wetness, and seasonal response.
- Existing grass rendering, placement, coverage, interaction, trample, and benchmark contracts remain unchanged.
- Imported tree assets do not become `VegetationLayer` CrossedCards.
- No per-tree or per-vertex CPU wind simulation is introduced.
- No per-frame full-field rebuild is introduced.
- No new layer, tag, component, runtime dependency, renderer, material, prefab, or generated asset is introduced by this relocation.
- Existing FBX and texture GUIDs remain unchanged.
- Generic and non-tree pack content remains intact until the user removes it.

## Non-goals

- Import-setting corrections.
- Tree materials, shaders, prefabs, colliders, LODs, impostors, renderers, placement, or coverage.
- Tree interaction or trample behavior.
- Runtime or editor procedural tree generation.
- Weather producer changes.
- Generated Mass or rock-system work.
- Moving generic FBX, OBJ, glTF, previews, grass, plants, mushrooms, rocks, or unrelated textures.
- Modifying `.gitignore` to track `Assets/References/Trees/`.

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
