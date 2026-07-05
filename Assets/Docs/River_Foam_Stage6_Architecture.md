# River Foam Stage 6 Architecture

## Purpose

This is the canonical architecture document for Stage 6 river Foam and surface tracing. It replaces the older material-state, topology, and progressive-scheduling implementation plans as the long-term source of truth.

The roadmap owns macro stage order. The active blocker document owns the current patch sequence. This document owns what Foam is allowed to be.

## Current status

Stage 6 is in the `4.11C` manually-born persistent material phase.

Accepted or mostly accepted foundations:

- persistent Foam material state exists;
- material aging uses Neutral Lifetime, Supported Aging Rate, and Negative Aging Rate;
- topology influences lifespan instead of directly painting Foam;
- the lifetime delta-time rebind fixed the bug where material did not age;
- support/negative overlap now makes negative pressure suppress support before accelerating age;
- manual birth controls are consolidated under `Foam Debug > Manual Birth Source`;
- the hidden multi-writer spawn scaling problem was corrected by the 5.4l composition-event/birth-budget refactor;
- the 5.4m realignment removes active pattern/complexity/density birth controls, restores one canonical stable manual source, and keeps those controls in a dedicated Inspector foldout;
- the 5.5 material-evolution pass adds the first persistent `Presence` morphing layer; 5.5c restores lifecycle authority after the initial erosion attempt shortened Foam life incorrectly; 5.5d changes morphing to area-balanced intrinsic wobble so deformation does not continually grow the footprint.

Still not accepted:

- 5.5d area-balanced intrinsic wobble needs validation without independent `Presence` death paths or monotonic area growth;
- organic breakup is not proven;
- topology interaction needs final proof and calibration;
- lateral drift/obstacle sliding is not implemented;
- obstacle interaction can still read as hard clipping.

Current blockers and patch order live in `River_Foam_Active_Blockers_and_Next_Patches.md`.

## Reference read

The target is a stylized persistent surface-film language, not realistic bubbly whitewater. The reference river contains:

- broad broken pale sheets;
- long contour-like ribbons around darker water pockets;
- medium branches and temporary connectors;
- shore/rock/log-adjacent skirts;
- peeling strips and detached fragments;
- substantial open water between Foam structures.

Small hairline strands and thin bright lines are part of the Foam language, but they are secondary detail. They must not replace the broad film/ribbon structure, and they must not drive a separate river-architecture rethink during this milestone.

## Responsibility split

### Topology

Topology answers: `where should Foam live longer or die faster?`

Topology may influence aging, validation, and diagnostics. It must not continuously paint Foam, steer Foam toward targets, fill empty water, or act as hidden material birth.

### Persistent material state

Material state answers: `what Foam currently exists?`

It stores presence, remaining life, and pattern identity. It is the thing that moves, ages, reconfigures, and stretches. Macro/meso shape change belongs here, not in topology and not only in final shader masking. Actual disappearance remains controlled by `Remaining Life`; independent `Presence` erosion must not shorten lifespan.

### Birth/source events

Birth events answer: `where did new Foam material enter the field?`

A source creates candidate material. Once merged into persistent state, source amount is discarded. Birth is not allowed to be a general-purpose final-art generator that randomizes the macro identity of the Foam.

### Rendering/presentation

Rendering answers: `how does existing material look this frame?`

The final shader owns micro breakup, crisp stylized thresholds, edge detail, and presentation polish. It may make edges crawl visually, but it must not be the only layer responsible for macro/meso shape change, and it must not conceal invalid state behavior.

## Canonical persistent material contract

Persistent Foam uses three semantic properties:

### Presence

`Presence` means material coverage in a cell. It is not remaining life, opacity budget, source amount, or topology support.

A cell can have partial presence. Transport and clipping must preserve the footprint as well as practical at the chosen simulation resolution.

### Remaining Life

`Remaining Life` is the ordinary survival clock for material and the only approved stored-state death authority. It decreases according to the local topology aging equation. It is not a visual fade value and should not be reinterpreted as source amount.

### Material Pattern

`Material Pattern` is a stable per-material identity used for deterministic breakup, edge behavior, and presentation variation. It should move with material. It is not a direct topology class.

## Source amount contract

`Amount` is a source-only spatial fill fraction over a candidate birth footprint.

Rules:

- higher Amount should represent a nested superset of lower Amount where practical;
- accepted source cells receive the configured Initial Remaining Life;
- Amount is not a life multiplier;
- Amount is not a later decay rate;
- Amount is discarded after source-to-persistent merge;
- source-to-source overlap is geometric union, not additive life inflation.

A birth patch may be partially filled by Amount, but randomness must not decide the macro identity of the source. With the same settings, repeated births should keep the same broad source type and approximate footprint; the source-fill field is keyed from source controls rather than event count. Macro/meso tearing belongs to later material evolution; renderer-side breakup should stay at micro/detail scale.

## Lifetime equation

The ordinary aging equation is:

```text
local age rate =
    Neutral baseline
    modified by positive support
    modified by negative aging pressure
```

Positive support preserves Foam by lowering the age rate. Negative aging pressure accelerates death and suppresses positive support before applying its acceleration. Full negative pressure inside a supported region must not accidentally become longer-lived than neutral water.

The currently intended full-influence authoring behavior remains approximately:

```text
Neutral Lifetime          = base seconds
Supported Aging Rate      = fraction of neutral aging speed
Negative Aging Rate       = multiplier over neutral aging speed
```

Example: Neutral `4`, Support `0.2`, Negative `4` means full support survives much longer than neutral and full negative dies much faster than neutral.

## Topology channels

Stage 6 topology is material-facing lifespan influence.

Primary classes:

- **Major Support** — broad regions where Foam can persist and form larger sheets/ribbons.
- **Connector Support** — thinner temporary support paths between major structures.
- **Negative Aging Pressure** — regions that cut holes, open dark pockets, and accelerate breakup/death.
- **Shore Support** — bank-adjacent support derived from the accepted shared shoreline/motion contract.
- **Pressure/Lee Support** — anchored support around static water-contacting obstacles.
- **Obstacle Footprint** — solid exclusion/valid-fluid mask, not a death timer.

Combined topology may overlap. Overlap is legal and must be resolved by the aging equation and valid-fluid clipping, not by popping material.

## Valid-fluid and obstacle rules

Material may exist only in valid fluid:

```text
Valid Fluid = river boundary coverage × (1 - canonical obstacle exclusion)
```

Solid obstacles are not generic negative-aging zones. They are exclusion geometry. Material crossing into obstacle footprint should be clipped or redirected by approved obstacle-flow behavior, not hidden by arbitrary fade tuning.

Obstacle clipping is a known active blocker. Do not compensate for it with opacity hacks or automatic births.

## Motion and transport rules

Persistent material movement is downstream river-space transport plus approved disturbance/obstacle motion. Rejected behaviors remain rejected:

- no shore suction;
- no generic target attraction;
- no topology steering network;
- no continuous fill controller;
- no material guidance field resurrected from old experiments;
- no hidden spread/reinforcement layer.

Material simulation now owns intrinsic macro/meso deformation. The first 5.5 pass was too conservative; 5.5b strengthened stored-state deformation so a material body can bend, stretch, and locally widen/narrow. 5.5c repairs lifecycle authority: morphing may not erase material before `Remaining Life` expires. 5.5d makes intrinsic wobble area-balanced by using opposed normalized material samples instead of a max/current union, so patches can bulge, compress, and relax without continual footprint growth. Larger river-surface coupling to waves, static pressure, lee influence, ripples, and disturbance fields is accepted as later dedicated work after intrinsic morphology is accepted. Lifecycle-safe stored fragmentation, lateral drift, and obstacle tangential sliding remain separate explicit, budgeted work.

## Manual birth phase

The current milestone is still manual proof, not final population.

Manual birth exists to prove that material can:

- be born with clear source semantics;
- move without stepping or footprint explosion;
- age according to topology;
- change shape over time;
- break up organically;
- interact with valid-fluid and obstacles;
- remain performance-bounded.

Automatic population is deferred until manually-born material satisfies those behaviors.

## Approved later source families

After the current manual proof phase, Stage 6 may add budgeted automatic births from:

- anchored shore/bank sources;
- rock/obstacle shoulder and lee sources;
- major-support film/sheet opportunities;
- connector/rim opportunities;
- sparse open-water fragments if needed.

These are later population features. They must not be added to cover up broken manual material behavior.

## Performance contract

Foam is allowed to use persistent textures and modest memory if that reduces runtime computation.

Runtime rules:

- active work is per active river/chunk;
- inactive/frozen/culled chunks should not pay birth or simulation cost;
- event pools are fixed-capacity;
- no per-event GameObjects;
- no steady-state managed allocations;
- material update rate is quality controlled;
- birth dispatch count is budgeted;
- visual complexity must not secretly multiply active C# events or dispatches.

The current rule is one budgeted event per manual birth source, with a per-material-step birth budget. Future visual complexity must not add hidden writer events or unbounded dispatches.

## Diagnostics contract

Primary workflow views:

- **Final Foam** — the exact player-facing Foam result;
- **Foam + Aging Topology** — final Foam over support/negative/obstacle topology;
- **Progressive Birth Source** — source candidate/debug view;
- **Progressive Birth Transfer** — source-to-material transfer debug view.

Diagnostics should prove state boundaries. They should not become another visual system or a permanent wall of controls.

## Non-goals for current phase

Do not add these while the active blockers remain unresolved:

- automatic anchored birth population;
- open-water population scheduling;
- new Foam source families as a substitute for fixing manual material behavior;
- topology-as-direct-painting;
- separate river architecture for thin lines;
- restored guidance fields or shore suction;
- disturbance coupling hidden inside unrelated morphology work;
- replacing intrinsic Foam morphology with water-surface coupling instead of layering it later;
- final beauty-only rendering polish;
- broad Inspector expansion.

## Failure rule

When Foam looks wrong, diagnose in this order:

1. source footprint and Amount gate;
2. source-to-source union;
3. source-to-persistent merge;
4. packed state encode/decode;
5. transport and valid-fluid clipping;
6. topology age rate;
7. CPU activity/reservation;
8. final renderer/diagnostics.

Do not fix an earlier boundary by adding later population logic, opacity tuning, or unrelated visual systems.

## Completion gate for Stage 6 manual proof

Before automatic population begins, manually-born Foam must show:

- stable source semantics;
- continuous downstream motion;
- visible temporal shape change;
- readable breakup without blur/sludge;
- believable topology aging response;
- at least initial lateral/obstacle interaction;
- no obstacle clipping that dominates the result;
- bounded runtime cost under active-chunk scenarios.
