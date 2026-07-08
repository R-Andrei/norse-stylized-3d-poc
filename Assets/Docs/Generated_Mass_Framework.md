# Generated Mass Framework

Status: active framework definition  
Current implementation target: EW-4D0 — Variable-Profile Topology Bevel Graph  
Current implementation step: EW-4D0.6 — Corner Vertex Patches  
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
- EW-4D0.6 adds shared corner vertex patches from endpoint profile arcs in the temporary workspace.
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
→ corner vertex patches
→ topology audit
→ UV2.z / vertex color ConvexEdgeWear markers
```

The current implementation is still in rebuild-workspace stages. EW-4D0.6 appends shared `ConvexEdgeWear` corner vertex patches to the temporary workspace after rail-sampled base faces and sampled bevel ribbon faces are built. It still does not final-commit visible bevel geometry because final topology validation and the active-path switch are reserved for EW-4D0.7. Until final commit is implemented, `Surface Mask Debug = Convex Edge Wear` may remain black even when ribbon and corner-patch workspace construction succeeds.

---

## 4. Full EW-4D0 roadmap

```text
EW-4D0.1 — topology graph foundation — completed
EW-4D0.2 — per-face selected-edge clipping / shared rail preflight — completed
EW-4D0.3 — rail/profile storage and sampled profile-grid preflight — completed
EW-4D0.4 — clipped base-face replacement workspace — completed
EW-4D0.5 — rail-sampled base boundaries + sampled variable-profile bevel ribbon emission — completed
EW-4D0.6 — corner vertex patches — current
EW-4D0.7 — final topology validation and active-path switch
EW-4D0.8 — variation tuning
EW-4D1   — corner quality refinement and crack/groove planning
```

Roadmap rule:

```text
Each step must leave docs current and must expose enough stats to prove whether the step succeeded before the next geometry layer is added.
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

Current EW-4D0.6 success target:

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
cornerPatchVertices > 0
cornerPatchesBuilt > 0
cornerPatchFacesBuilt > 0
cornerPatchFailed == 0
cornerPatchDegenerateFaces == 0
workspaceOpenEdgesAfterCorners <= workspaceOpenEdgesAfterRibbons
```

Visible bevels are not expected until the final commit step. Temporary workspace open edges are still reported at EW-4D0.6 for diagnosis; they become hard pass/fail criteria in EW-4D0.7 final topology validation.
