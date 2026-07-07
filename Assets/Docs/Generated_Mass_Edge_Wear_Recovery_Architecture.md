# Generated Mass Edge Wear Recovery Architecture

Status: active recovery plan  
Current patch: EW-3A.6 — Runtime Edge-Wear Atlas Decommission  
Next production patch: EW-4 — Main-Mesh Geometry Edge Wear

---

## 1. Decision

Convex generated-mass edge wear will not use FeatureAtlas0/1 for final normal rendering.

The production direction is:

```text
actual generated bevel/chamfer geometry
+ bevel-face material markers
+ bevel/custom normals
+ simple shader response on marked faces
```

FeatureAtlas0 and FeatureAtlas1 are retained only as temporary authoring/debug views until EW-4 is designed and validated.

---

## 2. Why the atlas-first approach failed

The atlas-first path tried to represent a line-like hard edge feature as a packed texture distance field.
That is the wrong representation.

Current width formula:

```csharp
float edgeWearWidth =
    scale * 0.018f * Mathf.Max(0.05f, settings.EdgeWearWidth);
```

The actual hard core was far smaller than the outer falloff:

```text
core  ≈ 0.014–0.030 × featureWidth
outer ≈ 0.68–1.02 × featureWidth
```

Approximate cube-like chart density:

```text
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

A hard ridge core that is sub-pixel at 128/256 cannot be recovered with supersampling, weighted averaging, dominant groups, or shader Micro tuning. The data needed for a stable hard edge is not present at the required resolution.

---

## 3. Superseded patch lessons

```text
EW-3A.1:
  Fixed resolution-dependent width inflation.
  Did not solve sub-pixel feature representation.

EW-3A.2:
  Added adaptive supersampling.
  Did not make 128/256 visually stable.

EW-3A.3:
  Reduced low-res Micro shape instability.
  Made symptoms less chaotic but did not solve representation.

EW-3A.4:
  Tried ridge-core preservation and Cross stabilization.
  Did not fix the underlying representation problem.

EW-3A.5:
  Tried dominant boundary-side groups.
  Did not fix the result because the atlas remained too low-density for the hard edge feature.
```

These patches should not be extended as the final edge-wear path.

---

## 4. Current EW-3A.6 code intent

EW-3A.6 does this:

```text
GeneratedMass.cs:
  normal-render edge wear no longer requests FeatureAtlas0/1
  debug views may still request FeatureAtlas0/1

GeneratedMassEditor.cs:
  budget preview describes temporary debug atlas use only
  edge-wear controls are labeled as reserved inputs for EW-4 geometry

SH_PixelSurfaceLit.shader:
  normal rendering no longer samples FeatureAtlas0/1 for edge wear
  atlas sampling remains available for debug modes

GeneratedMassFeatureAtlasBaker.cs:
  retained as temporary/debug boundary-field baker
```

No new public control is added.
No new debug mode is added.

---

## 5. EW-4 production architecture

EW-4 should implement edge wear before final mesh upload, not as runtime texture sampling.

Target generation flow:

```text
Generated compact mass base polyhedron
→ identify eligible convex edges
→ create bevel/chamfer faces on main mesh
→ mark bevel faces as convex edge-wear material region
→ emit mesh with bevel normals/custom normals
→ shader shades marked bevel faces
```

The preferred mesh marker is:

```text
UV2.z = convex edge-wear / bevel-face strength
```

Existing material-data channels already include UV2, so EW-4 should avoid adding a new channel unless proven necessary.

---

## 6. Expected visual improvement

Atlas edge wear could only paint over existing hard faces. Geometry edge wear changes the form:

```text
bevel faces catch light
worn edges have actual width in mesh space
normals can make the worn strip read from the isometric camera
material response is applied only where geometry says the edge exists
```

This directly targets the desired stylized rock reference: worn/chipped/softened edges, not white mask bands painted over polygon boundaries.

---

## 7. Performance comparison

Atlas path:

```text
128 Atlas0+Atlas1 = 128 KiB per mass
256 Atlas0+Atlas1 = 512 KiB per mass
512 Atlas0+Atlas1 = 2,048 KiB per mass
+ one or two runtime texture samples per shaded pixel
```

Simple bevel path:

```text
1 bevel edge ≈ 1 quad = 2 triangles = 6 rendered vertices
estimated vertex cost ≈ 80 bytes / vertex
per bevel edge ≈ 492 bytes including indices
```

Examples:

```text
24 bevel edges ≈ 11.5 KiB
48 bevel edges ≈ 23 KiB
80 bevel edges ≈ 38 KiB
```

Even with richer bevel segmentation, generated geometry is usually far cheaper than unique 512 atlases and produces better visual form.

---

## 8. Atlas future-use policy

FeatureAtlas0/1 should not be preserved as a default production dependency.

Potential future valid uses are limited to cases where an atlas is genuinely better than geometry, vertex data, or procedural shader data:

```text
large broad baked surface masks
unique high-frequency surface painting
debug visualization of boundary facts
```

Do not use FeatureAtlas0/1 for:

```text
convex edge wear
edge-local hard highlights
thin cracks/creases
chipped edge lines
```

Those are line/edge features and should use geometry, strips, grooves, or mesh-carried data.

---

## 9. Next work

```text
EW-4 — Main-Mesh Geometry Edge Wear
1. Inspect MassGenerator polygon/cut/triangulation flow.
2. Identify convex edge candidates before triangulation.
3. Generate controlled bevel/chamfer faces.
4. Mark bevel faces through UV2.z.
5. Emit correct normals or an explicit bevel-normal policy.
6. Update shader to shade UV2.z marked bevel faces as worn material.
7. Keep FeatureAtlas0/1 out of normal rendering.
```
