# 07 — Implementation Guide for the Visual Framework Proof of Concept

## Status
Companion document to **Part 06 — Proof of Concept**.

## Purpose
This document turns the proof of concept from a design idea into a practical beginner-friendly build guide. It is not meant to be a perfect production architecture. It is meant to help a solo developer or beginner game developer start building the scene without getting lost.

It assumes that the reader:

- is new or relatively new to game development,
- is willing to write some C#,
- is using Unity,
- is more comfortable with systems and logic than with conventional art production,
- wants a concrete path to something visible and playable.

It does **not** assume deep knowledge of shaders, procedural generation, animation rigs, or advanced rendering.

---

# 1. What this guide is trying to achieve

By the end of this guide, you should have a small playable scene that demonstrates the framework:

- a fixed or tightly controlled camera,
- a small stylized clearing,
- simple but coherent rendering,
- a few generated environment assets,
- at least one actor built from simple parts,
- at least one readable stylized attack,
- some limited procedural variation,
- a workflow you can continue to build on.

This is **not** the full game.

It is also **not** a final engine framework.

It is a proof of concept that answers one critical question:

> Can this style of making a game actually work for you?

If the answer is yes, you can refine the implementation later. If the answer is no, it is better to find out now than after months of architecture work.

---

# 2. What the prototype should contain

Keep the target modest.

## Minimum scene contents

Environment:

- one ground patch,
- one hut or ruin,
- one river or path,
- one small bridge,
- one dead tree or similarly simple organic object,
- several rocks,
- a carved stone or landmark,
- fog, snow, and a basic light setup.

Actors:

- one player placeholder,
- one rigid-part actor,
- one deformation-based actor,
- one trajectory-based actor or a simpler substitute.

Gameplay / interaction:

- walking,
- one dialogue interaction,
- one attack or ability test,
- one enemy reaction,
- one seed button or regeneration action.

This is enough.

If you build more than this before the foundation works, you are probably wasting time.

---

# 3. Recommended tools and setup

## Engine

Use **Unity 6 with URP**.

Why:

- good enough rendering,
- good editor workflow,
- Shader Graph support,
- easy scripting access,
- easy prefab iteration,
- large amount of learning material.

## Packages to install early

Keep the list small.

Install:

- Universal Render Pipeline
- Shader Graph
- Splines
- Input System (if not already in your project)
- TextMeshPro

Optional for later:

- Cinemachine
- VFX Graph
- Animation Rigging

Do not begin by adding every package you may someday need.

## Source control

Use Git immediately.

Even if you are a beginner, do not skip this.

At minimum:

- ignore Library, Temp, Logs, Obj,
- commit after every stable milestone,
- tag milestones like `blockout`, `first-shader`, `generated-rocks`, `first-combat`.

---

# 4. Suggested folder structure

Keep things boring and readable.

```text
Assets/
└── Game/
    ├── Demo/
    │   ├── Scenes/
    │   ├── Prefabs/
    │   ├── Materials/
    │   ├── Meshes/
    │   └── ScriptableObjects/
    │
    ├── Scripts/
    │   ├── Core/
    │   ├── Environment/
    │   ├── Actors/
    │   ├── Combat/
    │   ├── Effects/
    │   └── Editor/
    │
    ├── Shaders/
    ├── Art/
    │   ├── Placeholder/
    │   └── Generated/
    └── Audio/
```

The point is not perfection. The point is not having scripts scattered randomly across the project.

---

# 5. Build order: the correct sequence

Many beginners make the mistake of starting with the “coolest” system. That is usually the wrong system.

Build in this order:

1. Project setup
2. Primitive blockout scene
3. Camera and composition
4. Basic movement controller
5. Lighting, fog, palette, and placeholder materials
6. Ground patch generator
7. Rock generator
8. River or path ribbon
9. Bridge assembler
10. Hut assembler
11. Tree / landmark object
12. Rigid-part actor system
13. One simple attack
14. One deforming enemy
15. One scene regeneration system
16. Polish and evaluation

If you skip ahead to procedural world generation, you will bury yourself.

---

# 6. Step One — Create the primitive blockout scene

Open Unity and make a scene called:

`VisualFrameworkDemo.unity`

## Add the following objects using primitives only

- Ground: a plane or scaled cube
- Hut: a few cubes
- Bridge: several small cubes over a gap
- Tree: a cylinder trunk and some thin branch cubes
- Rocks: spheres or scaled cubes
- Landmark: a tall cube or cylinder
- Player: a capsule
- Enemy A: a sphere
- Enemy B: a few cubes or spheres

This is ugly on purpose.

## Why this step matters

You are solving:

- scene scale,
- spacing,
- camera readability,
- object proportions,
- navigation,
- silhouettes.

You are **not** trying to impress anyone yet.

If the composition is bad now, it will remain bad after you add procedural tricks.

## Scene layout advice

Keep the scene compact.

A good starting size is roughly **40 x 40 meters**.

Try this rough arrangement:

- player starts near one edge,
- hut sits off-center,
- river crosses one side,
- bridge provides a clear visual feature,
- dead tree frames the scene,
- rocks break up space,
- one open area serves as a test arena,
- one landmark sits somewhere visible from the camera.

Think in terms of **foreground**, **middle ground**, and **background**.

---

# 7. Step Two — Set up the camera

For this style, camera matters enormously.

## Recommended starting values

Try a perspective camera with:

- position high above the scene,
- downward angle around 45–55 degrees,
- slight horizontal rotation,
- FOV around 30–40.

These are not sacred numbers. They are just reasonable starting points.

## Camera rules for the prototype

- Keep camera movement simple.
- If possible, keep camera fixed at first.
- If the player moves, let the camera follow gently, not aggressively.
- Do not add free rotation.

Free camera rotation dramatically increases the burden on your content.

## What to evaluate

Stand in the scene and ask:

- Can I read object scale clearly?
- Are important objects visible?
- Do silhouettes overlap too much?
- Is there a clear sense of place?
- Does the scene feel composed rather than randomly arranged?

Only once this feels decent should you proceed.

---

# 8. Step Three — Add a very simple player controller

This can be extremely plain.

You only need enough control to move around and inspect the scene.

Because your project uses the **new Unity Input System**, keep the controller compatible with it.

## Minimum player movement features

- move on the XZ plane,
- rotate toward movement direction or cursor direction,
- optional simple dash later,
- no advanced combat yet.

If you do not already have a movement controller in your main project, make a temporary one for the prototype.

The exact controller is less important than being able to test readability from real movement.

---

# 9. Step Four — Establish the visual baseline

Now you turn the primitive blockout from “grey boxes” into something that at least has an aesthetic direction.

## Lighting

Start with:

- one directional light,
- soft shadows,
- cool tone,
- a bit of fog,
- one warm point light near the hut or fire,
- a neutral skybox.

If the game is wintry, the overall temperature should be cool, with a limited warm accent.

## Post-processing

Keep it minimal:

- light color adjustment,
- subtle bloom,
- maybe slight vignette,
- perhaps slight tonemapping if needed.

No heavy stylization filters yet.

## Palette

Pick a small palette.

For example, the scene might rely mainly on:

- snow / pale surfaces,
- dark stone,
- weathered wood,
- muted vegetation,
- one magical accent color,
- one warm fire-like accent color.

Do not let every object become a separate colour statement.

## First materials

Create simple materials even before shaders become sophisticated.

Make materials such as:

- `MAT_Stone`
- `MAT_Wood`
- `MAT_Snow`
- `MAT_Spirit`
- `MAT_Water`

At this stage, even ordinary URP Lit materials are acceptable.

The goal is simply to move away from default grey primitives.

---

# 10. Step Five — Create the first custom shader(s)

You do **not** need advanced shader knowledge immediately.

Make two Shader Graphs:

- `SG_StylizedOpaque`
- `SG_StylizedSpirit`

## Opaque shader: first version

Features to include:

- base color,
- secondary color,
- top-facing snow tint,
- world-space noise or variation,
- emission optional.

Even a very crude top-facing snow effect is enough.

For example, in Shader Graph:

1. Get world normal.
2. Dot it with world up.
3. Use a `Smoothstep`.
4. Use that as a blend factor between base surface color and snow color.

This gives you a simple “snow settles on upward surfaces” effect.

That one effect already creates cohesion.

## Spirit shader: first version

Keep it simple.

Features:

- emissive color,
- fresnel glow,
- simple dissolve or noise,
- optional transparency or alpha clip.

Use it for magical or supernatural objects only.

Do not overuse it.

---

# 11. Step Six — Build the ground patch generator

Now we move into actual procedural content.

The first custom mesh should be the **ground patch**.

## Why start with the ground patch

Because it affects everything else:

- object placement,
- scene silhouette,
- shadows,
- path readability,
- river depression,
- local atmosphere.

## First implementation approach

Generate a grid mesh.

For example:

- 33 x 33 or 65 x 65 vertices,
- fixed size,
- heights determined by simple functions.

### Data you need

- width and length,
- resolution,
- noise strength,
- noise scale,
- one or more flattening zones,
- optional river depression.

### Simplified algorithm

For each grid point:

1. Compute its world or local XZ position.
2. Evaluate broad noise.
3. Evaluate smaller detail noise.
4. Apply a flattening factor if near camp / player / hut.
5. Apply river depression if near river centerline.
6. Store resulting height.

Then create triangles between neighboring vertices.

## Advice for beginners

Do **not** chase perfect terrain generation.

You are not building an open-world terrain engine. You are building one small scene.

Your ground should simply be:

- uneven enough to catch light,
- smooth enough to walk on,
- shaped enough to support a path or river,
- simple enough to debug.

## First success condition

The scene should look better with the generated ground than it did with a flat plane.

If it looks worse, keep the generator simple and fix the result instead of adding more complexity.

---

# 12. Step Seven — Build the rock generator

The rock generator is the best first procedural prop generator.

## Why rocks are ideal

- they tolerate irregularity,
- they are useful everywhere,
- they reinforce the style,
- they can later inform cliffs or creatures,
- they do not require difficult UVs,
- they work well with snow.

## Simple implementation strategy

There are two reasonable beginner approaches:

### Approach A — Deform a sphere mesh

- Start from a sphere or icosphere mesh.
- Read its vertices.
- Move each vertex outward or inward based on noise.
- Apply axis scaling.
- Flatten the bottom slightly.
- Recalculate normals.

### Approach B — Assemble several primitive forms

- Take two or three low-poly lumps or cubes.
- Combine them as a clustered rock.
- Randomize scale and rotation.

Approach A is cleaner if you want to learn procedural mesh work.

## Recommended rock parameters

- seed,
- size,
- width/height/depth ratios,
- noise strength,
- noise scale,
- bottom flattening,
- tilt,
- optional fracture amount.

## Placement strategy

You do not need to generate their positions yet.

Place 8–15 rock generators manually in the scene, then randomize their seeds.

That gives you manual composition plus generated form.

## Acceptance test

When you stand in the scene:

- they should look like members of one family,
- they should not all look identical,
- snow should affect them similarly,
- they should support composition rather than clutter it.

If necessary, reduce variation rather than increase it. Too much variation usually destroys cohesion.

---

# 13. Step Eight — Build a ribbon-based river or path

A spline-based feature is one of the most useful systems in the prototype.

You can start with either a path or a river. A river is visually more striking, but a path is easier.

## Use Unity Splines

Create a spline that runs through your scene.

For a path:

- create a ribbon mesh along the spline,
- slightly flatten the ground under it,
- optionally darken or lighten the surface,
- optionally remove clutter near it.

For a river:

- carve a depression in the ground,
- create a flat or slightly wavy ribbon mesh for water,
- add some bank rocks,
- maybe add simple movement in the shader.

## First version should be simple

You do not need:

- physically simulated water,
- realistic shore erosion,
- advanced foam,
- perfect terrain blending.

You need something that is:

- readable,
- attractive enough,
- useful compositionally.

## Important concept

Splines are not just for rivers.

The same underlying concept can later power:

- roads,
- roots,
- energy beams,
- ghost bodies,
- attack trails,
- ropes,
- decorative trims.

That is why building a good ribbon / spline mindset now is valuable.

---

# 14. Step Nine — Build the bridge assembler

The bridge is an ideal example of **assembling simple pieces into a stylized object**.

## First version

Create a component that:

- samples positions along a spline or line,
- places planks at intervals,
- adds slight random offsets and rotations,
- places support posts every few planks,
- optionally adds side ropes.

This does not need to generate one combined mesh at first.

Instantiating repeated pieces is fine for the prototype.

## Parameters

- plank count or spacing,
- plank size,
- width,
- bridge sag,
- randomness amount,
- support spacing,
- rope on/off.

## Lessons this teaches

- not all procedural art needs custom mesh generation,
- repeating parts can still look handcrafted if variation is tasteful,
- spline-driven construction is powerful.

---

# 15. Step Ten — Build the hut assembler

The hut is one of the first generated objects that starts to feel like “real game art” rather than just a technical exercise.

## Keep the hut primitive

A beginner mistake is attempting too much detail too early.

Start with:

- a rectangular footprint,
- four main posts,
- simple wall sections,
- a pitched roof,
- a doorway,
- maybe a few supports or broken pieces.

## Implementation strategy

Build it from simple parts.

- posts: scaled cubes or generated prisms,
- wall panels: simple quads or cubes,
- roof panels: sloped planes or cubes,
- debris: a few broken pieces placed near the base.

## Key parameters

- width,
- length,
- wall height,
- roof pitch,
- lean amount,
- damage amount,
- snow amount,
- doorway side.

## Good beginner rule

Every parameter should have a visible purpose.

If you cannot explain what a parameter does artistically, it probably does not need to exist yet.

## What “damage” can mean

Do not overcomplicate it.

Damage can simply:

- omit a wall panel,
- skew a roof piece,
- tilt the whole hut slightly,
- add debris,
- lower one corner.

That is enough to make variants feel distinct.

---

# 16. Step Eleven — Add a dead tree or landmark generator

This is optional before the first actor, but useful for visual range.

## Simplest version

Use a trunk plus several branch segments.

Each branch can just be:

- a start position,
- a direction,
- a length,
- a thickness.

Then create stretched cylinders or prisms for them.

## Important stylistic decision

Do not aim for realistic botanical behavior.

Aim for strong silhouette.

A good stylized dead tree is mostly a composition problem, not a biology problem.

## Alternative

If the tree becomes frustrating, replace it temporarily with a standing stone, carved post, or simple ruin fragment.

You are allowed to defer difficult pieces.

---

# 17. Step Twelve — Create the first actor framework

Do **not** start with a fully rigged humanoid.

Start with a **part-based actor**.

## What a part-based actor is

An actor made from separate transforms:

- body,
- head,
- left arm or wing,
- right arm or wing,
- effect origin,
- shadow object.

Each part can be a primitive mesh.

Examples:

- a masked figure,
- floating armor,
- a bird,
- a small spirit,
- a construct.

## First system to build

Make a script that stores references to named parts.

For example:

- Body
- Head
- LeftPart
- RightPart
- WeaponOrigin

Then create a “pose” representation.

A pose is just local transforms for those parts.

## Beginner-friendly implementation choice

You do not need a full pose editor tool on day one.

You can start with a script containing a few public fields or a ScriptableObject that stores:

- local position,
- local rotation,
- local scale.

Then write a function that interpolates the current part transforms toward those target transforms.

That already gives you stylized animation behavior.

---

# 18. Step Thirteen — Make one simple stylized attack

Build only **one** attack at first.

A slash-like supernatural attack is a good candidate because it tests telegraphing, timing, trails, and impact.

## The phases

Use four phases:

1. anticipation,
2. active,
3. impact,
4. recovery.

## What each phase does

### Anticipation

- actor leans,
- head or mask turns,
- particles or trail energy gathers,
- maybe a ground telegraph appears.

### Active

- root rotates or moves,
- the attack trail appears,
- damage becomes active.

### Impact

- brief pause,
- particles burst,
- target reacts.

### Recovery

- actor returns to idle,
- trail fades,
- particles dissipate.

## First implementation shortcut

You do not need a generic action framework immediately.

You can hardcode this first attack in one script.

If the result is good, then abstract it into a reusable action system.

This is important. Beginners often over-abstract too early.

## What success looks like

Even with ugly primitives, the action should be readable:

- I can see it preparing,
- I understand when it becomes dangerous,
- the impact feels distinct,
- it resolves clearly.

If these things are not clear without fancy effects, the action design is weak.

---

# 19. Step Fourteen — Make a deforming actor

Now create a second actor whose expression comes from deformation rather than articulated parts.

A blob-like creature is a good beginner choice.

## Implementation options

### Simple transform-only approach

Use one sphere mesh.

On anticipation:

- scale Y downward,
- scale XZ upward.

On jump:

- scale Y upward,
- scale XZ inward.

On landing:

- scale Y downward strongly,
- restore gradually.

This already creates squash and stretch.

### Slightly more advanced approach

Add shader-driven wobble or vertex displacement.

But you do not need that initially.

## Why this actor matters

It proves that not every expressive actor needs a skeleton or rig.

It also gives contrast against the more rigid actor.

---

# 20. Step Fifteen — Add a trajectory-based actor or simple substitute

The third actor type tests motion readability.

A flying creature is ideal, but if that becomes difficult, use any actor whose identity is defined by trajectory and speed.

## Goals

- a clear curved movement,
- distinct anticipation,
- visually strong movement arc,
- readable threat.

This can be as simple as:

- a bird-like object,
- a darting spirit,
- a projectile-like creature,
- a diving construct.

Again, do not focus on realism.

Focus on **shape + movement + trail**.

---

# 21. Step Sixteen — Create a simple trail system

A trail is one of the highest-value effects you can build.

## First version approach

You have two options:

### Option A — Use Unity’s built-in Trail Renderer

This is completely acceptable for a first implementation.

Use it to test:

- attack arcs,
- diving motion,
- spirit trails.

### Option B — Build a custom ribbon trail later

A custom trail is more flexible, but only build it once you understand exactly what you need.

## Beginner recommendation

Start with the built-in Trail Renderer.

If it gives you the look you want, good. If not, then build a custom solution.

No need to suffer unnecessarily.

---

# 22. Step Seventeen — Add limited dialogue interaction

The framework includes the idea that quieter scenes matter too.

So add one trivial interaction.

## Minimum version

- walk up to actor,
- press interact,
- show a panel with portrait and text,
- press again to close.

That is enough.

You are not testing narrative systems here. You are testing whether the visual language can support non-combat presence.

A static portrait plus text is perfectly fine.

---

# 23. Step Eighteen — Add scene regeneration

At this point the scene has enough pieces that variation becomes interesting.

## What to regenerate

- rock seeds,
- hut seed or damage state,
- bridge irregularity,
- tree shape,
- small prop placement,
- maybe snow amount.

## What not to regenerate

- major object positions,
- player start,
- path of traversal,
- combat arena,
- key sight lines.

## Best interface

For the prototype, add a button in the inspector or a simple editor script:

- `Regenerate Scene`
- `Randomize All Seeds`

This makes it easy to iterate.

## Important principle

The scene should feel like the **same place in a different expression**, not a different place entirely.

---

# 24. Step Nineteen — Optional selective voxel experiment

Do this only if the rest is already working.

## Good use cases

- hit effect where a stone surface breaks into small cubes,
- magical dissolve,
- death effect,
- corruption spreading in stepped blocks,
- an object assembling itself from voxels.

## Beginner version

Fake it.

Spawn a bunch of little cubes:

- give them initial positions in a volume,
- launch or drift them,
- fade them or destroy them.

No need to build a voxel terrain engine.

The goal is aesthetic testing, not technical maximalism.

---

# 25. How to know when to refactor

A common trap is trying to build the “perfect architecture” from the first script.

For this prototype, follow this rule:

## Hardcode first, abstract second

If a feature works and you can clearly see repetition, then refactor.

Examples:

- one attack hardcoded first,
- one rock generator first,
- one hut generator first,
- one trail solution first.

Only after they prove useful should you build:

- shared action definitions,
- reusable pose systems,
- more generic generation utilities,
- broader content authoring tools.

This is the difference between building a working prototype and writing an overgeneralized framework that generates nothing.

---

# 26. A practical milestone schedule

Here is a realistic milestone sequence.

## Milestone 1 — Blockout and camera

Deliverables:

- blockout scene,
- playable movement,
- fixed camera,
- composition established.

Question answered:

- Is the scene worth developing further?

## Milestone 2 — Basic visual identity

Deliverables:

- lighting,
- fog,
- palette,
- first materials,
- simple post-processing.

Question answered:

- Does the scene begin to feel like a world?

## Milestone 3 — Generated environment basics

Deliverables:

- ground patch,
- rocks,
- path or river,
- bridge.

Question answered:

- Can generated forms replace some conventional asset production?

## Milestone 4 — First authored structure

Deliverables:

- hut or ruin,
- tree or landmark,
- minor dressing.

Question answered:

- Can your generation approach create recognizable environmental identity?

## Milestone 5 — First actor and attack

Deliverables:

- part-based actor,
- one stylized attack,
- simple trail,
- simple hit reaction.

Question answered:

- Can stylized performance work without conventional animation?

## Milestone 6 — Variation and second actor

Deliverables:

- deforming actor,
- regeneration button,
- small conversation interaction.

Question answered:

- Does the framework support more than one expressive mode?

## Milestone 7 — Evaluation pass

Deliverables:

- one clean presentation build,
- list of what works,
- list of what fails,
- next-step decision.

Question answered:

- Should you scale this approach up?

---

# 27. Common mistakes to avoid

## Mistake 1 — Starting with procedural world generation

Wrong priority.

If one scene is not compelling, generating 500 scenes is pointless.

## Mistake 2 — Chasing realism

This framework works best when it embraces stylization.

Trying to compete with realistic AAA environments will only expose the limitations of a small team or solo workflow.

## Mistake 3 — Overbuilding tools before testing art direction

A rock generator that takes three weeks to build but produces weak rocks is worse than three hand-placed placeholder rocks that already serve the composition.

## Mistake 4 — Letting randomness control composition

Randomness should enrich a place, not author the place for you.

## Mistake 5 — Too many colors or materials

A tight visual vocabulary is what makes simple geometry feel deliberate.

## Mistake 6 — Effects compensating for unreadable motion

If the action only feels good once buried under particles, the core motion probably is not good enough.

## Mistake 7 — Building production architecture too early

Prototype code is allowed to be slightly ugly if it helps you discover the right structure.

---

# 28. What to cut if you get overwhelmed

If you start drowning in complexity, cut in this order:

1. selective voxel experiment,
2. third actor,
3. advanced tree generator,
4. custom trail system,
5. dialogue polish,
6. hut damage variation,
7. river in favor of path.

Do **not** cut:

- camera,
- composition,
- one generated asset family,
- one coherent material language,
- one expressive actor,
- one readable attack.

Those are the heart of the prototype.

---

# 29. What the beginner should learn from this prototype

Even if you later rewrite everything, this proof of concept teaches several important disciplines:

- composing a space rather than only building systems,
- using constraints to achieve style,
- thinking of procedural generation as an artistic tool,
- building content grammars,
- making readable animation out of timing and silhouette,
- distinguishing prototype code from production code,
- evaluating an approach honestly before scaling it.

This is why the prototype matters.

It is not only about producing a nice screenshot.

It is about teaching you a usable mode of development.

---

# 30. What to do after the prototype works

If the prototype succeeds, your next step is not immediately “make the whole game.”

Instead:

1. clean up the parts that are obviously reusable,
2. identify which generators deserve proper tools,
3. decide what should be editor-time only and what should remain runtime-capable,
4. formalize the action / pose system if it proved worthwhile,
5. decide whether the visual language still feels appropriate for your project,
6. begin designing a second environment or encounter to test breadth.

A good next experiment after the clearing would be something that stresses the same language in a different way, such as:

- a tighter ruin space,
- a stronger weather scene,
- a more dramatic combat arena,
- a more overtly supernatural environment.

If the framework still works in a second context, confidence increases significantly.

---

# 31. Final advice

As a beginner or near-beginner, your biggest danger is not lack of intelligence. It is **scope distortion**.

You can absolutely build this proof of concept if you keep the questions narrow:

- Does the camera work?
- Does the scene read?
- Do the generated rocks look intentional?
- Does the bridge feel part of the same world?
- Does one stylized actor feel alive?
- Does one attack read clearly?
- Does limited regeneration enrich the scene?

That is enough.

The prototype does not need to prove that you can build the whole game.

It only needs to prove that this **way of making the game** is viable.

If it proves that, then the prototype has done its job.

---

# 32. Quick-start checklist

If you want the shortest possible starting list, use this:

## Day 1–2

- Create URP project
- Create demo scene
- Block out clearing with primitives
- Set fixed camera
- Add basic movement

## Day 3–4

- Add lighting and fog
- Create 4–5 placeholder materials
- Make scene feel coherent enough

## Day 5–7

- Build ground patch generator
- Build rock generator
- Place 10 rock instances

## Day 8–10

- Build path or river spline
- Assemble bridge
- Add one hut

## Day 11–13

- Build part-based actor
- Add one attack
- Add simple trail

## Day 14–16

- Build deforming blob-like actor
- Add one dialogue interaction
- Add regeneration button

## Day 17+

- Evaluate honestly
- Refine weak areas
- Decide whether to continue the framework

That is a sensible beginner roadmap.

---

# 33. Relationship to other documents

Use this document together with:

- **Part 00** for navigation and terminology,
- **Part 01** when adjusting the look and material language,
- **Part 02** when building generators and asset grammars,
- **Part 03** when building actors and attacks,
- **Part 06** for the conceptual scope and success criteria of the proof of concept.

This document is intentionally practical. It is the bridge between the exploratory framework and actually opening Unity and starting.
