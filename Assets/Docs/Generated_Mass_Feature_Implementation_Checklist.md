# Generated Mass Feature Implementation Checklist

Status: active implementation checklist  
Current patch: EW-3A.6 — Runtime Edge-Wear Atlas Decommission  
Next patch: EW-4 — Main-Mesh Geometry Edge Wear

---

## 1. Current rule

Do not implement final convex edge wear with FeatureAtlas0/1.

```text
FeatureAtlas0/1 are temporary debug tools only.
Normal-render edge wear must be geometry/mesh-data based.
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

Status: active cleanup patch.

Required behavior:

```text
GeneratedMass.cs:
  normal edge wear does not request FeatureAtlas0/1
  only Surface Mask Debug views request temporary atlases

GeneratedMassEditor.cs:
  atlas preview describes temporary debug atlas use
  edge-wear controls are reserved for upcoming geometry implementation

SH_PixelSurfaceLit.shader:
  normal rendering does not sample FeatureAtlas0/1 for convex edge wear
  existing atlas debug modes continue to work

GeneratedMassFeatureAtlasBaker.cs:
  retained only as temporary/debug boundary-field baker
```

---

## 3. EW-4 implementation checklist

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

Implement convex edge wear before triangulation.

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

Start simple. A stable bevel that reads correctly is more important than many ornamental controls.

### 3.4 Mesh marker policy

Preferred marker:

```text
UV2.z = convex edge-wear / bevel-face strength
```

Current UV2 contract already reserves Z for convex edge localization data.

Avoid UV3 for edge wear. UV3 was needed only by the atlas path and should not be required for final edge wear.

### 3.5 Normal policy

EW-4 must explicitly decide how bevel normals are emitted.

Minimum acceptable version:

```text
bevel faces use their own bevel face normals
adjacent major faces remain readable and faceted
```

Possible later upgrade:

```text
controlled custom normals for softer worn highlights
```

Do not rely on accidental RecalculateNormals behavior without checking the result.

### 3.6 Shader target

Shader should apply worn-edge material response from mesh data:

```hlsl
float edgeWearMask = saturate(input.materialMasks.z);
```

Then use existing reserved controls for:

```text
response strength
brightness lift
tint strength
macro/micro variation if supported by mesh/edge data
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

## 5. Validation checklist after EW-3A.6

After applying EW-3A.6:

```text
Surface Mask Debug = None:
  no FeatureAtlas0/1 should be generated for edge wear
  normal render should not show atlas-painted edge wear

Surface Mask Debug = ConvexEdgeWear or boundary atlas modes:
  temporary atlas should generate for inspection

Budget preview:
  should report no temporary debug atlas unless the selected debug mode requires one
```

---

## 6. Validation checklist after EW-4

After geometry edge wear is implemented:

```text
Edge wear visible in normal render without FeatureAtlas0/1.
Bevel/chamfer faces exist on the main mesh.
UV2.z or chosen marker is nonzero only on intended worn faces.
Normals make worn edges readable from the isometric camera.
Compact/Standard/Hero budgets produce stable results without atlas resolution artifacts.
No secondary renderer is required.
No atlas is generated unless a debug mode requests it.
```

---

## 7. Open questions for EW-4 design

Resolve these by inspecting code before implementation:

```text
Should bevels be created by additional clipping planes or by explicit edge-strip construction?
How many bevel segments are needed for the first useful result?
Should edge selection use every eligible convex edge first, or a budgeted subset?
Which current controls map directly to geometry and which should be temporarily ignored?
How should Macro/Micro variation work without atlas coordinates?
```

Do not answer these from memory. Inspect `MassGenerator.cs` first.
