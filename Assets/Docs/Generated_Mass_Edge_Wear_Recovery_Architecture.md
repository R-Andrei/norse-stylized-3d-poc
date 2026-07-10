# Generated Mass Edge Wear Recovery Architecture

Status: active edge-wear architecture  
Current implementation target: **EW-B — Deterministic Selected-Edge Bevel Kernel**  
Current implementation step: **EW-B3R3 — Two-Edge Corner Cap Closure**

---

## 1. Active decision

The active convex edge-wear geometry path is **source-owned local bevel network emission**.

The source mass is generated first through the normal compact-mass cut/chip pipeline. Edge wear is then emitted from the final source `PolygonFace` graph before final triangulation.

```text
final source polyhedron
→ topology graph
→ selected convex graph edges
→ per-face local offset replacement polygons
→ per-edge local bevel bridge faces
→ per-source-vertex local cap patches
→ topology audit / triangle preview
→ commit
```

This is the sane version of “generate with bevels”: bevels are part of final mass generation, but they are not generated before the shape cuts/chips where later operations would have to preserve them.

---

## 2. Why global selected-edge cuts are not active

EW-B2 proved the source-graph route was cheap and topologically valid, but it used one global half-space cut per selected edge. Validation committed successfully:

```text
accepted=27
committedConvexEdgeWearFaces=90
committedConvexEdgeWearRenderedVerticesEstimate=270
topologyOpenEdges=0
topologyNonManifoldEdges=0
topologyTJunctions=0
```

The visual result was wrong: large vertical strips and long planar gouges appeared because selected-edge cut planes clipped unrelated parts of the rock. That means the B2 construction primitive was topologically clean but semantically wrong for local edge wear.

EW-B3 replaces global selected-edge cuts with source-owned local geometry. EW-B3R3 specifically keeps isolated endpoint apex caps and multi-edge vertex-star caps intact, while moving two-edge corner caps to a dedicated ordered corner-patch builder. The active route must report:

```text
deterministicKernelGlobalCutsApplied=0
```

---

## 3. Desired feature, scoped correctly

This architecture covers only the geometry foundation for convex edge wear.

Required result:

```text
real generated bevel/chamfer geometry on selected convex edges
watertight topology
no non-manifold edges
no T-junctions
no render slivers/clipping
ConvexEdgeWear material mask preserved on bevel/cap faces
```

Later feature layers still need masks, shader response, cracks, plate seams, stains, moss/dirt, and broader rock-fracture systems. Those are not part of EW-B3R3.

---

## 4. EW-B3 implementation contract

Inputs:

```text
source PolygonFace list after all shape cuts/chips
EdgeWearTopologyGraph
mapped EdgeWearSelectedGraphEdge list
bevel depth from EdgeWearWidth
feature strength from EdgeWearAmount
minimum stable face / edge thresholds
```

Output:

```text
rebuilt PolygonFace list with Base and ConvexEdgeWear faces
Base replacement polygons owned by source faces
ConvexEdgeWear bevel/cap polygons owned by source edges/vertices
ConvexEdgeWear polygons triangulated before commit
final topology audit and triangle-preview counters
```

EW-B3 uses source-owned local construction:

```text
source face → local offset replacement polygon
selected source edge → local bridge face between adjacent face rails
affected source vertex → local cap patch from adjacent replacement-corner points
unselected source edge with separated rails → Base transition bridge
```

It must not use per-edge global half-space cuts for edge wear.

---

## 5. Diagnostics required

EW-B3 logs:

```text
deterministicKernelGeometryPending
deterministicKernelMappedSelectedEdges
deterministicKernelSourceFaces / Edges / Vertices
deterministicKernelAffectedFaces / Vertices
deterministicKernelGlobalCutsApplied
deterministicKernelFaceOffsetPolygonsAttempted / Built / Failures
deterministicKernelRailsExpected / Built / Missing
deterministicKernelLocalBevelFacesAttempted / Built / Failures
deterministicKernelTransitionFacesBuilt
deterministicKernelVertexCapsAttempted / Built / Failures
deterministicKernelConvexWearPolygonsTriangulated
deterministicKernelConvexWearTrianglesBuilt
deterministicKernelOpenEdgesAfterBuild
deterministicKernelNonManifoldEdgesAfterBuild
deterministicKernelTJunctionsAfterBuild
```

Expected EW-B3 success shape for the current dense validation rock:

```text
deterministicKernelGlobalCutsApplied=0
deterministicKernelFaceOffsetPolygonsBuilt=16
deterministicKernelLocalBevelFacesBuilt=36
deterministicKernelVertexCapsBuilt≈28
committedConvexEdgeWearFaces>0
topologyOpenEdges=0
topologyNonManifoldEdges=0
topologyTJunctions=0
triangulationPreviewSkippedConvexEdgeWearTriangles=0
```

---

## 6. Blocked later work

Do not proceed to mask widening, shader/material response, irregular width, profile segments, or density tuning until EW-B3 or its follow-up commits local bevel geometry without the B2 slice/gouge artifacts.


## 5. EW-B3R cap closure rule

The active vertex cap builder must not rely on unordered cap point polygons. For each affected source vertex it records replacement-corner points with source face ownership, orders those points around the source vertex using the vertex-star normal, and emits ConvexEdgeWear fan triangles from a local cap centre.

This is specifically different from the retired open-cycle cap logic: the cap boundary is owned by one source vertex and its incident source faces, not discovered from arbitrary open mesh edges after the fact.


EW-B3R3 note: two-edge corner vertices must not use the isolated endpoint apex cap path; they use ordered unique cap points and centre/boundary triangulation instead.
