# Generated Mass Feature Implementation Checklist

## Active feature

```text
EW-C — Explicit Single-Segment Chamfer Kernel
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

- [ ] Unity compiles without errors.
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

- [ ] Unity compiles without errors.
- [ ] All objects report `sourceBoundaryLoopNormalizationFailures=0`.
- [ ] The three previously blocked objects report guarded retrace removals.
- [ ] All objects report `sourceBoundaryChildIncidenceFailures=0`.
- [ ] All objects report `sourceBoundaryDuplicateChildKeyFailures=0`.
- [ ] All objects report `sourceBoundaryTerminalTransferFailures=0`.
- [ ] All objects report `expectedSourceBoundaryEdges=matchedSourceBoundaryEdges`.
- [ ] Candidate, active/deferred, replacement-face, and bevel-strip counts remain unchanged per object.
- [ ] `provisionalNonManifoldEdges=0`, `provisionalTJunctions=0`, and `tJunctionRecordsIncompatible=0` remain true.
- [ ] All 24 representative masses report `readyForVertexPatches=1`.
- [ ] Geometry commit remains disabled.

## EW-C3 — Crude vertex-run patches

- [ ] Emit a centre fan for a closed selected run.
- [ ] Emit a source-vertex-apex fan for an open selected run.
- [ ] Emit separate patches for separated selected runs.
- [ ] Preserve the validated source boundary identity.

### Exit criteria

- [ ] Output boundaries equal the explicitly preserved source-boundary set.
- [ ] Newly introduced open edges are zero.
- [ ] Non-manifold edges are zero.
- [ ] T-junctions are zero.

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
