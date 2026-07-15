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
Stage 7 — Secondary Water Effects              provisionally complete
Stage 8 — Reflections and Final Integration    stale/retired roadmap item
```

Stage 7 has no broad implementation queue. The targeted closure correction is now implemented as `4.11C.5.18C — Contact-Attached Pressure and Thin Birth Sources` and awaits Unity validation before Stage 7 is formally closed. `5.18C` retains the `5.18B` shared automatic-source evaluator but replaces cumulative source history with latest-update-only evidence, narrows object and Shore Ribbon source footprints, and gives Static Pressure an end-to-end authored Front Reach.

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

There is no active Chipping, Remaining-Life, reflection, or performance patch.

The only active River item is Unity validation of `4.11C.5.18C`. Validate latest-update-only Automatic Birth Sources first, then Static Pressure Front Reach, the immediate one-cell object shell, and cross-river-cell Shore Ribbon thickness before judging transported Final Foam. The experimental `0.50`-pressure-texel raster floor, default Front Reach, and any later object Width relabelling remain evidence-gated. After acceptance, close Stage 7 formally or select a new explicitly scoped feature.

## `4.11C.5.18C` validation gate

```text
Automatic Birth Sources
  no history trails; black after a material update with no source;
  yellow/cyan/magenta remain visible on first write;
  white means same-update overlap only.

Static Pressure Target
  Front Reach changes total upstream distance monotonically;
  requested and resolved metres/texels are reported honestly;
  Strength does not change reach; Contact Sharpness does not change total reach;
  wake and lee remain unchanged.

Object births
  cyan is an immediate one-water-cell shell outside obstacles;
  no obstacle-interior writes; Pressure cannot widen the shell;
  Arc Length changes tangential extent only.

Shore births
  Shore Ribbon is approximately one cross-river source cell at default;
  long longitudinal cells do not widen it;
  Inward Wash remains the only deliberately inward-reaching shore source.

Regression
  free-water source geometry, transport, Remaining Life, Chipping, Strands,
  and Final Foam remain unchanged apart from material born from thinner sources.
```

## Reopen conditions

Reopen Chipping only if:

- the accepted result fails at the production camera;
- a new content case makes the thin-strip artifact materially visible;
- a measured performance regression is traced to Chipping;
- the user explicitly requests a new Chipping feature.

Reopen performance only as the planned comprehensive pass, or earlier for a blocking regression.

Reopen reflections only through a new approved architecture and visual target.
