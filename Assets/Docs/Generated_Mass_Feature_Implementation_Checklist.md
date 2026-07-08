# Generated Mass Feature Implementation Checklist

Status: active implementation checklist  
Current patch: EW-4B.5 — Shared Corner Closure  
Next patch: visual/topology tuning after joint closure validation

---

## 1. Current rule

Do not implement final convex edge wear with FeatureAtlas0/1.

```text
FeatureAtlas0/1 are temporary debug tools only.
Normal-render edge wear is geometry/mesh-data based for plane-cut mass archetypes.
```

---

## 2. Completed / superseded work

### EW-Atlas-1 through EW-3A.5

Status: superseded for final edge wear.

These patches produced a reusable temporary boundary-atlas baker and useful debug views, but they failed as the final edge-wear representation.

Reason:

```text
128/256 atlases cannot reliably represent the current hard ridge-core feature.
The final output remained stair-stepped, broad, or coordinate-corrupted despite sampling and resolver changes.
```

Do not continue tuning Cross Coordinate, Micro Variation, dominant groups, or ridge-core preservation as the normal-render edge-wear solution.

### EW-3A.6

Status: completed cleanup patch.

Required behavior:

```text
GeneratedMass.cs:
  normal edge wear does not request FeatureAtlas0/1
  only explicit boundary Surface Mask Debug views request temporary atlases

GeneratedMassEditor.cs:
  atlas preview describes temporary debug atlas use
  edge-wear controls no longer describe atlas runtime rendering

SH_PixelSurfaceLit.shader:
  normal rendering does not sample FeatureAtlas0/1 for convex edge wear
  existing atlas debug modes continue to work

GeneratedMassFeatureAtlasBaker.cs:
  retained only as temporary/debug boundary-field baker
```

### EW-4A

Status: active first geometry pass.

Required behavior:

```text
GeneratedMass.cs:
  passes MassSurfaceFeatureSettings into MassGenerator.Generate
  ConvexEdgeWear debug shows UV2.z and does not request FeatureAtlas0/1

MassGenerator.cs:
  plane-cut archetypes receive generated convex bevel/chamfer cuts before triangulation
  bevel cap faces are marked as ConvexEdgeWear
  bevel face marker is emitted through UV2.z and vertex color A
  bevel faces are triangulated minimally

SH_PixelSurfaceLit.shader:
  normal render shades UV2.z-marked bevel faces using edge-wear material controls
```

---

## 3. EW-4A implementation checklist

### 3.1 Code to inspect first

```text
Game/Procedural/Masses/MassGenerator.cs
Game/Procedural/Masses/GeneratedMass.cs
Game/Procedural/Core/MeshData.cs
Game/Procedural/Core/MeshBuilder.cs
Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader
Docs/Generated_Mass_Framework.md
Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md
```

### 3.2 MassGenerator target

EW-4A implements convex edge wear before triangulation for plane-cut archetypes.

```text
base generated polyhedron
→ convex edge detection
→ bevel/chamfer cut or explicit bevel face construction
→ triangulation
→ mesh emission with bevel-face marker
```

Do not create a secondary renderer.
Do not create a separate feature mesh for final edge wear.
Do not allocate FeatureAtlas0/1 for final edge wear.

### 3.3 Bevel candidate policy

The implementation should choose convex edges using generated geometry facts:

```text
edge convexity
edge length
face angle
edge salience / profile settings
artist controls: Amount, Width, Coverage, Macro/Micro as applicable
budget cap
```

EW-4A starts simple. It uses deterministic edge scoring, selected convex-edge bevel cuts, minimal bevel-face triangulation, and UV2.z material markers. Stable bevel readability is more important than ornamental segmentation.

### 3.4 Mesh marker policy

Preferred marker:

```text
UV2.z = convex edge-wear / bevel-face strength
```

Current UV2 contract already reserves Z for convex edge localization data.

Avoid UV3 for edge wear. UV3 was needed only by the atlas path and should not be required for final edge wear.

### 3.5 Normal policy

EW-4A uses the existing mesh normal path with triangle-soup bevel faces. Because GeneratedMass emits one rendered vertex per triangle corner, recalculated normals produce faceted bevel-face normals. This is acceptable for the first pass.

Possible later upgrade:

```text
controlled custom normals for softer worn highlights
multi-strip bevels or chip segmentation
```

Do not add custom normal plumbing until the first-pass bevel faces are validated visually.

### 3.6 Shader target

Shader applies worn-edge material response from mesh data:

```hlsl
float edgeWearMask = saturate(input.materialMasks.z);
```

It uses the existing edge-wear controls for:

```text
response strength
brightness lift
tint strength
macro/micro variation later if supported by mesh/edge data
```

No FeatureAtlas0/1 sample should be needed for normal edge wear.

---

## 4. Cost budget checklist

Planning estimate:

```text
1 simple bevel edge ≈ 1 quad
1 quad = 2 triangles
current rendered mesh = one rendered vertex per triangle corner
2 triangles = 6 rendered vertices
estimated vertex cost ≈ 80 bytes / vertex
per bevel edge ≈ 492 bytes including indices
```

Examples:

```text
24 selected bevel edges ≈ 11.5 KiB
48 selected bevel edges ≈ 23 KiB
80 selected bevel edges ≈ 38 KiB
```

Compare against atlas memory:

```text
128 Atlas0+Atlas1 = 128 KiB
256 Atlas0+Atlas1 = 512 KiB
512 Atlas0+Atlas1 = 2,048 KiB
```

Geometry edge wear is expected to be cheaper than the atlas path that visually worked, while also producing better form and removing runtime atlas texture samples.

---

## 5. Validation checklist after EW-4A

After geometry edge wear is implemented:

```text
Edge wear visible in normal render without FeatureAtlas0/1.
Bevel/chamfer faces exist on the main GeneratedMass mesh for plane-cut archetypes.
Surface Mask Debug = ConvexEdgeWear shows UV2.z bevel-face markers without requesting FeatureAtlas0/1.
Boundary atlas debug modes still generate temporary atlases when explicitly selected.
UV2.z is nonzero only on generated bevel/chamfer faces.
Normals make worn edges readable from the isometric camera.
Compact/Standard/Hero budgets produce stable results without atlas resolution artifacts.
Ground generation and non-mass meshes are unaffected.
No secondary renderer is required.
No atlas is generated unless a debug mode requests it.
```

---

## 6. Open questions for EW-4B

Resolve these only after validating EW-4A output in Unity:

```text
Are first-pass bevel depths large enough to read from the isometric camera?
Are selected edges visually correct, or does edge scoring need tuning?
Do generated bevel face normals read well enough, or is a custom-normal policy needed?
Do we need multi-strip/chipped bevel segmentation before concave crease work?
Should PolishedStone, LayeredStone, and CarvedMarkerStone receive a separate edge-wear path?
How should Macro/Micro Variation map onto geometry without reintroducing atlas dependency?
```


## EW-4A.1 checklist status

Completed in EW-4A.1:

```text
- Removed Softness from physical bevel-depth calculation.
- Reduced the allowed single-plane bevel depth range.
- Re-defined Amount as material/mask strength rather than geometry size.
- Re-defined Coverage as selection fraction; max now attempts all eligible structural edges.
- Added conservative cut validation to reject bevel cuts that produce unstable sliver faces/edges.
- Updated inspector copy so Macro/Micro remain explicitly reserved.
```

Still required before leaving bevel work:

```text
- Proper local edge-strip beveling, instead of relying only on global sequential clipping.
- Bevel normal policy/custom normals or multi-strip bevel transition.
- Macro Variation wired to per-edge strength/width/selection variation.
- Micro Variation wired to along-edge chipped/segmented bevel detail.
```

## EW-4B checklist status

Completed in EW-4B:

- Replaced the EW-4A global edge-wear clipping loop with local edge-strip bevel construction.
- Extended bevel candidates with source edge endpoints and adjacent face indices.
- Trims only the two adjacent base faces for each selected edge candidate.
- Extracts trimmed rails and creates ConvexEdgeWear bevel strip faces.
- Adds endpoint cap faces so local strips do not leave open ends.
- Preserves the EW-4A.1 control contract: Width = geometry depth, Amount = generated worn-face strength, Coverage = eligible edge selection fraction, Softness = material/normal response.
- Keeps FeatureAtlas0/1 debug-only.

Validation focus after EW-4B:

1. Check that the previous gap/sliver artifacts from global cuts are gone or substantially reduced.
2. Check normal render and Surface Mask Debug = ConvexEdgeWear on the same plane-cut mass.
3. Confirm max Width no longer creates whole-rock shaved cuts.
4. Confirm max Coverage still selects all eligible structural candidates, while acknowledging that eligibility filters can still exclude short/base/shallow edges.
5. Do not tune Macro/Micro until local bevel topology is stable.

## EW-4B.1 checklist status

Completed in EW-4B.1:
- Local bevel candidate acceptance is now cumulative instead of all-or-nothing.
- Invalid selected candidates are skipped instead of collapsing the whole bevel pass.
- Rail extraction now prefers an actual clipped edge aligned with the source edge.
- Near-plane point fallback is bounded to the source-edge interval.
- Endpoint closure now uses per-edge triangular cap faces instead of one merged non-planar cap polygon per vertex.
- FeatureAtlas0/1 remain debug-only and unused by normal edge wear.

Validation focus after EW-4B.1:
1. `Surface Mask Debug = ConvexEdgeWear` should no longer go fully dark at max settings unless no candidate can be validly bevelled.
2. Width values below the previous failure range should not create long gap-like slivers.
3. Coverage thresholds should skip bad candidates rather than causing the whole bevel pass to disappear.
4. Final render should show only accepted bevel strips/caps, not atlas artifacts.


## EW-4B.2 checklist status

Completed in EW-4B.2:

- Added `_GeneratedMassGeometryEdgeWearEnabled` as a dedicated material property for geometry edge-wear final response.
- `GeneratedMass` sets that enable flag through its MaterialPropertyBlock.
- Normal edge-wear shading now gates on UV2.z, the dedicated enable flag, Response Strength, and Softness.
- Brightness Lift now uses a bounded additive lift so bevel response is visible on dark stone.
- FeatureAtlas0/1 remain temporary debug-only tools and are not sampled by normal edge wear.

Validation focus after EW-4B.2:

- With `Surface Mask Debug = ConvexEdgeWear` showing bevel masks, switch to normal render and confirm bevel faces visibly respond.
- Confirm Response Strength scales visibility.
- Confirm Brightness Lift creates an obvious test response on dark stone.
- Re-check Amount after final response is visible; Amount should no longer read as merely boolean.

## EW-4B.3 checklist status

Completed in EW-4B.3:

- No new visual debug view was added.
- Existing `Surface Mask Debug = ConvexEdgeWear` is the required geometry-mask validation view for UV2.z bevel/wear faces.
- `Convex Boundary Proximity` is explicitly treated as atlas diagnostic data, not geometry bevel proof.
- `MassGenerator` now tracks local bevel build candidate/selected/accepted counts.
- `MassGenerator` now buckets failed local bevel candidate attempts by rejection reason.
- If selected candidates exist but no local bevels are accepted, the editor console reports the actual rejection counts.

Validation focus after EW-4B.3:

1. Use `Surface Mask Debug = ConvexEdgeWear`, not Convex Boundary Proximity.
2. If it is dark, check the editor console warning for rejection counts.
3. Fix the bevel generator based on the dominant rejection bucket.

## EW-4B.4 checklist status

Completed in EW-4B.4:

- Removed whole-rebuilt-polyhedron validation as the per-candidate acceptance gate for local edge-strip bevels.
- Added candidate-local face validation for clipped base faces and generated bevel strips.
- Added relaxed validation for optional endpoint cap faces; invalid caps are skipped rather than rejecting the candidate.
- Split the former broad `rejectedValidation` console bucket into `rejectedValidationBaseFace`, `rejectedValidationBevelFace`, `rejectedValidationCapFace`, and `rejectedValidationGlobal`.
- Kept FeatureAtlas0/1 debug-only and did not modify shader, MeshData, MeshBuilder, GeneratedGround, or ground generation.

Validation focus after EW-4B.4:

1. Use `Surface Mask Debug = ConvexEdgeWear`.
2. Confirm the warning is gone or `accepted > 0` appears in the console summary.
3. If still failing, use the split validation bucket to identify whether the remaining blocker is base-face, bevel-face, cap-face, or global validation.
4. Only tune visual controls after physical bevel faces exist.


## EW-4B.5 checklist status

Completed in EW-4B.5:

- Replaced automatic per-edge endpoint-cap creation with a shared corner-closure accumulation pass.
- Collected bevel rail endpoints by original source vertex.
- Generated one shared `ConvexEdgeWear` corner patch when multiple bevel strips meet at the same source vertex.
- Preserved triangular endpoint caps only as an isolated-end fallback.
- Added `cornerClosures` and `skippedCornerClosures` to the editor summary string.
- Added an editor warning when accepted bevels exist but one or more corner closures are skipped.
- Did not modify shader, FeatureAtlas0/1, MeshData, MeshBuilder, GeneratedGround, or ground generation.

Validation focus after EW-4B.5:

1. Use the same settings that showed joint triangles/gaps after EW-4B.4.
2. Confirm physical bevels still exist and controls still affect Amount, Width, and Coverage.
3. Inspect bevel joints in normal render and `Surface Mask Debug = ConvexEdgeWear`.
4. If gaps remain, check the console for `skippedCornerClosures`.
5. Continue visual tuning only after joint holes are gone or isolated to a specific skipped-corner count.
