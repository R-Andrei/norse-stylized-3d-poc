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

`BoundedSingleEdgePreview` is editor-only and evaluates one selected source edge through a direct local solve: the selected support line receives the requested width while each endpoint-adjacent support line remains at zero offset. It does not use the shared multi-edge corner solution or an infinite edge plane. It clips the two owner faces and the two endpoint-adjacent support faces, removes both original edge endpoints from the support boundaries, and emits one bounded bevel polygon with no separate endpoint caps. It has its own transient mesh identity so lifecycle reuse cannot treat it as production.

This separation is mandatory. `OnEnable`, `OnValidate`, script reload, and Play Mode transitions may reconstruct a transient production mesh, but they may not run any diagnostic mode implicitly.

## Problem statement

The legacy construction emitted replacement faces, bevel strips, and corner patches independently, then attempted to repair overlap and incidence after assembly. The later convex recovery experiment replaced that with infinite whole-rock edge and junction half-spaces.

Both approaches are rejected as final geometry:

- independent strip/patch assembly can create incompatible shared boundaries;
- infinite junction planes cannot remain local to one endpoint neighborhood;
- infinite edge planes can intersect the whole convex rock and create a long inward cap even when topology, planarity, provenance, and band-integrity counters all pass.

The current bounded prototype solves the selected edge independently:

> For one selected source edge, offset only that edge on each owner face, keep each endpoint-adjacent edge at zero offset, intersect those support lines to obtain four rail points that lie on exact original adjacent boundaries, clip the two convex owner faces, replace each endpoint-support source vertex with its two ordered rail points, and emit exactly one bevel polygon between the rails.

Unity rejected reuse of the normal multi-edge corner solution for isolated closure: neighbouring bevel offsets move the shared corner into the owner-face interior, so it no longer belongs to the original endpoint-adjacent boundaries required by the isolated prototype. The selected edge therefore uses a bounded deterministic width-backoff solve with all neighbouring offsets fixed at zero. Owner faces must not be reconstructed by manually splicing rail points into the source loop; each owner is intersected with a local in-plane half-plane so convexity is preserved by construction. Endpoint-support faces are different: each removes exactly one source corner and replaces it with the two canonical rails already lying on its incident boundaries.

No unrelated source face may change its planar surface. The only intended source-face changes are the two owner clips and the two endpoint-support corner clips. No infinite bevel or junction plane participates in the bounded preview.

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
    -> pair the two rails at each endpoint to one exact graph-owned support face
    -> replace endpoint-support source vertex A with its ordered rail pair
    -> replace endpoint-support source vertex B with its ordered rail pair
    -> clip owner face A against rail A in its own plane
    -> clip owner face B against rail B in its own plane
    -> emit one bounded bevel polygon between the rails
    -> emit no endpoint-cap polygons
    -> preserve every unrelated source face surface unchanged
    -> one final polygon sanitation/conformity/seam pass
    -> orient the generated bevel outward from the original solid centre
    -> certify zero remaining generated-face winding failures
    -> certify provenance counts and rail fidelity
    -> certify polygon topology, source-solid containment, result-global convexity, face intersections, bounds, and retained volume with separate evidence
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

Endpoint-support clipping uses this topology directly. It may not search unrelated source segments by geometric proximity.

The prototype must produce:

- one stable isolated rail solution and four exact target boundaries;
- owner face A clipped locally by `a0 -> b0`;
- owner face B clipped locally by `a1 -> b1`;
- exactly one bounded bevel polygon `a0 -> b0 -> b1 -> a1`;
- endpoint-support face A with its source vertex replaced by the ordered pair `a0/a1`;
- endpoint-support face B with its source vertex replaced by the ordered pair `b0/b1`;
- zero endpoint-cap polygons;
- exactly four intended source-face modifications: two owners and two endpoint supports;
- zero geometric surface changes to unrelated source faces.

The support clips remove the triangular source corners rather than covering them with duplicate coplanar triangles. Each new support-face rail edge is shared directly with the bevel, so no cap or T-junction is required.

EW-B1.3 canonicalizes every accepted rail to the exact graph-owned target boundary segment before any geometry is built. The canonical point must remain within tolerance of the analytical owner and target face planes, and the same position is authoritative for all downstream bounded geometry. Convexity certification may simplify duplicate or collinear points only on temporary classification loops; emitted support and owner boundaries retain every real rail segment required for watertight topology.

The earlier endpoint-cap closure is rejected. It duplicated the corner area retained by the support face and produced positive volume plus the visible inward triangular crease. Higher-valence endpoint reconstruction remains deferred to the later multi-edge vertex-cap stage.

Preparation must retain face/provenance evidence across input validation, welding, boundary conformity, seam repair, and final validation. A failure must identify its stage, exact face index, provenance kind/index, and polygon failure category rather than reporting a generic owner-convexity message.

The exact audited shell must report:

```text
isolatedRailSolved = 1
targetBoundaries = 4
ownerClips = 2
boundarySubdivisions = 4
bevelFaces = 1
endpointCaps = 0
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

## EW-B1.5R2 diagnostic certification

EW-B1.5R1 proved the positive retained-volume delta is construction-level rather than numerical preparation drift. EW-B1.5R2 therefore does not alter bevel geometry. It certifies the missing facts: local edge convexity, source-shell convexity, result containment against every original source plane, bevel-plane sidedness, and signed volume contribution by provenance. Topology-valid shells are triangulated even when retained-volume certification fails so polygon and triangle volume can be compared independently, but rejected geometry is never published as a preview.

The AABB containment record remains useful but is explicitly coarse. Source-solid containment is the per-source-plane half-space test. Candidate classification is diagnostic-only until the representative edges prove whether concave or orientation-invalid edges are entering the bounded path.


## EW-B1.6 endpoint closure correction

The prior bounded prototype's endpoint-cap architecture is superseded. In the old shell, each endpoint-adjacent source face retained the original source vertex and merely received two collinear rail subdivisions, while a separate triangle covered the same corner region. That produced coplanar face duplication, positive signed-volume contribution, and the visible inward triangular/multi-surface crease even though edge incidence appeared manifold.

The corrected single-edge shell is:

1. clip both selected-edge owner faces to their solved longitudinal rails;
2. at endpoint A, replace the original support-face vertex with the ordered pair `a0, a1`;
3. at endpoint B, replace the original support-face vertex with the ordered pair `b1, b0` according to that support face's own boundary order;
4. emit one bevel quad `a0 → b0 → b1 → a1`;
5. emit no endpoint-cap polygons.

Each bevel endpoint edge is therefore shared directly by the bevel and one clipped support face. The prototype accepts this closure only when both endpoint rails identify the same exact graph-owned support face and its two incident source edges. Higher-valence or ambiguous endpoint layouts are rejected and deferred to bounded multi-edge vertex reconstruction.

EW-B1.6 also adds two shell-level gates absent from the earlier audit: every result vertex must lie inside every outward result-face plane, and no two face interiors may overlap or intersect improperly. The earlier undirected edge-incidence audit remains necessary but is no longer treated as sufficient proof of a valid solid.
The bounded prototype now also requires `resultVolume <= preparedSourceVolume`; the former `1.0001` allowance is removed because a full-edge bevel is a subtractive operation. Endpoint telemetry retains the exact source and rail positions, support-plane normals, graph-edge parameters, and residuals so any future closure failure can be diagnosed from one evaluation record.


## EW-B1.6R1 prepared-baseline certification

EW-B1.6 proved that endpoint-support clipping produces a closed, convex, contained, subtractive shell, but its first validation run never reached triangulation because two audits compared unlike states. Source-face change certification compared a numerically prepared result against the raw source shell, and face-intersection certification treated every result-only detector pair as newly introduced without auditing the prepared source.

EW-B1.6R1 leaves the bounded geometry unchanged. The authoritative source-face modification comparison is prepared source versus prepared result; the prior raw comparison remains cumulative telemetry. Face intersections are audited identically on both prepared shells and keyed by stable face provenance. The audit records complete source, result, unchanged, changed, new, and resolved pair sets, including coplanarity, shared vertices, shared boundary edges, source-graph adjacency, and boundary-contact evidence. Preview adoption rejects only a newly introduced or materially changed pair with no actual shared boundary contact. Absolute detector pairs that already exist in the prepared source remain diagnostic evidence rather than being misattributed to the bevel.

This correction is deliberately not a geometry patch. It exists to let the already-subtractive one-bevel/no-cap construction reach triangulation when the source-to-result delta is clean, so visual inspection can finally determine whether the original inward multi-surface crease has been removed.

## EW-B1.6R2 provenance-preserving source-baseline certification

EW-B1.6R1 established equivalent prepared comparison domains but exposed that the untouched source baseline entered preparation without `SourceFace:i` provenance. The prepared-source face-change audit therefore skipped every baseline face, and intersection delta keys could not match the same physical source pair between source and result.

EW-B1.6R2 clones the untouched source polygons with explicit source-face provenance before calling the shared bounded preparation pipeline. The attributed raw clone drives the historical raw face-change comparison; the original raw shell remains the authority for geometric bounds, volume, containment, and source-solid tests. Prepared source and prepared result now carry the same stable identities through source-change and intersection-delta certification.

A cumulative provenance audit independently certifies the attributed raw source, prepared source, and result. Each state must contain exactly one valid `SourceFace:i` identity for every original source face, with no missing, duplicate, out-of-range, or null records. Generated bounded faces are permitted only as separately attributed non-source records. Provenance failure is an explicit blocker and may never degrade into a misleading zero-modification result.

This patch changes no bounded bevel geometry. It exists to allow the subtractive one-quad/no-cap construction to reach triangulation only after its comparison identities are complete and unique.

## EW-B2 unified all-edge preview architecture

The primary editor workflow is one `Rebuild Edge-Wear Bevel Preview` action. It runs retained corner and plane-cut diagnostics for evidence, evaluates all eligible bounded edge rails, and publishes only a certified unified bounded result. The superseded plane-cut mesh is diagnostic-only and may not become the displayed preview.

The unified prototype must not merge complete single-edge replacement rocks. It constructs one shared result from untouched source vertices and active rail points so each affected source face and vertex junction is represented once. The first EW-B2 implementation tests a point-cloud convex-hull reconstruction. This is an experiment until it emits certified faces; the isolated single-edge architecture remains the proven geometry baseline.

## EW-B2.1 diagnostic storage and failure localization

Unified hull evaluation carries an explicit stage:

```text
CandidateEvaluation
PointCloud
PlaneExtraction
FacetOrdering
FacetSanitation
FacetClassification
Preparation
TopologyCertification
Triangulation
Complete
```

Every failure records both `stage` and `failureStage` before returning. Active rail-solved plans are counted before hull reconstruction. Plane extraction records triple classification and plane creation/merge/prune counts even on failure. Facet construction records the exact failed plane, support points, ordered and sanitized boundary sizes, area, convexity result, and reason.

The Unity Console receives one bounded, high-value summary with the blocker first. Exhaustive point, rail, face, plane-diagnostic, provenance, and intersection evidence is rewritten on every evaluation to:

```text
Library/GeneratedMassEdgeWearTelemetry.txt
```

The file is editor-only diagnostic output and is not a serialized asset or production dependency. Console truncation must never again hide the decisive blocker. The combined geometry algorithm is intentionally unchanged in EW-B2.1; the next geometry decision is made only after the exact hull stage is observed.


## EW-B2.2 normalization-safe combined-hull plane extraction

The combined point-cloud hull must never rely on `Vector3.Normalize()` to decide whether a seed triangle is geometrically usable. `TryBuildBoundedConvexHullPlanes` now measures the raw cross-product magnitude, rejects clearly degenerate and near-degenerate seeds separately, and divides explicitly only after passing a scale-aware threshold with a `PointMergeDistance` hard floor.

Each retained `BoundedHullPlane` carries its seed triple, seed cross magnitude, and merged seed-magnitude range. Candidate insertion requires a finite unit normal, finite distance, and at least three supporting points. Before facet ordering, a final invariant pass certifies every plane's normal length, distance, support indices, support residuals, and non-degenerate planar rank. Any survivor that violates those invariants fails at `PlaneExtraction`; malformed data must not leak into `FacetOrdering`.

The Console retains only aggregate threshold and failure evidence. Complete per-plane seeds, normals, distances, and support-point sets are written to `Library/GeneratedMassEdgeWearTelemetry.txt`. Rail solving, point-cloud membership, facet geometry, and bevel selection are unchanged in EW-B2.2.

## EW-B1.7 one-surface bevel render contract

The non-negotiable local output is one planar bevel polygon. Polygon provenance alone is not sufficient certification. The previous bounded implementation created one `BoundedEdgeBevel` quad but triangulated every polygon through a centre fan. A four-vertex bevel consequently became four render triangles around an inserted centre vertex. Generated Mass mesh output then recalculated each triangle normal independently, supplied no explicit mesh normals, and seeded material variation from each duplicated triangle-soup vertex index. The internal fan therefore introduced both lighting and colour/mask discontinuities, producing the visible four-surface inward crease even though telemetry reported `bevelFaces:1`.

A bounded bevel region now has this render contract:

1. one `BoundedEdgeBevel` polygon owns the complete outlined region;
2. its full boundary lies on one authoritative `PolygonFace.Normal` plane;
3. triangulation emits exactly `n - 2` direct triangles and no centre fan vertex;
4. every emitted triangle carries the same authoritative surface normal;
5. every emitted triangle carries the same authored surface-group key, so duplicated vertices cannot create material seams inside the polygon;
6. `MeshData.Normals` is complete, so Unity does not replace the authored bevel normal through recalculation;
7. region telemetry certifies polygon count, boundary size, triangle count, authored-normal coverage, authored surface-group coverage, fan-vertex count, plane residual, geometric-normal deviation, and exact failure provenance.

For the current quad this means two triangles sharing one normal. The diagonal is only a GPU implementation edge and must have no lighting or geometric fold. The separate point-cloud all-edge experiment remains rejected as a complete reconstruction because it suppressed fourteen of fifteen solved edges; EW-B1.7 changes only the local surface representation.

## EW-B3 authoritative all-edge one-surface rebuild

The authoritative editor rebuild no longer invokes the experimental point-cloud convex-hull path. `MassGenerator.EdgeWear.Orchestration.cs` obtains one shared corner-aware width solution, calls `AuditPlaneCutBevelKernel`, and publishes that complete edge-only shell directly when certification succeeds. `MassGenerator.EdgeWear.BoundedAllEdges.cs` is retained only as historical rejected implementation evidence and is not executed by the normal preview workflow.

This is a deliberate reclassification of the edge-only plane shell. Earlier visual inspection interpreted the long narrow region as several bevel faces. EW-B1.7 proved that one four-vertex bevel polygon had been centre-fan triangulated into four independently shaded render triangles. The plane-band audit already reported one retained face per built edge and zero split/interrupted/foreign-cut/collapsed bands. With direct one-surface triangulation, authored normals, and authored surface groups, the edge-only shell is now the simplest complete simultaneous convex reconstruction.

`TryTriangulateBoundedPreviewFaces` is the shared final surface contract for both:

```text
PolygonFaceProvenanceKind.BoundedEdgeBevel
PolygonFaceProvenanceKind.EdgeBevelPlane
```

Every such polygon:

- remains one analytical polygon;
- is triangulated without an inserted centre vertex;
- uses a stable existing boundary vertex as direct-fan anchor;
- searches all boundary anchors when necessary to avoid degenerate direct triangles;
- writes one authoritative polygon normal across all emitted triangles;
- writes one stable surface-group identity across all emitted triangles;
- must pass one-plane residual and rendered-normal certification.

The all-edge shell additionally requires:

```text
BandRetainedEdgeCount == PlanesBuilt
BandSingleFaceCount == PlanesBuilt
BevelRegionFaceCount == PlanesBuilt
BevelRegionRenderValid == 1
```

Conflict handling remains explicit. The edge-only builder may defer an attributed incompatible edge through its bounded conflict pass, but it may not silently suppress a plane merely because a point-cloud hull discarded its rail points. The compact audit lists active, built, and deferred source-edge indices. `MaterializedEdgeCoverageValid` is separate from geometric validity and is true only when `PlanesBuilt == ActiveEdgeCount`.

The single editor action remains `Rebuild Edge-Wear Bevel Preview`. It writes one concise `GeneratedMass all-edge bevel rebuild audit` record and rewrites `Library/GeneratedMassEdgeWearTelemetry.txt` with the complete authoritative audit. No old plane-cut preview button, bounded-edge cycling button, or point-cloud hull button is exposed.

## EW-B3.1 staged all-edge certification telemetry

The authoritative edge-plane shell retains its geometry unchanged. `MassGenerator.EdgeWear.PlaneCutKernel.cs` now captures six deterministic polygon-stage snapshots:

```text
AfterPlaneConstruction
AfterSanitation
AfterWeld
AfterBoundaryConformity
AfterSeamRepair
FinalCertification
```

Each snapshot carries topology and face-quality invariants. The first stage with an open edge and the first stage with a non-planar edge-wear face are retained independently. Generated-face identity is stable through `PolygonFaceProvenanceKind` and provenance index; an edge-plane bevel is therefore diagnosed as `EdgeBevelPlane:<sourceEdge>`, and a junction cap as `VertexJunctionPlane:<sourceVertex>`.

Final face-quality failure records contain the complete failed polygon boundary with a signed residual for every vertex, the exact residual/spread thresholds, offending vertex and boundary segment, authored/measured normals, area, minimum edge length, and preparation-touch evidence. Final open-edge records resolve their owner, nearest source vertex, incident built bevels, expected boundary or junction neighbour, nearest reversed segment, and mismatch distance. Vertex-junction coverage is reported for every source vertex touched by the retained bevel set.

`MassGenerator.EdgeWear.Diagnostics.Logging.cs` emits one bounded Console record. It places `primaryFailure` first, followed by the stage timeline and capped examples. The full failure set is written to `Library/GeneratedMassEdgeWearTelemetry.txt` in named sections. This telemetry is diagnostic-only and must not be used to weaken planarity, topology, one-surface, or complete-coverage gates.

## EW-B3.2 plane-exact clipping and radius welding

EW-B3.1 localized two independent numerical faults at `AfterPlaneConstruction`. `EdgeBevelPlane:17` inherited one vertex with an authored-plane residual of `6.68764114E-05`, which is inside the clipping classification tolerance but outside the strict face-planarity tolerance. Separately, two reversed source-face boundary pairs remained open despite endpoint mismatch of only `5.96046448E-08`, because rounded `VertexKey` identity is not equivalent to the declared Euclidean `PointMergeDistance` contract.

The authoritative edge-plane shell therefore uses an explicit numerical construction contract:

1. A genuine signed-distance crossing is solved on the source edge and the result is projected onto the analytical cut plane.
2. A tolerance-only inside/outside transition with no strict crossing selects the nearer endpoint and projects it onto the cut plane; an off-plane tolerated endpoint is never inserted directly into a bevel cap.
3. The same projected point is stored in the shared-intersection cache and cap-point set.
4. All cap points are projected again before deduplication. A sanitized cap is emitted only when its maximum residual is no greater than `PointMergeDistance * 0.25`.
5. Shared shell vertices are canonicalized by actual Euclidean distance under `PointMergeDistance`, with deterministic first-point ownership and no averaging.

This behavior is opt-in through `PlaneCutNumericalRepairTelemetry` and is enabled by the authoritative all-edge shell. Legacy clipping callers retain their previous behavior until separately audited. The stage timeline and failure dossiers remain authoritative. Numerical telemetry records both the repair work and its maximum movement so a successful result cannot hide an excessive snap.
## EW-B3.3 strict sequential clipping contract

EW-B3.2 proved that radius welding is correct and closed both numerical source-face seams, but its tolerance-only projected fallback is rejected. The reference run contained one fallback with `maxProjection:6.70406152E-05`; the resulting `EdgeBevelPlane:16` polygon had a matching `minEdge:6.70406152E-05`, residual `6.60419464E-05`, and `88.973671°` normal spread. The fallback emitted a projected endpoint and retained the original endpoint because the broad removal epsilon classified it as inside, producing a tiny off-plane hook in an existing bevel face.

The authoritative exact shell now separates removal tolerance from final geometric classification:

1. `PointMergeDistance * 0.25` defines strict `Inside`, `OnPlane`, and `Outside` states.
2. Only a genuine `Inside ↔ Outside` edge emits an analytical segment-plane intersection.
3. `Outside ↔ Outside` emits nothing. Same-side endpoint projection is forbidden.
4. `OnPlane` endpoints may be snapped only within the strict tolerance.
5. A sequential intersection must satisfy both the current cut plane and the owner face’s authored plane. Raw segment interpolation is retained when it already satisfies both; otherwise a closest two-plane correction is attempted.
6. Cached intersections are revalidated against both planes before reuse.
7. Collected cap points are validated, not globally reprojected. A failed residual aborts the cut transaction.
8. Any strict classification, denominator, cache, plane-residual, or cap invariant failure aborts the current cut before the polyhedron is replaced.
9. Deterministic true-distance welding from EW-B3.2 remains authoritative.

`PlaneCutNumericalRepairTelemetry` remains one cumulative record per physical evaluation. It now distinguishes classifications, on-plane snaps, genuine crossings, prohibited fallback attempts, two-plane corrections, owner/cut residuals, cap validation, exact construction failures, and the first stable owner/cut provenance failure. The six-stage EW-B3.1 timeline and complete failed-face/topology dossiers remain unchanged.

## Maximum Coverage contract — EW-B4.1

The accepted EW-B3.3 edge-plane shell can construct many simultaneous one-surface bevels with closed topology and exact planarity. Coverage selection is now separated into two layers:

1. **Structural eligibility** determines whether an edge can legitimately be beveled. The edge must have exactly two owner faces, finite usable normals, a segment longer than `max(PointMergeDistance * 4, maximumDimension * 0.00001)`, a valid outward orientation, a convex bounded-edge classification, and a non-coplanar owner-face relationship.
2. **Artistic preference** ranks structural edges by the established relative-length, face-angle, vertical/base suppression, random, upward-facing, and edge-character terms.

For Coverage below its maximum, the historic artistic filters and score ordering remain authoritative. At maximum Coverage, artistic filters no longer remove structurally eligible edges; every structurally eligible edge enters the selected set. This applies to the complete rock and is independent of the current camera.

`materializedCoverage` has a stronger meaning in maximum mode. It is true only when:

```text
structurallyEligible == selected
selected == built
widthInactive == 0
deferred == 0
rejected == 0
unmapped == 0
```

EW-B4.1 intentionally does not change the corner-width solver or locality deferral. A structurally selected edge may therefore still report `width-inactive`, `plane-locality-deferred`, `shell-conflict-deferred`, or `plane-rejected`. EW-B4.2 must resolve those exact measured categories through coordinated width reduction or a narrowly proven locality correction.

### Edge lifecycle telemetry

One `EdgeWearEdgeLifecycleRecord` is retained per source edge for the physical evaluation. Each record carries stable graph-edge identity, endpoints, owner-face evidence, length, dihedral, vertical position, structural classification, artistic status, candidate reason, selection, solved width, active state, plane result, and final reason. The Console reports compact aggregate counts and exact failure-category ID sets. The overwritten telemetry file includes one lifecycle line per source edge.

The former explicit-junction coverage summary is retained only as a labelled legacy heuristic. The authoritative edge-plane shell is judged by closed topology, manifold incidence, T-junction absence, face quality, one-surface rendering, bounds, and volume—not by whether obsolete explicit junction-cap faces were emitted.


## EW-B4.2 maximum-Coverage conflict contract

Maximum Coverage is now a complete-materialization mode. The edge-plane shell must retain every structurally selected candidate throughout conflict solving. The lower-Coverage artistic path may retain the historical deterministic candidate-deferral policy, but maximum Coverage may not.

When `AuditPlaneCutBandIntegrity` identifies a victim edge, foreign edge, or offending source vertex, the maximum-Coverage solver constructs one local cluster from those seed edges and all selected edges incident to their endpoints. It then moves every reducible cluster plane toward its source edge by a shared bounded scale step and rebuilds the complete shell.

Plane reduction preserves:

- the existing bevel-plane normal;
- source-edge provenance;
- positive source-edge removal;
- the strict owner-plane/cut-plane intersection contract;
- true-distance welding;
- one-surface render certification.

The minimum scale for each edge is derived only from the existing `PointMergeDistance` and `minimumStableEdgeLength` source-removal/width floors. Reaching that floor is an explicit unresolved conflict, not permission to delete the edge.

Maximum-Coverage completion requires equality across structural eligibility, selection, active positive-width edges, and built bevel faces. Geometry validity and coverage validity are reported separately. A partial but manifold shell is not a successful maximum-Coverage result. The audit may retain its geometry-valid evidence, but the preview triangle soup is withheld unless exhaustive coverage validity also passes.

The legacy local-junction-star extraction remains non-authoritative for the edge-plane shell. Its textual blocker is retained only in detailed telemetry and does not override a closed, manifold, planar shell.

## Stable incomplete rollback baseline

EW-B4.1 is the current immutable stable rollback point. It produces a closed, manifold, planar, one-surface-render-valid mesh with `36` of `40` structurally eligible bevels. Edges `{0/8/19/37}` are deferred, so the baseline is intentionally incomplete, but it is suitable for restoring known-good geometry while later exhaustive-coverage experiments remain invalid. Experimental patches must not overwrite or redefine this baseline.

## EW-B4.2R1 exact topology and locality evidence contract

EW-B4.2 conflict-cluster reduction reached `39` materialized bevels but introduced one T-junction at `AfterPlaneConstruction`; edge `0` remains deferred earlier by the plane-locality gate. The next step is diagnostic-only.

The T-junction dossier reproduces the same numerical predicate used by `AuditEdgeWearTopology`. For each unique junction vertex it identifies the unsplit host segment, exact host and vertex-owner provenance, segment parameter, closest-point residual, topology tolerance, associated bevel provenance, nearby candidate planes, current candidate widths/scales, and the latest conflict-reduction cluster that modified an associated edge. Records are captured at every stage where the defect remains, while `FirstTJunctionStage` identifies introduction.

Plane-locality deferral is a separate construction phase and has its own dossier. The record preserves the solved bevel plane, the unrelated-vertex guard that moves it, the limiting unrelated vertex, and the resulting source-edge removal failure. This distinguishes an intrinsically invalid solved plane from a plane made non-materializable by global unrelated-vertex preservation.

EW-B4.2R1 changes no geometry. It does not loosen topology tolerance, split host segments, raise the width floor, change conflict clusters, or modify locality. The subsequent correction must be chosen from the exact owner/host/conflict and source-removal evidence.
## EW-B4.2R2 topology-aware conflict retry contract

The maximum-Coverage width solver is transactional at the complete-shell level. A retry is accepted only when the prepared shell passes both the bevel-band audit and the same topology/face-quality invariants used by final certification.

A topology-breaking retry is not allowed to become the new reduction baseline. The solver retains the latest topology-clean scale map, rolls the full map back to that state, expands the interaction cluster, and then applies a coherent replacement reduction before rebuilding from immutable original candidates.

For T-junction-driven expansion, the cluster contains:

1. bevel provenance owning the junction vertex or host segment;
2. candidate bevel planes passing through the junction neighbourhood;
3. the latest earlier conflict cluster that modified any implicated bevel;
4. one bounded incident source-vertex star around those seed edges.

The topology audit is authoritative. Band-clean but topologically invalid geometry is rejected. Tolerances are not loosened to conceal near-coincident unsplit segments.

Edge `0` remains a separate cooperative-locality problem. EW-B4.2R2 intentionally preserves its explicit locality deferral and targets a valid 39-of-40 experimental shell. EW-B4.1-STABLE remains the immutable rollback baseline.

## EW-B4.2R2 failed replacement-trial finding

EW-B4.2R2 correctly refused the original T-junction retry, restored the latest topology-clean scale map, and applied one expanded-cluster replacement. That replacement did not preserve geometry validity: at `AfterPlaneConstruction` it contained three open edges and one bevel face with `0.886028051` degrees of normal spread against the `0.75` degree contract. No T-junction remained.

The R2 mapper was specialized to `PlaneCutTJunctionFailureRecord`. It therefore had no structured input for an open-edge/non-planar retry and aborted. The failure also exposed an audit-state flaw: a 39-plane attempted shell was later represented as zero built and 39 structurally rejected edges. These are transaction semantics failures, not candidate-selection results.

## EW-B4.2R3 transactional retry evidence contract

A solver pass has four independent states:

1. **attempted** — the complete candidate set and scale map were used to construct a trial shell;
2. **band-clean** — the attempted shell passed bevel-band integrity, regardless of topology;
3. **topology-clean** — the attempted shell passed open/non-manifold/T-junction/invalid/non-planar certification, regardless of band integrity;
4. **fully certified** — the same immutable trial passed both band and geometry contracts.

Each retained state owns cloned candidates, cloned faces, the complete scale map, pass identity, and the relevant stage snapshot. A later failed retry cannot mutate or relabel an earlier clean state.

Retry failures are captured at their earliest material stage. The generalized dossier hierarchy includes open edges, non-manifold edges, T-junctions, invalid faces, and non-planar bevel faces. Every record retains stable face/bevel provenance and contributes structured source-edge IDs to a generalized interaction cluster. Nearby candidate planes, the latest intersecting conflict pass, and one bounded incident source-vertex star are attribution evidence, not automatic permission for another geometry reduction.

EW-B4.2R3 deliberately preserves geometry behaviour. The established T-junction-driven R2 retry remains unchanged. A generalized non-T-junction failure is recorded and mapped, then the solver stops. The next width correction is selected only after Unity identifies the exact failed bevel, all open-edge owners, and the prior scale transition that produced them.

Lifecycle reporting distinguishes:

- `attemptedBuilt`: a bevel plane existed in the latest attempted shell;
- `certifiedBuilt`: that bevel belongs to the latest fully certified shell;
- `trialRejected`: attempted geometry failed solver certification;
- `localityDeferred`: the edge never entered the shell because its plane was infeasible;
- `rejected`: the source edge or plane failed a true structural/construction requirement.

EW-B4.1-STABLE remains immutable. EW-B4.2R3 is diagnostic and transactional infrastructure for the experimental 39-of-40 path; it is not a new stable geometry baseline.

## EW-B4.2R4 minimal topology retry contract

A topology-triggered width retry is no longer an extension of the ordinary band-conflict cluster solver. The exact T-junction record defines a separate bounded transaction.

The immutable rollback input is the complete latest topology-clean scale map. Saved rollback faces are evidence and fallback output only; they are never incrementally clipped into a new trial. Every factor trial starts from the original source polyhedron and original feasible candidate planes.

For each exact linked edge:

```text
requestedScale(edge) = topologyCleanScale(edge) * factor
effectiveScale(edge) = max(requestedScale(edge), existingNumericalFloor(edge))
```

No common absolute target is propagated from the failed state. Edges outside the exact cluster must remain byte-for-value equivalent within the existing float comparison tolerance. Any outside-cluster change is a solver defect and rejects the complete trial.

The initial bounded factor sequence is:

```text
0.95
0.90
0.85
0.80
```

Each factor independently rebuilds and certifies:

- bevel-band completeness and single-face ownership;
- cap survival or proven redundancy;
- zero open, non-manifold, T-junction, and invalid-face defects;
- zero face-quality failures under the existing residual and `0.75` degree spread limits;
- retained volume and source-bounds containment;
- one authored planar surface per bevel with no centre fan;
- triangulation and preview triangle-soup validity.

The first fully valid factor in descending order is the highest tested valid factor and may be committed. If none succeeds, the solver restores the prior topology-clean state, reports local infeasibility, withholds the preview, and does not automatically import the previous conflict cluster or an incident source-vertex star.

Each trial records its rollback, requested, and effective scales, any floor hits, every scale changed outside the cluster, attempted built count, stage-evaluation status, band/topology/face-quality/render/mesh validity, and every captured exact face/open-edge/T-junction failure dossier. The audit explicitly records `failedStateScalesReused=0`; an unresolved search names the restored topology-clean fallback state.

The lifecycle has two distinct success gates:

1. `solver-clean`: band and complete per-trial geometry/render/mesh certification passed inside the bounded search;
2. `fully-certified`: the authoritative outer final certification has repeated and accepted the complete shell.

`certifiedBuilt` is populated only at the second gate. Any later final failure demotes every attempted plane to `trialRejected`, clears the certified state, and leaves structural `rejected` reserved for genuine candidate/construction rejection.

Edge `0` remains an independent cooperative-locality problem. EW-B4.2R4 targets only the feasible 39-plane shell and cannot supersede `EW-B4.1-STABLE` until Unity produces a closed, planar, render-valid, mesh-valid 39-of-40 result.

## EW-B4.2R5 direct foreign band-plane retreat contract

R4 proved that a topology failure dossier and the width degree of freedom required to clear the preceding band conflict are not necessarily the same set of edges. The exact T-junction remains attributed to `{7/8/20}`, but the complete factor log showed that every R4 trial failed because unchanged `EdgeBevelPlane:9` continued to split victim edge `8` near axial parameter `0.9642-0.9643`.

R5 therefore derives two independent structured sets:

```text
topologyLinked = exact T-junction LinkedEdgeIndices
retreatEdges   = directly evidenced foreign edge from the latest prior
                 band-integrity record whose victim is topology-linked
```

For the reference shell:

```text
topologyLinked={7/8/20}
victim=8
foreign=9
retreatEdges={9}
```

No source-edge ID is hardcoded. The solver walks prior structured conflict records from newest to oldest, accepts only a `band-integrity` record whose victim belongs to the topology-linked set, and requires its foreign edge to remain an active bevel candidate.

Every trial clones the complete latest topology-clean scale map and applies one factor only to the retreat set:

```text
trialScale(9) = pass7Scale(9) * factor
all other scales = exact pass-7 values
```

The bounded sequence is:

```text
0.95
0.90
0.85
0.80
0.75
```

The final sample reproduces the known foreign-plane scale `0.133483887` that previously advanced beyond band integrity, while leaving `7/8/20` at the topology-clean pass-7 scale `0.177978516`.

The complete R4 transaction and certification rules remain active. Every trial rebuilds from immutable source faces and original candidates, rejects any outside-retreat scale change, and must pass band, cap, topology, face-quality, volume, bounds, one-surface, triangulation, and preview-mesh certification together. Failure restores pass `7` and does not trigger automatic pair search or cluster expansion.

Telemetry remains cumulative rather than compressed. The full file section is renamed `[Direct Foreign Band-Plane Retreat Search]` and retains one complete record per factor. It additionally records:

- `searchMode=direct-foreign-band-plane-retreat`;
- the direct prior band record (`bandPass`, victim, foreign, axial parameter, shared span);
- the unchanged topology-linked dossier;
- the exact retreat set;
- rollback/requested/effective scales and floor hits;
- collateral changes;
- every certification result and exact failure record.

The Console retains only the compact equivalent. The file log is the authoritative evidence source for subsequent solver decisions.



## EW-B4.2R6 dual-endpoint retreat contract

The R5 trial sequence proves that victim edge `8` is interrupted by two different foreign bevel planes at opposite axial ends. As edge `9` retreats, its violation moves toward axial `1` and eventually disappears; the remaining active blocker becomes edge plane `7` near axial `0.03006`. Both blocker transitions occurred while topology and face quality remained clean.

R6 retains two evidence layers:

```text
topologyLinked = {7/8/20}
retreatEdges   = two directly observed foreign planes for victim 8
protectedEdges = topologyLinked - retreatEdges
```

For the reference shell these resolve dynamically to:

```text
retreatEdges={7/9}
protectedEdges={8/20}
```

No source-edge number is embedded in the solver. The first retreat edge is obtained from the latest structured pre-T-junction band record. The opposing edge is obtained from a topology-clean direct-retreat trial whose structured band audit reports the same victim and a different active foreign plane.

Each dual trial clones the complete immutable topology-clean scale map and applies a shared factor only to the two endpoint planes. Protected edges remain byte-for-byte equivalent in scale to pass `7`. Every trial is rebuilt from original source faces and candidates; no failed faces, intersections, or scales become input to a later trial.

The bounded factor sequence remains:

```text
0.95
0.90
0.85
0.80
0.75
```

The final factor reproduces the known endpoint scale `0.133483887` for both planes while leaving `8/20` at `0.177978516`. This isolates the band-helpful portion of the old broad pass without repeating its topology-linked reductions.

Telemetry remains cumulative. Direct search trials and dual search trials are written to separate sections but retain one shared trial schema. Each trial now records search mode, protected edges, structured band victim/foreign IDs, axial position, span, scale transitions, collateral changes, and every certification result. `primaryFailure` remains historical solver provenance; `activeSearchFailure` identifies the current terminal trial blocker.

### Independent editor source-edge index debug

Source-edge indexing is a standalone editor diagnostic. It invokes a dedicated `SourceEdgeIndexDebug` generation mode that reconstructs the current plane-cut source faces, builds the authoritative `EdgeWearTopologyGraph`, and captures every graph edge before any bevel transaction is attempted. The record build does not require edge-wear amount, coverage, corner solving, band certification, topology retry, preview publication, or a valid bevel mesh.

The same generated-mass dimension, deterministic lean, grounding, recenter, and object transforms are applied to the independent records so the numbers align with the rendered production mass. Records are stored in a separate non-serialized editor cache on `GeneratedMass`; they are not owned by `UnifiedEdgeWearPreviewStatus` and remain usable when the bevel preview fails.

When enabled, the Scene renderer always draws the complete source graph. Structured bevel-search focus IDs may change line and label colour, but they never filter or suppress unrelated edges. The reference rock therefore reports `44 shown / 44 total`, with `{7/8/9/20}` highlighted when current search evidence exists. A manual refresh action rebuilds the source graph after recipe or shape changes.

No GameObject, component, layer, tag, material, mesh channel, production setting, serialized field, or runtime branch is added.

## Canonical edge viability preflight

The edge-plane shell no longer treats every structurally convex source segment as a bevel candidate. Before Coverage or corner solving, each source edge receives one immutable-source viability record.

The preflight order is:

```text
source topology
-> structural manifold and convexity checks
-> minimum 15-degree dihedral
-> two-width longitudinal footprint check
-> cached independent-plane locality interval
-> bounded isolated-edge construction certificate
-> minimum 25% locally feasible width
-> endpoint-transition central-span certificate
-> artistic ranking and Coverage
-> corner solving
-> global interaction solving
-> final shell certification
```

The locality test is an interval test rather than a repeated plane scan. For the normalized bevel normal, the preflight caches:

```text
retainFloor = max(unrelated source vertex projection + guard,
                  solid centre projection + guard)
removalCeiling = min(source endpoint projections) - minimumRemoval
```

The standard independent plane is viable only when `retainFloor <= removalCeiling`. Plane construction later reuses this interval and may not rescan source vertices. If a globally solved plane leaves the cached interval, that is a later interaction/solution failure, not a reason to reinterpret individual viability.

The bounded isolated-edge certificate is executed once for every edge that survives the cheap gates. Its result is cached with solved width, width fraction, owner/support evidence, endpoint consumption, remaining central span, topology, containment, bounds, retained volume, triangulation, and exact failure diagnostic.

Maximum Coverage is defined over geometric-eligible edges. Structural or geometric exclusions remain present in source-edge telemetry and Scene indexing, but they are not missing bevels and do not invalidate coverage.

The R4-R6 transaction searches remain global interaction diagnostics for edges that passed this preflight. They no longer act as a substitute for deciding whether a source edge deserved to enter the shell.

## Immutable source placement frame

Edge-wear reconstruction operates in normalized source-polyhedron coordinates, but the rendered mass also receives dimensions, deterministic lean, nonlinear grounding, and contact-centre recentering. Those placement parameters are properties of the authored source mass, not of the final triangulation.

For a plane-cut bevel preview, the generator now triangulates the unmodified authored source faces once before edge-wear evaluation and retains that triangle soup as the placement reference. The placement pipeline is resolved sequentially from that reference:

```text
dimensioned source reference
-> resolve and apply source lean
-> resolve and apply source grounding
-> resolve source contact centre and vertical offset
-> immutable completed placement frame
```

The completed frame is then applied without recomputation to:

```text
certified bevel output
independent source-edge debug records
```

A reconstructed bevel soup may change bounds, vertical extrema, low-vertex multiplicity, and triangle duplication. It is therefore prohibited from supplying placement parameters. The GameObject Transform is never modified by this process.

Ordinary generation remains behaviorally unchanged: when there is no separate reconstructed output, the ordinary output soup is also the placement reference and follows the established placement path.

## Viability audit integrity and stable baseline contract

The accepted R7 viability gate and R7R1 placement frame remain geometry-authoritative. R8 adds an evidence contract around them without changing candidate inclusion or shell construction.

### Attempted width is not certified width

The bounded isolated audit may reduce width repeatedly before failing. The final attempted width is useful diagnostic evidence, but it is not a certified feasible width unless the complete isolated construction passes. Telemetry therefore distinguishes:

```text
isolatedSucceeded
lastAttemptedWidth
maximumCertifiedWidth
maximumCertifiedWidthFraction
```

A failed isolated certificate always records zero certified width, even when the final rail attempt reached a small numerical value. Internal R7 decision fields remain untouched so this correction cannot alter the accepted viable set.

### Locality cache is mandatory input

Every selected edge must reach plane construction with one evaluated, locality-valid record from the immutable-source preflight. Construction may consume that record but may not recompute it. A missing record is an explicit evaluation failure and increments the cache-miss counter.

```text
locality evaluations
-> cached viability records
-> construction uses
-> zero solver recomputations
```

### Authoritative junction evidence

The old explicit-junction-face coverage heuristic assumed that any source vertex incident to multiple bevels required a separate `Junction` polygon. The certified edge-plane shell proves that assumption false: closed owner and bevel polygons can meet without an explicit junction face. That heuristic and its legacy text output are retired. Authoritative evidence is now limited to:

- extracted local-junction loops;
- final open/non-manifold/T-junction topology;
- exact failure dossiers;
- final polygon and triangle certification.

### Stable evaluation fingerprint

One deterministic evaluation fingerprint combines ordered viability exclusions, selected edges, certified edges, exact pre-placement polygon topology, and the immutable canonical placement frame. It is diagnostic only and does not affect geometry. Its purpose is to detect threshold, ordering, solver, topology, or placement regressions across repeated builds and later batch audits.


## Editor-only multi-seed viability matrix

The accepted edge viability and edge-plane-shell result must be calibrated across seeds and width extremes without turning the selected object into a test harness. R9 therefore runs the ordinary unified evaluation as a pure editor-side data operation.

### Matrix

```text
shape seeds:
1 / 1112 / 2223 / 3334 / 4445 /
5556 / 6667 / 7778 / 8889 / 9999

edge-wear widths:
minimum = 0.05
default = 1.0
maximum = 2.0

Coverage = 2.0 for every case
10 x 3 = 30 cases
```

The selected object's recipe is serialized once. Every case deserializes a fresh `MassRecipe`, sets only the case shape seed, constructs case-local surface settings, and invokes the same `UnifiedBoundedPreview` generator used by the manual rebuild. The resulting `MeshData` is inspected through the normal authoritative audit and then discarded. It is never applied to a Unity `Mesh`.

### Diagnostic capture

A matrix case opens one synchronous in-memory capture scope. The ordinary plane-shell audit is captured at `LogUnifiedAllEdgeBevelAudit`, and the canonical source placement frame is captured later at `AppendMassPlacementFrameTelemetry`. During that scope only, routine per-case Console and telemetry writes are suppressed. This avoids 30 repeated full logs while preserving the exact audit objects, cache counters, topology certification, and stable hashes.

The capture scope is non-reentrant and is cleared after every case, including exceptions. It does not cache geometry or viability evidence across different seeds or widths; each matrix coordinate is a distinct physical evaluation.

### Scheduling and cancellation

One matrix case executes per `EditorApplication.update`. A cancelable progress bar and Inspector cancellation action stop future cases while preserving completed rows. Domain reload and editor shutdown clear the progress UI and remove the update callback.

### Object-state isolation

R9 never invokes `GeneratedMass.Regenerate`, `EvaluateUnifiedEdgeWearPreview`, `MeshBuilder.ApplyToMesh`, collider binding, or material/atlas application. The job records the selected object's recipe JSON, local Transform, and shared mesh reference before the first case and verifies all three after completion or user cancellation.

### Reports

```text
Library/GeneratedMassEdgeWearBatchAudit.txt
Library/GeneratedMassEdgeWearBatchAudit.csv
```

The TXT report contains aggregate failure categories and compact per-case evidence. The CSV contains one completed-case row with eligibility/exclusion counts, shell certification, width reductions, topology, face quality, cache counters, preflight/total duration, stable component/evaluation fingerprints, and exact primary failure.

A case is successful only when every coexistence-eligible edge certifies, all geometry and render contracts are clean, the locality cache has zero misses/recomputations, the canonical source placement frame is used, and stable fingerprints are prepared. The batch does not weaken thresholds or hide failures through source-geometry fallback.

### Methods decision

- Accepted: run the exact authoritative builder against cloned recipe/settings data and discard the resulting `MeshData`.
- Accepted: one case per editor update with partial-report cancellation.
- Accepted: suppress repeated output only inside a short-lived diagnostic capture scope.
- Rejected: mutating and restoring the selected object's serialized seed/width for each case.
- Rejected: publishing 30 preview meshes or recooking colliders.
- Rejected: sharing viability/locality caches between different matrix coordinates.
- Rejected: changing geometry behavior as part of the audit harness.

## Coexistence viability closure

Individual viability answers whether one edge can support the requested bevel in isolation. It cannot prove that every individually valid bevel can coexist in one shell. The R9 matrix exposed three source-vertex-star openings, one near-endpoint T-junction, one strict plane-pair failure, and multiple technically closed shells whose conflict solver reduced width below the accepted `0.25` materialization floor.

R10 inserts one additional lifecycle stage:

```text
source topology
-> structural eligibility
-> individual geometric viability
-> bounded coexistence closure
-> coexistence eligibility
-> maximum-Coverage certification
```

### Width-floor invariant

`EdgeWearMinimumFeasibleWidthFraction` is shared by isolated preflight and global construction. Candidate scaling and every retreat/conflict trial must remain at or above `0.25` of requested width. Reaching the floor without a clean shell is a coexistence conflict, not permission to create a smaller bevel.

### Exact bounded exclusion trials

When the full individually viable set fails with a recognized source-vertex-star, plane-pair/T-junction, or hard width-floor conflict, structured evidence identifies a bounded implicated set. Failures outside that contract remain terminal and cannot be converted into exclusions:

- source vertex from a missing-junction open edge;
- linked bevels from a T-junction;
- owner/cut bevel provenance from an exact intersection failure;
- latest retry dossier or band-conflict victim/foreign pair.

Before exclusion, an intersection-cache entry that fails its current owner/cut plane certificate is invalidated and recomputed once through the existing analytical segment intersection and exact two-plane correction path. The corrected point must pass the same strict tolerance before replacing the cache entry. No tolerance is expanded.

The initial R10 closure used greedy single/pair trials. R10R2 supersedes that strategy with a bounded conflict-directed best-first frontier. Every state adds exactly one structured implicated edge to its explicit exclusion set, and every trial runs the existing exact whole-shell transaction and all final certificates; no approximate star-only success is accepted. Trial results are cached by explicit exclusions, retained source-edge IDs, and effective scales for that evaluation.

The deterministic preference order is:

1. fewest excluded edges;
2. least excluded requested width;
3. least excluded selection score;
4. stable source-edge order.

Failed states remain search evidence only and never mutate lifecycle state. The first completely certified state under the deterministic ordering is committed; total exclusions, evaluated states, and implicated candidates remain strictly capped. No edge ID participates in policy.

### Coexistence lifecycle

A generically excluded record becomes:

```text
ViabilityState = CoexistenceIneligible
CoexistenceEligible = false
Selected = false
Active = false
Deferred = false
Rejected = false
```

Its exact reason is one of:

```text
source-vertex-star-incompatible
plane-pair-incompatible
plane-band-incompatible
global-width-floor-conflict
candidate-conservation-incompatible
corner-width-missing
corner-width-inactive
coexistence-incompatible
```

Maximum Coverage is then defined as every coexistence-eligible edge certified. Coexistence exclusions are evidence-backed viability decisions, not hidden coverage losses.

### Telemetry and matrix contract

`[Coexistence Viability Closure]` records the geometric-to-coexistence denominator, reason counts and IDs, star/pair/trial evaluations and cache uses, exclusions, and minimum committed width scale. Stable evaluation fingerprints include the coexistence denominator and coexistence exclusion state.

The exhaustive 30-case topology matrix preserves the frozen R10R4 pass requirements: `certified == coexistenceEligible`, exact candidate conservation, and `minimumWidthScale >= 0.25`. Beginning with R11A.1, it uses the `EW-B4.2R11A.1-topology` contract and is paired with a separate ordinary-preview parity matrix. Aggregate failure categories derive from structured primary and retry evidence, including plane-band and candidate-conservation failures.

### Methods decision

- Accepted: authoritative whole-shell certification for every coexistence trial.
- Accepted: bounded deterministic exclusions when no legal width at or above the floor can coexist.
- Accepted: per-evaluation coexistence trial caching.
- Rejected: hardcoded problem-edge IDs.
- Rejected: tolerance loosening or sub-floor micro-bevels.
- Rejected: treating coexistence exclusions as deferred or rejected coverage.

## Conflict-directed coexistence closure and candidate conservation

R10R2 replaces the original greedy coexistence loop. The original loop could permanently commit a locally promising exclusion, stop when a subsequent band split used the generic blocker category, and accept a clean trial whose returned candidate set did not represent every selected edge not explicitly excluded.

The authoritative closure is now a bounded best-first search. Its root expected set comes from pre-closure selected lifecycle records. Every child adds exactly one edge from the current state's structured conflict set. The frontier is ordered by exclusion count, removed requested width, removed selection score, retained minimum width scale, and stable source-edge order. Exact exclusion/scale states are deduplicated. No intermediate state mutates lifecycle records.

A trial is eligible to win only when:

```text
actualCandidateIds == rootExpectedCandidateIds - explicitExclusionIds
attemptedCandidateCount == actualCandidateCount
geometry transaction fully certifies
certifiedCandidateCount == expectedCandidateCount
minimum materialized width scale >= 0.25
```

A mismatch is recorded as `candidate-conservation-failed`. Missing or unexpected IDs become structured conflict evidence and may only leave the Coverage denominator through an explicit `candidate-conservation-incompatible` exclusion in a later certified state.

Band integrity is now a first-class coexistence category. When authoritative audit fields contain both a victim and foreign edge, closure uses `plane-band-incompatible`; blocker prose is diagnostic only. Retry dossiers with T-junctions or source-vertex open-edge evidence are normalized to `plane-pair-incompatible` or `source-vertex-star-incompatible` respectively.

The closure remains bounded to twelve exclusions, 128 evaluated states, and ten implicated edges per failure. It does not perform a whole-mesh combinatorial sweep, loosen tolerances, or reuse caches across physical evaluations.

### Search evidence

`[Coexistence Conflict-Directed Search]` records:

```text
statesEvaluated
statesDeduplicated
maximumDepth
frontierRemaining
winningDepth
searchStateCandidateConservationFailures
```

Each processed state records exclusions, failure category, implicated edges, expected/actual/certified counts, exact expected/actual/missing/unexpected edge-ID sets, conservation validity, minimum width scale, full-validity state, and exact failure signature.

### Methods decision

- Accepted: conflict-directed best-first exclusion search with no intermediate lifecycle mutation.
- Accepted: explicit candidate conservation against the pre-closure selected set.
- Accepted: structured plane-band incompatibility from audit fields.
- Rejected: greedy progress commits.
- Rejected: silently losing candidates inside nested retries.
- Rejected: parsing edge IDs or solver decisions from human-readable blocker strings.


## Structured coexistence dossiers and committed-state finalization

R10R3 makes coexistence search provenance persistent across the complete bounded frontier. Every root or trial outcome receives a typed dossier containing its authoritative category, stage, source vertex, victim/foreign plane pair, linked edges, immutable incident-star membership, topology counts, and diagnostic text. Search behavior consumes only the typed fields.

When a child trial exits before producing new structured evidence, the state inherits its parent's actionable dossier and removes only already excluded edges from the active branch set. This prevents plane-band searches from ending at depth one and guarantees that a missing-junction branch can evaluate every still-active member of the source vertex's original viable star, including the terminal full-star exclusion state within the existing bounds.

A certified trial is not published until lifecycle and Coverage finalization also certify. The committed state must satisfy:

```text
coexistenceEligible == selected == attemptedBuilt == built
built == retainedCandidateCount
unresolvedWidthInactive == trialRejected == deferred == rejected == unmapped == 0
materializedCoverage == 1
```

Total `widthInactive` may remain nonzero only for records already classified as pre-shell `CoexistenceIneligible`; those records retain truthful zero-width evidence but are no longer part of the materialization denominator.

The failed root transaction's diagnostic, retry dossier, topology-stage provenance, and conflict pair are cleared only after this contract passes. R10R3 introduced report contract `EW-B4.2R10R3` and one complete bounded state ledger per closure case; R10R4 supersedes its width-inactivity finalization rule.

### Methods decision

- Accepted: typed failure dossiers on every trial exit.
- Accepted: inherited actionable provenance only when a child produces no newer structured evidence.
- Accepted: immutable incident-star membership from the original individually viable set.
- Accepted: explicit committed-state Coverage certification before solver success.
- Rejected: search termination caused only by loss of structured metadata.
- Rejected: publishing a clean polygon shell while retaining stale root-failure state.


## Corner-width eligibility reconciliation

R10R4 addresses the final `29/30` matrix failure from seed `2223` at minimum width. The failed root reported `34` geometrically eligible/selected records but constructed only `32` plane candidates. Excluding either edge in the structured T-junction pair `{18/35}` produced a clean 31-candidate shell, yet candidate conservation continued to expect two records that had already received no positive shared corner width.

The shared corner solution is now an explicit pre-shell coexistence boundary. A selected, individually viable edge that has no width entry becomes `corner-width-missing`; one whose solved width is at or below `PointMergeDistance` becomes `corner-width-inactive`. Both transitions preserve individual viability and zero-width evidence while atomically clearing candidate, selected, active, attempted, built, deferred, and rejected state. They are not topology failures and are not search-time exclusions.

Coverage now distinguishes:

```text
widthInactive                  all records with no positive shared width
unresolvedWidthInactive        inactive-width records still expected to materialize
preShellExclusions              corner-width eligibility exclusions
searchExclusions                exclusions selected by bounded coexistence search
```

Maximum-Coverage certification requires `unresolvedWidthInactive == 0`, not `widthInactive == 0`. This keeps telemetry truthful without allowing an inactive record to remain in the expected candidate set. `BuildPlaneCutExpectedCoexistenceEdgeSet` now requires selected, active, positive-width, coexistence-eligible records and only falls back to candidate IDs when no Coverage audit exists.

Committed exclusion reporting is the union of pre-shell corner-width exclusions and search-time exclusions, with the categories kept distinct. Search-state telemetry records exact expected, actual, missing, and unexpected candidate edge sets. Terminal matrix candidate-conservation failures are reported separately from candidate-conservation failures encountered only inside bounded search states.

### Validation status

R10R4 is runtime validated and frozen as the topology/coexistence baseline. The unchanged matrix passed `30/30` twice with identical fingerprints, exclusion sets, winning depths, and selected/certified counts. Seed `2223/minimum` produced `32/32` root candidate conservation, one generic exclusion from `{18/35}`, and a final `31/31` certified shell with two resolved `corner-width-inactive` records and zero unresolved inactive widths.

### Methods decision

- Accepted: classify zero shared-corner width before plane-candidate construction.
- Accepted: preserve total inactive-width evidence while gating only unresolved inactive widths.
- Accepted: report the union of pre-shell and search exclusions without conflating their causes.
- Rejected: endpoint snapping, tolerance expansion, welding changes, or search-budget expansion.
- Rejected: clearing `WidthInactive` merely to satisfy finalization.


## Visual selection and source-edge diagnostic authority

R11A separates exhaustive solver certification from the normal visual result. Maximum Coverage in the inspector means all artistically eligible candidates, not all geometrically viable candidates. The explicit editor-only `UnifiedBatchAudit` mode preserves exhaustive geometric coverage and the frozen R10R4 solver contract. The first R11A runtime pass nevertheless retained an exhaustive denominator in ordinary preview finalization; R11A.1 below is the authoritative correction.

The existing source-edge overlay is now an authoritative current-state diagnostic rather than a shape-only cache. Its validity key includes the complete production-generation state and current edge-wear amount, width, and Coverage. Any regeneration or relevant control change invalidates it. A current unified preview may seed the cache directly; otherwise the overlay performs a non-committing current eligibility/certification evaluation.

Each source edge receives one compact visual state:

```text
C  certified bevel
A  geometrically viable but artistically filtered
W  width-floor or shared-width exclusion
R  isolated-rail construction failure
X  coexistence exclusion
G  other geometric exclusion
B  structural exclusion
```

The overlay panel displays the shape seed and exact source-edge count. This directly prevents the previously observed 5727 graph from remaining visible after regeneration to seed 2223.

### Methods decision

- Accepted: exhaustive geometric candidate inclusion only through explicit batch-audit mode.
- Accepted: automatic source-overlay invalidation keyed by generation and edge-wear inputs.
- Accepted: update the existing overlay with compact state classification rather than adding views.
- Rejected: changing R10R4 topology, width-floor, intersection, welding, or coexistence rules in the visual-selection patch.
- Deferred: isolated-rail recovery and rendered bevel-normal correction.

## R11A.1 preview and exhaustive coverage contracts

R11A proved that artistic candidate filtering must be separate from exhaustive solver certification, but its first runtime pass exposed a stale denominator assumption. `MaximumCoverageMode` was still used as if it meant "all geometric candidates are required." That rejected otherwise valid ordinary previews whenever an `A` edge remained geometrically and coexistence eligible.

R11A.1 introduces the explicit invariant `RequireAllGeometricCandidates`:

- The topology viability matrix sets it to `true`; candidate inclusion is exhaustive and final certification requires `coexistenceEligible == selected == built`.
- Ordinary preview and the artistic preview parity matrix set it to `false`; final certification requires the selected visual set to materialize exactly, while intentionally artistically filtered records remain outside the denominator.
- `MaximumCoverageMode` remains a width/coverage tuning input and may still select cluster width reduction. It no longer defines the certification denominator.

The ordinary preview contract is:

```text
selected == active == attemptedBuilt == built == retained
unresolvedWidthInactive == 0
trialRejected == deferred == rejected == unmapped == 0
```

The exhaustive topology contract adds:

```text
coexistenceEligible == selected
```

Two separate editor audits are authoritative:

```text
Topology Viability Matrix
  contract: EW-B4.2R11A.1-topology
  policy: all-geometric
  reports: Library/GeneratedMassEdgeWearBatchAudit.txt|csv

Artistic Preview Parity Matrix
  contract: EW-B4.2R11A.1-preview
  policy: artistic-preview
  reports: Library/GeneratedMassEdgeWearPreviewParityAudit.txt|csv
```

The second audit is required because a passing exhaustive matrix alone cannot prove that the ordinary preview path builds. The plane construction, viability thresholds, coexistence search, topology certification, and frozen R10R4 geometry rules remain unchanged.
