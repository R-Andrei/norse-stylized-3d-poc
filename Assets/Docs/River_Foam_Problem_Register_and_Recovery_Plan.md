# River Foam Problem Register and Recovery Plan

**Document type:** problem register and recovery plan  
**Created after:** Patch `4.11C.5.1a` visual testing  
**Current status:** the catastrophic river-wide spreading failure is reduced, and lifetime can visibly work in some supported cases, but the system is not accepted. The next work must address the remaining failures one by one rather than continuing to automatic births, fracture, or rendering polish.

---

# 0. Current high-level status

The current Foam system has improved from the earlier complete failure mode, but it is still not a reliable material system.

What appears to be fixed or partially fixed:

- Foam no longer automatically expands into the entire river width in every test.
- Long lifetime settings can visibly preserve Foam in some supported areas.
- The `Presence / Presence × Remaining Life / Presence × Material Pattern / 0` state contract is still the correct state direction.
- The old procedural guidance network, lane attraction, shore suction, and reinforcement/rejuvenation systems must remain deleted.
- The new combined `Foam + Aging Topology` view is useful enough to expose interactions, but not yet sufficient to prove them quantitatively.

What is still failing:

1. Foam travels in a visibly laggy / stepwise way.
2. Foam leaves behind visible lines, crumbs, or streak fragments during transport.
3. Foam does not clearly or reliably respond to positive support and negative aging zones.
4. Foam has no useful lateral avoidance or chaotic drift, so it can get stuck against rocks or obstacles.
5. Current diagnostics do not yet prove whether a specific visible Foam fragment is aging at the expected local rate.
6. Obstacle interaction is currently an impermeable clip, not a flow-around behavior.
7. The transport model is still too grid-visible and blocky in debug contexts.

The next patches must not add automatic births, fracture, shredding, or mature rendering. They must repair the material transport and lifecycle proof chain first.

---

# 1. Problem A — Stepwise / laggy Foam movement

## Symptom

Foam does not appear to drift smoothly every frame. It appears to move in visible bursts, as if it waits and then jumps forward.

## Why this is bad

Foam is supposed to be water-carried material. Even stylized Foam should glide continuously enough that the player reads it as being transported by the river.

Stepwise motion also corrupts validation:

- a patch can appear to sample one topology zone for too long;
- then suddenly jump across another zone;
- aging response becomes hard to judge;
- high Material Flow Speed may become visually choppier instead of simply faster.

## Likely causes

The current conservative transport runs on fixed material simulation steps. If renderer interpolation between previous/current material states is not correct, or if the fixed update cadence is too low for the selected flow speed, the material will visibly jump.

Likely technical boundaries to inspect:

- `ResolveUpdateRate()` and actual material update cadence;
- transport substep count versus displayed frame rate;
- previous/current state texture swapping;
- material interpolation factor passed to the water shader;
- whether fresh writes to both ping-pong textures bypass interpolation;
- whether interface compression happens only after large transport moves, creating visible pulses;
- whether `FoamMaterialFlowSpeedMultiplier` increases displacement without increasing effective temporal smoothness.

## Correct behavior

- Foam should move visually every rendered frame through interpolation or sufficiently frequent substeps.
- `Material Flow Speed = 1` should match normal current movement.
- Increasing Material Flow Speed should increase speed without introducing larger jumps.
- Compression should not be visible as a discrete periodic pulse.

## Proposed patch

**Patch 4.11C.5.2 — Transport Temporal Continuity**

Main work:

1. Audit and repair previous/current material-state interpolation.
2. Add explicit runtime diagnostics for material update cadence, interpolation alpha, and transport displacement per update.
3. If interpolation is broken, fix interpolation first.
4. If interpolation is correct but cadence is too low, add bounded internal substeps or increase update rate based on visible displacement.
5. Ensure Material Flow Speed scales the stability/cadence calculation and not only the displacement.
6. Verify compression is not visibly pulsing.

## Acceptance criteria

Patch 4.11C.5.2 passes only if:

- Foam movement is visually continuous at `Material Flow Speed = 1`.
- Foam movement remains visually continuous at faster speeds such as `2`, `3`, and `4`.
- The patch does not restore any deleted guidance, spread, lane, shore suction, or reinforcement system.
- Foam does not become river-wide again.
- Inspector diagnostics make the material update cadence and interpolation state visible enough to debug future reports.

---

# 2. Problem B — Transport residue, left-behind lines, and shape loss

## Symptom

Foam leaves behind visible thin lines, crumbs, or streak fragments after the main body has moved downstream. Some of these fragments appear as stale source remnants or detached grid artifacts.

## Why this is bad

A trailing soft edge is acceptable. Persistent isolated line fragments are not.

Foam may later fracture and shred, but that has not been implemented yet. Current fragments are not approved fracture behavior; they are transport artifacts.

## Likely causes

The current finite-volume transport and compression preserve material quantity better than the older advection system, but still produce low-coverage fragments at grid interfaces.

Likely technical causes:

- partially transported Presence remains in cells behind the main core;
- compression is not sufficiently connectivity-aware;
- final rendering still displays isolated low-support fragments;
- source writes and transport writes may leave tiny valid remnants that are not connected to a meaningful body;
- obstacle/bank clipping can create partial-cell remnants along boundaries.

## Correct behavior

- Main Foam material should remain coherent during simple transport.
- A small trailing fade is acceptable.
- Isolated crumbs should not remain visibly alive unless a future fracture system explicitly creates them.
- Residue cleanup must not simply delete all low Presence edges, because that would destroy anti-aliased Foam boundaries.

## Proposed patch

**Patch 4.11C.5.3 — Residue Suppression and Shape Conservation**

Main work:

1. Add a connected-neighborhood residue classifier.
2. Distinguish legitimate soft edges from isolated crumbs.
3. Prefer render-only suppression first for suspect crumbs.
4. Where safe, conservatively reabsorb low-coverage isolated material into nearby stronger downstream/core cells.
5. Track integrated Presence area and visible core area before/after cleanup.
6. Avoid broad state deletion unless a fragment is clearly below a tiny threshold and disconnected.

## Acceptance criteria

Patch 4.11C.5.3 passes only if:

- A small patch can travel without leaving obvious stuck streak lines behind.
- Main Foam edges remain soft enough and are not harshly chopped.
- Integrated Presence does not grow.
- Visible core area does not collapse prematurely.
- No fake fracture/shredding behavior is introduced.

---

# 3. Problem C — Topology interaction is not proven or not strong enough

## Symptom

Foam appears to survive in green positive-support zones, suggesting some interaction exists. However, Foam crossing red negative-aging zones does not reliably die faster even when Negative Aging Rate is set very high, such as `8×`.

## Why this is bad

The whole material/topology split depends on topology modifying the Remaining Life clock in a visible, predictable way:

- positive support should preserve material;
- negative pressure should age it faster;
- positive/negative overlap should produce the documented multiplicative result.

If this is unclear, later automatic birth selection and fracture work cannot be trusted.

## Possible explanations

### C1 — Foam is crossing red zones too briefly

If the Foam travels quickly through a red cell and spends only a fraction of a second there, even `8×` aging may not kill it instantly. It should still lose a measurable amount of Remaining Life, but the result may not be obvious by eye.

### C2 — Debug topology and simulation topology do not align exactly

The visual composite may show a red/green/yellow block, but the simulation may sample slightly different coordinates or stale textures.

### C3 — Aging is applied to transported state before/after source/visibility in a way that makes visual interpretation misleading

If a visible piece is partly hidden or fragmented, the user may see the rendered portion rather than the whole live material field.

### C4 — Negative Aging Pressure values are visually saturated but numerically weak in the sampled location

The debug view may show red for a broad range of nonzero negative pressure, while actual numeric pressure under the Foam is not close to full strength.

## Correct behavior

The user must be able to see and verify:

- which topology value each visible Foam cell is sampling;
- what local aging multiplier is applied under that Foam;
- whether Remaining Life is decreasing at the expected rate;
- whether disappearance is due to Remaining Life reaching zero or due to visibility/transport hiding it.

## Proposed patch

**Patch 4.11C.5.4 — Topology Aging Proof and Interaction Calibration**

Main work:

1. Add a `Foam Life + Local Aging` diagnostic.
2. For visible Foam, display Remaining Life and sampled local aging multiplier together.
3. Add readouts:
   - visible Foam area;
   - live hidden Foam area;
   - average visible Remaining Life;
   - min/max visible Remaining Life;
   - average local aging multiplier under visible Foam;
   - average positive support under visible Foam;
   - average negative pressure under visible Foam.
4. Confirm compute sampling coordinates match composite debug visualization.
5. Make full-red negative regions visibly produce high local aging multipliers.
6. If necessary, adjust debug color mapping so weak negative pressure is not visually confused with full-strength negative pressure.

## Acceptance criteria

Patch 4.11C.5.4 passes only if:

- Foam in neutral water ages at approximately `1×`.
- Foam in full green support ages according to `Supported Aging Rate`.
- Foam in full red negative pressure ages according to `Negative Aging Rate`.
- Foam in overlap ages according to the multiplicative result.
- The diagnostic makes it clear whether a red-zone crossing was too brief or genuinely ignored.
- The topology view and compute sampling agree.

---

# 4. Problem D — Foam cannot move laterally around obstacles

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
- no source-size erasure.

## Proposed patch

**Patch 4.11C.5.5 — Controlled Lateral Drift and Obstacle Tangential Flow**

Main work:

1. Add a very small stochastic/coherent lateral drift field attached to water motion, not to topology support.
2. Add obstacle-gradient-aware tangential steering near obstacle boundaries.
3. When downstream motion is blocked by an obstacle, redirect some velocity along the obstacle tangent instead of zeroing it.
4. Keep lateral drift bounded and non-expansive.
5. Expose one meaningful control if needed:
   - `Foam Lateral Drift`, default low.
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

Patch 4.11C.5.5 passes only if:

- Foam near an obstacle can slide around it instead of sticking indefinitely.
- Foam can show slight lateral wobble in open water.
- A tiny patch remains a tiny patch, not a river-wide sheet.
- Increasing lateral drift increases wobble/sliding, not uncontrolled spreading.
- Lateral drift does not create material or rejuvenate it.

---

# 5. Additional problem E — Obstacle and support semantics are visually tangled

## Symptom

Green support zones appear near/around rocks, and Foam may survive there while also being physically blocked by the rock.

## Why this is confusing

A support zone is an aging influence. An obstacle footprint is a geometric invalidation/impermeability field. A wake or lee field may influence motion. These must remain separate.

Current screenshots make it hard to tell whether Foam is:

- alive because of support;
- stuck because of obstacle clipping;
- hidden by geometry;
- unable to leave because lateral velocity is missing.

## Proposed handling

Do not make this a separate implementation patch yet. It should be addressed through diagnostics in 4.11C.5.4 and obstacle motion in 4.11C.5.5.

If confusion remains afterward, add a later view that separates:

- obstacle footprint;
- obstacle wake/pressure motion;
- positive aging support;
- negative aging pressure.

---

# 6. Additional problem F — Debug topology is too blocky to judge fine interactions

## Symptom

Green/red/yellow topology regions are displayed as large blocky cells. Foam can be much finer and visually interpolated, so the interaction boundary is difficult to judge.

## Why this matters

If a Foam streak visually appears to cross a red zone, but the red zone is represented by a coarse cell or interpolated differently between shader and compute, the user cannot tell whether it should have aged quickly.

## Proposed handling

This should be part of 4.11C.5.4:

- add numeric sampled-aging readouts under visible Foam;
- make full-strength zones distinguishable from weak zones;
- optionally add cell-boundary or sampled-grid visualization only in advanced diagnostics.

Do not spend time polishing topology visuals before the numeric aging proof works.

---

# 7. Recommended repair order

The issues should be fixed in this order:

## 4.11C.5.2 — Transport Temporal Continuity

Reason: laggy movement makes every other visual test unreliable.

## 4.11C.5.3 — Residue Suppression and Shape Conservation

Reason: left-behind fragments make it impossible to know which material is real and which is a grid artifact.

## 4.11C.5.4 — Topology Aging Proof and Interaction Calibration

Reason: support and negative pressure can only be judged once movement and residue are stable enough.

## 4.11C.5.5 — Controlled Lateral Drift and Obstacle Tangential Flow

Reason: obstacle avoidance requires deliberate lateral motion, but lateral motion should be added only after basic transport/residue/lifetime proof is trustworthy.

Only after these pass should the roadmap continue to C.6 final lifecycle cleanup and later automatic birth systems.

---

# 8. Current blocked work

The following remain blocked:

- automatic anchored births;
- open-water births;
- birth population balancing;
- fracture readiness;
- persistent cracks;
- edge shredding;
- mature Foam rendering;
- final reference matching.

None of those should be implemented until the existing manually-born Foam can:

1. move smoothly;
2. keep its shape without stale residue;
3. age according to local topology;
4. move around obstacles without uncontrolled spreading.

---

# 9. Non-negotiable rules going forward

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

---

# 10. Immediate next patch recommendation

The immediate next patch should be:

**Patch 4.11C.5.2 — Transport Temporal Continuity**

It should not try to fix every issue at once. Its job is to make Foam motion visually continuous and add enough transport diagnostics to prevent future guessing.

After that, handle residue, topology proof, and lateral obstacle motion as separate patches.

---

# Patch 4.11C.5.2 Update — Problem 1: Transport Temporal Continuity

**Status:** implemented; Unity validation pending.

Problem 1 is the visible stepwise/laggy motion of persistent Foam. C.5.2 does not attempt to solve residue, topology calibration, red/green interaction strength, obstacle sliding, or lateral drift. It makes the transport clock and renderer interpolation testable first.

Implemented changes:

- Low/Medium/High material temporal minima are now 45/60/90 Hz.
- The stability resolver still raises update rate further when river Flow Speed and Material Flow Speed require it.
- The internal maximum transport Courant target is tightened to reduce per-step grid displacement.
- Inspector diagnostics now report material step duration, material steps last frame, render interpolation alpha, estimated cells per step, transport substeps used, and compression passes last frame.
- C.5 deletions remain permanent: procedural guidance, lane attraction, shore suction, generic spread/evolution steering, topology-as-motion, material reinforcement, material rejuvenation, and old MacCormack transport are still removed.

Validation target:

- Foam moves smoothly at Material Flow Speed 1.
- Foam remains smooth at Material Flow Speed 2–4.
- Render Interpolation Alpha changes between steps.
- Material Steps Last Frame does not repeatedly spike in visible bursts.
- Estimated Cells / Step stays below the transport target.


---

# Patch 4.11C.5.2 Result — Transport Temporal Continuity Attempt

**Status:** failed visual validation.

C.5.2 added higher material update rates and exposed timing counters, but user testing reported no visible improvement in laggy Foam travel. The failure was compounded by the Inspector layout: the requested transport counters were not quickly visible and the old topology/runtime metrics remained effectively unusable for live validation.

Conclusion:

- The immediate blocker is no longer another blind transport tweak.
- The debug layer must be reorganized before further transport fixes.
- Transport timing, interpolation, cells-per-step, substeps, and compression counters must be readable near the top of the Foam Inspector while Foam is still alive.

---

# Patch 4.11C.5.2b — Foam Debug Layer Reorganization

**Status:** implemented; Unity validation pending.

Purpose:

Replace the old flat Foam diagnostics dump with foldout-based sections. Every section begins with an explanation of what it is for, when to use it, what a healthy result means, and which later patch owns failures found there.

Implemented Inspector sections:

1. Foam Validation Overview
2. Foam View Modes
3. Transport / Motion
4. Material Lifetime
5. Topology Interaction
6. Birth / Source Debugging
7. Shape Conservation / Residue
8. Runtime State and Resources
9. Advanced Internal Diagnostics

Normal validation now starts with Overview, View Modes, and Transport / Motion instead of the giant topology metric dump. Raw/advanced/internal material-state views remain out of the normal workflow. Some exact under-Foam lifetime/topology values are explicitly marked as pending for 4.11C.5.4 rather than being hidden or implied.

The next transport fix should use the Transport / Motion section values:

- Material Step Duration
- Material Steps Last Frame
- Render Interpolation Alpha
- Estimated Cells / Step
- Transport Substeps Used
- Compression Passes Last Frame
- Material Flow Speed


---

# Patch 4.11C.5.2c — Foam Material Cadence and Render-Space Travel

**Status:** implemented; Unity visual validation pending.

C.5.2 failed because raising the material update rate to 45/60/90 Hz did not make Foam visibly smoother. The decisive diagnostic was that Foam was moving only about `0.035` cells per material step at 60 Hz, which meant the visible body could still appear to move only after roughly half a second of fractional cell accumulation.

C.5.2c changes the direction of the fix:

- Foam material simulation quality baselines are now Low `8 Hz`, Medium `12 Hz`, and High `16 Hz`.
- The existing conservative transport stability resolver remains active and may raise cadence only when authored speed would exceed the Courant target.
- Normal Foam rendering now uses the latest committed material state directly instead of relying on previous/current alpha for motion.
- The runtime binds `_FoamRenderTravelMetres`, a signed downstream offset equal to Foam speed times time since the latest material commit.
- The shader applies that offset only to Foam material sampling, not to topology/source/obstacle debug-map sampling.
- No extra render-history texture, texture copy, or additional compute pass was added.

Updated validation target:

- Material Update Rate should usually read near `8 / 12 / 16 Hz` depending on quality, unless stability raises it for fast flow.
- Render State Blend should read `1.000` during normal Foam rendering.
- Render Travel Since Step should change between material commits while live Foam is moving.
- Final Foam and Foam + Aging Topology should show the Foam overlay moving downstream smoothly rather than waiting for half-second cell-boundary jumps.
- Residue/streaks, topology aging calibration, and lateral/obstacle flow remain separate later fixes.


---

# Patch 4.11C.5.2d — Foam Phase Transport and Integer Commit

**Status:** implemented; Unity visual validation pending.

C.5.2c was visually rejected. Lowering the material cadence was correct, but tying render travel to time since the latest material step was not enough. The shader offset reset before crossing a whole foam texel, so the visible Foam still depended on tiny fractional material advection and continued to stutter.

C.5.2d changes the movement authority:

- Base downstream Foam travel is now owned by a persistent signed phase in metres.
- The shader samples Foam material at `global distance - residual phase`.
- The residual phase no longer resets on material ticks.
- When the phase crosses a whole material texel, a new `CommitPhaseTransport` compute kernel shifts the packed material texture by an integer cell count.
- The committed cell distance is subtracted from the phase, preserving continuous visual position.
- Base downstream velocity is removed from fractional conservative transport.
- The existing transport predictor/corrector path now retains only accepted disturbance-derived wake/lee/pressure material motion.
- Material cadence remains Low `8 Hz`, Medium `12 Hz`, High `16 Hz`, and base flow speed no longer raises cadence through the old Courant safety override.

Updated Transport / Motion validation:

- `Foam Phase / Cell` should ramp upward while Foam moves.
- `Committed Cells Last Frame` should increment when the residual crosses a cell.
- `Committed Cells Last Second` should be non-zero while visible Foam is travelling.
- `Base Downstream Transport` should read `Phase + integer commit`.
- `Fractional Base Advection` should read `Disabled`.
- Movement should be judged in Final Foam and Foam + Aging Topology before residue/topology/lateral problems are addressed.

Still not part of this patch:

- Residue/streak cleanup.
- Topology aging proof/calibration.
- Negative aging strength.
- Obstacle sliding.
- Lateral drift.
- Fracture/splitting.
