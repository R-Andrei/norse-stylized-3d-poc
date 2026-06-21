---
document_id: PS3D-06
title: "Visual Framework Proof of Concept"
version: 0.1
status: draft
scope: project-prototype
authoritative_for: "clearing prototype scope, experiments, construction sequence, acceptance criteria, findings and next implementation target"
related_documents: [PS3D-00, PS3D-01, PS3D-02, PS3D-03, PS3D-04, PS3D-05]
---

# Visual Framework Proof of Concept

## Purpose

This document defines a small playable scene intended to validate the Programmatic Stylized 3D Framework in the context of the current Norse mythology project.

It is best described as a **visual framework proof of concept**, **technical-art prototype**, or **playable visual prototype**. It is not yet a conventional vertical slice. It does not need to demonstrate every production discipline or represent final game quality across progression, user interface, narrative structure, content scale, and performance.

Its central question is:

> Can this development language produce a coherent, expressive, enjoyable-to-author game scene using programming-adjacent artistic tools?

## Framework Coverage

```text
Rendering and palette             → Part 01
Generated environment assets      → Part 02
Actors and action presentation    → Part 03
Limited scene variation           → Part 04
Project-specific interpretation   → Part 05
```

Part 06 applies those ideas. It should cross-reference the generic theory rather than duplicate all of it.

## Questions the Prototype Must Answer

### Artistic

- Can simple generated or assembled geometry feel intentional?
- Can one rendering language unify different geometry sources?
- Can the scene support quiet atmosphere, dialogue, and combat?
- Can actors built without conventional character production communicate identity and intention?
- Can selective voxel behaviour complement the style without defining the entire world?

### Technical

- Is the geometry kernel sufficient for several different asset families?
- Are seeded recipes deterministic and easy to direct?
- Can generated output be edited, saved, duplicated, or baked?
- Can presentation and gameplay timing derive from the same action phases?
- Can scene variation remain constrained by composition and gameplay rules?

### Workflow

- Is adjusting parameters, poses, curves, materials, and placement satisfying?
- Does a new variation take minutes rather than a conventional asset-production cycle?
- Do the tools expose artistic concepts rather than low-level numerical clutter?
- Is it easy to reject a bad result without destabilizing the scene?
- Does this feel like a viable long-term production method for the developer?

## Explicit Non-Goals

The prototype should not attempt to validate:

- the complete procedural overworld;
- production-scale chunk streaming;
- the full combat architecture;
- final progression or roguelike systems;
- complete narrative delivery;
- final UI and menus;
- a complete enemy roster;
- realistic humanoid animation;
- comprehensive voxel terrain;
- final optimization for the entire game;
- a universal generator for every possible asset.

These systems may be represented by minimal placeholders when needed to exercise the visual framework.

## Scene Concept

Build one small, manually composed clearing, approximately **40 × 40 metres** as a starting scale. The exact dimensions may change after camera testing.

The clearing should contain several kinds of visual problem in a compact space:

- uneven natural ground;
- a small water or traversal feature;
- a simple constructed crossing;
- one modest structure or ruin;
- one generated organic landmark;
- one carved, ritual, or civilizational sign;
- a family of generated rocks;
- a rough route through the scene;
- a background layer such as cliffs, slopes, trees, fog, or mountains;
- one warm or otherwise contrasting local light source;
- at least one conversational or passive actor;
- a small number of actor experiments with distinct motion languages.

The scene should be large enough to show depth and atmosphere, but small enough that every object can be inspected and revised.

## Roles Rather Than Mandatory Creatures

The prototype should test actor techniques, not lock the final roster.

### Rigid-part or incomplete actor

Tests:

- transform-based posing;
- separated or simplified geometry;
- strong silhouette;
- effects functioning as anatomy;
- an abstract or supernatural action.

A masked figure, armour fragments, animated relic, skeletal assembly, or other constructed body could fill this role.

### Deformable actor

Tests:

- squash and stretch;
- transform or shader deformation;
- anticipation through compression;
- trajectory;
- landing impact and recovery.

A blob-like body is an easy candidate but not a required final creature.

### Trajectory-driven actor

Tests:

- curved movement;
- discrete pose states;
- shadow communication;
- speed trails;
- readable fast attacks.

A small bird is a convenient candidate but can be replaced by another flying, leaping, or darting form.

### Passive or conversational actor

Tests:

- dialogue interaction;
- identity through posture, small gestures, material state, or environment;
- static portrait and text presentation;
- presence without full facial animation.

## Current Candidate Contents

These candidates are concrete enough to build but remain replaceable:

- generated rock family;
- generated dead or stylized tree;
- simple hut, shelter, or ruin assembled from a construction grammar;
- spline-defined river or stream;
- spline- or sample-assembled wooden bridge;
- carved standing stone or sign;
- masked or fragmented NPC/combatant;
- deformable primitive enemy;
- small flying attacker;
- snow, wind, fog, local warmth, trails, telegraphs, and impacts;
- one limited voxel-fragmentation or formation experiment.

## Manual Composition

The following should be authored directly in the scene:

- camera angle and framing;
- important foreground, middle-ground, and background relationships;
- the main traversal route;
- location of the structure;
- location and orientation of the crossing;
- location of the major organic landmark;
- conversation position;
- combat clearance;
- important sight lines;
- warm/cool or safe/danger contrast;
- the scene’s focal hierarchy.

The prototype is not testing whether random placement can accidentally produce good composition.

## Procedural Variation Boundaries

### Fixed

- camera and its permitted movement;
- major object anchors;
- route and crossing locations;
- encounter space;
- dialogue space;
- landmark hierarchy;
- critical navigation;
- gameplay telegraph visibility.

### Allowed to vary

- rock shapes and secondary rock placement;
- branch proportions and minor tree structure;
- structural damage, lean, and detail of the hut or ruin;
- bridge plank irregularity;
- subtle river curvature within authored bounds;
- snow amount and local breakup;
- minor debris and dressing;
- ambient particles and wind intensity;
- small material variations;
- optional voxel fragment arrangement.

### Forbidden outcomes

- generated objects block required navigation;
- decoration damages important sight lines;
- a structure becomes unreadable or impossible;
- the bridge disconnects from its banks;
- tree or cliff silhouettes hide attack telegraphs;
- randomness changes encounter purpose;
- secondary detail competes with the focal point;
- a seed changes unrelated systems unexpectedly.

## Prototype Asset Vocabulary

Begin with a deliberately small vocabulary.

### Primitive meshes

- cube and bevelled cube;
- plane and quad;
- cylinder or low-sided prism;
- cone and wedge;
- sphere or icosphere;
- capsule;
- simple ribbon;
- extruded 2D profile.

### Geometry operations

- add triangle and quad;
- add and bridge rings;
- tapered prism between points;
- ribbon along sampled positions;
- polygon/profile extrusion;
- seeded radial deformation;
- bottom flattening;
- mesh merge and bake;
- vertex colour assignment;
- collider generation or approximation.

### Shared materials

- stylized opaque material;
- spectral/effect material;
- water material;
- optional trail/telegraph material.

### Shared effects

- ribbon trail;
- radial ground telegraph;
- impact ring;
- snow or dust burst;
- wind streaks;
- simple emission pulse;
- optional voxel fragments.

## Project Setup

Use a small isolated Unity scene and avoid coupling the experiment to unfinished production systems.

Recommended early project features:

- a render pipeline that supports the intended shader workflow;
- Shader Graph or equivalent shader authoring;
- a spline solution;
- the Unity new Input System exclusively;
- editor scripts for regeneration and baking;
- a minimal test controller and camera;
- a global palette or style asset.

Do not install or architect large systems solely because they may become useful later.

## Suggested Folder Shape

```text
Assets/Game/
├── VisualFramework/
│   ├── Core/
│   ├── Geometry/
│   ├── Environment/
│   ├── Actors/
│   ├── Motion/
│   ├── Effects/
│   ├── Materials/
│   └── Editor/
└── Prototype/
    ├── Scenes/
    ├── Prefabs/
    ├── Recipes/
    ├── Poses/
    ├── Actions/
    └── Findings/
```

Create folders only when they contain real work. The structure is guidance, not a demand for empty architecture.

## Stage 1 — Primitive Blockout

Construct the entire clearing from primitive geometry before writing generators.

Use a plane or rough mesh for the ground, boxes for the structure, rectangular planks for the bridge, cylinders or prisms for the tree, spheres for rocks, and extremely simple actor proxies.

Lock or heavily constrain the camera. Establish:

- playable scale;
- object scale;
- route width;
- foreground and background depth;
- combat readability;
- silhouette separation;
- landmark placement;
- approximate lighting direction.

### Exit condition

The composition should already be intelligible and somewhat attractive as a greybox. If it is confusing, generators will multiply the confusion.

## Stage 2 — Palette, Lighting, and Atmospheric Baseline

Create one project palette asset and use it consistently.

Establish:

- shadow colour or value range;
- material-family colours;
- ground and snow relationship;
- warm or supernatural accents;
- danger and telegraph colour;
- distant atmospheric colour;
- fog density and falloff;
- one directional light;
- one local contrasting light;
- restrained post-processing.

Avoid arbitrary per-object colour selection.

### Exit condition

Primitive geometry should appear to belong to one world, and focal elements should remain readable without detailed textures.

## Stage 3 — First Opaque Material

Build the minimum shared material language:

- palette-driven dark and light values;
- world-space surface breakup;
- upward-facing environmental accumulation such as snow;
- per-object seed variation;
- optional emission;
- controlled vertex colour influence.

Do not begin with a universal shader containing every future feature.

### Exit condition

The ground, primitive rocks, structure, bridge, and tree can share related material behaviour without appearing identical.

## Stage 4 — Geometry Kernel and Rock Generator

The first true generator should be the rock family.

Reasons:

- topology is forgiving;
- failures often look natural;
- rocks exercise deformation, normals, vertex colours, snow, colliders, seeds, and baking;
- the same family can later produce debris, cliffs, cairns, or actor parts.

Start from a low-resolution convex mesh. Apply seeded radial variation, non-uniform scale, broad fracture or shear, and bottom flattening.

Expose meaningful controls:

- proportions;
- angularity;
- broad irregularity;
- fracture direction;
- base flattening;
- tilt;
- surface family;
- environmental accumulation.

### Exit condition

At least twelve variants look related, stable on the ground, readable from the game camera, and intentional under the common material.

Do not begin the tree generator before this works.

## Stage 5 — Ground Patch

Replace the flat plane with a generated or parametrically deformable ground patch.

Its height can combine:

- broad authored shape;
- low-frequency variation;
- path flattening;
- structure and combat-area flattening;
- river depression;
- local landmark adjustments.

Do not solve erosion or production terrain streaming.

### Exit condition

The clearing has readable large-scale form, stable navigation, and enough surface variation to support lighting without becoming noisy.

## Stage 6 — Spline River and Crossing

Build one reusable ribbon-along-curve tool. Use it first for water.

At sampled points, calculate tangent and side direction, create left/right vertices, connect samples, and store flow information for the shader.

Build the crossing as an assembler:

- sample a crossing curve;
- place simple planks;
- orient them to the tangent;
- apply bounded positional and rotational irregularity;
- add supports or ropes as optional modules;
- validate both banks.

### Exit condition

The river and crossing are visually coherent, clearly traversable, and remain connected across allowed seeds.

## Stage 7 — Structure Grammar

Construct one hut, shelter, or ruin from panels, posts, beams, roof forms, and damage rules.

Expose parameters such as:

- footprint;
- height;
- roof pitch;
- structural lean;
- opening location;
- damage pattern;
- missing modules;
- snow load;
- decoration slots.

Damage should be controlled incompleteness, not a destruction simulation.

### Exit condition

Several variants share an architectural language while differing visibly in silhouette and condition.

## Stage 8 — Organic Generator

Build one stylized tree or comparable organic landmark from a small authored branch graph and tapered segments.

Allow variation in:

- segment length;
- taper;
- twist;
- directional bias;
- missing terminal branches;
- broken ends;
- root spread;
- environmental response.

Do not begin with a universal botanical simulation.

### Exit condition

The generator creates a useful family of compositionally controllable forms rather than biologically plausible noise.

## Stage 9 — Part Rig and Pose Capture

Create a generic transform-based actor hierarchy and a pose representation.

A part rig may contain:

```text
ActorRoot
├── Body
├── HeadOrIdentityPart
├── LeftPart
├── RightPart
├── EffectOrigin
├── GroundOrigin
└── Shadow
```

Build an editor action that captures current local transforms into a pose asset.

### Exit condition

An actor can be manually posed in the scene, saved, restored, and interpolated without a skinned mesh.

## Stage 10 — Phase-Based Action Player

Represent actions through semantic phases:

- anticipation;
- commitment;
- active movement;
- impact;
- recovery.

Each phase may drive:

- target pose;
- root movement;
- interpolation curve;
- damage window;
- telegraph state;
- effect events;
- sound events;
- camera events;
- material state.

### Exit condition

One simple action remains readable with particles, sound, and camera shake disabled.

## Stage 11 — Three Contrasting Motion Tests

Implement three small actions that exercise different techniques.

### Rigid or incomplete actor action

A strongly posed supernatural strike, pulse, projection, or area action. It may use a trail as the visible weapon.

### Deformable actor action

Compression, leap or lunge, impact squash, radial effect, and damped recovery.

### Trajectory actor action

Pose compression, curved fast movement, target shadow, speed trail, and exit or recovery.

### Exit condition

The player can identify anticipation, active danger, impact, and recovery for all three, and each actor has a distinct motion language.

## Stage 12 — Reusable Trail and Telegraph Language

Build one custom ribbon trail and one family of ground or spatial telegraphs.

Trail samples should carry at least:

- position;
- age;
- width;
- optional orientation or colour value.

Telegraphs should be evaluated for:

- visibility against every tested surface;
- relationship to actual damage timing;
- readability under fog and effects;
- ability to express direction, area, or sequence.

### Exit condition

Effects amplify motion rather than hiding weak motion.

## Stage 13 — Scene Seed and Limited Variation

Add a scene director that distributes independent seed channels.

Suggested channels:

```text
rocks
organic-landmark
structure
crossing
surface-state
dressing
ambient-effects
```

Do not derive identity from array order. Use stable names or identifiers.

The scene seed may change secondary form and dressing, but not major composition.

### Exit condition

Several seeds look like intentional variants of the same authored scene, and changing one channel does not unexpectedly alter another.

## Stage 14 — Selective Voxel Experiment

Test one small semantic use of voxel-like behaviour without implementing voxel terrain.

Possible experiments:

- a struck surface temporarily breaks into grid-aligned fragments;
- an actor forms from cubic pieces;
- a supernatural effect quantizes smooth matter;
- a rune emits stepped fragments;
- an object dissolves and reassembles through a voxel mask.

This can initially be faked with spawned cubes, a grid sampler, a dissolve mask, and authored curves.

### Exit condition

Voxelization contributes a distinctive meaning or material contrast. If it merely looks fashionable or unrelated, remove it.

## Stage 15 — Dialogue and Quiet Presentation

Add one short interaction using static portrait and text.

Test whether the scene can hold attention without combat through:

- actor posture;
- small procedural gestures;
- environmental motion;
- sound and pause;
- camera framing;
- local lighting;
- symbolic props.

### Exit condition

The visual language supports narrative stillness as well as impact-heavy action.

## Editor Tool Requirements

Each generator should eventually support:

```text
Regenerate
Randomize Seed
Save or Copy Recipe
Duplicate Variant
Bake Mesh or Assembly
Clear Generated Output
Validate
```

The prototype does not need a polished universal editor window. Small custom inspectors are sufficient if they make iteration fast and safe.

## Data Separation

Keep these data types conceptually distinct:

- style palette;
- material family configuration;
- asset recipe;
- world or scene seed;
- part-rig definition;
- pose definition;
- action definition;
- semantic zone;
- prototype findings.

Do not store all prototype state in one scene director or monolithic ScriptableObject.

## Technical Acceptance Criteria

- Seeded generation is deterministic.
- Major generators can regenerate without leaking duplicate objects.
- Generated meshes have valid bounds and normals.
- Colliders are adequate for the prototype.
- Generated output can be baked or preserved.
- Shared materials work on all tested geometry families.
- Action timing and damage timing can remain synchronized.
- Regeneration does not invalidate critical navigation or composition.
- The scene performs adequately at prototype scale.

## Artistic Acceptance Criteria

- The clearing has a recognizable identity in a still image.
- It remains coherent in motion.
- Generated objects appear related rather than randomly low-poly.
- Important silhouettes remain readable.
- Quiet and active moments both look intentional.
- Actor simplicity feels designed rather than unfinished.
- VFX reinforce form, timing, and meaning.
- Several seeds look like variations of one art direction.
- A silent short recording looks like a game scene, not merely a procedural-generation demonstration.

## Workflow Acceptance Criteria

- A useful new rock variant requires no conventional modelling session.
- A structural variation can be authored through meaningful parameters.
- A new action can be built from poses, curves, and events.
- Generated assets can be manually placed and composed.
- Bad variants can be rejected or corrected easily.
- The developer enjoys the authoring loop enough to imagine repeating it across production.

## Failure Signals

Pause and revise if:

- the scene requires effects to hide unreadable geometry;
- every generator exposes dozens of arbitrary values;
- seeds alter important layout unpredictably;
- the camera must constantly change to make assets look acceptable;
- all objects share one generic low-poly appearance without identity;
- actor actions only read after particles and camera shake are added;
- procedural detail overwhelms focal points;
- runtime architecture is being built before editor authoring is pleasant;
- voxel work expands without a clear semantic purpose;
- the prototype becomes a disguised attempt to build the whole game.

## Recommended Immediate Start

The first concrete implementation target is deliberately narrow:

1. Create an isolated prototype scene.
2. Block out the clearing entirely with primitives.
3. Lock the camera and establish scale.
4. Define one palette and basic lighting setup.
5. Build the first shared opaque material.
6. Replace only the placeholder rocks with generated geometry.
7. Produce and compare at least twelve variants from the game camera.

Everything after that should be earned by the success of those rocks and the scene around them.

## Findings Log

Use this section while implementing.

### Confirmed successful techniques

- _Not evaluated yet._

### Techniques requiring revision

- _Not evaluated yet._

### Rejected experiments

- _None yet._

### New questions

- _Add questions as they emerge._

### Current implementation state

- Documentation and scope definition in progress.
- Primitive scene not yet assumed complete.

### Next experiment

- Primitive clearing, camera, palette, and generated rock family.

## Prototype Decision Entry Template

```md
### YYYY-MM-DD — Experiment or decision

**Question:**
What was being tested?

**Implementation:**
What was built?

**Observation:**
What happened in practice?

**Decision:**
Keep | Revise | Reject | Defer

**Affected documents or systems:**
List any framework or project sections that should change.
```

## Graduation Criteria

The proof of concept may grow toward a true vertical slice only when:

- the scene has a stable visual language;
- the asset and actor workflows are demonstrably repeatable;
- the developer wants to continue using them;
- at least one small encounter and one quiet interaction feel representative;
- controlled variation does not damage composition;
- the next risks concern game production rather than whether the artistic method works at all.

Until then, its job is to remain a focused laboratory.
