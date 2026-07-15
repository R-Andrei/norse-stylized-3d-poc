# Ground V4 Contact and Edge Accent Audit and Architecture

## Status

**Audit state: complete. Implementation has not started.**

This is the canonical audit and architecture record for V4 Contact / Edge Accents. Its architecture remains accepted, but implementation is queued until V3M Broad Macro Patch Completion passes gameplay-camera visual acceptance. V3M is tracked in `Ground_Macro_Patch_Audit_and_Architecture.md`.

The broader Ground system is not complete. The accepted completed slice is:

```text
GeneratedGround unified authoring
+ Painted Accent visual layer
+ persistent production bake
+ baked-only runtime
+ build enforcement
+ generated-asset cleanup
```

The queued post-V3M milestone is:

```text
V4 Contact / Edge Accents
```

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
→ Contact / Edge Accents
→ sparse motifs and stamps
→ runtime surface state later
```

Ground must visually connect terrain to the environment. Rocks, river banks, authored paths, modifier regions, structures, and other contact zones should feel seated in or belonging to the surface rather than placed on top of an unrelated material.

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
- semantically responsive to snow, dampness, compaction, shore proximity, and dry/rocky ground;
- capable of grounding a rock or standing mass without drawing a complete cartoon ring;
- capable of emphasizing both banks of a river without duplicating the River corridor's exact waterline rendering;
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
UV2 Y = river/shore influence
UV2 Z = rocky/dry secondary patch
UV2 W = standing-water/puddle potential
```

Evidence:

```text
Assets/Game/Procedural/Ground/GroundGenerator.cs:1031-1045
Assets/Game/Procedural/Ground/GroundHeightFieldSnapshot.cs:98-144
Assets/Game/Rendering/PixelSurface/Includes/PixelSurfaceGroundResponse.hlsl:9-47
```

These masks are produced at Ground mesh-vertex resolution and interpolated by the shader. A Standard 40 m Ground at Medium 33 resolution has approximately 1.25 m vertex spacing. This is appropriate for broad semantic response but not for a stable 0.05-0.50 m contact rim.

**Finding:** existing masks should classify and style a high-resolution contact field, but they should not be the sole geometric source of that field.

**Confidence:** proven, very high.

## 2. The existing shore mask is explicitly a broad hint, not the precise bank boundary

`GroundGenerator.EvaluateShoreInfluence` already evaluates the exact River snapshot, but its own comments state that `UV2.y` is a coarse, low-amplitude bank hint and that the River corridor owns the precise waterline mask.

Evidence:

```text
Assets/Game/Procedural/Ground/GroundGenerator.cs:1329-1438
```

The River snapshot already exposes the data needed for a precise static bank source:

```text
signed lateral distance
visible half-width
surface half-width
bank blend
```

Evidence:

```text
Assets/Game/Procedural/Rivers/StylizedRiverGroundSnapshot.cs:157-209
```

**Finding:** V4 should derive river-bank boundaries from `StylizedRiverGroundSnapshot`, then use the existing shore and damp/deposit masks for visual context. Deriving the production accent from the interpolated shore mask would lose precision that is already available.

**Confidence:** proven, very high.

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
River banks
GroundModifier/path boundaries
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
river-bank boundaries
+ modifier/path boundaries
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
- existing shore, compaction, damp, standing-water, exposure, and rocky/dry masks can style the same field differently;
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
+ current StylizedRiverGroundSnapshot set
+ current GroundModifierSnapshot set
+ current eligible GeneratedGeometryRegistry sources
→ deterministic Contact Accent source snapshots
→ source-specific boundary/footprint resolution
→ unified Ground-local contact coverage raster
→ live Edit Mode R8 preview
→ persistent production R8 output
→ baked-only Play Mode and Player shader sampling
```

## Source snapshots

### River bank source

Use the authoritative River snapshot. Generate left and right bank polylines from the sampled centreline and visible half-widths. Do not infer the bank from `UV2.y`.

The initial field should represent a restrained Ground-side band outside the visible waterline, with optional limited inward damp/deposit overlap. The hidden River handoff boundary is not an artistic bank and must not receive a Contact Accent.

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
shore
compaction
damp/deposit
standing-water potential
exposure
rocky/dry
```

Suggested response selection:

```text
shore or damp context
→ damp/deposit darkening and tint

compaction/path context
→ worn edge darkening and modest vegetation suppression in future systems

neither shore nor compaction
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
River Bank Influence
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
sample modifiers or River snapshots for Contact Accents
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
River spline or visible bank width
GroundModifier shape, transform, blend, or participation
GeneratedMass final geometry or transform
Ground shape changes that alter mass-to-ground contact
Ground patch size or mapping
Contact baker revision or resolution rule
```

### Unrelated

Painted Accent-only changes must not rebuild Contact Accent coverage. Contact Accent-only changes must not rebuild SurfaceStrokes, ProjectedGlyphs, companion clusters, or Painted Accent coverage.

## Event and ordering contract

- Existing GroundModifier and River notifications already reach `GeneratedGround`.
- V4 should subscribe to `GeneratedGeometryRegistry.SourceAdded`, `SourceRemoved`, and `SourceChanged` while enabled in Edit Mode.
- Registry events mark Contact Accent sources stale; they must not force immediate full Ground geometry regeneration.
- Source collection must be sorted deterministically before rasterization because the registry uses a `HashSet`.
- Max-combination makes coverage order-independent, but deterministic ordering is still required for signatures, diagnostics, and reproducible failure evidence.

## Diagnostics

The first implementation should report one compact record per Contact Accent build:

```text
eligible / rejected River sources
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
River Bank Contribution
Modifier Boundary Contribution
GeneratedMass Contact Contribution
Final Lit Contact Response
```

Per-source success logs are forbidden. Failure evidence should be capped to a few representative unique cases.

# Rejected shortcuts

- Use the current shore/compaction vertex masks as the complete production contact geometry.
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
- collect exact River, modifier, and GeneratedMass source snapshots;
- add deterministic mass contact-contour extraction with explicit bounds fallback;
- rasterize a transient Edit Mode R8 field;
- add source-isolated and raw-coverage debug views;
- add compact diagnostics and signatures;
- no normal lit visual response;
- no persistent production output yet;
- no runtime work.

Acceptance:

1. both visible River banks appear at the correct waterline rather than the hidden handoff boundary;
2. modifier core boundaries align with circle and oriented-box shapes;
3. GeneratedMass contacts align with the actual lower footprint rather than a full renderer rectangle;
4. moving or regenerating a GeneratedMass invalidates only the Contact Accent field;
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

After GeneratedMass, River, and modifier coverage is visually accepted, evaluate an explicit opt-in source for huts, bridge supports, ordinary props, and non-generated meshes. Do not add this component pre-emptively.

# Methods-tried ledger

## Accepted

- Existing semantic masks as visual context, not precise contact geometry.
- Exact River snapshots as bank-boundary authority.
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

1. Preserve this accepted V4 architecture while V3M is active.
2. Resume V4-A1 as a raw-source and coverage proof only after broad macro composition is visually accepted.
3. Validate source alignment and invalidation before introducing the normal lit visual effect.
