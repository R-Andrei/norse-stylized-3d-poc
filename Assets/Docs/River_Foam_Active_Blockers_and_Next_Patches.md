# River Foam Current Status and Deferred Work

## Current status

Stage 6 Chipping/Strands and the `5.18E` lifecycle correction are accepted. The original `5.18F` detaching-source attempt is rejected because legal short/wide object events could remain on its full-history fallback. The active replacement is `4.11C.5.18F.1`.

Accepted production sequence:

```text
coherent Foam
→ analytical Chipping
→ structural Strands
→ composition
```

Accepted Chipping baseline:

```text
D.0   dedicated Fray/fine-edge system retired;
D.1A  canonical zero-resource Chip eligibility accepted;
D.1A.1 persistent-Presence carrier rejected and rolled back;
D.1B  six-control Chipping authoring refactor accepted;
D.1C  camera-readable Chip population accepted.
```

Current Chipping controls:

```text
Chip Amount
Chip Size
Chip Spacing
Chip Irregularity
Chip Edge Width
Chip Interior Access
```

The current edge territory is accepted as good enough. `D.1D — Coherent Edge-Bite Admission` is skipped. The zoom-dependent over-capture around unresolved thin visual strips remains known technical debt and is not an active blocker.

Remaining-Life modulation of Chipping or Strands is optional future work only. It is not required for current completion and is not queued.

## Detaching Object Arc deposition replacement — `4.11C.5.18F.1` — implemented, Unity validation pending

The proven cause remains complete historical repainting by Contact Arc and Semi-Arc. The rejected `5.18F` attempted to gate that history with `frontierActive`, but the gate could remain false for the whole event when initial profile reach plus Head Trail exceeded the available reveal span.

`5.18F.1` removes that fallback completely:

```text
Contact Arc       finite centre pulse → two continuously moving frontiers;
Contact Semi-Arc  finite shoulder pulse → one continuously moving frontier;
short event       finite pulse, never a held full-history source;
startup timing    normalized material-step duration supplied by the CPU;
frontier depth    HeadTrailMetres + tangent-cell floor, bounded to release history.
```

Unchanged: canonical velocity, obstacle slowdown, donor transport, event frequency, same-anchor overlap, seeds, Contact Fleck, static wake rendering, Initial Presence, Initial Life, lifecycle/support aging, and contact-shell geometry.

Validation must prove that cyan occupancy stops in passed territory for minimum, midpoint, and maximum Arc/Semi-Arc settings; Material Presence behind the frontier must detach and advect.

## Initial Presence and lifecycle authority — `4.11C.5.18E` — Unity-validated and accepted

A runtime lifetime audit showed that the automatic source recipes already authored normalized `Initial Life`, but their peak deposited `Presence` remained hidden as hard-coded per-pattern ranges. `5.18E` exposes those exact existing ranges as `Initial Presence Min/Max`, so importing the patch preserves the prior source result by default while allowing direct tests of presence-limited visible lifetime:

```text
Shore Ribbon             0.90–1.00
Inward Wash              0.84–0.98
Object Contact Arc       0.88–1.00
Object Contact Semi-Arc  0.84–0.98
Object Contact Fleck     0.82–0.97
Free Water Lace          0.78–0.96
Free Water Cross-Lace    0.78–0.96
Free Water Fragment      0.76–0.94
```

`Initial Presence` is the peak persistent material amount before profile, progressive-formation, and valid-fluid masks; it is not opacity and does not replace `Initial Life`. Birth cohort behavior remains unchanged: later-revealed material is younger, and repeated writes over already occupied material do not blindly reset the whole patch.

Lifecycle-Faithful Final Foam now grants full meaningful-footprint participation at Presence `0.10` instead of `0.16`, while retaining the `0.02` disappearance floor and all existing life-driven erosion formulas. Neutral Lifetime and Negative Aging Rate authoring maxima are extended to `20` without changing existing serialized values.

Positive support gains `Full Supported Aging At`, default `0.92`, which exactly reproduces the previous fixed support curve. Lower values let ordinary positive support reach the authored Supported Aging Rate sooner. Negative Aging Pressure retains its accepted fixed `0.08–0.92` shaping and is deliberately not affected by this new control.

Resource contract:

```text
new textures/channels/buffers/dispatches = 0;
new automatic-source events = 0;
source-shape geometry and scheduling = unchanged;
negative-aging response = unchanged;
Lifecycle-Faithful presence footprint high edge = 0.16 → 0.10;
Unity shader/compute import and runtime validation = passed; the user confirmed the short-lifetime issue is fully fixed.
```

## Stage status

```text
Stage 1 — River Domain                         complete and validated
Stage 2 — Water Body                           complete and validated
Stage 3 — Surface Motion and Coherent Flow     complete and validated
Stage 4 — Refraction and Optical Distortion    complete and validated
Stage 5 — Runtime Disturbance and Interaction  accepted for the current milestone
Stage 6 — Foam                                 lifecycle validation active (`5.18E`)
Stage 7 — Secondary Water Effects              complete and validated
Stage 8 — Reflections and Final Integration    stale/retired roadmap item
```

The user Unity-validated `4.11C.5.18C` and `4.11C.5.18D`; the source/contact geometry and truthful Object Foam authoring contract remain accepted. `4.11C.5.18E` is a separate lifecycle-authoring correction: it exposes the formerly hidden source Presence ranges, loosens only the Lifecycle-Faithful meaningful-Presence high edge, extends lifecycle authoring ranges, and exposes positive-support saturation.

The old Stage 8 plan is not an active production stage. The current River has no visible reflection feature. Any dormant or experimental reflection code must not be treated as a completed system or as required future work. A future reflection feature would require a new explicitly approved scope.

## Performance status

All River performance work is deferred to one later **Full River Performance Pass**. Do not start isolated performance patches before that pass unless a new blocking regression requires immediate action.

The future pass must audit the complete River together, including at minimum:

```text
visible versus offscreen Foam work;
empty-field and automatic-source scheduler work;
Layer C / Layer D cadence and sleeping policy;
shader compile and runtime candidate-loop cost;
readback and diagnostic overhead;
chunk visibility, freezing, and quality tiers;
dormant or unwired systems that may create hidden cost;
River ↔ Ground regeneration interactions where separately authorized.
```

P4 accounting and the shader compile recovery checklist remain evidence sources for that future pass. They are not active patch queues.

## Current active queue

The only active River visual patch is `4.11C.5.18F.1 — Detaching Object Arc Deposition Replacement`. It changes Arc/Semi-Arc source rasterization plus one source-type-local GPU timing lane. Chipping, Remaining-Life erosion, Contact Fleck, reflection, and isolated performance work are not queued.

After `5.18F.1` validates, stop and reassess the actual object wake. Do not automatically add same-anchor locks, cooldowns, seed changes, static-wake changes, or velocity changes. Those remain unproven and require new evidence.

## `4.11C.5.18D` import gate

```text
C# and compute import
  no errors; no serialized value reset.

Object Foam Inspector
  Arc and Semi-Arc expose Profile Scale, not Width;
  Fleck exposes Fleck Size;
  help text states that the one-cell shell owns normal thickness.

Automatic Birth Sources
  existing view only; no additional selector;
  Object Contact Shell reports one cell plus current metre dimensions;
  Raw Object Half-Extents reports unpadded registered ranges when anchors exist.

Regression
  equivalent settings produce unchanged source and Final Foam geometry.
```

## Reopen conditions

Reopen Chipping only if:

- the accepted result fails at the production camera;
- a new content case makes the thin-strip artifact materially visible;
- a measured performance regression is traced to Chipping;
- the user explicitly requests a new Chipping feature.

Reopen performance only as the planned comprehensive pass, or earlier for a blocking regression.

Reopen reflections only through a new approved architecture and visual target.
