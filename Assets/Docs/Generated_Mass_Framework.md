# Generated Mass Framework

This document defines the stable Generated Mass feature contract. It is not a progress log.

The sole canonical progress ledger is:

```text
Docs/Generated_Mass_Feature_Implementation_Checklist.md
```

## Feature goal

Generated Mass produces deterministic convex stylized rock and mass geometry suitable for the isometric URP project. Edge wear must create real faceted bevel geometry, preserve the closed mass surface, remain inexpensive at runtime, vary with deliberate natural irregularity rather than uniform machine-like strips, support later artistic normal shaping, and carry explicit feature data to the final mesh and material response.

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
    -> deterministic edge-wide wear identity [EW-V1 planned]
    -> requested width and corner feasibility solve
    -> along-edge profile planning [constant in R13A.9a; EW-V2 planned]
    -> localized chip/break planning [EW-V3 planned]
    -> certified bevel construction [uniform R13A.9a accepted]
    -> topology and geometry validation
    -> triangulation
    -> technically valid normals and feature data
    -> artistic bevel-normal shaping [EW-V4 planned]
    -> final material/rendering finish [EW-V5 planned]
    -> final MeshData
```

Edge wear may not mutate the source polygon set unless the selected construction has passed its explicit production gate.

## Edge-wear control ownership

- **Amount** controls the strength or prominence of the edge-wear response and gates generated bevel geometry at zero; it does not rank topology eligibility.
- **Width** controls the uniform geometric bevel-depth reference used before per-edge macro variation.
- **Bevel Coverage** controls deterministic edge-selection density.
- **Macro Variation Coverage** controls which fraction of ordinary eligible canonical source edges participates in edge-to-edge width variation.
- **Macro Variation Strength** controls how strongly participating edges narrow from the uniform Width reference.
- **Softness** controls the current marked-bevel shader response and must not secretly expand geometric width.
- **Response Strength**, **Brightness Lift**, **Worn Edge Tint**, and **Tint Influence** control the current UV2.z-marked visual response.
- **Micro variation** means future deterministic geometric variation along one source edge, such as taper, swell, narrowing, or later localized chips. No Micro Variation control is exposed until EW-V2 has a real consumer.
- **Chipping** means a bounded local interruption or notch in an otherwise valid wear band. It is separate from smooth micro variation.
- **Normal shaping** means future artistic control of bevel light response. It must not be confused with the already-solved technical normal/tangent integrity contract.

`EW-V1A.2b` is the accepted and frozen V1A authoring/geometry baseline for explicit editor preview/audit evaluation. Coverage defaults to one; zero Coverage or zero Strength restores exact uniform requests; participating edges receive deterministic downward-only average-width variation while width remains constant along each edge. The dormant serialized Micro Variation backing value remains hidden and has no geometry or shader consumer. V1A.2 completed construction-width backoff, V1A.2a closed stable-floor classification, and V1A.2b closed the editor fixture proof. Ordinary production remains unchanged. Controls must have visible, testable responsibilities and meaningful tooltips; stale or disconnected controls are not exposed.

## Current construction boundary

`EW-V1A.2b` is the accepted deterministic per-edge macro-width irregularity baseline. `EW-B4.2R13A.9a` remains its exact uniform zero-control fallback. Together they close the current topology, viability, coexistence, micro-topology normalization, recovery, canonical identity, macro participation/width identity, and render-channel problems for the representative suite:

```text
current preview: passed
topology matrix: 33/33
artistic-preview matrix: 33/33
recovery fixtures: 5/5
unresolved fixtures: 0
negative exclusion: 1/1
```

The accepted geometry still runs through explicit editor preview/audit modes on a deep clone. Ordinary `MassGenerator.Generate(...)` remains `EdgeWearEvaluationMode.None`, and:

```text
geometryCommit=disabled
```

remains active. V1A acceptance therefore freezes deterministic edge-to-edge average-width irregularity on the editor visual/geometry foundation; it is not production promotion and it is not completion of the full edge-wear visual feature.

The retained legacy replacement/strip/patch path and rejected intermediate plane/junction experiments remain diagnostic history only. New work begins at EW-V2 smooth within-edge irregularity and must preserve both V1A.2b and the R13A.9a zero-irregularity fallback.

## Post-baseline edge-wear visual contract

The next planned sequence is:

```text
EW-V1  macro irregularity between edges
EW-V2  smooth micro irregularity along one edge
EW-V3  localized chips and notches
EW-V4  artistic bevel-normal shaping
EW-V5  final material/rendering finish
```

These are distinct responsibilities and may require multiple small passes each.

- V1A.2b freezes **Macro Variation Coverage** and **Macro Variation Strength**. Coverage selects a deterministic canonical-edge subset; Strength applies the downward-only `0.55x–1.0x` full-strength range to participants while nonparticipants remain exactly `1.0x`. Width remains constant along each edge. Zero on either control restores the R13A.9a uniform request path, and Coverage `1` authorizes every ordinary evaluated edge.
- V2 may introduce along-edge subdivisions and a bounded one-dimensional width profile, but must preserve exact endpoint/corner ownership and tier budgets.
- V3 may add local discontinuities only after the continuous V2 profile is stable; random high-frequency damage is not an acceptable substitute.
- V4 must preserve finite unit normals/tangents and the current render-normal integrity gates while changing artistic light response.
- V5 may enhance valid wear through material response but may not conceal profile, topology, or normal defects.
- Macro Variation Coverage or Strength at zero must reproduce the accepted R13A.9a geometry and source-edge identity. No Micro Variation control is exposed until EW-V2 owns a real implementation.
- Variation must be deterministic from stable provenance and existing recipe seeds. No per-frame work or new serialized seed is justified without a demonstrated authoring need.

### EW-V1A.1 implementation boundary

- Width identity and participation identity are independent deterministic streams from shape seed and canonical original source-edge index.
- Participation uses a stable threshold set: increasing Coverage adds edges without reshuffling earlier participants; Coverage `1` authorizes every ordinary evaluated edge.
- Participating edges receive the V1A width sample blended by Strength. Nonparticipants retain the base request exactly.
- The per-edge request enters before footprint and isolated viability, then remains authoritative through corner solving, recovery, explicit bounded preview, and final certified shell construction.
- Minimum-style clamping and every existing topology/quality/recovery gate remain authoritative.
- Artistic selection weights and seed stream are unchanged; any selection difference must arise from truthful width-dependent viability, not a new ranking policy.
- The one-click suite directly checks zero-by-Strength parity, zero-by-Coverage parity, repeated determinism, full-Coverage compatibility, participant bounds, and active distribution before the existing matrices and fixtures.
- Macro controls are not part of `ProductionGenerationState`; `EdgeWearEvaluationMode.None`, Play Mode generation, collider ownership, and active-gameplay cost are unchanged.
- V1A.1 does not implement within-edge taper, lobes, drift, chips, notches, normal shaping, or shader finish.

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
- Macro/micro irregularity, chips, and artistic normal shaping are dirty-time generation or rendering concerns; none may add steady-state per-frame mesh work.

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
- GM-R12B.1E is closed by its ordinary and bevel-preview runtime audits. R13A.9a is now the accepted recovery baseline: current preview passed; both `33/33` matrices passed; micro-topology component `14/24/30` is normalized; seed-8889 edges `13/23/39` are certified; edge `40` remains excluded; recovery fixtures passed `5/5`; unresolved fixtures are zero; and no case timed out or cancelled. R13A.9 remains a rejected intermediate.

## Edge-wear conflict-search execution contract

- A Generated Mass edge-wear evaluation may own only one active full-shell conflict frontier. A provisional corner/full-shell search may not recursively invoke the plane-kernel coexistence search.
- Ordinary non-provisional cases retain the existing plane-kernel coexistence owner. Provisional cases disable kernel recursion and return exact failure evidence to their single active frontier.
- Explicit editor validation searches are capped at 128 states and five seconds. State-budget exhaustion, time-budget exhaustion, and user cancellation are distinct reported outcomes; none may commit partial geometry.
- A synchronous matrix case must poll the cancelable editor progress callback between search states and clear the transient callback after the case.
- Priority ordering is fewest exclusions, lowest removed accepted artistic score, lowest removed certified width, then deterministic source-edge order. The first fully certified state is committed; exhaustive post-success optimization is forbidden.

R13A.9a is the accepted runtime baseline over the immutable certified-baseline architecture. R13A.9 remains a rejected intermediate because it missed both intended target executions and triggered an unrelated five-second material-recovery search. The recovery architecture is frozen unless new visible evidence demonstrates a meaningful edge that the current certified baseline wrongly excludes.


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
- Timeout, cancellation, exhaustion, or no acceptable recovery retains the current certified working baseline and its edge identity.

The accepted runtime gate is the R13A.9a one-click suite: current preview passed; topology and artistic-preview matrices passed `33/33`; outlier closure passed `5/5`; unresolved is `0`; negative exclusion passed `1/1`; state was preserved; and no timeout or cancellation occurred. This closes basic bevel/recovery work and opens EW-V1 macro irregularity as the next feature.


## EW-V1A.2 construction-width retention status

V1A.1 control cleanup is Unity-validated: its one-click suite passed zero-control parity, determinism, distribution, both `33/33` matrices, recovery `5/5`, unresolved `0`, and negative exclusion `1/1`. Active-width inspection nevertheless exposed an incomplete bounded-width schedule: an isolated rail could succeed at one width, owner/support construction could fail, and the edge could be excluded without testing smaller construction widths.

EW-V1A.2 preserves the frozen geometry contracts and extends only the existing finite width schedule:

- rail failure and rail-success/construction failure share one maximum of twelve attempts;
- every next attempt is `0.75` of the last solved width, bounded by the existing minimum stable width;
- only the existing `owner-face-support-insufficient` class may continue after a rail success;
- the first fully certified lower width wins and is reported as ordinary width reduction;
- minimum/budget exhaustion remains a truthful complete infeasibility proof;
- topology, face-quality, containment, volume/bounds, artistic, footprint, shallow-angle, micro-suppression, coexistence, and production behavior are not relaxed.

The editor macro contract additionally rejects unresolved owner/support exclusions and unproven losses of zero-macro certified meaningful edges. Unity validated the intended edge-10 recovery, but the minimum-tier matrix remained `32/33`; EW-V1A.2a owns that final classification closure. R13A.9a remains the accepted uniform baseline.


## EW-V1A.2a stable-width classification status

V1A.2 is runtime-proven for its intended active case: seed `8889` source edge `10` remains selected and certifies at a reduced width. The same suite passed macro parity, determinism, distribution, retention, recovery `5/5`, unresolved `0`, and negative exclusion `1/1`, but both matrices stopped at `32/33` on seed `8889` at the minimum Width tier.

The remaining rule is conservative and local:

- a reduced isolated result at the absolute `minimumStableEdgeLength` floor is excluded before corner solving;
- widths below `minimumStyleWidth` remain allowed under the existing `minimumStyleWidth * 0.25` recovery policy;
- the stable floor, width schedule, geometry kernels, macro controls, artistic scoring, and production path remain unchanged;
- a captured corner blocker replaces an empty or `none` batch primary failure.

EW-V1A.2a runtime validation passed both matrices `33/33`, preserved the active edge-10 certification, and passed macro and negative-exclusion contracts. That suite remained failed only because two non-certification-required historical fixtures used the new stable-floor terminal reason. EW-V1A.2b subsequently mapped that exact corroborated audit state in the editor resolver and passed the complete acceptance suite; production geometry remained unchanged.


## EW-V1A.2b fixture-resolution status

The stable-floor terminal reason is a production viability conclusion, but its historical outlier acceptance is editor-only. The resolver requires matching viability/final reasons plus isolated-success, positive-width, and fully inactive/unmaterialized evidence before reporting `stable-width-floor-proven-infeasible`. Certification-required visual fixtures cannot pass through this route.

No production generation, geometry, recovery, control, or rendering contract changes in V1A.2b.


## EW-V1A accepted freeze status

`EW-V1A.2b` passed its complete Unity acceptance gate: current preview; macro zero parity, determinism, distribution, and retention; topology `33/33`; artistic preview `33/33`; outlier closure `5/5`; unresolved `0`; negative exclusion `1/1`; and no cancellation or terminal failure. At the accepted active validation setting, Coverage was `1`, Strength was `0.55`, 39 ordinary edges participated, and the current shell certified `31/31` selected edges.

The accepted V1A boundary is deterministic variation between edges only. It does not include taper, lobes, drift, chips, notches, artistic normal shaping, or final material finish. `EW-V2` is the next feature owner; its exact implementation plan is not part of this freeze.
