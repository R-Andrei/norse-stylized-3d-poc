# River Foam Active Blockers and Next Patches

## Active implementation plan — `4.11C.5.18H.6.2 — Thin Mesh Profile and Front-Persistent Semi-Arc Lifecycle`

### Mission

Retain the visibly accepted `5.18H.6` mesh-fitted thin profile while eliminating every active Semi-Arc state that contains an arm without physical-front coverage. Do not alter Arc/Semi width coefficients, wake-arm width, signed offset semantics, mesh-profile producer, runtime resources, or unrelated source families.

### Implemented correction

1. Restored the direct five-point exact-waterline profile from `5.18H.6`; removed the rejected H.6.1 exterior support envelope and event-time clearance expansion.
2. Restored and locked profile raster coefficients to `0.34` strong width and `0.38` feather.
3. Semi-Arc Build path is front point → selected front half → shoulder → one downstream arm tip.
4. Semi-Arc Release removes the arm first and physical-front coverage last.
5. Actual two-segment length splits are packed through existing Arc/Semi-local lanes; object ribbons bypass source-fill evaluation so lane reuse cannot create holes.
6. Full Arc terminal-to-terminal lifecycle remains unchanged.

### Rejected patch

`5.18H.6.1` is rejected. Its `0.55`-cell feather caused double-thick fronts, and its exterior-envelope changes reduced mesh fidelity without solving arm-only Build states.

### Performance/resource contract

```text
new textures/channels/buffers/kernels/dispatches = 0;
new GPU-event vectors = 0;
per-material-update mesh scans/support queries = 0;
profile width/feather = exact H.6 values;
Contact Fleck, Shore, Free Water, transport, lifecycle, velocity,
RiverCorridor/Ground, scene/prefab/material/asset/meta = unchanged.
```

### Unity acceptance gates

1. Zero C# and compute import errors.
2. Semi-Arc front is present from the first visible Build update through the final Release update.
3. Semi-Arc remains exactly one front half plus one arm on both mirrored sides.
4. Front thickness is no greater than the accepted H.6 baseline; no double-row slab is acceptable.
5. Full Arcs retain both thin front halves and both arms.
6. No regression in other source families or profiler markers.

## Mesh-Fitted Arc Paths and True Half-C Semi-Arcs — `4.11C.5.18H.6` — implemented, Unity validation pending

Unity validation of `5.18H.5` confirmed that signed contact-fit controls worked, but rejected its remaining geometry assumptions: Semi-Arc still traversed the complete shoulder-to-shoulder connector, and both source types followed a bounds-derived half-ellipse rather than the generated object's waterline shape.

`5.18H.6` prepares one compact five-point profile from each static object's actual zero-padding waterline contour during the existing staged generated-source refresh:

```text
point 0 = negative-lateral physical shoulder
point 1 = half-distance sample on the first front half
point 2 = physical upstream/front point
point 3 = half-distance sample on the second front half
point 4 = positive-lateral physical shoulder
```

The resolver reconstructs the exact contour through its captured world basis, projects it into authoritative river `GlobalDistance/across` coordinates relative to the registered source centre, selects the upstream shoulder-to-shoulder chain, and samples each half by cumulative path distance. Readable generated meshes use their convex waterline contour; unavailable or degenerate exact geometry falls back first to the zero-padding bounds contour and finally to the former analytic five-point profile. The exact mesh scan occurs only during static-source preparation or dirty refresh, one source per existing staged refresh budget; material updates consume only the cached five points.

Pattern topology is now structurally distinct:

```text
Contact Arc
  downstream terminal A
  → arm A
  → both mesh-fitted front halves
  → arm B
  → downstream terminal B;

Contact Semi-Arc
  selected downstream terminal
  → selected arm
  → selected physical shoulder
  → only that mesh-fitted front half
  → physical upstream/front point.
```

The opposite Semi-Arc front half and arm are never evaluated. Deterministic side selection remains in the existing `variation.w` lane. Object Pattern or Arc/Semi/Fleck weight changes retire active Arc/Semi emitter events immediately while preserving already deposited persistent Foam, so the current pattern authority is visible without waiting for the former cycle to expire.

Signed Along-Flow and Across-River Contact Offsets remain independent, support-agnostic visual-fit controls. Zero uses the prepared physical profile. Across-River fit expands or contracts each shoulder-to-front lateral span independently; Along-Flow fit scales upstream depth relative to the physical shoulder chord while retaining the shoulder anchors. Negative values may deliberately pull source beneath the rendered silhouette. Only a small numerical scale floor prevents profile inversion.

The five points and front split reuse Arc/Semi-local lanes in the existing seven-`Vector4` event. No texture, channel, buffer, GPU-event vector, kernel, or dispatch is added. Arc/Semi no longer read the aggregate contact texture. Each evaluated texel selects one monotonic profile segment by lateral position; Arc additionally evaluates two straight arms and Semi-Arc one. The strong profile row remains `0.34` local normal cell; the final profile outer radius is approximately `0.89` local normal cell so sharp anisotropic sampled joints remain 8-connected without broadening the strong row.

Performance/resource contract:

```text
new textures/channels/buffers/kernels/dispatches = 0;
new GPU-event vectors = 0; existing seven Vector4 lanes retained;
per-material-update mesh scans/support queries = 0;
static/dirty preparation = one exact waterline scan per refreshed source;
Arc front work = one selected line-segment evaluation per texel;
Semi-Arc front work = one selected line-segment evaluation per texel;
Contact Fleck, Shore, Free Water, transport, lifecycle, velocity,
duty-cycle timing, RiverCorridor/Ground, scene/prefab/material/asset/meta = unchanged.
```

Mechanical validation covers exact-lane round trips, both Semi-Arc sides, absent-half exclusion, downstream-centre exclusion, signed offset extremes, CPU/GPU path-length parity, Build/Release monotonicity, representative anisotropic raster connectivity, unchanged kernels/resources, actual C# parsing, malformed multiline-string scanning, and changed-function HLSL parse/code generation. Unity 6000.5.0f1 C# compilation, D3D11 import, runtime visual validation, and profiler confirmation remain authoritative and pending.

## Distinct Single-Arm Semi-Arcs and Signed Contact Fit — `4.11C.5.18H.5` — Unity-observed intermediate; superseded by `5.18H.6`

Unity validation accepted `5.18H.4` as a properly oriented event-owned open-C source with no rear wrap. `5.18H.5` added signed contact-fit controls and removed the second straight Semi-Arc arm. Unity then confirmed the offsets worked but rejected the remaining geometry: Semi-Arc still traversed the complete connector and therefore remained nearly a full Arc, while both patterns followed a bounds-derived half-ellipse instead of the generated mesh waterline. `5.18H.6` supersedes those assumptions.

`5.18H.5` preserves the accepted terminal-to-terminal open-C topology and makes the patterns structurally distinct:

```text
Contact Arc
  complete upstream connector
  + two equal straight downstream arms;

Contact Semi-Arc
  complete upstream connector
  + exactly one deterministic selected-side downstream arm;
  opposite side terminates at its face shoulder.
```

The legacy Semi-Arc Lopsidedness range remains serialized but is hidden and inert. The existing `variation.w` lane now carries only the deterministic selected-side sign, so no GPU event-layout or resource change is required.

Each pattern gains two independent signed visual-fit controls in metres:

```text
Along-Flow Contact Offset
Across-River Contact Offset
```

Zero preserves `5.18H.4`. Negative values shrink the corresponding analytic radius and may deliberately pull the source beneath the rendered object silhouette. Positive values detach it farther. These controls are support-agnostic: they do not sample, infer, or compensate for negative support zones. Only a `0.005 m` numerical radius floor prevents invalid geometry. Offsets are resolved once during CPU event construction, then reuse the existing `objectData.y/z` half-extent lanes; CPU path length, dispatch bounds, and GPU geometry all consume the same adjusted extents.

Performance/resource contract:

```text
new textures/channels/buffers/kernels/dispatches = 0;
new GPU event lanes or persistent state = 0;
per-frame support or contour queries = 0;
Arc GPU work = unchanged;
Semi-Arc GPU work = marginally reduced by omitting the second arm;
Contact Fleck, Shore, Free Water, transport, lifecycle, duty-cycle timing,
and RiverCorridor/Ground interaction contracts = unchanged;
scene/prefab/material/asset/meta changes = none.
```

Mechanical validation must prove exact one-arm Semi-Arc topology for both selected sides, signed offset authority and numerical-floor behavior, CPU/GPU path-length parity, contiguous Build/Hold/Release masks, preserved downstream opening, unchanged kernels/resources, actual C# parser success, and HLSL parse/code generation. Unity 6000.5.0f1 C# compilation, D3D11 import, visual tuning, and profiler confirmation remain authoritative.

## Event-Owned Analytic Open-C Geometry — `4.11C.5.18H.4` — Unity-validated no-wrap baseline; refined by `5.18H.5`

Runtime inspection of `5.18H.3` rejected its Arc/Semi-Arc bridge authority. The straight wake arms were correct, but the bridge still sampled the single global all-obstacle contact field. That field has no per-object identity, so an event could admit rear or foreign contact cells and produce an O/near-O around a rock even though its own arms were straight.

`5.18H.4` removes the global object-contact field from Arc/Semi-Arc geometry. Every event now owns one analytic open C built from its explicit centre and zero-padding physical half-extents:

```text
terminal A
→ straight downstream arm to shoulder A
→ analytic upstream half-ellipse
→ shoulder B
→ straight downstream arm to terminal B
```

The bridge is exactly zero at and behind the shoulder plane. Only the two arms may occupy downstream territory, and their inner edges are capped so they cannot merge across the downstream centre on coarse or anisotropic Foam grids. Build and Release retain the accepted terminal-to-terminal composite path; either downstream terminal may be the first visible source without creating a rear-centre connector. Arc uses equal arms. Semi-Arc retains one dominant arm and a Lopsidedness-shortened opposite arm.

Active Arc/Semi-Arc authoring remains:

```text
Formation Speed
Wake Arm Length Min / Max
Initial Presence Min / Max
Initial Life Min / Max
Semi-Arc Lopsidedness Min / Max
```

The global object-contact field remains available to Contact Flecks only. It is now built only when an active Fleck requires it. Arc/Semi-Arc remove that texture read, use tightly bounded local X/Y raster rectangles, and use the existing CPU-resolved composite path length already packed in `kinematics.z`.

Performance/resource contract:

```text
new textures/channels/buffers/kernels/dispatches = 0;
new persistent CPU/GPU state = 0;
Arc/Semi-Arc contact-field texture reads = removed;
Arc/Semi-Arc-only updates skip the full-field object-contact build;
Arc/Semi-Arc raster rectangles = reduced in X and Y;
Contact Fleck geometry and contact-field semantics = unchanged;
Shore, Free Water, velocity, transport, lifecycle, static wake deformation,
and duty-cycle timing = unchanged;
scene/prefab/material/asset/meta changes = none.
```

Mechanical validation covers the event-owned no-wrap contract, terminal-to-terminal path coordinates, normal/reversed flow orientation, Arc/Semi-Arc authority, anisotropic raster connectivity, unchanged kernel/resource declarations, actual C# parser validation, and HLSL parsing/code generation of the exact changed functions. Unity 6000.5.0f1 C# compilation, D3D11 compute import, runtime geometry, and profiler confirmation remain pending.

### Current-base reconciliation — `Assets(68)` — complete

Read-only comparison against the previous `Assets(64)` source found that all three `5.18H.4` production files remain byte-identical in the current base. The only current River code changes are the explicit `RiverCorridor` ground-render role in `StylizedRiver.cs` and UV3 bank-distance/validity publication plus a TexCoord3 contract check in `StylizedRiverCorridorGeometry.cs`; neither file nor contract is consumed by Arc/Semi-Arc source construction. `River_Rendering_Roadmap.md` contains a newer corridor-material-mask contract and is three-way merged with this patch. The current archive no longer contains `River_Shader_Compile_Recovery_Checklist.md`, so the reconciled package preserves that removal rather than recreating the document; references are updated to the retained compile-recovery evidence in the canonical River documents. The production implementation remains byte-identical to the previously mechanically validated `5.18H.4` source.

## Superseded patch — Front Contact Bridge and Straight Wake Arms — `4.11C.5.18H.3` — runtime geometry rejected and replaced by `5.18H.4`

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
Stage 6 — Foam                                 event-owned analytic open-C validation active (`5.18H.4`)
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

P4 accounting and the retained shader-compile recovery entries in the canonical River documents remain evidence sources for that future pass. They are not active patch queues.

## Current active queue

The only active River visual patch is `4.11C.5.18H.4 — Event-Owned Analytic Open-C Geometry`. It preserves the `5.18G` per-anchor schedule, retains the straight wake arms from `5.18H.3`, and replaces that patch's unowned global-contact bridge with an event-owned upstream half-ellipse that structurally excludes the downstream rear centre. Chipping, Remaining-Life erosion, canonical velocity, transport, Contact Fleck geometry, reflection, and isolated performance work are not queued.

After `5.18H.4` validates, reassess Wake Arm Length defaults, downstream tendril width, and clean-face windows before changing defaults. Do not add further wake, velocity, seed, or transport changes unless new evidence requires them.

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
