# Generated Mass Feature Implementation Checklist

Status: active implementation checklist  
Current target: EW-4D0 — Variable-Profile Topology Bevel Graph  
Current step: EW-4D0.6T3 — T-Junction-Safe Open-Cycle Polygon Closure

---

## 1. Hard rules

```text
- Do not implement final convex edge wear with FeatureAtlas0/1.
- Do not use overlay meshes as the final production solution.
- Do not weaken topology validation to make geometry visible.
- Do not drop individual selected candidates to hide construction errors.
- Dirty-time generation may be heavier; runtime memory and runtime per-frame cost should stay reasonable.
- Keep docs updated in the same patch as each implementation step.
```

---

## 2. Active representation

Final plane-cut GeneratedMass convex edge wear should be:

```text
generated main-mesh variable-profile bevel ribbons
+ actual open-cycle polygon caps without centre-fan radial edges
+ UV2.z / vertex color ConvexEdgeWear material markers
+ shader response on marked generated geometry
```

FeatureAtlas0/1 are temporary debug/broad-mask tools only. They are not the normal-render convex edge-wear representation.

---

## 3. Staged EW-4D0 checklist

### EW-4D0.1 — Topology graph foundation — done

Implementation requirements:

```text
- Build graph from PolygonFace list.
- Preserve graph vertices, edges, faces, and adjacency.
- Map selected candidates to exact graph edges.
- Report graph stats.
- Emit no new bevel geometry.
```

Validation success:

```text
selectedGraphEdges == selected
missingSelectedGraphEdges == 0
mismatchedSelectedGraphFaces == 0
duplicateSelectedGraphEdges == 0
invalidGraphFaces == 0
invalidGraphEdges == 0
```

### EW-4D0.2 — Per-face selected-edge clipping / shared rail preflight — done

Implementation requirements:

```text
- For each selected edge, build the two adjacent face inset cuts.
- For each affected face, clip once against all selected-edge cuts on that face.
- Extract one rail from the clipped face boundary for each selected candidate/face pair.
- Emit no new bevel geometry.
```

Validation success:

```text
faceClipFailedFaces == 0
selectedFaceEdges == selectedGraphEdges * 2
expectedRails == selectedGraphEdges * 2
extractedRails == expectedRails
missingRails == 0
shortRails == 0
fragmentedRails == 0
```

### EW-4D0.3 — Rail/profile storage and sampled profile-grid preflight — done

Implementation requirements:

```text
- Pair the two extracted rails for every selected graph edge.
- Compute along-edge sample count.
- Use three cross-profile segments for the first curved-profile preflight.
- Generate finite profile-grid points P[t,k].
- Use the original sharp edge as the rounded-profile control line.
- Emit no new bevel geometry.
```

Validation success:

```text
profileEdgesPrepared == selectedGraphEdges
profileEdgesFailed == 0
profileInvalidPoints == 0
profileZeroWidth == 0
profileGridPoints > 0
affectedBaseFaces == faceClipAffectedFaces
replacedBaseFaces == faceClipSucceededFaces
baseFaceValidationFailures == 0
railSampleInsertionFailures == 0
ribbonEdgesPrepared == selectedGraphEdges
ribbonEdgesFailed == 0
ribbonFacesBuilt == ribbonFacesExpected
ribbonDegenerateFaces == 0
ribbonInvalidFaces == 0
openCycleClosureEdgesInput == workspaceOpenEdgesAfterRibbons
openCycleClosureComponentsInput == workspaceOpenEdgeComponentsAfterRibbons
openCycleClosureComponentsBuilt == openCycleClosureComponentsInput
openCycleClosureFacesExpected == openCycleClosureComponentsInput
openCycleClosureFacesBuilt == openCycleClosureFacesExpected
workspaceOpenEdgesAfterComponentClosure == 0
workspaceNonManifoldEdgesAfterComponentClosure == 0
workspaceTJunctionsAfterComponentClosure == 0
baseFaceValidationFailures == 0
workspaceBaseFaces == graphFaces
profileSegments == 3
```

### EW-4D0.4 — Clipped base-face replacement workspace — completed

Implementation requirements:

```text
- Build a temporary rebuild workspace.
- Preserve unaffected base faces.
- Replace affected base faces with the clipped polygons proven in EW-4D0.2.
- Keep replaced face feature as Base.
- Validate replaced base faces before any bevel ribbons are accepted.
- Do not commit the workspace as final geometry yet.
- Allow temporary open edges because ribbons/corner patches are not appended yet.
```

Validation success:

```text
affectedBaseFaces == faceClipAffectedFaces
replacedBaseFaces == faceClipSucceededFaces
baseFaceValidationFailures == 0
workspaceBaseFaces == graphFaces
```

### EW-4D0.5 — Rail-sampled base boundaries + sampled variable-profile bevel ribbon emission — completed

Implementation requirements:

```text
- Insert protected rail samples into clipped base-face boundaries before ribbon emission.
- Convert profile grids into ConvexEdgeWear ribbon faces.
- Generate one ribbon band face for each along-edge interval and profile segment.
- Mark ribbon faces as ConvexEdgeWear.
- Store material strength from candidate/edge style.
```

Validation success:

```text
railSampleInsertionFailures == 0
railSampledBaseFaces == affectedBaseFaces
railSamplesInserted > 0
ribbonEdgesPrepared == selectedGraphEdges
ribbonEdgesFailed == 0
ribbonFacesBuilt == ribbonFacesExpected
ribbonDegenerateFaces == 0
ribbonInvalidFaces == 0
workspaceConvexEdgeWearFaces > 0
Convex Edge Wear debug is no longer black after final commit step
```

### EW-4D0.6 — Corner vertex patches — completed but unsafe

Implementation result:

```text
- Endpoint profile arcs were gathered at touched graph vertices.
- Shared corner fan/radial patch faces were generated and marked ConvexEdgeWear.
- The stage proved corner faces can be built, but not that the workspace is safe.
```

Failure branch that blocks EW-4D0.7:

```text
- A failed patch could append partial faces before returning false.
- Corner point dedupe used PointMergeDistance rather than the validation edge-length scale.
- Corner normal fallback could silently become Vector3.up.
- workspaceOpenEdgesAfterCorners <= workspaceOpenEdgesAfterRibbons is not a sufficient pass condition.
```

### EW-4D0.6R — Corner patch hardening / failure diagnostics — completed as containment

Implementation requirements:

```text
- Build each corner patch into a local temporary face list first.
- Append corner faces to the rebuild workspace only when the patch is accepted.
- If a corner patch fails, append no partial faces.
- Add granular failure diagnostics for arcs, boundary points, duplicates, normals, ordering, centre distance, small faces, skipped degenerates, hard failures, accepted faces, and rejected faces.
- Use corner-local dedupe tolerance based on minimumStableEdgeLength.
- Replace unsafe Vector3.up normal fallback with a corner normal calculation that can actually fail.
- Count remaining open edges after corners near graph vertices and away from graph vertices.
- Keep this workspace-only; final visible commit waits for EW-4D0.7.
```

Validation success:

```text
cornerPatchVertices > 0
cornerPatchAcceptedFaces > 0
cornerPatchFailed == 0
cornerPatchHardFailures == 0
cornerPatchInvalidNormals == 0
cornerPatchOrderingFailures == 0
cornerPatchInsufficientBoundaryPoints == 0
workspaceOpenEdgesAfterCorners is substantially lower than the EW-4D0.6 baseline
remaining open edges are attributed by workspaceOpenEdgesNearGraphVerticesAfterCorners and workspaceOpenEdgesAwayFromGraphVerticesAfterCorners
```


### EW-4D0.6R2 — Workspace open-edge loop diagnostics — completed as proof

EW-4D0.6R validation proved that corner degenerates were eliminated, but remaining open edges were split between graph-vertex-local and away-from-vertex regions:

```text
cornerPatchFailed=2
cornerPatchDegenerateFaces=0
workspaceOpenEdgesAfterCorners=64
workspaceOpenEdgesNearGraphVerticesAfterCorners=30
workspaceOpenEdgesAwayFromGraphVerticesAfterCorners=34
```

EW-4D0.6R2 then proved the post-ribbon state is the cleaner closure target:

```text
workspaceOpenEdgesAfterRibbons=226
workspaceOpenEdgeComponentsAfterRibbons=22
workspaceOpenEdgeEndpointLeavesAfterRibbons=0
workspaceOpenEdgeEndpointBranchesAfterRibbons=0
workspaceOpenEdgesAfterCorners=64
workspaceOpenEdgeEndpointBranchesAfterCorners=14
```

### EW-4D0.6T — Actual open-cycle closure after ribbons — completed as topology-trace proof

Validation result:

```text
openCycleClosureEdgesInput=226
openCycleClosureComponentsInput=22
openCycleClosureNonCycleEndpoints=0
openCycleClosureTraceFailures=0
openCycleClosureTooSmallCycles=0
openCycleClosureInvalidNormals=1
openCycleClosureFacesBuilt=0
```

Verdict: topology tracing is valid; aggregate whole-cycle normal calculation is too strict and must be replaced by per-triangle normals.

### EW-4D0.6T2 — Per-triangle open-cycle closure normals — completed as open-edge proof

Validation result:

```text
openCycleClosureEdgesInput=226
openCycleClosureComponentsInput=22
openCycleClosureComponentsBuilt=22
openCycleClosureComponentsFailed=0
openCycleClosureFacesExpected=226
openCycleClosureFacesBuilt=226
workspaceOpenEdgesAfterComponentClosure=0
workspaceNonManifoldEdgesAfterComponentClosure=0
workspaceTJunctionsAfterComponentClosure=19
```

Verdict: per-triangle fan closure proved the open-cycle topology can close open edges and avoid non-manifold edges, but the radial centre-fan diagonals create T-junctions.

### EW-4D0.6T3 — T-junction-safe open-cycle polygon closure — current

Implementation requirements:

```text
- Keep actual post-ribbon open edges as the closure source.
- Require endpoint valence 2 for every open-edge endpoint.
- Trace each connected component into an ordered cycle.
- Compute centre = average(cycle vertices) for each cycle.
- Build one ConvexEdgeWear polygon cap per traced open-edge component.
- Preserve the traced cycle vertex order so cap boundary edges exactly match the real open edges.
- Compute a robust orientation normal from aligned per-edge triangle normals, but do not create radial centre-fan topology edges.
- Build the full closure face list transactionally and append only on success.
- Audit post-closure open edges, non-manifold edges, and T-junctions.
- Keep the step workspace-only; final visible commit remains EW-4D0.7.
```

Validation success:

```text
openCycleClosureEdgesInput == workspaceOpenEdgesAfterRibbons
openCycleClosureComponentsInput == workspaceOpenEdgeComponentsAfterRibbons
openCycleClosureComponentsBuilt == openCycleClosureComponentsInput
openCycleClosureFacesExpected == openCycleClosureComponentsInput
openCycleClosureFacesBuilt == openCycleClosureFacesExpected
openCycleClosureNonCycleEndpoints == 0
openCycleClosureTraceFailures == 0
openCycleClosureTooSmallCycles == 0
openCycleClosureInvalidNormals == 0
openCycleClosureDegenerateTriangles == 0
openCycleClosureInvalidFaces == 0
workspaceOpenEdgesAfterComponentClosure == 0
workspaceNonManifoldEdgesAfterComponentClosure == 0
workspaceTJunctionsAfterComponentClosure == 0
```

### EW-4D0.7 — Final topology validation and active-path switch

Implementation requirements:

```text
- Combine unaffected faces, clipped replacement base faces, bevel ribbon faces, and open-cycle closure faces.
- Weld/sanitize consistently.
- Audit open edges, non-manifold edges, and T-junctions.
- Commit only if topology is safe.
- Remove the old EW-4C.0 half-space fallback from the active post-preflight path.
```

Validation success:

```text
accepted == selectedGraphEdges
builtBevelEdges == selectedGraphEdges
rejectedValidationOpenEdge == 0
rejectedValidationNonManifoldEdge == 0
rejectedValidationTJunction == 0
producedBevelFaces > 0
Surface Mask Debug = Convex Edge Wear shows visible mask
normal render shows real worn bevel geometry
```

### EW-4D0.8 — Variation tuning

Implementation requirements:

```text
- Add deterministic per-edge style: width, profile curve, strength, chipped tendency.
- Add deterministic along-edge variation: taper, width noise, material noise, chip mask.
- Clamp rail wobble so variation cannot self-intersect or create topology breaks.
- Keep selected candidates stable; scale global width/profile richness if needed.
```

Validation success:

```text
same seed produces same variation
changed seed changes variation
edges vary between each other
wear varies within a single edge
no new topology failures
```

### EW-4D1 — Quality refinement and crack/groove planning

Implementation requirements:

```text
- Improve corner vertex patches if D0 fan patches are visibly pinched.
- Decide whether corner patches need an Adj-like topology pattern.
- Plan separate crack/groove features as concave/incised paths, not as convex bevels.
```

---

## 4. Validation checklist for the current patch

After importing EW-4D0.6R:

```text
1. Regenerate the same plane-cut rock.
2. Keep Edge Wear enabled with nonzero Amount / Width / Coverage.
3. Read the console summary.
4. Confirm the existing Step 1, Step 2, and Step 3 fields remain clean.
5. Confirm clipped-base workspace fields remain clean.
6. Confirm rail-sampled base boundary and ribbon fields are present and clean.
7. Confirm open-cycle closure diagnostics are present and explain any remaining failure.
```

Good current result:

```text
selectedGraphEdges == selected
faceClipFailedFaces == 0
extractedRails == expectedRails
profileEdgesPrepared == selectedGraphEdges
profileEdgesFailed == 0
profileInvalidPoints == 0
profileZeroWidth == 0
profileGridPoints > 0
affectedBaseFaces == faceClipAffectedFaces
replacedBaseFaces == faceClipSucceededFaces
baseFaceValidationFailures == 0
workspaceBaseFaces == graphFaces
railSampleInsertionFailures == 0
railSampledBaseFaces == affectedBaseFaces
railSamplesInserted > 0
ribbonEdgesPrepared == selectedGraphEdges
ribbonEdgesFailed == 0
ribbonFacesBuilt == ribbonFacesExpected
ribbonDegenerateFaces == 0
ribbonInvalidFaces == 0
workspaceConvexEdgeWearFaces > 0
openCycleClosureEdgesInput == workspaceOpenEdgesAfterRibbons
openCycleClosureComponentsInput == workspaceOpenEdgeComponentsAfterRibbons
openCycleClosureComponentsBuilt == openCycleClosureComponentsInput
openCycleClosureFacesExpected == openCycleClosureComponentsInput
openCycleClosureFacesBuilt == openCycleClosureFacesExpected
workspaceOpenEdgesAfterComponentClosure == 0
workspaceNonManifoldEdgesAfterComponentClosure == 0
workspaceTJunctionsAfterComponentClosure == 0
```

Expected visual result:

```text
Convex Edge Wear may still be black.
EW-4D0.5 emits ribbon faces only inside the temporary rebuild workspace. EW-4D0.6T3 closes actual post-ribbon open cycles in that same workspace using one ordered polygon cap per traced open-edge component. Final visible commit still waits for EW-4D0.7 final topology validation and active-path switch.
```

---

## 5. Files allowed for EW-4D0 patches

Allowed by default:

```text
Game/Procedural/Masses/MassGenerator.cs
Docs/Generated_Mass_Framework.md
Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md
Docs/Generated_Mass_Feature_Implementation_Checklist.md
```

Do not touch without explicit approval:

```text
Shaders
PixelSurface includes
GeneratedMass.cs
GeneratedMassEditor.cs
FeatureAtlas baker
GeneratedGround / ground generation
MeshData / MeshBuilder
River / foam systems
```
