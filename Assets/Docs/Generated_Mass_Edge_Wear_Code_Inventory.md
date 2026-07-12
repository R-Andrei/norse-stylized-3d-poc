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
| `MassGenerator.cs` | Shared constants and top-level generation entry points. |
| `MassGenerator.Types.cs` | Core polygon, edge, vertex-key, and mesh-output support records. |
| `MassGenerator.Helpers.cs` | Shared polygon sanitization, welding, geometry predicates, and utility methods. |
| `MassGenerator.Polyhedron.cs` | Convex half-space clipping, cap construction, and polygon-face maintenance. |
| `MassGenerator.MeshOutput.cs` | Final triangulation and feature-data emission. |

## Edge-wear orchestration and selection

| File | Responsibility |
|---|---|
| `MassGenerator.EdgeWear.Orchestration.cs` | Coordinates selection, corner solving, legacy construction, clone diagnostics, and compact logging. |
| `MassGenerator.EdgeWear.Graph.cs` | Source topology graph and generic topology audits. |
| `MassGenerator.EdgeWear.SelectionAndCorners.cs` | Deterministic edge selection, width feasibility, corner positions, and rail solving. |
| `MassGenerator.EdgeWear.Types.cs` | Edge-wear graph, provenance, diagnostic, and builder result records. |

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

## Plane-cut kernel

### `MassGenerator.EdgeWear.PlaneCutKernel.cs`

| Method | Responsibility |
|---|---|
| `AuditPlaneCutBevelKernel` | Builds all accepted cuts on a deep clone and runs final cap, topology, face, volume, and bounds gates. |
| `TryBuildPlaneCutBevelCandidate` | Converts one active selected edge and its solved rail into an inward half-space cut with source-edge provenance. |
| `ClonePolygonFacesForPlaneCutAudit` | Deep-clones source polygon records so diagnostics cannot mutate rendered geometry. |
| `ConformPlaneCutFaceBoundaries` | Preserves shared collinear segmentation where a final vertex lies inside another face edge. |
| `RepairPlaneCutNumericalSeams` | Snaps only mutually unique, opposite, near-identical open-edge pairs; rolls back unless the exact expected open-edge reduction occurs without new topology damage. |
| `CollectPlaneCutOpenEdges` | Extracts exact one-use clone edges for conservative seam pairing. |
| `IsPlaneCutCandidateAlreadySatisfied` | Detects a cut made redundant by earlier cuts before invoking the clipper. |
| `IsPlaneCutCandidateRedundant` | Accepts a missing final cap only when the final polyhedron satisfies the plane, contact is lower-dimensional, and the original sharp source edge is absent. |
| `DoesPlaneCutSourceEdgeSurvive` | Applies strict topology-scale source-edge survival testing. |
| `CountMatchingPlaneCutCaps` | Counts surviving `ConvexEdgeWear` faces on one candidate plane. |
| `CountInvalidPlaneCutFaces` | Rejects non-finite, degenerate, or oppositely wound faces. |
| `CalculatePlaneCutPolyhedronVolume` | Supplies retained-volume validation. |
| `ArePlaneCutBoundsContained` | Ensures diagnostic clipping does not expand source bounds beyond clip-consistent tolerance. |

### `MassGenerator.Polyhedron.cs`

| Method | Responsibility |
|---|---|
| `ClipPolyhedron` | Clips every face against one half-space and builds the corresponding cap. Optional parameters remain disabled for legacy callers. |
| `ClipPolygon` | Clips one polygon while collecting cap intersections. |
| `ResolveClipIntersection` | Reuses one canonical intersection for both faces sharing an undirected edge when explicitly enabled. |
| `IntersectEdge` | Computes the segment-plane intersection with optional segment clamping. |
| `CreateOrientedFace` | Orders cap vertices and enforces outward winding. |

## Compact diagnostic ownership

`MassGenerator.EdgeWear.Diagnostics.Logging.cs` owns the single compact physical-mass audit. The plane-cut field is:

```text
planeBevel=
selected/
active/
planesBuilt/
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
```

Representative failure text remains in `planeTrace=` and must stay concise.

## Live-geometry boundary

The plane-cut kernel currently operates on a deep clone. Rendered geometry remains on the existing path and every physical audit must continue to report:

```text
geometryCommit=disabled
```

No diagnostic result may mutate live polygon faces, serialized assets, materials, shaders, scenes, prefabs, tags, layers, or components.
