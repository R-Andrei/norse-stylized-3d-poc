# Generated Mass Edge Wear Recovery Architecture

Status: active recovery plan  
Current patch: EW-4B.3 — Geometry Edge-Wear Diagnostics and Rejection Evidence  
Next production patch: determined by EW-4B.3 rejection evidence

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

FeatureAtlas0 and FeatureAtlas1 are retained only as temporary authoring/debug views. EW-4A is now the active production edge-wear path for plane-cut masses.

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

## 4. Current EW-4A code intent

EW-4A does this:

```text
GeneratedMass.cs:
  creates MassSurfaceFeatureSettings before mesh generation
  passes those settings into MassGenerator.Generate(recipe, featureSettings)
  normal-render edge wear does not request FeatureAtlas0/1
  ConvexEdgeWear debug mode now shows the geometry UV2.z mask, not a temporary atlas

MassGenerator.cs:
  applies bevel/chamfer cuts after all plane-cut shape cuts and before triangulation
  selects eligible convex edges from polygon-face topology
  suppresses base/lower edges and very short/flat edges
  marks bevel cap faces as ConvexEdgeWear
  preserves feature metadata through later clipping
  triangulates bevel faces minimally instead of applying broad surface relief/facet density
  writes ConvexEdgeWear strength to UV2.z and vertex color A

SH_PixelSurfaceLit.shader:
  normal rendering shades UV2.z-marked bevel/chamfer faces with the existing edge-wear material controls
  normal rendering does not sample FeatureAtlas0/1 for edge wear
  atlas sampling remains available for boundary debug modes

GeneratedMassFeatureAtlasBaker.cs:
  retained as temporary/debug boundary-field baker only
```

No new public control is added.
No new debug mode is added.

---

## 5. EW-4A production architecture

EW-4A implements edge wear before final mesh upload, not as runtime texture sampling.

Implemented generation flow:

```text
Generated plane-cut compact mass base polyhedron
→ all normal profile / major / chip cuts complete
→ identify eligible convex edges
→ apply bevel/chamfer cuts to selected edges
→ mark bevel cap faces as convex edge-wear material regions
→ triangulate bevel faces minimally
→ emit mesh with UV2.z bevel-face marker
→ shader shades marked bevel faces
```

The mesh marker is:

```text
UV2.z = convex edge-wear / bevel-face strength
Vertex Color A = same marker for inspection/backward compatibility
```

Existing material-data channels already include UV2, so EW-4A adds no new vertex channel.

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
EW-4A validation — confirm bevel faces are visible and stable on plane-cut archetypes
EW-4B — refine bevel normals/depth/selection if first-pass output needs tuning
1. Inspect MassGenerator polygon/cut/triangulation flow.
2. Identify convex edge candidates before triangulation.
3. Generate controlled bevel/chamfer faces.
4. Mark bevel faces through UV2.z.
5. Emit correct normals or an explicit bevel-normal policy.
6. Update shader to shade UV2.z marked bevel faces as worn material.
7. Keep FeatureAtlas0/1 out of normal rendering.
```


## EW-4A.1 update — control semantics and conservative bevel stability

EW-4A.1 corrects the initial bevel pass without changing the agreed production direction. The previous EW-4A implementation let Softness scale physical bevel depth and allowed very wide one-plane cuts. That made max settings produce broad shaved surfaces, not worn edge detail, and increased the chance of slivers/gap-like artifacts from sequential global clipping.

The revised contract is:

```text
Amount   -> generated worn-face material strength; zero disables edge wear
Width    -> physical single-plane bevel/chamfer depth
Coverage -> fraction of eligible structural convex edges selected
Softness -> visual material softness only for now; future normal/blend control
Macro    -> reserved for per-edge variation
Micro    -> reserved for along-edge chipping/segmentation
```

Single-plane bevel width is now deliberately capped lower. Max Coverage selects all eligible structural candidates, but each global clip is validated and can be rejected if it creates unstable tiny faces or edges. This is a short-term stabilization pass, not the final bevel topology solution. A later local edge-strip bevel implementation remains required to avoid the limitations of global clipping and to support richer/chipped/multi-strip worn edges.

## EW-4B update — local edge-strip bevels replace global bevel cuts

EW-4B replaces the EW-4A/EW-4A.1 global sequential clipping bevel pass with a local edge-strip construction. The previous pass selected convex edges but then clipped the whole polyhedron with each bevel plane. This was useful as a proof that geometry-first edge wear was the correct representation, but it could still create slivers, gap-like strips, and unrelated cuts.

The new bevel foundation keeps edge-wear work local: selected candidates generate inset cuts only on their two adjacent base faces, the resulting rails are connected by a ConvexEdgeWear bevel face, and endpoint caps close the bevel at edge ends. UV2.z remains the bevel-face material mask used by the shader. No atlas is involved in normal rendering.

EW-4B is still a foundation pass, not the final chipped/rounded edge system. Macro Variation, Micro Variation, multi-strip rounded bevels, and custom softened normals remain later bevel milestones.

## EW-4B.1 update — robust local candidate acceptance

EW-4B validation identified the next concrete topology failure: local bevel construction was still all-or-nothing. A single selected candidate with fragile rail extraction or invalid endpoint cap topology could make the entire optional bevel pass return false, leaving the mesh with no `ConvexEdgeWear` UV2.z faces. EW-4B.1 fixes that failure mode by accepting local bevel candidates cumulatively and skipping individual invalid candidates.

## EW-4B.2 update — final material response follows UV2.z

EW-4B.1 validation confirmed that `Surface Mask Debug = ConvexEdgeWear` could show the generated local bevel mask while the normal final render remained unchanged. That isolated the failure to the final material response path, not bevel topology or UV2.z generation.

EW-4B.2 adds a dedicated `_GeneratedMassGeometryEdgeWearEnabled` material property set by `GeneratedMass`, then uses that property plus UV2.z and Response Strength for normal edge-wear shading. The shader no longer relies on `_SurfaceContract` to decide whether generated-mass geometry edge wear is allowed. Brightness Lift now uses a bounded additive lift so response changes remain visible on dark stone material.

This does not reintroduce FeatureAtlas0/1 to normal rendering.

The patch also replaces the most fragile pieces of the first local assembly:
- rails are extracted from actual clipped polygon edges aligned with the source edge when possible;
- fallback rail extraction is bounded to the source-edge interval;
- endpoint caps are emitted as small per-edge triangular caps rather than one merged cap polygon around a vertex.

The goal is topology robustness first. It is still not the final rounded/chipped bevel system.

## EW-4B.3 update — geometry evidence before more bevel changes

EW-4B.3 corrects the debugging workflow rather than adding a new visual mode. The existing `ConvexEdgeWear` surface-mask mode is the geometry-mask view and should be used to validate UV2.z bevel/wear faces. `Convex Boundary Proximity` remains an atlas diagnostic and must not be used as proof of geometry bevel generation.

The generator now tracks why local bevel candidates are rejected:

```text
InsetCut
FaceClip
RailExtraction
BevelFace
Validation
Unknown
```

If edge wear is enabled and all selected candidates fail, the editor receives a warning with candidate, selected, accepted, and rejection counts. The next bevel-topology fix must be based on those counts instead of screenshot inference.
