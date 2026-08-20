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


## GM-SURFACE.5Q-DIAG — exhaustive surface-orientation stage-attribution capture

### Status

- Approved by the user on 2026-08-07.
- Plan recorded before implementation.
- Stage E implementation authored within the approved six-file scope.
- Final offline source/scope/symbol/mode/production-variant audit passed (`71 / 71`); final changed-file-only archive extraction/integrity audit passed.
- Unity 6000.5.0f1 runtime evidence completed on 2026-08-07: 3,677/3,677 counted cases, 3,681 total render/readback passes, all 3,468 Stage E orientation cases, zero completeness failure, valid identity/alignment/Lambert contracts, and complete renderer-state restoration.

### Objective

Build one deliberately heavy, fail-closed diagnostic that determines **where the new HLSL path first breaks the expected relationship between actual surface orientation and observed lighting response**. The diagnostic must capture enough evidence in one successful run to distinguish raw mesh-mask ownership, ordinary pixel-value breakup, exposure scaling, mottle, exposure tinting, crevice response, base response, dirt/deposit response, wet/frost/monolithic closure, normal choice, direct-light multiplication, and final PBR behavior.

The test is not optimized for runtime. Completeness, reproducibility, per-pixel provenance, and per-triangle causal attribution have priority over case count.

### Approved file scope

Modify only:

- `Assets/Docs/Generated_Mass_Surface_Response_Architecture.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassSurfaceCausalityRenderAudit.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassBevelShadingDiagnosticSuite.cs`
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceForwardPass.hlsl`

Create, delete, move, or rename: none.

### Reviewed evidence

- The 5N-H3 run completed 209/209 decision cases, four auxiliary validation passes, zero readback errors, full triangle coverage, and a valid pixelwise stored-normal Lambert contract. The underlying identity/readback/light-direction infrastructure is therefore reusable.
- The 5N-H3 Lambert preflight measured configured-direction normalized RMSE `6.684517E-08` with best-fit scale exactly `0.5`, proving the controlled stored-normal `N·L` path itself is numerically coherent.
- Neutral stored-normal HLSL at dielectric F0 `0.04` matched the legacy neutral control to numerical precision, but actual-material stored-normal Stage B retained a large residual. This localizes a major discrepancy to the actual pre-light material/value path rather than the basic light-vector dot product.
- The current mesh-output contract stores exposure in `Color.g`, crevice/base in `Color.b`, and dirt/deposit in `UV2.y`; source-face masks are generated from source-face normal/height terms and generated bevel interiors inherit/interpolate source-boundary samples.
- The current forward path applies ordinary tonal breakup, exposure semantic scaling, generated-mass mottle, exposure tint, crevice, base, dirt/deposit, wet-damp, frost, global wet darkening, and monolithic closure before PBR receives the final albedo.
- Existing audit modes already expose base colour, tonal colour, exposure-scaled colour, mottle colour, post-semantic colour, stored normal, direct-only response, and full PBR. 5Q extends this audit-only surface rather than altering production rendering.
- The 5O visual trial retained the orientation-ordering defect after F0 parity and the cold-grey generated-normal bypass. 5Q must therefore test per-surface orientation/value causality directly instead of tuning global lighting magnitude.

### Acceptance criteria

1. Preserve the complete, already-working 5N-H3 identity/readback/Lambert and Stage A/B/C/D contracts.
2. Add an exhaustive Stage E orientation-causality matrix using the current view plus both already-validated alternate views and every deterministic light direction already in the H3 basis.
3. Capture the complete cumulative pre-light albedo chain, raw/generated material masks, relevant scalar fields, stored/geometric/current normals, and exact stored-normal `N·L` before evaluating final direct/PBR response.
4. For every Stage E view/direction, capture GPU direct-light response after every cumulative material stage, production-HLSL full response, a stored-normal production-HLSL response, a legacy actual-material control, and targeted one-layer ablations for the suspected HLSL-only value authorities.
5. Attribute every visible pixel to an exact triangle through the validated dedicated identity map; retain linear floating-point RGB and reject any non-finite sample.
6. Compute per-triangle and per-logical-bevel orientation evidence: mean `N·L`, cumulative stage value, direct response, orientation-normalized response, parent/bevel/parent ordering, source-face rank inversions, first divergent stage, and one-layer ablation improvement.
7. A bevel-parent envelope violation counts as an orientation defect only when the measured bevel `N·L` is itself between the two parent `N·L` values within tolerance. Brighter-than-both/darker-than-both is not universally invalid when the bevel normal legitimately faces the light more/less directly than both parents.
8. Emit a dedicated orientation-attribution CSV in addition to the existing BRDF CSV so the complete raw per-triangle evidence survives report summarization.
9. Fail closed unless every expected Stage E family/view/direction/stage is present, every identity/alignment contract passes, coverage is complete enough for attribution, and all GPU values are finite.
10. Do not change material values, production shader behavior, geometry, mask generation, normal generation, scenes, prefabs, profiles, layers, tags, or Inspector controls.
11. Keep the run incremental, cancellable, Editor-responsive, checkpointed, and ETA/progress aware despite the intentionally large case count.

### Planned Stage E capture surface

Static captures per validated camera view:

- triangle/geometric normal;
- stored normal;
- current resolved normal;
- raw mesh material channels;
- generated height/upwardness and contract fields;
- resolved exposure/crevice/base/dirt masks;
- nonlinear exposure/crevice/base/dirt visual masks;
- mottle field and material-response scalars;
- base albedo;
- after ordinary tonal breakup;
- after exposure semantic scaling;
- after mottle;
- after exposure tint;
- after crevice;
- after base/contact;
- after dirt/deposit;
- after wet-damp gathering;
- after frost;
- after global wet darkening;
- final monolithic/pre-PBR albedo.

Directional captures for every deterministic light direction and validated camera view:

- constant-neutral stored-normal direct response;
- cumulative stored-normal direct response after every pre-light stage listed above;
- actual production-HLSL full PBR response;
- actual production-HLSL stored-normal full PBR response;
- actual legacy material response on the same frozen mesh;
- targeted final-albedo direct/full-PBR ablations with pixel/broad/warp breakup, exposure, mottle, crevice, base, dirt/deposit, and all generated semantic value authorities disabled one at a time and together.

### Metrics and decision evidence

The report must not select a fix from whole-object averages. It must produce:

- per-stage source-face Spearman/Pearson orientation correlation;
- pairwise source-face orientation-order inversion counts using a minimum `N·L` separation to reject near-ties;
- parent–bevel–parent cases where bevel `N·L` is genuinely intermediate and the first cumulative material stage that causes a lighting-envelope escape;
- per-stage direct-response self-consistency against captured albedo × captured `N·L`;
- per-triangle residual introduced by each cumulative pre-light stage;
- correlation of that residual with raw exposure, crevice, dirt, height/upwardness, and mottle fields;
- one-layer ablation reduction in orientation inversions and residuals;
- legacy versus HLSL orientation-ranking disagreement under the same frozen mesh/light/view;
- a ranked `firstDivergentStage`/`dominantAblation` summary backed by the raw triangle rows.

The successful result may be `no single dominant stage`; the test is still valid if it preserves the complete evidence necessary to support that conclusion without another instrumentation patch.

### File-by-file sequence

1. Update this canonical plan first.
2. Extend the audit-only forward modes with raw masks, cumulative albedo checkpoints, exact `N·L`, and cumulative direct checkpoints; production variants remain token/behavior equivalent outside `_SURFACE_CAUSALITY_AUDIT`.
3. Extend the render-audit state machine after Stage D with the exhaustive Stage E static, directional, and ablation matrix and fail-closed completion accounting.
4. Add orientation-specific per-triangle analysis, first-divergence/ablation ranking, and summary fields.
5. Extend report/CSV output with the dedicated Stage E evidence and corrected conditional parent-envelope interpretation.
6. Update the framework/checklist with 5Q status, then run final scope, production-token-isolation, matrix-count, symbol, and package-integrity audits.

### Implemented Stage E matrix

- 13 cumulative albedo/direct checkpoints: base, tonal, exposure scale, mottle, exposure tint, crevice, base/contact, dirt/deposit, wet-damp, frost, global wet darkening, final pre-light/monolithic, and final overall tint.
- 6 raw/resolved mask/scalar captures plus triangle/current/stored normal captures per view.
- 27 controlled directions × 3 validated views.
- Per direction/view: exact GPU NdotL/attenuation, encoded GPU main-light direction, 13 cumulative direct checkpoints, legacy actual PBR, HLSL production PBR, HLSL stored-normal PBR, and 12 direct + 12 PBR ablations.
- Stage E case count: `3 × (22 + 27 × 42) = 3,468`. Existing H3 counted matrix: `209`. Total counted cases: `3,677`. Existing auxiliary identity/validation passes: `4`. Total GPU render/readback passes: `3,681`.
- Static cumulative albedo and directional NdotL/attenuation readbacks are retained only as managed audit evidence needed for exact pixelwise direct-product validation, then released when the audit is disposed.
- Pixelwise direct-product attribution is fail-closed across render cases: the cached albedo, cached NdotL/attenuation, and direct checkpoint must report the same identity-relative readback orientation and identical lighting-foreground pixel count before their pixel indices are multiplied.
- Every cumulative direct checkpoint reports pixel count, mean absolute residual, and normalized RMSE against captured `albedo × NdotL × distanceAttenuation × shadowAttenuation`.
- Per-triangle orientation evidence additionally preserves luminance min/P10/median/P90/max/std-dev, provenance, source-group IDs, stored/authored/geometric normals, geometry condition/aspect/minimum angle, and all captured CPU mask/structural endpoint values.
- The dedicated orientation CSV is streamed at finalization rather than constructed as one monolithic in-memory string. Render checkpoints remain incremental but are persisted every 32 completed render passes (plus startup/error/final boundaries) so the intentionally large run does not become quadratic in report-rebuild cost.

### Risks and validation

- The main risk is case-count/report-size growth. The run is intentionally heavy; responsiveness and cancellation are mandatory, not total duration.
- Audit-only shader branches must compile only under `_SURFACE_CAUSALITY_AUDIT`; production Generated Mass rendering must be byte/behavior equivalent outside that keyword.
- Existing H3 identity maps are reused for the same three views. Any added view is out of scope unless the plan is updated first.
- Legacy Shader Graph internals are not instrumented. Legacy is therefore captured as the actual same-mesh behavioral reference, while exact stage attribution is performed inside the HLSL path.
- Unity compilation/GPU execution completed successfully for the submitted 5Q evidence run. The resulting Stage E ownership record is preserved by GM-SURFACE.5R and is now the authoritative causal basis for the production material-response correction.

## GM-SURFACE.5R — Orientation-coherent material response baseline

Status: **implemented; static audit passed; user visual acceptance recorded 2026-08-09**.

### Why this patch exists

GM-SURFACE.5Q completed the full surface-orientation causality matrix instead of relying on whole-object averages. The run completed 3,677 counted cases, including all 3,468 Stage E orientation cases, with no completeness failure. Its controlled Lambert preflight matched the captured interpolated GPU normal field to approximately `1e-7` normalized RMSE, and every cumulative direct-light checkpoint matched the independently captured product of pre-light albedo, `NdotL`, distance attenuation, and shadow attenuation with normalized RMSE `0`.

Those results prove that the fundamental direct-light equation and published light direction are coherent. The active visual defect is introduced **before** that multiplication, by material-value authorities that encode geometry/upwardness/height or topology-sensitive tonal variation into pre-light albedo.

### Permanent problem record

The defect to preserve across sessions is:

> Under one incident light, individual Generated Mass source faces and ordinary bevel faces can render in a bright/dark ordering that contradicts their measured orientation to the light. The legacy material is accepted because its visible face ordering tracks orientation coherently. Global darkness, average PBR residual, specular/F0, and whole-rock exposure are not defect definitions.

5Q isolated the material chain using 1,514 source-face orientation comparisons and 1,364 conditional bevel comparisons across the deterministic direction/view sweep:

| Cumulative stage | Source inversions | Newly introduced source inversions | Conditional bevel violations | Newly introduced bevel violations | Interpretation |
| --- | ---: | ---: | ---: | ---: | --- |
| BASE | 0 | 0 | 64 | 64* | Constant-base direct response follows source-face `NdotL` exactly. |
| TONAL | 0 | 0 | 189 | 130 | Major bevel-only failure family. |
| EXPOSURE_SCALE | 28 | 28 | 216 | 39 | First real source-face orientation inversions. |
| MOTTLE | 27 | 0 | 205 | 1 | Minor contributor. |
| EXPOSURE_TINT | 27 | 0 | 205 | 0 | No additional ordering damage in the tested state. |
| CREVICE | 65 | 38 | 191 | 23 | Largest newly introduced source-face batch. |
| BASE_LAYER | 80 | 15 | 191 | 11 | Significant additional source-face damage. |
| DIRT | 80 | 0 | 186 | 2 | Not a primary owner. |
| WET/FROST/final closure | 80 | 0 | 186 | 0 | No further source-face inversion growth in the tested state. |

`*` The 64 BASE bevel count is a known classifier floor, not proof that constant base colour is wrong. The Stage E conditional bevel gate permits a wider parent-interval tolerance in `NdotL` than in luminance. Because constant-base luminance is proportional to `NdotL`, this can classify small, tolerance-allowed normal differences as luminance-envelope escapes. Production decisions therefore use **introduced stage errors**, direct-product identity, and the raw orientation rows instead of treating `firstDivergentStage=BASE` literally.

The source-face result is not affected by that classifier issue: BASE and TONAL have zero source-face inversions, while EXPOSURE, CREVICE, and BASE add 28, 38, and 15 respectively.

### Correlation evidence

The first source-face failure stage, `EXPOSURE_SCALE`, had exposure correlation `0.9957553` and height correlation `0.9000925`. The `CREVICE` stage had crevice correlation `-0.9678729` and height correlation `0.9795084`. `BASE_LAYER` retained very strong exposure/crevice/height correlations while increasing the inversion count to 80.

This establishes that the problematic value response is strongly tied to the generated semantic fields rather than to random light-vector error.

### Ablation evidence

The final pre-light state contained 80 source-face inversions and 186 conditional bevel violations, for a combined count of 266. Disabling **all pre-light value authorities** produced:

- source-face inversions: `0`;
- conditional bevel violations: `64` (the constant-base classifier floor);
- combined count: `64`;
- reduction from baseline: `0.7593985` (`75.93985%`).

Removing specular produced no reduction: `80` source inversions, `186` bevel violations, combined `266`. Wet, frost, monolithic response, and overall tint likewise produced no reduction in the tested state. The 5O F0/normal trial therefore remains rejected as a root-cause fix for the orientation defect.

Individual one-layer ablations are not interpreted as simple additive ownership because the layers interact nonlinearly: removing one wrong layer can expose or cease to cancel another wrong layer. The cumulative stage introduction counts are the authoritative evidence for where incoherence first appears.

### Architectural conclusion

Generated Mass semantic masks describe **material state**, not incident illumination.

The following rule is now permanent:

1. Exposure/upwardness/height may influence tint, weathering eligibility, roughness, accumulation, or other material identity, but must not brighten direct-light albedo strongly enough to override real `NdotL` ordering.
2. Crevice and base/contact masks may describe occlusion/material identity, but must not pre-darken direct-light albedo toward fixed dark targets. If stronger grounding is needed later, it belongs in a separately validated indirect/AO contract, not as fake direct-light loss.
3. Generated bevel tonal breakup must be independent of generated triangulation and interpolated source-face tonal authority. The bevel may retain topology-independent world-position pixel-cell variation.
4. Direct-light `NdotL`, attenuation, PBR light-vector math, geometry, and mask-generation ownership remain unchanged until evidence says otherwise.

### 5R production changes

The shared forward path now applies the rule above:

- Generated Mass exposure semantic luminance scale is fixed to `1.0`. Ground semantic scaling is unchanged. The separate value-preserving Generated Mass exposure tint path remains available.
- Generated Mass crevice no longer lerps albedo toward a dark neutral target. The mask can only blend toward `PS3D_ApplyValuePreservingTint(...)` using the existing crevice tint control.
- Generated Mass base/contact no longer lerps albedo toward a dark neutral target. The mask can only blend toward the existing value-preserving base tint.
- Convex Generated Mass bevels are identified from the packed `ConvexBoundary` structural contribution. On those bevels, interpolated vertex-R tonal authority, broad tonal variation, and cell-warp authority are suppressed. Shared world-position pixel-cell variation remains.
- The cold-grey material baseline matches the legacy pixel amplitudes: Pixel Variation `0.057`, Vertex Variation `0.09`, Profile Pixel Contrast `1.0`; HLSL-only Broad Variation and Cell Warp are `0` for this baseline.
- F0 remains `0.04` and the retained 5O cold-grey generated-normal bypass remains in place for this trial. Neither is claimed as the 5R causal fix.

### Cross-profile and ground impact

The exposure/crevice/base semantic contract is shared by Generated Mass stone profiles, so the shader-side removal of fake pre-light luminance authority applies to all Generated Mass profiles using this forward path. It is intentionally not limited to ColdGreyStone because 5R is an architecture correction: material-semantic masks must not masquerade as direct illumination.

Ground rendering remains on its existing semantic-scale path and does not use the Generated Mass crevice/base tint-only branch as a substitute for its ground response.

Only the cold-grey serialized material receives the legacy tonal-baseline value changes in this patch. Other Generated Mass stone materials keep their existing material values and require later profile-specific visual review if 5R is accepted.

### Acceptance and rollback criteria

5R is accepted only if same-scene visual comparison shows that:

- source-face brightness ordering again follows surface orientation relative to the same light;
- bevel segments no longer show unexplained bright/dark ordering that contradicts their orientation;
- the old-material/new-material comparison materially converges in orientation behavior, irrespective of any remaining global luminance difference.

If those criteria fail, do not tune specular, exposure, or global brightness as a substitute. Preserve the 5Q evidence and continue from the remaining pre-light/PBR differential. A failed 5R visual trial would mean another value authority or later PBR/indirect closure remains active; it would not invalidate the proven direct `NdotL`/attenuation product.

### Validation status

- Source implementation: complete.
- Canonical evidence/problem documentation: complete.
- Static scope/shared-shader/material-diff audit: passed (`39 / 39`).
- Unity 6000.5.0f1 compilation: user-applied session completed without a reported compile error; independent model-side reproduction was unavailable.
- Same-scene visual orientation comparison: user accepted on 2026-08-09.

## GM-SURFACE.5S — low-light directional form readability

Status: **implemented in source; static audit passed `58 / 58`; Unity compile/visual/performance validation pending**.

### Separation from 5R

5R is accepted for the former orientation-incoherence defect. 5S does not reopen that diagnosis. The new failure mode occurs when genuine direct illumination becomes weak enough that the remaining indirect response provides insufficient plane-to-plane contrast, even though the direct-light equation and pre-light material semantics remain correct.

The permanent 5R rule remains unchanged: Generated Mass exposure/upwardness/height, crevice, base/contact, dirt, and structural/bevel identity are material descriptors. They do not decide which surface is illuminated. Low-light readability belongs to the illumination/indirect-light layer and must derive from an actual light direction plus the resolved fragment normal.

### Architecture

The pre-5S production baseline constructed baked GI from `SampleSH(normalWS)` and supplied that value directly to URP PBR. 5S retains that source but now applies one bounded Generated-Mass-only directional multiplier before PBR:

```text
mainLight = GetMainLight()
sourceLuma = dot(mainLight.color, (0.2126, 0.7152, 0.0722))
sourceGate = saturate(sourceLuma)
facing = clamp(dot(normalWS, mainLight.direction), -1, 1)
wrap = saturate(_DiffuseWrap)
wrappedFacing = lerp(facing, max(facing, 0), wrap)
targetScale = 1 + 0.40 * wrappedFacing
formWeight = saturate(_ShadowAmbientStrength) * sourceGate
shapedBakedGI = bakedGI * lerp(1, targetScale, formWeight)
```

The bare main-light query is intentional. Current official URP source returns base main-light direction/color and unit shadow attenuation from that overload; position-aware overloads apply shadow/cookie attenuation. Therefore the form cue can preserve Sun-direction information while the real PBR direct term continues to respond to Weather's local cloud cookie. The supplied code-only archive does not include the project's installed package source, so exact Unity 6000.5.0f1 package-source parity remains a runtime-environment verification item.

5S deliberately does not multiply by main-light shadow attenuation or sample the cookie, because doing so would suppress the cue under the exact cloud-shadow condition that requires it. The bare main-light culling eligibility term is not consumed by the approved formula; if Generated Mass is intentionally excluded from the authoritative Sun culling mask, that edge case must be evaluated explicitly rather than changing the lighting contract implicitly.

Ground keeps raw `SampleSH(normalWS)`. Generated Mass receives the shaped value through one centralized baked-GI resolver used by production `BuildInputData`. Audit ambient/PBR mirror paths use the same resolver so actual-scene diagnostic output cannot silently diverge from production, while direct-only diagnostic modes remain unchanged.

### Control and serialization contract

No new shader property is introduced. Existing serialized controls are activated and relabeled only at the shader UI level:

- `_ShadowAmbientStrength` → **Low-Light Form Strength**;
- `_DiffuseWrap` → **Low-Light Form Wrap**.

Property names, defaults, CBUFFER ordering, and all material YAML remain unchanged. Existing stone profiles therefore activate their already-authored values without migration.

Generated Mass owns the authoring location for strength. The component publishes an object-level **Low-Light Form Strength** override through its existing renderer `MaterialPropertyBlock` path to `_ShadowAmbientStrength`, so one mass keeps the same 5S strength across compatible stone materials and profile swaps. The current default authored value is `0.42`, matching the prior effective ColdGrey baseline. This patch does not add an object-level Wrap control; `_DiffuseWrap` remains material-owned. Source implementation is complete; Unity compile and scene validation remain pending.

With an eligible white main light and current ColdGrey-aligned default values (`Strength 0.42`, `Wrap 0.12`), the approximate indirect multipliers are `1.168` facing, `1.0` perpendicular, and `0.852` opposite. Strength zero is exact 5R parity.

### Invariants

- no world-up/`normal.y`, height, exposure, crevice, base/contact, dirt, feature type, or bevel identity may choose the bright side;
- no emissive or minimum-light floor is added;
- no direct-light, normal-generation, material-semantic, geometry, Weather, or ground-response equation is modified;
- real nearby/strong lights remain the actual direct-light authority;
- if baked GI is zero, 5S contributes zero light;
- no extra texture/cookie/shadow sample, pass, varying, buffer, draw, dispatch, CPU update, or allocation is permitted.

### Cross-subsystem impact

The forward include is shared by the Generated Mass and Ground contracts inside the same Pixel Surface shader. The change must therefore gate the new GI shaping explicitly to Generated Mass. Ground behavior must remain algebraically identical. The five HLSL stone profiles share the Generated Mass shader contract and will all begin consuming their existing strength/wrap values; that cross-profile activation is intentional and requires visual review. Weather remains a producer of the Sun cookie only and receives no code or serialized-state change. The 5Q diagnostics remain editor-only and preserve their direct-light ownership contract; controlled neutral cases already zero both activated controls.

### Performance contract

The feature adds fixed per-fragment ALU only: a base main-light struct fetch/access, two dot products, clamps/max/lerps, scalar arithmetic, and one RGB GI multiply. No additional texture or shadow lookup is added. Runtime complexity remains O(P) for P covered fragments and O(1) persistent storage. Because active-gameplay GPU work is the highest-priority cost category, analytical cost is not acceptance evidence; same-scene GPU profiling remains mandatory before final acceptance.

### Validation and rollback

Static implementation audit passed `58 / 58`: exact five-file scope, shader/preprocessor balance, approved helper formula, Ground gating, direct-only diagnostic parity, 5R invariant markers, unchanged material bytes, unchanged property names/defaults/CBUFFER declarations, and all five stone-profile serialized strength/wrap values were verified. Unity shader compilation, visual behavior, exact installed-package API provenance, and GPU timing remain pending.

5S is visually acceptable only when weak/cloud-shadowed stone remains dark while major planes remain readable, rotating the main light rotates the cue, strong real lights remain dominant, Ground is unchanged, and strength zero restores 5R. If the effect is absent because baked GI is effectively zero, do not restore semantic fake-light fields; re-evaluate the illumination-layer design. If the locally installed URP source contradicts the assumed bare-main-light semantics, stop and revise the implementation plan rather than silently substituting another lighting source.

## GM-SURFACE.5S2 — extended low-light form strength

### Evidence

Scene validation of 5S1 shows a visible difference between strength `0` and `1`, proving that the object-level authoring and directional GI path are functioning. The remaining failure is amplitude: under very uniform weak illumination, maximum strength `1` still leaves major planes with insufficient perceptual separation.

### Response extension

5S2 keeps the existing 5S response shape and extends only its usable strength domain:

```text
strength = clamp(_ShadowAmbientStrength, 0, 2)
formWeight = strength * sourceGate
shapedBakedGI = bakedGI * lerp(1, targetScale, formWeight)
```

All other terms are unchanged. Therefore every value in `0..1` produces exactly the same result as 5S1, while values above `1` continue the same signed directional modulation instead of being discarded by `saturate`.

The existing target is bounded by `wrappedFacing in [-1, 1]` and the fixed `0.40` coefficient, giving `targetScale in [0.60, 1.40]`. With `strength <= 2` and `sourceGate <= 1`, the final GI multiplier is bounded by:

```text
minimum = 1 + 2 * (0.60 - 1) = 0.20
maximum = 1 + 2 * (1.40 - 1) = 1.80
```

The approved range therefore cannot invert the sign of baked GI and does not require an artificial brightness floor.

### Low-light adaptation decision

5S2 does not add a second position-aware main-light evaluation to compute a cloud/shadow darkness gate. In URP, position-aware main-light queries perform shadow attenuation and the full position-aware overload can apply `_LIGHT_COOKIES`; the existing PBR evaluation already performs that lighting work. Duplicating it solely for the helper would add per-fragment cost in the highest-priority runtime category.

The current architecture instead relies on relative weighting: 5S modifies only indirect GI, so strong genuine direct light naturally dominates it while weak direct light exposes more of the shaped indirect contribution. This preserves the desired low-light emphasis without another shadow/cookie lookup.

### Authoring contract

Generated Mass remains the authority for **Low-Light Form Strength** and publishes the value through its existing renderer property block. The object range is now `0..2`, default remains `0.42`, and compatible material/profile changes cannot silently replace the chosen object strength. `Low-Light Form Wrap` remains material-owned and unchanged.

The ShaderLab range for the existing `_ShadowAmbientStrength` property is widened to `0..2` for contract consistency only. The property name, default, CBUFFER layout, and material serialization are unchanged.

### Performance and rollback

Relative to 5S1, 5S2 adds no new texture, cookie, shadow, buffer, pass, draw, dispatch, CPU update, allocation, or generated-data work. The existing scalar strength clamp is simply widened. If maximum strength `2.0` remains visually insufficient, the next step must redesign the bounded response curve or introduce a separately approved readability layer; do not raise the current signed strength blindly beyond the mathematically safe domain and do not reintroduce semantic-mask fake illumination.

### 5S2 implementation status

Source implementation is complete. Offline static/cross-subsystem audit passed `35 / 35`, including exact six-file scope, material byte identity, unchanged custom editor/Weather/diagnostic C#, unchanged main-light and `SampleSH` call counts, unchanged Ground baked-GI resolver gate, exact `0..1` algebraic parity, and verified `0.20..1.80` maximum-strength multiplier bounds. Unity compilation and same-scene visual validation remain pending.

## GM-SURFACE.5S3 — Sun-orthogonal face separation

Status: **implemented and statically valid, but visually rejected by user validation on 2026-08-11; superseded by GM-SURFACE.5S4**.

### Evidence and problem split

5S2 user validation proves the object-level strength path and the primary Sun-facing GI response are active. At high Strength, the shader can now produce more contrast than desired between already-dark and already-bright regions. The remaining visible failure is that multiple neighboring planes can still share nearly the same low-light value.

This follows directly from the current one-dimensional primary input. A response that depends only on `dot(normal, mainLight.direction)` cannot distinguish two faces with equal projection onto the main-light axis. A nonlinear remap can enlarge a small difference but cannot split an exact degeneracy, and further Strength increases only enlarge the existing extremes.

### Two-axis indirect readability model

5S3 retains the existing wrapped Sun-facing term `p` and adds one secondary coordinate derived from the stable mesh geometric face normal. Let:

```text
L = normalized main-light direction
V = -GetViewForwardDir()
axisRaw = V - L * dot(V, L)
axis = normalize(axisRaw) when safely nonzero, otherwise 0
azimuth = clamp(dot(geometricFaceNormal, axis), -1, 1)
headroom = 1 - abs(p)
S = clamp(Low-Light Form Strength, 0, 2)
F = clamp(Low-Light Face Separation, 0, 1)
primary = S * p
separation = 0.50 * F * headroom * azimuth
combined = primary + separation
GI multiplier = 1 + 0.40 * sourceGate * combined
```

`V` is a constant view orientation axis rather than a per-pixel position-to-camera vector, so the secondary cue does not create a perspective gradient across one flat face. When the view axis becomes nearly parallel to the Sun axis, the orthogonal projection is degenerate and the secondary term resolves to zero rather than inventing an arbitrary fallback direction.

The primary term continues to use the resolved fragment normal exactly as 5S2 did. The secondary term uses the stored mesh geometric normal so whole-rock procedural normal perturbation cannot turn the face-separation cue into intra-face texture. Final mesh emission already writes the same resolved face normal to all three vertices of one emitted triangle, so no new face ID, mesh stream, or geometry-generation change is required.

### Bounded-response proof

For `S in [0,2]`, `F in [0,1]`, `p in [-1,1]`, and `azimuth in [-1,1]`:

```text
|separation| <= 0.50 * (1 - |p|)
```

At maximum positive primary response:

```text
combined <= 2p + 0.50(1-p) = 0.50 + 1.50p <= 2
```

and symmetrically the minimum is at least `-2`. Therefore with `sourceGate in [0,1]` the final multiplier remains within the already-approved 5S2 envelope `0.20..1.80`.

This is the central 5S3 contract: the new control creates more distinct face values **inside the existing maximum contrast envelope** instead of increasing that envelope.

### Authoring and serialization

Generated Mass owns both low-light authoring values at object level. Existing **Low-Light Form Strength** remains `0..2`. New **Low-Light Face Separation** is `0..1`, default `0`, and is published through the existing renderer property block to one new hidden shader scalar. Materials remain fallback storage only and are not the authoring authority for the new control.

No custom Inspector restructuring is part of 5S3. The current custom Inspector's generic non-excluded-property fallback is sufficient to expose the serialized control temporarily. A comprehensive Generated Mass Inspector redesign is separate future work and should reorganize the full surface rather than incrementally patching one lighting subsection.

### Lighting invariants

- Real direct lighting remains governed by URP main-light/additional-light equations and Weather shadow/cookie state.
- The camera-relative axis affects only the low-light baked-GI readability helper; it is not fed into direct-light `NdotL`.
- Semantic masks, world-up, height, crevice/base, dirt, feature type, bevel identity, and random per-face values remain prohibited as illumination authorities.
- Face Separation `0` is exact 5S2 parity.
- Both low-light controls at `0` recover the 5R baked-GI baseline.
- Ground remains outside the Generated Mass low-light helper.
- No new light/shadow/cookie lookup, SH sample, texture sample, pass, draw, buffer, varying, mesh channel, CPU-per-frame update, allocation, or generated storage is introduced.

### Camera tradeoff

The secondary cue intentionally depends on view rotation. Rotating the camera can change which otherwise-Sun-degenerate face receives the positive or negative tie-break. Camera translation does not change the axis. This is an accepted stylized readability tradeoff for 5S3; if later camera behavior makes the ordering objectionable, Face Separation `0` is the exact rollback and the secondary-axis design can be reconsidered independently.

### Validation state

Source implementation is complete. The offline static/cross-subsystem audit passes `40 / 40`: exact six-file scope, object-level property publication, hidden shader property/default, all four pass-local CBUFFER insertions relative to their preexisting layouts, stable geometric-normal transport, unchanged main-light and SH-sample counts, unchanged direct-only diagnostic helper, numerical 5S2 parity at Face Separation `0`, the `0.20..1.80` global multiplier bound, unchanged material bytes, and unchanged mesh-output/custom-Inspector source were verified. Unity 6000.5.0f1 compilation, same-scene comparison at multiple Strength/Face Separation combinations, camera-rotation stability, and GPU profiling remain runtime validation gates.

## GM-SURFACE.5S4 — generation-time adjacency-aware face-tone palette

Status: **implemented in source; offline static/cross-subsystem audit passed `58 / 58`; Unity compile/visual validation pending**.

### Why 5S3 is superseded

User comparison at Face Separation `0` versus `1` shows that the 5S3 camera-axis term produces almost no useful distinction on already-bright faces. On darker visible faces it behaves mostly as a small common brightness increase rather than a reliable face-to-face separator. The object-level property publication is proven active; the failure belongs to the runtime directional tie-breaker itself.

The remaining artistic goal is explicitly stylized: increase the frequency with which neighboring low-poly faces occupy distinct low-light values without increasing the already-sufficient maximum bright-versus-dark envelope. A single light-direction projection or secondary view-direction projection cannot guarantee that local neighboring faces receive different scalar values. 5S4 therefore moves face identity to deterministic mesh generation where actual topology and adjacency are available.

### Logical-face compilation

After final triangle emission and before material-mask inheritance, 5S4 reconstructs logical planar face groups from the final mesh. Two triangles belong to the same logical group when they share a quantized geometric edge and their final flat render normals are near-equal. This merges triangulation of one planar polygon while keeping actual changes in face orientation separate. It does not modify positions, indices, normals, tangents, provenance, structural-feature data, or existing material masks.

The compiler then builds the logical-face adjacency graph from shared boundary edges. Each group records total geometric triangle area for later zero-mean centering and whether its existing structural contribution marks it as a `ConvexBoundary` transition. All graph data is temporary generation-time state.

Convex transition/bevel groups are not allowed to become independent stylized tone authorities because doing so could recreate the former parent–bevel–parent ordering failure. For palette assignment, non-convex faces connected through one convex transition are treated as effective neighbours. After non-convex palette assignment, each convex transition derives its raw tone from the average of adjacent non-convex parent/neighbor tones; with two parents this guarantees an intermediate tone. Recentring and uniform scaling preserve that envelope relation.

### Palette assignment

The fixed base palette is:

```text
-1.0, -0.5, 0.0, +0.5, +1.0
```

Non-convex groups are processed deterministically in a high-connectivity-first order. Their effective-neighbour graph contains both direct non-convex adjacency and non-convex neighbors bridged through a convex transition. For every non-convex group, candidate palette values are scored by the minimum absolute tonal distance to already assigned effective neighbors. The candidate maximizing that minimum distance is chosen; deterministic Surface Seed hashing breaks equal-score ties. Convex transitions are resolved afterward from neighboring non-convex tones. This makes broad-face adjacency distinction the optimization target rather than random per-face variation while preserving bevel tonal bridging.

After assignment, the area-weighted mean tone of the complete rock is subtracted from every group. If recentering produces any absolute value above `1`, all tones are uniformly scaled down until the largest magnitude is `1`. Subtracting one shared mean and applying one shared positive scale preserve every nonzero pairwise difference while making the final field approximately zero-mean and bounded.

This is not a promise that every face in the entire rock has a globally unique grayscale identity. The contract is local readability: touching logical faces should preferentially differ and the generator should not systematically brighten or darken the rock as a whole.

### Packed channel contract

For Generated Mass, the existing secondary UV vector now has this active contract:

```text
UV2.x = reserved concave-crease strength
UV2.y = dirt-deposit / mineral-stain mask
UV2.z = generated convex edge-wear strength
UV2.w = signed generation-time logical-face tone in [-1,+1]
```

`UV2.w = 0` remains a neutral backward-compatible value for pre-5S4/generated meshes. The former reservation of this component for future concave-crease localization is retired. Future concave work must use the remaining approved contracts or request a new packing decision; it must not overwrite face tone silently.
The production-generation contract version is incremented in the same patch so ordinary Generated Mass synchronization treats pre-5S4 generated meshes as stale and rebuilds them with the compiled face-tone field.

Ground owns a separate UV2 semantic contract and may continue using its own `UV2.w` as standing-water potential. The shared shader must read signed face tone only inside the Generated Mass low-light branch.

### Runtime response

The existing object-level Low-Light Face Separation scalar `F` remains in `0..1`. Let `T` be the signed generated face tone from `UV2.w`, `sourceGate` the existing main-light source-colour gate, and `primaryScale` the existing 5S2 Sun-facing indirect-GI multiplier. The replacement response is:

```text
T = clamp(UV2.w, -1, +1)
F = saturate(Low-Light Face Separation)
faceToneScale = 0.16 * sourceGate * F * T
finalScale = clamp(primaryScale + faceToneScale, 0.20, 1.80)
shapedBakedGI = bakedGI * finalScale
```

At `F = 0`, `faceToneScale = 0` exactly and the shader is algebraically 5S2. At `F = 1`, the generated layer can move a face by at most `±0.16` indirect-GI multiplier, while the final clamp prevents the layer from expanding the already-approved 5S2 `0.20..1.80` global envelope. It therefore increases the number of distinguishable faces rather than extending maximum contrast.

The generated tone is applied only to baked/indirect GI. Strong direct or nearby real light continues to dominate naturally through URP PBR. No local darkness query is added.

### Performance and memory

Generation-time complexity is linear-to-near-linear in final triangle/edge/group counts using temporary edge maps, group adjacency sets, and sorting over the small logical-face set. This work runs only when the mass mesh is generated or regenerated and introduces no per-frame CPU work. Temporary graph allocations are discarded after compilation.

Persistent memory does not increase because the final signed scalar occupies an already-uploaded `UV2.w` component. The shader removes 5S3's view-forward access, Sun-orthogonal vector rejection, reciprocal-square-root normalization, geometric-normal dot, and headroom ALU. Runtime replacement is one interpolated scalar decode plus scalar multiply/add/clamp. Main-light query count, SH sample count, texture sampling, draw/pass count, buffers, and mesh-stream count remain unchanged.

### Invariants

- no material/profile chooses the face tone; generation topology and Surface Seed own it;
- no semantic exposure, crevice/base, dirt, height, world-up, bevel identity, or generated feature mask determines whether a face is bright or dark;
- coplanar triangles in one logical group carry exactly the same tone;
- convex transition/bevel tones remain inside the tonal envelope of their adjacent non-convex parents/neighbors when those neighbors exist;
- final tone is finite and inside `[-1,+1]`;
- the area-weighted mean is approximately zero;
- material-mask inheritance and logical-bevel reconciliation preserve the face-tone component exactly;
- direct lighting, Weather shadows/cookies, mesh normals, geometry, and 5R material semantics remain unchanged;
- Ground retains its own UV2 meaning and does not execute the Generated Mass face-tone branch;
- Face Separation `0` is exact 5S2 rollback.

### Validation gates

Offline static/cross-subsystem validation passes `58 / 58`, covering deterministic synthetic adjacency/transition cases, face-group/channel contracts, signed range, weighted-mean centering, mask-inheritance preservation, exact source scope, unchanged material bytes, unchanged direct-only diagnostic math, Ground isolation, main-light/SH call counts, and syntax/preprocessor balance. Unity remains authoritative for compilation and visual tuning.

## GM-SURFACE.6A — structural material-response baseline

### Purpose and boundary

GM-SURFACE.6A is the first production consumer of the packed structural semantic stream for artistic material response. It activates `ConvexBoundary` and `CornerChipCap` independently while preserving the unfinished whole-rock normal system as a separate concern. No structural normal, geometry, mask-generation, lighting-direction, or Weather behavior is changed by this update.

### Existing semantic inputs

The packed structural vector already carries at most two contribution slots:

```text
(primaryType, primaryStrength, secondaryType, secondaryStrength)
```

6A resolves the strongest matching contribution for each supported semantic type. `ConvexBoundary` and `CornerChipCap` use their normalized packed strengths directly. Ordinary source faces with neither contribution receive zero structural response.

### Convex material response

The existing object-authored edge response strength becomes the master **Convex Surface Response**. Existing edge softness controls the character of the response. The historical UV2 edge-wear albedo lift/tint function remains outside production and its brightness/tint strengths remain zero.

For semantic convex weight `C`, response strength `R`, and softness `S`:

```text
convexResponse = C * saturate(R)
convexBreakupReduction = convexResponse * lerp(0.15, 0.35, saturate(S))
convexSmoothnessBoost = convexResponse * lerp(0.03, 0.08, saturate(S))
```

The breakup term reduces only the existing `pixelVariation * _PixelVariation` contribution. It does not restore vertex-R, broad, or warp tonal authority on generated bevels; all GM-SURFACE.5R bevel restrictions remain intact.

### Corner-chip material response

6A adds one Generated-Mass-owned **Chip Interior Response**, range `0..1`, default `0.60`. For semantic chip weight `K` and authored response `I`:

```text
chipResponse = K * saturate(I)
chipBreakupIncrease = chipResponse * 0.35
chipSmoothnessReduction = chipResponse * 0.10
```

The chip cap remains lit by its existing faceted geometric/resolved normal. The new module alters only material breakup amplitude and smoothness.

### Combined response

The existing pixel-noise sample is evaluated exactly once as before. Only its amplitude is scaled:

```text
structuralPixelVariationScale = clamp(
    1 - convexBreakupReduction + chipBreakupIncrease,
    0.65,
    1.35)
```

PBR smoothness becomes:

```text
smoothness = saturate(
    ResolveProfileSmoothness()
    + convexSmoothnessBoost
    - chipSmoothnessReduction)
```

This gives the two semantic modules intentionally different material character without fixed albedo value authority. Convex transitions tend cleaner/smoother; chip interiors tend rougher/more broken. Direct lighting, indirect-light readability, face-tone palette, and normal construction remain separate layers.

### Authoring and legacy quarantine

`Convex Surface Response` and `Softness` remain object-owned existing controls. `Chip Interior Response` is a new object-owned control with default `0.60`. The comprehensive Generated Mass Inspector redesign remains deferred; 6A performs only the minimum local presentation needed to expose these active controls and removes obsolete Brightness Lift/Tint authoring from the normal active surface without deleting its serialized legacy fields.

The old UV2.z bevel albedo lift/tint helper remains historical/dead production code. Publishing a nonzero convex response strength for 6A must not reactivate that helper. Legacy bevel brightness-lift and tint-strength property-block values remain hard zero.

### Performance and validation contract

6A performs fixed work against the already-interpolated two-slot structural vector. No feature loop is introduced, so cost is independent of total rock bevel/chip count. No additional texture/noise evaluation, varying, mesh stream, light query, SH sample, buffer, pass, draw, dispatch, per-frame C# update, allocation, or persistent generated data is added.

Runtime acceptance originally required independent toggling of convex and chip response, unchanged ordinary source faces, no reappearance of bright/dark inserted bevel bands, unchanged normals, and target-GPU profiling. The source implementation passed its offline static/cross-subsystem audit (`68 / 68`), but Unity visual validation subsequently found no observable `0`→`1` response for either module. The 6A fixed coefficients are therefore visually rejected and superseded by GM-SURFACE.6A.1; the semantic architecture itself remains retained.


## GM-SURFACE.6A.1 — structural response visibility correction

### Supersession of 6A fixed coefficients

Unity visual validation of the original 6A coefficients found no visible difference between master response values `0` and `1` for either convex transitions or chip interiors. The semantic architecture remains valid, but the material response was too weak to function as useful art direction or as a decisive plumbing test. GM-SURFACE.6A.1 supersedes only the 6A fixed response coefficients and authoring surface; semantic production/packing, geometry, normals, lighting, and legacy bevel-response quarantine remain unchanged.

### Object authoring

The existing `Convex Surface Response` and `Chip Interior Response` controls remain the semantic/intensity gates in `0..1`.

Four object-owned profile controls define the full-response material behavior and are published through the existing Generated Mass renderer property block:

```text
Convex Variation Multiplier = 0.10   range 0..2
Convex Smoothness Offset    = +0.20  range -0.40..+0.40
Chip Variation Multiplier   = 2.00   range 0..3
Chip Smoothness Offset      = -0.20  range -0.40..+0.40
```

A variation multiplier of `1` is neutral for tonal breakup. A smoothness offset of `0` is neutral for PBR smoothness. This lets variation and smoothness be tested independently without adding diagnostic shader modes.

The former edge-wear `Softness` value is retained for recipe/diagnostic compatibility but no longer defines the 6A.1 production structural material response.

### Tonal-variation response

The existing base tonal offset is constructed exactly once from the current pixel, vertex, and broad terms after the existing 5R bevel restrictions:

```text
baseTonalOffset = (
    pixelVariation * PixelVariation
  + bevelIndependentVertexVariation * PixelVertexVariation
  + bevelIndependentBroadVariation * PixelBroadVariation
) * pixelProfileContrast
```

For resolved structural responses `C` and `K` and authored full-response multipliers `Cv` and `Kv`:

```text
variationScale = clamp(
    1
  + C * (clamp(Cv, 0, 2) - 1)
  + K * (clamp(Kv, 0, 3) - 1),
    0,
    3)

tonalOffset = baseTonalOffset * variationScale
```

Default full convex response therefore retains only `0.10x` of the tonal breakup that remains after 5R restrictions. Default full chip response applies `2.00x` of the complete existing tonal breakup. Ordinary faces receive `variationScale = 1` because both semantic responses are zero.

This does not restore suppressed vertex/broad tonal authority to convex bevels. Those terms are still zeroed before the structural multiplier; 6A.1 only scales the tonal offset that legitimately remains for that fragment.

### Smoothness response

PBR smoothness uses authored signed offsets directly:

```text
smoothness = saturate(
    ResolveProfileSmoothness()
  + C * clamp(ConvexSmoothnessOffset, -0.40, +0.40)
  + K * clamp(ChipSmoothnessOffset, -0.40, +0.40))
```

The default convex offset is `+0.20`; the default chip offset is `-0.20`. The master semantic response still gates each offset, so setting the corresponding master to `0` is exact structural-response rollback.

### Invariants

- no geometry, topology, mesh normal, whole-rock normal, or structural-normal change;
- no new semantic generation or mesh stream;
- no fixed albedo lift/darkening, tint, emission, rim-light, or fake illumination;
- the historical UV2.z bevel brightness/tint helper remains production-dead and its object-published brightness/tint strengths remain zero;
- ordinary source faces with no structural semantic remain algebraically unchanged;
- variation and smoothness can be independently neutralized with multiplier `1` and offset `0` respectively;
- Ground, Weather, low-light face-tone separation, direct-light equations, and 5Q direct-only diagnostic math remain unchanged;
- runtime remains fixed-cost scalar ALU against the existing interpolated structural vector with no new sample, varying, stream, loop, buffer, pass, draw, dispatch, allocation, or per-frame C# work.

### Failure interpretation

The 6A.1 defaults are intentionally strong enough to be visually diagnostic. If `Convex Surface Response = 1` with `Convex Variation Multiplier = 0.10` and `Convex Smoothness Offset = +0.20`, or `Chip Interior Response = 1` with `Chip Variation Multiplier = 2.00` and `Chip Smoothness Offset = -0.20`, still produces no visible change in Unity, the next investigation must target semantic-stream/shader-path plumbing rather than increasing the response magnitude again.

Source implementation is complete and the offline static/cross-subsystem audit passes `106 / 106`. Unity 6000.5.0f1 compilation, visual module-independence testing, and target-GPU timing remain authoritative pending runtime gates.

## GM-SURFACE.6A.2 — structural semantic transport diagnostics

### Trigger

Unity validation of GM-SURFACE.6A.1 found literally zero visible response across the full convex/chip master, variation-multiplier, and smoothness-offset ranges, including after regeneration. This satisfies the explicit 6A.1 failure boundary: material-response magnitude is no longer the active question. The next step is to prove each transport boundary on the actual live render mesh and active forward shader.

### Diagnostic scope

6A.2 is diagnostic-only. It does not alter structural semantic generation, final mesh packing, geometry, normals, lighting, tonal variation, smoothness response coefficients, low-light face tones, material assets, Ground, or Weather.

The patch extends two existing diagnostic surfaces:

1. **Live Render Mesh Audit** reads the final Unity mesh UV channel 4 and the renderer's current global `MaterialPropertyBlock`.
2. **Surface Debug** gains two Generated-Mass-only values that bypass normal/material/PBR shading and display structural data directly from the forward fragment input.

No new shader property, CBUFFER field, varying, mesh stream, texture, buffer, pass, draw, allocation, or per-frame C# update is introduced.

### Live render-mesh structural audit

The final render mesh is inspected with `Mesh.GetUVs(4, ...)`, matching the production `SetUVs(4, SurfaceFeatures)` / `TEXCOORD4` contract.

The audit reports:

- structural UV4 element count and missing/partial/non-finite/invalid state;
- primary and secondary encoded type histograms for codes `0..6`;
- non-zero `ConvexBoundary` vertex and triangle counts;
- non-zero `CornerChipCap` vertex and triangle counts;
- minimum/maximum non-zero strength for each expected semantic;
- current renderer property-block values for Convex Surface Response, Chip Interior Response, both variation multipliers, and both smoothness offsets.

An all-zero structural channel remains a legal packed vector shape, so it is not rejected merely for being zero. The audit instead surfaces **NO NON-ZERO CONVEX/CHIP SEMANTICS** explicitly because that condition makes every 6A/6A.1 structural response mathematically inert.

### Forward diagnostic views

Two temporary values reuse the existing Generated Mass `_MaskDebugMode` path:

- `29 — Structural Semantics`
- `30 — Structural Resolved Response`

Values `27` and `28` are already occupied by shared Ground debug identities, so 6A.2 does not reuse them.

`Structural Semantics` resolves the raw packed structural vector only:

```text
convexRaw = max(primary-is-1 * primaryStrength,
                secondary-is-1 * secondaryStrength)
chipRaw   = max(primary-is-3 * primaryStrength,
                secondary-is-3 * secondaryStrength)
```

`Structural Resolved Response` uses the same semantic strengths after the existing master gates:

```text
convexResolved = convexRaw * ConvexSurfaceResponse
chipResolved   = chipRaw   * ChipInteriorResponse
```

Diagnostic colour encoding is:

```text
ConvexBoundary = yellow  (1.00, 0.85, 0.05)
CornerChipCap  = cyan    (0.05, 1.00, 1.00)
neither        = black
```

Overlap saturates additively. The raw mode ignores all six 6A.1 response controls. The resolved mode depends only on the two master response controls and deliberately ignores variation multipliers and smoothness offsets.

The forward fragment returns the diagnostic colour before whole-rock normal construction, material tonal response, PBR, and fog. When neither structural diagnostic mode is active, the helper returns a negative sentinel and the existing 6A.1 production path continues unchanged.

### Decision tree

1. Live mesh audit reports zero convex and zero chip semantics → investigate generator/provenance/final packing.
2. Live mesh audit reports non-zero semantics but Structural Semantics renders black → investigate TEXCOORD4 attribute/varying transport or active shader variant.
3. Structural Semantics renders expected colours but Structural Resolved Response stays black with masters at `1` → investigate renderer property-block/master-uniform transport.
4. Both diagnostic views work and resolved colours track the master controls → semantic/master plumbing is proven; investigate later material-response application only.

### Status

Source implementation is present and the offline exact-scope/static/cross-subsystem audit passes `97 / 97`. Unity 6000.5.0f1 compilation and live diagnostic validation remain pending authoritative gates because Unity is unavailable in this environment.

## GM-SURFACE.6A.3 — structural material response semantic-membership correction

### Trigger

The 6A.2 live render-mesh audit proved that the user's audited test mass contains substantial `ConvexBoundary` coverage (`342` vertices / `114` triangles), while its packed convex strengths are only approximately `0.099..0.130`. The renderer property block simultaneously contains non-zero structural authoring values. Therefore the CPU-side producer, final mesh channel, and property publication are present for convex response, but the 6A/6A.1 resolver was multiplying the authored master response by an unrelated small packed feature strength.

The same audit reports zero `CornerChipCap` vertices/triangles on that specific mesh. That mesh therefore cannot validate chip material response regardless of chip response settings.

### Corrected material-response contract

`ConvexBoundary` and `CornerChipCap` are classification inputs for the 6A material module. Their packed normalized strengths remain transport data, but they are no longer a second hidden artistic intensity for variation/smoothness response.

For each semantic type:

```text
membership(type) = 1 when either packed slot matches type
                   and its packed strength is > 0.0001
                 = 0 otherwise

convexResponse = GeneratedMassSurface *
                 membership(ConvexBoundary) *
                 ConvexSurfaceResponse

chipResponse = GeneratedMassSurface *
               membership(CornerChipCap) *
               ChipInteriorResponse
```

The existing 6A.1 variation multipliers and smoothness offsets remain unchanged. The master controls now own the full artistic response magnitude once the corresponding semantic is present.

### Diagnostic contract

The raw **Structural Semantics** debug mode remains strength-based. It continues to visualize the actual packed semantic strength and therefore remains useful for transport/provenance diagnosis.

The **Structural Resolved Response** debug mode uses the corrected production resolver. A present semantic at master response `1` therefore resolves to full diagnostic intensity rather than being attenuated by packed strength.

The live Render Mesh Audit additionally reports whether a non-zero convex/chip master response cannot affect the audited mesh because that mesh contains zero vertices of the corresponding semantic type. This is a warning condition, not a geometry-format failure.

### Invariants

- Packed semantic values, type codes, strengths, generation, priority resolution, final packing, UV4 upload, and shader transport are unchanged.
- Geometry and all normal systems are unchanged.
- Structural profile defaults/ranges are unchanged.
- Materials, Ground, Weather, low-light face-tone response, and the historical bevel albedo-lift/tint quarantine are unchanged.
- Runtime cost remains fixed scalar ALU; no new sampling, buffers, streams, loops, passes, draws, allocations, or per-frame CPU work are introduced.

### Status

Source implementation is present. Offline static/cross-subsystem validation is recorded in the canonical implementation checklist. Unity 6000.5.0f1 compilation and live visual validation remain authoritative pending gates.


## GM-SURFACE.6A.4 — absolute structural variation strength

### Trigger

Post-6A.3 Unity validation proves the ConvexBoundary membership/material path is active, but a `Convex Surface Response` comparison using variation value `1.0` and smoothness offset `+0.10` remains barely visible. Under the 6A.1 multiplier contract, variation `1.0` is algebraically neutral. More importantly, 5R intentionally removes vertex and broad tonal breakup on convex generated transitions before the structural multiplier is applied, so the multiplier model depends on whatever small tonal residue survives earlier restrictions rather than expressing a stable structural material property.

### Authoring contract

6A.4 supersedes the multiplier interpretation with directly authored **absolute zero-mean variation strength** while preserving the existing numeric serialized values, ranges, and default initializers through field migration:

```text
Convex Variation Strength = range 0..2
Chip Variation Strength   = range 0..3
```

One authored strength unit equals `0.10` signed tonal amplitude before the existing global Pixel Effect Strength. Therefore the maximum authored convex range contributes approximately `±0.20`; the maximum chip range contributes approximately `±0.30`. Zero disables that module's structural tonal term exactly.

The master responses remain unchanged:

```text
convexResponse = GeneratedMassSurface * membership(ConvexBoundary) * ConvexSurfaceResponse
chipResponse   = GeneratedMassSurface * membership(CornerChipCap)  * ChipInteriorResponse
```

Smoothness offsets and their ranges/equation remain unchanged.

### Runtime contract

The accepted 5R/6A.3 base tonal construction remains unchanged. 6A.4 adds an independent structural term from the already-computed `pixelVariation`:

```text
convexAmplitude = convexResponse * clamp(ConvexVariationStrength, 0, 2) * 0.10
chipAmplitude   = chipResponse   * clamp(ChipVariationStrength,   0, 3) * 0.10
structuralAmplitude = max(convexAmplitude, chipAmplitude)

structuralTonalOffset = pixelVariation * structuralAmplitude
finalTonalOffset      = baseTonalOffset + structuralTonalOffset
```

Using `max` prevents primary/secondary semantic overlap near boundaries from stacking the two material amplitudes beyond the larger authored response. The structural term is intentionally not multiplied by the pre-existing pixel/vertex/broad variation amplitudes or by the bevel-independent broad/vertex suppression. It still passes through the existing final Pixel Effect Strength exactly once.

The term is zero-mean because it uses the signed pixel-cell variation already evaluated for the ordinary material. It adds no constant albedo lift, darkening, tint, emission, or lighting-direction cue.

### Serialization and shader contract

The serialized fields migrate from the former multiplier names with `FormerlySerializedAs`, retaining their stored numbers rather than silently resetting authored objects. Hidden shader/property-block names move from `...VariationMultiplier` to `...VariationStrength`; no material asset becomes authoring authority because Generated Mass republishes these hidden values through its renderer property block.

The live Render Mesh Audit reports the new strength property-block values. Raw/resolved structural semantic diagnostic modes are otherwise unchanged.

### Performance and invariants

- Reuses the existing `pixelVariation` value; no additional noise/hash/texture sample is evaluated.
- No new varying, vertex stream, buffer, loop, pass, draw, allocation, light query, or per-frame CPU work.
- Geometry, semantic generation/packing, whole-rock normals, structural normals, direct lighting, low-light face-tone response, Ground, Weather, and material assets remain unchanged.
- Historical UV2 bevel albedo lift/tint response remains production-dead and its brightness/tint strengths remain hard-zero.

### Status

Source implementation is present. Offline exact-scope/static/cross-subsystem validation is recorded in the canonical implementation checklist. Unity 6000.5.0f1 compilation and the one-comparison visual validation remain authoritative pending gates.
