# PS3D Stylized Vegetation Architecture

## Status

- **Domain:** procedural vegetation, wind response, interaction, and snow accumulation
- **Current implementation:** exploratory document only; no canonical vegetation runtime yet
- **Purpose:** outline a plausible shared architecture for dense stylized vegetation that remains reactive, snow-aware, and performant without requiring a large authored asset library
- **Authority:** existing project files and framework documents remain authoritative until a vegetation runtime is actually implemented

This document is intentionally earlier and looser than the river Foam architecture. It describes a likely direction, identifies the strongest architectural patterns, and records alternatives and curiosities worth remembering later. It does **not** lock exact data formats, class names, shader contracts, or pass-by-pass implementation work.

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
    + shared wind field
    + shared interaction field
    + shared environmental state inputs
    + fixed-cost vegetation shader logic
```

This means:

- geometry variety comes from procedural recipes and instance variation;
- motion comes mostly from shared world-space signals;
- interaction comes mostly from one or more persistent or semi-persistent fields;
- snow comes mostly from shared shading and state logic rather than bespoke meshes;
- the final renderer never loops over arbitrary counts of active movers or abilities.

This is likely the only direction that satisfies "dense," "interactive," and "performant" at the same time.

---

## 4. What the system represents

The final vegetation result does not need to represent a physically exact set of independent plants.

Instead, it can represent several layers of abstraction:

- **instance families:** visible clumps, stalks, shrubs, saplings, branch clusters, and ground cover patches;
- **wind response:** broad directional sway, gusting, turbulence, and local phase variation;
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

### 7.1 Preferred direction: one shared wind field

The framework docs already point toward wind as a shared field. Vegetation should follow that principle.

A likely wind stack is:

- global prevailing direction and strength;
- low-frequency gust modulation;
- medium-frequency turbulence or curl noise;
- optional local modifiers from terrain, rivers, cliffs, or authored volumes.

The field does not need to be a large full-resolution simulation immediately. Early versions may use:

- one global vector plus sampled world-space noise;
- a few layered directional bands;
- a compact chunked vector texture;
- or a hybrid where the "field" is analytic rather than stored.

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

### 8.1 Preferred direction: one shared vegetation interaction field

The strongest candidate is a river-style interaction field for vegetation.

This field could store concepts such as:

- bend direction;
- bend magnitude;
- depression or flattening amount;
- impulse age;
- rebound or recovery bias;
- optional effect category such as ordinary movement versus ability shock.

The field should be chunked and bounded, not global full-resolution over the entire world.

### 8.2 Sources

Likely interaction sources:

- player movement;
- enemy movement;
- footsteps or body sweep;
- dashes;
- rolls or slides;
- ground slams;
- explosions;
- spell pulses;
- large projectiles;
- moving heavy objects.

These should stamp into the field rather than directly updating thousands of instances.

### 8.3 Interaction vocabulary

The visible response can vary by source category:

- ordinary traversal: short-lived parting and bend;
- heavier bodies: deeper local flattening and slower rebound;
- ground slam: radial depression with secondary ripple or rebound ring;
- magical or elemental abilities: alternative bend color, frost shedding, scorch suppression, or directional burst patterns.

### 8.4 Persistent versus temporary reaction

Not all interaction needs the same lifetime.

Possible split:

- **ephemeral bend:** fades in fractions of a second to a few seconds;
- **temporary memory:** flattened or disturbed region survives longer;
- **authored special state:** ability leaves behind a frost-bitten, scorched, or trampled mask that persists until cleaned or regenerated.

The canonical baseline should likely focus on ephemeral bend plus a modest temporary memory. Longer-lived ground-state changes can come later.

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
- shared interaction field;
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
- quality-scaled interaction-field resolution and update frequency;
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

Most interactions should share one field, but some exceptional abilities may justify authored overlays:

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

### V0 - visual probes

- build a few procedural or baked clump archetypes;
- test instanced placement density;
- test shader-only wind on representative terrain;
- test whether the camera distance favors bold ribbons, cards, shells, or mixed families.

### V1 - shared wind foundation

- define canonical wind inputs;
- give all near-field vegetation one coherent wind language;
- validate silhouette readability and overdraw.

### V2 - interaction field

- add one shared vegetation interaction field;
- stamp player motion and one strong ability into it;
- validate parting, bend, flattening, and rebound.

### V3 - snow integration

- add shader-driven snow response;
- test ordinary winter scenes, exposed ridges, and interaction-driven shedding;
- confirm that the result reads as intentional rather than as white paint.

### V4 - chunking, LOD, and world integration

- connect coverage generation to terrain, rivers, rocks, and exclusion zones;
- add chunk-based culling and quality scaling;
- introduce distant coverage language.

### V5 - optional specialization

- family-specific motion tuning;
- shrub and sapling variants;
- advanced ability reactions;
- persistent trampling or seasonal state if still desired.

---

## 15. Working recommendation

If the project wants a strong result with the least risk, the best current recommendation is:

1. use a small generated or baked library of bold vegetation clumps rather than many unique assets;
2. render them through instancing and chunked placement;
3. drive them with one shared wind field and one shared vegetation interaction field;
4. treat snow primarily as a shared shading and state problem rather than a per-plant geometry simulation;
5. let far vegetation become stylized coverage rather than insisting on literal distant plants.

This direction aligns with the framework's broader preferences:

- procedural families instead of asset catalogues;
- shared field logic instead of per-object logic;
- stylized coherence instead of realism for its own sake;
- bounded cost instead of unscalable simulation.

The next concrete step, if desired later, would be a narrower architecture document for a first playable vegetation slice rather than a full final system.
