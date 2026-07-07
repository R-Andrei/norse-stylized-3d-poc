# River Foam Stage 6 Canonical Architecture

## Purpose

This is the canonical architecture contract for Stage 6 river Foam.

This document is the active source of truth for how the Foam system is allowed to work. It supersedes older persistent-morph, lateral-row-commit, pocket/entity, shader-macro-stretch, local-edge-fray, and one-off coherent-warp plans wherever they conflict with this contract.

The goal is to reproduce the broad behavior of the visual inspiration river: stylized pale surface-film sheets, connected ribbons, bank and obstacle skirts, temporary bridges, pinches, fractures, edge chipping, small fragments, and thin bright surface streaks, while preserving a performance-safe field-based architecture.

The target is not a physically exact fluid solver and not a foam entity database. The target is a fixed-grid mathematical field system with strict ownership boundaries and no circular dependencies.

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
motionIntent
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

## 4.7 Current status

Active/trusted:

```text
manual/source birth
source-to-persistent merge
downstream phase transport
lifecycle aging
support/negative aging influence
valid-fluid and obstacle clipping
```

Rejected/superseded:

```text
persistent stored-state morph as visual breakup
fractional lateral row weighting
per-cell stochastic lateral row commit
hidden neighbour-resampling morphology that writes persistent state
```

Actual lateral material transport is currently not active and must not be implied by debug labels.

## 4.8 Connectivity invariant

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
No low-res Layer D Film Source or Film Support helpers exist yet.
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

This is the desired baseline until local procedural breakup or low-res film support is added.

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

Build a low-res source field from upstream data.

Inputs:

```text
Persistent Presence
Remaining Life
Material Pattern
valid fluid
exclusion
bank/contact support
rock/contact support
wake/pressure/ripple support
motion intent
negative suppression
```

Output:

```text
_FoamFilmSourceHalf
```

Meaning:

```text
Where is broad visible film allowed or encouraged?
```

This is not persistent material.

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

A local function can produce convincing chipping and semi-organized chaos:

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

## 5.11 Performance target

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

Needed next debug views:

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

Status after 4.11C.5.10: partially complete.

Completed:

```text
Foam Shape Difference debug.
A/B/C/D/E/F source-level compliance audit.
Stale editor/help text cleanup.
Layer D dispatch gating while Final Foam does not consume _FoamShapeMask.
```

Still needed when low-res Layer D helpers are added:

```text
Foam Film Source debug.
Foam Film Support debug.
Verification that all new Layer D helpers write only Layer D products and do not feed Layer B or C.
```

## Phase 3 — Local procedural breakup probe

Test the cheapest “magical” layer honestly.

Scope:

```text
local procedural chipping/fray/cuts based on position, river UV, time, life, materialPattern
no neighbourhood search
no persistent mutation
no broad bridge support
```

Purpose:

```text
determine how much reference-like chaos can be achieved with local math alone
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

The next implementation work should start from compliance and debug visibility, not another visual patch.

