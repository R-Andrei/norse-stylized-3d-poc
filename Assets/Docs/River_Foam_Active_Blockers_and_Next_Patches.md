# River Foam Current Status and Deferred Work

## Current status

Stage 6 Chipping/Strands and the `5.18E` lifecycle correction are accepted. The rejected original `5.18F` was replaced by `5.18F.1`, which is now Unity-validated and accepted. The active visual patch is `4.11C.5.18H.3 — Front Contact Bridge and Straight Wake Arms`. The `5.18G` per-anchor scheduler remains accepted; the broad `5.18G.1` near-ring mantle is superseded.

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

## Front Contact Bridge and Straight Wake Arms — `4.11C.5.18H.3` — implemented, Unity validation pending

Unity validation of `5.18H.2` accepted the narrow source width and D3D11-safe compute path, but long Arc/Semi-Arc lengths still followed the obstacle contact ring into the downstream half and could visually close into an O. `5.18H.3` replaces that rear-following geometry with a hard split:

```text
upstream half of physical contact ring
→ left/right side shoulders
→ thin straight downstream wake arms
```

The front bridge samples only immediate obstacle-contact cells whose flow-local position is upstream of the side axis. At and behind the side axis, contact-ring emission is structurally zero. Each shoulder then starts a distance-to-segment ribbon parallel to river flow. Wake arms are clipped only by valid fluid, obstacle exclusion, river bounds, and their authored metre length; they never follow obstacle geometry behind the shoulders.

Active Arc/Semi-Arc authoring:

```text
Formation Speed
Wake Arm Length Min / Max
Initial Presence Min / Max
Initial Life Min / Max
Semi-Arc Lopsidedness Min / Max
```

The previously serialized metre Length fields now truthfully own straight downstream Wake Arm Length. The newer normalized Arm Reach fields are hidden and inert. Profile Scale, Contact Offset, and Breakup remain hidden and inert. Arc uses equal wake arms. Semi-Arc uses one dominant arm and shortens the opposite arm directly through Lopsidedness.

The accepted per-anchor `Build → Hold → Progressive Release → Rest` scheduler remains unchanged. Build and Release traverse one composite path: first arm tip → shoulder → upstream bridge → opposite shoulder → second arm tip. Arc Release follows Build order; Semi-Arc Release retracts the dominant arm first.

Width contract:

```text
front bridge = one immediate contact-cell layer;
wake arm = one strong cross-river row, with at most one feathered continuity row;
Presence is not reduced to imitate fractional-cell geometry.
```

Resource contract:

```text
new textures/channels/buffers/kernels/dispatches = 0;
new persistent GPU state = 0;
Contact Fleck, Shore, Free Water, velocity, transport, lifecycle,
object-contact construction, static wake deformation, and duty-cycle timing = unchanged;
scene/prefab/material/asset/meta changes = none.
```

`5.18H.2` remains the accepted D3D11-safe thin-width baseline but its rear-following path geometry is superseded by `5.18H.3`.

## Superseded geometry — Thin Open-C Object Ribbon Arcs — `4.11C.5.18H` / `5.18H.2`


Unity inspection of hidden obstacle renderers proved that `5.18G.1` solved internal source gaps but selected almost the entire immediate contact perimeter. The near-O footprint injected Foam behind the object, smeared into an object-width downstream slab, and made Arc Length, Profile Scale, Contact Offset, Breakup, and Semi-Arc lopsidedness visually ineffective.

`5.18H` keeps the accepted `5.18G` per-anchor Build/Hold/Progressive Release/Rest scheduler and replaces Arc/Semi-Arc geometry and authoring:

```text
Contact Arc
  one immediate-water-cell open-C ribbon;
  symmetric arms stop at a hard maximum of ±140 degrees;
  at least 80 degrees of downstream rear perimeter remain structurally unsourced;
  Build and same-order Release traverse one contiguous path.

Contact Semi-Arc
  the same continuous upstream bridge;
  one dominant shoulder arm and one lopsidedness-shortened arm;
  reverse-order Release retracts the dominant arm first;
  the downstream rear remains structurally open.
```

The immediate object-contact field is the complete width authority: Arc/Semi-Arc source occupancy is exactly one adjacent water-cell ring, not a multi-cell mantle and not a fractional-Presence approximation. A normalized obstacle-relative angle selects the open-C interval from that ring. No source can be emitted at the downstream rear centre.

Active Arc/Semi-Arc authoring is now deliberately small:

```text
Formation Speed
Arm Reach Min / Max
Initial Presence Min / Max
Initial Life Min / Max
Semi-Arc Lopsidedness Min / Max
```

Legacy metre Length, Profile Scale, Contact Offset, and Breakup fields remain serialized but hidden and inert so existing scene data is not migrated or reinterpreted. Arm Reach changes actual occupied contact texels and the CPU-resolved open-C path length used for Build duration. Presence, life, and lopsidedness use independent deterministic samples rather than the former shared high-biased `eventScale`.

Resource contract:

```text
new textures/channels/buffers/kernels/dispatches = 0;
new GPU persistent state = 0;
Contact Fleck, Shore, Free Water, velocity, transport, lifecycle,
object-contact construction, static wake deformation, and duty-cycle timing = unchanged;
scene/prefab/material/asset/meta changes = none.
```

`5.18G.1` is superseded as a spatial source contract. `5.18G` remains the accepted scheduler foundation.

## Superseded patch — Contiguous Object Face Sweep — `4.11C.5.18G.1`

Unity validation of `5.18G` accepted the per-anchor Build/Hold/Progressive Release/Rest duty cycle, but its source masks remained spatially incomplete: Build and Release used thin frontier bands, the authored Arc/Semi-Arc profile limited Hold coverage, and patterned source-fill variation could cut black stripes through an otherwise active contact region. That defeated the intended contact mantle even while an anchor was actively emitting.

`5.18G.1` preserves the scheduler and replaces only Arc/Semi-Arc spatial emission semantics:

```text
Contact Arc
  Build    one contiguous accumulated side-to-side sweep;
  Hold     complete upstream-facing one-cell physical contact mantle;
  Release  contiguous same-order clearing, so the first-built side releases first;
  Rest     no Arc source.

Contact Semi-Arc
  Build    non-trail side → complete front mantle → one-sided extension;
  Hold     complete upstream-facing mantle plus the optional one-sided extension;
  Release  extension retracts first, then the front clears in reverse Build order;
  Rest     no Semi-Arc source.
```

The mandatory mantle is every eligible upstream-facing water cell immediately adjacent to the raw physical obstacle. Arc Length, Semi-Arc Length, Profile Scale, breakup, and lopsidedness may alter deposition emphasis or the optional one-sided extension, but cannot remove an active mantle cell. Arc/Semi-Arc `SourceFillBlend` is now zero, so patterned source-fill gaps cannot punch stripes through the contact face. Contact Fleck retains its accepted stochastic fill behavior.

Existing authoring is unchanged:

```text
Contact Emission Cycle
  Anchor Coverage       default 1.00
  Hold Duration         default 5–10 s
  Release Duration      default 0.60–1.40 s
  Rest Duration         default 1–3 s
```

`Global Formation Speed` remains Build speed. No scene/prefab migration or serialized-value change is required.

The existing `Automatic Birth Sources` view must show one connected cyan region during Build, a fully covered upstream face during Hold, a connected shrinking region during Release, and black during Rest. Internal black stripes inside the currently active region are a failure.

Resource contract:

```text
new GPU textures/channels/buffers/kernels/dispatches = 0;
new persistent state = 0;
per-anchor duty-cycle scheduler = unchanged from 5.18G;
Contact Fleck, Shore, Free Water, velocity, transport, lifecycle,
contact-shell thickness, and static wake deformation = unchanged;
scene/prefab/material/asset/meta changes = none.
```

`5.18G` is retained as the accepted scheduler baseline; the `5.18G.1` near-ring spatial contract is superseded by `5.18H`.

## Detaching Object Arc deposition replacement — `4.11C.5.18F.1` — Unity-validated and accepted

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
Stage 6 — Foam                                 front bridge + straight wake-arm validation active (`5.18H.3`)
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

The only active River visual patch is `4.11C.5.18H.3 — Front Contact Bridge and Straight Wake Arms`. It preserves the `5.18G` per-anchor schedule and replaces the superseded near-ring mantle with a contiguous one-cell open-C contact path that structurally excludes the downstream rear. Chipping, Remaining-Life erosion, canonical velocity, transport, Contact Fleck geometry, reflection, and isolated performance work are not queued.

After `5.18H.3` validates, reassess Wake Arm Length defaults, downstream tendril width, and clean-face windows before changing defaults. Do not add further wake, velocity, seed, or transport changes unless new evidence requires them.

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
