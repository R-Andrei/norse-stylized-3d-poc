---
document_id: PS3D-02
title: "Procedural Geometry and Asset Grammars"
version: 0.1
status: draft
scope: generic-framework
authoritative_for: "geometry kernels, seeded recipes, environment generators, modular assemblies, editor tooling, baking and asset curation"
related_documents: [PS3D-00, PS3D-01, PS3D-03, PS3D-04, PS3D-05, PS3D-06]
---

# Procedural Geometry and Asset Grammars

## Context Capsule

This document belongs to the **Programmatic Stylized 3D Framework**: a reusable game-development language for creating artistically deliberate 3D games through procedural geometry, modular construction, shaders, simulation, constrained randomness, authored composition, and tool-assisted iteration.

The framework does **not** propose removing art or manual authorship. It changes the primary artistic instruments. Important composition, meaning, pacing, symbolism, and selection remain human decisions; code and procedural tools provide reusable ways to shape form, motion, atmosphere, and variation.

Concrete creatures, environments, and effects in this document are **illustrative patterns**, not mandatory content for any specific project. Project commitments belong in `05_Project_Application_Norse_Game.md`; the current test scene belongs in `06_Proof_of_Concept.md`.

## Purpose of This Part

This part describes the constructive vocabulary through which a developer can make asset families without relying on a separate conventional modelling workflow for every object.

The emphasis is on **grammars**, not one-off generators. A good generator exposes meaningful design decisions, creates a recognizable family, interoperates with shared materials, and can be used as an editor tool. Runtime generation is optional. Baking a chosen result is often the correct outcome.

The examples cover common natural and architectural forms because they provide a useful test bed. The same geometric operations can also support props, characters, effects, interfaces, or completely different genres.

## Recommended Geometry Kernel

Before building high-level generators, establish a small dependable library of operations:

- vertex, triangle, normal, UV, and colour accumulation;
- rings and ring bridging;
- tapered prisms and low-sided cylinders;
- ribbons sampled from curves;
- 2D profile extrusion;
- polygon filling and surface patch construction;
- mesh merging and material submesh assignment;
- seeded deformation fields;
- controlled normal generation;
- bounds, collider, pivot, and bake utilities.

This kernel should remain boring and predictable. Artistic complexity belongs in recipes and combinations rather than in duplicated low-level mesh code.

## Begin with a Small Primitive Library

The framework can begin from a surprisingly small set of meshes:

- cube;
- beveled cube;
- wedge;
- plane;
- cylinder;
- low-resolution sphere;
- icosphere;
- capsule;
- cone;
- torus segment;
- ribbon;
- extruded polygon;
- line or curve;
- ring of vertices;
- simple profile mesh.

These are not temporary placeholders. They are letters in a visual alphabet.

A raven can be built from wedges and ellipsoids. A hut can be built from panels, beams, and roof planes. A rock golem can be built from distorted icospheres. A shrine can be built from extruded silhouettes and rune curves.

The key is not the complexity of each primitive. It is the system that transforms and combines them.

## Shape Recipes

Every generated object can be represented as a recipe.

A recipe contains:

- seed;
- dimensions;
- proportions;
- style tags;
- material parameters;
- deformation parameters;
- attachment points;
- damage state;
- environmental state;
- optional semantic tags.

For example:

```csharp
public struct RockRecipe
{
    public int Seed;
    public Vector3 Scale;
    public int Subdivisions;
    public float SurfaceNoise;
    public float VerticalCompression;
    public float FractureStrength;
    public float BaseFlattening;
    public float SnowAmount;
    public RockStyle Style;
}
```

Recipes allow the same generator to create object families.

They also allow a manually authored object to remain procedural. The developer can choose the seed and tweak parameters until a particular object feels correct, then save the recipe.

Procedural does not have to mean different every run. It can mean **generated through rules**, even when the result is permanently selected.

---


## Rock Generator

Rocks are an ideal first generator because they appear everywhere and can serve multiple roles.

A simple process:

1. Start from a low-resolution icosphere or convex polyhedron.
2. Apply seeded displacement to vertices.
3. Stretch the shape along one or more axes.
4. flatten the bottom;
5. apply one or more directional fracture operations;
6. optionally cut or compress one side;
7. recalculate normals;
8. assign vertex colors based on height, normal, and noise;
9. generate attachment points or contact zones;
10. let the material system add snow, wetness, moss, soot, or rune light.

The generator can create:

- small stones;
- boulders;
- standing stones;
- cairn pieces;
- cliff fragments;
- mountain silhouettes;
- shrine bases;
- collapsed masonry;
- golem body parts;
- bridge supports;
- carved story stones.

### Distinct rock families

Different rule sets can imply different geological or mythological regions:

- rounded river stones;
- severe fractured mountain stone;
- black volcanic shards;
- pale sacred stone;
- layered slate;
- ice-embedded boulders;
- runic stones with unnatural symmetry;
- corrupted rocks that bend toward a source.

The world gains regional identity without requiring a separate hand-modelled asset for every region.

## Cliff and Mountain Generator

Mountains seen from a controlled camera do not need to be physically complete geological simulations.

A mountain can be constructed from:

- a broad terrain height field;
- layered cliff bands;
- large rock masses;
- fog planes;
- distant silhouette meshes;
- snow masks;
- atmospheric color reduction.

Possible generator inputs:

- ridge spline;
- base width;
- peak height;
- asymmetry;
- erosion frequency;
- cliff probability;
- snow line;
- wind direction;
- visible-camera side;
- silhouette exaggeration.

A ridge can begin as a spline. Around that spline, the system can place and blend large masses. The visible side receives more shape attention than the hidden side.

For distant mountains, the generator can prioritize silhouette over topological realism.

For near cliffs, the generator can create:

- vertical face zones;
- ledges;
- shelves;
- climb-blocking shapes;
- overhang meshes;
- debris fields;
- fog pockets;
- snow cornices.

## Tree Generator

A tree can be defined as a hierarchy of curves.

### Trunk

The trunk is represented by a spline or a sequence of control points.

At intervals along the trunk:

1. generate a ring of vertices;
2. vary ring radius;
3. rotate ring orientation;
4. offset ring center;
5. connect adjacent rings;
6. taper toward the top.

### Branches

Branches can be spawned from trunk samples according to:

- probability;
- height range;
- angle;
- length;
- recursion depth;
- wind bias;
- gravity bias;
- age;
- damage;
- species style.

Each branch is another tapered curve.

### Stylization over botany

The generator should not aim for botanical simulation. It should aim for recognizable families and expressive silhouettes.

Possible tree types:

- narrow conifers with repeated branch tiers;
- ancient trees with enormous trunks and few limbs;
- dead trees with severe angular branching;
- wind-bent ridge trees;
- sacred trees whose branches form circular motifs;
- corrupted trees whose limbs converge inward;
- talking trees with face-like negative spaces;
- root-heavy trees growing around ruins;
- ghost trees with incomplete or floating branches.

### Foliage options

Foliage can be:

- low-poly clusters;
- flat cards;
- convex masses;
- sparse individual leaves;
- particle leaves;
- stylized needles;
- absent entirely.

In a winter setting, trunks, branches, snow, and wind may carry more visual weight than conventional foliage.

## Root Generator

Roots deserve their own system because they can connect environment, lore, traversal, and magic.

A root generator can use splines that:

- travel over terrain;
- sink into terrain;
- wrap around rocks;
- branch toward landmarks;
- pulse with material effects;
- form barriers;
- create bridges;
- reveal pathways;
- invade structures.

Inputs might include:

- origin;
- destination attraction points;
- terrain-follow strength;
- branching probability;
- thickness decay;
- sacred or corrupted style;
- pulse phase;
- snow displacement amount.

Roots can visually imply intention. They can orient toward shrines, corpses, divine relics, or fractures in the world.

## Hut Generator

A hut generator begins with a footprint and a construction grammar.

### Inputs

- width;
- length;
- wall height;
- roof pitch;
- roof overhang;
- door side;
- window count;
- beam spacing;
- wall irregularity;
- lean;
- damage;
- snow load;
- warmth;
- cultural motif set.

### Construction process

1. define the footprint;
2. place corner posts;
3. subdivide walls into panels;
4. create a ridge line;
5. connect wall tops to the ridge;
6. generate roof surfaces;
7. add support beams;
8. cut or omit openings;
9. apply damage by removing or rotating modules;
10. add snow, ropes, charms, carvings, or roots.

The hut does not require sophisticated UV work if it uses:

- triplanar materials;
- world-space texture;
- vertex color;
- palette mapping;
- shared detail normals;
- procedural wear.

### Families from one generator

The same generator can create:

- camp huts;
- abandoned cabins;
- ritual shelters;
- hunter lodges;
- collapsed homes;
- watch posts;
- storage buildings;
- shrines;
- improvised barricades.

The distinction comes from proportions, module selection, material state, and decorative grammar.

## Ruin Generator

Ruins can be generated from structures or built directly from fragments.

A ruin recipe might specify:

- original footprint;
- surviving wall percentage;
- collapse direction;
- age;
- vegetation or root invasion;
- burn state;
- frost state;
- sacred significance;
- debris density;
- traversal openings.

A useful method is:

1. generate an intact abstract structure;
2. mark structural zones;
3. remove or rotate selected modules;
4. create debris from removed modules;
5. expose broken-edge geometry;
6. place snow and dirt according to exposure;
7. create clear traversal and visibility paths;
8. preserve one or two strong silhouettes.

A ruin should not be random rubble. It should retain enough structure to suggest what it was.

## Bridge Generator

A bridge is naturally defined by endpoints and a curve.

### Inputs

- start;
- end;
- width;
- sag;
- plank spacing;
- plank size;
- irregularity;
- rope count;
- support style;
- damage;
- snow;
- age.

### Construction

1. create a center spline;
2. sample it at fixed intervals;
3. place planks perpendicular to the tangent;
4. apply seeded offset and rotation;
5. create side posts;
6. generate rope curves;
7. create supports if the span requires them;
8. mark damaged or missing sections;
9. add collision and navigation metadata.

The same system can create:

- narrow footbridges;
- rope bridges;
- heavy wooden crossings;
- collapsed partial bridges;
- ritual bridges;
- root bridges;
- spectral bridges.

## River Generator

A river begins as a spline through a chunk or sequence of chunks.

The system can generate:

- water surface;
- riverbed depression;
- bank geometry;
- wetness zones;
- stone placement zones;
- vegetation zones;
- foam or flow paths;
- crossing candidates;
- audio emitters;
- fog pockets.

### Inputs

- width;
- depth;
- speed;
- curve tension;
- bank steepness;
- bed material;
- frozen percentage;
- turbulence;
- snow encroachment;
- crossing rules.

A river in a modular world can use sockets at chunk boundaries. The internal spline connects compatible entrance and exit sockets.

The water need not be physically simulated. A world-space flow shader and carefully placed foam can provide the impression of movement.

## Carving and Rune Generator

Carvings can be treated as vector geometry.

A motif library may contain:

- straight segments;
- arcs;
- branching symbols;
- knot-like patterns;
- animal abstractions;
- sacred marks;
- warnings;
- clan or location marks.

These can be represented as:

- curves;
- line segments;
- signed-distance functions;
- masks;
- decal textures generated from vectors;
- shallow extrusions.

A carving system can:

1. select a motif;
2. alter its scale and proportion;
3. project it onto a surface;
4. remove sections based on age;
5. fill it with snow, soot, blood, moss, or light;
6. trigger local material responses;
7. associate it with narrative data.

This allows symbols to be visually consistent and mechanically meaningful.

## Path Generator

Paths can be authored as splines, generated from navigation goals, or both.

The path system can affect:

- terrain height;
- ground color;
- snow depth;
- vegetation exclusion;
- stone placement;
- footprints;
- puddles;
- edge debris;
- NPC movement;
- encounter placement.

A path is therefore not simply a texture painted on terrain. It is a semantic corridor that influences many systems.

---

## The Need for Authoring Tools

The framework should not force every decision into source code.

The best version includes editor tools that expose procedural systems visually.

The developer should be able to:

- drag spline points;
- move sockets;
- paint semantic zones;
- preview seeds;
- lock selected results;
- regenerate only one layer;
- save recipes;
- inspect sightlines;
- edit palette parameters;
- preview snow and wind;
- test chunk compatibility;
- visualize navigation;
- stage actor poses;
- edit action phases.

The tools are where programming and manual authorship meet.

## Generator Inspector

Each generator can have:

- seed field;
- randomize button;
- lock controls;
- sliders;
- style presets;
- regenerate button;
- bake button;
- save recipe button.

A useful pattern is **selective locking**.

For a tree:

- lock trunk;
- regenerate branches;
- lock silhouette;
- regenerate snow;
- lock roots;
- change wind bias.

This makes procedural iteration feel like sculpting with rules.

## The First Asset Library

A viable starting library may contain only:

### Primitive meshes

- cube;
- wedge;
- sphere;
- icosphere;
- capsule;
- cylinder;
- cone;
- plane;
- ribbon;
- extruded polygon.

### Profiles

- mask outlines;
- roof cross-sections;
- blade arcs;
- rune curves;
- branch profiles;
- plank profiles;
- cliff outlines.

### Materials

- stone;
- wood;
- snow;
- ice;
- water;
- spirit;
- shadow;
- rune;
- foliage.

### Effects

- wind trail;
- impact burst;
- radial ground wave;
- snow puff;
- feather burst;
- spirit smoke;
- rune pulse;
- dissolve;
- reconstruction.

This is enough to create an initial visual world.

## Assets as Exceptions, Not Foundations

External or commissioned assets can be used strategically.

Good candidates:

- a central camp shrine;
- one important mask set;
- a signature boss;
- portrait art;
- unique landmarks;
- typography;
- key icons;
- special story objects.

These assets should enter an existing visual framework.

They should not force the rest of the game to imitate an unrelated style.

## Asset Ingestion

Any imported asset can be normalized through a pipeline that:

- sets scale;
- repairs pivots;
- replaces materials;
- applies palette mapping;
- adds snow parameters;
- configures collision;
- creates prefab variants;
- assigns semantic tags;
- validates complexity;
- generates thumbnails.

The asset becomes raw geometry inside the game’s visual language.

---

## Generator Quality Criteria

A generator is useful when:

1. Its parameters correspond to visible concepts.
2. Its output remains within a recognizable family.
3. It can produce both ordinary and exceptional variants.
4. Failures are detectable and correctable.
5. It exposes enough structure for shared shaders and world rules.
6. It supports deterministic regeneration.
7. It can be curated, duplicated, saved as a recipe, or baked.
8. It does not require runtime generation when editor-time generation is sufficient.

A generator is not useful merely because it produces many shapes. Quantity without direction is noise.
