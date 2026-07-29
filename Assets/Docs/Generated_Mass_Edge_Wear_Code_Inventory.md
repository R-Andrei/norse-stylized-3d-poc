# Generated Mass Structural Feature Code Inventory

Status: current ownership map

## Runtime entry and routing

### `MassGenerator.cs`

- owns public `Generate(...)` entry points;
- owns `EdgeWearEvaluationMode`;
- `BaseGeometryOnly` is the disabled-feature and safe-fallback path;
- `ResolveProductionSurfaceFeatureBuildMode(...)` selects `ProductionSurfaceFeatures` whenever edge wear or corner chipping is enabled;
- corner-chip production enters the certified chip-first full integration builder, while edge-only production uses the same unified certified augmentation path;
- owns editor diagnostic entry points and status structures.

### `MassGenerator.PlaneCut.cs`

- builds the base polyhedron and source placement soup;
- invokes structural feature construction only when mode is not `BaseGeometryOnly`;
- returns base geometry when no structural result is requested or accepted.

### `MassGenerator.EdgeWear.Orchestration.cs`

- dispatches bevel, corner, audit, and preview modes;
- treats `BaseGeometryOnly` as the no-structural-feature path;
- treats `ProductionSurfaceFeatures` as ordinary certified edge-wear construction without requiring preview state.

## Geometry and selection owners

- `MassGenerator.EdgeWear.PlaneCutKernel.cs` — plane-cut bevel/chip geometry, certification helpers, retained coexistence evidence.
- `MassGenerator.EdgeWear.SelectionArchitecture.cs` — ranking, selection, and ranked-discard production logic.
- `MassGenerator.EdgeWear.Graph.cs` — topology and candidate graph ownership.
- `MassGenerator.EdgeWear.BoundedSingleEdge.cs` — bounded edge construction support.
- `MassGenerator.EdgeWear.*` partials — feature-specific construction, orchestration, diagnostics, and reporting.

## Final mesh owners

- `MassGenerator.MeshOutput.cs` — final vertex emission, authored surface-group normal resolution, mesh data validation.
- `MassGenerator.Types.cs` — `TriangleSoup`, feature provenance, authored normals/groups, mesh data structures.
- `MeshBuilder` application path — assigns final normals and invokes Unity tangent recalculation.

## Component and editor owners

- `GeneratedMass.cs` — serialized recipe/settings ownership, mesh lifecycle, editor preview state.
- `Editor/GeneratedMassEditor.cs` — Inspector actions, incremental suites, mesh diagnostics, reports.
- Editor actions are diagnostics and do not own whether ordinary production geometry contains bevels/chips.

## Surface-response direction

Canonical owner: `Generated_Mass_Surface_Response_Architecture.md`.

- no per-rock atlases;
- shared triplanar whole-rock normal detail;
- compact primary/secondary feature fields;
- fixed zero-to-two structural evaluations per fragment;
- no arbitrary runtime feature list.

## Current production contract

GM-SURFACE.2 completed the production promotion:

1. `ProductionSurfaceFeatures` drives ordinary edge-only construction;
2. corner-enabled production uses the certified chip-first integration builder;
3. disabled structural settings retain cheap base-only parity;
4. failed decorative construction falls back deterministically to chip-only or base geometry;
5. frozen ranked selection remains authoritative;
6. production-state identity includes all bevel and corner-chip authoring settings.
