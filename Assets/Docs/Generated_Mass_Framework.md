# Generated Mass Framework

Status: active framework definition  
Current implementation target: **EW-B — Deterministic Selected-Edge Bevel Kernel**  
Current implementation step: **EW-B3R3 — Two-Edge Corner Cap Closure**  
Supersedes as active edge-wear routes: atlas-first/runtime edge-wear plans, EW-4A global cuts, EW-4B local strips, EW-4C half-space bevel-plane fallback, and EW-4D/R/R2/R3 sampled-ribbon plus open-cycle-closure bevel construction.

## 0. EW-B3R3 implementation status

EW-B3R3 continues the source-owned local bevel network and fixes the current blocker: two-edge corner cap closure. EW-B3R2 proved isolated endpoint apex caps work (`2/2` isolated caps built) and multi-edge vertex-star caps remain solved (`18/18` multi-star caps built), but validation still failed because `7/8` two-edge corner caps failed by area. EW-B3R3 keeps isolated apex caps and multi-star ordered caps intact, then routes two-edge corners through a dedicated ordered corner-patch builder.

Active edge-wear flow:

```text
final source PolygonFace polyhedron after all mass cuts/chips
→ source topology graph / edge-face adjacency
→ selected convex graph edges
→ per-source-face local offset replacement polygons
→ per-selected-edge local ConvexEdgeWear bridge faces
→ per-source-vertex local ConvexEdgeWear cap triangles ordered from the source vertex star
→ final topology audit
→ triangle-emission preview
→ UV2.z / vertex alpha ConvexEdgeWear markers
```

Implementation facts:

```text
- Active edge-wear construction enters MassGenerator.TryBuildDeterministicSelectedEdgeBevelKernelFaces(...).
- The active function does not call retired local-bevel, sampled-ribbon, workspace, open-cycle closure, or global selected-edge cut construction.
- EW-B3R3 emits local bridge geometry, keeps source-vertex-star ordered cap triangles for multi-edge stars, uses original-source-vertex apex triangles only for isolated endpoints, and uses ordered unique corner-patch triangles for two-edge corners.
- ConvexEdgeWear bevel/cap polygons are triangulated before commit, because final render fans ConvexEdgeWear faces.
- Topology audit and triangle preview remain mandatory before commit.
```

The EW-B3R implementation is a deliberate source-graph emission step, not a render-triangle mesh post-process.

### EW-B3R3 validation target

EW-B3R3 fixes the EW-B3R2 validation result: local face offsets, rails, and all selected-edge bevel bridges were built; isolated caps reached `2/2`; multi-star caps stayed at `18/18`; two-edge corner caps remained the blocker at `1/8` built with `7` area failures. The active blocker is therefore two-edge corner cap construction, not multi-star ordering, isolated endpoint caps, edge selection, rails, shader output, or global clipping.

Expected success indicators:

```text
deterministicKernelGlobalCutsApplied=0
deterministicKernelFaceOffsetPolygonsBuilt=16
deterministicKernelRailsBuilt=84
deterministicKernelLocalBevelFacesBuilt=36
deterministicKernelVertexCapsAttempted=28
deterministicKernelVertexCapsBuilt=28
deterministicKernelVertexCapFailures=0
deterministicKernelVertexCapLowValenceBuilt=10
deterministicKernelVertexCapLowValenceFailed=0
deterministicKernelOpenEdgesAfterBuild=0
deterministicKernelTJunctionsAfterBuild=0
```

---

## 1. Purpose

Generated Mass is the reusable compact-mass framework for procedural rocks, boulders, ice chunks, ore chunks, ruin fragments, monoliths, fossils, and similar compact generated objects.

It owns:

```text
base compact-mass shape generation
surface feature data
feature-budget policy
main-mesh feature support such as bevels/chamfers/grooves
shader/material interpretation
feature-oriented inspector controls
debug views for validating generated data
```

Core representation rule:

```text
Use the representation that matches the feature.
Hard edge features belong in mesh geometry or mesh-carried per-edge/per-face data.
Broad soft fields may use vertex masks, procedural shader data, or temporary/debug atlases when justified.
Do not force hard convex edge wear into packed low-resolution runtime atlases.
```

---

## 2. Current implementation facts

```text
GeneratedMass.cs
- FormComplexity and SurfaceFacetDensity are separate artist-facing controls.
- GenerationBudget still caps generated support-data cost.
- Normal-render convex edge wear passes MassSurfaceFeatureSettings into MassGenerator.
- Feature atlases are generated only for temporary/debug boundary Surface Mask Debug views.

GeneratedMassFeatureAtlasBaker.cs
- Retained as a temporary/debug boundary-field baker.
- The atlas path is not the final representation for hard convex edge wear.

MassGenerator.cs
- FormComplexity controls major cut count / dominant plane count.
- SurfaceFacetDensity controls surface triangulation density across major planes.
- The rendered mesh emits one rendered vertex per triangle corner.
- EW-B is the active recovery target for generated convex edge-wear bevel geometry.
- EW-B3 emits deterministic local bevel faces and cap patches from the final source polyhedron graph before triangulation.
- Retired local-bevel, sampled-ribbon/workspace, and open-cycle closure construction code is not used by the active path.

SH_PixelSurfaceLit.shader and PixelSurface generated-mass includes
- FeatureAtlas0/1 sampling remains available for boundary debug modes.
- Normal rendering no longer samples FeatureAtlas0/1 for convex edge-wear material response.
- Normal rendering shades UV2.z-marked generated geometry with generated mass edge-wear material controls.
```

---

## 3. Active convex edge-wear architecture

EW-B target:

```text
source PolygonFace list
→ source topology graph / edge-face adjacency
→ selected convex graph edges
→ deterministic selected-edge bevel kernel
→ source-owned local bevel faces on the final source polyhedron
→ ConvexEdgeWear bevel/cap triangles
→ final topology audit
→ UV2.z / vertex color ConvexEdgeWear markers
```

EW-B3 emits bevel geometry by building source-face replacement polygons, local edge bridge faces, and source-vertex cap patches from shared graph-owned records. This handles dense selected-edge networks directly from the source graph and avoids both retired ribbon closure and EW-B2 global slice/gouge artifacts.

---

## 4. Performance notes

The logged source topology for the current validation rock is:

```text
source faces = 16
source edges = 44
source vertices = 29
selected edges = 36
affected faces = 16
affected vertices = 28
```

The retired EW-4D0.7 successful-but-artifacted path estimated roughly:

```text
committedConvexEdgeWearFaces = 776
committedConvexEdgeWearTrianglesEstimate = 1681
committedConvexEdgeWearRenderedVerticesEstimate = 5043
```

The EW-B3 local source-owned path should remain dramatically cheaper than EW-4D because it emits one face-local replacement polygon per source face, one local bridge face per selected edge, and one cap patch per affected vertex. It does not create sampled profile grids or hundreds of ribbon quads before closure.

---

## 5. Current validation target

EW-B3 succeeds only if:

```text
Unity compiles.
The active route reports deterministicKernelGeometryPending=0.
deterministicKernelLocalBevelFacesBuilt > 0.
committedConvexEdgeWearFaces > 0.
committedConvexEdgeWearNgonFaces = 0.
triangulationPreviewSkippedConvexEdgeWearTriangles = 0.
topologyOpenEdges = 0.
topologyNonManifoldEdges = 0.
topologyTJunctions = 0.
Surface Mask Debug / Convex Edge Wear shows actual generated geometry.
```

## Next work items

1. Validate EW-B3 on the current 36-selected-edge mass.
2. If local emission fails, inspect faceOffset/rails/localBevel/vertexCap and topology-after-build counters.
3. If geometry commits, assess visual bevel width and sliver/clipping behavior before adding variation or material-mask expansion.


EW-B3R3 note: two-edge corner vertices must not use the isolated endpoint apex cap path; they use ordered unique cap points and centre/boundary triangulation instead.
