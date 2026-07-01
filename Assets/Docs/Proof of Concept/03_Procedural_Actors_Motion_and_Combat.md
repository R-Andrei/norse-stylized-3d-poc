---
document_id: PS3D-03
title: "Procedural Actors, Motion, and Combat"
version: 0.1
status: draft
scope: generic-framework
authoritative_for: "nontraditional actor construction, pose systems, action phases, expressive motion, VFX anatomy, combat readability, dialogue staging"
related_documents: [PS3D-00, PS3D-01, PS3D-02, PS3D-04, PS3D-05, PS3D-06]
---

# Procedural Actors, Motion, and Combat

## Context Capsule

This document belongs to the **Programmatic Stylized 3D Framework**: a reusable game-development language for creating artistically deliberate 3D games through procedural geometry, modular construction, shaders, simulation, constrained randomness, authored composition, and tool-assisted iteration.

The framework does **not** propose removing art or manual authorship. It changes the primary artistic instruments. Important composition, meaning, pacing, symbolism, and selection remain human decisions; code and procedural tools provide reusable ways to shape form, motion, atmosphere, and variation.

Concrete creatures, environments, and effects in this document are **illustrative patterns**, not mandatory content for any specific project. Project commitments belong in `05_Project_Application_Norse_Game.md`; the current test scene belongs in `06_Proof_of_Concept.md`.

## Purpose of This Part

This part explores how characters and combat can be expressive without requiring every actor to be a conventionally sculpted, skinned, and keyframe-animated humanoid.

It does not prohibit skeletal animation. It expands the available vocabulary to include rigid-part puppets, disconnected fragments, deformable primitives, spline bodies, animated shadows, procedural trajectories, material-state animation, and effects that function as anatomy.

The named creatures below form an **example library of motion problems**. They are not a proposed roster. A bird demonstrates trajectory and pose compression; a deformable primitive demonstrates squash and stretch; a fragmented construct demonstrates transform-based motion; an incomplete spirit demonstrates how effects can imply missing volume.

## Rejecting Conventional Character Production Without Rejecting Character

A project does not need to eliminate characters. It can redefine what a character body is.

A character can be:

- a hierarchy of rigid parts;
- a mask and a shadow;
- a collection of floating armor;
- a blob;
- a ribbon;
- a set of rocks;
- a bird made from wedges;
- a tree with moving branches;
- a signpost with expressive materials;
- a silhouette with effects;
- a spirit whose body is mostly particles.

This avoids:

- skin weights;
- complex deformation;
- facial rigs;
- anatomy;
- cloth simulation;
- large animation libraries.

## Rigid-Part Hierarchies

A procedural puppet may use a transform hierarchy:

```text
ActorRoot
├── Body
├── HeadOrMask
├── LeftPart
├── RightPart
├── WeaponOrTrailOrigin
├── Shadow
└── VFXAnchors
```

This is technically a rig, but it is not a conventional skinned-character pipeline.

Parts can be animated by:

- interpolation;
- springs;
- curves;
- constraints;
- inverse kinematics;
- noise;
- gameplay state;
- physics.

## Pose-Based Animation

Instead of hundreds of keyframes, define a small set of meaningful poses.

A pose stores local transforms:

```csharp
[Serializable]
public struct PartPose
{
    public string PartId;
    public Vector3 LocalPosition;
    public Vector3 LocalEulerAngles;
    public Vector3 LocalScale;
}
```

Possible poses:

- idle;
- alert;
- anticipation;
- attack;
- impact;
- recoil;
- recovery;
- damaged;
- death;
- dialogue.

Movement between poses can use:

- animation curves;
- spring interpolation;
- snap timing;
- overshoot;
- deliberate holds.

This supports highly stylized motion.

## Action Phases

A combat action can be represented as phases:

```text
Anticipation
Commitment
Active
Impact
Recovery
```

Each phase can contain:

- duration;
- target pose;
- movement curve;
- VFX events;
- SFX events;
- camera events;
- gameplay events;
- trail shape;
- telegraph state;
- hitbox state.

Example structure:

```csharp
[Serializable]
public class ActionPhase
{
    public float Duration;
    public AnimationCurve MotionCurve;
    public PoseDefinition TargetPose;
    public VfxEvent[] VfxEvents;
    public GameplayEvent[] GameplayEvents;
}
```

This unifies visual timing and game logic.

The attack does not have an animation clip that gameplay must chase. The attack definition produces both.

---


## Illustrative Actor Patterns

### Pattern: Masked or Faceless Silhouette

### Construction

- tapered body mesh;
- floating mask;
- optional arm wedges;
- dark material;
- trailing ribbon or cloak pieces;
- projected shadow;
- trail origin;
- small emissive accents.

The body may be deliberately indistinct. The mask provides identity.

### Attack

1. the body leans away from the target;
2. the mask rotates toward the target;
3. wind and particles pull inward;
4. the silhouette snaps through an arc;
5. the weapon may remain invisible;
6. a trail describes the attack;
7. snow or dust bursts on impact;
8. the pose holds briefly;
9. secondary pieces overshoot during recovery.

The attack is read through timing, shape, and effect rather than anatomical realism.

### Pattern: Deformable Primitive Creature

### Construction

- low-resolution sphere or generated soft shape;
- internal glow;
- simple shadow;
- optional surface features;
- vertex-displacement shader.

### Motion

Idle:

- subtle squash and stretch;
- slow vertex wobble;
- core movement.

Leap anticipation:

- flatten;
- widen;
- tilt.

Leap:

- stretch vertically;
- compress horizontally;
- follow a parabola;
- trail droplets.

Landing:

- flatten heavily;
- emit radial wave;
- spawn particles;
- recover through a damped spring.

The entire enemy may require no skeleton.

### Pattern: Small Flying Creature

### Construction

- ellipsoid body;
- head;
- wedge beak;
- two wing meshes;
- tail fan;
- wing pivots;
- shadow;
- wind-trail anchor.

### Pose vocabulary

- perched;
- glide;
- flap up;
- flap down;
- bank;
- dive;
- impact.

### Dive attack

1. the raven circles or pauses;
2. its shadow becomes the primary telegraph;
3. wings fold;
4. the body aligns with a curve;
5. wind trails lengthen;
6. feathers separate from the silhouette;
7. impact produces a narrow directional burst;
8. the raven exits or reconstructs.

The effect work is part of the character.

### Pattern: Fragmented Rigid-Part Body

### Construction

- helmet;
- breastplate fragment;
- pauldrons;
- gauntlets;
- shield;
- weapon;
- internal smoke or light.

The pieces do not need to touch.

Each follows a target transform with spring behavior.

Conceptually:

```csharp
velocity += (targetPosition - position) * stiffness * deltaTime;
velocity *= damping;
position += velocity * deltaTime;
```

### Expressive possibilities

- parts lag behind turns;
- armor expands during threat;
- pieces rotate into attack alignment;
- the body disassembles to avoid damage;
- gauntlets attack independently;
- the helmet watches separately;
- damage causes one piece to lose synchronization;
- death releases the internal spirit.

This character type is exceptionally compatible with procedural animation.

### Pattern: Constructed Mass Creature

### Construction

Use the rock generator to create:

- pelvis;
- torso;
- head;
- shoulder masses;
- arm segments;
- hand stones.

The stones may float with gaps between them.

### Animation

- transform hierarchy;
- procedural IK;
- spring lag;
- impact compression;
- dust in the gaps;
- rune core controlling cohesion.

The golem can reform from environmental rocks, linking actor and world systems.

### Pattern: Effect-Defined or Incomplete Body

### Construction

- mask;
- ribbon body;
- smoke;
- distortion;
- shadow;
- trailing particles.

The ribbon body can be generated from historical positions:

1. record recent head positions;
2. sample them;
3. create left and right ribbon vertices;
4. connect them into a strip;
5. vary width by age;
6. distort with noise;
7. fade the tail.

The body therefore emerges from movement.

### Pattern: Environmental Character

### Construction

- generated trunk;
- branches with gesture targets;
- knot-like eyes;
- mouth crack;
- snow;
- root network.

### Expression

- branch orientation;
- falling snow;
- bark separation;
- subtle trunk bend;
- light in cracks;
- wind pausing during speech;
- portrait and dialogue text.

The tree does not need facial animation to feel alive.

### Pattern: Animated Object Character

A signpost can become a character through:

- rotation;
- bending;
- creaking;
- painted or carved symbols;
- material changes;
- attached charms;
- shadow behavior;
- text timing;
- portrait art.

It may turn its arrows to lie, warn, or react.

The absurdity can be useful in a mythic world, particularly if the sign has a clear supernatural origin.

### Pattern: Autonomous Tool or Relic

A character may simply be:

- a sword;
- a shield;
- a lantern;
- a horn;
- a crown;
- a chain;
- a carved stone.

The object can communicate intent through:

- orientation;
- orbit;
- trail;
- rhythm;
- sound;
- light;
- shadow.

This expands the possible cast without expanding the traditional character-art burden.

---


## VFX as Anatomy

### Effects Are Not Decoration

For this style, effects are not merely polish. They are structural components of the visual design.

A character can be defined as:

```text
Raven = rigid bird form + shadow + wind trail + feathers
Ghost = mask + ribbon + smoke + distortion
Warrior = silhouette + attack arc + rune flash
Blob = sphere + squash + droplets + ground wave
Golem = rocks + dust joints + rune core
```

Removing the VFX would remove part of the character’s body language.

### Custom Trails

A reusable trail system can:

1. record world-space samples;
2. store age and orientation;
3. generate a ribbon mesh;
4. vary width;
5. fade over time;
6. distort in the shader;
7. support color, material, and noise profiles.

Trails can represent:

- swords;
- wind;
- spirits;
- bird dives;
- magic;
- roots;
- thrown weapons;
- movement afterimages.

A trail may be more visually important than the physical weapon.

### Ground Telegraphs

Telegraphs can use:

- decals;
- projected meshes;
- spline outlines;
- rune circles;
- cracks;
- snow displacement;
- shadows;
- particles moving inward;
- height-field ripples.

The telegraph should match the attacker’s visual grammar.

Examples:

- a raven telegraphs through its moving shadow;
- a golem telegraphs through cracks and displaced stones;
- a ghost telegraphs through a narrowing fog corridor;
- a blob telegraphs through a compressing ground ring;
- a masked figure telegraphs through gathering wind.

### Impact Language

A coherent impact system can combine:

- freeze-frame or brief animation hold;
- camera impulse;
- directional debris;
- target deformation;
- ground response;
- sound layering;
- trail termination;
- material flash;
- particle burst;
- delayed secondary movement.

The same underlying impact framework can produce different profiles:

- heavy;
- sharp;
- spectral;
- icy;
- organic;
- divine;
- corrupted.

---


## Combat as Graphic Choreography

### Readability Over Physical Accuracy

Telegraphed action combat benefits from stylization.

The player primarily needs to understand:

- who is attacking;
- where the attack will occur;
- when it becomes dangerous;
- what direction it moves;
- how long recovery lasts;
- what response is available.

A perfect sword swing is unnecessary if the attack communicates these facts with strong visual rhythm.

### Anticipation, Commitment, and Recovery

The most useful animation principles for this framework are often temporal rather than anatomical.

#### Anticipation

The attacker gathers energy or changes silhouette.

#### Commitment

The motion becomes difficult to cancel and the direction becomes clear.

#### Active phase

The attack occupies space.

#### Impact

The world responds.

#### Recovery

The attacker returns to vulnerability or readiness.

These phases can be exaggerated.

A silhouette may hold still longer than a realistic person. A raven may become a single arrow-like shape. A blob may flatten far beyond plausible material behavior.

Exaggeration produces clarity and style.

### Shared Motion Profiles

Motion profiles can be reusable:

- snap;
- heavy arc;
- float;
- spring;
- collapse;
- recoil;
- pulse;
- orbit;
- fold;
- dissolve;
- reform.

A motion profile defines:

- timing;
- easing;
- overshoot;
- damping;
- secondary lag;
- hold duration.

This is another visual vocabulary.

---


## Dialogue and Narrative Presentation

### Static Portraits Are Sufficient

Dialogue can use:

- static portraits;
- text;
- subtle portrait effects;
- environmental staging;
- limited actor motion;
- voice or sound cues.

Portraits may be:

- commissioned;
- generated and heavily curated;
- created from rendered 3D models;
- silhouettes;
- masks;
- relief carvings;
- symbolic representations.

The in-world character does not need facial animation.

### Environmental Dialogue Staging

During dialogue:

- the camera may shift slightly;
- wind may quiet;
- snow may pause near a speaker;
- nearby runes may activate;
- a mask may turn;
- a raven may hop closer;
- a fire may change;
- the background may darken;
- the speaker’s shadow may become more expressive.

Minimal movement can carry significance when the general visual style is restrained.

### Saga Tableaux

Important narrative scenes can become staged visual compositions.

Possible method:

1. freeze normal gameplay;
2. reposition characters into symbolic poses;
3. reduce or redirect lighting;
4. allow snow, fog, ribbons, or roots to continue moving;
5. reveal narration or dialogue;
6. move the camera slowly;
7. transition the world back into gameplay.

The tableau may resemble:

- a carved mural;
- a miniature stage;
- a memory;
- a ritual reenactment;
- a frozen moment in myth.

This avoids expensive cinematics while retaining visual ambition.

### Memories and Myths as Alternative Rendering Modes

A memory does not need to use the same representation as the present.

It can appear as:

- shadows on a wall;
- figures made from snow;
- constellations;
- floating masks;
- voxel fragments;
- roots forming silhouettes;
- moving carvings;
- miniature dioramas;
- spectral reenactments.

Different narrative layers can use different visual grammars.

---

## Action Authoring Tool

A timeline-like editor can display:

- action phases;
- pose targets;
- hitbox activation;
- trail width;
- VFX events;
- sound events;
- camera impulse;
- movement;
- recovery vulnerability.

The developer can adjust curves and immediately preview the attack.

This is art production through timing and systems.

---

## General Actor-Language Checklist

For every new actor family, define:

- its dominant silhouette;
- which pieces are solid, deformable, implied, or effect-driven;
- its idle rhythm;
- its anticipation language;
- how it commits to an action;
- how danger is telegraphed on the ground or in space;
- what constitutes impact;
- how it recovers;
- which part of the actor leads attention;
- which visual effects are structural rather than decorative;
- how the same action reads with effects temporarily disabled.

An actor is successful when the player can understand intent from timing and shape before relying on detailed anatomy.
