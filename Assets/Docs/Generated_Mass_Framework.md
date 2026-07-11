# Generated Mass Framework

## Status

- **Active edge-wear architecture:** EW-C — Explicit Single-Segment Chamfer Kernel
- **Validated implementation baseline:** EW-C2S6R3 — full EW-C2 provisional topology gate passed across all 24 physical masses
- **Current implementation step:** EW-C3A2 — Global patch-cluster stitching and source-boundary completion census, implemented and awaiting Unity validation
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
10. Build deterministic source-vertex patch components from the normalized explicit vertex-boundary records.
11. Emit one provisional corner patch per resolved boundary component.
12. Validate exact source-boundary preservation, zero unowned openings, zero non-manifold edges, and zero T-junctions.
13. Triangulate once.

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

## EW-C2S6R2 duplicate source-boundary provenance diagnostics

Manual EW-C2S6R1 validation produced 42 deliberate regeneration triplets and 23 distinct exact emission-summary signatures. Every observed signature except one reached `readyForVertexPatches=1`. The known 18-selected boundary signature now removes two guarded retrace pairs and passes. The remaining 36-selected boundary signature removes one guarded pair, preserves exact open-edge accounting, and retains only `sourceBoundaryDuplicateChildKeyFailures=1`; generic topology, source-boundary incidence, terminal-transfer, T-junction, non-manifold, and expected-open matching counters remain clean.

The residual warning identifies only the second child claiming an already-seen `TopologyEdgeKey` because the EW-C2S6R1 ownership audit stores prior keys in a `HashSet`. EW-C2S6R2 is diagnostic only. Before loop normalization it snapshots every child occurrence in deterministic `(BoundaryLoopIndex, BoundaryOrder, child index)` order. After normalization it builds the same occurrence map again. When a duplicate key is encountered, one compact group warning reports all surviving claimants, their source records and directed endpoints, raw and normalized cyclic positions, same- or inverse-direction relationship, cyclic adjacency and distance, use count, distinct provisional-face count, vertex ownership, terminal-transition status, and predicted current-rule disposition.

The inspector's manual `Regenerate` action also emits the object name and Unity entity ID as a clickable context line. Duplicate counters, blocker conditions, source-boundary normalization, provisional geometry, and final commit behavior are unchanged.

R2 validation captured 48 complete provisional-emission summaries. The only failing signature was the 36-selected source-boundary mass, repeated through `OnValidate()` and `OnEnable()`. Its repeated topology key has exactly two raw and two post-R1 occurrences. Both occurrences are terminal-transition candidates on consecutive source-boundary records in the same loop, they share source vertex `26`, and they describe the same two-use/two-distinct-face internal edge in exact inverse directions. They are non-adjacent in the flattened child walk both before and after R1, so they are not an R1 zero-boundary retrace. Both independently resolve to the current `terminal-transfer` disposition.

## EW-C2S6R3 shared terminal-transfer alias collapse

EW-C2S6R3 adds a second source-boundary ownership normalization after R1 and before final ownership audit. It removes a repeated key from the source-boundary record model only when the group contains exactly two raw and two surviving occurrences, the children are exact inverse terminal-transition candidates on different consecutive records of the same loop, the records share their corresponding source vertex, the children remain non-adjacent in the flattened loop walk, the provisional edge has exactly two uses on two distinct face records, and the key is not expected vertex-boundary-owned.

The pair is removed only from source-boundary ownership records. Its two provisional face uses remain untouched and continue to prove that the edge is internally closed. General non-adjacent inverse cancellation is not introduced. Unexpected inverse terminal groups remain subject to the existing duplicate-key blocker and additionally increment `sourceBoundaryTerminalAliasNormalizationFailures`. Diagnostics report collapsed pairs, removed children, and rejected alias normalization. Geometry remains provisional and final commit remains disabled.

## Validated EW-C2 topology gate

EW-C2S6R3 compiled and the complete scene regeneration produced 48 readiness, 48 corner, and 48 provisional-emission summaries: one `OnValidate()` and one matching `OnEnable()` result for each of the 24 physical Generated Mass objects. Every run reported `readyForChamferKernel=1`, `readyForChamferEmission=1`, `readyForVertexPatches=1`, `geometryEmission=provisional`, and `geometryCommit=disabled`.

Across the full set:

```text
replacementFaceFailures = 0
bevelStripFailures = 0
sourceBoundaryLoopNormalizationFailures = 0
sourceBoundaryTerminalAliasNormalizationFailures = 0
sourceBoundaryTerminalTransferFailures = 0
sourceBoundaryChildIncidenceFailures = 0
sourceBoundaryDuplicateChildKeyFailures = 0
unexpectedProvisionalOpenEdges = 0
missingExpectedVertexBoundaryEdges = 0
provisionalNonManifoldEdges = 0
provisionalTJunctions = 0
tJunctionRecordsIncompatible = 0
vertexBoundaryBranchFailures = 0
vertexBoundaryDuplicateFailures = 0
```

The formerly blocked 36-selected mass retained `candidateSelectedEdges=36`, `activeSelectedEdges=33`, `deferredSelectedEdges=3`, `replacementFacesBuilt=16`, and `bevelStripsBuilt=33`. Its boundary ownership reduced deterministically from seven raw descendants to five after one R1 retrace pair, then to three after one R3 terminal-alias pair. It finished with `expectedSourceBoundaryEdges=3`, `matchedSourceBoundaryEdges=3`, `expectedVertexBoundaryEdges=72`, `matchedVertexBoundaryEdges=72`, `provisionalOpenEdges=75`, and `readyForVertexPatches=1`.

EW-C2 is therefore complete. No further EW-C2 recovery patch is permitted unless EW-C3 exposes a reproducible regression in the validated provisional input.

## EW-C3 source-vertex boundary-component architecture

### Authoritative input

EW-C3 consumes the final `normalizedVertexBoundaries` produced by `NormalizeChamferVertexBoundaries(...)`. Each `ChamferExpectedVertexBoundary` already carries:

```text
SourceVertexIndex
SourceEdgeIndex
FaceIndex
Kind
Start / End
TopologyEdgeKey
```

Patch construction must use these records and their exact `VertexKey` connectivity. It must not rediscover arbitrary holes from provisional geometry.

### Component-driven ownership

The 24-mass validation proves that patch components cannot be assumed to map one-to-one to active selected runs or active affected vertices. Observed valid signatures include:

```text
vertexBoundaryComponents < activeSelectedRuns
vertexBoundaryComponents = activeSelectedRuns
vertexBoundaryComponents > activeSelectedRuns
open vertex-boundary chains on meshes with no source-boundary records
active vertices with no surviving vertex-boundary component after exact cancellation
```

Therefore:

- a source vertex owns zero or more connected normalized boundary components;
- one provisional patch is emitted per resolved component, not blindly per active run;
- active-run counts remain a consistency diagnostic only;
- an active source vertex with zero surviving boundary edges requires no patch and is not an error;
- multiple components at one source vertex are legal only when every component is independently ordered and closure-resolved.

### EW-C3A — Ordered component proof

EW-C3A is audit-only. It must add no patch faces.

For every `SourceVertexIndex`:

1. Group normalized boundary records by source vertex.
2. Build exact endpoint adjacency from `TopologyEdgeKey.First/Second`.
3. Reject duplicate keys, degree greater than two, disconnected edge references, or a component that is neither a simple chain nor a simple cycle.
4. Order each component deterministically:
   - open chain: begin at the lexicographically smaller degree-one `VertexKey`;
   - closed loop: begin at the lexicographically smallest `VertexKey`, then choose the first incident edge by stable provenance tuple `(Kind, SourceEdgeIndex, FaceIndex, Key)`;
   - orient each subsequent record so its start equals the preceding endpoint.
5. Preserve the ordered boundary records and ordered unique positions in a `ChamferVertexPatchComponent` record.
6. Cross-reference source-fan openness, active-run count, incident active edges, source-boundary records, and endpoint-to-source-vertex spoke keys.

EW-C3A must classify every component as one of:

```text
ClosedLoop
OpenChainSourceBoundaryResolved
OpenChainClosedSourceResolved
OpenChainClosedSourceClusterResolved
OpenChainUnresolved
```

`OpenChainSourceBoundaryResolved` requires each degree-one endpoint to map uniquely to a surviving explicit source-boundary child at the same source vertex. `OpenChainClosedSourceResolved` requires both endpoint-to-source-vertex spoke keys to satisfy an exact final-use equation: existing provisional uses plus planned EW-C3 patch-spoke uses must equal two, and the spoke key must not already be owned as an expected vertex or source-boundary edge. Distance and apparent collinearity are never accepted as closure proof. Any open chain lacking one of those proofs remains `OpenChainUnresolved` and blocks patch emission.

Required EW-C3A diagnostics include:

```text
patchSourceVertices
patchBoundaryRecords
patchBoundaryRecordsAssigned
patchBoundaryComponents
patchClosedLoopComponents
patchOpenChainComponents
patchSourceBoundaryResolvedChains
patchClosedSourceResolvedChains
patchUnresolvedOpenChains
patchZeroBoundaryActiveVertices
patchMultipleComponentVertices
patchComponentOrderingFailures
patchComponentProvenanceFailures
readyForVertexPatchComponents
```

The implemented EW-C3A pass runs only after the validated EW-C2 gate has set `readyForVertexPatches=1`. It materializes and audits components without appending provisional faces. EW-C3A failure therefore leaves the validated EW-C2 result intact while reporting `readyForVertexPatchComponents=0` and a compact warning with up to three unresolved or malformed component records.

Unity validation proved the extraction and ordering layer across all 24 physical masses:

```text
patchBoundaryRecords = patchBoundaryRecordsAssigned
patchComponentOrderingFailures = 0
patchComponentProvenanceFailures = 0
```

However, the original closure classifier resolved only 16 of 24 masses. Eight masses retained 20 open components because endpoint-to-source-vertex spokes and literal surviving source-boundary endpoint ownership were not a complete closure model. EW-C2 remained healthy across the entire set.

### EW-C3A1 — Direct closure-edge census

EW-C3A1 is diagnostic-only and adds no faces or ownership mutation. For every open component it defines the direct polygon-closing edge:

```text
direct closure = ordered chain end -> ordered chain start
```

Claims are grouped by the undirected `TopologyEdgeKey`. Counters and detailed warnings are emitted only for groups containing at least one currently unresolved component, while all other claims sharing that key remain visible to the group analysis.

For each unresolved direct-closure key, EW-C3A1 records:

```text
existing provisional uses
existing distinct provisional face records
planned patch uses across all open components
predicted final uses
expected vertex-boundary ownership
expected source-boundary ownership
existing segment roles and directed orientation
claiming source vertices and component indices
current component closure class
```

It classifies three evidence candidates without changing component readiness:

```text
existing complement:
    one existing inverse provisional use + one planned patch use = two

shared patch connector:
    zero existing uses + two inverse planned patch uses = two

source-boundary replacement:
    zero existing uses + one planned patch use = one surviving open edge,
    with two incident explicit source-boundary children matching the chain
    endpoint pair at either their terminal or outer endpoints
```

It also reports overuse, underuse, ownership conflict, and unresolved groups. Source-boundary diagnostics enumerate every incident surviving child, its source edge, loop/order, child endpoints, use count, terminal flags, and endpoint matches. No candidate is accepted as geometry behavior in EW-C3A1; the census exists only to prove the exact connector rule needed by EW-C3B.

Required EW-C3A1 diagnostics are:

```text
patchDirectClosureKeys
patchDirectClosureExistingComplementCandidates
patchDirectClosureSharedPatchCandidates
patchDirectClosureSourceBoundaryCandidates
patchDirectClosureOverused
patchDirectClosureUnderused
patchDirectClosureOwnershipConflicts
patchDirectClosureUnresolved
```

`readyForVertexPatchComponents` remains the unchanged EW-C3A closure result during this diagnostic patch.

Unity validation rejected the direct-closure hypothesis across all 24 physical masses. Sixteen unresolved closed-source components formed six exact endpoint-stitched cycles, while no unresolved direct key qualified as an existing complement, shared patch connector, or source-boundary replacement. The remaining four unresolved components belonged to source-boundary fans and required complete boundary-loop reconstruction rather than a guessed connector.

### EW-C3A2 — Global patch-cluster stitching and source-boundary completion census

EW-C3A2 retains source-local components as provenance arcs but introduces a global patch-cluster layer for unresolved closed-source chains. Candidate arcs are connected only by exact endpoint-key identity. Every connected cluster must satisfy:

```text
at least two local component arcs
every cluster endpoint has degree exactly two
one deterministic closed walk consumes every component exactly once
no repeated vertex-boundary topology key
ordered positions return exactly to the starting endpoint
```

The deterministic cluster walk starts at the lexicographically smallest endpoint key, chooses its first incident component using the existing component provenance comparator, reverses local arc orientation when required, and appends every oriented boundary record continuously. Passing components are classified `OpenChainClosedSourceClusterResolved`; no connector, spoke, face, or vertex is created.

Required cluster counters are:

```text
patchClosedSourceClusters
patchClosedSourceClusterComponents
patchClosedSourceClusterBoundaryRecords
patchClosedSourceClusterFailures
```

The expected validated census is six closed clusters containing the sixteen previously unresolved closed-source components.

EW-C3A2 also adds a diagnostic-only source-boundary completion census for the remaining unresolved open-fan components. For each original source-boundary loop it combines:

```text
surviving explicit source-boundary descendants
+
ordered existing vertex-boundary edges from unresolved source-fan components
```

The combined exact endpoint graph reports source-record count, surviving descendants, candidate components and edges, distinct vertices, vertex degrees, connected components, duplicate topology keys, use-count violations, and ownership conflicts. A candidate completion is considered proven only when the combined graph is one connected closed loop, every vertex has degree two, every edge has exactly one current provisional use, and ownership remains disjoint and explicit. This patch does not transfer ownership.

Required boundary-completion counters are:

```text
patchBoundaryCompletionLoops
patchBoundaryCompletionCandidateComponents
patchBoundaryCompletionCandidateEdges
patchBoundaryCompletionClosedLoops
patchBoundaryCompletionDegreeFailures
patchBoundaryCompletionConnectivityFailures
patchBoundaryCompletionDuplicateFailures
patchBoundaryCompletionOwnershipConflicts
```

`readyForVertexPatchComponents` now accepts exact closed-source clusters but continues to reject unresolved source-fan components until a later approved ownership-transfer patch.

### EW-C3B — Provisional patch emission

EW-C3B may begin only after EW-C3A reports zero unresolved and ordering/provenance failures across all 24 masses.

Closed-loop components use a crude centre fan:

```text
centre = arithmetic mean of the ordered unique boundary positions
triangle[i] = centre, boundary[i], boundary[i+1]
```

The loop orientation is chosen against the normalized sum of incident source-face normals. Every fan triangle must have finite vertices, stable positive area, and compatible winding. Boundary edges receive their second use from the patch; internal centre spokes must have exactly two patch-triangle uses.

Open-chain geometry must use the closure class proven by EW-C3A:

- source-boundary-resolved chains may use a source-vertex apex fan only when the two new radial spokes are installed as explicit ordered source-boundary descendants replacing the consumed terminal ownership at that source vertex;
- closed-source-resolved chains may use only the exact connector topology proven by EW-C3A;
- unresolved chains emit no geometry and remain hard blockers.

Patch faces carry `PolygonFaceFeature.ConvexEdgeWear`. Initial patch strength is the maximum strength of active selected source edges incident to the owning source vertex. No new artistic variation is added in EW-C3.

EW-C3B remains provisional. Replacement faces, bevel strips, and vertex patches are audited together and then discarded.

### EW-C3 topology gate

After provisional patches are added:

```text
actual output open-edge set
= surviving explicit source-boundary descendant set
```

Required conditions:

```text
remaining expected vertex-boundary edges = 0
unowned patch edges = 0
patch boundary edges with use count != 2 = 0
patch internal spokes with use count != 2 = 0
output non-manifold edges = 0
output T-junctions = 0
source-boundary mismatches = 0
patch area failures = 0
patch winding failures = 0
patch duplicate-edge failures = 0
```

No geometry becomes commit-capable until every physical mass passes this full gate with `geometryCommit=disabled`.

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

1. Compile EW-C3A2 in Unity.
2. Regenerate all 24 physical masses and verify the sixteen closed-source arcs become six exact clusters with zero cluster failures.
3. Inspect every source-boundary completion census and prove whether the four remaining components complete their original boundary loops.
4. Define a separate ownership-transfer patch only if every combined boundary loop passes degree, connectivity, use-count, duplicate, and ownership checks.
5. Keep patch-face emission and final geometry commitment disabled.
