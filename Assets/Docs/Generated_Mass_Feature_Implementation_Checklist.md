# Generated Mass Edge-Wear Progress Log and Implementation Checklist

## Canonical log policy

This is the sole canonical Generated Mass edge-wear progress ledger. It owns patch history, methods tried, validation results, the current blocker, and the active next step.

The code inventory, recovery architecture, and framework documents contain only their own current stable facts. They may reference this file but must not maintain competing or complementary progress histories.

## Active feature

```text
EW-K — Convex Plane-Cut Bevel Kernel
```

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

**Status:** implemented; Unity compilation and runtime suite pending. R13A.4 remains the stable incomplete fallback until every safety gate passes.

- [x] Restore R13A.4 ordinary geometry and corner behavior; do not retain R13A.5 sampled split-plane geometry.
- [x] Restrict augmentation initiation to certified multi-support retained-hull edges and finalized corner-inactive participants.
- [x] Build the exact retained point set from all original vertices except the selected endpoints plus four exact rails.
- [x] Enumerate and merge global supporting hull planes; emit a connected bevel-facet band with complete source provenance.
- [x] Reject any result that modifies a source face outside the two endpoint stars or loses a source-face provenance record.
- [x] Capture corner recovery evidence at the final `corner-width-inactive` transition.
- [x] Protect recovery targets and seed bounded neighbour-deferral subsets from their exact conflict records.
- [x] Forbid certified baseline-edge loss outside recovered corner participants and forbid any certified-count reduction.
- [x] Advance suite, topology, preview, and comprehensive contracts to R13A.6.
- [ ] Unity compiles with zero errors.
- [ ] Current seed `8889` restores R13A.4 identity: edge `39` active and edge `40` inactive unless a named recovery target is also certified without unrelated loss.
- [ ] Full suite retains current preview, topology `33/33`, preview `33/33`, and comprehensive evidence.
- [ ] Outlier recovery reaches `5/5`, or exhaustive hull evidence proves a target infeasible while R13A.4 hashes and edge identity remain unchanged.
