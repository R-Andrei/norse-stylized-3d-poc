# Generated Mass Edge Wear Recovery Architecture

Status: active recovery plan  
Current recovery target: EW-4D0 — Variable-Profile Topology Bevel Graph  
Current implementation step: EW-4D0.5 — Rail-Sampled Base Boundaries + Bevel Ribbon Emission

---

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
→ corner vertex patches
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

### EW-4D0.5 — Rail-sampled base boundaries + sampled variable-profile bevel ribbons — current step

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

### EW-4D0.6 — Corner vertex patches

At every original graph vertex touched by selected bevel edges, collect endpoint profile arcs and build a shared patch.

Initial patch policy:

```text
- build stable ordered fan/radial patches first
- mark corner patch faces as ConvexEdgeWear
- prefer watertightness over perfect corner aesthetics
```

Future refinement can replace fan patches with an Adj-like corner pattern if corners look pinched.

Expected stats:

```text
cornerPatchVertices
cornerPatchesCreated
cornerPatchesFailed == 0
```

### EW-4D0.7 — Final topology validation and active-path switch

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
