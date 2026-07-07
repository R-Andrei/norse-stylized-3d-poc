# Generated Mass Framework

Status: active framework definition  
Current implementation patch: EW-4B.4 — Candidate-Local Bevel Validation  
Supersedes: older Patch 14C/14D and EW-3A.1 through EW-3A.6 atlas-first/runtime edge-wear plans.

---

## 1. Purpose

The Generated Mass system is the reusable compact-mass framework for procedural rocks, boulders, ice chunks, ore chunks, ruin fragments, sacred monoliths, bone/fossil chunks, and similar compact generated objects.

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

Core rule:

```text
Generated Mass features must use the representation that matches the visual problem.
Line/edge features belong in mesh geometry or mesh-carried per-edge/per-face data.
Broad soft fields may use vertex masks, procedural shader data, or temporary/debug atlases when justified.
Do not force hard edge wear into packed low-resolution texture atlases.
```

---

## 2. Current implementation facts

```text
GeneratedMass.cs
- FormComplexity and SurfaceFacetDensity are separate artist-facing controls.
- GenerationBudget still caps generated support-data cost.
- Normal-render convex edge wear now passes MassSurfaceFeatureSettings into MassGenerator.
- Feature atlases are generated only for temporary boundary Surface Mask Debug views.

GeneratedMassFeatureAtlasBaker.cs
- Retained as a temporary/debug boundary-field baker.
- Can generate FeatureAtlas0 only, FeatureAtlas0+FeatureAtlas1, or no atlas.
- Generated atlas upload uses Apply(false, true), discarding CPU-readable texture memory after upload.
- The atlas path is not the final representation for hard convex edge wear.

MassGenerator.cs
- FormComplexity controls major cut count / dominant plane count.
- SurfaceFacetDensity controls surface triangulation density across major planes.
- The rendered mesh emits one rendered vertex per triangle corner.
- EW-4A applies generated convex bevel/chamfer cuts after plane-cut shape cuts and before triangulation.
- Bevel/chamfer cap faces are marked as ConvexEdgeWear and emitted through UV2.z.
- EW-4A affects plane-cut archetypes first; radial, layered and carved-marker archetypes are intentionally unchanged in this first pass.

SH_PixelSurfaceLit.shader
- FeatureAtlas0/1 sampling remains available for boundary debug modes.
- Normal rendering no longer samples FeatureAtlas0/1 for convex edge-wear material response.
- Normal rendering shades UV2.z-marked bevel/chamfer faces with the generated mass edge-wear material controls.
```

Important conclusion:

```text
The atlas-first edge-wear path was a failed representation choice.
EW-4A is the first production replacement: main-mesh bevel/chamfer edge wear on plane-cut masses with bevel-face material markers. Explicit custom-normal refinement remains a later step if the first-pass bevel normals are not sufficient.
```

---

## 3. Why runtime edge-wear atlases were decommissioned

The previous atlas path tried to represent convex edge wear as a packed boundary distance field.
That failed at Compact/128 and remained marginal at Standard/Detailed/256 because the authored edge feature is narrower than the available atlas texel density.

Approximate cube-like chart math:

```text
featureWidthWorld = objectMaxDimension * 0.018 * EdgeWearWidth

128 atlas:
  featureWidth ≈ 0.75 atlas px
  useful outer field ≈ 0.6 atlas px
  hard ridge core ≈ 0.01–0.02 atlas px

256 atlas:
  featureWidth ≈ 1.55 atlas px
  useful outer field ≈ 1.3 atlas px
  hard ridge core ≈ 0.02–0.05 atlas px

512 atlas:
  featureWidth ≈ 3.1 atlas px
  useful outer field ≈ 2.6 atlas px
```

A sub-pixel hard edge core cannot be made reliable by more sampling, cross-coordinate averaging, dominant groups, or shader interpretation. Those patches mostly disguised symptoms.

The correct representation for hard edge wear is:

```text
actual generated bevel/chamfer/groove geometry
+ bevel-face or edge-strip material markers
+ bevel/custom normals
+ simple shader response on marked geometry
```

---

## 4. Generated Mass feature budget policy

### 4.1 Budget is a numeric ceiling, not a style preset

The inspector may present named budgets, but the implementation must treat them as numeric cost ceilings.

```text
artist-requested shape settings
+ active generated feature requirements
+ budget target
→ resolved effective settings
```

A requested setting is preserved if the estimated/generated cost fits the active budget. It is clamped only when the math says it exceeds the budget.

### 4.2 Active budget tiers

There is no `Background` tier for this game. The project uses an isometric action/roguelike camera, and generated masses are generally reachable gameplay objects.

Use these budgets:

| Budget | Rendered vertex target | Temporary debug atlas cap | Intended use |
|---|---:|---:|---|
| Compact | <= 800 rendered verts | 128 | Small/simple gameplay masses |
| Standard | <= 1,600 rendered verts | 256 | Default gameplay rocks and masses |
| Detailed | <= 3,000 rendered verts | 256 | Important/larger/visually exposed masses |
| Hero | <= 8,000 rendered verts | 512 | Showcase, inspected, very large, or debug/hero masses |
| Custom / Debug | manual | manual | Testing and deliberate override only |

The budget becomes the cap for EW-4 bevel/chamfer geometry and other generated support data. The atlas cap is now only for temporary debug views unless a future broad-mask feature proves a production atlas is actually necessary.

### 4.3 Approximate current mesh estimates

Current estimates assume the existing plane-cut/facet-density model and one rendered vertex per triangle corner:

| Budget | Default effective shape | Estimated normal verts | Estimated harsh/fractured verts | Budget fit |
|---|---|---:|---:|---|
| Compact | Simple + Medium | ~504 | ~756 | Fits <= 800 |
| Standard | Complex + Medium | ~864 | ~1,152 | Fits <= 1,600 |
| Detailed | Complex + High | ~1,296 | ~1,728 | Fits <= 3,000 |
| Hero | HighlyComplex + High | ~1,674 | ~2,160 | Fits <= 8,000 with room for bevels |
| Hero Max / Debug | HighlyComplex + VeryHigh | ~2,232 | ~2,880 | Still acceptable before bevels |

### 4.4 Clamp order after EW-4

When generated or estimated cost exceeds the active budget, clamp in this order:

```text
1. Reduce optional/debug atlas resolution first, if an atlas is actually requested by a debug view.
2. Reduce bevel/chamfer richness or selected edge count.
3. Reduce SurfaceFacetDensity.
4. Reduce FormComplexity last.
```

Reason:

```text
FormComplexity changes the major silhouette and should be protected longest.
EW-4 geometry cost is visible but local and controllable.
Temporary debug atlas cost is not part of the shipping edge-wear solution.
```

---

## 5. EW-4A geometry edge-wear policy

EW-4A implements convex edge wear as generated main-mesh bevel/chamfer geometry for plane-cut mass archetypes.

```text
Supported in EW-4A:
  TerrainBoulder
  SquatBoulder
  StandingStone
  FlatSlab
  BrokenChunk
  FracturedPillar

Not yet affected in EW-4A:
  PolishedStone
  LayeredStone
  CarvedMarkerStone
```

Controls are wired as follows:

```text
Edge Wear Amount:
  enables/disables bevel generation and contributes to bevel-face material strength

Edge Wear Width:
  controls bevel/chamfer cut depth

Edge Wear Coverage:
  controls how many eligible convex edges are selected

Edge Wear Softness:
  makes the first-pass bevel cuts shallower/less aggressive

Response Strength / Brightness Lift / Tint / Tint Strength:
  control shader response on UV2.z-marked bevel faces

Macro Variation / Micro Variation:
  retained for later richer per-edge and along-edge variation; first pass uses deterministic edge scoring only
```

EW-4A writes:

```text
UV2.z = generated convex edge-wear strength on actual bevel/chamfer faces
Vertex Color A = same edge-wear strength for inspection/backward compatibility
```

The first pass relies on generated bevel face normals produced by the existing mesh-normal path. It does not add a new mesh channel and does not touch ground or other procedural mesh generators.

---

## 6. Temporary atlas policy

Atlases are not generated because an object is a Generated Mass. Atlases are not generated for normal-render convex edge wear.

Atlases may be generated only because an active authoring/debug view requests them.

### 6.1 FeatureAtlas0 — temporary boundary structure debug atlas

```text
R = convex boundary proximity
G = concave boundary proximity
B = dominant boundary structural salience
A = dominant boundary stable identity / seed
```

### 6.2 FeatureAtlas1 — temporary boundary coordinate/modulation debug atlas

```text
R = dominant boundary along-chain coordinate / phase
G = dominant boundary cross-boundary coordinate
B = dominant boundary coarse local modulation
A = dominant boundary fine local modulation
```

These atlases are retained only as temporary diagnostic tools. They are not the foundation for final convex edge wear.

### 6.3 Current atlas request policy

```text
Surface Mask Debug = None:
  FeatureAtlas0 not required.
  FeatureAtlas1 not required.

Surface Mask Debug requires Atlas0 diagnostics:
  FeatureAtlas0 required.
  FeatureAtlas1 not required unless the selected debug mode needs it.

Surface Mask Debug requires Atlas1 diagnostics:
  FeatureAtlas0 required.
  FeatureAtlas1 required.
```

Edge Wear Amount, Width, Coverage, Softness, Response Strength, Brightness Lift and Tint now drive EW-4A geometry edge wear. Macro Variation and Micro Variation are retained for later richer geometry variation. None of these controls make FeatureAtlas0/1 visible in normal rendering.

### 6.4 Temporary atlas memory table

Per RGBA32 atlas:

| Setup | GPU atlas memory |
|---|---:|
| No atlas | 0 MB |
| 128 Atlas0 only | ~0.0625 MB |
| 128 Atlas0 + Atlas1 | ~0.125 MB |
| 256 Atlas0 only | ~0.25 MB |
| 256 Atlas0 + Atlas1 | ~0.5 MB |
| 512 Atlas0 only | ~1.0 MB |
| 512 Atlas0 + Atlas1 | ~2.0 MB |

A two-atlas 512 debug setup costs about 2 MB per generated mass. This is acceptable for debug/hero inspection but not as the default solution for hard edge wear.

Generated feature atlas textures must continue to discard CPU-readable copies after upload:

```csharp
atlas.Apply(false, true);
```

---

## 7. EW-4A implementation notes

EW-4A implements convex edge wear as generated main-mesh geometry for plane-cut mass archetypes.

Implemented target:

```text
identify eligible convex edges after all plane-cut shape cuts
create narrow bevel/chamfer cap faces on the main mesh
mark bevel faces through UV2.z
mirror the marker into Vertex Color A for inspection/backward compatibility
triangulate bevel faces minimally so surface facet density does not explode bevel cost
shade marked faces as worn material in SH_PixelSurfaceLit.shader
avoid FeatureAtlas0/1 in normal edge-wear rendering
```

Current first-pass normal policy:

```text
GeneratedMass already emits one rendered vertex per triangle corner.
The existing mesh normal recalculation therefore gives bevel/chamfer faces their own faceted normals.
Custom softened normals remain a later refinement, not part of EW-4A.
```

Preferred material-data channel:

```text
UV2.z = convex edge-wear / bevel-face strength
```

Existing UV2 contract already reserved Z for convex edge localization data. EW-4A uses it without adding a new vertex channel.

---

## 8. Cost comparison: atlas path vs bevel path

Two RGBA32 atlases:

```text
128 Atlas0+Atlas1 = 128 KiB
256 Atlas0+Atlas1 = 512 KiB
512 Atlas0+Atlas1 = 2,048 KiB
```

Simple generated bevel estimate:

```text
1 bevel quad = 2 triangles
current rendered mesh = one rendered vertex per triangle corner
2 triangles = 6 rendered vertices
estimated vertex cost ≈ 80 bytes / vertex without UV3 atlas channel
per bevel edge ≈ 6 * 80 + 12 index bytes ≈ 492 bytes
```

Examples:

| Selected bevel edges | Simple bevel memory |
|---:|---:|
| 24 | ~11.5 KiB |
| 48 | ~23 KiB |
| 80 | ~38 KiB |

Even richer bevels generally remain far below a unique 512 two-atlas path. Geometry also removes one or two runtime atlas texture samples per shaded pixel.

---

## 9. Feature representation map

| Feature | Preferred representation | FeatureAtlas0/1? |
|---|---|---:|
| Convex edge wear | Bevel/chamfer geometry + UV2.z marker + normals | No |
| Edge macro variation | Per-edge selection/strength during generation | No |
| Edge micro variation | Bevel segmentation, edge hash, along-edge procedural data | No |
| Concave cracks/creases | Groove/crease geometry or dark line strips | Probably no |
| Broad stone face variation | Vertex colors + object-space shader noise | No |
| Dirt/mineral deposits | UV2.y + height/normal/procedural noise | No |
| Frost/snow exposure | vertex exposure mask + normal/up vector + noise | No |
| Arbitrary high-frequency surface painting | TBD, possibly atlas/decal | Maybe |

Do not keep FeatureAtlas0/1 alive out of inertia. Keep them only until a future feature proves they are the best representation.

---

## 10. Patch history status

```text
EW-Atlas-1 through EW-3A.5:
  Useful for learning/debugging boundary facts.
  Superseded as final edge-wear architecture.
  Do not continue patching the atlas path for normal edge wear.

EW-3A.6:
  Decommissions runtime atlas-based edge wear.
  Keeps atlases as temporary debug tools only.
  Updates docs and inspector messaging to prevent stale atlas-first guidance.

EW-4A:
  Implements first-pass plane-cut main-mesh bevel/chamfer edge wear.
  Writes UV2.z bevel-face markers and shades them in normal rendering.
  Leaves radial/layered/carved-marker archetypes unchanged.

EW-4B:
  Next work item if validation requires it.
  Refine bevel normals, edge selection, depth tuning, or chip segmentation.
```


## EW-4A.1 bevel control and stability cleanup

EW-4A.1 keeps the geometry-first edge-wear direction but corrects the first-pass control contract. Width is now the only control that changes physical single-plane bevel depth. Amount controls generated worn-face strength, Coverage controls the selected fraction of eligible structural edges, and Softness is limited to material response until custom normals or multi-strip bevels exist.

The first-pass single-plane bevel now uses a conservative depth range because broad one-plane cuts read as shaved facets and can destabilize the global clipping method. Max Coverage now attempts all eligible structural candidates, with individual cuts allowed to fail if they would create unstable slivers or invalid faces. This is still the conservative global-cut implementation; the proper local edge-strip bevel remains a required later bevel milestone before the bevel system is considered complete.

## EW-4B local edge-strip bevel foundation

EW-4B supersedes the EW-4A/EW-4A.1 global clipping bevel prototype for normal GeneratedMass edge wear. Convex edge wear for plane-cut masses is now built as local edge-strip geometry: each selected structural edge trims only its two adjacent base faces, inserts a marked bevel strip between the two trimmed rails, and adds endpoint cap faces to close the bevel at edge ends.

This removes the main EW-4A failure mode where a bevel candidate produced a whole-polyhedron cut that could slice unrelated faces or leave long sliver/gap-like feature faces. The corrected control contract remains unchanged: Width controls physical chamfer depth, Amount controls generated worn-face material strength, Coverage controls selected eligible structural edges, and Softness is reserved for shader/normal response rather than physical size.

The implementation remains intentionally scoped to the plane-cut mass archetypes. It does not modify MeshData, MeshBuilder, GeneratedGround, the ground generator, or non-mass procedural meshes. FeatureAtlas0/1 remain debug-only and are not used by normal edge wear.

## EW-4B.1 robust local bevel assembly update

EW-4B validation showed that the first local-strip implementation could still fail closed when one selected edge or corner produced invalid rail/cap topology. In that case `ConvexEdgeWear` debug became fully dark because no `ConvexEdgeWear` faces survived to UV2.z. EW-4B.1 keeps the local-strip direction but changes the assembly policy:

- selected candidates are now accepted cumulatively;
- one invalid candidate is skipped instead of aborting the entire edge-wear pass;
- rail extraction now prefers actual clipped polygon edges aligned with the source edge instead of arbitrary near-plane point min/max selection;
- endpoint closure uses per-edge triangular cap faces instead of one merged non-planar cap polygon per original vertex;
- the pass still fails closed if no candidate can produce valid topology.

EW-4B.1 remains plane-cut only and does not add FeatureAtlas0/1 back into normal rendering.

## EW-4B.2 final response fix

EW-4B.1 validation showed the local bevel geometry and UV2.z mask were present in `Surface Mask Debug = ConvexEdgeWear`, but the normal final render stayed visually unchanged. EW-4B.2 fixes the final material response path rather than changing bevel topology.

Changes:

- Adds `_GeneratedMassGeometryEdgeWearEnabled`, set by `GeneratedMass` material property blocks.
- The normal edge-wear shader response now gates on this dedicated property and UV2.z instead of relying on `_SurfaceContract`.
- Brightness Lift now uses a bounded additive lift so worn bevel faces are visible on dark stone albedo.
- FeatureAtlas0/1 remain debug-only and are still not sampled by normal edge wear.

This patch is intended to make Response Strength, Brightness Lift, Tint Influence, and Amount-driven UV2.z strength visible in final render once the ConvexEdgeWear debug mask confirms bevel geometry exists.

## EW-4B.3 geometry diagnostics update

EW-4B.3 does not add a new visual debug mode. It repairs the current investigation path by using the existing `Surface Mask Debug = ConvexEdgeWear` view as the geometry-mask view and by adding code-level rejection statistics in `MassGenerator`.

Important distinction:

```text
ConvexEdgeWear = UV2.z geometry bevel/wear face mask.
Convex Boundary Proximity = temporary FeatureAtlas0.R boundary diagnostic.
```

Only `ConvexEdgeWear` proves that geometry bevel faces reached the final mesh data path. Convex Boundary Proximity can look correct even when no bevel geometry exists, because it is still atlas/debug data.

EW-4B.3 records local bevel candidate counts, selected counts, accepted counts, and concrete rejection buckets. If edge wear is enabled but no local bevel faces are accepted, the editor console reports the counts instead of silently returning the unmodified mass.

This patch is intentionally diagnostic/evidence-oriented. It does not change bevel selection, bevel width, shader response, or atlas policy.

## EW-4B.4 candidate-local bevel validation update

EW-4B.3 rejection evidence isolated the current geometry blocker: selected local bevel candidates were reaching construction, but every attempted build was rejected by the final whole-polyhedron `ValidatePolyhedronFaces(localRebuiltFaces, ...)` gate. That validator checked every rebuilt face and edge, so one unrelated or harmless tiny edge could reject the candidate even after inset cuts, face clipping, rail extraction, and bevel-face construction had already succeeded.

EW-4B.4 removes that whole-polyhedron validation as the per-candidate acceptance gate. Candidate acceptance now validates the candidate-local changed geometry instead:

- clipped base faces touched by bevel inset cuts;
- generated bevel strip faces;
- optional endpoint cap faces using relaxed cap thresholds.

Endpoint caps remain optional closure helpers. Degenerate or too-small caps are skipped instead of rejecting an otherwise valid bevel strip. After welding/sanitizing, the pass only requires that the rebuilt candidate set still contains at least one generated `ConvexEdgeWear` face per selected accepted candidate.

The rejection statistics are also split so future console warnings identify whether a candidate failed base-face, bevel-face, cap-face, or global validation instead of reporting one broad `Validation` bucket. EW-4B.4 does not modify the shader, FeatureAtlas0/1, MeshData, MeshBuilder, GeneratedGround, or the ground generator.
