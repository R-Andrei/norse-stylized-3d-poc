# River Foam Current Status and Deferred Work

## Current status

Stage 6 Foam is complete for the current milestone.

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

## Stage status

```text
Stage 1 — River Domain                         complete and validated
Stage 2 — Water Body                           complete and validated
Stage 3 — Surface Motion and Coherent Flow     complete and validated
Stage 4 — Refraction and Optical Distortion    complete and validated
Stage 5 — Runtime Disturbance and Interaction  accepted for the current milestone
Stage 6 — Foam                                 complete and validated
Stage 7 — Secondary Water Effects              complete and validated
Stage 8 — Reflections and Final Integration    stale/retired roadmap item
```

The user Unity-validated `4.11C.5.18C — Contact-Attached Pressure and Thin Birth Sources`; the live-only source diagnostics, pressure reach, object shell, Shore Ribbon, free-water regression, and Final Foam behaved as intended. `4.11C.5.18D — Object Birth Control Semantics and Stage 7 Closure` now reconciles the remaining Inspector terminology and existing diagnostic readout without changing accepted runtime geometry or serialized values. Stage 7 is formally closed.

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

There is no active River visual-feature patch. Chipping, Remaining-Life morphology, reflection, and isolated performance work are not queued.

`4.11C.5.18D` is a behaviour-preserving authoring/documentation closure:

```text
Arc/Semi-Arc Width labels → Profile Scale;
Fleck Width label → Fleck Size;
fixed shell and raw extent evidence → two rows in the existing source view;
serialized backing fields and source arithmetic → unchanged.
```

After Unity import confirms the Inspector labels and existing debug view, the next sensible River effort is the separately scoped **Full River Performance Pass**. Do not start it implicitly; it requires explicit approval and should audit the whole River rather than produce isolated micro-patches.

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
