# Generated Mass Feature Implementation Checklist

Status: active concise checklist

## Non-negotiable active lighting acceptance criterion

- [ ] **Do not close the lighting defect from whole-object darkness, average luminance, F0/specular parity, ambient response, or exposure.**
- [ ] Under one controlled light direction, verify that each source face responds coherently to its own orientation relative to the light.
- [ ] For every sampled ordinary bevel, verify parent–bevel–parent response ordering against the bevel and parent surface orientations; a geometrically intermediate bevel must not randomly become darker than both parents or brighter than both parents.
- [ ] Use the legacy material behavior as the visual reference for coherent bright-to-dark progression as surfaces rotate relative to the same light.
- [ ] Any candidate fix that improves global brightness while leaving orientation-order inversions is a failed fix for this defect.

## Completed and frozen

- [x] Base Generated Mass deterministic construction.
- [x] Ordinary bevel candidate generation and bounded construction.
- [x] Corner-chip transaction geometry.
- [x] Post-chip ordinary candidate discovery.
- [x] Ranked one-loser-per-retry incompatibility reduction.
- [x] Retirement of combinatorial conflict-frontier search.
- [x] Focused pathological regression suite.
- [x] Final full incremental-selection suite pass.
- [x] GM-SURFACE.1 documentation consolidation.
- [x] Explicit `BaseGeometryOnly` / `ProductionSurfaceFeatures` routing boundary.

## GM-SURFACE.2 — production geometry promotion

- [x] Share the certified construction path between production and diagnostic callers.
- [x] Implement player-safe `ProductionSurfaceFeatures` behavior.
- [x] Route enabled ordinary generation through certified chips/bevels.
- [x] Preserve cheap deterministic base-only output when disabled.
- [x] Preserve certified chips with zero bevels.
- [x] Preserve deterministic base fallback.
- [x] Increment the production-generation contract through existing invalidation ownership.
- [ ] Confirm in Unity that existing scene rocks regenerate without scene edits.

## GM-SURFACE.3 — semantic contributions

- [x] Define semantic feature type and response role.
- [x] Emit `ConvexBoundary` and `CornerChipCap` generation-time contributions.
- [x] Reserve `ConcaveBoundary`, `Fracture`, `ImpactDent`, and `MaterialSeam` semantics.
- [x] Resolve deterministic primary and secondary contributions.
- [x] Define deterministic overflow/overlap policy.
- [x] Keep contribution storage generation-local in `TriangleSoup`; no persistent feature database or runtime list was introduced.

## GM-SURFACE.4 — packed mesh channels

- [x] Audit colour and UV stream ownership.
- [x] Preserve existing colour and UV2 material masks.
- [x] Avoid `TEXCOORD3`, which remains occupied by the retired diagnostic feature-atlas path.
- [x] Store primary/secondary type and strength in one `TEXCOORD4` `Vector4`.
- [x] Hold the production payload to exactly 16 bytes per final render vertex.
- [x] Derive role and current direction instead of persisting them.
- [x] Add finite/range/type/disabled-slot validation before mesh upload.
- [x] Validate the channel again after Unity mesh upload.
- [x] Include the stream in exact deterministic mesh-data comparisons.
- [ ] Confirm in Unity that base, bevel, and chip meshes expose complete `TEXCOORD4` data.

## GM-SURFACE.5 — whole-rock normals

- [x] Add shared textureless whole-rock normal response.
- [x] GM-SURFACE.5A: make strength true slope amplitude and suppress legacy broad masks on generated bevel/chip surfaces.
- [ ] GM-SURFACE.6: consume semantic stream for explicit convex accents and exposed-chip response.
- [ ] Add optional shared micro detail.
- [ ] Add deterministic per-rock transform/seed variation without unique materials.
- [ ] Provide low and standard cost tiers.
- [ ] Use correct normal blending.

## GM-SURFACE.6 — first structural responses

- [ ] Implement convex-boundary response.
- [ ] Implement corner-chip-cap response.
- [ ] Evaluate at most two structural slots.
- [ ] Confirm one module can be disabled without affecting the other or whole-rock detail.
- [ ] Confirm cost does not scale with total source feature count.

## GM-SURFACE.7 — concave creases

- [ ] Define geometry-versus-surface threshold.
- [ ] Detect or author concave boundaries.
- [ ] Compile into existing contribution slots.
- [ ] Implement concave normal/darkening/roughness response.
- [ ] Avoid new streams unless the existing contract is proven insufficient.

## Permanently rejected

- [x] Per-rock feature atlases.
- [x] Generated per-rock normal maps.
- [x] Permanent array slice per rock.
- [x] Unbounded per-pixel feature loops.
- [x] One renderer/material pass per feature.
- [x] Edge-wear-only normal architecture.
- [x] Persistent per-rock feature database.

## Historical surface-lighting work guard

The 5A–5O checklist contains valid historical fixes and diagnostic experiments. Do not infer the active root cause from their names. The current unresolved acceptance criterion is GM-SURFACE.5P surface-orientation response ordering: neighboring source faces and bevels must brighten/darken coherently with their orientation to the same light.

## GM-SURFACE.5B — completed pending Unity validation

- [x] Preserve faint whole-rock normal readability outside direct sunlight.
- [x] Keep the shadow-side response multiplicative and non-emissive.
- [x] Apply a deterministic rounded-normal proxy only to convex contributions.
- [x] Keep source faces planar and corner-chip caps faceted.
- [x] Add a minimal light-dependent convex lift without a new control row.
- [x] Add no texture, mesh stream, draw call or variable feature loop.
- [ ] Confirm the marked vertical bevel no longer reads as one uniform dark band.
- [ ] Confirm `Normal Strength = 0` remains exact visual parity.
- [ ] Confirm shaded response is visible but does not flatten form or glow.


## GM-SURFACE.5C — Generated-face material-mask inheritance

The dark-band bevel defect was confirmed to be a pre-light material-mask ownership defect, not a mesh-normal defect. Generated bevel and chip faces must not recompute Exposure, Crevice/Base, or DirtDeposit from their own new face normal, and the shader must not suppress those masks as a compensation. During final mesh emission, generated triangles now inherit the source-face material-mask samples present on their shared boundaries. Shader-side convex/chip mask suppression and the GM-SURFACE.5B convex lighting compensation are retired. Whole-rock normal response remains independent.

## GM-SURFACE.5D — completed generated-face material-mask correction

- [x] Remove triangle-wide averaging of all coincident source samples.
- [x] Resolve inherited masks per generated boundary vertex.
- [x] Interpolate unresolved interior generated vertices from their own resolved triangle boundary.
- [x] Make Generated Mass rendering consume compiled `Color.g`, `Color.b`, and `UV2.y` masks.
- [x] Stop recomputing Generated Mass crevice/base and dirt from generated-face orientation.
- [x] Preserve whole-rock normal response as an independent layer.
- [ ] Validate the previously bright lower bevel and darker upper bevel under a rotating directional light.

## GM-SURFACE.5E — raw lighting parity

- [x] Disable production bevel albedo lift and tint without deleting serialized authoring values.
- [x] Bypass normal-dependent pre-PBR value shaping for Generated Mass fragments.
- [x] Return raw `UniversalFragmentPBR` output for Generated Mass fragments.
- [x] Quarantine post-PBR light-colour reconstruction and shadow-side normal readability.
- [ ] Confirm bevel response tracks light direction comparably to `M_PixelStone` / `SG_PixelSurfaceLit` with whole-rock normal strength zero.
- [ ] Reintroduce stylization only after parity is visually confirmed.

## GM-SURFACE.5G — Logical-bevel material-mask continuity

- [x] Reconcile duplicated ordinary-bevel vertices by provenance and position.
- [x] Restrict production writes to `Color.g`, `Color.b`, and `UV2.y`.
- [x] Preserve source-face masks.
- [x] Add pre/post immutable-channel fingerprints during the bevel-shading diagnostic run.
- [x] Add pre/post degenerate-triangle identity parity and exclude matched degenerates from angular mismatch decisions.
- [x] Tighten logical-bevel shared-edge mask continuity threshold to `0.00001`.
- [x] Validate the complete evidence suite on two distinct accepted production meshes with exact geometry parity and zero internal mask jumps.
- [ ] Preserve visual acceptance as a scene-level check when the affected material or lighting baseline changes.

## GM-SURFACE.5G-H1 — Validation closure and diagnostic cleanup

- [x] Record two-rock validation evidence in the canonical architecture.
- [x] Replace the generic clean verdict with `LOGICAL_BEVEL_MASK_CONTINUITY_VALIDATED`.
- [x] Keep successful reports compact.
- [x] Retain complete per-bevel evidence for every failed invariant.
- [x] Preserve all permanent geometry, mapping, mask, normal, degenerate, and upload regression checks.
- [x] Confirm no separate obsolete DIAG1 implementation file remains.
- [x] Validate Unity compilation and compact C0 mask-continuity reporting; visual closure subsequently failed and is superseded by GM-SURFACE.5J.

## GM-SURFACE.5G-H2 — Comprehensive residual bevel-shading audit

- [x] Preserve accepted-build capture and geometry regression guards.
- [x] Add piecewise-linear gradient analysis for surface variation, exposure, crevice, dirt and structural channels.
- [x] Add triangle area/aspect, geometric-facet, render-normal, tangent and parent-envelope checks.
- [x] Add nominal/high direct-response sensitivity evidence and material-property inventory.
- [x] Rank per-bevel and per-edge evidence in one cancellable suite.
- [ ] Compile in Unity 6000.5.0f1.
- [x] Run on two visually failing rocks under the low-light condition and review the complete reports.
- [x] Escalate to the integrated frozen-mesh shader/material causality tournament because CPU-only evidence did not establish ownership.

## GM-SURFACE.5J — Integrated surface-causality reset

- [x] Reject and roll back GM-SURFACE.5I/5I-H1 production retriangulation.
- [x] Restore pre-5I triangulation source byte-for-byte while retaining GM-SURFACE.5G mask reconciliation.
- [x] Add one double-precision final-triangle classifier shared by capture, uploaded-mesh accounting, logical-bevel analysis and audit-only candidates.
- [x] Separate structural invalidity, numerical under-resolution and extreme-sliver conditioning.
- [x] Exclude unconditioned triangles from differential normal/gradient conclusions without fabricating 180-degree jumps.
- [x] Add validated audit-only fan/ear triangulation comparison with polygon coverage and noncrossing-diagonal checks.
- [x] Add frozen-mesh suspect/reference material-asset and full-renderer-state parity matrix.
- [x] Include renderer `MaterialPropertyBlock`, shadow, rendering-layer and probe state in full-state swaps.
- [x] Add pre-light, direct, ambient, stored-normal, generated-normal and triangle-normal isolation modes.
- [x] Add ablations for generated normal, fog/post, masks, procedural variation, atlases, profile effects, specular, shadows and additional lights.
- [x] Keep diagnostic shader work behind `_SURFACE_CAUSALITY_AUDIT`; production shader variant remains unchanged.
- [x] Add front-facing screen-space internal-edge derivative scoring through `AsyncGPUReadback`.
- [x] Rank only true ablations as possible dominant contributors.
- [x] Preserve incremental execution, visible progress/ETA, cancellation and checkpoint reports.
- [x] Resume the interrupted build from the recovered work tree rather than restart from an older baseline.
- [x] Replace the compile-time `MarkSceneClean` dependency with compile-safe best-effort clean-state restoration and explicit unresolved-restoration reporting.
- [x] Suppress the original suspect/reference renderers in every frozen-mesh render case so their shadow/reflection participation cannot contaminate parity results.
- [x] Prevent numerically under-resolved triangles from being reclassified as structurally winding-invalid before their orientation is reliable.
- [x] Normalize every temporary URP audit camera to a standalone Base camera and clear copied camera-stack state through reflection.
- [x] Reject an invalid suspect or reference geometry-filter selection before capture begins.
- [x] Complete final source audit: exact eleven-file scope, pre-5I triangulation byte parity, ordinary forward-path token parity, shader CBUFFER/keyword consistency, property-reference coverage, and static delimiter/preprocessor checks.
- [ ] Compile in Unity 6000.5.0f1 with no new errors or warnings.
- [ ] Run the two-object suspect/reference tournament under the known defective low-light setup.
- [ ] Submit the complete report and compare the named causal owner with the visual result.
- [ ] Select the next production correction only after the integrated evidence is reviewed.

## GM-SURFACE.5J-H1 — Compile integration correction

Status: source correction complete; full reconstructed-project static integration audit passed `41 / 41`; Unity 6000.5.0f1 compilation pending.

### Objective

Remove the `MassGenerator.IsFinite(Vector3)` / `IsFinite(float)` duplicate-member collision introduced by `MassGenerator.TriangleQuality.cs`, then run a full cross-partial declaration audit over the reconstructed current Generated Mass source rather than validating the changed file in isolation.

### Acceptance criteria

- [x] `MassGenerator.TriangleQuality.cs` declares no helper signature already present in any other `MassGenerator` partial file.
- [x] Every call inside the canonical triangle-quality kernel resolves to the renamed local helper.
- [x] No exact duplicate method signature exists across the complete reconstructed `MassGenerator` partial surface.
- [x] All changed C# files pass lexical delimiter and preprocessor checks.
- [x] All added public types/methods and direct callers remain present with matching arity.
- [x] Production triangulation files remain byte-identical to the trusted pre-5I baseline.
- [x] Ordinary non-audit shader path and approved eleven-file package scope remain unchanged.
- [ ] Unity 6000.5.0f1 compilation is rerun by the user; no claim of Unity compilation is made offline.

### Approved files

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Game/Procedural/Masses/MassGenerator.TriangleQuality.cs`

The corrected delivery may include the unchanged remaining GM-SURFACE.5J files so it can replace the rejected package atomically. No scene, prefab, material asset, serialized profile, layer, tag, production triangulation, mask compiler, normal/tangent policy, or ordinary shader behavior may change.

### Reviewed evidence

- Unity compiler: `CS0111` at `MassGenerator.TriangleQuality.cs(365,29)` for duplicate `IsFinite(Vector3)`.
- Existing colliding declaration: `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Graph.cs::IsFinite(Vector3)` in the same partial `MassGenerator` type. The new scalar helper was renamed at the same time to keep this subsystem private namespace collision-resistant.
- Complete `MassGenerator.TriangleQuality.cs` and all direct callers in `MassGenerator.MeshOutput.cs`, `MassGenerator.BevelShadingDiagnosticCapture.cs`, and `Editor/GeneratedMassBevelShadingDiagnosticSuite.cs`.
- Complete reconstructed current Generated Mass source under the recovered GM-SURFACE.5J work tree.

### Implementation sequence

1. Rename the triangle-quality-private finite helpers with a subsystem-specific prefix and update every internal call.
2. Scan exact method signatures across all `MassGenerator*.cs` partial files, not only the patch files.
3. Re-run static syntax, reference, scope, production-byte-parity, and shader-path-parity audits.
4. Package a corrected atomic GM-SURFACE.5J delivery and record hashes/results.

### Risks and invariants

- Renaming private helpers must not alter triangle classification results.
- Public `EvaluateFinalTriangleQuality` API and all report contracts remain unchanged.
- No unrelated cleanup or generic helper consolidation is permitted in this correction.

### Completed validation evidence

- The triangle-quality-private helpers are now `IsTriangleQualityFinite(Vector3)` and `IsTriangleQualityFinite(float)`; all eight declarations/calls use the scoped name.
- Token-equivalence after canonicalizing the helper identifier proves that the correction changes no triangle-classification behavior.
- Complete `Assets` cross-partial scan: `280` C# files, `6150` method declarations, `0` duplicate exact signatures.
- Generated Mass subtree scan: `45` C# files, `1801` method declarations, `0` duplicate exact signatures.
- Static source audit: `41 / 41` passed, including direct caller/type checks, lexical and preprocessor balance, production triangulation byte parity, unchanged unrelated 5J source, shader keyword/CBUFFER integration, and forbidden-marker checks.
- Offline environment has no Unity compiler; Unity compilation remains explicitly pending.

## GM-SURFACE.5J-H2 — Unity 6.5 API-warning cleanup

- [x] Replace all deprecated `FindObjectsByType<T>(FindObjectsInactive, FindObjectsSortMode.None)` calls with the unsorted Unity 6.5 overload.
- [x] Replace deprecated `ShaderUtil` property enumeration with `Shader.GetPropertyCount`, `Shader.GetPropertyName`, and `Shader.GetPropertyType`.
- [x] Preserve inactive-object inclusion and all render-audit behavior.
- [x] Record the completed single-subject evidence and its missing-reference limitation.
- [ ] Recompile in Unity 6000.5.0f1 with zero new errors or warnings.
- [ ] Rerun with exactly two Generated Mass root objects selected; verify the button label includes `Suspect + Reference` before starting.
- [ ] Review the full parity ownership result before selecting any production shading correction.

## GM-SURFACE.5K — Same-mesh legacy lighting decomposition

- [x] Record the named Stone Surface Profile material-reapplication path that explains debug-view material replacement.
- [x] Snapshot complete source `sharedMaterials`, global property block, and per-material-index property blocks before diagnostic regeneration.
- [x] Restore and verify source renderer material state immediately after capture and again at finalization, cancellation, or exception exit.
- [x] Load `Assets/Game/Demo/Materials/Stone/M_PixelStone.mat` automatically as the same-mesh legacy control.
- [x] Add current/legacy original-property-block and cleared-property-block parity cases without assigning diagnostic materials to the source renderer.
- [x] Partition final triangles into source-face, ordinary-bevel, junction/cap, corner-damage, and unclassified masks using accepted-build provenance.
- [x] Add per-class luminance distributions and class-to-whole response for every render case.
- [x] Add captured bevel-parent triplets and legacy ordering/envelope comparisons.
- [x] Add matched current/legacy main-direct and indirect environment cases.
- [x] Add audit-only raw-base, pixel-variation, exposure-scale, mottle, generated-layer, and final-prelight checkpoints; modes `20–24` return before overall tint/PBR.
- [x] Add area-weighted stored-normal main-light prediction, observed direct-to-prelight source/bevel response, and minimum visible-class sample guards.
- [x] Add individual pixel-cell, vertex, broad, and warp ablations while retaining the existing complete tournament.
- [x] Keep ordinary production HLSL token-equivalent after removing audit-only blocks.
- [x] Complete pre-package static integration, scope, API, declaration-collision, shader production-parity, and material-invariance audit (`106 / 106`).
- [x] Complete package and re-extraction byte/hash parity audit.
- [x] Compile in Unity 6000.5.0f1 after GM-SURFACE.5K-H1 corrected the two unresolved formatter calls.
- [x] Run the same-mesh legacy/HLSL tournament under the defective lighting: `71` cases, `384×384`, `68.60918` seconds, renderer material/property-block restoration verified.
- [x] Reject the zero-confidence first-match terminal label as insufficient for a production correction; preserve the bidirectional bright/dark user evidence.
- [ ] Select a production shader correction only after signed multi-direction BRDF ownership is identified.

## GM-SURFACE.5K-H1 — Environment-report formatter compile correction

- [x] Replace the two unresolved `F(...)` calls in `GeneratedMassSurfaceCausalityRenderAudit.BuildEnvironmentReport()` with the local `Format(float)` helper.
- [x] Confirm no undeclared single-letter invocation remains in the complete render-audit class.
- [x] Complete final diff, declaration, delimiter, obsolete-API, scope, and archive re-extraction integrity checks (`16 / 16`).
- [x] Compile in Unity 6000.5.0f1 with no errors or new warnings after the formatter correction.


## GM-SURFACE.5L-DIAG — Bidirectional BRDF workflow parity sweep

- [x] Record the canonical plan before code modification.
- [x] Replace isolated class-only mask cases with one full-mesh, depth-tested, checksummed 16-bit triangle-identity pass per subject.
- [x] Fail closed on missing, empty, corrupted, or out-of-range triangle identity.
- [x] Add eight deterministic object-relative directional-light Stage A groups: legacy metallic-zero, HLSL F0 `0.16`, and HLSL F0 `0.04`.
- [x] Isolate Stage A with constant neutral albedo, stored normals, one white directional light, no shadows, no additional lights, black ambient/GI, no reflections, no fog, and no post-processing.
- [x] Queue Stage B only after Stage A, selecting the two largest current-HLSL residual directions.
- [x] Add Stage B actual-material legacy/current/F0-0.04 cases while preserving actual normal and smoothness state.
- [x] Add a legacy `_SPECULARHIGHLIGHTS_OFF` case and an HLSL zero-specular, `0.96` diffuse-energy-normalized case.
- [x] Report signed per-triangle over/under response, per-direction P90/mean residuals, exact visible parent–bevel–parent ordering, and adaptive actual/diffuse evidence.
- [x] Require complete Stage A and Stage B evidence for a terminal F0 confirmation; incomplete evidence is inconclusive.
- [x] Keep the shared HLSL change under `_SURFACE_CAUSALITY_AUDIT` mode `25`; production token stream remains unchanged.
- [x] Complete final six-file static/scope audit (`38 / 38`) and changed-file-only archive re-extraction byte/hash parity.
- [ ] Compile in Unity 6000.5.0f1 with no errors or new warnings.
- [ ] Run the existing Inspector action and submit the complete `Library/GeneratedMassSurfaceCausalityAudit.txt` report.
- [ ] Select or reject the F0 `0.04` production correction only after the report and screenshots agree.

## GM-SURFACE.5M-DIAG — Exhaustive per-triangle bidirectional shader-response capture

- [x] Record canonical objective, scope, invariants, and fail-closed contracts before code edits.
- [x] Replace interpolated/checksummed identity with audit-only non-interpolated 24-bit identity.
- [x] Stop the tournament immediately on identity failure.
- [x] Expand deterministic light basis to 27 directions including the current scene-main-light direction.
- [x] Preserve signed per-triangle and parent/bevel ordering evidence for legacy, HLSL F0 0.16, and HLSL F0 0.04.
- [x] Complete whole-class/static scope and shader-production-isolation audit.
- [ ] Unity 6000.5.0f1 compilation.
- [ ] Unity runtime report with every fail-closed contract passing.

## GM-SURFACE.5N-DIAG — Dedicated identity and complete lighting ownership capture

- [x] Record the canonical plan before implementation.
- [x] Replace shared-forward identity transport with a dedicated editor-only depth-tested identity shader.
- [x] Remove the failed identity varying and mode from the shared production includes.
- [x] Retain all in-flight render resources through asynchronous readback completion.
- [x] Use nonzero base-255 RGB24 identity with CPU and GPU fail-closed contracts.
- [x] Use linear floating-point render/readback for all lighting cases and reject non-finite data.
- [x] Implement 27-direction Stage A, four-direction Stage B, two-direction/two-view Stage C, and six-case Stage D.
- [x] Require at least twelve evaluable Stage A directions, with a visible-population-aware two-to-eight predicted-lit triangle requirement per class.
- [x] Emit complete per-triangle RGB/BRDF/normal/order evidence to the text report and CSV.
- [x] Preserve production geometry, materials, shader defaults, scenes, prefabs, profiles, layers, tags, and Inspector controls.
- [x] Complete offline source/scope/contract audit (`84 / 84`) and exhaustive nonzero base-255 identity round-trip verification (`16,581,375 / 16,581,375`).
- [x] Complete exact nine-file scope audit and changed-file-only archive re-extraction byte parity.
- [ ] Unity 6000.5.0f1 compilation with no new errors or warnings.
- [ ] Runtime completion: 190 counted cases, two auxiliary identity passes, zero readback/non-finite/identity failures, complete source-renderer restoration.
- [ ] Review the mechanical ownership verdict before authoring any production correction.


## GM-SURFACE.5N-H1 — Floating-point finite-check compile correction

- [x] Record the Unity `CS0103` failure and compile-only correction in the canonical plan.
- [x] Replace all four unresolved `IsFinite(float)` calls in `CountNonFinitePixels(Color[])` with explicit NaN/infinity checks.
- [x] Verify no unresolved `IsFinite` call remains in the complete class.
- [x] Verify the 5N identity, case matrix, report, CSV, production geometry, materials, and shaders are otherwise unchanged.
- [x] Run offline whole-class structural, symbol, scope, and package-integrity checks.
- [ ] Compile in Unity 6000.5.0f1.
- [ ] Run the complete 5N ownership suite and review both report outputs.


## GM-SURFACE.5N-H2 — Keyword-free decomposition and readback alignment

- [x] Record the 5N-H1 runtime evidence: dedicated identity valid with 41,981 pixels, zero invalid IDs, and 92 visible triangles; unsupported legacy specular keyword caused the terminal failure.
- [x] Remove all `_SPECULARHIGHLIGHTS_OFF` mutation, validation, report, completion, and CSV dependencies from the diagnostic.
- [x] Replace Stage A diffuse cases with legacy/HLSL black-albedo specular-only captures and derive diffuse per triangle as full minus specular-only.
- [x] Reduce Stage B to five actual-material generated/stored-normal cases per selected direction.
- [x] Add a controlled current-view Lambert preflight before Stage A queueing.
- [x] Add per-lighting-case identity/readback orientation resolution with foreground IoU and pixel-count-difference contracts.
- [x] Reconcile the matrix to 209 decision cases, three identity passes, and 212 total render passes.
- [x] Update text and CSV reporting to the keyword-free variants, alignment evidence, and Lambert evidence.
- [x] Preserve dedicated identity rendering, floating-point lighting capture, cancellation, checkpointing, source-renderer restoration, and zero production asset mutation.
- [x] Complete exact five-file scope and offline structural/contract audit (`85 / 85`); dedicated identity shader and shared production includes remain byte-identical to 5N-H1.
- [ ] Compile in Unity 6000.5.0f1 with no new errors or warnings.
- [ ] Run the complete suite and require 209/209 decision cases, three valid identity passes, 212 total renders, valid Lambert preflight, alignment contracts passing, zero readback/non-finite failures, and complete renderer-state restoration.
- [ ] Review the mechanical ownership verdict before authoring a production visual correction.

## GM-SURFACE.5N-H3 — Pixelwise GPU-normal Lambert preflight correction

- [x] Record the 5N-H2 runtime evidence: identity valid with 41,494 pixels, zero invalid IDs, 87 visible triangles, 0.9999759 identity/light IoU, and non-empty finite Lambert response.
- [x] Remove CPU averaged triangle normals from Lambert validity.
- [x] Add one auxiliary current-view stored-normal capture using audit mode 14.
- [x] Compare the stored GPU normal and mode-12 Lambert response pixel-by-pixel through each capture's independently resolved identity orientation.
- [x] Report configured-direction RMSE, opposite-direction RMSE, best-fit scalar/RMSE, valid normal pixels, positive expected pixels, positive observed pixels, and mean foreground luminance.
- [x] Require at least 20,000 valid normal pixels, 2,000 positive expected pixels, 2,000 positive observed pixels, mean foreground luminance at least 0.02, and configured normalized RMSE at most 0.01.
- [x] Preserve all 209 Stage A/B/C/D decision cases; update to four auxiliary validation passes and 213 total render/readback passes.
- [x] Complete final scope, symbol, matrix, unchanged-production, and package-integrity audit.
- [ ] Compile in Unity 6000.5.0f1 with no new errors or warnings.
- [ ] Run the complete suite and require the pixelwise Lambert contract plus all existing ownership-matrix contracts before selecting a production visual correction.


## GM-SURFACE.5O — Cold-grey production lighting parity trial — rejected as root-cause fix

- [x] Record the production-trial plan before implementation.
- [x] Change the cold-grey material dielectric F0 from 0.16 to 0.04.
- [x] Bypass generated whole-surface normal perturbation only for the ColdGreyStone profile while preserving the authored control for other profiles.
- [x] Preserve shared shaders, geometry, triangulation, scenes, prefabs, recipes, layers, tags, and the completed 5N-H3 diagnostic matrix.
- [x] Visual validation supplied on 2026-08-07: the new HLSL material still exhibits wrong source-face and bevel brightness ordering relative to surface orientation, while the legacy material exhibits coherent orientation-driven lighting.
- [x] Reject F0/specular parity, whole-rock darkness, and the 5O generated-normal bypass as sufficient explanations or closure criteria for the active defect.
- [ ] Roll back or supersede the 5O behavioral trial only in a separately approved production patch; 5P changes documentation/comments only.
- [ ] Continue by tracing **per-fragment orientation-to-light response and parent–bevel–parent ordering**, not by tuning global brightness.

## GM-SURFACE.5P — Surface-orientation defect-definition freeze

- [x] Freeze the canonical problem as per-surface/per-bevel orientation-driven lighting response incoherence.
- [x] State explicitly that whole-object darkness and specular magnitude are not the defect definition.
- [x] Mark 5O visually insufficient as a root-cause correction.
- [x] Add matching comments at production normal publication, normal perturbation, forward-lighting, shader entry, bevel provenance, parent-envelope analysis, and report interpretation boundaries.
- [x] Preserve all executable rendering math, serialized values, diagnostic thresholds, geometry, scenes, prefabs, profiles, layers, and tags.
- [ ] Next production diagnosis/fix must directly validate surface orientation versus observed face/bevel response ordering under the same light.


## GM-SURFACE.5Q-DIAG — Exhaustive surface-orientation stage attribution

- [x] Record the canonical 5Q plan before implementation.
- [x] Preserve the completed H3 identity/readback/Lambert and Stage A/B/C/D matrix.
- [x] Add Stage E over all three validated camera views and all 27 deterministic light directions.
- [x] Capture triangle/current/stored normals; raw generated-mass channels; dirt/height/upwardness; resolved and nonlinear exposure/crevice/base/dirt fields; mottle and response scalars.
- [x] Capture every cumulative pre-light albedo stage and its stored-normal direct-light response.
- [x] Capture GPU NdotL, distance attenuation, shadow attenuation, and actual main-light direction.
- [x] Validate cumulative direct response pixel-by-pixel against captured albedo × captured NdotL × captured attenuation.
- [x] Fail closed if cached albedo, NdotL/attenuation, and direct-response captures disagree on identity-relative readback orientation or foreground pixel count.
- [x] Capture legacy actual-material, HLSL production-normal, and HLSL stored-normal PBR references for every Stage E view/direction.
- [x] Capture direct and PBR one-layer ablations for tonal, exposure, mottle, crevice, base, dirt, wet, frost, monolithic, overall tint, specular-zero, and all pre-light value authorities together.
- [x] Compute source-face NdotL/order inversions, conditional parent-bevel-parent violations, stage correlations, first divergent stage, and ablation reductions.
- [x] Correct the parent-envelope criterion so it is applied only when bevel NdotL is genuinely intermediate between parent NdotL values.
- [x] Preserve per-triangle provenance, geometry-quality, mask-endpoint, luminance-distribution, normal, and HLSL stage evidence in the dedicated streamed orientation CSV.
- [x] Reconcile the matrix to 3,677 counted cases and 3,681 total render/readback passes.
- [x] Preserve incremental/cancellable asynchronous execution; checkpoint the large run periodically rather than rebuilding the full growing report after every render.
- [x] Complete final offline source/scope/symbol/mode/production-variant audit (`71 / 71`).
- [x] Complete changed-file-only archive extraction/hash integrity audit.
- [x] Compile and run evidence accepted from the 2026-08-07 Unity 6000.5.0f1 session; the report completed 3,677/3,677 counted cases, 3,681 total render passes, valid identity/alignment/Lambert contracts, complete Stage E family coverage, and no completeness failure.
- [x] Review the complete 5Q text report and preserve its Stage E evidence in the canonical 5R problem record.
- [x] Root-cause ownership accepted as pre-light material/value response; production work may proceed only against the measured orientation defect.

## GM-SURFACE.5R — Orientation-coherent material response baseline

Status: **implemented; static audit passed; user visual acceptance recorded 2026-08-09**.

### Problem record

The unresolved defect is not whole-rock darkness, overall exposure, ambient strength, indirect-light magnitude, smoothness, or specular strength. It is **per-surface directional-light incoherence** in the HLSL Generated Mass material: source faces and ordinary bevel faces can render in a bright/dark ordering that contradicts their measured orientation to the same incident light. The legacy material remains the behavioral reference because its visible face ordering follows orientation coherently on the same frozen geometry.

GM-SURFACE.5Q completed the exhaustive Stage E matrix and established the following evidence on the frozen suspect mesh:

- all 3,677 counted cases and 3,468 orientation cases completed, with zero completeness failure;
- the controlled Lambert path matched captured GPU normals to approximately `1e-7` normalized RMSE;
- every cumulative direct checkpoint matched `captured pre-light albedo × captured NdotL × attenuation` with normalized RMSE `0`, exonerating the direct-light multiply itself;
- source-face orientation inversions were `0` at `BASE` and `TONAL`, then first appeared at `EXPOSURE_SCALE` (`+28`), increased strongly at `CREVICE` (`+38` newly introduced there), and increased again at `BASE_LAYER` (`+15` newly introduced there), reaching `80` in the final pre-light state;
- bevel conditional violations rose from the constant-base floor of `64` to `189` at `TONAL`, with `130` newly introduced there, then `+39` new violations at `EXPOSURE_SCALE`, `+23` at `CREVICE`, and `+11` at `BASE_LAYER`;
- disabling all pre-light value authorities reduced the combined orientation-error count from `266` to `64` (`75.93985%` reduction) and removed every source-face inversion;
- disabling specular produced zero reduction, so specular/F0 is not an owner of this defect;
- the retained 5O cold-grey generated-normal bypass did not visually close the defect and remains a rejected root-cause explanation.

The 5Q `firstDivergentStage=BASE` summary is not treated as evidence that constant base colour itself is wrong. Its conditional bevel classifier permits a wider NdotL parent-envelope tolerance than luminance-envelope tolerance, leaving a known 64-case constant-base floor. Production decisions therefore use **introduced** stage errors and the exact captured direct-product identity, not that summary label alone.

### Objective

Establish an orientation-coherent Generated Mass material baseline without changing directional-light math, geometry, topology, mesh normals, mask generation, or the validated diagnostic suite. Material semantics may retain tint/identity information, but they must not act as a second fake illumination field that can overpower real `NdotL` ordering.

### Approved files

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceForwardPass.hlsl`
- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader`
- `Assets/Game/Demo/Materials/Stone/M_PixelStone_HLSL_ColdGrey.mat`

### Acceptance criteria

1. Generated Mass exposure no longer brightens or darkens pre-light albedo according to the exposure/upwardness/height mask. Exposure tint remains available only through value-preserving tint controls.
2. Generated Mass crevice response no longer darkens direct-light albedo toward a fixed dark target. Crevice semantic tint may remain available, but its response must not impose a lighting-like luminance field before PBR.
3. Generated Mass base/contact response no longer darkens direct-light albedo toward a fixed dark target. Base semantic tint may remain available under the same value-preserving rule.
4. Generated Mass bevel tonal response is topology-independent: generated bevel/chamfer faces do not consume interpolated vertex-R tonal authority, broad tonal variation, or cell-warp authority. They retain only world-position pixel-cell variation.
5. The cold-grey baseline uses the legacy pixel amplitudes (`Pixel Variation 0.057`, `Vertex Variation 0.09`) and disables HLSL-only broad variation/warp for this parity baseline; `Profile Pixel Contrast` is `1.0`.
6. Ground-surface semantic rendering remains unchanged.
7. Dirt, mottle, wetness, frost, monolithic response, overall tint, PBR direct-light math, F0, smoothness, geometry, mask generation, and the 5Q diagnostic remain unchanged unless a compile fix is strictly required.
8. No scene, prefab, profile asset, layer, tag, project-setting, geometry, triangulation, normal-generation, or mask-generation change is permitted.

### Implementation sequence

- [x] Review current canonical documents, material defaults, Generated Mass property publication, mesh mask producers, forward shader stages, shader property contract, and the complete 5Q result before editing.
- [x] Record this canonical plan as the first source-tree modification.
- [x] Change Generated Mass tonal construction so bevel/chamfer pixels suppress interpolated vertex-R, broad variation, and warp authority while retaining world-position pixel-cell variation.
- [x] Remove Generated Mass exposure luminance scaling from the pre-light albedo path while preserving ground semantic scaling and value-preserving exposure tint support.
- [x] Replace Generated Mass crevice/base dark-albedo layers with value-preserving semantic tint-only application; do not move their old darkening into direct lighting.
- [x] Set the cold-grey parity baseline to legacy pixel amplitudes and disable HLSL-only broad/warp tonal values.
- [x] Add explicit shader comments defining exposure/crevice/base as material semantics rather than illumination and recording the 5Q evidence that motivated the change.
- [x] Update the canonical architecture/framework with the full 5Q evidence, the known bevel-classifier floor, the 5R response contract, rollback criteria, and unresolved follow-up if visual coherence is not restored.
- [x] Run exact-scope diff audit, serialized-material diff audit, shader preprocessor/symbol audit, and cross-subsystem shared-shader impact audit (`39 / 39` static checks passed). Package re-extraction integrity is completed at delivery packaging.
- [x] User-applied Unity 6000.5.0f1 session completed without a reported compile error, and same-scene visual orientation behavior was accepted on 2026-08-09. Independent model-side Unity reproduction was not available.

### Rollback / failure interpretation

If 5R does not materially restore the legacy-like orientation ordering, do not reframe the problem as global darkness or specularity. Preserve this evidence record and continue from the remaining pre-light/PBR differential using the 5Q raw orientation CSV. A failed 5R visual trial would mean either another pre-light authority still dominates or the remaining PBR/indirect closure requires separate treatment; it would not invalidate the proven `NdotL` and direct-product contracts.

## GM-SURFACE.5S — Low-light directional form readability

Status: **implemented in source; static audit passed `58 / 58`; Unity compile/visual/performance validation pending**.

### Problem record

GM-SURFACE.5R is accepted for the former surface-orientation incoherence defect. The remaining problem is separate: when real direct Sun illumination is strongly reduced, Generated Mass stone can remain correctly shaded yet lose enough directional contrast that top, side, and bevel planes become difficult to distinguish. Stronger real illumination restores the form read. This is therefore a low-light readability problem, not permission to restore exposure/upwardness/height, crevice, base/contact, bevel identity, or other semantic fields as fake illumination.

### Objective

Preserve dark low-light appearance while shaping the existing indirect/baked-GI response modestly according to the actual main-light direction and the resolved fragment normal. The response must rotate with the real main light, remain subordinate to genuine direct/local lights, create no emissive floor, and preserve exact 5R behavior when strength is zero.

### Approved files

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceForwardPass.hlsl`
- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader`

No material, C#, Weather, geometry, normal-generation, scene, prefab, profile, layer, tag, new shader property, texture, buffer, pass, or draw-call change is approved.

### Reviewed evidence and constraints

- Pre-5S production `BuildInputData` supplied `SampleSH(normalWS)` directly as baked GI; that baseline contained no low-light directional form shaping.
- `_ShadowAmbientStrength` and `_DiffuseWrap` are already serialized by the HLSL stone materials but are unused by the production forward implementation. They are reused without migration or default changes.
- The Weather cloud-shadow controller applies its generated cloud texture as the authoritative Sun cookie. Weather remains read-only for this update.
- Official current URP source shows bare `GetMainLight()` returns base main-light direction/color with unit shadow attenuation, while position-aware overloads apply shadowing and `_LIGHT_COOKIES`. The supplied code-only archive does not include the project's installed `Packages` source, so exact Unity 6000.5.0f1 local-package verification remains pending and must not be represented as passed.
- Existing 5Q controlled cases explicitly zero `_DiffuseWrap` and `_ShadowAmbientStrength`, disable cookies where required, and preserve direct-only ownership evidence. The production/audit PBR and ambient mirror must nevertheless share the new GI resolver so future actual-scene audit modes cannot silently diverge from production.

### Approved response

Let `bakedGI` be the existing `SampleSH(normalWS)` result and `mainLight = GetMainLight()`.

```text
sourceLuma = dot(mainLight.color, (0.2126, 0.7152, 0.0722))
sourceGate = saturate(sourceLuma)
facing = clamp(dot(normalWS, mainLight.direction), -1, 1)
wrap = saturate(_DiffuseWrap)
wrappedFacing = lerp(facing, max(facing, 0), wrap)
targetScale = 1 + 0.40 * wrappedFacing
formWeight = saturate(_ShadowAmbientStrength) * sourceGate
bakedGI *= lerp(1, targetScale, formWeight)
```

The helper deliberately does not use position-dependent shadow/cookie attenuation, because that would remove the readability cue precisely under the cloud-shadow condition it is intended to address. The current official bare-main-light helper can expose a per-object culling eligibility value, but that is not part of the approved 5S formula; unusual Sun culling-mask configurations therefore remain a validation risk rather than a silent design amendment.

For the current ColdGrey values (`Strength 0.42`, `Wrap 0.12`) with an eligible white main light, the approximate indirect multipliers are `1.168` fully facing, `1.0` perpendicular, and `0.852` fully opposite. The effect redistributes existing indirect response; if baked GI is zero, it adds zero light.

### Acceptance criteria

1. Cloud-shadowed/weak-Sun rocks remain visibly dark; no emissive/general exposure floor is introduced.
2. Major planes and bevel transitions remain readable through directional separation tied to the actual main-light direction.
3. Rotating the main light rotates the form cue. World-up, height, exposure, crevice, base/contact, dirt, structural/bevel identity, and other semantic fields do not choose which plane is brighter.
4. `_ShadowAmbientStrength = 0` restores exact 5R baked-GI behavior.
5. Ground continues to use its existing raw baked-GI path.
6. Existing nearby/strong real lights remain dominant over the modest indirect shaping.
7. 5R pre-light material semantics, normal construction, direct-light math, `BuildSurfaceData`, materials, Weather, geometry, masks, and diagnostic direct checkpoints remain unchanged.
8. Shader compilation introduces no warnings/errors and the shared-shader impact audit finds no unintended non-Generated-Mass behavior change.
9. Active-gameplay GPU cost is profiled before final acceptance. No texture fetch, shadow/cookie lookup, buffer, varying, CPU update, allocation, pass, draw, or dispatch is permitted by this design.

### Implementation sequence

- [x] Re-read repository instructions and review the current canonical Generated Mass documents, complete forward implementation, shader property/CBUFFER contract, five HLSL stone material values, Generated Mass material/profile publication, Weather Sun-cookie ownership, and relevant 5Q audit overrides.
- [x] Reconcile 5R user visual acceptance and record this 5S plan before any shader implementation change.
- [x] Update the canonical architecture and framework with the 5R acceptance state and 5S illumination-layer contract.
- [x] Add one Generated-Mass-only baked-GI resolver using the approved formula; Ground retains raw `SampleSH(normalWS)` through the explicit surface-contract gate.
- [x] Route production `BuildInputData` and the audit ambient/PBR mirror through the same resolver; the direct-only diagnostic helper remains byte-equivalent to the supplied 5R source.
- [x] Relabel the existing serialized controls to `Low-Light Form Wrap` and `Low-Light Form Strength` without changing property names, defaults, CBUFFER layout, or material YAML.
- [x] Run exact-scope diff, preprocessor/delimiter/symbol checks, zero-strength algebra check, material-byte-identity check, Ground-path audit, 5R-invariant audit, and cross-subsystem shared-shader audit (`58 / 58` passed).
- [ ] Unity 6000.5.0f1 compile, visual direction/low-light/strong-light validation, and GPU profiling remain authoritative runtime gates after delivery.

### Performance contract

The new work is fixed O(P) fragment ALU for P covered Generated Mass fragments: one base main-light fetch/struct access, two dot products, clamps/max/lerps, scalar arithmetic, and one baked-GI RGB multiply. Persistent memory remains O(1); there is no CPU or generated-data cost. Because this is active-gameplay fragment work, analytical cost is not acceptance evidence: target GPU timing remains pending until Unity profiling.

### Rollback / failure interpretation

If low-light form remains unreadable, first determine whether existing baked GI is effectively zero or the configured form strength is insufficient; do not restore semantic fake-light fields. If the helper is visibly active under strong light, tune only within the approved strength/wrap/form coefficient contract after evidence. If exact local URP source contradicts the assumed bare-`GetMainLight()` semantics, stop and revise the plan before changing the implementation API.

## GM-SURFACE.5S1 — Generated Mass low-light form-strength authoring

Status: **implemented in source; static validation pending; Unity validation pending**.

### Problem record

GM-SURFACE.5S is implemented, but the current effect remains too weak in the user's low-light validation scene. The present authoring location is the shared material value `_ShadowAmbientStrength`, which is unsuitable for Generated Mass workflow because material/profile changes should not decide per-object low-light readability strength. The next step is to publish one Generated-Mass-owned strength override through the existing renderer `MaterialPropertyBlock` so every compatible stone material on that mass receives the same 5S strength.

### Objective

Expose **Low-Light Form Strength** directly on `GeneratedMass`, default it to the current effective 5S value `0.42`, and publish it through the existing object-level property-block path to `_ShadowAmbientStrength`. The control must apply across compatible stone materials, remain bounded to the approved `0..1` 5S range, and leave `Low-Light Form Wrap` material-owned for now.

### Approved files

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Game/Procedural/Masses/GeneratedMass.cs`

No shader, material, Weather, scene, prefab, layer, tag, geometry, normal-generation, or new shader-property change is approved.

### Acceptance criteria

1. `GeneratedMass` exposes one object-level **Low-Light Form Strength** control.
2. The control writes `_ShadowAmbientStrength` through the existing renderer `MaterialPropertyBlock` path, so profile/material swaps on that mass do not change the chosen strength.
3. Default authored value is `0.42`, matching the current 5S effective ColdGrey baseline.
4. Values are clamped to the currently approved `0..1` 5S strength range.
5. No new control is added for Wrap in this patch.
6. Materials remain byte-identical; the shader property contract remains unchanged.
7. Existing Generated Mass property publication and all unrelated lighting/geometry behavior remain unchanged.

### Implementation sequence

- [x] Review current 5S implementation, canonical documents, Generated Mass material publication, and object-level `MaterialPropertyBlock` path.
- [x] Record this 5S1 plan in the canonical checklist before editing code.
- [x] Update framework and architecture to state that Generated Mass owns 5S strength authoring and publishes it object-level across compatible materials.
- [x] Add one serialized `GeneratedMass` field for **Low-Light Form Strength** with default `0.42` and approved `0..1` range.
- [x] Publish the field to `_ShadowAmbientStrength` through the existing renderer `MaterialPropertyBlock` path.
- [x] Run exact-scope diff audit, material-byte-identity audit, property-publication audit, and cross-subsystem impact audit.
- [ ] Unity compile and scene validation remain pending after delivery.

### Rollback / failure interpretation

If object-level strength `1.0` remains too weak, do not silently widen the 5S algorithm or reintroduce semantic fake-light fields in this patch. Record the evidence and evaluate a separate approved follow-up for stronger response range or different low-light shaping.

## GM-SURFACE.5S2 — Extended low-light form strength

Status: **implemented in source; static audit passed `35 / 35`; Unity validation pending**.

### Problem record

User validation of GM-SURFACE.5S1 proves that object-level `Low-Light Form Strength` is correctly wired and produces the intended directional response, but `1.0` remains visibly insufficient in very uniform low-light conditions. The defect is therefore no longer control plumbing; the current shader clamps the strength to `1.0`, making `1.0` the hard algorithmic ceiling.

### Objective

Extend the existing Generated-Mass-owned 5S strength from `0..1` to `0..2` while preserving the already-tested response exactly for every value in `0..1`. Keep the same real-main-light-direction / baked-GI architecture and the same `0.40` directional coefficient. Do not add an explicit per-position low-light gate in this patch: current URP applies shadow/cookie attenuation through position-aware main-light queries, so sampling that again before `UniversalFragmentPBR` would duplicate per-fragment lighting work. The GI-only response is already naturally more visible when real direct lighting is weak because direct light no longer dominates the shaped indirect term.

### Approved files

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Game/Procedural/Masses/GeneratedMass.cs`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceForwardPass.hlsl`
- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader`

No material, editor, Weather, scene, prefab, geometry, normal-generation, new shader property, texture, buffer, pass, draw call, or additional per-fragment shadow/cookie lookup is approved.

### Reviewed evidence and invariants

- GM-SURFACE.5S1 publishes the Generated-Mass-owned strength through the existing renderer `MaterialPropertyBlock` to `_ShadowAmbientStrength`; no material asset must become authoritative again.
- The current 5S helper computes `formWeight = saturate(_ShadowAmbientStrength) * sourceGate`; this is the exact hard ceiling that prevents values above `1.0` from having any effect.
- The current directional target remains `targetScale = 1 + 0.40 * wrappedFacing`, where `wrappedFacing` is bounded to `[-1, 1]`. With Wrap in `0..1`, `targetScale` is bounded to `0.60..1.40`.
- With strength clamped to `0..2` and `sourceGate` in `0..1`, `lerp(1, targetScale, formWeight)` is therefore bounded to `0.20..1.80`. No negative GI multiplier is possible in the approved range.
- For every strength in `0..1`, replacing `saturate(strength)` with `clamp(strength, 0, 2)` is algebraically identical; previous 5S/5S1 behavior is preserved exactly in that interval.
- Ground, 5R semantic-mask rules, direct lighting, material albedo construction, normals, Weather, and direct-only diagnostics remain unchanged.

### Acceptance criteria

1. `GeneratedMass` exposes `Low-Light Form Strength` over `0..2`, retaining default `0.42`.
2. `0`, `0.42`, and `1.0` produce the same shader response as before this patch; `2.0` produces a materially stronger response.
3. The forward helper honors values above `1.0` and clamps only to the approved `0..2` range.
4. At maximum strength the directional GI multiplier remains mathematically positive and bounded; no emission/minimum-light floor is introduced.
5. No additional shadow, cookie, texture, buffer, pass, draw, dispatch, or CPU-per-frame work is added.
6. Materials remain byte-identical and the object-level property block remains authoritative for Generated Mass strength.
7. Ground and all 5R orientation-coherence invariants remain unchanged.

### Implementation sequence

- [x] Re-read repository instructions and review current 5S/5S1 implementation, object-level property publication, shader range, shared forward helper, custom Generated Mass inspector behavior, material defaults, diagnostic zero-overrides, and current URP main-light/cookie structure.
- [x] Record this 5S2 plan as the first source-tree modification.
- [x] Update the canonical framework and surface-response architecture with the extended `0..2` contract, mathematical bounds, and explicit rejection of a duplicate position-aware low-light sample in this patch.
- [x] Change the Generated Mass serialized control and property-block publication clamp from `0..1` to `0..2`; preserve default `0.42`.
- [x] Change the shared forward helper from `saturate(_ShadowAmbientStrength)` to an explicit `0..2` clamp; preserve all other 5S math.
- [x] Change only the ShaderLab authoring range for the existing `_ShadowAmbientStrength` property from `0..1` to `0..2`; preserve property name/default/CBUFFER layout.
- [x] Run exact-scope diff, shader/preprocessor, algebraic-bound, material-byte-identity, Ground-isolation, 5R-invariant, diagnostic-parity, and cross-subsystem impact audits (`35 / 35` passed).
- [ ] Unity 6000.5.0f1 compile and same-scene visual validation at `0`, `1`, and `2` remain pending after delivery.

### Performance contract

Relative to 5S1, the production fragment path adds no new operation class: the existing scalar strength clamp widens from `0..1` to `0..2`. No new main-light call, shadow/cookie sample, texture fetch, varying, buffer, pass, draw, dispatch, CPU update, allocation, or generated data is added. Active-gameplay GPU cost should therefore remain effectively the 5S1 cost; Unity profiling remains the authoritative runtime check.

### Rollback / failure interpretation

If `2.0` remains insufficient, the evidence will show that simple amplitude extension has reached the safe current-response range. Do not silently raise the range past the point where the current signed multiplier can become zero/negative, and do not restore semantic fake-light fields. A further change must be separately designed around a stronger bounded response curve or a deliberately different readability layer.

## GM-SURFACE.5S3 — Sun-orthogonal face separation

Status: **implemented and statically valid, but visually rejected by user validation on 2026-08-11; superseded by GM-SURFACE.5S4**.

### Problem record

User validation of 5S2 shows that amplitude is no longer the primary limitation. Strength `2.0` can create more than enough bright-versus-dark contrast, but multiple neighboring faces can still collapse to nearly the same value when their projection onto the Sun direction is similar. The current helper is one-dimensional: its directional input is only `dot(resolvedNormal, mainLight.direction)`. A stronger scalar multiplier can enlarge existing differences but cannot distinguish two faces whose Sun projection is equal or nearly equal.

### Objective

Add a second, bounded orientation coordinate used only as an indirect-light readability tie-breaker. Keep the existing Sun-facing response and `Low-Light Form Strength` unchanged. Add a Generated-Mass-owned `Low-Light Face Separation` control whose default `0` is exact 5S2 parity. The new cue uses the mesh geometric face normal against a camera-view axis projected perpendicular to the Sun, so similarly Sun-facing faces can occupy different values without increasing the established global GI multiplier envelope.

### Approved files

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Game/Procedural/Masses/GeneratedMass.cs`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceForwardPass.hlsl`
- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader`

No material, Weather, scene, prefab, geometry-generation, mesh-channel, normal-generation, diagnostic C#, custom Inspector, layer, tag, texture, buffer, pass, draw-call, additional light query, or additional SH sample change is approved. The larger Generated Mass Inspector redesign is explicitly deferred to a separate future patch; 5S3 does not partially redesign that surface.

### Reviewed evidence and constraints

- The 5S2 helper computes its primary orientation from one scalar Sun projection and then scales existing baked GI. This explains why amplitude can be sufficient while distinct faces remain value-degenerate.
- Final Generated Mass render vertices already carry a stable per-triangle mesh normal: all three emitted vertices for one accepted triangle receive the same resolved face normal. No face ID or new mesh stream is required.
- Production forward shading retains that mesh normal separately from the optional whole-rock perturbed normal, so the new tie-breaker can remain face-stable across compatible stone profiles.
- The existing custom Inspector does not deliberately place the 5S strength control; non-excluded serialized fields are rendered by its generic fallback. The same temporary behavior is acceptable for the new control until the separately planned complete Inspector redesign.
- Unity 6000.5 URP documents `GetViewForwardDir()` as the world-space forward direction of the current view. Using its negation provides one view-to-camera axis that is constant across a view, avoiding per-face gradients from a per-pixel position-to-camera vector.
- The camera-relative term is an explicitly stylized indirect readability cue. Real direct lighting remains Sun-authoritative. The cue depends on camera rotation but not camera position.

### Approved response

Let `p` be the existing wrapped Sun-facing term, `S` the existing `Low-Light Form Strength` clamped to `0..2`, `F` the new `Low-Light Face Separation` clamped to `0..1`, `L` the normalized main-light direction, `V = -GetViewForwardDir()`, and `Ng` the stable mesh geometric face normal.

```text
axisRaw = V - L * dot(V, L)
axis = normalize(axisRaw) when |axisRaw|^2 is safely nonzero, otherwise 0
azimuth = clamp(dot(Ng, axis), -1, 1)
headroom = 1 - abs(p)
primary = S * p
separation = 0.50 * F * headroom * azimuth
combined = primary + separation
GI multiplier = 1 + 0.40 * sourceGate * combined
```

The existing 5S2 expression is algebraically recovered exactly when `F = 0`.

For all `S in [0,2]`, `F in [0,1]`, `p in [-1,1]`, `azimuth in [-1,1]`, and `sourceGate in [0,1]`, the `headroom = 1 - |p|` term keeps `combined` inside `[-2,2]`; therefore the existing global GI multiplier envelope remains `0.20..1.80`. The new control redistributes ambiguous faces inside the established envelope instead of increasing maximum contrast.

### Acceptance criteria

1. `Low-Light Face Separation = 0` is exact 5S2 shader parity for every existing strength/wrap value.
2. Increasing Face Separation creates visible differences among faces with similar Sun-facing response while preserving the same theoretical `0.20..1.80` maximum GI multiplier envelope.
3. The new tie-breaker uses the stable mesh geometric normal, not semantic masks, feature IDs, random face values, height, world-up, crevice/base, or whole-rock perturbation.
4. Existing `Low-Light Form Strength` remains independent. Authors can lower Strength to reduce extreme bright/dark contrast while keeping Face Separation high.
5. Ground remains unchanged and receives neither the primary 5S response nor the new tie-breaker.
6. Direct-light equations, Weather cookie/shadow behavior, 5R material semantics, geometry, mesh normals/channels, and direct-only diagnostics remain unchanged.
7. No additional main-light query, SH sample, texture/cookie/shadow sample, pass, draw, buffer, CPU-per-frame update, or generated-data allocation is introduced.
8. Materials remain byte-identical. The new shader property is hidden and object-authored through the existing Generated Mass property-block path.
9. Unity compilation and same-scene visual testing verify that camera rotation changes only the bounded readability tie-breaker and does not create obvious within-face gradients or unstable flicker.

### Implementation sequence

- [x] Re-read repository instructions and review 5S2 canonical state, the shared forward helper, production normal construction, mesh-output face-normal ownership, Generated Mass property publication, shader CBUFFER contract, custom Inspector fallback behavior, and the Unity 6000.5 camera helper contract.
- [x] Record this 5S3 plan as the first source-tree modification.
- [x] Update the canonical framework and surface-response architecture with the two-axis readability contract, bounded mathematics, camera-relative tradeoff, and deferred Inspector redesign.
- [x] Add object-level `Low-Light Face Separation` authoring with default `0`, range `0..1`, and property-block publication to one new hidden shader scalar.
- [x] Extend the shared baked-GI helper to accept the stable geometric face normal, compute the Sun-orthogonal view axis, and add the bounded separation term while keeping the existing primary response unchanged.
- [x] Preserve production and audit ambient/PBR parity by supplying the stable stored/geometric normal to the shared GI resolver; direct-only audit code remains unchanged.
- [x] Add the hidden shader property to all four existing per-material CBUFFER layouts at the same relative position, preserving each pass's preexisting layout and changing no material asset.
- [x] Run exact-scope diff, shader/preprocessor/CBUFFER, 5S2-parity, mathematical-bound, material-byte-identity, Ground-isolation, 5R-invariant, diagnostic-parity, view-helper-call-count, main-light-call-count, SH-sample-count, and cross-subsystem impact audits (`40 / 40` passed).
- [x] Unity execution reached user visual validation; the camera-axis separation response was rejected because it produced almost no useful separation on bright faces and behaved mainly as a small shared brightness lift on darker visible faces.

### Performance contract

The incremental fragment work is fixed ALU: one existing view-forward helper access, one vector rejection against the already-fetched main-light direction, normalization with a degenerate-axis guard, two dot products, one absolute/headroom calculation, and bounded scalar arithmetic. The patch adds no new main-light lookup, shadow/cookie lookup, SH sample, texture fetch, varying, mesh stream, loop, buffer, pass, draw, dispatch, CPU-per-frame work, allocation, or generated storage. GPU timing remains an authoritative runtime check.

### Rollback / failure interpretation

If Face Separation increases visible differences but camera-relative ordering is objectionable, set the new control to `0` for exact 5S2 behavior and redesign the secondary axis separately. If high Face Separation still leaves specific faces indistinguishable, first confirm that those faces are genuinely distinct in the stable geometric-normal field; do not increase Strength further or restore semantic/random fake-light authorities as a substitute.

## GM-SURFACE.5S4 — Generation-time adjacency-aware face tone palette

Status: **implemented in source; offline static/cross-subsystem audit passed `58 / 58`; Unity compile/visual validation pending**.

### Problem record

User validation rejects the GM-SURFACE.5S3 camera-axis Face Separation formulation. Raising Face Separation from `0` to `1` produces little or no useful distinction on already-bright faces and behaves mainly as a small shared brightness lift on darker visible faces. The remaining requirement is not larger bright-versus-dark amplitude; 5S2 already supplies enough amplitude. The requirement is to make neighboring logical low-poly faces occupy different low-light values more often, even when their real Sun projection is equal or nearly equal.

### Concrete design

Replace the 5S3 camera-relative runtime tie-breaker with a deterministic **generation-time face-tone palette**. Reuse the existing Generated-Mass-owned **Low-Light Face Separation** control as the sole authoring control for this layer.

The generator will:

1. reconstruct logical planar face groups from final triangles by shared quantized edges plus near-equal render normals;
2. build a logical-face adjacency graph from shared boundary edges;
3. classify logical groups carrying the existing `ConvexBoundary` structural contribution as transition/bevel groups; these do **not** receive independent palette identities;
4. build effective adjacency between non-convex logical faces, including adjacency bridged through a convex transition group, then assign one of five seed-deterministic base tonal classes `[-1, -0.5, 0, +0.5, +1]` to those non-convex faces by greedily maximizing minimum tonal distance from already assigned effective neighbors;
5. resolve each convex transition group's raw tone from the average of its adjacent non-convex parent/neighbor tones, so ordinary bevel tone stays between its parents instead of becoming an independent bright/dark identity;
6. area-weight recenter the completed rock to approximately zero mean tone, then uniformly scale down only when required to keep all final face tones inside `[-1, +1]`;
7. write the resulting signed face tone identically to every render vertex of each logical face in the currently unused Generated Mass `UV2.w` component.

The shader will decode the signed `UV2.w` face tone directly. At **Low-Light Face Separation = 1**, the stylized face layer contributes at most `±0.16` to the existing indirect-GI multiplier. The final multiplier remains clamped to the existing 5S2 global `0.20..1.80` envelope, so Face Separation redistributes faces inside the established contrast budget rather than creating darker darks or brighter brights beyond that budget. `Face Separation = 0` remains exact 5S2 parity.

### Approved files

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Game/Procedural/Masses/MassGenerator.MeshOutput.cs`
- `Assets/Game/Procedural/Masses/GeneratedMass.cs`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceForwardPass.hlsl`

Create/delete/move/rename: none.

No shader-property declaration, material, Weather, scene, prefab, custom-Inspector, layer, tag, new mesh stream, texture, buffer, pass, draw call, additional light query, additional SH sample, or per-frame C# change is approved. The comprehensive Generated Mass Inspector redesign remains separate future work.

### Reviewed evidence and constraints

- 5S3's control publication is correct; the failure is the runtime camera-axis equation, not object-to-shader plumbing.
- Final Generated Mass meshes duplicate render vertices per triangle and carry one stable flat render normal per emitted triangle. Coplanar adjacent triangles can therefore be grouped deterministically without changing geometry or normals.
- The existing Generated Mass `UV2.w` value is emitted as `0`, is finite-validated and carried through upload, is included in immutable diagnostic fingerprints, and is not consumed by current Generated Mass production shading. Ground has a separate semantic use for its own `UV2.w`; the new signed interpretation is therefore explicitly gated to the Generated Mass surface contract.
- Material-mask inheritance modifies only exposure, crevice/base, and dirt channels and leaves `UV2.w` untouched, so generation-time face tone remains stable through the existing inheritance/reconciliation pass.
- Existing projection/assembler readers transport `UV2` but do not consume `UV2.w` as Generated Mass lighting authority.
- The current hidden Face Separation shader scalar and GeneratedMass object control are reused; no new serialized authoring control or shader property is required.

### Acceptance criteria

1. `Low-Light Face Separation = 0` is exact 5S2 visual/shader parity regardless of generated face-tone data.
2. At nonzero separation, touching non-convex logical planar faces preferentially receive different generated tone values; source/cap faces separated by an ordinary convex transition are treated as effective neighbors for palette assignment. Coplanar triangles within one logical face receive exactly one shared tone.
3. ConvexBoundary transition/bevel groups do not receive independent palette classes; when adjacent non-convex parent tones exist, their raw tone is the average of those neighboring tones and therefore lies inside the parent-tone envelope.
4. Generation is deterministic for identical geometry and Surface Seed.
5. The area-weighted generated face-tone mean is approximately zero and every encoded value stays in `[-1,+1]`, preventing systematic whole-rock brightening/darkening from the palette itself.
6. At control `1`, face-tone contribution is limited to `±0.16` indirect-GI multiplier before the existing global clamp; final Generated Mass low-light multiplier remains inside `0.20..1.80`.
7. Face tone affects existing baked/indirect GI only. Direct URP lighting, Weather shadows/cookies, normals, material semantic masks, geometry, and 5R orientation rules remain unchanged.
8. Ground behavior is byte/algebraically unchanged by the shader branch and keeps its own `UV2.w` meaning.
9. No additional per-fragment light query, view-vector operation, `cross`, normalization, SH sample, texture sample, buffer access, loop, pass, or draw is introduced. The 5S3 camera-axis ALU is removed.
10. No new mesh stream or persistent per-rock allocation is introduced; generation-time temporary graph data is discarded after mesh compilation.
11. The production-generation contract version is incremented so existing Generated Mass objects invalidate stale pre-5S4 meshes and rebuild the new face-tone channel through the ordinary synchronization path.
12. Existing material assets remain byte-identical and the custom Inspector remains untouched.

### Implementation sequence

- [x] Re-read repository instructions and reconstruct the current source from the supplied archive plus accepted 5S/5S1/5S2/5S3 patches.
- [x] Review canonical Generated Mass documents, final mesh construction, mesh upload/channel validation, material-mask inheritance, immutable diagnostic fingerprints, current 5S3 shader helper, object-level low-light publication, and all current `UV2.w` consumers.
- [x] Record this canonical 5S4 plan as the first source-tree modification.
- [x] Update framework and surface-response architecture to supersede 5S3's camera-axis tie-breaker with the generation-time face-tone contract and reserve Generated Mass `UV2.w` for signed face tone.
- [x] Compile deterministic logical-face groups and adjacency from final mesh triangles, assign/recenter bounded tones, write them to `UV2.w`, and validate the generated channel.
- [x] Update GeneratedMass upload validation and Face Separation tooltip/comment to the generation-time palette semantics while retaining the same serialized control/property publication; increment the production-generation contract version so stale meshes rebuild the new channel.
- [x] Replace the 5S3 camera-axis runtime math with the fixed-cost face-tone multiplier and retain exact `Face Separation = 0` 5S2 parity.
- [x] Run exact-scope diff, deterministic-output, face-group constancy, adjacency distinction, weighted-mean/range, material-byte-identity, mask-inheritance immutability, Ground-isolation, 5R-invariant, diagnostic-parity, main-light/SH-call-count, shader-delimiter, and cross-subsystem impact audits (`58 / 58` passed).
- [ ] Unity 6000.5.0f1 compilation and same-scene visual tuning remain pending after delivery.

### Performance contract

Generation adds bounded dirty/build-time CPU work proportional to final triangle/edge/group counts. It allocates only temporary dictionaries/lists/sets during mesh compilation and stores the final scalar in an already-present vertex component, so persistent vertex memory is unchanged. Runtime fragment work becomes cheaper than 5S3: the view-forward lookup, Sun-orthogonal projection, reciprocal-square-root normalization, geometric-normal dot, headroom calculation, and associated vector ALU are removed and replaced with one interpolated scalar read plus a few scalar multiply/add/clamp operations. No per-frame CPU work is added.

### Implementation status

Source implementation and offline exact-scope/static/cross-subsystem validation are complete. The offline audit passes `97 / 97`. Unity 6000.5.0f1 compilation, live mesh readback, active forward-fragment diagnostic views, and runtime property-block verification remain authoritative pending gates because Unity and a C# compiler are unavailable in this environment.

### Risks and rollback

- `UV2.w` was previously reserved for future concave-crease localization. 5S4 intentionally consumes it for Generated Mass face tone because it is currently unused and avoids a new stream; future concave work must respect this updated channel contract rather than silently reusing the component.
- Greedy five-tone assignment maximizes local distinction but does not mathematically promise a globally unique value for every face. The acceptance target is adjacent/local readability, not globally unique grayscale IDs.
- If the fixed `±0.16` full-strength palette is too weak or too strong, tune the existing Face Separation control first. Do not add another palette magnitude control until visual evidence proves one is required.
- Setting Face Separation to `0` is the exact runtime rollback to 5S2 behavior. The generated tone channel may remain present at zero runtime cost beyond interpolation already carried by the existing `UV2` varying.

## GM-SURFACE.6A — Structural material response baseline

Status: **implemented source baseline; offline static/cross-subsystem audit passed `68 / 68`; Unity visual validation found no observable `0`→`1` response for either module; response coefficients are superseded by GM-SURFACE.6A.1**.

### Objective

Activate the existing packed `ConvexBoundary` and `CornerChipCap` semantic contributions as two independent, fixed-cost **material-response** modules. This first structural-response pass must not modify whole-rock normals, structural normals, geometry, semantic generation, direct-light equations, Weather behavior, or 5R/5S4 lighting contracts.

### Approved files

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Game/Procedural/Masses/GeneratedMass.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`
- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGeneratedMassFeatures.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceForwardPass.hlsl`

Create/delete/move/rename: none.

No `MassGenerator` implementation, mesh-format, material asset, Weather, scene, prefab, layer, tag, texture, buffer, new pass, new draw call, or structural-normal change is approved.

### Reviewed evidence

- `ConvexBoundary` and `CornerChipCap` are already emitted by the current semantic contribution resolver and packed into the existing primary/secondary `TEXCOORD4` slots with normalized strengths. No generator or vertex-stream change is required.
- Production forward shading currently consumes `ConvexBoundary` only as the GM-SURFACE.5R bevel-tonal suppression classifier; `CornerChipCap` has no production material response.
- The historical `ApplyGeneratedMassGeometryEdgeWearResponse` UV2.z path is not called by production shading and contains the quarantined bevel brightness-lift/tint model. 6A must not restore that path.
- Pre-6A `GeneratedMass.ApplyMaterialProperties` published `_GeneratedMassEdgeWearResponseStrength = 0`, `_GeneratedMassEdgeWearBrightnessLift = 0`, and `_GeneratedMassEdgeWearTintStrength = 0` to preserve the 5E/5R baseline. 6A reactivates only the serialized response-strength value as input to the new semantic convex module; legacy brightness/tint authority remains zero/quarantined.
- The custom Inspector already exposes Edge Wear Response Strength/Softness and the corner-chip geometry controls. 6A adds only one new object-level `Chip Interior Response` control and performs the minimum local wording/layout correction necessary for the active structural material response. The comprehensive Inspector redesign remains separate future work.

### Structural response contract

Resolve semantic strengths from both packed contribution slots:

```text
convex = max(
    primary.Type == ConvexBoundary ? primary.Strength : 0,
    secondary.Type == ConvexBoundary ? secondary.Strength : 0)

chip = max(
    primary.Type == CornerChipCap ? primary.Strength : 0,
    secondary.Type == CornerChipCap ? secondary.Strength : 0)
```

Convex response reuses the existing object-authored `edgeWearResponseStrength` as the master **Convex Surface Response** and `edgeWearSoftness` as its material-softness character:

```text
convexResponse = convex * saturate(_GeneratedMassEdgeWearResponseStrength)
convexBreakupReduction = convexResponse * lerp(0.15, 0.35, saturate(_GeneratedMassEdgeWearSoftness))
convexSmoothnessBoost = convexResponse * lerp(0.03, 0.08, saturate(_GeneratedMassEdgeWearSoftness))
```

Chip response adds one object-authored `Chip Interior Response`, range `0..1`, default `0.60`:

```text
chipResponse = chip * saturate(_GeneratedMassChipInteriorResponse)
chipBreakupIncrease = chipResponse * 0.35
chipSmoothnessReduction = chipResponse * 0.10
```

The existing world-position pixel variation is scaled before tonalScale construction:

```text
structuralPixelVariationScale = clamp(
    1 - convexBreakupReduction + chipBreakupIncrease,
    0.65,
    1.35)
```

Only the `pixelVariation * _PixelVariation` contribution uses this multiplier. Vertex-R/broad/warp 5R bevel restrictions remain unchanged.

PBR smoothness receives only the bounded semantic adjustment:

```text
smoothness = saturate(
    ResolveProfileSmoothness()
    + convexSmoothnessBoost
    - chipSmoothnessReduction)
```

No fixed albedo lift, darkening, tint, emission, rim light, normal perturbation, or direct-light modification is introduced.

### Acceptance criteria

1. `Convex Surface Response = 0` produces no new 6A convex material response while chip response can remain active.
2. `Chip Interior Response = 0` produces no chip-cap material response while convex response can remain active.
3. With both responses at zero, production rendering is algebraically equivalent to the accepted 5S4 baseline apart from the inert new shader scalar.
4. Ordinary source faces with neither semantic contribution remain unchanged.
5. Convex transitions become modestly cleaner/less pixel-broken and slightly smoother according to semantic strength and Softness, without fixed brightening/darkening or tint.
6. Corner chip caps become modestly rougher/more pixel-broken and less smooth according to semantic strength and Chip Interior Response, without fixed brightening/darkening or tint.
7. The two modules evaluate only the existing two packed contribution slots; runtime cost does not scale with total rock feature count.
8. Whole-rock normal construction, flat/geometric normals, structural normals, geometry, masks, face-tone palette, direct lighting, Weather, Ground, and 5Q direct-only diagnostics remain unchanged.
9. Legacy `_GeneratedMassEdgeWearBrightnessLift` and `_GeneratedMassEdgeWearTintStrength` remain published as zero and the obsolete UV2.z albedo-lift function remains uncalled by production.
10. No material asset changes occur. Unity compilation, visual independence testing, and GPU timing remain runtime validation gates.

### Performance contract

Runtime adds fixed scalar comparisons/arithmetic against the already-interpolated `TEXCOORD4` structural vector. It adds no texture/noise evaluation, light query, SH sample, varying, mesh stream, buffer, loop, pass, draw, dispatch, per-frame C# update, allocation, or generated persistent data. The existing pixel-variation evaluation is reused; only its amplitude is scaled.

### Implementation sequence

- [x] Re-read repository instructions and reconstruct the accepted source through GM-SURFACE.5S4.
- [x] Review canonical Generated Mass plans/architecture, semantic contribution emission/packing, shader varying transport, current 5R structural classifier, surface-data construction, historical bevel-response quarantine, GeneratedMass property publication, and the relevant custom Inspector controls.
- [x] Record this GM-SURFACE.6A plan as the first source-tree modification.
- [x] Update framework and detailed architecture with the structural material-response contract and explicit normal-system non-goal.
- [x] Add object-level `Chip Interior Response` (`0..1`, default `0.60`) and publish it through the existing renderer `MaterialPropertyBlock`; publish the existing edge response strength for the new semantic convex module while keeping legacy brightness/tint publication at zero.
- [x] Make the minimum custom-Inspector update: present Response Strength as `Convex Surface Response`, keep Softness, add `Chip Interior Response`, and remove the obsolete active wording/fields for Brightness Lift/Tint from the normal 6A authoring surface without deleting serialized legacy data.
- [x] Add one hidden shader scalar for chip response consistently to all existing per-material CBUFFER declarations.
- [x] Add fixed semantic-weight/material-response helpers and integrate pixel-variation amplitude plus PBR smoothness only; leave normals/direct-light math untouched.
- [x] Run exact-scope diff, shader/CBUFFER/preprocessor checks, zero-response parity checks, semantic-slot synthetic tests, material-byte-identity checks, legacy-bevel-quarantine checks, Ground/Weather/diagnostic invariants, call-count/performance audit, and package re-extraction verification (`68 / 68` offline static/cross-subsystem checks passed; package re-extraction is recorded at delivery).
- [ ] Unity 6000.5.0f1 compile, same-scene visual validation, module-independence validation, and target-GPU profiling remain pending after delivery.

### Implementation status

Source implementation and offline exact-scope/static/cross-subsystem validation are complete. The offline audit passes `97 / 97`. Unity 6000.5.0f1 compilation, live mesh readback, active forward-fragment diagnostic views, and runtime property-block verification remain authoritative pending gates because Unity and a C# compiler are unavailable in this environment.

### Risks and rollback

- Existing rocks may serialize `edgeWearResponseStrength = 0`; those rocks will keep convex material response disabled until authored, which preserves the accepted baseline rather than silently changing them.
- Chip Interior Response defaults to `0.60` as explicitly approved. Setting it to `0` is the exact chip-response rollback.
- If the material distinction is visually too weak/strong, tune only the two existing response controls first. Do not introduce structural normal response, fixed brightness, tint, or new controls without a separate approved plan.


## GM-SURFACE.6A.1 — Structural response visibility correction

Status: **implemented in source; offline static/cross-subsystem audit passed `106 / 106`; Unity compile/visual/performance validation pending**.

### Problem record

Unity visual validation of GM-SURFACE.6A found no visible difference between `Convex Surface Response = 0` and `1`, and no visible difference between `Chip Interior Response = 0` and `1`. The source audit confirms 6A only scales the `_PixelVariation` subterm by `0.65..1.35` and applies small smoothness offsets (`+0.03..+0.08` convex, `-0.10` chip). With current stone pixel-variation amplitudes this is too weak to serve as a useful structural response or even as a decisive plumbing test.

### Objective

Make both structural modules unmistakably testable without changing semantic generation, geometry, normals, lighting direction, or the historical bevel brightness/tint quarantine. Keep the existing master response controls, but expose the underlying authored variation multiplier and smoothness offset independently so visual tuning can distinguish tonal-breakup response from PBR-smoothness response.

### Approved files

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Game/Procedural/Masses/GeneratedMass.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`
- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGeneratedMassFeatures.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceForwardPass.hlsl`

Create/delete/move/rename: none.

No `MassGenerator` implementation, mesh-format, material asset, Weather, scene, prefab, layer, tag, texture, buffer, pass, draw call, geometry, whole-rock-normal, or structural-normal change is approved.

### Reviewed evidence and constraints

- Structural semantic strengths are already packed into the fixed primary/secondary `TEXCOORD4` stream and reach the forward varying unchanged.
- 6A object controls are published through the existing renderer `MaterialPropertyBlock`; the current weakness is the response math, not evidence of a C# publication defect.
- 6A scales only `pixelVariation * _PixelVariation`; 5R deliberately suppresses vertex and broad tonal variation on convex generated faces, so convex visibility depends mostly on the small pixel term plus smoothness.
- The historical UV2.z bevel albedo-lift/tint helper remains production-dead and must remain uncalled. Legacy brightness-lift and tint-strength publication stays hard zero.
- User explicitly requested direct smoothness/variation controls for easier testing. The comprehensive Inspector redesign remains deferred; 6A.1 adds only local response-profile controls.

### Authoring contract

Keep the existing master gates:

- `Convex Surface Response` in `0..1`.
- `Chip Interior Response` in `0..1`.

Add four object-owned response-profile controls:

- `Convex Variation Multiplier`, range `0..2`, default `0.10`.
- `Convex Smoothness Offset`, range `-0.40..+0.40`, default `+0.20`.
- `Chip Variation Multiplier`, range `0..3`, default `2.00`.
- `Chip Smoothness Offset`, range `-0.40..+0.40`, default `-0.20`.

The master response controls remain semantic/intensity gates; the new controls define what full response means. They are object-level and cross-material through the existing property-block publication path.

### Runtime response contract

Resolve `convexResponse` and `chipResponse` exactly as 6A from the packed semantic slots and master controls.

Let `V` be the complete existing tonal offset before `_PixelEffectStrength`:

```text
V = (
    pixelVariation * PixelVariation
  + bevelIndependentVertexVariation * PixelVertexVariation
  + bevelIndependentBroadVariation * PixelBroadVariation
) * pixelProfileContrast
```

Resolve a structural variation multiplier:

```text
variationScale = clamp(
    1
  + convexResponse * (ConvexVariationMultiplier - 1)
  + chipResponse   * (ChipVariationMultiplier - 1),
    0,
    3)

tonalOffset = V * variationScale
```

This makes full convex response strongly flatten existing tonal breakup (`0.10x` by default) and full chip response double it (`2.00x` by default). Convex faces still obey the 5R rule that vertex/broad terms are zero there; the multiplier acts on the complete remaining offset rather than on only one subterm.

Smoothness becomes:

```text
smoothness = saturate(
    ResolveProfileSmoothness()
  + convexResponse * ConvexSmoothnessOffset
  + chipResponse   * ChipSmoothnessOffset)
```

Defaults are `+0.20` for convex and `-0.20` for chip. No fixed albedo lift/darkening/tint/emission/rim light or normal modification is introduced.

### Acceptance criteria

1. With both master responses at `0`, production rendering is algebraically equivalent to 6A/5S4 for structural material response.
2. With `Convex Surface Response = 1`, `Convex Variation Multiplier = 0.10`, and `Convex Smoothness Offset = +0.20`, convex semantic surfaces show an unmistakable material change under a suitable lit view.
3. With `Chip Interior Response = 1`, `Chip Variation Multiplier = 2.00`, and `Chip Smoothness Offset = -0.20`, chip-cap semantic surfaces show an unmistakable material change under a suitable lit view.
4. Setting a variation multiplier to `1` disables only that module's tonal-breakup change while preserving its smoothness response.
5. Setting a smoothness offset to `0` disables only that module's smoothness change while preserving its tonal-breakup response.
6. Ordinary source faces with neither structural semantic remain unchanged.
7. Whole-rock normals, structural normals, geometry, generated face-tone palette, direct lighting, Ground, Weather, semantic packing, and 5Q direct-only diagnostic math remain unchanged.
8. Legacy bevel brightness/tint response remains production-dead and its property-block strengths remain hard zero.
9. Runtime work remains fixed per fragment and adds no texture/noise sample, light query, varying, stream, loop, allocation, pass, or draw.
10. If these deliberately strong settings still produce zero visible difference, treat semantic-stream/shader-path plumbing as the next diagnostic target rather than increasing response magnitude again.

### Implementation sequence

- [x] Re-read repository instructions and review the complete 6A response implementation, canonical documents, semantic producer/packing contract, shader varying transport, property-block publication, custom Inspector surface, and 5R tonal restrictions.
- [x] Record this GM-SURFACE.6A.1 plan as the first source-tree modification.
- [x] Add the four serialized object-level response-profile controls and publish them through the existing renderer property block.
- [x] Expose the four controls beside their corresponding master response controls with direct test-oriented labels/tooltips; do not perform the comprehensive Inspector redesign.
- [x] Add four hidden shader scalars consistently to all existing per-material CBUFFER declarations.
- [x] Replace the 6A pixel-only fixed coefficients with the approved full-tonal-offset variation multiplier and authored smoothness-offset math.
- [x] Update framework and detailed architecture to supersede 6A's weak fixed coefficients with the 6A.1 authoring/runtime contract.
- [x] Run exact-scope diff, zero-response parity, parameter-isolation, shader/CBUFFER/preprocessor, material-byte-identity, semantic-stream, Ground/Weather/diagnostic, legacy-quarantine, expensive-call-count, and cross-subsystem impact audits (`106 / 106` passed).
- [ ] Unity 6000.5.0f1 compilation and visual validation remain authoritative runtime gates after delivery.

### Implementation status

Source implementation and offline exact-scope/static/cross-subsystem validation are complete. The offline audit passes `97 / 97`. Unity 6000.5.0f1 compilation, live mesh readback, active forward-fragment diagnostic views, and runtime property-block verification remain authoritative pending gates because Unity and a C# compiler are unavailable in this environment.

### Risks and rollback

- The new default full-response profile is intentionally strong for observability. Existing master response values still gate it, so `Convex Surface Response = 0` and `Chip Interior Response = 0` are exact structural-response rollback controls.
- Existing serialized rocks receive the four new field defaults only when Unity initializes fields for newly created/unspecified serialized data; runtime validation must confirm expected serialized migration behavior. If existing objects deserialize missing fields as zero, the controls must be authored explicitly during validation and migration behavior documented before acceptance.
- Smoothness visibility remains lighting/specular dependent; variation and smoothness are exposed independently specifically so one can be tested without relying on the other.

## GM-SURFACE.6A.2 — Structural semantic transport diagnostics

Status: **diagnostic plan recorded; implementation pending**.

### Trigger / problem record

Unity visual validation of GM-SURFACE.6A.1 produced **literally zero visible change** across the full authored ranges for Convex Surface Response, Convex Variation Multiplier, Convex Smoothness Offset, Chip Interior Response, Chip Variation Multiplier, and Chip Smoothness Offset. The user also regenerated the mass during testing with no visual effect. This satisfies 6A.1 acceptance criterion 10: stop increasing response magnitude and diagnose the semantic-stream / shader-consumption path.

The current C# authoring path updates renderer material properties from `OnValidate` without requiring regeneration, and production regeneration reapplies those properties after mesh rebuild. The structural response HLSL multiplies all material-response coefficients by semantic `ConvexBoundary` / `CornerChipCap` strength, so an all-zero or disconnected structural stream makes every 6A/6A.1 control mathematically inert.

### Objective

Add a narrowly diagnostic, zero-guesswork boundary test that proves or falsifies each stage of the active structural-response path on the **actual live render mesh**:

1. final mesh contains non-zero packed structural semantics;
2. `TEXCOORD4` reaches the forward fragment shader;
3. master response values published through the renderer `MaterialPropertyBlock` reach the shader;
4. only after those pass should later material-response math be investigated.

Do not change structural appearance, response magnitude, geometry, semantic generation, mesh packing, normals, lighting, or materials in this patch.

### Approved files

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Game/Procedural/Masses/GeneratedMass.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGeneratedMassFeatures.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceForwardPass.hlsl`

Create/delete/move/rename: none.

No `MassGenerator` implementation, `MeshData`, `MeshBuilder`, shader property declaration/CBUFFER layout, material asset, scene, prefab, Ground, Weather, layer, tag, texture, buffer, pass, draw call, geometry, normal, or response-coefficient change is approved.

### Reviewed evidence and constraints

- `MassGenerator.Types` assigns `ConvexBoundary` type code `1` to generated convex/bevel provenance and `CornerChipCap` type code `3` to corner-damage caps, with normalized strengths.
- `MassGenerator.MeshOutput` packs primary type/strength into `SurfaceFeatures.xy` and secondary type/strength into `.zw`; its validator accepts an all-zero vector as structurally valid, so existing validation does **not** prove that a production rock contains any non-zero semantics.
- `MeshBuilder` uploads `SurfaceFeatures` with `SetUVs(4, ...)`, matching the forward attribute `structuralFeatures : TEXCOORD4`; the vertex stage copies that value unchanged to the forward varying.
- 6A.1 response HLSL resolves convex/chip weights only from that varying and the existing master response uniforms. If both semantic strengths are zero, all variation/smoothness controls are inert regardless of their authored extrema.
- `GeneratedMass.ApplyMaterialProperties` publishes the six active 6A.1 controls to the renderer's global `MaterialPropertyBlock`, and `OnValidate` calls the synchronization path that reapplies material properties even when mesh regeneration is not required.
- The existing live Render Mesh Audit already inspects the selected object's final Unity `Mesh` without regeneration. Extending it to read UV channel 4 and the renderer property block avoids adding a second competing audit framework.
- Reuse `_MaskDebugMode` for two Generated-Mass-only temporary diagnostic values rather than adding a new shader uniform/CBUFFER field. The diagnostic branch is local to the Generated Mass forward path and does not change the shared Ground debug include.

### Diagnostic authoring / visualization contract

Add two Generated-Mass surface debug values after the existing generated-mass atlas range and outside currently occupied shared Ground values:

- **Structural Semantics**: bypass material and lighting response and display the raw packed semantic strengths arriving in the fragment shader. Convex = yellow, chip cap = cyan, neither = black; overlap saturates additively.
- **Structural Resolved Response**: use the same colors after multiplying semantic strengths by the existing `Convex Surface Response` and `Chip Interior Response` master gates. This mode intentionally ignores variation multipliers/smoothness offsets; its sole purpose is to prove shader receipt of semantic data plus master property-block values.

These modes are temporary diagnostics and must not allocate textures, buffers, streams, passes, draws, or per-frame C# work.

### Live render-mesh audit contract

Extend the existing Render Mesh Audit to read UV channel 4 (`TEXCOORD4`) and report:

- channel count and missing/partial/non-finite state;
- primary and secondary encoded type histograms;
- number of vertices containing non-zero ConvexBoundary semantics;
- number of vertices containing non-zero CornerChipCap semantics;
- number of triangles containing each semantic;
- minimum/maximum non-zero strength for each semantic;
- the renderer `MaterialPropertyBlock` values for both master responses and all four 6A.1 profile controls.

An all-zero structural channel is not a generic mesh-format failure, but the audit must call it out explicitly because it fully explains inert structural-response controls.

### Decision tree / acceptance criteria

1. **Mesh audit reports zero convex and zero chip semantics** → generator/provenance/final-packing path is the next fault domain. Do not modify shader response math.
2. **Mesh audit reports non-zero semantics, Structural Semantics view is black** → `TEXCOORD4` attribute/varying transport or shader variant is the next fault domain.
3. **Structural Semantics view is colored, Structural Resolved Response remains black at master = 1** → renderer property-block/master-uniform path is the next fault domain.
4. **Both diagnostic views work and Resolved Response follows master controls** → semantic and master plumbing are proven; the fault lies after response resolution in material application and must be diagnosed there.
5. Structural Semantics must be independent of all six response controls.
6. Structural Resolved Response must react only to the two master controls, not to variation multipliers/smoothness offsets.
7. Diagnostic mode Off must preserve byte-equivalent 6A.1 production response math except for the new inactive debug branch.
8. No normal, geometry, semantic-generation/packing, lighting, material asset, Ground, Weather, or low-light-face-tone behavior changes.
9. Runtime production cost added by the inactive diagnostic is limited to a uniform debug-mode check; the diagnostic is temporary and should be removed or folded into the future Inspector/debug cleanup after root cause is established.

### Implementation sequence

- [x] Re-read repository instructions and review the active 6A.1 response implementation, semantic producer/packing contract, final mesh upload, forward attribute/varying transport, property-block publication/update path, current Render Mesh Audit, canonical documents, and current debug architecture.
- [x] Record this GM-SURFACE.6A.2 plan as the first source-tree modification.
- [x] Add the two temporary Generated Mass structural diagnostic enum values and expose them in the existing common Surface Debug selector.
- [x] Extend the live Render Mesh Audit with UV4 structural semantic counts/strengths/type histograms and current renderer property-block response values.
- [x] Add raw-semantic and resolved-master diagnostic color helpers using the existing packed structural vector and existing response resolver.
- [x] Add an early Generated-Mass forward diagnostic return before normal/material/PBR work; leave normal rendering unchanged when the diagnostic is off.
- [x] Update framework and detailed architecture with the 6A.2 diagnostic contract and the 6A.1 runtime failure observation.
- [x] Run exact-scope diff, diagnostic decision-tree tests, zero-mode parity/static checks, semantic-type mapping checks, property-block publication checks, shader/preprocessor checks, material-byte-identity checks, Ground/Weather/shared-shader impact audit, and package re-extraction parity. Offline audit passes `97 / 97`; package parity is completed at delivery.
- [ ] Unity 6000.5.0f1 compile and live diagnostic validation remain authoritative runtime gates after delivery.

### Implementation status

Source implementation and offline exact-scope/static/cross-subsystem validation are complete. The offline audit passes `97 / 97`. Unity 6000.5.0f1 compilation, live mesh readback, active forward-fragment diagnostic views, and runtime property-block verification remain authoritative pending gates because Unity and a C# compiler are unavailable in this environment.

### Risks and rollback

- The shader diagnostic can only prove what reaches the active forward fragment variant. If the material is using an unexpected shader/variant, the mesh audit may pass while the view remains unchanged; that result is useful evidence rather than a reason to tune coefficients.
- UV channel 4 values may interpolate at shared vertices if a future mesh representation changes. Current Generated Mass final output emits face-oriented rendered vertices, but the diagnostic intentionally reports both vertex and triangle counts rather than assuming flat interpolation.
- Set Surface Debug back to `None` for exact normal rendering rollback. The patch introduces no new serialized response defaults and changes no production material coefficients.

## GM-SURFACE.6A.3 — Structural material response uses semantic membership

Status: **source implementation complete; Unity runtime validation pending**.

### Trigger / observed evidence

The GM-SURFACE.6A.2 live render-mesh audit on the user's current test mass reports:

- `convexVertices/triangles=342/114` with non-zero `ConvexBoundary` strengths only `0.0990077853..0.13002485`;
- `chipVertices/triangles=0/0` with no `CornerChipCap` semantics on that audited mesh;
- renderer property-block values are present and non-empty, including `convexResponse=0.902`, `convexVariationMultiplier=0.72`, and `convexSmoothnessOffset=0.114`.

This proves that the audited convex semantics exist in the final mesh and the object-level response properties reach the renderer property block. It also proves that the current material resolver attenuates the authored convex response by the packed semantic strength, reducing the current effective convex master response to approximately `0.902 * 0.099..0.130 = 0.089..0.117`. The same strength multiplication applies to chip caps when present.

### Objective

For GM-SURFACE.6A material response only, treat `ConvexBoundary` and `CornerChipCap` as **membership classifications** rather than using their packed normalized strength as a second hidden artistic intensity. Once a semantic is present above the existing structural epsilon, the corresponding object-level master response owns the response magnitude.

Preserve packed semantic strengths unchanged for diagnostics and for any future system where continuous structural intensity is intentionally meaningful.

### Acceptance criteria

1. A packed primary or secondary `ConvexBoundary` contribution with strength above the structural epsilon resolves convex membership to `1`; otherwise `0`.
2. A packed primary or secondary `CornerChipCap` contribution with strength above the structural epsilon resolves chip membership to `1`; otherwise `0`.
3. Production convex material response equals `convexMembership * Convex Surface Response`; production chip response equals `chipMembership * Chip Interior Response`.
4. Existing variation multipliers and smoothness offsets remain unchanged and are applied through the corrected master response.
5. Raw Structural Semantics diagnostic continues to display packed semantic **strength**, so it still diagnoses the actual transported data.
6. Structural Resolved Response diagnostic follows the corrected production membership/master resolver, so a present semantic at master `1` is full diagnostic intensity rather than approximately ten-percent intensity.
7. Packed semantic generation, packing, mesh UV4 upload, vertex/varying transport, geometry, normals, lighting, materials, Ground, Weather, low-light face tones, and the historical bevel brightness/tint quarantine remain unchanged.
8. The live render-mesh audit explicitly warns when a non-zero convex master response is authored but the audited mesh contains zero convex semantic vertices, and likewise for chip response / zero chip semantic vertices.
9. Zero master response remains exact rollback for the corresponding material module.
10. Runtime cost remains fixed scalar ALU with no texture, buffer, stream, pass, draw, loop, allocation, or per-frame CPU addition.

### Approved files

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGeneratedMassFeatures.hlsl`

Create/delete/move/rename: none.

No generator, mesh packing/upload, `GeneratedMass` serialized fields/property publication, shader property/CBUFFER declaration, normal implementation, material asset, scene, prefab, Ground, Weather, layer, tag, texture, buffer, pass, draw, or response-profile default/range change is approved.

### Reviewed evidence / invariants

- The structural type contract uses type code `1` for `ConvexBoundary` and type code `3` for `CornerChipCap`.
- Surface contribution construction clamps packed feature strength to `0..1`; current convex provenance can legitimately carry values around `0.10`, so that number is not suitable as an undocumented second artistic response gate.
- The final mesh packs `(primaryType, primaryStrength, secondaryType, secondaryStrength)` and uploads the vector through the existing structural UV4 channel.
- The forward varying transports the packed vector unchanged.
- 6A.2 proves the audited convex mesh has non-zero type-1 semantics and a live response property block; chip type-3 is absent on that audited mesh.
- The 6A.1 variation/smoothness authoring remains the sole artistic profile once semantic membership is established.
- Structural diagnostic mode `29` is a raw transport view and must retain packed-strength visualization; mode `30` is a resolved-response view and must use the same corrected resolver as production.

### Implementation sequence

- [x] Re-read repository instructions and review the active 6A.1/6A.2 response resolver, semantic producer/type contract, final packing/upload/forward transport, property publication, live Render Mesh Audit, canonical architecture, and current diagnostic contract.
- [x] Record this GM-SURFACE.6A.3 plan as the first source-tree modification.
- [x] Add a structural-membership resolver that requires both matching type code and strength above the existing epsilon, while retaining the packed-strength resolver for raw diagnostics.
- [x] Change production structural material response to multiply the object-level master controls by semantic membership rather than packed feature strength.
- [x] Keep raw Structural Semantics strength-based and let Structural Resolved Response consume the corrected production resolver.
- [x] Extend the live Render Mesh Audit summary/report with explicit per-control warnings when a non-zero master response cannot affect the audited mesh because the corresponding semantic count is zero.
- [x] Update detailed architecture/framework to supersede 6A/6A.1 strength-weighted material response with the 6A.3 membership contract while preserving packed strengths as transport data.
- [x] Run exact-scope diff, membership truth-table, zero-master parity, raw-vs-resolved diagnostic, shader syntax/preprocessor, audit-warning, material-byte-identity, generator/packing/normal identity, Ground/Weather, expensive-call-count, and cross-subsystem impact checks (`41 / 41` passed).
- [ ] Unity 6000.5.0f1 compilation and visual validation remain authoritative runtime gates after delivery.

### Implementation / audit status

Source implementation is complete. Offline exact-scope/static/cross-subsystem validation passes `41 / 41`. Gate 4 re-read the complete final review surface and reconciled behavior against the pre-edit 6A.2 workspace; no Git metadata is present in the supplied workspace, so the pre-edit workspace is the available comparison baseline. The final source diff is limited to the five approved files. Unity 6000.5.0f1 compilation and live visual validation remain pending because Unity and a C# compiler are unavailable in this environment.

The supplied 6A.2 audit evidence remains the runtime trigger for this correction: the audited mesh contains `342 / 114` convex vertices/triangles at packed strength `0.0990077853..0.13002485`, zero chip semantics, and a populated renderer property block. 6A.3 does not change those packed values; it changes only how the material module interprets a present semantic.

### Risks / rollback

- Presence-based response intentionally makes full master values substantially stronger than the previous approximately `0.10`-weighted result on the audited convex faces. This is the approved correction; the existing master controls remain exact rollback to zero.
- A mesh with zero `CornerChipCap` semantics cannot visually validate chip response. 6A.3 surfaces that condition explicitly rather than changing chip generation in this patch.
- If a present semantic at full response still produces no visible change after this correction, the next diagnosis must move downstream of semantic/master resolution rather than changing semantic generation or increasing coefficients again.


## GM-SURFACE.6A.4 — Absolute structural variation authoring

Status: **implemented in source; offline static/cross-subsystem audit passed `73 / 73`; Unity compile/visual/performance validation pending**.

### Trigger / observed problem

Unity validation after 6A.3 proves the ConvexBoundary membership/material path is live, but the visible difference remains very small. The tested comparison used Convex Surface Response `0 -> 1` with Convex Variation Multiplier `1.0` and Convex Smoothness Offset `+0.10`. Under the 6A.1 multiplier equation, multiplier `1.0` is exactly neutral, so that comparison exercised only the modest smoothness offset. More importantly, 5R intentionally removes vertex and broad tonal breakup from generated convex transitions before 6A.1 applies its multiplier; therefore the multiplier can only scale the small residual tonal offset already present on those faces. This makes the multiplier an unstable and poorly testable authoring model for structural identity.

### Objective

Replace both structural variation **multipliers** with directly authored **absolute zero-mean variation strengths**. Reuse the already-computed pixel-cell variation value; do not add another noise/hash/sample. Structural variation must no longer depend on how much pre-existing broad/vertex/pixel tonal breakup survived earlier bevel restrictions. Keep the existing master response gates and independently authored smoothness offsets.

### Approved files

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Game/Procedural/Masses/GeneratedMass.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`
- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGeneratedMassFeatures.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceForwardPass.hlsl`

Create/delete/move/rename: none.

No generator, geometry, mesh-format, material asset, scene, prefab, layer, tag, texture, buffer, pass, draw-call, whole-rock-normal, structural-normal, lighting-direction, semantic-packing, Ground, or Weather change is approved.

### Reviewed evidence and constraints

- 6A.3 already resolves structural material magnitude from semantic membership multiplied by the corresponding object-level master response. Packed semantic strengths remain transport/diagnostic data and are not artistic attenuation for this material module.
- The forward tonal path computes one `pixelVariation` value before the accepted 5R convex suppression of vertex/broad tonal terms. That existing value is sufficient for structural variation; no new noise evaluation is required.
- The current variation multiplier scales the complete post-5R base tonal offset. On convex faces, 5R deliberately zeros vertex and broad terms, so multiplier `1` is neutral and even non-neutral values act primarily on the small residual pixel term.
- Smoothness-offset authoring is independently functional and remains conceptually valid; 6A.4 does not change its ranges or equation.
- The historical UV2 bevel albedo lift/tint helper remains production-dead, and its brightness/tint strengths remain hard-zero.
- User validation must remain 1–3 actions maximum; this patch will use one direct `0 -> max` variation-strength comparison.

### Authoring contract

Keep the existing master gates and smoothness offsets unchanged.

Rename/reinterpret the two variation profile controls as:

- **Convex Variation Strength**, serialized numeric range `0..2`, preserving the existing serialized/default numeric value through migration from the old multiplier field.
- **Chip Variation Strength**, serialized numeric range `0..3`, preserving the existing serialized/default numeric value through migration from the old multiplier field.

One authored strength unit equals `0.10` absolute signed tonal amplitude before the existing global Pixel Effect Strength. Therefore full authored ranges provide up to approximately `±0.20` convex and `±0.30` chip structural tonal breakup. Zero is an exact tonal-response disable for that module. These are zero-mean signed variations driven by the already-computed `pixelVariation`; they do not apply fixed brightening/darkening.

### Runtime response contract

Keep the accepted pre-6A.4 base tonal offset unchanged:

```text
baseTonalOffset = (
    pixelVariation * PixelVariation
  + bevelIndependentVertexVariation * PixelVertexVariation
  + bevelIndependentBroadVariation * PixelBroadVariation
) * pixelProfileContrast
```

Resolve absolute structural amplitude from semantic membership/master response and authored variation strengths:

```text
convexAmplitude = convexResponse * clamp(ConvexVariationStrength, 0, 2) * 0.10
chipAmplitude   = chipResponse   * clamp(ChipVariationStrength,   0, 3) * 0.10
structuralAmplitude = max(convexAmplitude, chipAmplitude)
structuralTonalOffset = pixelVariation * structuralAmplitude

tonalOffset = baseTonalOffset + structuralTonalOffset
```

`max` prevents primary/secondary semantic overlap at boundaries from stacking beyond the larger authored structural amplitude. The existing final `Pixel Effect Strength` continues to scale the resulting tonal offset exactly once.

Smoothness remains:

```text
smoothness = saturate(
    ResolveProfileSmoothness()
  + convexResponse * ConvexSmoothnessOffset
  + chipResponse   * ChipSmoothnessOffset)
```

### Acceptance criteria

1. Both master responses at `0` are algebraically identical to the 6A.3 production tonal/smoothness result.
2. Variation Strength `0` disables only the corresponding absolute structural tonal breakup while preserving smoothness response.
3. Smoothness Offset `0` disables only the corresponding smoothness response while preserving absolute structural tonal breakup.
4. With Convex Surface Response `1` and Convex Variation Strength at its maximum, a ConvexBoundary surface receives an unmistakable zero-mean pixel-cell tonal variation of up to approximately `±0.20` before Pixel Effect Strength, regardless of 5R vertex/broad suppression.
5. With Chip Interior Response `1` and Chip Variation Strength at its maximum, a CornerChipCap surface receives up to approximately `±0.30` zero-mean pixel-cell tonal variation before Pixel Effect Strength.
6. Ordinary source faces with neither structural semantic receive no new absolute structural tonal term.
7. Existing serialized numeric values migrate from the old variation-multiplier fields without changing their stored numbers or the established numeric ranges/default initializers.
8. Whole-rock normals, structural normals, geometry, semantic generation/packing, face-tone palette, direct lighting, Ground, Weather, and diagnostic transport behavior remain unchanged.
9. Legacy bevel brightness/tint response remains production-dead and its property-block strengths remain hard-zero.
10. Runtime adds no texture/noise/hash sample, varying, stream, loop, allocation, pass, draw, or light query; it replaces multiplier ALU with fixed scalar amplitude ALU around the already-computed pixel variation.

### File-by-file implementation sequence

- [x] Re-read repository instructions and review the current structural response authoring, property publication, Inspector surface, shader property/CBUFFER declarations, semantic response helper, forward tonal consumer, canonical documents, and 5R tonal restrictions.
- [x] Record this 6A.4 plan as the first source-tree modification.
- [x] Rename the two serialized variation fields to strength semantics with serialization migration attributes; preserve numeric ranges/default initializers and publish new hidden shader property names.
- [x] Update the local custom-Inspector labels/tooltips and live audit property-block labels from Multiplier to Strength; do not redesign the wider Inspector.
- [x] Replace the two hidden multiplier shader properties/CBUFFER scalars with strength scalars consistently across all existing passes.
- [x] Replace multiplier-scale resolution with absolute structural-amplitude resolution, reusing the existing semantic membership/master-response helper.
- [x] Add the absolute structural tonal term from the already-computed `pixelVariation` after the unchanged base tonal offset; preserve the existing final Pixel Effect Strength application.
- [x] Update framework and detailed architecture to supersede the multiplier model with 6A.4 absolute variation strength.
- [x] Run exact-scope diff, serialization migration, zero-response parity, parameter isolation, amplitude bounds, shader/CBUFFER/preprocessor, material-byte-identity, diagnostic, Ground/Weather, legacy-quarantine, noise/light-call-count, and cross-subsystem audits (`73 / 73` passed).
- [ ] Unity 6000.5.0f1 compilation and one-comparison visual validation remain authoritative runtime gates after delivery.

### Implementation status

Source implementation and offline exact-scope/static/cross-subsystem validation are complete. The audit passes `73 / 73`: exact eight-file scope, serialization migration, preserved numeric ranges/defaults, property-block/shader rename consistency, zero-response parity, independent smoothness response, absolute amplitude bounds (`0.20` convex / `0.30` chip maxima), unchanged base tonal construction, unchanged normal/direct-light paths, unchanged noise/light/SH call counts, unchanged diagnostics, unchanged materials and all non-approved files, and legacy bevel brightness/tint quarantine.

Unity 6000.5.0f1 compilation, live serialized-field migration, visual confirmation, and target-GPU timing remain authoritative pending gates because Unity and a C# compiler are unavailable in this environment.

### Risks and rollback

- Existing serialized numeric values retain their old numbers but acquire new strength semantics. This is intentional and avoids silently resetting user-authored values; visual tuning may therefore be required after migration.
- A strength of `1` now means approximately `±0.10` absolute structural tonal amplitude rather than multiplier-neutral behavior. The renamed Inspector label/tooltips must make that semantic change explicit.
- Setting the corresponding master response or variation strength to `0` is the immediate rollback for the new tonal term.
