# River Rendering Roadmap

## Purpose

Define the river as a sequence of independent problems. Each stage is completed, tested, and approved before work begins on the next one.

## Cornerstones

### Configurability

The same system must support anything from a calm, shallow, puddle-like stream to a furious, fast-moving river, as well as fully frozen river surfaces. High-impact controls must be clear in the Inspector, with sensible defaults and advanced settings grouped away from normal styling controls.

### Integrability

Every stage must be designed with later systems in mind. Water body, motion, refraction, interaction, foam, and reflections must connect through stable interfaces so later work can be added without refactoring completed stages.

### Stage-Gated Development

Later effects must not be used to hide problems in earlier stages. Each stage receives explicit acceptance tests and is only considered complete after the result is approved.

### Human-Readable Tooling

Each system needs clear controls, independent enable/disable options, useful debug views, and understandable runtime status.

### Isometric-Camera Suitability

Every solution must be designed and judged for the game's elevated, perspective camera with an isometric-style angle. Techniques that depend on close third-person viewing, shallow camera angles, or details that disappear or break at gameplay distance are not acceptable. Orthographic compatibility should be preserved where practical, but the perspective isometric-style camera is the primary production and acceptance target.

### HLSL-First Rendering

Prefer handwritten HLSL over Shader Graphs whenever practical. Graphs should only be used when they provide a clear technical or production advantage that justifies the additional abstraction and maintenance cost.

---

## Cross-Stage Foundation Work

**High-performance compatibility:** Visual features may carry meaningful cost when their importance justifies it, but the framework prefers shared fixed-cost representations, quality tiers, culling, sleeping, staggered updates, and lower-frequency simulation whenever they can provide comparable results. Per-effect/per-pixel scaling is avoided where a shared persistent field is practical.

**Generated static-geometry registration and authorship:** Active procedural geometry registers through a neutral event-driven registry. Solid stationary sources expose their final generated mesh and announce geometry changes; river runtimes perform bounds rejection, cache only geometry that touches the river, and unregister automatically with object or chunk lifetime. Generated stationary obstacles need no river emitter component. Their optional authorship is feature-specific: participation, Pressure mode and values, and Wake mode and values. Inherit resolves the detecting river's active defaults, Disabled removes only that source contribution, and Custom replaces only that feature's source values. Dynamic gameplay sources remain emitter-driven. The obsolete Static branch of `StylizedRiverDisturbanceEmitter` has been removed; the component is now dynamic-only. Legacy serialized Static instances remain inert solely as a migration guard and warn that the obsolete component should be removed.

**Generated channel and terrain integration:** A dedicated spline-following corridor generates the riverbed, slopes, shoreline, hidden overlap, collider handoff, and buried terrain apron. It samples an immutable pre-river ground snapshot and matches ground height, slope, normals, UVs, and surface metadata at the handoff.

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

**Shared shoreline output — implemented; Stage 6 validation pending:** The macro-wave, river-space noise, shore-wave profile, and shore attenuation primitives now live in the shared water-motion contract rather than only in the render pass. Stage 6 uses those exact functions to resolve one instantaneous left/right visible shoreline edge per longitudinal topology row by intersecting current positive shore-wave displacement with the corridor's mandatory hidden bank-cover profile. Consumers must not recreate an approximate shoreline rhythm independently.

**Intermediate shore-wave profile controls — implemented; Unity validation pending:** The accepted repeating Stage 3 carrier remains in place, but the bank-reaching component can now diverge from the centre-river macro wave through seven controls:

- `Shore Wave Height Scale` independently scales vertical bank-wave amplitude;
- `Shore Wave Length Scale` independently scales longitudinal bank-wave length;
- `Shore Wave Reach` limits the fraction of generated hidden shoreline allowance that can be wetted;
- `Shore Wave Transition Length` defines the world-space smoothing span for the within-wave profile and for blends between neighbouring waves with different overall sizes;
- `Shore Wave Size Variation` gives successive travelling waves stable deterministic differences in overall height and lateral reach;
- `Shore Side Asymmetry` blends from shared left/right size and profile values to independent bank values;
- `Shore Wave Profile Variation` creates deterministic variation inside each wave between its start, middle, and end.

Within-wave profile knots use a slope-continuous cubic curve that blends toward a smoother B-spline response as Transition Length increases. Successive wave-size values also blend across that configured metric span. A final zero-slope activation envelope is now applied to the signed shore-wave height near zero crossings and to lateral reach near both the normal shoreline and the maximum hidden-water allowance. This prevents the visible shore from leaving or rejoining either hard bound with a tangent discontinuity instead of merely smoothing the earlier profile values. Size identities are deterministic and travel with the existing carrier; they do not reseed or fluctuate independently at runtime. Left and right profiles are identical when Side Asymmetry is zero and become increasingly independent as it rises. Neutral size/profile variation values preserve the previous wave identities, while Transition Length still controls the new final shoreline onset/exit smoothing. Water displacement, surface normals, liquid refraction motion, instantaneous shoreline resolution, and Stage 6 Shore Support all consume the same shared evaluator. This is an intermediate extension of the existing carrier, not the later explicit travelling-wave-packet redesign; individual packet speeds, lifetimes, births, and independent length evolution remain deferred.

**Validated:** The original calm-through-furious motion contract remains accepted at neutral shore-profile values. The new shore-specific controls require focused Unity validation across reverse flow, freeze/thaw, asymmetric banks, and hidden-allowance limits. Presets reset the new controls to neutral values so selecting an existing motion preset preserves the accepted appearance. Detached splashes remain assigned to Stage 7.

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

**Impact Ripples — approved implementation:** one shared event system with configurable position, radius, signed impulse, initial elevation, analytic shape, sharpness, geometry contribution, and normal contribution. Propagation, decay, flow dissipation, and boundary response remain river-level rules for the shared field. Entry, exit, footsteps, landings, projectiles, attacks, and explosions feed the same solver rather than becoming separate water systems. Detached spray, droplets, and splashes remain Stage 7 consumers.

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

**Impact Ripple status:** complete and accepted after compilation, focused checks, and extensive user stress testing. R4 and R5 were collapsed into this focused finalization because the shared analytic profile, signed overlap, metric propagation, lifetime reservations, frozen-state handling, boundary interaction, quality behavior, and combined Stage 5 coexistence were tested sufficiently. Detached spray, droplets, and splash particles remain Stage 7 work and were not used to conceal ripple defects.

### Stage 5 closure

**Stage 5 is closed for the current gameplay milestone.**

- Stationary Pressure is complete and accepted.
- Stationary source preparation and the shared persistent Wake response are complete and accepted.
- Impact Ripples are complete and accepted.
- Full relative-motion preparation for dynamic Pressure and dynamic Wake is explicitly deferred until an authoritative movement/velocity system and real moving-water gameplay consumers exist. The current dynamic emitter path remains compatibility and foundation work, not a completed production movement model.
- A lightweight Inspector workload pass was completed. In a representative scene the accepted disturbance fields reported approximately `0.39 MB` allocated memory at the captured state, and deliberate user stress testing remained bounded at roughly `48,000` estimated cell-iterations. These are internal workload estimates rather than measured CPU/GPU milliseconds; a broader hardware profiling pass remains a future whole-project optimization milestone.
- Stage 6 may consume the accepted Pressure, Wake, Ripple, boundary, river-domain, freeze, chunking, and diagnostics contracts without reopening their visual behaviour.


## 6. Foam and Surface Tracing

**Problem:** Build the persistent stylized surface-film/Foam layer: broad broken sheets, contour ribbons around dark water pockets, temporary connectors, shore/obstacle skirts, peeling strips, and detached fragments. The system must work from the elevated perspective camera and must remain performance-safe across active river chunks.

**Canonical document:** `River_Foam_Stage6_Architecture.md` owns the Foam architecture. `River_Foam_Active_Blockers_and_Next_Patches.md` owns the current blocker order and immediate patch plan. This roadmap only keeps the macro sequence.

### Current status

Stage 6 is in the `4.11C` manually-born persistent material phase. The following foundations are in place or materially improved:

- persistent material state with Presence, Remaining Life, and Material Pattern semantics;
- source-only Amount and source-to-persistent merge rules;
- topology lifespan support and negative aging pressure;
- lifetime delta-time rebind, so material now ages according to runtime settings;
- support/negative aging response repair, so negative pressure suppresses positive support before accelerating death;
- consolidated manual Foam birth controls under `Foam Debug > Manual Birth Source`;
- 5.4l composition-event refactor, so one manual spawn starts one budgeted composition event instead of hidden multiple writer events;
- 5.4m manual source realignment, so birth no longer uses pattern/complexity/density controls to randomize macro Foam identity;
- 5.7c accepted stored-state surface morphing as functional, with a practical authored `Surface Morph Strength` range around 2-4 and approximately 2.5 as the current working value;
- 5.8 adds chaotic intermittent persistent material drift inside the existing simulation pass, with coherent lateral impulses, calm/no-drift intervals, local shear, and subtle resistance while preserving net-downstream phase transport; 5.8b recalibrates the response so normal values are useful, 5.8c separates macro patch transport, meso shear, and edge detail, and 5.8d increases macro authority while reducing micro/edge dominance.

Current failing area: movement/source/final-readability behavior. Manual source material is intentionally stable and persistent Foam now has intrinsic wobble, surface-driven morphing, and chaotic intermittent drift, but obstacle-aware tangential movement, better source shapes, topology proof, and final visual fragmentation remain unresolved.

### Canonical topology/material relationship

Topology influences lifespan. It is not direct Foam painting, steering, automatic filling, or population control.

Persistent material owns what Foam exists now. It moves, ages, changes shape, fragments, merges visually, and dies.

Birth/source events create candidate material. After merge, source Amount is discarded and persistent state owns survival.

### Current `4.11C` repair sequence

The current approved order is:

1. `4.11C.5.5 — Temporal Morphing and Material Shape Evolution`
2. `4.11C.5.6 — Surface-Coupled Foam Rendering`
3. `4.11C.5.6b — Foam Surface Clarity Filter`
4. `4.11C.5.7 — Surface-Driven Material Morphing`
5. `4.11C.5.7b — Surface Morph Calibration`
6. `4.11C.5.7c — Surface Morph Formula Rebalance`
7. `4.11C.5.8/5.8b/5.8c/5.8d — Chaotic Intermittent Foam Drift + Macro Authority Calibration`
8. `4.11C.5.9 — Obstacle-Based Tangential Foam Movement`
9. `4.11C.5.10 — Better Manual Source Shape Spawning`
10. `4.11C.5.11 — Topology Aging Proof and Calibration`
11. `4.11C.5.12 — Final Foam Visual Fragmentation and Reference-Matching`

Do not continue to automatic birth population until manually-born material passes those gates.

### Deferred after manual material acceptance

- **4.11D:** Anchored Birth Events.
- **4.11E:** Open-Water Births and Spatial Fairness.
- **4.11F:** Integrated Birth Population.
- **4.12:** Age/pattern-driven breakup and optional persistent fracture if justified.
- **4.13:** Mature Foam rendering/reference matching.
- **4.14:** Performance and regression closure.

### Public workflow

Primary debug views remain:

- Final Foam;
- Foam + Aging Topology;
- Progressive Birth Source;
- Progressive Birth Transfer.

Normal authoring controls should stay compact. Avoid restoring old debug button walls or adding broad new Inspector groups while the active blockers remain unresolved.

### Performance constraints

- active work is per active river/chunk;
- inactive/frozen/culled chunks should avoid unnecessary simulation and birth work;
- Foam events use fixed-capacity pools;
- no per-event GameObjects or steady-state managed allocations;
- birth dispatch count is quality-budgeted;
- visual complexity must not secretly multiply C# events or dispatches.

### Failure rule

When Foam looks wrong, diagnose state boundaries before adding new features: source footprint, source union, source-to-persistent merge, packed state, transport/clipping, topology age rate, runtime activity, then renderer/diagnostics.

Do not compensate for broken manual material behavior with automatic births, topology painting, opacity tuning, or unrelated water-architecture changes.

## 7. Secondary Water Effects

**Problem:** Supplement the height-field river with effects it cannot represent directly: splashes, droplets, spray, breaking-crest accents, bank and obstacle impacts, shallow-water footsteps, transient sheets, mist, configurable underwater caustics, submerged-bed wet darkening, a softened shoreline wetness band covering plausible wave and disturbance reach, and cascade transition hooks.

**Implemented:** Not started.

## 8. Reflections and Final Integration

**Problem:** Add controlled stylized reflections, liquid-versus-ice surface response, final specular behaviour, quality tiers, and completed integration of motion, disturbance, foam, and secondary effects.

**Implemented:** Not started.

---

## Working Rule

Before implementing a stage or sub-feature, define its acceptance tests. After approval, record a conservative summary under **Implemented** or **Validated**. Later work may consume earlier outputs, but it must not change an approved feature's contract unless that change is discussed and approved first.
