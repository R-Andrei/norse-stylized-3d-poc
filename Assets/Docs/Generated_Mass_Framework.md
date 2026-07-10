# Generated Mass Framework

## Status

- **Active edge-wear architecture:** EW-C — Explicit Single-Segment Chamfer Kernel
- **Current implementation step:** EW-C2S6R1 — Source-boundary loop retrace normalization
- **Geometry emission:** provisional build and audit only; final geometry commit remains disabled

## Feature goal

Generated masses need a crude physical chamfer on selected exposed convex source edges. The first production target is deliberately limited:

- one bevel strip per selected manifold edge;
- one new quadrilateral surface, or two triangles, between the two trimmed source faces;
- crude triangle-fan corner closure;
- no rounded profile, sampled ribbon, or arbitrary segment count;
- no runtime topology work.

The chamfer is generated from the final source `PolygonFace` polyhedron before final triangulation.

## Canonical generation order

1. Generate the final source mass faces.
2. Discover and score convex edge-wear candidates.
3. Select candidates from Coverage.
4. Build explicit source topology.
5. Preserve and classify source boundary loops.
6. Order the incident face/edge fan at every affected source vertex.
7. Solve one replacement corner per `(source face, source vertex)`.
8. Emit one replacement polygon per affected source face.
9. Emit one `ConvexEdgeWear` quad per selected internal manifold edge.
10. Emit one crude corner patch per contiguous selected run at a source vertex.
11. Validate that no new boundaries, non-manifold edges, or T-junctions were introduced.
12. Triangulate once.

## Validated EW-C0 healthy baseline

EW-C0 performs only candidate selection and source-topology readiness auditing. It does not alter the source face list. With edge wear enabled, the generated mass must therefore render exactly as it did before physical edge-wear geometry.

The readiness audit reports:

- source faces, vertices, edges, and directed half-edges;
- source boundary edges and traced boundary loops;
- source non-manifold edges and T-junctions;
- selected manifold, boundary, and non-manifold edges;
- affected closed and open vertex fans;
- selected-run readiness.

A source boundary is not automatically an error. Future validation compares the output boundary set against the explicitly preserved source boundary set.


## EW-C1 geometry-proof baseline

EW-C1 keeps the source mass visually unchanged while solving the coordinates needed by the first chamfer emission patch. It builds one explicit replacement point for every `(source face, source vertex)` corner.

Corner rules:

```text
neither adjacent edge selected -> preserve source vertex
one adjacent edge selected     -> selected offset line / source line intersection
both adjacent edges selected   -> offset line / offset line intersection
```

For a source face edge `A -> B` with unit direction `d`, face normal `n`, inward in-plane direction `m`, and solved edge width `w`:

```text
d = normalize(B - A)
m = normalize(n x d), sign-corrected toward the face centroid
L(t) = A + w m + t d
```

EW-C1R3 also:

- begins with one conservative constant width per selected source edge;
- iteratively reduces participating edge widths when an acute corner creates an excessive miter displacement;
- feeds insufficient common intervals on unselected internal edges back into the same monotonic width solve;
- uses a bounded binary search to find the largest stable scale for selected edges controlling a failed shared interval;
- keeps one width for the full source edge, so a correction at one endpoint propagates to the other endpoint;
- reconciles the endpoint identity of every unselected internal source edge after convergence;
- validates all hypothetical replacement polygons;
- validates the four points needed by every future one-strip bevel quad;
- verifies that the preserved source boundary loop does not collapse;
- emits no replacement faces, bevel strips, or corner patches.

The next geometry patch may proceed only when the unified corner/shared-edge solve converges and the corner audit reports `readyForChamferEmission=1`.



## EW-C2S5R1 face-local normalized provisional baseline

EW-C2S replaces the rejected EW-C2R conflict-deferral loop. The validated EW-C1R3 network remains authoritative: only candidates that genuinely require sub-stable width are deferred, while duplicate endpoint symptoms no longer trigger broad candidate removal.

Every inactive internal source edge is represented by an immutable shared middle span. The two incident replacement faces both emit that exact middle segment. Their face-specific terminal tails remain separate and are registered, together with active bevel-strip endpoint segments, as explicit source-vertex patch boundaries.

The construction ownership is therefore:

- replacement face owns its face-specific edge chain;
- inactive internal source edge owns the exact shared middle span;
- active selected edge owns one bevel strip;
- source vertex owns strip endpoints and inactive-edge tail segments;
- preserved source boundary remains explicitly identified.

Solved corners are not mutated sequentially. No compatibility loop removes otherwise valid active chamfers. EW-C2S succeeds only when every provisional opening belongs to the preserved source boundary or one valid explicit vertex-boundary chain/loop, with zero unexpected openings, zero missing boundaries, zero non-manifold edges, and zero T-junctions.

## EW-C2 provisional emission baseline

EW-C2 consumes the validated EW-C1 corner table and solved per-edge widths without recomputing them. It builds a temporary face list containing:

- one replacement polygon for every source face;
- one `ConvexEdgeWear` quad for every active positive-width selected edge;
- no vertex-run corner patches yet.

Every allowed provisional opening is registered before topology audit. The only valid open edges are:

- the solved descendants of preserved source-boundary edges;
- endpoint edges of active bevel strips that are reserved for EW-C3 vertex patches.


The first mixed EW-C2S2 validation sample contained 23 distinct generated topologies: 12 passed and 11 failed. EW-C2S3 then moved T-junction segmentation ahead of boundary normalization and preserved raw provisional provenance. EW-C2S4 added guarded preserved-source-boundary subdivision and compact diagnostics.

The complete EW-C2S4 placed-object validation contained 24 unique objects, each logged twice through `OnValidate` and `OnEnable`. Thirteen objects reached `readyForVertexPatches=1`; eleven remained blocked. Every object reported `provisionalTJunctions=0` and `tJunctionRecordsIncompatible=0`, proving the EW-C2S3/S4 segmentation defect is solved. The remaining failures split cleanly into eight objects with repeated face-local topology edges producing non-manifold/multi-owner results, and three objects with a separate preserved-source-boundary descendant mismatch.

EW-C2S5 addresses only the first remaining class. Before a replacement face or bevel strip participates in topology counting, its cyclic vertex walk is reduced by exact inverse-edge cancellation. A sequence `A -> B -> A` contributes no polygon boundary and is removed without moving any surviving point. Consecutive duplicate topology vertices are also removed. The same reduction runs after raw T-junction segmentation because split insertion can expose retraces on replacement tails or bevel endpoints. Any remaining repeated undirected edge inside one face is a hard failure rather than an inferred repair.

Processing order is now:

```text
provisional faces + face-local raw boundary registrations
-> exact initial face-walk retrace normalization
-> source-compatible fixed-point segmentation
-> preserved source-boundary subdivision
-> split matching expected vertex-boundary registrations
-> exact post-segmentation face-walk retrace normalization
-> rebuild edge-use counts
-> reconcile registrations against normalized topology
-> normalize boundary ownership
-> audit components and final topology
```

Expected vertex boundaries may be split into ordered child segments while preserving source-vertex, source-edge, source-face, and boundary-kind provenance. Preserved source-boundary parents are replaced by the same ordered child chain whenever a strict-interior provisional vertex subdivides them. This changes only segmentation, not boundary position, winding, ownership, or shape. The split requires raw source-vertex provenance, an actual provisional mesh vertex, a stable non-zero containing segment, strict interior placement outside endpoint tolerance, and point-to-segment distance within topology tolerance.

EW-C2S5 builds replacement-face boundary registrations locally, reduces the face walk, and publishes only registrations whose topology edges survive. After segmentation, registrations removed by an exact retrace are discarded only when their edge has zero remaining provisional uses. EW-C2S5 validation proved all 24 placed objects now have zero non-manifold edges, zero T-junctions, zero face-local duplicate-edge failures, and zero stale boundary registrations. Sixteen of 24 objects reached `readyForVertexPatches=1`; five additional objects were blocked only because a clean two-face internal edge used the same encoded direction in both face records, while three objects retained the separate preserved-source-boundary mismatch.

EW-C2S5R1 classifies openness from incidence rather than winding direction. A registered edge with exactly two actual provisional uses on two distinct face records is already internally closed and all registrations for that key are cancelled. Opposite-direction pairs remain the expected orientation; same-direction pairs increment `sameDirectionClosedInternalEdges` as a non-blocking diagnostic. A one-use edge keeps one compatible source owner. Zero-use stale registrations, two uses from the same face record, or more than two uses remain hard failures. This does not weaken final non-manifold, T-junction, winding, or face-local duplicate-edge validation.

EW-C2S5R1 validation reached the predicted gate: 21 of 24 placed objects report `readyForVertexPatches=1`. All 24 report zero non-manifold edges, zero T-junctions, zero face-local duplicate edges, zero stale registrations, and zero ownership failures. The only remaining blockers are three source-boundary objects: two with `expectedSourceBoundaryEdges=4, matchedSourceBoundaryEdges=2`, and the 36-edge boundary rock with `expectedSourceBoundaryEdges=5, matchedSourceBoundaryEdges=3`.

EW-C2S6 replaces the source-boundary `HashSet<TopologyEdgeKey>` as the authoritative model with one ordered `ChamferSourceBoundaryRecord` per original boundary half-edge. Each record retains source-edge identity, boundary-loop index/order, original source endpoints, parent segment, and an ordered child list. Split plans subdivide the matching child record directly, so split counts are unique source-owned subdivisions rather than provisional-face occurrences.

A subdivided source-boundary edge has two terminal transition children touching its original source vertices. Those terminal children are explicit source-vertex transition candidates. A terminal child with exactly two uses on two distinct provisional faces transfers to the source-vertex transition and is not expected open; a terminal child with exactly one use and no vertex-boundary registration remains a valid source-boundary opening. Non-terminal middle descendants and unsplit source-boundary children must remain one-use source-boundary openings. The expected topology-key set is derived from these records only after segmentation and vertex-boundary normalization. Duplicate descendant keys, incorrect terminal incidence, source/vertex ownership overlap, and non-unit incidence on expected open descendants remain hard failures with compact source-edge/loop/child diagnostics.

The provisional topology must contain no unexpected openings, no missing expected vertex-patch boundaries, no non-manifold edges, and no T-junctions. Normal operation emits one readiness summary, one corner summary, and one provisional-emission summary. Intermediate per-pair segmentation logs are suppressed; a failed final topology audit emits one deduplicated warning containing at most three actionable records. The face list is discarded after audit and the original source mass remains the rendered geometry.

## EW-C2S6R1 source-boundary loop retrace normalization

EW-C2S6 validation executed across all 24 placed masses and reached `readyForVertexPatches=1` on 21 objects. The three remaining source-boundary objects retained zero non-manifold edges, zero T-junctions, zero unexpected provisional openings, and zero missing vertex boundaries, but reported paired source-child incidence and duplicate-child-key failures. Their warnings contain adjacent children in the same ordered source-boundary loop with exact inverse `VertexKey` endpoints:

```text
A -> B
B -> A
```

These two children describe a zero-boundary excursion, not two open source-boundary descendants. EW-C2S6R1 normalizes each boundary loop after split application, final provisional edge-use reconstruction, and vertex-boundary normalization. Records are ordered by `BoundaryLoopIndex`, then `BoundaryOrder`, then child index. Adjacent exact inverse children, including the cyclic last/first pair, are removed from the source-boundary ownership walk only when their shared topology key has exactly two provisional uses on two distinct face records and has no expected vertex-boundary ownership.

The pass moves no coordinates, changes no candidate or width decisions, and modifies no provisional face geometry. A malformed loop order or an inverse pair that fails its incidence, face-provenance, or ownership guards remains a hard failure. Diagnostics report raw descendants, removed retrace pairs and children, normalized descendants, and normalization failures. Geometry remains provisional and final commit remains disabled.

## Validation invariant

Let `B_source` be the preserved source-boundary edge set and `B_output` the output-boundary set.

```text
new boundaries = B_output - B_source
```

A valid chamfer result requires:

```text
new boundaries = 0
output non-manifold edges = 0
output T-junctions = 0
```

It does not require a globally closed mesh when the source intentionally contains a boundary loop.

## Width ownership

The first chamfer implementation will use one constant solved width per selected source edge.

For source edge `e=(u,v)`:

```text
w_e = min(requested width, feasible width at u, feasible width at v)
```

The initial conservative feasibility bound is:

```text
w_max(vertex) = 0.25 * minimum adjacent source-edge length
```

This coefficient is a proof-stage safety policy, not the final artistic mapping. After this first bound, EW-C1R3 performs a monotonic global pass. For any corner whose solved displacement `d` exceeds its permitted displacement `d_max`, each selected edge participating in that corner is reduced by `0.95 * d_max / d`, clamped conservatively while widths remain useful. Widths only decrease and remain constant along each complete source edge. When an unselected internal edge has no stable common interval, all selected edges controlling its four incident corners are reduced together until the interval is preserved. If preserving the source edge requires a chamfer below the useful stable width, those locally incompatible candidate edges are deferred by assigning width zero; the rest of the compatible selected network remains active. Pre-existing short source edges are required to preserve their own source length rather than an unrelated larger render threshold.

## Preserved systems

- final-source candidate discovery;
- convexity and scoring;
- Coverage selection;
- source topology graph;
- generic topology audit;
- material-feature identity for future `ConvexEdgeWear` faces.

## Retired architecture

EW-B is retired. It independently rebuilt source faces, emitted local bridge faces, then inferred ownership from resulting open-edge coordinates and attempted post-hoc cap closure. That architecture repeatedly produced unexplained boundaries and T-junctions. Its construction code, cap experiments, rail records, and EW-B-only diagnostics have been removed.

## Performance policy

Chamfer topology is generated before gameplay and cached with the generated mass. The first implementation prioritizes deterministic, low-complexity geometry:

- one bevel strip per selected edge;
- no subdivision profile;
- no per-frame geometry work;
- no atlas dependency for the physical chamfer.

## Next work items

1. Compile and validate EW-C2S6R1 across all 24 placed masses.
2. Confirm the three boundary-loop objects report non-zero guarded retrace removals and `sourceBoundaryLoopNormalizationFailures=0`.
3. Require every object to report zero source-boundary incidence, duplicate-key, and terminal-transfer failures.
4. Require all 24 objects to report `expectedSourceBoundaryEdges=matchedSourceBoundaryEdges` and `readyForVertexPatches=1`.
5. Confirm candidate, active/deferred, replacement-face, and bevel-strip counts remain unchanged.
6. Keep geometry provisional and commit disabled during validation.
7. Begin EW-C3 only after the complete representative sample passes the final EW-C2 gate.
