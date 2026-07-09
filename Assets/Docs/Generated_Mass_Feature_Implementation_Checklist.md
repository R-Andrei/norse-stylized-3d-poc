# Generated Mass Feature Implementation Checklist

Status: active implementation checklist  
Current implementation target: **EW-B — Deterministic Selected-Edge Bevel Kernel**  
Current implementation step: **EW-B1S — Legacy Bevel Construction Purge**

---

## 1. Hard rules

```text
- Do not implement final convex edge wear with FeatureAtlas0/1.
- Do not use overlay meshes as the final production solution.
- Do not weaken topology validation to make geometry visible.
- Do not drop individual selected candidates to hide construction errors.
- Do not tune material response before physical bevel geometry is valid.
- Dirty-time generation may be heavier; runtime memory and runtime per-frame cost should stay reasonable.
- Keep docs updated in the same patch as each implementation step.
```

---

## 2. Active representation

Final plane-cut GeneratedMass convex edge wear should be:

```text
main-mesh deterministic selected-edge bevel geometry
+ explicit source-face replacement faces
+ explicit selected-edge bevel/chamfer faces
+ explicit affected-vertex caps/transitions
+ UV2.z / vertex color ConvexEdgeWear material markers
+ shader response on marked generated geometry after geometry is stable
```

FeatureAtlas0/1 are temporary debug/broad-mask tools only. They are not the normal-render convex edge-wear representation.

---

## 3. Superseded active checklist branch

EW-4D/R3 is no longer the active checklist branch.

Superseded active representation:

```text
generated main-mesh variable-profile bevel ribbons
+ actual open-cycle closure caps triangulated before final topology audit
+ UV2.z / vertex color ConvexEdgeWear material markers
```

Reason for superseding:

```text
- R3 builds candidate/graph/ribbon intermediate data but fails inside closure-cap triangulation.
- Latest validation had accepted=0, producedBevelFaces=0, committedConvexEdgeWearFaces=0.
- The failure happens after ribbonFacesBuilt=750 and workspaceTJunctionsAfterPreClosureTJunctionRepair=0.
- The post-ribbon closure model is therefore too fragile to remain the active foundation.
```

Do not retain EW-4D construction code as inactive source-level fallback. Historical notes may remain in docs, but the active source must not carry retired bevel-construction systems forward.

---

## 4. EW-B staged checklist

### EW-B0 — Edge Wear Bevel Kernel Reconciliation — complete

Implementation requirements:

```text
- Route active edge-wear construction through TryBuildDeterministicSelectedEdgeBevelKernelFaces(...).
- Build/reuse the source topology graph.
- Map selected candidates to graph edges.
- Report graph/candidate stats.
- Set deterministicKernelPending=1.
- Fail closed before geometry emission.
- Kept old EW-4D/R3 construction path inactive at B0; EW-B1S removes the retired construction path.
- Rewrite docs so EW-B is the only active plan.
```

Validation success:

```text
Unity compiles.
Generated_Mass_Framework.md names EW-B0 / EW-B as current.
Generated_Mass_Edge_Wear_Recovery_Architecture.md names EW-B0 / EW-B as current.
Generated_Mass_Feature_Implementation_Checklist.md names EW-B0 / EW-B as current.
ApplyGeneratedEdgeWearBevels calls TryBuildDeterministicSelectedEdgeBevelKernelFaces, not TryBuildTopologyGraphEdgeWearBevelFaces.
Regeneration with edge wear enabled leaves geometry unchanged and reports deterministicKernelPending=1.
```

### EW-B1S — Legacy bevel-construction purge — current

Implementation requirements:

```text
- Remove retired local-bevel, half-space, sampled-ribbon/workspace, open-cycle-closure, corner-patch, and workspace-T-junction-repair construction code from MassGenerator.cs.
- Remove legacy ribbon/open-cycle/workspace counters from the active warning summary.
- Keep candidate selection, topology graph construction, selected graph-edge mapping, final topology audit, triangle preview, and ConvexEdgeWear material-marker plumbing.
- Add/keep EW-B-specific selected-edge and affected-vertex records.
- Fail closed with deterministicKernelGeometryPending=1 before geometry emission.
```

Validation success:

```text
Unity compiles.
Search MassGenerator.cs: TryBuildLocalEdgeWearBevelFaces, TryBuildTopologyGraphEdgeWearBevelFaces, TryAppendEdgeWearOpenCycleClosureFaces, EdgeWearTopologyRebuildWorkspace, EdgeWearProfileGrid, EndpointCapAccumulator are absent.
Regeneration with edge wear enabled emits a short EW-B log with deterministicKernelGeometryPending=1.
No ribbon/open-cycle/workspace counters appear in the active summary.
No bevel geometry is expected yet.
```

### EW-B1R — Clean isolated-edge bevel case

Implementation requirements:

```text
- Use DeterministicBevelEdgeRecord and DeterministicBevelVertexRecord only.
- Support selected edges whose endpoints are both isolated selected-edge endpoints.
- Emit deterministic Base replacement faces, one ConvexEdgeWear bevel face, and explicit endpoint caps.
- Defer two-edge and multi-edge vertex stars to EW-B2.
- Commit only after topology and triangle-preview validation.
```

Strict exclusions:

```text
- no retired helper calls
- no sampled variable-profile grids
- no ribbon-density controls
- no open-cycle closure
- no edge-width variation
- no shader/material tuning
```

### EW-B2 — Affected-vertex cap correctness

Implementation requirements:

```text
- Support one selected incident edge at a vertex.
- Support two selected incident edges at a vertex.
- Support three or more selected incident edges at a vertex.
- Support mixed selected/unselected incident edge stars.
- Sort cap/transition geometry from source adjacency, not arbitrary open-hole tracing.
```

Validation success:

```text
no missing caps at selected-edge endpoints
no open edges around affected vertices
no non-manifold vertex caps
no T-junctions introduced by cap transitions
```

### EW-B3 — Real generated-rock validation

Implementation requirements:

```text
- Validate the constant-width kernel on normal generated mass seeds.
- Keep coverage/amount/width controls wired.
- Measure face/triangle/rendered-vertex cost.
- Identify unsupported topology cases before adding variation.
```

Validation success:

```text
multiple generated rock seeds commit bevel geometry
no seed fails because of post-ribbon closure/cap tracing
diagnostics identify any unsupported source-vertex cases explicitly
```

### EW-B4 — Multi-segment profile and irregularity

Implementation requirements:

```text
- Add controlled bevel profiles after the single-segment kernel is stable.
- Add width/strength variation only after topology stays stable.
- Preserve budget limits by quality tier.
```

Validation success:

```text
profile changes alter bevel shape without changing topology validity
variation does not introduce slivers or unclosed caps
rendered vertex cost remains within accepted tier budgets or is explicitly reported
```

### EW-B5 — Mask/material response refinement

Implementation requirements:

```text
- Use UV2.z / vertex alpha ConvexEdgeWear data from generated bevel/cap faces.
- Tune brightness/tint/smoothness/falloff response.
- Add edge-wear mask widening/softening only after geometry is stable.
```

Validation success:

```text
Convex Edge Wear debug view matches generated bevel/cap geometry
normal render shows inspiration-like exposed edge wear on physical bevels
material response remains independent from topology validity
```

---

## 5. Do not resume until EW-B1/B2 pass

Do not resume these items until the deterministic bevel kernel is structurally valid:

```text
EW-4D0.8 density/budget tuning
closure-cap ear-clipping fixes
sampled-ribbon closure repair
shader response tuning
mask expansion
cracks/grooves
rock fracture/plate features
```
