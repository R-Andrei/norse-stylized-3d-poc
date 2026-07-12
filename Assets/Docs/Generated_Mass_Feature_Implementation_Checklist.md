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

### EW-K1.3 Unity exit criteria

- [ ] Unity compiles with zero errors and zero warnings.
- [ ] Exactly 24 compact audits are emitted with every unrelated field unchanged.
- [ ] Every clone reports `planesBuilt=active` and `planesRejected=0`.
- [ ] The exceptional seam mass reports `seamPairs=2` and `open=0`, or remains rejected without arbitrary repair if the pairs are not mutually unique.
- [ ] The three no-cap cuts are classified as verified redundant only when their original source edges are absent; `capsMissing=0`.
- [ ] Every clone reports zero open edges, non-manifold edges, T-junctions, and invalid faces.
- [ ] Every clone reports `valid=1`.
- [ ] Rendered geometry remains unchanged and `geometryCommit=disabled` remains present.
- [ ] On success, stop clone-topology development and expose an editor-only plane-cut visual preview.

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
