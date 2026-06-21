---
document_id: PS3D-04
title: "World Construction and Generative Assembly"
version: 0.1
status: draft
scope: generic-framework
authoritative_for: "terrain representations, modular chunks, sockets, graph-first generation, semantic placement, deterministic variation, stable hubs and variable regions"
related_documents: [PS3D-00, PS3D-01, PS3D-02, PS3D-03, PS3D-05, PS3D-06]
---

# World Construction and Generative Assembly

## Context Capsule

This document belongs to the **Programmatic Stylized 3D Framework**: a reusable game-development language for creating artistically deliberate 3D games through procedural geometry, modular construction, shaders, simulation, constrained randomness, authored composition, and tool-assisted iteration.

The framework does **not** propose removing art or manual authorship. It changes the primary artistic instruments. Important composition, meaning, pacing, symbolism, and selection remain human decisions; code and procedural tools provide reusable ways to shape form, motion, atmosphere, and variation.

Concrete creatures, environments, and effects in this document are **illustrative patterns**, not mandatory content for any specific project. Project commitments belong in `05_Project_Application_Norse_Game.md`; the current test scene belongs in `06_Proof_of_Concept.md`.

## Purpose of This Part

This part describes how authored assets become places and how places become a world. It separates several concerns that are often incorrectly collapsed into one “procedural generation” system:

- world topology and route structure;
- terrain representation;
- modular region selection;
- semantic meaning of local areas;
- gameplay placement;
- environmental dressing;
- deterministic variation;
- streaming and persistence.

The framework favours **graph first, geometry second, decoration last**. Important landmarks, routes, encounter roles, and narrative relationships should be established before selecting meshes or scattering props.

A stable hub connected to changing expeditions is one useful pattern, not a universal requirement. The same techniques can support authored worlds with generated pockets, branching journeys, procedural dungeons, changing open regions, or fixed maps with state-driven redressing.

## Authored Mesh Terrain

The simplest reliable approach is a mesh ground surface created per chunk or region.

It may be edited manually and dressed procedurally.

Advantages:

- high compositional control;
- easy socket matching;
- predictable navigation;
- support for overhangs and non-height-field geometry;
- easy integration with ruins and cliffs;
- fewer runtime generation problems.

This approach is excellent for a fixed camp and useful for hand-designed chunk templates.

## Recipe-Generated Mesh Terrain

A more procedural approach generates a mesh from a recipe.

A height function might combine:

```text
height =
    broad authored shape
  + low-frequency terrain noise
  + ridge influence
  - river depression
  - path flattening
  + landmark adjustment
  + edge stitching
```

The recipe can contain:

- base elevation;
- height control points;
- ridge splines;
- river splines;
- path splines;
- cliff zones;
- flattening zones;
- landmark anchors;
- noise parameters;
- edge constraints.

This is likely the most balanced approach.

It allows procedural variation while preserving authored structure.

## Selective Density-Field Terrain

Some areas may benefit from smooth volumetric terrain:

- glacier caves;
- tunnels;
- root interiors;
- corrupted pits;
- giant hollow trees;
- collapsing snow chambers;
- supernatural pockets.

These can exist as special chunks or subregions.

The overall world can remain mesh-based while specific spaces use density fields.

## Terrain as Layered Representation

The visible world does not need one universal terrain technology.

A single scene can combine:

- a generated ground mesh;
- separate cliff meshes;
- rock masses;
- spline rivers;
- snow overlays;
- decals;
- fog;
- distant silhouette cards;
- voxelized magical fragments.

The player perceives one landscape. The implementation can use several representations chosen for their strengths.

---

## The Stable Hub Pattern

The camp is the stable point in a world that changes outside it.

It can function as:

- emotional home;
- narrative hub;
- place of recovery;
- visual benchmark;
- repository of persistent change;
- contrast to procedural wilderness.

The camp itself should be manually composed.

Its objects can still be procedurally generated, but their final placement and chosen variants can be fixed.

## Example Hub Composition

The camp may contain:

- a central fire or sacred object;
- two to four huts;
- a broken shrine;
- a blacksmith represented by floating armor;
- a raven near the exit;
- a mask spirit near the fire;
- a talking tree or sign;
- a river edge or small bridge;
- a distant mountain view;
- the path into the mutable wilderness.

The exact arrangement should serve narrative and navigation.

## Building a Hub from Generators

### Huts

Each hut uses the same generator but different saved recipes.

```text
Hut A
- broad roof
- low walls
- heavy snow
- warm light
- structurally intact

Hut B
- narrow footprint
- steep roof
- damaged wall
- hanging charms
- cold interior

Hut C
- partially collapsed
- roots entering the floor
- abandoned
- used for a later narrative purpose
```

### Paths

Splines connect:

- fire;
- huts;
- shrine;
- NPC stations;
- exit.

The path system:

- lowers snow;
- changes ground material;
- suppresses vegetation;
- places stones;
- guides footprints;
- supports NPC movement.

### Permanent NPCs

NPCs can use procedural puppet types:

- floating armor;
- raven;
- mask spirit;
- tree;
- signpost;
- object spirit.

### Background

Distant mountains can use simplified silhouette meshes and fog.

They need not be part of traversable geometry.

## Hub Evolution

The camp can change through state rather than through complete rebuilding.

Possible changes:

- huts are repaired;
- snow recedes or deepens;
- roots spread;
- new masks appear at the shrine;
- NPC effects change;
- paths become more travelled;
- carvings awaken;
- the central fire changes color or form;
- distant silhouettes shift;
- new objects are added;
- old objects decay.

The hub becomes a visual state machine.

---


## The Desired Experience

The player leaves the camp and enters a world that is different on each expedition without becoming meaningless.

The wilderness may contain:

- trees;
- mountains;
- rivers;
- ravines;
- bridges;
- carvings;
- huts;
- ruins;
- shrines;
- clearings;
- signs of former civilization;
- enemy encounters;
- narrative discoveries.

The procedural system should create variation in route, composition, and encounter sequence while preserving the authored identity of each location type.

## Chunk-Based World Construction

The world can be assembled from authored chunk definitions.

A chunk is not merely a square room. It is a semantic region containing:

- terrain recipe;
- connection sockets;
- path splines;
- river splines;
- landmark slots;
- encounter slots;
- decoration zones;
- visibility rules;
- biome tags;
- difficulty tags;
- narrative tags.

Example:

```csharp
public class WorldChunkDefinition : ScriptableObject
{
    public Vector2 Size;
    public ChunkSocket[] Sockets;
    public BiomeTag[] AllowedBiomes;
    public ChunkTag[] Tags;
    public TerrainRecipe Terrain;
    public SplineRecipe[] Paths;
    public SplineRecipe[] Rivers;
    public PlacementZone[] DecorationZones;
    public LandmarkSlot[] LandmarkSlots;
    public EncounterSlot[] EncounterSlots;
    public int Weight;
}
```

## Sockets

Sockets describe compatible connections.

```csharp
public struct ChunkSocket
{
    public string Id;
    public SocketType Type;
    public Vector3 LocalPosition;
    public Vector3 LocalForward;
    public float Width;
    public float Elevation;
}
```

Possible socket types:

- path;
- river in;
- river out;
- cliff pass;
- cave;
- bridge approach;
- narrow gate;
- biome transition;
- camp exit;
- boss entrance.

Compatibility can consider:

- type;
- width;
- elevation;
- direction;
- biome;
- rotation allowance;
- narrative requirement;
- distance from camp.

## Graph First, Geometry Second

The world should first be generated as an abstract graph.

Example:

```text
Camp
  -> Forest Path
  -> Fork
      -> Shrine
      -> River Crossing
          -> Ruins
          -> Mountain Ascent
```

The graph determines:

- critical route;
- optional branches;
- route length;
- landmark spacing;
- difficulty progression;
- biome progression;
- boss placement;
- return routes;
- narrative beats.

Only after the graph is valid are physical chunks selected.

This prevents the world from becoming a random chain of compatible entrances.

## Chunk Families

Useful chunk families include:

- camp transition;
- dense forest;
- sparse forest;
- open clearing;
- river bank;
- bridge;
- ravine;
- ruined settlement;
- shrine;
- hut;
- mountain path;
- frozen marsh;
- cave;
- dead end;
- treasure space;
- narrative landmark;
- combat arena;
- transition region.

Each family can have several templates and many parameter variations.

## Hiding Seams

Chunk boundaries should not read as obvious tile edges.

Methods include:

- irregular playable shapes inside logical bounds;
- overlapping rocks and vegetation;
- fog pockets;
- terrain skirts;
- transition strips;
- spline-continuous paths;
- spline-continuous rivers;
- matching edge height profiles;
- trees or cliffs near boundaries;
- camera framing;
- gradual material blending.

The chunk is a generation unit, not necessarily a visible unit.

---


## Placement Zones

Each chunk can contain authored semantic zones:

- dense forest;
- sparse forest;
- path exclusion;
- landmark sightline;
- combat clearance;
- river bank;
- exposed ridge;
- sheltered hollow;
- rock field;
- ruin debris;
- secret-space boundary;
- sacred ground;
- corrupted ground.

These zones tell the decorator what a location means.

## Rule-Based Placement

A tree candidate may be accepted when:

```text
inside forest zone
outside path exclusion
outside landmark sightline
slope below threshold
distance from large tree above threshold
surface supports vegetation
density test succeeds
```

Its form may then depend on context:

```text
near river       -> wider, darker
high elevation   -> shorter, wind-bent
near corruption  -> twisted
near shrine      -> roots orient toward shrine
exposed ridge    -> windward snow reduced
sheltered hollow -> deeper snow and fog
```

This creates environmental logic.

## Meaningful Signs of Civilization

Civilization should not appear as uniformly scattered props.

A hut implies:

- a path;
- nearby chopped wood;
- a view or resource;
- a bridge or crossing;
- debris;
- a reason for its location.

A carving implies:

- visibility;
- ritual purpose;
- a route;
- a territorial boundary;
- a warning;
- a narrative connection.

A bridge implies:

- two approach paths;
- a crossing need;
- bank geometry;
- maintenance or abandonment state.

The procedural system should place related objects as compositions.

## Prop Clusters

Instead of placing individual props independently, generate clusters.

Examples:

### Abandoned camp cluster

- fire ring;
- broken shelter;
- scattered bowl;
- wood pile;
- footprints ending abruptly;
- one carved warning.

### River crossing cluster

- bridge;
- approach stones;
- broken railing;
- wet bank;
- downstream debris;
- nearby sign.

### Shrine cluster

- central stone;
- radial smaller stones;
- cleared snow;
- roots;
- offerings;
- altered fog;
- enemy or NPC placement rule.

Clusters preserve implied history.

## Deterministic Seeds

Different generation layers should use derived seeds.

```text
World seed
Chunk selection seed
Terrain seed
Vegetation seed
Prop seed
Encounter seed
Weather seed
```

Conceptually:

```csharp
int vegetationSeed = Hash(worldSeed, chunkCoordinate, "vegetation");
int encounterSeed  = Hash(worldSeed, chunkCoordinate, "encounters");
```

This allows one layer to change without reshuffling everything else.

## Gameplay Before Decoration

Generation order should prioritize meaning and playability:

1. graph;
2. chunks;
3. terrain;
4. paths and rivers;
5. landmarks;
6. traversal;
7. encounters;
8. navigation;
9. major props;
10. decorative dressing;
11. atmosphere.

Decoration must respond to gameplay, not obstruct it by accident.

---


## Expedition Start

The player leaves the fixed camp through a known exit.

The generator has already created an abstract route:

```text
Camp Exit
-> Wind-Bent Forest
-> Frozen Stream
-> Optional Carved Stone
-> Abandoned Hut
-> Ravine Combat
-> Shrine
```

## Physical Chunk Selection

The system selects:

- one camp-transition chunk;
- one forest chunk with path-in and path-out;
- one stream chunk;
- one branch chunk for the optional stone;
- one hut clearing;
- one ravine arena;
- one shrine terminus.

Sockets are matched.

## Terrain Generation

Each selected chunk has a terrain recipe.

The forest chunk:

- broad shallow valley;
- central path spline;
- dense tree zones at edges;
- one exposed rock shelf;
- slight downward slope toward the stream.

The stream chunk:

- river spline;
- depressed bed;
- crossing point;
- wet-bank zones;
- fog accumulation.

The hut clearing:

- flattened landmark zone;
- saved hut recipe;
- approach path;
- view corridor;
- debris cluster.

## Dressing

The forest receives:

- wind-bent trees;
- sparse dead branches near the path;
- deeper snow in sheltered zones;
- exposed roots near the rock shelf;
- a few raven perches.

The stream receives:

- rounded rocks;
- partially frozen surface;
- wet dark materials;
- drifting mist;
- a generated plank crossing or stepping stones.

The hut clearing receives:

- one damaged hut;
- a wood pile;
- tracks;
- a carving;
- a small interaction or encounter.

## Combat

The ravine encounter may contain:

- one floating armor enemy;
- two blob-like lesser enemies;
- restricted space;
- clear telegraph visibility;
- cliff walls that catch impact dust;
- wind direction that aligns with the arena.

The encounter is authored as a combat composition even though the surrounding forms vary.

## Narrative Variation

The shrine may appear in several states:

- dormant;
- corrupted;
- occupied by a ghost;
- covered in offerings;
- fractured;
- partially voxelized;
- connected to a camp-state change.

The chunk is structurally familiar but narratively variable.

---

## Semantic Zone Painting

A chunk editor can allow painting or placing:

- forest density;
- exclusion;
- sacred influence;
- corruption;
- exposure;
- shelter;
- landmark visibility;
- debris probability;
- snow depth.

These maps do not need high resolution. They are conceptual fields.

## Chunk Validation

An editor validator can check:

- sockets align;
- path widths match;
- elevation differences are acceptable;
- encounter space is clear;
- landmarks are visible;
- required navigation exists;
- decoration zones do not block objectives;
- chunk bounds are respected;
- river flow direction is valid.

This converts fragile manual knowledge into repeatable rules.

## From One Scene to a Stable Hub

Once the clearing works:

1. create a manually composed camp;
2. generate and select hut recipes;
3. add permanent NPC puppets;
4. create camp-state variations;
5. establish the final camera and lighting language;
6. introduce narrative portrait presentation.

## From Hub to One Expedition

Then build:

- one graph;
- five to eight chunk families;
- one river crossing;
- one optional branch;
- one ruin;
- one shrine;
- two enemy families;
- one elite encounter;
- one return condition.

This tests the roguelike elements without defining the entire game as a roguelike.

## From Expedition to World Grammar

Later, add:

- biome transitions;
- difficulty rules;
- narrative state;
- more chunk families;
- more actor grammars;
- selective voxel events;
- camp evolution;
- world-state materials;
- unique landmarks.

The world grows by extending vocabularies, not by abandoning the framework.

---


## A Game Built from Grammars

The project can be understood as several interacting grammars.

### Geometry grammar

How forms are generated and combined.

### Material grammar

How surfaces respond to world state.

### Motion grammar

How actors anticipate, move, strike, recoil, and recover.

### Effect grammar

How wind, snow, runes, smoke, and impact behave.

### Composition grammar

How landmarks, paths, open spaces, and silhouettes are arranged.

### World grammar

How chunks connect and how expeditions are structured.

### Narrative grammar

How places and objects communicate history.

The game’s visual identity emerges from agreement between these grammars.

## The Developer as Toolmaker and Art Director

The programmer is not outsourcing artistic responsibility to randomness.

The programmer is:

- choosing constraints;
- building instruments;
- selecting results;
- rejecting weak results;
- staging compositions;
- tuning motion;
- controlling rhythm;
- defining symbols;
- deciding what the world emphasizes;
- deciding what the world hides.

That is authorship.

## Procedural Does Not Mean Automatic

Procedural systems can be used manually.

A generated hut may be regenerated fifty times, edited, and then permanently saved.

A tree generator may produce a family, from which five specific trees are selected.

A chunk may have procedural dressing but a fixed landmark.

An attack may be created from curves but tuned frame by frame through parameters.

The framework is not about pressing a button and accepting whatever appears. It is about making iteration possible in a medium that matches the developer’s strengths.

## Richness Through Interaction Between Simple Systems

A complex-looking scene can emerge from simple systems interacting:

- wind affects snow, fog, trees, ribbons, and sound;
- a shrine alters roots, materials, enemy behavior, and particles;
- a river influences terrain, wetness, fog, stones, bridges, and paths;
- camp state changes light, snow, props, dialogue, and music;
- an attack changes pose, trail, shadow, sound, ground material, and debris.

Each individual system can remain manageable.

The richness comes from their agreement.

---

## World-System Boundaries

The world generator should not own every decision.

- Terrain establishes traversable form and large-scale spatial rhythm.
- The route graph establishes structural purpose.
- Chunk selection realizes graph nodes physically.
- Gameplay placement reserves and validates functional space.
- Semantic dressing responds to meaning and local conditions.
- Rendering systems interpret shared environmental state.
- Narrative systems alter landmarks, populations, or material states without rebuilding the entire world unless necessary.

Keeping these responsibilities separate makes the world easier to direct, debug, regenerate, and preserve.
