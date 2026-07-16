# Generated Mass Framework

This document defines the stable Generated Mass feature contract. It is not a progress log.

The sole canonical progress ledger is:

```text
Docs/Generated_Mass_Feature_Implementation_Checklist.md
```

## Feature goal

Generated Mass produces deterministic convex stylized rock and mass geometry suitable for the isometric URP project. Edge wear must create real faceted bevel geometry, preserve the closed mass surface, remain inexpensive at runtime, and carry explicit feature data to the final mesh.

## Geometry budgets

Accepted final-mesh budgets remain:

| Tier | Vertex budget | Atlas guidance |
|---|---:|---|
| Standard | no more than 1,600 vertices | 256 when an atlas is actually required |
| High | no more than 3,000 vertices | 256; 512 only for unusually large assets |
| Hero | no more than 8,000 vertices | 512 when justified |

Quality tiers must not change the apparent edge-wear band width solely because of texture resolution.

## Canonical generation order

```text
base convex mass
    -> deterministic surface deformation
    -> polygon-face representation
    -> source topology graph
    -> edge-wear candidate selection
    -> width and corner feasibility solve
    -> bevel construction
    -> topology and geometry validation
    -> triangulation
    -> normals and feature data
    -> final MeshData
```

Edge wear may not mutate the source polygon set unless the selected construction has passed its explicit production gate.

## Edge-wear control ownership

- **Amount** controls the strength or prominence of the edge-wear response, not topology eligibility.
- **Coverage** controls deterministic edge selection density.
- **Width** controls geometric bevel width.
- **Softness** must not secretly expand geometric width; it belongs to shading/falloff response unless a separate approved geometry meaning is introduced.
- **Macro variation** means differences between edges.
- **Micro variation** means variation along one edge.

Controls must have visible, testable responsibilities and meaningful tooltips. Stale or disconnected controls must be removed rather than retained as placeholders.

## Current construction boundary

The active production candidate is the convex plane-cut kernel described in:

```text
Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md
```

It currently runs on a deep clone. The rendered geometry remains unchanged while:

```text
geometryCommit=disabled
```

is active.

The retained legacy replacement/strip/patch path is diagnostic comparison evidence and must not be mistaken for the approved future production architecture.

## Topology invariants

A production mass must have:

- zero open edges;
- zero non-manifold edges;
- zero T-junctions;
- finite vertices, unit normals, and finite unit tangents;
- non-degenerate retained faces;
- consistent outward winding;
- positive enclosed volume;
- deterministic output for identical inputs.

A bevel plane must either produce one surviving `ConvexEdgeWear` face or be proven redundant because earlier cuts already removed its source edge and satisfy its half-space.

## Feature-data contract

Bevel faces use:

```text
PolygonFaceFeature.ConvexEdgeWear
```

and preserve the selected edge’s feature strength through triangulation. Final material response may use brightness lift, optional tint, falloff contrast, and smoothness offset, but shader response must not compensate for invalid geometry.

## Atlas policy

Edge-wear atlases are optional diagnostic or feature-specific inputs. Final bevel visibility must not depend on the previously rejected low-resolution boundary atlas path.

Atlas generation is justified only when a retained material feature needs it. Geometry construction, boundary ownership, and bevel width must remain mesh-defined.

## Performance policy

- Prefer deterministic dirty-time construction over per-frame work.
- Generated rocks are static after generation unless explicitly regenerated.
- Lifecycle restoration first attempts to re-adopt a certified production mesh whose stored production state and generation-contract version match the current recipe.
- A missing, stale, preview, or uncertified mesh may rebuild once; later lifecycle callbacks must reuse the accepted result instead of repeating the rebuild.
- Ordinary `OnEnable` and `OnValidate` synchronize generated state rather than unconditionally regenerating.
- Material-only changes update renderer state without rebuilding geometry, recooking the collider, recalculating the world-triangle fingerprint, or notifying geometry consumers.
- River-interaction authoring changes notify consumers without rebuilding production geometry.
- Feature-atlas diagnostics are tracked separately from production geometry and may refresh atlas data without recooking an unchanged collider.
- Lifecycle restoration may rebuild a transient production mesh, but ordinary generation must not run diagnostic-grade edge-wear reconstruction or audits.
- Expensive validation is explicit editor/diagnostic-only and never runs from `OnEnable`, ordinary `OnValidate`, script reload, or Play Mode transitions.
- Collider recooking occurs only when geometry was rebuilt or the collider lost its certified mesh binding.
- Exact world-triangle fingerprints are invalidated by geometry changes and calculated lazily on the first consumer request.
- Any normal-generation semantic change must increment the production-generation contract version before old generated state may be reused. GM-R12B.1D advances this contract from `1` to `2`.
- Production geometry must respect the accepted tier budgets.
- Do not add per-frame full-mesh rebuilds.
- Cache reusable deterministic data when it materially reduces regeneration cost.

## Editor and diagnostics contract

- Ordinary production generation emits no edge-wear audit.
- Automatic lifecycle synchronization emits no per-object Console summary; performance evidence uses Profiler markers.
- The authoritative markers are `GeneratedMass.Synchronize`, `GeneratedMass.GenerateProduction`, `GeneratedMass.BindCollider`, `GeneratedMass.ComputeFingerprint`, and `GeneratedMass.NotifyConsumers`.
- Explicit plane-cut preview emits one dedicated compact plane-cut result per intentionally evaluated mass.
- The legacy replacement/strip/patch compact audit is explicit, single-object, opt-in evidence.
- Detailed evidence is opt-in, deduplicated, and capped to representative failures.
- Diagnostics must never alter production eligibility unless explicitly promoted.
- Editor-only previews must be clearly labeled and must not become serialized artistic controls accidentally.
- Existing layers, tags, components, asset names, and serialized structures may not change without approval.
- Live render-channel integrity audits are explicit, single-object, editor-only actions over the already-generated `MeshFilter.sharedMesh`; they must never run from generation, `OnEnable`, `OnValidate`, Play Mode transitions, or per frame.
- Diagnostic proof meshes and materials must use `HideAndDontSave`, must not serialize or replace the production mesh, and must restore the source renderer when removed or when selection changes.
- A render-channel repair may not be promoted from a proof clone until the exact failing triangle/channel is measured and the smallest-blast-radius ownership boundary is justified. GM-R12B.1D proved Generated Mass zero normals and promotes a Generated Mass-only normalization/validation repair; shared `MeshData`, `MeshBuilder`, UV construction, and shader behavior remain unchanged.

## Validation contract

Every geometry implementation patch requires:

1. Zero Unity compiler errors and warnings introduced by the patch.
2. Deterministic regeneration of the representative mass set.
3. Exact topology and geometry audit results.
4. Confirmation that unrelated compact fields remain unchanged.
5. Confirmation of the live/clone boundary.
6. Visual inspection before any production promotion.
7. Vertex-budget inspection after production promotion.

## Edge-wear recovery closure contract

- Historical editor fixtures may resolve as either a certified materialized bevel or a finite infeasibility result under the solver's complete current discrete admissible-width schedule. A discrete result must not be described as a continuous mathematical proof.
- No recovery diagnostic may add geometry attempts unless a separately approved method change declares the performance impact.
- Finalized corner recovery may use only exact recorded zeroing participants and their last positive widths. Width-recovery-provisional edges are not valid corner-augmentation initiators.
- Negative artistic fixtures are first-class regression gates. Seed `8889`, maximum width, source edge `40` is intentionally excluded and must remain inactive, uncertified, and unmaterialized. This fixture is editor-only and may not create a seed-specific production branch.
- Optional recovery remains subordinate to the immutable certified baseline. It may not reduce certified count, replace unrelated baseline identity, weaken topology/geometry/render guards, or erase a valid preview.

## Documentation ownership

| Document | Sole responsibility |
|---|---|
| `Generated_Mass_Feature_Implementation_Checklist.md` | Canonical progress log, methods tried, validation outcomes, active blocker, and next step. |
| `Generated_Mass_Edge_Wear_Recovery_Architecture.md` | Current architecture, invariants, rationale, and promotion gates. |
| `Generated_Mass_Edge_Wear_Code_Inventory.md` | Current files, methods, and dependency boundaries. |
| `Generated_Mass_Framework.md` | Stable feature, control, performance, and validation contract. |

Other documents may reference the canonical progress ledger, but must not maintain competing or complementary patch histories.


## Generated Mass render-normal integrity contract

- Every authored or geometric render normal is normalized explicitly from any finite, mathematically non-zero vector using double-precision magnitude evaluation. Triangle acceptance remains governed by the existing scale-relative geometry tests; no edge-length-squared threshold may be reused for a cross-product-squared normal test.
- Accepted triangles may not silently fall back to an unrelated axis normal. Only a truly zero or non-finite geometric normal is a deterministic generation failure; tiny but scale-valid triangles must normalize successfully.
- Generated Mass validates all authored `MeshData` channels before mesh application and validates final Unity normals/tangents after tangent reconstruction.
- Final normals and tangent XYZ vectors must be finite and unit length; tangent handedness must be finite and approximately `-1` or `+1`.
- These guards run only when Generated Mass geometry is built or explicitly regenerated. They add no per-frame work and do not alter shared procedural-mesh consumers.


## Scale-correct render-normal clarification

- Triangle validity is established by the generator's existing finite and scale-relative geometry tests. Render-normal normalization may not introduce an unrelated absolute triangle-size cutoff.
- A cross product may be normalized when it is finite and mathematically non-zero, even when its magnitude is below Unity's `Vector3.normalized` epsilon or below an edge-length threshold.
- Production and explicit editor diagnostics must use equivalent robust normalization semantics so an accepted production triangle and its audit cannot disagree solely because of normalization thresholds.
- UV-conditioning metrics are diagnostic warnings when all final channels and 3D geometry remain finite and valid; they are not independently a production failure.

## Edge-wear boundary-terminal and provisional-retention contract

- An isolated support rail terminates at the unique nearest forward intersection with the complete boundary of its exact owner source face. The solver may resolve a different boundary segment than the endpoint-adjacent assumption only through exact owner-plane polygon intersection and exact target-face provenance.
- A rail may not be clamped across a material segment miss, routed through an invented support chain, or allowed to bypass displacement, topology, containment, bounds, volume, face-quality, or render-channel certification.
- Requested-width fraction remains the normal viability gate. A locally certified edge below that fraction may be provisional only when it remains certified at the canonical minimum style floor.
- Provisional candidates are decided by bounded complete-shell conflict search. Ordinary non-provisional generation retains the direct path and pays no search cost.
- Corner-collapse participants and terminal plane-band victim/foreign pairs are branchable conflicts. Final render-normal/winding failure rejects a branch and never weakens the normal guard.
- Valid conflict states are ordered by certified count, accepted artistic score, certified width, and deterministic source-edge order. No seed or source-edge ID is production policy.

## GM-R12B.1E validation closeout

- The scale-correct normal contract is runtime-proven on `Rock_14`, `Rock_18`, seed `8889` ordinary output, and seed `8889` unified bevel-preview output.
- Representative audits report zero missing, zero, non-finite, or non-unit normals/tangents; the black-triangle and Bloom-orb artifact no longer reproduces.
- Finite UV-conditioning findings remain explicit warnings when 3D geometry and final render channels are valid.
- GM-R12B.1E is closed by its ordinary and bevel-preview runtime audits. R13A.6 is the latest runtime-validated safety state over the immutable R13A.4 baseline; active recovery-closure validation advances through R13A.7.

## Edge-wear conflict-search execution contract

- A Generated Mass edge-wear evaluation may own only one active full-shell conflict frontier. A provisional corner/full-shell search may not recursively invoke the plane-kernel coexistence search.
- Ordinary non-provisional cases retain the existing plane-kernel coexistence owner. Provisional cases disable kernel recursion and return exact failure evidence to their single active frontier.
- Explicit editor validation searches are capped at 128 states and five seconds. State-budget exhaustion, time-budget exhaustion, and user cancellation are distinct reported outcomes; none may commit partial geometry.
- A synchronous matrix case must poll the cancelable editor progress callback between search states and clear the transient callback after the case.
- Priority ordering is fewest exclusions, lowest removed accepted artistic score, lowest removed certified width, then deterministic source-edge order. The first fully certified state is committed; exhaustive post-success optimization is forbidden.

R13A.6 is the latest runtime-validated safety state over the immutable R13A.4 baseline; active recovery-closure validation advances through R13A.7.


## Certified baseline augmentation contract

- Optional edge-wear recovery is an augmentation of a fully certified ordinary shell, never a replacement generator. The baseline corner solution, plane audit, preview geometry, and lifecycle evidence remain immutable fallback state.
- Selected width-provisional edges are omitted from baseline certification. Corner-inactive recovery participants may be discovered from that baseline's explicit corner-conflict evidence.
- Augmentation begins from baseline exclusions with only recovery participants re-enabled. It may commit only a fully certified state that recovers an absent participant and is superior by certified count, accepted artistic score, then certified width.
- Timeout, state exhaustion, cancellation, or no superior augmented state must retain the certified baseline and publish the exact augmentation outcome. These outcomes may not erase a live preview, produce an empty matrix record, or be reported as collateral loss.
- Baseline evaluation and augmentation may never own nested full-shell search frontiers. Augmentation trials disable kernel coexistence recursion and remain bounded by editor-validation state, time, and cancellation safeguards.

## Retained-point multi-support recovery contract

- R13A.4 is the stable incomplete geometry baseline. R13A.5 sampled split-plane geometry is rejected and is not baseline behavior.
- Multi-support recovery uses the convex hull of all original source vertices except the two selected-edge endpoints plus the four exact solved rail points.
- Every emitted new facet must be a global supporting plane of that retained point set. The complete new facet set must form one connected bevel band and preserve all four rails.
- Both owner faces and only source faces in the selected endpoints' vertex stars may change. Every source face must retain one unique provenance record.
- Complete topology, strict intersection, containment, convexity, bounds, volume, face-quality, triangulation, and render-channel validation remain mandatory.

## Finalized corner-recovery and augmentation contract

- A corner recovery provisional is captured only from the exact conflict event that actually transitioned that edge to zero at the finalized `corner-width-inactive` state. It records the last positive width, collapsed shared edge, zeroing stage, uniform scale, the complete participant set, and the exact zeroed-edge set.
- Generic width-provisional edges do not authorize augmentation. Only certified retained-hull recovery and finalized corner-inactive recovery are valid trigger classes.
- Recovery targets are protected. A candidate may not reduce certified count or remove a baseline-certified edge outside the exact participant set of a recovered corner target.
- Timeout, cancellation, exhaustion, or no acceptable recovery retains the R13A.4 baseline and its edge identity.

The current implementation gate is the R13A.7 one-click suite. R13A.6 remains the latest runtime-validated safety state until R13A.7 passes Unity compilation and the complete suite.
