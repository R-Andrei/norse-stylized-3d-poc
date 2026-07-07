# Generated Mass Framework

Status: active framework definition  
Current implementation patch: EW-3A.6 — Runtime Edge-Wear Atlas Decommission  
Supersedes: older Patch 14C/14D and EW-3A.1 through EW-3A.5 atlas-first edge-wear plans.

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
- Normal-render convex edge wear no longer requests FeatureAtlas0 or FeatureAtlas1.
- Feature atlases are generated only for temporary Surface Mask Debug views.

GeneratedMassFeatureAtlasBaker.cs
- Retained as a temporary/debug boundary-field baker.
- Can generate FeatureAtlas0 only, FeatureAtlas0+FeatureAtlas1, or no atlas.
- Generated atlas upload uses Apply(false, true), discarding CPU-readable texture memory after upload.
- The atlas path is not the final representation for hard convex edge wear.

MassGenerator.cs
- FormComplexity controls major cut count / dominant plane count.
- SurfaceFacetDensity controls surface triangulation density across major planes.
- The rendered mesh emits one rendered vertex per triangle corner.
- UV2 is already part of the mesh material-data contract and is the preferred channel for future generated feature markers.

SH_PixelSurfaceLit.shader
- FeatureAtlas0/1 sampling remains available for debug modes.
- Normal rendering no longer samples FeatureAtlas0/1 for convex edge-wear material response.
```

Important conclusion:

```text
The atlas-first edge-wear path was a failed representation choice.
The next production edge-wear implementation is EW-4: main-mesh bevel/chamfer edge wear with bevel-face material markers and explicit normal policy.
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

## 5. Temporary atlas policy

Atlases are not generated because an object is a Generated Mass. Atlases are not generated for normal-render convex edge wear.

Atlases may be generated only because an active authoring/debug view requests them.

### 5.1 FeatureAtlas0 — temporary boundary structure debug atlas

```text
R = convex boundary proximity
G = concave boundary proximity
B = dominant boundary structural salience
A = dominant boundary stable identity / seed
```

### 5.2 FeatureAtlas1 — temporary boundary coordinate/modulation debug atlas

```text
R = dominant boundary along-chain coordinate / phase
G = dominant boundary cross-boundary coordinate
B = dominant boundary coarse local modulation
A = dominant boundary fine local modulation
```

These atlases are retained only as temporary diagnostic tools while EW-4 is designed and validated. They are not the foundation for final convex edge wear.

### 5.3 Current atlas request policy

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

Edge Wear Amount, Width, Coverage, Softness, Macro Variation, and Micro Variation are retained as reserved authoring inputs for the upcoming geometry-based implementation. They do not make FeatureAtlas0/1 visible in normal rendering.

### 5.4 Temporary atlas memory table

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

## 6. EW-4 target: geometry-first edge wear

EW-4 should implement convex edge wear as generated main-mesh geometry.

Minimum target:

```text
identify eligible convex edges during dirty-time generation
create narrow bevel/chamfer faces on the main mesh
mark bevel faces through UV2.z or equivalent mesh-carried data
apply clear bevel/custom normal policy
shade marked faces as worn material in SH_PixelSurfaceLit.shader
avoid FeatureAtlas0/1 in normal edge-wear rendering
```

Preferred material-data channel:

```text
UV2.z = convex edge-wear / bevel-face strength
```

Existing UV2 contract already reserves Z for future convex edge localization data. Use that before adding new channels.

---

## 7. Cost comparison: atlas path vs bevel path

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

## 8. Feature representation map

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

## 9. Patch history status

```text
EW-Atlas-1 through EW-3A.5:
  Useful for learning/debugging boundary facts.
  Superseded as final edge-wear architecture.
  Do not continue patching the atlas path for normal edge wear.

EW-3A.6:
  Decommissions runtime atlas-based edge wear.
  Keeps atlases as temporary debug tools only.
  Updates docs and inspector messaging to prevent stale atlas-first guidance.

EW-4:
  Next work item.
  Implement main-mesh bevel/chamfer edge wear and normal/material policy.
```
