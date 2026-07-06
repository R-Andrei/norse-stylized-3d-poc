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
- the 5.5 material-evolution pass adds the first persistent `Presence` morphing layer; 5.5c restores lifecycle authority after the initial erosion attempt shortened Foam life incorrectly; 5.5d changes morphing to area-balanced intrinsic wobble so deformation does not continually grow the footprint;
- 5.5d intrinsic runtime morphology is accepted as good enough for now;
- 5.6 adds render-only surface coupling so Final Foam responds to existing macro waves, static pressure, lee/depression, ripples, disturbance gradients, and wake energy without changing stored material state;
- 5.6b adds Foam interior clarity filtering so fine water-surface variation does not make the body noisy, while strong surface features can still imprint at reduced strength;
- 5.7 adds stored-state surface-driven material morphing: existing ripple, wake, static pressure, and lee fields can amplify or bias persistent `Material Presence` morphology, without changing birth or lifecycle authority;
- 5.7b adds a `Surface Morph Strength` calibration control so the stored-state response can be A/B tested at `0` and strengthened without changing lifecycle authority;
- 5.7c rebalances the internal surface-response formula so `1` is a normal readable authored effect, `2` is strong, and `3+` becomes stress-test territory instead of merely compensating for an undertuned curve;
- 5.8/5.8b/5.8c/5.8d proved that lateral stored-material movement must have macro body authority, but the local chaotic drift resolver is superseded by the explicit 5.9 field architecture;
- 5.9 adds the Unified Foam Motion Field as the single active lateral macro movement authority: a dense scrolling lane field plus a fixed obstacle-routing override field. The field is lateral-only, debug-visible, and performance-bounded to texture loads in `SimulateFoam`.

Still not accepted:

- obstacle routing has an initial field-based proof and still needs visual calibration after validation;
- source shapes still need improvement before final reference-matching;
- topology interaction needs final proof and calibration;
- final visual fragmentation / organic breakup is deferred until movement and source-shape foundations are stronger.

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

The final shader owns micro breakup, crisp stylized thresholds, edge detail, presentation polish, Foam interior clarity, and render-only coupling to the already-evaluated river surface. It may bend, thin, stretch, or edge-modulate the Final Foam mask using macro waves, static pressure, lee/depression, ripples, disturbance gradients, and wake energy. It should suppress ordinary high-frequency water variation inside solid Foam so the material remains clean and white, while allowing strong surface features to show through at reduced strength. It must not change `Material Presence`, Remaining Life, birth/population, or lifecycle authority, and it must not conceal invalid state behavior.

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

Persistent material movement is downstream river-space transport plus approved disturbance and explicit motion-field inputs. Rejected behaviors remain rejected:

- no shore suction;
- no generic target attraction;
- no topology steering network;
- no continuous fill controller;
- no hidden spread/reinforcement layer;
- no obstacle back-pressure, upstream compression, or radial repulsion masquerading as routing.

Material simulation now owns intrinsic macro/meso deformation. The first 5.5 pass was too conservative; 5.5b strengthened stored-state deformation so a material body can bend, stretch, and locally widen/narrow. 5.5c repairs lifecycle authority: morphing may not erase material before `Remaining Life` expires. 5.5d makes intrinsic wobble area-balanced by using opposed normalized material samples instead of a max/current union, so patches can bulge, compress, and relax without continual footprint growth. 5.6/5.6b add river-surface coupling at render time only: Final Foam samples the same water-surface influences used by the river shader, and Foam interiors are clarity-filtered so fine water detail does not dominate the white body. 5.7 keeps that render path separate while also sampling the same ripple, wake, static pressure, and lee fields inside the persistent material simulation. Those fields only amplify/bias the existing area-balanced morphology; they do not spawn Foam, shorten `Remaining Life`, or become topology steering. 5.7b keeps the same boundary but adds an explicit `Surface Morph Strength` control: `0` disables stored-state surface response for A/B testing. 5.7c recalibrates the formula itself rather than hiding a larger multiplier behind the control: `1` is now intended to be the normal readable authored response, `2` should read strong, and `3+` is overdrive/stress-test behavior.

5.8/5.8b/5.8c/5.8d are historical calibration work that proved macro body-scale lateral movement is necessary, but their local chaotic drift resolver is no longer the active macro authority. 5.9 replaces that path with the Unified Foam Motion Field. The field answers only lateral macro movement: downstream travel still belongs to phase transport, `Remaining Life` still owns death, topology still owns support/negative aging, birth still creates source material, and final rendering still owns presentation.

The 5.9 field has two explicit inputs. The dense lane field stores a signed lateral suggestion across the full river and scrolls by sample-coordinate phase; time does not rebuild the texture. The lane texture is generated as a layered/domain-warped fractal field so it reads as granular chaotic structure rather than a few broad ribbons. The obstacle-routing field is fixed in river space, generated only from dirty obstacle/domain data, and overrides the lane field near obstacles through an influence weight. Obstacle routing is lateral-only, groups obstacle cells into connected bodies, and includes a weaker upstream approach region before stronger near-obstacle override. `SimulateFoam` samples these fields as texture loads only; it does not run procedural lane noise or local obstacle search in the hot path.

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
- replacing intrinsic Foam morphology with water-surface coupling rather than layering them;
- letting ordinary granular water-surface variation dirty the Foam interior;
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

## 5.9e obstacle routing refinement

Obstacle routing must be interpreted as collision prevention, not as a generic proximity force. Close beside an object is not sufficient reason for strong redirection; the strongest influence is reserved for material that would otherwise hit the obstacle. The fixed obstacle-routing texture therefore uses a flow-relative collision-risk envelope: weak upstream approach, high direct-front override, minimal side influence, and no downstream tail after the obstacle is cleared. This preserves the Stage 6 ownership split and keeps runtime simulation cost unchanged.

## 5.9f collision-shadow obstacle routing correction

Obstacle routing must be based on likely collision, not closeness. The corrected obstacle-routing texture now treats the component bounds only as a cheap iteration window; the written influence is constrained to a flow-relative collision shadow. Far upstream influence remains weak, direct-front cells aligned with the obstacle footprint can reach full override, side-passing cells are capped to very low influence, and downstream influence is cut entirely once the obstacle is cleared. This keeps `SimulateFoam` at the same runtime cost: two lane loads plus one fixed obstacle-routing load.

## 5.9g obstacle shadow ramp correction

The final valid cells immediately before the obstacle exclusion zone should be the strongest part of the collision shadow. 5.9g therefore adds a direct-front contact band, removes the remaining downstream release tail, and changes the upstream approach ramp from an overly assertive near-linear feel to a slower eased ramp that only becomes strong near actual collision risk. The obstacle texture is still dirty-time data only; runtime simulation cost is unchanged.

## 4.11C.5.9h motion-field shape calibration

The unified Foam Motion Field remains a two-field system: a dense scrolling lane field plus a fixed dirty-time obstacle-routing field. 5.9h does not alter runtime ownership or sample count. It refines the generated field content so the lane field avoids large same-direction continents and the obstacle field behaves as a one-sided collision shadow. Obstacle components now track ids during flood fill so row-specific leading edges can be used when shaping the shadow. This keeps strongest obstacle influence at the final valid upstream cells before collision, while immediately releasing cells at or past the row-specific obstacle boundary.

## 4.11C.5.9i obstacle front-contact closure

5.9i is a final dirty-time shape correction for the obstacle-routing texture. It does not alter the two-field architecture, lane scrolling, runtime sampling, lifecycle ownership, topology ownership, or final rendering ownership. The obstacle collision shadow keeps the 5.9h row-specific leading-edge model, but its front-contact band is allowed to extend one to two cells into the obstacle-facing boundary so the strongest routing region visually and functionally touches the obstacle/negative topology zone instead of stopping short. The extension is constrained by the collision corridor so side-passing material is still not redirected merely because it is close to the object. Low-value routing influences below the artifact threshold are discarded to remove tiny stray strips outside the main shadow. Runtime remains two lane loads plus one obstacle-routing load.

## 4.11C.5.9j motion-field material advection correction

5.9j preserves the Unified Foam Motion Field architecture but corrects how it drives stored material. Motion-field sampling now uses the same visible/world phase-shifted coordinate already used for topology, boundary, and obstacle exclusion sampling. This keeps the debug field, obstacle routing, and actual material motion aligned after phase transport. The material morph pass now treats the field-driven macro offset as source-sampled advection: the base transport sample comes from the field-advected source position, not a blend of the current cell and the source cell. This avoids the previous stretch/growth behavior where old material was preserved while new material appeared downstream/laterally. Remaining Life still owns death; this is a relocation semantics correction, not a lifecycle erosion rule. Neutral lane regions are stored as low signed movement instead of exact zero so Foam is not hard-stopped by black/neutral patches in the absence of a dedicated lateral momentum field.
