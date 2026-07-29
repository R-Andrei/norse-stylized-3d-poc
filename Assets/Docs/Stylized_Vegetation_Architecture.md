# PS3D Stylized Vegetation Architecture

## Status

- **Domain:** exploratory procedural vegetation, vegetation response to external wind, interaction, and snow accumulation
- **Current implementation:** historical exploratory document aligned to the accepted production ownership and interaction milestone through `VEG-V2-INTERACT.2B.2`; detailed implementation status is tracked elsewhere
- **Purpose:** preserve early alternatives and long-term vegetation ideas
- **Authority:** `Docs/Vegetation_Rendering_and_Interaction_Architecture.md` is the canonical vegetation architecture and implementation plan. Where this document conflicts with it, the canonical document governs.

This document is intentionally earlier and looser than the canonical vegetation architecture. It describes a likely direction, identifies the strongest architectural patterns, and records alternatives and curiosities worth remembering later. It does **not** lock exact data formats, class names, shader contracts, or pass-by-pass implementation work.

---

## 1. Visual north star

Vegetation should feel like part of the same stylized world as the rocks, river, snow, fog, and atmosphere.

The goal is not botanical realism. The goal is a world where:

- grass, reeds, brush, and low branches move with a shared wind language;
- nearby movement parts and depresses vegetation clearly enough to read from gameplay distance;
- strong abilities can stamp visible temporary reactions into the environment;
- snow can gather, thin, and shed in a way that feels coherent even when the underlying method is simplified;
- broad coverage can be achieved without maintaining thousands of unique assets or heavy per-instance simulation.

The strongest image is not "every blade is accurate." The strongest image is "the whole field feels alive, connected, and authored."

---

## 2. Design constraints

The desired system must serve several constraints at once:

- very low dependence on hand-authored vegetation assets;
- high coverage density;
- stylized motion rather than physically exact simulation;
- interaction with player movement and gameplay abilities;
- at least a plausible snow state;
- bounded runtime cost;
- clear scalability from prototype scenes to larger outdoor spaces.

These constraints strongly discourage any architecture based on:

- one GameObject per plant;
- per-blade colliders;
- skeletal rigs for ordinary grass;
- CPU-side simulation of dense foliage;
- custom snow meshes on every small plant.

The system should instead follow the same philosophy already used by river disturbance and Foam:

```text
many visible instances
    driven by a small number of shared fields
    with fixed-cost or bounded-cost sampling
    plus chunking, sleeping, and quality tiers where helpful
```

---

## 3. Canonical mental model

The most promising direction is **field-driven instanced vegetation**.

A useful mental model is:

```text
small procedural family library
    + chunked instance placement
    + shared external Weather/Wind field
    + scene-owned immediate interaction field
    + Ground-owned historical trample fields where explicitly authored
    + shared environmental state inputs
    + fixed-cost vegetation shader logic
```

This means:

- geometry variety comes from procedural recipes and instance variation;
- motion comes mostly from shared world-space signals;
- ordinary actor interaction comes from one transient scene field with no stored trail history;
- short-lived or persistent trampling comes only from explicit Ground-owned history stamps;
- snow comes mostly from shared shading and state logic rather than bespoke meshes;
- the final renderer never loops over arbitrary counts of active movers or abilities.

This is likely the only direction that satisfies "dense," "interactive," and "performant" at the same time.

---

## 4. What the system represents

The final vegetation result does not need to represent a physically exact set of independent plants.

Instead, it can represent several layers of abstraction:

- **instance families:** visible clumps, stalks, shrubs, saplings, branch clusters, and ground cover patches;
- **wind response:** vegetation-specific response to externally supplied direction, gusting, turbulence, and local phase variation;
- **interaction response:** flattening, bending away, rebound, trampling memory, shockwaves, and ability-driven pulses;
- **environmental state:** snow load, wetness, frost, seasonal tint, and exclusion masks;
- **coverage logic:** where vegetation exists, how dense it is, and what family dominates that zone.

Not every family needs every feature.

For example:

- grass and reeds may use the full wind + interaction + snow stack;
- low shrubs may use weaker bend and chunkier snow response;
- distant tree canopies may use wind and snow only;
- ground moss may ignore dynamic bending and rely only on shading changes.

This asymmetry is desirable. It keeps the system expressive without forcing one expensive universal solution.

---

## 5. Procedural geometry strategy

### 5.1 Begin from a small family library

Vegetation should come from a compact family vocabulary rather than a large asset catalogue.

Likely useful families:

- grass tufts built from a few ribbon blades;
- reed bundles with taller vertical silhouettes;
- low brush clusters made from card fans or thickened ribbons;
- thorn or dead scrub silhouettes for sparse harsh areas;
- branch-and-needle or branch-and-card conifer clusters;
- simple saplings using spline trunks and lightweight crown geometry.

Each family can be generated from a recipe containing:

- seed;
- height range;
- width range;
- lean;
- blade or branch count;
- silhouette spread;
- curl or arc;
- tip sharpness;
- density tags;
- wind responsiveness;
- snow receptiveness.

### 5.2 Keep the visual burden on silhouette, not detail

At your camera distance, strong silhouette and timing will matter more than microstructure.

This suggests:

- fewer but bolder blades per tuft;
- exaggerated arcs and taper;
- readable clump breakup;
- strong directional lean in exposed areas;
- optional region-specific silhouette grammars such as bent tundra grass, stiff reeds, or snow-weighted shrubs.

### 5.3 Generated and baked are both valid

Procedural does not require fully runtime-generated meshes.

A practical split may be:

- generate family meshes in editor tooling;
- save a small baked library of canonical cluster archetypes;
- use runtime variation and instancing for scale;
- reserve true runtime mesh generation for specialized zones or hero features.

This can preserve the "few assets" goal while reducing runtime complexity.

---

## 6. Placement and coverage strategy

Vegetation likely needs its own world-construction layer rather than ad hoc per-object placement.

Useful placement inputs:

- terrain slope;
- altitude;
- moisture proxy;
- snow exposure;
- distance from water;
- riverbanks and shore masks;
- rock and path exclusion masks;
- biome or region tags;
- wind exposure;
- authored suppression and density volumes.

The likely coverage model is:

```text
coverage map / zone rules
    -> patch generation
    -> family selection
    -> instanced placement inside chunks
```

`VEG-V1H` establishes the current authored-domain baseline. When Ground Coverage Integration is enabled and a `GeneratedGround` is assigned, the Ground owns the complete placement extent, transform, sampled height, and authored coverage. The vegetation renderer generates candidates in Ground-local XZ, samples in world space, and packs accepted positions into the vegetation object's local space. The old `VegetationBenchmark.fieldSize` remains only as an unassigned fallback and as the fixed forced domain used by the V1G performance suite. Render bounds are derived from accepted instances rather than from that manual rectangle.

This is intentionally not the final hierarchy or multi-family authoring architecture. Automatic vegetation-child creation, painting through vegetation objects, multiple simultaneous vegetation families, and either multiple coverage fields or a richer family-assignment coverage representation require a separate approved infrastructure design. `VEG-V1H` fixes spatial ownership without choosing that later representation prematurely.

Each chunk can own:

- a list or buffer of visible vegetation instances;
- density and family metadata;
- optional local masks;
- culling bounds;
- LOD state;
- optional interaction-field relevance.

For far distances, dense vegetation may stop existing as discrete clumps and become:

- terrain-level tint shifts;
- stylized coverage normal/detail contributions;
- extremely cheap impostor strips or cards;
- sparse representative instances over a coverage impression.

That transition is important. Dense vegetation is often affordable only if it gradually becomes "coverage language" rather than "literal objects."

---

## 7. Wind architecture

### 7.1 Preferred direction: one shared external Weather/Wind field

Wind is not owned by vegetation. `Docs/Weather_Wind_Architecture.md` now defines the shared Weather-owned XZ wind domain, CPU query contract, GPU target field, and gameplay-anchor-centred dynamic response cache.

Vegetation owns only family-specific response:

- blade or branch stiffness;
- root-to-tip weighting;
- family response amplitude;
- deterministic local response variation;
- composition with interaction fields.

The rejected temporary vegetation provider and its analytical traveling-front recovery are superseded. The initial Weather implementation is XZ-only because the game camera and current consumers are top-down ground-domain systems. Future wind lines and gameplay effects must consume the same Weather source rather than creating separate vegetation or VFX wind state.

### 7.2 Multiple motion bands

Vegetation motion should probably not be one uniform sway term.

A useful breakdown:

- **macro sway:** broad slow directional bend shared by nearby vegetation;
- **secondary flutter:** smaller independent motion per blade, card, or branch group;
- **gust response:** temporary amplification and directional sharpening;
- **recovery bias:** tendency to spring back rather than remain permanently displaced;
- **weighting by family:** reeds, grass, shrubs, and conifer branches respond differently.

### 7.3 Stylization opportunity

Wind does not need to imitate real-world aerodynamics perfectly.

Interesting stylized choices could include:

- region-specific wind timing;
- slight rhythmic exaggeration in sacred or uncanny areas;
- wind that travels in visible bands across fields;
- vegetation that bends more like brushed calligraphy strokes than like literal blades.

These should remain optional. The baseline should still work in ordinary outdoor scenes.

---

## 8. Interaction architecture

### 8.1 Two lifetime classes, two owners

Vegetation interaction is split by whether the effect must retain history.

**Immediate displacement** is owned by one scene-level `VegetationInteractionDomain`. It is a moving world-space XZ field centred on an explicit gameplay anchor or fallback camera-ground projection. Ordinary actors register through `VegetationInteractor`, bend and temporarily flatten grass while present or moving, and leave no stored trail state. The field update rate is an exposed `5–60 Hz` control; the initial default is `20 Hz`, and `10 Hz` is an intentional supported tuning target. A configurable central margin prevents quarter-metre anchor motion from recentering the complete field continuously. Low-cadence swept movement is front-loaded toward the current actor position, and cells that lose support decay analytically each rendered frame through the configured immediate recovery time rather than remaining blended with the older stronger field.

**Historical trampling** is owned by one optional `VegetationTrampleDomain` on the affected Ground vegetation infrastructure in fixed Ground-local space, so short-lived trails, timed ability damage, and session-persistent stamps survive camera recentering. Its update rate is independently exposed across the same `5–60 Hz` range. Ordinary player movement does not write this field by default. Opt-in moving writers and queued circle, cone-sector, and line ability requests share the same stored footprint and recovery state.

### 8.2 Immediate sources

Any moving or stationary object may receive `VegetationInteractor`; the system has no player-specific dependency and does not require a Rigidbody. Initial sources include:

- player or enemy bodies parting grass without a trail;
- animals, crates, rolling objects, or other movers;
- large actors with wider and stronger immediate displacement;
- stationary bodies continuing to hold grass aside.

Movement is stamped as a swept capsule between fixed interaction samples so reduced update rates do not create disconnected circles. `Sweep Tail Retention` controls how strongly the previous end of that capsule remains supported relative to the current actor end; this reduces the visual wake without changing cadence or writing history. Each interactor selects `Radial`, fixed-world-`X`-biased, or `Hybrid` direction shaping. World-X bias and world-Z strength are independent controls: X bias redirects toward pure world `±X`, while Z strength multiplies the final biased Z component. The basis is the fixed map axes (`X` left/right, `Z` map up/down), not actor facing or motion direction; only Radial and Hybrid may additionally use speed-derived movement influence. Near the exact actor X centreline, deterministic world-cell variation selects a stable left/right side.

### 8.3 Layer response

All vegetation layers sample the same immediate field but retain independent material response:

- horizontal bend response;
- temporary flatten response;
- root-to-tip interaction exponent;
- maximum world-space interaction bend;
- interaction lighting-normal response;
- Weather influence retained while the recipe is displaced.

This lets tall soft grass react strongly while short stiff grass resists the same physical footprint. The root remains planted and Weather remains a separate owner. The shader samples immediate interaction once, attenuates Weather according to the recipe and effective interaction strength, then applies interaction deformation before combined lighting-normal evaluation.

### 8.4 Historical trails and gameplay ability stamps

Historical interaction is opt-in rather than an automatic consequence of movement. Selected large movers may write swept trails. Gameplay abilities submit explicit one-shot requests to the Ground-owned trample domain without temporary actors or coverage edits.

The supported gameplay shapes are:

- `Circle`: one radial footprint with 360-degree coverage;
- `Cone`: the same radial-sector footprint with coverage below 360 degrees and an authored world-XZ facing direction;
- `Line`: one width-controlled capsule between world-space start and end points.

Shape footprint, displacement mode, bend, flattening, recovery mode, recovery delay, recovery duration, deterministic edge irregularity, scale, seed, and priority remain independent controls. Displacement may be radial outward, fixed world direction, away from the shape centreline, or flatten-only. Timed ability state uses the same delayed asymmetric slow–fast–slow recovery as moving trails; session-persistent state lasts until runtime reset.

`VegetationTrampleStampTester` is an optional player-attached test harness with editable named configurations, previous/next/random selection, bounded randomized variants, Gizmo previews, and Inspector buttons. It invokes the exact public runtime API and is not part of gameplay ownership.

---

## 9. Snow and environmental accumulation

### 9.1 Prefer shared snow logic over true accumulation

Snow is the easiest feature to overbuild.

The likely correct first approach is not literal volumetric accumulation on every plant. It is a stylized shared snow function driven by:

- world normal orientation;
- upward exposure;
- local wind exposure or shelter;
- height and biome;
- optional occlusion or canopy factor;
- an authored seasonal or weather amount;
- optional interaction-induced shedding.

This can already produce a convincing result if the vegetation geometry is simple and the silhouettes are bold.

### 9.2 Likely visible snow behaviours

Useful snow responses:

- top-facing surfaces brighten and cool in hue;
- upper edges appear thicker or softer;
- fine grass tips carry less snow than broad leaves or shrub crowns;
- bent vegetation sheds or thins snow more quickly;
- dense exposed brush can hold clumpy bright caps;
- sheltered or wind-shadowed zones accumulate differently from exposed ridges.

### 9.3 Janky but good cheats

Several "fake" methods may be completely acceptable in this art direction:

- height-and-normal-based snow blend only;
- extra top-side shell tint without geometry growth;
- screen-space or camera-facing snow edge brightening;
- a separate snow-cap mask on shrubs and clumps;
- interaction briefly reducing snow amount locally;
- snow amount modulating bend stiffness so loaded vegetation moves differently.

These can read better than a costly physical simulation if the stylization is deliberate.

### 9.4 Optional later directions

If the project later needs stronger snow behaviour, possible upgrades include:

- chunk-level snow state textures;
- localized snow compression by footsteps or abilities;
- snow shedding events driven by strong wind gusts;
- branch-specific crown loading for larger vegetation;
- shared winter-state masks reused by terrain, rocks, props, and vegetation.

These are interesting, but none should be treated as mandatory for a first implementation.

---

## 10. Rendering and shading model

The canonical implementation now establishes the first shared-lighting contract in `Docs/Vegetation_Rendering_and_Interaction_Architecture.md` under `VEG-V1C`. Vegetation consumes ordinary URP scene lighting rather than referencing the time-of-day controller directly:

- ambient lighting comes from Unity/URP ambient spherical harmonics, which reflect the time-of-day system's published `RenderSettings` state;
- the main directional light provides sun direction, colour, and intensity;
- point and spot lights use URP additional-light attenuation;
- thin two-sided grass uses a wrapped two-sided diffuse response rather than PBR;
- authored root/body/tip colours remain the vegetation albedo language;
- real grass shadow casting and real-time shadow receiving remain excluded from the first pass.

The current pass is intentionally texture-free and fixed-cost apart from the bounded URP additional-light loop. PBR, specular response, translucency, and patch-edge shadow illusions remain later decisions. `VEG-V1E` adds an analytical wind-deformed lighting normal from the existing vertex displacement, without an additional Weather sample or fragment-light cost.


### Wind-deformed lighting normals and bend-side body shading

`VEG-V1E.2` is the current bend-reactive body-lighting contract. It preserves the `VEG-V1E.1` analytical normal implementation: the vertex shader returns the existing response-scaled full-tip Weather displacement before the accepted `rootToTip²` position weighting, differentiates that quadratic bend analytically as `up + (2t/H) × A_tip`, and crosses the tangent with the blade lateral direction to obtain the lighting normal.

- `Wind Normal Response` remains material-only, ranges from `0` to `4`, and defaults to `0.70`.
- `0` preserves the prior static-normal lighting path; `1` applies the complete analytical wind slope; values above `1` provide bounded stylized normal-tilt exaggeration without changing deformation.
- `Wind Bend Shading Response` is a separate material-only control in `0..2`, default `1`. It controls explicit curvature contrast rather than moving the diffuse boundary.
- The shader projects existing full-tip displacement onto the undeformed card normal, divides by blade height, and flips the sign for the rendered back face. Positive rendered-side bend is concave; negative is convex; bend within the card plane produces no response.
- The response fades in over `3%..30%` signed normal bend and grows from root to tip with `t²(3-2t)`.
- At response `1`, the concave face darkens by up to `30%` and the convex face brightens by up to `12%`; response `2` doubles those bounded amounts to a body multiplier range of `0.40..1.24`.
- The multiplier affects only `authored body colour × resolved ambient/direct lighting`. The punctual graphic edge accent is added afterward unchanged.
- Roots remain unchanged and upper vertices react progressively with bend.
- The update adds no Weather texture sample, sine, light loop, shadow sample, geometry, buffer, or draw call. It adds one scalar varying and low-cost vertex/fragment arithmetic.
- This is a stylized curvature cue, not real self-shadowing.


### Grass-owned macro patch composition

`VEG-V1F` adds a spatially coherent colour-composition layer above independent per-cluster micro variation. The grass field owns this pattern rather than following Ground by default.

- One signed dark/neutral/light macro value is evaluated in world space when each accepted cluster is built.
- The value is stored in the existing unused `VariationPhase.w` channel; the instance record remains three `Vector4` values and 48 bytes.
- Scale, seed, transition softness, and neutral separation define the baked field and require a vegetation rebuild.
- Dark and light strengths are material-only controls. They scale the complete root/base/tip body colour while leaving the punctual edge accent separate.
- Existing independent cluster colour variation remains as the micro layer.
- No procedural noise, texture, texture sample, extra buffer, draw call, or fragment noise evaluation is added at runtime.
- Ground's exact macro pattern is not reused because the current Ground implementation exposes only a shader-side evaluator, not a reusable CPU/cache result. Exact matching would not reduce active runtime computation and would add cross-subsystem coupling.
- Optional Ground influence, patch-driven size/stiffness/density, and authored mask input remain later decisions.

### Punctual-local-light blade-edge accent

`VEG-V1C.9` is the current contract. It retains VEG-V1C.6 lighting ownership and response, preserves the expanded authored edge-width ceiling and accepted edge-mask shape, measures the linear normalized blade footprint before clipping, and rejects narrow accents from the effective light-selected band inside each punctual-light evaluation.

`VEG-V1C.8` is rejected and superseded because it differentiated a saturated post-clip edge-distance field and applied one authored-width gate before the light-facing side selector. Subpixel bands could be falsely classified as wide, while the final bright band could be materially narrower than the tested authored band.

`VEG-V1C.7` remains rejected and superseded because its `fwidth` denominator, broad `1.0..1.5 px` transition, squared stability, and altered edge filter removed or weakened coherent game-camera accents.

- Ambient SH remains broad and never produces an edge term.
- The main sun and every directional light affect ordinary blade-body lighting only. They never produce the stylized edge accent and are never globally reduced by the accent master.
- Only punctual point and spot lights may produce the accent. `Local Edge Activation Threshold` evaluates normalized, unpowered punctual-light energy; below that threshold the accent is exactly zero.
- `Local Edge Falloff Power` shapes final graphic edge radiance once. A value of `1` follows normalized URP distance attenuation; higher values make the edge weaken faster without multiplying close-range distance attenuation above `1`.
- Ordinary local-light body illumination continues to use the full URP attenuation curve and is not narrowed or bounded by the edge-falloff control.
- `Stylized Edge Accent` remains the single master. Its nonlinear response is restrained below `1`, while `1` retains the established maximum edge gain of `4`. The coupled punctual body-fill restraint uses the same shaped response.
- The edge-side selector has a directional dead zone. The opposite edge and nearly perpendicular light/blade orientations receive zero instead of a symmetric residual.
- Eligible strong local lights retain additive post-albedo radiance, HDR-capable light colour, and the `Edge Highlight Whiteness` control.
- `Edge Accent Width` remains an authored normalized silhouette width and now supports `0.01..0.50`; existing serialized values are preserved. Screen-space derivatives antialias the boundary without widening the authored line.
- `Minimum Stable Accent Pixels` is evaluated per eligible punctual light. The shader differentiates the unsaturated normalized blade coordinate before clipping, converts that gradient to pixels per signed unit, narrows the authored band by the actual lateral light alignment at the side-selector midpoint, and fades only effective bands below the configured `1.0..1.2 px` range. Ordinary local-light body illumination does not use this gate.
- A master strength of `0` preserves the accepted VEG-V1C body-lighting result exactly.
- The custom additional-light path uses Unity 6.5 `_CLUSTER_LIGHT_LOOP` / `USE_CLUSTER_LIGHT_LOOP` compatibility while retaining the ordinary Forward light loop.
- This remains a light-directional vegetation response, not a camera Fresnel outline, ambient glow, or sun-driven field outline.
- **Historical and rejected — `VEG-V1C.10`:** this version attempted to identify the LightRay Spot through an unregistered high Rendering Layer bit. AF5A runtime diagnostics proved that the bit did not survive into GPU additional-light data. The current implementation is `VEG-V1C.11` geometric Spot matching; the high-bit identity must not be restored.

The vegetation shader should probably carry more responsibility than the geometry.

Likely responsibilities:

- wind bend;
- interaction bend and flattening;
- per-instance color and phase variation;
- snow blending;
- optional translucency or wrapped lighting;
- stylized edge response;
- LOD-aware simplification.

Likely inputs:

- instance transform and family parameters;
- per-instance seed or packed variation values;
- shared wind state;
- scene-owned immediate interaction field;
- optional Ground-owned trample history when explicitly authored;
- environmental state parameters;
- optional terrain or zone masks.

The final shader should remain fixed-cost with respect to active movers and abilities. That means no loops over nearby actors. All dynamic influence should already be resolved into shared data.

---

## 11. Quality, chunking, and performance model

The likely performance cornerstones are:

- instancing instead of one renderer per plant;
- chunk-based placement and culling;
- world-space shared fields instead of per-instance simulation;
- quality-scaled density;
- quality-scaled immediate-field resolution and an exposed `5–60 Hz` update rate;
- independently quality-scaled future trample-field resolution and `5–60 Hz` update rate;
- LOD transitions from clumps to coverage;
- sleeping or update suppression when no interactions are nearby;
- low-frequency updates for persistent field decay where possible.

### 11.1 Likely cost hierarchy

The expensive parts are likely to be:

- overdraw;
- too many unique materials or variants;
- too much vertex count in near-field clumps;
- too high an interaction-field resolution;
- too many transparent layers;
- CPU-heavy placement churn.

The system should therefore prefer:

- opaque or alpha-clipped stylized materials over expensive blended transparency where possible;
- bold simple shapes over many thin cards;
- larger shared chunks rather than excessive tiny runtime objects;
- stable buffers rather than frequent instance rebuilds.

### 11.2 A useful rule

If a feature requires touching every plant instance on the CPU every frame, it is probably the wrong architecture.

---

## 12. Alternatives and curiosities

This section is intentionally noncanonical. These are plausible variants or special-case ideas, not recommendations by default.

### 12.1 Terrain-detail-first approach

Unity terrain details or a terrain-driven grass system may be a useful prototype accelerator, especially for wide fields.

Pros:

- fast to get visible coverage;
- built-in placement workflows;
- useful for validating density and silhouette needs.

Cons:

- weaker control over project-specific stylized motion and interaction contracts;
- may resist the shared-field architecture you are already building elsewhere;
- may become awkward once snow and custom ability interaction matter.

This may still be a valid temporary stepping stone.

### 12.2 Pure compute-driven blade generation

It is possible to go much deeper and generate or animate vegetation almost entirely on the GPU.

This is attractive for density, but risky early:

- more complex tooling;
- harder debugging;
- potentially slower iteration during art-direction exploration.

This should be considered only if ordinary instancing stops being sufficient.

### 12.3 Coverage-only far fields

Far vegetation may not need to be instances at all.

An interesting stylized option is to turn distant grasslands into:

- animated terrain-space streaks;
- directional color bands;
- wind-swept normal modulation;
- sparse silhouette representatives only near skylines.

This may fit a mythic or diorama-like visual identity better than insisting on literal distant grass.

### 12.4 Ability-specific authored reactions

Ordinary traversal should use the shared immediate field, while exceptional abilities may submit explicit Ground-owned historical stamps or authored overlays:

- rune shock patterns burned into brush;
- frost bloom that stiffens and brightens nearby vegetation;
- corruption pulse that changes bend timing and tint rather than only displacement.

These can sit above the shared system as additive style layers.

---

## 13. Non-goals

The system should not become:

- a realistic plant-by-plant botany simulator;
- a physics sandbox of individual stems and leaves;
- an excuse for thousands of interactive colliders;
- a separate bespoke solution for every vegetation family;
- a literal snow-volume simulator on ordinary grass;
- a renderer dominated by alpha-blended overdraw;
- a dependency on a huge authored foliage library.

The point is not to reproduce a conventional open-world vegetation stack in miniature. The point is to create a coherent stylized alternative.

---

## 14. Suggested phased implementation direction

This is not a committed roadmap, only a likely sane order.

### V0 - visual and rendering benchmark

- follow the canonical V0 plan in `Docs/Vegetation_Rendering_and_Interaction_Architecture.md`;
- compare opaque strips, crossed cards, and hybrid clusters;
- test 12/16/20 clusters per square metre;
- consume the shared Weather XZ wind domain;
- validate silhouette readability, overdraw, and 1440p cost.

### V1 - production static vegetation renderer

- add vegetation profiles, Ground sampling, deterministic placement, chunking, culling, and LOD;
- continue consuming external wind inputs only.

### V2 - stylized lighting and patch-edge shadowing

- shared URP ambient, sun, and local-light response begins in the benchmark through `VEG-V1C`;
- later add internal depth shading and Ground-edge anchoring without a grass ShadowCaster pass.

### V3 - immediate interaction field

- add one scene-owned `VegetationInteractionDomain` and transform-driven `VegetationInteractor` registry;
- support multiple ordinary actors with no historical trail state;
- expose a `5–60 Hz` immediate update rate and validate continuous swept displacement at 10 Hz;
- validate parting, temporary flattening, rebound, layer stiffness differences, and combined Weather lighting response.

### V4 - opt-in trample history and snow integration

- **INTERACT.2A active implementation:** add one optional fixed Ground-local `VegetationTrampleDomain` per Ground vegetation root;
- only explicitly configured moving interactors write history; ordinary player interaction remains immediate and history-free by default;
- expose its update rate independently across `5–60 Hz`;
- support timed recovery and session-persistent state without claiming save-file permanence;
- **INTERACT.2B active implementation:** add queued gameplay-authored circle, cone-sector, and line stamps plus the optional player-attached test harness;
- add shader-driven snow response and interaction-driven shedding after trample ownership is validated.

### V5 - world and ecosystem integration

- connect coverage generation to terrain, rivers, rocks, and exclusion zones;
- add chunk-based culling and quality scaling;
- introduce distant coverage language.

### V6 - optional specialization

- family-specific motion tuning;
- shrub and sapling variants;
- advanced ability reactions;
- persistent trampling or seasonal state if still desired.

---

## 15. Working recommendation

If the project wants a strong result with the least risk, the best current recommendation is:

1. use a small generated or baked library of bold vegetation clumps rather than many unique assets;
2. render them through instancing and chunked placement;
3. drive them with one shared external Weather/Wind field and one scene-owned immediate interaction field;
4. add Ground-owned trample history only for explicitly configured trails or gameplay stamps;
5. treat snow primarily as a shared shading and state problem rather than a per-plant geometry simulation;
6. let far vegetation become stylized coverage rather than insisting on literal distant plants.

This direction aligns with the framework's broader preferences:

- procedural families instead of asset catalogues;
- shared field logic instead of per-object logic;
- stylized coherence instead of realism for its own sake;
- bounded cost instead of unscalable simulation.

The concrete implementation sequence is now defined by `Docs/Vegetation_Rendering_and_Interaction_Architecture.md`. This exploratory document should not be used as an implementation ledger.

## 16. Production ownership and layer hierarchy

The accepted production hierarchy separates Ground, vegetation recipes, shared Weather, and diagnostics:

```text
Scene
├── Systems
│   ├── Weather                         [WeatherWindDomain]
│   ├── Vegetation Interaction          [VegetationInteractionDomain]
│   └── Diagnostics
│       └── Vegetation Benchmark        [VegetationBenchmarkRunner]
│
└── GeneratedGround                     [GeneratedGround]
    └── Vegetation                      [GroundVegetation, optional VegetationTrampleDomain]
        ├── Grass_Default               [VegetationLayer]
        ├── Grass_Tall_Dark             [VegetationLayer]
        └── Grass_Short_Bright          [VegetationLayer]
```

Each named recipe is one GameObject with one `VegetationLayer` component directly attached. No additional child GameObject exists merely to hold the component.

A vegetation layer means one independently configured recipe plus one independently paintable coverage field. It does not require a unique biological family. All production layers use the fixed CrossedCards cluster and may differ in height, width, density, colour, stiffness, wind response, lighting, macro patches, or any later recipe setting. Masks may overlap.

`GeneratedGround` owns terrain geometry, transform, domain, height sampling, and surface revision. A `VegetationLayer` consumes that surface and owns its recipe, mask, placement instances, GPU resources, and rendering. `GroundVegetation` coordinates direct recipe children and is the ownership anchor for an optional `VegetationTrampleDomain`. It does not render or own recipe coverage; the trample component owns one shared fixed Ground-local historical field for all direct layers.

A production layer never chooses a Ground through an editable object-reference field. It resolves the nearest `GeneratedGround` ancestor automatically, including through the `Vegetation` organizational child. Reparenting changes ownership and rebuilds the layer. A layer outside a Ground hierarchy is invalid and renders nothing.

Production layers do not own benchmark suites. Exactly one scene-level `VegetationBenchmarkRunner` measures the enabled authored stack or an explicitly selected controlled layer. `WeatherWindDomain` remains the scene-owned wind source, while `VegetationInteractionDomain` independently owns immediate actor displacement. Neither shared system is stored on vegetation recipes.

Layer coverage authoring belongs to the selected `VegetationLayer`. Drag stamps mutate only that layer's serialized CPU mask, and one rebuild occurs when a changed brush stroke completes. The deleted legacy Ground vegetation mask and migration paths are no longer part of production ownership.

`GroundVegetation` manages direct recipe children on explicit editor actions only. The scene-level `VegetationBenchmarkRunner` is diagnostics-only and never renders vegetation. Its stack-aware timing mode toggles existing layer render flags for paired baseline windows; controlled-layer mode temporarily varies only the selected layer's density, suppresses siblings, and restores every captured flag and exact density.

`VEG-V2-INFRA.1B` is frozen by user acceptance on 2026-07-21 after a two-layer scene inventory reported one active runner, two ready enabled layers, exact 12-triangle-per-instance accounting, exact 48-byte instance-buffer accounting, and exact aggregate totals.

The current production sequence is authoritative in `Vegetation_Rendering_and_Interaction_Architecture.md`:

1. **Complete and frozen** — production rendering lives in `VegetationLayer`, each layer owns one independent coverage field, and `GroundVegetation` coordinates direct recipe children;
2. **Complete and frozen** — Weather ownership lives in one scene-level `Systems/Weather` `WeatherWindDomain`;
3. **Complete and validated** — the scene runner measures the exact authored stack, independently tests a selected layer at benchmark-owned `20/35/50` tiers, adds a non-duplicate exact authored density when required, and restores arbitrary density values exactly;
4. **Complete and validated** — obsolete combined-renderer, wind-shim, migration, retirement, Ground-mask, and fallback code is deleted; only production layers, the scene runner, and scene Weather remain;
5. **Complete at source level; Unity validation pending** — CrossedCards is the sole production cluster, controlled benchmarking varies density only, invalid-resolution runs cannot rank cases, and negative timing deltas are treated as baseline fluctuation;
6. **Complete and user-validated through VEG-V2-INTERACT.1B** — one scene-owned immediate field accepts multiple transform-driven interactors, exposes a `5–60 Hz` update rate, supports radial/fixed-world-X/hybrid direction shaping, suppresses world-Z independently, reduces Weather on displaced recipes, uses recenter hysteresis and low-cadence release compensation, and stores no trail history.
7. **Complete and user-validated through VEG-V2-INTERACT.2B.2** — one optional fixed Ground-local trample domain accepts opt-in moving writers and queued circle, cone-sector, and line ability requests, exposes an independent `5–60 Hz` update rate, and shares one historical footprint across every recipe on that Ground. Timed state is fully held for an authored delay, then follows the accepted asymmetric slow–fast–slow return; session-persistent state remains until runtime reset. The optional player-attached tester invokes the exact public runtime request path. The proposed large interaction stress benchmark is not part of the accepted continuation because realistic simultaneous actor scale is approximately five to ten, and the user does not expect vegetation interaction to be the limiting system at that scale.

### Arbitrary recipe density and benchmark-owned tiers

Production `VegetationLayer` density is an unrestricted positive integer recipe value. Values such as `37` or `43` clusters/m² remain exact and are not snapped to diagnostic presets. The scene-level `VegetationBenchmarkRunner` owns independent standard tiers of `20`, `35`, and `50` clusters/m².

A complete controlled benchmark begins with one exact current-authored-stack case, so mixed layer recipes such as `35` and `43` clusters/m² are measured together as one real scene configuration. It then runs the selected fixed CrossedCards layer at the standard density tiers. The selected layer's exact current density is added as one controlled case only when it is not already a standard tier. All captured arbitrary values are restored exactly.


### Production-only cleanup boundary

`VEG-V2-INFRA.3A` was superseded before Unity application because its scene-retirement transaction added unnecessary transitional machinery. `VEG-V2-INFRA.3` directly deletes obsolete source: the combined legacy renderer and editor, vegetation-owned Weather subclass, migration utilities, rejected retirement utility, Ground-owned vegetation mask, import identifiers, fallback placement domain, and diagnostic-subclass hooks.

The user deletes the obsolete `VegetationTest` GameObject directly in Unity if it still exists. Retained production ownership is limited to `VegetationLayer`, `GroundVegetation`, `VegetationBenchmarkRunner`, and `WeatherWindDomain`. `VEG-V2-FOUNDATION.1` removes the former geometry enum and unused OpaqueStrips/Hybrid paths; the current shader asset name remains historical only and is intentionally not renamed.


### CrossedCards production freeze

The two completed INFRA.2A reports validated the runner and restoration paths but could not select a timing winner because almost every delta remained inside observed noise and the measured resolution was `509 × 1285`, not the required `2560 × 1440`. Structural evidence is decisive: CrossedCards submits 12 triangles per cluster, Hybrid 24, and OpaqueStrips 32, while all candidates use the same 48-byte instance data and the user had already selected CrossedCards visually.

`VEG-V2-FOUNDATION.1` therefore freezes one production geometry:

- every `VegetationLayer` builds the same three-card CrossedCards cluster;
- recipe authoring exposes no geometry selector;
- the controlled benchmark varies density only at `20`, `35`, and `50` clusters/m², plus one exact non-tier authored density;
- performance ranking is invalid unless the run uses the required `2560 × 1440` target resolution;
- negative enabled-minus-disabled deltas are baseline fluctuation, never evidence that vegetation improves performance.


- `VEG-V1C.11` supersedes the failed runtime-only Rendering Layer identity from `VEG-V1C.10`. The Weather controller publishes the selected LightRay Spot position/range and horizontal source direction. Vegetation geometrically matches that Spot inside its existing additional-light loop, keeps the real Spot direction for body lighting, and uses the horizontal source direction only for the matched Spot's stylized blade-edge selector. Ordinary lights remain unchanged.


- `VEG-V1C.12` adds no vegetation authoring control. One controller-owned Weather LightRay `Accent Line Intensity` scales only the geometrically matched LightRay Spot's stylized edge response; ordinary punctual lights remain unchanged.
- `VEG-V1C.13` expands that master after Unity proof showed AF5C was severely underpowered: approximately `0.083` preserves AF5B, approximately `0.167` reproduces the former AF5C maximum, and `1` permits up to `12x` AF5B before the established receiver cap.


- `VEG-V1C.14` replaces the underpowered linear AF5D LightRay accent mapping with an exponential shared-controller range. `0` remains off; approximately `0.03` preserves the former low baseline; `0.10` reproduces the former AF5D maximum; `0.20` is approximately `3x`; `0.50` approximately `31x`; and `1.0` is `1000x` the former AF5D maximum. The former matched-LightRay `min(4, ...)` ceiling is removed. Ordinary punctual-light vegetation response remains unchanged.
- `VEG-V1C.15` halves the complete AF5E Weather LightRay accent response at every normalized controller value while preserving the exponential slider shape, `0 = off`, geometric Spot matching, and all ordinary-light behavior. The new maximum is `500x` the former AF5D reference instead of `1000x`.

- `VEG-V1C.16` reduces the complete AF5F Weather LightRay accent response to `40%` at every normalized controller value while retaining its exponential shape and `0 = off`. The upper endpoint is now `200x` the former AF5D reference instead of `500x`; geometric Spot matching, body lighting, and ordinary-light response remain unchanged.

- `VEG-V1C.17 / WEATHER-LIGHT-RAY-V1.1D-AF5H` preserves the AF5G `0.2 * (1001^c - 1)` response but removes two stable-runtime costs. The Weather controller caches the normalized and resolved response and recalculates `Mathf.Pow` only when the effective authored value or enable state changes. Vegetation bypasses geometric Spot matching when resolved production accent output is zero; the consolidated diagnostic mode deliberately forces matching so zero-output identity tests remain valid.
- AF5H explicitly supports one simultaneously matched vegetation-accent Spot. Central LightRay slot capacity does not imply multi-zone accent support; multiple matched Spots require a separately approved bounded GPU contract.
- The new-controller source default remains provisional `0.03`. Existing serialized controllers retain their saved value and AF5H performs no scene, prefab, or serialized migration. AF5G/AF5H visual acceptance and profiling remain pending.


### VEG-V1C.18 — AH LightRay-only accent participation coverage

`WEATHER-LIGHT-RAY-V1.1D-AH` adds one Weather-controller-owned `LightRay Vegetation Accent Coverage` value in the `0..1` range, defaulting to `1`. It does not scale radiance. Instead, each vegetation blade/card receives a stable noninterpolated candidate hash generated from indirect instance ID and one discrete crossed-card index. Only the geometrically matched Weather LightRay Spot applies the threshold. Ordinary directional lights, ordinary punctual lights, `Hut_Warm_Point`, and LightRay body illumination do not read the coverage value.

Production participation is deterministic: `0` selects no candidates, `1` selects all candidates, and intermediate values select approximately that fraction. Diagnostic mode bypasses the participation filter so all correctly matched candidates remain available for geometric and radiance classification. The benchmark shader wrapper is intentionally part of the patch because stable per-card selection cannot be derived safely from fragment world position under wind deformation. Unity validation remains pending.


### WEATHER-LIGHT-RAY-V1.1D-AH1 — whole-card LightRay coverage identity

AH1 corrects the coverage-selection unit without changing the controller or response equation. The AH centreline input varied between the root, middle, and tip vertex rows, allowing separate triangles of one card to be accepted independently. The benchmark crossed-card mesh contains six vertices per card, so the vertex shader now derives `cardIndex = SV_VertexID / 6` and hashes `instanceId + cardIndex`. Every triangle and longitudinal segment of an accepted card retains the complete existing LightRay accent response; rejected cards contribute no LightRay accent. Ordinary lights, body illumination, diagnostic coverage bypass, wind deformation, and accent brightness remain unchanged. Unity validation must confirm that low coverage produces a strict whole-card subset of the full-coverage result.

---

## Weather LightRay multi-Spot accent identity

Multiple authored and procedural LightRay Spots use direct indexed metadata aligned with each camera's URP additional-light order. Runtime LightRay Spots retain the default receiver Rendering Layer mask. The failed registered-bit identity is historical and must not be restored.

## Additional-light accent authority — V1C.12 / V1.2D2

Vegetation evaluates every URP additional light through the existing lighting loop. Project-specific Weather LightRay accent parameters and horizontal source direction are supplied by one two-`float4` indexed sidecar record. Ordinary lights remain generic; Weather Spot proxies receive active-preset overrides. Rendering Layer identity bits, geometric Spot matching, and shader-side LightRay searches are not part of the production contract.

## WEATHER-LIGHT-RAY-V1.2D2 — Protected LightRay edge-accent semantics

The indexed Weather LightRay sidecar now restores the direction contract established before multi-LightRay indexing:

- real Spot direction is used for ordinary vegetation body lighting;
- normalized horizontal celestial/LightRay source direction is used only for Weather blade-edge side selection;
- each camera builds records in its own URP additional-light order;
- the record contains preset-resolved intensity, stable whole-card coverage, selected edge-profile softness, override state, and source direction.

Control semantics are frozen:

```text
Accent Line Intensity
    scales Weather-specific edge radiance only

Vegetation Accent Coverage
    stable whole-card participation probability across the complete footprint
    0 = none, 1 = all directionally eligible cards
    never a radial mask and never a radiance multiplier

Vegetation Accent Softness
    shapes only the selected blade-edge profile
    0.5 preserves the authored edge mask
    never changes card selection, direction eligibility, attenuation, or footprint
```

Production code contains matching protected contract comments at Controller registration, renderer sidecar publication, and shader evaluation boundaries. Future changes must preserve those ownership divisions; they must not infer Weather edge direction from Spot position or `Light.direction`, reuse one camera's light ordering for another, or split the mirrored C#/HLSL record layout.
