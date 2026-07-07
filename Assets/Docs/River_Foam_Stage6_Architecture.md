# River Foam Stage 6 Architecture

## Purpose

This is the canonical architecture document for Stage 6 river Foam.

It replaces the older persistent-morph, lateral-row-commit, and final-shader-stretch plans as the long-term source of truth. Older patch notes remain historical only. Any statement in older docs, logs, or handoff notes that contradicts this document is superseded by this architecture contract.

The roadmap owns macro stage order. The active blocker document owns the next patch sequence. This document owns what Foam is allowed to be.

## Current implementation state after 4.11C.5.9r

The current implementation is a stable but reduced Foam baseline:

- persistent Foam material birth exists;
- downstream phase transport exists;
- Remaining Life aging exists;
- topology/support/negative aging influence still exists;
- valid-fluid and obstacle clipping still exist;
- the stale neighbour-sampling persistent morph path was removed;
- the unsafe lateral row-commit paths were disabled after they caused smearing, pulsing, and then cell-scale shredding;
- the Unified Foam Motion Field and obstacle-routing field still exist as generated/debug-visible intent fields;
- the Foam Motion Field debug overlay uses raw stored `Presence` rather than final render mask;
- the Foam Motion Field + Cell Grid debug view shows the actual persistent foam simulation grid;
- dynamic/static disturbance fields still exist and remain important inputs for future shape behavior, but they no longer drive stored-state morphing;
- actual lateral material transport is currently not active;
- the safe evaluated shape/morphology layer is not implemented yet.

The current baseline is intentionally conservative. It is not the target final Foam behavior. Its purpose is to preserve stable material while the system is rebuilt around clear ownership boundaries.

## Non-negotiable architecture rule

Foam has exactly two foam data products and three processing stages.

Do not describe input fields, helper systems, or debug views as independent foam-authority layers. They are data sources or inspection tools. They do not get to mutate Foam by themselves.

```text
Persistent Foam State
    ↓ Stage 1 — Persistent State Update
Persistent Foam State after update
    ↓ Stage 2 — Shape Evaluation
Evaluated Foam Shape
    ↓ Stage 3 — Rendering
Final pixels
```

The anti-soup rule:

```text
Only one stage writes each product.
```

- Stage 1 is the only writer of Persistent Foam State.
- Stage 2 is the only writer of Evaluated Foam Shape.
- Stage 3 is the only producer of final rendered pixels.

Input fields may feed stages. Input fields do not directly move, erase, deform, or render Foam.

---

# 1. Foam data products

## Product A — Persistent Foam State

Persistent Foam State is the durable simulation state.

It answers:

```text
Where does foam material exist?
How long does it have left?
What stable material identity/pattern does it carry?
```

Current packed state:

```text
R = Presence
G = Presence × Remaining Life
B = Presence × Material Pattern
A = reserved / future use
```

### Presence

`Presence` means material coverage in a persistent foam simulation cell. It is not opacity, source amount, support, topology, visual breakup, or remaining life.

### Remaining Life

`Remaining Life` is the durable survival clock. It is the ordinary stored-state death authority, subject to the approved lifecycle/topology equation and valid-fluid clipping.

### Material Pattern

`Material Pattern` is stable material identity. It should travel with the stored material and can be used later by Stage 2 for deterministic holes, edge behavior, fracture thresholds, and visual identity.

### What Persistent Foam State must not store

Persistent Foam State must not store temporary visual bending, temporary holes, render-time cracks, one-frame disturbance breakup, final colour, final opacity, or evaluated shape artifacts.

---

## Product B — Evaluated Foam Shape

Evaluated Foam Shape is the current visible foam shape derived from Persistent Foam State.

It answers:

```text
What does the foam look like right now?
Where are its visible holes?
Where are its frayed edges?
Where is it temporarily bent, thinned, cracked, chipped, or joined?
Where does disturbance make it more active?
```

This product is where the reference-river behavior should be recovered: bending ribbons, broken sheets, chipping edges, holes, splits, soft joins, disturbance-reactive activity, and baseline living motion.

Evaluated Foam Shape may animate aggressively. It must not be written back into Persistent Foam State.

Likely future texture:

```text
_FoamShapeMask
```

A first implementation may only need one channel:

```text
R = evaluated visible foam mask
```

Possible future channels, if justified:

```text
G = breakup / edge activity / thinness helper
B = disturbance response / surface energy helper
A = reserved
```

The exact packing is not locked here. The contract is locked: Evaluated Foam Shape is derived, inspectable, and non-persistent.

---

# 2. Processing stages

## Stage 1 — Persistent State Update

Stage 1 writes Persistent Foam State.

It owns:

- birth/source-to-persistent merge;
- downstream material transport;
- future real lateral material transport;
- future obstacle-guided material transport;
- Remaining Life aging;
- support/negative aging response;
- valid-fluid clipping;
- obstacle/solid exclusion clipping.

Stage 1 decides where durable foam material exists over time.

### Stage 1 allowed behavior

Stage 1 may move stored material through an approved transport model. It may age or clip stored material through the lifecycle/valid-fluid contract. It may create stored material through approved birth/source events.

### Stage 1 forbidden behavior

Stage 1 must not perform temporary visual breakup, render stretch, fake cracks, coherent visual deformation, per-cell random lateral row shifting, or neighbour-sampled morphing that writes back into persistent state.

### Stage 1 current status

Currently active:

- manual birth;
- downstream phase transport;
- lifecycle aging;
- support/negative aging influence;
- valid-fluid clipping.

Currently disabled/missing:

- real lateral transport;
- obstacle-guided material transport.

---

## Stage 2 — Shape Evaluation

Stage 2 writes Evaluated Foam Shape.

It reads Persistent Foam State plus input fields, then produces the current visible foam shape.

It owns:

- coherent deformation;
- ribbon bending;
- ribbon stretching/compression appearance;
- edge bending/curling/flutter;
- edge fray;
- internal holes;
- fractures;
- chipping;
- ribbon splitting appearance;
- ribbon joining/reconnection appearance;
- disturbance-reactive breakup;
- baseline living morphology away from disturbances.

Stage 2 decides how stored foam looks this frame.

### Stage 2 allowed behavior

Stage 2 may sample Persistent Foam State to produce a deformed/evaluated visible mask. It may apply time-varying shape logic. It may use motion, disturbance, topology, life, pattern, and intrinsic morph fields to change the evaluated mask.

### Stage 2 forbidden behavior

Stage 2 must not write back to Persistent Foam State. It must not move durable foam material. It must not destroy durable foam material. It must not pretend to be Stage 1 lateral transport. It must not create large fake motion that contradicts raw material location.

### Stage 2 current status

Not yet implemented as a separate safe evaluated-shape product. This is the next major feature family after the docs/code compliance pass.

---

## Stage 3 — Rendering

Stage 3 draws Evaluated Foam Shape.

It owns:

- colour;
- opacity;
- lighting response;
- minimum night visibility;
- final blend;
- small edge polish;
- small pixel/anti-alias-style treatment.

Stage 3 decides how the evaluated shape is drawn.

### Stage 3 allowed behavior

Stage 3 may use the evaluated mask, foam colour, water lighting, night visibility, and material controls to produce final pixels. It may perform small final polish that does not create macro motion or macro shape changes.

### Stage 3 forbidden behavior

Stage 3 must not create macro stretch, macro lateral drift, fake obstacle routing, fake downstream shedding, large deformation, or fake split/join behavior as the primary source of foam behavior.

---

# 3. Input fields

Input fields are not foam products and not processing stages. They are data that stages consume.

## Intrinsic Morph Field

The Intrinsic Morph Field is the baseline "foam is alive even in ordinary water" driver.

It exists because foam must morph even away from rocks, waves, ripples, lee depressions, and static pressure.

Possible ingredients:

- foam grid coordinates;
- time;
- Material Pattern;
- low-frequency generated morph noise;
- medium-frequency generated breakup noise;
- optional stable pattern identity carried by the material.

Feeds:

- Stage 2 coherent deformation;
- Stage 2 breakup;
- Stage 2 edge fray;
- Stage 2 split/join appearance.

Does not feed:

- Stage 1 material movement.

## Motion Field

The Motion Field describes lateral/current/routing intent.

Ingredients:

- dense lane field;
- fixed obstacle-routing field;
- future flow curvature or current structure;
- future object routing influence.

Feeds:

- Stage 1 future real lateral transport;
- Stage 2 coherent deformation direction;
- Stage 2 ribbon bend/compression direction.

It does not move foam by itself.

Current status:

- field generation exists;
- obstacle-routing generation exists;
- debug view exists;
- cell-grid debug view exists;
- no active material movement consumes it after 5.9p.

## Disturbance Fields

Disturbance fields describe water activity and obstacle/wake pressure.

Includes:

- static pressure;
- static lee wake;
- dynamic wake;
- ripples;
- waves;
- surface energy;
- obstacle proximity/turbulence information where available.

Feeds:

- Stage 1 lifecycle/support/decay where explicitly approved;
- Stage 2 deformation strength;
- Stage 2 breakup strength;
- Stage 2 morph speed;
- Stage 2 edge activity;
- Stage 3 minor lighting/surface presentation.

Disturbance fields do not directly move or destroy foam by themselves.

## Topology / Support Fields

Topology/support fields describe where foam is supported, unsupported, excluded, or negatively aged.

Includes:

- major support;
- connector support;
- negative aging pressure;
- shore support;
- pressure/lee support;
- obstacle footprint / solid exclusion;
- valid-fluid mask.

Feeds:

- Stage 1 lifecycle;
- Stage 1 valid-fluid clipping;
- Stage 2 stability / breakup bias;
- Stage 2 joining/fracture bias.

Topology may help foam survive or destabilize visually. It must not become a direct movement or painting system.

---

# 4. Complete visual-feature mapping

Every foam feature must be mapped to a stage and input set before implementation. If a feature appears to belong to multiple places, split it into durable material behavior and evaluated visual behavior.

## Downstream travel

Belongs to:

```text
Stage 1 — Persistent State Update / Transport
```

Inputs:

- river flow speed;
- foam material flow multiplier;
- phase accumulator;
- river/foam coordinate grid.

Writes:

- Persistent Foam State.

Meaning:

- durable material moves downstream.

Forbidden:

- render shader stretching foam downstream as the primary travel mechanism;
- Shape Evaluation pretending to move the stored footprint downstream.

## Real lateral drift of the foam body

Belongs to:

```text
Stage 1 — Persistent State Update / Transport
```

Inputs:

- Motion Field;
- obstacle routing intent;
- future lateral residual/phase or other approved coherent transport state;
- river/foam coordinate grid.

Writes:

- Persistent Foam State.

Meaning:

- durable foam material changes lateral position over time.

Current status:

- not active.

Forbidden:

- fractional row weighting that smears material;
- per-cell stochastic row shifting that shreds material;
- render-only lateral warp pretending to be transport;
- persistent morph neighbour-sampling pretending to be transport.

Likely future direction:

- coherent lateral transport with accumulated lateral phase/residual or an equivalent patch-coherent method;
- motion influence smoothed at a visible patch scale;
- no independent random stay/move decision per texel.

## Obstacle-guided movement around rocks

Belongs to:

```text
Stage 1 for durable route change
Stage 2 for visible bend/compression/fray around the route
```

Inputs:

- Motion Field obstacle routing;
- static obstacle footprint;
- pressure field;
- lee field;
- valid-fluid boundary.

Stage 1 role:

- future durable lateral routing around the obstacle.

Stage 2 role:

- visible bending, compression, narrowing, chipping, and fraying near obstacle influence.

Forbidden:

- obstacle field independently erasing foam;
- shader faking obstacle bypass while stored foam goes straight through;
- per-cell obstacle routing that tears the patch apart.

## Chaotic wandering / non-obstacle lateral liveliness

Belongs to:

```text
Stage 1 for future slow durable lateral drift
Stage 2 for visible body/edge wobble
```

Inputs:

- Intrinsic Morph Field;
- Motion Field lane component;
- Material Pattern;
- time.

Stage 1 future role:

- slow persistent lateral path variation, if approved.

Stage 2 role:

- visible ribbon wiggle, bend, breathing, and non-rigid motion.

Forbidden:

- old chaotic drift as hidden stored morph;
- random per-cell movement;
- independent shader drift unrelated to stored/evaluated shape.

## Ribbon bending

Belongs to:

```text
Stage 2 — Shape Evaluation / Coherent Deformation
```

Inputs:

- Persistent Presence;
- Intrinsic Morph Field;
- Motion Field direction;
- disturbance deformation strength;
- Material Pattern;
- time.

Implementation concept:

```text
evaluatedMask(x) = samplePersistentPresence(x - deformationVector(x, t))
```

Rules:

- deformation vector is smooth over multiple foam cells;
- bounded in amplitude;
- gradient-limited;
- pattern-stable;
- disturbance-modulated;
- never written back to Persistent Foam State.

## Ribbon stretching / narrowing / compression

Belongs to:

```text
Stage 2 — Shape Evaluation / Coherent Deformation
```

Inputs:

- flow/motion direction;
- pressure/lee/wake influence;
- Intrinsic Morph Field;
- support/topology.

Meaning:

- the visible foam shape elongates, narrows, or compresses coherently.

Forbidden:

- final render lead/trail stretch as the primary behavior;
- stored-state neighbour resampling;
- cell-scale tearing.

## Edge bending / curling / flutter

Belongs to:

```text
Stage 2 — Shape Evaluation / Edge Morphology
```

Inputs:

- local edge estimate from Persistent Presence or deformed mask;
- Intrinsic Morph Field;
- Material Pattern;
- disturbance agitation;
- Motion Field direction;
- time.

Behavior:

- edges wobble;
- edges curl inward/outward;
- edges fray more than interiors;
- thin protrusions appear temporarily.

Forbidden:

- moving persistent material edge cells randomly.

## Internal holes opening and closing

Belongs to:

```text
Stage 2 — Shape Evaluation / Breakup Morphology
```

Inputs:

- Persistent Presence;
- Material Pattern;
- Intrinsic Morph Field;
- Remaining Life;
- disturbance agitation;
- support/topology;
- time.

Behavior:

- holes appear inside foam patches;
- holes expand/contract;
- holes close visually;
- older or disturbed foam may perforate more.

## Chipping / edge erosion / fraying

Belongs to:

```text
Stage 2 — Shape Evaluation / Breakup Morphology
```

Inputs:

- edge exposure;
- Intrinsic Morph Field;
- Material Pattern;
- disturbance agitation;
- Remaining Life;
- support/topology.

Behavior:

- small chunks visually disappear from edges;
- edges become ragged;
- thin fingers chip away.

Stage 1 may later age unsupported material, but fast visible chipping belongs to Stage 2.

Forbidden:

- hidden lifecycle death based purely on visual breakup noise.

## Ribbon splitting

Belongs to:

```text
Stage 2 — Shape Evaluation / Breakup Morphology
```

Inputs:

- deformed mask;
- Intrinsic Morph Field;
- disturbance agitation;
- strain from deformation field;
- Material Pattern;
- Remaining Life.

Behavior:

- a continuous ribbon visually opens into two strands;
- fracture lines cut through it;
- thin necks break visually.

Initial implementation should be evaluated/visual only. Persistent split is not needed first.

Forbidden:

- stored material being shredded into independent texel ribbons.

## Ribbon joining / reuniting

Belongs to:

```text
Stage 2 — Shape Evaluation / Soft Reconnection
```

Inputs:

- nearby evaluated/deformed foam mask;
- support/topology;
- Intrinsic Morph Field;
- low disturbance or lee support;
- flow alignment;
- Material Pattern.

Behavior:

- nearby strands visually bridge;
- small gaps close;
- thin ribbons reconnect.

Implementation direction:

- local dilation/bridge test in evaluated shape pass;
- limited radius;
- support-biased;
- flow-aligned;
- does not create persistent material.

Forbidden:

- permanent material spawn outside birth rules;
- large visual bridges far from stored foam.

## Foam breaking more around lee depressions

Belongs to:

```text
Stage 2 — Shape Evaluation / disturbance-driven breakup
```

Inputs:

- static lee wake;
- dynamic wake;
- support/topology;
- Intrinsic Morph Field.

Behavior:

- higher breakup agitation;
- more internal holes;
- faster edge fray;
- possible longer survival if lee support is high.

Important split:

```text
Stage 1: lee/support may increase lifetime.
Stage 2: lee/turbulence may increase visible breakup and morph activity.
```

These are not contradictory because they write different products.

## Foam breaking more around static pressure

Belongs to:

```text
Stage 2 — Shape Evaluation / pressure agitation
```

Inputs:

- static pressure field;
- obstacle footprint;
- Motion Field obstacle routing;
- Intrinsic Morph Field.

Behavior:

- foam compresses visually near pressure ridge;
- edges become more active;
- ribbons bend around pressure zone;
- breakup increases at sharp pressure gradients.

Stage 1 may also use pressure/support for lifetime/topology.

## Foam responding to waves/ripples

Belongs to:

```text
Stage 2 for shape animation
Stage 3 for small presentation shimmer
```

Inputs:

- dynamic ripple field;
- wave/surface motion field;
- surface energy;
- Intrinsic Morph Field.

Stage 2 behavior:

- faster small-scale breakup;
- pulsing hole thresholds;
- edge flutter;
- temporary thinning/thickening.

Stage 3 behavior:

- small brightness/opacity response;
- minor surface-light interaction.

Forbidden:

- large render-only shape relocation.

## Foam responding to dynamic wakes

Belongs to:

```text
Stage 2 for temporary agitation/deformation
Stage 1 maybe later for lifecycle/local support if explicitly approved
```

Inputs:

- dynamic wake field;
- ripple field;
- moving object disturbance.

Behavior:

- short-lived increased breakup;
- local deformation;
- faster visible morph rate;
- temporary churning.

Dynamic wake should use the same Stage 2 breakup/deformation controls as static disturbance. It must not become a separate foam mover.

## Foam aging / fading out

Belongs to:

```text
Stage 1 — Lifecycle
```

Inputs:

- Remaining Life;
- support/topology;
- negative fields;
- valid fluid.

Writes:

- Persistent Foam State.

Stage 2 may make old foam visually thinner/more broken, but it must not own durable death.

```text
life expiration = Stage 1
old/thin fragmented look = Stage 2
```

## Foam colour, brightness, opacity, lighting

Belongs to:

```text
Stage 3 — Rendering
```

Inputs:

- Evaluated Foam Shape;
- Foam Colour;
- lighting;
- minimum night visibility;
- surface values.

Behavior:

- draw the evaluated shape attractively.

Forbidden:

- macro movement;
- macro deformation;
- fake split/join.

---

# 5. Stage 2 detailed design target

Stage 2 is the missing feature family. It should be implemented as an evaluated-shape layer, not as a resurrected persistent morph path.

## Stage 2 inputs

- Persistent Presence;
- Remaining Life;
- Material Pattern;
- Intrinsic Morph Field;
- Motion Field;
- Static pressure;
- Static lee wake;
- Dynamic wake;
- Ripple/wave field;
- Topology/support;
- time;
- foam grid coordinates.

## Stage 2 output

At minimum:

```text
R = visible evaluated mask
```

Optional later channels:

```text
G = breakup / edge activity
B = disturbance response
A = reserved
```

## Stage 2 sub-steps

### 2A — Resolve shape influences

For each foam cell, combine inputs into one set of shape controls:

```text
baseMorph
morphSpeed
deformationStrength
breakupStrength
edgeFrayStrength
reconnectStrength
supportStability
```

All disturbance, motion, topology, life, and intrinsic inputs that affect visible shape are resolved here. Later sub-steps consume these resolved values rather than independently inventing their own strengths.

### 2B — Coherent deformation

Compute a smooth deformation vector.

Inputs:

- Intrinsic Morph Field;
- Motion Field direction;
- disturbance gradients;
- Material Pattern;
- time.

Output:

```text
deformationVector
```

Rules:

- smooth over multiple cells;
- bounded amplitude;
- no per-cell random offsets;
- no unbounded stretch;
- no writeback to Persistent Foam State.

Then sample:

```text
deformedMask = samplePersistentPresence(cell - deformationVector)
```

This creates ribbon bending, stretching, narrowing, compression, and local flow appearance without moving stored material.

### 2C — Breakup morphology

Compute breakup from:

- Material Pattern;
- Intrinsic Morph Field;
- Remaining Life;
- edge exposure;
- disturbance agitation;
- deformation strain;
- support/topology.

Apply to the deformed mask:

```text
brokenMask = deformedMask × breakupMask
```

This creates holes, cracks, frayed edges, chipping, and thin-strand breakup.

### 2D — Soft reconnection

Use the local evaluated/deformed neighborhood.

Inputs:

- brokenMask;
- support/topology;
- flow alignment;
- reconnectStrength;
- low/moderate agitation.

Behavior:

- nearby strands can bridge visually;
- small gaps can close;
- ribbons can reunite.

Rules:

- small radius only;
- support-biased;
- does not spawn persistent material;
- does not bridge huge gaps.

### 2E — Output evaluated shape

Write the evaluated mask into the evaluated shape product, then allow debug/final rendering to sample the same product.

---

# 6. How influences combine without fighting

Every foam-related influence must be classified as one or more of these types:

```text
movement influence
deformation influence
breakup influence
lifetime influence
render influence
```

Then it may affect only the matching stage output.

Examples:

```text
Motion Field:
  movement influence → Stage 1 future transport
  deformation influence → Stage 2 bend direction

Static pressure:
  lifetime/support influence → Stage 1 lifecycle/topology
  deformation/breakup influence → Stage 2 pressure agitation

Dynamic wake:
  breakup/deformation influence → Stage 2
  possible lifetime influence → Stage 1 only if explicitly approved

Material Pattern:
  carried material identity → Stage 1 storage
  morphology identity → Stage 2

Remaining Life:
  durable survival → Stage 1
  old/thin visual look → Stage 2
```

No direct cross-writing is allowed.

---

# 7. Forbidden patterns from failed patches

The following are explicitly rejected:

- no persistent morph pass that samples neighbouring foam and writes it back;
- no fractional lateral row weighting as material movement;
- no per-cell stochastic lateral commit;
- no final shader macro stretch pretending to be foam behavior;
- no input field directly mutates foam;
- no debug view may imply stored state if it is showing final render mask;
- no layer may conceal broken transport by adding unrelated visual noise.

Reference-river tearing and our failed tearing are different phenomena:

```text
Reference tearing = coherent evaluated shape behavior.
Our broken tearing = persistent material shredded by cell-scale transport.
```

This distinction is permanent. Desired tearing belongs to Stage 2. Cell-scale material shredding is a bug.

---

# 8. Debug contract

Debug views must identify which product they show.

## Raw Material Presence

Shows Persistent Foam State only.

Use it to verify stored material, lifetime, transport, and clipping.

## Foam Motion Field

Shows lateral/obstacle intent plus raw stored presence overlay.

Use it to compare stored foam footprint against Motion Field intent. It does not prove movement unless Stage 1 has an active movement consumer.

## Foam Motion Field + Cell Grid

Shows Motion Field plus raw stored presence plus actual persistent foam simulation cell boundaries.

Use it to understand cell scale, one-row shifts, texel-size artifacts, and whether a behavior is cell-scale or patch-scale.

## Future Evaluated Shape

Should show Evaluated Foam Shape after Stage 2, before Stage 3 final colour/lighting.

## Final Foam

Shows final rendered result.

Use it for final presentation judgment only after raw/evaluated products behave correctly.

---

# 9. Performance contract

Stage 2 should preferably run at foam texture resolution rather than screen-pixel resolution for macro shape behavior.

Recommended direction:

```text
one compute pass over foam grid
writes compact evaluated mask texture
shader samples evaluated mask
```

Why:

- cost scales with foam field resolution;
- debug and final render can use the same evaluated result;
- macro behavior is inspectable;
- rendering remains focused on colour/light/blend;
- avoids hidden screen-space macro deformation in the final shader.

Memory is acceptable if runtime compute remains bounded and the texture is compact.

---

# 10. Feature recovery order

The recovery order is part of the architecture contract because order prevents systems from fighting.

1. Architecture contract and docs alignment.
2. Code compliance audit against this contract.
3. Shape Evaluation foundation: create the evaluated-shape product and debug view with minimal/no deformation.
4. Intrinsic morphology: restore baseline living foam everywhere through Stage 2 only.
5. Disturbance-driven morphology: reconnect static pressure, lee, dynamic wake, ripples, and waves as Stage 2 modifiers.
6. Coherent deformation: add smooth, bounded inverse deformation for ribbon bending/compression/stretch.
7. Soft split/join: add evaluated local breakup/reconnection behavior.
8. Real lateral transport: redesign Stage 1 lateral movement without row-weight smearing or per-cell dither shredding.
9. Final rendering polish: make Stage 3 draw the evaluated result without inventing macro behavior.

Automatic anchored/open-water birth population remains deferred until manually-born material, evaluated shape, and transport contracts are healthy.
