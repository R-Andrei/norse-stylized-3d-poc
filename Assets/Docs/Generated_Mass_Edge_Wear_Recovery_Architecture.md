# Generated Mass Edge-Wear Recovery Architecture

This document defines the current architecture and invariants only. It is not a patch history or validation log.

The sole canonical progress ledger is:

```text
Docs/Generated_Mass_Feature_Implementation_Checklist.md
```

## Current status

The active production candidate is a clone-only convex plane-cut bevel kernel. It reuses the validated deterministic edge selection, width feasibility, corner solving, and requested bevel normal, but replaces independent replacement-face, strip, and patch assembly with direct half-space cuts of the original closed convex polyhedron.

The legacy construction and repair path remains in the project as diagnostic comparison evidence. It does not control rendered geometry.

```text
geometryCommit=disabled
```

remains mandatory until topology and visual preview are both approved.

## Problem statement

The prior construction emitted replacement faces, bevel strips, and corner patches independently, then attempted to repair overlap and incidence after assembly. That architecture can create incompatible boundaries even when each local polygon is individually valid.

The recovery architecture removes the source of that incompatibility:

> Begin with one closed convex mass and intersect it with one inward half-space per active selected edge.

Each cut updates every affected face consistently and creates its own bevel cap. Later cuts naturally trim earlier caps at shared corners.

## Authoritative inputs

The plane-cut kernel consumes only established production/shared data:

- deterministic selected source edges;
- positive solved width per active edge;
- the two incident source faces;
- four solved rail points;
- requested bevel normal;
- edge-wear material strength;
- original source-edge endpoints.

Diagnostic-only overlap, patch, contained-owner, or corrected-clone results are not inputs.

## Generation flow

```text
source convex polygon faces
    -> source topology graph
    -> deterministic edge selection
    -> width and corner feasibility solve
    -> one candidate plane per active edge
    -> sequential half-space clipping on deep clone
    -> boundary conformity pass
    -> conservative numerical seam repair
    -> surviving-cap / redundant-cut classification
    -> topology, face, volume, and bounds audit
    -> editor-only visual preview
    -> explicit production promotion
```

## Candidate plane contract

A candidate is accepted only when:

- its selected edge is an internal manifold edge;
- its bevel normal is finite and non-zero;
- all four solved rail points are finite;
- the rail points are coplanar within the approved geometry tolerance;
- both original source-edge endpoints lie outside the retained half-space by a meaningful amount;
- a candidate-specific clipping epsilon remains below that measured removal.

The candidate stores the plane, material strength, tolerances, and exact original source-edge segment.

## Clipping contract

`ClipPolyhedron` clips all current faces against one plane, collects the shared intersection loop, emits one oriented `ConvexEdgeWear` cap when the cut has two-dimensional contact, welds shared vertices, and sanitizes the result.

Plane-cut diagnostics opt into:

- segment-clamped intersections;
- candidate-specific inside epsilon;
- canonical per-cut intersections keyed by the undirected current edge.

Legacy callers retain the previous defaults.

## Numerical seam repair

The final seam repair is deliberately narrow. It may modify the clone only when all of the following hold:

- both records are exact one-use open edges;
- they belong to different faces;
- they have opposite orientation;
- their corresponding endpoints differ only within a narrow topology-scale tolerance;
- each edge has exactly one mutual counterpart;
- snapping produces the exact expected reduction of two open records per pair;
- non-manifold and T-junction counts do not increase.

The repair snaps all occurrences of the involved endpoint keys to common midpoint targets and then welds. It rolls the entire operation back if any gate fails. It does not infer missing faces, bridge arbitrary holes, or merge ambiguous candidates.

## Redundant-cut classification

A cut may legitimately emit no new cap when previous cuts already place the entire current polyhedron inside its half-space.

A no-cap cut is accepted as redundant only when:

- every current vertex satisfies the candidate plane under the candidate-specific tolerance;
- the original sharp source-edge segment no longer survives under a strict topology-scale test;
- final contact with the plane does not form an unexplained two-dimensional face lacking the expected feature identity.

A nearby bevel boundary must not be mistaken for the original source edge. Source-edge survival therefore uses `PointMergeDistance` scale, not a percentage of stable edge length.

## Final topology gate

Every diagnostic clone must satisfy:

```text
planesRejected = 0
planesBuilt = active
capsMissing = 0
open = 0
nonManifold = 0
tJunction = 0
invalid = 0
valid = 1
```

Additional requirements:

- positive retained volume;
- retained volume no greater than source volume beyond numerical tolerance;
- final bounds contained by source bounds beyond clip-consistent tolerance;
- deterministic output for identical inputs;
- preserved `ConvexEdgeWear` feature strength;
- no live geometry mutation.

## Visual contract

The first production candidate uses sharp mitered meetings between adjacent bevel planes. A broader decorative vertex cut is optional future polish, not a prerequisite for valid topology.

Topology approval does not equal visual approval. The clone must be exposed through an explicit editor-only preview before production promotion.

## Retained and retired approaches

Retained as evidence:

- legacy replacement-face, strip, and patch construction;
- overlap classification;
- contained-owner and corrected-clone diagnostics;
- source graph, selection, width, corner, and provenance utilities.

Retired as the intended production direction:

- repairing independently emitted patch/replacement overlaps one category at a time;
- universal source-vertex patch centres;
- accepting area conservation without exact shared-boundary incidence;
- treating broad geometric proximity as proof that the original source edge survives;
- arbitrary open-edge closure.

## Production promotion gate

Production promotion requires two explicit approvals:

1. All representative diagnostic clones pass the full topology and geometry gate.
2. The editor-only plane-cut preview is visually approved across representative masses and control extremes.

Only then may the plane-cut result replace the current live path. Removal or quarantine of superseded legacy code is a later cleanup decision, not part of topology promotion.
