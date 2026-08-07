# Generated Mass Production Geometry and Surface-Response Architecture

Status: canonical
Architecture ID: GM-SURFACE.4

## Scope

This document owns the current Generated Mass decisions for production bevel/chip geometry, mesh normals, whole-rock normal detail, feature-localized surface responses, memory limits, shader limits, and future feature integration.

Repository code overrides this document if they differ; such disagreement is a defect.


## Non-negotiable active defect definition — surface-orientation lighting coherence

**This is the primary Generated Mass lighting defect. Do not reframe it as a general brightness, exposure, ambient-light, indirect-light, smoothness, or specular-intensity problem.**

The defective HLSL material does not preserve a coherent relationship between **surface orientation relative to the same incident light** and the resulting visible lighting response. Individual source faces and ordinary bevel faces can be ordered incorrectly:

- a surface that faces the light more directly can render darker than a less-directly-facing neighboring surface;
- a surface that faces the light less directly can render brighter than a more-directly-facing neighboring surface;
- an ordinary bevel whose geometric orientation lies between its two parent faces can render darker than both parents or brighter than both parents instead of producing the expected intermediate directional-light response;
- adjacent bevel segments can change brightness ordering in ways that are not explained by their geometric orientation to the light.

The accepted visual reference is the legacy `M_PixelStone` / `SG_PixelSurfaceLit` response on comparable Generated Mass geometry: as a face turns toward or away from the same directional light, its illumination changes coherently, and bevels visually bridge the directional-light responses of their adjacent parent surfaces.

Absolute object luminance is **not** the acceptance criterion. A globally darker or brighter rock can still be directionally correct. Conversely, matching average luminance, dielectric F0, overall exposure, ambient strength, or specular energy does **not** solve the defect if individual surface/bevel responses remain incorrectly ordered by orientation.

For controlled diffuse evidence, the invariant is explicit: fragment response must follow the fragment normal's `max(dot(N, L), 0)` ordering. For actual-material evidence, artistic layers may modify magnitude, but they must not introduce unexplained parent/bevel or neighboring-face ordering inversions relative to the accepted legacy behavior.

Any diagnosis, report, patch, or recommendation that discusses only whole-object darkness, average residual, specular strength, or global lighting parity without directly checking **per-surface orientation ordering** is insufficient to close this defect.

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


## Historical lighting-work guard

GM-SURFACE.5A through the earlier mask/readability/parity patches record real historical defects and experiments, but **none of those historical labels redefine the active GM-SURFACE.5P defect**. In particular, earlier "dark-band", mask-inheritance, shadow-readability, whole-rock-normal, BRDF/F0, and general-darkness findings must not be substituted for the current acceptance criterion: individual source faces and bevels must respond coherently to the same light according to their surface orientation, with parent–bevel–parent ordering consistent with the legacy reference.

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

## GM-SURFACE.5G-H1 — Validation Closure and Diagnostic Cleanup (historical)

Status: **superseded as a visual-closure claim**. The compact report correctly validated C0 shared-edge mask equality, but later screenshots proved that the residual low-light faceting remained. The production mask reconciliation is retained; the former visual closure/freeze interpretation is not.

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
- Unity compact-success reporting was subsequently validated, but the visual result failed acceptance; this section is retained only as the history of the narrow C0 mask check.

### Final scope and consistency audit

- Actual modified project files match the four approved paths exactly.
- `MassGenerator.MeshOutput.cs`, `MassGenerator.BevelShadingDiagnosticCapture.cs`, production geometry, mask compilation, and shader files are byte-identical to the validated GM-SURFACE.5G baseline.
- The editor change affects report selection and formatting only: the decision hierarchy is unchanged except for the specific clean verdict, all analysis still runs, and failure reports still emit the existing complete per-bevel evidence.
- No separate DIAG1 implementation file or additional obsolete Inspector action exists in the reviewed tree.
- Static and Unity compact-report checks passed at the time; later visual evidence superseded the closure interpretation.

## GM-SURFACE.5G — Logical-Bevel Continuous Material Masks and Geometry Regression Guard

Status: **retained partial production correction**. Two-rock evidence validates duplicate-position mask equality and immutable-geometry preservation. It does not validate the complete visible shading result; residual low-light faceting remains open.

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

## GM-SURFACE.5G-H2 — Comprehensive Residual Bevel-Shading Audit (completed evidence phase)

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

## GM-SURFACE.5J — Integrated Surface-Causality Reset

Status: **implemented in source; Unity compilation and runtime tournament pending**.

### Why this reset exists

The GM-SURFACE.5I and 5I-H1 production retriangulation experiments were rejected. They improved some global aspect-ratio totals but increased geometric-facet-risk counts, retained pathological triangles, and exposed incompatible definitions of “degenerate.” The follow-up proposal incorrectly inferred literal zero-area geometry from a zero normalized vector despite nonzero measured triangle area. Continuing to patch thresholds or triangulation selection would compound an untrusted baseline.

GM-SURFACE.5J therefore restores the exact pre-5I production triangulation, retains the independently validated GM-SURFACE.5G mask reconciliation, and replaces the fragmented investigation with one integrated causality system.

### Production baseline

- `MassGenerator.EdgeWear.BoundedSingleEdge.cs` and `MassGenerator.EdgeWear.Types.cs` are restored byte-for-byte to the authoritative pre-5I source supplied in `Assets-Code-Archive(42).zip`.
- No alternative triangulation is uploaded by this suite.
- No production mask, normal, tangent, provenance, surface-group, material, scene, prefab, or serialized setting is changed.
- The ordinary shader variant does not compile diagnostic derivative/branch work. Audit modes are isolated behind the local `_SURFACE_CAUSALITY_AUDIT` shader keyword and are enabled only on temporary cloned materials.

### Canonical final-triangle contract

`MassGenerator.EvaluateFinalTriangleQuality` is the only triangle-quality classifier used by committed-build capture, uploaded-mesh accounting, logical-bevel analysis, and audit-only candidate scoring. It operates on exact final positions in double precision and records:

- finite coordinates and distinct indices/positions;
- double area and scale-normalized double area;
- longest edge, shortest altitude, aspect ratio, and minimum angle;
- double-precision geometric normal and authored-normal winding agreement;
- separate structural, numerical-conditioning, and sliver classifications.

The categories are intentionally distinct: `ExactDegenerate`, `NumericallyUnderResolved`, `ExtremeSliver`, `WindingInvalid`, and valid conditioned geometry must never be inferred from `Vector3.normalized` returning zero. Differential normal/gradient analysis excludes structurally invalid, numerically under-resolved, and extreme-sliver triangles rather than fabricating 180-degree jumps.

### Frozen-mesh parity tournament

The suite regenerates each selected mass once to capture the exact accepted production build, then freezes those meshes for every render case. With two selected masses, the active object is the suspect and the other is the reference. The matrix crosses:

- suspect mesh with suspect renderer state;
- suspect mesh with reference material asset only;
- suspect mesh with full reference renderer state;
- reference mesh with suspect material asset only;
- reference mesh with full suspect renderer state;
- reference mesh with reference renderer state.

“Renderer state” includes the material asset, renderer-level `MaterialPropertyBlock`, shadow reception/casting, rendering-layer mask, light/reflection-probe usage, probe anchors, and relevant renderer flags. This separates material-asset ownership from renderer-property-block ownership and mesh/material interaction.

### Contribution and lighting tournament

Audit-owned temporary clones isolate:

- unlit pre-light albedo;
- constant-albedo full, main-direct-only, and ambient-only response;
- final, stored/render, and per-triangle geometric normals;
- full/direct/ambient response under final, stored, and triangle normals;
- generated whole-surface normal, fog/post, exposure, crevice, base contact, dirt, pixel variation, mottle, feature atlases, specular/smoothness, profile effects, generated tints, shadows, and additional lights;
- raw surface-variation, exposure, crevice/base, dirt, edge-wear, boundary-field, and boundary-modulation channels;
- current low-light state and an eight-times light-intensity final case.

All scene renderers and temporary light overrides are restored immediately after each audit render. GPU transfer uses `AsyncGPUReadback`; cancellation remains immediate and any pending render-target cleanup is deferred without blocking the Editor.

### Screen-space causality measurement

The suite projects generation-traced internal logical-bevel edges through the audit camera, rejects back-facing/nonprojectable edges, verifies samples against an audit-owned object mask, and measures the change in luminance derivative on the two sides of each edge. This detects triangular slope faceting rather than only C0 value steps. The report includes eligible/front-facing/projected edge counts, mean/P90/maximum derivative jump, value step, and a composite facet score for every case.

Only true ablation cases participate in dominant-contributor ranking. Raw channel visualizations, normal visualizations, and direct/ambient isolation views remain evidence but cannot falsely win the “dominant contributor” label merely because their output is visually flatter.

### Audit-only alternative triangulation

Legacy fans and deterministic ear-clipping candidates remain audit-only. Candidate coverage is validated through boundary-edge incidence, orientation, polygon-area parity, centroid containment, and noncrossing internal diagonals before canonical quality scoring. Results can establish whether a later geometry redesign is worthwhile, but they never mutate the production mesh.

### Resumed final-audit corrections

The interrupted implementation was recovered before packaging. The resumed Gate 4 audit identified two integration defects that must be corrected before delivery:

- The render audit referenced `EditorSceneManager.MarkSceneClean`, which is not part of the current public Unity Editor API. Scene-dirty restoration must therefore use a compile-safe reflection-only best-effort path and report any unresolved clean-state restoration instead of depending on a nonexistent compile-time symbol.
- Frozen parity clones occupy the suspect location while the original suspect/reference renderers still participate in shadow and reflection rendering. The originals must be suppressed for every case, while unrelated scene renderers remain active unless the audit-layer fallback requires complete renderer suppression. This prevents the original mesh from contaminating reference-mesh and cross-material cases.
- Winding classification must not promote a numerically under-resolved triangle into structural invalidity. Authored-normal winding is accepted as structural evidence only when the canonical double-precision triangle is sufficiently conditioned for a reliable orientation test.
- The audit camera must be normalized to a standalone URP Base camera after copying the source camera. Any copied overlay render type or camera stack must be cleared through compile-safe reflection so parity cases cannot inherit unrelated overlay cameras or stack rendering. The report must state whether normalization was applied or unavailable.
- Both suspect and optional reference selections must own a valid Generated Mass geometry filter before the suite starts; an invalid reference must fail before capture rather than producing a partial parity tournament.

These corrections do not change production geometry, masks, normals, materials, serialized assets, or ordinary shader behavior.

### Final source-audit evidence

- Approved scope contains exactly eleven changed source/document files: three canonical Markdown documents, three Generated Mass diagnostic/editor files, three Generated Mass capture/classification files, and two Pixel Surface shader files. No scene, prefab, material, profile, layer, tag, project-setting, or other serialized asset is changed.
- `MassGenerator.EdgeWear.BoundedSingleEdge.cs` and `MassGenerator.EdgeWear.Types.cs` match the trusted pre-5I baseline byte-for-byte.
- The non-audit `Frag` body is token-equivalent to the pre-5I production forward path after excluding the two Unity instance/stereo setup statements that remain shared before the audit preprocessor branch.
- `_SURFACE_CAUSALITY_AUDIT` is a local fragment keyword. Its two material constants are present in every `UnityPerMaterial` CBUFFER to preserve pass layout; audit functions and branches are compiled only in the ForwardLit audit variant.
- All thirty-two material-property overrides used by the tournament exist in `SH_PixelSurfaceLit.shader`. Static C# delimiter, shader delimiter/preprocessor, unresolved-symbol-reference, conflict-marker, obsolete direct-API, and changed-file-scope checks pass.
- Unity 6000.5.0f1 compilation, shader compilation, runtime GPU readback, scene-state restoration, and causal-tournament evidence remain pending and must not be treated as passed.

### Terminal contract

The report emits one geometry state per subject:

- `GEOMETRY_VALID`;
- `GEOMETRY_VALID_BUT_POORLY_CONDITIONED`;
- `GEOMETRY_NUMERICALLY_UNDER_RESOLVED`;
- `GEOMETRY_STRUCTURALLY_INVALID`.

The rendered tournament emits one causal ownership classification:

- `MATERIAL_OR_SHADER_OWNED`;
- `MESH_DATA_OWNED`;
- `MESH_MATERIAL_INTERACTION`;
- `LIGHTING_ENVIRONMENT_OWNED`;
- `MULTIPLE_CONTRIBUTORS`;
- an explicit inconclusive state.

No production shading fix is selected by GM-SURFACE.5J. The next production change must follow the combined canonical-geometry, parity, ablation, and visual evidence rather than another isolated hypothesis.


## GM-SURFACE.5J-H1 — Compile integration correction

The initial 5J delivery did not compile: `MassGenerator.TriangleQuality.cs::IsFinite(Vector3)` collided with the existing private `MassGenerator.EdgeWear.Graph.cs::IsFinite(Vector3)` because both files contribute to the same partial `MassGenerator` type. This invalidates the initial package's claim that static source integration had passed. The failure was caused by validating local changed-file declarations without a complete cross-partial signature scan.

The correction renames only the triangle-quality-private finite helpers to `IsTriangleQualityFinite`. Public APIs, call sites, classifier thresholds, report contracts, production triangulation, mask reconciliation, shader behavior, and all unrelated 5J implementation files are unchanged. After the correction, an exact-signature scanner reports `0` duplicate signatures across `280` C# files / `6150` declarations under `Assets` and `0` across `45` Generated Mass files / `1801` declarations. The full static integration audit passes `41 / 41`, including lexical/preprocessor checks, direct caller/type presence, token-equivalent classifier behavior, trusted pre-5I triangulation byte parity, unchanged unrelated implementation files, shader keyword/CBUFFER checks, and package-scope checks. Unity compilation and runtime causality evidence remain pending.

## GM-SURFACE.5J-H2 — Unity 6.5 API-warning cleanup and evidence review

Status: **implemented in source; static audit passed; Unity recompilation pending**.

### Objective

Remove every Unity 6.5 obsolete-API warning introduced by `GeneratedMassSurfaceCausalityRenderAudit.cs` without changing causality-suite behavior, report structure, production rendering, production geometry, serialized state, or the completed runtime evidence. Record the first completed single-subject tournament result and preserve its explicit limitation: no reference subject was supplied, so causal ownership remains inconclusive even though contributor evidence is useful.

### Approved files

- `Assets/Game/Procedural/Masses/Editor/GeneratedMassSurfaceCausalityRenderAudit.cs`
- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`

### Reviewed evidence

- Unity 6000.5.0f1 compiled GM-SURFACE.5J-H1 and emitted twelve `CS0618` warnings for six calls to the deprecated `FindObjectsByType<T>(FindObjectsInactive, FindObjectsSortMode)` overload and five deprecated `ShaderUtil` property-introspection calls.
- Repository rule requires unsorted `FindObjectsByType`; the current code explicitly requested `FindObjectsSortMode.None`, so replacing it with `FindObjectsByType<T>(FindObjectsInactive)` preserves intended unsorted behavior.
- Unity compiler diagnostics explicitly identify `Shader.GetPropertyCount`, `Shader.GetPropertyName`, and `Shader.GetPropertyType` as replacements.
- The completed single-subject report captured 49/49 render cases, made no serialized writes, restored pre-5I production triangulation, and ended `INCONCLUSIVE_REFERENCE_REQUIRED` because `reference present=0` and `materialParityAvailable=0`.
- The same report classified the suspect mesh `GEOMETRY_VALID_BUT_POORLY_CONDITIONED`, with zero exact degenerates, zero numerically under-resolved triangles, 32 extreme slivers, and zero improving audit-only triangulations.
- The strongest ablation evidence was `ALL_PIXEL_VARIATION_OFF`, reducing facet score from `0.0238571391` to `0.00886387` (`62.8460467%`). `ADDITIONAL_LIGHTS_OFF` reduced it by `22.63617%`; generated-surface-normal removal and most mask/profile ablations did not improve it.

### Invariants and non-goals

- No production mesh, material, shader, mask, normal, tangent, scene, prefab, layer, tag, or project-setting change.
- No causality metric, case ordering, selection behavior, light override, camera behavior, or report field change.
- No speculative production fix is selected from a run without a reference subject.
- No sorting is introduced; scene object enumeration remains unsorted.

### Implementation sequence

1. Replace all six obsolete `FindObjectsByType` calls with the Unity 6.5 unsorted overload that accepts only `FindObjectsInactive`.
2. Replace shader property enumeration with `Shader.GetPropertyCount`, `Shader.GetPropertyName`, and strongly typed `Shader.GetPropertyType` checks using `ShaderPropertyType`.
3. Scan the entire changed file and Generated Mass subtree for the obsolete APIs named in the compiler output.
4. Re-run complete C# exact-signature, lexical delimiter/preprocessor, package-scope, and behavioral-diff audits.
5. Package only the four changed files and record Unity validation as pending.

### Acceptance criteria

- Zero references remain to `FindObjectsSortMode`, `ShaderUtil.GetPropertyCount`, `ShaderUtil.GetPropertyName`, or `ShaderUtil.GetPropertyType` in the changed file.
- All previous call sites still include inactive objects where they did before.
- Texture, color/vector, and scalar material-property report formatting remains semantically equivalent.
- No exact duplicate C# signatures are introduced across `Assets` or the Generated Mass subtree.
- Final package contains exactly the approved four files.
- Unity 6000.5.0f1 recompilation produces no new error or warning from GM-SURFACE.5J-H2.

### GM-SURFACE.5J-H2 implementation and audit record

- All six deprecated `FindObjectsByType` overload calls now use `FindObjectsByType<T>(FindObjectsInactive)` and preserve their previous Include/Exclude behavior.
- All five deprecated `ShaderUtil` property calls now use the `Shader` instance API; texture detection uses `ShaderPropertyType.Texture`, replacing the old `ShaderUtil` string value `TexEnv` without changing report semantics.
- No causality case, score, renderer/light override, selection contract, shader, production mesh, or serialized state changed.
- Static obsolete-symbol, exact-signature, lexical/preprocessor, behavioral-diff, and changed-file-scope audit passed `22 / 22`. Unity compilation remains pending.

## GM-SURFACE.5K — Same-mesh legacy lighting decomposition and material-state invariance

Status: source implementation complete; static audit passed `106 / 106`; changed-file package staging and archive re-extraction byte/hash parity passed; Unity validation pending.

### Objective

Determine whether the current `PS3D/Pixel Surface Lit` path under-responds on ordinary source faces, over-responds on bevel/cap classes, or diverges at a specific pre-light, normal, direct, indirect, specular, property-block, or final-PBR stage when compared with the known-good legacy `M_PixelStone` material on the exact same frozen Generated Mass mesh. The diagnostic must leave every source renderer's material asset, shader, material slots, and `MaterialPropertyBlock` exactly as they were before capture, including cancellation and exception paths.

### Approved files

- `Assets/Game/Procedural/Masses/Editor/GeneratedMassBevelShadingDiagnosticSuite.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassSurfaceCausalityRenderAudit.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceForwardPass.hlsl`
- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`

No shader asset property, serialized material, scene, prefab, profile, layer, tag, geometry generator, triangulation, mesh channel, normal, tangent, or production material-mask compiler may change.

### Reviewed evidence

- `GeneratedMassBevelShadingDiagnosticSuite.Capture(...)` currently calls `GeneratedMass.Regenerate()` directly. `GeneratedMass.SynchronizeGeneratedState(...)` reaches `ApplyMaterialProperties()`, which calls `ApplyStoneSurfaceProfileMaterial()` before updating the property block. Therefore a diagnostic capture can temporarily or persistently replace a manually assigned renderer material when the mass's serialized `StoneSurfaceProfile` names an HLSL profile.
- `GeneratedMassEditor.DrawSurfaceMaskDebugControls()` changes the serialized `surfaceMaskDebug` field; `OnValidate()` then runs `SynchronizeGeneratedState(...)`. If the mass is not in `RendererMaterial` profile mode, the named profile material is reasserted. This explains the observed `M_PixelStone` to `M_PixelStone_HLSL_ColdGrey` switch when changing debug views; it is profile reapplication, not a shader changing itself.
- Current 5J render cases use audit-owned temporary renderers and cloned materials, but subject capture does not yet snapshot and restore source renderer material slots and property blocks.
- Both reported suspect meshes have zero exact degenerates, zero invalid vertex frames, zero internal render-normal fragmentation, and valid-but-poorly-conditioned geometry. The older `M_PixelStone` material renders the same geometry with plausible bright-to-dark transitions, while `M_PixelStone_HLSL_ColdGrey` can produce implausible relative response between source faces and bevels.
- The current screen-space metric measures internal edges within logical bevels. It does not separately score source faces, ordinary bevels, junction/cap surfaces, or corner-damage surfaces, and it does not compare bevel-parent triplets against the legacy material on the same mesh.
- `PixelSurfaceForwardPass.hlsl` is consumed only by `SH_PixelSurfaceLit.shader`. Existing audit branches are compiled only under `_SURFACE_CAUSALITY_AUDIT`; ordinary production variants must remain token-equivalent outside new conditional audit blocks.

### Acceptance criteria

1. Source renderer material state is captured before regeneration and restored after every capture, at finalization, cancellation, and exception exit.
2. Restoration covers the complete `sharedMaterials` array, global `MaterialPropertyBlock`, and per-material-index property blocks. The report records initial, observed-during-capture, restored, and final material/shader identities plus a terminal restoration verdict.
3. A one-subject run automatically loads `Assets/Game/Demo/Materials/Stone/M_PixelStone.mat` as the legacy control and renders it on the exact frozen suspect mesh; no second Generated Mass object is required.
4. The tournament includes current HLSL with original property block, current HLSL with property block cleared, and legacy material with the original suspect property block.
5. Final triangle provenance is partitioned into disjoint source-face, ordinary-bevel, junction/cap, corner-damage, and unclassified classes. Audit-owned class-only meshes generate masks without altering the production mesh.
6. Every render case reports per-class pixel count, mean/P10/P50/P90 luminance, and class-to-whole relative response.
7. Logical bevel records produce stable source-parent-A, bevel, and source-parent-B sample triplets using captured provenance and nearest final-triangle centroids. The report records envelope violations, normalized transition values, and ordering differences against the legacy control.
8. Audit-only HLSL checkpoints expose raw base colour, after pixel variation, after exposure semantic scale, after mottle, after generated crevice/base/dirt layers, final pre-light albedo, stored/final normals, main direct, ambient/indirect, and final PBR without changing the ordinary shader variant. Checkpoint modes `20–24` terminate before overall tint and PBR so they are genuine pre-light observations.
9. The same frozen geometry also reports area-weighted stored-normal main-light prediction for source and bevel classes, observed main-direct-to-prelight response, and the logarithmic bevel/source prediction residual. Cases with fewer than 32 visible source-face or bevel pixels terminate as inconclusive rather than fabricating ownership.
10. The terminal decision distinguishes source-face under-response, bevel over-response, semantic-class mismatch, source/bevel normal-path mismatch, direct/indirect/additional-light/specular integration mismatch, property-block ownership, multiple stages, and no reproduced mismatch.
11. Static validation includes changed-file declaration-collision comparison, delimiter/preprocessor checks, obsolete-API scan, exact changed-file scope, shader property/CBUFFER consistency, production-path conditional audit, and archive re-extraction hash parity. Unity 6000.5.0f1 compilation and runtime GPU validation remain explicitly pending unless actually run.

### Invariants

- Production geometry, indices, provenance, surface groups, mesh normals, tangents, and GM-SURFACE.5G mask reconciliation remain unchanged.
- Source materials and shaders are never edited; every diagnostic material is a `HideAndDontSave` clone.
- Audit renderers, cameras, class-mask meshes, and render targets are temporary and destroyed after use.
- Renderer material restoration is value-exact by object reference and property-block content, not merely name-equivalent.
- Optional selected reference-object support remains available, but the same-mesh legacy material is the primary control.
- No expensive work is added to runtime or ordinary shader variants.

### Non-goals

- Fixing the general named-profile policy that reasserts a profile material after unrelated inspector edits.
- Selecting or applying a production shader correction before the legacy-referenced class/stage report identifies ownership.
- Further production triangulation or geometry cleanup.
- Editing `M_PixelStone`, `M_PixelStone_HLSL_ColdGrey`, scenes, prefabs, or serialized Generated Mass profiles.

### File-by-file implementation sequence

1. `GeneratedMassBevelShadingDiagnosticSuite.cs`: add complete renderer material-state snapshots/restoration, use frozen initial material evidence, add report invariants and final restoration checks.
2. `GeneratedMassSurfaceCausalityRenderAudit.cs`: load the legacy control, add same-mesh material cases, class-mask meshes, class statistics, bevel-parent samples, stage comparison, and terminal ownership classification.
3. `PixelSurfaceForwardPass.hlsl`: add audit-only pre-light checkpoints inside `_SURFACE_CAUSALITY_AUDIT` blocks while leaving production execution unchanged.
4. `GeneratedMassEditor.cs`: update diagnostic help/button text and disclose the named-profile material-reapplication rule without changing serialized behavior.
5. Canonical documents: record the implemented contract, evidence fields, validation path, and pending Unity checks.
6. Final audit: compare every approved file with the 5J-H2 baseline, re-read all direct consumers, run static checks, package changed files only, and verify archive hashes after extraction.

### Risks and mitigations

- **Material restoration loss:** snapshot every slot and property-block scope; restore in nested `finally` paths and once more before terminal report generation.
- **Legacy/current colour differences confound lighting:** report absolute luminance and class-to-whole normalized response; compare bevel-parent transition coordinates within each material before comparing materials.
- **Class masks omit triangles:** report per-class triangle counts and unclassified triangle count; fail the class contract if classified totals do not equal uploaded triangles.
- **Centroid samples land off-screen or behind another surface:** validate object/class masks at every sample and report rejected triplets rather than fabricating values.
- **Audit branch changes production shading:** all checkpoint returns remain under `_SURFACE_CAUSALITY_AUDIT`; static extraction verifies ordinary branch token parity.
- **Suite duration increases:** cases remain incremental, cancellable, asynchronous, checkpointed, and ETA-visible.

### Validation and compliance status

- [x] Review surface and direct material/profile mutation path documented before code edits.
- [x] Approved scope and invariants recorded as the first source-tree modification.
- [x] Material-state snapshot/restoration implemented and statically verified, including nested capture-finally and terminal restoration paths.
- [x] Same-mesh legacy control and property-block cases implemented.
- [x] Surface-class masks/statistics, bevel-parent sampling, minimum-visible-class guard, and stored-normal main-light prediction implemented.
- [x] Audit-only HLSL stage checkpoints implemented with genuine pre-PBR return and ordinary-production token parity evidence.
- [x] Pre-package final diff/compliance audit passed `106 / 106` checks.
- [x] Package changed files only and verify archive re-extraction byte/hash parity.
- [ ] Unity 6000.5.0f1 compilation completed with no new errors or warnings.
- [ ] Runtime one-subject same-mesh legacy/HLSL tournament completed under the defective lighting setup.

## GM-SURFACE.5K-H1 — Compile correction for environment-report formatter

Status: source correction complete; static audit passed `16 / 16`; changed-file package and archive re-extraction byte/hash parity passed; Unity compile validation pending.

### Objective

Correct the GM-SURFACE.5K Unity compilation failure in `GeneratedMassSurfaceCausalityRenderAudit.BuildEnvironmentReport()` without changing the tournament, renderer material-state protection, shader audit modes, production shading, or report semantics.

### Approved files

- `Assets/Game/Procedural/Masses/Editor/GeneratedMassSurfaceCausalityRenderAudit.cs`
- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`

### Reviewed evidence

- Unity 6000.5.0f1 reports `CS0103` at `GeneratedMassSurfaceCausalityRenderAudit.cs:364` and `:366`: the identifier `F` does not exist in the class.
- `BuildEnvironmentReport()` uses the class-owned `Format(float)` helper for every other floating-point report field.
- `GeneratedMassBevelShadingDiagnosticSuite` owns a separate private `F(float)` helper, but that helper is not visible from `GeneratedMassSurfaceCausalityRenderAudit`; the two erroneous calls were copied with the wrong local helper name.
- The direct consumer is `GeneratedMassBevelShadingDiagnosticSuite.BuildReport()`, which appends `RenderAudit.BuildEnvironmentReport()` to the final text report.

### Acceptance criteria

1. Replace only the two unresolved `F(...)` calls with the existing local `Format(...)` helper.
2. Preserve the exact reported values and invariant-culture round-trip formatting contract.
3. No render case, material assignment, property-block handling, shader path, geometry, or serialized asset changes.
4. Scan the complete `GeneratedMassSurfaceCausalityRenderAudit` class for unresolved single-letter formatter invocations and confirm none remain.
5. Re-run complete changed-file delimiter/preprocessor, duplicate-signature, obsolete-API, package-scope, archive re-extraction, and source-hash checks.
6. Unity 6000.5.0f1 compilation remains pending until run by the user; do not claim it passed offline.

### File-by-file sequence

1. Correct the two formatter calls in `GeneratedMassSurfaceCausalityRenderAudit.BuildEnvironmentReport()`.
2. Record the compile correction in the framework and implementation checklist.
3. Compare the final source against GM-SURFACE.5K and verify that only the intended formatter substitutions and documentation differ.
4. Package changed files only and verify archive extraction byte/hash parity.

### Invariants and non-goals

- No behavior change beyond making the existing report code compile.
- No changes to source renderer material restoration, same-mesh legacy parity, surface-class analysis, GPU readback, camera/light isolation, HLSL audit checkpoints, or production shader output.
- No cleanup, refactor, renaming, or unrelated warning suppression.

### Risks and mitigations

- **Risk: another copied formatter alias remains unresolved.** Mitigation: scan every invocation in the complete class and specifically reject undeclared one-character invocation identifiers.
- **Risk: correction package unintentionally contains the full 5K archive rather than a layered fix.** Mitigation: diff against the exact GM-SURFACE.5K package and stage only the four approved files.

### Validation and compliance status

- [x] Repository instructions reloaded from `Assets/AGENTS.md`.
- [x] Compile error, affected implementation, local formatting contract, and direct report consumer reviewed before editing.
- [x] Canonical correction plan recorded as the first source-tree modification.
- [x] Two unresolved formatter calls corrected.
- [x] Final diff and static integration audit completed (`16 / 16`).
- [x] Changed-file package and re-extraction hash parity completed.
- [ ] Unity 6000.5.0f1 compilation completed with no errors or new warnings.

## GM-SURFACE.5L-DIAG — Bidirectional BRDF workflow parity sweep

Status: **source implementation and offline package audit complete; Unity compilation and runtime validation pending**.

### Objective

Replace the order-sensitive single-light ownership inference with one controlled same-mesh BRDF parity sweep that directly tests the strongest current hypothesis: the legacy metallic-zero dielectric workflow and the current HLSL specular workflow use materially different reflectance inputs. The diagnostic must preserve signed over-response and under-response evidence across deterministic light directions and must not select a production correction before Unity evidence is reviewed.

### Approved files

- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassSurfaceCausalityRenderAudit.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassBevelShadingDiagnosticSuite.cs`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceForwardPass.hlsl`

No files are created, deleted, moved, or renamed. Existing Inspector controls are reused.

### Reviewed evidence

- The completed GM-SURFACE.5K-H1 report used the same frozen mesh and restored all source renderer material/property-block state, but its terminal `SPECULAR_RESPONSE_MISMATCH` result had zero confidence, used first-match classification, and compared only one scene-light configuration.
- The legacy Shader Graph exposes metallic workflow with metallic zero. The HLSL forward shader defines `_SPECULAR_SETUP` and the suspect material supplies `_SpecularStrength = 0.16`; this changes both diffuse energy retention and angle-dependent specular response relative to a normal dielectric reflectance near 0.04.
- Existing class masks render isolated class-only meshes, so hidden class triangles can win depth where another class is frontmost in the final render.
- Existing matched-light cases set `_SpecularHighlights` as a float but do not synchronize the legacy `_SPECULARHIGHLIGHTS_OFF` keyword, and `_SpecularStrength = 0` is not energy-equivalent to metallic-zero diffuse.
- The current audit remains incremental, cancellable, asynchronous, checkpointed, and source-renderer invariant. Those contracts must remain unchanged.

### Acceptance criteria

1. Render one full-mesh depth-tested triangle-identity pass per subject. Every visible pixel must resolve to the actual frontmost final triangle and its provenance class; separate class-only mask meshes are retired.
2. Use eight deterministic object-relative main-light directions with one controlled white directional light, fixed intensity, no shadows, no additional lights, no ambient/GI, no reflections, no fog, no post-processing, constant neutral albedo, stored mesh normals, and one frozen mesh.
3. For every direction render the legacy metallic-zero baseline, current HLSL F0 0.16, and temporary HLSL F0 0.04 candidate. Material assets and source renderers remain untouched.
4. Preserve per-visible-triangle signed residuals. Report over-response counts, under-response counts, mean/P90 absolute residuals, source-face and ordinary-bevel coverage, and exact parent–bevel–parent ordering changes.
5. After Stage A, select the two directions with the largest current-HLSL residual and append Stage B cases: actual-albedo legacy/current/F0-0.04 PBR plus a genuine highlight-disabled, diffuse-energy-matched legacy/HLSL pair.
6. Legacy highlight disabling must synchronize `_SpecularHighlights = 0` with `_SPECULARHIGHLIGHTS_OFF`; the current HLSL diffuse case must use zero specular strength and a 0.96 albedo energy normalization.
7. Terminal BRDF evidence must be matrix-based, not first-match/order-dependent. It must distinguish confirmed F0 mismatch, contributing-but-insufficient F0 mismatch, F0 not primary, and unavailable/inconclusive evidence.
8. Production geometry, triangulation, normals, tangents, compiled masks, material assets, scenes, prefabs, layers, tags, and ordinary shader behavior remain unchanged.
9. Audit-only HLSL additions remain under `_SURFACE_CAUSALITY_AUDIT`; the production `Frag` token stream outside audit blocks remains unchanged.
10. The run remains incremental, cancellable, asynchronous, checkpointed, and Editor-responsive. No active-gameplay CPU/GPU work or persistent runtime memory is added.

### Invariants

- One accepted generation capture and one frozen mesh are reused for all render cases.
- Source `sharedMaterials`, global property block, and indexed property blocks are restored after capture, cancellation, exceptions, and finalization.
- Temporary light rotation, colour, intensity, enabled state, cookie, culling state, and `RenderSettings.sun` are restored after every case.
- Triangle identity uses a temporary triangle-soup mesh, exact uploaded-index parity, and an audit-only checksummed 16-bit ID plus 8-bit checksum vertex-colour output mode; no production mesh channel is changed.
- Existing geometric-quality, upload-parity, mask-continuity, and internal-edge facet evidence remains available.

### Non-goals

- No production F0/default change.
- No bevel luminance clamp or parent-derived lighting override.
- No geometry or triangulation work.
- No deletion of pixel variation or other artistic layers.
- No saved material, scene, prefab, profile, renderer-feature, layer, or tag edit.
- No new Inspector button.

### File-by-file sequence

1. Extend the render audit with depth-correct triangle identity, controlled directional-light cases, signed per-triangle statistics, adaptive worst-direction Stage B cases, keyword-safe legacy diffuse isolation, and matrix verdicts.
2. Extend the report with direction summaries, per-triangle response lines, parent/bevel ordering evidence, adaptive-case evidence, and the revised terminal contract.
3. Add one audit-only HLSL mode that returns the temporary triangle-identity vertex colour before any material or lighting work.
4. Update the framework and checklist only after implementation and static audits establish the actual final contract.
5. Re-read the complete changed implementation and direct consumers; compare final files with the GM-SURFACE.5K-H1 baseline; run scope, signature, delimiter, preprocessor, obsolete-API, keyword, production-path parity, and archive re-extraction checks.

### Affected modules and cross-subsystem impact

- Generated Mass editor diagnostics gain editor-only temporary meshes, light overrides, readback analysis, and report data.
- The shared Pixel Surface include gains one audit-only early return. Its only shader consumer is `PS3D/Pixel Surface Lit`; ordinary variants do not compile or execute the new mode.
- No runtime Generated Mass generation, material publication, scene rendering, or other subsystem behavior changes.

### Performance analysis

Stage A adds 24 deterministic BRDF cases. Stage B adds 10 cases for only the two worst directions. Six isolated class-mask renders and four contaminated matched-environment cases are removed, while one full-depth identity pass is added, yielding a net increase of 25 renders over the 71-case 5K run: `71 - 6 - 4 + 1 + 24 + 10 = 96`. At the measured 5K rate, `68.60918 × 96 / 71 = 92.77` seconds, so the analytical editor-time estimate is approximately 93 seconds at 384×384. The run remains asynchronous and cancellable after every case. Transient memory is one 384×384 colour target/readback plus bounded per-triangle aggregates and one temporary triangle-soup identity mesh. Active-gameplay runtime cost is zero. No `PERFORMANCE EXCEPTION` is required.

### Risks and mitigations

- **Triangle-ID quantization or index drift:** require each captured final-triangle index triplet to match the uploaded mesh before rendering; encode `triangleIndex + 1` into two constant `Color32` channels plus a nonlinear checksum byte; render into linear ARGB32 with no MSAA or dynamic resolution; and fail closed on missing, checksum-invalid, or out-of-range IDs.
- **Legacy keyword drift:** apply explicit enable/disable keyword sets on cloned materials and report the requested keyword state for each case.
- **Light contamination:** disable all scene lights except one controlled directional light, override the audit layer/culling state, and restore every mutated light field plus `RenderSettings.sun` in `finally`.
- **Occluded centroid samples:** require the identity pass to match the exact expected triangle ID in the sample neighbourhood.
- **Report growth:** retain per-triangle luminance only for the 34 BRDF render cases and emit complete signed triangle-comparison lines once per Stage A direction; retain aggregate summaries for the rest of the tournament.
- **False production conclusion:** keep `productionFixSelected=0`; Unity evidence and visual review remain mandatory before any material/default edit.

### Validation and compliance status

- [x] Repository instructions reloaded.
- [x] Current implementation, direct Inspector caller, shader consumer, legacy workflow evidence, canonical documents, and 5K limitations reviewed before editing.
- [x] Canonical 5L plan recorded as the first source-tree modification.
- [x] Depth-correct, checksummed triangle identity implemented with exact uploaded-index parity and statically verified.
- [x] Controlled eight-direction Stage A and adaptive two-direction Stage B implemented.
- [x] Signed per-triangle and parent/bevel ordering report implemented.
- [x] Matrix BRDF verdict implemented without first-match classification.
- [x] Audit-only shader mode implemented with exact four-line audit-branch diff and production-path parity verified.
- [x] Final six-file scope and consistency audit passed (`38 / 38`); changed-file-only archive re-extraction byte/hash parity passed.
- [ ] Unity 6000.5.0f1 compilation completed with no errors or new warnings.
- [ ] Runtime 5L sweep completed and the complete report reviewed.

## GM-SURFACE.5M-DIAG — Exhaustive per-triangle bidirectional shader-response capture

Status: source implementation and offline static audit complete; Unity compilation and runtime validation pending.

### Objective

Replace the failed interpolated triangle-identity contract with a fail-closed per-triangle identity stream and expand the BRDF sweep so one successful run records signed current-versus-legacy response over a complete deterministic directional basis. The diagnostic must produce no serialized scene/material writes and must not alter production geometry, material defaults, or ordinary shader behaviour.

### Reviewed evidence

- The GM-SURFACE.5L run completed 86 cases but its first identity case reported 21,051 valid and 21,082 invalid pixels. Every dependent render then lacked a whole-object identity mask; no BRDF conclusion was valid.
- The identity mesh duplicates all three vertices of every uploaded triangle. Its encoded vertex colour was nevertheless interpolated by the ordinary varying, making checksum bytes invalid at triangle coverage boundaries.
- The shared shader target remains 3.5. A direct fragment `SV_PrimitiveID` dependency would materially change the production shader platform contract. The equivalent audit-only contract is a `nointerpolation` varying sourced from the already duplicated per-triangle vertices.

### Approved implementation scope

Modify:

- this canonical architecture document;
- `Generated_Mass_Framework.md`;
- `Generated_Mass_Feature_Implementation_Checklist.md`;
- `GeneratedMassSurfaceCausalityRenderAudit.cs`;
- `GeneratedMassBevelShadingDiagnosticSuite.cs`;
- `PixelSurfaceForwardTypes.hlsl`;
- `PixelSurfaceForwardPass.hlsl`.

No material, scene, prefab, profile, geometry, triangulation, layer, tag, shader target, or Inspector action changes are permitted.

### Acceptance criteria

1. The audit-only varying carries the duplicated triangle ID with `nointerpolation`; production variants retain their existing varying layout and calculations.
2. Identity uses a direct 24-bit `triangleIndex + 1` value with zero reserved for background; no checksum or interpolated reconstruction remains.
3. The suite stops immediately when identity validation fails; no dependent lighting tournament is executed.
4. A successful run tests the six axes, twelve edge diagonals, eight cube corners, and current scene-main-light direction.
5. Signed per-triangle over-response, under-response, and parent/bevel ordering changes are retained for legacy dielectric, current HLSL F0 0.16, and temporary HLSL F0 0.04 cases.
6. The frozen uploaded vertex/index buffers, provenance mapping, renderer/material/property-block restoration, keyword contracts, finite-value contracts, and complete case accounting must all pass before an ownership verdict is emitted.
7. No production correction is selected by this implementation.

### Performance

The expanded sweep is editor-only, manually invoked, incremental, cancellable, checkpointed, and uses asynchronous GPU readback. It adds zero active-gameplay CPU/GPU work and zero persistent runtime memory. Case count scales linearly with the 27 deterministic directions. Production shader token flow outside `_SURFACE_CAUSALITY_AUDIT` remains unchanged.


## GM-SURFACE.5N-DIAG — Dedicated identity and complete lighting ownership capture

### Status

- Approved by the user on 2026-07-31.
- Source implementation and offline consistency audit complete; Unity 6000.5.0f1 compilation and runtime validation pending.

### Objective

Replace the failed shared-forward-pass triangle identity path with a dedicated editor-only depth-tested identity shader, then capture one self-validating 190-case same-mesh lighting matrix that separates BRDF F0/workflow, stored versus generated normals, pre-light albedo, view-dependent specular response, indirect response, and actual-scene closure.

### Reviewed evidence

- The GM-SURFACE.5M report terminated after one case with zero foreground identity pixels and `TRIANGLE_IDENTITY_CONTRACT_FAILURE`.
- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader` declares `_SURFACE_CAUSALITY_AUDIT` as fragment-only while the failed 5M identity path conditionally added a vertex-to-fragment varying under the same keyword.
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassSurfaceCausalityRenderAudit.cs` destroys temporary GPU render resources immediately after dispatch and before asynchronous readback completion.
- Existing 5M BRDF records store luminance only and accept two triangles per class, which is insufficient for the requested complete ownership decision.

### Approved file scope

Create:

- `Assets/Game/Procedural/Masses/Editor/GeneratedMassTriangleIdentityAudit.shader`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassTriangleIdentityAudit.shader.meta`

Modify:

- `Assets/Game/Procedural/Masses/Editor/GeneratedMassSurfaceCausalityRenderAudit.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassBevelShadingDiagnosticSuite.cs`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceForwardTypes.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceForwardPass.hlsl`
- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`

Reviewed but unchanged:

- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader`

### Invariants and non-goals

- No production material values, shader defaults, geometry, triangulation, scenes, prefabs, profiles, layers, tags, or Inspector controls change.
- The dedicated identity shader is editor-only and has no production shader variant or active-gameplay cost.
- The source renderer material array and all global/indexed property blocks remain restored on completion, cancellation, exception, and early contract failure.
- A failed preflight terminates before any dependent lighting case.
- No ownership verdict is emitted unless all 190 requested one-subject cases complete without readback or identity error.

### Implementation sequence

1. Add the dedicated identity shader and exact asset-path loading/preflight.
2. Remove 5M triangle identity data from the shared forward varying and fragment path.
3. Retain temporary render resources until asynchronous readback completion.
4. Replace the 5M tournament with 27-direction Stage A, four-direction Stage B, two-direction/two-view Stage C, and indirect/actual-scene Stage D.
5. Capture per-triangle linear RGB, luminance, parent IDs, stored-normal predictions, signed residuals, ordering, and coverage.
6. Write a complete per-triangle CSV beside the text report.
7. Apply fail-closed completeness and mechanical ownership rules.
8. Run final scope, shared-shader, symbol, archive, and documentation audits.

### Performance analysis

The patch adds zero active-gameplay CPU/GPU work and zero persistent runtime memory. The editor-only run performs 190 renders at 384×384 for one subject. Complexity is O(C × P + C × T), where C=190 cases, P=147,456 pixels, and T is the captured triangle count. Readback and reduction remain incremental, asynchronous, cancellable, and bounded to one case at a time. Temporary retained resources are one camera, renderer object, mesh/material clone, and render texture per in-flight case.

### Acceptance contract

- Dedicated identity shader loads by exact asset path, is supported, and has its named pass.
- CPU identity encode/decode round-trip failures equal zero for every triangle.
- Identity valid pixels are at least 1,024, invalid pixels equal zero, at least eight triangles are visible, and foreground bounds are at least 16×16 pixels.
- One-subject run completes exactly 190 cases with zero readback errors.
- Every required direction, adaptive direction, view, and closure case is present.
- At least 90% of identity-visible triangles have statistics in each comparison case; direction decisions require at least half of each visible source/bevel population, clamped to two-to-eight predicted-lit triangles per class, and at least twelve evaluable directions overall.
- Source renderer state restores exactly and no serialized scene or material write occurs.
- The report emits one mechanical verdict: `BRDF_F0_016_PRIMARY`, `GENERATED_NORMAL_PRIMARY`, `PRELIGHT_ALBEDO_PRIMARY`, `INDIRECT_OR_SCENE_ENVIRONMENT_PRIMARY`, `MIXED_BRDF_AND_NORMAL`, or an explicit contract failure.

### GM-SURFACE.5N implementation state

- Dedicated depth-tested identity rendering is isolated from the shared production forward interface.
- Identity uses a linear RGBA32 target, point sampling, no MSAA, duplicated vertices, and nonzero base-255 RGB24 encoding. CPU round-trip, shader support, named-pass, visible-pixel, invalid-pixel, distinct-triangle, and foreground-extent contracts are fail-closed.
- Lighting uses linear ARGBFloat rendering and RGBAFloat asynchronous readback. Any non-finite component is terminal; 8-bit colour quantization is not used for BRDF ownership measurements.
- Temporary camera, renderer, material, mesh, and render texture remain alive until readback completion.
- The decision matrix contains 190 counted cases plus two auxiliary alternate-view identity preflights. All requested cases, keyword states, readbacks, and at least 90% per-view visible-triangle coverage must pass.
- Stage A captures all 27 directions but evaluates ownership only where stored-normal prediction exposes at least half of each visible source/bevel population, clamped to a two-to-eight triangle requirement per class. At least twelve evaluable directions are required; unlit directions remain recorded rather than contaminating residual means.
- Controlled cases disable fog, post-processing, shadows, additional lights, probes, ambient contribution, reflections, and light cookies. Scene closure restores scene illumination while keeping fog and post-processing disabled so triangle ownership remains spatially valid.
- Offline source/scope/contract audit passes `84 / 84`. Exhaustive nonzero base-255 encode/decode verification passes all `16,581,375` representable triangle IDs with zero decode or zero-channel failures. Final changed-file-only archive re-extraction matches every source byte and contains exactly the approved nine files.
- No production visual correction is included. Unity compilation and runtime validation remain pending.


## GM-SURFACE.5N-H1 — Floating-point finite-check compile correction

### Status

- Unity 6000.5.0f1 reported four `CS0103` errors in `GeneratedMassSurfaceCausalityRenderAudit.CountNonFinitePixels(Color[])` because the implementation referenced an undeclared `IsFinite(float)` helper.
- Approved correction scope is compile-only: replace those four calls with explicit `float.IsNaN` / `float.IsInfinity` checks and preserve the 5N capture matrix, identity path, report contract, production geometry, materials, and shader behaviour.
- Unity compilation and runtime validation remain pending after the correction.

### Acceptance contract

- The complete changed class contains no unresolved `IsFinite` call.
- `CountNonFinitePixels(Color[])` rejects every NaN or positive/negative infinity component and accepts finite float values without changing any other audit result.
- Whole-class symbol, delimiter, duplicate-signature, case-count, scope, and archive-integrity checks pass before delivery.


## GM-SURFACE.5N-H2 — Keyword-free BRDF decomposition and readback alignment

### Objective

Repair the complete lighting-ownership suite after the 5N-H1 runtime report proved that the dedicated triangle-identity pass is valid but the legacy Shader Graph cannot enable `_SPECULARHIGHLIGHTS_OFF`. Remove all keyword-dependent diffuse isolation, validate identity-to-lighting buffer alignment explicitly, and gate the full ownership matrix behind a controlled Lambert preflight.

### Reviewed evidence

- The 5N-H1 report completed the current-view identity pass with 41,981 valid pixels, zero invalid pixels, and 92 distinct visible triangles.
- The first neutral legacy full-lighting case completed, but the following legacy diffuse case failed because the requested `_SPECULARHIGHLIGHTS_OFF` keyword remained disabled.
- The valid lighting case reported near-zero masked luminance while the identity result required a vertical readback flip, so identity and lighting buffers require an explicit relative-orientation contract before per-triangle attribution is trusted.
- The current Stage A and Stage B implementations depend on the unsupported keyword and must be replaced rather than retried.

### Approved files

Modify only:

- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassSurfaceCausalityRenderAudit.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassBevelShadingDiagnosticSuite.cs`

Create, delete, move, or rename: none.

### Invariants and non-goals

- Do not modify production materials, shader defaults, geometry, triangulation, scenes, prefabs, profiles, layers, tags, or Inspector controls.
- Retain the dedicated editor-only identity shader and exact base-255 nonzero triangle encoding.
- Retain asynchronous, incremental, cancellable execution and full source-renderer restoration.
- Do not infer diffuse response from an unsupported keyword.
- Do not issue an ownership verdict unless identity, alignment, Lambert preflight, case coverage, finite-value, completion, and restoration contracts all pass.

### Implementation sequence

1. Remove requested/applied specular-keyword state from render cases, material construction, reports, completion checks, and CSV logic.
2. Replace Stage A with six cases per direction: legacy neutral full, legacy black-albedo specular-only, HLSL F0 0.16 neutral full, HLSL F0 0.16 black-albedo specular-only, HLSL F0 0.04 neutral full, and HLSL F0 0.04 black-albedo specular-only. Derive diffuse as full minus specular-only per triangle.
3. Reduce Stage B to five actual-material cases per selected direction; Stage A owns diffuse/specular decomposition.
4. Add a current-view controlled Lambert preflight before Stage A. It uses the HLSL shader with constant neutral albedo, zero F0, zero smoothness, stored normals, one controlled directional light, and no ambient/shadows/additional lights/probes/fog/post-processing.
5. For every view, compare the identity foreground with the lighting foreground in both vertical orientations. Choose the higher-IoU orientation and require IoU at least 0.995 with foreground pixel-count difference no greater than one percent. Store and report the relative flip independently of projection sampling.
6. Gate Stage A queueing and all later stages on a Lambert fit with at least 32 eligible triangles, 24 positive-response triangles, mean foreground luminance at least 0.02, and normalized RMSE at most 0.05.
7. Update case totals to 209 decision cases, three auxiliary validation passes, and 212 total render passes.
8. Update summary, CSV, report, progress, and terminal decision contracts to the keyword-free decomposition and preflight fields.

### Performance

The patch remains editor-only and manually invoked. Active-gameplay CPU/GPU cost and persistent runtime memory remain zero. The complete run uses 212 bounded render/readback passes at 384×384, executes incrementally with asynchronous readback, and may be cancelled. No performance exception is requested.

### Validation

- Whole-class C# syntax, delimiter, duplicate-signature, and unresolved-symbol scans.
- Exact decision-case derivation: 1 Lambert preflight + 162 Stage A + 20 Stage B + 20 Stage C + 6 Stage D = 209 decision cases. The current-view identity plus two alternate-view identities are auxiliary, producing 212 total render passes.
- Verify no `_SPECULARHIGHLIGHTS_OFF` dependency remains in the diagnostic implementation or report contract.
- Verify alignment chooses only unchanged or vertical-flipped identity indexing and fails closed below threshold.
- Verify Lambert predicted/observed reduction uses the same visible triangle population and the selected relative orientation.
- Verify source renderer state restoration and zero serialized writes remain terminal requirements.
- Unity 6000.5.0f1 compilation and runtime report remain authoritative and pending.

### Status

- [x] Review and canonical plan recorded before implementation.
- [ ] Keyword-free cases implemented.
- [ ] Alignment and Lambert preflight implemented.
- [ ] Summary, CSV, report, and case totals reconciled.
- [x] Final scope/compliance audit complete.
- [ ] Unity compilation and runtime validation complete.


### GM-SURFACE.5N-H2 implementation state

- The dedicated identity pass remains unchanged and validated by the 5N-H1 runtime result: 41,981 valid foreground pixels, zero invalid pixels, and 92 visible triangles.
- All `_SPECULARHIGHLIGHTS_OFF` enable/disable and validation logic has been removed from the diagnostic. The shared legacy Shader Graph is no longer required to expose a specular-highlight keyword.
- Stage A now renders six cases for each of 27 directions. Black-albedo cases preserve dielectric/specular response while suppressing diffuse albedo; per-triangle diffuse is derived as `max(0, full - specularOnly)` independently for legacy, HLSL F0 0.16, and HLSL F0 0.04.
- A current-view Lambert preflight is queued immediately after identity. The complete matrix is not queued unless the preflight validates foreground alignment, controlled light publication, finite float readback, stored-normal attribution, positive response, and normalized fit error.
- Every lighting result resolves identity-to-lighting vertical orientation by foreground IoU and requires IoU at least 0.995 with no more than one-percent foreground-pixel-count difference. Projection flip and readback-buffer alignment are reported separately.
- Stage B contains five actual-material cases per selected direction; Stage A owns diffuse/specular decomposition. Stage C and Stage D retain view stability and indirect/actual-scene closure.
- The exact contract is 209 decision cases, three auxiliary identity passes, and 212 total render passes. Summary and CSV output use only the keyword-free case names and do not write synthetic zero decomposition values for stages that do not capture specular-only cases.
- No production material, shader default, geometry, triangulation, scene, prefab, profile, layer, tag, or Inspector control is modified. Unity compilation and runtime validation remain pending.

### GM-SURFACE.5N-H2 offline validation state

- Exact delta against the 5N-H1 atomic source is five files: the three canonical documents and the two editor diagnostic classes.
- The dedicated identity shader, its meta file, and both shared Pixel Surface includes are byte-identical to 5N-H1.
- Offline scope, structural, duplicate-signature, member-resolution, case-matrix, keyword-removal, black-albedo decomposition, alignment, Lambert, finite-readback, and documentation audit passes `85 / 85`.
- Unity 6000.5.0f1 compilation and runtime validation remain authoritative and pending.

## GM-SURFACE.5N-H3 — Pixelwise GPU-normal Lambert preflight correction

### Objective

Replace the invalid triangle-average Lambert gate from GM-SURFACE.5N-H2 with an exact pixelwise comparison between the GPU-interpolated stored normal field and the GPU-rendered controlled Lambert response, while leaving the dedicated identity system and Stage A/B/C/D ownership matrix unchanged.

### Reviewed evidence

- The 5N-H2 runtime report completed the current-view identity pass with 41,494 valid foreground pixels, zero invalid IDs, 87 visible triangles, and exact CPU identity round-trip.
- Identity-to-lighting alignment passed with foreground IoU `0.9999759` and foreground pixel-count difference ratio `0.00002409987`.
- The Lambert render itself was non-empty and finite with mean foreground luminance `0.37365678` and 71 visible rendered triangles.
- The terminal Lambert gate failed because it reduced each triangle to `FinalTriangleRecord.RenderNormal`, which is one averaged CPU triangle normal, while audit mode 12 evaluates the interpolated per-pixel `storedNormalWS`. The two quantities are not mathematically equivalent.
- Audit mode 14 already emits the exact interpolated stored world normal as `storedNormalWS * 0.5 + 0.5`; no shared shader change is required.

### Approved file scope

Modify only:

- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassSurfaceCausalityRenderAudit.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassBevelShadingDiagnosticSuite.cs`

Create, delete, move, or rename: none.

### Invariants and non-goals

- Do not modify production materials, shared shaders, shader defaults, geometry, triangulation, scenes, prefabs, profiles, layers, tags, or Inspector controls.
- Retain the dedicated identity shader, base-255 identity encoding, explicit identity/light readback alignment, floating-point lighting capture, asynchronous execution, cancellation, checkpointing, and source-renderer restoration.
- Do not change Stage A, Stage B, Stage C, or Stage D ownership cases or thresholds in this correction.
- Do not use CPU averaged triangle normals as a Lambert validity oracle.

### Implementation sequence

1. Queue one auxiliary current-view stored-normal capture immediately after the current-view identity pass and before the Lambert response case. Use existing audit mode 14 and the same camera/material isolation contract as the Lambert response.
2. Preserve the stored-normal capture pixels until the Lambert response completes. Resolve each Lambert lighting pixel to the corresponding stored-normal pixel through the independently validated identity-relative vertical-orientation mapping for both captures.
3. Decode each stored normal as `normalize(encoded * 2 - 1)` and evaluate three pixelwise prediction models over aligned foreground pixels: configured light direction, opposite light direction, and best scalar fit for the configured direction.
4. Replace triangle-count Lambert thresholds with pixel-level completeness and error contracts. Require at least 20,000 valid normal pixels, at least 2,000 positive expected pixels, finite samples, foreground alignment IoU at least 0.995, foreground pixel-count difference no greater than one percent, and configured-direction normalized RMSE at most 0.01.
5. Report configured/opposite RMSE, best-fit scale/RMSE, valid-normal pixel count, expected-positive pixel count, observed-positive pixel count, and mean foreground luminance. A failed preflight must identify direction reversal versus scalar mismatch versus general shader-path disagreement from these values.
6. Keep 209 counted decision cases. Add one auxiliary stored-normal capture, making four auxiliary validation passes and 213 total GPU render/readback passes.
7. Update summary, text report, completion contracts, progress accounting, and checklist/framework status to the corrected pixelwise preflight.

### Performance

The correction adds one 384×384 editor-only floating-point normal capture and one linear pixelwise comparison before the existing tournament. Active-gameplay CPU/GPU cost and persistent runtime memory remain zero. The additional transient normal buffer is bounded to one capture and is released after Lambert validation.

### Validation

- Whole-class C# delimiter, preprocessor, duplicate-signature, unresolved-symbol, and member-resolution scans.
- Exact case accounting: 209 counted decision cases, four auxiliary validation passes, 213 total render/readback passes.
- Verify the GPU-normal capture is mode 14, current-view only, and cannot queue Stage A by itself.
- Verify configured/opposite/best-fit models use corresponding pixels resolved through each result's identity-relative orientation.
- Verify no CPU triangle-average normal participates in Lambert validity.
- Verify existing Stage A/B/C/D case construction and production shader/material files are byte-identical to 5N-H2.
- Unity 6000.5.0f1 compilation and runtime report remain authoritative and pending.

### Status

- [x] Review and canonical plan recorded before implementation.
- [x] Auxiliary stored-normal capture implemented.
- [x] Pixelwise three-model Lambert validation implemented.
- [x] Summary/report/case accounting reconciled.
- [x] Final scope/compliance audit complete.
- [ ] Unity compilation and runtime validation complete.


## GM-SURFACE.5O — Cold-grey production lighting parity trial

### Status

- [x] Review complete and implementation plan recorded before production edits.
- [x] Production material F0 correction implemented.
- [x] Cold-grey whole-surface generated-normal perturbation bypass implemented.
- [x] Canonical framework/checklist reconciled.
- [x] Final scope/compliance/static audit complete.
- [x] 2026-08-07 visual validation completed: **trial is visually insufficient and rejected as a root-cause correction for the active surface-orientation ordering defect.**
- [ ] Behavioral rollback/supersession is separate future production work; 5P does not change rendering behavior.

### Objective

Historical 5O objective: test the two production changes directly supported by the completed 5N-H3 parity metrics. **This objective is superseded as a root-cause framing by GM-SURFACE.5P.** The active defect is not general BRDF/specular parity or whole-object darkness; it is incorrect per-surface and per-bevel response ordering relative to surface orientation under the same light.

### Approved file scope

Modify only:

- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Game/Procedural/Masses/GeneratedMass.cs`
- `Assets/Game/Demo/Materials/Stone/M_PixelStone_HLSL_ColdGrey.mat`

Create, delete, move, or rename: none.

### Reviewed evidence

- The completed 5N-H3 runtime matrix finished 209/209 decision cases with zero readback errors, full case coverage, a valid pixelwise Lambert preflight, and no completeness failure.
- Neutral stored-normal direct lighting proves F0 0.04 reproduces the legacy control to numerical precision while F0 0.16 introduces over-response and bevel/parent ordering inversions.
- Actual-material Stage B/C evidence shows removing the generated whole-surface normal perturbation improves parity by approximately 38–41 percent on average, but does not independently explain the full remaining mismatch.
- `Assets/Game/Procedural/Masses/GeneratedMass.cs`, `ApplyMaterialProperties`: the component publishes the serialized whole-surface normal strength through a renderer property block after applying the active stone profile material.
- `Assets/Game/Demo/Materials/Stone/M_PixelStone_HLSL_ColdGrey.mat`, `_SpecularStrength`: the cold-grey production material currently serializes 0.16.

### Acceptance criteria

1. The cold-grey production material serializes `_SpecularStrength = 0.04` and no other material property changes.
2. `ApplyMaterialProperties` publishes `_GeneratedMassSurfaceNormalStrength = 0` only when `stoneSurfaceProfile` is `ColdGreyStone`; all other stone profiles continue publishing their serialized authored strength unchanged.
3. No shared shader/include, geometry, triangulation, scene, prefab, recipe, layer, tag, or diagnostic matrix change is introduced.
4. Active-gameplay per-frame cost does not increase; the profile conditional executes only when the existing material-property publication path runs.
5. Unity validation must confirm the cold-grey rocks actually render with the new F0 and without the generated normal perturbation, and the visual comparison must determine whether the residual dark bias remains.

### File-by-file sequence

1. `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md` — record this plan first and later record final evidence/status.
2. `Assets/Game/Demo/Materials/Stone/M_PixelStone_HLSL_ColdGrey.mat` — change only `_SpecularStrength` from 0.16 to 0.04.
3. `Assets/Game/Procedural/Masses/GeneratedMass.cs` — publish zero whole-surface normal strength for `ColdGreyStone`; preserve the authored control for every other profile.
4. `Assets/Docs/Generated_Mass_Framework.md` — record the temporary production parity baseline and unresolved residual-darkness boundary.
5. `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md` — record implementation/validation state and the fact that GM-SURFACE.5 whole-rock normals remain available outside the cold-grey parity trial.

### Invariants and non-goals

- This is a production visual trial, not a new diagnostic pass.
- Do not compensate the remaining darkness with ambient, indirect, exposure, albedo, or direct-light multipliers in this patch.
- Do not change smoothness, diffuse wrap, direct strength, pixel variation, material masks, or generated geometry.
- Do not delete the whole-rock normal feature or its authoring fields; only cold-grey production publication is bypassed for this trial.
- Other stone profiles retain their current F0 values and generated-normal behavior.

### Risks and validation

- Existing serialized `surfaceNormalStrength` values must remain untouched; the trial is implemented at renderer-property publication so current scene instances immediately receive the bypass without serialized scene migration.
- The F0 correction is material-local; shared shader defaults remain unchanged.
- Static validation will compare the material YAML and `ApplyMaterialProperties` diff against the pre-edit baseline, scan for scope drift, and verify no shared shader files changed.
- Unity compilation and visual/runtime validation remain authoritative and pending after packaging.
- Offline final audit passed: exact five-file scope, material scalar-only delta, cold-grey-only normal bypass, shared shader/diagnostic byte parity, other stone-material byte parity, and C# structural/preprocessor balance.

## GM-SURFACE.5P — surface-orientation defect-definition freeze

### Status

- Approved by the user on 2026-08-07.
- Documentation/comment-only implementation complete.
- Final scope/comment-only/static audit passed.
- No rendering behavior change is authorized or introduced by this patch.

### Objective

Make the active defect impossible to misclassify in future work. The canonical problem is **per-surface and per-bevel directional-light response ordering versus geometric/stored surface orientation**, using the legacy material as the behavioral reference. General darkness and specular magnitude are secondary measurements only.

### Acceptance criteria

1. All canonical Generated Mass surface-response documents state the orientation-ordering defect before historical diagnostic conclusions.
2. Production Generated Mass material publication, whole-surface normal construction, forward lighting, and shader entry points carry source comments warning that global brightness/specular tuning is not the target defect.
3. Bevel provenance capture and both surface-causality diagnostic classes explicitly state that parent/bevel ordering and face-orientation response are the causal target.
4. The GM-SURFACE.5O F0/normal trial is recorded as visually insufficient for the active defect; its existence must not be interpreted as proof that specularity or whole-rock darkness owns the defect.
5. No shader math, material value, geometry, serialized asset, scene, prefab, profile, layer, tag, diagnostic threshold, or runtime behavior changes in 5P.

### Approved files

- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Game/Procedural/Masses/GeneratedMass.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.BevelShadingDiagnosticCapture.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassSurfaceCausalityRenderAudit.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassBevelShadingDiagnosticSuite.cs`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceForwardPass.hlsl`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGeneratedMassCore.hlsl`
- `Assets/Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader`

### Reviewed evidence

- User-supplied side-by-side visual validation on 2026-08-07 identifies the legacy bright-gray material as coherent and the new dark-gray HLSL material as incoherent at the individual-face/bevel level under the same scene light direction.
- The completed 5N-H3 suite directly records `WithinParentEnvelope`, `DarkerThanBothParents`, and `BrighterThanBothParents` bevel relationships and per-triangle light-direction evidence.
- The 5N-H3 matrix proved that changing F0 can remove a neutral BRDF mismatch, but the subsequent 5O visual trial did not repair the active per-surface orientation-ordering defect. F0 parity therefore cannot be treated as closure of this issue.
- The generated whole-surface normal ablation can change response magnitude, but the active acceptance criterion remains correct orientation-driven ordering for source faces and bevels.

### Invariants and non-goals

- The defect is not defined by whole-rock darkness.
- The defect is not defined by specular intensity or F0.
- The defect is not defined by whether one isolated bevel is absolutely dark or bright.
- The defect is whether **individual surfaces respond coherently to the same light according to their orientation**, including parent–bevel–parent ordering.
- 5P does not select a new root cause and does not author a rendering fix.
- 5P does not silently validate or invalidate geometry; geometry remains a separate evidence domain.
- 5P does not revert the 5O trial values; behavioral rollback or replacement requires its own explicit production patch.

### File-by-file sequence

1. Freeze this definition and the 5P plan in the canonical architecture document.
2. Mirror the invariant into the framework and implementation checklist, and mark the 5O visual result as insufficient for defect closure.
3. Add the same invariant as comments at Generated Mass material publication and whole-surface normal construction boundaries.
4. Add the invariant at the production forward-lighting and shader entry boundaries.
5. Add the invariant to generation-time bevel provenance capture and both editor diagnostic classes, especially parent-envelope/order analysis and report interpretation.
6. Audit the final diff for comment/document-only behavior and verify no executable token or serialized value changed.

### Risks and validation

The primary risk is future semantic drift: a later agent may optimize average brightness or specular parity and declare success while orientation-order inversions remain visible. The final audit therefore checks both wording consistency and executable-code identity after stripping added comments. Unity runtime validation is not required for a comment/document-only patch, but the next rendering patch must validate the actual face/bevel orientation-order behavior visually and numerically.

Final 5P audit result: exactly ten approved files differ from the post-5O baseline; all seven source/shader diffs are line-comment/XML-comment/blank-line only with zero changed executable lines; the three canonical documents contain the frozen orientation-ordering invariant; the cold-grey material and all files outside the approved scope are byte-identical to the post-5O baseline.

