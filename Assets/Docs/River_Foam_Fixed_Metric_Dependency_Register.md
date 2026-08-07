# River Foam Fixed-Metric Grid Migration — Dependency Register

## 1. Document identity

- Date: 2026-07-18
- Work type: Dependency audit and migration tracking register
- Implementation status: Fixed-metric migration is closed for the current milestone. The P12d Unity matrix selected `0.15 m`; P12t Chipping/Strands is frozen; P13A material packing and P13F/P13G automatic/Object source ownership are accepted. The current open issue is not a coordinate-grid migration blocker: Final Foam appears to hide more material than same-frame Layer C Presence and Remaining Life evidence implies, requiring a Layer E rendering audit.
- Source snapshot: user-supplied `Assets(72).zip` with accepted P9a, P10, and P10a overlays; no `.git` metadata is present
- Prior design sources:
  - `River Foam Fixed-Metric Resolution Handoff`
  - Follow-up audit response accepting the coordinate-contract corrections
- Persistent repository changes: canonical documentation; validated P2–P9 descriptor, cache, topology, routing, source, transport, replacement, film, shape, and production-render foundations; P10/P10a read-only diagnostic/Inspector presentation cleanup; P11 audit closure; P12 authored selection/allocation/candidate evidence; P12a committed-state interpolation, Motion Lane evidence, and report clipboard convenience; P12b parity-safe committed presentation, nonpersistent deposit-once ownership, and effective lateral face/flux evidence; P12c restored persistent Object Arc/Semi-Arc lifecycle plus warning-free inline source selection; P12d one-button nonserialized runtime candidate/lateral sweep ownership; P12e optional Presence-amplitude rendering plus bounded higher-order packed-state transport; rejected P12f hardened-mask edge detection; and partially accepted P12g single-contour/direct-carve ownership, rejected P12h reach-derived admission, approved P12i exact-mask ownership, rejected P12j clean binary eligibility, and mechanically implemented P12k exact pre-Chip rendered-mask ownership; later accepted P12t/P12u/P13A–P13G material, rendering, reveal, and source ownership; Weather receiver regression closure; and the accepted S3.1E.3 shared shoreline-motion contract. Historical rejected patch records remain for provenance.

## 2. Purpose

This document is the standalone dependency checklist for migrating River Foam from its current grid—fixed approximately in downstream metres but normalized across each local river row—to a fixed-metric, centreline-relative river-space lattice.

It identifies every dependency discoverable by static inspection of the supplied source snapshot that must be:

1. updated during the migration;
2. resolved by an explicit design decision;
3. regression-tested even if its code remains unchanged; or
4. deferred explicitly to the later strip/pooling architecture.

This is not an implementation plan and does not authorize code changes.


### 2.1 Current implementation disposition

Patch 04 establishes deterministic cache ownership of the coordinate contract while the active field remains `LegacyNormalizedAcross`. It does not activate the fixed-metric candidate.

Implemented dependency items through P4:

- immutable descriptor identity, dimensions, requested/resolved spacing, lattice fields, and deterministic signature;
- active legacy allocation assigned exclusively from the descriptor;
- exact one-strip candidate calculation from domain length and maximum asymmetric surface widths;
- explicit hardware-dimension failure without silent metric scale reduction;
- fixed-lattice cell-centre, fractional, nearest, containing, global/local-Y, allocated-boundary, and valid-length conversions;
- fixed-lattice metric-position generation and independent valid/out-of-bank mask generation;
- descriptor-owned X metric-row spacing/centres and prepared fixed `dy` metric-row behavior;
- provisional Foam-only quality-candidate mapping;
- five-lane C#/HLSL descriptor ABI, still intentionally unbound;
- active/candidate read-only descriptor diagnostics;
- payload format `3`, generator contract `2`, and generation/combined fingerprint contract `2`;
- complete descriptor serialization, deterministic reconstruction, metadata parity, and generation fingerprint ownership;
- specific rejection of format-2/generator-1 normalized-lateral cache products;
- explicit stale-grid, unsupported-contract, metadata, domain, obstacle, generation, and combined-key failure states;
- contiguous cache-limit enforcement without changing physical cell scale;
- descriptor-aware explicit preparation, development persistence, startup resolution, installation, and release preflight;
- Editor-only deterministic assertions for descriptor candidates, cache contract classification, and asymmetric/boundary conversions.

Current remaining program dependencies:

- no open fixed-metric coordinate or cache-contract migration blocker remains;
- diagnose the first render stage where same-frame Layer C Presence/Remaining Life ceases to match visible Final Foam;
- keep cache rebuilds explicit and descriptor-driven; a stale cache must report `PreparationRequired / GridDescriptorMismatch`, while live motion/render controls must not change the immutable descriptor;
- complete final performance profiling and any deliberate cache freeze only after the rendering-visibility issue is understood.

The read-only cache metadata Inspector deferred during P4 now exists in `StylizedRiverFoamTopologyCacheAssetEditor.cs`; no P4 metadata-presentation blocker remains.


### 2.1.1 Patch 06 dependency disposition

P6 closes these inactive fixed-metric dependencies while preserving active legacy behavior:

- Motion Lane physical coordinate generation, 32-m downstream basis, 10-m lateral reference span, descriptor-owned metre-to-cell scroll, and 0.20/0.40-m smoothing offsets;
- obstacle-routing approach, closure, contact, lateral margin, centre dead-band, and minimum corridor widths under a physical unit policy, with zero downstream release;
- physical Foam `(s,n)` to Disturbance normalized UV conversion for Pressure, Static Wake, Wake, and Ripple, independent of external texture dimensions;
- descriptor-aware Motion Lane/routing/obstacle renderer-debug UV without migrating normal production Foam sampling;
- one user-triggered comprehensive report with assigned-cache mutation proof.

Disturbance allocation, quality, generation, and renderer ownership remained unchanged. The corrected single P6 report reached live `Ready`, proved current-cache installation without build/write, validated all P6 contracts, proved cleanup/binding disablement, and returned `Overall: PASS`. P7 source, P8 transport/replacement, and P9 production-render consumers subsequently closed.


### 2.1.9 Patch 12b/P12c dependency disposition

P12b established three accepted foundations: nonpersistent current-minus-previous deposition ownership, one parity-safe previous-committed packed-state texture, and effective lateral face/flux evidence. Unity confirmed the broad original Layer C back-and-forth flicker was solved, but rejected P12b's global deposit-once application because Object Contact Arc/Semi-Arc became silent after their Build frontier completed. D3D11 also reported sixteen definite-assignment warnings from the extracted source-contribution helper.

P12c preserves the accepted P12b foundations and restores the documented source-family split:

- the CPU/GPU source-event ABI remains eight `float4` lanes / 128 bytes and still carries current plus previous phase/progress;
- Shore Ribbon, Inward Wash, Object Contact Fleck, and the three Free-Water families retain `max(0, currentCoverage - previousCoverage)` deposition permission and current absolute source-target merging;
- Object Contact Arc/Semi-Arc again resolve phase `0` Build, phase `1` Hold, and phase `2` Release and dispatch every active material tick;
- Arc/Semi-Arc use their unchanged phase-shaped evaluator contribution directly: cumulative Build, complete Hold, progressive Release, then no event during Rest;
- moving or retracting nonpersistent coverage never deletes material; manual ellipse, compound, segment, and probe injections remain unchanged;
- the previous-committed texture, 27-value transport metric buffer, face-level lane evidence, cache contract, serialized fields, kernels, resources, and source recipes are unchanged;
- the extracted `FoamEvaluateAutomaticSourceContribution` helper is removed and its existing evaluator selection is inline, matching the pre-P12b D3D11 definite-assignment structure;
- P7 validates hybrid source ownership and P12 reports the same ownership alongside committed-state and lateral evidence.

Unity accepted the P12c Object Build/Hold/Release/Rest behavior and warning repair. The existing P7/P9 endpoint reports remain available for final selected-candidate closure.

### 2.1.10 Patch 12d dependency disposition

P12d adds one explicit Play Mode sweep over the real production runtime:

- fixed spacings `0.25`, `0.20`, `0.15`, and `0.10 m`;
- lateral ratios `0`, the authored value, and `1` at every spacing;
- nonserialized effective spacing/lateral overrides only while the suite is active;
- diagnostic-only transient topology generation for test descriptors, with no assigned-cache read, write, replacement, or serialization;
- deterministic source/material reset, two-second warmup, and at least five seconds/30 frames of accounting per case;
- descriptor/topology/CFL/Jacobian/curvature/memory/work and effective lateral face/movement evidence;
- assigned-cache metadata/payload proof and authored-runtime restoration after completion, failure, or cancellation;
- one combined disk report plus an adjacent clipboard-copy action.

The sweep freezes the initialization Motion Time across all cases so topology generation does not vary merely because later matrix cases start later in wall-clock Play Mode. It does not select a visual winner, retune Foam, write serialized River state, or replace the final P7/P9 endpoint checks.


### 2.1.11 Patch 12e dependency disposition

P12d machine evidence proved that the lateral path is connected and stable, while visual review selected `0.15 m` and exposed donor-cell widening of thin Layer C strips. P12e adds two independent A/B decisions without changing allocation, cache, topology, sources, Film, Shape, kernels, textures, buffers, or dispatch count:

- `Material Transport Scheme` is serialized under `Foam > Runtime & Quality`; `Donor Cell` is the exact compatibility default, while `TVD Superbee` uses bounded monotonic interior-face reconstruction for the complete packed Presence/life-moment/pattern-moment vector;
- `Presence Footprint` is serialized under `Foam > Layer E — Rendering > General Composition`; `Current` is the exact compatibility default, while `Presence-Amplitude` prevents the resolved base mask from exceeding raw committed Presence before existing opaque pattern/lifecycle processing;
- both options are rebound during ordinary runtime updates and can be switched in Play Mode without cache rebuild or resource reallocation;
- P12 snapshot and sweep reports label both selections so copied evidence is attributable;
- TVD is an experimental diffusion-reduction option, not a formal guarantee that support can never grow under every merge, bend, or divergent velocity.

Unity C# compilation, D3D11 import, GPU runtime behavior, comparative cost, and visual acceptance remain authoritative and pending.

### 2.1.11A Material-contract simplification dependency freeze — `RIVER-FOAM-MATERIAL-C0`

C0 records the dependency boundary required before removing the P12 transport/visibility A/B surface. No runtime implementation changes occur in C0.

Accepted retained baseline:

```text
Bulk-Phase Residual TVD + Lifecycle-Faithful + Coverage-Only
= C × P × L Baseline
```

Future ownership becomes one `Material Contract` selector. C1 initially exposes only `C × P × L Baseline`; C2 later adds `Life Only`.

Safe-removal dependency rules for C1:

| Legacy surface | C1 disposition | Dependency rule |
| --- | --- | --- |
| Donor Cell selectable mode | remove selector/exclusive branch | retain donor/upwind mechanics still called by Bulk-Phase Residual TVD |
| TVD Superbee selectable mode | remove selector/exclusive branch | retain Superbee reconstruction because Bulk-Phase Residual TVD currently uses it |
| Bulk-Phase Residual TVD | retain, make unconditional baseline transport | preserve phase, integer shift, residual subtraction, current single-dispatch/no-extra-field contract |
| Concentration + Lifetime | remove selector/exclusive render branch | retain only helpers proven used elsewhere |
| Lifecycle-Faithful | retain, make unconditional baseline visibility | preserve current production arithmetic |
| Presence-Amplitude | remove selector/exclusive render/chipping/authoring plumbing | retain only helpers/properties proven shared with Coverage-Only |
| Coverage-Only | retain, make unconditional baseline footprint | preserve current production arithmetic and current packed material state |

C1 must not change Coverage, Presence, Remaining Life, or Material Pattern packing/transport semantics. Its acceptance boundary is exact behavior equivalence with the pre-C1 combination `Bulk-Phase Residual TVD + Lifecycle-Faithful + Coverage-Only`.

C2 owns the later material-state redesign. Its accepted conceptual direction is Remaining-Life-only binary cell material. Fractional residual-TVD transport dependencies must be re-audited before C2 implementation; they are not candidates for blind deletion in C1.

### 2.1.12 Patch 12f/P12g/P12h dependency disposition

Unity rejected P12f. `preChipMask` is produced by two independent hardening ramps, and derivative normalization detected both as edges. The production edge path also multiplied every candidate by the narrow edge band at each fragment, clipping connected candidates into partial stripes.

P12g established the accepted mode-specific eligibility/direct-carve correction; P12h narrows only its production admission:

- Current mode retains the accepted post-P12e contract exactly: `preChipSoftVisibility`, edge start `0.06`, derivative normalization, edge-width semantics, per-pixel candidate × edge-band selection, visible-support multiplication, interior access, and soft-mask Chip reconstruction;
- Presence-Amplitude also uses monotonic `preChipSoftVisibility`, but starts at calibrated soft value `0.148228`, where the unchanged hardening function first reaches the existing `preChipMask = 0.08` rendered-support boundary;
- candidate-independent `Chip Eligibility Composite` remains a narrow exterior permission band;
- Presence-Amplitude production keeps the unchanged analytical candidate field but limits its edge-attached permission to Chip Edge Width plus one bounded projected candidate reach. P12g's extra second reach is rejected because it admitted broad interior regions not represented by the eligibility band;
- Presence-Amplitude applies admitted Chip removal directly to the hardened pre-Chip mask, while Current retains the accepted soft-mask reconstruction;
- the existing `_FoamPresenceFootprintMode` uniform selects both paths; no new property, binding, control, resource, kernel, dispatch, serialized field, cache contract, source behavior, Film, Shape, or Layer C state is introduced;
- Presence-Amplitude remains `baseMask = min(baseMask, presence)` with no approved compression or threshold retuning.

P12h is rejected because even one projected reach remained a second permission mask. P12i supersedes it with literal mask ownership:

- Presence-Amplitude edge selection is exactly `chipCandidateField * chipEligibility.edgeBand`;
- Presence-Amplitude Interior Access is zero and cannot authorize removal elsewhere;
- `chipProductionSelection <= chipEdgeEligibility` is an invariant for every fragment;
- direct hardened-mask carving remains, but only for the already eligibility-clipped selection;
- Current retains its accepted edge and Interior Access behavior exactly.

Mechanical validation must prove exact Current-mode and eligibility byte/model equivalence, one-reach permission as a strict subset of P12g, unchanged direct-carve boundedness, complete signature/call stability, and unchanged protected code/resources/properties. Unity must prove warning-free import, the unchanged one exterior yellow contour, coherent magenta bites attached to nearby eligible edges, no broad detached interior removal, and unchanged Current output.

### 2.1.2 Patch 07 dependency disposition

P7 closes the following inactive fixed-metric source dependencies while preserving exact active legacy behavior:

- centralized global-distance/lateral-metre conversion and descriptor-aware source X/Y bounds;
- explicit compatibility-normalized versus metric-lateral transient manual commands without serialized-field or GPU-stride changes;
- all eight automatic source-family dispatch ranges, including fixed physical Y culling and exact legacy Arc/Semi-Arc and Free-Water formulas;
- Shore Ribbon compatibility-cell thickness/variation resolved to source-local metres only for fixed metric;
- descriptor-aware manual ellipse, compound, and segment placement and bounded Y dispatch;
- fixed physical isolated-probe patch/gap layout with exact legacy percentage/cell layout retained;
- descriptor-aware automatic/manual cell centres and domain clipping;
- one comprehensive report that validates all source families, manual commands, coordinate/probe mapping, production/debug evaluator identity, lifecycle/capacity invariants, cleanup, and cache immutability.
- row-union ownership for normalized compatibility ellipse/compound/segment commands on width-varying rivers, while metric commands retain fixed lateral-metre bounds; anchor equivalence is required, rectangle equality is not;
- Shore/Wash fixed range ownership across every actual candidate row and padded longitudinal endpoint, preventing endpoint-clamped width under-bounds;
- independent `ClearRange` full-Y ownership and full automatic GPU-lane parity, including build/hold/release progression values.

P7 did not activate fixed allocation, retune source recipes, scale birth budgets, migrate transport/film/shape/rendering, or change serialized River data. P8 and P9 subsequently closed those consumer migrations. Birth-density/capacity scaling remains a P12 decision.

### 2.1.3 Patch 08 closed dependency disposition

P8 owns the inactive fixed-metric persistent-material and replacement dependencies while preserving exact active legacy behavior:

- descriptor simulation bounds and `dx/dy` physical finite-volume interpretation;
- fixed curvilinear Jacobian correction for cell area, lateral-face length, and downstream CFL;
- separate downstream/lateral/total CFL evidence with the existing target and substep hard limit;
- conservative packed Presence/life-moment/pattern-moment transport and endpoint outflow under forward/reverse flow;
- complete previous/current descriptor ownership for generated-topology transitions;
- exact integer-aligned fixed-lattice persistent-state remap with deliberate clear for unsupported mappings;
- one dirty-time remap kernel using existing held/new state textures and no persistent allocation;
- one comprehensive live P8 report with cleanup and cache immutability.

P8 closure evidence is complete. P8a corrected the lateral descriptor-lane consumer, and P8b corrected the stale topology-validator symbol. The final Unity report copied all 1,491 expected overlap cells, cleared 863 exterior cells, reported zero remap mismatches, passed transport/CFL/curvature/topology/resource/cleanup/cache gates, and ended `Overall: PASS`.

P8 did not activate fixed allocation or change source budgets, cadence, topology generation, routing, Motion Lane, Disturbance allocation, scenes, prefabs, materials, cache assets, or serialized River fields. P9 subsequently closed film, shape, and production-render consumers. Activation/performance/final tuning remain P12.

### 2.2 Patch 05 closed dependency record

Patch 05 owns the following dependency closures while preserving the active legacy mapping:

- descriptor-authoritative CPU metric positions and metre-to-cell addressing for Major, Connector, Pocket, free-water, and prepared-path topology work;
- physical shoreline coverage with legacy-exact behavior and a provisional fixed-metric 0.10-metre feather baseline;
- exact-mesh obstacle candidate intervals and physical 3x3 per-cell sampling;
- descriptor-aware P5 GPU cell centres for current shore edges, evolving generated topology, topology capture/composition, and exact obstacle occupancy;
- immediate per-dispatch descriptor binding for P5-owned kernels only;
- cache capture/readback dimensional parity without a payload or generator-contract change.

Patch 05 explicitly does not close routing, Motion Lane, Disturbance same-point sampling, automatic/manual source rasterization, persistent transport, topology replacement, film, shape, or production rendering. Those remain owned by P6-P9.

Patch 05 implementation disposition:

- CPU topology field-space conversion is descriptor-aware for Major, Connector, Pocket, free-water, prepared-path, and local-mask work;
- the legacy public generator APIs remain as compatibility wrappers, while active runtime generation passes the immutable descriptor directly;
- boundary/fluid coverage shares one legacy-exact or fixed-physical feather contract;
- exact obstacle preparation uses descriptor X/global-Y ownership and nine in-cell physical samples;
- metric rows expose descriptor `dx/dy`, and topology-owned compute sequences bind the descriptor immediately before dispatch;
- dedicated P5 HLSL helpers migrate only current shore edges, evolving topology, topology capture/composition, and topology metrics; source, transport, film, and production-render helpers remain staged;
- all mechanical tests recorded in the canonical plan passed; repeated explicit builds initially exposed same-input payload drift. P5.1-P5.3 subsequently isolated and corrected the hidden topology-phase input, and the final generator-4 reports closed deterministic legacy reproduction before P6.


### 2.3 Patch 05.1 closed diagnostic record

Observed Unity evidence requires a separate diagnostic dependency closure:

```text
Build A = 1,954,946 bytes / 58C8036175508509
Build B = 1,954,518 bytes / 24BCF968B2B94F28
combined inputs for both = F182CD9FCC93A961B19B60CBD53C5639
descriptor for both = descriptor-v1/mapping-0-v0/768212E451E606B9
obstacle source count for both = 5
```

The dependency register therefore treats deterministic-generation observability as a mandatory owner of every later coordinate/cache/generator patch. P5.1 adds:

- exact per-obstacle provenance with hierarchy/component-stable diagnostic keys plus source, owner, and MeshFilter session EntityIds;
- hierarchy, provider/owner type, mesh identity, readability, exact counts and bounds;
- independent local-mesh, transform, provider-world, and direct-world fingerprints;
- provider/direct parity and build/captured-obstacle fingerprint parity;
- explicit source baselines and comparisons stored only under `Library/RiverFoamDiagnostics`, including immediate machine-baseline readback and production-relevant equality proof;
- two independent non-storing cache preparations from one Inspector action;
- complete payload byte/hash/first-difference evidence;
- section-level digests and topology inventory for descriptor, domain, inputs, generation settings, obstacle scalar field, Major, Connector, and Pocket products;
- assigned-cache comparison plus a before/after metadata-and-payload mutation proof;
- final diagnostic classification;
- a read-only cache-asset Inspector with explicitly triggered section analysis;
- copy, Console, and reveal-file actions.

P5.1 implementation is closed. It changed no active mapping, generator algorithm, source behavior, transport, rendering, serialized River setting, scene, prefab, material, or assigned cache asset. The patch added four explicit Editor-only diagnostic files, a read-only cache-asset Inspector, six River Inspector actions, twenty cache sections, a 26-lane versioned obstacle baseline with immediate binary round-trip proof, production-relevant versus provenance-only obstacle comparison, provider/direct/build-capture agreement, assigned-cache mutation proof, complete local report outputs, and canonical documentation. All seven changed/new C# files and both `UNITY_EDITOR` preprocessed forms parsed without syntax errors; the cache asset's fifteen serialized fields remained unchanged; the codec's only production change was `static` to `static partial`; four new GUIDs were unique across 315 supplied metadata files. P5.2/P5.3 then completed the Unity closure and released P6.

Permanent rule: any future deterministic generator, cache, coordinate, source-geometry, or state-migration patch must include the diagnostic and parity evidence required to validate its own behavior in the same patch. Diagnostic ownership may not be deferred.

### 2.4 Patch 05.2 fingerprint/parity dependency disposition

The complete P5.1 report closed the broad determinism question and exposed a narrower contract failure:

```text
live obstacle snapshots = exact
current Build A/B payloads = exact
provider fingerprints = five all-zero sentinels
independent direct fingerprints = five distinct nonzero values
assigned/current first difference = one obstacle scalar texel
classification = Input Fingerprint Gap
```

P5.2 therefore adds or updates these mandatory dependencies:

#### Shared exact-geometry identity — **U/T**

- `Game/Procedural/Core/GeneratedGeometryStableFingerprint.cs`
  - reserve all-zero as invalid;
  - reject all-zero exact-world computation results;
  - expose an explicit `IsDefault` check for all providers and consumers.
- `Game/Procedural/Masses/GeneratedMass.cs`
  - mark the four coupled fingerprint cache lanes `[NonSerialized]`;
  - reject restored valid-plus-zero state;
  - recompute before returning;
  - publish refreshed fingerprint, mesh, matrix, and validity only after complete local resolution.

Mandatory tests:

- every provider returns a nonzero identity;
- provider identity equals independent direct exact-world-triangle identity;
- the same equality survives one Editor restart when persistence/hot-reload proof is requested;
- no mesh or transform change is hidden behind an unchanged provider identity.

#### River obstacle identity collection — **U/T**

- `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.GeneratedSources.cs`
  - reject a provider success containing the sentinel;
  - independently verify provider/direct identity during explicit Edit Mode cache validation/preparation;
  - retain prepared-provider-only behavior during normal Play startup.
- `Game/Procedural/Rivers/RiverObstacleExclusionResolver.cs`
  - reject zero source identities;
  - advance the obstacle-source-set aggregate to contract `2`;
  - retain source-order independence through deterministic sorting.

Mandatory tests:

- five current sources produce five nonzero provider/direct-equal identities;
- aggregate identity changes when any source's exact local mesh or transform changes;
- aggregate identity is stable under registry enumeration reordering;
- aggregate construction cannot succeed with any zero lane.

#### Cache compatibility and startup — **U/T**

- `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyCacheCodec.cs`
- `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyCacheAsset.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.TopologyCache.cs`

Historical P5.2 contract changes, now superseded by P5.3:

```text
payload format = 3 (unchanged)
generator contract = 3
format 2 / generator 1 = Legacy Coordinate Contract
format 3 / generator 2 = Legacy Obstacle Fingerprint Contract
format 3 / generator 3 = P5.2 current; P5.3 reclassifies it as Legacy Dynamic Topology Phase
```

Mandatory tests:

- old generator-2 assets are rejected before input validation or installation;
- Play Mode performs zero generation and zero writes for the legacy asset;
- explicit P5.2 Editor rebuild created generator 3; P5.3 requires generator 4;
- rebuilt cache validates and installs exactly;
- cache metadata and payload remain unchanged during diagnostics.

#### Frozen legacy-raster parity — **U/T, Editor-only**

- new `Game/Procedural/Rivers/RiverObstacleExclusionResolver.LegacyParityDiagnostics.cs`
- `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.CacheDiagnostics.cs`

The diagnostic compares the frozen pre-P5 normalized-lateral raster and the P5 descriptor-owned legacy path against the same mesh and river state. It owns candidate bounds, exact accepted sample intervals, water parameters, cell order/offsets, CPU occupancy, duplicate counts, and first mismatch reconstruction.

It is not a production dependency. It must remain excluded from player builds and must never become a runtime fallback.

#### One-report validation ownership — **U/T, Editor-only**

- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.CacheDiagnostics.cs`
- `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`

The primary report performs five non-storing preparations and includes:

- provider/direct identity;
- obstacle stability;
- input-key stability;
- payload and section determinism;
- CPU emitted cells versus GPU obstacle scalar;
- frozen pre-P5 versus P5 legacy parity;
- assigned-cache mutation proof;
- one final ledger.

Primary output:

```text
Library/RiverFoamDiagnostics/<river>_LatestP52ComprehensiveValidation.txt
```

The P5.2 closure uses at most two reports: Report 1 is run before rebuilding and permits the expected generator-2 legacy stage; after one explicit rebuild and Editor restart, Report 2 requires the assigned generator-3 payload and metadata to match Build 1 exactly while re-proving provider/direct identity across reload. Supplemental reports are requested only when the comprehensive report cannot isolate a detected failure.

#### P5.2 explicit non-dependencies

P5.2 must not alter:

- active field mapping or dimensions;
- topology generation outside exact obstacle identity and Editor parity observation;
- source rasterization, routing, Motion Lane, Disturbance field content, transport, film, shape, or production rendering;
- compute/HLSL resources, kernels, textures, buffers, or dispatches;
- scenes, prefabs, materials, serialized River fields, or generated cache assets in the distributed patch.

This P5.2 closure condition was superseded by P5.3. Both P5.3 comprehensive reports later passed, the assigned cache was rebuilt under generator contract 4, and P6 was released.

Permanent validation rule: every later migration patch must include one comprehensive report, or at most two when a process/reload boundary prevents complete evidence in one transaction. Longer manual validation sequences require a recorded reason proving why report automation cannot capture the evidence.

### 2.5 Patch 05.3 deterministic topology-phase dependency disposition

P5.2 closed provider identity and same-session generation questions, then exposed a cross-reload hidden input:

```text
five builds before restart = identical, obstacle nonzero 587
five builds after restart = identical, obstacle nonzero 590
recorded domain/obstacle/generation/combined inputs = identical
frozen pre-P5 vs P5 descriptor raster = exact
```

The following dependencies become mandatory owners in P5.3.

#### Topology evaluation phase — **U/T**

- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Topology.cs`
  - owns topology phase contract `1`;
  - binds `_FoamTopologyEvaluationTime = 0f` every time topology parameters are configured;
  - retains `_FoamTime = ResolveInitializationMotionTime()` for current-shore/live topology behavior;
  - records the configured topology time for explicit Editor validation.
- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl`
  - declares one scalar `_FoamTopologyEvaluationTime`;
  - adds no texture, buffer, sampler, UAV, kernel, or dispatch.
- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Topology.hlsl`
  - `IsObstacleIntervalSampleInside` must use `_FoamTopologyEvaluationTime` when evaluating exact intervals against `RiverWaterEvaluateSurfaceHeight`;
  - every other `_FoamTime` consumer remains unchanged.

Mandatory tests:

- all five builds report contract `1`, time `0`, float bits `0x00000000`;
- all five payloads and all section digests are exact;
- the result remains exact after explicit rebuild and Editor restart;
- current-shore/live rendering time is not frozen.

#### Cache compatibility — **U/T**

- `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyCacheCodec.cs`
- `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyCacheAsset.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.TopologyCache.cs`

Contract matrix:

```text
format 2 / generator 1 = Legacy Coordinate Contract
format 3 / generator 2 = Legacy Obstacle Fingerprint Contract
format 3 / generator 3 = Legacy Dynamic Topology Phase
format 3 / generator 4 = current
```

Generator-3 assets must be rejected before deserialization, fingerprint comparison, installation, replacement, or Play Mode write. Payload format remains 3 because the byte layout is unchanged; generator semantics change because obstacle acceptance is now canonical rather than live-phase-dependent.

#### CPU candidate / GPU publication ownership — **U/T, Editor-only**

- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.CacheDiagnostics.cs`

The CPU interval list is a conservative candidate set. GPU occupancy is an accepted subset after nine interval tests at topology time zero. The diagnostic must compare ownership rather than require set equality.

Passing contract:

```text
candidate coordinates unique and in range
GPU output length equals field cell count
GPU scalar contains only exact 0/1 values
GPU-only occupied cells = 0
candidate-only cells = permitted and reported
five-build accepted scalar and payload = exact
```

Duplicate or out-of-range CPU candidates remain failures. Candidate-only cells are not failures because they are rejected by the canonical water-height interval test.

#### One/two-report validation ownership — **U/T, Editor-only**

- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.CacheDiagnostics.cs`
- `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`

Primary output:

```text
Library/RiverFoamDiagnostics/<river>_LatestP53ComprehensiveValidation.txt
```

Report 1 may accept generator 3 only as the explicit pre-rebuild legacy dynamic-phase stage. Report 2, after one explicit rebuild and one Editor restart, must classify generator 4 as current and prove assigned payload/metadata exact to Build 1. No third routine report or long manual checklist is permitted unless this comprehensive report cannot isolate a failure.

#### P5.3 explicit non-dependencies

P5.3 must not alter:

- active grid mapping, descriptor dimensions, or fixed-metric activation;
- CPU obstacle candidate generation or frozen pre-P5/P5 descriptor raster parity;
- current-shore animation or normal live water rendering time;
- routing, Motion Lane, Disturbance fields, sources, transport, film, shape, or production rendering;
- kernel list, dispatch dimensions/count, texture/buffer allocation, or serialized River state;
- scenes, prefabs, materials, generated topology cache assets, or unrelated assets in the distributed patch.

Closure result: both P5.3 comprehensive reports returned `Overall: PASS`, and the assigned format-3/generator-4 asset was exact after restart. P6 was released.

### 2.1.4 Patch 09 closed dependency disposition

P9 owns the remaining inactive fixed-metric visual-layer and production-render dependencies while preserving exact active legacy behavior:

- exact full-to-half structural grouping, odd terminal groups, and represented-cell ownership;
- physical-area-weighted film source and bank/padded clipping;
- physical film support spacing and aggregate visual-occupancy finite-volume geometry;
- structural-to-film sampling shared by shape evaluation and renderer debug views;
- descriptor-owned production Foam field UV and visual metre-offset conversion;
- valid allocated/represented clipping without edge saturation;
- production/debug same-physical-point evidence;
- one comprehensive live P9 report with cleanup and cache immutability.

P9 does not activate fixed allocation, change persistent resources, retune source recipes, alter unrelated water rendering, or modify serialized River data. Quality selection and visual/performance candidate sweeps remain P12.

Closure disposition: the final Unity report executed the actual GPU film-source, visual-occupancy, and shape paths with zero mismatches; passed odd-edge/represented-area, production/debug mapping, resource ownership, cleanup, live-state, and assigned-cache gates; and ended `Overall: PASS`. P9a then removed the three D3D11 warning-prone visual-occupancy helper forms without changing the formulas. The post-P9a rerun again ended `Overall: PASS`. No P9 dependency remains open.

### 2.1.5 Patch 10 closed dependency disposition

P10 owns only observability and presentation cleanup:

- publish already-computed split CFL and curvilinear Jacobian/`|κn|` values through the read-only runtime surface;
- show active and prepared-candidate descriptor geometry, contract identity, cell-count comparison, cache state, memory, dispatch, and source-area evidence in existing Inspector diagnostic groups;
- rename the normal cache action group to `Foam Cache & Validation` and align the cache-asset/default diagnostic guidance with that path;
- retain P9 as the visible endpoint regression while collapsing closed P5.1/P5.3/P6/P7/P8 actions under one historical/deep foldout;
- remove obsolete active P8/P9 instructions and align canonical status documents.

P10 adds no serialized data, Debug View, GPU readback, report kernel, periodic logging, production resource, compute/render behavior, cache payload, scene, prefab, or material change. Its primary mechanical audit passed 110/110 checks and an independent final audit passed 35/35 across the exact 11-file scope, 24 changed-file parser configurations, and all 89 River C# files.

P10a corrected one C# 9-incompatible multiline interpolation expression in the Fixed Candidate read-only row without changing its output. Unity then compiled, the supplied Inspector capture showed the intended Foam diagnostics/action organization and expected post-cleanup unallocated Edit Mode state, and the unchanged P9 endpoint report again ended `Overall: PASS`. P10/P10a are closed. Fixed-metric activation remains P12.

### 2.1.6 Patch 11 closed dependency disposition

P11 audited the complete post-P10a dependency chain and found no production defect. The audit covered:

- 89 River C# files across 356 parser configurations, with zero C# 9 multiline-interpolation defects, missing known imports, or duplicate exact method signatures;
- 24 compute/HLSL/shader files, 26 local includes, all 23 Foam kernels in exact order, and exact C# `FindKernel` parity;
- ten structured-buffer ABI contracts, all five descriptor publication lanes, compute/material descriptor bindings, and 207 declared literal Foam properties;
- payload format `3`, generator contract `4`, descriptor serialization, reconstruction, and fingerprint identity;
- production searches for stale normalized structural-Y reconstruction and duplicate spacing ownership;
- active legacy ownership, deferred fixed activation, P8/P9 endpoint presence, P10 Inspector actions, canonical status, and protected serialized-file scope.

No executable, shader, compute, cache, resource, serialized, scene, prefab, material, asset, or `.meta` change was required. P11 changes only the five canonical status documents and releases P12.

### 2.1.7 Patch 12 activation disposition

P12 closes the source-level activation dependency and leaves candidate selection open for Unity evidence:

- serialized River authoring selects `Fixed Metric` or `Legacy Normalized Across`; fixed is the source default;
- fixed candidate size can follow quality or explicitly select `0.25`, `0.20`, `0.15`, or `0.10 m`;
- `ResolveInitializationDimensions` selects the authored descriptor at the real production allocation gate;
- allocated mapping and size participate in resource-current, initialization-restart, dirty-notification, and failed-start recovery contracts;
- the existing descriptor-aware cache package remains authoritative, so a mismatched candidate requires the existing explicit Edit Mode rebuild;
- P9 validates whichever authored mapping is active while retaining all actual GPU film/shape/render checks;
- one Play Mode P12 snapshot reuses existing accounting for comparable descriptor, cache, initialization, topology, transport, memory, dispatch, cell, substep, visibility, and CPU-submit evidence;
- no automatic cache write, duplicate runtime, compute/render formula, resource, kernel, source recipe, scene, prefab, material, or cache-asset edit is introduced.

Unity must now determine whether fixed candidates are visually correct and affordable. P12 remains open until at least one fixed candidate and the intended comparison baseline have complete visual and snapshot evidence, followed by one passing P9 endpoint on the candidate selected for continuation.



### 2.1.7 P12a visual-evidence disposition

The first active fixed candidate passed descriptor/cache/topology/CFL/curvature/resource evidence but exposed two distinct review items. Layer C state changed at the accepted material cadence while Final and Layer C debug presentation selected only the newest committed texture, so conservative whole-cell updates appeared as abrupt edge flicker. P12a uses the already allocated previous/current state pair for ordinary fixed-step interpolation; it does not restore point-velocity residual prediction, add a transport path, or alter stored material.

The apparent lateral-response question is not yet classified as a defect. `Maximum Lateral Speed Ratio` sets the velocity ceiling, while `Lane Advection Ratio` only scrolls the generated route texture downstream. P12a adds range, mean-absolute, RMS, positive, negative, and near-neutral lane evidence to the existing live snapshot. Excessive total foam coverage remains a deliberate P13 tuning item.

## 3. Completeness statement and limits

### 3.1 Verdict on the earlier dependency list

The earlier list was **not exhaustive**. It covered the primary allocation, topology, source, compute, render, and cache path, but it did not fully enumerate several indirect contracts:

- topology morphology and cell-count thresholds;
- motion-lane generation and obstacle-routing morphology;
- the half-resolution visual-occupancy field;
- quality-linked birth budgets and cadence;
- topology replacement and state remapping;
- build preflight and development cache tooling;
- manual injection and isolated-life probes;
- disturbance-field integration;
- resolution-dependent diagnostics and metrics;
- authoring labels and serialized unit semantics;
- runtime reallocation, flow reversal, and domain-change behavior;
- curvature-dependent physical area on wide bends;
- the complete future strip boundary, renderer-indirection, and scheduling contract.

### 3.2 What “complete” means here

Within the supplied source snapshot, this register is intended to be an **exhaustive static dependency inventory** for the fixed-metric migration. It includes:

- all River Foam C# files;
- all River Foam compute and render shader files;
- all direct users of the current CPU field-space helper;
- all externally visible Foam consumers found in the supplied `Game/` tree;
- cache, editor, preflight, diagnostics, scene, river-domain, disturbance, obstacle, and documentation integration points.

It cannot prove dependencies that are absent from the supplied snapshot, created dynamically by external packages, injected through unpublished tooling, or visible only in runtime data. The archive contains no `.git` metadata and no complete project root, package manifest, Library, or current `Editor.log`. A final pre-implementation repository audit must repeat the reference scan against the live workspace.

## 4. Classification legend

| Code | Meaning |
|---|---|
| **U** | Mandatory implementation update for the contiguous fixed-metric Stage 1 |
| **D** | Mandatory design decision before implementation |
| **T** | Mandatory regression or integration test; code change is not automatically justified |
| **F** | Future strip/pooling dependency; not part of contiguous Stage 1 unless separately approved |
| **R** | Mandatory code review because a change is conditional on the final descriptor or unit policy |

A dependency can carry multiple classifications, such as **U/D/T**.

## 5. Migration invariant

Every system must use one authoritative metric-grid descriptor. No subsystem may independently reconstruct lateral Foam UV from local river width or derive spacing from a different length.

Conceptual descriptor:

```text
mappingContractVersion
columnsPer32MetreChunk
resolvedDxMetres
resolvedDyMetres
lateralLatticeOriginMetres
localGlobalYBase
rowCount
fieldOrStripStartMetres
fieldOrStripLengthMetres
validLengthMetres
```

Cell centre:

```text
s = fieldOrStripStart + (x + 0.5) * resolvedDx

globalY = localGlobalYBase + localY
n = lateralLatticeOrigin + (globalY + 0.5) * resolvedDy
```

“Global” means shared by strips belonging to one river or connected river network. It does **not** mean a world-aligned XZ lattice.

## 6. Top-level dependency matrix

| Dependency area | Class | Why it is affected | Minimum acceptance condition |
|---|---:|---|---|
| Grid allocation and dimensions | U/D/T | Current width and height encode two different physical semantics | Resolved `dx/dy`, origins, dimensions, and valid length come from one descriptor |
| CPU field-space conversion | U/T | Y currently means normalized cross-river fraction | CPU metre-to-cell and cell-to-metre round trips match the metric lattice |
| Compute coordinate conversion | U/T | Compute independently reconstructs normalized lateral position | GPU cell centres match CPU positions within tolerance |
| Production renderer sampling | U/R/T | Renderer divides lateral metres by local surface half-width | Render sampling addresses the same lattice as simulation |
| Topology generation | U/R/T | Morphology and placement use current field cells and normalized across values | Generated topology preserves intended physical shapes and valid-bank clipping |
| Boundary generation | U/D/T | Boundary feather and bank support use quality-specific cell counts | Physical boundary thickness is explicitly preserved or deliberately changed |
| Obstacle exclusion | U/T | Mesh occupancy is rasterized into current cell coordinates | Exact-mesh footprint aligns with the metric field at all widths and bends |
| Obstacle routing | U/D/T | Approach, closure, margins, and BFS operate in field cells | Routing envelopes preserve intended physical reach and no forbidden wrap occurs |
| Motion lane | U/D/T | Noise aspect, smoothing, and scroll use field dimensions/cells | Physical lane scale and scroll speed remain stable across river widths |
| Automatic birth-source dispatch | U/T | CPU culls event Y ranges using normalized lateral coordinates | Every event dispatch covers exactly the metric cells intersecting its physical bounds |
| Automatic source geometry | U/D/T | Several widths and feathers are cell-relative | Each parameter is classified as metres, cells, or sampling support |
| Manual injection and probes | U/R/T | Normalized source coordinates and cell dispatch ranges depend on old mapping | Manual strokes, ellipses, compounds, and probes land at correct physical positions |
| Persistent material transport | U/D/T | Current neighbour rows are normalized-fraction neighbours | Transport follows equal lateral metre positions and remains conservative |
| CFL and substep policy | U/T | Resolved `dx/dy` change stability terms | Runtime reports correct spacing, CFL, and substep count for every quality candidate |
| Curvilinear area/face metrics | D/R/T | Wide curved rivers have `J ≈ 1 - κn`, while solver uses `dx*dy` | Approximation bound or corrected metrics are explicitly adopted |
| Topology replacement transition | U/T | Current-to-previous mapping uses old normalized field coordinates | Rebuild/replacement preserves state without lateral jumps or smearing |
| Half-resolution visual occupancy | U/R/T | Film dimensions and represented-cell area derive from the structural field | Layer D remains physically aligned, conservative enough, and visually stable |
| Shape evaluation and boundary application | U/R/T | Full/half field mapping and cell-relative features change | Final shape, breakup, and clipping remain stable at the selected metric scale |
| Disturbance-field integration | T/R | Foam samples independently normalized Pressure/Wake/Ripple fields | Sampling at each metric Foam cell resolves the same world/river point |
| River-domain and geometry inputs | T/R | Centreline distance, normals, left/right widths, and curvature define the lattice | Asymmetric widths, bends, flow reversal, and domain changes remain valid |
| Quality policy | D/U/T | Shared quality enum also controls non-Foam systems | Foam gets a separate metric mapping without altering geometry/disturbance quality semantics |
| Birth budgets and capacities | D/R/T | Fixed events-per-step do not scale automatically with water area | Density and saturation remain acceptable from 5 m through wide-river cases |
| Resource lifetime and reallocation | U/T | Grid descriptor changes invalidate textures, buffers, and cached state | Reallocation is deterministic and state-loss behavior is explicit |
| GPU/CPU data-layout parity | U/T | Descriptor or source structs may gain/change fields | C# and HLSL strides, ordering, and values match exactly |
| Cache package and fingerprints | U/D/T | Existing caches encode normalized-grid products | Old caches miss deterministically and new caches fingerprint every mapping parameter |
| Cache build/preflight tooling | U/R/T | Build requires exact prepared cache artifacts | Editor preparation, release preflight, and stale reasons remain correct |
| Diagnostics and metrics | U/D/T | Many values are texel counts or cell perimeters | Physical area/length is reported where cell counts are no longer comparable |
| Memory/work accounting | U/T | Dimensions and later strips change resource and dispatch counts | Reported bytes, cells, dispatches, and iteration rates equal actual allocations/work |
| Debug views | U/T | Cell overlays and automatic-source views assume normalized Y | Overlays line up with world geometry and expose the metric descriptor |
| Inspector/authoring units | U/D/T | Help text still describes 64/96/128 across-river rows and cell-based widths | Labels and serialized migration semantics are unambiguous |
| Documentation | U | Architecture and roadmap describe the old coordinate contract | Canonical docs and active queue match the implemented design |
| Scene/prefab assets | T only | Existing serialized settings are validation inputs | No raw scene/prefab edit; existing scene continues to load and behave correctly |
| Future strip allocation | F/D/T | Required for local-width scaling and long rivers | Shared global-Y lattice, boundaries, pools, budgets, and renderer lookup are defined |

## 7. Detailed dependency register

### 7.1 Grid ownership, allocation, and runtime state

**Class: U/D/T**

Dependencies:

- quality-to-grid mapping;
- 32-metre chunk quantization;
- valid versus padded downstream length;
- lateral lattice origin and global-Y base;
- texture dimensions;
- maximum texture and cache dimensions;
- structural and half-resolution film dimensions;
- initialization signatures;
- allocated-quality tracking;
- resource rebuild and release rules;
- public field dimension and spacing properties.

Required decisions:

1. Whether requested metric sizes are rounded by changing `columnsPer32MetreChunk` and using `resolvedDx = 32 / columns`.
2. How the lateral lattice phase is selected and kept stable across rebuilds.
3. How far left/right the contiguous Stage 1 field allocates beyond local banks.
4. What happens when fixed metric dimensions exceed hardware or cache limits. Silent spatial degradation is not acceptable.
5. Whether quality changes clear material state or attempt a resampling transition.

Mandatory tests:

- exact 32 m, just under, and just over 32 m lengths;
- final padded chunk;
- asymmetric left/right widths;
- 5, 10, 20, and 40 m widths;
- texture-limit and cache-limit failure paths;
- disabled/enabled lifecycle;
- repeated initialization without leaks;
- quality switching;
- domain-version changes.

Primary files:

- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.RuntimeUpdates.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs`
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.cs`

### 7.2 Canonical CPU field-space mapping

**Class: U/T**

The current helper maps Y texels to `Across01`, then maps that normalized value through each row’s own left/right width. This must become fixed signed lateral metres.

Every direct caller must be migrated or explicitly retired:

- `StylizedRiverFoamConnectorTopologyGenerator.cs`
- `StylizedRiverFoamMajorTopologyGenerator.cs`
- `StylizedRiverFoamPocketTopologyGenerator.cs`
- `StylizedRiverFoamTopologyFieldSpace.cs`
- `RiverObstacleExclusionResolver.cs`
- `StylizedRiverFoamRuntime.Evolution.Connector.cs`
- `StylizedRiverFoamRuntime.Evolution.Major.cs`
- `StylizedRiverFoamRuntime.Injection.cs`
- `StylizedRiverFoamRuntime.Obstacles.cs`
- `StylizedRiverFoamRuntime.Resources.cs`
- `StylizedRiverFoamRuntime.RuntimeUpdates.cs`
- `StylizedRiverFoamRuntime.Topology.cs`

Mandatory mapping tests:

- cell centre to metres;
- metres to fractional cell;
- metres to nearest cell;
- left and right asymmetric widths;
- negative and positive lateral positions;
- exact lattice boundaries;
- out-of-field and out-of-bank positions;
- CPU/GPU parity;
- normal and reversed flow direction;
- strip-compatible global-Y indexing even in one-strip Stage 1.

### 7.3 Metric-row and CPU/GPU ABI contracts

**Class: U/R/T**

`FoamMetricRow`, automatic source-event structs, obstacle interval cells, and compute uniforms are shared CPU/GPU contracts. A new descriptor can be passed as uniforms, embedded in per-row data, or represented through a separate buffer, but the choice must be explicit.

Dependencies:

- `FoamMetricRow` spacing and topology data;
- source-event centre, extents, and cell-relative fields;
- object source `LateralCellSpacingMetres` or equivalent;
- `ShoreRibbonThicknessCells` and variation fields;
- buffer stride constants;
- shader property IDs;
- dispatch dimensions;
- fallback/neutral resources.

Primary files:

- `StylizedRiverFoamRuntime.State.cs`
- `StylizedRiverFoamRuntime.Binding.cs`
- `StylizedRiverFoamRuntime.Compute.cs`
- `CS_RiverFoam.Structs.hlsl`
- `CS_RiverFoam.Resources.hlsl`

Mandatory tests:

- `Marshal.SizeOf`/stride parity where available;
- source-event field ordering;
- metric-row buffer content inspection;
- neutral/fallback binding;
- no stale property reuse after reallocation.

### 7.4 CPU topology generators and topology morphology

**Class: U/D/T**

Topology is not merely positioned on the field; it contains morphology expressed in cells. Changing cell size can alter support widths, connector lengths, pocket rejection, local masks, dilation/erosion, junction continuity, and candidate ranking.

Dependencies:

- metric-position arrays;
- nearest-cell and fractional-cell conversion;
- normalized across anchors retained in topology records;
- local candidate masks;
- half extents expressed in cells;
- connector paths and widths;
- pocket masks and boundaries;
- major/connector/pocket overlap;
- boundary edge-cell constants;
- obstacle-aware topology generation.

Primary files:

- `FoamTopology/StylizedRiverFoamMajorCandidate.cs`
- `FoamTopology/StylizedRiverFoamMajorCandidateGenerator.cs`
- `FoamTopology/StylizedRiverFoamMajorTopology.cs`
- `FoamTopology/StylizedRiverFoamMajorTopologyGenerator.cs`
- `FoamTopology/StylizedRiverFoamConnectorTopology.cs`
- `FoamTopology/StylizedRiverFoamConnectorTopologyGenerator.cs`
- `FoamTopology/StylizedRiverFoamPocketTopology.cs`
- `FoamTopology/StylizedRiverFoamPocketTopologyGenerator.cs`

Required decisions:

- which morphology distances should preserve physical metres;
- which values genuinely represent raster connectivity and should remain cells;
- whether normalized across anchors remain authoring data while raster placement becomes metric;
- whether candidate budgets need area scaling.

Mandatory tests:

- topology continuity at changing width;
- support shape physical dimensions;
- major/connector/pocket counts and area;
- no one-cell gaps introduced by rounding;
- no topology outside banks;
- narrow-river and wide-river equivalence in metres;
- obstacle-adjacent topology;
- deterministic output and cache parity.

#### P5 morphology classification record

| Quantity | Classification | P5 treatment | Required regression evidence |
|---|---|---|---|
| Major candidate `CentroidCells`, half extents, minimum neck, and support feather | Candidate-local raster coordinates | Preserve. The candidate raster is resolution-independent from the Foam field and receives physical scale through `metresPerCandidateCell`. | Identical candidate generation for a fixed candidate seed; physical placed diameter/radii unchanged. |
| Four/eight-neighbour traversal, connectedness, boundary detection, one-cell raster guards | Structural sampling/connectivity | Preserve as cells. These operations define discrete adjacency or conservative sample support, not authored physical width. | No disconnected one-cell seams; deterministic component and boundary identity. |
| Major minimum new coverage and coverage ranking | Physical area in fixed mode; exact legacy cell count in legacy mode | Legacy remains four cells and `cells × 0.01`. Fixed mode uses `0.02 m²` per equivalent coverage unit, giving a `0.08 m²` minimum and area-stable ranking. | Legacy cache bytes unchanged; fixed 0.10/0.15/0.20/0.25-metre candidates resolve equivalent physical acceptance. |
| Maximum Major coverage | Fraction of valid water | Preserve as ratio. Equal-area fixed cells make the current valid-cell ratio a physical-area ratio. | Equivalent covered-area fraction across widths and candidate spacings. |
| Connector minimum Major component | Physical area in fixed mode; exact legacy count in legacy mode | Legacy remains five cells. Fixed mode requires `ceil(0.10 m² / (dx × dy))` cells. | Stable removal of tiny raster fragments across candidate spacings. |
| Major-region to Connector-component fallback association | Physical radius in fixed mode; exact legacy ring count in legacy mode | Legacy retains ten cell rings. Fixed mode selects the nearest labelled cell within `1.50 m`. | Stable component IDs and source-region ownership under fixed candidate spacing changes. |
| Free-water nearest-valid-cell fallback | Physical radius in fixed mode; exact legacy scan in legacy mode | Legacy retains a two-cell X/Y radius. Fixed mode derives independent X/Y search radii from the existing `0.34 m` acceptance radius and still applies the exact metric distance test. | No missed valid sample within 0.34 m; no event slides around an occupied obstacle cell. |
| Opportunity, host, endpoint, variant, relationship, prepared-point, and recycle-anchor counts | Semantic bounded counts | Preserve. These are authored/computational budgets, not spatial widths. | Counts remain bounded and deterministic; performance budget unchanged. |
| Coverage, containment, host remainder, and overlap fractions | Dimensionless ratios | Preserve. | Equivalent ratios for equivalent physical topology. |
| `FreeWaterEvolutionMaskResolution` and prepared `OffsetCells`/`CentroidCells` | Local normalized/candidate raster coordinates | Preserve. They are private prepared-shape representations and are converted through stored physical metres-per-cell. | Prepared-mask round trip and physical resampling parity. |
| Existing values named `*Metres` | Physical metres | Preserve as metres and convert only at the descriptor/raster boundary. | Physical extents unchanged across grid candidates. |
| Existing normalized lateral anchors | Normalized authoring coordinates | Preserve as authoring data; convert to signed metres at the sampled river row. | Same logical bank/centre placement on widening, narrowing, and asymmetric domains. |

### 7.5 Boundary and shore support

**Class: U/D/T**

Boundary generation currently uses quality-specific edge thicknesses in cells and per-row lateral spacing. The fixed metric lattice changes their physical width.

Dependencies:

- bank-edge feather;
- valid-water mask;
- shore support and shore negative aging;
- current shore-edge extraction;
- row-local left/right widths;
- padded downstream area;
- shape-stage boundary application.

Primary files:

- `StylizedRiverFoamRuntime.Topology.cs`
- `StylizedRiverFoamRuntime.Obstacles.cs`
- `StylizedRiverFoamMajorTopologyGenerator.cs`
- `CS_RiverFoam.Topology.hlsl`
- `CS_RiverFoam.Support.hlsl`
- `CS_RiverFoam.compute` kernels:
  - `BuildCurrentShoreEdges`
  - `ComposeTopology`
  - `ApplyBoundary`

Mandatory decision:

- preserve the current physical shore band, preserve current cell count, or redesign it deliberately.

Mandatory tests:

- both banks independently;
- asymmetric banks;
- tight bends;
- width transitions;
- no Foam outside the valid surface;
- no new dark/empty bank seam;
- shore birth and shore support remain aligned.

### 7.6 Obstacle exclusion rasterization

**Class: U/T**

Exact-mesh obstacle occupancy is converted into field cells. Every cell interval and cache artifact depends on the old coordinate mapping.

Dependencies:

- world/river-space obstacle projection;
- metric-to-cell conversion;
- compact interval encoding;
- GPU obstacle-cell buffer;
- exclusion texture update/readback;
- obstacle fingerprints;
- obstacle geometry version and stable registry timing;
- topology-cache inclusion.

Primary files:

- `RiverObstacleExclusionResolver.cs`
- `StylizedRiverFoamRuntime.Obstacles.cs`
- `StylizedRiverFoamRuntime.TopologyCache.cs`
- `CS_RiverFoam.compute` kernels:
  - `ClearObstacleExclusion`
  - `UpdateObstacleExclusion`
  - `BuildFoamObjectContactField`

Mandatory tests:

- rotated and sloped silhouettes;
- very small obstacles;
- obstacles touching banks;
- hidden renderer but active simulation obstacle;
- exact front/back/side occupancy;
- no one-cell holes;
- stable cache fingerprinting;
- rebuild after obstacle transform/version changes.

### 7.6.4 P6 recorded unit policy and validation ownership

P5.3 is complete. P6 owns the following decisions without activating the fixed grid:

- Motion Lane downstream noise uses a 32 m physical basis; the accepted first-octave wavelength remains `32 / 8.5 = 3.7647 m`.
- Motion Lane lateral noise uses a 10 m physical reference span; wide rivers deliberately contain more independent lateral structure.
- fixed smoothing uses 0.20 m and 0.40 m offsets with the existing two-pass weights; legacy offsets remain exactly one and two rows.
- fixed routing uses 2.0 m approach, 0.35 m front closure, 0.50 m contact reach, a minimum one-cell/22%-height lateral margin, and max(0.10 m, 10%-height) centre dead-band.
- Disturbance allocation and generation remain unchanged. Foam maps each physical cell centre to Disturbance UV through local distance and local left/right surface widths.
- normal production Foam UV migration remains P9; P6 changes only Motion Lane/routing debug sampling in the renderer.
- validation is one explicit P6 report by default. Supplemental visual evidence is requested only when the report cannot establish a shader-visible alignment failure.
- P6b correction: the report must own a non-storing live-resource transaction. It prepares generated obstacle sources, installs the assigned current cache through the normal initialization state machine, inspects descriptors/textures/telemetry while they remain live, and only then releases resources and disables bindings in `finally`. Calling `TryPrepareTopologyCacheInEditor()` is prohibited because that method serializes a payload and unconditionally releases the state before returning.
- validator validity is an acceptance contract: every runtime ledger line must name the live state it measured; released sentinel state may only appear in the dedicated post-cleanup proof. Motion Lane and routing signatures use `int.MinValue` as the uninitialized sentinel.
- P6b is Unity-validated and closed. The corrected live report monitors and forbids cache serialization, evaluates cached-scalar routing occupancy, captures evidence before cleanup, avoids altering foreign preparation state, proves resource/binding cleanup afterward, and returned `Overall: PASS`.

### 7.7 Obstacle routing and pressure support

**Class: U/D/T**

Routing currently uses cell-space BFS and multiple reach/margin constants derived from field dimensions or fixed cell counts. The metric migration can radically change their physical extent.

Dependencies:

- approach cells;
- front cells and front closure;
- lateral margins;
- contact cells;
- upstream support search;
- pressure envelope thickness;
- obstacle routing texture;
- obstacle slowdown and minimum downstream factor;
- flow direction.

Primary files:

- `StylizedRiverFoamRuntime.Obstacles.cs`
- `CS_RiverFoam.Motion.hlsl`
- `CS_RiverFoam.Support.hlsl`
- `CS_RiverFoam.compute`

Mandatory decisions:

- convert physical routing reaches to metres;
- retain only connectivity/search support in cells;
- define physical behavior around obstacles independently of river width and quality.

Mandatory tests:

- long and short objects;
- rotated objects;
- no full O-wrap around objects;
- correct C/∪ routing;
- no upstream spawn or motion leakage;
- narrow and wide rivers;
- forward and reversed flow;
- routing continuity through width changes.

### 7.8 Motion lane field

**Class: U/D/T**

The motion lane is a Foam-grid texture. Its procedural pattern, smoothing, aspect correction, scroll, CPU readback, and obstacle integration all depend on field dimensions.

Dependencies:

- normalized U/V noise input;
- `fieldWidth / fieldHeight` aspect treatment;
- lane scale and wavelength;
- smoothing radius in cells;
- scroll metres-to-cells conversion;
- field signature and rebuild rules;
- full-field CPU data/readback;
- sampling in transport and rendering.

Primary files:

- `StylizedRiverFoamRuntime.Obstacles.cs`
- `StylizedRiverFoamRuntime.Compute.cs`
- `CS_RiverFoam.Motion.hlsl`
- `RiverWaterFoamVelocity.hlsl`

Mandatory decisions:

- author lane scale in physical metres or normalized river proportions;
- preserve current visual frequency or current numerical settings;
- determine whether wide rivers should contain more independent lanes.

Mandatory tests:

- physical wavelength across widths;
- downstream scroll speed;
- no stationary/repainted canonical velocity bug;
- no zero-speed regions;
- obstacle-routing blend;
- renderer and simulation sample the same field.

### 7.9 Automatic birth scheduling and budgets

**Class: D/R/T**

The event scheduler is not directly a coordinate transform, but its fixed per-step event budgets and source capacities determine Foam density. A wider metric field has more cells and more physical area, while current budgets remain tied only to quality.

Dependencies:

- Low/Medium/High birth budgets;
- maximum automatic events per dispatch;
- pattern selection weights;
- formation cadence;
- held/active source scheduling;
- per-source event lifetime;
- event suppression and overlap.

Primary files:

- `StylizedRiverFoamRuntime.Constants.cs`
- `StylizedRiverFoamRuntime.BirthEvents.cs`
- `StylizedRiver.cs`

Mandatory decision:

- keep births authored per river, scale by active surface area/length, or establish explicit area-independent composition rules.

Mandatory tests:

- equal visual density on 5, 10, 20, and 40 m widths;
- event-cap saturation;
- no cadence changes caused solely by extra cells;
- pattern weights remain respected;
- deterministic seed behavior.

### 7.10 Automatic source geometry

**Class: U/D/T**

Every automatic source family must be reviewed separately. The migration cannot be validated by checking Arc alone.

Source types:

1. Shore Ribbon
2. Inward Wash
3. Object Contact Arc
4. Object Contact Semi-Arc
5. Object Contact Fleck
6. Free Water Lace Connector
7. Free Water Cross-Lace Connector
8. Free Water Torn Fragment

Primary files:

- `StylizedRiverFoamRuntime.State.cs`
- `StylizedRiverFoamRuntime.BirthEvents.cs`
- `StylizedRiverFoamRuntime.Injection.cs`
- `StylizedRiverFoamRuntime.Evolution.*.cs`
- `CS_RiverFoam.Evolution.hlsl`
- `CS_RiverFoam.Noise.hlsl`
- `CS_RiverFoam.compute` source evaluators and raster kernels

Per-parameter decision rule:

- **Metres:** visually meaningful length, width, offset, inward reach, trail length, and physical feather.
- **Cells:** raster support, connectivity, minimum sample coverage, or strictly discrete operations.
- **Normalized:** only proportions that intentionally scale with host/source geometry or local river width.

Mandatory source-family tests:

- physical bounding length and width;
- minimum one-cell footprint;
- build, hold, and release progression;
- source continuity;
- orientation invariance;
- flow reversal;
- bank clipping;
- obstacle clipping;
- no detached fragments unless intended;
- no cell-shaped rectangular blocks at accepted metric quality;
- no accidental physical widening of Shore Ribbon;
- accepted Arc/Semi-Arc C-shape preserved.

### 7.10.1 P7 recorded source-unit and range policy

- Every source parameter is classified as metres, seconds, normalized/unitless, or compatibility cells.
- `foamShoreRibbonThicknessCells` and `foamShoreRibbonOffsetVariationCells` remain serialized unchanged. Legacy uses local cross-river cells; fixed source preparation resolves them once using descriptor-owned local-normal spacing.
- Existing normalized public/manual entry points remain compatibility paths. New metric entry points own global distance, lateral metres, metre drift, and metre bend.
- Automatic fixed ranges are bounded in both X and Y and must contain the physical source footprint. Legacy dispatch formulas remain exact.
- Production and debug automatic-source kernels call the same shared evaluator; `InjectFoam` alone owns source-range Y dispatch.
- Validation uses one live comprehensive report by default and must prove its own preparation, measurement-before-cleanup, cleanup, and assigned-cache immutability.
- Compatibility manual bounds union every candidate row's local normalized-to-metre conversion; metric bounds remain fixed in metres. Shared authored-anchor placement is validated independently of dispatch-rectangle equality.
- Fixed Shore/Wash bounds evaluate every candidate row and padded endpoint at its actual domain distance; they do not clamp longitudinal feather rows to the source endpoints.
- The report proves all automatic GPU lanes, compatibility and metric compound paths, independent full-Y `ClearRange` ownership, and source inspection through absolute project paths.

### 7.11 Manual injection and isolated probes

**Class: U/R/T**

Manual ellipse, stroke, compound, clear-range, and isolated-life-probe paths can bypass automatic-source preparation. They must use the same metric descriptor.

Dependencies:

- normalized centre coordinates;
- metric radii and stroke endpoints;
- dispatch range culling;
- clear range;
- source read/write textures;
- isolated probe cell placement;
- debug source masks.

Primary files:

- `StylizedRiverFoamRuntime.Injection.cs`
- `StylizedRiverFoamRuntime.BirthTransfer.cs`
- `StylizedRiverFoamRuntime.BirthDiagnostics.cs`
- `StylizedRiverEditor.Actions.cs`
- `CS_RiverFoam.compute` kernels:
  - `ClearRange`
  - `InjectFoam`
  - `WriteIsolatedLifeProbe`
  - `ClearAutomaticBirthDebugAll`

Mandatory tests:

- placement at centre and both banks;
- physical ellipse dimensions;
- long strokes across width changes;
- clear exact region;
- probe lifetime unaffected by position;
- no old normalized-Y assumptions remain.

### 7.12 Persistent material transport

**Class: U/D/T**

The fixed metric Y lattice corrects the current hidden squeeze/stretch in which equal Y indices on adjacent rows can represent different lateral metre positions. Transport must nevertheless be revalidated fully.

Dependencies:

- neighbour addressing;
- downstream and lateral velocity conversion to cells;
- per-cell area;
- face lengths and conservative flux;
- obstacle footprint and routing;
- boundary clipping;
- endpoint outflow;
- presence/life/pattern moment conservation;
- CFL/substep calculation;
- metrics and fixed-point accumulation.

Primary files:

- `StylizedRiverFoamRuntime.RuntimeUpdates.cs`
- `StylizedRiverFoamRuntime.Compute.cs`
- `StylizedRiverFoamRuntime.Lifecycle.cs`
- `CS_RiverFoam.Simulation.hlsl`
- `CS_RiverFoam.Transport.hlsl`
- `CS_RiverFoam.Motion.hlsl`
- `CS_RiverFoam.compute` kernel `SimulateFoam`

Mandatory decisions:

- whether Stage 1 retains rectangular `dx*dy` area on bends;
- accepted `max(abs(curvature * lateralOffset))` approximation bound;
- whether wide/high-curvature sections are rejected, subdivided, or corrected using the curvilinear Jacobian and face metrics.

Mandatory tests:

- mass conservation with no birth/death;
- life and pattern-moment conservation;
- constant-width straight river;
- widening/narrowing river;
- left and right bends;
- 40 m width stress case;
- endpoint outflow;
- obstacle diversion;
- one- and multi-substep cases;
- flow reversal;
- no lateral jump at topology replacement.

### 7.13 Topology replacement and previous-state remapping

**Class: U/T**

The runtime can replace topology while preserving or transitioning state. The transition shader currently reconstructs positions using the existing normalized coordinate model.

Dependencies:

- current descriptor;
- previous descriptor;
- previous/current dimensions and lengths;
- current-to-previous metric mapping;
- state snapshots;
- topology-transition textures;
- change detection and lifetime.

Primary files:

- `StylizedRiverFoamRuntime.TopologyReplacement.cs`
- `StylizedRiverFoamRuntime.Lifecycle.cs`
- `StylizedRiverFoamRuntime.Compute.cs`
- `CS_RiverFoam.TopologyTransition.hlsl`
- `CS_RiverFoam.compute`

Mandatory tests:

- width changes;
- domain extension/shortening;
- quality changes;
- obstacle-triggered topology replacement;
- no state teleportation, duplication, loss, or lateral scale distortion;
- correct behavior when descriptors are incompatible and state must be cleared.

### 7.14 Half-resolution visual occupancy and film fields

**Class: U/R/T**

Layer D uses half-resolution fields derived from structural width and height. It computes represented cell count and physical area, advances visual occupancy, and feeds shape evaluation/rendering.

Dependencies:

- `filmWidth` and `filmHeight` rounding;
- full-to-film and film-to-full mapping;
- represented structural-cell count at odd edges;
- visual occupancy cell area;
- film source and support;
- visual occupancy advection;
- visual occupancy texture binding;
- debug descriptions that assume one film texel represents four structural cells.

Primary files:

- `StylizedRiverFoamRuntime.Resources.cs`
- `StylizedRiverFoamRuntime.Compute.cs`
- `StylizedRiverFoamRuntime.PublicSurface.cs`
- `CS_RiverFoam.compute` kernels:
  - `BuildFoamFilmSource`
  - `BuildFoamFilmSupport`
  - `AdvanceFoamVisualOccupancy`
  - `EvaluateFoamShape`
- `RiverWaterFoam.hlsl`
- `StylizedRiverEditor.DebugViews.cs`

Mandatory tests:

- odd structural dimensions;
- bank-edge film texels representing fewer than four valid cells;
- integrated area parity;
- visual occupancy transport alignment;
- no block artifacts reintroduced at half resolution;
- shape remains visually stable at every candidate metric size.

### 7.15 Shape evaluation, breakup, and noise

**Class: R/D/T**

Some shape/noise features use cell spacing as a physical minimum or scale. A more isotropic structural grid changes their frequency, feather, and apparent breakup.

Dependencies:

- feature size clamping by cell spacing;
- per-cell versus rendered-pixel breakup;
- support search distances;
- strand/chip/fragment scales;
- field aspect;
- temporal morph cadence;
- boundary clipping.

Primary files:

- `CS_RiverFoam.Noise.hlsl`
- `CS_RiverFoam.Evolution.hlsl`
- `CS_RiverFoam.Support.hlsl`
- `CS_RiverFoam.Topology.hlsl`
- `CS_RiverFoam.compute`
- `RiverWaterFoam.hlsl`

Mandatory tests:

- no structural-cell holes exposed;
- Layer E detail remains rendered-pixel detail;
- accepted chip/strand scales at camera distance;
- no anisotropic noise stretch;
- no change to lifecycle state from visual-only effects.

### 7.16 Production rendering

**Class: U/R/T**

The production renderer currently derives Foam field Y from `lateralMetres / surfaceHalfWidth`. That is incompatible with fixed metric Y.

Dependencies:

- field UV reconstruction;
- metric offset to UV conversion;
- longitudinal valid versus padded range;
- lateral origin/global-Y base;
- visual warp and stretch;
- persistent state sampling;
- topology and shape sampling;
- motion-lane/obstacle-routing sampling;
- film occupancy sampling;
- boundary clipping;
- debug render modes.

Primary files:

- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl`
- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoamVelocity.hlsl`
- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader`
- `StylizedRiverFoamRuntime.Binding.cs`

Mandatory tests:

- simulation/render sample parity;
- no half-cell offset;
- both banks and asymmetric widths;
- padded endpoint clipping;
- visual warp is expressed in metres correctly;
- static and moving cameras;
- all production and debug Foam modes;
- no change to unrelated river lighting, refraction, colour, or disturbance rendering.

### 7.17 Disturbance-field integration

**Class: T/R, not automatically U**

Foam consumes Static Pressure, Static Wake, Ripple, and related disturbance fields that retain their own dimensions and normalized coordinate mapping. They should not be converted merely because Foam changes.

Dependencies:

- disturbance texture dimensions;
- Foam-cell centre to disturbance UV conversion;
- external-field binding and neutral fallback;
- obstacle registry readiness and geometry version;
- static pressure support for sources;
- wake/lee support and negative aging;
- ripple/wave sampling;
- Disturbance quality remains separate despite sharing `StylizedRiverQuality`.

Integration files to test:

- `StylizedRiverDisturbanceRuntime.cs`
- `StylizedRiverDisturbanceRuntime.Binding.cs`
- `StylizedRiverDisturbanceRuntime.Resources.cs`
- `StylizedRiverDisturbanceRuntime.StaticPressure.cs`
- `StylizedRiverDisturbanceRuntime.StaticWake.cs`
- `StylizedRiverDisturbanceRuntime.Ripple.cs`
- `StylizedRiverDisturbanceRuntime.PublicSurface.cs`
- `StylizedRiverDisturbanceRuntime.GeneratedSources.cs`
- `StylizedRiverDisturbanceRuntime.SourcePathMath.cs`
- `StylizedRiverFoamRuntime.Injection.cs`
- `StylizedRiverFoamRuntime.Topology.cs`
- `StylizedRiverFoamRuntime.TopologyCache.cs`
- `CS_RiverFoam.Sampling.hlsl`
- `CS_RiverFoam.compute`

Mandatory tests:

- Foam and disturbance fields with different dimensions;
- same physical point sampled in both fields;
- Static Pressure front support;
- Static Wake lee support;
- Ripple and wave influence;
- neutral fallback when Disturbance is absent;
- no change to Disturbance field allocation or performance unless separately approved.

### 7.18 River-domain, geometry, and orientation inputs

**Class: T/R, not automatically U**

Foam depends on the river domain for centreline position, cumulative distance, tangent/normal, left/right widths, valid length, curvature, and flow orientation.

Integration files to test:

- `RiverDomainSnapshot.cs`
- `StylizedRiverGeometry.cs`
- `StylizedRiverCorridorGeometry.cs`
- `StylizedRiver.cs`
- `StylizedRiverDomainDebug.cs`

Mandatory tests:

- centreline distance monotonicity;
- asymmetric left/right widths;
- width variation;
- tight curves;
- reversed flow direction;
- changed spline/domain version;
- endpoints and padding;
- river renderer/corridor mesh matches the metric lattice physically;
- no corridor geometry modification is introduced by the Foam patch.

### 7.19 Shared quality enum and policy

**Class: U/D/T**

`StylizedRiverQuality` is shared by Foam, Disturbance, and corridor geometry. Reinterpreting the enum itself would derail unrelated systems.

Required policy:

- keep the shared enum and serialized values;
- introduce a Foam-specific mapping from quality to requested metric cell size;
- do not modify Disturbance or corridor geometry quality constants as part of this patch;
- record that the new Foam mapping is a fidelity upgrade, not a metric-equivalent rename of 64/96/128.

Primary files:

- `StylizedRiver.cs`
- `StylizedRiverFoamRuntime.Constants.cs`
- `StylizedRiverFoamRuntime.Resources.cs`
- `StylizedRiverEditor.Authoring.cs`

Mandatory integration tests:

- Low/Medium/High Foam dimensions;
- Disturbance dimensions unchanged;
- corridor mesh tessellation unchanged;
- existing serialized Medium scene loads without migration damage.

### 7.20 Cache asset, codec, fingerprints, and runtime cache state

**Class: U/D/T**

Existing cache artifacts encode products generated under the normalized-lateral contract.

Dependencies:

- asset storage contract;
- binary payload format;
- generator contract;
- generation fingerprint contract;
- field width/height/length;
- metric-grid mapping version;
- resolved `dx/dy`;
- lattice origin and global-Y base;
- topology arrays and obstacle exclusion;
- maximum dimension 8192;
- maximum cell count;
- runtime cache hit/miss reasons;
- stale settings, domain, obstacle, and generator detection.

Primary files:

- `FoamTopology/StylizedRiverFoamTopologyCacheAsset.cs`
- `FoamTopology/StylizedRiverFoamTopologyCacheCodec.cs`
- `FoamTopology/StylizedRiverFoamTopologyFingerprints.cs`
- `StylizedRiverFoamRuntime.TopologyCache.cs`

Mandatory decisions:

- payload-format bump only if serialized layout changes;
- generator and generation-fingerprint contract bumps are mandatory;
- exact failure policy for fields exceeding contiguous cache limits;
- whether one-strip descriptors are serialized now to ease Stage 2.

Mandatory tests:

- deterministic old-cache rejection;
- new cache encode/decode round trip;
- corruption and dimension limits;
- exact hit on unchanged inputs;
- stale reason for every descriptor change;
- no raw-editing of generated cache assets.

### 7.21 Cache preparation, development coordination, and build preflight

**Class: U/R/T**

The editor workflow requires exact prepared cache artifacts before builds. A mapping-contract change must propagate through all tooling and messages.

Primary files:

- `Editor/StylizedRiverEditor.Actions.cs`
- `Editor/StylizedRiverFoamBuildPreflight.cs`
- `Editor/StylizedRiverFoamDevelopmentCacheCoordinator.cs`
- `Editor/StylizedRiverEditor.Diagnostics.cs`
- `Editor/StylizedRiverEditor.Foam.cs`

Mandatory tests:

- explicit cache preparation;
- development auto/rebuild coordination, if enabled;
- stale cache diagnostics;
- build preflight pass with valid cache;
- build preflight failure with normalized-grid cache;
- no hidden scene reserialization;
- correct handling of obstacle registry readiness.

### 7.22 Diagnostics, metrics, and telemetry

**Class: U/D/T**

Several current diagnostics are resolution-dependent. Raw affected-texel counts, visible perimeter counts, and cell counts cannot be compared across grids without physical normalization.

Dependencies:

- field dimensions;
- spacing min/max;
- CFL and substeps;
- affected source texels;
- topology cell counts and ratios;
- perimeter cell count;
- integrated physical areas;
- transport conservation;
- cells and dispatches per second;
- memory estimates;
- cache state;
- debug summaries.

Primary files:

- `FoamTopology/StylizedRiverFoamCacheDiagnostics.cs`
- `FoamTopology/StylizedRiverFoamTopologyCacheAsset.cs`
- `Editor/StylizedRiverFoamTopologyCacheAssetEditor.cs`
- `StylizedRiverFoamRuntime.CacheDiagnostics.cs`
- `StylizedRiverDisturbanceRuntime.CacheDiagnostics.cs`
- `StylizedRiverFoamRuntime.BirthDiagnostics.cs`
- `StylizedRiverFoamRuntime.PublicSurface.cs`
- `StylizedRiverFoamRuntime.Compute.cs`
- `StylizedRiverEditor.Diagnostics.cs`
- `StylizedRiverEditor.DebugViews.cs`
- `CS_RiverFoam.compute` metrics kernels

Required diagnostic additions or corrections:

- requested and resolved `dx/dy`;
- lattice origin and local global-Y interval;
- allocated versus valid cells;
- dispatch-rounded thread envelope;
- invalid/out-of-bank occupancy percentage;
- physical affected source area;
- physical perimeter estimate or clearly resolution-specific perimeter label;
- maximum `abs(curvature * lateralOffset)`;
- contiguous cache-limit headroom;
- CFL components separately;
- Stage 1 whole-rectangle waste;
- per-obstacle source provenance and direct/provider fingerprint parity;
- same-input repeated-build payload and section parity;
- first full-payload and section byte differences;
- assigned-cache versus generated-build identity;
- explicit saved/copyable reports under `Library/RiverFoamDiagnostics`.

Diagnostic execution requirements:

- expensive diagnostics are explicit Edit Mode actions only;
- payload section analysis is explicit and never runs per Inspector repaint;
- no diagnostic may mutate a scene, prefab, material, generated source, River setting, or assigned cache asset;
- future deterministic contract patches ship their owning diagnostics in the same patch.

### 7.23 Memory and performance accounting

**Class: U/T**

The field owns multiple full-resolution, half-resolution, buffer, upload, readback, and transition resources. Cell count is not a complete cost model.

Mandatory measurements:

- allocated texture bytes;
- buffer bytes;
- CPU arrays/readbacks;
- kernel dispatch counts;
- launched threads;
- material-update cells per second;
- CFL and substeps;
- CPU submission time;
- GPU time;
- cache generation time;
- topology build time;
- source preparation time.

Mandatory comparison cases:

- current normalized Medium baseline;
- metric candidates 0.25, 0.20, 0.15, and 0.10 m;
- 5, 10, 20, and 40 m widths;
- straight and curved domains;
- active and idle/held states;
- visible and offscreen behavior as currently implemented.

No Stage 1 report may claim cost scales with active local water area. Until strips exist, cost still scales with the contiguous rectangle’s total length and maximum lateral extent.

### 7.24 Inspector and serialized authoring semantics

**Class: U/D/T**

The current editor describes quality in old structural terms and exposes several values in cells.

Primary files:

- `StylizedRiver.cs`
- `Editor/StylizedRiverEditor.Authoring.cs`
- `Editor/StylizedRiverEditor.Foam.cs`
- `Editor/StylizedRiverEditor.UI.cs`
- `Editor/StylizedRiverEditor.cs`

Required decisions:

- whether old serialized cell-based values are migrated, reinterpreted, or deprecated;
- whether metre-based replacements need compatibility fields;
- exact inspector labels and tooltips;
- whether resolved cell size is shown read-only by quality.

Mandatory tests:

- old scene loads without value reset;
- no prefab/scene reserialization caused only by opening inspector;
- units are explicit;
- values persist over domain reload and code recompilation;
- debug and runtime use identical values.

### 7.25 Debug views

**Class: U/T**

Every field-space debug visualization must be validated, including views that are not part of the production renderer.

Dependencies:

- structural cell grid;
- topology layers;
- obstacle footprint;
- routing and motion lane;
- automatic birth sources;
- current/live versus cumulative source display;
- visual occupancy field;
- persistent material state;
- shape output;
- cache diagnostics.

Primary files:

- `Editor/StylizedRiverEditor.DebugViews.cs`
- `Editor/StylizedRiverEditor.Diagnostics.cs`
- `StylizedRiverDomainDebug.cs`
- `SH_CleanStylizedRiver.shader`
- `RiverWaterFoam.hlsl`

Mandatory tests:

- overlays align with river geometry at both banks;
- cell grid is physically square/isotropic at selected target where intended;
- hidden rocks remain valid simulation obstacles;
- live Automatic Source view does not display stale cumulative cells;
- no upstream shell around objects;
- half-resolution occupancy overlay maps correctly.

### 7.26 Runtime scheduling, state ownership, and change handling

**Class: U/T**

Dependencies:

- material update cadence by quality;
- active/held/idle state;
- runtime initialization order;
- disturbance/obstacle readiness;
- cache startup validation;
- domain and geometry version observation;
- pending obstacle rebuild stabilization;
- renderer binding after resource replacement;
- topology replacement ownership;
- enable/disable and destruction cleanup.

Primary files:

- `StylizedRiverFoamRuntime.cs`
- `StylizedRiverFoamRuntime.Lifecycle.cs`
- `StylizedRiverFoamRuntime.RuntimeUpdates.cs`
- `StylizedRiverFoamRuntime.TopologyCache.cs`
- `StylizedRiverFoamRuntime.TopologyReplacement.cs`
- `StylizedRiverFoamSimulation.cs`

Mandatory tests:

- startup with valid cache;
- startup with stale cache;
- missing Disturbance runtime;
- obstacle registry arriving late;
- repeated enable/disable;
- quality and domain changes during Play Mode;
- no duplicate initialization or leaked render textures;
- renderer never samples released or mismatched fields.

### 7.27 Documentation and plan ownership

**Class: U**

Canonical documents that must be updated if implementation is approved:

- `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
- `Docs/River_Foam_Stage6_Architecture.md`
- `Docs/River_Rendering_Roadmap.md`
- `Docs/handoff.md`, if it is the active canonical handoff

Historical documents may retain old behavior as history but should not be silently rewritten unless the repository policy requires it.

The active blocker document’s stale H.4 versus H.6.2 queue must be reconciled before adding the metric-grid patch.

### 7.28 Scene and generated-asset policy

**Class: T only unless separately authorized**

Validation input:

- `Game/Demo/Scenes/VisualFrameworkDemo.unity`

Requirements:

- do not raw-edit or reserialize the scene as part of the code migration;
- do not modify prefabs without explicit authorization;
- regenerate Foam topology caches only through the approved editor workflow;
- document any manual inspector action the user must perform;
- compare against the scene’s current Medium baseline.

## 8. Compute-kernel dependency checklist

Every kernel in `CS_RiverFoam.compute` must be classified and tested. None may be assumed unaffected merely because its main purpose is not coordinate conversion.

| Kernel | Stage 1 status | Required focus |
|---|---:|---|
| `ClearRange` | U/T | Metric range/addressing and exact clearing |
| `InjectFoam` | U/T | Manual source placement and physical extent |
| `RasterizeFoamSourceEvent` | U/T | Metric cell centres, source widths, event bounds |
| `RasterizeFoamSourceEventDebug` | U/T | Exact parity with production source raster |
| `WriteIsolatedLifeProbe` | U/T | Probe cell placement |
| `ClearAutomaticBirthDebugAll` | T | Dimensions and complete clear |
| `BuildCurrentShoreEdges` | U/T | Bank edge and metric thickness |
| `ComposeTopology` | U/T | Topology layers and bank clipping |
| `CaptureGeneratedTopology` | U/T | Cache/readback dimensions and parity |
| `BuildEvolvingMajorSupport` | U/R/T | Local extents and physical support widths |
| `ClearObstacleExclusion` | T | Complete dimensions |
| `UpdateObstacleExclusion` | U/T | Metric obstacle intervals |
| `BuildFoamObjectContactField` | U/T | Cell-neighbour physical gradients and pressure alignment |
| `ResetTopologyMetrics` | T | Buffer reset |
| `MeasureTopologyMetrics` | U/D/T | Physical area/perimeter semantics |
| `ResetTransportMetrics` | T | Buffer reset |
| `SimulateFoam` | U/D/T | Conservative transport, CFL, area, curvature policy |
| `BuildFoamFilmSource` | U/T | Full-to-half mapping and area |
| `BuildFoamFilmSupport` | U/T | Half-resolution support alignment |
| `AdvanceFoamVisualOccupancy` | U/T | Metric advection and represented area |
| `EvaluateFoamShape` | U/R/T | Structural/film alignment and noise scale |
| `ApplyBoundary` | U/T | Valid-bank clipping and padding |

## 9. Complete file-level register for the supplied snapshot

The following is the file-level audit boundary. “Review/Test” does not mean the file will necessarily be modified; it means it may not be excluded from validation.

### 9.0 Mandatory update — shared exact generated-geometry identity

- **U/T** `Game/Procedural/Core/GeneratedGeometryStableFingerprint.cs`
- **U/T** `Game/Procedural/Masses/GeneratedMass.cs`

These shared files are in P5.2 scope only for the exact-world fingerprint sentinel and transient cache repair. No Mass generation, topology, edge-wear, material, scene, or prefab behavior is authorized.

### 9.1 Mandatory update or conditional update — River Foam runtime

- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Binding.cs`
- **R/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthDiagnostics.cs`
- **U/D/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs`
- **R/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthTransfer.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Compute.cs`
- **U/D/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs`
- **U/R/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Evolution.Connector.cs`
- **U/R/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Evolution.FreeWater.cs`
- **R/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Evolution.HostedNegative.Pose.cs`
- **U/R/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Evolution.HostedNegative.cs`
- **U/R/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Evolution.Major.cs`
- **U/R/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Evolution.Shared.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs`
- **U/D/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Obstacles.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.RuntimeUpdates.cs`
- **U/D/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.State.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Topology.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.TopologyCache.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.TopologyReplacement.cs`
- **R/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.cs`
- **R/T** `Game/Procedural/Rivers/StylizedRiverFoamSimulation.cs`

### 9.2 Mandatory update or conditional update — topology and obstacle conversion

- **R/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamConnectorTopology.cs`
- **U/D/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamConnectorTopologyGenerator.cs`
- **R/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamMajorCandidate.cs`
- **R/D/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamMajorCandidateGenerator.cs`
- **R/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamMajorTopology.cs`
- **U/D/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamMajorTopologyGenerator.cs`
- **R/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamPocketTopology.cs`
- **U/D/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamPocketTopologyGenerator.cs`
- **U/R/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyCacheAsset.cs`
- **U/D/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyCacheCodec.cs`
- **U/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyFieldSpace.cs`
- **U/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyFingerprints.cs`
- **U/T** `Game/Procedural/Rivers/RiverObstacleExclusionResolver.cs`
- **U/T, Editor-only** `Game/Procedural/Rivers/RiverObstacleExclusionResolver.LegacyParityDiagnostics.cs`

### 9.3 Mandatory update or conditional update — compute and rendering

- **U/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Coordinates.hlsl`
- **U/R/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Evolution.hlsl`
- **U/R/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Motion.hlsl`
- **R/D/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Noise.hlsl`
- **U/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl`
- **U/R/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Sampling.hlsl`
- **U/D/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Simulation.hlsl`
- **U/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Structs.hlsl`
- **U/R/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Support.hlsl`
- **U/R/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Topology.hlsl`
- **U/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.TopologyTransition.hlsl`
- **R/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Transport.hlsl`
- **U/D/T** `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`
- **U/T** `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl`
- **R/T** `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoamVelocity.hlsl`
- **U/R/T** `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader`

### 9.4 Mandatory update or conditional update — authoring, diagnostics, and cache tooling

- **U/R/T** `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`
- **U/T** `Game/Procedural/Rivers/Editor/StylizedRiverFoamTopologyCacheAssetEditor.cs`
- **U/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamCacheDiagnostics.cs`
- **U/T** `Game/Procedural/Rivers/FoamTopology/StylizedRiverFoamTopologyCacheAsset.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverFoamRuntime.CacheDiagnostics.cs`
- **U/T** `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Authoring.cs`
- **U/T** `Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs`
- **U/T** `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Diagnostics.cs`
- **T** `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Disturbances.cs`
- **U/D/T** `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
- **R/T** `Game/Procedural/Rivers/Editor/StylizedRiverEditor.UI.cs`
- **R/T** `Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs`
- **U/R/T** `Game/Procedural/Rivers/Editor/StylizedRiverFoamBuildPreflight.cs`
- **U/R/T** `Game/Procedural/Rivers/Editor/StylizedRiverFoamDevelopmentCacheCoordinator.cs`
- **U/D/T** `Game/Procedural/Rivers/StylizedRiver.cs`

### 9.5 Mandatory integration test — upstream river/domain/obstacle inputs

These files should not be modified automatically. They are required integration-test dependencies because Foam consumes their output.

- **T/R** `Game/Procedural/Rivers/RiverDomainSnapshot.cs`
- **T/R** `Game/Procedural/Rivers/StylizedRiverGeometry.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverCorridorGeometry.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDomainDebug.cs`
- **T/R** `Game/Procedural/Rivers/RiverDisturbanceFootprintResolver.cs`
- **T/R** `Game/Procedural/Rivers/StylizedRiverDisturbanceEmitter.cs`

### 9.6 Mandatory integration test — Disturbance subsystem

- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Binding.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Compute.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Constants.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.ContinuousSources.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Contracts.cs`
- **T/U** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.CacheDiagnostics.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Diagnostics.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Dispatch.cs`
- **U/T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.GeneratedSources.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Impact.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Members.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.PublicSurface.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Resources.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.Ripple.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.SourcePathMath.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.State.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.StaticPressure.cs`
- **T** `Game/Procedural/Rivers/StylizedRiverDisturbanceRuntime.StaticWake.cs`

### 9.7 Mandatory validation input — scene/assets

- **T, no raw edit** `Game/Demo/Scenes/VisualFrameworkDemo.unity`
- **T, regenerate through tooling only** all River Foam topology cache assets referenced by validation scenes
- **T** all materials using `SH_CleanStylizedRiver.shader`
- **T** all river instances with Foam enabled, not only the demo instance

### 9.8 Canonical documentation

- **U** `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
- **U** `Docs/River_Foam_Stage6_Architecture.md`
- **U** `Docs/River_Rendering_Roadmap.md`
- **R/U** `Docs/handoff.md`

## 10. Future strip/pooling-only dependency register

These dependencies are not required to prove the one-strip metric coordinate contract, but they are mandatory before claiming scalable local-width allocation or active-area cost scaling.

### 10.1 Strip descriptor and ownership — F/D

- strip start/end in centreline metres;
- columns and local global-Y interval;
- shared lattice phase and `dy`;
- overlap/ghost border ownership;
- endpoint and inter-strip boundary roles;
- domain version and generation fingerprint per strip.

### 10.2 Resource representation — F/D

- independent textures;
- texture arrays;
- atlas or packed pages;
- allocation buckets;
- pooling and reuse;
- fragmentation and memory caps;
- per-strip neutral resources;
- transition resources.

### 10.3 Cross-strip transport — F/D/T

- ghost-cell copies by matching global Y;
- cells present only in one strip’s wider interval;
- conservative flux ownership;
- flow reversal;
- simultaneous versus ordered dispatch;
- state transfer on strip activation/deactivation;
- topology and obstacle continuity.

### 10.4 Renderer lookup and indirection — F/D/T

- world/river point to strip index;
- strip-local UV;
- boundary blending;
- film field lookup;
- debug lookup;
- no seams or double samples.

### 10.5 Scheduling and budgets — F/D/T

- active strip detection;
- visible/offscreen/frozen states;
- update cadence by distance/activity;
- global active-cell cap;
- global memory cap;
- dispatch batching;
- many-river fairness;
- pre-generation and cache loading policy.

### 10.6 Strip cache format — F/D/T

- descriptor table;
- per-strip payloads;
- partial loading;
- compatibility and versioning;
- maximum strip count;
- exact cache hit/miss reporting;
- cache regeneration and build preflight.

### 10.7 Connected river components — F/D/T

Not needed for Stage 1, but required before state crosses independently authored components:

- endpoint connection identity;
- compatible downstream `dx`;
- compatible lateral lattice phase and `dy`;
- centreline endpoint alignment;
- tangent orientation;
- left/right handedness;
- reversed component direction;
- width mismatch;
- junction and branching ownership;
- conservative material transfer.

## 11. Mandatory visual-regression matrix

Before implementation, capture a baseline. After implementation, repeat the same matrix.

### 11.1 Source views

- Automatic Birth Source — live, not cumulative
- Arc only
- Semi-Arc only
- Fleck only
- Shore Ribbon only
- Inward Wash only
- each Free Water pattern independently
- all patterns at production weights

### 11.2 Topology and support views

- final composed topology
- major support
- connector support
- pocket support/negative aging
- shore support
- obstacle exclusion
- object contact field
- motion lane
- obstacle routing

### 11.3 State and rendering views

- persistent Presence
- Remaining Life
- Material Pattern
- visual occupancy/film
- evaluated shape
- production Foam render
- velocity/motion debug
- cache state and topology diagnostics

### 11.4 Geometry cases

- straight constant-width river
- widening river
- narrowing river
- asymmetric bank widths
- left bend
- right bend
- S-bend
- approximately 5 m width
- 10 m width
- 20 m width
- 40 m width
- very short river
- multi-chunk river
- field near contiguous cache limit
- forward and reversed flow

### 11.5 Obstacle cases

- small rock
- long rock
- rotated rock
- bank-adjacent rock
- multiple nearby rocks
- rock renderer hidden but simulation active
- thin obstacle interval
- obstacle crossing a future strip boundary

## 12. Mandatory numerical and performance validation

| Category | Required evidence |
|---|---|
| CPU/GPU coordinate parity | Sampled cell-centre positions and round trips |
| Source placement | Physical source bounds versus affected cell bounds |
| Conservation | Presence/life/pattern mass before/after transport |
| CFL | Downstream term, lateral term, total, and substeps |
| Curvature | Maximum `abs(κn)` and selected policy outcome |
| Topology | Physical areas, lengths, continuity, invalid-cell count |
| Memory | Actual resource bytes and CPU-side arrays |
| Compute | Dispatch count and launched thread envelope |
| Timing | CPU submission and GPU duration |
| Cache | Generation time, package size, hit/miss reason |
| Rendering | Frame time and exact sample alignment |
| Scalability | 5/10/20/40 m and increasing length comparisons |

## 13. Explicit non-dependencies and scope protections

The following must not be changed merely to make the Foam migration easier:

- Ground shaders or Ground generation;
- river corridor material-response behavior;
- Disturbance field resolution or simulation architecture;
- scene or prefab serialized data;
- tags, layers, components, folders, or asset names;
- accepted Arc/Semi-Arc source path logic except where a separately documented metric-unit conversion is required;
- inter-river state transfer in Stage 1;
- world-aligned XZ simulation.

Any newly discovered need to modify one of these areas is a scope-expansion event requiring plan amendment and approval.

## 14. Definition of dependency-complete implementation

The fixed-metric migration is not dependency-complete until:

1. every **U** item is implemented or explicitly removed from scope with evidence;
2. every **D** item has a recorded decision and acceptance criterion;
3. every **T** item has a recorded result;
4. all old normalized-lateral cache artifacts are rejected deterministically;
5. CPU, compute, renderer, debug, topology, obstacle, and source mappings agree;
6. all source families preserve approved physical behavior;
7. topology and routing morphology are no longer accidentally tied to old cell dimensions;
8. Disturbance and corridor systems are verified unchanged;
9. no scene or prefab was modified;
10. the canonical architecture and active-plan documents match the implemented state;
11. a final live-workspace reference scan finds no remaining unauthorized normalized-lateral Foam mapping.

## 15. Final conclusion

The fixed-metric change is a River Foam coordinate-system migration, not a resolution toggle. Its dependencies include every producer, transformer, cache, diagnostic, and consumer of Foam field coordinates, plus integration tests for the river-domain and Disturbance systems that provide external data.

The prior audit identified the most dangerous dependencies but did not enumerate all of them. This register is the required standalone checklist for planning and validating the update against the supplied source snapshot.


## RG-METRIC-P12j — Clean binary Presence Chip eligibility

- `RiverWaterFoamResolveStateMask` exposes one transient clean silhouette from the resolved base mask and existing near-death life gate before material-pattern erosion.
- `RiverWaterEvaluateFoam` carries that scalar through the existing stored/warped/lead/trail coupling, wake stretch, surface-break, retention, and liquid ownership without a new resource or sample.
- Presence-Amplitude eligibility uses Euclidean `ddx`/`ddy` gradient magnitude on the clean silhouette and a binary meaningful-support gate.
- Presence-Amplitude production remains exactly `saturate(candidate * eligibility)`; Interior Access and all derived permission regions remain disabled.
- Current eligibility, Interior Access, production selection, and soft-mask reconstruction remain the protected compatibility path.
- Unity visual validation remains pending.

## RG-METRIC-P12k — Exact pre-Chip rendered-mask ownership

- `RiverWaterFoamResolveStructuralStrandKeep` is resolved before Presence-Amplitude Chip selection.
- The exact no-Chip rendered geometry is `preChipRenderedMask = saturate(foam.mask * strandKeep)`.
- Presence-Amplitude support and edge distance use that mask's existing `0.08` `RiverWaterFoamResolveBaseCoverage` boundary and Euclidean `ddx`/`ddy` gradient magnitude.
- Presence-Amplitude production remains exactly `saturate(candidate * eligibility)` and directly carves the exact pre-Chip rendered mask.
- `Chip Eligibility Composite` displays the same pre-Chip rendered mask, and `Production Chip Mask` displays `preChipRenderedMask - finalFoamMask`.
- P12j `cleanSilhouette` transport is retired because it has no remaining consumer.
- Current eligibility, Interior Access, production selection, soft reconstruction, Strand order, and final result remain the protected compatibility path.
- Unity visual validation remains pending.



## RG-METRIC-P12l — Binary Candidate × Eligibility intersection

- P12k exact pre-Chip rendered-mask ownership remains the Presence-Amplitude geometry source.
- Presence-Amplitude candidate and edge-eligibility analytical fields are converted to explicit binary selected regions at their mathematical `0.5` contours.
- Presence-Amplitude production is exactly `candidateSelected * eligibilitySelected`; no fractional support, opacity, interpolation, derived admission, Interior Access, or secondary permission field participates.
- Every selected Presence-Amplitude production pixel removes the complete exact pre-Chip rendered mask; every unselected pixel preserves it unchanged.
- Presence-Amplitude `Chip Candidate Field`, `Chip Eligibility Composite`, and `Production Chip Mask` display the exact binary candidate, binary eligibility, and binary product used by production.
- Current candidate, eligibility, Interior Access, soft-mask reconstruction, debug values, and final result remain the protected compatibility path.
- Unity visual validation remains pending.


## RG-METRIC-P12m — Any-support binary Candidate × Eligibility selection

- P12l is mechanically valid but visually rejected: its `>= 0.5` tests select only the midpoint contour interiors of the antialiased Candidate and Eligibility fields and discard positive support below `0.5`.
- P12k exact pre-Chip rendered-mask ownership remains unchanged.
- Presence-Amplitude selects Candidate with `chipCandidateField > 0.0` and Eligibility with `chipEligibility.edgeBand > 0.0`.
- Presence-Amplitude production remains exactly `candidateSelected * eligibilitySelected`; every selected pixel removes the complete exact pre-Chip rendered mask and every unselected pixel preserves it.
- Current candidate, eligibility, Interior Access, production selection, soft-mask reconstruction, debug arithmetic, Strand order, and final result remain the protected compatibility path.
- Candidate and Eligibility generation, readability/subpixel gates, Edge Width geometry, controls, shader properties, textures, buffers, kernels, dispatches, persistent state, caches, scenes, prefabs, materials, and serialized fields are unchanged.
- `RiverWaterFoam.hlsl` has one shader consumer, `SH_CleanStylizedRiver.shader`; no other subsystem consumes the changed include. Active-gameplay instruction topology and memory traffic are unchanged because P12m replaces two comparison constants only.
- Inspector descriptions explicitly distinguish Presence-Amplitude binary masks from Current continuous diagnostics. No Debug View identity or enum value changes.
- Unity import and direct Candidate / Eligibility / Production / final-mask / Final comparison remain authoritative and pending.


## P12n dependency disposition — optional Candidate-Straddle admission A/B — visually rejected and superseded by P12o

P12n adds an optional Layer E-only permission route. The established P12m Rendered Edge Band route remains serialized value `0`, remains selectable, and is the fallback when the experimental cache is unavailable. Current Presence Footprint ignores Candidate Straddle and retains its prior Chipping arithmetic.

Dependency additions:

```text
StylizedRiver Chip Application / refresh rate
    -> StylizedRiverFoamRuntime.ChipAdmission
    -> BuildFoamChipStraddleAdmission compute kernel
    -> guarded point RFloat admission texture
    -> River forward fragment candidate-identity lookup
    -> exact pre-Chip rendered-mask removal
```

The admission texture is visual transient state, not Layer C material, cache topology, Film, Shape, or save data. It is allocated only on demand, fully overwritten per refresh, released through normal Foam resource teardown, and history is invalidated on route disable, lattice remap, resize, recreation, or teardown. The compute support evaluator reads interpolated previous/current Layer C state and mirrors the stable Layer E pattern/lifecycle/Strand equations with a fixed world-space footprint; it intentionally excludes screen derivatives and surface-wake deformation. Final removal remains clipped to exact per-fragment pre-Chip rendered Foam.

For the current approximate `64.2 × 6.75 m`, `0.125 m`-spacing domain including implemented guards, the analytical cache is approximately `520 × 67 = 34,840` RFloat texels (`136.1 KiB` logical), one dispatch of `545` groups per refresh, default `4 Hz`. Inactive/dormant candidates, impossible centre classifications, and already-satisfied perimeter tests exit early. Runtime GPU cost remains unmeasured. No other subsystem consumes the new include or texture; `SH_CleanStylizedRiver.shader` remains the sole render consumer.

Validation state: source/offline contract checks pass; Unity 6000.5 import, GPU timing, and visual A/B acceptance are pending.


## P12o dependency disposition — original Candidate Field with boundary-anchored Eligibility

P12o removes P12n candidate-level admission. The existing render-frame analytical Candidate Field remains the sole candidate producer. The experimental dependency now supplies Eligibility descriptors only:

```text
StylizedRiver Chip Application / boundary refresh rate
    -> StylizedRiverFoamRuntime.ChipAdmission (repurposed in place)
    -> BuildFoamChipBoundaryDescriptors compute kernel
    -> guarded point ARGBFloat boundary-descriptor texture
    -> River forward-fragment local strip reconstruction
    -> original Candidate × selected Eligibility
    -> exact pre-Chip rendered-mask removal
```

Each descriptor stores a River-space boundary anchor plus exact candidate identity and boundary state in the same ARGBFloat record. `Z` packs lateral identity, state (`0` unacquired, `1` valid, `2` locked), and a 10-bit inward-normal angle into one exact 24-bit integer; `W` stores the exact longitudinal identity. Initial acquisition stops at the first centre/ring occupied-empty disagreement, refines that known bracket by four binary-search steps, and locally refines the normal. Valid tracking searches from the previous boundary anchor and normal with intermediate samples so thin ribbons are not skipped; it cannot follow the moving candidate into Foam or reacquire a different edge during the same living cycle.

The P12m Rendered Edge Band route remains serialized value `0`, source default, fallback, and behaviorally protected. Enum value `1` is reassigned from rejected Candidate Straddle to `Boundary-Anchored Strip (Experimental)` so existing experimental scene values select the replacement without scene editing. `foamChipStraddleRefreshRate` migrates through `FormerlySerializedAs` to `foamChipBoundaryRefreshRate`.

The descriptor texture is transient Layer E state, not Layer C, Film, Shape, topology cache, or save data. It is allocated only while Presence-Amplitude + Boundary-Anchored Strip + nonzero Chipping and Edge Width are requested. For the previously observed guarded `520 × 67` candidate lattice, one ARGBFloat record per identity is `557,440` logical bytes (`544.4 KiB`). Absolute identities use a circular modulo index, so downstream lattice-origin movement preserves history without a second texture; decoded coordinates reject stale slots. The exact representable contract is longitudinal `±16,000,000` and lateral `-2048…2047` candidate cells, with fallback outside it. One low-frequency dispatch updates a bijective set of unique records in place. No high-resolution topology field, distance transform, extra pass, draw call, or new persistent simulation field is introduced.

Shared-shader impact remains confined to `SH_CleanStylizedRiver.shader`, the sole consumer of `RiverWaterFoam.hlsl`. Current Presence Footprint and the P12m fallback perform no descriptor texture load. Unity import, visual A/B, and GPU timing remain pending.

## P12p dependency disposition — retire experimental cache and isolate rendered exterior fringe

P12n Candidate Straddle and P12o Boundary-Anchored Strip are visually rejected and removed. Their serialized route selector, refresh rate, runtime allocation/update/release paths, descriptor texture, compute kernel/include, shader properties, fragment descriptor loads, and experiment-only source files have no surviving dependency.

The retained production chain is:

```text
original full-rate analytical Candidate Field
    ×
rendered Eligibility band
    -> exact binary Presence-Amplitude Production
    -> complete removal from exact pre-Chip rendered Foam
```

Presence-Amplitude Eligibility still reads `preChipRenderedMask`, but distance is derived only from the normalized exterior-fringe branch between the existing visible start `0.08` and rendered fringe ceiling `0.34`. The coordinate is clamped at `0.34`; therefore the inner hardened-body rise cannot restart the derivative estimate or create a second permission contour. Current Presence Footprint retains its established `preChipSoftVisibility` path unchanged.

Dependency impact is one shared River shader include and its sole consumer, `SH_CleanStylizedRiver.shader`. No texture, buffer, kernel, dispatch, cache, serialized control, render pass, scene, prefab, material, layer, or tag remains from the experiment. Memory and compute return to the P12m baseline. Unity import and visual validation remain pending.

## P12r — Binary-topology removal and P12p dependency restoration

- P12q's topology partial, topology compute include, three kernels, three `R8` textures, fixed-frequency lifecycle dispatch, serialized Eligibility route/metre-width controls, material texture/mode binding, and memory accounting are removed.
- The dependency graph returns exactly to P12p: `StylizedRiver` owns only `foamChipEdgeWidthPixels`; runtime binding supplies no Chip Eligibility texture or mode; `CS_RiverFoam.compute` has no Chip-topology include or kernels; `SH_CleanStylizedRiver.shader` consumes only the rendered per-fragment Eligibility path.
- `RiverWaterFoamResolveChipEligibility` again owns the sole Presence-Amplitude Eligibility signal using the isolated rendered exterior-fringe coordinate. The original Candidate evaluator and complete selected-pixel removal remain unchanged.
- No new dependency replaces P12q. P12q must not be reintroduced as an active or fallback route.
- Offline dependency audit passes: all ten restored implementation files match P12p byte-for-byte, all four topology-only files are absent, and no topology symbol/property/kernel remains. Unity compile/import and visual validation remain pending.



## P12s dependency disposition — optional Presence-Amplitude soft-mask reconstruction

**Status:** source implemented; Unity import and visual A/B pending.

P12s adds no new dependency category. It extends the existing Layer E render-only contract with two scalar material properties:

```text
_FoamPresenceChipApplicationMode
_FoamChipSoftEdgeStart
```

Ownership and flow:

```text
StylizedRiver serialized authoring
→ StylizedRiverFoamRuntime material-property binding
→ SH_CleanStylizedRiver forward fragment
→ RiverWaterFoamResolveChipEligibility
→ RiverWaterFoamEvaluateSelectionDiagnostics
→ RiverWaterFoamApplyChipAndStrands
```

Route `0`, `Exact Rendered Removal (Current)`, preserves the P12r binary any-support Candidate × isolated rendered-fringe Eligibility and complete deletion from `preChipRenderedMask`.

Route `1`, `Soft-Mask Reconstruction (Experimental)`, preserves the original continuous analytical Candidate field, computes a continuous soft Eligibility coordinate from `preChipSoftVisibility`, authored `Soft Edge Start`, and `Chip Edge Width`, gates it by binary `preChipRenderedMask > 0`, and reuses the accepted rehardened soft-mask reconstruction before structural Strands.

The selector is ignored by Current Presence Footprint, which retains its accepted soft reconstruction. Presence-Amplitude Interior Access remains disabled in both routes.

Impact classification:

- Layer C state, source generation, transport, lifetime, cadence, caches: unchanged.
- Layer D Film/Shape and `_FoamShapeMask`: unchanged.
- Layer E Candidate loop/search geometry: unchanged.
- Textures, samplers, buffers, kernels, dispatches, passes, draw calls: unchanged.
- Persistent memory: two serialized scalars per River component only; no GPU allocation.
- Active fragment work: one uniform route branch and existing reconstruction arithmetic when route `1` has production Chip coverage.
- Shared include consumers: only `SH_CleanStylizedRiver.shader`; no cross-subsystem consumer found.

Validation requirements are exact property/binding parity, shader signature/caller parity, P12r exact-route preservation, Current-route preservation, candidate-core byte identity, and Unity same-frame Candidate/Eligibility/Production/Probe/Final A/B.

## P12t dependency disposition — Soft Reconstruction baseline and Inspector ownership

- Presence-Amplitude Chipping now has one application contract: continuous original analytical Candidate × soft Eligibility, applied to the pre-hardened signal and rehardened before structural Strands.
- The removed `_FoamPresenceChipApplicationMode` scalar has no remaining authoring, runtime-binding, material-property, shader-uniform, or function-argument dependency.
- Exact Rendered Removal-only Eligibility, binary selection, and direct final-mask deletion have no remaining consumer.
- Production Chipping and its Candidate/Eligibility/Production/Probe/Difference diagnostics are Layer E render-only dependencies. Layer D evaluated-shape textures and temporal controls remain diagnostic-only and do not feed normal Final Foam.
- Runtime resources, kernels, dispatches, textures, buffers, passes, draw calls, cache contracts, and fixed-metric state are unchanged.


## P12u dependency disposition — unified automatic birth reveal speed

`RG-METRIC-P12u` changes automatic Layer C source scheduling only. All eight automatic recipes now share one CPU timing contract:

```text
requested reveal speed = base authored speed × pattern multiplier × deterministic jitter
raw duration = source path distance / requested reveal speed
resolved duration = max(one material update step, raw duration)
```

- Shore Ribbon and Inward Wash no longer use the historical `0.85–14 s` duration clamp.
- Object Contact Arc and Semi-Arc use the shared duration for Build only; Hold, Release, Rest, persistent-emitter ownership, and phase-shaped GPU evaluation are unchanged.
- Object Contact Fleck uses the shared duration and consumes normalized progress across the full `0–1` reveal interval.
- Lace, Cross-Lace, and Torn Fragment no longer use family-specific ceilings or Torn Fragment timing compression.
- Contact Fleck and all Free-Water correlated Min/Max samples use the complete deterministic `0–1` range.
- The automatic-event GPU ABI, 32-slot pool, source dispatch ranges, transport, material state, cache, Film, Shape, and rendering dependencies are unchanged.
- Runtime risk is bounded by the existing 32-event pool. Slow honest events can increase concurrent raster work and rejected starts; the P12u report exposes current occupancy and rejection evidence rather than modifying timing.
- One editor-only report partial and Inspector action are added. They allocate no runtime GPU resource and use the existing diagnostic report/clipboard contract.


## P13A dependency disposition — authoritative material and Coverage separation

P12t soft-reconstruction Chipping and P12u unified Reveal Speed are frozen and closed. The user accepted P12t as the sufficient visual baseline and reported P12u working as expected. P13A does not reopen Candidate geometry, Chipping application, Reveal Speed, source scheduling, or negative topology.

The persistent state reuses the existing ARGBHalf texture with no allocation change:

```text
R = Coverage × intrinsic Presence
G = Coverage × intrinsic Presence × Remaining Life
B = Coverage × intrinsic Presence × Material Pattern
A = geometric Coverage
```

Dependency flow:

```text
source shape / taper / breakup / subcell footprint / valid fluid
    -> geometric Coverage
authored Initial Presence + Initial Life
    -> intrinsic material properties
Coverage + intrinsic material
    -> coherent packed Layer C state
    -> Donor Cell or Coverage-only TVD Superbee transport
    -> explicit Layer C aging
    -> Final Visibility policy
    -> optional exact Presence-weighted resolved mask
    -> accepted P12t Chipping / Strands / composition
```

- Automatic and manual sources no longer use Initial Presence as source-fill probability or geometric attenuation. Shape-family multipliers remain Coverage shaping only.
- Birth overlap is an explicit single-cohort `Max + Refresh` approximation: maximum Coverage, maximum intrinsic Presence, maximum Remaining Life, with Pattern changed only by genuinely added Coverage.
- Valid-fluid clipping changes Coverage and proportional packed moments while preserving decoded Presence, Life, and Pattern.
- Convergent unit-capacity resolution also clips Coverage coherently and re-encodes the same intrinsic ratios instead of saturating packed channels independently.
- Donor Cell transports one coherent packed state. TVD Superbee reconstructs one bounded Coverage scalar and re-encodes the donor's coherent intrinsic material rather than independently limiting four packed channels.
- Concentration + Lifetime intentionally permits Coverage dilution and Remaining Life to reduce visibility. Lifecycle-Faithful uses meaningful Coverage for occupancy and leaves ordinary survival to explicit Layer C aging.
- Coverage-Only is the Inspector-facing name for serialized enum value `Current`; it ignores intrinsic Presence as visual amplitude. Presence-Amplitude carries exact decoded Presence through identical Presence-independent shape/surface-coupling weights so uniform Presence remains exactly proportional in the completed resolved mask.
- The three selectors move, without duplication, to `Foam > Transport & Visibility Contract`, followed by a permanently visible read-only explanation of each choice, the combined result, and persistent state meanings.
- Raw R-channel transport metrics remain valid but are relabelled `Material Amount`; they are no longer described as intrinsic Presence.
- The legacy transient fallback interprets positive RGB with zero alpha as old material amount, assigns Coverage equal to that amount and intrinsic Presence `1`, then rewrites the P13A contract on the next clamp/remap/update. Persistent caches are not changed.

Impact classification:

- textures, formats, samplers, buffers, kernels, dispatches, passes, draw calls, and persistent GPU memory: unchanged;
- automatic-event GPU/CPU ABI and 32-slot pool: structurally unchanged;
- P12t Candidate loop, Eligibility, soft reconstruction, and Strand order: unchanged;
- per-face TVD arithmetic: one Coverage limiter plus coherent decode/encode instead of independent packed-channel limiting; measured GPU cost pending;
- convergent capacity clipping: coherent Coverage re-encoding replaces independent packed-channel saturation; no new resource or dispatch;
- source-visible quantity and Lifecycle-Faithful persistence may increase substantially by design because hidden suppression is removed.


## RG-METRIC-P13B — Packet-rearmed birth and object-contact retention

P13B changes automatic Layer C source ownership and the existing obstacle velocity field; it does not change the P13A material packing, transport schemes, Final Visibility modes, Presence Footprint modes, lifecycle, or P12t Chipping.

Automatic-source contract:

- Shore and Free-Water have fixed deterministic slot spacing. Coverage selects a stable share of those slots. Activity is linear from zero to the existing maximum attempt rate.
- Each accepted slot is rearmed only after its event duration plus a distance-derived Minimum Packet Gap. Contact Flecks use per-object active/rearm ownership and cannot start while the same object owns an active contact cycle.
- Current-minus-previous reveal is a permission test; newly reached cells receive the complete current Coverage target. This prevents repainting behind the head without making Coverage depend on reveal cadence.
- Fleck reveal is spatial. Arc/Semi-Arc wakes are one-shot Build products. Only the immediate contact front is refreshed during Hold and progressively withdrawn during Release.
- Flecks are independent of the normalized Arc/Semi-Arc cycle mix and are controlled directly by Fleck Coverage, Activity, and packet gap.

Obstacle velocity contract:

```text
existing RGHalf obstacle texture
R = signed lateral-routing influence
G = independent slowdown influence
```

The existing one-sided collision route remains. A narrow dirty-time all-side contact halo writes slowdown only, allowing front/side/rear Foam to approach the exact authored Minimum Downstream Factor without lateral redirection. No texture, buffer, upload class, compute kernel, shader sample, pass, or draw call is added.

Control cleanup removes automatic-source Breakup Strength authoring and shader evaluation, Lace/Cross-Lace gap masks, Torn Fragment bite masks, Fleck mix weight, and confirmed unused Arc/Semi-Arc arm-reach/lopsidedness controls. Reserved event-record lanes remain structurally present for ABI compatibility.

Expected aggregate runtime work is lower because slots cannot immediately restart, finite packets stop writing behind their heads, and object wake arms are not refreshed after Build. Added CPU work is bounded rearm lookup and obstacle-dirty contact stamping; velocity adds a bounded multiply-and-lerp falloff calculation without a transcendental operation. Measured performance remains pending Unity profiling. P13B offline validation is `28/28 PASS`; Unity compilation, D3D11 import, Play Mode visual acceptance, and profiling remain pending.


Dependency classification:

- modified CPU producers: automatic source scheduler/event state and obstacle-routing field builder;
- modified GPU consumers: automatic-source raster evaluation and canonical velocity resolver;
- unchanged persistent material consumers: simulation packing/merge, Donor/TVD, lifecycle, Layer D/E state sampling;
- unchanged rendering ownership: P13A visibility contract and P12t Chipping/Strands;
- resource-count delta: zero;
- serialization removal: obsolete source controls only; new packet-gap fields retain standard Unity missing-field defaults.

## RG-METRIC-P13C — One-shot object packets and full-vector contact retention

P13C supersedes only the P13B Object Arc/Semi-Arc persistence and object-contact velocity details. P13A packed material, Donor/TVD transport, lifecycle, visibility, P12t Chipping, P12u reveal-speed resolution, Shore sources, and Free-Water sources remain unchanged.

Object-source dependency flow:

```text
registered object anchor
    -> shared per-object eligibility state
    -> Arc / Semi-Arc / Fleck selection
    -> one-shot Build-only source event
    -> current-minus-previous deposition permission
    -> ordinary P13A Layer C material
    -> shared halo + packet-gap rearm estimate
```

- Arc, Semi-Arc, and Fleck share one active-owner and `NextStartTime`; no recipe can overlap or bypass another recipe for the same object.
- Arc/Semi-Arc event duration equals resolved Build duration. Hold, Release, Rest, and persistent contact-front refresh are removed from event state, scheduling, rasterization, diagnostics, and Inspector authoring.
- The shared rearm estimate begins when the event completes. It includes the authored contact-slowdown outer reach travelled at the exact full-contact speed factor, then the authored Object Contact Minimum Packet Gap travelled at base Foam downstream speed.
- If contact slowdown is enabled and the authored minimum speed factor is zero, automatic object rearm remains disabled until the authority changes. No hidden scheduler speed floor overrides stagnation.
- A successful Fleck yields the next eligible opportunity to Arc/Semi-Arc when contact cycles are enabled, preventing high Fleck Activity from starving contact packets.

Canonical velocity dependency:

```text
base downstream + signed lateral routing
    -> routed float2 velocity
existing obstacle G slowdown influence
    -> one contact speed factor
    -> multiply complete routed float2
```

At full slowdown influence, downstream, lateral, and total routed velocity are scaled by the exact authored Object Contact Minimum Speed Factor. Signed routing direction is preserved. Authored Full Slowdown Reach and Slowdown Outer Reach rebuild only the existing dirty-time RGHalf obstacle field and are included in its signature.

Impact classification:

- modified CPU producers: object event scheduling/state and existing obstacle-field stamping;
- modified GPU consumers: Object Arc/Semi-Arc source evaluation and canonical velocity arithmetic;
- resource-count delta: zero;
- no new buffer, texture, kernel, dispatch, shader sample, pass, draw call, scene, prefab, material, layer, tag, or component;
- expected active-gameplay source-raster work decreases because Build-only events leave the 32-slot pool sooner and perform no Hold/Release dispatches;
- Offline validation passes `35/35`; Unity compilation, Play Mode source/retention acceptance, and profiling remain pending.


## RG-METRIC-P13D — Finite Object Contact Reinforcement Burst

P13D modifies only Object Arc/Semi-Arc source-event scheduling and source-raster interpretation. The accepted P13C full-vector object-contact slowdown, shared object clearance gate, and authored slowdown reaches remain unchanged.

Object contact dependency:

```text
resolved per-stroke Build duration
    × authored stroke count [1, 3]
    -> one finite source event duration
stroke 0
    -> accepted complete Arc/Semi-Arc packet
stroke 1 / stroke 2
    -> immediate contact profile only
final stroke completion
    -> existing shared halo + packet-gap rearm
```

- `Object Contact Stroke Count` is an authored integer with range `1–3` and default `2`.
- Each stroke uses the same resolved Reveal Speed and per-stroke Build duration. Increasing the count does not accelerate any stroke.
- Stroke zero preserves the accepted Arc/Semi-Arc contact and finite wake geometry. Later strokes never regenerate wake arms.
- Phase identity is carried through existing CPU event state and existing GPU event lanes; no GPU record field, buffer, texture, kernel, resource declaration, dispatch class, pass, draw call, or shader sample is added.
- At each stroke boundary, previous-deposition ownership resets so the first interval of the new contact-only stroke cannot be suppressed by progress wrapping from `1` back to `0`.
- Source Coverage/Presence/Life merge, transport, topology support, negative aging, P13C slowdown, shared rearm, and Layer E rendering remain unchanged.

Impact classification:

- modified CPU producers: Arc/Semi-Arc event duration, stroke phase/progress resolution, and finite-burst diagnostics;
- modified GPU consumer: Arc/Semi-Arc source evaluation and previous-deposition phase reset;
- bounded active-gameplay cost: at most two additional contact-only source sweeps per object event;
- expected default cost: one additional contact-only sweep because the default stroke count is `2`;
- no continuous emission is restored and no wake-arm repetition occurs;
- `36/36` offline validation passes. Unity compilation, Play Mode contact-establishment acceptance, and profiling remain pending.

## RG-METRIC-P13E — Independent Object Contact Reinforcement Cadence

P13E separates released Object packet cadence from maintenance of deliberately retained contact material. It changes CPU authoring, scheduling, event-state interpretation, and diagnostics only. The P13D initial finite burst, P13C full-vector slowdown, P13A packed material/transport/visibility, P12u reveal resolver, and all Layer E implementation remain unchanged.

Per-object authority becomes:

```text
full packet clock
    = released wake-arm length + Object Contact Minimum Packet Gap
      travelled at normal Foam downstream speed

reinforcement clock
    = one authored interval after the previous full contact burst or
      completed reinforcement
```

- `Object Contact Reinforcement Enabled` defaults to `true`.
- `Object Contact Reinforcement Interval (s)` has range `1–30 s` and default `6 s`.
- Full Arc/Semi-Arc packets retain the P13D `1–3` initial-stroke burst. Reinforcement is always one finite progressive contact-only stroke.
- Reinforcement reuses the last successful Arc/Semi-Arc recipe and deterministic seed, emits no wake-arm Coverage, never changes the full-packet clock, and cannot overlap any same-object event.
- A full packet that is already eligible blocks reinforcement. While a packet remains in clearance, due reinforcement is attempted before Flecks. Existing P13C pending-Fleck fairness remains at full-packet eligibility.
- Full packet rearm no longer includes slowdown-halo reach or minimum contact speed because the contact material is intentionally retained there. Fleck completion still advances only the shared packet clock.
- The existing event GPU layout, 32-slot pool, two-start-per-update budget, compute kernels, resources, dispatch classes, textures, shader samples, passes, and draw calls are unchanged.

Impact classification:

- added serialized state: one bool and one bounded interval scalar on `StylizedRiver`;
- added CPU state: one reinforcement authority signature, one clock and remembered contact recipe/seed per registered object, one CPU-only event classification flag, and bounded counters;
- active-gameplay cost: one finite contact-profile source event per enabled participating object per authored interval while its next full packet remains in clearance;
- no full-field or per-fragment cost delta;
- **PERFORMANCE EXCEPTION — approved:** bounded contact-only maintenance is accepted because the finite P13D initial burst did not reliably maintain supported contact Coverage. Disabling reinforcement is the zero-added-work alternative.

Offline validation and Unity acceptance are recorded with the delivered P13E package; Unity compilation, visual cadence review, and profiler evidence remain required before closure.

## RG-METRIC-P13F — Full Initial Contact Ring and Recipe-Complete Reinforcement

P13F changes only bounded Object Arc/Semi-Arc source geometry, per-phase reveal timing, and their diagnostics. The P13E scheduler, P13C complete-vector contact slowdown, P13A packed material and transport, P12u reveal-speed resolver arithmetic, and Layer E rendering remain protected.

Object source contract:

```text
initial Arc/Semi-Arc stroke
    -> one-cell ring around the complete obstacle boundary
    -> then the recipe's finite wake geometry once
later Arc stroke / periodic Arc reinforcement
    -> complete five-point Arc contact profile only
later Semi-Arc stroke / periodic Semi-Arc reinforcement
    -> deterministic selected half-profile only
```

- The complete ring is derived inside the existing bounded source-raster dispatch from the already-bound obstacle-exclusion texture. Eight neighbouring obstacle samples resolve boundary confidence and outward direction; no full-field contact build is added.
- The initial path length is `2 × complete front-profile length + one-time wake-arm length(s)`. Later contact strokes use their actual complete-Arc or selected-Semi path length.
- One requested Reveal Speed and deterministic jitter remain authoritative. Initial and later strokes resolve separate cadence-bounded durations so the longer first stroke does not make later contact strokes artificially slow.
- The previously reserved `Deposit.w` lane carries contact-stroke path length. The source event remains eight `float4` values; buffer stride, capacity, bindings, kernels, resources, passes, and draw calls are unchanged.
- Current-minus-previous deposition remains one-shot within each phase. A phase transition resets previous-shape subtraction so each finite later stroke receives one new birth opportunity.
- Wake geometry is first-stroke-only. No Hold, Release, persistent material-cadence emitter, or repeated wake emission returns.

Impact classification:

- CPU: separate initial/contact path and duration evidence per active Arc/Semi-Arc event;
- GPU: eight local obstacle-neighbour reads for phase zero only, inside the existing packet dispatch rectangle;
- memory: four CPU-only event fields; no additional GPU allocation or ABI growth;
- active-gameplay work: one finite complete-ring sweep per full object packet; later strokes retain bounded profile evaluation;
- lower-cost fallback: the P13E front-only first stroke;
- Unity compilation, Play Mode geometry acceptance, and measured performance remain pending.

## RG-METRIC-P13G — Object Spawning Acceptance Freeze and Weather-Shader Integration Boundary

P13G is documentation-only. It freezes the accepted P13B–P13F automatic-source and Object-source dependencies and records the external-edit boundary for pending Weather cloud-shading integration.

User acceptance establishes the current spawning baseline:

- automatic spawning and Object spawning are done for the current milestone;
- P13F works as expected and is materially better than the former persistent-emitter behavior;
- the initial complete obstacle-contact ring, recipe-complete Arc/Semi-Arc later strokes, finite contact-maintenance cadence, first-stroke-only wake ownership, shared object scheduling, and full-vector contact slowdown are retained;
- remaining River issues are not closed by this freeze and must be selected separately after the external shader work.

Protected dependency boundary:

```text
P13B–P13F source scheduling / source geometry
    -> frozen
P13C obstacle-contact velocity
    -> frozen
P13A packed material / transport / visibility
    -> frozen
P12u Reveal Speed
    -> frozen
P12t Layer E Chipping
    -> frozen
Weather cloud-shading integration in shared River shaders
    -> externally owned, pending post-change audit
```

The Weather thread may modify shared River shader files for cloud shading, but it must not silently change Foam source-event data, Layer C packing/transport/lifecycle, obstacle routing/slowdown, Foam visibility, Candidate/Eligibility, Chipping, or Strands. Because shared includes can affect multiple subsystems, the resumed River thread must inspect the exact external diff rather than rely on intent.

Impact classification:

- runtime compute: unchanged;
- dirty-triggered compute: unchanged;
- CPU/GPU memory: unchanged;
- project storage: small Markdown-only increase;
- `PERFORMANCE EXCEPTION`: none.

P13G modifies only the five canonical River Foam documents. Unity validation is not required for this documentation update. Compilation and focused spawning regression become mandatory after the external Weather shader integration is supplied.


## D8.15 Shore scheduling and fixed-metric dependency override

This section is the authoritative replacement for earlier entries stating that Shore Coverage selects a stable slot share or that Shore Activity maps to a fixed global attempt rate.

- Removed dependency: Shore Coverage. No Shore Ribbon or Inward Wash location is permanently masked.
- Existing dependency retained: fixed internal scheduling spacing `3.5 m` per bank. The slot population is length-derived with `2 * ceil(validFieldLength / 3.5 m)` entries and all entries remain eligible.
- Activity dependency: per-slot duty cycle, not global events/second. Minimum Packet Gap remains the physical clearance lower bound.
- Length dependency: Ribbon Length Min/Max and Inward Along-Bank Length Min/Max remain authored in cells. Runtime values are inclusive whole integers selected per event.
- Shore-contact dependency: `_FoamCurrentShoreEdgesRead` remains the sole current visible-edge authority. Ribbon and Inward start offset is fixed to the nearest inward cell centre; no authored Shore offset remains.
- Ribbon path dependency: existing longitudinal Foam-grid metric plus one current-shore-edge sample at each path column. New path textures, buffers, CPU paths, GPU path-build passes, and readbacks are forbidden for this contract.
- Capacity dependency: source-event capacity is `32 + shoreBucketCount`; reservation capacity is twice that value and is allocated only when the grid descriptor is applied.

No transport, lifecycle, topology, boundary, obstacle-routing, or final-render dependency changes.
