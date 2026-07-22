# Generated Mass Edge-Wear Recovery Architecture

This document defines the current architecture and invariants only. It is not a patch history or validation log.

The sole canonical progress ledger is:

```text
Docs/Generated_Mass_Feature_Implementation_Checklist.md
```

## Current status

`EW-B4.2R13A.9a` remains the frozen uniform basic-bevel/recovery architecture and exact zero-irregularity fallback. `EW-V1A.2b` is Unity-validated and accepted as the frozen deterministic per-edge macro-width extension. Its one-click suite passed current preview; macro zero parity, determinism, distribution, and retention; topology `33/33`; artistic preview `33/33`; recovery closure `5/5`; unresolved `0`; and negative exclusion `1/1`, with comprehensive evidence available and no cancellation or terminal failure.

The architecture now establishes:

- deterministic source-edge selection and canonical identity;
- exact local width/rail feasibility;
- immutable certified-baseline fallback;
- conflict-safe coexistence and bounded recovery;
- micro-topology normalization for invisible blockers;
- certified topology, convexity, containment, bounds, volume, face quality, triangulation, normals, tangents, and render channels;
- stable seed-8889 positive/negative fixtures, including edges `13/23/39`, suppressed component `14/24/30`, and excluded edge `40`.

The accepted R13A.9a uniform baseline and V1A.2b macro extension remain closed. V1A.2 completed owner/support construction-width backoff and retained the reported meaningful edge; V1A.2a closed stable-floor classification before corner solving; V1A.2b closed the corresponding editor-only fixture proof. The later EW-V2A multi-plane geometric Micro path is retired after runtime evidence showed high dirty-time cost and a repeated wide-end/narrow-middle result. EW-V1A.2c/d/e protected-Macro recovery experiments are likewise rejected and removed after failing functionally and producing an unresponsive 256-state editor case. EW-V1A.2f restores the complete V1A.2b recovery system and normalizes public Macro Strength to the certified effective amplitude. EW-S1 is rejected and removed. EW-V1A.3b is the accepted and frozen Macro-width/recovery boundary. `EW-C1A-RO2` completed the ownership/construction audit and approved one transactional corner cut after micro-topology normalization but before bevel-candidate construction. `EW-C1A.1` is implemented as a diagnostic-only transaction and its seed-8889 trial-0 report is accepted. EW-C1A.1a.8 passed the complete Unity gate and is frozen. EW-C1A.2 is implemented as a preview-only committed corner transaction with mandatory cap-ring bevels; Unity compile and visual acceptance remain pending.

The legacy construction and repair paths and the earlier bounded/plane experiments remain documented as architecture history and diagnostic comparison evidence. Any pre-R13A section that speaks of a future geometry direction is historical unless repeated by the current contract near the end of this document.

Ordinary production generation still performs no edge-wear evaluation or logging. Explicit editor preview/audit modes own the accepted bevel clone, and Play Mode uses production geometry.

```text
geometryCommit=disabled
```

remains mandatory. R13A.9a accepts the editor geometry foundation; it does not promote that clone to production.

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
    -> deterministically back off width until rails and the complete bounded shell certify
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


## R11B.1 coincident boundary-seam recovery

R11B.1 repairs source seams split only by quantization-boundary drift smaller than `PointMergeDistance`. Reconciliation is deliberately narrow: the second incidence must reverse-match an existing one-sided incidence, belong to a different face, and leave the canonical edge manifold after the second face is attached. No source face, scene, prefab, or production mesh is rewritten.

The candidate aggregate and topology graph independently apply the same rule and expose separate evidence counts. Acceptance requires candidate seam-pair count and graph seam-pair count to agree, the affected canonical edge to receive two owner faces, and both topology and preview matrices to remain fully certified. This patch does not relax isolated-rail, width-floor, dihedral, or certification constraints.

## EW-B4.2R11B.1C authoritative rollback and collateral contract

R11B.2 and R11B.3 are not part of the accepted architecture. The singleton-shell fallback produced zero recoveries. Bevel-graph micro-feature normalization certified smaller candidate universes by invalidating unrelated owner-face support, so its clean topology output was not valid recovery evidence. The active implementation is R11B.1 seam reconciliation plus the collateral guard below.

The immutable collateral baseline is captured after canonical source-edge/seam construction and the normal individual viability preflight, but before any prospective recovery transformation or artistic filtering. Each baseline record preserves its canonical key, source edge ID, owner-face pair, convexity classification, length, dihedral, maximum locally feasible width, width fraction, and geometric-eligibility state.

A recovery transaction is acceptable only when:

- every baseline geometrically viable edge remains geometrically viable;
- no baseline viable edge changes identity, owners, classification, or viability geometry;
- newly viable edges are recorded as recoveries rather than replacing baseline evidence;
- final coexistence, Coverage, topology, face quality, placement, and stable fingerprint contracts still pass.

The audit publishes baseline/current/recovered/lost/changed counts and exact IDs. `collateralLostEdges` and `collateralChangedEdges` are hard failures in both exhaustive topology and artistic preview parity matrices. This closes the audit hole that allowed R11B.3 to pass `30/30` after shrinking seed `2223/default` to `19` candidates.

Current report contracts:

```text
EW-B4.2R11B.1C-topology
EW-B4.2R11B.1C-preview
```

The next recovery architecture must be a local virtual support-chain rail: it may walk an unbranched source-segment chain for one endpoint, but it may not remove graph edges, move shared source vertices, rewrite face loops, or renumber unrelated canonical edges.

R11B.1C runtime validation restored seed `5727/default` to `rawSource/source=44/42`, two reconciled seam pairs, `geometric=36`, and `selected/certified=34/34`, with `collateral=36/36/0/0/0/1`. Both topology and artistic-preview matrices passed `30/30` with zero collateral, topology, face-quality, placement, or cache failures.

## EW-B4.2R11B.1D one-click validation suite

The Inspector now exposes one authoritative **Run Full Edge-Wear Validation Suite (1 Click)** action. It performs the ordinary current-seed preview rebuild first, captures `GeneratedMassEdgeWearTelemetry.txt`, then runs the exhaustive topology matrix and the artistic-preview parity matrix sequentially without requiring additional user interaction. The selected mass remains on the rebuilt current preview; all matrix coordinates remain immutable in-memory evaluations.

The canonical matrix seed set now includes `5727` because it exercises the accepted coincident-boundary seam reconciliation path that the earlier ten-seed set did not cover. Both policies therefore run eleven seeds across minimum/default/maximum width, for `33` cases each. Historical R10R4 acceptance remains the original deterministic `30/30` baseline; R11B.1D adds three regression coordinates without changing those original coordinates.

The suite writes one attachable report:

```text
Library/GeneratedMassEdgeWearValidationSuite.txt
```

That report embeds the current preview summary and full telemetry followed by both complete matrix reports. Focused matrix TXT/CSV files continue to be written separately. Inspector actions can copy the full combined report to the clipboard or reveal it in the file browser. Contracts are `EW-B4.2R11B.1D-suite`, `EW-B4.2R11B.1D-topology`, and `EW-B4.2R11B.1D-preview`.

## EW-B4.2R11B.1E authoritative geometry baseline lock

The R11B.4 owner-face support branch is retired. R11B.4 attempted bounded candidate-local boundary traversal, R11B.4.1 corrected line-intersection distance units, and R11B.4.2 added chain-first gathering plus shared-junction classification. The final runtime suite proved the branch had no usable yield: both policies retained all `33/33` ordinary construction cases and zero collateral failures, while the recovery stage itself completed `27` evaluations and `126` width attempts per policy with zero boundary segments examined, zero virtual corners, and zero recovered edges.

The authoritative geometry implementation is now R11B.1 seam reconciliation plus the R11B.1C collateral guard, with the R11B.1D one-click eleven-seed validation suite. R11B.1E removes every R11B.4 runtime path and telemetry field and changes only the suite/report contract labels. The geometry, candidate lifecycle, matrix seeds, thresholds, source graph, owner-face provenance, width floor, topology certification, and placement fingerprints are restored to R11B.1D.

No additional isolated-rail or micro-junction recovery work is authorized without new evidence of a broad production failure that is not already represented as a legitimate geometric exclusion. The next edge-wear work is artistic selection and presentation quality, not expansion of the topology solver. Contracts are `EW-B4.2R11B.1E-suite`, `EW-B4.2R11B.1E-topology`, and `EW-B4.2R11B.1E-preview`.


## EW-B4.2R12A artistic-selection evidence architecture

R11B.1E remains the immutable geometry baseline. R12A adds no selection or geometry policy. The current artistic score remains:

```text
(angleScore * 0.58 + lengthScore * 0.27 + deterministicRandom * 0.15)
* baseSuppression
* upwardEdgeBoost
* edgeCharacterBoost
```

The audit separately records diagnostic context that does not currently influence that formula: edge-axis verticality, a camera-independent owner-normal silhouette potential, maximum preflight width fraction, final solved-width fraction, local viable-edge density measured within the existing `maximumDimension * 0.34` length-score normalization scale, and shared-vertex crowding. The report marks each of these context weights as zero so later R12B decisions cannot accidentally treat descriptive evidence as an already-active selector.

The selection audit is captured after the unchanged descending score sort and Coverage count resolution. Per-edge evidence includes exact score components, eligibility gates, artistic filter reason, selection rank, selection threshold, and score delta. Aggregate evidence includes filter reasons; score minimum/median/maximum for all, selected, and filtered populations; and four-bin distributions for length, dihedral, edge orientation, silhouette potential, local density, and vertex crowding. Both 33-case policies retain their R11B.1E pass/fail rules. Contracts are `EW-B4.2R12A-suite`, `EW-B4.2R12A-topology`, and `EW-B4.2R12A-preview`.


## EW-B4.2R12A.1 — one-pass comprehensive artistic evidence contract

R12A.1 is diagnostic-only and sits strictly above the locked R11B.1E geometry boundary. It may read immutable artistic-preview case records and simulate alternative rankings in editor memory, but it may not alter source topology, geometric or coexistence eligibility, hard runtime gates, the production score, Coverage, widths, corner solving, plane-shell construction, placement, or certification.

The canonical one-click suite owns the evidence lifecycle. It reuses the already-generated eleven-seed by three-width artistic-preview matrix, exports every source edge and all known selection/viability/effect fields, evaluates the complete declared scenario universe, analyzes every possible selected slot plus 10%-100% Coverage deciles, writes the three comprehensive Library reports, and embeds the decisive evidence in the single combined validation report.

This patch is intended to end telemetry-by-installment. Any later artistic-selection proposal must be justified from the R12A.1 raw edge table, scenario table, correlations, cutoff sensitivity, Pareto evidence, and width stability. New instrumentation is justified only when a genuinely new runtime variable does not exist in the comprehensive export; it must not be used as a substitute for analyzing the captured evidence.

R12A.1 pass/fail remains subordinate to the existing geometry contracts: both 33-case matrices must pass, collateral lost/changed must remain zero, the current score must reproduce from exported components, all comprehensive outputs must be available, and the selected mass state must remain preserved.


## EW-B4.2R12A.1b — recorded-rank analyzer correction

The comprehensive analyzer's exact-current baseline is the production artistic ranking captured before coexistence removes later-incompatible candidates. R12A.1b validates the complete `GeometricEligible && ArtisticEligible` rank universe against `ArtisticEligibleCount`, validates final surviving candidates separately against `CandidateCount`, and permits post-coexistence gaps in `CandidateIndex`. Recorded ranks remain unique, contiguous, finite, and score-nonincreasing across the original artistic population. Alternative hypothetical policies retain deterministic source-edge tie-breaking.

Runtime validation passed: the full suite and both `33/33` matrices passed, comprehensive evidence was produced, seed `5727` retained `34/34` active/certified bevels, collateral loss/change remained zero, and current-score reconstruction error was `1.49011612E-08`.


## EW-B4.2R12B.1 — geometric-priority artistic selection

R12B.1 is the first production artistic-policy change above the locked R11B.1E geometry boundary. The R12A.1b comprehensive evidence showed that the previous multiplicative placement modifiers dominated ranking: base suppression and upward boost correlated far more strongly with rank than the nominal angle and length terms. The production policy now restores geometric quality as the primary selector while retaining bounded positional character.

The artistic angle gate changes from `angleScore > 0.035` to `angleScore > 0.055`, raising the approximate artistic dihedral floor from `17.94` degrees to `22.54` degrees. The geometric viability floor remains unchanged at `15` degrees.

The authoritative score is:

```text
core = angleScore * 0.60
     + lengthScore * 0.35
     + deterministicRandom * 0.05

basePriorityFactor = lerp(0.60, 1.00,
    inverseLerp(0.06, 0.20, baseSuppression))

upwardPriorityFactor = lerp(0.925, 1.075,
    inverseLerp(0.82, 1.08, upwardEdgeBoost))

score = core * basePriorityFactor * upwardPriorityFactor
```

`edgeCharacterBoost` remains recorded for evidence but no longer multiplies rank. It is object-wide and therefore cannot change intra-object ordering. Existing length and base gates, Coverage count resolution, descending candidate ordering, coexistence closure, width solving, corner solving, shell construction, and certification remain unchanged.

The editor scenario analyzer applies the same compressed base/upward factors and treats `current-exact`, current ablations, modifier masks, gate masks, and current-plus context sweeps as R12B.1 policies. Scenario count and CSV schemas remain unchanged. Contracts advance to `EW-B4.2R12B.1-suite`, `EW-B4.2R12B.1-topology`, `EW-B4.2R12B.1-preview`, and `EW-B4.2R12B.1-comprehensive`.


## GM-R12B.1C — accepted artistic baseline and render-integrity investigation boundary

The accepted EW-B4.2R12B.1 runtime suite passed current preview, both `33/33` matrices, comprehensive analysis, recorded-rank integrity, score reproduction, and collateral preservation. R12B.1 is therefore the authoritative artistic-selection baseline. No further ranking adjustment is justified by the current outliers.

The remaining visibly important missing bevels are classified as geometry-stage outliers: seed `2223` edge `36` and seed `8889` edges `13/23` fail isolated-rail viability, while seed `2223` edge `13` fails width/corner feasibility. This classification is current evidence, not a guarantee that the eventual render-mesh repair cannot alter their structural conditions. Any production fix involving shared mesh channels, vertex duplication, triangulation, UV semantics, or tangent ownership must re-evaluate those four edges before separate recovery work resumes.

The immediate implementation is editor-only and read-only. `GeneratedMassEditor` audits the live `MeshFilter.sharedMesh`, writes `Library/GeneratedMassRenderMeshAudit.txt`, draws the worst triangle, and can create temporary `HideAndDontSave` proof clones for tangent replacement or Unlit isolation. These tools never regenerate or repair production geometry, never serialize a clone, and never change `MeshData`, `MeshBuilder`, Generated Mass UV construction, shaders, materials, scenes, or prefabs. A production repair is prohibited until the audit and proof clone identify the exact failing channel.


## GM-R12B.1D — render-normal integrity ownership

The black-triangle/Bloom artifact is proven to originate from invalid Generated Mass stored normals, not from non-finite tangents. `Rock_14`, `Rock_18`, and the seed `8889` bevel preview each emitted `27` zero normals over `9` flat-shaded triangles while tangent magnitudes remained finite and equal to one. Ordinary offending meshes had no UV-degenerate or UV-ill-conditioned triangles, so UV-based tangent reconstruction is not the common root cause.

Generated Mass previously accepted a triangle cross product when its squared magnitude exceeded `MinimumEdgeLengthSqr = 1E-12`, then delegated normalization to `Vector3.normalized`. Cross magnitudes in the interval accepted by the generator but below Unity's internal normalization epsilon could therefore become `(0,0,0)`. GM-R12B.1D established Generated Mass ownership through explicit normalization, pre-application channel validation, and a final post-tangent Unity-mesh channel guard. Its first helper still reused the edge-length-squared threshold for cross-product-squared magnitude; GM-R12B.1E removes that dimensional mismatch while preserving the ownership boundary. Shared `MeshData` and `MeshBuilder` behavior remain unchanged.

The production generation contract advances to version `2`, forcing old accepted transient meshes to rebuild once. This is a render-channel semantic correction only; positions, indices, UV projection, topology, edge-wear ranking, widths, and shell construction are unchanged. The four unresolved bevel outliers must nevertheless be re-evaluated after rebuild before their geometry classes are treated as independent conclusively.

The editor audit contract is `GM-R12B.1D-render-audit-v2`. Zero normals are hard failures and outrank UV-conditioning warnings. The temporary proof path repairs invalid normals from exact triangle winding, then rebuilds only affected or otherwise unsafe tangents; it remains `HideAndDontSave` and never replaces serialized or production geometry.


## GM-R12B.1E — scale-correct normal normalization

GM-R12B.1D successfully removed the black-triangle/Bloom artifact from `Rock_14` and `Rock_18`: regenerated meshes reported zero missing, non-finite, zero, or non-unit normals/tangents, and the visible artifacts disappeared. Its first implementation nevertheless rejected seed `8889` face `76`, whose double area was `8.559025E-07` but whose relative area was a healthy `0.296998173`.

The rejection was dimensional, not geometric. `MinimumEdgeLengthSqr` is a length-squared threshold, while a cross-product squared magnitude is length-to-the-fourth. Comparing those quantities imposed an unintended absolute triangle-size floor and contradicted the existing scale-relative triangle acceptance contract.

GM-R12B.1E therefore normalizes any finite mathematically non-zero vector with a double-precision magnitude calculation. Existing scale-relative geometry validation remains authoritative for triangle acceptance. Truly zero or non-finite cross products still fail deterministically; tiny valid triangles do not. Production, editor audit, and temporary proof paths use the same normalization semantics.

Finite UV determinants below the diagnostic conditioning threshold remain reported because they can be useful rendering evidence, but they do not constitute a hard render-channel failure when positions, normals, tangents, UVs, colors, indices, winding, and 3D geometry are valid. The v3 audit reports `passed-with-warnings` rather than the ambiguous historical `flagged` status.

No topology, triangulation, UV projection, shared `MeshData`, shared `MeshBuilder`, shader, material, scene, prefab, edge-wear selection, or bevel-construction policy changes in this correction. Production generation contract version `2` remains current.

## EW-B4.2R13A.1 outlier-recovery architecture

R13A.1 does not reopen artistic ranking. EW-B4.2R12B.1 remains the accepted geometric-priority selection policy. The remaining targets are construction outliers that already pass structural and artistic evaluation.

### Exact-boundary endpoint canonicalization

An isolated rail endpoint is still owned by the exact endpoint-adjacent source edge and its two exact source-face planes. R13A.1 changes only the numerical authorization at the segment boundary:

1. Calculate the raw projection parameter in double precision.
2. Resolve the existing absolute point tolerance.
3. Convert that spatial tolerance to a segment parameter tolerance using `pointTolerance / boundaryLength`.
4. Accept only a raw parameter in `[-parameterTolerance, 1 + parameterTolerance]` whose clamped point remains within the absolute point tolerance.
5. Canonicalize the accepted point onto `[0,1]`.
6. Run every existing downstream ownership, plane, displacement, distinct-edge, non-collapse, topology, containment, bounds, volume, and face-quality certification unchanged.

Endpoint proximity is not independently invalid. The previous clearance rule rejected valid near-corner solutions before the stronger certification layers could evaluate them. R13A.1 does not permit edge walking, cross-face substitution, invented supports, or an expanded geometric tolerance.

### Width-monotonic viability

The old preflight compared local certified width with a fixed fraction of the current requested width. That allowed an edge certified at the minimum style tier to disappear when the global requested width increased. R13A.1 defines eligibility against the canonical minimum `Edge Wear Width` style tier instead:

```text
minimumStyleWidth = ResolveGeneratedEdgeWearWidth(maximumDimension, 0.05)
minimumRequiredCertifiedWidth = minimumStyleWidth * 0.25
```

The materialized width remains locally capped. Increasing the global request may thicken capable edges while constrained edges stop at their local maximum; it may not remove a constrained edge solely because its relative fraction became smaller.

### Terminal shared-edge retention search

The existing shared-interval solver remains primary. A bounded subset search runs only when its uniform scale would reduce at least one participating selected edge below the established minimum stable width.

- Maximum participants: six.
- Maximum non-empty states: 63.
- Every state defers non-retained local participants, solves a stable common scale for the retained subset, and rejects sub-floor retained widths.
- Objective order: retained count, summed production artistic score, retained certified width, deterministic source-edge order.
- No result bypasses the existing complete final corner, replacement-face, rail, plane-shell, topology, containment, bounds, volume, or face-quality validation.
- If no better subset certifies, the previous safe uniform-scale/all-defer behavior is preserved.

This search is dirty-time generation work only and has no per-frame or rendering cost.

### Active validation contracts

```text
EW-B4.2R13A.1-suite
EW-B4.2R13A.1-topology
EW-B4.2R13A.1-preview
EW-B4.2R13A.1-comprehensive
```

The explicit target cases are `2223/36`, `2223/13`, `8889/13`, and `8889/23`. These are acceptance cases, not seed-specific branches in production code. The editor-only one-click suite evaluates five topology-case fixtures: `2223/36` at maximum width, `2223/13` at default and maximum width, and `8889/13` plus `8889/23` at maximum width. Suite status cannot pass unless all five are active, materialized, and certified.

## EW-B4.2R13A.1 runtime rejection

R13A.1 is not an accepted geometry baseline. Runtime validation produced topology `31/33`, artistic preview `31/33`, and outlier recovery `0/5`.

The recorded endpoint parameters disproved the numerical-drift premise. The target rails were materially outside the presumed adjacent segment: `2223/36` was approximately `-0.874`, `8889/13` was `3.892`, and `8889/23` was `-0.596`. Their distances from the presumed segment were millimetres, while the authorized point tolerance was `0.00008`. Clamping them would have been an arbitrary geometry rewrite.

The global minimum-style-width rule made `2223/13` provisionally geometric but the local shared-edge solver still removed it as `corner-width-inactive`. The local 63-state subset solver recovered no target. Keeping additional constrained edges alive globally also introduced two maximum-width regressions: seed `1112` reached a terminal foreign-plane band split, and seed `5556` produced a branch rejected by the final render-normal/winding guard.

Therefore the following R13A.1 production mechanisms are superseded and must not be treated as accepted policy:

- endpoint overshoot clamping to the presumed adjacent segment;
- unconditional minimum-style-width eligibility;
- local shared-edge subset retention before full-shell evidence exists.

The five editor-only named-edge fixtures remain authoritative because they correctly prevented a false suite pass.

## EW-B4.2R13A.2 — owner-boundary terminal resolution and conflict-directed retention

R13A.2 restores the ordinary requested-width fraction gate as the default geometric eligibility rule. A locally certified edge that fails only that fraction may remain **provisional** when it is still certified at the canonical minimum style tier. Provisional status is not final authorization; it exists only to permit bounded full-shell conflict search. Cases without a selected provisional edge use the unchanged R12B.1E corner and plane path and pay no search cost.

### Complete owner-face boundary terminal

An isolated rail is a line on its exact owner source-face plane. When the intersection with the presumed endpoint-adjacent boundary lies outside that segment, the correct geometric question is not whether the point can be clamped. The rail must be clipped against the complete polygon boundary of the owner face.

For each rail R13A.2:

1. builds the existing selected offset support line on the exact owner source face;
2. establishes a forward ray from the selected-edge endpoint using the existing corner solve only to choose direction;
3. intersects that ray with every finite, manifold boundary segment of the exact owner face except the selected source edge;
4. rejects backward hits, non-finite intersections, off-segment hits outside the existing world-space point tolerance, and non-manifold target boundaries;
5. deduplicates coincident vertex hits and requires a unique nearest forward terminal;
6. preserves the exact resolved graph edge, target graph/source face, ray distance, segment parameter, and original-adjacent miss evidence;
7. runs the resolved point through the unchanged owner/target plane, displacement, distinct-boundary, collapse, topology, containment, bounds, volume, replacement-face, and render-channel certifications.

This is exact polygon clipping. It does not walk an arbitrary support chain, invent a virtual corner, move source vertices, substitute another owner face, or bypass downstream certification.

### Provisional width and full-shell conflict search

`WidthRecoveryProvisional` is set only when isolated construction succeeds, the ordinary requested-width fraction is below `0.25`, and the certified absolute width still meets `canonicalMinimumStyleWidth * 0.25`. A provisional edge enters the existing candidate population, but its survival is decided by a bounded full-shell search rather than by a global monotonicity promise.

The search is invoked only when at least one selected edge is provisional. It evaluates forced-defer sets against cloned lifecycle evidence. Every state runs:

- the complete corner-width and replacement-face solution;
- the complete plane-cut bevel kernel and its coexistence closure;
- band integrity, topology, containment, bounds, volume, face quality, placement, and final render-channel validation.

Corner-width collapse publishes the exact participating selected edges as branch candidates. A terminal plane-band split publishes its victim and foreign edges as branch candidates. If final render-channel validation rejects a provisional configuration before a more specific pair exists, the selected provisional edges are the bounded fallback branch set. Search is capped at `128` states and `10` forced deferrals.

A valid state is chosen deterministically by:

1. greatest certified edge count;
2. greatest summed accepted R12B.1 artistic score;
3. greatest total certified materialized width;
4. lexicographically smallest forced-defer edge set.

The winning state is rerun once against the authoritative coverage audit. Trial clones are editor/generation-time state only and are never serialized.

### Terminal band and render-channel branch semantics

Plane-band evidence must survive terminal minimum-width failure. `CopyPlaneCutBandAudit` therefore preserves the first victim/foreign conflict pair when the destination has none, allowing the full-shell search to branch instead of collapsing the case into an unclassified general failure.

The GM-R12B.1E render-normal guard remains unchanged. A trial that emits a final winding/normal disagreement is an invalid search branch, not a reason to weaken, flip, or bypass the normal contract.

### Contracts and acceptance

Contracts are:

```text
EW-B4.2R13A.2-suite
EW-B4.2R13A.2-topology
EW-B4.2R13A.2-preview
EW-B4.2R13A.2-comprehensive
```

Acceptance requires topology `33/33`, artistic preview `33/33`, comprehensive availability, zero collateral/topology/face-quality/placement/render-channel regression, and all five named outlier checks active and certified. No seed or source-edge ID appears in production recovery behavior.

## EW-B4.2R13A.2 runtime rejection — nested-search explosion

R13A.2 is not an accepted geometry baseline. The one-click suite completed the first 24 topology cases and then remained inside `seed 7778 / maximum width` for more than ten minutes until cancelled. Preview, outlier, and comprehensive stages did not run.

The failure was architectural rather than an unbounded language loop. A provisional full-shell frontier of up to 128 states called the plane kernel, and each state could start the existing coexistence frontier of up to 128 states. This allowed approximately 16,384 complete shell evaluations for one matrix case. R13A.2's nested search ownership is superseded.

## EW-B4.2R13A.3 — one active conflict frontier per evaluation

R13A.3 preserves complete owner-face boundary termination and provisional-width eligibility, but forbids nested full-shell search.

- Ordinary non-provisional cases continue through the existing plane-kernel coexistence search.
- A case containing a selected provisional candidate uses one orchestration-level frontier that recomputes corners and evaluates the complete shell for each forced-deferral state.
- The kernel coexistence frontier is disabled during those provisional state evaluations. Its exact corner, band, topology, face-quality, placement, and render-channel evidence is returned to the active frontier.
- The active frontier orders states by fewest exclusions, lowest removed production artistic score, lowest removed certified width, and deterministic edge order. It commits the first fully certified state.
- Both search routes are capped at 128 states and five seconds during explicit audit execution. Cancellation is polled between states through a transient editor callback.
- Search exhaustion, time-budget exhaustion, and cancellation are distinct terminal reasons. A partial or cancelled state is never committed.

The five named fixtures remain acceptance gates, but R13A.3 is first required to restore responsive matrix execution and the `33/33` topology and preview floor. No search limit increase is an acceptable substitute for eliminating nesting.

Contracts:

```text
EW-B4.2R13A.3-suite
EW-B4.2R13A.3-topology
EW-B4.2R13A.3-preview
EW-B4.2R13A.3-comprehensive
```


## EW-B4.2R13A.4 — certified baseline plus optional recovery augmentation

R13A.3 established a responsive five-second search boundary, but runtime validation rejected its replacement-solve behavior: seed `1/maximum` and `7778/maximum` returned empty records, the live seed-8889 preview was erased when no augmented shell was found, and all five recovery fixtures remained absent. R13A.4 makes the established certified shell immutable fallback state.

### Baseline-first invariant

1. Evaluate the ordinary selected set with selected provisional recovery edges forced off.
2. Require a fully certified shell and retain its corner solution, plane audit, preview soup, coverage lifecycle, and certification metrics.
3. Discover optional recovery participants from width-provisional evidence and baseline corner-collapse evidence.
4. Search only augmentation states derived from the certified baseline.
5. Commit an augmented state only when it is fully certified, recovers at least one absent target class, and is superior by certified count, accepted artistic score, then certified width.
6. On timeout, state exhaustion, cancellation, or no superior state, return the certified baseline unchanged.

Recovery may never erase a valid production preview, emit an empty matrix record, or masquerade as collateral loss. Explicit evidence distinguishes `augmentation-time-budget-exceeded`, `augmentation-state-budget-exceeded`, `augmentation-cancelled`, and `augmentation-found-no-superior-certified-shell`, together with baseline-applied state and the last failure dossier.

### Single-frontier invariant

The baseline may use the established kernel coexistence owner. Augmentation trials disable kernel conflict recursion and use one bounded orchestration frontier. The frontier begins from the baseline exclusion set with recovery participants re-enabled; it does not rediscover an unrelated complete shell. The existing 128-state, eight-additional-deferral, five-second, and cancellation limits remain editor-validation safeguards.

### Exact multi-support endpoint interval

Complete owner-face boundary intersection proved that three named outliers terminate through different support faces at one or both selected-edge endpoints. A shared-support-face requirement is therefore too narrow.

For such an endpoint, the four exact rails already define one finite bevel plane. R13A.4 applies that exact half-space to the complete convex source shell, requires the two selected source-edge endpoints to be removed, every foreign source vertex and the solid centre to remain, and preserves all source-face provenance. The result must contain one bounded bevel cap whose boundary contains all four solved rail terminals. Every modified non-owner source face must belong to the exact plane-intersected support interval.

This path is not a clamp, invented support chain, seed exception, or relaxed topology rule. It is an exact convex plane cut constrained by rail provenance and accepted only after the same strict intersection, manifold, containment, convexity, bounds, volume, face-quality, sidedness, triangulation, and render-channel certification as ordinary bounded bevels.

### R13A.4 promotion gate

```text
EW-B4.2R13A.4-suite
EW-B4.2R13A.4-topology
EW-B4.2R13A.4-preview
EW-B4.2R13A.4-comprehensive
```

The safety floor is live preview retained, topology `33/33`, artistic preview `33/33`, comprehensive evidence available, and zero collateral/topology/face-quality/placement/render regression. Only after that floor is met may `5/5` outlier recovery promote the geometry work.


## EW-B4.2R13A.6 — exact retained-point hull and finalized corner recovery

R13A.4 remains the immutable stable incomplete baseline. R13A.5 is not a valid successor: its two sampled transition planes recovered no target and could replace a certified baseline edge with an unrelated provisional edge.

### Exact retained-point convex-hull contract

For a multi-support edge, form the finite point set from all original source vertices except the two selected-edge endpoints, then add the four exact solved rail terminals. Enumerate the complete bounded set of non-collinear point triples. A triple contributes a facet only when every retained point lies on one side of its plane. Coplanar supporting planes are merged and their facet boundaries are ordered from the complete supporting-point set.

This construction is accepted only when:

- both selected endpoints are absent from the result;
- every unrelated original source vertex is retained by construction;
- all four exact rails occur on the connected new bevel-band boundary;
- every original source face retains exactly one provenance record;
- both owner faces and only faces in the two endpoint vertex stars are modified;
- all existing topology, intersection, containment, convexity, bounds, volume, face-quality, triangulation, and render-channel checks pass.

No sampled selected-edge split, local stitched patch, endpoint clamp, or relaxed foreign-vertex test is authorized.

### Finalized corner-inactive contract

Corner evidence is captured at the exact finalized width deactivation, not in an earlier speculative pass. A provisional record stores the target edge, last positive width, collapsed internal edge, uniform scale, and complete conflict participant set. Augmentation starts from the certified R13A.4 baseline with the target protected and may defer only its recorded neighbours.

A retained-hull edge is always excluded from the immutable baseline solve, including when its hull succeeds at the full requested width; it can enter only as an explicit augmentation target. An augmentation may commit only when it recovers at least one retained-hull or finalized-corner target, does not reduce certified edge count, and removes no certified baseline edge outside the exact participant set of a recovered corner target. Failure, timeout, cancellation, or exhaustion retains the baseline byte-for-byte in geometry ownership and edge identity.

### Regression gate

```text
EW-B4.2R13A.6-suite
EW-B4.2R13A.6-topology
EW-B4.2R13A.6-preview
EW-B4.2R13A.6-comprehensive
```

The safety floor remains current preview applied, topology `33/33`, artistic preview `33/33`, comprehensive evidence available, no unrelated baseline-edge substitution, and zero collateral/topology/face-quality/placement/render-channel regression. The intended recovery result remains `5/5`; an exhaustive retained-hull rejection is acceptable only with the R13A.4 baseline unchanged and complete finite evidence preserved.


## EW-B4.2R13A.7 — evidence-based recovery closure

R13A.6 is the current safe runtime state: the live preview remains applied, both 33-case matrices pass, comprehensive artistic evidence is available, seed `8889` edge `39` remains active, and tiny edge `40` remains inactive. The remaining failure is fixture semantics: the five historical coordinates are still reported only as missing recoveries even when the bounded solver has reached an exact terminal geometric blocker.

### Current discrete-width proof boundary

`TrySolveBoundedIsolatedSingleEdgeRails` owns a finite schedule of at most twelve widths, beginning at the locally admissible width and applying a `0.75` backoff down to `max(minimumStableEdgeLength, PointMergeDistance * 4)`. It stops at the first rail success. The bounded constructor then executes once at that solved width.

R13A.7 records every rail attempt already executed and the terminal single-plane, retained-hull, construction, and certification evidence. It adds no rail, hull, plane, or full-shell evaluation. A failed construction is classified as `complete-infeasible` only when the terminal construction ran at the minimum admissible width or at the twelfth and final scheduled rail attempt. A failure above that boundary is `unresolved`; it is not represented as a continuous or interval proof.

### Complete corner zeroing provenance

The R13A.6 capture read only `ChamferCornerSolution.Conflicts`, but `TrySolveCornerAwareChamferWidths` had a second zeroing mechanism: when the solved shared-edge scale produced no numerically material width change, the fallback deferred all still-positive participants to zero without creating a conflict record. R13A.7 records both actual zeroing stages:

- `SharedEdgeUniformScale` when the solved uniform scale directly drives one or more participants below the stable-width floor;
- `SharedEdgeForcedDeferral` when the no-progress fallback explicitly drives the remaining positive participants to zero.

Each record preserves the collapsed unselected edge, stage, scale, ordered participants, the exact subset actually transitioned to zero, and each participant's positive width before the event. Final inactive capture and augmentation consume only a conflict whose exact zeroed subset contains the target, preventing attribution to an earlier conflict that merely involved the same edge. Geometry and ordinary width decisions are unchanged.

### Recovery-target and negative-exclusion policy

Finalized corner recovery admits only geometrically eligible edges that pass the ordinary requested-width-fraction gate. Width-recovery-provisional edges cannot initiate corner augmentation. This general policy excludes marginal edge `40` without introducing any seed or source-edge branch in production code.

A corner target is `certified-recovery` only when the winning augmented coverage actually builds that target. If the existing bounded target-aware frontier is completely exhausted without cancellation, time-budget exhaustion, or remaining states, the target is `corner-recovery-proven-infeasible`; otherwise it remains unresolved. Baseline count, identity, topology, geometry, placement, and render-channel commit guards remain authoritative.

The editor suite keeps the five historical positive fixtures and adds the negative fixture `8889 / maximum / edge 40`. Positive fixtures pass as either certified recoveries or complete-current-discrete-schedule infeasible results. The negative fixture requires edge `40` to remain inactive, uncertified, unmaterialized, and finalized as `corner-width-inactive`.

### Performance and scope

All R13A.7 work remains explicit preview/audit-only. The patch adds bounded diagnostic records and report formatting around already-executed operations. It does not alter production score weights, selected-count policy, generation contracts, shared mesh infrastructure, normals, tangents, scenes, prefabs, materials, shaders, layers, or tags.

Contracts advance to:

```text
EW-B4.2R13A.7-suite
EW-B4.2R13A.7-topology
EW-B4.2R13A.7-preview
EW-B4.2R13A.7-comprehensive
```

Unity compilation and one-click runtime validation remain required before R13A.7 can replace the R13A.4/R13A.6 stable incomplete baseline.

## EW-B4.2R13A.8 — micro-topology normalization

### Runtime evidence that requires the change

The R13A.7 Unity suite compiled and retained current preview, topology `33/33`, artistic preview `33/33`, and comprehensive evidence. It resolved only `3/5` positive fixtures, with zero certified recoveries. The current discrete construction contract labelled seed `8889` maximum-width edges `13` and `23` complete-infeasible, but visual inspection and their audit values contradict feature-level exclusion: both are long, near-right-angle, high-silhouette edges. Their common blocker is the microscopic triangular transition formed by original edges `14/24/30`. Edge `24` is the seed edge (`length≈0.002811`, below the canonical minimum style width); collapsing it alone aliases edges `14` and `30`, so the complete bounded triangle must be treated as one micro component.

The user explicitly authorizes complete removal of topology at this invisible scale when it is required to build meaningful bevels. This authorization is local to the edge-wear working topology and does not modify the base Generated Mass mesh.

### Detection and bounded component expansion

`NormalizeEdgeWearMicroTopology` runs once after the explicit non-`None` edge-wear gate and before candidate viability. It first builds the original source graph solely to preserve stable IDs. Internal manifold edges at or below the minimum useful style scale are seeds. A seed expands through connected internal edges no longer than the global minimum certified bevel footprint; the whole component diameter must also remain within that footprint. Components are capped at six vertices and eight edges.

This two-scale rule is deliberate. On seed `8889`, edge `24` is the sub-style seed, while adjacent edges `14` and `30` are still far below the minimum bevel footprint and form the same invisible triangular source face. Treating only edge `24` as removable would produce a duplicate remapped edge and is correctly rejected. Treating `14/24/30` as one component removes the complete micro sliver without touching edges `13`, `23`, or any other meaningful source edge. Edge `40` is substantially larger than the component limit and cannot enter normalization.

### Deterministic collapse and certification

For each component, the normalizer tries every existing component vertex as the canonical retained point. It remaps component vertices, removes duplicates from the retained point cloud, and rebuilds a temporary convex base hull with the existing supporting-plane extractor and facet ordering utilities. It rejects any candidate that:

- retains fewer than four points or four faces;
- emits an invalid, degenerate, non-convex, open, or non-manifold hull;
- expands original bounds or increases volume beyond tolerance;
- loses more than `0.25%` of original volume;
- collapses, aliases, or removes any source edge outside the authorized component.

The winning candidate minimizes total squared displacement, then volume loss, then original graph-vertex index. If no candidate certifies, that component is skipped and the original source faces remain authoritative. No partial normalization is committed.

### Identity, diagnostics, and downstream ownership

Normalized graph indices remain internal solver IDs. Every surviving normalized edge carries its original source-edge ID through endpoint-remap provenance. Suppressed component edges remain visible in lifecycle and source-edge overlays as `M` with final reason `micro-topology-suppressed`. Any hull transition edge with no original provenance is marked `micro-topology-generated-transition`, structural-ineligible, and assigned only a synthetic non-colliding diagnostic ID. Public artistic records, fixture lookup, comprehensive ranking evidence, viability summaries, and lifecycle reports all use stable original IDs.

Each eligible component emits one compact exhaustive record: component and seed edge IDs, graph vertices, diameter, bounded eligibility, every canonical-vertex attempt, squared displacement, resulting volume and volume loss, exact rejection blocker, and selected candidate. This evidence is required so a failed normalization cannot degrade into another opaque geometry blocker.

The normalized faces then own the entire explicit edge-wear transaction: viability, artistic ranking, corner widths, coexistence, plane construction, triangulation, and render-channel validation. The production score formula and deterministic strength variation remain stable by using original source-edge identity when normalization is active.

### Suite semantics

The seed-8889 maximum-width edges `13` and `23` are now visually required fixtures and may pass only as certified recoveries; a discrete infeasibility label is no longer accepted for them. The edge-40 negative fixture accepts any definitive inactive, uncertified, unmaterialized exclusion reason rather than one hard-coded reason, but rejects unresolved/provisional states and rejects micro suppression. The two seed-2223 edge-13 corner fixtures remain a separate unresolved class unless this patch independently changes their certified outcome.

Contracts advance to:

```text
EW-B4.2R13A.8-suite
EW-B4.2R13A.8-topology
EW-B4.2R13A.8-preview
EW-B4.2R13A.8-comprehensive
```

All normalization work remains explicit editor preview/audit-only. No scene, prefab, material, shader, UV, shared mesh infrastructure, production score, normal/tangent semantic, layer, tag, component, serialized default, or production generation-contract change is included. R13A.8 subsequently passed Unity compilation, current-preview validation, both `33/33` matrices, comprehensive evidence, and the seed-8889 positive/negative fixtures; its only remaining failures were the two seed-2223 width-recovery fixtures addressed by R13A.9.


## EW-B4.2R13A.9 — material width-recovery closure

### R13A.8 runtime result and corrected blocker

R13A.8 runtime validation retained current preview, topology `33/33`, artistic preview `33/33`, comprehensive evidence, and the edge-40 negative fixture. Micro-topology component `14/24/30` normalized successfully and seed-8889 edges `13` and `23` became selected, active, and certified while edge `39` remained certified. The only unresolved positive fixtures are seed `2223`, source edge `13`, at default and maximum width.

The earlier corner-inactivation hypothesis is superseded. Current code and runtime evidence show that edge `13` is isolated-certified below the ordinary requested-width-fraction gate, receives `WidthRecoveryProvisional`, and is then deliberately forced to width zero in the immutable certified baseline. Because R13A.7/R13A.8 excluded every width-provisional edge from augmentation triggers, that target was never evaluated by the recovery frontier. Its reported `corner-width-inactive` state was therefore baseline recovery deferral, not a missing corner-conflict event.

### Material-significance gate

R13A.9 preserves every raw width-provisional edge as a forced-zero baseline exclusion. A provisional edge may enter augmentation only when all of the following are true:

- isolated construction already certified a positive width at or above the canonical absolute minimum;
- the edge remains selected and artistically eligible;
- its source length is at least two complete requested bevel footprints, where one footprint is the existing `requestedWidth * 2 + guard` viability footprint.

This gate is seed-independent. The two seed-2223 edge-13 fixtures are long enough to qualify. Tiny seed-8889 edge `40` does not support two complete footprints and therefore remains baseline-only even though it is close to the ordinary width-fraction boundary. No production seed or source-edge branch is introduced.

### Certified baseline and target-wise frontier

The immutable baseline still forces every raw width-provisional and retained-hull recovery edge off, then requires a complete certified shell. Recovery does not replace this baseline.

The bounded augmentation frontier now starts with:

1. one state that re-enables all recognized recovery targets;
2. one state per target that re-enables only that target while retaining every other baseline exclusion;
3. existing participant-deferral variants only for finalized corner targets.

This target-wise seed is necessary because an all-at-once provisional state can fail for an unrelated recovery edge and conceal a viable target. Search remains capped at 128 states and five seconds with cancellation between states. No width schedule, hull constructor, plane-cut kernel, geometry tolerance, or nested search is added.

A material width-recovery result may commit only if the target is built in a fully certified shell, total certified count does not decrease, and every baseline-certified edge remains built. Unlike corner recovery, material width recovery authorizes no baseline-edge exchange. The existing score and certified-width ordering applies only after these hard guards pass.

### Lifecycle and diagnostic semantics

`ChamferCornerSolution` now retains the exact externally forced deferral set. A width-provisional edge forced off in the certified baseline is labelled `recovery-baseline-deferred`; it is no longer misrepresented as an organic corner-width collapse. Search-only forced deferrals use `augmentation-forced-deferred`. Genuine corner-inactive capture continues to require an exact zeroing conflict.

Viability evidence records the material width-recovery length threshold and eligibility. Lifecycle evidence records baseline deferral, terminal recovery resolution, and failure evidence. Complete frontier exhaustion produces `width-recovery-proven-infeasible`; cancellation, time-budget exhaustion, or remaining states preserves unresolved status. A winning shell marks only targets actually built as `certified-recovery`.

The existing `planeEdges` fields now map internal graph IDs through canonical source-edge provenance. Separate `planeGraphEdges`/`graphEdges` fields preserve the raw internal IDs for geometry debugging. This removes the R13A.8 ambiguity where normalized graph indices appeared to contradict micro-suppressed canonical source IDs.

### Scope and performance

R13A.9 changes only explicit edge-wear preview/audit selection, bounded augmentation orchestration, lifecycle evidence, editor fixture semantics, and canonical documentation. The base Generated Mass mesh, micro-topology implementation, bounded single-edge geometry, plane-cut kernel, artistic score, normals, tangents, scenes, prefabs, materials, shaders, serialized defaults, and `EdgeWearEvaluationMode.None` path are unchanged.

Active-gameplay impact is zero. Explicit evaluation adds a bounded number of initial frontier states and report fields, but no additional geometry algorithm or larger search budget. Contracts advance to:

```text
EW-B4.2R13A.9-suite
EW-B4.2R13A.9-topology
EW-B4.2R13A.9-preview
EW-B4.2R13A.9-comprehensive
```

Unity runtime validation rejected R13A.9 as incomplete. Current preview, both `33/33` matrices, comprehensive evidence, canonical diagnostics, micro normalization, seed-8889 edges `13/23/39`, and the edge-40 negative fixture remained correct. However, both seed-2223 edge-13 fixtures remained unresolved with no material target execution, while seed `7778` maximum entered the generic branch frontier and exhausted the five-second augmentation budget. R13A.9 is therefore historical evidence, not an accepted recovery baseline.


## EW-B4.2R13A.9a — immutable material-recovery target execution

### R13A.9 runtime diagnosis

R13A.9 computed valid material-significance evidence during isolated viability, but later rediscovered targets through mutable lifecycle state. The collector required a record to remain `Selected` after certified-baseline and coexistence mutations. Seed-2223 edge `13` had already been reset to inactive selection state by that point, so the target disappeared before augmentation even though its immutable evidence remained valid. The same mixed frontier admitted unrelated material candidates into branch expansion, producing the seed-7778 five-second timeout.

R13A.9a removes both failure modes. Material target identity is captured exactly once immediately after graph mapping, when `context.SelectedEdges`, stable graph provenance, isolated viability, and artistic eligibility are simultaneously authoritative. The target set is then passed explicitly through baseline certification and recovery; it is never reconstructed from trial-mutated `Candidate`, `Selected`, `CoexistenceEligible`, or final-reason fields.

### Immutable material-significance predicate

A selected graph edge becomes an immutable material target only when all of the following already hold:

- isolated rail and bounded construction succeeded;
- requested-width fraction is below the ordinary `0.25` viability threshold;
- isolated certified width remains at or above the canonical minimum certified width;
- the edge is artistically eligible;
- source length supports at least two complete requested bevel footprints.

The predicate remains seed-independent. Reported seed-2223 edge `13` satisfies it at default and maximum width. Seed-8889 edge `40` fails the two-footprint length requirement and never enters the immutable target set. All raw width-provisional edges, including material targets, remain forced off in the immutable certified baseline.

### One-trial sequential material phase

Material recovery is no longer part of the 128-state branch frontier. Targets execute in deterministic graph-edge order. For each target:

1. derive the exclusion set from the current certified working baseline;
2. remove only that target from the exclusions;
3. execute one complete-shell trial with kernel conflict recursion disabled and render-channel validation enabled;
4. accept only when the target is built, the shell is fully certified, certified count increases, and every edge built by the working baseline remains built;
5. when accepted, promote that result to the working baseline before evaluating the next material target.

This sequential commit allows multiple independent material targets to accumulate without a combinatorial search. A failed target creates no child states and authorizes no baseline-edge exchange. Cancellation or any trial timeout terminates the material phase immediately and retains the latest certified working baseline.

### Separation from non-material recovery

Retained-hull and exact corner-participant recovery remain under the existing bounded non-material frontier. They are collected only after the material phase. Any material target already certified is added to the protected-edge set and cannot become a branch victim. If the material phase times out or is cancelled, non-material recovery does not continue in the same evaluation.

The existing non-material limits remain unchanged:

```text
maximum states: 128
maximum elapsed time: 5000 ms
maximum additional deferrals: 8
```

No width schedule, rail solve, retained-point hull, plane constructor, topology tolerance, search budget, or production score changes. The only added geometry call site is the scheduler's direct invocation of the already-existing complete-shell trial evaluator, once per immutable material target.

### Lifecycle and report contract

Each material target carries bounded execution state:

- immutable target membership;
- certified-baseline deferral;
- attempted;
- completed without cancellation or timeout;
- trial succeeded under strict baseline preservation;
- certified in the committed working baseline;
- exact terminal failure evidence.

The compact evaluation summary emits canonical source-edge sets for `eligible`, `baselineDeferred`, `attempted`, `completed`, `certified`, and `failed`. A non-certified target always appears in `failed`, including an explicit `not-attempted` state. Detailed lifecycle output retains both historical baseline deferral and current trial deferral so the two cannot be conflated.

`width-recovery-proven-infeasible` may be assigned only after the target's exact full-shell trial completes without cancellation or timeout and fails the strict target-plus-baseline certification contract. Zero-execution and incomplete trials remain unresolved.

### Scope and current status

R13A.9a modifies only selection evidence ownership, orchestration scheduling, lifecycle/report fields, editor contract labels, and canonical documentation. It does not modify the geometry kernel, micro-topology normalization, base Generated Mass mesh, scenes, prefabs, materials, shaders, shared mesh infrastructure, serialized defaults, production score, normals, tangents, or `EdgeWearEvaluationMode.None`.

Contracts advance to:

```text
EW-B4.2R13A.9a-suite
EW-B4.2R13A.9a-topology
EW-B4.2R13A.9a-preview
EW-B4.2R13A.9a-comprehensive
```

Unity compilation and the complete one-click suite passed. Runtime evidence is:

```text
suite: passed
current preview: passed
topology: 33/33
artistic preview: 33/33
outlier resolution: 5/5
certified recoveries: 2
finite proven-infeasible fixtures: 3
unresolved: 0
negative exclusion: 1/1
cancelled: 0
terminal reason: none
```

R13A.9a supersedes R13A.8 as the accepted recovery baseline. R13A.9 remains a rejected intermediate. The basic bevel/recovery architecture is frozen.

## Basic-bevel freeze boundary and visual continuation

The accepted R13A.9a shell is the zero-irregularity reference for every later visual pass.

### Next phases

```text
EW-V1A  deterministic macro irregularity between edges [accepted]
EW-S1   shader-only broad bevel normal/material breakup [active]
EW-C1   sparse corner damage [next]
EW-C2   sparse localized edge chips/notches [later]
EW-F1   face/crack and final material/rendering finish [later]
```

### Recovery architecture constraints on irregularity

- Macro Variation Coverage or Strength at zero must reproduce the R13A.9a requested/resolved/materialized widths and edge identity.
- V1 may vary the requested width per canonical source edge but retains one constant width along each edge. Coverage chooses a deterministic canonical-edge subset; Strength controls the downward variation of participants. Existing corner, viability, recovery, and full-shell certification judge the real varied widths.
- Universal geometric along-edge taper is retired. EW-S1 uses no subdivisions, planes, topology mutation, or recovery state.
- Sparse corner/chip geometry may add bounded local topology only through explicit source-edge/vertex provenance and tier budgets. It may not bypass endpoint ownership, micro-topology normalization, recovery fallback, or final shell certification.
- Shader normal work must preserve the existing finite-unit mesh normal/tangent integrity contract; lighting-normal perturbation is not permission to weaken technical geometry validation.
- Rendering finish remains downstream of valid geometry and normals.
- No visual phase may add per-frame mesh work or silently change production `EdgeWearEvaluationMode.None`.

`EW-V1A.1` exposes Macro Variation Coverage and Macro Variation Strength for explicit editor geometry evaluation. Coverage `1` preserves the V1A all-edge multiplier stream; zero on either control restores exact uniform requests. Its Unity suite passed, but active Width testing exposed the incomplete construction-width schedule corrected by EW-V1A.2. Geometric Micro authoring and construction are removed. EW-S1 material-only controls do not enter geometry settings or recovery. Production evaluation is unchanged.


### EW-V1A.1 integration with the frozen recovery system

- Width identity and participation identity are captured from canonical original source provenance before graph remapping, so micro-topology normalization cannot silently change either decision.
- Participation is a second salted deterministic hash threshold. Coverage increases produce nested participant sets; Coverage `1` explicitly includes every ordinary evaluated edge even if the normalized hash reaches its representable upper endpoint.
- Each viability record stores base request, Coverage, Strength, participation identity/state, width identity, sampled multiplier, effective multiplier, minimum-style clamp state, and varied request. Trial coverage clones preserve this evidence.
- The certified baseline and all recovery trials receive the same edge-local requests. V1A.1 does not add a recovery class, alter provisional-width predicates, broaden allowed collateral, or expand the bounded frontier.
- Corner displacement limits use the maximum request of the active participating edges; per-edge clamp scales are measured against each edge's own request. When Coverage or Strength is zero these reduce exactly to the uniform-width contract.
- Downward-only sampling cannot demand a width broader than the accepted baseline, but narrower requests may truthfully change footprint or feasibility outcomes. Full shell, recovery, canonical identity, and negative-exclusion certification remain mandatory.
- Direct V1A.1 suite evidence is additive: zero-by-Strength parity, zero-by-Coverage parity, repeated determinism, full-Coverage compatibility, participant bounds, and active distribution are checked before the existing `33/33`, `5/5`, unresolved-zero, negative-exclusion, comprehensive, state-preservation, timeout, and render-integrity contracts.
- Inspector cleanup is authoritative: ordinary Coverage is labelled Bevel Coverage, geometric Micro controls are removed, and every visible Edge Wear control has a current geometry or shader consumer.


### EW-V1A.2 full construction-width schedule

The isolated width schedule is authoritative only when each scheduled width covers both rail solving and bounded shell certification. A rail-success width is not a certified feasible width by itself.

EW-V1A.2 applies the existing finite contract as follows:

1. solve rails at the current width;
2. if rails fail, continue the existing `0.75` schedule;
3. if rails succeed, run the complete existing owner/support construction, preparation, provenance, topology, containment, bounds/volume, triangulation, and render certification;
4. if that result fails the existing owner/support construction class, continue at `lastSolvedWidth * 0.75`;
5. return immediately for every other failure class;
6. commit the first completely certified width, or report complete infeasibility only after minimum-width or twelve-attempt exhaustion.

Rail failures and construction failures consume the same twelve-attempt budget. Ordered evidence retains every failed width and the terminal certified or proven-infeasible width. This is not a new recovery frontier and does not alter corner/coexistence augmentation, material recovery, geometry tolerances, or production evaluation.

The V1A.2 one-click contract compares zero-macro certified meaningful source-edge identity with the active case and independently rejects any current `owner-face-support-insufficient` exclusion whose width schedule remains unresolved.


### EW-V1A.2a stable-floor classification boundary

The canonical minimum style width and the absolute stable geometry width are distinct:

```text
minimumStyleWidth = ResolveGeneratedEdgeWearWidth(maximumDimension, 0.05)
minimumRequiredCertifiedWidth = minimumStyleWidth * 0.25
minimumStableEdgeLength = maximumDimension * 0.0012
```

Widths below `minimumStyleWidth` remain valid when they satisfy the accepted width-monotonic recovery floor. A reduced isolated result that reaches `minimumStableEdgeLength` has no legal downward adjustment left for shared-corner solving. V1A.2a therefore classifies only that floor-bound reduced result before artistic selection and corner construction. It does not lower any tolerance, change the twelve-attempt schedule, or reinterpret the minimum style tier as the terminal geometry floor.

The batch diagnostic contract also replaces the sentinel `none` with a captured corner blocker so any future early corner failure carries its exact reason.


## EW-V1A.2b editor fixture-proof boundary

V1A.2a runtime validation passed current preview, macro parity/determinism/distribution/retention, both `33/33` matrices, and the edge-40 negative gate. The remaining `3/5` outlier result was an editor classification mismatch: two non-certification-required seed-2223 fixtures terminated with the exact production reason `maximum-certified-width-at-stable-width-floor`.

The editor may treat that outcome as proven infeasible only when the artistic audit record confirms all of the following:

- geometric, candidate, and selection exclusion;
- isolated construction succeeded and both requested and maximum-certified widths are positive;
- active, certified, and materialized output are all zero;
- `ViabilityFailureReason` and `FinalReason` both equal `maximum-certified-width-at-stable-width-floor`.

This fixture proof does not change production recovery architecture and cannot satisfy a fixture that explicitly requires certified recovery. The distinct report label is `stable-width-floor-proven-infeasible`.


## EW-V1A recovery-system freeze boundary

The accepted `EW-V1A.2b` macro feature does not replace or loosen the R13A.9a recovery architecture. It supplies deterministic per-edge requested widths before the existing viability, corner, coexistence, recovery, and certification stages. Zero Macro Variation Coverage or Strength returns the exact uniform request path.

The V1A freeze preserves these mandatory properties:

- participation and width identities derive from shape seed plus canonical original source-edge identity;
- increasing Coverage adds a stable nested participant set;
- Strength is downward-only and cannot request more than the uniform Width reference;
- construction-width reduction may retain a meaningful edge only through the complete bounded schedule and full certification;
- a reduced result at the absolute stable geometry floor is excluded before corner solving;
- certified-baseline fallback, material/corner recovery protections, topology/quality gates, and negative exclusions remain unchanged;
- width remains constant along each edge, so no V2/V3 profile behavior is implied.

Runtime acceptance: current preview passed with `31/31` selected/active/certified; both matrices passed `33/33`; recovery passed `5/5`; unresolved was `0`; negative exclusion passed `1/1`; and no cancellation or terminal failure occurred. EW-S1 preserves this geometry/recovery result and moves along-edge visual breakup to the fragment shader. The next geometry architecture work is sparse corner damage.

## EW-S1 recovery boundary

EW-S1 intentionally has no recovery algorithm.

- The complete EW-V2A multi-plane profile constructor, isolation audit, effective-depth tiers, selective admission, additional-plane budget, and Micro-specific matrix contracts are removed by restoring the accepted V1A.2b generator/diagnostic source.
- Scalar plane-cut bevel construction, Macro width variation, micro-topology normalization, corner solving, coexistence search, material-width recovery, stable-floor classification, topology certification, render-channel certification, outlier fixtures, and edge-40 exclusion remain unchanged.
- The new surface controls are material-only fields on `GeneratedMass`. They are not members of `MassSurfaceFeatureSettings`, `ProductionGenerationState`, or `FeatureAtlasGenerationState`, and they do not participate in source-edge debug freshness or geometry fingerprints.
- The fragment shader evaluates broad variation only when `_GeneratedMassGeometryEdgeWearEnabled`, UV2.z, and Edge Surface Variation Strength are all nonzero. Strength zero returns before analytic wave evaluation and preserves the accepted V1A.2b normal/albedo/smoothness path.
- Shader variation cannot rescue invalid geometry and cannot conceal topology failures. If no bevel face exists, UV2.z is zero and EW-S1 has no effect.
- Sparse corner damage and chips must use separate bounded geometry plans and must preserve the existing recovery/fixture contract.
## EW-V1A.2c certified-baseline corner-retention ordering

### Runtime evidence that opened the patch

The accepted V1A.2b run at Macro Strength `0.55` certified seed 8889 source edge `38`. The EW-S1 validation run at Strength `0.67` reduced that edge's requested width only from `0.0185500644` to `0.0184908062`, while its isolated maximum certified width remained `0.012897186`; nevertheless, the final shell classified it as `corner-width-inactive` and the Macro retention contract reported `unprovenLosses:{38}`.

### Proven ownership defect

The certified baseline was created before material-width target trials, but corner-inactive recovery candidates were collected only after the material phase selected its working baseline. Material trials inherit baseline-selected exclusions through `CollectBaselineSelectedExclusions`; a corner-collapsed edge could therefore become an inherited exclusion before corner recovery inspected the original conflict. The corner collector also required `CornerRecoveryProvisional` before it called `ApplyCornerRecoveryProvisionalEvidence`, even though the exact `ChamferCornerConflictRecord` was already available.

### Corrected recovery order

1. Create and fully certify the scalar baseline.
2. Collect corner-inactive recovery edges and participant sets from that original baseline immediately.
3. Run material-width target recovery with its existing target and baseline-loss contracts.
4. Merge any additional corner-inactive records from the post-material baseline into the preserved original set.
5. Run the existing bounded augmentation frontier with material-certified edges protected and conflict participants as the only permitted baseline exchanges.
6. Mark an edge `corner-recovery-proven-infeasible` only when the bounded frontier exhausts without cancellation, timeout, or remaining states.

No corner math, shared-edge scale search, conflict participant calculation, plane kernel, acceptance predicate, search budget, or production path changes in EW-V1A.2c.

### Validation contract

The one-click suite now includes a full-coverage Strength sweep at `0`, `0.25`, `0.55`, `0.67`, and `1`. Each sample compares against the exact zero-Strength baseline. A sample passes only when every baseline-certified artistic edge is still materialized or has complete isolated or corner-recovery infeasibility proof. Current-setting retention remains a separate required check.

Status: superseded after Unity proved that the collector still inspected only a current-strength baseline. It did not construct the zero-strength certified set that defines Macro retention.


## EW-V1A.2d generator-owned protected Macro retention

### Why EW-V1A.2c could not recover edge 38

`TryAuditCertifiedBaselineAugmentation` called its first current-strength trial `certifiedBaseline`. The editor created the real Strength-zero result later and only for contract comparison. Consequently, the generator had no protected set against which to recognize edge `38` as a loss. The EW-V1A.2c early collector therefore remained empty for that edge and no restoration state entered the frontier.

### Protected baseline ownership

For nonzero Macro Coverage and Strength:

1. Construct a local settings value with the same authoring values and Macro Strength exactly zero.
2. Rerun candidate construction, isolated viability, artistic selection, graph mapping, and material-recovery target capture on the same normalized source faces.
3. Run the complete existing certified-baseline augmentation path without a protected baseline argument.
4. Treat only a fully valid shell with a non-null preview as the protected baseline. Failure to certify this baseline is a hard evaluation failure.
5. Build the requested-strength result through the existing material and corner recovery path.
6. Compare certified graph-edge identities before accepting the result.

### Local restoration

For each protected loss, gather exact `ChamferCornerConflictRecord.ParticipatingSelectedEdges`. If the current corner solution has no exact conflict record, gather only selected graph edges incident to the lost edge's two endpoint vertices. The lost edge itself is always included.

The requested widths of that finite participant set are blended toward the protected widths at factors `0.25`, `0.5`, `0.75`, and `1`. Every trial rebuilds and validates the complete corner and plane shell with render-channel validation enabled. Acceptance requires all protected baseline edges and all edges already certified by the requested-strength result. No participant may be exchanged away merely to restore the target.

If every local factor fails, the already-certified zero-Macro protected outcome is returned. This is a safety fallback, not proof that Macro distribution succeeded; diagnostics identify it and the independent distribution contract remains authoritative.

### Evidence

Coverage lifecycle records now carry protected-baseline membership/width, initial-loss state, restoration attempt/certification, selected factor, participant set, and resolution. The unified audit summarizes the protected set, losses, restored edges, participant set, factor, and `retained`, `local-restoration`, or `full-zero-baseline-fallback` resolution.

No cancellation, timeout, missing evidence, or failed local trial is classified as geometric infeasibility.

Status: source implementation complete; Unity validation pending.


## EW-V1A.2e asymmetric protected-width search

EW-V1A.2d runtime evidence retained edge `38` but selected `full-zero-baseline-fallback`; the final audit reported Macro Strength `0`, participants `0`, varied `0`, and `macroDistribution=0`. The uniform `0.25/0.5/0.75/1` path is therefore superseded.

The corrected protected search uses the certified requested-Macro outcome as its state origin. This preserves every unrelated requested-Macro width and all already committed material/non-material recovery. The pre-recovery source audit is not a valid trial origin.

For each protected loss:

1. The lost graph edge is mandatory.
2. Exact conflict participants are used; endpoint-star participants remain fallback evidence.
3. The mandatory loss and each selected optional participant target the protected lifecycle record's positive solved width, then materialized width, then isolated maximum certified width, then requested width.
4. For each adjusted edge only, current local and isolated certified-width ceilings are raised to that independently certified protected target when the current-strength preflight stopped lower.
5. Optional participants are enumerated as asymmetric binary subsets, capped at eight.
6. States are ordered by total normalized deviation from the current requested-Macro widths, then adjusted count and graph-edge key.
7. The first fully certified minimum-cost state that retains all protected and current built edges and preserves nonzero variation wins.

The unified evidence labels internal graph-edge participants/adjustments separately from displayed source-edge losses. `full-zero-baseline-fallback` remains available only as explicit safety output and must fail the nonzero Macro distribution contract.

Status: rejected and removed from active code by EW-V1A.2f. Runtime evidence showed the asymmetric subset search could evaluate 256 complete shell states for one case, remain synchronous for more than one minute, ignore cancellation until the case returned, and still select full-zero fallback.
## EW-V1A.2f recovery boundary

EW-V1A.2f removes every recovery mechanism introduced by EW-V1A.2c/d/e. The active architecture is again the accepted EW-S1/V1A.2b scalar path:

- no independent zero-Macro candidate or shell certification;
- no protected baseline edge set;
- no protected lifecycle clone or telemetry;
- no uniform-factor or asymmetric-subset restoration search;
- no full-zero-baseline fallback;
- no Macro-specific recovery frontier beyond the accepted V1A.2b material/corner recovery already required by scalar bevel construction.

Macro Strength is normalized before it reaches the unchanged viability and recovery architecture. The public `0..1` control becomes effective amplitude `0..0.55` exactly once in `ResolveEdgeWearMacroRequestedWidth`. At control `1`, the solver therefore receives the previously Unity-certified old Strength `0.55` request. Downward-only sampling, width schedules, corner solving, material-width recovery, stable-floor proof, shell certification, topology gates, and negative exclusions are otherwise unchanged.

Validation fail-fast is an editor workflow boundary, not a recovery rule. A failed current preview or Macro contract writes a report and stops before matrices; a failed topology matrix stops before the artistic matrix. Normal successful validation still runs the complete suite.


## EW-V1A.3b restored dihedral-biased request boundary

EW-V1A.3b restores the complete EW-V1A.3 request and recovery behavior. It does not add, remove, or weaken a recovery mechanism. The complete EW-V1A.3b Unity suite passed, so this request and recovery boundary is accepted and frozen.

- The existing deterministic Macro sample is initialized before structural classification so early-ineligible diagnostic records retain stable nonzero values.
- Once structural classification succeeds and proves a convex edge, the same resolver is evaluated with measured dihedral. The resulting multiplier, requested width, footprint requirement, and length-to-width ratio are applied together before any width-dependent viability gate.
- The angle permission uses a normalized smoothstep from `15°` to `90°` and retains `0.35` of downward-reduction permission at the sharp endpoint. It can only reduce the amount of narrowing as dihedral increases; it cannot request a width broader than the uniform base width.
- Hash salts, participation semantics, sampled `0.55..1` stream, normalized effective Strength `0..0.55`, minimum-style floor, isolated schedules, corner solve, material-width recovery, stable-floor proof, shell certification, topology gates, outlier fixtures, and negative exclusions remain unchanged.
- No protected baseline, duplicate shell, asymmetric subset search, fallback state, extra plane, or geometric within-edge profile is restored.
- Removing EW-S1 shader breakup has no recovery consequence. The uniform UV2.z bevel response remains, while object-space wave, normal perturbation, material variation, and smoothness variation are deleted.


## EW-V1A.3a rejected recovery experiment

EW-V1A.3a changed the existing conflict-cluster reduction from uniform scaling to dihedral-priority scaling and added a global endpoint materialized-scale comparison. The supplied Unity report failed topology at `28/33`, including time-budget exhaustion, state-budget exhaustion, a collapsed bevel band, and one reported hierarchy inversion. That experiment is removed by EW-V1A.3b.

The restored recovery rule is the previously certified EW-V1A.3 rule: every edge in an implicated conflict cluster receives the same bounded reduction factor, and every existing isolated, locality, foreign-source-vertex, rail, topology, manifold, face-quality, and render certification remains authoritative.

The narrow seed-8889 source edge `10` is reduced before conflict recovery by isolated certification. Its wider attempts fail the existing foreign-source-vertex and exact-rail contracts; EW-V1A.3b records this as a deferred construction limitation rather than weakening recovery invariants.



## EW-V1A.3b accepted freeze and EW-C1A boundary



The accepted Unity report records current preview pass, Macro zero parity, angle mapping, determinism, distribution, and retention; topology `33/33`; artistic preview `33/33`; outlier resolution `5/5`; negative exclusion `1/1`; cancellation `0`; and terminal reason `none`.



The following recovery behavior is frozen:



- continuous dihedral-biased requested width;

- isolated width schedules and certified floors;

- corner/shared-width solving;

- material-width recovery;

- uniform conflict-cluster reduction;

- locality, foreign-source-vertex, rail, manifold, topology, face-quality, render, and negative-exclusion contracts.



EW-C1A must not modify these systems as an implicit corner-damage recovery mechanism. The completed ordering audit approves a transactional pre-bevel half-space cut after micro-topology normalization and before bevel candidate construction. Any corner cut must reject the trial when it cannot preserve original-edge descendant identity, creates anything other than one cap, violates manifold/topology/face-quality/render certification, exceeds the approved local volume loss, or prevents the later bevel shell from preserving unrelated frozen edge-wear results.

## EW-C1A pre-bevel corner-damage recovery boundary

**Decision:** pre-bevel cut approved. Post-bevel raw cutting is rejected.

C1A inserts between accepted micro-topology normalization and `BuildEdgeWearBevelCandidates`. It does not weaken, replace, or bypass any frozen V1A.3b recovery rule. The damaged source polyhedron becomes the input to the existing isolated-width, corner/shared-width, material-recovery, coexistence, plane-shell, topology, face-quality, and render-certification systems.

### Transaction boundary

Each depth trial operates on a fresh clone of the normalized `PolygonFace` list. The accepted source list is unchanged until one trial passes all requirements. The existing exact clipping primitives are reused, but C1A requires a structured success/failure wrapper because `ClipPolyhedron` currently has no return result or descendant-edge evidence. The implemented wrapper is `TryClipCornerDamageTransaction`; it assigns `PolygonFaceProvenanceKind.CornerDamageCap`, preserves the source clone on failure, and returns the clipped clone, one cap, exact numerical-repair telemetry, and a concrete blocker. `EvaluateCornerDamageTransaction` owns candidate ranking and the four bounded attempts, while `AuditCornerDamageIdentityMapping` proves untouched, shortened-descendant, and generated cap-ring identities.

The trial schedule is fixed:

```text
1.000000
0.750000
0.562500
0.421875
```

A failed trial is discarded completely. No partial face, intersection point, provenance assignment, or identity-map mutation survives.

### Required trial certification

The cut result must prove:

- one and only one cap face;
- at least three unique cap vertices;
- exact cap-plane residual within the existing exact-intersection tolerance;
- closed manifold topology, zero open edges, zero non-manifold edges, and zero T-junctions;
- finite, planar, simple, convex, outward-wound faces above the minimum area;
- positive volume loss no greater than `12%` of normalized source volume;
- no source-bound expansion beyond tolerance;
- every untouched original edge still maps to its frozen original ID;
- every shortened incident original edge maps to its parent original ID;
- no new edge aliases an original ID;
- accepted vertex/triangle budgets remain intact.

### Recovery identity boundary

The current `EdgeWearMicroTopologyNormalizationResult` exact-key map remains authoritative for untouched original edges. C1A adds result-owned maps for the three shortened original-edge descendants and generated cap-ring edges. These maps are consumed before Macro sampling and candidate diagnostics.

Cap-ring edges are not ordinary recovery participants in C1A.1. In C1A.2 they receive a dedicated constant requested width and bypass Macro variation. Once passed to the existing shell, all normal viability, corner, conflict, and render rules apply; no cap-ring-specific topology exception is permitted.

### Unrelated bevel preservation

Because the cut precedes candidate construction, preservation cannot be defined by comparing raw post-bevel polygons. It is defined by stable original edge identity:

```text
unaffected original edge that was certified by the undamaged V1A.3b baseline
    -> remains present in damaged source identity map
    -> remains geometrically eligible unless its local corner star was cut
    -> remains active/certified in the final shell or reports an explicit local corner-star reason
```

Edges incident to the selected damaged corner may change length, dihedral, or eligibility. All other original edges are protected from silent loss.

### Normal and tangent boundary

Recovery owns polygon geometry and authored face-plane normals only. It does not calculate the final Unity tangent channel. Final triangle topology is produced first; `BuildMeshData` then assigns authored or geometric render normals; `MeshBuilder.ApplyToMesh` recalculates tangents after final UV and index assignment. Corner damage therefore cannot run after final normals/tangents.

### Staged promotion

- `EW-C1A.1` is implemented as `EdgeWearEvaluationMode.CornerDamageTransactionAudit`. `ApplyGeneratedEdgeWearBevels` captures the transaction immediately after normalization and returns before `BuildEdgeWearBevelCandidates`, so it is diagnostic-only and cannot replace the accepted preview mesh. The Inspector writes `Library/GeneratedMassCornerDamageTransactionAudit.txt` and copies the report to the clipboard.
- `EW-C1A.2` retains the exact certified damaged faces and identity maps, classifies every cap-ring edge as mandatory/non-Macro, and renders the damaged corner only after all cap-ring bevels and all unrelated-baseline retention checks pass. Any partial result falls back to production geometry.
- `EW-C1A.3` must pass disabled/default/maximum policies over the 11 accepted seeds before freeze.

No production `EdgeWearEvaluationMode.None` promotion is included in C1A unless separately planned and approved.

## EW-C1A.1a / EW-C1A.1a.4 — Mandatory one-polygon render contract

Before corner-damage geometry can be promoted, the final polygon shell must not expose internal GPU triangulation as apparent rock cuts. One accepted `PolygonFace` therefore owns exactly one authored render normal, one stable authored surface group, one feature classification, and one feature strength across all emitted triangles.

The active selection order is:

```text
1. resolve one authoritative polygon normal and one stable surface group
2. try every stable direct boundary fan
3. if no direct anchor certifies, project the ordered boundary to the face plane
4. reject a zero-area or self-intersecting projection
5. run complete interval dynamic programming over all valid splits
6. emit exactly n - 2 triangles from the best complete solution
7. fail only when no complete certified triangulation exists
```

A candidate triangle and every selected internal diagonal must preserve the polygon boundary contract:

```text
triangle area > minimumTriangleArea
finite winding can be oriented to authoredNormal
normalized geometric/authored normal agreement >= 0.5
non-boundary diagonal midpoint remains inside/on the retained projected simple loop
non-boundary diagonal does not intersect unrelated boundary segments
```

Diagonal containment reuses `IsBoundedPointInsideOrOnPolygon` after unrelated-boundary intersection rejection. This is required because `BuildBoundedConvexityCheckLoop` may remove a tolerance-collinear slight reflex vertex while final emission retains it. The complete-solution ranking is deterministic: highest minimum triangle area, then highest minimum normal agreement, then lowest split index. This avoids the greedy-ear failure mode while preserving bounded `O(n^3)` dirty-time and `O(n^2)` temporary state. No per-frame path exists.

EW-C1A.1a.4 supersedes the projected-centre fallback from EW-C1A.1a.2/.3. Both active modes emit exactly `n - 2` triangles and introduce zero synthetic vertices. `PolygonSurfaceInternalFanVertexCount` and `BevelRegionInternalFanVertexCount` are again literal zero-only certification fields. The final `BuildMeshData` / `ValidateGeneratedMassMeshData` `0.5` guard is unchanged.

The group namespace preserves the accepted bevel identity formula and uses a separate ordinary-polygon prefix. Duplicate group ownership inside one shell is a certification failure. `PolygonSurfaceRenderValid` is required by baseline construction and every topology-scale retry before preview mesh acceptance. When no triangulation succeeds, the propagated failure reason records direct-anchor evidence, projected solver states, rejection counts, and selected triangle indices when present.

This correction changes no clipping, source topology, recovery scale, candidate selection, width solve, material identity, mesh channel, shader, serialized control, or runtime behavior. It remains before EW-C1A.2 because committed corner clipping creates an additional polygon cap, and that cap must inherit the same render contract immediately.


## EW-C1A.1a.5 retained-boundary normalization and reinsertion

The render-surface recovery order is now:

```text
1. stable direct boundary fan on the complete retained loop
2. complete interval-DP triangulation on the complete retained loop
3. tolerance-collinear working-loop simplification plus reverse boundary reinsertion
4. fail with exact local geometry evidence
```

Stage 3 may run only after stages 1 and 2 fail with normal-agreement evidence. A working-loop vertex is removable only when its projected point lies within the bounded collinear tolerance of its current neighbour segment, its local raw triangle is unstable, all local vertices remain inside the already-certified face-plane tolerance, the replacement boundary segment is contained and non-intersecting, and the reduced loop preserves signed-area orientation and simplicity.

Each removal stores the surviving original neighbours. After a simplified direct/DP solution is found, removals are replayed in reverse. The unique selected triangle owning the stored neighbour edge is replaced by two triangles containing the removed original vertex. Both replacements must pass the unchanged final triangle certification before the selected list is mutated.

The final certification requires exactly `originalVertexCount - 2` triangles, every original boundary segment owned exactly once, no synthetic vertex, zero internal fan vertices, one authored normal/group per polygon, and unchanged final mesh-normal agreement. Failure evidence includes original/projected coordinates, plane residuals, every candidate classification, retained/removed indices, simplified triangles, and reinsertion areas/normal dots.


## EW-C1A.1a.6 one-surface normalization parity

EW-C1A.1a.5 successfully resolved the previously untriangulatable six-vertex bevel polygon to a simplified solution and deterministic reinsertion. Unity evidence then showed replacement triangle `0/1/5` with finite area `7.15370675E-07` but reported `normalDot=0`. Explicit double-precision normalization of the reported cross product yields authored-normal agreement approximately `0.9999999996`.

The failure was caused by inconsistent normalization ownership:

```text
candidate cross passes finite/non-zero and minimum-area gates
candidate agreement uses Unity Vector3.normalized
small accepted cross may normalize to Vector3.zero
reported agreement becomes zero
```

`EvaluateOneSurfaceTriangleCandidate` and `TryResolveOneSurfaceTriangle` now use the existing `TryNormalizeMassVector` helper. Robust-normalization failure remains classified through the existing `NonFinite` result. Area and normal-agreement thresholds are unchanged.

The direct/DP/reinsertion order, deterministic ranking, boundary ownership, exact `n - 2` triangle count, zero internal fan vertices, analytical authored normals, stable surface groups, and all topology/recovery behavior remain unchanged. `MassGenerator.Types.cs::TryNormalizeMassVector`, `MassGenerator.MeshOutput.cs::BuildMeshData`, and `ValidateGeneratedMassMeshData` are unchanged and remain the authoritative final render-normal contract.

## EW-C1A.1a.7 final-space surface-normal ownership

EW-C1A.1a.6 restores the complete Macro contract and exposes one remaining topology-matrix failure at seed `6667`, maximum width, in final render-normal validation. The polygon-surface triangulation audit passes before dimensions and placement deformation, while the final `MeshData` validator compares transformed triangle geometry with the stored construction-space authored normal.

Recovery and triangulation continue to own polygon geometry, provenance, construction-space plane normals, and stable surface-group IDs. They do not own the final transformed render normal. The final owner is `MassGenerator.MeshOutput.cs::BuildMeshData`, after `ApplyDimensions` and `ApplyMassPlacementFrame` have completed.

For each authored surface group, `BuildMeshData` performs one deterministic final-position prepass. It orients each raw final geometric cross toward the stored authored plane normal, accumulates the raw crosses by group in double precision, and explicitly normalizes the sum. Every triangle in the group uses that one rebuilt normal and is independently required to meet the unchanged `0.5` agreement threshold. A failure reports group, triangle, agreement, original normal, rebuilt normal, final geometric normal, and transformed positions.

The stored authored normal is used only as a deterministic hemisphere guide during accumulation. Final winding and certification use the rebuilt final-space normal. This preserves one polygon as one visual surface through non-uniform dimensions, lean shear, and nonlinear grounding without changing polygon geometry or splitting surface groups.

The additional work is `O(T)` dirty-time and `O(G)` temporary state for `T` triangles and `G` authored surface groups. It adds no per-frame work. Direct fan, complete interval DP, tolerance-collinear reinsertion, exact `n - 2` output, zero internal fan vertices, Macro width, topology/recovery, and the complete-mesh validator remain unchanged.

This closes infrastructure only. On full Unity acceptance, EW-C1A.1a is frozen and the next active implementation is EW-C1A.2 corner-cut and cap-ring chip geometry, not EW-N1 artistic normals.

## EW-C1A.1a.8 shared-normal feasibility ownership

EW-C1A.1a.7 proves that final transformed-space ownership is necessary but its area-weighted candidate is not sufficient for seed `6667`, maximum width, `SourceFace:0`: the current group average reaches only `0.44671616` agreement for triangle `0` while the required threshold remains `0.5`.

Recovery, triangulation, clipping, and placement deformation remain unchanged. `MassGenerator.MeshOutput.cs::ResolveTransformedAuthoredSurfaceNormals` now owns a bounded feasibility search over the final authored-group triangle normals. It preserves the existing authored-hemisphere orientation and evaluates:

```text
area-weighted final normal
all individual triangle normals
all finite pair bisectors
both finite equal-angle centres for every triangle-normal triple
```

The selected candidate maximizes exact minimum agreement, then exact area-weighted average agreement, then uses defining triangle indices and candidate kind for deterministic ties. The support enumeration is complete for the only accepted domain: existence of one shared normal with every triangle dot at least `0.5`. A failure is definitive threshold-infeasibility evidence and reports full group/candidate/per-triangle data. It does not weaken the threshold, split the polygon, restore per-triangle render normals, change winding policy, or mutate grounding.

Per-group cost is `O(m^4)` dirty-time and `O(m)` temporary state; there is no per-frame owner. EW-C1A.1a.6 triangulation, exact `n - 2` output, authored group identity, Macro width, topology/recovery, and the complete-mesh validator remain unchanged.

Unity passed Macro preservation, topology and preview `33/33`, outliers `5/5`, negative exclusion `1/1`, and the former-X visual inspection. EW-C1A.1a is frozen and C1A.2 now owns chip geometry. A future exact acceptance failure below `0.5` would require a separately approved placement-deformation correction, not another render-normal exception.


## EW-C1A.2 committed-transaction and mandatory-ring recovery boundary

The certified C1A.1 result is no longer reconstructed after audit. The accepted trial commits the exact damaged face list, cap face, accepted depth, stable identity for every damaged output edge, generated cap-ring key/identity set, and the three affected original parent identities. A generated cap-ring identity that aliases any original identity rejects the trial.

The explicit editor preview enters `UnifiedBoundedPreview` through a scoped thread-local request. Orchestration substitutes the committed damaged faces immediately after micro-topology normalization. `BuildEdgeWearBevelCandidates` resolves identity from the committed output-edge map before falling back to the frozen normalization map.

Cap-ring candidates form a dedicated mandatory class:

```text
requestedWidth = min(0.50 * ordinaryRequestedWidth,
                     0.25 * acceptedDepth,
                     0.20 * shortestCapEdgeLength)
Macro participation = 0
artistic filtering = bypassed
coverage selection = mandatory before ordinary coverage
```

This classification does not grant a topology or recovery exception. A mandatory ring edge can still fail structural convexity, footprint, locality, isolated viability, shared-corner solving, width floors, conflict reduction, plane construction, manifold, face-quality, one-surface, or final render-normal certification. Any such failure rejects the whole preview so a raw or partially softened cap never reaches the displayed mesh.

The preview runs the frozen undamaged unified shell first. Built ordinary edges are compared by stable original identity. The three shortened parent identities are locally affected and exempt; every other baseline-certified identity must remain certified after the corner transaction. This two-generation proof is explicit editor/dirty-time work only and changes no active-gameplay owner.
