# Generated Mass Framework

Status: active framework definition  
Current documentation patch: EW-3 Documentation — Generated Mass Feature Budget Policy  
Supersedes as active planning source: `Rock_Generated_Mass_Upgrade_Plan.md` and older Patch 14C/14D historical atlas notes.

---

## 1. Purpose

The Generated Mass system is the reusable compact-mass framework for procedural rocks, boulders, ice chunks, ore chunks, ruin fragments, sacred monoliths, bone/fossil chunks, and similar compact generated objects.

It is not a rock-only shader test. It owns:

```text
base compact-mass shape generation
surface feature data
feature-budget policy
feature-atlas generation when required
main-mesh feature support such as future bevels/chamfers
shader/material interpretation
feature-oriented inspector controls
debug views for validating generated data
```

Core rule:

```text
Generated Mass produces reusable structural and semantic facts.
Individual features interpret those facts.
Do not bake one-off visual answers into framework data.
```

---

## 2. Current implementation facts

The current code state matters because the next work is a budget correction, not a new visual feature.

```text
GeneratedMass.cs
- FormComplexity and SurfaceFacetDensity are separate artist-facing controls.
- FeatureAtlasResolution currently resolves to GeneratedMassFeatureAtlasBaker.DefaultResolution.
- GeneratedMass currently calls GeneratedMassFeatureAtlasBaker.Bake(...) during regeneration.
- Atlas enable flags already exist in the material path:
  _GeneratedMassFeatureAtlas0Enabled
  _GeneratedMassFeatureAtlas1Enabled

GeneratedMassFeatureAtlasBaker.cs
- DefaultResolution = 512.
- MinimumResolution = 128.
- MaximumResolution = 512.
- The current baker creates FeatureAtlas0 and FeatureAtlas1 together.
- The current texture upload keeps CPU-readable texture memory with Apply(false, false).

MassGenerator.cs
- FormComplexity controls major cut count / dominant plane count.
- SurfaceFacetDensity controls surface triangulation density across major planes.
- Sparse/Low are effectively 1 segment.
- Medium is 2.
- High is 3.
- VeryHigh is 4.
- The rendered mesh emits one rendered vertex per triangle corner.
```

Consequence:

```text
The current fixed two-atlas 512x512 policy is too expensive as a default.
The next implementation work must make feature data demand-driven and budgeted before more visual layers are added.
```

---

## 3. Project constraints

Generated Mass must follow the project-wide performance stance:

```text
Prefer dirty-time generation over runtime work when possible.
Prefer one main mesh and one main material path.
Avoid secondary renderers for final generated surface features.
Avoid one texture/atlas per feature.
Keep runtime texture samples bounded.
Make expensive generated data optional by feature use.
Make quality cost deterministic and inspectable.
```

Target platform remains desktop PC first, with low-to-medium hardware compatibility preferred. Mobile is not a target, but unbounded per-object texture memory is still unacceptable.

---

## 4. Generated Mass feature budget policy

### 4.1 Budget is a numeric ceiling, not a style preset

The inspector may present named budgets, but the implementation must treat them as numeric cost ceilings.

Do not implement this:

```text
Standard = always Moderate + Medium
Detailed = always Complex + High
```

Implement this:

```text
artist-requested shape settings
+ active feature requirements
+ budget target
→ resolved effective settings
```

A requested setting is preserved if the estimated/generated cost fits the active budget. It is clamped only when the math says it exceeds the budget.

### 4.2 Active budget tiers

There is no `Background` tier for this game. The project uses an isometric action/roguelike camera, and generated masses are generally reachable gameplay objects, not distant scenery-only props.

Use these budgets:

| Budget | Rendered vertex target | Atlas cap | Intended use |
|---|---:|---:|---|
| Compact | <= 800 rendered verts | 128 | Small/simple gameplay masses |
| Standard | <= 1,600 rendered verts | 256 | Default gameplay rocks and masses |
| Detailed | <= 3,000 rendered verts | 256 | Important, larger, or more visually exposed masses |
| Hero | <= 8,000 rendered verts | 512 | Showcase, inspected, very large, or debug/hero masses |
| Custom / Debug | manual | manual | Testing and deliberate override only |

The budget also becomes the future cap for bevel/chamfer geometry and other generated support data.

### 4.3 Number-backed default shape targets

Approximate current mesh estimates, based on the current plane-cut/facet-density model and one rendered vertex per triangle corner:

| Budget | Default effective shape | Estimated normal verts | Estimated harsh/fractured verts | Budget fit |
|---|---|---:|---:|---|
| Compact | Simple + Medium | ~504 | ~756 | Fits <= 800 |
| Standard | Complex + Medium | ~864 | ~1,152 | Fits <= 1,600 |
| Detailed | Complex + High | ~1,296 | ~1,728 | Fits <= 3,000 |
| Hero | HighlyComplex + High | ~1,674 | ~2,160 | Fits <= 8,000 with room for bevels |
| Hero Max / Debug | HighlyComplex + VeryHigh | ~2,232 | ~2,880 | Still acceptable before bevels |

These are not forced combinations. They are safe defaults and warnings targets.

Bad default pairings:

```text
HighlyComplex + Low
Complex + Low
HighlyComplex + Sparse
```

Reason: they spend major-shape complexity without enough supporting surface density. The inspector should steer users toward better value combinations such as `Complex + Medium` or `Detailed + High`, depending on budget.

### 4.4 Clamp order

When generated or estimated cost exceeds the active budget, clamp in this order:

```text
1. Lower atlas resolution first.
2. Lower SurfaceFacetDensity next.
3. Lower FormComplexity last.
```

Reason:

```text
Atlas resolution is currently the largest scalable cost.
SurfaceFacetDensity is a multiplicative mesh-density cost.
FormComplexity changes the major silhouette/planes and should be protected longer.
```

---

## 5. Feature-gated atlas policy

Atlases are not generated because an object is a Generated Mass. Atlases are generated only because an active feature or debug view requests the data.

### 5.1 Atlas independence

```text
FeatureAtlas0 and FeatureAtlas1 are independently optional.
```

`FeatureAtlas0` is required when a feature needs boundary structure facts.  
`FeatureAtlas1` is required when a feature needs boundary-local coordinate/modulation facts.

If no active feature or debug mode requests an atlas, that atlas must not be generated.

### 5.2 Active boundary atlas contracts

```text
FeatureAtlas0 — Boundary Structure Atlas
R = convex boundary proximity
G = concave boundary proximity
B = dominant boundary structural salience
A = dominant boundary stable identity / seed

FeatureAtlas1 — Boundary Coordinate / Modulation Atlas
R = dominant boundary along-chain coordinate / phase
G = dominant boundary cross-boundary coordinate
B = dominant boundary coarse local modulation
A = dominant boundary fine local modulation
```

`FeatureAtlas1.G` is side-aware:

```text
0.5 = boundary core
< 0.5 = one adjacent patch side
> 0.5 = the other adjacent patch side
```

It is not a duplicate proximity/falloff channel.

Coordinate/identity/modulation channels are dominant-boundary facts, not intensity masks. They should be written from the dominant boundary sample for the texel. Only true proximity channels should accumulate by maximum.

### 5.3 Current edge-wear atlas requirements

```text
Edge wear disabled:
  FeatureAtlas0 not required.
  FeatureAtlas1 not required.

Edge wear enabled, Micro Variation = 0:
  FeatureAtlas0 required.
  FeatureAtlas1 not required.

Edge wear enabled, Micro Variation > 0:
  FeatureAtlas0 required.
  FeatureAtlas1 required.
```

Debug modes may request atlases in editor builds/authoring mode, but runtime should not generate unused atlases.

Future bevel selection should not automatically require runtime atlases. A bevel pass can use dirty-time graph data and bake the result into geometry/mesh markers. Atlases are needed only when the final material response samples boundary data at runtime.

---

## 6. Atlas memory policy

### 6.1 Memory table

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

Current fixed `512 Atlas0 + Atlas1` is a development/debug-level cost, not a production default.

### 6.2 Resolution rule

```text
Compact: 128 cap
Standard: 256 cap
Detailed: 256 cap
Hero: 512 cap
Custom / Debug: manual
```

512 should be Hero/debug only unless a later measured case proves that a non-Hero mass requires it.

### 6.3 CPU-readable memory

Generated feature atlas textures should discard CPU-readable copies after upload unless a future feature explicitly needs CPU readback.

Required implementation target:

```csharp
atlas.Apply(false, true);
```

The current code uses `Apply(false, false)`, which keeps CPU-readable texture memory. That is a known cleanup target for the budget patch.

---

## 7. Inspector policy

The normal inspector should not encourage bad complexity/density combinations.

Primary authoring path:

```text
Generation Budget:
  Compact / Standard / Detailed / Hero / Custom

Shape Detail:
  Auto Balanced
```

The inspector should show a read-only resolved preview:

```text
Effective Form Complexity
Effective Surface Facet Density
Estimated Rendered Vertices
FeatureAtlas0 required? resolution? estimated memory?
FeatureAtlas1 required? resolution? estimated memory?
Total estimated atlas memory
```

Advanced shape overrides may expose raw `FormComplexity` and `SurfaceFacetDensity`, but they should warn when the requested pair is poor value or over budget.

Example warning:

```text
HighlyComplex + Low is usually inefficient:
many major cuts, low supporting surface detail.
Suggested equivalent: Complex + Medium or Detailed budget.
```

Overrides are allowed, but the safe path must be the default.

---

## 8. Runtime and dirty-time cost model

### Mesh and bevel support

Mesh cost is usually cheaper than unbounded per-object atlas memory.

Approximate future bevel cost:

```text
extra mesh memory ~= extra vertices * 60-100 bytes + extra indices
```

This is acceptable when capped by the generated-mass budget. It is often preferable to adding another 512x512 atlas.

### Shader/material response

Runtime shader work should reuse already-requested data:

```text
existing vertex/mesh markers
optional FeatureAtlas0 sample
optional FeatureAtlas1 sample
small ALU response
```

Do not let each feature independently resample the same atlas data or require its own atlas.

### Dirty-time generation

Dirty-time work may be heavier when it avoids per-frame/runtime cost, but it must be bounded by the active budget.

---

## 9. Feature library direction

### Edge wear

Current status:

```text
generic boundary atlas foundation exists
Macro Variation works from salience/identity
Micro Variation has boundary-local coordinate/modulation support
current final render is still albedo/tint-based and cannot reach reference-rock quality alone
```

Next visual step after EW-3 budget policy:

```text
main-mesh worn bevel/chamfer support
explicit bevel normal policy
bevel-region material marking
bevel-aware material response
```

### Concave cracks / creases

Concave cracks are separate from convex edge wear. They should use concave proximity plus salience/identity/modulation, with a dark center and possible light lip. Do not force cracks into convex edge-wear controls.

### Broad stone face material variation

Large faces need broad mottling/exposure/deposit variation so edge wear does not look pasted onto flat material. Prefer existing mesh/vertex data and cheap baked/dirty-time masks before adding runtime procedural noise or new textures.

### Other future features

Frost, water polish, moss/deposit, sacred accents, pitting, and carved seams should consume shared boundary/mesh facts where practical. They should not create one private atlas per feature.

---

## 10. Immediate roadmap

### Completed / accepted foundation

```text
EW-Atlas-1 — Generic boundary atlas contract
EW-2A — Boundary cross-coordinate + fine-modulation debug view
EW-2B — Coordinate atlas write/debug correction
```

### Next documentation/code work

```text
EW-3 Documentation — Generated Mass Feature Budget Policy
  Status: this document update.
  Purpose: replace stale fixed two-atlas assumptions with demand-driven atlas generation and numeric budget rules.

EW-3 Code A — Optional atlas generation and atlas memory budget
  - Generate FeatureAtlas0 only when requested.
  - Generate FeatureAtlas1 only when requested.
  - Resolve atlas resolution from active budget.
  - Use Apply(false, true) after texture upload.
  - Add inspector preview for atlas requirement/resolution/memory.

EW-3 Code B — Numeric mesh budget resolver
  - Add generated-mass budget setting.
  - Resolve effective FormComplexity / SurfaceFacetDensity from numeric budget.
  - Preserve artist requested settings when they fit budget.
  - Clamp SurfaceFacetDensity before FormComplexity when over budget.
  - Add inspector warnings for poor-value combinations.
```

### Later visual feature work

```text
EW-4 — Main-mesh worn bevel/chamfer foundation.
EW-5 — Explicit bevel normal policy.
EW-6 — Bevel-region material marking.
EW-7 — Bevel-aware material response.
EW-8 — Concave cracks / crease response.
EW-9 — Broad stone face material variation.
```

No bevel/material/crack work should be started until EW-3 code has made atlas memory scalable.
