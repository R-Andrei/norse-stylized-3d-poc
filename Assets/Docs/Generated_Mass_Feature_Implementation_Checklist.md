# Generated Mass Feature Implementation Checklist

Status: active concise checklist

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
