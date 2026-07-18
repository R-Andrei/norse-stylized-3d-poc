# Ground V4 Contact and Edge Accent Audit and Architecture

## 2026-07-19 — GSU-M2.0 relationship

GSU-M2.0 changes reusable substrate rendering only. Its optional authored-colour texture, packed normal/cavity/roughness data, and worn visual edges are material identity; they do not create V4 Contact/Edge Accent sources or coverage. River banks and riverbeds remain excluded from V4, and V4 implementation remains queued after Ground material acceptance.


## Status

**Audit state: corrected and accepted. Implementation has not started.**

This is the canonical V4 Contact / Edge Accent record for GeneratedMass grounding and explicitly participating GroundModifier boundaries. Its earlier River-bank source architecture is superseded by `Ground_River_Coupled_Surface_Response_Architecture.md`. River banks and riverbeds are never rasterized into the V4 Contact field, never appear in V4 source diagnostics, and never invalidate V4 coverage.

V3M, V3R, and the V3S-A4B.3 River-coupled baseline are accepted. GSU-M1 is implemented and source-audited through GSU-M1.9A.5. M1.9A through M1.9A.4 are visually superseded; M1.9A.5 is the active source-art worn-edge Fine Gravel A/B evaluation and Unity comparison remains pending. GSU-M1.7 supplies generic shared/application authoring, GSU-M1.7.1 repairs its editor-only compile blocker, and GSU-M1.8 retains the accepted 256 runtime tier. M1.9A.3 changes no Contact/Edge Accent source, field, shader, renderer, or geometry. Gameplay visual/performance acceptance, sequential material expansion, and family acceptance remain before V4 begins.

The latest project source overrides this document whenever they conflict.

## Actual Ground mission

The mission is to build a complete restrained-stylized terrain system, not merely a procedural heightfield and not merely a Painted Accent generator.

The intended final static visual stack is:

```text
playable terrain shape
→ calm family/variant base material
→ broad macro patch composition
→ semantic surface-mask response
→ Painted Accent lines
→ River-Coupled Ground Response
→ Contact / Edge Accents
→ sparse motifs and stamps
→ runtime surface state later
```

Ground must visually connect GeneratedMass objects and explicitly selected modifier boundaries to the terrain. River banks and riverbeds are handled by the direct River-Coupled Ground Response and are outside this generated-field architecture.

The next milestone therefore targets localized Ground-side response around meaningful boundaries and contacts while preserving these constraints:

- no per-frame field generation;
- no runtime contour solving or texture upload;
- no extra scene objects, child renderers, decals, or mesh strips as the production representation;
- no automatic scan of every renderer or collider in the scene;
- no Ground geometry or collider change merely to produce a visual accent;
- no restoration of the retired raised Painted Accent ridge path;
- authoring remains centralized in `GeneratedGround`;
- Player rendering uses persistent production data only.

## Required visual behavior

Contact / Edge Accents are not universal outlines.

They should be:

- Ground-side rather than object-side;
- local to meaningful contact or boundary zones;
- broad enough to remain stable from the isometric camera;
- restrained in opacity and contrast;
- broken and irregular rather than mechanically perfect;
- family/variant controlled;
- semantically responsive to snow, dampness, compaction, standing-water potential, exposure, and dry/rocky ground;
- capable of grounding a rock or standing mass without drawing a complete cartoon ring;
- capable of defining path or modifier edges without becoming a hard road decal.

Possible responses include restrained darkening, damp/deposit tinting, snow compression or clearing, low-value rim emphasis, and limited local bias of other static features. The first implementation should not couple Painted Accent placement to contact fields; that interaction can be evaluated only after the contact layer works independently.

# Source audit

## 1. Existing Ground semantic channels are useful but too coarse to own contact geometry

`GroundGenerator` currently writes eight static semantic channels into the Ground mesh:

```text
Vertex Color R = tonal variation
Vertex Color G = exposure
Vertex Color B = damp/deposit
Vertex Color A = vegetation suitability
UV2 X = compaction/path/flatten influence
UV2 Y = reserved zero on ordinary Ground
UV2 Z = rocky/dry secondary patch
UV2 W = standing-water/puddle potential
```

Evidence:

```text
Assets/Game/Procedural/Ground/GroundGenerator.cs:1024-1034
Assets/Game/Procedural/Ground/GroundHeightFieldSnapshot.cs:180-317
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl:18-56
```

These ordinary-Ground masks are produced at Ground mesh-vertex resolution and interpolated by the shader. River shore/waterline data is not part of this ordinary-Ground contract; the dedicated corridor publishes its own `UV2.y` value. A Standard 40 m Ground at Medium 33 resolution has approximately 1.25 m vertex spacing. This is appropriate for broad semantic response but not for a stable 0.05-0.50 m contact rim.

**Finding:** existing masks should classify and style a high-resolution contact field, but they should not be the sole geometric source of that field.

**Confidence:** proven, very high.

## 2. River shore and bed response are explicitly outside V4

The River corridor exposes exact Riverbed Support in Unity UV channel index `3` / HLSL `TEXCOORD3.x`, while corridor `UV2.y` carries shore/waterline influence. Ordinary Ground writes zero to `UV2.y` and has no UV3 River stream. The Ground shader consumes River channels only when the renderer property block declares the `RiverCorridor` role. No generated Contact texture is required for River response.

**Finding:** V4 must not read `StylizedRiverGroundSnapshot`, derive bank polylines, rasterize River contribution, or use River changes in its coverage signature. River-coupled appearance is authoritative in `Ground_River_Coupled_Surface_Response_Architecture.md`.

**Confidence:** accepted architecture and implemented River channel contract; very high.

## 3. GroundModifier already contains exact analytical boundary input

Each `GroundModifierSnapshot` contains:

```text
shape: circle or oriented box
centre
right/forward basis
circle radius or box size
blend distance
height mode and strength
authored compaction/damp/standing-water strengths
feature exclusions
```

`EvaluateWeight` already calculates the modifier's shape distance and blend falloff, but the signed shape-distance helpers are private and the modifier has no Contact / Edge Accent controls.

Evidence:

```text
Assets/Game/Procedural/Ground/GroundModifier.cs:6-120
Assets/Game/Procedural/Ground/GroundModifier.cs:420-528
```

**Finding:** modifier boundaries can be generated analytically and deterministically without sampling the coarse compaction field. A small snapshot API extension can expose signed distance to the full-strength shape boundary while retaining the existing weight contract.

**Confidence:** proven, very high.

## 4. GeneratedMass can be discovered automatically through existing generated-geometry infrastructure

`GeneratedMass` is currently the only implementation of `IGeneratedGeometrySource`. It provides:

```text
GeometryChanged event
final GeometryMeshFilter
IsSolidGeometry = true
IsStaticGeometry = true
exact stable world-geometry fingerprint support
```

It registers with `GeneratedGeometryRegistry`, which provides source-added, source-removed, and source-changed events.

Evidence:

```text
Assets/Game/Procedural/Core/IGeneratedGeometrySource.cs
Assets/Game/Procedural/Core/GeneratedGeometryRegistry.cs
Assets/Game/Procedural/Masses/GeneratedMass.cs:344-351
Assets/Game/Procedural/Masses/GeneratedMass.cs:994-1005
Assets/Game/Procedural/Masses/GeneratedMass.cs:1354-1386
```

The River disturbance system already performs event-time footprint extraction from final generated meshes and caches the result. That code proves that generated geometry can be reduced to a bounded, deterministic contour without per-frame triangle work.

Evidence:

```text
Assets/Game/Procedural/Rivers/RiverDisturbanceFootprintResolver.cs
Assets/Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.GeneratedSources.cs
```

**Finding:** V4 should use the registry rather than scanning every scene renderer. GeneratedMass movement or regeneration can invalidate only the Contact Accent stage through existing geometry events.

**Confidence:** proven for discovery and invalidation; high-confidence inference for adapting final-mesh contour extraction to a Ground contact plane.

## 5. Ordinary structures and arbitrary props have no current Ground-contact contract

Blockout structures and ordinary MeshFilter objects do not implement `IGeneratedGeometrySource`, and `GeneratedGround` has no explicit manual contact-source component or serialized source list.

**Finding:** the first V4 implementation can legitimately cover:

```text
GeneratedMass contacts
explicit GroundModifier/path boundaries
```

A later explicit opt-in source is required for huts, bridge supports, ordinary props, or non-generated meshes. The system must not scan all renderers or colliders automatically.

Recommended later contract:

```text
GroundContactAccentSource
```

or an equivalent explicit interface/component that exposes an authored footprint mode and participation settings. Adding that component requires separate approval and is not part of the first proof patch.

**Confidence:** proven, very high.

## 6. The feature stack has a Generated Texture class but no Contact Accent feature kind

`GroundSurfaceFeatureCostClass` already defines:

```text
Shader Only
Mesh Mask Driven
Generated Texture
Runtime State
```

Evidence:

```text
Assets/Game/Procedural/Ground/GroundSurfaceFeatureCostClass.cs
```

The active shader resolver currently supports only Directional Streaks, Pooled Wetness, Painted Accent Lines, and Trampled Wear. No Contact / Edge Accent kind exists.

Evidence:

```text
Assets/Game/Procedural/Ground/GroundSurfaceFeatureKind.cs
Assets/Game/Procedural/Ground/GroundSurfaceVariantRecipe.cs
```

**Finding:** V4 should be a new `GroundSurfaceFeatureKind` using the existing `GeneratedTexture` cost class. It must not be disguised as Trampled Wear, Pooled Wetness, Frosted Rock Dust, or another reserved feature.

**Confidence:** proven, very high.

## 7. The Painted Accent production lifecycle is reusable as a contract, not as a geometry algorithm

The accepted Painted Accent system already proves:

- live Edit Mode procedural preview;
- persistent automatically owned R8 output;
- baked-only Play Mode and Player rendering;
- exact stale detection;
- hard build enforcement;
- project-wide generated-asset audit and confirmed-orphan cleanup.

Its SurfaceStroke, projected-glyph, cluster, and polyline algorithms are specific to Painted Accents and should not be reused for contact boundaries.

**Finding:** V4 should reuse and generalize the production lifecycle, validation, ownership, and cleanup patterns while implementing a separate boundary-source and rasterization pipeline.

**Confidence:** proven, very high.

# Representation options

## Option A — shader derivatives of existing vertex masks

Example:

```text
edge = gradient(compaction or shore)
```

Advantages:

- no generated texture;
- no bake lifecycle;
- one cheap shader operation.

Failures:

- tied to Ground mesh resolution;
- broad and unstable at Medium 33;
- cannot represent GeneratedMass contacts;
- cannot reliably distinguish core boundary from blend boundary;
- changes with triangulation/interpolation;
- produces a technical mask edge rather than a physical contact boundary.

**Decision:** rejected as the production representation. It may be useful as a temporary comparison/debug view only.

## Option B — per-source decals, mesh strips, or child renderers

Advantages:

- direct visual placement;
- source-specific materials.

Failures:

- extra objects and renderers;
- ordering, z-fighting, and lifecycle problems;
- poor fit with the accepted mesh-free Ground feature architecture;
- runtime object overhead;
- duplicates the retired Painted Accent representation mistakes.

**Decision:** rejected.

## Option C — one high-resolution unified R8 Contact Accent field

Pipeline:

```text
modifier/path boundaries
+ GeneratedMass contact contours
→ deterministic coverage raster
→ one Ground-local R8 field
→ shader combines coverage with existing semantic masks
```

Advantages:

- one additional texture sample;
- low memory;
- supports all first-phase source types;
- source geometry is resolved offline/editor-time;
- existing compaction, damp, standing-water, exposure, and rocky/dry masks can style the same field differently;
- simple persistent production lifecycle;
- exact source geometry need not exist in Player.

Limitation:

- source identity is not retained when multiple source classes overlap. Visual differentiation must come from the broad semantic context and source-weighted coverage written at bake time.

**Decision:** accepted as the recommended first production architecture.

## Option D — multi-channel generated Ground control texture

Possible channel contract:

```text
R = Painted Accent coverage
G = Contact / Edge Accent coverage
B/A = later static generated layers
```

Advantages:

- one texture binding and sample;
- explicit future packing contract.

Failures now:

- converting the accepted R8 Painted Accent asset to RGBA32 at 2048² increases raw storage from approximately 4 MiB to approximately 16 MiB per Ground;
- changes the already validated PA-B1–PA-B4 asset, signature, runtime, validation, and cleanup contracts;
- reserves memory before other channels are proven necessary.

**Decision:** deferred. V4 should not reopen the accepted Painted Accent production format merely to achieve theoretical packing. Separate R8 outputs are preferable until profiling proves packing is beneficial.

# Recommended V4 architecture

## Authoritative pipeline

```text
Ground base geometry and immutable GroundHeightFieldSnapshot
+ current explicitly participating GroundModifierSnapshot set
+ current eligible GeneratedGeometryRegistry sources
→ deterministic Contact Accent source snapshots
→ source-specific boundary/footprint resolution
→ unified Ground-local contact coverage raster
→ live Edit Mode R8 preview
→ persistent production R8 output
→ baked-only Play Mode and Player shader sampling
```

## Source snapshots

### River exclusion

River banks and riverbeds are not Contact sources. V3S interprets the corridor's shore/waterline and Riverbed Support channels directly in the Ground shader only on `RiverCorridor` role draws. Ordinary Ground publishes zero River shore data and no UV3 River stream. V4 collects no River snapshot and stores no River contribution.

### Modifier/path source

Use the modifier's exact circle or oriented-box boundary. The first implementation should target the full-strength core boundary, not the outer end of Blend Distance, because the core boundary represents the authored path/pad/region edge while the blend is a falloff implementation detail.

Future authoring may add:

```text
Contact Accent Participation
Boundary Target: Core / Blend / Both
Boundary Strength
```

These should remain on the modifier because they describe source participation, while visual style remains on the Ground variant feature recipe.

### GeneratedMass source

Use only active, solid, static generated geometry whose world bounds overlap the Ground domain.

Recommended contour resolution:

1. transform final mesh vertices into Ground local space;
2. sample Ground height at projected XZ positions;
3. retain lower/contact-band vertices and triangle-edge crossing candidates;
4. construct a deterministic 2D convex contact hull;
5. fall back to a conservative projected-bounds contour only when readable contact geometry is unavailable;
6. report the fallback explicitly.

GeneratedMass is convex-oriented geometry, so a convex hull is a suitable first contact representation. Concave arbitrary structures are deferred to the explicit manual-source contract.

## Coverage combination

Each source writes only its authored/source-weighted band into the same scalar field. Overlap combines by maximum, not addition, preventing intersections from becoming overbright.

The baker should support:

```text
inner width
outer width
edge softness
controlled breakup
source strength
```

The first production field should not encode dynamic wetness, snow depth, footprints, or time-varying disturbance.

## Shader response

The shader reads:

```text
contactCoverage
compaction
damp/deposit
standing-water potential
exposure
rocky/dry
```

Suggested response selection:

```text
damp or standing-water context
→ damp/deposit darkening and tint

compaction/path context
→ worn edge darkening and modest vegetation suppression in future systems

neither damp/standing-water nor compaction
→ object-contact grounding response

high exposure / snow context
→ restrained snow compression or cool shadowed rim rather than wet mud tint
```

The first patch should use existing family material colours—especially Damp Tint and base colour—rather than add a new arbitrary colour for every source class. A dedicated Contact Tint should be added only if the three production families cannot be tuned acceptably through existing palette controls.

## Resolution and memory

Contact Accents are broader than Painted Accent strokes. The first visual proof should compare:

```text
1024² R8 on Standard 40 m Ground
2048² R8 on Standard 40 m Ground
```

At 1024², one R8 field is approximately 1 MiB and one texel covers approximately 0.039 m on a 40 m patch. This is likely sufficient for 0.10-0.50 m contact bands, but the value is a validation recommendation, not an accepted default.

The final resolution rule should target stable world texel size and clamp to a bounded power-of-two range rather than blindly match Painted Accent resolution.

## Authoring ownership

Add a dedicated variant feature:

```text
Contact / Edge Accents
Cost Class: Generated Texture
```

Recommended first authoring controls:

```text
Enable Contact / Edge Accents
Overall Intensity
Band Width
Edge Softness
Breakup
Object Contact Influence
Modifier / Path Boundary Influence
Pattern Seed Offset
```

Source participation controls belong to the source when they are source-specific. Shared visual response belongs to the selected Ground variant.

`GeneratedGround` remains the unified authoring façade and should expose the resolved controls inline.

## Production workflow

V4 should generalize the existing feature-specific production action into:

```text
Bake Ground Surface Outputs
```

The action should bake every enabled static generated-texture Ground feature that is stale. The current `Bake Painted Accents` action may remain temporarily as a compatibility shortcut during migration, but the final ordinary workflow should not require one button per generated layer.

Production maintenance should similarly become Ground-output based rather than creating a separate audit window and build validator for every future generated feature.

No scene or prefab may be saved automatically.

## Runtime contract

Player runtime performs only:

```text
validate persistent Contact Accent texture structurally
bind texture and stored Ground-local mapping
sample it in the Ground shader
```

Player runtime must not:

```text
scan GeneratedGeometryRegistry
read source meshes
resolve mass contours
sample modifiers for Contact Accents
rasterize or upload Contact Accent coverage
silently fall back to procedural generation
```

## Invalidation contract

### Material-only

These should update only Ground material properties:

```text
Overall visible intensity
palette/tint response
normal debug view
```

### Contact coverage stale

These should invalidate only the Contact Accent source/coverage stages and their persistent bake:

```text
Contact band width, softness, breakup, or source weights
GroundModifier shape, transform, blend, or participation
GeneratedMass final geometry or transform
Ground shape changes that alter mass-to-ground contact
Ground patch size or mapping
Contact baker revision or resolution rule
```

### Unrelated

Painted Accent-only changes must not rebuild Contact Accent coverage. Contact Accent-only changes must not rebuild SurfaceStrokes, ProjectedGlyphs, companion clusters, or Painted Accent coverage.

## Event and ordering contract

- Existing GroundModifier notifications already reach `GeneratedGround`.
- V4 should subscribe to `GeneratedGeometryRegistry.SourceAdded`, `SourceRemoved`, and `SourceChanged` while enabled in Edit Mode.
- Registry events mark Contact Accent sources stale; they must not force immediate full Ground geometry regeneration.
- Source collection must be sorted deterministically before rasterization because the registry uses a `HashSet`.
- Max-combination makes coverage order-independent, but deterministic ordering is still required for signatures, diagnostics, and reproducible failure evidence.

## Diagnostics

The first implementation should report one compact record per Contact Accent build:

```text
eligible / rejected modifier sources
eligible / rejected GeneratedMass sources
exact / fallback mass contours
boundary segment or contour-point count
coverage resolution and world texel size
covered texels and fraction
source preparation time
raster time
upload time
current/live/baked status
```

Debug views should include:

```text
Raw Contact Coverage
Modifier Boundary Contribution
GeneratedMass Contact Contribution
Final Lit Contact Response
```

Per-source success logs are forbidden. Failure evidence should be capped to a few representative unique cases.

# Rejected shortcuts

- Use current semantic vertex masks as the complete production contact geometry.
- Add a dark outline around every object.
- Scan every `MeshRenderer`, `MeshFilter`, or `Collider` in the scene.
- Spawn decals, child meshes, or per-source renderers.
- Route Contact Accents through Painted Accent SurfaceStrokes or projected glyphs.
- Bake dynamic footprints, rain, snow depth, or runtime disturbance into this static field.
- Automatically delete or rewrite existing Painted Accent production assets during V4 migration.
- Add per-frame registry scans or full-field rebuilds.

# Implementation phases

## V4-A1 — Source contract and raw coverage proof

Purpose: prove input correctness before artistic composition.

Scope:

- add the Contact / Edge Accent feature kind and Generated Texture classification;
- collect exact explicit modifier and GeneratedMass source snapshots;
- add deterministic mass contact-contour extraction with explicit bounds fallback;
- rasterize a transient Edit Mode R8 field;
- add source-isolated and raw-coverage debug views;
- add compact diagnostics and signatures;
- no normal lit visual response;
- no persistent production output yet;
- no runtime work.

Acceptance:

1. modifier core boundaries align with circle and oriented-box shapes;
2. GeneratedMass contacts align with the actual lower footprint rather than a full renderer rectangle;
3. moving or regenerating a GeneratedMass invalidates only the Contact Accent field;
4. River structural or material changes do not stale Contact coverage;
5. unchanged regeneration is a cache hit;
6. no Painted Accent counters, output, or production asset changes.

## V4-A2 — Visual response and authoring

- add the normal lit shader composition;
- expose variant controls through GeneratedGround;
- tune Snowfield, Grassland, and Wet Mudflat comparison cases;
- prove contact response is restrained and source/context aware;
- keep source debug views.

## V4-A3 — Persistent production integration

- generalize Ground generated-output baking;
- persist Contact Accent R8 coverage;
- use baked-only Play Mode and Player rendering;
- extend build validation and generated-asset cleanup;
- migrate ordinary authoring toward **Bake Ground Surface Outputs**.

## V4-A4 — Explicit manual structure source, only if required

After GeneratedMass and modifier coverage is visually accepted, evaluate an explicit opt-in source for huts, bridge supports, ordinary props, and non-generated meshes. Do not add this component pre-emptively.

# Methods-tried ledger

## Accepted

- Existing semantic masks as visual context, not precise contact geometry.
- River exclusion from the Contact field; V3S owns shore and bed interpretation.
- Analytical GroundModifier boundaries.
- GeneratedGeometryRegistry for automatic GeneratedMass discovery and invalidation.
- Final-mesh, editor-time GeneratedMass footprint extraction.
- One unified scalar R8 Contact Accent field for the first production architecture.
- Separate static generated output from dynamic runtime surface state.
- Reuse/generalize Painted Accent production lifecycle and cleanup contracts.

## Rejected

- Mesh-resolution derivative edges as production contact geometry.
- Scene-wide arbitrary renderer/collider scanning.
- per-source decals, child meshes, or extra renderers.
- object-outline styling.
- routing the feature through the Painted Accent glyph generator.
- reopening Painted Accent R8 packing before V4 memory and sampling are measured.
- runtime source scans, contour extraction, rasterization, or fallback generation.

## Deferred

- explicit manual contact-source component for ordinary structures;
- packed multi-channel Ground control texture;
- Contact Accent influence on Painted Accent placement;
- dynamic snow compression, footprints, wetness, or disturbance;
- concave structure footprints and multi-material source categories.

# Next work items

1. Preserve V4 as GeneratedMass plus explicit GroundModifier coverage while V3S is active.
2. Resume V4-A1 only after GSU-M1 reusable-material acceptance, including GSU-M1.9A.3 Fine Gravel candidate selection and gameplay acceptance and the subsequent family-acceptance gate.
3. Validate source alignment, River independence, and invalidation before introducing the normal lit visual effect.

## GSU-M1.9A.1 — Fine Gravel Packed-Source A/B Evaluation — visually rejected; historical

**Status:** Rejected by Unity evidence; no longer actionable.

This temporary test installed `Fine Gravel A - Direct Normal` and `Fine Gravel B - Strong Form`, both produced from image-generated normal-style candidates. Neither candidate had genuinely periodic edge neighbourhoods, and their RGB fields did not constitute coherent packed slope data. Unity exposed visible repeat bands, malformed relief, flattening, and generally inadequate stone form. Do not validate, tune, or promote either A1 payload. GSU-M1.9A.3 overwrites those temporary payloads while retaining their serialized GUID/stable-ID plumbing only for safe migration.

## GSU-M1.9A.3 — Source-Preserved Integrable Stone Form — visually rejected; historical

**Status:** Superseded by GSU-M1.9A.4 after Unity exposed macro size segregation and insufficient contour definition.

GSU-M1.9A.1 is visually rejected. Its image-generated candidates were neither genuinely seamless nor valid coherent packed slope fields. GSU-M1.9A.2 remained an offline deterministic investigation only and proved periodic conversion, but its distance-cap reconstruction concentrated useful slope near stone rims and left large interiors too uniform. GSU-M1.9A.3 replaced the two temporary A/B texel payloads while deliberately retaining their existing GUIDs, library stable IDs, importer settings, and Ground-layer references so an installed A1 evaluation is upgraded without orphaning serialized selections. The legacy temporary filenames and stable IDs are cleanup debt only; they must be deleted when a winner replaces canonical `fine-gravel`.

The two historical A3 candidates were rebuilt deterministically from the user-supplied rounded-stone source rather than generated as final textures:

- **`Fine Gravel A3 - Source Preserved`** keeps restrained relief while preserving source-derived silhouettes, size distribution, neutralized internal stone-body cues, localized crowns, irregular shoulders, and hierarchical crevices.
- **`Fine Gravel A3 - Vertical Form`** uses the identical periodic stone layout, B cavity, and A variation, but increases coherent body slope and localized crown amplitude. It was the leading A3 candidate for stronger roundness and verticality before Unity rejected the shared layout and contour treatment.

The non-periodic source boundaries are moved to the centre one axis at a time; only stones intersecting each centre repair band are removed and repacked from extracted source silhouettes on a toroidal 1024² authoring canvas. This preserves most of the supplied layout while making opposite edge neighbourhoods continuous. Each stone uses a continuous side profile plus one or two localized crowns, source-body variation with its directional plane removed, and restrained microstructure. Broad whole-stone white plateaus are prohibited. The final 256² R/G channels are derived from one periodic height field, so the slopes are internally coherent and integrable; B contains a soft contact shoulder and narrower deep gap core; A contains non-directional per-stone and internal form variation.

Offline validation includes 3×3 shader-reference tiling, 256/128/64/32 mip tests, numerical wrap-to-adjacent ratios, per-stone height-distribution evidence, and a CPU reference that reproduces the current packed-detail decode, palette, cavity bands, flat-ground normal perturbation, and material values. The reference uses a simplified ambient/diffuse lighting term and is not claimed to reproduce the complete URP pass. Unity production-camera rendering remains authoritative.

Runtime architecture and cost do not change: three temporary 256² RGBA32 mipmapped slices remain during evaluation, only the selected substrate slice is sampled, and there is no new shader sample, ALU branch, draw call, renderer, mesh data, River data, or runtime CPU process. No C#, HLSL, ShaderLab, River source, scene, prefab, canonical Fine Gravel assignment, or unrelated material changes in this patch.

**Unity gate:** rebuild `SSDL_DefaultSurfaceDetails`, historically required comparison of the two A3 choices with identical shared/application values from the same close and production cameras, include dry and wet views, and judge body roundness, internal variation, coherent common light direction, cavity width, repetition, seam visibility, and mip survival. Select a winner or reject both; do not tune the shader to hide a deficient packed source.

## GSU-M1.9A.4 — Balanced Toroidal Mix and Hard Rock Contour

**Status:** Visually superseded by GSU-M1.9A.5 after Unity exposed persistent macro cross bias, excessive micro-fillers, and insufficient authored worn-edge definition.

GSU-M1.9A.3 is visually rejected. Its source-preserved reconstruction improved interior verticality, but Unity evidence exposed two remaining packed-source defects: the repaired source layout segregated large and small stones into repeatable macro regions that formed visible cross/square patterns when tiled, and the stone-to-gap transition remained too gradual, causing individual forms to read as soft dirt mounds rather than hard rocks. A4 replaces only the two temporary A/B packed payloads while retaining their GUIDs, stable IDs, importer settings, Ground adapters, and serialized selections. The temporary legacy filenames remain cleanup debt until one candidate replaces canonical `fine-gravel`.

A4 starts from the coherent periodic A3 vertical height/form data and applies only deterministic, periodic operations. Two independently phase-warped copies of the same source layout are combined without alpha-blended ghosting: the second copy contributes only substantial stone bodies inside genuinely low regions of the first. This breaks the previous tile-axis size bands and interleaves large, medium, and small forms more chaotically while preserving a single coherent height field. Both active candidates use exactly the same redistributed layout, coverage, cavity topology, and non-directional form variation:

- **`Fine Gravel A4 - Balanced Mix`** uses a moderately compressed contact wall and restrained slope amplitude.
- **`Fine Gravel A4 - Hard Rock Contour`** uses a narrower contact wall, stronger edge-normal energy, stronger stone-side cavity shoulder, and stronger neutral edge/body separation. It is the leading candidate for the requested hard-rock delimitation, but no winner is declared before Unity evidence.

The mixed layout covers approximately `58.6%` of the tile. High regions remain localized rather than broad plateaus: Balanced Mix places about `2.43%` of stone pixels above `0.90` height and `9.31%` above `0.75`; Hard Rock Contour places about `2.75%` above `0.90` and `11.38%` above `0.75`. Mean edge-gradient energy is approximately `1.94×` the inner-body gradient for Balanced Mix and `2.37×` for Hard Rock Contour. The final R/G slopes are re-derived from each periodic height field; B remains a hierarchical deep-gap core plus narrow stone-side contact shoulder; A remains lighting-neutral.

Runtime architecture and cost do not change: three temporary 256² RGBA32 mipmapped slices remain during evaluation, only the selected substrate slice is sampled, and there is no new shader sample, ALU branch, draw call, renderer, mesh data, River data, or runtime CPU process. No C#, HLSL, ShaderLab, River source, scene, prefab, canonical Fine Gravel assignment, or unrelated material changes in this patch.

**Unity gate:** rebuild `SSDL_DefaultSurfaceDetails`, compare the two A4 choices with identical shared/application values from the same close and production cameras, include dry and wet views, and judge local size mixing, absence of the prior cross/square macro pattern, hard contour readability, internal form, cavity width, repetition, seam visibility, and mip survival. Select a winner or reject both; do not tune the shader to conceal a deficient packed source.


## GSU-M1.9A.5 — Source-Art Packed Conversion, Macro Rebalance, and Worn Edge Accent

**Status:** Implemented and source-audited; Unity comparison pending.

### Objective

Replace the visually rejected A4 temporary Fine Gravel payloads with two controlled 256² candidates derived from the user-approved worn-rock source image. Preserve the reusable one-sample packed-detail architecture while correcting the three Unity-observed defects: repeated cross/square macro size segregation, excessive tiny-stone noise at gameplay distance, and insufficient hard-rock edge definition.

### Reviewed evidence

- Unity A4 repeat evidence shows a stable cross-like macro region where small stones concentrate through the tile centre while larger stones dominate surrounding regions.
- Unity close and production-camera evidence shows improved internal verticality but weak stone delimitation; rocks read as soft mounds because the packed source lacks an explicit bright worn-rim signal.
- The approved source image contains a better large/medium/small hierarchy, fewer micro-fillers, dark crevices, hard contours, and visible worn edge highlights. It is a beauty source only and must not be sampled directly as packed material data.
- `Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceMaterialDetail.hlsl` already maps positive A-channel variation toward the material light colour and maps B to contact/deep-cavity bands; therefore A can carry a lighting-neutral worn-rim accent without adding a shader sample or material-name branch.

### Approved files

- the five canonical Ground documents;
- `Assets/Game/Demo/Profiles/SurfaceMaterials/SSDL_DefaultSurfaceDetails.asset`;
- the two existing temporary `SSMP_FineGravel_AB_*` assets;
- the two existing temporary `GSLP_FineGravel_AB_*` assets;
- the two existing temporary packed PNG payloads.

Importer metadata, GUIDs, stable IDs, C#, HLSL, ShaderLab, River source, scenes, prefabs, canonical `fine-gravel`, and unrelated materials are outside scope.

### Implementation sequence

1. Treat the approved image as authoring source only. Extract stone silhouettes, neutralized surface character, crevice structure, and worn-edge cues.
2. Repack extracted source stones on a 1024² toroidal authoring canvas with local size-class balancing. Medium stones dominate; large stones remain distributed; the smallest filler class is capped and used only where necessary.
3. Construct coherent per-stone height with localized crowns, irregular internal form, and compressed contact walls. Derive final R/G slopes only after area-downsampling to 256².
4. Derive B as hierarchical contact shoulder plus deep crevice core. Derive A as neutral body variation plus an explicit narrow positive worn-rim band; no directional sunlight or cast shadow is baked.
5. Produce two candidates from the identical periodic layout: a restrained worn-edge candidate and a stronger worn-edge/contour candidate. Retain the temporary A/B GUID and stable-ID plumbing for safe serialized migration.
6. Validate 3×3 repeat, 256/128/64/32 mip survival, edge-neighbourhood continuity, size-density balance, small-stone share, packed-channel ranges, and a CPU reference matching current packed-detail decode. Package only after those checks pass.

### Invariants and non-goals

- Runtime remains one packed sample per active detailed substrate.
- Runtime resolution remains 256² RGBA32 with the existing mip/import contract.
- No geometry, parallax, displacement, extra draw call, renderer, runtime CPU process, or River contract change.
- The explicit rim is an authored material-form cue in A, not a world-light direction and not a replacement for URP lighting.
- A5 is rejected historical evidence. GSU-M2.0 is the active material gate; canonical Fine Gravel remains unfrozen until the imported authored-colour candidate passes Unity comparison and explicit user acceptance.

### Acceptance criteria

- No visible cross, square, quadrant, or axis-aligned size-density pattern in 3×3 repeats or Unity gameplay views.
- Medium stones dominate and tiny filler stones no longer create distant visual noise.
- Individual stones show a clearly delimited hard contour and restrained bright worn rim.
- Internal stone form and verticality remain at least as strong as the accepted part of A4.
- No broad seam band at full resolution or lower mips.
- No runtime architecture or cost regression.

### Implementation result

Historical A5 replaced the two temporary A4 texel payloads in place and retained their existing GUIDs, stable IDs, importer settings, Ground adapters, and serialized selections. Its choices, **`Fine Gravel A5 - Worn Edge`** and **`Fine Gravel A5 - Strong Rim`**, are visually rejected because the packed-only conversion discarded the authored colour and broad form that made the source attractive. They remain cleanup debt only and are not active validation candidates. GSU-M2.0 supersedes them with **`Fine Gravel — Imported Stone Ground 01`**.

The source is cropped to a low-discontinuity 1024² region, projected to a periodic luminance field, segmented into stone bodies, stripped of broad directional lighting planes per stone, and converted into coherent height, slope, cavity, neutral source character, and worn-rim data. The two candidates share one layout, height field, cavity topology, and source character; only packed slope amplitude, A-channel rim strength, and matching generic profile strengths differ.

The runtime tile contains `109` recognized stones after removal of sub-runtime fragments: `31` large, `41` medium, and `37` small by count. Small stones occupy about `0.94%` of runtime texels, medium stones about `9.78%`, and large stones about `51.10%`; total stone coverage is about `61.82%`. The smallest class therefore remains available as sparse filler without recreating the distant micro-pebble carpet. R/G are derived after final 256² downsampling, B remains the hierarchical crevice/contact signal, and A contains neutralized source texture plus a narrow positive worn-rim cue.

Static 256/128/64/32 packed and shader-reference tests report a worst wrap-to-ordinary-adjacency ratio of approximately `1.29`; no exact Unity seam, mip, lighting, dry/wet, or production-camera acceptance is claimed until the project test. Runtime architecture and nominal cost remain unchanged: three temporary 256² slices during evaluation, one packed sample for the selected substrate, no new shader branch, draw call, geometry, renderer, or runtime CPU process.

**Unity gate:** rebuild `SSDL_DefaultSurfaceDetails`, compare **Worn Edge** and **Strong Rim** with identical shared/application values from the same close and production cameras, include dry and wet views, and judge macro repetition, distant noise, worn-rim readability, internal form, cavity width, and mip stability. Promote neither candidate until explicit visual acceptance.
