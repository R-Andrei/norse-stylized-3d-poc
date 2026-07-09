# Generated Mass Framework

Status: active framework definition  
Current implementation target: **EW-B — Deterministic Selected-Edge Bevel Kernel**  
Current implementation step: **EW-B1S — Legacy Bevel Construction Purge**  
Supersedes as active edge-wear routes: atlas-first/runtime edge-wear plans, EW-4A global cuts, EW-4B local strips, EW-4C half-space bevel planes, and EW-4D/R/R2/R3 sampled-ribbon plus open-cycle-closure bevel construction.

## 0. EW-B1S implementation status

EW-B1S is a cleanup/foundation patch after the first EW-B1 attempt proved that the active kernel still delegated construction to retired local-bevel code. The patch strips stale bevel-construction systems from `MassGenerator.cs` and leaves only the viable foundation for the deterministic kernel:

```text
source topology graph
→ selected convex graph-edge mapping
→ deterministic selected-edge records
→ deterministic affected-vertex records
→ clean EW-B diagnostics
→ fail closed before geometry emission
```

Implementation facts:

```text
- Active edge-wear construction still enters MassGenerator.TryBuildDeterministicSelectedEdgeBevelKernelFaces(...).
- The active function no longer calls TryBuildLocalEdgeWearBevelFaces(...) or any EW-4D/R3 construction route.
- Retired local-bevel, half-space, sampled-profile, ribbon/workspace, corner-patch, T-junction-repair, and open-cycle-closure construction code has been removed from MassGenerator.cs.
- Legacy ribbon/open-cycle diagnostic spam is removed from the active summary.
- The current patch intentionally emits no bevel geometry; EW-B1R is the next geometry patch.
```

EW-B1S is not a visible feature patch. Its purpose is to prevent the next bevel implementation from accidentally building on expired construction code.

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
- EW-B is now the active recovery target for generated convex edge-wear bevel geometry.
- EW-B1S routes active edge-wear construction through the deterministic selected-edge bevel-kernel entry point.
- EW-B1S stops after graph, selected-edge, and affected-vertex classification.
- Retired EW-4B/EW-4C/EW-4D/R3 construction code has been removed rather than kept as an inactive construction fallback.

SH_PixelSurfaceLit.shader and PixelSurface generated-mass includes
- FeatureAtlas0/1 sampling remains available for boundary debug modes.
- Normal rendering no longer samples FeatureAtlas0/1 for convex edge-wear material response.
- Normal rendering shades UV2.z-marked generated geometry with generated mass edge-wear material controls.
```

---

## 3. Active convex edge-wear architecture

EW-B pipeline target:

```text
source PolygonFace list
→ source topology graph / edge-face adjacency
→ selected convex graph edges
→ deterministic selected-edge bevel kernel
→ explicit source-face replacement faces
→ explicit selected-edge bevel faces
→ explicit affected-vertex cap / endpoint / transition faces
→ final topology audit
→ UV2.z / vertex color ConvexEdgeWear markers
```

The active kernel must generate bevel topology from source ownership:

```text
source face owns replacement face boundaries
selected source edge owns one bevel face in EW-B1
affected source vertex owns the cap/transition geometry connecting incident bevels and replacement faces
```

The active kernel must not infer the core bevel closure by walking leftover open-edge components after emitting ribbons.

---

## 4. EW-B kernel plan

### EW-B0 — Edge Wear Bevel Kernel Reconciliation — complete

Implementation intent:

```text
- Deactivate EW-4D/R3 sampled-ribbon/open-cycle closure as the active route.
- Temporarily kept graph/candidate evidence and old helper code available; EW-B1S now removes retired construction code from the source.
- Update docs so EW-B is the only active plan.
- Add a deterministic bevel-kernel entry point in MassGenerator.cs.
- Fail closed after graph build and selected-edge mapping until EW-B1 implements geometry.
```

Expected validation:

```text
Unity compiles.
Docs identify EW-B0 / EW-B as current.
ApplyGeneratedEdgeWearBevels no longer calls TryBuildTopologyGraphEdgeWearBevelFaces as the active route.
Regeneration, if edge wear is enabled, reports deterministicKernelPending=1 and leaves source geometry unchanged.
```

### EW-B1S — Legacy Bevel Construction Purge — current

Implementation target:

```text
- Strip retired bevel-construction systems from MassGenerator.cs.
- Keep candidate selection and topology graph mapping as the only retained pre-kernel foundation.
- Add/keep EW-B-specific edge and vertex records.
- Fail closed before geometry emission with clean deterministic-kernel diagnostics.
- Do not emit bevel geometry in this cleanup patch.
```

Current implementation:

```text
- Removes retired bevel-construction functions and types from MassGenerator.cs.
- Leaves only source graph/candidate mapping, EW-B edge records, EW-B vertex records, topology audit, triangle preview, and material-marker plumbing.
- Reports deterministicKernelGeometryPending=1 and clean EW-B classification counters.
- Emits no bevel geometry in this patch.
```

Next geometry step: **EW-B1R — Clean Isolated-Edge Bevel Case**.

### EW-B2 — Vertex cap correctness

Explicitly handle affected vertex cases:

```text
one selected incident edge: endpoint cap/transition
two selected incident edges: corner transition
three or more selected incident edges: vertex cap polygon/triangle set
mixed selected/unselected vertex star: deterministic transition geometry
```

### EW-B3 — Real generated-rock validation

Apply the kernel to normal generated rocks using existing candidate scoring and edge-wear controls.

### EW-B4 — Profile and irregularity

Add multi-segment profiles, width variation, and along-edge irregularity only after the constant-width kernel is watertight.

### EW-B5 — Mask/shader/material refinement

Expand material response after physical geometry is stable.

---

## 5. Superseded EW-4D evidence retained for context

EW-4D proved useful sub-systems:

```text
- topology graph construction from PolygonFace lists
- selected candidate to graph-edge mapping
- face/edge diagnostics
- ConvexEdgeWear material marker plumbing
- topology audit utilities
```

EW-4D also proved the active sampled-ribbon/open-cycle closure architecture is not a good foundation:

```text
- EW-4D0.7 committed a closed polygon workspace but showed render/debug slivers.
- EW-4D0.7R/R2/R3 moved cap triangulation before final audit.
- R3 failed in cap triangulation and stopped committing any bevel geometry.
- The failure occurred after 750 ribbon faces had been built, so the post-ribbon closure stage became the whole feature blocker.
```

EW-4D0.8 density/budget tuning is cancelled as the next step. Density work resumes only after EW-B1/B2 geometry is valid.

---

## 6. Budget stance

Correctness comes first, but the EW-B direction is also a better performance foundation.

Known EW-4D scale from prior successful validation:

```text
committedConvexEdgeWearFaces=776
committedConvexEdgeWearTrianglesEstimate=1681
committedConvexEdgeWearRenderedVerticesEstimate=5043
```

EW-B1R should begin from a smaller topology target:

```text
one bevel quad/face per selected edge
plus explicit small affected-vertex cap/transition geometry
```

For the repeated validation case:

```text
selectedGraphEdges=36
graphVertices=29
```

A constant-width kernel should be measured in dozens of primary bevel faces plus caps, not hundreds of sampled ribbon faces before closure.

---

## 7. Non-goals for the next implementation step

EW-B1R must not add:

```text
mask widening
shader changes
fine cracks
grooves
moss/dirt
variable width
profile softness
random chipped rails
atlas dependence
post-ribbon open-cycle closure
```
