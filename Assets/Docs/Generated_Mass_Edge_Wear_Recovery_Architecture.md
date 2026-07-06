# Generated Mass Edge Wear Recovery Architecture

Status: recovery document; EW-1 macro-control patch prepared  
Baseline inspected: `Assets(15).zip`, pre-rejected Patch 14D.4 baseline  
Feature area: `GeneratedMass` / compact procedural mass framework  
Immediate priority: install and validate EW-1, which restores `Macro Variation` as inter-edge variation before any new edge-wear feature work.

---

## 1. Summary

Edge wear is currently stuck because the visible effect is mostly a brightened material band around detected convex edges. That can expose the ridge field, but it cannot by itself match the inspiration rocks. The references combine selected edge importance, physical worn/chipped edge form, lighting/normal support, broad face material variation, and separate crack/seam treatment.

The first code step is **not** bevels, normals, cracks, or a new atlas. The first step is EW-1: fix the existing Macro contract that has either regressed or become too weak:

```text
Macro Variation = inter-edge variation.
At high Macro values, some eligible edges must be clearly stronger/weaker than other eligible edges.
Macro must not become a general amplifier of all edges.
Micro Variation = intra-edge variation.
At high Micro values, the same selected edge may vary in local width, intensity, thinning, and continuity.
Micro must not decide which edge is globally important.
```

After Macro is fixed, the realistic path is:

```text
EW-1  Macro control recovery / Atlas0.G hierarchy proof
EW-2  Atlas/channel cleanup and generic surface-data policy
EW-3  Ordered ridge-chain coordinates for true Micro variation
EW-4  Main-mesh worn bevel/chamfer foundation
EW-5  Bevel-aware material and lighting response
EW-6  Controlled chips / interruptions
EW-7  Broad stone face material variation
EW-8  Separate concave crack/seam feature
EW-9  Atlas quality tiers and runtime budget policy
```

The main design rule is unchanged: **do not solve this by adding one texture atlas per feature**. Spend complexity at dirty-time where possible. Keep runtime cost bounded: one main mesh, one main material path, no secondary visual edge strips, no per-frame topology work, no shader-side edge detection, and no new edge-wear atlas unless explicitly justified by a shared data budget.

---

## 2. Current code facts that matter

Line numbers refer to the uploaded `Assets(15).zip` baseline.

| Area | Evidence | Meaning |
|---|---:|---|
| Generated atlases | `GeneratedMass.cs:409-416`, `875-895`, `1350-1365` | The mass already binds `FeatureAtlas0` and `FeatureAtlas1` through `MaterialPropertyBlock`; this is the correct per-object binding path. |
| Atlas resolution/format | `GeneratedMassFeatureAtlasBaker.cs:16-18`, `197-210` | Default atlas size is 512 RGBA32, no mips. That is about 1 MB per atlas, so two atlases are already about 2 MB per mass. |
| Atlas0 contract | `GeneratedMassFeatureAtlasBaker.cs:758-763` | `R = convex proximity`, `G = convex weight/importance`, `B = concave proximity`, `A = concave weight/importance`. |
| Atlas1 contract | `GeneratedMassFeatureAtlasBaker.cs:776-783` | `R = edge amplitude variation`, `G = width/smear variation`, `B = continuity/chip-thinning`, `A = reserved`. This is currently edge-wear-specific, not generic enough long term. |
| Macro/Micro tooltips | `GeneratedMassEditor.cs:1051-1059` | Current inspector already defines Macro as between-ridge variation and Micro as same-ridge variation. Preserve this. |
| Current Macro shader path | `SH_PixelSurfaceLit.shader:917-942`, `976-982` | Shader samples Atlas0, reads `convexWeight`, computes `macroWeight`, then multiplies final `edgeMask`. If Atlas0.G is too uniform or mapping is too weak, Macro will appear broken. |
| Boundary score source | `MassSurfaceFeatureGraph.cs:666-711` | Boundary score is derived from length, angle, height/lower penalty, and averaged normal exposure. This is likely where inter-edge hierarchy starts. |
| Current chain data | `MassSurfaceFeatureGraph.cs:126-140`, `568-612` | Boundary chains are BFS groups with index/kind/list/length only. They are not ordered and do not store cumulative along-chain coordinates. |
| Current Atlas1 coordinate | `GeneratedMassFeatureAtlasBaker.cs:58-82`, `795-847` | Atlas1 irregularity uses nearest segment and segment-local `bestT`; this cannot provide robust same-ridge variation along a whole chain. |
| Current visual response | `SH_PixelSurfaceLit.shader:989-1007`, `1638-1639` | Edge wear is albedo/tint response only. No bevel geometry, no normal-map/virtual bevel normal support. |
| Mesh path can support dirty-time geometry | `GeneratedMass.cs:872-887`, `MeshData.cs:17-22`, `MeshBuilder.cs:37-58` | A post-generation main-mesh bevel/chamfer processor is feasible if inserted before atlas baking and mesh application. |
| Legacy secondary feature meshes | `GeneratedMass.cs:341-348`, `856-858`, `1563-1607` | Legacy child object names still exist for cleanup. They are not a reason to reintroduce secondary visual feature meshes. |
| Suspected stale code | `MassGenerator.cs:2724-3435`, search result: no call to `FaceMaterialMaskLookup.Build(...)` in `Game/Procedural/Masses` | Old face/edge mask lookup appears unreachable in the current baseline and should be cleaned up only after a compile-safe call-site audit. |
| Stale docs | `Generated_Mass_Framework.md:199-207` vs current baker/checklist | Framework doc still describes `FeatureAtlas1` as possible future Water/Frost/Sacred storage, while code already uses it for edge-wear irregularity. This must be corrected or replaced by a generic surface-data policy. |

---

## 3. Current implementation assessment

Keep:

```text
main-mesh material rendering path
MaterialPropertyBlock atlas binding
FeatureAtlas0 semantic split: proximity separate from importance
existing Macro/Micro inspector meanings
raw Atlas0/Atlas1 debug diagnostics
legacy secondary-feature cleanup code, for now
```

Fix or redesign:

```text
Macro Variation currently no longer produces clear enough inter-edge difference.
Atlas1 is too edge-wear-specific for the future feature library.
Atlas1 uses segment-local coordinates, so Micro cannot become truly same-ridge-aware yet.
The visible result is still albedo-only edge painting.
The full inspiration-rock look needs physical edge form, lighting/normal support, face material variation, and separate cracks.
```

Scrap or remove after verification:

```text
rejected Patch 14D.4 as a base
another Atlas1-only noise rewrite as the next visual solution
FeatureAtlas2 for edge wear
secondary visible edge-wear strips
vertex-interpolated line masks on original hard-edge faces
unused MassGenerator FaceMaterialMaskLookup block, if compile/search confirms no call sites
stale docs that describe Atlas1 as future Water/Frost/Sacred storage while code uses it for edge wear
```

---

## 4. Clear implementation checklist

### EW-1 — Macro control recovery / Atlas0.G hierarchy proof

**Goal:** restore the existing control contract before any new feature work.

Required behavior:

```text
Macro = 0:
  eligible ridges are comparatively even.

Macro = 1:
  some eligible ridges are clearly stronger/weaker than other eligible ridges.
  The difference must be obvious in normal rendering, not only in debug.

Macro must not:
  add noisy same-edge variation;
  widen every edge equally;
  act as a simple global brightness amplifier.
```

Evidence inspected before EW-1:

```text
GeneratedMass.cs:648-657
  Macro tooltip defines between-ridge strength variation; Micro defines same-ridge local variation.

GeneratedMassFeatureAtlasBaker.cs:758-783
  Atlas0.G already stores convex ridge weight / importance; Atlas1 stores local edge irregularity.

MassSurfaceFeatureGraph.cs:666-711
  Boundary score already derives convex edge importance from length, angle, height/lower penalty, and normal exposure.
  Therefore EW-1 should first use the existing Atlas0.G field more strongly, not invent a new scoring system.

SH_PixelSurfaceLit.shader:917-982
  The shader reads Atlas0.G as `convexWeight`, computes `macroWeight`, and multiplies final `edgeMask`.
  The pre-EW-1 mapping compressed low weights upward with `lerp(0.32, 1.36, pow(convexWeight, 0.72))`, so weak eligible edges could remain too visible at Macro = 1.
```

EW-1 implementation:

```text
Changed file:
  Game/Rendering/PixelSurface/Shaders/SH_PixelSurfaceLit.shader

Behavior change:
  Macro Variation remains strictly inter-edge.
  Macro = 0 keeps all eligible ridges comparatively even.
  Macro = 1 maps Atlas0.G more aggressively so lower-importance eligible ridges recede and high-importance ridges stay pronounced.

No changes:
  no new atlas
  no new texture sample
  no new inspector control
  no dirty-time scoring change yet
  no Micro behavior change
```

Validation:

```text
Response Strength = 1, Brightness Lift = 1, Micro = 0.
Toggle Macro from 0 to 1.
At Macro = 1, different ridges must visibly separate in strength.
The same ridge must stay internally clean; no within-edge noise from Macro.
```

---

### EW-2 — Atlas/channel cleanup and generic surface-data policy

**Goal:** prevent the system from becoming an edge-wear-specific texture pile.

Required behavior:

```text
FeatureAtlas0 remains generic boundary data.
FeatureAtlas1 must be explicitly documented either as temporary edge-wear support or redefined into reusable surface response fields.
No FeatureAtlas2 for edge wear.
No stale doc claiming Atlas1 is future Water/Frost/Sacred storage while code uses it for edge wear.
```

Recommended direction:

```text
Atlas0 = Boundary Geometry Fields
  R convex proximity
  G convex importance
  B concave proximity
  A concave importance

Atlas1 = Surface Response / Irregularity Fields, not "edge wear only"
  R response amplitude variation
  G response width/spread/smear variation
  B response continuity/thinning/breakup variation
  A reserved shared response field
```

This lets edge wear, frost, polish, sacred accents, crack lips, and other future features reuse the same response-support vocabulary instead of each demanding a new atlas.

Likely files:

```text
Docs/Generated_Mass_Framework.md
Docs/Generated_Mass_Feature_Implementation_Checklist.md
Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md
```

Validation:

```text
Docs match current code.
Any planned channel repurpose is explicit and approved before code changes.
```

---

### EW-3 — Ordered ridge-chain coordinates for Micro

**Goal:** make Micro Variation capable of true same-ridge variation.

Required behavior:

```text
Each usable ridge chain has ordered segments.
Each segment has cumulative chain-local start/end coordinates.
Atlas1 or future response fields can sample u = along-chain coordinate and v = cross-ridge distance.
Micro changes local width/intensity/thinning along the same ridge.
Macro remains inter-edge only.
```

Evidence/problem:

```text
MassSurfaceFeatureGraph.cs:126-140      // chain lacks ordered/cumulative data
MassSurfaceFeatureGraph.cs:568-612      // BFS grouping, not ordered tracing
GeneratedMassFeatureAtlasBaker.cs:838-847 // current irregularity uses segment-local bestT
```

Likely files:

```text
MassSurfaceFeatureGraph.cs
GeneratedMassFeatureAtlasBaker.cs
Docs
```

Validation:

```text
Macro = 0 or fixed value.
Micro = 0: cleaner continuous edges.
Micro = 1: same ridge shows controlled width/intensity/thinning variation.
No noisy dots.
No edge-to-edge hierarchy changes caused by Micro.
```

---

### EW-4 — Main-mesh worn bevel/chamfer foundation

**Goal:** stop relying on painted bands as the entire edge-wear effect.

Required behavior:

```text
Selected important convex ridges become small physical worn faces in the main mesh.
No secondary renderer.
No overlay strip.
No new texture atlas.
Minor/noisy ridges can be ignored.
```

Implementation shape:

```text
MassGenerator.Generate(recipe)
→ surface graph / edge selection
→ main-mesh bevel/chamfer processor
→ feature atlas bake
→ MeshBuilder.ApplyToMesh
```

First version should be conservative:

```text
process only high-importance convex ridges
use narrow width scaled by object size / edge importance
preserve hard stylized facets
mark explicit bevel faces with a safe face/vertex role channel if approved
```

Why this matters:

```text
The inspiration rocks have physical/light-readable worn edges.
Current shader only changes albedo; it cannot create physical edge form.
Dirty-time geometry cost is preferable to adding more per-feature atlases.
```

Likely files:

```text
GeneratedMass.cs
MassSurfaceFeatureGraph.cs
new edge geometry processor file, exact name TBD after approval
GeneratedMassFeatureAtlasBaker.cs only if atlas bake must understand new bevel faces
SH_PixelSurfaceLit.shader only if same patch interprets bevel role
Docs
```

Validation:

```text
With Response Strength = 0, selected edges still have real physical bevel/chamfer form.
With Response Strength > 0, material response enhances those surfaces instead of drawing only a line.
No secondary child renderers return.
```

---

### EW-5 — Bevel-aware material and lighting response

**Goal:** make worn areas read as material on a worn surface, not as white lines.

Required behavior:

```text
explicit bevel/worn face role = primary edge-wear region
Atlas0 convex proximity = optional adjacent shoulder/fade
Atlas1/generic response field = Micro/local irregularity support
Macro = edge importance only
Micro = same-edge variation only
```

First use real bevel geometry normals. Defer shader-side virtual bevel normals unless real geometry is insufficient.

Likely files:

```text
SH_PixelSurfaceLit.shader
GeneratedMass.cs only if new property/channel binding is required
Docs
```

Validation:

```text
Effect remains readable with lower Brightness Lift.
Edges catch light through form/normal response, not only through maxed albedo lift.
```

---

### EW-6 — Controlled chips / interruptions

**Goal:** create sparse physical/material discontinuities along selected ridges.

Required behavior:

```text
some sections widen
some sections pinch
some small interruptions exist
edge does not become dotted noise
```

Use ordered chain `u` from EW-3 and bevel geometry from EW-4. Do not start here.

---

### EW-7 — Broad stone face material variation

**Goal:** make edge wear sit inside a believable stone material instead of on flat blocks.

Current evidence:

```text
SH_PixelSurfaceLit.shader has _StoneMottle* controls.
MassGenerator writes vertex color surface variation/exposure/crevice data.
```

Required behavior:

```text
large faces get subtle tonal/mottle/mineral variation
edge wear is no longer the only visible detail
this remains separate from Convex Edge Wear
```

---

### EW-8 — Concave cracks / seams as a separate feature

**Goal:** add the dark crack/seam component visible in the inspiration rocks.

Required behavior:

```text
dark inner crack or groove
optional light lip
not every polygon edge
separate from convex edge wear
```

Use Atlas0.B/A where real concave creases exist. If generated masses do not produce enough concave candidates, design a separate crack-network generator later.

---

### EW-9 — Atlas quality tiers and runtime budget

**Goal:** stop memory from scaling blindly with every mass and future feature.

Current cost:

```text
FeatureAtlas0 512 RGBA32 no mips ≈ 1 MB
FeatureAtlas1 512 RGBA32 no mips ≈ 1 MB
Current total ≈ 2 MB per generated mass
```

Target policy:

```text
Hero / close mass:      512 if needed
Normal gameplay mass:   256 preferred
Background / tiny mass: 128 or no atlas fallback
```

No feature should receive a new atlas unless the shared surface-data budget explicitly approves it.

---

## 5. Channel policy

Current known channels:

```text
Vertex color R = surface variation
Vertex color G = exposure
Vertex color B = crevice/base
Vertex color A = reserved/neutral in current broad mask path; candidate for explicit bevel-face role only after approval

UV3 / TEXCOORD3 = generated feature atlas UV

FeatureAtlas0.R = convex ridge proximity
FeatureAtlas0.G = convex ridge importance
FeatureAtlas0.B = concave crease proximity
FeatureAtlas0.A = concave crease importance

FeatureAtlas1.R = currently edge-wear amplitude variation
FeatureAtlas1.G = currently edge-wear width/smear variation
FeatureAtlas1.B = currently edge-wear continuity/chip-thinning variation
FeatureAtlas1.A = reserved
```

Policy:

```text
Use geometry/vertex data for structural roles when interpolation is safe.
Use atlases for reusable semantic/support fields, not final color art.
Do not allocate one atlas per feature.
Do not repurpose a channel silently.
Document every channel contract next to the code that writes and reads it.
```

---

## 6. Guardrails

Do not implement another patch that:

```text
changes Macro into a global edge amplifier;
uses Micro as generic full-edge noise only;
adds a wider/brighter albedo band and calls it solved;
adds FeatureAtlas2 for edge wear;
reintroduces secondary visual strips;
uses vertex-interpolated line masks on original hard-edge faces;
updates docs without matching inspected code;
changes code without listing exact files and behavior first.
```

Every implementation step must start from current code evidence, list exact files, list exact behavior changes, and receive approval before editing.

---

## 7. Immediate next action

Do **EW-1: Macro control recovery / Atlas0.G hierarchy proof** first.

No bevel work, no atlas redesign, no chain-coordinate work, and no new controls until Macro is restored and validated.

Acceptance test for the next patch:

```text
Use the pre-14D.4 baseline.
Response Strength = 1.
Brightness Lift = 1.
Micro Variation = 0.
Macro Variation = 0: eligible edges are comparatively even.
Macro Variation = 1: some eligible edges are clearly more visible than others.
The effect must be visible in normal render, not only in debug.
```
