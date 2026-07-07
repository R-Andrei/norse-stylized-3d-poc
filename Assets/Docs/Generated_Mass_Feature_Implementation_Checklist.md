# Generated Mass Feature Implementation Checklist

Status: active implementation tracker  
Companion document: `Generated_Mass_Framework.md`  
Current documentation patch: EW-3 Documentation — Generated Mass Feature Budget Policy

---

## 1. Active policy summary

Generated Mass feature data is demand-driven and budgeted.

```text
Do not generate FeatureAtlas0 unless an active feature/debug mode requires boundary structure.
Do not generate FeatureAtlas1 unless an active feature/debug mode requires boundary-local coordinates/modulation.
Do not default generated masses to two 512x512 atlases.
Do not keep CPU-readable atlas texture memory after upload unless a feature explicitly requires readback.
```

Current accepted atlas contracts:

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

`FeatureAtlas1.G` is side-aware: `0.5` at boundary core, below/above `0.5` for the two adjacent patch sides.

---

## 2. Completed foundation checkpoints

### EW-Atlas-1 — Generic boundary atlas contract

- [x] Replaced edge-wear-specific Atlas1 contract with generic boundary coordinate/modulation data.
- [x] Replaced Atlas0 proximity/weight pairing with boundary proximity, salience, and identity facts.
- [x] Macro Variation restored as inter-edge variation using salience/identity.
- [x] Micro Variation uses boundary-local coordinate/modulation, not edge-wear-specific atlas channels.
- [x] No FeatureAtlas2 added.
- [x] No new runtime texture sample added beyond the existing optional atlas paths.

### EW-2A — Cross coordinate and fine modulation diagnostics

- [x] Added missing Boundary Fine Modulation debug view for `FeatureAtlas1.A`.
- [x] Made `FeatureAtlas1.G` a side-aware boundary cross coordinate instead of duplicated proximity.
- [x] Stopped normal edge-wear render from treating `FeatureAtlas1.G` as proximity/falloff.

### EW-2B — Coordinate atlas write/debug correction

- [x] Gated Boundary Cross Coordinate debug by boundary presence.
- [x] Prevented empty face interiors from displaying as valid side data.
- [x] Wrote coordinate/identity/modulation channels from the dominant boundary sample instead of MaxByte-merging them.
- [x] Kept maximum accumulation only for true proximity channels.

---

## 3. EW-3 Documentation — generated-mass feature budget policy

- [x] Move atlas budgeting before bevel/chamfer work.
- [x] Remove `Background` tier language; this game uses reachable gameplay masses.
- [x] Define active budgets: Compact / Standard / Detailed / Hero / Custom.
- [x] Define numeric rendered-vertex targets.
- [x] Define atlas resolution caps.
- [x] Define atlas memory table.
- [x] Define feature-gated Atlas0 / Atlas1 policy.
- [x] Define inspector policy: safe budget-first authoring with advanced overrides.
- [x] Replace stale fixed two-atlas / 512-default language in the active framework docs.

---

## 4. EW-3 Code A — optional atlas generation and memory budget

### Goal

Make atlas memory proportional to feature use and budget.

### Checklist

- [ ] Add feature/debug data-requirement resolver:
  - [ ] `requiresFeatureAtlas0`
  - [ ] `requiresFeatureAtlas1`
- [ ] Skip `FeatureAtlas0` creation when not required.
- [ ] Skip `FeatureAtlas1` creation when not required.
- [ ] Preserve existing shader enable flags:
  - [ ] `_GeneratedMassFeatureAtlas0Enabled`
  - [ ] `_GeneratedMassFeatureAtlas1Enabled`
- [ ] Resolve atlas resolution from budget:
  - [ ] Compact -> 128 cap
  - [ ] Standard -> 256 cap
  - [ ] Detailed -> 256 cap
  - [ ] Hero -> 512 cap
  - [ ] Custom / Debug -> manual
- [ ] Make 512 hero/debug, not default.
- [ ] Use `atlas.Apply(false, true)` after atlas upload.
- [ ] Add inspector preview:
  - [ ] Atlas0 required? yes/no
  - [ ] Atlas0 resolution
  - [ ] Atlas0 estimated memory
  - [ ] Atlas1 required? yes/no
  - [ ] Atlas1 resolution
  - [ ] Atlas1 estimated memory
  - [ ] total estimated atlas memory

### Acceptance

```text
Edge wear disabled:
  no atlas textures are generated unless a debug view explicitly requests them.

Edge wear enabled, Micro Variation = 0:
  Atlas0 generated.
  Atlas1 not generated.

Edge wear enabled, Micro Variation > 0:
  Atlas0 and Atlas1 generated.

No path keeps CPU-readable atlas memory after upload unless explicitly required.
```

---

## 5. EW-3 Code B — numeric mesh budget resolver

### Goal

Make mesh complexity deterministic and budgeted without arbitrarily clamping good settings.

### Budget targets

| Budget | Rendered vertex target | Atlas cap |
|---|---:|---:|
| Compact | <= 800 | 128 |
| Standard | <= 1,600 | 256 |
| Detailed | <= 3,000 | 256 |
| Hero | <= 8,000 | 512 |
| Custom / Debug | manual | manual |

### Safe default shape targets

| Budget | Default effective shape | Estimated normal verts | Estimated harsh/fractured verts |
|---|---|---:|---:|
| Compact | Simple + Medium | ~504 | ~756 |
| Standard | Complex + Medium | ~864 | ~1,152 |
| Detailed | Complex + High | ~1,296 | ~1,728 |
| Hero | HighlyComplex + High | ~1,674 | ~2,160 |
| Hero Max / Debug | HighlyComplex + VeryHigh | ~2,232 | ~2,880 |

### Checklist

- [ ] Add `GeneratedMassBudget` enum:
  - [ ] Compact
  - [ ] Standard
  - [ ] Detailed
  - [ ] Hero
  - [ ] Custom / Debug
- [ ] Preserve artist-requested `FormComplexity` when estimated/generated cost fits budget.
- [ ] Preserve artist-requested `SurfaceFacetDensity` when estimated/generated cost fits budget.
- [ ] Clamp only when over budget.
- [ ] Clamp order:
  - [ ] lower atlas resolution first
  - [ ] lower SurfaceFacetDensity next
  - [ ] lower FormComplexity last
- [ ] Add inspector readout:
  - [ ] requested FormComplexity
  - [ ] effective FormComplexity
  - [ ] requested SurfaceFacetDensity
  - [ ] effective SurfaceFacetDensity
  - [ ] estimated/generated rendered vertex count
- [ ] Warn about poor-value combinations:
  - [ ] HighlyComplex + Low
  - [ ] Complex + Low
  - [ ] HighlyComplex + Sparse

### Acceptance

```text
Standard budget does not blindly force Moderate + Medium.
If Complex + High fits the Standard numeric target, it is preserved.
If it exceeds the target, density is reduced before major form complexity.
```

---

## 6. EW-4+ visual feature sequence

Do not start these until EW-3 code makes atlas memory scalable.

### EW-4 — Main-mesh worn bevel/chamfer foundation

- [ ] Select important convex boundary chains using generic boundary data.
- [ ] Generate worn bevel/chamfer support on the main mesh.
- [ ] Do not use secondary feature meshes.
- [ ] Keep bevel geometry capped by the active generated-mass budget.

### EW-5 — Explicit bevel normal policy

- [ ] Define normals for original faces.
- [ ] Define normals for bevel faces.
- [ ] Avoid accidental melted shading at bevel junctions.

### EW-6 — Bevel-region material marking

- [ ] Mark actual bevel/worn regions through reusable mesh data.
- [ ] Prefer existing vertex/UV channels where safe.
- [ ] Do not reintroduce triangle-wedge scalar masks on large faces.

### EW-7 — Bevel-aware material response

- [ ] Use bevel marker as primary worn support.
- [ ] Use FeatureAtlas0/1 only when requested by the active response.
- [ ] Keep Macro = inter-edge variation.
- [ ] Keep Micro = intra-edge variation.

### EW-8 — Concave cracks / crease response

- [ ] Treat concave cracks separately from convex edge wear.
- [ ] Use concave proximity and boundary-local modulation.
- [ ] Render dark crease center and possible light lip.

### EW-9 — Broad stone face material variation

- [ ] Add large-scale stone mottling/exposure/deposit variation.
- [ ] Prefer existing mesh/vertex data and cheap dirty-time masks before adding runtime procedural cost.

---

## 7. Reusable feature-library rules

Good data:

```text
boundary proximity
boundary salience
boundary identity
boundary along coordinate
boundary side coordinate
boundary-local modulation
bevel/worn support marker
broad exposure/deposit/material masks
```

Bad data:

```text
edge-wear-only opacity atlas
edge-wear chip atlas
frost-only atlas
moss-only atlas
sacred-only atlas
one atlas per feature
```

Feature modules should declare data requirements, then interpret shared facts. They should not silently force new persistent data layers.
