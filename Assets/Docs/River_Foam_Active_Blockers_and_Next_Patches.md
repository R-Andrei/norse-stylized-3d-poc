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

`RG-METRIC-P12l — Binary Candidate × Eligibility intersection` is mechanically valid but rejected by Unity visual evidence. Its `>= 0.5` tests select only the midpoint contour interiors of two antialiased fields, so positive Candidate or Eligibility support below `0.5` remains unselected and can preserve Foam. `RG-METRIC-P12m — Any-Support Binary Chip Selection and Full-Removal Proof` corrected that threshold defect. `RG-METRIC-P12n — Optional Candidate-Straddle Chip Admission A/B` and `RG-METRIC-P12o — Original Analytical Candidates with Boundary-Anchored Eligibility` are visually rejected and removed. `RG-METRIC-P12p — Retire Experimental Cache and Isolate the Rendered Exterior Fringe` is the active implementation: one original analytical Candidate Field multiplied by one rendered Eligibility band whose distance coordinate excludes the inner hard-body rise.

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
