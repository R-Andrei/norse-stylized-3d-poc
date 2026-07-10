---
document_id: PS3D-GROUND-01
title: "Ground Visual Design and Architecture"
version: 0.1
status: active-baseline
scope: generated-ground-style-and-architecture
authoritative_for: "generated ground visual doctrine, style pillars, family/variant interpretation, shared ground style layers, static surface-mask contracts, and ground roadmap priority"
related_documents: [PS3D-00, PS3D-01, PS3D-02, PS3D-04, PS3D-06]
implementation_documents:
  - Ground_Generation_Surface_Upgrade_Plan.md
---
### 2026-07-10 — Patch V3J.3A2: Explicit Signed Angle Jitter Degrees

Patch V3J.3A2 keeps the V3J.3A1 whole-chunk distribution fix but corrects the remaining angle-control semantics. The active Painted Accent 3D stroke orientation rule is now deliberately simple: start with the feature `Direction`, roll one deterministic signed random value per stroke, multiply by `Angle Jitter Degrees`, and apply that offset in degrees. A value of 0 produces parallel strokes; a value of 30 allows offsets anywhere from -30 to +30 degrees. The generator no longer uses normalized angle variety or orientation families for Painted Accent 3D strokes.

Raised fold height and cross-section form remain deferred to V3J.3B.

### 2026-07-10 — Patch V3J.3A1: 3D Stroke Distribution Fix

Patch V3J.3A1 corrects two V3J.3A layout bugs reported from the 3D line preview. First, stroke placement no longer walks cells sequentially from a random offset and stops as soon as enough strokes are accepted. That row-major traversal could populate only one side of the chunk because accepted strokes filled the target count before the traversal reached the rest of the patch. The generator now builds the full candidate-cell set, assigns each candidate a deterministic random sort key, globally sorts that set, and accepts from the shuffled order. This preserves deterministic generation while spreading accepted strokes across the whole chunk.

Second, `Angle Variety` has been replaced in active code and UI by explicit `Angle Jitter Degrees`. V3J.3A used slash/vertical/backslash orientation families, but validation showed that this produced unwanted one-way/perpendicular orientation changes. The active rule is now: take the feature's preferred `Direction`, then apply a symmetric clockwise/counter-clockwise jitter of up to the authored degree value, clamped to 0-30 degrees. This gives `base - jitter`, `base`, and `base + jitter` variation, not multiple perpendicular families.

This patch still does not add raised fold height, lateral squiggle, or cross-section form. The next meaningful visual step remains V3J.3B, which should sweep a height/profile cross-section along the accepted 3D surface strokes.

### 2026-07-10 — Patch V3J.3A: 3D Stroke Distribution Controls

Patch V3J.3A keeps the V3J.3R source-of-truth correction but fixes the first preview's layout problems: too few generated lines, overly long strokes, and overly uniform slash-like orientation. The patch deliberately does not solve raised fold height or lateral squiggle. The current validation target is still the line layout itself, because the user needs to see the full raised 3D result before judging whether lateral deviation is desirable.

The active Painted Accent controls are now explicit for 3D surface-stroke layout:

```text
Stroke Width        -> preview/runtime ribbon width in metres
Stroke Density      -> approximate stroke count per standard 40x40 patch
Stroke Length Min   -> lower length bound in metres
Stroke Length Max   -> upper length bound in metres
Angle Jitter Degrees -> explicit symmetric +/- degree offset around the preferred feature direction
```

Generation no longer derives line count mainly from `Strength` or line length mainly from generic `Scale`. `Strength` remains feature intensity, while these new controls own the visible 3D stroke distribution. Stroke placement now uses a larger deterministic attempt grid so density changes produce more reliable preview changes after support rejection. Angle selection now means symmetric jitter around the preferred direction only; it does not introduce perpendicular orientation families.

Raised 3D fold form remains the next step. Patch V3J.3B should sweep an actual cross-section/profile along the accepted surface strokes; V3J.3A is only the layout-control patch.

### 2026-07-10 — Patch V3J.3R: Painted Accent 3D Stroke Baseline Reconciliation

Patch V3J.3R resets the active Painted Accent baseline after the V3J.0-V3J.2 experiments proved the wrong source model. The prior active path generated a broad/noisy fold body field, tried to threshold or contour it, and then attempted to infer one useful line. Validation showed predictable failures: shader contour extraction produced embossed topographic soup, connected-region crest extraction produced fat blobs/ribbons, and threshold controls could not compensate for a bad body-first source.

The active source of truth is now a generated 3D surface stroke. Each stroke is a short ground-following local-space curve sampled against `GroundHeightFieldSnapshot`; its points and normals are real 3D surface data. The fold-field texture remains available, but it is now derived from those 3D strokes: `R` is baked line coverage, `G` is the optional body/support around that line, `B` is stroke-relative side polarity, and `A` remains semantic support/reserved. Runtime shader work must consume this baked data; it must not rediscover regions or contours from noise.

The old height-field preview has been removed from the active workflow and replaced by a 3D line/ribbon preview. The preview builds actual temporary mesh ribbons from the generated stroke points so the next validation answers the important question first: do the generated 3D surface lines themselves look promising enough to become the effect?

# Ground Visual Design and Architecture

## Purpose

This document is the canonical generated-ground design document.

It exists so ground direction does not have to be reconstructed from the generic visual-language docs or from an implementation patch plan. It defines the ground's visual philosophy, style target, design pillars, surface-family architecture, shader/data contracts, feature-layer strategy, and active implementation priority.

The short version is:

```text
Restrained stylized terrain:
BOTW/TOTK-like base-material restraint
+ Hades-1-like painted ground accents
+ mostly 3D procedural geometry
+ reusable procedural masks and style layers
+ family/variant tuning.
```

This is now the baseline. Future ground work must either serve this direction or explicitly document why the direction is being changed.

## Authority and Document Boundaries

This document owns the durable generated-ground design baseline.

Use it for questions such as:

- What should generated ground look like?
- How simple or noisy should ground be?
- How should BOTW/TOTK and Hades 1 influence the ground?
- How do ground families and variants relate to the shared style doctrine?
- Which ground layers are foundational and which are niche?
- What should be built before runtime footprints, puddles, grass suppression, or rain?
- How should ground features avoid becoming one-off silos?

Use `Ground_Generation_Surface_Upgrade_Plan.md` for implementation history, patch notes, exact current status, and concrete patch sequencing.

Use `Proof of Concept/01_Visual_Language_and_Rendering.md` for broader project rendering principles, palette, lighting, camera, snow, fog, and general stylized 3D language.

Use `Proof of Concept/06_Proof_of_Concept.md` for the clearing prototype scope and validation goals.

If these documents conflict on generated-ground direction, this document is the ground-specific source of truth and the other document should be patched.

## Current Decision

The ground direction is not:

```text
Tunic block-world terrain
Hades 2 hand-painted production density
realistic ARPG terrain material complexity
high-frequency procedural noise
runtime simulation first
feature-by-feature terrain special cases
```

The ground direction is:

```text
A calm, readable, mostly matte 3D stage floor,
made rich through broad patch composition,
selective painted-looking accents,
contact/edge response,
sparse reusable motifs,
and later runtime/weather/interaction state.
```

The ground should support the scene rather than become the scene's loudest element. It should look intentional from an isometric/action camera while leaving characters, hazards, VFX, rivers, rocks, structures, silhouettes, fog, and lighting room to read.

## Reference Interpretation

References are production grammar, not literal style mandates.

### BOTW/TOTK: base-material restraint

The useful lesson is that base terrain can be visually quiet. Large ground areas can rely on:

- broad color/value regions;
- low-frequency material variation;
- readable terrain forms;
- slope and shoreline relationships;
- vegetation, rocks, cliffs, props, and paths;
- lighting and atmosphere;
- composition and silhouette hierarchy.

The ground material does not need to impersonate every blade of grass, pebble, mud wrinkle, snow grain, or stain. A simple base can work if the scene around it carries enough form and meaning.

### Hades 1: authored-looking accent grammar

The useful lesson is not exact brushwork. The useful lesson is floor grammar:

- broad readable regions;
- strong value grouping;
- sparse decorative marks;
- short dark mound/crease lines;
- cracks, chips, stains, scuffs, trim, and rhythm;
- contact emphasis around walls, props, edges, and boundaries;
- ground detail that supports gameplay readability.

The small dark ground lines visible in Hades 1 are especially relevant. They read as shallow mounds, grass folds, soft contour breaks, mud creases, or hand-painted surface rhythm. They imply form without needing real height deformation.

### Tunic: readability reference only

Tunic remains useful for high-angle readability, clean silhouettes, and compositional economy.

It is not the generated-ground style target. Tunic's basic ground works because the whole world is reduced to chunky toy/block geometry. This project is allowed to use more organic rocks, rivers, generated masses, snow, mud, shorelines, foam, and terrain detail. In that context, Tunic-simple ground can look underdeveloped rather than elegant.

### Hades 2: ambition reference, not production target

Hades 2 is not a feasible generated-ground baseline. Its floor art is extremely authored and dense. It can inspire taste, but it should not define implementation expectations.

### Realistic dark ARPGs: caution reference

Realistic or high-detail ARPG ground implies heavier authored textures, scans, decals, lighting, material response, grime, and asset production. That is not the current project direction.

## Non-Goals

Generated ground must not drift into the following unless a future design decision explicitly replaces this document:

- full hand-painted terrain everywhere;
- Hades 2-level authored floor detail;
- Tunic/voxel/block terrain as the primary visual target;
- realistic scanned terrain materials;
- noisy procedural texture soup;
- constant high-frequency variation to hide weak composition;
- every ground family using unrelated shader branches;
- one-off feature silos that cannot serve multiple families;
- runtime footprints, puddles, rain, wetness, or grass trampling before the static visual language works;
- heavy terrain height noise that threatens combat readability;
- river-specific or family-specific hacks inside unrelated systems.

## Sacred Design Rule

The ground is a shared visual stack tuned by families and variants.

It is not a collection of unrelated special-case effects.

```text
Shared doctrine stack
  calm base
  broad patches
  semantic mask response
  painted accent lines
  contact / edge accents
  sparse motifs
  runtime state later

Family / variant tuning
  decides how a specific surface expresses that stack.
```

A new ground feature is valid only if it can name which layer it belongs to and how it remains reusable or deliberately scoped.

## Style Pillars

### Pillar 1 - Calm Base Surfaces

Base ground is the stage floor.

It should be:

- matte or mostly matte;
- broad;
- low-noise;
- low-to-moderate contrast;
- readable from the game camera;
- controlled by family/variant material values;
- subordinate to characters, hazards, VFX, rivers, rocks, props, silhouettes, and lighting.

The base material should not carry all terrain identity by itself. Earlier wet mud tuning showed the failure mode: broad smoothness/color changes can become plastic or playdough. The solution is not endless global material tweaking; it is a restrained base plus explicit style layers.

### Pillar 2 - Broad Macro Patch Composition

The first visible variation layer should be large and deliberate.

Good macro patches:

- use low-frequency variation;
- create broad value/color islands;
- respect the family palette;
- read from the isometric camera;
- avoid checkerboard noise;
- can be posterized or softened;
- can be biased by exposure, damp/deposit, shore, vegetation, and compaction masks;
- support composition instead of visual mush.

Bad macro patches:

- look like arbitrary Perlin noise;
- create equal activity everywhere;
- fight player/foe silhouettes;
- hide path readability;
- overpower rivers, shorelines, or combat telegraphs.

### Pillar 3 - Painted Accent Lines

Painted accent lines are the first foundational new style layer after this doctrine.

They are short, broken, Hades-1-like dark/value-shifted surface strokes. They should suggest:

- small mounds;
- grass folds;
- mud creases;
- snow wrinkles;
- soft contour breaks;
- surface age;
- hand-authored rhythm.

They are visual only. They do not require terrain deformation.

#### Chosen Implementation Direction - Generated 3D Surface Strokes

Patch V3J.3R changes the Painted Accent source of truth.

The active source model is no longer:

```text
generate anonymous scalar/noise body field
  -> threshold or contour the body
  -> infer a representative line
```

Validation retired that model. It produced either broad value-noise continents, embossed contour soup, or fat selected ribbons. The selected line is the visual feature we actually care about, so it should not be rediscovered from a noisy body field after the fact.

The active source model is now:

```text
generate short 3D surface stroke descriptors
  -> walk/sample those strokes on the generated ground surface
  -> preview the actual 3D stroke geometry
  -> bake cheap R/G/B/A texture channels from the strokes for shader use
```

The stroke is a local-space 3D curve on the generated terrain surface. For each sampled point, the generator knows:

```text
local 3D position on the ground
surface/render normal from GroundHeightFieldSnapshot
tangent along the stroke
across-line direction on the surface
stroke width, body width, strength, seed
```

This keeps the feature 3D at source while still allowing cheap shader rendering later. The texture is a baked representation of 3D surface strokes; it is not the authored source and must not be treated as a flat 2D decal system.

The feature remains visual-only unless explicitly promoted later:

```text
no collision change
no gameplay terrain deformation
no runtime footprint/wetness simulation
no new layers or tags
no production mesh modification during normal generation
```

The immediate validation target is the 3D line/ribbon preview, not final material response. If the 3D lines look good enough, later patches can decide whether to keep them as actual geometry, bake them into the shader fold-field texture, or use both.

#### Retired Painted Accent Experiments

The following experiments are retained only as history and should not be tuned as active direction:

```text
V3D-V3F.1 curve-distance strokes:
  line first -> inflated 2D relief tube -> side rails
  failed as scratches/capsules/rails

V3I/V3I.1 candidate stamps:
  discrete oval/ridge stamps -> body/line channels
  failed as leaf/brush stamps

V3I.2 continuous body field:
  domain-warped value field -> G body -> rough R/B
  failed as blocky/noisy field placement for this line target

V3J.0 final prototype:
  trusted existing R as if it were already the selected line
  failed as faint/blocky smudging

V3J.1 shader contour extraction:
  sampled neighboring G and drew local contour bands
  failed as embossed topographic soup

V3J.2 peak-region crest extraction:
  thresholded G, labeled regions, inferred internal crest lines
  threshold worked, but the source regions remained bad and selected lines read as fat blobs/ribbons
```

The lesson is now part of the baseline: **generate the 3D line intentionally, then derive any supporting body/texture response from that line.**

#### Fold-Field Texture Contract

The texture contract survives, but its source changes. Patch V3J.3R derives the generated fold-field texture from 3D surface strokes instead of from noise/body-first inference.

```text
R = baked selected stroke-line coverage from generated 3D surface strokes
G = soft body/support around those strokes, for context or later shading
B = stroke-relative signed side encoded 0..1, with 0.5 as neutral
A = semantic support / reserved future validity channel
```

The shader decodes the channels as:

```text
selectedLine = R
bodyContext  = G
signed       = B * 2 - 1
```

The debug views keep their existing names but now mean:

```text
Ground Painted Accent Lines
  shows R: baked line coverage from generated 3D surface strokes

Ground Painted Accent Relief
  shows G: soft support/body around those 3D strokes

Ground Painted Accent Signed Relief
  shows B decoded as stroke-relative side polarity

Ground Painted Accent Final Prototype
  shades the baked R line with B polarity and weak G context
```

Runtime shader policy is strict: the shader samples baked channels and shades them. It does not perform connected-component labeling, contour extraction, body-field thresholding, or line discovery.

#### 3D Line Preview Policy

The old `Build Height Preview` / `Clear Height Preview` workflow is retired. It visualized the rejected G/body field as a displaced grid and therefore kept attention on the wrong artifact.

The active preview is:

```text
GeneratedGround inspector:
  Stroke Width
  Build 3D Line Preview
  Clear 3D Line Preview

GeneratedGround child object:
  __FoldFieldLinePreview_Debug
```

The preview mesh is built from the generated stroke descriptors. Each stroke point is lifted slightly along the sampled surface normal, then widened into a small ribbon using the local tangent and normal. This is editor/debug-only geometry:

```text
it does not modify the generated ground mesh
it does not modify collision
it does not imply production displacement
it does not require a new layer/tag/component
```

#### Chunk-Library and Runtime Policy

The intended game workflow is a library of reusable authored chunks:

```text
editor:
  generate/author chunks
  validate the fold-field result
  save the chunk as reusable runtime content

runtime map builder:
  choose authored chunks from the library
  rotate/place/connect them in new arrangements
  generate run-specific minutiae such as unit placement and doodads
```

The outside world may be rebuilt while the player is in camp, but the fold-field feature is not a per-frame simulation. It is generated at edit time or load/camp rebuild time before gameplay resumes.

Because chunks can be rotated and reused, fold-field sampling must be chunk-local rather than world-locked. The shader samples with object-space X/Z (`positionOS.xz`) so the painted fold field rotates with the chunk when the runtime map builder rotates that chunk.

#### Resolution and Performance Policy

There is no low/background/hero/special resolution tier for this feature.

The policy is:

```text
visible authored gameplay chunk with PaintedAccentLines active:
  generate one 256x256 RGBA32 fold-field texture

hidden/offscreen/background chunk:
  disable PaintedAccentLines entirely
  do not generate a lower-resolution fold-field texture
```

Memory budget:

```text
256x256 RGBA32 = 262,144 bytes = 256 KiB per active chunk

1 chunk    = 256 KiB
10 chunks  = 2.5 MiB
50 chunks  = 12.5 MiB
100 chunks = 25 MiB
200 chunks = 50 MiB
```

This is acceptable for the projected game scale. A visual style demo validates one chunk. A vertical slice may use roughly ten chunks. A beta-like version may use around fifty. A full game might use around one hundred active selected chunks, with two hundred treated as a remote upper bound. If chunks are meant to be fully hidden by walls, relief, fog, or camera framing, they should disable the feature rather than use a reduced texture.

Patch V3I generation is bounded:

```text
fixed 256x256 texture
RGBA32
no mipmaps
CPU texture copy discarded after upload
candidate rasterization instead of broad per-pixel cell searching
hard candidate cap
```

The production cleanup target is to sample the fold field once per visible ground fragment and reuse that result through albedo/smoothness/surface response. Prototype patches may temporarily sample more than once while the data model is being validated.


They should be:

- short;
- broken;
- slightly curved;
- clustered rather than uniform;
- low-to-medium contrast;
- darkened or value-shifted rather than pure black;
- sparse enough that quiet ground remains quiet;
- tuned per family/variant;
- reusable across snow, mud, grass, rocky dirt, shore, and path surfaces.

They must not become:

- uniform hatching;
- full-screen scribble noise;
- equal-density procedural cracks everywhere;
- hard black outlines unrelated to lighting/material;
- a mud-only or grass-only feature silo.

#### Patch V3I Validation and V3I.1 Body-Shape Correction

Patch V3I validated the new data path: the ground is now reading generated local-space fold-field texture data instead of the retired curve-distance stroke fallback when `PaintedAccentLines` is active. The three debug channels changed exactly as expected for the first prototype:

```text
Ground Painted Accent Relief:
  showed the generated G/body field as soft pale fold bodies

Ground Painted Accent Signed Relief:
  showed gradient polarity around those bodies

Ground Painted Accent Lines:
  showed rough edge/crescent candidates derived from the bodies
```

The result proved the architecture but not the final body shape. The V3I generator used one soft elongated ellipse per candidate, so the relief field read as repeated large oval/leaf stamps. That is still the wrong art read for the final target.

Patch V3I.1 corrects the body generator before any line-art polish. The candidate primitive changes from:

```text
one soft ellipse
```

to:

```text
one short curved tapered ridge/fold body
  with variable width
  asymmetric side weight
  deterministic local warp
  optional small side lobe
  lower density and more negative space
```

This keeps the accepted field-first architecture:

```text
fold body first
then signed polarity
then rough contour/line candidate
```

It does not return to line-first curve strokes. It also does not introduce mesh displacement, collision, fake normals, 3D preview tooling, family tuning, or final line extraction. The validation target remains `Ground Painted Accent Relief`: it should read less like large oval stamps and more like short irregular low terrain folds/ridges. Final contour extraction remains Patch V3J.


#### Patch V3I.2A - Candidate-Stamp Generator Retirement

Patch V3I.2A is a cleanup/redirection patch before the next generator implementation. It removes the V3I/V3I.1 candidate-stamp generator internals from the active code path and replaces them with a neutral 256x256 placeholder texture.

The V3I/V3I.1 validation outcome is now locked:

```text
V3I proved:
  GeneratedGround-owned fold-field texture plumbing works.
  Local-space sampling works.
  The R/G/B/A debug contract is active.
  New shape / seed changes update generated data.

V3I failed visually because:
  the generator still created discrete procedural shapes.
  the relief channel read as large oval/leaf stamps.

V3I.1 improved:
  ellipses became smaller curved tapered forms.
  density was reduced.
  bodies were less uniformly oval.

V3I.1 still failed visually because:
  the model remained candidate/stamp based.
  the output read as sparse brush marks, not a natural secondary height layer.
```

Therefore, the following systems are retired as an active direction:

```text
BuildCandidates(...)
RasterizeCandidates(...)
FoldCandidate
candidate density/cell spawning
candidate curvature/asymmetry/side-lobe stamp model
ellipse/ridge stamp language as the source of the body field
```

The retained systems are:

```text
GeneratedGround-owned fold texture lifecycle
local/object-space sampling
256x256 RGBA32 active-chunk policy
R/G/B/A texture contract
debug views
shader router
retired curve-distance fallback for inactive/missing generated data
```

The next accepted implementation is continuous field generation:

```text
continuous domain-warped scalar field F(local x, local z)
  -> shaped visual height/body G
  -> gradient polarity B
  -> rough selected contour/edge R
```

Patch V3I.2A intentionally produces a blank/neutral generated fold texture for active `PaintedAccentLines` chunks. This prevents further visual tuning of the rejected stamp generator while keeping compile/runtime plumbing intact for Patch V3I.2. It is expected that Painted Accent Relief / Signed Relief / Lines debug views will show no generated fold marks during this transition.

Patch V3I.2 will replace the neutral placeholder with:

```text
GenerateBaseNoiseField(...)
GenerateDomainWarp(...)
ShapeContinuousBodyField(...)
ApplySemanticSupport(...)
SmoothBodyField(...)
BuildPixelsFromContinuousField(...)
```

No candidate spawning, no stamp rasterization, no mesh displacement, no 3D preview tooling, no final line extraction, and no family tuning are part of V3I.2A.


#### Patch V3I.2 - Continuous Domain-Warped Fold Height Field

Patch V3I.2 replaces the neutral V3I.2A placeholder with the first continuous scalar-field implementation. This is the first generator that matches the accepted field-first direction instead of the rejected candidate/stamp direction.

The active model is now:

```text
continuous domain-warped scalar field F(local x, local z)
  -> shaped visual height/body G
  -> gradient polarity B
  -> rough selected contour/edge R
```

The generator no longer uses:

```text
BuildCandidates(...)
RasterizeCandidates(...)
FoldCandidate
discrete mark spawning
ellipse stamps
curved ridge stamps
```

The V3I.2 field generation pipeline is:

```text
local chunk coordinate
  -> deterministic domain warp
  -> broad fractal value field
  -> medium fractal value field
  -> ridge-like fractal component
  -> directional continuity component
  -> semantic support from the existing generated ground masks
  -> percentile-based coverage threshold
  -> soft body shaping
  -> light smoothing
  -> R/G/B/A texture write
```

The coverage normalization is important. The generator does not use one hard global threshold; it resolves a percentile threshold from the generated field so the active body coverage remains bounded by feature strength:

```text
low strength  -> lower active coverage
high strength -> higher active coverage
```

This is intended to prevent both failure extremes:

```text
full-screen cloudy mush
isolated procedural stamps
```

The texture contract remains unchanged:

```text
R = rough contour/edge candidate from G
G = continuous visual fold-height/body field
B = signed side from the gradient of G, encoded 0..1
A = semantic support / reserved
```

The primary validation target remains `Ground Painted Accent Relief`, which displays the G/body field projected on the current ground surface. `Ground Painted Accent Lines` remains a rough derivation and is not final line extraction. V3J remains the line extraction polish patch.

This implementation is still visual-only:

```text
no mesh displacement
no collision change
no fake normal
no production terrain deformation
```

A true debug height preview is explicitly planned as the next diagnostic tooling layer:

```text
Patch V3I.3 - Fold Field Height Preview Debug Mesh
```

The chosen preview approach is Option B: generate an editor/debug-only preview mesh from the fold-field texture and displace that preview by the G channel. This will show the field honestly at preview resolution instead of relying on the existing ground mesh density. It must remain debug-only and must not imply gameplay displacement.



#### Patch V3I.3 - Fold Field Height Preview Debug Mesh

Patch V3I.3 implements the planned Option B diagnostic preview. The existing `Ground Painted Accent Relief`, `Ground Painted Accent Signed Relief`, and `Ground Painted Accent Lines` views remain projected texture-channel debug modes. They are useful, but they do not show the field as actual relief.

The new preview is editor/debug-only:

```text
Generated fold-field G channel
  -> temporary child preview mesh
  -> vertex height = sampled ground height + G * debug height scale + small lift
```

The preview mesh is intentionally separate from the production ground mesh:

```text
does not modify the generated ground mesh
does not modify collision
does not change gameplay terrain
does not imply production displacement
does not require new layers or tags
```

The preview mesh is created as a child object named:

```text
__FoldFieldHeightPreview_Debug
```

The `GeneratedGround` inspector exposes:

```text
Build Height Preview
Clear Height Preview
```

This preview uses the same generated G/body values that are written into the fold-field texture, not a GPU readback or a second approximation. `GroundPaintedAccentFoldFieldGenerator.Generate(...)` now returns the generated body array alongside the uploaded texture so the debug mesh can be built from the exact same scalar field.

This patch is diagnostic only. It does not tune the continuous field generator and does not perform final contour extraction. The next decision should be based on inspecting the projected G channel and the height preview mesh together.


#### Patch V3I.3A - Debug Isolation and Preview Color Readability

Patch V3I.3 validation exposed two pipeline issues:

```text
1. The generated fold field could still influence the normal final ground render.
2. The preview mesh had real displacement, but its material did not visualize height values from top view.
```

Patch V3I.3A fixes those issues before any generator tuning. The generated fold field remains diagnostic-only until final response is deliberately rebuilt in V3J/V3K.

Final render isolation rule:

```text
Generated fold-field data may feed:
  Ground Painted Accent Relief
  Ground Painted Accent Signed Relief
  Ground Painted Accent Lines
  Ground Painted Accent Final Prototype
  Fold Field Height Preview mesh

Generated fold-field data must not feed:
  normal final albedo
  normal final smoothness
  normal final specular
  production material response
```

The final forward pass now zeros Painted Accent final-render contribution while `_GroundPaintedAccentFoldFieldEnabled` is active. This preserves the debug data path while preventing the experimental G field from appearing as noise in the normal game render.

The height preview mesh now uses an explicit hidden debug shader:

```text
Hidden/PS3D/Ground Fold Field Height Preview
```

The shader reads the preview mesh vertex color/body value and maps it to a visible low/mid/high debug gradient. This makes the height field readable from top view as well as from profile. The preview renderer also disables shadow casting and shadow receiving so the preview does not contaminate lighting diagnostics.

Preview cleanup was also made more robust: clearing the preview removes all child objects whose names begin with `__FoldFieldHeightPreview_Debug`, rather than relying on only one exact child lookup.

V3I.3A is a correctness patch only:

```text
no generator tuning
no final line extraction
no production displacement
no collision changes
```


#### Patch V3J.0 - Painted Accent Final Visual-Response Proof

Patch V3J.0 adds one debug-only view:

```text
Ground Painted Accent Final Prototype
```

This view exists to answer a specific architecture question before generator tuning continues: can the generated fold-field contract be turned into the intended painted fold/crease visual language? It is not a production render path and does not remove the V3I.3A final-render isolation rule.

The prototype response treats the generated channels as follows:

```text
R = selected contour / narrow visible crease source
G = soft fold body / context gate only
B = signed side / crease-highlight polarity
A = support / still reserved for field semantics
```

The important rule is that `G` must not directly become broad albedo darkening. The previous V3I.3 validation proved that broad body modulation reads as noisy stains. In V3J.0, `G` only gates where the narrow `R` contour is allowed to become visible; the prototype color is driven by a crease mask, a smaller side highlight mask, and a very low context term.

This patch intentionally does not tune the field generator. A bad field can still make bad shapes. The proof target is narrower: if the current ugly field can still produce crease-like marks when only the contour/signed channels are emphasized, the architecture is viable and the next patch should improve field shape/placement. If the prototype still reads as stains despite the restricted response, the response model or channel contract needs revision before generator tuning.

V3J.0 is limited to:

```text
debug-mode enum plumbing
shader-only prototype visualization
documentation of the proof contract
```

It does not add mesh displacement, collision changes, production normal perturbation, family tuning, generator tuning, new components, decals, or runtime state.

V3J.1 correction also failed the actual target. It moved extraction into the shader, but the shader produced embossed contour soup: many local G level sets, not one selected line per meaningful fold. V3J.2 reconciles the architecture: region selection belongs to generation/dirty time, not the runtime fragment shader. The generator now thresholds G, labels connected peak regions, rejects small junk regions, extracts one representative internal crest line per accepted region, and writes that selected line to R. The Final Prototype shader consumes R directly and only uses G as weak context plus B for one-sided dark/light polarity.

#### Patch V3J.2 - Precomputed Peak-Region Crest Lines

Patch V3J.2 is the reconciliation patch after the failed V3J.0/V3J.1 shader-only proof attempts. The accepted division of responsibility is now:

```text
generation / dirty time:
  generate continuous G/body field
  apply a temporary peak threshold
  identify connected peak regions
  discard tiny regions
  select one internal crest/accent line per accepted region
  write that selected line to R

runtime shader:
  sample R/G/B/A
  shade the already-selected R line
  do not perform connected-component or contour-band extraction
```

Temporary authoring controls live on `GroundSurfaceFeatureRecipe` for `PaintedAccentLines` only:

```text
Painted Accent Peak Threshold
Painted Accent Minimum Peak Area
Painted Accent Crest Width Texels
```

These controls are proof/tuning controls, not final family art direction. The important contract is that `R` is now line-selection data, not a broad activity field and not a shader-derived contour approximation.


### Pillar 4 - Contact and Edge Accents

Ground should visually respond around meaningful geometry and semantic boundaries. This is one of the main ways an isometric scene looks authored instead of assembled.

Contact/edge accent candidates:

- rock bases;
- standing stones;
- cliffs and banks;
- river shorelines;
- bridge or crossing contact;
- path boundaries;
- modifier boundaries;
- raised/lowered terrain edges;
- structure foundations;
- camp pads and authored clearings;
- damp deposits near water;
- snow buildup near wind-protected edges.

This layer may add local darkening, dampness, deposit hints, outline-like value shifts, accent-line density changes, or surface-wear emphasis. It must not turn every object into a heavy decal blob.

### Pillar 5 - Sparse Motifs and Stamps

After accent lines and contact accents, the next detail tier is sparse motif/stamp content.

Examples:

- chips;
- cracks;
- small stones;
- dirt strokes;
- dry scuffs;
- mud stains;
- snow scrape marks;
- tiny tuft-like marks;
- leaf/debris hints;
- frost specks;
- ash specks;
- broken trim marks.

Rules:

- sparse beats dense;
- clusters beat uniform distribution;
- motifs should respond to semantic masks;
- motifs should not tile obviously;
- motifs should not be required for the base ground to look acceptable;
- each family/variant should be able to reduce or disable motif density.

### Pillar 6 - Runtime State Later

Runtime surface state remains valuable, but it is not the foundation right now.

Deferred runtime state includes:

- rain wetness;
- drying;
- snow depth;
- snow compression;
- footprints;
- grass trampling;
- mud disturbance;
- puddle growth;
- standing-water evolution;
- disturbance age.

These systems should wait until the static visual language works. Runtime state is expensive in complexity even if the texture memory is manageable. It should not be used to compensate for unresolved art direction.

### Pillar 7 - Geometry Still Matters

The ground is not only a shader plane.

The scene should also be carried by:

- terrain silhouette;
- banks and slopes;
- rivers and shore corridors;
- generated rocks/masses;
- structures and ruins;
- grass/vegetation later;
- snow banks later;
- fog and lighting;
- prop placement;
- manual composition.

A calm base material works only if these scene layers participate.

## Ground Composition Stack

The intended render/meaning stack is:

```text
Playable terrain shape
  ↓
Calm family base material
  ↓
Broad macro patch composition
  ↓
Static semantic mask response
  exposure / damp / deposit / shore / vegetation / compaction / rocky-dry
  ↓
Painted accent lines
  ↓
Contact / edge accents
  ↓
Sparse motifs and stamps
  ↓
Runtime surface state later
  wetness / snow depth / compression / footprints / mud / puddles
  ↓
Debug override
```

The ordering matters:

- the base must remain readable without detail layers;
- macro patches define the broad composition;
- semantic masks make the ground respond to generated meaning;
- accent lines add the Hades-1-like authored rhythm;
- contact accents glue objects and terrain together;
- sparse motifs add identity without noise;
- runtime state modifies the surface only after the static language is proven.

## Family and Variant Architecture

The existing family/variant architecture remains correct. It now has clearer meaning.

```text
GroundSurfaceProfile
  semantic / mask-generation profile

GroundSurfaceStyleProfile
  visual surface family

GroundSurfaceVariantRecipe
  family-local recipe

GroundMaterialControls
  calm base-material response

GroundSurfaceFeatureRecipe
  reusable style-layer tuning

GeneratedGround
  resolver, top-level authoring surface, per-object override owner
```

The family decides what kind of surface this is.

The doctrine decides how all surfaces speak visually.

The variant tunes how much of each shared style layer appears.

### GroundSurfaceProfile

`GroundSurfaceProfile` owns semantic/mask-generation intent. It should describe what the generated surface means and what static masks are produced, not the entire visual material response.

Examples:

- snowfield semantic profile;
- wet mudflat semantic profile;
- grassland semantic profile;
- future rocky scrub profile;
- future ash or frost profile.

### GroundSurfaceStyleProfile

`GroundSurfaceStyleProfile` owns a visual family.

Examples:

- Snowfield;
- Wet Mudflat;
- Grassland;
- future Rocky Scrub;
- future Ash/Frost/Corruption surface.

A family should not require bespoke shader logic for basic ground language. It should tune the shared doctrine stack.

### GroundSurfaceVariantRecipe

`GroundSurfaceVariantRecipe` owns a family-local recipe.

Examples:

```text
Snowfield.clean
Snowfield.patchy
Snowfield.dirty_thawing
Snowfield.wind_scoured

WetMudflat.damp_mud
WetMudflat.waterlogged
WetMudflat.trampled
WetMudflat.frozen_thaw

Grassland.clean_meadow
Grassland.patchy_meadow
Grassland.damp_meadow
Grassland.worn_meadow
```

Variants should tune:

- base color/value;
- snow/mud/damp/shore response;
- macro patch scale/contrast;
- painted accent-line density/contrast;
- contact accent strength;
- sparse motif density;
- specific mask response such as compaction or wetness.

Variants should not create unrelated one-off rendering pipelines.

### GroundMaterialControls

`GroundMaterialControls` should remain the calm base material and broad response recipe.

It is appropriate for:

- base color;
- secondary color;
- brightness/value bias;
- tint strength;
- damp darkening;
- snow tinting;
- smoothness/specular baseline;
- patch scale/contrast;
- broad material response.

It is not the right place to fake every terrain detail. If visual richness requires small strokes, contact accents, motif stamps, or runtime state, those should be explicit layers.

### GroundSurfaceFeatureRecipe

`GroundSurfaceFeatureRecipe` should evolve from one-off features into reusable style-layer tuning.

Good feature kinds are doctrine layers or reusable semantic responses:

- PaintedAccentLines;
- ContactEdgeAccents;
- SparseMotifs;
- DirectionalSurfaceMarks;
- CompactionResponse;
- PooledWetnessResponse;
- ShoreDepositResponse.

Bad feature kinds are overly narrow unless deliberately scoped:

- one exact mud decal;
- one exact snow footprint look;
- one exact puddle shape hardcoded into a family;
- a feature that only works because a single current asset happens to need it.

## Current Feature Interpretation

Existing feature work should be reclassified under the doctrine rather than discarded.

### DirectionalStreaks

Keep, but reinterpret as an early directional surface-mark proof.

It may eventually fold into Painted Accent Lines or Directional Surface Marks.

### PooledWetness

Keep as a wetness response proof, but do not treat it as final puddles.

Real puddles/standing water are later explicit features or runtime state. Wet mud base should remain mostly matte.

### TrampledWear

Keep as a compaction/path response proof.

Patch U proved this flow:

```text
GroundModifier authored surface mask
→ generated metadata
→ UV2.x compaction/path
→ shader feature response
```

After the doctrine pivot, `TrampledWear` is no longer the active cornerstone. It should be used as one stackable compaction response layer, not polished as a bespoke terrain direction.

### PaintedAccentLines

This is the first real doctrine-layer feature.

It creates short, broken, slightly curved, dark/value-shifted surface strokes that suggest grass folds, mud creases, snow wrinkles, small mounds, and surface age. It is visual only: no decals, textures, height deformation, mesh edits, or runtime state.

It must remain sparse and authored-looking. Failure modes are global hatching, scratch noise, grass-blade hair, mud-crack networks, or black outline marks.

## Feature Stack Direction

Patch V3 makes the variant feature list the canonical composition model.

```text
GroundSurfaceVariantRecipe.features
  -> first enabled recipe of each supported ShaderOnly kind wins
  -> GeneratedGround writes explicit shader-property blocks per kind
  -> shader applies all supported layers in a stable renderer-defined order
```

This replaces the earlier proof-feature shortcut where `_GroundFeatureMode` selected one mutually exclusive feature. `_GroundFeatureMode` may remain as a hidden serialized compatibility property, but it must not be extended as the long-term feature architecture.

A variant may now combine shader-only feature recipes, for example:

```text
grassland.damp_meadow
  PaintedAccentLines
  PooledWetness

grassland.worn_meadow
  PaintedAccentLines
  TrampledWear

snowfield.wind_scoured
  DirectionalStreaks
  PaintedAccentLines
```

Composition rules:

- feature recipes are a list, not a dropdown choice;
- features are not mutually exclusive by default;
- first enabled recipe of a given kind wins;
- duplicate enabled recipes of the same kind should be treated as authoring mistakes;
- unsupported or non-ShaderOnly recipes may remain in the asset contract but do not render until implemented;
- shader composition order is stable and renderer-defined, not asset-list-order-defined;
- style authors are responsible for choosing coherent combinations.

Current supported shader stack layers:

```text
DirectionalStreaks
PooledWetness
TrampledWear
PaintedAccentLines
```

Future layers such as ContactAccents and SparseMotifs should follow the same stack model instead of adding one-off bespoke plumbing.

## Static Surface Data Contract

Generated ground uses vertex colors and UV2 as static semantic masks.

Current intended contract:

```text
Vertex Color R = tonal patch variation
Vertex Color G = exposure / snow-hold potential
Vertex Color B = damp / deposit potential
Vertex Color A = vegetation suitability

UV2.x = compaction / path / flatten influence
UV2.y = river / shore influence
UV2.z = rocky / dry secondary patch
UV2.w = authored standing-water / puddle potential
```

Design rules:

- these are semantic masks, not final visual effects;
- shader/style layers interpret them;
- debug modes must remain available and trustworthy;
- do not repurpose channels silently;
- if a channel meaning changes, update this document and the implementation plan together.

## GroundModifier Contract

Ground modifiers can now affect height, authored surface masks, or both.

The accepted design is:

```text
Mode = None + Surface Effect Mode = Custom
  → surface masks only, no height change

Mode = Flatten/Lower/Raise + Surface Effect Mode = Custom
  → height change + authored surface masks

Mode = Flatten + Surface Effect Mode = AutoFromHeight
  → legacy flatten writes compaction

Surface Effect Mode = None
  → height-only modifier, no authored surface meaning
```

Design rules:

- prefer surface-only masks when visual response is enough;
- allow small height denivelations for roads, wagon tracks, camp pads, puddle basins, and intentional terrain shaping;
- keep height changes combat-safe and camera-stable;
- do not bake final snow paths or grass paths into base ground; future snow/grass systems should interpret masks/runtime state.

## River Corridor Contract

River corridors must remain style-agnostic.

Correct relationship:

```text
GeneratedGround resolves family/variant/material/feature state
→ applies MaterialPropertyBlock to ground renderer
→ refreshes dependent river corridor renderer
```

Wrong relationship:

```text
StylizedRiver knows Snowfield, WetMudflat, TrampledWear, PaintedAccentLines, etc.
```

The river may consume ground snapshots, shore masks, and material-property refreshes. It must not own ground style-family logic.

## Shader and Material Contract

Generated ground uses a dedicated ground shader path, separate from generated masses.

Important rules:

- keep ground-specific contracts explicit;
- use `MaterialPropertyBlock` for object/variant-resolved values;
- avoid duplicating materials for every variant;
- keep debug modes stable;
- do not shift existing debug enum values casually;
- avoid turning the shader into a monolith of unrelated family branches;
- aggregate shared style-layer controls rather than hardcoding family-specific features.

The shader stack should eventually expose or receive controls for:

- base material response;
- macro patch scale/contrast;
- semantic mask responses;
- painted accent lines;
- contact/edge accents;
- sparse motif density;
- wetness/compaction/shore responses;
- runtime state later.

## Cost Classes

Ground features should declare or imply cost class.

Preferred progression:

```text
ShaderOnly
  first pass for broad visual layers

MeshMaskDriven
  when generated static masks are needed

GeneratedTexture
  only when the visual cannot be achieved cleanly from mesh masks/world-space shader logic

RuntimeState
  only after static style language is accepted
```

Do not make every style pay for every feature.

## Active Roadmap

The old runtime-first roadmap is paused.

Patch T and Patch U remain useful, but they are not the active direction:

- Patch T established the authored surface-mask contract.
- Patch U proved a shader feature can consume `UV2.x` compaction/path.

The active direction is now style calibration and shared doctrine layers.

| Priority | Patch | Goal |
| --- | --- | --- |
| 1 | V0 — Ground Visual Doctrine Documentation | Completed by documentation. Establish the canonical doctrine and this design doc. |
| 2 | V1 — Style Calibration Setup | Completed as a temporary `Style Calibration` surface family with four comparison variants. |
| 3 | V2 — Base Ground Simplification | Implemented as an asset/docs retune: Snowfield and Wet Mudflat now target calmer matte, lower-noise base surfaces. |
| 4 | V2B — Grassland Baseline Family | Implemented as a real production `Grassland` surface family so shared style layers can be validated across snow, mud, and living ground. |
| 5 | V3 — Shader Feature Stack + Painted Accent Lines | Implemented as the first stackable doctrine layer and as the migration away from the old single `_GroundFeatureMode` proof-feature slot; V3D refines the raw accent-line mask from large strips into smaller clustered micro-strokes, and V3E upgrades those strokes into curved visual-relief terrain folds. |
| 6 | V4 — Contact / Edge Accent Layer | Add localized response near shores, rocks, modifier boundaries, paths, banks, and object contact zones. |
| 7 | V5 — Sparse Motif Layer | Add reusable sparse chips, cracks, scuffs, stains, snow scratches, stones, and debris hints. |
| 8 | V6 — Feature Stack Authoring Polish | Add richer editor warnings, cost summaries, and feature-combination guidance after more stack layers exist. |
| 9 | Later | Runtime Surface State Stub | Revisit wetness, snow depth, compression, footprints, and disturbance after static style acceptance. |
| 10 | Later | Footprints / Rain / Puddles / Grass Integration | Build on runtime state only after the visual doctrine is proven. |
| 11 | Future | Mixed Terrain / Profile Blending | Blend surface families such as snow over mud, rocky scrub over soil, or worn path through snow. |

## Style Calibration Requirements

Patch V1 should not add final features. It should create a comparison setup.

The same clearing should be tested under several lanes:

```text
Calm BOTW-like base
  simple, matte, low-noise, broad color/value regions

Hades-accent lane
  same base plus stronger painted accent lines and contact marks

Hybrid target lane
  restrained base plus selective accent lines/contact response/sparse motifs

Pixel/faceted lane
  current PS3D material-space pixel identity pushed harder
```

The goal is to decide the visible lane before deeper implementation.

The likely target is the hybrid lane.

### Patch V1 Implementation

Patch V1 implements the calibration setup as assets, not as new shader code or runtime systems.

Canonical calibration assets:

```text
Assets/Game/Demo/Profiles/Ground/GSP_StyleCalibration.asset
Assets/Game/Demo/Profiles/Ground/Styles/GSSP_StyleCalibration.asset
```

`GSP_StyleCalibration` is a neutral semantic/mask-generation profile. Its purpose is to keep the generated static masks steady while the visible style variants change. It is not a production biome or final terrain identity.

`GSSP_StyleCalibration` is a temporary development surface family discovered by the existing `GeneratedGround` family dropdown. It contains four comparison variants:

| Variant id | Display name | Purpose |
| --- | --- | --- |
| `calibration.calm_base` | Calm Base | BOTW/TOTK-like restraint test: matte, quiet, broad, low-noise ground. |
| `calibration.hades_accent_proxy` | Hades Accent Proxy | Current-tool approximation of stronger Hades-1-like ground mark rhythm. Uses `DirectionalStreaks` as a proxy, not as the final accent-line implementation. |
| `calibration.hybrid_target_proxy` | Hybrid Target Proxy | Expected target lane: calm base plus restrained accent rhythm. |
| `calibration.pixel_faceted` | Pixel-Faceted | Stress test for the existing PS3D material-space pixel/faceted identity. |

The Hades and Hybrid variants originally used `DirectionalStreaks` as a calibration stand-in. Patch V3 replaces that proxy with real stackable `PaintedAccentLines` recipes while keeping DirectionalStreaks available as a separate surface-mark layer.

Patch V1 does not add:

- `PaintedAccentLines` shader logic;
- contact/edge accent logic;
- sparse motifs;
- shader feature stack migration;
- runtime state;
- footprints, puddles, rain, grass suppression, roads, or wagon tracks;
- scene edits or new components.

The purpose is screenshot comparison from the same clearing and camera, so the next implementation patch is guided by evidence instead of taste drift.


### Patch V2 Implementation

Patch V2 records the first calibration conclusion and retunes production families accordingly.

Calibration conclusion:

```text
Use Calm Base as the foundation.
Keep Hybrid as the target philosophy.
Do not use Pixel-Faceted as the default ground read.
Do not mistake DirectionalStreaks for real Hades-1-like painted accent lines.
```

Implemented adjustments:

- `Pixel-Faceted` is now a flat display label. The stable id remains `calibration.pixel_faceted`.
- Snowfield variants reduce fine pixel noise, patch contrast, warp, and over-strong directional streaks.
- Wet Mudflat variants reduce fine pixel noise, damp darkening, patch contrast, and feature intensity while staying matte.
- `TrampledWear` remains a useful compaction-response proof, but it is not treated as the primary ground-style foundation.

Patch V2 is still not final ground art. It is a base-floor cleanup so later doctrine layers have room to work.

Patch V2 does not add painted accent lines, contact accents, sparse motifs, runtime state, new shader properties, scene edits, river logic, or shader feature stack migration.

### Patch V2B Implementation

Patch V2B adds the missing third production baseline family: `Grassland`.

This is not a vegetation-rendering patch. It does not add grass blades, foliage placement, grass physics, wind animation, density maps, or grass suppression. It adds a calm living-ground surface family so future shared doctrine layers can be tested against three different material/value regimes instead of only snow and mud.

Canonical three-family test set after V2B:

```text
Snowfield
  pale, cold, soft, low-value surface

Wet Mudflat
  dark, earthy, damp, matte surface

Grassland
  muted green/olive, living-ground, medium-value surface
```

Shared ground features must prove themselves across this set before they are treated as part of the baseline visual language. A feature that only works on snow or only works on mud should remain a family-specific response, not a doctrine pillar.

Implemented assets:

```text
Assets/Game/Demo/Profiles/Ground/GSP_Grassland.asset
Assets/Game/Demo/Profiles/Ground/Styles/GSSP_Grassland.asset
```

`GSP_Grassland` is a semantic/mask-generation profile with high vegetation suitability, moderate damp/deposit response, moderate footprint visibility, low snow eligibility, and soft broad patches.

`GSSP_Grassland` is a production visual family with four baseline variants:

| Variant id | Display name | Purpose |
| --- | --- | --- |
| `grassland.clean_meadow` | Clean Meadow | Calm matte meadow baseline. Muted olive-green, low noise, broad soft variation. |
| `grassland.patchy_meadow` | Patchy Meadow | Slightly more exposed-earth/olive patching while staying restrained. |
| `grassland.damp_meadow` | Damp Meadow | Cooler, darker, river-adjacent living ground. Uses a very subtle `PooledWetness` proof response only; not real puddles. |
| `grassland.worn_meadow` | Worn Meadow | Browner, compressed/path-capable meadow ground. Uses a restrained `TrampledWear` proof response so compaction masks can be tested on grassland. |

Patch V2B deliberately keeps `Style Calibration` as a development-only comparison family. It does not convert calibration into production grassland. Grassland is a real family; Style Calibration remains a temporary visual lane tester.

Patch V2B does not add painted accent lines, contact accents, sparse motifs, shader feature stack migration, runtime state, vegetation rendering, scene edits, river logic, new shader properties, or new components.

### Patch V3 Implementation

Patch V3 implements `Shader Feature Stack + Painted Accent Lines`. Patch V3A fixes shader include isolation after the first V3 compile issue. Patch V3B moves ground debug selection onto the `GeneratedGround` component so validation no longer requires opening shared material assets. Patch V3C cleans up that object-level debug UX by removing slash characters from debug labels and removing the obsolete Unity 6.5 editor-refresh overload. Patch V3D refines the raw Painted Accent Lines mask after validation showed the first line generator produced large isolated strips rather than small broken ground creases. Patch V3E then replaces the remaining straight/bar-like primitive with curved visual-relief terrain-fold strokes. Patch V3F exposes the three painted-accent channels separately and strengthens the final side-dependent value relief. Patch V3F.1 makes the relief body more continuous and the signed-side channel readable, but validation shows the model still reads as curve tubes and side rails. Patch V3G retires the curve-distance stroke model as the chosen direction and redirects the feature toward generated visual fold-field data.

Technical contract:

```text
GroundSurfaceVariantRecipe.features
  list of feature recipes

GeneratedGround
  resolves first enabled ShaderOnly recipe of each supported kind
  writes explicit MaterialPropertyBlock properties per feature kind

Shader
  evaluates DirectionalStreaks, PooledWetness, TrampledWear, and PaintedAccentLines as stackable layers
```

The old generic `_GroundFeatureMode` slot is now a deprecated compatibility property. It must not receive new modes.

`PaintedAccentLines` is the first Hades-1-like doctrine layer, but its V3D-V3F.1 curve-distance implementation is retired as the final visual model. That implementation remains in code temporarily as a fallback/comparison path only. It must not be tuned, extended, or used as the basis for family polish.

Retired V3D-V3F.1 model:

```text
world-space procedural curve strokes
  -> distance-to-curve line mask
  -> inflated distance-to-curve relief body
  -> side bands derived from curve side
```

Reason for retirement:

```text
It produces scratches, fat tubes, and rail-like signed-side bands. The target requires an underlying visual fold/height field whose selected edges produce accent lines.
```

Accepted V3G direction:

```text
generated visual fold field F(x,z)
  -> relief/body channel from F
  -> precomputed selected crest line from thresholded peak regions
  -> signed side from fold-field gradient/polarity
```

The useful parts of V3 are retained: `PaintedAccentLines` feature kind, feature-stack authoring, material-property plumbing, object-level `GeneratedGround` debug selection, and the three-channel debug contract. The implementation source changes from direct shader curve strokes to generated fold-field data. The layer remains visual-only: no mesh displacement, no collision change, no terrain height edit, no runtime footprints/wetness state, and no decal system. Generated/cached texture data is allowed for the fold field if it is produced at ground regeneration/dirty time and sampled cheaply at runtime.

Canonical validation set after V3:

```text
Snowfield
Wet Mudflat
Grassland
```

Each shared feature must be judged across all three before it is accepted as part of the baseline ground language.

Patch V3G fold-field validation rule for Painted Accent Lines:

```text
Generated field debug first, extracted line second, final color last.
```

`Ground Painted Accent Lines` debug should show the selected crest/accent strokes extracted from thresholded peak regions of the fold field. `Ground Painted Accent Relief` should show the underlying visual fold-height/body field, not a widened line tube. `Ground Painted Accent Signed Relief` should show gradient/polarity information that can drive shadow/highlight side selection, not decorative parallel rails. The selected line should not show straight bars, giant crescent strips, continuous worms, full-screen hatching, dense hair-like noise, crack networks, many contour rings around one bump, or full closed outlines around every bump. Normal rendering should eventually read as subtle visual mound/crease relief through painted shadow/highlight or, if later accepted, a tiny shader-only normal cue. Final family tuning must wait until the fold field, selected line, and signed side read are all directionally correct.


### 2026-07-09 — Patch V3G: Painted Accent Direction Reset / Fold-Field Plan

Patch V3G retires the V3D-V3F.1 curve-distance stroke model as the final Painted Accent Lines direction. The previous path proved the feature stack, object-level debug workflow, shader property plumbing, and the three-channel diagnostic contract, but validation showed that the source representation is wrong: it starts from a curve, inflates the curve into a tube-like body, and derives rail-like side bands.

The chosen direction is now a generated visual fold field. The ground generator will eventually create persistent/cached visual fold data at regeneration or dirty time. The shader will sample that generated data and use it to render accent lines and visual relief. The expected channel meaning is:

```text
line contour
  selected contour/ridge/edge strokes extracted from the fold field

relief body
  underlying visual fold-height/body influence

signed relief side
  gradient/polarity field for painted shadow/highlight side selection
```

The old shader curve-stroke code remains temporarily as a runtime fallback/comparison path until the fold-field replacement is implemented. It is explicitly retired and must not be tuned as the final solution.

### 2026-07-09 — Patch V3E: Painted Accent Lines Curved Relief Model

V3E redefines the active Painted Accent Lines primitive as visual terrain-fold strokes, not 2D line stamps. The shader now builds each stroke from several local control points to produce irregular curved marks, then derives both the line mask and a soft signed relief body from that same curve. The relief is used only for subtle painted value shaping: it is not mesh displacement, collision, terrain height, decals, textures, runtime state, or generated atlas data.

### 2026-07-09 — Patch V3F: Painted Accent Relief Debug + Visual Relief Strengthening

V3F separates the Painted Accent Lines visual model into three debuggable channels:

```text
line contour
  thin dark/painted crease

relief body
  broader soft fold area around the contour

signed relief side
  side-dependent field used for painted shadow/highlight
```

The object-level ground debug dropdown now exposes `Ground Painted Accent Relief` and `Ground Painted Accent Signed Relief` in addition to `Ground Painted Accent Lines`. Normal rendering uses the narrow contour for crease/tint response and the signed side field for stronger value-side shadow/highlight. This remains visual-only: no mesh deformation, collision change, decals, textures, generated atlases, runtime state, new mesh channels, or new components are introduced.

## Acceptance Criteria

Ground work is successful when:

- a quiet ground area still looks intentional, not empty;
- broad patches read from the game camera;
- accent lines feel authored, not procedurally sprayed;
- contact accents make rocks/rivers/paths feel integrated;
- variants feel related by one visual language;
- snow, mud, and grassland differ through tuning, not unrelated pipelines;
- the ground does not fight characters, VFX, hazards, or UI;
- debug masks correspond to visible responses;
- feature cost remains opt-in and understandable;
- screenshots make the style direction easier to choose, not harder.

Ground work is failing when:

- visual richness comes mostly from noise;
- every surface has equal detail density;
- the shader becomes a list of hardcoded family branches;
- feature work cannot explain which doctrine layer it improves;
- a quiet area looks unfinished;
- a detailed area looks like texture soup;
- runtime simulation is used to hide an unresolved static style.

## Authoring Workflow Target

The authoring flow should remain object/profile driven:

```text
Select GeneratedGround
→ choose Surface Family
→ choose Surface Variant
→ optionally override material/style controls
→ place GroundModifiers for semantic/height intent
→ regenerate
→ inspect debug masks
→ inspect final ground
```

Style-profile authoring should remain asset-backed:

```text
Open GroundSurfaceStyleProfile
→ inspect variants
→ tune material controls
→ tune feature-layer recipes
→ apply to open generated grounds
```

The UI should expose design concepts, not low-level noise clutter.

## Debug and Validation Rules

Ground debugging should remain compact, trustworthy, and object-owned.

Normal validation path:

```text
Select GeneratedGround
→ Ground Debug
→ Debug View
→ choose the needed ground debug view
```

GeneratedGround writes the selected debug mode through its renderer-local `MaterialPropertyBlock` using `_MaskDebugMode`. Authors should not need to open or edit shared material assets to validate generated-ground masks or doctrine-layer debug views. Material asset debug controls are fallback/internal only.

Ground debug changes are visual/material-property-block changes only. They must not regenerate terrain, change mesh data, instantiate materials, or require a style/profile asset edit. River corridor renderers may receive the same parent-ground debug view through the existing ground material-property refresh path, but river code must remain style-agnostic.

Debug view labels must avoid slash characters because Unity enum dropdowns treat slashes as submenu separators. Use flat labels such as `Ground Compaction Path`, `Ground Damp Deposit`, and `Ground Rocky Dry`.

Required mask/debug concepts:

- tonal patch;
- exposure/snow-hold;
- damp deposit;
- vegetation suitability;
- compaction path;
- shore;
- rocky dry;
- standing-water/puddle potential;
- painted accent lines;
- combined ground mask where useful.

Validation should always distinguish:

```text
mask exists and is correct
shader interprets mask correctly
style tuning looks good
```

Do not diagnose visual tuning before confirming the mask path.

## Runtime State Contract - Deferred

Runtime state remains a future layer. The current likely channel contract is:

```text
R = wetness
G = snow depth / snow amount
B = compression / footprint / trample
A = mud / standing water / disturbance age
```

This contract is not active implementation priority. It should be revisited after the static stack is accepted.

## Future Family Examples

Future ground families should be added through profiles/style assets and should obey the same doctrine.

Possible families:

- Rocky Scrub;
- Frozen Dirt;
- Ash Field;
- Corrupted Ground;
- Worn Road;
- Snow-over-Mud blend;
- Riverbank/Silt;
- Sacred/Ritual Ground.

Each family should tune:

- calm base material;
- macro patch behavior;
- accent-line behavior;
- contact/edge response;
- sparse motif identity;
- semantic mask interpretation;
- runtime response later.

## Maintenance Rules

1. This document owns generated-ground visual doctrine.
2. The implementation plan records patch history and concrete work sequencing.
3. Generic visual-language docs may summarize this doctrine but should link here instead of duplicating every detail.
4. When implementation changes a channel contract, feature-layer meaning, or roadmap priority, update this document and the implementation plan together.
5. Do not allow examples to become accidental requirements. Mark them as examples if they are not committed.
6. Do not introduce a new ground feature unless it identifies its doctrine layer.
7. Do not resume runtime surface-state work until the static style calibration is accepted or the pause is explicitly lifted.

## Final Baseline Statement

Generated ground should be a restrained stylized stage: simple enough to preserve readability, rich enough to feel designed, and structured enough that every future family can share one visual language.

The base is calm. The interest comes from broad patches, meaningful masks, selective painted accents, contact response, sparse motifs, and later runtime state.

That is the ground baseline until deliberately changed.
