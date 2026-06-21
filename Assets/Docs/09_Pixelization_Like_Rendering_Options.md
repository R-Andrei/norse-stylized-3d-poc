---
document_id: PS3D-09
title: "Pixelization-Like Rendering Options"
version: 0.1
status: decision-study
scope: project-rendering
authoritative_for: "viable pixelization-like rendering techniques, project compatibility, performance trade-offs, and recommended prototype experiments"
related_documents: [PS3D-01, PS3D-05, PS3D-06, PS3D-08]
last_updated: 2026-06-22
---

# Pixelization-Like Rendering Options

## Purpose

This document records rendering techniques that can give the project a pixel-art-influenced or digitally quantized appearance without requiring hand-painted textures. It only includes approaches that can plausibly apply across the whole project: generated terrain, rocks, buildings, props, units, snow, lighting, and heavy spell visual effects.

The project currently uses Unity 6.5 with URP and Shader Graph. Its visual framework relies on procedural geometry, shared material behaviour, dynamic time-of-day lighting, local lights, environmental overlays, and effects that may function as part of actor anatomy. The preferred solution must therefore preserve dynamic lighting and remain usable across both opaque surfaces and purpose-built VFX shaders.

## Evaluation criteria

Each option is assessed against:

- applicability to ground, buildings, props, and units;
- compatibility with transparent and emissive spell effects;
- compatibility with many dynamic lights and dark scenes;
- support for procedural geometry without authored UVs;
- compatibility with snow, wetness, corruption, frost, and other material states;
- runtime cost and scalability;
- temporal stability while the camera and objects move;
- ability to remain fully or mostly programmatic;
- ability to use one coherent visual language without forcing every material to look identical.

## Viable technique families

| Technique | Role | World geometry | Units | Heavy VFX | Many lights | Performance | Main strengths | Main risks |
|---|---|---|---|---|---|---|---|---|
| Shared material-space pixel cells and palette quantization | Primary style system | Excellent | Excellent | Requires a sibling VFX shader | Excellent when standard URP lighting is preserved | Low to moderate | UV-free, procedural, stable on moving objects, compatible with snow and material states | Repetitive or noisy if cell scale and palette ranges are poorly controlled |
| Generated tiny textures with nearest filtering | Primary or secondary surface system | Excellent with triplanar or generated UVs | Good | Good through dedicated effect textures | Excellent | Low for simple sampling; moderate with triplanar projection | True texel character, deterministic generation, cheap reusable assets | Visible tiling, generated-asset management, triplanar sampling cost |
| Per-face or vertex-palette variation | Supporting layer | Excellent | Excellent | Limited | Excellent | Very low | Reinforces procedural geometry, almost free, deterministic | Looks polygonal rather than pixelated when used alone |
| Quantized diffuse lighting or lighting ramps | Supporting or primary lighting style | Excellent | Excellent | Requires a separate VFX lighting model | Medium to high implementation risk with complex light accumulation | Moderate | Strong illustrated look, unified value structure, readable forms | Hard bands, temporal popping, custom-lighting complexity, possible conflict with many lights |
| Ordered or blue-noise dithering | Supporting layer | Good | Good | Excellent | Excellent | Low | Represents intermediate tones with a pixel-art vocabulary; useful for fades and transparency | Shimmer, crawling patterns, and visual noise if too fine or screen-locked |
| Low-resolution scene rendering with nearest-neighbour upscale | Global style layer | Excellent | Excellent | Excellent | Excellent | Often reduces pixel cost, but exact gain depends on bottleneck | Applies to the whole scene automatically, including lights and effects | Camera shimmer, disappearing thin effects, coarse shadows, UI separation, project-wide commitment |
| Full-screen palette quantization and dither | Global finishing layer | Excellent | Excellent | Excellent | Excellent because it operates after scene lighting | Low to moderate | Cohesive final image, easy to toggle, no per-asset setup | Can crush emissive gradients, muddy colour-coded gameplay effects, and damage night readability |
| Major-edge or chip accents | Supporting geometry/material layer | Excellent | Excellent | Limited | Excellent | Low to moderate | Clarifies cuts, chips, silhouettes, and snow-catching edges | Requires mesh metadata and must avoid outlining every triangulation edge |
| Pixel-aware VFX shader family | Required companion system | Not applicable | Not applicable | Excellent | Depends on chosen VFX lighting model | Low to moderate per effect | Keeps trails, smoke, particles, telegraphs, and emission in the same visual language | Cannot be solved by the opaque material alone; transparency and bloom need separate tuning |
| Hybrid material plus global finish | Complete project strategy | Excellent | Excellent | Excellent | Excellent | Configurable | Combines stable asset-level style with optional scene-wide cohesion | More systems to tune; effects can become over-processed if both layers are strong |

## Technique details

### 1. Shared material-space pixel cells and palette quantization

A shared opaque shader evaluates a snapped object-space or world-space position. Each cell receives a deterministic palette value, roughness value, or material-state bias.

Recommended uses:

- object space for moving units, props, and generated rocks so the pattern remains attached to the object;
- world space for large ground surfaces and structures when continuity across adjacent meshes is desirable;
- material-specific cell scale so a house, unit, and ground patch share a visual language without using identically sized blocks;
- clustered cells rather than independent white-noise cells;
- narrow palette ranges around a material family rather than unrestricted random colours;
- low smoothness and restrained specular response;
- snow, frost, corruption, wetness, and rune emission applied as later layers.

The safest first version should preserve the normal URP Lit lighting path. It should quantize material inputs and surface breakup before lighting, rather than immediately replacing URP's full light accumulation with a custom toon-lighting model.

### 2. Generated tiny textures with nearest filtering

Small textures such as 8x8, 16x16, or 32x32 can be generated from seeds and sampled with point filtering. These may store colour variation, roughness, mineral patterns, or dither masks.

Projection choices:

- generated UVs for objects whose generator already owns topology;
- object-space triplanar projection for irregular procedural meshes;
- world-space projection for terrain continuity;
- texture arrays for multiple material families.

This option provides authentic texel structure while remaining artist-independent. It is more asset-oriented than pure shader cells and becomes more expensive when triplanar projection requires multiple texture samples.

### 3. Per-face or vertex-palette variation

The generator writes deterministic material information into vertex colours or another mesh channel. Broad faces can become slightly darker, lighter, colder, warmer, chipped, snow-prone, or mineral-rich.

This is an excellent low-cost layer for rocks, buildings, ground tiles, and rigid-part units. It should support another technique rather than act as the entire pixelization solution.

### 4. Quantized lighting

Diffuse lighting is reduced to a small number of value bands or sampled from a ramp. This can create a strong illustrated or pixel-art influence.

For this project, full custom quantized lighting should be treated carefully because the game expects dynamic time-of-day changes, many local lights, emissive spells, and potentially VFX Graph. A first implementation should not discard URP lighting merely to obtain hard bands. Safer experiments include mild post-lighting value quantization, material-side palette restriction, or quantizing only selected lighting terms.

### 5. Dithering

Dithering alternates between allowed colours or alpha levels to represent intermediate values.

Good uses:

- dissolves;
- spirit transparency;
- spell fades;
- snow or corruption transitions;
- optional lighting-band transitions;
- low-resolution shadow or fog transitions.

Object-space or stable world-space dithering is less likely to crawl across geometry. Screen-space dithering is useful for full-screen cohesion and transparent effects but must be tested for camera shimmer.

### 6. Low-resolution scene rendering

The scene is rendered below the final display resolution and upscaled with nearest-neighbour filtering. This automatically affects geometry, lights, shadows, transparent effects, particles, fog, and post-processing.

This is the strongest global connection to traditional pixel art. It is also the most consequential option. It may simplify the rendered image and lower fragment cost, but it can make thin trails disappear, produce camera shimmer, coarsen shadow boundaries, and complicate UI rendering.

It should be tested as an optional quality/style mode, not adopted before motion, combat effects, night lighting, and UI have been observed at representative resolutions.

### 7. Full-screen palette quantization and dither

A URP full-screen pass can remap the final scene into restricted value or colour steps and optionally apply dithering.

This is less geometrically pixelated than low-resolution rendering, but it can unify all scene elements. It is particularly useful as a mild finishing layer.

The effect must protect:

- gameplay telegraph colours;
- important emissive spell values;
- dark-scene silhouette readability;
- snow highlights;
- warm and sacred accents.

Aggressive final-image quantization is likely to be harmful. A restrained pass may be useful.

### 8. Major-edge and chip accents

Generated meshes can identify important macro edges rather than every triangulation edge. The shader can then lighten exposed chips, darken creases, collect snow, or apply a material-state bias.

This adds a carved or weathered quality but requires additional mesh metadata. It is a later refinement, not the first pixelization experiment.

### 9. Pixel-aware VFX shaders

No opaque world shader can style every particle, trail, smoke volume, telegraph, and transparent spirit automatically.

The project should eventually have a sibling effect material language sharing:

- palette roles;
- pixel or dither scale;
- quantized masks;
- emission ranges;
- alpha clipping or dithered transparency;
- distortion limits;
- snow, wind, corruption, and divine-state inputs where relevant.

URP Shader Graph can support VFX Graph-compatible shaders, so this can remain part of the same programmatic material framework rather than becoming an unrelated effects pipeline.

## Compatibility by content type

| Content type | Best-fit techniques |
|---|---|
| Ground and terrain | World-space pixel cells, generated tiny textures, per-vertex variation, optional low-resolution rendering |
| Rocks and debris | Object-space pixel cells, per-face variation, low smoothness, later edge accents |
| Houses and ruins | Object- or world-space cells by module, generated wood/stone patterns, vertex masks, material-state overlays |
| Units and rigid-part actors | Object-space cells, palette families, per-part seed variation, restrained dithering |
| Deforming or spline actors | Object-space cells where stable, effect-shader variants where geometry changes heavily |
| Snow and frost | World-space accumulation combined with quantized breakup and optional dithered boundaries |
| Trails and telegraphs | Dedicated VFX shader using quantized masks, point-sampled patterns, and carefully preserved gameplay colours |
| Smoke, fog, and volumetric-looking effects | VFX-specific shader, optional six-way lighting, low-frequency dither or quantization |
| Heavy magical emission | Dedicated effect shader; avoid aggressive final palette clipping that destroys gradients and colour identity |
| UI and text | Native-resolution rendering; generally exclude from scene pixelation unless intentionally designed for it |

## Performance and lighting considerations

- A small amount of arithmetic for snapped coordinates, hashing, palette selection, and roughness breakup is normally a reasonable cost for a shared opaque material.
- World-space and object-space procedural cells avoid texture-memory pressure but consume shader arithmetic.
- Generated tiny textures reduce arithmetic and are cheap to sample, but triplanar projection increases sample count.
- Vertex and face metadata are extremely cheap and should be used wherever the generator already controls the mesh.
- A low-resolution scene render can reduce fragment shading cost, which may help with overdraw-heavy effects and many lights, but it does not solve CPU-side light, particle, draw-call, or simulation costs.
- A full-screen pass adds another screen-sized operation and should remain simple.
- Transparent VFX remain a major performance concern because of overdraw regardless of pixel-art treatment.
- The opaque material should initially preserve URP Lit behaviour so it continues to work with the project's dynamic directional light and local lights.
- Custom lighting quantization should be deferred until the material-space look has been judged under representative day, night, hearth, aura, and spell-light conditions.

## Ranked recommendation

### Rank 1 - Shared material-space pixel language

Build a common opaque material system using:

- object-space or world-space snapped cells;
- material-family palettes;
- low smoothness;
- subtle per-face or vertex variation;
- clustered deterministic breakup;
- environmental overlays such as snow;
- standard URP lighting retained initially.

Why it ranks first:

- it scales across terrain, buildings, props, and units;
- it requires no hand-painted textures or UV authoring;
- it remains stable on moving objects;
- it is compatible with procedural geometry;
- it does not automatically damage spell effects or UI;
- it can be extended gradually;
- it preserves the existing many-light and day-night foundation.

### Rank 2 - Mild low-resolution scene rendering

Test a lower internal render resolution with nearest-neighbour upscale as an optional global layer.

Why it ranks second:

- it automatically affects geometry, VFX, lights, fog, and shadows;
- it can provide strong final cohesion;
- it may reduce fragment cost;
- it is easy to compare on and off.

Why it is not rank 1:

- it is a project-wide presentation decision;
- it can cause shimmer and erase thin effects;
- it complicates UI and detailed telegraphs;
- it does not replace the need for good materials;
- heavy application may conflict with the desired dynamic lighting richness.

### Supporting layers

Use per-face or vertex variation immediately because it is cheap. Test restrained dithering and final-image palette control later. Defer edge accents and fully custom quantized lighting until the shared material and representative VFX exist.

## Recommended prototype experiments

### Experiment A - Shared pixel-faceted opaque material

Apply one prototype material to:

- three generated masses;
- the ground;
- one hut wall;
- one roof or wooden beam;
- the player proxy.

The material should expose:

- coordinate mode: object or world;
- cell scale;
- palette family;
- breakup strength;
- clustering;
- smoothness;
- per-object seed;
- vertex-colour influence;
- snow amount.

Test under:

- midday directional lighting;
- dark night lighting;
- hearth light;
- player aura light;
- at least one saturated spell-like point light.

Acceptance questions:

- Do all objects belong to one world without appearing to share the same substance?
- Does the pattern remain stable while units move?
- Are spell colours preserved?
- Does the ground avoid obvious repetition?
- Do the rocks stop looking polished?
- Is the scene readable at the gameplay camera distance?

### Experiment B - Optional low-resolution global pass

After Experiment A works, test several internal-resolution levels with nearest-neighbour upscale.

Keep UI at native resolution.

Test:

- slow camera tracking;
- rapid player movement;
- thin weapon trails;
- radial telegraphs;
- small particles;
- bloom;
- dark silhouettes;
- overlapping local lights;
- snow and fog.

Acceptance questions:

- Does motion shimmer become distracting?
- Do telegraphs remain mechanically readable?
- Do thin effects disappear?
- Does the scene gain cohesion beyond what the material already provides?
- Is the performance improvement meaningful on the target hardware?
- Is the result worth making a project-wide constraint?

## Current decision

Proceed first with the shared material-space pixel language. Preserve standard URP Lit lighting in the initial implementation. Treat low-resolution scene rendering as the second experiment and as an optional global layer until combat VFX, night lighting, and UI prove that it is safe.

Do not yet commit to:

- fully custom toon lighting;
- aggressive full-screen palette reduction;
- detailed procedural normal maps;
- visible outlines on every triangle;
- mandatory low-resolution rendering;
- one universal pixel scale for every material.

## Sources and implementation references

- Existing framework: `01_Visual_Language_and_Rendering.md`
- Proof-of-concept requirements: `06_Proof_of_Concept.md`
- Current implementation state: `08_Proof_of_Concept_Implementation_Log.md`
- Unity 6.5 URP Lit Shader Graph documentation
- Unity 6.5 URP Fullscreen Shader Graph and Full Screen Pass Renderer Feature documentation
- Unity URP render-scale and nearest-neighbour upscaling documentation
- Unity VFX Graph and Shader Graph compatibility documentation
