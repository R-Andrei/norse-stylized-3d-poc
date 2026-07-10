# River Foam Stage 6 Canonical Architecture

## Purpose

This is the canonical architecture contract for Stage 6 river Foam.

This document is the active source of truth for how the Foam system is allowed to work. It supersedes older persistent-morph, lateral-row-commit, pocket/entity, shader-macro-stretch, local-edge-fray, and one-off coherent-warp plans wherever they conflict with this contract.

The goal is to reproduce the broad behavior of the visual inspiration river: stylized pale surface-film sheets, connected ribbons, bank and obstacle skirts, temporary bridges, pinches, fractures, edge chipping, small fragments, and thin bright surface streaks, while preserving a performance-safe field-based architecture.

The target is not a physically exact fluid solver and not a foam entity database. The target is a fixed-grid mathematical field system with strict ownership boundaries and no circular dependencies.

## Current implementation status — `4.11C.5.16A.1`

The Layer C source-population prerequisite is provisionally complete enough for evolution work:

```text
Shore birth: implemented and accepted provisionally.
Static object/contact birth: implemented and accepted provisionally.
Free-water lace, cross-lace, and fragment birth: implemented and accepted provisionally.
Cross-lace longitudinal blockiness: known parked structural-resolution limitation.
```

The active architecture block is now movement/evolution, beginning at the upstream motion authority rather than at final rendering.

`4.11C.5.16A — Unified Foam Velocity Contract` establishes one canonical physical velocity resolver shared by compute and the existing Motion Field debug view:

```text
raw scrolling lane intent       ─┐
fixed obstacle-routing intent    ├─> resolved Foam velocity
base downstream Foam speed       ┤
obstacle slowdown controls       ┘
```

The resolved contract contains:

```text
nonnegative downstream speed magnitude in metres/second;
signed lateral speed in metres/second;
lateral intent;
downstream speed factor;
obstacle influence;
raw lane and obstacle intent for diagnostics/future strain work.
```

The raw lane and obstacle textures remain separate because they have different coordinate rules: lane intent scrolls through sample space, while obstacle routing must stay fixed to world/river obstacles. They are married logically through one pure resolver, not packed into one misleading texture.

`4.11C.5.16A` validation confirmed physical lane advection, signed lateral velocity, flow reversal, no premature material movement, and the shared velocity authority. It also exposed three pre-transport corrections:

```text
runtime Inspector HelpBoxes changed layout height from frame to frame;
obstacle-yellow debug composition could brighten a slowed region after speed darkening;
one generic lane scale changed downstream sign frequency and across-river variation together.
```

`4.11C.5.16A.1 — Velocity Diagnostics Stability + Route Frequency` corrects those issues without adding resources or changing material state:

```text
runtime transport diagnostics use fixed-height rows only;
Motion Field hue carries lateral/obstacle meaning and brightness carries downstream speed only;
Direction Change Frequency controls downstream sign-change frequency;
Across-River Coherence independently controls how broadly neighbouring rows share intent;
the Motion Lane signature is versioned and includes both route-shape controls;
Obstacle Routing remains independent and does not rebuild for lane-only changes.
```

This patch still does **not** move stored material laterally. The current global downstream phase transport remains temporarily active. After `5.16A.1` is validated, the next movement patch must replace that final authority with conservative unified 2D Layer C advection so local slowdown and stagnation can become real material behavior.

Exact next major patch after validation:

```text
4.11C.5.16B — Conservative Unified 2D Material Advection
```

---

# 0. Non-negotiable design goals

## 0.1 Visual target decomposition

The reference river is not one effect. It is a stack of visual phenomena:

1. **Broad pale surface film**  
   Large white/pale sheets sit on the water surface. They read as continuous film rather than as discrete particles.

2. **Connected ribbons and current seams**  
   Foam forms long broken bands along flow lanes, banks, rocks, and darker water pockets.

3. **Split / merge / pinch / reunite appearance**  
   Visible film narrows, breaks, rejoins, and creates temporary necks. This must be visual field behavior, not per-pocket identity tracking.

4. **Chipping and edge chaos**  
   Edges chip, fray, crack, and flutter. This can be largely procedural and local.

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

```hlsl
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

The existing Motion Field debug modes use the same pure contract. Hue encodes route meaning: neutral gray is straight motion, red/blue encode signed lateral velocity, and yellow indicates obstacle influence. Brightness is applied only after hue composition and represents `downstreamSpeedFactor`: bright is full-speed, dark is slowed, and near-black is near-stagnation. White overlays raw stored material Presence.

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

```hlsl
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
global downstream phase transport as a temporary legacy movement authority
lifecycle aging
support/negative aging influence
valid-fluid and obstacle clipping
unified physical Foam velocity contract for validation and future consumers
```

Rejected/superseded:

```text
persistent stored-state morph as visual breakup
fractional lateral row weighting
per-cell stochastic lateral row commit
hidden neighbour-resampling morphology that writes persistent state
```

Actual lateral material transport is currently not active and must not be implied by debug labels.

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

```hlsl
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
read-only previous visual shape only if visual history is explicitly added
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

Future possible products, if justified:

```text
_FoamFilmSourceHalf
_FoamFilmSupportHalf
_FoamBreakMask
_FoamEdgeMask
_FoamShapeHistory
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
use visual-only history to reduce flicker if needed
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

### D4 — Optional Visual History

Only add if flicker becomes a real problem.

Inputs:

```text
previous _FoamShapeMask
current desired _FoamShapeMask
```

Output:

```text
smoothed _FoamShapeMask or _FoamShapeHistory
```

Rules:

```text
visual history may smooth appearance;
visual history must not feed Layer C;
visual history must not become material history.
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
small local fray
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
Progressive Birth Source
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
tiny local edge-fray as the main look
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
visual-only temporal history
real lateral material transport
real material merge rules
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
Foam Shader Detail Probe debug view
Foam Shader Detail Difference debug view
shader-side local procedural chipping/fray/cuts based on river metres, material UV, material pattern, time, Remaining Life, and surface energy
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

## Phase 5 — Full-res Evaluated Shape

Combine upstream material, support, and local breakup into `_FoamShapeMask`.

Acceptance:

```text
Foam Evaluated Shape visibly differs from Material Presence in broad structural ways.
Interiors remain mostly solid.
Edges chip/fray.
Small gaps can bridge visually.
Weak/old regions can pinch.
No durable material is mutated.
```

## Phase 6 — Switch Final Foam to Layer D

Only after Layer D earns it.

Scope:

```text
Final shader samples _FoamShapeMask as broad structure.
Legacy shader macro-shape logic is demoted/removed.
Shader retains local detail and thin streaks.
```

## Phase 7 — Thin bright streak layer

Add separate shader-side streaks.

Scope:

```text
fast narrow white scratches/streaks
local/no-neighbour procedural math
separate from broad film mask
```

## Phase 8 — Optional visual history

Only if flicker is a real problem.

Scope:

```text
visual-only smoothing/hysteresis for _FoamShapeMask
no feedback into Layer C
```

## Phase 9 — Performance tiers and chunk scheduling

Formalize:

```text
resolution per quality tier
Layer C update rate
Layer D helper/shape update rate
active chunk caps
culling/freezing rules
debug/profiling counters
```

---

# 13. Current active conclusion

The current architecture is compatible with the final solution if corrected as follows:

```text
Keep Layer C Persistent Foam Material.
Keep Layer D _FoamShapeMask as the visual product.
Keep Layer E shader composition.
Rename the old ambiguous Stage 1.5 concept to Layer B — External Influence Fields.
Do not let Layer B read foam.
Move foam-derived sheet support into Layer D.
Treat 5.9z coherent coordinate warp as a failed/superseded prototype, not as the main path.
```

Compliance/debug visibility is complete through `4.11C.5.10`, and failed Layer D visual probes were retired in `4.11C.5.10B` and `4.11C.5.11B`. `4.11C.5.12` proved Layer E can create sub-cell shader-side detail, but it remains debug-only. `4.11C.5.13`, `5.13B`, and `5.13C` establish the first low-resolution Layer D Film Source / Film Support pipeline with corrected domain-space sampling and material-gated source semantics. `4.11C.5.13D` now tunes that clean spread shape: support bias is narrower, cross-flow spread is weaker/evidence-gated, bridge behavior is stricter, and final support contribution is more conservative. The next work is Unity validation of 5.13D before any Final Foam switch.



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
2 Progressive Birth Source — clean from topology-support film contamination.
3 Material Presence — clean Layer C material truth.
4 Material Remaining Life — clean Layer C material-life truth.
5 Foam Motion Field — external motion/routing debug, not topology support.
6 Foam Motion Field + Cell Grid — external motion plus intentional material-space cell grid.
7 Foam Evaluated Shape — contaminated before 5.13C through _FoamShapeMask.
8 Foam Shape Difference — truthful comparison, but its evaluated-shape input was contaminated before 5.13C.
9 Foam Shader Detail Probe — inherited contaminated _FoamShapeMask before 5.13C.
10 Foam Shader Detail Difference — inherited contaminated _FoamShapeMask before 5.13C.
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

This keeps the selected performance model intact: no GPU readback, no connected-component extraction, no new textures or buffers, and no new object-contact resource binding. The object-contact field remains the 5.15A.2/5.15A.3.4 stable broad contact authority. Any future sharper edge-distance field correction must be a separate resource-audited patch.

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

