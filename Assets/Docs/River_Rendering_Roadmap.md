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

**Generated static-geometry registration and authorship:** Active procedural geometry registers through a neutral event-driven registry. Solid static sources expose their final generated mesh and announce geometry changes; river runtimes perform bounds rejection, cache only geometry that touches the river, and unregister automatically with object or chunk lifetime. Generated static obstacles need no river emitter component. Their optional authorship is feature-specific: participation, Static Pressure mode and values, and Static Wake mode and values. `Obstruction Wake` remains the current serialized/code name for Static Wake until a separately approved compatibility-safe rename. Inherit resolves the detecting river's active defaults, Disabled removes only that feature, and Custom replaces only that feature. Dynamic gameplay sources remain emitter-driven.

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

**Validated:** Complete and accepted. Calm through furious motion, lighting response, reverse flow, irregular shores, shoreline lapping, and frozen-state suppression passed visual testing. Presets favour turbulence and shore activity over excessively fast, uniform macro-wave bands; detached splashes remain assigned to Stage 7.

## 4. Refraction and Optical Distortion

**Problem:** Distort the riverbed and submerged scene convincingly without doubled silhouettes, invalid screen samples, hard boundaries, or camera-dependent artifacts. Liquid and frozen states need distinct distortion behaviour.

**Implemented:** A configurable depth-aware screen-space layer distorts the complete transmitted opaque scene using separate Stage 3 macro-wave and animated-detail optical fields. It supports restrained liquid refraction, depth influence, shoreline protection, foreground-crossing rejection, static ice warping, and quality-scaled ice diffusion. Liquid surface shadows are subdued so the refracted underwater shadow remains dominant. An optional zero-extra-sample silhouette guard reduces object-edge contraction by reusing the original and refracted depth values.

**Validated:** Complete and accepted. Riverbed detail, submerged objects, and underwater shadows deform continuously with surface motion; liquid and frozen optics, depth protection, shoreline behavior, day/night lighting, and local-light response passed visual testing. A faint gray disocclusion trace can remain around strongly refracted high-contrast silhouettes; it is accepted as a low-priority screen-space limitation.

## 5. Runtime Disturbance and Interaction

**Problem:** Add attached pressure, stationary and moving wakes, one-shot ripples, downstream transport, spreading, and decay without replacing the Stage 3 base motion field or scaling water-shader cost with active effect count.

### Canonical feature vocabulary

Stage 5 uses one consistent source-based vocabulary:

- **Static Pressure** — attached water buildup caused by registered stationary geometry.
- **Dynamic Pressure** — the moving-source form of the same attached pressure language, driven by an emitter and relative object/water motion. Planned, not implemented.
- **Static Wake** — the downstream disturbance continuously produced by stationary geometry. The current code and serialized fields call this `Obstruction Wake`.
- **Dynamic Wake** — the persistent disturbance deposited by a moving emitter. The current code and serialized fields call this `Moving Trail`.
- **Impact Ripples** — one-shot event-driven disturbances such as entry, exit, footsteps, landings, projectiles, attacks, and explosions.

Documentation and design discussion should use the canonical names above. Existing runtime, Inspector, serialized-property, shader-property, and compatibility names are not renamed casually; any code-level terminology migration requires a separate approved plan that preserves existing scenes and serialized data.

### Source ownership

- **Generated stationary geometry:** discovered through the generated-geometry registry; eligible for Static Pressure and Static Wake with per-object Inherit, Disabled, or Custom authorship.
- **Dynamic gameplay objects:** owned by `StylizedRiverDisturbanceEmitter`; future Dynamic Pressure and existing/provisional Dynamic Wake consume emitter movement samples.
- **Impact events:** emitted through the river runtime's one-shot event API; emitters may automatically request entry and exit impacts, while gameplay systems may request footsteps, attacks, projectile hits, and scripted impacts directly.
- One source must not be simultaneously owned by both the registry and an emitter for the same continuous static behavior.

### Implemented foundation

A river-owned, chunked runtime already contains separate representations and provisional code paths for Static Pressure, Static Wake, Dynamic Wake, and Impact Ripples. Static geometry preparation is cached and frame-budgeted. The persistent fields support quality-scaled resolution, chunk activity, sleeping, downstream transport, spreading, decay, and fixed-cost shader sampling. Dynamic emitters already support river detection, manual footprints, swept motion submission, and optional entry/exit impact requests.

Static Pressure computes its feasible height from flow, blockage, local mesh support, and Stage 3 wave headroom. Strength selects a normalized point inside that range, Contact Sharpness controls the one-sided upstream falloff, and Profile Variation controls deterministic lateral redistribution. Profile changes use an independent randomized cadence rather than Stage 3 wave frequency. Cached support preparation uses an adaptive vertical inspection range and 16/32/64 lateral rows selected from disturbance-field coverage. The final pressure ridge is contact-anchored with a short open-water rise, hidden overlap beneath the obstruction, and flow-facing attenuation that reduces side-face buildup while preserving the upstream face.

Editor diagnostics report resolved profile resolution, support and multiplier ranges, row classifications, and per-row height/contact graphs. Stage 5 controls do not participate in riverbed or corridor generation.

### Validated

**Static Pressure is complete and accepted.** It passed the tested elevated perspective/isometric views on small and large generated rocks, 16- and 32-row profiles, low and exaggerated Strength, minimum and high Profile Variation, independent changing profiles, contact placement beneath the obstruction, and reduced side-face buildup. The temporary 92%/96% support-safe floor and floor-only diagnostics were removed. Useful target/support/profile diagnostics were retained. Width-aware multiplier bounds remain part of the accepted implementation. No further Static Pressure work should be introduced unless a concrete regression or a proven shared-field integration defect is demonstrated.

The rejected V4 continuously driven obstacle-wave solver is noncanonical and must not be restored.

### Planned visual contracts

**Static Wake — next:** one continuously sourced stationary-object effect made from a short sheltered lee region, controlled side-release/shear near the rear corners, and a broad low-amplitude disturbance that starts at the object, widens downstream, follows river-space flow, and decays. It must not resemble a second pressure ridge, a uniform tube, or repeated circular pulses. It should later provide a useful turbulence/intensity source for Stage 6 foam without rendering foam during Stage 5.

**Impact Ripples — after Static Wake:** one event system with configurable position, radius, signed impulse, geometry contribution, normal contribution, propagation, and decay. Entry, exit, footsteps, landings, projectiles, attacks, and explosions are event profiles feeding the same solver rather than separate water systems. Detached spray, droplets, and splashes remain Stage 7 consumers.

**Dynamic Pressure and Dynamic Wake — deferred package:** design together around emitter-provided movement relative to local river flow. Dynamic Pressure remains attached to the current object position; Dynamic Wake persists after the object passes. A source drifting with the current should create little attached pressure, while upstream or cross-current movement should create stronger leading-face pressure. These features must reuse the accepted visual language where practical without rebuilding expensive static mesh-support profiles every update.

### Remaining Stage 5 work

1. Inspect and document the current Static Wake implementation before changing it: source bake, wake field, simulation, controls, debug views, and existing serialized names.
2. Agree on Static Wake acceptance tests and an exact file-level change plan before implementation.
3. Tune and accept Static Wake without modifying accepted Static Pressure behavior.
4. Inspect the current Impact Ripple implementation and event API, define signed/event-profile behavior, then tune and accept it independently.
5. Defer Dynamic Pressure and Dynamic Wake until the static-source and event-driven features are accepted; design both dynamic features together around one emitter movement-state contract.
6. After all Stage 5 capabilities are accepted, profile Low, Medium, and High quality and run combined overlap, source-removal, sleeping, culling, reverse-flow, and frozen-state regression.

## 6. Foam and Surface Tracing

**Problem:** Generate and transport readable foam from banks, obstacles, turbulence, wakes, and runtime disturbances without unrelated motion layers, tearing fronts, blur, or knot-dependent speed.

**Implemented:** Not started.

## 7. Secondary Water Effects

**Problem:** Supplement the height-field river with effects it cannot represent directly: splashes, droplets, spray, breaking-crest accents, bank and obstacle impacts, shallow-water footsteps, transient sheets, mist, configurable underwater caustics, submerged-bed wet darkening, a softened shoreline wetness band covering plausible wave and disturbance reach, and cascade transition hooks.

**Implemented:** Not started.

## 8. Reflections and Final Integration

**Problem:** Add controlled stylized reflections, liquid-versus-ice surface response, final specular behaviour, quality tiers, and completed integration of motion, disturbance, foam, and secondary effects.

**Implemented:** Not started.

---

## Working Rule

Before implementing a stage or sub-feature, define its acceptance tests. After approval, record a conservative summary under **Implemented** or **Validated**. Later work may consume earlier outputs, but it must not change an approved feature's contract unless that change is discussed and approved first.
