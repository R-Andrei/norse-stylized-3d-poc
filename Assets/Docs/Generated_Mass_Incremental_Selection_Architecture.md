# Generated Mass Incremental Feature Selection Architecture

Status: canonical and frozen
Architecture ID: GM-SEL.1

## Purpose

This document owns the certified interaction between corner chipping and ordinary bevel selection. Surface-response work is downstream and must not alter this contract without new failure evidence.

## Selection philosophy

- Corner chipping has priority over ordinary bevel preservation.
- A certified chip is not rolled back because it prevents bevels.
- Ordinary candidates are discovered on the accepted post-chip topology.
- Ordinary bevel selection is opportunistic, not preservational.
- Zero surviving ordinary bevels is valid.
- Incompatibility keeps the higher-ranked candidate and discards one lower-ranked non-mandatory candidate.
- Exact ties use deterministic existing identity ordering.
- No combinatorial subset maximization is required or active.
- Every reduced set requires complete geometry, topology, mesh, normal/tangent, render-channel, and performance certification.

## Production algorithm

```text
base mass
→ certified corner transactions
→ topology rebuild
→ ordinary candidate discovery and ranking
→ viability/width constraints
→ complete construction and certification
→ on failure discard exactly one lower-ranked ordinary candidate
→ retry until certified or zero ordinary candidates remain
```

Each retry removes one new ordinary candidate, so the process is bounded by candidate count.

## Mandatory candidates

Mandatory feature-owned candidates, including required chip-cap transitions where applicable, are not removed by the generic lowest-ranked fallback. Failure to certify mandatory geometry invokes the explicit production fallback defined by `Generated_Mass_Surface_Response_Architecture.md`.

## Retained production telemetry

- ranked discard attempts;
- ranked discard applied;
- ranked discard evidence;
- cluster width reduction evidence;
- resolved/unresolved conflict status;
- victim/foreign identities and coverage evidence.

## Retired architecture

The conflict-frontier solver, subset-state search, winning-depth telemetry, frontier budgets, and diagnostic Phase 6B approximate simulation are retired and must not be reintroduced without demonstrated necessity.

## Validation status

The closure suite passed all focused pathological cases and the final full regression matrix documented in `Generated_Mass_Incremental_Selection_Implementation_Plan.md`.

## Downstream boundary

Normals, tangents, packed feature channels, and shared material responses consume the accepted final geometry. They must not change ranking, candidate viability, or certified subset selection.
