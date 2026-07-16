# Generated Mass Edge-Wear Code Inventory

This document describes the current code layout and dependency boundaries only. It is not a progress log.

The sole canonical patch history, methods-tried ledger, validation record, current blocker, and next-step list is:

```text
Docs/Generated_Mass_Feature_Implementation_Checklist.md
```

## Dependency boundary

```text
MassGenerator orchestration
    -> production/shared construction
    -> diagnostic harness

Diagnostic harness
    -> immutable builder artifacts
    -> production/shared utilities

Production/shared construction
    -X-> diagnostic-only results
```

`ChamferBuildArtifacts` remains the builder-result boundary. Diagnostic experiments may read its geometry and provenance, but their results must not control live construction unless a later production-promotion patch explicitly replaces the live path.

## Core MassGenerator files

| File | Responsibility |
|---|---|
| `MassGenerator.cs` | Shared constants and explicit evaluation-mode entry points: production `None`, editor-only `PlaneCutPreview`, editor-only `LegacyDiagnosticAudit`, and editor-only `BoundedSingleEdgePreview`. |
| `MassGenerator.Types.cs` | Core polygon, edge, vertex-key, mesh-output support records, plus source, rejected plane, bounded bevel, and bounded endpoint-cap provenance. |
| `MassGenerator.Helpers.cs` | Shared polygon sanitization, welding, geometry predicates, and utility methods. |
| `MassGenerator.Polyhedron.cs` | Convex half-space clipping, cap construction, and polygon-face maintenance. |
| `MassGenerator.MeshOutput.cs` | Final triangulation and feature-data emission. |

## Edge-wear orchestration and selection

| File | Responsibility |
|---|---|
| `MassGenerator.EdgeWear.Orchestration.cs` | Runs only for an explicit diagnostic mode. Plane-cut preview and legacy reconstruction are mutually exclusive; ordinary production generation never enters this orchestration. |
| `MassGenerator.EdgeWear.Graph.cs` | Source topology graph and generic topology audits. |
| `MassGenerator.EdgeWear.SelectionAndCorners.cs` | Deterministic edge selection, width feasibility, corner positions, and rail solving. |
| `MassGenerator.EdgeWear.Types.cs` | Edge-wear graph, provenance, diagnostic, and builder result records. |

## Evaluation entry-point contract

```text
MassGenerator.Generate(...)
    -> EdgeWearEvaluationMode.None
    -> no edge-wear orchestration or audit output

MassGenerator.GeneratePlaneCutBevelPreview(...)
    -> EdgeWearEvaluationMode.PlaneCutPreview
    -> shared selection/corner preparation
    -> plane-cut kernel only
    -> one plane-cut compact audit

MassGenerator.RunLegacyEdgeWearDiagnosticAudit(...)
    -> EdgeWearEvaluationMode.LegacyDiagnosticAudit
    -> legacy replacement/strip/patch/corrected-clone evidence only
    -> one legacy compact audit
    -> returned mesh data is discarded by GeneratedMass

MassGenerator.GenerateBoundedSingleEdgeBevelPreview(...)
    -> EdgeWearEvaluationMode.BoundedSingleEdgePreview
    -> shared selection and corner solve
    -> one bounded source edge selected by stable ordinal
    -> one bounded bevel polygon, two clipped endpoint-support faces, and zero endpoint caps
    -> one bounded-edge compact audit
```

## Retained legacy construction path

The following files retain the previous replacement-face, strip, patch, boundary-repair, and overlap-investigation implementation. They remain available as comparison evidence while the plane-cut clone is validated, but their diagnostic outputs do not modify rendered geometry.

| File group | Responsibility |
|---|---|
| `MassGenerator.EdgeWear.BoundaryPlanning.cs` | Source-boundary planning and ownership preparation. |
| `MassGenerator.EdgeWear.BoundaryNormalization.cs` | Boundary normalization and segmentation utilities. |
| `MassGenerator.EdgeWear.BoundaryCompletion.cs` | Boundary completion and closure candidates. |
| `MassGenerator.EdgeWear.PatchConstruction.cs` | Replacement, bevel-strip, and vertex-patch construction. |
| `MassGenerator.EdgeWear.SliverAndTriangulation.cs` | Patch triangulation, sliver analysis, and geometry predicates. |
| `MassGenerator.EdgeWear.ContainedOwnership.cs` | Contained-patch owner provenance. |
| `MassGenerator.EdgeWear.Diagnostics.Contained*.cs` | Clone-only contained-owner experiments and topology evidence. |
| `MassGenerator.EdgeWear.Diagnostics.CorrectedClone.cs` | Clone-only corrected-topology evaluation. |
| `MassGenerator.EdgeWear.Diagnostics.Overlap.cs` | Render-faithful overlap classification. |

## Bounded bevel prototype

### `MassGenerator.EdgeWear.BoundedSingleEdge.cs`

| Method | Responsibility |
|---|---|
| `AuditBoundedSingleEdgeBevel` | Selects one eligible source edge by stable ordinal, solves it independently with deterministic width backoff, builds its bounded shell, prepares both result and source-baseline shells through the same numerical pipeline, certifies final generated-face outward winding, records raw/prepared/result bounds and volume evidence, certifies topology and exact triangulation, and returns an editor-only preview soup. |
| `BuildBoundedSingleEdgeEligibleList` | Produces the stable source-edge order from selected internal manifold edges. Eligibility does not depend on the shared multi-edge width solution. |
| `TrySolveBoundedIsolatedSingleEdgeRails` | Starts from the normal per-edge width and tries at most twelve deterministic reductions until four stable isolated rail points are available. |
| `TrySolveBoundedIsolatedRailPoint` | Offsets only the selected edge support line, leaves the endpoint-adjacent support line at zero, projects the accepted solution onto the exact graph-owned target boundary, certifies both analytical face planes, and records the canonical point plus snap distance. |
| `TryBuildBoundedSingleEdgeFaces` | Clones the source shell, clips the two endpoint-support corners and two selected-edge owner faces, then emits one bounded rail quadrilateral with no endpoint-cap polygons. |
| `TryClipBoundedOwnerSourceFace` | Projects one convex owner face and its isolated rail into a local 2D basis, clips the polygon against the rail half-plane while retaining the non-edge side, snaps the two intersections to the rail endpoints, and certifies planarity, simplicity, convexity, and winding. |
| `TryClipBoundedEndpointSupportFaces` | Pairs rails `0/2` and `1/3` by source endpoint, requires one exact support face per pair, and installs both validated support-face replacements. |
| `TryClipBoundedEndpointSupportFace` | Verifies exact graph-face and incident-edge ownership, replaces one original endpoint vertex with the ordered previous/next rail pair, and certifies the clipped support polygon. |
| `TryPrepareBoundedPreviewFaces` | Preserves collinear boundary subdivisions through input validation, weld, conformity, seam repair, and final validation. Convexity uses a temporary duplicate/collinear-simplified loop without altering the audited topology, and failures record exact stage, face, provenance, polygon category, and canonical-subdivision involvement. |
| `TryCreateBoundedFace` | Creates the one bounded bevel polygon with explicit provenance and construction-time preferred orientation. Final shell outwardness is certified separately after preparation. |
| `TryOrientBoundedGeneratedFacesOutward` | Uses the original convex solid centre to reverse and reconstruct an inward `BoundedEdgeBevel`, then requires zero remaining generated-face winding failures. Historical endpoint-cap handling remains inert because EW-B1.6 emits no caps. Original source faces are not reordered. |
| `AuditBoundedSourceFaceChanges` | Requires exactly two modified owners and two modified endpoint-support faces, while separately reporting any unexpected source-surface or boundary-only change. |
| `AuditBoundedRailFidelity` | Measures solved-corner retention and any final bevel-polygon extent beyond the four rail bounds. |
| `AuditBoundedResultConvexity` | Tests every result vertex against every outward result-face half-space and records the worst violation provenance. |
| `AuditBoundedFaceIntersections` | Reuses directed triangle intersection predicates to report improper coplanar overlaps and non-coplanar face intersections. |
| `TryTriangulateBoundedPreviewFaces` | Classifies convexity through the same temporary duplicate/collinear-reduced loop used by preparation, chooses an interior fan centre from that region, and emits one oriented triangle per segment of the unchanged real boundary. Failures record exact face and provenance. |

## Plane-cut kernel

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

| Method | Responsibility |
|---|---|
| `AuditPlaneCutBevelKernel` | Builds and certifies the deterministic clean-band edge-only bevel shell, invokes bounded local junction-star extraction as non-mutating evidence, triangulates the exact audited shell, and returns it for optional editor preview. It does not execute the global junction solver. |
| `TryBuildCleanPlaneCutEdgeOnlyShell` | Replays edge planes from the untouched source shell, prepares and audits each complete state, attributes bevel-band conflicts, and deterministically defers the weaker conflicting edge within a 12-pass budget. |
| `TrySelectPlaneCutEdgeConflictDeferral` | Resolves an attributed victim/foreign edge pair through the stable backtracking priority, or defers the victim for single-edge split/collapse failures. |
| `TryBuildPlaneCutBevelCandidate` | Converts one active selected edge and solved rail into a local half-space cut, shifts it outward to retain unrelated source vertices, and explicitly identifies the one safe-deferral case where that shift prevents meaningful source-edge removal. |
| `TryPreparePlaneCutPreviewFaces` | Performs the one final sanitation pass, then conformity and plane-preserving conservative seam repair, producing the exact polygon shell used by the authoritative audit and preview triangulation. |
| `TriangulatePlaneCutPreviewFaces` | Triangulates those exact convex faces with flat deterministic fans while preserving feature classification. |
| `AuditPlaneCutPreviewTriangleSoup` | Audits the exact preview triangles for degeneracy, winding, welded edge incidence, bounds agreement, and volume agreement before adoption. |
| `ClonePolygonFacesForPlaneCutAudit` | Deep-clones source polygon records so diagnostics cannot mutate rendered geometry. |
| `ConformPlaneCutFaceBoundaries` | Preserves shared collinear segmentation where a final vertex lies inside another face edge. |
| `RepairPlaneCutNumericalSeams` | Projects only mutually unique, opposite, near-identical open-edge pairs onto their incident analytical face planes; rolls back unless displacement stays local, face planarity is preserved, and the exact expected topology improvement occurs. |
| `CollectPlaneCutOpenEdges` | Extracts exact one-use clone edges for conservative seam pairing. |
| `IsPlaneCutCandidateAlreadySatisfied` | Detects a cut already satisfied by earlier cuts using strict candidate-relative half-space tolerance. |
| `IsPlaneCutCandidateRedundant` | Accepts a missing final cap only when every final vertex satisfies the candidate half-space under a tolerance below half the measured source-edge removal. |
| `IsPlaneCutHalfSpaceSatisfied` | Supplies the shared exact convex half-space test used by pre-cut and final redundancy classification. |
| `ResolvePlaneCutRedundancyTolerance` | Derives a numerical tolerance that remains strictly smaller than the candidate source-edge removal. |
| `CountMatchingPlaneCutCaps` | Counts surviving `ConvexEdgeWear` faces on one candidate plane. |
| `AuditPlaneCutFaceQuality` | Certifies final edge-wear face planarity and triangle-normal spread, and rechecks junction-cap compactness, aspect ratio, and polygon complexity. |
| `AuditPlaneCutBandIntegrity` | Measures one-face ownership, axial coverage, and generated-face interactions per retained edge; attributes the first victim/foreign source-edge conflict and acts as an authoritative preview gate from EW-L1.1 onward. |
| `RecordPlaneCutBandConflict` | Captures the first victim edge, foreign edge, responsible vertex, coverage, axial location, and shared span for deterministic conflict resolution and compact evidence. |
| `CountInvalidPlaneCutFaces` | Rejects non-finite, degenerate, or oppositely wound faces. |
| `CalculatePlaneCutPolyhedronVolume` | Supplies retained-volume validation. |
| `ArePlaneCutBoundsContained` | Ensures diagnostic clipping does not expand source bounds beyond clip-consistent tolerance. |

### `MassGenerator.EdgeWear.LocalJunction.cs`

| Method | Responsibility |
|---|---|
| `AuditPlaneCutLocalJunctionStars` | Enumerates every source vertex with at least two retained incident bevel planes and records compact bounded-star/loop evidence. |
| `TryExtractPlaneCutLocalJunctionStar` | Bounds copied final face polygons with edge-perpendicular endpoint cutbacks, enforces incident provenance, and extracts one ordered open-patch boundary loop without modifying the rock. |
| `ClipPlaneCutLocalJunctionPolygon` | Clips one copied face polygon against one local bound without emitting a global cap. |
| `TryOrderPlaneCutLocalJunctionBoundary` | Requires one degree-two connected boundary component and orders it deterministically by quantized vertex keys. |
| `IsPlaneCutLocalJunctionLoopSelfIntersecting` | Projects the ordered loop onto the source-vertex average-normal plane and rejects non-adjacent segment intersections. |

### `MassGenerator.EdgeWear.PlaneCutJunctionSolver.cs` — rejected experimental evidence

The active preview no longer calls the global solve. This file remains compiled for historical evidence plus shared replay, provenance, band-audit, and deterministic edge-priority helpers.

| Method | Responsibility |
|---|---|
| `SolvePlaneCutGlobalJunctionSystem` | Rejected infinite-junction-plane experiment. Retained for evidence; not called by the active preview. |
| `EvaluatePlaneCutJunctionState` | Builds one edge-only state, applies accepted local junction transactions, then performs one authoritative deterministic full replay and exact audit for a complete clean state. |
| `TryFindBestPlaneCutVertexJunction` | Searches direct and adaptive normal/depth candidates by cloning the current accepted state, applies only the proposed junction plane, and selects the best locally certified cap by aspect, compactness, complexity, depth, and stable rank. |
| `TryBuildPlaneCutSystemFaces` | Replays all active edge planes and accepted junction planes in deterministic order on a fresh source clone. |
| `BuildPlaneCutJunctionNormalOptions` | Produces stable incident-bevel, angle-weighted source-face, radial, and blended junction directions. |
| `TryBuildPlaneCutVertexJunctionCandidate` | Builds one local junction-plane trial for a requested normal/depth while retaining unrelated original vertices and limiting removal to the source-vertex neighborhood. |
| `DoesPlaneCutJunctionJoinIncidentBevels` | Requires a proposed cap to contact every incident bevel strip retained by the solver state. |
| `IsPlaneCutJunctionInfluenceLocal` | Rejects a local junction trial when its intersection with any incident bevel extends beyond the endpoint-local axial allowance. |
| `TryMeasurePlaneCutJunctionInfluence` | Projects a junction/bevel intersection onto the original source-edge axis and returns penetration, shared-axis span, and the width/depth-derived local limit. |
| `TryFindSinglePlaneCutCap` | Resolves the unique generated `ConvexEdgeWear` face on a proposed junction plane. |
| `IsStablePlaneCutVertexJunctionCap` | Rejects collapsed, non-local, low-compactness, or over-elongated junction caps and returns compactness plus aspect ratio for deterministic scoring. |
| `ComparePlaneCutBacktrackCandidates` | Orders conflicting edges for deterministic deferment by localization burden, strength, selection score, width, source length, and source-edge index. |

### `MassGenerator.Polyhedron.cs`

| Method | Responsibility |
|---|---|
| `ClipPolyhedron` | Clips every face against one half-space, preserves retained-face provenance, and assigns optional source-edge or source-vertex provenance to the new cap. Optional behavior remains disabled for legacy callers. |
| `ClipPolygon` | Clips one polygon while collecting cap intersections. |
| `ResolveClipIntersection` | Reuses one canonical intersection for both faces sharing an undirected edge when explicitly enabled. |
| `IntersectEdge` | Computes the segment-plane intersection with optional segment clamping. |
| `CreateOrientedFace` | Orders cap vertices and enforces outward winding. |

## Editor preview ownership

| File | Responsibility |
|---|---|
| `GeneratedMass.cs` | Holds independent non-serialized rejected-plane and bounded-edge preview state, cycles the bounded edge ordinal, and exposes the clone-only legacy diagnostic action. Distinct preview mesh suffixes prevent production-state reuse. |
| `Editor/GeneratedMassEditor.cs` | Exposes Previous/Evaluate/Next/Restore controls for the bounded single-edge prototype, retains the whole-rock plane diagnostic as explicitly rejected evidence, and keeps the legacy audit single-object-only. No diagnostic action is invoked by lifecycle restoration. |

## Compact diagnostic ownership

`MassGenerator.EdgeWear.Diagnostics.Logging.cs` owns the compact explicit diagnostic records. EW-B1 emits:

```text
GeneratedMass bounded edge compact audit.
boundedEdge=candidateCount/selectedOrdinal/sourceEdge/isolatedRailSolved/widthAttempts/solvedWidth/targetBoundaries/ownerClips/boundarySubdivisions/bevelFaces/endpointCaps/modifiedSourceFaces/foreignSourceFacesModified/railDeviation/maxExtentBeyondRails/valid
boundedEdgeClass=selected owner faces/normals/normal dot/dihedral/cross-face interior signed distances/solid-centre sidedness/tolerance/classification and full eligible-pool classification counts
boundedOwner=attempted/clipped/intersectionFailure/degenerate/nonPlanar/nonSimple/nonConvex/windingFailure
boundedPrepare=attempted/succeeded/inputFaces/inputVertices/inputUniqueVertices/outputFaces/outputVertices/outputUniqueVertices/welded/conformed/seamPairs/seamTouchedFaces/inputTopology/outputTopology/inputVolume/outputVolume/volumeDelta/volumeRatio/exactFailure
boundedSourcePrepare=the same complete preparation evidence for the untouched source baseline
boundedTopology=open/nonManifold/tJunction/invalidFaces
boundedBounds=raw/prepared validity, tolerance, raw/prepared/result minima and maxima, and per-side containment margins
boundedSolid=source-convexity plane violations and exact result-versus-source-plane containment violations with provenance
boundedVolume=rawSource/preparedSource/result/rawRatio/preparedRatio/sourcePreparationRatio/rawDelta/preparedDelta/minimumRatio/maximumRatio/lowerMargin/upperMargin/valid
boundedLocalVolume=signed source/result volumes plus original/replacement owner, bevel, endpoint-cap, foreign, local/global delta, and residual attribution
boundedCertification=attempted/facesReoriented/outwardWindingFailures
boundedBevelPlane=construction plane/final face normals, agreement, plane distance, solid/source-edge sidedness, and rail residual
boundedVolumeCrossCheck=diagnostic triangulation attempt/validity, signed/absolute triangle volume, and polygon/triangle deltas
boundedMesh=triangles/triangulatedFaces/degenerate/open/nonManifold/winding/bounds/volume/exact triangulation failure
geometryCommit=disabled
```

The retained whole-rock plane diagnostic field is:

```text
planeBevel=
selected/
active/
planesBuilt/
planesLocalized/
planesDeferred/
planesRejected/
capsBuilt/
capsMissing/
capsRedundant/
conformalSplits/
seamPairs/
open/
nonManifold/
tJunction/
invalid/
valid

planeVertexJunction=
candidates/
directBuilt/
adaptiveBuilt/
backtrackBuilt/
cleanSharp/
unresolved/
triangleCaps/
quadCaps/
largerCaps/
edgesDeferred/
rebuildPasses

planeSolve=
states/
junctions/
trials/
rebuilds/
polygonAudits/
triangleAudits/
edgesDeferred/
elapsedMilliseconds/
timedOut

planeFaceQuality=
faces/
seamTouched/
nonPlanar/
elongatedJunction/
maximumPlaneDeviation/
maximumNormalSpread/
minimumJunctionCompactness/
maximumJunctionAspect/
worstVertexCount

planeBand=
retained:<count>,
single:<count>,
split:<count>,
interrupted:<count>,
foreignCut:<count>,
overlongJunction:<count>,
collapsed:<count>,
minCoverage:<ratio>,
maxJunctionInfluence:<ratio>,
maxSharedAxisSpan:<ratio>

edgeConflict=
passes:<count>,
deferred:<count>,
resolved:<0|1>,
budgetExhausted:<0|1>,
victim:<sourceEdge>,
foreign:<sourceEdge|-1>,
vertex:<sourceVertex>,
deferredEdge:<sourceEdge>,
victimCoverage:<ratio>,
foreignAxial:<parameter>,
foreignSpan:<ratio>

localJunction=
candidates:<count>,
extracted:<count>,
closed:<count>,
branched:<count>,
selfX:<count>,
foreign:<count>,
missing:<count>,
duplicate:<count>,
loopVertices:<minimum>-<maximum>,
maxExtent:<ratio>

planeMesh=
triangles/
degenerate/
open/
nonManifold/
winding/
bounds/
volume/
valid
```

During EW-L1.1, `planeVertexJunction` and `planeSolve` remain all zeroes because the rejected global junction solver is not executed. `planeBand` is an authoritative clean-shell gate, `edgeConflict` records deterministic deferral evidence, and `localJunction` reports the final retained endpoint stars.

Representative failure text remains in `planeTrace=` and must stay concise.

## Live-geometry boundary

The production `MassGenerator.Generate` path remains unchanged and every physical audit continues to report:

```text
geometryCommit=disabled
```

In Edit Mode only, `GeneratedMassEditor` can request the audited clone through the non-serialized plane-cut preview control. The active preview contains edge bevel planes only; global vertex-junction planes are not emitted. Locality-incompatible candidates and weaker attributed edge-plane conflicts may be explicitly deferred. Clean-band, polygon, and triangle certification govern preview adoption, while bounded local-junction loop extraction is reported from the final retained-edge set. Preview is disabled in Play Mode and does not serialize a production selector. No diagnostic result may mutate serialized assets, materials, shaders, scenes, prefabs, tags, layers, or components.

## EW-B1.4 bounded source-face certification

`MassGenerator.EdgeWear.BoundedSingleEdge.cs` now distinguishes exact boundary-cycle equality from planar-region equality. Foreign faces that preserve the same region but contain rail subdivisions or narrow seam-repair movement increment `foreignBoundarySubdivided`; only genuine region changes increment `foreignSourceFacesModified` and block preview adoption.

## EW-B1.5 final bounded-shell certification

`MassGenerator.EdgeWear.BoundedSingleEdge.cs` now treats construction-time preferred normals as non-authoritative. After preparation, only generated bevel and endpoint-cap faces are certified against the original solid centre and reconstructed if inward. The compact audit reports reorientation, remaining outward failures, independent bounds and retained-volume values, triangulated-face count, and exact triangulation failure provenance. Triangulation preserves every canonical subdivision while using the same subdivision-safe convexity classification already used by preparation.

## EW-B1.5R1 preparation-equivalent telemetry

`TryPrepareBoundedFaces` is the single numerical preparation implementation for both the bounded shell and the untouched source baseline. `BoundedPreparationAudit` records input/output cardinality, topology, weld/conform/seam repair, volume drift, and exact failure provenance. `AuditBoundedSingleEdgeBevel` volume-certifies the result against the prepared source while preserving raw-source comparisons. `MassGenerator.EdgeWear.Diagnostics.Logging.cs` emits all cumulative fields in one bounded-edge record.

## EW-B1.5R2 classification and attribution telemetry

`AuditBoundedEdgeClassificationPool` and `AuditBoundedSelectedEdgeClassification` classify selected manifold edges without changing eligibility. `AuditBoundedSourceSolidContainment` distinguishes coarse AABB containment from exact half-space containment against every original source plane. `AuditBoundedLocalVolumeAttribution` decomposes the signed volume delta by source/replacement owner faces, bounded bevel, endpoint caps, and foreign faces using one common interior reference point. `AuditBoundedBevelPlaneSidedness` records the construction plane relationship to the solid centre and source edge. Topology-valid shells are triangulated for diagnostic cross-check even when bounds or retained-volume certification rejects preview adoption.


## EW-B1.6 endpoint support clipping and shell certification

`MassGenerator.EdgeWear.BoundedSingleEdge.cs` now builds the bounded single-edge shell without endpoint-cap polygons. `TryClipBoundedEndpointSupportFaces` pairs rails `0/2` at source endpoint A and rails `1/3` at endpoint B. `TryClipBoundedEndpointSupportFace` verifies exact graph-face ownership and incident-edge provenance, removes the original source vertex, inserts the two canonical rail vertices in source-boundary order, and validates the resulting convex support polygon.

`AuditBoundedSourceFaceChanges` distinguishes the two owner modifications, two endpoint-support modifications, and any unexpected source-surface change. `AuditBoundedLocalVolumeAttribution` separately records original and replacement support-face contributions. Historical cap fields remain in telemetry and should report zero.

`AuditBoundedResultConvexity` tests the final shell against every result face half-space. `AuditBoundedFaceIntersections` reuses the existing directed triangle intersection implementation to report improper coplanar overlaps and non-coplanar face intersections. Preview adoption now requires zero result-convexity violations and zero improper face-pair intersections in addition to the existing polygon, topology, bounds, volume, winding, and triangle-soup gates.
Endpoint-support telemetry includes exact source/rail positions, support normals, graph-edge parameters, edge residuals, and support-plane residuals. The retained-volume upper ratio is `1.0`, so a shell that adds any measured material cannot be adopted as a valid bevel preview.


## EW-B1.6R1 prepared source-change and intersection-delta audit

`MassGenerator.EdgeWear.BoundedSingleEdge.cs` now executes `AuditBoundedSourceFaceChanges` twice: raw source versus prepared result is retained as historical numerical evidence, while prepared source versus prepared result drives the exact four-face modification gate. `ApplyBoundedSourceFaceChangeAudits` stores both datasets without deleting the earlier counters.

`AuditBoundedFaceIntersections` now returns provenance-keyed pair evidence rather than mutating one result-only count. It runs over both prepared source and prepared result. `ApplyBoundedFaceIntersectionDelta` partitions pairs into unchanged, changed, new, and resolved sets and counts only introduced improper interior pairs as a hard failure. Every pair record includes transient face indices, provenance, coplanarity, shared vertices, shared boundary edges, source-graph adjacency, and boundary-contact status. `MassGenerator.EdgeWear.Diagnostics.Logging.cs` emits the complete datasets in the existing single bounded-edge record.

No endpoint-support clipping, owner clipping, rail solving, bevel emission, candidate selection, volume threshold, production path, or River behavior changes in EW-B1.6R1.

## EW-B1.6R2 prepared-source provenance certification

`MassGenerator.EdgeWear.BoundedSingleEdge.cs` now creates an attributed raw source clone through `ClonePolygonFacesForPlaneCutAudit(..., assignSourceFaceProvenance:true)` before source-baseline preparation. `TryPrepareBoundedFaces` preserves those identities. Raw source-face changes use the attributed raw clone, while raw bounds, volume, containment, and source-solid authority continue to use the original source shell.

`AuditBoundedSourceFaceProvenance` records the expected and observed source identity sets for attributed raw source, prepared source, and prepared result. It reports total/source/non-source/null face counts, unique valid identities, missing identities, duplicates, out-of-range identities, and first failure indices. `SourceProvenanceCertificationValid` is a hard prerequisite for source-change, intersection-delta, triangulation, and preview validity.

`MassGenerator.EdgeWear.Diagnostics.Logging.cs` emits the cumulative `boundedSourceProvenance` group in the existing one-record bounded audit. No per-face Console messages are added.

## EW-B2 unified all-edge implementation

`MassGenerator.EdgeWear.BoundedAllEdges.cs` owns the experimental shared all-edge reconstruction. It evaluates each selected manifold edge with the isolated rail solver, builds one point cloud from retained source vertices and active rails, extracts supporting hull planes, orders and sanitizes hull facets, classifies source/bevel/junction provenance, and passes the result through the existing bounded preparation and certification pipeline.

`GeneratedMass.cs` and `GeneratedMassEditor.cs` expose one authoritative `Rebuild Edge-Wear Bevel Preview` action. `MassGenerator.EdgeWear.Orchestration.cs` runs retained corner and plane-cut diagnostics, calls the unified bounded evaluator, logs one result, and publishes only a valid unified soup.

## EW-B2.1 hull localization telemetry

`BoundedAllEdgesAuditResult` now stores:

```text
stage / failureStage
pointCloudRank / pointCloudBounds
hullTriplesTested
hullDegenerateTriples
hullSupportingTriples
hullStraddlingTriples
hullPlanesCreated
hullPlanesMerged
hullPlanesBeforePrune
hullPlanesRemovedUnderThreePoints
hullPlaneCount
hullPlanesAttempted
hullFacesCompleted
hullFailurePlaneIndex
hullFailurePlaneNormal
hullFailurePlaneDistance
hullFailurePlanePointCount
hullFailureOrderedVertexCount
hullFailureSanitizedVertexCount
hullFailureFacetArea
hullFailureConvexityValid
hullFailureReason
```

`TryBuildBoundedConvexHullPlanes` populates extraction counters before any return. `TryBuildBoundedHullFaces` records the exact facet stage and plane before returning. `RefreshBoundedAllEdgePlanCounts` prevents early hull failures from reporting a false zero active-edge count.

`MassGenerator.EdgeWear.Diagnostics.Logging.cs` writes one concise Console summary and one complete UTF-8 telemetry file at `Library/GeneratedMassEdgeWearTelemetry.txt`. The file is overwritten per physical evaluation and contains the full retained plane diagnostic, hull points, hull faces, edge plans, and certification evidence. File-write success or exact failure is included in the Console summary.


## EW-B2.2 normalization-safe hull-plane extraction

`MassGenerator.EdgeWear.BoundedAllEdges.cs` extends `BoundedAllEdgesAuditResult` with:

```text
hullNearDegenerateTriples
hullNormalizationRejectedTriples
hullPostNormalizationInvalidTriples
hullPlaneMinimumCrossMagnitude
hullMinimumRejectedCrossMagnitude
hullMaximumRejectedCrossMagnitude
hullMinimumAcceptedCrossMagnitude
hullInvalidPlanesRemoved
hullFirstInvalidPlaneIndex
hullFirstInvalidSeedA/B/C
hullFirstInvalidSeedCrossMagnitude
hullFirstInvalidPlaneReason
hullPlaneEvidence
```

`BoundedHullPlane` now retains `SeedPointA/B/C`, `SeedCrossMagnitude`, and the minimum/maximum merged seed magnitude.

`TryBuildBoundedConvexHullPlanes` explicitly measures and divides the raw normal rather than using implicit Unity normalization. `RecordBoundedHullRejectedCrossMagnitude` preserves the rejected magnitude range. `TryValidateBoundedHullPlaneInvariant` certifies finite unit normal, finite distance, valid support references, plane residuals, and non-degenerate support rank before any facet is ordered. `FormatBoundedHullPlaneEvidence` writes complete retained-plane evidence to the detailed telemetry file.

`MassGenerator.EdgeWear.Diagnostics.Logging.cs` keeps the Console summary bounded while reporting threshold/rejection/invariant counters and writes the full `hullPlanes` dataset to `Library/GeneratedMassEdgeWearTelemetry.txt`.

## EW-B1.7 planar bevel rendering

`MassGenerator.EdgeWear.BoundedSingleEdge.cs` now treats `PolygonFaceProvenanceKind.BoundedEdgeBevel` separately during final triangulation. It emits a direct convex fan anchored at an existing boundary vertex, producing `n - 2` triangles and zero inserted centre vertices. The complete bevel boundary is certified against the authoritative polygon plane before emission. `BoundedSingleEdgeAuditResult` retains region-level surface evidence and is reused by the unified evaluator through `CertificationAudit`.

`MassGenerator.Types.cs` extends `TriangleSoup` with optional authored surface normals and authored surface-group keys. Existing triangle calls remain unchanged and store neither. Bevel-region triangles explicitly store their `PolygonFace.Normal` and one stable polygon group for all emitted vertices.

`MassGenerator.MeshOutput.cs` resolves an authored surface normal before applying the fallback geometric flat normal. It writes one normal for every emitted vertex into `MeshData.Normals`, preserving prior flat normals for ordinary triangles while forcing all triangles of one bevel polygon to share the same plane normal. When a surface-group key exists, surface variation is hashed from that shared key rather than the duplicated triangle-soup vertex index, preventing internal colour/mask seams.

`MassGenerator.EdgeWear.Diagnostics.Logging.cs` adds `boundedBevelRegion` to both single-edge and unified summaries and to the full telemetry file. The record includes polygon faces, boundary vertices, direct triangles, authored-normal triangles, authored surface-group triangles, internal fan vertices, plane residual, geometric-normal deviation, render validity, and exact failure evidence.

`MassGenerator.EdgeWear.BoundedAllEdges.cs` requires the region render contract to pass before a unified preview may report geometry validity. No all-edge hull, suppression, rail, or point-cloud behavior changes in EW-B1.7.

## EW-B3 all-edge preview authority

### `MassGenerator.EdgeWear.Orchestration.cs`

The `applyUnifiedBoundedPreview` branch no longer calls `AuditBoundedAllEdgesBevel`. It runs one shared `AuditExplicitChamferCornerSolution`, calls `AuditPlaneCutBevelKernel` once, logs the authoritative all-edge result, and returns the resulting complete shell. `UnifiedEdgeWearPreviewStatus` now maps:

```text
CandidateCount      = SelectedEdgeCount
RailSolvedEdgeCount = ActiveEdgeCount
ActiveEdgeCount     = PlanesBuilt
DeferredEdgeCount   = PlanesDeferred
RejectedEdgeCount   = PlanesRejected
BevelFaceCount      = BevelRegionFaceCount
TriangleCount       = PreviewTriangleCount
```

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

The edge-only shell now records:

```text
BevelRegionFaceCount
BevelRegionBoundaryVertexCount
BevelRegionTriangleCount
BevelRegionAuthoredNormalTriangleCount
BevelRegionAuthoredSurfaceGroupTriangleCount
BevelRegionInternalFanVertexCount
BevelRegionMaximumPlaneResidual
BevelRegionMaximumNormalDeviationDegrees
BevelRegionRenderValid
MaterializedEdgeCoverageValid
ActiveEdgeEvidence
BuiltEdgeEvidence
DeferredEdgeEvidence
```

The old local `TriangulatePlaneCutPreviewFaces` centre-fan implementation is removed. Plane-shell faces pass through `TryTriangulateBoundedPreviewFaces`, and preview validity requires one rendered bevel surface for every built edge.

### `MassGenerator.EdgeWear.BoundedSingleEdge.cs`

`IsOneSurfaceBevelFace` recognizes both `BoundedEdgeBevel` and `EdgeBevelPlane`. `TryFindStableOneSurfaceFanAnchor` selects an existing boundary vertex whose direct fan preserves every boundary segment without degenerate triangles. Surface-group keys include provenance kind and provenance index so separate bevel polygons cannot accidentally share material variation identity.

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

`LogUnifiedAllEdgeBevelAudit` emits the authoritative one-line summary and rewrites `Library/GeneratedMassEdgeWearTelemetry.txt`. The record places selected/active/built/deferred/rejected coverage and one-surface evidence before secondary topology details. `FormatPlaneCutBevelAuditFields` now includes `planeSurface` and exact active/built/deferred edge sets.

### `GeneratedMassEditor.cs`

The sole button remains `Rebuild Edge-Wear Bevel Preview`. Inspector help now describes the all-edge edge-plane shell rather than the retired point-cloud hull. Success text reports materialized bevels, active selected edges, one-surface face count, triangles, and explicit deferred/rejected warnings.

### `MassGenerator.EdgeWear.BoundedAllEdges.cs`

Historical rejected point-cloud hull implementation. It remains source evidence only and is no longer called by the authoritative inspector action. Do not extend or repair this path unless a future decision explicitly reopens it.

## EW-B3.1 staged failure telemetry additions

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

| Symbol | Responsibility |
|---|---|
| `PlaneCutStageSnapshot` | Stores per-stage face, vertex, topology, invalidity, and face-planarity invariants. |
| `PlaneCutFaceQualityFailureRecord` | Stores exact stable face identity, source edge, measured planarity failure, thresholds, offending geometry, preparation touches, and complete vertex residual evidence. |
| `PlaneCutOpenEdgeFailureRecord` | Stores exact open-edge owner, geometry, source-vertex association, expected neighbour, nearest boundary mismatch, and cause. |
| `PlaneCutJunctionCoverageRecord` | Stores per-source-vertex incident bevel coverage, junction expectation/emission, assigned open edges, and failure reason. |
| `CapturePlaneCutStageSnapshot` | Audits one material polygon stage and records first-open/first-non-planar introduction. |
| `CapturePlaneCutModifiedFaceIdentities` | Attributes boundary-conformity and seam-repair movement to stable face identities. |
| `MeasurePlaneCutFacePlanarityDetailed` | Produces exact residual, normal-spread, offending vertex/segment, area, and minimum-edge evidence. |
| `AuditPlaneCutOpenEdgeFailures` | Builds complete final open-edge dossiers and nearest expected-neighbour evidence. |
| `AuditPlaneCutJunctionCoverage` | Summarizes every touched source vertex and identifies missing/duplicate junction coverage. |

`TryPreparePlaneCutPreviewFaces` now accepts the active audit by reference so sanitation, welding, conformity, and seam repair are captured without changing their geometry behavior. Junction-solver trial preparation passes a local scratch audit and remains semantically unchanged.

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

| Symbol | Responsibility |
|---|---|
| `FormatPlaneCutPrimaryFailure` | Places the exact first failed face or open edge at the start of the Console record. |
| `FormatPlaneCutStageTimeline` | Emits six compact stage snapshots. |
| `FormatCappedPlaneCutFaceFailures` | Emits at most three representative face-quality dossiers in Console. |
| `FormatCappedPlaneCutOpenEdges` | Emits at most four complete open-edge dossiers in Console. |
| `FormatCappedPlaneCutJunctionFailures` | Emits at most three failed junction-coverage records in Console. |
| `BuildPlaneCutDetailedTelemetry` | Writes named, structured failure sections to `Library/GeneratedMassEdgeWearTelemetry.txt` without expanding all successful geometry. |

## EW-B3.2 numerical construction additions

### `MassGenerator.Polyhedron.cs`

| Symbol | Responsibility |
|---|---|
| `ClipPolyhedron(... enforceExactPlaneIntersections, useDistanceWelding, numericalRepairs)` | Enables the strict numerical contract only for an explicitly opted-in caller. |
| `IntersectEdge` exact branch | Distinguishes genuine line-plane crossings from tolerance-only transitions and projects every emitted intersection onto the analytical cut plane. |
| `ProjectPointOntoCutPlane` | Performs the authoritative orthogonal point-to-plane correction. |
| cap residual gate | Reprojects cap points, measures residual before and after projection, and suppresses any cap exceeding `PointMergeDistance * 0.25`. |

### `MassGenerator.Helpers.cs`

| Symbol | Responsibility |
|---|---|
| `WeldSharedVerticesByDistance` | Deterministically chooses the nearest earlier canonical vertex inside `PointMergeDistance`; records comparisons, matches, actual movement, and maximum movement. |

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

| Symbol | Responsibility |
|---|---|
| `PlaneCutNumericalRepairTelemetry` | Cumulative per-evaluation evidence for exact intersections, cap projection/certification, and radius welding. |
| `TryBuildCleanPlaneCutEdgeOnlyShell` | Creates one numerical telemetry record per conflict pass and enables the strict contract for the authoritative shell. |
| `TryPreparePlaneCutPreviewFaces(... numericalRepairs)` | Uses true-distance welding at the explicit `AfterWeld` stage when the authoritative shell supplied telemetry. |

### `MassGenerator.EdgeWear.PlaneCutJunctionSolver.cs`

`TryBuildPlaneCutSystemFaces` accepts optional numerical telemetry. When present, every edge and junction cut uses exact plane intersections and true-distance welding. Existing junction-search and legacy callers that omit the telemetry remain unchanged.

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

`FormatPlaneCutNumericalRepairs` adds a bounded `numerics:{...}` Console block and a `[Numerical Repairs]` detailed-file section. It reports intersection requests, strict crossings, projected tolerance fallbacks, cache reuse, projection count and maximum movement, cap projection/residual/rejection, and distance-weld comparison/match/movement evidence.
## EW-B3.3 strict clipping additions

### `MassGenerator.Polyhedron.cs`

| Symbol | Responsibility |
|---|---|
| `PlaneClipPointClassification` | Separates exact `Inside`, `OnPlane`, and `Outside` states from the broader candidate removal epsilon. |
| `ClipPolygonExact` | Executes strict Sutherland–Hodgman transitions and emits no same-side fallback geometry. |
| `TryResolveExactClipIntersection` | Accepts only genuine strict crossings, validates shared-cache reuse, preserves owner and cut planes, and fails closed on any invariant violation. |
| `TryConstrainPointToOwnerAndCutPlanes` | Computes the closest correction satisfying both analytical planes when raw segment interpolation exceeds strict residual tolerance. |
| `SnapOnPlanePoint` | Canonically snaps only strict on-plane endpoints and records movement plus owner/cut residuals. |
| `RecordExactClipFailure` | Captures the first stable owner/cut provenance, classifications, distances, residuals, and exact failure reason. |
| cap validation path | Validates collected points without global one-plane reprojection and aborts the cut transaction on residual failure. |

Legacy callers continue through `ClipPolygonLegacy`. The authoritative all-edge shell opts into `ClipPolygonExact` through its existing numerical-telemetry argument.

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

`PlaneCutNumericalRepairTelemetry` adds strict classification counts, on-plane snap evidence, prohibited fallback attempts, two-plane correction evidence, owner/cut residual maxima, exact construction failure count, cap validation count, and a first exact-failure dossier.

### `MassGenerator.EdgeWear.PlaneCutJunctionSolver.cs`

`TryBuildPlaneCutSystemFaces` compares the cumulative exact-failure count around each edge or junction cut. A new failure aborts the shell with an explicit strict-clipping blocker; partial faces are never accepted.

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

`FormatPlaneCutNumericalRepairs` extends the bounded Console `numerics` block. `AppendPlaneCutNumericalRepairDossier` writes the complete `[Strict Intersection Contract]` section, and `FormatPlaneCutPrimaryFailure` promotes an exact construction failure when no later face/topology dossier exists.

## EW-B4.1 exhaustive Coverage and lifecycle additions

### `MassGenerator.EdgeWear.Types.cs`

| Symbol | Responsibility |
|---|---|
| `EdgeWearCoverageAudit` | Holds the per-evaluation source-edge population, structural/artistic/selection/materialization counts, stable lookup tables, and lifecycle records. |
| `EdgeWearEdgeLifecycleRecord` | Stores one compact cumulative record for a source edge from eligibility through final plane-shell outcome. |

### `MassGenerator.EdgeWear.SelectionAndCorners.cs`

| Symbol | Responsibility |
|---|---|
| `BuildEdgeWearBevelCandidates(..., out EdgeWearCoverageAudit)` | Builds all source-edge records, separates structural eligibility from artistic preference, and makes maximum Coverage exhaustive. |
| `TryClassifyEdgeWearStructuralEdge` | Applies the established solid-centre/owner-plane convexity classifier before exhaustive selection. |
| `MapEdgeWearCoverageAuditToGraph` | Assigns stable topology graph-edge IDs and selected state to lifecycle records. |
| `ApplyEdgeWearCoverageCornerSolution` | Attributes solved width, width inactivity, and positive-width activity to selected records. |
| `RecalculateEdgeWearCoverageAudit` | Rebuilds aggregate counts from the cumulative record set after each material stage. |

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

| Symbol | Responsibility |
|---|---|
| `PlaneCutBevelAuditResult.CoverageAudit` | Carries complete selection/materialization evidence into Console and file telemetry. |
| `FinalizeEdgeWearCoverageAfterPlaneShell` | Marks retained planes as built and attributes unresolved active edges to explicit deferral. |
| `MarkUnresolvedEdgeWearCoverageAsRejected` | Gives active edges a stable failure reason when the shell transaction fails. |
| `IsEdgeWearCoverageMaterialized` | Enforces the exhaustive maximum-Coverage equality contract while retaining lower-Coverage active-edge semantics. |

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

| Symbol | Responsibility |
|---|---|
| `FormatEdgeWearCoverageSummary` | Emits bounded structural, artistic, selected, width, active, built, deferred, rejected, and unmapped counts. |
| `FormatEdgeWearCoverageIdSummary` | Emits exact source-edge ID sets for each omission category. |
| `AppendEdgeWearCoverageLifecycle` | Writes one detailed but compact line per source edge to `[Edge Lifecycle]`. |

Historical note: the explicit-junction-face coverage heuristic was retired in EW-B4.2R8. It incorrectly treated every multi-bevel source vertex as requiring a separate junction polygon and contradicted certified closed shells.


## EW-B4.2 conflict-cluster width reduction additions

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

| Symbol | Responsibility |
|---|---|
| `PlaneCutConflictWidthReductionRecord` | Stores one bounded conflict retry: stable victim/foreign/source-vertex identities, cluster edge IDs, scale evidence, band metrics, and outcome. |
| `TryBuildCleanPlaneCutEdgeOnlyShell` | Dispatches maximum Coverage to coordinated width reduction and lower Coverage to the historical candidate-deferral policy. |
| `TryBuildCleanPlaneCutEdgeOnlyShellWithWidthReduction` | Rebuilds the complete selected shell, reduces local conflict clusters without removing candidates, and fails explicitly at a geometric floor or bounded pass budget. |
| `BuildScaledPlaneCutCandidates` | Reconstructs one complete pass from immutable original candidates and cumulative per-edge scales. |
| `ScalePlaneCutBevelCandidate` | Moves a bevel plane toward its source edge while preserving normal/provenance and recomputing strict positive removal and clip epsilon. |
| `ResolvePlaneCutCandidateMinimumScale` | Derives the numerical width/removal floor from existing geometry tolerances. |
| `TryBuildPlaneCutConflictCluster` | Expands victim/foreign/offending-vertex evidence into a deterministic local incident-edge cluster. |
| `FinalizeEdgeWearCoverageAfterPlaneShell` | Records final materialized width, width scale, and width-reduced state for every built selected edge. |
| maximum-Coverage preview gate | Publishes triangle soup only when both geometric certification and exhaustive coverage certification pass; a manifold partial shell remains diagnostics-only. |

### `MassGenerator.EdgeWear.Types.cs`

`EdgeWearEdgeLifecycleRecord` now distinguishes solved width from final materialized width and records the final scale plus whether conflict resolution reduced the edge. `EdgeWearCoverageAudit` counts reduced built edges separately.

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

`FormatPlaneCutEdgeConflictAudit` reports conflict mode, pass count, reduction count, minimum final scale, unresolved count, and legacy deferral evidence. `[Conflict Width Reduction]` contains one compact dossier per retry. The Console record now separates `geometryValid` and `coverageValid`, and maximum-Coverage artistic exclusions are labelled `wouldBeArtisticallyFiltered`.

Historical note: the legacy local-junction diagnostic text was removed in EW-B4.2R8. The authoritative local-junction extraction counts and final topology dossiers remain.

## EW-B4.2R1 diagnostic additions

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

| Symbol | Responsibility |
|---|---|
| `PlaneCutTJunctionFailureRecord` | Stores one exact stage-specific T-junction vertex/host/ownership/provenance/scale/conflict dossier. |
| `PlaneCutLocalityDeferralRecord` | Stores the complete solved-versus-localized plane and source-removal evidence for an active edge deferred before shell construction. |
| `PlaneCutDiagnosticSegment` | Retains stable face and segment provenance while reproducing the topology T-junction predicate. |
| `CapturePlaneCutTJunctionFailures` | Re-runs the authoritative T-junction geometry test with the same tolerance and creates exact records without modifying faces. |
| `TryMeasurePlaneCutTJunction` | Measures interior segment parameter, closest point, and residual using the same endpoint exclusions as `AuditEdgeWearTopology`. |
| `ResolvePlaneCutLastConflictForEdges` | Links a T-junction's associated bevel IDs to the latest width-reduction cluster that modified them. |
| `FormatPlaneCutCandidateScaleEvidence` | Reports current materialized candidate widths and scales for associated edges. |
| `TryBuildPlaneCutBevelCandidate` locality dossier | Records the limiting unrelated vertex and solved/localized source-removal values whenever the locality guard defers an edge. |
| `PlaneCutConflictWidthReductionRecord.PreviousScaleEvidence / AppliedScaleEvidence` | Preserves exact per-edge scales for every conflict pass instead of only the cluster minimum. |

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

| Symbol | Responsibility |
|---|---|
| `FormatPlaneCutTJunctionFailure` | Emits compact or complete host-segment and owner-face T-junction evidence. |
| `FormatPlaneCutLocalityDeferral` | Emits compact or complete plane-locality evidence, including edge `0`'s limiting vertex and removal failure. |
| `[T-Junction Failures]` | Contains every stage-specific T-junction dossier in the overwritten telemetry file. |
| `[Locality Deferrals]` | Contains every active edge deferred by the locality guard before shell construction. |
| `FormatPlaneCutPrimaryFailure` | Promotes the exact T-junction record ahead of the generic topology blocker. |

EW-B4.2R1 is diagnostic-only. The stable EW-B4.1 implementation remains preserved separately as the rollback baseline.
## EW-B4.2R2 topology-aware conflict closure additions

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

| Symbol | Responsibility |
|---|---|
| `IsPlaneCutRetryGeometryClean` | Requires zero open, non-manifold, T-junction, invalid-face, and non-planar counts before a width-reduction retry may be accepted. |
| `ClonePlaneCutScaleMap` | Preserves and restores a complete deterministic scale state for topology rollback. |
| `TryBuildPlaneCutTopologyConflictCluster` | Legacy R2 broad-cluster helper retained for historical comparison; EW-B4.2R4 no longer calls it for T-junction recovery. |
| `AddPlaneCutTopologyClusterEdge` | Adds stable edge/vertex membership and cumulative entry reasons to a topology cluster. |
| `FindLatestPlaneCutConflictIntersectingEdges` | Resolves the latest previous reduction that touched any implicated bevel edge. |
| `FormatPlaneCutClusterReasonEvidence` | Produces compact per-edge cluster-entry evidence for the structured telemetry file. |
| `PlaneCutTJunctionFailureRecord.LinkedEdgeIndices` | Retains structured implicated source-edge IDs for solver use instead of reparsing formatted telemetry strings. |
| `PlaneCutConflictWidthReductionRecord` topology fields | Records trigger category, band/topology state, topology counts, rollback application, cluster reasons, and previous/rollback/applied scale evidence. |
| `TryBuildCleanPlaneCutEdgeOnlyShellWithWidthReduction` | Rejects topology-invalid retries, rolls back to the last topology-clean scale state, expands the interaction cluster, and accepts only band-clean plus topology-clean prepared shells. |

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

`FormatPlaneCutEdgeConflictAudit` reports topology rejection, expansion, and rollback totals. `[Conflict Width Reduction]` now records each retry's trigger, band/topology validity, topology counters, rollback, cluster reasons, and scale-state transition.

## EW-B4.2R3 generalized retry and transaction additions

### `MassGenerator.EdgeWear.Types.cs`

| Symbol | Responsibility |
|---|---|
| `EdgeWearCoverageAudit.AttemptedBuiltCount` | Counts selected edges whose bevel planes existed in the latest attempted shell. |
| `EdgeWearCoverageAudit.TrialRejectedCount` | Counts attempted bevels excluded because the solver trial failed certification, not because the source edge was structurally rejected. |
| `EdgeWearEdgeLifecycleRecord.AttemptedBuilt` | Distinguishes attempted plane construction from certified materialization. |
| `EdgeWearEdgeLifecycleRecord.TrialRejected` | Marks an edge belonging to an invalid solver transaction while keeping `Rejected` reserved for structural/construction rejection. |

### `MassGenerator.EdgeWear.SelectionAndCorners.cs`

`RecalculateEdgeWearCoverageAudit` now aggregates attempted, certified, trial-rejected, locality-deferred, and rejected states independently.

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

| Symbol | Responsibility |
|---|---|
| `PlaneCutSolverTransactionState` | Immutable cloned snapshot of one attempted, band-clean, topology-clean, or fully certified pass: candidates, faces, scales, pass identity, and stage invariants. |
| `PlaneCutRetryFailureDossier` | Generalized exact retry evidence for open, non-manifold, T-junction, invalid-face, and non-planar failures plus linked bevel IDs and cluster attribution. |
| `CapturePlaneCutSolverTransactionState` | Clones a retry state so later scale mutations cannot overwrite prior clean evidence. |
| `CapturePlaneCutRetryFailureDossier` | Re-audits the earliest failed-stage faces and stores exact per-category failure records. |
| `ResolvePlaneCutFirstRetryFailureStage` | Selects the earliest material stage across open-edge, T-junction, and non-planar evidence. |
| `CollectPlaneCutRetryLinkedEdges` | Combines face provenance, open-edge neighbours, T-junction provenance, and nearby candidate-plane matches into stable implicated edge IDs. |
| `BuildPlaneCutNonManifoldFailureEvidence` | Emits deterministic non-manifold segment/face provenance and contributes implicated bevel IDs. |
| `BuildPlaneCutInvalidFaceFailureEvidence` | Emits exact invalid-face reasons and stable provenance. |
| `TryBuildPlaneCutGeneralizedFailureCluster` | Maps any generalized retry dossier to implicated bevels, the latest intersecting conflict pass, and an incident source-vertex star. |
| `ApplyPlaneCutRetryFailureToResult` | Copies the latest exact failed-trial topology and face-quality evidence into the primary audit instead of leaving default zeros. |
| `FinalizeEdgeWearCoverageAfterFailedPlaneShell` | Replaces the obsolete unresolved-as-rejected path; records attempted bevels as trial-rejected and preserves locality deferrals and true rejections separately. |
| `FormatPlaneCutCandidateDifferenceEvidence` | Emits the exact attempted-minus-certified source-edge set. |

The removed `MarkUnresolvedEdgeWearCoverageAsRejected` behaviour is obsolete: an invalid solver transaction no longer converts all active source edges into structural rejections.

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

| Symbol | Responsibility |
|---|---|
| `FormatPlaneCutSolverTransactionState` | Emits bounded pass, candidate, scale, and stage evidence for one immutable transaction snapshot. |
| `FormatPlaneCutRetryFailureDossier` | Emits compact Console or complete file evidence for a generalized retry failure. |
| `AppendPlaneCutRetryFailureDossiers` | Writes all exact non-planar-face, open-edge, and T-junction records under `[Retry Failure Dossiers]`. |
| `[Transactional Solver States]` | Records latest attempted, band-clean, topology-clean, and fully certified states independently. |
| coverage/lifecycle formatters | Report `attemptedBuilt`, `certifiedBuilt`, and `trialRejected` separately and list exact trial-rejected edge IDs. |
| `FormatPlaneCutPrimaryFailure` | Promotes exact face/open/T-junction evidence and falls back to the latest generalized retry dossier rather than a misleading generic/default-zero result. |

EW-B4.2R3 changes diagnostic and state semantics only. Width reduction, conflict cluster geometry, clipping, welding, preparation, rendering, and edge `0` locality remain unchanged.

## EW-B4.2R4 minimal topology scale-search additions

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

| Symbol | Responsibility |
|---|---|
| `PlaneCutTopologyScaleTrialRecord` | Retains one factor trial's base pass, exact cluster, factor, rollback/requested/effective scale evidence, floor hits, collateral changes, attempted built count, per-stage evaluation status, all certification counters, and every captured exact failure record. |
| `TryBuildMinimalPlaneCutTopologyCluster` | Derives the topology-recovery cluster only from structured exact T-junction-linked bevel IDs. |
| `TrySearchMinimalPlaneCutTopologyScales` | Restores the immutable topology-clean scale map for every trial, tests bounded rollback-relative factors, rejects collateral changes, and commits only the highest tested fully valid factor. |
| `TryEvaluatePlaneCutTopologyScaleTrial` | Rebuilds from original source faces/candidates and certifies band, caps, topology, face quality, volume, bounds, one-surface rendering, triangulation, and preview mesh validity. |
| `FormatPlaneCutScaleDifferencesOutsideCluster` | Enforces the `collateralChanged={none}` invariant against the complete rollback scale map. |
| `ApplyPlaneCutTopologyTrialAuditToResult` | Promotes only the committed trial's exact geometry and render evidence into the main audit. |
| final certification lifecycle | Keeps a successful solver result in `solver-clean` state; only the outer final certification promotes it to `fully-certified` and populates `certifiedBuilt`. |

The active T-junction path no longer uses the R2 previous-conflict/incident-star expansion. Generalized non-T-junction dossiers remain diagnostic-only and do not trigger another automatic geometry reduction.

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

| Symbol | Responsibility |
|---|---|
| `FormatPlaneCutTopologyScaleSearchAudit` | Adds compact topology-clean base-state, failed-state-scale-reuse invariant, exact cluster, factor, failure-count, collateral, fallback, and unresolved evidence to the Console summary. |
| `FormatPlaneCutTopologyScaleTrial` | Formats one complete factor trial with attempted built count, scale transitions, stage-evaluation status, certification evidence, and all exact captured failures. |
| `AppendPlaneCutTopologyScaleTrials` | Writes the `[Minimal Topology Scale Search]` telemetry section. |

EW-B4.2R4 changes only topology-triggered retry strategy, telemetry, and certification lifecycle accuracy. It does not change artistic selection, plane construction, clipping, welding, seam repair, tolerances, edge `0` locality, or geometry commit.

## EW-B4.2R5 direct foreign band-plane retreat additions

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

| Symbol | Responsibility |
|---|---|
| `TopologyScaleSearchMode` | Identifies the active bounded search strategy without replacing the existing cumulative trial structure. |
| `TopologyScaleSearchTriggerEvidence` | Retains the exact prior band record that justified the retreat target, including source pass, victim, foreign edge, axial parameter, and shared span. |
| `TopologyScaleSearchTopologyLinkedEvidence` | Preserves the exact T-junction-linked bevel set independently from the edges whose scales are adjusted. |
| `TryResolvePlaneCutForeignBandRetreatTarget` | Walks structured prior `band-integrity` reductions newest-first and selects the directly evidenced foreign plane whose victim belongs to the topology-linked set. |
| R5 direct-retreat search path | Originally isolated the directly evidenced foreign edge and tested `{0.95/0.90/0.85/0.80/0.75}` from the immutable topology-clean state. In R6 this responsibility is retained through the shared `TrySearchPlaneCutRetreatScales` implementation. |
| R5 direct-retreat trial certification | Originally enforced complete transaction certification and zero outside-retreat changes. In R6 this responsibility is retained through `TryEvaluatePlaneCutRetreatTrial`. |

The existing `PlaneCutTopologyScaleTrialRecord` remains the cumulative per-trial evidence record. R5 deliberately extends its interpretation rather than creating a competing telemetry family.

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

| Symbol | Responsibility |
|---|---|
| `FormatPlaneCutTopologyScaleSearchAudit` | Adds search mode, direct trigger evidence, unchanged topology-linked edges, and exact retreat edges to the compact Console summary. |
| `FormatPlaneCutTopologyScaleTrial` | Preserves the complete factor-trial record while labelling the adjusted set as `retreatEdges`. |
| `AppendPlaneCutTopologyScaleTrials` | Writes search mode, trigger, topology-linked evidence, retreat set, and all unchanged detailed trial dossiers. |
| `[Direct Foreign Band-Plane Retreat Search]` | Replaces the R4 section title for the active experiment while preserving the same detailed telemetry structure and all prior failure categories. |

EW-B4.2R5 changes only the topology-triggered retry target selection, bounded factor sequence, telemetry labels, and canonical documentation. It does not change selection, clipping, welding, preparation, tolerances, rendering, edge `0` locality, inspector workflow, or production geometry commit.



## EW-B4.2R6 dual-endpoint search and edge-index overlay additions

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

| Symbol | Responsibility |
|---|---|
| `PlaneCutTopologyScaleTrialRecord.SearchMode` | Distinguishes direct foreign-plane and dual-endpoint trials while retaining one cumulative schema. |
| `PlaneCutTopologyScaleTrialRecord.BandVictimEdgeIndex` / `BandForeignEdgeIndex` | Store the structured active band pair from each factor trial. |
| `BandForeignAxialParameter` / `BandForeignSharedSpanRatio` | Preserve the exact location and magnitude of the active foreign-plane incursion. |
| `ProtectedEdgeEvidence` | Records topology-linked edges whose pass-7 scales must remain unchanged in the active search. |
| `TryResolvePlaneCutOpposingBandRetreatTarget` | Finds the second endpoint plane from topology-clean direct-retreat trials with the same victim and a different structured foreign edge. |
| `BuildPlaneCutProtectedEdgeSet` | Computes `topologyLinked - retreatEdges` deterministically. |
| `SetPlaneCutDebugFocusEdges` | Publishes the union of topology-linked and active retreat edges for the editor overlay. |
| `TrySearchPlaneCutRetreatScales` | Shared immutable transaction search for one-edge direct retreat and two-edge dual-endpoint retreat. |
| `TryEvaluatePlaneCutRetreatTrial` | Rebuilds and fully certifies one direct or dual trial while enforcing zero collateral scale changes. |
| `ApplyPlaneCutActiveSearchFailure` | Separates the current terminal search blocker from the historical primary retry failure. |

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

| Symbol | Responsibility |
|---|---|
| `FormatPlaneCutTopologyScaleSearchAudit` | Adds protected edges and active-search failure evidence to the compact Console summary. |
| `FormatPlaneCutTopologyScaleTrial` | Emits search mode, protected set, active band victim/foreign pair, axial parameter, span, and all existing certification evidence. |
| filtered `AppendPlaneCutTopologyScaleTrials` | Writes direct and dual trial records into separate cumulative telemetry sections. |
| `[Dual-Endpoint Foreign-Plane Retreat Search]` | Full factor-by-factor evidence for the current R6 experiment. |

### `MassGenerator.cs`

| Symbol | Responsibility |
|---|---|
| `EdgeWearDebugEdgeRecord` | Editor diagnostic record containing an authoritative source-edge index, transformed endpoints, structural/manifold state, and optional search-focus state. |
| `SourceEdgeIndexDebugStatus` | Carries the independent source-topology graph result and diagnostic without requiring a bevel preview. |
| `GenerateSourceEdgeIndexDebug` | Runs the dedicated source-edge indexing mode and returns transformed source records independently of bevel success or edge-wear settings. |
| debug transform helpers | Apply the same dimensions, deterministic lean, grounding, and recenter operations to graph-edge endpoints without changing production mesh transforms. |
| `UnifiedEdgeWearPreviewStatus.DebugEdges` | Retains search-focus evidence for telemetry highlighting; it is no longer the owner of the complete source-edge overlay. |

### `MassGenerator.EdgeWear.Orchestration.cs`

`BuildUnifiedEdgeWearDebugEdges` snapshots every topology-graph edge and marks the structured current focus set when constructing unified preview status.

### `GeneratedMass.cs`

Stores the complete independently generated source-edge graph in a separate non-serialized `UNITY_EDITOR` cache. `RefreshSourceEdgeIndexDebug` rebuilds that cache directly from the current mass recipe; unified-preview records remain separate and are consulted only for optional search highlighting.

### `MassGenerator.PlaneCut.cs`

`SourceEdgeIndexDebug` captures all source-topology edges immediately after the authored plane cuts and before any bevel evaluation, then continues through the normal unmodified mass triangulation path.

### `MassGenerator.EdgeWear.Orchestration.cs`

`BuildSourceEdgeIndexDebugEdges` builds records directly from `TryBuildEdgeWearTopologyGraph`; it does not require candidates, selected coverage, width solving, corner solving, or shell certification.

### `Editor/GeneratedMassEditor.cs`

| Control or method | Responsibility |
|---|---|
| `Source Edge Index Debug` | Separate inspector section with no ownership under the bevel-preview transaction. |
| `Show All Source Edge Numbers in Scene` | Enables every source graph edge and authoritative index; no focus filter is applied. |
| `Highlight Active Bevel Search Edges` | Optionally colours current structured search IDs while leaving all other edges visible. |
| `Refresh Source Edge Graph` | Rebuilds independent records after recipe or shape changes without running the bevel solver. |
| `RegisterSourceEdgeIndexOverlayRenderer` | Registers the editor-global Scene callback once per domain load. |
| `DrawGlobalSourceEdgeIndexOverlay` | Draws the selected mass's independent records even when no bevel preview has run or the current bevel transaction failed. |
| `SetSourceEdgeIndexOverlayState` | Publishes non-serialized enabled/highlight/target state and requests Scene repaint only when that state changes. |
| `DrawSourceEdgeIndexOverlay` | Draws all transformed source edges and indices depth-tested by default, with optional explicit x-ray mode; search focus changes colour only. |
| source-overlay status evidence | Reports the complete shown/total count and optional search-highlight IDs in both inspector and Scene panel. |

EW-B4.2R6 changes only bounded topology-recovery search, diagnostic telemetry, and editor visualization. Selection, clipping, welding, preparation, tolerances, surface rendering, edge `0` locality, production geometry, and the one-button rebuild action remain unchanged.

## EW-B4.2R7 canonical viability additions

### `MassGenerator.EdgeWear.Types.cs`

| Symbol | Responsibility |
|---|---|
| `EdgeWearViabilityState` | Separates structural exclusion, geometric exclusion, viable-unselected, and viable-selected lifecycle states. |
| `EdgeWearEdgeViabilityRecord` | Cached immutable-source evidence for dihedral, footprint, locality interval, isolated construction, feasible width fraction, endpoint span, and exact failure reason. |
| `EdgeWearCoverageAudit.ViabilityByKey` | Owns one reusable viability record per source `EdgeKey` before graph indexing exists. |
| `EdgeWearCoverageAudit.ViabilityByGraphEdge` | Maps the same cached records to authoritative graph edge IDs without recalculation. |
| viability cache counters | Record cheap locality evaluations, bounded isolated evaluations, later locality-cache uses, and total preflight time. |

### `MassGenerator.EdgeWear.SelectionAndCorners.cs`

| Symbol | Responsibility |
|---|---|
| viability constants | Canonical `15 degree`, `2x footprint`, `25% width`, and `0.5x central-span` thresholds. |
| `BuildEdgeWearBevelCandidates` | Builds structural records, runs cheap generic gates, executes one isolated certificate per survivor, and only then creates the Coverage candidate list. |
| `BuildEdgeWearViabilitySourceVertexList` | Builds the immutable unique source-vertex set once for locality evaluation. |
| `EvaluateIndependentPlaneLocalityViability` | Computes and caches the retain-floor/removal-ceiling interval and limiting source vertex. |
| `RunEdgeWearIsolatedViabilityPreflight` | Runs the existing bounded isolated-edge certificate once per cheap-gate survivor and caches all reusable results. |
| `ResolveEdgeWearIsolatedViabilityFailure` | Maps exact isolated evidence to the canonical generic exclusion taxonomy. |
| `MapEdgeWearCoverageAuditSourceIndices` | Maps lifecycle and viability cache records to authoritative source graph indices. |
| `MapEdgeWearCoverageAuditToGraph` | Applies final viable/unselected/selected lifecycle state without overriding structural or geometric exclusions. |

### `MassGenerator.EdgeWear.BoundedSingleEdge.cs`

The isolated audit now exposes endpoint consumption, remaining central span, and the minimum required central span from the already solved rail set. No second rail solve is performed for viability.

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

`TryBuildPlaneCutBevelCandidate` consumes the cached locality interval. The previous per-candidate scan over every source graph vertex is removed from plane construction and therefore cannot repeat in global solver retries.

Maximum-Coverage certification preserves `GeometricEligibleCount` as individual viability evidence, then compares built edges against the R10 `CoexistenceEligibleCount`.

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

`[Edge Viability Preflight]` writes thresholds, cache counters, elapsed time, every hard-gate result, locality interval, isolated width and endpoint evidence, topology counts, exact diagnostic, and final generic failure reason. Coverage summaries distinguish structural and geometric eligibility.

### `Editor/GeneratedMassEditor.cs` R7 refinement

`X-Ray Hidden Source Edges` switches the independent overlay between depth-tested `LessEqual` drawing and explicit `Always` x-ray drawing. Visible-only mode is the default; the complete 44-edge record set remains cached in both modes.

`AuditExplicitChamferCornerSolution` caps each selected edge's initial width by the cached `MaximumLocallyFeasibleWidth` before shared corner interaction solving. This consumes the isolated preflight result rather than recomputing local width feasibility.

## EW-B4.2R7R1 immutable placement-frame additions

### `MassGenerator.PlaneCut.cs`

| Symbol | Responsibility |
|---|---|
| `BuildPlaneCutMass(..., out TriangleSoup placementReferenceSoup, ...)` | Triangulates the immutable authored source faces before edge-wear evaluation and returns that deterministic source soup as the canonical placement reference. |

### `MassGenerator.cs`

| Symbol | Responsibility |
|---|---|
| `GenerateInternal` placement branch | Applies dimensions to source and output, resolves one placement frame from the immutable source reference, and reuses it for the reconstructed output and debug-edge endpoints. Successful previews no longer derive placement from their own triangle soup. |
| `BuildMassSoup(..., out TriangleSoup placementReferenceSoup, ...)` | Returns the ordinary output itself as the placement reference for non-plane-cut builders and forwards the immutable plane-cut source reference for edge-wear previews. |

### `MassGenerator.MeshOutput.cs`

| Symbol | Responsibility |
|---|---|
| `MassPlacementFrame` | Per-generation cached lean, grounding, and recenter parameters resolved from one reference soup. |
| `ResolveAndApplyMassPlacementFrame` | Resolves each placement stage sequentially from the source reference while applying it to that reference exactly once. |
| `ApplyMassPlacementFrame` | Reuses the completed frame for bevel output and source-edge debug positions without recalculating bounds or contact-centre statistics. |

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

| Symbol | Responsibility |
|---|---|
| `AppendMassPlacementFrameTelemetry` | Appends `[Canonical Placement Frame]` provenance, parameters, reuse counters, and the diagnostic legacy-preview frame delta after unified preview placement is complete. |

## EW-B4.2R8 viability audit-integrity additions

### `MassGenerator.EdgeWear.Types.cs`

| Symbol | Responsibility |
|---|---|
| isolated audit truth fields | Separate `IsolatedLastAttemptedWidth` from `IsolatedMaximumCertifiedWidth` and its certified fraction. |
| `ViabilityLocalityCacheMissCount` | Counts selected-edge construction attempts that lack a complete cached viability record. |
| `ViabilityLocalityRecomputationCount` | Contract counter that must remain zero because solver-time source scans are forbidden. |

### `MassGenerator.EdgeWear.SelectionAndCorners.cs`

`RunEdgeWearIsolatedViabilityPreflight` now records truthful isolated audit semantics. The accepted R7 decision fields remain unchanged; a failed attempt is reported as zero certified width without changing eligibility behavior.

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

| Symbol | Responsibility |
|---|---|
| construction cache guard | Increments cache use only for evaluated locality-valid records and records an explicit miss otherwise. |
| retired explicit-junction audit | Removes `PlaneCutJunctionCoverageRecord` and `AuditPlaneCutJunctionCoverage`; final topology and local-loop extraction remain authoritative. |
| `PrepareEdgeWearStableEvaluationFingerprints` | Hashes ordered exclusion reasons, selected/certified edge IDs, and exact final polygon topology. |

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

| Symbol | Responsibility |
|---|---|
| `FormatEdgeWearViabilityExclusionSummary` | Reports generic exclusion counts and optional exact edge-ID sets. |
| `FormatEdgeWearLocalityCacheContract` | Reports evaluations, construction uses, zero recomputations, unused records, and cache misses. |
| corrected isolated record | Emits success, attempt count, last attempted width, maximum certified width/fraction, and terminal diagnostic. |
| `CapturePendingEdgeWearStableFingerprint` | Bridges the completed plane-shell component hashes to the later placement-frame append. |
| `AppendStableEvaluationFingerprint` | Adds placement and combined evaluation hashes after the canonical frame is available. |

Removed telemetry:

```text
legacyJunctionHeuristic
legacyLocalJunctionDiagnostic
[Legacy Junction Heuristic - Non-Authoritative]
```

No R8 symbol changes bevel geometry, eligibility thresholds, widths, topology tolerances, rendering, or placement.


## EW-B4.2R9 editor-only viability-matrix additions

### `MassGenerator.cs`

| Symbol | Responsibility |
|---|---|
| `EdgeWearBatchAuditCaseResult` | Editor-only immutable-output audit carrier for one non-published matrix evaluation, including viability counts, shell certification, cache contract, placement invariants, timings, and fingerprints. |
| `GenerateUnifiedEdgeWearBatchAuditCase` | Runs the exact unified edge-wear generation path against a cloned recipe/settings pair, captures diagnostics in memory, returns the case result, and never applies the produced `MeshData` to a Unity mesh. |

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

| Symbol | Responsibility |
|---|---|
| `EdgeWearBatchAuditCapture` | Short-lived static capture scope for one synchronous editor evaluation. It is cleared before the next matrix case. |
| `TryBeginEdgeWearBatchAuditCapture` / `CompleteEdgeWearBatchAuditCapture` | Guard non-reentrancy, collect the authoritative plane-shell audit and canonical placement frame, and convert them into the public case result. |
| `PopulateEdgeWearBatchAuditResult` | Extracts generic exclusion counts, certification, topology, face quality, cache counters, timings, component hashes, and exact failure evidence from the authoritative audit. |
| `PopulateEdgeWearBatchPlacementFingerprints` | Reproduces the accepted placement/evaluation fingerprint contracts from the captured canonical frame without writing normal manual telemetry. |
| batch-aware logging guards | Suppress readiness, unified Console, detailed telemetry, and placement append output only while the explicit matrix capture scope is active. Manual rebuild logging remains unchanged. |

### `Editor/GeneratedMassEditor.cs`

| Symbol | Responsibility |
|---|---|
| `EdgeWearBatchShapeSeeds` | Canonical ten-seed stratified matrix. |
| `EdgeWearBatchWidths` / `EdgeWearBatchWidthNames` | Canonical minimum/default/maximum width matrix: `0.05`, `1.0`, `2.0`. |
| `EdgeWearViabilityMatrixJob` | Holds the immutable selected-object snapshot, one cloned-recipe case queue, completed results, and cancellation state. |
| `AdvanceEdgeWearViabilityMatrix` | Runs one case per editor update through the exact authoritative builder. |
| `BuildEdgeWearViabilityMatrixAggregate` | Classifies coverage, topology, face-quality, placement, and cache-contract failures and records exact case coordinates. |
| `WriteEdgeWearViabilityMatrixReports` | Writes one TXT aggregate/detail report and one CSV row set under `Library`. |
| target-state preservation audit | Compares recipe JSON, local Transform, and shared mesh reference before and after the matrix. |

R9 creates no runtime type, serialized state, scene object, mesh publication path, or production geometry behavior.

## EW-B4.2R10 coexistence-viability additions

### `MassGenerator.EdgeWear.Types.cs`

| Symbol | Responsibility |
|---|---|
| `EdgeWearViabilityState.CoexistenceIneligible` | Separates individually valid but jointly incompatible edges from structural/geometric exclusions and solver deferrals. |
| coexistence coverage counters | Preserve geometric and coexistence denominators plus star/pair/trial/exclusion evidence. |
| lifecycle coexistence fields | Store the per-edge eligibility flag and exact generic coexistence reason. |

### `MassGenerator.EdgeWear.SelectionAndCorners.cs`

`RecalculateEdgeWearCoverageAudit` now computes coexistence-eligible and coexistence-ineligible counts. Initial individual viability marks the record coexistence-eligible; later closure may demote it without changing its individual viability evidence.

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

| Symbol | Responsibility |
|---|---|
| `ResolvePlaneCutCandidateMinimumScale` | Enforces the shared `0.25` minimum materialized-width fraction in global candidate scaling. |
| `TryResolvePlaneCutCoexistenceByExclusion` | R10R2 runs the bounded conflict-directed best-first frontier and commits only a completely certified candidate-conserving retained set. |
| `EvaluatePlaneCutCoexistenceExclusionTrial` | Reuses the exact authoritative retreat-trial transaction for one retained candidate set and caches the full outcome. |
| `BuildPlaneCutCoexistenceConflictEdgeSet` | Collects implicated candidates from source-vertex, T-junction, strict plane-pair, retry, and band-conflict evidence. |
| `ApplyPlaneCutCoexistenceSuccess` | Publishes the certified retained faces and converts exact excluded lifecycle records to `CoexistenceIneligible`. |
| coexistence comparison helpers | Tie-break by excluded count, removed width, removed selection score, and stable edge order. |

The current closure is bounded to twelve exclusions, 128 evaluated states, and ten structured implicated candidates per failure. It does not loosen geometry tolerances or encode source-edge IDs.

### `MassGenerator.Polyhedron.cs`

`TryResolveExactPlaneIntersection` now treats an invalid cached intersection as stale evidence rather than an immediate terminal failure. It removes that one cache entry, recomputes the analytical crossing once, applies the existing exact owner/cut two-plane correction when required, and caches the result only after the unchanged strict certificate passes. Numerical telemetry records cache invalidations and successful recomputations.

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

| Symbol | Responsibility |
|---|---|
| `FormatEdgeWearCoexistenceSummary` | Writes denominator transition, generic reason counts/IDs, cached trial counters, excluded edges, and minimum committed width scale. |
| coexistence-aware batch capture | Exposes coexistence eligibility/exclusion counts and trial/cache evidence to the matrix. |
| evaluation fingerprint v2 | Includes the coexistence denominator and coexistence exclusion states. |

### `MassGenerator.cs`

`EdgeWearBatchAuditCaseResult.Passed` now requires selected/certified counts to match `CoexistenceEligibleCount` and rejects any materialized minimum width below `0.25`.

### `Editor/GeneratedMassEditor.cs`

The initial R10 matrix report contract was `EW-B4.2R10`; R10R2 supersedes it with `EW-B4.2R10R2`. TXT/CSV cases report geometric/coexistence denominators, candidate-conservation evidence, conflict-directed search counters, generic coexistence exclusions, and the hard width-floor result.

## EW-B4.2R10R2 conflict-directed closure additions

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

| Symbol | Responsibility |
|---|---|
| `PlaneCutCoexistenceSearchNode` | Carries one immutable search outcome plus the exact reason assigned to every explicit exclusion on that path. |
| `PlaneCutCoexistenceSearchStateRecord` | Compact cumulative state evidence for the canonical telemetry file. |
| `TryResolvePlaneCutCoexistenceByExclusion` | Runs the bounded best-first frontier and commits only the first completely certified state under the canonical exclusion ordering. |
| `PopulatePlaneCutCandidateConservation` | Compares actual trial candidates against the complete root-selected set minus explicit exclusions. |
| `BuildPlaneCutExpectedCoexistenceEdgeSet` | Captures the pre-closure selected/active candidate contract from lifecycle records. |
| `BuildPlaneCutCoexistenceConflictEdgeSet` | Uses structured band victim/foreign, source-vertex, T-junction, retry, strict-intersection, and conservation evidence; it no longer falls back to an arbitrary broad candidate list. |
| `ResolvePlaneCutCoexistenceExclusionReason` | Adds `plane-band-incompatible`, normalizes retry evidence, and routes candidate-conservation mismatches. |
| trial-cache key | Includes explicit exclusions as well as retained edge scales, preventing absent-candidate state aliasing. |
| search telemetry fields | Record state counts, depth, frontier, winning depth, conservation evidence, and per-state dossiers. |

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

| Symbol | Responsibility |
|---|---|
| `AppendPlaneCutCoexistenceSearchStates` | Writes the complete bounded search-state ledger. |
| `FormatEdgeWearCoexistenceSummary` | Adds plane-band, candidate-conservation, and search-frontier evidence. |
| batch capture additions | Expose plane-band/conservation exclusion counts and search counters to the matrix. |

### `MassGenerator.cs`

`EdgeWearBatchAuditCaseResult` now carries plane-band and candidate-conservation exclusion counts plus conflict-directed search counters.

### `Editor/GeneratedMassEditor.cs`

The R10R2 matrix contract was `EW-B4.2R10R2`. TXT/CSV case rows introduced the new exclusion and search fields. Aggregate failure classification recognizes retry T-junction evidence, terminal plane-band splits, and candidate-conservation failures separately.


## EW-B4.2R10R3 structured dossier additions

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

| Symbol | Responsibility |
|---|---|
| `PlaneCutCoexistenceFailureDossier` | Typed per-state provenance: category, stage, source vertex, victim/foreign pair, linked edges, immutable star, topology counts, and diagnostic. |
| `BuildPlaneCutCoexistenceFailureDossier` | Extracts authoritative structured evidence from root and trial audits, including retry dossiers and early trial exits. |
| `ResolvePlaneCutEffectiveFailureDossier` | Uses newer structured evidence when present; otherwise carries the parent's actionable conflict through the child state. |
| `BuildPlaneCutImmutableIncidentStar` | Captures the complete source-vertex star from the original individually viable candidate list. |
| `BuildPlaneCutCoexistenceConflictEdgeSet` | Branches from the effective typed dossier and filters only edges already explicitly excluded. |
| `IsPlaneCutWinningStateFinalized` | Certifies exact lifecycle, Coverage, candidate, and materialized-width state before closure may return success. |
| `ApplyPlaneCutCoexistenceSuccess` | Clears stale root failure provenance, commits explicit exclusions, finalizes retained edges, and enforces the winning-state contract. |

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

`AppendPlaneCutCoexistenceSearchStates` now writes each state's stage, source vertex, victim/foreign pair, linked set, immutable star, implicated set, candidate counts, minimum width scale, final validity, and signature. Batch capture stores the same trace for TXT reporting.

### `MassGenerator.cs` and `Editor/GeneratedMassEditor.cs`

`EdgeWearBatchAuditCaseResult` carries the complete coexistence search trace. The R10R3 matrix report contract was `EW-B4.2R10R3`; its TXT report appended `[Case N Coexistence Search]` sections without expanding Console output or changing the CSV schema.


## EW-B4.2R10R4 corner-width eligibility reconciliation

### `MassGenerator.EdgeWear.SelectionAndCorners.cs`

| Symbol | Responsibility |
|---|---|
| `ApplyEdgeWearCoverageCornerSolution` | Treats the shared corner-width solve as the pre-shell coexistence boundary and classifies missing or numerically inactive widths before plane construction. |
| `SetEdgeWearCornerWidthCoexistenceIneligibility` | Applies the complete atomic lifecycle transition for `corner-width-missing` and `corner-width-inactive` while retaining truthful width evidence. |
| `RecalculateEdgeWearCoverageAudit` | Separates total `WidthInactiveCount` from `UnresolvedWidthInactiveCount` and counts pre-shell corner-width exclusions independently from search exclusions. |

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

| Symbol | Responsibility |
|---|---|
| plane-candidate assembly | Reuses the corner-width lifecycle transition defensively and synchronizes selected/active denominators before shell construction. |
| `BuildPlaneCutExpectedCoexistenceEdgeSet` | Requires selected, active, positive-width, coexistence-eligible records; candidate fallback is permitted only when no Coverage audit exists. |
| `IsEdgeWearCoverageMaterialized` | Requires zero unresolved inactive widths while preserving resolved inactive-width evidence. |
| `IsPlaneCutWinningStateFinalized` | Applies the same unresolved-width contract to committed coexistence winners and reports total plus unresolved counts on failure. |
| `SynchronizePlaneCutCoexistenceExclusionEvidence` | Publishes the deterministic union of pre-shell and search-time coexistence exclusions and their exact reasons. |
| `PlaneCutCoexistenceSearchStateRecord` | Stores exact expected, actual, missing, and unexpected edge-ID sets for every processed search state. |

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

Coexistence telemetry now has dedicated `cornerWidthMissing` and `cornerWidthInactive` categories, reports `preShellExclusions` separately from `searchExclusions`, and labels internal search-state candidate-conservation failures distinctly from terminal matrix failures.

### `MassGenerator.cs` and `Editor/GeneratedMassEditor.cs`

`EdgeWearBatchAuditCaseResult` carries both corner-width exclusion counts. R10R4 introduced the `EW-B4.2R10R4` matrix contract; TXT and CSV output include both categories, and terminal candidate-conservation failures are no longer conflated with failed intermediate search states. Its solver acceptance was two deterministic `30/30` runs.


## EW-B4.2R11A visual selection and overlay reliability

### `MassGenerator.EdgeWear.SelectionAndCorners.cs`

Ordinary inspector maximum Coverage no longer bypasses `ArtisticEligible`. The candidate list now contains only artistically eligible, geometrically viable records. `UnifiedBatchAudit` is the explicit editor-only exception that still includes every geometrically viable candidate so the frozen 30-case R10R4 solver matrix remains comparable.

### `MassGenerator.cs` and `MassGenerator.EdgeWear.Orchestration.cs`

`EdgeWearEvaluationMode.UnifiedBatchAudit` separates exhaustive matrix evaluation from normal visual preview. `EdgeWearDebugEdgeRecord` now carries a stable state, reason, length, and dihedral. The existing source-edge view uses the full current eligibility/certification evaluation and classifies each edge as certified, artistically filtered, width-floor failure, isolated-rail failure, coexistence-excluded, or another structural/geometric state.

### `GeneratedMass.cs`

The source-edge cache records the complete production-generation state plus edge-wear amount, width, and Coverage. Regeneration invalidates stale data; a successful unified preview reuses its current classified records. The overlay can therefore prove its shape seed and source-edge count rather than silently drawing a previous mass.

### `Editor/GeneratedMassEditor.cs`

The existing source-edge overlay auto-refreshes when its cached state no longer matches the selected mass. Labels include compact state codes (`C`, `S`, `E`, `A`, `W`, `R`, `X`, `G`, `B`) and the Scene panel reports the current shape seed and category counts. No additional debug view was added. The original single `EW-B4.2R11A` matrix contract is superseded by the dual R11A.1 topology and preview contracts documented below.

## EW-B4.2R11A.1 preview coverage-contract repair

### `MassGenerator.EdgeWear.Types.cs`

`EdgeWearCoverageAudit.RequireAllGeometricCandidates` separates exhaustive solver coverage from ordinary visual coverage. `MaximumCoverageMode` remains independent and continues to control maximum-coverage solver behavior.

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

`IsEdgeWearCoverageMaterialized` and `IsPlaneCutWinningStateFinalized` now require the coexistence denominator only when `RequireAllGeometricCandidates` is true. Ordinary previews certify exact materialization of the selected visual candidate set. No plane, clipping, topology, width-floor, or search geometry logic changed.

### `MassGenerator.cs` and `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

`EdgeWearBatchAuditCaseResult` records the active denominator policy and calculates `ExpectedCertificationCount` accordingly. `GenerateUnifiedEdgeWearPreviewParityAuditCase` uses `UnifiedPreviewBatchAudit`, which captures the same immutable placement and full audit evidence as the exhaustive batch path without enabling all-geometric selection.

### `Editor/GeneratedMassEditor.cs`

The editor exposes two independent 30-case audits. The existing topology matrix remains exhaustive and writes the canonical batch report. The artistic preview parity matrix follows the ordinary preview candidate policy and writes separate TXT/CSV reports. Aggregate coverage failures and certified ratios use each case's explicit expected certification count. Report contracts are `EW-B4.2R11A.1-topology` and `EW-B4.2R11A.1-preview`.


## EW-B4.2R11B.1 coincident boundary-seam reconciliation

`MassGenerator.EdgeWear.SelectionAndCorners.cs` now canonicalizes two one-sided source-edge incidences when their endpoints match in reversed order within `PointMergeDistance`, the incidences belong to different faces, and no exact `EdgeKey` match exists. This repairs quantization-boundary splits without changing source faces or broadening the tolerance.

`MassGenerator.EdgeWear.Graph.cs` applies the same policy to topology vertices and graph edges: a missed `VertexKey` may alias an existing vertex only within `PointMergeDistance`, and a missed edge key may reuse only a reversed, currently one-sided graph edge. The graph records vertex-alias and seam-pair counts.

Coverage telemetry preserves raw and canonical source counts, candidate-stage seam-pair count, graph vertex aliases, graph seam-pair count, and the canonical source-edge IDs marked `coincidentSeamReconciled`. Matrix contracts are `EW-B4.2R11B.1-topology` and `EW-B4.2R11B.1-preview`. R11B.1 does not implement micro-junction rail traversal or rendered-normal correction.

## EW-B4.2R11B.1C rollback and collateral-preservation guard

R11B.2 and R11B.3 are rejected recovery experiments. R11B.2's singleton plane-shell fallback evaluated candidates but recovered none. R11B.3 mutated a temporary bevel graph and caused broad owner-face provenance failures, reducing seed `2223/default` from `32` to `19` geometrically eligible edges and seed `5727/default` from `36` to `22`. The authoritative implementation therefore returns to R11B.1 coincident boundary-seam reconciliation.

### `MassGenerator.EdgeWear.SelectionAndCorners.cs`

`CaptureEdgeWearCollateralBaseline` snapshots every canonical edge immediately after the unchanged individual geometric viability preflight and before any later recovery or artistic selection stage. `EvaluateEdgeWearCollateralPreservation` compares the current lifecycle against that immutable snapshot on every Coverage recalculation. It records recovered, lost, and changed edge IDs. A changed baseline edge includes source-index, owner-face, classification, length, dihedral, feasible-width, or width-fraction drift.

### `MassGenerator.EdgeWear.Types.cs` and `MassGenerator.cs`

`EdgeWearCoverageAudit` owns the baseline snapshot and collateral result. `EdgeWearBatchAuditCaseResult.Passed` now requires `CollateralPreservationValid`, zero lost edges, and zero changed edges. A future recovery pass may add newly viable edges, but it cannot pass by making an unrelated baseline edge ineligible or changing its provenance.

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs` and `Editor/GeneratedMassEditor.cs`

Coverage telemetry reports `collateral=baseline/current/recovered/lost/changed/valid` plus exact ID sets. Both 30-case matrices classify collateral loss separately and fail the affected case. Report contracts are `EW-B4.2R11B.1C-topology` and `EW-B4.2R11B.1C-preview`.

## EW-B4.2R11B.1D validation-suite editor additions

| Symbol | Responsibility |
|---|---|
| `EdgeWearValidationSuiteJob` | Owns one current-preview capture followed by the topology and artistic-preview matrices, and produces one final status. |
| `StartEdgeWearValidationSuite` | Starts the one-click workflow and captures the selected mass's current preview telemetry before matrix execution. |
| `FinishEdgeWearValidationSuite` | Finalizes the chained audits, writes the combined report, and emits one compact suite Console summary. |
| `BuildEdgeWearValidationSuiteReport` | Embeds current preview telemetry and both complete matrix TXT reports into `Library/GeneratedMassEdgeWearValidationSuite.txt`. |
| `GetEdgeWearLibraryPath` | Centralizes editor-only `Library` report paths for focused and combined audits. |
| `EdgeWearBatchShapeSeeds` | Eleven-seed canonical set; R11B.1D appends `5727`, producing `33` minimum/default/maximum coordinates per policy. |

The focused topology and artistic-preview buttons remain available. The full-suite Inspector row also exposes clipboard copy and file-reveal actions for the single combined report.

## EW-B4.2R11B.1E recovery retirement and baseline-lock inventory

R11B.4, R11B.4.1, and R11B.4.2 are rejected candidate-local owner-face support experiments. In the final one-click runtime suite, each candidate policy completed all `33/33` construction coordinates with zero topology, face-quality, placement, cache, or collateral failures, but the fallback itself reported `27` evaluations, `126` width attempts, zero virtual corners, zero traversed boundary segments, and zero certified recoveries. Seed `5727/default` likewise remained on the accepted `36` geometrically viable and `34/34` selected/certified result while its three fallback candidates produced zero recoveries.

The active implementation therefore restores the R11B.1D code and report schema. `MassGenerator.EdgeWear.SelectionAndCorners.cs`, `MassGenerator.EdgeWear.Types.cs`, `MassGenerator.EdgeWear.Diagnostics.Logging.cs`, and `MassGenerator.cs` are byte-identical to R11B.1D. All owner-face support interval methods, records, counters, CSV columns, hit classifications, and zero-yield matrix gates are removed.

`Editor/GeneratedMassEditor.cs` retains the eleven-seed, `33`-case-per-policy one-click validation suite, clipboard copy, report reveal, and focused matrix actions. Only the report contracts advance to `EW-B4.2R11B.1E-suite`, `EW-B4.2R11B.1E-topology`, and `EW-B4.2R11B.1E-preview`. Coincident-seam reconciliation and collateral-preservation auditing remain authoritative.


## EW-B4.2R12A artistic-selection audit inventory

R12A is telemetry-only. It does not alter geometric eligibility, candidate ordering, Coverage selected-count calculation, corner solving, solved widths, shell construction, or certification. `MassGenerator.EdgeWear.SelectionAndCorners.cs` now records the exact current score inputs—length score, dihedral score, deterministic random term, base suppression, upward-edge boost, and recipe character boost—alongside diagnostic-only context metrics for edge-axis orientation, camera-independent silhouette potential, feasible and solved width fraction, local geometric-edge density measured within the existing `maximumDimension * 0.34` length-score normalization scale, and shared-vertex crowding. The diagnostic-only context metrics carry explicit zero score weight in the report.

`CaptureEdgeWearArtisticSelectionAudit` runs after the unchanged descending score sort and selected-count calculation. It records the actual selection rank, threshold score, and threshold delta without changing the ordered candidate list. `MassGenerator.EdgeWear.Diagnostics.Logging.cs` adds an `[Artistic Selection Audit]` section with filter-reason counts, all/selected/filtered score ranges, and length, dihedral, orientation, silhouette, density, and crowding distributions. Every bin reports `all/selected/artistically-filtered` counts.

`EdgeWearBatchAuditCaseResult` and `Editor/GeneratedMassEditor.cs` project the same audit into both 33-case matrix TXT/CSV reports and the one-click combined report. Matrix pass criteria are unchanged. Report contracts advance to `EW-B4.2R12A-suite`, `EW-B4.2R12A-topology`, and `EW-B4.2R12A-preview`.


## EW-B4.2R12A.1 comprehensive artistic evidence suite

### `MassGenerator.cs`

`EdgeWearArtisticEdgeAuditRecord` is the complete editor-only export boundary for artistic analysis. It carries canonical source identity, endpoints and midpoint, both owner normals and the bevel normal, owner faces, length, dihedral, orientation, seam provenance, every structural/geometric/coexistence/artistic gate, score inputs and multiplicative modifiers, diagnostic context, locality and isolated-rail evidence, effect variation/strength/depth, solved/materialized width, and the final lifecycle state. `EdgeWearBatchAuditCaseResult.ArtisticEdges` exposes the immutable per-case record set to the editor without changing runtime selection.

### `MassGenerator.EdgeWear.SelectionAndCorners.cs`

The existing candidate pass now copies already-computed geometry, normal, score-component, effect, and context values into the lifecycle record. The production score formula, hard artistic gates, descending score ordering, Coverage selected-count calculation, and generated bevel geometry remain unchanged.

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

`PopulateEdgeWearArtisticEdgeRecords` maps the complete lifecycle and viability evidence into the editor-only public record array after each batch case. The export is ordered by canonical source-edge ID and does not recalculate or mutate eligibility, score, width, or certification state.

### `Editor/GeneratedMassEditor.cs`

The one-click suite now derives a comprehensive scenario analysis from the existing 33 artistic-preview matrix cases; it performs no additional rock generation. The scenario universe includes the exact current policy, random/modifier/gate ablations, every angle/length/random weight triple on a 0.05 simplex under all eight modifier masks, all hard-gate masks, single-metric controls, signed context-weight sweeps, and named composite policies. Every fixed selected slot and native Coverage decile is analyzed.

The combined report contains raw per-edge evidence, score/metric Pearson and Spearman correlations, Pareto-front and dominance evidence, per-edge rank ranges and selection frequencies across the full scenario universe, named-policy churn, every cutoff threshold and gap, no-random sensitivity, scenario intersection/union/core evidence, and cross-width rank/Jaccard stability. The same run automatically writes full raw tables to `Library/GeneratedMassEdgeWearArtisticComprehensiveAudit.txt`, `Library/GeneratedMassEdgeWearArtisticComprehensiveEdges.csv`, and `Library/GeneratedMassEdgeWearArtisticComprehensiveScenarios.csv`. The user still needs to copy only `GeneratedMassEdgeWearValidationSuite.txt`.

Contracts are `EW-B4.2R12A.1-suite`, `EW-B4.2R12A.1-topology`, `EW-B4.2R12A.1-preview`, and `EW-B4.2R12A.1-comprehensive`.


## EW-B4.2R12A.1b recorded-rank analyzer correction inventory

`Editor/GeneratedMassEditor.cs` treats the complete pre-coexistence `GeometricEligible && ArtisticEligible` population as the production rank universe and validates final surviving `Candidate` membership separately. This preserves legitimate original ranks when coexistence later removes candidates. The exact-current outcome uses recorded production ranks; score reconstruction remains independent.


## EW-B4.2R12B.1 geometric-priority selection inventory

### `MassGenerator.EdgeWear.SelectionAndCorners.cs`

The artistic angle gate is `angleScore > 0.055`. Candidate score uses angle/length/random weights `0.60/0.35/0.05`, a base priority factor compressed to `0.60..1.00`, and an upward priority factor compressed to `0.925..1.075`. Raw `ArtisticBaseSuppression` and `ArtisticUpwardEdgeBoost` values remain recorded so the analyzer can reproduce the compressed factors. `ArtisticCharacterBoost` remains exported but no longer affects ordering.

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

The artistic-selection summary reports the R12B.1 score contract. Existing fields, distributions, and CSV projections remain unchanged.

### `Editor/GeneratedMassEditor.cs`

`current-exact` uses weights `0.60/0.35/0.05`, base and upward modifier masks only, and the same factor compression as production. Current-policy ablations and current-plus context sweeps use the R12B.1 baseline. Hypothetical modifier masks may still include character boost as an explicit counterfactual. Scenario count, raw edge export, scenario export, and rank-integrity validation remain unchanged. Report contracts are `EW-B4.2R12B.1-suite`, `EW-B4.2R12B.1-topology`, `EW-B4.2R12B.1-preview`, and `EW-B4.2R12B.1-comprehensive`.


## GM-R12B.1C live render-mesh audit and proof inventory — superseded diagnostic baseline

### `Editor/GeneratedMassEditor.cs`

| Symbol | Responsibility |
|---|---|
| `DrawRenderMeshDiagnostics` | Exposes the explicit single-object live-mesh audit, report copy/reveal, Scene-view controls, and temporary proof actions. |
| `BuildRenderMeshAudit` | Reads the current `MeshFilter.sharedMesh` and audits channel counts/finiteness, vector magnitudes, robust position outliers, triangle area/conditioning, UV determinants, stored-normal agreement, and outward winding without mutating the mesh. |
| `BuildRenderMeshTriangleAudit` | Captures exact per-triangle positions, indices, UV0, normals, tangents, colors, UV2, 3D area, relative area, UV determinant, normal agreement, and winding evidence. |
| `BuildRenderMeshAuditReport` | Originally wrote `GM-R12B.1-render-audit-v1`; GM-R12B.1D supersedes this contract with the zero-normal-aware v2 report. |
| `DrawRenderMeshAuditSceneOverlay` | Draws the selected worst triangle, indices, and failure reason with depth-tested or X-ray Handles. |
| `CreateRenderMeshProofClone` | Creates one temporary `HideAndDontSave` clone at the exact source transform while temporarily suppressing the source renderer. |
| `RepairProofMeshTangents` | Historical tangent-only proof helper. Removed and replaced by `RepairProofMeshNormalsAndTangents` in GM-R12B.1D after the proof failed on zero stored normals. |
| `DestroyRenderMeshProofClone` | Restores the source renderer and destroys all temporary proof objects, meshes, and materials on explicit removal or editor deselection. |

No production or shared procedural file changes in this patch. `MeshData.cs`, `MeshBuilder.cs`, `MassGenerator.MeshOutput.cs`, shaders, materials, and serialized assets remain unchanged pending proof.


## GM-R12B.1D render-normal repair inventory

### `MassGenerator.Types.cs`

| Symbol | Responsibility |
|---|---|
| `TryNormalizeMassVector` | GM-R12B.1D introduced explicit normalization to avoid Unity's larger `Vector3.normalized` zero cutoff; GM-R12B.1E corrects it to accept every finite mathematically non-zero vector using double-precision magnitude evaluation. |
| `TriangleSoup.AddTriangle` | Stores authored normals only through the explicit normalization contract; invalid authored normals remain absent and fall back to geometric face-normal construction. |

### `MassGenerator.MeshOutput.cs`

| Symbol | Responsibility |
|---|---|
| `BuildMeshData` | Uses explicit authored/geometric normal normalization and fails deterministically instead of emitting `Vector3.up` or zero normals. |
| `ValidateGeneratedMassMeshData` | Verifies complete finite Generated Mass channels, valid indices, normalizable geometric triangles, and stored-normal agreement before `MeshBuilder` application. |

### `GeneratedMass.cs`

| Symbol | Responsibility |
|---|---|
| `ProductionGenerationContractVersion = 2` | Invalidates previously accepted transient meshes after the render-normal semantic correction. |
| `ValidateGeneratedRenderMeshChannels` | Runs once after `MeshBuilder.ApplyToMesh` and tangent recalculation to require complete finite unit normals/tangents, valid handedness, positions, UV0, UV2, and colors. |

### `Editor/GeneratedMassEditor.cs`

| Symbol | Responsibility |
|---|---|
| `BuildRenderMeshAudit` | Treats zero normals as hard failures and chooses them ahead of UV-conditioning warnings. |
| `BuildRenderMeshAuditReport` | Emits `GM-R12B.1D-render-audit-v2` with explicit zero-normal triangle flags. |
| `RepairProofMeshNormalsAndTangents` | Reconstructs invalid proof-clone normals from exact triangle geometry, then rebuilds only affected or unsafe tangents. |

No changes are made to shared `MeshData.cs`, `MeshBuilder.cs`, shaders, materials, scenes, prefabs, UV projection, or edge-wear geometry.


## GM-R12B.1E scale-correct render-normal correction inventory

### `MassGenerator.Types.cs`

| Symbol | Responsibility |
|---|---|
| `TryNormalizeMassVector` | Normalizes every finite mathematically non-zero vector using a double-precision magnitude calculation. It no longer compares cross-product magnitude squared (length^4) with `MinimumEdgeLengthSqr` (length^2). |

### `MassGenerator.MeshOutput.cs`

`BuildMeshData` and `ValidateGeneratedMassMeshData` retain the GM-R12B.1D hard channel contract, but tiny scale-valid triangles such as seed `8889` face `76` now produce a finite unit normal instead of failing the incorrect absolute cutoff. No source positions, indices, UVs, feature channels, topology, or tangent-generation ownership changes.

### `Editor/GeneratedMassEditor.cs`

| Symbol | Responsibility |
|---|---|
| `TryNormalizeRenderMeshVector` | Mirrors production's finite non-zero, double-precision normalization contract for audits and proof clones. |
| `BuildRenderMeshTriangleAudit` | Computes geometric normals through the robust helper rather than Unity's small-vector normalization epsilon. |
| `BuildRenderMeshAuditReport` | Emits `GM-R12B.1E-render-audit-v3`, with `passed`, `passed-with-warnings`, or `failed` status. Finite UV-conditioning observations remain warnings and do not represent channel failure. |
| `RepairProofMeshNormalsAndTangents` | Can reconstruct normals and tangents for tiny valid proof triangles without rejecting them through the former absolute cutoff. |

`ProductionGenerationContractVersion` remains `2`: GM-R12B.1E corrects an over-restrictive implementation of the already-promoted render-normal contract and introduces no new serialized or reusable mesh semantics.

## EW-B4.2R13A.1 outlier-recovery inventory

### `MassGenerator.EdgeWear.BoundedSingleEdge.cs`

`TrySolveBoundedIsolatedRailPoint` now computes the exact adjacent-segment parameter in double precision, derives parameter tolerance from the existing absolute point tolerance, and canonicalizes only a spatially bounded endpoint overshoot. Endpoint proximity is no longer an independent rejection. Successful audits retain canonicalization count, maximum parameter overshoot, maximum snap distance, and minimum endpoint distance. A remaining outside-segment failure includes raw/canonical parameter, overshoot, parameter tolerance, snap distance, point tolerance, and nearest-endpoint distance in the existing compact diagnostic.

### `MassGenerator.EdgeWear.SelectionAndCorners.cs`

`RunEdgeWearIsolatedViabilityPreflight` compares certified local width against the canonical minimum style width floor rather than against the current requested-width fraction. `TryFindChamferSharedEdgeRetentionSubset` is a terminal, hard-capped recovery path used only when uniform scaling would deactivate a participant. It enumerates at most 63 retained subsets, solves each subset's stable common scale, rejects sub-floor retained widths, and ranks certified results by retained count, production artistic score, retained width, and deterministic source-edge order.

### `MassGenerator.EdgeWear.Types.cs`

`EdgeWearEdgeViabilityRecord` stores the canonical minimum style width, required certified floor, and isolated-boundary canonicalization evidence. `ChamferCornerStats` stores bounded retention-search invocation, state, commit, and participant counts.

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

The viability report labels the new `minimumStyleWidth` gate explicitly, retains requested-width fractions as evidence, reports successful boundary canonicalization measurements, and reports retention-search counts in the compact corner audit. The old failure name `maximum-feasible-width-below-minimum-scale` is superseded by `maximum-feasible-width-below-minimum-style-floor`.

### `MassGenerator.EdgeWear.Orchestration.cs`, `MassGenerator.cs`, and `Editor/GeneratedMassEditor.cs`

Debug-state classification, batch exclusion naming, CSV headers, one-click suite contracts, and comprehensive-report contracts advance to R13A.1. `EdgeWearValidationSuiteJob` stores topology cases and evaluates five editor-only canonical source-edge fixtures so a target cannot disappear behind geometric exclusion while the suite still passes. The artistic score formula and recorded-rank analyzer remain unchanged from the accepted R12B.1 baseline.

## EW-B4.2R13A.2 boundary and full-shell recovery inventory

R13A.1 runtime validation failed (`31/33`, `31/33`, outliers `0/5`). Its endpoint clamp and local retention-subset implementation are superseded. R13A.2 changes the following ownership points.

### `MassGenerator.EdgeWear.BoundedSingleEdge.cs`

`TrySolveBoundedIsolatedRailPoint` no longer authorizes an out-of-segment point by clamping it to the presumed adjacent edge. It builds the exact owner-face selected offset line and delegates to `TryResolveBoundedOwnerBoundaryHit`.

`TryResolveBoundedOwnerBoundaryHit` intersects the forward rail against every manifold boundary segment on the exact owner graph/source face, excluding the selected edge. It deduplicates vertex hits, requires a unique nearest forward terminal, and returns exact resolved edge/target-face provenance. `MeasureBoundedPointAgainstGraphEdge` retains the original-adjacent parameter and segment-distance evidence so a changed terminal cannot be confused with numerical drift.

`BoundedOwnerBoundaryHit`, `BoundedIsolatedRailPoint`, and `BoundedSingleEdgeAuditResult` carry resolved edge, original edge, candidate count, ray/segment measurements, snap distance, and alternate-boundary counts. All existing downstream bounded geometry and topology audits remain authoritative.

### `MassGenerator.EdgeWear.SelectionAndCorners.cs`

`RunEdgeWearIsolatedViabilityPreflight` restores `FeasibleWidthFractionValid` as the ordinary gate. `WidthRecoveryProvisional` is a narrow secondary state for an isolated-certified edge that fails the requested-width fraction but still meets the canonical minimum-style absolute floor.

`TrySolveCornerAwareChamferWidths` no longer performs the R13A.1 local 63-state retention subset. When uniform shared-edge scaling would deactivate participants, it records a `ChamferCornerConflictRecord` and retains the existing safe local zeroing behavior for that trial. The full-shell owner decides which participant, if any, is deferred.

### `MassGenerator.EdgeWear.Types.cs`

`ChamferCornerSolution` owns a list of `ChamferCornerConflictRecord` values. `EdgeWearCoverageAudit.CloneForTrial` and `EdgeWearEdgeLifecycleRecord.CloneForTrial` provide isolated lifecycle state for bounded full-shell search without mutating the authoritative audit during rejected states. Viability records are immutable during these trials and are shared read-only.

### `MassGenerator.EdgeWear.Orchestration.cs`

`HasSelectedWidthRecoveryProvisional` keeps the ordinary R12B.1E route untouched for all non-provisional selections.

`TryAuditConflictDirectedChamferPlaneSolution` owns the bounded full-shell retention search. `EvaluateChamferPlaneRetentionTrial` runs complete corner and plane-shell certification against a cloned coverage audit. `CollectChamferPlaneRetentionBranchEdges` obtains branch candidates from corner-collapse participants, terminal band victim/foreign evidence, or selected provisional edges when a final render-channel rejection has no narrower pair.

Trial ranking is certified count, summed production artistic score, total materialized width, then deterministic forced-defer order. The winner is rerun once against the real audit.

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

`CopyPlaneCutBandAudit` now preserves the first available victim/foreign conflict evidence. This keeps terminal minimum-width band splits branchable by the full-shell owner instead of degrading them to an unclassified construction failure.

### Diagnostics and editor validation

`MassGenerator.EdgeWear.Diagnostics.Logging.cs`, `MassGenerator.cs`, and `GeneratedMassEditor.cs` restore requested-width-fraction exclusion naming, publish provisional-width and complete owner-boundary evidence, and advance suite/report contracts to R13A.2. The existing five editor-only named source-edge fixtures remain unchanged and are part of suite pass/fail.

No shared `MeshData`, `MeshBuilder`, shader, material, UV, scene, prefab, artistic score, Coverage count, or production generation-contract version changes are part of R13A.2.

## EW-B4.2R13A.3 single-search execution inventory

R13A.2 runtime validation was cancelled at topology case `24/33` (`seed 7778`, maximum width) after more than ten minutes. The completed cases remained `24/24`, but preview, outlier, and comprehensive stages never ran. The cause was nested bounded search: `TryAuditConflictDirectedChamferPlaneSolution` could invoke `AuditPlaneCutBevelKernel`, whose width-reduction path could independently invoke `TryResolvePlaneCutCoexistenceByExclusion` for every outer state.

### `MassGenerator.EdgeWear.Orchestration.cs`

`TryAuditSingleSearchChamferPlaneSolution` replaces the R13A.2 optimizer behavior for selected provisional candidates. Each forced-deferral state runs the corner solver and one complete plane-shell evaluation with kernel coexistence recursion explicitly disabled. The frontier is ordered by fewest forced exclusions, lowest removed R12B.1 artistic score, lowest removed certified width, and deterministic edge order. The first fully certified state wins; the search no longer evaluates every remaining state after success.

The provisional search is capped at 128 states, ten forced exclusions, and five seconds. It checks the editor audit cancellation probe between states and reports explicit cancellation, time-budget, and state-budget terminal reasons. The winning state is rerun once against the authoritative lifecycle audit.

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

`AuditPlaneCutBevelKernel` and the maximum-coverage width-reduction path accept an `allowCoexistenceSearch` execution flag. Ordinary non-provisional evaluations pass `true` and retain the existing coexistence search. Provisional single-search states pass `false`, so a shell failure returns its exact conflict evidence to the sole active frontier rather than starting a nested second frontier.

The ordinary coexistence search also owns a five-second audit budget and cancellation checks. Its detailed report now includes `timeBudgetExceeded`, `cancelled`, and `elapsedMs` beside the existing state counters.

### `MassGenerator.cs` and `Editor/GeneratedMassEditor.cs`

`SetEditorEdgeWearAuditCancellationProbe` is a transient, nonserialized editor-validation hook. Matrix evaluation installs a cancelable-progress callback only for the synchronous case generation and clears it in `finally`. A cancellation detected inside a search prevents the partially evaluated case from being appended to the matrix.

Suite and report contracts advance to R13A.3. Owner-face boundary resolution, provisional-width evidence, the five mandatory outlier fixtures, GM-R12B.1E channel guards, and the R12B.1 artistic policy remain unchanged.


## EW-B4.2R13A.4 certified-baseline augmentation inventory

R13A.3 runtime validation proved that the single-search execution guard prevented editor lockup, but it returned `31/33` topology, `31/33` artistic-preview, `0/5` outlier recovery, erased the live seed-8889 preview when augmentation failed, and withheld comprehensive evidence because two maximum-width cases returned empty audit records. R13A.4 therefore changes recovery from a replacement solve into an optional augmentation of an already certified baseline.

### `MassGenerator.EdgeWear.Orchestration.cs`

`TryAuditCertifiedBaselineAugmentation` first evaluates and retains a complete certified baseline with selected width-recovery provisional edges forced off. Corner-inactive recovery participants are then added to the recovery set from the baseline corner-conflict evidence. If no recoverable participant exists, the certified baseline is returned directly.

Optional augmentation starts from the baseline exclusion set with only recovery edges re-enabled. Every trial uses a cloned lifecycle audit and disables kernel coexistence recursion, so only one bounded frontier is active. A candidate may replace the baseline only when it is fully certified, materializes at least one previously absent recovery edge, and is superior by certified count, accepted R12B.1 artistic score, then certified width. Timeout, state exhaustion, cancellation, or no superior state returns the immutable baseline and records explicit augmentation evidence; it never clears a valid preview or matrix record.

`PlaneCutBevelAuditResult` now publishes baseline-certified/applied and augmentation attempted/applied, state/frontier, elapsed-time, timeout/cancellation, last failure, and implicated-edge evidence.

### `MassGenerator.EdgeWear.BoundedSingleEdge.cs`

The complete owner-face boundary solver remains authoritative. When the two exact rails at one endpoint resolve through different support faces, `TryBuildBoundedMultiSupportPlaneCutFaces` constructs the endpoint interval with one exact bevel half-space cut rather than requiring one shared support face. The two selected source-edge endpoints are removed while every other source vertex is required to remain, every source face retains provenance, one bounded bevel cap is required, and all four solved rail points are retained on the cap boundary.

The multi-support path records both support-face terminals per endpoint, the exact cut plane, modified support interval count, and boundary-path vertex count. It remains subject to the existing strict intersection, source-provenance, owner/support modification, manifold topology, containment, convexity, bounds, volume, face-intersection, sidedness, triangulation, and render-channel audits. Ordinary same-support endpoint construction remains unchanged.

### Diagnostics, editor, and contracts

`MassGenerator.EdgeWear.Diagnostics.Logging.cs` publishes the baseline/augmentation and multi-support endpoint fields in the existing compact audit records. `GeneratedMassEditor.cs` advances suite, topology, preview, and comprehensive contracts to R13A.4. The five named editor-only outlier fixtures remain mandatory; no production seed or source-edge-ID policy is introduced.

No shared `MeshData`, `MeshBuilder`, shader, material, UV, scene, prefab, artistic-score, Coverage-count, or production generation-contract change is part of R13A.4.


## EW-B4.2R13A.6 retained-point hull and finalized corner recovery inventory

R13A.4 remains the stable incomplete runtime baseline. R13A.5 is rejected because its sampled two-plane family recovered `0/5`, changed certified edge identity without recovering a named target, and failed to capture any finalized `corner-width-inactive` participant.

### `MassGenerator.EdgeWear.BoundedSingleEdge.cs`

The R13A.4 ordinary and same-support paths remain unchanged. Multi-support recovery now constructs the exact retained point set: every original source vertex except the two selected-edge endpoints, plus the four exact solved rail terminals. The existing bounded convex-hull plane extractor enumerates all non-degenerate point triples, retains only global supporting planes, merges coplanar planes, and emits however many facets the retained point set requires. Source-plane facets retain complete `SourceFace` provenance; all new supporting facets are attributed to the selected bounded bevel.

The retained hull is accepted only when both selected endpoints disappear, all four rails lie on the connected bevel-band boundary, every source face remains uniquely represented, both owner faces and only endpoint-star support faces change, and all existing preparation, topology, strict-intersection, containment, convexity, bounds, volume, face-quality, triangulation, and render-channel checks pass. Compact evidence records point, plane, supporting-triple, bevel-facet, and adjacency counts.

### `MassGenerator.EdgeWear.SelectionAndCorners.cs`, `Types.cs`, and `Orchestration.cs`

Corner recovery evidence is captured at the exact finalized transition to `corner-width-inactive`, before lifecycle deactivation. The record retains the last positive width, collapsed shared edge, uniform scale, and exact conflict participants. Recovery no longer requires the target to remain artistically eligible after final corner collapse.

Generic width-provisional edges cannot initiate augmentation. The only production recovery classes are a successfully certified multi-support retained hull and a finalized corner-inactive participant. Every retained-hull edge is removed from the immutable baseline solve even when its certified hull width satisfies the ordinary requested-width fraction; it can enter only through the audited augmentation path. Recovery targets remain protected from branch deferral. An augmented result may replace the R13A.4 baseline only when it certifies at least one recovery target, does not reduce total certified count, and loses no baseline edge outside the exact participant set of a recovered corner target. This forbids unrelated substitutions such as the rejected seed-8889 edge `39` to edge `40` exchange.

### Contracts

Editor suite, topology, preview, and comprehensive contracts advance to `EW-B4.2R13A.6`. No scene, prefab, material, shader, UV, shared mesh builder, artistic-score formula, layer, tag, or production generation-contract change is included.


## EW-B4.2R13A.7 recovery-closure inventory

### `MassGenerator.EdgeWear.Types.cs`

`EdgeWearIsolatedWidthAttemptRecord` is a bounded diagnostic record for the already-executed isolated rail schedule and its one terminal construction. `EdgeWearEdgeViabilityRecord` retains schedule completion, terminal-at-minimum state, resolution, and compact ordered evidence. `ChamferCornerZeroingStage` distinguishes uniform-scale deactivation from the no-progress forced-deferral fallback. Each conflict separately retains all participants and the exact subset actually transitioned to zero; lifecycle records retain the matching stage, full participant set, exact zeroed subset, and final recovery resolution.

### `MassGenerator.EdgeWear.BoundedSingleEdge.cs`

The twelve-step `0.75` rail schedule is unchanged. The solver records each existing attempt and preserves the original nine-argument overload for `MassGenerator.EdgeWear.BoundedAllEdges.cs`. `AuditBoundedSingleEdgeBevel` annotates the terminal construction with exact single-plane and retained-hull outcomes. `FinalizeBoundedSingleEdgeAuditResult` reports `certified`, `complete-rail-infeasible`, `complete-infeasible`, or `unresolved` without issuing any additional geometry call.

### `MassGenerator.EdgeWear.SelectionAndCorners.cs`

`RecordChamferCornerConflict` captures exact participant widths and the exact zeroed-edge subset for both zeroing stages. The former unrecorded `!edgeChanged` fallback now emits `SharedEdgeForcedDeferral` evidence before setting widths to zero. Final inactive capture accepts only the event that actually zeroed the target. Isolated viability retains compact width-attempt evidence only for failed, repeated, or multi-support evaluations.

### `MassGenerator.EdgeWear.Orchestration.cs`

`CollectCornerInactiveRecoveryEdges` rejects width-recovery-provisional and ordinary width-fraction-ineligible edges. `ApplyCornerRecoveryResolution` marks a target certified only when the winning coverage built it, marks complete frontier exhaustion as proven infeasible, and preserves unresolved status for cancellation, time, or state-budget termination. The existing certified-baseline commit guards are unchanged.

### `MassGenerator.EdgeWear.Diagnostics.Logging.cs`

The lifecycle and bounded audit records now include schedule completion/resolution, ordered per-width evidence, single-plane and retained-hull terminal outcomes, corner zeroing stage, full participant set, exact zeroed subset, and corner recovery resolution. Complete evidence remains in the existing explicit editor telemetry/report surfaces.

### `Editor/GeneratedMassEditor.cs`

Suite semantics now report positive fixture resolution as certified, proven infeasible, or unresolved. The suite adds `8889 / maximum / edge 40` as an editor-only negative exclusion fixture and advances all R13 contracts to R13A.7. No production seed/source-edge policy is introduced.

No changes are made to `MassGenerator.cs`, `GeneratedMass.cs`, `MassGenerator.EdgeWear.PlaneCutKernel.cs`, `MassGenerator.MeshOutput.cs`, shared procedural mesh types, serialized assets, materials, or shaders.
