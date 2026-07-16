# Current River Rendering Roadmap

`4.11C.5.17D.1C — Camera-Readable Chip Population` is Unity-validated and accepted. Static Chipping work is closed for the current milestone.

Accepted production sequence:

```text
coherent Foam → analytical Chipping → structural Strands → composition
```

Accepted Chipping controls:

```text
Amount, Size, Spacing, Irregularity, Edge Width, Interior Access
```

`D.1A.1` is rejected and rolled back. `D.1D — Coherent Edge-Bite Admission` is skipped because the current result is good enough. Remaining-Life interaction is deferred as optional future enhancement rather than an active dependency. The zoom-dependent thin-strip capture remains known technical debt and is not blocking completion.

There is no active Chipping patch. `4.11C.5.18E` and the detaching-deposition replacement `4.11C.5.18F.1` are Unity-validated and accepted. The active visual patch is `4.11C.5.18H.6 — Mesh-Fitted Arc Paths and True Half-C Semi-Arcs`. `5.18H.4` is Unity-validated as the accepted no-wrap open-C baseline; the `5.18G` duty-cycle scheduler is retained, `5.18G.1` is superseded, and `5.18H.3` is rejected as the Arc/Semi-Arc bridge authority.

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

## Superseded visual patch — Contiguous Object Face Sweep — `4.11C.5.18G.1`

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

The transport audit still identifies full historical Arc/Semi-Arc repainting as the obstacle-locked wake cause. The first `5.18F` implementation did not reliably remove it: its activation threshold could be unreachable for legal short/wide recipes, preserving the old source mask.

The replacement uses no activation fallback:

```text
Contact Arc       finite centre pulse, then two moving reveal differences;
Contact Semi-Arc  finite shoulder pulse, then one moving reveal difference;
insufficient span finite pulse only;
passed territory  always stops emitting.
```

No new authoring control or runtime resource is added. C# supplies normalized material-step duration through an object-only unused GPU lane so the initial pulse survives the first dispatch without being held for the event lifetime.

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

## Accepted Stage 7 closure correction — `4.11C.5.18C` Contact-Attached Pressure and Thin Birth Sources

Unity-validated and accepted. `5.18C` supersedes the cumulative evidence contract of `5.18B` while retaining its shared automatic-source evaluator and existing debug resources.

```text
Automatic Birth Sources
  latest material update only;
  yellow shore, cyan object, magenta free water, white same-update overlap;
  one live unique-source-texel counter.

Static Pressure
  authored Front Reach in metres;
  requested → longitudinal pressure texels → explicit 0.50-texel raster floor;
  requested and resolved reach reported;
  hidden CPU reach inflation and profile-count reach scaling removed.

Object Foam
  immediate eight-neighbour water shell outside obstacle texels;
  raw unpadded physical extents retained;
  Pressure may orient/weight but cannot add shell occupancy.

Shore Ribbon
  thickness authored in cross-river Foam cells;
  cross-river spacing only for normal width/feather;
  base Source Offset plus bounded cell variation;
  stale Shore Ribbon inward-reach authority removed.
```

No new texture, channel, buffer, dispatch, persistent material state, or free-water source change is introduced. Object contact checks fall from 24 neighbours to 8. The user confirmed the source/pressure/final-render validation gate passed and the result works as intended.

## Final Stage 7 authoring closure — `4.11C.5.18D` Object Birth Control Semantics

Implemented; Unity import pending. This patch does not change accepted source arithmetic or serialized values. It corrects the authoring contract:

```text
Arc/Semi-Arc Arm Reach
  early tangential reveal, feather/profile gates, and local allowance
  inside the fixed immediate contact shell.

Fleck Size
  discrete fleck capsule geometry inside the same fixed shell.

Object Contact Shell
  structural immediate water-cell adjacency; not author-controlled width.
```

The existing Automatic Birth Sources view gains two compact evidence rows for current cell dimensions and raw physical obstacle half-extents. No new diagnostic view or runtime resource is introduced. Stage 7 is formally complete and validated.

# Current River completion state

The current fixed-strength Chipping and structural Strand result is accepted. `5.18E` is validated and fixes the prior lifespan-authoring problem. `5.18F.1` is validated and makes Object Arc/Semi-Arc cohorts detach instead of being continuously repainted at the obstacle. `5.18G` adds the intentional per-object duty cycle. `5.18G.1` proved contiguous phase coverage but its near-ring spatial contract is superseded by active patch `5.18H`. Stage 3 remains fully validated; all unrelated Stage 7 source/contact geometry remains accepted. The former Stage 8 reflection/final-integration queue is stale: the current River has no visible reflection feature and no active reflection plan.

All River performance work is deferred to one later comprehensive pass covering offscreen/empty-field work, cadence, sleeping, shader/runtime cost, diagnostic/readback overhead, chunk policies, and dormant systems together.

# River Rendering Roadmap

## Current Foam completion order — July 2026

P4 accounting is the retained measurement baseline. Offscreen freeze, empty-field sleeping, cadence/readback optimization, shader/runtime cost, chunk policies, and dormant-system cost are deferred to one later comprehensive River performance pass rather than separate patches.

B.2 was rejected because each candidate remained a rigid three-circle cluster. B.2A established one connected contour but its local motion and radius pulse were visually ineffective. B.2B-B.2E proved that nonlinear coordinate warping and one coupled Evolution Amount/Rate could not simultaneously provide stable shape, slow lifecycle, dormant dwell, and independent motion. The surviving B.2F–B.2L lifecycle, rigid movement, projected readability, lateral reach, multi-axis contour geometry, and decoupled timing are retained in the accepted current baseline. D.1A supersedes the old Presence-isovalue Edge Coverage with one canonical derivative-normalized edge/interior helper; D.1B and D.1C complete the accepted authoring and population model.

The Chipping completion ledger is:

```text
2D2B-A.1  irregularize candidates and expose independent selection depths;
2D2B-B    gate candidates with transported Material Pattern and apply production Chip before Strands;
2D2B-B.1  expose independent distribution, size, and shape irregularity controls; lower spacing floor to 0.10 m;
2D2B-B.1A replace the hidden metre-radius cap with a bounded radius ratio, double and centre size variation, and normalize strongly displaced shape lobes;
2D2B-B.2  rejected rigid three-lobe evolution proof;
2D2B-B.2A replace each cluster with one connected contour; evolution visually rejected as tiny and rotation-dominant;
2D2B-B.2B replace local oscillation with chaotic coordinate advection and geometric turnover;
2D2B-B.2D historical body-access foundation — superseded by D.1A;
2D2B-B.2E restore candidate-independent permission diagnostics and attempt bounded turnover/warp safety — superseded for evolution by B.2F;
2D2B-B.2F independent lifecycle, rigid motion, variation, and adaptive complete search — retained in the accepted current baseline;
2D2B-B.2G add bounded singular-value projected-size LOD and replace whole-candidate distance fade with subpixel-only lifecycle suppression — Unity-validated and accepted;
2D2B-B.2H extended rigid lateral travel and adaptive rectangular search — retained in the accepted current baseline;
2D2B-B.2I replace transported Material Pattern ranking admission with a local visible-support/material-depth proxy — rejected after Unity validation;
2D2B-B.2J remove the failed third admission gate and keep Edge Coverage plus candidate-level Interior Access as parallel permissions — Unity-validated and accepted;
2D2B-B.2K replace the weak mirror-symmetric A-to-B contour morph with independent multi-axis sine-harmonic geometry while preserving all size and motion ownership — geometry accepted;
2D2B-B.2L decoupled Shape Change Cadence and Transition Time — retained in the accepted current baseline;
5.17D.1A  canonical zero-resource derivative-normalized Chip Edge Width and consolidated diagnostics — Unity-validated and accepted;
5.17D.1B  replace Radius Ratio and three static irregularity controls with bounded Chip Size and one Chip Irregularity control — Unity-validated and accepted;
5.17D.1C  camera-readable medium/large-biased population and projected-readability admission — Unity-validated and accepted;
5.17C     optional future Remaining-Life interaction; not queued and not required for current completion.
```

Current production uses D.1A's derivative-normalized `Chip Edge Width` plus optional deterministic `Chip Interior Access`; the old Presence-isovalue `Edge Coverage`, `materialEdgeDepth`, Material Admission, and third admission gate are historical and absent. D.1B provides one Size and one Irregularity control instead of the retired radius-ratio and three-irregularity surface. D.1C suppresses the tiny candidate tail after bounded projected enlargement. B.2F–B.2L continue to own lifecycle, rigid movement, readability, lateral reach, multi-axis contour geometry, and decoupled shape timing. Strands remain a separate Layer E structural operation after Chipping. No hidden second Chip pass or dedicated fine-edge system exists.

## Purpose

Define the river as a sequence of independent problems. Each stage is completed, tested, and approved before work begins on the next one.

## Cornerstones

### Configurability

The same system must support anything from a calm, shallow, puddle-like stream to a furious, fast-moving river, as well as fully frozen river surfaces. High-impact controls must be clear in the Inspector, with sensible defaults and advanced settings grouped away from normal styling controls.

### Integrability

Every stage must be designed with later systems in mind. Water body, motion, refraction, interaction, and foam must connect through stable interfaces so later approved work can be added without refactoring completed stages. Reflections are not part of the current production contract.

### Stage-Gated Development

Later effects must not be used to hide problems in earlier stages. Each stage receives explicit acceptance tests and is only considered complete after the result is approved.

### Human-Readable Tooling

Each system needs clear controls, independent enable/disable options, useful debug views, and understandable runtime status.

### Isometric-Camera Suitability

Every solution must be designed and judged for the game's elevated, perspective camera with an isometric-style angle. Techniques that depend on close third-person viewing, shallow camera angles, or details that disappear or break at gameplay distance are not acceptable. Orthographic compatibility should be preserved where practical, but the perspective isometric-style camera is the primary production and acceptance target.

### HLSL-First Rendering

Prefer handwritten HLSL over Shader Graphs whenever practical. Graphs should only be used when they provide a clear technical or production advantage that justifies the additional abstraction and maintenance cost.

---

## Current performance gate — explicit Foam topology preparation

River Foam Play startup is now governed by a cache-only policy. Exact caches install directly. Structurally compatible stale caches may remain visible for one session but are never rebuilt or saved during Play. Missing or incompatible caches leave topology-dependent Foam unavailable with an actionable preparation diagnostic rather than running the expensive topology pipeline.

Cache generation is an explicit Edit Mode operation under the River Inspector. The operation may synchronously scan obstacles and read back the prepared mask because it is deliberate dirty-time work; those operations are forbidden during ordinary Play startup. The former `InitializeOnLoad` polling/persistence workflow is retired.

`4.11C.5.17B.P3` makes that explicit transaction bounded and predictable: one final generated-topology GPU publication, one normal serialization, one storage clone, one direct stored-byte verification, and one save. The expensive repeated-serialization, deserialization, generated-channel, and corruption-rejection proof is now a separate explicit integrity action. Registry notification waves are coalesced and no longer rebuild the shoreline-only boundary texture.

`4.11C.5.17B.P4` adds measurement-only steady-state accounting before any runtime optimization is selected. It reports Layer C commit/substep rates, dispatch and logical cell work, topology refresh/evolution work, asynchronous metrics cadence, empty-field commits, renderer-visible/offscreen frames, and CPU command-submission time. It does not change cadence, sleeping, topology, rendering, or compute kernels.

This River-only performance gate does not correct the River→GeneratedGround restoration cascade. Structural ownership across those features is deferred to a separately authorized cross-feature patch.

## Cross-Stage Foundation Work

**High-performance compatibility:** Visual features may carry meaningful cost when their importance justifies it, but the framework prefers shared fixed-cost representations, quality tiers, culling, sleeping, staggered updates, and lower-frequency simulation whenever they can provide comparable results. Per-effect/per-pixel scaling is avoided where a shared persistent field is practical.

**Generated static-geometry registration and authorship:** Active procedural geometry registers through a neutral event-driven registry. Solid stationary sources expose their final generated mesh and announce geometry changes; river runtimes perform bounds rejection, cache only geometry that touches the river, and unregister automatically with object or chunk lifetime. Generated stationary obstacles need no river emitter component. Their optional authorship is feature-specific: participation, Pressure mode and values, and Wake mode and values. Inherit resolves the detecting river's active defaults, Disabled removes only that source contribution, and Custom replaces only that feature's source values. Dynamic gameplay sources remain emitter-driven. The obsolete Static branch of `StylizedRiverDisturbanceEmitter` has been removed; the component is now dynamic-only. Legacy serialized Static instances remain inert solely as a migration guard and warn that the obsolete component should be removed.

**Generated channel and terrain integration:** A dedicated spline-following corridor generates the riverbed, slopes, shoreline, hidden overlap, collider handoff, and buried terrain apron. It samples an immutable pre-river ground snapshot and matches ground height, slope, normals, UVs, and surface metadata at the handoff.

The visible corridor render mesh exposes a **River Corridor Material Masks** stream in Unity UV channel index `3` / HLSL `TEXCOORD3`. `X` is Riverbed Support: it is `1` on `Centre`, `FlatBedEdge`, and `BedSlope`, and `0` on `HiddenCover`, `OuterBlend`, and `BuriedApron`. `Y` is outward distance in metres from the Riverbed Support boundary. `Z` is corridor-bank validity: it begins on the final BedSlope boundary vertex and remains `1` through `HiddenCover`, `OuterBlend`, and `BuriedApron`. `W` remains reserved zero. Corridor `UV2.y` remains the shore/waterline influence; ordinary Ground writes zero to that component and publishes no UV3 River stream. The Ground-owned property block marks the corridor renderer with the explicit `RiverCorridor` role, which authorizes all River-channel interpretation. These are semantic geometry values only; River code does not own bank or riverbed colour, wetness, smoothness, detail normals, cavity response, or substrate style.

**Natural channel variation:** Deterministic river-space controls provide asymmetric shoreline width variation and safety-limited bed roughness with configurable lower-slope reach. The water surface, corridor, collider, ground concealment, and spatial queries consume the same resolved channel shape.

**Directional-light shadow stability:** The main directional light uses a tested Depth Bias of `0.13`, preventing low-angle light leaks and holes in elongated shadows while keeping acceptable contact and self-shadowing.

## 1. River Domain and Coordinate Contract

**Problem:** Establish one continuous river-space representation for distance along the river, position across it, local direction, width, height, and world-space conversion. Motion must not change speed, reverse, jump, or reveal spline knots.

**Implemented:** An authoritative arc-length-resampled `RiverDomainSnapshot` now provides local, oriented, and connected global distance; lateral position; width; surface frames; projection; bank distance; and inside/outside queries. The surface mesh and terrain carving consume the same domain. The original `UV0` contract is preserved, and `UV1` reserves global/oriented distance and metric lateral data for later systems.

**Validated:** The domain contract validation passed. Constant-speed travel passed across bends and knots, reverse flow passed, spline-knot edits did not introduce movement discontinuities, and connected distance offsets produced the expected global range.

## 2. Water Body

**Problem:** Make the static river body read coherently through colour, depth, opacity, clarity, lighting response, and bank integration. It must support calm shallow water, forceful deep water, and fully frozen river surfaces while exposing stable inputs for all later systems.

**Implemented:** A handwritten URP HLSL compositor combines the already-lit opaque scene with vertical-depth transmission, shallow/deep liquid colour, clarity, tint strength, and independent surface presence. Liquid, Frozen, and Custom states provide separate ice optics through one stable freeze contract. Ambient light, the main sun, shadows, local lights, light-colour influence, and minimum night visibility are configurable. Motion, refraction, foam, and reflection inputs remain explicit and neutral.

**Validated:** Complete and accepted. Liquid and frozen endpoints, shallow and deep bodies, day/night response, local-light response, depth readability, and the elevated perspective isometric camera target passed visual testing.

## 3. Surface Motion and Coherent Flow

**Problem:** Add one coherent downstream motion field whose geometric waves, animated normals, current accents, and shoreline lapping agree on direction and speed. It must remain continuous through bends, knots, width variation, connected distance offsets, reverse flow, and liquid/frozen states.

**Implemented:** A shared river-space HLSL motion layer provides vertical macro displacement, evolving flow-aligned normal detail, directional current accents, visible-shore motion with hidden-edge safety fading, automatic surface refinement, displacement clearance reporting, and a neutral persistent-disturbance input reserved for Stage 5.

**Shared shoreline output — implemented and validated:** The macro-wave, river-space noise, shore-wave profile, and shore attenuation primitives now live in the shared water-motion contract rather than only in the render pass. Stage 6 uses those exact functions to resolve one instantaneous left/right visible shoreline edge per longitudinal topology row by intersecting current positive shore-wave displacement with the corridor's mandatory hidden bank-cover profile. Consumers must not recreate an approximate shoreline rhythm independently.

**Intermediate shore-wave profile controls — implemented and validated:** The accepted repeating Stage 3 carrier remains in place, but the bank-reaching component can now diverge from the centre-river macro wave through seven controls:

- `Shore Wave Height Scale` independently scales vertical bank-wave amplitude;
- `Shore Wave Length Scale` independently scales longitudinal bank-wave length;
- `Shore Wave Reach` limits the fraction of generated hidden shoreline allowance that can be wetted;
- `Shore Wave Transition Length` defines the world-space smoothing span for the within-wave profile and for blends between neighbouring waves with different overall sizes;
- `Shore Wave Size Variation` gives successive travelling waves stable deterministic differences in overall height and lateral reach;
- `Shore Side Asymmetry` blends from shared left/right size and profile values to independent bank values;
- `Shore Wave Profile Variation` creates deterministic variation inside each wave between its start, middle, and end.

Within-wave profile knots use a slope-continuous cubic curve that blends toward a smoother B-spline response as Transition Length increases. Successive wave-size values also blend across that configured metric span. A final zero-slope activation envelope is now applied to the signed shore-wave height near zero crossings and to lateral reach near both the normal shoreline and the maximum hidden-water allowance. This prevents the visible shore from leaving or rejoining either hard bound with a tangent discontinuity instead of merely smoothing the earlier profile values. Size identities are deterministic and travel with the existing carrier; they do not reseed or fluctuate independently at runtime. Left and right profiles are identical when Side Asymmetry is zero and become increasingly independent as it rises. Neutral size/profile variation values preserve the previous wave identities, while Transition Length still controls the new final shoreline onset/exit smoothing. Water displacement, surface normals, liquid refraction motion, instantaneous shoreline resolution, and Stage 6 Shore Support all consume the same shared evaluator. This is an intermediate extension of the existing carrier, not the later explicit travelling-wave-packet redesign; individual packet speeds, lifetimes, births, and independent length evolution remain deferred.

**Validated:** Complete and accepted. The original calm-through-furious motion contract and the seven shore-specific controls passed focused Unity validation, including reverse flow, freeze/thaw, asymmetric banks, transition smoothing, and hidden-allowance limits. Presets retain neutral-compatible behavior. Detached particle effects are not required by the current Stage 3 contract.

## 4. Refraction and Optical Distortion

**Problem:** Distort the riverbed and submerged scene convincingly without doubled silhouettes, invalid screen samples, hard boundaries, or camera-dependent artifacts. Liquid and frozen states need distinct distortion behaviour.

**Implemented:** A configurable depth-aware screen-space layer distorts the complete transmitted opaque scene using separate Stage 3 macro-wave and animated-detail optical fields. It supports restrained liquid refraction, depth influence, shoreline protection, foreground-crossing rejection, static ice warping, and quality-scaled ice diffusion. Liquid surface shadows are subdued so the refracted underwater shadow remains dominant. An optional zero-extra-sample silhouette guard reduces object-edge contraction by reusing the original and refracted depth values.

**Validated:** Complete and accepted. Riverbed detail, submerged objects, and underwater shadows deform continuously with surface motion; liquid and frozen optics, depth protection, shoreline behavior, day/night lighting, and local-light response passed visual testing. A faint gray disocclusion trace can remain around strongly refracted high-contrast silhouettes; it is accepted as a low-priority screen-space limitation.

## 5. Runtime Disturbance and Interaction

**Problem:** Add attached pressure, stationary and moving wake sources, one-shot ripples, downstream transport, spreading, and decay without replacing the Stage 3 base motion field or scaling water-shader cost with active effect count.

**Current status:** The stationary Pressure source, stationary Wake source, and Impact Ripple system are accepted. Their river-level controls are unified and source-agnostic where appropriate. Impact Ripples completed the approved safety/event-contract, local-metric propagation, nonlinear strength, progressive lifetime reservation, cached shore/static-obstacle boundary, and final ridge/override passes. Full relative-motion preparation for dynamic Pressure and dynamic Wake sources remains deferred.

### Canonical feature vocabulary

Stage 5 exposes three user-facing feature groups:

- **Pressure** — attached leading-face water buildup. Registered stationary geometry already implements it; dynamic emitters will later prepare the same visual response from object motion relative to local river flow.
- **Wake** — the attached sheltered lee, rear/side release energy, and transported downstream disturbance. Stationary geometry and dynamic emitters use different source preparation but share the same river-level response, persistent field, transport, widening, geometry, normals, overlap, and decay.
- **Impact Ripples** — one-shot event-driven disturbances such as entry, exit, footsteps, landings, projectiles, attacks, and explosions.

Source type is an implementation and ownership distinction, not a separate river-level visual feature. The Inspector therefore exposes one **Pressure** group and one **Wake** group. The former public **Moving Trail** group has been removed.

Compatibility names remain internally where required:

- `staticPressure...` maps to canonical Pressure.
- `obstructionWake...` maps to canonical Wake.
- Legacy `movingTrail...` fields remain hidden only for serialized compatibility and are mirrored from Wake.
- Existing `StaticPressure...`, `ObstructionWake...`, and `MovingTrail...` API properties remain compatibility aliases.

### Source ownership and preparation

- **Registered stationary geometry:** discovered through the generated-geometry registry; eligible for Pressure and Wake with per-object Inherit, Disabled, or Custom authorship.
- **Dynamic gameplay objects:** owned by the dynamic-only `StylizedRiverDisturbanceEmitter`; submit movement, footprint, source-local strength, and contribution data.
- **Impact events:** emitted through the river runtime's one-shot event API; emitters may request entry and exit impacts, while gameplay systems may request footsteps, attacks, projectile hits, and scripted impacts directly.
- A source must not be owned simultaneously by the registry and an emitter for the same continuous behavior.

The architecture rule is **shared response rules, different source preparation**:

- stationary geometry may use cached mesh contour, support, blockage, and rear-boundary preparation;
- dynamic emitters use swept movement and eventually relative object/water velocity;
- after preparation, both consume the same Pressure/Wake response settings and shared fields.

### Implemented foundation

A river-owned, chunked runtime contains separate representations for Pressure, Wake, and Impact Ripples. Stationary geometry preparation is cached and frame-budgeted. Persistent fields support quality-scaled resolution, chunk activity, sleeping, downstream transport, spreading, decay, and fixed-cost shader sampling. Dynamic emitters support river detection, manual footprints, swept movement submission, and optional entry/exit impact requests.

The stationary Pressure source computes feasible height from flow, blockage, local mesh support, and Stage 3 wave headroom. Strength selects a normalized point inside that range, Contact Sharpness controls the one-sided contact falloff, and Profile Variation controls deterministic lateral redistribution. Profile changes use an independent randomized cadence rather than Stage 3 wave frequency. Cached support preparation uses an adaptive vertical inspection range and 16/32/64 lateral rows selected from disturbance-field coverage. Each prepared row retains upstream and downstream waterline boundaries. The pressure crest and hidden tail are clamped to the upstream half of the row's actual along-flow thickness, preserving the rear half as a pressure-free region for the later lee and wake. Tier-aware inward crest insets are `0.50`, `0.65`, and `0.75` disturbance-field cells for 16-, 32-, and 64-row profiles respectively.

Editor diagnostics report resolved profile resolution, support and multiplier ranges, row classifications, row-thickness range and median, maximum crest and pressure-end depth as a percentage of row thickness, rear-protection clamping, rows entering the protected rear region, and per-row height/contact graphs including downstream and rear-protection boundaries.

The stationary Wake source uses a separate cached source texture and the shared persistent wake field. Pressure and Wake have independent dirty/rebuild ownership, so pressure-profile morphing does not rebake the wake source. Wake chunks reserve each source's resolved reach plus downstream transport headroom; when a source disappears, new injection stops immediately while already-transported energy remains active for a finite source-specific decay period.

The cached stationary Wake source separates a geometry-aware attached lee from rear-corner release energy. The lee follows each lateral row's actual downstream object boundary, uses inward-only contour smoothing and a small hidden overlap, scales its length from local obstruction thickness, and attenuates strongly side-facing rows. It remains attached to the obstruction and produces bounded negative geometry up to `0.20 m` at maximum Strength. Side releases inject positive energy into the persistent field; synchronized sine pulsing was removed.

The persistent Wake field is shared by stationary and dynamic sources. It transports, widens, overlaps, decays, and sleeps independently of source count. Widening (`0.35–1.25`, default `0.65`) controls lateral diffusion. Transported geometry is extracted from a softly saturated compact energy core rather than the complete broad turbulence field. Wake Surface Height supports `0–0.40 m`, and Wake Surface Compactness (`0.80–3.00`, default `1.50`) determines how much of the broad field becomes visible geometry. Spatial lee protection prevents positive trail geometry from erasing the strongest central depression while allowing side trails to broaden and merge after the lee weakens.

Stationary Wake variation uses adaptive 16/32/64-row lateral profiles rather than whole-source pulsing. Per-row lee depth, length, and trailing-edge offsets evolve through smooth deterministic targets. Left and right releases independently vary lateral origin, energy, width, and downstream offset, so newly injected energy forms changing and slightly meandering trail histories. Variation updates at `12 Hz`, remains independent of Stage 3 motion and other rocks, and uses randomized intervals from `0.50–2.00 s` with defaults of `0.60–0.90 s`. `Variation = 0` restores the accepted stable source shape and stops variation-driven rebaking.

Dynamic wake injection now consumes the canonical Wake Strength, Reach, and Spread instead of a separate Moving Trail rule set. Widening, Surface Height, Surface Compactness, persistent transport, geometry, normals, overlap, and decay were already shared. The full relative-motion dynamic source model remains deferred; dynamic movement currently supplies most source variation naturally, while the canonical Variation value remains the common envelope for the eventual completed source preparation.

Wake debugging was consolidated after acceptance. The retained wake-specific views are `Static Wake Source` (legacy debug name for the stationary source texture), `Wake Energy`, and `Final Wake Geometry Height`. High-level runtime diagnostics retain field resolution, simulation rate, active chunks, source count, memory, and sleeping state.

### Validated

**Pressure from registered stationary geometry is complete and accepted.** It passed elevated perspective/isometric tests on small and large generated rocks, adaptive profile tiers, low and exaggerated Strength, minimum and high Profile Variation, independent changing profiles, contact placement beneath the obstruction, and reduced side-face buildup.

A later `Static Pressure Target` debug-view regression exposed two edge cases:

- fixed hidden penetration could extend through thin geometry and approach or reach the downstream side;
- former inward crest insets of `0.75`, `1.00`, and `1.50` cells could bury too much of the ridge beneath medium and large rocks.

The final correction stores each row's downstream boundary and clamps the crest plus hidden pressure tail to the upstream 50% of the row's actual thickness. The inward crest insets were reduced to `0.50`, `0.65`, and `0.75` cells. The user validated both corrections at Medium quality: pressure no longer crossed thin objects, and large rocks regained a readable upstream ridge without recreating a detached mound or downstream pressure.

The temporary 92%/96% support-safe floor and floor-only diagnostics were removed. Useful target, support, profile, thickness, and rear-protection diagnostics were retained. Width-aware multiplier bounds remain part of the accepted implementation. No further Pressure work should be introduced unless a concrete regression or proven shared-field integration defect is demonstrated.

**The stationary Wake source and shared Wake response are complete and visually accepted.** The final result was tested on small, large, wide, thin, angled, and irregular registered rocks. The accepted source topology is a geometry-aware central rear lee plus separate rear-corner releases. The lee visibly depresses the actual surface and remains configurable from subtle to strongly exaggerated settings. Transported side trails visibly raise the surface when Surface Height and Compactness permit it, preserve a protected central depression, widen and decay through the shared field, and respond to Reach, Spread, Widening, Surface Height, and Surface Compactness without returning to periodic pulse trains.

The first scalar variation experiment, which made the whole source brighten, dim, expand, and contract together, was rejected as a visible breathing effect. It was replaced by adaptive spatial lee profiles and independent left/right release trajectories. The replacement produced the approved perception of changing geometry rather than a static hole, static hill, or synchronized heartbeat.

The accepted architecture shares transported Wake geometry between stationary and dynamic sources. Source identity affects injection, not downstream transport or rendering. Attached lee remains a separate local stationary envelope. Existing wake energy is not cleared when dynamic objects cross it, and overlapping energy uses bounded nonlinear geometry response. Obstacle-aware transport is deferred unless testing later exposes a visible ghost wake passing through solid geometry.

The rejected V4 continuously driven obstacle-wave solver is noncanonical and must not be restored.

### Planned visual contracts

**Impact Ripples — approved implementation:** one shared event system with configurable position, radius, signed impulse, initial elevation, analytic shape, sharpness, geometry contribution, and normal contribution. Propagation, decay, flow dissipation, and boundary response remain river-level rules for the shared field. Entry, exit, footsteps, landings, projectiles, attacks, and explosions feed the same solver rather than becoming separate water systems. Detached spray, droplets, and splashes are outside the accepted Stage 5 contract and are not an active requirement.

**Dynamic Pressure/Wake source preparation — deferred package:** design both around emitter-provided movement relative to local river flow. Pressure remains attached to the current object position; Wake persists after the object passes. A source drifting with the current should create little attached pressure, while upstream or cross-current movement should create stronger leading-face pressure. The dynamic Wake source must continue injecting into the accepted shared persistent field and use the canonical Wake controls. Expensive stationary mesh-support preparation must not be rebuilt for moving sources.

### Approved Impact Ripple architecture

Impact Ripples remain one coherent persistent field and one simulation. Entry, exit, footsteps, projectiles, attacks, explosions, debris, and scripted events use reusable event settings rather than separate solvers. The final water shader continues to sample a fixed number of shared fields and never loops over active events.

River-level response owns shared propagation, decay, flow dissipation, shoreline reflection, and obstacle reflection. Event-level settings own world position, radius, signed impulse, initial elevation, analytic shape, sharpness, geometry contribution, and normal contribution. Positive impulse represents water being pushed or struck; negative impulse represents withdrawal or suction. Per-event propagation and decay are deliberately excluded because event identity is lost after overlap in the shared field.

The final advanced solver will replace average-width propagation with cached local river metrics, use time-derived progressive chunk reservations, and add one cached two-channel boundary mask:

```text
R = fluid coverage
G = boundary reflection response
```

Shorelines will progressively absorb most incoming amplitude and return only a weak broad reflection. Registered static obstructions will use a stronger but still damped reflection. The mask is rebuilt only when the river domain, disturbance resources, quality, or participating static geometry changes.

### Impact Ripple implementation passes

#### R0 — Safety and event contract

- Reject `EmitImpact` immediately while the river is fully frozen.
- Defensively discard pending impact commands every fully frozen runtime frame.
- Clear all ripple state on the liquid-to-fully-frozen transition so no event survives thaw.
- Introduce a profile-ready event contract with radius, signed impulse, initial elevation, shape, sharpness, geometry contribution, and normal contribution.
- Preserve the former public `EmitImpact(position, radius, strength, geometry, normal)` overload as a compatibility forwarder.
- Preserve the provisional crater/ring shape at the profile midpoint so R0 does not silently retune appearance.
- Separate dynamic-emitter entry and exit Impact settings from continuous Wake strength and contributions.
- Preserve existing emitter behavior through one-time serialized migration; exit becomes a signed negative event rather than the same positive shape at a hidden multiplier.
- Keep the static-only `12 Hz` optimization only when there are no pending impacts and no active ripple chunks.
- Replace the centre-only test action with configurable longitudinal/across position, signed profile settings, opposite-sign, overlap, and near-shore test actions.
- Report pending commands, impacts injected during the last simulation step, current internal ripple substeps, and the maximum observed during a recent diagnostic window.
- Lifetime-reservation records and their diagnostics begin in R2; R0 must not invent placeholder reservations.

**R0 status:** compiled and focused validation passed. Fully frozen events no longer replay after thaw, signed and independent entry/exit settings work, the configurable test actions work, and the provisional ripple appearance remains acceptable.

#### R1 — Local metric propagation

- Cache two compact `float4` records per longitudinal ripple row: world-space surface centre plus downstream tangent, and local side plus left/right surface widths. Keep companion CPU arrays for precomputed minimum longitudinal/lateral cell sizes used by bounds and stability.
- Reconstruct local tangent direction and world-space neighbour distances in compute without allocating a full two-dimensional position texture.
- Inject event radius in world metres rather than deriving an ellipse from one local half-width.
- Replace the river-wide average lateral cell size in ripple propagation while leaving the accepted Wake and Pressure paths unchanged.
- Derive stability requirements from the smallest relevant cells in active chunks instead of the most restrictive point on the complete river.
- Report metric-row count, the active ripple region's minimum cell size, and whether the bounded substep limit was reached alongside existing substep diagnostics.
- Validate approximately circular world-space propagation through narrow, broad, asymmetric, transitioning, straight, and tightly bending sections.
- Preserve correct radial expansion under reverse flow.

**R1 status:** compiled and focused validation passed. World-space ripple propagation remained approximately circular through the tested river shapes, and reverse-flow behaviour remained correct.

#### R1.1 — Strength-response correction

- Keep the authored Strength range at `0–3`, but resolve it through `s × (3 - 0.4s)` so values around `1` are clearly visible and values from `2–3` remain bounded stress-test settings.
- Apply the same resolved strength to injected height, velocity, normal detail, initial elevation, and the permitted ripple-height envelope.
- Update new-component defaults and disturbance presets for the shaped response; existing serialized raw Strength values remain intact and may require author retuning after the response change.

**R1.1 status:** compiled and focused validation passed together with R2. The nonlinear response made normal values readable while preserving a bounded overload range.

#### R2 — Lifetime and progressive chunk reservation

- Add lightweight active-impact lifetime records.
- Derive bounded lifetime from event magnitude, base decay, flow dissipation, minimum visible energy, and maximum lifetime.
- Use the same flow-adjusted effective decay for visible simulation and CPU reservation lifetime so resource coverage cannot drift from visual fading.
- Expand the reserved interval progressively from propagation radius and downstream advection rather than reserving the full maximum range immediately.
- Keep each activated chunk alive until the latest overlapping reservation ends, then allow the existing chunk sleeping path to clear it.
- Use an analytic visible-energy threshold rather than GPU readback: after impacts overlap in the shared field, individual event energy can no longer be identified reliably without adding another field or reduction pipeline.
- Prevent fast flow or slow decay from clipping ripples at chunk boundaries.
- Report active reservation count, longest remaining reservation, resolved Strength, and effective decay.

**R2 status:** compiled and focused validation passed together with R1.1. Progressive reservations remained bounded, avoided visible chunk clipping, adapted to live flow/decay changes, and returned to sleep after activity ended.

#### R3 — Shore and static-obstacle boundaries

- Build one cached `RGHalf` two-channel ripple boundary mask at ripple-field resolution: red stores fractional fluid coverage and green stores reflection hardness.
- Generate a metric-aware shoreline absorption band from each row's real left/right surface widths. The band progressively removes amplitude before terminal contact instead of treating a sloped bank as a vertical wall.
- Rasterize participating registered stationary geometry from its cached waterline contour into the same mask. The rasterization is event-driven and rebuilt only when the river domain, quality/resources, or registered static geometry changes.
- Use damped reflective finite-difference neighbour sampling instead of the former lateral edge fade. Shore Reflection remains deliberately weak; Obstacle Reflection may be stronger but remains lossy.
- Suppress injection in solid cells, prevent propagation through registered solids, and clear existing ripple state inside cells that become solid after a mask rebuild.
- Add independent generated-geometry `Impact Ripple Collision` authorship with `Inherit` and `Disabled`; it does not alter Pressure or Wake participation.
- Expose river-level Shore Reflection and Obstacle Reflection controls, high-level mask/source diagnostics, and one compact `Ripple Boundary` debug view.
- Avoid per-frame collider scanning, a second mask, GPU readback, and per-obstacle loops in the final water shader.

**R3 status:** complete and accepted. The combined shore/static-obstacle mask compiled after the D3D11-safe scalar signed-distance hotfix. Focused and extensive Unity testing confirmed damped shore interaction, stronger but lossy obstacle interaction, static-solid blocking, correct reverse-flow behavior, stable reservations/sleeping, and no reported Pressure or Wake regression.

#### Final ripple visual and authoring pass

- Added river-level `Ridge Emphasis` (`0.75–1.50`, default `1.15`) that affects only the positive ridge height, outward ridge velocity, and normal-detail ridge. It does not deepen the centre, change event radius, propagation, decay, reflection, or initial elevation.
- Narrowed the analytic ridge width slightly so its edge reads more clearly without turning the result into a rigid ring.
- Extended Impact Ripple Strength from `0–3` to `0–4` while preserving the accepted nonlinear mapping through `3`.
- Added an explicit overload segment from `3–4`: Strength `3` still resolves to `5.4`, while Strength `4` resolves to `9.0` for exceptional override-style events.
- Preserved the existing `0.28 m` maximum-height ceiling through Strength `3`; the overload segment progressively unlocks up to approximately `0.45 m` at Strength `4`.
- Updated disturbance presets to use Ridge Emphasis values of `1.05` for Subtle, `1.15` for Balanced, and `1.25` for Reactive.

**Impact Ripple status:** complete and accepted after compilation, focused checks, and extensive user stress testing. R4 and R5 were collapsed into this focused finalization because the shared analytic profile, signed overlap, metric propagation, lifetime reservations, frozen-state handling, boundary interaction, quality behavior, and combined Stage 5 coexistence were tested sufficiently. Detached spray, droplets, and splash particles were not used to conceal ripple defects and are not an active Stage 5 requirement.

### Stage 5 closure

**Stage 5 is closed for the current gameplay milestone.**

- Stationary Pressure is complete and accepted.
- Stationary source preparation and the shared persistent Wake response are complete and accepted.
- Impact Ripples are complete and accepted.
- Full relative-motion preparation for dynamic Pressure and dynamic Wake is explicitly deferred until an authoritative movement/velocity system and real moving-water gameplay consumers exist. The current dynamic emitter path remains compatibility and foundation work, not a completed production movement model.
- A lightweight Inspector workload pass was completed. In a representative scene the accepted disturbance fields reported approximately `0.39 MB` allocated memory at the captured state, and deliberate user stress testing remained bounded at roughly `48,000` estimated cell-iterations. These are internal workload estimates rather than measured CPU/GPU milliseconds; a broader hardware profiling pass remains a future whole-project optimization milestone.
- Stage 6 may consume the accepted Pressure, Wake, Ripple, boundary, river-domain, freeze, chunking, and diagnostics contracts without reopening their visual behaviour.

## 6. Foam and Surface Tracing

**Problem:** Build the stylized surface-film/Foam layer: broad broken sheets, contour ribbons around darker water pockets, temporary connectors, shore/obstacle skirts, peeling strips, detached fragments, chipping edges, thin fast white streaks, and disturbance-reactive organic motion. The system must work from the elevated perspective camera and remain performance-safe across active river chunks.

**Canonical document:** `River_Foam_Stage6_Architecture.md` owns the Foam architecture. It is now the guiding source of truth for data ownership, dependencies, allowed/forbidden reads, visual target decomposition, debug requirements, rejected approaches, and implementation sequence. `River_Foam_Active_Blockers_and_Next_Patches.md` owns only the current recovery queue.

`4.11C.5.11` tested the first post-baseline Layer D local procedural breakup probe. Validation rejected it as the fine-fragmentation solution: it was active, but the removals were cell/ribbon-shaped because `_FoamShapeMask` is the wrong resolution for atomic detail. `4.11C.5.11B` retires that code and restores the clean pass-through Layer D baseline. Fine breakup now belongs in Layer E shader composition; Layer D should focus on macro sheet/contact/bridge structure.

### Current status after `4.11C.5.16E.2`

The accepted state is restored to `5.16E`: both Final Foam visibility policies remain, support aging may reach `0.05`, and Layer C/Layer D ownership is unchanged.

`5.16E.1` face-consistent residual gating and released object/shore source formation failed Unity validation. Source disabling did not stop constrained foam stutter, and the released sources produced an unwanted grey-interior/white-fringe result. The failure confirms that conservative redistribution beside blocked faces cannot be reconstructed by a single point-velocity render offset.

The no-allocation audit proved that residual-predicted Final Foam alone stuttered while both committed and evaluated previews were stable. `5.16E.2` therefore promotes committed Layer C presentation to normal Final Foam and retires render-only residual point-velocity backtracing. Evaluated shape remains diagnostic-only.

### Stable foundations

Accepted/stable foundations:

- persistent material state with `Presence`, `Remaining Life`, and `Material Pattern` semantics;
- source-only `Amount` and source-to-persistent merge rules;
- topology lifespan support and negative aging pressure;
- lifetime delta-time rebind, support/negative aging response repair, and precision-safe once-per-material-tick aging at a `0.05` supported minimum;
- conservative local 2D packed-state transport through the canonical velocity;
- 5.9n persistent morph cleanup in the compute/simulation path;
- 5.9p lateral commit shredder disable;
- Motion Field ownership derives from committed `Presence` and uses the shared meaningful-Presence visibility gate;
- Motion Field + Cell Grid debug implemented;
- stale Surface Morph Strength UI/control surface quarantined;
- `_FoamShapeMask` and `Foam Evaluated Shape` debug implemented;
- Stage 2/Layer D time binding corrected so animated visual shape work can use current frame time.

### Rejected/superseded active paths

Rejected or superseded as active planning direction:

- 5.5-5.7 stored-state morphing proved the need for living shape behavior, but its neighbour-resampling implementation is rejected because it acted as hidden material transport;
- 5.8 chaotic drift proved the need for body-scale lateral motion, but the implementation is rejected because it lived as hidden morph/movement authority;
- 5.9 fractional lateral row weighting is rejected because it smeared and pulsed material;
- 5.9 per-cell stochastic/source-owned row commit is rejected because it shredded foam into ribbons;
- 5.9y dense interior holes are rejected because they produced marbled/scratched interiors unlike the reference river;
- 5.9z coordinate warp is rejected and retired because it produced numeric differences without useful visible structural change and cannot create structural sheet/bridge/pinch behavior by itself;
- 5.11 Layer D local procedural breakup is rejected and retired because it produced visible but cell/ribbon-shaped removals; fine breakup belongs in Layer E shader composition at rendered-pixel scale;
- naive multi-radius edge classification is rejected as a default: radius 1/3/5 box sampling costs `179` samples per cell, about `2.93M` samples for a 128×128 field evaluation;
- final shader macro stretch/warp must not be treated as the source of broad Foam structure;
- 5.16D–5.16D.2 occupancy-native macro breakup is rejected and retired; it produced weak-film suppression rather than coherent tears;
- a separate persistent damage texture/channel is rejected;
- pocket IDs, connected components, and foam entity databases remain rejected unless explicitly reopened.

### Canonical architecture summary

Layer ownership:

1. `Layer A — River Domain`: river coordinate basis, valid fluid, boundary/shore mapping, material UV contract.
2. `Layer B — External Influence Fields`: foam-agnostic support/contact/motion/exclusion/wake/pressure fields. May feed Layer C and Layer D. Must not read Layer C or Layer D.
3. `Layer C — Persistent Foam Material`: durable `FoamState`; only owner of real material birth, death, life, and movement.
4. `Layer D — Visual Foam / Film Evaluation`: Film Source, Film Support, advected temporal occupancy, and `_FoamShapeMask`. May read C; must not write C.
5. `Layer E — Shader Composition`: final color/opacity/edge softness/local breakup/thin streaks/debug pixels. No feedback.
6. `Layer F — Scheduling, Quality, Debug`: dispatch order, quality tiers, allocation, binding, labels, debug views. No behavior ownership.

The condensed stage rule remains:

```text
Stage 1 / Layer C = persistent material truth.
Stage 2 / Layer D = visual film structure.
Stage 3 / Layer E = shader polish and local detail.
```

### Current recovery sequence

The modern execution order is:

1. Layer A/B/C ownership and diagnostic correction — complete.
2. Layer E rendered-pixel detail proof — complete as debug-only `5.12`.
3. Layer D Film Source / Film Support — complete through `5.13D`.
4. Layer C source population families — complete provisionally through `5.15B.3.1`.
5. Canonical velocity diagnostics — accepted in `5.16A.1`.
6. Conservative local 2D Layer C material transport — accepted through `5.16B.1`.
7. Advected Layer D temporal occupancy and debug-footprint consistency — accepted through `5.16C.1`.
8. Occupancy-native macro breakup — rejected and removed through `5.16D.R`.
9. Final Foam visibility-policy A/B plus low-rate aging precision — implemented in `5.16E`; both visibility policies retained.
10. Released source formation plus face-consistent residual advection — rejected and removed through `5.16E.1R`.
11. Zero-memory presentation audit — passed; only residual-predicted Final Foam stuttered.
12. Committed Final Foam promotion and residual prediction retirement — Unity-validated and accepted in `5.16E.2`.
13. River Inspector and diagnostics redesign — Unity-validated and accepted through R1–R5; canonicalized and temporary plan retired in R6.
14. Transport Presence capacity-loss attribution — Unity-validated and accepted in `5.16E.3`.
15. Capacity audit closure and deferred sub-1% PoC limitation — accepted in `5.16E.3C`; original `0.10%` target retained, temporary review threshold `1.00%`.
16. Layer E final-rendering contract lock — accepted in documentation-only `5.17P`.
18. Decide whether `_FoamShapeMask` production integration is still required after Layer E comparison.
19. Reopen capacity correction only if the review threshold is exceeded or visible loss appears.
20. Formal performance tiers, active-chunk scheduling, and profiling gates.

Final Foam reads committed packed Layer C material without residual backtracing. The `5.16E` A/B visibility control remains an artistic choice. `_FoamShapeMask` remains diagnostic-only through Foam Evaluated Final Preview.

## Approved Layer E finishing contract — `4.11C.5.17P`

The inspiration comparison is refreshed before final rendering work. The production river is not expected to copy the reference one-to-one. Its accepted macro result already contains the required family resemblance: broad predominantly horizontal bands, lateral travel, split/merge behavior, obstacle-driven convergence, and stronger shore accumulation. Slightly fatter ribbons and greater bank accumulation are acceptable consequences of the current field resolution and source grammar. Layer C and the existing motion system remain the macro authority.

The remaining target is local rendered character: foam should read as pale, substantially opaque, chipped, structurally stranded, and energetic while preserving the accepted macro ribbons.

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

### Historical — `5.17B.2 — Pre-Hardening Binary Edge Cuts`

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

The original periodic Strand path and the partial-presence Edge Fragmentation path are retired. The Strand authoring group remains; Edge Fragmentation authoring is removed.

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

D1A removes the failed reconstruction and is Unity-validated for exact equivalence.

### `5.17B.2D1B — Strand Shaping and Projected Detail Floor`

D1B is visually rejected and superseded. Spacing and Width collapsed into inverse occupancy controls, Curvature had negligible visible authority, and the source fallback did not prevent one-pixel line noise. D1D replaces this schema.

### `5.17B.2D1C — Strand Spatial Controls and Resolution Cutoff`

D1C is visually rejected. The control schema promised geometric Spacing, Width, and Curvature, but the extracted effect has no explicit centreline/phase representation.

### `5.17B.2D1D — Strand Control Model Reset and Coherent Pattern Transport`

D1D is Unity-validated and accepted. It gives the independent Strand path Strength, hierarchical Scale, candidate Density, inward Reach, coherent pattern transport, and Material-Pattern-aware projected filtering without texture, persistent field, compute work, or Layer C/D mutation.

### `5.17B.2D2 — visually rejected`

D2 still thresholded procedurally eroded coherent visibility and therefore produced nested internal iso-contours instead of geometric edge-connected breakup. The failure was architectural, not a calibration issue.

### Lifetime and topology rule

Layer C remains the sole owner of Remaining Life. Support and Negative Topology influence Foam only through the accepted Layer C aging rules unless a future separately approved experiment proves another dependency is necessary. Layer E must never modify Remaining Life.

Remaining-Life modulation of Chipping or Strands is optional future work and is not queued. The former `5.17C` progression plan and `5.17D` fine-fragment/final-energy queue are retired from the active roadmap. Dedicated Fray and micro-fragment systems are not required for the production camera.

### Performance contract

```text
new persistent textures / fields / channels = 0
new compute kernels / dispatches / readbacks = 0
cost location = fragment shader only
wide neighbourhood sampling = rejected by default
```

Reuse the existing shader-detail probe and available samples where practical. Profile before accepting any broad sampling stencil.

### Public workflow and debug requirements

The accepted River Inspector contract is compact and closed by default. Production authoring is grouped by feature; Foam authoring follows Layers A–E. Debug presentation, read-only runtime telemetry, generated status, and mutating actions are separate top-level responsibilities.

`Debug Views` is the only authoring surface for Water Body, Surface Motion, Refraction, Disturbance, and Foam debug substitutions. It applies exclusive selections over the existing serialized enum fields, reports legacy conflicts without silently changing them, and follows the shader priority:

```text
Foam > Disturbances > Refraction > Surface Motion > Water Body
```

The hub provides `Normalize to Rendered View` and `Reset All Debug Views`.

Foam views are grouped by architectural ownership:

```text
Layer A — Topology
  Foam + Aging Topology

Layer B — Velocity
  Foam Motion Field
  Foam Motion Field + Cell Grid

Layer C — Persistent Material
  Material Presence
  Material Remaining Life
  Automatic Birth Sources

Layer D — Primary
  Foam Evaluated Shape
  Foam Evaluated Final Preview

Layer D — Advanced Internals
  Foam Film Source
  Foam Film Support
  Foam Instantaneous Film Target
  Foam Temporal Occupancy

Layer D — Comparisons
  Foam Shape Difference
  Foam Temporal Difference

Layer E — Rendering
  Foam Chip And Strand Probe
  Foam Chip And Strand Difference
```

Every view must identify whether it displays persistent material truth, an external influence field, a Layer D helper/product, or final shader output. A raw-material diagnostic must not reuse final `foam.mask`.

`Runtime Diagnostics` contains labelled, selectable, non-editable, stable-height rows. Foam diagnostics are grouped by Layers A–D, with transport accounting under Layer C. Pending asynchronous readback and unavailable runtime states change values, not row count. Constant Inspector repaint is allowed only for a visible live Disturbance or Foam diagnostic leaf in Play Mode with one selected River.

All generation, validation, cache, test-source, lifecycle-probe, clear, and reset operations live under `Actions`. They must not be mixed into production authoring or read-only diagnostics.

### Performance constraints

- active work is per active river/chunk;
- inactive/frozen/culled chunks should avoid unnecessary simulation and visual-film work;
- no per-event GameObjects or steady-state managed allocations;
- no runtime CPU Foam-cell loops;
- no GPU readback;
- no runtime obstacle search in Foam simulation;
- no pocket/entity database by default;
- no full-res wide-radius neighbourhood classifier as default;
- no shader-side wide-neighbour structural search;
- Layer D broad structure should use compact grid products and quality-tiered update rates;
- Layer E shader work should stay local unless a separately approved profiling case proves otherwise.

For one High-quality 32 m chunk, the target future Layer D cost is roughly `175k–240k` reads/update and about `28k` writes/update, usually at `16–24 Hz`, not 60 Hz by default.

### Failure rule

When Foam looks wrong, diagnose product/stage boundaries before adding features: River Domain, External Influence Fields, Persistent Foam Material, Visual Foam/Film Evaluation, Shader Composition, then Scheduling/Debug.

Do not compensate for broken material behavior with automatic births, topology painting, opacity tuning, final-shader macro stretch, or unrelated water-architecture changes.

## 7. Secondary Water Effects

**Status:** Provisionally complete for the current milestone. The existing River, disturbance, shoreline, and Foam result supplies the required secondary visual read at the production camera. There is no broad mandatory queue for splashes, droplets, spray, mist, caustics, wetness bands, or other previously listed effects.

One targeted user-directed Foam/secondary-water tweak remains before formal closure. Any additional secondary effect requires a new explicit visual target, cost budget, and approval rather than continuation of the old checklist.

## 8. Reflections and Final Integration — stale roadmap item

The former Stage 8 plan is retired from the active roadmap. The current River has no visible reflection feature, and reflection work is not required for the current milestone. Any dormant or experimental reflection implementation is not part of the accepted production contract.

Future reflection work, if desired, must begin as a new explicitly approved feature with a defined visual target, integration plan, and performance budget. Final integration of the accepted River stages is already handled by the current water compositor and does not require a separate Stage 8 queue.

---

## Working Rule

Before implementing a stage or sub-feature, define its acceptance tests. After approval, record a conservative summary under **Implemented** or **Validated**. Later work may consume earlier outputs, but it must not change an approved feature's contract unless that change is discussed and approved first.

### Foam historical correction note

The detailed 5.9e-5.9n lateral-transport notes that previously lived here are no longer active planning guidance. Their results are summarized under Stage 6 above and in `River_Foam_Stage6_Architecture.md`.

Current correction:

- the Motion Field remains valuable as a Layer B external influence/debug field;
- current Layer C lateral material transport is disabled;
- fractional lateral row weighting is rejected;
- per-cell stochastic/source-owned row commit is rejected;
- neighbour-sampling persistent morph is rejected;
- future real lateral material movement must be redesigned under the Layer C Persistent Foam Material contract;
- future broad bending, tearing, bridge/pinch support, and joining appearance belong to Layer D Visual Foam / Film Evaluation;

## Stage 6 Foam update — 4.11C.5.13

The first structural Layer D film-support pass now exists. Runtime Foam can build half-resolution `_FoamFilmSource` and `_FoamFilmSupport` fields and use them to produce `_FoamShapeMask`. This remains debug/product-only; Final Foam still uses the legacy shader path until Layer D is visually accepted. The rendering roadmap remains unchanged: broad film structure belongs to Layer D, sub-cell breakup/thin streaks belong to Layer E, and Final Foam should switch to `_FoamShapeMask` only after validation.

## Stage 6 Foam update — 4.11C.5.13B

Layer D Film Source, Film Support, and _FoamShapeMask are now explicitly domain-space visual products. Persistent FoamState remains material-space and is phase-corrected when Layer D reads it. Shader debug views for Layer D products now sample with fieldUV rather than materialUV, preventing Film Source / Film Support / Evaluated Shape from pulsing with the material cell-grid residual phase. Final Foam remains unchanged.

## Stage 6 Foam update — 4.11C.5.13C

Layer D Film Source is now material-gated. The previous 5.13/5.13B formula allowed Layer B support/topology to seed visual Film Source directly, which made Foam Film Source, Film Support, Evaluated Shape, Shape Difference, and shader-detail probes reproduce support topology shapes. The corrected rule is: persistent material creates Film Source; external support/contact/topology may bias or suppress material-derived source and spread, but cannot create visual film from zero. Final Foam remains unchanged.

## Stage 6 Foam update — 4.11C.5.13C validation and 5.13D planning

`4.11C.5.13C` has now been Unity-validated. The support-topology contamination is fixed: Film Source no longer displays generic support topology, and Film Support / Evaluated Shape now derive from material-fed source rather than topology-only source. The remaining issue is visual quality, not architecture: the current Film Support spread is too blunt and capsule-like around the material ribbon.

`4.11C.5.13D — Layer D Film Spread Shape Tune` has now been implemented as compute-only Layer D tuning. It narrows support bias, weakens and gates cross-flow spread, tightens bridge/fill behavior, and lowers final Film Support dominance in `_FoamShapeMask`. It still must not be treated as Final Foam integration: Final Foam remains unchanged until Layer D is visually accepted.

## Stage 6 Foam update — 4.11C.5.13D

`4.11C.5.13D` tunes the existing Layer D Film Source / Film Support / Evaluated Shape formulas without changing the architecture. Support/contact/topology still cannot create Film Source from zero material. Final Foam still does not consume `_FoamShapeMask`.

The compute pass now uses a narrower support-bias range, weaker and source-gated cross-flow spread, stricter bridge thresholds, lower bridge contribution, and a more conservative `supportShape` contribution in `EvaluateFoamShape`. The intended result is a Film Support field that remains broader than Film Source but is less uniformly inflated and less capsule-like.

Validation should compare `Foam Film Source`, `Foam Film Support`, `Foam Evaluated Shape`, `Foam Shape Difference`, and `Final Foam`.

## Stage 6 Foam update — 4.11C.5.14A

`4.11C.5.14A` begins the automatic source-population phase without adding a new visual-film authority. The audit result is that manual/progressive birth, support/lifetime capture, topology/contact fields, and Layer D material-derived spread already exist; the missing piece is automatic material birth near specific supported places.

The patch adds a disabled-by-default **Automatic Source Population** inspector foldout and the first conservative source class: sparse shore/contact births. These births create real persistent FoamState material through the existing `PendingInjection` / `QueueMaterialBirth` / `InjectFoam` path. Support topology then preserves or suppresses the born material through the existing Remaining Life rules.

The patch deliberately does not add environmental/contact film as a separate visual product, does not switch Final Foam to `_FoamShapeMask`, and does not allow support/topology to render as foam from zero material.

Validation should enable Automatic Foam Birth and compare `Material Presence`, `Material Remaining Life`, `Foam Film Source`, `Foam Film Support`, `Foam Evaluated Shape`, `Foam Shape Difference`, and unchanged `Final Foam`. The original `Shore Contact Birth Amount` and the bloated `5.14B` low-level controls are superseded by the `4.11C.5.14C` shore controls.

## Stage 6 Foam update — 4.11C.5.14B / 5.14C

`4.11C.5.14B` was a spawning-control pass. Validation of `5.14A` proved automatic shore birth and support capture work, but also showed that the old `Shore Contact Birth Amount` slider produced oversized blocky chunks because it controlled density, footprint, material amount, initial life, elongation, and compound shape at once.

`5.14B` correctly introduced source-population presets and source-class-specific spawning, but it exposed too many low-level shore controls. `4.11C.5.14C` simplified the shore UI, but validation showed the hidden implementation was too sparse and same-shaped. `4.11C.5.14D` replaces one-shot shore strokes with deterministic full-strength shore source events.

The current shore controls are:

```text
Coverage
Activity
Patch Size
Pattern
```

The implemented patterns are `Mixed`, `Shore Ribbons`, and `Inward Wash`. The active implemented source class is still shore/contact only; River Body, Obstacle Contact, and Lee/Wake presets are documented placeholders and intentionally do not spawn yet. Final Foam remains unchanged.

## Stage 6 Foam update — 4.11C.5.14D

`4.11C.5.14D` keeps automatic source population in Layer C and rewrites shore birth as deterministic source events. The patch explicitly rejects a many-faint-deposits accumulation model. Instead, bounded deterministic shore slots start normal-strength progressive source events that reveal their area spatially over time.

Two shore event recipes are implemented first:

```text
Shore Ribbon   bank-parallel opaque material source event
Inward Wash    shore-attached inward/downstream source event
```

Both recipes create real persistent `FoamState` material through the existing progressive composition / injection path. Support topology still only affects survival/capture after material exists. Layer D and Final Foam integration are unchanged.

## Stage 6 Foam update — 4.11C.5.14E

`4.11C.5.14D` failed visually because automatic shore events still emitted generic progressive segment injections. `4.11C.5.14E` replaces that output path with a dedicated typed automatic source-event rasterizer. The new kernel reads live current shore edges, evaluates shore-local analytic masks, and writes real Layer C material with `FoamMergeBornPresence`.

The user-facing source controls stay unchanged: Coverage, Activity, Patch Size, and Pattern. The internal shape vocabulary now has real `ShoreRibbon` and `InwardWash` event types instead of two recipes that merely changed numeric parameters on the same capsule stamp. Layer D formulas and Final Foam integration are unchanged.

## River Foam 4.11C.5.14F update

The automatic source-event rasterizer remains the selected foundation. 5.14F adds formation kinematics and stroke-style Inward Wash behavior:

- Added Shore Foam Formation Speed as a high-level authoring control.
- Replaced fixed source durations with distance / speed timing.
- Reworked Inward Wash from a filled tongue into a moving curved stroke-head to prevent blob accumulation.
- Left Final Foam and Layer D composition untouched until Layer C source material is visually accepted.

Next after validation: if shore ribbons and inward strokes are accepted, expand the same source-event vocabulary to open-water streamlines and sheet borders, because the inspiration river contains interior white threads in addition to bank-attached foam.

## River Foam 4.11C.5.14G update

The next shore-spawning refinement after 5.14F is `4.11C.5.14G — Shore Wash Stroke Refinement`.

Resulting direction:

- Keep Formation Speed; it solved the event-pop speed issue well enough for now.
- Keep Shore Ribbons mostly stable.
- Reduce `Inward Wash` from a broad source body into a smaller shore-detachment stroke.
- Protect `Mixed` by making Inward Wash occasional until pure Inward Wash is acceptable.

This remains a spawning-only patch. Static-object spawning, free-water spawning, foam evolution, Layer D tuning, and Final Foam integration are later steps.

## River Foam 4.11C.5.14H update

After 5.14G, the next step stayed on shore spawning and converted the hardcoded shore-source recipe into an authoring framework. `Foam Birth Sources` now exposes live `Shore Foam` controls and staged `Object Foam` / `Free Water Foam` placeholders.

For Shore Foam, `Mixed` now uses normalized pattern shares instead of a hardcoded ribbon/wash split. Shore Ribbon and Inward Wash each expose per-pattern Formation Speed, dimensions, Initial Life, and Breakup Strength. Runtime sampling is correlated and aspect-guarded so Length / Width / Reach do not randomize into incoherent combinations.

This is still spawning-only work. Object-contact spawning, free-water spawning, foam evolution, Layer D tuning, and Final Foam integration remain later steps.

- 4.11C.5.15A: Object Foam birth category enabled for static object/contact spawning. Contact Arcs and Contact Flecks are CPU-scheduled from static disturbance source snapshots and GPU-gated by obstacle exclusion/static pressure. Free-water source spawning remains staged after object contact birth validation.

- 4.11C.5.15A.1: Object Foam activation wiring fix. Source-category toggles are now authoritative when automatic birth is enabled and the preset is not Off; Object Foam runtime diagnostics include static source anchor count.

- 4.11C.5.15A.2: Object Foam shape refinement through an Object Contact Edge Field. The field is built from obstacle exclusion and static pressure/contact evidence, then sampled by Contact Arc/Fleck source events so object births follow contact edges rather than rectangular object half-extents. This remains Layer C spawning only; no wake-tail, free-water, Layer D, or Final Foam integration was added.

- 4.11C.5.15A.3 / 5.15A.3.4: Historical object contact edge-distance correction failed due incomplete compute-resource wiring and was recovered to the broad contact field. `5.18C` now supersedes that broad normal-thickness rule with an immediate eight-neighbour shell while preserving `_FoamObjectContactFieldRead` and all existing resources.

- 4.11C.5.15A.4: Object Foam adds `Contact Semi-Arcs` as a third Layer C source pattern. Semi-arcs reuse the existing source-event rasterizer and object-contact field, carry deterministic signed lopsidedness through `Curvature` / `variation.w`, and evaluate a one-sided tangent window instead of the full-arc `abs(tangentDistance)` support. This targets lopsided object shoulder foam without adding free-water spawning, Layer D tuning, Final Foam integration, or new compute resources.

## Foam Source Update — 4.11C.5.15B

Free Water Foam birth is implemented as persistent material skeletons, not final rendered foam. The first two patterns are Lace Connectors and Torn Fragments. This should be validated in Material Remaining Life before judging Final Foam. Thin white glints visible in the inspiration footage are intentionally deferred to water-surface shader/rendering polish.

### Foam Source Update — 4.11C.5.15B.2

Added Cross-Lace Connectors to Free Water Foam. This is a horizontal/cross-current moving head+stroke pattern that complements the original with-flow Lace Connector and Torn Fragment patterns. The patch intentionally does not change Coverage/Activity or introduce shader glints; it only adds the missing source shape class needed for cross-river pale ribbons.

## River Foam movement foundation — `4.11C.5.16A` through `5.16B`

`4.11C.5.16A` established the shared physical velocity contract. `4.11C.5.16A.1` stabilized and validated its route-frequency, across-river-coherence, and slowdown diagnostics.

`4.11C.5.16B` replaces the obsolete global X-column phase commit with conservative local 2D Layer C transport:

```text
canonical resolved velocity at cell centres;
arithmetic-mean velocity at shared faces;
first-order donor-cell packed-state flux;
closed bank/obstacle/invalid faces;
flow-direction-aware one-way endpoint outflow;
CFL target 0.90 and maximum 64 substeps;
lifecycle aging applied once on the final CFL substep for the complete material tick;
births applied after the completed tick;
local render residual was initially added in `5.16B` but was later rejected and removed in `5.16E.2`.
```

No new full-resolution texture is introduced. The canonical velocity include and raw Motion-field compute include remain unchanged. Unity validation of transport is the only active gate.

Immediate order:

```text
5.16D.R removes the rejected macro-breakup experiment and restores the accepted 5.16C.1 baseline;
5.16E adds reversible packed-material visibility policies and non-stalling `0.05` support aging without `_FoamShapeMask` production integration;
5.16E.1 source release and next-face residual gating are rejected and removed;
5.16E.1R proved residual prediction was the stutter source without another allocation;
5.16E.2 promotes committed Final Foam and removes residual prediction;
5.16E.3 attributes capacity loss to unit overflow and fractional shoreline capacity with zero residual;
5.16E.3C retains the `0.10%` engineering target, temporarily tolerates measured sub-1% PoC loss, and defers expensive transport correction;
```
