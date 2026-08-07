# River Foam Stage 6 Architecture — Current Accepted Baseline

## Fixed-metric River Foam coordinate status — P12

The coordinate-consumer migration through `RG-METRIC-P9`, P10/P10a Inspector cleanup, and P11 repository-wide audit are closed. P12 now makes the authored grid selection authoritative at production allocation.

Source defaults select `Fixed Metric / Quality Default` (`0.25 m` Low, `0.15 m` Medium, `0.10 m` High). Explicit `0.25`, `0.20`, `0.15`, and `0.10 m` candidates and direct `Legacy Normalized Across` rollback/A-B selection are available in `Foam → Runtime & Quality`.

Changing selection invalidates live Foam resources and requires the assigned topology cache to be rebuilt for the new descriptor through the existing explicit Edit Mode workflow. Runtime generation or automatic cache serialization is not introduced. Every production consumer continues to read the same active descriptor.

Current Inspector ownership is:

```text
Foam → Runtime & Quality
  Grid Mode + Fixed Cell Size

Runtime Diagnostics → Foam
  authored/active selection + descriptor + candidate + CFL/curvature + memory/work evidence

Actions → Foam Cache & Validation
  explicit cache lifecycle + P12 candidate capture + current P9 endpoint regression
  Historical / Deep Diagnostics (collapsed by default)
```

The first P12 Medium fixed candidate passed runtime/cache/transport evidence. P12a restored ordinary previous/current interpolation. P12b corrected the broad dead-edge Layer C back-and-forth flicker, added one parity-safe previous-committed packed-state texture, and added effective lateral face/flux evidence. Unity then exposed that P12b had incorrectly applied deposit-once ownership to Object Contact Arc/Semi-Arc and therefore silenced their authored Hold and Release phases.

P12c restored the then-required hybrid source contract; later P13C–P13F superseded its persistent Object-emitter lifecycle. The accepted current result is finite automatic packets, a complete initial obstacle-contact ring, first-stroke-only Arc/Semi-Arc wakes, recipe-complete finite contact reinforcement, optional finite independent contact maintenance, and no Hold/Release/Rest material-cadence Object emitter. P12d passed the complete 12-case runtime sweep and selected `0.15 m` after rejecting `0.20 m`. P13F/P13G source ownership is accepted and frozen. No new Debug View is introduced.


### Complete fixed-spacing/lateral-response sweep — P12d

One Play Mode action now runs the real Foam runtime through this matrix:

```text
spacing: 0.25 / 0.20 / 0.15 / 0.10 m
lateral ratio per spacing: 0 / authored / 1
```

The suite uses nonserialized runtime overrides. Each case receives a real descriptor allocation and diagnostic-only transient topology generation, then a deterministic source/material reset, two-second warmup, and at least five seconds/30 frames of ordinary runtime accounting. The assigned cache asset is neither read for mismatched test descriptors nor written/replaced. Initialization Motion Time is frozen across the matrix. After completion, failure, or cancellation, overrides are removed and normal authored cache-only ownership is restored.

The combined report owns machine evidence only: descriptor, topology, CFL/curvature, memory/work, lane face cancellation, lateral movement, zero-ratio isolation, cache immutability, and restoration. Visual candidate selection remains direct Unity review. Existing P7, P9, and single-candidate P12 reports remain final focused regressions.


### Presence and transport A/B ownership — P12e

P12e exposes two independent runtime comparisons while preserving the former result as both defaults:

```text
Foam > Runtime & Quality
  Material Transport Scheme
    Donor Cell
    TVD Superbee
    Bulk-Phase Residual TVD (accepted serialized default)

Foam > Layer E — Rendering > General Composition
  Presence Footprint
    Current
    Presence-Amplitude
```

`Donor Cell` retains the exact first-order packed-state face donor. `TVD Superbee` uses bounded monotonic interior-face reconstruction and the existing CFL/substep contract; it carries Presence, Presence×Remaining Life, and Presence×Pattern through the same conservative face flux and does not alter closed faces or endpoint outflow. D3's three-pass FCT selections were rejected because they remained visibly diffuse while increasing dispatch and memory cost; their runtime resources and selectable enum values were removed by D4. D5 also removes the rejected `Nearest-Characteristic` branch. `Bulk-Phase Residual TVD` is the accepted production transport because it preserves the one-dispatch/no-extra-field budget while removing the dominant shared downstream motion from repeated neighbour averaging.

`Current` retains the accepted Layer E footprint mapping. `Presence-Amplitude` caps the resolved base footprint by committed Presence before the existing opaque patterned-body evaluation. It does not change Layer C material, Remaining Life, Film, Shape, sources, colour, lighting, or final composition controls.

Both controls are serialized, independent, and rebound live. Every current transport selection dispatches exactly one full-field material kernel per CFL substep and allocates no transport-specific full-field texture. `Bulk-Phase Residual TVD` adds only scalar phase/shift state and two material-property values for previous/current presentation phase. No mode changes cache topology or adds a Debug View. P12 snapshot/sweep and Coverage reports include the selected values. Visual compactness and the D5 performance/accounting evidence are accepted. The one-time D5 ABBA suite is retired from the Inspector; its result is frozen below.


### Mode-specific Chip-edge ownership — P12g/P12j

P12f is rejected: its hardened-mask derivative produced exterior and false interior contours, and its per-pixel edge multiplication fragmented connected candidates. P12g replaced only the Presence-Amplitude path; Unity accepted its single exterior eligibility contour and direct carve, but rejected its doubled-diameter production permission as over-broad. P12h is also rejected because one projected reach still created a second production area outside the displayed mask. P12i removes every derived admission region.

Current and Presence-Amplitude intentionally use separate Chip paths:

```text
Current
  edge source       = preChipSoftVisibility
  edge start        = 0.06
  edge selection    = candidate × narrow edge band
  Chip application  = accepted soft-mask reconstruction

Presence-Amplitude
  edge source       = clean committed-Presence/life silhouette
  support gate      = binary at meaningful Presence 0.02
  edge gradient     = length(ddx, ddy)
  edge selection    = candidate field × exact displayed eligibility mask
  Chip application  = direct hardened-mask carve
```

P12j clean-silhouette ownership is rejected because the signal is produced before patterned erosion and structural Strand shaping and therefore cannot define the rendered edge. P12k resolves `strandKeep` before Presence-Amplitude Chip selection and uses `preChipRenderedMask = saturate(foam.mask × strandKeep)` as the sole support, eligibility, removal, and debug geometry. Production permission remains Candidate × Eligibility with no projected reach, inferred depth, or Presence-Amplitude Interior Access.

Current is a protected compatibility path and must remain arithmetic-identical. Presence-Amplitude remains `baseMask = min(baseMask, presence)` with no compression or threshold retuning. Both modes retain the existing candidate field, identity, lifecycle, source system, transport, Film, Shape, resources, kernels, and controls; Current retains Interior Access while Presence-Amplitude disables it. Mechanical validation must confirm Current equivalence, exact pre-Chip rendered-mask ownership, the per-fragment invariant `production <= eligibility`, exact visible-removal reporting, retirement of clean-silhouette plumbing, protected resources/properties, and warning-free changed-function parsing; Unity visual validation remains pending.


### Historical automatic-source ownership at P12c

```text
Nonpersistent sources
  Build/progression    deposit newly revealed positive coverage only;
  repeated old coverage    no new deposit;

Object Arc/Semi-Arc persistent emitters
  Build    progressively growing active emitter;
  Hold     complete active emitter replenished each material tick;
  Release  progressively retracting active emitter;
  Rest     no active event.
```

The GPU uses `max(0, current - previous)` only for nonpersistent source families. When that gate admits a texel, the current authored source contribution remains the absolute Presence target expected by `FoamMergeBornPresence`. Object Arc/Semi-Arc bypass the difference gate and use their existing phase-shaped contribution directly. Manual injections remain explicit one-shot commands.

The `Automatic Birth Sources` view therefore has two valid semantics: nonpersistent sources show only newly deposited coverage, while Object Arc/Semi-Arc show the complete currently active emitter through Build, Hold, and Release. Rest is black.


Static Chipping work is complete for the current milestone and is Unity-validated as a whole.

Accepted production sequence:

```text
coherent Foam
→ analytical Chipping
→ structural Strands
→ composition
```

Accepted Chipping authoring surface:

```text
Chip Amount
Chip Size
Chip Spacing
Chip Irregularity
Chip Edge Width
Chip Interior Access
```

Accepted implementation state:

```text
D.0   dedicated fine-edge/Fray system retired;
D.1A  canonical zero-resource derivative-normalized edge/interior permission accepted;
D.1A.1 persistent-Presence carrier rejected and rolled back;
D.1B  six-control Chipping authoring refactor accepted;
D.1C  medium/large-biased, camera-readable Chip population accepted.
```

The current edge band is good enough for production continuation. `D.1D — Coherent Edge-Bite Admission` is not required and is skipped. The zoom-dependent over-capture around unresolved thin visual strips remains a known deferred limitation. Remaining-Life interaction is not part of the current completion contract and may be reconsidered only as a future optional enhancement.

There is no active Chipping patch. Future Chipping work requires a new visual or performance justification rather than continuation of the retired queue.


Current milestone contract:

```text
Stage 6 Chipping/Strands visual baseline = complete and validated;
Foam lifecycle authoring/visible-duration correction = Unity-validated through 5.18E;
Object Arc/Semi-Arc detaching deposition = Unity-validated through 5.18F.1;
per-object Build/Hold/Progressive Release/Rest scheduling = accepted through 5.18G;
5.18G.1 contiguous near-ring mantle = superseded after Unity visual audit;
thin source width and D3D11-safe Arc/Semi-Arc compute = accepted through 5.18H.2;
event-owned analytic open-C bridge plus straight downstream wake arms = Unity-validated through 5.18H.4;
distinct signed contact fit = Unity-observed through 5.18H.5;
mesh-fitted full Arcs and true one-half Semi-Arcs = implemented through 5.18H.6; thin front-persistent Semi-Arc lifecycle correction = implemented through 5.18H.6.2, Unity validation pending;
Remaining-Life erosion formulas = unchanged by 5.18E, 5.18F.1, 5.18G, 5.18G.1, 5.18H, 5.18H.4, 5.18H.5, 5.18H.6, 5.18H.6.1, and 5.18H.6.2;
dedicated Fray/fine-fragment work = retired;
all River performance optimization = deferred to one later comprehensive River performance pass.
```

No scene, prefab, material, `.meta`, Ground, or Generated Mass file is part of this documentation reconciliation.

## Thin Mesh Profile and Front-Persistent Semi-Arc Lifecycle — `4.11C.5.18H.6.2` — implemented, Unity validation pending

`5.18H.6.2` restores the exact `5.18H.6` five-point waterline profile and exact `0.34`/`0.38` strong-width/feather contract. The rejected H.6.1 exterior support envelope, event-time clearance expansion, and `0.55` feather are absent.

Semi-Arc lifecycle path coordinates are front-first: the physical front has path coordinate zero, the selected shoulder follows, and the sole downstream arm tip has path coordinate one. Accumulated Build therefore establishes front coverage immediately; reverse Release removes the arm first and front last. Full Arc path order is unchanged. Actual segment-length splits reuse existing object-ribbon lanes and source-fill is disabled for Arc/Semi events.

## Front-Persistent Semi-Arc Release and Exterior Contact Envelope — `4.11C.5.18H.6.1` — Unity-rejected regression; superseded by `5.18H.6.2`

Unity runtime validation of `5.18H.6` accepted the mesh-fitted Arc/Semi-Arc direction but found intermittent Semi-Arc states with a downstream arm and no visible physical-front source. `5.18H.6.1` corrects the two proven causes while preserving the accepted terminal-to-terminal Build order.

- Semi-Arc uses same-order progressive Release. The arm terminal retracts first, then the arm retracts toward its shoulder, and the selected front half remains until the final release interval. Early Build may still begin as an arm-only terminal segment by design.
- The cached exact waterline contour is reduced to an exterior five-point support envelope rather than interior convex chords. Signed Along-Flow fit translates the complete profile; signed Across-River fit scales it uniformly about the object centre, preserving asymmetry without collapsing one randomly selected half.
- Event construction applies a cell-aware outward centreline clearance equal to the existing `0.34`-cell strong ribbon radius. Shared miter joints reject out-of-range intersections and remain upstream of both adjacent shifted support lines. The shader strong row remains `0.34` local normal cell; profile feather is `0.55` cell, giving an approximately `0.89`-cell outer radius required by the expanded anisotropic obstacle-equivalent connectivity audit.
- Actual within-half segment-length splits reuse `variation.x` and `kinematics.w` for Arc/Semi events. The seven-`Vector4` GPU event layout, kernels, textures, buffers, dispatches, and material-update work remain unchanged.

Mechanical validation passed 5,000 randomized convex profiles, 180 anisotropic obstacle-equivalent raster cases, 1,640 Build/Release phase checks, C# parser validation, changed-function HLSL parse/code generation, kernel/resource parity, and unchanged unrelated source evaluators. Unity 6000.5.0f1 C# compilation, D3D11 import, runtime visual validation, and profiler confirmation remain authoritative and pending.

## Mesh-Fitted Arc Paths and True Half-C Semi-Arcs — `4.11C.5.18H.6` — Unity-observed baseline; release/envelope refined by `5.18H.6.1`

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

The five points and front split reuse Arc/Semi-local lanes in the existing seven-`Vector4` event. No texture, channel, buffer, GPU-event vector, kernel, or dispatch is added. Arc/Semi no longer read the aggregate contact texture. Each evaluated texel selects one monotonic profile segment by lateral position; Arc additionally evaluates two straight arms and Semi-Arc one. The strong profile row remains `0.34` local normal cell; the outer feather is approximately `0.72` cell so diagonal sampled segments remain 8-connected without broadening the strong row.

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

## Thin Open-C Object Ribbon Arcs — `4.11C.5.18H` — width accepted through `5.18H.2`; rear-following geometry superseded by `5.18H.3`

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

## Contiguous Object Face Sweep — `4.11C.5.18G.1` — superseded by `5.18H`

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

The original `5.18F` attempt is rejected. Its frontier activated only after the revealed span exceeded `initial reach + HeadTrailMetres`; legal short/wide Arc and Semi-Arc combinations could never satisfy that threshold and silently retained the old full-history repaint behavior.

`5.18F.1` replaces that implementation rather than layering another gate on top:

```text
Contact Arc
  finite startup centre pulse;
  two frontiers advance continuously from initial reach to final reach;
  historical centre/interior territory cannot fall back to full repainting.

Contact Semi-Arc
  finite startup shoulder pulse;
  one frontier advances continuously;
  short spans become a finite pulse instead of a held source.
```

The CPU packs normalized material-step duration into an object-source-only GPU lane that Arc/Semi-Arc did not otherwise consume. This guarantees that the startup pulse covers the first raster update at Low/Medium/High material cadence. Frontier depth still uses the existing `HeadTrailMetres`, a tangent-projected cell floor, and a bounded fraction of the available growth span so historical territory is released during every viable event.

Resource contract:

```text
changed runtime files = StylizedRiverFoamRuntime.Injection.cs + CS_RiverFoam.compute;
new textures/channels/buffers/kernels/dispatches/state = 0;
serialized controls and source scheduling = unchanged;
Unity C# compile, compute import, and detachment validation = passed;
the user confirmed that object Foam now detaches and travels as intended.
```

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
Unity shader/compute import and runtime validation = passed; the user confirmed the visible-lifetime issue is fully fixed.
```

## Object Birth Control Semantics and Stage 7 closure — `4.11C.5.18D` — Unity-validated and accepted

The user validated `4.11C.5.18C` in Unity and confirmed that the live-only source view, contact-attached Static Pressure, immediate object shell, and cell-based Shore Ribbon work as intended. Stage 7 Secondary Water Effects is therefore complete and validated for the current milestone.

`5.18D` performs the final authoring and documentation reconciliation without changing accepted source geometry or serialized values:

```text
Object Contact Arc Width      → Inspector label: Profile Scale;
Object Contact Semi-Arc Width → Inspector label: Profile Scale;
Object Contact Fleck Width    → Inspector label: Fleck Size.
```

The existing serialized backing fields and numeric values remain untouched. Arc/Semi-Arc Profile Scale truthfully describes early tangential reveal, feather/profile gating, outward placement bias, and coarse local allowance inside the fixed one-cell shell. Fleck Size describes the fleck capsule geometry. None of these controls owns contact-normal source thickness. The existing serialized and compute field names remain historical implementation details so the validated source arithmetic and stored values stay untouched.

The existing `Automatic Birth Sources` diagnostic gains only two compact read-only rows: the fixed one-cell object shell with current Foam cell dimensions, and the registered raw physical object half-extent range. No new debug view is added. The `RiverFoamStaticObjectSource` contract now explicitly records that its historically named `StaticPressure...HalfLength` members are the zero-padding physical obstacle extents, while the general extent members are padded disturbance/wake bounds.

Resource and behaviour contract:

```text
production source geometry/arithmetic = unchanged;
compute/shader files = unchanged;
serialized field names/values = unchanged;
new textures/channels/buffers/dispatches = 0;
new debug views = 0;
existing debug rows = +2 compact rows;
Stage 7 = complete and validated.
```

## Contact-attached Pressure and thin automatic birth sources — `4.11C.5.18C` — Unity-validated and accepted

`4.11C.5.18C` keeps the shared production/debug source evaluator introduced by `5.18B`, but supersedes its cumulative diagnostic contract and corrects the actual source/contact geometry revealed by that view.

Automatic Birth Sources debug is now latest-material-update-only:

```text
yellow  = current Shore Ribbon or Inward Wash source;
cyan    = current Object Contact Arc, Semi-Arc, or Fleck source;
magenta = current Free-Water Lace, Cross-Lace, or Torn Fragment source;
white   = same-update overlap between multiple source events/categories;
black   = no automatic source written during the latest material update.
```

The complete existing debug texture is cleared once at the beginning of each material update. RGB stores only current-update categories; alpha is set only when a later source event writes a texel already occupied during the same update. The cumulative history counter, cumulative readback state, alpha-only transient-clear kernel, and view-entry history reset authority are removed. One live unique-source-texel counter remains. Normal rendering still uses `RasterizeFoamSourceEvent` and pays no debug UAV write.

Static Pressure gains an honest `Front Reach` authoring value in metres. The old hidden `0.22–0.48 m` CPU authority, obstacle-length inflation, surface-spacing inflation, and pressure-profile-count reach scaling are removed. Requested metres are converted to longitudinal pressure texels and resolved against one explicit `0.50`-texel experimental raster floor. Runtime diagnostics report requested reach plus resolved metres/texels. Strength still owns height; Contact Sharpness still owns falloff shape inside the resolved reach. Wake and lee systems are unchanged.

Object Foam contact eligibility is now the immediate eight-neighbour water shell outside obstacle texels. The former `5 × 5` / 24-neighbour dilation and larger-axis `2.55`-cell reach are removed. Pressure may stabilize contact orientation/confidence and choose the upstream-facing side, but it cannot spatially add source cells. The currently named `StaticPressureAlongHalfLength` / `StaticPressureAcrossHalfWidth` values remain in use because source tracing proved they are the zero-padding raw physical obstacle extents; the more general `AlongHalfLength` / `AcrossHalfWidth` values are padded disturbance/wake bounds.

Standard Shore Ribbon normal thickness is now authored as `Source Thickness` in cross-river Foam cells, default `1`. The GPU resolves thickness and its small antialias feather from cross-river spacing only; it no longer uses `max(longitudinal, cross-river spacing)`. `Source Offset` is a base metre value and `Offset Variation` is bounded in cross-river cells. Shore Ribbon's stale pseudo inward-reach contribution is removed. Inward Wash retains its separate metre-based width and genuine inward reach. Free-water source geometry and scheduling are unchanged.

Performance/resource contract:

```text
new textures/channels/buffers/dispatches = 0;
object contact neighbour checks = 24 → 8;
automatic-birth debug counters = 2 → 1;
normal automatic-source debug write = still absent;
Static Pressure full-field clear/finalize = unchanged;
full River performance pass = still deferred.
```

`5.18C` was Unity-validated by the user and works as intended. Its `0.50`-texel pressure raster floor and current default Front Reach are accepted for the milestone; any future retuning requires a new visual justification rather than continuation of the closed Stage 7 queue.

# River Foam Stage 6 Canonical Architecture

## Canonical zero-resource Chip eligibility — `4.11C.5.17D.1A` — Unity-validated and accepted

The previous edge selector thresholded `materialEdgeDepth = baseMask`. That scalar represented local Presence-derived coverage, not distance from the actual rendered boundary. Equal scalar thresholds therefore produced thick territory where Presence rose slowly and little or no territory where it rose sharply.

D.1A deletes `materialEdgeDepth` from patterned-mask outputs, stored/warped/lead/trail evaluations, surface coupling, retained-material ownership, the result structure, shader properties, runtime bindings, Inspector authoring, and diagnostics.

The replacement reuses the existing production fields:

```text
V = ResolveBaseCoverage(preChipMask)
S = saturate(preChipSoftVisibility)
G = max(fwidth(S), 0.001)
P = max(0, S - 0.06) / G
```

`P` is an approximate inward coordinate measured in rendered pixels. It is a local derivative-normalized estimate, not a global geometric distance field. For authored edge width `W >= 0`:

```text
W = 0:
  edgeBand = 0 exactly

W > 0:
  edgeMembership = 1 - smoothstep(W - 0.5, W + 0.5, P)
  edgeBand = V × edgeMembership
  interiorRegion = V × (1 - edgeMembership)
```

Production uses one permission helper:

```text
edgeSelection = activatedCandidates × edgeBand
interiorSelection = interiorAdmittedCandidates × interiorRegion
productionSelection = max(edgeSelection, interiorSelection)
```

The Inspector slider covers `0–256 px`, while direct numeric entry accepts any non-negative value for intentionally extreme tests. No hidden upper shader clamp is applied.

Diagnostics are reduced to three exact handoffs:

```text
Chip Candidate Field
  activated analytical candidates before material permission;

Chip Eligibility Composite
  dark gray = pre-Chip Foam;
  yellow = canonical edge band;
  magenta = optional Interior Access authority;
  cyan = permission outside visible support, expected absent;

Production Chip Mask
  exact hardened coverage removed before Strands.
```

Performance contract:

```text
new textures/channels/samples = 0
new kernels/dispatches = 0
new candidate iterations = 0
new persistent memory = 0
```

The patch adds one local division and removes the complete `materialEdgeDepth` carrier and four redundant Chip debug paths.


## Multi-axis Chip contour geometry — `4.11C.5.17B.2D2B-B.2K` — geometry accepted; timing superseded by B.2L

B.2J permission behavior is Unity-validated and accepted. B.2K changes only the analytical candidate silhouette because the previous temporal contour could not produce meaningful geometry evolution: it used three cosine harmonics, stayed mirror-symmetric, interpolated along one A-to-B coefficient line, and multiplied all temporal change by Shape Irregularity.

The static B.2J contour remains the cosine-harmonic baseline:

```text
S(theta) = ShapeIrregularity × [a1 cos(theta) + a2 cos(2theta) + a3 cos(3theta)]
```

Temporal Shape Change now occupies the independent sine-harmonic subspace:

```text
T(theta,t) = b1(t) sin(theta) + b2(t) sin(2theta) + b3(t) sin(3theta)
```

Two deterministic, candidate-specific sine-coefficient directions are orthonormalized. A normalized quarter-cycle trajectory moves through that two-dimensional plane. The temporal coefficient direction is normalized to a constant L1 budget of `0.55`, so its raw radial target remains positive in `[0.45, 1.55]` before normalization.

The temporal target is normalized in squared-radius space. Fourier orthogonality gives its exact angular mean, and the final contour blends static and temporal squared radii:

```text
temporalArea = constant for every trajectory phase
finalRadiusSquared = lerp(staticRadiusSquared, temporalRadiusSquared, ShapeChangeAmount)
```

This preserves temporal radial area exactly instead of turning Shape Change into another Size Pulse. Shape Irregularity owns static cosine asymmetry, while Shape Change independently owns temporal sine geometry; either may be zero without disabling the other. Candidate Radius remains the area-equivalent authored scale.

## Historical orthogonal Chip permission — `4.11C.5.17B.2D2B-B.2J` — superseded by D.1A

> Historical methods-tried record. D.1A removes the Presence-isovalue edge territory described below and retains only deterministic Interior Access as an optional permission within the canonical complementary interior.

Unity validation rejected B.2I. Its local visible-support/material-depth proxy contained no reliable established-body distance information, compressed useful response into the top of the slider, and was multiplied after both Edge Coverage and Interior Access. `Chip Inward Admission` therefore duplicated and serially gated the two existing permissions instead of owning an independent domain.

B.2J removes that third gate entirely. Production Chipping now has two parallel, truthful permissions:

```text
Chip Edge Coverage
  owns only the weak Presence-transition fringe;

Chip Interior Access
  deterministically grants a fraction of activated candidate identities
  full permission over established pre-Chip visible Foam.
```

For pre-Chip visible support `V`, edge eligibility `E`, activated candidate field `C`, and candidate-level interior admission `I_c`:

```text
edgeSelection = C × E
interiorSelection = I_c
productionSelection = V × max(edgeSelection, interiorSelection)
```

There is no Material Pattern gate, inward-depth proxy, or final serial admission multiplier. At `Interior Access = 0`, Edge Coverage remains fully operational. At `Edge Coverage` minimum, Interior Access remains fully operational. `Chip Activation` controls the participating candidate population; `Interior Access` controls the fraction of that population permitted to cut established body.

Candidate-independent diagnostics use:

```text
edgeAuthority = V × E
interiorAuthority = V × InteriorAccess
potentialEligibility = max(edgeAuthority, interiorAuthority)
```

The dedicated debug slot formerly used by Inward Admission now shows `Chip Interior Access` authority. Removing the failed gate also removes its square root, derivative, smoothstep, shader property, binding, and serialized Inspector control.

## Extended rigid lateral Chip travel — `4.11C.5.17B.2D2B-B.2H` — retained in the accepted current baseline

B.2F/B.2G are Unity-validated and accepted for lifecycle, rigid evolution, and bounded projected-size readability. B.2H changes only the authority and search completeness of the existing rigid lateral centre motion.

### Control and physical meaning

```text
Lateral Motion Amount (spacing)
  maximum plus/minus candidate-centre excursion measured in Candidate Spacing;
  range 0–2.5.

Lateral Motion Speed (cycles/s)
  independent frequency of the existing smooth periodic lateral wave.
```

For Candidate Spacing `S`, amount `A`, frequency `f`, candidate phase `phi`, and the existing signed smooth periodic wave `W`:

```text
centreY(t) = staticCentreY + S × A × W(f × t + phi)
excursion = ±S × A
peak-to-peak travel = 2 × S × A
peak centre speed = 6 × f × S × A
```

The factor `6` is the exact maximum derivative per cycle of `RiverWaterFoamResolveChipSignedWave`; the Inspector exposes resolved excursion and peak physical speed. Translation remains rigid, so the larger range cannot shear, rotate, or stretch an individual contour.

### Exact adaptive rectangular search

The previous `0.25 × spacing` maximum fit inside the B.2F `3×3`/`5×5` search. At the new `2.5 × spacing` maximum, candidate source cells can originate farther laterally. Let:

```text
R = maximum candidate radius in spacings after bounded view scale,
    size irregularity, and size pulse
A = Lateral Motion Amount
J = 0.39 × Distribution Irregularity
C = 0.5 + J

required downstream offset = floor(R + C), clamped to 1–2
required lateral offset    = floor(R + A + C), clamped to 1–4
```

The shader evaluates the smallest enclosing rectangle from `3×3` through `5×9`. At the complete authored maxima:

```text
R <= 0.65 × 1.40 × 1.45 = 1.320 spacings
A <= 2.5 spacings
C <= 0.89 spacings

required downstream offset = floor(1.320 + 0.89) = 2
required lateral offset    = floor(1.320 + 2.5 + 0.89) = 4
maximum search             = 5×9
```

Lower settings retain smaller rectangles, including `3×3`, `3×5`, `3×7`, `3×9`, `5×3`, `5×5`, `5×7`, and `5×9` as required.

## Bounded projected-size Chip LOD — `4.11C.5.17B.2D2B-B.2G` — Unity-validated and accepted

B.2F remains authoritative for candidate identity, lifecycle, rigid motion, pulse, and shape change. B.2G changes only how each static candidate radius is rendered when projection would make stable Chips unreadably small.

### Controls and ownership

```text
Minimum Stable Radius (px)
  target screen radius for each fully formed candidate;
  zero preserves pure world-space sizing.

Maximum View Scale
  upper bound on readability enlargement;
  one disables enlargement.
```

Candidate positions, hashes, activation, eligibility, lifecycle phase, motion, rotation, pulse, and contour coefficients remain River-space systems. No candidate is generated or anchored in screen space.

### Metric-correct projected radius

For River coordinate `p = (distance, lateral)`, the fragment shader forms the screen-to-River Jacobian from `ddx(p)` and `ddy(p)`. `metresPerPixel` is the largest singular value of that Jacobian: the square root of the largest eigenvalue of its Gram matrix. It therefore measures the most compressed projected direction and automatically accounts for camera distance, field of view, resolution, and surface foreshortening.

For one candidate’s static authored radius `R`, projected radius `r`, target `P`, and maximum scale `S`:

```text
r = R / metresPerPixel
w = 1 - smoothstep(0.75 P, P, r)
required = clamp(P / max(r, epsilon), 1, S)
viewScale = lerp(1, required, w)
stabilizedStaticRadius = min(
    R × viewScale,
    0.65 × CandidateSpacing × candidateSizeMultiplier)
```

The scale is never below one, so close Chips are never shrunk. The target is a readability floor rather than exact billboard locking. `Maximum View Scale` and the existing `0.65 × spacing × sizeMultiplier` static-radius cap prevent unbounded world-space growth.

The target is evaluated after deterministic size irregularity, so each candidate receives its own readability decision. The spacing-relative cap is scaled by that same size multiplier, preserving the accepted authored maximum. Size pulse and lifecycle are applied last:

```text
finalRadius = stabilizedStaticRadius
              × sizePulse
              × lifecycleScale
```

Formation and Dissolve therefore still reach exact zero, and Dormant Time remains exact zero coverage.

### Subpixel tail and search completeness

The old whole-candidate attenuation between approximately `0.75–2.0 px` radius is removed. Only the genuinely unresolved lifecycle tail is faded:

```text
subpixelVisibility = smoothstep(0.25, 0.75, finalRadius / metresPerPixel)
```

D.1C adds a separate fully formed readability admission after bounded enlargement:

```text
stabilizedPixels = stabilizedStaticRadius / metresPerPixel
readabilityVisibility =
  Minimum Stable Radius <= 0
    ? 1
    : smoothstep(0.65 × target, target, stabilizedPixels)
```

This admission suppresses candidates that remain too small even after `Maximum View Scale` has been exhausted. It is evaluated before Size Pulse and lifecycle, so those systems do not redefine candidate readability and still reach exact zero through their existing paths.

The same singular-value metric is used as conservative world-space contour antialias width. Candidate-search selection and the Inspector readout use the conservative `Maximum View Scale` ceiling. The existing `3×3`/`5×5` search and `1.34 × spacing` final-radius cap remain sufficient.

Serialized tuning version 8 assigns `Minimum Stable Radius = 2 px` and `Maximum View Scale = 1.75`.

## Independent Chip lifecycle and rigid evolution — `4.11C.5.17B.2D2B-B.2F` — Unity-validated and accepted

B.2F supersedes the coupled B.2E Evolution Amount/Rate model. B.2D production eligibility and B.2E candidate-independent permission diagnostics remain unchanged.

### Control ownership

```text
Lifecycle — always active for every candidate retained by Chip Activation
  Formation Time
  Stable Time
  Dissolve Time
  Dormant Time

Rigid Motion
  Downstream Speed
  Lateral Motion Amount / Speed
  Rotation Amount / Speed

Living Variation — established stage only
  Size Pulse Amount / Speed
  Shape Change Amount / Cadence / Transition Time
```

The rejected coupled Evolution Amount/Rate storage and the obsolete Chip tuning-version migration chain are removed by D.1B.

### Four-stage lifecycle

For authored durations `F`, `S`, `D`, and `Q`:

```text
T = F + S + D + Q
τ = frac(time / T + candidatePhase) × T
smooth5(x) = 6x^5 - 15x^4 + 10x^3

formation = smooth5(τ / F)
dissolution = 1 - smooth5((τ - F - S) / D)
lifeRadiusScale = min(formation, dissolution)
```

This guarantees monotonic zero-to-one Formation, exact one during Stable Time, monotonic one-to-zero Dissolve, and exact zero throughout Dormant Time. Candidate phase is deterministic per lattice cell, so the same identity returns asynchronously on the next cycle. Lifecycle does not depend on any motion, pulse, rotation, or shape amount.

Size pulse and shape change are eased in and out inside Stable Time. Formation and Dissolve therefore return to the accepted authored base contour and cannot be reversed by a pulse wave.

### Rigid evolution and smear removal

The nonlinear value-noise coordinate warp, derivative-basis inversion, and local metric correction are removed. Candidate-local position is now:

```text
Delta_i(p,t) = p - Centre_i(t)
Centre_i.y(t) = StaticCentre_i.y
                + CandidateSpacing × LateralAmount × wave_i(t)
```

The spatial Jacobian of translation is the identity. Rotation uses an orthonormal axis transform. Neither operation can shear, fold, or stretch a candidate. Downstream Speed remains a uniform River-space translation of the complete field.

### Independent living variation

```text
livingRadius = authoredRadius
               × lifeRadiusScale
               × (1 + SizePulseAmount × pulseWave × stableAuthority)
```

`Size Pulse Amount` is a direct fractional radius excursion: `0.20` means `80%–120%` while established. It never controls death or Dormant Time.

B.2K supersedes the old single-axis A-to-B contour interpolation. Static Shape Irregularity remains the accepted cosine-harmonic contour. Shape Change independently adds a candidate-specific sine-harmonic target moving through an orthonormal two-axis coefficient plane. The target uses a positive fixed coefficient budget and squared-radius blending with a phase-invariant angular mean, so the silhouette redistributes area without becoming a Size Pulse. Candidate Radius remains the area-equivalent scale; the bounded `1.52x` lobe reach is handled by the adaptive search.

### Candidate-search completeness

B.2F originally used an adaptive `3×3`/`5×5` search for the then-approved `0.25 × spacing` lateral excursion. B.2H expanded rigid travel to an adaptive `3×3` through `5×9` contract. B.2K retains the same rectangular derivation but multiplies radius reach by the conservative multi-axis lobe factor:

```text
shapeReach = sqrt(lerp(1, 1.52^2, ShapeChangeAmount))
```

The maximum lateral offset therefore extends to five source cells, producing a `5×11` maximum only when the combined radius, morph, jitter, and lateral settings require it. The area-equivalent candidate-radius base remains:

```text
0.65 Chip Size ratio × 1.40 Chip Irregularity size ceiling × 1.45 Size Pulse
= 1.320 × Chip Spacing
```

### Migration

Serialized tuning version 7 assigns the deliberate slow lifecycle defaults `2.5 / 5 / 2.5 / 4 seconds`. The rejected nonlinear warp is not preserved. Legacy Evolution Amount migrates only to the bounded authorities that have a truthful equivalent:

```text
Lateral Motion Amount = 0
Rotation Amount       = 0
Size Pulse Amount     = old amount × 0.08 radius
Shape Change Amount   = old amount
```

The rejected nonlinear warp is not reinterpreted as rigid movement. Lateral and rotation therefore start neutral and require deliberate authoring.

All independent speeds receive slow defaults.

## Superseded Chip turnover and warp-safety model — `4.11C.5.17B.2D2B-B.2E`

B.2D production selection is retained.

### Candidate-independent eligibility domain

`Chip Eligibility Composite` no longer consumes current candidate contours, Activation, lifecycle phase, or current Evolution position. It now displays the potential permission field:

```text
preChipVisibleSupport
× binaryAntialiasedMaterialAdmission
× max(edgeEligibility, Chip Interior Access)
```

`Chip Interior Access` is a probability over deterministic candidate cells, so its candidate-independent representation is expected permission authority across the visible body. Fractional Interior Access therefore changes yellow intensity rather than drawing current candidate blobs. `Chip Final Selection` and `Production Chip Mask` remain the current-candidate diagnostics.

### Complete candidate turnover

At full Evolution Amount, each participating candidate now has exact lifecycle endpoints:

```text
birth:  radius = 0, visibility = 0
mature: radius reaches the authored maximum, visibility = 1
release: radius = 0, visibility = 0
next cycle: deterministic asynchronous return through frac(candidate clock)
```

The old `0.13×` minimum radius is removed. A pixel-footprint gate suppresses unresolved birth/death remnants; Evolution Amount `0` remains on the unchanged static path.

### Bounded spatial evolution

Spatial coordinate warp authority is capped at `0.20`. Values above `0.20` continue increasing lifecycle, pulse, and contour-morph authority but cannot increase coordinate displacement or fold severity. The derivative-basis correction now:

```text
rejects reversed or near-collapsed bases through a smooth stability weight;
limits corrected local-vector magnitude to 0.67×–1.50×;
blends to the uncorrected evolved-space delta when the basis is unstable.
```

This replaces the former inverse correction that permitted up to `6×` local-vector amplification. No texture, sample, kernel, dispatch, buffer, persistent write, serialized control, migration, or candidate-loop expansion is added.

## Historical visible Chip domain and binary admission — `4.11C.5.17B.2D2B-B.2D` — superseded by D.1A

The B.2C composite exposed a real production limitation, not merely a diagnostic mismatch. Production `Chip Selection Depth` was evaluated against `materialEdgeDepth = baseMask`, which is a local Presence-derived coverage value rather than geometric distance from the Foam boundary. Established material saturates that scalar to `1`, while the serialized selector stops at `0.95`; established Foam was therefore mathematically unavailable to the Chip path. The same path multiplied `materialBody` twice and used a smooth Material Pattern value as cut strength, allowing hardening to erase many partial cuts.

B.2D makes the zero-resource contract explicit instead of pretending that local Presence amplitude contains interior distance:

```text
Chip Edge Coverage
  retains the old low-cost Presence-transition selector;
  low values permit thin weak-material strips;
  high values permit all non-saturated edge material;
  it does not claim geometric distance through an established body.

Chip Interior Access
  candidate-level deterministic permission for established visible Foam;
  0 keeps every activated candidate edge-only;
  1 grants every activated candidate full visible-body permission;
  intermediate values admit complete connected candidates, not pixel noise.

```

The production formula is now:

```text
preChipVisibleSupport = ResolveBaseCoverage(preChipMask)

edgeSelection =
    activatedCandidates
    × materialBody
    × edgeBand(Chip Edge Coverage)

interiorSelection =
    activated candidates admitted by Chip Interior Access

productionChipSelection =
    preChipVisibleSupport
    × max(edgeSelection, interiorSelection)
```

`materialBody` is applied once. At `Chip Activation = 1` and `Chip Interior Access = 1`, every activated candidate can reach selection value `1` anywhere beneath the pre-Chip visible Foam body. Candidate spacing, radius ratio, irregularity, lifecycle, motion, and contour coverage are then the only remaining spatial limits. B.2J adds no texture, texture sample, compute kernel, dispatch, buffer, Layer C write, Layer D write, or candidate-search expansion; the existing adaptive B.2H rectangular search remains unchanged.

Serialized tuning version 6 still initializes existing Rivers to `Interior Access = 0`. The removed historical admission value is no longer read, bound, or clamped.

At B.2D, the Chip composite was corrected only to compare pre-material Chip Final Selection against pre-Chip rendered coverage rather than a post-breakup mask. B.2E then made the permission diagnostic candidate-independent. B.2J preserves that contract while reducing the domain to the independent Edge Coverage and Interior Access authorities.

### Canonical methods-tried ledger

| Method | Status | Reason |
| --- | --- | --- |
| Treat Presence-derived `baseMask` as geometric Chip depth | Rejected | Saturates throughout established Foam and cannot encode inward distance. |
| Raise the existing threshold alone | Rejected | Cannot recover information absent from a flat saturated scalar; even `1` remains derivative-band dependent. |
| Smooth Material Pattern multiplied as Chip strength | Rejected | Partial values are frequently swallowed by the later hardening thresholds. |
| New Chip distance texture or compute pass | Rejected by current constraint | Would provide real distance but violates the approved zero-resource architecture. |
| Separate edge coverage from candidate-level interior permission | Implemented in B.2D | Gives truthful edge-only control and guaranteed whole-body endpoint without new resources. |
| Binary antialiased Material Pattern admission | Rejected after Unity evidence | Produced disconnected transported ranking islands and duplicated Activation/Interior Access ownership. |
| Threshold visible-support/material-depth geometric mean | Rejected after B.2I validation | Local coverage saturated through established bodies, useful response collapsed near one, and the serial multiplier made it dependent on Interior Access. |
| Remove the third gate; keep Edge Coverage and Interior Access as parallel permissions | Implemented in B.2J | Restores independent control ownership and removes unnecessary per-fragment admission work. |
| Three cosine harmonics interpolated along one A-to-B line | Rejected after B.2J validation | Stayed mirror-symmetric, depended on Shape Irregularity, and read as one dragged blob with weak pulse-like flexing. |
| Independent sine-harmonic two-axis temporal geometry | Accepted in B.2K; timing decoupled in B.2L | Decouples temporal shape from static irregularity, keeps coefficient authority constant, and redistributes lobes without changing radius controls. |
| One cycles-per-second control for both target cadence and transition speed | Rejected after B.2K validation | Correct geometry could still switch abruptly because event spacing and interpolation duration were inseparable. |
| Shape Change Cadence plus Shape Transition Time | Retained in the accepted current Chipping baseline; no separate validation task remains | Uses constant-distance deterministic targets and quintic transitions so event frequency and actual geometric speed are independently authored. |
| Candidate-shaped Eligibility Composite | Rejected after B.2D validation | Showed where current chips landed, not the candidate-independent area where Chipping is permitted. |
| Expected permission field for fractional Interior Access | Implemented in B.2E | Represents candidate-level admission probability without changing production behavior. |
| Lifecycle radius floor of `0.13×` | Rejected | Prevented true growth from and return to zero. |
| Spatial warp authority across the full Evolution Amount range | Rejected | Increased coordinate fold and smear risk beyond the visually usable range. |
| Warp capped at `0.20`, full-range lifecycle/morph authority retained | Implemented in B.2E | Separates useful turnover from unsafe coordinate deformation. |
| Up to `6×` inverse-basis correction | Rejected | Allowed near-fold metric reconstruction to amplify local deltas into streaks. |
| Stability-gated `0.67×–1.50×` local correction | Implemented in B.2E | Bounds correction and falls back smoothly near collapsed or reversed bases. |
| Coupled Evolution Amount/Rate for lifecycle, motion, pulse, and morph | Rejected after B.2E validation | Could not provide slow death/dormancy and independent tuning. |
| Zero-duration dormant phase | Rejected | Candidate immediately re-entered formation and read as twinkling. |
| Nonlinear coordinate warp plus inverse metric correction | Rejected and removed in B.2F | Continued to permit smeared or elongated candidates. |
| Explicit four-stage lifecycle independent of variation | Implemented in B.2F | Guarantees formation, stable dwell, dissolve, and exact dormant dwell. |
| Rigid translation/rotation with bounded pulse/morph pairs | Implemented in B.2F | Gives independent physical ownership and removes motion-induced stretching. |
| Adaptive `3×3`/`5×5` candidate search | Implemented in B.2F | Preserves complete coverage when pulse or lateral travel exceeds compact reach. |
| Whole-candidate fade from `0.75–2.0 px` projected radius | Rejected after B.2F validation | Combined with Foam hardening, it erased stable distant Chips rather than merely antialiasing them. |
| Increase all world-space Chip radii for distant readability | Rejected | Would make close Chips oversized and would not account for projection angle or resolution. |
| Unbounded exact screen-size locking | Rejected | Would create arbitrarily large world-space cuts and excessive overlap at extreme distance. |
| Bounded singular-value projected-size LOD | Implemented in B.2G | Preserves world-space identity, enlarges only undersized Chips, keeps lifecycle zero endpoints, and caps world growth. |
| Raise Lateral Motion Amount without expanding source-cell search | Rejected | Candidates can clip or disappear when translated beyond the searched lattice rows. |
| Rigid `0–2.5 spacing` lateral travel with exact adaptive rectangular search | Implemented in B.2H | Provides tenfold travel authority while preserving complete candidate contours and smaller search tiers at lower settings. |

## Purpose

This is the canonical architecture contract for Stage 6 river Foam.

This document is the active source of truth for how the Foam system is allowed to work.

The goal is to reproduce the broad behavior of the visual inspiration river: stylized pale surface-film sheets, connected ribbons, bank and obstacle skirts, temporary bridges, pinches, fractures, edge chipping, small fragments, and thin bright surface streaks, while preserving a performance-safe field-based architecture.

The target is not a physically exact fluid solver and not a foam entity database. The target is a fixed-grid mathematical field system with strict ownership boundaries and no circular dependencies.

## Historical evidence gate — eligibility composites and shape-preserving Chip advection — `4.11C.5.17B.2D2B-B.2C`

Unity evidence from B.2B showed that the large coordinate warp could stretch otherwise valid Chip blobs into long ribbons. The cause was architectural: the warped coordinate controlled both candidate movement and local contour distance. B.2C keeps the same large animated lookup field, but converts each candidate-local delta back into the unwarped River metric through screen-space derivative bases before evaluating the connected contour. The field still controls candidate translation, compression, turnover, and clustering; it no longer owns local chip aspect ratio. Near coordinate folds, the inverse is bounded to prevent unbounded correction.

B.2C also adds exactly two comparison diagnostics using existing fragment data and no new texture:

```text
Chip Eligibility Composite

Dark gray   exact current Final Foam mask;
Cyan        eligibility outside the rendered mask;
Bright yellow rendered Foam overlapping eligibility.
```

B.2C deliberately did not alter either formula.

## Chaotic Chip advection and geometric turnover — `4.11C.5.17B.2D2B-B.2B`

Unity rejected B.2A because its candidate-local centre motion was capped to `0.10 × Candidate Spacing`, radius pulse was only `±3.5%`, and animated orientation could rotate by roughly `±66°`. The visible result was therefore nearly stationary chips rotating in place, with no meaningful travel or growth/shrink.

B.2B replaces that local-wave model with a continuous animated coordinate field evaluated before lattice lookup:

```text
base River coordinate
→ Field Speed downstream translation
→ broad animated coordinate warp
→ finer independently moving coordinate warp
→ candidate lattice lookup
```

At full Evolution Amount the combined field can displace the analytical pattern by several candidate spacings, with materially stronger lateral than downstream authority. Because lookup occurs after the coordinate warp, the existing fixed `3×3` search remains complete regardless of total field displacement. Spatially varying compression and release allow temporary concentrations to form and dissolve without persistent candidate pairs.

Candidate turnover now changes geometry as well as opacity. Each stable cell hash owns an independent lifecycle rate and phase; a candidate grows from approximately `0.13×`, reaches approximately `1.08×`, then shrinks before disappearing. A separate bounded radius pulse adds visible scale variation. Animated rigid rotation is removed: contour orientation remains hash-authored and fixed, while coefficients morph between two candidate-specific connected contours.

The controls remain:

```text
Chip Field Speed (m/s)  global downstream field translation;
Chip Evolution Rate     coordinate-warp, lifecycle, and morph time scale;
Chip Evolution Amount   multi-spacing advection, geometric turnover,
                        radius pulse, and contour-morph authority.
```

No texture, compute dispatch, Motion Field sample, persistent state, candidate-loop expansion, control, or debug view is added. `Evolution Amount = 0` disables the coordinate warp and geometric turnover while preserving Field Speed.

## Superseded Chip evolution attempts

`B.2` is rejected because one candidate was a rigid three-circle union. `B.2A` correctly replaced that union with one connected contour, but its movement and scale formulas were visually ineffective and rotation-dominant. B.2B retains the accepted single-contour representation and static authoring controls while replacing only the evolution model.

## Production Chip control model — `4.11C.5.17B.2D2B-B.1A`

B.1A corrects three source-proven formula defects without adding textures, compute work, candidate samples, or time input.

```text
Candidate Spacing (m)       average lattice density; minimum 0.10 m;
Distribution Irregularity   centre jitter only;
Candidate Radius Ratio      mean radius / spacing, range 0.05–0.65;
Size Irregularity           centred candidate scale range;
Shape Irregularity          fixed-extent silhouette redistribution.
```

Absolute mean radius is now truthful and explicit:

```text
mean radius = Candidate Spacing × Candidate Radius Ratio
```

The ratio is bounded so the existing fixed 3×3 analytical candidate search remains complete at maximum centre jitter and maximum size variation. The old metre-radius authoring was dishonest because shader evaluation silently capped it to `spacing × 0.46`; serialized tuning version 4 migrates the previously visible effective radius into the new ratio.

At Size Irregularity `0`, every candidate uses the mean radius. At `1`, deterministic candidate scale spans approximately `0.58×–1.42×`, twice the former range and centred on the authored mean.

At Shape Irregularity `0`, one circle occupies the candidate outer radius. B.1A originally redistributed that extent across three displaced lobes; Unity later rejected that construction because it read as a rigid cluster. B.2A supersedes only the silhouette construction with one connected harmonic contour while preserving Radius Ratio and Size Irregularity authority.

Distribution, size, and shape hashes remain stable and independent. B.2B adds coordinate advection, geometric turnover, and contour morphing without changing those authoring meanings.

## Superseded production Chip handoff — `4.11C.5.17B.2D2B-B`

This section records the original B production contract and is superseded for Chip domain/admission mathematics first by B.2D/B.2J and now authoritatively by D.1A above. Current production order is:

```text
Layer C persistent material and transported Material Pattern
→ analytical production Chip selection
→ accepted Layer E Strands
→ final composition
```

Production Chipping uses no new texture, compute pass, persistent field, or Layer C mutation. The sparse, jittered single-contour world-space candidates are filtered in four explicit stages:

```text
Chip Activation
× Chip Selection Depth
× transported Material Pattern gate
→ soft-body Chip cut before Strands.
```

The transported gate is a fixed smooth function of Layer C `Material Pattern`. It has no wall-clock phase and no random reseeding. River-space candidates therefore do not remain permanently open: eligible material moving through a candidate controls whether the cut is active. The final cut is reconstructed from the pre-hardening soft body, then applied to the accepted hardened mask before Strands. Legacy `Chip Strength` is serialized only for migration and is no longer bound to the shader, preventing a hidden second Chip pass.

Production controls are mathematically separate:

```text
Chip Activation
  fraction of analytical candidates retained;
Candidate Spacing (m)
  average world-space candidate density;
Distribution Irregularity
  deterministic centre-position jitter only;
Candidate Radius Ratio
  mean candidate radius divided by Candidate Spacing;
Size Irregularity
  candidate-to-candidate radius variation only;
Shape Irregularity
  circle to asymmetric connected-contour distortion only;
Chip Edge Coverage
  edge-only Presence-transition territory eligible for removal;
Chip Interior Access
  deterministic fraction of candidate identities allowed beyond the edge band.
```

Chip diagnostics are ordered and authoritative:

```text
Chip Candidate Field
Chip Activated Candidates
Chip Edge Coverage
Chip Eligibility Composite candidate-independent permission domain
Chip Final Selection       current candidate intersections after both permissions
Chip Interior Access       candidate-independent established-body authority
Production Chip Mask       exact hardened coverage removed
```

`Production Chip Mask` must agree with Final Foam. `Chip Activation = 0` must reproduce the pre-B result exactly. Strands remain unchanged and are clipped only because the already-chipped body reaches them first.

## Play Mode topology startup policy — `4.11C.5.17B.P1`

Topology generation is explicit dirty-time Editor work. Ordinary Play startup is cache-only and must never call obstacle cache generation, `BuildMajorTopology`, `BuildConnectorTopology`, `BuildPocketTopology`, delayed topology replacement, cache-asset mutation, or `AssetDatabase.SaveAssets`.

Startup outcomes are authoritative:

```text
Exact
  validate and install the assigned payload;
  continue without generation or persistence.

Stale-compatible
  install the structurally compatible payload for this session;
  report exact obstacle/settings mismatch reasons;
  do not rebuild or save during Play.

Missing / incompatible
  leave topology-dependent Foam disabled in a stable Preparation Required state;
  report one actionable diagnostic;
  do not retry, generate, read back, or save during Play.
```

The authoring workflow is **Actions → Foam Cache & Validation → Prepare / Rebuild Foam Topology Cache**. It runs outside Play, reuses the existing obstacle/Major/Connector/Pocket generators, builds one payload, stores the assigned cache once, and calls `SaveAssets` once. A matching next Play entry must install the exact cache with zero topology builds.

One compact `[River Foam P1] Startup` summary is emitted per startup outcome. It records total wall time, field dimensions, cache outcome/reason flags, phase counts/times, topology-build counts, registry event counts, dirty cycles, restarts, and Play cache-write attempts. Detailed per-event success logging remains prohibited.

The River-to-Ground restoration cascade is deliberately not corrected here. That is cross-feature work for a separate thread; this River-only patch neither changes Ground code nor suppresses River/Ground structural ownership.

## Explicit cache-build efficiency contract — `4.11C.5.17B.P3`

P1 defines when topology work is allowed. P3 defines the normal cost contract for that explicit Edit Mode work without changing topology results.

The ordinary **Prepare / Rebuild Foam Topology Cache** transaction must:

```text
wait for one ready River domain and settled obstacle registry;
build obstacle exclusion once;
build Major, Connector, and Pocket CPU topology once each;
publish generated topology to the GPU once, after the complete graph exists;
validate package dimensions, fingerprints, collections, and checksum contract;
serialize the normal payload once;
clone payload bytes once into the persistent cache asset;
verify stored metadata and bytes directly;
call SaveAssets once.
```

Incomplete Major-only and Major-plus-Connector GPU publications are prohibited during explicit preparation. They remain available only where an actual live runtime stage requires partial visual publication.

The exhaustive deterministic proof is not part of the normal build. **Run Exhaustive Cache Integrity Proof** explicitly performs repeated serialization, deserialization, exact byte reproduction, generated-channel parity, and deliberate corruption rejection. Strict release validation remains a separate non-mutating action that verifies the stored payload against current domain, obstacle, and generation fingerprints.

Runtime cache readers may use the cache asset's internal read-only payload reference because the codec never mutates source bytes. Public callers continue to receive defensive copies.

GeneratedGeometryRegistry callbacks are observations, not river-boundary invalidations. A restoration wave is coalesced into one pending notification burst and resolved only after the disturbance runtime reports its obstacle registry ready. The final `ObstacleGeometryVersion` decides whether the active cached obstacle field/topology is stale. The geometric river boundary texture is not rebuilt for obstacle-source notifications because it contains shoreline coverage only.

P3 acceptance requires one generated-topology GPU publication and one normal serialization, an unchanged payload size/hash for unchanged inputs, successful strict cache validation and exhaustive proof, and an exact-cache Play launch with zero generation or persistence.

## Steady-state work accounting contract — `4.11C.5.17B.P4`

P4 observes operational Foam work without becoming a scheduler. The accounting window remains inactive by default, begins only through the explicit Start / Reset action after Play startup resources are ready, and never gates dispatch, material lifetime, topology evolution, debug evaluation, readback, rendering, or resource ownership.

The canonical categories are:

```text
total work
  compute dispatch count and logical cell-iteration count;

Layer C material work
  fixed material commits, CFL transport substeps, maximum per-substep CFL,
  material dispatch/cell counts, and CPU command-submission time;

topology work
  dirty evaluations, positive queued-work observations, evolution checks,
  composite topology refreshes, topology dispatch/cell counts, and CPU
  command-submission time;

Layer D diagnostic work
  visual occupancy/shape evaluations, dispatch/cell counts, and CPU
  command-submission time;

diagnostic readbacks
  topology and transport metric requests, completions, errors, and topology
  request timeouts;

empty-field evidence
  material commits that continued while the latest fresh asynchronous metrics
  sample reported zero integrated Presence and zero visible Presence core;

visibility evidence
  accounted active frames split by `surfaceRenderer.isVisible`.
```

CPU measurements cover main-thread setup and GPU command submission, not GPU execution. Visibility is the renderer visibility signal, not a gameplay-distance policy. Empty-field qualification is asynchronous evidence and must not itself stop lifetime aging. Logging is explicit and compact through **Start / Reset P4 Accounting** and **Log P4 Work Summary** under Actions; Runtime Diagnostics remain read-only; no periodic Console output is permitted.

## Current implementation status — `4.11C.5.17B.2D2A`

Unity presentation audit is conclusive: normal Final Foam stuttered beside rocks and banks, while both `Foam Committed Final Preview` and `Foam Evaluated Final Preview` remained stable on the same material population. The committed simulation therefore was not oscillating. The rejected point-velocity residual predictor was the presentation fault.

`4.11C.5.16E.2 — Committed Final Foam Promotion + Residual Prediction Retirement` promotes the proven committed Layer C presentation to normal Final Foam and removes residual prediction from active ownership:

```text
Final Foam
  = current committed Layer C packed state
  + selected Concentration + Lifetime or Lifecycle-Faithful policy
  + existing surface coupling, colour, lighting, opacity, and fog;

removed
  = _FoamRenderAdvectionSeconds
  + render-side canonical velocity reconstruction
  + obstacle-influence prediction fade
  + downstream/lateral point backtrace.
```

`Foam Committed Final Preview` is removed because it is now identical to production. Serialized debug value `16` is reserved and resolves safely to Final Foam. `Foam Evaluated Final Preview` remains diagnostic-only for the later Layer D production decision. No Layer D promotion occurs in this patch.

The two Layer C ARGBHalf textures remain mandatory read/write ping-pong storage for conservative transport; they are not render-history textures and are not removed. The patch adds no texture, field, channel, buffer, kernel, dispatch, or memory allocation. It reduces fragment work by removing production Motion Lane, Obstacle Routing, and Obstacle Exclusion reads that existed only for residual prediction.

P12a later uses that same previous/current committed pair for ordinary fixed-step presentation interpolation. This supersedes the literal `current only` presentation line above without reversing the accepted removal of velocity reconstruction or coordinate backtracing.

Both accepted visibility policies remain available. Supported Aging Rate remains `0.05–1.00`, and lifecycle aging remains one complete material-tick decrement on the final CFL substep. The grey-body/white-border appearance exists in committed and evaluated presentation alike and is therefore recorded as a later Layer E shader-composition issue, not a transport or lifecycle fault.

Status: `5.16E.2` is Unity-validated and accepted. Normal Final Foam matches the former committed preview, the rock/bank stutter is gone in both visibility policies, `Foam Evaluated Final Preview` remains available, and serialized debug value `16` resolves safely to Final Foam. `5.16E.3` has since attributed the remaining transport capacity loss, and `5.16E.3C` records the consciously deferred sub-1% PoC limitation without changing the solver.

The River Inspector and diagnostics redesign R1–R5 is also Unity-validated and accepted. It changes only Editor organization and presentation: all sections are collapsed by default, authoring follows feature ownership and Foam Layers A–E, one exclusive debug hub controls the existing serialized debug fields, runtime telemetry is read-only and stable-height, mutating tools live under Actions, and constant repaint is limited to visible live diagnostic leaves.

`4.11C.5.17A.1 — Interior Composition Authority Correction` is accepted. `5.17B` and `5.17B.1` are rejected; `5.17B.2` established usable pre-hardening authority and `5.17B.2C` provides state-preserving Inspector tuning and Hold Foam State. Same-state evidence rejected `5.17B.2B`, the periodic `5.17B.2A` Strand path, reconstructed D1, and the D1B/D1C shaping models. `5.17B.2D1A` proved exact lineification extraction. `5.17B.2D1D` is Unity-validated and accepted: Strength, Scale, Density, and Reach now produce viable Strands without excessive visual artefacting. `5.17B.2D2A` is partially useful but not accepted.

## Approved Layer E finishing contract — `4.11C.5.17P`

The inspiration comparison is refreshed before final rendering work. The production river is not expected to copy the reference one-to-one. Its accepted macro result already contains the required family resemblance: broad predominantly horizontal bands, lateral travel, split/merge behavior, obstacle-driven convergence, and stronger shore accumulation. Slightly fatter ribbons and greater bank accumulation are acceptable consequences of the current field resolution and source grammar. Layer C and the existing motion system remain the macro authority.

The approved Layer E order is:

```text
committed Layer C material and selected visibility policy
  -> local rendered morphology
  -> interior colour / opacity composition
  -> post-breakup edge contrast
  -> final water lighting, fog, and reflection/refraction composition
```

Layer E reads upstream data and writes screen pixels only. It adds no persistent damage state, transported channel, compute pass, or feedback into Layers C or D.

### `5.17A / 5.17A.1 — Layer E Interior Composition`

`5.17A` failed visual validation. The controls were bound correctly, but `Interior Fill Strength` operated after the visible mask had already been hardened near full coverage, `Interior Opacity Floor` was incorrectly capped by `Foam Colour` alpha, and `Edge Emphasis Strength` added only a weak second whitening treatment instead of controlling the existing rim produced by the edge-versus-interior lighting transition.

`4.11C.5.17A.1 — Interior Composition Authority Correction` replaces that failed control model. Its authoring surface is:

```text
Foam Colour                 base RGB/tint and base opacity before the interior floor
Interior Opacity Floor      absolute minimum opacity for established Foam body
Edge Contrast               signed control over the existing edge-versus-interior lighting contrast
```

`Interior Fill Strength` is removed. The current visibility path already supplies a hardened established body, so a second fill remap had no useful authority and risked duplicating later morphology. `Interior Opacity Floor` may now exceed `Foam Colour` alpha, but it is gated by `smoothstep(0.42, 0.82, incoming mask)` and therefore cannot create Foam in weak fringe or outside the incoming silhouette. `Edge Contrast` ranges from `-1` to `+1`: negative values suppress the existing bright rim toward filtered interior lighting, zero preserves the accepted pre-5.17A lighting exactly, and positive values intensify the existing edge.

Production Final Foam and Foam Evaluated Final Preview share the corrected composition helper. Neutral values are:

```text
Interior Opacity Floor = 0
Edge Contrast           = 0
```

At neutral values, opacity reduces exactly to `smoothstep(0.08, 0.46, mask) × Foam Colour alpha` and lighting reduces exactly to the pre-5.17A edge/interior transition. The correction adds arithmetic only: no texture sample, branch, loop, neighbourhood stencil, persistent resource, compute dispatch, readback, material-state change, or silhouette expansion.

### `5.17B / 5.17B.1 — Rejected Hardened-Mask Breakup`

`4.11C.5.17B` and `4.11C.5.17B.1` are both visually rejected. Stronger threshold constants did not solve the ownership error.

Both patches applied breakup after the visibility signal had already been hardened:

```text
float hardVisible = smoothstep(0.22, 0.58, softVisibility);
float fringe = smoothstep(0.06, 0.34, softVisibility) * 0.34;
float hardenedMask = saturate(max(hardVisible, fringe));
```

Most visible body pixels therefore entered the breakup helper near `1.0`. The old equations altered mainly the narrow antialiased transition, while `5.17A.1` Interior Opacity Floor concealed partial erosion that did not reach zero. Breakup Scale also weakened the result at broader settings because the broad/diagonal composite had a compressed centre-weighted distribution. No further hardened-mask threshold recalibration is allowed.

### Historical `5.17B.2 — Pre-Hardening Binary Edge Cuts`

Historical status: Unity-validated for visible authority at that time. Breakup Scale visibly modifies the result and is provisionally accepted with the rest of this feature family; no separate Scale-coherence correction blocks current work.

The public authoring surface is unchanged:

```text
Chip Strength   0–1; default 0
Breakup Scale   0–1; default 0.5
```

`RiverWaterFoamPatternedMask` now preserves its continuous pre-hardening `softVisibility` transiently while leaving the accepted hardening equation numerically unchanged. `RiverWaterFoamResult` carries that scalar through the existing visual warp, stretch, surface-break, stored-retention, and freeze coupling beside the existing hardened mask. Layer E then performs antialiased binary survival tests against `softVisibility` and multiplies the result into the hardened mask. Exact saturated soft cores remain protected. The post-breakup result is removal-only and always satisfies `postBreakupMask <= hardenedMask`, so Interior Opacity Floor cannot refill a removed pixel.

The stable pattern path still evaluates exactly the same broad, diagonal, mid, and fine noise calls. Static distribution analysis shows the selected-field means remain effectively matched across Scale endpoints, so Scale changes feature size/frequency without silently collapsing authority. The accepted combined visibility pattern is unchanged.

Production Final Foam, Foam Evaluated Final Preview, Foam Shader Detail Probe, and Foam Shader Detail Difference continue to use the same breakup helper. The Probe shows the exact production post-breakup silhouette. Difference remains removal-only: black is unchanged and magenta/red is removed coverage; green remains zero. The evaluated preview supplies its evaluated shape to the same binary helper without promoting Layer D to production.

The fixed proof still reads no Remaining Life, Support, Negative Topology, surface-energy multiplier, river-location multiplier, or additional time input. It adds no texture sample, procedural-noise call, texture, buffer, persistent field, compute kernel, dispatch, readback, shader property, or C# binding. Incremental cost is fragment arithmetic plus one transient scalar and possible register pressure.

The original short-cut contribution is separated into optional `5.17B.2A` Foam Strands.

### Superseded `5.17B.2A` / rejected `5.17B.2B`

The periodic-lane Strand implementation and the partial-presence Edge Fragmentation model are no longer part of the active architecture. The Fragmentation controls and shader properties are removed.

### `5.17B.2C — State-Preserving Foam Authoring`

Status: Unity-validated and accepted.

The River Inspector no longer treats every serialized edit as a structural river change. `OnValidate()` still clamps settings, keeps required outputs/runtimes present, and applies live material values, but it no longer queues `RegenerateAll()`. The custom Inspector now requests the existing debounced full rebuild only when one of the structural authoring sections changes:

```text
Setup
River Domain
Channel Shape
Shoreline Safety
Natural Variation
Surface Mesh
```

Spline edits remain structural through the existing spline-change callback. Water rendering, surface motion, refraction, runtime-disturbance tuning, Foam Layers A–E, debug selection, and diagnostics no longer rebuild the River domain merely because a value changed. Layer E rendering values continue through the existing per-frame property binding, so they preserve the active Layer C textures.

`Runtime & Quality` also exposes the non-persistent Play Mode diagnostic **Hold Foam State**. While held, the runtime preserves the allocated Layer C material and existing Layer D products, skips topology evolution, births, aging, transport, and Layer D temporal advancement, discards elapsed wall time, and continues binding the current textures plus live Layer E properties. Pending manual/automatic work remains queued and resumes without catch-up when the hold is released. The toggle is not serialized authoring data and resets when the component is enabled or disabled.

This patch adds no texture, buffer, shader property, compute kernel, dispatch, readback, or persistent simulation field. Structural or resource-allocation changes may still legitimately rebuild and clear state; Hold Foam State is for same-domain rendering comparisons, not for preserving material across incompatible domain changes.

### `5.17B.2D1 / D1A — Lineification Extraction`

`5.17B.2D1` is visually rejected. The failed reconstruction and its transient Strand field are removed.

The neutral body and separate stable lineified soft signal from D1 are retained.

```text
same lineified soft visibility
same Breakup-Scale-selected Chip pattern
same fwidth antialiasing footprint
same exact-core protection
```

Therefore, at the same Breakup Scale, these configurations are intended to be mathematically equivalent:

The proof deliberately adds no spacing reinterpretation, width reinterpretation, curvature warp, grouping mask, delta reconstruction, or post-threshold screen-space culling. Strand Spacing, Strand Width, and Strand Curvature remain serialized for the later shaping step but are visibly disabled in the Inspector and have no shader authority during D1A. Production Final Foam and Foam Evaluated Final Preview use the same exact helper.

D1A removes the failed four-component transient Strand field and its propagation through stored, warped, lead, and trail evaluations. It adds no texture sample, procedural-noise evaluation, hash evaluation, texture, buffer, persistent field, compute kernel, dispatch, readback, Layer C mutation, or Layer D mutation. Unity same-state validation confirmed exact visual equivalence.

### `5.17B.2D1B — Strand Shaping and Projected Detail Floor`

`4.11C.5.17B.2D1B` is visually rejected as a Strand-control solution. It deliberately combines the shaping controls with artifact suppression because both operate on the same continuous source family:

```text
Strand Spacing
  higher values retain fewer, more separated structures.

Strand Width
  changes continuous selection breadth/depth;
  reference 0.50 preserves the D1A threshold model at resolved distances.

Strand Curvature
  broad-modulates the existing stable anisotropic band field;
  reference 0.55 preserves the D1A source and no time/seed replacement is introduced.
```

The shader now carries a dedicated transient `strandSoftVisibility` beside the coherent and legacy lineified signals. At the serialized reference values (`Spacing 0.55`, `Width 0.50`, `Curvature 0.55`) and when source bands are resolved, the Strand path reduces to D1A. Projected river-space footprint is measured once before wake/lee branching. The Strand-only pattern pair then falls back continuously from fine to medium and from medium to broad existing bands as source density becomes unresolved; the anisotropic band breaker similarly falls back to the existing broad/diagonal field. No finished removal mask is re-thresholded, grouped, dithered, or culled per pixel.

D1B adds no texture sample, procedural-noise/hash call, texture, buffer, persistent field, compute kernel, dispatch, readback, Layer C mutation, or Layer D mutation. It adds transient arithmetic/register pressure only.

### `5.17B.2D1C — Strand Spatial Controls and Resolution Cutoff`

`4.11C.5.17B.2D1C` is visually rejected. Static mathematics assigned distinct coordinate-frequency, band-width, and coordinate-warp operations, but Unity evidence showed that all three controls were effectively inert. D1C shaped only the anisotropic band while the decisive candidate topology remained fixed. Its projected-detail estimate also omitted the transported Material Pattern phase that participates in every procedural coordinate. No further constant calibration is allowed on that control model.

### `5.17B.2D1D — Strand Control Model Reset and Coherent Pattern Transport`

`4.11C.5.17B.2D1D` is Unity-validated and accepted. Its Strength, Scale, Density, and Reach controls can produce viable controlled lineification without excessive visual artefacting. This accepted Strand path is the reference that D2 must preserve.

The independent Strand authoring surface is now:

```text
Strand Strength
  overall Strand contribution;

Strand Scale
  hierarchical broad/medium/fine source ownership;
  zero retains finer subdivision, one keeps broader structure;

Strand Density
  candidate selection prevalence only;
  zero is sparse, one is dense;

Strand Reach
  anisotropic attenuation, presence eligibility, and cut depth only;
  zero remains shallow, one permits deeper weak-to-medium-body channels.
```

The old serialized Spacing, Width, and Curvature values migrate into Scale, Density, and Reach through `FormerlySerializedAs`; the obsolete names are removed from bindings, shader properties, Inspector labels, and active documentation.

Strand Scale builds that pair hierarchically from the existing broad, medium, and fine bands. Fine and medium contribution disappear first as their projected footprint becomes unresolved; if even the broad organization is unresolved, Strand authority returns to the coherent Foam body. Candidate thresholds receive derivative-aware antialiasing.

Projected resolution now includes the transported Material Pattern derivative multiplied by the same seed factors used by the broad, diagonal, medium, fine, and anisotropic sources. The derivative is resolved outside wake/lee branching. Stored, warped, lead, and trail Strand patterns and resolution authority are transported with the soft shape that owns them; `max` paths choose the winning sample's pattern rather than applying the stored pattern to a different visible shape.

D1D adds no texture sample, texture, buffer, persistent field, compute kernel, dispatch, readback, Layer C mutation, or Layer D mutation. It removes the extra D1C shaped-coordinate noise call and replaces it with arithmetic, transient Strand pattern/resolution values, and derivative-aware candidate selection.

### `5.17B.2D2 — Rejected Visibility-Contour Role Separation`

Unity visually rejected D2. That scalar contains procedural noise, time-driven morphology, wake/stretch composition, and internal valleys; it is not monotonic distance from the Foam silhouette. Spatially varying thresholds therefore exposed nested iso-contours and elongated channels rather than edge-connected bites and shallow perimeter roughness. Sequential multiplication did not create geometric boundary awareness. No further threshold calibration of that model is allowed.

### Lifetime and topology rule

Layer C remains the sole owner of Remaining Life. Support and Negative Topology influence Foam through the accepted Layer C aging rates. Layer E must never modify Remaining Life.

The former `5.17C` lifecycle-derived morphology progression and `5.17D` fine-fragment/final-energy queue are retired from the active completion contract. Remaining-Life modulation of Chipping or Strands may be reconsidered only as a future optional experiment. Dedicated Fray, micro-fragment, micro-bubble, and glint systems are not required for the production camera.

### Performance contract

```text
new persistent textures / fields / channels = 0
new compute kernels / dispatches / readbacks = 0
cost location = fragment shader only
wide neighbourhood sampling = rejected by default
```

Reuse the existing shader-detail probe and available samples where practical. Profile before accepting any broad sampling stencil.

---

# 0. Non-negotiable design goals

## 0.1 Visual target decomposition

The reference river is not one effect. It is a stack of visual phenomena:

1. **Broad pale surface film**
Large white/pale sheets sit on the water surface. They read as continuous film rather than as discrete particles.

2. **Connected ribbons and current seams**
Foam forms long broken bands along flow lanes, banks, rocks, and darker water pockets.

3. **Emergent split / merge / pinch / reunite appearance**
Visible film narrows, separates, rejoins, and creates temporary necks through material advection and temporal sheet evolution. A separate persistent macro-fracture state is explicitly rejected.

4. **Chipping and edge chaos**
This can be largely procedural and local.

5. **Thin bright streaks**
Narrow fast white scratches/streaks in the reference are not the same layer as broad film. They should be shader-side detail or a separate lightweight detail layer.

6. **Bank / rock / obstacle contact foam**
Pale film gathers around banks and obstacles. This is not merely transported material; the visual system needs external contact/support fields.

## 0.2 Performance target

The solution must remain viable for desktop PC first, including low-to-medium hardware. Mobile is not a target, but the game must not rely on high-end GPU headroom.

The architecture must scale with:

```text
river field cells × update rate × active visible river chunks
```

not with:

```text
number of foam islands / pockets / entities
```

and not primarily with:

```text
screen pixels × frame rate × wide neighbourhood shader samples
```

## 0.3 Data authority rule

Every data product has exactly one writer.

```text
Layer A writes/owns River Domain data.
Layer B writes/owns External Influence Fields.
Layer C writes/owns Persistent Foam Material.
Layer D writes/owns Visual Foam/Film products such as _FoamShapeMask.
Layer E writes only final rendered pixels.
Layer F schedules/binds/debugs; it does not own foam behavior.
```

If Foam looks wrong, diagnose the owner of the wrong product instead of adding another hidden authority.

## 0.4 No circular dependencies

The dependency graph must be acyclic.

Allowed flow:

```text
Layer A — River Domain
        ↓
Layer B — External Influence Fields
        ↓
Layer C — Persistent Foam Material
        ↓
Layer D — Visual Foam / Film Evaluation
        ↓
Layer E — Shader Composition
```

Layer A may also be read directly by B, C, D, and E. Layer B may be read by C, D, and E. Layer C may be read by D and E. Layer D may be read by E.

Forbidden flow:

```text
Layer D → Layer C
Layer D → Layer B
Layer C → Layer B
Layer E → any compute/simulation layer
Layer E → Layer C
Layer E → Layer D
```

Foam-derived visual helper fields belong inside Layer D. They are not External Influence Fields and must never feed Layer C.

## 0.5 No entity database by default

Do not introduce foam pocket IDs, connected-component tracking, per-pocket state, or a foam island database unless field methods fail and the user explicitly approves that architectural pivot.

The default solution is field math:

```text
for each cell/pixel, compute output from upstream fields and local/limited-neighbour information
```

not:

```text
for each foam island, track identity, split history, merge history, velocity, and shape state
```

---

# 1. Canonical layer stack

The previous three-stage summary remains useful, but the complete architecture is six named layers. The letters are intentional: do not use `Stage 1.5` because it implies an arbitrary half-stage and caused confusion.

```text
Layer A — River Domain
Layer B — External Influence Fields
Layer C — Persistent Foam Material
Layer D — Visual Foam / Film Evaluation
Layer E — Shader Composition
Layer F — Scheduling, Quality, Debug
```

The condensed user-facing summary is:

```text
Stage 1 = Layer C: persistent material simulation.
Stage 2 = Layer D: visual film/shape compute evaluation.
Stage 3 = Layer E: shader composition and local polish.
```

Layer A and Layer B are upstream foundations. Layer F is orchestration.

---

# 2. Layer A — River Domain

## 2.1 Abstract responsibility

Layer A defines the coordinate system and river-space truth.

It answers:

```text
Where is the river?
What direction is downstream?
What is across-river coordinate here?
Which cells are valid water?
Where are banks and boundaries?
How do world/surface pixels map into foam/material textures?
```

Layer A does not know or care whether Foam exists.

## 2.2 Current relevant code

Current and related code paths include:

```text
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/RiverDomainSnapshot.cs
Assets/Game/Procedural/Rivers/StylizedRiverCorridorGeometry.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Coordinates.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Sampling.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl
```

Important current symbols/functions include:

```text
RiverDomainSnapshot
BuildSharedSplineSamples(...)
_FoamDimensions
_FoamValidLength
_FoamSimulationLength
_FoamGlobalStart
_FoamFieldLength
_FoamBoundary
_FoamCurrentShoreEdgesRead / _FoamCurrentShoreEdgesWrite
FoamValidFluidAt(int2 coordinate)
LoadBoundaryCoverage(...)
SampleBoundaryCoverageBilinear(...)
FoamUVToTexelCoordinate(...)
RiverWaterFoamResult.materialUV
```

## 2.3 Owned data

Layer A owns or defines:

```text
river-space coordinate convention
full-resolution foam grid dimensions
valid/simulation length
boundary/coverage texture
shore edge texture
river/global start offset
field length
material-space UV mapping
flow direction conventions
```

## 2.4 Allowed reads

Layer A may read:

```text
river spline/domain data
river width and shape settings
corridor/water mesh settings
terrain/corridor geometry where required for masks
```

Layer A must not read:

```text
Persistent Foam State
_FoamShapeMask
Stage D visual helper textures
shader output
```

## 2.5 Writers and consumers

Layer A writes domain data. It may be consumed by:

```text
Layer B — to place external influence fields in river space.
Layer C — to transport and clip persistent material.
Layer D — to evaluate visible shape in the same coordinate system.
Layer E — to sample foam and render debug/final output.
```

## 2.6 Connectivity invariant

All directional concepts must use this layer's coordinate basis.

If one field says “left,” “right,” “upstream,” “downstream,” “across,” or “cell,” it must mean the same thing to every consumer. Any disagreement here is a Layer A bug, not a Foam artistic issue.

---

# 3. Layer B — External Influence Fields

## 3.1 Abstract responsibility

Layer B contains foam-agnostic environmental influence. It answers:

```text
Where is foam encouraged?
Where is foam suppressed?
Where is material or visual film shaped by banks, rocks, wakes, pressure, or motion intent?
```

Layer B does not mean “foam exists here.” It means the environment provides a reason for foam to be born, preserved, suppressed, bent, visually supported, or locally agitated.

## 3.2 Critical correction

Layer B must not read Foam data.

This is the correction to the earlier ambiguous `Stage 1.5` wording.

Allowed:

```text
River Domain → External Influence Fields → Persistent Foam Material
River Domain → External Influence Fields → Visual Foam/Film Evaluation
```

Forbidden:

```text
Persistent Foam Material → External Influence Fields → Persistent Foam Material
Visual Foam/Film → External Influence Fields
```

Foam-derived sheet-support fields are Layer D internal fields, not Layer B fields.

## 3.3 Current relevant code

Current and related code paths include:

```text
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Compute.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Obstacles.cs
Assets/Game/Procedural/Rivers/FoamTopology/*
Assets/Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.*.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Topology.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Support.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Motion.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Evolution.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.TopologyTransition.hlsl
```

Important current texture/symbol names include:

```text
_FoamTopologyRead / _FoamTopologyWrite
_FoamTopologySourcesRead / _FoamTopologySourcesWrite
_FoamTopologyGeneratedRead
_FoamTopologyTransitionFromRead
_FoamObstacleExclusionRead / _FoamObstacleExclusionWrite
_FoamMotionLaneRead
_FoamObstacleRoutingRead
_FoamRippleField
_FoamWakeField
_FoamStaticWakeField
_FoamStaticPressureField
_FoamEvolvingMajorRead / Write
_FoamEvolvingHostedNegativeRead / Write
_FoamEvolvingFreeWaterNegativeRead / Write
_FoamEvolvingConnectorRead / Write
_FoamCurrentShoreEdgesRead / Write
FoamResolveMotionFieldSample(...)
FoamLoadObstacleRoutingCell(...)
FoamSampleMotionLaneSmooth(...)
FoamValidFluidAt(...)
```

## 3.4 Owned data

Layer B owns external influence textures and fields, including current or future versions of:

```text
valid-fluid support context
obstacle/solid exclusion
positive support/topology
negative/free-water suppression or aging pressure
bank/shore/contact support
rock/contact support
wake support and lee context
pressure/ripple disturbance influence
motion/lateral intent
birth support
lifetime support
visual contact support
```

## 3.5 Allowed reads

Layer B may read:

```text
Layer A river domain and coordinate data
rocks/obstacles/banks/collider-derived river interactions
static disturbance emitters
dynamic disturbance emitters
time
its own previous influence texture when a field is intentionally persistent, such as wake decay
```

Layer B must not read:

```text
_FoamStateRead
_FoamStateWrite
_FoamShapeMask
Layer D visual helper textures
Final shader output
```

## 3.6 Writers and consumers

Layer B writes External Influence Fields.

Layer B may be consumed by:

```text
Layer C — birth, lifetime, exclusion, future real material transport.
Layer D — visual support, visual deformation, bridge/pinch context, contact film.
Layer E — debug or local polish if useful.
```

## 3.7 Resolver requirement

Layer B should resolve raw contradictory inputs before consumers read them.

Do not expose several raw fields that can mean different directions to different layers. Instead, raw inputs should be combined into canonical resolved fields with fixed meanings.

Raw inputs may include:

```text
bank contact
rock contact
obstacle exclusion
lane motion
obstacle routing
pressure/wake/ripple
negative zones
dynamic emitter influence
```

Canonical resolved outputs should eventually include explicit meanings such as:

```text
birthSupport
lifetimeSupport
exclusion
resolvedFoamVelocity
visualContactSupport
breakupAgitation
```

If two raw influences conflict, Layer B resolves the conflict once. Layer C and Layer D then read the same resolved intent instead of inventing separate interpretations.

## 3.8 Connectivity invariant

Layer B is upstream context. It is not Foam state.

Correct language:

```text
The motion field says the environment would prefer visual/material lateral influence here.
The obstacle routing field says this area has local obstacle-driven routing intent.
The support field says foam should survive or appear more strongly here.
```

Incorrect language:

```text
The motion field moved this foam cell.
The support field created persistent foam by itself.
The visual sheet field caused Stage 1 material to merge.
```

Only Layer C can move persistent material. Only Layer D can create visual-only broad film shape.

## 3.9 Unified Foam velocity contract

Patch `4.11C.5.16A` makes resolved Foam velocity the canonical Layer B motion output.

Current raw inputs:

```text
Motion Lane texture: signed lateral route preference; sampled at a physically advected X phase.
Obstacle Routing texture R: signed route-around-obstacle preference; fixed in river space.
Obstacle Routing texture G: obstacle influence / collision-shadow strength; fixed in river space.
River Flow Speed × Liquid Factor × Downstream Speed Ratio: base Foam speed.
```

Canonical pure resolver:

```text
lateralIntent = clamp(
    lerp(laneIntent, obstacleIntent, obstacleInfluence),
    -1,
    1);

slowdown = saturate(
    obstacleInfluence * ObstacleSlowdownStrength);

downstreamFactor = lerp(
    1,
    ObstacleMinimumDownstreamFactor,
    slowdown);

vDownstream = max(0, baseFoamSpeed * downstreamFactor);
vLateral = lateralIntent * baseFoamSpeed * MaximumLateralSpeedRatio;
```

Invariant:

```text
vDownstream >= 0
```

Therefore the final movement system may move material left or right, slow it, or temporarily stop downstream movement, but it may never move material upstream.

The shared implementation lives in:

```text
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoamVelocity.hlsl
```

Compute raw-field sampling lives in:

```text
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Motion.hlsl
```

The existing Motion Field debug modes use the same pure contract. Hue encodes route meaning: neutral gray is straight motion, red/blue encode signed lateral velocity, and yellow indicates obstacle influence. Brightness is applied only after hue composition and represents `downstreamSpeedFactor`: bright is full-speed, dark is slowed, and near-black is near-stagnation. White uses the shared meaningful-Presence visibility gate derived from committed material. Raw Material Presence remains available in its dedicated amplitude-faithful view.

Motion Lane authoring now has independent shape controls:

```text
Direction Change Frequency:
  controls how often left/right intent changes sign downstream;

Across-River Coherence:
  controls how broadly neighbouring lateral rows share an instruction;

Low Lateral Motion Coverage:
  compresses a selected fraction of the field toward low lateral magnitude;

Lane Advection Ratio:
  controls how quickly the authored route pattern moves downstream in sample space.
```

At defaults `Direction Change Frequency = 1` and `Across-River Coherence = 1`, the generated field preserves the pre-split baseline. Higher direction frequency changes every downstream octave, breaker, cross-cut, and warp frequency without increasing across-river frequency. Higher coherence lowers across-river noise frequency while the existing two-pass across-width smoothing remains the final anti-checkerboard guarantee.

Lane phase is now advanced in physical metres:

```text
laneScrollMetres = baseFoamSpeed × LaneAdvectionRatio × deltaTime
laneScrollCells = laneScrollMetres / longitudinalCellSpacing
```

The old wraps-per-second formula, whose physical speed scaled with total river length, is retired.

---

# 4. Layer C — Persistent Foam Material

## 4.1 Abstract responsibility

Layer C is the durable Foam simulation.

It answers:

```text
Where does actual foam material exist?
How old is it?
What stable material pattern does it carry?
How does it move downstream?
Where is it born, preserved, clipped, or killed?
```

Layer C is the only writer of Persistent Foam State.

## 4.2 Current relevant code

Current and related code paths include:

```text
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.*.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Simulation.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Sampling.hlsl
```

Important current kernels include:

```text
InjectFoam
CommitPhaseTransport
SimulateFoam
ApplyBoundary
ClearRange
```

Important current texture/symbol names include:

```text
_FoamStateRead
_FoamStateWrite
currentState
previousState
writeState
DispatchPhaseCommit(...)
DispatchSimulationRange(...)
DispatchApplyBoundary(...)
FoamDecodeMaterialState(...)
FoamEncodeMaterialState(...)
FoamMergeBornPresence(...)
FoamClipPackedToValidFluid(...)
FoamApplyPersistentMaterialMorph(...)
```

## 4.3 Persistent state packing

Canonical packed state:

```text
R = Presence
G = Presence × normalized Remaining Life
B = Presence × normalized Material Pattern
A = reserved / future use
```

Decoded state:

```text
struct FoamMaterialState
{
    float presence;
    float remainingLife;
    float materialPattern;
};
```

## 4.4 Meaning of fields

### Presence

Persistent material coverage in a foam simulation cell.

It is not:

```text
final opacity
visual support
topology pressure
shader streak strength
shape-mask brightness
```

### Remaining Life

The actual durable survival clock.

Only Layer C may change it.

Layer D and Layer E may read it as metadata for visual fragility, but they must not write or reinterpret it as a visual mask owner.

### Material Pattern

Stable material identity/pattern data that travels with persistent foam.

Layer D and Layer E may use it for deterministic procedural variation.

## 4.5 Allowed reads

Layer C may read:

```text
previous Persistent Foam State
Layer A River Domain data
Layer B External Influence Fields
time/update delta
source injection/birth events
```

Layer C must not read:

```text
_FoamShapeMask
Layer D visual helper textures
shader local noise result
final rendered pixels
```

## 4.6 Owned behavior

Layer C owns:

```text
foam birth/source-to-persistent merge
persistent presence
downstream material transport
future real lateral material transport if approved
future real obstacle-guided material transport if approved
Remaining Life aging
support/negative aging response
valid-fluid clipping
obstacle/solid exclusion clipping
real material merge rules if added later
```

Layer C must not own:

```text
temporary visual chipping
temporary visual bending
temporary visual bridge/pinch behavior
shader-local streaks
final color/opacity
large hidden neighbour-sampled morphology that writes back to FoamState
```

## 4.7 Source population contract

Source population is Layer C birth preparation. It may read Layer A/B context, choose where real material should be born, and queue birth through the same persistent material injection path used by manual sources.

Source population must obey this rule:

```text
support/context may choose birth candidates;
only Layer C birth creates material;
support/context must not render as foam by itself.
```

The intended route for shore, rock, wake, and current-seam foam is therefore:

```text
Layer B environmental support/contact/wake context
  -> Layer C automatic source population creates real FoamState material
  -> Layer C support/negative aging captures or kills that material
  -> Layer D derives broad visual film from the material
  -> Layer E adds pixel-scale breakup/streaks/polish
```

This replaces the earlier temptation to add a separate visual-only environmental film authority. Such a visual-only product is postponed and should not be introduced until source population has been tested and found insufficient.

Patch `4.11C.5.14A` added the first automatic source class: conservative shore/contact birth. Validation proved the plumbing but showed the first control design was too crude. Patches `5.14B–5.14H` established source-class-specific authoring and a dedicated typed source-event rasterizer. `5.15A–5.15A.4` added static object/contact arcs, semi-arcs, and flecks. `5.15B–5.15B.3.1` added free-water lace connectors, cross-lace connectors, and progressively revealed torn fragments. These source families are not final-quality, but they now provide sufficiently varied real `FoamState` material to unblock evolution work. Spawning is parked unless a concrete regression blocks evolution validation.

## 4.8 Current status

Active/trusted:

```text
manual/source birth
automatic shore birth
automatic static object/contact birth
automatic free-water lace/cross-lace/fragment birth
source-to-persistent merge
conservative local 2D Layer C material transport
lifecycle aging
support/negative aging influence
valid-fluid and obstacle clipping
unified physical Foam velocity consumed by Layer C and Layer D
half-resolution advected Layer D temporal occupancy
```

Rejected/superseded:

```text
global downstream phase transport
persistent stored-state morph as visual breakup
fractional lateral row weighting
per-cell stochastic lateral row commit
hidden neighbour-resampling morphology that writes persistent state
```

Actual lateral material transport is active in Layer C. Temporal occupancy may move visual sheet structure in Layer D, but it remains a separate visual product and must never be described as additional durable material.

## 4.9 Connectivity invariant

Layer C is the only layer that can truthfully say:

```text
actual foam material moved
actual foam material was born
actual foam material died
actual Remaining Life changed
```

If Layer D displays foam slightly offset from the material, that is visual interpretation only. It must never be described as actual material movement.

---

# 5. Layer D — Visual Foam / Film Evaluation

## 5.1 Abstract responsibility

Layer D is runtime GPU compute that derives visible broad foam/film shape from upstream data.

It answers:

```text
Given persistent foam material plus river/support/motion context,
what should the broad visible foam film look like right now?
```

Layer D is the only writer of Evaluated Foam Shape products.

## 5.2 Current relevant code

Current and related code paths include:

```text
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Compute.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Binding.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Sampling.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Motion.hlsl
```

Current important symbols:

```text
EvaluateFoamShape kernel
DispatchEvaluateShape()
evaluateShapeKernel
shapeMaskTexture
_FoamShapeMaskWrite
_FoamShapeMask
FoamEvaluatedShape = 7
FoamShapeDifference = 8
IsShapeProductDebugActive
FoamEvaluateIntrinsicShapeMask(...)
BindField()
```

## 5.3 Current implementation state after 4.11C.5.10B

The current code has a Layer D output slot, `_FoamShapeMask`, and two Layer D debug views:

```text
Foam Evaluated Shape = displays _FoamShapeMask directly.
Foam Shape Difference = compares _FoamShapeMask against raw persistent Presence.
```

After validation of 5.10, the 5.9z coordinate-warp prototype was retired and `EvaluateFoamShape` was reset to a clean pass-through baseline:

```text
float FoamEvaluateIntrinsicShapeMask(
    FoamMaterialState material,
    float validFluid)
{
    return saturate(material.presence * validFluid);
}

_FoamShapeMaskWrite[coordinate] =
    FoamEvaluateIntrinsicShapeMask(material, validFluid);
```

The 5.10 validation screenshots showed clear green/magenta signed values in `Foam Shape Difference`, proving the 5.9z product was numerically changing `_FoamShapeMask`. However, `Material Presence` and `Foam Evaluated Shape` still looked and behaved basically identical in normal mask display. The conclusion is precise:

```text
5.9z did work at the value/difference level.
5.9z did not work at the visible structural-shape level.
```

The failure reason remains architectural, not merely amplitude tuning:

```text
- one-to-two-cell coordinate displacement affects mostly contours;
- broad solid masks remain broad and solid after nearby sampling;
- blend-to-base damped visible differences;
- the operation redistributed coverage inside the same overall ribbon/blob;
- coordinate warp cannot create visual bridge/pinch/sheet/contact support by itself.
```

Therefore 5.9z is no longer present as active code in Layer D. Its lesson is retained in documentation as a rejected/superseded prototype. Layer D now starts from a truthful baseline where `Foam Shape Difference` should be mostly black until a new Layer D component is intentionally added.

`DispatchEvaluateShape()` remains gated behind Layer D debug use because Final Foam still does not consume `_FoamShapeMask`.

## 5.3.1 4.11C.5.10 compliance audit result

The first source audit after the architecture lock found the current code broadly compatible with the acyclic Layer A-F graph:

```text
Layer B external influence generation was not found reading FoamState or _FoamShapeMask.
Layer C persistent material kernels were not found reading _FoamShapeMask or Layer D helper products.
Layer D writes _FoamShapeMask only, not FoamState.
Layer E debug/final paths render pixels only and do not feed compute.
Final Foam remains disconnected from _FoamShapeMask.
```

Cleanups made from that audit:

```text
Added Foam Shape Difference debug.
Corrected stale Foam Evaluated Shape descriptions.
Corrected Water Body help text so persistent Foam is described as downstream material transport, not active lateral disturbance transport.
Removed unused wake/pressure disturbance-transport constants left from abandoned material-motion experiments.
Gated DispatchEvaluateShape to Layer D debug use until Final Foam actually consumes the product.
```

Known non-urgent caveats:

```text
The transition-hold fallback may still bind persistent state where the shape mask is expected; evaluated-shape debug during topology transition should be treated cautiously until a dedicated transition ShapeMask snapshot exists.
Shader-side Final Foam still owns legacy macro shaping until the accepted Layer D film/shape product replaces it.
Low-res Layer D Film Source and Film Support helpers exist after `4.11C.5.13`; coordinate-space and support-source semantics were corrected and validated through `4.11C.5.13B` and `4.11C.5.13C`, and spread was tuned in `4.11C.5.13D`. The latest architectural correction is that Layer D should not be asked to invent shore/rock/contact film from a single central manual ribbon. Source placement belongs in Layer C source population first; Layer D then spreads material-derived products.
```

## 5.3.2 4.11C.5.10B validation response and reset

The first validation after `Foam Shape Difference` showed the exact problem:

```text
Foam Shape Difference: clearly non-black, with green/magenta bands.
Material Presence: visually broad white ribbon/blob.
Foam Evaluated Shape: visually broad white ribbon/blob, effectively the same silhouette and behavior.
Final Foam: unchanged, as intended.
```

Interpretation:

```text
The 5.9z coordinate warp changed values but not useful visible structure.
A debug difference view can look dramatic while the actual shape remains player-useless.
Future Layer D work must prove visible structural benefit, not just nonzero numeric difference.
```

5.10B therefore removes the warp helpers and Motion Field/routing bindings from `DispatchEvaluateShape()`. The pass-through baseline is deliberately boring so future probes have a clean comparison target:

```text
Material Presence ~= Foam Evaluated Shape
Foam Shape Difference ~= black
```

4.11C.5.11 tested a deliberately isolated local procedural breakup probe on top of this clean baseline. Validation proved the probe was active, but it produced cell/ribbon-shaped removals because `_FoamShapeMask` is too coarse for atomic fine breakup. 4.11C.5.11B retires that probe and returns Layer D to pass-through. Fine breakup now belongs in Layer E shader composition; Layer D remains the future macro film-structure layer.

## 5.4 Allowed reads

Layer D may read:

```text
Layer A River Domain data
Layer B External Influence Fields
Layer C Persistent Foam State
time
read-only previous Layer D temporal occupancy
```

Layer D may not read:

```text
final shader output
screen-space result
any future downstream product that would create a cycle
```

## 5.5 Owned products

Current product:

```text
_FoamShapeMask
R = evaluated broad visible foam/film mask
```

Current helper products:

```text
_FoamFilmSource
_FoamFilmSupport
_FoamVisualOccupancyA / _FoamVisualOccupancyB
```

Future possible products, if justified:

```text
_FoamDamageMask
_FoamBreakMask
_FoamEdgeMask
```

Any foam-derived helper field belongs here, not in Layer B.

## 5.6 Allowed behavior

Layer D may visually:

```text
widen foam
connect nearby foam
bridge small gaps
pinch weak links
soften contours
bend/ripple broad film using motion fields
increase old-foam fragility based on Remaining Life
use contact/support fields to create bank/rock film support
use low-res helper fields for broad sheet behavior
advect temporal visual occupancy so broad sheets can persist, pinch, tear, split, and rejoin over time
```

## 5.7 Forbidden behavior

Layer D must not:

```text
write _FoamStateWrite
modify Presence
modify Remaining Life
modify Material Pattern
move durable material
spawn durable material
kill durable material
feed back into Layer B
feed back into Layer C
track pocket IDs
own connected-component identity
hide broken Stage C transport with visual-only macro movement
```

## 5.8 Correct meaning of visual offset

Incorrect:

```text
Stage D moved this foam cell right.
```

Correct:

```text
Stage D displayed the broad visible film slightly right of durable material, within bounded visual-shape rules.
```

Layer D may lie visually. It may not corrupt material truth.

## 5.9 Required internal structure

Layer D should become a small fixed-grid pipeline, not one all-powerful pass.

### D1 — Visual Film Source

Build a low-res material-derived source field. This field may read upstream support/contact data as bias or suppression, but support must not create Film Source by itself.

Inputs:

```text
Persistent Presence
Remaining Life
Material Pattern
valid fluid
exclusion
bank/contact support as bias only
rock/contact support as bias only
wake/pressure/ripple support as bias only
motion intent as future bias only
negative suppression
```

Output:

```text
_FoamFilmSourceHalf
```

Meaning:

```text
Where does material-derived broad visible film begin?
```

This is not persistent material. It is also not raw Layer B support. Persistent material creates this source; Layer B support/contact fields can only bias or suppress it.

### D2 — Visual Sheet Support

Create broad field support for reference-like sheets/ribbons.

Inputs:

```text
_FoamFilmSourceHalf
Layer B visual/contact support
flow direction / river basis
optional disturbance agitation
```

Operations should be cheap field operations such as:

```text
directional spread along flow
weaker spread across flow
separable blur/spread
small-gap closing approximation
thresholded sheet support
contact/bank support expansion
```

Output:

```text
_FoamFilmSupportHalf
```

Meaning:

```text
Where can broad visible surface film structurally exist?
```

### D3 — Full-resolution Evaluated Shape

Create `_FoamShapeMask`.

Inputs:

```text
Persistent Foam State
_FoamFilmSourceHalf
_FoamFilmSupportHalf
valid fluid
obstacle exclusion
Remaining Life
Material Pattern
Motion Field / obstacle routing
optional local procedural breakup seed data
```

Output:

```text
_FoamShapeMask
```

This pass combines durable material truth with broad visual support, while preserving the rule that only Layer C owns real material.

### D4 — Advected Temporal Visual Occupancy

Implemented in `4.11C.5.16C` as the structural moving visual sheet used by Final Foam integration and shader-local detail.

Inputs:

```text
previous half-resolution temporal occupancy
current Film Source
current Film Support
canonical local 2D velocity
valid fluid and obstacle exclusion
Layer C CFL substep count
```

Output:

```text
current half-resolution temporal occupancy
```

Rules:

```text
history is transported through the same face-velocity and closed-boundary contract as Layer C;
history builds quickly toward newly supported film and releases more slowly when support disappears;
history may preserve a visual sheet, bridge, pinch, or tear over time;
history must not feed Layer C or Layer B;
history must not become material lifetime or material quantity;
Final Foam remains disconnected until the coordinate-consistent evaluated-shape A/B path is accepted.
```

## 5.10 Local-only breakup limit

A local function can produce convincing chipping and semi-organized chaos when it runs at the right visual scale:

```text
visible = broadMask * proceduralPattern(position, riverUV, time, life, materialPattern)
```

This can create:

```text
edge chipping
animated breakup
small cuts
apparent fragments
thin cracks
life-based fragility
```

But a purely local function cannot reliably bridge based on nearby foam, because an empty cell between two foam patches and an empty cell in open water have identical local foam presence.

Therefore:

```text
local procedural math = decorative breakup/detail
low-res support field = structural bridge/sheet/contact behavior
```

The `4.11C.5.11` validation adds a second, stricter lesson:

```text
local procedural breakup should not be baked into _FoamShapeMask when the desired detail is finer than a foam field cell.
```

Layer D writes a foam-field texture. Removing cells inside `_FoamShapeMask` exposes simulation-cell scale and creates long ribbon/cell-shaped holes. The inspiration reference's fine breakup is much more granular, closer to rendered-pixel/sub-cell detail.

Corrected ownership:

```text
Layer D owns macro visual structure: broad film, sheet support, bridge/pinch/split, bank/rock/contact film, smooth mask foundation.
Layer E owns micro visual detail: granular edge breakup, tiny cuts, thin streaks, highlight scratches, final polish.
```

## 5.11 Layer D local procedural breakup probe — validation and rejection

`4.11C.5.11` implemented the first cheap local-only Layer D breakup test. Its purpose was to test the best-case version of the no-neighbour "magical" approach before paying for low-resolution structural support fields.

Implemented code path in 5.11:

```text
CS_RiverFoam.compute
  EvaluateFoamShape(...)
    FoamClipPackedToValidFluid(...)
    FoamDecodeMaterialState(...)
    FoamEvaluateLocalProceduralBreakupShape(...)
      FoamResolveMaterialPhysicalPosition(...)
      FoamEvaluateLocalBreakupField(...)
      FoamSourceFillValueNoise(...) / EvaluateFoamSourceFillField(...)
    _FoamShapeMaskWrite[coordinate] = result

StylizedRiverFoamRuntime.Compute.cs
  DispatchEvaluateShape()
    bound _FoamBoundary
    bound _FoamObstacleExclusionRead
    bound _FoamStateRead
    bound _FoamShapeMaskWrite
    bound _FoamTime / _FoamSeed
    bound _FoamGlobalStart / _FoamFieldLength
    bound _FoamMetricRows
```

The probe correctly obeyed the dependency rules:

```text
no neighbouring FoamState sampling
no Motion Field lane
no Obstacle Routing field
no Topology support fields
no low-res Film Source / Film Support
no Final Foam shader mask
no entity or pocket identity
no persistent FoamState mutation
```

Validation result:

```text
Foam Shape Difference became clearly non-black, mostly magenta/removal.
The removals were long cell/ribbon-shaped gaps.
The result exposed _FoamShapeMask cell scale.
It did not resemble the granular, almost atomic breakup in the inspiration river.
Final Foam remained unchanged, as intended.
```

Conclusion:

```text
5.11 proved that Layer D local-only breakup can produce difference values, but it is rejected as the fine-fragmentation solution.
The issue is layer/resolution mismatch, not inactivity.
Do not tune this Layer D breakup probe further.
Fine fragmentation must be tested in Layer E shader composition at rendered-pixel scale.
Layer D should stay focused on macro film structure.
```

`4.11C.5.11B` retires the 5.11 probe as active code. The baseline shape path is again:

```text
CS_RiverFoam.compute
  EvaluateFoamShape(...)
    FoamClipPackedToValidFluid(...)
    FoamDecodeMaterialState(...)
    FoamEvaluateIntrinsicShapeMask(...)
    _FoamShapeMaskWrite[coordinate] = result

StylizedRiverFoamRuntime.Compute.cs
  DispatchEvaluateShape()
    binds _FoamBoundary
    binds _FoamObstacleExclusionRead
    binds _FoamStateRead
    binds _FoamShapeMaskWrite
```

Expected baseline after 5.11B:

```text
Material Presence ~= Foam Evaluated Shape
Foam Shape Difference = black or effectively black
Final Foam unchanged
```

## Layer D structural performance target

For a High 32 m chunk:

```text
Full field: 128×128 = 16,384 cells
Half field: 64×64 = 4,096 cells
```

Proposed core Layer D cost target:

```text
D1 Film Source:     ~32k reads/update,  ~4k writes/update
D2 Sheet Support:   ~40k–60k reads/update, ~8k writes/update
D3 Full Shape:      ~100k–150k reads/update, ~16k writes/update
Total:              ~175k–240k reads/update/chunk, ~28k writes/update/chunk
```

Recommended update rates:

```text
Low:    8 Hz
Medium: 12–16 Hz
High:   16–24 Hz
```

Avoid default full-res wide neighbourhood classifiers. Radius 1/3/5 box sampling costs:

```text
3×3 + 7×7 + 11×11 = 9 + 49 + 121 = 179 samples/cell
128×128×179 ≈ 2.93M samples/update/chunk
```

That is not the default architecture.

---

# 6. Layer E — Shader Composition

## 6.1 Abstract responsibility

Layer E is the water render shader. It turns upstream textures into final pixels.

It answers:

```text
How should this water pixel look right now?
```

It does not own simulation or broad structure.

## 6.2 Current relevant code

Current and related code paths include:

```text
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl
```

Important current symbols include:

```text
_FoamShapeMask
_FoamDebugView
_FoamMotionLane
_FoamObstacleRouting
RiverWaterEvaluateFoam(...)
RiverWaterFoamResult
RiverWaterFoamResult.materialUV
foam.presence
foam.remainingLife
foam.mask
Foam Evaluated Shape debug branch
Foam Motion Field debug branch
Foam Motion Field + Cell Grid debug branch
```

## 6.3 Allowed reads

Layer E may read:

```text
Layer A coordinate/material UV data
Layer B influence fields if needed for debug or local polish
Layer C Persistent Foam State if needed
Layer D _FoamShapeMask and visual helper products
local procedural noise
time
```

Layer E writes:

```text
screen pixels only
```

Layer E must not feed back into any compute texture or simulation state.

## 6.4 Owned behavior

Layer E owns:

```text
final foam color
opacity
edge softness
small local chipping
thin bright streaks
sparkle/highlights
reflection/refraction blending
water lighting/composition
debug visualization
```

Layer E should not own:

```text
broad sheet creation
macro split/join decisions
bank/rock film support as structure
wide neighbourhood bridge logic
persistent material movement
```

## 6.5 Broad structure vs local detail rule

Use this rule:

```text
If the effect needs context/nearby foam/support, do it in Layer D compute.
If the effect is local per-pixel polish, do it in Layer E shader.
```

More precise correction:

```text
Decorative breakup should try local procedural math first.
Structural film connectivity should use cheap Layer D field support.
```

This prevents the shader from doing expensive wide neighbourhood searches per visible screen pixel, while still allowing rich local chaos cheaply.

## 6.6 Final Foam switch rule

Final Foam must not consume `_FoamShapeMask` as the production shape until Layer D is visibly better than current final foam.

Before the switch, debug views compare:

```text
Material Presence
Foam Evaluated Shape
Foam Shape Difference
```

After the switch, shader-side legacy macro foam shaping should be reduced or demoted so there is only one broad-structure authority.

---

# 7. Layer F — Scheduling, Quality, Debug

## 7.1 Abstract responsibility

Layer F is orchestration. It controls when layers update, which textures are allocated, what debug view is shown, and which quality tier is active.

It does not own foam behavior.

## 7.2 Current relevant code

Current and related code paths include:

```text
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.*.cs
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Authoring.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Disturbances.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Diagnostics.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.UI.cs
```

Important current symbols include:

```text
StylizedRiverFoamDebugView
ResolveFoamDebugView(...)
IsMotionFieldDebugActive(...)
BindField()
BindDisabled()
DispatchEvaluateShape()
EnsureResources(...)
shapeMaskTexture
motionLaneTexture
obstacleRoutingTexture
FoamEvaluatedShape = 7
```

## 7.3 Owned behavior

Layer F owns:

```text
update cadence
quality tiers
active chunk culling/freezing
debug view selection
texture allocation/release
compute kernel lookup
compute texture binding
Inspector display and labels
profiling marker placement
```

Layer F must not own:

```text
foam birth logic
foam material motion math
visual bridge/break math
shader breakup math
```

## 7.4 Recommended quality/update targets

For broad foam/film:

```text
Layer C Persistent Material:
Low    ~8 Hz
Medium ~12 Hz
High   ~16 Hz

Layer D Visual Film:
Low    ~8 Hz helper/support, 8–16 Hz shape
Medium ~12–16 Hz helper/support, 16–24 Hz shape
High   ~16–24 Hz helper/support, 24 Hz shape unless profiling proves 60 is cheap

Layer E Shader:
Every rendered frame, local-only detail.
```

Distant, frozen, or offscreen chunks should reduce or skip Layer C and Layer D updates where safe.

## 7.5 Required debug views

Existing useful debug views:

```text
Final Foam
Foam + Aging Topology
Automatic Birth Sources
Material Presence
Material Remaining Life
Foam Motion Field
Foam Motion Field + Cell Grid
Foam Evaluated Shape
Foam Shape Difference
```

Additional existing debug views:

```text
Foam Film Source
Foam Film Support
```

Each debug view must explicitly state what product it displays:

```text
Persistent material truth
External influence field
Layer D visual helper
Evaluated shape
Final shader output
```

Do not reuse a final-render mask in a raw material debug view.

## 7.6 Accepted Inspector and diagnostics contract

The accepted River tooling contract is:

```text
all top-level and nested foldouts
  collapsed by default;

production authoring
  grouped by River feature;

Foam production authoring
  grouped by Layers A–E plus Runtime & Quality;

Debug Views
  one central exclusive controller over the existing serialized debug enums;

Runtime Diagnostics
  labelled, selectable, read-only, stable-height rows;

Generated Status
  read-only generated and compatibility results;

Actions
  all generation, validation, cache, test-source, probe, clear, and reset operations.
```

The debug hub preserves the existing runtime enum fields and shader behavior. Selecting a new non-Final view clears the other debug systems. Existing scenes with multiple active debug fields are reported without silent modification; the rendered result follows the established shader priority:

```text
Foam > Disturbances > Refraction > Surface Motion > Water Body
```

`Normalize to Rendered View` preserves the winning view and clears hidden lower-priority selections. `Reset All Debug Views` returns every system to Final.

Runtime diagnostics must not own or mutate Foam behavior. Diagnostic rows remain present while values transition through Edit Mode, unavailable runtime, pending asynchronous readback, and live states. Ordinary authoring, Debug Views, Generated Status, and Actions do not request constant repaint. Constant repaint is permitted only in Play Mode, with one selected River, while a visible live Disturbance or Foam diagnostic leaf requires updates.

The planned Presence capacity-loss attribution belongs only at:

```text
Runtime Diagnostics
  Foam
    Layer C — Persistent Material
      Transport Accounting
```

It must not restore ad-hoc warning boxes, duplicate selectors, or editable controls inside diagnostics.

---

# 8. Canonical connectivity table

| Layer | May read | Must not read | Writes | Consumers |
|---|---|---|---|---|
| A — River Domain | River geometry, spline/domain, corridor/terrain geometry as needed | FoamState, ShapeMask, shader output | domain snapshots, boundary/coverage, coordinate mapping | B, C, D, E |
| B — External Influence | A, obstacles, banks, emitters, time, own previous influence history | FoamState, ShapeMask, D helper fields, shader output | topology/support/exclusion/motion/wake/pressure influence fields | C, D, E/debug |
| C — Persistent Material | A, B, previous FoamState, source events, time | ShapeMask, D helpers, shader output | FoamState | D, E/debug |
| D — Visual Foam/Film | A, B, C, time, optional previous visual shape | shader output, future downstream products | ShapeMask, visual helper fields | E |
| E — Shader Composition | A, B, C, D, local noise, time | nothing downstream; no feedback | final pixels | screen only |
| F — Scheduling/Debug | settings, runtime state, visibility | behavior internals as authority | dispatch/binding/debug decisions | all layers indirectly |

If a proposed feature violates this table, stop and redesign before coding.

---

# 9. Conflict examples and resolutions

## 9.1 “Layer C says left, Layer D says right”

This is only a contradiction if both layers claim to move material.

Canonical resolution:

```text
Layer C owns material movement.
Layer D owns visual interpretation only.
```

If Layer D's visible offset fights the material so strongly that foam appears to move the wrong way, the fix is to bound or retune Layer D. Do not let Layer D write material state.

## 9.2 “Motion Field moves foam”

Incorrect.

Canonical language:

```text
Motion Field is an external influence/input field.
Layer C may use it later for approved real material transport.
Layer D may use it for visible film deformation.
The Motion Field itself does not move foam.
```

## 9.3 “Support created foam”

Incorrect unless Layer C used support during an approved birth rule.

Canonical language:

```text
Support encourages birth/survival/visual film.
Persistent material only exists if Layer C writes it.
Visual film only appears if Layer D writes it.
```

## 9.4 “Stage D bridge is a material merge”

Incorrect.

Canonical language:

```text
Layer D bridge is visual-only surface film connectivity.
Real material merge, if ever needed, belongs to Layer C.
```

## 9.5 “Shader breakup split the foam”

Incorrect.

Canonical language:

```text
Shader breakup split the rendered appearance at this pixel.
It did not split Persistent Foam State.
```

---

# 10. Accepted and rejected techniques

## 10.1 Accepted primary techniques

Accepted as architectural direction:

```text
fixed-grid field math
read/write ownership per layer
External Influence Fields upstream of Persistent Material and Visual Film
Persistent Foam State as durable material truth
Visual Foam/Film Evaluation as broad structural interpretation
shader-side local procedural breakup and thin streaks
low-res Layer D helper fields for sheet support/bridging
bounded visual-only offsets
quality-tiered update cadence
explicit debug views for every product
```

## 10.2 Rejected as primary techniques

Rejected/superseded as primary architecture:

```text
foam pocket IDs
connected-component foam islands
per-pocket entity database
persistent stored-state morph as visual breakup
neighbour-resampled morphology that writes FoamState
fractional lateral row weighting
per-cell stochastic lateral row commit
dense interior hole cutting as the main look
5.9z coordinate warp as the final shape solution
naive full-res 179-sample wide-neighbour classifiers as default
shader-side wide-neighbour structural foam search
using final shader masks as raw material debug truth
```

## 10.3 Techniques allowed only with caution

Allowed but not as first resort:

```text
mip/pyramid helper fields
jump-flood/distance fields
real material merge rules beyond the existing source merge contract
additional temporal damage/history products beyond the accepted occupancy pair
```

These require separate approved plans and validation.

---

# 11. Performance model

## 11.1 Resolution assumptions

Typical per 32 m river chunk:

```text
Low:    full 64×64  = 4,096 cells;  half 32×32 = 1,024 cells
Medium: full 96×96  = 9,216 cells;  half 48×48 = 2,304 cells
High:   full 128×128 = 16,384 cells; half 64×64 = 4,096 cells
```

## 11.2 Proposed Layer D cost target

For High, per 32 m chunk:

```text
D1 Film Source:
~32k reads/update
~4k writes/update

D2 Sheet Support:
~40k–60k reads/update
~8k writes/update

D3 Full Shape:
~100k–150k reads/update
~16k writes/update

Core total:
~175k–240k reads/update/chunk
~28k writes/update/chunk
```

At 16 Hz:

```text
~3–4M reads/sec/chunk
```

For 3 active High chunks:

```text
~9–12M reads/sec
```

This is acceptable as a target for low-end desktop-class GPUs if shader work remains local and chunk/update scheduling is respected.

## 11.3 Why not shader-side structural search

At 1080p, if water covers 25% of the screen:

```text
1920×1080×0.25 ≈ 518k water pixels
```

A shader-side 8-sample structural neighbourhood effect at 60 FPS costs roughly:

```text
518k×8×60 ≈ 249M samples/sec
```

This scales with screen coverage and frame rate. Broad structure should therefore be computed into compact fields, not rediscovered per rendered pixel.

## 11.4 Why not naive wide full-res classifiers

A radius 1/3/5 full-res classifier costs:

```text
3×3 + 7×7 + 11×11 = 179 samples/cell
128×128×179 ≈ 2.93M samples/update/chunk
```

At 60 FPS:

```text
~176M samples/sec/chunk
```

That is too expensive as a default and still not as clean as low-res sheet support for broad film.

## 11.5 Memory target

High chunk approximate additional Layer D memory:

```text
_FoamShapeMask RHalf 128×128 ≈ 32 KB
Two half-res RHalf helpers 64×64 ≈ 16 KB total
Optional previous shape RHalf ≈ 32 KB
```

Expected added memory:

```text
~16–48 KB/chunk without richer RG helpers
```

This is a reasonable memory trade for lower runtime cost.

---

# 12. Implementation roadmap

## Phase 1 — Documentation lock

Status: this document.

Purpose:

```text
make dependencies, ownership, rejected paths, and target layers canonical
```

## Phase 2 — Compliance/debug visibility

Status after 4.11C.5.13C: complete for current Layer D debug/product pipeline.

Completed:

```text
Foam Shape Difference debug.
A/B/C/D/E/F source-level compliance audit.
Stale editor/help text cleanup.
Layer D dispatch gating while Final Foam does not consume _FoamShapeMask.
```

Completed later by `4.11C.5.13` and follow-up corrections:

```text
Foam Film Source debug.
Foam Film Support debug.
Domain-space Layer D sampling fix in 5.13B.
Material-gated Film Source semantic fix in 5.13C.
Verification that new Layer D helpers write only Layer D products and do not feed Layer B or C.
```

## Phase 3 — Layer E shader-side local detail probe

Status: implemented and validated as a technical proof in `4.11C.5.12`.

Purpose:

```text
determine how much reference-like fine chaos can be achieved with local shader math before/alongside the structural Layer D film-support system
```

Implemented diagnostic scope:

```text
Foam Chip And Strand Probe debug view
Foam Chip And Strand Difference debug view
sub-cell granular edge breakup at rendered-pixel scale
no neighbourhood search
no persistent mutation
no _FoamShapeMask mutation
no broad bridge support
no Final Foam change
```

Validation rule:

```text
Accept this as Layer E detail only if the result reads as pixel/sub-cell edge detail and not as cell/ribbon holes, dirty static noise, or broad structural breakup.
```

## Phase 4 — Low-res Visual Film Source and Sheet Support

Add Layer D low-res helper textures.

Scope:

```text
_FoamFilmSourceHalf
_FoamFilmSupportHalf
fixed-tap/separable directional spread
small-gap visual bridging support
bank/rock/contact film support
flow-aware sheet elongation
```

Purpose:

```text
create broad film/sheet behavior that local noise cannot know
```

## Phase 5 — Advected Temporal Visual Occupancy

Status: accepted through `4.11C.5.16C.1`.

Scope:

```text
two half-resolution RHalf ping-pong textures;
canonical-velocity donor-cell advection;
closed shore/obstacle/invalid/lateral exterior faces;
flow-aware endpoint outflow;
asymmetric exponential build/release toward Film Source + Film Support;
full-resolution _FoamShapeMask combines Presence and temporal occupancy;
no feedback into Layer C;
debug-gated while Final Foam remains disconnected.
```

Accepted evidence:

```text
occupancy follows the Layer C route;
stationary target/occupancy difference converges to black;
no permanent coordinate offset was found;
comparative material diagnostics use a shared meaningful-Presence gate;
Final Foam remains unchanged.
```

## Phase 6 — Occupancy-Native Macro Breakup Experiment

Status: rejected and retired through `4.11C.5.16D.R`.

The zero-extra-field experiment attempted to infer strain, weakness, connected cut evidence, slow healing, and local shape authority directly from temporal occupancy. It added no textures, channels, buffers, kernels, or dispatches, but Unity validation did not produce convincing macro tears. It mainly suppressed weak film and exaggerated existing lane transitions.

Permanent rule:

```text
do not restore 5.16D–5.16D.2 breakup code;
do not add a persistent damage field or packed damage channel;
use emergent lateral advection/temporal occupancy for macro separation;
use the accepted Layer E Chipping and structural Strands for visible breakup; no micro-tearing continuation is queued.
```

## Historical Phase 7 — Coordinate-Consistent Final Foam Integration

Status: not required by the accepted Final Foam result; retained as historical architecture only.

Scope:

```text
add a reversible A/B path in which Final Foam consumes _FoamShapeMask;
sample evaluated shape and material attributes through one consistent presentation coordinate contract;
avoid the rejected preview mismatch between committed shape and residual-shifted life/pattern;
preserve Layer C truth and all transport/lifetime ownership;
retain the legacy Final Foam path until the evaluated path is explicitly accepted.
```

Acceptance:

```text
no upstream-looking expiry illusion;
no double movement or obstacle snap-back;
evaluated broad film is visibly preferable to the legacy macro shape;
normal material lifetime, pattern, and spawning remain unchanged;
performance cost is measured and bounded.
```

## Historical Phase 8 — Shader-Local Micro-Tearing and Edge Erosion — retired

Scope:

```text
small edge notches and short cracks;
thin strand separation and chipped weak fringes;
age- and disturbance-sensitive local erosion;
rendered-pixel procedural math only;
zero new persistent textures, fields, channels, or compute dispatches.
```

## Historical Phase 9 — Thin bright streak and local polish layer — not queued

Scope:

```text
fast narrow white scratches/streaks;
glints, bubbles, edge lighting, and foam colour polish;
local/no-neighbour procedural math;
separate from broad film structure.
```

## Deferred comprehensive River performance pass

Formalize:

```text
resolution per quality tier;
Layer C update rate;
Layer D helper/shape update rate;
active chunk caps;
culling/freezing rules;
debug/profiling counters.
```

---

# 13. Current active conclusion

The active architecture is now:

```text
Layer C owns durable material birth, transport, lifetime, and death;
Layer D owns material-derived Film Source / Film Support;
Layer D owns the accepted advected temporal visual occupancy sheet;
Layer D does not own a separate persistent fracture or damage state;
_FoamShapeMask is the full-resolution diagnostic product;
Layer E owns the accepted analytical Chipping, structural Strands, colour, opacity, and lighting;
Final Foam consumes an ordinary fixed-step temporal blend of the previous/current committed Layer C states; `_FoamShapeMask` remains a diagnostic/evaluated product and has no queued production-integration patch.
```

`4.11C.5.16E.2` remains the accepted no-backtrace architecture. P12a adds only ordinary interpolation between the two already committed Layer C states so the 8/12/16 Hz material cadence is not exposed as hard edge changes; it does not reconstruct velocity or move the sample coordinate. Final Foam keeps both reversible visibility policies, the supported-aging minimum is `0.05`, and lifecycle aging is quantized once per complete material tick rather than once per CFL substep. `_FoamShapeMask` remains diagnostic-only.

The former visibility discrepancy is resolved diagnostically. Material Coverage and the Visibility Pipeline Composite proved that `Concentration + Lifetime` hid a broad low-Coverage footprint already present in Layer C. `Lifecycle-Faithful` exposes that state honestly; it did not create the blob. The active blocker is therefore Layer C transport compactness: under-resolved ribbons widen while their integrated packed material remains broadly conserved. D4 preserves Layer C ownership and tests two one-dispatch, zero-extra-field transport alternatives under an absolute no-regression performance ceiling.

The accepted R1–R5 Inspector redesign is the current Layer F tooling contract. `4.11C.5.16E.3 — Transport Presence Capacity-Loss Attribution Audit` is Unity-validated and accepted. It uses `Runtime Diagnostics > Foam > Layer C — Material & Lifecycle > Transport Accounting` and changes no transport, lifecycle, source, Layer D, or rendering behavior.

## `4.11C.5.16E.3` transport Presence attribution contract

The existing total Presence clamp/capacity loss remains the authoritative before-versus-stored transport measurement. The audit observes the same raw transported and final packed state and divides Presence loss sequentially into:

```text
raw positive Presence -> unit [0,1] capacity;
unit-limited Presence -> fractional boundary coverage;
boundary-limited Presence -> boundary × (1 - obstacle exclusion);
remaining capacity-limited Presence -> state-validity rejection;
remaining tiny Presence -> final minimum-state cutoff.
```

The audit additionally stores:

```text
maximum raw transported Presence;
maximum raw excess above local valid-fluid capacity;
any/unit/boundary/obstacle capacity-hit cell-substep counts;
signed CPU attribution residual against the existing total.
```

Counts are cell-substep samples, not unique cells. The same cell may contribute more than once when CFL safety requires multiple substeps. Loss values remain area-weighted fixed-point material units. Peaks use the same `4096` fixed-point scale without area weighting.

The metric buffer contract is:

```text
12 -> 23 raw uint entries;
48 -> 92 bytes;
+44 bytes per active Foam runtime.
```

The existing reset kernel, simulation kernel, capture cadence, and asynchronous readback are reused. New arithmetic and atomics execute only while `_FoamTransportMetricsEnabled != 0`. No new texture, persistent field, channel, kernel, dispatch, or readback is permitted.

The category values are fixed-point partitioned against the existing total whenever their float stages reconcile. This avoids manufacturing a residual solely through independent per-category rounding. If the float stages do not reconcile, categories are accumulated independently so the signed residual exposes the unmatched path rather than hiding it.

No limiter, rejected-inflow retention, redistribution, epsilon change, source change, or gate change is part of `5.16E.3`.

## `4.11C.5.16E.3C` accepted closure and deferral contract

Unity validation used only the corresponding automatic source class in each controlled case:

| Test | Total loss | Unit | Boundary | Obstacle | Peak raw | Peak local excess |
|---|---:|---:|---:|---:|---:|---:|
| Open water | 0.067% | 0.000% | 0.067% | 0.000% | 0.7727 | 0.0215 |
| Shoreline | 0.484% | 0.222% | 0.262% | 0.000% | 1.2046 | 0.2046 |
| Obstacle | 0.747% | 0.479% | 0.267% | 0.000% | 1.2246 | 0.2246 |

Every capture reconciled with zero signed residual and reported zero obstacle-capacity, state-validity, and minimum-cutoff loss. The object-source case therefore compresses Foam into neighbouring valid cells; it does not lose Foam by storing it inside the obstacle footprint. Boundary and unit-capacity losses are both real, while unconstrained open-water transport remains below the original target in the tested population.

The engineering target remains `0.10%`. `5.16E.3C` introduces an Editor-only temporary `1.00%` review threshold for the accepted PoC deferral. A result between those values is displayed as `Deferred known limitation`; a result above `1.00%` is again reported as an active review failure. This status does not redefine the solver as corrected and does not alter any runtime threshold, transport equation, state, source, or rendering path.

Exact receiver-acceptance transport was deferred because the audited multi-pass solution would cost approximately `2–3×` the current Layer C transport work. The cheaper single-pass directional vacancy cap was also deferred because its estimated `10–25%` Layer C transport cost is not justified by the current sub-1% and visually tolerated loss. Fractional shoreline storage relaxation remains a future zero-dispatch visual A/B candidate, but it is explicitly partial and may create hidden shoreline reservoirs.

Capacity-hit category counters can overlap; `Total` is the union of hit samples, while unit and boundary counts may both include the same cell-substep. No morphology continuation is queued. The numerical issue must be reopened if the `1.00%` review threshold is exceeded, visible loss appears, or materially different content invalidates the controlled evidence; otherwise it belongs to the deferred comprehensive River performance pass.

---

# Addendum — 4.11C.5.13 Low-Resolution Layer D Film Source / Film Support

`4.11C.5.13` implements the first real structural Layer D helper system. This is not a foam entity database and not a pocket tracker. It is a fixed-size field pipeline:

```text
Layer C FoamState + Layer B external support/contact fields
    -> half-resolution Film Source
    -> half-resolution Film Support directional spread
    -> full-resolution _FoamShapeMask
    -> Layer E debug/render sampling
```

Ownership remains acyclic:

```text
Layer B does not read FoamState, Film Source, Film Support, or _FoamShapeMask.
Layer C does not read Film Source, Film Support, or _FoamShapeMask.
Layer D reads Layer B and Layer C and writes only visual products.
Layer E reads visual products and writes screen pixels only.
```

New Layer D products:

```text
_FoamFilmSource  — half-resolution RHalf visual-film permission/source field.
_FoamFilmSupport — half-resolution RHalf broad sheet/contact/bridge support field.
```

`BuildFoamFilmSource` is material-gated after `4.11C.5.13C`. Persistent material creates Film Source. Layer B topology, pressure, lee, shore, and contact support may bias or suppress that material-derived source, but they must not seed Film Source from zero. The result is clipped by valid fluid and obstacle exclusion and suppressed by negative-aging pressure.

`BuildFoamFilmSupport` performs a cheap fixed-tap directional spread over the half-resolution Film Source. It favours along-flow continuity, applies weaker across-flow widening, and includes small diagonal support for bridge/cohesion. After `4.11C.5.13C`, Layer B support/contact can bias or suppress that spread, but cannot create spread without material-derived Film Source. This is the intended low-cost alternative to wide full-resolution neighbourhood classifiers.

`EvaluateFoamShape` now combines clipped persistent material with the film source/support product. This is allowed because `_FoamShapeMask` is visual interpretation, not durable material truth. Fine sub-cell detail still belongs in Layer E shader composition.

New debug views:

```text
Foam Film Source  — samples _FoamFilmSource.
Foam Film Support — samples _FoamFilmSupport.
```

Final Foam remains disconnected from `_FoamShapeMask` until the Layer D output is validated.

# Addendum — 4.11C.5.13B Layer D Domain-Space Film Sampling Fix

`4.11C.5.13B` corrects the coordinate ownership of the Layer D film pipeline.

The fixed contract is:

```text
Layer C FoamState:
  material-space persistent storage.
  Rendering may use residual material travel to display it smoothly.

Layer B external support/contact fields:
  domain-space river support fields.
  These do not follow material residual phase.

Layer D Film Source / Film Support / _FoamShapeMask:
  domain-space current visual products.
  They may read phase-corrected material, but the products themselves are anchored to the river domain.

Layer E shader debug/render sampling:
  Layer C material views use materialUV.
  Layer D visual products use fieldUV.
```

The bug fixed by `5.13B` was that `_FoamFilmSource`, `_FoamFilmSupport`, and `_FoamShapeMask` were sampled through `foam.materialUV`. Because `foam.materialUV` includes residual phase travel and snaps back after integer material commits, domain-anchored film/support products appeared to slide and then snap with the cell grid. The fix is not a tuning change; it is a coordinate-space ownership correction.

Implementation details:

```text
CS_RiverFoam.compute:
  - added FoamResolveMaterialPhaseOffsetUV;
  - added FoamResolveMaterialUVForDomainUV;
  - added FoamSampleMaterialStateForDomainUV;
  - BuildFoamFilmSource samples support/contact fields at domainUV and material state at phase-corrected materialUV;
  - EvaluateFoamShape samples material at phase-corrected materialUV but writes domain-space _FoamShapeMask.

StylizedRiverFoamRuntime.Compute.cs:
  - DispatchEvaluateShape explicitly binds _FoamPhaseTransportMetres before all Layer D kernels.

SH_CleanStylizedRiver.shader:
  - Layer D debug views sample _FoamShapeMask, _FoamFilmSource, and _FoamFilmSupport with foam.fieldUV.

RiverWaterFoam.hlsl:
  - Layer E shader-detail probe uses stable river-space diagnostic coordinates instead of inheriting the residual material phase.
```

Do not reverse this split. If a future effect needs durable material motion, it belongs in Layer C. If a future effect needs broad visual film support, it belongs in Layer D and writes domain-space visual products. If a future effect needs pixel-scale local polish, it belongs in Layer E and must not feed back into compute state.

# Addendum — 4.11C.5.13C Material-Gated Layer D Film Source

`4.11C.5.13C` fixes a semantic Layer D bug exposed after the domain-space sampling fix. The bug was not a coordinate issue. The film products were stable, but `Foam Film Source`, `Foam Film Support`, `Foam Evaluated Shape`, `Foam Shape Difference`, and the shader-detail probe inherited shapes from Layer B support topology because Film Source allowed support to become source directly.

Rejected behaviour:

```text
Layer B topology/support
    -> Film Source
    -> Film Support
    -> _FoamShapeMask
```

Canonical behaviour after this patch:

```text
Layer C material, phase-corrected into domain space
    -> Film Source

Film Source + Layer B support/contact bias/suppression
    -> Film Support

Layer C material + Film Source + Film Support
    -> _FoamShapeMask
```

Hard rule:

```text
Generic Layer B support/contact/topology cannot create visual film by itself.
It can bias, preserve, widen, or suppress material-derived film.
If the project later needs environmental foam/film that appears without spawned material, it must be a separately named and documented product, not accidental generic topology support.
```

Debug-view audit from the 5.13B baseline:

```text
0 Final Foam — clean from the Layer D support-source bug; still uses legacy Final Foam.
1 Foam And Aging Topology — intentionally topology/support-based.
2 Automatic Birth Sources — latest-material-update-only automatic Layer C source-event footprints; RGB identifies source category and white marks same-update overlap; no manual/test-source ownership.
3 Material Presence — clean Layer C material truth.
4 Material Remaining Life — clean Layer C material-life truth.
5 Foam Motion Field — external motion/routing debug, not topology support.
6 Foam Motion Field + Cell Grid — external motion plus intentional material-space cell grid.
7 Foam Evaluated Shape — contaminated before 5.13C through _FoamShapeMask.
8 Foam Shape Difference — truthful comparison, but its evaluated-shape input was contaminated before 5.13C.
9 Foam Chip And Strand Probe — inherited contaminated _FoamShapeMask before 5.13C.
10 Foam Chip And Strand Difference — inherited contaminated _FoamShapeMask before 5.13C.
11 Foam Film Source — direct root of the support-source contamination before 5.13C.
12 Foam Film Support — inherited contaminated Film Source before 5.13C.
```

Implementation details:

```text
CS_RiverFoam.compute:
  - added FoamVisualFilmInfluence;
  - added FoamResolveVisualFilmInfluenceAtDomainUV;
  - Film Source now uses materialBody * supportBias * negativeSuppression;
  - supportBias is a multiplier only and cannot seed source from zero;
  - Film Support still spreads Film Source, but Layer B support only biases/suppresses that spread.

StylizedRiverFoamRuntime.Compute.cs:
  - BuildFoamFilmSupport now binds _FoamTopologyRead and _FoamTopologySourcesRead because the support pass needs bias/suppression data.

StylizedRiverEditor.cs:
  - Film Source / Film Support debug descriptions now state that support cannot create visual film from zero.
```

Expected validation:

```text
Foam Film Source should follow spawned/material foam instead of generic support topology shapes.
Foam Film Support may be broader than Film Source but should not appear where no material-derived Film Source exists nearby.
Foam Shape Difference should show additions caused by material-derived film spread, not raw support topology.
Foam And Aging Topology remains the explicit view for support topology.
Final Foam remains unchanged.
```

---

# Addendum — 4.11C.5.13C Unity Validation and 5.13D Gold-Standard Next Target

Unity validation after `4.11C.5.13C` confirmed the semantic correction worked:

```text
Foam Film Source no longer displays raw support topology where no material-derived foam exists.
Foam Film Support now expands material-derived Film Source instead of topology support.
Foam Evaluated Shape and Foam Shape Difference no longer inherit generic support shapes.
Final Foam remains unchanged.
```

This means the active issue has moved from architecture/semantics to visual spread quality. The current Layer D film system is clean enough to tune, but not visually final.

## Current meaning of Layer D debug views

```text
Foam Film Source
  Half-resolution material-derived visual source.
  It should answer: where is real persistent material feeding possible visual film?
  It is not final foam and not support topology.

Foam Film Support
  Half-resolution spread/support field fed by Film Source.
  It should answer: where can material-derived film broaden, connect, or preserve macro continuity?
  It may be broader than source, but it must not appear from generic support alone.

Foam Evaluated Shape
  Full-resolution domain-space visual mask.
  It combines phase-corrected persistent material with Film Source/Support.
  It is visual interpretation, not durable material truth.

Foam Shape Difference
  Signed comparison against raw material presence.
  Green now means material-derived Layer D addition.
  Magenta means Layer D/Layer E removal.
  It must no longer be interpreted as automatic foam generation.
```

## Why 5.13D was needed

The latest screenshots showed Film Support behaving like a thick, uniform low-resolution dilation around the spawned material ribbon. This is expected from the current first-pass spread formula, but it is not the desired final film shape.

The issue is now:

```text
semantically correct source/support;
visually primitive spread/threshold behavior.
```

The patch tunes spread shape without adding new architecture.

## 4.11C.5.13D — Layer D Film Spread Shape Tune

Status: implemented as a compute-only tuning pass; pending Unity validation.

Target:

```text
Make Film Support less like a uniform capsule dilation and more like controlled surface-film support.
```

Concrete tuning responsibilities:

```text
Film Source:
  keep close to material-derived truth;
  avoid over-thickening the source at half resolution;
  keep support as a small multiplier/suppression only.

Film Support:
  preserve along-flow continuity;
  weaken and condition cross-flow widening;
  tighten bridge/fill thresholds;
  reduce uniform spread around simple ribbons;
  keep support/contact as bias/suppression, not source.

EvaluateFoamShape:
  make supportShape more conservative;
  reduce support dominance over base/material source;
  keep additions visible but selective.
```

Implementation completed in this patch:

```text
FoamResolveVisualFilmInfluenceAtDomainUV:
  supportBias is now 0.94-1.08 instead of 0.90-1.18.

BuildFoamFilmSupport:
  along-flow taps remain the dominant continuity path;
  cross-flow taps are reduced and gated by source/evidence;
  diagonal spread is reduced and tied to the same cross-flow gate;
  bridge thresholds are stricter;
  bridge contribution is reduced to 0.42.

EvaluateFoamShape:
  supportShape threshold is stricter;
  supportShape no longer dominates visualFilm;
  sourceShape remains visible but slightly more conservative.
```

Files intentionally not changed:

```text
StylizedRiverFoamRuntime.*.cs
SH_CleanStylizedRiver.shader
RiverWaterFoam.hlsl
StylizedRiver.cs
StylizedRiverEditor.cs
```

No-touch rules:

```text
Do not switch Final Foam to _FoamShapeMask.
Do not reintroduce support-only source.
Do not add environmental contact film yet.
Do not add entities, pocket IDs, or connected-component foam tracking.
Do not mutate FoamState, Remaining Life, or Material Pattern from Layer D.
Do not tune Layer E shader detail as part of this patch.
Do not expose Inspector controls yet.
```

Primary code file to inspect first:

```text
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute
```

Specific functions to inspect:

```text
FoamResolveVisualFilmInfluenceAtDomainUV(...)
FoamResolveVisualFilmSourceAtDomainUV(...)
BuildFoamFilmSource
FoamLoadFilmSource(...)
BuildFoamFilmSupport
EvaluateFoamShape
```

Acceptance criteria:

```text
Foam Film Source still follows material only.
Foam Film Support remains broader than source but less uniformly inflated.
Foam Shape Difference shows smaller/more selective green additions.
No support-topology shapes return.
No phase/cell-grid stutter returns.
Final Foam remains unchanged.
```

### 2026-07-09 — River Foam 4.11C.5.14A Layer C Automatic Shore/Contact Source Population

Audited the current birth architecture after 5.13D validation. The audit found that manual/progressive birth, support/lifetime capture, topology/contact fields, and Layer D material-derived spread exist, but automatic birth near specific environmental locations was missing. The correct next step is therefore Layer C source population, not a new Layer D environmental-film authority.

Implemented the first conservative source class: disabled-by-default automatic shore/contact birth. The runtime scans sparse shore-support-band candidates at a low fixed cadence, accepts a bounded subset based on the river seed and amount, then queues real persistent material through `PendingInjection`, `QueueMaterialBirth`, and the existing `InjectFoam` compute kernel. The material then lives or dies under the existing support/negative aging system.

This patch initially added Inspector controls under `Source Population`: `Automatic Birth Enabled` and `Shore Contact Birth Amount`, plus runtime counters/status. Validation showed that the single amount slider was overloaded and could create large shore chunks. Patch `4.11C.5.14B` then overcorrected by exposing too many implementation controls. Patch `4.11C.5.14C` simplified the control surface, but validation showed the hidden implementation was too starved. Patch `4.11C.5.14D` keeps the Layer C source-population route and uses deterministic full-strength source events controlled by Coverage, Activity, Patch Size, and Pattern. It does not switch Final Foam, does not create support-only Film Source, does not add a visual-only environmental film texture, and does not create entities or pocket IDs.

### 2026-07-09 — River Foam 4.11C.5.14B Source Population Controls / Shore Birth Profile

Validated `4.11C.5.14A` enough to confirm automatic birth works, but the old `Shore Contact Birth Amount` created large blocky chunks because it controlled density, footprint, initial amount, initial life, elongation, and compound shape together. This was a source-profile design problem, not a Layer C architecture problem.

`4.11C.5.14B` correctly defined source-class-specific spawning as the contract for future automatic birth, but its Inspector exposed too many implementation controls and was not suitable for authoring or validation.

### 2026-07-09 — River Foam 4.11C.5.14C Simplified Shore Spawn Controls

`4.11C.5.14C` keeps the source-class-specific spawning contract but removes the low-level shore controls from the Inspector. Shore Contact Birth is now a deterministic sparse shoreline-stroke recipe controlled by four intent-level values: Coverage, Size, Strength, and Persistence.

The shore recipe no longer exposes per-tick budget, support threshold, inward band, radius, elongation, stroke length, initial amount, initial life, jitter, or shape mode. Internally, Coverage maps to candidate spacing/acceptance and budget; Size maps to conservative radius/stroke length; Strength maps to initial material presence; Persistence maps to initial Remaining Life. Shore birth always uses small deterministic strokes, never compound blobs.

### 2026-07-09 — River Foam 4.11C.5.14D Deterministic Shore Source Events

`4.11C.5.14D` replaces the `5.14C` one-shot sparse shore stroke recipe with deterministic shore source events. The architectural rule is unchanged: automatic shore birth creates real persistent Layer C material through the existing progressive composition / material injection path; support/lifetime capture decides survival; Layer D may only spread material-derived film; Final Foam remains unchanged.

The patch rejects faint-deposit accumulation as the shore strategy. Automatic shore events now spawn normal-strength material and reveal their area spatially over the event duration. This is intended to reduce the visual read of a completed patch teleporting into existence without paying for many weak births.

The Source Population UI now exposes only:

```text
Coverage
Activity
Patch Size
Pattern: Mixed / Shore Ribbons / Inward Wash
```

Two recipes are implemented: `Shore Ribbon`, a bank-parallel opaque ribbon source event, and `Inward Wash`, a shore-attached event that drifts inward/downstream from the bank contact band. Both are scheduled through deterministic slots distributed along both banks, bounded by a maximum number of starts and scans per update.

### 2026-07-09 — River Foam 4.11C.5.14E Automatic Source Event Rasterizer

Runtime validation of `4.11C.5.14D` showed that deterministic source-event scheduling alone was not enough. Both `Shore Ribbons` and `Inward Wash` still flowed through the generic progressive composition / `PendingInjection` / `InjectFoam` segment path, so the GPU only received capsule-like stamps. The result was predictable near-shore rectangles/bars, insufficient coverage at max settings, and no strong visual difference between patterns.

`4.11C.5.14E` keeps the Layer C material-birth contract but separates final automatic source generation from the manual/debug injection primitive path. Automatic shore slots now create typed source events and a dedicated `RasterizeFoamSourceEvent` compute kernel evaluates shore-local analytic masks against `_FoamCurrentShoreEdgesRead`. The kernel writes real persistent material through `FoamMergeBornPresence`; support/lifetime capture, Layer D Film Source/Support, and Final Foam integration remain unchanged.

Implemented event types:

```text
ShoreRibbon
  live-shore-following ribbon band with deterministic breakup and tapered ends.

InwardWash
  shore-attached inward/downstream tongue with progressive area reveal and curvature.
```

The current UI remains Coverage, Activity, Patch Size, and Pattern. The old generic `PendingInjection` path remains available for manual/debug/simple births only.

### 4.11C.5.14F source formation rule

Automatic Layer C source events now separate three concepts that were previously coupled:

1. **Coverage** — which eligible shoreline slots can participate over time.
2. **Activity** — how often new source events start.
3. **Formation Speed** — how quickly a single source event forms along its path, in metres per second.

This keeps source density/frequency independent from source kinematics. The user-facing problem was that source events appeared as if a mask popped on in about one second. The fix is distance-based formation: a longer source path takes longer to form at the same formation speed.

Inward Wash also changes from a filled reveal mask to a moving stroke-head. The source rasterizer now writes a short curved head/trailing segment per update, while persistent FoamState preserves the trail. This preserves the Layer C rule: the rasterizer writes real material, not visual-only film, and Layer D only interprets that material afterward.

### 4.11C.5.14G shore wash refinement rule

`4.11C.5.14G` keeps the 5.14E/5.14F automatic source-event rasterizer architecture but tightens the `Inward Wash` source class. The scope is still shore-related Layer C spawning only.

The refined rule is:

- `Shore Ribbon` remains the primary validated shore source.
- `Inward Wash` is not a large filled tongue and not a broad moving body. It is a small detaching stroke that starts by following the shore and then peels inward.
- Wash events use separate, shorter head-trail limits from ribbons.
- Wash fill noise is low so shape is controlled by stroke geometry rather than chunky source-fill cells.
- `Mixed` is protected from bad wash dominance by greatly reducing Inward Wash weighting.

This patch still writes real persistent FoamState material through the Layer C rasterizer. It does not alter Layer D visual-film evaluation or Final Foam.

### 4.11C.5.14H foam birth authoring framework

`4.11C.5.14H` does not alter the Layer C source-event rasterizer contract. It changes authoring: shore source recipes are no longer hardcoded experimental constants. They are controlled by a source-category inspector framework.

Current source categories:

```text
Shore Foam      implemented source category
Object Foam     staged placeholder for later static-object/contact spawning
Free Water Foam staged placeholder for later open-water source spawning
```

The implemented Shore Foam category keeps Coverage and Activity as category-level density/rate controls. Pattern composition is controlled by normalized pattern shares whose sum is always one. This means changing `Shore Ribbons` versus `Inward Wash` changes which source type is selected in Mixed mode, not the total source rate.

Each implemented shore pattern now owns its own source-authoring controls: Formation Speed multiplier, dimensions, Initial Life, and Breakup Strength. `Initial Life` is the normalized Remaining Life written into newly born persistent material; it is not event duration. Event duration still derives from source path distance divided by formation speed.

Dimension selection now uses a correlated event scale plus small per-axis jitter and aspect guards. This preserves deterministic variety without allowing short/fat or reach/width-incoherent shore wash events.

### 4.11C.5.15A Object Foam source category

Object Foam extends the Layer C source-event rasterizer with static-object source events. The anchor list is exported from the existing disturbance runtime static source registry, keeping scheduling deterministic and bounded on CPU. GPU rasterization evaluates object-local contact arc/fleck masks, gates them by valid fluid, obstacle exclusion, and static pressure contact evidence, then merges births into persistent material state. This remains a spawning feature only; it does not change transport/evolution or Final Foam composition.

### 4.11C.5.15A.1 Object Foam activation correction

Object Foam activation is category-driven. `Spawn Preset` no longer silently disables Shore or Object source categories except when set to `Off`. The intended hierarchy is: `Automatic Foam Birth` global master switch, `Spawn Preset = Off` global disable, and per-category `Enabled` toggles for Shore/Object/Free Water. Object Foam runtime diagnostics include copied static source anchor count before events are scheduled.

### 4.11C.5.15A.2 Object Contact Edge Field

Object Foam now uses a local contact-edge field for final source shape authority. CPU static source snapshots still schedule bounded object events; the GPU contact field supplies per-cell contact confidence, object-to-water normal, and upstream/front-side relevance derived from obstacle exclusion plus static pressure/contact context.

This preserves the selected performance model: no GPU readback, no texture-wide source spawning, no particle system, and no connected-component event generation. Object extents remain as coarse bounds only. Contact Arc and Contact Fleck masks now use field normal/tangent space so they can follow actual contact edges rather than object half-extent rectangles.

## Addendum — 4.11C.5.15A.4 Object Contact Semi-Arcs

Object Foam now has three Layer C source recipes: full Contact Arcs, lopsided Contact Semi-Arcs, and Contact Flecks. This is still persistent material birth, not Layer D shape evaluation and not Final Foam rendering.

The reason for the additional recipe is mathematical: full Contact Arcs use a tangent-space mask centred by `abs(tangentDistance)`, which is stable but inherently symmetric. Contact Semi-Arcs use the same object-contact field and coarse object bounds, but carry deterministic signed lopsidedness through the existing source-event `Curvature` / GPU `variation.w` channel. The semi-arc evaluator projects into contact tangent space, multiplies by the signed side, and gates the source with a one-sided interval:

```text
-backReach < tangentDistance * side < revealedForwardReach
```

This keeps the selected performance model intact: no GPU readback, no connected-component extraction, no new textures or buffers, and no new object-contact resource binding. Historical 5.15A.2/5.15A.3.4 used a broad contact authority. `5.18C` supersedes that normal-thickness rule with the immediate eight-neighbour water shell while retaining the same contact texture, source-event rasterizer, and raw physical object bounds.

### Layer C Free Water Birth — 4.11C.5.15B

Free Water Foam is now a Layer C source category alongside Shore Foam and Object Foam. It writes persistent material state through the same automatic source-event rasterizer instead of inserting final visual foam.

Implemented source grammars:

- **Lace Connector**: head+stroke emission along a curved open-water path. Earlier samples persist in FoamState while the head advances.
- **Torn Fragment**: asymmetric local fragment shape revealed by a timed sweep. It is patch-shaped, but not instant.

Bright glints/scratches from the visual reference remain out of Layer C. They belong to later shader/rendering work, not persistent material birth.

The source-event dispatch path now supports an optional Y range. Existing shore/object events dispatch the full field height; free-water events dispatch only the lateral band required by their local shape.

#### 4.11C.5.15B.2 Cross-Lace Connectors

Free Water Foam now has a third source grammar: **Cross-Lace Connector**. The original Lace Connector is flow-aligned because its sampled path runs along global distance. Cross-Lace swaps that path basis so the source head travels across the river laterally while the ribbon only bends slightly along flow. This is intended to supply the horizontal/cross-current pale ribbons visible in the visual target without increasing global spawn density or inserting final foam art.

Cross-Lace remains Layer C material birth only. It writes persistent FoamState through the existing automatic source-event rasterizer, is clipped by river boundary and obstacle exclusion, and uses the existing local X/Y dispatch bounds.


### Clean binary Presence-Amplitude eligibility — P12j

P12i retained exact Candidate × Eligibility ownership but Unity showed that the eligibility field itself remained procedurally stippled and that fractional visible-support intensity weakened removal across faint fringe. P12j separates eligibility geometry from patterned Foam rendering.

`Current` remains unchanged. `Presence-Amplitude` receives a transient clean silhouette from committed Presence plus the existing near-death life gate before pattern erosion. The silhouette follows the existing stored/warped/lead/trail and surface-coupling path, then drives edge width through Euclidean screen-gradient magnitude. Meaningful support is binary permission; candidate antialiasing remains the only fractional removal contour. Production remains exactly Candidate × Eligibility and no secondary permission region exists.

### Exact pre-Chip rendered-mask Chip ownership — P12k

The Presence-Amplitude Chip edge is defined by the exact no-Chip mask that Final would compose:

```text
strandKeep = existing structural Strand evaluation
preChipRenderedMask = saturate(foam.mask × strandKeep)
visible support = RiverWaterFoamResolveBaseCoverage(preChipRenderedMask)
production = candidate × eligibility(preChipRenderedMask)
final = preChipRenderedMask × (1 - production)
removed = preChipRenderedMask - final
```

This ordering includes Presence amplitude, Remaining Life, patterned erosion, warp/stretch coupling, hardening, and structural Strands before eligibility. Current retains the prior selection/application ordering unchanged.



### Binary Presence-Amplitude Chip selection — P12l

P12k defines the exact no-Chip rendered Foam geometry as `preChipRenderedMask = foam.mask × strandKeep`. P12l changes only Presence-Amplitude Chip selection and application:

```text
candidateSelected   = chipCandidateField >= 0.5 ? 1 : 0
eligibilitySelected = chipEligibility.edgeBand >= 0.5 ? 1 : 0
productionSelected  = candidateSelected × eligibilitySelected
finalFoamMask       = productionSelected == 1 ? 0 : preChipRenderedMask
```

The `0.5` values are contour-selection thresholds, not removal strengths. Production is binary and removes 100% of selected Foam. Presence-Amplitude Candidate, Eligibility, and Production debug views expose these exact three masks. Current retains its accepted continuous candidate/eligibility fields, Interior Access, soft-mask reconstruction, and Strand ordering.


### Any-support binary Presence-Amplitude Chip selection — P12m

P12k remains the geometry owner and P12l remains the full-removal foundation. P12m changes only the binary interpretation of the existing antialiased Candidate and Eligibility fields:

```text
candidateSelected   = chipCandidateField > 0.0 ? 1 : 0
eligibilitySelected = chipEligibility.edgeBand > 0.0 ? 1 : 0
productionSelected  = candidateSelected × eligibilitySelected
finalFoamMask       = productionSelected == 1 ? 0 : preChipRenderedMask
```

This implements exact per-pixel any-support ownership. It does not expand or regenerate Candidate or Eligibility geometry, add Interior Access, derive another permission region, or modify Layer C, Layer D, Strands, source behavior, transport, lifetime, controls, resources, or serialized state.

Presence-Amplitude Candidate, Eligibility, and Production diagnostics are binary. Current retains its continuous Candidate/Eligibility values, optional Interior Access, continuous production coverage, soft reconstruction, and existing final behavior. `Foam Chip And Strand Probe` is the authoritative exact final Foam mask in both modes.

Because Candidate visibility includes existing antialias, readability, and subpixel fades before binary selection, every positive tail receives full Presence-Amplitude authority. Hard edges or candidate pop-in are an accepted risk of the absolute any-support rule and require Unity evidence before any separate tuning patch.


## Layer E optional Candidate-Straddle admission — `RG-METRIC-P12n` — rejected

P12n adds transient candidate permission inside Layer E. It does not mutate Presence, Remaining Life, Material Pattern, transport, sources, Film, Shape, or topology caches.

```text
Existing analytical candidate identity and motion
        +
low-frequency centre/perimeter support classification
        -> binary candidate admission texture
        -> existing render-frame analytical candidate
        × exact pre-Chip rendered Foam
        -> full binary removal at selected Foam pixels
```

The experimental support test is candidate-level rather than a per-pixel edge-distance field. A new candidate enters only when its centre is outside the camera-independent shaped support and at least two irregular-perimeter samples are inside. A previously admitted candidate retains while one perimeter contact remains and its centre is not convincingly interior. This produces complete candidate-shaped overlaps without using the derivative Edge Width band.

The support evaluator is a fixed-world-footprint approximation of the no-Chip Layer E state. It mirrors stable pattern, lifecycle erosion, hardening, and structural Strand policy from the render include, but deliberately omits screen derivatives and surface-wake deformation. This limitation can create missed/no-op attachment or timing mismatch; exact final removal prevents it from removing pixels where rendered Foam does not exist.

The established P12m Rendered Edge Band path remains default and selectable. Current Presence Footprint remains unchanged. Candidate Straddle evaluates admitted candidates on every positive exact pre-Chip rendered-mask pixel rather than inheriting the Rendered Edge Band route’s `0.08` BaseCoverage gate, so faint exact Foam fringe is part of the same binary removal. Candidate Straddle uses one on-demand point RFloat texture, one due-time compute dispatch at default `4 Hz`, and point loads only for overlapping active candidates in the existing fragment candidate loop. No new pass, draw, persistent simulation field, or high-resolution topology texture is introduced. Unity visual and performance acceptance are pending.


## Layer E boundary-anchored Eligibility — `RG-METRIC-P12o`

P12o restores the required ownership split:

```text
original full-rate analytical Candidate Field
        ×
selected Eligibility route
        -> Production
        -> exact pre-Chip rendered Foam removal
```

The experimental cache does not contain candidate shape or candidate authority. It stores one local boundary descriptor per deterministic candidate identity. Initial acquisition samples the current candidate centre, then checks the existing deterministic ring directions until the first occupied/empty disagreement supplies a known boundary bracket. Four binary refinements locate the transition and a local four-axis probe refines the inward normal. Once valid, tracking starts from the previous boundary anchor and inward normal, using intermediate support samples across the tracking interval so thin Foam ribbons are not skipped. Failed or discontinuous tracking locks the descriptor until the candidate reaches dormancy, preventing interior reacquisition or boundary teleportation.

At render time the original Candidate Field is unchanged. A valid descriptor reconstructs a local strip with inward depth limited by `Chip Edge Width` and tangent extent limited by the current analytical candidate reach. Experimental Production is binary Candidate × strip Eligibility. The strip is displayed directly in `Chip Eligibility Composite`; no hidden admission or secondary reach field exists.

The descriptor solver is camera-independent and topology-based at candidate-local samples. It uses occupied/empty support and a bracketed boundary transition rather than derivatives of a soft scalar. This is not a global exact edge field; sharp curvature and screen-derived micro-boundaries remain risks to validate in Unity.

Current/P12m remains the default and fallback. The experimental texture is one on-demand ARGBFloat record per guarded candidate identity, updated at default `4 Hz`. `XY` stores the River-space boundary anchor; `Z` exactly packs lateral identity, three-state ownership, and a 10-bit normal angle; `W` stores the exact longitudinal identity. Absolute identities map through a circular modulo cache so ordinary downstream lattice-origin movement preserves valid and locked history without a second texture. For the previously observed `520 × 67` allocation, logical payload is `544.4 KiB`. No additional render pass, draw, high-resolution topology field, or persistent material state is added.

Status: source implementation and `41/41` pre-package offline gates pass. Unity 6000.5 import, same-frame visual A/B, and measured GPU cost remain pending.

## P12p Layer E addendum — one Candidate Field and isolated rendered-fringe Eligibility

P12n/P12o low-frequency candidate/boundary caches are rejected and removed. Layer E returns to one candidate producer: the original analytical render-frame Candidate Field. Candidate lifecycle, motion, pulse, rotation, irregularity, view stabilization, and antialiasing are unchanged.

Presence-Amplitude production remains:

```text
binary any-positive-support Candidate
×
binary any-positive-support rendered Eligibility
=
binary Production
```

Selected Production pixels remove the complete exact `preChipRenderedMask`. Eligibility is derived from the exterior rendered-fringe coordinate only. `preChipRenderedMask` is clamped to `0.34`, normalized from the existing visible start `0.08`, and differentiated in screen space. The clamp makes the coordinate flat through the inner hardened body, preventing the hard-body rise from acting as another edge. Current Presence Footprint retains the accepted soft-visibility edge path.

This remains render-only Layer E work. It adds no persistent state, material mutation, topology field, texture, buffer, kernel, dispatch, pass, or cache.

## P12r Layer E architecture correction

P12q's separate binary-topology Eligibility subsystem is removed. Layer E returns to the P12p architecture:

```text
exact no-Chip rendered Foam
        +
original analytical Candidate Field
        +
per-fragment rendered exterior-edge Eligibility
        ↓
Candidate × Eligibility
        ↓
complete selected-pixel removal
```

There is no Chip topology texture, erosion compute stage, fixed-frequency Chip update, Eligibility-mode selector, or world-space erosion-width control. This removal does not change Layer C material state, transport, sources, Film, Shape, Strands, candidate identity, candidate animation, or final Foam composition. Offline source comparison confirms all ten restored implementation files are byte-identical to P12p.



## P12s Layer E addendum — selectable soft-mask reconstruction under Presence-Amplitude

P12s does not change Layer ownership. Layer C remains persistent material, Layer D remains visual Film/Shape evaluation, and Layer E remains render-only composition.

The Candidate Field is one unchanged full-rate analytical field for every route. Only Presence-Amplitude Eligibility/application is selectable:

### Exact Rendered Removal — retained default

```text
binaryCandidate = Candidate > 0
binaryEligibility = rendered-fringe Eligibility > 0
Production = binaryCandidate × binaryEligibility
Final = Production ? 0 : exactPreChipRenderedMask
```

### Soft-Mask Reconstruction — experimental

```text
binaryVisibleSupport = exactPreChipRenderedMask > 0
softEligibility = binaryVisibleSupport
                  × soft edge band(preChipSoftVisibility,
                                   SoftEdgeStart,
                                   ChipEdgeWidth)
Production = continuousCandidate × softEligibility
postChipSoft = coherentSoftVisibility × (1 - Production)
Final = reharden(postChipSoft) × structuralStrandKeep
```

The experiment uses binary visible support so faint Foam does not weaken permission, while Candidate and edge membership remain continuous. Interior Access is disabled for Presence-Amplitude. Current Presence Footprint remains on the accepted soft-mask reconstruction path and is not affected by the selector.

The architecture adds two scalar uniforms and no resource or cadence. The shared include still has one production consumer. The primary visual risk remains derivative-coordinate quality; `Soft Edge Start` makes that coordinate tunable without changing Candidate geometry or adding another topology system.

## P12t accepted Layer E Chipping contract

Production Chipping is Layer E render-only work. Its sole accepted application is soft-mask reconstruction:

```text
Original continuous analytical Candidate
× soft Eligibility permission
→ modify coherent pre-hardened visibility
→ reharden Foam body and fringe
→ apply structural Strands
→ Final Foam
```

Presence-Amplitude uses binary exact rendered support to authorize its soft Eligibility coordinate and disables Interior Access. Current retains its historical soft Eligibility plus optional deterministic Interior Access. Exact Rendered Removal and its application selector are removed.

Layer D remains a diagnostic evaluated-shape system. Its Visual Occupancy Build/Release controls and previews do not feed normal Final Foam. Candidate, Eligibility, Production, Chip-and-Strand Probe, and Chip-and-Strand Difference are Layer E diagnostics.


## Unified Automatic Source Reveal-Speed Contract — `RG-METRIC-P12u`

Automatic Layer C source timing now has one authoritative definition across Shore Ribbon, Inward Wash, Object Contact Arc, Object Contact Semi-Arc, Object Contact Fleck, Lace Connector, Cross-Lace Connector, and Torn Fragment:

```text
requested reveal speed = Base Reveal Speed × recipe Reveal Speed Multiplier × deterministic jitter
resolved reveal duration = max(one Layer C material step, path distance / requested reveal speed)
```

This is source formation/reveal only. It does not control event Activity, already-born Foam transport, Remaining Life, or final rendering.

Arc/Semi-Arc use Reveal Speed for Build only. Their accepted Build → Hold → progressive Release → Rest lifecycle and persistent-emitter behavior remain unchanged. Flecks consume their complete event duration. Torn Fragments use the same honest path-distance timing as every other source rather than a compressed sweep formula.

The previous Shore `14 s`, Object `4 s`, Lace `5 s`, Cross-Lace `3.5 s`, and Torn Fragment `1.35 s` ceilings are superseded. The material cadence is the only minimum-duration constraint. Extremely slow authored speeds can increase 32-slot pool occupancy and reject new starts; saturation is an explicit runtime condition and is reported rather than hidden by acceleration.

Contact Fleck and Free-Water correlated size/life/presence sampling now reaches the full authored Min/Max intervals. Source grammar, geometry, deposition ownership, source strength, transport, Layer D, and Layer E are otherwise unchanged.


## P13A authoritative Layer C material and visibility contract

P12t Layer E Chipping and P12u automatic Reveal Speed are frozen baselines. P13A changes the semantic boundary between geometric occupancy, persistent material, transport, and Final visibility.

### Packed state

```text
Coverage C          = A
Intrinsic Presence P = R / A
Remaining Life L     = G / R
Material Pattern M   = B / R

Packed = (C×P, C×P×L, C×P×M, C)
```

Coverage is geometric cell occupancy. Presence is intrinsic authored material strength. Life is intrinsic lifecycle state. Pattern is stable material identity. Shape/profile/subcell/valid-fluid operations may alter Coverage; only explicit material-authoring or overlap policy changes Presence; only Layer C lifecycle aging or overlap refresh changes Life.

A positive old RGB state with zero alpha is decoded transiently as `Coverage = R`, `Presence = 1`, preserving the former visible material amount until normal state rewrite.

### Birth

For a source with `Initial Presence = 0.75` and `Initial Life = 1.00`, every newly occupied sample decodes to `P=0.75`, `L=1.00` regardless of whether Coverage is `1.0`, `0.25`, or another positive shape value. Cross-Lace subcell attenuation remains geometric Coverage and no longer weakens intrinsic material.

Birth overlap is `Max + Refresh` because one cell stores one cohort:

```text
C = max(existing C, source C)
P = max(existing P, source P)
L = max(existing L, source L)
M = existing M, unless source adds new C
```

This deliberately prevents weak dying material from suppressing a fresh event. It is not a multi-cohort physical mixture.

### Transport

Donor Cell transports the complete packed donor state. TVD Superbee limits/reconstructs Coverage alone and re-encodes the donor's coherent intrinsic state at the reconstructed Coverage. Conservative flux then moves Coverage and all material moments together. Uniform material retains its decoded Presence/Life through numerical diffusion; cells mixing different material decode explicit moment-weighted values.

D3 adds two explicitly experimental transport-integrated FCT selections. They use Donor Cell as the bounded low-order state and derive signed correction mass from a Lax-Wendroff target minus the Donor flux on each open interior east/north face. A multidimensional cell limiter accepts only the correction mass permitted by the current-cell/neighbour Coverage extrema and resolved valid-fluid capacity. Every accepted Coverage correction uses the actual correction donor's decoded `P/L/M` and one shared face limiter, so the equal-and-opposite packed transfer remains:

```text
ΔPackedMass = ΔCoverageMass × (P, P×L, P×M, 1)
```

The Low and Medium modes use fixed correction scales `0.35` and `0.70`. They are diagnostic candidates, not authoring controls or accepted defaults. Their temporary resources exist only while either experimental mode is selected.

Unit capacity and the valid-fluid boundary own maximum Coverage. Convergent capacity resolution and valid-fluid clipping both reduce Coverage coherently and re-encode the same intrinsic Presence, Life, and Pattern; neither independently saturates packed moments or reinterprets a boundary fraction as intrinsic material.

### Final visibility

```text
Transport Scheme
    -> spatial distribution of Coverage
Final Visibility Mode
    -> Coverage + Life to resolved shape
Presence Footprint
    -> whether intrinsic Presence scales that shape
P12t Chipping / Strands / final composition
```

- Concentration + Lifetime sharpens local Coverage and retains continuous patterned erosion from Life.
- Lifecycle-Faithful uses a meaningful-Coverage footprint and passes full pattern survival while Layer C Life remains positive. State removal at Life zero remains owned by Layer C.
- Coverage-Only ignores Presence amplitude after shape resolution.
- Presence-Amplitude carries the resolved Coverage/Life shape and its exact Presence-weighted counterpart through identical Presence-independent wake/warp/surface-coupling weights. Uniform Presence remains exactly proportional in the completed resolved mask. Presence does not feed source coverage, hardening, pattern thresholds, Chipping eligibility geometry, transport selection, or coupling weights.

The three policies are co-located under `Foam > Transport & Visibility Contract` with an always-visible resolved-contract explanation.

### Preserved ownership

Negative topology and all aging rates remain unchanged. The original analytical Candidate, soft Eligibility, accepted soft-mask Chipping reconstruction, and structural Strand order remain Layer E authorities. P13A adds no resource, cadence, pass, or draw-call dependency.

### RIVER-FOAM-TRANSPORT-D1 ribbon compactness diagnostic contract

The visibility audit established that `Concentration + Lifetime` can hide broad fractional Coverage while `Lifecycle-Faithful` exposes it. That does not by itself determine whether the exposed support is correct. The active transport question is whether the Layer C finite-volume solve preserves a thin ribbon footprint or conserves only integrated packed material while spreading its support across progressively more cells.

For scalar Coverage, the conservative update is:

```text
M_i^n = C_i^n A_i
M_i^(n+1) = M_i^n - Δt (F_e - F_w + F_n - F_s)
C_i^(n+1) = clip(M_i^(n+1) / A_i, 0, validFluid_i)
```

This guarantees neither constant support width nor constant interface thickness. At an isolated one-cell maximum the Superbee backward and forward differences have opposite signs, so the limited slope is zero and the first update is the Donor Cell update. A one-cell ribbon can therefore become multiple fractional cells while preserving integrated Coverage. Presence, Life, and Pattern remain coherent because all packed moments use the same face flux.

`RIVER-FOAM-TRANSPORT-D1` adds an explicit Editor-only CPU diagnostic mirror; it does not modify the production solver. One Inspector action runs three bounded matrices:

1. a one-dimensional intrinsic-diffusion matrix with `144` parameter cases and `720` checkpoint rows across Donor Cell/Superbee, 1/2/4/8-cell widths, full/quarter-cell initial Coverage, CFL `0.10–0.90`, and travel `0.25–4 m`;
2. a Cartesian two-dimensional matrix with `108` parameter cases and `324` checkpoint rows across both schemes, flow-aligned/cross-flow ribbons, 1/2/4-cell widths, downstream/lateral/diagonal velocity, CFL `0.25/0.50/0.75`, and travel `0.5–2 m`;
3. a current live-field matrix with up to `108` parameter cases and `324` checkpoint rows when all three automatically selected open, high-lateral, and obstacle-influenced anchors resolve, comparing anchor-resolved downstream-only frozen-local-uniform, live-downstream-only, and complete live velocity.

Live travel checkpoints integrate Coverage-mass-weighted mean speed rather than maximum cell speed. Every checkpoint records integrated Coverage error, peak Coverage, support at `C = 0.02/0.10/0.20/0.30/0.50`, Coverage-weighted centroid error, covariance-derived designated thickness/length, principal minor/major extents, connected components at `C = 0.10`, and low-Coverage tail fractions. The covariance includes each cell's finite interior second moment (`Δ²/12`), so a single occupied cell measures one cell of thickness rather than zero.

The comparison contract is diagnostic:

```text
1D growth                    -> intrinsic scheme diffusion
2D minus corresponding 1D   -> orientation and split-axis effects
frozen live-field growth    -> fixed-metric/valid-fluid geometry
downstream-only minus frozen -> downstream-speed gradients and slowdown
complete-live minus downstream-only -> lateral intent, shear, and routing
1-cell versus wider ribbons -> resolution dependence
C=0.25 versus C=1.0          -> subcell-amplitude dependence
```

The suite runs only from an explicit Play Mode button, performs no GPU readback, adds no per-frame work, never seeds or advances live Foam state, and writes one report under `Library/RiverFoamDiagnostics`. It is an Editor-only scalar Coverage CPU mirror rather than a synthetic GPU dispatch: intrinsic Presence, Remaining Life, and Pattern are held uniform because production transports all packed moments through the same face flux. This isolates footprint deformation but does not replace Unity compilation or future GPU parity evidence. Its output is evidence for a later solver decision; it must not automatically select compression, geometric VOF, higher resolution, or a Lagrangian representation.


### RIVER-FOAM-TRANSPORT-D2 conservative compactness tournament contract

D1 proved that the current transport is conservative but not compactness-preserving. D2 remains Editor-only and diagnostic. It adds no production kernel, resource, serialized control, runtime selection, or default change.

The supplied repository does not contain the deleted `4.11C.5.4c` predictor/corrector/compression implementation. The only surviving evidence is the removal tombstone in `CS_RiverFoam.Transport.hlsl` and the corresponding deleted-runtime placeholders. D2 therefore does not restore, approximate, or attribute behavior to that historical path.

For every valid cell:

```text
V_i = resolved valid-fluid capacity
C_i = Coverage
q_i = C_i / V_i
M_i = C_i A_i
```

After each mirrored TVD Superbee transport substep, a candidate may request an equal-and-opposite mass transfer across an interior open face. Transfer direction is always from the lower normalized fill toward the higher normalized fill. No candidate creates or deletes requested mass.

Normal interface-compression candidate:

```text
q_f = 0.5 (q_a + q_b)
D_face = s_n CFL [4 q_f (1 - q_f)] min(V_a A_a, V_b A_b)
```

Flux-corrected anti-diffusive candidate:

```text
D_face = s_a CFL |q_b - q_a| min(V_a A_a, V_b A_b)
```

`D_face` is only a desired transfer. Before application, D2 accumulates every cell's total requested outflow and inflow. Donor scale is limited by available mass; receiver scale is limited by remaining valid-fluid capacity. Each final face transfer uses the smaller donor/receiver scale, then all faces apply simultaneously. The required invariants are:

```text
Σ M_after = Σ M_before
0 <= C_i <= V_i
closed face => zero transfer
zero-mass gap => no bridge source
uniform q => zero correction
binary 0/1 interface => zero correction unless a partial receiving capacity exists
```

The candidate ledger is intentionally bounded:

- baseline TVD Superbee;
- normal compression strengths `0.25 / 0.50 / 0.75`;
- anti-diffusion strengths `0.15 / 0.30 / 0.45`;
- one hybrid candidate `normal 0.35 + anti-diffusion 0.20`.

Uniform synthetic cases compare every candidate against an exact geometric reference. The reference translates the original axis-aligned ribbon analytically and rasterizes its exact overlap with each base cell; it therefore owns the zero-diffusion target without requiring a numerical solver. Selected cases also run current Superbee at `2×` and `4×` linear resolution and conservatively downsample to the base lattice. These references expose what resolution alone buys without proposing the resulting cell-count cost as a default.

D2 retains all D1 global measurements and adds local branch evidence at `C >= 0.10`. For every supported cell, contiguous support is measured along horizontal, vertical, and both diagonal axes; the minimum physical run is that cell's local thickness. The report records median, P95, maximum, support compactness, and:

```text
separation excess = max(0, global designated thickness - local P95 thickness)
```

This separates branch displacement/splitting from actual local blob thickening more honestly than global covariance alone.

The tournament contains four evidence layers:

1. deterministic candidate self-checks for conservation, bounds, uniform/binary invariance, partial valid-fluid capacity, zero-gap non-bridging, and repeatability;
2. synthetic exact-reference transport across both orientations, 1/2/4-cell widths, full/quarter Coverage, downstream/lateral/diagonal velocity, low/production/high CFL, and `0.5/1/2 m` travel;
3. correction-only adversarial topology for parallel separated ribbons, detached blobs, an L bend, a Y split, a hollow ring, checkerboard input, and a smooth fractional hump;
4. captured live-field transport for baseline plus the two strongest non-baseline synthetic candidates at the D1 open-low-lateral, open-high-lateral, and obstacle-influenced anchors.

The provisional diagnostic targets after `1 m` in open uniform transport are:

```text
initial width 1 cell: local P95 growth <= 1.50
initial width 2 cells: local P95 growth <= 1.25
initial width 4 cells: local P95 growth <= 1.15
absolute mass error ratio < 0.001
centroid error < 0.5 base cell
no negative Coverage, capacity excess, or unexpected component change
```

The report ranks candidates from exact-reference error, centroid error, local-thickness growth, mass/bounds, topology mismatches, and target failures. Ranking only chooses which candidates receive the expensive live-field matrix. It does not change production and is not an approval. D2 evidence rejected the isotropic post-transport correction as a production design: weak anti-diffusion improved compactness but retained excessive thickness and introduced phase/topology errors, while stronger normal/hybrid variants snapped or fragmented material.

D2's original synchronous Inspector execution is superseded by D3. The same evidence matrix now advances as a cooperative main-thread state machine with a `4 ms` target slice. It exposes stage, current case, progress, elapsed time, ETA, Pause/Resume, and Cancel; it writes partial checkpoints under `Library/RiverFoamDiagnostics`; cancellation, disable, destruction, or Play Mode exit preserve completed rows instead of blocking or discarding them. One indivisible bounded operation may exceed the target slice and the final report records the slowest observed slice and its case. D1 remains accepted historical evidence; its synchronous Inspector launch is retired so no legacy long-running diagnostic remains user-launchable without the responsive job contract.


### RIVER-FOAM-TRANSPORT-D3 transport-integrated FCT experiment

D3 tests whether anti-diffusion tied to the actual advective face flux can preserve compactness more honestly than D2's generic post-pass. The experiment adds no new birth, lifecycle, visibility, rendering, cache, topology, or default behavior. Donor Cell and TVD Superbee remain the existing one-dispatch paths. Only the two explicitly experimental FCT selections use the following three-pass material substep:

1. `BuildFctLowOrder`
   - execute the existing Donor Cell packed flux into a temporary low-order state;
   - write signed Lax-Wendroff-minus-Donor Coverage correction mass for each open east/north face;
   - preserve ordinary open longitudinal endpoint outflow in the low-order solve; correction faces exist only between two valid simulation cells.
2. `BuildFctLimiter`
   - derive local Coverage lower/upper bounds from the current cell and open valid neighbours;
   - clamp the upper bound to resolved valid-fluid capacity;
   - accumulate every requested positive/negative correction contribution and calculate `R+ / R-` acceptance ratios.
3. `ApplyFctCorrection`
   - choose one face coefficient from donor-removal and receiver-addition capacity;
   - apply equal-and-opposite packed correction mass to the low-order state;
   - execute the existing transport accounting and lifecycle/topology write exactly once on the final corrected state.

The authoritative equations are:

```text
C_target,f = 0.5(C_- + C_+) - 0.5 λ_f (C_+ - C_-)
D_f = s_FCT Δt u_f L_f (C_target,f - C_upwind,f)
α_f(D_f >= 0) = min(R^-_-, R^+_+)
α_f(D_f <  0) = min(R^+_-, R^-_+)
```

where `D_f` is signed physical Coverage mass, `L_f` is face length, and `s_FCT` is the fixed Low/Medium experiment scale. Interior face application is equal and opposite; final clamping is a numerical guard rather than the intended compactness mechanism.

Conditional performance cost while FCT is selected is four full-field `ARGBHalf` temporary textures and three compute dispatches instead of the single standard transport dispatch for each CFL substep. Donor Cell and TVD Superbee allocate none of those textures and dispatch none of those kernels. Exact GPU time, temporal stability, one-cell thickness retention, centroid error, topology behavior, and packed-state clamp loss remain pending Unity evidence. Failure of both bounded strengths to preserve a one-cell ribbon near its original local thickness without material hiding or topology damage is the escalation gate for geometric/Lagrangian ribbon representation.


## RG-METRIC-P13B — Packet-rearmed birth and object-contact retention

P13B changes automatic Layer C source ownership and the existing obstacle velocity field; it does not change the P13A material packing, transport schemes, Final Visibility modes, Presence Footprint modes, lifecycle, or P12t Chipping.

Automatic-source contract:

- Shore and Free-Water have fixed deterministic slot spacing. Coverage selects a stable share of those slots. Activity is linear from zero to the existing maximum attempt rate.
- Each accepted slot is rearmed only after its event duration plus a distance-derived Minimum Packet Gap. Contact Flecks use per-object active/rearm ownership and cannot start while the same object owns an active contact cycle.
- Current-minus-previous reveal is a permission test; newly reached cells receive the complete current Coverage target. This prevents repainting behind the head without making Coverage depend on reveal cadence.
- Fleck reveal is spatial. Arc/Semi-Arc wakes are one-shot Build products. Only the immediate contact front is refreshed during Hold and progressively withdrawn during Release.
- Flecks are independent of the normalized Arc/Semi-Arc cycle mix and are controlled directly by Fleck Coverage, Activity, and packet gap.

Obstacle velocity contract:

```text
existing RGHalf obstacle texture
R = signed lateral-routing influence
G = independent slowdown influence
```

The existing one-sided collision route remains. A narrow dirty-time all-side contact halo writes slowdown only, allowing front/side/rear Foam to approach the exact authored Minimum Downstream Factor without lateral redirection. No texture, buffer, upload class, compute kernel, shader sample, pass, or draw call is added.

Control cleanup removes automatic-source Breakup Strength authoring and shader evaluation, Lace/Cross-Lace gap masks, Torn Fragment bite masks, Fleck mix weight, and confirmed unused Arc/Semi-Arc arm-reach/lopsidedness controls. Reserved event-record lanes remain structurally present for ABI compatibility.

Expected aggregate runtime work is lower because slots cannot immediately restart, finite packets stop writing behind their heads, and object wake arms are not refreshed after Build. Added CPU work is bounded rearm lookup and obstacle-dirty contact stamping; velocity adds a bounded multiply-and-lerp falloff calculation without a transcendental operation. Measured performance remains pending Unity profiling. P13B offline validation is `28/28 PASS`; Unity compilation, D3D11 import, Play Mode visual acceptance, and profiling remain pending.


### Layer ownership after P13B

```text
Activity attempt accumulator
    -> stable eligible deterministic slot/object
    -> packet-clearance gate
    -> finite reveal event
    -> current-minus-previous permission
    -> full current Coverage target on newly reached cells
    -> P13A Layer C merge/transport/lifecycle
```

Object contact is split explicitly:

```text
Build:   contact front + finite wake arm(s), born once
Hold:    immediate contact front refresh only
Release: progressively withdraw contact-front refresh only
Rest:    source off
Wake:    ordinary transported Layer C material after Build
```

Velocity ownership remains canonical across compute transport and render interpolation. Signed routing influence cannot create slowdown by itself; slowdown influence cannot create lateral routing by itself. This prevents the new contact retention halo from bending Foam around the rear of the object.

## P13C addendum — one-shot Object source ownership and contact retention

Object source ownership is now finite and packet-based:

```text
Arc / Semi-Arc / Fleck Build
    -> newly reached source interval only
    -> authored Coverage + intrinsic material birth
    -> source event completes
    -> no persistent source ownership remains
```

Arc and Semi-Arc no longer have Hold, Release, or Rest phases. Their immediate contact front and finite wake geometry are born during Build and become ordinary transported Layer C material. Apparent persistence must come from explicit topology/lifecycle support and canonical velocity retention, never repeated source writes.

All three Object recipes use one per-object source state and one clearance gate. Completion schedules conservative halo clearance plus authored downstream packet spacing. A Fleck is supplemental in Mixed mode: after one Fleck, an eligible Arc/Semi-Arc opportunity takes precedence before another Fleck may start.

Object-contact velocity uses the existing obstacle texture with independent meanings:

```text
R: signed routing intent
G: contact slowdown influence
```

The canonical resolver computes normal routed downstream/lateral velocity first and scales the complete vector by the contact speed factor. Full influence therefore applies the exact authored minimum speed factor to total motion while retaining routing direction. Full and outer contact reaches are authored dirty-time obstacle-field parameters; no new field or sample exists.

Layer boundaries remain:

- Layer A topology may extend or accelerate Remaining Life but does not emit material.
- Layer B owns both routing direction and complete-vector object-contact speed retention.
- Layer C sources own finite birth only; material transport/lifecycle continue after source completion.
- Layers D/E remain unchanged by P13C.

P13C offline validation: `35/35 PASS`. Unity compilation, Play Mode source/retention acceptance, and profiling remain pending.

## P13D addendum — finite object-contact reinforcement

P13D refines P13C Object Arc/Semi-Arc birth without changing layer ownership.

```text
stroke 0: complete finite Object packet
    contact front + accepted finite wake geometry
stroke 1/2: contact-only reinforcement
    immediate supported front profile only
then: source ownership ends
    Layer B transport + Layer A lifecycle/support own persistence
```

The authored `Object Contact Stroke Count` is bounded to `1–3` with default `2`. Each stroke is a complete progressive Build at the existing Reveal Speed. The event remains finite: there is no Hold, Release, Rest, persistent emitter, or repeated wake ownership.

Stroke transitions are explicit source phases. Previous-deposition subtraction applies only within the same stroke phase; it resets at each phase boundary so a new contact-only sweep cannot be cancelled by the previous stroke ending at progress one.

Layer boundaries remain:

- Layer A support may prolong reinforced contact material but does not emit it.
- Layer B P13C full-vector slowdown may retain the finite contact burst near the obstacle but does not create or refresh material.
- Layer C owns the bounded source burst, intrinsic birth properties, transport, and lifecycle.
- Layers D/E do not create the observed contact holes and are unchanged by P13D.

P13D adds no texture, buffer, event GPU lane, kernel, pass, draw call, or shader sample. Its bounded extra source-raster work is limited to at most two contact-only sweeps per Arc/Semi-Arc event. `36/36` offline validation passes; Unity compilation, Play Mode acceptance, and profiling remain pending.

## RG-METRIC-P13E architecture addendum — released-packet cadence versus contact maintenance

Object-contact Foam now owns two independent clocks because released wake spacing and retention of supported contact material are different responsibilities.

```text
registered object anchor
    -> one shared same-object active owner

full Arc/Semi-Arc packet
    -> P13D finite initial burst
    -> remembers successful recipe + deterministic seed
    -> schedules released-packet clearance at normal downstream speed

contact-only reinforcement
    -> one finite progressive stroke
    -> reuses remembered contact geometry
    -> writes no wake arms
    -> schedules only its own next interval
```

The scheduler attempts full packets first, then due reinforcement while the full packet still waits for clearance, then Flecks. A due full packet blocks reinforcement; P13C Fleck fairness remains within that full-packet opportunity. No two event types may own the same object concurrently.

Full packet clearance is intentionally independent of the contact slowdown halo. The halo is designed to retain contact material, so waiting for that material to leave would couple stronger retention to lower source availability. Released wake-arm length plus the authored packet gap remains the spacing authority for complete packets.

Reinforcement is not a persistent emitter. Each event enters the unchanged 32-slot pool, progresses through one contact-profile reveal, writes only current-minus-previous deposition, completes, and releases its slot. P13A Max + Refresh birth merging may restore lost contact Coverage/Life up to authored values but cannot add beyond those values. P13C complete-vector slowdown and topology support then own persistence.

The CPU event receives one private classification bit; the eight-`float4` GPU ABI remains unchanged. Existing Arc/Semi-Arc phase `1` already means contact-profile-only deposition, so no compute-shader or resource change is required.

## RG-METRIC-P13F architecture addendum — complete contact establishment

P13F keeps Object Foam in Layer C but separates the geometry and duration of the initial packet stroke from later contact maintenance.

```text
Layer C full object packet
    phase 0:
        actual obstacle-boundary ring
        + one-time recipe wake

Layer C later finite strokes
    Arc:
        complete authored Arc contact profile
    Semi-Arc:
        selected authored half-profile

Layer A:
    support/lifecycle only
Layer B:
    complete-vector retention only
Layer E:
    unchanged
```

The obstacle ring is not reconstructed from Layer E and does not use final visibility, Chipping, or Strands. It is born directly into Layer C Coverage from cells immediately adjacent to the existing obstacle-exclusion mask. Locally derived outward normals order the progressive ring reveal from the upstream contact face toward the rear on both sides.

The first stroke and later contact strokes carry independent path lengths and cadence-bounded durations while sharing one requested Reveal Speed. This preserves the P12u metres-per-second contract across geometrically unequal phases. Phase transitions reset only one-shot deposition ownership; they do not create a persistent emitter.

The P13E independent reinforcement scheduler is unchanged. A periodic reinforcement event is still one finite event, but its geometry is now recipe-complete: full Arc profile or selected Semi-Arc half-profile, with no wake arm.

No new persistent field, GPU record growth, kernel, pass, draw call, or final-render sample is introduced.

## RG-METRIC-P13G architecture freeze — accepted source ownership and pause boundary

The post-P13F automatic-source and Object-source architecture is accepted and frozen for the current milestone.

### Frozen Layer C birth ownership

- Automatic sources emit finite packets; completed source paths are not continuously repainted.
- Object Arc, Semi-Arc, and Fleck use one bounded per-object owner.
- Complete Object packets and finite contact-maintenance events are separately scheduled.
- A complete Arc/Semi-Arc packet begins with a narrow ring around the complete actual obstacle boundary and emits its recipe wake once.
- Later Arc contact strokes cover the complete Arc profile. Later Semi-Arc contact strokes cover the deterministic selected half-profile.
- Independent contact maintenance uses the same recipe-complete contact geometry and never emits a wake.
- No Hold, Release, Rest, or material-cadence persistent Object emitter exists.

### Frozen Layer B retention ownership

The existing obstacle field retains its accepted split meaning:

```text
R = signed lateral-routing influence
G = independent object-contact slowdown influence
```

Object-contact slowdown scales the complete routed velocity vector. It does not create material. Layer C topology support and lifecycle remain responsible for how long born material survives.

### Frozen downstream contracts

P13G does not reopen:

- P13A Coverage/Presence/Remaining-Life/Pattern packing and birth merge;
- Donor Cell and TVD Superbee transport;
- Final Visibility or Presence Footprint behavior;
- P12u Reveal Speed;
- P12t Candidate, Eligibility, soft-mask Chipping reconstruction, or Strands.

### External Weather shader integration — complete

Weather cloud shading is integrated through the native URP directional-light cookie path and the focused Arc/Semi-Arc/Foam regression is accepted. The integration did not change Foam source scheduling, Layer C state, topology, cache data, opacity ownership, Chipping, Strands, or simulation cadence.

The later accepted S3.1E.3 shore-wave result also preserves this architecture. Its Length, Gap, profile evolution, analytical shoreline, accent, and edge-blend values are live shared motion/render inputs and do not participate in the immutable Foam grid descriptor.

P13G is documentation-only and has no runtime performance effect. No `PERFORMANCE EXCEPTION` applies.

## Visibility investigation closure and active transport blocker

The earlier Final-versus-Presence/Life discrepancy is closed. The established Coverage, Material Amount, Material State Composite, and Visibility Pipeline Composite demonstrated that the first divergence was the concentration visibility base rejecting low Coverage. Chipping was not responsible. `Lifecycle-Faithful` is the diagnostic authority because it reveals the complete living Layer C footprint.

That exposed the underlying blocker: thin ribbons balloon in Layer C itself. The current work must change transport without hiding material, increasing simulation resolution, adding full-field passes/resources, or increasing total river cost. D4 is the active bounded experiment.



## RIVER-FOAM-TRANSPORT-D4 — zero-regression single-pass transport experiments (historical; superseded by D5)

### Why D4 exists

D1 proved that the accepted scalar finite-volume transport conserves integrated material while widening under-resolved ribbons. D2 proved that generic conservative post-pass compression reduces some spread but introduces centroid shift, neck erosion, and topology damage. D3 tested transport-integrated FCT, but direct Unity Coverage captures showed that both Low and Medium still spread and lower Coverage intensity. D3 also violated the project performance direction by using three full-field dispatches and four full-field temporary `ARGBHalf` textures while selected. The D3 algorithms are therefore rejected on both visual and performance grounds.

The D4 performance contract is absolute:

```text
material dispatches per CFL substep <= TVD Superbee baseline (one)
additional full-field transport textures = zero
serialized default remains Donor Cell
no birth/lifecycle/visibility/topology/cache rewrite
candidate acceptance requires measured GPU mean and P95 <= TVD Superbee
```

D4 deliberately does not add a strength control. It exposes two algorithmically distinct one-pass experiments so visual and cost evidence can select or reject them without producing another tuning surface.

### Candidate A — Bulk-Phase Residual TVD

The resolved longitudinal velocity is decomposed as:

```text
u(x,y) = Ubulk + uresidual(x,y)
```

`Ubulk` is the current configured base Foam downstream speed with river-flow sign. Its displacement is accumulated as one signed scalar phase in cell units:

```text
phase += sign(flow) * Ubulk * dt / dx
integerShift = trunc_toward_zero_when_abs_phase_at_least_one(phase)
phase -= integerShift
```

The existing ping-pong material dispatch reads the old packed state at `destinationX - integerShift`, then applies the existing TVD Superbee face solve only to residual longitudinal velocity and lateral/routing velocity. In open valid water the residual longitudinal component is approximately zero. Near slowdown/contact fields it becomes negative relative motion, allowing obstacle-adjacent material to lag behind the rigid bulk translation without numerically advecting the shared speed every tick.

The fractional phase is not discarded. Previous/current phase values are bound with the previous/current committed textures and applied as separate longitudinal sample offsets before temporal interpolation. Manual and automatic births evaluate their physical longitudinal position with the same current phase, so newly born material remains world-aligned instead of inheriting a presentation offset. The phase remains attached to the persistent state if the user changes transport modes; only the Bulk-Phase candidate advances it. Resource rebuild/reset returns it to zero.

Open-water Motion Lane sampling is streamwise-coherent in this candidate: it samples one shared centre-row lane value across the river width, while existing obstacle routing remains spatially local. This tests whether a ribbon can bend/translate coherently without cross-width lane divergence fanning it into a blob. No lane texture, pass, or dispatch is added.

Known experiment boundaries:

- integer cell transfer is exact-copy rather than conservative geometric remap across changing metric/boundary capacity;
- partial-cell valid-fluid boundaries may clip shifted material;
- the fractional presentation phase adds a small fragment-shader coordinate offset and must be included in the GPU benchmark;
- it is not accepted until births, obstacle lag, reverse flow, endpoint outflow, and previous/current interpolation are visually verified.

### Candidate B — Nearest-Characteristic

The shared base downstream component uses the same scalar phase and whole-cell state shift as Candidate A. The remaining longitudinal/lateral characteristic displacement is converted to an integer source offset through a deterministic low-discrepancy temporal sequence:

```text
dresidual = uresidual * dt / spacing
thresholdAxis = frac((sequenceIndex + 1) * goldenRatio + axisOffset)
offsetAxis = sign(dresidual) * (floor(abs(dresidual)) +
    (frac(abs(dresidual)) > thresholdAxis ? 1 : 0))
stateNext(destination) = statePrevious(destination - offset)
```

Uniform subcell residual motion therefore crosses a cell on the correct long-term fraction of material ticks without requiring a per-cell phase texture. A naive nearest backtrace was explicitly rejected during implementation because the production per-tick displacement is normally below half a cell and would otherwise leave material stationary forever. Coverage, Presence amount, Remaining-Life moment, and Pattern moment move together. Lifecycle still executes exactly once on the final substep. The method intentionally gives up strict finite-volume conservation: multiple destinations can select one source and some sources can be skipped. Its purpose is to test whether stable thin silhouettes and lower texture-read cost are preferable to conservative numerical diffusion for this stylized system.

The candidate adds no texture and no dispatch. Compared with TVD Superbee it removes four face solves, neighbour reconstruction loads, and limiter arithmetic from the hot path. The existing transport accounting will expose duplication/loss as before/after discrepancy rather than concealing it.

Known experiment boundaries:

- divergent/convergent fields can duplicate or lose packed material;
- temporally rounded cell crossings can step or pulse, although the low-discrepancy sequence avoids permanent sub-half-cell stasis;
- obstacle/bank source selection may create holes;
- it is rejected if material gain/loss, jitter, or topology artifacts are more objectionable than TVD spread.

### Responsive validation ownership

The default validation action is now `Transport Quick Gate`, not the exhaustive D2 tournament. It measures the currently selected transport mode for ten seconds using existing steady-state work accounting, then requests the existing Coverage/Visibility diagnostic through `AsyncGPUReadback`. The job changes no setting, blocks no frame, can be cancelled immediately, and produces one copied report containing:

- selected transport mode;
- observed duration;
- one-dispatch/zero-extra-texture structural contract;
- existing material dispatch/cell-iteration/CPU-submission accounting;
- same-state Coverage histogram and integrated material/life evidence.

The D2 exhaustive matrix remains available only as a clearly labelled optional historical post-pass comparison. It is cooperative, pausable, cancellable, and checkpointed, but it does not test either D4 runtime candidate and must not be used as the default D4 gate.

The Quick Gate does not claim GPU time. Final performance acceptance requires identical-state Unity GPU Profiler captures for TVD Superbee, Bulk-Phase Residual TVD, and Nearest-Characteristic. A candidate is accepted only when both mean and P95 Foam cost are no higher than TVD; values within measurement noise are not sufficient evidence.

### D4 pending acceptance — closed by D5 evidence

- Unity C# and compute/shader compilation.
- Forward and reverse-flow phase direction.
- Birth alignment while fractional phase is nonzero.
- State continuity when switching among all four transport modes.
- One-cell horizontal/vertical thickness at birth, 1 m, and 2 m.
- Coverage gain/loss and topology under Nearest-Characteristic.
- Obstacle routing and endpoint behavior under both candidates.
- Quick Gate responsiveness/cancellation/readback completion.
- GPU mean/P95 comparison against TVD Superbee.

No D4 candidate is a production default. `DonorCell` remains the serialized initializer, and TVD Superbee remains the accepted comparison control.


## RIVER-FOAM-TRANSPORT-D5 — Bulk-Phase acceptance and diagnostic consolidation

### Decision boundary

D5 does not introduce another transport algorithm. Runtime evidence rejected `Nearest-Characteristic` completely and showed that `Bulk-Phase Residual TVD` is the only candidate worth finishing. The remaining decision is narrow and factual:

1. does Bulk-Phase preserve the visually accepted thin-ribbon behavior across the current live river;
2. does it stay within the absolute performance ceiling of TVD Superbee;
3. does transport accounting remain inside the accepted conservation and capacity-loss gates.

No spawning, lifetime, Coverage, Presence, Pattern, visibility, topology, cache, or final-render default is changed by this patch. Spawn recalibration remains blocked until transport acceptance is complete, because births were previously tuned around transport diffusion and visibility suppression.

### Rejected branch removal

`Nearest-Characteristic` is removed from the serialized transport enum, Inspector contract text, runtime parameters, C# setup, and compute path. Existing serialized integer value `3` now resolves through the existing fallback to `Donor Cell`; it is not reassigned to another experiment. The following rejected implementation state is removed:

- temporal crossing sequence index;
- nearest-characteristic HLSL helpers;
- nearest source-cell transport branch;
- nearest-specific Inspector and diagnostic labels.

This reduces code and shader branching rather than leaving a failed selectable mode in the production authoring surface.

### One-button acceptance suite

The former per-mode ten-second Quick Gate is replaced by one `TVD vs Bulk-Phase Acceptance Suite` action. One click runs the complete comparison and a second button copies the report.

The suite is a cooperative frame-driven state machine. It contains no synchronous loop, thread wait, sleep, blocking readback, or multi-minute CPU matrix. The Editor remains interactive at every stage, and explicit cancellation restores the authored transport selection immediately while preserving completed blocks.

The sequence is ABBA:

1. TVD warmup A — 3 seconds;
2. TVD measurement A — 10 seconds;
3. Bulk-Phase warmup A — 3 seconds;
4. Bulk-Phase measurement A — 10 seconds;
5. Bulk-Phase warmup B — 3 seconds;
6. Bulk-Phase measurement B — 10 seconds;
7. asynchronous Bulk-Phase Coverage report;
8. TVD warmup B — 3 seconds;
9. TVD measurement B — 10 seconds;
10. asynchronous TVD Coverage report;
11. restore the authored transport selection and finalize the report.

ABBA ordering reduces monotonic scene-drift and thermal-order bias without pretending that the live field is rewound. The suite deliberately does not clear, reseed, hold, or restore the persistent Foam state; it measures the real current scene. The report states this limitation explicitly.

### Evidence captured per block

Each ten-second block records:

- Unity `FrameTimingManager` GPU frame time when supported;
- CPU total, main-thread, render-thread, and Present-wait timing;
- Unity unscaled frame delta as a universal fallback;
- exact material step, dispatch, cell-iteration, CFL/substep, and CPU submission accounting;
- latest packed Amount/Life/Pattern conservation error;
- Capacity/Clamp loss and unit/boundary/obstacle attribution;
- capacity-hit counts.

The last Bulk-Phase block and last TVD block additionally capture the existing non-blocking Coverage/Visibility report. Reports are stored under `Library/RiverFoamDiagnostics/` and are copyable/revealable from the Inspector.

`FrameTimingManager` measures the complete frame, not one compute dispatch. That is intentional: the hard user contract is total river cost, and a transport mode that indirectly increases rendered Foam cost is not allowed to hide behind dispatch-only timing. The report still includes exact structural parity so a whole-frame result can be interpreted correctly.

### Performance verdict contract

Bulk-Phase is accepted only when all of the following hold:

- one material dispatch per CFL substep remains true;
- no additional full-field transport resource exists;
- at least 120 nonzero GPU timing samples are captured for each mode;
- Bulk-Phase P95 GPU frame time is no higher than TVD P95;
- the upper approximate 95% confidence bound for `Bulk mean - TVD mean` is at or below zero;
- transport conservation and Capacity/Clamp evidence is reviewed against the existing targets;
- the already-observed visual compactness remains acceptable.

If GPU timing is unsupported or the strict confidence condition is not met, the suite reports `INCONCLUSIVE`; it never silently promotes the candidate. A measured regression over the one-percent ceiling produces `FAIL`.

### Obsolete test-suite removal

The following transport diagnostics are retired from the Inspector and reduced to tombstones so they cannot be launched or add runtime/editor logic:

- D1 synchronous Ribbon Transport Preservation matrix;
- D2 Conservative Compactness Tournament and cooperative job;
- D4 per-selected-mode Transport Quick Gate.

Their conclusions remain in this canonical document. The unrelated P12 grid/cadence candidate sweep remains because it validates a different architectural decision and is not part of the retired transport-compactness experiments.

### D5 unresolved decision

Only one decision remains: accept or reject Bulk-Phase from the copied D5 report plus the user's visual assessment. If accepted, remove the `Experimental` label in a subsequent freeze patch and then recalibrate births per source family. If rejected, do not revive Nearest, FCT, D1, or D2; return to transport architecture under the same zero-regression ceiling.


## RIVER-FOAM-TRANSPORT-D6 — Bulk-Phase promotion, acceptance freeze, and spawn-pack audit

### Accepted production transport

`Bulk-Phase Residual TVD` is promoted from experimental candidate to the production Layer C transport baseline. Enum value `2` is retained, so existing serialized selections remain valid. The Inspector label no longer says Experimental, and new `StylizedRiver` instances initialize to Bulk-Phase. `Donor Cell` and `TVD Superbee` remain explicit rollback/reference selections.

The accepted D5 evidence was:

- same structural budget as TVD: one material dispatch per CFL substep, one substep at the measured `0.434` CFL, and zero additional full-field transport textures/buffers;
- aggregate GPU mean `3.605662 ms` versus TVD `3.603349 ms`, a measured difference of `+0.002313 ms / +0.064%`;
- aggregate GPU P95 `4.005222 ms` versus TVD `3.985664 ms`, a difference of `+0.019558 ms / +0.491%`;
- approximate 95% mean-difference interval `[-0.011976, +0.016603] ms`; its upper bound is approximately `+0.461%` of the TVD mean, below the project's hard 1% regression ceiling;
- Bulk maximum unaccounted Material Amount error `0.103%`, below the `0.250%` review threshold;
- Bulk Capacity/Clamp loss `0.156–0.174%`, lower than the two TVD blocks (`0.181–0.283%`) even though the old absolute aspirational `0.100%` target was not met by either mode;
- direct Material Coverage review accepted the major visual result: uniform downstream movement no longer balloons thin packets through repeated full-speed neighbour averaging.

This is a user-approved production decision. Do not reopen Nearest-Characteristic, FCT, post-pass compression, D1, or D2 as selectable transport alternatives.

### Diagnostic cleanup

The D5 TVD-versus-Bulk one-time acceptance suite is retired after completing its decision. Its Inspector section and runtime lifecycle hooks are removed, and `StylizedRiverFoamRuntime.TransportQuickGate.cs` remains only as a tombstone. The prior D1/D2 tombstones remain documentary only. The unrelated fixed-metric candidate sweep and automatic-birth reveal-speed report remain because they validate different systems.

### Spawn-pack audit: why the accepted transport reveals oversized packs

The remaining abundance problem is not a reason to weaken Coverage visibility or restore transport diffusion. Current automatic births are intrinsically large and can weld together or continually rejuvenate existing material:

1. Shore sources run at up to `5.0 × Activity` events/s. At the default Activity `0.45`, that is `2.25` attempted starts/s across two banks. Shore Ribbon weight is `0.88`. With Patch Size `0.35`, deterministic event scale is approximately `0.287–0.413`; the authored `2.20–7.00 m` range therefore resolves to roughly `3.58–4.18 m` per ordinary shore ribbon before transport. One packet is already several metres long.
2. Free-water sources run at up to `1.10 × Activity`; default Activity `0.25` gives `0.275` events/s. Mixed weights are Lace `0.30`, Cross-Lace `0.45`, Torn Fragment `0.25`. Lace packets can be `1.40–5.80 m` long, Cross-Lace `0.70–2.40 m`, and Torn Fragments `0.35–1.35 m`.
3. Every eligible static object participates in contact cycles by default because Object Contact Cycle Coverage is `1.00`. Arc packets contain two downstream arms up to `1.80 m`, Semi-Arcs one arm up to `1.35 m`; the default packet contains two finite strokes, and contact-only reinforcement is enabled every `6 s` while the next full packet waits.
4. Packet clearance is source-slot-local. Shore and free-water slots wait for their own completed event plus a distance-based gap, and object anchors wait for their own packet clearance. There is no cross-family or neighbouring-slot packet-overlap gate, so distinct packets may be born into paths that later meet.
5. `FoamMergeBornMaterial` uses `max` Coverage but also applies `max(existing, source)` to Presence and Remaining Life even when the source adds zero new Coverage. An overlapping birth therefore cannot increase Coverage above the larger footprint, but it can rejuvenate the entire existing cell. Repeated contacts can weld separate packets into one long-lived connected pack.

### Highest-impact correction order

The next spawn patch should target local pack size and packet independence, not global river-wide material count:

1. **Added-Coverage-only birth merge.** If `sourceCoverage <= existingCoverage`, preserve the existing packed state instead of refreshing Presence/Life. When source Coverage genuinely extends the footprint, mix only the newly added Coverage fraction into the packed Presence/Life/Pattern moments. This is full-field cost-neutral and directly prevents overlapping packets from indefinitely rejuvenating old connected material.
2. **Cross-source packet isolation gate.** Before starting a new automatic event, compare its prepared longitudinal/lateral envelope against the at-most-32 active automatic event envelopes. Defer the event or scan another deterministic slot when overlap/separation is below a small packet-gap threshold. This is low-frequency CPU work at birth time, requires no field texture/readback, and prevents adjacent shore/free/object packets from being authored on top of one another. It must be benchmarked, but the fixed 32-event ceiling makes the work strictly bounded.
3. **Shorter authored packet envelopes while retaining event population.** Reduce source length maxima/minima, not Activity or global source Coverage. Shore Ribbons are the first target because the normal default packet is about four metres and owns 88% of shore events. Free-water Lace is the second target because one event can reach 5.8 m. Keep thin widths and strong initial material; use more short packets rather than fewer weak packets.
4. **Motion coherence as a separate follow-up.** Bulk-Phase removes dominant downstream numerical spreading, but lateral lane divergence, obstacle slowdown gradients, and routing can still stretch or merge nearby packets. Ordinary open-water lateral intent should remain coherent across packet width; only explicit obstacle routing should create branch separation.

No spawn defaults are changed by D6. The audit establishes the next implementation order so transport promotion and spawn recalibration are not conflated.

## RIVER-FOAM-SPAWN-D7 — packet independence and overlap-safe birth merge

### Accepted transport baseline

`Bulk-Phase Residual TVD` is the production transport baseline. The accepted D5 ABBA evidence showed one material dispatch per CFL substep, no additional full-field transport resource, aggregate whole-frame GPU mean `+0.064%`, P95 `+0.491%`, and an approximate upper 95% mean-regression bound of `+0.461%` versus TVD Superbee. `Nearest-Characteristic` remains permanently rejected. The completed transport acceptance suite and obsolete D1/D2 compactness suites are retired from the Inspector.

### Active problem after transport acceptance

The remaining visual failure is packet scale and packet welding, not a need to hide living material. The desired population may contain many thin pockets, but independent Shore, Object, and Free-Water births must not combine into a few giant connected reservoirs.

D7 therefore changes two ownership rules without editing scenes, prefabs, authored values, simulation cadence, textures, buffers, kernels, dispatch count, or render passes.

### Added-Coverage-only birth merge

The previous Layer C birth merge used max Coverage but independently max-refreshed intrinsic Presence and Remaining Life. A new source could therefore rejuvenate an older cell even when it added no geometric Coverage.

D7 makes the existing scalar-coverage assumption explicit:

```text
addedCoverage = max(0, sourceCoverage - existingCoverage)
```

When `addedCoverage` is zero, the existing packed material state is returned unchanged. When the source genuinely adds occupied fraction, only that new fraction contributes material moments:

```text
existingAmount = existingCoverage * existingPresence
addedAmount = addedCoverage * sourcePresence
combinedCoverage = existingCoverage + addedCoverage
combinedAmount = existingAmount + addedAmount

combinedPresence = combinedAmount / combinedCoverage
combinedLife =
    (existingAmount * existingLife + addedAmount * sourceLife) /
    combinedAmount
combinedPattern =
    (existingAmount * existingPattern + addedAmount * sourcePattern) /
    combinedAmount
```

This is implemented inside the existing `FoamMergeBornMaterial` call sites. It adds no texture read, no dispatch, and no persistent state. Repeated overlap no longer resets the age or strength of the already occupied fraction.

The scalar field cannot distinguish two disjoint subcell shapes with the same Coverage. D7 therefore preserves the existing max-Coverage union model rather than pretending that hidden subcell geometry exists.

### Shared automatic packet-envelope reservation

Per-slot and per-object clearance alone cannot stop neighbouring source identities or different source families from beginning intersecting packets. D7 adds one bounded CPU-side start filter shared by Shore, Object, and Free-Water automatic events.

Before an automatic event is committed, its already prepared geometry is converted to an axis-aligned envelope in river coordinates:

```text
longitudinal = global-distance range + reveal trail + feather + one-cell padding
lateral = prepared source geometry + feather/padding
```

Shore envelopes sample the live left/right shore at the event start, midpoint, and end. Object contact envelopes include object extents, contact padding, and wake-arm reach. Free-Water envelopes use the prepared centre, longitudinal extent, width, and lateral padding.

A candidate is rejected only when both its longitudinal and lateral intervals overlap an active or recently released reservation. The caller continues its existing bounded deterministic slot/object scan, so another independent location can still start during the same opportunity.

Reservations are fixed-capacity (`64`), allocation-free, and checked only at low-frequency automatic start attempts. They are not scanned per cell or per GPU dispatch. A reservation expires after the source event duration plus the relevant authored Minimum Packet Gap converted through the existing downstream-speed clearance contract.

Contact-only reinforcement from the same Object anchor is the sole intentional overlap exemption. The added-Coverage-only merge still prevents it from rejuvenating already occupied material.

### Diagnostics

The existing Birth Activity diagnostic gains one compact read-only row:

```text
Shared Packet Separation:
    active reservations / overlap rejects this update / total overlap rejects
```

No new action, test suite, foldout, readback, or Inspector control is introduced.

### Scene and serialization contract

D7 does not edit `VisualFrameworkDemo.unity`, any prefab, material, asset metadata, or serialized `StylizedRiver` value. Packet-length and pattern-mix recalibration remains a manual authoring step on the existing river instance after the code correction is validated.

### Performance contract

- Material GPU dispatch count: unchanged.
- Full-field texture/buffer allocation: unchanged.
- Simulation cadence and CFL substeps: unchanged.
- Birth-merge HLSL work: replaces max-refresh operations with a guarded moment blend only when Coverage is genuinely added.
- Shared envelope work: bounded CPU comparisons only when an automatic start is attempted; no per-frame allocation and no per-cell work.

D7 must be rejected or revised if profiling demonstrates a measurable one-percent river-cost regression, but its structural work is outside the material transport/render hot path.

## D8.15 historical Shore spawning architecture — superseded by D8.16

This section is retained as historical evidence only. D8.16 supersedes its per-bucket Activity and transition-only Ribbon birth contracts. D8.15 superseded every earlier Stage 6 statement that Shore Coverage permanently selects a subset of shoreline slots or that Shore Activity drives one global fixed events-per-second accumulator.

- Shore Coverage is removed for the complete Shore family. Every valid left/right shoreline scheduling bucket remains eligible.
- The existing `3.5 m` per-bank lattice is internal scheduling/deconfliction only. Bucket count is `2 * max(1, ceil(validFieldLength / 3.5 m))`, so concurrent opportunity scales with river and active-chunk length.
- Each bucket owns an independent deterministic renewal state: cycle index, next-start time, and active event ID. Different buckets may own events simultaneously; one bucket may not overlap itself.
- Activity is the target active-time fraction of each bucket. For event duration `D`, Activity `A`, and packet-clearance time `G`, the next cycle starts after `D + max(D * (1-A) / A, G)` for `0 < A < 1`; zero disables new starts and one leaves only packet clearance.
- Ribbon and Inward Wash retain user-facing Min/Max cell lengths. Event creation chooses one inclusive whole-cell effective length; no user-facing resolved-length control exists.
- Both Shore recipes begin in the nearest valid Foam cell touching the current visible shore. Ribbon has one fixed `1 x 1` birth head.
- Ribbon path authority is the already-built `_FoamCurrentShoreEdgesRead` texture. Each longitudinal path-cell column independently selects the nearest inward row from that current edge, so the head follows the visible bank without a new path buffer, build kernel, readback, or spline solve.
- Ribbon material birth contains only newly traversed cells. Automatic Birth Sources debug separately shows the current persistent head every material tick. No cumulative source body is emitted.
- The source event pool is descriptor-derived as `32 + shoreBucketCount`; packet reservations remain twice event capacity. Allocation occurs only during resource build.

Transport, lifecycle, final Foam rendering, topology, boundary construction, and current-shore-edge construction are unchanged by D8.15.


## D8.16 accepted Shore population and emitter contract

Status: implemented in source; Unity compilation and live validation pending.

- Shore Activity controls a river-length-scaled target active-head population rather than per-bucket duty cycle. The mean target is `Activity * represented bank length / 17.5 m`, where represented bank length is both banks across the active Foam domain. Fractional means resolve deterministically to a stable floor/ceiling target at event-completion or bounded population-cycle boundaries.
- The Inspector reports the predicted floor/ceiling head range, mean, represented shoreline length, chunk estimate, and live runtime population status in Play Mode.
- The scheduler starts at most one Shore event per material tick, never kills an existing event when the target drops, and uses the existing 3.5 m buckets only for candidate distribution and packet-clearance ownership.
- Each active Shore Ribbon performs a one-cell current-head birth attempt every material tick. Existing packet-independent merge semantics fill only missing Coverage; delayed updates additionally emit every skipped path cell.
- The existing `_FoamCurrentShoreEdgesRead` texture remains the only shoreline-path authority. Shore metric/edge lookup uses the unshifted world column while material writes use phase-shifted storage coordinates. No path buffer, path kernel, readback, or extra shoreline solve exists.
- Automatic Birth Sources samples source storage with the current Bulk Phase and composites current source colours over committed persistent Foam shown in dark grey.
- Transport, lifecycle mathematics, merge semantics, final Foam rendering, scenes, prefabs, materials, Object scheduling, Free-Water scheduling, and Inward Wash geometry remain unchanged.

## RIVER-FOAM-VELOCITY-B1 addendum — independent Shore component suppression

Layer B Canonical Velocity now supports two independent spatial Shore controls:

```text
Shore Lateral Movement Suppression:    0..1
Shore Downstream Movement Suppression: 0..1
```

They reuse the existing current Shore Support value in `_FoamTopologySources.b`. No separate Shore velocity mask, texture, buffer, kernel, pass, readback, shoreline evaluation, or source-provenance state exists.

Let:

```text
S  = saturate(Shore Support)
CL = saturate(Shore Lateral Movement Suppression)
CD = saturate(Shore Downstream Movement Suppression)
```

The resolved component-retention factors are:

```text
shoreLateralFactor    = 1 - S * CL
shoreDownstreamFactor = 1 - S * CD
```

After lane/obstacle routing and the existing object-contact full-vector slowdown:

```text
vDownstream' = vDownstream * shoreDownstreamFactor
vLateral'    = vLateral * shoreLateralFactor
```

At full Shore Support, control value one removes the selected component exactly. The Shore Support fade provides the spatial transition toward unaffected interior water. Defaults are zero, preserving pre-B1 behavior.

The controls are spatial Layer B controls. They apply to every persistent Foam packet currently inside Shore Support, regardless of whether the material originated from Shore, Object, or Free-Water birth. Source-specific suppression would require persistent provenance and is outside this architecture.

Compute transport and Motion Field debug modes 5/6 call the same pure resolver in:

```text
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoamVelocity.hlsl
```

Compute samples Shore Support at the same physical motion coordinate used for lane and obstacle fields. Motion Field diagnostics sample the same bound `_FoamTopologySources` field and therefore show the final suppressed velocity directly:

```text
lateral suppression -> red/blue hue collapses toward neutral grey;
downstream suppression -> field brightness approaches near-black;
both at one -> full-Shore-Support cells resolve to zero velocity.
```

The upstream invariant remains authoritative: the pure resolver returns nonnegative absolute downstream speed. In Bulk-Phase transport, a resolved absolute speed of zero becomes the existing negative residual that cancels bulk movement, producing zero world downstream travel rather than upstream travel.

Object Contact slowdown remains independent and multiplicative. Its routing and slowdown field generation, authored controls, and minimum speed factor are unchanged.


## RIVER-FOAM-VELOCITY-B1A accepted addendum — footprint-conservative Shore velocity contact

B1A preserves canonical Shore Support in `_FoamTopologySources.b` exactly and adds no resource. The previously reserved alpha channel now stores an independent Layer B velocity-contact mask:

```text
Topology Sources R = Pressure Support
Topology Sources G = Lee Support
Topology Sources B = canonical Shore Support
Topology Sources A = Shore Velocity Contact Support
```

Canonical Shore Support remains cell-centre sampled and continues to drive lifecycle/topology semantics. Shore Velocity Contact Support is footprint-conservative so a Foam texel whose lateral footprint intersects the current visible water edge participates in Shore velocity suppression even when its centre lies slightly outside that edge.

For cell-centre signed shore distance `d` and local lateral half-cell width `h`:

```text
cellTouchesCurrentWater = d + h >= 0
shoreVelocityDistance = max(0, d - h)
shoreVelocitySupport = cellTouchesCurrentWater
    ? 1 - smoothstep(coreWidth, coreWidth + fadeWidth, shoreVelocityDistance)
    : 0
```

The stored alpha value additionally applies the existing valid-domain and exact obstacle-footprint gates. Compute canonical velocity and Motion Field debug modes consume alpha for Shore component suppression; lifecycle/material-topology consumers continue to consume blue.

This change adds no texture, buffer, kernel, pass, dispatch, readback, shoreline solve, or additional canonical-velocity texture sample. It changes only which channel of the already loaded topology-source texel supplies Shore velocity influence.
