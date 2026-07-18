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

## Edge-wear micro-topology normalization contract

- Explicit edge-wear preview and audit transactions may consume a connected micro-topology component when it contains at least one internal manifold seed edge no longer than the canonical minimum useful style scale and every expanded component edge plus the complete component diameter remains below the global minimum certified bevel footprint.
- Normalization is forbidden in production `EdgeWearEvaluationMode.None`; it never rewrites the authored/generated base mass mesh or any serialized asset.
- Every candidate collapse maps the component to one existing component vertex. Invented midpoint vertices and hull expansion are forbidden. Candidate choice is deterministic by minimum squared displacement, then minimum volume loss, then lowest original graph-vertex index.
- The temporary convex hull must be closed, manifold, finite, positive-volume, contained by the original bounds, lose no more than the bounded micro-volume allowance, and preserve every non-component source edge after endpoint remapping. Any failure leaves the original topology unchanged.
- Stable original source-edge identity remains authoritative for reports, fixtures, and overlays. Consumed edges are retained as diagnostic records with `micro-topology-suppressed` and overlay code `M`; any new transition edge created by the normalized hull is structural-ineligible, receives only a synthetic non-colliding diagnostic ID, and cannot become a bevel candidate.
- Every eligible component retains one bounded diagnostic record containing seed/all edge IDs, graph vertices, diameter, every canonical-vertex attempt, displacement, resulting volume/loss, exact blocker, and selected candidate.
- The normalized faces are the source of truth for viability, artistic selection, shared-corner solving, coexistence, and final bevel construction. All existing topology, convexity, source-provenance, bounds, volume, face-quality, triangulation, normal, tangent, and render-channel certification remains mandatory.
- Seed `8889` maximum-width micro component `14/24/30` is the canonical validation fixture. Suppressing that invisible triangular component is authorized only if original edges `13` and `23` certify, edge `39` remains certified, and edge `40` remains inactive and uncertified.

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
- Every width-provisional edge remains forced off in the immutable certified baseline. Only an artistically eligible provisional edge whose source length supports at least two complete requested bevel footprints may become a material width-recovery target. Tiny or barely structural provisional edges remain baseline-only exclusions.
- Material width recovery is decided from one immutable post-selection target set. Each target receives exactly one non-branching complete-shell trial with every other baseline exclusion unchanged; successful targets are committed sequentially so later trials must preserve the certified baseline plus every earlier material recovery. Ordinary non-provisional generation retains the direct path and pays no recovery cost.
- Corner-collapse participants and terminal plane-band victim/foreign pairs are branchable conflicts. Final render-normal/winding failure rejects a branch and never weakens the normal guard.
- Valid conflict states are ordered by certified count, accepted artistic score, certified width, and deterministic source-edge order. No seed or source-edge ID is production policy.

## GM-R12B.1E validation closeout

- The scale-correct normal contract is runtime-proven on `Rock_14`, `Rock_18`, seed `8889` ordinary output, and seed `8889` unified bevel-preview output.
- Representative audits report zero missing, zero, non-finite, or non-unit normals/tangents; the black-triangle and Bloom-orb artifact no longer reproduces.
- Finite UV-conditioning findings remain explicit warnings when 3D geometry and final render channels are valid.
- GM-R12B.1E is closed by its ordinary and bevel-preview runtime audits. R13A.8 remains the accepted recovery baseline: current preview and both `33/33` matrices are safe, micro-topology component `14/24/30` is normalized, seed-8889 edges `13/23/39` are certified, and edge `40` remains excluded. R13A.9 preserved those results but was rejected because it attempted neither seed-2223 edge-13 fixture and allowed an unrelated material-recovery case to exhaust the five-second frontier. R13A.9a is implemented and statically validated; Unity runtime validation is pending.

## Edge-wear conflict-search execution contract

- A Generated Mass edge-wear evaluation may own only one active full-shell conflict frontier. A provisional corner/full-shell search may not recursively invoke the plane-kernel coexistence search.
- Ordinary non-provisional cases retain the existing plane-kernel coexistence owner. Provisional cases disable kernel recursion and return exact failure evidence to their single active frontier.
- Explicit editor validation searches are capped at 128 states and five seconds. State-budget exhaustion, time-budget exhaustion, and user cancellation are distinct reported outcomes; none may commit partial geometry.
- A synchronous matrix case must poll the cancelable editor progress callback between search states and clear the transient callback after the case.
- Priority ordering is fewest exclusions, lowest removed accepted artistic score, lowest removed certified width, then deterministic source-edge order. The first fully certified state is committed; exhaustive post-success optimization is forbidden.

R13A.8 remains the accepted runtime baseline over the immutable certified-baseline architecture. R13A.9 is a rejected intermediate: it preserved safety and canonical diagnostics but missed both intended target executions and triggered an unrelated five-second material-recovery search. R13A.9a separates immutable material-target execution from the bounded non-material frontier; Unity validation is pending.


## Certified baseline augmentation contract

- Optional edge-wear recovery is an augmentation of a fully certified ordinary shell, never a replacement generator. The baseline corner solution, plane audit, preview geometry, and lifecycle evidence remain immutable fallback state.
- Every selected width-provisional edge is omitted from baseline certification. Materially significant targets are captured once from selected graph-edge identity and immutable preflight/artistic evidence before any trial mutates lifecycle state. Finalized corner-inactive and retained-hull targets remain separate non-material classes.
- Material targets execute first in deterministic graph-edge order. Each receives one complete-shell trial with kernel coexistence recursion disabled, no child frontier, and all other baseline exclusions unchanged. A successful trial must build the target and preserve every edge built by the current certified working baseline; it then becomes the working baseline for the next material target.
- Only retained-hull and exact corner-participant targets enter the existing bounded non-material frontier. That frontier may commit only a fully certified superior state; material recoveries are protected from later branch deferral, and only a recovered corner target may authorize loss of its exact recorded conflict participants.
- Every material target reports immutable eligibility, baseline deferral, attempted, completed, certified, and exact failure state. A material target may become `width-recovery-proven-infeasible` only after its one target trial completes without timeout or cancellation. Timeout, state exhaustion, cancellation, or no acceptable augmented state must retain the latest certified baseline and may not erase a live preview, produce an empty matrix record, or be reported as collateral loss.
- Baseline evaluation and recovery may never own nested full-shell search frontiers. Material target trials disable kernel coexistence recursion and create no frontier children. Non-material augmentation retains the existing bounded editor-validation state, time, and cancellation safeguards.

## Retained-point multi-support recovery contract

- R13A.4 is the stable incomplete geometry baseline. R13A.5 sampled split-plane geometry is rejected and is not baseline behavior.
- Multi-support recovery uses the convex hull of all original source vertices except the two selected-edge endpoints plus the four exact solved rail points.
- Every emitted new facet must be a global supporting plane of that retained point set. The complete new facet set must form one connected bevel band and preserve all four rails.
- Both owner faces and only source faces in the selected endpoints' vertex stars may change. Every source face must retain one unique provenance record.
- Complete topology, strict intersection, containment, convexity, bounds, volume, face-quality, triangulation, and render-channel validation remain mandatory.

## Finalized corner-recovery and augmentation contract

- A corner recovery provisional is captured only from the exact conflict event that actually transitioned that edge to zero at the finalized `corner-width-inactive` state. It records the last positive width, collapsed shared edge, zeroing stage, uniform scale, the complete participant set, and the exact zeroed-edge set.
- A width-provisional edge authorizes augmentation only when it is artistically eligible and its source length supports at least two complete requested bevel footprints. This material-significance gate excludes tiny provisional edges without seed or source-edge policy. Certified retained-hull and finalized corner-inactive recovery remain valid trigger classes.
- Recovery targets are protected. A material candidate must increase certified count and preserve every edge built by the current working baseline; successful material targets are committed sequentially and protected from later non-material deferral. Corner recovery may remove only the exact participant set of a recovered corner target.
- Timeout, cancellation, exhaustion, or no acceptable recovery retains the R13A.4 baseline and its edge identity.

The accepted runtime gate remains the R13A.8 one-click suite. R13A.9 retained current preview, both `33/33` matrices, seed-8889 edges `13/23/39`, micro component `14/24/30`, and the edge-40 negative fixture, but it remained `3/5`, attempted neither seed-2223 edge-13 fixture, and raised worst-case editor time through an unrelated five-second material frontier. R13A.9a replaces that execution path with immutable one-trial-per-target scheduling; static validation is complete and Unity validation remains mandatory.
