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
