# Generated Mass Production Geometry and Surface-Response Architecture

Status: canonical
Architecture ID: GM-SURFACE.4

## Scope

This document owns the current Generated Mass decisions for production bevel/chip geometry, mesh normals, whole-rock normal detail, feature-localized surface responses, memory limits, shader limits, and future feature integration.

Repository code overrides this document if they differ; such disagreement is a defect.

## Current implementation state

GM-SURFACE.5 adds the first shared whole-rock normal layer. It is a bounded four-sample object-space procedural gradient evaluated only for the Generated Mass surface contract. Each rock supplies only strength and scale through its existing material property block. The layer allocates no unique textures, performs no feature loop, and is independent of the primary/secondary structural feature stream. Convex and chip-specific modules remain deferred to GM-SURFACE.6.


GM-SURFACE.2 promotes the certified bevel and corner-chip builder into ordinary production generation. `MassGenerator.Generate(...)` now resolves enabled structural settings to `ProductionSurfaceFeatures`; corner-chip production uses the same certified chip-first integration and ranked ordinary-bevel reduction used by the validated diagnostic path. `BaseGeometryOnly` remains only the zero-feature and safe-fallback route. Preview actions collect evidence around the same builder and no longer own whether the final rock contains the geometry.

## Final production contract

Generated Mass rendering is composed from:

1. certified structural geometry;
2. correct geometric mesh normals;
3. shared triplanar whole-rock normal detail;
4. at most two compact mesh-local structural feature contributions per fragment.

No per-rock feature atlas or generated per-rock normal map is permitted.

## Geometry ownership

Geometry owns features that materially affect silhouette, parallax, shadows, collision, contact shape, or large-scale lighting. This includes ordinary bevels, corner chips, deep fractures, large dents, and major concave forms.

Surface response owns grain, pits, shallow creases, erosion breakup, fine fracture texture, weathering, coatings, and other sub-triangle detail.

Mesh normals describe final geometry. They must not be distorted to fake full-surface stone texture.

## Production geometry flow

The target ordinary generation path is:

```text
base mass
→ certified corner chips
→ post-chip topology rebuild
→ ranked ordinary bevel selection
→ one-loser-per-retry compatibility reduction
→ certified final geometry
→ final provenance and mesh normals
→ packed feature contributions
→ final Unity Mesh
```

Corner chips have priority over ordinary bevel preservation. Zero surviving ordinary bevels is valid. Production fallback is deterministic:

```text
chips + certified bevel subset
→ certified chips with zero bevels
→ certified base geometry
```

## Generic feature-contribution contract

GM-SURFACE.3 establishes the generation-time semantic contribution model in `MassGenerator.Types.cs`. Every final triangle-soup vertex now carries deterministic primary and secondary contribution slots alongside existing feature, authored-normal, and authored-surface-group data.

Each contribution currently contains semantic type, response role, deterministic feature identity, strength, direction, and priority. Spatial width and cross-feature coordinates remain intentionally deferred to the packed-channel design because the current production geometry does not yet expose one canonical representation for them.

Initial semantic feature types:

- `ConvexBoundary`
- `ConcaveBoundary`
- `CornerChipCap`
- `Fracture`
- `ImpactDent`
- `MaterialSeam`

Artistic variants are parameters, not new semantic types.

Current emitters map certified convex bevel provenance to `ConvexBoundary` and corner-damage cap provenance to `CornerChipCap`. The model reserves the remaining semantic types without pretending they are implemented.

Generation resolves contributions into at most two local structural slots. Major damage outranks ordinary convex/concave structural response; exact-priority ties use semantic type and feature identity. Clone/copy paths preserve both slots explicitly. Runtime shading never loops through an arbitrary feature list.

## Packed mesh-local fields

GM-SURFACE.4 establishes the first production packed stream in `TEXCOORD4`. `TEXCOORD3` remains untouched because the retired diagnostic feature-atlas path still writes there and must not silently collide with production data.

The fixed `Vector4` layout is exactly 16 bytes per final render vertex:

```text
X = primary MassSurfaceFeatureType integer code
Y = primary normalized strength
Z = secondary MassSurfaceFeatureType integer code
W = secondary normalized strength
```

Response role is derived from semantic type. Current response direction is derived from the final render/geometric normal. Feature identity, priority, and generation-only provenance are not persisted. This keeps the payload at the preferred ceiling rather than storing values the first shader modules do not need.

Both `MeshData.Validate()` and the final Unity `Mesh` channel audit require a complete finite stream, legal integer type codes, normalized strengths, and zero strength for empty slots. Exact mesh-data comparisons now include the structural stream.

When more than two features overlap, deterministic generation chooses or combines the two highest-value compatible structural responses. Priority order:

1. geometric structural transition;
2. major damage or exposed surface;
3. convex/concave structural response;
4. coating or accumulation;
5. shared whole-rock micro detail.

## Whole-rock normal response

Every rock may use shared material resources for continuous stone response:

- macro triplanar normal detail;
- optional micro triplanar normal detail;
- deterministic per-rock scale, offset, orientation, and seed variation.

Shared textures are loaded once per material family. Low quality may use one triplanar layer; standard quality may use macro plus micro layers.

## Shader performance contract

- zero per-rock feature textures;
- zero arbitrary feature loops;
- zero scene-wide topology lookup;
- fixed evaluation of zero, one, or two structural contributions;
- no extra renderer or material pass per feature;
- shared materials and SRP Batcher compatibility;
- no per-frame feature generation.

Shader cost after mesh generation must not scale with the total number of source features on the rock.

## Memory contract

At 2,000 vertices per rock and 2,000 fully retained meshes:

- 8 bytes/vertex: approximately 30.5 MiB;
- 16 bytes/vertex: approximately 61 MiB;
- 24 bytes/vertex: approximately 91.5 MiB.

At 150 resident meshes:

- 8 bytes/vertex: approximately 2.3 MiB;
- 16 bytes/vertex: approximately 4.6 MiB;
- 24 bytes/vertex: approximately 6.9 MiB.

Heavy generation records should be discarded or compacted after final mesh construction.

## Rejected directions

The following are explicitly rejected:

- per-rock feature atlases;
- generated per-rock normal maps;
- permanent texture-array slices per rock;
- virtual atlas storage that scales with every rock;
- unbounded per-pixel feature-record loops;
- one renderer or material pass per feature;
- mesh-normal deformation as a substitute for stone texture;
- an edge-wear-only normal architecture;
- persistent per-rock feature databases;
- keeping certified bevel/chip geometry permanently behind preview actions.

## Implementation sequence

1. GM-SURFACE.1 — documentation consolidation and explicit production/diagnostic routing boundary.
2. GM-SURFACE.2 — production bevel/chip promotion through the certified builder. **Completed.**
3. GM-SURFACE.3 — semantic feature-contribution model. **Completed.**
4. GM-SURFACE.4 — packed primary/secondary mesh channels. **Completed.**
5. GM-SURFACE.5 — shared whole-rock triplanar normal response.
6. GM-SURFACE.6 — convex and chip response modules.
7. GM-SURFACE.7 — concave-crease integration through the same contract.

## Documentation authority

- This file owns production geometry and surface response.
- `Generated_Mass_Incremental_Selection_Architecture.md` owns frozen candidate selection.
- `Generated_Mass_Framework.md` owns the module-level overview.
- `Generated_Mass_Feature_Implementation_Checklist.md` owns active/completed work status only.
- Historical recovery detail must not present superseded states as current architecture.


## GM-SURFACE.5A — reference direction and lighting-foundation correction

The accepted visual direction is stylized authored rock: large readable planar masses, softened real convex transitions, narrow selective edge accents, localized concave seams, restrained whole-face undulation, exposed fractured interiors, and semantic weathering. Generic high-frequency noise is not the target.

GM-SURFACE.5A establishes these current rules:

- Whole-rock normal strength is true slope amplitude. `0` is geometric normals only; `1` is deliberately exaggerated and must be unmistakable.
- The shared field contains broad undulation plus subordinate medium breakup, remains object-space attached, and uses no per-rock textures.
- Convex bevels and corner-chip caps use their real geometric normals for illumination.
- Legacy broad exposure, crevice/base, and dirt masks are suppressed on generated convex and chip surfaces so those surfaces do not read as independently painted dark bands.
- Untouched source faces keep the existing material-mask contract.
- GM-SURFACE.6 owns explicit narrow convex accents and exposed-chip artistic treatment; GM-SURFACE.5A does not fake those effects through the broad masks.
- Full adjacent-source mask interpolation remains a future refinement if the semantic suppression policy proves insufficient; no extra vertex stream is introduced speculatively.

## GM-SURFACE.5B — shadow readability and convex transition correction

GM-SURFACE.5B addresses two visual defects confirmed against the canonical
stylized-rock references:

1. broad procedural normal relief was readable under direct illumination but
   nearly disappeared on shaded faces;
2. ordinary convex bevel strips could still read as uniformly dark inserted
   bands even after legacy material-mask suppression.

The production correction remains atlas-free and fixed-cost.

### Shaded-face normal readability

The whole-rock perturbed normal now contributes a restrained multiplicative
hemispherical value difference relative to the unperturbed geometric normal.
This preserves faint surface relief in shade without adding emission or making
unlit faces self-illuminated. `Normal Strength = 0` remains exact parity because
the modulation is based on the difference between perturbed and geometric
normal response.

### Convex rounded-normal proxy

The current final render topology duplicates vertices per triangle and does not
yet persist both adjacent source-face normals for each bevel vertex. Ordinary
convex contributions therefore receive a deterministic object-space ellipsoid
normal proxy blended with their generated face normal. The proxy varies over
the bevel surface, prevents one constant dark-band value, and requires no new
mesh stream. It applies only to `ConvexBoundary`; source faces remain planar and
corner-chip caps remain faceted.

This is the bounded production correction for the present data contract. A
future exact adjacent-source-normal interpolation is permitted only if visual
evidence proves the proxy insufficient and the additional stream cost is
explicitly approved.

### Minimal convex readability

Convex contributions receive a restrained positive value lift proportional to
light already present in the PBR result. The response is multiplicative and
non-emissive; it must not become a glowing outline in shadow. The full artistic
convex-edge module remains GM-SURFACE.6 work.


## GM-SURFACE.5C — Generated-face material-mask inheritance

The dark-band bevel defect was confirmed to be a pre-light material-mask ownership defect, not a mesh-normal defect. Generated bevel and chip faces must not recompute Exposure, Crevice/Base, or DirtDeposit from their own new face normal, and the shader must not suppress those masks as a compensation. During final mesh emission, generated triangles now inherit the source-face material-mask samples present on their shared boundaries. Shader-side convex/chip mask suppression and the GM-SURFACE.5B convex lighting compensation are retired. Whole-rock normal response remains independent.

## GM-SURFACE.5D — Compiled generated-face material masks

GM-SURFACE.5C was incomplete in two concrete ways: it assigned one triangle-wide average to every generated-face vertex, and the Pixel Surface forward pass still recomputed Generated Mass crevice/base and dirt from generated-face orientation. GM-SURFACE.5D retires that behaviour. Exact source-boundary samples are now preserved per generated vertex; interior generated vertices interpolate only from resolved boundary vertices on their own triangle; generated caps without a source-boundary sample keep their generation-time compiled channels. The Generated Mass forward path consumes `Color.g`, `Color.b`, and `UV2.y` directly for exposure, crevice/base, and dirt. Generated-face normals affect lighting, not pre-light material classification.

## GM-SURFACE.5E — raw lighting parity baseline

Generated Mass production rendering currently uses raw `UniversalFragmentPBR` output as the lighting authority while bevel-lighting parity is validated against the older `SG_PixelSurfaceLit` material.

For Generated Mass fragments, the parity baseline deliberately bypasses:

- `ApplyGeneratedMassGeometryEdgeWearResponse` bevel albedo lift/tint;
- normal-dependent `ApplyStylizedValueShaping` before PBR;
- post-PBR lighting-colour neutralization/reconstruction;
- the GM-SURFACE.5B shadow-side normal-readability multiplier.

The whole-rock normal field remains available, but parity validation is performed with its strength set to zero. Compiled exposure, crevice/base and dirt channels remain part of `ResolvePixelSurfaceColor`; this patch does not alter geometry, mesh normals, tangents, masks or structural feature streams.

The quarantined bevel-response properties remain serialized for compatibility but are published to the production renderer as zero. They must not be re-enabled until raw bevel lighting is confirmed and any later convex accent is designed as an explicit, light-aware artistic layer rather than a replacement for geometric lighting.

## GM-SURFACE.5F-DIAG — bevel-shading evidence suite

Before any further geometry, normal, mask, or shader correction, the selected
Generated Mass can be audited through:

```text
Generated Mass Inspector → Mesh Diagnostics → Run Bevel-Shading Evidence Suite
```

The suite is read-only, incremental across Editor updates, cancellable, and
checkpoints its copyable report to:

```text
Library/GeneratedMassBevelShadingDiagnostic.txt
```

At completion the full report is also copied to the system clipboard. The suite
audits the final rendered mesh rather than assuming construction intent. It
covers:

- structural `TEXCOORD4` classification of convex and chip triangles;
- connected convex-strip reconstruction from final mesh topology;
- stored-normal and geometric-normal clusters within each strip;
- stored-normal versus geometric-normal agreement;
- adjacent non-convex parent-normal evidence recoverable from final topology;
- compiled Exposure, Crevice/Base, and DirtDeposit ranges within each strip;
- internal material-mask discontinuities;
- controlled Lambert-response comparisons against adjacent parent samples,
  including the active directional-light direction when one is available;
- material and shader identity plus relevant authored property values.

The report produces an explicit leading decision category such as:

- `PRE_LIGHT_MASK_DISCONTINUITY_CONFIRMED`;
- `INTERNAL_RENDER_NORMAL_SEAMS_CONFIRMED`;
- `BEVEL_NORMAL_OUTSIDE_ADJACENT_LIGHTING_ENVELOPE`;
- `FINAL_MESH_PARENT_PROVENANCE_INSUFFICIENT`;
- `STORED_NORMAL_GEOMETRIC_DISAGREEMENT`;
- `NO_STRUCTURAL_DEFECT_FOUND_IN_CURRENT_MESH_CHANNELS`.

No production change may be selected from visual speculation while this suite
has unresolved evidence. If final-mesh topology cannot recover sufficient
parent evidence, the next diagnostic step must instrument construction-time
provenance rather than modifying geometry.

## GM-SURFACE.5F-DIAG2 — comprehensive bevel-shading evidence contract

The bevel-shading suite is generation-traced and read-only with respect to production algorithms. A run captures the normal production regeneration path and correlates generation-time logical bevel identity with the uploaded Unity mesh. It must not group bevels solely by final-mesh connectivity.

For every source-edge bevel the report owns all of the following evidence in one run:

- graph edge and candidate identity;
- true parent source-face IDs and normals;
- source edge endpoints;
- every final triangle carrying that bevel provenance;
- authored surface-group IDs;
- authored, geometric, stored render, and uploaded mesh normals;
- internal shared-edge normal and material-mask jumps;
- exposure, crevice/base, and dirt channels per vertex;
- structural feature channels;
- tangent-to-normal orthogonality;
- current directional-light response compared with both true parent normals;
- parent-normal cone violations;
- generation-capture to uploaded-mesh parity;
- an explicit evidence-based terminal decision.

The suite remains a single Inspector action, runs its analysis incrementally across Editor updates, supports cancellation, writes a checkpointable copyable report, and introduces no production shader or geometry changes. The required action remains:

`Generated Mass Inspector -> Mesh Diagnostics -> Run Bevel-Shading Evidence Suite`

## GM-SURFACE.5F-DIAG2-H1 — committed production build capture contract

The first DIAG2 implementation failed its runtime capture contract. It wrapped an entire forced regeneration in one append-only capture, so internal trial, certification, audit, preview, fallback, and final mesh builds could contribute triangles to the same snapshot. The observed report captured zero logical bevels and 2,512 triangles against a 204-triangle uploaded mesh; its terminal no-defect decision is invalid.

The corrected diagnostic contract is build-scoped and fail-closed:

- every `BuildMeshData` invocation receives a unique diagnostic build identity;
- logical bevel records and final triangles are stored per build rather than globally;
- the exact `MeshData` passed by `GeneratedMass` to `MeshBuilder.ApplyToMesh` marks one completed build as accepted for upload;
- causal analysis uses only that accepted build;
- graph-edge, candidate, logical-bevel, provenance kind, and provenance index fields remain explicit rather than being treated as one implicit identity domain;
- provenance categories are resolved through named enum values inside `MassGenerator`; raw numeric enum literals are prohibited in the editor report;
- capture completeness is checked before analysis, including unique accepted build, captured/uploaded triangle parity, nonzero logical bevels when selected bevels exist, and complete ordinary-bevel triangle mapping;
- any failed invariant produces `CAPTURE_CONTRACT_FAILURE` with `causalAnalysisPerformed=0`;
- a clean accepted build may emit a success verdict only after complete capture and at least one logical bevel have been analyzed; the current validated success verdict is `LOGICAL_BEVEL_MASK_CONTINUITY_VALIDATED`.

This update changes diagnostics only. It does not alter generated geometry, normals, tangents, material masks, structural mesh channels, shader behavior, materials, scenes, prefabs, or production rendering. When capture is inactive, instrumented generation points perform only an inactive-state check and allocate no diagnostic collections.

## GM-SURFACE.5F-DIAG2-H2 — committed plane-cut logical provenance capture

H1 validated build-scoped capture and exact accepted-mesh correlation: the diagnostic separated fourteen internal mesh builds, selected one accepted build, and matched all 204 captured triangles to the uploaded mesh. The remaining failure was identity-specific: the accepted build contained 121 ordinary bevel triangles but zero logical bevel records.

The former logical hook lived only in the bounded all-edge reconstruction path. That path is used for independent audit evidence and ends with `geometryCommit=disabled`; it is not the committed production route that emits the accepted `EdgeBevelPlane` soup. H2 removes that wrong-path hook.

The committed production route now registers logical bevel ownership only after `TryAuditCertifiedBaselineAugmentation` has selected a valid materialized soup and immediately before that soup is returned to the final `BuildMeshData` call. Registration scans the materialized soup for distinct named `EdgeBevelPlane` provenance identities and resolves each identity through the production `ChamferTopologyContext`. Each record preserves graph edge, candidate, source edge, emitted provenance kind/index, true parent faces and normals, source endpoints, and strength as separate fields.

Final triangles continue to be captured only inside their build-scoped `BuildMeshData` record. The accepted report now includes per-provenance triangle counts even when the capture contract fails, making mixed production provenance visible without relying on raw enum integers. Any emitted plane provenance that cannot resolve to a selected production graph edge is a capture-contract failure.

This update changes diagnostics only. It does not alter edge selection, plane solving, geometry materialization, triangulation, normals, tangents, material masks, mesh channels, shaders, materials, or serialized assets.

## GM-SURFACE.5G-H1 — Validation Closure and Diagnostic Cleanup Plan

Status: **implemented; static audit passed; Unity compact-report validation pending**.

### Objective

Freeze the validated GM-SURFACE.5G production behavior, record the two-rock evidence, and reduce successful bevel-shading reports to the permanent regression summary while retaining full per-bevel evidence on any failure.

### Approved files

- `Assets/Game/Procedural/Masses/Editor/GeneratedMassBevelShadingDiagnosticSuite.cs`
- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`

### Reviewed evidence

- `Generated Mass Test (3)`: 612 vertices, 204 triangles, 35 logical bevels, 121 bevel triangles, geometry fingerprint parity, zero source-face mask changes, zero internal mask jumps, zero upload mismatches.
- `Generated Mass Test (1)`: 564 vertices, 188 triangles, 34 logical bevels, 120 bevel triangles, geometry fingerprint parity, zero source-face mask changes, zero internal mask jumps, zero upload mismatches.
- No separate DIAG1 implementation file remains. The confirmed superseded remnants are the generic success verdict and unconditional successful per-triangle report expansion.

### Invariants

- No production generation, geometry, topology, normal, tangent, provenance, mask-compilation, shader, material, or serialized-asset behavior changes.
- Capture-contract, immutable-fingerprint, source-face preservation, degenerate parity, final-mesh correspondence, logical mapping, and shared-edge mask-continuity checks remain active.
- Any failed invariant retains complete logical-bevel, shared-edge, and triangle evidence.
- A clean successful run emits a compact report and the terminal verdict `LOGICAL_BEVEL_MASK_CONTINUITY_VALIDATED`.

### Non-goals

- No geometry cleanup.
- No normal or tangent changes.
- No new Inspector controls or report-mode settings.
- No deletion of the comprehensive suite.

### Implemented closure behavior

- Clean successful runs emit the compact terminal verdict `LOGICAL_BEVEL_MASK_CONTINUITY_VALIDATED` and omit repetitive per-bevel triangle evidence.
- Any capture, geometry, mapping, normal, material-mask, parent-cone, light-response, or upload failure retains the complete logical-bevel evidence sections.
- The comprehensive suite and all permanent regression checks remain available through the existing Inspector action; no new control was added.

### Validation status

- Static source/reference and scope audit passed.
- Production behavior is supported by two accepted Unity evidence runs on distinct generated rocks:
  - `Generated Mass Test (3)`: 612 vertices, 204 triangles, 35 logical bevels, 121 bevel triangles.
  - `Generated Mass Test (1)`: 564 vertices, 188 triangles, 34 logical bevels, 120 bevel triangles.
- Both runs reported exact immutable fingerprint parity, zero source-face mask changes, zero internal mask jumps, zero upload mismatches, zero geometry regression, and complete logical mapping.
- Unity compilation and one compact-success report from this H1 cleanup remain pending because this source workspace cannot run Unity 6000.5.0f1.

### Final scope and consistency audit

- Actual modified project files match the four approved paths exactly.
- `MassGenerator.MeshOutput.cs`, `MassGenerator.BevelShadingDiagnosticCapture.cs`, production geometry, mask compilation, and shader files are byte-identical to the validated GM-SURFACE.5G baseline.
- The editor change affects report selection and formatting only: the decision hierarchy is unchanged except for the specific clean verdict, all analysis still runs, and failure reports still emit the existing complete per-bevel evidence.
- No separate DIAG1 implementation file or additional obsolete Inspector action exists in the reviewed tree.
- Static delimiter, reference, obsolete-verdict, changed-file, and package-scope checks passed. Unity compilation and one compact-success run remain pending.

## GM-SURFACE.5G — Logical-Bevel Continuous Material Masks and Geometry Regression Guard

Status: **validated on two distinct generated rocks; production behavior accepted and frozen**.

### Objective

Correct discontinuous exposure, crevice/base, and dirt-deposit values across internal triangulation edges of committed `EdgeBevelPlane` surfaces without changing geometry, topology, normals, tangents, provenance, surface groups, logical-bevel ownership, or source-face material masks.

### Reviewed evidence

The validated H2 report captured one accepted 204-triangle production build, 35 logical bevels, 121 mapped `EdgeBevelPlane` triangles, zero render-normal fragmentation, zero parent-normal-cone violations, and 25 logical bevels with shared-edge material-mask discontinuities. The strongest discontinuities occurred with zero render-normal jump, proving that the compiled material channels—not lighting orientation—were the actionable defect. Two accepted zero-area triangles produced invalid geometric-normal comparisons and are treated as preserved pre-existing geometry, not as upload corruption.

### Approved files

- `Assets/Game/Procedural/Masses/MassGenerator.MeshOutput.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.BevelShadingDiagnosticCapture.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassBevelShadingDiagnosticSuite.cs`
- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md` only if the production contract requires clarification
- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md` only after implementation status is established

### Production contract

1. The existing generated-face inheritance pass remains responsible for initial source-boundary and triangle-interior values.
2. A second logical-bevel reconciliation pass groups duplicated render vertices by `(provenance kind, provenance index, quantized position)` for committed ordinary bevel polygons.
3. Every duplicate representing the same logical bevel point receives the same averaged exposure, crevice/base, and dirt-deposit values.
4. Only `Colors.g`, `Colors.b`, and `UV2.y` may change.
5. Source-face vertices and non-bevel generated categories remain untouched by the reconciliation pass.
6. No geometry builder, solver, weld, sanitation, triangulation, normal, tangent, or provenance operation is called by the mask compiler.

### Diagnostic regression contract

When the bevel-shading capture is active, each `BuildMeshData` records immutable-channel fingerprints immediately before and after mask compilation. The fingerprint covers vertex positions, triangle indices, normals, structural feature data, UV0, and non-mask portions of color/UV2. Any difference is a capture-contract failure and the report emits `GEOMETRY_REGRESSION` before material conclusions.

The capture also records:

- pre/post full material-mask fingerprints for cross-run determinism comparison;

- source-face mask changes before/after compilation;
- reconciled logical-bevel position groups and vertex count;
- pre/post degenerate triangle identities;
- new and removed degenerate triangles;
- accepted-build geometry fingerprint equality.

Zero-area triangles are excluded from angular geometric-normal comparisons. They remain present and must have exact pre/post identity parity in this patch.

### Acceptance criteria

- accepted captured triangle count equals uploaded triangle count;
- accepted build geometry fingerprint matches before/after compilation;
- source-face mask change count is zero;
- new and removed degenerate triangle counts are zero;
- all ordinary bevel triangles remain mapped;
- shared-edge exposure, crevice, and dirt jumps are each at most `0.00001`;
- render-normal, surface-group, parent-cone, and upload checks remain valid;
- no shader, material, scene, prefab, profile, layer, tag, or project-setting changes.

### Non-goals

- removing or retriangulating degenerate triangles;
- changing bevel geometry, width, selection, or topology;
- changing authored or stored normals;
- changing shader response;
- changing source-face material pattern values.

## GM-SURFACE.5G-H2 — Comprehensive Residual Bevel-Shading Audit (active plan)

### Objective
Diagnose the residual low-light triangular faceting that remains after GM-SURFACE.5G without changing production geometry, normals, mask compilation, shaders, materials, scenes, or serialized assets.

### Approved files
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassBevelShadingDiagnosticSuite.cs`
- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`

### Reviewed evidence
- The committed-build capture and logical provenance contract pass on two production rocks.
- GM-SURFACE.5G removed shared-edge value discontinuities and preserved immutable geometry.
- Visual inspection still shows localized triangular tonal facets in low light; strong direct illumination suppresses their visibility.
- `PS3D/Pixel Surface Lit` combines mesh color/UV channels, procedural world-position variation, generated feature atlases, whole-surface normals, URP direct light, SH ambient, shadows, and PBR response.

### Invariants
- Diagnostic-only patch; no production mesh, mask, shader, material, scene, or prefab behavior change.
- Existing incremental/cancellable editor execution remains.
- Accepted-build identity, triangle parity, geometry fingerprint, logical mapping, and mask-continuity checks remain intact.
- Successful or failed results must remain evidence-based and fail closed.

### Audit dimensions
1. Triangle area, sliver/aspect quality, winding, planarity and geometric-normal spread.
2. Stored/render normal, authored normal, tangent orthogonality and parent-envelope behavior.
3. C0 value continuity and C1-like piecewise-linear gradient jumps for surface variation, exposure, crevice, dirt, and all structural channels.
4. Low- and high-intensity direct-light response using both stored and geometric normals.
5. Material configuration relevant to ambient, direct, shadow, smoothness, specular, whole-surface normal and procedural variation.
6. Ranked worst logical bevels and shared edges, with a terminal evidence classification rather than a preselected cause.

### Acceptance criteria
- Project compiles with no new errors.
- Suite remains incremental and cancellable.
- Existing capture and geometry guards still pass.
- Report includes complete aggregate counters and ranked evidence for every tested cause family.
- Terminal decision identifies one or more supported cause families or states that no captured family explains the residual defect.

### Non-goals
- No production correction.
- No automatic material mutation or screenshot capture.
- No geometry cleanup, retriangulation, normal rebuilding, shader branch, or new Inspector control.

### File sequence
1. Extend the existing editor suite data model and shared-edge analysis.
2. Add aggregate ranking and decision logic.
3. Correct documentation status: 5G is a partial fix; residual visual defect remains open.
4. Run static scope, delimiter, reference, and diff audits.

### Risks
- Gradient magnitudes depend on triangle scale; report normalized and absolute evidence together.
- Procedural world-position and atlas sampling cannot be fully reconstructed CPU-side; the report must label those paths as unisolated rather than infer certainty.
- A clean numerical result does not override failed visual evidence.

### Status
- Review: complete.
- Plan: approved by user and active.
- Implementation: complete in patch source.
- Static scope and consistency audit: passed.
- Unity compilation and runtime validation: pending.

### Implementation record
- Editor suite extended with C0 and normalized gradient evidence for all captured mask and structural channels.
- Triangle area/aspect, geometric-normal facet risk, render-normal continuity, tangent orthogonality, parent envelope, direct-response sensitivity and upload parity remain in one report.
- No production or shader file modified.
- Static audit pending below; Unity compile and runtime report remain pending.

## GM-SURFACE.5I — Certified Bevel Triangulation Quality (active implementation plan)

### Objective
Remove confirmed avoidable bevel-triangulation defects—zero-area triangles, pathological slivers, and poor internal diagonals—without changing polygon boundaries, selected bevels, plane-cut geometry, render normals, tangents, provenance, surface groups, material compilation, shaders, materials, scenes, or serialized assets.

### Approved files
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.BoundedSingleEdge.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassBevelShadingDiagnosticSuite.cs`
- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`

### Reviewed evidence
- Two accepted production rocks reported widespread sliver risk: 31/35 and 33/34 logical bevels.
- Maximum accepted triangle aspect ratios reached approximately 49,348 and 5,151.
- A minority of bevels showed geometric-normal facet risk, including approximately 20–30 degree adjacent geometric-normal variation.
- Render normals, logical ownership, provenance, upload parity, and structural channels remained coherent.
- The same material-gradient defect occurs across all bevels, so this geometry patch is quality cleanup and does not claim to resolve the material/shader-dependent residual shading defect.

### Production design
1. Evaluate every stable boundary-fan triangulation and the complete certified general triangulation for each finalized one-surface polygon.
2. Score complete candidates deterministically by: zero invalid triangles, lowest maximum aspect ratio, highest minimum internal angle, highest minimum triangle area, highest minimum authored-normal agreement, then stable deterministic index ordering.
3. Select the best complete candidate rather than accepting the first stable fan.
4. Use tolerance-collinear reinsertion only when no complete certified fan or general candidate exists.
5. Preserve the exact ordered polygon boundary and emit exactly `n-2` triangles.

### Invariants
- Polygon boundary positions and winding remain unchanged.
- No source-face or bevel-plane construction changes.
- No selected-edge, width, coexistence, welding, or sanitation changes.
- Authored surface group, provenance, logical ownership, render normals, tangents, and all material/structural vertex values remain tied to the same polygon vertices.
- No post-build triangle deletion.
- Deterministic tie-breaking is mandatory.

### Diagnostics and acceptance
- Existing comprehensive suite remains the validation authority for accepted mesh parity, geometry fingerprints, logical mapping, degenerates, sliver/aspect risk, geometric-normal facet risk, mask continuity, and upload correspondence.
- Required runtime outcome: zero new open/non-manifold/T-junction regressions; zero upload mismatch; zero unmapped bevel triangles; no geometry-fingerprint inconsistency inside mask compilation; no source-face mask change.
- Quality target: eliminate degenerate accepted triangles where an alternate complete triangulation exists and materially reduce maximum aspect ratio and geometric-facet-risk counts on the two known stones.
- A narrow polygon may retain an unavoidable sliver only when every complete certified triangulation is worse or invalid; deterministic best-candidate selection must still be used.

### Non-goals
- No material/shader correction.
- No logical-bevel material-field redesign.
- No polygon-boundary simplification except the existing certified tolerance-collinear fallback when all complete candidates fail.
- No new Inspector control.

### File sequence
1. Extend internal triangulation candidate/state quality data.
2. Evaluate and compare complete fan and general candidates.
3. Preserve existing fallback and emission contracts.
4. Retain and clarify comprehensive geometry-quality reporting.
5. Update canonical overview/checklist status.
6. Run static scope, reference, delimiter, and final-diff audit; Unity compilation/runtime validation remains pending outside this workspace.

### Status
- Review: complete.
- Plan: approved by user and active.
- Implementation: complete in patch source.
- Static audit: passed.

### GM-SURFACE.5I implementation and static audit record

- Implemented complete-candidate comparison between the best stable boundary fan and the certified general interval triangulation.
- Extended triangle candidates and dynamic-programming states with maximum aspect ratio and minimum internal angle.
- General triangulation ranking now minimizes worst aspect ratio before maximizing minimum angle, minimum area, authored-normal agreement, and deterministic split order.
- Stable fan-anchor ranking uses the same quality ordering and deterministic anchor tie-break.
- Existing boundary validity, `n-2` triangle count, area threshold, authored-normal agreement, provenance, surface-group, and emission certification remain unchanged.
- Existing tolerance-collinear reinsertion remains reachable only when both complete fan and complete general triangulations fail.
- Actual modified project files match the approved production/type/document scope. The editor comprehensive suite required no source change because it already reports accepted degenerates, sliver/aspect risk, geometric-normal facet risk, mapping, upload parity, and geometry regression.
- Static delimiter, symbol-reference, changed-file, and package-scope checks passed.
- Unity compilation and two-rock runtime validation remain pending because Unity 6000.5.0f1 is unavailable in this workspace.

## GM-SURFACE.5I-H1 — fail-closed final-emission triangulation certification

The first 5I validation improved many aspect-ratio outliers but exposed an unacceptable gap: candidate-space validity did not guarantee that the exact emitted/uploaded triangle remained non-degenerate under final float positions. H1 therefore certifies each candidate against its exact final polygon vertices using duplicate-index rejection, scale-relative coincident-position rejection, double-precision area evaluation, and authored-normal agreement before it may participate in quality ranking. Invalid candidates are excluded rather than merely penalized. Candidate ranking is lexicographic, with normal agreement ahead of shape-quality preferences.

The bevel-shading evidence suite now independently counts degenerates in the uploaded Unity mesh, the captured ordinary-bevel triangle set, and the per-logical-bevel analysis. Any uploaded degenerate or accounting disagreement is a terminal failure. Material compilation, shaders, bevel boundaries, widths, provenance, source faces, and authored render-normal policy remain unchanged.
