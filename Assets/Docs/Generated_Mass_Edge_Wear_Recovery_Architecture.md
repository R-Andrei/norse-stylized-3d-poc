# Generated Mass Edge-Wear Recovery Architecture

This document defines the current architecture and invariants only. It is not a patch history or validation log.

The sole canonical progress ledger is:

```text
Docs/Generated_Mass_Feature_Implementation_Checklist.md
```

## Current status

The active experimental geometry direction is now a bounded single-edge bevel primitive built from a true one-edge local rail solve. Deterministic edge selection, local width feasibility, exact source-boundary ownership, provenance, final generated-face outward certification, and exact polygon/triangle certification remain useful.

Unity wireframe validation rejected infinite whole-rock half-space planes for both vertex junctions and edge bevels. A whole-rock edge plane can produce one manifold, planar, uninterrupted provenance-owned cap that still runs inward and bears little resemblance to the intended bounded rail quadrilateral. The next admissible geometry must geometrically modify only the two owner faces, emit one bounded bevel polygon, and close only the two local endpoint neighborhoods. Endpoint-adjacent non-owner boundaries may receive collinear subdivisions solely to share the local cap edges without changing their planar surface.

The legacy construction and repair path remains in the project as diagnostic comparison evidence. Ordinary production generation performs no edge-wear evaluation or edge-wear logging. In Edit Mode only, explicit non-serialized actions may evaluate the plane-cut clone or run the legacy comparison audit for one selected mass. Play Mode always uses production geometry.

```text
geometryCommit=disabled
```

remains mandatory until topology and visual preview are both approved.

## Evaluation-mode isolation

The generator has four explicit internal modes:

```text
None
PlaneCutPreview
LegacyDiagnosticAudit
BoundedSingleEdgePreview
```

`MassGenerator.Generate(...)` always uses `None`. It builds the unchanged production mass shell and does not enter edge-wear selection, corner solving, replacement/strip/patch construction, corrected-clone diagnostics, plane-cut solving, or edge-wear logging.

`PlaneCutPreview` is editor-only and runs only the shared edge selection/corner preparation plus the plane-cut kernel and its dedicated compact result. It does not execute the legacy reconstruction audit.

`LegacyDiagnosticAudit` is editor-only, explicit, single-object, clone-only evidence. It does not apply generated mesh data to the component, recook the collider, refresh the stable world fingerprint, or notify the geometry registry.

`BoundedSingleEdgePreview` is editor-only and evaluates one selected source edge through a direct local solve: the selected support line receives the requested width while each endpoint-adjacent support line remains at zero offset. It does not use the shared multi-edge corner solution or an infinite edge plane, geometrically modifies only the two owner faces, and adds exact graph-owned collinear boundary subdivisions to endpoint-adjacent non-owner faces for watertight local caps. It has its own transient mesh identity so lifecycle reuse cannot treat it as production.

This separation is mandatory. `OnEnable`, `OnValidate`, script reload, and Play Mode transitions may reconstruct a transient production mesh, but they may not run any diagnostic mode implicitly.

## Problem statement

The legacy construction emitted replacement faces, bevel strips, and corner patches independently, then attempted to repair overlap and incidence after assembly. The later convex recovery experiment replaced that with infinite whole-rock edge and junction half-spaces.

Both approaches are rejected as final geometry:

- independent strip/patch assembly can create incompatible shared boundaries;
- infinite junction planes cannot remain local to one endpoint neighborhood;
- infinite edge planes can intersect the whole convex rock and create a long inward cap even when topology, planarity, provenance, and band-integrity counters all pass.

The current bounded prototype solves the selected edge independently:

> For one selected source edge, offset only that edge on each owner face, keep each endpoint-adjacent edge at zero offset, intersect those support lines to obtain four rail points that lie on exact original adjacent boundaries, clip only the two convex owner faces against their local rail lines, emit exactly one bevel polygon between the rails, and close each endpoint with one local bounded cap.

Unity rejected reuse of the normal multi-edge corner solution for isolated closure: neighbouring bevel offsets move the shared corner into the owner-face interior, so it no longer belongs to the original adjacent boundary required by the prototype endpoint cap. The selected edge therefore uses a bounded deterministic width-backoff solve with all neighbouring offsets fixed at zero. Owner faces must not be reconstructed by manually splicing rail points into the source loop; each owner is intersected with a local in-plane half-plane so convexity is preserved by construction.

No unrelated source face may change its planar surface. Topology-only collinear boundary subdivision is permitted where a rail endpoint lies on that face so the local endpoint cap shares the exact same segment. No infinite bevel or junction plane participates in the bounded preview.

## Authoritative inputs

The bounded primitive consumes only established production/shared data:

- deterministic selected source edges;
- requested width and a deterministic isolated width-backoff limit;
- the two incident source faces;
- the four endpoint-adjacent graph edges and their opposite source-face owners;
- four isolated rail points that lie on those exact graph-owned boundaries;
- requested bevel normal;
- edge-wear material strength;
- original source-edge endpoints.

Diagnostic-only overlap, patch, contained-owner, or corrected-clone results are not inputs.

## Generation flow

```text
source convex polygon faces
    -> source topology graph
    -> deterministic edge selection
    -> stable eligible selected-edge order
    -> choose one editor-only source-edge ordinal
    -> solve the selected edge directly with only its support line offset
    -> deterministically back off width until all four rail points lie inside their exact adjacent source-edge segments
    -> bind each rail point to its graph-owned endpoint-adjacent source face and segment
    -> split those four exact non-owner boundaries at the rail points
    -> clip owner face A against rail A in its own plane
    -> clip owner face B against rail B in its own plane
    -> emit one bounded bevel polygon between the rails
    -> emit one bounded endpoint-cap triangle at each source endpoint
    -> preserve every unrelated source face surface unchanged
    -> one final polygon sanitation/conformity/seam pass
    -> orient only the generated bevel and endpoint caps outward from the original solid centre
    -> certify zero remaining generated-face winding failures
    -> certify provenance counts and rail fidelity
    -> certify polygon topology, bounds, and retained volume with separate evidence
    -> classify convexity on temporary duplicate/collinear-reduced loops
    -> triangulate every segment of the unchanged audited boundaries
    -> certify exact triangle topology, winding, bounds, and volume
    -> explicit editor-only preview
    -> later multi-edge owner-face reconstruction
    -> later bounded multi-bevel vertex-cap construction
    -> explicit production promotion
```

The rejected `PlaneCutPreview` remains available only as historical diagnostic evidence. It is not the implementation base for EW-B work.

## Rejected whole-rock plane candidate contract

The following contract remains documented for the explicit historical `PlaneCutPreview`; it is not the active EW-B primitive.


A candidate is accepted only when:

- its selected edge is an internal manifold edge;
- its bevel normal is finite and non-zero;
- all four solved rail points are finite;
- the rail points are coplanar within the approved geometry tolerance;
- both original source-edge endpoints lie outside the retained half-space by a meaningful amount;
- every unrelated original topology vertex lies inside the retained half-space;
- the plane is shifted outward when necessary to satisfy that locality rule;
- when this shift alone prevents meaningful removal of either selected endpoint, that edge is safely deferred and retains its original sharp geometry;
- every other candidate-construction failure remains a hard rejection;
- a candidate-specific clipping epsilon remains below the final measured removal.

The candidate stores the plane, material strength, clipping tolerance, and the measured minimum distance by which the plane removes the original source edge.

## Rejected whole-rock clipping contract

This contract likewise applies only to the retained plane diagnostic and shared certification helpers.


`ClipPolyhedron` clips all current faces against one plane, collects the shared intersection loop, emits one oriented `ConvexEdgeWear` cap when the cut has two-dimensional contact, welds shared vertices, and sanitizes the result.

Plane-cut diagnostics opt into:

- segment-clamped intersections;
- candidate-specific inside epsilon;
- canonical per-cut intersections keyed by the undirected current edge.

Legacy callers retain the previous defaults.

## Numerical seam repair

The final seam repair is deliberately narrow. It may modify the clone only when all of the following hold:

- both records are exact one-use open edges;
- they belong to different faces;
- they have opposite orientation;
- their corresponding endpoints differ only within a narrow topology-scale tolerance;
- each edge has exactly one mutual counterpart;
- snapping produces the exact expected reduction of two open records per pair;
- non-manifold and T-junction counts do not increase.

The repair snaps all occurrences of the involved endpoint keys to common midpoint targets and then welds. It rolls the entire operation back if any gate fails. It does not infer missing faces, bridge arbitrary holes, or merge ambiguous candidates.

## Redundant-cut classification

A cut may legitimately emit no new cap when previous cuts already place the entire current convex polyhedron inside its half-space.

The authoritative proof is the convex half-space result itself: every final vertex must satisfy the candidate plane under a tolerance that is numerically stable but remains below half of the candidate's measured minimum source-edge removal. Because the original source edge was proven outside that plane by the larger measured amount when the candidate was created, it cannot remain in its original position once the complete final polyhedron satisfies the stricter half-space test.

Approximate overlap with the old source-edge line is not authoritative because a nearby bevel boundary may be collinear with that line after earlier cuts. A no-cap candidate remains blocked whenever any final vertex lies outside the strict candidate-relative tolerance.

## Final topology gate

Every diagnostic clone must satisfy:

```text
planesRejected = 0
planesBuilt + planesDeferred = active
planesBuilt > 0
capsMissing = 0
open = 0
nonManifold = 0
tJunction = 0
invalid = 0
valid = 1

planeMesh.degenerate = 0
planeMesh.open = 0
planeMesh.nonManifold = 0
planeMesh.winding = 0
planeMesh.bounds = 0
planeMesh.volume = 0
planeMesh.valid = 1
```

Additional requirements:

- positive retained volume;
- retained volume no greater than source volume beyond numerical tolerance;
- final bounds contained by source bounds beyond clip-consistent tolerance;
- deterministic output for identical inputs;
- preserved `ConvexEdgeWear` feature strength;
- bevel-band integrity remains measured as structural evidence;
- local junction-star extraction remains diagnostic and does not alter the exact edge-only shell;
- no live geometry mutation.


## Bounded single-edge primitive contract

For one selected internal manifold source edge with owner faces A and B, EW-B1.2 solves four local rail points:

```text
Face A rail: a0 -> b0
Face B rail: a1 -> b1
```

At every endpoint/owner-face corner, the selected edge support line is offset by the candidate width and the one adjacent source-edge support line remains at zero offset. Their intersection must lie strictly inside that exact adjacent source-edge segment. The solver starts at the normal per-edge width, backs off deterministically for at most `12` attempts, and accepts the largest stable width.

Each solved rail point carries explicit ownership:

- owner graph/source face;
- source endpoint vertex;
- adjacent graph edge;
- opposite target graph/source face containing that adjacent boundary.

Boundary subdivision uses this topology directly. It may not search unrelated source segments by geometric proximity.

The prototype must produce:

- one stable isolated rail solution and four exact target boundaries;
- owner face A clipped locally by `a0 -> b0`;
- owner face B clipped locally by `a1 -> b1`;
- exactly one bounded bevel polygon `a0 -> b0 -> b1 -> a1`;
- exactly one endpoint cap at source vertex A;
- exactly one endpoint cap at source vertex B;
- one collinear boundary subdivision for each of `a0`, `b0`, `a1`, and `b1` on its graph-owned endpoint-adjacent non-owner source face;
- zero geometric surface changes to unrelated source faces.

The non-owner subdivisions are topological only: normalizing away collinear vertices must recover the original source polygon exactly. They are necessary because each local endpoint cap edge must be shared by the adjacent source face rather than ending as a T-junction.

EW-B1.3 canonicalizes every accepted rail to the exact graph-owned target boundary segment before any geometry is built. The canonical point must remain within tolerance of the analytical owner and target face planes, and the same position is authoritative for all downstream bounded geometry. Convexity certification may remove intentional collinear subdivision points from a temporary check loop only; the audited shell retains them for watertight topology.

The endpoint caps are an intentionally local prototype closure. They validate that bounded geometry can remain watertight without an infinite plane. They are not the final multi-bevel vertex-cap representation.

Preparation must retain face/provenance evidence across input validation, welding, boundary conformity, seam repair, and final validation. A failure must identify its stage, exact face index, provenance kind/index, and polygon failure category rather than reporting a generic owner-convexity message.

The exact audited shell must report:

```text
isolatedRailSolved = 1
targetBoundaries = 4
ownerClips = 2
boundarySubdivisions = 4
bevelFaces = 1
endpointCaps = 2
modifiedSourceFaces = 2
foreignSourceFacesModified = 0
railDeviation <= tolerance
maxExtentBeyondRails <= tolerance
open = 0
nonManifold = 0
tJunction = 0
invalid = 0
valid = 1
```

The editor cycles one eligible selected edge at a time through a non-serialized ordinal. A rejected edge returns production geometry and concise evidence; it does not alter production state or serialized recipe data.

## Rejected global vertex-junction solver evidence

The retained `MassGenerator.EdgeWear.PlaneCutJunctionSolver.cs` implementation records the rejected experiment that searched infinite junction half-spaces, candidate normals, cut depths, and deferred-edge states. It remains available for code-history evidence and shared non-junction utilities, but `AuditPlaneCutBevelKernel(...)` may not call `SolvePlaneCutGlobalJunctionSystem(...)`.

The following are no longer part of active preview evaluation:

- the `48`-state breadth-first search;
- the three-second junction-solver budget;
- direct/adaptive junction-plane trials;
- global junction-cap emission;
- junction-driven edge backtracking;
- `VertexJunctionPlane` faces in the active preview shell.

`planeSolve` must remain all zeroes during EW-L1 evaluation. A later bounded local-cap implementation must not reactivate these mechanisms.

## Bounded local junction-star extraction contract

The edge-only shell is a complete closed convex diagnostic result. EW-L1 does not modify that shell to create a junction. Instead, it constructs a temporary open local surface patch for each source vertex with at least two retained incident edge bevels.

For each candidate source vertex:

- use every source edge incident to the vertex to define the local bound;
- orient one cutback plane perpendicular to each incident source edge and facing away from the source vertex;
- derive cutback from solved bevel width plus the minimum stable geometry scale;
- cap cutback at `25%` of the corresponding source-edge length;
- clip copied face polygons individually, without clipping the complete rock and without emitting any cutback cap;
- retain only original source faces incident to the source vertex and edge-bevel faces owned by retained incident source edges;
- treat any other surviving provenance as a foreign-face extraction failure;
- require every retained incident bevel provenance exactly once;
- collect one-use boundary segments and require one connected component with degree two at every boundary vertex;
- order the loop deterministically from the smallest quantized vertex key;
- reject projected self-intersection, branches, disconnected components, missing incident bevels, and duplicate incident bevels.

The extracted loop is diagnostic data only. EW-L1 does not triangulate, project, fill, serialize, or render a local cap. Local extraction failure does not hide an otherwise exact topology-valid edge-only preview, because the immediate purpose is to distinguish edge-plane defects from rejected global-junction-plane defects.

## Bevel-band provenance and longitudinal-integrity contract

Face-level validity is not sufficient. A closed convex shell can still turn one intended bevel corridor into several individually planar generated faces that form a long inward-looking crease. The final authority therefore preserves explicit non-serialized provenance on every polygon face:

- original source face and source-face index;
- edge-bevel cap plane and source-edge index;
- vertex-junction cap plane and source-vertex index.

Clipping must preserve the provenance of every retained face and assign the requested provenance only to the newly emitted cap. Deep clones, final sanitation, conformity, and conservative seam repair must retain the same provenance unchanged.

For every retained edge candidate:

- exactly one final face must retain that edge-bevel provenance;
- the owned face must preserve meaningful axial coverage along the original source edge;
- generated neighbors may touch the band only inside local endpoint zones;
- a junction cap may shorten the incident band near its own source endpoint, but may not run longitudinally through the band interior;
- an unrelated junction or edge plane may not split the band inside the source-edge interior;
- a manifold multi-face corridor is invalid when the intended one-edge-to-one-band relationship is broken.

Junction influence is measured by projecting the junction/incident-bevel intersection onto the original source-edge axis. The maximum penetration and the axial span of that shared intersection must remain within a local allowance derived from solved bevel width and junction cut depth, capped at `25%` of source-edge length. This is a hard structural gate, not a scoring preference.

EW-L1 proved that edge half-spaces can also violate this contract: the representative edge-only shell retained one owned face per selected edge yet reported one interrupted band and one foreign edge-plane cut, while the inward multi-face crevice remained visible. Edge-band integrity is therefore authoritative from EW-L1.1 onward. A split, missing, collapsed, interrupted, foreign-cut, or overlong band may not be adopted as preview geometry.

## Deterministic clean-band conflict resolution

EW-L1.1 keeps the edge-plane experiment bounded and deterministic. It does not reactivate the rejected global junction solver. The active preview may evaluate at most `12` complete edge-only shells. Every pass starts from the untouched source shell and replays the currently retained edge candidates in stable order.

When `AuditPlaneCutBandIntegrity` finds a failure, it records:

- the victim source-edge index;
- the foreign cutting source-edge index when the adjacent generated face has edge provenance;
- the nearest responsible source vertex;
- victim axial coverage;
- foreign axial location and shared longitudinal span.

For an attributed victim/foreign pair, the existing stable backtracking priority compares localization burden, strength, selection score, width, source-edge length, and source-edge index. Only the weaker edge is deferred. A split, collapsed, or otherwise single-edge failure defers the victim edge itself. Conflict deferrals are counted separately from locality deferrals and hard candidate rejections.

A clean-band replay succeeds only when every retained edge owns exactly one face and all band failure counters are zero. If no deterministic edge can be attributed, every candidate is consumed, or the bounded pass budget is exhausted, preview adoption is refused and production geometry remains displayed. Local-junction extraction runs only on the final clean retained-edge set.

## Safe partial-preview contract

A locality deferral is not a topology failure. It is permitted only when the candidate was shifted outward specifically to retain unrelated source vertices and that shift makes meaningful removal of the selected source edge impossible. The edge is omitted from the cut list, so the original sharp edge remains intact.

A partial preview is valid only when:

- every non-deferred active edge builds successfully;
- no hard candidate rejection occurs;
- all built cuts have complete cap or redundancy accounting;
- every retained bevel owns one uninterrupted, unsplit, non-collapsed band with no foreign interior cut;
- the bounded conflict-resolution pass budget is not exhausted;
- the final polygon shell and exact triangle soup pass every existing gate;
- at least one bevel plane is built.

The editor must report built, active, deferred, and rejected counts. A hard rejection or downstream geometry failure still falls back to production geometry.

## Audited preview handoff

The preview must render the same representation that passed certification. The completed clone is sanitized once, conformed, seam-repaired, and audited. Conservative seam repair projects each paired endpoint onto the intersection of the two incident analytical face planes, bounds the displacement by the narrow seam tolerance, and rolls back unless topology improves without moving any face off its original plane. Those exact faces are triangulated with flat deterministic convex fans. The preview path must not invoke displaced-centre surface relief or perform a second polygon sanitation pass.

Every final `ConvexEdgeWear` face is certified for scale-relative plane deviation and triangle-normal spread. Any edge-wear face over `0.75` degrees of triangle-normal spread is invalid. Final junction caps must also retain at least `0.06` compactness and aspect ratio no greater than `12`.

The resulting triangle soup is independently checked for finite and non-degenerate triangles, outward winding, exact welded edge incidence, bounds agreement with the audited polygon shell, and volume agreement. Preview adoption is refused when polygon, face-quality, or triangle certification fails.

## Visual contract

EW-B1 renders exactly one bounded source-edge bevel at a time. The intended bevel must read as one outward face between the solved rails. Internal triangulation is permitted only when every triangle remains coplanar with the same analytical bevel face.

The following are unacceptable regardless of manifold topology:

- a long inward crease;
- a whole-rock cap extending outside the solved rail quadrilateral;
- more than one bevel polygon for the selected source edge;
- geometric modifications to unrelated source faces beyond required collinear boundary subdivision;
- endpoint closures that extend longitudinally down the rock.

Topology approval does not equal final visual approval. EW-B1 must prove the primitive before any multi-edge owner-face reconstruction or multi-bevel endpoint cap is attempted.

## Retained and retired approaches

Retained as evidence:

- legacy replacement-face, strip, and patch construction;
- overlap classification;
- contained-owner and corrected-clone diagnostics;
- source graph, selection, width, corner, and provenance utilities.

Retired as the intended production direction:

- global half-space planes used as vertex-local junction caps;
- infinite whole-rock half-space planes used as the final edge-bevel primitive;
- repairing independently emitted patch/replacement overlaps one category at a time;
- universal source-vertex patch centres;
- accepting area conservation without exact shared-boundary incidence;
- treating broad geometric proximity as proof that the original source edge survives;
- arbitrary open-edge closure.

## Temporary cross-feature preview rule

MG-X1 canonical-preview isolation is deferred while bounded bevel geometry is unfinished. Diagnostic previews currently replace the displayed GeneratedMass mesh and can therefore change the obstacle fingerprint observed by River tooling. This is accepted only as temporary development behavior.

Before authoritative River cache preparation or Edit-to-Play cache validation:

1. restore **Production Geometry** on every previewed GeneratedMass;
2. allow registry changes to settle;
3. prepare and validate the cache only from canonical production geometry.

River must continue treating a changed obstacle fingerprint as a real mismatch. The final pipeline remains one canonical production mesh, renderer, collider, fingerprint source, and River obstacle representation after bounded bevels are promoted.

## Production promotion gate

Production promotion requires two explicit approvals:

1. All representative diagnostic clones pass the full topology and geometry gate.
2. The bounded bevel architecture is visually approved across representative masses and control extremes after multi-edge owner-face reconstruction and bounded vertex caps are complete.

Only then may bounded edge-wear geometry replace the current live path. Removal or quarantine of superseded legacy and whole-rock plane experiments is a later cleanup decision, not part of topology promotion.

## Bounded foreign-face equivalence

EW-B1 foreign source faces may receive intentional collinear boundary subdivisions so endpoint-cap edges are shared exactly. Certification compares those faces as planar regions in a common 2D basis using area agreement and mutual containment. Equivalent regions with different boundary vertex cycles are reported as boundary-subdivided, not surface-modified. Any actual foreign region or area change remains a hard preview blocker.

## EW-B1.5 final bounded-shell result

EW-B1.5 proved one complete bounded shell on source edge `11`: `19` faces triangulated into `98` triangles with no polygon, topology, winding, bounds, volume, or triangle-soup failure. It also disproved generated-face winding as the cause of the edge `6` and `7` volume failures: both had zero reorientation and zero outward-winding failures.

## EW-B1.5R1 preparation-equivalent certification

Numerical preparation is part of the audited geometry state. A prepared bounded shell must therefore be compared against a source baseline that has passed through the same deterministic polygon copy, weld, boundary conformity, seam repair, and final validation stages. The raw source remains the strict bounds authority and remains fully reported, but retained-volume validity uses the prepared source volume. The accepted interval remains unchanged at `0.75 < result/preparedSource <= 1.0001`.

Generated Mass geometry diagnostics follow a cumulative evidence rule. When an unresolved blocker needs more evidence, new structured fields are added without deleting earlier still-relevant fields. Result preparation and source preparation independently report cardinality, topology, numerical repair, volume drift, and exact failure provenance. Bounds and volume report raw, prepared, and result states plus margins. This remains one structured record per physical evaluation; exhaustive telemetry must not become per-face or repeated lifecycle Console spam.
