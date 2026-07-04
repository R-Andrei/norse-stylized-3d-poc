# River Foam Problem Register and Recovery Plan

**Document type:** problem register and recovery plan  
**Created after:** Patch `4.11C.5.1a` visual testing  
**Last updated after:** Patch `4.11C.5.2d` visual testing  
**Current status:** base downstream Foam motion is now visually smooth, but the Foam material system is still not accepted. The remaining failures are now mostly shape definition, shape evolution, topology/lifetime proof, and obstacle/lateral movement.

---

# 0. Current high-level status

The Foam system has recovered from the earlier catastrophic material-transport failure, but it is not yet a convincing or reliable Foam system.

## Confirmed fixed or materially improved

- Foam no longer automatically expands into the entire river width in every normal test.
- The persistent material contract remains correct: `R = Presence`, `G = Presence × Remaining Life`, `B = Presence × Material Pattern`, `A = 0`.
- The old procedural guidance network, lane attraction, shore suction, material reinforcement, material rejuvenation, old Amount persistence, active Integrity, and active Phase systems must remain deleted.
- Base downstream movement is no longer visibly laggy/stepwise after `4.11C.5.2d`.
- Transport residue in the old sense — left-behind crumbs, stale streak fragments, or obvious trailing material remnants — appears fixed or at least no longer the main visible blocker after `4.11C.5.2d`.

## Still failing

1. Foam shape is static: once spawned, the visible body moves downstream in almost exactly the same silhouette instead of slowly changing, eroding, deforming, or breaking up.
2. Foam is visually too blurry and too smear-like.
3. Foam is still composed of obvious line/ribbon structures rather than reading as a coherent but organically broken material mass.
4. Foam does not clearly or reliably respond to positive support and negative aging zones.
5. Current diagnostics do not yet prove whether a specific visible Foam fragment is aging at the expected local rate.
6. Foam has no useful lateral drift, avoidance, or obstacle-aware sliding, so it can get stuck against rocks or obstacle boundaries.
7. Obstacle interaction is still effectively an impermeable clip, not a flow-around behavior.
8. The transport model may still be grid-visible/blocky in debug contexts, but this is no longer the leading visible problem after phase transport.

The next patches must still avoid automatic births, fracture, mature rendering polish, and new guidance systems until manually-born Foam can move, change shape, age, and interact with obstacles in a controlled way.

---

# 1. Problem A — Stepwise / laggy Foam movement

**Status:** solved by `4.11C.5.2d`.

## Original symptom

Foam did not appear to drift smoothly every frame. It appeared to move in visible bursts, as if it waited and then jumped forward.

## Why the earlier approaches failed

`4.11C.5.2` tried to solve the issue by raising material update rates and tightening transport cadence. That failed because the visible Foam body still moved only after enough tiny fractional cell transfers accumulated.

`4.11C.5.2c` lowered cadence and added render travel since the latest material step. That also failed because the render offset reset before crossing a full Foam texel, so the visible body still depended on tiny fractional conservative transport.

## Accepted fix direction

`4.11C.5.2d` changed base downstream movement authority:

- Foam has a persistent signed phase in metres.
- The shader samples Foam material at `global distance - residual phase`.
- The residual phase does not reset on material ticks.
- When phase crosses a whole material texel, `CommitPhaseTransport` shifts the packed material texture by an integer cell count.
- The committed cell distance is subtracted from the phase so visible position remains continuous.
- Base downstream velocity is removed from fractional conservative transport.
- The conservative transport path now retains only accepted disturbance-derived wake/lee/pressure material motion.

## Current result

Foam now smoothly moves downstream. This problem should remain marked fixed unless future patches regress it.

## Regression checks

- `Base Downstream Transport` should read `Phase + integer commit`.
- `Fractional Base Advection` should read `Disabled`.
- `Foam Phase / Cell` should ramp upward while Foam moves.
- `Committed Cells Last Second` should be non-zero while visible Foam is travelling.
- The visible Foam body should not return to half-second jumps.

---

# 2. Problem B — Static Foam shape / no temporal morphing

**Status:** active blocker.

## Symptom

Foam now moves smoothly downstream, but the shape itself does not meaningfully change. It behaves like a static decal or stamp being pushed forward. The silhouette, internal line pattern, and broad structure remain too similar to the spawned shape over time.

## Why this is bad

Movement alone is not enough. Foam should read as living material carried by water. It should slowly erode, wobble, deform, open small gaps, soften, tighten, and vary along its edges. Even before explicit fracture is implemented, the body should not remain a frozen stamp.

This also blocks visual validation of future systems:

- topology aging is harder to judge if the visible body is a static smear;
- obstacle interaction will look fake if the same rigid shape simply collides with rocks;
- future fracture will have no believable pre-fracture state to build from.

## Likely causes

The current system correctly preserves persistent material state, but it currently preserves it too literally for visual purposes.

Likely technical causes:

- `Presence` is transported/shifted but not shape-evolved.
- `Material Pattern` is generated at birth but not used strongly enough to create time-varying breakup.
- The shader is presenting the stored state without enough temporal edge erosion or internal animated breakup.
- No approved persistent fracture, shredding, or morphology system exists yet.
- The new phase transport correctly moves the Foam, but phase transport intentionally does not alter shape.

## Correct behavior

Foam should slowly change while preserving the idea of the original material body:

- broad body remains recognizable for a short time;
- edges subtly crawl, erode, and reform;
- internal holes or density variations can appear and fade;
- small sub-shapes can narrow or widen;
- changes should be net-downstream and water-carried, not random teleporting;
- shape evolution must not cause uncontrolled area growth;
- shape evolution must not resurrect deleted guidance/spread systems.

## Proposed patch

**Patch 4.11C.5.3 — Foam Shape Definition and Temporal Morphing**

Main work:

1. Add a controlled render-side shape evolution layer using existing material state, remaining life, material pattern, river coordinates, and time.
2. Make the visible edge threshold vary over time so edges subtly crawl instead of remaining frozen.
3. Add internal breakup variation so Foam is not a single static white stamp.
4. Keep the persistent material texture authoritative for existence and lifetime.
5. Do not introduce persistent fracture yet.
6. Do not make topology directly move Foam.
7. Do not reintroduce old procedural guidance, lane attraction, shore suction, material spread, or rejuvenation.

## Acceptance criteria

Patch `4.11C.5.3` passes this part only if:

- Foam moving through open water visibly changes shape over several seconds.
- The same Foam patch does not remain a static stamp.
- Morphing does not cause river-wide spreading.
- Morphing does not create extra material outside the transported body.
- Motion remains smooth after the change.

---

# 3. Problem C — Foam is too blurry and line/ribbon-based

**Status:** active blocker.

## Symptom

After `4.11C.5.2d`, Foam motion is smooth, but the visible result is too blurry. The material also reads as a stack of soft horizontal/linear ribbons rather than an organic Foam mass.

The old residue problem is not the same thing. The old issue was stale material left behind during transport. This new issue is that the main Foam body itself is composed of visible line-like structures and excessive blur.

## Why this is bad

Stylized Foam can be soft, but it cannot be so blurred that the shape loses definition. It also should not look like dragged paint strokes or a stack of translucent lines unless that is an intentional internal pattern.

A too-blurry Foam body makes every later test worse:

- topology aging disappearance becomes hard to see;
- obstacle clipping looks like a smear;
- lateral movement will be hard to judge;
- future fracture will not read clearly.

## Likely causes

Likely technical causes include:

- the spawned/progressive source footprint is ribbon-like;
- Foam rendering uses too much softness or filtering for the current texture resolution;
- the phase-transported state is being visually softened more than intended;
- internal material pattern contributes line structures but not coherent breakup;
- the current debug/final Foam presentation does not distinguish body core, edge softness, and internal texture strongly enough.

## Correct behavior

- Foam should have a readable core.
- Edges may be soft, but the whole body should not become a large blurred cloud.
- Internal detail should look like broken Foam structure, not repeated lines.
- Shape definition should remain readable from the isometric camera.
- Debug views may be blockier, but final Foam must not be excessively blurred.

## Proposed handling

This should be handled together with `4.11C.5.3 — Foam Shape Definition and Temporal Morphing`, because blur, line composition, and lack of morphing are visually entangled.

Main work:

1. Separate body core, edge falloff, and internal breakup in the Foam shader.
2. Reduce excessive blur/softening.
3. Preserve enough antialiasing that the Foam does not become harsh pixel blocks.
4. Make internal structure use `Material Pattern` and time-varying breakup more effectively.
5. Avoid changing transport, topology, or source Amount behavior in this patch unless a direct source-footprint bug is confirmed.

## Acceptance criteria

Patch `4.11C.5.3` passes this part only if:

- Foam is visibly less blurry.
- The main body reads as Foam rather than soft paint strokes.
- Internal line/ribbon artifacts are reduced or broken up.
- Edges remain stylized and soft without destroying shape readability.
- The change does not reintroduce transport residue.

---

# 4. Problem D — Topology interaction is not proven or not strong enough

**Status:** active blocker.

## Symptom

Foam appears to survive in some green positive-support zones, suggesting some interaction may exist. However, Foam crossing red negative-aging zones still does not reliably die faster, even when Negative Aging Rate is set very high, such as `8×`.

After phase transport, this remains unaccepted: Foam does not visibly or reliably age according to the support/negative topology seen in the debug view.

## Why this is bad

The material/topology split depends on topology modifying the Remaining Life clock in a visible, predictable way:

- positive support should preserve material;
- negative pressure should age it faster;
- positive/negative overlap should produce the documented multiplicative result.

If this is unclear, later automatic birth selection, lifecycle cleanup, and fracture work cannot be trusted.

## Possible explanations

### D1 — Foam is crossing red zones too briefly

If Foam travels quickly through a red cell and spends only a fraction of a second there, even `8×` aging may not kill it instantly. It should still lose a measurable amount of Remaining Life, but the result may not be obvious by eye.

### D2 — Debug topology and simulation topology do not align exactly

The composite debug view may show a red/green/yellow block, while the simulation samples slightly different coordinates, phase-compensated coordinates, or stale textures.

### D3 — Phase transport changed visible/storage coordinate interpretation

After `4.11C.5.2d`, visible Foam position and storage position differ by the residual phase. Any lifecycle/topology sampling must account for this. If a visible Foam fragment is shown over a red zone but the lifecycle code samples topology at its unphased storage coordinate, aging will appear wrong.

### D4 — Aging is applied but hidden by render/visibility state

The material may be losing Remaining Life numerically while the visible mask remains too soft/blurry to show the loss clearly.

### D5 — Debug color saturation may exaggerate weak topology

The debug view may show red for a broad range of nonzero negative pressure, while the actual sampled negative pressure under the Foam is not close to full strength.

## Correct behavior

The user must be able to see and verify:

- which topology value each visible Foam cell is sampling;
- what local aging multiplier is applied under that Foam;
- whether Remaining Life is decreasing at the expected rate;
- whether disappearance is due to Remaining Life reaching zero or due to visibility/transport hiding it.

## Proposed patch

**Patch 4.11C.5.4 — Topology Aging Proof and Interaction Calibration**

Main work:

1. Add or repair `Foam Life + Local Aging` diagnostics.
2. For visible Foam, display Remaining Life and sampled local aging multiplier together.
3. Confirm topology sampling uses the same world position as visible phase-transported Foam.
4. Add compact readouts:
   - visible Foam area;
   - live hidden Foam area;
   - average visible Remaining Life;
   - min/max visible Remaining Life;
   - average local aging multiplier under visible Foam;
   - average positive support under visible Foam;
   - average negative pressure under visible Foam.
5. Make full-red negative regions visibly produce high local aging multipliers.
6. If necessary, adjust debug color mapping so weak negative pressure is not visually confused with full-strength negative pressure.

## Acceptance criteria

Patch `4.11C.5.4` passes only if:

- Foam in neutral water ages at approximately `1×`.
- Foam in full green support ages according to `Supported Aging Rate`.
- Foam in full red negative pressure ages according to `Negative Aging Rate`.
- Foam in overlap ages according to the multiplicative result.
- The diagnostic makes it clear whether a red-zone crossing was too brief or genuinely ignored.
- The topology view and compute sampling agree for phase-transported Foam.

---

# 5. Problem E — Foam cannot move laterally around obstacles

**Status:** active blocker.

## Symptom

Foam can get stuck against rocks or obstacle boundaries. It does not slide around them or find a lateral path. It appears to press into the obstacle and remain there.

## Why this is bad

Real Foam carried by water should not simply stop forever at an obstacle edge. It should be able to:

- drift slightly laterally under ordinary flow;
- split around an obstacle;
- slide along the obstacle boundary;
- rejoin or continue downstream after passing the obstacle.

The previous old system overcorrected by spreading and dragging Foam everywhere. The current system overcorrects in the opposite direction: too little lateral motion.

## Correct behavior

Foam needs constrained lateral freedom:

- subtle chaotic lateral drift in ordinary water;
- obstacle-aware tangential sliding near rocks;
- no continuous river-wide spreading;
- no attraction to hidden procedural lanes;
- no source-size erasure;
- no topology support as motion.

## Proposed patch

**Patch 4.11C.5.5 — Controlled Lateral Drift and Obstacle Tangential Flow**

Main work:

1. Add a small coherent lateral drift field attached to water motion, not topology support.
2. Add obstacle-gradient-aware tangential steering near obstacle boundaries.
3. When downstream motion is blocked by an obstacle, redirect some velocity along the obstacle tangent instead of zeroing it.
4. Keep lateral drift bounded and non-expansive.
5. Expose one meaningful control only if needed, for example `Foam Lateral Drift`, default low.
6. Keep obstacle movement deterministic and stable, not frame-random.

## Important distinction

This patch must not restore deleted behavior:

- no procedural lane attraction;
- no shore suction;
- no topology support as velocity;
- no global spread/evolution field;
- no reinforcement/rejuvenation;
- no autonomous network.

The lateral field exists only to let already-born material move plausibly inside the river and around obstacles.

## Acceptance criteria

Patch `4.11C.5.5` passes only if:

- Foam near an obstacle can slide around it instead of sticking indefinitely.
- Foam can show slight lateral wobble in open water.
- A tiny patch remains a tiny patch, not a river-wide sheet.
- Increasing lateral drift increases wobble/sliding, not uncontrolled spreading.
- Lateral drift does not create material or rejuvenate it.

---

# 6. Problem F — Obstacle interaction is still an impermeable clip

**Status:** active blocker, owned by `4.11C.5.5`.

## Symptom

Obstacle interaction currently behaves like invalid material clipping. Foam that reaches a rock or blocked cell is stopped/removed/hidden rather than redirected around the obstacle.

## Why this is bad

An obstacle field and a flow field are different things:

- obstacle footprint says where Foam cannot exist;
- obstacle wake/pressure says how water motion changes;
- positive support says how Foam lifetime changes;
- negative aging says how Foam lifetime decays faster.

Those concepts must stay separate.

## Proposed handling

Handle this together with `4.11C.5.5`:

- use obstacle gradients to find tangential direction;
- route blocked downstream movement sideways along the obstacle edge;
- avoid making obstacle support zones into motion zones;
- keep clips only for truly invalid cells inside the obstacle.

---

# 7. Problem G — Diagnostics do not prove local aging

**Status:** active blocker, owned by `4.11C.5.4`.

## Symptom

Current diagnostics still do not prove whether one visible Foam fragment is aging at the exact local rate expected from the topology under it.

## Why this matters

Without this proof, visual reports remain ambiguous. If Foam survives over a red area, the system must show whether:

- the visible Foam did not actually sample that red cell;
- the red cell was visually saturated but numerically weak;
- the Foam crossed too quickly to visibly die;
- positive support counteracted negative aging;
- Remaining Life decreased but rendering hid the change;
- aging was genuinely ignored.

## Correct behavior

The Inspector should make a local aging mismatch obvious without requiring guesswork.

## Proposed handling

This belongs in `4.11C.5.4`, not in rendering polish or obstacle flow.

---

# 8. Watch item H — Debug topology/grid visibility

**Status:** lower priority / watch item.

## Symptom

Green/red/yellow topology regions can be displayed as large blocky cells. Foam can be finer and shader-smoothed, so interaction boundaries can be hard to judge.

## Current assessment

After `4.11C.5.2d`, the main visual blockers are no longer movement stepping or stale transport residue. Debug blockiness may still matter for proof workflows, but it is not currently the leading gameplay/rendering defect.

## Proposed handling

Do not spend a separate patch polishing topology visuals yet. Handle the important part through numeric aging proof in `4.11C.5.4`.

Possible later work:

- sampled-grid overlay;
- clearer full-strength versus weak topology colors;
- optional cell-boundary visualization in advanced diagnostics.

---

# 9. Recommended repair order

## 4.11C.5.3 — Foam Shape Definition and Temporal Morphing

Reason: movement is now smooth, but the Foam itself is a blurry static stamp made of line/ribbon structures. This should be fixed before judging mature rendering, fracture, or obstacle flow. It also makes topology/lifetime tests easier to read.

Owned problems:

- static shape / no morphing;
- excessive blur;
- line/ribbon composition of the main Foam body.

## 4.11C.5.4 — Topology Aging Proof and Interaction Calibration

Reason: support and negative pressure still do not visibly or provably affect Foam at the expected local rate.

Owned problems:

- topology response;
- local aging proof;
- phase-compensated sampling verification;
- diagnostic proof under visible Foam.

## 4.11C.5.5 — Controlled Lateral Drift and Obstacle Tangential Flow

Reason: obstacle avoidance requires deliberate lateral motion and flow-around behavior. This should be added only after the Foam body is readable and topology/lifetime proof is trustworthy enough.

Owned problems:

- lack of lateral movement;
- obstacle sticking;
- impermeable clip behavior.

Only after these pass should the roadmap continue to lifecycle cleanup, automatic birth systems, fracture readiness, and mature Foam rendering.

---

# 10. Current blocked work

The following remain blocked:

- automatic anchored births;
- open-water births;
- birth population balancing;
- persistent cracks;
- fracture/shredding;
- mature Foam rendering;
- final reference matching.

None of those should be implemented until manually-born Foam can:

1. move smoothly;
2. remain visually defined rather than blurry/line-based;
3. change shape over time;
4. age according to local topology;
5. move around obstacles without uncontrolled spreading.

---

# 11. Non-negotiable rules going forward

1. Do not restore deleted procedural network guidance.
2. Do not restore lane attraction.
3. Do not restore shore suction.
4. Do not restore generic material spread/evolution steering.
5. Do not restore material reinforcement or rejuvenation from disturbances.
6. Do not make topology support directly move Foam.
7. Do not make negative pressure directly erase Foam.
8. Do not hide live material in diagnostics without showing that it is live-but-hidden.
9. Do not introduce lateral motion that causes uncontrolled area growth.
10. Ask before adding any new persistent state channel, topology class, or authoring control.
11. Do not add new Foam documents unless explicitly requested; update this problem register instead.

---

# 12. Patch history notes

## Patch 4.11C.5.2 — Transport Temporal Continuity

**Status:** failed visual validation.

Implemented higher update rates and diagnostics, but user testing reported no visible improvement in laggy Foam travel.

## Patch 4.11C.5.2b — Foam Debug Layer Reorganization

**Status:** accepted as usability improvement.

Replaced the old flat Foam diagnostics dump with foldout-based sections:

1. Foam Validation Overview
2. Foam View Modes
3. Transport / Motion
4. Material Lifetime
5. Topology Interaction
6. Birth / Source Debugging
7. Shape Conservation / Residue
8. Runtime State and Resources
9. Advanced Internal Diagnostics

## Patch 4.11C.5.2c — Foam Material Cadence and Render-Space Travel

**Status:** failed visual validation.

Lowering cadence was correct, but tying render travel to time since the latest material step was not enough. The shader offset reset before crossing a whole Foam texel, so visible Foam still depended on tiny fractional material advection.

## Patch 4.11C.5.2d — Foam Phase Transport and Integer Commit

**Status:** solved base downstream movement; exposed next blockers.

Implemented phase-driven movement plus integer cell commits. User validation confirmed Foam now moves smoothly downstream. However, testing also confirmed the remaining issues:

- the Foam shape is static;
- the visible body is too blurry;
- the Foam is still composed of line/ribbon structures;
- topology aging response remains unproven or ineffective;
- Foam still lacks lateral/obstacle flow-around behavior.
