# Generated Mass Edge-Wear Progress Log and Implementation Checklist

## Canonical log policy

This is the sole canonical Generated Mass edge-wear progress ledger. It owns patch history, methods tried, validation results, the current blocker, and the active next step.

The code inventory, recovery architecture, and framework documents contain only their own current stable facts. They may reference this file but must not maintain competing or complementary progress histories.

## Current accepted state

- **Accepted uniform basic bevel/recovery fallback:** `EW-B4.2R13A.9a`.
- **Accepted and frozen edge-width baseline:** `EW-V1A.3b`. Its public Macro Strength `0..1` maps to effective amplitude `0..0.55`, applies the accepted continuous `15°..90°` dihedral bias with sharp-edge reduction permission `0.35`, and retains the EW-V1A.2f scalar recovery architecture.
- **Unity acceptance evidence:** the complete `EW-V1A.3b-suite` passed current preview, Macro zero parity, angle mapping, determinism, distribution, retention, topology `33/33`, artistic preview `33/33`, outlier resolution `5/5`, negative exclusion `1/1`, cancellation `0`, and terminal reason `none`.
- Macro width, scalar recovery, isolated width schedules, conflict-cluster reduction, and uniform bevel response are frozen. Reopen them only for a demonstrated regression caused by a later feature.
- Seed `8889` retains active/certified source edge `10`, certified edges `13`, `23`, and `39`; source edge `40` remains intentionally excluded; micro-topology component `14/24/30` remains suppressed. Edge `10` remains a documented isolated-construction width limitation, not an active Macro task.
- Width remains constant along each edge. Universal geometric within-edge profiling and EW-S1 object-space normal/material breakup are retired and removed. The retained rendering path is the uniform UV2.z-marked bevel response: Response Strength, Brightness Lift, Worn Edge Tint, Tint Influence, and Softness.
- Existing stale EW-S1 YAML keys may remain inert until Unity resaves affected serialized assets. Scenes and other serialized assets are not edited by V1A.3.
- **Accepted and frozen render-surface infrastructure:** `EW-C1A.1a.8`. The final one-click Unity suite passed Macro contract, topology `33/33`, preview `33/33`, outliers `5/5`, negative exclusion `1/1`, cancellation `0`, and terminal reason `none`. One logical polygon continues to own one authored surface group and one final shared render normal with the unchanged `0.5` agreement guard.

Historical ledger rule: sections below preserve method history and old acceptance checklists. Unchecked boxes inside superseded or historical patches are evidence of what was not accepted at that time; they are not active work unless a later section explicitly reactivates them.

## Active feature

```text
EW-C1A.3o — Bounded endpoint-cell subface reconstruction
```

EW-V1A.3b and EW-C1A.1a.8 remain accepted and frozen. EW-C1A.3g remains the accepted ownership/fallback boundary, EW-C1A.3h remains the accepted minimum-width endpoint-conflict detector, and EW-C1A.3i remains the accepted preparation-performance baseline. Unity report `Pasted text(162).txt` runtime-rejects C1A.3n as a recovery result while preserving every frozen correctness and timing gate: complete matrix `33/33`, enabled success baseline `6/22`, `104` endpoint-patch attempts, `0` prepared/applied, `0` false positives, `0` guard false negatives, and no identity/materialization mismatch. Endpoint-local support executed, but the replacement still owned complete long polygon faces; maximum removed radius remained `1.02464962` and patch-native axial influence reached `1.48524439` against `0.0346355699` minimum allowance. C1A.3o keeps the same corner-chip transaction, ordinary bevel identities, guard, cache, fallback, controls, and visual intent. It changes only the recovery ownership unit from a complete face to a bounded endpoint-local subface, preserves remote remainders, and reconstructs a missing local incident bevel fragment from its already prepared candidate when the failed multi-bevel shell has consumed it.

**Scope sanity:** this is still the original corner-chip/ordinary-edge-bevel coexistence problem. It is not a new edge-wear feature, shader task, material task, or general mesh rewrite. The research became deep because the proven failure occurs inside exact convex-polyhedron topology at one shared endpoint; C1A.3o is the final justified ownership correction before another runtime decision.

## EW-C1A.3o implementation plan — bounded endpoint-cell subface reconstruction

### Status

- [x] Interruption/status audit complete; no C1A.3o source or package survived.
- [x] Exact delivered C1A.3n baseline reconstructed and byte-reconciled.
- [x] Canonical plan recorded before implementation.
- [x] Replace whole-face endpoint-patch ownership with bounded endpoint-cell face subdivision.
- [x] Preserve remote face remainders with original feature, provenance, normal, and surface ownership.
- [x] Reconstruct missing local incident bevel fragments from prepared bevel/source planes inside the bounded cell.
- [x] Store and verify cell planes, source-face signatures, split/remainder signatures, local fragments, stitch loops, and cap evidence.
- [x] Add endpoint-cell and synthetic-fragment telemetry through report, matrix CSV, and aggregate suite.
- [x] Reconcile framework, recovery architecture, and code inventory.
- [x] Run static, scope, package-replay, and compliance validation.
- [ ] Unity compile and complete one-click suite pending user execution.

### Runtime evidence and current blocker

- Unity report `Pasted text(162).txt` completes all `33/33` cases within budget but leaves the corner matrix at `17/33`: `104` endpoint-patch attempts, `0` prepared, `0` applied, `66` locality rejections, `34` patch-extraction rejections, `1` disconnected patch, `1` boundary crossing, and `2` incident-band-join failures.
- Endpoint-local support executed (`480` samples, minimum `6` per successful incident set), but maximum removed-vertex radius remained `1.02464962`; maximum patch-native axial influence reached `1.48524439` against a minimum allowance of `0.0346355699`.
- The remaining defect is ownership, not tolerance: the recovery still selects and replaces complete polygon faces. A locally valid cut on one end of a long bevel/source face can therefore remove a remote vertex or make the stored stitch boundary span most of the source edge.
- Some incident bevel fragments are already absent from the failed edge-only shell. A recovery cannot require the exact fragment it is meant to reconstruct to survive as an existing `EdgeBevelPlane` polygon.

### Approved implementation

1. Preserve the accepted C1A.3g exact-baseline fallback, C1A.3h endpoint-conflict guard, C1A.3i preparation cache/replay, one complete authoritative build, exact identities, mandatory ring, unrelated retention, topology/render gates, and all timing limits.
2. Build one deterministic endpoint cell around the implicated source vertex. For each retained incident source edge, create an axial limit plane perpendicular to that edge at the existing patch-native allowed endpoint influence. The local cell is the intersection of all source-side axial half-spaces.
3. Partition every intersected source or incident-bevel polygon into endpoint-local fragments and untouched remote remainders. Apply the junction boundary only to local fragments. Preserve remote remainders exactly in feature, strength, provenance, normal, and shared split positions.
4. Reject any local removal that reaches a non-incident bevel identity, any open/non-manifold split, multiple or branched local boundaries, remote remainder loss, or cell influence beyond an incident axial limit. Do not enlarge endpoint or axial allowances.
5. When an incident bevel polygon is absent from the failed edge-only shell, reconstruct a synthetic local incident fragment from its prepared bevel plane, its two owner source-face planes, the source shell, and the endpoint cell. The synthetic fragment retains that incident source-edge identity and is used only inside the bounded cell.
6. Generate one connecting `BoundedEndpointCap`, require it to join every incident local fragment, and require the recombined remote remainders plus local replacement to form one closed manifold shell.
7. At prepared and legal-minimum widths, require matching incident identities, endpoint-cell plane identities/limits, source-face selection signatures, local/remote split topology, local stitch-loop class, and one stable cap.
8. Store the prepared cell planes, matched source-face signatures, replacement faces including remote remainders, local fragment signatures, remote remainder signatures, split-loop signatures, synthetic incident identities, and cap metrics.
9. During the existing one authoritative materialization, reproduce the ordinary shell once, verify all prepared source/split signatures, replace only the matched source faces with the stored partitioned faces, then run every existing final identity, coverage, topology, triangulation, render-channel, and soup check. Any mismatch is a recovery false positive and returns the exact ordinary baseline.
10. Add diagnostics for axial cell limits, faces subdivided, local fragments, remote remainders, synthetic incident fragments, missing synthetic identities, cell vertices/faces, local/remote split signatures, cap joins, and authoritative split-signature mismatches.

### Implemented result

- [x] Every reached source or incident-bevel polygon is subdivided through deterministic source-edge axial planes. The source-side fragment remains endpoint-local; every opposite fragment is retained as a remote remainder with unchanged feature, strength, provenance identity, and authored normal.
- [x] The junction plane clips only the endpoint-local fragment. The recombined trial shell contains all untouched original faces, preserved remote remainders, retained local fragments, any required synthetic incident fragment, and exactly one `BoundedEndpointCap`.
- [x] Missing incident bevel fragments are reconstructed from an isolated shell built with the same prepared bevel candidate and source faces, then clipped through the identical endpoint cell. No new authored bevel identity is introduced.
- [x] Prepared and legal-minimum trials reuse the exact prepared cell limits and require matching incident/synthetic identities plus selected-face, boundary, fragment, remainder, cell-vertex, and cell-face topology cardinalities.
- [x] Authoritative application verifies cell-limit integrity, exact selected source-face signatures, outer stitch-boundary signatures, replacement cardinality, exactly one bounded cap, and the exact multiset of stored local-fragment and remote-remainder signatures before splicing.
- [x] Endpoint-cell evidence reaches the integration plan, search aggregate, current report, 135-column matrix CSV, matrix aggregate, one-click suite, sentinel, and comprehensive C1A.3o contracts.
- [x] Final scope is exactly nine files. `MassGenerator.EdgeWear.PlaneCutKernel.cs`, `MassGenerator.EdgeWear.Orchestration.cs`, `MassGenerator.EdgeWear.SelectionAndCorners.cs`, and the C1A.3h guard body remain byte-identical to the delivered C1A.3n baseline.
- [x] Static architecture, ownership, field, arity, CSV, directive, lexical, scope, and format validation passed `499/499`; patch and changed-files ZIP replay each reproduce all `353/353` Assets files byte-for-byte with zero mismatches.
- [ ] Unity 6000.5.0f1 compilation and the complete `EW-C1A.3o-suite` remain pending user execution. Runtime acceptance still requires at least one previously rejected recovery to prepare and apply while every frozen gate remains green.

### Approved file scope

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
- `Assets/Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
- `Assets/Game/Procedural/Masses/MassGenerator.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.PlaneCutKernel.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.PlaneCutJunctionSolver.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

No file may be created, deleted, moved, or renamed inside `Assets`. No Inspector control, serialized field/default, production generation mode, shader/material/cloud integration, scene, prefab, endpoint allowance, width floor, corner ranking, or global solver behavior may change.

### Unity acceptance gate

- Complete matrix `33/33`, retain the existing six enabled successes, and prepare/apply at least one previously rejected endpoint-cell recovery.
- Recovery false positives `0`, guard false negatives `0`, plan/materialization mismatches `0`, disabled parity `11/11`, unrelated retention and mesh channels `33/33`.
- Corner matrix below `35 s`, every case below `5 s`, full suite below `90 s`.

## EW-C1A.3n implementation plan — endpoint-anchored support and patch-native axial certification

### Status

- [x] Read-only review complete.
- [x] Canonical plan recorded before implementation.
- [x] Replace full incident-face maximum support with deterministic endpoint-local support sampling.
- [x] Add candidate pre-extraction rejection for missing local incident support and remote controlling support.
- [x] Replace endpoint-patch use of `IsPlaneCutJunctionInfluenceLocal(...)` with patch-native axial certification.
- [x] Add support and axial evidence through plan, status, report, case CSV, and aggregate outputs.
- [x] Advance active contracts and reconcile stable architecture documents.
- [x] Run static, scope, package-replay, and final compliance checks.
- [ ] Unity compile and complete one-click suite pending user execution.

### Read-only evidence

- Authoritative reconstructed baseline is `/mnt/data/ew_c1a3n_baseline`: `Assets-Code-Archive(18).zip` overlaid with the corrected full C1A.3l package and then the delivered C1A.3m package. No `.git` directory is present, so repository status/history are unavailable; byte comparison against this reconstructed baseline is the scope authority.
- Unity report `/mnt/data/Pasted text(161).txt` reports `cornerEndpointPatchRecoveryAttempts=104`, `Prepared=0`, `Applied=0`, `Rejects=104`, `FalsePositives=0`, `Locality=66`, `PatchExtraction=25`, `DisconnectedPatch=10`, `BoundaryCrossing=1`, `IncidentBandJoin=2`, `cornerChippingCases=17/33`, `cornerChippingElapsedMs=18906.103`, and all case/matrix/suite budgets passing.
- The same report records `MaximumRemovedVertexRadius=1.02464962`, while maximum generated intersection and replacement radii are `0.21821025`. This proves that the dominant locality failure is remote original geometry being placed on the removed side, not generated replacement geometry exceeding the endpoint neighborhood.
- `TryBuildPlaneCutEndpointPatchCandidate(...)` computes `currentSupport` as the maximum projection over every vertex of every incident `EdgeBevelPlane` face, then sets `planeDistance=currentSupport-targetCutback`. Incident bevel polygons span the full source edge, so a remote vertex can control boundary placement.
- `TryExtractPlaneCutEndpointPatch(...)` and `TryClipPlaneCutEndpointPatchFaces(...)` already implement exact removed/intersection/replacement locality, cut-local component growth, conservative non-incident-bevel rejection, one closed untouched stitch loop, and stored replacement signatures. These remain the active ownership model.
- `TryBuildPlaneCutEndpointPatchReplacement(...)` still calls `IsPlaneCutJunctionInfluenceLocal(...)`, which first requires one global connector face and measures full connector influence/shared-axis ratios. This validator belongs to historical global junction planes and must remain unchanged there, but it must no longer certify a bounded endpoint patch.
- Direct callers/consumers reviewed: `TryPrepareCornerDamageEndpointPatchRecovery(...)`, prepared/minimum replacement parity, `MaterializePlaneCutBevelSolvedPlan(...)`, coexistence/exclusion/retreat propagation, integration-plan evidence transfer, current status, diagnostics logging, matrix case capture/CSV, suite aggregation, and the four canonical Generated Mass documents.
- Historical comparison reviewed: accepted C1A.3g fallback authority, C1A.3h guard, C1A.3i cache/replay, runtime-rejected C1A.3j/C1A.3k, bounded C1A.3l, exact-locality C1A.3m, the corrected C1A.3l kernel propagation, and the current reconstructed baseline.

### Approved implementation

1. Preserve C1A.3m exact removed/intersection/replacement locality, cut-local component traversal, conservative non-incident-bevel extraction rejection, one closed untouched stitch loop, exact selected-face/boundary signatures, prepared/minimum parity, authoritative splice, exact baseline fallback, and one complete authoritative build maximum.
2. Compute `localRadius` before candidate plane placement from the existing minimum-stable-length, incident-width, and target-cutback policy.
3. For each incident bevel identity, gather deterministic endpoint-local support samples from its bevel faces: vertices inside the radius, closest points where face edges enter/cross the radius, and local segment endpoints/intersections required to represent support inside the endpoint neighborhood.
4. Require at least one finite local support sample for every incident identity. Compute the selected support projection only from those samples. Record the global incident-face support separately for evidence, but never use it to place the endpoint boundary.
5. Place the candidate plane at `localSupport-targetCutback`. Reject before extraction if any incident identity lacks local support, no endpoint-local shell geometry lies on the removed side, the controlling support sample is remote, or the removed-side component is already provably nonlocal.
6. Replace only the endpoint-patch call to `IsPlaneCutJunctionInfluenceLocal(...)` with a dedicated patch-native axial validator. For each incident source edge, project the stored replacement/cap geometry onto the edge from the implicated endpoint, record maximum axial influence, derive allowed endpoint consumption from existing incident width, candidate cut depth, plane tolerance, and minimum stable edge length, and reject opposite-end influence or excessive axial consumption.
7. Keep the historical global-junction influence method and all historical global solver declarations unchanged and uncalled.
8. Add per-candidate and aggregate evidence for local support sample counts, local support radius/projection, global-minus-local support delta, controlling support identity/radius, maximum/allowed axial influence, and axial rejection identity/endpoint.
9. Advance active contracts to `EW-C1A.3n` without adding serialized state, controls, Inspector actions, assets, or new geometry dependencies.
10. Unity acceptance requires at least one previously rejected endpoint patch to prepare and apply, existing six enabled successes retained, zero recovery false positives/guard false negatives/identity mismatches, unrelated retention and channels `33/33`, complete matrix below `35 s`, and no case above `5 s`.

### Implemented result

- [x] Candidate placement now uses endpoint-local support only. Every incident identity must contribute finite in-radius support; full-face support is diagnostic evidence and no longer controls `planeDistance`.
- [x] The bounded replacement now uses patch-native axial certification over its ordered stitch loop and cap. The historical global-junction influence validator and `SolvePlaneCutGlobalJunctionSystem(...)` remain unchanged for their historical owner, with the global solver still declaration-only.
- [x] Support and axial evidence reaches the integration plan, per-attempt status, certified search telemetry, current report, 123-column case CSV, matrix aggregate, suite aggregate, sentinel, and comprehensive contracts under `EW-C1A.3n`.
- [x] Final scope is exactly the nine approved files. `MassGenerator.EdgeWear.PlaneCutKernel.cs`, C1A.3i replay owners, and the C1A.3h guard body remain byte-identical to the corrected C1A.3m baseline. No file was created, deleted, moved, or renamed.
- [x] Static/compliance validation passed `79/79`; all `209` C# files passed lexical/delimiter/directive checks; patch and changed-files ZIP replay each reproduced all `353/353` project files byte-for-byte with zero mismatches.
- [ ] Unity 6000.5.0f1 compilation and the complete `EW-C1A.3n-suite` remain pending user execution. Runtime acceptance still requires at least one prepared/applied recovery and every frozen correctness/performance gate.

### Approved file scope

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
- `Assets/Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
- `Assets/Game/Procedural/Masses/MassGenerator.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.PlaneCutJunctionSolver.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

`MassGenerator.EdgeWear.PlaneCutKernel.cs` remains frozen at corrected C1A.3l/C1A.3m propagation. No file may be created, deleted, moved, or renamed inside `Assets`. Production generation, shaders/includes, cloud integration, materials, scenes, prefabs, serialized controls/defaults, Inspector actions, endpoint allowance, width floors, guard decisions, C1A.3i cache/replay keys, and the historical global junction solver remain unchanged.

## EW-C1A.3m implementation plan — exact cut-locality semantics

### Status

- [x] Read-only review complete.
- [x] Canonical plan recorded before implementation.
- [x] Replace full-selected-face extent locality with exact removed/intersection/replacement locality.
- [x] Restrict connected-component growth to locally affected shared edges.
- [x] Add exact locality evidence through plan, status, report, case CSV, and aggregate outputs.
- [x] Advance active contracts and reconcile stable architecture documents.
- [x] Run static, scope, package-replay, and final compliance checks.
- [ ] Unity compile and complete one-click suite pending user execution.

### Read-only evidence

- Corrected C1A.3l source is the complete hotfixed tree at `/mnt/data/ew_c1a3l_compile_hotfix_build/full-patch-replay`; the compile hotfix propagates `PlaneCutEndpointPatchReplacement` consistently through coexistence and retreat trials.
- Unity report `/mnt/data/Pasted text(160).txt` reports `cornerEndpointPatchRecoveryAttempts=104`, `Prepared=0`, `Applied=0`, `Rejects=104`, `FalsePositives=0`, `Locality=102`, `PatchExtraction=2`, `cornerChippingCases=17/33`, `cornerChippingElapsedMs=18454.468`, and all case/matrix/suite budgets passing.
- No attempt reached boundary-loop, boundary-crossing, cap creation, incident-band joining, stitch topology, band integrity, prepared/minimum parity, or materialization-signature checks. Runtime evidence therefore isolates the active blocker before those stages.
- `TryExtractPlaneCutEndpointPatch(...)` currently rejects every selected face if any original face vertex lies outside `localRadius`. Long bevel/source polygons can be locally clipped near the endpoint while retaining distant negative-side vertices unchanged, so this check measures original face extent rather than modified geometry extent.
- Component growth currently traverses any shared edge between two faces that each contain at least one positive-side vertex. C1A.3m must instead traverse only edges whose local segment is actually affected by the cut, so a distant positive vertex cannot pull unrelated face area into the patch.
- `TryClipPlaneCutEndpointPatchFaces(...)` already exposes the exact removed side, generated intersections, surviving replacement polygons, and cap vertices required for cut-locality certification without adding a geometry dependency.

### Approved implementation

1. Keep C1A.3l selected-face-only clipping, one closed untouched stitch loop, exact selected-face/boundary signatures, prepared/minimum parity, authoritative splice, false-positive fallback, and one-complete-build maximum.
2. During extraction, classify each vertex and edge by signed distance to the local boundary. Seed from incident bevel faces with a real local cut. Grow the selected component only across shared edges whose segment participates in the positive-side region or intersects the plane within the certified radius.
3. Remove the rule that every original selected-face vertex must lie inside `localRadius`.
4. Before clipping, reject only positive-side vertices that will be removed and lie outside `localRadius`.
5. During clipping, certify every new edge-plane intersection, every generated cap vertex, and every new replacement vertex against `localRadius`. Original retained negative/on-plane vertices outside the radius are permitted and counted diagnostically.
6. Record maximum removed-vertex radius, maximum intersection radius, maximum replacement-vertex radius, retained-outside-radius count, selected-face count before/after cut-local filtering, and locality failure source (`removed-vertex`, `intersection`, `cap`, or `replacement`).
7. Preserve the two current non-incident-bevel `PatchExtraction` rejections unless the local-edge traversal proves they are no longer part of the selected component; do not broaden ownership to non-incident bevels.
8. Advance active contracts to `EW-C1A.3m` and propagate the new evidence through current status, report, matrix case CSV, aggregate suite, sentinel, and comprehensive outputs without adding controls.
9. Unity acceptance requires at least one previously rejected endpoint patch to prepare and apply, existing six enabled successes retained, zero recovery false positives/guard false negatives/identity mismatches, unrelated retention and channels `33/33`, complete matrix below `35 s`, and no case above `5 s`.

### Approved file scope

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
- `Assets/Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
- `Assets/Game/Procedural/Masses/MassGenerator.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.PlaneCutJunctionSolver.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

`MassGenerator.EdgeWear.PlaneCutKernel.cs` is excluded unless exact implementation evidence requires a signature change; its corrected C1A.3l endpoint-patch propagation is frozen. No file may be created, deleted, moved, or renamed inside `Assets`. Production generation, shaders/includes, cloud integration, materials, scenes, prefabs, serialized controls/defaults, Inspector actions, endpoint allowance, width floors, guard decisions, C1A.3i cache/replay keys, and the historical global junction solver remain unchanged.

### Implemented result

- [x] Removed the full-selected-face radius rejection. Original retained negative/on-plane vertices may remain outside the endpoint radius and are counted rather than rejected.
- [x] Selected-component traversal now crosses only shared edges whose positive segment or exact plane intersection is locally affected; distant positive vertices can no longer pull unrelated face area into the replacement.
- [x] Exact locality certification now applies separately to removed positive vertices, generated edge-plane intersections, generated non-original replacement vertices, and cap vertices. Failure evidence identifies `removed-vertex`, `intersection`, `replacement`, or `cap` ownership.
- [x] Added maximum removed/intersection/replacement radii, retained-outside count, selected-face counts before/after local filtering, and locality failure source through the plan, current status, report, case CSV, matrix aggregate, suite aggregate, sentinel, and comprehensive output.
- [x] Preserved C1A.3l selected-face-only clipping, the one closed untouched stitch loop, exact selected-face and ordered-boundary signatures, prepared/minimum parity, authoritative splice, false-positive fallback, and one-complete-build maximum. The two non-incident-bevel extraction rejections remain conservative.
- [x] Active contracts advanced to `EW-C1A.3m`; framework, recovery architecture, and code inventory now describe exact cut-locality ownership without adding controls or serialized state.
- [x] Final implementation scope is exactly nine approved files. Corrected C1A.3l `MassGenerator.EdgeWear.PlaneCutKernel.cs`, C1A.3i replay owners, the C1A.3h guard algorithm, production generation, shaders, assets, controls, endpoint allowance, and width floors remain byte-identical or behaviorally frozen as specified.
- [x] Static/compliance validation passed `46/46`; all `209` C# files introduced no new structural findings relative to corrected C1A.3l. Final patch and changed-files ZIP replay each reproduced all `353/353` project files byte-for-byte with zero mismatches.
- [ ] Unity 6000.5.0f1 compilation and the complete `EW-C1A.3m-suite` remain pending user execution. Runtime acceptance requires at least one previously rejected patch to prepare and apply, existing six enabled successes retained, zero false positives/guard false negatives/identity mismatches, unrelated retention and channels `33/33`, complete matrix below `35 s`, and no case above `5 s`.

## EW-C1A.3l implementation plan — bounded local endpoint-star face-patch replacement

### Status

- [x] Read-only review complete.
- [x] Canonical plan recorded before implementation.
- [x] C1A.3k global half-space endpoint-star recovery removed or superseded.
- [x] Bounded local patch extraction, clipping, replacement, and signature verification implemented.
- [x] Prepared/minimum-width parity and authoritative splice integrated.
- [x] Per-attempt rejection classification and `EW-C1A.3l` contracts implemented.
- [x] Stable architecture documents reconciled.
- [x] Static, scope, package-replay, and final compliance checks passed.
- [ ] Unity compile and complete one-click suite pending user execution.

### EW-C1A.3l compile hotfix — endpoint-patch signature propagation

- [x] Read-only review of the two Unity compiler errors and all direct coexistence-search callers/definitions completed before code edits.
- [x] Root cause recorded: `PlaneCutEndpointPatchReplacement endpointPatch` was added to one coexistence caller and to `EvaluatePlaneCutCoexistenceExclusionTrial(...)`, but not to `TryResolvePlaneCutCoexistenceByExclusion(...)`; the recursive exclusion-trial call also omitted the new argument.
- [x] Added `endpointPatch` to the coexistence resolver signature, passed it from all three callers, and passed it into every exclusion-trial evaluation.
- [x] Propagated the replacement into retreat-search and retreat-trial shell construction so exclusion/retreat certification evaluates the same prepared local replacement as the authoritative path.
- [x] Exact call-arity/signature scans, all-209-file C# structural checks, scope audit, and package replay passed; Unity compilation remains the required runtime gate.

Approved hotfix scope is limited to this checklist and `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.PlaneCutKernel.cs`. No behavior, thresholds, telemetry schema, controls, shaders, assets, fallback ownership, guard decisions, cache keys, or one-build limits may change.

Hotfix result: the four affected private method chains now have aligned call/definition arity (`16`, `20`, `24`, and `22` arguments respectively), and the prepared endpoint patch reaches coexistence and retreat trial shell construction. Static/compliance validation passes `54/54`; Unity compilation and the complete suite remain pending.

### Read-only evidence

- Authoritative implementation tree is Git working tree `/mnt/data/ew_c1a3k_patchrepo`: `HEAD` is synthetic delivered C1A.3j baseline commit `3b1ff68`; the nine-file working-tree diff is the delivered C1A.3k overlay. `Assets/AGENTS.md` requires this checklist to be the first persistent implementation write.
- Unity report `/mnt/data/Pasted text(158).txt`, SHA-256 `dfda321bae169d963c3ab454897c511de2bc963003d7754c67e436b05a0db584`, reports `cornerEndpointStarRecoveryAttempts=104`, `Prepared=0`, `Applied=0`, `Rejects=104`, `FalsePositives=0`, `cornerChippingCases=17/33`, `cornerChippingElapsedMs=19470.682`, and all case/matrix/suite budgets passing.
- The report's terminal case rows contain five supported two-band stars and eleven supported three-band stars. Thirteen terminal diagnostics state `the endpoint-star boundary could not remove a local prepared-shell vertex without affecting unrelated geometry`; three state `endpoint-star boundary exceeded local influence or shared-axis limits`. The supported star-size gate therefore passes, but the additional infinite plane still has global ownership.
- `MassGenerator.EdgeWear.PlaneCutJunctionSolver.cs::TryPrepareCornerDamageEndpointStarRecovery(...)` builds the complete prepared edge-only shell, chooses up to three normals and two depths, then calls `TryBuildPlaneCutVertexJunctionCandidate(...)` and `TryValidateCornerDamageEndpointStarTrial(...)`.
- `TryBuildPlaneCutVertexJunctionCandidate(...)` rejects any plane that removes a vertex outside one local radius. `TryValidateCornerDamageEndpointStarTrial(...)` passes the candidate into `TryBuildPlaneCutSystemFaces(...)`, which applies `ClipPolyhedron(...)` to every face through a global `VertexJunctionPlane`. This is the exact ownership mismatch demonstrated by the runtime diagnostics.
- `MassGenerator.Polyhedron.cs::ClipPolygon(...)`, `SanitizePolygon(...)`, `CreateOrientedFace(...)`, shared `EdgeKey`/`VertexKey`, welding, topology audit, face-quality audit, one-surface triangulation, and render-channel checks already provide the primitives required to clip a selected face set and validate a stitched replacement without adding a new geometry dependency.
- `MassGenerator.EdgeWear.PlaneCutKernel.cs::MaterializePlaneCutBevelSolvedPlan(...)` owns the single authoritative shell build and all final identities, coverage, topology, triangulation, render validity, and soup. It currently consumes only `PreparedJunctions`; C1A.3l must add one prepared local replacement while retaining this ownership boundary and the one-build maximum.
- Direct callers/consumers reviewed: `GenerateCornerDamageFullCertificationSearch`, `TryPrepareCornerDamageIntegrationPlan`, `TryPassCornerDamageEndpointConflictGuard`, `TryCompleteCornerDamageIntegrationPlan`, `ApplyCornerDamageIntegrationPlanEvidence`, corner report construction, matrix case capture, CSV output, aggregate suite counters, and the four canonical Generated Mass documents.
- Historical comparison reviewed: C1A.3g exact baseline bundle/fallback, C1A.3h guard, C1A.3i replay/cache, C1A.3j exact-two global cap, C1A.3k two/three-band global endpoint-star plane, current `HEAD`, and the complete current working diff.

### Objective and acceptance

1. Replace C1A.3k's global junction-plane cut with one prepared `PlaneCutEndpointPatchReplacement` that owns the implicated source vertex, sorted incident bevel identities, exact selected-face signatures, one ordered untouched boundary loop/signature, replacement faces, cap metrics, and prepared/minimum-width parity evidence.
2. Build the ordinary prepared edge-only shell once. For each deterministic local normal/depth trial, classify only faces connected to the incident bevel seeds that the trial plane actually removes or crosses. Reject when the selected patch is disconnected, branched, contains multiple boundary loops, crosses a selected/unselected boundary edge, or requires modification of an unrelated face.
3. Clip only the selected patch faces with exact shared intersections. Preserve every untouched face unchanged, retain the outside portion of each selected source/bevel face with original provenance, create exactly one `BoundedEndpointCap`, weld/sanitize, and require one closed manifold stitched shell.
4. Require one ordered, closed, non-branching boundary loop between the replacement patch and untouched shell. Store exact prepared face signatures and boundary positions/signature. At legal-minimum widths, require the same incident identities, selected provenance set, boundary topology/signature class, one cap, and all existing local/full certification gates.
5. During the one authoritative materialization, rebuild ordinary bevel faces once, verify the prepared selected-face and boundary signatures, remove only the matched local faces, splice cloned stored replacement faces, then run all existing final cap/band/identity/coverage/topology/triangulation/render checks. Signature mismatch or final failure is a recovery false positive and returns the exact C1A.3g ordinary baseline.
6. Add per-attempt rejection classification for unsupported star, patch extraction, disconnected/branched/multiple loops, boundary crossing, no local removal, cap creation, incident-band join, stitch topology, locality, band integrity, prepared/minimum parity, and final materialization false positive.
7. Preserve C1A.3h guard decisions for unsupported or failed replacements, C1A.3i cache/replay behavior, mandatory ring, unrelated retention, exact identities, normal/tangent channels, stored-soup emission, endpoint allowance, width floors, and all performance budgets.
8. Unity target: complete `33/33`; retain the existing six enabled successes; prepare and apply at least one previously rejected endpoint-star replacement; zero recovery false positives, guard false negatives, identity mismatches, topology/render failures, or budget violations.

### Approved file scope

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
- `Assets/Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
- `Assets/Game/Procedural/Masses/MassGenerator.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.PlaneCutKernel.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.PlaneCutJunctionSolver.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

No file may be created, deleted, moved, renamed, or generated inside `Assets`. `MassGenerator.Types.cs`, `MassGenerator.Polyhedron.cs`, `MassGenerator.EdgeWear.Orchestration.cs`, `MassGenerator.EdgeWear.SelectionAndCorners.cs`, `GeneratedMass.cs`, mesh output, shaders/includes including cloud integration, materials, scenes, prefabs, recipes, metadata, serialized settings/defaults, Inspector controls/actions, production `EdgeWearEvaluationMode.None`, and active-gameplay generation remain unchanged.

### File-by-file sequence

1. Extend non-serialized plan/status/telemetry with a prepared local replacement record, exact patch/boundary signatures, rejection-class counters, and final-splice evidence.
2. Replace `TryPrepareCornerDamageEndpointStarRecovery(...)` with deterministic selected-face extraction, one-loop boundary construction, selected-face-only clipping, cap generation, stitch validation, and dual-width parity.
3. Extend `TryBuildPlaneCutSystemFaces(...)`/materialization to apply at most one stored local replacement after ordinary bevel planes, verify all prepared signatures, and never execute the old global junction-plane cut for C1A.3l.
4. Preserve ranked search behavior and the C1A.3h guard; a failed local replacement remains an endpoint-conflict-guard rejection, while a prepared replacement permits the existing one complete authoritative build.
5. Propagate rejection-class, preparation, application, false-positive, face/loop/cap, signature, and timing evidence through the existing corner report, matrix CSV, aggregate suite, sentinel, and comprehensive outputs; advance active contracts to `EW-C1A.3l` without new controls.
6. Reconcile framework, recovery architecture, and code inventory after implementation behavior is final.
7. Reread complete modified files and direct producers/consumers; compare final behavior with C1A.3k, C1A.3i, current `HEAD`, and the plan; run exact-scope diff, structural C# checks, contract scans, line-ending/BOM checks, patch/ZIP replay, and record Unity validation as pending.

### Risks and controls

- **Patch leaks into unrelated geometry:** selected/unselected shared edges must remain entirely inside/on the local cut; any crossed boundary edge rejects the trial before mutation.
- **Disconnected or ambiguous ownership:** selected faces must form one connected component with one closed non-branching boundary loop; multiple components/loops or degree other than two reject.
- **Stale authoritative splice:** prepared face signatures and ordered boundary positions must match the materialized edge-only shell exactly within existing `VertexKey`/merge tolerances; mismatch fails closed as a recovery false positive.
- **Identity loss:** selected bevel faces retain original edge provenance; the new cap uses `BoundedEndpointCap` and does not replace an incident edge identity. Existing mandatory/unrelated identity checks remain authoritative.
- **Prepared/minimum drift:** both widths must produce the same incident set, selected provenance signature, boundary topology/signature class, and one cap; uncertainty or mismatch rejects preparation.
- **Performance regression:** at most six local trials per guard conflict, one selected connected component, one prepared replacement, and one complete authoritative build. No search queue, second build, persistent cache, runtime callback, or per-frame work is permitted.

### Implemented result

- [x] Replaced the active C1A.3k additional half-space recovery with a prepared `PlaneCutEndpointPatchReplacement` that owns exact selected-face signatures, one ordered untouched stitch loop, replacement faces, and prepared/minimum boundary evidence. The historical global junction solver remains declaration-only.
- [x] Preparation now starts from crossed incident bevel faces, extracts only their connected affected face component, rejects non-incident bevel ownership or local-radius overflow, and requires exactly one closed degree-two selected/unselected boundary loop before any mutation.
- [x] Only selected patch faces are clipped. Surviving selected faces retain their original feature/provenance; one `BoundedEndpointCap` supplies connecting geometry; unrelated faces are neither globally clipped, welded, nor sanitized.
- [x] Prepared and legal-minimum-width trials must agree on incident identities, selected provenance, and boundary topology. The one authoritative materializer exact-matches selected-face signatures plus ordered boundary positions before removing only those local faces and inserting the stored replacement. Any mismatch is a recovery false positive and returns the exact C1A.3g ordinary baseline.
- [x] Added explicit rejection classes and per-case/suite evidence for unsupported stars, extraction, disconnected patches, boundary loops/crossings, no local removal, cap creation, incident-band joining, stitch topology, locality, band integrity, prepared/minimum parity, and materialization signatures. Active contracts are `EW-C1A.3l`.
- [x] Final scope is exactly the ten approved files; no shader, material, scene, prefab, serialized setting, Inspector control, replay key, endpoint allowance, width floor, production mode, or unrelated system changed.
- [x] Static and compliance validation passed `48/48`; all `209` C# files introduced no new structural findings; patch and changed-files ZIP replay reproduced all `353/353` project files byte-for-byte from the delivered C1A.3k baseline.
- [ ] Unity 6000.5.0f1 compilation and the complete `EW-C1A.3l-suite` remain pending user execution. Runtime acceptance requires at least one previously rejected endpoint conflict to prepare and apply, zero false positives/guard false negatives/identity mismatches, complete `33/33` matrix coverage, and all existing timing and fallback gates.

## EW-C1A.3k implementation plan — three-band endpoint-star preconstruction ownership

### Status

- [x] Read-only review complete.
- [x] Canonical plan recorded before implementation.
- [x] C1A.3j exact-two post-bevel recovery removed or superseded.
- [x] Two/three-band endpoint-star boundary preparation implemented.
- [x] Dual-width local star certification integrated with the one authoritative build.
- [x] Telemetry and active contracts advanced to `EW-C1A.3k`.
- [x] Stable architecture documents reconciled.
- [x] Static, scope, package-replay, and final compliance checks passed.
- [ ] Unity compile and complete one-click suite pending user execution.

### Read-only evidence

- Authoritative source is Git working tree `/mnt/data/ew_c1a3i_git`: `HEAD` is synthetic accepted C1A.3i commit `21a5351`; the ten-file working-tree diff is the delivered C1A.3j overlay. `Assets/AGENTS.md` requires this checklist to be the first implementation write.
- `Pasted text(157).txt` reports `cornerSharedEndpointJunctionRecoveryAttempts=104`, `Prepared=0`, `Applied=0`, `Rejects=104`, `FalsePositives=0`, `cornerChippingCases=17/33`, `cornerChippingElapsedMs=18230.049`, and all case/matrix/suite budgets passing.
- Eleven final failure diagnostics state `shared endpoint recovery supports exactly two retained bevel bands at one source vertex`; five state `a selected vertex-junction plane emitted no unique cap`. This proves the exact-two eligibility rule excludes the common local star and the conflict-rail-derived plane does not necessarily cut the surviving edge-only shell.
- `TryPrepareCornerDamageSharedEndpointJunctionRecovery` currently computes one cap from only victim/foreign normals and conflict-segment support, then clips it after all edge planes. `TryBuildPlaneCutVertexJunctionCandidate` already derives a guaranteed local removal plane from the actual edge-only shell support and protects unrelated source vertices, but it is currently reachable only through the rejected global solver.
- `BuildPlaneCutJunctionNormalOptions`, `TryBuildPlaneCutVertexJunctionCandidate`, `TryBuildPlaneCutSystemFaces`, `DoesPlaneCutJunctionJoinIncidentBevels`, `IsPlaneCutJunctionInfluenceLocal`, and `IsPlaneCutJunctionTrialGeometryValid` provide deterministic local construction and full exact certification without requiring `SolvePlaneCutGlobalJunctionSystem`.
- `MaterializePlaneCutBevelSolvedPlan` already consumes `PlaneCutBevelSolvedPlan.PreparedJunctions` during the existing single complete shell build. Final identities, coverage, topology, triangulation, render validity, and soup remain materialization-owned.

### Objective and acceptance

1. Replace the exact-two conflict-rail cap with one endpoint-star transaction that gathers the complete retained incident set at the implicated source vertex and supports only deterministic stars of `2..3` bands.
2. Build the edge-only prepared shell once for the ranked candidate, derive bounded normal options from the complete incident star, and place each trial plane from the shell's actual support using the existing unrelated-source-vertex and locality limits. No global search queue, edge deferral, breadth-first state, or second complete authoritative build is permitted.
3. Select at most one deterministic boundary by fixed normal/depth ordering. The identical boundary must emit one unique stable cap, join every incident band, remain local, and pass full topology, face-quality, band-integrity, triangulation, and render certification at both prepared and legal-minimum widths.
4. Store only the dual-width-certified boundary in `PreparedJunctions`; final materialization remains sole authority. A final mismatch or failure is a recovery false positive and returns the exact ordinary baseline under C1A.3g.
5. Preserve C1A.3h guard decisions for unsupported or failed stars, C1A.3i cache/replay behavior, mandatory ring, unrelated retention, exact identities, normal/tangent channels, stored-soup emission, and all performance budgets.
6. Unity target: complete `33/33`; retain the existing six enabled successes; prepare and apply at least one endpoint-star recovery; zero recovery false positives, guard false negatives, identity mismatches, topology/render failures, or budget violations.

### Approved file scope

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
- `Assets/Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
- `Assets/Game/Procedural/Masses/MassGenerator.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.PlaneCutKernel.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.PlaneCutJunctionSolver.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

No files may be created, deleted, moved, renamed, or generated inside `Assets`. No shader/include, cloud, material, scene, prefab, serialized setting, Inspector control, production mode, triangulation policy, corner scoring, cut-depth control, width floor, endpoint allowance, replay key, or historical global-junction solver activation may change.

### File-by-file sequence

1. Replace C1A.3j recovery fields and report labels with endpoint-star preparation/application evidence in `MassGenerator.EdgeWear.Types.cs`, `MassGenerator.cs`, diagnostics, and editor matrix contracts.
2. Replace `TryPrepareCornerDamageSharedEndpointJunctionRecovery` with a bounded two/three-band endpoint-star preparation routine in `MassGenerator.EdgeWear.PlaneCutJunctionSolver.cs`; reuse deterministic local helper routines only and keep `SolvePlaneCutGlobalJunctionSystem` uncalled.
3. Keep `PreparedJunctions` threading in `MassGenerator.EdgeWear.PlaneCutKernel.cs`, but update final application/false-positive ownership checks for the endpoint-star contract.
4. Preserve the C1A.3h guard flow in `MassGenerator.cs`: only guard-proven conflicts invoke the local star preparation, unsupported/failed stars continue ranked rejection, and only the first prepared or guard-clear candidate receives the one complete authoritative build.
5. Update framework, recovery architecture, and code inventory with C1A.3j runtime rejection and C1A.3k current ownership.
6. Run exact-scope diff review, editor/player C# structural checks, duplicate-field and contract scans, global-solver call scan, line-ending/BOM checks, and full 353-file ZIP/patch replay.

### Risks and controls

- **Reactivating rejected global junction behavior:** direct prohibition on calling `SolvePlaneCutGlobalJunctionSystem`; no queue, deferred-edge set, state search, backtracking, or independent time budget.
- **Combinatorial local search:** maximum three incident bands, fixed normal ordering, fixed depth factors, first fully dual-width-certified result only, one prepared junction maximum.
- **Post-bevel redundancy:** candidate distance derives from actual edge-only shell support through `TryBuildPlaneCutVertexJunctionCandidate`, not conflict-segment support.
- **Overlong influence or identity loss:** unrelated-source support protection, existing local-influence/shared-axis gates, all-incident band joining, exact band audit, mandatory/unrelated identity checks, and final materialization authority remain mandatory.
- **False-positive preparation:** identical fixed plane must pass prepared and legal-minimum shell certification; final materialization failure increments false-positive evidence and falls back exactly.
- **Performance regression:** no additional complete build; local trial count is statically bounded and remains inside existing `4 s`, `5 s`, `35 s`, and `90 s` budgets.

### Implemented result

- C1A.3j is retained only as runtime-rejected history. Its exact-two conflict-rail cap method and active field/report names are superseded by the C1A.3k endpoint-star contract.
- Guard-proven conflicts gather the complete retained incident identity set at the implicated endpoint. Only matching prepared/minimum stars of two or three bands are eligible; unsupported counts remain ranked rejections and now report the actual incident count.
- Preparation builds the actual edge-only shell once, evaluates at most three deterministic normal options and two fixed depth factors, protects every unrelated source vertex, and counts each bounded local trial explicitly.
- The identical candidate boundary must pass unique-cap, every-incident-band join, locality/shared-axis, topology, face-quality, band-integrity, triangulation, and render-validity certification at both prepared and legal-minimum widths.
- At most one dual-width-certified boundary is stored in `PlaneCutBevelSolvedPlan.PreparedJunctions`. The unchanged single materialization remains final authority; failure after preparation is a recovery false positive and returns the exact C1A.3g ordinary baseline.
- Current status, per-case CSV, matrix aggregate, suite summary, sentinel, and comprehensive contracts now use `EW-C1A.3k` and expose attempts, local trials, prepared/rejected/applied/false-positive counts, endpoint/edge identities, incident count, normal rank, cap metrics, diagnostics, and duration.
- Relative to the delivered C1A.3j baseline, nine files change. `MassGenerator.EdgeWear.PlaneCutKernel.cs` was declared in scope but remains byte-identical because its existing prepared-junction materialization path already satisfies C1A.3k ownership.
- Static validation passes `26/26` explicit architecture checks, reports no new structural findings across all `209` C# files, preserves one historical global-solver declaration with no call site, one complete materialization call, four centralized baseline-fallback exits, and original file-format shape. Full patch and ZIP replay reproduce the authoritative `353`-file C1A.3k tree byte-for-byte.

## EW-C1A.3i implementation plan — cached normalized foundation and guarded isolated-viability replay

### Status

- [x] Read-only review complete.
- [x] Canonical plan recorded before implementation.
- [x] Normalized source foundation cache implemented.
- [x] Candidate-local isolated-viability replay implemented with exact-match guards.
- [x] Search and matrix telemetry updated to `EW-C1A.3i`.
- [x] Stable architecture documents reconciled.
- [x] Static, scope, replay-package, and final compliance checks passed.
- [ ] Unity compile and complete one-click suite pending user execution.

### Read-only evidence

- Authoritative source is `Assets-Code-Archive(18).zip` overlaid by `GeneratedMass_EW-C1A.3h_ChangedFiles.zip`; the resulting tree contains `353` project files and no Git metadata.
- `Pasted text(155).txt` reports `cornerChippingElapsedMs=37717.684`, `cornerChippingCases=17/32`, `cornerEndpointConflictGuardMilliseconds=781.288`, `cornerCandidatePreparationAttempts=106`, `cornerCandidatePreparationRejects=100`, and `cornerCompleteAuthoritativeBuilds=6`.
- The matrix case rows total approximately `33.898 s` in `integrationPreflightMs`; complete materialization totals approximately `1.220 s`. The dominant cost is repeated preflight, not the guard or the one complete build.
- `MassGenerator.GenerateCornerDamageFullCertificationSearch` currently invokes `GenerateInternal(...CornerDamageIntegrationPreflight...)` once per ranked candidate.
- `MassGenerator.EdgeWear.Orchestration.ApplyGeneratedEdgeWearBevels` repeats `NormalizeEdgeWearMicroTopology` and `BuildEdgeWearBevelCandidates` for every candidate.
- `BuildEdgeWearBevelCandidates` runs `RunEdgeWearIsolatedViabilityPreflight` across every provisional edge; `AuditBoundedSingleEdgeBevel` is the expensive per-edge construction.
- `CornerDamageTransactionAuditResult.AffectedOriginalEdgeIndices` provides the mandatory exclusion set for replay. `EdgeWearCoverageAudit.CloneForTrial` and lifecycle stable identities provide existing immutable audit semantics, but no current search-level replay cache exists.

### Objective and acceptance

1. Reuse the exact normalized micro-topology result across ranked candidates within one corner search.
2. Cache isolated-viability evidence after a full audit and replay it only when the current ordinary edge is provably unchanged: same stable identity, endpoints, owner normals, length, dihedral, requested width, locality interval/limiting evidence, and not present in the current transaction's affected identity set.
3. Treat every failed comparison, missing record, mandatory edge, non-finite value, or ambiguous orientation as a replay miss and execute the unchanged full audit.
4. Preserve C1A.3h guard pass/reject logic, one-complete-build maximum, final authority, exact fallback, mandatory ring, unrelated retention, identity, topology, normal/tangent, and production contracts.
5. Add report evidence for normalized-foundation builds/reuses and isolated-replay attempts/hits/misses/full evaluations.
6. Unity target: complete `33/33`; corner matrix `<35 s`; each enabled case `<=4 s` target and `<5 s` hard maximum; zero guard false negatives, plan mismatches, or replay parity diagnostics.

### Approved file scope

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
- `Assets/Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
- `Assets/Game/Procedural/Masses/MassGenerator.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Orchestration.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

No files may be created, deleted, moved, renamed, or generated inside `Assets`. No shader/include, cloud, material, scene, prefab, serialized setting, Inspector control, production mode, triangulation, corner scoring, cut depth, width floor, or guard threshold may change.

### File-by-file sequence

1. Add non-serialized foundation/replay records and telemetry fields in `MassGenerator.EdgeWear.Types.cs` and public status fields in `MassGenerator.cs`.
2. Establish one search-scoped replay cache in `GenerateCornerDamageFullCertificationSearch`; preserve all C1A.3g fallback exits and C1A.3h completion logic.
3. Reuse normalized micro-topology in `MassGenerator.EdgeWear.Orchestration.cs`.
4. Add exact-match replay/store helpers around `RunEdgeWearIsolatedViabilityPreflight` in `MassGenerator.EdgeWear.SelectionAndCorners.cs`; full audit remains the fallback.
5. Propagate counters and timings through existing diagnostics and the 33-case editor matrix; advance active contracts to `EW-C1A.3i`.
6. Update framework, recovery architecture, and code inventory with only accepted current ownership facts.
7. Run full-scope diff, structural C# checks, contract scans, line-ending/BOM checks, and ZIP/patch replay against the 353-file authoritative tree.

### Risks and controls

- **Stale viability reuse:** controlled by exact stable-identity, geometry, normal, width, locality, and affected-edge checks; uncertainty is a cache miss.
- **Mutable shared records:** cached evidence is copied into current records; current coverage/context objects are never shared across candidates.
- **Normalization mutation:** the normalized foundation is reused read-only; candidate transactions continue producing their own construction-face clones.
- **Decision drift:** guard, prepared identity, complete build, and emission code remain unchanged; new telemetry exposes replay use and full-audit fallback.
- **Performance regression:** no new full build, candidate retry, persistent cache, runtime callback, per-frame work, or player allocation is permitted.

### Implemented result

- One thread-local, search-scoped replay cache is created and disposed inside `GenerateCornerDamageFullCertificationSearch`.
- The first editor integration preflight stores its normalized micro-topology foundation; later ranked attempts reuse that same read-only result.
- Successful isolated viability is copied only for unaffected ordinary identities with exact endpoint/owner-normal pairing, length, dihedral, requested/minimum widths, artistic eligibility, and locality interval/limiting position/projection parity.
- Mandatory, affected, missing, changed, unsuccessful, or ambiguous records execute the original `AuditBoundedSingleEdgeBevel` path.
- Replay code and the normalization optimization are compiled only under `UNITY_EDITOR`; preprocessed player projections of `MassGenerator.cs`, `MassGenerator.EdgeWear.Orchestration.cs`, and `MassGenerator.EdgeWear.SelectionAndCorners.cs` are byte-for-byte equal to C1A.3h.
- Existing C1A.3g baseline fallback, C1A.3h endpoint guard, one-complete-build maximum, materialization, emission, and production ownership remain unchanged.
- Corner report, per-case CSV, matrix aggregate, suite summary, comprehensive projection, and sentinel contracts now expose C1A.3i replay evidence.
- Static validation covers all `209` C# files in editor and player preprocessor projections, `39/39` architecture contracts, exact ten-file scope, preserved line-ending/BOM/terminal-newline state, and byte-identical 353-file patch/ZIP replay.

## Visual-development sequence

```text
EW-V1A.2f  normalized deterministic scalar safety baseline [superseded by frozen V1A.3b]
EW-V1A.3b  dihedral-biased width plus S1 removal [accepted and frozen]
EW-S1      object-space bevel breakup [rejected and removed]
EW-C1A-RO2 pre-bevel ordering/ownership audit [complete]
EW-C1A.1  transactional pre-bevel corner cut and provenance proof [implemented; transaction accepted]
EW-C1A.1a one polygon, one render surface [accepted and frozen through EW-C1A.1a.8]
EW-C1A.2  cap-ring bevel integration and raw/integrated seed proof [complete]
EW-C1A.3  unified authoring workflow and first 33-case gate [implemented; Unity result 14/33]
EW-C1A.3a deterministic fully certified single-corner search [superseded by C1A.3b]
EW-C1A.3b bounded staged single-corner certification [superseded by C1A.3c]
EW-C1A.3c predictive complete preflight and one-final-build search [superseded by C1A.3e]
EW-C1A.3d validation-suite de-duplication and research scheduling [implemented; Unity scheduling accepted]
EW-C1A.3e authoritative integration plan and topology-baseline reuse [implemented; Unity result: identity parity fixed, performance failed]
EW-C1A.3f solve/materialization split [runtime rejected: valid ordinary preview status lost on corner failure]
EW-C1A.3g complete authoritative build and truthful ordinary-baseline fallback [architecture accepted; corner matrix 15/33]
EW-C1A.3h minimum-width foreign-plane endpoint-conflict preparation guard [functionally accepted; performance rejected]
EW-C1A.3i cached normalized foundation and guarded isolated-viability replay [accepted performance baseline]
EW-C1A.3j bounded two-band shared endpoint-junction recovery [runtime rejected: 0/104 prepared]
EW-C1A.3k bounded two/three-band endpoint-star half-space recovery [runtime rejected: 0/104 prepared]
EW-C1A.3l bounded local endpoint face-patch replacement [runtime rejected: 0/104 prepared; 102 locality, 2 extraction]
EW-C1A.3m exact cut-locality semantics [implemented; static validation complete; Unity pending]
EW-C2      sparse chips, notches, and break events
EW-N1      final artistic normal shaping across all accepted worn geometry
EW-F1      broad-face finish, cracks, and crevices
EW-F2      final material and rendering finish
```

The uniform R13A.9a baseline remains the mandatory zero-irregularity fallback throughout.

## EW-C0 — Reconciliation and topology readiness

### Code cleanup

- [x] Remove EW-B deterministic geometry entry point.
- [x] Remove independent face-offset and rail-reconciliation construction.
- [x] Remove generated open-edge ownership inference.
- [x] Remove source-vertex cap reconstruction experiments.
- [x] Remove isolated/two-edge/multi-star cap special cases.
- [x] Remove EW-B-only records, reject reasons, counters, and summary output.
- [x] Remove unused EW-B triangulation-preview machinery.
- [x] Preserve candidate discovery, source graph, candidate mapping, and generic topology audit.

### Healthy baseline

- [x] Edge-wear geometry emission is intentionally disabled.
- [x] Source `PolygonFace` geometry remains unchanged.
- [x] Edge-wear enablement cannot empty or corrupt the source mass.
- [x] Readiness output is separate from a geometry-failure message.

### Topology readiness

- [x] Build directed half-edges from source graph faces.
- [x] Link opposite half-edges.
- [x] Trace source boundary loops.
- [x] Count selected manifold, boundary, and non-manifold edges.
- [x] Report affected open and closed vertex fans.
- [ ] Validate exact ordered one-rings on representative seeds in Unity.
- [ ] Validate exact contiguous selected-run counts in EW-C1.

### EW-C0 exit criteria

- [x] Unity compiles without errors.
- [ ] Current source mass renders unchanged.
- [ ] Readiness log replaces the deterministic-kernel failure log.
- [ ] Current seed reports 16 source faces, 29 vertices, 44 edges, and 4 source boundary edges.
- [ ] Selected boundary edges are zero.
- [ ] Selected non-manifold edges are zero.
- [ ] Source non-manifold edges are zero.
- [ ] Source T-junctions are zero.
- [ ] Source boundary edges form traceable loop topology.
- [ ] Canonical documents contain no active EW-B instructions.

## EW-C1R3 — Compatible-edge deferral and face-corner/rail solver

- [x] Reuse the validated source graph and directed half-edge topology through a `ChamferTopologyContext`.
- [x] Solve one constant conservative initial width per selected source edge.
- [x] Iteratively reduce participating source-edge widths when acute corners exceed the displacement limit.
- [x] Feed failed unselected-edge common intervals back into the same monotonic solve.
- [x] Use a bounded binary search to find the largest stable shared-edge width scale.
- [x] Preserve pre-existing short unselected edges against their source length rather than a larger unrelated stability threshold.
- [x] Keep each reduced width constant across both endpoints of the full selected source edge.
- [x] Record convergence passes, clamp applications, clamped edge count, and the exact worst-corner identity.
- [x] Compute one point per `(source face, source vertex)` corner.
- [x] Preserve the source point when neither adjacent source edge is selected.
- [x] Solve selected/unselected offset-line intersections.
- [x] Solve selected/selected offset-line intersections.
- [x] Reconcile exact shared endpoints on unselected internal source edges.
- [x] Validate hypothetical replacement-face area, winding, and stable edge lengths.
- [x] Validate future selected-edge strip span and rail length.
- [x] Validate that source-boundary edges remain stable.
- [x] Emit no geometry and preserve the original rendered mass.

### Exit criteria

- [ ] Unity compiles without errors.
- [ ] `expectedCorners` equals `solvedCorners`.
- [ ] `cornerSolveFailures=0`.
- [ ] `nonFiniteCorners=0`.
- [ ] `cornerWidthConvergenceFailures=0`.
- [ ] `cornerWidthBelowMinimumFailures=0`.
- [ ] `sharedEdgeWidthConvergenceFailures=0`.
- [ ] `sharedEdgeWidthBelowMinimumFailures=0`.
- [ ] `excessiveDisplacementCorners=0`.
- [ ] `replacementFacesValid` equals `sourceFaces`.
- [ ] All replacement-face failure counters are zero.
- [ ] `sharedUnselectedEndpointFailures=0`.
- [ ] `selectedRailsValid` equals `selectedEdges`.
- [ ] All selected-rail failure counters are zero.
- [ ] `solvedBoundaryEdges` equals `sourceBoundaryEdges`.
- [ ] `readyForChamferEmission=1`.
- [ ] Final rendered geometry remains unchanged.

## EW-C2 — Provisional replacement faces and one-strip edge geometry

- [x] Reuse the validated EW-C1 corner and width solution without recomputation.
- [x] Build one temporary replacement polygon per source face.
- [x] Emit one temporary `ConvexEdgeWear` quad per active positive-width selected internal edge.
- [x] Preserve candidate strength and orient each strip from explicit candidate normal provenance.
- [x] Register solved source-boundary descendants explicitly.
- [x] Register active-strip endpoint boundaries explicitly for EW-C3.
- [x] Do not emit vertex-run corner patches.
- [x] Audit actual provisional openings by exact topology-key set membership.
- [x] Keep final geometry commit disabled.

### Exit criteria

- [ ] Unity compiles without errors.
- [ ] `replacementFacesBuilt` equals `sourceFaces`.
- [ ] `bevelStripsBuilt` equals `activeSelectedEdges`.
- [ ] `matchedSourceBoundaryEdges` equals `expectedSourceBoundaryEdges`.
- [ ] `matchedVertexBoundaryEdges` equals `expectedVertexBoundaryEdges`.
- [ ] `unexpectedProvisionalOpenEdges=0`.
- [ ] `missingExpectedVertexBoundaryEdges=0`.
- [ ] `provisionalNonManifoldEdges=0`.
- [ ] `provisionalTJunctions=0`.
- [ ] `readyForVertexPatches=1`.
- [ ] Rendered source geometry remains unchanged.


## EW-C2S3 — Raw-provenance segmentation and failure classification

- [x] Remove EW-C2R duplicate-boundary compatibility deferral.
- [x] Preserve the EW-C1R3 active positive-width edge network.
- [x] Stop mutating solved corners during inactive-edge reconciliation.
- [x] Build one immutable shared middle span per inactive internal source edge.
- [x] Split replacement-face edge chains around shared spans.
- [x] Register active strip endpoints as explicit vertex boundaries.
- [x] Register face-specific inactive-edge tails as explicit vertex boundaries.
- [x] Group vertex boundaries into source-vertex components.
- [x] Build shared spans and explicit strip/tail boundary provenance.
- [x] Normalize distinct-owner internal cancellations.
- [x] Keep same-owner and multi-owner boundary conflicts as hard failures.
- [x] Preserve replacement-face and bevel-strip provenance before topology audit.
- [x] Reconstruct provisional segment records with face kind, role, local edge, and source owner.
- [x] Move T-junction segmentation before ownership normalization.
- [x] Use graph-face one-ring or source-edge endpoint ownership instead of endpoint-only ownership.
- [x] Split every provisional use of a planned topology edge in identical parameter order.
- [x] Split matching expected vertex-boundary registrations while preserving provenance.
- [x] Update preserved source-boundary descendants when segmentation occurs.
- [x] Run segmentation to a bounded fixed point.
- [x] Print exact registration/use records for unresolved ownership groups.
- [x] Keep geometry commit disabled.
- [ ] Unity compiles without errors.
- [ ] Previously passing placed masses remain `readyForVertexPatches=1`.
- [ ] `tJunctionRecordsCompatible` and `provenanceCompatibleTJunctionSplits` become non-zero on previously failing T-junction cases.
- [ ] `tJunctionRecordsIncompatible=0`, or every non-zero record has exact diagnostic provenance.
- [ ] Validate exact source-boundary preservation.
- [ ] Validate zero missing and zero unexpected provisional openings.
- [ ] Validate zero same-owner and multi-owner boundary failures.
- [ ] Validate zero non-manifold edges and zero T-junctions.
- [ ] Require every representative placed mass to report `readyForVertexPatches=1` before EW-C3.

## EW-C2S4 — Preserved-boundary subdivision and compact diagnostics

- [x] Permit an existing raw-provenance provisional vertex to subdivide a segment explicitly classified as `PreservedSourceBoundary` without requiring containing-face one-ring membership.
- [x] Require at least one source-vertex owner and confirm the point is an actual provisional mesh vertex.
- [x] Retain stable-length, endpoint-distance, strict-interior parameter, and point-to-segment tolerance guards.
- [x] Split every provisional use of the containing topology edge consistently.
- [x] Replace every split source-boundary parent with its ordered child edge chain.
- [x] Add `preservedSourceBoundarySplits` and prevent preserved-boundary splits from incrementing `replacementOrdinaryEdgeSplits`.
- [x] Keep bounded fixed-point segmentation.
- [x] Count unique T-junction records rather than repeated pass encounters.
- [x] Suppress intermediate compatible/incompatible per-pair logs.
- [x] Emit at most one final topology warning containing at most three unique actionable records.
- [x] Keep geometry commit disabled.

### Exit criteria

- [x] Unity compiles and the EW-C2S4 audit runs.
- [x] The known test rock retains `activeSelectedEdges=33` and `deferredSelectedEdges=3`.
- [x] The known test rock reports `preservedSourceBoundarySplits=3`.
- [ ] The known test rock reports exact source-boundary descendant matching; observed EW-C2S4 result remains `expectedSourceBoundaryEdges=5`, `matchedSourceBoundaryEdges=3`.
- [x] `unexpectedProvisionalOpenEdges=0` on the known test rock.
- [x] `missingExpectedVertexBoundaryEdges=0` on the known test rock.
- [x] `provisionalNonManifoldEdges=0` on the known test rock.
- [x] `provisionalTJunctions=0` across all 24 placed objects.
- [ ] `readyForVertexPatches=1` on the known boundary rock.
- [x] Intermediate compatible/incompatible segmentation spam is removed.
- [ ] Every representative placed mass reports `readyForVertexPatches=1` before EW-C3 begins.


## EW-C2S5 — Face-local retrace normalization

- [x] Reduce exact cyclic `A -> B -> A` inverse-edge excursions using existing `VertexKey` identity.
- [x] Remove consecutive duplicate topology vertices without collinearity simplification.
- [x] Run the same reducer in hypothetical replacement-face validation.
- [x] Build replacement-face boundary registrations locally and publish only registrations backed by the reduced face walk.
- [x] Normalize initial bevel-strip walks before provisional emission.
- [x] Run a second normalization pass over replacement and bevel face records after raw T-junction segmentation.
- [x] Reject every remaining repeated undirected edge inside one provisional face.
- [x] Remove zero-use registrations only when their key was explicitly removed by exact retrace normalization.
- [x] Cancel registrations for an internally closed edge only when it has exactly two opposite-direction uses on two distinct face records.
- [x] Keep zero-use stale provenance, same-face duplicate uses, same-direction paired uses, and more-than-two uses as hard failures.
- [x] Add compact retrace, duplicate-edge, registration-reconciliation, and stale-provenance counters.
- [x] Keep candidate selection, width solving, active/deferred decisions, source-boundary descendant logic, vertex patches, and geometry commit unchanged.

### Exit criteria

- [x] Unity compiles without errors.
- [x] All eight EW-C2S4 non-manifold/multi-owner objects report `provisionalNonManifoldEdges=0`.
- [x] All placed objects report `vertexBoundaryMultiOwnerFailures=0`.
- [x] All placed objects report `faceLocalNormalizationFailures=0`, `faceLocalDuplicateEdgeFailures=0`, and `staleBoundaryRegistrationFailures=0`.
- [x] Previously passing objects remain `readyForVertexPatches=1`.
- [x] Active/deferred selected-edge counts and built bevel-strip counts remain unchanged per object.
- [x] `provisionalTJunctions=0` and `tJunctionRecordsIncompatible=0` remain true across the sample.
- [x] Only the three isolated preserved-source-boundary descendant mismatches remain blocked for EW-C2S6.
- [x] Geometry commit remains disabled.

## EW-C2S5R1 — Two-face internal boundary cancellation

- [x] Treat exactly two provisional uses on two distinct face records as an internally closed edge regardless of encoded direction.
- [x] Keep opposite-direction pairing as the expected orientation.
- [x] Count same-direction two-face pairs in non-blocking `sameDirectionClosedInternalEdges` diagnostics.
- [x] Keep zero-use stale provenance, two uses from one face record, more-than-two uses, face-local duplicate edges, non-manifold edges, and T-junctions as hard failures.
- [x] Keep candidate selection, width solving, active/deferred decisions, source-boundary descendant logic, vertex patches, and geometry commit unchanged.

### Exit criteria

- [x] Unity compiles without errors.
- [x] The five EW-C2S5 ownership-only blockers report `vertexBoundarySameOwnerDuplicateFailures=0` and `vertexBoundaryMultiOwnerFailures=0`.
- [x] Those five objects reach `readyForVertexPatches=1`.
- [x] All 24 objects retain `provisionalNonManifoldEdges=0`, `provisionalTJunctions=0`, `faceLocalDuplicateEdgeFailures=0`, and `staleBoundaryRegistrationFailures=0`.
- [x] Active/deferred selected-edge counts and built bevel-strip counts remain unchanged.
- [x] Only the three preserved-source-boundary descendant mismatches remain blocked for EW-C2S6.
- [x] Geometry commit remains disabled.

## EW-C2S6 — Explicit source-boundary descendant ownership

- [x] Build one ordered source-boundary record per original boundary half-edge.
- [x] Preserve source-edge identity, boundary-loop index/order, source endpoints, solved parent endpoints, and ordered child segments.
- [x] Apply raw split plans directly to matching child records in stable parameter order.
- [x] Count `preservedSourceBoundarySplits` from unique source-owned child subdivisions rather than provisional-face occurrences.
- [x] Derive provisional source-boundary segment-role lookup keys from the explicit record children.
- [x] Classify the first and last child of a subdivided source edge as terminal source-vertex transitions.
- [x] Classify terminal children as either one-use source-boundary openings or two-distinct-face source-vertex transitions.
- [x] Keep unsplit, non-terminal, and one-use terminal descendants in the expected open source-boundary set.
- [x] Require each expected open descendant to have exactly one use and no vertex-boundary ownership overlap.
- [x] Reject duplicate descendant keys, invalid terminal incidence, invalid open-child incidence, and source/vertex ownership overlap.
- [x] Add compact source-edge/loop/order/child diagnostics and summary counters.
- [x] Keep candidate selection, width solving, retrace normalization, T-junction segmentation, vertex patches, and geometry commit unchanged.

### Exit criteria

- [ ] Unity compiles without errors.
- [ ] All three previously blocked boundary-loop objects report `sourceBoundaryTerminalTransferFailures=0`, with terminal children classified only as open or transferred.
- [ ] All objects report `sourceBoundaryChildIncidenceFailures=0` and `sourceBoundaryDuplicateChildKeyFailures=0`.
- [ ] All objects report `expectedSourceBoundaryEdges=matchedSourceBoundaryEdges`.
- [ ] All 24 representative masses report `readyForVertexPatches=1`.
- [ ] `provisionalNonManifoldEdges=0`, `provisionalTJunctions=0`, and `tJunctionRecordsIncompatible=0` remain true.
- [ ] Geometry commit remains disabled.

## EW-C2S6R1 — Source-boundary loop retrace normalization

- [x] Group explicit source-boundary records by boundary-loop identity.
- [x] Order every loop by boundary order and child index.
- [x] Detect exact adjacent inverse children by existing `VertexKey` identity and equal `TopologyEdgeKey`.
- [x] Include the cyclic last/first loop seam.
- [x] Require exactly two provisional uses on two distinct face records.
- [x] Reject cancellation when the key has expected vertex-boundary ownership.
- [x] Repeat only while a strictly guarded inverse pair is removed.
- [x] Add raw, removed-pair, removed-child, normalized, and normalization-failure counters.
- [x] Keep invalid loop order and rejected inverse-pair guards as hard failures.
- [x] Keep candidate selection, width solving, face geometry, bevel strips, T-junction segmentation, vertex patches, and geometry commit unchanged.

### Exit criteria

- [x] Unity compiles without errors.
- [x] All observed regeneration summaries report `sourceBoundaryLoopNormalizationFailures=0`.
- [ ] The three previously blocked objects report guarded retrace removals.
- [x] All observed regeneration summaries report `sourceBoundaryChildIncidenceFailures=0`.
- [ ] All objects report `sourceBoundaryDuplicateChildKeyFailures=0`.
- [x] All observed regeneration summaries report `sourceBoundaryTerminalTransferFailures=0`.
- [x] All observed regeneration summaries report `expectedSourceBoundaryEdges=matchedSourceBoundaryEdges`.
- [ ] Candidate, active/deferred, replacement-face, and bevel-strip counts remain unchanged per object.
- [x] All observed regeneration summaries retain `provisionalNonManifoldEdges=0`, `provisionalTJunctions=0`, and `tJunctionRecordsIncompatible=0`.
- [ ] All 24 representative masses report `readyForVertexPatches=1`.
- [x] Geometry commit remains disabled.

## EW-C2S6R2 — Duplicate source-boundary pair provenance diagnostics

- [x] Snapshot every source-boundary child occurrence before R1 loop normalization.
- [x] Rebuild deterministic occurrence groups after normalization.
- [x] Preserve loop, boundary-order, child-index, source-edge, source-vertex, parent, and directed-endpoint provenance.
- [x] Report raw and normalized occurrence counts and cyclic pair metrics.
- [x] Classify same-direction, inverse-direction, and directionally incompatible pairs.
- [x] Report same-loop status, forward/reverse cyclic distance, and adjacency.
- [x] Report use count, distinct provisional-face count, and expected vertex ownership.
- [x] Report each surviving occurrence's terminal-transition status and predicted current-rule disposition.
- [x] Log the manually regenerated object's name and Unity entity ID from the inspector action.
- [x] Keep duplicate counters, ownership acceptance, readiness blockers, topology mutation, and geometry commit unchanged.

### Exit criteria

- [x] Unity compiles without errors.
- [ ] Manual regeneration emits one clickable object-context line before the audit triplet.
- [x] The 36-selected failing object still reports `sourceBoundaryDuplicateChildKeyFailures=1` and `readyForVertexPatches=0`.
- [x] One duplicate-group warning reports all raw and normalized occurrences for the repeated key.
- [x] The warning unambiguously reports direction relationship, loop relationship, cyclic distances, adjacency, and ownership disposition.
- [x] `sourceBoundaryDuplicateGroupDiagnosticsLogged=1` appears in the failing emission summary.
- [x] Candidate, width, corner, replacement-face, bevel-strip, normalization, and generic topology counters are unchanged.
- [x] Geometry remains provisional and commit-disabled.

## EW-C2S6R3 — Shared terminal-transfer alias collapse

- [x] Run alias normalization after R1 and before source-boundary ownership audit.
- [x] Require exactly two raw and two surviving occurrences for the repeated key.
- [x] Require exact inverse directed endpoints and terminal-transition status on both children.
- [x] Require different consecutive source-boundary records on the same loop with the corresponding shared source vertex.
- [x] Require the children to remain non-adjacent in the flattened loop walk.
- [x] Require exactly two provisional uses on two distinct face records and no expected vertex-boundary ownership.
- [x] Remove only the two source-boundary ownership claims; do not change provisional face geometry.
- [x] Report collapsed alias pairs, removed alias children, and alias-normalization failures.
- [x] Keep unexpected duplicate groups and rejected alias candidates blocked.
- [x] Keep candidate selection, width solving, face construction, strip construction, topology audits, and geometry commit unchanged.

### Exit criteria

- [x] Unity compiles without errors.
- [x] The 36-selected mass reports `sourceBoundaryTerminalAliasPairsCollapsed=1`.
- [x] The 36-selected mass reports `sourceBoundaryTerminalAliasChildrenRemoved=2`.
- [x] The 36-selected mass reports `sourceBoundaryTerminalAliasNormalizationFailures=0`.
- [x] `sourceBoundaryDescendants=3`, `expectedSourceBoundaryEdges=3`, and `matchedSourceBoundaryEdges=3`.
- [x] `sourceBoundaryDuplicateChildKeyFailures=0` and all source-boundary incidence/transfer failures remain zero.
- [x] `expectedVertexBoundaryEdges=72`, `matchedVertexBoundaryEdges=72`, and `provisionalOpenEdges=75`.
- [x] Candidate and construction counts remain `36/33/3`, `replacementFacesBuilt=16`, and `bevelStripsBuilt=33`.
- [x] Non-manifold edges, T-junctions, unexpected openings, and missing expected boundaries remain zero.
- [x] The 36-selected mass reports `readyForVertexPatches=1`.
- [x] All 24 physical masses report `readyForVertexPatches=1`.
- [x] Geometry remains provisional and commit-disabled.

## Full EW-C2 provisional topology gate

- [x] All 24 physical masses produce matching `OnValidate()` / `OnEnable()` emission summaries.
- [x] All 24 physical masses report `readyForChamferKernel=1`.
- [x] All 24 physical masses report `readyForChamferEmission=1`.
- [x] All 24 physical masses report `readyForVertexPatches=1`.
- [x] Replacement-face and bevel-strip failures are zero across the full set.
- [x] Source-boundary normalization, incidence, transfer, duplicate, and matching failures are zero across the full set.
- [x] Unexpected openings, missing expected vertex boundaries, non-manifold edges, and final T-junctions are zero across the full set.
- [x] Geometry remains provisional and commit-disabled.

## EW-C3A — Ordered source-vertex patch-component proof

- [x] Add `ChamferVertexPatchComponent` with source vertex, ordered records, ordered positions, closure state, and provenance.
- [x] Group final normalized `ChamferExpectedVertexBoundary` records by `SourceVertexIndex`.
- [x] Build exact `VertexKey` adjacency and reject duplicate keys or degree greater than two.
- [x] Order open chains from a deterministic degree-one endpoint.
- [x] Order closed loops from a deterministic minimum endpoint and provenance tie-break.
- [x] Orient every record continuously along the component walk.
- [x] Require every normalized boundary record to belong to exactly one component.
- [x] Record source-fan state, active-run count, active incident edges, and source-boundary records per component.
- [x] Classify `ClosedLoop`, `OpenChainSourceBoundaryResolved`, `OpenChainClosedSourceResolved`, or `OpenChainUnresolved`.
- [x] Require source-boundary chains to map both endpoints uniquely to surviving source-boundary ownership.
- [x] Require closed-source spoke keys to satisfy exact existing-use plus planned-use closure.
- [x] Count active source vertices with no surviving boundary component without treating them as failures.
- [x] Report expected and assigned boundary records plus independent component readiness.
- [x] Keep patch-face emission and geometry commit disabled.

### EW-C3A exit criteria

- [x] Unity compiles without errors.
- [x] `patchBoundaryRecords=patchBoundaryRecordsAssigned` and every normalized boundary record appears in exactly one ordered component.
- [x] `patchComponentOrderingFailures=0`.
- [x] `patchComponentProvenanceFailures=0`.
- [ ] `patchUnresolvedOpenChains=0` across all 24 physical masses.
- [x] Component branch and duplicate failures remain zero.
- [x] Existing EW-C2 candidate, width, replacement-face, bevel-strip, source-boundary, and topology counters remain unchanged.
- [ ] `readyForVertexPatches=1` remains true and `readyForVertexPatchComponents=1` across all 24 masses.
- [x] No patch faces are emitted and geometry commit remains disabled.

Validation note: component extraction and ordering pass across all 24 masses, but 20 open components on eight masses remain unresolved. Therefore `patchUnresolvedOpenChains=0` and full-set `readyForVertexPatchComponents=1` remain open.

## EW-C3A1 — Direct closure-edge census

- [x] Build one directed direct-closure claim from chain end to chain start for every open component.
- [x] Group claims by undirected `TopologyEdgeKey`.
- [x] Retain all claimants for a reported key while limiting counters and warnings to groups containing an unresolved component.
- [x] Record existing uses, distinct face records, segment roles, and segment direction.
- [x] Record planned uses and every claiming component's source vertex, index, source-fan state, chain size, closure class, and direction.
- [x] Detect strict existing-complement candidates.
- [x] Detect strict two-patch shared-connector candidates.
- [x] Enumerate incident surviving source-boundary children and endpoint relationships.
- [x] Detect diagnostic source-boundary replacement candidates without modifying ownership.
- [x] Report overused, underused, ownership-conflict, and unresolved direct-closure keys.
- [x] Add all eight direct-closure summary counters.
- [x] Leave EW-C3A closure classification and readiness unchanged.
- [x] Emit no patch faces and perform no source-boundary or mesh mutation.

### EW-C3A1 exit criteria

- [x] Unity compiles without errors.
- [x] All 24 physical masses retain the validated EW-C2 counters and `readyForVertexPatches=1`.
- [x] Every unresolved open component is represented by a direct-closure group warning.
- [x] Existing-complement census completed; zero qualifying candidates were found.
- [x] Shared-patch census completed; zero qualifying candidates were found.
- [x] Source-boundary diagnostics enumerate all incident surviving children and expose terminal/outer endpoint matching.
- [x] All sixteen unresolved keys were classified: eight overused, one underused, and seven ownership-conflicted.
- [x] No patch faces are emitted and geometry commit remains disabled.

## EW-C3A2 — Global patch-cluster stitching and boundary completion census

- [x] Collect provenance-valid unresolved closed-source component arcs.
- [x] Build exact endpoint adjacency across local source-vertex ownership.
- [x] Require degree two at every cluster endpoint.
- [x] Deterministically order each cluster from the smallest endpoint and stable component provenance.
- [x] Reverse local arc orientation only when required for continuous traversal.
- [x] Reject repeated expected vertex-boundary keys, component reuse, disconnected walks, and failure to close.
- [x] Materialize ordered cluster records without emitting faces.
- [x] Classify passing arcs as `OpenChainClosedSourceClusterResolved`.
- [x] Group unresolved source-fan components by original source-boundary loop.
- [x] Combine surviving source-boundary descendants with candidate component edges for diagnostics.
- [x] Report degree, connectivity, duplicate, use-count, and ownership evidence per loop.
- [x] Leave source-boundary ownership and children unchanged.
- [x] Keep patch geometry and final geometry commitment disabled.

### EW-C3A2 exit criteria

- [x] Unity compiles without errors.
- [x] All 24 masses retain `readyForVertexPatches=1` and all validated EW-C2 counters.
- [x] `patchClosedSourceClusters=6` and `patchClosedSourceClusterComponents=16` for the validated physical set.
- [x] `patchClosedSourceClusterFailures=0`.
- [x] The five previously failing closed-source masses now report `readyForVertexPatchComponents=1`.
- [x] Every remaining unresolved source-fan component appears in a boundary-completion census.
- [x] Boundary-completion diagnostics expose exact degree, connectivity, duplicate, one-use, and ownership status.
- [x] No patch faces or ownership transfers occur and geometry commit remains disabled.

## EW-C3A3 — Proven boundary promotion and multi-cycle lineage audit

- [x] Build derived final source-boundary and remaining vertex-patch ownership sets without mutating the validated source records.
- [x] Require one connected closed degree-two graph before promotion.
- [x] Require unique topology keys and one provisional use on every source and candidate edge.
- [x] Require exact disjoint source-boundary and vertex-boundary ownership before transfer.
- [x] Require every candidate component and candidate edge to be consumed exactly once.
- [x] Materialize deterministic `ChamferFinalSourceBoundaryLoop` records for passing completions.
- [x] Classify passing components `OpenChainSourceBoundaryCompletionResolved`.
- [x] Keep promoted components out of future patch-face emission.
- [x] Detect multiple disconnected closed cycles and refuse automatic promotion.
- [x] Deterministically order every derived cycle and report exact edge positions, use counts, and ownership.
- [x] Report source orders, source edges, candidate vertices, active runs, and active selected edges per cycle.
- [x] Preserve raw, post-R1, and post-R3 source-child counts plus exact removal reasons for lineage diagnostics.
- [x] Report consecutive source-order partitioning, removed-alias cross-cycle links, original provenance coverage, and cycle winding.
- [x] Keep patch geometry and final geometry commitment disabled.

### EW-C3A3 exit criteria

- [x] Unity compiles without errors.
- [x] All 24 masses retain `readyForVertexPatches=1` and all validated EW-C2 counters.
- [x] Physical aggregate reports `patchBoundaryCompletionTransfers=2`.
- [x] Physical aggregate reports two transferred components and two transferred edges.
- [x] `patchBoundaryCompletionTransferFailures=0`.
- [x] The two 18-selected masses report `patchUnresolvedOpenChains=0`.
- [x] 23 of 24 physical masses report `readyForVertexPatchComponents=1`.
- [x] The 36-selected mass reports one multi-cycle loop and two derived cycles.
- [x] `patchBoundaryCompletionCycleLineageFailures=0`.
- [x] The complete lineage warning identifies the record ranges, removed aliases, ownership, use counts, and winding of both cycles.
- [x] No patch faces are emitted and geometry commit remains disabled.

## EW-C3A4 — Multi-cycle boundary/patch ownership resolution

- [x] Require exactly one original source-boundary loop, two derived cycles, and two candidate components.
- [x] Require consecutive source-record partitions and a removed R1/R3 child connecting the cycles.
- [x] Require every candidate component to appear in exactly one cycle.
- [x] Require one-use incidence and correct disjoint derived ownership on every cycle edge.
- [x] Select exactly one source cycle with `windingDot >= 0.95`.
- [x] Require the residual cycle to have `abs(windingDot) <= 0.25` and at least `0.50` alignment separation.
- [x] Promote exactly one source-cycle candidate edge in derived ownership.
- [x] Demote exactly one residual-cycle source child in derived ownership.
- [x] Apply the swap to cloned sets and require source count, patch count, union, and disjointness invariants before commit.
- [x] Classify the source component `OpenChainSourceBoundaryMultiCycleResolved`.
- [x] Classify the residual component `OpenChainSourceBoundaryResidualPatchResolved`.
- [x] Preserve the residual full loop in `ChamferVertexPatchCluster.OrderedCompletionEdges`.
- [x] Keep original expected ownership, source children, provisional faces, and geometry commit unchanged.

### EW-C3A4 exit criteria

- [x] Unity compiles without errors.
- [x] All 24 masses retain `readyForVertexPatches=1` and all EW-C2 topology counters.
- [x] Physical aggregate reports `patchBoundaryMultiCycleResolutions=1`.
- [x] Physical aggregate reports one source cycle and one residual patch cycle.
- [x] Physical aggregate reports one promoted edge and one demoted edge.
- [x] Winding-selection, ownership-swap, and count-invariant failures are zero.
- [x] The 36-selected mass retains three derived final source-boundary edges and 72 remaining vertex-patch boundaries.
- [x] The 36-selected mass reports `patchUnresolvedOpenChains=0`.
- [x] All 24 physical masses report `readyForVertexPatchComponents=1`.
- [x] No patch faces are emitted and geometry commit remains disabled.

## EW-C3B1 — Provisional source-vertex patch emission and final topology audit

- [x] Materialize the validated component/cluster ownership result as a persistent audit-local patch plan.
- [x] Reconstruct the exact physical aggregate of 492 loops and 1503 patch-boundary edges.
- [x] Preserve `ConvexEdgeWear` identity, loop provenance, and the complete final topology audit.
- [x] Keep all emitted geometry provisional and commit-disabled.
- [x] Unity validation confirmed that the plan and ownership stages remain valid.
- [x] Unity validation rejected arithmetic-centre fans: 21 physical masses failed child-triangle area, two failed child-triangle winding, and one passed.
- [x] Retire centre-fan emission without changing the validated patch plan.

## EW-C3B1R1 — Boundary-only deterministic triangulation

- [x] Emit a three-edge loop directly as one triangle with no generated centre.
- [x] Project larger loops into a stable basis perpendicular to `ExpectedNormal`.
- [x] Reject unstable projected area and projected polygon self-intersection.
- [x] Select ears deterministically by original loop-position index and stable topology-key ordering.
- [x] Require every ear to be convex, empty of remaining projected vertices, and bounded by a non-intersecting diagonal.
- [x] Require every 3D triangle to have finite positions, stable area, compatible winding, and three unique topology edges.
- [x] Build every loop atomically and require exactly `boundaryCount - 2` triangles.
- [x] Require each original patch-boundary edge once and each internal diagonal twice within the loop triangulation.
- [x] Replace provisional `VertexPatchSpoke` classification with `VertexPatchDiagonal`.
- [x] Rebuild the complete provisional topology after patch insertion.
- [x] Require patch boundaries and patch diagonals to have exactly two total uses.
- [x] Require the final actual open-edge set to equal the derived final source-boundary set exactly.
- [x] Keep the complete geometry provisional and discarded after audit.

### EW-C3B1R1 Unity result

- [x] Unity compiled without errors.
- [x] The physical plan retained 492 attempted loops, 1503 consumed boundary edges, and 519 attempted triangles.
- [x] Direct-triangle emission eliminated all former patch winding failures.
- [x] Six physical masses completed the full provisional patch topology audit.
- [x] Thirteen physical masses exposed the patch-area threshold mismatch.
- [x] Five physical masses exposed expected-normal projection crossings.
- [x] All 24 retained EW-C3A ownership readiness and commit-disabled geometry.

### EW-C3B1R1 exit criteria

- [x] Unity compiles without errors.
- [x] The physical aggregate reports 492 attempted patch loops and 1503 consumed patch-boundary edges.
- [ ] The physical aggregate reports 519 attempted and built patch triangles.
- [ ] The physical aggregate reports 27 built internal patch diagonals.
- [ ] Attempted and built patch-loop counts match exactly.
- [ ] Patch construction, non-finite, area, winding, and duplicate-edge failures are zero.
- [ ] Projection, self-intersection, ear-selection, and diagonal-intersection failures are zero.
- [ ] Patch boundary-use and diagonal-use failures are zero.
- [ ] Final source-boundary-use failures and unexpected final open edges are zero.
- [ ] Final output open edges equal the derived final source-boundary set exactly.
- [ ] Final patch non-manifold edges and T-junctions are zero.
- [ ] All 24 physical masses report `readyForChamferPatchTopology=1`.
- [ ] All 24 retain `readyForVertexPatchComponents=1` and `readyForVertexPatches=1`.
- [ ] Geometry remains provisional and commit-disabled.

## EW-C3B1R2 — Patch-local area gate and complete non-triangle feasibility census

- [x] Pass `TinyFaceAreaEpsilon` only to provisional patch-triangle construction and ear validation.
- [x] Leave replacement-face and bevel-strip `minimumStableFaceArea` gates unchanged.
- [x] Use patch-local raw Newell/cross-product normals so tiny patch triangles do not inherit the generic polygon-normal fallback threshold.
- [x] Continue evaluating every patch loop after an individual loop failure.
- [x] Keep each failed loop atomic and append none of its provisional faces.
- [x] Add complete loop-failure, maximum-boundary-count, area-failure, and self-intersection counters.
- [x] Return structured first-crossing evidence with proper, endpoint-touch, and collinear-overlap classification.
- [x] Log loop kind, source vertices, ordered 3D positions, normals, alignment, projection scale, and non-planarity for projected crossings.
- [x] Exhaustively enumerate cyclic triangulations for every loop with four or more boundary positions.
- [x] Require every feasible candidate triangle to be finite, unique, above `TinyFaceAreaEpsilon`, and positively aligned.
- [x] Record total and feasible candidate counts plus one deterministic best diagonal set.
- [x] Keep feasibility selection diagnostic-only; do not replace active ear clipping in R2.
- [x] Keep all geometry provisional and final commitment disabled.

### EW-C3B1R2 exit criteria

- [x] Unity compiles without errors.
- [x] All 24 physical masses retain 492 attempted loops, 1503 consumed patch-boundary edges, and 519 attempted triangles in aggregate.
- [x] All previously observed patch-area blockers disappear.
- [x] `patchLoopsFailed` reports the complete set of failing loops rather than one first failure per mass.
- [x] `patchMaximumBoundaryCount` captures the largest physical patch loop.
- [x] Every non-triangle loop contributes to the feasibility counters.
- [x] Every projected crossing warning contains full structured evidence.
- [x] The feasibility census proves whether each crossing loop has at least one valid cyclic 3D triangulation.
- [x] All 24 retain `readyForVertexPatchComponents=1` and `readyForVertexPatches=1`.
- [x] Geometry remains provisional and commit-disabled.

Validated R2 result: 17/24 physical masses reached the complete patch-topology gate. All 492 loops were audited; 484 built, eight failed, all patch-area and winding counters were zero, two non-triangle local loops had feasible cyclic triangulations and passed, and the remaining two folded local loops plus six closed-source clusters had zero feasible cyclic triangulations.

## EW-C3B1R3 — Source-local patch cell-complex feasibility census

- [x] Audit only non-triangle loops whose exhaustive cyclic feasibility result is empty.
- [x] Preserve successful direct-triangle and ear-clipped provisional emission unchanged.
- [x] Derive a component-local expected normal from each component's represented source faces.
- [x] Plan one source-vertex fan triangle for every component boundary edge.
- [x] Detect shared endpoints between consecutive cluster components.
- [x] Plan endpoint bridge triangles when adjacent components have different source vertices.
- [x] Treat coincident adjacent source vertices as directly closing the matching endpoint spokes.
- [x] Build the combined source-to-source central graph from bridge edges and existing topology uses.
- [x] Require one-use central edges to form deterministic degree-two closed loops.
- [x] Run a read-only exhaustive triangulation census for each central source-vertex loop.
- [x] Audit combined existing-plus-planned use counts for every boundary, spoke, bridge, central boundary, and central diagonal.
- [x] Reject planned overlap with final source-boundary ownership.
- [x] Audit prospective T-junctions against the existing provisional geometry.
- [x] Log component counts, source vertices, fan/bridge/central triangles, complete edge incidence, geometry minima, failure classes, and `feasibleCellComplex`.
- [x] Keep the cell-complex path diagnostic-only and append no prospective faces.
- [x] Keep final geometry commitment disabled.

### EW-C3B1R3 exit criteria

- [x] Unity compiles without errors.
- [x] Exactly eight physical cell-complex census entries are produced.
- [x] `patchCellComplexesAudited=8` across the 24 physical masses.
- [x] The audited population is exactly two local folded cells and six closed-source cluster cell complexes.
- [x] Every cell-complex entry reports all component boundary counts and planned edge incidences.
- [x] Component-local geometry and bridge feasibility are established for every audited loop.
- [x] Every one-use central graph is either closed directly or resolved into simple central loops.
- [x] Every central loop records a complete read-only triangulation result.
- [x] Prospective incidence and T-junction counters identify whether each alternative surface is safe.
- [x] No source-boundary ownership, replacement face, bevel strip, or committed geometry changes occur.
- [x] All 24 retain `readyForVertexPatchComponents=1` and `readyForVertexPatches=1`.
- [x] Geometry remains provisional and commit-disabled.


Validated R3 result: exactly eight physical cell complexes were audited—two local folded loops and six closed-source clusters. All eight were infeasible. The census planned 49 component-fan triangles, 16 endpoint bridges, and five central source edges, found no central loops, and reported 22 component failures, seven bridge failures, one central-graph failure, 29 geometry failures, 46 incidence failures, and 20 prospective T-junctions. The source-vertex cell-complex model is rejected.

## EW-C3B1R4 — Directed-manifold boundary triangulation census

- [x] Audit only the eight loops with no feasible aggregate-normal cyclic triangulation.
- [x] Recover the unique pre-patch face occurrence and direction of every folded-loop boundary edge.
- [x] Require one coherent loop orientation that reverses every owning face boundary edge.
- [x] Enumerate every cyclic triangulation without expected-normal projection or aggregate-normal rejection.
- [x] Require directed outer-edge incidence and opposite directed diagonal incidence.
- [x] Require combined existing-plus-candidate use count two for every candidate edge.
- [x] Reject candidate overlap with final source-boundary ownership.
- [x] Validate candidate triangles using `TinyFaceAreaEpsilon` and raw 3D normals.
- [x] Reject improper candidate-candidate triangle intersections in actual 3D.
- [x] Reject improper candidate-existing-face intersections beyond shared topology.
- [x] Audit combined T-junction and non-manifold results.
- [x] Rank candidates by quality, internal dihedral, boundary dihedral, area, and stable diagonal order.
- [x] Keep the directed candidate diagnostic-only and append no candidate faces.
- [x] Keep final geometry commitment disabled.

### EW-C3B1R4 exit criteria

- [x] Unity compiles without errors.
- [x] Exactly eight physical directed-manifold census entries were produced.
- [x] `patchDirectedLoopsAudited=8` and `patchDirectedBoundaryEdgesChecked=49` across the physical set.
- [x] Six closed-source clusters were classified as directed-boundary conflicts before candidate enumeration.
- [x] The two coherent local quads produced four candidates total.
- [x] All four candidates passed incidence and candidate-candidate intersection.
- [x] All four candidates failed existing-face intersection; no directed triangulation was feasible.
- [x] The 484 successful R2 loops remained unchanged.
- [x] No candidate face was emitted and geometry remained commit-disabled.

Validated R4 result: the remaining blockers are boundary representation and sanitation, not triangle selection. The six position-key clusters are not authoritative face-sector boundary components. The two coherent local quads are sub-resolution sliver boundaries under `PointMergeDistance`.

## EW-C3B1R5 — Authoritative half-edge decomposition and sliver-normalization census

- [x] Deep-clone the pre-patch replacement/bevel face records before successful patch faces are appended.
- [x] Preserve face kind, source-face, source-edge, exact vertices, and face normal in the diagnostic snapshot.
- [x] Build one directed half-edge per snapshot face corner with face-local next/previous indices.
- [x] Link only exact two-use opposite-direction twins.
- [x] Mark one-use edges as authoritative boundary half-edges.
- [x] Find each boundary successor by rotating through actual face adjacency and exact twins.
- [x] Trace oriented loops, open chains, positional pinch keys, successor failures, and assignment failures.
- [x] Map exactly the six current `ClosedSourceCluster` plans to authoritative component IDs without mutating the plan.
- [x] Classify exact, split, combined, shared, open, ambiguous, and missing-edge cases.
- [x] Report exact face-record/corner and source provenance for every proposed corrected component.
- [x] Select sliver loops dynamically from coherent local quads whose directed candidates fail only existing-face intersection and contain an edge at or below `PointMergeDistance`.
- [x] Mirror the complete relevant `SanitizePolygon(...)` removal order on tracked copies.
- [x] Reject inconsistent incident-face survivor decisions instead of choosing an arbitrary representative.
- [x] Apply a consistent virtual key remap across cloned replacement/bevel faces only.
- [x] Re-sanitize and validate cloned faces, rebuild segment records and edge uses, and rerun non-manifold/T-junction auditing.
- [x] Recover the actual post-collapse boundary from the cloned authoritative half-edge graph.
- [x] Test a resulting triangle read-only with directed incidence and existing-surface/topology checks.
- [x] Append no R5 candidate face, mutate no live patch plan, and keep geometry commitment disabled.

### EW-C3B1R5 exit criteria

- [x] Unity compiled without errors.
- [x] Exactly six physical cluster plans covering 41 boundary edges were audited.
- [x] Every authoritative boundary half-edge was assigned once or had an explicit successor failure.
- [x] Eight co-directed two-use edges were isolated as the cause of 16 successor failures and 16 open chains.
- [x] No cluster edge was missing, but no corrected closed partition was proven.
- [x] Four microscopic local quads were audited, revealing that two already-successful loops were incorrectly included.
- [x] Every audited sliver chose a deterministic survivor with zero representative conflicts and zero affected-face failures.
- [x] The cloned sliver audit was proven non-authoritative because it did not rerun the real provisional segmentation path before topology evaluation.
- [x] The 484 successful R2 loops remained unchanged.
- [x] All 24 retained `readyForVertexPatchComponents=1` and `readyForVertexPatches=1`.
- [x] Geometry remained provisional and commit-disabled.

Validated R5 result: the cluster blocker is eight unresolved co-directed two-use edges, and the sliver blocker requires strict failed-loop targeting plus a full cloned provisional rebuild.

## EW-C3B1R5R1 — Co-directed twin classification and targeted sliver re-audit

- [x] Record every co-directed pair with exact key, directed uses, face-record/corner identity, face kind, source provenance, and face-local neighbours.
- [x] Report stored/calculated face normals and source-topology adjacency for both uses.
- [x] Classify radial face-sector relationship without using position identity as topology ownership.
- [x] Build a read-only face-orientation parity graph from every two-use edge.
- [x] Report parity contradictions and the exact cloned faces requiring reversal.
- [x] Reject parity solutions whose reversed faces disagree with authoritative stored normals.
- [x] Rebuild twins and cluster components under the orientation-parity hypothesis.
- [x] Rebuild the same topology while treating co-directed uses as independent boundary sectors.
- [x] Require complete successor, assignment, internal-direction, and exclusive cluster-component gates for either hypothesis.
- [x] Select a hypothesis only when exactly one interpretation passes; otherwise report `Unresolved`.
- [x] Carry sliver eligibility from the actual loop-emission path.
- [x] Audit only sliver-signature local quads whose real R2 triangulation failed.
- [x] Count already-successful sliver-like quads as excluded.
- [x] Reapply deterministic sanitizer survivor selection to cloned replacement/bevel faces.
- [x] Rerun compatible T-junction segmentation on the cloned topology.
- [x] Rerun face-walk, vertex-boundary, source-boundary loop, terminal-alias, and ownership normalization.
- [x] Recover the post-collapse component using the selected half-edge hypothesis.
- [x] Evaluate a resulting triangle only after the full cloned rebuild.
- [x] Mutate no live face, patch plan, ownership record, or committed geometry.

### EW-C3B1R5R1 exit criteria

- [x] Unity compiled without errors.
- [x] Physical aggregate `patchCoDirectedUsePairsAudited=8`.
- [x] Every co-directed pair reported complete use, provenance, normal, source-adjacency, and sector evidence.
- [x] `patchCoDirectedPairsUnclassified=0`.
- [x] Orientation parity was rejected by nine contradictions, six required reversals, and six normal failures.
- [x] Independent sectors produced 108 closed loops for 108 existing plans with zero open chains, successor failures, or internal-direction failures.
- [x] The remaining `Unresolved` label was traced to the obsolete exact-cluster-match gate rather than a sector-topology failure.
- [x] Physical aggregate `patchSliverLoopsAudited=2`.
- [x] Physical aggregate `patchSliverSuccessfulLoopsExcluded=2`.
- [x] Both virtual collapses had zero representative and affected-face failures.
- [x] `patchSliverPostSegmentationIncompatibleTJunctions=0`.
- [x] Both failed sliver loops resolved to authoritative triangles.
- [x] The 484 successful R2 loops remained unchanged.
- [x] All 24 retained `readyForVertexPatchComponents=1` and `readyForVertexPatches=1`.
- [x] Geometry remained provisional and commit-disabled.

Validated R5R1 result: orientation parity is retired. Independent face sectors are the authoritative boundary representation, but all plans must be remapped globally before any live correction.

## EW-C3B1R5R2 — Authoritative sector-loop repartition and corrected full-topology census

- [x] Build a corrected pre-patch clone for every mass.
- [x] Apply only the two validated sliver collapses before corrected decomposition.
- [x] Rerun the full provisional segmentation and boundary-normalization path after a collapse.
- [x] Build the independent-sector half-edge decomposition from the corrected snapshot.
- [x] Separate preserved source-boundary components from patch-hole components.
- [x] Compare every existing plan against every authoritative sector loop.
- [x] Audit every authoritative face-corner occurrence for exactly one legacy provenance owner.
- [x] Report plans split across sector loops and sector loops combining multiple plans.
- [x] Preserve exact matches for lineage and map remaining plans deterministically by occurrence and key overlap.
- [x] Construct corrected patch loops from opposite authoritative half-edge order without depending on lineage.
- [x] Triangulate every corrected loop with patch-local area policy.
- [x] Audit candidate-candidate and candidate-existing-face intersections.
- [x] Append corrected faces only to cloned provisional records.
- [x] Audit unexpected openings, source-boundary preservation, non-manifold edges, and T-junctions on the complete clone.
- [x] Report occurrence-level sector ownership separately from position-key topology.
- [x] Keep live plans, live geometry, and geometry commitment unchanged.

### EW-C3B1R5R2 exit criteria

- [x] Unity compiles without errors.
- [x] Physical aggregate `patchSectorMassesAudited=5`.
- [ ] Physical aggregate `patchSectorExistingPlanLoops=108`.
- [ ] Physical aggregate `patchSectorAuthoritativeLoops=108`.
- [ ] All sector boundary half-edges are assigned exactly once.
- [ ] `patchSectorLoopCountInvariantFailures=0`.
- [ ] `patchSectorOwnershipInvariantFailures=0`.
- [ ] `patchSectorProvenanceFailures=0`.
- [ ] Physical aggregate `patchCorrectedMassesAudited=24`.
- [ ] Physical aggregate `patchCorrectedLoopsAttempted=492`.
- [ ] Physical aggregate `patchCorrectedSliverCollapses=2`.
- [ ] Physical aggregate `patchCorrectedLoopsBuilt=492`.
- [ ] `patchCorrectedLoopsFailed=0`.
- [ ] `patchCorrectedCloneTopologyFailures=0`.
- [ ] `patchCorrectedFinalUnexpectedOpenEdges=0`.
- [ ] `patchCorrectedFinalSourceBoundaryFailures=0`.
- [ ] `patchCorrectedFinalNonManifoldEdges=0`.
- [ ] `patchCorrectedFinalTJunctions=0`.
- [ ] All 24 report `readyForCorrectedChamferPatchTopology=1`.
- [ ] The live R2 result remains 484 built / 8 failed and geometry remains commit-disabled.


Validated R5R2 result: the 108-plan / 108-sector count remained stable, but the full corrected clone was not authoritative. The new intersection gate rejected validated baseline geometry; sixteen co-directed sector occurrences were incorrectly counted as missing legacy provenance; one proven sliver was globally merged; and direct-triangle winding still depended on an unstable aggregate normal. No live topology was changed.

## EW-C3B1R5R3 — Intersection attribution, promoted-sector ownership, and reserved-sliver integration

- [x] Carry all successful live vertex-patch records into a clone-only baseline intersection control.
- [x] Attribute candidate-internal, accepted-patch, replacement-face, and bevel-strip intersections separately.
- [x] Compare the former vertex-zero fan face test against deterministic polygon-aware face triangulation.
- [x] Count fan-only face hits without treating them as authoritative rejection.
- [x] Log exact candidate, face, provenance, shared-key, coplanar, and fan/polygon evidence.
- [x] Recognize both exact uses of every co-directed pair as promoted sector-boundary occurrences when no legacy owner exists.
- [x] Keep promoted occurrences face-corner-specific rather than assigning invented legacy plan ownership.
- [x] Reserve the two proven sliver triangles before global sector traversal.
- [x] Include reserved triangles in the half-edge decomposition so their boundary edges close before remaining loops are traced.
- [x] Build three-edge patch triangles directly opposite authoritative boundary occurrences.
- [x] Keep larger-loop triangulation on deterministic projected ear clipping.
- [x] Keep live plans, live provisional records, final mesh, and geometry commitment unchanged.
- [x] Add a concise canonical method-attempt ledger to prevent repeated rejected approaches.

### EW-C3B1R5R3 validated result

- [x] Unity compiled and the 24 physical masses produced deterministic duplicate pairs.
- [x] Physical aggregate `patchCorrectedBaselineLoopsAudited=484`.
- [x] Baseline categories reconciled; 185/484 loops were rejected, dominated by replacement-face contacts.
- [x] Fan-only and polygon-aware evidence were separated.
- [x] Physical aggregate `patchSectorLegacyOwnedBoundaryHalfEdges=347`.
- [x] Physical aggregate `patchSectorPromotedBoundaryHalfEdges=16`.
- [x] `patchSectorBoundaryHalfEdgesUnassigned=0`.
- [x] `patchSectorBoundaryHalfEdgesMultiAssigned=0`.
- [x] Physical aggregate `patchCorrectedReservedSliverLoops=2`.
- [x] Physical aggregate `patchCorrectedReservedSliverTriangles=2`.
- [x] `patchCorrectedReservedSliverOccurrenceConflicts=0`.
- [ ] Baseline contact calibration accepted; exact-key contact is rejected because it produced 185 false-or-unproven baseline rejections.
- [ ] Both reserved sliver masses preserve their legacy loop count; one remained `22 → 21`.
- [x] Geometry remained provisional and commit-disabled.

## EW-C3B1R5R4 — Boundary-aware contact and sliver-count reconciliation

- [x] Build outer patch-boundary segments by cancelling internal patch-triangle diagonals.
- [x] Build sanitized replacement/bevel face-boundary segments independently of triangle keys.
- [x] Permit contact only when every detected contact lies on both authoritative boundary sets.
- [x] Continue rejecting interior penetration, proper coplanar crossings, candidate-internal overlap, and accepted-patch interior overlap.
- [x] Replace verbose generic face triangulation with silent deterministic projected ear clipping.
- [x] Use the sanitized render fan as the authoritative face-intersection representation and retain silent polygon triangulation as comparison evidence.
- [x] Reuse the R5R1 post-collapse three-edge boundary for reserved slivers.
- [x] Match reserved face occurrences by exact opposite direction rather than broad segment descent.
- [x] Cap detailed intersection samples to one total for baseline mode and one for corrected mode per physical evaluation.
- [x] Suppress detailed co-directed and authoritative-component dumps unless temporary verbose diagnostics are enabled.
- [x] Remove full assignment matrices from ordinary sector summaries.
- [x] Keep live plans, production provisional records, final geometry, and geometry commitment unchanged.

### EW-C3B1R5R4 validated result

- [x] Unity compiled without errors; three `CS0162` warnings came only from the compile-time false verbose switch.
- [x] Physical aggregate `patchCorrectedBaselineLoopsAudited=484`.
- [x] Boundary-aware contact recovered 61 baseline loops; 124 remained rejected.
- [x] Remaining blocking events were dominated by 121 replacement-face and four bevel-strip intersections.
- [x] 123/125 blocking events were reproduced by the independent polygon comparison.
- [x] Sector-bearing aggregate remained `108 plans = 108 authoritative loops`.
- [x] Sector ownership remained `347 legacy + 16 promoted = 363 authoritative`.
- [x] Both reserved sliver triangles were present with zero occurrence conflicts.
- [x] The first sliver count remained `20 → 20`.
- [ ] The second sliver count remained `22 → 21`.
- [ ] Six corrected sector loops retained boundary-occurrence failures.
- [x] Geometry remained provisional and commit-disabled.

## EW-C3B1R5R5 — Overlap ownership, occurrence causes, and sliver component delta

- [x] Classify each rejected baseline loop once as patch-contained, replacement-contained, partial coplanar overlap, non-coplanar penetration, bevel penetration, or unclassified.
- [x] Compute deterministic projected overlap area for coplanar render-faithful triangle sets.
- [x] Distinguish overlaps with an authoritative boundary-owner face from overlaps with unrelated faces.
- [x] Split corrected boundary-occurrence failures into missing opposite, duplicate opposite, direction mismatch, and extra patch-boundary edge.
- [x] Compare pre-collapse and post-collapse sliver components after excluding the locally reserved sliver component.
- [x] Apply the validated removed-to-representative vertex remap before component comparison.
- [x] Count exact component matches, disappeared components, post-collapse merges, pre-collapse splits, loop-count deficit, and a compact component trace.
- [x] Replace compile-time verbose constant with a non-constant read-only gate to remove `CS0162` warnings.
- [x] Gate detailed intersection, sector, sliver, half-edge, and per-loop failure evidence behind verbose diagnostics.
- [x] Emit one compact no-stacktrace recovery summary and suppress only adjacent identical `OnValidate`/`OnEnable` lifecycle duplicates.
- [x] Keep live plans, replacement faces, bevel strips, final geometry, and geometry commitment unchanged.

### EW-C3B1R5R5 exit criteria

- [ ] Unity compiles without errors or `CS0162` warnings.
- [ ] Physical aggregate overlap classifications equal the 124 R5R4 rejected baseline loops.
- [ ] `patchOverlapUnclassified=0`, or every unclassified loop is isolated with a concrete reason.
- [ ] Boundary-owner and non-owner overlap populations reconcile with the classified-loop total.
- [ ] Projected overlap area is finite and deterministic across duplicate evaluations.
- [ ] The six occurrence failures reconcile exactly across missing, duplicate, direction-mismatch, and extra-edge counters.
- [ ] Both sliver masses emit deterministic pre/post component-delta summaries.
- [ ] The remaining `22 → 21` deficit is explained by an exact disappeared, merged, or split component population.
- [ ] Default Console output contains one compact no-stacktrace summary per physical evaluation, suppresses adjacent identical `OnValidate`/`OnEnable` duplicates, and produces no detailed flood.
- [ ] Geometry remains provisional and commit-disabled.


## MG-R1 — Behaviour-preserving partial-class extraction

- [x] Change `MassGenerator` to `public static partial class MassGenerator`.
- [x] Keep the public `Generate` entry points and orchestration in `MassGenerator.cs`.
- [x] Extract plane-cut construction, polyhedron utilities, radial construction, mesh output, geodesic topology, helpers, and core types into focused partial files.
- [x] Extract edge-wear selection/corners, boundary planning, boundary completion, normalization, patch construction, half-edge diagnostics, corrected topology, sliver/triangulation, graph utilities, and edge-wear types into focused partial files.
- [x] Preserve all 358 detected top-level method declarations exactly once.
- [x] Preserve all 103 detected nested type declarations exactly once.
- [x] Parse every extracted C# file with zero syntax errors or missing nodes.
- [x] Preserve CRLF and avoid `.meta` changes.
- [x] Keep live topology, rendered geometry, and geometry commitment unchanged.

### MG-R1 Unity exit criteria

- [x] Unity compiles without errors or new warnings.
- [x] All 24 physical masses regenerate.
- [x] Compact R5R5 summaries match the pre-refactor baseline per mass.
- [ ] Rendered mesh appearance is unchanged.
- [x] Existing live readiness remains unchanged.
- [x] Corrected-clone diagnostics remain unchanged.
- [x] `geometryCommit=disabled` remains present for every mass.
- [x] No duplicate-type, missing-member, or partial-class accessibility error appears.
- [x] Only after all parity checks pass may superseded code removal begin.


## MG-R2 — Diagnostic quarantine and first deletion wave

- [x] Add `Generated_Mass_Edge_Wear_Code_Inventory.md` as the canonical symbol inventory.
- [x] Remove historical detailed log methods and call sites.
- [x] Remove per-intersection evidence payload, category, log budget, and limit constant.
- [x] Remove all methods proven unreferenced after diagnostic quarantine.
- [x] Remove dead detailed-message construction left inside active topology methods.
- [x] Preserve candidate selection, corner solving, replacement faces, bevel strips, patch plans, sector decomposition, sliver normalization, overlap classification, and compact counters.
- [x] Reduce all `MassGenerator` partials from 28,982 to 26,546 lines.
- [x] Preserve CRLF and avoid `.meta` changes.
- [x] Parse every changed C# file with zero syntax errors or missing nodes.

### MG-R2 Unity exit criteria

- [x] Unity compiles without errors or new warnings.
- [x] All 24 physical masses regenerate.
- [x] Every compact audit is byte-for-byte identical to the MG-R1 baseline.
- [x] Historical direct-closure, source-boundary completion, half-edge, triangulation, sector, sliver, and intersection evidence logs are absent during ordinary regeneration.
- [ ] Rendered mesh appearance remains unchanged.
- [x] `geometryCommit=disabled` remains present for every mass.
- [x] Only after parity passes may MG-R3 remove rejected hypothesis calculations and their counters/types.

## MG-R2R1 — Orphaned half-edge classification cleanup

- [x] Remove the unused local classification value and assignments.
- [x] Remove the now-unreferenced classification enum.
- [x] Preserve all branch conditions and retained counter updates.
- [x] Unity compiles with zero warnings.
- [x] All 24 compact audits remain unchanged.
- [x] `geometryCommit=disabled` remains present.

## MG-R3 — Superseded feasibility subsystem removal

- [x] Prove cell-complex feasibility writes only cell-only counters and has no topology, blocker, or compact consumer.
- [x] Remove cell-complex audit methods, exclusive helpers, types, and counters.
- [x] Prove historical directed-manifold feasibility writes only audit counters.
- [x] Remove the directed-manifold audit and comparison-only helper while retaining directed utilities used by sliver recovery.
- [x] Remove rejected orientation-parity propagation, simulated face reversal, parity data, and counters.
- [x] Replace hypothesis selection with the retained independent-boundary-sector acceptance decision.
- [x] Preserve promoted co-directed half-edge identification used by corrected sector ownership.
- [x] Preserve overlap ownership, sector ownership, sliver lineage, compact-audit fields, and commitment state.
- [x] Reduce all `MassGenerator` partials from 26,510 to 24,911 lines.
- [x] Parse every changed C# file with zero syntax errors or missing nodes.
- [x] Preserve CRLF and avoid `.meta` changes.

## MG-R3R1 — Stale corrected-topology call-site compile fix

- [x] Replace the obsolete `null` hypothesis argument with `false` for `useIndependentBoundarySectors`.
- [x] Preserve the former `null` semantics: unresolved/default decomposition, not independent-sector promotion.
- [x] Change no counters, topology records, compact fields, or geometry commitment state.
- [x] Unity compiles with zero errors and zero warnings.
- [x] All 24 compact audits remain identical to MG-R2R1.

### MG-R3 Unity exit criteria

- [x] Unity compiles with zero errors and zero warnings.
- [x] All 24 physical masses regenerate.
- [x] Every compact audit is byte-for-byte identical to MG-R2R1.
- [ ] Rendered mesh appearance remains unchanged.
- [x] `geometryCommit=disabled` remains present for every mass.
- [x] No removed parity, cell-complex, or directed-manifold symbol appears in compiler output or runtime logs.

## MG-R4 — Obsolete counter and result-type reduction

- [x] Build a field-level producer/consumer inventory for all three edge-wear stats structures.
- [x] Remove only fields with no compact, readiness, topology, ownership, sliver, blocker, or commitment consumer.
- [x] Preserve the three mutating `ReduceChamferFaceRetraces(...)` calls after deleting their dead counters.
- [x] Remove three uncalled `ToSummaryString()` methods.
- [x] Remove methods and result types newly orphaned by counter deletion.
- [x] Reduce `ChamferEmissionStats` from 286 to 95 fields.
- [x] Reduce `ChamferCornerStats` from 60 to 16 fields.
- [x] Reduce `ChamferReadinessStats` from 28 to 13 fields.
- [x] Reduce all `MassGenerator` partials from 24,911 to 22,480 lines.
- [x] Parse every changed C# file with zero syntax errors or missing nodes.
- [x] Preserve CRLF and avoid `.meta` changes.

### MG-R4 Unity exit criteria

- [x] Unity compiles with zero errors and zero warnings.
- [x] All 24 physical masses regenerate.
- [x] Every compact audit is byte-for-byte identical to MG-R3R1.
- [ ] Rendered mesh appearance remains unchanged.
- [x] `geometryCommit=disabled` remains present for every mass.
- [x] No removed counter, summary, direct-closure census, active-run audit, or preliminary half-edge audit appears in compiler output or runtime logs.


## MG-R5 — Production-candidate and diagnostic-harness separation

- [x] Move the public edge-wear orchestration methods into `MassGenerator.EdgeWear.Orchestration.cs`.
- [x] Add one explicit `ChamferBuildArtifacts` builder result boundary.
- [x] Remove the corrected-clone call from `TryEmitAndAuditChamferVertexPatches`.
- [x] Invoke the clone-only harness from orchestration after the builder returns its artifacts.
- [x] Move corrected-clone and sector census methods into `MassGenerator.EdgeWear.Diagnostics.CorrectedClone.cs`.
- [x] Move overlap/intersection methods into `MassGenerator.EdgeWear.Diagnostics.Overlap.cs`.
- [x] Move compact logging into `MassGenerator.EdgeWear.Diagnostics.Logging.cs`.
- [x] Move diagnostic-only result types into `MassGenerator.EdgeWear.Diagnostics.Types.cs`.
- [x] Prove production-candidate files contain zero references to diagnostic-harness methods or diagnostic-only types.
- [x] Preserve every MG-R4 method and nested type; add only the builder result and diagnostic wrapper.
- [x] Parse every `MassGenerator` C# partial with zero syntax errors or missing nodes.
- [x] Preserve CRLF and avoid `.meta` changes.

### MG-R5 Unity exit criteria

- [x] Unity compiles with zero errors and zero warnings.
- [x] All 24 physical masses regenerate.
- [x] Every compact audit is byte-for-byte identical to MG-R4.
- [ ] Rendered mesh appearance remains unchanged.
- [x] `geometryCommit=disabled` remains present for every mass.
- [x] No production-candidate file requires a diagnostic-harness method or diagnostic-only type.

## MG-R6A — Contained-patch ownership-transfer feasibility

- [x] Move render-faithful overlap predicates and face/patch geometry helpers into a production/shared partial.
- [x] Preserve baseline overlap classification through the same shared classifier.
- [x] Build contained candidates from successful production patch records without reading diagnostic counters.
- [x] Require `PatchContainedInReplacement` classification.
- [x] Record deterministic containing-owner provenance in `ChamferBuildArtifacts`.
- [x] Keep live patch records, replacement faces, bevel strips, plans, and commitment unchanged.
- [x] Test one-candidate omission only in a cloned complete patch set.
- [x] Require every transferred boundary segment to be present on the owner boundary.
- [x] Require omitted patch-boundary keys to remain exactly two-use.
- [x] Compare source-boundary, unexpected-open-edge, non-manifold, and T-junction results against the existing clone baseline and reject any new defect.
- [x] Add compact `contained=candidates/resolved/stillRequired/ownerAmbiguous/boundaryTransferFailures/topologyFailures`.
- [x] Parse every changed C# file with zero syntax errors or missing nodes.
- [x] Preserve CRLF and avoid `.meta` changes.

### MG-R6A Unity exit criteria

- [x] Unity compiles with zero errors and zero warnings.
- [x] All 24 physical masses regenerate.
- [x] Existing compact fields match MG-R5; only `contained=` is added.
- [x] Aggregate contained candidates equal the 22 patch-contained overlaps.
- [x] `candidates = resolved + stillRequired`.
- [x] `stillRequired = ownerAmbiguous + boundaryTransferFailures + topologyFailures`.
- [x] Aggregate result is `contained=22/0/22/0/22/0`.
- [x] Rendered mesh appearance remains unchanged.
- [x] `geometryCommit=disabled` remains present for every mass.

## MG-R6B — Contained replacement-owner repartitioning

- [x] Preserve every contained patch; do not retry direct patch omission.
- [x] Consume only production-proven `ChamferContainedPatchCandidate` owner provenance.
- [x] Project owner and retained-patch boundaries into one deterministic owner-plane basis.
- [x] Split endpoint contacts and collinear overlaps before ownership cancellation.
- [x] Cancel shared directed owner/patch segments and trace residual owner cycles.
- [x] Protect original owner vertices and authoritative patch-boundary endpoints from collinear simplification.
- [x] Triangulate only simple residual owner cycles.
- [x] Preserve replacement-face provenance and feature/material data on residual triangles.
- [x] Validate owner-area conservation.
- [x] Validate exact two-use patch-boundary incidence.
- [x] Compare open/source-boundary/non-manifold/T-junction results against each mass's existing clone baseline.
- [x] Reclassify the target patch against transformed replacement geometry.
- [x] Test candidates individually.
- [x] Group individually resolved candidates by owner and test a combined per-mass clone.
- [x] Add compact `containedRepartition=` and `containedCombined=` evidence.
- [x] Keep all construction clone-only and retain `geometryCommit=disabled`.

### MG-R6B Unity exit criteria

- [ ] Unity compiles with zero errors and zero warnings.
- [x] All 24 physical masses regenerate with one compact audit each.
- [x] All prior compact fields remain unchanged.
- [x] Aggregate `containedRepartition=` candidate count is 22.
- [x] Every candidate enters exactly one terminal repartition category.
- [x] Aggregate result is `containedRepartition=22/0/0/0/0/22/0/0`.
- [x] `containedCombined=0/0/0/0/0` correctly records that no individually accepted candidate reached the combined pass.
- [ ] Rendered mesh appearance remains unchanged.
- [x] `geometryCommit=disabled` remains present for every mass.

## MG-R6B.1 — Contained boundary-incidence decomposition

- [x] Preserve MG-R6B residual construction without changing geometry.
- [x] Decompose every authoritative patch-boundary segment by exact face-kind and provenance use.
- [x] Detect collinear split-equivalent coverage independently of exact `TopologyEdgeKey` equality.
- [x] Distinguish owner-interior residual ownership from owner-boundary external ownership.
- [x] Detect an external counterpart that crosses a patch endpoint without matching segmentation.
- [x] Add deterministic candidate and segment categories: exact, split-equivalent, residual-missing, external-unsplit, underused, overused, and ambiguous.
- [x] Continue overlap and topology checks after boundary rejection as shadow evidence.
- [x] Keep split-equivalent evidence diagnostic-only; do not promote candidates or alter `containedRepartition=` semantics.
- [x] Cap verbose representative traces to one case per classification.
- [x] Keep live geometry unchanged and retain `geometryCommit=disabled`.

### MG-R6B.1 Unity exit criteria

- [ ] Unity compiles with zero errors and zero warnings.
- [x] All 24 physical masses regenerate with one compact audit each.
- [x] All pre-MG-R6B.1 fields remain unchanged, including `containedRepartition=22/0/0/0/0/22/0/0`.
- [x] Aggregate `containedBoundary=` is `22/0/0/0/0/0/0/22`.
- [x] Aggregate `containedBoundarySegments=` is `66/0/0/0/0/44/22/0`.
- [x] Aggregate `containedShadow=` is `22/22/0/14/22/0/22`.
- [ ] Rendered mesh appearance remains unchanged.
- [x] `geometryCommit=disabled` remains present for every mass.

MG-R6B.1 evidence proves that all 22 target overlaps are removed, but all 22 transformed clones gain unexpected open and non-manifold edges; 14 also gain T-junctions. Each retained patch has the same three-edge signature: two underused segments and one overused segment.

## MG-R6B.2 — Bundled contained-boundary repair

- [x] Add a deterministic boundary-guided owner-notch construction for the proven contained cases.
- [x] Order each retained patch boundary and identify one contiguous run shared with the owner boundary.
- [x] Replace the shared owner run with the reversed complementary patch path.
- [x] Retain the generic directed-segment arrangement as a deterministic fallback.
- [x] Subdivide every transformed cloned face at authoritative patch endpoints lying in an edge interior.
- [x] Preserve positions, winding, area, feature data, and replacement provenance.
- [x] Classify residual-owner edge occurrences from the exact transformed record range rather than broad source-face identity.
- [x] Apply the same endpoint alignment to the combined per-mass clone.
- [x] Add `containedRepair=` construction and terminal evidence.
- [x] Keep live geometry unchanged and retain `geometryCommit=disabled`.

### MG-R6B.2 Unity exit criteria

- [x] Unity compiles sufficiently to execute the audit with no reported compile failure.
- [x] The final MG-R6B.2 run contains 24 physical-mass compact audits.
- [x] All live and pre-contained compact fields remain unchanged.
- [x] Aggregate `containedRepair=` candidate count is 22.
- [x] `guidedResiduals + genericFallbacks + buildFailures = candidates`.
- [x] `resolved + buildFailures + boundaryFailures + topologyFailures + overlapRemaining = candidates`.
- [x] Aggregate result is `containedRepair=22/22/0/0/0/0/22/0/0`.
- [x] `containedRepartition=`, `containedBoundary=`, `containedBoundarySegments=`, and `containedShadow=` remain unchanged from MG-R6B.1.
- [x] `containedCombined=0/0/0/0/0` correctly remains inactive because no individual candidate resolves.
- [ ] Rendered mesh appearance remains unchanged.
- [x] `geometryCommit=disabled` remains present for every mass.

## MG-R6 — Final MassGenerator refactor closure

- [x] Audit the current post-MG-R6B.2 source rather than rolling back useful functional work.
- [x] Remove the uncalled private `FaceMaterialMaskLookup` subsystem and its five support types.
- [x] Remove the uncalled `TryClipPolyhedron` transaction wrapper and its three private helpers while preserving active direct clipping.
- [x] Remove the unused `VertexKey.ToDiagnosticString` formatter.
- [x] Reduce all `MassGenerator` partials from 26,395 to 25,537 lines.
- [x] Reduce method declarations from 553 to 523 and private nested type declarations from 114 to 108.
- [x] Verify every remaining method has a surviving caller or method-group reference.
- [x] Verify every remaining private nested type has a surviving reference.
- [x] Verify production/shared edge-wear files contain zero diagnostic-only dependencies.
- [x] Preserve all MG-R6A through MG-R6B.2 clone-only work and compact fields.
- [x] Preserve CRLF and avoid `.meta`, editor, serialized asset, shader, material, scene, or prefab changes.

### MG-R6 Unity exit criteria

- [x] Unity compiles with zero errors and zero warnings.
- [x] Exactly 24 compact audits are emitted for the same physical masses.
- [x] Every compact field remains identical to the final MG-R6B.2 baseline.
- [x] Aggregate `containedRepair=` remains `22/22/0/0/0/0/22/0/0`.
- [x] Rendered mesh appearance remains unchanged.
- [x] `geometryCommit=disabled` remains present for every mass.
- [x] Close the `MG-R` workstream and continue functional topology work under `EW-*`.

## EW-K1 — Convex plane-cut bevel kernel

- [x] Resume after validated candidate selection and explicit corner/width solving.
- [x] Keep the existing replacement-face, strip, patch, and contained-repair chain unchanged as comparison evidence.
- [x] Build one deterministic bevel cut plane per active selected edge from the solved four-point rail and requested bevel normal.
- [x] Reject non-finite, non-coplanar, boundary, or non-removing planes before clipping.
- [x] Apply all accepted planes only to a deep-cloned source polyhedron through the existing `ClipPolyhedron` kernel.
- [x] Preserve `ConvexEdgeWear` feature strength on each generated cap.
- [x] Audit one surviving cap per active selected edge.
- [x] Audit closed topology, zero non-manifold edges, zero T-junctions, valid faces, retained volume, and contained bounds.
- [x] Add compact `planeBevel=selected/active/planesBuilt/planesRejected/capsBuilt/capsMissing/open/nonManifold/tJunction/invalid/valid` evidence.
- [x] Keep rendered geometry and `geometryCommit=disabled` unchanged.

### EW-K1 Unity result

- [x] Unity compiles with zero errors and zero warnings.
- [x] Exactly 24 compact audits are emitted with all pre-EW-K1 fields unchanged.
- [x] All 498 active edges report accepted planes and emitted caps; `planesRejected=0`.
- [x] Seventeen of 24 clones report `valid=1` immediately.
- [x] Four failures are isolated to non-conformal shared-edge segmentation: 15 open edges total and two T-junctions, with zero non-manifold or invalid faces.
- [x] Two failures are topology-clean bounds-only numerical rejections.
- [x] One topology-clean clone has one cap consumed by later cuts.
- [x] Rendered geometry remains unchanged and `geometryCommit=disabled` remains present.

## EW-K1.1 — Conformal plane-cut completion

- [x] Preserve every final collinear polyhedron vertex where it subdivides another face edge.
- [x] Weld again after conformity insertion and do not run a later collinear-removal pass.
- [x] Add an opt-in segment-clamp parameter to the shared clipper; keep all legacy callers on the previous default behavior.
- [x] Enable segment clamping only for the clone-only EW-K path.
- [x] Align bounds validation tolerance with `PlaneEpsilon`.
- [x] Distinguish a verified redundant plane from an unexplained missing cap.
- [x] Require the final polyhedron to satisfy a redundant plane and require the original sharp source edge not to survive.
- [x] Expand compact evidence to `planeBevel=selected/active/planesBuilt/planesRejected/capsBuilt/capsMissing/capsRedundant/conformalSplits/open/nonManifold/tJunction/invalid/valid`.
- [x] Keep rendered geometry and `geometryCommit=disabled` unchanged.

### EW-K1.1 Unity result

- [x] Unity compiles with zero errors and zero warnings.
- [x] Exactly 24 compact audits are emitted with every pre-EW-K1.1 field unchanged.
- [x] All 498 active planes build with zero rejection.
- [x] Bounds failures reduce to zero.
- [x] T-junctions reduce to zero.
- [x] One later-consumed cap is classified as redundant.
- [x] Eighteen of 24 masses report `valid=1`.
- [x] Three masses retain four open edges each.
- [x] Three masses retain one unexplained missing cap each.
- [x] Rendered geometry remains unchanged and `geometryCommit=disabled` remains present.

## EW-K1.2 — Canonical intersection and cut-tolerance completion

- [x] Add a per-cut cache keyed by the undirected current polyhedron edge.
- [x] Reuse the exact cached intersection for both incident faces and cap construction.
- [x] Preserve the shared clipper's legacy behavior unless canonicalization is explicitly enabled.
- [x] Record a candidate-specific clip epsilon below the measured source-edge removal.
- [x] Use the candidate epsilon only in the clone-only EW-K path.
- [x] Keep segment clamping, bounds validation, redundancy classification, and all topology gates active.
- [x] Keep rendered geometry and `geometryCommit=disabled` unchanged.

### EW-K1.2 Unity result

- [x] Unity compiles and emits the expected 24 physical-mass audits.
- [x] Every pre-EW-K1.2 compact field remains unchanged.
- [x] All 498 active planes build with zero rejection.
- [x] Canonical per-cut intersections reduce open-edge failures from three masses to one mass.
- [x] Open edges reduce from 12 to 4.
- [x] Non-manifold edges, T-junctions, invalid faces, and bounds failures remain zero.
- [x] Valid clones increase from 18/24 to 20/24.
- [ ] One mass still contains two mutually corresponding numerical seams, reported as four open edge records.
- [ ] Three cuts still emit no cap because earlier cuts appear to have already satisfied their planes while broad source-edge survival tolerance misclassifies nearby bevel boundaries as the original edge.
- [x] Rendered geometry remains unchanged and `geometryCommit=disabled` remains present.

## EW-K1.3 — Final seam and redundant-cut resolution

- [x] Preserve the EW-K clone-only boundary and all legacy live behavior.
- [x] Collect exact one-use open-edge records after all plane cuts.
- [x] Pair only mutually unique edges from different faces with opposite orientation and near-identical endpoints under a narrow topology-scale tolerance.
- [x] Snap verified pair endpoints to shared midpoint targets across every occurrence of the involved vertex keys.
- [x] Roll back the entire seam repair unless it produces exactly two fewer open records per pair without increasing non-manifold edges or T-junctions.
- [x] Detect a plane already satisfied by earlier cuts before invoking the clipper.
- [x] Require strict `PointMergeDistance`-scale proof that the original source edge no longer survives.
- [x] Tighten final redundant-plane source-edge survival to the same strict topology scale.
- [x] Allow final validity to depend on complete final cap/redundancy accounting rather than requiring every active plane to have emitted a new cap at its own step.
- [x] Expand `planeBevel=` with `seamPairs` after `conformalSplits`.
- [x] Consolidate all progress history into this file and remove duplicate timelines, result censuses, and next-step lists from the inventory, architecture, and framework documents.

### EW-K1.3 Unity result

- [x] Unity compiles with zero errors and zero warnings.
- [x] Exactly 24 compact audits are emitted with every unrelated field unchanged.
- [x] Every clone reports `planesBuilt=active` and `planesRejected=0`.
- [x] The exceptional seam mass reports `seamPairs=2`, `open=0`, and `valid=1`.
- [x] All 24 clones report zero open edges, non-manifold edges, T-junctions, and invalid faces.
- [x] Valid clones increase from 20/24 to 21/24.
- [ ] Three cuts still report `capsMissing=1`; each final mesh is topology-clean and already satisfies the cut plane, but the approximate source-line test still rejects redundancy.
- [x] Rendered geometry remains unchanged and `geometryCommit=disabled` remains present.

## EW-K1.4 — Strict half-space redundancy and editor preview

- [x] Record each candidate's measured `MinimumSourceRemoval`.
- [x] Define redundancy from the final convex half-space result under a tolerance strictly below half of `MinimumSourceRemoval`.
- [x] Remove approximate source-line overlap as an authoritative redundancy gate.
- [x] Keep `capsMissing` as a hard failure whenever any final vertex remains outside the candidate half-space.
- [x] Return the audited clone from the kernel without changing normal generation.
- [x] Add an editor-only, non-serialized `Show Plane-Cut Bevel Preview` control to `GeneratedMassEditor`.
- [x] Apply preview faces only when the clone reports `valid=1`; otherwise retain normal geometry.
- [x] Disable preview generation in Play Mode and provide an explicit `Show Production Geometry` action.
- [x] Keep the production `MassGenerator.Generate` entry point and all runtime callers on normal geometry.
- [x] Update progress only in this canonical ledger; update the code inventory only for current method/API ownership.

### EW-K1.4 Unity result

- [x] Unity compiles and emits the expected 24 compact audits.
- [x] Every clone reports `planesBuilt=active`, `planesRejected=0`, `capsMissing=0`, zero polygon-topology failures, and `valid=1`.
- [x] The three former no-cap cases are accounted for as verified redundancies.
- [x] Normal inspector regeneration retains production geometry while preview is disabled.
- [x] The editor-only preview can be displayed and restored explicitly.
- [ ] Visual approval failed: representative previews lose or fold large surface regions and expose displaced-centre triangle fans.
- [x] Root cause is a certification boundary mismatch: polygon faces are audited, then `TriangulatePolyhedron` sanitizes them again and applies displaced-centre surface relief before rendering.
- [x] A second missing guard allows an infinite local bevel plane to remove unrelated source vertices while still passing broad retained-volume validation.
- [x] Production geometry remains uncommitted; `geometryCommit=disabled` remains active.

## EW-K1.5 — Audited mesh handoff and local-cut guard

- [x] Localize every candidate plane so every unrelated original topology vertex remains inside its half-space.
- [x] Reject a localized plane when retaining unrelated vertices prevents meaningful removal of both selected source-edge endpoints.
- [x] Record localized-plane count in compact `planeBevel=` evidence.
- [x] Sanitize the completed clone once before the authoritative final polygon audit.
- [x] Run conformity and conservative seam repair after that sanitation, with no later polygon sanitation.
- [x] Triangulate the exact audited faces directly with flat deterministic convex fans.
- [x] Bypass displaced-centre relief and the ordinary second sanitation pass for editor preview only.
- [x] Audit the exact preview triangle soup for degeneracy, winding, welded open/non-manifold edges, bounds agreement, and volume agreement.
- [x] Add compact `planeMesh=triangles/degenerate/open/nonManifold/winding/bounds/volume/valid` evidence.
- [x] Return the exact audited triangle soup to the editor preview; retain production geometry whenever either polygon or triangle audit fails.
- [x] Keep runtime production generation, serialized assets, shaders, materials, scenes, prefabs, layers, tags, and components unchanged.

### EW-K1.5 Unity result

- [x] Unity compiles with zero errors and zero warnings.
- [x] Exactly 24 compact audits are emitted with every unrelated field unchanged.
- [x] Every accepted plane preserves all unrelated source vertices; impossible local candidates are rejected rather than clipping another region.
- [x] Twenty-one masses report fully valid polygon and triangle-soup previews.
- [x] Three masses each reject exactly one locality-incompatible edge; the other 495 active cuts remain valid.
- [x] Each blocked mass falls back to production geometry because EW-K1.5 treats one locality rejection as fatal to the whole preview.
- [x] The tested blocked rock therefore shows no preview change, a dark Edge Wear debug view, `planeMesh=0/0/0/0/0/0/0/0`, and repeated identical audit output while toggling.
- [x] `Show Production Geometry` still restores the original geometry immediately.
- [x] Production generation and `geometryCommit=disabled` remain unchanged.

## EW-K1.6 — Safe partial preview and deferred-edge accounting

- [x] Reclassify only the specific locality failure “retain unrelated vertices but cannot still remove the selected source edge” as a safe per-edge deferral.
- [x] Keep malformed provenance, invalid normals, non-coplanar rails, non-local solved planes, duplicate caps, topology damage, and triangle-soup failures as hard rejections.
- [x] Permit preview validity when `planesBuilt + planesDeferred = active`, `planesRejected = 0`, at least one plane is built, and all cap/topology/mesh gates pass.
- [x] Continue auditing and rendering only the successfully built local cuts; deferred edges retain their original sharp source geometry.
- [x] Add `planesDeferred` to compact `planeBevel=` evidence after `planesLocalized`.
- [x] Return a non-serialized preview status containing active, built, deferred, rejected, applied, and concise diagnostic state.
- [x] Show explicit inspector feedback when a partial preview is active, including built/active and deferred counts.
- [x] Name the transient mesh as a plane-cut preview only when the audited preview was actually adopted.
- [x] Keep production generation, Play Mode, serialization, materials, shaders, scenes, prefabs, tags, layers, and components unchanged.

### EW-K1.6 Unity result

- [x] Unity compiles with zero errors and zero warnings.
- [x] Valid non-zero `planeMesh` previews render real bevel faces without the destructive clipping or displaced-centre fan failures seen in EW-K1.4.
- [x] The Edge Wear debug view marks the accepted plane-cut bevel faces.
- [x] Safe locality deferral permits useful partial previews rather than suppressing every valid bevel on a mass.
- [x] Representative previews confirm controllable physical bevel geometry is now visually available for evaluation.
- [x] Remaining visual issues are ordinary authoring/topology-quality issues: excessive width at current settings, uniform straight strips, some deferred edges, and artificial recessed base junctions where competing bevel planes trim a primary vertical strip into multiple triangles.
- [x] `Show Production Geometry` and Play Mode remain on production geometry.
- [x] `geometryCommit=disabled` remains active.

## EW-K2 — Base-junction strip preservation and authoritative width control

- [x] Keep the successful convex plane-cut and final triangle-soup audit architecture unchanged.
- [x] Reuse the existing serialized `Edge Wear > Width` field as the sole authoritative physical bevel-width control; do not add a competing preview-only width setting.
- [x] Preserve the established physical mapping for values `0.25-2.0`.
- [x] Extend the same control below `0.25` with a thinner `0.0015-0.006` maximum-dimension range so the current oversized look can be reduced without changing existing serialized values.
- [x] Detect selected multi-edge junctions close to the generated mass base.
- [x] At a base junction with one clearly dominant vertical structural edge, preserve that primary bevel strip to the base and safely defer competing low-verticality base-edge cuts that would trim it into an inward triangular pit.
- [x] Keep all non-junction locality deferral, malformed-candidate rejection, polygon audit, and exact triangle-soup audit gates unchanged.
- [x] Add compact `planeJunction=vertices/protectedEdges/deferredEdges` evidence without adding verbose per-junction logs.
- [x] Keep runtime production promotion disabled.

### EW-K2 Unity result

- [x] Unity compiles with zero errors and zero warnings.
- [x] The existing `Edge Wear > Width` control works across the expanded thin range and remains the sole physical width control.
- [x] Representative successful previews retain non-zero valid `planeMesh`.
- [x] Width reduction exposes the junction defect more clearly because accidental narrow crevices receive stronger shadowing.
- [ ] Visual junction approval failed: tapered strips, widening wedges, recessed endpoint pits, and several-triangle closures remain common at base, upper, side, and non-vertical junctions.
- [x] Compact evidence confirms the base-only heuristic is non-general: almost every mass reports `planeJunction=0/0/0`; only one mass reports a non-zero result.
- [x] The base-only dominant-vertical-edge deferral rule is rejected as the general solution and must not be extended with more orientation-specific cases.
- [x] Production promotion remains disabled and `geometryCommit=disabled` remains present.

## EW-K2.1 — General vertex junction caps

- [x] Retire the base-only dominant-vertical-edge junction deferral heuristic.
- [x] Preserve the validated edge-plane kernel, safe locality deferral, Width mapping, polygon audit, and exact triangle-soup audit.
- [x] Record source edge index, endpoint vertex indices, and solved width on every accepted edge-plane candidate.
- [x] Group accepted edge cuts by original source vertex after all edge planes are applied.
- [x] Treat every original vertex with at least two built incident bevels as a general junction candidate.
- [x] Derive one outward junction normal from the incident bevel-plane normals.
- [x] Derive conservative cutback from the smallest incident solved width.
- [x] Retain every unrelated original topology vertex and require all removed current points to remain within a local junction radius.
- [x] Apply each junction cut transactionally; commit only one unique stable local `ConvexEdgeWear` cap.
- [x] Classify accepted caps as triangle, quad, or larger convex polygon.
- [x] Reject collapsed, remote, or pathological sliver caps without removing the already-valid incident edge bevels.
- [x] Add compact `planeVertexJunction=candidates/built/deferred/triangleCaps/quadCaps/largerCaps/sliverRejected` evidence.
- [x] Update progress only in this canonical ledger; update inventory and architecture only for current ownership and contracts.
- [x] Keep runtime production promotion disabled.

### EW-K2.1 Unity result

- [x] Unity compiles with zero errors and zero warnings.
- [x] Existing `planeBevel` and `planeMesh` topology validity remains intact.
- [x] `planeVertexJunction` reports general candidates across all representative masses and multiple junction orientations.
- [x] Accepted caps remain polygon-clean and exact-triangle-soup valid.
- [x] Some previously defective junctions are visibly replaced by deliberate flat caps.
- [ ] Visual completion failed: representative rocks still retain tapered wedges and dark crevice junctions where the one-shot cap attempt is deferred or sliver-rejected.
- [x] The representative rock reports `10` candidates, `6` built, `3` deferred, and `1` sliver rejection; the same four unresolved junctions remain visible across width changes.
- [x] The one-normal/one-depth attempt is therefore retained only as the direct first trial, not as the complete solver.
- [x] Width values from `0.05` through `2.0` continue to control physical strip width.
- [x] `Show Production Geometry`, Play Mode, and `geometryCommit=disabled` remain unchanged.

## EW-K2.2 — Global junction solver with deterministic edge backtracking

- [x] Rebuild every solver state from the original source polyhedron rather than mutating one failed junction attempt into the next.
- [x] Maintain one explicit state containing active edge planes, accepted junction planes, and deterministically deferred source-edge IDs.
- [x] Search a bounded deterministic family of junction normals: incident bevel-normal sum, angle-weighted original face-normal sum, radial direction, and fixed blends.
- [x] Search fixed cutback factors derived from the local solved bevel width.
- [x] Require every accepted trial to create one unique local cap, join every preserved incident bevel strip, retain unrelated original vertices, pass cap-quality gates, and pass the exact prepared polygon and triangle-soup audit.
- [x] Score direct and adaptive junction candidates together by minimum cut depth, then compactness, lower polygon complexity, and stable normal rank.
- [x] Use breadth-first edge backtracking so the first accepted solution preserves the maximum number of edge bevels within the bounded state search.
- [x] At an unresolved vertex, branch only by deferring one incident edge, ordered deterministically by localization burden, strength, selection score, solved width, source-edge length, and source-edge index.
- [x] Re-solve both endpoints and every downstream junction from the original polyhedron after each deferral.
- [x] Retain a deterministic greedy fallback only after the bounded breadth-first search is exhausted; never retain an unresolved miter as a valid result.
- [x] Permit final vertex states only as an audited junction cap, one remaining active bevel, or no active bevel.
- [x] Replace compact evidence with `planeVertexJunction=candidates/directBuilt/adaptiveBuilt/backtrackBuilt/cleanSharp/unresolved/triangleCaps/quadCaps/largerCaps/edgesDeferred/rebuildPasses`.
- [x] Require `unresolved=0` for polygon and preview validity.
- [x] Keep progress history only in this canonical ledger and keep production promotion disabled.

### EW-K2.2 Unity exit criteria

- [ ] Unity compiles with zero errors and zero warnings.
- [ ] All 24 masses report `planeVertexJunction.unresolved=0`.
- [ ] Every preview-eligible mass reports valid `planeBevel` and `planeMesh` topology.
- [ ] The representative `10`-candidate rock has no remaining tapered wedge or dark crevice junction.
- [ ] Edge deferrals are deterministic and limited to the minimum compatible set found by the bounded global search.
- [ ] Every excluded edge is accounted for by `planesDeferred` and `planeVertexJunction.edgesDeferred`.
- [ ] Width remains functional across `0.05-2.0`.
- [ ] Production geometry, Play Mode, and `geometryCommit=disabled` remain unchanged.


## EW-K2.2R1 — Emergency solver isolation and explicit preview evaluation

- [x] Hard-gate `AuditPlaneCutBevelKernel(...)` behind explicit plane-cut preview generation.
- [x] Make normal `GeneratedMass.Regenerate()` production-only.
- [x] Replace the persistent preview toggle with explicit `Evaluate`, `Refresh`, and `Show Production Geometry` editor actions.
- [x] Mark evaluated previews stale after serialized changes without automatically rerunning the solver.
- [x] Add a per-object regeneration re-entrancy guard.
- [x] Preserve the EW-K2.2 solver implementation for later measured optimization.
- [x] Keep production promotion disabled.

### EW-K2.2R1 Unity result

- [x] Unity compilation/domain reload returns to a usable duration instead of remaining in `Running Backend` indefinitely.
- [x] Ordinary generation records report zero `planeBevel`, `planeVertexJunction`, and `planeMesh` work, proving the solver no longer runs from domain reload or normal regeneration.
- [x] One explicit representative preview evaluates successfully and remains editor-only.
- [x] Explicit evaluation still takes approximately eight seconds for one mass.
- [x] The representative preview reports `planeBevel=18/15/12/0/3/0/12/0/0/0/2/0/0/0/0/1`.
- [x] The same preview reports `planeVertexJunction=10/1/4/2/3/0/0/1/6/3/15` and a valid `198`-triangle preview mesh.
- [ ] Visual approval still fails: a long narrow bevel/junction region reads as a dark trench and contains visibly different triangle lighting.
- [x] The result proves topology validity alone is insufficient; final bevel/junction face planarity and junction-cap shape quality must become authoritative validity gates.
- [x] The attached ordinary-generation log contains two matching 24-mass sequences, confirming the broader duplicate `OnEnable`/`OnValidate` regeneration issue remains for later `MG-P1`.
- [x] `geometryCommit=disabled` remains active.

## EW-K2.2R2 — Bounded solver and certified face quality

- [x] Add compact `planeSolve=states/junctions/trials/rebuilds/polygonAudits/triangleAudits/edgesDeferred/elapsedMs/timedOut` metrics.
- [x] Reduce the interactive breadth-first state ceiling from `512` to `48`.
- [x] Add a hard three-second editor solve budget.
- [x] Stop rebuilding all edge and prior junction planes for every local candidate.
- [x] Build the edge-only state once, clone the current accepted state per local trial, and apply only the proposed new junction plane.
- [x] Retain one authoritative full system rebuild and exact polygon/triangle audit per complete clean state.
- [x] Remove exact polygon and triangle-soup audits from the inner normal/depth trial loop.
- [x] Count candidate trials, state rebuilds, exact audits, deferred edges, elapsed time, and timeout state without per-trial logging.
- [x] Raise the minimum accepted junction-cap compactness from `0.005` to `0.06`.
- [x] Add a hard junction-cap aspect limit of `12`.
- [x] Rank valid candidates by lower aspect ratio, then higher compactness, lower polygon complexity, lower cut depth, and stable normal rank.
- [x] Reject complete states when final prepared junction caps fall outside the same compactness/aspect limits.
- [x] Add final edge-wear face planarity and triangle-normal-spread certification.
- [x] Reject final previews containing any edge-wear face over the scale-relative plane-deviation limit or `0.75` degrees of triangle-normal spread.
- [x] Add compact `planeFaceQuality=faces/seamTouched/nonPlanar/elongated/maxDeviation/maxNormalSpread/minJunctionCompactness/maxJunctionAspect/worstVertices` evidence.
- [x] Project conservative seam-repair snap targets onto the two incident analytical face planes.
- [x] Reject and roll back seam repair if projected endpoints move beyond the narrow seam tolerance, disturb topology, or move any touched face off its original plane.
- [x] Keep production generation, serialized assets, shaders, materials, scenes, prefabs, tags, layers, and components unchanged.
- [x] Keep production promotion disabled.

### EW-K2.2R2 Unity exit criteria

- [ ] Unity compiles with zero errors and zero warnings.
- [ ] Normal domain reload still performs zero plane-cut solver work.
- [ ] One representative explicit preview completes in at most the three-second solver budget.
- [ ] `planeSolve.timedOut=0` for the representative mass, or the preview aborts cleanly with production geometry retained when the budget is exceeded.
- [ ] Inner exact audit counts are bounded near completed-state count rather than candidate-trial count.
- [ ] `planeFaceQuality.nonPlanar=0`.
- [ ] `planeFaceQuality.elongated=0`.
- [ ] Maximum triangle-normal spread remains below `0.75` degrees.
- [ ] The pictured long dark trench is removed or the responsible edge/junction is deliberately deferred instead of certified.
- [ ] Every accepted preview retains zero open edges, non-manifold edges, T-junctions, invalid faces, winding failures, bounds failures, and volume failures.
- [ ] Width remains functional across `0.05-2.0`.
- [ ] Production geometry, Play Mode, and `geometryCommit=disabled` remain unchanged.


### EW-K2.2R2 Unity result

- [x] Unity compiles and normal generation remains isolated from the plane-cut solver.
- [x] Explicit single-object preview performance is substantially improved and no longer blocks every mass on every compile or inspector change.
- [ ] Visual approval still fails: the same representative source-edge bevel remains a long dark crevice.
- [x] Wireframe evidence proves the defect is real geometry rather than merely per-triangle lighting: one intended bevel corridor is partitioned into at least four generated faces and turns into the source mass.
- [x] Face-level planarity, compactness, aspect, topology, bounds, and volume certification can all pass while the one-edge-to-one-band relationship is broken.
- [x] The next authority must therefore certify final generating-plane provenance, endpoint-local junction influence, and longitudinal bevel-band integrity.
- [x] Production promotion remains disabled and `geometryCommit=disabled` remains active.

## EW-K2.2R3 — Bevel-band integrity audit and junction influence proof

- [x] Add non-serialized polygon-face provenance for original source faces, edge-bevel cap planes, and vertex-junction cap planes.
- [x] Preserve provenance through clipping, cloning, final sanitation, conformity, and conservative seam repair.
- [x] Tag every edge cap with its source-edge index and every junction cap with its source-vertex index.
- [x] Require every retained source-edge bevel to own exactly one surviving final bevel face.
- [x] Measure the axial coverage of each owned bevel face along its original source edge.
- [x] Measure each endpoint junction cap's maximum penetration and shared-axis span along every incident source edge.
- [x] Bound junction influence by the smaller of a width/depth-derived local distance and `25%` of source-edge length.
- [x] Reject local junction candidates whose intersection with an incident bevel runs longitudinally beyond the endpoint-local allowance.
- [x] Detect generated faces from unrelated junction or bevel planes that split a bevel-band boundary in the interior of the source edge.
- [x] Treat split, interrupted, foreign-cut, overlong-junction, or collapsed bands as unresolved solver states so deterministic edge backtracking can remove the weaker conflict.
- [x] Add compact `planeBand=retained/singleFace/split/interrupted/foreignCut/overlongJunction/collapsed/minCoverage/maxJunctionInfluence/maxSharedAxisSpan` evidence.
- [x] Keep the 48-state/three-second bounded solver, exact topology audits, production isolation, and `geometryCommit=disabled` unchanged.

### EW-K2.2R3 Unity exit criteria

- [ ] Unity compiles with zero errors and zero warnings.
- [ ] Ordinary generation reports `planeBand=0/0/0/0/0/0/0/0/0/0` and still performs zero solver work.
- [ ] The representative explicit preview either produces one coherent outward bevel band or deliberately defers the conflicting edge.
- [ ] Every accepted retained edge reports one owned bevel face with no split, interruption, foreign cut, overlong junction, or collapse.
- [ ] `planeBand.split=0`.
- [ ] `planeBand.interrupted=0`.
- [ ] `planeBand.foreignCut=0`.
- [ ] `planeBand.overlongJunction=0`.
- [ ] `planeBand.collapsed=0`.
- [ ] The wireframe no longer shows the intended bevel corridor partitioned into a long inward multi-face crease.
- [ ] Every accepted preview retains valid `planeBevel`, `planeFaceQuality`, and `planeMesh` evidence.
- [ ] Width remains functional across `0.05-2.0`.
- [ ] Production geometry, Play Mode, and `geometryCommit=disabled` remain unchanged.


### EW-K2.2R3 Unity result

- [x] Unity compiles and the explicit preview remains isolated from ordinary generation.
- [ ] Visual approval fails: the representative source-edge corridor is still divided into several generated faces and forms a long inward crease.
- [x] Wireframe evidence confirms the failure is real generated geometry, not normal smoothing or triangle-lighting noise.
- [x] Provenance and band-integrity rejection did not make global half-space junction planes reliably local.
- [x] Global half-space planes are rejected as the final vertex-junction architecture; retained edge-plane, width, topology, and certification work remains reusable.
- [x] Production promotion remains disabled.

## MG-P1A — Production generation and diagnostic isolation

- [x] Add an explicit internal edge-wear evaluation mode: `None`, `PlaneCutPreview`, or `LegacyDiagnosticAudit`.
- [x] Make ordinary `MassGenerator.Generate(...)` use `None` and skip edge-wear candidate discovery, topology-context construction, corner solving, legacy reconstruction, corrected-clone diagnostics, plane-cut solving, and edge-wear logging.
- [x] Keep `GeneratedMass.OnEnable`, `OnValidate`, and explicit production regeneration capable of rebuilding the transient production mesh without running diagnostic-grade edge-wear work.
- [x] Make explicit plane-cut preview run only the shared selection/corner preparation and plane-cut kernel; do not run the legacy replacement/strip/patch audit beside it.
- [x] Add one dedicated `GeneratedMass plane-cut bevel compact audit` for explicit preview evaluation.
- [x] Preserve the full legacy replacement/strip/patch/corrected-clone audit behind an explicit single-object editor action.
- [x] Ensure the legacy diagnostic action does not apply a mesh, recook a collider, refresh the world-geometry fingerprint, or notify geometry consumers.
- [x] Keep all diagnostic geometry clone-only and keep `geometryCommit=disabled`.

### MG-P1A Unity exit criteria

- [ ] Unity compiles with zero errors and zero warnings.
- [ ] Script reload with all 24 masses emits zero `GeneratedMass edge wear compact audit` messages.
- [ ] Entering Play Mode emits zero edge-wear diagnostic audits.
- [ ] Exiting Play Mode emits zero edge-wear diagnostic audits.
- [ ] All masses still restore valid production meshes and colliders.
- [ ] Explicit production regeneration of one mass emits zero edge-wear diagnostic audits and preserves its production result.
- [ ] Explicit plane-cut preview emits exactly one `GeneratedMass plane-cut bevel compact audit` and no legacy replacement/patch compact audit.
- [ ] Explicit legacy audit on one selected mass emits exactly one `GeneratedMass edge wear compact audit` and does not change displayed geometry.
- [ ] Domain-reload and Play Mode transition durations are recorded for comparison with the previous 79–96 second range.
- [ ] `geometryCommit=disabled` remains active.

### MG-P1A Unity result

- [x] Unity compiles and ordinary script reload no longer runs Generated Mass edge-wear diagnostic audits.
- [x] Entering and exiting Play Mode no longer runs automatic edge-wear diagnostic audits.
- [x] Production meshes remain available after lifecycle restoration.
- [x] Explicit plane-cut preview and explicit legacy diagnostics remain opt-in.
- [ ] Exact post-P1A reload and Play Mode timing measurements were not supplied.

## MG-P1B — Lifecycle coalescing and deterministic production-state reuse

- [x] Replace direct `OnEnable` and `OnValidate` regeneration with deterministic generated-state synchronization.
- [x] Add a serialized production-generation state covering every normal mesh input and a manually maintained generation-contract version.
- [x] Re-adopt an existing restored production mesh only when the stored state matches, the mesh name matches the current production identity, and the mesh contains valid triangle geometry.
- [x] Reject plane-cut preview meshes and arbitrary assigned meshes as reusable production state.
- [x] Permit a missing or stale production mesh to rebuild once, then allow a later `OnEnable` or `OnValidate` callback to reuse the accepted result instead of rebuilding again.
- [x] Keep manual `Regenerate` as an authoritative forced production rebuild.
- [x] Classify feature-atlas state separately from production geometry and refresh diagnostic atlases without collider recooking or geometry notifications when positions and triangles are unchanged.
- [x] Apply material properties without rebuilding geometry for material-only changes.
- [x] Track river-interaction authoring separately and notify geometry consumers once without rebuilding the mass mesh.
- [x] Rebind or recook the `MeshCollider` only when the mesh binding is missing or production/preview geometry was actually rebuilt.
- [x] Replace eager exact world-triangle fingerprint calculation with invalidation plus lazy calculation on the first consumer request.
- [x] Add low-overhead Profiler markers for synchronization, production generation, collider binding, fingerprint calculation, and consumer notification.
- [x] Preserve explicit plane-cut preview, explicit legacy diagnostics, production visuals, and `geometryCommit=disabled`.

### MG-P1B Unity exit criteria

- [ ] Unity compiles with zero errors and zero warnings.
- [ ] The first reload after applying P1B may rebuild each legacy object once to establish its hidden accepted state, but no mass regenerates twice in the same restoration.
- [ ] A later harmless script reload performs zero production regeneration when Unity restores each certified production mesh.
- [ ] When Unity does not retain a transient mesh, each affected mass performs at most one fallback production rebuild.
- [ ] Entering and exiting Play Mode produces no duplicate production regeneration per mass.
- [ ] Changing Base Color or another material-only control produces zero `GeneratedMass.GenerateProduction` and zero `GeneratedMass.BindCollider` recook markers.
- [ ] Changing one river-interaction control produces one `GeneratedMass.NotifyConsumers` marker and zero production-generation markers.
- [ ] Changing Shape Seed produces exactly one production generation, one collider recook, one fingerprint invalidation, and one consumer notification.
- [ ] Manual Regenerate performs exactly one forced production rebuild even when the accepted state already matches.
- [ ] An atlas diagnostic view builds the required atlas while leaving collider geometry and river geometry notifications unchanged.
- [ ] Exact world-triangle fingerprints are computed only when a consumer calls `TryGetStableWorldGeometryFingerprint`.
- [ ] A retained plane-cut preview is rejected during restoration and production geometry is restored.
- [ ] Explicit legacy diagnostics change no retained mesh, collider, production state, fingerprint, or registry state.

### MG-P1B Unity result

- [x] Unity compilation and editor lifecycle behavior are confirmed usable after deterministic production-state reuse.
- [x] The performance-recovery sequence is accepted as complete enough to resume explicit edge-wear geometry work.
- [x] No further retained-mesh asset persistence pass is currently justified.
- [x] The inward multi-face bevel defect remains intentionally unchanged by the lifecycle patch.

## EW-L1 — Edge-only baseline and bounded junction-star extraction

- [x] Remove `SolvePlaneCutGlobalJunctionSystem(...)` from the active explicit preview path.
- [x] Retain the global junction-solver source only as rejected experimental evidence; do not execute its state search, normal/depth trials, edge backtracking, or timeout budget.
- [x] Build the preview shell by replaying only accepted `EdgeBevelPlane` candidates on a deep source clone.
- [x] Preserve locality-only safe deferral, source/edge provenance, final sanitation, conformity, plane-preserving seam repair, cap/redundancy accounting, polygon topology audit, and exact triangle-soup certification.
- [x] Keep `planeSolve=0/0/0/0/0/0/0/0/0` for explicit L1 preview evaluation.
- [x] Identify every original source vertex with at least two retained incident bevel planes as one local-junction candidate.
- [x] Bound each candidate neighborhood with planes perpendicular to every source edge incident to that vertex.
- [x] Derive each cutback distance from solved bevel width and geometry scale, capped at `25%` of the corresponding source-edge length.
- [x] Apply those bounds only to copied face polygons; do not clip the complete rock or emit any new junction cap.
- [x] Collect the bounded surface star from source faces incident to the source vertex and bevel faces owned by incident retained edges.
- [x] Reject unrelated source-face, edge-bevel, or junction provenance within the bounded star.
- [x] Require every retained incident bevel to appear exactly once in the bounded star.
- [x] Extract one one-use boundary component and require every boundary vertex to have degree two.
- [x] Order the boundary deterministically and reject branches, disconnected components, duplicate incident bevels, missing incident bevels, and projected self-intersection.
- [x] Add compact `localJunction=candidates/starsExtracted/closedLoops/branched/selfIntersecting/foreignFace/missingIncidentBevel/duplicateIncidentBevel/minLoopVertices/maxLoopVertices/maxExtentRatio` evidence.
- [x] Render the exact certified edge-only shell even when local-loop extraction reports a diagnostic failure; L1 does not fill or alter any local loop.
- [x] Keep production generation, editor lifecycle performance, serialized assets, and `geometryCommit=disabled` unchanged.

### EW-L1 Unity exit criteria

- [ ] Unity compiles with zero errors and zero warnings.
- [ ] Ordinary generation and lifecycle restoration remain unchanged from confirmed MG-P1B behavior.
- [ ] One explicit representative preview reports `planeSolve=0/0/0/0/0/0/0/0/0`.
- [ ] The exact edge-only polygon shell and triangle soup remain topology-, winding-, bounds-, and volume-valid.
- [ ] The previously reported inward multi-face crevice disappears when all global junction planes are absent, or is proven to originate from interacting edge planes.
- [ ] `localJunction.candidates` matches the number of source vertices with at least two retained incident bevels.
- [ ] Every successful star reports one closed, non-branching, non-self-intersecting loop.
- [ ] Every successful star contains no foreign provenance, missing incident bevel, or duplicate incident bevel.
- [ ] Return `planeBevel`, `planeSolve`, `planeFaceQuality`, `planeBand`, `localJunction`, `planeMesh`, and `planeTrace` for the same representative rock.
- [ ] Do not begin local cap construction until the representative edge-only shell and extracted loops are understood.
- [ ] `geometryCommit=disabled` remains active.

### EW-L1 Unity result

- [x] Unity compiles and the active preview reports `planeVertexJunction=0/0/0/0/0/0/0/0/0/0/0` and `planeSolve=0/0/0/0/0/0/0/0/0`, proving the rejected global junction solver is absent.
- [x] The representative edge-only preview remains polygon- and triangle-topology valid with `planeMesh=174/0/0/0/0/0/0/1`.
- [ ] Visual approval fails: the long inward multi-face crevice remains without any global junction plane.
- [x] `planeBand=15/15/0/1/1/0/0/0.904226/0/0` proves one retained edge band is interrupted by a foreign generated edge plane.
- [x] `localJunction=10/10/9/0/0/1/1/0/8/9/28.1881` independently reports one foreign face, one missing incident bevel, and a non-local maximum extent.
- [x] The result is authoritative Outcome B: interacting edge half-spaces, not only the rejected junction half-spaces, can corrupt the one-edge-to-one-band relationship.
- [x] Do not begin bounded local cap construction on this shell. Resolve edge-plane conflicts first.

## EW-L1.1 — Edge-plane conflict attribution and clean-band backtracking

- [x] Make `planeBand` and `localJunction` compact output self-describing instead of requiring positional schema lookup.
- [x] Add `edgeConflict=passes/deferred/resolved/budgetExhausted/victim/foreign/vertex/deferredEdge/victimCoverage/foreignAxial/foreignSpan` as named compact evidence.
- [x] Attribute the first bevel-band failure to the victim source edge and, when provenance permits, the foreign cutting source edge.
- [x] Record the nearest responsible source vertex, victim axial coverage, foreign axial location, and shared longitudinal span.
- [x] Add a deterministic clean-band replay loop limited to `12` complete edge-only shell evaluations.
- [x] On an attributed victim/foreign conflict, compare the two source edges with the existing stable backtracking priority and defer only the weaker edge.
- [x] For split, collapsed, or otherwise unattributed single-edge failures, defer the victim edge itself.
- [x] Rebuild every pass from the untouched source shell; do not incrementally mutate a previously failed shell.
- [x] Accept the edge-only preview only when every retained band has one owned face and zero split, interruption, foreign cut, overlong influence, or collapse.
- [x] Count conflict-driven deferrals in `planeBevel.planesDeferred` while preserving locality deferrals and hard rejections as distinct outcomes.
- [x] Keep the global junction solver dormant, keep local-loop extraction non-mutating, and keep `geometryCommit=disabled`.

### EW-L1.1 Unity exit criteria

- [ ] Unity compiles with zero errors and zero warnings.
- [ ] The representative preview emits named `planeBand`, `edgeConflict`, and `localJunction` values.
- [ ] `edgeConflict.victim` and `edgeConflict.foreign` identify the source edges responsible for the previously reported corridor split.
- [ ] The bounded replay defers one or more weaker conflicting edges without invoking `planeSolve`.
- [ ] An accepted preview reports `planeBand.split:0`, `interrupted:0`, `foreignCut:0`, `overlongJunction:0`, and `collapsed:0`.
- [ ] `edgeConflict.resolved:1` when a detected conflict is eliminated, with `budgetExhausted:0`.
- [ ] The long inward multi-face crevice disappears and is replaced either by one coherent retained band or by a clean sharp source edge where the weaker bevel was deferred.
- [ ] If conflict deferrals cascade or the 12-pass budget is exhausted, preview adoption is refused and production geometry remains displayed.
- [ ] Local-junction extraction is rerun only on the final clean retained-edge set.
- [ ] Polygon, triangle, bounds, volume, lifecycle, and performance behavior remain valid.
- [ ] `geometryCommit=disabled` remains active.

### EW-L1.1 Unity result

- [x] Unity compiles and the bounded conflict resolver identifies source-edge conflict `victim:36`, `foreign:18`, at source vertex `19`.
- [x] The resolver deterministically defers edge `36` in two complete passes and reports `resolved:1`, `budgetExhausted:0`.
- [x] The accepted edge-only state reports one face per retained edge and zero split, interruption, foreign cut, overlong influence, or collapse.
- [x] The exact triangle soup remains valid with `planeMesh=168/0/0/0/0/0/0/1`.
- [ ] Visual approval still fails: the same long inward crease remains after the attributed foreign edge is removed.
- [x] This proves the malformed corridor can be intrinsic to one edge's own whole-rock half-space cap rather than only an interaction between two generated planes.
- [x] Infinite whole-rock edge bevel planes are rejected as the final bevel primitive. Further plane-quality thresholds or conflict backtracking are not an admissible geometry direction.
- [x] The next experiment must use the four solved rail points directly as a bounded local bevel polygon.

## EW-B1 — Bounded single-edge bevel primitive

- [x] Add an editor-only `BoundedSingleEdgePreview` evaluation mode independent of production, the rejected whole-rock plane diagnostic, and the legacy reconstruction audit.
- [x] Build a deterministic eligible-edge list from selected internal manifold edges. The original isolated-edge corner solve was Unity-tested and rejected because valid full-solution edges could disappear when every neighbour was forced to zero width.
- [x] Evaluate exactly one selected source edge at a time, addressed by stable source-edge order and a non-serialized editor ordinal.
- [x] Attempt direct owner-loop rail splicing. Unity rejected this reconstruction because the retained owner polygon was frequently non-convex even when the source face and intended local trim were convex.
- [x] Emit exactly one bounded bevel polygon from the four solved rail points `a0/b0/b1/a1`.
- [x] Emit exactly two local endpoint-cap triangles using the original source endpoints and the two rail endpoints at each end.
- [x] Preserve every unrelated source face geometrically; insert only the four required collinear rail-boundary subdivisions into endpoint-adjacent non-owner faces so the two local caps share exact watertight edges. Carry explicit bounded-bevel, bounded-endpoint, and source-face provenance through final preparation.
- [x] The first prototype forced every other selected edge to zero width and required one isolated active edge. Unity rejected that requirement after source edges `8` and `10` lost their rail solve and multiple other edges failed owner convexity.
- [x] Reuse the exact polygon topology, bounds, volume, winding, and triangle-soup certification gates before applying the editor preview.
- [x] Add concise named `boundedEdge`, `boundedTopology`, and `boundedMesh` evidence with `geometryCommit=disabled`.
- [x] Add editor-only Previous, Evaluate/Refresh, Next, and Show Production Geometry controls for one selected Generated Mass.
- [x] Give the bounded preview a distinct transient mesh identity so lifecycle reuse can never adopt it as production geometry.
- [x] Keep production generation, collider/fingerprint lifecycle behavior, serialized recipes, and Play Mode unchanged.

### EW-B1 Unity result

- [x] Unity compiles and the editor-only bounded-edge controls run without restoring the rejected whole-rock junction solver.
- [x] Candidate traversal is deterministic and reports `candidateCount:18` for the representative mass.
- [ ] No tested edge produced a valid bounded preview.
- [x] Source edges `6`, `7`, `11`, `14`, `15`, and `16` reached one active isolated rail but failed with `a bounded owner polygon is not convex`.
- [x] Source edges `8` and `10` failed with `isolatedActiveEdges:0`, proving that forcing neighbouring widths to zero does not preserve the established full corner solution.
- [x] Failed bounded previews restored non-bevel production presentation. MG-X1 preview isolation remains deferred until bounded bevel implementation is complete; River cache validation is non-authoritative while any GeneratedMass preview is active.
- [x] The next correction must reuse the normal full selected-edge rail solution and trim each convex owner face through a local face-plane half-plane clip rather than direct loop splicing.

## EW-B1.1 — Direct rail reuse and convex owner-face clipping

- [x] Run the established full selected-edge corner solution without forcing neighbouring edges to zero width.
- [x] Select one source edge for emission and require that edge to retain a positive solved width and all four full-solution rail corners.
- [x] Remove the `isolatedActiveEdges` acceptance rule and replace it with `selectedRailSolved` evidence.
- [x] Project each convex owner face and its rail into a stable local 2D basis.
- [x] Clip the owner polygon by the local rail half-plane, retaining the side containing the non-edge source vertices.
- [x] Require exactly two boundary intersections matching the two solved rail endpoints and require the rail endpoints to form one adjacent retained boundary segment.
- [x] Preserve the source-face analytical plane, winding, simplicity, convexity, area, and provenance after clipping.
- [x] Add named `boundedOwner` evidence for attempted/clipped owners and intersection, degenerate, non-planar, non-simple, non-convex, and winding failures.
- [x] Keep the bounded bevel quad, two local endpoint caps, four non-owner boundary subdivisions, exact topology/triangle certification, production isolation, and `geometryCommit=disabled` unchanged.
- [x] Record the deferred MG-X1 rule: restore Production Geometry on every previewed mass before authoritative River cache preparation; do not weaken the River obstacle-fingerprint contract.

### EW-B1.1 Unity exit criteria

- [ ] Unity compiles with zero errors and zero warnings.
- [ ] Previously rejected edges `8` and `10` now report either `selectedRailSolved:1` or a precise full-solution missing-width/corner reason.
- [ ] Previously non-convex edges no longer fail through direct owner-loop splicing.
- [ ] A valid candidate reports `selectedRailSolved:1`, `ownerClips:2`, `boundarySubdivisions:4`, `bevelFaces:1`, `endpointCaps:2`, `modifiedSourceFaces:2`, and `foreignSourceFacesModified:0`.
- [ ] `boundedOwner` reports `attempted:2`, `clipped:2`, and zero failure counters.
- [ ] Polygon and exact triangle-soup topology remain watertight and valid.
- [ ] The selected edge renders as one bounded outward bevel face with two short local endpoint closures.
- [ ] Production geometry is restored before any authoritative River cache preparation.

### EW-B1 Unity exit criteria

- [ ] Unity compiles with zero errors and zero warnings.
- [ ] Ordinary generation, script reload, and Play Mode remain consistent with the confirmed MG-P1B behavior and emit no automatic bounded-edge audit.
- [ ] Evaluating one bounded edge emits exactly one `GeneratedMass bounded edge compact audit`.
- [ ] Superseded by EW-B1.1: the audit reports `selectedRailSolved:1`, `ownerClips:2`, `boundarySubdivisions:4`, `bevelFaces:1`, `endpointCaps:2`, `modifiedSourceFaces:2`, and `foreignSourceFacesModified:0`.
- [ ] `railDeviation` and `maxExtentBeyondRails` remain within the certified geometry tolerance.
- [ ] `boundedTopology` reports zero open, non-manifold, T-junction, and invalid-face failures.
- [ ] `boundedMesh` reports zero degenerate, open, non-manifold, winding, bounds, and volume failures.
- [ ] The selected edge renders as one outward bounded bevel face rather than a long inward whole-rock cap.
- [ ] Previous/Next cycles deterministically through the eligible selected edges without altering production data.
- [ ] A rejected bounded edge falls back to production geometry while retaining its candidate count, ordinal, source-edge index, and concise blocker.
- [ ] Show Production Geometry restores the certified production mesh immediately.
- [ ] `geometryCommit=disabled` remains active.

## EW-C4 — Commit and visual proof

- [ ] Commit replacement faces, edge strips, and corner patches.
- [ ] Confirm `ConvexEdgeWear` feature data reaches the final mesh.
- [ ] Wire Amount, Coverage, and Width to their approved responsibilities.
- [ ] Validate one-strip faceted chamfers in final rendering.

## EW-C5 — Controlled irregularity and material response

- [ ] Add deterministic width variation only after stable topology.
- [ ] Add crude optional second strip only if visually necessary.
- [ ] Add shader/material response without changing topology ownership.


EW-C1R3 permits local candidate deferral: a selected candidate whose required solved width falls below the useful geometry threshold is assigned width zero and excluded from edge-strip emission. This is not a topology failure; it preserves the source surface while allowing compatible candidates to proceed.

### EW-B1.1 Unity result

- [x] Unity compiles and the direct full-solution rail reuse path runs across the representative candidate list.
- [ ] No tested candidate produced a valid bounded bevel face or endpoint cap.
- [x] Source edges `6`, `7`, `14`, `15`, and `16` retained a full-solution rail but that rail was embedded zero times in the original endpoint-adjacent boundaries.
- [x] Source edges `8` and `10` had no active width in the full multi-edge corner solution.
- [x] Source edge `11` completed both local owner clips and four boundary subdivisions, then failed later preparation with an un-attributed generic non-convex message.
- [x] The result proves that a shared multi-edge solved corner cannot be reused for the isolated endpoint-cap prototype: neighbouring offsets can move the corner into the owner-face interior, away from the original adjacent source boundary.
- [x] Full multi-edge rail reuse is rejected for EW-B1 isolated closure. It remains relevant only to the later shared multi-edge reconstruction and bounded vertex-cap stages.

## EW-B1.2 — True isolated rail construction and exact boundary ownership

- [x] Remove the shared multi-edge `ChamferCornerSolution` as an input to the isolated bounded primitive.
- [x] Solve each of the selected edge's four rail points directly with the selected support line offset and the endpoint-adjacent support line fixed at zero offset.
- [x] Start from the normal per-edge width and deterministically back off by `0.75` for at most `12` attempts, accepting the largest stable isolated width.
- [x] Require every rail point to be finite, locally bounded, and strictly inside its exact adjacent source-edge segment.
- [x] Record owner graph/source face, source endpoint, adjacent graph edge, and opposite target graph/source face for every rail point.
- [x] Require four distinct exact target graph edges before bounded geometry emission.
- [x] Replace nearest-segment boundary searching with graph-owned exact segment subdivision on the recorded target source face.
- [x] Keep the local owner-face half-plane clipping, one bevel quad, two endpoint triangles, four collinear non-owner subdivisions, and exact topology/triangle certification.
- [x] Add `isolatedRailSolved`, `widthAttempts`, `solvedWidth`, and `targetBoundaries` to `boundedEdge` evidence.
- [x] Add `boundedPrepare` evidence for input validation, weld, conformity, seam repair, failure stage, exact face, polygon failure kind, and provenance.
- [x] Keep MG-X1 deferred: diagnostic previews remain non-authoritative for River cache preparation until bounded bevel production integration is complete.
- [x] Keep `geometryCommit=disabled` and make no production, lifecycle, River, scene, prefab, material, shader, tag, layer, or recipe changes.

### EW-B1.2 Unity exit criteria

- [ ] Unity compiles with zero errors and zero warnings.
- [ ] Previously full-solution-deferred edges `8` and `10` receive an independent isolated-width attempt rather than failing from the shared width map.
- [ ] Every solved rail reports `isolatedRailSolved:1`, `targetBoundaries:4`, and a positive `solvedWidth`.
- [ ] Every rail point splits its exact graph-owned endpoint-adjacent boundary exactly once.
- [ ] At least one representative candidate reports `ownerClips:2`, `boundarySubdivisions:4`, `bevelFaces:1`, `endpointCaps:2`, and `valid:1`.
- [ ] `boundedOwner` reports zero failure counters for an accepted candidate.
- [ ] `boundedPrepare.failedStage:none`; otherwise the exact face/provenance and polygon category identify the remaining blocker.
- [ ] Polygon and exact triangle-soup topology report zero open, non-manifold, T-junction, winding, bounds, and volume failures.
- [ ] The valid candidate renders as one bounded outward bevel quad with two short endpoint closures and no long inward whole-rock crease.
- [ ] Production Geometry is restored before authoritative River cache preparation.

### EW-B1.2 Unity result

- [x] Isolated rails, exact target ownership, and both owner clips succeeded for ordinary candidates.
- [ ] No bounded bevel was emitted because endpoint-adjacent source faces failed input convexity after rail subdivision.
- [x] The failure occurs before weld, conformity, seam repair, topology, or triangulation.
- [x] The solved rail was accepted near the exact boundary but the unsnapped solved point was inserted, creating a microscopic reflex corner.

## EW-B1.3 — Canonical boundary rails and subdivision-safe certification

- [x] Replace each accepted solved rail position with its exact projection onto the graph-owned target boundary segment.
- [x] Certify the canonical position against both analytical face planes and use it everywhere: owner clips, boundary subdivisions, bevel quad, endpoint caps, and rail audits.
- [x] Track `canonicalRails` and `maxBoundarySnap` in the bounded audit.
- [x] Preserve real subdivision vertices, but simplify duplicate/collinear points only for the convexity check.
- [x] Report whether a preparation failure occurred on a canonical rail-subdivided source face.
- [x] Keep production, lifecycle, River, scenes, prefabs, materials, shaders, tags, layers, and recipes unchanged.

### EW-B1.3 Unity exit criteria

- [ ] A representative candidate reports `canonicalRails:4`, `ownerClips:2`, `boundarySubdivisions:4`, and `valid:1`.
- [ ] `boundedPrepare.failedStage:none`; otherwise the remaining blocker identifies the exact non-subdivision defect.
- [ ] The preview is one bounded outward bevel with two local endpoint closures and no long inward crease.


### EW-B1.3 Unity result

- [x] Canonical rail snapping fixed input convexity: ordinary candidates now reach one bevel face, two endpoint caps, clean preparation, and clean polygon topology.
- [ ] Preview adoption is blocked by `foreignSourceFacesModified:2` even though the two foreign faces only contain intentional collinear rail subdivisions plus narrow seam repair.
- [x] The remaining blocker is certification, not bounded geometry construction.

## EW-B1.4 — Planar region equivalence and foreign boundary certification

- [x] Compare source faces as planar regions through common-plane projection, area agreement, and mutual containment rather than exact vertex-cycle identity.
- [x] Preserve strict rejection when a foreign source surface changes area or region.
- [x] Count equivalent non-identical foreign boundaries separately as `foreignBoundarySubdivided`.
- [x] Keep preview validity gated by `foreignSourceFacesModified:0`; intentional boundary subdivision is not a surface modification.
- [x] Make no bounded geometry, production, lifecycle, River, scene, prefab, material, shader, tag, layer, or recipe changes.

### EW-B1.4 Unity exit criteria

- [x] Edges `6` and `11` report `foreignSourceFacesModified:0` and `foreignBoundarySubdivided:2`.
- [ ] Exact polygon and triangle certification passes and the bounded preview renders one outward bevel with two local endpoint caps.

### EW-B1.4 Unity result

- [x] Planar-region equivalence removed the false foreign-surface blocker while retaining exact region-change rejection.
- [x] Edges `6`, `7`, and `11` reach clean polygon preparation and topology with one bevel face and two endpoint caps.
- [ ] Edges `6` and `7` stop at the combined bounds/volume gate despite clean undirected topology.
- [ ] Edge `11` is accepted by subdivision-safe preparation but rejected by the stricter unsimplified triangulation convexity test.
- [x] The remaining blockers are final generated-face winding, split bounds/volume evidence, and consistent bounded triangulation rather than rail or owner-face construction.

## EW-B1.5 — Outward winding certification and consistent bounded triangulation

- [x] Certify the final prepared bevel quad and endpoint caps against the original solid centre, reverse only generated bounded faces that point inward, and reconstruct their immutable `PolygonFace` records.
- [x] Run a second generated-face audit and require `outwardWindingFailures:0` before topology, volume, or triangulation can pass.
- [x] Split the previous combined bounds/volume blocker into explicit `boundsValid`, source/result volume, `volumeRatio`, and `volumeValid` evidence without weakening thresholds.
- [x] Make triangulation use the same duplicate/collinear-reduced convexity classification as preparation while emitting one triangle for every segment of the unchanged real boundary.
- [x] Verify every emitted triangle exists and agrees with the parent polygon winding.
- [x] Record exact triangulation face, provenance, failure category, and reason instead of the previous generic centre-fan error.
- [x] Keep isolated rail solving, owner clipping, canonical boundary ownership, endpoint-cap topology, production generation, lifecycle, River, scenes, prefabs, materials, shaders, tags, layers, and recipes unchanged.

### EW-B1.5 Unity result

- [x] Edge `11` reached `valid:1`, emitted `98` triangles across `19` faces, and passed polygon, topology, bounds, retained-volume, winding, and exact triangle-soup certification.
- [x] Subdivision-safe final triangulation is proven: edge `11` preserved all four canonical boundary subdivisions and reported no triangulation failure.
- [x] Edges `6` and `7` reported `boundsValid:1`, `facesReoriented:0`, and `outwardWindingFailures:0`; their remaining blocker is not winding.
- [x] Edges `6` and `7` exceeded the raw-source upper volume ratio by only `0.00531%` and `0.00174%` beyond the `1.0001` threshold respectively, while edge `11` passed at `1.000088`.
- [x] The prior high-confidence winding diagnosis for edges `6` and `7` is rejected by direct telemetry.
- [ ] A wireframe preview still must confirm that edge `11` is visually the intended local outward bevel with two endpoint closures.

### EW-B1.5 methods decision

- [x] Accepted: final outward certification relative to the original convex solid centre. A preferred bevel normal may guide construction but is not authoritative for shell winding.
- [x] Accepted: simplify only a temporary classification loop; preserve the real subdivided boundary in emitted topology.
- [x] Rejected: removing collinear rail subdivisions to satisfy triangulation.
- [x] Rejected: treating the edges `6` and `7` retained-volume failure as evidence of inward generated-face winding.
- [x] Rejected: weakening retained-volume limits before comparing preparation-equivalent shells.

## EW-B1.5R1 — Preparation-equivalent retained-volume certification and cumulative telemetry

- [x] Prepare a clone of the untouched source shell through the exact same polygon-copy, weld, boundary-conformity, seam-repair, and final validation pipeline used by the bounded result.
- [x] Keep the raw source shell as the strict geometric-bounds authority while also reporting prepared-source bounds and containment margins.
- [x] Use the prepared source volume, not the numerically unprepared raw source volume, as the retained-volume comparison baseline.
- [x] Preserve the existing retained-volume acceptance interval `0.75 < ratio <= 1.0001`; do not loosen it.
- [x] Retain raw-source volume, raw ratio, and raw delta as evidence rather than deleting the prior comparison.
- [x] Add independent result-preparation and source-preparation telemetry for face/vertex/unique-vertex cardinality, weld, conformity, seam pairs, seam-touched faces, topology before/after, invalid faces, preparation volume drift, exact failure stage, face, kind, and provenance.
- [x] Add raw/prepared/result bounds, bounds tolerance, per-side containment margins, raw/prepared volume ratios and deltas, source-preparation ratio, threshold values, and threshold margins.
- [x] Keep all evidence in the existing single bounded-edge record. Do not emit per-face or per-success Console messages.
- [x] Establish the cumulative diagnostic rule: when a new Generated Mass geometry blocker requires new evidence, add structured fields without deleting still-relevant earlier evidence.
- [x] Keep rail solving, width solving, owner clipping, canonical rails, endpoint topology, triangulation, production generation, River, scenes, prefabs, materials, shaders, tags, layers, and recipes unchanged.

### EW-B1.5R1 Unity exit criteria

- [x] Edges `6`, `7`, and `11` report successful `boundedSourcePrepare` records with zero final topology and polygon failures.
- [x] `boundedVolume.preparedRatio` identifies the preparation-equivalent retained-volume result while `rawRatio` remains available as comparison evidence.
- [x] Edges `6` and `7` prove genuine post-preparation expansion through negative `upperMargin` values; source preparation changes volume by only approximately `1.18E-08`.
- [x] Edge `11` remains `valid:1` with `98` triangles and no regression in topology or triangle-soup certification.

### EW-B1.5R1 methods decision

- [x] Accepted: compare like with like. A prepared result must be volume-certified against a source shell subjected to the same deterministic numerical preparation.
- [x] Accepted: exhaustive structured telemetry is preferable to repeated hypothesis-driven patches, provided it remains one record per physical evaluation.
- [x] Rejected: increasing the `1.0001` upper threshold merely because two candidates narrowly miss it.
- [x] Deferred: edges `8` and `10` minimum-width rail feasibility remains outside this patch.

## EW-B1.5R2 — Edge classification, source-solid containment, and volume attribution telemetry

- [x] Classify every bounded eligible edge as `Convex`, `Concave`, `Coplanar`, `Ambiguous`, or `InvalidOrientation` without filtering the candidate pool.
- [x] Record the selected edge owner faces, outward normals, normal dot, dihedral angle, cross-face interior signed distances, solid-centre sidedness, tolerance, and complete pool classification counts.
- [x] Audit the original source shell against every source face plane and report exact source-convexity violations.
- [x] Test every bounded-result vertex against every original source face plane and report exact outward-containment violation face, provenance, vertex, source plane, and maximum distance.
- [x] Attribute signed volume using one common interior reference point across the two original owners, two replacement owners, bevel quad, endpoint caps, and foreign source faces.
- [x] Report local replacement delta, foreign delta, global signed-volume delta, and local/global residual so the exact volume contributor can be identified.
- [x] Record bevel-plane normal, final bevel-face normal, their agreement, plane distance, solid-centre side, both source-edge endpoint sides, and rail-plane residual.
- [x] Triangulate topology-valid shells even when retained-volume certification fails, audit the diagnostic triangle soup, and report independent signed/absolute triangle volume plus polygon/triangle deltas.
- [x] Keep preview adoption gated by the existing bounds and retained-volume rules; diagnostic triangulation of a rejected shell must not mark it valid or publish it.
- [x] Preserve all earlier telemetry in the same single physical-evaluation record.
- [x] Keep rail solving, clipping, face construction, candidate selection, production generation, River, scenes, prefabs, materials, shaders, tags, layers, and recipes unchanged.

### EW-B1.5R2 purpose

The bounded construction is topologically complete but edges `6`, `7`, and `11` all add measurable volume. This patch determines whether those edges are actually convex, whether the generated shell escapes the original solid, and exactly which replacement or generated face contributes the increase. It is evidence collection before any candidate filtering or geometry correction.

### EW-B1.5R2 Unity exit criteria

- [x] Edges `6`, `7`, and `11` report `Convex`; the complete eligible pool reports `18` convex and zero concave, coplanar, ambiguous, or invalid-orientation candidates.
- [x] `boundedSolid` reports zero source-convexity and result-containment violations for edges `6`, `7`, and `11`.
- [x] `boundedLocalVolume` attributes the positive delta primarily to endpoint-cap contributions and closes to the global delta within approximately `1.8E-04` before cap removal.
- [x] `boundedVolumeCrossCheck` produces valid `98`-triangle diagnostic soups for edges `6`, `7`, and `11` and agrees with polygon volume within approximately `1.1E-07`.
- [x] `boundedBevelPlane` reports near-unit normal agreement, zero/negligible rail residual, positive source-edge side, and negative solid-centre side for every tested edge.

### EW-B1.5R2 methods decision

- [x] Accepted: collect classification and containment evidence before changing candidate eligibility or rail geometry.
- [x] Accepted: retain the AABB audit as coarse evidence, but do not treat it as source-solid containment.
- [x] Accepted: triangulate rejected shells diagnostically without publishing them.
- [x] Rejected: raising the retained-volume threshold or treating edge `11` as geometrically correct merely because it narrowly passes.


## EW-B1.6 — Endpoint support-face clipping and cap removal

- [x] Replace the obsolete endpoint-cap closure with direct clipping of the two endpoint-adjacent support faces.
- [x] Require the two rails at each endpoint to resolve to one exact shared support face and to the two graph edges incident to the removed source vertex.
- [x] Replace each endpoint source vertex with its ordered rail pair, preserving the support face's original winding and analytical plane.
- [x] Emit exactly one bounded bevel quad and zero `BoundedEndpointCap` polygons.
- [x] Require exactly two owner-face clips, two endpoint-support clips, four rail insertions, two removed endpoint vertices, and four intentionally modified source faces.
- [x] Reject unsupported endpoint valence or provenance layouts explicitly instead of guessing a closure.
- [x] Add result-global convexity certification against every result face plane.
- [x] Add non-adjacent face-intersection and coplanar-overlap certification using the existing directed triangle intersection implementation.
- [x] Expand local signed-volume attribution to separate original/replacement endpoint-support faces while retaining obsolete cap contributions as zero-valued historical evidence.
- [x] Tighten the bounded prototype's upper retained-volume ratio from `1.0001` to `1.0`; a certified bevel must not add material.
- [x] Record endpoint source/rail positions, support normals, edge parameters, edge residuals, and support-plane residuals in the cumulative single-record audit.
- [x] Keep isolated rail solving, owner clipping, bevel width, candidate selection, production generation, River, scenes, prefabs, materials, shaders, tags, layers, and recipes unchanged.

### EW-B1.6 problem and intended correction

EW-B1.5R2 proved the tested edges are convex, the source shell is convex, all result vertices remain inside the original solid, and the bevel plane is correctly inset. The remaining positive volume and visible inward triangular crease came from a different structural error: each endpoint cap duplicated the corner region still retained by the unchanged endpoint support face. The correct full-edge bevel removes the original endpoint vertex from that support face and lets the support face share the bevel's endpoint rail edge directly. No separate endpoint cap is required.

### EW-B1.6 Unity exit criteria

- [x] Edges `6`, `7`, and `11` report `endpointSupport.clipped:2`, `removedVertices:2`, `railInsertions:4`, `bevelFaces:1`, and `endpointCaps:0`.
- [x] Construction modifies exactly two owner and two endpoint-support faces; the remaining `boundaryOnlyUnexpectedSourceFaces:2` blocker is now proven to come from comparing the prepared result against the raw source baseline.
- [ ] Polygon topology, result-global convexity, and source-solid containment report zero failures; face-intersection acceptance remains blocked until source-baseline pairs are separated from newly introduced pairs.
- [ ] `resultVolume < preparedSourceVolume` is proven for edges `6`, `7`, and `11`; triangulation and preview remain blocked by invalid raw-baseline audit gates.
- [ ] Wireframe and shaded views show one flat bevel band with no large inward triangle, duplicated corner, or multi-surface crease.

### EW-B1.6 methods decision

- [x] Accepted: endpoint support-face clipping is the correct bounded closure for a full-edge bevel on the current three-valent convex topology.
- [x] Rejected: preserving the full support face and covering its corner with a coplanar endpoint-cap triangle.
- [x] Rejected: accepting edge `11` solely because duplicated geometry happened to remain below the retained-volume threshold.
- [x] Deferred: higher-valence endpoint reconstruction remains a later bounded multi-edge problem and must fail explicitly in the single-edge prototype.


### EW-B1.6 Unity result

- [x] Endpoint-cap removal corrected the volume direction: edges `6`, `7`, and `11` retain ratios `0.999938342`, `0.999936028`, and `0.999909296` respectively.
- [x] All three completed shells report zero open edges, non-manifold edges, T-junctions, invalid faces, source-solid escapes, and global-convexity violations.
- [x] Local volume attribution closes to the global subtraction within approximately `5.4E-08`, proving the support-face replacement and cap removal account correctly.
- [ ] No shell reached triangulation because source-face modification certification compared the prepared result against the raw source and counted two shared numerical boundary repairs as unexpected changes.
- [ ] The result-only intersection gate reported pairs without auditing the prepared source baseline; repeated untouched pair `SourceFace:9~SourceFace:14` proves the raw result count cannot be treated as bevel-introduced evidence.

## EW-B1.6R1 — Prepared-baseline source changes and intersection-delta certification

- [x] Preserve the raw source-versus-result face-change comparison as historical evidence, but make prepared source-versus-prepared result the authoritative modification gate.
- [x] Report raw and prepared owner, support, unexpected, boundary-only, foreign-modification, and foreign-boundary-subdivision counts separately.
- [x] Run the same directed-triangle face-intersection audit over both the prepared source shell and prepared bounded result.
- [x] Key intersection pairs by exact face provenance rather than transient post-preparation list index.
- [x] Record every reported pair with source/result face indices, coplanar classification, shared-vertex count, shared-boundary-edge count, source-graph adjacency, and boundary-contact classification.
- [x] Partition result pairs into unchanged baseline, changed baseline, newly introduced, and resolved sets; preserve the complete pair evidence for every set in the single physical-evaluation record.
- [x] Reject preview adoption only for newly introduced or materially changed improper interior intersections. Baseline pairs and pairs with actual shared boundary contact remain evidence but are not automatically bevel failures.
- [x] Keep endpoint-support geometry, rail positions, bevel width, candidate eligibility, volume limits, production generation, River, scenes, prefabs, materials, shaders, tags, layers, and recipes unchanged.

### EW-B1.6R1 purpose

EW-B1.6 produced the intended subtractive one-quad/no-cap shell but could not display it because two certification gates lacked equivalent source baselines. R1 corrects those gates so the current geometry can reach exact triangulation when it introduces no new interior intersection, while retaining the raw comparisons that exposed the mismatch.

### EW-B1.6R1 Unity exit criteria

- [ ] `boundedSourceChanges` reports `baseline:prepared`, four prepared modifications, and zero prepared unexpected/boundary-only foreign changes while retaining the raw two-face numerical difference.
- [ ] `boundedFaceIntersections` reports source and result pair sets plus `introducedInterior:0` for at least one representative edge.
- [ ] At least one of edges `6`, `7`, or `11` reaches nonzero triangle emission and `valid:1`.
- [ ] Shaded and wireframe inspection determines whether the original inward multi-surface crease is actually removed by endpoint-support clipping.

### EW-B1.6R1 methods decision

- [x] Accepted: certification must compare numerically equivalent prepared states.
- [x] Accepted: intersection validity is a source-to-result delta, not an absolute result pair count.
- [x] Accepted: preserve exact provenance-pair evidence cumulatively in one record instead of adding per-pair Console messages.
- [x] Rejected: changing bevel geometry again before the corrected shell is allowed to triangulate and render.

### EW-B1.6R1 Unity result

- [x] Prepared source and prepared result intersection audits both ran and confirmed `introducedInterior:0` for edges `6`, `7`, and `11`.
- [x] The repeated physical source contact `9~14` appeared as `None:-1~None:-1` in the source audit and `SourceFace:9~SourceFace:14` in the result audit, proving that the prepared source baseline had lost source-face provenance before delta matching.
- [x] The prepared source-change comparison reported `preparedModified:0`, `preparedOwnerModified:0`, and `preparedSupportModified:0` despite two successful owner clips and two successful endpoint-support clips.
- [x] The false zero-modification result is caused by `AuditBoundedSourceFaceChanges` skipping baseline faces whose provenance is not `SourceFace:i`; this is an identity/certification failure, not a bevel-geometry failure.
- [x] No candidate reached triangulation because the exact four-face modification gate refused the provenance-incomplete comparison.

## EW-B1.6R2 — Provenance-preserving prepared source baseline

- [x] Clone the untouched source shell with `assignSourceFaceProvenance:true` before source-baseline preparation.
- [x] Prepare the attributed source clone through the existing deterministic bounded preparation pipeline without changing its geometry.
- [x] Use the attributed raw clone for the retained raw source-face comparison while keeping the original raw source shell authoritative for bounds, volume, containment, and source geometry.
- [x] Preserve source-face provenance through prepared-source change comparison and prepared-source intersection auditing so physical pairs match by `SourceFace:i` identity.
- [x] Add independent provenance completeness audits for the attributed raw source, prepared source, and prepared bounded result.
- [x] Record expected source-face count, total faces, source-provenance faces, unique valid identities, missing identities, duplicates, out-of-range identities, non-source faces, null faces, and the first failing identity in each category.
- [x] Require exactly one valid `SourceFace:i` record for every original source face in all three audited states before source-change or intersection-delta certification can pass.
- [x] Fail explicitly on missing, duplicate, out-of-range, or null source records instead of silently reporting zero modifications.
- [x] Keep rails, owner clipping, endpoint-support clipping, bevel emission, volume limits, candidate eligibility, production generation, River, scenes, prefabs, materials, shaders, tags, layers, and recipes unchanged.

### EW-B1.6R2 purpose

EW-B1.6 created the intended subtractive one-quad/no-cap shell, and R1 corrected the comparison domains, but the prepared source baseline still had no stable `SourceFace:i` identities. That caused the face-change audit to skip every baseline face and caused identical source/result intersection contacts to appear simultaneously new and resolved. R2 restores exact source identity before preparation and certifies that identity set explicitly. Its sole purpose is to let the already-built geometry reach triangulation only when the prepared comparison is real.

### EW-B1.6R2 Unity exit criteria

- [x] `boundedSourceProvenance.certified:1` for edges `6`, `7`, and `11`.
- [x] Raw and prepared source provenance each report `expected:16`, `uniqueValid:16`, `missing:0`, `duplicates:0`, and `outOfRange:0`; the result reports the same source identity set plus one non-source bevel face.
- [x] `boundedSourceChanges` reports four prepared modifications: two owner faces, two endpoint-support faces, and zero unexpected foreign changes.
- [x] The baseline pair `SourceFace:9~SourceFace:14` matches as unchanged rather than one new plus one resolved pair.
- [x] Edges `6`, `7`, and `11` each emit `90` triangles across `17` faces and reach `valid:1`; visual inspection remained inconclusive because the primary inspector action still displayed the rejected plane-cut preview.

### EW-B1.6R2 methods decision

- [x] Accepted: provenance is part of the audit state and must be established before numerical preparation, not inferred afterward from transient list order.
- [x] Accepted: hard completeness and uniqueness certification prevents another silent zero-evidence result.
- [x] Accepted: preserve both raw and prepared comparisons, but give both the same stable face identities.
- [x] Rejected: changing bevel geometry again while identity loss is the only proven blocker.

### EW-B1.6R2 Unity result

- [x] Edges `6`, `7`, and `11` each produced one bounded bevel face, zero endpoint caps, two owner modifications, two endpoint-support modifications, zero unexpected source modifications, and `valid:1`.
- [x] Each result emitted `90` triangles across `17` faces with zero open, non-manifold, winding, bounds, or volume failures.
- [x] Result volume became strictly subtractive: prepared ratios were `0.999938342`, `0.999936028`, and `0.999909296`.
- [x] Source/result intersection evidence matched stable provenance correctly; baseline contact `SourceFace:9~SourceFace:14` remained unchanged and edge `6` added only one legal graph-adjacent boundary contact.
- [x] The per-edge bounded geometry is accepted geometrically and diagnostically.
- [ ] Visual acceptance remained unproven because the main inspector action still enabled and displayed the superseded whole-rock plane-cut mesh rather than the certified bounded mesh.

## EW-B2 — Unified all-edge bounded hull preview and inspector consolidation

- [x] Replace the visible multi-button preview workflow with one `Rebuild Edge-Wear Bevel Preview` action.
- [x] Run corner and legacy plane-cut diagnostics from that action, but never publish the rejected plane-cut mesh.
- [x] Evaluate every eligible edge through the isolated bounded rail solver in one operation.
- [x] Build one combined point cloud from untouched source vertices and active bounded rail points rather than stacking complete single-edge replacement rocks.
- [x] Attempt one shared convex-hull reconstruction so affected source faces and vertex junctions are generated once.
- [x] Preserve one cumulative all-edge telemetry result with candidate, rail, hull, preparation, topology, containment, volume, and triangulation evidence.
- [x] Remove obsolete visible previous/next, single-edge, legacy, and duplicate preview controls.
- [x] Keep production generation, runtime behavior, River, scenes, prefabs, materials, shaders, tags, layers, and recipes unchanged.

### EW-B2 purpose

The certified single-edge construction could not become the normal preview by repeatedly replacing the whole rock once per edge. Adjacent bevels share source faces and vertices. EW-B2 therefore introduced one authoritative inspector action and a combined reconstruction stage intended to produce one rock containing all feasible bounded bevels.

### EW-B2 Unity result

- [x] The one-button action ran the corner diagnostic, plane diagnostic, and unified bounded evaluation together.
- [x] Candidate evaluation found `18` convex candidates; `15` solved isolated rails and `3` were rejected locally.
- [x] The combined point cloud contained `74` unique points.
- [ ] No combined polygon was emitted: the first hull iteration returned before preparation with `faces:0` and `triangles:0`.
- [ ] The exact hull failure was not recoverable from the Console record because hull counters were assigned only after both plane and face construction succeeded.
- [ ] The exhaustive rail/point evidence was appended before `boundedTrace`, causing Unity Console truncation before the decisive blocker.
- [x] The `active:0` field was misleading rather than geometric evidence: active plans were counted only after hull construction, despite individual records already reporting `state=active`.

### EW-B2 methods decision

- [x] Accepted: one authoritative inspector operation and one displayed combined result.
- [x] Accepted: combined reconstruction must operate on shared source geometry, not merge complete isolated replacement meshes.
- [x] Partially accepted: the point-cloud convex-hull experiment reached candidate and point-cloud construction but has not yet produced a face.
- [x] Rejected: a single enormous Console line as the storage format for exhaustive telemetry.
- [x] Rejected: assigning decisive counters only after downstream stages succeed.
- [x] Deferred: changing the hull algorithm until the exact failed substage, plane, and facet evidence is available.

## EW-B2.1 — Hull failure localization and non-truncating telemetry

- [x] Add explicit stage tracking for candidate evaluation, point-cloud construction, plane extraction, facet ordering, facet sanitation, facet classification, preparation, topology certification, triangulation, and completion.
- [x] Put `stage`, `failureStage`, and the exact blocker at the beginning of the Console record.
- [x] Count active rail-solved plans before hull reconstruction so an early hull return cannot falsely report `active:0`.
- [x] Record point-cloud dimensional rank and exact bounds.
- [x] Record plane-extraction triples tested, degenerate triples, supporting triples, straddling triples, created planes, merged planes, pre-prune planes, under-supported planes removed, and final planes even when extraction fails.
- [x] Record planes attempted, faces completed, failed plane index/normal/distance/support-point count, ordered/sanitized vertex counts, facet area, convexity result, and exact facet failure reason.
- [x] Emit one bounded high-value Console summary containing the decisive stage, blocker, hull counters, preparation, topology, volume, mesh, and diagnostic status.
- [x] Rewrite the complete verbose point, face, rail, plane-diagnostic, provenance, and intersection evidence to `Library/GeneratedMassEdgeWearTelemetry.txt` on every evaluation.
- [x] Report telemetry-file write success or the exact write exception in the Console summary.
- [x] Keep the combined geometry algorithm, rail solving, candidate eligibility, inspector workflow, production generation, River, scenes, prefabs, materials, shaders, tags, layers, and recipes unchanged.

### EW-B2.1 purpose

EW-B2 failed inside combined hull reconstruction, but its telemetry could not distinguish plane extraction from facet construction and the decisive blocker was truncated. B2.1 changes no geometry. It makes every early return self-identifying and preserves exhaustive evidence outside the Console while keeping one concise, copyable summary per physical evaluation.

### EW-B2.1 Unity exit criteria

- [ ] One click produces a Console record whose opening fields identify the exact final stage and blocker.
- [ ] `pointCloud.rank` and the complete plane-extraction counters remain populated on an early hull failure.
- [ ] If plane extraction succeeds, facet counters identify the exact failed plane and whether ordering, sanitation, or convexity failed.
- [ ] `Library/GeneratedMassEdgeWearTelemetry.txt` is rewritten and contains the complete non-truncated hull points, hull faces, edge results, and retained diagnostic evidence.
- [ ] The next geometry patch is chosen only from the localized failure evidence.


### EW-B2.1 Unity result — zero-normal fake hull plane localized

- [x] One click produced an untruncated decisive Console summary and rewrote `Library/GeneratedMassEdgeWearTelemetry.txt` successfully.
- [x] Candidate evaluation remained stable: `18` convex candidates, `15` rail-solved edges, `3` local rail rejections, and `15` active combined plans.
- [x] The point cloud remained three-dimensional with `74` unique points.
- [x] Plane extraction tested `64824` triples, created `43` merged plane records, and reached facet construction.
- [x] Facet construction completed `11` planes and failed on plane index `11`.
- [x] The failed plane reported `normal:(0/0/0)`, `distance:0`, and all `74` points as supporting points; this is invalid plane data, not a legitimate difficult hull facet.
- [x] The failure is caused by a threshold mismatch: the pre-normalization degeneracy gate accepted a very small cross product that Unity normalization collapsed to zero.
- [x] Rejected interpretation: the 74-point bounded cloud itself is incapable of producing a convex hull.

### EW-B2.1 methods decision

- [x] Accepted: the diagnostic localization patch succeeded and identified one exact numerical failure.
- [x] Accepted: exhaustive evidence remains in the telemetry file while the Console carries the blocker first.
- [x] Rejected: changing rail geometry, candidate eligibility, or combined-hull architecture in response to a malformed zero-normal seed plane.

## EW-B2.2 — Normalization-safe hull-plane extraction

- [x] Replace implicit `Vector3.Normalize()` during hull-plane extraction with explicit finite magnitude measurement and division.
- [x] Add a scale-aware minimum cross-magnitude threshold with a hard floor equal to `PointMergeDistance`, preventing vectors within Unity's normalization dead zone from becoming plane seeds.
- [x] Preserve the historical clearly-degenerate triple count and separately count near-degenerate triples rejected by the stronger normalization-safe threshold.
- [x] Record total normalization rejections, post-normalization invalid triples, threshold value, rejected cross-magnitude range, and minimum accepted cross magnitude.
- [x] Require every candidate plane normal to be finite and unit length, its distance to be finite, and its support set to contain at least three points before insertion or merging.
- [x] Retain the seed point indices and seed cross magnitude for every final hull plane, plus the minimum and maximum seed magnitude merged into that plane.
- [x] Add a final plane-list invariant pass before facet ordering. It verifies finite unit normals, finite distances, in-range support points, support residuals, and non-degenerate planar support rank.
- [x] Fail at `PlaneExtraction` with the exact first invalid plane, seed triple, seed magnitude, and reason if any malformed plane survives candidate validation.
- [x] Write complete per-plane normal, distance, support, seed, and magnitude evidence to the existing non-truncating telemetry file.
- [x] Keep rail solving, candidate eligibility, hull point generation, facet ordering, bevel geometry, inspector workflow, production generation, River, scenes, prefabs, materials, shaders, tags, layers, and recipes unchanged.

### EW-B2.2 purpose

EW-B2.1 proved that the current combined-hull blocker was a fake plane with a zero normal and all 74 cloud points. The previous code rejected only cross products below approximately `1E-6`, then called Unity normalization, whose practical zero threshold is larger. B2.2 aligns the extraction gate with explicit normalization, prevents malformed planes from entering the list, and makes plane invariants authoritative before facet construction. It is a numerical correction, not a bevel redesign.

### EW-B2.2 Unity exit criteria

- [ ] `planeExtraction.normalizationRejected` is nonzero when near-collinear seed triples exist, while `postNormalizationInvalid:0` is expected.
- [ ] `planeExtraction.invalidRemoved:0` and no zero/non-unit plane reaches facet ordering.
- [ ] The final plane count excludes the former all-point zero-normal plane.
- [ ] Hull reconstruction advances beyond the former failure at facet plane `11`, or reports the next exact real facet blocker with valid normal and seed evidence.
- [ ] `Library/GeneratedMassEdgeWearTelemetry.txt` includes one `hullPlanes` record per retained plane with seed and support evidence.

### EW-B2.2 Unity result — numerical hull fix accepted, combined architecture rejected

- [x] Normalization-safe plane extraction removed the zero-normal fake plane and completed hull reconstruction with `stage:Complete`, `faces:17`, `triangles:90`, and `triangleSoupValid:1`.
- [x] Plane extraction reported `normalizationRejected:29`, `postNormalizationInvalid:0`, and `invalidRemoved:0`; the EW-B2.1 numerical diagnosis was correct.
- [x] The combined hull retained only one of fifteen rail-solved bevels: `railSolved:15`, `hullSuppressed:14`, `active:1`, `bevelFaces:1`.
- [x] The final 31-point cloud and volume matched the earlier isolated edge-11 result, proving the supposed all-edge result had collapsed to one surviving isolated bevel.
- [x] Rejected: suppressing requested bevel planes until a partial convex hull becomes valid. A complete all-edge result may not report success while fourteen solved edges are discarded.
- [x] Rejected: the point-cloud convex-hull shortcut as the multi-edge production architecture. Shared source faces and shared vertices require topology-driven reconstruction.
- [ ] Multi-edge reconstruction remains blocked. No further all-edge work proceeds until the local bevel surface itself satisfies the one-surface visual contract.

## EW-B1.7 — One planar bevel surface render contract

### Corrected diagnosis

The visible defect is not four separately attributed bevel polygons. The certified `BoundedEdgeBevel` is one four-vertex polygon, but `TryTriangulateBoundedPreviewFaces` emits one centre-fan triangle per boundary edge. A quad therefore becomes four render triangles meeting at an inserted centre vertex. `BuildMeshData` then ignores `PolygonFace.Normal`, recalculates one normal per triangle, and Unity recalculates normals again because Generated Mass mesh data supplies no explicit normals. It also hashes surface variation from the duplicated triangle-soup vertex index, so the same polygon receives discontinuous colour/mask values across its internal fan edges. On the long narrow bevel, the four triangles therefore read as four separate folded surfaces even when their analytical polygon normal is shared.

`bevelFaces:1` therefore proved only polygon provenance. It did not prove one rendered surface. The numbered four-face screenshot is the exact centre-fan decomposition of the single bevel quad:

```text
one bevel polygon
→ four centre-fan triangles
→ four independently calculated normals
→ visible /\/\ crease
```

### Required implementation

- [x] Special-case `BoundedEdgeBevel` polygon triangulation so a convex boundary emits `boundaryVertexCount - 2` direct triangles and never inserts a centre fan vertex.
- [x] A four-vertex bevel region must emit exactly two triangles rather than four.
- [x] Carry the authoritative `PolygonFace.Normal` through `TriangleSoup` for every bevel-region triangle.
- [x] Carry one authored surface-group key across every triangle of the same bevel polygon so duplicated triangle-soup vertices resolve identical surface variation and material masks.
- [x] Write explicit normals into `MeshData` for every Generated Mass render vertex. Non-authored triangles retain their existing geometric flat normal; bevel triangles share one authored plane normal.
- [x] Orient direct bevel triangles against the authoritative polygon normal before emission.
- [x] Reject a bevel region whose complete boundary exceeds one-plane tolerance.
- [x] Record cumulative region evidence: polygon count, boundary vertices, emitted triangles, authored-normal triangles, internal fan vertices, maximum plane residual, maximum geometric-normal deviation, exact failed face/provenance, and render validity.
- [x] Require `boundedBevelRegion.renderValid:1` before a bounded preview can pass.
- [x] Keep rail solving, owner/support clipping, hull-point selection, candidate selection, bevel width, production generation, River, scenes, prefabs, materials, shaders, tags, layers, and recipes unchanged.

### EW-B1.7 purpose

The complete outlined red region must read as one flat bevel plane. GPUs still require triangles, but those triangles are an invisible implementation detail: they must cover one polygonal region, introduce no centre vertex, and use one identical authored normal. This patch addresses the original inward four-surface crease directly. It does not attempt to solve the separate all-edge suppression problem.

### EW-B1.7 Unity exit criteria

- [ ] The current four-vertex bevel reports `polygonFaces:1`, `boundaryVertices:4`, `triangles:2`, `authoredNormalTriangles:2`, `authoredSurfaceGroupTriangles:2`, and `internalFanVertices:0`.
- [ ] `maxPlaneResidual` remains within tolerance and `renderValid:1`.
- [ ] The previously numbered four triangular surfaces visually collapse into one uniformly lit planar bevel region with no radial centre junction and no inward crease.
- [ ] The unified audit must continue to state honestly that the current point-cloud experiment has only one active bevel when fourteen rails are suppressed; EW-B1.7 does not accept that as an all-edge solution.

### EW-B1.7 methods decision

- [x] Accepted: one polygon is not sufficient evidence; polygon triangulation and rendered normals are part of the bevel geometry contract.
- [x] Accepted: direct convex triangulation with one shared authored normal is the correct render representation for a flat bevel polygon.
- [x] Rejected: centre-fan triangulation for a long narrow bevel quad.
- [x] Rejected: relying on Unity normal recalculation for a polygon intended to remain one authored plane.
- [x] Rejected: continuing shared all-edge reconstruction before the exact local one-surface requirement is visually proven.

### EW-B1.7 Unity result — local one-surface bevel accepted

- [x] The previously defective four-vertex bevel reports `polygonFaces:1`, `boundaryVertices:4`, `triangles:2`, `authoredNormalTriangles:2`, `authoredSurfaceGroupTriangles:2`, `internalFanVertices:0`, `maxPlaneResidual:0`, and `renderValid:1`.
- [x] Triangle-soup output fell from `90` to `88`, exactly matching removal of the two surplus centre-fan triangles.
- [x] Shaded inspection confirms the numbered four-way inward crease collapsed into one flat, uniformly lit bevel surface.
- [x] Root cause closed: the original `/\/\` appearance was one analytical bevel polygon rendered as four independently shaded centre-fan triangles, not four authoritative bevel polygons.
- [x] The remaining blocker is now exclusively whole-rock coverage: the point-cloud all-edge experiment still suppresses fourteen of fifteen rail-solved bevels.

## EW-B3 — Authoritative whole-rock all-edge one-surface bevel rebuild

### Purpose

The primary rebuild button must rebuild the complete Generated Mass with every simultaneously feasible selected edge represented by one bounded bevel polygon. EW-B2's point-cloud convex-hull shortcut is retired from the active path because it discarded fourteen solved bevel planes and returned an isolated one-edge result. EW-B3 promotes the already-certified edge-only plane shell as the whole-rock builder now that EW-B1.7 has fixed the actual four-surface rendering defect.

The earlier visual rejection of the edge-plane shell is reclassified. Its compact audit already proved `planeBand.single == planesBuilt`, with zero split, interrupted, foreign-cut, overlong-junction, or collapsed bands. The apparent multiple surfaces were caused by centre-fan triangulation and per-triangle normals/material variation, which EW-B1.7 corrected.

### Required implementation

- [x] Stop invoking `AuditBoundedAllEdgesBevel` from the authoritative inspector action. The point-cloud hull and its edge-suppression loop are no longer part of normal evaluation.
- [x] Run the shared corner-aware width solution once for all selected edges.
- [x] Build one complete edge-only shell by clipping the source convex solid with every retained selected-edge bevel plane.
- [x] Preserve deterministic conflict attribution. A geometrically incompatible edge may be explicitly deferred, but no solved edge may disappear through hull suppression.
- [x] Require every built edge to retain exactly one bevel-band polygon: `BandRetainedEdgeCount == PlanesBuilt` and `BandSingleFaceCount == PlanesBuilt`.
- [x] Apply the EW-B1.7 one-planar-surface triangulation contract to both `BoundedEdgeBevel` and `EdgeBevelPlane` provenance.
- [x] Triangulate each bevel polygon directly from an existing boundary vertex, use one authored polygon normal and one authored surface-group identity, and emit no centre fan vertex.
- [x] Search for a stable direct-fan boundary anchor when a bevel polygon contains more than four vertices or collinear boundary subdivisions.
- [x] Require the number of certified one-surface bevel polygons to equal `PlanesBuilt` before preview adoption.
- [x] Publish the complete all-edge edge-plane shell as the only displayed preview from `Rebuild Edge-Wear Bevel Preview`.
- [x] Keep one inspector button and one cumulative all-edge audit record.
- [x] Write detailed audit evidence to `Library/GeneratedMassEdgeWearTelemetry.txt`.
- [x] Report active, built, deferred, and rejected source-edge indices explicitly; deferred edges remain visible evidence rather than silent omission.
- [x] Keep production geometry, River integration, scenes, prefabs, materials, shaders, tags, layers, and serialized recipes unchanged.

### EW-B3 validity contract

A preview may report geometry validity only when:

```text
planesBuilt > 0
planesBuilt + planesDeferred == activeEdges
planesRejected == 0
bandRetainedEdges == planesBuilt
bandSingleFaces == planesBuilt
bandSplit == 0
bandInterrupted == 0
bandForeignCut == 0
bandCollapsed == 0
oneSurfaceFaces == planesBuilt
oneSurfaceRenderValid == 1
internalFanVertices == 0
open == 0
nonManifold == 0
tJunction == 0
triangleSoupValid == 1
```

`materializedCoverage` is reported separately. It is `1` only when every active selected edge is built. A conflict-deferred edge may still permit a diagnostic preview, but it must be named in `deferred:{...}` and the inspector must show a warning.

### EW-B3 methods decision

- [x] Accepted: the edge-only plane shell is the authoritative simultaneous all-edge reconstruction for the current convex Generated Mass topology.
- [x] Accepted: an infinite mathematical cut plane is safe only when the retained bounded cap passes the existing single-band, source-containment, bounds, volume, topology, and one-surface render contracts.
- [x] Accepted: EW-B1.7 invalidated the prior visual diagnosis of multiple analytical bevel faces; the defect was render triangulation and normal/material discontinuity.
- [x] Rejected: the point-cloud hull and iterative `HullSuppressed` fallback as an all-edge builder.
- [x] Rejected: reporting a one-edge partial hull as a valid whole-rock preview.
- [x] Rejected: retaining a separate all-edge hull pass merely as routine telemetry after it has been disproven and removed from the authoritative path.
- [x] Deferred: resolving the currently conflict-deferred edge through coordinated local width reduction is a later coverage improvement, not a prerequisite for displaying the fourteen already certified simultaneous bevels.

### EW-B3 Unity exit criteria

- [ ] One click emits `GeneratedMass all-edge bevel rebuild audit` and does not emit or execute the point-cloud hull audit.
- [ ] The current reference rock reports approximately `active:15`, `built:14`, `deferred:1`, `rejected:0`, `surfaceFaces:14`, `surfaceRenderValid:1`, and `internalFanVertices:0`.
- [ ] The displayed rock visibly contains all built bevels rather than one isolated bevel.
- [ ] Every visible bevel band reads as one planar surface with no centre-fan crease.
- [ ] The deferred edge index is present explicitly in the audit and inspector warning.


### EW-B3R1 — telemetry string-literal compile correction

- [x] Unity compilation exposed malformed multiline string literals in `MassGenerator.EdgeWear.Diagnostics.Logging.cs` inside `LogUnifiedAllEdgeBevelAudit`.
- [x] Root cause: newline characters were written directly across ordinary C# quoted string literals instead of being represented as escaped `\n` sequences.
- [x] Replace every malformed telemetry newline with an explicit `\n` escape; no geometry, solver, preview, audit semantics, or inspector behavior changed.
- [x] Parse every C# file included in the EW-B3 patch with the C# tree-sitter grammar and require zero syntax errors before packaging EW-B3R1.
- [x] Add a dedicated lexical scan across every changed C# file for raw newlines inside ordinary string literals, unterminated strings/comments, and unbalanced delimiters.
- [x] Preserve the project CRLF line-ending convention.
- [ ] Unity must compile the corrected file before EW-B3 geometry validation resumes.

The original EW-B3 archive is rejected because it was distributed with basic C# syntax errors. Syntax-tree and lexical validation are mandatory for every later code patch; delimiter-only checks are not sufficient.

## EW-B3 Unity result — whole-rock shell reaches coverage but fails certification

- [x] The authoritative edge-plane shell selected `18` candidate edges, activated and built all `15` positive-width planes, deferred `0`, rejected `0`, and reported `materializedCoverage:1`.
- [x] The shell did not reach surface triangulation: `surfaceFaces:0`, `surfaceTriangles:0`, and `meshTriangles:0` are downstream consequences of polygon certification failure, not evidence that the fifteen planes were omitted.
- [x] Final polygon topology reported `open:4`, `nonManifold:0`, `tJunction:0`, and `invalidFaces:0`.
- [x] Final face-quality certification reported at least one `ConvexEdgeWear` polygon exceeding either the authored-plane residual tolerance or the `0.75°` triangle-normal-spread tolerance.
- [x] The existing two-line summary is rejected as insufficient: it records only aggregate counts and the generic blocker, but not the failed face, source edge, measured value, threshold, failure-introduction stage, open-edge ownership, expected neighbour, or shared-vertex junction coverage.
- [x] No geometry correction is authorized from this record because the exact responsible face and first failing construction stage remain unidentified.

## EW-B3.1 — Stage timeline and exact failure dossiers

### Purpose

EW-B3.1 changes no bevel, width, clipping, conflict, or triangulation geometry. It makes one rebuild answer exactly where the current all-edge shell first becomes invalid and which stable generated entity is responsible. Telemetry remains one Console record plus one overwritten structured file; it is extensive but capped and failure-oriented rather than a full successful-geometry dump.

### Required implementation

- [x] Capture the same invariant set after `AfterPlaneConstruction`, `AfterSanitation`, `AfterWeld`, `AfterBoundaryConformity`, `AfterSeamRepair`, and `FinalCertification`.
- [x] Each stage records faces, total vertices, unique vertices, edge-bevel faces, junction faces, open edges, non-manifold edges, T-junctions, invalid faces, non-planar bevel faces, maximum authored-plane residual, and maximum triangle-normal spread.
- [x] Record `firstOpenEdgeStage` and `firstNonPlanarStage` rather than inspecting only the final shell.
- [x] For every failed bevel face, retain stable provenance, source-edge index, face index, boundary vertex count, authored and measured normals, authored plane distance, maximum residual and threshold, offending vertex/position/signed residual, maximum normal spread and threshold, offending boundary segment and triangle normal, area, minimum edge length, all vertex residuals, first failing stage, and whether boundary conformity or seam repair touched it.
- [x] For every final open edge, retain owner face provenance, endpoints, length, nearest source vertex, incident built edges, expected junction/boundary neighbour, junction-face count, nearest reversed boundary segment and mismatch distance, first open stage, and classified cause.
- [x] For every source vertex touched by built bevel planes, retain incident built-edge indices, whether multiplicity requires junction consideration, emitted junction-face count, assigned open-edge count, and exact coverage failure reason.
- [x] Keep the Console record bounded to the first three face failures, first four open edges, and first three failed junction-coverage records, with omitted counts when caps are exceeded.
- [x] Put `primaryFailure` before secondary counters so the failed stage, stable face identity, cause, measured value, and threshold survive copying.
- [x] Rewrite `Library/GeneratedMassEdgeWearTelemetry.txt` with structured sections for evaluation summary, stage timeline, every face-quality failure, every open edge, every touched vertex, and preparation movement. Successful faces are summarized rather than expanded vertex-by-vertex.
- [x] Preserve one physical Console record and no per-face logging spam.
- [x] Keep production geometry, River, scenes, prefabs, materials, shaders, tags, layers, recipes, and inspector controls unchanged.

### EW-B3.1 Unity exit criteria

- [ ] One click identifies the exact first non-planar face by `EdgeBevelPlane:<sourceEdge>` or other stable provenance.
- [ ] The face dossier identifies whether plane residual, normal spread, or both failed and reports measured values beside their thresholds.
- [ ] The stage timeline proves whether the defect exists immediately after plane construction or is introduced by sanitation, welding, boundary conformity, or seam repair.
- [ ] All four open edges identify their owner, source vertex, incident bevel set, expected neighbour, nearest boundary mismatch, and whether a missing shared-vertex junction is implicated.
- [ ] The structured telemetry file remains readable and materially below a full successful-geometry dump.
- [ ] The next geometry patch targets only the first stage and stable entity proven by this record.

### EW-B3.1 methods decision

- [x] Accepted: layered telemetry — decisive Console summary, capped representative dossiers, and a complete structured failure file.
- [x] Accepted: stable provenance identities such as `EdgeBevelPlane:17`, `VertexJunctionPlane:8`, and `SourceFace:12` rather than transient list indices alone.
- [x] Accepted: cumulative diagnostics are retained and extended; previous topology, band, volume, surface, and edge-coverage evidence is not removed.
- [x] Rejected: another geometry hypothesis before identifying the exact failed face and first failing stage.
- [x] Rejected: either a two-line aggregate or a 16,000-line indiscriminate dump as the diagnostic format.

## EW-B3.1 Unity result — exact numerical faults identified

- [x] The first face-quality failure is `EdgeBevelPlane:17` at `AfterPlaneConstruction`; it is born invalid and is untouched by boundary conformity or seam repair.
- [x] Face `22` contains five vertices. Vertex `3` is `6.68764114E-05` off the authored bevel plane against a `1.99999995E-05` limit; normal spread is only `0.21306245°` against `0.75°` and is not the failure.
- [x] The four open-edge records form two reversed source-face seam pairs: `SourceFace:0 ↔ SourceFace:7` and `SourceFace:7 ↔ SourceFace:12`.
- [x] Every seam endpoint mismatch is only `5.96046448E-08`, far below `PointMergeDistance = 1E-05`; the edges are numerically coincident but receive different quantized `VertexKey` values.
- [x] The missing-junction summary is not the cause of these four openings: every open-edge dossier reports `incidentEdges:{none}` and `junctionExpected:0`.
- [x] Root cause A: tolerant clipping can classify a near-plane endpoint as inside, then `IntersectEdge` returns or clamps that endpoint without enforcing the analytical cut plane. The bevel cap receives an off-plane vertex.
- [x] Root cause B: `WeldSharedVertices` uses one rounded quantization bucket and performs no true radius comparison; points inside the accepted merge radius can remain separate when they straddle a rounding boundary.
- [x] Rejected: loosening the planarity or topology thresholds. Both audits correctly exposed construction defects.
- [x] Rejected: junction reconstruction as the next patch for this specific run.

## EW-B3.2 — Plane-exact intersections and true-distance welding

### Purpose

Correct the two numerical construction faults identified by EW-B3.1 without changing edge selection, widths, bevel planes, conflict policy, junction policy, one-surface rendering, or production geometry.

### Required implementation

- [x] Scope exact-intersection and distance-weld behavior to the authoritative edge-plane shell; legacy callers retain their existing behavior unless they explicitly provide numerical-repair telemetry.
- [x] Every edge-plane clipping intersection is projected onto the analytical `CutPlane` before it is added to a clipped face, cap-point set, or shared-intersection cache.
- [x] A genuine signed-distance crossing uses the analytical line-plane solution. A tolerance-only transition with no strict crossing chooses the nearer endpoint and projects it onto the cut plane instead of returning an off-plane endpoint.
- [x] Reproject every collected cap point before deduplication and certify the sanitized cap against a strict `PointMergeDistance * 0.25` residual limit before emission.
- [x] Reject a cap immediately if that strict residual contract fails; downstream cap-missing telemetry remains authoritative.
- [x] Replace single-bucket quantized welding in the authoritative shell with deterministic nearest canonical matching under `distance² <= PointMergeDistance²`.
- [x] Preserve first-point canonical ownership; do not average unrelated geometry.
- [x] Apply true-distance welding after each authoritative shell cut and again at `AfterWeld` preview preparation.
- [x] Preserve the complete EW-B3.1 stage timeline and failure dossiers.
- [x] Add cumulative numerical telemetry: intersection requests, strict crossings, projected tolerance fallbacks, cache reuse, maximum projection movement, cap residual before/after projection, cap rejection, distance-weld comparisons/matches/moved vertices, and maximum weld movement.
- [x] Keep one Console record and one overwritten structured telemetry file.
- [x] Keep River, scenes, prefabs, materials, shaders, tags, layers, recipes, inspector controls, and geometry commit unchanged.

### EW-B3.2 Unity exit criteria

- [ ] `EdgeBevelPlane:17` no longer appears in `Face Quality Failures`; `nonPlanar:0` at `AfterPlaneConstruction` and every later stage.
- [ ] The two near-miss source-face seams are welded; `open:0` at `AfterPlaneConstruction` or, at latest, `AfterWeld`, and remain zero through final certification.
- [ ] Numerical telemetry reports at least one projected fallback or nonzero projection movement for the reference rock, with `capResidualAfter <= 2.5E-06` and `capRejected:0`.
- [ ] Distance welding reports moved vertices with `maxWeldMove <= 1E-05` and final topology remains manifold with no T-junctions.
- [ ] All fifteen built bevel polygons reach the EW-B1.7 one-surface render audit.

### EW-B3.2 methods decision

- [x] Accepted: analytical plane projection is part of clipping construction, not a later cosmetic repair.
- [x] Accepted: topology equivalence uses the declared Euclidean merge radius, not quantization-bucket identity.
- [x] Accepted: retain strict planarity and topology thresholds unchanged.
- [x] Rejected: broad replacement of every project weld or clip path before the authoritative shell proves the correction.
## EW-B3.2 Unity result — topology repaired, tolerance fallback exposed as malformed sequential clipping

- [x] True-distance welding succeeded completely: topology is `open:0`, `nonManifold:0`, `tJunction:0`, `invalidFaces:0` from `AfterPlaneConstruction` through final certification.
- [x] The prior `EdgeBevelPlane:17` residual failure disappeared.
- [x] The only remaining face-quality failure moved to `EdgeBevelPlane:16`, face `21`, at `AfterPlaneConstruction`.
- [x] Face `21` has six vertices, residual `6.60419464E-05 / 1.99999995E-05`, normal spread `88.973671° / 0.75°`, and minimum edge length `6.70406152E-05`.
- [x] Numerical evidence contains exactly one tolerance fallback: `fallbackProjected:1`, with `maxProjection:6.70406152E-05`. The fallback movement and malformed minimum edge are the same scale.
- [x] The one fallback occurred when a vertex approximately `6.7E-05` outside the analytical cut plane was classified as inside by the broader clipping epsilon. The code emitted both the projected endpoint and the original tolerated endpoint, creating a tiny off-plane hook in a previously planar bevel face.
- [x] Cap construction itself remains planar: `capResidualBefore` and `capResidualAfter` are `1.1920929E-07`, `capRejected:0`.
- [x] Boundary conformity, seam repair, and distance welding are not responsible: `conformTouched:0`, `seamTouched:0`, and `maxWeldMove:1.71201307E-07`.
- [x] Missing junction caps are not a blocker for this result because the shell is already closed and every topology counter is zero.
- [x] Accepted from EW-B3.2: deterministic true-distance welding.
- [x] Rejected from EW-B3.2: tolerance-only same-side endpoint projection as valid geometry. It creates a duplicate endpoint spike and does not preserve the existing face plane.

## EW-B3.3 — Strict classification and owner-plane-preserving sequential clipping

### Purpose

Remove the sole tolerance-fallback construction path proven to create the malformed `EdgeBevelPlane:16` hook. Preserve the successful radius weld and all EW-B3.1 diagnostics. Do not change edge selection, widths, plane solving, conflict policy, junction policy, rendering, or geometry commit.

### Required implementation

- [x] In the authoritative exact shell, classify every polygon vertex with a strict three-state contract using `PointMergeDistance * 0.25`: `Inside`, `OnPlane`, or `Outside`.
- [x] Do not use the broader candidate `ClipEpsilon` to retain analytically outside vertices in finished exact-mode polygons.
- [x] Emit analytical intersections only for genuine strict `Inside ↔ Outside` transitions.
- [x] Emit no geometry for `Outside ↔ Outside` edges. Same-side projected fallbacks are forbidden and recorded as invariant failures if requested.
- [x] Canonically snap only `OnPlane` endpoints, with movement bounded by the strict classification tolerance.
- [x] Preserve the owner face plane during sequential clipping. A raw segment intersection is accepted when it satisfies both the current cut plane and the existing face’s authored plane.
- [x] If numerical correction is required, solve the closest point satisfying both planes; never project only onto the current cut plane.
- [x] Validate cached intersections against both the owner and current cut plane before reuse.
- [x] Stop reprojecting all cap points after collection. Validate them against the strict residual limit and reject the cut transaction rather than moving an existing face boundary.
- [x] Abort the current cut transaction if any exact classification, denominator, cache, owner-plane, cut-plane, or cap residual invariant fails. Do not commit partial geometry.
- [x] Retain true-distance welding after each cut and at `AfterWeld`.
- [x] Extend cumulative telemetry with strict classification counts, on-plane snaps, same-side fallback attempts, two-plane corrections, owner/cut residual before and after correction, exact construction failure count, and one first-failure dossier with stable owner/cut provenance and endpoint classifications.
- [x] Keep the full stage timeline, face dossiers, topology dossiers, bounds, volume, materialization, and one-surface evidence.

### EW-B3.3 validity contract

```text
fallbackProjected == 0
sameSideFallbackAttempts == 0
exactFailures == 0
maxCutPlaneResidualAfter <= PointMergeDistance * 0.25
maxOwnerPlaneResidualAfter <= PointMergeDistance * 0.25
capRejected == 0
open == 0
nonManifold == 0
tJunction == 0
nonPlanar == 0
```

### EW-B3.3 Unity exit criteria

- [ ] `EdgeBevelPlane:16` no longer appears in `Face Quality Failures`.
- [ ] `fallbackProjected:0`, `sameSideFallbackAttempts:0`, and `exactFailures:0`.
- [ ] `topology:0/0/0/0` remains unchanged from the successful B3.2 weld result.
- [ ] Every stage reports `nonPlanar:0`.
- [ ] All fifteen built bevel polygons reach the EW-B1.7 one-surface render audit and produce a valid preview mesh.

### EW-B3.3 methods decision

- [x] Accepted: strict classification tolerance and broad removal tolerance are different concepts and must not share one inclusion test.
- [x] Accepted: a sequential clip intersection belongs simultaneously to the current cut plane and the existing owner-face plane.
- [x] Accepted: exact-mode construction fails closed rather than synthesizing same-side geometry.
- [x] Rejected: preserving the B3.2 tolerance fallback with additional sanitation or a looser planarity threshold.
- [x] Rejected: junction reconstruction as a response to a shell that already reports zero open, non-manifold, and T-junction defects.

## EW-B3.3R1 — Legacy ClipPolygon call-site compatibility correction

- [x] Unity compilation exposed one missed six-argument `ClipPolygon` call in `MassGenerator.EdgeWear.LocalJunction.cs` after EW-B3.3 expanded the exact-mode overload.
- [x] Root cause: parser-only validation confirmed syntax but did not validate cross-file overload resolution; the historical local-junction helper still requires the legacy clipping contract.
- [x] Add a backward-compatible six-argument `ClipPolygon` overload that delegates directly to `ClipPolygonLegacy`.
- [x] Preserve EW-B3.3 exact-mode behavior for the authoritative all-edge shell; no geometry, tolerance, telemetry, or inspector behavior changes.
- [x] Validate every `ClipPolygon` declaration and invocation across the complete Generated Mass source set by parsed argument count, in addition to syntax and malformed-string checks.
- [x] Rejected: adding fabricated exact-mode provenance to the historical local-junction helper merely to satisfy the expanded signature.

### EW-B3.3R1 Unity exit criteria

- [ ] Unity compiles without `CS7036` at `MassGenerator.EdgeWear.LocalJunction.cs:464`.
- [ ] One rebuild reaches the unchanged EW-B3.3 numerical and geometry audit.

## EW-B3.3 Unity result — all selected bevels are geometrically valid

- [x] The reference rock completed with `selected:18`, `active:15`, `built:15`, `deferred:0`, `rejected:0`, `surfaceFaces:15`, `surfaceRenderValid:1`, `topology:0/0/0/0`, `faceQuality:0`, and `meshValid:1`.
- [x] Strict clipping completed with no projected fallback, no same-side fallback request, no exact construction failure, and authored-plane residuals at floating-point noise scale.
- [x] The one-surface bevel primitive and the simultaneous edge-plane shell are accepted as the authoritative all-edge geometry architecture.
- [x] A maximum-Coverage run also produced valid geometry, but only `32` of `36` selected candidates materialized and several visually relevant source edges were absent.
- [x] The remaining problem is coverage semantics and edge lifecycle, not bevel planarity, topology, triangulation, normals, or material grouping.

## EW-B4.1 — Exhaustive maximum Coverage and complete edge lifecycle

### Purpose

Make maximum `Edge Wear Coverage` mean every structurally eligible convex source edge enters the selected set. Preserve the existing artistic ranking and filtering for all lower Coverage values. Record one compact lifecycle row per source edge so every omission has an exact reason and stable source-edge index.

### Required implementation

- [x] Separate structural eligibility from artistic preference.
- [x] Structural eligibility requires exactly two owner faces, finite usable owner normals, a numerically usable segment, a certified convex classification, and a non-coplanar owner-face relationship.
- [x] Use a numerical minimum length of `max(PointMergeDistance * 4, maximumDimension * 0.00001)` rather than the previous artistic `maximumDimension * 0.015` cutoff when maximum Coverage is active.
- [x] Retain the established bounded-edge convexity classifier and its solid-centre orientation evidence for exhaustive eligibility.
- [x] At maximum Coverage, include every structurally eligible edge even when it fails the artistic length, angle, base-position, or score preference.
- [x] Below maximum Coverage, preserve the existing artistic length threshold, angle-score threshold, base suppression, score ordering, and selected-count calculation.
- [x] Map every source-edge lifecycle record to the stable topology-graph edge index before corner solving.
- [x] Record per edge: endpoints, owner faces, face count, length, dihedral angle, vertical position, structural classification, artistic eligibility, candidate reason, score, selection, solved width, width inactivity, active state, built/deferred/rejected state, and final reason.
- [x] Add a bounded Console coverage summary with exact ID sets for structural exclusions, artistic-only exclusions, width-inactive edges, deferred edges, and rejected edges.
- [x] Add `[Edge Coverage Summary]` and one approximately one-line-per-source-edge `[Edge Lifecycle]` section to `Library/GeneratedMassEdgeWearTelemetry.txt`.
- [x] Redefine `materializedCoverage` at maximum Coverage to require `structurallyEligible == selected == built`, with no width-inactive, deferred, rejected, or unmapped edge.
- [x] Relabel the explicit-junction coverage counter as a legacy non-authoritative heuristic. A closed manifold edge-plane shell does not require explicit junction-cap faces.
- [x] Preserve the accepted EW-B3.3 geometry, strict clipping, distance welding, one-surface rendering, stage timeline, failure dossiers, bounds, volume, and topology certification.
- [x] Do not add camera-visibility filtering. Coverage governs the complete generated rock.
- [x] Do not yet change coordinated width solving or locality deferral policy; those remain EW-B4.2 work after the exhaustive selected set is measured.

### EW-B4.1 validation targets

- [ ] At maximum Coverage, `coverage.max=1` and `coverage.structural == coverage.selected`.
- [ ] The previously omitted visible edges either materialize or appear by exact source-edge ID under `widthInactive`, `deferred`, `rejected`, or `structuralIneligible` with a complete lifecycle reason.
- [ ] Lower Coverage values retain their prior sparse artistic distribution rather than selecting every structural edge.
- [ ] Existing bevel geometry remains `topology:0/0/0/0`, `faceQuality:0`, `surfaceRenderValid:1`, and `meshValid:1` for any emitted preview.
- [ ] The full lifecycle section remains tens of lines for a normal rock, not an indiscriminate per-face or per-triangle dump.

### EW-B4.1 methods decision

- [x] Accepted: maximum Coverage is an exhaustive structural mode, not merely 100% of a pre-filtered artistic candidate pool.
- [x] Accepted: artistic length, shallow-angle, base-position, random, and character preferences remain ranking inputs below maximum Coverage.
- [x] Accepted: complete edge lifecycle telemetry is cumulative evidence for selection, width solving, plane construction, and final materialization.
- [x] Rejected: camera-visible-only bevel generation.
- [x] Rejected: loosening the accepted geometry certifications to hide omitted edges.
- [x] Deferred to EW-B4.2: coordinated connected-cluster width reduction and any locality-policy correction needed to make every structurally selected edge materialize.


## EW-B4.1 Unity result — exhaustive selection exposes four shell conflicts

- [x] Maximum Coverage selected all `40` structurally eligible edges from `44` source edges: `coverage.max:1`, `structural:40`, `selected:40`, and `widthInactive:0`.
- [x] Four additional shallow/artistic-only edges entered the authoritative shell; the small new bevel in the validation image confirms the selection change reached geometry.
- [x] The final shell remained geometrically valid for the retained set: `topology:0/0/0/0`, `faceQuality:0`, `surfaceRenderValid:1`, and `meshValid:1`.
- [x] The old conflict policy removed four selected bevels: `deferred:{0/8/19/37}`, leaving `built:36` and `materializedCoverage:0`.
- [x] The reported local-junction-star blocker is non-authoritative for this closed manifold shell. It is a consequence of an incident selected band being removed, not a separate topology defect.
- [x] The label `artisticallyFiltered` is misleading at maximum Coverage because those edges remain selected; it must be presented as `wouldBeArtisticallyFiltered`.
- [x] Root cause: `TryBuildCleanPlaneCutEdgeOnlyShell` resolves a band conflict by deleting one deterministic victim candidate and rebuilding, rather than reducing the interacting bevel widths.

## EW-B4.2 — Conflict-cluster width reduction without maximum-Coverage deferral

### Purpose

At maximum Coverage, preserve every structurally selected edge and resolve local bevel-band interactions by reducing the complete interacting width cluster. A maximum-Coverage preview must either materialize every selected edge or fail explicitly; it must not display a silently partial rock.

### Required implementation

- [x] Keep the existing candidate-deferral path below maximum Coverage so sparse artistic previews retain established behavior.
- [x] At maximum Coverage, prohibit conflict-driven candidate removal.
- [x] Build a deterministic conflict cluster from the band-audit victim edge, foreign edge, offending source vertex, and all selected bevels incident to the seed endpoints.
- [x] Reduce the cluster together by a bounded `0.75` scale step and rebuild the complete selected shell.
- [x] Derive each edge's minimum scale from the existing numerical source-removal floor and minimum usable bevel width; do not introduce a new artistic minimum-width default.
- [x] Move a reduced plane toward its source edge while preserving its normal, source-edge provenance, strict clipping contract, and positive minimum source removal.
- [x] Retain all selected candidates on every maximum-Coverage retry. If no cluster member can reduce further, reject the complete maximum-Coverage result with an explicit geometric-floor blocker.
- [x] Withhold the preview triangle soup whenever maximum Coverage is active but exhaustive `coverageValid` certification fails, even if the retained partial shell is geometrically manifold.
- [x] Use a bounded maximum of `32` complete-shell passes. Report budget exhaustion rather than falling back to candidate deletion.
- [x] Record every reduction pass with victim edge, foreign edge, source vertex, cluster IDs, previous/requested/applied/floor scales, band coverage, foreign axial/span evidence, and result.
- [x] Record each built edge's solved width, final materialized width, final width scale, and whether conflict reduction changed it.
- [x] Separate `geometryValid` from `coverageValid` in the primary audit. A geometrically valid partial shell reports a Coverage failure, not a local-junction failure.
- [x] Demote the local-junction-star diagnostic to legacy non-authoritative detail and do not promote it into `primaryFailure` for a closed manifold shell.
- [x] Rename maximum-Coverage artistic telemetry to `wouldBeArtisticallyFiltered`.
- [x] Preserve EW-B3.3 strict clipping, true-distance welding, one-surface rendering, topology, face-quality, volume, bounds, and full B3.1 failure telemetry.

### EW-B4.2 validity contract

```text
geometryValid == 1
coverageValid == 1
structurallyEligible == selected
selected == active
active == built
deferred == 0
rejected == 0
widthInactive == 0
unresolvedConflicts == 0
topology == 0/0/0/0
faceQuality == 0
surfaceRenderValid == 1
meshValid == 1
```

### EW-B4.2 validation targets

- [ ] The maximum-Coverage reference rock reports `selected:40`, `active:40`, `built:40`, `deferred:0`, `coverageValid:1`, and `materializedCoverage:1`.
- [ ] `conflictSolve.mode:clusterWidthReduction`; any required reductions are listed in `[Conflict Width Reduction]` and no edge is removed.
- [ ] Previously deferred edges `0/8/19/37` appear in the built set.
- [ ] Every width-reduced lifecycle row reports a positive `materializedWidth` and `0 < materializedWidthScale < 1`.
- [ ] The complete rock remains closed, planar, single-surface, and mesh-valid.
- [ ] If a cluster reaches its derived numerical floor, the complete preview is rejected with exact cluster evidence rather than displaying a partial rock.

### EW-B4.2 methods decision

- [x] Accepted: coordinated local width reduction is the maximum-Coverage conflict policy.
- [x] Accepted: the numerical minimum width is derived from existing geometry tolerances, not a new aesthetic threshold.
- [x] Accepted: maximum Coverage is all-or-nothing for structurally selected edge materialization.
- [x] Rejected: deleting one victim edge per conflict and treating the remaining partial shell as the final maximum-Coverage preview.
- [x] Rejected: treating the legacy local-junction heuristic as authoritative while topology is closed and manifold.
- [ ] Deferred: a Scene-view source-edge ID overlay remains optional diagnostic UI; the authoritative lifecycle file already records stable IDs and endpoint coordinates and no overlay change is required for the B4.2 geometry correction.

## Stable rollback baseline — EW-B4.1-STABLE

- [x] Preserve EW-B4.1 as the immutable stable incomplete rollback baseline while later maximum-Coverage experiments continue.
- [x] Stable evidence: `40` structurally eligible edges, `36` materialized bevels, deferred edges `{0/8/19/37}`, `topology:0/0/0/0`, `faceQuality:0`, `surfaceRenderValid:1`, and `meshValid:1`.
- [x] Do not overwrite, relabel, or package experimental B4.2 geometry as the stable baseline.
- [x] The rollback limitation is explicit: geometry is valid but maximum-Coverage materialization is incomplete.

## EW-B4.2 Unity result — 39 bevels expose one T-junction and one locality deferral

- [x] Conflict-cluster reduction materialized three of the four B4.1-deferred edges and reached `built:39` from `selected:40`.
- [x] The reduction solver completed ten passes, nine cluster reductions, no unresolved band conflict, and a minimum materialized width scale of `0.0750847`.
- [x] Fifteen selected edges were width-reduced.
- [x] Edge `0` remained deferred before shell conflict solving by the plane-locality candidate gate; the current Console evidence does not contain the limiting unrelated vertex, solved/localized plane distances, or source-removal values.
- [x] The final experimental shell is invalid: one T-junction exists from `AfterPlaneConstruction` through final certification.
- [x] Four raw open edges are repaired by seam repair, but the T-junction remains; seam repair is therefore not the stage that creates or resolves the T-junction.
- [x] Face quality remains certified and no strict clipping invariant fails.
- [x] The current topology counter does not identify the T-junction vertex, host segment, owner faces, implicated bevel IDs, width scales, or last conflict pass. Geometry changes are blocked until those facts are captured.
- [x] EW-B4.2 remains experimental and does not supersede EW-B4.1-STABLE.

## EW-B4.2R1 — Exact T-junction and locality-deferral dossiers

### Purpose

Add diagnostic evidence only. Identify the exact unsplit host segment responsible for the experimental B4.2 T-junction and fully explain edge `0`'s plane-locality deferral. Preserve all B4.2 geometry, width scales, conflict decisions, tolerances, and preview validity gates unchanged.

### Required implementation

- [x] Reproduce the authoritative topology T-junction test with the exact same tolerance and endpoint exclusions.
- [x] Record one stable dossier per detected T-junction per captured stage: junction vertex, all owner face identities, host face and provenance, host segment index/endpoints, interpolation parameter, closest point, distance/tolerance, and number of matching host segments.
- [x] Attribute exact bevel provenance from the junction vertex and host face separately from broader candidate-plane matches.
- [x] Record current materialized width and scale for every associated candidate edge.
- [x] Link each T-junction dossier to the latest conflict-reduction pass whose cluster contains an associated edge, including exact cluster IDs and per-edge applied scales.
- [x] Record `FirstTJunctionStage` independently from open-edge and non-planar stages.
- [x] Capture exact per-edge previous and applied scales in every conflict-width-reduction record.
- [x] For every plane-locality deferral, record source edge/vertices/faces, source positions, bevel normal, solved width, solved and localized plane distances, localization delta, guard margin, limiting unrelated vertex/position/projection, solved and localized source-removal values, required minimum removal, and exact blocker.
- [x] Promote an exact T-junction dossier into `primaryFailure` before the generic topology message.
- [x] Add bounded Console examples and full `[T-Junction Failures]` and `[Locality Deferrals]` telemetry-file sections.
- [x] Do not alter edge selection, corner widths, plane construction, strict clipping, welding, seam repair, conflict clusters, materialization, or geometry commit.

### EW-B4.2R1 validation targets

- [ ] One button press identifies the single T-junction's exact vertex, host face/segment, owner faces, associated bevel IDs/scales, first stage, and last modifying conflict pass.
- [ ] Edge `0` appears in `[Locality Deferrals]` with its limiting unrelated vertex and solved-versus-localized source-removal evidence.
- [ ] Stage counts and geometry output remain identical to the B4.2 experimental run.
- [ ] The next geometry patch is selected only from the exact T-junction and locality dossiers.

### EW-B4.2R1 methods decision

- [x] Accepted: preserve EW-B4.1-STABLE independently from the experimental working tree.
- [x] Accepted: cumulative failure records with stable face/edge provenance and exact numerical values.
- [x] Rejected: guessing that the minimum width scale alone caused the T-junction.
- [x] Rejected: changing seam repair, width floors, intersection caching, or edge `0` locality policy before exact attribution.
## EW-B4.2R1 Unity result — exact topology and locality attribution

- [x] The single T-junction is born at `AfterPlaneConstruction` and persists through final certification.
- [x] Exact junction vertex: `(-0.896241307 / 1.05277002 / -0.468577236)`.
- [x] Exact unsplit host: `SourceFace:2`, segment `2`, at `t=0.998316765`.
- [x] Junction residual is `0.000101929516` against topology tolerance `0.000102707592`.
- [x] Directly implicated bevels are `{7/8}`; nearby matching candidate planes are `{7/8/20}`.
- [x] Edge scales at failure are `7=0.133483887`, `8=0.166648686`, and `20=0.133483887`.
- [x] The latest relevant reduction is pass `8`, but that pass reduced `{8/9/17/18/19/36/40}` and omitted local interacting edges `7` and `20`.
- [x] Root cause: the conflict cluster is not closed over the complete local interaction star, and a retry is accepted from band integrity alone without topology certification.
- [x] Edge `0` has no feasible independent locality-plane interval: retaining unrelated vertex `27` requires a plane movement that changes source removal from `+0.00362432003` to `-0.0000200271606`, below the required `+0.0000513537962`.
- [x] Edge `0` requires a later cooperative locality solve; it is intentionally outside EW-B4.2R2.
- [x] EW-B4.1-STABLE remains the immutable rollback baseline.

## EW-B4.2R2 — Topology-aware conflict-cluster closure

### Purpose

Preserve the 39-of-40 experimental coverage gain while rejecting and repairing width-reduction states that introduce topology defects. Keep edge `0` locality-deferred. Do not alter selection, strict clipping, welding, one-surface rendering, topology tolerances, or the stable EW-B4.1 rollback bundle.

### Required implementation

- [x] Require every maximum-Coverage retry to pass both bevel-band integrity and prepared-shell topology/face-quality certification before acceptance.
- [x] Treat open edges, non-manifold edges, T-junctions, invalid faces, or non-planar bevel faces as retry rejection conditions.
- [x] Preserve the latest topology-clean scale map even when the corresponding pass still has a band conflict.
- [x] When a retry introduces a T-junction, roll back the complete scale map to the latest topology-clean state before applying a replacement reduction.
- [x] Build the topology conflict cluster from all T-junction-linked bevel IDs, the latest prior conflict cluster touching those IDs, and the one-hop incident source-vertex star.
- [x] For the proven reference defect, topology cluster construction necessarily includes `{7/8/20}` and the pass-8 cluster that modified edge `8`.
- [x] Reduce the complete expanded cluster coherently and rebuild from immutable original candidates.
- [x] Never accept a band-clean shell that remains topologically invalid.
- [x] Fail explicitly if a topology defect cannot be mapped to a T-junction interaction cluster, reaches its numerical floor, or exhausts the bounded retry budget.
- [x] Extend each conflict record with trigger category, band validity, topology counters, rollback evidence, cluster-entry reasons, previous/rollback/applied scales, and result.
- [x] Extend the bounded Console conflict summary with topology-rejected, topology-expanded, and topology-rollback counts.
- [x] Preserve full EW-B4.2R1 T-junction and locality dossiers.

### EW-B4.2R2 target contract

```text
selected == 40
active == 40
built == 39
deferred == {0}
open == 0
nonManifold == 0
tJunction == 0
invalidFaces == 0
nonPlanar == 0
surfaceRenderValid == 1
meshValid == 1
```

### Methods decision

- [x] Accepted: conflict retries are transactions whose acceptance includes topology and face quality, not only band coverage.
- [x] Accepted: a topology-breaking width state is rolled back before the interaction cluster is expanded.
- [x] Accepted: cluster closure includes direct T-junction provenance, nearby matching bevel planes, the last responsible conflict cluster, and a bounded one-hop source-vertex star.
- [x] Rejected: loosening the T-junction tolerance to hide the near-coincident unsplit host segment.
- [x] Rejected: changing edge `0` locality behavior in the same patch as topology conflict closure.

## EW-B4.2R2 Unity result — T-junction rollback exposes an unmapped open/non-planar retry

- [x] EW-B4.2R2 rejected the original T-junction state and performed one topology rollback plus one expanded-cluster retry.
- [x] The replacement trial removed the T-junction but was invalid immediately at `AfterPlaneConstruction`: `open:3`, `nonPlanar:1`, and maximum normal spread `0.886028051` against the `0.75` degree limit.
- [x] The replacement defect persists through every captured preparation stage; welding, conformity, and seam repair do not introduce it.
- [x] The R2 generalized topology mapper accepts only T-junction records. Because the replacement trial had no T-junction, it could not identify a complete interaction cluster and aborted.
- [x] The top-level zero topology/face-quality fields in the failed R2 audit were uninitialized defaults. The stage timeline is the authoritative failed-trial evidence.
- [x] The R2 failure path produced contradictory lifecycle evidence: the trial attempted `39` bevel planes while the coverage ledger relabelled those same edges as `rejected` and reported `built:0`.
- [x] Rejected interpretation: the 39 attempted bevels were structurally rejected candidates. They were constructed in an invalid solver trial and require a distinct `trialRejected` state.
- [x] Geometry changes remain blocked until the three open edges, the non-planar bevel face, and the responsible prior conflict-scale state are identified exactly.
- [x] EW-B4.1-STABLE remains the immutable valid rollback baseline.

## EW-B4.2R3 — Generalized retry-failure dossiers and transactional solver state

### Purpose

Correct diagnostic and solver-state semantics without changing width-reduction geometry. Preserve immutable evidence for every attempted, band-clean, topology-clean, and fully certified retry; identify every failure category, and stop the current non-T-junction retry after exact attribution rather than applying another guessed scale change.

### Required implementation

- [x] Preserve distinct immutable snapshots for the latest attempted, band-clean, topology-clean, and fully certified solver states, including pass, candidate IDs, per-edge scales, faces, and stage invariants.
- [x] Capture a retry-failure dossier at the earliest failing stage rather than relying on final/default counters.
- [x] Capture complete open-edge dossiers for retry trials, including owner provenance, endpoints, nearest reversed boundary mate, and first stage.
- [x] Capture complete non-planar-face dossiers, including stable face/bevel provenance, vertices, plane residual, normal spread, offending vertex/segment, and first stage.
- [x] Capture non-manifold and invalid-face evidence and include their stable bevel provenance in generalized cluster attribution.
- [x] Generalize retry-failure cluster evidence across open edges, non-manifold edges, T-junctions, invalid faces, and non-planar bevel faces.
- [x] Link each generalized failure to nearby candidate planes, implicated bevel provenance, the latest intersecting conflict pass, and the bounded incident source-vertex star.
- [x] Preserve the existing R2 T-junction retry behaviour unchanged. For a non-T-junction generalized failure, capture/map the evidence and stop without applying another width reduction.
- [x] Distinguish `attemptedBuilt`, `certifiedBuilt`, `trialRejected`, `localityDeferred`, and true structural `rejected` lifecycle states.
- [x] Ensure a failed trial cannot overwrite the last clean/certified state or relabel attempted bevels as structurally rejected.
- [x] Copy the exact latest failed-trial topology and face-quality evidence into the top-level audit instead of leaving zero defaults.
- [x] Add bounded Console retry examples and full `[Transactional Solver States]` and `[Retry Failure Dossiers]` sections to the structured telemetry file.
- [x] Preserve selection, candidate planes, width scales, conflict reductions, clipping, welding, seam repair, render certification, and geometry commit unchanged.

### EW-B4.2R3 expected reference audit

```text
attemptedBuilt == 39
certifiedBuilt == 0
trialRejected == 39
localityDeferred == {0}
rejected == 0
retry failure == open:3 + nonPlanar:1
latest attempted pass is explicit
latest band-clean/topology-clean/certified passes are explicit
three exact open-edge dossiers exist
one exact non-planar bevel dossier exists
```

### EW-B4.2R3 methods decision

- [x] Accepted: attempted construction and certified materialization are separate lifecycle states.
- [x] Accepted: solver retry states are immutable transactions; failed trials cannot corrupt the latest clean state.
- [x] Accepted: generalized defect attribution must cover every topology and face-quality category, not only T-junctions.
- [x] Accepted: the current non-T-junction trial is diagnostic-only and stops after evidence capture.
- [x] Rejected: representing an invalid attempted shell as `rejected` source edges.
- [x] Rejected: publishing default-zero final counters after an early solver failure.
- [x] Deferred: the next geometry correction until the R3 Unity dossier identifies the exact face, open-edge owners, linked bevels, and responsible scale pass.

## EW-B4.2R4 — Minimal topology cluster and transactional relative-scale search

### Purpose

Replace the failed R2 broad topology-recovery reduction with a bounded search that starts from the immutable latest topology-clean scale state, changes only the exact T-junction-linked bevels, preserves their rollback-relative width ratios, and commits only a complete fully certified shell.

### Implemented behaviour

- [x] Derive the initial topology-recovery cluster only from `PlaneCutTJunctionFailureRecord.LinkedEdgeIndices`; the reference T-junction therefore begins with `{7/8/20}`.
- [x] Do not import the previous conflict cluster or recursively add an incident source-vertex star to a T-junction scale search.
- [x] Restore every trial from the complete latest topology-clean scale map; failed-pass scales and generated faces are never reused as trial input.
- [x] Apply each factor relative to every cluster edge's rollback scale: `trialScale(edge) = topologyCleanScale(edge) * factor`, clamped only by that edge's existing numerical floor.
- [x] Test bounded descending factors `{0.95/0.90/0.85/0.80}` and commit the first/highest fully valid tested factor.
- [x] Rebuild every trial from immutable source faces and the original candidate set.
- [x] Reject a trial if any scale outside the exact cluster differs from the topology-clean rollback state.
- [x] Certify band integrity, cap survival/redundancy, open/non-manifold/T-junction/invalid topology, face quality, retained volume, source bounds, one-surface triangulation, and preview mesh validity per trial.
- [x] Preserve pass `7` as the fallback topology-clean state when no tested factor fully certifies; do not automatically broaden the cluster.
- [x] Keep edge `0` locality-deferred and outside this search.
- [x] Add compact `topologyScaleSearch` Console telemetry and a complete `[Minimal Topology Scale Search]` file section with one record per factor, rollback/requested/effective scales, floor hits, collateral changes, stage-evaluation status, certification counters, and all exact captured face/open-edge/T-junction failure records.
- [x] Record `trialBaseState=topologyClean:<pass>`, `failedStateScalesReused=0`, and the explicit topology-clean fallback state when no factor certifies.
- [x] Correct certification lifecycle semantics: a solver-clean shell is not labelled `fully-certified` or counted in `certifiedBuilt` until final cap, topology, face-quality, volume, bounds, one-surface, triangulation, and preview-mesh certification succeeds.
- [x] Preserve edge selection, structural eligibility, strict clipping, true-distance welding, seam repair, topology/face-quality tolerances, one-surface rendering, edge `0` locality, production geometry, and the one-button inspector workflow.

### Expected reference outcomes

Successful search:

```text
topologyScaleSearch.baseState == topologyClean:7
topologyScaleSearch.failedStateScalesReused == 0
topologyScaleSearch.cluster == {7/8/20}
topologyScaleSearch.collateralChanged == {none}
topologyScaleSearch.committedFactor in {0.95/0.90/0.85/0.80}
attemptedBuilt == 39
certifiedBuilt == 39
trialRejected == 0
deferred == {0}
topology == 0/0/0/0
faceQuality == 0
surfaceRenderValid == 1
meshValid == 1
geometryValid == 1
coverageValid == 0
```

No valid tested factor:

```text
topologyScaleSearch.baseState == topologyClean:7
topologyScaleSearch.failedStateScalesReused == 0
topologyScaleSearch.cluster == {7/8/20}
topologyScaleSearch.committedFactor == none
topologyScaleSearch.collateralChanged == {none}
topologyScaleSearch.unresolved == 1
latestTopologyClean == pass 7
certifiedBuilt == 0
trialRejected == 39
preview withheld
```

### EW-B4.2R4 methods decision

- [x] Accepted: minimal exact topology interaction before any deliberate broader construction.
- [x] Accepted: multiplicative rollback-relative scale changes instead of one stale absolute scale shared across the cluster.
- [x] Accepted: each factor is a complete immutable rebuild and full certification transaction.
- [x] Accepted: `fully-certified` is a final geometry/render/mesh state, not merely band-clean plus intermediate topology-clean.
- [x] Rejected: calculating a target from the failed pass and applying that absolute scale after rollback.
- [x] Rejected: importing prior conflict clusters or incident stars automatically into T-junction recovery.
- [x] Rejected: creating a synthetic triangle over the pass-9 needle gap.
- [x] Rejected: loosening clipping, weld, topology, or normal-spread tolerances.
- [x] Unity validation: the minimal transaction containment worked, but all four factors remained band-invalid because unchanged foreign plane `9` continued to split bevel-band edge `8` near axial parameter `0.9642-0.9643`.
- [x] Unity validation: factors `0.90` and `0.85` additionally opened a three-edge gap around source vertex `8` after edge `8` reached its numerical floor `0.166648686`; factors `0.95` and `0.80` remained topology-clean.
- [x] Partially useful: immutable rollback, exact topology-linked evidence, collateral-change rejection, full per-trial certification, and fallback to pass `7` are accepted infrastructure.
- [x] Rejected: uniform factor search over topology-linked `{7/8/20}`. It omitted the directly evidenced foreign band plane `9` and ceased to be proportional once edge `8` hit its floor.
- [ ] Pending: preserve a separate immutable `EW-B4.2-STABLE` baseline only after a fully certified 39-of-40 shell is proven; do not replace `EW-B4.1-STABLE`.

## EW-B4.2R5 — Direct foreign band-plane retreat search

### Purpose

Use the full R4 telemetry to separate the topology-linked failure dossier from the band plane that actually prevents certification. Restore the immutable pass-7 state, keep topology-linked edges `{7/8/20}` unchanged, and retreat only the directly evidenced foreign plane that splits victim edge `8`'s bevel band.

### Evidence and decision

The R4 file telemetry established:

```text
topologyLinked={7/8/20}
direct band victim=8
direct foreign plane=9
foreign axial parameter approximately 0.9642-0.9643
pass-7 scale(9)=0.177978516
pass-8 scale(9)=0.133483887
```

Every R4 factor retained edge `9` at `0.177978516`, so every trial remained band-invalid. The earlier broad pass advanced beyond band integrity only after edge `9` reached `0.133483887`, but it simultaneously reduced `7/20` and created the T-junction. R5 therefore isolates the useful foreign-plane retreat from the harmful topology-linked reductions.

### Implemented behaviour

- [x] Preserve the exact T-junction-linked dossier `{7/8/20}` as topology evidence; do not reinterpret those edges as the width-adjustment set.
- [x] Resolve the retreat target from the latest prior structured `band-integrity` record whose victim belongs to the topology-linked set.
- [x] For the reference failure, require direct evidence `victim=8`, `foreign=9`, `bandPass=7`; do not hardcode those source-edge IDs.
- [x] Restore every trial from the immutable latest topology-clean scale map.
- [x] Change only the directly evidenced foreign edge; the reference retreat set is `{9}`.
- [x] Test descending factors `{0.95/0.90/0.85/0.80/0.75}` against edge `9`'s pass-7 scale.
- [x] Preserve `7`, `8`, and `20` exactly at their pass-7 topology-clean scales during R5.
- [x] Reject any scale change outside the exact retreat set.
- [x] Rebuild every trial from original source faces and the original candidate set.
- [x] Retain the full R4 per-trial certification contract: band, caps, topology, face quality, retained volume, bounds, one-surface rendering, triangulation, and preview mesh validity.
- [x] Commit the first/highest fully valid factor; otherwise restore pass `7` without broadening the search.
- [x] Preserve edge `0` as locality-deferred and do not touch its neighbourhood.
- [x] Preserve the cumulative full telemetry structure. The Console remains compact, while the file records search mode, trigger evidence, topology-linked edges, exact retreat edges, rollback/requested/effective scales, floor hits, collateral changes, all validity gates, and exact failure dossiers for every trial.

### Expected reference outcome

```text
topologyScaleSearch.mode == direct-foreign-band-plane-retreat
topologyScaleSearch.trigger contains bandPass:7,victim:8,foreign:9
topologyScaleSearch.topologyLinked == {7/8/20}
topologyScaleSearch.retreatEdges == {9}
topologyScaleSearch.failedStateScalesReused == 0
topologyScaleSearch.collateralChanged == {none}
topologyScaleSearch.committedFactor in {0.95/0.90/0.85/0.80/0.75}
attemptedBuilt == 39
certifiedBuilt == 39
trialRejected == 0
deferred == {0}
topology == 0/0/0/0
faceQuality == 0
surfaceRenderValid == 1
meshValid == 1
geometryValid == 1
coverageValid == 0
```

### EW-B4.2R5 methods decision

- [x] Accepted: topology attribution and width-adjustment attribution are distinct evidence sets.
- [x] Accepted: retreat the directly evidenced foreign band plane before perturbing topology-linked bevel planes.
- [x] Accepted: full file telemetry is authoritative for solver research; the compact Console summary is only a navigation aid.
- [x] Rejected: reducing `7/8/20` again before edge `9` has been isolated.
- [x] Rejected: importing incident-star neighbours `10/19/37` without a new exact failure record that directly implicates them.
- [x] Rejected: broadening clusters, loosening tolerances, changing welding, or fabricating a junction surface.
- [x] Unity result: edge `9`-only retreat remained band-invalid at every tested factor. Factors `0.95/0.90/0.85` remained blocked by plane `9`; factors `0.80/0.75` exposed plane `7` at the opposite end of victim edge `8`. Every trial remained topology-clean and face-quality-clean.



## EW-B4.2R6 — Dual-endpoint foreign-plane retreat and source-edge number overlay

### Purpose

Use the complete R5 trial sequence to identify both directly evidenced endpoint planes that interrupt victim edge `8`. Preserve the topology-clean pass-7 widths for protected edges `8/20`, retreat only endpoint planes `7/9`, and add an editor-only numbered source-edge overlay so telemetry IDs can be inspected directly on the rock.

### R5 evidence

```text
factors 0.95/0.90/0.85:
  victim=8, foreign=9, axial approximately 0.9662-0.9699

factors 0.80/0.75:
  victim=8, foreign=7, axial approximately 0.03006

all five trials:
  topology=0/0/0/0
  faceQuality=0
  collateralChanged={none}
  bandValid=0
```

The bevel band is therefore interrupted from both axial ends. Edge `9`-only retreat is accepted as a diagnostic isolation method but rejected as a complete solution.

### Implemented behaviour

- [x] Run the existing direct foreign-plane retreat transaction first and retain its complete five-trial telemetry.
- [x] Store structured victim, foreign edge, axial parameter, and shared-span evidence on every search trial rather than relying only on a failure string.
- [x] Resolve the opposing endpoint plane from the first topology-clean direct trial whose victim matches and whose foreign plane differs from the original foreign plane.
- [x] For the reference rock, derive `retreatEdges={7/9}` without hardcoding either source-edge ID.
- [x] Preserve `protectedEdges={8/20}` exactly at their immutable pass-7 topology-clean scales.
- [x] Test paired relative factors `{0.95/0.90/0.85/0.80/0.75}` on only the two directly evidenced endpoint planes.
- [x] Rebuild every direct and dual trial from immutable source faces, original candidates, and the complete pass-7 scale map.
- [x] Reject any scale change outside the active retreat set and retain complete per-trial band, topology, face-quality, cap, volume, bounds, one-surface, triangulation, and mesh certification.
- [x] Preserve separate `[Direct Foreign Band-Plane Retreat Search]` and `[Dual-Endpoint Foreign-Plane Retreat Search]` file sections.
- [x] Add an `activeSearchFailure` record so the current search blocker is reported separately from the historical pass-8 T-junction retained as `primaryFailure`.
- [x] Preserve the cumulative full-file telemetry as the authoritative diagnostic record.
- [x] Historical R6 implementation: add a bevel-preview-fed source-edge overlay and optional focus-only filtering.
- [x] R6R3 correction: supersede preview-fed/focus-only ownership with an independent `Source Edge Index Debug` section.
- [x] Build all source-edge records directly from the current mass recipe and source topology graph without requiring bevel-preview success, edge-wear amount, coverage, or geometry publication.
- [x] Show all source edges whenever the independent overlay is enabled; current bevel-search edges may be highlighted but never hide unrelated source edges.
- [x] Keep the source-edge debug feature non-serialized, editor-only, component-free, and absent from production geometry/runtime behaviour.
- [x] R6R1 usability correction: draw the numbered source edges as an x-ray overlay with dark underlays, endpoint markers, separated callout labels, and a Scene status panel that reports visible/total records and the focused edge IDs.
- [x] R6R1 inspector evidence: report focused/total overlay record counts and warn when focus-only filtering has no structured focus records.
- [x] R6R2 callback correction: reject the per-inspector `OnSceneGUI` path after Unity showed populated `4 focused / 44 total` records while drawing neither the Scene panel nor any edge callouts.
- [x] Register one editor-global `SceneView.duringSceneGui` renderer, keyed to the explicitly enabled Generated Mass instance, so source-edge diagnostics draw independently of bevel-transaction success and custom-editor instance lifecycle.
- [x] Keep renderer state non-serialized and avoid continuous repaint by repainting only when enabled, filter, or target state changes.
- [x] Preserve edge `0` as locality-deferred and preserve `EW-B4.1-STABLE` unchanged.
- [x] R6R3 source-graph ownership correction: add `GenerateSourceEdgeIndexDebug`, `RefreshSourceEdgeIndexDebug`, and a separate non-serialized source-edge record cache.
- [x] R6R3 inspector correction: replace `Only Active Search Edges` with `Show All Source Edge Numbers in Scene`, optional search highlighting, and an explicit `Refresh Source Edge Graph` action.
- [x] R6R3 Scene contract: the status panel must report `44 shown / 44 total` for the reference rock; search focus changes colour only and cannot reduce the shown count.

### Expected reference search

```text
topologyScaleSearch.mode == dual-endpoint-foreign-plane-retreat
topologyScaleSearch.retreatEdges == {7/9}
topologyScaleSearch.protectedEdges == {8/20}
topologyScaleSearch.topologyLinked == {7/8/20}
topologyScaleSearch.failedStateScalesReused == 0
topologyScaleSearch.collateralChanged == {none}

decisive known-width trial:
  scale(7) = 0.133483887
  scale(9) = 0.133483887
  scale(8) = 0.177978516
  scale(20) = 0.177978516
```

A successful outcome remains `certifiedBuilt=39`, clean topology/face quality, one-surface rendering, valid mesh, and edge `0` alone deferred. If no factor certifies, restore pass `7` and report the active dual-endpoint blocker without broadening the cluster.

### EW-B4.2R6 methods decision

- [x] Accepted: edge `8` is a dual-endpoint band interaction; directly evidenced endpoint planes `7/9` are the only R6 width degrees of freedom.
- [x] Accepted: full file telemetry remains the primary evidence source and is retained cumulatively across direct and dual searches.
- [x] Accepted: numbered source-edge Scene diagnostics are useful now because solver records use stable source-edge IDs and the active neighbourhood is small.
- [x] Accepted: source-edge indexing is a standalone topology diagnostic, not a bevel-preview subfeature; all source edges remain visible independently of solver state.
- [x] Rejected: focus-only filtering as the primary overlay mode because it obscures the complete source graph and confused debug ownership.
- [x] Rejected: reducing protected edges `8/20` before the dual endpoint planes have been tested independently.
- [x] Rejected: expanding to incident-star neighbours `10/19/37` without new direct failure evidence.
- [x] Rejected: serializing the edge-number overlay as an artistic or production setting.

## EW-B4.2R7 — Canonical edge viability preflight

### Purpose

Maximum Coverage now means every **geometrically viable** bevel edge, not every mathematically convex source segment. A source edge must pass one canonical preflight before Coverage, corner solving, conflict reduction, or shell construction may use it.

### Canonical hard gates

- [x] Preserve the existing manifold, finite-normal, numerical-length, and convex structural gates.
- [x] Require a minimum convex dihedral of `15 degrees`.
- [x] Require `edgeLength >= requestedWidth * 2 + numericalGuard`.
- [x] Build and cache one independent-plane locality interval from the immutable source polyhedron:
  - retain every unrelated source vertex;
  - retain the solid centre;
  - remove both source-edge endpoints by the minimum required distance;
  - reject when the retain floor exceeds the source-removal ceiling.
- [x] Run one bounded isolated-edge certificate for each edge that survives the cheap gates.
- [x] Require the isolated shell to pass owner/support clipping, topology, face quality, containment, bounds, retained volume, triangulation, and preview geometry certification.
- [x] Require the maximum locally feasible width to remain at least `25%` of requested width.
- [x] Require the isolated endpoint transitions to leave a central span of at least `max(minimumStableEdgeLength, requestedWidth * 0.5)`.
- [x] Apply all hard gates before Coverage and before the shared corner solver.
- [x] Keep artistic ranking separate. Maximum Coverage may override artistic ranking, but it may never override geometric viability.

### Cached evidence contract

- [x] Store one `EdgeWearEdgeViabilityRecord` per source edge in the evaluation audit.
- [x] Map the same record from `EdgeKey` to authoritative source graph edge index once the graph exists.
- [x] Cache the locality retain floor, removal ceiling, margin, limiting vertex, guard, and minimum source removal.
- [x] Reuse the cached locality interval during plane construction; do not rescan all source vertices in solver passes.
- [x] Cache isolated solved width, width fraction, endpoint consumption, remaining span, topology counts, and exact diagnostic.
- [x] Record locality-evaluation count, isolated-evaluation count, locality-cache use count, and total preflight time in the full telemetry file.

### Lifecycle and coverage semantics

Every source edge ends preflight in exactly one state:

```text
StructuralIneligible
GeometricIneligible
ViableUnselected
ViableSelected
```

Only `ViableSelected` edges enter corner solving and the whole-rock shell. Geometrically ineligible edges are not deferred, rejected, trial-rejected, or missing coverage.

At maximum Coverage:

```text
coverage denominator = geometric eligible edges
```

A valid maximum-Coverage result requires every geometric-eligible selected edge to certify. Structural and geometric exclusions remain fully reported but do not invalidate materialized coverage.

### Canonical failure reasons

```text
dihedral-below-bevel-viability
edge-too-short-for-bevel-footprint
independent-plane-locality-infeasible
isolated-rail-solve-failed
owner-face-support-insufficient
maximum-feasible-width-below-minimum-scale
endpoint-star-consumes-edge-span
isolated-topology-invalid
isolated-face-quality-invalid
isolated-containment-invalid
isolated-volume-or-bounds-invalid
isolated-construction-invalid
```

No source-edge ID is encoded in any gate.

### Telemetry

The authoritative file adds:

```text
[Edge Viability Preflight]
```

with thresholds, cache-use counters, elapsed preflight time, and one complete record per source edge. Existing lifecycle, conflict, topology, numerical, and final-certification sections remain cumulative.

### EW-B4.2R7 methods decision

- [x] Accepted: viability is a prerequisite to selection, not a solver outcome.
- [x] Accepted: locality is represented as a cached feasible plane-distance interval and consumed later without repeating the source-vertex scan.
- [x] Accepted: the proven bounded isolated-edge builder is the authoritative per-edge construction certificate.
- [x] Accepted: expensive deterministic preflight is dirty-time work and is performed once per physical evaluation; solver retries consume cached records.
- [x] Accepted: global conflict solving remains necessary only for interactions among individually legitimate bevels.
- [x] Rejected: Maximum Coverage overriding shortness, shallow-angle, locality, owner-support, isolated-construction, or minimum-width viability.
- [x] Rejected: deciding inclusion or exclusion from source-edge IDs observed on one reference rock.
- [x] Rejected: shrinking an intrinsically non-viable edge until it becomes visually or numerically meaningless.

### Source-edge debug depth mode

- [x] Default the independent source-edge overlay to depth-tested visible edges so rear-side source lines do not appear detached from the rendered mass.
- [x] Retain an explicit `X-Ray Hidden Source Edges` toggle for complete topology inspection.
- [x] Seed the shared corner solver from the cached maximum locally feasible width, so the bounded isolated width search is not repeated or ignored later in the same evaluation.

## EW-B4.2R7R1 — Immutable source placement frame

### Defect

A certified bevel preview previously resolved dimensions, lean, grounding, and ground recentering from the reconstructed bevel triangle soup. Bevel reconstruction changes triangle count, low-vertex multiplicity, bounds, and sometimes the vertical range. The preview could therefore receive a different lean distance, grounding frame, and contact-centre translation than the unmodified source mass even though the GameObject Transform did not change.

### Canonical placement contract

- [x] Build one immutable pre-bevel source triangle soup from the authored plane-cut faces before any edge-wear transaction begins.
- [x] Use the ordinary production triangulation inputs: current surface facet density, edge character, and surface seed.
- [x] Resolve dimensions once and apply them to both the immutable reference and the output soup.
- [x] Resolve lean parameters once from the dimensioned immutable source reference.
- [x] Apply that lean frame to the immutable reference, then resolve grounding from the leaned source reference.
- [x] Apply that grounding frame to the immutable reference, then resolve the ground-contact centre and vertical offset from the grounded source reference.
- [x] Apply the completed frame unchanged to the bevel output and source-edge debug records.
- [x] Never derive placement parameters from a reconstructed bevel soup.
- [x] Preserve the existing output-derived placement path for ordinary non-bevel generation by using the output soup itself as the reference.
- [x] Keep the cache local to one generation. Recipe or geometry changes create a new immutable source reference on the next explicit rebuild.

### Placement telemetry

The unified rebuild file appends:

```text
[Canonical Placement Frame]
```

with frame provenance, build/reuse counts, reference/output vertex counts, lean, grounding, recenter parameters, and the legacy output-derived frame delta that would previously have been applied to a successful preview.

Required successful-preview invariants:

```text
placementFrameSource=immutable-pre-bevel
placementFrameBuilds=1
previewDerivedPlacementParameters=0
objectTransformChanged=0
previewUsesCanonicalFrame=1
```

### EW-B4.2R7R1 methods decision

- [x] Accepted: source mass, successful bevel preview, and independent source-edge records must share one immutable source-derived placement frame.
- [x] Accepted: the source reference may be triangulated once during the explicit editor rebuild; this is deterministic dirty-time work and is not repeated inside solver trials.
- [x] Rejected: recomputing lean, grounding, or contact-centre placement from the bevel triangle soup.
- [x] Rejected: changing the normal production mass placement algorithm as part of this preview alignment correction.

## EW-B4.2R8 — Viability audit integrity

### Purpose

R7R1 established a valid `29/29` maximum-Coverage shell. R8 changes no geometry. It makes the accepted viability evidence deterministic, correctly named, and safe to consume in the upcoming multi-seed audit.

### Isolated-width audit semantics

- [x] Preserve the accepted viability and corner-width decisions unchanged.
- [x] Distinguish the last attempted isolated width from a width that completed the full isolated geometry certificate.
- [x] Record `isolatedSucceeded`, `lastAttemptedWidth`, `maximumCertifiedWidth`, and `maximumCertifiedWidthFraction`.
- [x] When the isolated certificate fails, report `maximumCertifiedWidth=0` and `maximumCertifiedWidthFraction=0`; never label the last failed attempt as a maximum feasible width.
- [x] Retain the internal accepted width-decision fields for unchanged R7 behavior; R8 changes telemetry semantics only.

### Locality-cache contract

- [x] Count a construction use only when the selected edge has an evaluated, locality-valid cached viability record.
- [x] Count every missing or incomplete construction record as `localityCacheMissesDuringConstruction`.
- [x] Fail candidate construction explicitly on a cache miss; never rescan source vertices as a fallback.
- [x] Report locality evaluations, construction uses, solver recomputations, unused evaluated records, and construction cache misses.
- [x] Required invariant: `recomputationsDuringSolver=0` and `localityCacheMissesDuringConstruction=0`.

### Viability exclusion summary

- [x] Build generic reason categories from lifecycle reason codes, never source-edge IDs.
- [x] Report counts and exact ordered IDs for boundary, dihedral, footprint, locality, isolated-rail, owner-support, width-fraction, endpoint-span, and other exclusions.
- [x] The category counts must reconstruct `source - structural/geometric exclusions = geometric eligible`.

### Retired diagnostics

- [x] Remove the explicit-junction-face coverage heuristic from the active plane-cut evaluation.
- [x] Remove `legacyJunctionHeuristic`, `[Legacy Junction Heuristic - Non-Authoritative]`, and `legacyLocalJunctionDiagnostic` from Console and file telemetry.
- [x] Preserve the authoritative local-junction extraction counts, final topology audit, open-edge dossiers, and T-junction dossiers.

### Stable evaluation fingerprint

- [x] Hash ordered exclusion reasons, selected edge IDs, certified edge IDs, exact final polygon topology, and the canonical placement frame.
- [x] Append `[Stable Evaluation Fingerprint]` after canonical placement is resolved.
- [x] Record source/structural/geometric/selected/certified counts plus component hashes and one combined evaluation hash.
- [x] Identical rebuilds with unchanged settings must produce identical fingerprints.

### Explicit non-goals

- [x] Do not change `15 degrees`, `2x footprint`, `25% width`, or endpoint-span thresholds.
- [x] Do not change selection, corner solving, plane construction, clipping, welding, topology tolerances, rendering, placement, or source-edge Scene debug.


## EW-B4.2R9 — Editor-only multi-seed viability matrix

### Purpose

R9 changes no Generated Mass geometry. It executes the accepted R7/R8 viability and edge-plane-shell builder over a deterministic matrix without publishing any intermediate mesh or modifying the selected object.

### Canonical matrix

- [x] Add one inspector action: `Run Edge-Wear Viability Matrix (30 Cases)`.
- [x] Use ten deterministic stratified shape seeds: `1`, `1112`, `2223`, `3334`, `4445`, `5556`, `6667`, `7778`, `8889`, and `9999`.
- [x] Evaluate each seed at edge-wear width `0.05` (`minimum`), `1.0` (`default`), and `2.0` (`maximum`).
- [x] Force maximum Coverage (`2.0`) for every matrix case while preserving the selected mass's other recipe and surface-feature settings.
- [x] Run exactly one case per `EditorApplication.update` and expose cancellation through both the progress UI and Inspector.

### Immutable evaluation contract

- [x] Serialize the selected `MassRecipe` once and create an isolated clone for every case.
- [x] Change only the clone's shape seed and the case-local edge-wear width/Coverage values.
- [x] Call the same authoritative `UnifiedBoundedPreview` generation path used by manual evaluation.
- [x] Build and discard `MeshData`; never call `MeshBuilder.ApplyToMesh`, bind a collider, or publish a preview.
- [x] Suppress per-case Console records and per-case `GeneratedMassEdgeWearTelemetry.txt` writes while retaining the exact audit and placement results in memory.
- [x] Verify after completion that the selected object's recipe JSON, local Transform, and shared mesh reference are unchanged.

### Case pass contract

Every case passes only when:

```text
completed
previewApplied
certifiedBuilt == coexistenceEligible
coverageValid == 1
geometryValid == 1
meshValid == 1
surfaceRenderValid == 1
openEdges == 0
nonManifoldEdges == 0
tJunctions == 0
invalidFaces == 0
nonPlanarFaces == 0
stableFingerprintPrepared == 1
localityCacheMissesDuringConstruction == 0
solverLocalityRecomputations == 0
objectTransformChanged == 0
previewDerivedPlacementParameters == 0
previewUsesCanonicalFrame == 1
```

### Output contract

- [x] Write `Library/GeneratedMassEdgeWearBatchAudit.txt` with aggregate counts and one compact full-fidelity record per case.
- [x] Write `Library/GeneratedMassEdgeWearBatchAudit.csv` with one row per completed case.
- [x] Record seed, width tier/value, eligibility and exclusion counts, certified/deferred/rejected counts, width reductions, topology, face quality, cache counters, timings, all six stable fingerprints, and exact primary failure.
- [x] Emit one compact final Console summary only.
- [x] Preserve partial results when the user cancels through the progress UI or Inspector.

### Explicit non-goals

- [x] Do not change viability thresholds, edge selection, corner solving, plane construction, conflict solving, topology tolerances, placement, rendering, or production geometry commit.
- [x] Do not add serialized fields, components, objects, tags, layers, scenes, prefabs, materials, or runtime work.

## EW-B4.2R10 — Coexistence viability closure

### Evidence and purpose

The R9 matrix completed all 30 coordinates without mutating the selected object, but only 25 cases certified. Three failures were missing source-vertex junctions, one was a near-endpoint T-junction, and one was a strict cached plane-pair intersection failure. Nine nominally successful cases also reduced at least one bevel below the accepted `0.25` meaningful-width fraction. R10 treats these as generic coexistence failures rather than edge-ID exceptions.

### Hard materialized-width floor

- [x] Reuse the canonical `EdgeWearMinimumFeasibleWidthFraction = 0.25` constant for both isolated viability and global conflict solving.
- [x] Clamp every solver candidate minimum scale to at least `0.25` of requested width.
- [x] Never report a shell as successful by reducing a bevel below the meaningful-width floor.
- [x] Require the matrix case pass contract to report `minimumWidthScale >= 0.25`.

### Bounded coexistence closure

- [x] Run coexistence closure only after the complete individually viable candidate set fails authoritative plane construction or shell certification.
- [x] Derive the implicated local candidate set from structured strict-intersection, open-edge, T-junction, retry-dossier, band-conflict, and source-vertex evidence.
- [x] Invoke coexistence exclusion only for source-vertex-star, plane-pair/T-junction, or hard width-floor evidence; unrelated construction or face-quality failures remain terminal and cannot be hidden by dropping edges.
- [x] Initial R10 used deterministic single/pair exclusion trials; R10R2 supersedes that greedy strategy with conflict-directed best-first states.
- [x] Reuse the exact authoritative plane construction, preparation, band, topology, face-quality, containment, bounds, volume, render, and mesh certificates for every trial.
- [x] When an intersection-cache entry fails its current owner/cut plane certificate, invalidate it and recompute the exact segment intersection once through the existing two-plane correction path; never loosen tolerance.
- [x] Cache each retained-edge/scale trial for the physical evaluation; never repeat an identical coexistence trial.
- [x] Bound closure to 12 exclusions, 128 evaluated states, and ten structured implicated candidates per failure.
- [x] Select the first fully certified best-first state ordered by exclusion count, removed requested width, removed selection score, retained minimum scale, and stable edge order.
- [x] Do not hardcode any source-edge IDs.

### Lifecycle and Coverage

- [x] Add `CoexistenceIneligible` after individual geometric viability and before final selection/certification accounting.
- [x] Record exact generic reasons, including source-vertex-star, plane-pair, plane-band, width-floor, candidate-conservation, and terminal coexistence incompatibility.
- [x] Remove coexistence-ineligible edges from selected/active state without classifying them as deferred, rejected, or trial-rejected.
- [x] Maximum Coverage now requires `certified == coexistenceEligible`.
- [x] Preserve structural and individual geometric counts so the audit can distinguish each denominator transition.

### R9/R10 matrix integrity

- [x] Initial R10 used report contract `EW-B4.2R10`; R10R2 supersedes it with `EW-B4.2R10R2`.
- [x] Report geometric and coexistence eligibility separately, plus coexistence exclusions, trials, and cache uses.
- [x] Classify missing-junction, T-junction, strict-intersection, face-quality, placement, width-floor, and other failures from authoritative primary-failure evidence rather than zeroed downstream flags.
- [x] Preserve the same 30 deterministic cases and selected-object immutability contract.

### Explicit non-goals

- [x] Do not loosen topology, endpoint, plane, welding, or face-quality tolerances.
- [x] Do not change source generation, individual viability thresholds, placement, rendering, scenes, prefabs, materials, components, tags, layers, or production geometry commit.
- [x] Do not share caches between seeds or width coordinates.

## EW-B4.2R10R2 — Conflict-directed closure and candidate conservation

### Evidence and correction

The first R10 matrix enforced the `0.25` materialized-width floor and corrected stale exact-intersection recovery, but only `21/30` cases certified. Six remaining failures exposed structured foreign-plane band splits that the closure did not classify, two were missing source-vertex junctions, and one retry dossier retained a T-junction. One trial also exposed a selected-versus-certified mismatch because retained candidates were not certified against the complete pre-closure selected set.

### Conflict-directed best-first search

- [x] Replace greedy permanent progress commits with a bounded best-first frontier.
- [x] Search states contain the complete explicit exclusion set, retained scale map, authoritative audit, exact failure signature, and per-edge exclusion reasons.
- [x] Expand only edges implicated by structured source-vertex, T-junction, strict-intersection, retry-dossier, band-victim/foreign, or candidate-conservation evidence.
- [x] Order states by fewest exclusions, least removed requested width, least removed selection score, greatest retained minimum scale, and stable edge order.
- [x] Deduplicate exact exclusion/scale states and keep the existing `12`-exclusion, `128`-state, ten-implicated-edge bounds.
- [x] Commit lifecycle exclusions only after a completely certified winning state is selected; never publish intermediate progress exclusions.
- [x] Add `plane-band-incompatible` as a structured coexistence reason without parsing diagnostic prose.
- [x] Normalize retry T-junction and missing-junction evidence into the same generic closure categories.

### Candidate-conservation certificate

- [x] Build the root expected candidate set from pre-closure selected lifecycle records before coexistence closure.
- [x] For every trial, require the actual retained candidate IDs to equal `rootExpected - explicitExclusions`.
- [x] Encode explicit exclusions in the trial-cache key so absent upstream candidates cannot alias another search state.
- [x] Reject any nominally successful geometry trial with missing or unexpected candidates as `candidate-conservation-failed`.
- [x] Allow missing expected candidates to enter the structured conflict set so they can only disappear through an explicit `candidate-conservation-incompatible` lifecycle exclusion.
- [x] Record expected, actual, missing, unexpected, and certified candidate evidence plus search-state telemetry.

### Matrix/report integrity

- [x] Update the report contract to `EW-B4.2R10R2`.
- [x] Add plane-band and candidate-conservation exclusion counts.
- [x] Record states evaluated/deduplicated, maximum depth, frontier remainder, winning depth, and candidate-conservation failures.
- [x] Classify retry dossiers containing `t-junctions:` as T-junction failures and classify terminal band-split evidence separately from generic construction failure.

### Explicit non-goals

- [x] Do not loosen plane, topology, endpoint, welding, or face-quality tolerances.
- [x] Do not change the `15 degree`, `2x`, `0.25`, or endpoint-span viability thresholds.
- [x] Do not alter source generation, canonical placement, rendering, scenes, prefabs, materials, components, tags, layers, or production geometry commit.


## EW-B4.2R10R3 — Structured trial dossiers and winning-state finalization

### Structured failure provenance

- [x] Add a typed coexistence failure dossier for plane-band, strict-intersection, missing-junction, T-junction, open-boundary, face-quality, containment, bounds, surface, and candidate-conservation outcomes.
- [x] Populate a dossier for the root transaction and every exclusion trial, including failures that exit before band auditing.
- [x] Preserve the parent dossier when a child exits before producing new structured evidence; never terminate a searchable plane-band or source-vertex-star branch merely because the child blocker is generic.
- [x] Store immutable source-vertex star membership from the original individually viable candidate set and branch over the still-active members after each exclusion.
- [x] Route search decisions from typed fields only; human-readable blocker text remains diagnostic.

### Winning-state finalization

- [x] Clear the failed root transaction's diagnostic, retry dossier, open-edge/T-junction stage, and stale conflict provenance after a certified coexistence winner is selected.
- [x] Apply explicit coexistence exclusions, finalize retained lifecycle records as built, recalculate Coverage, and require exact denominator/count equality before returning success.
- [x] Reject an internally inconsistent winner as `winning-state-finalization-failed` with all predicate values.
- [x] Preserve candidate conservation, the `0.25` width floor, unchanged tolerances, and the canonical placement frame.

### Search evidence

- [x] Update the matrix report contract to `EW-B4.2R10R3`.
- [x] Record stage, source vertex, victim/foreign pair, linked edges, immutable star, implicated edges, candidate counts, width scale, validity, and terminal signature for every processed state.
- [x] Append per-case `[Case N Coexistence Search]` ledgers to the TXT batch report for every case that invokes closure.


## EW-B4.2R10R4 — Corner-width eligibility reconciliation

### Lifecycle correction

- [x] Classify a selected individually viable edge with no shared corner-width entry as `corner-width-missing`.
- [x] Classify a selected individually viable edge with width at or below `PointMergeDistance` as `corner-width-inactive`.
- [x] Preserve geometric viability and `WidthInactive` evidence while clearing candidate, selected, active, attempted, built, trial-rejected, deferred, and rejected state.
- [x] Keep corner-width exclusions distinct from search-time source-star, plane-pair, plane-band, width-floor, and candidate-conservation exclusions.

### Coverage and finalization

- [x] Add `UnresolvedWidthInactiveCount` and require it to be zero for materialized Coverage and committed winning-state finalization.
- [x] Retain total `WidthInactiveCount` as cumulative evidence instead of clearing it to satisfy certification.
- [x] Require the expected coexistence set to be selected, active, positive-width, coexistence-eligible, not deferred, and not rejected.
- [x] Use candidate-ID fallback only when no Coverage audit exists.
- [x] Publish the union of pre-shell and search-time exclusions after a certified winner.

### Telemetry and reports

- [x] Add dedicated corner-width missing/inactive exclusion counts and edge IDs.
- [x] Record expected, actual, missing, and unexpected edge-ID sets for every search state.
- [x] Separate terminal matrix candidate-conservation failures from failed intermediate search states.
- [x] Update the matrix report contract to `EW-B4.2R10R4`.

### Validation

- [x] Confirm seed `2223/minimum` reports two pre-shell corner-width exclusions, root candidate conservation `32/32`, and one generic T-junction exclusion.
- [x] Confirm the final case reports `coexistenceEligible == selected == certified == 31`, `widthInactive == 2`, and `unresolvedWidthInactive == 0`.
- [x] Run the unchanged 30-case matrix twice and require `30/30`, identical fingerprints/exclusion sets, zero topology/face-quality/placement/cache failures, and minimum width scale at least `0.25`.

### Explicit non-goals

- [x] Do not alter topology, endpoint, plane, clipping, welding, or face-quality tolerances.
- [x] Do not add endpoint snapping or expand search budgets.
- [x] Do not change source generation, individual viability thresholds, canonical placement, rendering, scenes, prefabs, materials, components, tags, layers, or production geometry commit.


## EW-B4.2R11A — Visual selection and overlay reliability

### Selection separation

- [x] Stop ordinary inspector maximum Coverage from bypassing `ArtisticEligible`.
- [x] Add explicit editor-only `UnifiedBatchAudit` evaluation so the 30-case matrix still includes every geometrically viable candidate.
- [x] Preserve all R10R4 topology, candidate-conservation, width-floor, placement, and coexistence rules.

### Existing overlay upgrade

- [x] Invalidate source-edge debug data when production-generation or edge-wear inputs change.
- [x] Reuse classified records from a current unified preview; otherwise rebuild the current graph without committing geometry.
- [x] Display the current shape seed and source-edge count in the Scene panel.
- [x] Classify records in the existing view as certified, artistically filtered, width-floor failure, isolated-rail failure, coexistence exclusion, or another geometric/structural exclusion.
- [x] Keep diagnostics to the single existing source-edge overlay; add no extra view.
- [x] Update the matrix report contract to `EW-B4.2R11A`.

### Validation

- [ ] Confirm changing from seed `5727` to `2223` while the overlay remains enabled immediately changes the panel to seed `2223` and `39` source edges.
- [ ] At ordinary maximum Coverage, confirm seed `5727` edge `39` and seed `2223` edge `33` are marked `A` and are not bevel candidates.
- [ ] Confirm seed `2223` edges `13/14` are marked `W` and edge `36` is marked `R`.
- [x] Run the exhaustive matrix under `EW-B4.2R11A`; it retained `30/30` and frozen R10R4 fingerprints, but did not validate the ordinary preview path. Superseded by the dual R11A.1 audits below.

### Explicit non-goals

- [x] Do not modify isolated-rail construction, the `0.25` width floor, dihedral thresholds, topology tolerances, welding, or coexistence search.
- [x] Do not change bevel shading normals or shader response in R11A.
- [x] Do not enable production geometry commit.

## EW-B4.2R11A.1 — Preview coverage-contract repair

### Runtime regression addressed

- [x] Record that R11A's overlay refresh and artistic filtering worked, but ordinary preview certification still used the exhaustive `coexistenceEligible == selected` denominator.
- [x] Correct seed `5727` so its valid `28/28` artistically selected shell is not rejected because one geometrically viable edge is intentionally marked `A`.
- [x] Correct seed `2223` so its valid `30/30` winning child is not rejected because one artistically filtered edge remains coexistence-eligible.

### Contract separation

- [x] Add `RequireAllGeometricCandidates` to `EdgeWearCoverageAudit` and batch capture/result state.
- [x] Require `coexistenceEligible == selected` only for the explicit exhaustive topology matrix.
- [x] For ordinary preview, require `selected == active == attempted == built == retained`, zero unresolved inactive widths, and zero rejected/deferred/unmapped records.
- [x] Keep maximum Coverage available for width-reduction behavior without treating it as an exhaustive candidate policy.

### Editor audit parity

- [x] Rename the existing audit to **Topology Viability Matrix (30 Exhaustive Cases)**.
- [x] Add **Artistic Preview Parity Matrix (30 Cases)** using the same candidate path as the ordinary preview button.
- [x] Write preview-parity reports to `Library/GeneratedMassEdgeWearPreviewParityAudit.txt|csv` without replacing the frozen topology report.
- [x] Advance report contracts to `EW-B4.2R11A.1-topology` and `EW-B4.2R11A.1-preview`.

### Validation

- [ ] Rebuild seed `5727`; require `selected == attempted == certified == 28`, `coverageValid == 1`, and preview applied.
- [ ] Rebuild seed `2223`; require edge `33` artistic filtering, one generic coexistence exclusion, final `selected == attempted == certified == 30`, and preview applied.
- [ ] Run the topology matrix; require `30/30` and fingerprints/exclusions unchanged from frozen R10R4.
- [x] Run the artistic preview parity matrix; runtime result passed `30/30` with zero coverage/topology/placement/cache failures.

### Explicit non-goals

- [x] Do not change candidate geometry, isolated-rail construction, width floor, topology tolerances, coexistence search, shading normals, shaders, or production commit.


## EW-B4.2R11B.1 — Coincident boundary-seam reconciliation

- [x] Preserve R11A.1 ordinary-preview and exhaustive-topology denominator contracts.
- [x] Detect reversed one-sided source-edge incidences whose endpoints differ by no more than `PointMergeDistance`.
- [x] Require distinct owner faces and avoid merging same-direction or already two-sided edges.
- [x] Canonicalize the corresponding graph vertices and graph edge without modifying source faces.
- [x] Add raw/canonical source counts, seam-pair counts, graph vertex-alias count, graph seam-pair count, and per-edge reconciliation evidence.
- [x] Advance topology and preview report contracts to `EW-B4.2R11B.1-*`.
- [x] Validate seed 5727: the two reversed boundary pairs become canonical two-face edges and are assessed normally; runtime result restored `source=42`, `geometric=36`, and `selected/certified=34/34`.
- [x] Run the exhaustive topology matrix and artistic preview parity matrix; both passed `30/30` with zero topology, face-quality, placement, cache, and collateral failures.
- [ ] Defer seed 2223 edge 36 micro-junction rail recovery to R11B.2.

## EW-B4.2R11B.1C — Rollback and collateral-preservation guard

### Rollback

- [x] Remove the zero-yield R11B.2 singleton plane-shell fallback.
- [x] Remove R11B.3 bevel-graph micro-feature normalization and its source/provenance mutations.
- [x] Restore R11B.1 coincident boundary-seam reconciliation as the active geometry baseline.
- [x] Preserve R11A.1 artistic-preview and exhaustive-topology denominator contracts.

### Collateral audit

- [x] Capture an immutable individual-viability baseline before any future recovery stage.
- [x] Record newly recovered, collateral-lost, and collateral-changed edge IDs.
- [x] Treat source identity, owner faces, classification, length, dihedral, feasible width, and width fraction as protected baseline state.
- [x] Fail both matrices when any baseline viable edge is lost or changed.
- [x] Add `collateral=baseline/current/recovered/lost/changed/valid` to compact telemetry and TXT/CSV reports.
- [x] Advance report contracts to `EW-B4.2R11B.1C-topology` and `EW-B4.2R11B.1C-preview`.

### Runtime validation

- [x] Rebuild seed `2223/default`; runtime validation restored the pre-normalization candidate universe with zero collateral loss/change.
- [x] Rebuild seed `5727/default`; runtime result was `rawSource/source=44/42`, seam pairs `2`, `geometric=36`, `selected/certified=34/34`, and `collateral=36/36/0/0/0/1`.
- [x] Run both 30-case matrices; both passed `30/30` with `collateralPreservationFailures=0` and no topology, face-quality, placement, or cache failures.

### Next recovery constraint

- [x] Evaluate only a candidate-local virtual support-chain rail that leaves the source graph and unrelated lifecycle records unchanged; R11B.4 through R11B.4.2 were tested and rejected.
- [x] Reject any recovery patch with zero recovered edges or any collateral lost/changed edge; the final R11B.4.2 suite produced zero recoveries and triggered retirement.

## EW-B4.2R11B.1D — One-click validation suite

- [x] Add one Inspector action that rebuilds the current preview and runs both canonical matrices sequentially.
- [x] Append seed `5727` to the canonical matrix set so coincident-boundary seam reconciliation is always regression-tested.
- [x] Expand each matrix from `30` to `33` cases without changing the original ten-seed coordinates.
- [x] Write one combined report containing current-preview telemetry plus both complete matrix reports.
- [x] Add Copy Full Validation Report and Reveal Full Report Inspector actions.
- [x] Keep the two focused matrix buttons available.
- [x] Unity runtime validation passed under R11B.1D: current seed `5727` rebuilt `34/34`, topology passed `33/33`, artistic preview passed `33/33`, collateral failures were zero, and the combined report was produced.

## EW-B4.2R11B.1E — Recovery retirement and geometry baseline lock

- [x] Remove all R11B.4/R11B.4.1/R11B.4.2 owner-face support interval code.
- [x] Remove fallback records, counters, hit telemetry, CSV fields, and zero-recovery matrix failure rules.
- [x] Restore the four geometry/audit source files byte-for-byte to R11B.1D.
- [x] Retain coincident-boundary seam reconciliation and the collateral-preservation guard.
- [x] Retain seed `5727`, both `33`-case matrices, and the one-click combined validation report.
- [x] Advance contracts to `EW-B4.2R11B.1E-suite`, `EW-B4.2R11B.1E-topology`, and `EW-B4.2R11B.1E-preview`.
- [x] Record the final rejected-branch evidence: `27` evaluations, `126` width attempts, zero virtual corners, zero traversed segments, and zero recoveries per policy.
- [x] Run the one-click suite once and require suite pass, topology `33/33`, artistic preview `33/33`, seed `5727` selected/certified `34/34`, and zero collateral failures. Runtime validation passed under R11B.1E.
- [x] After validation, close geometry recovery and proceed to adaptive artistic selection.


## EW-B4.2R12A — Artistic-selection telemetry and audit

### Scope

- [x] Preserve R11B.1E geometry, geometric eligibility, score ordering, selected-count calculation, widths, and certification behavior.
- [x] Record exact current score components: length, dihedral, deterministic random term, base suppression, upward-edge boost, and recipe character boost.
- [x] Record diagnostic-only edge-axis orientation, silhouette potential, feasible/solved width fraction, local viable-edge density, and shared-vertex crowding with explicit zero score weight.
- [x] Record each viable edge's artistic gates, filter reason, selection rank, threshold, and threshold delta.
- [x] Add all/selected/filtered score minimum, median, and maximum values.
- [x] Add all/selected/filtered distributions for length, dihedral, orientation, silhouette, local density, and crowding.
- [x] Project the audit into current-preview telemetry, both matrix TXT reports, both matrix CSV reports, and the one-click combined report.
- [x] Keep matrix pass/fail criteria unchanged.
- [x] Advance report contracts to `EW-B4.2R12A-suite`, `EW-B4.2R12A-topology`, and `EW-B4.2R12A-preview`.

### Runtime acceptance

- [x] Compile with zero C# errors.
- [x] Run the one-click suite and require current preview pass, topology `33/33`, artistic preview `33/33`, and zero collateral failures. Runtime result passed all requirements.
- [x] Confirm `[Artistic Selection Audit]` is present and `captured=1`.
- [x] Use the aggregate and per-edge evidence to define the comprehensive R12A.1 audit instead of retuning selection from assumptions.


## EW-B4.2R12A.1 — Comprehensive artistic evidence suite

### Immutable behavior boundary

- [x] Preserve the R11B.1E geometry baseline and all R12A production selection behavior.
- [x] Perform no additional geometry rebuilds beyond the existing one-click current preview and two 33-case matrices.
- [x] Keep the production score formula, hard gates, descending ordering, Coverage calculation, widths, and certification unchanged.

### Complete raw evidence

- [x] Export every source edge for every artistic-preview matrix coordinate.
- [x] Include canonical IDs, endpoints, midpoint, owner normals, bevel normal, owner faces, classification, seam provenance, length, dihedral, and orientation.
- [x] Include every structural, geometric, coexistence, and artistic gate plus filter/candidate/final reasons.
- [x] Include all score components and modifiers, selection rank/threshold/delta, context metrics, locality and isolated-rail viability, effect variation/strength/depth, solved/materialized width, and complete lifecycle state.

### Exhaustive ranking analysis

- [x] Evaluate the exact current policy and named random/modifier/gate ablations.
- [x] Evaluate every angle/length/random weight triple at 0.05 resolution under all eight modifier masks.
- [x] Evaluate all hard-gate masks, single-metric controls, signed context sweeps, and named composite policies.
- [x] Analyze every fixed selected slot and native Coverage deciles from 10% through 100%.
- [x] Report score/metric Pearson and Spearman correlations, Pareto frontier and dominance inversions, per-edge rank ranges and selection frequencies, threshold gaps, no-random sensitivity, scenario churn/intersection/union/core, and cross-width stability.

### One-click output contract

- [x] Embed decisive comprehensive evidence in `Library/GeneratedMassEdgeWearValidationSuite.txt`.
- [x] Automatically write the complete audit TXT, raw edge CSV, and full scenario CSV without asking the user to perform extra validation steps.
- [x] Fail the suite when comprehensive evidence is unavailable or cannot be written.
- [x] Advance contracts to `EW-B4.2R12A.1-suite`, `EW-B4.2R12A.1-topology`, `EW-B4.2R12A.1-preview`, and `EW-B4.2R12A.1-comprehensive`.

### Runtime acceptance

- [x] Compile with zero C# errors.
- [x] Run **Full Edge-Wear Validation Suite (1 Click)** once.
- [x] Require suite pass, both matrices `33/33`, zero collateral failures, `artisticComprehensiveAvailable=1`, and current-score reproduction error within floating-point tolerance. R12A.1b passed with both matrices `33/33`, valid recorded production ranks, and maximum score-reproduction error `1.49011612E-08`.
- [x] Analyze the single copied combined report and make the next artistic-policy decision without another incremental telemetry patch. The accepted decision is R12B.1 geometric-priority artistic selection.


## EW-B4.2R12B.1 — Geometric-priority artistic selection

### Production policy

- [x] Raise the artistic angle gate from `0.035` to `0.055` without changing the `15`-degree geometric viability floor.
- [x] Change the core score weights to angle `0.60`, length `0.35`, and deterministic random `0.05`.
- [x] Compress base placement influence to `0.60..1.00` using the existing `0.06..0.20` raw suppression range.
- [x] Compress upward orientation influence to `0.925..1.075` using the existing `0.82..1.08` raw boost range.
- [x] Remove `edgeCharacterBoost` from intra-object rank multiplication while preserving its recorded evidence field.
- [x] Preserve Coverage, descending sort behavior, coexistence, widths, corners, geometry, and certification.

### Analyzer and reports

- [x] Reproduce the R12B.1 formula in `current-exact` score validation.
- [x] Update current/no-random/no-modifier/no-gate and current-plus context scenarios to the R12B.1 core weights and compressed placement factors.
- [x] Preserve the `1,931` scenarios-per-case universe and existing comprehensive CSV schemas.
- [x] Update the artistic audit formula text and advance all report contracts to `EW-B4.2R12B.1`.

### Runtime acceptance

- [x] Compile with zero C# errors.
- [x] Run **Full Edge-Wear Validation Suite (1 Click)** once.
- [x] Require suite pass, topology `33/33`, preview `33/33`, comprehensive evidence available, valid recorded ranks, score-reproduction error no greater than `0.000002`, and zero collateral loss/change. The accepted run passed both matrices `33/33`, retained comprehensive evidence, reproduced current scores within `5.96046448E-08`, and reported collateral `36/36/0/0/0/1`.
- [x] Visually compare seeds `2223`, `5727`, and `8889`. R12B.1 materially improved ordinary ranking and is accepted as the artistic-selection baseline. Remaining omissions are geometric outliers rather than ranking failures: seed `2223` edge `36` and seed `8889` edges `13/23` fail isolated-rail viability; seed `2223` edge `13` fails width/corner feasibility.


## GM-R12B.1C — Baseline closeout and live render-integrity proof tooling

### Accepted boundary

- [x] Keep EW-B4.2R12B.1 as the active artistic-selection baseline; do not retune ranking while the remaining outliers are geometric.
- [x] Preserve the unresolved outlier set explicitly: `2223/36`, `2223/13`, `8889/23`, and `8889/13`.
- [x] Treat the black-triangle/Bloom artifact as a broader render-mesh integrity problem that may still have structural implications for those outliers depending on the eventual production repair.

### Combined diagnostic and proof implementation

- [x] Add one explicit **Mesh Diagnostics** inspector section for a selected `GeneratedMass`.
- [x] Audit the already-generated `MeshFilter.sharedMesh` without regeneration, mutation, serialization, or automatic execution.
- [x] Inspect finite positions, normals, tangents, UV0, UV2, and colors; normal/tangent magnitude; robust position outliers; triangle index validity; 3D area/sliver conditioning; UV determinant; stored-normal agreement; and outward winding.
- [x] Write one compact report to `Library/GeneratedMassRenderMeshAudit.txt`, including exact worst-triangle evidence and capped worst UV/tangent lists.
- [x] Draw the worst triangle and vertex indices in the Scene view with optional X-ray depth behavior.
- [x] Initially add a temporary non-serialized tangent-only proof clone. GM-R12B.1D supersedes it with the normal/tangent proof after zero normals were proven.
- [x] Add a temporary non-serialized Unlit proof clone using the untouched audited mesh.
- [x] Suppress and restore the source renderer only while a proof clone is active; remove the proof automatically when the mass is deselected.
- [x] Keep all production mesh generation, `MeshData`, `MeshBuilder`, UV construction, shaders, materials, scenes, and prefabs unchanged until the proof identifies the exact cause.

### Required evidence before production repair

- [x] Audit `Rock_14` seed `839`, `Rock_18` seed `1468`, and seed `8889` with bevels enabled. The three live meshes each contained exactly `27` zero stored normals across `9` triangles; tangents remained finite and unit length. Seed `8889` without bevels remains part of production-fix validation.
- [x] Identify the decisive common invalid channel. Ordinary `Rock_14`/`Rock_18` meshes had no UV-degenerate or UV-ill-conditioned triangles but did have zero stored normals on the visible failure triangles; seed `8889` preview combined the same zero-normal defect with UV conditioning warnings.
- [x] Use the Unlit proof clone at the retained failure angle. It removed the visible dark-triangle/Bloom symptom, consistent with a Lit basis failure. The tangent-only proof correctly refused to proceed because the affected vertices had zero normals.
- [x] Promote the smallest proven repair in GM-R12B.1D: Generated Mass explicit normal normalization plus Generated Mass-specific final channel validation; shared `MeshData`/`MeshBuilder` semantics remain unchanged.


## GM-R12B.1D — Generated Mass render-normal integrity repair

### Proven cause

- [x] Confirm `Rock_14`, `Rock_18`, and seed `8889` preview each emitted `27` zero normals while positions, UVs, colors, UV2, and tangents remained finite.
- [x] Confirm the affected ordinary triangles had valid 3D area and valid UV determinants, excluding UV-conditioned tangent reconstruction as the common cause.
- [x] Identify the normalization-threshold mismatch: Generated Mass accepted cross products above `MinimumEdgeLengthSqr = 1E-12`, then called Unity `Vector3.normalized`, which can return zero for magnitudes below Unity's larger normalization epsilon. Measured failing double areas `4.12636973E-06` and `8.067349E-06` lie inside that mismatch band.

### Production repair

- [x] Add one explicit `TryNormalizeMassVector` contract used by authored and geometric render normals.
- [x] Remove the silent `Vector3.up` fallback for accepted triangles; invalid accepted geometry now fails deterministically with the exact face index.
- [x] Validate Generated Mass `MeshData` positions, normals, UV0, UV2, colors, triangle indices, geometric normal construction, and stored-normal/winding agreement before mesh application.
- [x] Validate the final Unity mesh after `RecalculateTangents()` for complete finite positions, unit normals, unit tangents, valid handedness, UV0, UV2, and colors.
- [x] Increment `ProductionGenerationContractVersion` from `1` to `2` so previously accepted transient meshes regenerate once under the corrected normal contract.
- [x] Keep shared `MeshData`, `MeshBuilder`, UV construction, geometry, topology, materials, shaders, scenes, and prefabs unchanged.

### Diagnostic update

- [x] Advance the audit contract to `GM-R12B.1D-render-audit-v2`.
- [x] Treat zero normals as hard failures and prioritize them ahead of UV-conditioning warnings when selecting the worst triangle.
- [x] Replace the tangent-only proof with a temporary **Normal/Tangent Repair Proof Clone** that reconstructs invalid normals from triangle geometry before rebuilding only affected or unsafe tangents.

### Runtime acceptance — superseded and completed by GM-R12B.1E

- [x] Compile with zero C# errors.
- [x] Regenerate `Rock_14` seed `839`, `Rock_18` seed `1468`, seed `8889` without bevels, and seed `8889` with bevels; GM-R12B.1E completes the scale-correct form of this repair with zero invalid normals/tangents.
- [x] Confirm the black triangle and Bloom orb no longer reproduce at the retained camera angles.
- [ ] Run the combined R13A.1 one-click edge-wear suite and retain topology `33/33`, preview `33/33`, valid comprehensive evidence, and zero collateral loss/change.
- [ ] Re-evaluate `2223/36`, `2223/13`, `8889/23`, and `8889/13` under R13A.1.


## GM-R12B.1E — scale-correct normal repair follow-up

### Runtime evidence from GM-R12B.1D

- [x] Regenerate and audit `Rock_14` seed `839`: zero invalid normals/tangents and no visible black-triangle/Bloom artifact.
- [x] Regenerate and audit `Rock_18` seed `1468`: zero invalid normals/tangents and no visible black-triangle/Bloom artifact.
- [x] Confirm the promoted zero-normal repair solved the original visual failure.
- [x] Identify seed `8889` face `76` as a tiny but healthy triangle (`doubleArea=8.559025E-07`, `relativeArea=0.296998173`) rejected only by the absolute normal cutoff.
- [x] Identify the dimensional mismatch: cross-product magnitude squared is length^4 and may not be compared to `MinimumEdgeLengthSqr` in length^2.

### Correction

- [x] Replace production normal normalization with finite non-zero double-precision normalization.
- [x] Apply the same normalization semantics to editor audit geometric normals and normal/tangent proof reconstruction.
- [x] Keep the existing scale-relative triangle quality tests authoritative; do not add a new absolute triangle-size floor.
- [x] Advance the live audit contract to `GM-R12B.1E-render-audit-v3`.
- [x] Report finite UV-conditioning findings as `passed-with-warnings`; reserve `failed` for invalid indices/channels, zero or non-finite normals/tangents, non-finite geometry, or degenerate 3D triangles.
- [x] Keep `ProductionGenerationContractVersion = 2`; this patch corrects the implementation of that contract without changing its reuse semantics.

### Runtime acceptance

- [x] Compile with zero C# errors.
- [x] Regenerate seed `8889` without bevel preview; face `76` completes with finite unit geometric and stored normals.
- [x] Regenerate seed `8889` with bevel preview; zero missing, non-finite, zero, or non-unit normals/tangents.
- [x] Re-audit `Rock_14` and `Rock_18`; zero invalid channels retained and the black-triangle/Bloom artifact no longer reproduces.
- [ ] Run the combined R13A.1 one-click edge-wear suite and retain topology `33/33`, preview `33/33`, comprehensive availability, and zero collateral regression.
- [ ] Recheck `2223/36`, `2223/13`, `8889/23`, and `8889/13` under the R13A.1 recovery contract.

## EW-B4.2R13A.1 — isolated-rail and width-monotonic outlier recovery

**Status:** rejected by Unity runtime validation; superseded by EW-B4.2R13A.2.

### Locked baselines

- [x] Keep EW-B4.2R12B.1 as the accepted artistic-selection baseline. Do not alter the angle gate, score weights, placement compression, deterministic random contribution, descending-score order, or Coverage selected-count contract.
- [x] Close the runtime-proven GM-R12B.1E normal-integrity repair in this combined patch rather than creating a standalone closeout patch.
- [x] Keep shared `MeshData`, `MeshBuilder`, shaders, materials, UV projection, scenes, and prefabs unchanged.

### Isolated-rail recovery

- [x] Calculate the solved endpoint parameter against the exact adjacent source-edge segment in double precision.
- [x] Derive parameter tolerance from the existing absolute point tolerance divided by exact boundary length.
- [x] Permit only endpoint overshoot that remains inside that same absolute spatial tolerance, then clamp to the exact segment endpoint.
- [x] Remove endpoint proximity itself as an exclusion. A point at or near a legitimate source-edge endpoint must proceed to the existing plane, displacement, provenance, distinct-edge, collapse, containment, topology, bounds, volume, and face-quality checks.
- [x] Do not walk onto another source edge, invent support geometry, revive the rejected support-chain fallback, or bypass any downstream certification.
- [x] Preserve successful canonicalization evidence and include complete raw parameter/snap evidence in the failure diagnostic if a point remains outside tolerance.

### Width-monotonic viability

- [x] Define the viability floor from the canonical minimum style width (`Edge Wear Width = 0.05`) rather than as a fraction of the current requested width.
- [x] Require a certified local width of at least `minimumStyleWidth * 0.25`.
- [x] Continue solving the actual width as the locally certified width capped by the current request; increasing global width may cap a constrained edge but may not remove it solely because its fraction of the larger request became small.
- [x] Preserve the old requested-width fraction as diagnostic evidence only.

### Bounded shared-edge retention

- [x] Invoke retention search only when the existing uniform shared-edge scale would deactivate at least one participating selected edge.
- [x] Hard-cap the local search at six participants and therefore at most 63 non-empty subsets.
- [x] For each retained subset, defer the other local participants, solve the subset's own stable common scale, and reject any retained width below the existing minimum stable width.
- [x] Select a valid result by greatest retained count, highest summed production artistic score, greatest retained certified width, then deterministic source-edge order.
- [x] Preserve the existing safe uniform-scale/all-defer behavior when no better certified subset exists.
- [x] Run every committed result through the unchanged complete corner, replacement-face, rail, plane-shell, topology, containment, bounds, volume, and face-quality audits.
- [x] Add one editor-only five-check outlier contract over topology-matrix cases so the one-click suite cannot pass merely because a target edge remained geometrically excluded. Production behavior contains no seed or edge-ID branch.

### Target runtime acceptance

- [ ] Full suite contract is `EW-B4.2R13A.1-suite`; topology, preview, and comprehensive contracts use the matching R13A.1 suffix, and `outlierRecoveryChecks=5/5`.
- [ ] Topology matrix passes `33/33`.
- [ ] Artistic-preview matrix passes `33/33`.
- [ ] Comprehensive evidence remains available; recorded production ranks remain valid; current score reproduction remains within tolerance.
- [ ] Collateral lost/changed, topology failures, face-quality failures, and placement failures remain zero.
- [ ] Seed `2223`, edge `36` becomes active and certified, or returns a new exact downstream certification failure proving the bounded endpoint canonicalization was not sufficient.
- [ ] Seed `8889`, edges `13` and `23` become active and certified, or return new exact downstream certification failures.
- [ ] Seed `2223`, edge `13` remains present at its certified local width across default and maximum requested widths and survives the corner solution.
- [ ] Re-audit representative ordinary and bevel-preview meshes; retain zero invalid normals/tangents and absence of the black-triangle/Bloom artifact.

## EW-B4.2R13A.1 runtime result — rejected

- [x] Run the R13A.1 one-click suite.
- [x] Record topology `31/33`, artistic preview `31/33`, and outlier recovery `0/5`.
- [x] Confirm the target isolated-rail misses are not numerical tolerance errors: `2223/36`, `8889/13`, and `8889/23` land materially outside the presumed adjacent segment.
- [x] Confirm `2223/13` becomes provisionally geometric but remains `corner-width-inactive` under the local retention model.
- [x] Record the two maximum-width regressions: seed `1112` terminal plane-band split and seed `5556` final winding/normal guard rejection.
- [x] Reject endpoint clamping, unconditional global width monotonicity, and local 63-state retention as the accepted recovery architecture.

## EW-B4.2R13A.2 — owner-boundary and full-shell conflict recovery

**Status:** rejected by Unity runtime validation; superseded by EW-B4.2R13A.3.

### Locked boundaries

- [x] Preserve EW-B4.2R12B.1 artistic gates, score, ordering, deterministic random contribution, and Coverage count.
- [x] Preserve GM-R12B.1E scale-correct normal generation and final render-channel guards.
- [x] Keep shared `MeshData`, `MeshBuilder`, UV generation, shaders, materials, scenes, prefabs, and generation-contract version unchanged.
- [x] Retain the five editor-only outlier fixtures; do not add seed or edge-ID branches to production.

### Complete owner-face boundary resolution

- [x] Remove R13A.1 endpoint overshoot authorization and clamping.
- [x] Intersect each isolated support ray against every manifold boundary segment on its exact owner source face, excluding the selected edge.
- [x] Reject backward, non-finite, off-segment, non-manifold, and ambiguous nearest hits.
- [x] Deduplicate coincident vertex hits and select only a unique nearest forward terminal.
- [x] Preserve exact original-adjacent and resolved-boundary evidence.
- [x] Keep all existing plane, displacement, provenance, distinct-boundary, collapse, topology, containment, bounds, volume, replacement-face, and render-channel checks.

### Full-shell retention

- [x] Restore requested-width fraction as the ordinary viability gate.
- [x] Mark an edge provisional only when isolated construction certified an absolute width at the canonical minimum style floor.
- [x] Invoke conflict search only when a selected provisional edge exists; all ordinary cases retain the direct R12B.1E path.
- [x] Remove the local 63-state shared-edge subset search.
- [x] Publish corner-collapse participants as branch candidates.
- [x] Preserve terminal plane-band victim/foreign evidence as branch candidates.
- [x] Treat final render-normal/winding rejection as an invalid state; do not weaken the guard.
- [x] Cap search at 128 states and 10 forced deferrals.
- [x] Rank valid states by certified count, summed production artistic score, total certified width, then deterministic edge order.
- [x] Evaluate trials on cloned lifecycle audits and rerun only the winning state against authoritative evidence.

### Runtime acceptance

- [ ] Compile with zero C# errors.
- [ ] Full suite contract is `EW-B4.2R13A.2-suite`; topology, preview, and comprehensive reports use matching contracts.
- [ ] Topology matrix passes `33/33`.
- [ ] Artistic-preview matrix passes `33/33`.
- [ ] Outlier recovery passes `5/5`: `2223/max/36`, `2223/default/13`, `2223/max/13`, `8889/max/13`, and `8889/max/23` are active and certified.
- [ ] Seed `1112/maximum` no longer ends in an unresolved edge-6/edge-7 band split.
- [ ] Seed `5556/maximum` produces no final winding/normal exception.
- [ ] Comprehensive evidence remains available with valid recorded ranks and score reproduction.
- [ ] Collateral lost/changed, topology, face-quality, placement, and render-channel failures remain zero.
- [ ] Representative render audit retains zero invalid normals/tangents and no black-triangle/Bloom regression.

## EW-B4.2R13A.2 runtime result — rejected

- [x] Unity compiled sufficiently to start the one-click suite.
- [x] Current seed `8889` preview remained valid and materialized.
- [x] Topology completed `24/24` cases before cancellation.
- [x] Record the stall at topology case `24/33`: `seed 7778`, maximum width.
- [x] Cancel after more than ten minutes; preview `0/0`, outlier `0/0`, comprehensive unavailable.
- [x] Diagnose nested 128-state provisional and 128-state coexistence frontiers as the execution explosion.
- [x] Reject R13A.2 search ownership; do not rerun the unchanged suite.

## EW-B4.2R13A.3 — single-search execution correction

**Status:** rejected by Unity runtime validation; superseded by EW-B4.2R13A.4.

### Execution architecture

- [x] Prevent provisional full-shell states from invoking the plane-kernel coexistence frontier.
- [x] Retain exactly one active conflict frontier per evaluation path.
- [x] Stop provisional search at the first fully certified priority-ordered state.
- [x] Order equal-depth states by removed R12B.1 artistic score, removed certified width, and deterministic edge order.
- [x] Keep the 128-state and ten-forced-deferral caps.
- [x] Add a five-second audit search budget with explicit terminal evidence.
- [x] Add synchronous progress-bar cancellation polling between search states.
- [x] Clear the transient editor cancellation callback in `finally` and do not append a cancelled partial case.
- [x] Preserve complete owner-face boundary resolution, provisional-width semantics, the five outlier fixtures, and all render/topology guards.

### Runtime acceptance

- [ ] Compile with zero C# errors.
- [ ] Full suite contract is `EW-B4.2R13A.3-suite`; topology, preview, and comprehensive reports use matching contracts.
- [ ] `seed 7778 / maximum` returns within five seconds or reports the explicit time-budget failure; it must never lock the editor indefinitely.
- [ ] Topology matrix passes `33/33`.
- [ ] Artistic-preview matrix passes `33/33`.
- [ ] Outlier recovery passes `5/5`.
- [ ] Comprehensive evidence remains available and collateral/topology/face-quality/placement/render failures remain zero.


### EW-B4.2R13A.3 runtime result — rejected

- [x] Unity compilation completed after the R13A.3a `System.Globalization` import correction.
- [x] The suite completed without the former multi-minute editor lockup.
- [x] Topology matrix returned `31/33`; failures were `seed 1/maximum` and `seed 7778/maximum` at the five-second search boundary.
- [x] Artistic-preview matrix returned `31/33` at the same coordinates.
- [x] Outlier recovery returned `0/5`.
- [x] Current seed `8889` preview was erased (`applied=0`) when the optional recovery search found no certified shell.
- [x] Comprehensive evidence was unavailable because timed-out cases returned empty artistic records.
- [x] Reject replacement-solve recovery; retain the single-frontier and cancellation safeguards only.

## EW-B4.2R13A.4 — certified baseline augmentation and multi-support endpoints

**Status:** stable incomplete runtime baseline. Safety floor passed; outlier recovery remained `0/5` and is continued by R13A.6.

### Certified baseline fallback

- [x] Build and fully certify an ordinary baseline with selected provisional recovery edges forced off.
- [x] Retain baseline corner solution, plane audit, preview soup, lifecycle evidence, and certification metrics as immutable fallback.
- [x] Discover corner-inactive recovery participants from baseline corner-conflict evidence.
- [x] Start augmentation from the baseline exclusion set with recovery participants re-enabled.
- [x] Disable kernel coexistence recursion during augmentation trials so only one frontier is active.
- [x] Commit augmentation only when it is fully certified, recovers at least one absent participant, and is superior by count, score, then width.
- [x] On timeout, state exhaustion, cancellation, or no superior shell, retain and report the certified baseline instead of clearing preview or matrix evidence.
- [x] Publish explicit baseline/applied and augmentation state, elapsed time, frontier, last failure, and implicated-edge evidence.

### Multi-support endpoint construction

- [x] Detect endpoints whose exact owner-boundary rails resolve through different support faces.
- [x] Use the four exact rails to authorize one selected-edge bevel half-space cut across the convex source shell.
- [x] Require the solid centre and every foreign source vertex to remain while both selected source-edge endpoints lie on the removed side.
- [x] Require one unique bounded bevel cap and preserve all four solved rail terminals on its boundary.
- [x] Preserve complete unique source-face provenance and classify only exact cut-plane support-interval modifications as expected.
- [x] Keep strict intersection, manifold, containment, convexity, bounds, volume, face-quality, sidedness, triangulation, and render-channel certification mandatory.
- [x] Keep ordinary same-support endpoint construction unchanged.

### Runtime acceptance

- [x] Compile with zero C# errors.
- [x] Full suite contract is `EW-B4.2R13A.4-suite`; topology, preview, and comprehensive reports use matching contracts.
- [x] Current seed `8889` preview remains applied even when optional augmentation fails.
- [x] Seed `1/maximum` and `7778/maximum` return certified baseline records rather than empty collateral failures.
- [x] Topology matrix passes `33/33`.
- [x] Artistic-preview matrix passes `33/33`.
- [x] Comprehensive evidence is available with valid recorded ranks and score reproduction.
- [ ] Outlier recovery passes `5/5`.
- [x] Collateral lost/changed, topology, face-quality, placement, and render-channel failures remain zero.
- [ ] Representative render audit retains zero invalid normals/tangents and no black-triangle/Bloom regression.


## EW-B4.2R13A.6 — baseline restoration, retained-point hull, and finalized corner injection

**Status:** runtime safety validated, recovery incomplete. The one-click suite compiled and preserved current preview, topology `33/33`, artistic preview `33/33`, comprehensive evidence, edge `39` active, and edge `40` inactive, but all five historical fixtures remained unresolved. R13A.7 supersedes the five-certified-only closure gate; R13A.4/R13A.6 remains the stable incomplete safety state.

- [x] Restore R13A.4 ordinary geometry and corner behavior; do not retain R13A.5 sampled split-plane geometry.
- [x] Restrict augmentation initiation to certified multi-support retained-hull edges and finalized corner-inactive participants.
- [x] Build the exact retained point set from all original vertices except the selected endpoints plus four exact rails.
- [x] Enumerate and merge global supporting hull planes; emit a connected bevel-facet band with complete source provenance.
- [x] Reject any result that modifies a source face outside the two endpoint stars or loses a source-face provenance record.
- [x] Attempt finalized `corner-width-inactive` capture. Runtime evidence later proved the R13A.6 source incomplete because the forced-deferral zeroing branch emitted no conflict record.
- [x] Protect recovery targets and seed bounded neighbour-deferral subsets from their exact conflict records.
- [x] Forbid certified baseline-edge loss outside recovered corner participants and forbid any certified-count reduction.
- [x] Advance suite, topology, preview, and comprehensive contracts to R13A.6.
- [x] Unity compiles with zero errors.
- [x] Current seed `8889` restores R13A.4 identity: edge `39` active and edge `40` inactive.
- [x] Full suite retains current preview, topology `33/33`, preview `33/33`, and comprehensive evidence.
- [x] Runtime result recorded: outlier recovery reached `0/5`. Exact terminal blockers were preserved for the three multi-support fixtures, but the two `2223/13` corner fixtures still lacked zeroing provenance; closure moves to R13A.7.


## EW-B4.2R13A.7 — recovery closure, discrete-schedule proof, and edge-40 negative gate

**Status:** Unity runtime-tested and diagnostically useful, but recovery remains incomplete. The suite compiled, retained current preview plus both `33/33` matrices and comprehensive evidence, resolved only `3/5` positive fixtures with zero certified recoveries, left the two seed-`2223` corner fixtures unresolved, and failed the edge-40 negative gate only because that assertion required one exact exclusion reason. R13A.4/R13A.6 remains the accepted stable incomplete safety baseline; R13A.8 supersedes the seed-8889 micro-topology conclusion and the reason-specific negative assertion.

### Objective and acceptance

- [x] Preserve the R13A.6 safety floor in Unity: current preview applied, topology `33/33`, artistic preview `33/33`, comprehensive evidence available, seed `8889` edge `39` active, and edge `40` inactive.
- [ ] Resolve each of the five historical fixtures. Runtime result: `3/5` were classified complete under the current discrete construction contract, `0/5` were certified recoveries, and `2/5` remained unresolved. Visual review later rejected the feature-level infeasibility conclusion for seed-`8889` edges `13/23`.
- [x] Add an editor-only negative fixture requiring `8889/maximum/40` to remain inactive, uncertified, and unmaterialized.
- [x] Keep production selection, score weights, geometry invariants, normal/tangent semantics, and serialized assets unchanged.

### Reviewed evidence and corrected plan assumptions

- [x] Verified the supplied `Assets(69).zip` tree already contains the R13A.6 contracts and has no Git metadata; branch, HEAD, and working-tree comparisons are unavailable.
- [x] Read the current canonical framework, R13 recovery architecture/checklist/inventory, the R13A.6 suite evidence, all ten expected edit files, and direct preview, plane-kernel, mesh-output, and render-validation callers.
- [x] Confirmed `TrySolveBoundedIsolatedSingleEdgeRails` performs at most twelve `0.75` width-backoff **rail** attempts and stops at the first rail success. `AuditBoundedSingleEdgeBevel` then performs one bounded construction/certification attempt at that solved width.
- [x] Corrected the handoff assumption: R13A.7 may call a failed construction "complete-schedule infeasible" only when that terminal construction was executed at the minimum admissible width or after the full twelve-attempt rail cap. A failed construction above the floor remains unresolved because narrower constructions were not executed.
- [x] Confirmed the unrecorded corner-width path: `TrySolveCornerAwareChamferWidths` can force all positive participants to zero in its `!edgeChanged` fallback without adding a `ChamferCornerConflictRecord`; `CaptureFinalCornerInactiveRecoveryEvidence` can therefore see no conflict.
- [x] Confirmed edge `40` belongs to the width-recovery-provisional class (`feasibleWidthFraction` below the ordinary `0.25` gate) and must not initiate corner augmentation.

### Approved files and implementation sequence

1. [x] `MassGenerator.EdgeWear.Types.cs`: add bounded diagnostic records for rail-width attempts, schedule completion, corner zeroing stage, and corner recovery resolution.
2. [x] `MassGenerator.EdgeWear.BoundedSingleEdge.cs`: record the already-executed width attempts and terminal construction evidence without adding geometry attempts.
3. [x] `MassGenerator.EdgeWear.SelectionAndCorners.cs`: record both uniform-scale and forced-deferral zeroing events; preserve ordinary width decisions.
4. [x] `MassGenerator.EdgeWear.Orchestration.cs`: admit only ordinary feasible-width corner targets, preserve baseline identity, and mark only fully exhausted target-aware searches as proven infeasible.
5. [x] `MassGenerator.EdgeWear.Diagnostics.Logging.cs`: publish ordered width-attempt, schedule-resolution, corner-stage, and corner-resolution evidence.
6. [x] `Editor/GeneratedMassEditor.cs`: replace the five-certified-only gate with certified/proven/unresolved resolution counts and add the edge-40 negative fixture.
7. [x] Update `Generated_Mass_Framework.md`, `Generated_Mass_Edge_Wear_Recovery_Architecture.md`, and `Generated_Mass_Edge_Wear_Code_Inventory.md` after code is final.

### Invariants, non-goals, and performance

- [x] No additional hull, plane, rail, or full-shell attempt is authorized by R13A.7.
- [x] No continuous-width proof, nested search, state/time-budget increase, seed-specific production branch, generic width-provisional recovery, or score/default change.
- [x] Active-gameplay cost remains zero; all new work is bounded diagnostic assignment/reporting or existing explicit editor augmentation.
- [x] Baseline commit rules remain count-, identity-, topology-, geometry-, placement-, and render-channel safe.

### Validation and compliance

- [x] Parsed all `153` project C# files with tree-sitter C# with zero syntax errors; introduced method definitions/calls and overload arities were scanned, including the retained nine-argument `TrySolveBoundedIsolatedSingleEdgeRails` caller.
- [x] Preserve CRLF, preprocessor balance, and absence of malformed multiline strings, trailing whitespace, and conflict markers.
- [x] Confirm the final diff contains only the ten approved paths and no serialized/shared-render changes.
- [x] Unity compiles with zero introduced errors.
- [x] One-click suite retains current preview, topology `33/33`, preview `33/33`, comprehensive evidence, and zero reported collateral/topology/face-quality/placement/render regressions.
- [ ] Positive fixture closure reached only `3/5`; seed-`8889` edges `13/23` must be recovered rather than accepted as infeasible.
- [ ] Edge-40 negative assertion reported `0/1` even though edge `40` was inactive, uncertified, and unmaterialized; R13A.8 broadens the assertion to any definitive exclusion while forbidding micro suppression.
- [x] Static invocation comparison confirms no added rail-at-width, bounded construction, single-plane, retained-hull, plane-kernel, or augmentation-trial geometry call site.
- [x] The full suite completed without cancellation or a five-second guard breach; R13A.8 must remeasure because normalization adds bounded explicit editor work.

## EW-B4.2R13A.8 — micro-topology normalization

**Status:** Unity runtime-validated. Current preview passed; topology and artistic-preview matrices passed `33/33`; micro component `14/24/30` normalized; seed-8889 edges `13/23/39` certified; edge `40` remained excluded; the negative fixture passed. The only unresolved positive fixtures are seed-2223 edge `13` at default and maximum width.

### Objective and evidence

- [x] Record the user decision that topology below useful visible feature scale may be completely consumed inside the edge-wear transaction when required to build meaningful bevels.
- [x] Preserve the base Generated Mass source mesh and production `EdgeWearEvaluationMode.None` path unchanged.
- [x] Treat seed `8889` source edge `24` (`length≈0.002811`) as the sub-style seed because it is shorter than the current global minimum style width (`≈0.002993`).
- [x] Expand the authorized component to the complete microscopic triangle `14/24/30`: collapsing edge `24` alone aliases the two remaining triangle edges, while all three edges and the component diameter remain far below the global minimum certified bevel footprint (`≈0.006027`).
- [x] Unity proved that suppressing `14/24/30` recovers original source edges `13` and `23` as geometrically viable, selected, active, and certified at maximum width.
- [x] Unity retained source edge `39` active/certified and source edge `40` inactive/uncertified.
- [x] Unity preserved topology and artistic-preview matrices at `33/33`, current preview, comprehensive evidence, and zero terminal collateral/topology/face-quality/placement/render-channel regression.

### Implemented architecture

1. [x] Build an original source graph only after the explicit non-`None` evaluation gate, preserving stable source-edge identity and provenance.
2. [x] Detect internal-manifold seed edges at or below the minimum useful style width, then expand through connected internal edges no longer than the global minimum certified bevel footprint; require the complete component diameter to remain within that footprint.
3. [x] Cap every component at six vertices and eight edges.
4. [x] Evaluate deterministic collapse to every existing component vertex; never invent a midpoint or expand the source hull.
5. [x] Rebuild a temporary convex base hull from remapped retained source vertices and accept only a closed, finite, positive-volume, non-expanding result with at most `0.25%` volume loss.
6. [x] Require every non-component source edge to survive endpoint remapping without collapse, aliasing, or disappearance.
7. [x] Choose the certified collapse with minimum total squared displacement, then minimum volume loss, then lowest original graph-vertex index.
8. [x] Use normalized faces as the source of truth for viability, artistic selection, corner solving, coexistence, bounded/plane construction, triangulation, and render validation.
9. [x] Keep internal normalized graph indices separate from stable original/display IDs. Suppressed source edges receive `micro-topology-suppressed` and overlay code `M`; generated transition edges receive synthetic IDs and remain structural-ineligible.
10. [x] Preserve deterministic artistic variation through original source-edge identity.
11. [x] Retain the complete existing shell, convexity, winding, source-provenance, bounds, volume, face-quality, placement, normal, tangent, and render-channel gates after normalization.
12. [x] Record bounded component/candidate evidence: seed/all edge IDs, graph vertices, diameter, every canonical-vertex attempt, displacement, volume/loss, exact blocker, and selected candidate.

### Suite and diagnostic semantics

- [x] Advance suite, topology, artistic-preview, and comprehensive contracts to R13A.8.
- [x] Require seed-`8889` maximum-width edges `13` and `23` to pass only as certified recoveries; a discrete infeasibility label is no longer accepted for these visually required fixtures.
- [x] Accept edge `40` as a negative fixture under any definitive inactive, uncertified, unmaterialized exclusion reason, while rejecting unresolved/provisional states and explicitly rejecting micro suppression.
- [x] Preserve unique original source IDs in public artistic records and use synthetic IDs only for generated transition edges, preventing normalized graph-index collisions with fixtures or comprehensive ranking evidence.
- [x] Extend detailed reports and comprehensive edge CSV evidence with micro-suppressed/generated-transition state.

### Scope and non-goals

- [x] Code changes are limited to `MassGenerator.cs`, `MassGenerator.EdgeWear.Graph.cs`, `MassGenerator.EdgeWear.Types.cs`, `MassGenerator.EdgeWear.SelectionAndCorners.cs`, `MassGenerator.EdgeWear.Orchestration.cs`, `MassGenerator.EdgeWear.Diagnostics.Logging.cs`, and `Editor/GeneratedMassEditor.cs`.
- [x] Documentation changes are limited to the framework, recovery architecture, checklist, and code inventory.
- [x] No scene, prefab, material, shader, metadata, layer, tag, component, shared mesh infrastructure, production score, normal/tangent semantic, serialized default, or production generation-contract change.
- [x] No seed-specific production behavior, global source-face relaxation, arbitrary face deletion, continuous-width scan, nested search, or larger search budget.
- [x] Edge `40` is larger than the bounded component threshold and the negative fixture also forbids micro suppression.

### Performance contract

- [x] Active-gameplay impact remains zero because normalization runs only after the explicit non-`None` edge-wear evaluation gate.
- [x] Normalization runs once per explicit preview/audit evaluation; component size is bounded to six canonical candidates, and each candidate reuses the existing convex-hull utilities.
- [x] Cases with no sub-style seed perform only the original graph build and component scan; no bevel geometry search, rail schedule, plane-kernel frontier, or augmentation budget was expanded.
- [x] Compared R13A.8 with R13A.7: topology maximum total remained effectively unchanged (`3225.65 ms` versus `3224.45 ms`), artistic maximum total decreased (`996.31 ms` versus `1015.76 ms`), and no case breached the existing five-second guard.

### Static validation and delivery

- [x] Parse all `153` project C# files with tree-sitter C# and require zero syntax errors.
- [x] Scan introduced method definitions/calls and overload arities, required namespaces, contract strings, fixture isolation, multiline strings, preprocessor balance, CRLF, trailing whitespace, and conflict markers.
- [x] Confirm the final diff contains exactly the eleven declared paths.
- [x] Reconstruct the seed-`8889` source graph from the R13A.7 report: `42` edges, `28` unique vertices, seed edge `{24}`, connected component `{14/24/30}`, and no connection to edge `40`.
- [x] Independently test all three existing-vertex collapse candidates against the reported point cloud: each preserves every non-component source edge without identity collision and produces a contained positive-volume convex hull; the minimum-displacement candidate loses only a microscopic fraction of volume.
- [x] Clean-apply the delivery patch to the exact R13A.7 source and verify all eleven changed files byte-identical; the final package repeats this check after documentation reconciliation.
- [x] Unity compiled and the full R13A.8 one-click suite completed. The micro-topology result is accepted; suite status remained failed only because two seed-2223 width-recovery fixtures were unresolved.


## EW-B4.2R13A.9 — Material Width-Recovery Closure and Canonical Diagnostics

**Status:** Unity runtime-tested and rejected as incomplete. Both 33-case matrices and the current preview passed, canonical diagnostics remained correct, but material-width recovery attempted neither seed-2223 edge-13 fixture, left the suite at `3/5`, and triggered an unrelated five-second augmentation timeout for seed 7778 at maximum width.

### Objective

Close the two remaining seed-2223 / source-edge-13 fixtures without weakening the certified R13A.8 baseline, while preserving the accepted micro-topology normalization result for seed 8889 and the deliberate exclusion of tiny source edge 40.

### Corrected diagnosis

R13A.8 runtime evidence and current code inspection show that seed-2223 edge 13 is not driven from a positive shared-corner width to zero by an uncaptured corner-clamp event. It is classified as `WidthRecoveryProvisional` because isolated construction succeeds below the ordinary requested-width fraction, then is intentionally forced to zero in the immutable certified baseline. The augmentation collector excludes all width-recovery-provisional edges, so the target is never attempted and cannot produce corner-conflict provenance. The final `corner-width-inactive` label therefore conflates baseline recovery deferral with genuine corner inactivation.

### Approved scope

Modify only:

- `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
- `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
- `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Docs/Generated_Mass_Framework.md`
- `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`
- `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
- `Game/Procedural/Masses/MassGenerator.EdgeWear.Orchestration.cs`
- `Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs`
- `Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`

No scene, prefab, material, shader, metadata, shared mesh infrastructure, base Generated Mass mesh, production score, geometry kernel, or `EdgeWearEvaluationMode.None` behavior may change.

### Implementation plan

- [x] Preserve every raw `WidthRecoveryProvisional` edge in the forced-zero certified baseline.
- [x] Add a seed-independent material-significance gate for width recovery: the edge must remain artistically eligible and its source length must support at least two requested bevel footprints.
- [x] Admit only materially significant width-provisional edges into bounded augmentation; tiny or barely structural edges remain baseline-only exclusions.
- [x] Seed the bounded frontier with both the combined recovery state and one target-wise state per recovery edge, so a viable target is tested without unrelated provisional edges.
- [x] Preserve the existing 128-state, five-second, cancellation, full-shell certification, non-decreasing certified-count, and baseline-identity guards. Width recovery may not exchange or remove any certified baseline edge.
- [x] Record forced baseline recovery deferral explicitly rather than reporting it as a genuine corner-width collapse.
- [x] Record width-recovery eligibility and terminal resolution in structured lifecycle evidence.
- [x] Advance the one-click suite to R13A.9 and recognize certified recovery or finite target-aware augmentation exhaustion for the two seed-2223 fixtures.
- [x] Canonicalize existing `planeEdges` diagnostics through source-edge provenance while retaining graph-index evidence separately for debugging.
- [x] Update the architecture, framework, and inventory after implementation is final.

### Acceptance criteria

- Seed 2223 edge 13 at default and maximum width is either certified at its already isolated-certified width or receives explicit finite target-aware augmentation exhaustion evidence.
- Seed 8889 edges 13, 23, and 39 remain active and certified.
- Seed 8889 edge 40 remains inactive, uncertified, unmaterialized, and not admitted as a material width-recovery target.
- Micro-topology component `14/24/30` remains normalized and suppressed.
- Current preview passes; topology and artistic-preview matrices remain 33/33; comprehensive evidence remains available.
- No collateral identity loss, topology, face-quality, placement, render-channel, normal, tangent, timeout, or state-preservation regression occurs.

### Performance contract

No active-gameplay work is added. R13A.9 reuses existing isolated widths and the existing bounded full-shell augmentation evaluator. It adds only a bounded number of initial frontier states and structured report fields during explicit preview/audit evaluation. No new width search, hull algorithm, plane-cut algorithm, or per-frame path is authorized.

### Validation required before acceptance

- [x] Parse or compile every changed C# file and scan introduced references/imports.
- [x] Confirm no geometry-kernel invocation count or production-path call site changed.
- [x] Confirm exact declared file scope, CRLF preservation, preprocessor balance, and no conflict markers.
- [x] Unity compiled with zero reported C# errors.
- [x] The one-click R13A.9 suite completed and its complete report was reviewed.
- [x] Runtime evidence was reconciled: topology and artistic-preview remained `33/33`, but both seed-2223 edge-13 fixtures remained unresolved with zero recovery execution, and seed-7778 maximum width exhausted the five-second augmentation budget.

### Static implementation result

- All `153` project C# files parse with zero tree-sitter C# syntax errors.
- Introduced method definitions/calls, constructor arities, field references, and required namespace imports pass static scans.
- The final working diff contains exactly the nine approved paths, preserves CRLF, has balanced preprocessor directives, and contains no conflict markers or trailing whitespace.
- Geometry/search invocation-site counts match R13A.8; no new rail, hull, bounded-construction, plane-kernel, or augmentation evaluator call site was added.
- Reconstructed fixture evidence classifies seed-2223 edge `13` as material at both default and maximum width, while seed-8889 edge `40` fails the same general gate.
- The delivery patch clean-applies to the exact R13A.8 source and reproduces all nine changed files byte-for-byte; the final package repeats this verification after documentation reconciliation.
- Unity runtime evidence supersedes the static expectation: R13A.9 preserved correctness but failed its execution objective and introduced a material editor-time regression. R13A.9 is not an accepted recovery baseline.

## EW-B4.2R13A.9a — Immutable Material-Recovery Target Execution

**Status:** Unity-validated and accepted as the frozen uniform basic-bevel/recovery baseline. R13A.9 remains a rejected diagnostic/canonical-ID intermediate.

### Objective

Execute each materially significant width-provisional target exactly once from an immutable post-selection target set, preserve every certified baseline edge, eliminate generic branch expansion for material recovery, and publish direct eligible/deferred/attempted/certified/failed evidence. The immediate acceptance targets are seed-2223 source edge `13` at default and maximum width.

### Reviewed evidence

- `Pasted text(97).txt` reports R13A.9 suite status `failed`, outlier resolution `3/5`, both seed-2223 edge-13 fixtures unresolved, and seed-7778 maximum-width topology/artistic cases ending with `augmentation-time-budget-exceeded; certified baseline retained`.
- Seed-2223 edge `13` is artistically eligible and selected before the baseline transaction: length `0.424399436`, dihedral `93.269455`, score `0.763687432`, isolated-certified width `0.00247510313`, and requested-width fractions `0.0777777806` / `0.042857144` at default/maximum.
- `MassGenerator.EdgeWear.SelectionAndCorners.cs::RunEdgeWearIsolatedViabilityPreflight` computes `MaterialWidthRecoveryEligible`, while `MassGenerator.EdgeWear.Orchestration.cs::CollectSelectedMaterialWidthRecoveryEdges` later rediscovers targets through mutable lifecycle state.
- `MassGenerator.EdgeWear.Orchestration.cs::TryAuditCertifiedBaselineAugmentation` mixes material targets with multi-support/corner recovery in one branch-expanding frontier. This allowed unrelated seed-7778 recovery to consume the five-second budget while the intended seed-2223 target set was absent.
- `MassGenerator.EdgeWear.Orchestration.cs::MapEdgeWearCoverageAuditToGraph` is the last stable point where `context.SelectedEdges`, immutable viability evidence, canonical artistic eligibility, and graph provenance are simultaneously available before baseline trial cloning mutates lifecycle state.

### Approved scope

Modify only:

- `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
- `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
- `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Docs/Generated_Mass_Framework.md`
- `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`
- `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
- `Game/Procedural/Masses/MassGenerator.EdgeWear.Orchestration.cs`
- `Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs`
- `Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`

No geometry kernel, micro-topology implementation, base Generated Mass mesh, scene, prefab, material, shader, metadata, shared mesh infrastructure, production score, serialized default, search-budget increase, or `EdgeWearEvaluationMode.None` behavior may change.

### Invariants and non-goals

- The raw `WidthRecoveryProvisional` class remains excluded from the immutable certified baseline.
- Material recovery eligibility is seed-independent and must be derived from selected graph-edge identity plus immutable preflight/artistic evidence, not rediscovered from trial-mutated `Candidate`, `Selected`, `CoexistenceEligible`, or final-reason state.
- Each material target receives exactly one full-shell trial with that target re-enabled and all other baseline exclusions unchanged.
- A material trial may not defer, exchange, or remove any certified baseline edge. No branch expansion is permitted for material targets.
- Multi-support retained-hull and genuine corner-participant recovery remain separate and retain their existing bounded search behavior.
- No new width search, rail solve, hull construction algorithm, plane-cut algorithm, tolerance relaxation, continuous scan, or per-frame path is authorized.

### File-by-file implementation sequence

1. [x] `SelectionAndCorners.cs`: centralize the immutable material-significance predicate over preflight/artistic evidence and remove reliance on mutable trial state.
2. [x] `Types.cs`: add bounded lifecycle execution state required to distinguish immutable target membership, baseline deferral, actual trial execution, certification, and exact failure.
3. [x] `Orchestration.cs`: capture the target set once after graph mapping; pass it into baseline augmentation; run one non-branching target trial per material edge; preserve all baseline-built edges; commit successful targets sequentially; keep material outcomes separate from the existing non-material frontier.
4. [x] `Diagnostics.Logging.cs`: emit compact canonical sets for material recovery `eligible`, `baselineDeferred`, `attempted`, `completed`, `certified`, and `failed`, plus exact per-target failure evidence already produced by the trial.
5. [x] `GeneratedMassEditor.cs`: advance suite contracts to R13A.9a. Existing fixture semantics reject zero-execution unresolved states, and the production finalizer can now emit `width-recovery-proven-infeasible` only after a completed target trial.
6. [x] Canonical docs: reconcile architecture, framework, inventory, performance, and runtime acceptance wording after final code is stable.

### Acceptance criteria

- Seed-2223 edge `13` at default and maximum appears in the immutable eligible set, baseline-deferred set, and attempted set.
- Each fixture becomes certified or reports the exact completed isolated-target full-shell failure. Neither may remain unresolved with zero execution.
- Seed-7778 maximum no longer enters a branch-expanding material-recovery search and does not hit the five-second augmentation budget because of material recovery.
- Seed-8889 edges `13`, `23`, and `39` remain certified; edge `40` remains inactive and absent from the immutable material target set; component `14/24/30` remains micro-suppressed.
- Current preview passes; topology and artistic-preview matrices remain `33/33`; comprehensive evidence remains available.
- Zero terminal collateral, topology, face-quality, placement, render-channel, normal, tangent, timeout, cancellation, or state-preservation regression.

### Performance contract

- Active-gameplay impact remains zero.
- Material recovery performs exactly one existing full-shell evaluation per immutable selected target and creates no child frontier states.
- The existing non-material recovery search retains its current `128`-state / `5000 ms` limits; material trials do not consume or expand that frontier.
- No geometry invocation site is added beyond the existing trial evaluator call path; the correction changes state scheduling and evidence ownership only.

### Risks and required checks

- [x] Confirm immutable target graph IDs remain valid through every coverage clone and canonical source-ID mapping. Target IDs are passed explicitly; execution evidence is copied by graph ID and rendered through existing canonical source provenance.
- [x] Confirm a failed material trial cannot be misreported as proven infeasible unless the exact target trial completed without cancellation or timeout. The finalizer requires `Attempted && TrialCompleted && !TrialSucceeded`; cancellation/time-budget outcomes remain unresolved.
- [x] Confirm a material winner cannot erase a baseline-built edge, even when score/count tie-breaks would otherwise prefer it. Acceptance explicitly verifies every built edge in the current working baseline and requires a certified-count increase.
- [x] Confirm non-material recovery still operates from the correct certified state when material recovery succeeds or fails in the same case. Successful material results become the sequential working baseline; failed results leave it unchanged; certified material edges are protected from later branches.
- [x] Compare R13A.9a worst-case matrix time against R13A.8 and R13A.9, with explicit attention to seed 7778 maximum. R13A.9a topology maximum was `3181.0092 ms`, artistic-preview maximum was `1515.7155 ms`, and no case hit the five-second guard.

### Validation requirements

- [x] Parse all changed C# files and scan introduced definitions/calls, overload arities, imports, field propagation, and report schema consistency.
- [x] Parse all `153` project C# files with tree-sitter C# and scan multiline strings, preprocessor balance, CRLF, trailing whitespace, and conflict markers.
- [x] Compare final geometry/search invocation counts with R13A.9. Rail, bounded construction, retained-hull, plane-kernel, and non-material search call sites are unchanged; one scheduler call site invokes the existing complete-shell evaluator once per immutable material target.
- [x] Confirm the final diff contains exactly the nine approved paths and preserves all unrelated files.
- [x] Clean-apply the final patch to the exact R13A.9 source and verify every changed file byte-identical.
- [x] Unity compiled and the complete one-click R13A.9a suite passed.

### Static implementation result

- The exact working diff contains the nine approved files and no others.
- All `153` project C# files parse with zero tree-sitter C# syntax errors; introduced method arities, field references, imports, contract strings, and report-schema tokens pass static scans.
- The immutable predicate classifies the reported seed-2223 edge `13` default/maximum fixtures as targets and rejects seed-8889 edge `40` through the same two-footprint rule.
- Material recovery creates no frontier children, disables kernel conflict recursion, runs in sorted target order, and stops before non-material recovery after cancellation or timeout.
- Search constants remain `128` states, `5000 ms`, and eight additional deferrals for the separate non-material frontier.
- CRLF, terminal newlines, preprocessor/region balance, and existing fixture isolation are preserved.
- Unity runtime evidence supersedes the delivery limitation: suite status `passed`; topology `33/33`; artistic preview `33/33`; outlier resolution `5/5`; certified recoveries `2`; proven infeasible fixtures `3`; unresolved `0`; negative exclusion `1/1`; cancellation `0`; terminal reason `none`.
- R13A.9a is the accepted recovery baseline. Further work must preserve it as the exact zero-irregularity fallback rather than reopen historical recovery design without new visible evidence.

### Runtime acceptance and freeze decision

- [x] Current preview passed with `31/31` selected/certified bevels on seed `8889` maximum.
- [x] Topology matrix passed `33/33`; maximum total time `3181.0092 ms`; state preserved; failure coordinates `none`.
- [x] Artistic-preview matrix passed `33/33`; maximum total time `1515.7155 ms`; state preserved; failure coordinates `none`.
- [x] Recovery closure passed `5/5` with zero unresolved fixtures.
- [x] Seed-8889 positive and negative fixtures remained correct, including suppression of `14/24/30` and exclusion of edge `40`.
- [x] Canonical source-edge and internal graph-edge diagnostics remain distinct.
- [x] Freeze R13A.9a as the uniform basic-bevel/recovery baseline.

## EW-V — Post-baseline edge-wear visual roadmap

**Status:** active roadmap. Basic bevel construction is frozen; visual irregularity and finish remain open.

### Phase boundary

R13A.9a answers whether a visually meaningful source edge can receive a safe, certified bevel and whether insignificant topology may be removed without collateral damage. The next phases answer how that bevel varies and reads artistically. They must build on the accepted shell rather than replace its viability, recovery, topology, or provenance architecture.

### Planned passes

1. **EW-V1 — Macro irregularity between edges**
   - Give each selected source edge a deterministic wear identity.
   - Vary average geometric bevel width and, later if justified, a small set of edge-wide character values.
   - Preserve current coverage and artistic selection semantics unless a separate selection patch is explicitly approved.
2. **EW-V2 — Smooth micro irregularity within one edge**
   - Replace a constant-width rail strip with a bounded one-dimensional width profile along normalized edge distance.
   - Start with asymmetric endpoint taper, broad swell/narrow regions, and low-frequency drift.
   - Add only enough subdivisions to represent the profile under explicit tier and vertex budgets.
3. **EW-V3 — Localized chips and break events**
   - Add optional isolated notches, chipped intervals, or stepped local wear only after continuous V2 profiles are stable.
   - A chip is a bounded geometric event, not random high-frequency noise and not a shader mask pretending to remove geometry.
4. **EW-V4 — Artistic normal shaping**
   - Preserve the solved render-normal integrity contract while shaping how bevel bands catch light.
   - Distinguish technical normal correctness, already solved, from future artistic normal response.
5. **EW-V5 — Final material/rendering finish**
   - Tune brightness, tint, smoothness, edge-region breakup, and any approved wear detail channels after the geometry and normals are accepted.
   - Rendering may enhance valid geometry but may not conceal topology or profile defects.

### Cross-phase invariants

- `Macro Variation Coverage = 0` or `Macro Variation Strength = 0` must reproduce the accepted R13A.9a geometry and report identity exactly, apart from explicitly versioned diagnostic fields. No Micro Variation control is exposed until EW-V2 owns a real consumer.
- Variation is deterministic from stable source-edge provenance and existing recipe seeds. Do not add a new serialized seed until a real authoring need is demonstrated.
- No per-frame generation, mesh scan, or material-driven geometry rebuild is allowed.
- Micro-topology suppression, recovery closure, negative fixtures, canonical source identity, topology, convexity, bounds, volume, face quality, triangulation, normals, tangents, and render-channel certification remain mandatory.
- Quality tiers and representative vertex budgets remain authoritative.
- Each implementation patch must include the comprehensive evidence required for that whole pass; do not add essential diagnostics later in small trickles.

## EW-V1A — Deterministic Per-Edge Macro Wear Identity

**Status:** implemented and statically validated; Unity compile, one-click runtime evidence, and representative visual acceptance pending.

### Objective

Activate the existing serialized **Macro Variation** control as truthful geometry authoring. At zero it preserves the uniform baseline. Above zero it produces deterministic differences in average bevel width between selected source edges so neighbouring edges no longer look cloned.

### First-slice scope

- Reuse the existing `edgeWearMacroVariation` field; do not add another public control or seed.
- Derive one stable edge-wide multiplier from shape seed plus canonical source-edge identity. Micro-generated transition edges and micro-suppressed edges are never variation targets.
- Apply the multiplier to the edge's requested geometric width before ordinary shared-corner solving and viability/recovery evaluation, so all existing certification operates on the real varied request.
- Keep one constant width per edge in V1A. No along-edge segmentation, endpoint profile, chip, normal, or shader-finish change belongs in this patch.
- Keep coverage, artistic score weights, selection threshold, micro-topology policy, recovery policy, and baseline search budgets unchanged.
- Use the audited downward-only multiplier contract: `Macro Variation = 1` spans `0.55x–1.0x`; the Generic Test Mass default `0.32` spans approximately `0.856x–1.0x` before minimum-style clamping.
- No edge requests a broader width in V1A. Existing feasibility may still reduce a varied request; macro irregularity never weakens certification gates merely to preserve the sampled multiplier.

### Required evidence in the implementation patch

- Canonical per-edge requested multiplier and requested/resolved/materialized widths.
- Aggregate multiplier and width distribution for each evaluated mass, including minimum, median, maximum, and count clamped by feasibility.
- A zero-control semantic-parity check proving exact `1.0x` requests plus repeated selected/certified/topology/evaluation hashes and canonical identity. Historical R13A.9a hash constants are not embedded in production code.
- Representative visual cases showing clearly different average widths on several meaningful edges without losing important edges.
- The existing one-click suite expanded in the same patch so topology, artistic-preview, recovery, negative fixtures, micro suppression, normal/tangent integrity, and performance remain one-run evidence.

### Acceptance criteria

- `Macro Variation = 0` reproduces R13A.9a.
- Non-zero Macro Variation creates visible but coherent edge-to-edge width differences on representative stones.
- The result remains deterministic across regeneration and domain reload.
- No important certified edge is lost solely because of unstable variation ownership.
- Current preview passes; topology and artistic-preview matrices remain `33/33`; recovery remains `5/5`; unresolved remains `0`; negative exclusion remains `1/1`.
- No topology, face-quality, placement, render-channel, normal, tangent, collider, lifecycle, or active-gameplay regression occurs.

### Explicit non-goals

- No variation within one edge.
- No chips, notches, cracks, or missing intervals.
- No artistic normal shaping.
- No final material polish.
- No production promotion or `geometryCommit` change.

### EW-V1A pre-implementation audit and approved plan

**Audit date:** 2026-07-18. **Baseline:** accepted `EW-B4.2R13A.9a` source plus the pending Generated Mass documentation refresh.

#### Reviewed ownership and evidence

- `GeneratedMass.cs` already serializes `edgeWearMacroVariation`, exposes it publicly, applies recipe defaults, writes the unused material property, and marks explicit previews stale through ordinary `OnValidate`. The value is not part of `ProductionGenerationState`; it must remain excluded so base production geometry and Play Mode do not regenerate for editor-only bevel variation.
- `MassSurfaceFeatureGenerator.cs::MassSurfaceFeatureSettings` is the immutable settings boundary used by explicit source-index, preview, topology-matrix, and artistic-preview evaluations. It currently omits macro variation and must carry the existing control.
- `MassGenerator.EdgeWear.SelectionAndCorners.cs::BuildEdgeWearBevelCandidates` is the first point where canonical original source-edge identity, source geometry, minimum style width, and the base requested width coexist. This is the authoritative V1A sampling point.
- `EdgeWearMicroTopologyNormalizationResult::ResolveOriginalSourceEdgeIndex` supplies stable pre-normalization source identity. Generated transitions and suppressed records are already excluded from candidate construction.
- `RunEdgeWearIsolatedViabilityPreflight`, `PopulateEdgeWearArtisticContextMetrics`, `AuditExplicitChamferCornerSolution`, `TrySolveCornerAwareChamferWidths`, and the final corner audit still assume one global requested width. They must consume each edge's stored requested width without changing geometry tolerances or kernels.
- Existing artistic deterministic variation changes response strength only. It is not geometric irregularity and remains unchanged to preserve accepted selection and material-response semantics.
- `GeneratedMassEditor.cs` owns the one-click suite, matrix settings snapshots, contract labels, outlier fixtures, and report assembly. It must capture Macro Variation and add zero-parity, deterministic-repeat, and active-distribution acceptance evidence in the same patch.
- The shader declares `_GeneratedMassEdgeWearMacroVariation` but does not consume it. V1A is geometry-only and does not change shader or material behavior.

#### Deterministic width contract

For every ordinary candidate source edge, use the existing shape seed plus canonical original source-edge index and a fixed implementation salt to derive `identity01` in `[0,1]`. The requested multiplier is:

```text
sampledMultiplier = lerp(0.55, 1.00, smoothstep(0, 1, identity01))
requestedMultiplier = lerp(1.00, sampledMultiplier, clamp01(MacroVariation))
variedRequestedWidth = max(minimumStyleWidth, baseRequestedWidth * requestedMultiplier)
effectiveMultiplier = variedRequestedWidth / baseRequestedWidth
```

Properties:

- `Macro Variation = 0` produces exactly `1.0` and exactly the accepted base requested width.
- V1A is downward-only: no edge requests a width larger than the accepted R13A.9a baseline.
- `Macro Variation = 1` samples a smooth deterministic range from `0.55x` to `1.0x`.
- The current Generic Test Mass default `0.32` yields an unclamped range of approximately `0.856x` to `1.0x`; this is visible enough to test while remaining conservative.
- Minimum-style clamping may intentionally erase variation at the thinnest setting rather than create sub-style bevels.
- The identity uses no position hash, candidate ordinal, runtime randomness, new serialized seed, or seed-specific exception.

#### Approved implementation scope

Modify only the existing pending documentation-refresh files that require V1A reconciliation plus:

- `Game/Procedural/Masses/MassSurfaceFeatureGenerator.cs`
- `Game/Procedural/Masses/GeneratedMass.cs`
- `Game/Procedural/Masses/MassGenerator.cs`
- `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`
- `Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs`
- `Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
- `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`

`MassGenerator.EdgeWear.Orchestration.cs` may change only if a final call-site audit proves per-edge data cannot be consumed through the existing coverage audit. No shader, material, scene, prefab, metadata, base-mass generator, micro-topology algorithm, recovery policy, plane/hull/bounded geometry kernel, score weight, coverage rule, search budget, normal/tangent algorithm, serialized default, or production evaluation mode may change.

#### Implementation sequence

1. [x] Carry `EdgeWearMacroVariation` through immutable settings and every explicit editor matrix snapshot.
2. [x] Sample and store canonical per-edge macro identity, sampled multiplier, effective multiplier, base requested width, and varied requested width before geometric viability.
3. [x] Make footprint, locality-context reporting, isolated preflight, material-width recovery thresholds, corner initialization, corner displacement limits, clamp ratios, explicit bounded-edge preview, and final solved/materialized comparisons use the edge's own request.
4. [x] Keep selected-edge artistic score weights, random stream, threshold, and candidate-order policy unchanged apart from real width-dependent viability evidence.
5. [x] Add aggregate and per-edge diagnostic evidence, canonical deterministic signatures, and direct one-click contracts for zero parity, repeated determinism, and active distribution.
6. [x] Update Inspector/tooltips and canonical docs to describe the implemented geometry meaning without claiming V2/V3 behavior.
7. [x] Complete parser, import/reference, arity, CSV-schema, scope, line-ending, formula, invocation-count, clean-apply, and package validation. Unity compilation/runtime remains explicitly pending.

#### Plan validation

- Downward-only sampling cannot ask any edge to exceed the width already admitted by the accepted baseline.
- Existing viability, corner, coexistence, recovery, and shell certification remain authoritative; V1A only changes their per-edge input width.
- Source-edge identity remains stable through micro-topology normalization because sampling uses original canonical provenance before graph remapping.
- Production regeneration remains unaffected because the control is passed only through explicit surface-feature evaluation settings and is not added to `ProductionGenerationState`.
- No additional 33-case matrix is needed. One current-object zero-control case and two repeated current-control cases provide direct parity/determinism evidence while the existing matrices continue to prove topology, artistic selection, recovery, negative exclusion, and performance.
- The plan introduces no per-frame work, no extra geometry algorithm, no branch-search expansion, and no new active-gameplay allocation.

#### V1A acceptance additions

- Zero-control audit: every ordinary evaluated edge has identity evidence, effective multiplier `1`, varied request equal to base request within the existing float tolerance, and the same selected/certified canonical edge hashes in two repeated zero evaluations.
- Determinism audit: two repeated current-control evaluations have identical ordered canonical macro signatures, selected hash, certified hash, and evaluation hash.
- Active-distribution audit: when Macro Variation is above zero and at least two ordinary geometric edges are available, at least two distinct effective multipliers exist unless every edge is truthfully clamped to the same minimum style width; the report states which case occurred.
- Existing R13A.9a current preview, matrices `33/33`, recovery `5/5`, unresolved `0`, negative exclusion `1/1`, micro suppression, canonical identity, normal/tangent, render-channel, state-preservation, and timeout contracts remain mandatory.


### EW-V1A implementation result — static validation complete, Unity pending

#### Implemented behavior

- Sampling occurs once during source-edge discovery from `shapeSeed + canonicalOriginalSourceEdgeIndex + fixed salt`. Position hashes, candidate ordinals, runtime randomness, and new serialized seeds are not used.
- Ordinary source edges receive one constant edge-wide request. Micro-generated transitions remain at `1.0x`; micro-suppressed records never become candidates.
- `Macro Variation = 0` takes an exact branch to the base requested width. Above zero, smooth deterministic samples blend toward `0.55x–1.0x` and are clamped only by the existing minimum style width.
- The varied request is authoritative for footprint length, width ratios, isolated feasibility schedules, central-span requirements, corner initialization, corner displacement limits, clamp ratios, recovery evidence, explicit bounded-edge preview, and final shell construction.
- Artistic score weights remain `0.60 angle / 0.35 length / 0.05 random`. Micro-topology normalization, coexistence/recovery policy, geometry kernels, normal/tangent generation, search limits, production generation, shaders, materials, scenes, prefabs, and serialized defaults are unchanged.
- The one-click suite contract is `EW-V1A-suite`. It adds four direct current-object evaluations: two at zero and two at the current control, then runs the existing topology/artistic matrices and recovery/negative/comprehensive contracts.

#### Static validation completed

- All `153` project C# files parse without syntax errors.
- Constructor/helper definition and call arities match after the settings and corner-solver signature changes.
- Newly introduced references were audited for local scope and required imports; `System.Collections.Generic`, `System.Globalization`, and existing Unity namespaces cover the additions.
- Matrix CSV schema is `112/112`; comprehensive edge CSV schema is `100/100`.
- Geometry-kernel invocation-site counts are unchanged. One editor-only parity-audit call site is added. Search limits remain `128` states and `5000 ms`.
- Formula reconstruction proves exact `1.0x` at control zero, `0.55x–1.0x` at control one, and approximately `0.856x–1.0x` at control `0.32`. Seed `8889` produces distinct stable samples for canonical edges `13`, `23`, and `39`.
- Changed C# and canonical Markdown files retain their original CRLF convention, have balanced preprocessors, no trailing whitespace/conflict markers, and cleanly apply to the accepted source plus documentation-refresh baseline.
- Unity 6000.5.0f1 is unavailable in the implementation environment. Do not accept or freeze V1A until Unity compilation and the complete one-click suite pass.

#### Required Unity/visual decision

1. Compile with zero C# errors.
2. Run **Full Edge-Wear Validation Suite (1 Click)** and require macro contract passed, both matrices `33/33`, recovery `5/5`, unresolved `0`, negative exclusion `1/1`, comprehensive evidence, no timeout/cancellation, and no terminal geometry/render regression.
3. Compare the same representative stones at Macro Variation `0`, the current default, and `1`. Confirm zero parity and clearly visible but coherent differences between meaningful neighbouring edges.
4. Confirm seed `8889` canonical edges `13/23/39` remain certified, edge `40` remains excluded, and `14/24/30` remain micro-suppressed.
5. If downward-only variation is visually sufficient, freeze V1A. If it is too conservative, plan a separate V1B bounded-widening probe rather than weakening V1A certification.


## EW-V1A.1 — Edge-Wear Control Cleanup and Macro Authoring Split

### Status

- [x] Read-only review complete.
- [x] Concrete plan recorded before implementation edits.
- [x] Inspector/control implementation complete.
- [x] Macro participation implementation complete.
- [x] Diagnostics and one-click contracts updated.
- [x] Canonical documentation reconciled.
- [x] Static/package validation complete.
- [ ] Unity compilation and runtime/visual acceptance complete.

### Objective

Remove exposed edge-wear controls that have no current consumer and replace the single ambiguous Macro Variation control with two truthful artistic controls:

- **Macro Variation Coverage** — deterministic fraction of ordinary eligible canonical source edges that participate in macro width variation;
- **Macro Variation Strength** — magnitude of downward width variation on participating edges.

The existing ordinary bevel Coverage remains selection coverage and is relabelled **Bevel Coverage** in the Inspector to distinguish it from macro participation.

### Reviewed evidence

- Repository reconstruction is clean at commit `8e08724` after the accepted R13A.9a patch, documentation refresh, and EW-V1A patch; `git status --short` is empty.
- `GeneratedMass.cs` owns serialized edge-wear authoring fields, recipe/default transfer, source-debug freshness, public getters, and material-property forwarding.
- `GeneratedMassEditor.cs` owns the explicit Edge Wear Inspector, one-click suite, matrix snapshots/restoration, macro contract, CSV/report output, and the raw-property exclusion list.
- `MassSurfaceFeatureGenerator.cs` owns immutable explicit-evaluation settings.
- `MassGenerator.EdgeWear.SelectionAndCorners.cs` owns canonical-source-edge macro sampling and per-edge requested-width authority.
- `MassGenerator.cs`, `MassGenerator.EdgeWear.Types.cs`, and `MassGenerator.EdgeWear.Diagnostics.Logging.cs` own batch/canonical evidence and acceptance reporting.
- `MassGenerator.EdgeWear.Orchestration.cs` consumes the immutable settings but does not require a new geometry algorithm.
- `PixelSurfaceGeneratedMassFeatures.hlsl` currently consumes Response Strength, Brightness Lift, Worn Edge Tint, Tint Influence, and Softness. `EdgeWearAmount`, Width, and Bevel Coverage are consumed by current geometry/selection paths. These controls remain exposed.
- The exposed **Micro Variation** control has no geometry or shader consumer. Its serialized value and hidden shader declaration may remain for future migration, but the control must be removed from the current Inspector until EW-V2 begins.
- The existing V1A Macro Variation implementation uses one deterministic scalar for all edges and therefore cannot control participation independently from strength.

### Approved files

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Game/Procedural/Masses/GeneratedMass.cs`
6. `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`
7. `Game/Procedural/Masses/MassSurfaceFeatureGenerator.cs`
8. `Game/Procedural/Masses/MassGenerator.cs`
9. `Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs`
10. `Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
11. `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`

No scene, prefab, material, shader, metadata, base-mass generation, geometry kernel, normal/tangent, search-budget, or runtime production file is authorized.

### Invariants and non-goals

- `Macro Variation Strength = 0` or `Macro Variation Coverage = 0` must reproduce the uniform R13A.9a width request exactly.
- `Coverage = 1` with the same Strength must preserve the existing V1A all-edge variation behavior and deterministic multiplier stream.
- Participation uses a second deterministic hash stream from `shape seed + canonical original source-edge ID + fixed participation salt`; increasing Coverage produces a stable nested participant set.
- Participating edges retain the existing downward-only `0.55x–1.0x` full-strength contract. Non-participating edges remain exactly `1.0x`.
- Micro-generated transitions do not participate. Micro-suppressed records remain non-candidates.
- Existing viability, corner, coexistence, recovery, shell certification, artistic selection weights, micro-topology normalization, and performance budgets remain authoritative.
- No within-edge variation, chips, widening, normal shaping, material finish, or production Play Mode behavior is added.
- The hidden legacy Micro Variation value is not sampled, forwarded into geometry, or shown in the Inspector.

### File-by-file implementation sequence

1. Add serialized/public/default/state support for `edgeWearMacroVariationCoverage` while retaining the existing `edgeWearMacroVariation` serialized field as Strength for migration stability.
2. Forward Coverage and Strength through `MassSurfaceFeatureSettings`, batch settings, source-debug freshness, matrix snapshots, and explicit evaluation calls.
3. Extend the canonical macro resolver with an independent participation identity and participation result; preserve the existing width identity/sample stream exactly.
4. Extend lifecycle, coverage, batch, per-edge, summary, CSV, and signature evidence with Coverage, Strength, participation identity, and participant state.
5. Replace the Inspector controls with **Bevel Coverage**, **Macro Variation Coverage**, and **Macro Variation Strength**; move macro controls into Geometry Edge-Wear Inputs; remove Micro Variation from the visible Edge Wear section and its editor-only property dependency.
6. Advance the one-click contract to `EW-V1A.1`, add exact zero-by-strength, zero-by-coverage, repeated determinism, full-coverage compatibility, and current-distribution checks, and preserve all R13A.9a/V1A safety gates.
7. Reconcile all four canonical documents with the actual control ownership and Unity-pending status.
8. Run full parser, namespace/import, definition/call arity, CSV schema, deterministic formula, exact scope, line-ending, preprocessor, invocation-count, clean-apply, and package validation.

### Risks and controls

- **Risk:** Coverage and Bevel Coverage become artistically ambiguous. **Control:** explicit Inspector labels and tooltips distinguish selection from macro participation.
- **Risk:** adding participation changes existing V1A results. **Control:** Coverage defaults to `1`, the multiplier identity hash/salt remains unchanged, and full Coverage must reproduce V1A signatures.
- **Risk:** coverage selection changes when topology is normalized. **Control:** sample only canonical original source-edge identity.
- **Risk:** zero controls still clamp to minimum style width. **Control:** exact early branch to base requested width whenever either control is zero.
- **Risk:** stale Micro Variation remains visible through generic Inspector fallback. **Control:** retain its raw-property exclusion while removing its explicit drawing and property dependency.
- **Risk:** diagnostics report graph IDs as source IDs. **Control:** all participant evidence uses canonical source-edge mapping already established by R13A.9.

### Acceptance criteria

- The Edge Wear Inspector shows no Micro Variation control.
- Every shown Edge Wear control has a current geometry or shader consumer.
- Bevel Coverage, Macro Variation Coverage, and Macro Variation Strength have distinct truthful responsibilities.
- Coverage `0` and Strength `0` each produce exact uniform-request parity.
- Coverage `1` preserves the previous V1A deterministic all-edge distribution for the same Strength.
- Intermediate Coverage produces a deterministic subset, never more participants than ordinary evaluated edges, and stable nested membership as Coverage increases.
- Both matrices remain `33/33`; recovery remains `5/5`; unresolved remains `0`; negative exclusion remains `1/1`; no timeout/cancellation or geometry/render regression occurs.
- Representative visual testing confirms Coverage controls how many edges vary and Strength controls how strongly those participating edges narrow.

### EW-V1A.1 implementation result — controls validated; active visual baseline rejected

- `GeneratedMass.cs` adds serialized Macro Variation Coverage with default `1`, retains the existing serialized Macro Variation field as Strength, includes both controls in explicit-preview freshness/settings/recipe transfer, and sets dormant Micro Variation recipe defaults to zero without deleting the serialized backing field.
- `GeneratedMassEditor.cs` relabels ordinary Coverage as **Bevel Coverage**, moves both Macro controls into Geometry Edge-Wear Inputs, removes visible Micro Variation drawing and its editor property dependency, and preserves the hidden raw-property exclusion so generic Inspector fallback cannot expose it.
- `ResolveEdgeWearMacroRequestedWidth` uses independent salted deterministic streams for participation and width. Coverage thresholds canonical participation; Coverage `1` explicitly includes every ordinary evaluated edge; Strength blends the existing V1A width sample; zero on either control returns the base request exactly.
- Coverage/lifecycle/batch telemetry, deterministic signatures, summary reports, and matrix CSV expose Coverage, Strength, participant counts, participation identity, and participant state. Trial clones preserve the new coverage-level evidence.
- One-click contracts advance to `EW-V1A.1` and directly test zero-by-Strength parity, zero-by-Coverage parity, repeated current-control determinism, full-Coverage compatibility, participant bounds, and current distribution before the existing topology, artistic, recovery, negative, comprehensive, state, timeout, and render gates.
- Geometry kernels, micro-topology normalization, recovery rules, artistic score weights, normals/tangents, shaders, materials, scenes, prefabs, search budgets, base-mass generation, and production `EdgeWearEvaluationMode.None` remain unchanged.
- Unity later validated the V1A.1 control split and one-click contracts, but active testing at the current Width exposed the incomplete owner/support construction-width schedule now corrected by V1A.2. V1A.1 therefore remains historical rather than an accepted irregularity baseline.


## EW-V1A.2 — Complete Construction-Width Backoff and Active-Macro Retention

**Status:** Implementation and static/package validation complete; Unity compilation and runtime acceptance pending.

### Objective

Preserve meaningful bevels when a width-specific isolated rail succeeds but bounded owner/support construction fails. Continue the existing finite `0.75` width schedule through full bounded construction/certification instead of stopping at the first rail-success width. Add an active-macro retention contract so an edge certified by the zero-macro comparison may narrow but may not silently disappear without a complete bounded infeasibility proof.

### Reviewed evidence

- The active Unity report `Pasted text(100).txt` uses seed `8889`, Macro Variation Coverage `1`, Strength `0.55`, and passes the existing V1A.1 suite while source edge `10` is `GeometricIneligible` with `owner-face-support-insufficient`. Its macro multiplier is `0.999998331`, so the edge is effectively at the full base width rather than being removed by deliberate macro narrowing.
- The preceding zero-strength report at the same current Width also excludes source edge `10` with the same incomplete owner/support result. Therefore zero-macro comparison alone cannot detect this regression; the acceptance contract must additionally reject every unresolved `owner-face-support-insufficient` result directly.
- The same report records one rail-success/construction-failure attempt at `0.012897186`, `scheduleResolution:unresolved`, and no lower construction attempt.
- Accepted earlier runtime evidence for the same seed/source edge certified it after width backoff at approximately `0.00722873956`, proving the current disappearance is not a stable feature-level exclusion.
- `MassGenerator.EdgeWear.BoundedSingleEdge.cs::TrySolveBoundedIsolatedSingleEdgeRails` currently performs at most twelve `0.75` rail attempts but returns immediately at the first rail success. `AuditBoundedSingleEdgeBevel` then performs bounded construction/certification only once at that width.
- `MassGenerator.EdgeWear.SelectionAndCorners.cs::ResolveEdgeWearIsolatedViabilityFailure` classifies incomplete owner/support construction as `owner-face-support-insufficient`; it already consumes `BoundedSingleEdgeAuditResult` and needs no policy relaxation if the audit returns the largest certified lower width.
- `GeneratedMassEditor.cs::EvaluateMacroVariationContract` already evaluates zero-macro and active-macro cases and exposes canonical per-edge audit records, so retention can be checked without new runtime telemetry or fixture-specific production logic.
- The reconstructed source has no Git metadata. The exact baseline is `Assets(69).zip` plus the delivered R13A.7, R13A.8, R13A.9, R13A.9a, documentation-refresh, V1A, and V1A.1 patches in order.

### Approved files

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Game/Procedural/Masses/MassGenerator.EdgeWear.BoundedSingleEdge.cs`
6. `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

No scene, prefab, material, shader, metadata, serialized authoring field, macro sampling formula, artistic selection score, micro-topology implementation, all-edge plane/hull kernel, corner/coexistence solver, search budget, normal/tangent behavior, base-mass generation, or production `EdgeWearEvaluationMode.None` path is authorized.

### Invariants and non-goals

- Keep the existing maximum of twelve discrete width attempts and the existing `0.75` backoff factor.
- Count rail failures and rail-success/construction failures against the same twelve-attempt budget.
- Retry only after a rail-success result fails the existing owner/support construction contract. Do not convert topology, containment, face-quality, bounds, volume, artistic, footprint, shallow-angle, micro-suppression, or ordinary coexistence exclusions into generic width recovery.
- Each retry uses a strictly smaller requested width derived from the last solved width, so a local starting-width cap cannot repeat the same failed construction width.
- The first fully certified result wins and remains subject to all existing shell, provenance, topology, render-channel, and materialization checks.
- If the minimum width or attempt budget is exhausted, retain truthful `complete-infeasible` evidence; do not fabricate certification.
- Macro Coverage/Strength semantics, deterministic identities, and zero-control parity remain unchanged.
- Active-macro retention is an editor validation contract only; no seed/source-edge fixture enters production behavior.

### File-by-file implementation sequence

1. [x] `BoundedSingleEdge.cs`: split the current one-schedule audit into a private single-schedule core plus a bounded wrapper; aggregate and renumber all rail/construction attempts; retry only owner/support failures at `lastSolvedWidth * 0.75`; preserve the twelve-attempt total cap and exact final resolution.
2. [x] `GeneratedMassEditor.cs`: advance contracts to `EW-V1A.2`; compare zero-macro certified meaningful source edges with the active-macro case and independently reject every current owner/support exclusion whose bounded schedule remains unresolved; report retained, proven-infeasible, and unproven-loss source-edge sets.
3. [x] Canonical docs: reconcile the construction-width schedule, active-retention acceptance boundary, performance implications, and Unity-pending status.
4. [x] Validate all project C# with an available parser; scan introduced method calls/overloads/imports; verify exact scope, CRLF, preprocessor balance, no fixture-specific production literals, unchanged geometry-kernel invocation counts, clean patch application, and byte-identical package reproduction.

### Risks and controls

- **Risk:** recursive retries exceed the accepted attempt budget. **Control:** pass the remaining attempt count into the single-schedule rail solver and aggregate against one hard total of twelve.
- **Risk:** a retry repeats the same locally capped width. **Control:** derive the next requested width from the last solved width, not the previous external request.
- **Risk:** failed-attempt evidence is lost when a later retry succeeds. **Control:** merge every ordered attempt into the final result and renumber it before final formatting.
- **Risk:** broad retrying hides real topology defects. **Control:** retry only the existing owner/support construction failure class; every other failure returns immediately.
- **Risk:** retention contract treats intentionally insignificant exclusions as regressions. **Control:** baseline membership requires a zero-macro edge that was artistically eligible and actually certified/materialized.

### Acceptance criteria

- Seed `8889`, source edge `10`, Coverage `1`, Strength `0.55` is geometric, selected, active, and certified at a reduced width, or receives a genuinely complete bounded infeasibility proof after the full schedule.
- The active-macro retention contract has zero unproven losses relative to the zero-macro certified meaningful edge set and zero unresolved current `owner-face-support-insufficient` exclusions.
- Width-attempt evidence shows every failed construction width and the final certified or minimum/budget terminal width in strict descending order.
- Existing zero-strength/zero-coverage parity, deterministic distribution, both matrices `33/33`, recovery `5/5`, unresolved `0`, negative exclusion `1/1`, micro-suppression `14/24/30`, and intentional edge-40 exclusion remain intact.
- No terminal topology, face-quality, containment, placement, render-channel, normal, tangent, timeout, cancellation, state-preservation, or meaningful performance regression occurs.


### EW-V1A.2 implementation result — active edge recovered; minimum-tier matrix closure required

- `AuditBoundedSingleEdgeBevel` now owns one finite twelve-attempt schedule across both rail failures and rail-success/construction failures. The existing `0.75` factor, minimum width, and all geometry/certification gates remain unchanged.
- The previous single-pass body is retained as `AuditBoundedSingleEdgeBevelSingleSchedule`; the outer wrapper aggregates and renumbers every attempt, retries only incomplete owner/support construction, derives each next request from the last solved width, and commits the first fully certified result.
- Incomplete topology, face-quality, containment, bounds/volume, artistic, footprint, shallow-angle, micro-suppression, and coexistence results are not converted into generic width recovery.
- The V1A.2 editor contract compares zero-macro certified meaningful source edges with the active case and independently rejects every unresolved `owner-face-support-insufficient` exclusion. Reports now include `macroRetention` plus baseline, certified, proven-infeasible, and unproven-loss evidence.
- Suite, topology, artistic-preview, and comprehensive contract labels advance to `EW-V1A.2`.
- Static validation passed all 153 project C# files with zero parser errors; introduced helper/overload arities and imports passed; exact scope is six approved files; CRLF, terminal newlines, preprocessor balance, and fixture isolation are preserved; geometry-kernel invocation counts are unchanged.
- The final patch clean-applies to the exact reconstructed V1A.1 baseline, and clean-applied/package files are byte-identical.
- Unity 6000.5.0f1 validated the intended active recovery: Coverage `1`, Strength `0.55`, source edge `10` is selected and certified at `0.00725466711`; macro retention passed; recovery remained `5/5`; unresolved remained `0`; negative exclusion remained `1/1`. Both matrices reported `32/33` because seed `8889` at minimum Width reached corner solving with one floor-bound selected edge and no certified output. V1A.2a owns that remaining classification mismatch.


## EW-V1A.2a — stable-width classification closure

**Status:** implementation and static/package validation complete; Unity validation pending.

### Read-only review evidence

- [x] Reviewed `AGENTS.md`, current Git status, `HEAD` (`5af94c0`, V1A.1 baseline), the complete V1A.2 working diff, and the active Unity report `Pasted text(101).txt`.
- [x] Reviewed the complete current implementations and direct data path in `MassGenerator.EdgeWear.BoundedSingleEdge.cs::AuditBoundedSingleEdgeBevel`, `MassGenerator.EdgeWear.SelectionAndCorners.cs::RunEdgeWearIsolatedViabilityPreflight`, `ResolveEdgeWearIsolatedViabilityFailure`, `AuditExplicitChamferCornerSolution`, and `TrySolveCornerAwareChamferWidths`.
- [x] Reviewed batch capture and failure propagation in `MassGenerator.EdgeWear.Diagnostics.Logging.cs::CompleteEdgeWearBatchAuditCapture` and `PopulateEdgeWearBatchAuditResult`, plus contract/report assembly in `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`.
- [x] Confirmed V1A.2 fixes the reported active edge: seed `8889`, source edge `10` is selected and certified at `0.00725466711` after the finite `0.012897186 -> 0.00967288949 -> 0.00725466711` schedule.
- [x] Confirmed the only matrix failure is seed `8889`, minimum Width tier, in both topology and artistic-preview matrices (`32/33`). The case reaches corner solving with no certified output and reports `primaryFailure=none`.
- [x] Corrected the earlier terminology: `minimumStyleWidth` is not the terminal geometry floor. The accepted width-monotonic policy explicitly permits certified widths down to `minimumStyleWidth * 0.25`. The full corner solver's absolute floor is `minimumStableEdgeLength` (`maximumDimension * 0.0012`).
- [x] The failing minimum-tier participant reaches the isolated schedule's exact minimum stable width. It therefore has no legal downward adjustment when the shared corner solve requires a reduction. This is a pre-corner classification mismatch, not a reason to lower the floor or revert V1A.2.
- [x] `CompleteEdgeWearBatchAuditCapture` replaces an empty primary failure with `CornerBlocker`, but does not replace the literal sentinel `none`; `PopulateEdgeWearBatchAuditResult` may set that sentinel before completion. This directly explains the blank matrix diagnosis.

### Objective and acceptance criteria

- [x] Classify an isolated result whose maximum certified width is at the absolute minimum stable geometry width as geometrically ineligible before artistic selection and corner construction.
- [x] Preserve the complete isolated attempt evidence, certified width, requested-width fraction, and minimum stable floor evidence.
- [x] Use a specific terminal reason: `maximum-certified-width-at-stable-width-floor`.
- [x] Keep the existing `minimumStyleWidth * 0.25` width-monotonic recovery policy unchanged; do not reject every certified width below `minimumStyleWidth`.
- [x] Treat the new reason as a width-floor exclusion in compact diagnostics and Scene-view edge state.
- [x] Replace an empty **or literal `none`** batch primary failure with the captured corner blocker.
- [x] Advance one-click and matrix contract labels to `EW-V1A.2a`.
- [ ] Unity acceptance: current preview passes; macro zero parity, determinism, distribution, and retention pass; topology `33/33`; artistic preview `33/33`; recovery `5/5`; unresolved `0`; negative exclusion `1/1`.

### Approved implementation scope

- [x] `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- [x] `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
- [x] `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
- [x] `Docs/Generated_Mass_Framework.md`
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs`
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.Orchestration.cs`
- [x] `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

### Invariants and non-goals

- [x] Do not change the V1A macro coverage/strength formula or serialized controls.
- [x] Do not change the twelve-attempt `0.75` width schedule, geometry kernels, tolerances, minimum stable width, minimum style width, recovery search, artistic score, micro-topology normalization, normals, tangents, shaders, materials, scenes, prefabs, metadata, or production generation path.
- [x] Do not add seed-specific or edge-ID-specific production behavior.
- [x] Preserve every unrelated working-tree change from the documentation refresh, V1A, V1A.1, and V1A.2.

### Implementation sequence

- [x] Add the stable-floor viability gate after isolated schedule evidence is captured and before width-recovery eligibility/final viability classification.
- [x] Map the new terminal reason into existing width-floor diagnostics and the direct Scene-view debug-state consumer in `MassGenerator.EdgeWear.Orchestration.cs`.
- [x] Correct batch primary-failure sentinel handling.
- [x] Advance contract labels and reconcile the four canonical documents.
- [x] Run parser, semantic reference/import, scope, line-ending, diff, clean-apply, and package verification.
- [x] Run Unity one-click validation and record runtime results before freezing V1A.

### EW-V1A.2a implementation result — production/matrix closure validated; fixture resolver incomplete

- `RunEdgeWearIsolatedViabilityPreflight` preserves the complete V1A.2 schedule, then excludes only a result reduced to `minimumStableEdgeLength` while the original per-edge request remains above that floor. Direct requests at the floor are not automatically rejected.
- The new terminal reason is `maximum-certified-width-at-stable-width-floor`; compact exclusion reporting and Scene-view state classify it as a width-floor result.
- The accepted `minimumStyleWidth * 0.25` width-monotonic recovery rule is unchanged. Edge `10` at the active settings remains outside the new gate because its certified `0.00725466711` width is above the stable floor.
- Batch completion now replaces both an empty primary failure and the literal sentinel `none` with the exact captured corner blocker.
- Contracts advance to `EW-V1A.2a-suite`, `EW-V1A.2a-topology`, `EW-V1A.2a-preview`, and `EW-V1A.2a-comprehensive`.
- All 153 project C# files parse successfully; introduced call/signature/import checks pass; exact V1A.2a scope is eight files; CRLF, terminal newlines, preprocessor balance, and whitespace rules are preserved; geometry/search invocation counts are unchanged.
- The patch clean-applies to the exact V1A.2 baseline; clean-applied files and the changed-files package are byte-identical.
- Unity validation confirmed current preview, macro parity/determinism/distribution/retention, both matrices `33/33`, and negative exclusion `1/1`. Outlier closure remained `3/5` because the editor fixture resolver did not recognize the new exact stable-floor terminal proof. This was the V1A.2a intermediate state; V1A.2b subsequently closed the resolver contract and became the accepted macro baseline.

## EW-V1A.2b — Stable-floor fixture resolution

### Pre-edit review evidence

- [x] Runtime evidence reviewed: `EW-V1A.2a-suite` reports current preview passed, macro parity/determinism/distribution/retention passed, topology and artistic-preview matrices passed `33/33`, negative exclusion passed `1/1`, and only the two seed-2223 edge-13 outlier fixtures remained unresolved after terminating with `maximum-certified-width-at-stable-width-floor`.
- [x] `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs` reviewed at `EvaluateOutlierRecoveryContract` and `EvaluateOutlierRecoveryExpectation`. The resolver currently recognizes certified recovery, `corner-recovery-proven-infeasible`, `width-recovery-proven-infeasible`, and complete isolated-schedule exhaustion, but not the new stable-floor terminal reason.
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs` reviewed at the V1A.2a viability gate. The reason is emitted only when isolated construction succeeded, the maximum certified width reached `minimumStableEdgeLength` within `PointMergeDistance`, and the original requested width remained strictly above that floor.
- [x] `Game/Procedural/Masses/MassGenerator.cs` and `MassGenerator.EdgeWear.Types.cs` reviewed for audit-record propagation. The public artistic audit record carries the exact viability/final reason and isolated success/width evidence, but not the internal stable-floor scalar; the exact terminal reason is therefore the authoritative proof boundary for this editor fixture resolver.
- [x] Canonical Generated Mass inventory, recovery architecture, framework, and this checklist reviewed. V1A.2a production behavior is retained; only fixture classification and contract labels require change.
- [x] Git state reviewed. The working tree contains exactly the eight uncommitted V1A.2a files over commit `9210ba0` (`V1A.2 baseline`). These changes are preserved and form the implementation baseline.

### Objective and acceptance criteria

- [x] Classify an exact `maximum-certified-width-at-stable-width-floor` terminal result as proven infeasible for non-certification-required historical fixtures.
- [x] Require matching `ViabilityFailureReason` and `FinalReason`, geometric/candidate/selection exclusion, isolated construction success, positive requested width, positive maximum certified width, and zero active/certified/materialized output before accepting the stable-floor proof.
- [x] Report the distinct resolution label `stable-width-floor-proven-infeasible`; do not collapse it into generic schedule exhaustion in fixture evidence.
- [x] Preserve seed-8889 edges `13/23` as certification-required fixtures and preserve the edge-40 negative gate unchanged.
- [x] Advance suite, topology, artistic-preview, and comprehensive labels to `EW-V1A.2b`.
- [x] Unity acceptance: current preview passed; macro parity, determinism, distribution, and retention passed; topology `33/33`; artistic preview `33/33`; outlier closure `5/5`; unresolved `0`; negative exclusion `1/1`; no cancellation or timeout.

### Approved implementation scope

- [x] `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- [x] `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
- [x] `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
- [x] `Docs/Generated_Mass_Framework.md`
- [x] `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

### Invariants and non-goals

- [x] No production geometry, viability, width schedule, corner/coexistence, recovery, micro-topology, selection-score, macro-control, shader, material, scene, prefab, serialized-data, normal/tangent, or runtime-generation change.
- [x] No new seed-specific production policy. Fixture literals remain editor-only and unchanged.
- [x] The resolver may accept only the exact V1A.2a terminal reason with corroborating inactive/unmaterialized audit state.
- [x] R13A.9a remains the exact uniform zero-control fallback; V1A.2b becomes the accepted macro-irregularity baseline only after complete Unity validation and artistic acceptance.

### File-by-file implementation sequence

- [x] Record this persistent plan before implementation edits.
- [x] Add one editor-only stable-floor proof predicate and distinct resolution output.
- [x] Advance editor contract labels to V1A.2b.
- [x] Reconcile the four canonical documents with the V1A.2a runtime result and V1A.2b pending status.
- [x] Run parser, semantic reference/import, scope, line-ending, diff, clean-apply, and package verification.
- [x] Run Unity one-click validation and record runtime results before freezing V1A.

### Risks and validation controls

- [x] Risk: accepting an arbitrary textual reason without terminal evidence. Control: require exact matching viability/final reasons, isolated success, positive widths, and inactive/uncertified/unmaterialized output.
- [x] Risk: weakening certification-required visual fixtures. Control: stable-floor proof remains insufficient when `requireCertifiedRecovery` is true.
- [x] Risk: obscuring the actual resolution class. Control: emit `stable-width-floor-proven-infeasible` explicitly in the fixture report.
- [x] Risk: scope drift into production geometry. Control: exact five-file scope and post-change diff audit.

### EW-V1A.2b implementation result — Unity validated and accepted

- `IsStableWidthFloorProvenInfeasible` accepts only an exact V1A.2a terminal record: geometric/candidate/selection exclusion; isolated construction success; positive requested and maximum-certified widths; no active, certified, or materialized output; and identical `ViabilityFailureReason`/`FinalReason` values of `maximum-certified-width-at-stable-width-floor`.
- `EvaluateOutlierRecoveryExpectation` counts that proof for non-certification-required fixtures and emits the distinct resolution `stable-width-floor-proven-infeasible`. Certification-required seed-8889 edges `13/23` remain unchanged.
- Suite, topology, artistic-preview, and comprehensive labels advance to `EW-V1A.2b`. No production Generated Mass source file changes.
- All 153 project C# files parse successfully; helper definition/call and contract-label checks pass; exact V1A.2b scope is five files; CRLF, terminal newlines, whitespace, conflict-marker, and preprocessor checks pass; no production source file differs from the V1A.2a baseline.
- The final patch clean-applies to the exact V1A.2a baseline; all five clean-applied files and the changed-files ZIP are byte-identical. Unity then passed the complete V1A.2b suite: both matrices `33/33`, outlier closure `5/5`, unresolved `0`, negative exclusion `1/1`, all macro contracts passed, and no cancellation or terminal failure.


## EW-V1A freeze — accepted macro-irregularity baseline

### Pre-edit review evidence

- [x] Unity runtime evidence reviewed from the accepted `EW-V1A.2b-suite`: overall status passed; current preview passed; macro zero parity, determinism, distribution, and retention passed; topology and artistic-preview matrices passed `33/33`; outlier resolution passed `5/5` with two certified recoveries, three proven-infeasible outcomes, and zero unresolved; negative exclusion passed `1/1`; cancellation was zero and terminal reason was `none`.
- [x] Active seed-8889 evidence reviewed: Coverage `1`, Strength `0.55`, all 39 ordinary evaluated edges participated and varied, requested-width multipliers ranged from `0.756234646` to `0.999998331`, and the current preview certified `31/31` selected edges.
- [x] Edge-retention evidence reviewed: source edge `10` remained `ViableSelected` and certified at `0.00725466711` after the complete three-attempt bounded schedule.
- [x] `GeneratedMass.cs`, `MassSurfaceFeatureGenerator.cs`, `MassGenerator.EdgeWear.SelectionAndCorners.cs`, diagnostics/orchestration, and `Editor/GeneratedMassEditor.cs` reviewed at the V1A control, deterministic sampling, stable-floor classification, reporting, and Inspector ownership points.
- [x] The four canonical Generated Mass documents reviewed against the accepted V1A.2b implementation and runtime report. Their pending-language and next-owner sections require reconciliation; no code or serialized asset change is required.
- [x] Git state reviewed. Commit `260b740` is the exact V1A.2b accepted source/documentation baseline; the freeze patch begins from a clean working tree.

### Objective and acceptance criteria

- [x] Freeze `EW-V1A.2b` as the accepted deterministic per-edge macro-width irregularity baseline.
- [x] Preserve `EW-B4.2R13A.9a` as the exact zero-irregularity fallback when Macro Variation Coverage or Strength is zero.
- [x] Record the accepted artistic control boundary: Bevel Coverage selects bevel candidates; Macro Variation Coverage selects a deterministic participating subset; Macro Variation Strength controls downward narrowing; Width remains constant along each edge.
- [x] Record that V1A does not implement within-edge taper/lobes/drift, chips/notches, artistic normal shaping, or final material finish.
- [x] Move active ownership to `EW-V2` planning without inventing an implementation patch in this freeze.
- [x] Keep ordinary production at `EdgeWearEvaluationMode.None` with `geometryCommit=disabled`.

### Approved freeze scope

- [x] `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- [x] `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
- [x] `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
- [x] `Docs/Generated_Mass_Framework.md`

### Invariants and non-goals

- [x] Documentation-only freeze; no C#, shader, material, scene, prefab, metadata, serialized authoring, geometry, recovery, selection, normal/tangent, or production-state change.
- [x] Do not change V1A.2b contract labels or reinterpret its runtime evidence.
- [x] Do not expose Micro Variation before EW-V2 owns a real consumer.
- [x] Do not define V2 geometry, subdivisions, defaults, or controls in this patch; only identify the next feature boundary.

### File-by-file implementation sequence

- [x] Record this persistent freeze plan before editing the other canonical documents.
- [x] Update the code inventory current baseline/ownership and append the V1A freeze boundary.
- [x] Update the recovery architecture accepted status and preserve its frozen recovery invariants.
- [x] Update the framework current construction/visual roadmap boundary.
- [x] Update this checklist active feature, runtime acceptance, freeze decision, and next work item.
- [x] Run complete final-file consistency review, Markdown/scope/line-ending/whitespace checks, clean-apply validation, and changed-file package verification.

### Risks and validation controls

- [x] Risk: confusing the accepted macro baseline with production promotion. Control: repeat `EdgeWearEvaluationMode.None` and `geometryCommit=disabled`.
- [x] Risk: losing exact zero-variation parity ownership. Control: retain R13A.9a as the zero-control fallback and quote the V1A.2b parity contract.
- [x] Risk: claiming V2/V3 behavior exists. Control: state constant width along each edge and enumerate excluded future responsibilities.
- [x] Risk: documentation drift across canonical files. Control: cross-check accepted identifier, runtime counters, controls, invariants, and next owner in all four documents.

### Freeze result — accepted and documented

- `EW-V1A.2b` is frozen as the accepted deterministic per-edge macro-width irregularity baseline; `EW-B4.2R13A.9a` remains the exact zero-control uniform fallback.
- The accepted runtime record is preserved exactly: current preview passed with `31/31` selected/active/certified; macro parity/determinism/distribution/retention passed; topology and artistic-preview matrices passed `33/33`; outlier closure passed `5/5` with two certified recoveries, three proven-infeasible outcomes, and zero unresolved; negative exclusion passed `1/1`; cancellation was zero and terminal reason was `none`.
- V1A owns deterministic edge participation and constant per-edge average width only. V2/V3/V4/V5 responsibilities remain explicitly unimplemented.
- The freeze changes only the four canonical Markdown documents. All 153 project C# files remain byte-identical to the V1A.2b accepted baseline and parse successfully.
- Exact four-file scope, CRLF, terminal newlines, fenced-block balance, trailing-whitespace/conflict-marker checks, `git diff --check`, clean patch application, byte identity, ZIP contents, and CRC verification passed.
- No additional Unity run is required for this documentation-only freeze. The next implementation work is EW-V2 planning in a new chat.

## EW-S1 — retire geometric Micro and add shader bevel-surface response

**Status:** Implementation and static/package validation complete; Unity compilation and visual acceptance pending.

### Objective and accepted decision

- [x] Remove EW-V2A geometric within-edge width profiling, its Inspector controls, multi-plane profile construction, effective-depth isolation/backoff, selective admission, additional-plane work, and Micro-specific validation.
- [x] Restore `EW-V1A.2b` scalar bevel geometry plus Macro edge-to-edge width variation as the authoritative edge-wear geometry baseline.
- [x] Add shader-only broad bevel-face response for normal, value, worn-edge tint response, and smoothness breakup without adding vertices, triangles, planes, mesh channels, textures, atlases, or geometry certification search.
- [x] Keep Edge Surface Variation Strength `0` on the accepted V1A.2b normal/albedo/smoothness output path.
- [x] Record sparse corner damage as the next geometry owner, followed by sparse edge chips/notches and later face/crack finish.

### Reviewed evidence and constraints

- [x] Reviewed `AGENTS.md`, the complete EW-V2A.4 source overlay, the accepted `Assets(74).zip` V1A.2b baseline, the complete settings/Inspector/generator/shader path, and all four canonical Generated Mass documents. The supplied source contains no Git metadata; no remote source was substituted.
- [x] Reviewed the user-supplied EW-V2A.4 runtime report and screenshots. At Coverage/Strength/Broadness `1/1/1`, the implementation retained `12/21` profiles only after `32` admission attempts and emitted `58` bevel planes, including `27` additional planes. The visible result repeated one wide-end/narrow-middle profile family. This is the performance and artistic rejection basis.
- [x] `MassGenerator.MeshOutput.cs` already writes the actual generated bevel-face mask to UV2.z and vertex-colour alpha and emits authored bevel normals. `PixelSurfaceForwardTypes.hlsl` already carries object-space position, world normal, material masks, generated-mass bounds, and Surface Seed to the fragment stage. No new mesh data is required.
- [x] Shared-shader impact audit completed. `SH_PixelSurfaceLit.shader` is also used by Ground, but EW-S1 is multiplied by `_GeneratedMassGeometryEdgeWearEnabled` and UV2.z. The shader default for the Generated Mass enable flag is zero, so Ground remains on the unchanged path.

### Approved and final file scope

- [x] `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- [x] `Docs/Generated_Mass_Framework.md`
- [x] `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
- [x] `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
- [x] `Game/Procedural/Masses/GeneratedMass.cs`
- [x] `Game/Procedural/Masses/MassSurfaceFeatureGenerator.cs`
- [x] `Game/Procedural/Masses/MassGenerator.cs`
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.Orchestration.cs`
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.PlaneCutJunctionSolver.cs`
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.PlaneCutKernel.cs`
- [x] `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`
- [x] `Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader`
- [x] `Game/Rendering/PixelSurface/Includes/PixelSurfaceGeneratedMassFeatures.hlsl`
- [x] `Game/Rendering/PixelSurface/Includes/PixelSurfaceForwardPass.hlsl`

### Implementation result

- [x] Restored six files byte-for-byte to the accepted V1A.2b source: `MassSurfaceFeatureGenerator.cs`, `MassGenerator.cs`, orchestration, plane-cut kernel, junction solver, and diagnostics logging. Production still enters `EdgeWearEvaluationMode.None`; explicit editor geometry again uses one scalar plane-cut bevel per accepted source edge.
- [x] Removed the reflected/serialized geometric Micro field, settings transport, shader property, Inspector controls, profile/admission/isolation implementation, and Micro suite contracts. Existing serialized YAML keys may remain inert until Unity resaves those assets; no serialized asset was raw-edited.
- [x] Added four material-only controls in `GeneratedMass.cs` and `GeneratedMassEditor.cs`: `Edge Surface Variation Strength`, `Edge Surface Variation Scale`, `Edge Normal Breakup`, and `Edge Material Breakup`. They are recipe-owned and property-block-bound but do not enter geometry settings, production generation state, feature-atlas state, or geometry freshness.
- [x] Added `GeneratedMassEdgeWearSurfaceVariation` in `PixelSurfaceGeneratedMassFeatures.hlsl`. It evaluates two deterministic broad analytic waves from existing object-space position, Generated Mass bounds, and Surface Seed only on enabled UV2.z-marked bevel faces.
- [x] Added bounded bevel-only normal perturbation, subtle signed value breakup, worn-edge brightness/tint-response modulation, and signed smoothness variation. No texture or feature-atlas sample was added.
- [x] Integrated the variation result once per fragment in `PixelSurfaceForwardPass.hlsl`; the original `BuildSurfaceData` base smoothness path remains unchanged and smoothness variation is applied only when the resolved offset is nonzero.
- [x] Added matching hidden shader properties and identical declarations in all four UnityPerMaterial CBUFFER blocks; removed `_GeneratedMassEdgeWearMicroVariation`.

### Invariants and non-goals

- [x] V1A.2b selection, canonical identity, corner solving, scalar plane construction, Macro Coverage/Strength, micro-topology normalization, recovery, topology certification, hard fixtures, edge-40 exclusion, UV2.z face marking, and geometry budgets remain unchanged.
- [x] EW-S1 does not change silhouette, bevel width, candidates, selected edges, solved widths, vertices, triangles, colliders, or geometry fingerprints.
- [x] No scenes, prefabs, materials, recipes as serialized assets, metadata, layers, tags, components, buffers, textures, atlases, or mesh channels changed.
- [x] EW-S1 does not implement corner damage, chips, cracks, silhouette displacement, or broad-face normal variation.

### Validation and compliance

- [x] Static/source/package validation passed: exact 15-file scope; six restored generator/diagnostic files are byte-identical to V1A.2b; all 170 C# files and 38 HLSL/Shader files pass available lexical/delimiter/preprocessor checks; settings constructor/call arity is `12/12`; all four property/CBUFFER/binding chains are complete; active source contains no geometric Micro owner; the accepted zero-strength albedo and base-smoothness source paths remain intact; no serialized asset or mesh-channel change exists; text hygiene and line endings pass; clean patch application, changed-file byte identity, ZIP contents, and CRC pass.
- [ ] Unity must compile the C# and shader changes, run the existing V1A.2b one-click edge-wear suite, and visually compare Strength `0` against enabled EW-S1 controls from the actual game camera.

### Post-implementation consistency and compliance result

- [x] Final diff matches the approved 15-file scope. No code, shader, document, or serialized asset outside that scope changed.
- [x] Complete final changed files and direct settings, geometry, mesh-output, shader-input, and shared-Ground consumers were reread. The only intentional differences from V1A.2b are the removal of the dormant geometric Micro field/property and the addition of EW-S1 authoring/property-block/shader response. The six V2-modified generator/diagnostic owners are restored exactly to V1A.2b.
- [x] Compared with EW-V2A.4, all multi-plane profile construction, depth tiers, isolation, selective admission, additional-plane budgeting, and Micro contracts are absent. Generated Mass geometry therefore no longer performs V2A profile work.
- [x] A deterministic mathematical sample of 1,000 arbitrary object-space lines confirms the two-wave field does not encode a universal centre pinch: the exact line centre was the minimum on `8/1000` lines and the maximum on `7/1000`; average zero crossings decreased from `6.332` at Scale `0.5` to `3.167` at Scale `1` and `1.590` at Scale `2`, supporting the documented higher-Scale/broader-feature control direction.
- [x] Static source evidence confirms enabled EW-S1 adds two analytic `sincos` evaluations only after the generated-bevel-mask/Strength early return and adds no texture samples. Actual GPU cost is unmeasured and must be checked in Unity; no claim of measured runtime cost is made.
- [ ] A compatible Unity/Roslyn/shader compiler is unavailable in this workspace. Unity compilation, shader compilation, actual GPU timing, and visual acceptance remain pending; the concrete next action is the Unity validation sequence below.

### Unity acceptance target

- Strength `0` must show the accepted uniform bevel material response and unchanged V1A.2b geometry.
- Strength `1`, Scale `1`, Normal Breakup `1`, and Material Breakup `1` must visibly break the uniform bevel highlight/material strip without changing silhouette or geometric width.
- The existing V1A.2b topology and artistic-preview matrices must remain `33/33`; outlier recovery must remain `5/5`; edge-40 exclusion must remain `1/1`; no cancellation or terminal failure is permitted.


## EW-V1A.2c — full-range Macro retention closure

**Status:** Source implementation and static/package validation complete; Unity acceptance pending.

### Pre-edit review evidence

- [x] Reread `AGENTS.md` completely. The mandatory review, persistent-plan-first, strict-scope, final consistency, evidence, and validation requirements apply.
- [x] Reconstructed the current authoritative source as the complete EW-S1 overlay in `/mnt/data/audit_s1`; no `.git` directory or Git metadata is present. Historical comparison therefore uses the exact extracted `Assets(74).zip` V1A.2b state, the pre-EW-S1 EW-V2A.4 overlay, the EW-S1 overlay, and the supplied Unity reports.
- [x] Reviewed the complete Macro request path in `MassGenerator.EdgeWear.SelectionAndCorners.cs`: `ResolveEdgeWearMacroRequestedWidth`, candidate/viability mapping, `AuditExplicitChamferCornerSolution`, `TrySolveCornerAwareChamferWidths`, shared-edge uniform scaling, conflict recording, corner-solution application, and lifecycle evidence.
- [x] Reviewed the complete recovery ownership in `MassGenerator.EdgeWear.Orchestration.cs`: certified baseline creation, material-width target recovery, corner-inactive recovery collection, protected-state generation, bounded full-shell augmentation, acceptance, resolution, and final coverage ownership.
- [x] Reviewed the Macro contract and retention comparison in `Editor/GeneratedMassEditor.cs`, the lifecycle/coexistence reporting in `MassGenerator.EdgeWear.Diagnostics.Logging.cs`, the related type contracts, and all four canonical Generated Mass documents.
- [x] Compared relevant current files with V1A.2b and pre-EW-S1. `MassGenerator.EdgeWear.SelectionAndCorners.cs` is byte-identical in all three states. EW-S1 restored orchestration and diagnostics exactly to V1A.2b. The failure is therefore not a pre-Macro source rollback.
- [x] Reviewed accepted Strength `0.55` reports and the failing Strength `0.67` report. Seed 8889 source edge `38` changes from certified to `corner-width-inactive` while its requested width decreases only from `0.0185500644` to `0.0184908062`, and its isolated maximum certified width remains `0.012897186`. The current report records no bounded infeasibility proof and fails Macro retention with `unprovenLosses:{38}`.
- [x] Proven recovery-order defect: `TryAuditCertifiedBaselineAugmentation` collects corner-inactive candidates only after `EvaluateMaterialWidthRecoveryTargets` may replace the certified baseline. `EvaluateMaterialWidthRecoveryTargets` carries all baseline exclusions into each target trial. A corner-inactive edge can therefore be converted into an inherited forced exclusion before corner recovery ownership inspects the original certified-baseline conflict.
- [x] Proven evidence-gate defect: `CollectCornerInactiveRecoveryEdges` iterates exact `ChamferCornerConflictRecord` entries but rejects the record unless `record.CornerRecoveryProvisional` is already true, then calls `ApplyCornerRecoveryProvisionalEvidence`. The exact conflict record should be the authority; pre-populated lifecycle evidence must not be a prerequisite.

### Objective and acceptance criteria

- [ ] Preserve every zero-Macro baseline-certified artistic edge across Macro Strength `0–1`, unless a complete bounded full-shell search proves that edge infeasible at the tested setting.
- [ ] Preserve corner-inactive recovery candidates from the original certified baseline across the material-width recovery phase.
- [ ] Derive provisional corner-recovery evidence directly from exact corner conflict records; do not require the lifecycle flag to be populated beforehand.
- [ ] Keep the material-width recovery target contract, scalar bevel construction, selection, Macro sampling, topology gates, render gates, budgets, edge-40 exclusion, and EW-S1 shader response unchanged.
- [ ] Add a focused seed-8889 Macro Strength sweep at `0`, `0.25`, `0.55`, `0.67`, and `1`. Every baseline-certified edge must remain certified or carry complete proven-infeasible corner/width evidence at every sample.
- [ ] Require the existing current-setting parity, determinism, distribution, topology `33/33`, artistic-preview `33/33`, outlier `5/5`, and negative-exclusion `1/1` contracts to continue passing.
- [ ] Seed 8889 edge `38` must be certified at Strength `0.67`, or the report must contain complete corner-recovery search evidence and a proven-infeasible resolution. An unexplained `corner-width-inactive` result is prohibited.

### Approved implementation scope

- [x] `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- [x] `Docs/Generated_Mass_Framework.md`
- [x] `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
- [x] `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.Orchestration.cs`
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs` — reviewed; no edit required because existing lifecycle export already carries the exact corner-recovery evidence and resolution fields.
- [x] `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

`MassGenerator.EdgeWear.SelectionAndCorners.cs` was reviewed as the producer of exact conflict records but does not require a behavioral edit for this correction. The exact zeroing and conflict-recording math remains unchanged.

### Invariants and non-goals

- [x] No change to `ResolveEdgeWearMacroRequestedWidth`, sampled multiplier range, Coverage semantics, Strength semantics, minimum style width, edge scores, candidate selection, isolated width schedules, corner equations, shared-edge scale search, plane construction, topology repair, normals, tangents, shaders, materials, scenes, prefabs, serialized assets, production `EdgeWearEvaluationMode.None`, or runtime generation.
- [x] No seed-specific production exception. Seed 8889 is a validation fixture only.
- [x] Recovery may exchange only directly recorded conflict participants and must pass the existing complete corner, plane-shell, topology, face-quality, containment, render-channel, and baseline-loss acceptance gates.
- [x] Do not classify search cancellation, timeout, state-budget exhaustion, missing evidence, or incomplete trials as proven infeasible.
- [x] Do not resume corner-damage work until the Macro sweep and existing suite pass in Unity.

### File-by-file implementation sequence

- [x] Record this persistent plan before implementation edits.
- [x] Preserve original certified-baseline corner-recovery candidates and participant evidence before material-width target trials can replace the baseline.
- [x] Merge any additional corner-inactive candidates found in the post-material recovery baseline without discarding the original set.
- [x] Remove the circular provisional-evidence prerequisite from exact conflict-record collection and retain complete source/graph-edge, participant, scale, stage, and last-positive-width evidence.
- [x] Extend diagnostics only where required to distinguish the baseline source of a corner-recovery candidate and its final resolution.
- [x] Extend the editor Macro contract with the fixed five-sample Strength sweep and concise per-sample retention evidence.
- [x] Reconcile the four canonical documents with the full-range retention contract and pending Unity status.
- [x] Run available parser/compiler, source-reference, scope, line-ending, whitespace, diff, clean-apply, byte-identity, and package checks. A compatible C# compiler and Unity are unavailable; all available structural/source checks pass and compilation remains pending.
- [x] Reread every final changed file and affected producer/consumer, compare against the pre-edit EW-S1 state and V1A.2b, and record every intentional difference.
- [ ] Run the Unity one-click suite and record the complete runtime result before marking EW-V1A.2c complete.

### Risks and validation controls

- [x] Risk: corner recovery could remove unrelated baseline-certified edges. Control: retain `IsChamferPlaneRetentionTrialAcceptableForRecovery` and its conflict-participant-only baseline-loss allowance unchanged.
- [x] Risk: material-width target recovery and corner recovery could conflict. Control: material-certified edges remain protected; original and post-material corner candidate sets are merged, not substituted.
- [x] Risk: a broader sweep materially increases editor validation time. Control: exactly five fixed samples, reuse the existing batch case path, no production or per-frame work.
- [x] Risk: diagnostics could falsely claim infeasibility. Control: only exhausted bounded recovery with complete trials may emit `corner-recovery-proven-infeasible`; all other terminal states remain unresolved and fail the contract.
- [x] Risk: scope drift into EW-S1 shader or later damage work. Control: maximum seven-file approved scope and exact final six-file diff audit; `Diagnostics.Logging.cs` was reviewed and remains byte-identical.

### Post-implementation consistency and compliance result

- [x] The final diff is exactly six files: the four canonical documents, `MassGenerator.EdgeWear.Orchestration.cs`, and `Editor/GeneratedMassEditor.cs`. No shader, setting, Macro sampler, corner solver, plane kernel, junction solver, diagnostics logger, scene, prefab, material, or serialized asset changed.
- [x] Original certified-baseline corner conflicts are now collected before material-width trials and merged with post-material conflicts. Exact conflict records populate provisional evidence directly; the bounded recovery frontier and all acceptance gates remain unchanged.
- [x] The Macro contract now includes the fixed full-coverage Strength sweep `0/0.25/0.55/0.67/1` and reports current and per-sample retention separately. Exact exhausted `corner-recovery-proven-infeasible` records count as complete proof; cancellation, timeout, and incomplete search do not.
- [x] Available validation passes: exact scope, all 170 C# files through lexical/delimiter/preprocessor checks, changed-file CRLF/terminal-newline/whitespace/conflict-marker checks, source-reference assertions, unchanged-owner byte identity, clean patch application, changed-file byte identity, and ZIP CRC/content checks. Unity/Roslyn compilation and runtime validation remain unavailable and pending.

### Unity acceptance target

- [ ] Current preview passes with `primaryFailure=none`.
- [ ] Macro zero parity, determinism, distribution, current retention, and five-sample full-range retention pass.
- [ ] Seed 8889 Strength `0.67` has no unproven loss for source edge `38`.
- [ ] Topology and artistic-preview matrices pass `33/33`; outlier closure passes `5/5`; negative exclusion passes `1/1`; cancellation is zero and terminal reason is `none`.


## EW-V1A.2d — zero-baseline protected Macro solve

**Status:** Source implementation and available static/package validation complete; Unity acceptance pending.

### Read-only review evidence

- [x] Reread `AGENTS.md` completely from `/mnt/data/ew_v1a2c_patchrepo/AGENTS.md` before editing. The mandatory review, plan-first, exact-scope, post-change audit, evidence, and validation gates apply.
- [x] Verified the reconstructed authoritative patch repository at `/mnt/data/ew_v1a2c_patchrepo`: `HEAD=fecf6653222750a21b7bbba47438fda1b61e8839` (`baseline`), with only the six EW-V1A.2c working-tree files modified before this patch. No unrelated working-tree changes exist.
- [x] Compared the working tree with `HEAD`, `GeneratedMass_EW-S1_ChangedFiles.zip`, `GeneratedMass_EW-V1A.2c_ChangedFiles.zip`, and the superseded `GeneratedMass_EW-V2A.4_ChangedFiles.zip`. `HEAD` matches the EW-S1 generator/editor baseline; the current six-file diff is exactly EW-V1A.2c.
- [x] Reviewed the complete Macro width producer and corner consumer chain in `MassGenerator.EdgeWear.SelectionAndCorners.cs`: `ResolveEdgeWearMacroRequestedWidth`, viability records, `AuditExplicitChamferCornerSolution`, `TrySolveCornerAwareChamferWidths`, conflict recording, corner-solution application, and requested-width resolution.
- [x] Reviewed the complete recovery orchestration in `MassGenerator.EdgeWear.Orchestration.cs`: current-strength baseline creation, material-width recovery, non-material frontier construction, conflict-participant protection, full-shell certification, final baseline selection, and batch-audit output ownership.
- [x] Reviewed the relevant type ownership in `MassGenerator.EdgeWear.Types.cs`: `EdgeWearCoverageAudit`, lifecycle and viability records, clone semantics, graph identity, corner solutions, and conflict records. `CloneForTrial` currently clones lifecycle records but shares viability objects; any Macro-width override therefore requires an explicit deep viability clone.
- [x] Reviewed the diagnostic consumers in `MassGenerator.EdgeWear.Diagnostics.Logging.cs`, the fixed five-sample Macro contract in `Editor/GeneratedMassEditor.cs`, the public batch result path in `MassGenerator.cs`, the settings producer in `MassSurfaceFeatureGenerator.cs`, and all four canonical Generated Mass documents.
- [x] Reviewed the failing EW-V1A.2c Unity report supplied as `Pasted text(114).txt`. The exact sweep is `0=pass`, `0.25=pass`, `0.55=pass`, `0.67=fail`, `1=fail`; edge `38` remains isolated-width viable but becomes `corner-width-inactive`, and EW-V1A.2c records no restoration attempt.
- [x] Proven ownership defect: `TryAuditCertifiedBaselineAugmentation` names the current-strength result `certifiedBaseline`; it does not construct or receive the zero-Strength certified result used by `EvaluateMacroRetentionContract`. The generator therefore cannot know which current-strength losses violate the accepted zero-Macro baseline.
- [x] Proven architecture boundary: the Editor creates the zero-strength result only after generation by calling `EvaluateMacroVariationCase(1f, 0f)`. That result is diagnostic-only and cannot guide the corner or plane solver.
- [x] Final pre-implementation correction: cloning the current-strength viability audit and merely restoring `RequestedWidth` is not an exact zero-Macro baseline because isolated-width preflight is downward-only and may have stopped at the smaller current request. The protected baseline must rerun `BuildEdgeWearBevelCandidates`, isolated viability, artistic selection, graph mapping, corner solving, material recovery, plane-shell construction, and render validation with Macro Strength exactly zero on the same normalized faces.

### Objective and acceptance criteria

- [x] Construct and fully certify a zero-Macro protected baseline inside generator ownership whenever Macro Strength is nonzero.
- [x] Use the same normalized source faces and authoring selection policy as the requested-strength solve, but rebuild the zero-Macro candidates, isolated viability, selected graph context, and complete shell from scratch; do not approximate the protected result by mutating current-strength viability evidence.
- [x] Compare the final requested-strength certified edge set with the zero-Macro protected set before accepting the result.
- [x] For every protected loss, restore the lost edge and its exact corner-conflict participants toward their zero-baseline requested widths through a finite deterministic schedule; use the graph vertex star only when no exact conflict record exists.
- [x] Accept a restoration trial only when the complete shell, topology, face quality, containment, render channels, material-width recoveries, and the entire protected edge set remain certified.
- [x] If all bounded local restoration trials fail, retain the fully certified zero-Macro protected result rather than deleting a protected edge. Report the fallback explicitly.
- [x] Preserve Macro zero parity, determinism, distribution, and the five-sample retention sweep. Seed 8889 edge `38` must remain certified at Strength `0.67` and `1`, or the suite must fail with explicit protected-solve evidence rather than an unexplained loss.
- [x] Preserve topology `33/33`, preview `33/33`, outlier closure `5/5`, edge-40 exclusion `1/1`, geometry budgets, and production `EdgeWearEvaluationMode.None`.

### Approved implementation scope

- [x] `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- [x] `Docs/Generated_Mass_Framework.md`
- [x] `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
- [x] `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs`
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.Orchestration.cs`
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
- [x] `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

### Invariants and non-goals

- [x] Do not change the Macro hash, salts, sampled multiplier range, Coverage/Strength authoring semantics, edge score, candidate ordering, selected-count policy, scalar bevel plane construction, junction solver, shader response, scenes, prefabs, materials, serialized assets, layers, tags, components, or runtime generation mode.
- [x] Do not add seed-specific production behavior. Seed 8889 remains a validation fixture only.
- [x] Do not reintroduce geometric Micro variation or additional profile planes.
- [x] Do not classify cancellation, timeout, incomplete search, missing evidence, or state-budget exhaustion as proof of infeasibility.
- [x] Keep all new recovery work dirty-time/editor-evaluation only; add no per-frame work.

### File-by-file implementation sequence

- [x] Record this persistent plan before implementation edits.
- [x] Add explicit deep-clone support for viability-owned width data and protected-Macro lifecycle evidence in `MassGenerator.EdgeWear.Types.cs`.
- [x] Add bounded requested-width blending helpers in `MassGenerator.EdgeWear.SelectionAndCorners.cs`; protected zero-Macro viability is rebuilt by the normal candidate/preflight path, while local trials deep-clone current viability and preserve current Macro identity.
- [x] Add an independent zero-strength candidate/context/preflight/certification evaluation, protected-edge comparison, exact conflict/vertex-star participant collection, finite local restoration, and certified zero-baseline fallback in `MassGenerator.EdgeWear.Orchestration.cs`.
- [x] Add concise overall and per-edge protected-Macro evidence in `MassGenerator.EdgeWear.Diagnostics.Logging.cs` without changing public serialized contracts.
- [x] Advance Editor contract labels to EW-V1A.2d and require the existing fixed sweep to pass against the generator-owned protected solve.
- [x] Reconcile `Generated_Mass_Framework.md`, `Generated_Mass_Edge_Wear_Recovery_Architecture.md`, and `Generated_Mass_Edge_Wear_Code_Inventory.md` with the final implementation and supersede the ineffective EW-V1A.2c ownership claim.
- [x] Run all available C# structural/parser checks, namespace/reference scans, exact-scope audit, text hygiene, diff checks, clean-apply, byte-identity, package checks, and post-change reread. Unity compilation and runtime validation must remain explicitly pending if unavailable.

### Risks and controls

- [x] Risk: zero-baseline evaluation reuses current-strength feasibility evidence. Control: rerun the normal candidate and isolated-preflight path with a local zero-strength settings value; use deep viability clones only for later local width-blend trials.
- [x] Risk: restoring one protected edge removes another. Control: acceptance requires the complete protected edge set, not only the current target edge.
- [x] Risk: local width restoration expands beyond the actual conflict. Control: use exact `ChamferCornerConflictRecord.ParticipatingSelectedEdges`; fall back only to the lost edge's two endpoint vertex stars.
- [x] Risk: fallback hides complete loss of Macro variation. Control: emit explicit local-restoration factor or full-zero-baseline fallback evidence and keep the separate Macro distribution contract active.
- [x] Risk: validation time increases. Control: one protected baseline plus four deterministic local factors (`0.25/0.5/0.75/1`); no combinatorial profile search and no per-frame work.
- [x] Risk: EW-V1A.2c dead logic remains. Control: replace or integrate it deliberately, document every retained behavior, and verify no duplicate recovery ownership remains.


### Post-implementation consistency and compliance result

- [x] Final diff is exactly the approved nine files. No settings producer, public generator API, plane-cut kernel, junction solver, shader, scene, prefab, material, recipe asset, metadata, layer, tag, or component changed.
- [x] The protected baseline uses a deep viability clone and a recursion guard; current records are not mutated. Ordinary trial clone behavior remains unchanged.
- [x] Local restoration is finite and deterministic, limited to exact conflict or endpoint-star participants, and requires the union of protected and current built edges.
- [x] EW-V1A.2c remains only as the ordinary current-strength recovery ordering/evidence path. EW-V1A.2d adds a separate final zero-baseline retention gate; no duplicate frontier is introduced.
- [x] Available structural, reference, scope, text-hygiene, diff, clean-apply, byte-identity, and package checks pass. Unity/Roslyn compilation and runtime acceptance remain unavailable and pending.

### Unity acceptance target

- [ ] `status=passed`, `macroVariationContractStatus=passed`, `macroRetention=1`, `retentionCurrent=1`, and `retentionSweep=1`.
- [ ] `retentionSweepCases` passes at `0/0.25/0.55/0.67/1` with no unproven loss for edge `38`.
- [ ] Protected-Macro telemetry identifies the zero-baseline edge set, initial losses, participants, attempted factors, selected factor or fallback, and final resolution.
- [ ] Topology and preview matrices pass `33/33`; outlier closure passes `5/5`; negative exclusion passes `1/1`; cancellation is zero and terminal reason is `none`.


## EW-V1A.2e — asymmetric local Macro preservation

### Read-only review evidence

- [x] Reread `/mnt/data/ew_v1a2d_applycheck/AGENTS.md` completely before editing. The mandatory review, plan-first, exact-scope, evidence, post-change audit, and validation gates apply.
- [x] Reconstructed the authoritative current state in `/mnt/data/ew_v1a2e_work`: `HEAD=69fba12` (`EW-V1A.2c baseline`) plus the exact nine-file EW-V1A.2d working-tree diff. No unrelated tracked changes exist in this reconstructed state.
- [x] Compared the current working implementation with `HEAD`, EW-S1, EW-V1A.2c, EW-V1A.2d, and the supplied EW-V1A.2d Unity report `Pasted text(115).txt`. EW-V1A.2d restores retention but returns `full-zero-baseline-fallback`, causing `macroDistribution=0` and final Macro Strength `0`.
- [x] Reviewed the complete current protected-baseline and local-restoration path in `MassGenerator.EdgeWear.Orchestration.cs`, the trial width override helper in `MassGenerator.EdgeWear.SelectionAndCorners.cs`, protected lifecycle/clone ownership in `MassGenerator.EdgeWear.Types.cs`, diagnostics in `MassGenerator.EdgeWear.Diagnostics.Logging.cs`, the Editor distribution/retention contract in `GeneratedMassEditor.cs`, and all four canonical Generated Mass documents.
- [x] Proven trial-baseline defect: `ApplyMacroProtectedRetention` calls `BuildMacroProtectionTrialCoverage(sourceCoverage, ...)` even after `currentOutcome` may contain certified material/non-material recovery. Local trials therefore restart from the pre-recovery audit rather than the strongest valid requested-Macro result.
- [x] Proven target-width defect: `BuildMacroProtectionTrialCoverage` blends toward protected `RequestedWidth`; the accepted protected evidence is the certified `SolvedWidth`/`MaterializedWidth`. Seed 8889 source edge `38` has protected requested width `0.0188216623` but protected certified width `0.012897186`.
- [x] Proven search-shape defect: EW-V1A.2d tests one uniform interpolation factor across all participants. The failed factors prove only that this single line through width space has no acceptable mixed state; they do not prove that asymmetric participant subsets are infeasible.
- [x] The current exact local participant set is graph edges `13/14/37/38/39`; the source-edge loss is `38` and graph-edge loss is `37`. The identity distinction must be labelled in diagnostics.

### Objective and acceptance criteria

- [x] Start every Macro-protection trial from `currentOutcome.Coverage`, preserving all already-certified requested-Macro, material-width, and non-material recovery state outside the local protection region.
- [x] Force every initially lost protected edge to its exact protected certified width, not its protected requested width.
- [x] For adjusted edges, carry forward only the protected positive certified-width ceiling needed to make that target reachable; retain current Macro identity, locality evidence, and all unrelated viability state.
- [x] Search deterministic asymmetric subsets of the remaining local participants; unchanged participants remain at current requested-Macro widths and selected participants use exact protected certified widths.
- [x] Order states by minimum total normalized deviation from the current requested-Macro widths, then deterministic graph-edge order; accept the strongest fully certified mixed state.
- [x] Require the complete union of protected and current built graph edges, full topology/face-quality/containment/render validation, and measurable Macro distribution.
- [x] Keep full-zero-baseline fallback explicit and contract-failing; never describe it as successful Macro preservation.
- [x] Preserve the fixed `0/0.25/0.55/0.67/1` retention sweep, topology `33/33`, preview `33/33`, outliers `5/5`, negative exclusion `1/1`, and production `EdgeWearEvaluationMode.None`.

### Approved implementation scope

- [x] `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- [x] `Docs/Generated_Mass_Framework.md`
- [x] `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
- [x] `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.Orchestration.cs`
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
- [x] `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`
- [x] `MassGenerator.EdgeWear.SelectionAndCorners.cs` is reviewed and must remain unchanged; EW-V1A.2e will call its existing private partial-class width mutation helper from orchestration.

### Invariants and non-goals

- [x] Do not change Macro sampling, salts, multiplier range, authoring semantics, candidate score/order, selected-count policy, corner equations, plane construction, junction solving, EW-S1 shader response, assets, or runtime mode.
- [x] Do not add seed-specific behavior or geometric Micro variation.
- [x] Do not permit unaffected edges to move away from their current requested-Macro state.
- [x] Do not accept a state that restores one protected edge by dropping another current or protected edge.
- [x] Keep the search finite, deterministic, local, dirty-time only, and capped to eight optional participant edges.

### File-by-file implementation sequence

- [x] Record this plan before implementation edits.
- [x] Extend protected lifecycle evidence in `MassGenerator.EdgeWear.Types.cs` with adjusted-edge and evaluated-state details.
- [x] Replace uniform restoration in `MassGenerator.EdgeWear.Orchestration.cs` with current-outcome-based asymmetric subset search and exact protected certified-width targets.
- [x] Extend compact and per-edge diagnostics in `MassGenerator.EdgeWear.Diagnostics.Logging.cs` with graph/source identity, adjusted subset, state count, and fallback status.
- [x] Advance Editor labels to EW-V1A.2e and explicitly require no full-zero fallback for a nonzero Macro distribution pass.
- [x] Reconcile the framework, recovery architecture, and code inventory with the final implementation and mark EW-V1A.2d superseded by runtime evidence.
- [x] Complete exact-scope, final-file reread, caller/consumer consistency, parser/reference, text hygiene, diff, clean-apply, byte-identity, and package validation. Unity compilation/runtime remain pending because no compatible compiler or Unity installation is available.

### Risks and controls

- [x] Risk: subset count grows. Control: lost edges are mandatory, optional participants are capped at eight, states are ordered by preservation cost, and search stops at the first fully certified minimum-cost tier.
- [x] Risk: protected solved width is absent. Control: target precedence is positive `SolvedWidth`, then `MaterializedWidth`, then isolated maximum certified width, then protected requested width.
- [x] Risk: current-strength isolated preflight stopped below a larger protected certified target. Control: raise only the adjusted edge's local/isolated certified-width ceiling to the independently certified protected target before recomputing dependent fractions.
- [x] Risk: current material recovery is lost. Control: every trial deep-clones `currentOutcome.Coverage`, not the pre-recovery source audit.
- [x] Risk: diagnostics mix graph and source identities. Control: internal search evidence labels graph-edge sets; lifecycle/report conversion separately emits display source-edge sets.
- [x] Risk: no mixed state exists. Control: retain explicit full-zero fallback as a safety output, set unresolved/full-zero evidence, and keep the distribution contract failing.

### Post-implementation consistency and compliance result

- [x] Final incremental diff against the exact EW-V1A.2d baseline contains only the approved eight files. `SelectionAndCorners.cs`, Macro sampling, plane-cut kernel, junction solver, `MassGenerator.cs`, `MassSurfaceFeatureGenerator.cs`, mesh output, and EW-S1 shader includes remain byte-identical to EW-V1A.2d.
- [x] Final caller/signature audit found three `ApplyMacroProtectedRetention` call sites and one definition, each with the same nine-argument contract. All introduced helper definitions and lifecycle consumers resolve within the existing partial class and diagnostics owners.
- [x] Seed-8889 evidence produces 16 deterministic states for the five-edge local region: graph edge `37` is mandatory and tested first alone; optional graph edges are added in normalized preservation-cost order. Its exact protected certified target is `0.012897186`, not the zero-Macro requested width `0.0188216623`.
- [x] Available non-Unity validation passes: exact scope, all 170 C# lexical/delimiter/preprocessor scans, introduced-reference checks, line endings/text hygiene, deterministic subset enumeration through eight optional edges, `git diff --check`, clean incremental patch application, `8/8` byte identity, ZIP contents/CRC, and artifact byte verification.
- [ ] Unity 6000.5.0f1 compilation and EW-V1A.2e runtime acceptance remain pending; the concrete next action is the one-click suite at seed `8889`, Coverage `1`, Strength `0.67`.

### Unity acceptance target

- [ ] `status=passed`, `macroVariationContractStatus=passed`, `macroDistribution=1`, and `macroRetention=1`.
- [ ] At seed `8889`, Coverage `1`, Strength `0.67`: `participants=39`, `varied>0`, edge `38` certified, resolution `asymmetric-local-restoration`, and `fullZeroBaselineFallback=0`; every graph edge outside the reported adjusted subset retains its requested-Macro width.
- [ ] The fixed Strength sweep passes at `0/0.25/0.55/0.67/1`; topology and preview remain `33/33`, outliers `5/5`, negative exclusion `1/1`, cancellation `0`, terminal reason `none`.

## EW-V1A.2f — remove failed Macro protection and normalize certified Strength

**Status:** source implementation and available static/package validation complete; Unity acceptance pending.

### Read-only review evidence

- [x] Reread `AGENTS.md` completely from the supplied authoritative snapshot before editing. Mandatory review, persistent-plan-first, exact-scope, post-change audit, evidence, and validation gates apply.
- [x] Reconstructed the exact supplied current state by overlaying `Assets(74).zip`, `GeneratedMass_EW-S1_ChangedFiles.zip`, and EW-V1A.2c/d/e changed-file packages in order. The resulting current tree contains only the nine c/d/e-owned differences from EW-S1.
- [x] The supplied snapshot contains no `.git` directory. Connected GitHub inspection found repository `R-Andrei/norse-stylized-3d-poc`; its latest visible commit is `b0d9e9db1354cc7270bc9a2cf0d24948e11e8ec4` (`Update current work`) and predates the supplied V1A/S1 implementation. GitHub therefore provides historical context only; the exact supplied package overlay is the authoritative working state for this patch.
- [x] Compared the current tree against the accepted EW-S1 tree and the superseded EW-V1A.2c/d/e packages. The only c/d/e code differences are protected-baseline cloning/telemetry, zero-baseline duplicate evaluation, recovery search, full-zero fallback, retention sweep, and contract labels.
- [x] Reviewed the complete current and accepted EW-S1 versions of `MassGenerator.EdgeWear.Types.cs`, `MassGenerator.EdgeWear.SelectionAndCorners.cs`, `MassGenerator.EdgeWear.Orchestration.cs`, `MassGenerator.EdgeWear.Diagnostics.Logging.cs`, and `Editor/GeneratedMassEditor.cs`, plus direct settings producer `GeneratedMass.cs`, `MassSurfaceFeatureGenerator.cs`, public generator entry points, plane-cut/junction owners, and EW-S1 shader consumers.
- [x] Reviewed the cancelled EW-V1A.2e report `Pasted text(116).txt`: the current preview evaluated 16 states and still selected `full-zero-baseline-fallback`; the artistic matrix reached a 256-state seed-1 maximum-width case, remained synchronous for more than one minute, and was cancelled after only three cases. Macro distribution remained failed while retention passed only through zero fallback.
- [x] Proven complexity defect: EW-V1A.2e enumerates up to `2^8 = 256` local subsets and runs a complete certified shell evaluation for each state. The current implementation has no top-level per-state time budget or cancellation probe between states.
- [x] Proven functional defect: the current result can return Strength zero, participants zero, and varied zero through `full-zero-baseline-fallback`; therefore the c/d/e architecture does not preserve Macro styling and must not remain active.
- [x] Accepted replacement boundary: restore the EW-S1/V1A.2b scalar geometry path, preserve Macro Coverage and deterministic per-edge sampling, and normalize user Strength `1` to the previously Unity-certified internal amplitude `0.55`.

### Objective and acceptance criteria

- [x] Remove all EW-V1A.2c/d/e protected-baseline, deep-clone, local-search, full-zero-fallback, and five-sample retention-sweep code and telemetry.
- [x] Restore the accepted EW-S1/V1A.2b scalar bevel orchestration, corner solve, diagnostics, and type contracts without changing unrelated EW-S1 shader response.
- [x] Keep the Inspector Strength range `0..1`, but map it linearly to internal Macro amplitude `0..0.55`; user Strength `1` must therefore reproduce the previously certified old Strength `0.55` behavior.
- [x] Preserve Coverage semantics, deterministic hashes/salts, sampled multiplier range, minimum-style clamp, candidate selection, corner equations, scalar plane construction, junction solving, topology gates, and production `EdgeWearEvaluationMode.None`.
- [x] Make the one-click suite stop before matrices when the current preview or Macro contract has already failed; stop before the artistic matrix when the topology matrix fails.
- [x] Record direct elapsed milliseconds for current preview rebuild and Macro contract evaluation; retain existing per-case and aggregate matrix timings.
- [x] Require the Macro contract to exercise normalized maximum control Strength `1`, with zero parity, determinism, distribution, and retention against the accepted scalar baseline.
- [x] Acceptance requires current preview, Macro contract, topology `33/33`, preview `33/33`, outliers `5/5`, and negative exclusion `1/1` with no protected-search/fallback telemetry.

### Approved implementation scope

- [x] `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- [x] `Docs/Generated_Mass_Framework.md`
- [x] `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
- [x] `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
- [x] `Game/Procedural/Masses/GeneratedMass.cs`
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs`
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.Orchestration.cs`
- [x] `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
- [x] `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

### Invariants and non-goals

- [x] Do not change shaders, shader includes, materials, scenes, prefabs, recipes, metadata, layers, tags, components, vertex layouts, mesh budgets, or runtime generation mode.
- [x] Do not reintroduce geometric Micro variation, extra bevel planes, zero-baseline duplicate generation, protected-edge recovery, or combinatorial search.
- [x] Do not add seed-specific behavior or silently preserve old serialized Strength amplitudes; the approved semantic change is one normalized `0..1` control whose maximum equals the certified amplitude.
- [x] Do not remove focused matrix buttons or existing topology/outlier/negative evidence.
- [x] Keep all validation work editor-only and add no per-frame work.

### File-by-file implementation sequence

- [x] Record this persistent plan before any implementation edit.
- [x] Restore EW-S1 type contracts, corner/selection owner, orchestration, and diagnostics from the exact accepted package versions.
- [x] Add one canonical generator certified-amplitude constant and apply it only in `ResolveEdgeWearMacroRequestedWidth`; mirror the same literal in the editor-only report owner with a static equality check, then update the user-facing tooltip and Macro diagnostics with control/effective Strength evidence.
- [x] Restore the EW-S1 Editor suite baseline, advance labels to EW-V1A.2f, remove the five-strength sweep and protected-fallback checks, test normalized maximum Strength `1`, add preview/contract timing, and add decisive fail-fast gates.
- [x] Reconcile the framework, recovery architecture, and code inventory; mark EW-V1A.2c/d/e rejected and removed from active code.
- [x] Complete exact-scope, full-file reread, caller/consumer comparison, reference/import scan, C# structural checks, text hygiene, diff, clean-apply, byte-identity, and package validation.

### Risks and controls

- [x] Risk: restoring whole accepted files could remove unrelated EW-S1 work. Control: only the four c/d/e-owned generator files are restored from the exact EW-S1 package; `GeneratedMass.cs` and shader owners are not replaced.
- [x] Risk: normalized Strength is applied twice or the editor label drifts. Control: apply the `0.55` factor only inside the canonical Macro width resolver; the editor mirror is report-only, and static validation requires both literals to remain equal while all settings paths remain raw control values.
- [x] Risk: fail-fast prevents useful reports. Control: always write the combined report with current preview/Macro timing and terminal reason before stopping; matrices run normally only after prerequisite contracts pass.
- [x] Risk: normalized maximum still loses an accepted edge on another matrix case. Control: run both complete `33/33` matrices at control Strength `1` and retain the existing retention contract without fallback allowances.

### Post-implementation consistency and compliance result

- [x] Exact observed scope is the approved ten files. No shader, material, scene, prefab, recipe, metadata, layer, tag, vertex-layout, mesh-budget, or production-mode owner changed.
- [x] `MassGenerator.EdgeWear.Types.cs` and `MassGenerator.EdgeWear.Orchestration.cs` are byte-identical to the accepted EW-S1 package. `SelectionAndCorners.cs` differs from EW-S1 only by the canonical `0.55` normalization constant and its single use in `ResolveEdgeWearMacroRequestedWidth`; `GeneratedMass.cs` differs only by the approved tooltip; diagnostics differ only by control/effective Strength reporting.
- [x] Active C# contains no protected-Macro clone, zero-baseline duplicate generation, restoration search, full-zero fallback, c/d/e suite label, retention sweep, or geometric Micro profile owner.
- [x] The generator and editor report constants are both exactly `0.55`; the generator factor occurs only in the canonical resolver, and all settings/audit fields retain raw control Strength. Sampled mapping across 10,001 control values is bounded and control `1` exactly equals the old effective Strength `0.55`, with minimum multiplier `0.7525`.
- [x] The one-click suite records preview/Macro timings, evaluates normalized maximum control `1`, forces suite matrices to Coverage `1` and control Strength `1`, stops before matrices on prerequisite failure, and stops before the artistic matrix on topology failure while still writing the combined report.
- [x] All 170 C# files pass available lexical, delimiter, preprocessor, and region scans. All changed files pass UTF-8, CRLF, terminal-newline, trailing-whitespace, conflict-marker, exact-reference, clean-diff, clean-apply, and byte-identity checks.
- [x] Compatible C# compiler, Unity 6000.5.0f1, and Unity shader compiler remain unavailable; no Unity compilation or runtime success is claimed.

### Unity acceptance target

- [ ] `contract=EW-V1A.2f-suite`, `status=passed`, `macroVariationContractStatus=passed`, `macroDistribution=1`, and `macroRetention=1`.
- [ ] Report shows control Strength `1`, effective Strength `0.55`, no `macroProtection`, no `full-zero-baseline-fallback`, and direct current-preview/Macro-contract timings.
- [ ] Topology and artistic-preview matrices pass `33/33`; outlier closure passes `5/5`; negative exclusion passes `1/1`; cancellation is zero and terminal reason is `none`.


## EW-V1A.3 — dihedral-biased Macro and EW-S1 breakup removal

### Status

- [x] User authorization received.
- [x] Authoritative uploaded source archive verified and extracted safely.
- [x] Mandatory read-only producer/consumer, Inspector, diagnostics, shader, and canonical-document audit completed.
- [x] Exact implementation scope confirmed at eleven modified files with no created, deleted, moved, renamed, generated, serialized-asset, or metadata files.
- [ ] Dihedral-biased Macro producer implemented.
- [ ] Runtime-helper mathematical contract and per-edge diagnostics implemented.
- [ ] EW-S1 authoring, property transport, Inspector controls, shader properties, CBUFFER members, and fragment work removed.
- [ ] Canonical architecture and code inventory reconciled.
- [ ] Post-implementation full-file, caller/consumer, scope, consistency, performance, static, clean-package, and Unity validation completed.

### Objective and accepted decision

Preserve the accepted EW-V1A.2f deterministic scalar-bevel architecture while making Macro width hierarchy follow convex form: lower-dihedral convex edges may receive more of the deterministic downward width reduction, while sharper convex edges retain more of the base width on average. Remove the visually ineffective EW-S1 object-space surface-breakup system completely and retain the accepted uniform UV2.z-marked bevel brightness/tint/softness response.

### Authoritative source and reviewed evidence

- Source: uploaded `Assets-Code-Archive(6).zip`, SHA-256 `6ae849b22fa1faa6fbb348b2f49b608dd3e9715ae634ed2c6805135813aca009`; `314` archive entries; ZIP CRC passed; no unsafe absolute or parent-traversal paths.
- Extracted source root: `Assets/`; `.git`, `Packages`, `ProjectSettings`, and Unity metadata are absent, so branch/HEAD/diff/history and local Unity compilation are unavailable rather than inferred.
- Current implementation contract is EW-V1A.2f: `GeneratedMassEditor.cs` labels the suite and comprehensive report as EW-V1A.2f.
- `MassGenerator.EdgeWear.SelectionAndCorners.cs` currently resolves deterministic Macro width before owner-normal validation and before `TryClassifyEdgeWearStructuralEdge` supplies `DihedralDegrees`; the resolver consumes no angle input.
- `RequestedWidth`, `RequiredFootprintLength`, and `LengthToWidthRatio` currently derive immediately from that angle-independent width. The final convex width must overwrite all three together after classification.
- Early structural-ineligible records are included in current Macro aggregates/signatures. Their existing deterministic initialization must remain so reordering does not silently produce default-zero diagnostic fields.
- `PixelSurfaceGeneratedMassFeatures.hlsl` currently evaluates two fixed-direction object-space waves with two `sincos` calls and no source-edge identity, tangent, endpoints, normalized along-edge coordinate, or bevel-local frame. It feeds lighting-normal, value/tint-response, and smoothness variation through `PixelSurfaceForwardPass.hlsl`.
- Active EW-S1 ownership outside serialized scene remnants is limited to `GeneratedMass.cs`, `GeneratedMassEditor.cs`, `SH_PixelSurfaceLit.shader`, `PixelSurfaceGeneratedMassFeatures.hlsl`, and `PixelSurfaceForwardPass.hlsl`.
- The shader contains four `UnityPerMaterial` blocks. Their complete field sets are intentionally not identical; the patch removes the four S1 members from every block while preserving each block's existing order and unrelated differences.
- Direct callers/consumers reviewed and unchanged: `MassSurfaceFeatureGenerator.cs`, `MassGenerator.cs`, `MassGenerator.EdgeWear.Types.cs`, `MassGenerator.EdgeWear.Orchestration.cs`, `MassGenerator.EdgeWear.PlaneCutKernel.cs`, `MassGenerator.EdgeWear.PlaneCutJunctionSolver.cs`, and `MassGenerator.MeshOutput.cs`.

### Approved file scope

Modify only:

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Game/Procedural/Masses/GeneratedMass.cs`
6. `Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs`
7. `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
8. `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`
9. `Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader`
10. `Game/Rendering/PixelSurface/Includes/PixelSurfaceGeneratedMassFeatures.hlsl`
11. `Game/Rendering/PixelSurface/Includes/PixelSurfaceForwardPass.hlsl`

Create/Delete/Move/Rename/Generate/Metadata: none. If another source or serialized asset becomes necessary, stop, update this plan, and obtain approval before editing it.

### Dihedral mapping contract

Use the conservative soft-bias constants approved from the audit:

```text
shallow angle                 = 15 degrees
sharp angle                   = 90 degrees
sharp-edge reduction permission = 0.35
```

The C# implementation must normalize the angle before smoothing; it must not pass degrees directly as `Mathf.SmoothStep`'s interpolation parameter:

```text
angle01 = InverseLerp(15, 90, dihedralDegrees)
sharpness = SmoothStep(0, 1, angle01)
anglePermission = Lerp(1.0, 0.35, sharpness)
randomReduction = 1.0 - sampledMultiplier
effectiveStrength = Clamp01(controlStrength) * 0.55
requestedMultiplier = 1.0 - randomReduction * effectiveStrength * anglePermission
finalWidth = Max(minimumStyleWidth, baseWidth * requestedMultiplier)
```

Properties required by construction and the runtime mathematical contract:

- Strength zero, Coverage zero, nonparticipant, and generated-transition paths preserve exact multiplier `1` and exact base requested width.
- For a fixed sampled multiplier and Strength, increasing dihedral cannot reduce the requested multiplier or final width because `anglePermission` is nonincreasing and the final minimum-style `Max` preserves monotonicity.
- Sharp convex edges retain nonzero seeded variation: at maximum Strength and minimum sampled multiplier the 90-degree lower bound is approximately `0.913375`, not full-width locking.
- Participation identity, width identity, hash salts, smooth sampled `0.55..1` stream, Coverage semantics, normalized public Strength, and certified maximum effective Strength `0.55` remain unchanged.

### Implementation sequence

1. [x] In `SelectionAndCorners.cs`, add the pure normalized angle-permission helper and extend the existing Macro resolver so it consumes dihedral while preserving all current deterministic identities and exact zero/nonparticipant behavior.
2. [x] Preserve the current early Macro initialization using the shallow-angle/no-protection state for early structural-ineligible diagnostic continuity; after successful convex classification, rerun the same resolver with the measured dihedral and overwrite `Macro*`, `RequestedWidth`, `RequiredFootprintLength`, and `LengthToWidthRatio` before any width-dependent gate.
3. [x] In `Diagnostics.Logging.cs`, add a public diagnostic contract entry point inside the `MassGenerator` partial class that exercises the actual runtime helper/resolver, not a duplicated Editor formula. Validate bounds, exact parity, determinism, and angle monotonicity over a dense deterministic sample set. Add mapping constants and computed per-edge angle-permission evidence to summaries and edge logs without adding persistent fields to `Types.cs`.
4. [x] In `GeneratedMassEditor.cs`, advance labels to EW-V1A.3, invoke the runtime mathematical contract from the existing one-click Macro contract, include its report/pass state in the final contract, update Macro tooltips/help, and remove all four EW-S1 serialized-property ownership and Inspector controls.
5. [x] In `GeneratedMass.cs`, remove the four EW-S1 shader IDs, recipe fields, serialized fields, public properties, recipe application/comparison/default ownership, and material property-block writes. Do not edit scenes or other serialized assets to remove stale ignored YAML keys.
6. [x] In `SH_PixelSurfaceLit.shader`, remove exactly the four hidden S1 properties and the four matching members from every existing `UnityPerMaterial` block while preserving each block's current unrelated field set/order.
7. [x] In `PixelSurfaceGeneratedMassFeatures.hlsl`, remove the S1 variation struct, object-space wave evaluator, normal perturbation, and smoothness offset. Reduce `ApplyGeneratedMassGeometryEdgeWearResponse` to the exact former zero-variation uniform response formula using the UV2.z face mask directly.
8. [x] In `PixelSurfaceForwardPass.hlsl`, remove variation evaluation, normal modification, and smoothness modification; call the simplified uniform response and preserve all unrelated mask-debug, normal, color, tint, PBR, shadow, and lighting behavior.
9. [x] Reconcile the framework, recovery architecture, and code inventory: EW-V1A.2f is the accepted safety baseline; V1A.3 changes only initial convex requested width and removes rejected S1 breakup; uniform response remains; corner damage/chips remain deferred.
10. [x] Complete post-change validation and record exact evidence below before delivery.

### Invariants and non-goals

- No candidate score/order, coverage selection, corner equation, isolated schedule, material-width recovery, coexistence search, plane construction, junction solve, topology certification, mesh output, vertex channel, production generation mode, seed stream, or minimum-style floor changes.
- No Macro-protection baseline, fallback, subset enumeration, duplicate shell, extra plane, or geometric within-edge Micro path.
- No new serialized seed, control, array, buffer, texture, atlas, mesh channel, component, layer, tag, dependency, scene/prefab/material edit, or per-frame CPU work.
- Concave edges remain excluded. The bias operates only among successfully classified convex candidates.
- The existing uniform bevel Response Strength, Brightness Lift, Worn Edge Tint, Tint Influence, and Softness behavior remains numerically unchanged.
- Existing early-ineligible Macro diagnostic population remains deterministic and nonzero-safe; only successfully classified convex records receive angle-biased overwrite.

### Risks and controls

- **Incorrect C# smoothstep:** degrees passed directly to `Mathf.SmoothStep` would clamp almost all edges to the sharp endpoint. Control: explicit `InverseLerp` followed by `SmoothStep(0,1,t)` and dense runtime-helper test.
- **Stale width-dependent fields:** updating only Requested Width would leave footprint and ratio inconsistent. Control: one helper applies all dependent fields immediately after the final convex resolve.
- **Diagnostic regression from naive reordering:** early-ineligible records could receive default zero identities/widths. Control: preserve early deterministic initialization, then overwrite convex records.
- **Over-suppressed visible variation:** the previously proposed `0.15` permission would leave most 70-95 degree edges near uniform. Control: use `0.35` and expose angle-permission/final multiplier per edge.
- **Shader layout regression:** the four CBUFFERs are not globally identical. Control: remove only the same four S1 members from each existing block and compare all remaining ordered fields against baseline per block.
- **Serialized remnants:** scene YAML still contains old S1 keys. Control: do not raw-edit assets; Unity ignores fields no longer represented by the component.

### Performance contract

- Dirty/build-time CPU remains `O(E)` with `O(1)` incremental memory. The patch adds constant scalar math and a second resolver application only for successfully classified convex edges; it adds no search state or allocation.
- Active rendering GPU improves analytically because enabled bevel fragments no longer execute two `sincos` operations, gradient/covector/tangent-normal work, value/tint-response variation, or smoothness variation.
- Four obsolete property-block writes are removed. No numerical GPU percentage may be claimed without Unity profiler evidence.
- No performance exception is authorized.

### Acceptance and post-change audit requirements

- [x] Actual changed-file set equals the approved eleven files and contains no other operation.
- [x] All changed C# files pass available lexical/parser/compiler checks; all introduced references and namespaces are resolved by static caller/consumer inspection.
- [ ] Dense runtime-helper mathematical audit passes exact zero/generated-transition parity, output bounds, deterministic repeatability, and nondecreasing multiplier/width across increasing dihedral.
- [x] Macro diagnostics expose shallow/sharp angles, sharp permission, sampled multiplier, angle permission, control/effective Strength, final multiplier, and final width.
- [x] Repository search finds no active C#/Inspector/shader references to the four removed S1 controls/functions/properties; only stale serialized scene keys and historical documentation may remain and must be identified.
- [x] Every shader CBUFFER has exactly the four S1 members removed and otherwise preserves its baseline ordered field list.
- [x] The simplified uniform HLSL response matches the former zero-variation branch numerically.
- [x] Static delimiter/preprocessor/line-ending/text-hygiene checks pass for every changed source file.
- [x] A clean changed-files package applies over the uploaded source and reproduces all final changed files byte-for-byte.
- [ ] Unity compilation and the complete EW-V1A.3 one-click report remain pending until run in the user's Unity 6000.5.0f1 project. Required runtime result: zero parity `1`, determinism `1`, angle mapping `1`, distribution `1`, retention `1`, topology `33/33`, artistic preview `33/33`, outliers `5/5`, negative exclusion `1/1`, cancellation `0`, terminal reason `none`.
- [ ] Visual acceptance remains pending: identical seed/camera/lighting captures must show a clearer shallow-versus-sharp width hierarchy while preserving visible variation on sharp edges and the uniform worn response.


### EW-V1A.3 post-implementation evidence

- **Authoritative input:** `Assets-Code-Archive(6).zip`, SHA-256 `6ae849b22fa1faa6fbb348b2f49b608dd3e9715ae634ed2c6805135813aca009`; 314 entries; ZIP CRC passed; no unsafe paths; no Git metadata or runnable Unity project files.
- **Actual operations:** exactly the approved eleven files were modified. No file was created, deleted, moved, renamed, generated, or metadata-edited inside the supplied source tree.
- **C# parser:** Tree-sitter C# parsed all 185 supplied `.cs` files with zero `ERROR` or missing nodes. No C# compiler or Unity executable/project environment was available, so Unity compilation is explicitly pending rather than inferred.
- **Reference/import audit:** all introduced helper, contract, and suite-state names have one defining owner and the expected direct consumers. Existing `System.Globalization`, `System.Text`, and `UnityEngine` imports cover all introduced APIs; no namespace/import was added or found missing.
- **Geometry ordering:** early diagnostic initialization remains at shallow permission. Successfully classified convex edges rerun the same resolver with measured dihedral and atomically overwrite Macro evidence, requested width, required footprint, and length-to-width ratio before footprint/locality gates.
- **Mathematical static mirror:** an independent 10,001-angle sweep over `0..180°` passed endpoint bounds, nonincreasing permission, and nondecreasing fixed-sample multiplier/width for representative samples and Strengths. The sharp-end minimum at sample `0.55`, control Strength `1`, and permission `0.35` is `0.913375`. The in-Unity runtime-helper contract remains pending and must report `angleMapping=1`.
- **EW-S1 removal:** no active C#, Inspector, HLSL, or ShaderLab reference remains. The untouched demo scene contains exactly 25 stale serialized instances of each removed field; these are intentionally inert and were not raw-edited.
- **Shader audit:** each of four `UnityPerMaterial` blocks removed exactly the four EW-S1 members and otherwise preserves its baseline ordered field list (`102→98`, `87→83`, `87→83`, `87→83`). Each removed shader property had baseline occurrence count five and final count zero.
- **Uniform-response parity:** the final HLSL retains the former zero-variation face-mask, softness, lift, value-preserving tint, and final blend formulas. Both `sincos` calls, gradient work, normal perturbation, material variation, and smoothness offset are absent.
- **Structural/text audit:** all changed C#/HLSL/ShaderLab files pass delimiter and preprocessor nesting checks; all eleven files pass NUL, trailing-whitespace, final-newline, and baseline line-ending-convention checks.
- **Package reproduction:** a clean overlay package passed ZIP integrity, reproduced all 11 changed files byte-for-byte, and produced a complete reconstructed tree identical to the final workspace across all 314 supplied files. The final delivery archive is regenerated after this ledger update and revalidated before delivery.
- **Performance:** dirty-time complexity remains `O(E)` with `O(1)` incremental memory and no search/allocation path. Active GPU work improves analytically through removal of two `sincos` evaluations and dependent normal/material/smoothness work on enabled bevel fragments. No numerical runtime claim is made without profiler evidence.
- **Pending acceptance:** Unity 6000.5.0f1 compilation, the complete EW-V1A.3 one-click suite, identical visual captures, and optional profiler comparison. Corner damage remains blocked until explicit visual/freeze approval.


## EW-V1A.3a — convexity-priority Macro allocation and final-width ownership

### Status

- [x] User authorization received after EW-V1A.3 passed technical validation but failed the supplied visual hierarchy review.
- [x] Authoritative EW-V1A.3 source reconstructed from the uploaded source archive plus the delivered EW-V1A.3 changed-file package; `.git` remains absent.
- [x] Complete read-only review covered the current Macro producer, viability records, isolated-width evidence, corner-solved width transport, maximum-coverage plane-cut conflict solver, public batch result, diagnostics, one-click validation, four canonical documents, and direct unchanged callers/consumers.
- [x] Exact implementation scope confirmed at nine modified files with no created, deleted, moved, renamed, generated, serialized-asset, shader, material, scene, prefab, recipe, or metadata files.
- [x] Tiered cross-edge Macro allocation implemented.
- [x] Convexity-priority conflict-cluster width ownership implemented.
- [x] Final-width hierarchy telemetry and runtime contracts implemented.
- [x] Canonical framework, recovery boundary, and code inventory reconciled.
- [ ] Post-implementation scope, consistency, performance, static, clean-package, and Unity validation completed.

### Runtime and visual evidence

The supplied Unity 6000.5.0f1 report `Pasted text(118).txt` passed EW-V1A.3 technically: status passed, Macro zero parity/determinism/distribution/retention/angle mapping all `1`, topology `33/33`, artistic preview `33/33`, outliers `5/5`, negative exclusion `1/1`, cancellation `0`, and terminal reason `none`. The two supplied captures nevertheless show the desired sharp-edge width hierarchy is absent.

The report proves two independent causes:

1. The EW-V1A.3 mathematical contract fixes one random sample while varying angle, so it cannot detect cross-edge inversion between different hashes. At maximum Strength, source edge `4` (`26.5506401°`) retained multiplier `0.939218283`, while source edges `6` and `13` (`90°`) retained only `0.91501838` and `0.923236609`.
2. Source edge `10` (`87.8944397°`, graph edge `34`) received essentially full Macro width (`0.999999404`, requested `0.0225093551`) but isolated construction certified only `0.00725466711`; the maximum-coverage conflict solver then uniformly scaled graph edges `{33/34/36/37/38/39}` to `0.5625`, leaving materialized width `0.00408075005`. The direct conflict was victim graph edge `34` versus foreign graph edge `38`; graph edge `34` was the sharper member.

The existing uniform cluster loop in `MassGenerator.EdgeWear.PlaneCutKernel.cs` applies one requested scale to every edge in the incident conflict star. It has no dihedral priority. The visual failure is therefore not resolved by lowering the existing sharp-angle permission again.

### Objective and accepted decision

Make convex form own both the initial Macro allocation and every discretionary conflict-cluster width reduction:

- Macro Strength remains deterministic, downward-only, scalar per edge, zero-parity safe, and Coverage-controlled.
- Different source-edge hashes may vary only inside non-overlapping dihedral retention tiers, so a lower convexity tier cannot randomly retain more width than a higher tier at the same nonzero Strength.
- The maximum-coverage shell solver protects the sharper directly implicated edge and any still-sharper cluster members while lower-priority cluster members remain reducible.
- A protected sharp edge may be reduced only after all lower-priority members in that conflict cluster have reached their existing geometric minimum scales. No topology, containment, face-quality, or width-floor rule is weakened.
- Hard isolated/locality limits remain authoritative and are reported separately from discretionary conflict scaling. This patch does not invent a wider rail or bypass source-vertex preservation.

### Approved file scope

Modify only:

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Game/Procedural/Masses/MassGenerator.cs`
6. `Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs`
7. `Game/Procedural/Masses/MassGenerator.EdgeWear.PlaneCutKernel.cs`
8. `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
9. `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

Create/Delete/Move/Rename/Generate/Metadata: none. If another source or serialized asset becomes necessary, stop, update this plan, and obtain approval before editing it.

### Tiered Macro allocation contract

At maximum public Strength, participating convex edges use deterministic, non-overlapping retention intervals:

```text
15° <= dihedral < 25° : 0.7525 .. 0.81
25° <= dihedral < 45° : 0.81   .. 0.87
45° <= dihedral < 70° : 0.87   .. 0.92
70° <= dihedral < 85° : 0.92   .. 0.96
85° <= dihedral       : 0.96   .. 1.00
```

The existing smooth hash identity selects within the interval. Public Strength linearly blends from exact multiplier `1` at Strength `0` to that maximum-strength target at Strength `1`. Coverage, participation identity, width identity, hash salts, generated-transition exclusion, canonical original source-edge identity, minimum-style clamp, and constant width along each edge remain unchanged.

Required contracts:

- exact zero-Strength, zero-Coverage, nonparticipant, and generated-transition parity;
- deterministic repeatability;
- every interval is bounded and touches its neighbour without overlap or inversion;
- at the same nonzero Strength, every multiplier in a higher tier is greater than or equal to every multiplier in the tier below;
- actual maximum-control edge records obey the same cross-identity ordering;
- Requested Width, Required Footprint Length, and Length-to-Width Ratio remain atomically derived from the final tiered multiplier.

### Conflict-cluster final-width ownership contract

For each failed maximum-coverage band pass:

1. Resolve dihedral for the victim, foreign edge, and complete incident cluster from the existing coverage audit.
2. Use the sharper directly implicated edge as the protection anchor. Protect every cluster edge whose dihedral is at least the anchor dihedral within numerical tolerance.
3. Reduce every nonprotected cluster edge that remains above its existing `minimumScaleByEdge` floor. Derive the next requested scale from the current minimum scale of this reduction set, not from an already smaller protected or floored edge.
4. Retry the unchanged shell, preparation, band, topology, face-quality, and render contracts.
5. If no lower-priority member remains reducible, retain exactly the highest-priority edge and reduce the remaining protected members. Only when that set also reaches floor may the final protected edge be reduced as an explicitly recorded priority fallback. If an earlier recorded fallback has already lowered the protected edge below another member's immutable geometric floor, record evidence-only fallback stage `3`; it permits the unavoidable floor ordering but does not lower or bypass any floor.

The bounded pass limit remains `32`; reduction factor remains `0.75`; existing per-edge minimum scales remain unchanged. This is a deterministic priority schedule, not a subset search or duplicate-shell frontier.

### Diagnostics and validation contract

- `PlaneCutConflictWidthReductionRecord` reports reduction set, protected set, edge/dihedral priority evidence, fallback stage (`1` protected-set retreat, `2` final protected retreat, or evidence-only `3` immutable-floor ordering), and whether a priority violation occurred.
- `PlaneCutBevelAuditResult` and `EdgeWearBatchAuditCaseResult` expose priority-pass, fallback, and violation counts. `Passed` requires zero priority violations; a proven floor fallback is reported but not automatically invalid.
- The one-click suite advances to `EW-V1A.3a` and reports `macroConvexityHierarchy` plus matrix `convexityPriorityFailures`.
- The runtime Macro contract evaluates the actual resolver across different edge identities and all tiers, not only one fixed sample over changing angles.
- Each matrix case checks active built edges sharing a source endpoint. When conflict scaling occurs, a higher-dihedral incident edge may not receive a lower materialized scale than a lower-dihedral neighbour unless the generator recorded a geometric-floor priority fallback.
- Per-edge output retains the full chain: Macro multiplier/requested width, isolated certified fraction, corner-solved width, conflict materialized scale, and final materialized width.

### Invariants and non-goals

- Do not widen edge `10` beyond its certified isolated rail result in this patch.
- Do not relax foreign-source-vertex, rail, locality, topology, manifold, containment, convexity, bounds, volume, face-quality, triangulation, tangent, normal, or render-channel checks.
- Do not change candidate selection, artistic score, corner equations, isolated width schedule, material-width recovery, coexistence exclusion, mesh output, production `EdgeWearEvaluationMode.None`, or micro-topology normalization.
- Do not restore EW-V2A geometric Micro, EW-V1A.2c/d/e Macro protection, duplicate baselines, asymmetric state searches, fallback shells, EW-S1 breakup, or per-frame work.
- Do not edit scenes, materials, prefabs, recipes, shaders, serialized assets, or metadata.

### File-by-file implementation sequence

1. [x] `SelectionAndCorners.cs`: replace soft angle permission with deterministic tier bounds and target multiplier resolution while preserving existing hashes and early diagnostic initialization.
2. [x] `PlaneCutKernel.cs`: add convexity-priority reduction-set construction, protected/fallback scheduling, telemetry, and unchanged shell retries.
3. [x] `MassGenerator.cs`: extend editor-only batch result with priority counters/evidence and require zero recorded priority violations.
4. [x] `Diagnostics.Logging.cs`: populate new public fields, replace the weak fixed-sample angle contract with cross-identity tier validation, and expand compact/full conflict and final-width evidence.
5. [x] `GeneratedMassEditor.cs`: advance labels, integrate Macro hierarchy and final-width priority into fail-fast/matrix status, and report failure coordinates once.
6. [x] Reconcile the three remaining canonical documents.
7. [x] Run exact-scope, full-file reread, caller/consumer, namespace/reference, all-source parser, preprocessor, text-hygiene, formula, priority-schedule, clean-overlay, patch-apply, and byte-identity validation.

### Performance analysis

- Macro change remains constant scalar work per evaluated edge and allocates no persistent arrays or mesh channels.
- Conflict reduction already rebuilds one candidate shell per bounded pass. Priority scheduling adds one `O(C)` scan for a local cluster of `C` edges and no combinatorial search. In the current seed-8889 six-edge cluster, expected passes may increase from two uniform reductions to several prioritized retries, but remain inside the existing 32-pass cap.
- Memory increase is bounded to small per-pass edge-index lists and telemetry strings in editor/dirty evaluation. Active gameplay and rendering cost remain unchanged.
- A numerical dirty-time claim requires the Unity matrix report; static analysis proves only unchanged asymptotic complexity.

### Risks and controls

- [x] Risk: a noncausal low-dihedral star edge is reduced before the direct conflicting pair. Control: the protection anchor is derived from the victim/foreign pair, while all lower-priority cluster members retreat together; detailed reduction/protection evidence is mandatory.
- [x] Risk: equal-angle conflicts cannot preserve every sharp edge. Control: deterministic score/index tie-breaking retains one highest-priority edge; fallback is permitted only after lower-priority floors and is reported.
- [x] Risk: tier boundaries visibly step. Control: adjacent intervals touch exactly, public Strength blends continuously from uniform width, and no tier overlap permits cross-edge inversion.
- [x] Risk: added retries regress editor validation time. Control: no search frontier, unchanged 32-pass cap, per-case timing retained, and any material timing increase is reported rather than hidden.
- [x] Risk: old validation passes while uniform cluster scaling remains. Control: actual cross-identity tier checks and endpoint-star materialized-scale checks are mandatory suite gates.

### Unity acceptance target

- [ ] `contract=EW-V1A.3a-suite`, `status=passed`, `macroConvexityHierarchy=1`, zero parity/determinism/distribution/retention all `1`.
- [ ] Topology and artistic-preview matrices pass `33/33`, with `convexityPriorityFailures=0`; outliers remain `5/5`, negative exclusion `1/1`, cancellation `0`, terminal reason `none`.
- [ ] Seed 8889 telemetry shows graph edge `34` protected from discretionary cluster scaling while lower-priority members remain reducible, or records a concrete geometric-floor fallback.
- [ ] Identical close views show the prominent sharp edge materially wider than the EW-V1A.3 result without lost bevel coverage or invalid geometry.

### Implemented source state

- `ResolveEdgeWearMacroRequestedWidth` now maps the existing smooth edge identity into one of five non-overlapping dihedral retention intervals. Public Strength blends from exact multiplier `1` to the selected interval value; hash identities, Coverage, generated-transition exclusion, minimum-style clamp, and scalar-per-edge geometry remain unchanged.
- The maximum-coverage conflict loop no longer applies one scale to an entire cluster. It resolves victim/foreign priority from the existing graph-edge coverage map, protects the sharper implicated edge plus any sharper cluster members, and scales only lower-priority members that remain above their existing minimum floors.
- If lower-priority members are exhausted, the solver records fallback stage `1` while retaining the highest-priority edge; only a second exhausted state may use fallback stage `2` and reduce that final edge. The existing 32-pass budget and 0.75 factor remain unchanged.
- Public batch telemetry now reports priority passes, protected reductions, floor fallbacks, violations, and evidence. A result cannot pass if a discretionary reduction bypasses the priority path or mutates a protected member.
- The one-click Macro contract now checks the actual runtime resolver across 10,001 angles and 257 cross-edge identities. Maximum-control actual edge records receive a separate cross-tier inversion check.
- Matrix aggregation reports `convexityPriorityFailures` and rejects a materialized-scale inversion between incident active bevels unless the solver recorded a geometric-floor fallback.

### Post-implementation static evidence

- **Scope:** exactly nine approved files are modified; no file is created, deleted, moved, renamed, generated, or metadata-edited. The reconstructed source remains 314 files.
- **C# structure:** Tree-sitter C# parses all 185 supplied `.cs` files with zero `ERROR` or missing nodes. A separate lexical delimiter and preprocessor scan passes all 228 C#/HLSL/ShaderLab files.
- **Reference/import audit:** every introduced helper and telemetry field has one defining owner and the expected direct consumers. Existing `System`, `System.Collections.Generic`, `System.Globalization`, `System.Text`, and `UnityEngine` imports cover all introduced APIs. No C# compiler or Unity executable is available in the supplied source environment, so Unity compilation remains pending.
- **Macro proof:** the tier resolver passes 10,001 angle samples × 257 identities × three Strengths (`7,710,771` evaluations) with bounded output, deterministic repeatability, nondecreasing fixed-identity angle behavior, and zero cross-tier inversion. Exact zero-Strength, zero-Coverage, and generated-transition parity remain represented by the runtime contract.
- **Seed-8889 worked evidence:** the EW-V1A.3 report maps graph edge `34` to `87.8944397°` and graph edge `38` to `79.507843°`. The new priority rule selects edge `34` as anchor, protects `{34}`, and initially reduces `{33/36/37/38/39}`. Its new Macro target is approximately `0.999999719`; its earlier isolated certified width remains `0.00725466711`, while the rejected EW-V1A.3 final materialized width was `0.00408075005`.
- **Text/package hygiene:** all nine changed files preserve the source tree's CRLF convention, contain no mixed line endings, NUL bytes, trailing whitespace, or extra blank EOF lines. The changed-files ZIP passes CRC and overlays the baseline to reproduce all 314 final files byte-for-byte. The unified patch applies with `patch --binary -p1` and reproduces the same final tree byte-for-byte.
- **Performance:** static inspection confirms `O(E)` Macro mapping and one `O(C)` priority scan per existing bounded conflict pass. The pass cap remains `32`, the reduction factor remains `0.75`, and no subset enumeration, duplicate shell, persistent per-edge allocation, or active-gameplay/rendering work is added. Numerical dirty-time evidence remains pending in Unity.

### Remaining acceptance

- [x] Static full-source and clean-package validation completed.
- [ ] Unity compilation completed with no errors.
- [ ] EW-V1A.3a one-click suite passed, including `macroConvexityHierarchy=1`, zero matrix priority failures, and no protected-edge mutation.
- [ ] Seed-8889 visual review confirms the sharp upper bevel no longer receives the discretionary cluster shrink applied to lower-priority neighbours.

The isolated-width limit for source edge `10` remains explicitly outside this patch. Its expected improvement is removal of the later `0.5625` conflict scale, not bypass of the separately certified isolated width.

## EW-V1A.3b — safe width closure, V1A.3 restoration, and Macro freeze

**Status:** implementation approved; plan recorded before source edits; Unity acceptance pending.

### Objective and decision

- Reject EW-V1A.3a as a freeze baseline because the supplied Unity report completed only `28/33` topology cases and terminated fail-fast with `topologyStatus=failed`.
- Restore the technically passing EW-V1A.3 continuous dihedral-biased request mapping and the original uniform conflict-cluster reduction path.
- Remove the V1A.3a-only tiered Macro, convexity-priority cluster solver, telemetry, and invalid global materialized-scale inversion contract.
- Preserve the already accepted EW-S1 breakup removal and uniform UV2.z bevel response from EW-V1A.3.
- Freeze Macro width work after the complete EW-V1A.3b suite passes. The narrow seed-8889 source edge `10` remains a documented isolated-construction ceiling: requested width `0.0225093607`, maximum isolated-certified width `0.00725466711`; joint vertex-star recovery is explicitly deferred.
- After freeze, the next feature owner is `EW-C1A — Sparse Single-Plane Corner Damage`; no corner-damage implementation is included in this patch.

### Reviewed evidence

- Current source is the corrected EW-V1A.3a nine-file implementation reconstructed from the uploaded authoritative archive and corrected package. No `.git` metadata is available.
- `Pasted text(120).txt` reports `contract=EW-V1A.3a-suite`, `status=failed`, `topologyCases=28/33`, `topologyConvexityPriorityFailures=1`, `failFastTriggered=1`, and `terminalReason=fail-fast: topology viability matrix failed`.
- The same report proves source edge `10` already receives the highest Macro tier and essentially full requested multiplier (`0.999999642`) but certifies only `0.00725466711` in isolated preflight. Macro retuning cannot widen that edge without a new local construction architecture.
- The supplied EW-V1A.3 baseline and its prior complete Unity report passed zero parity, angle mapping, determinism, distribution, retention, topology `33/33`, artistic preview `33/33`, outliers `5/5`, and negative exclusion `1/1`.
- Direct source comparison shows EW-V1A.3a differs from EW-V1A.3 in exactly the nine approved files below. Restoring the five code files to the V1A.3 implementation removes the entire tier/priority experiment without touching unrelated shader, scene, prefab, material, or serialized ownership.

### Approved affected files

Modify only:

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Game/Procedural/Masses/MassGenerator.cs`
6. `Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs`
7. `Game/Procedural/Masses/MassGenerator.EdgeWear.PlaneCutKernel.cs`
8. `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
9. `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

Create/Delete/Move/Rename/Generate/Metadata: none. If another file becomes necessary, stop, amend this plan, and obtain approval before editing it.

### Exact implementation

1. [x] Restore `SelectionAndCorners.cs` to the EW-V1A.3 continuous mapping:
   - `effectiveStrength = Clamp01(controlStrength) * 0.55`;
   - `angle01 = InverseLerp(15, 90, dihedralDegrees)`;
   - `sharpness = SmoothStep(0, 1, angle01)`;
   - `anglePermission = Lerp(1, 0.35, sharpness)`;
   - `finalMultiplier = 1 - (1 - sampledMultiplier) * effectiveStrength * anglePermission`;
   - `requestedWidth = Max(minimumStyleWidth, baseWidth * finalMultiplier)`.
   Preserve hashes, Coverage semantics, early diagnostic initialization, convex overwrite order, and all dependent width fields.
2. [x] Restore `PlaneCutKernel.cs` to the EW-V1A.3 cluster operation. For each implicated cluster member, multiply its current scale by the same requested reduction factor. Remove all V1A.3a priority-set helpers, protected/reduction edge lists, fallback staging, evidence strings, and violation checks.
3. [x] Restore `MassGenerator.cs` and `Diagnostics.Logging.cs` to the EW-V1A.3 public result and telemetry contracts. Remove every V1A.3a convexity-priority field and consumer.
4. [x] Restore `GeneratedMassEditor.cs` to the proven EW-V1A.3 Macro and matrix contracts; remove `macroConvexityHierarchy`, endpoint-star global materialized-scale inversion evaluation, and matrix priority-failure aggregation. Advance user-visible suite labels to `EW-V1A.3b` without changing test content.
5. [x] Reconcile all four canonical documents: EW-V1A.3a is rejected; EW-V1A.3b restores EW-V1A.3 geometry and freezes Macro width after passing Unity validation; edge `10` is a deferred isolated-construction limitation; EW-C1A is next.
6. [x] Complete full-file, caller/consumer, exact-scope, reference/import, parser, text-hygiene, clean-overlay, patch-apply, and byte-identity audits.

### Invariants and non-goals

- Preserve EW-S1 removal, the uniform bevel response, generated-mass shader state, scenes, prefabs, materials, recipes, metadata, and serialized assets.
- Do not alter candidate selection, artistic scoring, micro-topology normalization, corner equations, isolated schedules, material-width recovery, coexistence search, topology repair, plane construction, junction solving, mesh output, production generation mode, or render channels beyond exact EW-V1A.3 restoration.
- Do not add a joint vertex-star solver, relax foreign-source-vertex or rail invariants, or force edge `10` wider.
- Do not implement corner damage in this patch.
- No per-frame work, new allocation architecture, new dependency, new seed, new control, new mesh channel, or performance exception.

### Performance contract

- Restoring EW-V1A.3 removes V1A.3a per-conflict priority scans, list allocations, evidence formatting, and additional prioritized retries.
- Dirty/build-time complexity returns to the previously accepted bounded scalar path. Active gameplay and rendering behavior remain unchanged.
- Memory and storage do not increase.

### Acceptance

- [x] Exact changed-file set equals the nine approved files.
- [x] No active `ConvexityPriority`, `MacroTier`, `macroConvexityHierarchy`, or `materialized-scale-inversion` source reference remains outside historical documentation.
- [x] All supplied C# files parse and all introduced references/imports resolve under available static checks.
- [x] Clean changed-files overlay and unified patch reproduce the final 314-file source tree byte-for-byte.
- [x] Unity compilation succeeds.
- [x] One-click result reports `contract=EW-V1A.3b-suite`, `status=passed`, Macro zero parity/angle mapping/determinism/distribution/retention all `1`, topology `33/33`, artistic preview `33/33`, outliers `5/5`, negative exclusion `1/1`, cancellation `0`, and terminal reason `none`.
- [x] Macro width work is frozen. The next separate activity is the read-only EW-C1A ownership/construction audit.

### Post-implementation static evidence

- Exact scope audit finds only the nine approved modified files across the 314-file reconstructed source. No file is created, deleted, moved, renamed, generated, or metadata-edited.
- `MassGenerator.cs`, `SelectionAndCorners.cs`, `PlaneCutKernel.cs`, and `Diagnostics.Logging.cs` are byte-identical to the previously Unity-passing EW-V1A.3 source. `GeneratedMassEditor.cs` is byte-identical after normalizing only the four contract strings from `EW-V1A.3b` back to `EW-V1A.3`.
- Tree-sitter C# parsing passes all 185 supplied `.cs` files with zero error or missing nodes. No C# compiler or Unity executable is present in the supplied archive; Unity compilation remains pending and is not claimed.
- Static source search finds no active V1A.3a tier, priority, hierarchy, or materialized-scale-inversion symbol. Existing imports are unchanged from the compiled EW-V1A.3 baseline, so no new namespace dependency is introduced.
- The continuous `15°..90°`, `0.35` permission, effective Strength `0.55` formula and the original uniform conflict-cluster scaling loop are present exactly as in EW-V1A.3.
- All nine files preserve CRLF line endings and contain no NUL bytes or trailing whitespace.
- The historical reports confirm EW-V1A.3 passed the complete suite while EW-V1A.3a failed topology at `28/33`.
- The nine-entry changed-files ZIP passes CRC validation and overlays the corrected EW-V1A.3a source to reproduce all 314 final files byte-for-byte. The unified patch applies with `patch --binary -p1` and independently reproduces the same 314-file final tree byte-for-byte.

## EW-V1A.3b-F — Unity acceptance freeze

**Status:** accepted and complete.

### Evidence

The supplied Unity one-click report records:

- `contract=EW-V1A.3b-suite`;
- `status=passed`;
- `currentPreviewPassed=1`;
- Macro zero parity, angle mapping, determinism, distribution, and retention all `1`;
- topology `33/33`;
- artistic preview `33/33`;
- outlier resolution `5/5`, with two certified recoveries, three proven infeasible cases, and zero unresolved;
- negative exclusion `1/1`;
- `cancelled=0`;
- `terminalReason=none`;
- seed `8889` current preview `31/31` candidates active and certified.

### Freeze decision

- EW-V1A.3b is the authoritative edge-width and uniform bevel-response baseline.
- Macro Coverage/Strength semantics, continuous dihedral permission, scalar width schedules, corner/shared-width solve, material recovery, uniform conflict-cluster reduction, topology certification, and the uniform UV2.z bevel response are frozen.
- EW-S1 object-space breakup and EW-V1A.3a convexity-priority recovery remain rejected.
- Seed-8889 edge `10` is accepted at its certified isolated width. A joint vertex-star recovery is deferred and is not an active task.
- No source code, shader, scene, prefab, material, recipe, metadata, or serialized asset changes are required for this freeze.

### Historical EW-C1A-RO assumption — superseded by EW-C1A-RO2

The earlier proposal to investigate a raw post-bevel corner plane is closed and superseded. `EW-C1A-RO2` proved the preferred insertion point is after micro-topology normalization and before bevel-candidate construction. A post-bevel cut without a locally bevelled cap ring is prohibited.

The audit outputs, exact construction order, candidate and cut equations, provenance requirements, bounded retry schedule, hard rejection conditions, validation ownership, and staged file scopes are recorded in the authoritative `EW-C1A-RO2` section below. `EW-C1A.1` is implemented and its seed-8889 transaction is accepted. `EW-C1A.1a` is the active validation boundary before C1A.2.

### Freeze-patch compliance evidence

- Actual modified files are exactly the four canonical Markdown documents. No code, shader, serialized asset, metadata, or generated file changed.
- The freeze is grounded in the supplied `EW-V1A.3b-suite` report: complete status passed, both matrices `33/33`, outliers `5/5`, negative exclusion `1/1`, cancellation `0`, and terminal reason `none`.
- All four modified documents use CRLF consistently, contain no NUL bytes or trailing whitespace, and contain no active statement that EW-V1A.3b acceptance remains pending.
- A clean comparison against the reconstructed EW-V1A.3b source reports four modified files, zero missing files, and zero extra files.
- No Unity rerun is required for this documentation-only freeze. The next technical validation belongs to the future EW-C1A implementation, not this acceptance record.

## EW-C1A-RO2 — Pre-bevel corner-damage ordering and ownership audit

**Status:** complete. **Decision:** `A. PRE-BEVEL CUT APPROVED`.

This section is the authoritative replacement for the earlier `EW-C1A-RO` post-bevel investigation wording. Historical references to a raw post-bevel cut are superseded. A post-bevel cut without a locally bevelled cap ring is prohibited.

### Source and repository evidence

- Authoritative current source was reconstructed from `Assets-Code-Archive(6).zip`, then overlaid with `GeneratedMass_EW-V1A.3_ChangedFiles.zip`, `GeneratedMass_EW-V1A.3b_ChangedFiles.zip`, and `GeneratedMass_EW-V1A.3b_Freeze_ChangedFiles.zip` in that order. The final tree contains `314` files. No `.git` directory is present, so branch, `HEAD`, status, and local history are unavailable.
- `Game/Procedural/Masses/MassGenerator.PlaneCut.cs::BuildPlaneCutMass` builds the convex source `List<PolygonFace>`, triangulates an immutable placement reference, and then calls `ApplyGeneratedEdgeWearBevels` only for explicit evaluation modes.
- `Game/Procedural/Masses/MassGenerator.EdgeWear.Orchestration.cs::ApplyGeneratedEdgeWearBevels` currently calls `NormalizeEdgeWearMicroTopology`, assigns `edgeWearFaces`, then immediately calculates bounds and calls `BuildEdgeWearBevelCandidates`. The exact preferred insertion point is between normalized-face assignment and candidate construction.
- `Game/Procedural/Masses/MassGenerator.EdgeWear.Graph.cs::TryBuildEdgeWearTopologyGraph` already provides normalized vertex-to-edge, vertex-to-face, edge-to-face, face-to-edge, and face-to-vertex adjacency. `EdgeWearGraphVertex.EdgeIndices` and `.FaceIndices` are the authoritative corner-candidate incidence sets.
- `Game/Procedural/Masses/MassGenerator.Polyhedron.cs::ClipPolyhedron`, `ClipPolygonExact`, `TryResolveExactClipIntersection`, `CreateOrientedFace`, `WeldSharedVerticesByDistance`, and `SanitizeAllFaces` provide the reusable half-space, exact-intersection, cap creation, welding, and polygon-cleanup primitives.
- `ClipPolyhedron` is not directly sufficient for C1A because it returns `void`, can abandon a failed exact clip without structured evidence, and does not export descendant-edge or cap-ring identity. C1A.1 must wrap the same primitives in a transactional `Try...` operation on a clone and commit only a fully certified result.
- `Game/Procedural/Masses/MassGenerator.EdgeWear.BoundedSingleEdge.cs::TryTriangulateBoundedPreviewFaces` performs final polygon triangulation. EW-C1A.1a routes every accepted polygon through `TryTriangulateBoundedOneSurfaceFace`, which emits direct boundary triangles with one authoritative polygon normal and one authored surface group.
- `Game/Procedural/Masses/MassGenerator.cs::GenerateInternal` transforms the complete triangle soup and only then calls `MassGenerator.MeshOutput.cs::BuildMeshData`. `BuildMeshData` derives a final geometric face normal unless the triangle carries an authored normal. `Game/Procedural/Core/MeshBuilder.cs::ApplyToMesh` sets those normals and calls `Mesh.RecalculateTangents()` after the final topology is assigned. Therefore every corner cut and every cap-ring bevel necessarily precedes final normals and tangents.

### Ordering decision

The approved final construction order is:

```text
1. Build the convex source PolygonFace polyhedron.
2. Run accepted micro-topology normalization.
3. Build the normalized topology graph.
4. Select at most one deterministic source corner.
5. Clone the normalized PolygonFace list.
6. Apply one bounded corner cut to the clone.
7. Create exactly one CornerDamageCap face.
8. Weld/sanitize and certify the damaged source polyhedron.
9. Preserve original IDs for untouched and shortened original-edge descendants.
10. Assign deterministic generated identity to cap-ring edges.
11. Build ordinary bevel candidates on the damaged polyhedron.
12. Build dedicated stable-width cap-ring bevel candidates.
13. Run the existing corner/shared-width, isolated, coexistence, topology, face-quality, and render certification.
14. Triangulate the complete final polygon shell.
15. Assign authored bevel/cap normals and surface groups; derive remaining geometric normals.
16. Apply dimensions and immutable placement frame.
17. Build MeshData and validate all render channels.
18. Apply the Unity Mesh and recalculate tangents.
19. Apply uniform worn-edge material response.
```

Normals are not a precondition for cutting. Source face normals are plane ownership data used by the polygon kernel. Final render normals are produced only after corner damage and bevel topology are complete.

### Candidate mathematics

A normalized graph vertex is eligible only when all conditions are true:

```text
incidentFaceCount == 3
incidentEdgeCount == 3
all three incident edges are closed manifold
convexIncidentEdgeCount >= 2
maximumIncidentDihedral >= 55 degrees
minimumIncidentEdgeLength >= 8 * minimumStableEdgeLength
vertex is not consumed by a suppressed micro-topology component
```

Each incident edge is classified with the existing `TryClassifyEdgeWearStructuralEdge` function and its existing solid-centre/tolerance contract.

For eligible normalized graph vertex `v`:

```text
sharpness = saturate((maximumIncidentDihedral - 35) / 65)
size = saturate(minimumIncidentEdgeLength / massBoundsDiagonal)
upwardExposure = saturate(dot(normalize(v - solidCentre), up) * 0.5 + 0.5)
random = StableHash01(shapeSeed, normalizedGraphVertexIndex, CornerDamageSelectionSalt)
score = 0.55 * sharpness + 0.25 * size + 0.15 * upwardExposure + 0.05 * random
```

Choose the highest score, then the lower normalized graph vertex index as the deterministic tie-breaker. C1A selects no more than one corner per mass.

### Cut mathematics

For selected vertex position `v`, incident face set `F`, and shortest incident edge length `Lmin`:

```text
outwardNormal = normalize(sum(face.Normal * faceArea(face)) for face in F)
depthHash = StableHash01(shapeSeed, normalizedGraphVertexIndex, CornerDamageDepthSalt)
baseDepth = Lmin * lerp(0.08, 0.16, depthHash)
trialFactors = { 1.0, 0.75, 0.5625, 0.421875 }
depth = clamp(baseDepth * trialFactor,
              2 * minimumStableEdgeLength,
              0.18 * Lmin)
planePoint = v - outwardNormal * depth
retain p when dot(outwardNormal, p - planePoint) <= cutTolerance
```

The first fully certified trial wins. If all four trials fail, the result is the unchanged normalized source polyhedron.

### Identity contract

Current exact-key normalization mapping is sufficient for untouched edges but not for the three original edges shortened by the cut. C1A must add explicit result-owned identity maps:

```text
untouched original edge key -> existing OriginalSourceEdgeIndex
shortened original-edge descendant key -> same OriginalSourceEdgeIndex as its parent
cap-ring edge key -> deterministic generated cap-ring identity
cap face -> PolygonFaceProvenanceKind.CornerDamageCap,
            ProvenanceIndex = selected normalized graph vertex index
```

Cap-ring generated identity must derive from the selected normalized graph vertex and the ordered pair of intersected original source-edge IDs. It must not derive from floating-point coordinates, output vertex order, or mutable face order.

### Cap and cap-ring contract

The cut is not visually complete until the new cap ring is physically bevelled. A raw sharp cap ring may exist only inside C1A.1 diagnostic evidence and must not become the accepted preview.

The C1A.2 requested cap-ring width is:

```text
requestedCapRingWidth = min(
    ordinaryRequestedWidth,
    0.20 * acceptedCornerDepth,
    0.15 * shortestCapRingEdgeLength)
```

A cap-ring edge is eligible only when it is closed manifold, its measured cap/adjacent-face dihedral is at least `25` degrees, and its length is at least `4 * requestedCapRingWidth`. Cap-ring width is constant and receives no Macro variation in C1A.

### Transaction and rejection contract

C1A.1 must clone the normalized faces and commit only when all checks pass:

```text
exactly one cap face
cap boundary has at least three unique vertices
closed manifold topology
open edges == 0
non-manifold edges == 0
T-junctions == 0
all faces finite, planar, simple, convex, outward, and above minimum area
maximum cap plane residual within exact clip tolerance
volume loss > 0 and <= 12 percent of normalized source volume
bounds do not expand beyond tolerance
unrelated original edges remain present through identity mapping
vertex/triangle budgets remain within the accepted Generated Mass tier
final render certification succeeds
```

C1A.2 additionally requires every unrelated EW-V1A.3b certified bevel to remain active or to have an explicit local corner-star ownership reason. No unrelated bevel may disappear silently.

### Staged implementation order

#### EW-C1A.1 — Transactional pre-bevel corner cut and provenance proof

**Goal:** prove selection, clipping, cap topology, original-edge descendant mapping, and bounded four-depth certification without changing the accepted preview mesh.

**Expected modified files:**

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Game/Procedural/Masses/MassGenerator.cs`
6. `Game/Procedural/Masses/MassGenerator.Types.cs`
7. `Game/Procedural/Masses/MassGenerator.Polyhedron.cs`
8. `Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
9. `Game/Procedural/Masses/MassGenerator.EdgeWear.Orchestration.cs`
10. `Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs`
11. `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
12. `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

No new source file, serialized field, asset edit, shader/material change, mesh channel, dependency, production-mode change, or per-frame path is permitted in C1A.1.

**Implementation status:** source implementation and static/package validation complete; the seed-8889 transaction certified trial `0` and is accepted. The next pending gate is EW-C1A.1a visual/non-regression validation.

**Reviewed current-source evidence:**

- `ApplyGeneratedEdgeWearBevels` calls `NormalizeEdgeWearMicroTopology` and then `BuildEdgeWearBevelCandidates`; the diagnostic transaction executes between these calls but does not substitute trial faces into the accepted preview path.
- `NormalizeEdgeWearMicroTopology` supplies normalized faces, a normalized graph, original-edge remaps, generated-transition keys, suppressed-edge evidence, bounded volume, and bounded source dimensions.
- `ClipPolyhedron`, `ClipPolygonExact`, `TryResolveExactClipIntersection`, `CreateOrientedFace`, `WeldSharedVerticesByDistance`, and `SanitizeAllFaces` own the existing half-space and cap primitives. C1A.1 adds a structured transactional wrapper because the current `void` mutator cannot prove success or identity ownership.
- `TryBuildEdgeWearTopologyGraph`, `AuditEdgeWearTopology`, `CalculatePlaneCutPolyhedronVolume`, `CalculatePolygonArea`, `CalculatePolygonNormal`, `IsBoundedPolygonConvex`, and `ClonePolygonFacesForPlaneCutAudit` are reusable through the `MassGenerator` partial class without expanding file scope.
- The reconstructed authoritative source contains 314 files, no `.git` metadata, and matches the completed EW-C1A-RO2 audit source byte-for-byte.

**Candidate contract:**

```text
incident faces == 3
incident edges == 3
all incident edges manifold
convex incident edges >= 2
maximum incident dihedral >= 55 degrees
minimum incident edge length >= 8 * minimumStableEdgeLength
vertex is not consumed by micro-topology normalization
```

```text
sharpness = saturate((maximumIncidentDihedral - 35) / 65)
size = saturate(minimumIncidentEdgeLength / massBoundsDiagonal)
upwardExposure = saturate(dot(normalize(vertex - solidCentre), up) * 0.5 + 0.5)
random = stableHash01(shapeSeed, normalizedGraphVertexIndex, CornerDamageSelectionSalt)
score = 0.55 * sharpness + 0.25 * size + 0.15 * upwardExposure + 0.05 * random
```

The highest score wins; exact score ties resolve to the lower normalized graph vertex index.

**Cut contract:**

```text
outwardNormal = normalize(sum(incidentFace.Normal * incidentFaceArea))
baseDepth = minimumIncidentEdgeLength * lerp(0.08, 0.16, stableDepthHash)
trialFactors = { 1.0, 0.75, 0.5625, 0.421875 }
depth = clamp(baseDepth * trialFactor,
              2 * minimumStableEdgeLength,
              0.18 * minimumIncidentEdgeLength)
planePoint = selectedPosition - outwardNormal * depth
retain p when dot(outwardNormal, p - planePoint) <= tolerance
```

Each trial starts from a fresh deep clone. Failed trials never mutate the frozen source or preview faces.

**Certification and identity contract:**

- exactly one `CornerDamageCap` face;
- cap has at least three unique vertices, stable area, correct winding, and bounded plane residual;
- zero open edges, zero non-manifold edges, and zero T-junctions;
- every face finite, planar, convex, outward-facing, and above stable area;
- positive volume loss no greater than `0.12`;
- no bounds expansion beyond tolerance;
- untouched normalized original edges retain their original identity;
- shortened incident edges map to their parent original identities;
- cap-ring edges receive deterministic identities from selected normalized vertex, ordered intersected parent-edge IDs, and a fixed salt;
- no duplicate or ambiguous identity mapping.

**File-by-file sequence:**

1. [x] Add `CornerDamageCap` provenance and diagnostic result types.
2. [x] Add the cloned structured cut helper and exact blocker evidence.
3. [x] Add deterministic candidate scoring, depth selection, certification, and identity maps.
4. [x] Add editor-only orchestration capture without preview substitution.
5. [x] Add report formatting, `Library` output, Inspector run button, clipboard copy, and reveal action.
6. [x] Reconcile all four canonical documents with final symbols and behavior.
7. [x] Run complete scope, caller/consumer, C# parse, delimiter, identity-math, package-overlay, and patch-reproduction validation.

**Static acceptance:** exact 12-file scope; all C# parses; no unresolved references; deterministic one-candidate/four-trial bound; failed-trial source immutability; one-or-zero certified cap; closed topology; volume loss `<= 0.12`; complete descendant/cap-ring identity evidence; report save/copy/reveal ownership; clean overlay and patch reproduction.

**Unity acceptance:** compile successfully, then run one explicit seed-8889 EW-C1A.1 audit. The report must include all candidates, the selected normalized vertex, all attempted depth factors, exact blocker or certified result, cap/topology/volume evidence, and identity maps. Visual acceptance is not part of C1A.1 because the accepted preview remains unchanged.

**Static completion evidence:** exact 12-file scope; all 185 supplied C# files parsed; all preprocessor blocks balanced; no unresolved new source symbol found by the source-level caller/consumer audit; transaction branch proven after normalization and before bevel candidates; failed trials operate only on cloned faces; fixed one-candidate/four-trial bounds; candidate/depth/volume/identity constants verified; no serialized control, asset, shader, mesh-channel, production-mode, or per-frame ownership added; changed-files overlay and unified patch reproduce the complete 314-file source tree byte-for-byte.

#### EW-C1A.2 — Visible corner-cut and mandatory cap-ring bevel preview

**Patch identifier:** `EW-C1A.2`

**Status:** [implementation complete; Unity compile and visual validation pending]

**Goal:** commit the already-certified C1A.1 damaged polyhedron in one explicit editor-only preview, preserve stable original identities on untouched and shortened descendant edges, create a dedicated mandatory non-Macro cap-ring candidate class, and render one physically softened damaged corner without changing production geometry.

##### Read-only evidence reviewed before implementation

- Repository state: the supplied authoritative archive contains `331` files and no `.git` directory. `git status`, `HEAD`, branch, remote, and commit-history comparisons are unavailable. The untouched extracted archive is the pre-edit baseline.
- `MassGenerator.EdgeWear.SelectionAndCorners.cs::EvaluateCornerDamageTransaction` certifies one of four bounded trials but currently stores only the accepted trial index. `EvaluateCornerDamageTrial` owns the prepared damaged face list as a local value and discards it after certification.
- `MassGenerator.EdgeWear.Types.cs::CornerDamageTransactionAuditResult` contains candidate/trial evidence but no committed face list, damaged-edge identity map, cap-ring key set, or affected-parent set.
- `MassGenerator.EdgeWear.SelectionAndCorners.cs::AuditCornerDamageIdentityMapping` already proves untouched, shortened-descendant, and cap-ring mappings. Those records are diagnostic-only and are not consumed by ordinary candidate construction.
- `MassGenerator.EdgeWear.SelectionAndCorners.cs::BuildEdgeWearBevelCandidates` resolves ordinary identity only through `EdgeWearMicroTopologyNormalizationResult`, applies Macro requested-width resolution to every structural edge, and excludes artistically filtered candidates unless an audit requests all geometric candidates.
- `MassGenerator.EdgeWear.Orchestration.cs::ApplyGeneratedEdgeWearBevels` sorts every candidate only by artistic score and calculates coverage over the complete candidate count. Without an explicit cap-ring class, a cap edge can receive Macro variation, be omitted by artistic filtering, or be dropped by coverage.
- `MassGenerator.EdgeWear.BoundedSingleEdge.cs::TryTriangulateBoundedPreviewFaces` already routes every final polygon, including `CornerDamageCap`, through the accepted one-polygon/one-surface triangulator. No triangulation change is justified.
- `MassGenerator.MeshOutput.cs::ResolveTransformedAuthoredSurfaceNormals` is the accepted EW-C1A.1a.8 final shared-normal owner. No final-normal or `0.5` guard change is justified.
- `GeneratedMass.cs::RegenerateInternal` and `GeneratedMassEditor.cs::DrawEdgeWearViabilityMatrixControls` own explicit editor preview application, stale state, Inspector actions, report persistence, and clipboard delivery.

##### Approved file scope

**Modify:**

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Game/Procedural/Masses/MassGenerator.cs`
6. `Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
7. `Game/Procedural/Masses/MassGenerator.EdgeWear.Orchestration.cs`
8. `Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs`
9. `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
10. `Game/Procedural/Masses/GeneratedMass.cs`
11. `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

**Create/delete/move/rename:** none.

##### Explicit non-goals and frozen owners

- Do not add serialized corner controls or defaults in C1A.2. Control authoring remains C1A.3 after visual approval.
- Do not change production `EdgeWearEvaluationMode.None` behavior.
- Do not modify `MassSurfaceFeatureGenerator.cs`, `MassGenerator.Polyhedron.cs`, `MassGenerator.EdgeWear.BoundedSingleEdge.cs`, `MassGenerator.MeshOutput.cs`, shaders, materials, scenes, prefabs, recipe assets, mesh channels, layers, tags, or metadata.
- Do not reopen Macro variation, scalar recovery, one-surface triangulation, final transformed shared-normal selection, or the `0.5` agreement guard without new concrete regression evidence.
- Do not emit a raw or partially bevelled cap. Any missing, deferred, rejected, or unbuilt mandatory cap-ring edge rejects the complete corner preview and falls back to production geometry.

##### Implementation sequence

1. [x] Extend the internal C1A transaction result so the accepted trial retains the prepared damaged faces, one cap face, accepted depth, stable identity by damaged `EdgeKey`, cap-ring keys, and the three affected original parent identities. Reject any generated identity collision with an original identity.
2. [x] Add an explicit public `GenerateCornerDamagePreview` status/API. `MassGenerator.PlaneCut.cs` is frozen and outside the approved scope; therefore the synchronous editor-only request uses a `[ThreadStatic]` scoped corner-preview context while invoking the existing `UnifiedBoundedPreview` return path. The context is incremented only around the corner generation and restored in `finally`, so ordinary unified preview behavior remains unchanged without expanding scope.
3. [x] Extend candidate/lifecycle records with a candidate class and mandatory flag. Resolve untouched and descendant identity from the committed damaged-edge map. Mark cap-ring edges as `CornerDamageCapRing`.
4. [x] Resolve the initial cap-ring requested width as `min(0.50 * ordinary requested width, 0.25 * accepted cut depth, 0.20 * shortest cap-edge length)`. Require the existing minimum style width; otherwise reject the preview. Cap-ring candidates use this fixed requested width, bypass Macro participation, and remain subject to existing locality, isolated viability, corner/coexistence, topology, face-quality, and render certification.
5. [x] Order mandatory cap-ring candidates by stable generated identity before ordinary candidates. Select all mandatory candidates plus `ceil(ordinary candidate count * existing coverage)`. Preserve ordinary coverage semantics and existing artistic score ordering.
6. [x] Generate the frozen unified preview once as an editor-only baseline, then generate the corner preview. Compare built ordinary edges by stable original identity. The three affected parent identities are exempt; every other baseline-built original edge must remain built. Reject the corner preview on collateral loss.
7. [x] Require exactly one cap, exactly the certified cap-ring count, and every mandatory cap-ring edge selected, active, built, and present in the final debug/audit state. Return production geometry on any failure.
8. [x] Add `GeneratedMass` preview state and an Inspector action named `Rebuild EW-C1A.2 Corner-Chip Preview`, plus a complete report saved under `Library`, copied automatically to the clipboard, and exposed through Copy/Reveal buttons.
9. [x] Update the framework, recovery architecture, and code inventory with the final accepted implementation ownership. Do not record Unity acceptance before the user runs the patch.
10. [x] Reread every modified file and affected caller/consumer, compare the final tree with the untouched 331-file baseline, run available delimiter/preprocessor/reference/scope/whitespace/package checks, and record Unity compile/visual validation as pending.

##### Acceptance criteria

- [x] Available source-level delimiter, preprocessor, symbol-reference, exact-scope, frozen-owner, line-ending, whitespace, ZIP-overlay, and patch-reproduction checks pass.
- [ ] Unity compiles with no new error or unresolved symbol.
- [ ] The seed-8889 explicit preview certifies one transaction and applies one damaged corner.
- [ ] Exactly one cap is present and every cap-ring edge is mandatory, selected, built, and rendered.
- [ ] No raw or partial cap can reach the visible mesh.
- [ ] All baseline-built original bevel identities outside the three affected parent edges remain built.
- [ ] Closed topology, bounds, positive volume, vertex budget, one-surface rendering, final normal agreement, and render channels pass.
- [ ] Production generation and the existing unified preview remain unchanged when the new action is not selected.
- [ ] The report is written, copied to the clipboard, and available through Copy/Reveal controls.
- [ ] User visual review confirms a readable softened chip with no X/radial triangulation boundaries before C1A.3 begins.

##### Performance contract

- The new work is explicit editor/dirty-time only. Active-gameplay CPU, GPU, fragment work, textures, buffers, and persistent runtime memory do not change.
- Corner selection remains `O(V + E)` with at most four cloned cut trials.
- Cap-ring candidate count is bounded by the cap polygon edge count, currently three for the accepted seed-8889 transaction.
- The explicit preview performs one frozen unified baseline generation plus one corner generation so unrelated built-edge retention is proven by stable identity. This approximately doubles explicit preview construction cost but adds no recurring work.
- No performance exception is approved for per-frame or production rebuilding.

##### Static completion evidence

- Exact actual scope: the approved `11` modified files; `0` created, deleted, moved, or renamed files; all `331` supplied files preserved.
- Frozen geometry/render owners remain byte-identical: `MassGenerator.Polyhedron.cs`, `MassGenerator.EdgeWear.BoundedSingleEdge.cs`, `MassGenerator.MeshOutput.cs`, `MassGenerator.PlaneCut.cs`, and `MassSurfaceFeatureGenerator.cs`.
- The accepted C1A.1 five-field `identity=` report tuple remains compatible; generated-ID collision evidence is appended as the separately named `generatedIdentityCollisions=` field.
- Source-level validation passed `61/61`: all `195` supplied C# files have balanced delimiters and preprocessor blocks; all new symbol references resolve within the supplied source; no conflict markers or trailing whitespace; all modified files retain CRLF endings.
- The changed-files ZIP overlay and unified patch each reproduced the complete final `331`-file `Assets` tree byte-for-byte from the untouched archive baseline.
- A Unity compiler/runtime is unavailable in this environment. Unity compile, seed-8889 preview execution, visual review, and the existing complete one-click regression suite remain explicitly pending.

#### EW-C1A.2a — Corner-damaged construction provenance bridge

**Patch identifier:** `EW-C1A.2a`

**Status:** [implementation complete; static validation passed; Unity compile/runtime/visual validation pending]

**Objective:** preserve the certified semantic corner transaction unchanged, create one exact construction clone whose complete face list is densely attributed as `SourceFace`, and feed only that construction clone into the existing bounded-bevel viability and shell pipeline. This repairs the observed `mandatoryCapRing=3/0/0/0` failure without changing clipping, candidate geometry rules, triangulation, final normals, shaders, controls, or production behavior.

##### Unity failure evidence

- Seed `8889` reported `transactionCertified=1`, `acceptedTrial=0`, `capFaces=1`, three generated cap-ring identities, and `mandatoryCapRing=3/0/0/0` with diagnostic `no geometrically viable edge-wear candidates`.
- The same report showed `cornerConstruction=0/0/0/0/0/0`; therefore the failure occurred before topology/corner construction and not in the certified cut itself.
- `MassGenerator.EdgeWear.BoundedSingleEdge.cs::AuditBoundedSingleEdgeBevel` sets `expectedSourceFaceCount = sourceFaces.Count`, clones the complete input with `assignSourceFaceProvenance: true`, and requires all expected indices to appear exactly once.
- `MassGenerator.EdgeWear.PlaneCutKernel.cs::ClonePolygonFacesForPlaneCutAudit` assigns `SourceFace` only when existing provenance is `None`; it intentionally preserves `CornerDamageCap` provenance.
- The committed damaged polyhedron contains exactly one semantic `CornerDamageCap`, so its attributed raw baseline can contain at most `sourceFaces.Count - 1` `SourceFace` records. Every isolated viability audit therefore rejects before any candidate can survive.

##### Read-only review and comparison evidence

- The current authoritative tree contains `331` files and no `.git` directory. Branch, HEAD, status, and history comparisons are unavailable; `/mnt/data/c1a2_failure_audit/current` is the accepted EW-C1A.2 pre-edit baseline and remains untouched.
- Complete current versions and symbol inventories were reviewed for all eight approved files. Direct producer/consumer review included `MassGenerator.EdgeWear.BoundedSingleEdge.cs::AuditBoundedSingleEdgeBevel`, `AuditBoundedSourceFaceProvenance`, `MassGenerator.EdgeWear.PlaneCutKernel.cs::ClonePolygonFacesForPlaneCutAudit`, `MassGenerator.cs::GenerateCornerDamagePreview`, `GeneratedMass.cs`, and `GeneratedMassEditor.cs`.
- `CornerDamageTransactionAuditResult.AcceptedFaces` is the semantic transaction geometry and retains `CornerDamageCap` provenance. `ApplyGeneratedEdgeWearBevels` currently passes it directly to `BuildEdgeWearBevelCandidates`.
- No evidence supports modifying the frozen bounded-bevel audit. Its complete dense source-face contract is correct for its construction input. The missing responsibility is an adapter owned by the committed corner transaction.

##### Approved file scope

**Modify:**

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
6. `Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs`
7. `Game/Procedural/Masses/MassGenerator.EdgeWear.Orchestration.cs`
8. `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`

**Create/delete/move/rename:** none.

##### Frozen owners and non-goals

- Do not modify `MassGenerator.Polyhedron.cs`, `MassGenerator.EdgeWear.BoundedSingleEdge.cs`, `MassGenerator.EdgeWear.PlaneCutKernel.cs`, `MassGenerator.MeshOutput.cs`, `MassGenerator.cs`, `GeneratedMass.cs`, `GeneratedMassEditor.cs`, shaders, assets, scenes, prefabs, recipes, mesh channels, serialized controls, layers, tags, or metadata.
- Do not alter cut geometry, cap semantic provenance, stable output-edge identities, cap-ring width, Macro bypass, mandatory selection, coverage, locality, isolated viability, corner/coexistence, recovery, topology, one-surface triangulation, final-normal selection, or the `0.5` render-normal guard.
- Do not use the construction clone as transaction/report authority. `AcceptedFaces` and `AcceptedCapFace` remain the semantic evidence.

##### Required construction attribution contract

For every accepted semantic face `AcceptedFaces[i]`, create a new `PolygonFace` with:

```text
vertices = exact copied vertex list
normal = semantic face normal
feature = semantic face feature
featureStrength = semantic face feature strength
provenanceKind = SourceFace
provenanceIndex = i
```

The clone must preserve face order and geometry exactly. It must contain `N` unique dense source indices for `N` construction faces. The original semantic list and its `CornerDamageCap` record remain unchanged.

##### File-by-file implementation sequence

1. [x] Extend `CornerDamageTransactionAuditResult` with the accepted construction face list and dense construction-provenance counts.
2. [x] In `TryCommitCornerDamageTransactionResult`, build and certify the exact construction clone after semantic geometry and identity certification. Reject the transaction if the clone is null, count-mismatched, non-dense, or geometrically different.
3. [x] In `ApplyGeneratedEdgeWearBevels`, require the construction list and use it for bounds, candidate construction, and all subsequent corner-preview shell work. Preserve semantic `AcceptedFaces` for report/acceptance evidence.
4. [x] Extend preview capture/report telemetry with `constructionSourceProvenance=attributed/expected` and `semanticCapFaces` without changing the existing report tuples.
5. [x] Update framework, recovery architecture, and code inventory with the semantic-versus-construction ownership boundary.
6. [x] Reread every modified file and affected producer/consumer, compare against the untouched EW-C1A.2 baseline, run exact-scope, delimiter, preprocessor, reference, line-ending, whitespace, frozen-owner, package-overlay, and patch-reproduction checks, and record Unity validation as pending.

##### Acceptance criteria

- [x] Exactly the eight approved files differ; no file is created, deleted, moved, or renamed.
- [x] The semantic accepted face list remains separate, is never mutated by the adapter, and retains the previously certified one-cap seed-8889 transaction evidence.
- [x] The construction helper enforces `constructionSourceProvenance=N/N`, dense unique indices `0..N-1`, copied vertex order/values, matching normal direction, feature, and feature strength.
- [x] Candidate construction consumes only the construction list; transaction/report evidence continues to consume the semantic list.
- [x] Frozen clipping, bounded-bevel provenance validation, triangulation, final normals, shaders, editor controls, and production generation remain byte-identical.
- [x] Available source/static/package validation passes. Unity compile and runtime preview remain pending until the user applies the patch.
- [ ] The next seed-8889 report advances past `mandatoryCapRing=3/0/0/0`; expected first evidence is `constructionSourceProvenance=N/N`, `semanticCapFaces=1`, and non-zero cap-ring candidate/construction counts. Any later genuine geometry blocker must be reported at its actual stage.

##### Static completion and post-change compliance evidence

- Exact actual scope: the approved `8` modified files; `0` created, deleted, moved, or renamed files; all `331` source files preserved.
- Source validation passed `91/91` combined checks. All `195` C# files have balanced delimiters and preprocessor blocks; new definitions/callers resolve exactly once where required; existing C1A.1 identity and C1A.2 preview report tuples remain present; no serialized field, using directive, conflict marker, trailing whitespace, or line-ending regression was introduced.
- The semantic list is assigned only from the certified trial. The construction adapter allocates a separate face list and separate vertex lists, overwrites provenance only on the clones, enforces dense `SourceFace` indices, and compares copied vertices, normal direction, feature, and feature strength before commit.
- `ApplyGeneratedEdgeWearBevels` consumes `AcceptedConstructionFaces` only inside the scoped corner-preview branch and no longer assigns `AcceptedFaces` to the bevel pipeline. `CompleteCornerDamagePreviewCapture` requires complete construction attribution while preserving semantic cap evidence.
- Frozen owner hashes remain byte-identical for `MassGenerator.Polyhedron.cs`, `MassGenerator.EdgeWear.BoundedSingleEdge.cs`, `MassGenerator.EdgeWear.PlaneCutKernel.cs`, `MassGenerator.MeshOutput.cs`, `MassGenerator.cs`, `GeneratedMass.cs`, `GeneratedMassEditor.cs`, and `MassSurfaceFeatureGenerator.cs`.
- The eight-entry changed-files ZIP overlaid the untouched baseline and reproduced all `331` final files byte-for-byte. The unified patch passed `git apply --check`, applied to the untouched baseline, and reproduced all `331` final files byte-for-byte. ZIP CRC validation passed.
- Unity 6000.5.0f1 compilation, seed-8889 preview execution, visual review, and the complete one-click regression suite are unavailable here and remain pending.

##### Performance contract

- One additional `O(F + V)` face/vertex clone is created only during the explicit editor corner preview after a transaction is certified.
- No active-gameplay, per-frame, shader, GPU, texture, buffer, persistent cache, or serialized-memory cost is added.
- No performance exception is required.

#### EW-C1A.2b — Corner-chip authoring and visual acceptance

**Patch identifier:** `EW-C1A.2b`

**Status:** [implemented; static validation passed; Unity validation pending]

**Goal:** make the already-certified single-corner chip visibly authoritative and controllable while preserving the accepted pre-bevel construction order. Corner chipping remains editor-preview-only in this patch. The canonical base polyhedron is cut first; edge-wear candidate discovery and bevel construction then operate on the chipped topology, including the three new mandatory cap-ring edges.

##### Read-only evidence reviewed before implementation

- Repository state: the reconstructed accepted EW-C1A.2a tree contains `331` files and no `.git` directory. Branch, `HEAD`, status, remote, and history comparisons are unavailable. `/mnt/data/c1a2b_work/baseline` is the untouched pre-edit baseline; `/mnt/data/c1a2b_work/current` is the implementation tree.
- Unity evidence supplied by the user reports `status=passed`, `transactionCertified=1`, `semanticCapFaces=1`, `constructionSourceProvenance=17/17`, `mandatoryCapRing=3/3/3/3`, `ordinaryRetention=31/28/28/0`, and no collateral loss for seed `8889`. The structural C1A.2/C1A.2a contracts are therefore working. The user screenshots show that the default cut is too subtle to identify reliably and that the cap ring appears disconnected from current edge-wear authoring.
- `MassGenerator.EdgeWear.SelectionAndCorners.cs::EvaluateCornerDamageTransaction` currently resolves depth from a fixed deterministic `0.08..0.16` fraction and clamps every trial to `0.18` of the shortest incident edge. `BuildCornerDamageCandidate` uses fixed selection weights `0.55/0.25/0.15/0.05` for sharpness/size/upward/random.
- `ResolveCornerDamageCapRingRequestedWidth` currently hard-codes `0.50 * ordinaryRequestedWidth`, `0.25 * acceptedDepth`, and `0.20 * shortestCapEdgeLength`. `BuildEdgeWearBevelCandidates` gives cap-ring candidates ordinary edge-wear amount but no authorable ring-strength multiplier.
- `MassSurfaceFeatureSettings` carries the current edge-wear settings into all generator paths. `GeneratedMass.CreateSurfaceFeatureSettings` is the authoritative live-Inspector snapshot owner. Existing explicit preview rebuilds already mark stale on `OnValidate` and rebuild from the current snapshot.
- `GeneratedMassEditor.DrawEdgeWearFeature` owns the visible edge-wear controls. `DrawEdgeWearBevelPreview` owns the corner preview action/report. `OnSceneGUI` currently draws render-audit and river-pressure overlays but no corner-chip marker.
- `MassGenerator.GenerateInternal` already transforms edge-debug positions through dimensions and the immutable placement frame before exposing them. The corner marker requires the same transformation ownership; no `MassGenerator.MeshOutput.cs` change is justified.
- Reviewed complete current owners and direct producers/consumers: `AGENTS.md`; all four canonical Generated Mass documents; `MassSurfaceFeatureGenerator.cs`; `GeneratedMass.cs`; `MassGenerator.cs`; `MassGenerator.EdgeWear.Types.cs`; `MassGenerator.EdgeWear.Orchestration.cs`; `MassGenerator.EdgeWear.SelectionAndCorners.cs`; `MassGenerator.EdgeWear.Diagnostics.Logging.cs`; `GeneratedMassEditor.cs`; and the unchanged clipping, bounded-bevel, plane-cut, triangulation, placement, mesh-output, recipe-baker, and shader consumers relevant to the new fields.

##### Approved controls and defaults

The serialized `Corner Chipping` group is preview-only in EW-C1A.2b:

```text
Enable Corner Chipping      bool          default Off
Corner Chip Depth           0.04..0.35    default 0.18
Corner Chip Depth Variation 0..0.50       default 0.15
Top-Facing Preference       0..1          default 0.65
Cap-Ring Width Scale        0.20..1.25    default 0.75
Cap-Ring Wear Strength      0..1.50       default 1.00
```

- `Corner Chip Depth` is the requested fraction of the selected corner's shortest incident edge.
- `Corner Chip Depth Variation` applies deterministic symmetric multiplicative variation around the requested fraction. Zero resolves exactly to the requested value.
- `Top-Facing Preference` controls the selection-score weight assigned to upward exposure. At `0.5`, the prior `0.55/0.25/0.15/0.05` score is reproduced exactly; remaining weight is redistributed in the established sharpness/size/random proportions.
- `Cap-Ring Width Scale` multiplies the current ordinary generated edge-wear width before the existing depth and shortest-cap-edge safety ceilings.
- `Cap-Ring Wear Strength` multiplies current edge-wear material strength only on the mandatory cap ring. It does not change ring geometry.

##### Approved file scope

**Modify:**

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Game/Procedural/Masses/MassSurfaceFeatureGenerator.cs`
6. `Game/Procedural/Masses/GeneratedMass.cs`
7. `Game/Procedural/Masses/MassGenerator.cs`
8. `Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
9. `Game/Procedural/Masses/MassGenerator.EdgeWear.Orchestration.cs`
10. `Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs`
11. `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
12. `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

**Create/delete/move/rename:** none.

##### Frozen owners and non-goals

- Do not change production `EdgeWearEvaluationMode.None`; enabled corner controls are consumed only by the explicit EW-C1A.2b preview in this patch.
- Do not modify `MassGenerator.Polyhedron.cs`, `MassGenerator.EdgeWear.BoundedSingleEdge.cs`, `MassGenerator.EdgeWear.PlaneCutKernel.cs`, `MassGenerator.MeshOutput.cs`, `MeshBuilder.cs`, shaders, materials, scenes, prefabs, recipe assets, mesh channels, layers, tags, or metadata.
- Preserve the certified transaction, semantic/construction provenance split, stable descendant/cap-ring identities, mandatory all-or-nothing cap ring, ordinary coverage semantics, Macro bypass on cap-ring candidates, unrelated bevel retention, one-polygon/one-surface triangulation, shared-normal resolver, and exact `0.5` render-normal guard.
- Do not add chip count, multiple chips, jagged cap geometry, cap displacement, arbitrary edge notches, secondary fractures, cap-ring Macro variation, or production promotion.
- Do not add per-frame generation, shader work, buffers, textures, or persistent runtime caches.

##### File-by-file implementation sequence

1. [x] Complete and record the read-only source, caller/consumer, documentation, runtime-evidence, scope, and performance review.
2. [x] Record this persistent plan as the first repository change.
3. [x] Add the six clamped corner-authoring fields to `MassSurfaceFeatureSettings` while preserving old constructor callers through trailing optional parameters.
4. [x] Add serialized fields, properties, feature-recipe/default ownership, recipe matching/reset behavior, and current snapshot transport in `GeneratedMass.cs`.
5. [x] Parameterize corner selection, requested depth, deterministic depth variation, trial ceiling, cap-ring width scale, and cap-ring wear strength without changing eligibility, clipping, retry, or mandatory-ring rules.
6. [x] Extend transaction/preview capture with requested/resolved authoring evidence, cap-ring limit evidence, selected corner, semantic cap vertices, and transformed local Scene-marker positions.
7. [x] Transform marker positions through the same dimensions and immutable placement frame used by the generated preview mesh.
8. [x] Expose the Corner Chipping controls in `GeneratedMassEditor`, disable the rebuild action when authoring is off, and draw the selected original corner, cap outline, ring edges, centre line, and `Corner Chip` label while the preview is current.
9. [x] Advance report/Inspector contracts to EW-C1A.2b and print the exact consumed live settings, requested/resolved/accepted depth, all width limits, winning limit, ring strength, selected local position, and cap edge lengths.
10. [x] Reconcile framework, recovery architecture, and code inventory with the accepted authoring and Scene-marker ownership.
11. [x] Reread every modified file and affected unchanged producer/consumer; compare the final tree with the untouched EW-C1A.2a baseline; run exact-scope, C# delimiter/preprocessor/reference, constructor-call, serialized-property, frozen-owner, line-ending, whitespace, package-overlay, patch-application, and artifact-integrity checks.
12. [x] Unity compile and visual/runtime validation remain pending user execution.

##### Acceptance criteria

- Exactly the twelve approved files differ; no file is created, deleted, moved, or renamed.
- Existing serialized masses retain corner chipping disabled by default. Existing `MassSurfaceFeatureSettings` callers compile unchanged through optional trailing defaults.
- Pressing the corner-preview rebuild consumes the current live Inspector values. Any edit marks the existing preview stale.
- Disabled corner chipping cannot silently generate a chip; the preview reports the disabled authoring state and returns production geometry.
- With depth variation `0`, resolved depth fraction equals requested depth exactly before bounded trial fallback. The accepted depth and accepted trial remain explicitly reported.
- The transaction still occurs before candidate discovery. Shortened original edges use normal edge-wear settings. New cap-ring edges use current ordinary width times `Cap-Ring Width Scale`, then the existing depth and cap-edge safety ceilings; all three remain mandatory and Macro-free.
- Cap-ring wear strength uses current edge-wear amount times the authoring multiplier and remains clamped to the existing feature-strength contract.
- Scene marker positions match the final preview mesh local space after dimensions, lean, grounding, and recenter placement.
- Report telemetry identifies the exact selected corner and makes every clamp visible. No hidden cached settings survive a rebuild.
- All accepted C1A.2a structural counters remain valid; no unrelated baseline bevel is lost.
- Frozen clipping, bounded shell, triangulation, final-normal, shader, material, production, and `0.5` guard owners remain byte-identical.

##### Static/package validation outcome

- Exact scope: the approved twelve files differ from the untouched EW-C1A.2a baseline; no file was created, deleted, moved, or renamed.
- All `195` C# files passed delimiter and preprocessor-balance scans. All five existing `MassSurfaceFeatureSettings` construction sites remain compatible: three retain the original twelve arguments and two explicitly pass all eighteen values.
- Serialized-field/property/recipe/default/snapshot wiring, disabled-preview production fallback, transaction-before-candidate order, post-chip construction-face use, depth/selection/width/strength formulas, final-space marker transformation, report fields, and Scene overlay ownership passed targeted source checks.
- Modified files preserve CRLF line endings, final newlines, and clean trailing-whitespace/conflict-marker checks. Frozen clipping, bounded-shell, plane-cut, mesh-output, mesh-builder, shader, material, scene, prefab, and asset owners remain byte-identical.
- The twelve-entry changed-files ZIP passed CRC testing and overlaid onto the untouched baseline to reproduce all `331` final files byte-for-byte. The unified patch passed `git apply --check`, applied to the untouched baseline, and reproduced all `331` final files byte-for-byte.
- Unity 6000.5.0f1 compilation, seed-8889 authoring/marker validation, visual before/after acceptance, control-response checks, and the complete one-click regression suite are unavailable here and remain pending.

##### Performance contract

- The six serialized scalars add negligible component storage. No new runtime buffer, texture, mesh channel, or shader input is added.
- Selection/depth arithmetic is constant-time per eligible corner. Scene handles draw only for one selected current editor preview.
- The existing explicit preview remains dirty/editor-time work: one baseline generation plus one corner generation. No active-gameplay or per-frame generation path is added.
- No performance exception is required.

#### EW-C1A.2c — Split chip-geometry and edge-wear integration previews

**Patch identifier:** `EW-C1A.2c`

**Status:** [implemented; static/compliance/package validation passed; Unity validation pending]

**Goal:** stop the primary corner-chip authoring preview immediately after the certified corner cut so the removed material and silhouette can be judged without ordinary or cap-ring bevel geometry. Preserve the current combined corner-chip plus edge-wear path as a separately named integration/certification preview that rebuilds from the current live Inspector settings.

##### Read-only evidence reviewed before implementation

- Repository state: the accepted EW-C1A.2b source contains `331` files and no supplied Git metadata. `/mnt/data/c1a2c_work/baseline` is the untouched accepted baseline. `/mnt/data/c1a2c_work/current` is a local review/implementation copy with a synthetic baseline commit used only for exact diff and patch verification.
- User Unity evidence for seed `8889` reports `status=passed`, one certified semantic cap, `constructionSourceProvenance=17/17`, mandatory cap ring `3/3/3/3`, ordinary retention `28/28`, and no collateral loss. It also reports `cornerChipDepthResolved=0.20`, `acceptedTrial=2`, and `acceptedDepth=0.0671515092`; the combined preview visibly presents bevel changes while the chip itself is not identifiable.
- `MassGenerator.GenerateCornerDamagePreview` currently runs one ordinary unified-bevel baseline and one corner-damaged unified-bevel generation. It therefore always compares production geometry with a chipped mesh containing all selected ordinary bevels plus the mandatory cap ring.
- `MassGenerator.ApplyGeneratedEdgeWearBevels` already commits the corner transaction before candidate discovery. `CornerDamageTransactionAuditResult.AcceptedFaces` is the exact semantic chipped polyhedron; `AcceptedConstructionFaces` is the dense `SourceFace` clone used only by bevel construction.
- `TryTriangulateBoundedPreviewFaces` already accepts a list of convex polygon faces, preserves one polygon/one authored surface, emits no synthetic fan vertex, and does not require bevel faces. It can triangulate `AcceptedFaces` directly without modifying the frozen triangulator.
- `GeneratedMass` currently owns one nonserialized corner-preview state and one `PreviewGenerationMode.CornerDamage`; `GeneratedMassEditor` exposes one ambiguous rebuild button and one report file. `OnSceneGUI` already draws the selected corner and cap from final transformed marker positions.
- The current transaction report exposes requested/resolved fractions and accepted absolute depth but does not expose shortest incident edge length, requested absolute depth, accepted fraction, retry factor, or accepted/requested ratio.
- Reviewed complete current owners and direct producers/consumers: `AGENTS.md`; all four canonical Generated Mass documents; `MassGenerator.cs`; `MassGenerator.PlaneCut.cs`; `MassGenerator.EdgeWear.Types.cs`; `MassGenerator.EdgeWear.Orchestration.cs`; `MassGenerator.EdgeWear.SelectionAndCorners.cs`; `MassGenerator.EdgeWear.Diagnostics.Logging.cs`; `GeneratedMass.cs`; `GeneratedMassEditor.cs`; and the unchanged clipping, construction-provenance, bounded triangulation, placement, mesh-output, settings transport, feature-atlas, material, and shader consumers.

##### Approved file scope

**Modify:**

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Game/Procedural/Masses/MassGenerator.cs`
6. `Game/Procedural/Masses/MassGenerator.PlaneCut.cs`
7. `Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
8. `Game/Procedural/Masses/MassGenerator.EdgeWear.Orchestration.cs`
9. `Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs`
10. `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
11. `Game/Procedural/Masses/GeneratedMass.cs`
12. `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

**Create/delete/move/rename:** none.

##### Frozen owners and non-goals

- Do not modify corner clipping, retry factors, candidate eligibility/scoring, construction provenance, bevel candidate construction, bounded shell construction, one-surface triangulation, final normal resolution, the `0.5` guard, shaders, materials, settings defaults, scenes, prefabs, assets, mesh channels, layers, tags, or metadata.
- Do not change production `EdgeWearEvaluationMode.None` or promote corner chipping into production generation.
- Do not remove the accepted combined integration path or weaken its mandatory ring and unrelated-bevel retention gates.
- Do not add chip count, jaggedness, multiple chips, edge notches, secondary fractures, per-frame work, buffers, textures, or runtime caches.

##### File-by-file implementation sequence

1. [x] Complete and record the read-only source, caller/consumer, runtime-evidence, scope, and performance review.
2. [x] Record this persistent plan as the first repository change.
3. [x] Add explicit generator evaluation modes/APIs for `Corner Chip Geometry Preview` and `Corner Chip + Edge Wear Integration Preview`; retain the old API as a compatibility alias to integration.
4. [x] Route the geometry-only mode through the certified semantic `AcceptedFaces` and the existing one-surface triangulator, then stop before candidate discovery.
5. [x] Preserve the integration mode's current baseline comparison, live settings, mandatory cap ring, construction provenance, and ordinary retention logic.
6. [x] Extend transaction/status capture with shortest incident edge length, requested absolute depth, accepted depth fraction, accepted retry factor, and accepted/requested ratio.
7. [x] Split `GeneratedMass` editor-only preview mode/state into mutually exclusive geometry and integration previews, with distinct mesh suffixes, stale flags, status/report accessors, and compatibility aliases.
8. [x] Replace the ambiguous Inspector action with two explicit buttons, reports, summaries, and Scene labels: `Corner Chip — Geometry` and `Corner Chip — With Edge Wear`.
9. [x] Reconcile framework, recovery architecture, and code inventory with the split preview ownership and unchanged production order.
10. [x] Reread every modified file and affected unchanged producer/consumer; compare final behavior and exact scope with the untouched EW-C1A.2b baseline; run C# delimiter/preprocessor/reference checks, frozen-owner verification, whitespace/line-ending checks, package-overlay reproduction, patch application, and artifact integrity checks.
11. [x] Unity 6000.5.0f1 compile, seed-8889 geometry-only visual acceptance, integration regression, and complete one-click edge-wear suite remain pending user execution.

##### Static/compliance completion evidence

- Exact scope comparison passed: the same `331` source files remain present, exactly the twelve approved files differ, and no file was created, deleted, moved, or renamed.
- Targeted source/contract validation passed `117/117`; all `195` C# files passed delimiter, preprocessor, and region-balance checks; `git diff --check` passed.
- Every changed file retains CRLF line endings, a final newline, and no trailing whitespace.
- Frozen clipping, construction, bounded-triangulation, mesh-output/final-normal, surface-settings transport, and mesh-builder owners remain byte-identical to EW-C1A.2b.
- The changed-files overlay and unified patch each reproduce the complete final `331`-file tree byte-for-byte; archive integrity and path-safety checks pass.
- Unity compilation and runtime/visual acceptance are unavailable in this environment and are not claimed.

##### Acceptance criteria

- The geometry-only preview contains the certified chipped polyhedron and no ordinary bevel faces, cap-ring bevel faces, edge-wear candidate selection, Macro variation, coverage selection, or ordinary-retention baseline generation.
- The integration preview continues to build from the current live Inspector snapshot and retains the accepted `3/3/3/3` mandatory ring and unrelated-bevel retention contract.
- Both preview modes are explicit, mutually exclusive, independently stale, independently reported, and return production geometry on failure.
- The Scene marker remains final-placement accurate and labels the active preview mode.
- Reports expose `shortestIncidentEdgeLength`, `requestedDepthAbsolute`, `acceptedDepthAbsolute`, `acceptedDepthFraction`, `acceptedRetryFactor`, and `acceptedVsRequestedRatio`. A visibly reduced accepted cut is no longer hidden by fraction/absolute-unit ambiguity.
- The old `GenerateCornerDamagePreview` and `EvaluateCornerDamagePreview` entry points remain source-compatible aliases to the integration preview.
- Exactly the twelve approved files differ; no file is created, deleted, moved, or renamed.
- Frozen clipping, construction provenance, bevel construction, bounded triangulation, final normals, shaders, settings/defaults, production generation, and render guard owners remain byte-identical.

##### Performance contract

- Geometry-only preview performs one corner transaction and one polygon triangulation. It removes the current ordinary unified-bevel baseline and combined bevel construction from the primary authoring action.
- Integration preview retains the existing explicit editor-only two-generation cost.
- No active-gameplay, per-frame, shader, GPU, persistent-memory, texture, buffer, or cache cost is added. No performance exception is required.

#### EW-C1A.3 — Single-chip generalization and Inspector cleanup

**Patch identifier:** `EW-C1A.3`

**Status:** [implemented; Unity validation pending]

**Goal:** combine the minimal C1A.2d authoring cleanup with the 33-case single-chip acceptance gate. Corner Chipping is one normal authoring group. `Rebuild Corner Chip Preview` shows the raw certified cut using those controls. The existing `Rebuild Edge-Wear Bevel Preview` automatically applies corner chipping first whenever it is enabled, then runs the ordinary current-settings bevel pipeline. The separate corner-plus-edge-wear button, preview-specific Inspector explanations, duplicate report controls, and user-facing preview-only terminology are removed. The existing one-click edge-wear suite gains the C1A.3 matrix; no new validation button is added.

##### Read-only evidence reviewed before implementation

- The authoritative current tree was reconstructed from `Assets-Code-Archive(11).zip` plus the accepted EW-C1A.2, EW-C1A.2a, EW-C1A.2b, and EW-C1A.2c overlays in chronological order. It contains `331` files and no `.git` directory, so branch, `HEAD`, status, history, and repository-diff evidence are unavailable.
- User Unity evidence passed both C1A.2c paths for seed `8889`: geometry-only `status=passed`, one cap, `ordinaryBevelCandidates=0`, `bevelFaces=0`; integration `status=passed`, mandatory ring `3/3/3/3`, unrelated retention `28/28`, and no collateral loss.
- `GeneratedMassEditor.DrawEdgeWearFeature` exposes the six serialized controls under `Corner Chipping (Preview-Only)`. `DrawEdgeWearBevelPreview` adds two corner-specific buttons, two report-control groups, and separate geometry/integration status text. This is the confirmed Inspector clutter to remove.
- `GeneratedMass.EvaluateUnifiedEdgeWearPreview` always selects `PreviewGenerationMode.UnifiedEdgeWear`, while the corner-integrated path is isolated behind `EvaluateCornerDamageIntegrationPreview`. Therefore the normal bevel preview does not yet consume enabled corner controls.
- `MassGenerator.GenerateCornerDamageIntegrationPreview` already performs the required order: frozen unified baseline, certified pre-bevel corner cut, candidate discovery on damaged topology, mandatory cap ring, and unrelated-retention proof. `MassGenerator.GenerateUnifiedEdgeWearPreview` owns the ordinary preview entry point and can route to that integration path when Corner Chipping is enabled without changing production `EdgeWearEvaluationMode.None`.
- `GeneratedMassEditor` already owns the accepted 11-seed array and the asynchronous one-click suite. The existing suite has current-preview, topology, and artistic-parity stages. C1A.3 can add one internal asynchronous 33-case stage without adding Inspector controls.
- Complete current owners and direct callers/consumers were reviewed: `AGENTS.md`; the four canonical Generated Mass documents; `MassGenerator.cs`; `MassGenerator.EdgeWear.Diagnostics.Logging.cs`; `GeneratedMass.cs`; `GeneratedMassEditor.cs`; `MassSurfaceFeatureGenerator.cs`; `MassGenerator.EdgeWear.Types.cs`; `MassGenerator.EdgeWear.Orchestration.cs`; `MassGenerator.EdgeWear.SelectionAndCorners.cs`; `MassGenerator.PlaneCut.cs`; `MassGenerator.MeshOutput.cs`; `MeshData.cs`; and `MeshBuilder.cs`.

##### Approved file scope

**Modify:**

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Game/Procedural/Masses/MassGenerator.cs`
6. `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
7. `Game/Procedural/Masses/GeneratedMass.cs`
8. `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

No file may be created, deleted, moved, or renamed. `MassSurfaceFeatureGenerator.cs`, clipping, candidate construction, bounded shell construction, triangulation, final normals, shaders, materials, assets, mesh channels, and production generation remain unchanged.

##### Inspector and preview contract

1. [x] Rename the visible group to `Corner Chipping`; remove preview-only language from serialized tooltips and Inspector copy.
2. [x] Keep exactly the existing six controls and defaults. Add no control, foldout, preview selector, or debug setting.
3. [x] Keep one action named `Rebuild Corner Chip Preview`; it runs the raw geometry-only cut and automatically writes/copies its report without separate report buttons.
4. [x] Remove the separate corner-plus-edge-wear integration button and duplicate corner report UI. Preserve internal compatibility APIs only where they prevent unrelated caller breakage.
5. [x] Make `Rebuild Edge-Wear Bevel Preview` use the current Corner Chipping settings automatically: disabled uses the frozen ordinary unified path; enabled uses the certified corner-first integration path and stores the corner status for the existing Scene marker.
6. [x] Use one Scene label, `Corner Chip`, for either the raw chip preview or the normal edge-wear preview with corner chipping enabled.
7. [x] Remove the standalone EW-C1A.1 transaction-audit controls from the Inspector. The transaction remains covered by the corner preview and the one-click suite.

##### C1A.3 one-click matrix

The existing one-click suite gains one asynchronous internal stage over the accepted 11 seeds and three policies:

```text
Disabled         Corner Chipping off; exact parity against the frozen ordinary unified preview
Default          enabled; depth 0.18, variation 0.15, current top/ring controls
Maximum Depth    enabled; depth 0.35, variation 0, current top/ring controls
```

Total: `33` cases. No matrix button is added.

For disabled cases, the corner-aware unified entry point must be byte-for-byte equivalent at `MeshData` channel level to the frozen ordinary unified entry point and must select no corner. For enabled cases, one geometry-only run and one integrated run must independently select the same corner/trial/depth/ring identities, certify one cap, preserve dense construction provenance, build the complete mandatory ring, retain every unrelated ordinary bevel, and produce valid final channels. Every final matrix mesh is applied to a temporary editor mesh to verify normal and recalculated tangent channel count, finiteness, and non-degenerate tangent direction.

##### File-by-file implementation sequence

1. [x] Record this persistent plan as the first write.
2. [x] Add the corner-aware unified preview entry point and a frozen ordinary-baseline entry point in `MassGenerator.cs`; expose construction-provenance counts already held by the transaction status.
3. [x] Route `GeneratedMass` unified preview through the corner-aware entry point, retain one raw corner preview, and collapse integration compatibility onto the normal unified preview.
4. [x] Remove duplicate Inspector workflows and preview-only wording; retain one raw chip action and one normal edge-wear action.
5. [x] Add the internal asynchronous 33-case C1A.3 suite stage, deterministic case/result aggregation, exact disabled parity checks, cap/ring/retention checks, and normal/tangent validation.
6. [x] Extend the existing combined suite report and summary with C1A.3 status, `33/33` count, zero-parity, selection determinism, transaction, cap-ring, retention, channel, cancellation, and terminal evidence.
7. [x] Update framework, recovery architecture, and code inventory to remove the superseded split-workflow authoring contract and record the accepted normal workflow.
8. [x] Reread every modified file and all affected owners; verify exact scope, compatibility aliases, preprocessor boundaries, C# structure, frozen-owner hashes, report completeness, ZIP overlay reproduction, and patch reproduction. Mark Unity compilation/runtime validation pending.

##### Acceptance criteria

- [x] Exactly the eight approved files differ; `0` files are created, deleted, moved, or renamed.
- [x] The Inspector shows `Corner Chipping`, the same six controls, one `Rebuild Corner Chip Preview` action, and no separate integration action or corner report controls.
- [x] `Rebuild Edge-Wear Bevel Preview` consumes the current corner controls when enabled and remains exact ordinary behavior when disabled.
- [x] The one-click report contains `cornerChippingStatus`, `cornerChippingCases=33/33`, exact disabled parity `11/11`, selection determinism `33/33`, transaction/cap/ring/retention/channel results, `cancelled=0`, and `terminalReason=none` when accepted.
- [x] No clipping, bevel solver, triangulation, final-normal, shader, serialized default, mesh-channel, production, or per-frame behavior changes.
- [x] Available static/package validation passes. Unity 6000.5.0f1 compilation and the one-click runtime matrix remain pending until the user applies the patch.

##### Implementation result

- Exact source scope: eight modified files; no create/delete/move/rename.
- Inspector: one `Corner Chipping` group with the existing six controls, one raw chip action, one ordinary edge-wear action, and no separate integration/report/audit controls.
- Routing: the ordinary unified preview is byte-identical to the frozen path when corner chipping is disabled and uses the certified pre-bevel integration path when enabled.
- Suite: asynchronous 11-seed × 3-policy C1A.3 stage added to the existing one-click workflow; no new button.
- Static source-contract checks: `53/53` passed.
- C# delimiter and preprocessor checks: `195/195` files passed.
- Changed-files ZIP overlay and unified patch each reproduced the complete `331/331`-file final tree byte-for-byte.
- Unity 6000.5.0f1 compilation, Inspector rendering, and runtime `33/33` acceptance remain pending.

### Performance contract

- The normal edge-wear preview keeps the existing explicit editor-only cost. When Corner Chipping is enabled it performs the already accepted retention baseline plus one post-chip unified construction; disabled behavior remains the single frozen unified construction.
- The 33-case C1A.3 matrix is asynchronous editor validation. Disabled cases perform two parity generations; enabled cases perform one raw transaction generation plus the existing two-generation integrated proof. It adds no recurring update and no gameplay path.
- Candidate adjacency and scoring remain `O(V + E)` over the small normalized source graph. At most one selected corner runs at most four cloned half-space cut trials. Cap-ring work adds only the cap polygon edge count to ordinary candidates.
- Temporary construction memory remains bounded to dirty/editor evaluation. The matrix holds only case summaries and temporary per-case meshes; every temporary Unity mesh is destroyed before the next case.
- Active gameplay, per-frame CPU, GPU shader work, textures, buffers, persistent caches, serialized defaults, and production generation do not change. No performance exception is required.

### Audit exit result

- [x] Exact pre-bevel insertion point identified.
- [x] Existing clipping, cap, welding, topology, volume, triangulation, normal, and tangent owners identified.
- [x] Final normals/tangents proven to occur after all geometry.
- [x] Pre-bevel cutting approved.
- [x] Raw post-bevel sharp-ring cut rejected.
- [x] Identity blocker identified and bounded by explicit descendant/cap-ring maps.
- [x] No new source file, shader, material, mesh channel, dependency, or per-frame path is required.
- [x] EW-C1A.1 implementation explicitly approved; implementation plan recorded; Unity transaction report certified trial `0` and was accepted.

## EW-C1A.1a — One polygon, one render surface

**Status:** [implemented; Unity validation pending]

### Objective

Remove visible internal triangulation boundaries from ordinary source and junction polygons before EW-C1A.2 commits corner-damage geometry. Every accepted `PolygonFace` remains triangulated for GPU output, but all triangles emitted from that polygon must share one authoritative polygon normal and one stable material surface-group identity.

### Read-only evidence reviewed before implementation

- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.BoundedSingleEdge.cs::TryTriangulateBoundedPreviewFaces` currently special-cases only `BoundedEdgeBevel` and `EdgeBevelPlane`. Every other polygon is emitted as one centre-fan triangle per real boundary segment using `CalculateAverage(convexityLoop)` and `AddOrientedTriangle`. A quadrilateral therefore emits four triangles meeting at an inserted centre vertex.
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.BoundedSingleEdge.cs::TryTriangulateBoundedBevelRegionFace` already proves the required solution for bevel polygons: stable boundary-anchor triangulation, `boundaryVertexCount - 2` triangles, one `PolygonFace.Normal`, and one authored surface group.
- `Assets/Game/Procedural/Masses/MassGenerator.Types.cs::TriangleSoup` already stores authored normals and authored surface groups per emitted triangle vertex. No shared type or mesh-channel extension is required.
- `Assets/Game/Procedural/Masses/MassGenerator.MeshOutput.cs::BuildMeshData` already consumes those authored channels. Without an authored normal it calculates a triangle normal; without an authored group it hashes surface variation from the duplicated triangle-soup vertex index. This is the direct cause of visible X/radial boundaries across one polygon.
- Direct callers reviewed: `MassGenerator.EdgeWear.BoundedSingleEdge.cs`, `MassGenerator.EdgeWear.BoundedAllEdges.cs`, and `MassGenerator.EdgeWear.PlaneCutKernel.cs`. All route final polygon shells through `TryTriangulateBoundedPreviewFaces`.
- Diagnostic consumers reviewed: `MassGenerator.EdgeWear.Diagnostics.Logging.cs` and the existing one-click EW-V1A.3b suite. The suite already exercises all callers across the accepted 33/33 topology and preview matrices.
- Historical evidence reviewed: the accepted `EW-B1.7 — One planar bevel surface render contract` solved this exact ownership defect for bevel polygons only. EW-C1A.1a extends the same contract to every accepted polygon; it does not alter geometry selection, clipping, or bevel construction.
- Repository comparison limitation: the supplied archive contains no `.git` metadata, so `git status`, `HEAD`, commit history, and repository diffs are unavailable. The authoritative current source was reconstructed from `Assets-Code-Archive(6).zip`, EW-V1A.3b, the freeze docs, EW-C1A-RO2, and EW-C1A.1 in accepted chronological order.

### Approved file scope

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.BoundedSingleEdge.cs`
6. `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.PlaneCutKernel.cs`
7. `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`

No other file operation is approved. `MassGenerator.Types.cs` and `MassGenerator.MeshOutput.cs` are reviewed consumers but require no change because their authored-normal and authored-group contracts already exist and are correct.

### Required triangulation and ownership contract

For an accepted convex polygon with ordered real boundary vertices `v[0..n-1]`, `n >= 3`:

1. Find a stable boundary anchor `a` that maximizes the minimum emitted triangle area.
2. Emit exactly `n - 2` triangles:

```text
(v[a], v[a+1], v[a+2])
(v[a], v[a+2], v[a+3])
...
(v[a], v[a+n-2], v[a+n-1])
```

Indices wrap modulo `n`. Each emitted triangle must have area greater than `max(TinyFaceAreaEpsilon, minimumStableFaceArea * 0.001)` and positive winding against the normalized `PolygonFace.Normal`.

All triangles from one polygon receive:

```text
authoredNormal = normalize(face.Normal)
authoredSurfaceGroup = stable non-negative key(face provenance kind, provenance index, fallback face index)
```

The group key must be identical inside one polygon and collision-free across distinct output polygons in the same shell. The fallback face index is used only when provenance is absent or negative.

### New mandatory audit fields

`BoundedSingleEdgeAuditResult` and `PlaneCutBevelAuditResult` will record:

```text
PolygonSurfaceFaceCount
PolygonSurfaceBoundaryVertexCount
PolygonSurfaceExpectedTriangleCount
PolygonSurfaceTriangleCount
PolygonSurfaceAuthoredNormalTriangleCount
PolygonSurfaceAuthoredSurfaceGroupTriangleCount
PolygonSurfaceInternalFanVertexCount
PolygonSurfaceGroupCollisionCount
PolygonSurfaceMaximumPlaneResidual
PolygonSurfaceMaximumNormalDeviationDegrees
PolygonSurfaceRenderValid
PolygonSurfaceFailureFace
PolygonSurfaceFailureProvenanceIndex
PolygonSurfaceFailureReason
```

`PolygonSurfaceRenderValid` requires:

```text
faceCount > 0
triangleCount == expectedTriangleCount
triangleCount == authoredNormalTriangleCount
triangleCount == authoredSurfaceGroupTriangleCount
internalFanVertexCount == 0
surfaceGroupCollisionCount == 0
no recorded failure
```

The existing bevel-only fields remain and must continue to pass unchanged.

### File-by-file implementation sequence

1. [x] Extend `BoundedSingleEdgeAuditResult` with polygon-surface ownership counters and failure evidence.
2. [x] Replace the ordinary-face centre-fan branch in `TryTriangulateBoundedPreviewFaces` with the same stable direct boundary triangulation contract used by bevel faces.
3. [x] Resolve one stable authored surface group per polygon and fail on any in-shell group collision.
4. [x] Preserve bevel-specific counters while making the general polygon-surface contract mandatory for the complete shell.
5. [x] Propagate the new counters through `PlaneCutBevelAuditResult` at every surface-audit copy/commit point.
6. [x] Add concise `polygonSurface=` evidence to current-preview, detailed, matrix, and outlier telemetry without changing Inspector workflow.
7. [x] Reconcile all four canonical documents with the implemented symbols and results.
8. [x] Complete full-source parsing, reference/import scans, scope/diff audit, static mathematical checks, package-overlay reproduction, and patch reproduction.

### Invariants and non-goals

- Preserve EW-V1A.3b candidate selection, width resolution, isolated recovery, conflict recovery, topology, and frozen acceptance behavior.
- Preserve the diagnostic-only EW-C1A.1 transaction and its accepted preview non-regression behavior.
- Do not merge distinct `PolygonFace` records. Real source-face, bevel-face, junction-face, and future cap-face boundaries remain.
- Do not change clipping, cap construction, source topology, edge identities, UV channels, shaders, materials, scenes, prefabs, recipes, serialized controls, or per-frame work.
- Do not alter `BuildMeshData`; it must simply receive complete authored data from triangulation.
- Do not promote corner damage into the visual preview in this patch.

### Risks and concrete controls

- **Degenerate direct fan:** a polygon with no stable boundary anchor fails with exact face/provenance evidence; no centre-fan fallback is permitted because it would reintroduce the defect.
- **Surface-group collision:** fail before mesh output and report both owner faces.
- **Incorrect winding:** orient each direct triangle against the authoritative polygon normal and require positive geometric-normal dot.
- **Non-planar accepted polygon:** enforce the existing one-plane tolerance for every polygon, not only bevel polygons.
- **Matrix regression:** the existing one-click suite remains authoritative; all old gates must stay green.

### Acceptance criteria

Static acceptance:

- all supplied C# files parse;
- only the seven approved files differ;
- no new namespace/import is unresolved;
- no ordinary centre-fan call remains in `TryTriangulateBoundedPreviewFaces`;
- every polygon path emits `boundaryVertexCount - 2` triangles with authored normal and group;
- all propagation and logging references resolve;
- changed-files overlay and unified patch reproduce the final tree byte-for-byte.

Unity acceptance:

- compile succeeds;
- existing EW-C1A.1 transaction remains certified;
- full EW-V1A.3b suite remains `status=passed`, topology `33/33`, preview `33/33`, outliers `5/5`, negative exclusion `1/1`;
- current preview reports `polygonSurface.renderValid=1`, zero internal fan vertices, zero group collisions, and all polygon triangles authored;
- the marked front-face X/radial internal boundaries disappear while genuine polygon boundaries remain.

### EW-C1A.1a implementation result

- [x] `TryTriangulateBoundedPreviewFaces` no longer emits ordinary centre fans. Every accepted polygon calls `TryTriangulateBoundedOneSurfaceFace`.
- [x] Each polygon emits exactly `boundaryVertexCount - 2` direct triangles from the most stable boundary anchor.
- [x] Every emitted triangle receives the normalized `PolygonFace.Normal` and one stable non-negative authored surface group.
- [x] Existing bevel surface-group values remain unchanged through the retained `0x4B1D0000` formula. Ordinary/source/junction/future-cap polygons use the separate `0x3A710000` domain.
- [x] In-shell surface-group collisions are fatal and record the group plus both face owners.
- [x] The complete-shell polygon-surface contract is mandatory in both baseline and topology-retry geometry gates.
- [x] `polygonSurface` telemetry records face/boundary/expected-triangle/actual-triangle counts, authored-channel counts, fan count, collisions, residuals, normal deviation, validity, and first failure.
- [x] `MassGenerator.Types.cs`, `MassGenerator.MeshOutput.cs`, Inspector controls, clipping, topology, shaders, materials, assets, and the diagnostic-only C1A.1 transaction remain unchanged.
- [x] Post-implementation static/compliance audit passed: exact seven-file scope; all 185 supplied C# files parsed without syntax errors; no using/import changed; 14,336 representative authored-group identities produced zero arithmetic collisions; all changed files retain CRLF and no trailing whitespace; the changed-files ZIP reproduced the complete 318-file reconstructed source tree byte-for-byte; the unified patch reproduced the complete Docs and Game trees byte-for-byte; ZIP CRC validation passed.
- [ ] Unity compile and visual/non-regression acceptance remain pending.

**Next step after acceptance:** EW-C1A.2 commits the already-certified pre-bevel corner transaction and integrates the new cap-ring edges into bevel selection.


## EW-C1A.1a.1 — Ordered-boundary render-normal agreement hotfix

**Status:** [implemented; Unity full-suite validation pending]

### Objective

Fix the single EW-C1A.1a matrix regression without weakening `MassGenerator.MeshOutput.cs::ValidateGeneratedMassMeshData`. Seed `6667`, maximum width, reached valid topology and failed only because triangle `0` had an authored polygon normal whose dot against the final geometric triangle normal was below the existing `0.5` render-normal agreement threshold.

### Read-only evidence reviewed before implementation

- Unity full-suite evidence: `Pasted text (2)(4).txt` reports `topologyCases=32/33` and the sole failure coordinate `seed=6667/width=maximum`, with `InvalidOperationException: Generated mass triangle 0 contains a render normal that disagrees with its winding.`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.BoundedSingleEdge.cs::TryTriangulateBoundedOneSurfaceFace` already swaps `b/c` when `dot(cross(b-a,c-a), authoredNormal) < 0`. The previously proposed swap-only correction therefore already exists and cannot explain or fix the remaining failure.
- `Assets/Game/Procedural/Masses/MassGenerator.MeshOutput.cs::BuildMeshData` also flips negative winding before output. `ValidateGeneratedMassMeshData` subsequently requires the normalized geometric normal to have a minimum dot of `0.5` against all stored render normals. The failure is therefore positive-but-insufficient normal agreement, not an uncorrected negative winding.
- Ordinary polygons newly began using `PolygonFace.Normal` as their authored render normal in EW-C1A.1a. Before that patch, ordinary triangles used their own geometric normals. Bevel polygons already used `PolygonFace.Normal` and remain accepted.
- `Assets/Game/Procedural/Masses/MassGenerator.Polyhedron.cs::CalculatePolygonNormal` proves the project already uses ordered-boundary Newell normals as the geometric normal of a polygon.
- Direct callers and consumers reviewed: `TryTriangulateBoundedPreviewFaces`, `TriangleSoup.AddTriangle`, `BuildMeshData`, and `ValidateGeneratedMassMeshData`.
- Repository comparison limitation: no `.git` metadata exists in the supplied archive, so status, HEAD, and commit-history comparisons remain unavailable. The current source is the reconstructed EW-C1A.1a tree.

### Approved file scope

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.BoundedSingleEdge.cs`

No other source, document, asset, shader, scene, prefab, recipe, metadata, or generated input may change.

### Corrected normal and fan contract

For bevel-owned polygons (`BoundedEdgeBevel` or `EdgeBevelPlane`), retain the normalized `PolygonFace.Normal` exactly.

For every other accepted polygon, calculate one ordered-boundary Newell normal:

```text
N = Σ_i (
    (y_i - y_{i+1})(z_i + z_{i+1}),
    (z_i - z_{i+1})(x_i + x_{i+1}),
    (x_i - x_{i+1})(y_i + y_{i+1}))
```

Require `|N|² > MinimumEdgeLengthSqr`, normalize it, and orient it to the existing face normal:

```text
if dot(N, normalize(face.Normal)) < 0:
    N = -N
```

The resulting `N` is the single authored render normal for that ordinary polygon.

A boundary-fan anchor is valid only when every emitted triangle satisfies:

```text
triangleArea > minimumTriangleArea
abs(dot(normalize(cross(b-a, c-a)), N)) >= 0.5
```

Rank valid anchors by:

1. greatest minimum normal agreement;
2. greatest minimum triangle area;
3. lowest anchor index.

During emission, swap `b/c` when the geometric normal points opposite `N`, then require:

```text
dot(normalize(cross(b-a, c-a)), N) >= 0.5
```

This matches the unchanged final mesh guard. No relaxed threshold, per-triangle render normal, centre-fan fallback, or mesh-output exception is permitted.

### File-by-file implementation sequence

1. [x] Record the failed coordinate, corrected diagnosis, two-file scope, equations, invariants, and validation gates in this canonical plan before source modification.
2. [x] Add a local ordered-boundary normal resolver for ordinary one-surface polygons; retain the existing analytical normal for bevel polygons.
3. [x] Extend stable fan-anchor selection to require and rank the `0.5` render-normal agreement contract.
4. [x] Enforce the same `0.5` agreement during triangle emission before writing authored channels.
5. [x] Reread the complete modified source and affected callers/consumers; compare against the pre-edit EW-C1A.1a implementation.
6. [x] Run all available C# parsing, symbol/reference, line-ending, scope/diff, formula, package-overlay, patch-reproduction, and ZIP-integrity checks.

### Invariants and non-goals

- Preserve the one-polygon/one-normal/one-surface-group ownership contract.
- Preserve all bevel polygon authored normals and stable surface-group identities.
- Preserve EW-V1A.3b selection, widths, recovery, topology, and validation behavior.
- Preserve EW-C1A.1 as diagnostic-only.
- Do not change `MassGenerator.MeshOutput.cs` or weaken its `0.5` guard.
- Do not use per-triangle normals for ordinary polygons.
- Do not restore centre-fan vertices.
- Do not change clipping, topology, cap construction, identities, shaders, materials, controls, or per-frame work.

### Acceptance criteria

Static acceptance:

- exactly the two approved files differ;
- all supplied C# files parse;
- no using/import changes or unresolved symbols;
- ordinary polygons resolve an ordered-boundary authored normal;
- bevel polygons retain `PolygonFace.Normal`;
- fan-anchor selection and emission both require normalized normal dot `>= 0.5` after orientation;
- no centre-fan fallback exists;
- changed-files overlay and unified patch reproduce the final source tree byte-for-byte.

Unity acceptance:

- compile succeeds;
- seed `6667`, maximum width no longer throws a render-normal/winding exception;
- full suite returns topology `33/33`, preview `33/33`, outliers `5/5`, negative exclusion `1/1`, no cancellation, and no terminal failure;
- polygon-surface telemetry retains expected/actual/authored count equality, zero internal fan vertices, zero group collisions, and `renderValid=1`;
- the front-face X/radial triangulation remains visually absent.


### EW-C1A.1a.1 implementation result

- [x] The prior swap-only diagnosis was corrected before source modification: both triangulation and `BuildMeshData` already corrected negative winding, while the final mesh guard rejected positive normal agreement below `0.5`.
- [x] Ordinary polygons now derive one Newell normal from their complete ordered boundary and orient it to the existing analytical face normal.
- [x] `BoundedEdgeBevel` and `EdgeBevelPlane` polygons retain their existing normalized `PolygonFace.Normal` without reseeding or changing their surface-group ownership.
- [x] Fan-anchor selection rejects any candidate containing a degenerate triangle or a triangle with absolute normalized normal agreement below `0.5`, then ranks survivors by minimum agreement, minimum area, and stable index order.
- [x] Final triangle emission corrects negative winding and rejects normalized render-normal agreement below `0.5` before writing authored normals and groups.
- [x] `MassGenerator.MeshOutput.cs`, `TriangleSoup`, topology, clipping, recovery, shaders, materials, controls, assets, and the diagnostic-only corner transaction remain byte-identical to EW-C1A.1a.
- [x] Post-implementation source audit passed: exactly two approved files differ; all 185 supplied C# files parse; imports are unchanged; no unresolved helper call remains; CRLF and trailing-whitespace contracts pass; synthetic planar, reversed, subdivided, and narrow-boundary polygons satisfy the Newell/fan agreement model.
- [x] Package-overlay, patch-reproduction, and ZIP-integrity evidence is recorded in the delivered static-validation report.
- [ ] Unity compile, seed `6667` maximum-width verification, complete one-click suite, and visual confirmation remain pending.

## EW-C1A.1a.2 — One-surface fallback triangulation and complete Macro probe evidence

**Status:** [implemented; Unity validation pending]

### Objective

Restore the complete EW-V1A.3b validation suite while preserving the accepted one-polygon/one-render-surface correction. Direct boundary triangulation remains preferred. A projected centre fan becomes a certified fallback only when no boundary anchor can satisfy the unchanged final render-normal agreement threshold for every triangle.

### Read-only evidence reviewed before implementation

- Unity report `Pasted text(125).txt` reports `currentPreviewPassed=1`, `polygonSurface.renderValid=1`, and a fail-fast Macro contract failure before either topology matrix runs. `macroZeroParity=0`, `strengthZero=0`, `coverageZero=0`, `macroRetention=0`, and `unprovenLosses={audit-unavailable}` prove the current maximum-width preview is healthy while the two uniform-width probe builds cannot complete their audits.
- `Game/Procedural/Masses/MassGenerator.EdgeWear.BoundedSingleEdge.cs::TryTriangulateBoundedOneSurfaceFace` currently permits only one direct boundary fan and returns failure when `TryFindStableOneSurfaceFanAnchor` finds no anchor whose every triangle has normalized normal agreement `>= 0.5`.
- The same function already assigns one authored normal and one authored surface group per polygon. The visible X/radial defect was caused by per-triangle render/material ownership, not by centre-fan geometry alone.
- `Game/Procedural/Masses/MassGenerator.MeshOutput.cs::ValidateGeneratedMassMeshData` remains authoritative and unchanged: every stored render normal must agree with the final triangle winding by at least `0.5`.
- `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs::EvaluateMacroVariationContract` evaluates strength-zero, coverage-zero, current, and maximum cases, but its report currently emits only `currentFailure` and `maximumFailure`. The exact zero-probe blockers are therefore lost.
- Direct callers/consumers reread: `MassGenerator.EdgeWear.BoundedAllEdges.cs`, `MassGenerator.EdgeWear.PlaneCutKernel.cs`, `TriangleSoup.AddTriangle`, `MassGenerator.MeshOutput.cs`, `MassGenerator.EdgeWear.Diagnostics.Logging.cs`, and the full-suite editor job.
- Repository comparison limitation: the supplied archive contains no `.git` metadata. The pre-edit comparison baseline is the delivered EW-C1A.1a.1 reconstructed tree.

### Approved file scope

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
6. `Game/Procedural/Masses/MassGenerator.EdgeWear.BoundedSingleEdge.cs`
7. `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
8. `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

No other source, asset, shader, scene, prefab, material, recipe, metadata, mesh channel, generated input, or per-frame path may change.

- Post-plan scope audit: `PolygonSurface*` storage is copied through `MassGenerator.EdgeWear.PlaneCutKernel.cs`, which is outside the approved eight-file scope. The plan therefore derives both mode face counts from existing propagated fields instead of adding new storage. This preserves exact scope and behavior: one fallback face always contributes exactly one internal fan vertex.

### Triangulation contract

For each accepted polygon, resolve exactly one authored normal and one stable authored surface group before choosing triangulation.

Preferred direct boundary fan:

```text
triangle count = boundary vertex count - 2
internal fan vertices = 0
for every triangle:
    area > minimumTriangleArea
    dot(oriented geometric normal, authored polygon normal) >= 0.5
```

If no direct boundary anchor satisfies the contract, calculate the arithmetic boundary centre and project it onto the authoritative polygon plane:

```text
centre = sum(boundary vertices) / boundary vertex count
residual = dot(authoredNormal, centre) - planeDistance
projectedCentre = centre - authoredNormal * residual
```

Emit one triangle per boundary segment:

```text
(projectedCentre, vertex[i], vertex[i+1])
```

For every fallback triangle:

```text
area > minimumTriangleArea
orient b/c so dot(cross(b-a, c-a), authoredNormal) >= 0
dot(normalize(cross(b-a, c-a)), authoredNormal) >= 0.5
```

All triangles from either mode receive the same polygon authored normal and authored surface group. Failure is permitted only when both the direct fan and projected-centre fallback fail.

### Telemetry contract

Add one explicit triangulation mode enum. The approved scope does not include `MassGenerator.EdgeWear.PlaneCutKernel.cs`, which owns the copied complete-shell audit structure. Therefore no new propagated storage field is added. Reuse the existing counters without scope expansion:

```text
centreFanFallbackFaces = PolygonSurfaceInternalFanVertexCount
boundaryFanFaces = PolygonSurfaceFaceCount - centreFanFallbackFaces
```

Each fallback face inserts exactly one projected centre, so this derivation is exact and is validated as `0 <= internalFanVertices <= faces`. `polygonSurface` formatting prints both derived face counts.

Per-face expected triangle count is mode-dependent:

```text
boundary fan: boundary vertex count - 2
centre fallback: boundary vertex count
```

`polygonSurface` telemetry must report both face counts. `internalFanVertices` may be nonzero only when it equals `centreFanFallbackFaces`.

### Macro probe evidence contract

The full-suite Macro report must emit all four probe outcomes and blockers:

```text
strengthZeroPassed
strengthZeroFailure
coverageZeroPassed
coverageZeroFailure
currentPassed
currentFailure
maximumPassed
maximumFailure
```

No probe failure may collapse to only `audit-unavailable` when its `PrimaryFailure` exists.

### File-by-file implementation sequence

1. [x] Record current evidence, eight-file scope, formulas, invariants, risks, and validation gates before source modification.
2. [x] Add the explicit polygon triangulation mode type and reuse the existing face/internal-fan counters so no unapproved PlaneCutKernel edit is required.
3. [x] Keep direct boundary triangulation as the preferred path and add a projected-centre fallback using the same authored normal/group contract and unchanged `0.5` guard.
4. [x] Accumulate mode-correct expected triangles and internal fan vertices in the existing audit fields; derive and format boundary/fallback face counts without changing PlaneCutKernel.
5. [x] Print all four Macro probe success/failure records in the one-click report.
6. [x] Reconcile the framework, recovery architecture, and code inventory with the actual final symbols and fallback ownership.
7. [x] Reread every changed file and affected caller/consumer, compare against EW-C1A.1a.1, and run all available parsing, reference, scope, formatting, formula, packaging, overlay, and patch-reproduction checks.

### Invariants and non-goals

- Preserve one polygon = one authored render normal + one authored material surface group.
- Preserve direct boundary triangulation when it certifies.
- Preserve existing bevel normals and bevel surface-group identities.
- Preserve `ValidateGeneratedMassMeshData` and its `0.5` agreement threshold.
- Preserve EW-V1A.3b selection, width, topology, recovery, and outlier behavior.
- Preserve EW-C1A.1 as diagnostic-only.
- Do not add per-triangle material identity, per-triangle authored normals, shader exceptions, new controls, assets, dependencies, or runtime work.

### Risks and controls

- **Risk:** fallback centre lies off-plane. **Control:** project it onto the authoritative plane and record the resulting residual.
- **Risk:** fallback recreates visible X lines. **Control:** every fallback triangle shares the same authored normal and surface group; visual acceptance must confirm the previous X/radial boundaries remain absent.
- **Risk:** expected-triangle telemetry remains hard-coded to `n-2`. **Control:** calculate and accumulate expected counts after the selected mode is known.
- **Risk:** fallback hides invalid polygons. **Control:** retain all existing convexity, planarity, area, render-normal, collision, topology, and final mesh certification gates.

### Acceptance criteria

Static acceptance:

- exactly the eight approved files differ;
- all supplied C# files parse with balanced preprocessor blocks;
- no unresolved new symbol or import change;
- direct fan remains first;
- projected centre is explicitly projected to the polygon plane;
- both modes enforce area and normalized render-normal dot `>= 0.5`;
- all triangles retain one authored polygon normal/group;
- expected triangle counts are mode-correct;
- all four Macro probes print pass/failure evidence;
- changed-files overlay and unified patch reproduce the final reconstructed tree byte-for-byte.

Unity acceptance:

- compile succeeds;
- `macroZeroParity=1` and `macroRetention=1`;
- topology `33/33`, preview `33/33`, outliers `5/5`, negative exclusion `1/1`;
- no cancellation or terminal failure;
- `polygonSurface.renderValid=1`, authored counts equal actual triangles, surface-group collisions remain zero, and fallback count/internal fan count are consistent;
- the former X/radial surface pattern remains visually absent.

### EW-C1A.1a.2 implementation result

- [x] Direct boundary fans remain the preferred path and keep `n - 2` triangles with no internal vertex.
- [x] Ordinary polygons with no certifiable direct anchor now use one arithmetic boundary centre projected onto the authoritative polygon plane.
- [x] Both modes use the same area, winding, normalized `0.5` render-normal agreement, authored polygon normal, and authored surface-group contract.
- [x] Bevel polygons remain direct-fan-only and retain the accepted bevel normal and surface-group ownership.
- [x] `PolygonSurfaceExpectedTriangleCount` is mode-aware. Each successful fallback increments `PolygonSurfaceInternalFanVertexCount` exactly once; diagnostics derive and print boundary-fan and fallback face counts without modifying `PlaneCutKernel`.
- [x] The Macro report prints `strengthZeroPassed/Failure`, `coverageZeroPassed/Failure`, `currentPassed/Failure`, and `maximumPassed/Failure`.
- [x] Post-change source audit passed `55/55`: exact eight-file scope, all 185 supplied C# files parsed, preprocessor blocks balanced, imports unchanged, direct callers/consumers reread, unchanged mesh/final guard and PlaneCut propagation confirmed, CRLF/trailing-whitespace/fence checks passed, and the mathematical direct/fallback mirror passed.
- [x] Delivery overlay and patch-application tests reproduce the intended 314-file Assets view byte-for-byte. ZIP CRC and final patch whitespace/application checks are rerun during final packaging.
- [ ] Unity compilation, complete one-click suite, and visual confirmation remain pending.

## EW-C1A.1a.3 — Universal one-surface projected-centre fallback

**Status:** implemented; static validation passed; Unity validation pending.

### Failure evidence and reviewed ownership

- Unity full-suite report `Pasted text(126).txt` failed before the topology matrix because both zero-Macro probes reported `EdgeBevelPlane:18` / face `30` as having no stable direct triangulation anchor. The current and maximum probes remained valid, so the blocker is the fallback eligibility restriction rather than the accepted preview topology.
- `Game/Procedural/Masses/MassGenerator.EdgeWear.BoundedSingleEdge.cs::TryTriangulateBoundedOneSurfaceFace` currently gates `TryResolveProjectedOneSurfaceCentre` with `!isBevelFace`; the failing `EdgeBevelPlane` polygon therefore cannot use the implemented fallback.
- The same method passes `false` to `TryEmitOneSurfaceTriangle` in its fallback branch and increments only `PolygonSurfaceInternalFanVertexCount`. A bevel fallback must instead retain bevel ownership and increment `BevelRegionInternalFanVertexCount` exactly once.
- `TryTriangulateBoundedPreviewFaces` currently requires `BevelRegionInternalFanVertexCount == 0`. That direct-fan-only condition conflicts with the approved universal one-surface fallback and must become a bounded consistency condition.
- `MassGenerator.EdgeWear.PlaneCutKernel.cs` already copies `BevelRegionInternalFanVertexCount` through baseline, trial, and committed audit results. It requires no modification.
- `MassGenerator.EdgeWear.Diagnostics.Logging.cs` currently prints only bevel `internalFanVertices`. It must also derive and print bevel `boundaryFanFaces` and `centreFanFallbackFaces` from existing propagated fields.
- Direct callers and consumers reviewed without planned edits: `MassGenerator.EdgeWear.BoundedAllEdges.cs`, `MassGenerator.EdgeWear.PlaneCutKernel.cs`, `MassGenerator.Types.cs::TriangleSoup`, `MassGenerator.MeshOutput.cs::ValidateGeneratedMassMeshData`, and the editor full-suite Macro probe job.
- Repository comparison limitation: the supplied source tree has no `.git` metadata. The pre-edit baseline is the delivered EW-C1A.1a.2 reconstructed tree.

### Approved file scope

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Game/Procedural/Masses/MassGenerator.EdgeWear.BoundedSingleEdge.cs`
6. `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`

No other code, shader, material, scene, prefab, recipe, asset, metadata, mesh channel, control, generated input, or per-frame path may change.

### Universal triangulation contract

For every accepted polygon, including `BoundedEdgeBevel` and `EdgeBevelPlane`:

```text
1. resolve one authored normal and one authored surface group
2. try a stable direct boundary fan
3. if no direct anchor certifies, try one projected-centre fan
4. reject only when both modes fail
```

The fallback remains:

```text
centre = arithmetic mean of boundary vertices
projectedCentre = centre - authoredNormal *
    (dot(authoredNormal, centre) - planeDistance)
```

Every fallback triangle must preserve the same existing requirements:

```text
area > minimumTriangleArea
winding oriented toward authoredNormal
normalized geometric/render-normal dot >= 0.5
one authored normal for the polygon
one authored surface group for the polygon
```

For a bevel fallback, `TryEmitOneSurfaceTriangle` must receive `isBevelFace = true`, preserving bevel triangle counters, feature ownership, analytical bevel normal, and bevel surface-group identity.

### Counter and certification contract

Each successful projected-centre fallback contributes exactly one internal fan vertex:

```text
PolygonSurfaceInternalFanVertexCount += 1
if bevel polygon:
    BevelRegionInternalFanVertexCount += 1
```

Complete-shell validation remains:

```text
0 <= PolygonSurfaceInternalFanVertexCount <= PolygonSurfaceFaceCount
```

Bevel-region validation becomes:

```text
0 <= BevelRegionInternalFanVertexCount <= BevelRegionFaceCount
BevelRegionTriangleCount == BevelRegionAuthoredNormalTriangleCount
BevelRegionTriangleCount == BevelRegionAuthoredSurfaceGroupTriangleCount
BevelRegionFailureFace < 0
```

No final mesh guard is weakened. `MassGenerator.MeshOutput.cs::ValidateGeneratedMassMeshData` retains its `0.5` render-normal agreement threshold.

### Telemetry contract

`planeSurface` and bounded bevel-region records must derive:

```text
centreFanFallbackFaces = clamp(
    BevelRegionInternalFanVertexCount,
    0,
    BevelRegionFaceCount)

boundaryFanFaces = max(
    0,
    BevelRegionFaceCount - centreFanFallbackFaces)
```

Both counts and `internalFanVertices` must be printed. The zero-Macro probes should therefore identify the failing `EdgeBevelPlane:18` as one certified centre-fan fallback rather than fail before the matrices.

### Implementation sequence

1. [x] Record the current failure, exact six-file scope, reviewed owners, formulas, invariants, and validation gates before implementation.
2. [x] Remove the `!isBevelFace` fallback restriction and replace the face-class-specific blocker with a universal two-mode blocker.
3. [x] Preserve bevel ownership in fallback emission and increment the bevel internal-fan counter exactly once per fallback bevel face.
4. [x] Replace the direct-fan-only bevel-region validation condition with bounded counter consistency while preserving all triangle, authored-normal, authored-group, and failure gates.
5. [x] Add bevel-region direct/fallback face counts to current, bounded, and detailed telemetry using existing propagated counters.
6. [x] Reconcile the framework, recovery architecture, and code inventory with the universal fallback and unchanged final mesh guard.
7. [x] Reread all modified files and affected callers/consumers; run parsing, namespace/reference, scope, line-ending, whitespace, formula, patch, ZIP, and overlay reproduction checks.
8. [x] Unity compile, complete one-click suite, and visual confirmation remain pending user validation.

### Invariants and non-goals

- Preserve one polygon = one authored render normal + one authored surface group.
- Preserve direct boundary triangulation as the preferred mode.
- Preserve analytical normals and stable surface groups for bevel polygons.
- Preserve EW-V1A.3b selection, Macro width, topology, recovery, and outlier behavior.
- Preserve EW-C1A.1 as diagnostic-only.
- Do not weaken the final mesh guard, introduce per-triangle material identities, add controls, alter shaders/assets, or add runtime work.

### Acceptance criteria

Static acceptance:

- exactly the six approved files differ;
- all supplied C# files parse and preprocessor blocks balance;
- no import or namespace change;
- direct fan remains first and fallback is available to every polygon class;
- fallback bevel triangles update both complete-shell and bevel-region counters;
- both internal-fan counters are bounded by their face counts;
- all render-normal, area, authored-normal, authored-group, collision, and final mesh guards remain active;
- telemetry prints direct/fallback bevel face counts;
- changed-files overlay and unified patch reproduce the final tree byte-for-byte.

Unity acceptance:

```text
macroZeroParity=1
macroRetention=1
topologyCases=33/33
previewCases=33/33
outlierResolutionChecks=5/5
negativeExclusionChecks=1/1
cancelled=0
terminalReason=none
```

The former X/radial surface pattern must remain visually absent.

### EW-C1A.1a.3 implementation result

- [x] Removed the bevel-class exclusion. `BoundedEdgeBevel` and `EdgeBevelPlane` now use the same direct-first, projected-centre-second selection as every other polygon.
- [x] Fallback bevel triangles pass `isBevelFace = true` into `TryEmitOneSurfaceTriangle`, preserving analytical bevel normals, bevel feature/group ownership, and all bevel triangle counters.
- [x] Each fallback bevel face increments both `PolygonSurfaceInternalFanVertexCount` and `BevelRegionInternalFanVertexCount` exactly once.
- [x] Bevel-region certification now permits a bounded number of certified fallback faces while retaining triangle/authored-normal/authored-group equality and failure gates.
- [x] Current, bounded, compact, and detailed bevel-region telemetry now print derived `boundaryFanFaces` and `centreFanFallbackFaces` alongside `internalFanVertices`.
- [x] `MassGenerator.EdgeWear.PlaneCutKernel.cs` remains unchanged and continues propagating the existing counters. `MassGenerator.MeshOutput.cs::ValidateGeneratedMassMeshData` remains unchanged at normalized render-normal agreement `>= 0.5`.
- [x] Post-change audit passed: 53/53 dedicated static checks; all 185 supplied C# files parsed; preprocessor blocks balanced; exact six-file scope; imports unchanged; critical consumers byte-identical; CRLF/trailing-whitespace/Markdown-fence checks passed; six-file ZIP CRC passed; complete 314-file Assets overlay reproduced byte-for-byte; patch applied with strict whitespace checking and reproduced the Docs/Game view byte-for-byte.
- [ ] Unity compilation, complete one-click suite, and visual confirmation remain pending.

- [x] Final diff contains 17 source-line changes in `MassGenerator.EdgeWear.BoundedSingleEdge.cs` and 33 telemetry-line additions in `MassGenerator.EdgeWear.Diagnostics.Logging.cs`; no selection, width, clipping, topology, shader, material, asset, serialized-control, or per-frame code changed.
- [x] Final source comparison confirms `MassGenerator.EdgeWear.PlaneCutKernel.cs`, `MassGenerator.MeshOutput.cs`, `MassGenerator.Types.cs`, `MassGenerator.EdgeWear.Types.cs`, and `GeneratedMassEditor.cs` are byte-identical to EW-C1A.1a.2.
## EW-C1A.1a.4 — Deterministic complete one-surface polygon triangulation

**Status:** [implemented; static/compliance validation passed; Unity validation pending]

### Objective

Replace the insufficient fan-only fallback with a deterministic complete triangulation solver that preserves one polygon as one authored render surface. A stable direct boundary fan remains preferred. When no direct anchor certifies, a bounded interval dynamic-programming solver must search all valid internal-diagonal decompositions of the ordered polygon and return the best complete `n - 2` triangle solution.

### Read-only evidence reviewed before implementation

- Unity report `Pasted text(127).txt` reports `currentPreviewPassed=1`, current `polygonSurface.renderValid=1`, current `planeSurface.renderValid=1`, and fail-fast before both 33-case matrices. The exact failures are `strengthZeroFailure` and `coverageZeroFailure` on `EdgeBevelPlane:18` / face `30`: the polygon has neither a stable direct fan nor a stable projected-centre fallback. Current and maximum probes pass.
- `Game/Procedural/Masses/MassGenerator.EdgeWear.BoundedSingleEdge.cs::TryTriangulateBoundedOneSurfaceFace` currently offers only `BoundaryFan` and `ProjectedCentreFan`. Both are star/fan decompositions and therefore do not cover all valid triangulations of an ordered planar polygon.
- `TryTriangulateBoundedPreviewFaces` verifies every source shell polygon is finite, subdivision-safe, planar, simple, and convex after `BuildBoundedConvexityCheckLoop` removes tolerance-collinear vertices. The emitted boundary retains those vertices, so the complete solver must support a retained loop containing a tolerance-accepted slight reflex turn rather than applying strict convex half-space containment. It routes bounded-single, bounded-all, baseline plane-shell, topology-scale retry, and diagnostic plane-shell paths through the same one-surface triangulator.
- Existing reusable predicates reviewed: `TryProjectChamferPatchLoop`, `CalculateChamferPatchSignedArea`, `ChamferPatchPolygonSelfIntersects`, `ChamferPatchCross2D`, `ChamferPatchPointInOrOnTriangle`, `ChamferPatchDiagonalIntersectsRemainingBoundary`, and `TryGetChamferPatchSegmentIntersectionEvidence`. Existing greedy ear-clipping owners were reviewed but cannot prove a complete solution under the stricter final render-normal threshold.
- `MassGenerator.MeshOutput.cs::BuildMeshData` consumes authored normals/groups. `ValidateGeneratedMassMeshData` remains authoritative and rejects normalized geometric/render-normal agreement below `0.5`. `TriangleSoup.AddTriangle` already stores one authored normal and one authored group per emitted triangle.
- Post-implementation reread finding: `BuildBoundedConvexityCheckLoop` removes vertices within `max(PointMergeDistance * 8, maximumEdgeLength * 1e-6)` of the neighbouring span before `IsBoundedPolygonConvex`, but `TryTriangulateBoundedOneSurfaceFace` receives the unsimplified boundary. Therefore a strict same-side test against every retained edge is not equivalent to the accepted convexity contract; `IsBoundedPointInsideOrOnPolygon` is the existing correct retained-loop containment owner.
- Final performance-audit finding: an intermediate implementation cached `OneSurfaceTriangleCandidate[n,n,n]`, which was `O(n^3)` temporary memory and violated the approved `O(n^2)` bound. The final solver evaluates each reachable `(i,k,j)` candidate directly inside the interval loop and stores only projected points, the `O(n^2)` diagonal table, and the `O(n^2)` state table.
- Direct callers and propagation owners reread: `MassGenerator.EdgeWear.BoundedAllEdges.cs`, `MassGenerator.EdgeWear.PlaneCutKernel.cs`, `MassGenerator.EdgeWear.Diagnostics.Logging.cs`, `MassGenerator.EdgeWear.PatchConstruction.cs`, `MassGenerator.EdgeWear.ContainedOwnership.cs`, `MassGenerator.EdgeWear.SliverAndTriangulation.cs`, and the one-click Macro/full-suite editor workflow.
- Historical comparison: EW-C1A.1a.2 added the projected-centre fallback; EW-C1A.1a.3 made it universal. The new Unity evidence proves the fallback geometry itself is insufficient. The supplied reconstructed source has no authoritative `.git` metadata; EW-C1A.1a.3 is the exact pre-edit baseline.

### Approved file scope

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
6. `Game/Procedural/Masses/MassGenerator.EdgeWear.BoundedSingleEdge.cs`
7. `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`

No other source, caller, asset, shader, material, scene, prefab, recipe, metadata, generated input, mesh channel, serialized control, or per-frame path may change. `MassGenerator.EdgeWear.PlaneCutKernel.cs`, `MassGenerator.MeshOutput.cs`, `MassGenerator.Types.cs`, and `GeneratedMassEditor.cs` remain read-only consumers.

### Triangulation contract

For one polygon with ordered vertices `v[0..n-1]`, one authored normal, and one authored surface group:

```text
1. try every stable direct boundary fan
2. if none certifies, project the ordered boundary to its authoritative plane
3. reject a non-finite, zero-area, or self-intersecting projection
4. solve interval [0,n-1] by bounded dynamic programming
5. emit exactly n - 2 triangles from the selected complete solution
```

A candidate triangle `(i,k,j)` certifies only when:

```text
area(i,k,j) > minimumTriangleArea
all positions and normals are finite
winding can be oriented to authoredNormal
dot(normalize(cross), authoredNormal) >= 0.5
required non-boundary diagonals have an inside-or-on midpoint in the retained projected simple loop
required diagonals do not intersect unrelated boundary segments
```

For interval state `S(i,j)` and split `k`:

```text
S(i,k) succeeds
S(k,j) succeeds
triangle(i,k,j) certifies
score.minimumArea = min(left.minimumArea, right.minimumArea, triangle.area)
score.minimumNormalDot = min(left.minimumNormalDot, right.minimumNormalDot, triangle.normalDot)
```

Deterministic ranking:

```text
1. highest complete-solution minimum triangle area
2. highest complete-solution minimum normal agreement
3. lowest split vertex index
```

Complexity is bounded dirty-time `O(n^3)` with `O(n^2)` state. No runtime/per-frame path is introduced.

### Render and telemetry contract

- Every selected triangle receives the same polygon authored normal, authored surface group, feature, and feature strength.
- Both direct and general modes emit exactly `n - 2` triangles and introduce zero synthetic/internal fan vertices.
- The projected-centre fan is removed from active selection. `PolygonSurfaceInternalFanVertexCount` and `BevelRegionInternalFanVertexCount` return to the literal value `0`.
- Existing propagated fields cannot represent successful general-triangulation face counts without editing `PlaneCutKernel.cs`, which is outside approved scope. Logging must therefore stop deriving false direct/fallback counts from internal-fan fields and instead report the truthful policy `direct-preferred/general-complete`, exact triangle counts, and `internalFanVertices:0`.
- Any failed polygon blocker must include face/provenance, boundary count, authored normal, plane residual, minimum triangle area, direct-fan anchors tested, best partial direct-fan evidence, exact rejected triangle/reason, projected states evaluated, valid triangle candidates, area/normal/diagonal rejection counts, complete-solution status, and selected triangle indices when available. Existing `PolygonSurfaceFailureReason` propagation carries this evidence through `PlaneCutKernel.cs` without scope expansion.

### File-by-file implementation sequence

1. [x] Record the new Unity evidence, disproven assumption, exact seven-file scope, algorithm, invariants, risks, and validation gates before source modification.
2. [x] Replace `ProjectedCentreFan` with a non-serialized `GeneralTriangulation` mode and add bounded solver/evidence types.
3. [x] Extend direct-fan evaluation to return exact tested-anchor and rejecting-triangle evidence without changing successful anchor ranking.
4. [x] Add deterministic projected interval-DP triangulation using the existing projection, self-intersection, cross-product, and segment-intersection predicates.
5. [x] Emit the selected `n - 2` indexed triangles through the unchanged one-surface authored-normal/group path; restore both internal-fan counters to exact zero-only certification.
6. [x] Replace inaccurate centre-fan mode telemetry with truthful direct-preferred/general-complete policy output and exact failure evidence.
7. [x] Reconcile framework, recovery architecture, and code inventory with the final symbols and supersede EW-C1A.1a.2/.3 fan fallback instructions.
8. [x] Reread all changed files and affected callers/consumers; compare with EW-C1A.1a.3; run parser, preprocessor, namespace/reference, scope, line-ending, whitespace, algorithm, deterministic-solver, patch, ZIP, and complete-overlay checks.
9. [ ] Unity compile, four Macro probes, complete 33-case matrices, outlier/negative checks, and visual confirmation remain pending user validation.

### Invariants and non-goals

- Preserve one polygon = one authored render normal + one authored surface group.
- Preserve direct boundary fan as the preferred fast path.
- Preserve analytical bevel normals and stable bevel surface-group identities.
- Preserve final `0.5` mesh guard; do not lower thresholds or negate authoritative normals.
- Preserve EW-V1A.3b selection, Macro width, clipping, topology, recovery, outlier, and negative-exclusion behavior.
- Preserve EW-C1A.1 as diagnostic-only and do not promote corner geometry.
- Do not add synthetic centre vertices, per-triangle material identity, per-triangle authored normals, controls, dependencies, shaders, assets, or runtime work.

### Risks and controls

- **Risk:** a diagonal crosses or leaves the retained polygon boundary. **Control:** projection self-intersection proof, explicit unrelated-boundary segment intersection rejection, and the existing tolerance-aware `IsBoundedPointInsideOrOnPolygon` midpoint test. A strict convex half-space test is prohibited because the retained loop may contain a tolerance-accepted slight reflex vertex removed only from the convexity-check copy.
- **Risk:** greedy selection misses a complete solution. **Control:** interval dynamic programming evaluates every valid split and stores the best complete subsolution.
- **Risk:** deterministic ties drift. **Control:** fixed area/dot/index ranking with explicit epsilon comparisons and stable reconstruction order.
- **Risk:** audit output becomes opaque. **Control:** include exact direct and general candidate/rejection evidence in the propagated failure reason.
- **Risk:** telemetry falsely labels general faces as direct fans. **Control:** remove derived mode counts that cannot be propagated under approved scope; report policy and literal zero internal vertices only.
- **Risk:** dirty-time cost or temporary memory grows. **Control:** triangle certification occurs inside the interval loop without an `n^3` candidate cache; time remains `O(n^3)`, temporary memory remains `O(n^2)`, observed polygon boundary counts are small, and no per-frame invocation exists.

### Acceptance criteria

Static acceptance:

- exactly the seven approved files differ;
- all supplied C# files parse and preprocessor blocks balance;
- no import, namespace, serialized, shader, material, asset, or caller change outside scope;
- projected-centre active selection is absent;
- direct fan remains first; complete DP fallback emits exactly `n - 2` triangles and zero internal vertices;
- every selected triangle satisfies area, winding, and normalized normal agreement `>= 0.5`;
- deterministic solver tests cover fan failure/general success, reversed projection orientation, near-collinear boundary vertices, no-solution evidence, and stable tie-breaking;
- final mesh guard and all one-surface authored-normal/group checks remain active;
- changed-files overlay and patch reproduce the final source tree byte-for-byte.

Unity acceptance:

```text
strengthZeroPassed=1
coverageZeroPassed=1
currentPassed=1
maximumPassed=1
macroZeroParity=1
macroRetention=1
topologyCases=33/33
previewCases=33/33
outlierResolutionChecks=5/5
negativeExclusionChecks=1/1
cancelled=0
terminalReason=none
```

The former X/radial internal surface pattern must remain visually absent.

### Post-implementation consistency and compliance result

- Exactly the seven approved files differ from EW-C1A.1a.3; no file was created, deleted, moved, renamed, or modified outside scope.
- The completed reread corrected one planned predicate before packaging: the retained boundary can contain a tolerance-accepted slight reflex vertex removed only from `BuildBoundedConvexityCheckLoop`. General diagonal containment therefore reuses `IsBoundedPointInsideOrOnPolygon` after unrelated-boundary intersection rejection instead of an invalid strict convex half-space test.
- All `185/185` supplied C# files parse with tree-sitter, all preprocessor blocks balance, imports/namespaces remain unchanged, and the new partial-class helper references resolve to existing owners. Unity compilation is unavailable in the supplied source-only environment and remains pending.
- Static/compliance validation passed `56/56`: direct-first selection, active projected-centre removal, interval-DP loops/ranking/reconstruction, exact `n - 2` emission, literal zero internal-fan certification, unchanged `0.5` mesh guard, truthful policy logging, CRLF/BOM/whitespace/fence checks, unchanged read-only callers/consumers, and absence of any `n^3` candidate cache.
- Independent deterministic mirror checks passed direct-fan success, fan-failure/general-success on a tolerance-reflex eight-vertex loop, reversed orientation, clean no-solution rejection, stable lowest-split tie resolution, and `50/50` DP-versus-exhaustive ranking comparisons.
- The pre-result changed-files overlay and generated patch each reproduced all `314` supplied files byte-for-byte. The final rebuilt artifact reproduction, strict patch application, ZIP integrity, and hashes are recorded in `GeneratedMass_EW-C1A.1a.4_StaticValidation.txt` delivered with the patch.
- Item 9 remains pending: Unity compile, all four Macro probes, both `33/33` matrices, outlier `5/5`, negative exclusion `1/1`, and visual confirmation. EW-C1A.2 remains blocked until that evidence passes.


## EW-C1A.1a.5 — Tolerance-collinear boundary reinsertion

**Status:** implemented; static/compliance validation passed; Unity runtime and visual acceptance pending.

### Failure evidence and reviewed ownership

- Unity full-suite report `Pasted text(128).txt` fails only the Macro strength-zero and coverage-zero probes. Both report `EdgeBevelPlane:18`, face `30`, with six retained boundary vertices, maximum plane residual `1.1920929E-07`, minimum triangle area `3.98206534E-09`, `10` valid general-triangulation candidates, `4` normal-agreement rejections, zero diagonal rejections, and no complete DP solution. Current and maximum-Macro probes remain valid.
- The best direct fan is anchored at retained vertex `1`, certifies `3/4` required triangles with minimum area `0.00124909263` and minimum normal dot `0.99999994`, then rejects triangle `1/5/0` only for `NormalAgreement`. This isolates the blocker to one tolerance-collinear retained-boundary subdivision, not polygon planarity, simplicity, diagonal containment, or area.
- `Game/Procedural/Masses/MassGenerator.EdgeWear.BoundedSingleEdge.cs::BuildBoundedConvexityCheckLoop` already removes tolerance-collinear vertices before convexity certification, while `TryTriangulateBoundedPreviewFaces` intentionally triangulates the complete retained boundary. The render triangulator therefore needs a bounded normalization/reinsertion stage rather than another fan or a weakened normal threshold.
- `TryTriangulateBoundedPreviewFaces` is consumed by `MassGenerator.EdgeWear.BoundedAllEdges.cs`, three plane-cut validation/commit paths in `MassGenerator.EdgeWear.PlaneCutKernel.cs`, and the bounded single-edge path. These callers require exactly the same complete boundary and `n - 2` triangle result; no caller edit is approved.
- `MassGenerator.Types.cs::TriangleSoup` preserves authored normal/group per emitted triangle. `MassGenerator.MeshOutput.cs::BuildMeshData` reorients winding only, and `ValidateGeneratedMassMeshData` rejects any final triangle whose minimum vertex render-normal agreement is below `0.5`. These consumers remain unchanged.
- Existing projection, self-intersection, diagonal-intersection, and point-in-polygon predicates are owned by the edge-wear partial class and remain reusable without a new dependency.
- Canonical documents reviewed: this checklist, `Generated_Mass_Framework.md`, `Generated_Mass_Edge_Wear_Recovery_Architecture.md`, and `Generated_Mass_Edge_Wear_Code_Inventory.md`. EW-C1A.1a.2/.3 centre-fan instructions are superseded; EW-C1A.1a.4 direct-first/general-complete remains the base solver.
- Repository comparison limitation: the supplied authoritative source tree contains `314` files and no `.git` metadata. The pre-edit baseline is the byte-identical delivered EW-C1A.1a.4 tree. Relevant superseded comparisons are EW-C1A.1a.3 and EW-C1A.1a.4 changed-file archives and patches.

### Approved file scope

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
6. `Game/Procedural/Masses/MassGenerator.EdgeWear.BoundedSingleEdge.cs`
7. `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`

No other code, shader, material, scene, prefab, recipe, asset, metadata, serialized control, mesh channel, generated input, dependency, or per-frame path may change.

### Boundary normalization and reinsertion contract

The existing direct boundary fan remains first. The existing complete DP solver remains second on the original retained loop. Only when both fail and the exact failure includes `NormalAgreement` may the bounded reinsertion path run:

```text
1. project the complete retained boundary to the authoritative face plane
2. classify removable tolerance-collinear vertices without modifying PolygonFace
3. remove one eligible vertex at a time from a working index loop
4. certify the simplified loop as finite, simple, same-winding, and boundary-contained
5. run the existing direct-first/general-complete solver on the simplified loop
6. reinsert removed vertices in reverse removal order by subdividing the triangle adjacent to their preserved boundary edge
7. emit exactly originalVertexCount - 2 triangles using only original retained vertices
```

A retained vertex `P` between current working neighbours `A` and `B` is eligible only when all conditions pass:

```text
A/P/B positions and projections are finite
P projects onto segment A-B within tolerance
projected distance(P, segment A-B) <= collinearTolerance
raw local triangle A/P/B fails only NormalAgreement or Area/NonFinite caused by tolerance collinearity
all three vertices remain within the already-certified face-plane tolerance
replacement segment A-B intersects no unrelated retained boundary segment
midpoint(A,B) is inside or on the retained projected polygon
removing P leaves at least three vertices
simplified loop keeps the original signed-area orientation and remains non-self-intersecting
```

Removal ranking is deterministic:

```text
1. smallest projected distance to neighbour segment
2. smallest absolute projected local cross
3. lowest original vertex index
```

For reinsertion, a removed vertex record stores `(previousOriginalIndex, removedOriginalIndex, nextOriginalIndex)`. In reverse removal order, find the selected triangle containing boundary edge `A-B`. Replace oriented triangle `(A,B,C)` with two triangles that preserve its winding and include `P`:

```text
(A,P,C)
(P,B,C)
```

or the cyclic equivalent when the parent edge orientation is `B-A`. Both replacement triangles must independently pass the unchanged area, finite, winding, and normalized authored-normal dot `>= 0.5` certification. Reinsertion fails if the parent boundary edge is absent, belongs to more than one triangle, either replacement triangle fails, or the final triangle count is not `originalVertexCount - 2`.

The stage is render-triangulation normalization only. It must not modify `PolygonFace.Vertices`, source topology, bevel geometry, Macro width, candidate selection, recovery, or mesh-output thresholds.

### Diagnostics contract

The failure evidence must retain current direct/general fields and add:

```text
boundaryVertices:
  original index, 3D position, projected position, plane residual

unstableBoundaryCandidates:
  previous/current/next indices
  local area and normal dot
  segment parameter and projected distance
  local projected cross
  eligible flag and blocker

simplification:
  attempts
  removed indices in order
  retained indices
  signed-area before/after
  self-intersection result
  selected simplified triangles

reinsertion:
  removed index and parent edge
  parent triangle
  replacement triangles
  replacement areas and normal dots
  success/blocker
```

Successful aggregate telemetry remains truthful under the existing propagated audit fields: `triangulationPolicy:direct-preferred/general-complete/collinear-reinsert`, exact expected/actual/authored triangle counts, zero internal fan vertices, zero group collisions, and `renderValid=1`. No new propagated plane-cut fields are required.

### File-by-file implementation sequence

1. [x] Record exact Unity evidence, reviewed owners/callers/consumers, seven-file scope, algorithm, invariants, risks, and acceptance gates before source modification.
2. [x] Add bounded audit/value types for projected boundary vertices, unstable candidates, removals, reinsertion records, and deterministic evidence formatting.
3. [x] Refactor the existing general solver into a helper that accepts an indexed working boundary while retaining original vertex indices and unchanged direct/DP triangle certification.
4. [x] Add deterministic tolerance-collinear candidate classification and one-at-a-time working-loop simplification using existing projection, diagonal-intersection, self-intersection, and point-in-polygon predicates.
5. [x] Triangulate the simplified loop, reinsert removed vertices in reverse order through certified parent-edge subdivision, and require exactly original `n - 2` original-vertex triangles.
6. [x] Add complete local-geometry, simplification, and reinsertion evidence to failure output; update policy logging without adding false propagated counters.
7. [x] Reconcile framework, recovery architecture, and code inventory; mark EW-C1A.1a.4 as base behavior extended by bounded collinear reinsertion.
8. [x] Reread all changed files and affected callers/consumers; compare final behavior with EW-C1A.1a.4 and superseded a.2/.3; run parser, preprocessor, namespace/reference, exact-scope, line-ending, whitespace, deterministic algorithm, exhaustive mirror, patch, ZIP, and complete-overlay checks.
9. [ ] Unity compile, four Macro probes, topology/preview `33/33`, outlier `5/5`, negative exclusion `1/1`, and former-X visual confirmation remain pending user validation.

### Invariants and non-goals

- Preserve one polygon = one authored render normal + one authored surface group.
- Preserve direct fan first and complete DP second; reinsertion is a bounded third path only after original-loop failure.
- Preserve every original retained boundary vertex and boundary segment in final emitted triangles.
- Preserve exactly `n - 2` triangles and zero synthetic/internal fan vertices.
- Preserve analytical bevel normals, stable surface groups, feature identity, and strength.
- Preserve the final `0.5` mesh guard; do not lower thresholds, project emitted vertices, or substitute authored normals per triangle.
- Preserve EW-V1A.3b selection, Macro width, clipping, topology, recovery, outlier, and negative-exclusion behavior.
- Preserve EW-C1A.1 as diagnostic-only; do not promote corner geometry.
- Do not add controls, runtime work, dependencies, shaders, assets, or unrelated cleanup.

### Risks and controls

- **Risk:** simplification removes a meaningful corner. **Control:** only remove vertices within projection tolerance of their current neighbour segment, require local instability evidence, preserve winding/simplicity/containment, and reinsert every removed vertex before emission.
- **Risk:** several consecutive collinear vertices make stored parent edges stale. **Control:** remove one vertex at a time, store current surviving original neighbours, and reinsert in strict reverse order.
- **Risk:** reinsertion creates a weak triangle. **Control:** certify both replacement triangles with the unchanged final area and `0.5` normal-agreement contract before mutating the selected triangle list.
- **Risk:** a parent edge is internal rather than boundary-owned. **Control:** each removal stores a working-loop boundary edge; reinsertion requires exactly one selected parent triangle containing that edge.
- **Risk:** solver becomes nondeterministic. **Control:** fixed candidate ranking, original-index identity, stable working-loop order, existing DP tie rules, and reverse-order reinsertion.
- **Risk:** dirty-time complexity grows. **Control:** observed loops are small; simplification performs at most `n-3` iterations, and each iteration may rerun the existing `O(n^3)` triangulator after bounded candidate predicates. The conservative third-path worst case is therefore `O(n^4)` dirty-time with `O(n^2)` temporary memory. No per-frame path is added.

### Post-implementation audit result

- Exact scope: all and only the seven approved files differ; no file was created, deleted, moved, renamed, or given metadata.
- Source review: final implementations, all shared triangulation callers, `TriangleSoup`, `BuildMeshData`, `ValidateGeneratedMassMeshData`, editor suite ownership, and reused projection/intersection predicates were reread after implementation. No caller signature, serialized control, shader/material input, mesh channel, source topology, corner transaction, or runtime callback changed.
- Static validation: `56/56` checks passed. All `185/185` supplied C# files parse, preprocessor blocks balance, imports/namespaces remain unchanged, CRLF/BOM/whitespace and Markdown fences are valid, and the final `0.5` mesh-normal guard remains byte-identical. Unity compilation is unavailable in the supplied source-only environment.
- Algorithm validation: mirrors pass one removed vertex, consecutive removals with reverse reinsertion, absent and duplicate parent-edge rejection, and stable-loop non-removal. The existing complete-DP method is byte-identical to EW-C1A.1a.4, whose independent `50/50` exhaustive ranking comparison already passed.
- Performance: the bounded third path may rerun `O(n^3)` triangulation after successive removals, so its conservative worst-case cost is `O(n^4)` dirty-time with `O(n^2)` temporary memory. Observed loops are small and no per-frame path exists.
- Preliminary artifact validation: the seven-file ZIP overlay and strict unified-patch application each reproduced the complete `314`-file final tree byte-for-byte; ZIP integrity passed. Final artifacts are rebuilt and reverified after this status write.
- Unity compile, the four Macro probes, both `33/33` matrices, outlier `5/5`, negative exclusion `1/1`, and former-X visual confirmation remain pending. EW-C1A.2 remains blocked.

### Acceptance criteria

Static acceptance:

- exactly the seven approved files differ;
- all supplied C# files parse and preprocessor blocks balance;
- no import, namespace, serialized, shader, material, asset, caller, or mesh-output change outside scope;
- direct and original-loop DP remain first and second;
- reinsertion runs only after original-loop failure with local tolerance-collinear evidence;
- final triangles use only original retained vertices, preserve every boundary segment, count exactly `n - 2`, and have zero internal fan vertices;
- every final triangle satisfies finite, area, winding, and normalized normal agreement `>= 0.5`;
- deterministic mirrors cover single and consecutive removals, reverse reinsertion, absent/duplicate parent-edge rejection, no-simplification cases, and DP-versus-exhaustive ranking;
- final mesh guard remains unchanged;
- changed-files overlay and unified patch reproduce the final `314`-file source tree byte-for-byte.

Unity acceptance:

```text
strengthZeroPassed=1
coverageZeroPassed=1
currentPassed=1
maximumPassed=1
macroZeroParity=1
macroRetention=1
topologyCases=33/33
previewCases=33/33
outlierResolutionChecks=5/5
negativeExclusionChecks=1/1
cancelled=0
terminalReason=none
```

The former X/radial internal surface pattern must remain visually absent. EW-C1A.2 remains blocked until this evidence passes.


## EW-C1A.1a.6 — One-surface explicit-normalization parity

**Status:** implemented; static/compliance validation passed; Unity runtime and visual acceptance pending.

### Failure evidence and completed read-only review

- Unity report `Pasted text(129).txt` keeps current preview valid but fails both zero-Macro probes on `EdgeBevelPlane:18`, face `30`. The collinear-reinsertion path removes retained vertex `1`, resolves simplified triangles `0/2/5|2/3/5|3/4/5`, and attempts replacements `0/1/5 + 1/2/5`. The first replacement reports area `7.15370675E-07`, `normalDot=0`, and `NormalAgreement`; the second reports `normalDot=0.99999994`.
- The reported positions produce `cross(1-0,5-0)=(-3.79246489E-11,-1.01170237E-06,1.01169201E-06)`, magnitude `1.43075589E-06`, area `7.15377944E-07`, and explicit double-precision normalized agreement `0.9999999996` with authored normal `(0,-0.707106769,0.707106769)`. The triangle is finite, non-zero, above the current minimum area, and geometrically aligned; the reported zero dot is a normalization-path defect.
- `Game/Procedural/Masses/MassGenerator.EdgeWear.BoundedSingleEdge.cs::EvaluateOneSurfaceTriangleCandidate` currently accepts the cross product through `geometricNormal.sqrMagnitude > MinimumEdgeLengthSqr`, then computes agreement through `geometricNormal.normalized` and `authoredNormal.normalized`. Unity small-vector normalization can collapse the accepted cross product to zero.
- `TryResolveOneSurfaceTriangle` repeats the same mismatch after candidate certification by assigning `normalizedGeometricNormal = geometricNormal.normalized`.
- `Game/Procedural/Masses/MassGenerator.Types.cs::TryNormalizeMassVector` already owns the Generated Mass normalization contract: finite input, mathematically non-zero double-precision magnitude, explicit division, and finite near-unit output. `MassGenerator.MeshOutput.cs::BuildMeshData` and `ValidateGeneratedMassMeshData` already use this helper for authored and geometric render normals.
- All candidate-evaluation consumers were reviewed: direct boundary fan ranking, interval-DP triangle evaluation, tolerance-collinear local classification, reinsertion replacement certification, final indexed-triangulation certification, and fan diagnostics. All require the same robust normalization semantics; no caller signature or result type must change.
- `TryResolveOneSurfaceTriangle` is consumed only by `TryEmitOneSurfaceTriangle`; replacing its final normalization with the shared helper preserves winding and deviation ownership.
- Canonical documents reviewed: this checklist, `Generated_Mass_Framework.md`, `Generated_Mass_Edge_Wear_Recovery_Architecture.md`, and `Generated_Mass_Edge_Wear_Code_Inventory.md`. Relevant shared contracts reviewed: `MassGenerator.Types.cs`, `MassGenerator.MeshOutput.cs`, `MassGenerator.EdgeWear.Types.cs`, and all direct one-surface candidate/resolve callers.
- Repository comparison limitation: the supplied authoritative tree contains `314` files and no `.git` metadata. The pre-edit baseline is reconstructed from the accepted archive sequence through `GeneratedMass_EW-C1A.1a.5_ChangedFiles.zip`; source and canonical documents are compared against that byte-identical package chain rather than an unavailable `HEAD`.

### Approved file scope

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Game/Procedural/Masses/MassGenerator.EdgeWear.BoundedSingleEdge.cs`

No other code, shader, material, scene, prefab, recipe, asset, metadata, serialized control, mesh channel, generated input, dependency, or per-frame path may change.

### Implementation contract

1. `EvaluateOneSurfaceTriangleCandidate` must normalize both `geometricNormal` and `authoredNormal` through existing `TryNormalizeMassVector` before calculating `normalDot`.
2. A failed robust normalization returns `OneSurfaceTriangleCandidateFailure.NonFinite`; area and normal-agreement classifications remain otherwise unchanged.
3. `TryResolveOneSurfaceTriangle` must normalize the post-winding geometric normal through `TryNormalizeMassVector` rather than `Vector3.normalized`.
4. Direct fan, complete interval DP, tolerance-collinear simplification/reinsertion, deterministic ranking, minimum triangle area, `OneSurfaceMinimumRenderNormalDot`, authored normal/group ownership, and final `n - 2` certification remain byte-for-byte unchanged outside these normalization expressions.
5. `MassGenerator.Types.cs::TryNormalizeMassVector` and `MassGenerator.MeshOutput.cs` remain unchanged. The one-surface producer must match their existing semantics; the final mesh guard remains authoritative and unchanged.

### File-by-file implementation sequence

1. [x] Complete and record the read-only source/caller/consumer/document review and exact five-file plan before source edits.
2. [x] Replace candidate normal normalization with `TryNormalizeMassVector` parity and preserve existing failure taxonomy.
3. [x] Replace final oriented-triangle normalization with `TryNormalizeMassVector` parity.
4. [x] Reconcile framework, recovery architecture, and code inventory with the narrow normalization correction and unchanged triangulation ownership.
5. [x] Reread all five final files and affected unchanged callers/consumers; compare final source with EW-C1A.1a.5 and record every intentional difference.
6. [x] Run all available parser, preprocessor, reference, scope, import, formatting, numerical mirror, package-overlay, strict-patch, and ZIP-integrity checks.
7. [ ] Unity compile, four Macro probes, topology/preview `33/33`, outlier `5/5`, negative exclusion `1/1`, and former-X visual confirmation remain pending user validation.

### Invariants and non-goals

- Preserve the EW-C1A.1a.5 triangulation architecture without another fallback, threshold, or topology change.
- Preserve `MinimumEdgeLengthSqr`, `minimumTriangleArea`, and `OneSurfaceMinimumRenderNormalDot`.
- Preserve every original retained boundary vertex and segment, exact `n - 2` output, zero internal fan vertices, analytical authored normals, and stable surface groups.
- Preserve final `MassGenerator.MeshOutput.cs` winding and `0.5` render-normal validation unchanged.
- Preserve EW-V1A.3b Macro width, candidate selection, topology, recovery, outlier, negative-exclusion, shader/material, serialized, and production behavior.
- Add no control, dependency, allocation structure, diagnostics schema, runtime callback, or per-frame work.

### Risks and controls

- **Risk:** robust normalization accepts a mathematically non-zero but unusably small triangle. **Control:** the existing finite-cross gate and `area > minimumTriangleArea` certification remain before agreement evaluation, and the unchanged final mesh guard uses the same robust helper.
- **Risk:** candidate and final emission normalize differently. **Control:** both paths call the same existing `TryNormalizeMassVector` owner.
- **Risk:** failure taxonomy changes unexpectedly. **Control:** robust normalization failure maps to existing `NonFinite`; `Area` and `NormalAgreement` conditions remain unchanged.
- **Risk:** scope drifts into the shared helper or mesh output. **Control:** both files are read-only comparison owners and are excluded from the approved write scope.

### Post-implementation consistency and compliance result

- Exact scope: all and only the five approved files differ from the reconstructed EW-C1A.1a.5 baseline; no file was created, deleted, moved, renamed, or given metadata.
- Source delta: `MassGenerator.EdgeWear.BoundedSingleEdge.cs` differs only at the two approved normalization blocks. Reversing those two blocks reproduces the EW-C1A.1a.5 file byte-for-byte after line-ending normalization. Imports, namespace, signatures, thresholds, triangulation order, types, diagnostics, and all surrounding logic remain unchanged.
- Shared owners: `MassGenerator.Types.cs`, `MassGenerator.MeshOutput.cs`, `MassGenerator.EdgeWear.Types.cs`, `MassGenerator.EdgeWear.Diagnostics.Logging.cs`, `MassGenerator.EdgeWear.BoundedAllEdges.cs`, and `MassGenerator.EdgeWear.PlaneCutKernel.cs` remain byte-identical. The final `minimumNormalDot < 0.5f` guard remains unchanged.
- Static/compliance validation passed `51/51`: the complete tree remains `314` files; all `185/185` C# files parse with tree-sitter; preprocessor blocks balance; exact scope, imports, BOM state, CRLF, trailing whitespace, Markdown fences, helper references, threshold preservation, and absence of Unity `.normalized` in both corrected methods passed.
- Numerical mirror: the reported replacement triangle `0/1/5` has cross magnitude `1.43075588818E-06`, area `7.15377944088E-07`, and explicit normalized authored-normal agreement `0.999999999636`, which passes the unchanged area and `0.5` thresholds.
- Preliminary delivery validation: the five-file ZIP overlay and strict Git-format patch each reproduced all `314` final files byte-for-byte, and ZIP integrity passed. Final named artifacts are rebuilt and reverified after this status write.
- Unity compilation is unavailable in the supplied source-only environment. The four Macro probes, both `33/33` matrices, outlier `5/5`, negative exclusion `1/1`, and former-X visual confirmation remain pending. EW-C1A.2 remains blocked.

### Acceptance criteria

Static acceptance:

- exactly the five approved files differ;
- all supplied C# files parse and preprocessor blocks balance;
- no import, namespace, caller signature, serialized, shader, material, asset, mesh-output, or triangulation-architecture change occurs outside scope;
- both one-surface normalization sites use `TryNormalizeMassVector` and no `geometricNormal.normalized` or `authoredNormal.normalized` remains in those methods;
- the reported `0/1/5` triangle mirror produces finite agreement approximately `1` and passes the unchanged area and `0.5` thresholds;
- `TryNormalizeMassVector`, `BuildMeshData`, `ValidateGeneratedMassMeshData`, final `0.5` guard, and EW-C1A.1a.5 direct/DP/reinsertion code remain unchanged;
- changed-files overlay and unified patch reproduce the final `314`-file tree byte-for-byte.

Unity acceptance:

```text
strengthZeroPassed=1
coverageZeroPassed=1
currentPassed=1
maximumPassed=1
macroZeroParity=1
macroRetention=1
topologyCases=33/33
previewCases=33/33
outlierResolutionChecks=5/5
negativeExclusionChecks=1/1
cancelled=0
terminalReason=none
```

The former X/radial internal surface pattern must remain visually absent. EW-C1A.2 remains blocked until this evidence passes.


## EW-C1A.1a.7 — Post-transform authored surface-normal rebuild

**Status:** implemented; static/compliance validation passed; Unity runtime and visual acceptance pending.

### Objective and acceptance boundary

Close the final EW-C1A.1a render-infrastructure blocker by resolving one shared authored surface normal from the final transformed triangle positions for every authored surface group. Preserve the complete EW-C1A.1a.6 triangulation architecture and the final `0.5` triangle/render-normal agreement guard. After complete Unity acceptance, freeze polygon render ownership and return directly to EW-C1A.2 visible corner-cut and cap-ring chip shaping.

### Failure evidence and completed read-only review

- Unity report `Pasted text(130).txt` passes the complete Macro contract: `macroVariationContractStatus=passed`, `macroZeroParity=1`, `macroAngleMapping=1`, `macroDeterminism=1`, `macroDistribution=1`, and `macroRetention=1`.
- The same report advances into the topology matrix and passes `32/33`. The sole failure coordinate is `seed=6667/width=maximum`, with `InvalidOperationException: Generated mass triangle 0 contains a render normal that disagrees with its winding.` Current seed `8889` remains valid with `polygonSurface.renderValid=1`, `planeSurface.renderValid=1`, exact authored-channel counts, and zero internal fan vertices.
- `Game/Procedural/Masses/MassGenerator.cs::GenerateInternal` applies `ApplyDimensions` and then the resolved mass-placement frame to `soup.Positions` before calling `BuildMeshData`.
- `Game/Procedural/Masses/MassGenerator.MeshOutput.cs::ApplyDimensions` performs positive non-uniform scaling. `ApplyLean` is height-dependent shear. `ApplyGrounding` performs height-dependent vertical flattening and horizontal broadening. These operations change final triangle normals; grounding is not a single global linear transform.
- `MassGenerator.Types.cs::TriangleSoup` stores one pre-transform authored normal and one stable authored surface-group ID per emitted one-surface triangle.
- `MassGenerator.MeshOutput.cs::BuildMeshData` currently consumes the stored authored normal after all position transforms and uses it as the final render normal. It only flips triangle winding against that stored normal. It does not rebuild the normal from final transformed positions.
- `MassGenerator.MeshOutput.cs::ValidateGeneratedMassMeshData` calculates the geometric normal from final `MeshData.Vertices` and rejects minimum geometric/render-normal agreement below `0.5`.
- `Game/Procedural/Core/MeshBuilder.cs::ApplyToMesh` writes supplied `MeshData.Normals` directly and recalculates tangents only. It does not repair stale authored normals.
- All grouped triangle producers were reviewed. Authored surface groups are emitted by `MassGenerator.EdgeWear.BoundedSingleEdge.cs::TryEmitOneSurfaceTriangle`; each logical polygon receives one stable group and one shared authored plane normal. Ordinary legacy helpers emit no authored group.
- Historical comparison: the authoritative current `MassGenerator.MeshOutput.cs` SHA-256 is `6d907a5a54bb62d131633550528c9f0bf0446e729f26c4c6eea34187bd78dd88`, byte-identical to `Assets-Code-Archive(6).zip`. None of the EW-C1A.1a changed-file overlays modified this owner.
- Canonical documents reviewed: this checklist, `Generated_Mass_Framework.md`, `Generated_Mass_Edge_Wear_Recovery_Architecture.md`, and `Generated_Mass_Edge_Wear_Code_Inventory.md`. Direct caller/producer/consumer contracts reviewed: `MassGenerator.cs`, `MassGenerator.MeshOutput.cs`, `MassGenerator.Types.cs`, `MassGenerator.EdgeWear.BoundedSingleEdge.cs`, `MeshData.cs`, and `MeshBuilder.cs`.
- Repository limitation: the supplied authoritative tree contains `314` files and no `.git` metadata. The pre-edit baseline is the byte-identical EW-C1A.1a.6 final overlay; comparison to `HEAD`, Git status, and Git history is unavailable.

**High-confidence hypothesis:** the seed-6667 maximum-width failure is caused by pre-transform authored plane normals being compared with post-transform triangle geometry. Evidence: the pre-transform polygon-surface audit passes, all position transforms occur after triangle-soup construction, grouped authored normals are not transformed or rebuilt, and the only failure occurs in the final `MeshData` normal/winding validator. Unity evidence with the added group-level diagnostics will verify or falsify this hypothesis.

### Approved file scope

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Game/Procedural/Masses/MassGenerator.MeshOutput.cs`

No other code, shader, material, scene, prefab, recipe, asset, metadata, serialized control, source topology, triangle-soup channel, mesh channel, dependency, generated input, or runtime callback may change.

### Implementation contract

1. `BuildMeshData` performs a deterministic prepass over final transformed `TriangleSoup.Positions` for triangles with an authored surface group.
2. Each grouped triangle must also have a finite explicitly normalized authored source normal. Its final geometric cross product must be finite and mathematically non-zero.
3. Orient each raw final geometric cross product into the hemisphere of that triangle's original authored normal, then accumulate the raw cross product by surface group in double precision. Raw cross products provide area weighting.
4. Explicitly normalize each group sum through existing `TryNormalizeMassVector`. The result is the sole final render normal for every triangle and rendered vertex in that group.
5. During the existing output pass, grouped triangle winding is resolved against the rebuilt group normal. Each grouped triangle must independently satisfy normalized geometric/rebuilt-group agreement `>= 0.5` before vertex emission.
6. A grouped-triangle failure must report triangle index, group ID, agreement, original authored normal, rebuilt group normal, final oriented geometric normal, and the three final transformed positions.
7. Ungrouped triangles retain existing geometric-normal behavior. An authored normal without an authored group retains existing behavior; no new grouping inference is introduced.
8. `ValidateGeneratedMassMeshData` remains unchanged and authoritative. Its existing `minimumNormalDot < 0.5f` rejection remains the final complete-mesh guard.
9. Surface-group IDs, material-variation hashing, feature identity/strength, triangle indices, positions, UVs, colours, UV2, tangents, and all EW-C1A.1a.6 triangulation paths remain unchanged.

### File-by-file implementation sequence

1. [x] Complete and record the read-only implementation/caller/producer/consumer/document/historical review and exact five-file plan before source edits.
2. [x] Add a final-position authored-group normal accumulator and resolver inside `MassGenerator.MeshOutput.cs`.
3. [x] Make `BuildMeshData` consume the rebuilt group normal and add detailed grouped-triangle agreement evidence without weakening the final validator.
4. [x] Reconcile framework, recovery architecture, and code inventory with final-space normal ownership and the explicit return-to-C1A.2 boundary.
5. [x] Reread every complete modified file and affected unchanged caller/producer/consumer; compare final source against EW-C1A.1a.6 and the historical archive owner.
6. [x] Run all available parser, preprocessor, reference, scope, import, formatting, deterministic numerical, package-overlay, strict-patch, and ZIP-integrity checks.
7. [ ] Unity compile, Macro preservation, topology/preview `33/33`, outlier `5/5`, negative exclusion `1/1`, and former-X visual confirmation remain pending user validation.

### Invariants and non-goals

- Preserve direct fan, complete interval DP, tolerance-collinear simplification/reinsertion, deterministic ranking, original-boundary preservation, exact `n - 2` output, and zero internal fan vertices.
- Preserve one logical polygon = one surface group = one final shared render normal.
- Preserve `TryNormalizeMassVector`, all triangle-area thresholds, `OneSurfaceMinimumRenderNormalDot`, and the final `0.5` mesh-output agreement threshold.
- Preserve all geometry positions and indices. This patch changes only the authored render normal selected after those positions are final.
- Preserve EW-V1A.3b Macro width, candidate selection, topology, recovery, outlier, negative-exclusion, shader/material, serialized, and production behavior.
- Add no authoring control, shader input, texture, buffer, mesh channel, source-file dependency, collider mutation, or per-frame work.
- Do not begin artistic normal shaping. EW-N1 remains after topology-changing chip geometry. This patch is final render-space correctness only.

### Risks and controls

- **Risk:** an authored group contains conflicting polygon identities. **Control:** existing one-surface surface-group collision audits remain authoritative; every final grouped triangle must also pass the unchanged `0.5` validator.
- **Risk:** nonlinear grounding makes one logical polygon too non-planar for one shared normal. **Control:** rebuild from area-weighted final triangle normals and reject any individual triangle below `0.5`; do not silently split the group or weaken the threshold.
- **Risk:** numerical cancellation in group accumulation. **Control:** accumulate raw cross components in double precision and normalize through `TryNormalizeMassVector`.
- **Risk:** winding and accumulation use different orientation semantics. **Control:** both use the final transformed geometry; accumulation uses the original authored normal only as a deterministic hemisphere guide, while final output winding and certification use the rebuilt group normal.
- **Risk:** material masks change. **Control:** exposure/crevice/deposit masks intentionally consume the corrected final-space shared normal; surface variation group hashing and every non-normal input remain unchanged.
- **Risk:** runtime cost. **Control:** one additional `O(T)` build-time pass and `O(G)` temporary group state, where `T` is triangle count and `G` is authored polygon-group count; no per-frame owner exists.


### Post-implementation consistency and compliance result

- Exact scope: all and only the five approved files differ from EW-C1A.1a.6; no file was created, deleted, moved, renamed, or given metadata.
- Source delta: `MassGenerator.MeshOutput.cs` adds one private double-precision group accumulator, one final-position resolver, one grouped-triangle pre-emission validator, invariant evidence formatting, and grouped-normal consumption inside `BuildMeshData`. No import, namespace, public/internal signature, transform, position, index, feature, material hash, mesh channel, or caller changes.
- Final validator: `ValidateGeneratedMassMeshData` is text-identical to EW-C1A.1a.6 and retains `minimumNormalDot < 0.5f`.
- Unchanged owners: `MassGenerator.cs`, `MassGenerator.Types.cs`, `MassGenerator.EdgeWear.BoundedSingleEdge.cs`, `MassGenerator.EdgeWear.Types.cs`, `MassGenerator.EdgeWear.Diagnostics.Logging.cs`, `MeshData.cs`, and `MeshBuilder.cs` remain byte-identical.
- Static/compliance validation passed `42/42`: complete tree `314/314`; C# parse `185/185`; preprocessor/region balance; exact scope; BOM/CRLF/trailing-whitespace/fence hygiene; no new imports; final-position prepass, double accumulation, area weighting, rebuilt lookup, winding, detailed evidence, and unchanged final guard checks.
- Numerical mirrors passed: a transformed planar group resolves identical final triangle/group normals; a synthetic transform-space case has stale authored-normal agreement about `0.3956` (rejected by `0.5`) while the rebuilt group normal agrees at `1.0`.
- Preliminary changed-files overlay and strict patch application each reproduced all `314` files byte-for-byte; ZIP integrity passed. Final artifacts are rebuilt after this status write.
- Unity compilation is unavailable in the supplied source-only environment. The one-click suite and former-X visual confirmation remain pending. Until they pass, EW-C1A.2 remains blocked.

### Static acceptance criteria

- exactly the five approved files differ from the EW-C1A.1a.6 baseline;
- all supplied C# files parse and preprocessor blocks balance;
- `BuildMeshData` resolves grouped final normals from transformed positions before output;
- accumulation uses raw area-weighted crosses and double-precision components;
- every grouped final triangle is certified against its rebuilt normal at `0.5` before emission;
- `ValidateGeneratedMassMeshData` and its final `0.5` condition remain byte-for-byte unchanged;
- no source topology, transform, triangle-soup, material hash, mesh-channel, triangulation, threshold, shader/material, serialized, or per-frame behavior changes;
- changed-files ZIP overlay and strict patch application reproduce the complete final tree byte-for-byte.

### Unity acceptance criteria

```text
macroVariationContractStatus=passed
macroZeroParity=1
macroRetention=1
topologyCases=33/33
previewCases=33/33
outlierResolutionChecks=5/5
negativeExclusionChecks=1/1
cancelled=0
terminalReason=none
```

The former X/radial internal surface pattern must remain visually absent. After this gate passes, freeze EW-C1A.1a and proceed directly to EW-C1A.2 visible corner-cut, cap-ring bevel, and chip-shape integration.

## EW-C1A.1a.8 — Deterministic shared-normal feasibility resolver

**Status:** implemented; static validation passed; Unity validation pending.

### Objective

Replace the EW-C1A.1a.7 area-weighted-only final authored-group normal with a deterministic maximin shared-normal resolver. The resolver must preserve one logical polygon = one surface group = one final shared render normal and must not weaken the existing per-triangle `0.5` render-normal agreement contract. This is the final bounded C1A.1a infrastructure attempt. If the complete candidate search cannot reach `0.5`, the failure is definitive evidence that final placement deformation makes the one-shared-normal contract infeasible for that group; no triangulation, chip geometry, threshold, or material workaround is permitted in this patch.

### Runtime evidence and read-only review

- The complete user-provided `Pasted text(134).txt` report passes the complete Macro contract: `macroVariationContractStatus=passed`, `macroZeroParity=1`, `macroAngleMapping=1`, `macroDeterminism=1`, `macroDistribution=1`, and `macroRetention=1`.
- The same report reaches topology `32/33` and fails only seed `6667`, maximum width, group `979435520` (`0x3A610000`, encoded `SourceFace:0`), triangle `0`: `normalDot=0.44671616`, original authored normal `(1/-4.670168E-08/0)`, rebuilt area-weighted normal `(0.990835965/0.135062173/0.00150746643)`, final geometric normal `(0.419798046/0.237562269/-0.8759759)`, transformed positions `a=(0.7278668/0.00199273229/-0.4563232)`, `b=(0.69592905/0.103431523/-0.444118977)`, and `c=(0.634270668/0.750027061/-0.298312873)`.
- `Game/Procedural/Masses/MassGenerator.cs::GenerateInternal` applies `ResolveDimensions`, `ApplyDimensions`, and the resolved `MassPlacementFrame` before calling `BuildMeshData`. It remains unchanged.
- `Game/Procedural/Masses/MassGenerator.MeshOutput.cs::ResolveTransformedAuthoredSurfaceNormals` currently orients each final raw geometric cross toward its stored authored normal, sums raw crosses by group, normalizes the area-weighted sum, and treats that single candidate as authoritative.
- `Game/Procedural/Masses/MassGenerator.MeshOutput.cs::BuildMeshData` resolves grouped winding against the rebuilt normal, calls `ValidateTransformedAuthoredSurfaceTriangle`, and then emits the same group normal for every triangle vertex.
- `Game/Procedural/Masses/MassGenerator.MeshOutput.cs::ValidateGeneratedMassMeshData` independently rejects any final triangle whose minimum vertex-normal agreement is below `0.5`; this final validator is unchanged and remains authoritative.
- `Game/Procedural/Masses/MassGenerator.Types.cs::TriangleSoup` stores one authored normal and one stable authored surface-group ID per emitted triangle vertex. `TryNormalizeMassVector` performs explicit double-magnitude normalization and remains unchanged.
- `Game/Procedural/Masses/MassGenerator.EdgeWear.BoundedSingleEdge.cs::ResolvePolygonSurfaceGroup` encodes ordinary polygon provenance with prefix `0x3A710000`; group `0x3A610000` decodes to `SourceFace:0`. The EW-C1A.1a.6 direct fan, complete DP, tolerance-collinear reinsertion, explicit-normalization, and group-identity contracts remain unchanged.
- Direct consumers and related modules reviewed and unchanged: `MassGenerator.cs`, `MassGenerator.Types.cs`, `MassGenerator.EdgeWear.BoundedSingleEdge.cs`, `MassGenerator.EdgeWear.PlaneCutKernel.cs`, `MassGenerator.EdgeWear.Diagnostics.Logging.cs`, `GeneratedMassEditor.cs`, `MeshData.cs`, and `MeshBuilder.cs`.
- Historical comparison: EW-C1A.1a.7 added only the final-position area-weighted group-normal prepass, grouped pre-emission validation/evidence, and grouped-normal consumption inside `BuildMeshData`; EW-C1A.1a.6 and the archive used the stored authored normal directly. The current patch starts from the byte-identical EW-C1A.1a.7 overlay.
- Repository limitation: the supplied authoritative tree contains `314` files and no `.git` metadata. `HEAD`, Git status, and Git history are unavailable. File-level history is reconstructed from the accepted changed-file overlays and patches.

### Approved scope

Modify only:

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Game/Procedural/Masses/MassGenerator.MeshOutput.cs`

Create/delete/move/rename/generated repository files: none.

### Mathematical contract

For one authored surface group with final, explicitly normalized, original-authored-hemisphere-oriented triangle normals `n_i`, choose a unit shared normal `N` that maximizes:

```text
minimumAgreement(N) = min_i dot(N, n_i)
```

A feasible shared normal at the existing threshold satisfies:

```text
minimumAgreement(N) >= 0.5
```

If any candidate can satisfy the required `0.5` threshold, the corresponding spherical cap has radius at most `60°`; a minimum enclosing cap in this positive acceptance domain is supported by one, two, or three boundary normals. The deterministic candidate set is therefore complete for the required feasibility decision:

1. the EW-C1A.1a.7 area-weighted normalized sum;
2. each individual triangle normal;
3. each finite normalized pair sum `normalize(n_i + n_j)`;
4. each finite equal-angle triple axis `±normalize(cross(n_i - n_j, n_i - n_k))`.

Every candidate is evaluated against every triangle. Select by:

1. highest minimum triangle dot;
2. highest area-weighted average triangle dot;
3. lowest defining triangle-index tuple;
4. stable candidate-kind order.

Primary candidate ordering compares the exact evaluated minimum dot; a lower minimum can never win through tolerance. One small fixed epsilon is limited to deterministic worst-triangle/evidence tie classification. The acceptance threshold remains exactly `0.5`.

### Implementation contract

1. Replace the accumulator-only group state with final transformed per-group triangle evidence: triangle index, explicitly normalized oriented geometric normal, raw cross/area weight, first normalized original authored normal, and double-precision area-weighted sum.
2. Preserve the existing rule that each raw final geometric cross is oriented into the hemisphere of its triangle's stored authored normal before group scoring.
3. Evaluate the complete deterministic candidate set above. Do not use random search, iterative optimization, Unity `Vector3.normalized`, or an approximate grid.
4. Return the winning shared normal only when it is finite and its exact evaluated minimum dot is at least `0.5`.
5. If no candidate reaches `0.5`, fail before vertex emission with definitive threshold-infeasibility evidence: encoded/decoded group identity, triangle count, area-weighted candidate minimum/average/worst triangle, best enumerated feasibility candidate kind/defining indices/minimum/average/worst triangle, original authored normal, selected candidate normal, and every grouped triangle's index, normalized geometric normal, and area weight. Do not describe a below-threshold candidate as the exact unconstrained global optimum.
6. `BuildMeshData` continues to orient each triangle's output winding against the selected shared normal and continues to call the existing grouped-triangle validator before vertex emission.
7. `ValidateGeneratedMassMeshData` and its `minimumNormalDot < 0.5f` rejection remain byte-for-byte unchanged.
8. Surface-group IDs, positions, indices, triangulation, features, material variation hashing, UVs, colours, UV2, tangents, transforms, placement deformation, public/internal signatures, and editor suite ownership remain unchanged.

### File-by-file implementation sequence

1. [x] Complete and record the read-only implementation, caller, producer, consumer, validation, documentation, runtime-evidence, and accepted-overlay history review.
2. [x] Record this concrete five-file plan before source edits.
3. [x] Replace the area-weighted-only resolver in `MassGenerator.MeshOutput.cs` with deterministic group evidence and maximin candidate enumeration.
4. [x] Add definitive infeasibility and winning-candidate diagnostics without changing the existing final validator.
5. [x] Reconcile framework, recovery architecture, and code inventory with shared-normal feasibility ownership and the return-to-C1A.2 boundary.
6. [x] Reread every complete modified file and affected unchanged caller/producer/consumer; compare final source with EW-C1A.1a.7, EW-C1A.1a.6, and the archive owner.
7. [x] Run all available parser, preprocessor, reference, scope, import, formatting, deterministic mathematical/numerical, package-overlay, strict-patch, and ZIP-integrity checks.
8. [ ] Unity compile, Macro preservation, topology/preview `33/33`, outlier `5/5`, negative exclusion `1/1`, and former-X visual confirmation remain pending user validation.

### Invariants and non-goals

- Preserve one logical polygon = one stable authored surface group = one final shared render normal.
- Preserve the direct fan, complete interval DP, tolerance-collinear simplification/reinsertion, explicit normalization, original boundary, exact `n - 2` output, and zero synthetic centre vertices.
- Preserve all geometry, placement deformation, source topology, triangle order, material identity, feature identity/strength, and render channels.
- Preserve all existing `0.5` agreement checks. Do not lower, bypass, soften, or reinterpret the threshold.
- Do not split an authored surface group, restore per-triangle normals, edit source faces, change grounding, alter culling/winding policy, or add artistic normal shaping.
- Do not begin C1A.2 chip geometry in this patch. Full Unity acceptance freezes C1A.1a and returns directly to C1A.2.
- Add no serialized control, shader/material input, texture, buffer, asset, dependency, component, tag, layer, or per-frame work.

### Risks and controls

- **Risk:** candidate enumeration is incomplete or failure evidence overstates a below-threshold optimum. **Control:** enumerate one-, two-, and three-support spherical-cap candidates plus the current area-weighted baseline; this is complete for existence of a `>= 0.5` cap. Verify against deterministic analytic fixtures and random positive-cap numerical optimization. Label below-threshold output as the best enumerated feasibility candidate, not the exact unconstrained optimum.
- **Risk:** pair or triple construction is degenerate. **Control:** skip only candidates that fail existing explicit normalization; individual normals and the area-weighted baseline remain available.
- **Risk:** tie behavior changes between platforms or an epsilon masks the true optimum. **Control:** exact minimum dot is always primary and cannot be overridden; area-weighted average, lexicographic defining indices, and stable candidate-kind order apply only after exact primary equality. The fixed epsilon is limited to worst-triangle/evidence ties.
- **Risk:** a group is genuinely infeasible. **Control:** fail with complete per-triangle evidence; do not weaken the threshold or silently split the group.
- **Risk:** candidate cost expands dirty-time. **Control:** per-group enumeration is `O(m^4)` worst-case time from triple candidates evaluated across `m` triangles and `O(m)` temporary group memory; observed authored polygon groups are small and there is no per-frame owner.
- **Risk:** material masks change. **Control:** only the final shared normal can change; surface-group hashing and every non-normal material input remain unchanged.

### Static acceptance criteria

- exactly the five approved files differ from EW-C1A.1a.7;
- all C# files parse and preprocessor/region blocks balance;
- candidate enumeration includes area-weighted, individual, all finite pair-bisector, and both finite triple equal-angle centres;
- selection uses exact minimum dot, then area-weighted average dot, defining indices, and stable kind order; no tolerance may override a higher minimum;
- accepted normals have evaluated minimum dot `>= 0.5` before `BuildMeshData` emits vertices;
- infeasible groups report complete group/candidate/per-triangle evidence;
- `ValidateGeneratedMassMeshData` remains byte-for-byte unchanged;
- no triangulation, geometry, transform, placement, group ID, material hash, mesh channel, threshold, shader/material, serialized, editor-suite, or per-frame behavior changes;
- changed-files ZIP overlay and strict patch application reproduce the complete final tree byte-for-byte.

### Unity acceptance criteria

```text
macroVariationContractStatus=passed
macroZeroParity=1
macroRetention=1
topologyCases=33/33
previewCases=33/33
outlierResolutionChecks=5/5
negativeExclusionChecks=1/1
cancelled=0
terminalReason=none
```

The former X/radial internal surface pattern must remain visually absent. If the best enumerated feasibility candidate remains below `0.5`, stop C1A.1a and use the new proof to plan a separately approved placement-deformation correction. If the gate passes, freeze C1A.1a and proceed directly to EW-C1A.2 visible corner-cut, cap-ring bevel, and chip-shape integration.

### Implementation and available verification result

- `MassGenerator.MeshOutput.cs` now retains final transformed per-triangle unit normals and raw-cross area weights for each authored surface group, preserves the existing authored-normal hemisphere orientation, and enumerates the area-weighted baseline, every individual triangle normal, every finite pair bisector, and both finite equal-angle axes for every triangle triple.
- Candidate selection uses the exact evaluated minimum triangle dot as the primary score, then exact area-weighted average dot, lexicographic defining triangle indices, and stable candidate-kind order. `AuthoredSurfaceNormalScoreTieEpsilon` is used only to choose deterministic worst-triangle evidence and cannot make a lower primary score win.
- The resolver accepts only a finite candidate with exact `minimumDot >= 0.5f`. A below-threshold group fails before vertex emission with encoded and decoded surface-group identity, original authored normal, area-weighted candidate, best enumerated feasibility candidate, and every triangle index, normal, and area weight.
- `BuildMeshData`, grouped triangle validation, `ValidateGeneratedMassMeshData`, vertex/material emission, transforms, positions, indices, surface-group IDs, mesh channels, triangulation, Macro width, placement deformation, shaders/materials, serialized controls, editor suite routing, and per-frame ownership remain unchanged from EW-C1A.1a.7.
- Complete modified-file and affected caller/producer/consumer rereads found no additional required source owner. Exactly the five approved files differ from the EW-C1A.1a.7 baseline; the authoritative tree remains `314` files.
- Available static validation passed `40/40`: all `185` C# files parse, preprocessor/region blocks balance, imports and critical unchanged bodies match, CRLF/BOM/trailing-whitespace and Markdown fences are clean, surface-group decoding matches all encoded classes/provenances, analytic one/two/three-support fixtures pass, and `30` deterministic random positive-cap cases match multi-start numerical optimization with maximum gap `2.22044604925E-16`.
- Conservative dirty-time cost is `O(m^4)` per authored group and temporary group storage is `O(m)`; observed polygon groups are small and no per-frame path was added.
- Unity compilation, the complete one-click suite, and visual confirmation remain pending. Package-overlay, strict-patch, ZIP-integrity, and final artifact-hash results are recorded in the accompanying static-validation report after packaging.

#### EW-C1A.3a — Deterministic fully certified single-corner search

**Patch identifier:** `EW-C1A.3a`

**Status:** [implemented; static/compliance/package validation passed; Unity validation pending]

**Goal:** replace unconditional commitment to the highest-scoring eligible corner with one deterministic score-ordered search that accepts the first corner capable of completing the entire single-chip feature. Each corner retains the existing four bounded depth trials. A transaction-certified corner then receives one shared cap-ring width schedule; every schedule step applies one uniform multiplier to all three cap-ring edges. The corner is accepted only when the transaction, complete mandatory cap ring, post-chip bevel construction, unrelated-bevel retention, final render mesh, and channel validation all pass. Failure of one corner continues to the next ranked eligible corner. No multiple-corner generation, new control, Inspector workflow, button, shader, normal rule, triangulation rule, or production path is added.

##### Read-only evidence reviewed before implementation

- The authoritative current tree was reconstructed from `Assets-Code-Archive(11).zip` plus the accepted EW-C1A.2, EW-C1A.2a, EW-C1A.2b, EW-C1A.2c, and EW-C1A.3 overlays in chronological order. It contains `331` files and no `.git` directory, so branch, `HEAD`, status, and history evidence are unavailable.
- The user Unity `EW-C1A.3-suite` report completed without cancellation or terminal error. Frozen Macro, topology, ordinary preview, outlier, and negative-exclusion gates passed. The corner stage failed `14/33`; disabled parity passed `11/11`, while enabled cases passed only `3/22`.
- The matrix root breakdown is: `10` selected-corner transaction failures, `6` transaction-certified but incomplete-ring failures, `3` complete-ring but unrelated-retention failures, and `3` enabled passes. This proves that fixed rank-0 selection and one cap-ring requested width are insufficient.
- `EvaluateCornerDamageTransaction` currently gathers every eligible candidate but commits only the highest score, then runs four depth trials. It exposes no candidate rank and performs no fallback to the next eligible corner.
- `ResolveCornerDamageCapRingRequestedWidth` currently produces one common requested width for all ring edges. `BuildEdgeWearBevelCandidates` already treats the three cap-ring edges as mandatory, Macro-free candidates at that common width. Therefore full uniform ring-width search can be added without changing candidate construction or the bevel solver.
- `GenerateCornerDamageIntegrationPreview` currently evaluates one baseline and one fixed-corner integration attempt. This is the correct editor-only owner for score-ordered full certification search and can retain one baseline while trying deterministic candidate-rank/ring-scale attempts.
- `GenerateCornerDamageGeometryPreview` currently evaluates the raw transaction independently. It must use the same full-certification search result, then render only the accepted raw cut, so the raw and integrated endpoints select the same fully viable corner.
- `CompleteCornerDamagePreviewCapture` already proves transaction, mandatory ring, unrelated retention, and final preview acceptance. It is the authoritative per-attempt acceptance predicate and will gain search summary fields rather than duplicating geometry rules.
- `GeneratedMassEditor.EvaluateCornerChippingMatrixCase` already runs raw and integrated endpoints independently and checks selection equality, transaction, complete ring, retention, and channels. It requires only truthful search-stage/result reporting; no new button or stage is required.
- Complete affected owners and direct callers/consumers were reviewed: `AGENTS.md`; the four canonical Generated Mass documents; `MassGenerator.cs`; `MassGenerator.EdgeWear.Types.cs`; `MassGenerator.EdgeWear.SelectionAndCorners.cs`; `MassGenerator.EdgeWear.Orchestration.cs`; `MassGenerator.EdgeWear.Diagnostics.Logging.cs`; `GeneratedMass.cs`; `GeneratedMassEditor.cs`; `MassGenerator.PlaneCut.cs`; `MassGenerator.EdgeWear.BoundedSingleEdge.cs`; `MassGenerator.MeshOutput.cs`; `MeshData.cs`; and `MeshBuilder.cs`.

##### Approved file scope

**Modify:**

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Game/Procedural/Masses/MassGenerator.cs`
6. `Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
7. `Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs`
8. `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
9. `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

No file may be created, deleted, moved, or renamed. `GeneratedMass.cs`, `MassSurfaceFeatureGenerator.cs`, `MassGenerator.EdgeWear.Orchestration.cs`, clipping, candidate construction, bounded shell construction, triangulation, final normals, shaders, materials, assets, mesh channels, serialized controls/defaults, Inspector controls/actions, and production `EdgeWearEvaluationMode.None` remain unchanged.

##### Deterministic search contract

1. [x] Rank every eligible corner by the existing score descending, then normalized graph vertex index ascending. Do not change eligibility or scoring.
2. [x] For each ranked corner, run the existing depth factors `{1, 0.75, 0.5625, 0.421875}` in order. A corner with no certified transaction is rejected and search continues.
3. [x] For each transaction-certified corner, run one shared cap-ring multiplier schedule `{1, 0.75, 0.5625, 0.421875, 0.31640625, 0.25}`. Every attempt applies the same multiplier to all three ring edges. Partial rings remain rejected.
4. [x] Reuse one frozen ordinary baseline per integrated search. Accept a corner only when `CompleteCornerDamagePreviewCapture` proves one cap, dense construction provenance, mandatory ring expected/candidate/selected/built equality, successful post-chip preview, and zero unrelated bevel loss.
5. [x] Continue after transaction, ring, construction, or retention failure. Fail only after every eligible corner has been rejected at every applicable ring scale.
6. [x] The raw `Rebuild Corner Chip Preview` first performs the same full-certification search, then emits only the raw geometry for the accepted candidate rank. The integrated and raw endpoints must independently resolve the same accepted rank/vertex/trial/depth/ring identities.
7. [x] Search state must be scoped with `try/finally`, editor-synchronous, and restored after every generation attempt. No state may leak into disabled, ordinary, audit, or production generation.

##### Search diagnostics

Every corner status/report gains:

```text
candidateCorners
attemptedCorners
attemptedConfigurations
acceptedCornerRank
capRingCommittedScale
searchFailureStage
searchFailureReason
searchAttempts
```

The final failed result retains the deepest deterministic blocker in this order: unrelated retention, complete post-chip construction, cap-ring completion, transaction certification, candidate availability. The one-click CSV gains accepted rank, attempted corners/configurations, committed ring scale, and truthful failure stage. Existing aggregate fields remain for compatibility.

##### File-by-file implementation sequence

1. [x] Record this persistent plan as the first write.
2. [x] Add candidate-rank, search summary, and per-attempt evidence fields in `MassGenerator.EdgeWear.Types.cs` and the public corner status.
3. [x] Change `EvaluateCornerDamageTransaction` to select a requested eligible rank from the unchanged deterministic ordering and report that rank.
4. [x] Apply the scoped uniform ring multiplier in `ResolveCornerDamageCapRingRequestedWidth` without changing the base width limits or candidate solver.
5. [x] Add the full-certification search owner in `MassGenerator.cs`; reuse one integrated baseline, search all ranked corners and scales, select the first complete pass, and preserve the deepest failure when none pass.
6. [x] Make geometry-only preview use the accepted full-search rank and render only that raw cut.
7. [x] Extend `MassGenerator.EdgeWear.Diagnostics.Logging.cs` with search summaries and exact failure-stage evidence.
8. [x] Extend the existing C1A.3 case/report rows in `GeneratedMassEditor.cs`; add no Inspector controls or buttons.
9. [x] Update framework, recovery architecture, and code inventory.
10. [x] Reread every modified file and affected owner; verify exact scope, disabled parity routing, state restoration, C# structure, frozen-owner hashes, package overlay reproduction, and unified patch reproduction. Mark Unity compilation/runtime validation pending.

##### Acceptance criteria

- [x] Exactly the nine approved files differ; `0` files are created, deleted, moved, or renamed.
- [x] Existing controls, Inspector layout, buttons, settings defaults, and disabled routing are unchanged.
- [ ] Every accepted enabled case reports `acceptedCornerRank >= 0`, `attemptedCorners >= 1`, `capRingCommittedScale > 0`, complete mandatory ring, and zero collateral loss.
- [x] A rejected rank-0 corner can fall through to a later deterministic rank. A cap-ring failure can fall through the uniform scale schedule before rejecting that corner.
- [ ] Raw and integrated endpoints independently select identical final rank, vertex, trial, depth, and cap-ring identities.
- [ ] The existing one-click C1A.3 matrix reaches `33/33` without relaxing transaction, ring, retention, topology, normal, tangent, or disabled-parity rules.
- [x] No active-gameplay, per-frame, shader, texture, buffer, cache, mesh-channel, or production-generation cost is added.
- [x] Available static/package checks pass. Unity 6000.5.0f1 compilation and the complete runtime suite remain pending until user validation.

##### Performance contract

- The search is explicit editor-only work. One integrated baseline is reused across all candidate/ring attempts in one search.
- Candidate gathering remains `O(V + E)` and frozen-tie-preserving deterministic ranking is `O(k²)` over the small eligible corner set `k`. Each ranked corner runs at most four existing transaction trials; only transaction-certified corners enter at most six uniform ring attempts.
- The raw preview intentionally performs the same complete search before rendering the selected cut, so it cannot display a corner that the normal edge-wear preview would reject.
- No recurring update, active-gameplay generation, shader work, persistent cache, texture, buffer, or mesh-channel change is permitted.
- The one-click matrix may take materially longer because it now exhausts valid alternatives instead of stopping after rank `0`. The report must expose attempted-corner/configuration counts and elapsed time. No hard runtime budget is invented; cancellation must remain responsive between matrix cases.


##### Implementation result

- Candidate eligibility and score are unchanged. Every eligible corner is now sorted once and addressable by deterministic rank.
- Full integration search reuses one ordinary baseline, exhausts the six common ring scales for each transaction-certified rank, and accepts only the first existing complete corner-preview certification. Candidate-local `InvalidOperationException` construction blockers advance deterministically as `post-chip-construction`; unexpected exception classes still propagate.
- Raw corner preview resolves the same full-feature candidate and then renders only the semantic cut.
- Reports and C1A.3a matrix rows expose eligible/attempted counts, accepted rank, committed ring scale, exact failure stage/reason, and the concise attempt trace.
- No control, Inspector layout, button, serialized default, clipping rule, candidate rule, bevel solver, triangulation rule, final-normal rule, shader, mesh channel, production callback, or per-frame owner changed.
- Static source/contract validation passed `43/43`; all `195` C# files passed delimiter and preprocessor/region checks.
- Exact scope is nine modified files across the unchanged `331`-file tree; frozen clipping, orchestration, shell, triangulation, final-normal, settings, shader, mesh-channel, and production owners remain byte-identical.
- The changed-files ZIP and unified patch each reproduced the complete final `331/331`-file tree byte-for-byte, and ZIP CRC validation passed.
- Unity compilation and the `33/33` runtime matrix remain pending.


#### EW-C1A.3e.1 — Missing topology-job coverage snapshot compile correction

**Patch identifier:** `EW-C1A.3e.1`

**Status:** [implemented; static/compliance validation passed; Unity compile pending].

**Objective:** restore Unity compilation after EW-C1A.3e introduced ordinary-baseline materialization inside the topology matrix. `EvaluateEdgeWearViabilityMatrixCase` consumes `job.EdgeWearCoverage`, but `EdgeWearViabilityMatrixJob` does not currently declare or initialize that immutable snapshot. Add the missing field without changing matrix semantics, settings, budgets, geometry, or runtime behavior.

**Approved files:**

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

**Reviewed evidence:**

- Unity compiler error: `GeneratedMassEditor.cs(1507,25): CS1061`, reporting that `EdgeWearViabilityMatrixJob` has no `EdgeWearCoverage` definition.
- `GeneratedMassEditor.cs::EvaluateEdgeWearViabilityMatrixCase` uses `job.EdgeWearCoverage` only for the new ordinary baseline settings; the topology audit settings intentionally retain exhaustive coverage `2f`.
- `GeneratedMassEditor.cs::EdgeWearViabilityMatrixJob` snapshots `EdgeWearAmount`, Macro settings, softness, and crease settings from `GeneratedMass`, but omits `EdgeWearCoverage`.
- `CornerChippingMatrixJob` already snapshots `target.EdgeWearCoverage`, confirming the authoritative source and immutable-job pattern.
- The accepted EW-C1A.3d baseline and final EW-C1A.3e diff were reviewed; this unresolved member was introduced only by EW-C1A.3e topology-baseline reuse.

**Invariants and non-goals:**

- No change to corner search, authoritative plans, topology audit coverage, artistic selection, baseline fingerprints, budgets, reports, Inspector controls, serialized settings, geometry, shaders, runtime, or production generation.
- The field is immutable and captured once from `GeneratedMass.EdgeWearCoverage`, matching the existing job-snapshot contract.
- No unrelated cleanup or refactor.

**Implementation sequence:**

1. [x] Add `EdgeWearCoverage` to `EdgeWearViabilityMatrixJob` beside the other immutable edge-wear settings.
2. [x] Initialize it from `target.EdgeWearCoverage` in the job constructor.
3. [x] Verify every `job.EdgeWearCoverage` reference resolves to that field and no other job/settings member is missing.
4. [x] Reread the complete modified class and direct topology/corner baseline consumers; run structural, reference, whitespace, diff, clean-apply, and package-replay checks.
5. [x] Unity compilation remains the external acceptance gate.

**Acceptance criteria:**

- Unity no longer reports CS1061 for `EdgeWearViabilityMatrixJob.EdgeWearCoverage`.
- `EvaluateEdgeWearViabilityMatrixCase` still uses `2f` for exhaustive topology audit coverage and the captured authored coverage only for ordinary baseline generation.
- Exactly the two approved files change; no runtime or generation behavior changes.

##### Implementation result

- `EdgeWearViabilityMatrixJob` now declares immutable `EdgeWearCoverage` beside `EdgeWearAmount` and initializes it from `target.EdgeWearCoverage`.
- `EvaluateEdgeWearViabilityMatrixCase` remains semantically split: the topology audit still uses exhaustive coverage `2f`, while only the retained ordinary baseline uses the captured authored coverage.
- Every `job.*` member referenced by `EvaluateEdgeWearViabilityMatrixCase` resolves against the complete `EdgeWearViabilityMatrixJob` contract; no second missing member was found.
- Exact scope is two modified files in the unchanged `331`-file C1A.3e tree. All other files are byte-identical.
- All `195/195` C# files pass lexical delimiter, preprocessor, and region checks. Modified files retain CRLF, one final newline, and no trailing whitespace.
- Clean overlay and unified-patch replay reproduce the final `331/331`-file tree byte-for-byte.
- Unity compilation remains pending; the concrete next action is to apply this correction and allow Unity to resume compilation.

#### EW-C1A.3e — Authoritative corner-integration plan and topology-baseline reuse

**Patch identifier:** `EW-C1A.3e`

**Status:** [implemented; static/compliance validation passed; Unity validation pending].

**Objective:** eliminate the EW-C1A.3d preflight/final-build divergence by making one committed corner-integration plan own the exact damaged topology, selected ordinary and mandatory identities, resolved widths, shell result, and emitted preview. Reuse matching default topology outputs as corner baselines so the corner matrix performs no duplicate ordinary baseline generation. Preserve the per-rock target of `<= 4 s`, hard maximum of `5 s`, corner-matrix `35 s`, and development-only complete-suite `90 s` boundary.

**Approved files:**

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
- `Assets/Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
- `Assets/Game/Procedural/Masses/MassGenerator.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Orchestration.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

**Reviewed evidence:**

- Unity report `Pasted text(142).txt` reports `totalElapsedMs=73901.5597`, `totalBudgetExceeded=0`, topology `33/33`, artistic fingerprints `33/33`, and a real corner stage that reached `22` cases before the `35 s` corner boundary.
- The same report records `cornerBaselineBuilds=8` and `cornerBaselineCacheUses=22`, proving the corner stage rebuilt one ordinary baseline for every reached seed instead of consuming the matching default topology result.
- Seven enabled cases report `integration-preflight-mismatch`; exact counts can agree while the final ordinary identity set differs, and other cases regress from preflight mandatory `3/3` to final `1/3` or `0/3`.
- `MassGenerator.GenerateCornerDamageFullCertificationSearch` currently retains a predictive `CornerDamageIntegrationPreflightRecord`, then calls `RunCornerDamageIntegrationAttempt`, which invokes `GenerateInternal` and independently repeats candidate discovery, width solving, coexistence, and shell construction.
- `BuildCornerDamageIntegrationPreflightRecord` currently stores only scalar predictions and identity arrays; it discards the prepared damaged faces, topology context, coverage lifecycle, and width solution that produced those predictions.
- `GeneratedMassEditor.GetOrBuildCornerChippingBaseline` keys only by shape seed and always calls `GenerateUnifiedEdgeWearPreviewBaseline` on the first policy for each seed. Topology audit generation already materializes a mesh and unified status but discards them.
- The supplied reconstructed archive has no Git metadata. The byte-identical EW-C1A.3d tree is the implementation baseline; EW-C1A.3c and EW-C1A.3d package diffs were reviewed to distinguish generator changes from validator-only scheduling changes.

**Invariants and non-goals:**

- No new Inspector controls, actions, serialized defaults, assets, shaders, mesh channels, production generation, runtime update work, or corner-count controls.
- No topology, retention, complete-ring, clipping, triangulation, final-normal, or deterministic ranking gate is weakened.
- The production `EdgeWearEvaluationMode.None` path remains unchanged.
- A planned shell may be emitted only when it contains all mandatory ring identities and zero unrelated baseline loss.
- Final emission must consume the committed plan; it must not rediscover candidates or rerun the corner-width/coexistence solver.
- Exact baseline reuse requires a seed/settings/mode fingerprint match; mismatch falls back to a local baseline build rather than stale reuse.

**Implementation sequence:**

1. [x] Extend the preflight record into an internal immutable `CornerDamageIntegrationPlan` carrying prepared damaged faces, topology context, coverage lifecycle, solved widths, exact planned identities, shell soup, and deterministic plan hash.
2. [x] Materialize candidate plans from the retained prepared state; validate exact mandatory and unrelated identity sets; continue deterministic corner ranking only when plan construction fails.
3. [x] Add a scoped editor-only plan override so final preview generation consumes the accepted shell soup/status without repeating candidate discovery, viability, width solving, coexistence, or shell construction.
4. [x] Replace generic mismatch telemetry with exact missing/unexpected ordinary and mandatory identities plus `integrationPlanHash` and `emittedPlanHash`.
5. [x] Preserve one final emitted mesh build, zero fallback builds, and per-rock `<= 4 s` target / `5 s` hard maximum reporting.
6. [x] Materialize one exact ordinary unified baseline alongside each topology default case, retain that mesh/status, and seed the corner cache by exact settings fingerprint; target `cornerBaselineBuilds=0` and `cornerBaselineCacheUses=33`.
7. [x] Update framework, recovery architecture, code inventory, report contracts, and acceptance gates.
8. [x] Complete exact-scope diff, full modified-file reread, caller/consumer consistency audit, structural checks, package replay, and Unity-pending declaration.

**Acceptance criteria:**

- `cornerChippingCases=33/33` and `cornerChippingMatrixBudgetExceeded=0`.
- `fullIntegrationBuilds <= 1`, `fullFallbackBuilds=0`, and final emission performs no solver rediscovery.
- `integrationPlanMismatches=0`; missing/unexpected planned ordinary and mandatory identity sets are empty for every accepted case.
- `integrationPlanHash` equals `emittedPlanHash` for every accepted case.
- `cornerBaselineBuilds=0` and `cornerBaselineCacheUses=33` when topology settings fingerprints match.
- Every enabled case remains below the `5 s` hard maximum; cases above `4 s` report over-target without being misclassified as hard-budget failures.
- Topology, Macro, artistic fingerprint, disabled parity, cap-ring, retention, normal, tangent, state-restoration, and production invariants remain unchanged.

##### Implementation result

- `CornerDamageIntegrationPreflightRecord` now retains the exact damaged faces, topology context, coverage lifecycle, corner-width solution, stable limits, and cap-ring width evidence that produced the candidate decision.
- The ranked search converts that retained state into one `CornerDamageIntegrationPlan` by running the complete shell kernel once. The plan owns the exact preview soup, unified status, ordinary and mandatory identity sets, unrelated-retention result, and deterministic plan hash.
- Accepted final preview emission consumes the committed plan through an editor-only scoped override. It clones the already-certified soup and status and does not rediscover candidates or repeat viability, width, coexistence, or shell solving.
- Emission validation reports exact missing and unexpected ordinary/mandatory identities together with `integrationPlanHash` and `emittedPlanHash`; any disagreement is an explicit `integration-plan-mismatch` failure.
- Each topology default case now additionally materializes and retains one ordinary unified baseline using the exact current edge-wear coverage/settings required by the corner matrix. The all-geometric topology audit mesh is never relabeled as ordinary. The corner matrix seeds an exact recipe/settings/mode fingerprint cache from the retained ordinary result and rebuilds only on a true fingerprint miss.
- Matrix telemetry distinguishes the `4 s` target from the `5 s` hard maximum and exposes plan attempts, plan mismatches, baseline builds/cache uses, timing partitions, and exact identity differences.
- The six normal Corner Chipping controls, existing preview actions, clipping, topology, complete-ring, retention, triangulation, final-normal, shader, serialized asset, runtime, and production-generation contracts remain unchanged.
- Exact scope is ten modified files in the unchanged `331`-file tree, with no create/delete/move/rename; frozen clipping, bounded-shell, triangulation, final-normal, settings, and production owners remain byte-identical.
- Targeted source/contract validation passed `97/97`; all `195/195` C# files passed delimiter, lexical-state, preprocessor, and region checks; modified files retain CRLF, one final newline, and no trailing whitespace.
- `git diff --check` passed with CR-at-EOL handling. The changed-files ZIP and unified patch each reproduced the complete final `331/331`-file tree byte-for-byte, and ZIP CRC validation passed.
- Unity compilation, `33/33` corner acceptance, zero plan mismatches, zero baseline rebuilds, and the per-rock runtime gate remain pending.


#### EW-C1A.3d — Validation-suite de-duplication and research scheduling

**Patch identifier:** `EW-C1A.3d`

**Status:** [implemented; static/compliance validation pending completion; Unity validation pending].

**Objective:** ensure the existing one-click validator reaches the EW-C1A.3c corner matrix instead of exhausting its global budget in already-passing duplicate regressions. Preserve the exhaustive 33-case topology matrix, derive a 33-case artistic-selection fingerprint contract from its captured edge records, move the corner matrix before artistic mesh materialization, and materialize only the fixed 12-case difficult artistic sentinel set inside the one-click suite. The standalone Artistic Preview Parity Matrix remains exhaustive. The development-only complete-suite hard stop increases from `58 s` to `90 s`; the enabled per-rock corner hard stop remains `4 s`, below the user's stated `5 s` maximum.

**Approved files:**

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
- `Assets/Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

**Reviewed evidence:**

- Unity report `Pasted text(140).txt` reports `totalElapsedMs=59008.2899`, `totalBudgetExceeded=1`, and `cornerChippingCases=0/0`; the 58-second boundary was exhausted before the corner stage.
- The same report records current preview `2487.1697 ms`, Macro contract `6445.3241 ms`, topology `33/33`, artistic preview `33/33`, and only `2.125 ms` in the unstarted corner stage.
- `GeneratedMassEditor.FinishEdgeWearViabilityMatrix` currently orders topology -> full 33-case artistic preview -> corner, so the active feature is scheduled after the duplicate materialization stage.
- `GeneratedMassEditor.EvaluateEdgeWearViabilityMatrixCase` uses `GenerateUnifiedEdgeWearBatchAuditCase` for topology and `GenerateUnifiedEdgeWearPreviewParityAuditCase` for artistic parity. The topology path includes every geometric candidate; it is not byte-equivalent to the ordinary artistic baseline used by `GenerateUnifiedEdgeWearPreviewBaseline`. Therefore the topology mesh cannot truthfully replace the corner matrix's existing one-per-seed ordinary baseline cache without changing semantics.
- Every topology result already captures `ArtisticEdges`, artistic eligibility, scores, and deterministic source-edge identities. These records are sufficient for a non-emitting 33-case artistic-selection fingerprint contract.
- Git metadata is absent from the supplied archive. The accepted reconstructed EW-C1A.3c tree is the comparison baseline.

**Invariants and non-goals:**

- No corner-search, clipping, cap-ring, bevel-construction, triangulation, final-normal, shader, serialized setting, Inspector-control, production-generation, or per-frame behavior changes.
- The standalone focused topology and artistic matrix buttons retain their existing exhaustive 33-case behavior.
- The one-click suite retains exhaustive topology `33/33`, artistic fingerprint `33/33`, and a fixed difficult artistic materialization set of seeds `1`, `2223`, `8889`, and `5727` at minimum/default/maximum width (`12` cases).
- The existing corner baseline cache remains one ordinary artistic baseline per seed. It is not replaced with the semantically different all-geometric topology mesh.
- The corner enabled-case hard stop remains `4 s`; the corner matrix hard stop remains `35 s`. Only the development-only whole-suite boundary increases to `90 s`.
- Any untouched stage after a budget/fail-fast exit reports `not-run`, never `failed` with `0/0` cases.

**Implementation sequence:**

1. [x] Add suite-owned artistic case scheduling so the standalone artistic matrix remains `33` cases while the one-click suite materializes only the fixed `12` sentinels.
2. [x] Derive and validate deterministic artistic-selection fingerprints for all `33` topology cases from their captured `ArtisticEdges`; compare all `12` sentinel fingerprints with their materialized artistic results.
3. [x] Reorder one-click stages to current preview -> Macro -> topology -> artistic fingerprint -> corner -> artistic sentinel materialization -> outlier/negative/comprehensive evidence.
4. [x] Increase only the complete-suite development hard stop to `90 s`; retain the `4 s` enabled-rock and `35 s` corner-matrix boundaries.
5. [x] Add truthful stage status/timing and `artisticFingerprintCases`, `artisticMaterializedCases`, remaining-budget, and cache-use telemetry to the combined report.
6. [x] Update framework, recovery architecture, and code inventory ownership/contracts.
7. [x] Complete exact-scope diff, full modified-file reread, caller/consumer consistency audit, structural checks, and delivery reproduction.

**Acceptance criteria:**

- The one-click suite reaches and executes the corner matrix before artistic mesh materialization.
- `topologyCases=33/33`, `artisticFingerprintCases=33/33`, and `artisticMaterializedCases=12/12`.
- The corner stage reports an actual result rather than `0/0`; untouched stages report `not-run` on early termination.
- `totalElapsedMs < 90000` and `totalBudgetExceeded=0` for acceptance.
- Enabled corner generation remains bounded by `4 s` per rock and the corner matrix remains bounded by `35 s`.
- Every frozen baseline gate remains unchanged.

**Implementation evidence:**

- The one-click stage order is now current preview -> Macro -> exhaustive topology -> 33-case artistic fingerprint -> corner matrix -> 12-case artistic sentinel materialization -> comprehensive evidence.
- Suite-owned artistic scheduling uses seeds `{1/2223/8889/5727}` across minimum/default/maximum widths; the standalone artistic action remains exhaustive over all 11 seeds and three widths.
- Fingerprint derivation consumes already-captured topology `ArtisticEdges` and emits no mesh. Sentinel materializations must match the corresponding deterministic rank/score fingerprint.
- Report contracts advance to `EW-C1A.3d-suite`, `EW-C1A.3d-33-case`, and `EW-C1A.3d-preview-sentinel`; untouched stages report `not-run`.
- The development-only global hard stop is `90,000 ms`; corner enabled-case and matrix hard stops remain `4,000 ms` and `35,000 ms` respectively.
- No generator, clipping, bevel, triangulation, normal, shader, settings, Inspector-layout, production, or per-frame owner changed.
- Unity compilation and runtime acceptance remain pending.
- Post-change audit: exactly the five approved files differ across the unchanged `331`-file tree; no file was created, deleted, moved, or renamed.
- Targeted source/contract validation passed `78/78`; all `195/195` C# files passed lexical delimiter, preprocessor, and region checks.
- The final projection clone is field-complete and non-destructive: topology audit records remain untouched while editor-only projected records carry the artistic rank/candidate contract.
- Frozen MassGenerator, corner-search, bevel-construction, triangulation, final-normal, GeneratedMass, settings, shader, asset, and production owners remain byte-identical to the accepted EW-C1A.3c baseline.
- Changed-files overlay and unified-patch replay each reproduce the complete final `331/331`-file tree byte-for-byte; ZIP CRC validation passes.

#### EW-C1A.3c — Complete integration preflight and one-final-build search

**Patch identifier:** `EW-C1A.3c`

**Status:** [implemented; static/compliance validation passed; Unity compilation, runtime matrix completion, and performance acceptance pending].

**Objective:** replace the remaining expensive candidate filter in EW-C1A.3b. Ranked corners must be evaluated with a complete non-emitting integration preflight. The search may execute exactly one final integrated mesh build for the highest-ranked preflight-certified corner. The outer fallback build is removed. Matrix and case budgets are tightened so the complete one-click suite remains below one minute.

**Approved files:**

- `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md`
- `Assets/Docs/Generated_Mass_Framework.md`
- `Assets/Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
- `Assets/Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
- `Assets/Game/Procedural/Masses/MassGenerator.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Orchestration.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs`
- `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
- `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

**Reviewed evidence:**

- Unity report `Pasted text(139).txt`: EW-C1A.3b processed `20/33` cases in `55,987.809 ms`, hit six `5 s` case budgets and the `55 s` matrix budget, while all frozen Macro/topology/ordinary-preview/outlier/negative gates passed.
- Seed `1/default`: ranking `0.176 ms`, transaction `1.835 ms`, ring preflight `15.916 ms`, integration `8647.886 ms`; the first integration plus fallback crossed the budget.
- Seed `5556/default`: ten cheap transaction/preflight attempts but three complete integrations plus two fallbacks consumed `4157.288 ms`.
- `MassGenerator.GenerateCornerDamageFullCertificationSearch` currently launches a complete integration for every ring-preflight-positive corner and may launch one lower-scale fallback.
- `CornerDamageRingPreflight` currently proves only the three isolated mandatory edges. It does not prove the ordinary selected identity set, topology-context readiness, local mandatory coexistence, or predicted unrelated retention.
- `GeneratedMassEditor.EvaluateCornerChippingMatrixCase` already caches one ordinary baseline per seed, but its `5 s` budget is checked only after an indivisible synchronous generation call returns.
- Git metadata is absent from the supplied archive. The accepted EW-C1A.3b reconstructed source is the comparison baseline.

**Invariants and non-goals:**

- No new controls, Inspector groups, buttons, serialized defaults, assets, shaders, mesh channels, production generation, or per-frame work.
- Corner cut remains after micro-topology normalization and before bevel candidate discovery.
- Existing transaction, stable identities, mandatory complete ring, unrelated-retention rule, bounded construction, polygon-surface triangulation, and final-normal contracts remain unchanged.
- No acceptance-gate weakening and no partial cap ring.
- Exactly one complete final integration build is permitted per enabled case. `FullFallbackBuildCount` must remain zero.
- If final integration disagrees with preflight, report `integration-preflight-mismatch`; do not search another full-build candidate in the same matrix case.

**Implementation sequence:**

1. [x] Add a complete integration-preflight record carrying transaction evidence, resolved shared ring scale, selected identity predictions, mandatory completion, topology-context readiness, candidate conservation, and predicted unrelated retention.
2. [x] Add an editor-only `CornerDamageIntegrationPreflight` evaluation mode that performs candidate discovery/selection and topology-context construction without shell triangulation or final mesh emission.
3. [x] Search all ranked corners with preflight only, retain the first fully certified record, then perform exactly one complete integration build. Remove the fallback build path.
4. [x] Compare the final build against the retained preflight fingerprint and report `integration-preflight-mismatch` on disagreement.
5. [x] Tighten enabled-case target/hard budgets to `2 s` / `4 s`, corner-matrix target/hard budgets to `25 s` / `35 s`, and add complete one-click-suite elapsed/hard-budget telemetry below `60 s`.
6. [x] Update canonical architecture/code ownership and report contracts to EW-C1A.3c.
7. [x] Complete exact-scope diff, caller/consumer reread, structural/static checks, package reproduction, and Unity-pending declaration.

**Acceptance criteria:**

- `fullIntegrationBuilds <= 1` and `fullFallbackBuilds = 0` for every enabled case.
- A corner reaches final integration only when preflight predicts complete ring, selected-candidate conservation, topology-context readiness, and zero unrelated baseline loss.
- A final/preflight disagreement reports `integration-preflight-mismatch`.
- Case hard budget `4 s`; corner matrix hard budget `35 s`; whole one-click suite hard budget below `60 s`.
- Disabled parity and every frozen edge-wear gate remain unchanged.
- Unity matrix completion and performance acceptance remain pending until the user runs the suite.

**Static completion evidence:**

- Exactly the ten approved files differ from the accepted EW-C1A.3b reconstructed baseline; no file was created, deleted, moved, or renamed.
- `CornerDamageIntegrationPreflight` performs full candidate discovery/selection, topology-context construction, candidate-conservation checking, non-emitting corner-width solving, mandatory-ring prediction, and ordinary-identity retention prediction.
- The ranked search executes preflight only until one corner passes, then permits exactly one final integration build. The fallback integration path is absent and `FullFallbackBuildCount` remains telemetry-only at zero.
- Matrix baseline timing and observed integration timing are passed into the case-budget admission gate; enabled cases use `4 s`, the corner matrix uses `35 s`, and the complete suite owns a `58 s` hard boundary with final-evidence reserve.
- All 195 C# files passed lexical delimiter and preprocessor checks; approved caller/consumer references and report contracts were reread. Unity compilation is not available in this environment and is not claimed.

#### EW-C1A.3b — Bounded staged single-corner certification

**Patch identifier:** `EW-C1A.3b`

**Status:** [implemented; static/compliance validation passed; Unity validation pending]

**Goal:** preserve the deterministic fully certified single-corner search introduced by EW-C1A.3a while eliminating its unacceptable editor cost. The user cancelled the first runtime matrix after `2:38` at seed `1`, case `3/33`; the two enabled cases had each taken approximately `80–84 s`. The active patch must keep the same transaction, complete-ring, unrelated-retention, topology, normal, tangent, and disabled-parity gates while finishing or hard-stopping the entire corner matrix in less than one minute.

##### Runtime evidence and diagnosed multiplier

- `Pasted text(138).txt` reports `status=cancelled`, `cornerChippingCases=3/3`, and `terminalReason=cancelled by user` after the progress dialog reached only seed `1`, maximum-depth.
- Seed `1` timings were `2890.7043 ms` disabled, `84189.3291 ms` default, and `80434.0495 ms` maximum-depth.
- Each enabled matrix case called both the raw geometry preview and integrated preview; each endpoint independently reran the complete candidate-rank × six-ring-scale search.
- Each scale attempt regenerated the entire post-chip edge-wear system even though the existing isolated viability preflight already reports every mandatory ring edge's maximum certified width.
- The previous report's `attemptedConfigurations` counted only one endpoint search and therefore understated the real number of complete generation attempts.

##### Approved file scope

**Modify:**

1. `Docs/Generated_Mass_Feature_Implementation_Checklist.md`
2. `Docs/Generated_Mass_Framework.md`
3. `Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md`
4. `Docs/Generated_Mass_Edge_Wear_Code_Inventory.md`
5. `Game/Procedural/Masses/MassGenerator.cs`
6. `Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs`
7. `Game/Procedural/Masses/MassGenerator.EdgeWear.Orchestration.cs`
8. `Game/Procedural/Masses/MassGenerator.EdgeWear.SelectionAndCorners.cs`
9. `Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs`
10. `Game/Procedural/Masses/Editor/GeneratedMassEditor.cs`

No file may be created, deleted, moved, or renamed. `GeneratedMass.cs`, `MassSurfaceFeatureGenerator.cs`, clipping, plane-cut geometry, bounded bevel construction, coexistence solving, polygon triangulation, final shared normals, shaders, materials, assets, serialized controls/defaults, Inspector layout/actions, mesh channels, and production `EdgeWearEvaluationMode.None` remain unchanged.

##### Bounded staged search contract

1. [x] Cache one frozen ordinary unified-preview baseline per shape seed across disabled/default/maximum-depth matrix policies. Disabled parity returns that exact mesh/status without a second generation.
2. [x] Evaluate ranked corners in the unchanged deterministic order. Each rank runs the existing bounded transaction depth trials once per staged preflight.
3. [x] Add an internal `CornerDamageRingPreflight` mode. It builds lifecycle/isolated-viability evidence only for the three mandatory cap-ring edges and returns before artistic selection, coexistence, shell construction, triangulation, or final mesh emission.
4. [x] Resolve one uniform ring scale from the minimum of the three existing isolated maximum-certified-width ratios, quantized downward through `{1, 0.75, 0.5625, 0.421875, 0.31640625, 0.25}`. Reject the corner when no scale at or above `0.25` certifies all three mandatory edges.
5. [x] Run one complete integrated construction at the resolved scale. Permit at most one next-lower uniform fallback only for cap-ring completion, post-chip construction, or unrelated-retention failure.
6. [x] Accept only the existing complete preview predicate: one certified semantic cap, dense construction provenance, mandatory expected/candidate/selected/built equality, successful final preview, zero unrelated bevel loss, and valid final channels.
7. [x] The 33-case matrix performs one integrated staged search per enabled case. It no longer invokes the raw preview. Selection determinism is checked with a lightweight repeated transaction fingerprint at the accepted rank.
8. [x] The interactive raw preview still performs the same staged certification once, then emits only the accepted semantic cut; it does not start a second independent full search.
9. [x] Preserve responsive cancellation between candidates/cases and restore all scoped candidate-rank/ring-scale state through `IDisposable`/`try-finally`.

##### Performance and telemetry contract

- Target per enabled case: `<= 2 s`; hard stop: `5 s`.
- Target complete corner matrix: `<= 40 s`; hard stop: `55 s`.
- A hard-stop breach reports `failureStage=performance-budget`, returns control, and preserves the selected object's mesh/preview state.
- Reports expose `baselineBuilds`, `baselineCacheUses`, `transactionAttempts`, `ringPreflightAttempts`, `fullIntegrationBuilds`, `fullFallbackBuilds`, `geometrySearchReuses`, candidate-ranking/transaction/ring-preflight/integration milliseconds, and case/matrix budget flags.
- The matrix CSV reports actual expensive build counts rather than the old hidden duplicated endpoint work.
- No gameplay, recurring update, shader, GPU, texture, buffer, persistent cache, mesh-channel, or production-generation cost is added.

##### File-by-file implementation result

1. [x] `MassGenerator.EdgeWear.Types.cs` owns staged preflight and performance telemetry records.
2. [x] `MassGenerator.EdgeWear.SelectionAndCorners.cs` times candidate ranking/transaction work and supports mandatory-ring-only viability preflight.
3. [x] `MassGenerator.EdgeWear.Orchestration.cs` routes the internal preflight mode through transaction and mandatory candidate viability, then exits before full bevel construction.
4. [x] `MassGenerator.cs` owns the bounded search, shared baseline input, resolved uniform scale, one optional fallback, lightweight selection fingerprint, and case hard stop.
5. [x] `MassGenerator.EdgeWear.Diagnostics.Logging.cs` captures staged records and prints exact build/timing/budget evidence.
6. [x] `GeneratedMassEditor.cs` caches one baseline per seed, removes the duplicate raw search from every matrix case, applies `5 s` case and `55 s` matrix gates, and advances report contracts to EW-C1A.3b.
7. [x] The four canonical documents record the performance regression, replacement architecture, ownership, and pending Unity gate.
8. [ ] Unity 6000.5.0f1 compilation and the complete runtime matrix remain pending user validation.

##### Acceptance criteria

- [x] Exactly the ten approved files differ; no file is created, deleted, moved, or renamed.
- [x] No control, Inspector section, action, serialized default, clipping rule, final geometry rule, shader, mesh channel, or production path changes.
- [x] Disabled cases use one cached baseline and exact mesh/status parity.
- [x] Enabled matrix cases perform no raw geometry search and at most one complete build plus one lower fallback for each attempted corner.
- [x] Mandatory-ring preflight evaluates only the three generated cap-ring candidates.
- [x] A successful case cannot be accepted after crossing the `5 s` hard budget.
- [x] The matrix aborts with `performance-budget` no later than the `55 s` hard stop.
- [ ] Complete runtime acceptance requires `33/33`, disabled parity `11/11`, selection determinism `33/33`, transaction/ring/retention/channel gates `33/33`, `caseBudgetExceeded=0`, `matrixBudgetExceeded=0`, cancellation `0`, and terminal reason `none`.

##### Static/compliance completion evidence

- [x] Exact scope is the ten approved files across the unchanged `331`-file source tree; no file was created, deleted, moved, or renamed.
- [x] Custom source/contract validation passed `81/81`, including staged-search routing, shared baseline ownership, mandatory-ring-only preflight, one integration plus at most one fallback, lightweight determinism fingerprinting, `5 s` case and `55 s` matrix hard stops, report telemetry, and absence of the raw geometry endpoint from matrix cases.
- [x] All `195/195` C# files passed lexical delimiter, comment/string, preprocessor, and `#region`/`#endregion` balance checks.
- [x] All changed files preserve CRLF, terminal newlines, BOM state, Markdown-fence balance, and contain no actual trailing spaces or tabs.
- [x] `GeneratedMass.cs`, settings transport, clipping, plane-cut shell, bounded triangulation, final mesh normals, shaders, serialized controls/defaults, Inspector layout/actions, mesh channels, and production generation remain byte-identical to EW-C1A.3a.
- [ ] Unity 6000.5.0f1 compilation, actual per-case/matrix timings, cancellation responsiveness, and complete `33/33` runtime acceptance remain pending; no runtime speedup is claimed from static evidence.

## EW-C1A.3f.1 compile correction — value-type audit guard

### Objective and acceptance criteria

- [x] Restore Unity compilation after C1A.3f compared the value-type `PlaneCutBevelAuditResult` snapshot to `null` inside `MaterializePlaneCutBevelSolvedPlan`.
- [x] Remove only the invalid value-type null comparison. Preserve the existing reference guards and `SolveValid` success contract.
- [x] Do not change plane solving, shell materialization, deadlines, identities, telemetry, budgets, Inspector behavior, serialized data, or production generation.

### Approved files

- [x] `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md` — canonical plan and final validation evidence.
- [x] `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.PlaneCutKernel.cs` — remove the invalid `solvedPlan.Audit == null` guard.

### Read-only evidence reviewed before implementation

- [x] Unity compiler error: `MassGenerator.EdgeWear.PlaneCutKernel.cs(1201,39): CS0019`, operator `==` cannot compare `PlaneCutBevelAuditResult` with `null`.
- [x] `PlaneCutBevelAuditResult` is declared as a private `struct` in `MassGenerator.EdgeWear.PlaneCutKernel.cs`; it is therefore non-nullable in this contract.
- [x] `PlaneCutBevelSolvedPlan.Audit` is a value-type field initialized from `result` when the solved plan is created.
- [x] `MaterializePlaneCutBevelSolvedPlan` already guards the plan reference, source faces, retained candidates, active identities, and `SolveValid`; no replacement null sentinel is required.
- [x] Direct callers in `MassGenerator.cs` and the combined `AuditPlaneCutBevelKernel` wrapper consume the returned value-type audit and do not require nullable behavior.
- [x] Synthetic C1A.3e.1 baseline commit `38fe7bf`, complete C1A.3f diff, `Assets/AGENTS.md`, canonical C1A.3f plan, and affected type/caller contracts were reviewed.

### Implementation sequence and invariants

1. [x] Delete only `solvedPlan.Audit == null ||` from the materialization guard.
2. [x] Confirm no other `PlaneCutBevelAuditResult` null comparisons exist.
3. [x] Run exact two-file scope, structural C# and preprocessor checks, diff hygiene, and ZIP/patch replay.
4. [x] Record Unity compilation as pending user validation.

### Risks and controls

- [x] Risk: removing the guard could admit an uninitialized audit. Control: a non-null `PlaneCutBevelSolvedPlan` is created only after `result` initialization and assigns `Audit = result`; `SolveValid` remains the explicit semantic gate.
- [x] Risk: scope drift into C1A.3f behavior. Control: exact two-file patch; only one executable expression changes.

### Validation status

- [x] Final diff is restricted to the two approved files; the executable diff removes one invalid value-type null comparison and changes no other code.
- [x] All `195/195` C# files pass delimiter, comment/string, preprocessor, and region-balance checks.
- [x] No `solvedPlan.Audit == null` or other `PlaneCutBevelAuditResult` null comparison remains.
- [x] Both modified files preserve CRLF, terminal newline, BOM state, and whitespace hygiene.
- [x] Changed-files ZIP and unified patch replay reproduce the final tree byte-for-byte from the untouched C1A.3f delivery baseline.
- [ ] Unity 6000.5.0f1 compilation pending user execution.

## EW-C1A.3f implementation plan — authoritative solve/materialization split

### Objective and acceptance criteria

- [x] Preserve the C1A.3e correctness result: authoritative planned and emitted ordinary/mandatory identity sets remain exact and `integrationPlanMismatches=0`.
- [x] Remove complete polygon-shell and triangle-soup construction from rejected candidate plans. Candidate search performs deterministic plane-and-rail solving only; only the accepted candidate may construct the shell and triangulate/materialize preview soup.
- [x] Split the existing plane-cut kernel into an authoritative plane-and-rail solved plan and a separate one-time shell/materialization step without changing candidate ordering, width schedules, coexistence, topology, face-quality, triangulation, or final mesh-channel rules.
- [x] Add cooperative case-deadline checks at bounded candidate, conflict-pass, and materialization boundaries so one synchronous attempt cannot silently run beyond the `5 s` hard maximum.
- [x] Materialize the topology-stage ordinary baseline with the authored `EdgeWearWidth`, not the topology audit case width `1f`, so exact cross-stage fingerprints can match.
- [ ] Expected Unity evidence: `cornerBaselineBuilds=0`, `cornerBaselineCacheUses=33`, `authoritativeSolveAttempts>=1` for enabled cases, `planMaterializationBuilds<=1`, `planMaterializationMismatches=0`, every enabled case `<5000 ms`, and corner matrix `33/33` within `35 s`.

### Approved files

- [x] `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md` — canonical plan, evidence, status, and final audit.
- [x] `Assets/Docs/Generated_Mass_Framework.md` — stable solve/materialization ownership and performance boundary.
- [x] `Assets/Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md` — authoritative solved-plan contract and one-time materialization.
- [x] `Assets/Docs/Generated_Mass_Edge_Wear_Code_Inventory.md` — exact code ownership.
- [x] `Assets/Game/Procedural/Masses/MassGenerator.cs` — ranked search, solved-plan ownership, one-time materialization, deadline scope, telemetry.
- [x] `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs` — solved-plan/materialization records and telemetry.
- [x] `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.PlaneCutKernel.cs` — split deterministic plane-and-rail solve from one accepted polygon-shell/triangle-soup materialization; preserve existing rules.
- [x] `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Orchestration.cs` — consume only materialized accepted plans during final preview emission.
- [x] `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs` — copy/report solve, materialization, deadline, and mismatch evidence.
- [x] `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs` — authored-width topology baseline snapshot, aggregate telemetry, matrix acceptance checks.

### Read-only evidence reviewed before implementation

- [x] Repository state: synthetic accepted C1A.3e.1 baseline commit `38fe7bf`; clean working tree before this plan edit.
- [x] Unity report `Pasted text(143).txt`: total `88,444.4819 ms`; topology `33/33`; corner matrix `3/9`; `cornerIntegrationPlanAttempts=19`; `cornerIntegrationPlanMismatches=0`; enabled plan time `647–8,600 ms`; final emission time `0 ms`; cross-stage baseline reuse failed (`cornerBaselineBuilds=3`).
- [x] `MassGenerator.cs` — `GenerateCornerDamageFullCertificationSearch`, `TryBuildCornerDamageIntegrationPlan`, `RunCornerDamageIntegrationPlanEmission`, identity/hash validation.
- [x] `MassGenerator.EdgeWear.Orchestration.cs` — prepared preflight capture and committed-plan early-return consumer.
- [x] `MassGenerator.EdgeWear.Types.cs` — preflight, integration-plan, and search telemetry contracts.
- [x] `MassGenerator.EdgeWear.PlaneCutKernel.cs` — `AuditPlaneCutBevelKernel` currently performs candidate solve, clean polygon-shell construction, topology/quality certification, triangulation, soup audit, and coverage finalization in one call.
- [x] `GeneratedMassEditor.cs` — topology baseline currently uses the matrix case `width`; the default topology case is named `default` but has value `1f`, while the corner matrix fingerprints authored `target.EdgeWearWidth`.
- [x] Direct callers/consumers: unified preview path, batch topology audit, corner integration preflight, committed-plan preview routing, matrix baseline seeding and exact fingerprint lookup.
- [x] Canonical framework, recovery architecture, code inventory, C1A.3e plan, AGENTS.md, and C1A.3d→C1A.3e diff.

### Invariants and non-goals

- [x] Production `EdgeWearEvaluationMode.None`, runtime callbacks, serialized settings/defaults, Inspector layout, controls, shaders, materials, assets, clipping/cap creation, source identity mapping, width schedules, coexistence rules, triangulation policy, authored normals/tangents, and mesh channels remain unchanged.
- [x] No new layers, tags, components, assets, folders, dependencies, per-frame work, or persistent cache.
- [x] The `4 s` target, `5 s` hard maximum, `35 s` corner matrix, and `90 s` research suite boundaries remain unchanged.
- [x] Failure is not hidden by relaxing retention, mandatory ring, topology, or identity parity rules.

### File-by-file sequence

1. [x] Introduce deadline scope/probe and solved-plan/materialization telemetry/types.
2. [x] Refactor the plane-cut kernel into deterministic polygon-shell solve and separate triangle-soup materialization while retaining the existing combined wrapper for unrelated callers.
3. [x] Change corner plan search to run solve-only for candidates and materialize exactly once after acceptance; validate exact identity/hash parity after emission.
4. [x] Correct topology-stage ordinary baseline settings to use the authored edge-wear width and record cross-stage cache hits.
5. [x] Update diagnostics and canonical architecture/inventory documents.
6. [x] Run scope, diff, structural C# and preprocessor checks, static contract checks, patch/ZIP replay, and record Unity validation as pending.

### Risks and controls

- [x] Risk: solve-only and materialization paths diverge. Control: materialization consumes the exact solved planes/rails/candidates/coverage; no rediscovery or identity selection; exact planned/emitted identity/hash comparison remains mandatory.
- [x] Risk: skipping triangulation during rejected solves could defer a render-contract failure to the accepted candidate. Control: accepted materialization remains a hard certification step; a failure reports `plan-materialization`, does not search another complete materialization, and preserves production fallback.
- [x] Risk: deadline probes alter unrelated audits. Control: probes are editor-only, active only inside corner search, and return false elsewhere.
- [x] Risk: stale topology baseline reuse. Control: exact recipe/settings fingerprint remains required; mismatch locally rebuilds.

### Validation and audit status

- [x] Final diff is restricted to the ten approved files.
- [x] Complete final modified files and affected callers/consumers reread.
- [x] C# delimiter/comment/string/preprocessor checks pass for all `195/195` source files.
- [x] Static contracts `103/103` prove one materialization path, authored-width baseline capture, deadline probes, exact C1A.3f telemetry/contracts, and unchanged frozen owners.
- [x] Changed-files ZIP and unified patch reproduce the final `331/331`-file tree byte-for-byte from commit `38fe7bf`; ZIP CRC passes with exactly ten entries.
- [ ] Unity compile and `EW-C1A.3f-suite` runtime validation pending user execution.

## EW-C1A.3g implementation plan — complete authoritative build and truthful ordinary-baseline fallback

### Objective and acceptance criteria

- [x] Record C1A.3f as runtime-rejected from `Pasted text(148).txt`: `currentPreviewPassed=0` and an all-zero public summary coexist with `planeBevel=30/30/30`, `polygonSurface.renderValid=1`, `planeSurface.renderValid=1`, and `planeMesh.renderValid=1`.
- [x] Preserve each ordinary baseline as one mesh/status/timing bundle. A supplied cached baseline is reusable only when both mesh and applied status are present; otherwise build one local ordinary unified baseline and retain its exact mesh and status.
- [x] Treat solve-only plane/rail output as candidate preparation, not final authority. Prepared identities may reject an obviously invalid candidate, but only the completed clean shell may own final ordinary/mandatory identities, retention counts, hashes, render validity, and preview soup.
- [x] Perform at most one complete authoritative shell construction per enabled rock. Final emission must consume the completed soup/status without rediscovery or a second solve.
- [x] Route every unsuccessful corner-enabled exit through one fallback helper that returns the exact ordinary baseline mesh and unified status while retaining a failed `CornerDamagePreviewStatus` with the real stage, diagnostic, search summary, and telemetry.
- [x] Add current corner-status evidence to the one-click suite so an ordinary-preview pass cannot hide the actual corner failure stage.
- [x] Preserve the frozen production path, controls, serialized data, shaders, assets, triangulation policy, normal/tangent ownership, width schedules, candidate ranking, and mandatory/unrelated-retention gates.

### Approved files

- [x] `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md` — first persistent write, evidence, concrete plan, implementation status, and final compliance audit.
- [x] `Assets/Docs/Generated_Mass_Framework.md` — replace the false solve-only authority boundary with complete-build authority and truthful fallback ownership.
- [x] `Assets/Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md` — define candidate preparation, one complete build, exact soup emission, and baseline fallback.
- [x] `Assets/Docs/Generated_Mass_Edge_Wear_Code_Inventory.md` — update exact C1A.3g code ownership.
- [x] `Assets/Game/Procedural/Masses/MassGenerator.cs` — baseline bundle, corrected candidate-preparation/complete-build boundary, post-build retention validation, centralized fallback, and updated failure priorities.
- [x] `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs` — separate prepared identity evidence from final authoritative identity evidence without adding serialized state.
- [x] `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs` — advance report contracts and label candidate preparation versus complete authoritative construction truthfully.
- [x] `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs` — pass the cached baseline mesh through the corner path, advance suite contracts, and include current corner-status evidence.

No file may be created, deleted, moved, renamed, generated, or modified outside this eight-file scope. `MassGenerator.EdgeWear.Orchestration.cs`, `MassGenerator.EdgeWear.PlaneCutKernel.cs`, `GeneratedMass.cs`, `MeshData.cs`, `MassGenerator.MeshOutput.cs`, all shaders/includes, materials, scenes, prefabs, assets, and metadata are reviewed unchanged owners.

### Read-only evidence reviewed before implementation

- [x] Source authority: `/mnt/data/Assets-Code-Archive(18).zip`, SHA-256 `862adafc137d885207ffb0debdeaaba0b6fb18dc96744830af24831a4e19088a`, containing `353` project files under `Assets`; no `.git` directory is present, so real branch, `HEAD`, status, diff, and history are unavailable. The archive includes the post-pause weather/cloud work and is authoritative over the handoff reconstruction.
- [x] Handoff: `GeneratedMass_EW-C1A.3g_Continuation_Handoff_2026-07-23(1).md`, SHA-256 `82f332e815c1a977b99f0a40d7ee824faf4f742c39b12597207b8a9dd7755a36`, records the same corrected boundary and forbids shader/cloud edits.
- [x] Runtime report `Pasted text(148).txt`: suite `EW-C1A.3f-suite`, total `8391.0696 ms`, current preview `1616.5212 ms`, fail-fast before topology/corner matrices, public status all zero/default, ordinary telemetry `30/30/30` and render-valid.
- [x] `MassGenerator.cs:1225-1736` — `GenerateCornerDamageFullCertificationSearch` builds or receives only baseline status, retains default unified failure state, returns production `Generate(...)` on no-plan, budget, materialization, and final-emission failures, and can pair non-preview geometry with preview status.
- [x] `MassGenerator.cs:2862-2905` — `GenerateUnifiedEdgeWearPreviewWithBaseline` receives cached `baselineMesh` but discards it before calling the corner path.
- [x] `MassGenerator.cs:1809-2132` — solve-only output is used to define planned identities before materialization; `MaterializePlaneCutBevelSolvedPlan` later performs the clean-shell build and can change final retained identities.
- [x] `MassGenerator.EdgeWear.PlaneCutKernel.cs:901-1609` — complete authority is reached only after conflict reduction, retained-candidate finalization, clean-shell construction, topology/face/coverage certification, triangulation, and final `TriangleSoup` creation.
- [x] `MassGenerator.EdgeWear.Orchestration.cs:103-134` — an already materialized committed plan is consumed without rediscovery by cloning its exact soup and status; this owner requires no edit.
- [x] `GeneratedMass.cs:2268-2308` — public Inspector/current-preview fields copy the returned unified status directly, explaining the observed all-zero summary when the corner wrapper returns default status.
- [x] `Editor/GeneratedMassEditor.cs:1948-1979` already caches mesh/status/timing together, but the generator API drops the mesh; `CaptureCurrentPreview` records only ordinary status and telemetry, not the current corner status.
- [x] `MeshData.cs` is a mutable managed data container with no disposal or native ownership. Returning the exact cached/local baseline object matches the existing corner-disabled baseline path and requires no clone/disposal protocol.
- [x] Canonical checklist, framework, recovery architecture, code inventory, `Assets/AGENTS.md`, direct callers, status producers/consumers, complete-build consumer, and validation suite contracts were reviewed before this first write.

### Invariants and non-goals

- [x] Production `EdgeWearEvaluationMode.None` remains byte-identical and no player/runtime callback changes.
- [x] No new Inspector controls, foldouts, debug views, serialized fields, defaults, assets, dependencies, layers, tags, components, folders, shaders, includes, or per-frame work.
- [x] Candidate ranking, corner depth/scoring, cap-ring tuning, width schedules, conflict rules, triangulation, mesh channels, authored normals/tangents, and render response remain unchanged.
- [x] Mandatory cap-ring completion, unrelated ordinary-retention, exact identity, mesh validity, `<=4 s` target, `<5 s` hard case, `35 s` matrix, and `90 s` suite gates are not relaxed.
- [x] One failed complete build returns the ordinary baseline; it does not authorize a second complete candidate build.

### File-by-file implementation sequence

1. [x] Add a non-serialized baseline bundle in `MassGenerator.cs`; thread cached/local baseline mesh, status, and timing through all corner search entry points.
2. [x] Rename solve-only search semantics to candidate preparation and store its identity evidence separately from final authoritative identities.
3. [x] Materialize exactly one selected candidate; derive final ordinary/mandatory identities and unrelated-retention from the completed coverage, then establish final hashes and soup/status as the authoritative plan.
4. [x] Add one centralized baseline-return helper and use it for no candidate, deadline, preparation failure exhaustion, complete-build failure, identity/retention failure, and emission failure/budget exits.
5. [x] Update failure priority so candidate-preparation, complete-build, complete-build-retention, and emission failures preserve the deepest diagnostic.
6. [x] Add current corner-status report capture/output and advance C1A.3g report/suite contract labels without changing the one-click workflow.
7. [x] Update the framework, recovery architecture, and code inventory to the implemented ownership model.
8. [x] Run exact-scope diff review, changed-caller/consumer reread, C# lexical/preprocessor/region checks, contract assertions, whitespace/line-ending checks, archive replay, and record Unity compile/runtime as pending.

### Risks and controls

- [x] Risk: returning the supplied baseline object allows mutation by the caller. Control: this matches the existing corner-disabled `GenerateUnifiedEdgeWearPreviewWithBaseline` contract; `MeshData` is returned as completed generator output and no generator path mutates it after return.
- [x] Risk: a prepared candidate passes but its one complete build fails. Control: report the exact complete-build stage, return the ordinary baseline, and do not try another complete build.
- [x] Risk: final identities differ from prepared estimates. Control: prepared identities are diagnostic/ranking evidence only; final authority is recomputed from completed coverage and compared only against the emitted status from the same stored soup.
- [x] Risk: cached mesh/status mismatch. Control: cached reuse requires a non-null mesh and applied status; otherwise build one local baseline pair.
- [x] Risk: scope drift into plane kernel or cloud shaders. Control: both are reviewed unchanged owners and excluded from the approved file list.

### Validation and compliance status

- [x] Final diff restricted to the eight approved files; no create/delete/move/rename and no shader/cloud change.
- [x] All failure exits return baseline mesh plus baseline unified status and retain a non-null failed corner status.
- [x] One enabled search contains at most one call to `MaterializePlaneCutBevelSolvedPlan`; emission consumes stored soup/status.
- [x] Final authoritative identities/hashes are derived after materialization; prepared identities are not used as final authority.
- [x] Current one-click suite includes a `[Current Corner Status]` section and C1A.3g contracts.
- [x] All C# files pass delimiter, comment/string, preprocessor, and region-balance checks; changed files preserve their original line-ending convention, terminal newline, BOM state, and whitespace hygiene.
- [x] Changed-files ZIP and unified patch replay reproduce the final tree byte-for-byte from the untouched archive extraction.
- [ ] Unity 6000.5.0f1 compilation and `EW-C1A.3g-suite` runtime validation pending user execution; no runtime success or performance improvement may be claimed from static checks.


## EW-C1A.3h implementation plan — minimum-width foreign-plane endpoint-conflict preparation guard

### Objective and acceptance criteria

- [x] Record the C1A.3g Unity result accurately: ownership/fallback and one-build architecture passed, but corner acceptance remained `15/33`; enabled success was `4/22`, candidate preparation rejected `0/22`, and the complete authoritative shell rejected `18/22`.
- [x] Add a deterministic preparation guard that rejects a candidate only when an actual prepared foreign edge plane still intersects a victim bevel-band rail away from both permitted endpoint zones after the complete local conflict cluster has retreated to every member's legal minimum scale.
- [x] Continue ranked transaction/preflight/preparation after a guard rejection. The first guard-clear candidate alone may consume the one complete authoritative build.
- [x] Preserve the exact C1A.3g ordinary baseline bundle, centralized truthful fallback, final completed-shell authority, stored-soup emission, production path, and one-complete-build maximum.
- [x] Report guard attempts, passes, proven rejects, tested rail intersections, selected victim/foreign identities, axial parameter, endpoint allowance, local cluster, minimum scales, retreat capacity, guard duration, and any same-class complete-build false negative.
- [x] Retain mandatory cap-ring, unrelated-retention, exact identity, mesh-channel, `<=4 s` target, `<5 s` hard case, `35 s` matrix, and `90 s` suite gates without relaxation.

### Approved files

- [x] `Assets/Docs/Generated_Mass_Feature_Implementation_Checklist.md` — first persistent write, runtime evidence, plan, implementation status, and final compliance audit.
- [x] `Assets/Docs/Generated_Mass_Framework.md` — record the accepted C1A.3g ownership boundary and C1A.3h preparation guard.
- [x] `Assets/Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md` — define the minimum-width local-cluster proof and its non-authoritative role.
- [x] `Assets/Docs/Generated_Mass_Edge_Wear_Code_Inventory.md` — record exact C1A.3h code ownership.
- [x] `Assets/Game/Procedural/Masses/MassGenerator.cs` — preparation guard, deterministic ranked continuation, complete-build false-negative classification, and evidence propagation.
- [x] `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Types.cs` — non-serialized plan/search telemetry for guard evidence.
- [x] `Assets/Game/Procedural/Masses/MassGenerator.EdgeWear.Diagnostics.Logging.cs` — C1A.3h contracts and guard evidence in the existing corner report.
- [x] `Assets/Game/Procedural/Masses/Editor/GeneratedMassEditor.cs` — aggregate guard telemetry in the existing one-click suite and 33-case matrix.

No file may be created, deleted, moved, renamed, generated, or modified outside this eight-file scope. `MassGenerator.EdgeWear.PlaneCutKernel.cs`, `MassGenerator.EdgeWear.PlaneCutJunctionSolver.cs`, `MassGenerator.EdgeWear.Orchestration.cs`, `GeneratedMass.cs`, `MeshData.cs`, `MassGenerator.MeshOutput.cs`, all shaders/includes including cloud integration, materials, scenes, prefabs, serialized assets, metadata, and production owners are reviewed unchanged.

### Read-only evidence reviewed before implementation

- [x] Current source root `/mnt/data/ew_c1a3g_work` exactly matches all eight entries in `GeneratedMass_EW-C1A.3g_ChangedFiles.zip`, SHA-256 `9a6ba3d4357499bdd1093fc1612ab8b5cd1ecba7078984762170aedb7afd497a`, over authoritative post-cloud archive `Assets-Code-Archive(18).zip`, SHA-256 `862adafc137d885207ffb0debdeaaba0b6fb18dc96744830af24831a4e19088a`.
- [x] No `.git` directory exists in the supplied source, so real branch, `HEAD`, status, diff, and history are unavailable; the C1A.3g changed-files package and current byte comparison are the accepted-version comparison evidence.
- [x] Unity report `Pasted text(153).txt`, SHA-256 `1be52b5c6bdaf86e68d7f6b0745c84ae82debabd4c4272de3fdb2d210c54d030`: suite elapsed `59703.5626 ms`, current preview passed, topology and artistic fingerprints `33/33`, corner matrix `15/33`, disabled parity `11/11`, complete builds `22`, build mismatches `0`, deadline aborts `0`, unrelated retention and channels `33/33`.
- [x] The same report records seventeen complete-build failures of the form `foreign generated plane EdgeBevelPlane:X splits bevel-band edge Y at axial parameter T; conflict cluster reached its geometric minimum width`; every reported `T` is endpoint-adjacent (`0.03379..0.1854` or `0.9303..0.9632`). The remaining seed `7778` default failure is the separate generalized-retry class.
- [x] `MassGenerator.cs::GenerateCornerDamageFullCertificationSearch` currently accepts the first solve-valid prepared candidate and breaks before the one complete build; therefore a proof rejection inside `TryPrepareCornerDamageIntegrationPlan` naturally continues deterministic candidate ranking without adding another complete build.
- [x] `MassGenerator.EdgeWear.PlaneCutJunctionSolver.cs:1087-1159` defines the authoritative band-split classification: project a foreign bevel-face boundary segment onto the victim source-edge axis and reject only when its midpoint lies outside the endpoint allowance `clamp(max(width*4, minimumStableEdgeLength*0.5)/edgeLength, 0.03, 0.25)`.
- [x] `MassGenerator.EdgeWear.PlaneCutKernel.cs:2043-2075, 2700-2844, 6970-7039, 7041-7087` defines legal retreat: the conflict cluster is victim/foreign endpoints plus incident candidate edges; each candidate may retreat only to `ResolvePlaneCutCandidateMinimumScale`; unresolved geometry at those floors is the exact failure class.
- [x] `PlaneCutBevelSolvedPlan` already owns the actual prepared edge planes, post-cut source faces, topology graph, widths, tolerances, and stability limits required for a non-materializing proof. No plane-kernel edit is required.
- [x] `GeneratedMassEditor` already copies all corner search telemetry into each matrix case and aggregate; new non-serialized counters can be propagated without new controls, actions, or serialized state.
- [x] `Assets/AGENTS.md`, the complete C1A.3g search/preparation/completion/emission methods, status and telemetry contracts, report builders, matrix case/aggregate/job consumers, canonical framework/architecture/inventory sections, and unchanged plane-kernel/junction-solver producers were reviewed before this first write.

### Invariants and non-goals

- [x] The guard is preparation-only and predictive. Only `MaterializePlaneCutBevelSolvedPlan` remains authoritative for final retained identities, topology, coverage, render validity, and soup.
- [x] Reject only a geometrically proven persistent minimum-width rail split. Do not use a fixed global axial threshold, report-string parsing, seed-specific exclusions, or artistic heuristics.
- [x] Do not change edge/corner scoring, depth, cap-ring tuning, width schedules, conflict reduction, clipping, topology certification, triangulation, normals/tangents, shaders, materials, serialized settings, production generation, or per-frame work.
- [x] Do not run a second complete build after an accepted guard-clear candidate fails. Such a failure remains a truthful baseline fallback and is counted as guard false-negative evidence when it matches the guarded class.
- [x] The generalized-retry seed `7778` default failure is outside this patch unless the new guard independently proves a foreign-plane floor conflict for a later ranked candidate.

### File-by-file implementation sequence

1. [x] Extend non-serialized plan/status/telemetry with endpoint-conflict guard evidence and timing.
2. [x] Reconstruct deterministic local conflict clusters from victim/foreign candidate endpoints, scale only those cluster planes to each legal minimum, and retain all non-cluster planes at their prepared widths.
3. [x] Intersect the minimum-width victim and foreign bevel planes with the victim owner source-face planes, then clip the resulting rail against every post-cut source-face half-space and every non-defining retained candidate half-space.
4. [x] Apply the exact authoritative endpoint allowance to the projected victim-edge axial parameter; reject only an interior rail split that survives the complete local cluster's minimum-width retreat.
5. [x] Integrate the guard after solve-valid identity/retention preparation and before setting `plan.Valid`; let existing ranked search continue on proof rejection.
6. [x] Classify a later complete-build foreign-plane geometric-floor failure as a guard false negative without changing fallback or retry behavior.
7. [x] Propagate guard evidence through the existing corner report, matrix cases, suite aggregate, and C1A.3h contracts.
8. [x] Update stable framework, recovery architecture, and code inventory only after implementation behavior is final.
9. [x] Run exact-scope diff review, complete modified-file and affected-owner reread, C# delimiter/comment/string/preprocessor/region checks, contract checks, patch/ZIP replay, and record Unity validation as pending.

### Risks and controls

- [x] Risk: false-positive rejection from an infinite plane intersection outside the finite shell. Control: clip the reconstructed victim/foreign rail against every post-cut source-face half-space and every non-defining retained candidate half-space after only the implicated local cluster retreats.
- [x] Risk: rejecting legal endpoint junctions. Control: use the same width/stability-derived endpoint allowance and strict interior test as the authoritative band audit.
- [x] Risk: optimistic proof from scaling unrelated planes. Control: keep non-cluster planes at prepared widths; only the exact victim/foreign endpoint-star cluster receives legal minimum retreat.
- [x] Risk: numerical near-coplanarity. Control: reject parallel/ill-conditioned triple-plane systems and require finite values plus explicit tolerance checks; uncertain geometry passes to the authoritative build rather than being rejected.
- [x] Risk: performance regression from pairwise checks. Control: the guard is dirty/editor-time `O(e^3)` worst case over tens of retained edges, has no mesh construction or triangulation, records duration, and remains under the existing `5 s` case gate.

### Validation and audit status

- [x] Final diff restricted to the eight approved files.
- [x] Complete final files and direct producers/consumers reread against C1A.3g accepted source.
- [x] C# lexical/preprocessor/region checks pass for all current source files.
- [x] Static contracts prove deterministic guard placement, exact endpoint formula, local-cluster-only minimum scaling, half-space proof, ranked continuation, one complete build, unchanged fallback, and C1A.3h telemetry/contracts.
- [x] Changed-files ZIP and unified patch replay reproduce the final tree byte-for-byte.
- [ ] Unity 6000.5.0f1 compilation and `EW-C1A.3h-suite` runtime validation pending user execution.
