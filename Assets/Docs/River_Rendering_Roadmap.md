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

**Generated static-geometry registration and authorship:** Active procedural geometry registers through a neutral event-driven registry. Solid static sources expose their final generated mesh and announce geometry changes; river runtimes perform bounds rejection, cache only geometry that touches the river, and unregister automatically with object or chunk lifetime. Generated static obstacles need no river emitter component. Their optional authorship is feature-specific: participation, Static Pressure mode and values, and Obstruction Wake mode and values. Inherit resolves the detecting river's active defaults, Disabled removes only that feature, and Custom replaces only that feature. Dynamic gameplay sources remain emitter-driven.

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

**Problem:** Add persistent impact ripples, player and object wakes, stationary obstruction pressure, downstream transport, spreading, and decay without replacing the Stage 3 base motion field or scaling water shading cost with active effect count.

**Implemented:** A river-owned, chunked runtime keeps four interaction features independent: height-aware Static Pressure, static Obstruction Wakes, dynamic Moving Trails, and one-shot Impact Ripples. Generated static geometry registers automatically and may inherit, disable, or customize Static Pressure and Obstruction Wake independently.

Static Pressure computes its feasible height from flow, blockage, local mesh support, and Stage 3 wave headroom. Strength selects a normalized point inside that range, Contact Sharpness controls the one-sided upstream falloff, Profile Variation controls deterministic lateral redistribution, and an independent randomized cadence changes lateral profiles without depending on Stage 3 wave frequency. Cached support preparation uses an adaptive vertical inspection range and 16/32/64 lateral rows selected from disturbance-field coverage. The final pressure ridge is contact-anchored with a short open-water rise, hidden overlap beneath the obstruction, and flow-facing attenuation that reduces side-face buildup while preserving the upstream face.

Static geometry preparation is cached and frame-budgeted. Shared pressure, wake, trail, and ripple representations keep water-shader cost independent of active-source count. Editor diagnostics report resolved profile resolution, support and multiplier ranges, row classifications, and per-row height/contact graphs. Stage 5 controls do not participate in riverbed or corridor generation.

**Validated:** Static Pressure is visually resolved and accepted under the tested elevated perspective/isometric views, including 16- and 32-row sources, low and exaggerated strength, changing lateral profiles, contact placement beneath obstructions, and reduced side-face buildup. Final cleanup must remove superseded support-floor tuning and its floor-specific diagnostics, then rerun Static Pressure regression before the feature is marked fully closed. Obstruction Wakes, Moving Trails, and Impact Ripples remain to be evaluated and accepted separately, so Stage 5 as a whole remains in progress. The rejected V4 continuously driven obstacle-wave solver is noncanonical and must not be restored.

**Remaining Stage 5 work:**
- Remove the superseded 92%/96% support-safe floor and its floor-only Inspector/graph diagnostics, then rerun Static Pressure regression.
- A/B test the width-aware animated multiplier bounds at minimum and maximum Profile Variation; retain them only if they prevent a visible high-intensity regression.
- Validate and tune Obstruction Wakes independently of Static Pressure.
- Validate and tune Moving Trails independently of Static Pressure and Obstruction Wakes.
- Validate and tune Impact Ripples independently.
- Profile Low, Medium, and High quality after all four interaction features are individually accepted, then perform combined overlap and sleeping/culling regression.

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

Before implementing a stage, define its acceptance tests. After approval, record a conservative summary under **Implemented**. Later stages may consume earlier outputs, but they must not change an approved stage's contract unless that change is discussed and approved first.
