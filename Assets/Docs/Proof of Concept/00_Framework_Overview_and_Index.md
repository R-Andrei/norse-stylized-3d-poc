---
document_id: PS3D-00
title: "Framework Overview and Index"
version: 0.1
status: draft
scope: generic-index
authoritative_for: "framework philosophy, terminology, document routing, cross-document boundaries"
related_documents: [PS3D-01, PS3D-02, PS3D-03, PS3D-04, PS3D-05, PS3D-06]
---

# Programmatic Stylized 3D Framework

## Overview and Index

## Purpose

This framework describes a way of developing visually rich 3D games when the developer wants substantial artistic authorship but does not want conventional illustration, sculpting, skinning, rigging, texture painting, or keyframe animation to become the centre of production.

The proposal is not “let the computer make the art.” It is:

> **Build a visual language whose native tools are geometry rules, curves, shaders, transform systems, simulation, composition, and controlled variation.**

The developer remains responsible for taste. The framework supplies repeatable instruments through which that taste can be expressed.

Its recommended base is **stylized modular 3D**, supported by procedural or parametric asset construction and selective voxel techniques. A fully voxel world is optional and usually unnecessary. Voxelization is more valuable when it has a clear visual, narrative, or mechanical role than when it is merely the universal representation of every object.

## Central Division of Responsibility

The framework is built around one distinction:

> **Hand-author meaning and composition. Procedurally author form, variation, motion, and atmosphere.**

Human authorship should normally decide:

- what a place means;
- where the player looks and moves;
- which objects are important;
- the rhythm of danger and rest;
- the silhouette and emotional purpose of a character;
- which generated result is accepted, edited, baked, or rejected;
- how rules combine into a coherent style.

Procedural systems can then assist with:

- families of rocks, vegetation, ruins, buildings, and props;
- surface states such as snow, wear, wetness, corruption, or heat;
- secondary environmental placement;
- rigid-part, spline-based, deformable, or effect-driven actor motion;
- deterministic scene variants;
- trails, particles, wind, fog, and other atmospheric behaviour;
- modular terrain and world assembly.

Procedural does not mean automatic. A generator may be used like a modelling tool: place it, adjust it, regenerate it, choose a result, and bake it. Runtime randomness is only one possible use.

## Framework Layers

The framework can be understood as several interacting grammars:

1. **Shape grammar** — primitives, proportions, profiles, curves, assemblies, and deformation rules.
2. **Material grammar** — palette, lighting response, surface variation, environmental state, and emission.
3. **Motion grammar** — poses, phases, trajectories, springs, squash, rigid-part motion, and procedural deformation.
4. **Effect grammar** — trails, telegraphs, impacts, particles, distortion, fragments, and transient geometry.
5. **Composition grammar** — focal points, sight lines, spacing, contrast, density, landmark hierarchy, and manual layout.
6. **World grammar** — terrain representations, chunks, sockets, graphs, semantic zones, and deterministic dressing.
7. **Narrative grammar** — environmental state, tableaux, dialogue staging, symbolic transformations, and recurring motifs.

A distinctive result rarely comes from one sophisticated system. It emerges when several simple grammars consistently reinforce one another.

## Document Routing Map

Use this section as the default reading guide in later conversations.

```text
Rendering and palette             → Part 01
Generated environment assets      → Part 02
Actors and action presentation    → Part 03
Terrain and world assembly         → Part 04
Limited scene variation            → Part 04
Project-specific interpretation    → Part 05
Current proof of concept           → Part 06
```

More detailed routing:

| Question or task | Read |
|---|---|
| Camera, composition, palette, lighting, shaders, snow, fog, or voxel aesthetics | `01_Visual_Language_and_Rendering.md` |
| Generated-ground visual doctrine, style pillars, family/variant ground interpretation, shared ground style layers, static ground mask contracts, or ground roadmap priority | `../Ground_Visual_Design_and_Architecture.md` |
| Procedural meshes, rocks, vegetation, buildings, ruins, bridges, rivers, paths, carvings, recipes, or baking | `02_Procedural_Geometry_and_Asset_Grammars.md` |
| Nontraditional characters, rigid-part rigs, pose systems, action phases, trails, telegraphs, impacts, or dialogue staging | `03_Procedural_Actors_Motion_and_Combat.md` |
| Terrain representations, chunks, sockets, graphs, semantic placement, deterministic generation, hubs, or changing wilderness | `04_World_Construction_and_Generative_Assembly.md` |
| Facts, working assumptions, decisions, constraints, and open questions belonging specifically to the current Norse mythology project | `05_Project_Application_Norse_Game.md` |
| Scope, construction order, experiments, acceptance criteria, and findings for the clearing prototype | `06_Proof_of_Concept.md` |

### Common Reading Bundles

```text
General framework discussion
    → Part 00

Rendering implementation
    → Parts 00 + 01

A procedural environment object
    → Parts 00 + 02

An unusual actor or combat action
    → Parts 00 + 03

Terrain or map generation
    → Parts 00 + 04

Applying a generic technique to the current game
    → Parts 00 + relevant generic part + 05

Continuing the prototype
    → Parts 00 + 06, then consult 01–04 only as needed

Ground visual design or generated-ground style work
    → Ground_Visual_Design_and_Architecture.md + Ground_Generation_Surface_Upgrade_Plan.md
```

## Core Terms

### Programmatic stylized 3D

A visual-development approach in which code-adjacent systems are primary artistic tools. It may still contain manually placed objects, purchased assets, commissioned illustrations, conventional meshes, or authored animations. The term describes the centre of gravity, not an ideological prohibition.

### Procedural authorship

The deliberate creation of rules, parameters, constraints, and tools that generate or transform content. Procedural authorship includes selection, rejection, manual adjustment, and baking. It is not synonymous with random runtime generation.

### Recipe

A compact, meaningful description of an asset or variation. A useful recipe exposes art-directable ideas such as *roof pitch*, *branch bias*, *damage*, *silhouette width*, or *snow load* rather than dozens of arbitrary noise multipliers.

### Asset grammar

A reusable construction logic capable of producing a family of related objects. A hut grammar may produce shelters, cabins, damaged structures, or ritual buildings while preserving a common visual identity.

### Geometry kernel

A small library of reusable low-level operations—rings, ribbons, tapered prisms, profile extrusion, polygon surfaces, mesh merging, and controlled deformation—from which higher-level generators are built.

### Part rig

A transform hierarchy used as an actor body without requiring a conventionally skinned mesh. Parts may be rigid, disconnected, deformable through shaders, or connected by effects.

### Pose definition

A data representation of target transforms, shape values, material values, or effect states for an actor. Poses can be interpolated through authored curves or selected discretely.

### Action phase

A semantic segment of an action, commonly anticipation, commitment, active motion, impact, and recovery. Gameplay events and presentation events derive from the same phase timeline.

### Semantic placement zone

An authored region carrying meaning such as *path*, *shelter*, *riverbank*, *sacred ground*, *combat clearance*, or *exposed ridge*. Placement rules respond to meaning rather than sampling the world uniformly.

### Chunk socket

A typed connection point through which modular world pieces can be joined. A socket may represent a path, river, doorway, pass, cave, elevation transition, or other continuity requirement.

### Selective voxel use

Voxel or grid-like techniques applied where they add specific value: destruction, formation, supernatural transformation, snow volume, corruption, disintegration, or a contrasting material state. The rest of the world may remain ordinary meshes.

## Principles

### Coherence is more important than geometric complexity

A simple object that obeys the same palette, lighting, snow, edge, motion, and proportion rules as the rest of the world will usually contribute more than a highly detailed object built in an unrelated style.

### Meaningful parameters are part of the art interface

The quality of a generator depends on whether a person can direct it. Expose concepts a designer can reason about. Hide incidental implementation variables behind higher-level controls.

### Generated results should be curated

A generator is allowed to produce failures during development. The workflow must make those failures easy to identify, reject, adjust, or prevent. A “randomize” button is not a substitute for art direction.

### Motion can be graphic rather than anatomical

Readability, timing, and silhouette are often more important than biomechanical fidelity. Actors can be assembled from rigid parts, trails, masks, fragments, ribbons, shadows, or deformable primitives if their motion clearly communicates intention.

### Effects may be structural

A trail can function as a weapon. Smoke can function as a body. Dust can connect disconnected stones into a creature. A moving shadow can communicate an airborne attack. Effects are not necessarily late decoration.

### World generation should begin with structure

Generate topology, routes, landmarks, and encounter roles before choosing local geometry and decoration. Randomly attaching attractive chunks rarely creates meaningful geography by itself.

### Build one convincing scene before building a content empire

The framework should first prove that a small composed scene can look intentional, move clearly, and be pleasant to author. Large-scale procedural systems magnify both strengths and unresolved weaknesses.

## Boundaries Between Documents

Each topic has one authoritative home:

- Part 01 owns the visual and rendering behaviour of snow; Part 02 describes what geometry supplies to it; Part 04 describes how world state distributes it; Part 06 defines the minimum snow test for the prototype.
- Part 02 owns construction of generated objects; Part 04 owns their placement within generated regions.
- Part 03 owns expressive actor presentation; Part 05 records which actor approaches the current project intends to use.
- Part 04 owns generic hub-and-expedition patterns; Part 05 records whether the current game uses one.
- Part 06 records experimental implementation and findings; it should not silently redefine the durable framework or project canon.
- `Ground_Visual_Design_and_Architecture.md` owns generated-ground visual doctrine; `Ground_Generation_Surface_Upgrade_Plan.md` owns generated-ground implementation history and patch sequencing.

## Status Vocabulary

Project and prototype documents should distinguish:

- **Confirmed** — an active decision unless deliberately changed.
- **Working direction** — currently preferred, but still open to revision.
- **Experiment** — being tested without a commitment to ship it.
- **Illustrative example** — explains a technique and is not a planned feature.
- **Rejected** — considered and intentionally excluded for now.
- **Unresolved** — requires design or technical investigation.

## Maintenance Rules

1. Keep Parts 00–04 generic enough to describe a reusable development language.
2. Put concrete game facts and decisions in Part 05.
3. Put temporary prototype scope, build order, and findings in Part 06.
4. Add cross-references instead of copying complete explanations between documents.
5. Update the overview routing map when a topic moves.
6. Record uncertain information as uncertain rather than allowing examples to become accidental requirements.
7. Prefer stable concept names across every document.

## Current Document Set

```text
00_Framework_Overview_and_Index.md
01_Visual_Language_and_Rendering.md
02_Procedural_Geometry_and_Asset_Grammars.md
03_Procedural_Actors_Motion_and_Combat.md
04_World_Construction_and_Generative_Assembly.md
05_Project_Application_Norse_Game.md
06_Proof_of_Concept.md
../Ground_Visual_Design_and_Architecture.md
../Ground_Generation_Surface_Upgrade_Plan.md
```

## Closing Position

The framework is not a shortcut around creativity. It is an attempt to make a different kind of creativity operational.

Its premise is that a programmer can develop a serious visual language by authoring relationships: between shape and motion, weather and surface, action and trail, landmark and route, fixed composition and generated variation. The resulting art remains human-directed because the rules, priorities, exceptions, and final selections remain authored.
