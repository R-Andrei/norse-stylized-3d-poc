# Generated Mass Framework

Status: active framework definition  
Current implementation target: EW-4D0 — Variable-Profile Topology Bevel Graph  
Current implementation step: EW-4D0.6T3 — T-Junction-Safe Open-Cycle Polygon Closure  
Supersedes: atlas-first/runtime edge-wear plans, EW-4A global cuts, EW-4B local strips, and EW-4C.0 half-space bevel planes as production convex edge-wear construction paths.

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
- Feature atlases are generated only for temporary boundary Surface Mask Debug views.

GeneratedMassFeatureAtlasBaker.cs
- Retained as a temporary/debug boundary-field baker.
- The atlas path is not the final representation for hard convex edge wear.

MassGenerator.cs
- FormComplexity controls major cut count / dominant plane count.
- SurfaceFacetDensity controls surface triangulation density across major planes.
- The rendered mesh emits one rendered vertex per triangle corner.
- EW-4D0 is the active recovery target for generated convex edge wear on plane-cut masses.
- EW-4D0.1 built a real topology graph and maps selected candidates before any new geometry is emitted.
- EW-4D0.2 preflights per-face selected-edge clipping and shared rail extraction.
- EW-4D0.3 preflights sampled profile grids from paired rails.
- EW-4D0.4 builds temporary clipped base-face replacement workspaces.
- EW-4D0.5 inserts protected rail samples into those base-face boundaries and appends sampled ConvexEdgeWear ribbon faces to the temporary workspace.
- EW-4D0.6 added shared corner vertex patches from endpoint profile arcs in the temporary workspace, but validation showed partial unsafe closure: generated corner faces can still leave substantial open workspace edges.
- EW-4D0.6R made corner generation transactional, removed degenerate corner faces, removed unsafe Vector3.up normal fallback, and split remaining open edges into near-graph-vertex and away-from-graph-vertex counts. Validation proved the remaining failure is not purely corner-local: the current baseline is cornerPatchFailed=2, workspaceOpenEdgesAfterCorners=64, workspaceOpenEdgesNearGraphVerticesAfterCorners=30, workspaceOpenEdgesAwayFromGraphVerticesAfterCorners=34.
- EW-4D0.6R2 proved the post-ribbon open topology is structured: 226 open edges in 22 components, with no leaves or branches. EW-4D0.6T proved the open-cycle trace is valid but exposed one overly strict aggregate-cycle-normal failure. EW-4D0.6T2 proved per-triangle fan closure can close all open edges and non-manifold edges, but the radial centre-fan diagonals introduced T-junctions: openCycleClosureComponentsBuilt=22, openCycleClosureFacesBuilt=226, workspaceOpenEdgesAfterComponentClosure=0, workspaceNonManifoldEdgesAfterComponentClosure=0, workspaceTJunctionsAfterComponentClosure=19. EW-4D0.6T3 is the active refinement: close each traced open cycle as one topology-owned ConvexEdgeWear polygon cap, avoiding centre-fan radial edges while preserving actual open-edge boundary order.
- Later EW-4D0 steps will final-audit the complete workspace and commit only after topology audit.

SH_PixelSurfaceLit.shader
- FeatureAtlas0/1 sampling remains available for boundary debug modes.
- Normal rendering no longer samples FeatureAtlas0/1 for convex edge-wear material response.
- Normal rendering shades UV2.z-marked bevel/chamfer/ribbon faces with generated mass edge-wear material controls.
```

---

## 3. Active convex edge-wear architecture

EW-4D0 pipeline:

```text
source PolygonFace list
→ topology graph
→ selected convex graph edges
→ affected-face clipping
→ shared rail extraction
→ sampled profile-grid preparation
→ clipped base-face replacement
→ variable-profile bevel ribbon emission
→ open-edge component diagnostics after ribbons
→ transactional open-cycle polygon caps without centre-fan radial edges
→ final topology audit
→ UV2.z / vertex color ConvexEdgeWear markers
```

The current implementation is still in rebuild-workspace stages. EW-4D0.6 proved that shared `ConvexEdgeWear` corner vertex patches can be generated from endpoint profile arcs, but the fan implementation was not topology-owned enough. EW-4D0.6R then proved that transactionality/dedupe/normal hardening fixed corner degenerates, but not total closure. EW-4D0.6R2 proved the decisive next fact: after ribbons, the workspace has 226 open edges grouped into 22 clean cycle-like components with no leaf or branch endpoints; after point-cloud corner fans, the remainder becomes branched. EW-4D0.6T moved closure to actual post-ribbon open cycles and proved cycle tracing is valid, but failed on one aggregate polygon normal. EW-4D0.6T2 replaced the whole-cycle normal requirement with per-triangle closure normals and proved open edges/non-manifold edges can be solved, but exposed 19 T-junctions from centre-fan radial triangulation. EW-4D0.6T3 keeps the same topology-owned closure source but emits one ordered polygon cap per traced cycle so the topology audit sees only the real boundary edges that must be paired.

It still does not final-commit visible bevel geometry because final topology validation and the active-path switch are reserved for EW-4D0.7. Until final commit is implemented, `Surface Mask Debug = Convex Edge Wear` may remain black even if `workspaceOpenEdgesAfterComponentClosure == 0`.

---

## 4. Full EW-4D0 roadmap

```text
EW-4D0.1 — topology graph foundation — completed
EW-4D0.2 — per-face selected-edge clipping / shared rail preflight — completed
EW-4D0.3 — rail/profile storage and sampled profile-grid preflight — completed
EW-4D0.4 — clipped base-face replacement workspace — completed
EW-4D0.5 — rail-sampled base boundaries + sampled variable-profile bevel ribbon emission — completed
EW-4D0.6 — corner vertex patches — completed but unsafe / superseded by EW-4D0.6R
EW-4D0.6R — corner patch hardening / failure diagnostics — completed as containment
EW-4D0.6R2 — workspace open-edge loop diagnostics — completed as proof
EW-4D0.6T — actual open-cycle closure after ribbons — completed as topology-trace proof
EW-4D0.6T2 — per-triangle open-cycle closure normals — completed as open-edge proof
EW-4D0.6T3 — T-junction-safe open-cycle polygon closure — current
EW-4D0.7 — final topology validation and active-path switch
EW-4D0.8 — variation tuning
EW-4D1   — corner quality refinement and crack/groove planning
```

Roadmap rule:

```text
Each step must leave docs current and must expose enough stats to prove whether the step succeeded before the next geometry layer is added.
```
EW-4D0.6T3 acceptance rule:

```text
Use the actual post-ribbon open-edge topology as the closure source of truth.
Trace open-edge connected components only when every endpoint has valence 2.
Build closure caps into a temporary list first and append them only if all cycles pass.
One closure polygon cap is expected per traced open-edge component, not one centre-fan triangle per open edge.
Do not introduce radial centre-fan edges during topology proof; they already proved they close open edges but create T-junctions.
Use a robust cap normal derived from aligned per-edge triangle normals only for orientation/material data; topology ownership comes from the ordered cycle boundary.
The target proof is workspaceOpenEdgesAfterComponentClosure == 0, workspaceNonManifoldEdgesAfterComponentClosure == 0, and workspaceTJunctionsAfterComponentClosure == 0.
Even a successful EW-4D0.6T3 closure remains workspace-only; it must not fall through into the obsolete EW-4C half-space path.
```


---

## 5. Control contract

```text
Edge Wear Amount:
  generation enable/strength and material response strength.

Edge Wear Width:
  base physical bevel depth before variation.

Edge Wear Coverage:
  selected eligible convex edge fraction.

Edge Wear Softness:
  material/normal/profile response. It should not secretly change physical bevel width in the current path.

Macro Variation:
  future per-edge variation: width, profile curve, material strength, chip tendency.

Micro Variation:
  future along-edge variation: taper, noise, chip masks, safe rail wobble.
```

Selected candidates are authored intent. EW-4D should not hide errors by silently dropping individual selected edges. If the full selected set is too aggressive, global width/profile richness may scale down, or the whole result should fail closed.

---

## 6. Budget and performance policy

Dirty-time generation may be more expensive because generated masses can be cached or generated offline.

Runtime priorities:

```text
avoid unique per-object multi-megabyte runtime atlases
avoid runtime per-frame generation work
avoid secondary overlay renderers as final output
allow more mesh triangles within budget
allow existing UV2.z / vertex color material markers
```

Active budget tiers:

| Budget | Rendered vertex target | Temporary debug atlas cap | Intended use |
|---|---:|---:|---|
| Compact | <= 800 rendered verts | 128 | Small/simple gameplay masses |
| Standard | <= 1,600 rendered verts | 256 | Default gameplay rocks and masses |
| Detailed | <= 3,000 rendered verts | 256 | Important/larger/visually exposed masses |
| Hero | <= 8,000 rendered verts | 512 | Showcase, inspected, very large, or debug/hero masses |
| Custom / Debug | manual | manual | Testing and deliberate override only |

If generated cost exceeds budget, clamp in this order:

```text
1. optional/debug atlas resolution first, if a debug atlas is actually requested
2. bevel/chamfer richness or selected edge count
3. SurfaceFacetDensity
4. FormComplexity last
```

---

## 7. Temporary atlas policy

Atlases are not generated because an object is a Generated Mass. Atlases are not generated for normal-render convex edge wear.

Atlases may be generated only because an active authoring/debug view requests them or because a future broad-mask feature explicitly justifies them.

Do not use FeatureAtlas0/1 for:

```text
convex edge wear
edge-local hard highlights
thin cracks/creases
chipped edge lines
```

---

## 8. Supported archetype scope for EW-4D0

EW-4D0 targets plane-cut mass archetypes first:

```text
TerrainBoulder
SquatBoulder
StandingStone
FlatSlab
BrokenChunk
FracturedPillar
```

Separate paths may be needed later for:

```text
PolishedStone
LayeredStone
CarvedMarkerStone
```

---

## 9. Data emission contract

Final EW-4D bevel/ribbon/corner faces should write:

```text
UV2.z = generated convex edge-wear strength
Vertex Color A = same marker for inspection/backward compatibility
PolygonFaceFeature = ConvexEdgeWear
```

The existing shader response should read the generated marker. Shader changes are not part of EW-4D preflight unless validation later proves the existing response path is insufficient after geometry exists.

---

## 10. Validation policy

A step is not complete because code exists. It is complete only when console stats prove the step succeeded.

Current EW-4D0.6T3 success target:

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
ribbonEdgesPrepared == selectedGraphEdges
ribbonEdgesFailed == 0
ribbonFacesBuilt == ribbonFacesExpected
ribbonDegenerateFaces == 0
ribbonInvalidFaces == 0
workspaceOpenEdgesAfterRibbons is classified by components/features/proximity
workspaceOpenEdgeComponentsAfterRibbons > 0
workspaceOpenEdgeEndpointLeavesAfterRibbons == 0
workspaceOpenEdgeEndpointBranchesAfterRibbons == 0
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

Visible bevels are not expected until the final commit step. EW-4D0.6T3 is a topology proof only; EW-4D0.7 owns final commit and visible active-path switch.
