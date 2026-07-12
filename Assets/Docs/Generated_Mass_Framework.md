# Generated Mass Framework

## Status

- **Active edge-wear architecture:** EW-C — Explicit Single-Segment Chamfer Kernel
- **Validated implementation baseline:** EW-C2S6R3 — full EW-C2 provisional topology gate passed across all 24 physical masses
- **Validated EW-C3A baseline:** EW-C3A4 — component ownership and closure classification passed across all 24 physical masses; six closed-source clusters, two single-loop promotions, and one count-preserving multi-cycle ownership swap are validated
- **Validated EW-C3B1R2 result:** the complete 492-loop/1503-boundary/519-triangle plan was audited; the patch-local area correction produced zero area or winding failures and 17/24 complete topology passes, while eight folded loops rejected every cyclic triangulation of their global ordered boundary
- **Validated EW-C3B1R3 result:** exactly eight source-local cell complexes were audited and all were infeasible; source-vertex fan/bridge reconstruction is rejected by incidence and T-junction evidence
- **Validated EW-C3B1R4 result:** eight folded loops were audited; six closed-source clusters failed authoritative directed-boundary coherence, while both coherent local quads rejected both candidates only at existing-surface intersection
- **Validated EW-C3B1R5 result:** the expected six cluster plans and 41 cluster edges were audited, but eight co-directed two-use provisional edges caused 16 successor failures and decomposed every cluster into open chains; the sliver census also selected four microscopic quads and proved that its cloned audit had not re-entered the real segmentation pipeline
- **Validated EW-C3B1R5R1 result:** all eight co-directed pairs were classified; orientation parity was rejected by nine contradictions, six required face reversals, and six normal failures, while the independent-sector interpretation produced 108 closed loops for 108 existing plans with zero open chains, successor failures, or internal-direction failures. Exactly two failed slivers were audited, two successful microscopic quads were excluded, and both failed slivers resolved cleanly to triangles after the full cloned segmentation path.
- **Validated EW-C3B1R5R2 result:** the sector-bearing masses retained 108 authoritative loops for 108 plans, but the first complete-clone proof rejected validated baseline patches, treated sixteen promoted co-directed occurrences as missing ownership, and globally merged one proven sliver closure
- **Validated EW-C3B1R5R3 result:** the five sector-bearing masses passed the exact `347 legacy-owned + 16 promoted = 363 authoritative` occurrence equation with 108/108 loops, but the exact-key contact classifier rejected 185 of 484 already-successful loops and one reserved sliver still reduced `22 → 21`
- **Validated EW-C3B1R5R4 result:** authoritative boundary contact recovered 61 baseline loops and raised corrected construction to 360 loops while retaining the exact sector proof and both local sliver triangles; 124 baseline loops still show likely real replacement/bevel overlap, six corrected loops have boundary-occurrence mismatches, and one sliver mass remains `22 → 21`
- **Validated EW-C3B1R5R5 result:** overlap ownership and containment, boundary-occurrence causes, and remap-aware sliver component lineage produced the stable 24-mass compact baseline used for the MassGenerator refactor.
- **Validated refactor boundary:** MG-R5 — Production-Candidate and Diagnostic-Harness Separation; all 24 compact audits match MG-R4.
- **Validated MG-R6A result:** `contained=22/0/22/0/22/0`; all 22 contained patches have deterministic owners, no patch can be deleted safely, and every direct omission fails boundary transfer.
- **Validated MG-R6B runtime result:** `containedRepartition=22/0/0/0/0/22/0/0`; all 22 candidates build and conserve area, then fail exact boundary incidence.
- **Validated MG-R6B.1 result:** `containedBoundary=22/0/0/0/0/0/0/22`, `containedBoundarySegments=66/0/0/0/0/44/22/0`, and `containedShadow=22/22/0/14/22/0/22`; all target overlaps are removed, but every candidate creates open and non-manifold edges and 14 create T-junctions.
- **Validated MG-R6B.2 result:** `containedRepair=22/22/0/0/0/0/22/0/0`; all 22 guided residuals still fail boundary incidence.
- **Validated refactor closure:** MG-R6 compiled and preserved the exact 24-mass MG-R6B.2 baseline; the `MG-R` workstream is closed.
- **Active functional step:** EW-K1.1 — Conformal Plane-Cut Completion.
- **Geometry emission:** the legacy replacement/strip/patch chain and the new plane-cut kernel both operate only on temporary clones; final geometry commit remains disabled

## Feature goal

Generated masses need a crude physical chamfer on selected exposed convex source edges. The first production target is deliberately limited:

- one bevel strip per selected manifold edge;
- one new quadrilateral surface, or two triangles, between the two trimmed source faces;
- deterministic boundary-only corner triangulation;
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

Unity validation across all 24 physical masses produced the exact aggregate:

```text
patchClosedSourceClusters=6
patchClosedSourceClusterComponents=16
patchClosedSourceClusterBoundaryRecords=41
patchClosedSourceClusterFailures=0
```

The five formerly blocked closed-source masses passed. Twenty-one of twenty-four physical masses reached `readyForVertexPatchComponents=1`. The remaining three masses contained four source-fan components. Two identical 18-selected masses each formed one connected three-edge completion loop. The 36-selected mass formed two disconnected, individually closed three-edge cycles from one original source-boundary loop. No degree, duplicate-key, use-count, or ownership failure was present.

### EW-C3A3 — Proven boundary promotion and multi-cycle lineage

EW-C3A3 performs one guarded ownership correction and one read-only lineage audit. It still emits no patch face and does not alter the original normalized `ChamferSourceBoundaryRecord.Children`.

For an unresolved source-fan completion, promotion is permitted only when the combined exact graph satisfies all of the following:

```text
one original source-boundary loop
one connected component
one closed cycle
every graph vertex has degree two
no duplicate topology keys
every source and candidate edge has one provisional use
source edges are source-boundary-owned only
candidate edges are vertex-boundary-owned only
every candidate component and edge is consumed exactly once
```

Passing candidate edges move only in derived ownership sets:

```text
remainingVertexPatchBoundaryKeys
→ finalSourceBoundaryKeys
```

The original source-boundary children and provisional geometry remain unchanged. Passing components receive `OpenChainSourceBoundaryCompletionResolved` and reference an ordered `ChamferFinalSourceBoundaryLoop`. They require no patch geometry.

Required promotion counters are:

```text
patchBoundaryCompletionTransfers
patchBoundaryCompletionTransferredComponents
patchBoundaryCompletionTransferredEdges
patchBoundaryCompletionTransferFailures
```

The two validated 18-selected masses are expected to report one transferred component and one transferred edge each, reducing the unresolved physical set from three masses to one.

When one original source-boundary loop resolves into multiple disconnected closed cycles, EW-C3A3 refuses promotion and emits a detailed lineage warning. The audit deterministically orders each derived cycle and reports:

- source-boundary orders and source-edge indices represented by each cycle;
- candidate source vertices, active run count, and active source edges;
- every ordered edge position, provisional use count, and current ownership type;
- cycle winding relative to the ordered original parent boundary;
- whether the cycles partition consecutive source-record ranges;
- whether an R1 retrace or R3 terminal-alias removal connects different cycles;
- every original source-boundary record with raw, post-R1, and post-R3 child counts plus each removal reason.

Required lineage counters are:

```text
patchBoundaryCompletionMultiCycleLoops
patchBoundaryCompletionDerivedCycles
patchBoundaryCompletionCycleLineageFailures
```

Unity validation confirmed the two physical single-loop promotions, zero transfer failures, and 23/24 component readiness. The remaining 36-selected mass produced two exact three-edge cycles. Cycle 1 contained source orders 2 and 3 plus candidate edge 0 and aligned exactly with the original source-boundary normal (`windingDot=1.000000`). Cycle 0 contained source order 1 plus candidate edges 35 and 43 and was nearly orthogonal (`windingDot=0.019933`). The R3 removed alias connected the two cycles but was already proven internally closed, so it cannot be restored as boundary geometry.

### EW-C3A4 — Multi-cycle boundary/patch ownership resolution

EW-C3A4 applies one narrow, count-preserving derived ownership swap only when the validated two-cycle pattern satisfies every strict guard:

```text
exactly one original source-boundary loop
exactly two derived cycles and two candidate components
consecutive source-record partitions
removed R1/R3 lineage connects the cycles
each candidate component belongs to exactly one cycle
every cycle edge has one provisional use
all current derived ownership is disjoint and correct
exactly one cycle aligns >= 0.95 with the original loop normal
the residual cycle has |alignment| <= 0.25
alignment separation is >= 0.50
exactly one candidate edge is promoted
exactly one source child is demoted
source, patch, union, and disjointness counts remain invariant
```

The aligned cycle becomes the derived final source-boundary loop. Its candidate edge moves from `remainingVertexPatchBoundaryEdges` to `finalSourceBoundaryEdges` and its component receives `OpenChainSourceBoundaryMultiCycleResolved`.

The orthogonal cycle becomes a complete residual patch target. Its one surviving source child moves from `finalSourceBoundaryEdges` to `remainingVertexPatchBoundaryEdges`; the full ordered cycle is retained in a `ChamferVertexPatchCluster` through `OrderedCompletionEdges`, and its component receives `OpenChainSourceBoundaryResidualPatchResolved`.

No original expected-ownership set, normalized source child, provisional face, vertex, or topology edge is changed. The validated 36-selected equation remains:

```text
75 provisional open edges
= 3 derived final source-boundary edges
+ 72 remaining vertex-patch boundaries
```

New counters:

```text
patchBoundaryMultiCycleResolutions
patchBoundaryMultiCycleSourceCycles
patchBoundaryMultiCycleResidualPatchCycles
patchBoundaryMultiCyclePromotedEdges
patchBoundaryMultiCycleDemotedEdges
patchBoundaryMultiCycleWindingSelectionFailures
patchBoundaryMultiCycleOwnershipSwapFailures
patchBoundaryMultiCycleCountInvariantFailures
```

Unity validation confirmed the exact A4 result across the physical set:

```text
24 / 24 readyForVertexPatchComponents=1
patchBoundaryMultiCycleResolutions=1
patchBoundaryMultiCycleSourceCycles=1
patchBoundaryMultiCycleResidualPatchCycles=1
patchBoundaryMultiCyclePromotedEdges=1
patchBoundaryMultiCycleDemotedEdges=1
all A4 failure counters=0
```

The 36-selected object retained the count-preserving derived contract `75 = 3 final source-boundary edges + 72 remaining patch-boundary edges`.

### EW-C3B1 / EW-C3B1R1 — Provisional patch emission and final topology audit

EW-C3B may begin only after EW-C3A reports zero unresolved and ordering/provenance failures across all 24 masses.

EW-C3B1 materialized the validated ownership result as a `ChamferVertexPatchPlan` and reconstructed the exact expected physical aggregate: 492 patch loops and 1503 derived patch-boundary edges. Unity validation rejected the arithmetic-mean centre fan on 23 masses, so the centre-fan operation is retired without changing ownership architecture.

EW-C3B1R1 preserved the plan and switched to boundary-only deterministic triangulation. Three-edge loops emit directly; larger loops use expected-normal projection and deterministic ear clipping. Unity validation retained the exact aggregate plan and eliminated the old winding failures, but produced:

```text
 6 / 24 masses: complete provisional patch topology passed
13 / 24 masses: a direct or ear-clipped triangle was above TinyFaceAreaEpsilon but below the unrelated replacement/bevel minimumStableFaceArea
 5 / 24 masses: the ordered non-planar boundary crossed in the expected-normal projection before ear clipping
```

All 24 retained `readyForVertexPatchComponents=1`, `readyForVertexPatches=1`, and `geometryCommit=disabled`. No EW-C2 or EW-C3A ownership regression occurred.

### EW-C3B1R2 — Patch-local area gate and complete feasibility census

EW-C3B1R2 preserves the validated plan and the R1 boundary-only emitter. It makes one narrow acceptance correction:

```text
provisional patch-triangle minimum area = TinyFaceAreaEpsilon
replacement-face and bevel-strip minimum area = unchanged minimumStableFaceArea
```

`TinyFaceAreaEpsilon` is already the final mesh face-retention floor. Using it only for patch triangles removes the false mismatch in which renderable patch triangles were rejected by the larger replacement/bevel stability threshold. Patch-local ordered-loop and triangle normals are computed from their raw Newell/cross-product vectors without inheriting the generic polygon-normal fallback threshold, so the tiny-area gate and normal test use compatible scales.

R2 no longer stops after the first failed loop. Every one of the 492 planned loops is evaluated. A failed loop contributes no provisional faces, increments `patchLoopsFailed`, logs its own evidence, and does not prevent later loops from being tested. The final combined topology audit still runs only when every loop succeeds.

For every loop with four or more boundary positions, R2 also performs a read-only exhaustive cyclic triangulation census. It enumerates all Catalan boundary triangulations and accepts a candidate only when every triangle:

- has three finite, distinct topology vertices;
- has area greater than `TinyFaceAreaEpsilon`;
- has a finite normal with positive alignment to `ExpectedNormal`;
- participates in a cyclic boundary triangulation with `n - 2` triangles and `n - 3` non-crossing combinatorial diagonals.

The census does not emit its selected candidate. It records total candidate and feasible triangulation counts, loops with and without a feasible result, and one deterministic best diagonal set. The best candidate maximizes minimum normalized triangle quality, then minimum normal alignment, then minimum triangle area, with the lexicographically smallest diagonal-index set as the final tie-break.

Expected-normal projection crossing remains a blocker for the active R1 ear clipper, but R2 now reports structured evidence: loop kind, source vertices, ordered 3D positions, expected and ordered normals, alignment, projection extent and epsilon, projected area, maximum out-of-plane distance, normalized non-planarity, first crossing edge pair, crossing type, projected endpoints, and all four orientation values. Crossing types are `Proper`, `EndpointTouch`, or `CollinearOverlap`.

New summary evidence includes:

```text
patchLoopsFailed
patchMaximumBoundaryCount
patchTriangleAreaFailuresTotal
patchSelfIntersectionLoopsTotal
patchTriangulationCandidatesTested
patchFeasibleTriangulations
patchLoopsWithFeasibleTriangulation
patchLoopsWithoutFeasibleTriangulation
```

Open-chain ownership and patch-loop membership remain exactly those proven by EW-C3A. Patch faces remain temporary `ConvexEdgeWear` provisional faces. No vertex is created or moved, and no geometry is committed.

R2 Unity validation completed the physical census:

```text
492 loops attempted
484 loops built
8 loops failed
519 triangles attempted
486 triangles built
482 direct-triangle loops built
2 ear-clipped loops built
0 patch-area failures
0 patch-winding failures
```

The ten non-triangle loops consisted of four local four-edge loops and six closed-source clusters. Two local loops had two feasible cyclic triangulations each and passed the complete final topology audit. The remaining two local loops and all six clusters had zero feasible cyclic triangulations. All eight blockers were proper expected-normal projection crossings. This proves that the validated outer ownership cycle is not always one geometric polygon; folded source-local provenance must be retained.

### EW-C3B1R3 — Source-local patch cell-complex feasibility census

R3 is read-only and runs only for the eight R2 loops with no feasible cyclic triangulation. It does not alter the successful direct-triangle or ear-clipped paths.

Each original `ChamferVertexPatchComponent` is audited as a local fan cell from its source vertex to every ordered component boundary edge. The fan uses a component-local expected normal derived only from that component's represented source faces. Adjacent open components in a cluster are joined at their shared endpoint. When their source vertices differ, R3 plans an endpoint bridge triangle from the shared endpoint to both source vertices; when the source vertices coincide, the matching endpoint spokes close directly.

After component fans and endpoint bridges are planned, R3 audits the source-to-source graph. Edges whose combined existing and planned use count is two are already closed. One-use edges must form degree-two central loops. Each central loop receives a read-only exhaustive triangulation census on the original source-vertex positions; a feasible central fill contributes one additional use to each central boundary edge and two uses to every internal diagonal.

The complete prospective cell complex must prove:

```text
each original patch boundary: existing 1 + planned 1 = 2
each component spoke: combined uses = 2
each endpoint bridge/source edge: combined uses = 2
each central diagonal: planned uses = 2
planned edges overlapping final source-boundary ownership = 0
prospective T-junctions = 0
component-local geometry failures = 0
bridge geometry failures = 0
central graph/triangulation failures = 0
```

R3 reports one structured cell-complex census per audited folded loop, including component boundary counts, source vertices, component fan and bridge triangle counts, central graph and fill counts, every planned edge's existing-plus-planned incidence, local geometry minima, prospective T-junction count, and `feasibleCellComplex`. No prospective face is appended to `provisionalFaceRecords` in R3.

New summary evidence includes:

```text
patchCellComplexesAudited
patchLocalFoldedCellsAudited
patchClusterCellComplexesAudited
patchComponentFanTrianglesPlanned
patchEndpointBridgeTrianglesPlanned
patchCentralLoopsFound
patchCentralTrianglesPlanned
patchCellComplexesFeasible
patchCellComplexesInfeasible
patchCellComponentFailures
patchCellBridgeFailures
patchCellCentralGraphFailures
patchCellIncidenceFailures
patchCellGeometryFailures
```

### EW-C3B1R4 — Directed-manifold boundary triangulation census

R3 validation rejected all eight source-vertex cell complexes. The physical aggregate was:

```text
8 cell complexes audited
0 feasible
49 component-fan triangles planned
16 endpoint bridges planned
5 central source edges
0 central loops
22 component failures
7 bridge failures
1 central-graph failure
29 geometry failures
46 incidence failures
20 prospective T-junctions
```

The decisive evidence was that geometrically valid source-fan cases still produced invalid incidence and T-junctions. The removed original source vertex is not generally part of the trimmed replacement/bevel topology and cannot be reintroduced as a universal patch centre.

R4 instead recovers the unique directed owner of every one-use folded-loop boundary edge from the pre-patch provisional faces. The candidate patch cycle must traverse each edge opposite to the owning face. All cyclic triangulations of the coherent directed boundary are enumerated without expected-normal projection or aggregate-normal rejection.

Each candidate must prove:

```text
outer boundary use: existing 1 + directed patch 1 = 2
internal diagonal use: existing 0 + two opposite patch uses = 2
final source-boundary overlap = 0
triangle area > TinyFaceAreaEpsilon
improper candidate-candidate 3D intersections = 0
improper candidate-existing-face 3D intersections = 0
combined T-junctions = 0
combined non-manifold edges = 0
```

The diagnostic ranks feasible candidates by minimum normalized triangle quality, then lower maximum internal dihedral, lower maximum boundary dihedral, higher minimum area, and lexicographically stable diagonals. It does not emit the selected candidate.

New summary evidence includes:

```text
patchDirectedLoopsAudited
patchDirectedBoundaryEdgesChecked
patchDirectedBoundaryConflicts
patchDirectedCandidatesTested
patchDirectedCandidatesPassingIncidence
patchDirectedCandidatesPassingTriangleIntersection
patchDirectedCandidatesPassingExistingFaceIntersection
patchLoopsWithFeasibleDirectedTriangulation
patchLoopsWithoutFeasibleDirectedTriangulation
patchDirectedTriangleIntersectionFailures
patchDirectedExistingFaceIntersectionFailures
patchDirectedTJunctionFailures
patchDirectedNonManifoldFailures
```

R4 validation produced the decisive split:

```text
8 directed loops audited
49 boundary edges checked
6 directed-boundary conflicts
4 candidates tested across the two coherent local quads
4 passed incidence
4 passed candidate-candidate intersection
0 passed existing-face intersection
0 feasible directed triangulations
```

The six conflicts are the six `ClosedSourceCluster` loops with boundary counts `5, 6, 6, 6, 9, 9`. Their undirected positional cycles are not authoritative oriented boundary components. The two local four-edge loops have coherent ownership, but each contains a sub-`PointMergeDistance` edge and both triangulations overlap the existing replacement/bevel surface. Triangle selection is no longer the active blocker.

### EW-C3B1R5 — Validated half-edge and sliver census result

R5 remained diagnostic-only and preserved the complete R4 baseline:

```text
24 physical masses
17 readyForChamferPatchTopology=1
7 readyForChamferPatchTopology=0
492 patch loops attempted
484 patch loops built
8 patch loops failed
geometryCommit=disabled
```

It audited exactly the expected cluster population:

```text
6 ClosedSourceCluster plans
41 cluster boundary edges
0 missing authoritative edges
```

The first face-sector traversal did not recover closed components. Across the five cluster-bearing masses it found:

```text
8 co-directed two-use provisional edges
16 successor failures
16 positional pinch/end vertices
16 open boundary chains
0 corrected cluster partitions
```

Each co-directed pair has the same undirected `TopologyEdgeKey` but both owning faces traverse it in the same direction. Because R5 linked only opposite-direction twins, those eight pairs remained unlinked and necessarily produced the 16 open-chain endpoints. R5 did not yet distinguish whether each pair is a true internal adjacency with an orientation inconsistency or two coincident but independent boundary sectors.

R5 also audited four sub-`PointMergeDistance` local quads rather than the intended two failed loops. Two entries belonged to already-successful R2 quads. All four chose a deterministic sanitation survivor without representative or face failures, but the cloned topology then ran the generic topology audit without first applying the real provisional T-junction segmentation and boundary-descendant normalization. Its reported post-collapse T-junctions therefore were not authoritative.

### EW-C3B1R5R1 — Co-directed classification and targeted sliver re-audit

R5R1 remains read-only. It adds exact evidence for every co-directed two-use pair:

```text
edge key and high-precision directed uses
face-record and face-corner identity
replacement/bevel kind
source-face and source-edge provenance
face-local previous/next half-edges
stored and calculated normals
source-topology adjacency
radial face-sector relationship
```

Two cloned interpretations are tested independently.

**Orientation-parity hypothesis**

- opposite directed two-use edges impose equal face parity;
- co-directed two-use edges impose opposite face parity;
- a BFS reports parity contradictions;
- required faces are virtually reversed;
- every reversal is checked against the authoritative stored/source-facing normal;
- twins and boundary components are rebuilt;
- all six cluster plans must resolve with no successor, assignment, or internal-direction failure.

**Independent-boundary-sector hypothesis**

- the two co-directed uses remain distinct boundary half-edges;
- traversal preserves exact face-corner sectors;
- every half-edge must be assigned exactly once;
- all cluster edges must decompose into complete, exclusive closed components;
- no ambiguous successor or duplicated component ownership is accepted.

A hypothesis is selected only when exactly one interpretation passes its complete topology gate. If both pass or both fail, the result remains explicitly unresolved.

R5R1 also carries sliver eligibility from the actual live loop path. A local quad is audited only when it has the R4 sliver signature **and** its real R2 emission fails. Already-successful microscopic quads are counted as excluded, not normalized again. Expected physical totals are:

```text
failed sliver loops audited = 2
successful sliver-like loops excluded = 2
```

After a deterministic virtual collapse, cloned replacement/bevel faces now re-enter the same provisional sequence used by the live topology proof:

```text
SegmentRawChamferTJunctions(...)
NormalizeChamferProvisionalFaceWalks(...)
rebuild face/use records
NormalizeChamferVertexBoundaries(...)
NormalizeChamferSourceBoundaryLoops(...)
CollapseChamferSourceBoundaryTerminalTransferAliases(...)
AuditChamferSourceBoundaryOwnership(...)
```

Only then is the post-collapse half-edge component recovered and a possible triangle evaluated. R5R1 still mutates no live face, patch plan, source-boundary ownership, or committed geometry.

R5R1 adds these principal counters:

```text
patchCoDirectedUsePairsAudited
patchCoDirectedSourceAdjacentPairs
patchCoDirectedSameSectorPairs
patchCoDirectedSeparateSectorPairs
patchCoDirectedAmbiguousSectorPairs
patchHalfEdgeParityContradictions
patchHalfEdgeParityFacesReversed
patchHalfEdgeParityNormalFailures
patchHalfEdgeParityClustersResolved
patchHalfEdgeSectorClustersResolved
patchHalfEdgeParityHypothesisAccepted
patchHalfEdgeSectorHypothesisAccepted
patchHalfEdgeHypothesisAmbiguities
patchSliverSuccessfulLoopsExcluded
patchSliverPostSegmentationTJunctions
patchSliverPostSegmentationIncompatibleTJunctions
```

R5R1 validation established:

```text
co-directed pairs audited = 8
source-adjacent pairs = 8
orientation-parity contradictions = 9
faces requiring parity reversal = 6
parity normal failures = 6
parity clusters resolved = 0
independent-sector loops = 108
existing plans on affected masses = 108
sector open chains = 0
sector successor failures = 0
sector internal-direction failures = 0
failed sliver loops audited = 2
successful sliver-like loops excluded = 2
slivers resolved to triangles = 2
post-segmentation incompatible T-junctions = 0
```

The legacy `sectorAccepted=0` result was caused only by requiring the authoritative sector loops to remain exact copies of the known-invalid position-key cluster plans. It is not evidence against the sector topology. The authoritative loop count is preserved one-for-one, but some old plan boundaries must be repartitioned globally.

### EW-C3B1R5R2 — Authoritative sector-loop repartition and corrected full-topology census

R5R2 remains clone-only. For every physical mass it builds a corrected pre-patch snapshot. The two validated sliver masses first apply their sanitation-consistent endpoint collapse and rerun the full provisional segmentation and boundary-normalization sequence. All other masses retain the validated pre-patch replacement/bevel snapshot unchanged.

The corrected snapshot is decomposed into face-sector-aware closed boundary loops. On the five cluster-bearing masses, all current plans are compared against all authoritative sector loops rather than only the six legacy cluster objects. Every authoritative face-corner boundary occurrence is checked against the complete legacy plan set and must have exactly one provenance owner. The census also reports plans contributing to multiple sector loops and sector loops combining multiple plans. Exact plan matches are retained for lineage; non-exact plans are mapped deterministically by maximum occurrence and key overlap. The resulting patch geometry is always constructed from the authoritative half-edge component, never from the old cluster label or its lineage assignment.

For each authoritative loop R5R2:

```text
builds the opposite boundary order from owning face half-edges
constructs a diagnostic patch loop
triangulates with the patch-local TinyFaceAreaEpsilon policy
checks candidate-candidate and candidate-existing-face intersections
appends triangles only to a cloned topology
checks source-boundary preservation and unexpected openings
runs the final generic non-manifold and T-junction audit
```

R5R2 reports both the occurrence-level sector ownership and the final position-key topology. This distinction is important: two independent face sectors may share an identical geometric edge key. R5R2 must not hide a resulting generic non-manifold count; such a result would prove that the production representation needs sector-distinct vertex identity before commitment.

Principal R5R2 counters include:

```text
patchSectorMassesAudited
patchSectorExistingPlanLoops
patchSectorAuthoritativeLoops
patchSectorExactPlanMatches
patchSectorRepartitionedPlanLoops
patchSectorBoundaryHalfEdgesAssigned
patchSectorLoopCountInvariantFailures
patchSectorOwnershipInvariantFailures
patchSectorProvenanceFailures
patchSectorPlansAttempted
patchSectorPlansBuilt
patchSectorPlansFailed
patchCorrectedMassesAudited
patchCorrectedLoopsAttempted
patchCorrectedLoopsBuilt
patchCorrectedLoopsFailed
patchCorrectedTrianglesAttempted
patchCorrectedTrianglesBuilt
patchCorrectedSliverCollapses
patchCorrectedFinalUnexpectedOpenEdges
patchCorrectedFinalNonManifoldEdges
patchCorrectedFinalTJunctions
readyForCorrectedChamferPatchTopology
```

The live 484 successful patch faces, live plan, and final geometry remain unchanged.

R5R2 Unity validation compiled and ran deterministically but did not satisfy the corrected-topology gate. Across 24 physical masses it attempted 492 authoritative loops, found 491 global components, built 286, and rejected 205. The failures were dominated by 196 candidate intersection reports. The same gate rejected many already-successful baseline triangle patches, so those intersection results are not authoritative without attribution and a baseline control. The five cluster-bearing masses still preserved 108 authoritative loops for 108 plans. Their 363 authoritative occurrences decomposed into 347 legacy-owned occurrences plus exactly sixteen occurrences exposed by the eight co-directed pairs. Those sixteen are promoted sector boundaries, not missing ownership. Both sanitation collapses executed, but one sliver was globally retraced into a malformed seven-edge neighbour rather than preserving the locally proven triangle.

### EW-C3B1R5R3 — Intersection calibration, promoted sectors, and reserved slivers

R5R3 remains clone-only and adds a canonical baseline control. Every live-successful R2 patch loop is passed through the same attributed intersection test before reconstructed sector loops are judged. Candidate hits are classified as internal, accepted-patch, replacement-face, or bevel-strip. Existing faces are tested both with the former vertex-zero fan and with deterministic polygon-aware triangulation. Fan-only hits are evidence about the old diagnostic representation and do not reject a candidate; polygon-aware improper hits remain blockers.

Sector occurrence ownership now recognizes the exact two face-corner uses of every proven co-directed pair as `PromotedCoDirectedSectorBoundary` occurrences. They remain separate face sectors and require no invented legacy patch owner. The expected cluster-bearing aggregate is 347 legacy-owned plus sixteen promoted occurrences, with zero unexplained or multiply owned occurrences.

The two R5R1-proven slivers are reserved before global tracing. Their sanitation-consistent collapse is applied, their exact three authoritative boundary occurrences are recovered, and one occurrence-oriented triangle is appended only to the corrected clone. Those triangle edges become internal before the remaining sector decomposition runs, preventing the validated local closure from being merged into a neighbouring global component.

Three-edge authoritative components no longer derive orientation from an unsanitized aggregate loop normal. Their ordered patch triangle is built directly opposite the three owning boundary half-edges; area and final normal are measured afterward. Larger loops retain deterministic projected ear clipping.

Principal R5R3 counters include:

```text
patchCorrectedBaselineLoopsAudited
patchCorrectedBaselineLoopsRejected
patchCorrectedBaselineIntersectionFailures
patchCorrectedBaselineCandidateInternalIntersections
patchCorrectedBaselineAcceptedPatchIntersections
patchCorrectedBaselineReplacementFaceIntersections
patchCorrectedBaselineBevelStripIntersections
patchCorrectedBaselineFanOnlyIntersections
patchCorrectedBaselinePolygonAwareIntersections
patchCorrectedCandidateInternalIntersections
patchCorrectedAcceptedPatchIntersections
patchCorrectedReplacementFaceIntersections
patchCorrectedBevelStripIntersections
patchCorrectedFanOnlyFaceIntersections
patchCorrectedPolygonAwareFaceIntersections
patchCorrectedAllowedBoundaryContacts
patchCorrectedPolygonTriangulationFailures
patchSectorLegacyOwnedBoundaryHalfEdges
patchSectorPromotedBoundaryHalfEdges
patchCorrectedReservedSliverLoops
patchCorrectedReservedSliverTriangles
patchCorrectedReservedSliverOccurrenceConflicts
```

R5R3 validation audited all 484 live-successful loops and rejected 185, almost entirely against replacement faces. This disproved exact triangle-key identity as a sufficient legal-contact classifier. The sector proof itself passed exactly, and both local sliver triangles remained valid.

### EW-C3B1R5R4 — Boundary-aware contact and exact sliver reservation

R5R4 retains the R5R3 ownership model but changes the proof gate. Every patch loop now exposes its true outer boundary segments by cancelling internal triangle diagonals. Every sanitized replacement or bevel face exposes its polygon boundary independently of the triangle representation used for intersection testing. A patch-to-face contact is legal only when every detected contact lies on both authoritative boundary sets. Interior penetration and proper coplanar crossings remain failures.

Existing faces are gated against the sanitized vertex-zero fan used by the current rendered `ConvexEdgeWear` path. Deterministic projected ear clipping runs silently as comparison evidence; failure of that comparison is recorded without changing the render-faithful gate. It does not call the verbose vertex-patch triangulator, so a non-simple diagnostic face no longer emits a full failure dump and stack trace.

Reserved slivers now reuse the validated R5R1 normalization result directly. The exact three post-collapse patch positions are recovered after sanitation and segmentation, their opposite face half-edges are matched by exact direction, and the reserved triangle is appended before global sector traversal. The expected counts are therefore:

```text
sliver mass A: 20 → 20
sliver mass B: 22 → 22
reserved sliver triangles = 2
reserved occurrence conflicts = 0
```

Detailed intersection evidence is capped to one representative sample total for baseline mode and one for corrected mode per physical evaluation. Normal validation relies on compact per-mass summaries; full triangle positions require the explicitly enabled temporary verbose diagnostic constant.

R5R4 validation recovered 61 baseline loops but retained 124 rejected baseline loops: 121 against replacement faces and four against bevel strips. The corrected clone built 360 of 491 authoritative loops. The remaining blocking contacts were reproduced by the independent polygon representation in 123 of 125 cases, which means they are not explainable as vertex-zero-fan artifacts alone. The sector proof remained exact at 108/108 loops and 363/363 owned occurrences. Both reserved sliver triangles remained valid, but one sliver mass still reported `22 → 21`.

### EW-C3B1R5R5 — Overlap ownership and component lineage

R5R5 remains diagnostic-only. Each rejected baseline loop is assigned exactly one primary overlap class:

```text
patch contained by replacement face
replacement face contained by patch
partial coplanar overlap
non-coplanar replacement penetration
bevel-strip penetration
unclassified
```

Coplanar classifications use deterministic projected triangle-set overlap area. The census also distinguishes overlaps with an authoritative patch-boundary owner from overlaps with unrelated faces. This determines whether the likely production correction is patch elimination/ownership transfer or clipping of excess replacement-face area.

Boundary-occurrence failures are no longer reported as one undifferentiated count. R5R5 records missing opposite edges, duplicate opposite edges, same-direction mismatches, and extra patch boundary edges.

The remaining sliver deficit is audited through a pre-collapse/post-collapse component comparison. The census removes the locally reserved sliver component, applies the validated removed-to-representative vertex remap to the remaining pre-collapse components, then reports exact component matches, disappeared components, post-collapse merges, pre-collapse splits, the remaining loop-count deficit, and a compact deterministic component trace.

Default Console output is one compact no-stacktrace emission summary per physical evaluation. Adjacent identical `OnValidate`/`OnEnable` lifecycle duplicates are suppressed without collapsing separate same-origin physical evaluations. Successful readiness and corner details, per-intersection samples, half-edge details, sector details, and sliver geometry dumps remain behind the explicit verbose diagnostic gate.

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
patch internal diagonals with use count != 2 = 0
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

## MassGenerator source architecture — MG-R1 through MG-R6

`MassGenerator` is implemented as one static partial class across responsibility-focused files. `MassGenerator.cs` contains only orchestration, shared tolerances, and the public `Generate` entry points. Plane-cut construction, polyhedron operations, radial construction, output, geodesic topology, helpers, core types, and edge-wear stages live in separate files under `Assets/Game/Procedural/Masses/`.

MG-R1 is Unity-validated as behaviour-preserving: all 24 compact R5R5 summaries match the pre-refactor baseline exactly, with no public API, geometry, readiness, or commitment change.

MG-R2 and MG-R2R1 are Unity-validated cleanup waves:

- the canonical code inventory is `Generated_Mass_Edge_Wear_Code_Inventory.md`;
- historical detailed logging and per-intersection evidence payloads are removed;
- methods proven unreferenced after diagnostic removal are deleted;
- normal output remains the compact no-stacktrace summary;
- the orphaned classification warning was removed without changing any compact value.

MG-R3 removes three rejected proof families after explicit producer/consumer tracing:

- orientation parity and unused co-directed classification;
- source-vertex cell-complex feasibility;
- historical directed-manifold feasibility.

The independent-sector decision remains authoritative, and directed ownership/triangulation utilities still required by validated sliver recovery remain present. MG-R3/MG-R3R1 are Unity-validated with unchanged 24-mass compact output and zero warnings. MG-R4 removes write-only counter/result state and newly orphaned diagnostics, reducing the source to 22,480 lines across all `MassGenerator` partials; Unity validation confirms all 24 compact audits remain unchanged. Future recovery work must be added to the responsible partial file, and rejected/superseded methods must be removed through separately validated cleanup waves rather than retained indefinitely.

MG-R3R1 is a compile-only correction: one corrected-topology sliver call still passed the old nullable hypothesis argument. It now passes `false`, exactly matching the former `null` path that selected the unresolved/default half-edge decomposition.

MG-R4 reduces `ChamferEmissionStats` from 286 to 95 fields, `ChamferCornerStats` from 60 to 16, and `ChamferReadinessStats` from 28 to 13. It removes three uncalled summary builders, 19 orphaned methods, and two orphaned types. The compact logger and public edge-wear entry point remain text-identical to MG-R3R1; retrace-normalization calls that mutate face walks remain active.

MG-R5 establishes one explicit builder-to-diagnostics boundary. `MassGenerator.EdgeWear.Orchestration.cs` coordinates the production candidate and clone-only harness. `ChamferBuildArtifacts` carries the already-created plan, provisional snapshots, topology context, normalized boundaries, thresholds, and shared spans to `MassGenerator.EdgeWear.Diagnostics.*.cs`. Production-candidate files contain no references to diagnostic-harness methods or diagnostic-only types. The corrected clone, overlap census, lineage analysis, diagnostic types, and compact logging are isolated in explicit diagnostic partials.

MG-R5 removes no algorithms. The separation adds 155 lines of wrapper/file overhead, producing 22,635 lines across all `MassGenerator` partials: 14,186 production/shared edge-wear lines, 570 orchestration lines, 3,840 diagnostic-harness lines, and a 10-line compatibility shim. Unity validation confirmed the same 24 compact audits. Geometry remains provisional and `geometryCommit=disabled`.

MG-R6A adds production-side contained-candidate identification without production-side mutation. Successful patch loops are classified from render-faithful triangle geometry, authoritative boundary segments, and replacement-face provenance. Only `PatchContainedInReplacement` loops are passed through `ChamferBuildArtifacts`; a deterministic owner requires one containing replacement face with a shared authoritative patch-boundary segment.

The clone-only diagnostic removes one candidate patch at a time and tests whether the owner already provides the complete boundary and manifold topology. The compact field is `contained=candidates/resolved/stillRequired/ownerAmbiguous/boundaryTransferFailures/topologyFailures`. Unity validation produced `contained=22/0/22/0/22/0`: all 22 patches remain topologically required, every owner is deterministic, and direct deletion is rejected by boundary transfer rather than owner ambiguity or general topology.

MG-R6B retains each patch and subtracts its region from the deterministic replacement owner only in a diagnostic clone. Owner and patch boundaries are projected into one stable owner-plane basis, split at endpoint contacts and collinear overlaps, and combined as directed boundaries. Shared owner/patch segments cancel; the remaining directed segments form residual owner cycles, which are triangulated deterministically. Original owner vertices and authoritative patch-boundary endpoints are protected so the transform cannot hide a T-junction by deleting required segmentation.

The individual compact field is `containedRepartition=candidates/resolved/arrangementFailures/triangulationFailures/areaFailures/boundaryFailures/topologyFailures/overlapRemaining`. Every resolved candidate conserves area, gives every retained patch-boundary edge two uses, does not worsen baseline-relative topology, and removes the target replacement overlap. `containedCombined=attempted/applied/ownerConflicts/topologyFailures/remainingOverlaps` then tests all individually resolved candidates together, grouped by original owner. Live replacement faces, bevel strips, patch records, rendered geometry, and commitment remain unchanged.

Unity validation produced `containedRepartition=22/0/0/0/0/22/0/0`: all 22 contained candidates reached the exact boundary-incidence gate, but none reached the original topology or overlap gates. MG-R6B.1 therefore keeps construction unchanged and adds `containedBoundary=`, `containedBoundarySegments=`, and `containedShadow=`. Exact edge use is separated from collinear split-equivalent coverage and from real missing, unsplit, underused, overused, or mixed ownership. Shadow checks independently report whether the overlap was removed and whether T-junction, unexpected-open-edge, source-boundary, or non-manifold counts increased. This evidence is clone-only and cannot promote a candidate.

Unity validation of MG-R6B.1 produced `containedBoundary=22/0/0/0/0/0/0/22`, `containedBoundarySegments=66/0/0/0/0/44/22/0`, and `containedShadow=22/22/0/14/22/0/22`. Every candidate has the same three-edge signature: two underused authoritative patch-boundary segments and one overused segment. The target overlap is removed in all 22 cases, but none is topology-clean; every candidate adds unexpected open and non-manifold edges, and 14 add T-junctions. This rejects a validator-only correction and proves that the transformed face complex needs explicit boundary repair.

MG-R6B.2 bundles the two topology corrections supported by that evidence. First, it reconstructs the owning replacement face as an explicit boundary notch: the contiguous patch-boundary run shared with the owner is removed from the owner walk and replaced by the reversed complementary patch path. The existing generic arrangement remains a deterministic fallback. Second, after owner replacement, every affected cloned face is subdivided wherever an authoritative retained-patch endpoint lies in the interior of one of its edges. This aligns the owner, neighboring replacement faces, bevel strips, and other patch records to the same endpoint segmentation without moving vertices or changing area. The compact field is `containedRepair=candidates/guidedResiduals/genericFallbacks/endpointAligned/resolved/buildFailures/boundaryFailures/topologyFailures/overlapRemaining`. Its terminal result reconciles as `candidates = resolved + buildFailures + boundaryFailures + topologyFailures + overlapRemaining`; guided/fallback and endpoint-aligned counts are construction observations. The exact two-use, baseline-relative topology, and overlap-removal gates remain authoritative, and live geometry commitment remains disabled.

Runtime validation produced `containedRepair=22/22/0/0/0/0/22/0/0`. Every candidate uses the guided residual path, no adjacent endpoint insertion is detected, and all 22 still fail exact boundary incidence. The functional experiment is retained unchanged for future topology work.

MG-R6 is the final source-refactor closure. It removes the uncalled private face-material-mask subsystem, its five support types, the uncalled transactional polyhedron-clipping wrapper and three helpers, and one unused vertex-key formatter. The closure removes 858 lines, 30 methods, and six private types without touching edge-wear logic. Before EW-K1, all `MassGenerator` partials total 25,537 lines and edge-wear totals 22,366 lines. EW-K1 adds one focused clone-only kernel partial without reopening the refactor. Static call and type audits find no remaining unreferenced method or private nested type, and production/shared edge-wear code has no diagnostic-only dependency. The historically named MG-R6A through MG-R6B.2 patches remain in the tree as useful functional research, but future topology changes use an `EW-*` prefix.

## Performance policy

Chamfer topology is generated before gameplay and cached with the generated mass. The first implementation prioritizes deterministic, low-complexity geometry:

- one bevel strip per selected edge;
- no subdivision profile;
- no per-frame geometry work;
- no atlas dependency for the physical chamfer.

## Next work items

1. Validate EW-K1.1 compilation with zero errors and zero warnings.
2. Require exactly 24 compact audits with all pre-EW-K1.1 fields unchanged.
3. Require every plane to be accounted for by a surviving cap or verified redundancy.
4. Require zero open edges, non-manifold edges, T-junctions, and invalid faces, with `planeBevel ... /valid=1`.
5. Keep rendered geometry unchanged and `geometryCommit=disabled` until visual validation of the plane-cut clone.


## EW-K1 convex plane-cut resumption

The fastest resumption point is after the validated selected-edge width and corner solve, before independent replacement faces, strips, and corner patches are emitted. EW-K1 uses the existing `ClipPolyhedron` convex half-space operation to cut each selected bevel directly from a deep-cloned source polyhedron. Later cuts trim earlier caps automatically, so corner closure is inherited from the closed convex polyhedron rather than reconstructed through patch ownership.

The existing topology-recovery path is retained unchanged for evidence, but it is no longer the preferred production candidate. EW-K1 proved the architecture with 498 accepted planes and emitted caps; 17 of 24 masses were immediately valid. EW-K1.1 repairs only final edge conformity, clip-tolerance bounds handling, and later-consumed cap accounting. It reports `planeBevel=selected/active/planesBuilt/planesRejected/capsBuilt/capsMissing/capsRedundant/conformalSplits/open/nonManifold/tJunction/invalid/valid`. Promotion is forbidden until every plane is accounted for by a surviving cap or verified redundancy and every clone is closed, manifold, T-junction-free, finite, volume-valid, and contained within source bounds.
