# Generated Mass Edge Wear Recovery Architecture

Status: active recovery plan  
Current documentation patch: EW-3 Documentation — Generated Mass Feature Budget Policy  
Current code foundation: EW-Atlas-1 + EW-2A + EW-2B accepted after debug validation

---

## 1. Summary

Edge wear stalled because the system tried to make worn stone from an albedo/tint band around hard polygon edges. Better noise changed the band but did not solve the material/form problem.

The recovery direction is now:

```text
1. Keep the generic boundary atlas foundation.
2. Make boundary atlases optional and budgeted before more features are added.
3. Add real main-mesh worn bevel/chamfer support.
4. Add bevel normals and bevel-aware material response.
5. Add separate concave crack/crease response.
6. Add broad stone face variation.
```

The immediate next implementation is not another Micro/Macro tuning pass and not bevels yet. It is EW-3: generated-mass feature budget and optional atlas policy.

---

## 2. Current accepted atlas foundation

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

It is not proximity/falloff.

Coordinate/identity/modulation channels are dominant-boundary facts. They must not be MaxByte-merged like intensity masks. Only true proximity channels accumulate by maximum.

---

## 3. Macro / Micro control contract

```text
Macro Variation = inter-edge variation
  Which boundary chains are stronger or weaker than other boundary chains?
  Macro 0 ignores salience/identity as visible edge-strength variation.
  Macro 1 allows some boundaries to stay strong while others recede.
  Macro must not create same-edge noise or width wobble.

Micro Variation = intra-edge variation
  How does the same boundary vary along itself?
  Micro 0 ignores boundary-local coordinate/modulation.
  Micro 1 uses along coordinate and modulation to vary local strength/spread.
  Micro must not decide which edge is globally important.
```

Macro now works from generic boundary salience/identity. Micro has usable boundary-local data, but a painted albedo band is still not enough to match the reference rocks.

---

## 4. Why EW-3 budget work comes before bevels

Current code facts:

```text
GeneratedMassFeatureAtlasBaker.DefaultResolution = 512.
The current baker creates FeatureAtlas0 and FeatureAtlas1 together.
The current upload keeps CPU-readable atlas memory with Apply(false, false).
```

Current default atlas cost:

```text
512 Atlas0 + 512 Atlas1 = ~2 MB GPU pixel data per mass.
With CPU-readable copies retained, practical retained memory can approach ~4 MB per mass before overhead.
```

That cannot be the default in a scene with many generated masses. Bevel geometry cost is acceptable only after atlas memory stops scaling blindly.

Therefore the next work is:

```text
EW-3 — Feature-gated atlas generation and generated-mass budget policy.
```

---

## 5. EW-3 target policy

### 5.1 Atlas generation is demand-driven

```text
Do not generate an atlas because an object is a Generated Mass.
Generate an atlas only because an active feature or debug view requests that atlas.
```

Both atlases are independently optional:

```text
FeatureAtlas0: generated only when boundary structure is required.
FeatureAtlas1: generated only when boundary coordinates/modulation are required.
```

Current edge-wear dependency:

```text
Edge wear disabled:
  no FeatureAtlas0
  no FeatureAtlas1

Edge wear enabled, Micro Variation = 0:
  FeatureAtlas0 required
  FeatureAtlas1 not required

Edge wear enabled, Micro Variation > 0:
  FeatureAtlas0 required
  FeatureAtlas1 required
```

Debug modes may request the atlas they inspect during authoring.

### 5.2 Budgets are numeric ceilings

There is no `Background` tier. Generated masses are gameplay-reachable objects in an isometric action/roguelike game.

Use these budgets:

| Budget | Rendered vertex target | Atlas cap | Use |
|---|---:|---:|---|
| Compact | <= 800 | 128 | Small/simple gameplay masses |
| Standard | <= 1,600 | 256 | Default gameplay masses |
| Detailed | <= 3,000 | 256 | Important/larger visible masses |
| Hero | <= 8,000 | 512 | Showcase/large/inspected/debug masses |
| Custom / Debug | manual | manual | Testing/override only |

The budget is not a philosophical preset. It is a cap. Artist-requested settings are preserved when the estimated/generated cost fits.

### 5.3 Default shape targets

| Budget | Default effective shape | Estimated normal verts | Estimated harsh/fractured verts |
|---|---|---:|---:|
| Compact | Simple + Medium | ~504 | ~756 |
| Standard | Complex + Medium | ~864 | ~1,152 |
| Detailed | Complex + High | ~1,296 | ~1,728 |
| Hero | HighlyComplex + High | ~1,674 | ~2,160 |
| Hero Max / Debug | HighlyComplex + VeryHigh | ~2,232 | ~2,880 |

Avoid defaulting to poor-value pairs such as `HighlyComplex + Low`, `Complex + Low`, or `HighlyComplex + Sparse`.

Clamp order when over budget:

```text
1. Lower atlas resolution first.
2. Lower SurfaceFacetDensity next.
3. Lower FormComplexity last.
```

### 5.4 Atlas memory targets

| Setup | GPU atlas memory |
|---|---:|
| No atlas | 0 MB |
| 128 Atlas0 only | ~0.0625 MB |
| 128 Atlas0 + Atlas1 | ~0.125 MB |
| 256 Atlas0 only | ~0.25 MB |
| 256 Atlas0 + Atlas1 | ~0.5 MB |
| 512 Atlas0 only | ~1.0 MB |
| 512 Atlas0 + Atlas1 | ~2.0 MB |

Texture upload should release CPU-readable copies:

```csharp
atlas.Apply(false, true);
```

---

## 6. EW-3 implementation checklist

### EW-3 Documentation — this patch

- [x] Replace stale fixed two-atlas assumptions in the active docs.
- [x] Define feature-gated Atlas0 / Atlas1 policy.
- [x] Define Compact / Standard / Detailed / Hero budgets.
- [x] Remove `Background` tier language.
- [x] Document atlas memory numbers.
- [x] Move atlas budget work before bevel work.

### EW-3 Code A — optional atlas generation and atlas memory budget

- [ ] Add feature/debug requirement resolver for `FeatureAtlas0`.
- [ ] Add feature/debug requirement resolver for `FeatureAtlas1`.
- [ ] Skip Atlas0 bake when no active feature/debug mode needs it.
- [ ] Skip Atlas1 bake when no active feature/debug mode needs it.
- [ ] Resolve atlas resolution from active budget.
- [ ] Make 512 Hero/debug, not default.
- [ ] Use `Apply(false, true)` after atlas upload.
- [ ] Add inspector preview for atlas requirement, resolution, and estimated memory.

### EW-3 Code B — generated-mass numeric budget resolver

- [ ] Add Generated Mass Budget: Compact / Standard / Detailed / Hero / Custom.
- [ ] Preserve artist-requested shape settings when they fit the budget.
- [ ] Estimate or measure rendered vertex count for budget checks.
- [ ] Clamp SurfaceFacetDensity before FormComplexity when over budget.
- [ ] Show effective FormComplexity / SurfaceFacetDensity in the inspector.
- [ ] Warn about poor-value combinations such as HighlyComplex + Low.

---

## 7. Next visual feature sequence after EW-3

```text
EW-4 — Main-mesh worn bevel/chamfer foundation
  Create physical support on selected convex boundary chains.
  No secondary meshes.
  Same main mesh and material path.

EW-5 — Explicit bevel normal policy
  Supply or control normals for bevel faces.
  Stop relying on albedo bands for form.

EW-6 — Bevel-region material marking
  Mark bevel/worn regions through reusable mesh data.
  Prefer existing vertex/UV channels where safe.

EW-7 — Bevel-aware material response
  Use bevel marker + optional boundary atlases.
  Edge wear becomes material response on real support geometry, not only a painted line.

EW-8 — Concave cracks / crease response
  Separate dark crease/lip behavior from convex edge wear.

EW-9 — Broad stone face material variation
  Add large-scale mottling/exposure/deposit variation so edge wear is not pasted onto flat base material.
```

Do not start EW-4 until EW-3 code makes atlas memory scalable.

---

## 8. Rule for future feature libraries

Good reusable generated data:

```text
boundary proximity
boundary salience
boundary identity
boundary-local along coordinate
boundary-local side coordinate
boundary-local modulation
bevel/worn support marker
broad exposure/deposit/material variation
```

Bad framework data:

```text
edge-wear macro factor
edge-wear opacity atlas
edge-wear chip atlas
frost-only atlas
moss-only atlas
sacred-only atlas
one new atlas per feature
```

Features should interpret shared facts. They should not force the framework to become a pile of feature-specific texture layers.
