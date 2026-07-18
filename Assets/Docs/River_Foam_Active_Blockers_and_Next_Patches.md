# River Foam Active Blockers and Next Patches

## Current status

`RG-METRIC-P2` through `RG-METRIC-P10` are Unity-validated and closed. `RG-METRIC-P11` completed the full mechanical and consistency audit without finding a production defect.

`RG-METRIC-P12 — Fixed-metric activation and candidate evidence` is active in Unity. The first Medium/Quality Default fixed candidate (`dx≈0.1495 m`, `dy=0.15 m`) passed descriptor, cache, topology, CFL, curvature, memory, and steady-state runtime evidence.

`RG-METRIC-P12a` proved that ordinary previous/current interpolation traversed the complete material-step interval, but Unity visual review still showed dead Layer C edge cells repeatedly reappearing in both Material Presence and Remaining Life. The follow-up code audit found two concrete ownership defects and one evidence gap:

- automatic source events re-rasterized cumulative Build/Hold/Release coverage after transport and lifecycle, so a dead covered cell could receive fresh Presence, life, and pattern moments;
- the previous committed presentation alias could be overwritten when transport used an even number of ping-pong substeps;
- the P12 lane report measured cell-centre intent, while actual lateral transport averages adjacent velocities at faces and could cancel materially.

`RG-METRIC-P12b — Deposit-once automatic sources, stable committed-state ownership, and effective lateral-flux evidence` is mechanically implemented and awaiting Unity validation. It changes ownership as follows:

- automatic sources use the positive difference between current and previous Build coverage as a deposition gate, then merge the current authored source target at that newly advancing frontier;
- Hold and Release retain event scheduling but deposit no new material and never delete material;
- already-born material belongs exclusively to transport and lifecycle;
- one dedicated packed-state presentation texture preserves the exact previous committed state for any substep parity;
- the existing transport metric buffer now records material-weighted lateral face speed and signed Presence-area movement;
- generated-lane diagnostics now include face intent, opposing-neighbour cancellation, and cancellation ratio;
- every report retains disk output and its adjacent clipboard-copy action.

Excessive total Final Foam coverage remains pinned for the later layer-by-layer P13 tuning pass and is not altered by P12b.

P12 changes the source default for existing and new Rivers to:

```text
Grid Mode: Fixed Metric
Fixed Cell Size: Quality Default
Low: 0.25 m
Medium: 0.15 m
High: 0.10 m
```

The Inspector also exposes explicit `0.25`, `0.20`, `0.15`, and `0.10 m` candidates plus direct `Legacy Normalized Across` rollback/A-B selection.

Changing Grid Mode or the resolved Fixed Cell Size deliberately invalidates the active Foam resources. A cache built for another descriptor is rejected by the existing cache contract; rebuild the assigned cache explicitly in Edit Mode for the selected candidate. No automatic cache write, scene/prefab/material migration, or test-only shadow runtime is added.

## P12 implementation ownership

P12:

- selects either the fixed or legacy descriptor at the real allocation gate;
- tracks mapping and candidate-size ownership through resource-current, restart, and dirty-notification paths;
- preserves the selected descriptor in the existing cache package/fingerprint contract;
- keeps all migrated source, topology, transport, replacement, film, shape, debug, and production-render implementations unchanged;
- updates the P9 endpoint to validate the authored active selection instead of requiring legacy;
- adds one Play Mode P12 snapshot that reuses existing steady-state accounting and reports descriptor, cache, initialization, topology, CFL, Jacobian, curvature, memory, dispatch, cell, substep, visibility, and CPU-submit evidence;
- leaves visual acceptance to direct Game/Scene review.

No compute shader, HLSL include, render shader, topology generator, source recipe, resource declaration, kernel, persistent texture/buffer, scene, prefab, material, or cache asset is changed by the source patch.

## Active blocker

P12 cannot be closed until P12b Unity validation proves the corrected source and committed-state ownership in the real fixed candidate. The remaining questions are behavioral and measurable; the coordinate migration itself is complete.

Required Unity sequence:

1. Apply P12b and require zero C# errors and zero shader/compute errors or warnings. No cache rebuild is expected because the descriptor, cache schema, topology recipe, and serialized River data are unchanged.
2. Enter Play Mode and inspect `Material Presence` and `Remaining Life` around previously eroded automatic-source regions. Dead edge cells must not repeatedly reappear merely because the old event is in Hold or Release.
3. Inspect `Automatic Birth Sources`: Build should show only the newly revealed deposition frontier; Hold, Release, and Rest should be black because those phases no longer deposit material.
4. Under `Actions → Foam Cache & Validation → P12 Candidate Evidence`, capture at least five representative seconds, write the snapshot, and use the adjacent clipboard-copy action. The report must pass source deposit-once evidence, distinct committed-state ownership, and finite face/flux metrics.
5. Rerun the P9 consumer regression once for the candidate intended to continue and use its clipboard-copy action.
6. Only after the ownership fix is visually accepted, compare the practical fixed/legacy or fixed-size candidates needed for selection.

Do not retune source recipes or birth budgets merely to conceal a coordinate or lifecycle defect. Overall aesthetic Foam quantity remains P13 work.

## Next patch after P12 evidence

`RG-METRIC-P13 — Final tier selection, tuning, cache freeze, and contiguous Stage 1 baseline closure`.

P13 will choose the accepted quality/cell-size policy from P12 evidence, make any justified final tuning, rebuild/freeze the accepted caches, remove rejected temporary candidate guidance, and close the contiguous fixed-metric Stage 1 baseline.
