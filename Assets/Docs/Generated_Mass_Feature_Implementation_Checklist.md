# Generated Mass Feature Implementation Checklist

Status: active implementation checklist  
Current implementation target: **EW-B — Deterministic Selected-Edge Bevel Kernel**  
Current implementation step: **EW-B3R3 — Two-Edge Corner Cap Closure**

---

## 1. Hard rules

```text
- Do not implement final convex edge wear with FeatureAtlas0/1.
- Do not use overlay meshes as the final production solution.
- Do not weaken topology validation to make geometry visible.
- Do not tune material response before physical bevel geometry is valid.
- Dirty-time generation may be heavier; runtime memory and runtime per-frame cost should stay reasonable.
- Keep docs updated in the same patch as each implementation step.
- Do not reintroduce retired local bevel, sampled-ribbon, workspace, or open-cycle closure construction.
- Do not use per-edge global half-space cuts as the active edge-wear construction primitive.
```

---

## 2. Active representation

Final plane-cut GeneratedMass convex edge wear should be:

```text
main-mesh deterministic selected-edge bevel geometry
+ source-face replacement polygons
+ source-edge local bevel bridge faces
+ source-vertex local cap patches
+ UV2.z / vertex color ConvexEdgeWear material markers
+ shader response on marked generated geometry after geometry is stable
```

FeatureAtlas0/1 are temporary debug/broad-mask tools only. They are not the normal-render convex edge-wear representation.

---

## 3. Completed cleanup steps

### EW-B0 — Edge Wear Bevel Kernel Reconciliation — complete

```text
- Active route moved to TryBuildDeterministicSelectedEdgeBevelKernelFaces(...).
- Docs stopped treating EW-4D/R3 as current.
```

### EW-B1S — Legacy bevel-construction purge — complete

```text
- Removed retired local-bevel, half-space fallback, sampled-ribbon/workspace, open-cycle-closure, corner-patch, and workspace-T-junction-repair construction code from MassGenerator.cs.
- Active summary no longer reports ribbon/open-cycle/workspace diagnostic spam.
- EW-B source graph / selected edge / affected vertex classification remains active.
```

### EW-B2 — Source-Graph Beveled Mass Emission — complete as evidence, superseded as construction primitive

```text
- Proved source-graph pre-triangulation bevel emission can commit valid topology cheaply.
- Produced committed geometry with topologyOpenEdges=0, topologyNonManifoldEdges=0, topologyTJunctions=0.
- Failed visually because one global selected-edge cut plane can slice unrelated faces and create long strips/gouges.
```

---

## 4. EW-B3R3 — Two-Edge Corner Cap Closure — current

Implementation requirements:

```text
- Build source topology graph from final Base PolygonFace list.
- Map selected convex candidates to graph edges.
- Build DeterministicBevelEdgeRecord and DeterministicBevelVertexRecord diagnostics.
- For each affected source face, build one local offset replacement polygon.
- Record one rail segment for each source face-edge.
- For each selected graph edge, build one local ConvexEdgeWear bridge face between adjacent face rails.
- For unselected graph edges whose adjacent rails separated because neighboring vertices moved, build a Base transition bridge.
- For multi-edge affected source vertices, build ConvexEdgeWear cap triangles from replacement-corner points ordered around the source vertex star.
- For isolated endpoint affected source vertices, build ConvexEdgeWear terminal cap triangles using the original source vertex as an apex plus stable replacement boundary points.
- For two-edge affected source vertices, build ConvexEdgeWear corner-patch triangles from ordered unique cap points; do not use the isolated endpoint apex path.
- Triangulate ConvexEdgeWear polygons before commit.
- Run final topology audit and triangle-emission preview.
- Commit only if open/non-manifold/T-junction counts are zero and ConvexEdgeWear triangle preview skips are zero.
```

Validation success:

```text
Unity compiles.
deterministicKernelGeometryPending=0.
deterministicKernelGlobalCutsApplied=0.
deterministicKernelFaceOffsetPolygonsBuilt>0.
deterministicKernelLocalBevelFacesBuilt>0.
deterministicKernelVertexCapsBuilt>0.
committedConvexEdgeWearFaces>0.
committedConvexEdgeWearNgonFaces=0.
triangulationPreviewSkippedConvexEdgeWearTriangles=0.
topologyOpenEdges=0.
topologyNonManifoldEdges=0.
topologyTJunctions=0.
Convex Edge Wear debug view shows local generated bevel geometry without B2 slice/gouge artifacts.
```

---

## 5. Later steps, blocked until EW-B3 validates

```text
EW-B4 — bevel width/coverage tuning after local geometry commits.
EW-B5 — irregular edge-wear mask expansion and material response.
EW-B6 — optional profile/softness/variation only after deterministic geometry remains stable.
```

## Next work items

1. Validate EW-B3 on the same dense 36-selected-edge mass.
2. If EW-B3 fails, use faceOffset/rails/localBevel/vertexCap counters to identify the blocker.
3. If EW-B3 commits, inspect final render and Convex Edge Wear debug for the B2 clipping/gouge artifacts.


### EW-B3R — Deterministic vertex-star cap closure — current

```text
- Keep EW-B3 local face-offset and local edge-bridge construction.
- Build vertex caps from source face-owned replacement-corner records.
- Order cap boundaries around each source vertex using source vertex-star normals.
- Emit cap triangles directly; do not create arbitrary n-gon caps.
- Add per-cap-case diagnostics for isolated, two-edge, and multi-star vertices.
- Required gate: vertexCapsBuilt == vertexCapsAttempted and openEdgesAfterBuild == 0 before commit.
```


EW-B3R3 note: two-edge corner vertices must not use the isolated endpoint apex cap path; they use ordered unique cap points and centre/boundary triangulation instead.
