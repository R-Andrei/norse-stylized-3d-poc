# Generated Mass Edge-Wear Recovery Architecture

## Status

EW-C is the active recovery architecture. EW-B is rejected and removed from active code. EW-C0 topology readiness and EW-C1 solved-corner/rail readiness are validated. EW-C2S6R1 keeps the validated positive-width network, shared-span face construction, guarded segmentation, exact face-local retrace normalization, incidence-based internal closure, and explicit ordered source-edge descendant records, then removes strictly guarded zero-boundary retraces from each ordered source-boundary loop before ownership audit. Geometry remains provisional and uncommitted.

The target is a **crude, single-segment physical chamfer**, not a general-purpose smooth bevel modifier.

## Scope restrictions

The first commit-capable kernel supports:

- selected convex internal manifold edges only;
- one bevel segment;
- one solved width per source edge;
- replacement source faces;
- one bevel quad per selected edge;
- centre-fan closure for a closed selected corner loop;
- source-vertex-apex fan closure for an open selected run;
- explicit preservation of source boundary loops.

It does not support:

- rounded profiles;
- arbitrary segment counts;
- beveling source boundary edges;
- non-manifold input;
- post-hoc hole ownership;
- sampled ribbons or global cut planes.

## Why EW-B was retired

EW-B used the following sequence:

```text
independent face offsets
→ edge bridges
→ collect provisional open edges
→ infer source ownership from coordinates
→ reconstruct cap components
```

That direction was structurally backward. Generated geometry was asked to reveal topology that was already known in the source graph. It produced persistent open edges and T-junctions, and ownership classifications changed when geometry changed.

EW-C defines output topology first and solves positions second.

## Temporary topology representation

EW-C uses a small build-time half-edge layer. It is not a full Blender-style modelling system.

Each directed half-edge stores:

```text
origin vertex
destination vertex
source face
source undirected edge
next
previous
opposite
selected flag
```

This is sufficient to trace:

- source boundary loops;
- ordered face fans around a source vertex;
- contiguous selected-edge runs;
- explicit output corner ownership.

## Source boundary policy

Source boundary edges are traced before chamfer geometry. The first kernel never selects them for beveling.

A valid output preserves the same source-boundary identity. The final audit checks for additional boundaries rather than demanding zero total open edges.

## Single-edge geometry

For selected source edge `A → B` shared by faces `F0` and `F1`:

```text
d = normalize(B - A)
```

For face normal `n_f`, the in-face perpendicular is:

```text
m_f = normalize(n_f × d)
```

Its sign is corrected so it points into the source polygon.

For solved width `w`, the face-offset support line is:

```text
L_f(t) = A + w*m_f + t*d
```

The replacement rail on each incident face is obtained from the two replacement face corners at `A` and `B`.

The bevel strip is:

```text
A on F0
B on F0
B on F1
A on F1
```

This is one quad, later triangulated into two triangles if required.

## Face-corner solver

At source face corner `V`, the face is bounded by previous and next source edges.

- neither selected: preserve `V`;
- one selected: intersect its offset line with the unselected source-edge line;
- both selected: intersect the two offset lines.

The result is one explicit replacement point for `(face, source vertex)`.

No generated point is assigned later by nearest-distance ownership.

## Vertex-run closure

Incident half-edges are ordered around each source vertex. Selected edges are partitioned into contiguous runs.

### Closed selected loop

When the selected boundary segments form a closed loop around the source vertex:

```text
centre = average(ordered boundary points)
```

Emit a triangle fan from the centre. If concavity later invalidates a centre fan, replace this local operation with projected ear clipping without changing the architecture.

### Open selected run

When selected edges are bounded by unselected edges or a preserved source boundary, use the original source vertex as the crude apex and emit a fan over the ordered run.

### Multiple selected runs

Emit one independent crude patch for each contiguous selected run.

## Width feasibility

The first solver uses a conservative local bound:

```text
w_max(V) = 0.25 * min(adjacent source-edge lengths at V)
```

For selected edge `e=(u,v)`:

```text
w_e = min(w_requested, w_max(u), w_max(v))
```

A later refinement may solve less conservative geometric limits. The topology architecture does not depend on that refinement.

## EW-C0 readiness audit

EW-C0 emits no chamfer geometry. It proves that the source is suitable for the next stage by reporting:

- source graph validity;
- selected candidate mapping;
- directed half-edge count;
- source boundary-loop traceability;
- selected boundary/non-manifold exclusions;
- affected open and closed vertex fans.

Readiness blockers are:

- invalid source faces or edges;
- missing or mismatched selected graph edges;
- source non-manifold edges;
- source T-junctions;
- selected boundary or non-manifold edges;
- untraceable boundary topology;
- disconnected affected vertex fans.


## EW-C1 explicit corner solver

EW-C1 produces a build-time corner table keyed by `(graph face index, source vertex index)`. The table is authoritative for future replacement faces and selected-edge strips.

For two coplanar face lines

```text
p + t r
q + s k
```

with face normal `n`, the intersection parameter is:

```text
t = dot(cross(q - p, k), n) / dot(cross(r, k), n)
```

A near-zero denominator is an explicit corner-solve failure; EW-C1 does not guess ownership from nearby generated coordinates.

One width is solved per selected source edge. The proof-stage initial limit is the minimum of the requested width and 25% of every adjacent source-edge length at both endpoints and both incident faces. The same width is used at both endpoints.

Acute face corners can create a long miter even when the raw perpendicular offset is small. EW-C1R3 therefore performs up to twelve monotonic global passes. For a corner with displacement `d` and permitted displacement `d_max`, every selected edge participating in that corner is reduced by:

```text
scale = 0.95 * d_max / d
```

The reduced value is written back to the source-edge width, so both endpoints use the same width. The same pass also tests every unselected internal edge. If its two incident replacement faces retain no stable common interval, the solver gathers the selected edges controlling the four endpoint corners and uses a bounded binary search to find the largest stable common scale. The solve fails explicitly if either corner displacement or shared-edge overlap cannot be satisfied at the minimum stable width, or if the pass budget does not converge.

Unselected internal edges are reconciled in source-edge parameter space. Both incident faces receive the exact same endpoint `Vector3` values from the common rail interval. This is explicit source-edge ownership, not a generated-hole bridge.

EW-C1 validates:

- complete corner count;
- finite coordinates;
- conservative corner displacement;
- replacement-face area and winding;
- no collapse of previously stable source edges;
- exact/reconciled shared endpoints on unselected internal edges;
- stable span and length for every future selected-edge strip;
- preservation of the source boundary edges.

Geometry emission remains disabled.

## Patch sequence

### EW-C0 — Reconciliation and topology readiness

Completed and validated. EW-B was removed; source boundary loops, directed half-edges, ordered vertex fans, and selected runs are proven for the current source.

### EW-C1R3 — Compatible-edge deferral and face-corner/rail solver

Implemented as a geometry-neutral proof. Solve one stable replacement point per `(face, source vertex)`, propagate acute-corner and shared-edge overlap constraints across complete selected source edges, reconcile unselected shared endpoints after convergence, and validate every future selected-edge strip.

### EW-C2 — Replacement faces and one-strip edge geometry

Emit replacement `Base` faces and one `ConvexEdgeWear` quad per selected internal edge. Corner openings remain intentional and have explicit source provenance.


### EW-C2S — Shared-span face splitting

EW-C2R is rejected. Its duplicate-endpoint compatibility loop suppressed eighteen otherwise usable chamfers and still produced twenty-four unexplained openings. The failure came from forcing complete inactive-edge rails to coincide by sequentially mutating shared face corners.

EW-C2S preserves the EW-C1R3 positive-width network and never mutates solved corners during inactive-edge reconciliation. For each inactive internal source edge, the two incident face intervals are projected onto the source-edge support line and their stable common middle span is stored explicitly.

Each incident replacement face emits an ordered edge chain:

```text
face corner at endpoint A
optional face-specific tail
shared middle-span endpoint A
shared middle-span endpoint B
optional face-specific tail
face corner at endpoint B
```

The opposite face emits the same middle span in reverse orientation, so the middle segment is topologically shared. The terminal tails remain open and are registered to the corresponding source vertex.

The complete EW-C3 input boundary consists of:

- active bevel-strip endpoint segments;
- inactive-edge face-tail segments;
- grouped source-vertex components that must form only open chains or closed loops.

EW-C2S3 was motivated by a 23-topology placed-mass sample: 12 provisional results passed and 11 failed. The failing set contained T-junction-only cases and a separate family with multi-owner boundaries/non-manifold edges. EW-C2S2's splitter fired zero times because it normalized registrations first and required owner evidence at containing-edge endpoints.

EW-C2S3 stores provisional face records for replacement faces and bevel strips, reconstructs every segment with role and source ownership, and performs segmentation on raw boundary registrations. A point owned by source vertex `V` may split an ordinary replacement segment when `V` belongs to that graph face's one-ring, or a bevel segment when `V` is an endpoint of the owning source edge. EW-C2S4 adds a role-specific exception for `PreservedSourceBoundary`: original containing-face one-ring membership is unnecessary when the point is already an actual provisional mesh vertex, carries raw source-vertex provenance, lies strictly inside the stable containing segment, remains outside endpoint tolerance, and is within topology distance tolerance. Split plans are applied to every use of the same topology edge, and matching expected vertex/source-boundary records are split into the same ordered children.

EW-C2S4 was validated across 24 unique placed objects. All 24 reported zero final T-junctions and zero incompatible T-junction records. Thirteen reached the vertex-patch gate. Eight failures shared one deterministic signature: one provisional face traversed the same undirected edge multiple times, commonly as `A -> B -> A` or `A -> B -> A -> B`, causing four or six provisional uses and multiple tail/endpoint registrations. Three additional failures were isolated to the separate preserved-source-boundary descendant contract.

EW-C2S5 treats an exact inverse-edge pair as a zero-boundary excursion. It removes only topology-key-equal `A -> B -> A` walks and consecutive duplicate vertices; it does not remove merely collinear points, enlarge tolerance, move corners, modify widths, or defer additional selected edges. Initial replacement faces publish boundary registrations only after this reduction. A second pass runs after T-junction segmentation over both replacement and bevel face records. Every normalized face must retain at least three vertices, positive stable area, compatible winding, and no repeated undirected topology edge.

The canonical order is now:

```text
source topology
-> width/corner solve
-> immutable shared spans
-> provisional faces and strips
-> initial face-local retrace normalization
-> raw provenance segmentation
-> preserved-boundary subdivision
-> post-segmentation face-local retrace normalization
-> registration/topology reconciliation
-> boundary normalization
-> topology audit
-> vertex patches
-> final commit
```

### EW-C2S5R1 — Two-face internal boundary cancellation

Only after fixed-point segmentation and post-segmentation face-walk normalization does ownership normalization run. Subdividing a preserved source boundary replaces one parent key with an ordered child chain and does not move or seal the boundary. A registered topology edge remains a source-vertex patch boundary only when it has one actual provisional use and one compatible owner. EW-C2S5 validation proved that some clean internal edges have exactly two uses on two distinct face records but carry the same encoded direction; direction is therefore diagnostic, not the definition of openness. EW-C2S5R1 cancels every registered edge with exactly two actual uses on two distinct face records. Opposite directions remain expected; same-direction pairs increment `sameDirectionClosedInternalEdges` without blocking the vertex-patch gate. A zero-use registration not explicitly removed by retrace normalization is stale provenance and fails. Same-face duplicate uses, more than two uses, and unrecognized repeated face-local edges remain hard failures. Intermediate compatible/incompatible pair logs are not emitted. Geometry remains provisional and uncommitted, and success still requires zero T-junctions and zero non-manifold edges.

EW-C2S5R1 validation produced 21 of 24 objects ready for vertex patches. The five ownership-only failures were removed without changing active/deferred edge counts, and all 24 objects retained zero non-manifold edges and zero T-junctions. Three objects remain blocked solely by the source-boundary key contract.

### EW-C2S6 — Explicit source-boundary descendant ownership

The authoritative source-boundary representation is one `ChamferSourceBoundaryRecord` per original boundary half-edge. The record stores source-edge identity, boundary loop and order, original source vertices, solved parent endpoints, and an ordered child chain. The topology-key sets used by provisional segment classification are derived caches, never the ownership source of truth.

When a raw-provenance split point subdivides a boundary child, the record is split in the same geometric parameter order as every provisional face use. `preservedSourceBoundarySplits` counts unique accepted record subdivisions. Parent identity and child order therefore survive repeated fixed-point segmentation passes.

Ownership after subdivision is deterministic:

```text
unsplit source edge:       its single child remains source-boundary-owned
subdivided source edge:    first and last children are source-vertex transitions
                           non-terminal middle children remain source-boundary-owned
```

A terminal transition candidate with exactly two provisional uses on two distinct face records transfers to source-vertex ownership and is excluded from the expected open set. A terminal candidate with exactly one use and no vertex-boundary registration remains source-boundary-owned. Every non-terminal or unsplit expected source-boundary child must likewise have exactly one use and no vertex-boundary ownership overlap. Duplicate child keys, bad terminal incidence, source/vertex ownership overlap, or non-unit open-child incidence fail the gate. Diagnostics identify source edge, loop, boundary order, child index, endpoints, use count, and terminal flags.

### EW-C2S6R1 — Source-boundary loop retrace normalization

EW-C2S6 validation reached 21 of 24 objects ready for vertex patches. The three blocked objects had exact open-edge accounting and zero generic topology failures, but adjacent ordered source-boundary children described the same topology edge in opposite directions. Sequential EW-C2S6 auditing classified the first child as a two-use incidence failure and the inverse child as a duplicate key.

After final edge-use reconstruction and vertex-boundary normalization, EW-C2S6R1 flattens each loop in `(BoundaryOrder, child index)` order and reduces exact adjacent inverse pairs, including the cyclic last/first seam. Cancellation requires exact reversed `VertexKey` endpoints, an identical `TopologyEdgeKey`, exactly two provisional uses, two distinct provisional face records, and no expected vertex-boundary ownership. The reducer repeats only while a guarded pair is removed.

The reducer changes only source-boundary ownership records. It does not move vertices, alter provisional faces, change width solving, defer candidates, modify replacement faces or bevel strips, weaken T-junction or non-manifold audits, emit vertex patches, or enable geometry commit. Invalid loop ordering and inverse pairs that fail any guard increment `sourceBoundaryLoopNormalizationFailures` and remain hard blockers.

### EW-C3 — Crude vertex-run patches

Close each selected run with a centre fan or source-apex fan. Preserve the source boundary set and reject new boundaries, non-manifold edges, or T-junctions.

### EW-C4 — Commit and visual proof

Commit only after topology validation, then prove the one-strip chamfer in final rendering and wire approved controls.

### EW-C5 — Controlled irregularity and material response

Add deterministic variation or an optional second strip only after the single-strip topology is stable.

EW-C1R3 permits local candidate deferral: a selected candidate whose required solved width falls below the useful geometry threshold is assigned width zero and excluded from edge-strip emission. This is not a topology failure; it preserves the source surface while allowing compatible candidates to proceed.

## EW-C2 provisional geometry emission

EW-C2 reuses the authoritative EW-C1 corner table and per-source-edge width table. It does not re-solve corners or widths.

For each source face, EW-C2 emits a temporary replacement polygon in the original winding using the solved `(face, source vertex)` corners. For each active selected internal manifold edge, it emits one quad:

```text
corner(FaceA, VertexA)
corner(FaceA, VertexB)
corner(FaceB, VertexB)
corner(FaceB, VertexA)
```

The quad is oriented against the selected candidate's expected bevel normal and carries `ConvexEdgeWear` feature identity and candidate strength. Deferred zero-width candidates emit no strip and retain source-surface continuity through the reconciled replacement faces.

EW-C2 intentionally omits vertex patches. Construction therefore registers two classes of permitted open topology before auditing the temporary face list:

1. solved descendants of source boundary edges;
2. endpoint edges of active bevel strips, reserved for EW-C3 vertex patches.

Success requires exact set membership, not count-only similarity:

```text
actual provisional open edges
= preserved solved source-boundary edges
  union expected active-strip endpoint edges
```

Additionally:

```text
unexpected provisional open edges = 0
missing expected vertex boundaries = 0
provisional non-manifold edges = 0
provisional T-junctions = 0
```

The provisional geometry is discarded after audit. EW-C3 is the first stage allowed to close the explicit vertex boundaries, and only its complete topology may become commit-capable.

## Next work items

1. Compile and validate EW-C2S6R1 across all 24 placed masses.
2. Confirm raw and normalized descendant counters explain every guarded inverse-pair removal.
3. Confirm `sourceBoundaryLoopNormalizationFailures=0` and all existing source-boundary ownership failures are zero.
4. Require all 24 objects to report exact source-boundary matching and `readyForVertexPatches=1`.
5. Confirm the selected-edge network and emitted replacement/strip counts are unchanged.
6. Begin EW-C3 only after that complete EW-C2 validation passes.
