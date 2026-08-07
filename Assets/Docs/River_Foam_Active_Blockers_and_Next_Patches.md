## RIVER-FOAM-SPAWN-D8.4D — Object contact flat-head repair

- Component-isolated Smoke evidence measured both Arc and Semi-Arc contact-only 1×1 heads at `1.438` cells while every isolated wake head measured exactly `1`.
- The shared contact evaluator used bounded-segment distance against the complete profile and then gated by path distance. Samples beyond a moving reveal boundary could therefore remain inside the width radius of the boundary point, producing rounded head caps.
- Moving contact-head intervals now clip each profile segment to the exact revealed path interval and require an unclamped projection inside the clipped segment before width testing. This removes reveal-boundary caps.
- Full-body contact evaluation is intentionally unchanged to avoid redesigning accepted Arc/Semi-Arc body geometry. Wake geometry, component isolation, source-event ABI, serialized controls, scenes, transport and final rendering are unchanged.

## RIVER-FOAM-SPAWN-D8.4C — Object Arc/Semi-Arc component isolation and head repair

- Production Arc and Semi-Arc contact strokes now use `HeadWidthCells` uniformly whenever `HeadLengthCells` defines a moving-head interval; full-body intervals retain `ContactWidthCells`.
- Their wake arms now use the authoritative head width uniformly during moving-head intervals instead of tapering from body width to head width.
- The existing Smoke suite adds five diagnostic-only component rows: Arc contact, negative wake, positive wake, Semi-Arc contact, and the selected positive wake. Production dispatches explicitly reset component mode to the complete union.
- No source-event ABI, scene, serialized control, transport, or final-rendering change is introduced.

## RIVER-FOAM-SPAWN-D8.4C — Object Arc/Semi-Arc component isolation and head repair

- Production Arc and Semi-Arc contact strokes now use `HeadWidthCells` uniformly whenever `HeadLengthCells` defines a moving-head interval; full-body intervals retain `ContactWidthCells`.
- Their wake arms now use the authoritative head width uniformly during moving-head intervals instead of tapering from body width to head width.
- The existing Smoke suite adds five diagnostic-only component rows: Arc contact, negative wake, positive wake, Semi-Arc contact, and the selected positive wake. Production dispatches explicitly reset component mode to the complete union.
- No source-event ABI, scene, serialized control, transport, or final-rendering change is introduced.

# River Foam Active Blockers and Next Patches

## RIVER-FOAM-SPAWN-D8.4A — Cell-Exact Ribbon Caps and Progressive Heads

Status: implemented in source; Unity compilation and runtime validation pending.

### Objective

Correct the production geometry defects demonstrated by the D8.3C11 Smoke and Exhaustive reports without changing the source-event ABI, serialized controls, scenes, prefabs, materials, transport, or final rendering. Correct the Shore/Inward diagnostic fixture so it supplies the same cell-valued legacy fields as production.

### Approved files

- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.P7Diagnostics.cs`
- `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`

### Reviewed evidence

- Lace and Cross-Lace 1×1 integrated Coverage is approximately 1.75–1.875 and 1×3 is approximately 9.75–10.25. The shared bent-ribbon evaluator measures distance to bounded segments, creating semicircular endpoint extensions.
- Object Fleck body areas track requested dimensions, but a 1×1 head on a 5×3 body produces approximately 10 cells because the evaluator always rasterizes the full body segment.
- Broken Filament uses the same bent-ribbon evaluator and inherits endpoint expansion and body-to-head width interpolation.
- The Shore/Inward audit fixture writes `WidthMetres = widthCells * dy` and `FeatherMetres = 0`, while the D8 packing path consumes those legacy-named fields as cell counts.

### Implementation

1. In `FoamEvaluateBentRibbonCoverage4x4`, reject samples whose raw projection lies outside each polyline segment, removing endpoint caps while preserving interior segment joins.
2. Use body width only when the active interval spans the complete body; otherwise use the declared head width uniformly across the moving head.
3. In `FoamEvaluateObjectContactFleckSource`, derive the revealed longitudinal interval from progress and head length, rasterize only that interval, and use the authoritative active body/head width.
4. In `BuildCellSpawnerAuditEvent`, write Shore/Inward width and head width as cell values through the legacy-named fields.

### Invariants and non-goals

- No event ABI or struct-layout change.
- No additional dispatch, texture, buffer, or persistent allocation.
- No Arc/Semi-Arc correction in this update.
- Broken Filament stochastic gating remains unchanged.
- Existing 4×4 raster supersampling remains unchanged.

### Acceptance criteria

- Lace and Cross-Lace body cases approach integrated areas 1, 5, and 3 for requested 1×1, 5×1, and 1×3 footprints.
- Lace, Cross-Lace, Broken Filament, and Fleck moving-head cases are bounded by the declared head dimensions.
- Fleck retains correct body scaling and its 1×1 head case falls from approximately 10 cells toward 1 cell.
- Corrected Shore/Inward fixtures supply three cells for the 1×3 width case and one cell for the head-width case.
- Smoke remains asynchronous and Editor-responsive.

### Performance

The bent-ribbon evaluator retains the existing fixed 4×4 samples and seven path points. It adds a raw projection range test and can skip distance work outside a segment. Fleck adds constant scalar interval calculations. Runtime complexity, dispatch count, textures, buffers, and persistent memory remain unchanged.

### Validation status

- Static source audit: passed. Final diff is limited to the three approved files; shader delimiters are balanced; the shared bent-ribbon evaluator has exactly the intended Lace, Cross-Lace, and Broken Filament consumers; Arc/Semi-Arc evaluators and event ABI are unchanged.
- Unity shader/C# compilation: pending in Unity 6000.5.0f1.
- Smoke suite: pending.


## Current status

`RG-METRIC-P2` through `RG-METRIC-P12d` are closed. The Unity P12d matrix completed all 12 fixed-spacing/lateral cases with `Overall: PASS`, restored authored runtime ownership, and left the assigned cache unchanged. Visual review selected fixed spacing `0.15 m`.

`RG-METRIC-P12e — Presence-amplitude rendering and TVD transport A/B` is Unity-imported and visually exercised. Presence-Amplitude materially improves thin-strip footprint control. The selected fixed spacing remains `0.15 m`.

`RG-METRIC-P12f — Presence-Amplitude Chip-edge parity` is rejected by Unity visual evidence. It derived Presence-Amplitude edge distance from the hardened `preChipMask`; that signal contains a fringe ramp and a hard-body ramp, so derivative-normalized edge distance detected an exterior contour and a false interior contour. Production Chip still multiplied each candidate by the narrow per-pixel edge band, so admitted candidates were clipped into fragmented stripes rather than removed as complete connected bites.

`RG-METRIC-P12g — Mode-specific single-contour Chip admission` is Unity-imported and partially accepted. Its Presence-Amplitude eligibility coordinate is correct: Unity shows one exterior yellow contour and no false interior contour. Its production admission is rejected: the doubled projected candidate diameter authorizes large interior magenta removals that are not representative of the narrow eligibility band. The accepted `Current` mode remains protected and unchanged.

`RG-METRIC-P12h — Edge-attached Presence Chip bites` is rejected by Unity visual evidence. Its one-reach production region was still a second permission mask derived from the yellow eligibility mask, so Production Chip removal remained materially broader than `Chip Eligibility Composite` and still admitted removal outside the selected area.

`RG-METRIC-P12i — Exact Presence Chip eligibility ownership` is Unity-rejected as an eligibility-quality implementation. Production remained a strict subset of the displayed mask, but the displayed Presence-Amplitude mask itself is derived from procedurally eroded `preChipSoftVisibility` and multiplied by fractional visible-support intensity. Unity evidence shows stippled/pixel-cluster eligibility and faint fringe that receives too little removal authority. `Current` remains protected and unchanged.

`RG-METRIC-P12j — Clean binary Presence Chip eligibility` is rejected by Unity visual evidence. Its clean committed-Presence/life silhouette is broader than the actual rendered Foam because material-pattern erosion and structural Strand shaping occur afterward. Unity `Chip Eligibility Composite` shows eligibility contours detached from the rendered body by a spatially varying distance.

`RG-METRIC-P12k — Exact pre-Chip rendered-mask ownership` remains retained. It correctly moved Presence-Amplitude eligibility to the exact no-Chip rendered mask passed to final composition: `preChipRenderedMask = foam.mask × strandKeep`.

`RG-METRIC-P12l — Binary Candidate × Eligibility intersection` is mechanically valid but rejected by Unity visual evidence. Its `>= 0.5` tests select only the midpoint contour interiors of two antialiased fields, so positive Candidate or Eligibility support below `0.5` remains unselected and can preserve Foam. `RG-METRIC-P12m — Any-Support Binary Chip Selection and Full-Removal Proof` corrected that threshold defect. `RG-METRIC-P12n — Optional Candidate-Straddle Chip Admission A/B` and `RG-METRIC-P12o — Original Analytical Candidates with Boundary-Anchored Eligibility` are visually rejected and removed. `RG-METRIC-P12p — Retire Experimental Cache and Isolate the Rendered Exterior Fringe` restored one original analytical Candidate Field multiplied by one rendered Eligibility band whose distance coordinate excludes the inner hard-body rise; later P12s/P12t work superseded only its application route.

`RG-METRIC-P12q — Binary Morphology Eligibility` is Unity-rejected and fully removed by `RG-METRIC-P12r`. The source is restored to the P12p rendered-edge implementation with no topology route, resource, kernel, control, or fallback retained.

`RG-METRIC-P12s — Optional Presence-Amplitude Soft-Mask Reconstruction A/B` is visually accepted as the production direction. `RG-METRIC-P12t — Soft Reconstruction Baseline and Layer D/E Inspector Reconciliation` promotes that route to the sole Chipping application, removes Exact Rendered Removal and its selector, moves Production Chipping into Layer E, and labels Layer D evaluated-shape controls as diagnostic-only. The user accepted the resulting Chipping appearance as imperfect but sufficient; P12t is frozen and closed.

`RG-METRIC-P12u — Unified Automatic Birth Reveal-Speed Contract` is user-validated as working as expected and is frozen and closed. Its live report output and performance profile remain optional evidence work rather than open correctness blockers.

`RG-METRIC-P13A — Authoritative Birth Material and Coverage-Separated Transport` and `RG-METRIC-P13A.1 — D3D11 Struct-Selection Compile Hotfix` are implemented and Unity-imported. P13A remains the current Coverage/Presence/Life baseline; P13A.1 removed the D3D11 struct-valued conditional without changing its calculations.

`RG-METRIC-P13B` through `RG-METRIC-P13F` establish packet-rearmed automatic birth, one-shot Object packets, complete-vector contact retention, finite initial reinforcement, independent contact-maintenance cadence, a complete initial obstacle-contact ring, and recipe-complete later Arc/Semi-Arc contact strokes. The user reports that the final P13F result works as expected and is materially better than the former persistent-emitter implementation. Automatic spawning and Object spawning are accepted and frozen for the current milestone. Remaining River issues exist but are outside the active spawning scope.

Weather cloud shading is integrated and user-tested. The post-Weather source audit found the River change confined to the native URP `_LIGHT_COOKIES` ForwardLit variant; P13F/P13G spawning, Layer C, object retention, and Layer E contracts remain unchanged. Arc and Semi-Arc packets are accepted and remain frozen.

`RIVER-MOTION-S3.1` through `RIVER-MOTION-S3.1E.3` are Unity-tested and accepted. The closed Stage 3 result includes shoreline-safe trough restoration, sign-independent full-surface detail authority, one analytical ordinary/overflow shoreline, adaptive hidden-band tessellation, a complete signed-brightness Shoreline Accent, opaque-scene edge blending, post-solve overflow variation, deterministic profile evolution, and independently authored shore-wave Length and Gap. The final accepted packet model is one positive zero-slope lobe across the complete authored Length plus one explicit nonnegative Gap; Gap `0` makes adjacent packets meet without Length adding hidden calm distance. `RIVER-MOTION-S3.1D.1` resolved the D3D11 reserved-token compile failure. S3.1E, S3.1E.1, and S3.1E.2 are rejected historical attempts and must not be restored.

During the Length/Gap work, Play Mode Foam entered `PreparationRequired / GridDescriptorMismatch` because the assigned topology cache predated the current structural river domain. Explicit Edit Mode cache preparation rebuilt the exact current descriptor and restored normal Foam startup. Length, Gap, profile evolution, shoreline accent, and other live motion/render controls remain outside the immutable Foam grid descriptor and must not invalidate the topology cache.

`RIVER-FOAM-VIS-D1` is Unity-imported and exercised. Its held-state Coverage report and pipeline composite prove that `Concentration + Lifetime` suppresses broad low-Coverage support before later Pattern/life shaping, while `Lifecycle-Faithful` exposes substantially more coherent stored structure. That visibility result is diagnostic evidence, not the root correction: the newly exposed structure is too thick and blob-like because transported Coverage expands from thin source ribbons into broad fractional support. The active blocker is now Layer C ribbon-shape preservation during transport.

## RIVER-FOAM-TRANSPORT-D1 — Ribbon compactness diagnostic suite

**Status:** Unity-completed. The one-button suite passed all formula self-checks, completed the 1D/2D/live matrices, resolved all three live anchor classes, and produced the accepted D1 baseline report in `72,032.246 ms`. This patch is diagnostic-only. Production births, persistent state, lifecycle, transport kernels, selected transport/visibility contracts, rendering, topology caches, and authored defaults remain unchanged.

### Problem statement and rationale

The current conservative finite-volume solve preserves integrated packed material but does not prove preservation of a thin ribbon footprint. A one-cell or subcell ribbon may spread into several lower-Coverage cells while decoded Presence and Remaining Life remain coherent. `Concentration + Lifetime` hid most of that spread; `Lifecycle-Faithful` exposed it. The project requirement is not to hide transported material after the fact. A ribbon may advect, bend, split, and age, but its local thickness should remain approximately invariant unless an explicit authored or lifecycle process changes thickness.

The next implementation decision must therefore be based on measured shape transport, not only conservation or final-render appearance. The suite is intentionally broader than one scene observation: it separates intrinsic discretization diffusion from 2D orientation/CFL effects and from deformation caused by the live Motion Lane, obstacle routing, slowdown, valid-fluid clipping, and curvilinear metrics.

### Objective

Add one Play Mode Inspector action that executes a deterministic, source-free CPU mirror of the current Layer C scalar Coverage transport and writes one clipboard-ready report. The mirror must use the same Donor Cell/Superbee reconstruction, coherent Coverage capacity, fixed-metric cell/face geometry, canonical live velocity contract, and valid-fluid clipping as production. It must not mutate live Foam state or serialized River settings.

### Acceptance criteria

1. One button runs the complete matrix; one adjacent button copies the combined report.
2. The suite records the live descriptor, spacing, metric-row, Motion Lane, obstacle-routing, slowdown, boundary, obstacle-exclusion, CFL, and selected contract inputs before testing.
3. An exact 1D matrix covers both transport schemes, initial widths 1/2/4/8 cells, full-cell and quarter-cell Coverage, CFL values 0.10 through 0.90, and travel checkpoints 0.25/0.50/1/2/4 m.
4. A 2D Cartesian matrix covers both schemes, flow-aligned and cross-flow ribbons, widths 1/2/4 cells, downstream/lateral/diagonal uniform velocity, CFL 0.25/0.50/0.75, and checkpoints 0.5/1/2 m.
5. A live-field matrix uses automatically selected open, high-lateral-intent, and obstacle-influenced anchors. It compares anchor-resolved downstream-only frozen-local-uniform velocity, live downstream-only velocity, and complete live velocity for both schemes, both ribbon orientations, and widths 1/2/4 cells.
6. Every checkpoint reports integrated Coverage conservation, peak Coverage, thresholded support at C=0.02/0.10/0.20/0.30/0.50, centroid displacement error, designated thickness and length, principal minor/major extent, connected components, and low-Coverage tail fractions.
7. The report separates solver diffusion, lateral/shear deformation, obstacle/slowdown influence, resolution dependence, and subcell-amplitude dependence. It must not recommend a production correction automatically.
8. The suite writes under `Library/RiverFoamDiagnostics`, updates the existing latest-report surface, restores no state because it never mutates live state, and fails explicitly when required live arrays are unavailable.

### Reviewed evidence and constraints

- `StylizedRiverFoamRuntime.P8Diagnostics.cs::ValidateP8ConservativeTransport` proves packed-moment conservation but does not measure ribbon shape.
- `CS_RiverFoam.Simulation.hlsl::FoamTransportSuperbeeSlopeComponent` returns zero at isolated extrema; a one-cell ribbon therefore begins with first-order donor behavior.
- `CS_RiverFoam.Simulation.hlsl::FoamResolveInteriorFaceDonor` reconstructs Coverage only and carries coherent intrinsic material.
- `CS_RiverFoam.compute::SimulateFoam` applies conservative face flux, capacity clamp, valid-fluid clipping, then lifecycle aging.
- `StylizedRiverFoamRuntime.Obstacles.cs` retains CPU copies of resolved Motion Lane, obstacle routing, obstacle exclusion, and readable boundary Coverage required for a no-mutation live-field mirror.
- Fixed spacing remains `0.15 m`; automatic and Object spawning remain accepted and frozen.

### Approved files

- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.RibbonTransportDiagnostics.cs` (new)
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.RibbonTransportDiagnostics.cs.meta` (required Visible Meta Files companion)
- `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`
- `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
- `Assets/Docs/River_Foam_Stage6_Architecture.md`

### Invariants and non-goals

- No compute shader, shader include, kernel, texture, buffer, render pass, source recipe, lifecycle formula, transport selection, visibility mode, Chipping, Strand, cache, scene, prefab, material, layer, or tag change.
- No synchronous GPU readback and no per-frame runtime work. The suite runs only from one explicit user action.
- The CPU mirror is diagnostic evidence. Any production solver/compression/interface design requires a separate reviewed patch after the report is analyzed.
- The suite must preserve unrelated River state and use invariant-culture numeric output.

### File-by-file sequence

1. Record this plan and the architecture rationale.
2. Add the isolated CPU transport mirror, matrix runner, measurements, anchor selection, report writer, and public status/report surface.
3. Add one Run button and one Copy button to the active Foam diagnostic actions.
4. Perform static formula parity, matrix cardinality, conservation self-check, scope, documentation, and package audits.
5. Leave Unity 6000.5.0f1 execution pending for the user and request the complete copied report.

### Risks and validation gates

- A CPU mirror can diverge from HLSL through indexing, limiter, metric, or clipping differences. The suite therefore includes closed-form one-step donor checks, Superbee extremum checks, interior-plateau conservation checks, and explicit formula/version text in the report.
- Live-field anchor selection may fail in narrow or obstructed rivers. `Open / Low-Lateral` requires `|lane| <= 0.10`; `Open / High-Lateral` requires `|lane| >= 0.25`; both require negligible routing/slowdown and a `0.90`-valid seed neighbourhood. `Obstacle-Influenced` requires a valid centre but intentionally permits clipped neighbouring seed cells so exclusion/boundary deformation can be measured. Missing classes are reported as skipped rather than silently substituted.
- Large matrices can stall the Editor. The implementation uses compact arrays, bounded domains, deterministic cases, and a stopwatch; elapsed time is reported.
- Unity compilation and one-button execution are mandatory before this diagnostic patch can be accepted.

### Implementation status

- [x] Review current transport, velocity, metric, valid-fluid, diagnostics, report, and canonical documentation surfaces.
- [x] Record objective, rationale, scope, invariants, matrix, risks, and validation plan before code edits.
- [x] Implement the one-button suite and copy action.
- [x] Complete post-implementation parity/scope/documentation audits.
- [x] Run in Unity and analyze the copied report.

### Unity execution evidence

- Unity `6000.5.0f1` ran the complete suite in `WindowsEditor` on `River_Strip` in `VisualFrameworkDemo.unity`.
- All five formula/self-check groups passed: donor closed form, Superbee isolated-extremum fallback, limiter sign/monotone samples, interior-plateau conservation, and 2D diagonal conservation.
- The report completed the full 1D and 2D matrices, resolved `3/3` live anchor classes, and ended with `Overall execution: PASS`.
- The accepted D1 interpretation is transport compactness failure rather than material creation: integrated Coverage remains conserved while thin support broadens into lower-Coverage cells.

### Post-implementation audit evidence

- Exact baseline comparison against the supplied source archive plus `RIVER-FOAM-VIS-D1` overlay contains only the five approved delivered files: the new diagnostic partial and required `.meta`, the Inspector action file, this active plan, and the Stage 6 architecture document.
- The CPU mirror preserves the production scalar Coverage contract from `CS_RiverFoam.Simulation.hlsl::FoamTransportSuperbeeSlopeComponent`, `FoamResolveInteriorFaceDonor`, `FoamResolveLongitudinalFaceFlux`, `FoamResolveLateralFaceFlux`, and `CS_RiverFoam.compute::SimulateFoam`: closed invalid faces, physical longitudinal outflow only, averaged interior face velocity, Donor/Superbee reconstruction, simultaneous x/y flux, metric-area mass update, `[0,1]` clamp, and valid-fluid capacity clip.
- Live travel distance integrates Coverage-mass-weighted mean speed, while expected centroid displacement integrates the Coverage-mass-weighted velocity vector. The live input mirror reproduces the canonical Motion Lane scroll interpolation, signed obstacle routing, complete-vector slowdown, fixed-metric cell/face geometry, and downstream flow-direction sign. `FrozenLocalUniform` is intentionally synthetic and holds only the anchor-resolved downstream component spatially uniform; `LiveDownstreamOnly` adds captured downstream gradients/slowdown, and `CompleteLive` then adds captured lateral intent/routing.
- The implemented matrix contains `144` one-dimensional parameter cases with `720` checkpoint rows, `108` Cartesian parameter cases with `324` checkpoint rows, and up to `108` live-field parameter cases with `324` checkpoint rows when all three anchor classes resolve.
- Independent float32 formula checks reproduce the isolated-cell Donor update `[1-CFL, CFL]`, Superbee extremum fallback, monotone limiter slope `0.5`, interior-plateau conservation within `4.77e-7` cell units, and 2D diagonal conservation within `1.12e-8 m²`. The suite repeats equivalent checks in Unity before reporting success.
- C# delimiter/preprocessor structure, owned symbol uniqueness, public Inspector call sites, report-writer availability, exact changed-file scope, and invariant-culture raw numeric output were checked offline. No Unity compiler is available in this environment; import and execution remain authoritative and pending.


## RIVER-FOAM-TRANSPORT-D2 — Conservative compactness candidate tournament

**Status:** Implemented offline and statically audited as a diagnostics-only follow-up to the Unity-completed D1 report. Unity 6000.5.0f1 import and one-button execution remain pending. No production transport, birth, lifecycle, visibility, rendering, cache, scene, prefab, material, layer, tag, or authored default changes are included.

### D1 evidence and problem statement

The Unity D1 suite completed all formula checks and synthetic/live matrices. It proved that integrated Coverage is conserved while thin support broadens:

- one-cell cross-flow ribbons widen by roughly `5.5×` after `1 m` under the current fixed-metric TVD Superbee transport even with frozen uniform downstream velocity;
- flow-aligned ribbons remain exactly one-cell thick under uniform downstream velocity, but the complete live lateral field widens them substantially;
- 4–8-cell ribbons are materially more stable, confirming that the present `0.15 m` lattice under-resolves the intended thin features;
- `Lifecycle-Faithful` exposes the transported state honestly, while `Concentration + Lifetime` only hides its low-Coverage tails.

The required correction must therefore compact the transported Layer C state conservatively. It must not erase Coverage, shorten lifetime, weaken Presence, reduce births, or hide the result in Layer E.

### Historical transport audit

The supplied repository contains only tombstones for the removed `4.11C.5.4c` predictor/corrector/compression path:

- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Transport.hlsl`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamSimulation.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthTransfer.cs`

No recoverable implementation, validation report, failure rationale, or version-control history is present in the supplied archive. D2 must not restore or infer that deleted design. Every candidate is an isolated new CPU diagnostic model with explicit formulas, bounds, and evidence.

### Objective

Add one Play Mode Inspector action that runs a deterministic conservative compactness tournament over the trusted D1 CPU transport mirror. Compare the current TVD Superbee baseline with bounded interface-compression and flux-corrected anti-diffusive candidates, exact geometric translation references, and higher-resolution Superbee references. Produce one clipboard-ready report that ranks evidence but does not alter production selection.

### Candidate families

1. **Baseline TVD Superbee:** exact D1 transport with no compactness correction.
2. **Normal interface compression:** after each transport substep, move Coverage mass from lower normalized fill `q=C/V` toward the adjacent higher-fill cell using a bounded face transfer proportional to `q(1-q)` and a dimensionless compression Courant.
3. **Flux-corrected anti-diffusion:** after each transport substep, move Coverage mass up the local normalized-fill gradient using a bounded face transfer proportional to `|Δq|`.
4. **Combined conservative correction:** apply a weak normal-compression pass followed by a weak anti-diffusive pass to test whether the two mechanisms complement rather than destabilize each other.
5. **Exact geometric rectangle reference:** analytically translate the original axis-aligned ribbon under uniform velocity and rasterize exact cell-overlap Coverage. This is the zero-diffusion shape target for synthetic cases.
6. **Higher-resolution Superbee references:** rerun selected synthetic cases at `2×` and `4×` linear resolution and downsample conservatively to the base lattice. These are cost/quality references, not proposed runtime defaults.

All conservative candidates operate in physical mass units, normalize interface decisions by valid-fluid capacity, accumulate equal-and-opposite face transfers, globally limit donor outflow and receiver capacity before application, and preserve `0 <= C <= V`.

### Acceptance criteria

1. One button runs the complete D2 suite; one adjacent button copies the latest report.
2. The suite reuses the D1 live descriptor, metric, boundary, obstacle, Motion Lane, routing, slowdown, cadence, and velocity capture without mutating live Foam.
3. Candidate self-checks prove conservation, capacity bounds, uniform-field invariance, binary-interface invariance, zero-gap non-bridging, partial-valid-fluid safety, and deterministic repeatability.
4. A synthetic tournament covers both ribbon orientations, widths `1/2/4`, initial Coverage `1.00/0.25`, downstream/lateral/diagonal uniform velocity, production CFL plus low/high comparison CFLs, and `0.5/1/2 m` checkpoints.
5. An adversarial topology suite covers two parallel ribbons with multiple gaps, two detached blobs, an L bend, a Y split, a hollow ring, a checkerboard field, and a smooth fractional hump. It reports unintended merging, component loss/creation, hole filling, oscillation, and bound violations.
6. A live-field tournament covers the D1 open-low-lateral, open-high-lateral, and obstacle-influenced anchors, both orientations, widths `1/2/4`, frozen downstream-only, live downstream-only, and complete live velocity modes.
7. Every checkpoint retains D1 mass, centroid, covariance, support, component, and tail metrics and adds support compactness plus local branch-thickness median, 95th percentile, maximum, and global-separation excess at `C >= 0.10`.
8. Synthetic uniform cases are compared against the exact geometric reference; selected cases also compare against `2×/4×` Superbee references.
9. The report applies provisional evidence thresholds—open-field mass error `<0.1%`, no bounds violation, centroid error `<0.5` cell, and target thickness growth `<=1.5× / 1.25× / 1.15×` for initial widths `1/2/4` after `1 m`—but must label the ranking diagnostic rather than changing production.
10. The suite writes under `Library/RiverFoamDiagnostics`, updates the latest diagnostic report surface, performs no GPU readback, and adds no per-frame work.

### Approved files

- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.CompactnessTournamentDiagnostics.cs` (new)
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.CompactnessTournamentDiagnostics.cs.meta` (new required companion)
- `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`
- `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
- `Assets/Docs/River_Foam_Stage6_Architecture.md`

The existing D1 diagnostic file may be read and reused but is not approved for modification unless a compile-level sharing defect is discovered. Any such deviation requires plan revision before editing.

### Invariants and non-goals

- No production compute shader or shader edit.
- No serialized field or authoring control.
- No runtime candidate selection or default change.
- No source, lifetime, Presence, Pattern, Chipping, Strand, or final-composition tuning.
- No attempt to reconstruct the deleted `4.11C.5.4c` implementation from its tombstone.
- No claim that a CPU candidate is production-ready without a later GPU parity and runtime-performance patch.

### File-by-file sequence

1. Record this plan and the historical-audit limitation.
2. Add the D2 candidate solver, conservative limiter, local-thickness measurement, synthetic/adversarial/live matrices, exact/high-resolution references, scoring, report writer, and public status surface.
3. Add the Run and Copy controls beside D1 in the active Inspector diagnostics panel.
4. Document the formulas, interpretation contract, provisional thresholds, and non-production status in Stage 6 architecture.
5. Run offline deterministic self-checks, candidate-bound tests, matrix-cardinality checks, delimiter/preprocessor checks, changed-file scope audit, and package-byte audit.
6. Leave Unity 6000.5.0f1 compilation and execution pending for the user; request the complete copied D2 report.

### Risks and validation gates

- Anti-diffusion can generate oscillation, checkerboarding, topology merging, or capacity overshoot. The common face-transfer limiter must scale all outgoing and incoming transfers before simultaneous application, and the adversarial suite must fail these pathologies explicitly.
- Global covariance can confuse branch separation with branch thickening. D2 therefore adds thresholded four-axis directional-run local thickness and reports separation excess independently.
- Compression may oppose physically authored divergence. Live-field results must be reported separately from uniform-shape preservation; no candidate may be promoted solely because it forces all live cases toward the original global covariance.
- CPU evidence cannot prove HLSL parity or performance. A selected candidate requires a separate reviewed GPU experiment.
- Unity compilation and one-button execution are mandatory before D2 can be accepted.

### Implementation status

- [x] Review D1 report, current transport formulas, D1 CPU mirror, live-field capture, Inspector report plumbing, canonical docs, and removed-path tombstones.
- [x] Record D2 objective, candidates, matrix, metrics, invariants, risks, and approved files before code edits.
- [x] Implement the candidate tournament and Inspector controls.
- [x] Complete offline audit and package the changed files.
- [ ] Run in Unity and analyze the copied report.

### Post-implementation audit evidence

- Exact changed-file scope is limited to the two new diagnostic files, the Inspector action surface, this active plan, and the Stage 6 architecture document. D1, production compute/shader files, runtime simulation, births, lifecycle, visibility, caches, scenes, prefabs, materials, layers, tags, and serialized defaults remain byte-identical to the supplied D1-overlaid baseline.
- The tournament contains eight candidates: the unchanged TVD Superbee baseline, three normal-compression strengths, three anti-diffusion strengths, and one weak hybrid. Candidate ranking only selects baseline plus the strongest two non-baseline candidates for live evidence; it cannot change production.
- Offline parser/delimiter scans pass with no merge markers, TODO/FIXME markers, unbalanced braces, or duplicate method declarations. Referenced D1 helper types/methods and current runtime properties were resolved against their declarations.
- An independent numerical mirror passed all eight candidate invariant checks and `800` randomized partial-capacity conservation/bounds cases. This verifies the limiter arithmetic independently of the embedded Unity self-checks; it does not replace Unity compilation or execution.
- Candidate correction uses physical mass, normalized valid fill, equal-and-opposite interior-face transfer, global donor/receiver limiting, simultaneous application, and final `0 <= C <= V` enforcement. Uniform, binary, fractional-ramp, zero-gap, partial-capacity, conservation, and deterministic-repeatability checks are embedded in the Unity report.
- Synthetic cardinality is deterministic: up to `2,592` exact-reference checkpoints for eight candidates, plus nine adversarial patterns, selected `2x/4x` resolution references, and up to `486` live-field checkpoints for baseline plus two ranked candidates across three anchors.
- No Unity compiler is available in the delivery environment. Unity import, execution time, candidate self-check results, complete ranking, and live-anchor evidence remain authoritative and pending.

## Superseded visibility blocker — diagnostic finding retained for provenance

`RIVER-FOAM-VIS-D1` completed this diagnostic chain. Chipping was not the primary loss; the held-state evidence showed that the selected scalar visibility base rejected broad low-Coverage support before later Pattern/life shaping:

```text
Layer C Material Coverage
Layer C Material Presence
Layer C Material Remaining Life
Layer C Material Amount
Layer C Material State Composite
Layer E Visibility Pipeline Composite
Final Foam
```

Interpret the pipeline composite explicitly:

- red without green: meaningful Coverage exists, but the selected scalar visibility-policy base rejects it;
- green without blue: later pre-Chip Pattern/life shaping, surface coupling, or Presence-footprint selection removes it;
- blue support absent from Final Foam: only then inspect Chipping, Strands, final opacity/colour composition, or shoreline coverage blending.

The Coverage report measured `4,634` nonzero material cells, of which `2,428` reached `C >= 0.02`; only `526` received any meaningful `Concentration + Lifetime` base support. `Lifecycle-Faithful` restored coherent structure but exposed the actual transport defect: thin source ribbons had expanded into broad fractional-Coverage support. The stale cache was rebuilt and is not an open defect. This section is superseded by `RIVER-FOAM-TRANSPORT-D1`; do not use visibility suppression, birth reduction, Presence decay, or lifetime reduction as a substitute for compactness evidence.

## RIVER-FOAM-VIS-D1 — Coverage and visibility diagnostics

**Status:** Unity-imported and exercised. The held-state Coverage report, Material State Composite, and Visibility Pipeline Composite were accepted as authoritative evidence. This diagnostic patch does not change birth, lifecycle, transport, cache, visibility, Chipping, Strands, composition, or authored defaults.

### Objective

Expose the missing geometric Coverage evidence and the exact production visibility stages required to determine why strong, living Layer C material is not becoming Final Foam. Preserve the current `TVD Superbee / Concentration + Lifetime / Coverage-Only` production contract while gathering evidence.

### Acceptance criteria

1. Add a literal linear `Material Coverage` view sampled from the same temporally interpolated committed Layer C state as Presence and Remaining Life.
2. Add a literal linear `Material Amount` view showing packed `Coverage × Presence`.
3. Add a `Material State Composite` with red = Coverage, green = decoded Presence, blue = decoded Remaining Life; Presence and Life remain black below the existing meaningful-Coverage authority threshold.
4. Add a `Visibility Pipeline Composite` with red = meaningful raw Coverage footprint, green = the selected visibility-policy base mask before pattern/life shaping, and blue = the exact production pre-Chip Foam mask after pattern/life shaping, surface coupling, and Presence-footprint selection.
5. Add one explicit Play Mode Coverage Distribution report. It must read the same previous/current committed state pair and captured interpolation alpha, decode Coverage/Presence/Life, print Coverage buckets, integrated Coverage, Material Amount, life-weighted Material Amount, and the selected visibility-policy base response. The report must state that exact pre-Chip visibility remains fragment/pixel dependent and is inspected through the blue channel of `Visibility Pipeline Composite`, not approximated on CPU.
6. Keep all new enum values explicit and outside existing serialized values. Existing debug identities and production arithmetic remain unchanged.
7. Inspector descriptions must define every channel mathematically and must not imply that Presence or Remaining Life represent occupied cell area.

### Reviewed evidence

- `Game/Procedural/Rivers/StylizedRiver.cs::StylizedRiverFoamDebugView` and `ResolveFoamDebugView` own serialized debug identities and validation.
- `Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs` owns Layer C labels, descriptions, and debug-specific Inspector actions.
- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader::SampleCommittedFoamState` samples the same previous/current Layer C pair used by Final Foam; existing Presence and Remaining Life views apply only a binary Coverage authority gate.
- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl::RiverWaterFoamResolveStateMask` computes the selected base visibility mask before `RiverWaterFoamPatternedMask`; `RiverWaterEvaluateFoam` later produces the exact coupled pre-Chip production mask in `RiverWaterFoamResult.mask`.
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs`, `.Resources.cs`, `.Lifecycle.cs`, and `.Binding.cs` establish the ARGBHalf committed-state resources, dimensions, interpolation alpha, fixed `MaterialContourSharpness = 0.78`, and renderer bindings.
- Existing Async GPU readback patterns in `StylizedRiverFoamRuntime.BirthDiagnostics.cs` and `.Compute.cs` establish the non-blocking diagnostic ownership pattern.

### Approved file scope

Modify exactly:

1. `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Game/Procedural/Rivers/StylizedRiver.cs`
3. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs`
4. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.VisibilityDiagnostics.cs` — new diagnostic-only partial file
5. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl`
6. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader`

Create exactly one C# partial file. No scene, prefab, material, cache, metadata, layer, tag, component, texture allocation, compute resource, kernel, dispatch, render pass, or draw-call change is authorized.

### Implementation sequence

1. Record this plan before implementation.
2. Extend the explicit serialized debug enum and resolver.
3. Expose the selected pre-pattern base visibility through `RiverWaterFoamResult` without changing mask calculations.
4. Implement the four shader debug substitutions from already-available state/result values.
5. Add Layer C selector labels, mathematical descriptions, and the Play Mode report actions.
6. Implement asynchronous previous/current ARGBHalf readback, exact CPU decode/interpolation, Coverage buckets, and clipboard-ready reporting.
7. Run exact-scope diff, serialized-value uniqueness, C#/HLSL delimiter and call-arity checks, production-arithmetic comparison, shared-shader consumer audit, and package reproduction. Unity import and visual validation remain authoritative.

### Invariants and risks

- Final Foam, `foam.mask`, Chipping, Strands, lighting, opacity, transport, lifecycle, source deposition, and topology cache state remain byte-for-byte behaviorally unchanged except for carrying one diagnostic `baseVisibility` value through the existing result structure.
- The CPU report measures committed Layer C state and the scalar visibility-policy base function. Procedural pattern erosion, screen derivatives, surface warp/wake coupling, and Presence-footprint selection are pixel-space operations; the report must not claim to reproduce them. The Visibility Pipeline Composite is the authoritative visual comparison for those later stages.
- Async readback is explicit user action only and adds no steady-state runtime work.

### Implementation result and post-edit audit

- Added explicit serialized debug values `27` through `30` for Material Coverage, Material Amount, Material State Composite, and Visibility Pipeline Composite; all 24 Foam debug enum values remain unique and the resolver accepts each new value.
- Added an explicit Play Mode `Capture Coverage Report` action. It asynchronously reads the committed previous/current `ARGBHalf` state pair, captures interpolation and contract settings including Hold Foam State, and reports Coverage buckets, integrated `C`, `C×P`, `C×P×L`, weighted Presence/Life, and the selected scalar base response.
- Exposed only the already-computed selected base mask as `RiverWaterFoamResult.baseVisibility`. The HLSL signature and all four call sites use 19 arguments; existing mask, Pattern/life shaping, coupling, Chipping, Strand, composition, resource, kernel, dispatch, property, and pass arithmetic is unchanged.
- Exact-scope comparison contains six files: five approved modifications and one approved diagnostic-only partial file. C#/HLSL/shader delimiter checks pass; HLSL preprocessor directives balance; Layer C and Layer E label/value arrays remain paired; the shared Foam include has one shader consumer, `SH_CleanStylizedRiver.shader`.
- The CPU concentration-base implementation reproduces the shader formula at fixed sharpness `0.78`: `low = 0.1674`, `high = 0.5288`, exponent `2.04`. The six-file delivery archive reproduces the audited working tree byte-for-byte when overlaid on the supplied baseline.
- Unity shader compilation, render-view correctness, `ARGBHalf` readback/channel sanity, and report generation passed in the user scene. The captured report measured strong Presence (`0.936749` Coverage-weighted), substantial Remaining Life (`0.746135` material-weighted), and broad low-Coverage support rejected by the concentration base. `Lifecycle-Faithful` exposed that support as coherent but excessively thick foam, transferring the active blocker from visibility to transport compactness.

The detailed patch records below preserve their original patch-time status and terminology for provenance. Where those historical records conflict with the Current status or Current active blocker above, the current sections govern.

### P12g reviewed evidence

- The rejected P12f baseline in `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl::RiverWaterFoamResolveChipEligibility` selected `preChipMask` / `0.08` when `presenceFootprintMode > 0.5`. `RiverWaterFoamHardenSoftVisibility` constructs that mask as `max(smoothstep(0.22, 0.58, soft), smoothstep(0.06, 0.34, soft) * 0.34)`, which has distinct fringe and hard-body rises. Unity `Chip Eligibility Composite` screenshots show both rises being detected as yellow contours, including one inside rendered Foam.
- The same include computes `edgeSelection = result.chipCandidateField * chipEligibility.edgeBand`. This clips a connected analytical candidate to the narrow edge band at every fragment. Unity `Production Chip Mask` and Final screenshots show partial striped/pixel-fragmented removal instead of complete candidate bites.
- The accepted pre-P12f `Current` path is preserved in the immutable reconstructed post-P12e baseline: `preChipSoftVisibility`, edge start `0.06`, the existing `fwidth` normalization, per-pixel edge-band selection, and soft-mask reconstruction in `RiverWaterFoamApplyChipAndStrands`.
- `RiverWaterFoamResolveBaseCoverage(preChipMask)` begins rendered support at `preChipMask = 0.08`. Solving the unchanged `RiverWaterFoamHardenSoftVisibility(soft) = 0.08` relation gives `soft = 0.148228` to six decimal places. Presence-Amplitude can therefore use the same monotonic `preChipSoftVisibility` coordinate as Current while beginning at its actual rendered-support boundary.
- Candidate evaluation already computes each candidate's projected radius and the bounded multi-axis maximum shape-reach scale. These values can admit a complete connected candidate when its contour intersects the Presence-Amplitude edge band without a new texture, buffer, kernel, distance transform, dispatch, property, or serialized field.
- `Shaders/SH_CleanStylizedRiver.shader` already owns `_FoamPresenceFootprintMode`, passes it to selection evaluation, and calls `RiverWaterFoamApplyChipAndStrands` for production and evaluated-shape preview. Mode-specific application requires only propagating the existing uniform into those two calls.
- The mode producer, Inspector, runtime binding, shader property, P12 reports, source/lifecycle system, Layer C transport, Film, Shape, and cache contracts were reviewed and require no change.
- The supplied source snapshot has no Git metadata. Historical comparison is against the immutable reconstructed post-P12e and post-P12f baselines plus the accepted patch archives.

### P12g objective and acceptance criteria

1. Preserve `Current` mode arithmetic and behavior exactly: soft-visibility edge source, `0.06` edge start, derivative normalization, edge-width partition, candidate × edge-band selection, interior access, and soft-mask reconstruction.
2. For `Presence-Amplitude` only, use `preChipSoftVisibility` with calibrated start `0.148228` so one monotonic coordinate follows the actual rendered-support boundary and cannot restart at the hard-body ramp.
3. Keep `Chip Eligibility Composite` candidate-independent: yellow remains the narrow exterior permission band and must not show a false interior contour.
4. For `Presence-Amplitude` only, admit complete connected edge candidates using the candidate's current projected maximum contour diameter plus authored Chip Edge Width. Do not clip the removal shape to the narrow eligibility band.
5. For `Presence-Amplitude` only, apply admitted Chip selection directly to the already-hardened pre-Chip mask so one connected analytical candidate produces one coherent removed bite rather than threshold-fragmented soft-mask remnants.
6. Preserve Presence-Amplitude itself exactly as `baseMask = min(baseMask, presence)`. Do not add amplitude compression, new control, threshold tuning, source/lifetime changes, or overall-Foam tuning.
7. Preserve candidate identity, lifecycle, shape formulas, Interior Access, Current-mode output, TVD/Donor transport, fixed spacing `0.15 m`, source behavior, Layer C state, Film, Shape, colour, lighting, and all Debug View identities.
8. Unity acceptance requires warning-free import; one exterior yellow contour in Presence-Amplitude; coherent connected magenta candidate bites; no false interior yellow contour; no pixel-fragmented partial removals; and unchanged accepted Current behavior.

### P12g approved file scope and implementation sequence

1. **Plan/status first:** this document, then `River_Foam_Fixed_Metric_Dependency_Register.md`, `River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`, `River_Foam_Stage6_Architecture.md`, and `River_Rendering_Roadmap.md`.
2. **Eligibility contract:** `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl` restores the accepted Current branch and gives Presence-Amplitude the calibrated monotonic soft-visibility start plus exposed local inward distance.
3. **Candidate admission:** the same include retains Current per-pixel clipping exactly, while Presence-Amplitude accumulates complete candidates admitted by Edge Width plus the current bounded projected candidate diameter.
4. **Chip application:** the same include keeps Current soft-mask reconstruction exactly and adds direct hardened-mask carving for Presence-Amplitude.
5. **Production callers:** `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader` passes `_FoamPresenceFootprintMode` into both production and evaluated-preview Chip application calls.
6. **Validation:** exact-scope diff audit; immutable-baseline comparison; Current-mode arithmetic/model equivalence; calibrated edge-start derivation; single-contour monotonicity; connected-candidate admission proof/model; direct-carve boundedness; function-signature/call-site scan; shader braces/preprocessor/property scan; cross-subsystem shared-shader audit; protected-file/resource/kernel/property/serialized-field comparison; archive byte reproduction. Unity shader import and visual comparison remain authoritative and pending.

### P12g risks and non-goals

- The projected inward coordinate is a local derivative approximation, not a global signed-distance field. Candidate-diameter expansion is a bounded local admission method; Unity visual evidence remains authoritative.
- Complete candidate admission may remove more area than P12f because it restores the connected analytical bite instead of clipping it to a narrow stripe. It must remain constrained by Chip Edge Width, candidate size, activation, lifecycle, and visible support.
- Current mode is a protected compatibility branch. No cleanup, shared retuning, or direct-carve behavior may leak into it.
- No Presence-Amplitude compression, transport change, source change, new Debug View, report, control, texture, buffer, kernel, dispatch, cache, scene, prefab, or material change is included.

### P12g implementation status

- [x] Complete read-only post-P12f review and record evidence, invariants, approved scope, implementation sequence, risks, and validation plan.
- [x] Update the remaining canonical documents.
- [x] Implement mode-specific single-contour eligibility, connected candidate admission, and Chip application.
- [x] Complete final consistency/compliance validation and package reproduction: 48/48 primary gates and 32/32 independent gates pass; protected files remain byte-identical; Current eligibility and Chip application are exact across 250,000 cases each; calibrated single-contour sampling passes 360,036 cases; connected-candidate admission passes the exact-distance bound model; direct carving passes 250,000 bounded cases; changed HLSL functions parse with the Clang HLSL frontend; archive extraction reproduces all seven files byte-for-byte.

### P12h reviewed evidence

- Unity `Chip Eligibility Composite` evidence after P12g shows one correct exterior yellow contour in Presence-Amplitude. Therefore `RiverWaterFoamResolveChipEligibility` with `preChipSoftVisibility` and calibrated start `0.148228` is retained unchanged.
- Unity `Production Chip Mask` evidence after P12g shows broad interior magenta candidate regions that extend far beyond the yellow eligibility contour. Final rendering shows corresponding excessive interior bites.
- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl` currently computes `candidateAdmissionDepthPixels = Chip Edge Width + 2 * projectedContourReachPixels`. With the accepted scene defaults `Chip Edge Width = 5 px`, `Stable Candidate Radius = 3 px`, and bounded maximum shape reach `1.52`, the minimum representative production depth is `5 + 2 * (3 * 1.52 + 1) = 16.12 px`, before larger candidates or pulse expansion. This proves production permission is materially wider than the displayed five-pixel eligibility band.
- The P12g code tests `chipEligibility.estimatedInwardPixels` independently at each fragment. It does not evaluate a candidate centre or exact candidate/edge intersection. The implementation is therefore a broad per-pixel permission region, not literal candidate-level admission.
- A candidate touching an exterior edge band can extend inward by approximately one projected candidate reach beyond the band. `Chip Edge Width + projectedContourReachPixels` is the correct low-cost local bound for an edge-attached bite; the extra second reach in P12g is unjustified.
- Presence-Amplitude direct hardened-mask carving is retained because P12f's soft-mask reconstruction produced fragmented partial removals.
- The accepted Current path is preserved in the immutable post-P12g baseline: edge start `0.06`, candidate × edge-band selection, Interior Access, and soft-mask reconstruction.
- The shared Water shader/include cross-subsystem audit found the changed functions are used only by River Foam evaluation and its existing Debug Views; no Ground, Disturbance, reflection, refraction, wetness, lighting, or non-Foam render path consumes the admission-depth arithmetic.
- The supplied source snapshot has no Git metadata. Historical comparison is against the immutable reconstructed post-P12g baseline and accepted patch archives.

### P12h objective and acceptance criteria

1. Preserve `Current` eligibility, candidate selection, Interior Access, and Chip application byte-for-byte.
2. Preserve the accepted Presence-Amplitude single-contour eligibility coordinate and yellow `Chip Eligibility Composite` output exactly.
3. For Presence-Amplitude only, replace `Edge Width + two projected candidate reaches` with `Edge Width + one projected candidate reach`.
4. Preserve the analytical candidate field inside the one-reach edge-attached permission region and retain direct hardened-mask carving; do not restore narrow five-pixel band clipping or soft-mask reconstruction.
5. Keep Production Chip removal visually attached to a nearby eligible exterior edge. No large detached interior blobs may be admitted by edge permission.
6. Preserve Presence-Amplitude exactly as `baseMask = min(baseMask, presence)`. No amplitude compression, control, threshold, candidate, transport, source, lifecycle, Film, Shape, colour, or lighting change is allowed.
7. Add no texture, buffer, kernel, dispatch, property, serialized field, Debug View, report, scene, prefab, material, cache, file, or `.meta`.
8. Unity acceptance requires warning-free import; unchanged single yellow exterior contour; coherent magenta bites attached to nearby eligible edges; no broad detached interior removals; unchanged Current behavior.

### P12h approved file scope and implementation sequence

1. **Plan/status first:** this document, then `River_Foam_Fixed_Metric_Dependency_Register.md`, `River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`, `River_Foam_Stage6_Architecture.md`, and `River_Rendering_Roadmap.md`.
2. **Production admission:** `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl` changes only the Presence-Amplitude candidate admission depth from two projected reaches to one and corrects the ownership comments.
3. **Shader caller audit:** `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader` remains executable-byte-identical unless a documentation comment must be corrected; its property, calls, and Debug View mappings must not change.
4. **Validation:** exact-scope diff; immutable post-P12g comparison; Current-path byte/model equivalence; Presence eligibility byte/model equivalence; one-reach geometry model; direct-carve boundedness; function/call-site scan; shader brace/preprocessor/property scan; cross-subsystem shared-shader audit; protected-file/resource/kernel/property/serialized-field comparison; archive byte reproduction. Unity import and visual comparison remain authoritative and pending.

### P12h risks and non-goals

- The inward coordinate remains a local derivative approximation rather than a global signed-distance field. One projected reach is a low-cost edge-attached bound; Unity visual evidence remains authoritative.
- Large candidates may still produce deep bites when their actual projected radius is large. That is authored candidate size, not broad permission from an extra diameter.
- Current mode is a protected compatibility branch. No shared cleanup or arithmetic rewrite may leak into it.
- No Presence compression, transport/source/lifecycle tuning, overall-Foam tuning, new diagnostics, or architecture change is included.

### P12h implementation status

- [x] Complete read-only post-P12g review and record Unity evidence, code evidence, scope, invariants, implementation sequence, risks, and validation plan.
- [x] Update the remaining canonical documents.
- [x] Implement one-reach Presence-Amplitude production admission.
- [x] Complete final consistency/compliance validation and package reproduction: 46/46 primary gates and 18/18 independent gates pass; 593 protected files remain byte-identical; the eligibility and Chip-application functions are byte-identical; the only executable HLSL change removes one extra projected reach; 300,000 primary and 500,000 independent randomized admission cases pass; the changed selection function parses with the available Clang HLSL frontend; archive extraction reproduces all six files byte-for-byte.

### P12i objective and acceptance criteria

1. Preserve `Current` eligibility, edge selection, Interior Access, and soft-mask reconstruction exactly.
2. Preserve the accepted Presence-Amplitude single-contour eligibility calculation and displayed yellow mask exactly.
3. Presence-Amplitude production selection must equal `saturate(chipCandidateField * chipEligibility.edgeBand)` and therefore can never exceed the exact displayed eligibility mask.
4. Presence-Amplitude Interior Access must be disabled so no secondary permission region can remove material outside the selected eligibility area.
5. Preserve Presence-Amplitude direct hardened-mask carving, Presence amplitude, candidate formulas, transport, sources, Film, Shape, controls, resources, kernels, caches, scenes, prefabs, and materials.
6. Unity acceptance requires warning-free import and proof that every magenta Production Chip pixel lies inside the yellow eligibility area. No requirement remains to preserve a complete candidate outside that mask.

### P12i implementation status

- [x] Remove the projected-reach Presence-Amplitude candidate-admission path.
- [x] Bind Presence-Amplitude production directly to the exact candidate-times-eligibility intersection.
- [x] Disable Presence-Amplitude Interior Access while retaining the Current path unchanged.
- [x] Complete source, numerical-bound, compatibility, scope, and archive validation.

### P12j reviewed evidence

- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl::RiverWaterFoamResolveChipEligibility` currently derives Presence-Amplitude edge distance from `preChipSoftVisibility` and normalizes by `fwidth`. `preChipSoftVisibility` is produced by `RiverWaterFoamPatternedMask`, which includes material-pattern noise, lifecycle erosion thresholds, temporal morph, and strand attenuation before it reaches eligibility.
- `RiverWaterFoamEvaluateFoam` then mixes that noisy soft signal through stored/warped sampling, wake/lee lead/trail stretching, surface-break modulation, and stored-retention coupling. The supplied Unity `Chip Eligibility Composite` screenshots show the resulting screen-pixel stipple directly inside the yellow eligibility field.
- `RiverWaterFoamResolveChipEligibility` currently computes `edgeBand = visibleSupport * edgeMembership`, where `visibleSupport = smoothstep(0.08, 0.46, preChipMask)`. `RiverWaterFoamEvaluateSelectionDiagnostics` then computes Presence-Amplitude production as `chipCandidateField * edgeBand`, and `RiverWaterFoamApplyChipAndStrands` directly carves by that fractional value. Therefore faint but visible fringe receives only fractional removal authority.
- Representative support values from the exact production formula are: `preChipMask 0.10 -> 0.008`, `0.15 -> 0.089`, `0.20 -> 0.236`, `0.25 -> 0.421`, `0.46 -> 1.0`. This proves a full Chip candidate cannot fully remove the faint outer fringe under P12i.
- `RiverWaterFoamResolveStateMask` already owns a clean `baseMask` before `RiverWaterFoamPatternedMask`. In Presence-Amplitude, `baseMask = min(baseMask, presence)` and therefore provides a noise-free committed-Presence silhouette. The existing `lifeGate = smoothstep(0.015, 0.070, life)` is deterministic lifecycle ownership and can be applied without reintroducing material-pattern noise.
- The shared shader/include cross-subsystem review found the changed values are consumed only by River Foam evaluation, Chip diagnostics, and final Foam composition. Ground, Disturbance, reflection, refraction, wetness, lighting, and non-Foam render paths do not consume these functions.
- The source snapshot contains no Git metadata. Historical comparison is against the immutable reconstructed post-P12i baseline and the accepted/rejected P12f-P12i patch archives.

### P12j objective and acceptance criteria

1. Preserve the complete `Current` Chip eligibility, candidate selection, Interior Access, soft-mask reconstruction, and debug output arithmetic exactly.
2. For Presence-Amplitude only, expose a transient clean silhouette from `RiverWaterFoamResolveStateMask` using the already-resolved Presence-amplitude `baseMask` multiplied by the existing near-death life gate; do not include material-pattern noise, erosion pattern, morph, or strand terms.
3. Carry the clean silhouette through the same stored/warped/lead/trail spatial coupling, wake/lee stretch, surface-break, stored-retention, and liquid-factor ownership as the final Foam body, without adding a texture, buffer, kernel, sample, dispatch, property, control, or serialized field.
4. Build Presence-Amplitude edge distance from the clean silhouette using `length(float2(ddx(clean), ddy(clean)))`, not `fwidth(preChipSoftVisibility)`.
5. Convert Presence-Amplitude support to near-binary permission so any visibly supported eligible pixel can receive full Chip removal authority. Do not multiply removal strength by soft support intensity.
6. Preserve the exact production ownership rule: `chipProductionSelection = saturate(chipCandidateField * chipEligibility.edgeBand)`. No projected reach, inferred depth, Interior Access, or second permission field may authorize Presence-Amplitude removal.
7. Preserve Presence-Amplitude itself as `baseMask = min(baseMask, presence)`, selected spacing `0.15 m`, transport choices, source behavior, Film, Shape, colour, lighting, candidates, controls, resources, kernels, caches, scenes, prefabs, and materials.
8. Unity acceptance requires warning-free import, spatially coherent non-stippled yellow eligibility, complete removal authority across faint visible fringe inside eligibility, no magenta outside yellow, antialiased candidate contours, and unchanged Current behavior.

### P12j approved file scope and implementation sequence

1. **Plan/status first:** this document, then `River_Foam_Fixed_Metric_Dependency_Register.md`, `River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`, `River_Foam_Stage6_Architecture.md`, and `River_Rendering_Roadmap.md`.
2. **Clean silhouette production:** `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl` adds one transient scalar to `RiverWaterFoamResolveStateMask` and `RiverWaterFoamResult`, then carries it through existing render-only coupling.
3. **Mode-specific eligibility:** the same include leaves Current byte-identical and gives Presence-Amplitude clean-gradient edge geometry plus binary permission.
4. **Production caller:** `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader` passes the clean silhouette into selection diagnostics. Debug identities and final Chip application remain unchanged.
5. **Validation:** exact-scope diff; full-file reread; Current-function/call arithmetic comparison; clean-silhouette no-noise ownership scan; derivative model; binary-support/removal bounds; `production <= eligibility` proof; call-site/signature scan; shader braces/preprocessor/property/resource/kernel scan; cross-subsystem shared-shader audit; protected-file comparison; archive byte reproduction. Unity import and visual comparison remain authoritative and pending.

### P12j risks and non-goals

- The clean silhouette is a render-time scalar, not a global signed-distance field. Its derivative still depends on screen projection, but it no longer contains material-pattern or lifecycle-erosion noise.
- Binary permission intentionally allows a full candidate value to remove faint visible fringe. Candidate antialiasing remains owned by the unchanged analytical candidate field.
- No amplitude compression, candidate retuning, transport/source/lifecycle tuning, overall-Foam tuning, new Debug View, report, control, texture, buffer, kernel, dispatch, cache, scene, prefab, material, file, or `.meta` change is included.

### P12j implementation status

- [x] Complete read-only post-P12i review and record exact evidence, invariants, scope, implementation sequence, risks, and validation plan.
- [x] Update remaining canonical documents.
- [x] Implement clean silhouette transport and mode-specific binary eligibility.
- [x] Complete final consistency/compliance validation and archive reproduction: 63/63 primary gates and 25/25 independent gates pass; seven-file scope is exact; protected files remain byte-identical; Current equivalence, binary support, faint-fringe full removal, pattern-noise independence, Euclidean-gradient rotation, function parsing, resource/property, and archive-byte checks pass.

### P12k reviewed evidence

- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl::RiverWaterFoamResolveStateMask` constructs P12j `cleanSilhouette = baseMask × smoothstep(0.015, 0.070, remainingLife)` before `RiverWaterFoamPatternedMask`. Material-pattern erosion, temporal morph, coherent edge/interior thresholds, and Strand soft-shape ownership therefore do not exist in the clean silhouette.
- `RiverWaterFoamEvaluateFoam` later applies stored/warped/lead/trail coupling, wake/lee stretch, surface-break modulation, stored retention, and liquid clipping to both `mask` and `cleanSilhouette`, but the two signals remain geometrically different because only `mask` passed through patterned erosion and only final application resolves `strandKeep`.
- `RiverWaterFoamApplyChipAndStrands` computes final no-Chip geometry as `shape × RiverWaterFoamResolveStructuralStrandKeep(...)`. This is the exact mask that reaches `RiverWaterResolveFoamComposition` when production Chip is zero.
- `SH_CleanStylizedRiver.shader` currently computes selection before `RiverWaterFoamApplyChipAndStrands`, passes `foam.cleanSilhouette` into selection, displays `RiverWaterFoamResolveBaseCoverage(foam.mask)` as the grey `Chip Eligibility Composite` body, and displays `productionChipRemovedMask` recorded before Strand multiplication. These three signals do not describe the same rendered object.
- Unity P12j evidence shows a white/yellow eligibility contour offset from the grey rendered body by a nonuniform distance. This follows directly from the code ordering: patterned erosion and Strand shaping are spatially varying, so no fixed threshold or offset applied to `cleanSilhouette` can recover the final rendered edge.
- The exact visible-support threshold is `RiverWaterFoamResolveBaseCoverage(mask) = smoothstep(0.08, 0.46, mask)`. Therefore Presence-Amplitude edge distance must be derived from the `0.08` isocontour of `preChipRenderedMask`, not from `cleanSilhouette`, `softVisibility`, or `foam.mask` alone.
- The source snapshot contains no Git metadata. Historical comparison uses the immutable reconstructed post-P12j baseline and accepted/rejected P12e-P12j archives.

### P12k objective and acceptance criteria

1. Preserve `Current` mode eligibility, Interior Access, candidate selection, soft-mask reconstruction, final result, and debug arithmetic exactly.
2. For Presence-Amplitude only, resolve the exact structural Strand keep before Chip selection and construct `preChipRenderedMask = saturate(foam.mask × strandKeep)`.
3. Derive Presence-Amplitude visible support and edge distance from `preChipRenderedMask`: support through `RiverWaterFoamResolveBaseCoverage(preChipRenderedMask)`, boundary start `0.08`, and Euclidean screen-gradient normalization `length(float2(ddx(mask), ddy(mask)))`.
4. Preserve exact production ownership: `chipProductionSelection = saturate(chipCandidateField × chipEligibility.edgeBand)`. No secondary permission region, projected reach, Interior Access, or surrogate silhouette may authorize Presence-Amplitude removal.
5. Apply Presence-Amplitude removal directly to the exact pre-Chip rendered mask: `finalFoamMask = preChipRenderedMask × (1 - productionChip)` and `productionChipRemovedMask = preChipRenderedMask - finalFoamMask`.
6. Make `Chip Eligibility Composite` display coverage from `preChipRenderedMask` in Presence-Amplitude, and make `Production Chip Mask` display the exact visible removal recorded above.
7. Retire P12j `cleanSilhouette` plumbing because it has no remaining consumer; do not leave duplicate render coupling or stale contract comments.
8. Preserve Presence amplitude, TVD/Donor transport, selected spacing `0.15 m`, sources, Film, Shape, candidate formulas, controls, resources, kernels, caches, scenes, prefabs, materials, and all Debug View identities.
9. Unity acceptance requires warning-free import; grey pre-Chip body and yellow eligibility anchored to the same rendered edge; every magenta pixel inside yellow eligibility; no detached capture contour; and unchanged Current behavior.

### P12k approved file scope and implementation sequence

1. **Plan/status first:** this document, then `River_Foam_Fixed_Metric_Dependency_Register.md`, `River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`, `River_Foam_Stage6_Architecture.md`, and `River_Rendering_Roadmap.md`.
2. **Exact pre-Chip geometry:** `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl` adds a zero-resource helper that resolves structural Strand keep and the exact no-Chip rendered mask.
3. **Eligibility contract:** the same include keeps Current arithmetic unchanged and changes only Presence-Amplitude eligibility input to the exact pre-Chip rendered mask with the existing `0.08` composition boundary.
4. **Chip application:** the same include applies Presence-Amplitude production directly to the exact pre-Chip rendered mask and records exact visible removal; Current retains its accepted soft reconstruction and Strand order.
5. **Production/debug caller:** `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader` computes the Presence-Amplitude pre-Chip mask before selection, passes it to eligibility/application, and uses it in the two Chip debug views.
6. **Retire superseded plumbing:** remove P12j `cleanSilhouette` outputs, result fields, coupling, call arguments, and comments after proving no remaining consumer.
7. **Validation:** exact-scope diff; full changed-file reread; function/call-site signature scan; Current arithmetic extraction/equivalence; exact pre-Chip mask model; `production <= eligibility` and exact-removal proofs; clean-silhouette zero-reference scan; shader brace/preprocessor/property/resource/kernel scan; cross-subsystem shared-shader audit; protected-file comparison; archive byte reproduction. Unity shader import and visual evidence remain authoritative and pending.

### P12k risks and non-goals

- Edge distance remains a local screen-space derivative approximation. The critical correction is that the derivative is now taken from the exact pre-Chip rendered geometry rather than a different silhouette.
- Presence-Amplitude Chipping is intentionally constrained to the existing eligibility band. The patch does not attempt complete connected-candidate admission outside that band.
- No amplitude compression, candidate retuning, Chip-control retuning, transport/source/lifecycle tuning, overall-Foam tuning, new Debug View, report, control, texture, buffer, kernel, dispatch, cache, scene, prefab, material, file, or `.meta` change is included.

### P12k implementation status

- [x] Complete read-only post-P12j review and record evidence, invariants, scope, sequence, risks, and validation plan.
- [x] Update remaining canonical documents.
- [x] Implement exact pre-Chip rendered-mask ownership and retire superseded clean-silhouette plumbing.
- [x] Complete final consistency/compliance validation and archive reproduction.

### P12l reviewed evidence

- Unity post-P12k evidence shows `Chip Eligibility Composite` as a broad selected edge band, while `Production Chip Mask` resembles a weak copy of that band and Final shows little or no complete removal.
- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl::RiverWaterFoamSoftIrregularChip` returns antialiased candidate contours through `1 - smoothstep(radius - aa, radius + aa, distance)`. `RiverWaterFoamEvaluateSelectionDiagnostics` then multiplies those candidates by fractional readability/subpixel/lifecycle values, so `chipCandidateField` is continuous `0...1`, not a selected/not-selected region.
- `RiverWaterFoamResolveChipEligibility` also returns a continuous edge band through `1 - smoothstep(width - 0.5, width + 0.5, inwardPixels)`, so Presence-Amplitude production currently multiplies two fractional fields.
- `RiverWaterFoamEvaluateSelectionDiagnostics` currently computes Presence-Amplitude production as `saturate(chipCandidateField * chipEligibility.edgeBand)`. `RiverWaterFoamApplyChipAndStrands` then multiplies the exact pre-Chip rendered mask by `(1 - productionChip)`, so fractional production values only attenuate Foam instead of removing it completely.
- `Production Chip Mask` currently displays `productionChipRemovedMask = preChipRenderedMask - finalMask`, which expands to `preChipRenderedMask * candidate * eligibility`; it is not the exact candidate/eligibility intersection requested by the user.
- `Chip Candidate Field` and `Chip Eligibility Composite` currently display the same soft values consumed by production. The requested contract is instead a binary region intersection: a pixel is selected when the corresponding soft analytical field is at or above its mathematical contour midpoint (`0.5`), and every selected pixel is removed completely.
- The source snapshot contains no Git metadata. Historical comparison uses the immutable reconstructed post-P12k baseline and accepted/rejected P12e-P12k archives.

### P12l objective and acceptance criteria

1. Preserve `Current` mode candidate, eligibility, Interior Access, production selection, soft-mask reconstruction, final result, and all Current debug arithmetic exactly.
2. For Presence-Amplitude only, convert the analytical candidate field to an explicit binary region with `candidateSelected = chipCandidateField >= 0.5 ? 1 : 0`.
3. For Presence-Amplitude only, convert the exact pre-Chip rendered-mask eligibility field to an explicit binary region with `eligibilitySelected = chipEligibility.edgeBand >= 0.5 ? 1 : 0`.
4. Define Presence-Amplitude production exactly as `productionSelected = candidateSelected * eligibilitySelected`; no transparency, interpolation, band extension, projected reach, Interior Access, inferred region, support-intensity weighting, or additional production field is permitted.
5. Remove 100% of the exact pre-Chip rendered Foam wherever `productionSelected == 1`, and remove 0% elsewhere.
6. Make Presence-Amplitude `Chip Candidate Field`, yellow `Chip Eligibility Composite`, and `Production Chip Mask` display the exact binary candidate, binary eligibility, and binary product used by production.
7. Preserve P12k exact pre-Chip rendered-mask ownership, Presence amplitude, transport modes, selected spacing `0.15 m`, sources, Film, Shape, candidate geometry, controls, resources, kernels, caches, scenes, prefabs, materials, and Debug View identities.
8. Unity acceptance requires warning-free import; Production Chip Mask must equal the visual intersection of the binary Candidate and Eligibility views; every production-selected pixel must be fully absent from Final; Current must remain unchanged.

### P12l approved file scope and implementation sequence

1. **Plan/status first:** this document, then `River_Foam_Fixed_Metric_Dependency_Register.md`, `River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`, `River_Foam_Stage6_Architecture.md`, and `River_Rendering_Roadmap.md`.
2. **Binary selection ownership:** `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl` keeps candidate and eligibility generation unchanged, but in the Presence-Amplitude branch converts each to an explicit binary selected region and sets production to their exact product.
3. **Full removal:** the same include keeps Current application unchanged and makes Presence-Amplitude application return zero Foam for selected pixels and the exact pre-Chip rendered mask for unselected pixels. `productionChipRemovedMask` becomes the exact binary production mask used by the debug view.
4. **Diagnostic truth:** `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader` continues using existing views; the Presence-Amplitude values supplied to Candidate, Eligibility, and Production views are the exact binary masks used by production.
5. **Validation:** exact-scope diff; full changed-file reread; Current-branch extraction/equivalence; one-million-case binary truth-table and full-removal proofs; debug/production identity proof; no secondary authorization scan; shader brace/preprocessor/property/resource/kernel scan; cross-subsystem shared-shader audit; protected-file comparison; archive byte reproduction. Unity shader import and visual evidence remain authoritative and pending.

### P12l risks and non-goals

- Binary contours intentionally remove antialias gradients from the three Presence-Amplitude Chip masks. The requested behavior is exact selected/not-selected ownership and complete removal, not fractional edge smoothing.
- Candidate density, radius, spacing, amount, lifecycle, and geometry remain unchanged. If the binary candidate region covers most of the binary eligibility band, Production correctly resembles Eligibility and any later change is candidate tuning, not ownership math.
- No amplitude compression, candidate retuning, Chip-control retuning, transport/source/lifecycle tuning, overall-Foam tuning, new Debug View, report, control, texture, buffer, kernel, dispatch, cache, scene, prefab, material, file, or `.meta` change is included.

### P12l implementation status

- [x] Complete read-only post-P12k review and record exact evidence, invariants, scope, sequence, risks, and validation plan.
- [x] Update remaining canonical documents.
- [x] Implement binary Candidate × Eligibility ownership and complete Presence-Amplitude removal.
- [x] Complete final consistency/compliance validation and archive reproduction.


## RG-METRIC-P12m — Any-Support Binary Chip Selection and Full-Removal Proof

### P12m reviewed evidence

- The supplied `Assets-Code-Archive(4).zip` contains no `.git` metadata. Repository `HEAD`, status, staged state, history, and commit comparison are unavailable; preservation and compatibility comparison use an immutable pre-edit copy of the supplied files.
- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl::RiverWaterFoamSoftIrregularChip` returns `1 - smoothstep(localRadius - aa, localRadius + aa, distanceToCentre)`, so the analytical Candidate has valid positive antialias support below `0.5`.
- `RiverWaterFoamEvaluateSelectionDiagnostics` multiplies Candidate coverage by continuous readability and subpixel gates before merging candidates, so a positive value below `0.5` can also represent an intentionally fading but still supported candidate.
- `RiverWaterFoamResolveChipEligibility` returns `1 - smoothstep(widthPixels - 0.5, widthPixels + 0.5, estimatedInwardPixels)`, so Presence-Amplitude Eligibility also has valid positive antialias support below `0.5`.
- The current P12l Presence-Amplitude branch uses `chipCandidateField >= 0.5` and `chipEligibility.edgeBand >= 0.5`. These comparisons discard all positive support below their midpoint contours.
- `RiverWaterFoamApplyChipAndStrands` already returns `0.0` for every selected Presence-Amplitude production pixel and preserves `exactPreChipMask` elsewhere. The application stage is already complete removal; only selection changes.
- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader` is the only consumer of `RiverWaterFoam.hlsl`. It already passes one selection result to production, evaluated preview, Candidate, Eligibility, Production, and final-mask diagnostics. No caller edit is required.
- `StylizedRiver.cs`, `StylizedRiverEditor.Foam.cs`, `StylizedRiverFoamRuntime.Binding.cs`, and `StylizedRiverFoamRuntime.Constants.cs` confirm that Presence Footprint mode ownership and binding are existing contracts and require no change.
- P12l's Current branch is separate and remains protected. The pre-edit Current selection/application blocks and the complete protected caller files were copied outside the work tree for post-change byte comparison.

### P12m objective and acceptance criteria

1. Preserve `Current` candidate, eligibility, Interior Access, production selection, soft-mask reconstruction, final result, and Current debug arithmetic byte-for-byte.
2. For Presence-Amplitude only, select Candidate wherever `chipCandidateField > 0.0`.
3. For Presence-Amplitude only, select Eligibility wherever `chipEligibility.edgeBand > 0.0`.
4. Define production exactly as `productionSelected = candidateSelected * eligibilitySelected`; no epsilon, midpoint threshold, fractional attenuation, reach, expansion, Interior Access, support-intensity multiplier, inferred region, or secondary permission field is permitted.
5. Preserve existing full removal: every production-selected pixel returns `0.0`; every unselected pixel returns the exact pre-Chip rendered mask.
6. Make Inspector descriptions mode-specific: Presence-Amplitude views describe binary any-positive-support Candidate, Eligibility, and their exact product; Current retains continuous values and optional Interior Access.
7. Preserve P12k exact pre-Chip rendered-mask ownership, Candidate generation, Eligibility geometry, Edge Width, Strands, Presence amplitude, transport modes, selected spacing `0.15 m`, sources, Film, Shape, resources, kernels, caches, scenes, prefabs, materials, properties, serialized fields, and Debug View identities.
8. Unity acceptance requires warning-free import; Presence-Amplitude Production must equal the visual intersection of Candidate and Eligibility; every Production-selected pixel must be black in `Foam Chip And Strand Probe`; Final may remain pale only where that probe is black, proving the colour belongs to water/refraction/fog rather than Foam; Current must remain unchanged.

### P12m approved file scope and implementation sequence

Modify exactly:

1. `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
3. `Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
4. `Docs/River_Foam_Stage6_Architecture.md`
5. `Docs/River_Rendering_Roadmap.md`
6. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs`
7. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl`

Create, delete, move, rename, serialized asset, scene, prefab, material, cache, and `.meta` changes: none.

Implementation sequence:

1. **Plan/status first:** record this reviewed evidence, scope, invariants, risks, sequence, and validation here before any implementation edit.
2. **Any-support selection:** change only the two Presence-Amplitude field comparisons in `RiverWaterFoamEvaluateSelectionDiagnostics` from `>= 0.5` to `> 0.0`; update only the directly stale comments.
3. **Diagnostic truth:** change only the four relevant descriptions in `StylizedRiverEditor.DebugViews.cs`, with explicit Current versus Presence-Amplitude meanings; add no view or enum value.
4. **Canonical synchronization:** record P12l as visually rejected and P12m as the active implementation in the remaining four canonical documents.
5. **Validation:** exact-scope diff; complete modified-file reread; protected Current-branch and protected caller byte comparison; one-million-case any-support truth model plus explicit boundary cases; Candidate/Eligibility/Production diagnostic identity proof; function/call-site scan; shader delimiter/preprocessor scan; C# parser/compiler where available; namespace/import scan; property/resource/kernel/serialized-field scan; cross-subsystem shared-shader audit; changed-file archive reproduction and SHA-256. Unity import and visual comparison remain authoritative and pending.

### P12m risks and non-goals

- `> 0.0` includes the existing antialias support fringe: approximately one screen-pixel Candidate fringe and up to the existing half-pixel Eligibility transition beyond their `0.5` contours. This is intentional any-support interpretation, not new geometry.
- Candidate is multiplied by continuous readability and subpixel gates before selection. Any positive tail from those existing gates becomes full binary authority in Presence-Amplitude, so candidate pop-in or isolated distant pixels may become more visible. This consequence follows the approved absolute any-support rule. P12m will not alter those gates without separate Unity evidence and approval.
- Hard binary cuts may be pixelated. The accepted rule prioritizes zero residual Foam over fractional edge smoothing.
- No amplitude compression, candidate geometry or lifecycle retuning, Chip-control retuning, transport/source/lifecycle tuning, overall-Foam tuning, new Debug View, report, control, texture, buffer, kernel, dispatch, cache, scene, prefab, material, file, or `.meta` change is included.

### P12m implementation status

- [x] Complete read-only review and record source provenance, current formulas, direct caller/consumer, mode ownership, protected compatibility state, exact scope, risks, and validation plan.
- [x] Persist the concrete P12m plan before implementation edits.
- [x] Implement any-support Candidate and Eligibility selection.
- [x] Align mode-specific Inspector diagnostic descriptions.
- [x] Synchronize remaining canonical documents.
- [x] Complete post-implementation consistency/compliance audit, offline validation, and changed-file archive reproduction. Unity shader/C# import and visual acceptance remain explicitly pending.


### P12m post-implementation consistency and compliance result

- Exact source delta: seven approved files modified; zero files added, deleted, moved, renamed, serialized, or accompanied by `.meta` changes.
- `RiverWaterFoam.hlsl` differs from P12l only in the directly stale Presence-Amplitude comment and the two comparisons `chipCandidateField > 0.0` / `chipEligibility.edgeBand > 0.0`.
- The complete Current selection and application blocks are byte-identical to the supplied pre-edit archive. `SH_CleanStylizedRiver.shader`, `StylizedRiver.cs`, `StylizedRiverEditor.Foam.cs`, `StylizedRiverFoamRuntime.Binding.cs`, and `StylizedRiverFoamRuntime.Constants.cs` are also byte-identical.
- `StylizedRiverEditor.DebugViews.cs` differs only in the four approved mode-specific description strings; using directives, enum identities, control flow, and executable C# are unchanged.
- One million deterministic randomized Candidate/Eligibility/pre-Chip cases plus explicit `0`, smallest-positive, `1e-12`, `1e-8`, `1e-6`, `0.001`, `0.499999`, `0.5`, and `1.0` boundaries pass the exact any-support and full-removal model.
- Candidate, Eligibility, Production, removed-mask, and final-mask diagnostic call paths remain connected to the same production values. Function signatures and call counts are unchanged.
- Shader/C# delimiter scans, shader preprocessor balance, Markdown fence balance, namespace/import comparison, shared-include consumer scan, and property/resource/kernel/serialized-field counts pass. `RiverWaterFoam.hlsl` still has exactly one consumer: `SH_CleanStylizedRiver.shader`.
- Active-gameplay instruction topology, texture samples, loops, branches, passes, memory traffic, compute dispatches, and persistent resources are unchanged; P12m replaces two comparison constants only.
- A seven-file changed-source archive was extracted and reproduced byte-for-byte during validation. The final delivery archive is generated from this finalized plan state and reverified before delivery.
- No Unity 6000.5.0f1 compiler/import environment is available in this workspace. The available Clang HLSL driver cannot compile because its required `hlsl.h` resource header is absent, and no C# compiler is installed. Unity import, shader compilation, warning status, and visual acceptance are therefore pending and must not be represented as passed.

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

## Historical blocker at the P12p decision point

Unity evidence rejects both P12n Candidate Straddle and P12o Boundary-Anchored Strip. The low-frequency cache produces permission geometry unrelated to the required continuous rendered edge band and must be removed completely. The sole retained route is the original full-rate analytical Candidate Field multiplied by one rendered Eligibility band.

The remaining blocker is narrower: the visible pale exterior Foam fringe survives some Chips because Presence-Amplitude Eligibility currently derives distance from the complete hardened `preChipRenderedMask`. `RiverWaterFoamHardenSoftVisibility` constructs that mask from two rises: `hardVisible = smoothstep(0.22, 0.58, soft)` and `fringe = smoothstep(0.06, 0.34, soft) * 0.34`. Derivatives of the complete mask can therefore respond to the inner hard-body rise instead of exclusively tracking the actual exterior fringe. P12p isolates the exterior rendered-fringe coordinate and uses it as the only Presence-Amplitude edge-distance source.

No new diagnostic view, texture, buffer, kernel, dispatch, serialized control, render pass, or candidate system is authorized. GPU timing is unmeasured.

## Historical continuation rule after P12p evidence

If the isolated rendered-fringe coordinate produces one coherent exterior Eligibility band and every Production pixel is black in `Foam Chip And Strand Probe`, continue from that route. If it still fails, stop derivative tuning and reassess the Layer E mask construction itself. P13 remains deferred until Chipping is accepted.

P13 will choose the accepted quality/cell-size policy from P12 evidence, make any justified final tuning, rebuild/freeze the accepted caches, remove rejected temporary candidate guidance, and close the contiguous fixed-metric Stage 1 baseline.

## RG-METRIC-P12n — Optional Candidate-Straddle Chip Admission A/B

### P12n authorization and reviewed evidence

- The user explicitly approved implementation as a secondary route. The existing P12m Rendered Edge Band route must remain the default, selectable, and removable independently if the experiment fails.
- Unity screenshots after P12m show that the current Candidate field exists and moves, but the narrow/fragmented Eligibility intersection produces sparse Production removal that does not read as complete candidate-shaped edge bites.
- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl::RiverWaterFoamEvaluateSelectionDiagnostics` currently generates the accepted analytical candidates in the fragment shader, then Presence-Amplitude production selects only `candidateSelected * eligibilitySelected`. Candidate geometry, motion, pulse, rotation, irregularity, lifecycle, and final binary removal are not the failed subsystem and must remain unchanged.
- `RiverWaterFoamResolveChipEligibility` derives a screen-space edge estimate from local derivatives of the shaped pre-Chip rendered mask. The current route remains preserved for A/B comparison; P12n does not attempt another threshold or derivative retune.
- `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs` owns the existing committed-state presentation interval and is the appropriate dirty-time integration point for a low-frequency admission refresh. `StylizedRiverFoamRuntime.Binding.cs` owns render-material property/resource binding. `StylizedRiverFoamRuntime.Compute.cs` owns kernel resolution and dispatch telemetry. `StylizedRiverFoamRuntime.Resources.cs` owns GPU resource release.
- `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute` is the existing River Foam compute asset. P12n adds one isolated kernel and one isolated include; it does not add a compute asset or per-frame full-field rebuild.
- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader` is the sole consumer of `RiverWaterFoam.hlsl`. The shared-shader impact is therefore confined to the River forward-water pass and its existing Chip debug views.
- The supplied source has no Git metadata. Historical comparison uses the immutable extracted P12m source and byte snapshots captured before implementation.

### P12n objective and acceptance criteria

1. Add `Rendered Edge Band (Current)` and `Candidate Straddle (Experimental)` as explicit Chip Application routes. Preserve Rendered Edge Band as the serialized/source default and preserve its P12m behavior for direct A/B comparison.
2. Apply Candidate Straddle only when Presence Footprint is `Presence-Amplitude`. `Current` Presence Footprint retains its existing accepted Chip arithmetic regardless of the selected experimental route.
3. Keep the existing analytical Candidate generation unchanged in Final. The new cache only admits or rejects each deterministic candidate identity; it does not rasterize, reshape, enlarge, smooth, or move candidates.
4. Admit a candidate when a low-frequency subcell Layer E support test proves that the candidate straddles Foam: centre outside plus at least two of eight irregular-perimeter samples inside. Retain admission with binary hysteresis while at least one perimeter sample remains inside and the centre is not convincingly interior.
5. Use one point-loaded `RFloat` admission texel per candidate-cache slot. Every refresh overwrites the complete texture. Hysteresis is valid only while lattice origin and dimensions are unchanged; route disable, remap, resize, or recreation invalidates history.
6. Refresh the admission cache at an authored default of `4 Hz`, with an Inspector range of `1–8 Hz`. Inactive/dormant candidates must be rejected before support sampling. No admission dispatch occurs while the current Rendered Edge Band route is selected.
7. Keep one fallback record bound at all times so material resource binding is valid before the first experimental update. When Candidate Straddle is selected but its cache is unavailable, production falls back to the existing Rendered Edge Band route rather than silently disabling Chipping.
8. Candidate Straddle Production is the complete admitted analytical Candidate intersected by the exact pre-Chip rendered Foam at application. It deliberately removes the current derivative Eligibility band from the experimental production route; it does not alter the exact final removal target.
9. Reuse the existing Debug View identities. In Candidate Straddle, `Chip Candidate Field` remains the complete raw Candidate field; `Chip Eligibility Composite` displays admitted-candidate territory over exact pre-Chip Foam; `Production Chip Mask` displays only Foam pixels actually removed; `Foam Chip And Strand Probe` remains the authoritative final Foam mask.
10. Preserve sources, transport, selected fixed spacing `0.15 m`, Layer C storage, Film, Shape, existing Candidate controls, Current/P12m route, scenes, prefabs, materials, caches, layers, tags, and unrelated shader composition.
11. Unity acceptance requires warning-free import, direct A/B switching, visible complete candidate-shaped edge bites in the experimental route, no interior detached candidates, truthful debug views, and unchanged Rendered Edge Band and Current-Presence behavior.

### P12n approved file scope

Modify exactly:

1. `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
3. `Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
4. `Docs/River_Foam_Stage6_Architecture.md`
5. `Docs/River_Rendering_Roadmap.md`
6. `Game/Procedural/Rivers/StylizedRiver.cs`
7. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
8. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs`
9. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Binding.cs`
10. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Compute.cs`
11. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs`
12. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs`
13. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`
14. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl`
15. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader`

Create exactly:

1. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.ChipAdmission.cs`
2. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.ChipAdmission.cs.meta`
3. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.ChipAdmission.hlsl`
4. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.ChipAdmission.hlsl.meta`

Delete, move, rename, scene, prefab, material, cache, layer, and tag changes: none.

### P12n implementation sequence

1. **Plan/status first:** record this reviewed evidence, exact scope, contracts, risks, performance model, implementation sequence, and validation plan before any code or shader edit.
2. **Authoring contract:** add the route enum, default-current serialized field, `4 Hz` refresh setting, getters, and sanitization in `StylizedRiver.cs`; expose the route and route-specific controls in `StylizedRiverEditor.Foam.cs`.
3. **Admission runtime:** create `StylizedRiverFoamRuntime.ChipAdmission.cs` to own RFloat cache allocation, deterministic candidate-lattice bounds, low-frequency scheduling, compute parameter upload, fallback binding, history invalidation, and release helpers.
4. **Compute admission:** add one kernel and isolated include that reproduces the existing deterministic candidate identity/lifecycle/motion/irregular perimeter and performs centre-plus-eight-point subcell Layer E support tests against interpolated previous/current material state.
5. **Lifecycle/resource integration:** resolve the new kernel, update admission only in the experimental route, bind the cache in held and normal presentation paths, and release the cache through existing resource teardown.
6. **Render A/B route:** add hidden shader properties/texture binding, preserve Current and Rendered Edge Band branches, and add the experimental admitted-candidate branch without changing analytical candidate geometry or final pre-Chip-mask removal ownership.
7. **Diagnostic truth:** make existing Inspector descriptions route-specific and make Production display actual removed Foam support in Presence-Amplitude.
8. **Canonical synchronization:** record P12m visual rejection, P12n architecture, resource/performance scope, and pending Unity evidence in the remaining four canonical documents.
9. **Validation:** exact-scope diff; complete modified/new-file reread; protected-route extraction/equivalence; kernel/property/texture/call-site scans; C# compile/parser where available; HLSL delimiter/preprocessor scans; namespace/import scan; deterministic cache-index coverage model; lifecycle/admission/hysteresis truth model; update-cadence/dispatch proof; memory/workload calculation; shared-shader cross-subsystem audit; archive extraction byte comparison. Unity import, GPU timing, and visual A/B acceptance remain authoritative and pending.

### P12n performance model and constraints

- Candidate-cache storage is one `RFloat` texel per guarded lattice slot. For the current approximately `64.2 × 6.75 m` domain at `0.125 m` Candidate Spacing with the implemented `3` longitudinal and `6` lateral guard cells, the analytical model is approximately `520 × 67 = 34,840` slots, `139,360 bytes` or `136.1 KiB` logical payload, and `545` groups at `64` threads. Actual Unity allocation depends on the live descriptor and GPU texture alignment.
- Standard refresh is `4 Hz`, not the `12–17 Hz` material cadence. Inactive/dormant candidates exit before support sampling. Active candidates evaluate one centre support first; algebraically impossible cases exit before perimeter work, and perimeter sampling stops as soon as the required one retained or two entering contacts are found. The conservative no-early-exit ceiling for the current `3.2 s` active / `5.7 s` total lifecycle is approximately `34,840 × (3.2/5.7) × 9 × 4 ≈ 704,000` support evaluations per second. Actual work is lower and remains unmeasured.
- P12n adds one compute dispatch only when an experimental refresh is due. Switching to Rendered Edge Band or Current Presence Footprint invalidates history and stops that dispatch. It adds no render pass, draw call, full-resolution topology texture, distance transform, or per-frame full-field rebuild.
- Final Candidate-Straddle rendering adds one point texture load only for an active analytical candidate that already overlaps the current fragment inside the existing candidate search loop. Rendered Edge Band and Current-Presence branches perform no admission-texture load.
- The compute support evaluator is a camera-independent subcell approximation of the no-Chip Layer E footprint. It shares state/pattern/lifecycle/Strand equations where practical but does not reproduce screen derivatives or surface-wake deformation. This limitation is intentional for the experimental A/B route and must be judged visually.

### P12n risks and non-goals

- Low-frequency binary admission may pop when a candidate first contacts or leaves Foam. Binary hysteresis limits toggling; no opacity blending or partial removal is introduced.
- Candidate admission tests a camera-independent support approximation. Strong screen-space or surface-coupling deformation can create a mismatch between admission and exact Final support. Final removal remains clipped by the exact pre-Chip rendered mask, so mismatch can cause a no-op or imperfect attachment but cannot remove non-Foam pixels.
- Genuine internal holes and Strand openings are real boundaries and may admit candidates. The `centre outside + at least two perimeter samples inside` consensus rejects isolated single-sample noise but does not redefine visible topology.
- Candidate Straddle intentionally removes a complete admitted candidate overlap, not only a narrow Edge Width band. That is the experimental behavior under comparison; Edge Width and Interior Access remain relevant only to the preserved current route.
- No camera-visible-range culling, phased partial-buffer updates, adaptive sample count, high-resolution texture, signed-distance field, new Debug View, candidate retuning, transport/source tuning, or overall-Foam tuning is included in this first experimental implementation.

### P12n implementation status

- [x] Complete read-only review and record current candidate/render/runtime/compute ownership, direct callers/consumers, exact scope, invariants, risks, performance model, and validation plan.
- [x] Persist the concrete P12n plan before implementation edits.
- [x] Implement the optional authoring and runtime admission route.
- [x] Implement compute admission and render A/B consumption.
- [x] Align route-specific diagnostics and canonical documents.
- [x] Complete post-implementation consistency/compliance audit, offline validation, and changed-file archive reproduction.
- [ ] Unity 6000.5.0f1 import, GPU timing, and visual A/B acceptance.

### P12n plan amendment — target-3.5-compatible admission texture

Post-plan implementation review found that `SH_CleanStylizedRiver.shader` is explicitly compiled with `#pragma target 3.5`. A fragment `StructuredBuffer<uint>` would risk forcing a shader-target increase and changing platform compatibility. That is outside the approved visual experiment and is not justified.

The admission cache contract is therefore amended before the resource implementation is finalized:

- Replace the planned packed `uint` structured buffer with one point-loaded `RFloat` random-write texture containing binary `0/1` admission.
- Preserve stale-data safety without a hash by allowing hysteresis only when texture dimensions and lattice origin are unchanged. Every refresh writes every cache texel; when origin or dimensions change, history is disabled for that dispatch.
- Logical memory remains four bytes per candidate slot, equal to the planned packed record. The current approximately `27,756`-candidate domain remains approximately `108.4 KiB` plus texture allocation alignment.
- The River fragment shader retains `#pragma target 3.5`; no shader target, render pipeline requirement, or platform contract changes.
- The existing fallback becomes `Texture2D.blackTexture`; no fallback GPU buffer allocation is required.

This amendment changes only the internal cache representation. The route selection, cadence, candidate test, final analytical candidates, A/B behavior, approved project-file scope, and validation requirements remain unchanged.


### P12n final source result and post-change audit

- `StylizedRiver.cs` adds `StylizedRiverFoamChipApplicationMode`, keeps `RenderedEdgeBand = 0` as the source/serialization default, and adds a `1–8 Hz` Candidate-Straddle refresh control with `4 Hz` default/fallback. No scene or material serialization was edited.
- `StylizedRiverFoamRuntime.ChipAdmission.cs` allocates one guarded point-filtered `RFloat` random-write texture, refreshes it only while Presence-Amplitude + Candidate Straddle is requested, invalidates stale history on route disable/remap/resize, and binds the current route plus cache contract through the existing property block. Unsupported/failed allocation falls back to Rendered Edge Band.
- `CS_RiverFoam.ChipAdmission.hlsl::BuildFoamChipStraddleAdmission` reproduces deterministic candidate identity, lifecycle, rigid motion, rotation, pulse, irregular perimeter, and conservative maximum view-stabilized radius. It samples a camera-independent Layer E support approximation at the centre and up to eight irregular perimeter positions. It rejects inactive/dormant and impossible-centre cases early and stops perimeter sampling once the binary decision is established.
- `RiverWaterFoam.hlsl::RiverWaterFoamEvaluateSelectionDiagnostics` preserves the P12m Rendered Edge Band branch and accepted Current-Presence branch. Candidate Straddle only replaces Presence-Amplitude permission with admitted candidate identity; analytical candidate generation and exact final pre-Chip-mask removal remain shared.
- Candidate Straddle evaluates admitted candidates wherever the exact pre-Chip rendered mask has positive support, rather than inheriting the Rendered Edge Band route’s `0.08` BaseCoverage gate. This prevents faint exact Foam fringe from surviving solely because it lies below the old eligibility threshold; the preserved P12m and Current routes retain their existing support gates.
- Existing debug identities are retained. Candidate shows raw analytical candidates; Eligibility shows route-specific permission; Production now reports actual Foam coverage removed; `Foam Chip And Strand Probe` remains the final mask authority. Candidate-Straddle Eligibility evaluates outside current support only in that debug view so cyan territory is truthful without production cost.
- Exact approved scope is `15` modified and `4` created project files. No files were deleted, moved, or renamed. No scene, prefab, material, cache, layer, tag, shader target, render pass, or draw call changed.
- Offline checks pass for delimiter/preprocessor balance, property/kernel/call-site contracts, `36`-argument selection signature parity, unique new GUIDs, guarded lattice indexing, exhaustive admission/hysteresis Boolean equivalence, default-current ownership, and protected Current application arithmetic. Unity compilation/import and GPU/visual evidence are unavailable here and remain pending.


## RG-METRIC-P12o — Original Analytical Candidates with Boundary-Anchored Eligibility

### P12o authorization and reviewed evidence

- The user explicitly rejected P12n Candidate Straddle and authorized its replacement. The required relationship is exact and mode-visible: `original full-rate Candidate Field × selected Eligibility = Production`.
- Unity screenshot evidence shows a P12n candidate acquiring permission at an exterior edge, moving into the Foam body, cutting a round interior hole, and disappearing when cached admission changes. This behavior follows directly from P12n candidate-level authorization and is not a tuning defect.
- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl::RiverWaterFoamEvaluateSelectionDiagnostics` still evaluates the accepted analytical Candidate geometry every rendered frame. The candidate loop, hashes, movement, lifecycle, rotation, pulse, view stabilization, irregular contour, and activation are protected and must remain identical between routes.
- P12n adds candidate-level authority through `chipStraddleCandidates` and then sets experimental Production from that complete admitted candidate. That block is the rejected behavior and will be removed.
- The current P12m `Rendered Edge Band` route remains a required fallback and A/B baseline. Its derivative-based Eligibility arithmetic and Presence-Amplitude any-support selection remain protected.
- `StylizedRiverFoamRuntime.ChipAdmission.cs` and `CS_RiverFoam.ChipAdmission.hlsl` currently own the low-frequency experimental cache. They will be repurposed in place, without file/meta churn, to store and update boundary descriptors only. No low-frequency candidate field or candidate admission value will remain.
- The experimental descriptor is one record per existing deterministic candidate identity: boundary anchor in River coordinates, inward normal angle, and tracking state. Compute may reproduce candidate identity and current centre only to search for a nearby boundary; it does not produce candidate shape, movement, or Production authority.
- Boundary detection uses binary occupied/empty support samples and a local bracket plus binary search. It does not use `ddx`, `ddy`, `fwidth`, scalar-gradient normalization, a high-resolution river-wide field, or river-cell edge geometry.
- The supplied source has no Git metadata. Pre-edit comparison uses the immutable reconstructed P12n workspace and captured hashes. The protected candidate-core snapshot SHA-256 is `d43d12194a12d56f182f97c3c7dff8a1813273ab2cae95fcd1968e439b34eb13`; the protected P12m branch snapshot SHA-256 is `8b6eb2db50ebba3968bb93a3ec3dd165bfc93556a75d60fa2a07e93298a6eb0e`.

### P12o objective and acceptance criteria

1. Remove Candidate Straddle as a behavior and option. Replace enum value `1` with `Boundary-Anchored Strip (Experimental)` while preserving serialized numeric compatibility.
2. Preserve `Rendered Edge Band (Current)` as value `0`, default, fallback, and behaviorally unchanged route.
3. Preserve the original render-frame analytical Candidate Field as the sole candidate implementation. Switching routes must not change Candidate geometry, position, movement, lifecycle, pulse, rotation, size, irregularity, activation, or debug output.
4. Use the low-frequency cache only to acquire and track local Foam-boundary descriptors. A descriptor contains an anchor, inward normal, and state; it contains no candidate mask or candidate admission authority.
5. Initial acquisition searches around the current analytical candidate centre for mixed binary Foam support, estimates the inward direction from occupied/empty topology, brackets one outside-to-inside transition, and refines the boundary anchor by four binary-search steps.
6. After acquisition, tracking starts from the previous boundary anchor and normal, not from the moving candidate centre. A lost or discontinuous boundary locks the descriptor for the remainder of the current candidate lifecycle; it cannot reacquire another interior or unrelated edge until dormancy resets the state.
7. The render shader reconstructs an analytical local strip from the descriptor. Eligibility is limited to the Foam side from approximately one antialias pixel outside through authored `Chip Edge Width` inward, and is tangentially bounded to the local candidate reach. The strip does not follow the candidate between cache refreshes.
8. Experimental Production is the exact binary product of the original Candidate union and the experimental Eligibility union. No hidden admission, reach, secondary permission region, or candidate-level authorization is permitted.
9. `Chip Candidate Field` must be route-identical. `Chip Eligibility Composite` must show the current derivative band for the current route and the actual boundary-anchored strip for the experimental route. `Production Chip Mask` must equal Candidate × selected Eligibility. `Foam Chip And Strand Probe` remains the authoritative final surviving Foam.
10. The exact pre-Chip rendered mask remains the final removal target. Every selected Production pixel removes complete Foam; no partial attenuation or reconstruction after Chipping is introduced.
11. Preserve Layer C, Film, Shape, sources, transport, fixed spacing `0.15 m`, existing candidate controls, scenes, prefabs, materials, cache assets, layers, tags, render-pass count, draw-call count, and unrelated River composition.
12. Unity acceptance requires warning-free import; route-identical Candidate debug on the same paused frame; a narrow coherent experimental Eligibility strip; no travelling interior holes; exact Production intersection; black final-mask probe at every Production pixel; and unchanged current route.

### P12o approved file scope

Modify exactly:

1. `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
3. `Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
4. `Docs/River_Foam_Stage6_Architecture.md`
5. `Docs/River_Rendering_Roadmap.md`
6. `Game/Procedural/Rivers/StylizedRiver.cs`
7. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
8. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs`
9. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Binding.cs`
10. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.ChipAdmission.cs`
11. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Compute.cs`
12. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs`
13. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs`
14. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.ChipAdmission.hlsl`
15. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`
16. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl`
17. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader`

Create: none.

Delete: none.

Move/rename: none. The two P12n auxiliary source filenames and their existing `.meta` files are retained solely to avoid Unity asset churn; their implementation and symbols are replaced with boundary-eligibility ownership.

### P12o implementation sequence

1. Update this canonical plan before code.
2. Replace authoring labels and serialized refresh ownership from Candidate Straddle to Boundary-Anchored Strip while preserving enum numeric value and serialized refresh data through `FormerlySerializedAs`.
3. Repurpose the runtime cache from one scalar admission texture to one point-loaded `ARGBFloat` descriptor texture. Bind route mode, availability, origin, dimensions, and descriptor texture; disable and release it outside Presence-Amplitude experimental use.
4. Replace the compute kernel with boundary acquisition/tracking. Keep deterministic candidate identity and centre reproduction only as a search origin. Remove candidate-perimeter admission, hysteresis authority, and complete-candidate caching.
5. Replace the fragment-side admission load with descriptor decoding and analytical strip evaluation inside the existing candidate loop. Preserve the original Candidate accumulation unchanged; separately union experimental Eligibility strips; compute Presence-Amplitude experimental Production as exact binary Candidate × Eligibility.
6. Update the existing four debug descriptions and canonical documents. Do not add a debug view.
7. Run scope reconciliation, C#/HLSL contract checks, delimiter/preprocessor checks, protected candidate/current-route hash comparisons, deterministic descriptor state-machine models, package reproduction, and cross-subsystem shader-consumer audit.

### P12o performance and memory budget

- Active-gameplay fragment cost in the experimental route: one point descriptor load only for an active candidate whose analytical search cell is already being evaluated, plus descriptor decode, one dot product for inward depth, one tangent projection, and bounded strip comparisons. The original Candidate loop and Current route receive no added texture load.
- Dirty-time compute: one candidate-record dispatch at the authored refresh rate, default `4 Hz`. Inactive/dormant and locked records exit before support sampling. Valid descriptors attempt a short previous-anchor track. Unacquired records use a fixed local stencil and at most four binary-search refinements.
- Memory: one `ARGBFloat` descriptor texel per guarded candidate identity. For the P12n-reported `520 × 67` allocation, logical payload is `520 × 67 × 16 = 557,440 bytes`, approximately `544.4 KiB`. No second history texture is planned; prior descriptor state is read and overwritten in place by the same record thread.
- No high-resolution topology texture, distance transform, extra render target, render pass, draw call, persistent Layer C field, or per-frame full-field rebuild.
- GPU milliseconds remain unverified until Unity profiling. The experimental route is removable and the current route remains available if visual or measured cost is unacceptable.

### P12o risks

- The compute support evaluator is a camera-independent approximation of the no-Chip Layer E footprint; descriptor anchors may miss or lag screen-derived micro-boundaries. Unity debug comparison must decide whether the local topology is sufficiently aligned.
- One local line cannot exactly represent a sharp corner or boundary curvature larger than the candidate-local tangent extent. The strip is intentionally bounded so approximation error cannot authorize a river-wide line.
- A `4 Hz` descriptor update can visibly step if the Foam boundary itself moves quickly. This patch does not add descriptor interpolation; first establish geometric correctness and profile cost.
- `ARGBFloat` random-write support is required. Unsupported allocation falls back to Rendered Edge Band with one warning.
- In-place record read/write assumes one compute thread owns each unique texel. The dispatch/index proof is mandatory.

### P12o performance amendment — acquisition early exit

- Post-implementation cost review found that accumulating all eight perimeter samples into a topology gradient would force nine support evaluations for every living unacquired record and additional refinement work for mixed records. That is unnecessary for a local boundary bracket and is not retained.
- The approved acquisition implementation instead samples the centre, then tests the existing eight ring directions in deterministic order and stops at the first occupied/empty disagreement. That first mixed direction supplies the initial outside-to-inside bracket; the four-step boundary refinement and local four-axis normal refinement remain unchanged.
- Fully uniform inside/outside records still require at most centre plus eight ring checks, while boundary-near records can exit the ring search early. This preserves the same binary-topology requirement and removes the full eight-sample gradient accumulation.
- This amendment changes only dirty-time acquisition work inside the already approved compute include. It does not change scope, serialized defaults, candidate generation, tracking-from-anchor behavior, render eligibility, debug contracts, or the current fallback route.

### P12o descriptor-identity amendment — moving lattice origin

- Post-implementation consistency review found that the candidate lattice origin advances as the analytical Candidate field translates downstream. Treating an origin change as global history invalidation would let all living candidates reacquire unrelated boundaries roughly whenever the moving lattice crossed one Candidate Spacing, violating the lock-until-dormancy contract.
- The approved correction keeps one descriptor texture and indexes it as a circular cache by absolute candidate-cell identity modulo the current dimensions. The descriptor stores the absolute longitudinal cell coordinate as an exact float integer and packs the lateral cell coordinate, three-state ownership, and a 10-bit inward-normal angle into one exact 24-bit float integer. Compute and render both validate the decoded coordinates before using history or Eligibility.
- The descriptor layout is therefore `XY = boundary anchor`, `Z = packed lateral identity/state/normal`, `W = exact longitudinal identity`. The representable contract is longitudinal `±16,000,000` candidate cells and lateral `-2048…2047`; the runtime falls back with one warning outside that range. The contiguous current candidate range maps bijectively into the texture for unchanged dimensions, so one thread still owns each write and origin movement does not remap a candidate identity to a different cache slot. Dimension changes still invalidate/recreate history.
- This correction adds no texture, memory, dispatch, file, control, or render pass. It preserves the one-`ARGBFloat` budget while preventing origin-driven mid-lifecycle reacquisition.

### P12o current status

- Authorization: approved.
- Read-only source review: complete.
- Canonical plan: recorded before implementation.
- Implementation: source-complete inside the approved `17` modified paths; no file or `.meta` was created, deleted, moved, or renamed.
- Protected behavior: the original analytical Candidate core, `RiverWaterFoamApplyChipAndStrands`, and the Current Presence compatibility branch are byte-identical to P12m. The P12m Rendered Edge Band any-support product remains present and is still value `0`, source default, and fallback.
- Offline audit: `41/41` source/model gates pass before packaging, including 36-argument caller parity, compute/property contracts, exact descriptor packing, circular-cache continuity, boundary acquisition, thin-ribbon tracking, strict strip depth, unique writes, and sole shared-include consumer review.
- Analytical experimental cost at the previously observed `520 × 67` guarded lattice: `544.4 KiB` logical descriptor payload; approximately `704,135` support evaluations/second for the uniform-living model and a conservative all-living mixed ceiling of `1,486,507`/second at `4 Hz` before activation, locked-state, and other early exits. GPU milliseconds remain unmeasured.
- Final changed-file archive reproduction: `17/17` entries extracted and reproduced byte-for-byte. The delivery archive is rebuilt from this finalized plan state and rechecked before delivery.
- Unity 6000.5 import, visual A/B, and GPU timing: pending and must not be represented as passed.

## RG-METRIC-P12p — Retire Experimental Cache and Isolate the Rendered Exterior Fringe

### P12p authorization and reviewed evidence

- The user explicitly approved removing the P12n/P12o low-frequency route and returning to one rendered-edge-band implementation.
- Unity screenshots show P12o rectangular/local permission artifacts and no improvement to the primary surviving-fringe failure. The experimental cache is visually rejected.
- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl::RiverWaterFoamHardenSoftVisibility` constructs final hardened Foam as `max(hardVisible, fringe)`, where `hardVisible = smoothstep(0.22, 0.58, soft)` and `fringe = smoothstep(0.06, 0.34, soft) * 0.34`.
- `RiverWaterFoamResolveChipEligibility` currently derives Presence-Amplitude inward distance from derivatives of the complete hardened `preChipRenderedMask`. That source contains both the exterior fringe rise and the inner hard-body rise.
- `RiverWaterFoamApplyChipAndStrands` already removes selected Presence-Amplitude pixels completely from the exact pre-Chip rendered mask. `SH_CleanStylizedRiver.shader` passes `finalFoamMask` as the sole Foam-mask input to `RiverWaterResolveFoamComposition`; no later Foam-edge overlay exists in the supplied source.
- The immutable P12m source is the accepted pre-experiment comparison baseline. P12p restores every P12n/P12o non-document path to that baseline before applying the isolated-fringe eligibility change. The supplied source contains no Git metadata.

### P12p objective and acceptance criteria

1. Delete the P12n/P12o low-frequency candidate/boundary cache, compute include, kernel, serialized mode, refresh control, runtime allocation/binding/update/release paths, shader properties, and experimental debug wording.
2. Preserve the original full-rate analytical Candidate Field exactly. No candidate position, lifecycle, motion, pulse, rotation, irregularity, spacing, amount, search loop, or antialiasing change is permitted.
3. Preserve Current Presence Footprint arithmetic exactly.
4. Keep one Presence-Amplitude route: binary any-positive-support `Candidate × Rendered Eligibility`, followed by complete removal from the exact `preChipRenderedMask`.
5. Derive Presence-Amplitude Eligibility from an isolated rendered exterior-fringe coordinate:
   - `exactRenderedMask = saturate(preChipRenderedMask)`;
   - `outerFringeMask = min(exactRenderedMask, 0.34)`;
   - `outerEdgeCoordinate = saturate((outerFringeMask - 0.08) / (0.34 - 0.08))`;
   - estimate inward pixels from the Euclidean screen derivative of `outerEdgeCoordinate`;
   - retain the existing authored `Chip Edge Width` smooth band.
6. Use exact rendered support only where `preChipRenderedMask` exceeds the existing visible start. Once the mask reaches the `0.34` fringe ceiling, the coordinate must remain flat so the inner hard-body rise cannot create another edge.
7. Keep `Chip Candidate Field`, `Chip Eligibility Composite`, `Production Chip Mask`, and `Foam Chip And Strand Probe`; add no view.
8. Unity acceptance requires zero C# and shader/compute errors or warnings, one coherent yellow exterior band without P12o rectangles, magenta Production equal to Candidate × Eligibility, and a black final-mask probe at every Production pixel including the pale exterior fringe.

### P12p approved file scope

Modify exactly:

1. `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
3. `Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
4. `Docs/River_Foam_Stage6_Architecture.md`
5. `Docs/River_Rendering_Roadmap.md`
6. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs`
7. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
8. `Game/Procedural/Rivers/StylizedRiver.cs`
9. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Binding.cs`
10. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Compute.cs`
11. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs`
12. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs`
13. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`
14. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl`
15. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader`

Delete exactly:

1. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.ChipAdmission.cs`
2. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.ChipAdmission.cs.meta`
3. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.ChipAdmission.hlsl`
4. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.ChipAdmission.hlsl.meta`

Create: none. Move/rename: none. Scene, prefab, material, cache, layer, and tag edits are prohibited.

### P12p implementation sequence

1. Record this plan before implementation.
2. Restore all P12n/P12o implementation paths to the immutable P12m source and delete the four experiment-only files.
3. Change only the Presence-Amplitude branch of `RiverWaterFoamResolveChipEligibility` to use the isolated rendered-fringe coordinate.
4. Update the existing Inspector/debug wording and all five canonical documents to mark P12n/P12o rejected and P12p active.
5. Audit exact scope, deleted references, kernel/property/resource counts, Current and Candidate byte identity, sole shared-include consumer, delimiters/preprocessor, formula boundary cases, and archive reproduction.

### P12p performance and risk

- Runtime compute improves relative to P12o because one kernel, one low-frequency full-cache dispatch, one descriptor texture, its allocation/release/binding work, and experimental fragment descriptor loads are removed.
- Relative to P12m, the fragment route remains arithmetic-only with no new sample, loop, pass, buffer, texture, or dispatch. The Presence-Amplitude eligibility source adds a clamp and normalization around the existing derivative estimate.
- Memory returns to the P12m baseline; the approximately `544.4 KiB` logical P12o descriptor payload and its driver allocation are removed.
- Risk: clamping at `0.34` intentionally prevents eligibility from extending through the hard-body rise. The result may be shallower than desired, but it directly matches the requested narrow visible exterior band and remains controlled by `Chip Edge Width`.
- Unity shader compilation, visual acceptance, and GPU timing remain pending.

### P12p current status

- Authorization: approved.
- Read-only source review: complete.
- Canonical plan: recorded before implementation.
- Implementation: source-complete inside the approved `15` modified and `4` deleted paths; no file was created, moved, or renamed.
- Experimental retirement: all P12n/P12o runtime, compute, shader-property, serialized-control, Inspector, and fragment-descriptor references are absent. The four experiment-only source/metadata files are deleted.
- Protected behavior: `StylizedRiver.cs`, runtime binding/compute/lifecycle/resources, `CS_RiverFoam.compute`, and `SH_CleanStylizedRiver.shader` are byte-identical to P12m. The analytical Candidate evaluator, Current eligibility branch, pre-Chip mask resolver, and final full-removal function are byte-identical to P12m.
- Offline validation: `97/97` scope, retirement-reference, protected-path, delimiter, preprocessor, Markdown, sole-consumer, formula, and hard-body-flatness checks pass.
- Packaging: the changed-file archive contains the `15` replacement project files plus a deletion manifest and Windows deletion helper for the `4` retired paths. Applying the archive and manifest to the captured P12o source reproduces the final Assets tree byte-for-byte with zero mismatches; ZIP path safety passes.
- Compiler availability: no C# compiler is installed. The available Clang HLSL frontend cannot run because its required `hlsl.h` resource header is missing. Unity 6000.5 import and shader compilation remain pending and must not be represented as passed.
- Unity visual acceptance and GPU timing: pending.

## RG-METRIC-P12r — Remove Binary Topology and Restore the P12p Rendered Edge Band

### P12r authorization and reviewed evidence

- The user explicitly rejected P12q after Unity visual inspection and ordered the 4 Hz topology route deleted rather than retained or refined. The supplied screenshot shows large white, rectilinear topology regions that exclude the actual soft Foam body and therefore fail the required edge-permission contract.
- The complete current P12q.1 implementation, its direct Inspector producer, runtime lifecycle/binding/resource consumers, compute kernels, shader caller, shared Foam include, memory accounting, and all five canonical documents were reread before implementation.
- Exact current-versus-P12p comparison identifies fifteen modified existing project files and four P12q-only files. The immutable reconstructed P12p workspace is the restoration authority because it is the last implementation before Binary Morphology was introduced.
- `RiverWaterFoam.hlsl::RiverWaterFoamResolveChipEligibility` in P12p uses the rendered soft-fringe coordinate from `0.08` to `0.34`; `RiverWaterFoamEvaluateSelectionDiagnostics` uses the original full-rate analytical Candidate Field and multiplies it by that rendered Eligibility band. `RiverWaterFoamApplyChipAndStrands` performs the existing complete selected-pixel removal.
- The P12q-only path consists of `StylizedRiverFoamRuntime.ChipTopology.cs`, `CS_RiverFoam.ChipTopology.hlsl`, three compute kernels, three `R8` textures, serialized Eligibility-route/metre-width controls, lifecycle dispatches, material texture/mode binding, shader sampling, and memory accounting. No other subsystem consumes those resources.
- The supplied source has no Git metadata. The immutable P12q.1 and P12p workspaces and packaged archives are the historical comparison authorities.

### P12r objective and acceptance criteria

1. Delete the complete P12q Binary Morphology implementation, including all runtime allocation/update/release code, compute kernels/include, serialized controls, Inspector UI, material properties, texture sampling, memory accounting, documentation, and both P12q-only `.meta` files.
2. Restore P12p as the sole Presence-Amplitude Eligibility implementation: one original analytical Candidate Field multiplied by the isolated rendered exterior-fringe band.
3. Restore the P12p default, serialized layout, shader signature, shader caller, debug wording, runtime binding, lifecycle, release path, and memory accounting exactly.
4. Preserve the original Candidate evaluator, fixed spacing `0.15 m`, transport/source/lifecycle state, Film, Shape, Strands, exact pre-Chip rendered-mask ownership, and complete selected-pixel removal.
5. Do not add a replacement route, new control, threshold, texture, buffer, kernel, dispatch, cadence, layer, tag, component, scene, prefab, material, or cache change.
6. Remove active P12q instructions from all canonical documents. Retain only a concise historical rejection/removal record sufficient to prevent accidental reintroduction.
7. Unity acceptance requires zero C# and shader/compute errors or warnings, no Binary Morphology control, no topology resource allocation/dispatch, original Candidate debug behavior, and the restored P12p Eligibility/Production behavior.

### P12r approved file scope

Modify exactly:

1. `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
3. `Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
4. `Docs/River_Foam_Stage6_Architecture.md`
5. `Docs/River_Rendering_Roadmap.md`
6. `Game/Procedural/Rivers/StylizedRiver.cs`
7. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
8. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs`
9. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Binding.cs`
10. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs`
11. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs`
12. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs`
13. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`
14. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl`
15. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader`

Delete exactly:

1. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.ChipTopology.cs`
2. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.ChipTopology.cs.meta`
3. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.ChipTopology.hlsl`
4. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.ChipTopology.hlsl.meta`

Create: none. Move/rename: none. Scene, prefab, material, cache, layer, and tag edits are prohibited.

### P12r implementation sequence

1. Record this plan before any implementation edit.
2. Restore the ten existing implementation files from the immutable P12p source and delete the four P12q-only files.
3. Synchronize all five canonical documents to P12p ownership and mark P12q rejected/removed without leaving active instructions or tunable controls.
4. Reread every final modified file and all direct callers/consumers; compare implementation files against P12p byte-for-byte and verify only the canonical documentation intentionally differs.
5. Validate exact scope, deleted-file absence, zero P12q symbols/properties/kernels/includes, candidate/application byte identity, shader signature/caller parity, delimiter/preprocessor balance, compute-kernel inventory, serialized-field inventory, memory-accounting restoration, and reproducible patch extraction with explicit deletion handling.

### P12r risks and current status

- Risk: P12p remains visually imperfect; this patch is a strict rollback and cleanup, not a new Eligibility fix or tuning pass.
- Risk: ordinary ZIP extraction cannot delete the four P12q-only files. Delivery must include an explicit deletion helper and deletion manifest.
- Authorization: approved by the user's explicit delete-and-revert instruction.
- Read-only review: complete.
- Canonical plan: recorded before implementation.
- Implementation: complete inside the approved scope. Ten implementation files match the immutable P12p source byte-for-byte and the four P12q-only files are absent.
- Post-change audit: `60/60` offline gates pass: `58/58` scope, identity, symbol-removal, delimiter/preprocessor, shader-contract, kernel-inventory, serialized-field, and documentation checks plus safe-archive and byte-identical extraction/deletion reproduction.
- Compiler availability: no local C# compiler or usable Unity-compatible HLSL compiler is available. Unity 6000.5 compilation and visual validation remain pending and authoritative.


## RG-METRIC-P12s — Optional Presence-Amplitude Soft-Mask Reconstruction A/B

### P12s authorization and reviewed evidence

- The user approved testing the previously accepted soft-mask reconstruction family after P12q was deleted and P12r restored P12p.
- The complete current P12r implementation, the sole shader caller, the original analytical Candidate evaluator, the exact P12p Presence-Amplitude Eligibility/application branches, the accepted Current soft-reconstruction branch, serialized authoring, Inspector production/debug UI, runtime material binding/property IDs, and all five canonical River Foam documents were reviewed before implementation.
- `RiverWaterFoam.hlsl::RiverWaterFoamApplyChipAndStrands` already contains the accepted Current reconstruction: multiply `coherentSoftVisibility` by `(1 - productionChip)`, reharden the modified signal with `RiverWaterFoamHardenSoftVisibility`, reconstruct the hardened-mask ratio, then apply structural Strands. This is the historical behavior to reuse rather than reimplement from memory.
- `RiverWaterFoam.hlsl::RiverWaterFoamResolveChipEligibility` already contains the accepted Current soft coordinate: `preChipSoftVisibility`, start `0.06`, `fwidth` normalization, and authored `Chip Edge Width`. The P12s experiment will reuse that coordinate family for Presence-Amplitude with an authored start value while replacing the Current branch's fractional support multiplier with binary exact rendered support.
- `RiverWaterFoamEvaluateSelectionDiagnostics` currently binarizes Presence-Amplitude Candidate and Eligibility before exact final-mask deletion. P12s must retain that entire P12r route as value `0` while value `1` uses the unchanged continuous analytical Candidate field multiplied by the soft Eligibility band.
- `SH_CleanStylizedRiver.shader` is the sole production consumer of `RiverWaterFoam.hlsl`. No ground, mass, vegetation, compute, scene, prefab, material, cache, layer, or tag path consumes the proposed controls.
- The supplied workspace has no Git metadata. P12r is the immutable pre-edit baseline; the P12m archive is the historical source confirming the accepted Current reconstruction arithmetic.

### P12s objective and acceptance criteria

1. Preserve the original analytical Candidate evaluator byte-for-byte. Candidate identity, search bounds, lifecycle, movement, rotation, pulse, irregularity, shape change, readability LOD, and cadence must not change.
2. Preserve `Exact Rendered Removal (Current)` as enum value `0`, serialized default, fallback, and behaviorally identical P12r route.
3. Add `Soft-Mask Reconstruction (Experimental)` as a Presence-Amplitude-only application route. Current Presence Footprint remains on its existing accepted soft-reconstruction path regardless of this selector.
4. Add one authored `Soft Edge Start` control with range `0–0.25` and default `0.06`, matching the accepted historical Current coordinate rather than introducing an unverified constant.
5. For Presence-Amplitude Soft-Mask Reconstruction, define visible support as binary `preChipRenderedMask > 0`; low rendered amplitude must not reduce Eligibility authority.
6. For Presence-Amplitude Soft-Mask Reconstruction, compute the continuous Edge band from `preChipSoftVisibility`, authored `Soft Edge Start`, `fwidth`, and existing `Chip Edge Width`; disable Interior Access exactly as in the retained Presence-Amplitude route.
7. Production for the experimental route is continuous `originalCandidateField × softEdgeBand`. Application must reuse the accepted soft-mask reconstruction and structural-Strand order, not multiply or fade the already-final rendered mask directly.
8. Debug views must expose the exact route-specific masks: original continuous Candidate and soft Eligibility/Production in the experiment; retained binary masks in Exact Rendered Removal; exact final mask in `Foam Chip And Strand Probe`.
9. Add no texture, sampler, buffer, kernel, dispatch, render pass, draw call, loop, candidate search expansion, scene edit, prefab edit, material edit, cache change, layer, tag, component, or fixed-grid change.
10. Unity acceptance requires warning-free import, unchanged Candidate behavior, direct A/B switching, a tunable coherent soft band, visible fringe/body response regenerated from the chipped soft signal, and no regression in the retained exact route or Current Presence Footprint.

### P12s approved file scope

Modify exactly:

1. `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
3. `Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
4. `Docs/River_Foam_Stage6_Architecture.md`
5. `Docs/River_Rendering_Roadmap.md`
6. `Game/Procedural/Rivers/StylizedRiver.cs`
7. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
8. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs`
9. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Binding.cs`
10. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs`
11. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl`
12. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader`

Create: none. Delete: none. Move/rename: none. Metadata/companion changes: none.

### P12s implementation sequence

1. Record this plan before implementation.
2. Add the serialized route enum and `Soft Edge Start`, exact clamping/public ownership, Inspector controls, material property IDs/binding, and disabled defaults.
3. Extend the shared shader contract with the two scalar properties while preserving the sole caller and all unrelated argument order.
4. Add the route-specific Presence-Amplitude soft Eligibility branch; preserve the P12r exact branch and Current branch.
5. Add route-specific continuous selection and reuse the existing soft-mask reconstruction; preserve exact selected-pixel deletion for route `0` and the accepted Current arithmetic.
6. Synchronize Inspector/debug wording and all five canonical documents.
7. Audit exact scope; candidate-core identity; P12r exact-route identity; Current branch identity; shader property/binding/signature parity; enum/default/clamp consistency; delimiters/preprocessor; no resource/kernel/pass change; formula boundary cases; and byte-identical package reproduction.

### P12s performance, risks, and current status

- Runtime cost is fragment arithmetic only. The experiment adds two scalar uniforms and one uniform route branch. It reuses the existing Candidate loop, derivatives, hardening function, Strands, texture samples, render pass, and draw call.
- Soft reconstruction performs the existing baseline/modified hardening and ratio arithmetic when a production Chip is present. This cost already exists in Current Presence Footprint; P12s permits the same arithmetic under Presence-Amplitude when selected.
- Risk: the soft coordinate can still form an imperfect band because it is derivative-normalized. `Soft Edge Start` is exposed specifically for controlled Unity tuning rather than another hard-coded threshold patch.
- Risk: binary exact rendered support combined with a low `Soft Edge Start` can grant authority to extremely faint positive fringe. This is intentional for the experiment and must be judged in the Candidate, Eligibility, Production, Probe, and Final views.
- Authorization: approved.
- Read-only review: complete.
- Canonical plan: recorded before implementation.
- Implementation: source-complete inside the approved twelve-file scope; no file or metadata was created, deleted, moved, or renamed.
- Protected behavior: the complete analytical Candidate core, Current Eligibility/selection arithmetic, P12r exact Eligibility/removal arithmetic, and accepted soft-reconstruction arithmetic pass immutable-baseline identity checks.
- Offline source/model audit: `93/93` scope, ownership, binding, shader-contract, route, protected-path, resource-count, delimiter/preprocessor, and mathematical boundedness gates pass.
- Compiler availability: no local C# compiler is installed. The available Clang HLSL frontend cannot run because its required `hlsl.h` resource header is missing. Unity 6000.5 import remains authoritative and pending.
- Packaging: the twelve-file changed-source archive extracts safely over the immutable P12r baseline and reproduces the complete final workspace byte-for-byte with zero mismatches; no deletion helper is required.
- Unity visual A/B and measured GPU timing: pending.

## RG-METRIC-P12t — Soft Reconstruction Baseline and Layer D/E Inspector Reconciliation

### P12t authorization and reviewed evidence

- The user accepted P12s as the production direction, explicitly requested Soft-Mask Reconstruction become the base, requested the rejected Exact Rendered Removal route be removed, and authorized a focused Layer D/Layer E Inspector cleanup.
- The complete current P12s implementation and the twelve approved files were reviewed before editing. The supplied source contains no Git metadata; the immutable `p12s_audit` workspace is the pre-edit comparison authority.
- `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl::RiverWaterFoamApplyChipAndStrands` contains two Presence-Amplitude routes: exact selected-pixel deletion for selector value `0`, and accepted soft-mask reconstruction for selector value `1`. The accepted reconstruction multiplies the coherent pre-hardened visibility by `(1 - productionChip)`, rehardenes it through `RiverWaterFoamHardenSoftVisibility`, reconstructs the hardened-mask ratio, and applies structural Strands afterward.
- `RiverWaterFoamResolveChipEligibility` and `RiverWaterFoamEvaluateSelectionDiagnostics` contain route-specific Presence-Amplitude exact-removal and soft-reconstruction branches. The exact-removal-only branch, binary selection branch, route selector argument, and related debug wording are obsolete after the user's acceptance decision.
- `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs::DrawFoamLayerD` currently owns all Production Chipping controls even though the canonical architecture and fragment implementation place Chipping in render-only Layer E. `DrawFoamLayerE` currently contains visibility/composition and Strands only.
- `Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs` categorizes Candidate, Eligibility, and Production as `Layer D — Chip Selection`; the actual shader path evaluates them in Layer E and mutates no Layer D texture or persistent state.
- Layer D's `Visual Occupancy Build Time` and `Visual Occupancy Release Time` remain active compute inputs but affect diagnostic evaluated-shape products only; normal Final Foam samples committed Layer C directly. They must remain but be labelled diagnostic-only.
- `Chip Interior Access` is forced to zero in Presence-Amplitude by `RiverWaterFoamEvaluateSelectionDiagnostics`; the Inspector currently exposes it unconditionally. `Soft Edge Start` only affects Presence-Amplitude and should be shown only in that footprint mode.
- `SH_CleanStylizedRiver.shader` is the sole production consumer of `RiverWaterFoam.hlsl`; no ground, mass, vegetation, scene, prefab, material, cache, layer, tag, compute resource, or other subsystem consumes the route selector.

### P12t objective and acceptance criteria

1. Make Soft-Mask Reconstruction the sole Chipping application for Presence-Amplitude. Preserve its P12s arithmetic and processing order exactly.
2. Delete `StylizedRiverFoamPresenceChipApplicationMode`, its serialized field/property, material property ID/binding/default, shader property/uniform, function arguments, route branches, and route-specific Inspector/debug wording.
3. Delete the Exact Rendered Removal-only Presence-Amplitude Eligibility, binary Candidate/Eligibility selection, and direct final-mask deletion branches.
4. Preserve the original analytical Candidate evaluator, candidate search, lifecycle, movement, rotation, pulse, irregularity, readability logic, and cadence byte-for-byte.
5. Preserve Current Presence Footprint behavior and its historical soft reconstruction.
6. Move the complete Production Chipping Inspector section from Layer D into Layer E without renaming serialized controls or changing defaults.
7. Relabel Layer D as `Layer D — Evaluated Shape (Diagnostic Only)` and state explicitly that its temporal occupancy controls and previews do not affect normal Final Foam.
8. Reclassify Candidate, Eligibility, Production, Probe, and Difference debug views under one Layer E Chipping/Rendering category. Add or remove no Debug View.
9. Show `Presence-Amplitude Edge Start` only when Presence Footprint is Presence-Amplitude. Show `Chip Interior Access` only when Presence Footprint is Current.
10. Preserve every other active Foam control and shader/runtime consumer. Add no resource, texture, sampler, buffer, kernel, dispatch, pass, draw call, loop, scene edit, prefab edit, material edit, cache change, layer, tag, component, or fixed-grid change.
11. Unity acceptance requires warning-free import, unchanged accepted P12s visuals, no application selector, Chipping controls under Layer E, diagnostic-only Layer D wording, correct conditional controls, and unchanged Candidate diagnostics.

### P12t approved file scope

Modify exactly:

1. `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
3. `Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
4. `Docs/River_Foam_Stage6_Architecture.md`
5. `Docs/River_Rendering_Roadmap.md`
6. `Game/Procedural/Rivers/StylizedRiver.cs`
7. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
8. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs`
9. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Binding.cs`
10. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs`
11. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl`
12. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader`

Create: none. Delete: none. Move/rename: none. Metadata, scene, prefab, material, cache, layer, and tag edits are prohibited.

### P12t implementation sequence

1. Record this plan before implementation.
2. Remove the serialized application selector and its C#/material/shader binding contract.
3. Collapse Presence-Amplitude Eligibility, selection, and application to the accepted P12s soft-reconstruction route while preserving Candidate and Current arithmetic.
4. Move Production Chipping authoring into Layer E; relabel Layer D diagnostic-only and conditionally expose the two mode-specific controls.
5. Reclassify existing Chipping debug views into Layer E and simplify descriptions to the sole production route.
6. Synchronize the remaining four canonical documents, marking Exact Rendered Removal rejected/removed and P12s soft reconstruction promoted.
7. Audit exact scope, removed-symbol absence, Candidate and Current byte identity, accepted soft-reconstruction identity, serialized/property/signature parity, Inspector ownership, conditional UI, debug inventory, resource/kernel/pass counts, delimiters/preprocessor, canonical consistency, and byte-identical package reproduction.

### P12t performance and risks

- Runtime cost decreases slightly: one scalar uniform, one uniform route branch, exact-removal-only derivative/selection arithmetic, and direct-deletion branch are removed. No resource or dispatch changes occur.
- Inspector-only changes have no runtime cost.
- Risk: P12s remains visually imperfect. P12t freezes and cleans that accepted result; it does not alter Soft Edge Start, Edge Width, Candidate geometry, or the derivative-normalized soft Eligibility formula.
- Risk: removing the serialized selector changes the component's serialized field set. The field is removed without scene/prefab edits; Unity will ignore the obsolete serialized key on existing assets. No asset rewrite is required for this patch.
- Authorization: approved.
- Read-only review: complete.
- Canonical plan: recorded before implementation.
- Implementation: source-complete inside the approved twelve-file scope; no file or metadata was created, deleted, moved, or renamed.
- Removed route: the application enum, serialized field/property, material property ID/binding/default, shader property/uniform, route arguments, exact-removal Eligibility, binary selection, and direct final-mask deletion branches are absent.
- Preserved behavior: the original analytical Candidate evaluator, Current Presence Footprint selection, and accepted P12s soft reconstruction remain unchanged apart from deleted obsolete arguments/comments.
- Inspector reconciliation: Production Chipping is under Layer E; Layer D is diagnostic-only; Presence-Amplitude Edge Start and Current-only Interior Access are conditionally displayed; existing Chipping diagnostics are grouped under Layer E.
- Post-change audit: `64/64` offline scope, removed-symbol, protected-arithmetic, shader-arity, Inspector-contract, debug-inventory, resource-count, delimiter/preprocessor, and documentation gates pass.
- Packaging: the twelve-file archive applies safely over the captured P12s baseline and reproduces the complete final workspace byte-for-byte with zero mismatches; ZIP path safety passes.
- Unity compilation and visual validation: pending.

## RG-METRIC-P12u — Unified Automatic Birth Reveal-Speed Contract

### P12u authorization and reviewed evidence

- **Authorization:** approved by the user after the complete Formation Speed audit across Shore Ribbon, Inward Wash, Object Contact Arc, Object Contact Semi-Arc, Object Contact Fleck, Free Water Lace Connector, Cross-Lace Connector, and Torn Fragment.
- **Authoritative source:** reconstructed post-P12t workspace at `/mnt/data/work_p12u`; the supplied source contains no Git metadata, so pre-edit comparison uses the immutable `/mnt/data/baseline_p12t` copy and SHA-256 inventory.
- `StylizedRiverFoamRuntime.BirthEvents.cs::TryBeginAutomaticShoreSourceEvent` resolves `distance / speed` and clamps it to `AutomaticShoreSourceMinimumDuration..AutomaticShoreSourceMaximumDuration` (`0.85..14 s`).
- `StylizedRiverFoamRuntime.BirthEvents.cs::TryBeginAutomaticObjectSourceEvent` resolves Build duration and clamps it to `AutomaticObjectSourceMinimumDuration..AutomaticObjectSourceMaximumDuration` (`0.35..4 s`). Arc/Semi-Arc Hold, Release, and Rest are separate authored phases and must remain unchanged.
- `StylizedRiverFoamRuntime.BirthEvents.cs::TryBeginAutomaticFreeWaterSourceEvent` clamps Lace to `0.75..5 s`, Cross-Lace to `0.55..3.5 s`, and compresses Torn Fragment timing with `0.35 + distance / speed * 0.35` before a `1.35 s` ceiling.
- Shore, Object, and Free-Water creation paths apply an undocumented `0.05 m/s` effective-speed floor before duration resolution.
- `CS_RiverFoam.compute::FoamEvaluateObjectContactFleckSource` completes Fleck reveal during `smoothstep(0.0, 0.18, progress)` rather than across the full event duration.
- `StylizedRiverFoamRuntime.BirthEvents.cs::TryBeginAutomaticObjectSourceEvent` samples Contact Fleck correlated dimensions only from the upper `78–100%` of their authored ranges. `TryBeginAutomaticFreeWaterSourceEvent` samples all Free-Water correlated dimensions only from the upper `72–100%` of their authored ranges.
- `StylizedRiverFoamRuntime.Injection.cs::ResolveAutomaticSourceDepositionState` drives all nonpersistent automatic sources with normalized `elapsed / Duration`; the GPU does not independently preserve the requested metres-per-second rate.
- `StylizedRiverFoamRuntime.State.cs::AutomaticFoamSourceEvent` and the existing 32-slot `automaticFoamSourceEvents` pool provide bounded runtime ownership. Longer honest events may increase pool occupancy and rejected starts; the system must report saturation instead of silently accelerating events.
- `StylizedRiverEditor.Actions.cs` already provides one-button reports with adjacent clipboard actions. P12u will add one dedicated Play Mode reveal-speed report using the existing report state and disk-output contract.

### P12u objective and acceptance criteria

1. One shared timing resolver owns reveal timing for all eight automatic Layer C source recipes.
2. Requested reveal speed equals base authored speed × per-pattern multiplier × deterministic jitter; no family-specific duration ceiling or compressed timing formula may change it.
3. Resolved reveal duration equals `max(materialStepDuration, pathDistance / requestedSpeed)`. The material cadence is the only timing floor.
4. Arc/Semi-Arc use the shared resolver for Build only. Hold, Release, and Rest remain byte-for-byte behaviorally unchanged.
5. Contact Fleck reveal consumes normalized progress `0..1` instead of completing at `0.18`.
6. Contact Fleck and all three Free-Water recipes sample their correlated authored Min/Max ranges over the complete deterministic `0..1` interval.
7. Shore Ribbon, Inward Wash, Arc, Semi-Arc, Fleck, Lace, Cross-Lace, and Torn Fragment preserve their source geometry, dispatch bounds, deposition ownership, source amount, Remaining Life, breakup, and transport contracts except for the explicitly approved timing/range corrections.
8. Serialized field names and values remain unchanged. Inspector labels change from Formation Speed to Reveal Speed for clarity only.
9. One Play Mode report plus adjacent clipboard action prints, for every recipe, the latest observed path distance, requested speed, raw duration, resolved duration, actual speed, cadence-limited state, active count, pool occupancy, and rejected starts. Unobserved recipes must be identified explicitly rather than fabricated.
10. No new texture, buffer, kernel, dispatch, render pass, shader-rendering path, or persistent GPU resource.

### P12u approved file scope

**Modify:**

- `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
- `Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
- `Assets/Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
- `Assets/Docs/River_Foam_Stage6_Architecture.md`
- `Assets/Docs/River_Rendering_Roadmap.md`
- `Assets/Game/Procedural/Rivers/StylizedRiver.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.State.cs`
- `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
- `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`
- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`

**Create:**

- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.RevealSpeedDiagnostics.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.RevealSpeedDiagnostics.cs.meta`

**Delete / move / rename:** none.

### P12u implementation sequence

1. Add a shared resolved-timing value and resolver in `StylizedRiverFoamRuntime.BirthEvents.cs`.
2. Replace Shore, Object, and Free-Water family-specific duration formulas with that resolver; remove obsolete automatic duration-limit constants.
3. Record generic reveal timing on every automatic event and retain one latest timing sample per source type for the report.
4. Change Fleck and Free-Water correlated sampling to the full `Hash01` range.
5. Change Contact Fleck GPU reveal from the `0..0.18` window to full normalized progress.
6. Rename Inspector labels/tooltips only; serialized names remain unchanged.
7. Add the one-button Play Mode reveal-speed report and adjacent clipboard action.
8. Synchronize all five canonical documents, then run scope, syntax, contract, model, protected-behavior, and package-reproduction checks.

### P12u invariants and non-goals

- Do not tune Coverage, Activity, pattern weights, source dimensions, Presence, Remaining Life, breakup, Hold, Release, Rest, downstream transport, or Layer D/E rendering.
- Do not change the eight automatic source shapes or their dispatch/raster coordinate formulas.
- Do not modify manual/progressive birth duration contracts.
- Do not add a new source pool, resize the 32-slot automatic pool, or hide saturation.
- Do not change scene or prefab serialized values.

### P12u performance and risks

- CPU timing resolution remains one small calculation when an event starts. Report telemetry adds a fixed nine-entry CPU array and no per-frame allocations.
- GPU work per active event is unchanged. Honest slow reveals can keep more events active simultaneously, increasing existing source raster dispatches up to the unchanged 32-event bound.
- Risk: high Activity plus very slow Reveal Speed can saturate the pool and reject starts. The report must expose pool occupancy and rejected starts; P12u must not silently accelerate events to avoid saturation.
- Risk: restoring the complete Fleck/Free-Water Min/Max range changes the deterministic size distribution by design. Serialized Min/Max values remain unchanged.
- **Status:** source implementation complete; the user reports that P12u works as expected in Unity. Live report capture and performance profiling remain unmeasured; P12u is frozen and closed.

### P12u post-implementation reconciliation

- **Actually modified:** the thirteen declared existing files.
- **Actually created:** `StylizedRiverFoamRuntime.RevealSpeedDiagnostics.cs` and its `.meta`.
- **Actually deleted / moved / renamed:** none.
- **Scope discrepancy:** none.
- The shared resolver is the only automatic-source duration authority. Shore, Object, and Free-Water family ceilings/compression and the `0.05 m/s` event-start floors are absent.
- Arc/Semi-Arc Hold, Release, and Rest arithmetic is unchanged; the resolver controls Build only.
- Contact Fleck reveal now spans normalized progress `0..1`. Contact Fleck and all Free-Water correlated Min/Max sampling use the complete deterministic `Hash01` range.
- The Play Mode report covers all eight source types, active counts, current pool occupancy, last-update rejected starts, and latest requested/raw/resolved/actual timing; an adjacent Inspector action copies the report.
- Static/model audit: exact scope, C#/HLSL delimiter and preprocessor balance, resolver ownership, obsolete-formula absence, call arity, full-range endpoint reachability, one-million-case timing model, serialized-field preservation, kernel/resource-count preservation, protected `Injection.cs` ownership, and package-reproduction gates pass.
- Unity 6000.5 compilation, live report output, visual formation-speed comparison, and pool-saturation observation remain pending.

## RG-METRIC-P13A — Authoritative Birth Material and Coverage-Separated Transport

### Status

- Authorization: approved by the user after read-only audit and contract review.
- P12 closure: P12t soft-reconstruction Chipping and P12u unified Reveal Speed are frozen as accepted baselines. P13A does not reopen Candidate geometry, Chipping application, Reveal Speed, source Activity, or negative topology.
- Read-only review: complete.
- Canonical plan: recorded before implementation.
- Implementation: source changes complete.
- Offline validation: complete; 25/25 mathematical/static authority checks pass, exact scope reconciles, and package reproduction is recorded below.
- Unity 6000.5 import, Play Mode visual validation, and measured performance: pending.

### Reviewed evidence

1. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Simulation.hlsl` currently packs `R = Presence`, `G = Presence × Remaining Life`, `B = Presence × Material Pattern`, leaves alpha zero, clips Presence directly to valid-fluid coverage, merges births only through newly added Presence, and reconstructs all packed TVD channels independently.
2. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute` currently multiplies automatic Initial Presence by source shape, source-fill, family/subcell contribution, and valid-fluid coverage before encoding. It also passes Initial Presence into source-fill area selection. Manual injection uses the same mixed amount/coverage semantics.
3. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Sampling.hlsl` explicitly clears sampled alpha, proving alpha is unused by the current persistent-state contract.
4. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl` decodes the current three-channel contract, uses stored Presence as both geometric footprint and material amplitude, lets Lifecycle-Faithful still apply continuous life-driven patterned erosion, and applies Presence-Amplitude before nonlinear hardening rather than once to the resolved shape.
5. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader` is the sole production consumer of `RiverWaterFoam.hlsl`. Its Layer C debug paths decode raw channels directly and must be synchronized with the new packed state.
6. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs` places Material Transport under Runtime & Quality while Final Visibility and Presence Footprint are under Layer E. The user approved one shared `Transport & Visibility Contract` section with a live read-only explanation.
7. `Game/Procedural/Rivers/StylizedRiver.cs` serializes the three selectors independently. Enum integer values and serialized field names can remain unchanged, so scenes and prefabs require no edit.
8. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs` allocates the persistent state as existing ARGBHalf textures. P13A reuses alpha and adds no texture, buffer, kernel, dispatch, pass, or draw call.
9. The supplied source archive contains no Git metadata and no Unity `.meta` files. The immutable pre-edit workspace at `/mnt/data/river_patch_base` is the comparison authority. Scene, prefab, material, cache, layer, tag, and metadata edits are prohibited.

### Objective and authoritative contract

1. Separate geometric Coverage from intrinsic material Presence without adding storage:
   - `R = Coverage × Presence` (transported material amount),
   - `G = Coverage × Presence × Remaining Life`,
   - `B = Coverage × Presence × Material Pattern`,
   - `A = Coverage`.
2. Decode:
   - `Coverage = A`,
   - `Presence = R / A`,
   - `Remaining Life = G / R`,
   - `Material Pattern = B / R`.
3. Preserve a legacy in-memory fallback for a positive RGB state with zero alpha so an active pre-P13 state is not interpreted as empty during transient editor/runtime replacement. New writes always use the P13A contract.
4. Initial Presence and Initial Life are intrinsic birth properties. Source shape, taper, breakup, subcell width, and valid-fluid clipping may change Coverage only. They must not attenuate decoded Presence or Life.
5. Birth overlap uses Max + Refresh:
   - Coverage becomes the maximum of existing and incoming Coverage;
   - Presence becomes the maximum intrinsic Presence;
   - Remaining Life becomes the maximum intrinsic Remaining Life;
   - Pattern remains stable unless genuinely new Coverage is added.
6. Valid-fluid clipping reduces Coverage and all packed moments proportionally while preserving decoded Presence, Remaining Life, and Pattern.
7. Donor Cell transports the coherent packed state unchanged.
8. TVD Superbee reconstructs bounded Coverage only and re-encodes one coherent donor material state. It must not reconstruct four unrelated packed channels.
9. `Concentration + Lifetime` uses local Coverage concentration and the existing continuous life-pattern erosion.
10. `Lifecycle-Faithful` uses meaningful Coverage as the footprint and does not apply continuous hidden life erosion while Remaining Life is positive. Explicit lifecycle aging and negative topology still determine when the state reaches zero.
11. `Coverage-Only` (serialized enum value remains `Current`) ignores intrinsic Presence as visual amplitude after shape resolution.
12. `Presence-Amplitude` carries each resolved shape and its exact Presence-weighted counterpart through identical Presence-independent wake/warp/surface-coupling weights, then selects the Presence-weighted result. Uniform Presence `0.75` is therefore exactly proportional to the equivalent Presence `1.00` resolved mask. Presence does not feed source geometry, hardening thresholds, or coupling weights.
13. P12t analytical Candidates, soft Eligibility, soft-mask Chipping reconstruction, and Strand order remain intact. Only their input mask amplitude/decoding changes under the new explicit contract.

### Inspector requirement

Create one section immediately after Runtime & Quality:

`Foam > Transport & Visibility Contract`

It owns, without duplication:

1. `Material Transport Scheme`
2. `Final Foam Visibility Mode`
3. `Presence Footprint`

Directly below, a permanently visible read-only panel must explain:

- selected transport behaviour;
- selected final-visibility behaviour;
- selected Presence-footprint behaviour;
- the combined result;
- persistent meanings of Coverage, Presence, Remaining Life, and Material Pattern.

Inspector-facing labels become `Donor Cell`, `TVD Superbee`, `Coverage-Only`, and `Presence-Amplitude`. Enum values and serialized field names remain unchanged.

### Approved file scope

Modify exactly:

1. `Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
3. `Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
4. `Docs/River_Foam_Stage6_Architecture.md`
5. `Docs/River_Rendering_Roadmap.md`
6. `Game/Procedural/Rivers/StylizedRiver.cs`
7. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs`
8. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
9. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs`
10. `Game/Procedural/Rivers/Editor/StylizedRiverEditor.Diagnostics.cs`
11. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.State.cs`
12. `Game/Procedural/Rivers/StylizedRiverFoamRuntime.P8Diagnostics.cs`
13. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Simulation.hlsl`
14. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Sampling.hlsl`
15. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`
16. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl`
17. `Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Structs.hlsl`
18. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl`
19. `Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader`

Scope amendment before either added file was edited: the read-only consumer audit found that `CS_RiverFoam.Resources.hlsl` is the canonical inline declaration of the persistent packed-state channels and `CS_RiverFoam.Structs.hlsl` is the GPU-side declaration of the automatic source payload. Both require comment-only synchronization with the approved contract; their resource and struct declarations remain byte-for-byte structurally unchanged.
A second scope amendment was recorded before the two added diagnostic files were edited: `StylizedRiverEditor.Diagnostics.cs` exposes the R-channel conservation metric to the user and `StylizedRiverFoamRuntime.P8Diagnostics.cs` prints the same historical metric in the P8 report. P13A keeps the metric and public property names for compatibility, but their visible labels must say transported Material Amount rather than intrinsic Presence.

Create/delete/move/rename: none. Scene, prefab, material, cache, metadata, layer, tag, component, resource, kernel, dispatch, pass, and draw-call changes are prohibited.

### Implementation sequence

1. Record this plan before implementation.
2. Introduce Coverage-aware packed-state encode/decode/clamp/clip/merge helpers and coherent Coverage-only TVD reconstruction.
3. Separate automatic and manual birth Coverage from authored Presence/Life; remove Initial Presence from source-fill selection and material attenuation.
4. Preserve alpha through all state sampling/remap/transport paths and update lifecycle/diagnostic calculations to use the correct decoded quantity.
5. Update Layer E decoding and mode semantics. Carry unscaled and exact Presence-weighted resolved shapes through identical Presence-independent coupling; make Lifecycle-Faithful free of continuous hidden life erosion.
6. Synchronize raw Layer C debug decoding and descriptions.
7. Consolidate the three selectors in the new Inspector section and add the live read-only contract panel.
8. Synchronize the remaining four canonical documents and record P12 closure plus P13A architecture.
9. Run exact-scope diff, source invariant checks, mathematical model tests, delimiter/preprocessor checks, removed-misuse searches, shared-shader consumer audit, and package reproduction.

### Invariants and non-goals

- Preserve negative topology and all explicit lifecycle rates unchanged.
- Preserve P12u Reveal Speed calculations and automatic event scheduling unchanged.
- Preserve source dimensions, Activity, Coverage population controls, shape equations, taper, breakup, and event progression except that their output now owns Coverage rather than intrinsic Presence.
- Preserve fixed spacing, quality modes, resources, texture formats, CFL/substep cadence, endpoint outflow, closed faces, and transport dispatch count.
- Preserve Chipping Candidate identity/geometry/animation and the accepted soft-mask reconstruction order.
- Do not add a new debug view, control, texture, field, pass, or runtime allocation.
- Do not tune scene values or defaults.

### Performance and risks

- Persistent memory is unchanged: alpha of the existing ARGBHalf state is reused.
- Dispatch, kernel, pass, draw-call, and texture-sample counts are unchanged.
- TVD per-face arithmetic changes from four independent limiter channels to one Coverage limiter plus decode/encode arithmetic. Aggregate GPU cost is expected to be similar but is unmeasured; Unity profiling remains required before a performance claim.
- Risk: Lifecycle-Faithful material now remains structurally present until explicit lifecycle death, so overall Foam quantity may rise substantially. This is intentional and must be tuned later through explicit birth/lifecycle controls, not hidden suppression.
- Risk: a scalar Coverage field cannot reconstruct exact subcell ribbon geometry. It preserves subcell occupancy and intrinsic material values, but the selected Final Visibility policy still decides how that occupancy appears.
- Risk: Max + Refresh is an explicit single-cohort overlap approximation. It prioritizes authored source authority over exact multi-population mixing and may refresh occupied cells more strongly than the former added-Presence merge.

### Post-implementation reconciliation

- Actual scope: exactly the nineteen declared files were modified. No file was created, deleted, moved, or renamed; no scene, prefab, material, cache, metadata, resource, kernel, dispatch, pass, draw call, layer, tag, or component changed.
- Persistent packing is now `R = Coverage × Presence`, `G = R × Remaining Life`, `B = R × Material Pattern`, `A = Coverage`. Legacy positive RGB with effectively zero alpha migrates to explicit Coverage without clearing the visible material amount.
- Birth shape, taper, breakup, reveal, family/subcell shaping, and valid-fluid clipping affect Coverage only. Initial Presence and Initial Life are encoded directly as intrinsic values. Max + Refresh prevents weak dying state from rejecting a fresh source.
- Donor Cell remains first-order coherent packed transport. TVD reconstructs one bounded Coverage value and re-encodes one coherent donor material state. Unit-capacity and valid-fluid clipping now reduce Coverage coherently while preserving decoded Presence, Life, and Pattern.
- Lifecycle-Faithful no longer performs continuous render-only life erosion while Layer C Remaining Life is positive. Negative topology and every explicit lifecycle rate are unchanged.
- Presence-Amplitude now remains exactly proportional through the complete wake/warp/surface-coupling algebra because coupling weights are resolved from the unscaled Coverage/Life shape and applied identically to its Presence-weighted counterpart. The accepted P12t soft Chipping geometry remains unscaled and protected.
- The P8 remap diagnostic now validates native P13A states and migration of legacy zero-alpha states. Historical R-channel transport reports remain structurally compatible but are labelled Material Amount rather than intrinsic Presence.
- Material Presence and Remaining Life debug views now use a binary meaningful-Coverage gate and display literal decoded values without a soft Coverage brightness ramp or a minimum Life brightness floor.
- Offline validation: 25/25 model/static checks pass, including one million float64 roundtrips, one million ARGBHalf precision cases, Initial Presence `0.75` authority across 100,000 Coverage values, overlap refresh, Donor mixing, coherent over-capacity clipping, 500,000 TVD face cases, one million full-coupling Presence-linearity cases, explicit neutral/negative aging clocks, P12u byte identity, and P12t Candidate/Eligibility arithmetic protection.
- Static consistency: all changed C#/HLSL/Shader delimiters and preprocessor blocks balance; changed-function call arities match; compute kernels, resources, source-event layouts, shader properties, pass count, and target directives are unchanged.
- Changed-file package reproduction: applying the exact nineteen-file patch set over the immutable supplied base reproduces every modified project byte with no additional, missing, or deleted path.
- Unity 6000.5 compilation/import, live Play Mode visual/lifetime validation, and measured GPU/CPU performance remain pending and must not be inferred from offline validation.

## RG-METRIC-P13A.1 — D3D11 Struct-Selection Compile Hotfix

**Status:** Implementing after Unity D3D11 compile failure reported by the user.

### Objective

Restore `CS_RiverFoam` compilation on D3D11 without changing any P13A transport, material-state, lifecycle, birth, visibility, Inspector, resource, kernel, or dispatch behaviour.

### Observed evidence

Unity reports `type mismatch between conditional values` at:

`Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Simulation.hlsl(355)`

The error is repeated for every `CS_RiverFoam` kernel because all kernels compile the shared include. The failing expression selects between two `FoamMaterialState` struct values with the HLSL conditional operator:

```hlsl
FoamMaterialState donorState = faceVelocity >= 0.0
    ? negativeState
    : positiveState;
```

D3D11 rejects the struct-valued conditional even though the equivalent packed `float4` donor was already selected immediately above.

### Approved file scope

Modify exactly:

1. `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Simulation.hlsl`

Create/delete/move/rename: none inside the Unity project. Generate the hotfix ZIP, validation report, and checksum outside the project.

### Implementation

1. Record this hotfix before changing shader code.
2. Replace the struct-valued conditional with an explicit D3D11-safe branch that copies the four scalar fields from the selected decoded endpoint.
3. Preserve `negativeState` and `positiveState` for the existing TVD Coverage slope calculation; do not add another decode or alter any field calculation.
4. Confirm there are no other `FoamMaterialState` conditional expressions in the River compute source.
5. Run exact-scope diff, semantic-equivalence checks, delimiter/preprocessor checks, package reproduction, and archive safety checks.

### Invariants and non-goals

- No P13A equations or mode semantics change.
- No transport branch, limiter, Coverage reconstruction, encode/decode, capacity clamp, lifecycle, birth merge, render, or diagnostic behaviour changes.
- No kernel, resource, texture, buffer, dispatch, pass, property, serialized field, scene, prefab, material, metadata, layer, tag, or component changes.
- P12t and P12u accepted baselines remain frozen.

### Performance

The replacement performs no new arithmetic. It selects the same already-decoded endpoint and copies its four scalar fields through an explicit branch. Donor Cell still returns before the TVD path. Expected runtime cost is equivalent to the rejected struct-valued conditional after compiler lowering; Unity profiling remains pending.

### Validation status

- Unity D3D11 compilation: pending user validation.
- Offline semantic and structural validation: passed.

### Post-implementation reconciliation

- Actual project scope is exactly the two declared files. No project file was added, deleted, moved, or renamed.
- The only shader-code difference from P13A is replacement of the struct-valued conditional with one explicit branch that copies `coverage`, `presence`, `remainingLife`, and `materialPattern` from the same selected endpoint.
- The branch is mathematically identical to the rejected conditional for 500,000 randomized packed endpoint pairs; maximum decoded-field error was `0.000e+00`.
- No other `FoamMaterialState`-valued conditional remains in the River compute source.
- The complete P13A mathematical/semantic suite remains `25/25 PASS`.
- Hotfix-specific scope, semantic, delimiter, preprocessor, and unrelated-source checks are `7/7 PASS`.
- Applying the two-entry hotfix ZIP over the immutable P13A tree reproduces the completed P13A.1 tree byte-for-byte with zero unsafe, added, deleted, or differing paths.
- Compute kernels, resources, dispatches, mode calculations, persistent packing, lifecycle, birth, render, diagnostics, and accepted P12t/P12u source remain unchanged.
- Unity 6000.5 D3D11 compilation is pending and is the required next validation action.



## RG-METRIC-P13B — Packet-Rearmed Birth and Object Contact Retention

**Status:** Source implemented; offline validation passed; Unity validation pending.

### Objective

Prevent automatic Foam sources from building giant persistent Layer C reservoirs through repeated emission while preserving finite thin source packets and long-lived object-contact Foam. Reuse current resources and reduce repeated work rather than adding a filament field, texture, buffer, kernel, pass, or draw call.

### Read-only reviewed evidence

- The supplied source archive and P13A/P13A.1 patch archives contain no `.git` metadata. `HEAD`, status, staged state, and history are unavailable. The immutable post-P13A.1 copy at patch start is the comparison authority.
- `StylizedRiverFoamRuntime.BirthEvents.cs::AutomaticShoreSourceProfile.SlotSpacingMetres` and `AutomaticFreeWaterSourceProfile.SlotSpacingMetres` interpolate slot spacing from Coverage, while `TryStartAutomaticShoreSourceEvent` and `TryStartAutomaticFreeWaterSourceEvent` also reject each deterministic slot through a Coverage probability. Coverage therefore changes both candidate density and participation.
- `AutomaticShoreSourceProfile.EventsPerSecond`, `AutomaticObjectSourceProfile.EventsPerSecond`, and `AutomaticFreeWaterSourceProfile.EventsPerSecond` interpolate from nonzero family minimum rates once Activity is positive. Activity is therefore not a linear zero-to-maximum rate.
- Shore and Free-Water sources have deterministic slot identities but no per-slot active/rearm record. `TryStartAutomaticShoreSourceEvent` and `TryStartAutomaticFreeWaterSourceEvent` may select the same logical slot again as soon as their global rate accumulator fires.
- Nonpersistent automatic sources already use current-minus-previous deposition permission in `CS_RiverFoam.compute::EvaluateFoamAutomaticSourceRasterSample`. Shore Ribbon, Inward Wash, Lace, Cross-Lace, and Torn Fragment therefore do not need a new deposition architecture. Contact Fleck is the exception because `FoamEvaluateObjectContactFleckSource` multiplies the whole completed shape by one global reveal amplitude, so every tick increases the complete footprint and passes current-minus-previous permission.
- `StylizedRiverFoamRuntime.Injection.cs::IsPersistentAutomaticSourceEmitter` classifies Object Arc and Semi-Arc as persistent. `DispatchAutomaticFoamSourceEvents` dispatches them every material tick, while `FoamResolveObjectRibbonPhaseMask` returns the complete front and wake geometry throughout Hold. Wake arms are therefore replenished for the full Hold interval.
- `CS_RiverFoam.compute::FoamEvaluateObjectContactArcSource` and `FoamEvaluateObjectContactSemiArcSource` already compute front-profile and downstream wake-arm shapes separately before combining them. The immediate contact profile can remain refreshable while wake arms use one-shot Build deposition without a new event lane or resource.
- `StylizedRiverFoamRuntime.Obstacles.cs::StampObstacleRoutingComponent` writes one RGHalf routing texture. Its current G channel is a one-sided upstream collision influence and `ResolveFixedMetricObstacleRoutingPolicy` has zero downstream release cells. Side/rear contact Foam therefore receives no retention slowdown.
- `RiverWaterFoamVelocity.hlsl::RiverWaterResolveFoamVelocityContract` currently treats R as direction and G as the shared routing/slowdown influence. The same RG texture can instead encode R as signed routing influence and G as independent slowdown influence. This permits a narrow all-around contact slowdown halo without lateral redirection and without a new texture sample.
- Automatic-source Breakup Strength is exposed for Shore Ribbon, Inward Wash, Contact Fleck, Lace, Cross-Lace, and Torn Fragment. `CS_RiverFoam.compute::FoamSourceEventBreakup` is then mixed with different hidden strengths, while Lace/Cross-Lace also apply separate gap masks and Torn Fragment applies a separate bite mask. The controls do not own one predictable operation. Arc/Semi-Arc breakup fields are serialized legacy state and already absent from the active Inspector.
- P13A Coverage/Presence/Life packing, P12u Reveal Speed, P12t Chipping, negative topology, material transport, Final Visibility modes, and Presence Footprint modes are direct consumers or adjacent contracts but do not require semantic changes in P13B.

### Acceptance criteria

1. Coverage selects a stable fraction of a fixed deterministic slot population. It does not also change slot spacing.
2. Activity is linear from zero to each family maximum start rate. Activity zero emits nothing; Activity one attempts at the documented maximum rate.
3. Shore and Free-Water logical slots cannot start another event until the prior event duration plus a distance-derived packet-clearance interval has elapsed. The clearance interval uses authored Minimum Packet Gap and current Foam downstream speed.
4. Contact Flecks are rearmed per object, cannot overlap another active Fleck from the same object, and use a spatial reveal sweep so only newly revealed geometry is deposited.
5. Object Arc/Semi-Arc front and wake geometry are deposited once during Build. During Hold, only the immediate object-contact front profile is refreshed. Release progressively withdraws that front-only refresh and never refreshes wake arms. Existing deposited material remains governed by transport, support, and lifecycle.
6. The current obstacle-routing RGHalf texture becomes `R = signed routing influence`, `G = slowdown influence`. Existing upstream routing remains one-sided. A narrow contact slowdown halo covers front, shoulders, sides, and rear without adding lateral routing.
7. When slowdown is enabled, full contact influence reaches the exact authored Minimum Downstream Factor. The former Slowdown Strength field becomes an Inspector-facing Slowdown Falloff/Reach authority; zero disables slowdown and higher values broaden the influence response.
8. Remove every visible automatic-source Breakup Strength Min/Max control and remove generic breakup, Lace/Cross-Lace gap masks, and Torn Fragment bite masks from automatic-source geometry. Preserve source dimensions, curvature, end taper, deterministic width variation, and explicit recipe silhouettes.
9. Do not change P13A persistent packing/merge/transport, Final Visibility modes, Presence Footprint modes, P12u timing, P12t Chipping, negative topology, source Initial Presence/Life, or scene tuning values.
10. Runtime resource, kernel, dispatch-structure, texture-sample, pass, and draw-call counts must not increase. Accepted-event and repeated-raster work should decrease.

### Approved project file scope

Modify exactly:

1. `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
3. `Assets/Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
4. `Assets/Docs/River_Foam_Stage6_Architecture.md`
5. `Assets/Docs/River_Rendering_Roadmap.md`
6. `Assets/Game/Procedural/Rivers/StylizedRiver.cs`
7. `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
8. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs`
9. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs`
10. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.State.cs`
11. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs`
12. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs`
13. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Obstacles.cs`
14. `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`
15. `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Motion.hlsl`
16. `Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoamVelocity.hlsl`
17. `Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader`

Create/delete/move/rename: none inside the Unity project. Patch ZIP, validation report, and checksum are generated outside the project.

### Implementation sequence

1. Record this complete plan before source edits.
2. Add authoritative Shore, Object Fleck, and Free-Water Minimum Packet Gap controls; move Activity/Coverage tooltips to their corrected contracts; relabel obstacle slowdown ownership; remove visible and serialized automatic-source Breakup Strength controls.
3. Make Shore/Free-Water deterministic slot spacing fixed and Coverage participation stable; make all Activity rates linear from zero; add bounded per-slot/per-object rearm state and clear it with automatic-source state.
4. Convert Contact Fleck reveal to a spatial accumulated sweep and add per-object active/rearm checks.
5. Split Object Arc/Semi-Arc current/previous front and wake contributions in the rasterizer: one-shot Build deposition for all geometry, Hold refresh for contact front only, and progressive Release withdrawal for that front only. Wake arms are never refreshed after Build.
6. Remove automatic-source generic breakup and hidden gap/bite masks while preserving explicit width noise, end taper, curvature, and silhouettes.
7. Repack obstacle routing writes as signed routing plus independent slowdown; stamp the all-around contact slowdown halo at obstacle dirty-time; update compute and render velocity decoding without adding calls or samples.
8. Synchronize the remaining four canonical documents.
9. Run exact-scope reconciliation, all-reference scans, HLSL/C# delimiter and preprocessor checks, event ABI/kernel/resource/pass/property invariants, deterministic rearm and deposition model tests, obstacle-channel model tests, D3D11 conditional scan, and package reproduction.

### Invariants and non-goals

- No filament field or procedural ribbon renderer.
- No new persistent texture, buffer, event vector, compute kernel, full-field pass, shader sample, material pass, or draw call.
- Preserve the 32-event pool and existing source-event GPU byte layout.
- Preserve fixed spacing `0.15 m`, material cadence, transport substeps, state formats, and cache ownership.
- Preserve P13A Coverage/Presence/Life authority and overlap policy unchanged.
- Preserve negative topology, support topology, lifecycle rates, and object-contact positive support unchanged.
- Preserve source Reveal Speed and geometric min/max dimensions except removal of breakup/gap/bite shaping.
- Do not edit scenes, prefabs, materials, metadata, layers, tags, or components.
- Do not tune Final Visibility or Presence Footprint in this patch.

### Performance and risks

- Per-slot rearm state is CPU-only and bounded by the deterministic Shore/Free-Water slot counts plus registered object count. It allocates no per-frame collection.
- Source start arithmetic adds dictionary lookups only on bounded event-attempt scans. Fewer accepted overlapping events and removal of hidden source noise are expected to reduce aggregate source-raster work; this is unmeasured until Unity profiling.
- Object Arc/Semi-Arc still use the existing event dispatch range during Hold and progressive front-only Release, but only the contact profile writes material. Wake-arm refresh work is removed. A later dispatch-range optimization is possible but outside this patch.
- Contact slowdown stamping runs only when the obstacle-routing texture is rebuilt. It reuses existing CPU scratch/state, texture memory, upload, shader sample, and velocity call count.
- Risk: removing breakup/gap masks makes individual source strokes more continuous. Packet spacing, finite lengths, curvature, explicit widths, end taper, Chipping, and Strands remain available to prevent featureless shapes.
- Risk: very low Foam downstream speed or Minimum Downstream Factor can produce long rearm intervals. This is intentional: a source must not stack packets that have not cleared.
- Risk: object Hold refresh can still build a dense immediate contact band. Its width remains the fixed contact-profile ribbon and cannot refresh downstream arms.

### Validation status

- Read-only review and exact scope declaration: complete.
- Plan recorded before implementation edits: complete.
- Implementation: complete in the declared source scope.
- Offline source/model validation: complete; exact results recorded in the P13B implementation record below.
- Unity 6000.5 C# compilation, D3D11 shader import, Play Mode source-population review, and profiler evidence: pending user validation.


### P13B implementation record

Implemented source contract:

1. Shore and Free-Water use fixed deterministic slot spacing. Coverage is a stable slot-participation threshold and no longer changes slot density. Activity is linear from zero to the existing family maximum attempt rate.
2. Every accepted Shore and Free-Water slot records a next-start time equal to event duration plus `Minimum Packet Gap / resolved downstream speed`. Contact Flecks use equivalent per-object state, reject concurrent Flecks from the same object, and also wait while that object's Arc/Semi-Arc cycle is active.
3. Finite event rasterization uses current-minus-previous only as a per-cell reveal permission. A newly reached cell receives the complete current geometric Coverage target, avoiding cadence-dependent derivative Coverage while preventing writes behind the reveal head.
4. Contact Fleck reveal is spatial along its tangent instead of globally increasing the complete Fleck mask.
5. Arc/Semi-Arc Build deposits contact and finite wake geometry once. Hold refreshes only the immediate contact front. Release progressively withdraws only that front refresh. Wake arms are not repainted after Build.
6. Object Flecks are removed from the normalized Arc/Semi-Arc mix. Mixed mode enables Flecks directly through Fleck Coverage, Fleck Activity, and Minimum Fleck Packet Gap; Arc/Semi-Arc weights normalize only those two cycle recipes.
7. The existing obstacle-routing RGHalf texture now means `R = signed lateral-routing influence`, `G = independent slowdown influence`. Existing one-sided upstream routing is preserved. A dirty-time `0.45 m` all-side contact halo reaches full slowdown inside `0.10 m` without another resource, upload, sample, or dispatch.
8. The serialized slowdown-strength scalar is Inspector-labelled `Obstacle Slowdown Falloff`. Zero disables slowdown. Any positive setting reaches the exact authored Minimum Downstream Factor at full contact; the setting changes how quickly influence approaches that minimum.
9. Generic automatic-source Breakup Strength controls and evaluation are removed, together with Lace/Cross-Lace gap masks and Torn Fragment bite masks. The reserved source-event ABI lanes remain zeroed/ignored so the GPU record layout and P7 diagnostics remain compatible.
10. Confirmed unused Arc/Semi-Arc arm-reach and Semi-Arc lopsidedness serialized/accessor/sanitizer controls are removed. Existing scene YAML may retain unknown historical keys; no scene or prefab is edited.

Preserved contracts:

- P13A Coverage/Presence/Life packing, birth merge, Donor/TVD transport, Final Visibility and Presence Footprint;
- P12u reveal-speed resolver and source-event pool capacity;
- P12t Candidate/Eligibility/soft-reconstruction Chipping;
- topology support, negative aging, lifecycle rates, fixed metric `0.15 m`, kernels, buffers, textures, passes, and draw calls.

Performance disposition:

- Active-gameplay source writes are expected to decrease because finite sources stop writing behind their reveal head, same-slot/object packet rearming prevents rapid restarts, and Object wake arms are no longer refreshed during Hold/Release.
- Added cost is bounded dictionary lookup/arithmetic during source-attempt scans and dirty-time CPU contact-halo stamping. There is no per-frame allocation, GPU resource, shader sample, kernel, pass, or draw-call increase.
- Velocity evaluation replaces one shared influence with signed routing plus slowdown. Falloff uses a bounded quartic-to-linear blend with scalar multiplies and one lerp; no transcendental operation is added. Measured CPU/GPU impact remains pending Unity profiling.

Offline validation proves source-level invariants and package reproduction only. Unity compilation, D3D11 import, Play Mode visual acceptance, and performance remain pending.

### Post-implementation consistency and compliance reconciliation

- Final project diff is exactly the 17 approved files. Added/deleted/moved/renamed project files: none.
- The complete source/model suite is `28/28 PASS`: exact scope, C#/HLSL/shader lexical and preprocessor balance, serialized-property/property-reference resolution, removed-control scans, packet-rearm and deposition ownership, Fleck spatial reveal, Object Build/Hold/Release ownership, obstacle RG separation, exact-minimum slowdown mathematics, event ABI, kernel/property/pass invariants, P13A/P12t byte identity, P12u resolver byte identity, and D3D11 struct-conditional regression scan.
- Randomized model evidence includes 100,000 packet-clearance cases, 1,001 × 1,001 Fleck reveal samples, 10,000 one-shot reveal cells, 100,000 slowdown cases, and 100,000 signed-routing/slowdown combinations.
- Applying the 17-entry patch archive over the immutable post-P13A.1 baseline reproduces the completed P13B tree byte-for-byte with zero unsafe, added, deleted, or differing paths.
- Final reread confirms no new texture, buffer, event vector, kernel, full-field pass, shader sample, material pass, draw call, layer, tag, component, scene, prefab, material, or metadata edit.
- Unity 6000.5 C# compilation, D3D11 shader import, Play Mode source spacing/contact retention review, and CPU/GPU profiling remain pending and are the required next validation actions.

## RG-METRIC-P13B.1 — Reveal-Speed Diagnostic Object-Cycle Compile Hotfix

**Status:** Implemented; offline validation passed; Unity compilation pending.

### Objective

Restore Unity C# compilation after P13B removed `IsPersistentAutomaticSourceEmitter` from `StylizedRiverFoamRuntime.Injection.cs` while `StylizedRiverFoamRuntime.RevealSpeedDiagnostics.cs` retained two stale calls. Preserve all P13B runtime, spawning, retention, timing, transport, rendering, and diagnostic calculations.

### Read-only reviewed evidence

- Unity reports `CS0103` at `StylizedRiverFoamRuntime.RevealSpeedDiagnostics.cs:121` and `:138`: `IsPersistentAutomaticSourceEmitter` does not exist in the current context.
- Source-wide search finds exactly those two executable references. The only other occurrence is historical P13B planning text.
- `StylizedRiverFoamRuntime.Injection.cs::IsAutomaticObjectContactCycle` is the current shared classifier for `ObjectContactArc` and `ObjectContactSemiArc`.
- The diagnostic uses the missing helper only to select `ObjectBuildDuration` as reveal duration and to print Hold/Release/Rest timing for Arc/Semi-Arc events. `IsAutomaticObjectContactCycle` has the exact required event set.

### Approved project file scope

Modify exactly:

1. `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.RevealSpeedDiagnostics.cs`

Create/delete/move/rename: none inside the Unity project. Patch ZIP, validation report, and checksum are generated outside the project.

### Implementation sequence

1. Record this hotfix before changing C# source.
2. Replace both stale `IsPersistentAutomaticSourceEmitter` calls with `IsAutomaticObjectContactCycle`.
3. Confirm no executable reference to the removed helper remains.
4. Confirm the current classifier still resolves exactly Arc and Semi-Arc.
5. Run exact-scope, replacement-count, delimiter/preprocessor, source-reference, and package-reproduction checks.

### Invariants and non-goals

- No event timing, event type, source scheduling, packet rearm, object retention, transport, lifecycle, rendering, Chipping, Inspector, serialization, resource, kernel, pass, scene, prefab, material, layer, tag, or component change.
- P13A/P13A.1 and P13B runtime behaviour remains unchanged.
- No new helper is introduced; the current authoritative object-cycle classifier is reused.

### Performance

No runtime cost change. The diagnostic is editor-only, and the replacement calls the existing constant-time event-type classifier.

### Validation status

- Offline exact-scope and semantic validation: passed.
- Unity 6000.5 C# compilation: pending user validation.

### Post-implementation reconciliation

- Actual project diff is exactly the two declared files.
- Both stale calls were replaced with `IsAutomaticObjectContactCycle`; no executable `IsPersistentAutomaticSourceEmitter` reference remains.
- `IsAutomaticObjectContactCycle` still classifies exactly `ObjectContactArc` and `ObjectContactSemiArc`, preserving reveal-duration and Hold/Release/Rest diagnostic behaviour.
- No runtime source file was modified. P13B spawning and object-retention calculations remain byte-identical.
- Unity compilation is pending and is the required next validation action.


## RG-METRIC-P13C — One-Shot Object Packets and Full-Vector Contact Retention

**Status:** Source implementation complete; `35/35` offline validation gates pass; package reproduction complete; Unity validation pending.

### Objective

Remove the remaining persistent Object Arc/Semi-Arc emitters and make object-contact slowdown reduce the complete routed Foam velocity vector. Arc, Semi-Arc, and Fleck become finite one-shot packets behind one shared per-object rearm gate. Preserve P13A material authority, P12u reveal-speed progression, P12t Chipping, Shore/Free-Water source ownership, topology aging, and all existing GPU resources.

### Read-only reviewed evidence

- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs::DispatchAutomaticFoamSourceEvents` treats Object Arc/Semi-Arc Hold as unconditional new deposition and continues dispatching until `Build + Hold + Release` completes.
- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute::FoamEvaluateObjectContactArcSource`, `FoamEvaluateObjectContactSemiArcSource`, and the `refreshObjectContact` branch retain/rewrite the immediate contact front through Hold/Release.
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs::CompleteAutomaticObjectContactCycle` rearms Arc/Semi-Arc from `ObjectRestDuration`, while Flecks use a separate `automaticObjectFleckNextStartTimes` dictionary. The two source classes therefore do not share one clearance authority.
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Obstacles.cs::StampObstacleContactSlowdownComponent` already writes an all-side slowdown halo into the existing obstacle-routing texture, but its `0.10 m` full reach and `0.45 m` outer reach are hidden constants and are absent from the routing-field signature.
- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoamVelocity.hlsl::RiverWaterResolveFoamVelocityContract` applies slowdown only to downstream velocity. Lateral velocity remains unscaled, so full contact influence does not reduce total speed to the authored minimum factor.
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.RevealSpeedDiagnostics.cs` and `StylizedRiverFoamRuntime.P7Diagnostics.cs` still assume Arc/Semi-Arc Build/Hold/Release/Rest state and must be reconciled when those phases are removed.
- The source archive has no `.git` metadata. The immutable post-P13B.1 tree at `/mnt/data/p13c_base` is the comparison authority.

### Approved project file scope

Modify exactly:

1. `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
3. `Assets/Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
4. `Assets/Docs/River_Foam_Stage6_Architecture.md`
5. `Assets/Docs/River_Rendering_Roadmap.md`
6. `Assets/Game/Procedural/Rivers/StylizedRiver.cs`
7. `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
8. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs`
9. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs`
10. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.State.cs`
11. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs`
12. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Obstacles.cs`
13. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.RevealSpeedDiagnostics.cs`
14. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.P7Diagnostics.cs`
15. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs`
16. `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs`
17. `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`
18. `Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoamVelocity.hlsl`

Create/delete/move/rename: none inside the Unity project. Patch archive, validation report, and checksum are generated outside the project.

### Implementation sequence

1. Replace serialized Object Hold, Release, Rest, and Fleck-specific packet-gap controls with one `Object Contact Minimum Packet Gap (m)` control. Add authored `Object Contact Full Slowdown Reach (m)` and `Object Contact Slowdown Outer Reach (m)` controls; preserve existing serialized slowdown falloff/minimum-factor backing names.
2. Replace `AutomaticObjectContactCycleState` plus `automaticObjectFleckNextStartTimes` with one per-object source state containing cycle index, next eligible time, and last successful object event type. Arc/Semi-Arc/Fleck share this state and one clearance gate. A completed Fleck must not immediately chain into another Fleck when contact cycles are enabled; the next eligible object event is a cycle. Flecks remain supplemental rather than starving Arc/Semi-Arc at high Activity.
3. Make Arc/Semi-Arc event duration equal Build duration only. Remove Hold/Release/Rest from event state, creation, phase progression, completion, and diagnostics. Arc/Semi-Arc use the ordinary current-minus-previous one-shot deposition path.
4. Schedule every completed Arc/Semi-Arc/Fleck through one clearance resolver: slowdown-halo clearance at the authored minimum full-vector speed plus the authored downstream packet gap at base Foam speed. Slowdown disabled resolves factor `1`. Positive slowdown with minimum factor `0` yields no automatic rearm.
5. Reduce object source evaluation to Build-only geometry and remove the Hold/Release refresh bypass. Preserve Arc/Semi-Arc build order, dimensions, contact profile, and one-shot wake geometry.
6. Stamp the authored contact-halo reaches into the existing RGHalf obstacle field and include both reaches in the dirty-time routing signature.
7. Apply the resolved contact speed factor to both downstream and lateral velocity components. Preserve routing direction and the exact minimum-factor contract.
8. Reconcile Inspector help, public diagnostic properties, the Object source status row, P7 diagnostics, reveal-speed diagnostics, and all five canonical documents. The post-implementation audit found `StylizedRiverFoamRuntime.PublicSurface.cs` and `StylizedRiverEditor.DebugViews.cs` still exposed obsolete Hold/Release/Rest counters; scope was amended before either file was edited. Run exact-scope, stale-reference, serialized-property, event-state, source-deposition, rearm, velocity, resource/kernel/pass, delimiter/preprocessor, and package-reproduction checks.

### Invariants and non-goals

- No Arc/Semi-Arc source rasterization occurs after Build completes.
- Arc/Semi-Arc/Fleck cannot bypass the shared object clearance gate or run concurrently for the same object.
- Flecks remain supplemental: when both Flecks and contact cycles are enabled, a successful Fleck is followed by a contact-cycle opportunity before another Fleck from that object.
- Full contact slowdown scales both velocity components by the exact authored minimum speed factor; outside the halo velocity is unchanged.
- Preserve P13A Coverage/Presence/Life/Pattern packing, merge, Donor/TVD transport, Final Visibility, and Presence Footprint unchanged.
- Preserve P12u reveal-speed resolver, source geometry and dimensions, fixed `0.15 m` metric, topology support/negative aging, P12t Chipping/Strands, source Initial Presence/Life, and the 32-event pool.
- Do not edit Shore or Free-Water spawning, scenes, prefabs, materials, metadata, layers, tags, components, textures, buffers, kernels, passes, draw calls, or shader sample count.
- Do not add debug views.

### Performance and risks

- Removing Hold/Release dispatches and shortening Arc/Semi-Arc event occupancy reduces active-gameplay source-raster work.
- Shared object state replaces one dictionary and does not allocate per frame.
- Contact reach changes rebuild only the existing dirty-time obstacle-routing texture. Two scalar settings are added to its signature; no new field or upload exists.
- Full-vector slowdown adds one scalar multiply to the lateral component and reuses the existing slowdown calculation.
- Conservative halo-clearance timing uses the full outer reach at the authored minimum speed. It may delay rearm longer than actual material takes to leave the halo; this is intentional to prevent stacking.
- A positive slowdown with minimum speed factor zero disables automatic object rearm by contract. Inspector text must state this explicitly.

### Acceptance criteria

1. `Automatic Birth Sources` shows Arc/Semi-Arc cyan only while Build advances.
2. Arc/Semi-Arc event duration and source dispatch stop at Build completion; Hold/Release/Rest state and controls are absent.
3. Arc, Semi-Arc, and Fleck share one per-object next-eligible time and cannot overlap or bypass it.
4. A successful Fleck cannot starve contact cycles by repeatedly winning every shared rearm interval.
5. Object packet rearm equals conservative halo clearance plus authored packet-gap clearance; minimum speed zero with slowdown enabled yields no automatic rearm.
6. Full slowdown influence scales downstream, lateral, and total velocity magnitude by the exact minimum speed factor.
7. Authored full/outer halo reach changes rebuild the existing obstacle field and obey `outer >= full >= 0`.
8. No new runtime resource, kernel, pass, sample, draw call, scene, prefab, material, layer, tag, or component exists.

### Validation status

- Read-only review and exact scope declaration: complete.
- Plan recorded before implementation edits: complete.
- Implementation: complete.
- Post-change consistency/compliance audit: complete; `35/35 PASS`.
- Patch reproduction over the immutable post-P13B.1 source: complete; final archive extraction reproduces all 323 project files byte-for-byte with no added, deleted, or differing path.
- Unity 6000.5 C# compilation, D3D11 shader import, Play Mode source/retention review, and profiling: pending user validation.

### Implementation record

- Arc/Semi-Arc `Duration` now equals resolved Build duration. Injection dispatch uses only generic progress advancement; completion immediately returns the object to shared clearance ownership.
- Arc/Semi-Arc compute evaluation contains Build geometry only. The persistent contact bypass and all Hold/Release source masks are absent.
- Arc, Semi-Arc, and Fleck use one `AutomaticObjectSourceState` dictionary. The separate Fleck next-start dictionary is removed. Active ownership is checked against all three recipes.
- Shared completion resolves conservative halo-clearance plus packet-gap time. Slowdown disabled uses factor one; positive slowdown with minimum factor zero returns infinite clearance until authoring authority changes.
- Object-contact reach controls replace hidden `0.10 m` / `0.45 m` constants and participate in obstacle-field dirty signatures.
- Canonical velocity resolves normal routing first, then scales the complete routed `float2` by one contact speed factor. At full influence total velocity reaches the exact authored minimum factor.
- Obsolete Hold/Release/Rest/Fleck-gap properties and Inspector controls are removed. The old Fleck-only gap is not migrated into the new shared object gap; existing components receive the approved new `1.0 m` default. The discovered public/debug counter consumers were added to scope before edit; the Inspector now reports Arc/Semi-Arc building, Fleck building, and waiting-for-clearance counts.
- No texture, buffer, event GPU lane, kernel, resource declaration, shader sample, pass, draw call, scene, prefab, material, metadata, layer, tag, or component was added.

### Offline validation evidence

`35/35 PASS`, including exact 18-file reconciliation, no additions/deletions, stale-state scans, serialized-property resolution, shared-owner checks, Build-only dispatch/evaluation checks, 200,000 one-shot permission cases, 200,000 packet-clearance cases, 200,000 full-vector slowdown cases, C#/HLSL/preprocessor/Markdown structure, unchanged 23-kernel manifest, unchanged eight-`float4` source GPU ABI, byte-identical P12u reveal resolver, byte-identical P13A simulation contract, and byte-identical P12t/P13A rendering include.


## RG-METRIC-P13D — Finite Object Contact Reinforcement Burst

**Status:** source implementation and offline validation complete; Unity import, Play Mode acceptance, and profiling pending.

### Objective

Make one-shot Object Arc and Semi-Arc packets establish a reliable supported contact band without restoring persistent emission. Each object packet uses a finite authored stroke count: stroke one emits the accepted complete Arc/Semi-Arc geometry, and strokes two and three, when enabled, progressively reinforce only the immediate object-contact profile. The burst then ends and the existing shared object packet-clearance gate begins.

The observed contact holes are treated as Layer C/source-establishment loss. P13D does not attribute them to Layer E and does not modify Chipping, Strands, Final Visibility, or Presence Footprint.

### Reviewed evidence and current constraints

- `StylizedRiverFoamRuntime.Injection.cs::ResolveAutomaticSourceDepositionState` currently gives Arc/Semi-Arc one normalized Build from zero to one. `UpdateAutomaticFoamSourceEvents` dispatches only when reveal progress advances and completes the event at `Elapsed >= Duration`.
- `CS_RiverFoam.compute::FoamEvaluateObjectContactArcSource` evaluates the complete front profile plus both finite wake arms; `FoamEvaluateObjectContactSemiArcSource` evaluates the selected contact half plus one finite wake arm. `FoamEvaluateAutomaticSourceContribution` uses current-minus-previous permission and writes the complete current Coverage target once.
- `AutomaticFoamSourceEvent` already carries CPU-only `ObjectBuildDuration`; the fixed GPU event ABI has an available Object Arc/Semi-Arc `Header.y` phase lane and existing previous phase/progress lanes in `Deposit`. No new GPU vector, buffer, texture, kernel, pass, or draw call is required.
- A multi-stroke event must reset deposition permission when the stroke phase changes. Otherwise normalized progress wraps from approximately one to zero and both CPU dispatch gating and GPU current-minus-previous evaluation would skip the first interval of the reinforcement stroke.
- Object `materialStepProgress` must remain based on one stroke's `ObjectBuildDuration`, not total burst duration, so raster continuity is unchanged when Stroke Count exceeds one.
- `StylizedRiverFoamRuntime.P7Diagnostics.cs::ValidateP7AutomaticSourceOwnershipContracts` currently asserts Build-only duration and a constant zero phase; it must be replaced with finite-burst, phase-reset, full-first/contact-only-reinforcement evidence.
- `StylizedRiverFoamRuntime.RevealSpeedDiagnostics.cs` still contains superseded Hold/Release/Rest wording and computes Arc/Semi-Arc reveal speed from total event duration. It must report per-stroke reveal duration and total burst duration separately.
- The source set contains no `.git` metadata. The immutable reconstructed post-P13C tree at `/mnt/data/p13d_post_p13c` is the comparison authority.

### Approved file scope

**Modify:**

1. `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
3. `Assets/Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
4. `Assets/Docs/River_Foam_Stage6_Architecture.md`
5. `Assets/Docs/River_Rendering_Roadmap.md`
6. `Assets/Game/Procedural/Rivers/StylizedRiver.cs`
7. `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
8. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.State.cs`
9. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs`
10. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs`
11. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.P7Diagnostics.cs`
12. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.RevealSpeedDiagnostics.cs`
13. `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`

**Create/Delete/Move/Metadata:** none.

If evidence requires another path, implementation stops and this scope is amended before that path is edited.

### Authoritative contract

1. Add `Object Contact Stroke Count`, integer range `1–3`, default `2`.
2. One Arc/Semi-Arc event contains exactly the configured finite number of strokes.
3. Stroke `0` progressively emits the accepted complete packet: contact profile plus finite wake arm or arms.
4. Strokes `1` and `2` progressively emit only the immediate contact profile. They never emit or refresh wake arms.
5. Every stroke uses the same resolved Reveal Speed and therefore the same per-stroke Build duration. Total event duration is `perStrokeBuildDuration × strokeCount`.
6. At a stroke boundary, CPU dispatch gating treats the changed stroke phase as new deposition even though normalized stroke progress wrapped to zero. GPU previous contribution is reset for the changed phase.
7. Reinforcement uses the existing P13A Max + Refresh birth merge. It is not additive beyond the authored Coverage target, but it may restore Coverage and Life lost since the prior stroke.
8. After the last stroke, the event ends completely. No emitter remains. The existing shared per-object clearance authority starts from final burst completion.
9. P13C full-vector slowdown remains unchanged. Unity validation uses `Object Contact Minimum Speed Factor = 0.02` for 98% slowdown; P13D does not raw-edit the scene or silently replace its serialized value.
10. The holes are not assigned to Layer E. Layer E files and controls remain byte-identical.

### Implementation sequence

1. Add the serialized Stroke Count field, sanitized public property, Inspector control, tooltip, and reset/sanitize ownership.
2. Carry the finite Stroke Count in the CPU event state and create Arc/Semi-Arc events with total duration equal to per-stroke Build duration multiplied by Stroke Count.
3. Resolve Arc/Semi-Arc deposition state as integer stroke phase plus normalized per-stroke progress. Preserve ordinary source side/progress semantics.
4. Update dispatch gating and GPU previous-state handling so phase changes reset one-shot deposition permission without reintroducing a persistent bypass.
5. In HLSL, keep complete geometry for phase zero and switch phases one/two to progressive contact-profile-only evaluation. Preserve current profile geometry, raster widths, source material, and initial life/presence.
6. Reconcile P7 and reveal-speed diagnostics with the finite-burst contract.
7. Update all five canonical documents with the accepted P13D ownership and performance implications.

### Invariants and non-goals

- Preserve P13A Coverage/Presence/Life/Pattern packing and Max + Refresh merge.
- Preserve P13C shared object rearm, packet-gap calculation, full-vector slowdown, halo dimensions, and object-source exclusivity.
- Preserve Object Arc/Semi-Arc first-stroke geometry and build order.
- Preserve Fleck, Shore, and Free-Water source behaviour.
- Preserve P12u Reveal Speed resolver calculations; only event scheduling multiplies the resolved per-stroke duration by a finite count.
- Preserve P12t Chipping and all Layer D/E rendering code byte-for-byte.
- No continuous source refresh, no Hold/Release/Rest restoration, no extra interval control, and no additive accumulation loop.

### Performance

Execution remains bounded by the existing 32-event pool and source raster kernel. An Arc/Semi-Arc event may remain active for two or three per-stroke Build durations and issue one or two additional contact-profile-only raster sequences. Wake arms are evaluated geometrically inside the shared source function but contribute zero during reinforcement phases; no full-field pass or persistent emitter is restored. Default Stroke Count `2` approximately doubles object-cycle source dispatch duration relative to P13C for those events, but object clearance begins later and the shared per-object gate prevents overlapping cycles. Measured Unity CPU/GPU cost remains pending.

**PERFORMANCE EXCEPTION — approved:** one bounded contact-only reinforcement stroke is accepted to make supported object Foam establish reliably. The lower-cost single-stroke P13C result was visually insufficient. Stroke Count remains capped at three and defaults to two.

### Acceptance criteria

1. Stroke Count is clamped to `1–3` and appears once under Object Foam.
2. Stroke one produces the complete current Arc/Semi-Arc packet.
3. Later strokes write contact profile Coverage but produce zero wake-arm Coverage.
4. No deposition interval is lost when phase changes.
5. No source dispatch occurs after the final stroke.
6. The shared object clearance timer starts only after the final stroke completes.
7. P12u requested/per-stroke reveal speed remains exact; diagnostics distinguish per-stroke duration from total burst duration.
8. No Layer E source or shader file changes.
9. Exact 13-file scope reconciliation and package reproduction pass.
10. Unity import, Play Mode contact establishment, and profiler evidence are explicitly pending.

### Implementation record

- Added one serialized `Object Contact Stroke Count` control with range `1–3` and default `2`; no scene or prefab was raw-edited.
- Arc/Semi-Arc event creation captures the sanitized stroke count and sets total event duration to resolved per-stroke Build duration multiplied by that count. Flecks remain one stroke.
- Injection resolves integer stroke phase plus normalized per-stroke progress. CPU dispatch treats a phase change as new deposition, and the existing GPU `Header`/`Deposit` lanes carry current/previous phase without changing the eight-`float4` event ABI.
- Arc and Semi-Arc phase zero preserve the complete accepted packet. Later phases return only the progressively revealed immediate contact profile. Both wake-arm paths are excluded from reinforcement.
- GPU previous contribution is omitted when the stroke phase changes, then ordinary current-minus-previous one-shot ownership resumes within the new stroke.
- Reveal-speed diagnostics now report per-stroke duration, stroke index/progress, and total burst duration. P7 diagnostics validate first-stroke progression, phase reset, contact-only reinforcement, repeated-interior zero, and finite total duration.
- P13C velocity/clearance code, P13A material transport/packing, P12u reveal resolver, and P12t/P13A Layer E rendering remain byte-identical.

### Offline validation evidence

`36/36 PASS`, including exact 13-file reconciliation; no added/deleted project paths; serialized-property and Inspector uniqueness; event-state/duration/phase ownership; exactly two contact-only reinforcement branches; persistent-emitter absence; 500,000 stroke phase/progress cases; 500,000 duration cases; 500,000 phase-boundary dispatch cases; 500,000 one-shot phase-reset cases; 500,000 per-stroke reveal-speed cases; C#/HLSL/preprocessor/Markdown structure; unchanged 23-kernel manifest; unchanged eight-`float4` source GPU ABI; byte-identical P13C velocity and P13A/P12t protected files; and byte-identical P12u reveal-speed resolver. Package reproduction is recorded in the external validation report after final archive generation.

## RG-METRIC-P13E — Independent Object Contact Reinforcement Cadence

**Status:** implementation complete; `48/48` offline source/package gates pass; Unity compilation, Play Mode cadence/shape acceptance, and profiling remain pending.

### Objective

Allow supported Object Arc/Semi-Arc contact material to be refreshed at an authored finite cadence without regenerating downstream wake arms or coupling full-packet frequency to the intentionally strong object-contact slowdown. Preserve P13D finite initial bursts, P13C full-vector slowdown, P13A material authority, P12u reveal speed, and P12t Layer E rendering.

### Read-only reviewed evidence

- `StylizedRiverFoamRuntime.BirthEvents.cs::AdvanceAutomaticObjectBirthSources` starts full Arc/Semi-Arc packets whenever the shared per-object `NextStartTime` expires and schedules Flecks from a separate activity accumulator. There is no independent contact-only maintenance scheduler.
- `StylizedRiverFoamRuntime.BirthEvents.cs::ResolveAutomaticObjectPacketClearanceSeconds` includes slowdown-halo clearance at `Object Contact Minimum Speed Factor`. With `0.02`, a `0.50 m` halo and `0.45 m/s` base Foam speed contribute approximately `55.6 s` before the authored packet gap, so stronger retention nearly disables all later object contact birth.
- `CS_RiverFoam.compute::FoamEvaluateObjectContactArcSource` and `FoamEvaluateObjectContactSemiArcSource` already interpret `Header.y >= 0.5` as progressive contact-profile-only deposition. P13D already proves phase-boundary current-minus-previous ownership; no compute-shader edit is required.
- `StylizedRiverFoamRuntime.Injection.cs::ResolveAutomaticSourceDepositionState` can emit a reinforcement-only event by fixing the existing phase lane to contact-only phase `1` while advancing one ordinary normalized stroke.
- `AutomaticObjectSourceState` currently has one `NextStartTime` and remembers only the last event type. Independent maintenance requires separate full-packet and reinforcement clocks plus the last successful Arc/Semi-Arc recipe/seed so reinforcement exactly reuses the established contact geometry.
- Full packet rearm should follow released wake clearance at normal Foam downstream speed, not clearance of contact material that is intentionally retained in the slowdown halo. The appropriate conservative distance is the completed packet's wake-arm length plus `Object Contact Minimum Packet Gap`.
- The source set contains no `.git` metadata. `/mnt/data/p13e_base` is the immutable post-P13D comparison authority.

### Approved project file scope

**Modify exactly:**

1. `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
3. `Assets/Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
4. `Assets/Docs/River_Foam_Stage6_Architecture.md`
5. `Assets/Docs/River_Rendering_Roadmap.md`
6. `Assets/Game/Procedural/Rivers/StylizedRiver.cs`
7. `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
8. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.State.cs`
9. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs`
10. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs`
11. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs`
12. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs`
13. `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs`
14. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.P7Diagnostics.cs`
15. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.RevealSpeedDiagnostics.cs`

**Create/Delete/Move/Rename/Metadata:** none inside the Unity project. Patch archive, validation report, and checksum are generated outside the project.

If evidence requires another project path, implementation stops and this scope is amended before that path is edited.

### Authoritative contract

1. Add `Object Contact Reinforcement Enabled`, default `true`.
2. Add `Object Contact Reinforcement Interval (s)`, range `1–30 s`, default `6 s`.
3. P13D full Arc/Semi-Arc packets remain finite: stroke zero emits complete contact plus wake geometry and optional later strokes reinforce contact only.
4. After a full Arc/Semi-Arc burst completes, the object records both a full-packet next-eligible time and a reinforcement next-eligible time.
5. A reinforcement event is exactly one progressive contact-only stroke. It reuses the most recent successful Arc/Semi-Arc recipe, side selection, deterministic seed, contact profile, Initial Presence, Initial Life, and Reveal Speed inputs. It emits zero wake-arm Coverage.
6. Reinforcement never changes or resets the full-packet clock. A reinforcement is allowed only while the full packet remains in clearance; once the full-packet clock becomes eligible, reinforcement is blocked. Existing P13C Fleck fairness remains: a pending supplemental Fleck may take an eligible full-packet opportunity, but reinforcement cannot take it.
7. Flecks remain behind the full-packet shared gate and cannot run concurrently with a full packet or reinforcement from the same object. Flecks do not reset reinforcement cadence, and a completed Fleck still yields the next eligible opportunity back to Arc/Semi-Arc when contact cycles are enabled.
8. Full Arc/Semi-Arc rearm is calculated from `(wake-arm length + Object Contact Minimum Packet Gap) / normal Foam downstream speed`. It no longer includes slowdown halo reach or minimum contact speed.
9. Reinforcement completion schedules the next reinforcement at `completion + authored interval`. Disabling reinforcement sets the reinforcement clock to infinity without altering full-packet cadence.
10. No persistent emitter is restored. Each reinforcement is one finite event and dispatches only while its reveal progress advances.
11. Existing `Object Contact Stroke Count` continues to control only the initial full burst. Reinforcement amount is intentionally fixed to one contact-only stroke per interval.
12. No compute shader, GPU event record, texture, buffer, kernel, pass, draw call, shader sample, scene, prefab, material, layer, tag, or component changes.

### Implementation sequence

1. Add serialized reinforcement Enabled/Interval authoring, sanitized public properties, Inspector controls, tooltips, and read-only ownership text.
2. Split per-object state into `NextPacketStartTime` and `NextReinforcementTime`; store last successful contact event type and deterministic seed.
3. Add a CPU-only event flag identifying contact-only maintenance. Create reinforcement events through the existing Arc/Semi-Arc geometry builder with contact-path reveal distance, one stroke, and fixed contact-only phase.
4. Attempt full contact cycles first, due reinforcements second, and Flecks third within the unchanged two-start-per-update budget. Preserve the existing pending-Fleck fairness inside full-cycle selection; packet eligibility still blocks reinforcement even when that eligible opportunity is yielded to a Fleck.
5. On full packet completion, schedule released-wake clearance and the first reinforcement interval separately. On reinforcement completion, update only its next interval. On Fleck completion, update only full-packet clearance.
6. Remove slowdown-halo/minimum-factor coupling from full-packet clearance authority and add a separate reinforcement-authority signature so changing cadence cannot trigger an unrelated full packet.
7. Reconcile runtime counts, public status, Object packet Inspector status, P7 contract evidence, Reveal-Speed report wording, and all five canonical documents.
8. Run exact-scope, serialized-property, scheduler-priority, event-phase, duration/reveal, rearm-independence, protected-file, resource/kernel/ABI, structure, stale-reference, and package-reproduction checks.

### Invariants and non-goals

- Preserve P13D initial finite stroke count and full first-stroke geometry.
- Preserve P13C complete-vector slowdown and authored contact-halo dimensions byte-for-byte.
- Preserve P13A Coverage/Presence/Life/Pattern packing, birth merge, Donor/TVD transport, and visibility contracts.
- Preserve P12u reveal resolver arithmetic; reinforcement uses the same recipe inputs but resolves duration from contact-profile path length.
- Preserve P12t Chipping, Strands, colour, opacity, and all Layer D/E code byte-for-byte.
- Do not add Arc/Semi-Arc Activity. Full packet frequency remains distance-controlled; the new interval controls contact-only maintenance.
- Do not allow reinforcement before an object has successfully emitted a full Arc/Semi-Arc packet.
- Do not allow reinforcement to emit wake arms, overlap an active object event, or postpone/accelerate full packet eligibility.

### Performance and risks

- No full-field work or resource is added. Each enabled participating object may issue one finite contact-only source sweep per authored interval while its full packet remains in clearance.
- Default cadence `6 s` is approximately `0.167` reinforcement starts per second per participating object before event-pool and per-update budgets. This is far below the removed persistent emitter's material-cadence writes.
- Full packet rearm becomes more frequent than P13D under extreme slowdown because retained contact material no longer blocks released-wake cadence. Wake length plus packet gap remains the conservative released-packet spacing authority.
- Reinforcement uses the existing 32-event pool and unchanged maximum two object starts per update. Busy scenes may defer, not multiply, due events.
- **PERFORMANCE EXCEPTION — approved:** bounded contact-only maintenance is accepted because P13D's finite initial burst does not reliably maintain supported Layer C contact material. The lower-cost alternative is disabling reinforcement.

### Acceptance criteria

1. Reinforcement controls appear once under Object Foam with defaults `Enabled=true`, `Interval=6 s`.
2. A full Arc/Semi-Arc burst remains byte-equivalent to P13D and ends completely.
3. A due reinforcement starts only after a previous full contact packet, uses phase `1`, has one stroke, and emits contact profile only.
4. Reinforcement emits no wake Coverage and never resets full-packet next eligibility.
5. Full-packet eligibility blocks reinforcement; existing pending-Fleck fairness may consume that eligible opportunity. While the packet remains in clearance, reinforcement is attempted before Flecks. No same-object concurrency occurs.
6. Full packet clearance uses normal downstream speed and wake length plus packet gap, with no dependency on slowdown falloff, halo reach, or minimum speed factor.
7. Changing reinforcement controls does not reset the full-packet clock. Disabling reinforcement prevents new maintenance events.
8. Runtime/Inspector diagnostics distinguish full Arc/Semi-Arc bursts from contact-only reinforcement.
9. No compute/shader/resource/ABI change exists and protected P13C/P13A/P12 files remain byte-identical.
10. Exact 15-file reconciliation and package reproduction pass; Unity compilation, Play Mode cadence review, and profiling remain explicitly pending.

### Implementation record

- Added serialized `Contact Reinforcement Enabled` and `Contact Reinforcement Interval (s)` controls under Object Foam, with defaults `true` and `6 s` and an explicit Inspector explanation that reinforcement occurs only while the next full packet remains in clearance.
- Split per-object state into independent full-packet and reinforcement clocks and remembered the last successful Arc/Semi-Arc recipe/seed.
- Added one CPU-only event classification flag. Reinforcement reuses the existing Arc/Semi-Arc event type, existing P13D contact-only phase, existing current-minus-previous deposition, and unchanged eight-`float4` GPU event ABI.
- Full packet scheduling remains first, reinforcement second while packet clearance remains active, and Flecks third within the unchanged two-start-per-update budget. Same-object concurrency remains prohibited.
- Full Arc/Semi-Arc packet clearance now uses released wake-arm length plus packet gap at normal downstream speed. Contact slowdown no longer suppresses future full-packet availability.
- Reinforcement completion updates only the reinforcement clock. Full-packet eligibility remains byte-for-byte unaffected by maintenance completion.
- Runtime status, Object packet Inspector status, P7 evidence, and Reveal-Speed reporting distinguish full bursts from contact-only reinforcement.
- Exact project delta: `15` modified files, `0` added, `0` deleted. Protected compute, velocity, material simulation, and Layer E rendering files are byte-identical to post-P13D.
- Offline validation: `48/48 PASS`, including `500,000` independent-clock cases, `500,000` P13D full-burst equivalence cases, `500,000` reinforcement clock-isolation cases, `200,000` clearance/slowdown-independence cases, `200,000` reinforcement phase/duration cases, exact scope/resource/ABI checks, and clean package reproduction over the immutable post-P13D baseline.



## RG-METRIC-P13F — Full Initial Contact Ring and Recipe-Complete Reinforcement

### Status

**Source implementation complete / offline validation complete / Unity validation pending.** P13E is the immutable comparison baseline. Unity evidence shows the bounded scheduler and full-vector slowdown materially improve object Foam, but the first finite packet does not reliably establish contact around the complete obstacle and later front-only reinforcement cannot rebuild the desired Arc/Semi-Arc contact extent.

### Read-only evidence

- `CS_RiverFoam.compute::FoamEvaluateObjectContactArcSource` currently emits `frontShape + two wake arms` for phase `0`, then only `frontShape` for every phase `>= 1`.
- `CS_RiverFoam.compute::FoamEvaluateObjectContactSemiArcSource` currently emits one selected front half plus one wake arm for phase `0`, then only that selected front half for every phase `>= 1`.
- `_FoamObstacleExclusionRead` is already bound to the source-raster kernel. The existing separate object-contact-field build runs only for Flecks; invoking it throughout Arc/Semi-Arc Build would add repeated full-field dispatches. P13F therefore derives one-cell contact confidence and outward normal from the eight neighboring obstacle-exclusion samples inside the existing bounded source dispatch.
- `StylizedRiverFoamRuntime.Injection.cs::ResolveAutomaticSourceDepositionState` and `CS_RiverFoam.compute::RasterizeFoamSourceEvent` already provide phase-aware current-minus-previous one-shot deposition. Phase changes reset one-shot permission; no persistent emitter is required.
- P13D/P13E currently reuse one `ObjectBuildDuration` for the initial full packet and every shorter contact-only stroke. That does not preserve authored metres-per-second Reveal Speed once P13F makes the initial ring path and later Arc/Semi-Arc contact paths materially different.
- The eight-`float4` GPU source-event record has one unused lane: `Deposit.w`. P13F may carry the later contact-stroke path length in that lane while preserving record size and every existing buffer/resource binding.
- `StylizedRiverFoamRuntime.P7Diagnostics.cs` and `StylizedRiverFoamRuntime.RevealSpeedDiagnostics.cs` explicitly assume one per-stroke duration and front-only reinforcement; both are direct consumers and must be updated.
- The supplied source contains no `.git` metadata. `/mnt/data/p13f_work` reconstructed from `Assets-Code-Archive(10).zip` plus P13A through P13E is the post-P13E comparison authority.

### Objective

Establish complete supported contact material around an obstacle with the first finite stroke, then reinforce the complete authored recipe contact geometry without regenerating wake arms:

- Arc and Semi-Arc first stroke: full obstacle-contact ring, followed by the recipe's existing one-time wake arm geometry.
- Arc later initial strokes and independent maintenance strokes: complete Arc contact profile, no wake arms.
- Semi-Arc later initial strokes and independent maintenance strokes: selected Semi-Arc half-profile, no wake arm.

### Approved project scope

**Modify:**

1. `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
2. `Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
3. `Assets/Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
4. `Assets/Docs/River_Foam_Stage6_Architecture.md`
5. `Assets/Docs/River_Rendering_Roadmap.md`
6. `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
7. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs`
8. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs`
9. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.State.cs`
10. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.P7Diagnostics.cs`
11. `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.RevealSpeedDiagnostics.cs`
12. `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`
13. `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Structs.hlsl`

**Create/Delete/Move/Rename/Metadata:** none inside the Unity project. Patch archive, validation report, and checksum are generated outside the project.

If evidence requires another project path, implementation stops and this scope is amended before that path is edited.

### Authoritative geometry contract

1. Phase/stroke `0` for both Arc and Semi-Arc derives a one-cell all-side contact ring directly from the existing obstacle-exclusion texture inside the event's bounded source dispatch.
2. The initial ring is progressively revealed from the upstream face toward the rear on both sides using the locally resolved outward normal. It is not a global amplitude fade and each contact cell receives one one-shot birth opportunity.
3. After the ring completes, the existing recipe wake geometry is emitted once: two existing finite arms for Arc, one selected finite arm for Semi-Arc. Wake geometry is never emitted by phase/stroke `>= 1` or by independent reinforcement events.
4. Arc phase/stroke `>= 1` uses the complete existing five-point Arc contact profile (`point0 -> point4`).
5. Semi-Arc phase/stroke `>= 1` uses only its deterministic selected half-profile (`point0 -> point2` or `point2 -> point4`).
6. The full ring uses contact-field confidence only; front-side relevance must not suppress side or rear cells. A conservative event-local bounds gate prevents contact cells belonging to a nearby obstacle from being admitted.
7. No Layer E shape, Chipping, Strand, colour, opacity, visibility, or Presence Footprint code changes.

### Reveal-speed and event contract

1. One authored Base Reveal Speed and recipe multiplier remain authoritative.
2. A full packet resolves an initial-stroke duration from `full-ring proxy length + one-time wake traversal length`.
3. Later Arc/Semi-Arc contact strokes resolve a separate duration from their actual contact-profile path length at the same requested speed and deterministic jitter.
4. Full event duration is `initial duration + (Stroke Count - 1) × contact-stroke duration`.
5. Independent P13E maintenance events contain exactly one contact stroke and use only the contact-stroke duration.
6. `ObjectBuildDuration` remains the initial-stroke duration; add CPU-only contact-stroke duration/path evidence. Pack the contact-stroke path length into the previously reserved `Deposit.w` lane and update the shared struct contract comment; do not change the eight-`float4` GPU ABI.
7. Material-step reveal feather uses the active phase duration, not always the initial duration.

### Implementation sequence

1. Add explicit initial/contact path lengths and separate durations to CPU event creation and state.
2. Update phase/progress resolution and active-phase material-step progress while preserving current-minus-previous phase reset.
3. Pack contact-stroke path length into `Deposit.w` without changing GPU record size.
4. Add bounded local obstacle-neighbour ring evaluation and event-local ownership gating to Arc/Semi-Arc phase `0`; sequence the one-time wake after the ring.
5. Keep Arc later phases on the complete front profile and Semi-Arc later phases on the selected half-profile.
6. Update Inspector ownership text and Reveal-Speed/P7 diagnostics for distinct initial/contact durations and the new geometry contract.
7. Update all five canonical documents.
8. Run exact-scope, C#/HLSL structure, signature/caller, GPU ABI, phase/duration, ring bounds, wake-one-shot, Arc/Semi-Arc distinction, protected-file, kernel/resource, stale-text, and package-reproduction checks.

### Invariants and non-goals

- Preserve P13E scheduler priority, independent reinforcement interval, full-packet clearance, same-object concurrency prevention, and CPU-only reinforcement flag.
- Preserve P13C full-vector slowdown and obstacle-field generation byte-for-byte.
- Preserve P13A packed material, merge, Donor/TVD transport, and visibility implementation.
- Preserve P12u requested-speed resolver arithmetic; only path selection and phase duration ownership change.
- Preserve P12t/P13A Layer E implementation byte-for-byte.
- Do not add new authoring controls. Existing Stroke Count and Reinforcement Interval remain the amount/cadence authorities.
- Do not broaden the contact ring beyond the existing one-cell obstacle-contact field.
- Do not restore Hold, Release, Rest, material-cadence refresh, or repeated wake emission.

### Performance and risks

- No new resource, kernel, dispatch, render-shader sample, pass, or draw call. Arc/Semi-Arc phase `0` performs eight obstacle-neighbour reads only inside the existing bounded source-raster dispatch; no full-field contact build is added.
- Phase `0` may evaluate more contact cells than the former front profile, but only during one finite initial stroke. Later strokes remain bounded contact profiles.
- The ring progress coordinate uses dot products and saturate arithmetic; no trigonometric function is added.
- Event-local bounds must include the obstacle rear while excluding nearby contact rings. Incorrect bounds are the highest visual risk.
- **PERFORMANCE EXCEPTION — approved:** one finite full-ring source sweep per complete object packet is accepted to establish supported contact material. The lower-cost fallback is the P13E front-only first stroke.

### Acceptance criteria

1. In `Automatic Birth Sources`, the first Arc and Semi-Arc stroke visibly traverses a complete narrow ring around the obstacle before/with its one-time wake arm geometry.
2. Arc stroke `2/3` and periodic reinforcement traverse the complete Arc contact profile and emit no wake arms.
3. Semi-Arc stroke `2/3` and periodic reinforcement traverse only the selected Semi-Arc half-profile and emit no wake arm.
4. No source geometry remains after each finite stroke/event completes.
5. Requested Reveal Speed is preserved independently for the longer initial stroke and shorter contact strokes, except explicit material-cadence limitation.
6. No nearby obstacle contact ring is admitted by another object's event.
7. P13E scheduling and P13C slowdown remain unchanged.
8. No new runtime resource/ABI size, kernel, pass, draw call, scene edit, or Layer E change.
9. Exact 13-file reconciliation and package reproduction pass; Unity compilation and Play Mode acceptance remain explicitly pending.

### P13F implementation record

- Exact project delta: `13` modified files, `0` added, `0` deleted.
- Phase zero derives a one-cell all-side ring from eight local obstacle-exclusion neighbour reads inside the existing bounded source dispatch, then emits the existing finite recipe wake geometry once.
- Later Arc strokes and periodic Arc reinforcement use the complete five-point contact profile. Later Semi-Arc strokes and periodic Semi-Arc reinforcement use the deterministic selected half-profile. Neither later route emits a wake arm.
- Initial and later strokes resolve separate durations from separate path lengths at one requested Reveal Speed and deterministic jitter. Full burst duration is `initialDuration + (strokeCount - 1) × contactDuration`.
- The existing eight-`float4` GPU source-event ABI remains unchanged; `Deposit.w` now carries contact-stroke path length.
- P13E scheduling, P13C slowdown, P13A material/transport/visibility, P12u resolver arithmetic, and Layer E rendering remain protected.
- Offline source validation: `63/63 PASS`, including `500,000` reveal-speed cases, `500,000` unequal-phase cases, `500,000` ring-coordinate cases, exact scope/ABI/kernel/resource checks, and protected-file byte comparisons.
- Unity compilation, D3D11 shader import, complete-ring visual acceptance, nearby-obstacle isolation, and profiler evidence remain pending.

## RG-METRIC-P13G — Object Spawning Acceptance Freeze and External Shader-Integration Pause

**Status: documentation-only implementation complete / user acceptance recorded / runtime source frozen / external shader integration pending.**

### Objective

Record the user's Unity acceptance of the complete P13B–P13F automatic-spawning and Object-spawning result, close the spawning work for the current milestone, and define the safe pause/resume boundary while another thread performs Weather cloud-shading integration in shared River shader files.

### Expected affected files

Modify:

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md
Assets/Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md
Assets/Docs/River_Foam_Stage6_Architecture.md
Assets/Docs/River_Rendering_Roadmap.md
```

Create/Delete/Move/Metadata: none.

### Reviewed evidence

- User Unity result after P13F: the implementation "works as expected" and gives a "MUCH better result than before."
- User scope decision: spawning generally, and Object spawning specifically, may be marked done for now.
- P13F final contract: every full Arc/Semi-Arc packet starts with a complete narrow obstacle-contact ring; Arc later strokes reinforce the complete Arc contact profile; Semi-Arc later strokes reinforce the selected half-profile; wake geometry remains first-stroke-only.
- P13E scheduler contract: independent finite contact-only reinforcement cadence is separate from complete-packet spacing and never emits wake arms.
- P13C retention contract: object-contact slowdown scales the complete routed velocity vector and uses the existing obstacle-routing resource.
- P13A material contract, P12u reveal-speed resolver, and P12t Layer E Chipping remain protected.
- P13F offline report: `63/63 PASS`, three `500,000`-case numerical suites, exact 13-file package reproduction, unchanged compute-kernel/resource manifests, and no new runtime resource or rendering pass.

### Accepted frozen spawning contract

1. Shore and Free-Water automatic sources are finite packet emitters with stable slot participation and distance-derived rearm spacing; they do not continuously repaint completed paths.
2. Object Arc, Semi-Arc, and Fleck sources share bounded per-object ownership. No Hold/Release/Rest material-cadence emitter exists.
3. A complete Arc/Semi-Arc packet is finite. Its first stroke establishes a complete narrow ring around the actual obstacle boundary and emits its recipe wake once.
4. Later strokes and independent maintenance events are contact-only. Arc uses the complete Arc contact profile; Semi-Arc uses the deterministic selected half-profile. No later stroke regenerates a wake arm.
5. Contact maintenance is an authored finite interval, not an every-material-update persistent emitter.
6. Object-contact slowdown applies to the complete routed velocity vector. Topology support and lifecycle remain the material-persistence authorities.
7. Initial Presence, Initial Life, Coverage, Donor/TVD transport, visibility policy, Reveal Speed, Chipping, and Strands retain their current accepted ownership. This freeze does not claim that all remaining River Foam visibility or lifetime issues are solved.

### Pause and external-edit boundary

A separate thread is authorized to make small Weather cloud-shading changes in shared River shader files. Those changes are outside P13G and outside the frozen spawning implementation. The River Foam thread must assume source drift after that work.

Before resuming River Foam work:

1. Obtain the newest user-supplied archive or exact changed files after Weather integration. Supplied files remain authoritative; do not clone or replace them from Git.
2. Read `Assets/AGENTS.md` and re-inventory the five canonical River Foam documents plus every externally changed River shader/include.
3. Diff the new source against the post-P13F/P13G baseline and classify each external change as cloud-shading-only or Foam-affecting.
4. Verify that source-event scheduling, Layer C packing/transport/lifecycle, obstacle velocity, and Layer E Foam functions are unchanged unless the user explicitly approved otherwise.
5. Run Unity compilation and a focused spawning regression before selecting the next River issue.

### Non-goals

- No implementation, shader, scene, prefab, material, tuning, serialization, resource, kernel, pass, or draw-call change.
- No performance tuning or profiler closure.
- No reopening of P12 Chipping, P12u Reveal Speed, P13A material authority, or P13B–P13F spawning architecture.
- No assertion that unrelated pending River issues are solved.

### Performance

P13G is documentation-only. Active-gameplay runtime compute, dirty-triggered runtime work, CPU/GPU memory, and build/runtime storage are unchanged. The only storage delta is the small Markdown increase in the five canonical documents. No `PERFORMANCE EXCEPTION` applies.

### Acceptance and validation

- User acceptance of P13F spawning behavior: recorded.
- P13B–P13F spawning scope: frozen for the current milestone.
- Exact documentation scope: five modified Markdown files; no project implementation files.
- Documentation validation: `19/19 PASS`, covering exact scope, zero added/deleted files, unchanged 323-file inventory, P13G presence in all five documents, Markdown fence balance, acceptance/freeze/pause wording, and no runtime-file delta.
- Unity validation is not required for the P13G document patch because it changes no runtime or serialized project data. Unity compilation and runtime spawning regression are required only after the external Weather shader integration is complete.

### P13G implementation record

Actually affected files:

Modify:

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md
Assets/Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md
Assets/Docs/River_Foam_Stage6_Architecture.md
Assets/Docs/River_Rendering_Roadmap.md
```

Create/Delete/Move/Metadata: none.

Expected-versus-actual discrepancy: none.

Post-change consistency result:

- Exact project delta: five modified Markdown files, zero added, deleted, moved, renamed, metadata, serialized, shader, or runtime files.
- All five canonical documents record the same acceptance, freeze, and external Weather-shader pause boundary.
- Markdown fence balance passes in every modified document.
- The 323-file Unity project inventory is unchanged.
- Runtime and performance validation are not applicable to P13G because it contains no implementation or serialized-data changes.



## RIVER-MOTION-S3.1 — Shoreline-Safe Trough Restoration and Variable Positive Overflow

**Status: source implementation complete / static validation passed / Unity D3D11 import and Play Mode validation pending.**

### Objective

Correct the shared Stage 3 surface evaluator so increasing Macro Wave Height or Shore Wave Height does not reduce the river's effective visible width by lowering the water below the corridor at the normal shoreline. Preserve interior trough depth, preserve positive shore lapping, make positive overflow reach follow the current crest instead of remaining broad and mechanical, and decouple overall shore-wave height variation from reach variation. Reuse the existing serialized `additionalShorelineOverlap` geometry authority as an explicit `Positive Overflow Allowance (m)` control in `Surface Motion > Shore Wave Profile`; do not add duplicate serialized state.

This same patch records successful Weather cloud integration and accepted Arc/Semi-Arc packets in the River Rendering Roadmap. It does not reopen spawning.

### Reviewed source and evidence

- Authoritative source: user-supplied `Assets-Code-Archive(17).zip`, SHA-256 `862adafc137d885207ffb0debdeaaba0b6fb18dc96744830af24831a4e19088a`, `353` safe entries, no `.git` metadata. Git branch, `HEAD`, status, diff, and history are unavailable; comparison authority is the unchanged extracted archive.
- User screenshots show alternating apparent width loss along both banks, stronger at larger Wave Height, repeated exposed-bed troughs, and long mechanically uniform overflow spans.
- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterCommon.hlsl::RiverWaterEvaluateSurfaceHeight` multiplies the complete signed `blendedHeight` by one `bankMask`. `RiverWaterResolveMotionBankMask` retains `shoreMotion` at the normal visible shoreline, so negative displacement is retained there instead of returning to the static waterline.
- `Assets/Game/Procedural/Rivers/StylizedRiverCorridorGeometry.cs::ResolveCorridorHeight` reaches `waterHeight` at the outer BedSlope boundary and begins HiddenCover from `waterHeight`; therefore negative water displacement at the normal shoreline is terrain-occluded and appears as reduced width.
- `RiverWaterResolveShoreWaveProfiles` applies one `waveSize` to both `heightProfile` and `reachProfile`, coupling overall crest height and lateral reach. The scene values `shoreWaveSizeVariation: 0.068`, `shoreWaveProfileVariation: 0.016`, and `shoreWaveTransitionLength: 3` provide little visible variation.
- `RiverWaterEvaluateSurfaceHeight` resolves hidden reach from `shoreWaveReach × reachProfile` without conditioning it on the current positive crest. Positive half-waves can therefore use a broad similar reach for much of their length.
- `Assets/Game/Procedural/Rivers/StylizedRiver.cs` already owns `additionalShorelineOverlap`, clamps it to `0–8 m`, includes it in `ResolvedShorelineOverlap`, and builds `GeneratedSurfaceHalfWidth` from that result. `StylizedRiverEditor.Authoring.cs` currently exposes it under `Shoreline Safety`; moving the same property to the Shore Wave Profile avoids duplicate ownership and gives the requested positive-overflow control.
- Direct consumers reviewed: `RiverWaterMotion.hlsl`, `RiverWaterRefraction.hlsl`, `SH_CleanStylizedRiver.shader`, `CS_RiverFoam.Topology.hlsl`, `StylizedRiverFoamRuntime.Topology.cs`, and `StylizedRiverCorridorGeometry.cs`. Foam topology already calls the shared surface evaluator and requires no signature or binding change.

### Expected affected files

Modify:

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/River_Rendering_Roadmap.md
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Authoring.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterCommon.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterMotion.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterRefraction.hlsl
```

Create/Delete/Move/Metadata/Serialized assets: none.

### Invariants and non-goals

1. The exact normal visible half-width is the minimum water edge. Negative Stage 3 displacement must be zero at and outside that edge.
2. Negative displacement remains unchanged in the interior after a smooth restoration band using the existing `Shore Motion Width` metric distance.
3. Positive displacement retains the existing `Shore Motion`, hidden bank-cover, and `Shore Wave Reach` ownership. Positive crests may use the complete generated overlap, including the authored extra allowance.
4. Hidden positive reach is conditioned on the current positive shore crest. Troughs receive zero hidden reach; weak crest shoulders receive proportionally less; crest peaks may reach the authored maximum.
5. Successive waves retain deterministic stable identities. Height and reach variation become partially correlated rather than identical; no runtime wave objects, births, lifetimes, textures, buffers, kernels, or CPU state are added.
6. Surface geometry, finite-difference normals, refraction detail attenuation, instantaneous Stage 6 shoreline support, and Foam obstacle-waterline evaluation must consume the same final surface rule.
7. Existing serialized values, shader properties, material ABI, compute resources, spawning, P13A material authority, P12t Layer E rendering, and Weather cookie integration remain unchanged.
8. No scene, prefab, material, component, layer, tag, folder, or preset default change.

### File-by-file implementation sequence

1. `RiverWaterCommon.hlsl`: split signed surface displacement into crest and trough paths; add a shoreline trough-restoration mask; make hidden reach depend on the current positive shore crest; clamp hidden shore-wave evaluation to the normal shoreline; resolve partially independent deterministic height/reach sizes without changing public function signatures.
2. `RiverWaterMotion.hlsl`: finite-difference the complete final shared surface height at every sample instead of multiplying unmasked blended heights by one centre bank mask.
3. `RiverWaterRefraction.hlsl`: obtain its detail-motion bank mask from the same final shared surface evaluator, removing its independent pre-patch reach approximation.
4. `StylizedRiverEditor.Authoring.cs`: remove `additionalShorelineOverlap` from the generic Shoreline Safety group; expose the same serialized property as `Positive Overflow Allowance (m)` under Shore Wave Profile; mark the change as structural regeneration without changing Motion preset ownership.
5. `River_Rendering_Roadmap.md`: record successful Weather integration, accepted Arc/Semi-Arc spawning, the signed shoreline invariant, crest-conditioned overflow, independent height/reach variation, and the relocated overflow control.
6. Run scope reconciliation, protected-contract searches, delimiter/preprocessor checks, deterministic numerical model checks, and source-level caller/consumer audit. Unity 6000.5.0f1 D3D11 import and Play Mode visual validation remain required from the user.

### Performance and risks

- No new texture, buffer, kernel, dispatch, render pass, draw call, simulation cadence, runtime component, or CPU update.
- The profile evaluator adds one deterministic reach-size interpolation path. The surface evaluator reuses the already-resolved profile and applies bounded scalar `min/max/smoothstep/multiply` work.
- Surface-normal finite differences evaluate the final shared height for four offsets. This removes the old central-mask approximation and keeps the profile-evaluation count comparable after eliminating the duplicate profile resolution formerly performed inside `RiverWaterEvaluateSurfaceHeight`.
- Refraction replaces one profile-only bank-mask approximation with one exact shared surface evaluation. After the normal-path refactor, the complete refraction path retains ten macro-height evaluations and reduces profile resolutions from six to five relative to the captured pre-edit source. The patch adds bounded scalar masking/interpolation work but no new scaling factor or resource. Runtime cost remains unmeasured until Unity profiling.
- Highest visual risk: a trough-restoration band that is too wide can flatten near-bank motion. The implementation must use the already-authored `Shore Motion Width`, preserve full troughs beyond that distance, and add no new tuning default.
- Highest structural risk: moving the overflow field into a non-structural Inspector group without explicit regeneration tracking. The editor must set `structuralAuthoringChanged` only for that field.

### Acceptance criteria

1. Raising `Wave Height` no longer moves the exact normal shoreline inward during negative macro or shore-wave phases.
2. At least one `Shore Motion Width` inside the bank, negative wave height is identical to the shared unprotected interior result within numerical tolerance.
3. Positive shore crests still overtop the bank-cover profile and can reach farther when `Positive Overflow Allowance (m)` is increased and the river structurally regenerates.
4. `Shore Wave Reach = 0` disables hidden overflow; `1` permits the complete generated allowance. Troughs never consume hidden reach.
5. Positive overflow grows and recedes through each crest instead of forming a broad constant-reach plateau.
6. Height and reach variation are deterministic, continuous, and not numerically identical when Size Variation is non-zero.
7. Surface displacement, normals, refraction detail attenuation, Foam obstacle interval tests, and instantaneous Shore Support use the same evaluator.
8. Weather cloud shading and P13B–P13F spawning source paths remain byte-identical.
9. Exact six-file reconciliation passes. Unity compilation, D3D11 shader import, and visual validation are recorded as pending unless supplied by the user.

### RIVER-MOTION-S3.1 implementation record

Actually affected files:

Modify:

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/River_Rendering_Roadmap.md
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Authoring.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterCommon.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterMotion.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterRefraction.hlsl
```

Create/Delete/Move/Metadata/Serialized assets: none.

Expected-versus-actual discrepancy: none.

Implemented behavior:

- `RiverWaterResolveShoreTroughMask` restores negative displacement smoothly over the authored `Shore Motion Width`, reaches exactly zero at the normal visible shoreline, and remains zero through hidden overlap. Full negative displacement is retained at least one authored restoration width inside the river.
- `RiverWaterResolvePositiveShoreReach` gives hidden reach only to positive shore crests and scales it through a smooth normalized crest envelope before applying the existing zero-slope reach bounds.
- Hidden shore-wave height evaluation clamps lateral phase input to the exact visible shoreline, so overflow continues the shoreline crest instead of changing waveform phase across generated overlap.
- Shore-wave overall height and reach use separate deterministic salts with a `45% / 55%` correlated blend for reach. Zero Size Variation remains exactly compatible with the former `1.0` multiplier.
- Surface normals finite-difference the complete final signed surface at all four samples. Refraction obtains its bank-detail attenuation from the same final evaluator. Existing Foam topology and instantaneous shore-support consumers already call that shared evaluator and required no source change.
- The existing serialized `additionalShorelineOverlap` field is now exposed exactly once as `Surface Motion > Shore Wave Profile > Positive Overflow Allowance (m)`. It retains its existing `0–8 m` clamp and structural regeneration ownership; no duplicate field or serialized migration was introduced.
- `River_Rendering_Roadmap.md` records successful Weather cloud integration, accepted/frozen Arc and Semi-Arc packet behavior, and the S3.1 signed shoreline/positive-overflow contract.

Post-change consistency and compliance result:

- Fresh authoritative archive comparison: `353 / 353` files preserved; exactly the six declared files differ; zero added or deleted files.
- Markdown fence balance: pass for both modified canonical documents.
- C#/HLSL delimiter and comment/string balance: pass for all four implementation files.
- HLSL preprocessor balance and relevant function-call arity: pass.
- Serialized/control ownership: pass; one existing field, one Inspector location, structural regeneration retained.
- Resource/per-frame audit: pass; no new texture, buffer, sampler, kernel, dispatch, pass, draw call, runtime component, `Update`, `FixedUpdate`, `LateUpdate`, or `OnValidate` path.
- Protected contract audit: `StylizedRiverFoamRuntime.BirthEvents.cs`, `StylizedRiverFoamRuntime.Injection.cs`, `StylizedRiverFoamRuntime.Obstacles.cs`, `CS_RiverFoam.compute`, `RiverWaterFoam.hlsl`, `RiverWaterFoamVelocity.hlsl`, and `SH_CleanStylizedRiver.shader` are byte-identical to the supplied post-Weather source.
- Numerical contract checks: `800,000` randomized signed shoreline-mask cases pass; `100,000` crest-conditioned reach cases pass; `50,000` deterministic height/reach-size cases confirm non-identical correlated variation (`mean absolute delta 0.051722`, maximum `0.374715`); `10,000` zero-variation compatibility cases pass.
- Analytical evaluation-count audit: the complete refraction path retains ten macro-height evaluations and reduces shore-profile resolutions from six to five relative to the captured source. Additional work is bounded scalar masking/interpolation. No measured runtime-performance result is claimed.
- Unity `6000.5.0f1` D3D11 shader import, Play Mode visual acceptance, reverse-flow/freeze/refraction regression, and profiler evidence are pending because Unity is unavailable in the patch environment.

## RIVER-MOTION-S3.1A — Surface-Detail Continuity and Shoreline-Aligned Overflow Mesh

**Status: source implementation complete / static validation passed / Unity D3D11 import and Play Mode validation pending.**

### Objective

Correct the two Unity regressions introduced by `RIVER-MOTION-S3.1` without reverting its accepted shoreline-underflow protection or positive-overflow control:

1. make trough restoration affect signed vertical displacement only, never the stable visible-water mask used by detail normals, current accents, or refraction;
2. make the single generated water mesh place vertices exactly at both normal visible shorelines and distribute existing cross-river segments through the hidden overflow bands, so positive overflow transitions through dedicated geometry instead of exposing broad triangles that straddle the shoreline.

No second water mesh exists or will be added.

### Reviewed evidence and current constraints

- Unity screenshots supplied after S3.1 show large calm-looking regions separated from detailed water by abrupt longitudinal boundaries. `RiverWaterCommon.hlsl::RiverWaterEvaluateSurfaceHeight` currently assigns `bankMask = blendedHeight < 0 ? troughMask : positiveBankMask`. `RiverWaterMotion.hlsl::RiverWaterEvaluateMotionFragment` multiplies detail-normal strength, detail-normal blending, and current accent by that output; `RiverWaterRefraction.hlsl::RiverWaterEvaluateRefraction` multiplies optical detail by the same output. The sign branch therefore changes shading authority whenever the macro height crosses zero.
- `StylizedRiverGeometry.BuildSurfaceMesh` currently distributes all cross-river vertices uniformly from `-surfaceHalfWidth` to `+surfaceHalfWidth`. Increasing `additionalShorelineOverlap` increases the distance between those vertices and does not guarantee a vertex at either `±visibleHalfWidth`. Unity screenshots show the resulting stepped polygonal water/ground intersection in the positive-overflow band.
- The existing surface is one generated mesh built by `StylizedRiverGeometry.BuildSurfaceMesh`; S3.1 added no mesh renderer, mesh object, pass, or draw call.
- Direct consumers reviewed: `SH_CleanStylizedRiver.shader` vertex/fragment calls, `RiverWaterMotion.hlsl`, `RiverWaterRefraction.hlsl`, `CS_RiverFoam.Topology.hlsl`, `StylizedRiverCorridorGeometry.cs`, and `StylizedRiver.BuildSurface`. Public shader signatures and serialized authoring remain stable.
- Comparison authority is the post-S3.1 reconstructed source: user-supplied `Assets-Code-Archive(17).zip` plus `RIVER-MOTION-S3.1_Shoreline_Safe_Troughs_Variable_Positive_Overflow_2026-07-23.zip`. No Git metadata exists.

### Expected affected files

Modify:

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/River_Rendering_Roadmap.md
Assets/Game/Procedural/Rivers/StylizedRiverGeometry.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterCommon.hlsl
```

Create/Delete/Move/Metadata/Serialized assets: none.

### Invariants and non-goals

1. Preserve S3.1 trough protection: negative displacement is zero at and outside the normal visible shoreline and full-strength in the interior after the authored restoration band.
2. Preserve S3.1 positive crest reach, independent height/reach variation, overflow authoring, Weather cookie integration, and frozen Arc/Semi-Arc spawning.
3. `bankMask` remains the compatibility output name but returns one sign-independent visible-water/detail mask. It must not branch on macro-height sign.
4. Signed displacement is resolved as `positiveHeight × positiveBankMask + negativeHeight × troughMask`.
5. Use one water mesh and the existing segment budget. Do not add a renderer, mesh, material, pass, draw call, texture, buffer, kernel, dispatch, or per-frame CPU work.
6. Preserve the exact total cross-river segment count selected by `ResolveCrossSegments`. Redistribute those segments so both visible shoreline boundaries are explicit vertices and each hidden overflow band receives dedicated intervals.
7. Preserve all shader function signatures, serialized fields, Inspector controls, scene data, prefabs, materials, components, layers, tags, and folders.

### File-by-file implementation sequence

1. `RiverWaterCommon.hlsl`: keep `positiveBankMask` as the stable output mask; split positive and negative displacement before applying their separate masks; remove the sign-switched output branch.
2. `StylizedRiverGeometry.cs`: replace uniform full-width cross coordinates with one deterministic three-band mapping—left hidden overlap, visible channel, right hidden overlap. Allocate `1/2/3` hidden intervals per side for the current low/medium/high base segment ranges while preserving the supplied total segment count. Place vertices exactly at `-leftVisibleHalfWidth` and `+rightVisibleHalfWidth` for every longitudinal row.
3. `River_Rendering_Roadmap.md`: mark S3.1 visual validation failed, record S3.1A ownership and pending Unity validation.
4. Run exact-scope, one-mesh/resource, segment-count, shoreline-coordinate, signed-height, stable-detail-mask, caller/signature, delimiter/preprocessor, protected spawning/Weather, and fresh-package reproduction checks.

### Performance and risks

- Runtime shader cost decreases slightly by replacing a dynamic sign-selection assignment with two `min/max` height components and one addition; no new profile/noise evaluation exists.
- Surface generation adds only dirty-time scalar coordinate mapping. Vertex count, triangle count, draw calls, materials, and render passes remain exactly unchanged because the total cross-river segment count is preserved.
- The visible-channel receives fewer intervals than the former uniform distribution because dedicated hidden-band intervals are now explicit. Allocation is bounded: low uses `1 + 4 + 1`, medium `2 + 8 + 2`, high `3 + 14 + 3`; runtime-disturbance counts above 20 keep only three intervals per hidden side and retain all remaining intervals in the visible channel.
- Risk: very large positive overflow allowance can still exceed the visual fidelity supported by a fixed segment budget. S3.1A targets the requested small authored increases and removes the current missing-shoreline-vertex defect; it does not authorize an unbounded vertex-density increase.

### Acceptance criteria

1. The entire normal visible river surface retains continuous detail normals, current accents, and refraction through positive/negative macro-wave sign changes.
2. No abrupt calm/wave boundary remains at a zero crossing solely because displacement changed sign.
3. S3.1 underflow protection remains exact at the normal shoreline.
4. One generated water mesh remains; renderer, mesh, vertex-count, triangle-count, and draw-call ownership are unchanged.
5. Every generated row contains explicit left and right normal-shoreline vertices plus dedicated hidden-overlap vertices.
6. Positive overflow enters the hidden band through those shoreline-aligned vertices and no longer exposes the broad uniform triangle that crossed the normal shoreline.
7. Exact four-file reconciliation passes. Unity 6000.5.0f1 D3D11 import and focused visual validation remain pending unless supplied by the user.

### RIVER-MOTION-S3.1A implementation record

Actually affected files:

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/River_Rendering_Roadmap.md
Assets/Game/Procedural/Rivers/StylizedRiverGeometry.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterCommon.hlsl
```

Create/Delete/Move/Metadata/Serialized assets: none. Expected-versus-actual discrepancy: none.

Implemented behavior:

- `RiverWaterEvaluateSurfaceHeight` now returns `positiveBankMask` through the existing `bankMask` output for all signed macro heights. Detail normals, current accents, and refraction therefore retain one sign-independent visible-water authority.
- Signed vertical displacement is resolved independently: positive height uses the existing positive shore/overflow mask; negative height uses only the S3.1 trough-restoration mask. The S3.1 no-underflow invariant is preserved without attenuating unrelated surface detail.
- `StylizedRiverGeometry.BuildSurfaceMesh` still creates one mesh with the exact existing `crossSegments + 1` vertices per row and the exact existing triangle count. Its cross coordinates now use three bands rather than one uniform full-width interpolation.
- Segment allocation is `1 hidden + 4 visible + 1 hidden` for six segments, `2 + 8 + 2` for twelve, `3 + 14 + 3` for twenty, and a maximum of three hidden intervals per side for larger runtime-disturbance counts. Each row contains exact vertices at the left and right visible shorelines and at both generated outer edges.
- No shader signature, serialized field, Inspector control, scene, prefab, material, renderer, mesh object, texture, buffer, kernel, dispatch, pass, draw call, runtime component, update method, Weather receiver, Foam source, or Layer E path changed.

Post-change consistency and compliance result:

- Exact post-S3.1 delta: four modified files, zero added or deleted files.
- `24/24 PASS` static and numerical checks.
- `800,000` randomized signed-displacement equivalence cases pass with zero numerical error relative to the intended piecewise crest/trough formula.
- `200,000` sign-independence checks confirm the detail mask does not depend on positive or negative macro height.
- `100,000` randomized asymmetric-width mesh-coordinate cases pass monotonicity, endpoint, exact-shoreline, and vertex-count checks.
- Low/Medium/High/runtime segment allocations and unchanged vertex/triangle formulas pass.
- C# and HLSL delimiter balance, HLSL preprocessor balance, Markdown fence balance, one-mesh/resource scans, stale-symbol scans, and shader caller counts pass.
- `StylizedRiverFoamRuntime.BirthEvents.cs`, `StylizedRiverFoamRuntime.Injection.cs`, `StylizedRiverFoamRuntime.Obstacles.cs`, `CS_RiverFoam.compute`, `RiverWaterFoam.hlsl`, `RiverWaterFoamVelocity.hlsl`, `SH_CleanStylizedRiver.shader`, `RiverWaterMotion.hlsl`, and `RiverWaterRefraction.hlsl` remain byte-identical to the supplied post-S3.1 source.
- Runtime cost remains unmeasured. Shader evaluation adds no noise/profile call or scaling resource. Mesh build uses the same vertex and triangle counts and adds only dirty-time coordinate mapping.
- Unity `6000.5.0f1` C# compilation, D3D11 shader import, Play Mode confirmation of full-surface detail continuity, positive-overflow smoothness, reverse flow, freeze/thaw, refraction, Stage 6 shoreline support, and profiler evidence are pending because Unity is unavailable in the patch environment.

## RIVER-MOTION-S3.1B — Authoritative Dynamic Shoreline and Stylized Shoreline Accent

**Status:** source implementation and static validation complete; Unity validation pending.

### Objective

Replace the remaining faceted positive-overflow terrain-intersection contour with one shared dynamic shoreline boundary. The same boundary must own visible-water clipping and a new complete-shoreline accent so ordinary and overflow regions use identical edge logic. Increase hidden-overflow tessellation automatically as the generated allowance grows without reducing the existing visible-channel segment count. Add authored accent width, strength, colour, and signed brightness; negative brightness darkens and positive brightness brightens.

### Reviewed evidence

- Unity screenshots after S3.1A show the no-underflow and full-surface-detail corrections working, but positive overflow still advances through stair-like segments. The defect becomes more visible as `Positive Overflow Allowance (m)` increases.
- `Assets/Game/Procedural/Rivers/StylizedRiverGeometry.cs::BuildSurfaceMesh` currently treats `ResolveCrossSegments()` as the total segment budget and assigns at most three intervals to each hidden band. Increasing hidden width therefore increases metres per hidden interval and exposes a faceted moving contact contour.
- The existing implementation still uses one generated mesh, one `MeshRenderer`, one material, one Forward pass, and one draw call. S3.1A did not add a second surface.
- `RiverWaterCommon.hlsl::RiverWaterEvaluateSurfaceHeight` already evaluates the same positive crest, resolved reach, hidden-bank attenuation, and S3.1 trough restoration used by visible displacement. `RiverWaterResolveHiddenBankCoverOffset` matches `StylizedRiverCorridorGeometry.ResolveCorridorHeight` HiddenCover ownership: `bankCover × smoothstep(hiddenT)` above static water level.
- `RiverWaterResolveCurrentVisibleShoreHalfWidth` exists for Stage 6 but performs 24 full surface evaluations plus four refinements. That search is unsuitable for every render fragment. S3.1B will instead solve the now-monotonic hidden-band contact using the already-evaluated positive shore height, reach, Shore Motion, bank cover, and a bounded scalar bisection with no additional noise/profile evaluations.
- `SH_CleanStylizedRiver.shader` has one Forward pass. Its fragment stage already owns final body composition before Foam. A shoreline accent can be applied there without a second mesh, pass, texture, depth edge detector, or rock/object outlining.
- The current visible contact fringe is accidental and not authoritative. The user accepts its drawing-like quality and explicitly approved complete-shoreline controls for thickness, strength, colour, and brightness, with brightness required to support negative values.
- Comparison authority is the post-S3.1A reconstructed source in `/mnt/data/current_river`. The supplied source has no Git metadata.

### Approved files

Modify:

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/River_Rendering_Roadmap.md
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/StylizedRiverGeometry.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Authoring.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterCommon.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterMotion.hlsl
```

Create/Delete/Move/Metadata/Scenes/Prefabs/Materials: none.

### Invariants and non-goals

1. Preserve one generated water mesh, one renderer, one material, one Forward pass, and one draw call.
2. Preserve S3.1 trough restoration, S3.1A sign-independent detail/refraction authority, positive crest/reach variation, Weather cookie integration, and frozen P13F/P13G spawning.
3. The dynamic shoreline minimum is the normal visible half-width. Negative waves never retreat it inward. Positive crests may extend it only through the generated hidden allowance.
4. Dynamic overflow contact must use the same HiddenCover equation as the corridor: static water plus `bankCover × smoothstep(hiddenT)`.
5. The render path must not call the historical 24-sample shoreline search per fragment. Use a bounded scalar solve over the monotonic hidden band and reuse already-evaluated Stage 3 values.
6. Visible-channel cross-river segments remain exactly equal to `ResolveCrossSegments()`. Hidden-band segments are additional, automatic, spacing-derived, and capped; they are not stolen from the visible channel.
7. The Shoreline Accent is lateral-shoreline-only. Do not use screen-depth edge detection and do not outline rocks, obstacles, Foam, or other scene silhouettes.
8. Accent Width is world-space metres inward from the current shoreline. Strength zero disables it. Signed Brightness range `[-1, 1]` maps to multiplier `[0, 2]`; negative values darken and positive values brighten the authored colour.
9. Do not add a texture, buffer, kernel, dispatch, render pass, draw call, runtime component, layer, tag, folder, or per-frame CPU rebuild.

### File-by-file implementation sequence

1. `River_Foam_Active_Blockers_and_Next_Patches.md`: record this plan before implementation.
2. `StylizedRiver.cs`: add serialized Shoreline Accent colour/strength/width/signed-brightness state, shader IDs, public read-only properties, clamping, defaults, and MaterialPropertyBlock bindings; bind existing `shorelineBankCover` to the render shader.
3. `StylizedRiverEditor.cs` and `StylizedRiverEditor.Authoring.cs`: add `Water Body > Shoreline Accent` and expose Colour, Strength, Width, and signed Brightness with exact ownership tooltips.
4. `StylizedRiverGeometry.cs`: treat the existing cross-segment result as visible-channel intervals; derive additional left/right hidden intervals from maximum domain hidden width and visible-channel metric spacing; preserve exact normal-shore vertices and one mesh.
5. `RiverWaterCommon.hlsl`: factor the current surface calculation so normal/refraction/Foam callers preserve their signature; add a render-only surface-and-shoreline overload that resolves the monotonic bank-cover intersection using already-evaluated shore values and bounded bisection.
6. `RiverWaterMotion.hlsl`: carry `currentShoreHalfWidth` in `RiverWaterMotionResult` and use the new render-only evaluator without changing finite-difference normal calls.
7. `SH_CleanStylizedRiver.shader`: add properties/uniforms, clip the Forward fragment against `currentShoreHalfWidth`, derive one anti-aliased world-space inward shoreline band, and blend the signed-brightness accent into body colour before Foam and fog.
8. `River_Rendering_Roadmap.md`: record the authoritative shoreline/accent contract and pending Unity validation.
9. Run exact-scope, serialized-state, shader ABI/caller, monotonic shoreline-solve, geometry spacing/cap, one-resource, delimiter/preprocessor, protected Weather/Foam, and fresh-package reproduction checks.

### Performance and risks

- Geometry: dirty-time generation adds hidden-band vertices and triangles proportional to actual hidden allowance. Visible-channel vertices remain unchanged. Hidden intervals use visible-channel metric spacing and are capped to the existing visible-segment count per bank.
- Vertex/fragment shader: one render surface evaluation now returns the current shoreline. The shoreline solve uses ten scalar bisection iterations over monotonic smoothstep functions and performs no additional noise, profile, texture, depth, or full-surface evaluations. Finite-difference normal and Foam/compute callers keep the cheaper existing path.
- Fragment composition adds one `clip`, `fwidth`, smooth band, and colour blend. No extra sample, pass, draw, or buffer exists.
- Risk: the authoritative clip changes the final visible edge from incidental depth competition to the shared solved edge. Unity must confirm no gap or floating lip at extreme Bank Cover, Shore Motion, Reach, or Overflow Allowance values.
- Risk: larger hidden tessellation increases vertex count. The cap prevents unbounded growth, but exact GPU/CPU cost remains unmeasured.

### Acceptance criteria

1. Positive overflow contact is a continuous curved contour without stair-step turns at the tested `0`, `0.15`, and `0.30 m` additional allowances.
2. Ordinary and overflow shoreline regions use one continuous current-shore boundary with no join, double line, or colour discontinuity.
3. Negative macro/shore waves never move the current shoreline inside the normal visible half-width.
4. Full-surface detail, current accents, and refraction remain continuous through wave sign changes.
5. Shoreline Accent Width, Strength, Colour, and signed Brightness affect both banks and both ordinary/overflow regions; negative Brightness darkens and positive Brightness brightens.
6. Rocks and interior objects receive no shoreline accent from this feature.
7. One mesh/renderer/material/pass/draw remains. Visible-channel segment count is unchanged; hidden segments are additional and bounded.
8. Exact nine-file reconciliation passes. Unity 6000.5.0f1 D3D11 compile/import and focused Play Mode visual/profiler validation remain pending unless supplied by the user.

### RIVER-MOTION-S3.1B implementation record

Actually affected files:

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/River_Rendering_Roadmap.md
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/StylizedRiverGeometry.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Authoring.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterCommon.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterMotion.hlsl
```

Create/Delete/Move/Metadata/Scenes/Prefabs/Materials: none. Expected-versus-actual discrepancy: none.

Implemented behavior:

- `StylizedRiver` now owns `shorelineAccentColor`, `shorelineAccentStrength`, `shorelineAccentWidth`, and signed `shorelineAccentBrightness`, with MaterialPropertyBlock bindings and no preset ownership. Existing `shorelineBankCover` is also bound to the render shader so the rendered shoreline uses the same HiddenCover height contract as corridor generation.
- `Water Body > Shoreline Accent` exposes `Colour`, `Strength`, `Width (m)`, and `Brightness`. Brightness is clamped to `[-1, 1]` and maps to colour multiplier `[0, 2]`; negative values darken and positive values brighten.
- `BuildSurfaceMesh` preserves the complete `ResolveCrossSegments()` result as visible-channel intervals. Left/right hidden intervals are additional, derive from maximum domain hidden width divided by visible-channel metric spacing, and are capped to the visible interval count per bank. Exact normal-shore and outer-edge vertices remain present in every row.
- `RiverWaterEvaluateSurfaceHeightCore` preserves the existing public `RiverWaterEvaluateSurfaceHeight` signature for finite-difference normals, refraction, Foam topology, and other consumers. The render-only `RiverWaterEvaluateSurfaceHeightAndShoreline` reuses already-evaluated shore height/reach and resolves current bank contact through ten bounded scalar bisection iterations against `bankCover × smoothstep(hiddenT)`.
- `RiverWaterMotionResult.currentShoreHalfWidth` carries the resolved edge to the Forward fragment. The fragment clips the single water surface against that edge, derives an anti-aliased world-space inward accent band from the same edge, and blends the authored accent into body colour before Foam and fog.
- Ordinary shoreline and positive-overflow shoreline therefore share one boundary and one accent calculation. Interior rocks and other scene silhouettes are not inputs to the accent.
- One water mesh, one `MeshRenderer`, one material, one Forward pass, and one draw call remain. No texture, buffer, kernel, dispatch, runtime component, layer, tag, folder, or per-frame CPU rebuild was added.

Post-change consistency and compliance result:

- Exact post-S3.1A delta: nine modified files, zero added or deleted files.
- `71/71 PASS` static, scope, contract, numerical, and protected-file checks.
- `300,000` randomized shoreline cases confirm the resolved edge remains within `[visibleHalfWidth, surfaceHalfWidth]`; negative/zero crests never extend or retreat it. Ten-step bisection differs from a 64-step reference by at most `0.007585 m` across test cases with up to `8 m` hidden width.
- `100,000` randomized asymmetric mesh cases pass monotonic coordinates, exact left/right outer edges, exact left/right normal shorelines, preserved visible interval count, and hidden-segment caps.
- C#/HLSL/shader delimiter balance, HLSL preprocessor balance, shader property/ID/binding parity, caller counts, one-pass/resource scans, historical-search exclusion, signed-brightness mapping, and roadmap/plan consistency pass.
- `StylizedRiverFoamRuntime.BirthEvents.cs`, `StylizedRiverFoamRuntime.Injection.cs`, `StylizedRiverFoamRuntime.Obstacles.cs`, `CS_RiverFoam.compute`, `CS_RiverFoam.Topology.hlsl`, `RiverWaterFoam.hlsl`, and `RiverWaterFoamVelocity.hlsl` remain byte-identical to the post-S3.1A baseline.
- A nine-entry changed-file package applied over a fresh post-S3.1A baseline reproduces all `353/353` project files byte-identically; archive traversal-path inspection passes.
- Runtime cost remains unmeasured. The render adds ten scalar bisection iterations, one fragment clip, one derivative-based anti-alias width, one smooth band, and one colour blend; no extra profile/noise evaluation or texture sample is added. Dirty-time mesh generation adds bounded hidden-band vertices/triangles while preserving visible-channel resolution.
- Unity `6000.5.0f1` C# compilation, D3D11 shader import, Play Mode shoreline/accent visual acceptance, reverse-flow/freeze/refraction/Stage 6 regression, and profiler evidence are pending because Unity is unavailable in the patch environment.


## RIVER-MOTION-S3.1C — Persistent Overflow Variation and Shoreline Coverage Blend

**Status:** source implementation and offline validation complete; Unity validation pending.

### Objective

Preserve visible shore-wave size/profile variation after the S3.1B bank-cover contact solve, clarify that the existing shore-wave length scale is also the wave-spacing/frequency control, and replace the visibly pixel-stepped water/ground colour transition with a cheap world-space coverage blend. The complete shoreline accent must follow the same blend. Ordinary and positive-overflow shoreline regions retain one authoritative dynamic boundary.

### Reviewed evidence

- User screenshots after S3.1B show the authoritative shoreline and accent functioning, but a `0.33 m` Positive Overflow Allowance with Size Variation `0.67`, Side Asymmetry `0.67`, and Profile Variation `0.85` still produces similar visible overflow widths and long intervals without positive shore waves.
- `RiverWaterCommon.hlsl::RiverWaterResolveShoreWaveProfiles` creates substantial pre-solve height/reach variation, but `RiverWaterResolveRenderedShoreHalfWidth` returns only the bank-cover intersection. Tall crests can therefore converge on similar final extensions even when their pre-solve profile values differ.
- `RiverWaterResolveCurrentVisibleShoreHalfWidth` independently resolves Stage 6 shore support through the same shared profile functions. Any post-solve variation must be applied to both render and Stage 6 outputs from one deterministic helper.
- `shoreWaveLengthScale` already changes the travelling wave coordinate wavelength. Lower values produce shorter spacing and more frequent shore waves; the current Inspector label/tooltip does not state this clearly. The user’s value `1.27` makes waves less frequent than the centre-river macro wavelength.
- S3.1B clips the Forward fragment against the analytical shoreline, which fixes geometry-defined contact ownership but leaves the final water/ground silhouette binary. The accent band is smooth inside water; the outer contact remains pixel-stepped because the final fragment is either kept or discarded.
- `Assets/Settings/PC_RPAsset.asset` serializes `m_MSAA: 1`, so alpha-to-coverage has no useful multisample coverage in the supplied PC configuration. A dither fallback would replace stairs with visible stipple. The current Forward shader already has the refracted opaque-scene colour used for body composition, so a narrow world-space blend back to that existing scene colour can provide the requested transparency-like visual transition without changing Blend, ZWrite, queue, pass count, samples, or sorting ownership.
- Comparison authority is the complete post-S3.1B reconstructed source in `/mnt/data/s31c_base`. The supplied source has no Git metadata.

### Approved files

Modify:

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/River_Rendering_Roadmap.md
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Authoring.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterCommon.hlsl
```

Create/Delete/Move/Metadata/Geometry/Scenes/Prefabs/Materials: none.

### Invariants and non-goals

1. Preserve one generated water mesh, one renderer, one material, one Forward pass, one draw call, existing Blend/ZWrite/queue state, and S3.1B adaptive hidden tessellation.
2. Preserve S3.1 trough restoration, S3.1A sign-independent detail/refraction authority, S3.1B authoritative dynamic shoreline and signed accent brightness, Weather cookie integration, and frozen P13F/P13G spawning.
3. Existing Size Variation and Profile Variation remain the only variation authoring controls. Add no duplicate frequency or variation slider.
4. Zero Size Variation and zero Profile Variation must preserve the S3.1B shoreline extension exactly.
5. Post-solve variation must only reduce a solved positive extension toward the normal shoreline. It must never exceed the solved bank-cover contact, extend past the generated allowance, or retreat below the normal visible half-width.
6. Render and Stage 6 current-shore outputs must use the same deterministic post-solve usage function.
7. Relabel the existing `shoreWaveLengthScale` display as length/spacing and state explicitly that lower values create more frequent waves.
8. Shoreline Edge Blend Width is world-space metres inward from the current shoreline, independent of accent strength. Zero preserves the hard S3.1B contact.
9. Edge blending uses the already-evaluated opaque-scene/refraction colour; do not add a texture sample, transparent blend state, alpha-to-coverage dependency, dither pattern, pass, or draw.
10. Foam, geometry, refraction source, runtime components, scenes, prefabs, materials, layers, tags, buffers, textures, kernels, and dispatches remain unchanged.

### File-by-file implementation sequence

1. `River_Foam_Active_Blockers_and_Next_Patches.md`: record this plan before implementation.
2. `RiverWaterCommon.hlsl`: add one deterministic overflow-usage profile derived from existing Size/Profile Variation; evaluate it only for the final render shoreline and final Stage 6 shore output rather than inside ordinary surface-height/normal/refraction evaluations; scale both solved extensions after bank-cover contact while preserving all public caller signatures.
3. `StylizedRiver.cs`: add serialized Shoreline Edge Blend Width, shader ID, read-only property, clamp, default, and MaterialPropertyBlock binding; clarify `shoreWaveLengthScale` spacing/frequency tooltip.
4. `StylizedRiverEditor.Authoring.cs`: relabel Shore Wave Length Scale as `Shore Wave Length / Spacing Scale`, explain lower/more-frequent behavior, and expose Shoreline Edge Blend Width under the existing Shoreline Accent group.
5. `SH_CleanStylizedRiver.shader`: add the edge-blend property/uniform; preserve the authoritative clip; calculate a world-space coverage value from current-shore distance; fade the accent through it; blend the completed water/Foam/fog colour back to the existing scene colour only inside the authored edge band.
6. `River_Rendering_Roadmap.md`: record the post-solve variation and visual coverage contract.
7. Run exact-scope, serialization/shader-binding parity, zero-variation compatibility, post-solve bounds/variation, render/Stage 6 shared-helper, one-resource, protected-file, delimiter/preprocessor, and fresh-package reproduction checks.

### Performance and risks

- Shared motion adds deterministic scalar hash/profile arithmetic once for the final render shoreline and once for each final Stage 6 row output, plus one multiply/lerp after shoreline solving. Ordinary surface-height, finite-difference normal, refraction, and Foam topology evaluations do not pay this post-solve variation cost. No additional noise, texture, surface-height, or bank-cover solve is added.
- Fragment composition adds one world-space smooth coverage calculation and one final colour lerp. It reuses `refraction.sceneColour`; there is no additional sample and no hardware blending-state change.
- Risk: very large Edge Blend Width values can visually soften more of the shoreline accent and near-bank Foam than desired. The authored range is capped at `0.15 m`, with a small default.
- Risk: the scene-colour blend is a visual contact treatment, not geometric transparency. It intentionally preserves the existing depth-writing architecture and will not reveal later transparent objects through the water edge.
- Exact GPU cost remains unmeasured until Unity profiling.

### Acceptance criteria

1. Current high Size/Profile Variation values produce materially different final positive-overflow widths even when multiple crests can overtop the bank-cover solve.
2. Zero Size/Profile Variation reproduces S3.1B final shoreline extension.
3. Lower Shore Wave Length / Spacing Scale visibly creates more frequent waves; higher values create wider spacing.
4. Render and Stage 6 shore support use the same deterministic post-solve usage profile.
5. Edge Blend Width zero preserves the current hard boundary. Positive values create a smooth visual water-to-ground transition without changing the analytical shoreline, mesh, depth, blend state, or draw count.
6. The Shoreline Accent fades with the same contact coverage and remains continuous through ordinary and overflow regions.
7. S3.1/S3.1A/S3.1B behavior, Weather integration, Foam, reverse flow, and freeze contracts remain unchanged.
8. Exact six-file reconciliation and offline validation pass. Unity 6000.5.0f1 D3D11 import, focused visual acceptance, and profiling remain pending unless supplied by the user.


### RIVER-MOTION-S3.1C implementation record

Actually affected files:

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/River_Rendering_Roadmap.md
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Authoring.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterCommon.hlsl
```

No files were created, deleted, moved, renamed, or modified outside the approved six-file scope.

Implemented behavior:

- `RiverWaterResolveShoreOverflowUsageProfile` derives one deterministic post-solve usage value from the existing Size Variation, Profile Variation, Side Asymmetry, Transition Length, spacing, seed, side, and travelling wave coordinate. At zero Size/Profile Variation it returns exactly `1`; otherwise it remains within `[0,1]`.
- The S3.1B bank-cover contact remains the maximum legal positive extension. Render and Stage 6 multiply only that solved extension by the shared usage value, so variation can pull a crest toward the normal shoreline but cannot extend past the solved contact or generated allowance.
- The helper is evaluated only for the final render shoreline and final Stage 6 row output. Ordinary surface height, finite-difference normal, refraction, and Foam topology calls retain their previous cost and signatures.
- `Shore Wave Length Scale` is displayed as `Shore Wave Length / Spacing Scale`; its tooltip states that lower values create shorter spacing and more frequent waves.
- `Water Body > Shoreline Accent > Edge Blend Width (m)` is serialized with range `0..0.15 m` and default `0.04 m`. Zero preserves the S3.1B hard visual contact.
- The Forward pass retains the analytical clip, `Blend One Zero`, `ZWrite On`, `Transparent-10` queue, one pass, and one draw. A positive Edge Blend Width smoothly blends the completed water/Foam/fog/accent colour back to the existing `refraction.sceneColour` inside the current shoreline. No new texture sample, hardware alpha blend, alpha-to-coverage, or dither exists.

Offline validation:

- Exact approved scope: six modified files, zero added/deleted/moved files.
- Structural, preprocessor, serialized-field, shader-property, MaterialPropertyBlock, Inspector, shared-caller, one-pass, Blend/ZWrite/queue, sample-count, and protected-file checks: PASS.
- `200,000` randomized zero-variation cases reproduced S3.1B usage `1.0` exactly.
- `200,000` randomized usage/bounds and post-solve cases remained within the normal shoreline and solved contact.
- Representative high-variation authoring produced post-solve usage `p10=0.4318`, `p90=0.7673`, spread `0.3355`, standard deviation `0.1314`, and maximum `1.0`.
- `100,000` randomized edge-coverage cases remained bounded and monotonic; zero width preserved hard-edge compatibility.
- Offline checks: `69/69 PASS`.
- Six-entry package traversal inspection: PASS. Applying the package to a fresh post-S3.1B baseline reproduces all `353/353` project files byte-identically.

Pending validation:

- Unity `6000.5.0f1` C# compile and D3D11 shader import.
- Play Mode comparison of the user’s current high-variation settings before/after S3.1C.
- Frequency validation using lower and higher Length / Spacing Scale values.
- Edge Blend Width visual validation at `0`, `0.02`, `0.04`, and `0.08 m`, including accent and Foam contact.
- Reverse-flow, freeze/thaw, refraction, Stage 6 shore support, and CPU/GPU profiler validation.


## RIVER-MOTION-S3.1D — Deterministic Shore-Wave Profile Evolution

**Status:** source implementation and offline validation complete; Unity validation pending.

### Objective

Add low-cost deterministic profile evolution so each travelling shore-wave identity changes shape over authored time instead of carrying one fixed normalized crest/trough silhouette across the river. Preserve the S3.1C dynamic shoreline, overflow variation, edge coverage blend, complete Shoreline Accent, full-surface detail normals, refraction, Weather lighting/shadows, Stage 6 shore support, and frozen Foam spawning contracts.

### Reviewed evidence

- User acceptance after S3.1C identifies one remaining Stage 3 issue: height, reach, start/middle/end, and post-solve usage vary, but the underlying normalized shore-wave silhouette remains recognizably repeated along the river.
- `RiverWaterCommon.hlsl::RiverWaterEvaluateMacroHeight` owns one signed normalized carrier and one steepness exponent. `RiverWaterEvaluateBlendedMacroHeightDetailed` changes shore amplitude and reach profiles but passes the same authored steepness and unmodified carrier shape into every shore evaluation.
- `RiverWaterCommon.hlsl::RiverWaterResolveShoreWaveProfiles` already derives a stable travelling `waveCoordinate`; `floor(waveCoordinate)` is a stable identity for a wave moving at the authored flow speed. A deterministic time phase keyed by that identity can evolve without CPU objects, buffers, allocations, or live reseeding.
- `RiverWaterMotion.hlsl` evaluates the same shared height for vertex displacement and four finite-difference normal samples. `RiverWaterRefraction.hlsl` independently evaluates the same height and normal contract. New evolution inputs must reach both paths or geometry, detail/refraction, and lighting normals will diverge.
- `StylizedRiverFoamRuntime.Topology.cs`, `CS_RiverFoam.Topology.hlsl`, and `CS_RiverFoam.compute` pass the complete Stage 3 shore-wave contract into obstacle-waterline and current-shore evaluation. New evolution inputs must be bound there so Foam support follows the same animated shoreline; spawning scheduling and source geometry remain outside scope.
- `SH_CleanStylizedRiver.shader` owns the authoritative dynamic shoreline clip, Shoreline Accent, edge coverage blend, Foam composition, URP `_LIGHT_COOKIES`, and final shadowed lighting. The patch must only add two scalar uniforms and pass them through existing shared motion/refraction calls; it must not change accent, Foam, shadow, cookie, blend, depth, queue, pass, or composition ownership.
- Comparison authority is the complete post-S3.1C source in `/mnt/data/s31d_base`. The supplied source contains no Git metadata.

### Approved files

Modify:

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/River_Rendering_Roadmap.md
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Authoring.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Topology.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterCommon.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterMotion.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterRefraction.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Topology.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute
```

Create/Delete/Move/Metadata/Geometry/Scenes/Prefabs/Materials: none.

### Invariants and non-goals

1. Preserve one water mesh, renderer, material, Forward pass, draw call, current Blend/ZWrite/queue state, adaptive hidden tessellation, and authoritative dynamic shoreline.
2. Preserve S3.1 trough restoration, S3.1A sign-independent detail authority, S3.1B accent ownership, S3.1C post-solve variation and edge coverage blend, Weather `_LIGHT_COOKIES`, lighting/shadow composition, refraction architecture, and P13F/P13G Arc/Semi-Arc spawning.
3. Add only `Profile Evolution Strength` and `Profile Evolution Duration (s)`. Strength defaults to `0` for exact compatibility. Duration defaults to `8 s` and has no effect at zero strength.
4. Evolution is stateless, deterministic, shader-driven, stable per travelling wave identity, smoothly blended across adjacent wave boundaries, and offset between waves and banks according to the existing side-asymmetry contract.
5. Evolution changes normalized shore-wave shape, not only amplitude or overflow reach. It may vary roundness/steepness and shoulder fullness while preserving zero crossings, sign, bounded amplitude, and the existing wave-height authority.
6. Zero evolution strength must use the exact pre-S3.1D shape path. No evolution hash or shaping arithmetic may affect the output in compatibility mode.
7. Visible displacement, finite-difference normals, refraction macro/detail attenuation, obstacle-waterline checks, rendered shoreline/accent, and Stage 6 current-shore support must consume the same evolution inputs and time.
8. New profile coefficients must be resolved once and reused through the repeated hidden-shore search for each Stage 6 side. Do not multiply evolution hashing by the existing 24 coarse plus four refinement samples.
9. Do not add GameObjects, runtime wave collections, per-wave CPU state, allocations, textures, buffers, kernels, dispatches, passes, samples, components, or draw calls.
10. Do not change Foam source scheduling, Layer C packing, Layer E Foam rendering, shoreline accent formulas, edge coverage formulas, cloud-cookie plumbing, shadow controls, disturbance ownership, scene values, or serialized assets.

### File-by-file implementation sequence

1. `River_Foam_Active_Blockers_and_Next_Patches.md`: record this plan before implementation.
2. `StylizedRiver.cs`: add serialized evolution strength/duration, public read-only properties, range clamps, compatibility defaults, shader IDs, and MaterialPropertyBlock bindings.
3. `StylizedRiverEditor.Authoring.cs`: expose both controls under Shore Wave Profile with direct lifetime/compatibility tooltips.
4. `RiverWaterCommon.hlsl`: add a stable per-wave temporal evolution resolver with boundary smoothing; add a shaped shore-only macro evaluator that preserves the current macro evaluator unchanged; thread strength/duration through shared surface and shoreline functions; resolve and reuse one coefficient set during Stage 6 hidden-shore search.
5. `RiverWaterMotion.hlsl`: pass evolution inputs through vertex displacement, fragment motion, and finite-difference normals so surface geometry and lighting normals remain coherent.
6. `RiverWaterRefraction.hlsl`: pass the same inputs through macro-height/bank-mask and finite-difference optical normal evaluation.
7. `SH_CleanStylizedRiver.shader`: add property/CBUFFER parity and pass the inputs through existing motion and refraction calls without changing accent, Foam, lighting, shadows, cookie variants, edge blend, or final composition.
8. `StylizedRiverFoamRuntime.Topology.cs`, `CS_RiverFoam.Resources.hlsl`, `CS_RiverFoam.Topology.hlsl`, and `CS_RiverFoam.compute`: bind and consume the same values for obstacle-waterline and current-shore support only; preserve source/event/spawning code.
9. `River_Rendering_Roadmap.md`: record the accepted deterministic evolution architecture and protected cross-stage consumers.
10. Run exact-scope, serialization/shader/compute binding parity, zero-strength compatibility, bounded-shape, identity continuity, per-wave offset, Stage 6 coefficient-reuse, shared-caller, accent/detail/Foam/shadow protection, delimiter/preprocessor, protected-file, and fresh-package reproduction checks.

### Performance and risks

- Recommended evolution uses one smoothed triangle-cycle scalar per travelling wave identity and lightweight polynomial shaping of the existing carrier. It adds no second trigonometric carrier and reuses the existing `pow` through an evolved effective steepness.
- Render motion and refraction add deterministic scalar/hash arithmetic to existing Stage 3 evaluations. Stage 6 resolves the evolution coefficients once per side and reuses them through its repeated contact search. Exact GPU cost remains unmeasured until Unity profiling.
- Risk: excessive evolution strength can make successive shapes visibly pulse. The default is zero; the authored control is clamped to `0..1`, and duration is clamped to a slow `1..30 s` range.
- Risk: coefficient discontinuities at wave identity boundaries would create normal/accent/shoreline seams. The implementation must use the existing metric Transition Length boundary blend and validate value continuity.
- Risk: render and Foam topology can diverge if time or properties are bound differently. Both use the current shared motion time and the same serialized properties; binding parity is mandatory.

### Acceptance criteria

1. Strength `0` reproduces the S3.1C normalized shore carrier, displacement, shoreline, normal, refraction, and Foam-support formulas exactly.
2. Positive strength makes an individual travelling wave change roundness/shoulder shape over the authored duration while retaining its stable travelling identity.
3. Adjacent waves have deterministic phase offsets and do not morph in lockstep; Transition Length keeps wave-boundary values continuous.
4. Shape evolution preserves sign, zero crossings, maximum authored amplitude bounds, trough restoration, positive-overflow limits, and post-solve variation bounds.
5. Shoreline Accent, edge coverage blend, detail normals/current accents, refraction, Foam obstacle-waterline/current-shore support, cloud-cookie lighting, and shadows remain coherent with the evolved geometry.
6. No new resource, pass, draw, kernel, dispatch, component, scene/prefab/material edit, or spawning behavior exists.
7. Exact twelve-file reconciliation and offline validation pass. Unity 6000.5.0f1 D3D11 import, focused visual acceptance, and profiling remain pending unless supplied by the user.

### RIVER-MOTION-S3.1D implementation record

Actually affected files:

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/River_Rendering_Roadmap.md
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Authoring.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Topology.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterCommon.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterMotion.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterRefraction.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Topology.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute
```

No file was created, deleted, moved, or renamed. Geometry, scenes, prefabs, materials, render passes, draw calls, textures, buffers, kernels, dispatch topology, components, spawning schedulers, cloud-cookie lighting, shadow composition, Shoreline Accent formulas, edge-coverage formulas, and Layer E Foam rendering remain unchanged.

Implemented behavior:

- `Profile Evolution Strength` (`0..1`, default `0`) and `Profile Evolution Duration (s)` (`1..30`, default `8`) are authored under Shore Wave Profile and bound identically to render and Foam-topology consumers. Strength zero returns before evolution identity/hash work and preserves the S3.1C carrier path.
- Each travelling shore-wave identity derives a stable seeded temporal phase from the existing moving wave coordinate. A smooth deterministic narrow-to-broad-to-narrow cycle evolves two bounded coefficients: existing steepness/roundness authority and a lightweight shoulder polynomial. No second wave carrier, additional sine, or additional power evaluation is introduced.
- Adjacent identities blend through the existing metric Transition Length. Existing Shore Side Asymmetry controls whether the two banks share or independently offset the evolution state.
- The shaped carrier preserves sign, zero crossings, and unit amplitude bounds. Existing trough restoration, bank-cover contact solve, positive-overflow reach, post-solve usage, authoritative shoreline clip, accent, edge blend, detail normals/current accents, refraction, lighting, shadows, and Foam support remain downstream owners.
- Vertex displacement, longitudinal finite-difference normals, fragment motion, optical refraction normals, obstacle-waterline evaluation, and Stage 6 current-shore support use the same strength, duration, seed, and motion time. Stage 6 resolves one coefficient set per bank before its bounded hidden-contact search and reuses it for every coarse and refinement sample.

Offline validation:

- Exact approved scope: twelve modified files, zero added/deleted/moved files.
- Structural, delimiter, preprocessor, serialized-property, shader-property/CBUFFER, MaterialPropertyBlock, Inspector, C#/compute binding, function-signature/call-site, one-pass, Blend/ZWrite/queue, `_LIGHT_COOKIES`, and protected-formula checks: PASS.
- Zero-strength compatibility model: `200,000/200,000 PASS`.
- Bounded shape/sign/amplitude model: `200,000/200,000 PASS`.
- Wave-identity travelling, adjacent-phase-offset, full-duration evolution, and transition-boundary continuity models: PASS; maximum sampled identity-boundary jump `6.79164e-06`.
- No additional `sin` or `pow` invocation exists relative to S3.1C. The repeated Stage 6 hidden-contact loop contains no evolution resolver.
- Offline checks: `99/99 PASS`.

Performance classification:

- No CPU lifecycle, allocation, resource, dispatch, pass, sample, mesh, renderer, or draw-count change.
- Positive evolution strength adds bounded scalar/hash/polynomial work to the existing shared Stage 3 render/refraction/topology evaluations. Stage 6 reuses one resolved coefficient set per side. Strength zero avoids evolution identity/hash work through an explicit compatibility return.
- Exact GPU cost remains unmeasured until Unity profiling.

Pending validation:

- Unity `6000.5.0f1` C# compile and D3D11 shader import.
- Strength-zero visual A/B against S3.1C.
- Positive-strength observation of one travelling wave across a complete evolution duration and adjacent waves with independent phases.
- Focused checks for Shoreline Accent/edge blend, full-surface detail/current accents, refraction, Weather cloud shade and shadows, Foam obstacle-waterline/current-shore support, reverse flow, and freeze/thaw.
- CPU/GPU profiling at the accepted final settings.



## RIVER-MOTION-S3.1D.1 — D3D11 Reserved-Token Compile Hotfix

**Status:** source hotfix and offline validation complete; Unity D3D11 reimport pending.

### Objective

Restore D3D11 compilation after S3.1D by renaming the local evolution-cycle variable `triangle`, which D3D11 parses as an HLSL primitive keyword. Preserve the exact S3.1D arithmetic, shader interfaces, evolution behavior, shoreline accent, detail normals, Foam support, refraction, Weather lighting/shadows, and all serialized values.

### Reviewed evidence

- Unity reports `syntax error: unexpected token 'triangle'` at `RiverWaterCommon.hlsl:377-378` in the River Forward vertex program and every `CS_RiverFoam` kernel that includes the shared file.
- `RiverWaterCommon.hlsl::RiverWaterResolveShoreEvolutionIdentityState` declares `float triangle` at line 377 and uses it twice on lines 378-379.
- A complete search of `Assets/Game/Rendering/Water/Resources/PS3DRiver` finds no other standalone `triangle` identifier in active `.hlsl` or `.compute` source.
- S3.1C-to-S3.1D comparison shows the failing declaration belongs only to the new deterministic profile-evolution resolver. Its direct consumers are `RiverWaterMotion.hlsl`, `RiverWaterRefraction.hlsl`, `SH_CleanStylizedRiver.shader`, `CS_RiverFoam.Topology.hlsl`, and `CS_RiverFoam.compute`; none require signature or behavior changes.
- The supplied source contains no Git metadata. Comparison authority is the reconstructed complete post-S3.1D source in `/mnt/data/s31d1_base`.

### Approved files

Modify:

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterCommon.hlsl
```

Create/Delete/Move/Metadata/Scenes/Prefabs/Materials/Geometry: none.

### Invariants and non-goals

1. Rename only the local `triangle` identifier and its two references to a D3D11-safe descriptive identifier.
2. Preserve the exact cycle, smoothing, return value, function signatures, call sites, uniforms, properties, compute bindings, and serialized controls.
3. Do not modify Shoreline Accent, edge coverage, detail normals/current accent, Foam source or topology behavior, refraction, lighting, cloud cookies, shadows, mesh generation, passes, resources, kernels, dispatches, or draw calls.
4. Do not address the unrelated `_FORWARD_PLUS` deprecation warning in this compile hotfix.

### File-by-file implementation sequence

1. Update this canonical plan with the failure evidence and exact two-file scope.
2. In `RiverWaterCommon.hlsl`, rename `triangle` and its two uses without changing any operators, constants, control flow, or surrounding code.
3. Run exact-diff, reserved-token, delimiter/preprocessor, numerical-equivalence, direct-consumer hash, package-safety, and fresh-package reproduction checks.
4. Record the actual scope, evidence, remaining Unity validation, and compliance result here.

### Acceptance criteria

1. No standalone `triangle` token remains in active PS3DRiver HLSL/compute source.
2. The hotfix diff changes exactly one local identifier at one declaration and two references.
3. The evolution identity-state output is numerically identical for representative randomized inputs.
4. Every direct consumer and all non-approved project files remain byte-identical to post-S3.1D.
5. The two-file package applies cleanly over post-S3.1D and reproduces the audited hotfix tree exactly.
6. Unity `6000.5.0f1` D3D11 import remains the final compilation authority.


### RIVER-MOTION-S3.1D.1 implementation record

Actually affected files:

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterCommon.hlsl
```

No file was created, deleted, moved, or renamed.

Implemented behavior:

- Renamed the local `triangle` variable in `RiverWaterResolveShoreEvolutionIdentityState` to `triangleWave` and changed its two references.
- Operators, constants, control flow, function signatures, call sites, uniforms, serialized values, runtime formulas, and all direct consumers remain unchanged.
- The unrelated `_FORWARD_PLUS` deprecation warning remains outside this compile-only hotfix.

Offline validation:

- Exact approved scope: two modified files; zero added/deleted/moved files.
- The code diff contains exactly one local declaration rename and two reference renames. Replacing `triangleWave` with `triangle` in the final include reproduces the post-S3.1D include byte-for-byte.
- Standalone `triangle` tokens in active PS3DRiver `.hlsl` and `.compute` source: `0` after the hotfix.
- Delimiter and preprocessor balance: PASS.
- Numerical rename-equivalence model: `200,000/200,000 PASS`, maximum difference `0`.
- `RiverWaterMotion.hlsl`, `RiverWaterRefraction.hlsl`, `SH_CleanStylizedRiver.shader`, `CS_RiverFoam.Topology.hlsl`, and `CS_RiverFoam.compute` remain byte-identical to post-S3.1D.
- Two-file package traversal, application, and complete-project reproduction: PASS; all `353/353` files match the audited hotfix tree.

Post-change consistency and compliance:

- Final diff matches the recorded two-file scope and every plan item.
- S3.1D evolution arithmetic and all protected accent, detail, Foam, refraction, Weather-lighting, shadow, geometry, pass, resource, kernel, and draw contracts remain unchanged.
- Unity `6000.5.0f1` D3D11 import is unavailable in this environment and remains the final compile validation step.


Historical rejected attempt: `RIVER-MOTION-S3.1E.1` tried to correct the incomplete S3.1E spacing split. Spacing now drives the actual shore-wave packet centres and visible repetition, while Length drives packet support width. The previous S3.1E implementation only moved deterministic wave identity and variation assignment, which left visible spacing largely unchanged.


`RIVER-MOTION-S3.1E.2 — Independent Shore-Wave Length and Gap` is Unity-rejected. Although its period was `Length + Gap`, each packet still evaluated one complete signed `2π` cycle. The positive half appeared as overflow while the negative half appeared as additional calm space, so increasing Length also increased the visible gap. Its packet-edge fade, signed-height zero-crossing fade, and Length-normalized reach activation compounded that coupling.

`RIVER-MOTION-S3.1E.3 — Positive-Lobe Length/Gap Decoupling` replaces the rejected signed cycle with one nonnegative zero-slope lobe spanning the complete authored Length. Gap is the only finite calm interval between packets; Gap zero makes adjacent lobes meet at a shared zero-slope point. Transition Length now shapes shoulders only inside the packet, and positive reach no longer normalizes against Length. The shared evaluator remains authoritative for displacement, normals/detail, refraction, rendered shoreline, Shoreline Accent, edge blending, and Foam shoreline support. Length and Gap remain dynamic motion inputs outside the immutable Foam grid descriptor. The stale Foam cache observed during S3.1E testing was explicitly rebuilt and restored normal Play Mode Foam startup.

## RIVER-FOAM-TRANSPORT-D3 — Transport-integrated FCT experiment and responsive diagnostics

**Status:** implementation and offline audit complete; Unity 6000.5.0f1 compilation, D3D11 import, Play Mode behavior, responsiveness, and GPU profiling remain pending. Production defaults remain unchanged until Unity evidence accepts a candidate.

### Objective

Implement a bounded, conservative, transport-integrated Flux-Corrected Transport (FCT) experiment for Layer C Coverage and make the existing D2 compactness tournament cooperative, cancellable, resumable, checkpointed, and continuously observable. The patch must test whether a limited high-order correction can preserve thin ribbon thickness without hiding material in Layer E, shifting the ribbon by whole cells, violating valid-fluid capacity, or severing legitimate topology.

### Reviewed evidence and constraints

- D1 proves the current scalar finite-volume transport preserves integrated material but broadens one- and two-cell ribbons severely.
- D2 proves isotropic post-transport sharpening is the wrong production design. `AntiDiffusion-0.15` improves compactness but still shifts centroids and can fragment topology; normal compression and the hybrid are rejected.
- The production runtime already resolves the multidimensional transport CFL as `downstreamCfl + lateralCfl` in `StylizedRiverFoamRuntime.Lifecycle.cs::ResolveTransportCflContract`. D3 must preserve that contract; the D2 synthetic diagonal stress does not justify replacing it.
- Packed state remains `R=C×P`, `G=C×P×L`, `B=C×P×M`, `A=C`. Every accepted correction must move all packed moments coherently using one limiter derived from Coverage.
- No long-running diagnostic may monopolize the Unity main thread. Any diagnostic exceeding a brief action must run incrementally across frames, expose stage/progress/elapsed/ETA, remain cancellable, preserve partial results, and write checkpoint reports throughout execution.

### Approved files

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/River_Foam_Stage6_Architecture.md
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Compute.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.VisibilityDiagnostics.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.CompactnessTournamentDiagnostics.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.CompactnessTournamentJob.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.CompactnessTournamentJob.cs.meta
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Simulation.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute
```

No scene, prefab, material, cache asset, layer, tag, folder, runtime component, or authored default change is approved.

Scope clarification recorded before implementation of the diagnostic label change: `StylizedRiverFoamRuntime.VisibilityDiagnostics.cs` is a direct consumer of the transport enum and must name the two experimental FCT modes accurately in copied Coverage reports. The change is label-only; report formulas, readback, buckets, and production behavior remain unchanged.

### FCT experiment contract

D3 adds two explicit experimental transport selections while retaining `Donor Cell` and `TVD Superbee` byte-for-byte in their selected execution paths:

```text
Experimental FCT — Low
Experimental FCT — Medium
```

Both use the same three-stage conservative substep:

1. **Low-order transport and candidate correction generation**
   - Compute the bounded Donor Cell update into an intermediate packed state.
   - Compute one signed anti-diffusive correction mass per interior east/north face from a Lax-Wendroff target minus the Donor Cell flux.
   - A positive face correction moves material from the negative-side cell to the positive-side cell; a negative value moves it in the opposite direction.
   - Correction packed moments use the intensive material state of the actual correction donor.

2. **Per-cell limiter construction**
   - Derive lower/upper Coverage bounds from the current cell plus open valid neighbours and clamp the upper bound to resolved valid-fluid capacity.
   - Accumulate all requested positive and negative correction contributions for the cell.
   - Compute bounded `R+` and `R-` acceptance ratios. Uniform fields, closed faces, empty gaps, negative Coverage, and capacity overflow must remain impossible.

3. **Limited correction and lifecycle**
   - Each face uses one shared limiter coefficient based on donor removal and receiver addition capacity.
   - Apply equal-and-opposite packed correction mass to the low-order state.
   - Clamp only as a final numerical guard, then execute the existing lifecycle/topology evaluation once on the final substep.

The Low and Medium modes differ only in a fixed diagnostic correction scale. They are not new authoring controls and do not alter the serialized default. The experiment allocates temporary FCT textures only while one of those modes is selected.

### Responsive D2 tournament contract

Replace the synchronous `RunConservativeCompactnessTournament()` body with a cooperative job:

- Start returns immediately after validation and immutable input capture.
- Work advances from `StylizedRiverFoamRuntime` once per frame with a strict time budget of approximately `4 ms`.
- Public state exposes active/paused status, normalized progress, current stage/case, elapsed time, and ETA.
- Inspector actions provide Start, Pause/Resume, Cancel, Copy Partial/Final Report, and Open Report Folder.
- Completed units append to the in-memory report and rewrite a checkpoint file under `Library/RiverFoamDiagnostics`; cancellation retains that partial report and completed-case ledger.
- Play Mode exit, disable, destruction, or resource invalidation must cancel safely rather than leave callbacks or stale state.
- D1 remains historical evidence and is not rerun by D3. Its legacy synchronous Inspector launch is retired/disabled rather than leaving another blocking diagnostic available.

### Acceptance criteria

1. Existing Donor Cell and TVD Superbee execution, allocations, bindings, and serialized defaults remain unchanged when selected.
2. FCT substeps conserve packed material across interior faces to floating-point tolerance, respect open longitudinal outflow, and enforce `0 <= C <= validFluid`.
3. One limiter coefficient applies to all four packed moments so decoded Presence, Remaining Life, and Pattern are not independently distorted.
4. Selecting either FCT mode allocates and binds temporary resources deterministically; leaving FCT releases them without changing the persistent state.
5. The D2 tournament Start action returns within one frame, the Inspector remains interactive, progress and ETA update, Pause halts work, Resume continues, Cancel completes promptly, and partial reports remain copyable.
6. No diagnostic frame performs more than the configured cooperative time slice except one indivisible case; the report records the slowest slice and case.
7. Documentation records the rejected D2 post-pass, the exact D3 algorithm, the nonblocking diagnostic rule, performance cost, and pending Unity validation.

### File-by-file sequence

1. Update this canonical plan before code.
2. Extend transport enum/tooltip and validation without changing the default.
3. Add FCT temporary resources and kernel handles with conditional allocation/release.
4. Add HLSL resources, face-correction helpers, limiter construction, and three compute kernels.
5. Dispatch FCT kernels from the existing material substep loop only for the two experimental modes.
6. Convert D2 to an incremental state machine and expose progress/cancellation controls in the Inspector.
7. Update Stage 6 architecture and record exact production/non-production boundaries.
8. Run scope, delimiter, shader-resource, kernel-binding, enum/default, lifecycle-once, ping-pong, cancellation, checkpoint, and package-reproduction audits.

### Risks and non-goals

- FCT may still fail one-cell thickness or may create grid-aligned artifacts. This patch is an experiment, not approval of the algorithm.
- Three FCT compute passes and temporary textures increase work while an experimental mode is selected. Donor/Superbee cost must remain unchanged.
- The patch does not change births, source geometry, lifecycle settings, visibility modes, Presence Footprint, Chipping, Strands, topology caches, or rendering.
- The patch does not restore the removed historical predictor/corrector implementation; no recoverable implementation exists in the supplied repository.

### D3 implementation record

Implemented behavior:

- `StylizedRiverFoamTransportScheme` now exposes `Experimental FCT — Low` and `Experimental FCT — Medium`; the serialized field still initializes to `DonorCell` and the existing Donor/TVD enum values remain `0/1`.
- FCT selection conditionally creates four full-field `ARGBHalf` temporary textures: low-order packed state, east correction mass, north correction mass, and per-cell limiter. Leaving either experimental mode releases them without clearing, remapping, or replacing the persistent state.
- `BuildFctLowOrder`, `BuildFctLimiter`, and `ApplyFctCorrection` implement the recorded three-stage substep. The Low/Medium fixed correction scales are `0.35 / 0.70`. Standard Donor/TVD selection still dispatches only `SimulateFoam`; the FCT textures are not bound and the FCT strength is not set while a standard mode is selected.
- The final FCT transport accounting reconstructs the unclamped Donor raw mass before adding accepted correction mass, so any low-order capacity clip remains visible in the existing clamp-loss and Presence-attribution diagnostics rather than being concealed by the intermediate texture.
- The former synchronous D2 entry point is replaced by an `IEnumerator<int>` state machine advanced from `LateUpdate` under `UNITY_EDITOR` with a `4 ms` target slice. Start captures immutable inputs and returns; Pause/Resume update the state immediately; explicit Cancel finalizes immediately between slices; lifecycle cancellation preserves a partial report.
- Inspector state now shows progress, current stage/case, elapsed time, ETA, checkpoint errors, Pause/Resume, Cancel, Copy Partial/Final Report, and Reveal Report. Checkpoints are written from completed rows throughout execution, and the final ledger records the slowest observed slice and its case.
- The old D1 synchronous launch button is disabled and labelled `D1 Runner Retired`; the accepted D1 report remains the fixed baseline instead of exposing another potentially blocking Inspector action.
- `River_Foam_Stage6_Architecture.md` now records the rejected D2 post-pass, exact D3 equations/pass ownership, conditional resource/dispatch cost, packed-moment contract, responsive diagnostic rule, and pending acceptance boundary.

Intentional changed-file set:

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/River_Foam_Stage6_Architecture.md
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Compute.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.VisibilityDiagnostics.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.CompactnessTournamentDiagnostics.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.CompactnessTournamentJob.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.CompactnessTournamentJob.cs.meta
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Simulation.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute
```

### D3 offline audit evidence

- Final changed-file scope versus the supplied D2 baseline: `17/17` approved files, no deletion, no scene/prefab/material/cache/layer/tag edit.
- Changed C#/HLSL delimiter, string/comment, and preprocessor balance: `PASS`, `0` errors.
- Tournament blocking/thread-wait scan: `PASS`; no `Task.Run`, sleep, wait handle, cancellation-token worker, synchronous task wait, or persistent Editor callback was introduced.
- Responsive contract scan: `PASS`; the `4 ms` target slice, iterator yields, progress state, Pause/Resume, immediate inter-slice Cancel, checkpoints, lifecycle cancellation, and responsive execution ledger are all present.
- Kernel parity: all three FCT kernels have matching pragma, HLSL definition, C# `FindKernel`, and dispatch sites.
- FCT resource parity: all eight read/write resource names have matching HLSL declaration, kernel use, and C# binding.
- Shared-include impact: `CS_RiverFoam.Resources.hlsl` and `CS_RiverFoam.Simulation.hlsl` are consumed only by `CS_RiverFoam.compute` in PS3DRiver.
- Serialized default audit: `DonorCell` remains the initializer; the TVD HLSL branch remains exact value `1`.
- Standard lifecycle audit: the extracted `FoamApplyLifecycleAndWrite` body is whitespace/comment-normalized identical to the D2 `SimulateFoam` lifecycle tail.
- New `.meta` GUID `93fd42881a8f49ce969a218aa7084ec5`: unique across `Assets`.
- Legacy D1 launch audit: `PASS`; the old synchronous Inspector action is retired/disabled, while its accepted report remains the fixed baseline.
- Independent randomized packed-correction mirror: `2,000/2,000 PASS`; maximum four-moment conservation error `1.066e-14`, maximum Coverage-capacity excess `1.110e-16`, maximum packed invariant excess `0`.
- Independent one-cell 1D pulse sanity at production CFL over one metre equivalent travel conserves total mass for all modes. Effective width is `6.839472` cells for the Donor low-order baseline, `5.915613` for FCT Low, and `5.138264` for FCT Medium; centroids are `36.666667`, `36.564692`, and `36.399958`. This proves only that the candidate direction reduces mirror diffusion; it also predicts remaining over-width and backward phase error, so neither strength is accepted.
- Final delivery archive audit: the archive contains exactly the `17` approved changed/new files; overlaying it on the supplied D2 baseline reproduces the complete final `Assets` tree byte-for-byte; archive traversal and unexpected-entry scans pass.
- Unity compiler and shader compiler are unavailable in the patch environment. C# compilation, D3D11 compute import, actual resource creation/binding, GPU transport accounting, visual thickness/centroid/topology behavior, runtime cost, and the nonblocking Inspector workflow remain explicitly pending.

### D3 acceptance status

- Criteria 1–4 and 7: offline structure/invariant evidence passed; Unity parity remains pending.
- Criteria 5–6: implementation is present, but actual Editor responsiveness, Pause/Resume, cancellation latency, checkpoint persistence, and slowest-slice evidence require direct Unity execution.
- No FCT candidate is approved as production behavior by this patch. Donor Cell remains the serialized default and `Lifecycle-Faithful` remains the honest visibility mode for transport evaluation.



## RIVER-FOAM-TRANSPORT-D4 — single-pass compactness/performance candidate patch (historical; superseded by D5)

### Decision basis

The D3 FCT Low/Medium runtime candidates are rejected. Direct Material Coverage review showed continued footprint expansion with lower intensity, and their three-dispatch/four-temporary-texture cost cannot satisfy the project's zero-regression performance ceiling. Do not strengthen FCT, hide its spread in Layer E, or retain its resources as dormant production alternatives.

D4 tests two replacements simultaneously while preserving the accepted maximum structural budget:

```text
one material dispatch per CFL substep
zero additional full-field transport textures
no cache/topology/source/lifecycle/render-pass expansion
```

### Implemented candidate selections

```text
Donor Cell
TVD Superbee
Bulk-Phase Residual TVD — Experimental
```

Enum values `0/1` remain Donor/TVD. Rejected D3 values `2/3` are reused by the two D4 experiments so existing serialized experimental selections resolve to a current experiment rather than an invalid enum. The serialized initializer remains `DonorCell`.

#### Bulk-Phase Residual TVD

- Accumulates base downstream displacement as signed subcell phase.
- Extracts whole-cell shifts per CFL substep and applies them by offsetting the existing state read coordinate inside `SimulateFoam`.
- Subtracts the same base component from longitudinal transport velocity, leaving slowdown residuals and lateral/routing velocity for the existing Superbee reconstruction.
- Binds previous/current fractional phases separately and shifts each committed state sample before interpolation.
- Evaluates manual and automatic birth positions with the active phase.
- Uses centre-row Motion Lane intent across the river width only while this candidate is selected; obstacle routing remains local.
- Adds no kernel, dispatch, buffer, or texture.

#### Nearest-Characteristic

- Uses the same scalar bulk downstream phase as Bulk-Phase Residual TVD.
- Converts residual/lateral subcell displacement into deterministic temporally rounded integer crossings through a scalar low-discrepancy sequence.
- Selects one source cell and copies its complete packed material state.
- Explicitly avoids the naive nearest-sample failure where production displacement below half a cell would never move.
- Applies valid-fluid clipping and the existing lifecycle write once.
- Records before/after/clamp accounting; conservation is intentionally not guaranteed.
- Adds no kernel, dispatch, buffer, or texture and removes the neighbour/face reconstruction work from the selected hot path.

### Rejected D3 removal

- FCT enum labels and runtime selection logic are removed.
- FCT kernel lookups and dispatches are removed.
- FCT temporary RenderTextures and conditional allocation/release are removed.
- FCT HLSL resources, helper functions, and compute entry points are removed.
- Historical D3 documentation remains as a rejected record; current architecture and Inspector text point only to D4 candidates.

### Diagnostic workflow correction

A new `Transport Quick Gate` is the default D4 action:

1. User selects one transport mode.
2. One click starts ten seconds of existing steady-state work accounting and returns immediately.
3. The Inspector remains interactive; explicit Cancel preserves a partial report.
4. Completion requests the established Coverage/Visibility report asynchronously.
5. Copy returns one combined report for comparison with the other modes.

It does not silently cycle serialized settings and does not claim GPU timing. Exact GPU acceptance remains a direct identical-state Profiler comparison. The old D2 exhaustive tournament is relabelled `Legacy D2 Exhaustive Tournament — Optional`, explicitly states that it does not test D4, and remains cooperative/cancellable only for historical use.

### Required Unity validation

1. Compilation and shader import with no missing kernel/property/binding error.
2. Material Coverage captures for TVD, Bulk-Phase, and Nearest at birth/1 m/2 m for one-cell horizontal and vertical ribbons.
3. Forward/reverse flow and nonzero-phase birth alignment.
4. Mode switching without state clear, coordinate jump, or birth misregistration.
5. Nearest gain/loss and obstacle-hole review through existing transport accounting.
6. Quick Gate start/cancel/complete/copy behavior with no blocking frame.
7. Identical-state GPU mean and P95; any candidate above TVD is rejected.

### Non-goals

- No Presence×Life state rewrite.
- No Lagrangian ribbon system.
- No resolution increase.
- No visibility threshold compensation.
- No new authoring slider.
- No production-default change.

### D4 unresolved decision — closed by D5 evidence

Select at most one candidate only after visual compactness and GPU evidence. If Bulk-Phase fails due phase/boundary artifacts and Nearest fails due loss/duplication, return to architecture assessment under the same zero-regression performance ceiling rather than adding corrective passes.


## RIVER-FOAM-TRANSPORT-D6 — accepted transport and active spawn-pack blocker

### Closed transport decision

`Bulk-Phase Residual TVD` is accepted and promoted as the production/default material transport. It retains enum value `2`, one material dispatch per CFL substep, and no additional full-field transport resource. D5 measured `+0.064%` aggregate GPU mean and `+0.491%` aggregate GPU P95 against TVD; the upper approximate 95% mean-regression bound was `+0.461%`, below the hard 1% ceiling. Bulk accounting was no worse than TVD and its Capacity/Clamp loss was lower in both paired blocks.

The completed D5 acceptance suite is removed from the Inspector and retired to a code tombstone. Nearest-Characteristic, FCT, D1, and D2 remain rejected/retired and must not be restored.

### Active blocker: automatic packets are too large and can coalesce

The user wants many small thin pockets, not a small number of gigantic connected packs. Current evidence and code audit identify three independent contributors:

- **Oversized authored packets:** default Shore Ribbon events resolve to approximately `3.58–4.18 m` at Patch Size `0.35`; Free-Water Lace reaches `5.80 m`; object Arc packets can emit two `1.80 m` wake arms.
- **No cross-source separation authority:** clearance is per deterministic slot or object anchor, so neighbouring slots and different source families can create paths that overlap or merge.
- **Overlap rejuvenation:** `FoamMergeBornMaterial` refreshes Presence and Remaining Life with `max` even when a birth adds no new Coverage, allowing repeated overlap to keep a large connected pack alive.

### Approved next investigation order

1. Implement and test added-Coverage-only birth merging so overlap cannot refresh existing cells without extending geometry.
2. Add a bounded active-event envelope separation gate shared by shore, object, and free-water starts; no GPU readback or new full-field resource.
3. Re-author source length envelopes downward while retaining thin width, strong material, and comparable event population. Start with Shore Ribbon and Free-Water Lace.
4. Review lateral Motion Lane divergence and obstacle velocity gradients only after spawn packets remain independent.

## RIVER-FOAM-SPAWN-D7 — active validation and authoring recalibration

### Implemented code correction

- `Bulk-Phase Residual TVD` remains the accepted production transport.
- Birth overlap can no longer refresh Presence, Remaining Life, or Pattern unless it genuinely adds Coverage.
- Automatic Shore, Object, and Free-Water packets share one bounded active/recent envelope reservation, preventing intersecting prepared packets from starting as one welded pack.
- Same-anchor contact-only reinforcement remains intentionally permitted; the new merge prevents age rejuvenation where it adds no Coverage.
- No scene, prefab, serialized value, GPU resource, kernel, dispatch, simulation cadence, or render path changed.

### Required focused validation

1. Confirm Unity C# and compute compilation.
2. In `Runtime Diagnostics > Foam > Birth Activity`, verify `Shared Packet Separation` reports reservations and occasional overlap rejections without permanently saturating all `64` slots.
3. Observe Shore, Object, and Free-Water births separately and together. Multiple small packets should remain spatially independent instead of beginning as one connected pack.
4. Verify old material does not visibly brighten or regain life when an overlapping source writes no new Coverage.
5. Confirm same-anchor Object contact reinforcement still runs when enabled and does not recreate a giant persistent reservoir.

### Manual authoring recalibration after code validation

Do not reduce Coverage, Activity, ribbon thickness, initial Presence, or global lifetime first. Reduce individual prepared packet dimensions and compensate packet spacing so the river may retain many small identities rather than a few long connectors.

Initial recommended trial values are recorded in the delivery message rather than serialized by D7. Reassess motion/velocity only after these packet-size changes, so source geometry and transport deformation remain separable.

### Remaining work

- Validate D7 packet independence in Unity.
- Apply the recommended Shore/Free-Water length and mix values manually to the existing river instance.
- Reassess residual Motion Lane and obstacle-routing divergence only if small independent packets still merge or stretch after birth.

## RIVER-FOAM-SPAWN-D8 — cell-exact source geometry contract

### Status

- D8.1 baseline and ABI freeze: **COMPLETE — STATIC AUDIT; UNITY VALIDATION PENDING**
- D8.2 cell-authoritative serialized controls: **NOT STARTED**
- D8.3 shared exact primitives: **NOT STARTED**
- D8.4 Shore conversion and continuation scheduling: **NOT STARTED**
- D8.5 Object-source conversion: **NOT STARTED**
- D8.6 Free-water conversion: **NOT STARTED**
- D8.7 manual/scripted injection conversion: **NOT STARTED**
- D8.8 rendering-footprint correction: **NOT STARTED**
- D8.9 full contract and performance acceptance: **NOT STARTED**

### Objective

Replace metric-authored and source-specific birth geometry with a shared cell-authoritative contract. Every automatic and manual source must obey explicit Length, Width, progressive-head, bend, offset, contact-span, and wake-arm dimensions expressed in Foam cells. `Min = Max = 1` must resolve to exactly one cell. Fractional Coverage caused by subcell placement or rotation is valid; hidden width, feather, cap, jitter, or rendering expansion is not.

### Acceptance criteria

- Every automatic and manual source uses cell-authoritative resolved dimensions.
- `Min = Max = 1` resolves to exactly `1` for every seed.
- Isolated one-cell births integrate to `1.00 ± 0.02` cells in the applicable local dimension.
- A one-cell source never produces two fully occupied adjacent rows or columns.
- Coverage is written only where the texel square geometrically overlaps the authored envelope.
- Length and Width remain independent, including `Length < Width`; no universal capsule rule is permitted.
- Curvature, breakup, and irregularity may remove Coverage inside an envelope but may not expand it.
- The full morphological Object contact ring is removed from Arc and Semi-Arc authored footprints.
- Shore Ribbon uses deterministic same-bank continuation rather than sparse packet-only scheduling.
- Final visual support is bounded by sampled Coverage.
- Bulk-Phase Residual TVD remains production transport.
- No additional Foam simulation pass, persistent texture, persistent buffer, per-cell stage, or grid-resolution increase.
- Total River GPU regression remains below `1%` under identical-state comparison.
- `Assets/Game/Demo/Scenes/VisualFrameworkDemo.unity` remains untouched.

### Approved files and review surface

Canonical documentation:

- `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
- `Assets/Docs/River_Foam_Stage6_Architecture.md`
- `Assets/Docs/River_Foam_Fixed_Metric_Grid_Upgrade_Plan.md`
- `Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md`

Primary C# contracts and producers/consumers:

- `Assets/Game/Procedural/Rivers/StylizedRiver.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.State.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthTransfer.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.SourceUnits.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.RuntimeUpdates.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Binding.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Compute.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.RevealSpeedDiagnostics.cs`
- `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
- `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`

GPU contracts and evaluators:

- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Structs.hlsl`
- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl`
- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`
- `Assets/Game/Rendering/Water/Shaders/Includes/RiverWaterFoam.hlsl`

A temporary diagnostic partial may be added only under the approved path:

- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.CellSpawnerContractDiagnostics.cs`

### Reviewed evidence and current constraints

- `StylizedRiverFoamRuntime.State.cs::AutomaticFoamSourceEventType` declares eight automatic recipes.
- `StylizedRiverFoamRuntime.State.cs::AutomaticFoamPacketReservation` stores metric global/lateral bounds and therefore cannot remain the sole authority for cell-authored source separation.
- `StylizedRiverFoamRuntime.State.cs::AutomaticFoamSourceEvent` remains metric-authoritative for source width, reach, feather, reveal path, head trail, object wake length, and object contact path length.
- `StylizedRiverFoamRuntime.State.cs::FoamSourceEventGpuData` and `CS_RiverFoam.Structs.hlsl::FoamSourceEventData` are fixed eight-`float4` CPU/GPU structures with source-dependent lane reuse. Repacking feasibility must be proven before preserving the current stride is promised.
- `River_Foam_Active_Blockers_and_Next_Patches.md` records Bulk-Phase Residual TVD as accepted transport and D7 added-Coverage-only merging plus bounded packet reservations as the current baseline.
- Existing source evaluators use metric dimensions, hidden raster floors, and source-specific expansion. D8 must replace them rather than tune defaults.
- The exact raster implementation must be validated numerically. A simple projected linear feather is not accepted as exact for arbitrary orientation.
- Length and Width require independent bounded-patch and bounded-ribbon semantics. A capsule formula cannot represent `Length < Width` without violating the authored envelope.

### Invariants

- Preserve Layer C packed-state meaning and added-Coverage-only birth merging.
- Preserve transport, lifecycle cadence, support/lifetime capture, and existing accepted obstacle motion behavior during source-contract work.
- Preserve source identities and serialized tombstones long enough to avoid breaking existing scenes.
- Preserve one bounded source-event buffer unless the ABI audit proves a stride change is unavoidable; no second source-event buffer is permitted.
- Preserve Editor responsiveness. Any diagnostic uses incremental Editor updates and asynchronous GPU readback only; unsupported async readback reports `UNSUPPORTED` and stops.

### Non-goals

- No obstacle slowdown/routing redesign in D8.
- No new Foam transport scheme.
- No grid-resolution change.
- No scene/prefab/material tuning or migration.
- No unrelated diagnostic cleanup.
- No production-default authoring change before the user manually validates a supplied value block.

### File-by-file implementation sequence

1. D8.1: record baseline symbols, enum consistency, kernel/property/stride inventory, source-event lane map, resource counts, and current dispatch ownership in this plan; no runtime behavior change.
2. D8.2: add hidden metric tombstones and cell-authoritative serialized controls; update Inspector bindings; keep old raster path temporarily.
3. D8.3: implement shared bounded-patch and bounded-path-ribbon texel-overlap primitives plus isolated primitive diagnostics.
4. D8.4: convert Shore Ribbon and Inward Wash; add per-bank Ribbon continuation state and bypass same-run packet rejection.
5. D8.5: convert Object Arc, Semi-Arc, Fleck; remove the full morphological ring; crop contact paths and audit each component independently.
6. D8.6: convert Lace and Cross-Lace; replace Torn Fragment runtime geometry with a bounded broken filament while retaining serialized identity if required.
7. D8.7: add cell APIs and route legacy metric manual injection through local metric-to-cell conversion and shared primitives.
8. D8.8: make final visual geometric support proportional to Coverage and clip detail masks by Coverage.
9. D8.9: run full source-contract, packing, symbol, kernel, serialized-property, package-overlay, and identical-state GPU acceptance audits.

### Risks and required responses

- **ABI capacity risk:** eight `float4` lanes may be insufficient after explicit head/contact/wake controls. Produce a complete per-source lane map before editing structures. Stop and update this plan before any stride increase.
- **Raster accuracy risk:** arbitrary-angle overlap may fail the `±0.02` target. Treat numeric audit results as authority; do not hide error with a wider tolerance without approval.
- **Shore scheduling risk:** allowing overlap alone will not create continuous ribbons. Implement explicit same-bank run continuation.
- **Rendering risk:** replacing the Coverage threshold may expose weaknesses in chips/strands or visual warp. Enforce `final geometric alpha <= sampled Coverage support` and audit shared shader/include impact.
- **Serialization risk:** existing scene values include temporary D7 tuning. Do not auto-migrate or edit the scene; provide manual values after acceptance.
- **Performance risk:** exact overlap may cost more than source-specific distance fields. Measure source raster and total River cost; reject a `>=1%` total regression.

### Validation and compliance checks

- Full cross-partial C# symbol audit.
- Transport enum audit against `StylizedRiverFoamTransportScheme`.
- CPU/GPU structure layout and stride equality audit.
- Shader-property ID declaration/use audit.
- Compute kernel pragma/lookup/binding audit.
- Inspector `SerializedProperty` declaration audit.
- No retired enum or lifecycle diagnostic references.
- No new dispatch, persistent texture, persistent buffer, or per-cell state.
- Async-only focused diagnostic behavior with cancel, progress, ETA, and partial report.
- Exact source tests at axis-aligned, fractional-offset, diagonal, curved, and boundary placements.
- Identical-state GPU mean/P95 comparison against Bulk-Phase baseline.
- Final package contains changed source and Markdown files only.

### D8.1 recorded baseline

Static audit completed against the supplied `Assets-Code-Archive(28).zip` source tree. The archive contains no Git metadata, so no repository commit SHA or diff-against-HEAD proof is available.

#### Transport and retired-symbol consistency

- Active transport references resolve only to `DonorCell`, `TvdSuperbee`, and `BulkPhaseResidualTvd`.
- `ExperimentalBulkPhaseResidualTvd`, `CancelTransportAcceptanceSuiteForLifecycle`, and `AdvanceTransportAcceptanceSuiteFrame` are absent from the River Foam C#/HLSL tree.
- `RasterizeFoamSourceEvent` and `RasterizeFoamSourceEventDebug` each have matching compute pragmas, kernel bodies, and C# `FindKernel` lookups.

#### Source-event allocation baseline

- Source-event capacity: `32` records (`StylizedRiverFoamRuntime.Constants.cs::AutomaticFoamSourceEventCapacity`).
- Packet-reservation capacity: `64` CPU records (`AutomaticFoamPacketReservationCapacity`).
- GPU event structure: eight `Vector4`/`float4` lanes = `128` bytes per record.
- Source-event GPU allocation: one structured buffer created in `StylizedRiverFoamRuntime.Resources.cs` with `Marshal.SizeOf<FoamSourceEventGpuData>()`.
- Foam runtime buffer allocations in the reviewed resource allocator: topology metrics, transport metrics, and automatic source events. D8 must not add another source buffer.
- Production and debug raster kernels share the same event buffer and source evaluator path; debug adds its existing debug UAV/counter path only when active.

#### Current CPU-to-GPU lane map

| Lane | Non-Arc/Semi-Arc meaning | Arc/Semi-Arc meaning |
|---|---|---|
| `Header.x` | source type | source type |
| `Header.y` | side sign | finite stroke phase |
| `Header.z` | current reveal progress | current per-stroke reveal progress |
| `Header.w` | shape seed | shape seed |
| `Distance.xy` | start/end storage-global distance | contact point 0 xy |
| `Distance.z` | centre storage-global distance | object-centre storage-global distance |
| `Distance.w` | flow direction | contact point 1 x |
| `Shore.x` | shore inset | contact point 1 y |
| `Shore.y` | width metres; Shore Ribbon may pack thickness metres/cells | straight wake-arm length metres |
| `Shore.z` | inward reach | normalized material-step progress |
| `Shore.w` | feather metres | contact point 2 x |
| `Material.x` | intrinsic Presence | intrinsic Presence |
| `Material.y` | normalized Remaining Life | normalized Remaining Life |
| `Material.z` | pattern seed | pattern seed |
| `Material.w` | pattern feature size | pattern feature size |
| `Variation.x` | source-fill seed | negative-half first-segment split |
| `Variation.y` | zero/reserved | contact point 2 y |
| `Variation.z` | zero/reserved | contact point 3 x |
| `Variation.w` | curvature / selected recipe rotation or side | curvature / selected Semi-Arc side |
| `Kinematics.x` | formation speed metres/second | contact point 3 y |
| `Kinematics.y` | moving-head trail metres | contact point 4 x |
| `Kinematics.z` | derived source path length metres | initial contact-path length metres |
| `Kinematics.w` | source-fill blend tombstone | positive-half first-segment split |
| `ObjectData.x` | object/free-water centre lateral metres | object-centre lateral metres |
| `ObjectData.y` | object half-length / free-water half-length | contact point 4 y |
| `ObjectData.z` | object half-width / free-water half-width | front split |
| `ObjectData.w` | contact offset / free-water recipe parameter | source-local lateral cell spacing metres |
| `Deposit.x` | previous side/phase | previous stroke phase |
| `Deposit.y` | previous progress | previous stroke progress |
| `Deposit.z` | previous-state-valid flag | previous-state-valid flag |
| `Deposit.w` | zero | reinforcement contact-stroke path length metres |

#### ABI conclusion

- **Proven:** the existing stride is exactly `128` bytes and is mirrored by CPU/HLSL declarations.
- **Proven:** Arc/Semi-Arc consume all 32 scalar components except that some material semantics are shared rather than geometry-specific.
- **Proven:** non-Arc recipes have several nominally reserved components, but those lanes are not uniformly free across all recipes.
- **Inference — High confidence:** preserving one eight-`float4` structure remains feasible only through source-specific repacking and reconstruction; it is not yet proven for the complete D8 control set.
- **Gate for D8.2/D8.3:** define the replacement per-recipe packing table before changing `FoamSourceEventGpuData` or `FoamSourceEventData`. Any stride increase or second buffer requires a plan update and approval.

#### Static limitations

- Unity C# compilation, compute shader import, Inspector serialization binding, runtime dispatch counts, and GPU timing were not executable in this environment.
- The supplied archive includes serialized assets, but D8.1 changed only this canonical Markdown plan.
- D8.1 is complete as a static baseline/ABI freeze; Unity validation remains required before treating the supplied archive as a compiled baseline.


## D8.2 implementation record — cell controls and packing design

Status: **IN PROGRESS — SOURCE EDITS PENDING STATIC AUDIT**

Objective:

- Add the complete cell-authored source-control surface without changing source raster behavior in this patch.
- Preserve all legacy metric fields as runtime-authoritative migration tombstones until D8.3 converts each recipe.
- Record a replacement eight-`float4` packing design before any CPU/HLSL ABI edit.

Approved files for D8.2:

- `Assets/Game/Procedural/Rivers/StylizedRiver.cs`
- `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
- `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`

Reviewed direct surface:

- `StylizedRiver.cs`: complete automatic Shore, Object, and Free-Water serialized source fields, public accessors, defaults, and `OnValidate` clamps.
- `StylizedRiverEditor.Foam.cs`: complete source recipe Inspector sections and all `SerializedProperty` lookups for the legacy metric controls.
- `StylizedRiverFoamRuntime.State.cs`: current CPU source-event record and packet-reservation contracts.
- `CS_RiverFoam.Structs.hlsl`: current eight-`float4` source-event record.
- `StylizedRiverFoamRuntime.BirthEvents.cs`: all producers and per-recipe lane assignments.
- `CS_RiverFoam.compute`: all automatic-source lane consumers.

D8.2 invariants:

1. No automatic or manual birth geometry changes.
2. No CPU/GPU event stride change.
3. No compute or shader edits.
4. No scene, prefab, material, or serialized asset edits.
5. New cell fields are serialized and Inspector-visible but explicitly staged; legacy metric values remain runtime-authoritative through D8.2.
6. Every Min/Max pair is clamped to `>= 1` and ordered in `OnValidate`.
7. Every head dimension is independently clamped to `>= 1`.
8. Every cell reveal speed is independently clamped to the documented cell-speed range.

Replacement event packing design for D8.3:

- Keep the existing `128-byte` / eight-`float4` record.
- Preserve lanes 0–1 for common identity, progress, amount, presence, life, and recipe selection.
- Preserve lanes 2–6 as recipe-union payload rather than assigning one universal meaning.
- Preserve lane 7 for common progression and compact auxiliary values.
- Reconstruct local cell spacing from the bound Foam-grid descriptor in the kernel; do not transmit per-event spacing after D8.3.
- Reconstruct deterministic source-local noise from `stableId` and recipe; do not transmit random variation lanes.
- For Shore, Inward Wash, Fleck, Lace, Cross-Lace, and Broken Filament, lanes 2–4 hold start/end or patch basis plus body/head dimensions; lanes 5–6 remain available for bend, offset, and source-specific bounds.
- For Arc/Semi-Arc, retain five prepared contact points in lanes 2–6. Pack authored contact width/span, wake width/length, and head dimensions into lane 7 plus currently redundant common components after D8.3 removes metric spacing and hidden source-scale data. If the final producer/consumer audit cannot fit these values without precision loss, stop before ABI edits and revise this plan; a stride increase remains unapproved.

File-by-file sequence:

1. Add staged serialized cell controls and public accessors in `StylizedRiver.cs`.
2. Add defaults and `OnValidate` clamps in `StylizedRiver.cs`.
3. Replace visible metric geometry controls with a clearly labelled staged Cell Geometry block in `StylizedRiverEditor.Foam.cs`; retain legacy metric fields hidden from the custom Inspector.
4. Run static serialized-property, duplicate-symbol, brace, and source-tree consistency checks.
5. Record final diff, unchanged runtime authority, and pending Unity validation here.

Acceptance for D8.2:

- C# source is syntactically balanced.
- Every new Inspector property resolves to exactly one serialized field.
- No existing runtime getter or birth-event producer is redirected to the new fields.
- No HLSL or compute file changes.
- No serialized asset changes.
- Existing source visuals remain unchanged by construction.

### D8.2 completion audit

Status: **SOURCE COMPLETE — STATIC AUDIT PASS; UNITY COMPILATION/INSPECTOR VALIDATION PENDING**

Implemented:

- Added staged cell controls for Shore Ribbon, Inward Wash, Object Arc, Object Semi-Arc, Object Fleck, Free-Water Lace, Free-Water Cross-Lace, and the future Broken Filament replacement.
- Added independent body/contact/wake/head dimensions, cell offsets or bend amplitudes, and reveal speed in cells per second.
- Added public sanitized accessors and one centralized `OnValidate` sanitizer.
- Added a shared Cell Min/Max Inspector helper and replaced visible legacy metric geometry controls with staged cell controls.
- Preserved all legacy fields and all runtime birth producers unchanged.
- Preserved the CPU/GPU source-event ABI and all compute/shader files unchanged.

Static evidence:

- `StylizedRiver.cs`: balanced `{}` and `()` counts; no duplicate serialized field names.
- `StylizedRiverEditor.Foam.cs`: balanced `{}` and `()` counts.
- All `Find("...")` references in `StylizedRiverEditor.Foam.cs` resolve to serialized fields in `StylizedRiver.cs`; missing count `0`.
- Representative staged controls occur only in `StylizedRiver.cs` and `StylizedRiverEditor.Foam.cs`; no runtime birth producer consumes them.
- `CS_RiverFoam.Structs.hlsl` is byte-identical to the D7C archive baseline.
- `CS_RiverFoam.compute` is byte-identical to the D7C archive baseline.
- Changed files are restricted to the three D8.2-approved files.

Intentional behavior difference:

- The custom Inspector now presents staged cell geometry instead of legacy metric geometry for automatic source recipes.

Intentionally unchanged behavior:

- Automatic source event preparation, GPU packing, raster geometry, packet scheduling, transport, lifecycle, and final rendering are unchanged.
- Existing D7C metric values remain runtime-authoritative until D8.3.

Pending verification:

- Unity C# compilation under `6000.5.0f1`.
- Inspector rendering with zero missing-property or layout errors.
- Visual parity check confirming unchanged births despite editing staged values.

---

# D8.3A implementation record — shared bounded ribbon foundation and Shore conversion

Patch: `RIVER-FOAM-SPAWN-D8.3A_Cell_Exact_Shore_Primitives`

## Implemented

- Added a source-longitudinal cell-spacing resolver alongside the existing lateral resolver.
- Converted Shore Ribbon event preparation to use only the D8 cell controls for:
  - resolved segment length;
  - resolved width;
  - head length;
  - head width;
  - bank offset;
  - reveal speed.
- Converted Inward Wash event preparation to use only the D8 cell controls for:
  - along-bank length;
  - stroke width;
  - inward reach;
  - head length;
  - head width;
  - bank offset;
  - bend amplitude;
  - reveal speed.
- Removed Patch Size, metric length/width/reach interpolation, post-resolution size jitter, width/length coupling, fixed metric feather, and hidden metric head-trail authority from these two recipes.
- Repacked Shore Ribbon and Inward Wash recipe-local GPU lanes without changing the fixed eight-float4 / 128-byte event ABI:
  - `shore.x` = offset cells;
  - `shore.y` = body width cells;
  - `shore.z` = inward reach cells;
  - `shore.w` = head width cells;
  - `variation.w` = bend amplitude cells;
  - `kinematics.x` = reveal speed cells/second;
  - `kinematics.y` = head length cells;
  - `kinematics.z` = path length cells;
  - `kinematics.w` = D8 cell-source contract marker.
- Added shared HLSL bounded-ribbon helpers.
- Shore Ribbon uses analytic one-dimensional texel interval overlap in the longitudinal and lateral cell axes. It has no feather or cap enlargement.
- Inward Wash uses a bounded four-by-four texel-area estimator over a square-capped finite path segment. Samples outside the finite longitudinal path domain are rejected, preventing capsule-cap extension.
- Updated fixed-metric dispatch bounds and automatic packet envelopes so Shore/Inward cell values are converted through local cell spacing rather than interpreted as metres.

## Deliberately not included

- Same-bank Shore Ribbon continuation scheduling is not implemented yet. Existing candidate-slot cadence and packet scheduling remain active.
- Object Arc, Semi-Arc, Fleck, Free-Water, and manual injection remain on their previous metric raster paths.
- The final Lifecycle-Faithful rendering footprint remains unchanged.
- The focused cell-contract audit is not yet exposed. D8.3A must first compile and receive direct visual validation before the diagnostic is built around the final primitive behavior.
- The old P7 metric probe diagnostics have not yet been rewritten for the D8 Shore lane semantics and must not be used as acceptance evidence for these recipes.

## Static verification performed

- No new compute kernel or dispatch.
- No new texture or buffer.
- `FoamSourceEventGpuData` remains eight `Vector4` values / 128 bytes.
- Object Arc/Semi-Arc packing path is unchanged.
- Changed C# and HLSL files have balanced braces and parentheses.
- The supplied package contains source and documentation files only.

## Required next phase

1. Compile and visually validate D8.3A in Unity.
2. Add the incremental Cell-Exact Spawner Contract Audit for the shared primitive and Shore/Inward recipes.
3. Replace sparse Shore Ribbon packet scheduling with deterministic same-bank continuation.
4. Only after those pass, reuse the primitive for Object sources.


## D8.3B — Remaining cell geometry conversion

Implemented after D8.3A:

- Object Arc/Semi-Arc/Fleck recipe-local dimensions are union-packed as Foam-cell values without changing the 128-byte event stride.
- Free-Water Lace, Cross-Lace, and the serialized Torn identity now consume the staged cell Length, Width, Head, and Bend controls.
- Final geometric support is proportional to Coverage rather than `smoothstep(0.02, 0.10, C)`.
- The known Shore Ribbon scheduling/orientation/transport defect is deliberately unchanged and remains the immediate post-conversion blocker.
- Legacy metric fields remain serialization tombstones; they are no longer geometry authority for the converted GPU recipes.


## D8.3C — Automatic-only contract audit and obsolete manual-source removal

The obsolete manual Foam spawning surface is removed rather than hidden. This includes its serialized controls, public normalized/metric emission APIs, moving manual composition producer, pending manual/material-birth queues, dispatch-range helpers, Inspector section, `InjectFoam` kernel, C# kernel lookup, and closed P7 manual-source report. The surviving `WriteIsolatedLifeProbe` path is a diagnostic state writer, not a gameplay/authoring spawner.

The replacement `Cell-Exact Spawner Contract Audit` covers only the eight surviving automatic recipes. It runs 672 deterministic cases incrementally across Editor updates: 8 recipes × 7 geometry scenarios × 12 seeds. It exposes Run, Cancel, progress/ETA through the shared diagnostic status, preserves partial reports on cancellation, and writes TXT/CSV reports under `Library/RiverFoam`. No scene, prefab, material, cache, or serialized river state is mutated.

## D8.3C8 plan — Play Mode cell-exact GPU audit replacement

Status: IMPLEMENTATION IN PROGRESS

### Objective

Replace the rejected Edit Mode `EditorApplication.update` audit runner with a Play Mode-only runner that advances from the live `StylizedRiverFoamRuntime.LateUpdate` lifecycle after the production Foam runtime is ready. Preserve isolated diagnostic render targets and asynchronous GPU readback; do not write the visible river's persistent Foam state.

### Acceptance criteria

- Two explicit actions exist: `Run Cell-Exact Smoke Suite` and `Run Cell-Exact Exhaustive Suite`.
- Both actions are disabled outside Play Mode and report `PLAY MODE REQUIRED` when invoked incorrectly.
- Smoke suite covers all eight recipes plus half-cell, diagonal, 1x5, 3x1, and one-cell-head cases in no more than 40 cases.
- Exhaustive suite retains the 672-case matrix.
- Runner advances only from `LateUpdate`; no `EditorApplication.update`, play-mode callback, assembly-reload callback, or shutdown callback remains for this audit.
- Runtime readiness is observed, not driven by the diagnostic. The audit never calls `EnsureResources()`.
- Readiness wait has a hard timeout and reports the exact initialization/resource state.
- Live Inspector state includes suite type, runtime readiness, current phase, case, readback state, completed/total, pass/fail, elapsed time, ETA, latest measurement, and report paths.
- Cancellation preserves partial TXT/CSV reports and invalidates pending readback callbacks safely.
- Audit resources are isolated per case and released without `DestroyImmediate` from `OnValidate` or runtime callbacks.
- No runtime source geometry, transport, scene, prefab, material, or serialized asset is changed.

### Approved files

- `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.P7Diagnostics.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs`
- `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`

### Reviewed evidence and constraints

- The rejected runner starts from `RunCellSpawnerContractAudit`, subscribes to `EditorApplication.update`, and calls `EnsureResources()` while still in Edit Mode.
- `StylizedRiverFoamRuntime.Lifecycle.LateUpdate` already owns Play Mode runtime initialization and calls `EnsureResources()` before normal Foam work.
- `P12Diagnostics` and `P12Sweep` establish the existing Play Mode diagnostic convention and readiness condition: `initializationPhase == Ready && AreResourcesCompleteAndCurrent()`.
- The existing GPU case dispatcher already writes to temporary `DebugTexture` and `StateTexture` targets and reads only the temporary debug target through `AsyncGPUReadback`.
- The eight-float4 / 128-byte automatic source ABI remains unchanged.

### File-by-file sequence

1. Replace the audit state machine in `P7Diagnostics` with Play Mode smoke/exhaustive suite selection, readiness timeout, LateUpdate advancement, live state properties, cancellation, and report ownership.
2. Add one editor-guarded audit advancement call to `Lifecycle.LateUpdate` after runtime resources are confirmed ready; include the active audit in the runtime-work predicate.
3. Replace the old Edit Mode Inspector block with Play Mode smoke/exhaustive controls and complete live status rows.
4. Audit the final diff for stale Edit Mode hooks, stale action labels, missing method/property references, delimiter/preprocessor balance, and unintended runtime changes.

### Non-goals

- No source raster behavior changes.
- No Shore Ribbon scheduling, orientation, or transport fix.
- No performance acceptance run in this patch.
- No changes to other historical diagnostics.

### Risks and mitigations

- Pending readback after cancellation: generation token invalidates the callback; resources are released by the callback or immediate cancellation path when no readback is pending.
- Play Mode exit during a run: `OnDisable` cancels the suite and preserves a partial report before runtime resources are released.
- Runtime not ready: hard timeout fails with initialization phase and resource-completeness evidence instead of waiting indefinitely.
- Repaint overhead: Inspector repaint is throttled and active only while a suite is running.

### D8.3C8 implementation and audit record

Status: IMPLEMENTED; UNITY PLAY MODE VALIDATION PENDING

Implemented exactly within the approved four-file scope:

- Replaced the single Edit Mode action with Play Mode-only Smoke and Exhaustive actions.
- Smoke suite contains 48 cases: six high-value scenarios for each of eight automatic recipes.
- Exhaustive suite retains 672 cases: eight recipes × seven scenarios × twelve seeds.
- Removed all Cell-Exact audit subscriptions to `EditorApplication.update` and all audit-specific play-mode, assembly-reload, and editor-shutdown callbacks.
- The audit now advances only through editor-guarded calls from `StylizedRiverFoamRuntime.LateUpdate`.
- The diagnostic observes normal runtime initialization and never calls `EnsureResources()` itself.
- Added a 15-second runtime-readiness timeout with exact phase/resource evidence.
- Added separate Smoke and Exhaustive TXT/CSV report names under `Library/RiverFoam`.
- Added complete live Inspector state: suite, runtime readiness, runner phase, current case, pass/fail, readback, elapsed/ETA, latest result, and report path.
- Replaced use of the production automatic-source event buffer with a dedicated one-record diagnostic event buffer per case.
- Temporary State and Debug textures remain isolated diagnostic targets; visible persistent Foam state is not bound as the write target.
- Play Mode exit or component disable cancels the active suite and preserves a partial report before normal runtime resources are released.

Static compliance evidence:

- No `EditorApplication.update` Cell-Exact audit subscription remains.
- No Cell-Exact audit `playModeStateChanged`, `AssemblyReloadEvents`, or `EditorApplication.quitting` hook remains.
- No old `RunCellSpawnerContractAudit` method or old single-action label remains.
- New runtime/Inspector method and property references resolve one-to-one across the changed partial and editor files.
- Changed C# files pass comment/string-aware brace, bracket, parenthesis, and preprocessor-balance checks.
- No compute shader, source raster behavior, transport code, scene, prefab, material, or serialized asset changed.

Pending proof:

- Unity C# compilation in 6000.5.0f1.
- Play Mode runtime readiness and Smoke-suite execution.
- Exhaustive suite execution only after Smoke passes.

## D8.3C9 plan — GPU audit fixture and measurement correction

Status: IMPLEMENTED; UNITY PLAY MODE VALIDATION PENDING

### Objective

Correct the Play Mode GPU audit harness so body tests measure complete bodies, head tests measure only the one-cell moving head, Object recipes receive valid synthetic prerequisites, Inward Wash receives real geometry assertions, and the exhaustive readiness watchdog cannot expire while the production runtime is already ready.

### Acceptance criteria

- Runtime source geometry, transport, rendering, scene data, serialized river values, and the eight-float4 automatic-source ABI remain unchanged.
- `1 × 5 Body` and `3 × 1 Body` cases reveal the complete body rather than a one-cell head window.
- `Head = 1 × 1` runs on a larger `5 × 3` body and reports head-specific expected dimensions.
- Report and CSV distinguish Body versus Head measurements and include expected dimensions.
- Inward Wash body cases receive a real area assertion; curvature is not used as a blanket exemption.
- Object Arc/Semi-Arc fixtures use source-local contact-profile offsets and produce a valid non-degenerate profile.
- Object Fleck receives an isolated synthetic object-contact field with non-zero support and a valid normal.
- The readiness timeout measures only one continuous not-ready interval and is reset immediately when resources are ready.
- Exhaustive execution may exceed fifteen seconds without a false readiness failure.
- Broken Filament remains exempt from full-rectangle area equality because authored breakup may remove interior Coverage; all measured support remains reported.
- All per-case temporary textures and buffers are released after readback, cancellation, or completion.

### Approved files

- `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.P7Diagnostics.cs`

### Reviewed evidence

- `StylizedRiverFoamRuntime.P7Diagnostics.cs`: current suite constructs every scenario with `HeadLengthCells = 1` and `HeadWidthCells = 1`, so body tests measure only the active one-cell head.
- `StylizedRiverFoamRuntime.P7Diagnostics.cs`: current Object contact points are populated with absolute global positions, while the D8 HLSL interprets them as source-local metric offsets before converting them to cells.
- `CS_RiverFoam.compute`: Object Fleck multiplies geometry by `_FoamObjectContactFieldRead.r * .a`; the current harness binds a black neutral texture, guaranteeing zero output.
- `StylizedRiverFoamRuntime.Lifecycle.cs`: the audit is advanced once with `runtimeReady=false` before the resource-ready branch and again with `true` afterward. Direct resource readiness must therefore be authoritative inside the audit runner.
- The Smoke and partial Exhaustive reports show one-cell Shore/Inward success, body cases collapsing to approximately one cell, all Object fixtures returning zero, and the exhaustive run aborting after fifteen seconds despite `phase=Ready; complete=True`.

### File-by-file sequence

1. Extend per-case capture metadata with expected body/head dimensions and isolated object-contact texture ownership.
2. Correct readiness handling and timeout reset.
3. Correct body/head scenario construction.
4. Replace invalid Object fixture coordinates and bind a valid synthetic Object Fleck contact field.
5. Apply area assertions to Inward Wash and expected-width adjacency checks to head cases.
6. Extend TXT/CSV evidence and release all new temporary resources.
7. Run complete static delimiter, preprocessor, symbol, diff-scope, and stale-runner checks.
8. Record implementation and pending Unity proof in this plan.

### Non-goals

- No source raster changes.
- No Lace, Cross-Lace, Broken Filament, Shore scheduling, Object runtime, transport, or rendering correction.
- No scene, prefab, material, cache, or serialized asset changes.
- No new compute kernel, dispatch, persistent texture, or persistent buffer.


### D8.3C9 implementation and audit record

Implemented within the approved two-file scope.

- No runtime source preparation, source raster HLSL, transport, lifecycle, Editor action, scene, prefab, material, cache, or serialized asset changed.
- The diagnostic body scenarios now set Head Length/Width equal to Body Length/Width, revealing the full body.
- The dedicated head scenario now uses a `5 × 3` body with a `1 × 1` head and reports `ExpectedLength/ExpectedWidth = 1 × 1`.
- The readiness path directly checks `AreCellSpawnerAuditResourcesReady`; the earlier `LateUpdate(false)` hint cannot start or expire the watchdog while resources are complete.
- Object Arc/Semi-Arc contact points are source-local metric offsets.
- Object Fleck receives a temporary ARGBHalf contact field cleared to `(support=1, normal=(-1,0), alpha=1)`.
- Inward Wash joins the singular-recipe area assertion after its audit fixture separates path Length from Inward Reach.
- Broken Filament remains measurement-only for area because its authored internal breakup intentionally removes Coverage.
- New object-contact textures are owned by the per-case capture and released through the existing readback/cancellation cleanup path.

Static evidence completed:

- Comment/string-aware brace, bracket, and parenthesis scan: PASS.
- Preprocessor balance: PASS.
- New field, helper, binding, expected-dimension, readiness-reset, and resource-release invariants: PASS.
- Final diff scope: only the approved canonical Markdown plan and `StylizedRiverFoamRuntime.P7Diagnostics.cs`.
- No compute-shader or automatic-source ABI diff.

Pending proof:

- Unity C# compilation in `6000.5.0f1`.
- Corrected 48-case Smoke execution.
- Exhaustive execution beyond fifteen seconds without false readiness failure.

## D8.3C10 — Deterministic replay and oriented footprint evidence

Status: IMPLEMENTED; UNITY PLAY MODE VALIDATION PENDING

### Objective

Make the Play Mode GPU audit reject suite-order-dependent evidence, report independent oriented Length and Width measurements, measure Coverage outside the authored envelope, and stop treating non-zero composite output as an automatic pass.

### Implementation

- One unreported GPU warm-up dispatch now completes before case 001.
- Smoke contains the existing 48 measured cases plus six deterministic replay cases for cases 001–006.
- Replay compares Coverage area, projected Length, projected Width, and outside-envelope Coverage within `0.01`; a mismatch fails the suite immediately.
- Every measured row now reports:
  - integrated Coverage area;
  - oriented projected Length;
  - oriented projected Width;
  - Coverage outside the authored oriented envelope;
  - support bounds and fully occupied row/column runs.
- Head measurements are strict for every recipe, including Broken Filament.
- Broken Filament full-body area may be lower than the filled rectangle because authored breakup can remove Coverage, but it may not exceed the envelope or dimensions.
- Object Arc and Semi-Arc total-union cases are explicitly non-accepting until contact and wake components can be isolated. Non-zero composite output is never promoted to PASS.
- CSV evidence includes replay identity and individual readback, adjacency, Length, Width, area, envelope, and replay verdicts.

### Scope

- `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.P7Diagnostics.cs`

### Non-goals

- No compute shader changes.
- No automatic-source geometry correction.
- No transport, rendering, scene, prefab, material, cache, or serialized-data change.
- No claim that Arc/Semi-Arc components are validated by their current total-union fixture.

## D8.4B — Shore/Inward production footprint repair

Implemented scope:

- Shore Ribbon width is now interpreted as an inward cell interval beginning at
  the authored bank inset, rather than a radius centred on the shoreline. This
  prevents the valid-fluid clip from discarding approximately half the source.
- Shore Ribbon selects Head Width whenever Head Length is shorter than Body
  Length, including at progress 1. Full-body cases continue to use Body Width.
- Inward Wash offsets its active centreline inward by half the active width so
  its complete footprint lies in valid fluid.
- Inward Wash uses one authoritative active width across the interval: Body
  Width for a full-body interval, Head Width for a progressive-head interval.
- Deterministic replay mismatches remain reported failures but no longer abort
  Smoke before the remaining cases execute.

Unchanged:

- Arc and Semi-Arc contact/wake geometry.
- Lace, Cross-Lace, Object Fleck and Broken Filament production evaluators.
- Source-event ABI, serialized controls, scenes, prefabs and materials.


## D8.4D1 — Audit readiness preflight and non-destructive cache-required handling

Status: implemented, Unity validation pending.

The Cell-Exact Smoke/Exhaustive actions must not start a suite when the production Foam runtime is already in the terminal `CachePreparationRequired` phase. That phase cannot resolve in Play Mode because production topology generation is intentionally disabled there. The Inspector now reports the exact corrective action instead of creating a zero-readback failure report:

`Actions → Foam Cache & Validation → Prepare / Rebuild Foam Topology Cache`

If the runtime transitions to `CachePreparationRequired` after a suite has begun waiting, the suite preserves a precondition/cancelled report and releases only audit-owned state. It no longer records the missing cache as a failed GPU footprint case. Other transient initialization phases continue waiting under the existing bounded readiness timeout; `InitializationPhase.Failed` and a real timeout remain failures.

D8.4D contact-head shader behavior is unchanged by this update.


## D8.5 — Cell-Exact Audit Stabilization and Closure

Status: implemented in source; Unity validation pending.

- Smoke replay cases now reuse the exact captured `AutomaticFoamSourceEvent`, packed GPU data, and dispatch range from cases 001–006.
- Replay compares the complete raw ARGBHalf payload. Raw mismatches are reported with differing-value count and maximum decoded delta, but do not redefine geometry acceptance.
- Support-projected length/width and the legacy centre-classified outside value are informational only. They no longer fail fractionally rasterized one-cell footprints.
- Solid recipes pass by integrated area. Broken Filament body cases require non-empty bounded area. Arc/Semi-Arc composite rows use three-component and two-component area contracts respectively; isolated component rows use one primitive area.
- Production compute-shader behavior is unchanged by D8.5.
- The existing Smoke/Exhaustive actions, copied report, and report folder remain the only user workflow.

## D8.6 — Arc/Semi-Arc Body Component Isolation

Status: implemented in source; static audit passed; Unity validation pending.

Objective:

- Resolve the five remaining D8.5 composite-body failures without changing production geometry speculatively.
- Measure Object Arc contact, negative wake, and positive wake bodies independently.
- Measure Object Semi-Arc contact and selected positive wake bodies independently.

Approved files:

- `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.P7Diagnostics.cs`
- `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`

Acceptance criteria:

- Existing 59 Smoke cases remain unchanged.
- Add 25 component-isolated body cases: five body scenarios for three Arc components and two Semi-Arc components.
- Every isolated body row uses the existing one-primitive integrated-area contract.
- No production compute shader, event ABI, source packing, scene, prefab, material, or serialized control changes.
- The existing Smoke action, copied report, and report-folder workflow remain the only user workflow.
- Suite remains incremental, cancellable, asynchronous, and Editor-responsive.

Implementation sequence:

1. Append component-isolated Object Arc body cases for scenarios 0–4 and component modes 1–3.
2. Append component-isolated Object Semi-Arc body cases for scenarios 0–4 and component modes 1 and 3.
3. Update the existing Smoke tooltip from 59 to 84 cases.
4. Audit final scope and static structure; Unity runtime validation remains pending.

Invariants and non-goals:

- Production shader/runtime geometry is unchanged.
- Existing progressive component rows 055–059 remain present.
- Composite rows remain informational evidence until isolated body results establish the correct union contract.
- No new Inspector action, secondary suite, or report file is added.

Risks:

- Smoke grows by 25 asynchronous readbacks. Expected duration remains only a few seconds based on the measured 59-case duration of 1.808 seconds.
- Component overlap cannot be inferred from isolated area alone; final composite acceptance may require replacing multiplication with an observed-union contract after evidence is collected.

D8.6 implementation audit:

- Final changed-file scope matches the approved three files.
- Smoke case count is 84: 48 base cases + 6 immutable replays + 5 isolated progressive heads + 25 isolated body components.
- The 25 new rows cover scenarios 0–4 for Arc contact/negative wake/positive wake and Semi-Arc contact/positive wake.
- Existing component modes and one-primitive isolated-area acceptance are reused; no shader/runtime production path changes were made.
- Existing Inspector workflow and report files are unchanged; only the Smoke tooltip count was updated.
- Static diff review completed. Unity C# compilation and Play Mode execution remain pending.

## D8.7 — Contact-Profile and Composite-Union Contract Repair (2026-07-28)

### Scope

Diagnostic closure only. Production Foam shaders, runtime source geometry, event packing, serialized controls, scenes, prefabs, and materials are unchanged.

### Evidence resolved

D8.6 isolated all Object Arc and Object Semi-Arc body components. Every wake-only body matched its direct cell-area contract exactly. Contact-only bodies formed a coherent curved profile rather than a rectangular ribbon:

- Object Arc contact is the full two-sided profile.
- Object Semi-Arc contact is the selected half-profile.
- Composite output is a union with dimension-dependent overlap, so `component count × rectangular primitive area` is not a valid expected area.

### D8.7 contract

The 84-case Smoke suite is reordered without adding cases:

1. Shore and Inward cases remain first so immutable replay references 001–006 remain stable.
2. Arc/Semi-Arc isolated body and head components run before their composite rows.
3. Remaining recipe rows run after the evidence required by union validation exists.

Acceptance now uses:

- wake-only components: direct integrated area;
- Semi-Arc contact body: bounded non-empty half-profile anchor;
- Arc contact body: approximately twice the matching Semi-Arc half-profile, with raster tolerance;
- Arc/Semi-Arc composites: mathematically valid measured union bounds, `max(component areas) <= union area <= sum(component areas)`, with a small half-float/raster tolerance;
- progressive isolated heads: unchanged direct one-primitive contract;
- Broken Filament: unchanged non-empty fragmented-envelope contract.

Support projection, legacy outside Coverage, and raw replay differences remain informational and do not override integrated production geometry.

### Expected closure result

The 11 D8.6 failures should resolve without any production geometry change. All 84 Smoke rows should pass if the measured component relationship and union bounds remain consistent.

### Next work items

- Run the 84-case Smoke suite once.
- If all rows pass, close the automatic-spawner cell-exact footprint work and retain raw replay mismatch as a documented diagnostic limitation.
- Do not reopen production geometry unless a component-specific integrated-area contract regresses.

## RIVER-FOAM-SPAWN-D8.8 — Shore/Inward Domain-Fixture Isolation

Status: implemented; Unity compilation and runtime validation pending.

The cell-exact geometry audit no longer evaluates Shore Ribbon and Inward Wash against mutable production river-domain masks. Their existing Smoke cases now bind audit-owned temporary resources to the unchanged production debug kernel:

- Boundary coverage: uniformly valid (`1`).
- Obstacle exclusion: uniformly absent (`0`).
- Current shore edges: fixed left edge far outside the fixture and right edge at lateral metre `0`.
- Grid metric rows: unchanged production fixed-metric descriptor/buffer, preserving the exact cell spacing used to pack the source event.

The synthetic resources are created per dispatch, cleared deterministically, retained through asynchronous GPU readback, and released with the rest of the capture. They cannot leak into normal runtime rendering. Report rows are marked `synthetic-domain-fixture`.

This separates source-shape geometry from production environmental clipping. No production compute shader, source evaluator, event ABI, serialized asset, scene, prefab, or material changed.

### Acceptance

Shore and Inward geometry rows continue to use their declared integrated cell-area contracts, but those contracts are now measured under deterministic valid-fluid conditions. Production-domain shoreline clipping remains a separate visual/runtime concern and is not used to decide whether the authored source primitive is cell-exact.

### Next work items

- Run the 84-case Smoke suite and confirm Shore/Inward return to approximately `1`, `5`, `3`, and `1` under `synthetic-domain-fixture`.
- Confirm Arc/Semi-Arc contact-profile and union contracts remain unchanged.
- If synthetic Shore/Inward still underfill, investigate only the source evaluator or fixed metric packing; do not attribute the result to production boundary/shore masks.

## RIVER-FOAM-SPAWN-D8.8A — Shore/Inward Synthetic Domain Coordinate Alignment

Status: validated and closed on 2026-07-28.

D8.8 bound a fixed synthetic right shore at lateral metre `0`, but retained the
production fixed-metric lattice and the production dispatch-range resolver. Shore
Ribbon and Inward Wash ranges are resolved around the geometric river bank, so
ordinary dispatched rows were not near the synthetic zero-metre shore and were
rejected by the debug kernel before source-shape evaluation. This produced the
observed deterministic zero area and zero support for all 18 Shore/Inward rows.

D8.8A keeps the repair diagnostic-only:

- Boundary coverage remains audit-owned and uniformly valid (`1`).
- Obstacle exclusion remains audit-owned and uniformly absent (`0`), now using
  the production-compatible `RHalf` field resource.
- Shore edges remain audit-owned, now using the production-compatible `RGHalf`
  one-row resource.
- Each synthetic shore column is populated from the stable geometric left/right
  shore values already stored in the production metric row. This aligns the
  synthetic shore with the unchanged metric lattice and dispatch range while
  excluding mutable current-shore mask state.
- A CPU spatial preflight verifies that the dispatched lateral range reaches the
  selected synthetic shore. A mismatch now records an explicit fixture failure
  instead of dispatching another set of meaningless zero-coverage cases.

No production compute shader, automatic-source evaluator, event ABI, serialized
asset, scene, prefab, material, Inspector action, or gameplay runtime path changed.

### Validation result

`CellExactSpawnerSmokeSuite(8)` completed all `84/84` asynchronous GPU
readbacks in `2.672 s` with `84 PASS`, `0 FAIL`. Shore Ribbon and Inward Wash
returned approximately `1`, `1`, `1`, `5`, `3`, and `1` integrated cells under
the aligned synthetic fixture. Arc/Semi-Arc profile and union rows, Fleck, Lace,
Cross-Lace, and Broken Filament all remained PASS. The D8.8 synthetic-domain
blocker and the automatic-spawner cell-exact production geometry contract are
therefore closed.

## RIVER-FOAM-SPAWN-D8.9 — Cell-Exact Audit Reporting Closure

Status: implemented in source; final Unity validation pending.

D8.9 changes diagnostic reporting only:

- Broken Filament body rows now print the actual non-empty bounded-envelope
  acceptance interval used by their pass logic, instead of displaying the solid
  primitive's incompatible generic lower bound.
- Replay rows label unequal raw half-float payloads as informational evidence.
  Raw equality remains available in TXT/CSV output, but does not override the
  integrated geometry result or contribute to suite pass/fail.
- The production compute shader, source evaluators, event ABI, runtime dispatch,
  serialized assets, scenes, prefabs, materials, and Inspector workflow remain
  unchanged.

### Final validation gate

Run the existing 84-case Smoke suite once. Required evidence:

- `84/84` asynchronous GPU readbacks complete;
- `84 PASS`, `0 FAIL`;
- Broken Filament rows show `fragmented-envelope-contract` with a lower bound of
  `0.001` as an exclusive lower bound and the correct bounded upper value;
- replay differences, if present, are labeled `raw-mismatch informational`;
- all integrated areas and recipe contracts remain unchanged from
  `CellExactSpawnerSmokeSuite(8)`.

## RIVER-FOAM-SPAWN-D8.11 — Dedicated Shore Ribbon Behavior Suite

Status: implemented; Unity validation pending.

D8.10 was reverted because its single-dispatch temporal rows did not measure the requested multi-tick persistent trail and the primary 1 cell/s case failed. The broad Cell-Exact Spawner Contract Audit is restored to its 84-case geometry/regression role.

A separate Play Mode suite now owns Shore Ribbon behavior validation. It does not mutate the visible persistent Foam field or serialized scene state. It uses audit-owned boundary, obstacle, shore, birth-debug, and persistent material textures while dispatching the production `RasterizeFoamSourceEventDebug` path repeatedly at the real 8 Hz material cadence.

The suite begins with a hard control-authority preflight:

- automatic Shore birth must be active;
- Object and Free-Water automatic birth must be inactive;
- the Shore recipe must resolve exclusively to Shore Ribbon;
- every currently active automatic source event must be Shore Ribbon.

It then runs deterministic right- and left-bank cases covering:

- 1 cell/s at 8-cell and 20-cell lengths;
- 2 cells/s at 8-cell and 20-cell lengths;
- both banks;
- one explicit delayed 3.5-cell material tick as stall robustness, not as a normal reveal speed.

Every production material tick is dispatched. At each crossed-cell checkpoint, asynchronous GPU readback measures the cumulative audit-owned packed material state and reports:

- resolved progress and completion boundary;
- integrated Coverage;
- integrated intrinsic Presence;
- integrated Remaining Life;
- longitudinal and lateral support bounds;
- longitudinal-to-lateral direction ratio;
- empty longitudinal columns inside the accumulated trail;
- monotonic head/trail progression.

Hard contracts:

1. Shore Ribbon length and head travel grow along field/global-distance X.
2. One-cell width remains lateral/across-shore.
3. No complete longitudinal cell column may be absent between the accumulated trail endpoints.
4. Coverage, Presence, and Remaining Life must all be written to the audit-owned persistent state.
5. The event reaches its resolved finite length and completion boundary.
6. The runner remains incremental, cancellable, Editor-responsive, and asynchronous.

The dedicated report is preserved as:

```text
Library/RiverFoam/ShoreRibbonBehaviorSuite.txt
Library/RiverFoam/ShoreRibbonBehaviorSuite.csv
```

No production source scheduler, source geometry, transport, lifecycle, scene, prefab, material, or serialized River value is changed by D8.11. The first report must identify whether the defect is already present in repeated production birth/merge or lies later in the visible runtime pipeline.


## RIVER-FOAM-SPAWN-D8.12 — Shore Ribbon Production-Pipeline Localization

Status: implemented; Unity validation pending.

D8.12 upgrades the dedicated Shore Ribbon Behavior Suite instead of changing production spawning behavior. The D8.11 report proved that Shore-only controls were authoritative, that ordinary 1–2 cells/s repeated births contained no complete longitudinal gaps, and that the delayed 3.5-cell tick did create a persistent gap. It did not independently execute lifecycle and transport, and its raw X/Y bounding-box direction ratio produced misleading failures on short curved footprints.

Every checkpoint now captures four audit-owned stages from the same accumulated Shore Ribbon state:

1. `BIRTH` — repeated production source raster accumulated through the current head progress.
2. `LIFECYCLE_ONLY` — one 8 Hz `SimulateFoam` step with transport delta zero.
3. `TRANSPORT_ONLY` — one 8 Hz `SimulateFoam` step with lifecycle delta zero.
4. `COMBINED` — one ordinary 8 Hz simulation step, matching the production simulation-before-next-birth order.

The simulation branches reuse the production metric buffer, topology, motion, routing, transport scheme, and lifecycle configuration, but bind audit-owned state, valid boundary, and zero-obstacle resources. They never swap or overwrite the visible persistent Foam textures.

For each stage, TXT and CSV now report:

- integrated Coverage, intrinsic Presence, and Remaining Life;
- support bounds and internal empty longitudinal columns;
- metric longitudinal span and metric lateral span;
- along/across direction ratio in metres rather than raw texture dimensions;
- Coverage, Presence, and Life retention relative to the birth snapshot;
- explicit stage pass/fail.

The hard localization gates are intentionally stage-specific:

- Birth must remain continuous, predominantly along-shore after the initial short footprint, material-bearing, and monotonic.
- Lifecycle-only must not erase Coverage or Presence in one 8 Hz step and must retain nonzero life.
- Transport-only must not create a complete internal gap and must retain at least 80% of integrated Presence in one step.
- Combined simulation must not create a complete internal gap and must retain at least 75% of integrated Presence in one step.

The delayed-tick case remains a robustness case rather than a normal reveal-speed case. The report-access controls remain `Copy Shore TXT + CSV` and `Open Shore Reports Folder`.

No production source scheduler, source geometry, lifecycle, transport, shader, scene, prefab, material, or serialized River value is changed by D8.12. The first D8.12 report is expected to localize the sparse live ribbon result to birth, lifecycle, transport, combined simulation, or a later visual-shape/final-render stage.

## RIVER-FOAM-SPAWN-D8.13 — Shore Ribbon Progressive Body Authority

Status: **rejected by live Play Mode evidence on 2026-07-30; superseded by D8.14; do not restore.** The cumulative source body produced broad overlapping Shore birth packs in Automatic Birth Sources instead of one moving 1×1 birth head. A contemporaneous Editor freeze is correlated with D8.13 but remains unverified without a crash log or GPU capture.

### Objective

Make a finite Shore Ribbon event authoritatively maintain its complete progressively revealed body while the event is forming. Coverage and Activity continue to schedule finite events; resolved Length defines the final along-shore body length; Reveal Speed defines reveal duration; Body Width and Head Width remain direct cell controls. After the final complete deposition, the event despawns and the resulting material returns to ordinary persistent Foam transport and lifecycle ownership.

### Approved files

- `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.P7Diagnostics.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.ShoreRibbonDiagnostics.cs`

No scene, prefab, material, serialized River value, scheduler, source-event ABI, event packing, lifecycle implementation, transport implementation, or Inspector authoring-control change is approved.

### Reviewed evidence

- `CS_RiverFoam.compute::FoamEvaluateShoreRibbonSource` currently evaluates only `[revealedEnd - HeadLength, revealedEnd]`; the persistent body is expected to arise solely from historical births.
- `StylizedRiverFoamRuntime.Injection.cs::SimulateFullField` transports and ages persistent Foam before the next automatic birth merge. Historical source cells therefore move and age independently while the source head continues along the authored path.
- `CS_RiverFoam.Simulation.hlsl::FoamMergeBornMaterial` adds only missing Coverage. Re-evaluating an already occupied revealed body does not reset Presence, Remaining Life, or Pattern; it returns the existing material when no Coverage is added.
- D8.12 reconstructed stationary accumulated birth at every checkpoint and applied only one isolated downstream step. It did not carry simulation output into the next tick and therefore could not validate recurrent production behavior.
- `StylizedRiverFoamRuntime.SourceUnits.cs::TryResolveAutomaticSourceDispatchRange` already covers the complete finite Shore Ribbon path and its lateral envelope. No dispatch-range expansion is required.

### Accepted design

1. Preserve the existing moving-head evaluator for isolated head diagnostics.
2. Add a Shore Ribbon cumulative revealed-shape evaluator whose union is:
   - completed body `[0, headStart]` at Body Width;
   - current head `[headStart, revealedEnd]` at Head Width.
3. Production Shore Ribbon raster permission evaluates the complete current revealed shape every tick. Other source recipes retain current-minus-previous permission.
4. Existing packet-independent material merge fills only Coverage missing from the active revealed shape; occupied material is not refreshed.
5. Component mode `4` is reserved for `Shore Ribbon Moving Head Only` in the cell-exact audit so its `Head=1x1 on 5x3 Body` contract remains isolated.
6. Replace D8.12 checkpoint reconstruction with four recurrent audit-owned timelines: Birth Only, Lifecycle Only, Transport Only, and Combined Production. Each lane commits its previous output as the next tick's input, and simulation precedes cumulative birth exactly as in production.
7. Use the active material update rate rather than a hard-coded 8 Hz assumption. The delayed case advances source progress by 3.5 cells in one audit tick while retaining one ordinary material simulation step; it is source-stall robustness, not a production delta-time claim.

### Invariants and non-goals

- Shore Ribbon remains a finite event and despawns on the existing completion boundary.
- Coverage, Activity, Length Min/Max, Reveal Speed, body width, head length, head width, and bank offset retain their current meanings and serialization.
- No permanent source, walker population, separate production field, source velocity compensation, lifetime retuning, or shore-wide transport suppression is introduced.
- Inward Wash, Object Foam, and Free-Water recipes retain their existing evaluators and deposition permission.
- The production event record and GPU ABI remain unchanged.
- The visible persistent Foam field is never read from or written by the dedicated audit.
- Diagnostics remain incremental, cancellable, asynchronous, progress-visible, and partial-report preserving.

### File-by-file implementation sequence

1. `CS_RiverFoam.compute`
   - add the cumulative Shore Ribbon revealed-shape evaluator;
   - route production Shore Ribbon evaluation to it;
   - retain moving-head-only evaluation under debug component mode `4`;
   - bypass current-minus-previous permission only for cumulative Shore Ribbon production/development mode.
2. `StylizedRiverFoamRuntime.P7Diagnostics.cs`
   - assign component mode `4` to Shore Ribbon head-only cases in Smoke, replay, and Exhaustive construction;
   - label the mode explicitly in reports.
3. `StylizedRiverFoamRuntime.ShoreRibbonDiagnostics.cs`
   - replace snapshot branches with recurrent lane state and ping-pong textures;
   - simulate each lane before birth using production transport substeps and lane-owned Bulk-Phase state;
   - deposit the same cumulative event into all recurrent lanes;
   - capture only selected checkpoints through asynchronous GPU readback;
   - report continuous-run length, metric spans, and recurrent stage results.
4. This document
   - record final diff, static checks, Unity validation state, and any deviation.

### Acceptance criteria

- Generic Cell-Exact Smoke remains `84/84 PASS`; the Shore Ribbon `Head=1x1 on 5x3 Body` row still measures the isolated 1×1 moving head.
- At normal 1 and 2 cells/s cases on both banks, every recurrent lane contains a continuous occupied run covering the progressively revealed authored path within one-cell raster tolerance.
- At event completion, the Birth Only and Combined Production lanes contain a continuous run covering the resolved 8- or 20-cell length within one-cell tolerance.
- The delayed 3.5-cell source-progress case contains no missing internal source-path segment.
- Event elapsed time reaches the existing duration exactly once; no extra source tick occurs after completion.
- Non-Shore source evaluators and deposition permission remain unchanged.
- No diagnostic resource aliases the visible persistent state.

### Performance budget

Production adds no dispatch, buffer, texture, CPU event iteration, or persistent allocation. The existing Shore Ribbon event envelope is already dispatched each material tick. More cells inside that envelope can reach `FoamMergeBornMaterial`, but already satisfied Coverage exits through the existing no-added-Coverage branch. The expected cost scales with active Shore Ribbon revealed area and is small relative to full-field transport. Unity GPU profiling remains pending.

The recurrent audit uses three simulation ping-pong lane pairs plus one Birth Only state and reads back only checkpoints. It removes D8.12's per-checkpoint branch reconstruction and advances every timeline once per material tick.

### Implementation result

- `CS_RiverFoam.compute` now evaluates Shore Ribbon production births as the union of the completed revealed body and current moving head. Debug component mode `4` retains the previous moving-head-only evaluator.
- Cumulative Shore Ribbon birth permission bypasses current-minus-previous only for Shore Ribbon production/development mode. `Inward Wash`, Object, and Free-Water evaluators and permission remain unchanged.
- `StylizedRiverFoamRuntime.P7Diagnostics.cs` assigns `Moving Head Only` mode to the Shore 1×1 head case in Smoke, replay, and Exhaustive construction. Static review caught and corrected an initial replay-mode omission before packaging.
- `StylizedRiverFoamRuntime.ShoreRibbonDiagnostics.cs` now advances four audit-owned histories recurrently. Lifecycle, transport, and combined lanes commit their output before the next cumulative birth, using the active material cadence, production transport-substep count, and lane-owned Bulk-Phase state.
- D8.12 snapshot reconstruction and `Graphics.CopyTexture` stage branching were removed. Readback remains asynchronous and occurs only at selected checkpoints.
- The dedicated fixture still uses deterministic synthetic Shore geometry with the live runtime topology/motion resources. It validates recurrent production contracts without mutating visible Foam, but it is not a captured replay of one naturally scheduled live event.

### Final source scope

Modified exactly:

- `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.P7Diagnostics.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.ShoreRibbonDiagnostics.cs`

No scene, prefab, material, serialized River value, scheduler, source-event ABI, event packing, lifecycle implementation, transport implementation, or Inspector authoring-control file changed.

### Validation and compliance status

- Gate 1 review: complete for the Shore evaluator, raster permission, packet-independent merge, event packing/progression, dispatch range, production simulation order, shared simulation bindings, generic footprint audit, dedicated Shore suite, and canonical handoff evidence.
- Gate 2 plan: recorded here before code or shader modification.
- C#/shader implementation: complete in source.
- Static changed-file reconciliation: passed; exactly the four approved paths differ.
- Static delimiter/symbol/contract checks: passed.
- Static cumulative-interval model: passed at 8/12/16 Hz for 8-cell and 20-cell ribbons at 1 and 2 cells/s, including the 3.5-cell delayed-progress case; no internal longitudinal interval gap was produced.
- Generic suite construction audit: passed; Smoke remains 84 cases and Exhaustive remains 672 cases; Shore head replay preserves component mode `4`.
- Shared-shader impact audit: passed statically; cumulative authority is gated to source type `ShoreRibbon`, and every other recipe remains on the previous permission path.
- Unity 6000.5.0f1 C#/shader compilation: pending in the user project because Unity and its shader compiler are unavailable in the archive environment.
- Cell-Exact Smoke Suite: pending.
- Recurrent Shore Ribbon Behavior Suite: pending.
- Live Play Mode visual validation: pending.
- GPU profiling of the larger maintained Shore birth area: pending.

## RIVER-FOAM-SPAWN-D8.14 — Discrete 1×1 Shore Ribbon Head Traversal

Status: source implementation complete; static scope/contract audit passed; Unity 6000.5.0f1 compilation, shader import, and Play Mode validation pending. The existing `StylizedRiverEditor.Actions.cs` help text remains stale because that ninth file was explicitly outside the approved scope.

### Objective

Remove D8.13 progressive-body authority and restore Shore Ribbon birth as a finite sequence of discrete source cells. Each newly entered path cell is born exactly once from one selected longitudinal grid column and one selected lateral grid row. A normal material tick enters zero or one new path cell at the authored 1–2 cells/s speeds; a delayed tick may enter several cells, but each remains an independent 1×1 birth cell. No Shore Ribbon body is rewritten, and no transport, lifecycle, or final-render behavior is modified.

### Approved files

- `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
- `Assets/Game/Procedural/Rivers/StylizedRiver.cs`
- `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs`
- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.P7Diagnostics.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.ShoreRibbonDiagnostics.cs`

Explicitly outside scope:

- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.SourceUnits.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.RuntimeUpdates.cs`
- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Simulation.hlsl`
- `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`
- every scene, prefab, material, `.asset`, `.meta`, layer, tag, and serialized scene value.

`StylizedRiverEditor.Actions.cs` contains D8.13-era suite help text describing lifecycle/transport/combined branches. It is outside the approved file set and will not be modified. Final compliance must mark that Inspector copy as pending rather than silently expanding scope.

### Reviewed evidence

- Repository instructions: `Assets/AGENTS.md`, read completely before review and implementation.
- Continuation evidence: `/mnt/data/River_Foam_Shore_Ribbon_Spawning_Continuation_Handoff(1).md`, especially the finite-event contract, cell-exact requirement, diagnostic responsiveness requirement, and prohibition on scene modification.
- D8.13 source state: `Assets-Code-Archive(44).zip` plus `RIVER-FOAM-SPAWN-D8.13_Shore_Ribbon_Progressive_Body_Authority.zip`; D8.13 changed-file package SHA-256 `e4e5c9a59401b8424c9f0d669b9c38d81119b6f9ad1f34160c850ebb0e1a19ab`.
- `CS_RiverFoam.compute::FoamEvaluateShoreRibbonRevealedSource` emits `[event start, current head]` and `EvaluateFoamAutomaticSourceRasterSample` bypasses current-minus-previous permission for that entire region. This is the direct source of D8.13 wide, overlapping birth packs.
- `StylizedRiverFoamRuntime.Injection.cs::DispatchAutomaticFoamSourceEvents` currently dispatches every active Shore Ribbon whenever floating progress advances, even when no new integer path cell is entered.
- `StylizedRiverFoamRuntime.BirthEvents.cs::TryBeginAutomaticShoreSourceEvent` resolves fractional Shore length/width/head controls and clamps each endpoint independently, so the final path can be fractional and shorter than the authored cell count near a domain edge.
- `CS_RiverFoam.compute::FoamEvaluateShoreRibbonSource` uses interval overlap for both axes. A nominal one-cell interval can cover two texels when positioned between grid centres; it does not enforce a single birth texel.
- `StylizedRiverFoamRuntime.SourceUnits.cs::TryResolveAutomaticSourceDispatchRange` already covers the complete Shore path and lateral envelope, so no dispatch-range change is required.
- `StylizedRiverFoamRuntime.State.cs::FoamSourceEventGpuData` already carries current and previous progress plus D8 cell counts in the existing fixed ABI. No ABI expansion is required.
- `StylizedRiverFoamRuntime.P7Diagnostics.cs` and `StylizedRiverFoamRuntime.ShoreRibbonDiagnostics.cs` already provide asynchronous audit-owned readback and resource-lifetime infrastructure. The dedicated suite can be reduced to current-tick birth and accumulated birth without synchronous GPU access.

### Mathematical contract

For integer path length `N`, speed `v` cells/s, and event time `t`:

```text
d(t) = min(N, v t)
R(t) = 0                                      when d(t) <= 0
R(t) = clamp(ceil(d(t) - 1e-5), 0, N)        otherwise
```

For one update from `t0` to `t1`, birth exactly the integer path-cell indices:

```text
k in [R(t0), R(t1))
```

At the intended minimum cadence and maximum normal reveal speed:

```text
v <= 2 cells/s
f >= 8 Hz
Delta d = v/f <= 0.25 cell/tick
```

Therefore each normal tick enters at most one new cell. A delayed jump remains legal because the shader evaluates every integer `k` in the newly entered half-open interval without converting the interval into one long body rectangle.

### Accepted implementation

1. `StylizedRiver.cs`
   - remove the serialized D8 Shore Ribbon width-min, width-max, head-length, and head-width fields and their accessors/sanitization;
   - keep Shore length, bank offset, reveal speed, Initial Presence, and Initial Life;
   - leave legacy metre-era fields untouched because they are outside the current authoritative cell-control surface and are not required by D8.14.
2. `StylizedRiverEditor.Foam.cs`
   - remove Width, Head Length, and Head Width controls from Shore Ribbon Pattern;
   - show one read-only `Birth Head: Fixed 1 × 1 cell` row;
   - keep Segment Length, Bank Offset, Reveal Speed, Initial Presence, and Initial Life.
3. `StylizedRiverFoamRuntime.BirthEvents.cs`
   - resolve Shore length to an integer cell count;
   - shift the whole segment inside the domain instead of independently clipping its endpoints;
   - fix Shore body width, head length, and head width to one cell in event state;
   - preserve Inward Wash construction unchanged.
4. `StylizedRiverFoamRuntime.Injection.cs`
   - add one CPU integer revealed-count helper matching the shader equation;
   - dispatch Shore Ribbon only when the revealed integer count increases;
   - retain all non-Shore dispatch gating unchanged.
5. `CS_RiverFoam.compute`
   - remove `FoamEvaluateShoreRibbonRevealedSource` and D8.13 cumulative permission;
   - add a discrete Shore Ribbon birth evaluator;
   - derive the authoritative path-cell spacing from event start/end and integer cell count;
   - for each newly entered path-cell centre, choose exactly one nearest longitudinal column and exactly one nearest lateral row with deterministic tie-breaking;
   - return binary `1` only for those winning texels;
   - preserve Inward Wash, Object, and Free-Water evaluation and permission byte-for-byte outside the Shore branch.
6. `StylizedRiverFoamRuntime.P7Diagnostics.cs`
   - update Shore scenarios and reporting to the fixed 1×1 discrete-head contract;
   - keep Smoke and Exhaustive suite counts unchanged;
   - retain audit-owned asynchronous GPU readback.
7. `StylizedRiverFoamRuntime.ShoreRibbonDiagnostics.cs`
   - remove lifecycle, transport, and combined simulation lanes;
   - maintain `CURRENT_TICK_SOURCE` and `ACCUMULATED_BIRTH` audit-owned textures only;
   - clear current-tick source, rasterize one production Shore event, and merge the same birth into accumulated state;
   - report new integer cell count, current-tick occupied cells, maximum occupied rows per source column, accumulated occupied columns, internal gaps, completion count, and post-completion dispatch count;
   - dispatch no transport or lifecycle kernel.
8. This document
   - record actual diff, static checks, deviations, Unity validation state, and pending Inspector-copy correction.

### Invariants and non-goals

- Shore Ribbon source birth is fixed at one longitudinal cell by one lateral cell per path-cell identity.
- A Shore Ribbon event emits each integer path-cell identity once and only once.
- Delayed progress emits multiple independent 1×1 cells in one dispatch; it never emits a body rectangle.
- Coverage, Activity, Length Min/Max, Bank Offset, Reveal Speed, Initial Presence, and Initial Life retain authority.
- Inward Wash, Object, and Free-Water source construction, packing, dispatch gating, shader evaluation, and diagnostics remain unchanged unless a shared test label must distinguish the fixed Shore contract.
- No production transport, lifecycle, support, merge, presentation, final visibility, or rendering code changes.
- No scene or serialized asset edit.
- No synchronous GPU readback, blocking wait, or automatic diagnostic run.

### Acceptance criteria

- D8.13 cumulative revealed-body evaluator and production permission branch are absent.
- A normal Shore birth dispatch contains zero or one occupied texel per event.
- Every occupied Shore birth column contains at most one occupied row.
- A delayed 3.5-cell jump emits the exact integer count `R(t1)-R(t0)` with no internal path-cell gap and at most one occupied row per emitted column.
- At event completion, accumulated birth contains exactly `N` occupied path columns, zero internal gaps, and no additional source dispatch after completion.
- Shore width/head authoring fields are absent from source and Inspector; the Inspector states `Fixed 1 × 1 cell`.
- Inward Wash, Object, and Free-Water source branches remain unchanged in the final shared-shader diff.
- Cell-Exact Smoke still constructs 84 cases and Exhaustive still constructs 672 cases.
- Diagnostics remain incremental, cancellable, asynchronous, progress-visible, and partial-report preserving.

### Performance budget

For a length-`N` event, D8.13 repeatedly evaluated/wrote an average revealed length near `N/2` over approximately `N f / v` material ticks:

```text
W_D8.13 approximately N^2 f / (2v)
```

D8.14 emits each path cell once:

```text
W_D8.14 = N
```

For `N=20`, `f=8 Hz`, `v=1 cell/s`:

```text
W_D8.13 approximately 1600 source-cell writes
W_D8.14 = 20 source-cell writes
reduction = 80x
```

D8.14 also skips Shore dispatches on ticks where the integer revealed count does not advance. No new production texture, buffer, dispatch type, persistent allocation, per-frame full-field pass, or GPU ABI lane is introduced.

### Validation plan

1. Static scope/diff audit against the exact D8.13 source state.
2. C# and HLSL delimiter, symbol, removed-field-reference, and shared-branch checks.
3. Mathematical revealed-count tests at 8/12/16 Hz, 1/2 cells/s, 8/20/47 cells, both directions, domain-edge placement, and a delayed 3.5-cell jump.
4. Generic suite construction count and Shore fixed-head scenario audit.
5. Unity 6000.5.0f1 compilation and shader import in the user project.
6. Cell-Exact Smoke Suite, dedicated Shore Ribbon birth suite, and Shore-only Play Mode visual validation.

### Implementation result

- `StylizedRiver.cs` removes the authoritative Shore Ribbon Width Min/Max, Head Length, and Head Width serialized controls and accessors. Length, Bank Offset, Reveal Speed, Initial Presence, and Initial Life remain authoritative.
- `StylizedRiverEditor.Foam.cs` replaces those removed controls with `Birth Head: Fixed 1 × 1 cell`.
- `StylizedRiverFoamRuntime.BirthEvents.cs` resolves Shore length to an integer cell count, shifts the complete segment inside the domain without shortening it, and fixes Shore body/head width and head length to one cell. Inward Wash follows its pre-D8.14 construction path.
- `StylizedRiverFoamRuntime.Injection.cs` dispatches a Shore event only when `R(current)-R(previous) > 0`; all other recipe gates retain their previous phase/progress logic.
- `CS_RiverFoam.compute` removes D8.13 cumulative-body authority. Shore birth now selects exactly one nearest longitudinal column and one nearest lateral row for each newly entered integer path-cell centre. The current dispatch may contain several independent cells after a delayed progress jump, but no body rectangle is evaluated.
- `StylizedRiverFoamRuntime.P7Diagnostics.cs` retains `84` Smoke and `672` Exhaustive cases while treating every Shore scenario as fixed one-cell lateral birth. D8.13 debug component mode `4` is removed.
- `StylizedRiverFoamRuntime.ShoreRibbonDiagnostics.cs` is birth-only. It owns `CURRENT_TICK_SOURCE` and `ACCUMULATED_BIRTH`, dispatches no lifecycle or transport kernel, uses asynchronous readback, and validates occupied cell count, occupied columns, maximum rows per column, internal gaps, completion, and zero post-completion birth.

### Final changed-file reconciliation

Modified exactly the eight approved files:

- `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
- `Assets/Game/Procedural/Rivers/StylizedRiver.cs`
- `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs`
- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.P7Diagnostics.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.ShoreRibbonDiagnostics.cs`

No scene, prefab, material, `.asset`, `.meta`, layer, tag, transport source, lifecycle source, final-render source, or GPU ABI definition changed.

### Validation and compliance status

- Gate 1 review: complete for repository instructions, canonical River Foam plan, serialized controls, Inspector mapping, Shore event construction, event packing/progression, dispatch gating/range, shared GPU event ABI, shared source raster, generic footprint audit, dedicated suite, lifecycle caller, and Inspector action caller.
- Gate 2 canonical plan: recorded before code or shader modification.
- Gate 3 implementation: complete within the approved eight-file scope.
- Gate 4 final source/diff audit: passed for scope, delimiter balance, removed-symbol references, D8.13 symbol removal, birth-only diagnostic ownership, suite cardinality, and shared-shader Shore-only routing.
- Static revealed-count model: passed at `8/12/16 Hz`, `1/2 cells/s`, lengths `8/20/47`, both flow directions, and a delayed `3.5-cell` jump. Normal ticks enter at most one cell; the delayed case emits `[1,4,1,1,1]` cells across its birth dispatches and totals exactly eight.
- Static domain placement model: passed for both flow directions and domain lengths `1/8/20/47/64` cells; segment endpoints remain inside the domain and retain exactly the integer resolved length.
- Static fixed-lattice ownership model: passed for integer, half-cell, and fractional source offsets; every path-cell centre has one deterministic winning grid column.
- Generic suite construction: passed statically; Smoke remains `84` and Exhaustive remains `672`.
- Unity 6000.5.0f1 C# compilation and compute-shader import: pending because Unity is unavailable in the archive environment.
- Cell-Exact Smoke Suite, Shore Ribbon discrete birth suite, live Automatic Birth Sources validation, and freeze regression check: pending in the user project.
- Inspector consistency limitation: `StylizedRiverEditor.Actions.cs` still describes D8.13 lifecycle/transport/combined branches. Correcting that text requires explicit approval to modify the currently excluded ninth file; runtime behavior and report contents do not depend on the stale text.

### Current gate status

- Gate 1 review: complete.
- Gate 2 canonical plan: complete.
- Gate 3 implementation: complete in source.
- Gate 4 static audit: complete; Unity validation and the explicitly excluded Inspector-copy correction remain pending.

## RIVER-FOAM-SPAWN-D8.15 — Persistent Shore-Following Heads and Length-Scaled Shore Scheduling

Status: **implementation authorized; Gate 1 review complete; Gate 2 plan recorded before code modification; implementation in progress.**

### Objective

Replace D8.14's global fixed-rate Shore scheduler and intermittent source-head dispatch with a scalable per-bank-bucket scheduler and persistent Shore Ribbon head semantics. Every Ribbon event selects one start cell against the current visible shore, resolves one whole-cell effective length from the existing user-facing Min/Max cell controls, keeps one logical 1x1 head alive for the complete event, advances that head in flow order along the existing `_FoamCurrentShoreEdgesRead` contour, births each newly traversed cell once, and terminates after the final path cell has occupied its complete cell-duration. Multiple buckets may own events concurrently; event population scales with valid shoreline length and active chunks.

This patch removes Shore Coverage for the entire Shore source family. Activity and Minimum Packet Gap become the complete Shore population controls. No user-facing `Resolved Length` control is added: resolved/effective length is event-local runtime state selected inclusively from the existing Min/Max controls.

### Approved files

Canonical documents:

- `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
- `Assets/Docs/River_Foam_Stage6_Architecture.md`
- `Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
- `Assets/Docs/River_Rendering_Roadmap.md`

Runtime and authoring:

- `Assets/Game/Procedural/Rivers/StylizedRiver.cs`
- `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.State.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.SourceUnits.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.P12Sweep.cs`
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.ShoreRibbonDiagnostics.cs`
- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute`

Explicitly outside scope:

- every transport kernel and transport control;
- lifecycle mathematics and material-state merge semantics;
- final Foam rendering and visibility;
- scenes, prefabs, materials, `.asset`, `.meta`, layers, and tags;
- river geometry/domain generation;
- `_FoamCurrentShoreEdgesRead` allocation or `BuildCurrentShoreEdges`;
- boundary, metric, topology, obstacle-routing, and motion-lane generation;
- Object and Free-Water scheduling or source geometry;
- Inward Wash shape evaluation beyond shared Shore scheduling, whole-cell along-length selection, and fixed shore-touching start offset;
- GPU event ABI expansion;
- new path buffers, path textures, shoreline readback, or shoreline-build kernels;
- `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Actions.cs`.

### Gate 1 reviewed evidence

- Repository rules: `Assets/AGENTS.md`, read completely from the reconstructed D8.14 source before review.
- Source base: `Assets-Code-Archive(44).zip` plus accepted D8.13 and D8.14 changed-file packages. Baseline hashes are recorded in `/mnt/data/d815out/baseline_hashes.txt`.
- `StylizedRiver.cs`: Shore Coverage is serialized as `foamShoreFoamCoverage`; Ribbon and Inward Wash expose cell-authoritative offset controls; Ribbon and Inward Wash retain user-facing Min/Max cell lengths.
- `StylizedRiverEditor.Foam.cs::DrawFoamAutomaticSourcePopulationSection`: the Inspector currently presents Coverage and Shore start-offset controls and describes the superseded permanent-slot model.
- `StylizedRiverFoamRuntime.BirthEvents.cs::AutomaticShoreSourceProfile`: Coverage and a global `5 * Activity` events-per-second rate are scheduler authorities.
- `StylizedRiverFoamRuntime.BirthEvents.cs::AdvanceAutomaticShoreBirthSources`: one global accumulator gives equal attempt rates to short and long rivers.
- `StylizedRiverFoamRuntime.BirthEvents.cs::TryStartAutomaticShoreSourceEvent`: the existing 3.5 m per-bank lattice and deterministic cursor already provide bounded scalable scheduling buckets, but Coverage permanently masks buckets and per-bucket rearm stores only one float timestamp.
- `StylizedRiverFoamRuntime.BirthEvents.cs::TryBeginAutomaticShoreSourceEvent`: D8.14 samples fractional Min/Max values with `Lerp`; Ribbon treats the selected candidate as the whole-segment centre; Ribbon and Inward Wash use authored shore offsets.
- `StylizedRiverFoamRuntime.Injection.cs::DispatchAutomaticFoamSourceEvents`: D8.14 dispatches Ribbon only when the integer revealed count changes, producing one-tick source-debug flashes at 1–2 cells/s under the live 12 Hz material cadence.
- `CS_RiverFoam.compute::FoamEvaluateDiscreteShoreRibbonBirth`: the existing evaluator already reads `_FoamCurrentShoreEdgesRead` independently at every selected longitudinal column and selects exactly one nearest inward lateral row. The current shore-edge texture is therefore the complete implicit Ribbon path; no path calculation or resource is missing.
- `StylizedRiverFoamRuntime.SourceUnits.cs::TryResolveAutomaticSourceDispatchRange`: D8.14 still dispatches the complete event envelope even when only one head/catch-up column is needed.
- `StylizedRiverFoamRuntime.State.cs::AutomaticFoamSourceEvent` and `FoamSourceEventGpuData`: CPU event state may add bucket ownership without changing the fixed eight-`float4` GPU ABI.
- `StylizedRiverFoamRuntime.Members.cs` and `Resources.cs`: the event pool and GPU buffer are fixed at 32 entries; a 100 m river owns 58 Shore buckets before Object/Free-Water allowance, so fixed capacity silently breaks length scaling.
- `ShoreRibbonBehaviorSuite(2).txt`: live preflight observed 13 concurrent Ribbon events, proving the global fixed-rate scheduler populated many independent heads. The suite's synthetic birth path itself remained one cell per column, proving geometry and scheduler/lifetime were separate defects.
- `CellExactSpawnerSmokeSuite(11).txt`: the broad suite returned 83/84, with a Shore diagonal replay failure; it is not accepted as current completion evidence.

### Accepted scheduling contract

For valid river length `L` and fixed internal bucket spacing `S = 3.5 m`:

```text
B = max(1, ceil(L / S))             // buckets per bank
M = 2B                              // both banks
```

All `M` buckets are always eligible. Coverage is absent. Bucket `i` owns the interval:

```text
bucketStart = domainMinimum + i * S
bucketEnd   = min(bucketStart + S, domainMinimum + validFieldLength)
```

Each bucket keeps:

```text
Initialized
CycleIndex
NextStartTime
ActiveEventId
```

The bucket's cycle seed chooses its recipe, whole-cell effective dimensions, and a deterministic jittered start inside the geometrically valid portion of its interval. Distinct buckets may own events concurrently. One bucket may not own overlapping events.

For actual event duration `D`, Activity `A`, and distance-derived packet-clearance time `G`:

```text
ActivityIdle(D,A) = +infinity                 when A <= 0
ActivityIdle(D,A) = D * (1-A) / A             when 0 < A < 1
ActivityIdle(D,A) = 0                         when A >= 1
NextStartTime     = startTime + D + max(ActivityIdle, G)
```

Activity therefore controls each bucket's target active-time fraction; Minimum Packet Gap remains the hard physical clearance authority. Startup uses deterministic per-bucket phase staggering across one representative activity/clearance cycle. Scan work remains bounded to 32 buckets and 3 successful starts per material update.

### Effective-length contract

No user-facing `Resolved Length` field exists. Ribbon keeps `Length Min/Max (cells)`; Inward Wash keeps `Along-Bank Length Min/Max (cells)`. Event creation resolves an inclusive whole-cell count:

```text
minimum = max(1, round(authoredMinimum))
maximum = max(minimum, round(authoredMaximum))
resolved = minimum + floor(hash01 * (maximum-minimum+1))
```

The selected value is clamped only to the largest whole-cell path that fits the selected candidate without moving the candidate or shortening below authored Min. If Min cannot fit at that candidate, the cycle advances to another deterministic candidate.

### Ribbon event/path contract

The selected candidate is the centre of path cell zero. For flow sign `q`, longitudinal cell size `dx`, and effective length `N`:

```text
startBoundary = candidateCentre - q * 0.5 * dx
endBoundary   = startBoundary + q * N * dx
```

Head index at elapsed time `t`, reveal speed `v` cells/s, and event duration `N/v`:

```text
head(t) = clamp(floor(v*t + 1e-5), 0, N-1)
```

Equivalently from normalized progress `p`:

```text
head(p) = clamp(floor(p*N + 1e-5), 0, N-1)
```

For each head index `k`, the longitudinal column is the deterministic nearest grid column to the centre of cell `k`. At that column, the existing `_FoamCurrentShoreEdgesRead` provides the selected bank's current visible edge, and the head row is the nearest valid fluid row half one lateral cell inward. The implicit path is therefore:

```text
P[k] = (nearestColumn(k), nearestInwardRow(currentShoreEdge[nearestColumn(k)]))
```

The path follows river curvature through the existing river-distance coordinate and follows local bank waviness laterally through the existing shore-edge texture. No path is generated or stored.

### Birth/debug contract

The same event remains active through all `N` head cells. Material birth and source visualization are separate:

```text
debugCoverage = current head cell every material tick
birthCoverage = every newly entered path cell exactly once
```

Normal same-cell tick:

```text
previousHead == currentHead
birth: none
debug: current head remains visible
```

Delayed tick:

```text
previousHead < currentHead
birth: each P[previousHead+1 ... currentHead]
debug: P[currentHead]
```

Normal gameplay dispatches Ribbon only when a new path cell is entered. `Automatic Birth Sources` debug mode dispatches every material tick so the active head remains continuously visible. The dispatch range is reduced to the current head and any catch-up columns, with the full lateral row range; the GPU's existing shore-edge lookup selects the single row.

### Capacity contract

The former fixed 32-event pool becomes descriptor-derived at resource initialization:

```text
shoreBucketCount   = 2 * max(1, ceil(validFieldLength / 3.5))
eventCapacity      = 32 + shoreBucketCount
reservationCapacity = 2 * eventCapacity
```

The existing 32-entry allowance remains for non-Shore sources. CPU arrays and the existing structured GPU buffer are allocated once per resource build. No per-frame or per-event allocation is permitted.

For `L = 100 m`:

```text
shoreBucketCount = 58
eventCapacity = 90
GPU event data = 90 * 128 B = 11,520 B
```

### File-by-file implementation sequence

1. Update this canonical plan first.
2. Remove Shore Coverage and Ribbon/Inward shore-start offset serialized properties and Inspector controls; retain Min/Max length controls.
3. Replace fixed capacity constants and static arrays with descriptor-derived capacities allocated during resource initialization.
4. Add per-bucket schedule state and CPU-only bucket ownership on Shore events.
5. Replace the global accumulator scheduler with bounded all-bucket renewal scheduling, deterministic startup staggering, inclusive whole-cell length resolution, and geometrically valid candidate intervals.
6. Make Ribbon candidate position path-cell zero rather than the segment centre; force zero shore offset for both Shore recipes.
7. Split Ribbon birth/debug coverage in the compute shader and keep the current head continuously visible only when source debug is active.
8. Restrict Ribbon dispatch to current/catch-up columns and keep all non-Shore dispatch paths unchanged.
9. Remove obsolete accumulator resets, update the Shore report's control-authority text, and update canonical architecture documents.
10. Perform final full-surface diff, shared-shader, scope, allocation, formula, and symbol audits; record Unity-only validation as pending.

### Acceptance criteria

- Shore Coverage and both Shore recipe start-offset controls are absent from runtime authority and Inspector UI.
- Ribbon and Inward Min/Max cell lengths remain user-facing; event-local effective lengths are inclusive whole integers.
- Number of Shore scheduling buckets scales as `2 * ceil(validFieldLength / 3.5 m)` and every bucket remains eligible.
- Activity controls per-bucket renewal duty cycle; Minimum Packet Gap remains a hard clearance lower bound.
- Multiple buckets may own events concurrently; one bucket never owns overlapping events.
- A Ribbon candidate is the first head-cell centre, not the segment centre.
- Each Ribbon event retains one identity from start through completion and its current 1x1 head remains continuously visible in Automatic Birth Sources.
- Each newly traversed Ribbon path cell is born once; no cumulative body is emitted.
- Every head cell samples the existing current visible shore edge at its own longitudinal column and selects one nearest inward row.
- No path resource, path solve, shoreline readback, transport, lifecycle, final-render, scene, prefab, material, or GPU ABI change occurs.
- Event/buffer capacity scales with valid river length and is allocated only during resource build.
- Object, Free-Water, and Inward Wash shape-evaluator behavior remains unchanged.

### Performance budget

Normal Ribbon birth work remains approximately `v` tiny dispatches per active event per second, where `v` is the authored 1–2 cells/s. Source-debug mode adds one tiny current-head dispatch per active Ribbon per material tick only while that explicit debug view is selected. The Ribbon dispatch range contains only current/catch-up columns by the full existing lateral row count. New normal-runtime shoreline work, textures, buffers, kernels, passes, and readbacks: zero. Scheduler work is bounded CPU dictionary lookup/hash/arithmetic over at most 32 scanned buckets and 3 successful starts per material tick.

### Validation plan

Offline/static:

1. Exact changed-file reconciliation against the approved list and D8.14 baseline hashes.
2. C# and HLSL delimiter/preprocessor/symbol checks.
3. Removal-reference checks for Shore Coverage, Ribbon offset, Inward offset, global Shore accumulator, and fixed Shore event-rate constant.
4. Mathematical scheduler simulations across 5/32/100 m lengths, Activity 0/0.25/0.5/1, both flow directions, Min/Max length ranges, startup staggering, and bounded scan/start budgets.
5. Head progression and catch-up simulations at 8/12/16 Hz, 1/2 cells/s, lengths 8/20/47, including delayed updates and final-cell dwell.
6. Capacity calculations and buffer/array size consistency.
7. Shared compute-shader audit proving non-Shore birth paths are unchanged.

Unity pending:

- Unity 6000.5.0f1 compilation and compute-shader import.
- Live Shore-only Play Mode observation before any further broad diagnostic suite.
- Automatic Birth Sources evidence: multiple length-scaled heads may coexist, but each head remains stable, moves cell by cell along its local visible bank, and terminates once.
- Final Foam evidence that the resulting births form Shore strokes; transport/lifecycle interpretation remains explicitly outside this patch.

### D8.15 implementation and Gate 4 audit status

Status: source implementation complete; offline/static audit complete; Unity compilation, compute import, live Shore-only production observation, and profiler evidence pending.

Actual changed files reconcile exactly with the approved list:

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/River_Foam_Stage6_Architecture.md
Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md
Assets/Docs/River_Rendering_Roadmap.md
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.State.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.SourceUnits.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.P12Sweep.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.ShoreRibbonDiagnostics.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute
```

Implemented differences from D8.14:

- removed Shore Coverage and Ribbon/Inward shore-start offset runtime and Inspector authority;
- replaced the global fixed-rate Shore accumulator with all-bucket per-slot renewal state, deterministic phase staggering, bounded scan/start work, and completion-relative Activity/Gap rearm;
- retained active bucket ownership while Shore starts are temporarily disabled, preventing re-enable overlap with a still-running event;
- made event-local Ribbon/Inward along-length resolution inclusive and whole-cell, bounded to the authored range and the largest length that fits the selected candidate;
- made the Ribbon candidate the centre of path cell zero;
- retained one Ribbon event identity and logical `1 x 1` head through the complete event;
- separated one-time birth Coverage from current-head source-debug Coverage;
- reused `_FoamCurrentShoreEdgesRead` as the only lateral path authority and added no path resource or solve;
- narrowed Ribbon dispatch to current/catch-up columns;
- derived event and reservation capacities from valid river length once during resource initialization;
- preserved Inward Wash, Object, and Free-Water shape evaluators byte-for-byte.

Offline evidence:

```text
D8.15 static/mathematical audit: 131/131 PASS
Changed-file reconciliation: exactly 17 approved files
Serialized scene/prefab/material changes: 0
New shoreline resources/kernels/passes/readbacks: 0
5 m / 32 m / 100 m Shore bucket counts: 4 / 20 / 58
100 m event/reservation capacities: 90 / 180
100 m GPU event buffer: 11,520 bytes
Head progression: exact N births at 8/12/16 Hz, 1/2 cells/s, N=8/20/47
Randomized geometry bounds: 20,000 cases per recipe/flow direction
Non-Shore shader evaluator comparison: byte-identical
```

Offline evidence file:

```text
/mnt/data/d815out/d815_static_audit.txt
```

Unity-only blockers remain explicit. No broad diagnostic suite is requested before the live production result is observed. The first Unity gate is compilation followed by direct Automatic Birth Sources observation of stable, concurrent, shore-following heads.

---

## RIVER-FOAM-SPAWN-D8.16 — Continuous Shore Emitters, Activity-Resolved Population, and Phase-Correct Source/Trail View

### Status

**Authorized and in implementation on 2026-07-30. Unity validation pending.**

This section supersedes the D8.15 Shore population, intermittent Ribbon birth, and Automatic Birth Sources presentation contracts. D8.15 remains historical evidence for whole-cell length selection, current-shore-edge path authority, and finite event identity only.

### Objective

Implement the accepted Shore spawning behavior without touching transport, lifecycle mathematics, material merge semantics, or Final Foam rendering:

1. `Activity` directly resolves a river-length-scaled target active-head population.
2. The Inspector displays the predicted active-head range from current Activity and represented river length, and Play Mode displays runtime population status.
3. Multiple finite Shore events may coexist, but the scheduler starts at most one replacement per material tick and never fills every 3.5 m candidate bucket merely because Activity is one.
4. One Shore Ribbon event owns one persistent 1×1 head from start through completion.
5. The current head attempts birth every material tick; `FoamMergeBornMaterial` remains the authority that fills only missing Coverage and does not refresh already full material.
6. A delayed material tick emits each skipped path cell separately plus the current head cell; no cumulative body or stretched rectangle is emitted.
7. The existing `_FoamCurrentShoreEdgesRead` texture remains the only shoreline path authority. No path texture, path buffer, CPU path cache, new kernel, readback, spline solve, or extra shoreline pass is added.
8. Automatic Birth Sources displays phase-correct active heads over phase-correct committed persistent Foam, so the view shows both the emitter and the deposited trail.

### Accepted Activity contract

Let:

```text
L = represented valid river length in metres
B = 2L = represented bank length across both banks
A = clamp01(Activity)
S = 17.5 m full-Activity head spacing across bank length
```

The mean target is:

```text
meanHeads = A * B / S
```

The predicted Inspector range is:

```text
minimumHeads = floor(meanHeads)
maximumHeads = ceil(meanHeads)
```

The runtime target uses deterministic fractional duty:

```text
targetHeads = minimumHeads +
    (stablePopulationPhase < frac(meanHeads) ? 1 : 0)
```

The fractional decision changes only at a stable population boundary derived from the current authored Shore event-duration range; it never changes every frame. If the target decreases, existing heads finish normally. The scheduler stops replacements until active heads are at or below the target. Minimum Packet Gap and packet reservations may temporarily prevent the target from being reached.

At `Activity = 1`:

```text
5 m river   -> mean 0.57 heads across both banks
32 m river  -> mean 3.66 heads across both banks
100 m river -> mean 11.43 heads across both banks
```

### Reviewed evidence

Repository instructions re-read before work:

```text
Assets/AGENTS.md
```

Canonical/current documents reviewed:

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/River_Foam_Stage6_Architecture.md
Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md
Assets/Docs/River_Rendering_Roadmap.md
```

Complete implementation surface and direct contracts reviewed:

```text
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.State.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.SourceUnits.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthDiagnostics.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Binding.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.PublicSurface.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Lifecycle.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Resources.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Simulation.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Coordinates.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl
```

Observed D8.15 evidence reviewed:

- The current river displays substantially more than the intended 3–5 heads.
- Heads intermittently attempt material birth instead of emitting continuously.
- Automatic Birth Sources shows head markers only and does not show committed deposited material.
- Head markers can visibly drift backward and jump forward under Bulk-Phase transport.

Source-localized causes:

1. `AdvanceAutomaticShoreBirthSources` applies Activity as an independent active/idle duty cycle to every 3.5 m bucket, making the active population approach the full bucket count at Activity one.
2. `DispatchAutomaticFoamSourceEvents` dispatches a Ribbon for material birth only when `currentHeadCell > previousHeadCell`; debug mode alone dispatches same-cell ticks.
3. `FoamEvaluatePersistentShoreRibbonHead` sets `birthShape` only for newly entered cells, excluding the current same-cell head on ordinary ticks.
4. `FoamAutomaticSourceGlobalDistanceAtColumn` applies Bulk Phase to source storage coordinates, but Automatic Birth Sources renders `_FoamBirthDebug` at raw unshifted `foam.fieldUV`.
5. `_FoamCurrentShoreEdgesRead` is built in unshifted world-column coordinates, while the D8.15 source evaluator indexes it with the phase-shifted storage coordinate.
6. The debug texture is intentionally cleared each material tick and currently stores only active source markers. The committed trail already exists in `_FoamCurrent` and can be rendered without another GPU resource.

### Approved files

Modify exactly:

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/River_Foam_Stage6_Architecture.md
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.DebugViews.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Members.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.BirthEvents.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Injection.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader
```

### Invariants and non-goals

- Do not modify transport kernels, transport controls, lifecycle mathematics, `FoamMergeBornMaterial`, Final Foam evaluation, scenes, prefabs, materials, river geometry, boundary/topology generation, or current-shore-edge generation.
- Do not add a new serialized population control. `Activity` remains normalized `0..1`.
- Do not add a path resource, compute pass, kernel, readback, CPU path list, or per-frame allocation.
- Do not change Object or Free-Water scheduling or geometry.
- Do not change Inward Wash geometry. Inward Wash shares only the new Shore-family population budget and existing packet-clearance scheduler.
- Preserve user-facing Ribbon Length Min/Max in cells and deterministic inclusive whole-cell effective-length selection.
- Preserve finite event completion and packet reservation authority.

### File-by-file implementation sequence

1. **This document** — record the exact D8.16 contract, review surface, approved scope, risks, and validation gates before code edits.
2. `StylizedRiver.cs` — define the invariant 17.5 m full-Activity head-spacing constant and update Activity tooltip semantics; no serialized-field addition.
3. `StylizedRiverFoamRuntime.Constants.cs` — reduce Shore starts to one per material tick.
4. `StylizedRiverFoamRuntime.Members.cs` — add bounded Shore population target/status state and separate active Shore count.
5. `StylizedRiverFoamRuntime.BirthEvents.cs` — replace per-bucket duty scheduling with global length-scaled target scheduling, retain buckets as candidate/clearance state, update stable fractional target at bounded population boundaries, and maintain active Shore count.
6. `StylizedRiverFoamRuntime.Injection.cs` — dispatch every active Ribbon each material tick and preserve finite completion; continue catch-up range support.
7. `CS_RiverFoam.compute` — include the current head in birth every tick, resolve current-shore lookup through the corresponding unshifted world column, and retain exact 1×1 row/column ownership.
8. `SH_CleanStylizedRiver.shader` — phase-correct `_FoamBirthDebug` sampling and composite active source colours over committed Coverage trail.
9. `StylizedRiverEditor.Foam.cs` — show predicted head range, represented bank length, and Play Mode runtime population status below Activity.
10. `StylizedRiverEditor.DebugViews.cs` — update Automatic Birth Sources description and legend/status wording.
11. `River_Foam_Stage6_Architecture.md` — freeze the accepted population, continuous-emitter, and source/trail debug contracts and mark conflicting historical statements superseded.

### Performance contract

Normal production:

- No new persistent GPU resource.
- No new compute kernel or pass.
- No new GPU readback.
- One bounded O(event-capacity) CPU count is avoided by maintaining `activeAutomaticShoreSourceEventCount` directly.
- Active Ribbon dispatch frequency increases to material cadence, but each dispatch remains restricted to the current head column plus delayed catch-up columns and the existing field-height row search. The requested behavior requires this source attempt; full cells return immediately through existing merge semantics.
- Population is reduced from near one event per 3.5 m bucket to the Activity-resolved target, substantially reducing the number of concurrent Ribbon events.

Debug view:

- No additional texture or pass. The fragment shader already has `_FoamCurrent`, `_FoamBirthDebug`, phase values, and packed-state decode helpers.
- One extra committed-state sample is performed only while Automatic Birth Sources is selected.

### Risks

1. **Population oscillation:** prevented by changing the fractional head decision only at a stable population boundary and never killing existing heads on target reduction.
2. **Target starvation:** packet reservations may block starts; runtime status must report waiting for clearance rather than pretending target achievement.
3. **Coordinate drift:** source storage coordinate, world shore coordinate, and render debug coordinate must use explicit separate mappings.
4. **Shared shader regression:** only Foam debug view `2` may change; all other Foam and final-render branches must remain byte-equivalent apart from line movement required by the scoped edit.
5. **Continuous source cost:** active population reduction and one-column dispatch bounds must be verified structurally; Unity profiling remains pending.

### Acceptance criteria

1. Activity one on the current approximately 32 m represented river predicts 3–4 active Shore heads, not approximately twenty.
2. Inspector shows predicted range and represented bank length in Edit Mode; Play Mode status shows active count, resolved current target, predicted range, and clearance waiting where applicable.
3. Each Ribbon head stays continuously visible, progresses monotonically in flow direction, follows the current visible shore laterally, and terminates after its effective Min/Max-selected whole-cell length.
4. Every active Ribbon attempts birth from its current 1×1 head every material tick; delayed ticks also cover skipped shoreline cells.
5. Automatic Birth Sources shows committed persistent Foam in neutral grey plus active source heads in category colours; same-update source overlap remains white.
6. Bulk Phase does not cause visible head backtracking or forward snapping in the debug view.
7. No cumulative Ribbon source body, wide Shore birth footprint, new shoreline path resource, or transport/lifecycle change exists.

### Validation and compliance status

- Gate 1 review: **complete**.
- Gate 2 canonical plan: **complete**.
- Implementation: **in progress**.
- Final scope/diff audit: pending.
- Static C#/HLSL checks: pending.
- Unity 6000.5.0f1 C# compilation and shader import: pending in user project.
- Direct Play Mode behavior and runtime profiling: pending in user project.

### D8.16 implementation and Gate 4 audit record

Source implementation status: **implemented within the approved scope; Unity compilation, shader import, GPU execution, live behavior, and profiling remain pending.** This is not a validated or release-ready patch until those Unity gates pass.

Implemented differences from D8.15:

1. `Activity` now resolves `meanHeads = Activity * (2 * validFieldLength) / 17.5 m`, with floor/ceiling prediction and deterministic fractional selection at event-completion or bounded duration-derived population boundaries.
2. `activeAutomaticShoreSourceEventCount` is maintained directly. A falling target never kills a live event; replacements stop until the population is at or below target.
3. The scheduler starts at most one Shore event per material tick. Existing 3.5 m buckets remain candidate and packet-clearance ownership only. On completion, a bucket rearms after packet clearance; no per-bucket Activity idle calculation remains.
4. Every active Shore Ribbon dispatches at material cadence. The shader emits the current 1x1 head on every tick and additionally emits each skipped intermediate cell after a delayed tick. The existing `FoamMergeBornMaterial` contract remains unchanged and rejects repeated attempts into already full cells.
5. Shore metric and current-shore-edge lookup resolve from the unshifted world column corresponding to the phase-shifted storage cell. No path resource, path solve, kernel, pass, or readback was added.
6. Automatic Birth Sources subtracts current Bulk Phase when sampling `_FoamBirthDebug` and composites category-coloured active emitters over phase-correct committed Coverage shown in dark grey.
7. The Shore Inspector displays predicted active-head range, mean, represented bank length, estimated 32 m Foam chunk count, and Play Mode runtime population status. The debug panel displays the same runtime population status.
8. `AutomaticShoreSourceMaximumStartsPerUpdate` changed from `3` to `1`. No serialized default changed.

Intentional unchanged behavior confirmed:

- Shore Ribbon event length remains an inclusive whole-cell effective selection between the existing user-facing Min/Max values.
- Finite event duration and completion remain authoritative.
- Inward Wash geometry/evaluator remains byte-equivalent; it shares only the Shore population budget and corrected world-column Shore metric lookup.
- Object and Free-Water evaluators and schedulers remain unchanged.
- `CS_RiverFoam.Simulation.hlsl`, lifecycle mathematics, SourceUnits dispatch-range logic, packet-independent merge semantics, final Foam rendering, scenes, prefabs, materials, current-shore-edge generation, topology, and boundary generation remain unchanged.

Offline Gate 4 evidence:

```text
Changed files: exactly the 11 approved paths
Serialized Unity assets changed: 0
Static audit: 61/61 PASS
Population calculations at Activity 1:
  5 m   -> mean 0.571429, predicted 0-1
  32 m  -> mean 3.657143, predicted 3-4
  100 m -> mean 11.428571, predicted 11-12
New compute kernels: 0
New GPU resources: 0
New readbacks: 0
New shoreline/path passes: 0
Maximum Shore starts per material tick: 1
```

Static audit covered changed-file scope, serialized-asset exclusion, C#/HLSL delimiter balance, removal of the old per-bucket Activity resolver, direct Shore active-count lifecycle, clearance-only bucket rearm, continuous Ribbon CPU dispatch, current-head/catch-up birth semantics, unshifted world Shore lookup, phase-correct source sampling, committed-trail composite, Inspector/debug population reporting, forbidden-file hash equality, and byte equality of all non-Ribbon source evaluator functions.

Unavailable validation and concrete next action:

- Unity 6000.5.0f1 C# compilation and compute/water-shader import: apply the changed files in the user project and provide the complete Console output if any error appears.
- Live production behavior: in Play Mode inspect the normal Shore authoring section and Automatic Birth Sources directly; do not run the broad diagnostic suites.
- Runtime performance: profile only after compilation and direct behavior pass; verify active-head count and material-cadence one-column Ribbon dispatch cost.

### D8.16 status update

- Gate 1 review: **complete**.
- Gate 2 canonical plan: **complete**.
- Gate 3 scoped implementation: **complete in source**.
- Gate 4 final scope, consistency, and static audit: **complete offline; 61/61 PASS**.
- Unity compilation, shader import, live visual validation, and profiling: **pending in user project**.

## RIVER-FOAM-VELOCITY-B1 — Independent Shore Component Suppression

### Status

**Implemented in source on 2026-07-31. Gate 4 offline audit passed; Unity compilation, shader import, Play Mode validation, and profiling remain pending.**

### Objective

Add two independent Layer B Canonical Velocity controls that reuse the existing current Shore Support field:

```text
Shore Lateral Movement Suppression:    0..1
Shore Downstream Movement Suppression: 0..1
```

At full Shore Support:

```text
0 = preserve the existing component;
1 = set that velocity component to exactly zero.
```

The existing Foam Motion Field and Foam Motion Field + Cell Grid debug views must resolve velocity through the same shared contract so changes are directly visible.

### Approved files

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/River_Foam_Stage6_Architecture.md
Assets/Game/Procedural/Rivers/StylizedRiver.cs
Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Constants.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Compute.cs
Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.Binding.cs
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Resources.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Motion.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoamVelocity.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader
```

### Reviewed evidence

1. `StylizedRiver.cs` owns current Layer B velocity authoring, including Downstream Speed Ratio, Maximum Lateral Speed Ratio, Object Contact slowdown, public accessors, migration, reset, and `OnValidate` clamping.
2. `StylizedRiverEditor.Foam.cs::DrawFoamLayerB()` is the exact Inspector hierarchy for Canonical Velocity controls.
3. `StylizedRiverFoamRuntime.Compute.cs::ConfigureSharedComputeParameters()` binds the canonical velocity inputs used by `SimulateFoam`.
4. `StylizedRiverFoamRuntime.Binding.cs` binds the same inputs to the water material for Motion Field debug rendering and defines hold/disabled fallbacks.
5. `CS_RiverFoam.Motion.hlsl::FoamResolveVelocity()` is the only compute caller of the shared velocity contract.
6. `SH_CleanStylizedRiver.shader` Foam debug modes 5 and 6 are the only render caller of the shared velocity contract.
7. `_FoamTopologySourcesRead.b` / `_FoamTopologySources.b` is the existing current Shore Support field. It is already generated, bound, aligned with Foam-grid coordinates, and available to compute and the water shader.
8. `CS_RiverFoam.Simulation.hlsl::FoamResolveGridVelocity()` passes the physical motion coordinate, including Bulk Phase, into `FoamResolveVelocity()`. The Shore Support sample must use that same coordinate.
9. `RiverWaterFoamVelocity.hlsl` owns the pure shared resolver. Any signature change requires both compute and shader callers to remain consistent.

### Mathematical contract

Let:

```text
S  = saturate(Shore Support)
CL = saturate(Shore Lateral Movement Suppression)
CD = saturate(Shore Downstream Movement Suppression)
```

Component-retention factors:

```text
lateralFactor    = 1 - S * CL
downstreamFactor = 1 - S * CD
```

After existing lane/obstacle routing and object-contact slowdown:

```text
vDownstream' = vDownstream * downstreamFactor
vLateral'    = vLateral * lateralFactor
```

At full Shore Support:

```text
CL=1 -> vLateral'=0
CD=1 -> vDownstream'=0
```

The existing Object Contact slowdown remains independent and multiplicative. The Motion Field debug brightness must use the total resolved downstream factor, and its red/blue lateral hue must use the total resolved lateral velocity.

### Invariants

- Defaults are `0`, preserving current velocity until explicitly changed.
- No new texture, buffer, kernel, pass, dispatch, readback, shoreline calculation, topology field, or per-frame CPU work.
- Reuse existing Shore Support exactly; do not alter its generation, width, fade, obstacle exclusion, or lifecycle meaning.
- The controls are spatial Layer B controls. They affect any persistent Foam currently inside Shore Support, regardless of source provenance.
- Preserve upstream prohibition: downstream suppression may reduce world downstream speed to zero but may not produce upstream world motion.
- Preserve all spawning, transport scheme mathematics, lifecycle, merge, final-render, scene, prefab, material, Object slowdown, Object/Free-Water source, and Shore source behavior.
- Foam Motion Field debug modes 5 and 6 must visibly reflect the same velocity used by transport.

### Non-goals

- Do not diagnose or fix Foam birth width in this patch.
- Do not add source provenance to persistent Foam state.
- Do not change Shore Support reach or add a separate Shore velocity mask.
- Do not add a new debug view or legend colour.
- Do not change Object Contact slowdown semantics.

### File-by-file implementation sequence

1. **This document** — record the accepted plan before code edits.
2. `StylizedRiver.cs` — add bounded serialized controls, defaults, tooltips, public accessors, reset, and validation clamps.
3. `StylizedRiverEditor.Foam.cs` — expose both controls inside Layer B — Canonical Velocity immediately before Object Contact slowdown.
4. `StylizedRiverFoamRuntime.Constants.cs` — add material property IDs.
5. `StylizedRiverFoamRuntime.Compute.cs` — bind both values to the shared simulation compute configuration.
6. `StylizedRiverFoamRuntime.Binding.cs` — bind both values to live material debug rendering and safe zero fallbacks.
7. `CS_RiverFoam.Resources.hlsl` — declare compute uniforms.
8. `RiverWaterFoamVelocity.hlsl` — extend the pure contract with Shore Support and independent component suppression.
9. `CS_RiverFoam.Motion.hlsl` — load existing Shore Support at the same physical motion coordinate and pass it to the shared contract.
10. `SH_CleanStylizedRiver.shader` — declare properties/uniforms, sample existing Shore Support in debug modes 5/6, and pass it to the same resolver.
11. `River_Foam_Stage6_Architecture.md` — freeze the accepted Layer B contract and debug semantics.

### Risks and mitigation

1. **Bulk residual transport:** world downstream suppression to zero must still pass through the existing bulk-residual subtraction. The pure resolver returns zero absolute downstream speed; the residual transport path then cancels bulk movement as designed.
2. **Coordinate mismatch:** compute Shore Support must use `motionSampleCoordinate`, not raw storage coordinates. Shader debug must sample Shore Support with the same field-space UV used for route and obstacle fields.
3. **Shared include regression:** all callers of `RiverWaterResolveFoamVelocityContract()` must be updated together and audited.
4. **Hold/disabled stale uniforms:** Binding fallbacks must explicitly set both suppression values to zero.
5. **Debug mismatch:** Motion Field hue and brightness must derive from final suppressed velocity/factors, not pre-suppression route intent.

### Validation and compliance plan

Offline:

- exact changed-file scope audit;
- C# and HLSL delimiter/symbol checks;
- verify both shared resolver callers use the new signature;
- verify no new resource/kernel/pass/readback;
- verify forbidden spawning, simulation, lifecycle, merge, and final-render files remain unchanged;
- mathematical endpoint checks for Shore Support/control combinations 0 and 1.

Unity 6000.5.0f1:

1. Compile C# and import compute/water shaders.
2. In `River_Strip -> Stylized River -> Foam -> Layer B — Canonical Velocity`, set both controls independently and inspect Foam Motion Field.
3. At both controls `1`, full Shore Support cells must display zero lateral hue and near-black downstream brightness; interior water remains unchanged.
4. At lateral `1`, downstream `0`, shore cells retain brightness but lose red/blue lateral colour.
5. At lateral `0`, downstream `1`, shore cells retain lateral hue but become near-black.

### Status checklist

- Gate 1 review: **complete**.
- Gate 2 canonical plan: **complete**.
- Gate 3 implementation: **complete in source**.
- Gate 4 final scope, consistency, and static audit: **complete offline; 49/49 PASS**.
- Unity compilation, shader import, Play Mode visual validation, and profiling: **pending in user project**.


### RIVER-FOAM-VELOCITY-B1 implementation and Gate 4 audit record

Implemented differences from D8.16:

1. Added two zero-default serialized Layer B controls and public accessors: Shore Lateral Movement Suppression and Shore Downstream Movement Suppression.
2. Bound both controls to `SimulateFoam` and to the water material used by Motion Field debug modes. Hold and disabled bindings explicitly reset both values to zero.
3. Extended the shared pure velocity resolver with the existing Shore Support value and independent downstream/lateral component-retention factors.
4. Compute transport samples `_FoamTopologySourcesRead.b` at the same physical motion coordinate used by lane and obstacle routing.
5. Motion Field debug modes 5 and 6 sample `_FoamTopologySources.b` and call the same shared resolver, so hue and brightness visualize the final suppressed velocity.
6. No source, transport scheme, lifecycle, merge, final-render, scene, prefab, material, topology-generation, or Object slowdown behavior changed.

Offline Gate 4 evidence:

```text
Changed files: exactly the 11 approved paths
Serialized Unity assets changed: 0
Static audit: 49/49 PASS
New compute kernels: 0
New RenderTexture allocations: 0
New GraphicsBuffer allocations: 0
New AsyncGPUReadback references: 0
Forbidden spawning/simulation/lifecycle files: byte-identical
Shared velocity-contract occurrences: one definition + two updated callers
Endpoint mathematics:
  Shore=1, lateral=1, downstream=0 -> lateral factor 0, downstream factor 1
  Shore=1, lateral=0, downstream=1 -> lateral factor 1, downstream factor 0
  Shore=1, lateral=1, downstream=1 -> both factors 0
  Shore=0 -> both factors 1 regardless of controls
```

Unavailable validation and concrete next action:

- Unity 6000.5.0f1 C# compilation and compute/water-shader import: apply the changed files and provide complete Console output if any error appears.
- Play Mode velocity validation: use Foam Motion Field and Foam Motion Field + Cell Grid with the exact control combinations in the validation section.
- Runtime profiling: after visual acceptance, compare `SimulateFoam` GPU time at both controls zero versus one. The implementation adds one existing-texture Shore Support load per canonical velocity resolution and no additional pass or allocation.

## RIVER-FOAM-VELOCITY-B1A — Conservative Shore-Contact Velocity Mask

Status: implemented in source; Gate 1–4 offline review complete; Unity compilation/shader import and Play Mode visual validation pending.

### Objective

Close the visible one-cell gaps between the current-shore boundary and the Layer B Shore velocity-suppression zone without changing canonical Shore Support, lifecycle, spawning, transport mathematics, authored suppression controls, or adding any resource/pass.

### Approved files

```text
Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md
Assets/Docs/River_Foam_Stage6_Architecture.md
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.compute
Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Motion.hlsl
Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/SH_CleanStylizedRiver.shader
```

No other file may change.

### Reviewed evidence

1. `CS_RiverFoam.compute::ComposeTopology()` computes canonical Shore Support from the Foam cell centre and writes it to `_FoamTopologySources.b`. A boundary cell whose footprint intersects water can therefore have `nearestCurrentShore < 0` at its centre and receive zero canonical Shore Support.
2. `CS_RiverFoam.Motion.hlsl::FoamLoadShoreVelocityInfluence()` currently consumes `_FoamTopologySourcesRead.b` for the B1 component-suppression controls.
3. `SH_CleanStylizedRiver.shader` Motion Field modes likewise consume `_FoamTopologySources.b`, so the debug view and transport share the same current gap.
4. `_FoamTopologySources.a` is written as zero by `ComposeTopology()` and has no current material-topology consumer. `FoamResolveMaterialTopology()` consumes only RGB, preserving alpha for an independent velocity-only contact mask.
5. `FoamMetricRow.widthsAndSpacing.w` is the local minimum lateral Foam-cell spacing and is available in `ComposeTopology()` without another lookup or resource.

### Accepted design

Preserve canonical Shore Support in channel B exactly. Reuse channel A for a velocity-only footprint-conservative Shore contact mask:

```text
Topology Sources R = Pressure Support
Topology Sources G = Lee Support
Topology Sources B = canonical Shore Support (unchanged)
Topology Sources A = Shore Velocity Contact Support (new)
```

For signed cell-centre distance `d = nearestCurrentShore` and local half lateral cell width `h = 0.5 * metric.widthsAndSpacing.w`:

```text
cellTouchesCurrentWater = d + h >= 0
shoreVelocityDistance = max(0, d - h)
```

Then:

```text
shoreVelocitySupport = cellTouchesCurrentWater
    ? 1 - smoothstep(coreWidth, coreWidth + fadeWidth, shoreVelocityDistance)
    : 0
```

The final alpha channel is:

```text
shoreVelocitySupport * validDomainMask * (1 - obstacleFootprint)
```

Transport and Motion Field debug both consume alpha instead of blue. Canonical Shore Support and all lifecycle/topology consumers continue using blue unchanged.

### Acceptance criteria

- With both Shore suppression controls at `1`, the zero-velocity band reaches every valid Foam cell whose footprint touches the current visible shore; no one-cell white velocity gaps remain between the bank and suppressed region.
- Lateral-only and downstream-only suppression retain their B1 component semantics.
- Motion Field and Motion Field + Cell Grid visibly match transport because both consume the same alpha mask.
- Canonical Shore Support `.b`, lifecycle/aging, spawning, transport scheme mathematics, Object slowdown, scenes, prefabs, materials, and authored widths remain unchanged.
- New textures/buffers/kernels/passes/dispatches/readbacks: zero.

### File-by-file implementation sequence

1. `CS_RiverFoam.compute`: derive the footprint-conservative velocity mask during existing `ComposeTopology()` and write it to `_FoamTopologySources.a` while leaving RGB unchanged.
2. `CS_RiverFoam.Motion.hlsl`: consume `.a` in `FoamLoadShoreVelocityInfluence()`.
3. `SH_CleanStylizedRiver.shader`: consume `.a` in Motion Field debug modes.
4. `River_Foam_Stage6_Architecture.md`: document the independent channel-A velocity-contact contract and its non-impact on lifecycle.
5. Re-read the full review surface, compare against the pre-B1A tree, run static scope/contract checks, and record Gate 4 evidence here.

### Risks and constraints

- Alpha must remain velocity-only; no lifecycle/material topology resolver may start consuming it.
- The conservative edge test must not expand into dry cells farther than half a local lateral Foam cell beyond the current visible shore.
- Obstacle footprint exclusion remains authoritative.
- No new hot-path texture sample is permitted; B1 already loads the complete topology-source texel and will switch channel selection only.

### Validation

Pending Unity 6000.5.0f1 compilation/shader import and Play Mode Motion Field inspection in the user project.

### Implementation and Gate 4 audit record

Implemented differences from B1:

1. `ComposeTopology()` preserves canonical Shore Support in `_FoamTopologySources.b` and writes footprint-conservative Shore Velocity Contact Support to the previously reserved alpha channel.
2. `FoamLoadShoreVelocityInfluence()` consumes alpha; the shared B1 component-suppression mathematics are unchanged.
3. Motion Field debug modes consume the same alpha mask, preserving direct transport/debug parity.
4. Lifecycle/material-topology resolution continues to consume canonical blue Shore Support only.
5. No new texture, buffer, kernel, pass, dispatch, readback, shoreline solve, control, serialized asset, or hot-path topology-source sample was added.

Offline Gate 4 evidence:

```text
Changed files: exactly the 5 approved paths
Serialized Unity assets changed: 0
Static audit: 20/20 PASS
Compute kernel count: unchanged (22)
Canonical shoreSupport formula: preserved
Material topology shoreSupport source: anchoredSources.b preserved
Material topology alpha consumers: 0
Compute velocity Shore influence: _FoamTopologySourcesRead.a
Motion Field Shore influence: _FoamTopologySources.a
New resource declarations in modified shader files: 0
HLSL/Shader delimiter checks: PASS
```

Unavailable validation and concrete next action:

- Unity 6000.5.0f1 compute/water-shader import: apply the changed files and provide the complete Console output if compilation/import fails.
- Play Mode visual validation: with both Shore suppression controls at `1`, inspect Foam Motion Field and confirm the stationary band touches the visible shore without the prior one-cell white gaps.

## RIVER-FOAM-MATERIAL-C0 — Material Contract Simplification Direction

Status: documentation-only architecture freeze; no production code, shader, serialized asset, scene, prefab, material, resource, kernel, dispatch, or runtime behavior changes in C0.

### Objective

Freeze the accepted direction for simplifying the Layer C transport/visibility contract before any destructive cleanup or Life-Only implementation.

The current implementation exposes three independent experimental selectors:

- `Material Transport Scheme`: Donor Cell / TVD Superbee / Bulk-Phase Residual TVD;
- `Final Foam Visibility Mode`: Concentration + Lifetime / Lifecycle-Faithful;
- `Presence Footprint`: Coverage-Only / Presence-Amplitude.

The accepted retained baseline is exactly:

```text
Bulk-Phase Residual TVD
+
Lifecycle-Faithful
+
Coverage-Only
=
C × P × L Baseline
```

C0 records that the three legacy selectors are to be consolidated into one future user-facing `Material Contract` selector. The initial consolidated option will be `C × P × L Baseline`; a later independent patch will add `Life Only`.

### Approved files

Documentation only:

- `Assets/Docs/River_Foam_Active_Blockers_and_Next_Patches.md`
- `Assets/Docs/River_Foam_Stage6_Architecture.md`
- `Assets/Docs/River_Foam_Fixed_Metric_Dependency_Register.md`
- `Assets/Docs/River_Rendering_Roadmap.md`

No other file is authorized for C0.

### Gate 1 reviewed evidence

Current source state reviewed before this plan entry:

- `Assets/Game/Procedural/Rivers/StylizedRiver.cs`
  - `StylizedRiverFoamTransportScheme` exposes `DonorCell`, `TvdSuperbee`, and `BulkPhaseResidualTvd`.
  - `StylizedRiverFinalFoamVisibilityMode` exposes `ConcentrationAndLifetime` and `LifecycleFaithful`.
  - `StylizedRiverFoamPresenceFootprintMode` exposes `Current` (`Coverage-Only`) and `PresenceAmplitude`.
  - Serialized defaults currently retain independent fields for all three selectors.
  - `foamChipSoftEdgeStart` is explicitly Presence-Amplitude-only authoring.
- `Assets/Game/Procedural/Rivers/Editor/StylizedRiverEditor.Foam.cs`
  - `DrawFoamTransportVisibilityContract()` exposes the three selectors independently and summarizes their combined result.
  - Presence-Amplitude-only and Coverage-Only-only Chipping authoring branches remain present.
- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/CS_RiverFoam.Simulation.hlsl`
  - `FoamTransportUsesTvdSuperbee()` returns true for both standalone TVD Superbee and Bulk-Phase Residual TVD.
  - Therefore the Superbee reconstruction and its donor/upwind mechanics are shared dependencies of the retained Bulk-Phase baseline and are not safe to delete merely because the standalone TVD selector is retired.
- `Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/Includes/RiverWaterFoam.hlsl`
  - Final visibility still branches between Lifecycle-Faithful and Concentration + Lifetime.
  - Chipping/shape code still contains protected Coverage-Only compatibility arithmetic and dedicated Presence-Amplitude behavior.
- `Assets/Game/Procedural/Rivers/StylizedRiverFoamRuntime.VisibilityDiagnostics.cs`
  - diagnostics still report all three selectors independently;
  - its Lifecycle-Faithful text contains a known stale description (`smoothstep(0.02, 0.10, C)`) that does not match the current production `RiverWaterFoamResolveMeaningfulCoverageFootprint()` implementation, which returns saturated Coverage directly. C1 must reconcile this report to the retained production path rather than preserving stale wording.

Historical documents contain many superseded transport/visibility A/B experiments. They remain historical evidence; C0 does not rewrite or erase those records.

### Accepted architecture direction

#### C × P × L Baseline

The retained baseline must preserve the current accepted combination exactly:

1. **Transport:** Bulk-Phase Residual TVD.
   - Keep the global Bulk Phase, integer shift, residual subtraction, and current one-dispatch/no-extra-field contract.
   - Keep the Superbee reconstruction and donor/upwind mechanics required by Bulk-Phase Residual TVD.
   - Remove only obsolete selectable transport alternatives and code that exists exclusively to select or execute those alternatives.
2. **Final visibility:** Lifecycle-Faithful.
   - Make the existing Lifecycle-Faithful production route unconditional in C1.
   - Remove Concentration + Lifetime selection and code that exists exclusively for that obsolete mode.
3. **Presence footprint:** Coverage-Only.
   - Make the existing Coverage-Only production route unconditional in C1.
   - Remove Presence-Amplitude selection and code/authoring that exists exclusively for that obsolete mode.

C1 is a consolidation/removal patch only. It must not change persistent material packing or the semantics of Coverage, Presence, Remaining Life, or Material Pattern.

#### Life Only

`Life Only` is a later material-contract option and is explicitly outside C0 and C1 implementation scope.

Accepted conceptual direction for that future patch:

- persistent Foam material is binary at Foam-cell resolution;
- Remaining Life is the sole persistent material-existence/lifecycle authority;
- no weak-versus-strong Foam distinction is required;
- no fractional cell-occupancy material meaning is required by the desired visual contract;
- shader-side Chipping, Strands, erosion, fragmentation, and breakup remain rendering concerns rather than persistent density/occupancy state;
- transport cannot simply reuse fractional C × P × L flux semantics unchanged, so Life-Only transport/collision/movement rules require a focused implementation audit before C2 code is authorized.

### Invariants

- C0 changes documentation only.
- The current runtime remains Bulk-Phase/visibility/presence selectable until C1 is implemented.
- `C × P × L Baseline` must mean the exact current accepted combination, not a reinterpretation or retune.
- Shared algorithms required by the retained baseline remain even if their historical selector is removed.
- Historical A/B documentation remains historical; current architecture sections must identify the future authoritative replacement without falsifying prior results.
- C1 and C2 remain separate regression boundaries.

### Non-goals

C0 does not:

- remove enums, serialized fields, Inspector controls, shader properties, bindings, diagnostics, or branches;
- modify Foam packing;
- modify transport mathematics;
- modify lifecycle;
- modify Final Foam rendering;
- implement Life Only;
- change serialized defaults;
- edit scenes, prefabs, materials, caches, or generated assets.

### Planned follow-up patches

#### RIVER-FOAM-MATERIAL-C1 — Baseline Contract Consolidation

Planned objective: replace the three independent selectors with one `Material Contract` selector containing only `C × P × L Baseline`, while deleting only code proven exclusive to obsolete modes.

Required preservation rule:

> If an algorithm is still called by Bulk-Phase Residual TVD, Lifecycle-Faithful, or Coverage-Only, it is retained even if its name originated in a retired experimental mode.

Expected removals include, subject to C1 Gate 1 proof:

- Donor Cell as a selectable production mode;
- standalone TVD Superbee as a selectable production mode;
- transport-mode branching that is not required by the retained Bulk-Phase path;
- Concentration + Lifetime selector and exclusive render branch;
- Presence-Amplitude selector and exclusive render/chipping/authoring plumbing;
- obsolete three-selector Inspector summary/status/diagnostic plumbing.

C1 acceptance criterion: with `Material Contract = C × P × L Baseline`, simulation and rendering are behaviorally identical to the pre-C1 combination `Bulk-Phase Residual TVD + Lifecycle-Faithful + Coverage-Only`.

#### RIVER-FOAM-MATERIAL-C2 — Life-Only Binary Cellular Contract

Planned objective: add `Life Only` to the consolidated selector and implement its independent persistent-state/transport/render contract only after a focused transport audit.

C2 must not be merged into C1.

### File-by-file C0 sequence

1. **This document** — record objective, evidence, invariants, non-goals, and C1/C2 boundaries before modifying any other canonical document.
2. `River_Foam_Stage6_Architecture.md` — add the authoritative current/future material-contract direction and supersession note for the three-selector experiment surface.
3. `River_Foam_Fixed_Metric_Dependency_Register.md` — record which legacy dependencies are retained versus candidates for deletion, especially the retained Superbee/upwind dependency of Bulk-Phase Residual TVD.
4. `River_Rendering_Roadmap.md` — update the current roadmap so C0 -> C1 -> C2 is the active sequence while preserving historical P12/P13 entries as historical evidence.

### Risks

1. **Over-deletion in C1:** names such as `Superbee` or donor/upwind may look obsolete while still being required by Bulk-Phase Residual TVD. Mitigation: dependency proof before deletion; retain any shared path.
2. **Behavioral drift hidden by consolidation:** making one route unconditional can accidentally alter arithmetic order or defaults. Mitigation: C1 requires exact before/after baseline comparison.
3. **Historical-document confusion:** old experiments must remain readable without appearing current. Mitigation: add explicit supersession/current-direction statements rather than rewriting historical evidence.
4. **Life-Only scope bleed:** cleanup could accidentally begin changing packed material semantics. Mitigation: C1 explicitly forbids material-state redesign; C2 owns it.

### C0 acceptance criteria

- Exactly the four approved Markdown files change.
- No production/source/shader/serialized file changes.
- All four documents identify `Bulk-Phase Residual TVD + Lifecycle-Faithful + Coverage-Only` as the retained `C × P × L Baseline` direction.
- All four documents identify one future `Material Contract` selector as the replacement for the three current selectors.
- C1 is explicitly cleanup/consolidation only.
- C2 is explicitly the separate Life-Only behavior patch.
- Documents explicitly prohibit deleting Superbee/upwind mechanics still required by Bulk-Phase Residual TVD.

### C0 validation and status

- Gate 1 review: complete.
- Gate 2 plan: complete with this entry.
- Gate 3 documentation updates: pending.
- Gate 4 scope/consistency audit: pending.
- Unity validation: not required because C0 changes Markdown only.

### C0 implementation and Gate 4 audit record

C0 documentation updates completed exactly within the approved four-file Markdown scope.

Intentional differences from the pre-C0 documentation state:

1. `River_Foam_Active_Blockers_and_Next_Patches.md` now owns the active C0 plan, reviewed dependency evidence, accepted retained baseline, C1 safe-removal rules, and separate C2 Life-Only boundary.
2. `River_Foam_Stage6_Architecture.md` now identifies `C × P × L Baseline` as the future retained single-contract baseline while explicitly preserving the current three-selector runtime until C1.
3. `River_Foam_Fixed_Metric_Dependency_Register.md` now records safe-removal dependency rules, including the requirement to retain Superbee and donor/upwind mechanics still used by Bulk-Phase Residual TVD.
4. `River_Rendering_Roadmap.md` now makes C0 -> C1 -> C2 the active material-contract migration sequence without rewriting historical P12/P13 experiment records.

Offline Gate 4 evidence:

```text
Changed files: exactly the 4 approved Markdown paths
Production/source/shader/serialized files changed: 0
Canonical C0 consistency audit: 27/27 PASS
Balanced Markdown code fences: PASS in all 4 files
Retained baseline named in all 4 files: PASS
Single Material Contract direction named in all 4 files: PASS
C1/C2 separation named in all 4 files: PASS
Shared Superbee/donor-upwind preservation rule represented in all 4 files: PASS
```

Final C0 status:

- Gate 1 review: **complete**.
- Gate 2 canonical plan: **complete**.
- Gate 3 documentation implementation: **complete**.
- Gate 4 scope/consistency audit: **complete; 27/27 PASS**.
- Unity validation: **not required; documentation-only patch**.

No C1 production cleanup or C2 Life-Only implementation is included in C0.
