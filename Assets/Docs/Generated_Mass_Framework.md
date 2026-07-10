# Generated Mass Framework

## Status

- **Active edge-wear architecture:** EW-C — Explicit Single-Segment Chamfer Kernel
- **Current implementation step:** EW-C2 — Provisional replacement-face and one-strip bevel emission
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


## EW-C2 provisional emission baseline

EW-C2 consumes the validated EW-C1 corner table and solved per-edge widths without recomputing them. It builds a temporary face list containing:

- one replacement polygon for every source face;
- one `ConvexEdgeWear` quad for every active positive-width selected edge;
- no vertex-run corner patches yet.

Every allowed provisional opening is registered before topology audit. The only valid open edges are:

- the solved descendants of preserved source-boundary edges;
- endpoint edges of active bevel strips that are reserved for EW-C3 vertex patches.

The provisional topology must contain no unexpected openings, no missing expected vertex-patch boundaries, no non-manifold edges, and no T-junctions. The face list is discarded after audit and the original source mass remains the rendered geometry.

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

1. Validate EW-C2 provisional replacement-face and one-strip emission.
2. Require all provisional openings to match explicit source-boundary or vertex-patch provenance.
3. Implement EW-C3 crude vertex-run patches only after the provisional topology audit reports ready.
4. Commit visible chamfer geometry only after EW-C3 preserves the source boundary set with zero new openings, non-manifold edges, or T-junctions.
