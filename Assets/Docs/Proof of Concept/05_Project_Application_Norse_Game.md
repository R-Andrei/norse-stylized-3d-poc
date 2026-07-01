---
document_id: PS3D-05
title: "Project Application: Norse Mythology Game"
version: 0.1
status: draft
scope: project-specific
authoritative_for: "confirmed project facts, working directions, project constraints, application of the generic framework, open questions and decision log"
related_documents: [PS3D-00, PS3D-01, PS3D-02, PS3D-03, PS3D-04, PS3D-06]
---

# Project Application: Norse Mythology Game

## Purpose

This document records how the generic Programmatic Stylized 3D Framework currently applies to the specific game project.

Unlike Parts 00–04, this document is allowed to contain concrete project facts, current preferences, rejected directions, and temporary design assumptions. It should remain careful about certainty: examples discussed during exploration must not silently become canon or production commitments.

The project profile should be reread whenever a generic framework discussion is being converted into a decision for the actual game.

## Certainty Legend

- **Confirmed** — currently treated as an active project decision.
- **Working direction** — preferred direction, but still open to revision.
- **Experiment** — worth testing without a commitment to ship.
- **Illustrative only** — an example used to explain the framework.
- **Rejected for now** — intentionally excluded from current planning.
- **Unresolved** — requires further design or technical investigation.

## Confirmed Project Facts

### Identity

- The game is **based on Norse mythology**.
- It has **roguelike elements**, but it should not be reduced to or described simply as “a roguelike.”
- The intended experience has action, role-playing, narrative, atmosphere, and systemic variation in proportions that remain open to refinement.
- The game is intended to be artistically expressive and immersive rather than treating visuals as interchangeable packaging around mechanics.

### Developer and production constraints

- The developer is primarily a programmer.
- Manual authorship, scene composition, parameter tuning, and visual experimentation are welcome.
- The central production constraint is not an aversion to labour; it is a decision not to spend years specializing in conventional drawing, character modelling, skinning, rigging, and animation disciplines that are not the desired centre of practice.
- The visual pipeline should therefore create artistic leverage through programming-adjacent tools and systems.
- Unity’s **new Input System** is used exclusively. Project code and documentation should not propose the legacy `UnityEngine.Input` API.

### Chosen visual direction

- **Stylized 3D** is the current foundation.
- **Selective voxel usage** remains available as a visual or mechanical technique.
- A **fully voxel world** is rejected for now.
- Procedural geometry, modular construction, shaders, effects, transforms, curves, and controlled randomness are intended to function as artistic instruments.
- Manual scene composition remains desirable. The framework is not expected to compose every important view automatically.

## Working Experience Direction

The following points are active working directions rather than immutable commitments:

- A controlled top-down, high-angle, or isometric-adjacent camera is likely to reduce asset burden and strengthen composition.
- Combat should be telegraphed, intentional, and visually stylized rather than dependent on physically perfect weapon animation.
- Actors may be humanoid, non-humanoid, abstract, fragmented, deformable, effect-defined, or environmental.
- Dialogue may use static character portraits and text rather than expensive facial performance.
- Story-rich and quiet scenes must be supported alongside combat.
- Menus, user interface, and most gameplay systems are not the immediate visual-framework risk.
- The principal open risk is whether the proposed art language can create enough beauty, specificity, consistency, and emotional range.

## World-Structure Working Direction

A currently attractive structure is:

- one stable, authored home or camp area;
- variable regions outside it;
- hand-designed or parametrically authored map pieces;
- a graph or chunk system that combines those pieces into different expeditions;
- important compositions and landmarks protected from uncontrolled random placement;
- deterministic seeds and semantic placement rules for controlled variety.

This direction resembles a **stable anchor plus variable exterior** pattern. It should not yet be treated as a complete world-design specification.

## Actor and Motion Direction

The project is open to actors that avoid conventional production burdens while remaining expressive.

Possible techniques include:

- rigid-part puppets;
- masks or strong facial simplification;
- silhouettes and abstract bodies;
- deformable primitive bodies;
- separated fragments connected by smoke, dust, light, or force;
- birds or flying forms driven by pose states and trajectories;
- animated objects, relics, trees, signs, armour, or environmental structures;
- attacks represented through trails, wind, runes, shadows, or materialization rather than a permanently modelled weapon.

Specific examples previously discussed—such as a raven, floating mask, ghost, blob, rock construct, talking tree, or animated armour—are **illustrative only unless promoted to a confirmed project decision later**.

## Environment Direction

The visual framework should support:

- natural spaces containing trees, rocks, rivers, cliffs, snow, paths, or comparable environmental systems;
- sparse signs of civilization such as bridges, huts, ruins, carvings, shrines, posts, and damaged structures;
- procedural families of objects rather than a requirement for unique handcrafted models at every location;
- strong environmental storytelling through placement, state, weather, and material response;
- authored focal points surrounded by controlled generated variation.

The exact climate, biome distribution, architectural vocabulary, and symbolic motifs should be recorded here as they become confirmed.

## Rendering Direction

Likely project-specific uses of the generic rendering framework include:

- a restricted palette with region or world-state variants;
- strong silhouettes and readable value separation from the game camera;
- shared environmental states such as snow, frost, wind, age, damage, warmth, or supernatural influence;
- selective emission for symbols, eyes, effects, ritual elements, and important interactables;
- atmospheric depth through fog, reduced distant contrast, and controlled background silhouettes;
- effects treated as part of actor and world anatomy rather than late polish;
- voxel-like fragmentation or formation used only where it has semantic value.

No final palette or material style is confirmed yet.

## Current Framework Application Map

```text
Project visual identity and rendering choices
    → Generic rules in Part 01

Project environment generators and asset families
    → Generic rules in Part 02

Project actor construction and combat presentation
    → Generic rules in Part 03

Project hub, terrain, chunks, and variable expeditions
    → Generic rules in Part 04

Immediate validation scene and build order
    → Part 06
```

## Rejected or Deferred Directions

### Rejected for now

- Treating the game as a fully voxel world merely to avoid conventional asset production.
- Describing the project as simply a roguelike.
- Requiring realistic humanoid animation as the baseline for every character.
- Allowing uncontrolled procedural generation to determine important composition or narrative geography.

### Deferred

- Final production-scale chunk generation.
- A complete terrain streaming architecture.
- Full character roster and animation requirements.
- Final dialogue portrait pipeline.
- Large-scale AI-assisted asset production.
- Final decisions about commissioned or purchased signature assets.

## Information Deliberately Not Asserted Here

Earlier discussions contained story summaries that were later described as factually inaccurate. This document therefore does not restate detailed lore, divine relationships, plot events, world-state causes, or protagonist history without reconfirmation.

When story canon is supplied or corrected, it should be added under a dedicated **Confirmed Narrative Canon** section rather than inferred from exploratory examples or old summaries.

## Confirmed Narrative Canon

At present, only the following high-level statement is safe to treat as confirmed:

> The game is based on Norse mythology and includes roguelike elements without being defined solely as a roguelike.

Further canonical information should be added here after explicit confirmation.

## Open Questions

- What exact camera behaviour best supports exploration, combat, and composition?
- How abstract may the player character be while preserving attachment and readability?
- Which environmental conditions are universal world systems?
- How much of the exterior world is assembled from authored chunks versus generated terrain recipes?
- Which visual motifs are specific to the game rather than generic framework examples?
- How does narrative persistence interact with changing expeditions?
- Which elements deserve bespoke commissioned assets after the procedural language is proven?
- What should selective voxelization mean within the fiction?

## Decision Log

### Current decisions

- Stylized modular 3D selected as the active foundation.
- Selective voxel usage retained as an experiment or accent.
- Fully voxel world rejected for now.
- Generic framework documentation separated from project-specific documentation.
- The clearing prototype receives its own document in Part 06.

### Future entry format

```md
### YYYY-MM-DD — Decision title

**Status:** Confirmed | Working direction | Experiment | Rejected

**Decision:**
Describe the decision.

**Reasoning:**
Explain why it was made.

**Consequences:**
List systems or documents affected.
```

## Current Project State

- The visual-development framework has been selected conceptually.
- The framework documentation is being modularized.
- No production-scale implementation is assumed complete.
- The next concrete activity is the visual framework proof of concept described in Part 06.
