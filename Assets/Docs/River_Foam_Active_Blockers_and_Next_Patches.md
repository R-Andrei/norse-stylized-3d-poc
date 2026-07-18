# River Foam Active Blockers and Next Patches

## Current status

`RG-METRIC-P2` through `RG-METRIC-P10` are Unity-validated and closed.

`RG-METRIC-P11 — Full Mechanical and Consistency Audit` is mechanically verified and complete. It found no fixed-metric production defect and therefore changes documentation only.

The latest Unity 6000.5.0f1 endpoint evidence remains the post-P10a P9 comprehensive report:

```text
Film source actual GPU aggregation: PASS
Visual occupancy physical geometry: PASS
Shape/film structural mapping actual GPU: PASS
Production/debug physical-point mapping: PASS
Kernel/resource and unrelated-render ownership: PASS
Live runtime state remained untouched: PASS
Diagnostic cleanup and disabled bindings: PASS
Assigned cache remained unchanged: PASS
Overall: PASS
```

The accompanying Inspector capture confirms the P10 organization and the expected Edit Mode post-cleanup state. The active runtime coordinate mapping remains:

```text
LegacyNormalizedAcross
```

A complete `FixedMetricLattice` candidate is prepared but deliberately inactive. Candidate activation, visual comparison, and performance selection remain P12 work.

## `RG-METRIC-P11` closure

### Audit objective

Verify the complete post-P10a fixed-metric dependency chain without changing production behavior:

- C# 9 syntax, namespace/import, invocation, and duplicate-signature consistency;
- CPU/GPU descriptor lanes and structured-buffer ABI;
- compute kernel names, order, thread groups, bindings, and declared resources;
- cache format, generator contract, descriptor serialization, and fingerprint identity;
- fixed-versus-legacy ownership across topology, routing, sources, transport, replacement, film, shape, debug, and production rendering;
- stale normalized structural-Y reconstruction and duplicate spacing ownership;
- Inspector action paths, endpoint reports, phase status, and canonical documentation;
- scope protection for scenes, prefabs, materials, cache assets, shaders, compute, HLSL, serialized fields, and `.meta` files.

### Result

```text
River C# files audited:                  89
C# parser configurations:              356
C# syntax nodes inspected:       2,043,155
Methods indexed:                      1,641
C# 9 multiline interpolation defects:     0
Missing known namespace/imports:           0
Duplicate exact method signatures:         0

Compute/HLSL/shader files balanced:    24 / 24
Local include references resolved:     26 / 26
Foam kernels:                          23 / 23, exact order
C# kernel lookup parity:               23 / 23
Structured-buffer ABI contracts:      10 / 10
Descriptor ABI lanes:                   5 / 5
Literal Foam shader properties:       207 / 207 declared

Cache payload format:                       3
Cache generator contract:                   4
Production stale structural-Y formulas:     0
Unowned direct spacing derivations:          0
Executable or serialized P11 changes:        0
```

The only normalized structural-Y formula found outside explicit compatibility ownership is the expected closed P6 diagnostic comparison. All direct spacing derivations are accounted for as legacy compatibility branches, descriptor-owned fixed branches, non-grid authoring spacing, or diagnostics.

Two pre-existing mixed-line-ending River source files remain byte-identical to the supplied baseline and are unrelated to this migration. P11 does not normalize or touch them.

### Exact P11 repository scope

Documentation only:

1. `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
3. `Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
4. `Docs/River_Foam_Stage6_Architecture.md`
5. `Docs/River_Rendering_Roadmap.md`

No C#, compute, HLSL, shader, resource, kernel, Debug View, serialized River field, cache payload, cache asset, scene, prefab, material, asset, or `.meta` file changes.

## Active blockers

There is no open P2–P11 implementation blocker.

Fixed-metric activation remains intentionally blocked by process, not by a known defect: P12 must compare the legacy and fixed candidates in Unity before the active mapping can change.

## Next patch — `RG-METRIC-P12`

**Objective:** activate fixed-metric candidates under explicit test control, compare them against the accepted legacy baseline, and select the runtime quality/tier policy using visual and measured performance evidence.

Required work:

1. expose or use the already-planned candidate activation path without silently changing existing serialized River content;
2. generate/install the required fixed-metric candidate cache through the existing explicit cache workflow;
3. compare legacy and fixed candidates at identical camera, flow, source, obstacle, and rendering settings;
4. test straight and curved reaches, asymmetric banks, odd dimensions, padded endpoints, obstacles, forward/reverse flow, static camera, and moving camera;
5. inspect production Foam plus the existing topology, routing, material-state, film, shape, and physical-coordinate debug paths;
6. measure initialization, dispatch cadence, GPU/CPU cost, memory, cell count, substeps, CFL, and candidate-cache size under representative active-chunk counts;
7. select or reject candidate tiers without changing source recipes merely to hide coordinate defects;
8. record every accepted default and any required P12 correction before P13 tuning/cache freeze.

P12 must stop if activation requires a scene/prefab/material migration not explicitly approved, if legacy content changes before comparison, if a coordinate/debug parity gate fails, or if measured cost violates the project budget.

## Remaining program

```text
P12  fixed-metric candidate activation, visual comparison, and performance sweep
P13  final quality-tier selection, tuning, cache freeze, and Stage 1 closure
```

## Non-goals until P12/P13

- no source-recipe retuning before candidate parity is established;
- no birth-budget or cadence change solely to compensate for coordinate differences;
- no unrelated topology, transport, film, shape, or water-render redesign;
- no performance claim based only on one Editor diagnostic duration;
- no unapproved scene, prefab, material, or cache-payload migration.
