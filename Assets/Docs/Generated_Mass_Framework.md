# Generated Mass Framework

This document defines the stable Generated Mass feature contract. It is not a progress log.

The sole canonical progress ledger is:

```text
Docs/Generated_Mass_Feature_Implementation_Checklist.md
```

## Feature goal

Generated Mass produces deterministic convex stylized rock and mass geometry suitable for the isometric URP project. Edge wear must create real faceted bevel geometry, preserve the closed mass surface, remain inexpensive at runtime, and carry explicit feature data to the final mesh.

## Geometry budgets

Accepted final-mesh budgets remain:

| Tier | Vertex budget | Atlas guidance |
|---|---:|---|
| Standard | no more than 1,600 vertices | 256 when an atlas is actually required |
| High | no more than 3,000 vertices | 256; 512 only for unusually large assets |
| Hero | no more than 8,000 vertices | 512 when justified |

Quality tiers must not change the apparent edge-wear band width solely because of texture resolution.

## Canonical generation order

```text
base convex mass
    -> deterministic surface deformation
    -> polygon-face representation
    -> source topology graph
    -> edge-wear candidate selection
    -> width and corner feasibility solve
    -> bevel construction
    -> topology and geometry validation
    -> triangulation
    -> normals and feature data
    -> final MeshData
```

Edge wear may not mutate the source polygon set unless the selected construction has passed its explicit production gate.

## Edge-wear control ownership

- **Amount** controls the strength or prominence of the edge-wear response, not topology eligibility.
- **Coverage** controls deterministic edge selection density.
- **Width** controls geometric bevel width.
- **Softness** must not secretly expand geometric width; it belongs to shading/falloff response unless a separate approved geometry meaning is introduced.
- **Macro variation** means differences between edges.
- **Micro variation** means variation along one edge.

Controls must have visible, testable responsibilities and meaningful tooltips. Stale or disconnected controls must be removed rather than retained as placeholders.

## Current construction boundary

The active production candidate is the convex plane-cut kernel described in:

```text
Docs/Generated_Mass_Edge_Wear_Recovery_Architecture.md
```

It currently runs on a deep clone. The rendered geometry remains unchanged while:

```text
geometryCommit=disabled
```

is active.

The retained legacy replacement/strip/patch path is diagnostic comparison evidence and must not be mistaken for the approved future production architecture.

## Topology invariants

A production mass must have:

- zero open edges;
- zero non-manifold edges;
- zero T-junctions;
- finite vertices and normals;
- non-degenerate retained faces;
- consistent outward winding;
- positive enclosed volume;
- deterministic output for identical inputs.

A bevel plane must either produce one surviving `ConvexEdgeWear` face or be proven redundant because earlier cuts already removed its source edge and satisfy its half-space.

## Feature-data contract

Bevel faces use:

```text
PolygonFaceFeature.ConvexEdgeWear
```

and preserve the selected edge’s feature strength through triangulation. Final material response may use brightness lift, optional tint, falloff contrast, and smoothness offset, but shader response must not compensate for invalid geometry.

## Atlas policy

Edge-wear atlases are optional diagnostic or feature-specific inputs. Final bevel visibility must not depend on the previously rejected low-resolution boundary atlas path.

Atlas generation is justified only when a retained material feature needs it. Geometry construction, boundary ownership, and bevel width must remain mesh-defined.

## Performance policy

- Prefer deterministic dirty-time construction over per-frame work.
- Generated rocks are static after generation unless explicitly regenerated.
- Expensive validation may remain editor/diagnostic-only.
- Production geometry must respect the accepted tier budgets.
- Do not add per-frame full-mesh rebuilds.
- Cache reusable deterministic data when it materially reduces regeneration cost.

## Editor and diagnostics contract

- Normal Console output is one compact audit per physical mass.
- Detailed evidence is opt-in, deduplicated, and capped to representative failures.
- Diagnostics must never alter production eligibility unless explicitly promoted.
- Editor-only previews must be clearly labeled and must not become serialized artistic controls accidentally.
- Existing layers, tags, components, asset names, and serialized structures may not change without approval.

## Validation contract

Every geometry implementation patch requires:

1. Zero Unity compiler errors and warnings introduced by the patch.
2. Deterministic regeneration of the representative mass set.
3. Exact topology and geometry audit results.
4. Confirmation that unrelated compact fields remain unchanged.
5. Confirmation of the live/clone boundary.
6. Visual inspection before any production promotion.
7. Vertex-budget inspection after production promotion.

## Documentation ownership

| Document | Sole responsibility |
|---|---|
| `Generated_Mass_Feature_Implementation_Checklist.md` | Canonical progress log, methods tried, validation outcomes, active blocker, and next step. |
| `Generated_Mass_Edge_Wear_Recovery_Architecture.md` | Current architecture, invariants, rationale, and promotion gates. |
| `Generated_Mass_Edge_Wear_Code_Inventory.md` | Current files, methods, and dependency boundaries. |
| `Generated_Mass_Framework.md` | Stable feature, control, performance, and validation contract. |

Other documents may reference the canonical progress ledger, but must not maintain competing or complementary patch histories.
