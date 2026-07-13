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
    -> one bounded bevel polygon plus two endpoint caps
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
| `TryBuildBoundedSingleEdgeFaces` | Clones the source shell, inserts four exact graph-owned collinear endpoint-boundary subdivisions, geometrically modifies only the two owner faces, and emits one bounded rail quadrilateral plus two local endpoint caps. |
| `TryClipBoundedOwnerSourceFace` | Projects one convex owner face and its isolated rail into a local 2D basis, clips the polygon against the rail half-plane while retaining the non-edge side, snaps the two intersections to the rail endpoints, and certifies planarity, simplicity, convexity, and winding. |
| `TryInsertBoundedRailSubdivisions` | Uses the stored adjacent graph-edge and opposite-face provenance to split each exact target boundary once; it performs no nearest-segment search across unrelated faces. |
| `TryPrepareBoundedPreviewFaces` | Preserves collinear boundary subdivisions through input validation, weld, conformity, seam repair, and final validation. Convexity uses a temporary duplicate/collinear-simplified loop without altering the audited topology, and failures record exact stage, face, provenance, polygon category, and canonical-subdivision involvement. |
| `TryCreateBoundedFace` | Creates one bounded bevel or endpoint-cap polygon with explicit bounded provenance and construction-time preferred orientation. Final shell outwardness is certified separately after preparation. |
| `TryOrientBoundedGeneratedFacesOutward` | Uses the original convex solid centre to reverse and reconstruct only inward `BoundedEdgeBevel` or `BoundedEndpointCap` faces, then requires zero remaining generated-face winding failures. Original source faces are not reordered. |
| `AuditBoundedSourceFaceChanges` | Normalizes collinear subdivisions before comparison, requiring exactly two geometrically modified owner faces and zero unrelated source-surface changes after final preparation. |
| `AuditBoundedRailFidelity` | Measures solved-corner retention and any final bevel-polygon extent beyond the four rail bounds. |
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
boundedOwner=attempted/clipped/intersectionFailure/degenerate/nonPlanar/nonSimple/nonConvex/windingFailure
boundedPrepare=attempted/succeeded/inputFaces/inputVertices/inputUniqueVertices/outputFaces/outputVertices/outputUniqueVertices/welded/conformed/seamPairs/seamTouchedFaces/inputTopology/outputTopology/inputVolume/outputVolume/volumeDelta/volumeRatio/exactFailure
boundedSourcePrepare=the same complete preparation evidence for the untouched source baseline
boundedTopology=open/nonManifold/tJunction/invalidFaces
boundedBounds=raw/prepared validity, tolerance, raw/prepared/result minima and maxima, and per-side containment margins
boundedVolume=rawSource/preparedSource/result/rawRatio/preparedRatio/sourcePreparationRatio/rawDelta/preparedDelta/minimumRatio/maximumRatio/lowerMargin/upperMargin/valid
boundedCertification=attempted/facesReoriented/outwardWindingFailures
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
