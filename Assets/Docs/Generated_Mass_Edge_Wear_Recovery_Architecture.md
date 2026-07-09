# Generated Mass Edge Wear Recovery Architecture

Status: active recovery plan  
Current recovery target: EW-4D0 — Variable-Profile Topology Bevel Graph  
Current implementation step: EW-4D0.7 — Final Topology Validation and Active-Path Switch

## EW-4D0.7 final topology validation and active-path switch note

EW-4D0.6T7 proved the rebuild workspace can reach zero open edges, zero non-manifold edges, and zero T-junctions after pre-closure T-junction repair, branch-aware open-boundary cycle decomposition, and topology-scale closure-cap validation. EW-4D0.7 is the first active commit step: successful EW-4D construction now explicitly commits the rebuilt workspace to the generated polygon face list and bypasses the obsolete EW-4C half-space fallback. This is a correctness/visibility patch, not density tuning; committed ConvexEdgeWear faces also report estimated triangulated triangles and rendered vertices so EW-4D0.8 can reduce sample density by quality tier.

## 1. Current decision

Convex generated-mass edge wear is no longer a production atlas feature and is no longer being pursued through EW-4B local strips or EW-4C.0 half-space bevel planes.

The active production direction is:

```text
plane-cut GeneratedMass source faces
→ explicit topology graph
→ selected convex graph edges
→ per-face multi-edge clipping
→ shared clipped rails
→ sampled variable-profile bevel grids
→ bevel ribbon geometry
→ post-ribbon open-edge diagnostics
→ transactional open-cycle polygon caps without centre-fan radial edges
→ final topology audit
→ UV2.z / vertex color material markers
```

This path accepts higher dirty-time generation cost because GeneratedMass output is cached or can be generated offline. Runtime cost should stay reasonable: additional mesh triangles and existing mesh attributes are acceptable; per-object multi-megabyte runtime atlases are not.

---

## 2. Why previous representations are superseded

### Atlas-first edge wear, EW-3A series

The atlas-first path tried to represent a narrow convex edge feature as a packed low-resolution distance field. This failed because the useful hard ridge core was below atlas texel density at 128 and 256 resolution.

```text
128 atlas:
  edge feature was often below one usable pixel.

256 atlas:
  edge feature remained marginal and unstable.

512 atlas:
  more stable, but too expensive as a unique per-object runtime dependency.
```

FeatureAtlas0/1 remain valid only as temporary authoring/debug views or for future broad surface masks where texture representation is genuinely appropriate.

### EW-4A global bevel cuts

EW-4A proved that geometry is the right representation, but global cuts could affect unrelated faces, created slivers/gaps, and gave too little topological control.

### EW-4B local edge strips

EW-4B proved physical bevel strips could be generated and marked, but the construction produced cracks around corners and joints. EW-4B.7 added topology audit and then correctly rejected unsafe results, often yielding no visible bevel mask.

### EW-4C.0 half-space rebuild

EW-4C.0 avoided local strip/cap cracks in theory, but it assumed one selected edge support plane would become one visible bevel face. Unity validation disproved that assumption: selected candidates reached construction but no valid active bevel faces were produced.

EW-4D0 replaces the active construction path with explicit topology ownership and sampled bevel ribbons.

---

## 3. EW-4D0 design goals

EW-4D0 must support:

```text
- visible geometry bevels, not atlas-painted edges
- curved / multi-segment bevel profiles
- selected edge candidates preserved as authored intent
- deterministic per-edge variation
- deterministic along-edge variation
- future chipped / irregular rail boundaries
- final watertight topology validation
- UV2.z / vertex color markers for material response
```

EW-4D0 does not attempt to solve deep cracks/grooves. Those are a separate future feature layer because cracks are concave/incised paths, not convex bevels.

---

## 4. Control semantics

```text
Edge Wear Amount:
  Enables edge-wear generation and controls generated material strength.

Edge Wear Width:
  Controls base physical bevel depth before per-edge and along-edge variation.

Edge Wear Coverage:
  Controls which eligible convex graph edges are selected. Once selected, construction should not drop individual candidates to hide topology problems. If the selected set is too aggressive, width/profile richness should scale down globally or the whole attempt should fail closed.

Edge Wear Softness:
  Reserved for material response, normal policy, and profile feel. It should not secretly widen/narrow the physical bevel in the current implementation.

Macro Variation:
  Per-edge differences: width scale, profile curve, material strength, profile segment richness, chipped tendency.

Micro Variation:
  Along-edge differences: width/taper/noise/chip mask sampled along a single bevel edge.
```

---

## 5. Full EW-4D0 implementation plan

### EW-4D0.1 — Topology graph foundation — completed

Builds an explicit graph from the current `PolygonFace` list:

```text
vertices
edges
faces
edge → adjacent faces
face → ordered boundary edges
vertex → incident edges/faces
selected candidate → graph edge
```

Success proof from validation:

```text
selectedGraphEdges == selected
missingSelectedGraphEdges == 0
mismatchedSelectedGraphFaces == 0
duplicateSelectedGraphEdges == 0
invalidGraphFaces == 0
invalidGraphEdges == 0
```

### EW-4D0.2 — Per-face selected-edge clipping / shared rail preflight — completed

For every affected source face:

```text
1. collect all selected boundary-edge inset cuts on that face
2. clip the face once against all selected-edge offsets
3. extract the clipped rail for each selected candidate/face pair
```

Success proof from validation:

```text
faceClipFailedFaces == 0
expectedRails == selectedGraphEdges * 2
extractedRails == expectedRails
missingRails == 0
shortRails == 0
fragmentedRails == 0
```

### EW-4D0.3 — Rail/profile storage and sampled profile-grid preflight — completed

For every selected edge, pair its two extracted rails and prepare a sampled profile grid:

```text
P[t, k]

t = along-edge sample index
k = cross-bevel profile segment index
```

The current preflight does not emit geometry. It proves that each selected edge can create finite profile points from its two shared rails and its original sharp edge as a rounded-profile control line.

Success target:

```text
profileEdgesPrepared == selectedGraphEdges
profileEdgesFailed == 0
profileInvalidPoints == 0
profileZeroWidth == 0
profileGridPoints > 0
```

### EW-4D0.4 — Build clipped base-face replacement workspace — completed

Build a temporary rebuild workspace containing:

```text
unaffected source faces
+ clipped replacement base faces for affected source faces
```

Rules:

```text
- unaffected base faces remain unchanged in the workspace
- affected base faces are rebuilt from clipped polygons
- face feature remains Base
- face strength remains source base strength
- the workspace is not committed as final geometry in this step
- temporary open edges are allowed because bevel ribbons and corner patches are not appended yet
- no visible bevel mask is expected yet
```

Expected stats:

```text
preservedBaseFaces
affectedBaseFaces
replacedBaseFaces
baseFaceValidationFailures
workspaceFaces
workspaceBaseFaces
workspaceTemporaryOpenEdges
```

Success proof:

```text
affectedBaseFaces == faceClipAffectedFaces
replacedBaseFaces == faceClipSucceededFaces
baseFaceValidationFailures == 0
workspaceBaseFaces == graphFaces
```

### EW-4D0.5 — Rail-sampled base boundaries + sampled variable-profile bevel ribbons — completed

Insert protected rail samples into clipped base-face boundaries, then turn each profile grid into `ConvexEdgeWear` ribbon faces inside the temporary rebuild workspace.

For each selected edge:

```text
for each along-edge interval t0→t1:
  for each profile band k→k+1:
    create quad/face from P[t0,k], P[t1,k], P[t1,k+1], P[t0,k+1]
```

This step is still workspace-only: ribbon faces are built and counted, but final visible commit waits until corner patches and final topology validation.

Expected stats:

```text
railSampleInsertionFailures == 0
railSampledBaseFaces == affectedBaseFaces
railSamplesInserted > 0
ribbonEdgesPrepared == selectedGraphEdges
ribbonEdgesFailed == 0
ribbonFacesBuilt == ribbonFacesExpected
ribbonDegenerateFaces == 0
ribbonInvalidFaces == 0
workspaceConvexEdgeWearFaces > 0
```

### EW-4D0.6 — Corner vertex patches — completed but unsafe

At every original graph vertex touched by selected bevel edges, EW-4D0.6 collected endpoint profile arcs and built shared fan/radial patches inside the temporary rebuild workspace. This proved that corner patch faces can be generated and marked as `ConvexEdgeWear`, but the first implementation was not safe enough to final-commit.

Observed failure pattern:

```text
corner patches can build faces, but still leave unresolved workspace open edges
failed patches can partially append faces before returning false
corner dedupe used PointMergeDistance instead of validation-scale tolerance
corner normal fallback could silently become Vector3.up
workspaceOpenEdgesAfterCorners <= workspaceOpenEdgesAfterRibbons is too weak as a pass condition
```

EW-4D0.6 is therefore superseded by EW-4D0.6R before EW-4D0.7 is allowed.

### EW-4D0.6R — Corner patch hardening / failure diagnostics — completed as containment step

EW-4D0.6R is a containment and diagnostics patch, not a final visibility patch. It must make corner patch generation auditable before the active-path switch.

Implementation requirements:

```text
- build each corner patch into a local face list first
- append local faces to the rebuild workspace only if the patch is accepted
- if the patch fails, append nothing
- dedupe corner endpoint/profile points using validation-scale tolerance
- distinguish duplicate point rejects, insufficient boundary points, invalid normals, ordering failures, centre-too-close failures, small rejected faces, skipped degenerates, and hard failures
- replace unsafe Vector3.up normal fallback with a corner normal function that can actually fail
- count remaining open edges after corners and attribute them near vs away from graph vertices
- fail closed if open edges remain above the source/base graph boundary baseline
```

Expected stats:

```text
cornerPatchVertices > 0
cornerPatchAcceptedFaces > 0
cornerPatchFailed == 0
cornerPatchHardFailures == 0
cornerPatchInvalidNormals == 0
cornerPatchOrderingFailures == 0
workspaceOpenEdgesAfterCorners is substantially lower than the EW-4D0.6 baseline
workspaceOpenEdgesNearGraphVerticesAfterCorners and workspaceOpenEdgesAwayFromGraphVerticesAfterCorners identify where remaining holes are
```

Do not proceed to EW-4D0.7 until corner closure is genuinely near-safe, not merely generating faces.


### EW-4D0.6R2 — Workspace open-edge loop diagnostics — completed as proof

EW-4D0.6R validation proved that corner degenerates were eliminated, but remaining open edges were split between graph-vertex-local and away-from-vertex regions:

```text
cornerPatchFailed=2
cornerPatchDegenerateFaces=0
workspaceOpenEdgesAfterCorners=64
workspaceOpenEdgesNearGraphVerticesAfterCorners=30
workspaceOpenEdgesAwayFromGraphVerticesAfterCorners=34
```

EW-4D0.6R2 then proved the more important topology fact:

```text
workspaceOpenEdgesAfterRibbons=226
workspaceOpenEdgeComponentsAfterRibbons=22
workspaceOpenEdgeEndpointLeavesAfterRibbons=0
workspaceOpenEdgeEndpointBranchesAfterRibbons=0
workspaceOpenEdgesAfterCorners=64
workspaceOpenEdgeComponentsAfterCorners=5
workspaceOpenEdgeEndpointBranchesAfterCorners=14
```

The post-ribbon open edges are clean cycle-like components. The point-cloud corner fan stage reduces open edges but turns the remainder into branched topology. Therefore the active closure source must be the actual post-ribbon open-edge cycles, not endpoint profile point clouds.

### EW-4D0.6T — Actual open-cycle closure after ribbons — completed as topology-trace proof

EW-4D0.6T validation proved the open-edge topology source is correct, but the first implementation failed for a non-topological reason:

```text
openCycleClosureEdgesInput=226
openCycleClosureComponentsInput=22
openCycleClosureComponentsBuilt=2
openCycleClosureComponentsFailed=1
openCycleClosureFacesExpected=50
openCycleClosureFacesBuilt=0
openCycleClosureNonCycleEndpoints=0
openCycleClosureTraceFailures=0
openCycleClosureTooSmallCycles=0
openCycleClosureInvalidNormals=1
openCycleClosureDegenerateTriangles=0
openCycleClosureInvalidFaces=0
```

That means cycle tracing and endpoint valence were valid. The blocker was one aggregate cycle polygon normal that collapsed or cancelled. The next refinement must not reject an otherwise traceable open cycle just because a whole-loop normal is unstable.

### EW-4D0.6T2 — Per-triangle open-cycle closure normals — completed as open-edge proof

Validation result:

```text
openCycleClosureEdgesInput=226
openCycleClosureComponentsInput=22
openCycleClosureComponentsBuilt=22
openCycleClosureComponentsFailed=0
openCycleClosureFacesExpected=226
openCycleClosureFacesBuilt=226
workspaceOpenEdgesAfterComponentClosure=0
workspaceNonManifoldEdgesAfterComponentClosure=0
workspaceTJunctionsAfterComponentClosure=19
```

Verdict: per-triangle fan closure proved the open-cycle topology can close open edges and avoid non-manifold edges, but the radial centre-fan diagonals create T-junctions.

### EW-4D0.6T3 — T-junction-safe open-cycle polygon closure — completed as cap-shape proof

EW-4D0.6T3 kept the topology-owned post-ribbon open-cycle source but emitted one ordered ConvexEdgeWear polygon cap per traced cycle instead of centre-fan radial triangles.

Validation result to carry forward:

```text
openCycleClosureEdgesInput=226
openCycleClosureComponentsInput=22
openCycleClosureComponentsBuilt=22
openCycleClosureComponentsFailed=0
openCycleClosureFacesExpected=22
openCycleClosureFacesBuilt=22
workspaceOpenEdgesAfterComponentClosure=0
workspaceNonManifoldEdgesAfterComponentClosure=0
workspaceTJunctionsAfterComponentClosure=12
workspaceTJunctionsOnClosureEdgesAfterComponentClosure=0
workspaceTJunctionsOnBaseEdgesAfterComponentClosure=9
workspaceTJunctionsOnConvexEdgeWearEdgesAfterComponentClosure=3
```

Verdict: the cap shape is correct; remaining T-junctions live on Base and ConvexEdgeWear workspace edges and must be edge-split, not hidden by validation weakening.

### EW-4D0.6T4 — Workspace T-junction edge split repair — completed as failed-face diagnostic

EW-4D0.6T4 proved the repair target is correct but the first implementation was too brittle:

```text
workspaceTJunctionRepairCandidates=12
workspaceTJunctionRepairBaseCandidates=9
workspaceTJunctionRepairConvexEdgeWearCandidates=3
workspaceTJunctionRepairClosureCandidates=0
workspaceTJunctionRepairInsertedVertices=0
workspaceTJunctionRepairSkippedClosureEdges=12
workspaceTJunctionRepairFailedFaces=1
workspaceTJunctionsAfterTJunctionRepair=12
```

Verdict: keep the same edge-split repair target, but validate repaired split faces at topology-repair scale and expose exact failure reasons.

### EW-4D0.6T4b — Robust T-junction edge split repair — completed as tolerance/closure-edge diagnostic

Implementation requirements:

```text
- Preserve the EW-4D0.6T3 polygon cap closure.
- Use the topology audit's own T-junction definition: a unique workspace vertex lying on another face edge interior.
- For every affected non-closure Base or ConvexEdgeWear edge, insert the exact audited vertex into that polygon edge.
- Validate repaired split faces with topology-repair edge length, not the larger ribbon-generation edge length.
- Expose too-near-start/end, failed-no-insertion, failed-area, failed-short-edge, failed-non-finite, and applied-face counters.
- Repair on a cloned workspace-face list and commit it only if the repaired audit is clean.
```

EW-4D0.6T4b kept the T4 target but proved two more facts:

```text
workspaceTJunctionRepairCandidates=12
workspaceTJunctionRepairSkippedClosureEdges=12
workspaceTJunctionRepairTooNearStart=4
workspaceTJunctionRepairTooNearEnd=1
workspaceTJunctionRepairFailedShortEdge=1
workspaceTJunctionRepairInsertedVertices=0
workspaceTJunctionsAfterTJunctionRepair=12
```

Therefore the active repair must not skip closure cap edges, and repaired split-edge validation must use the same tolerance scale as the T-junction audit rather than a broader generation-stability edge length.

### EW-4D0.6T4c — Closure-inclusive T-junction split repair — completed as ordering proof

- Keep EW-4D0.6T3 polygon open-cycle closure unchanged.
- Include closure cap edges in `CollectTopologyTJunctionRepairRecords(...)`; do not skip them during active repair.
- Use `CalculateTopologyTJunctionTolerance(...)` consistently for T-junction detection and repaired split-edge validation.
- Expected first target from the current mass:

```text
workspaceTJunctionRepairSkippedClosureEdges=0
workspaceTJunctionRepairClosureCandidates > 0
workspaceTJunctionRepairInsertedVertices > 0
workspaceTJunctionRepairFailedFaces=0
workspaceTJunctionRepairFailedShortEdge=0
workspaceOpenEdgesAfterTJunctionRepair=0
workspaceNonManifoldEdgesAfterTJunctionRepair=0
workspaceTJunctionsAfterTJunctionRepair=0
```

- Keep EW-4D0.6T6 workspace-only so it cannot fall through into the obsolete EW-4C half-space path.

Validation target:

```text
workspaceOpenEdgesAfterComponentClosure == 0
workspaceNonManifoldEdgesAfterComponentClosure == 0
workspaceTJunctionRepairCandidates > 0
workspaceTJunctionRepairFailedFaces == 0
workspaceTJunctionRepairFailedArea == 0
workspaceTJunctionRepairFailedShortEdge == 0
workspaceTJunctionRepairFailedNonFinite == 0
workspaceTJunctionRepairAppliedFaces > 0
workspaceOpenEdgesAfterTJunctionRepair == 0
workspaceNonManifoldEdgesAfterTJunctionRepair == 0
workspaceTJunctionsAfterTJunctionRepair == 0
```


### EW-4D0.6T6 — Pre-closure T-junction repair and recomputed open-cycle closure — current

Evidence that motivates this step: EW-4D0.6T4c eliminated the 12 post-closure T-junctions by splitting audited Base, ConvexEdgeWear, and closure-cap edges, but the post-closure split introduced 4 non-manifold edges. That proves the split repair target is real, but the ordering is wrong: closure caps must be generated from the already-split boundary, not repaired afterward.

EW-4D0.6T6 keeps the T5 workspace sequence but changes closure tracing to branch-aware cycle decomposition: ribbons, audit/repair T-junctions in the post-ribbon workspace, recompute actual open-edge diagnostics, trace open cycles from the repaired boundary, then build polygon closure caps from that repaired cycle graph. The target remains strict: zero open edges, zero non-manifold edges, and zero T-junctions after component closure.

### EW-4D0.6T7 — Topology-scale closure cap validation — completed

Validation after EW-4D0.6T6 proved the branch-aware tracer is no longer the blocker: `openCycleClosureNonCycleEndpoints=0`, `openCycleClosureTraceFailures=0`, and `openCycleClosureTooSmallCycles=0`. It then failed on `openCycleClosureInvalidFaces=1` after one component built, so EW-4D0.6T7 keeps the topology path and changes only closure cap validation. Repaired split vertices must remain in cap boundaries; cap edge-length validation therefore uses the same topology/audit tolerance scale used by T-junction repair, not the broader ribbon-generation edge scale. New counters identify cap rejection by area, short edge, non-finite point, duplicate-point collapse, and min/max cap vertex count.

### EW-4D0.7 — Final topology validation and active-path switch — current

Commit the new EW-4D result only if it passes topology audit:

```text
open edges do not increase
non-manifold edges do not increase
T-junctions do not increase
builtBevelEdges == selectedGraphEdges
ConvexEdgeWear faces exist
```

At this step the old EW-4C.0 half-space fallback should stop being the active post-preflight path.

### EW-4D0.8 — Variation tuning pass

Add or tune deterministic style fields:

```text
per-edge width scale
per-edge profile curve/power
per-edge material strength
along-edge taper/noise/chip mask
safe rail wobble clamp
```

This step is where the bevels move closer to the reference rocks: varied edge width, less mechanical continuity, chipped/intermittent wear, and stronger/weaker sections on the same edge.

### EW-4D1 — Corner quality and crack/groove planning

If D0 fan corner patches are watertight but visually pinched, upgrade corner meshes. Separately plan crack/groove generation as a concave surface feature system rather than trying to force cracks into convex edge bevels.

---

## 6. Validation policy

Every EW-4D step must be validated by concrete stats before moving on.

Do not relax topology validation to make bevels visible. A black `Convex Edge Wear` debug view is preferable to a committed cracked mesh.

Do not add new visual debug modes unless an existing view cannot represent the data. Current preference is to keep diagnostics in compact console summary fields and use the existing `Surface Mask Debug = Convex Edge Wear` view once geometry exists.

---

## 7. Runtime and memory policy

Dirty-time compute may increase. The result is cached mesh data or can be generated offline.

Runtime concerns:

```text
acceptable:
  more triangles within mass budget
  existing UV2.z / vertex color markers
  deterministic generated mesh variation

avoid:
  unique multi-megabyte runtime atlases per mass
  runtime per-pixel atlas dependencies for hard edge wear
  secondary renderers or overlay meshes as final output
```

---

## 8. File scope policy

EW-4D0 should change only:

```text
Game/Procedural/Masses/MassGenerator.cs
Docs/Generated_Mass_Framework.md
Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md
Docs/Generated_Mass_Feature_Implementation_Checklist.md
```

Do not touch unless explicitly approved:

```text
Shaders
PixelSurface includes
GeneratedMass.cs
GeneratedMassEditor.cs
FeatureAtlas baker
GeneratedGround / ground generation
MeshData / MeshBuilder
River / foam systems
```
