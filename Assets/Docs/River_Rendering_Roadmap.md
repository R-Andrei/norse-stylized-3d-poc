# River Rendering Roadmap

## Purpose

Define the river as a sequence of independent problems. Each stage is completed, tested, and approved before work begins on the next one.

## Cornerstones

### Configurability

The same system must support anything from a calm, shallow, puddle-like stream to a furious, fast-moving river. High-impact controls must be clear in the Inspector, with sensible defaults and advanced settings grouped away from normal styling controls.

### Integrability

Every stage must be designed with later systems in mind. Water body, motion, refraction, interaction, foam, and reflections must connect through stable interfaces so later work can be added without refactoring completed stages.

### Stage-Gated Development

Later effects must not be used to hide problems in earlier stages. Each stage receives explicit acceptance tests and is only considered complete after the result is approved.

### Human-Readable Tooling

Each system needs clear controls, independent enable/disable options, useful debug views, and understandable runtime status.

---

## 1. River Domain and Coordinate Contract

**Problem:** Establish one continuous river-space representation for distance along the river, position across it, local direction, width, height, and world-space conversion. Motion must not change speed, reverse, jump, or reveal spline knots.

**Implemented:** Not started.

## 2. Water Body

**Problem:** Make still water read as a coherent body through colour, depth, opacity, clarity, and bank integration. It must support both calm shallow water and forceful deep water, while exposing stable inputs for all later systems.

**Implemented:** Not started.

## 3. Surface Motion and Coherent Flow

**Problem:** Add normals, waves, current accents, and surface displacement that all agree on direction and speed. Motion must remain continuous through bends and knots and must scale cleanly from calm to furious.

**Implemented:** Not started.

## 4. Refraction and Scene Integration

**Problem:** Distort the riverbed and submerged scene convincingly without doubled silhouettes, invalid screen samples, hard boundaries, or camera-dependent artifacts.

**Implemented:** Not started.

## 5. Interaction and Source Detection

**Problem:** Detect banks, static obstacles, runtime objects, and their movement relative to the river. Produce consistent interaction data that later systems can use without knowing how the river mesh or spline is built.

**Implemented:** Not started.

## 6. Foam Generation, Transport, and Rendering

**Problem:** Produce sharp, readable foam from banks, obstacles, turbulent regions, and runtime interactions. Foam must move consistently downstream, remain configurable from sparse to violent, and avoid visible repetition, tearing fronts, knot-dependent speed, blur, and unrelated visual layers.

**Implemented:** Not started.

## 7. Reflections and Final Lighting

**Problem:** Add controlled stylized reflections and final lighting integration without invalid reflection patches, excessive cost, or conflict with opacity and refraction. A reliable fallback must exist when high-quality reflection data is unavailable.

**Implemented:** Not started.

---

## Working Rule

Before implementing a stage, define its acceptance tests. After approval, record a conservative summary under **Implemented**. Later stages may consume earlier outputs, but they must not change an approved stage's contract unless that change is discussed and approved first.
