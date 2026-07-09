# Generated Mass Edge Wear Recovery Architecture

Status: active edge-wear architecture  
Current implementation target: **EW-B — Deterministic Selected-Edge Bevel Kernel**  
Current implementation step: **EW-B1S — Legacy Bevel Construction Purge**

---

## 1. Recovery decision

The active convex edge-wear geometry path is no longer EW-4D/R3 sampled ribbons plus post-ribbon open-cycle closure.

The active path is now a deterministic selected-edge bevel kernel:

```text
source topology graph
→ selected convex source edges
→ explicit face/edge/vertex-owned bevel output
→ final topology audit
→ ConvexEdgeWear mesh markers
```

Reason: the latest EW-4D0.7R3 validation proved the sampled-ribbon route can build most intermediate geometry but fail the entire feature in post-ribbon closure-cap triangulation.

Latest R3 failure facts:

```text
accepted=0
producedBevelFaces=0
committedConvexEdgeWearFaces=0
rejectedValidationCapFace=1
selectedGraphEdges=36
faceClipSucceededFaces=16
extractedRails=72
ribbonFacesBuilt=750
workspaceTJunctionsAfterPreClosureTJunctionRepair=0
openCycleClosureComponentsInput=26
openCycleClosureComponentsBuilt=1
openCycleClosureComponentsFailed=1
openCycleClosureCapEarClipFailures=1
openCycleClosureCapCandidateConvexRejects=15
```

The immediate failure is not edge selection, graph build, rail extraction, or ribbon generation. The blocker is the closure model.

---

## 2. Desired feature, scoped correctly

This architecture covers only the geometry foundation for convex edge wear.

The desired result is:

```text
real generated bevel/chamfer geometry on selected convex edges
watertight topology
no non-manifold edges
no T-junctions
no render slivers/clipping
ConvexEdgeWear material mask preserved on bevel/cap faces
```

It does not attempt to produce the full final reference rock by itself. Later layers still need masks, shader response, cracks, plate seams, stains, moss/dirt, and broader rock-fracture features. Those are separate steps. The bevel kernel remains the correct geometry first step.

---

## 3. Why EW-4D/R3 is superseded as the active construction path

EW-4D/R3 route:

```text
source faces
→ topology graph
→ selected convex graph edges
→ affected-face clipping
→ shared rail extraction
→ sampled profile-grid preparation
→ clipped base-face replacement
→ variable-profile bevel ribbon emission
→ open-edge diagnostics
→ pre-closure T-junction repair
→ branch-aware open-cycle closure
→ cap ear-clipping
→ final topology audit
```

That architecture inverted the bevel problem. It emitted large amounts of bevel/ribbon geometry first and then inferred missing closure from leftover open-edge components.

A bevel kernel should not need to discover its own closure after the fact. It should know every required output from the source topology:

```text
source face → replacement face
selected source edge → bevel face
affected source vertex → cap / endpoint / transition geometry
```

EW-4D/R3 remains useful as historical evidence, but its construction code is no longer retained as a source-level fallback for EW-B.

---

## 4. Active EW-B architecture

### 4.1 Inputs

```text
source PolygonFace list
EdgeWearTopologyGraph built from source Base faces
selected convex EdgeWearSelectedGraphEdge list
bevel width/depth
material feature strength
minimum stable face area / edge length thresholds
```

### 4.2 Outputs

```text
rebuilt PolygonFace list
Base replacement faces
ConvexEdgeWear selected-edge bevel faces
ConvexEdgeWear affected-vertex cap/transition faces
summary diagnostics
```

### 4.3 Source ownership model

Every generated piece must have an owner:

```text
Original source face owns its replacement polygon.
Selected source edge owns its bevel strip/chamfer face.
Affected source vertex owns the cap or transition geometry connecting incident changes.
```

This is the central difference from EW-4D/R3.

### 4.4 Required topology cases

The kernel must explicitly handle:

```text
source edge selected between two valid source faces
source face with zero selected boundary edges
source face with one selected boundary edge
source face with multiple selected boundary edges
source vertex touched by one selected incident edge
source vertex touched by two selected incident edges
source vertex touched by three or more selected incident edges
mixed selected/unselected incident edges at the same vertex
invalid/non-manifold input graph
```

Invalid input or unimplemented case must fail closed before committing geometry.

---

## 5. EW-B1S code state

EW-B1S uses this active entry point in `MassGenerator.cs`:

```text
TryBuildDeterministicSelectedEdgeBevelKernelFaces(...)
```

The entry point now performs only EW-B-owned setup:

```text
source topology graph
selected candidate → graph-edge mapping
DeterministicBevelEdgeRecord list
DeterministicBevelVertexRecord list
vertex-case classification
clean EW-B diagnostics
fail closed before geometry emission
```

Explicitly removed from the active source:

```text
TryBuildLocalEdgeWearBevelFaces
TryApplyLocalEdgeWearBevels
TryBuildTopologyGraphEdgeWearBevelFaces
TryApplyHalfSpaceEdgeWearBevels
TryBuildHalfSpaceEdgeWearSupportPlanes
EW-4D sampled profile/grid/ribbon workspace functions
EW-4D open-cycle closure / cap triangulation functions
EW-4D workspace T-junction repair functions
old corner patch / endpoint cap accumulators
legacy ribbon/open-cycle/workspace diagnostic summary output
```

The patch intentionally does not emit bevel geometry. EW-B1R must implement the first geometry case using EW-B records directly, not by calling retired helpers.

## 6. Next implementation step

**EW-B1R — Clean Isolated-Edge Bevel Case**

Required behavior:

```text
- choose supported selected edges whose two endpoints are isolated selected-edge endpoints
- emit source-owned replacement faces for those edges
- emit one ConvexEdgeWear bevel face per supported edge
- emit simple explicit endpoint caps
- run final topology audit and triangle-preview gate
- leave two-edge and multi-edge vertex stars deferred to EW-B2
```

EW-B1R must not call any retired EW-4B/EW-4C/EW-4D construction function.

## 7. Retained viable pieces

Retained for EW-B use:

```text
BuildEdgeWearBevelCandidates(...)
TryBuildEdgeWearTopologyGraph(...)
TryMapSelectedCandidatesToGraph(...)
AuditEdgeWearTopology(...)
triangle emission preview helpers
ConvexEdgeWear material marker plumbing
general polygon helpers such as ClipPolygon, SanitizePolygon, CreateOrientedFace, CalculatePolygonArea, CalculatePolygonNormal
```

Removed from active source in EW-B1S:

```text
legacy local bevel construction
half-space bevel reconstruction
sampled profile grids
rail-sampled workspace construction
variable-profile ribbon emission
post-ribbon open-cycle closure
closure cap triangulation
workspace T-junction repair
corner patch / endpoint accumulator code
legacy ribbon/open-cycle/workspace diagnostic summary output
```

## 8. Patch sequence

```text
EW-B0  — Reconciliation / architecture reset — complete
EW-B1  — Failed first implementation; proved old helper dependency was still active
EW-B1S — Legacy bevel-construction purge — current
EW-B1R — Clean isolated-edge bevel case
EW-B2  — Explicit affected-vertex cap correctness
EW-B3  — Real generated-rock validation and candidate coverage tuning
EW-B4  — Multi-segment profile and controlled width variation
EW-B5  — Edge-wear mask/material response refinement
```
