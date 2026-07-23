# River Foam Active Blockers and Next Patches

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

River Foam work is paused while a separate thread performs small shared River shader updates for Weather cloud shading. Those external edits are expected not to alter Foam behavior, but that expectation is unverified until the receiving River thread compares the post-Weather source against this frozen P13F/P13G baseline. Do not resume implementation from this plan until the external shader work is complete and current source is re-inventoried.

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

## Active blocker

Unity evidence rejects both P12n Candidate Straddle and P12o Boundary-Anchored Strip. The low-frequency cache produces permission geometry unrelated to the required continuous rendered edge band and must be removed completely. The sole retained route is the original full-rate analytical Candidate Field multiplied by one rendered Eligibility band.

The remaining blocker is narrower: the visible pale exterior Foam fringe survives some Chips because Presence-Amplitude Eligibility currently derives distance from the complete hardened `preChipRenderedMask`. `RiverWaterFoamHardenSoftVisibility` constructs that mask from two rises: `hardVisible = smoothstep(0.22, 0.58, soft)` and `fringe = smoothstep(0.06, 0.34, soft) * 0.34`. Derivatives of the complete mask can therefore respond to the inner hard-body rise instead of exclusively tracking the actual exterior fringe. P12p isolates the exterior rendered-fringe coordinate and uses it as the only Presence-Amplitude edge-distance source.

No new diagnostic view, texture, buffer, kernel, dispatch, serialized control, render pass, or candidate system is authorized. GPU timing is unmeasured.

## Next patch after P12p evidence

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

